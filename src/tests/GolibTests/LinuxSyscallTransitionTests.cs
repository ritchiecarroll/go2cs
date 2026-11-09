using System;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// A linux system call enters the runtime's syscall family the way Go's does (docs/phase4/DESIGN-native-call-seam.md
// §9 F-1, §13.2). Go's syscall.Syscall brackets RawSyscall6 with runtime_entersyscall / runtime_exitsyscall, which
// Go linknames to runtime.entersyscall / exitsyscall: for the length of the call the goroutine reads _Gsyscall (a
// goroutine profile says "syscall"), and the transition seam (GoNativeCall) is open. The converted bodies of those two
// in syscall/linux/syscall_linux_impl.cs are EMPTY, so no linux syscall reaches either.
//
// The keystone takes a syscall NUMBER, not an address, so S-b's shape (an [UnmanagedCallersOnly] probe reading the
// status from inside the window) cannot reach it. A goroutine blocked in read(2) on a pipe is observed from the
// test's thread instead: its g status through the runtime's GoGStatusOf seam, its transition depth on its golib
// Goroutine. Writing one byte then releases it, and the read's result proves it was blocked in that call.
//
// Linux-only by nature: syscall.Syscall(trap, a1, a2, a3) is declared by the linux flavour (GolibTests.csproj removes
// this file for any other).
[TestClass]
public unsafe class LinuxSyscallTransitionTests
{
    private const uint Grunning = 2;

    [DllImport("libc", SetLastError = true)]
    private static extern int pipe(int* fds);

    [DllImport("libc", SetLastError = true)]
    private static extern nint write(int fd, byte* buf, nuint count);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    [TestMethod]
    public void ASyscallBlockedInReadRunsInsideGsyscallAndOneSeamTransition()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Inconclusive("syscall.Syscall's linux bracket: NOT MEASURED on this host");
            return;
        }

        int* fds = stackalloc int[2];
        Assert.AreEqual(0, pipe(fds), $"premise: pipe(2) failed, errno {Marshal.GetLastPInvokeError()}");
        int readFd = fds[0], writeFd = fds[1];
        byte* buffer = (byte*)NativeMemory.Alloc(1);
        nuint bufferAddress = (nuint)buffer;

        try
        {
            Goroutine? blocked = null;
            object? gHandle = null;
            uint before = 0, after = 0;
            int depthBefore = -2, depthAfter = -2;
            uintptr r1 = 0;
            syscall_package.Errno errno = default;
            Exception? thrown = null;
            using ManualResetEventSlim entering = new();
            using ManualResetEventSlim done = new();

            goǃ(() =>
            {
                try
                {
                    blocked = Goroutine.Current;
                    gHandle = runtime_package.GoCurrentGHandle();
                    before = runtime_package.GoCurrentGStatus();
                    depthBefore = GoNativeCall.TransitionDepth;
                    entering.Set();

                    (r1, _, errno) = syscall_package.Syscall((uintptr)syscall_package.SYS_READ, (uintptr)(nuint)readFd, (uintptr)bufferAddress, 1);

                    after = runtime_package.GoCurrentGStatus();
                    depthAfter = GoNativeCall.TransitionDepth;
                }
                catch (Exception ex)
                {
                    thrown = ex;
                }
                finally
                {
                    entering.Set();
                    done.Set();
                }
            });

            Assert.IsTrue(entering.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not start");
            Assert.IsNotNull(blocked, $"premise: the goroutine has a golib identity ({thrown})");
            Assert.IsNotNull(gHandle, "premise: the goroutine has a runtime g");

            // Poll until the goroutine reads as Go's would inside the call, or until the deadline. Before the read
            // starts it is _Grunning at depth 0 either way; only a goroutine INSIDE the bracket reads _Gsyscall at 1.
            uint inside = runtime_package.GoGStatusOf(gHandle!);
            int depthInside = Volatile.Read(ref blocked!.m_nativeDepth);
            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);

            while ((inside != runtime_package.GoGSyscallStatus() || depthInside != 1) && DateTime.UtcNow < deadline && !done.IsSet)
            {
                Thread.Sleep(10);
                inside = runtime_package.GoGStatusOf(gHandle!);
                depthInside = Volatile.Read(ref blocked.m_nativeDepth);
            }

            Assert.IsFalse(done.IsSet, $"premise: the goroutine is still blocked in read(2) while it is observed ({thrown})");

            byte one = 1;
            Assert.AreEqual((nint)1, write(writeFd, &one, 1), "premise: the releasing write(2)");
            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), "the goroutine did not return from read(2)");
            Assert.IsNull(thrown, $"the call threw on a goroutine: {thrown}");
            Assert.AreEqual((uintptr)1, r1, $"premise: the read returned the released byte (errno {errno})");

            Assert.AreEqual(Grunning, before, "a running goroutine enters the call _Grunning");
            Assert.AreEqual(0, depthBefore, "and outside native code");
            Assert.AreEqual(runtime_package.GoGSyscallStatus(), inside, "blocked in read(2) it reads _Gsyscall, as Go's runtime_entersyscall makes it");
            Assert.AreEqual(1, depthInside, "and the seam saw exactly one transition (Q2: one path, entered once)");
            Assert.AreEqual(Grunning, after, "and it leaves _Grunning");
            Assert.AreEqual(0, depthAfter, "with the transition closed");
        }
        finally
        {
            close(readFd);
            close(writeFd);
            NativeMemory.Free(buffer);
        }
    }
}
