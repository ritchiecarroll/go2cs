// sigprocmask_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// runtime.rtsigprocmask — the linux flavour's signal-mask syscall, realized over the kernel's own
// rt_sigprocmask(2) through libc's syscall(2), with Go's set as the kernel sees it.
//
// Go implements this in assembly (sys_linux_amd64.s: `rt_sigprocmask` → SYS_rt_sigprocmask with
// an 8-byte set, crashing the process on failure), so the converted declaration in os_linux.cs is a
// bodyless partial and PartialStubGenerator fills it with a throw. That throw was the runtime row's
// INIT door on Linux (the 2026-09-04 first-contact census): crash_unix_test.go's `//go:build unix`
// init() asks `Sigisblocked(SIGQUIT)`, whose only syscall is this one, and the throw in the test
// package's static constructor shadowed 436 of 436 tests before the first ran.
//
// SCOPE — exactly one declaration. This file bodies `rtsigprocmask` and nothing else: the other
// 41 bodyless partials in runtime/linux (rt_sigaction, sigaltstack, sigtramp, raise, futex, clone,
// mincore, madvise, getg, open, …) are NOT it and keep their generated stubs; `getg` in particular
// is its own item (Q40), and rt_sigaction's install layer is signal_posix_impl.cs's bridge over
// PosixSignalRegistration, which deliberately ELIDES the ensureSigM goroutine that was
// rt_sigprocmask's other caller. What remains reachable through this body today is the mask
// READ (`_SIG_SETMASK` with a nil new set) and per-thread block/unblock of ordinary signals; the
// scheduler-side callers (minit's minitSignalMask, newosproc's block-around-clone, sigblock/
// unblocksig on the cgo path) are not reached because the converted scheduler does not run them.
//
// Shape: the same raw-syscall-through-libc form the syscall package's posix_spawn seam uses.
// libc's syscall(2) returns -1 and SETS errno — it never returns -errno — so the errno is read
// back through SetLastError. Go's asm crashes on failure (MOVL $0xf1, 0xf1); the managed
// equivalent is a loud exception naming the errno, never a silent zero mask. The set crosses the
// boundary as the kernel's 8 bytes: Go's sigset is `[2]uint32`, low word first, and the kernel's
// sigsetsize argument is what Go passes (8); glibc's 128-byte sigset_t is not involved, so the two
// internal signals glibc refuses to block (SIGCANCEL/SIGSETXID) are handled exactly as Go handles
// them — by the kernel. A nil box on either side is a NULL pointer, as in Go.
//
// The public `GoSigprocmask` helper is the guard's door (GolibTests/LinuxSignalMaskTests.cs) and
// the pattern the tree already uses for a seam consumers may drive — a Go-prefixed PUBLIC helper
// per operation, the native mirror private to the seam file. It goes through the converted
// `sigprocmask` wrapper so the guard exercises this body, not a copy of it.
//
// Hand-owned (no sigprocmask_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using System;
using System.Runtime.InteropServices;
using go.golib;

partial class runtime_package {

// linux/amd64 syscall number; the seam asserts the architecture elsewhere (sys_linux_amd64 is
// the only linux flavour the corpus emits).
private const long SYS_rt_sigprocmask = 14;

[DllImport("libc", EntryPoint = "syscall", SetLastError = true)]
private static extern long sys_rt_sigprocmask(long number, long how, IntPtr set, IntPtr oldset, long sigsetsize);

internal static partial void rtsigprocmask(int32 how, ж<sigset> @new, ж<sigset> old, int32 size) {
    bool hasNew = @new is not null && !@new.IsNilPointer;
    bool hasOld = old is not null && !old.IsNilPointer;

    // Two 8-byte kernel sets, native so the kernel reads exactly what Go's asm would hand it.
    IntPtr buffer = Marshal.AllocHGlobal(16);
    try {
        Marshal.WriteInt64(buffer, 0, 0L);
        Marshal.WriteInt64(buffer, 8, 0L);
        if (hasNew) {
            ref sigset set = ref @new.Value;
            Marshal.WriteInt64(buffer, 0, unchecked((long)((ulong)set[0] | ((ulong)set[1] << 32))));
        }

        long rc = sys_rt_sigprocmask(SYS_rt_sigprocmask, how,
            hasNew ? buffer : IntPtr.Zero,
            hasOld ? buffer + 8 : IntPtr.Zero,
            size);

        if (rc < 0) {
            int errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException($"runtime.rtsigprocmask: rt_sigprocmask(how={how}, sigsetsize={size}) failed with errno {errno}");
        }

        if (hasOld) {
            ulong bits = unchecked((ulong)Marshal.ReadInt64(buffer, 8));
            ref sigset set = ref old.Value;
            set[0] = (uint32)bits;
            set[1] = (uint32)(bits >> 32);
        }
    }
    finally {
        Marshal.FreeHGlobal(buffer);
    }
}

// GoSigprocmask drives the converted sigprocmask wrapper (and so this file's body) from outside the
// package: `how` is Go's _SIG_BLOCK/_SIG_UNBLOCK/_SIG_SETMASK (0/1/2, the kernel's numbering), a
// null newMask is Go's nil set (a pure read), and the return value is the mask the kernel held
// BEFORE the call, as Go's `old` out-parameter reports it — bit (n-1) for signal n.
public static ulong GoSigprocmask(int how, ulong? newMask) {
    ж<sigset> Ꮡnew = newMask is ulong bits
        ? new StandardBox<sigset>(new sigset(new uint32[]{ (uint32)bits, (uint32)(bits >> 32) }.array()))
        : ж<sigset>.NilBoxOfDims(2L);
    ж<sigset> Ꮡold = new StandardBox<sigset>(new sigset(new uint32[2].array()));

    sigprocmask((int32)how, Ꮡnew, Ꮡold);

    ref sigset result = ref Ꮡold.Value;
    return (ulong)result[0] | ((ulong)result[1] << 32);
}

} // end runtime_package
