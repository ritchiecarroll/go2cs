using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// S3 sites 3 and 4's Arm64 rows against the linux/arm64 flavour's own converted constants. This is the twin of
/// LinuxAmd64RuntimeSyscallFlavourTests (LinuxRuntimeSyscallNumberTests has the per-arch rows).
/// - linux/arm64 flavour only: GolibTests.csproj excludes this file unless $(GoTargetOS) is linux and $(GoTargetArch)
///   is arm64.
/// </summary>
[TestClass]
public class LinuxArm64RuntimeSyscallFlavourTests
{
    [TestMethod]
    public void TheArm64DieNumbersAgreeWithTheConvertedFlavour()
    {
        var n = runtime_package.GoDieNumbersProbe(Architecture.Arm64)!.Value;

        Assert.AreEqual((long)syscall_package.SYS_GETPID, n.getpid, "SYS_GETPID");
        Assert.AreEqual((long)syscall_package.SYS_GETTID, n.gettid, "SYS_GETTID");
        Assert.AreEqual((long)syscall_package.SYS_TGKILL, n.tgkill, "SYS_TGKILL");
        Assert.AreEqual((long)syscall_package.SYS_EXIT_GROUP, n.exitGroup, "SYS_EXIT_GROUP");
    }

    [TestMethod]
    public void TheArm64SigprocmaskNumberAgreesWithTheConvertedFlavour()
    {
        Assert.AreEqual((long)syscall_package.SYS_RT_SIGPROCMASK, runtime_package.GoSigprocmaskNumberProbe(Architecture.Arm64),
            "the converted flavour's SYS_RT_SIGPROCMASK");
    }
}
