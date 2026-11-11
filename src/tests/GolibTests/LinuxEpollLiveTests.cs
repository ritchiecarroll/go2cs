using System;
using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using poll = go.@internal.poll_package;

namespace GolibTests;

/// <summary>
/// S3 site 1's admission reading: the linux netpoll's epoll mapping (internal/poll/linux/runtime_netpoll_impl.cs)
/// proven LIVE on the running arch, in the commit that lifts the X64-only admission guard.
/// - GoEpollLiveProbe drives the mapping with its own epoll instance: epoll_create1, EPOLL_CTL_ADD of a pipe's read end
///   with a 64-bit token, one byte written, then the wait (epoll_wait at amd64, epoll_pwait with a NULL sigmask at
///   arm64). The token must read back from this arch's data offset, so a wrong event size or offset (12 bytes packed
///   at amd64, 16 unpacked at arm64) reads as a wrong token.
/// - The os.Create and os.ReadFile round trip is the path the guard broke at linux/arm64: the first pollable
///   FD.Init reaches the poller's initializer (runs 42959669726 and 42970423967).
/// - Linux flavour only (GolibTests.csproj excludes this file under every other $(GoTargetOS)); it runs on both linux
///   arches.
/// </summary>
[TestClass]
public class LinuxEpollLiveTests
{
    // Every byte distinct, so a read at the wrong offset cannot match by accident.
    private const ulong Token = 0x1122334455667788UL;

    [TestMethod]
    public void TheMappingWaitsAndReadsTheTokenBackLive()
    {
        var probe = poll.GoEpollLiveProbe(Token);
        Assert.IsNotNull(probe, $"{RuntimeInformation.ProcessArchitecture} must have an epoll mapping");

        var (createErrno, ctlErrno, waitErrno, ready, events, token, eventSize) = probe.Value;

        Assert.AreEqual((nuint)0, createErrno, "epoll_create1's errno");
        Assert.AreEqual((nuint)0, ctlErrno, "EPOLL_CTL_ADD's errno");
        Assert.AreEqual((nuint)0, waitErrno, "the wait's errno (epoll_wait at amd64, epoll_pwait at arm64)");
        Assert.AreEqual(1, ready, "exactly the pipe's read end is ready");
        Assert.AreEqual(1u, events & 1u, "EPOLLIN is set");
        Assert.AreEqual(Token, token, "the data word reads back from this arch's offset");
        Assert.AreEqual(RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? 16 : 12, eventSize,
            "struct epoll_event: 16 bytes unpacked at arm64, 12 packed at amd64");
    }

    [TestMethod]
    public void OsCreateAndReadFileRoundTrip()
    {
        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"go2cs-epoll-live-{Guid.NewGuid():N}.txt");
        byte[] want = [(byte)'a', (byte)'r', (byte)'m', (byte)'6', (byte)'4'];

        try
        {
            var (f, createErr) = os_package.Create(path);
            Assert.IsNull(createErr, $"os.Create: {createErr?.Error()}");

            var (n, writeErr) = f.Write(want);
            Assert.IsNull(writeErr, $"File.Write: {writeErr?.Error()}");
            Assert.AreEqual((nint)want.Length, n, "bytes written");

            error closeErr = f.Close();
            Assert.IsNull(closeErr, $"File.Close: {closeErr?.Error()}");

            var (data, readErr) = os_package.ReadFile(path);
            Assert.IsNull(readErr, $"os.ReadFile: {readErr?.Error()}");
            CollectionAssert.AreEqual(want, data.ToSpan().ToArray(), "os.ReadFile reads back what was written");
        }
        finally
        {
            System.IO.File.Delete(path);
        }
    }
}
