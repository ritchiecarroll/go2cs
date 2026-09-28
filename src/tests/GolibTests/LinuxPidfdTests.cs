using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using syscall = go.syscall_package;
using unix = go.@internal.syscall.unix_package;

namespace GolibTests;

/// <summary>
/// The pidfd a posix_spawn child gets through SysProcAttr.PidFD, and the os probe built on it.
/// </summary>
/// <remarks>
/// os's checkClonePidfd (os/linux/pidfd_linux_impl.cs) answered ENOSYS until 2026-09-27, so os never
/// took its pidfd paths and TestFindProcessViaPidfd/TestStartProcessWithPidfd skipped on "pidfd not
/// available: clone(CLONE_PIDFD): function not implemented". The probe now answers nil: the spawn seam
/// mints a child's pidfd by pidfd_open(child) right after posix_spawn, and checkPidfd's own first step
/// has already proven pidfd_open on this kernel. This arm pins that seam -- a spawned child's pidfd
/// signals and reaps it -- which every os process operation now rests on. It was green before the probe
/// changed (the seam's pidfd predates it), so it is a MUST-NOT-REGRESS arm; the probe's own red-first
/// arms are os's two pidfd tests.
/// </remarks>
[TestClass]
public class LinuxPidfdTests
{
    private const int SIGKILL = 9;
    private const int CLD_KILLED = 2;

    [TestMethod]
    public void ASpawnedChildsPidfdSignalsAndReapsIt()
    {
        ref var pidfd = ref heap<nint>(out var Ꮡpidfd);
        pidfd = -1;

        syscall.ProcAttr attr = new()
        {
            Files = new slice<uintptr>(new uintptr[] { 0, 1, 2 }),
            Sys = new StandardBox<syscall.SysProcAttr>(new syscall.SysProcAttr { PidFD = Ꮡpidfd }),
        };

        var (pid, _, err) = syscall.StartProcess(
            "/bin/sleep"u8,
            new slice<@string>(new @string[] { "/bin/sleep"u8, "30"u8 }),
            new StandardBox<syscall.ProcAttr>(attr));

        Assert.IsNull(err, $"spawning /bin/sleep failed: {err}");
        Assert.IsTrue(pidfd >= 0, "SysProcAttr.PidFD must come back filled with a pidfd for the spawned child");

        try
        {
            Assert.IsNull(unix.PidFDSendSignal((uintptr)pidfd, syscall.SIGKILL), "pidfd_send_signal through the child's pidfd");

            ref var info = ref heap(new unix.SiginfoChild(), out var Ꮡinfo);
            error werr;

            do
            {
                werr = unix.Waitid((nint)unix.P_PIDFD, pidfd, Ꮡinfo, (nint)syscall.WEXITED, nil);
            }
            while (AreEqual(werr, syscall.EINTR));

            Assert.IsNull(werr, $"waitid(P_PIDFD) must reap the child: {werr}");
            Assert.AreEqual((int)pid, info.Pid, "the pidfd names the spawned child");
            Assert.AreEqual(CLD_KILLED, info.Code, "the child died of the signal sent through its pidfd");
            Assert.AreEqual(SIGKILL, info.Status);
        }
        finally
        {
            syscall.Close(pidfd);
        }
    }
}
