// DarwinSyscallBracketTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using syscall = go.syscall_package;

namespace GolibTests;

// darwin's syscall keystones enter a system call the way Go's do (docs/phase4/DESIGN-darwin-syscall-bracket.md;
// COORD 20261126T071719Z with P3's F1-F7). Go brackets four of them with entersyscall/exitsyscall, syscalln
// (runtime/sys_darwin.go's syscall_syscalln) and the exported Syscall, Syscall6 and Syscall9 (asm_darwin_amd64.s),
// and leaves rawsyscalln, RawSyscall and RawSyscall6 raw. For the length of a bracketed call the goroutine reads
// _Gsyscall and the transition seam (GoNativeCall) is open, in default mode too.
//
// Three kinds of row:
//   - DIRECT: an [UnmanagedCallersOnly] probe stands in for the libc function, as DarwinNativeTransitionTests does,
//     and reads the goroutine's status and the seam's depth from inside the call. syscalln and rawsyscalln are
//     reached through syscall's GoSyscallnCall/GoRawSyscallnCall seams, because their `params variadic<uintptr>`
//     pack cannot be built by reflection.
//   - REAL WRAPPERS (P3's F3): the converted cooked bodies (GoSyscall6X, GoSyscallPtr, and syscall.Read on a pipe)
//     and a raw call to libc's own read. A probe cannot stand inside a real libc read, so a goroutine BLOCKED in
//     it is observed from the test's thread, as LinuxSyscallTransitionTests does on linux (linux's F-1 row, here on
//     darwin: P3's F1, the default-mode arm).
//   - THROWS (P3's F4): golib's call can throw where Go's cannot (a null function pointer; more than nine
//     arguments). Each bracketed entry must leave the goroutine _Grunning with the transition closed, and the
//     exception that surfaces must be the original one.
//
// Darwin-only by nature: the seams and the keystones are declared by the darwin flavour (GolibTests.csproj removes
// this file for any other). On any other host each row is Inconclusive, never a pass.
[TestClass]
public unsafe class DarwinSyscallBracketTests
{
    private const uint Grunning = 2;
    private const nuint ProbeResult = 50;

    [ThreadStatic]
    private static uint t_statusInside;

    [ThreadStatic]
    private static int t_depthInside;

    // Every arity the keystones pass: a cdecl callee ignores argument registers it does not read.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static nuint Probe()
    {
        t_statusInside = runtime_package.GoCurrentGStatus();
        t_depthInside = GoNativeCall.TransitionDepth;
        return ProbeResult;
    }

    private static uintptr ProbeFn => (uintptr)(nuint)(nint)(delegate* unmanaged[Cdecl]<nuint>)&Probe;

    // ---- DIRECT rows ----------------------------------------------------------------------------------------------

    [TestMethod]
    public void SyscallIsBracketed() =>
        AssertDirect(() => syscall.Syscall(ProbeFn, 1, 2, 3), bracketed: true, "Syscall");

    [TestMethod]
    public void Syscall6IsBracketed() =>
        AssertDirect(() => syscall.Syscall6(ProbeFn, 1, 2, 3, 4, 5, 6), bracketed: true, "Syscall6");

    [TestMethod]
    public void Syscall9IsBracketed() =>
        AssertDirect(() => syscall.Syscall9(ProbeFn, 1, 2, 3, 4, 5, 6, 7, 8, 9), bracketed: true, "Syscall9");

    [TestMethod]
    public void SyscallnIsBracketed() =>
        AssertDirect(() => syscall.GoSyscallnCall(ProbeFn, stackalloc uintptr[] { 1, 2, 3 }), bracketed: true, "syscalln");

    [TestMethod]
    public void RawSyscallStaysRaw() =>
        AssertDirect(() => syscall.RawSyscall(ProbeFn, 1, 2, 3), bracketed: false, "RawSyscall");

    [TestMethod]
    public void RawSyscall6StaysRaw() =>
        AssertDirect(() => syscall.RawSyscall6(ProbeFn, 1, 2, 3, 4, 5, 6), bracketed: false, "RawSyscall6");

    [TestMethod]
    public void RawsyscallnStaysRaw() =>
        AssertDirect(() => syscall.GoRawSyscallnCall(ProbeFn, stackalloc uintptr[] { 1, 2, 3 }), bracketed: false, "rawsyscalln");

    // ---- REAL WRAPPERS: the converted cooked bodies --------------------------------------------------------------

    [TestMethod]
    public void TheConvertedSyscall6XIsBracketed() =>
        AssertDirect(() => syscall.GoSyscall6X(ProbeFn, 1, 2, 3, 4, 5, 6), bracketed: true, "syscall6X (converted)");

    [TestMethod]
    public void TheConvertedSyscallPtrIsBracketed() =>
        AssertDirect(() => syscall.GoSyscallPtr(ProbeFn, 1, 2, 3), bracketed: true, "syscallPtr (converted)");

    private delegate (uintptr r1, uintptr r2, syscall.Errno err) KeystoneCall();

    private static void AssertDirect(KeystoneCall call, bool bracketed, string name)
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive($"darwin's {name}: NOT MEASURED on this host");
            return;
        }

        uint before = 0, inside = 0, after = 0;
        int depthBefore = -2, depthInside = -2, depthAfter = -2;
        uintptr r1 = 0;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                before = runtime_package.GoCurrentGStatus();
                depthBefore = GoNativeCall.TransitionDepth;

                (r1, _, _) = call();

                inside = t_statusInside;
                depthInside = t_depthInside;
                after = runtime_package.GoCurrentGStatus();
                depthAfter = GoNativeCall.TransitionDepth;
            }
            catch (Exception ex)
            {
                thrown = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), $"the goroutine did not finish {name}");
        Assert.IsNull(thrown, $"{name} threw on a goroutine: {thrown}");
        Assert.AreEqual((uintptr)ProbeResult, r1, $"premise: the probe ran as {name}'s native target");

        Assert.AreEqual(Grunning, before, "a running goroutine enters the call _Grunning");
        Assert.AreEqual(0, depthBefore, "and outside native code");

        if (bracketed)
        {
            Assert.AreEqual(runtime_package.GoGSyscallStatus(), inside, $"inside {name} it reads _Gsyscall, as Go's entersyscall makes it");
            Assert.AreEqual(1, depthInside, $"and the seam saw exactly one transition inside {name}");
        }
        else
        {
            Assert.AreEqual(Grunning, inside, $"inside {name} it stays _Grunning: Go's raw form does not enter a system call");
            Assert.AreEqual(0, depthInside, $"and no transition opens inside {name}");
        }

        Assert.AreEqual(Grunning, after, "it leaves _Grunning");
        Assert.AreEqual(0, depthAfter, "with no transition open");
    }

    // ---- REAL WRAPPERS: a goroutine blocked in a real read on a pipe ---------------------------------------------

    [DllImport("libc", SetLastError = true)]
    private static extern int pipe(int* fds);

    [DllImport("libc", SetLastError = true)]
    private static extern nint write(int fd, byte* buf, nuint count);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    private static uintptr LibcRead => (uintptr)(nuint)NativeLibrary.GetExport(NativeLibrary.Load("/usr/lib/libSystem.B.dylib"), "read");

    // linux's F-1 row on darwin, through the converted wrapper: syscall.Read -> read -> syscall -> syscalln.
    [TestMethod]
    public void AGoroutineBlockedInTheConvertedReadReadsGsyscall() =>
        AssertBlockedRead((fd, buffer) =>
        {
            var (n, err) = syscall.Read(fd, buffer);
            return (n, err is null ? "nil" : err.Error());
        }, bracketed: true, "syscall.Read");

    // The raw counterpart: RawSyscall on libc's own read blocks without entering a system call, as Go's raw form.
    [TestMethod]
    public void AGoroutineBlockedInARawReadStaysGrunning() =>
        AssertBlockedRead((fd, buffer) =>
        {
            fixed (byte* p = &buffer[0])
            {
                var (r1, _, errno) = syscall.RawSyscall(LibcRead, (uintptr)(nuint)fd, (uintptr)(nuint)p, 1);
                return ((nint)r1, errno == 0 ? "nil" : errno.Error());
            }
        }, bracketed: false, "RawSyscall(read)");

    private static void AssertBlockedRead(Func<nint, slice<byte>, (nint n, string err)> read, bool bracketed, string name)
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive($"darwin's {name}: NOT MEASURED on this host");
            return;
        }

        int* fds = stackalloc int[2];
        Assert.AreEqual(0, pipe(fds), $"premise: pipe(2) failed, errno {Marshal.GetLastPInvokeError()}");
        int readFd = fds[0], writeFd = fds[1];

        try
        {
            Goroutine? blocked = null;
            object? gHandle = null;
            uint before = 0, after = 0;
            int depthBefore = -2, depthAfter = -2;
            (nint n, string err) result = (-1, "unset");
            Exception? thrown = null;
            using ManualResetEventSlim entering = new();
            using ManualResetEventSlim done = new();
            slice<byte> buffer = new slice<byte>(1);

            goǃ(() =>
            {
                try
                {
                    blocked = Goroutine.Current;
                    gHandle = runtime_package.GoCurrentGHandle();
                    before = runtime_package.GoCurrentGStatus();
                    depthBefore = GoNativeCall.TransitionDepth;
                    entering.Set();

                    result = read(readFd, buffer);

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

            // Wait for the goroutine to be inside the read. A bracketed read shows itself as _Gsyscall at depth 1; a
            // raw read shows nothing, so for it the observation is taken once the read has had time to block (the
            // goroutine still not done), and it must read _Grunning at depth 0.
            uint wantInside = bracketed ? runtime_package.GoGSyscallStatus() : Grunning;
            int wantDepth = bracketed ? 1 : 0;
            uint inside = runtime_package.GoGStatusOf(gHandle!);
            int depthInside = Volatile.Read(ref blocked!.m_nativeDepth);
            DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(bracketed ? 5 : 1);

            while ((!bracketed || inside != wantInside || depthInside != wantDepth) && DateTime.UtcNow < deadline && !done.IsSet)
            {
                Thread.Sleep(10);
                inside = runtime_package.GoGStatusOf(gHandle!);
                depthInside = Volatile.Read(ref blocked.m_nativeDepth);
            }

            Assert.IsFalse(done.IsSet, $"premise: the goroutine is still blocked in {name} while it is observed ({thrown})");

            byte one = 1;
            Assert.AreEqual((nint)1, write(writeFd, &one, 1), "premise: the releasing write(2)");
            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), $"the goroutine did not return from {name}");
            Assert.IsNull(thrown, $"{name} threw on a goroutine: {thrown}");
            Assert.AreEqual((nint)1, result.n, $"premise: {name} returned the released byte (err {result.err})");

            Assert.AreEqual(Grunning, before, "a running goroutine enters the read _Grunning");
            Assert.AreEqual(0, depthBefore, "and outside native code");
            Assert.AreEqual(wantInside, inside, bracketed
                ? $"blocked in {name} it reads _Gsyscall, as Go's entersyscall makes it"
                : $"blocked in {name} it stays _Grunning: Go's raw form does not enter a system call");
            Assert.AreEqual(wantDepth, depthInside, bracketed ? "and the seam saw exactly one transition" : "and no transition opened");
            Assert.AreEqual(Grunning, after, "it leaves _Grunning");
            Assert.AreEqual(0, depthAfter, "with no transition open");
        }
        finally
        {
            close(readFd);
            close(writeFd);
        }
    }

    // ---- THROWS --------------------------------------------------------------------------------------------------

    [TestMethod]
    public void SyscallOnANullPointerThrowsAndLeavesGrunning() =>
        AssertThrows(() => syscall.Syscall(0, 1, 2, 3), "null function pointer", "Syscall");

    [TestMethod]
    public void Syscall6OnANullPointerThrowsAndLeavesGrunning() =>
        AssertThrows(() => syscall.Syscall6(0, 1, 2, 3, 4, 5, 6), "null function pointer", "Syscall6");

    [TestMethod]
    public void Syscall9OnANullPointerThrowsAndLeavesGrunning() =>
        AssertThrows(() => syscall.Syscall9(0, 1, 2, 3, 4, 5, 6, 7, 8, 9), "null function pointer", "Syscall9");

    [TestMethod]
    public void SyscallnOnANullPointerThrowsAndLeavesGrunning() =>
        AssertThrows(() => syscall.GoSyscallnCall(0, stackalloc uintptr[] { 1, 2, 3 }), "null function pointer", "syscalln");

    [TestMethod]
    public void SyscallnWithTenArgumentsThrowsAndLeavesGrunning() =>
        AssertThrows(() => syscall.GoSyscallnCall(ProbeFn, stackalloc uintptr[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }), "10 arguments", "syscalln");

    private static void AssertThrows(KeystoneCall call, string message, string name)
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive($"darwin's {name}: NOT MEASURED on this host");
            return;
        }

        uint after = 0;
        int depthAfter = -2;
        Exception? thrown = null;
        using ManualResetEventSlim done = new();

        goǃ(() =>
        {
            try
            {
                try
                {
                    call();
                }
                catch (Exception ex)
                {
                    thrown = ex;
                }

                after = runtime_package.GoCurrentGStatus();
                depthAfter = GoNativeCall.TransitionDepth;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(30)), $"the goroutine did not finish {name}");
        Assert.IsInstanceOfType(thrown, typeof(ArgumentException), $"{name} must surface golib's own refusal, got {thrown}");
        StringAssert.Contains(thrown!.Message, message, $"{name}: the surfacing exception is the original one");
        Assert.AreEqual(Grunning, after, $"after {name} throws, the goroutine is _Grunning again (the exit ran)");
        Assert.AreEqual(0, depthAfter, $"and the transition {name} opened is closed");
    }
}
