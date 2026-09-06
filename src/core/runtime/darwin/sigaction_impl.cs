// sigaction_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The darwin run layer's increment 6 (2026-09-05, Q41): runtime.sigaction, the darwin flavour's
// signal INSTALL/QUERY primitive, realized over libc's sigaction(2) -- the entry point Go's own
// trampoline calls -- with `new` ENCODED into a native 16-byte struct sigaction and `old` DECODED
// back into the managed fields, so libc never sees a managed address or a stale register.
//
// WHY, measured rather than assumed. The train-26 crash report (run 33946376666 at 8241ace970,
// SignalPrimitives on osx-arm64, exit 138 with ZERO stderr after 2 of main.go's 6 lines) placed
// the death INSIDE the CLR's own stack walk -- Frame::GetFunction under StackFrameIterator under
// the panic report's StackTrace capture -- reading a Frame link of 0x0000004200000000, which is
// { sa_mask 0, sa_flags 0x42 = SA_SIGINFO|SA_RESTART }: the second eight bytes of a sigaction(2)
// READ-BACK of a handler the CLR's PAL had installed. How that read-back reached the managed
// stack is read from the code, not guessed:
//
//   * Go's sigaction is `//go:cgo_unsafe_args` and hands its trampoline `&sig`, the head of a
//     contiguous (sig, new, old) block that exists only on a Go stack; the trampoline unpacks the
//     block by offset (sys_darwin_amd64.s:122-124, sys_darwin_arm64.s:255-257: 8(DI) new, 16(DI)
//     old, 0(DI) sig).
//   * The converted form (sys_darwin.cs) dispatches libcCall(fn, FromPinnedBox(Ꮡsig)) -- a box of
//     the FIRST parameter alone. GoLibcCall.DispatchArgsStruct places that uint32's one field in
//     the first register, and GoLibcCall.Call's one-argument arm leaves the second and third
//     registers holding whatever the caller-saved registers held. libc's sigaction then reads
//     `new` from and writes `old` through STALE REGISTERS.
//   * On arm64 the third register pointed into the managed stack where the unmanaged call's own
//     frame link lives, and the 16-byte read-back overwrote it; the next stack walk -- the panic
//     report for the FuncPCABI0(sigtramp) door one line later -- dereferenced {0, 0x42} and died
//     mute. On x64 the same dispatch survived because its stale register pointed elsewhere.
//
// The usigactiont box's address and bytes were never handed to libc at all -- which CORRECTS the
// frames reading that placed the write through the box's interior. The remedy is the same
// either way: marshal at the seam.
//
// WHAT THE BODY DOES. Both managed boxes become one 32-byte native buffer: bytes 0-15 the `new`
// image, 16-31 the `old` image, each darwin's user-visible struct sigaction
// { union __sigaction_u (8) | sigset_t sa_mask (4) | int sa_flags (4) } -- 16 bytes on amd64 AND
// arm64 (defs_darwin_amd64.go:128, defs_darwin_arm64.go:130 at the pinned go1.23.12), so one
// marshal serves both mac legs. Go's usigactiont carries the handler as `__sigaction_u [8]byte`,
// which its callers read and write as `*(*uintptr)(unsafe.Pointer(&sa.__sigaction_u))`
// (os_darwin.go:393/410/414/423); the mirror moves those eight bytes as one little-endian word.
// The converted struct cannot be passed by address in any form: its [8]byte union converts to an
// array<byte> carrying a managed reference, so the box has no pinnable storage and the CLR lays
// the struct out however it likes (the struct-passing class). A nil box on either side is a NULL
// pointer, exactly as in Go: a nil `new` is a pure query (getsig), a nil `old` discards the
// previous action (setsig).
//
// FAILURE. Go's trampoline crashes the process on a nonzero return (sys_darwin_amd64.s:126-128
// `MOVL $0xf1, 0xf1`, sys_darwin_arm64.s:259-261 `BL notok<>`). sigaction(2) returns -1 and SETS
// errno -- the opposite of increment 5's pthread_sigmask, which RETURNS it -- so the DllImport
// declares SetLastError and the exception names the errno the call left. Loud, never a silently
// uninstalled handler.
//
// WHAT THIS DOES NOT CLEAR. The SignalPrimitives row still dies at the NEXT line of the same
// path: setsig (os_darwin.go:390) computes abi.FuncPCABI0(sigtramp) for the handler to install,
// and sigtramp is assembly with no managed body -- Q52's design, signal delivery INTO managed
// code, whose handler this seam will be the place to encode. At this row the `new` arm is
// exercised by signal.Ignore's SIG_IGN install (statement 2 of six: sigignore -> setsig(sig,
// _SIG_IGN), signal_unix.go:256) and the `old` arm by signal.Notify's getsig (sigenable,
// signal_unix.go:205), both BEFORE the door; what the increment buys is that the door is reached
// with an intact stack and REPORTED on arm64 as it already is on x64.
//
// SCOPE -- exactly one declaration. This file bodies `sigaction` and nothing else. The other
// `&first-parameter` libcCall sites in sys_darwin.cs with parameters behind the first -- setitimer,
// kevent, pthread_kill, syscall_syscall9 -- carried the same stale-register shape until the block
// lift (Q56, 2026-09-05, DESIGN-cgo-unsafe-args-block-lift.md): their generated bodies now construct
// the whole parameter block and the dispatcher places every field. This file stays the seam for
// sigaction because its pointees are reference-bearing (usigactiont holds a managed handler word)
// and have no address the block could carry -- the lift hands libc an order token for such a
// pointee and it answers EFAULT; the native mirror here is that class's remedy, one site at a time.
//
// Registered as `"runtime": {"sigaction": goosDarwin}` in manualTypeOperations.go: the converted
// sigaction is BODIED (it calls libcCall), and a bodied function is displaced only through that
// registry -- increment 5's door, not the bodyless-partial displacement.
//
// Hand-owned (no sigaction_impl.go exists, so a reconvert never regenerates this file).

using System;
using System.Runtime.InteropServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // sigaction(2): 0 on success, -1 on failure with errno SET -- read through SetLastError, the
    // opposite of increment 5's pthread_sigmask, which returns its errno and sets nothing.
    [DllImport("libc", EntryPoint = "sigaction", SetLastError = true)]
    private static extern int sigaction_libc(int sig, nint act, nint oact);

    // darwin's user-visible struct sigaction, 16 bytes on amd64 and arm64 alike:
    // { __sigaction_u (8) | sa_mask (4) | sa_flags (4) } -- the same three fields, in the same
    // order, as Go's usigactiont (defs_darwin_*.go).
    private const int NativeSigactionBytes = 16;
    private const int SigactionHandlerOffset = 0;
    private const int SigactionMaskOffset = 8;
    private const int SigactionFlagsOffset = 12;
    private const int SigactionHandlerBytes = 8;

    // runtime.sigaction -- Go: `func sigaction(sig uint32, new *usigactiont, old *usigactiont)`.
    // A nil box on either side is a NULL pointer, exactly as in Go.
    internal static void sigaction(uint32 sigʗp, ж<usigactiont> Ꮡnew, ж<usigactiont> Ꮡold)
    {
        bool hasNew = Ꮡnew is not null && !Ꮡnew.IsNilPointer;
        bool hasOld = Ꮡold is not null && !Ꮡold.IsNilPointer;

        // One allocation, two 16-byte regions: the `new` image at 0, the `old` image at 16.
        nint buffer = Marshal.AllocHGlobal(NativeSigactionBytes * 2);

        try
        {
            for (int i = 0; i < NativeSigactionBytes * 2; i += 8)
            {
                Marshal.WriteInt64(buffer, i, 0L);
            }

            if (hasNew)
            {
                ref usigactiont @new = ref Ꮡnew.Value;

                Marshal.WriteInt64(buffer, SigactionHandlerOffset, unchecked((long)handlerWordOf(@new.__sigaction_u)));
                Marshal.WriteInt32(buffer, SigactionMaskOffset, unchecked((int)(uint32)@new.sa_mask));
                Marshal.WriteInt32(buffer, SigactionFlagsOffset, (int32)@new.sa_flags);
            }

            int rc = sigaction_libc(unchecked((int)sigʗp),
                hasNew ? buffer : 0,
                hasOld ? buffer + NativeSigactionBytes : 0);

            if (rc != 0)
            {
                int errno = Marshal.GetLastPInvokeError();
                throw new InvalidOperationException($"runtime.sigaction: sigaction(sig={sigʗp}) failed with errno {errno}");
            }

            if (hasOld)
            {
                ref usigactiont old = ref Ꮡold.Value;

                storeHandlerWord(old.__sigaction_u, unchecked((ulong)Marshal.ReadInt64(buffer, NativeSigactionBytes + SigactionHandlerOffset)));
                old.sa_mask = (uint32)unchecked((uint)Marshal.ReadInt32(buffer, NativeSigactionBytes + SigactionMaskOffset));
                old.sa_flags = (int32)Marshal.ReadInt32(buffer, NativeSigactionBytes + SigactionFlagsOffset);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // The eight bytes of __sigaction_u as the little-endian word Go's callers read through
    // `*(*uintptr)(unsafe.Pointer(&sa.__sigaction_u))`. array<byte> is a window over a managed
    // byte[], so the helper reads the same storage the struct holds.
    private static ulong handlerWordOf(array<byte> u)
    {
        if (u.Length != SigactionHandlerBytes)
        {
            throw new InvalidOperationException($"runtime.sigaction: usigactiont.__sigaction_u holds {u.Length} bytes, expected {SigactionHandlerBytes}");
        }

        ulong word = 0;

        for (int i = 0; i < SigactionHandlerBytes; i++)
        {
            word |= (ulong)u[i] << (8 * i);
        }

        return word;
    }

    private static void storeHandlerWord(array<byte> u, ulong word)
    {
        if (u.Length != SigactionHandlerBytes)
        {
            throw new InvalidOperationException($"runtime.sigaction: usigactiont.__sigaction_u holds {u.Length} bytes, expected {SigactionHandlerBytes}");
        }

        for (int i = 0; i < SigactionHandlerBytes; i++)
        {
            u[i] = (byte)(word >> (8 * i));
        }
    }

    // GoSigactionQuery drives the `old` arm from outside the package -- the Go-prefixed PUBLIC
    // helper per operation the tree uses for a seam consumers may drive (GoSigprocmask is its
    // increment-5 sibling), with the native mirror private to this file. A pure query: NULL `new`,
    // exactly what getsig performs. Returns the handler word Go's callers read (SIG_DFL 0, SIG_IGN 1
    // or a trampoline address), the mask and the flags the kernel holds for `sig`.
    public static (ulong handler, uint32 mask, int32 flags) GoSigactionQuery(int sig)
    {
        ж<usigactiont> Ꮡold = new StandardBox<usigactiont>(new usigactiont());

        sigaction((uint32)sig, ж<usigactiont>.NilBox, Ꮡold);

        ref usigactiont old = ref Ꮡold.Value;
        return (handlerWordOf(old.__sigaction_u), old.sa_mask, old.sa_flags);
    }

    // GoSigactionInstall drives the `new` arm from outside the package: it installs, verbatim, the
    // triple a prior GoSigactionQuery read (handler word, mask, flags) -- which is how the darwin
    // signal bridge (signal_posix_darwin_impl.cs, increment 9) puts back the disposition its kernel
    // SIG_IGN displaced, .NET's own SA_SIGINFO handler included, a thing signal(2) cannot reinstall
    // with its flags. A NULL `old`, exactly what setsig performs.
    public static void GoSigactionInstall(int sig, ulong handler, uint32 mask, int32 flags)
    {
        ж<usigactiont> Ꮡnew = new StandardBox<usigactiont>(new usigactiont());

        ref usigactiont @new = ref Ꮡnew.Value;
        storeHandlerWord(@new.__sigaction_u, handler);
        @new.sa_mask = mask;
        @new.sa_flags = flags;

        sigaction((uint32)sig, Ꮡnew, ж<usigactiont>.NilBox);
    }
}
