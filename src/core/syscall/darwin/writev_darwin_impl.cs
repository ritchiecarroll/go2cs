// writev_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// writev's iovec ARRAY on darwin -- the linux seam of 2026-09-02 (syscall/linux/sockaddr_linux_impl.cs
// GoWritevNative, internal/poll/linux/fd_writev_unix.cs) met on darwin's libc path, 2026-10-25.
//
// The generated wrapper (zsyscall_darwin_amd64.go) hands libc `unsafe.Pointer(&iovecs[0])`. The
// converted `Iovec` holds its `Base` as a `ж<byte>`, an OBJECT REFERENCE, so the element is
// reference-bearing and its number is an ORDER TOKEN, not an address. darwin's libc funnel
// (GoLibcCall.Call -> NativeStructMarshal.CallLibc) marshals a token's pointee only while every
// pointer field in it is nil (the 2026-09-27 ruling, option (b)); a writev vector's bases never are,
// so the token reached the kernel and its copyin answered EFAULT: net's TestBuffers_WriteTo and all
// eight subtests read "writev: bad address" on both mac legs, and the peer read "". Behind that, the
// funnel copies ONE box per token, so a marshalled vector would have carried element 0 of n.
//
// The remedy is the linux one, kept to a single displacement because darwin's internal/poll writev
// is already a //go:linkname forward to this function (fd_writev_libc.go): the native iovec array is
// built here from NativeIovec (sockaddr_darwin_impl.cs, the msghdr family's mirror), each base is
// the PINNED address of the caller's own buffer -- `(uintptr)` on a reference-free box pins it for
// the box's reachability and returns the address, the rule GoRecvmsgNative already stands on -- and
// one libc writev runs over it through the same trampoline the generated wrapper used. No copy in,
// no copy out: the bytes the kernel reads are the caller's.
//
// Registered in the converter's manualConversionFuncs ("syscall": writev -> goosDarwin); the body
// this replaces is the generated wrapper's, and the Go semantics are its own: one call, no EINTR
// loop (internal/poll owns that), errno through errnoErr.

using System;
using System.Runtime.InteropServices;

// The aliases the converted neighbours declare for themselves -- a converted file's aliases are
// file-scoped, so a hand-owned companion restates the ones it uses.
using abi = go.@internal.abi_package;

// Hand-owned (no writev_darwin_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]
// The native vector is a stack or unmanaged buffer of raw pointers.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // Go caps a vector at IOV_MAX (1024, internal/poll's maxVec), so the image is a stack buffer for
    // the ordinary handful and an unmanaged one only past this threshold, as on linux.
    private const int writevStackVector = 32;

    public static unsafe (uintptr cnt, error err) writev(nint fd, slice<Iovec> iovecs) {
        nint count = len(iovecs);

        NativeIovec* heap = null;
        NativeIovec* stack = stackalloc NativeIovec[count <= writevStackVector ? (int)count : 0];
        NativeIovec* native = stack;

        if (count > writevStackVector) {
            heap = (NativeIovec*)NativeMemory.Alloc((nuint)count, (nuint)sizeof(NativeIovec));
            native = heap;
        }

        try {
            for (nint i = 0; i < count; i++) {
                // A nil base crosses as 0, which is what libc reads for a nil pointer. Any other
                // base is the pinned address of the caller's buffer for as long as its box is
                // reachable, which is why the slice is kept alive across the call below.
                native[i].Base = (byte*)(nint)(uintptr)iovecs[i].Base;
                native[i].Len = (nuint)iovecs[i].Len;
            }

            var (r0, _, e1) = syscallX(abi.FuncPCABI0(libc_writev_trampoline), (uintptr)fd, (uintptr)(void*)native, (uintptr)count);
            // Every native[i].Base points into a buffer reached through iovecs[i].Base; keeping the
            // SLICE alive keeps those boxes, and so their pins, alive across the call.
            System.GC.KeepAlive(iovecs);

            uintptr cnt = (uintptr)r0;
            if (e1 != 0) {
                return (cnt, errnoErr(e1));
            }

            return (cnt, default!);
        } finally {
            if (heap != null) {
                NativeMemory.Free(heap);
            }
        }
    }
}
