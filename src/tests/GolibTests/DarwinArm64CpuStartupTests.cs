using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cpu = go.@internal.cpu_package;
using Arm = System.Runtime.Intrinsics.Arm;

namespace GolibTests;

/// <summary>
/// The darwin/arm64 flavour's internal/cpu start-up, internal/cpu/darwin-arm64/cpu_arm64_darwin_impl.cs: the twin of
/// LinuxArm64CpuStartupTests. Its module initializer runs the converted Initialize: doinit's option table, then Go's
/// darwin osInit (sysctl reads for ATOMICS, CRC32, SHA512 and SHA3; AES, PMULL, SHA1 and SHA2 assumed, as Go assumes
/// them on Apple silicon), then GODEBUG. The tests check:
/// - that start-up ran at all: osInit's four assumed flags are set (without the initializer every flag read false,
///   P6's D5 reading on osx-arm64, 2026-11-25);
/// - that the CLR's own detection agrees, for the extensions .NET exposes;
/// - that the option table is cpu_arm64.go's, and that cpu.aes=off clears HasAES and nothing else through it.
/// HasDIT is held false whatever the silicon reports (the darwin ruling, 2026-11-06; the impl's header).
/// darwin/arm64 flavour only: GolibTests.csproj excludes this file unless $(GoTargetOS) is darwin and
/// $(GoTargetArch) is arm64. The amd64 flavour's counterpart is InternalCpuGodebugTests. The method names carry the
/// flavour because the expected-skip manifests match test names alone.
/// </summary>
[TestClass]
public class DarwinArm64CpuStartupTests
{
    // A GODEBUG naming cpu.* options would legitimately move the flags off osInit's answer.
    private static void RequireNoCpuGodebug()
    {
        string godebug = Environment.GetEnvironmentVariable("GODEBUG") ?? "";
        if (godebug.Split(',').Any(kv => kv.Trim().StartsWith("cpu.", StringComparison.Ordinal)))
            Assert.Inconclusive($"GODEBUG sets cpu.* options ({godebug}); the flags are not osInit's answer here");
    }

    [TestMethod]
    public void DarwinArm64StartUpRanOsInit()
    {
        RequireNoCpuGodebug();

        var a = cpu.ARM64;

        Assert.IsTrue(a.HasAES, "darwin's osInit assumes AES on Apple silicon; false means start-up never ran it");
        Assert.IsTrue(a.HasPMULL, "darwin's osInit assumes PMULL");
        Assert.IsTrue(a.HasSHA1, "darwin's osInit assumes SHA1");
        Assert.IsTrue(a.HasSHA2, "darwin's osInit assumes SHA2");
        Assert.IsFalse(a.HasDIT, "HasDIT is held false: the CLR exposes no PSTATE.DIT control");
        Assert.IsFalse(a.HasCPUID, "darwin's osInit never sets HasCPUID");
    }

    [TestMethod]
    public void DarwinArm64ClrSeesTheSameExtensions()
    {
        RequireNoCpuGodebug();

        var a = cpu.ARM64;

        Assert.AreEqual(Arm.Aes.IsSupported, a.HasAES, "AES: .NET's Arm.Aes against internal/cpu");
        Assert.AreEqual(Arm.Crc32.IsSupported, a.HasCRC32, "CRC32: .NET's Arm.Crc32 against internal/cpu (sysctl hw.optional.armv8_crc32)");
        Assert.AreEqual(Arm.Sha1.IsSupported, a.HasSHA1, "SHA1: .NET's Arm.Sha1 against internal/cpu");
        Assert.AreEqual(Arm.Sha256.IsSupported, a.HasSHA2, "SHA2: .NET's Arm.Sha256 against internal/cpu");
    }

    [TestMethod]
    public void DarwinArm64OptionTableIsCpuArm64Gos()
    {
        string[] names = cpu.GoDarwinArm64OptionsProbe("").Select(o => o.name).ToArray();

        CollectionAssert.AreEqual(
            new[] { "aes", "pmull", "sha1", "sha2", "sha512", "sha3", "crc32", "atomics", "cpuid", "isNeoverse" },
            names, "cpu_arm64.go doinit's option table, in its order");
    }

    [TestMethod]
    public void DarwinArm64EmptyOrNonCpuGodebugLeavesEveryFlagAlone()
    {
        RequireNoCpuGodebug();

        (string name, bool enabled)[] empty = cpu.GoDarwinArm64OptionsProbe("");

        Assert.AreEqual(cpu.ARM64.HasAES, empty.Single(o => o.name == "aes").enabled, "the probe starts from start-up's state");
        CollectionAssert.AreEqual(empty, cpu.GoDarwinArm64OptionsProbe("gctrace=0,madvdontneed=1"), "non-cpu keys change nothing");
    }

    [TestMethod]
    public void DarwinArm64CpuAesOffClearsHasAesAndNothingElse()
    {
        RequireNoCpuGodebug();

        (string name, bool enabled)[] before = cpu.GoDarwinArm64OptionsProbe("");
        (string name, bool enabled)[] after = cpu.GoDarwinArm64OptionsProbe("cpu.aes=off");

        CollectionAssert.AreEqual(before.Select(o => o.name).ToArray(), after.Select(o => o.name).ToArray(), "the same table");

        for (int i = 0; i < before.Length; i++)
        {
            if (before[i].name == "aes")
                Assert.IsFalse(after[i].enabled, "cpu.aes=off clears HasAES");
            else
                Assert.AreEqual(before[i].enabled, after[i].enabled, $"cpu.aes=off leaves {before[i].name} alone");
        }

        Assert.AreEqual(before.Single(o => o.name == "aes").enabled, cpu.ARM64.HasAES, "the probe restored the flags");
    }
}
