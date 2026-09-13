// zsyscall_windows_ptrout_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// This package's member of the ptrout class -- the generated wrappers that take a Go `**T`
// OUT-PARAMETER, a slot the caller owns and the KERNEL writes a raw address into.
//
// The class, its failure mode and its remedy are documented once, in
// syscall/windows/zsyscall_windows_ptrout_impl.cs; that file is the reference and this one does not
// restate it. The short form: a `ж<ж<T>>` is a managed box whose storage is an OBJECT REFERENCE, so
// it has no eight-byte slot to lend. While the held pointer is nil the `ж<T> -> uintptr` operator
// answers 0, Windows reads that as "no output wanted", the call SUCCEEDS, and the caller reads back
// the nil it started with. The remedy is a native cell local to the call and a publish afterwards.
//
// NetUserGetLocalGroups is the netapi32 member os/user reaches, and like syscall's NetUserGetInfo it
// is NOT of the publish-the-address-and-stop family: its buffer is an ARRAY of
// LocalGroupUserInfo0, whose single field is a `ж<uint16>` -- a managed reference. os/user's
// listGroupsForUsernameAndDomain does not even use Reinterpret; it builds a
// `ReadOnlySpan<LocalGroupUserInfo0>` directly over the native buffer, which is a THIRD route to the
// same fabrication (a Span<T> over native memory where T carries a managed reference). So this body
// ships with the transcription in os/user's lookup_windows_impl.cs, never on its own: publishing a
// real address while that call site stayed as-is would upgrade today's contained nil into a
// fabricated managed reference, which is a CLR type-safety break.
//
// THE FOUR ORDINARY POINTER ARGUMENTS carry the reference file's other closure, for the same reason
// it gives: golib pins managed storage for the BOX's lifetime only (`m_pin`, a GCHandle owned by
// that one box), and a hand-own gets none of `convSyscallFunnelCall`'s `var ᴋN`/`GC.KeepAlive`
// emission because the converter drops a `[module: go.GoManualConversion]` file from the convert
// set. `serverName`/`userName` are UTF16PtrFromString element references into managed
// `slice<uint16>`s; `entriesRead`/`totalEntries` are `heap(new uint32(), ...)` boxes the KERNEL
// WRITES, and a domain query can block for as long as a DC takes to answer. The out-cell needs no
// such holder -- it is a native stack local.

using System;

[module: go.GoManualConversion]

// The native out-cell and its address are pointer work.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using syscall = go.syscall_package;

partial class windows_package
{
    // Publishes the raw address the kernel wrote into a native out-cell back into the caller's Go
    // pointer. ValueSlot rather than Value for the reason the reference file gives: the caller's box
    // legitimately holds null on entry -- that is what an out-parameter IS -- and Value's
    // nil-pointer guard value-peeks, so it would panic on the very write that fills the slot in. A
    // zero address publishes the nil pointer with no special case, since ж<T>'s native constructor
    // already treats zero as nil.
    private static void publishPointerOut<T>(ж<ж<T>> slot, nuint written)
    {
        if (slot != nil)
        {
            slot.ValueSlot = (ж<T>)(uintptr)written;
        }
    }

    // NetUserGetLocalGroups(serverName, userName, level, flags, &buf, prefMaxLen, &entriesRead,
    // &totalEntries). Reports failure through the RETURN VALUE rather than GetLastError -- the
    // generated body's own reading, kept verbatim -- and the buffer is freed with NetApiBufferFree.
    //
    // Only `buf` is an out-POINTER; entriesRead and totalEntries are ordinary `*uint32` out-params
    // whose managed boxes carry blittable storage, so they pass through unchanged.
    public static unsafe error /*neterr*/ NetUserGetLocalGroups(ж<uint16> ᏑserverName, ж<uint16> ᏑuserName, uint32 level, uint32 flags, ж<ж<byte>> Ꮡbuf, uint32 prefMaxLen, ж<uint32> ᏑentriesRead, ж<uint32> ᏑtotalEntries) {
        nuint cell = 0;
        uintptr cellAddr = Ꮡbuf == nil ? 0 : (uintptr)(void*)(&cell);

        var (r0, _, _) = syscall.Syscall9(procNetUserGetLocalGroups.Addr(), 8, (uintptr)ᏑserverName, (uintptr)ᏑuserName, (uintptr)level, (uintptr)flags, cellAddr, (uintptr)prefMaxLen, (uintptr)ᏑentriesRead, (uintptr)ᏑtotalEntries, 0);

        System.GC.KeepAlive(ᏑserverName);
        System.GC.KeepAlive(ᏑuserName);
        System.GC.KeepAlive(ᏑentriesRead);
        System.GC.KeepAlive(ᏑtotalEntries);

        if (r0 != 0) {
            return ((syscall.Errno)r0);
        }

        publishPointerOut(Ꮡbuf, cell);

        return default!;
    }
}
