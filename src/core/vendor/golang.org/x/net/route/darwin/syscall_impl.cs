// syscall_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// route.sysctl, hand-owned: the one darwin //go:linkname pull of syscall the converter's forward
// registry cannot carry (S7b, 2026-10-02).
//
// Go's route/syscall.go pulls `//go:linkname sysctl syscall.sysctl` over a bodyless
// `func sysctl(mib []int32, old *byte, oldlen *uintptr, new *byte, newlen uintptr) error`, and
// syscall authorizes it (linkname_bsd.go). The other seven darwin pulls of syscall are forward rows
// (linknameForwardTargets in src/go2cs/visitFuncDecl.go). This one cannot be: syscall's sysctl takes
// `mib []_C_int`, and `_C_int` is syscall's UNEXPORTED `type _C_int int32`, so widening the target
// public is CS0051 in syscall's own build -- measured with the row present, and refused since by
// TestLinknameForwardTargetsExposeNoUnexportedTypes. Left unfilled, PartialStubGenerator's throw
// reached every net.Interfaces / net.InterfaceAddrs caller on darwin: route.FetchRIB is their only
// source, and IpAdapterAddresses died on it on both mac legs (run 36959682011).
//
// So route calls libc sysctl(3) itself. No pointer into managed memory crosses the boundary, the
// readdir_r precedent (os/darwin/dir_darwin_impl.cs): ONE unmanaged block per call holds the mib
// words, the oldlen slot and the old/new buffers. mib and new are copied IN (both are input-only,
// so a copy is exact and needs no element-type reinterpretation); the kernel's oldlen and the
// first oldlen bytes of old are copied OUT through the caller's own pointers. old is FetchRIB's
// `&b[0]` with b of length *oldlen, so unsafe.Slice(old, *oldlen) is exactly the caller's buffer.
// The block is freed in the finally, so nothing native outlives the call.
//
// The error is Go's: on -1, the errno sysctl(3) set, as a syscall.Errno -- FetchRIB compares it
// against syscall.ENOMEM to retry a table that grew between its two calls.

[module: go.GoManualConversion]

namespace go.vendor.golang.org.x.net;

using System.Runtime.InteropServices;
using Interop = System.Runtime.InteropServices.Marshal; // route has its own Marshal (RouteMessage.Marshal)
using @unsafe = unsafe_package;
using syscall = syscall_package;

partial class route_package {

[DllImport("libc", EntryPoint = "sysctl", SetLastError = true)]
private static extern int sysctl_native(nint name, uint namelen, nint oldp, nint oldlenp, nint newp, nuint newlen);

internal static partial error sysctl(slice<int32> mib, ж<byte> old, ж<uintptr> oldlen, ж<byte> @new, uintptr newlen) {
    nint mibCount = len(mib);
    nuint oldCapacity = oldlen == nil ? 0 : (nuint)oldlen.Value;
    nuint newCount = newlen;

    // Layout: [mib words][oldlen slot][old buffer][new buffer], each region pointer-aligned.
    nint mibBytes = mibCount * sizeof(int);
    nint oldlenOffset = (mibBytes + nint.Size - 1) & ~(nint.Size - 1);
    nint oldOffset = oldlenOffset + nint.Size;
    nint newOffset = oldOffset + (nint)((oldCapacity + (nuint)nint.Size - 1) & ~((nuint)nint.Size - 1));
    nint total = newOffset + (nint)newCount;

    nint block = Interop.AllocHGlobal(total == 0 ? 1 : total);

    try {
        for (nint i = 0; i < mibCount; i++) {
            Interop.WriteInt32(block, (int)(i * sizeof(int)), mib[i]);
        }

        Interop.WriteIntPtr(block, (int)oldlenOffset, (nint)oldCapacity);

        if (@new != nil && newCount > 0) {
            var source = @unsafe.Slice(@new, newlen).ToSpan();

            for (int i = 0; i < source.Length; i++) {
                Interop.WriteByte(block, (int)newOffset + i, source[i]);
            }
        }

        int rc = sysctl_native(block, (uint)mibCount,
            old == nil ? 0 : block + oldOffset,
            oldlen == nil ? 0 : block + oldlenOffset,
            @new == nil ? 0 : block + newOffset,
            newCount);

        if (rc == -1) {
            return (syscall.Errno)(uintptr)(nuint)Interop.GetLastPInvokeError();
        }

        nuint written = (nuint)Interop.ReadIntPtr(block, (int)oldlenOffset);

        if (old != nil && oldCapacity > 0) {
            var target = @unsafe.Slice(old, (uintptr)oldCapacity).ToSpan();
            int count = (int)(written < oldCapacity ? written : oldCapacity);

            for (int i = 0; i < count; i++) {
                target[i] = Interop.ReadByte(block, (int)oldOffset + i);
            }
        }

        if (oldlen != nil) {
            oldlen.Value = written;
        }

        return default!;
    }
    finally {
        Interop.FreeHGlobal(block);
    }
}

} // end route_package
