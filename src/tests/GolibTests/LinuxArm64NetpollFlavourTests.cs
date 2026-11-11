using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using poll = go.@internal.poll_package;

namespace GolibTests;

/// <summary>
/// S3 site 1's Arm64 row against the linux/arm64 flavour's own converted constants and EpollEvent. This is the twin of
/// LinuxAmd64NetpollFlavourTests, added by S3 site 9.
/// - arm64 has no epoll_wait: the row's wait is epoll_pwait, and the flavour's table names it SYS_EPOLL_PWAIT.
/// - linux/arm64 flavour only: GolibTests.csproj excludes this file unless $(GoTargetOS) is linux and
///   $(GoTargetArch) is arm64.
/// </summary>
[TestClass]
public class LinuxArm64NetpollFlavourTests
{
    [TestMethod]
    public void TheArm64RowAgreesWithTheConvertedFlavour()
    {
        var k = poll.GoEpollKernelProbe(Architecture.Arm64)!.Value;

        Assert.AreEqual((uintptr)syscall_package.SYS_EPOLL_PWAIT, (uintptr)k.wait, "SYS_EPOLL_PWAIT");
        Assert.AreEqual((uintptr)syscall_package.SYS_EPOLL_CTL, (uintptr)k.ctl, "SYS_EPOLL_CTL");
        Assert.AreEqual((uintptr)syscall_package.SYS_EPOLL_CREATE1, (uintptr)k.create1, "SYS_EPOLL_CREATE1");
        Assert.AreEqual(Unsafe.SizeOf<syscall_package.EpollEvent>(), k.eventSize, "the flavour's EpollEvent size");
    }
}
