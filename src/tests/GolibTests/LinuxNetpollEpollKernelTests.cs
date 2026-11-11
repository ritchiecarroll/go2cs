using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using poll = go.@internal.poll_package;

namespace GolibTests;

/// <summary>
/// S3 site 1 (docs/phase4/DATA-la64-s3-census.md): the linux netpoll hand-own's per-arch kernel contract,
/// internal/poll/linux/runtime_netpoll_impl.cs's epollKernelFor, read through its GoEpollKernelProbe.
/// - The rows are pinned to Go's own go1.26.10 tables beside the census: sysnum-linux-amd64.tsv,
///   sysnum-linux-arm64.tsv and layouts-linux-amd64-arm64.tsv.
/// - Each flavour's own row is also cross-checked against that flavour's converted constants:
///   LinuxAmd64NetpollFlavourTests at linux/amd64 and LinuxArm64NetpollFlavourTests at linux/arm64. Each names
///   constants only its flavour's table has (SYS_EPOLL_WAIT is amd64's alone), so each is its own file.
/// - Linux flavour only: the probe exists only in internal/poll/linux (GolibTests.csproj excludes this file
///   under every other $(GoTargetOS)).
/// </summary>
[TestClass]
public class LinuxNetpollEpollKernelTests
{
    [TestMethod]
    public void Amd64KeepsEpollWaitAndThePackedTwelveByteEvent()
    {
        var k = poll.GoEpollKernelProbe(Architecture.X64);

        Assert.IsNotNull(k, "X64 must be mapped");
        Assert.AreEqual(((nuint)232, (nuint)233, (nuint)291, 12, 4), k.Value,
            "amd64: epoll_wait 232, epoll_ctl 233, epoll_create1 291; struct epoll_event packed, 12 bytes, data at 4");
    }

    [TestMethod]
    public void Arm64UsesEpollPwaitAndTheUnpackedSixteenByteEvent()
    {
        var k = poll.GoEpollKernelProbe(Architecture.Arm64);

        Assert.IsNotNull(k, "Arm64 must be mapped");
        Assert.AreEqual(((nuint)22, (nuint)21, (nuint)20, 16, 8), k.Value,
            "arm64: epoll_pwait 22 (there is no epoll_wait), epoll_ctl 21, epoll_create1 20; struct epoll_event unpacked, 16 bytes, data at 8");
    }

    [TestMethod]
    public void EveryOtherArchIsRefused()
    {
        foreach (Architecture arch in Enum.GetValues<Architecture>())
        {
            if (arch is Architecture.X64 or Architecture.Arm64)
                continue;

            Assert.IsNull(poll.GoEpollKernelProbe(arch), $"{arch} has no mapping and must be refused, not misread");
        }
    }
}
