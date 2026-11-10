// InternalCpuGodebugTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.@internal;
using Arm = System.Runtime.Intrinsics.Arm;
using X86 = System.Runtime.Intrinsics.X86;

namespace GolibTests;

/// <summary>
/// Guards internal/cpu's start-up GODEBUG handling (cpu_x86_impl.cs): doinit's option table is
/// registered on every OS and GODEBUG's cpu.* options are applied only off Windows, as Go's
/// getGodebugEarly does. An empty or non-cpu GODEBUG leaves every flag alone, and cpu.aes=off clears
/// HasAES and nothing else. On an arm64 process the table is cpu_arm64.go's and the flags are cpu.ARM64,
/// filled by the OS's osInit (cpu_arm64_process_impl.cs; darwin/cpu_arm64_darwin_impl.cs from sysctl,
/// linux/cpu_arm64_process_linux_impl.cs from the kernel's AT_HWCAP), with HasDIT held false by ruling; on an
/// x64 process cpu.ARM64 stays unfilled, as before.
/// </summary>
[TestClass]
public class InternalCpuGodebugTests
{
    [TestMethod]
    public void GodebugIsAppliedOnlyOffWindows()
    {
        Assert.AreEqual(!OperatingSystem.IsWindows(), cpu_package.GoCpuAppliesGodebug);
    }

    [TestMethod]
    public void AnEmptyOrNonCpuGodebugLeavesEveryFlagAlone()
    {
        RequireX86();
        (string name, bool enabled)[] empty = cpu_package.GoCpuOptionsProbe("");

        // 23 since Go 1.26 (vpclmulqdq, avx512cd and avx512dq joined doinit's table; 20 through 1.25).
        Assert.AreEqual(23, empty.Length, "doinit's table at GOAMD64 level 1");
        Assert.AreEqual(cpu_package.X86.HasAES, Flag(empty, "aes"));
        Assert.AreEqual(cpu_package.X86.HasSSE41, Flag(empty, "sse41"));
        Assert.AreEqual(cpu_package.X86.HasAVX2, Flag(empty, "avx2"));
        CollectionAssert.AreEqual(empty, cpu_package.GoCpuOptionsProbe("gctrace=0,madvdontneed=1"), "non-cpu keys change nothing");
    }

    [TestMethod]
    public void CpuAesOffClearsHasAesAndNothingElse()
    {
        RequireX86();
        (string name, bool enabled)[] before = cpu_package.GoCpuOptionsProbe("");
        (string name, bool enabled)[] after = cpu_package.GoCpuOptionsProbe("cpu.aes=off");

        Assert.IsFalse(Flag(after, "aes"), "cpu.aes=off clears HasAES");
        CollectionAssert.AreEqual(before.Where(o => o.name != "aes").ToArray(), after.Where(o => o.name != "aes").ToArray(), "every other flag is untouched");
        Assert.AreEqual(X86.Aes.IsSupported, cpu_package.X86.HasAES, "the probe restores the live flag");
    }

    [TestMethod]
    public void AnX64ProcessLeavesArm64Unfilled()
    {
        RequireX86();
        Assert.IsFalse(cpu_package.GoIsArm64Process);
        var arm64 = cpu_package.ARM64;
        bool[] flags = [arm64.HasAES, arm64.HasPMULL, arm64.HasSHA1, arm64.HasSHA2, arm64.HasSHA512, arm64.HasSHA3,
            arm64.HasCRC32, arm64.HasATOMICS, arm64.HasCPUID, arm64.HasDIT, arm64.IsNeoverse];
        Assert.IsFalse(flags.Any(f => f), "the arm64 arm runs only on an arm64 process");
    }

    [TestMethod]
    public void Arm64RegistersCpuArm64GosOptionTable()
    {
        RequireArm64();
        (string name, bool enabled)[] empty = cpu_package.GoCpuOptionsProbe("");

        CollectionAssert.AreEqual(
            new[] { "aes", "pmull", "sha1", "sha2", "sha512", "sha3", "crc32", "atomics", "cpuid", "isNeoverse" },
            empty.Select(o => o.name).ToArray(), "cpu_arm64.go doinit's table, in order");
        Assert.AreEqual(cpu_package.ARM64.HasAES, Flag(empty, "aes"));
        Assert.AreEqual(cpu_package.ARM64.HasPMULL, Flag(empty, "pmull"));
        CollectionAssert.AreEqual(empty, cpu_package.GoCpuOptionsProbe("gctrace=0,madvdontneed=1"), "non-cpu keys change nothing");
    }

    [TestMethod]
    public void Arm64CpuAesOffClearsHasAesAndNothingElse()
    {
        RequireArm64();
        (string name, bool enabled)[] before = cpu_package.GoCpuOptionsProbe("");
        (string name, bool enabled)[] after = cpu_package.GoCpuOptionsProbe("cpu.aes=off");

        Assert.IsFalse(Flag(after, "aes"), "cpu.aes=off clears ARM64.HasAES");
        CollectionAssert.AreEqual(before.Where(o => o.name != "aes").ToArray(), after.Where(o => o.name != "aes").ToArray(), "every other flag is untouched");
        Assert.AreEqual(Flag(before, "aes"), cpu_package.ARM64.HasAES, "the probe restores the live flag");
    }

    [TestMethod]
    public void DarwinArm64OsInitSetsGosFeaturesAndHoldsDitFalse()
    {
        if (!(cpu_package.GoIsArm64Process && OperatingSystem.IsMacOS()))
            Assert.Inconclusive("not an arm64 process on macOS");

        // Go's darwin osInit sets these four unconditionally.
        Assert.IsTrue(cpu_package.ARM64.HasAES, "HasAES");
        Assert.IsTrue(cpu_package.ARM64.HasPMULL, "HasPMULL");
        Assert.IsTrue(cpu_package.ARM64.HasSHA1, "HasSHA1");
        Assert.IsTrue(cpu_package.ARM64.HasSHA2, "HasSHA2");

        // Every Apple silicon part reports the ARMv8 CRC32 and v8.1 atomics sysctls.
        Assert.IsTrue(cpu_package.ARM64.HasCRC32, "hw.optional.armv8_crc32");
        Assert.IsTrue(cpu_package.ARM64.HasATOMICS, "hw.optional.armv8_1_atomics");

        // RULED (COORD, the AES trio seat's Q2): the .NET runtime exposes no PSTATE.DIT control, so the
        // flag describes what the converted runtime can do and stays false whatever the silicon reports.
        Assert.IsFalse(cpu_package.ARM64.HasDIT, "HasDIT is held false");

        // darwin's osInit sets neither.
        Assert.IsFalse(cpu_package.ARM64.HasCPUID, "HasCPUID");
        Assert.IsFalse(cpu_package.ARM64.IsNeoverse, "IsNeoverse");
    }

    [TestMethod]
    public void LinuxArm64OsInitMapsTheKernelsHwcapAsGoDoes()
    {
        if (!(cpu_package.GoIsArm64Process && OperatingSystem.IsLinux()))
            Assert.Inconclusive("not an arm64 process on Linux");

        // A second read of the kernel's word, independent of the file under test's getauxval: the
        // auxiliary vector as /proc/self/auxv publishes it, (type, value) pairs of 64-bit words.
        ulong hwcap = ReadAuxv(16); // AT_HWCAP
        bool Bit(int n) => (hwcap & (1UL << n)) != 0;

        // cpu_arm64_hwcap.go's hwcapInit, bit for bit (arch/arm64/include/uapi/asm/hwcap.h).
        Assert.AreEqual(Bit(3), cpu_package.ARM64.HasAES, "HWCAP_AES");
        Assert.AreEqual(Bit(4), cpu_package.ARM64.HasPMULL, "HWCAP_PMULL");
        Assert.AreEqual(Bit(5), cpu_package.ARM64.HasSHA1, "HWCAP_SHA1");
        Assert.AreEqual(Bit(6), cpu_package.ARM64.HasSHA2, "HWCAP_SHA2");
        Assert.AreEqual(Bit(7), cpu_package.ARM64.HasCRC32, "HWCAP_CRC32");
        Assert.AreEqual(Bit(8), cpu_package.ARM64.HasATOMICS, "HWCAP_ATOMICS");
        Assert.AreEqual(Bit(11), cpu_package.ARM64.HasCPUID, "HWCAP_CPUID");
        Assert.AreEqual(Bit(17), cpu_package.ARM64.HasSHA3, "HWCAP_SHA3");
        Assert.AreEqual(Bit(21), cpu_package.ARM64.HasSHA512, "HWCAP_SHA512");

        // RULED (COORD, 2026-11-06), as on darwin: whatever HWCAP_DIT (bit 24) says.
        Assert.IsFalse(cpu_package.ARM64.HasDIT, "HasDIT is held false");

        // .NET reads the same word for its own intrinsics, so where it has a class for the feature the
        // two agree. A cross-check only: the kernel's word above is the source.
        Assert.AreEqual(Arm.Aes.IsSupported, cpu_package.ARM64.HasAES, "Arm.Aes");
        Assert.AreEqual(Arm.Sha1.IsSupported, cpu_package.ARM64.HasSHA1, "Arm.Sha1");
        Assert.AreEqual(Arm.Sha256.IsSupported, cpu_package.ARM64.HasSHA2, "Arm.Sha256");
        Assert.AreEqual(Arm.Crc32.IsSupported, cpu_package.ARM64.HasCRC32, "Arm.Crc32");

        // IsNeoverse: Arm Ltd. (implementer 'A') and one of hwcapInit's seven part numbers, read from
        // the kernel's MIDR_EL1 for cpu0, only when HWCAP_CPUID lets Go read it at all.
        bool neoverse = false;
        string midrPath = "/sys/devices/system/cpu/cpu0/regs/identification/midr_el1";
        if (Bit(11) && File.Exists(midrPath))
        {
            ulong midr = ulong.Parse(File.ReadAllText(midrPath).Trim()[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            ushort part = (ushort)((midr >> 4) & 0xfff);
            neoverse = ((midr >> 24) & 0xff) == 'A' && part is 0xd0c or 0xd40 or 0xd49 or 0xd4f or 0xd8e or 0xd84 or 0xd83;
        }
        Assert.AreEqual(neoverse, cpu_package.ARM64.IsNeoverse, "IsNeoverse");
    }

    private static ulong ReadAuxv(ulong type)
    {
        byte[] auxv = File.ReadAllBytes("/proc/self/auxv");
        for (int i = 0; i + 16 <= auxv.Length; i += 16)
        {
            ulong key = BitConverter.ToUInt64(auxv, i);
            if (key == type)
                return BitConverter.ToUInt64(auxv, i + 8);
            if (key == 0) // AT_NULL
                break;
        }
        Assert.Fail($"/proc/self/auxv carries no entry of type {type}");
        return 0;
    }

    private static bool Flag((string name, bool enabled)[] options, string name) => options.Single(o => o.name == name).enabled;

    private static void RequireArm64()
    {
        if (!cpu_package.GoIsArm64Process)
            Assert.Inconclusive("not an arm64 process: internal/cpu registers no arm64 options here");
    }

    private static void RequireX86()
    {
        if (!X86.X86Base.IsSupported)
            Assert.Inconclusive("not an x86 host: internal/cpu registers no x86 options here");
    }
}
