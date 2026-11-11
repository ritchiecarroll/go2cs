using System;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// S3's runtime/linux sites (docs/phase4/DATA-la64-s3-census.md §2, §5(a)): the libc-syscall seams below the syscall
/// flavour take their numbers by arch, through their Go*Probe helpers.
/// - The rows are pinned to Go's own go1.26.10 tables beside the census (sysnum-linux-amd64.tsv and
///   sysnum-linux-arm64.tsv).
/// - Each flavour's own row is also cross-checked against that flavour's converted constants, in
///   LinuxAmd64RuntimeSyscallFlavourTests (linux/amd64) and LinuxArm64RuntimeSyscallFlavourTests (linux/arm64). A
///   converted SYS_* constant is the COMPILED flavour's, so a cross-check is true only in its own flavour.
/// - Linux flavour only: the probes live in runtime/linux (GolibTests.csproj excludes this file under every other
///   $(GoTargetOS)).
/// </summary>
[TestClass]
public class LinuxRuntimeSyscallNumberTests
{
    // Site 3: dieFromSignal's getpid, gettid, tgkill and exit_group (runtime/linux/signal_posix_impl.cs).
    [TestMethod]
    public void DieFromSignalTakesEachArchsOwnNumbers()
    {
        Assert.AreEqual((39L, 186L, 234L, 231L), runtime_package.GoDieNumbersProbe(Architecture.X64)!.Value, "amd64");
        Assert.AreEqual((172L, 178L, 131L, 94L), runtime_package.GoDieNumbersProbe(Architecture.Arm64)!.Value,
            "arm64 (amd64's exit_group 231 is munlockall here)");

        foreach (Architecture arch in Enum.GetValues<Architecture>())
        {
            if (arch is not (Architecture.X64 or Architecture.Arm64))
                Assert.IsNull(runtime_package.GoDieNumbersProbe(arch), $"{arch} has no mapping and must be refused");
        }
    }

    // Site 4: rtsigprocmask's rt_sigprocmask (runtime/linux/sigprocmask_impl.cs).
    [TestMethod]
    public void RtSigprocmaskTakesEachArchsOwnNumber()
    {
        Assert.AreEqual(14L, runtime_package.GoSigprocmaskNumberProbe(Architecture.X64), "amd64");
        Assert.AreEqual(135L, runtime_package.GoSigprocmaskNumberProbe(Architecture.Arm64), "arm64 (amd64's 14 is removexattr here)");

        foreach (Architecture arch in Enum.GetValues<Architecture>())
        {
            if (arch is not (Architecture.X64 or Architecture.Arm64))
                Assert.IsNull(runtime_package.GoSigprocmaskNumberProbe(arch), $"{arch} has no mapping and must be refused");
        }
    }
}
