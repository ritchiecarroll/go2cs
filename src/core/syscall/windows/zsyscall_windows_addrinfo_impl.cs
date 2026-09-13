// zsyscall_windows_addrinfo_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of the NAME-RESOLUTION pair -- GetAddrInfoW and FreeAddrInfoW -- the
// members of the syscall struct-passing class that `net`'s DNS path forces. Reached the first time
// any converted program resolves a name or a service: net.Dial -> resolveAddrList -> LookupPort.
//
// THE CLASS, one more time. The generated wrapper hands the kernel `uintptr(unsafe.Pointer(hints))`
// and `uintptr(unsafe.Pointer(result))` -- the addresses of MANAGED objects whose layout is not the
// native one. Native ADDRINFOW is 48 bytes of scalars and raw pointers; the converted AddrinfoW
// holds `Canonname ж<uint16>`, `Addr Pointer` and `Next ж<AddrinfoW>` as MANAGED REFERENCES, so its
// field offsets bear no relation to the native ones and the CLR is entitled to move it. Both
// directions are wrong: the hints Windows READS are garbage, and the `*ADDRINFOW` Windows WRITES
// lands in a reference slot -- an 8-byte native address written where the collector expects an
// object pointer. The observed shape is a process kill, `Fatal error. 0xC0000005` inside
// syscall.Syscall6, with crypto/tls's TestVerifyHostname as the first measured consumer.
//
// It is the same seam as GetTimeZoneInformation / win32finddata1 / WSAPROTOCOL_INFOW and takes the
// same remedy: a blittable [StructLayout(Sequential)] mirror, a call through the package's own
// LazyProc, and an explicit copy at the boundary. What is NEW here -- and is the whole reason this
// file is long -- is that the OUTPUT is a linked structure whose records point at OTHER native
// records, and copying the top level alone would leave the consumer reading exactly the memory this
// file exists to keep it away from.
//
// WHY A NATIVE CHAIN CANNOT SIMPLY BE HANDED BACK. net's lookupIP walks the result and reads the
// sockaddr through it:
//
//     addr := unsafe.Pointer(result.Addr)
//     a := (*syscall.RawSockaddrInet4)(addr).Addr           // lookup_windows.go
//
// Converted, that is `((ж<RawSockaddrInet4>)(uintptr)(addr)).Value.Addr` -- and RawSockaddrInet4's
// `Addr [4]byte` / `Zero [8]uint8` are golib `array<T>` values, each a BACKING-ARRAY REFERENCE plus
// bounds. Reading that struct out of a native sockaddr_in FABRICATES two managed references from
// address bytes and port bytes; `array<T>.Clone` then dereferences them. That is the fabricated-
// reference fork the sha3 arc named (BOARD "the fabricated-reference deref"), and it has no general
// fix: a `byte[]` cannot be a window on memory the CLR does not own.
//
// SO THE CHAIN IS COPIED INTO MANAGED RECORDS, AND THE SOCKADDR WITH IT. Each native ADDRINFOW
// becomes a `ж<AddrinfoW>` box, `Next` links the boxes, and `Addr` names a real managed
// `ж<RawSockaddrInet4>` / `ж<RawSockaddrInet6>` built from the native sockaddr -- typed by ai_family
// exactly as the native ai_addr is. The consumer's cast then finds the box it expects, holding real
// managed arrays.
//
// HOW A MANAGED POINTER SURVIVES THE `unsafe.Pointer` FIELD. `Addr` is Go's `syscall.Pointer`
// (`type Pointer *struct{}`), and the consumer projects it to a scalar and converts the scalar back
// to a typed pointer -- the exact round trip golib's ManagedPointerTokens exists to make work
// ("making unsafe.Pointer round-trip for a MANAGED pointer"; ж.PointerTokens.cs). The sockaddr box
// is registered under its own pointer-order token and `Addr` carries that token, so
// `(*RawSockaddrInet4)(unsafe.Pointer(result.Addr))` recovers the very box this file built. Until
// now the only minter of those tokens was the reflect bridge; this is the second, and the shape is
// the one the table was written for rather than a new use of it.
//
// The token table holds its boxes WEAKLY -- by design, so a printed pointer can never leak -- so the
// strong reference the sockaddr needs comes from s_sockaddrAnchors below, keyed on the record that
// points at it. A record the caller can still dereference is a record whose sockaddr is still alive,
// which is precisely the Go lifetime.
//
// THE NATIVE CHAIN IS FREED HERE, EAGERLY, and FreeAddrInfoW is therefore a no-op. Every managed
// record is a complete copy, so there is nothing left for the caller's `defer FreeAddrInfoW(result)`
// to release; freeing at the copy also means no native allocation ever escapes this function, which
// is the "the mirror is a LOCAL at the call site" rule the rest of the class follows. GetAddrInfoW
// is the only producer of a `*AddrinfoW` in Go's syscall package, so no caller can arrive at
// FreeAddrInfoW holding a chain this file did not build.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_addrinfo_impl.go exists, so a reconvert never regenerates this
// file). The two declarations it replaces are registered in the converter's manualConversionFuncs,
// which is what turns their generated bodies into placeholders.
[module: go.GoManualConversion]

// The native mirror and the sockaddr transcription are pointer work throughout -- see
// zsyscall_windows_impl.cs for why this is declared rather than inherited.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // ADDRINFOW exactly as Windows lays it out: 48 bytes on x64, four ints, a size_t, and three raw
    // pointers. Every field the converted AddrinfoW holds as a managed reference is a machine
    // pointer here, which is the whole difference.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeAddrinfoW
    {
        public int32 Flags;
        public int32 Family;
        public int32 Socktype;
        public int32 Protocol;
        public nuint Addrlen;
        public uint16* Canonname;
        public byte* Addr;
        public NativeAddrinfoW* Next;
    }

    // Keeps each record's sockaddr box alive for exactly as long as the record itself is reachable.
    // ManagedPointerTokens is deliberately weak, so without this the box behind a token could be
    // collected while the record still names it and the consumer's cast would silently fall back to
    // a native-address box over a token -- a wild read. Keyed on the ж<AddrinfoW> record because
    // that is what a caller holds while it dereferences Addr; the whole chain is additionally
    // reachable from the head through Next, which is what the caller's `defer FreeAddrInfoW(result)`
    // pins for the duration of the walk.
    private static readonly ConditionalWeakTable<object, object> s_sockaddrAnchors = new();

    // GetAddrInfoW is the native transcription of the generated wrapper -- see the file header for
    // why it cannot be a literal conversion. Return values follow the Go original exactly:
    // GetAddrInfoW reports failure through its RETURN VALUE rather than WSAGetLastError, so a
    // non-zero result becomes that Errno directly and no last error is consulted. `result` is left
    // untouched on failure, as it is in Go, because the kernel did not write it.
    public static unsafe error /*sockerr*/ GetAddrInfoW(ж<uint16> Ꮡnodename, ж<uint16> Ꮡservicename, ж<AddrinfoW> Ꮡhints, ж<ж<AddrinfoW>> Ꮡresult) {
        NativeAddrinfoW nativeHints = default;
        NativeAddrinfoW* hints = null;

        if (Ꮡhints != nil) {
            ref AddrinfoW managed = ref Ꮡhints.Value;

            // Only the four scalars are carried. ai_addrlen, ai_canonname, ai_addr and ai_next must
            // be zero in a hints record -- Windows rejects the call otherwise -- and they are
            // exactly the fields whose managed forms have no native meaning, so `default` above is
            // both the required value and the only representable one.
            nativeHints.Flags = managed.Flags;
            nativeHints.Family = managed.Family;
            nativeHints.Socktype = managed.Socktype;
            nativeHints.Protocol = managed.Protocol;

            hints = &nativeHints;
        }

        NativeAddrinfoW* native = null;
        uintptr r0;

        // The names are the caller's NUL-terminated UTF-16 buffers, reached through golib pointer
        // boxes whose uintptr conversion can only hand back a TRANSIENT pinned address. Pinning them
        // HERE keeps them fixed for the whole call instead, exactly as findFirstFile1 does. Either
        // may be nil -- net passes a node name for a host lookup and a service name for a port
        // lookup, never both -- and `fixed` has no null form, so the four combinations are spelled
        // out rather than folded.
        if (Ꮡnodename == nil) {
            if (Ꮡservicename == nil) {
                r0 = callGetAddrInfoW(null, null, hints, &native);
            } else {
                fixed (uint16* servicename = &Ꮡservicename.Value) {
                    r0 = callGetAddrInfoW(null, servicename, hints, &native);
                }
            }
        } else {
            fixed (uint16* nodename = &Ꮡnodename.Value) {
                if (Ꮡservicename == nil) {
                    r0 = callGetAddrInfoW(nodename, null, hints, &native);
                } else {
                    fixed (uint16* servicename = &Ꮡservicename.Value) {
                        r0 = callGetAddrInfoW(nodename, servicename, hints, &native);
                    }
                }
            }
        }

        if (r0 != 0) {
            return ((Errno)r0);
        }

        try {
            if (Ꮡresult is not null && !Ꮡresult.IsNilPointer) {
                // ValueSlot, not Value: the pointee is itself a POINTER, so the box legitimately
                // holds null before this call and Value's value-peeking nil check reads that as a
                // nil-pointer dereference (the split is documented on ValueSlot in ж.cs). Go's
                // `*result = head` is a write THROUGH a real pointer, never a dereference of nil.
                Ꮡresult.ValueSlot = copyAddrinfoChain(native);
            }
        }
        finally {
            // Freed here, always: nothing native escapes this function (see the file header).
            if (native != null) {
                Syscall(procFreeAddrInfoW.Addr(), 1, (uintptr)(void*)native, 0, 0);
            }
        }

        return default!;
    }

    // FreeAddrInfoW releases nothing, because GetAddrInfoW above already did. The chain a caller
    // holds is managed, so the collector owns it; the Go call site (`defer FreeAddrInfoW(result)`)
    // stays exactly where Go put it and simply has nothing to do. Handing this address to the real
    // FreeAddrInfoW is what must NOT happen -- it is a managed object, and ws2_32 would free memory
    // it does not own.
    public static void FreeAddrInfoW(ж<AddrinfoW> Ꮡaddrinfo) {
    }

    private static unsafe uintptr callGetAddrInfoW(uint16* nodename, uint16* servicename, NativeAddrinfoW* hints, NativeAddrinfoW** result) {
        var (r0, _, _) = Syscall6(procGetAddrInfoW.Addr(), 4,
            (uintptr)(void*)nodename, (uintptr)(void*)servicename, (uintptr)(void*)hints, (uintptr)(void*)result, 0, 0);

        return r0;
    }

    // Transcribes the native chain into managed records, preserving order. A record's Addr names a
    // managed sockaddr box (see managedSockaddrPointer); its Next names the following record, so the
    // Go walk `for ; result != nil; result = result.Next` reads exactly as it does in Go.
    private static unsafe ж<AddrinfoW> copyAddrinfoChain(NativeAddrinfoW* native) {
        ж<AddrinfoW> head = default!;
        ж<AddrinfoW> tail = default!;

        for (NativeAddrinfoW* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<AddrinfoW>();
            ref AddrinfoW record = ref box.Value;

            record.Flags = cursor->Flags;
            record.Family = cursor->Family;
            record.Socktype = cursor->Socktype;
            record.Protocol = cursor->Protocol;
            // Reported as the NATIVE length, which is what a Go caller reading it back would mean by
            // it -- it describes the sockaddr, not the managed box that now carries it.
            record.Addrlen = (uintptr)cursor->Addrlen;
            record.Canonname = copyNativeCanonname(cursor->Canonname);

            object? sockaddr;
            record.Addr = managedSockaddrPointer(cursor->Addr, (nint)cursor->Addrlen, out sockaddr);

            if (sockaddr is not null) {
                s_sockaddrAnchors.Add(box, sockaddr);
            }

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    // Builds the managed sockaddr the record's Addr names, and hands back the box itself so the
    // caller can anchor it. The TYPE is chosen by the address family, exactly as the native ai_addr
    // is: Windows writes a sockaddr_in for AF_INET and a sockaddr_in6 for AF_INET6, and net casts to
    // the matching Go type after reading ai_family. An unrecognized or short family yields nil,
    // which net's `default:` arm already handles (it answers EWINDOWS).
    //
    // Port is copied VERBATIM. Go's RawSockaddrInet4.Port holds the port in NETWORK byte order in
    // memory, which is the same in-memory pattern sockaddr_in.sin_port carries, and the caller
    // finishes with syscall.Ntohs -- so a swap here would be a second one.
    private static unsafe Pointer managedSockaddrPointer(byte* addr, nint addrlen, out object? sockaddr) {
        sockaddr = null;

        if (addr == null || addrlen < 2) {
            return nil;
        }

        uint16 family = *(uint16*)addr;

        if (family == AF_INET && addrlen >= sizeof(NativeSockaddrInet4)) {
            NativeSockaddrInet4* native = (NativeSockaddrInet4*)addr;
            var sa = @new<RawSockaddrInet4>();

            sa.Value.Family = native->Family;
            sa.Value.Port = native->Port;
            sa.Value.Addr = copyNativeOctets(native->Addr, 4);
            sa.Value.Zero = copyNativeOctets(native->Zero, 8);

            sockaddr = sa;
            return tokenPointer(sa);
        }

        if (family == AF_INET6 && addrlen >= sizeof(NativeSockaddrInet6)) {
            NativeSockaddrInet6* native = (NativeSockaddrInet6*)addr;
            var sa = @new<RawSockaddrInet6>();

            sa.Value.Family = native->Family;
            sa.Value.Port = native->Port;
            sa.Value.Flowinfo = native->Flowinfo;
            sa.Value.Addr = copyNativeOctets(native->Addr, 16);
            sa.Value.Scope_id = native->Scope_id;

            sockaddr = sa;
            return tokenPointer(sa);
        }

        return nil;
    }

    // Registers a managed pointer under its own pointer-order token and wraps that token in the
    // Go `syscall.Pointer` the field holds. The wrapper is built over a NATIVE-address ж<EmptyStruct>
    // deliberately: syscall.Pointer's generated uintptr conversion takes the address of the storage
    // its box addresses, and for a native box that IS the number handed in -- so the token survives
    // the field round trip unchanged and reaches the consumer's `(*T)(unsafe.Pointer(...))` cast,
    // where ManagedPointerTokens resolves it back to this box.
    private static Pointer tokenPointer<T>(ж<T> box) {
        nuint token = box.PointerOrderToken;

        ManagedPointerTokens.Register(token, box);

        return new Pointer((ж<EmptyStruct>)(uintptr)token);
    }

    // Copies an inline native octet run into the golib array<byte> the converted sockaddr holds.
    private static unsafe array<byte> copyNativeOctets(byte* source, nint length) {
        var destination = new array<byte>(length);

        for (nint i = 0; i < length; i++) {
            destination[i] = source[i];
        }

        return destination;
    }

    // Copies ai_canonname -- a NUL-terminated native UTF-16 run -- into a managed buffer and hands
    // back a pointer to it, so a caller reading it reads managed storage rather than memory this
    // function is about to free. The terminator is kept, because every reader of a *uint16 name in
    // Go stops at one. Nothing in the corpus requests AI_CANONNAME today (net never sets the flag,
    // and Windows leaves the field NULL without it), so this is the faithful-copy arm rather than a
    // measured path.
    private static unsafe ж<uint16> copyNativeCanonname(uint16* source) {
        if (source == null) {
            return default!;
        }

        nint length = 0;

        while (source[length] != 0) {
            length++;
        }

        var name = new array<uint16>(length + 1);

        for (nint i = 0; i < length; i++) {
            name[i] = source[i];
        }

        return Ꮡ(name, 0);
    }
}
