using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using poll = go.@internal.poll_package;

namespace GolibTests;

/// <summary>
/// S3 site 1's X64 row against the linux/amd64 flavour's own converted constants and EpollEvent
/// (LinuxNetpollEpollKernelTests has the arch-neutral rows).
/// - linux/amd64 flavour only: SYS_EPOLL_WAIT is in amd64's table alone. GolibTests.csproj excludes this file unless
///   $(GoTargetOS) is linux and $(GoTargetArch) is not arm64. An unset $(GoTargetArch) is amd64.
/// - Moved here from LinuxNetpollEpollKernelTests by S3 site 9, so that GolibTests compiles at linux/arm64.
///   LinuxArm64NetpollFlavourTests is its twin.
/// </summary>
[TestClass]
public class LinuxAmd64NetpollFlavourTests
{
    [TestMethod]
    public void TheAmd64RowAgreesWithTheConvertedFlavour()
    {
        var k = poll.GoEpollKernelProbe(Architecture.X64)!.Value;

        Assert.AreEqual((uintptr)syscall_package.SYS_EPOLL_WAIT, (uintptr)k.wait, "SYS_EPOLL_WAIT");
        Assert.AreEqual((uintptr)syscall_package.SYS_EPOLL_CTL, (uintptr)k.ctl, "SYS_EPOLL_CTL");
        Assert.AreEqual((uintptr)syscall_package.SYS_EPOLL_CREATE1, (uintptr)k.create1, "SYS_EPOLL_CREATE1");
        Assert.AreEqual(Unsafe.SizeOf<syscall_package.EpollEvent>(), k.eventSize, "the flavour's EpollEvent size");
    }
}
