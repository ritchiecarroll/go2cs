// os_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The bootstrap constants Go's linux osinit path sets and the managed runtime never reached
// (increment 7 of the runtime row, 2026-09-05). This file has no `<name>.go` counterpart, so a
// -stdlib reconvert never emits over it; the module marker states the ownership explicitly.
//
// WHY. Go sets physPageSize from the auxiliary vector (sysauxv, AT_PAGESZ, with an mincore probe as
// the fallback when the vector is absent) and physHugePageSize from
// /sys/kernel/mm/transparent_hugepage/hpage_pmd_size (getHugePageSize), both on the osinit path
// schedinit runs before any Go code. The managed host runs none of that: schedinit, osinit and
// sysargs are never called, so both fields sat at their zero values — and every page-allocator row
// died on `alignUp(n, physPageSize)` answering 0, then `mmap(0 bytes)` answering EINVAL, then
// `failed to reserve page summary memory` (the increment-6 readings). The converted getHugePageSize
// cannot run either: it reads the sysfs file through the runtime's raw `open`/`read`/`closefd`,
// which are assembly stubs that throw.
//
// WHAT THIS SETS, per flavour (increment 7 is linux; windows and darwin have their own osinit and
// are not touched by this file):
//   physPageSize      — Environment.SystemPageSize, which is sysconf(_SC_PAGESIZE): the same value the
//                       kernel places in AT_PAGESZ, so Go's primary source, never the mincore fallback.
//   physHugePageSize  — the sysfs file, parsed exactly as getHugePageSize parses it (leading decimal
//                       digits; a non-power-of-two answers 0; an unreadable file answers 0). 0 is
//                       Go's own value when the file cannot be read, and mallocinit accepts it.
// WHAT STAYS ZERO / UNTOUCHED (the rest of osinit and sysargs): ncpu is already Environment.ProcessorCount
// in the converted runtime2.cs; startupRand (AT_RANDOM), secureMode (AT_SECURE), the auxv copy,
// archauxv/vdsoauxv (the vDSO tables), osArchInit (a no-op on linux/amd64) and the signal-mask
// state are not set — none of them has a reaching consumer on the rows this increment measures,
// and each is named here so the next reader finds the boundary rather than rediscovering it.
//
// The [ModuleInitializer] runs before Main and forces runtime_package's type initializer, exactly as
// goenvs_impl.cs does for envs; the fields have no initializers of their own to overwrite.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using @unsafe = go.unsafe_package;

using System.Threading;
using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    private const string sysTHPSizeFile = "/sys/kernel/mm/transparent_hugepage/hpage_pmd_size";

    [ModuleInitializer]
    internal static void ᴛInitBootstrapConstants()
    {
        physPageSize = (uintptr)(nuint)Environment.SystemPageSize;
        physHugePageSize = readTransparentHugePageSize();
    }

    // getHugePageSize's parse, over the file the managed host CAN read: the leading decimal digits
    // (Go stops at the first non-digit of a 20-byte read), a negative or non-power-of-two value
    // answers 0, an unreadable file answers 0.
    private static uintptr readTransparentHugePageSize()
    {
        string text;

        try
        {
            text = File.ReadAllText(sysTHPSizeFile);
        }
        catch (Exception)
        {
            return 0;
        }

        return parseHugePageSize(text);
    }

    internal static uintptr parseHugePageSize(string text)
    {
        long v = 0;
        int i = 0;

        for (; i < text.Length && i < 20 && text[i] >= '0' && text[i] <= '9'; i++)
            v = v * 10 + (text[i] - '0');

        if (i == 0 || v < 0)
            return 0;

        if ((v & (v - 1)) != 0)
            return 0;

        return (uintptr)(nuint)v;
    }

    /// <summary>The two bootstrap constants as the runtime holds them, for the standing guard.</summary>
    public static (nuint physPageSize, nuint physHugePageSize) GoBootstrapConstants()
    {
        return ((nuint)physPageSize, (nuint)physHugePageSize);
    }

    /// <summary>getHugePageSize's parse over caller-supplied text, for the guard's negative arms.</summary>
    public static nuint GoParseHugePageSize(string text)
    {
        return (nuint)parseHugePageSize(text);
    }

    // The linux thread primitives, which Go implements in assembly (sys_linux_amd64.s) and the host filled
    // with a NotImplementedException stub, recorded by the test host as an infrastructure-error that no
    // manifest entry can disclose (runtime's TestNewOSProc0 and TestSignalM).
    //   getpid answers the process id, which is exactly what it is.
    //   clone starts a function pointer on a new OS thread with a caller-supplied stack; a goroutine here is
    //   a CLR thread with no M and no g0 stack, so that has no managed meaning and refuses by name.
    //   tgkill signals ONE thread of the process; an M here has no kernel thread id of its own, so it
    //   refuses by name too. Each panic names the primitive and why, in the spelling the other refusals use.
    internal static partial nint getpid() => (nint)Environment.ProcessId;

    internal static partial int32 clone(int32 flags, @unsafe.Pointer stk, @unsafe.Pointer mp, @unsafe.Pointer gp, @unsafe.Pointer fn) =>
        throw panic("runtime: clone: goroutines are CLR threads, so a raw clone syscall that starts a function pointer on a new OS thread has no managed meaning");

    internal static partial void tgkill(nint tgid, nint tid, nint sig) =>
        throw panic("runtime: tgkill: an M here is a CLR thread with no kernel thread id of its own, so a signal cannot be sent to one thread of the process");

    // Three probes for the GolibTests seam over the linux thread primitives below (GolibTests is outside the
    // InternalsVisibleTo grant). Linux-only by construction, like the file; the test finds them by reflection.
    public static int32 GoCloneProbe() => clone(0, nil, nil, nil, nil);

    public static nint GoGetpidProbe() => getpid();

    public static void GoTgkillProbe() => tgkill(0, 0, 0);

    // syscall.AllThreadsSyscall's runtime half (os_linux.go), REFUSED BY NAME BEFORE THE WORLD IS
    // STOPPED (ruling 2026-09-28 02:10, Q6). Go stops the world and then runs the system call on
    // every M, signalling each thread to run it too. There are no Ms to signal here, and under the
    // stop-the-world contract the stop now succeeds, so the converted body would run the call on the
    // calling thread alone and report success: a credential change that reached one thread. Go
    // refuses the same way under cgo, where it cannot see every thread either. The credential
    // setters do not come through here (syscall/linux/cgocaller_linux_impl.cs), and
    // AllThreadsSyscall answers ENOTSUP before it would, as a cgo build's does.
    internal static (uintptr r1, uintptr r2, uintptr err) syscall_runtime_doAllThreadsSyscall(uintptr trap, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) =>
        throw new PanicException("runtime: doAllThreadsSyscall: the managed host cannot run a system call on every thread (goroutines are CLR threads with no Ms to signal), so it is refused before the world is stopped");

    /// <summary>
    /// syscall.AllThreadsSyscall's runtime half, syscall_runtime_doAllThreadsSyscall, called the way
    /// Setuid/Setgid reach it (the trap is getpid; amd64's number, and it is never executed: the
    /// call refuses before it could run). Returns what it raised by type and message, or null if it
    /// returned, and whether worldsema was free afterwards: a plain acquire on another goroutine
    /// within the timeout.
    /// </summary>
    public static (string? failure, bool worldsemaFree) GoAllThreadsSyscallProbe(int timeoutMs)
    {
        string? failure = null;

        try
        {
            syscall_runtime_doAllThreadsSyscall((uintptr)39, 0, 0, 0, 0, 0, 0);
        }
        catch (Exception ex)
        {
            failure = $"{ex.GetType().Name}: {ex.Message}";
        }

        ManualResetEventSlim acquired = new(false);

        Goroutine.Start(() =>
        {
            semacquire(Ꮡworldsema);
            acquired.Set();
            semrelease(Ꮡworldsema);
        });

        // The event is not disposed: on a leak the parked goroutine still holds it.
        return (failure, acquired.Wait(timeoutMs));
    }
}
