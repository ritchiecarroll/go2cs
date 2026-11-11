using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// S3 sites 3 and 4's X64 rows against the linux/amd64 flavour's own converted constants
/// (LinuxRuntimeSyscallNumberTests has the per-arch rows).
/// - linux/amd64 flavour only: a converted SYS_* constant is the compiled flavour's. At linux/arm64, SYS_GETPID is 172 and
///   SYS_RT_SIGPROCMASK is 135, so these checks are true here alone. GolibTests.csproj excludes this file unless
///   $(GoTargetOS) is linux and $(GoTargetArch) is not arm64.
/// - Moved here from LinuxRuntimeSyscallNumberTests after the first arm-leg reading (run 42956466727) read both reds at
///   linux/arm64: "Expected:&lt;172&gt;. Actual:&lt;39&gt;. SYS_GETPID" and "Expected:&lt;135&gt;. Actual:&lt;14&gt;".
///   LinuxArm64RuntimeSyscallFlavourTests is its twin.
/// </summary>
[TestClass]
public class LinuxAmd64RuntimeSyscallFlavourTests
{
    [TestMethod]
    public void TheAmd64DieNumbersAgreeWithTheConvertedFlavour()
    {
        var n = runtime_package.GoDieNumbersProbe(Architecture.X64)!.Value;

        Assert.AreEqual((long)syscall_package.SYS_GETPID, n.getpid, "SYS_GETPID");
        Assert.AreEqual((long)syscall_package.SYS_GETTID, n.gettid, "SYS_GETTID");
        Assert.AreEqual((long)syscall_package.SYS_TGKILL, n.tgkill, "SYS_TGKILL");
        Assert.AreEqual((long)syscall_package.SYS_EXIT_GROUP, n.exitGroup, "SYS_EXIT_GROUP");
    }

    [TestMethod]
    public void TheAmd64SigprocmaskNumberAgreesWithTheConvertedFlavour()
    {
        Assert.AreEqual((long)syscall_package.SYS_RT_SIGPROCMASK, runtime_package.GoSigprocmaskNumberProbe(Architecture.X64),
            "the converted flavour's SYS_RT_SIGPROCMASK");
    }
}
