using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cpu = go.@internal.cpu_package;
using Arm = System.Runtime.Intrinsics.Arm;

namespace GolibTests;

/// <summary>
/// S3 site 0's reading (COORD ruling, 2026-11-10): the linux/arm64 flavour's internal/cpu start-up,
/// internal/cpu/linux-arm64/cpu_arm64_hwcap_impl.cs. Its module initializer stores getauxval(AT_HWCAP) in HWCap and
/// runs the converted Initialize: doinit, then hwcapInit's mapping, then GODEBUG. The tests check:
/// - that the kernel's word arrived;
/// - that each flag is Go's own mapping of its bit (cpu_arm64_hwcap.go's constants, restated here as literals);
/// - that the CLR's own detection agrees, for the extensions .NET exposes;
/// - that cpu.aes=off clears HasAES and nothing else, through cpu_arm64.go's option table.
/// HasDIT is held false whatever the kernel reports (the darwin ruling, 2026-11-06; the impl's header).
/// linux/arm64 flavour only: GolibTests.csproj excludes this file unless $(GoTargetOS) is linux and
/// $(GoTargetArch) is arm64. The amd64 flavour's counterpart is InternalCpuGodebugTests.
/// </summary>
[TestClass]
public class LinuxArm64CpuStartupTests
{
    // The kernel's AT_HWCAP bits as Go names them (internal/cpu/cpu_arm64_hwcap.go, go1.26.10).
    private const ulong FP = 1UL << 0, ASIMD = 1UL << 1, AES = 1UL << 3, PMULL = 1UL << 4, SHA1 = 1UL << 5,
        SHA2 = 1UL << 6, CRC32 = 1UL << 7, ATOMICS = 1UL << 8, CPUID = 1UL << 11, SHA3 = 1UL << 17,
        SHA512 = 1UL << 21;

    // A GODEBUG naming cpu.* options would legitimately move the flags off the kernel's word.
    private static void RequireNoCpuGodebug()
    {
        string godebug = Environment.GetEnvironmentVariable("GODEBUG") ?? "";
        if (godebug.Split(',').Any(kv => kv.Trim().StartsWith("cpu.", StringComparison.Ordinal)))
            Assert.Inconclusive($"GODEBUG sets cpu.* options ({godebug}); the flags are not the kernel's word here");
    }

    [TestMethod]
    public void TheKernelsHwcapReachedInternalCpu()
    {
        ulong hw = cpu.HWCap;

        Assert.AreNotEqual(0UL, hw, "getauxval(AT_HWCAP) answered nothing");
        Assert.AreEqual(FP | ASIMD, hw & (FP | ASIMD), "every arm64 Linux kernel reports FP and ASIMD");
    }

    [TestMethod]
    public void EachFlagIsGosMappingOfTheKernelsBit()
    {
        RequireNoCpuGodebug();

        ulong hw = cpu.HWCap;
        var a = cpu.ARM64;

        Assert.AreEqual((hw & AES) != 0, a.HasAES, "HasAES");
        Assert.AreEqual((hw & PMULL) != 0, a.HasPMULL, "HasPMULL");
        Assert.AreEqual((hw & SHA1) != 0, a.HasSHA1, "HasSHA1");
        Assert.AreEqual((hw & SHA2) != 0, a.HasSHA2, "HasSHA2");
        Assert.AreEqual((hw & SHA3) != 0, a.HasSHA3, "HasSHA3");
        Assert.AreEqual((hw & SHA512) != 0, a.HasSHA512, "HasSHA512");
        Assert.AreEqual((hw & CRC32) != 0, a.HasCRC32, "HasCRC32");
        Assert.AreEqual((hw & ATOMICS) != 0, a.HasATOMICS, "HasATOMICS");
        Assert.AreEqual((hw & CPUID) != 0, a.HasCPUID, "HasCPUID");
        Assert.IsFalse(a.HasDIT, "HasDIT is held false: the CLR exposes no PSTATE.DIT control");
    }

    [TestMethod]
    public void TheClrSeesTheSameExtensions()
    {
        RequireNoCpuGodebug();

        var a = cpu.ARM64;

        Assert.AreEqual(Arm.Aes.IsSupported, a.HasAES, "AES: .NET's Arm.Aes against internal/cpu");
        Assert.AreEqual(Arm.Crc32.IsSupported, a.HasCRC32, "CRC32: .NET's Arm.Crc32 against internal/cpu");
        Assert.AreEqual(Arm.Sha1.IsSupported, a.HasSHA1, "SHA1: .NET's Arm.Sha1 against internal/cpu");
        Assert.AreEqual(Arm.Sha256.IsSupported, a.HasSHA2, "SHA2: .NET's Arm.Sha256 against internal/cpu");
    }

    [TestMethod]
    public void TheOptionTableIsCpuArm64Gos()
    {
        string[] names = cpu.GoLinuxArm64OptionsProbe("").Select(o => o.name).ToArray();

        CollectionAssert.AreEqual(
            new[] { "aes", "pmull", "sha1", "sha2", "sha512", "sha3", "crc32", "atomics", "cpuid", "isNeoverse" },
            names, "cpu_arm64.go doinit's option table, in its order");
    }

    [TestMethod]
    public void AnEmptyOrNonCpuGodebugLeavesEveryFlagAlone()
    {
        RequireNoCpuGodebug();

        (string name, bool enabled)[] empty = cpu.GoLinuxArm64OptionsProbe("");

        Assert.AreEqual(cpu.ARM64.HasAES, empty.Single(o => o.name == "aes").enabled, "the probe starts from start-up's state");
        CollectionAssert.AreEqual(empty, cpu.GoLinuxArm64OptionsProbe("gctrace=0,madvdontneed=1"), "non-cpu keys change nothing");
    }

    [TestMethod]
    public void CpuAesOffClearsHasAesAndNothingElse()
    {
        RequireNoCpuGodebug();

        (string name, bool enabled)[] before = cpu.GoLinuxArm64OptionsProbe("");
        (string name, bool enabled)[] after = cpu.GoLinuxArm64OptionsProbe("cpu.aes=off");

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
