// sigprocmask_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The darwin run layer's increment 5 (2026-09-04): runtime.sigprocmask, the darwin flavour's
// signal-mask primitive, realized over the same libc entry point Go's own trampoline calls.
//
// WHY, measured rather than assumed. The train-24 behavioral-stderr stage (run 33914945822 at
// 8f82b3f63) placed the SignalPrimitives death on BOTH mac legs at the same statement -- the third
// of six, signal.Notify -- by two independent derivations: osx-x64's stack
//
//     panic: FuncPCABI0: no program counter exists for runtime.sigprocmask_trampoline
//     internal/abi.FuncPC() <- internal/abi.FuncPCABI0() <- runtime.sigprocmask
//       (sys_darwin.go:413) <- runtime.ensureSigM.func1 (signal_unix.go:1075)
//
// and osx-arm64's stdout count (2 of main.go's 6 lines, with ZERO stderr -- exit 138). The death is
// in COMPUTING the trampoline's address, not in dispatching through it: sigprocmask_trampoline is a
// bodyless partial the PartialStubGenerator fills with a throw, so abi.FuncPCABI0 has no PC to
// answer with. Bodying sigprocmask removes the FuncPCABI0 call from the path entirely.
//
// WHAT THE DARWIN FLAVOUR IS, and every clause here differs from the linux arm C1 landed the same
// day (runtime/linux/sigprocmask_impl.cs, runtime Linux increment 1) -- this is NOT that body with
// the platform word changed:
//
//   * The libc entry point is pthread_sigmask, NOT sigprocmask. Go's own trampoline calls
//     libc_pthread_sigmask on amd64 AND arm64 (sys_darwin_amd64.s:135, sys_darwin_arm64.s:248, at
//     the corpus's pinned go1.23.12), which is the only correct choice: the runtime's masks are
//     PER-THREAD, and POSIX leaves process-wide sigprocmask undefined in a threaded process.
//   * The set is 32 BITS. Go's darwin sigset is `type sigset uint32` (os_darwin.go:376) and
//     darwin's C sigset_t is __uint32_t, so the two agree exactly and the marshal is four bytes.
//     Linux's set is [2]uint32 = 8 bytes with a sigsetsize argument; there is none here.
//   * The `how` numbering is DIFFERENT: darwin is _SIG_BLOCK 1 / _SIG_UNBLOCK 2 / _SIG_SETMASK 3
//     (os_darwin.cs:394-396) against Linux's 0 / 1 / 2. A borrowed body would mis-name every call.
//   * pthread_sigmask RETURNS its error number and does NOT set errno (POSIX), so the failure is
//     read from the return value -- not through SetLastError, which is how the linux arm's
//     syscall(2) form reports. Reading errno here would report a stale, unrelated value.
//
// Go's trampoline CRASHES the process on a nonzero return (MOVL $0xf1, 0xf1). The managed
// equivalent is a loud exception naming the errno, never a silent no-op mask -- the same choice the
// linux arm made, for the same reason: a signal mask that quietly did not change is worse than a
// stopped process.
//
// SCOPE -- exactly one declaration. This file bodies `sigprocmask` and nothing else. The other
// bodyless trampolines reached from the same file (sigaction, sigaltstack, sigtramp, raise,
// pthread_kill, kqueue, …) are NOT it and keep their generated stubs; in particular
// runtime.sigaction is the install side and a separate item -- bodied by increment 6
// (sigaction_impl.cs, 2026-09-05) -- and clearing this door does not clear
// the SignalPrimitives row -- signal.Notify is the third of SIX statements and three calls sit
// behind it.
//
// Registered as `"runtime": {"sigprocmask": goosDarwin}` in manualTypeOperations.go: the converted
// sigprocmask is BODIED (it calls libcCall), and a bodied function is displaced only through that
// registry -- the same door increment 4's pipe/read/write1 went through, not the bodyless-partial
// displacement C1's linux arm could use.
//
// Hand-owned (no sigprocmask_impl.go exists, so a reconvert never regenerates this file).

using System;
using System.Runtime.InteropServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // pthread_sigmask(3): returns 0 on success and an errno on failure; it does NOT set errno.
    // SetLastError is therefore deliberately absent -- reading it here would report an unrelated
    // stale value and turn a real failure into a misleading one.
    [DllImport("libc", EntryPoint = "pthread_sigmask")]
    private static extern int sigmask_pthread_sigmask(int how, nint set, nint oset);

    // runtime.sigprocmask -- Go: `func sigprocmask(how uint32, new *sigset, old *sigset)`.
    // A nil box on either side is a NULL pointer, exactly as in Go: a nil `new` is a pure read of
    // the current mask, and a nil `old` discards the previous one.
    internal static void sigprocmask(uint32 howʗp, ж<sigset> Ꮡnew, ж<sigset> Ꮡold)
    {
        bool hasNew = Ꮡnew is not null && !Ꮡnew.IsNilPointer;
        bool hasOld = Ꮡold is not null && !Ꮡold.IsNilPointer;

        // Two 4-byte native sets, so the kernel reads exactly the width Go's asm would hand it.
        nint buffer = Marshal.AllocHGlobal(8);

        try
        {
            Marshal.WriteInt32(buffer, 0, 0);
            Marshal.WriteInt32(buffer, 4, 0);

            if (hasNew)
            {
                Marshal.WriteInt32(buffer, 0, unchecked((int)(uint32)Ꮡnew.Value));
            }

            int rc = sigmask_pthread_sigmask((int)howʗp,
                hasNew ? buffer : 0,
                hasOld ? buffer + 4 : 0);

            if (rc != 0)
            {
                throw new InvalidOperationException($"runtime.sigprocmask: pthread_sigmask(how={howʗp}) failed with errno {rc}");
            }

            if (hasOld)
            {
                Ꮡold.Value = (sigset)unchecked((uint32)Marshal.ReadInt32(buffer, 4));
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // GoSigprocmask drives this body from outside the package -- the Go-prefixed PUBLIC helper per
    // operation the tree uses for a seam consumers may drive, with the native mirror private to this
    // file. `how` is Go's darwin numbering (_SIG_BLOCK 1 / _SIG_UNBLOCK 2 / _SIG_SETMASK 3, NOT
    // Linux's 0/1/2), a null newMask is Go's nil set (a pure read), and the return value is the mask
    // the kernel held BEFORE the call -- bit (n-1) for signal n, as Go's `old` out-parameter reports.
    public static uint32 GoSigprocmask(int how, uint32? newMask)
    {
        ж<sigset> Ꮡnew = newMask is uint32 bits
            ? new StandardBox<sigset>((sigset)bits)
            : ж<sigset>.NilBox;
        ж<sigset> Ꮡold = new StandardBox<sigset>((sigset)0);

        sigprocmask((uint32)how, Ꮡnew, Ꮡold);

        return (uint32)Ꮡold.Value;
    }
}
