// interface_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of net.adapterAddresses -- the single producer behind EVERY Windows
// interface and DNS-configuration answer the corpus can give. Four consumers read what it returns:
// interfaceTable (net.Interfaces), interfaceAddrTable (net.InterfaceAddrs / Interface.Addrs),
// interfaceMulticastAddrTable (Interface.MulticastAddrs) and dnsReadConfig, which is
// getSystemDNSConfig's ONLY source of DNS servers on Windows. That last one is why this file is the
// wall behind GetAddrInfoW: name resolution cannot start without a resolver list, so until this
// function returns managed records, no converted program on Windows can resolve a host name at all.
//
// THE DEFECT, and why it is NOT a wrapper defect. Go asks the kernel to fill a []byte and then walks
// that buffer as a linked record:
//
//     b = make([]byte, l)
//     windows.GetAdaptersAddresses(AF_UNSPEC, flags, 0, (*windows.IpAdapterAddresses)(unsafe.Pointer(&b[0])), &l)
//     for aa := (*windows.IpAdapterAddresses)(unsafe.Pointer(&b[0])); aa != nil; aa = aa.Next { … }
//
// The CALL is legitimate -- a byte buffer is a byte buffer, and the kernel writes native bytes into
// it. The WALK is not. `Ꮡ(b, 0).Reinterpret<byte, IpAdapterAddresses>()` asks golib to alias a byte
// run as a reference-bearing struct, which it correctly refuses (IpAdapterAddresses carries nine
// `ж<T>` fields, an `array<byte>` and an `array<uint32>` -- managed references where the native
// record has raw pointers and inline storage), so the reinterpret falls back to a NATIVE-ADDRESS box
// over the buffer. Reading any field through that box fabricates a managed reference out of adapter
// bytes. The measured shape is a process kill on the very first field touched -- the loop's own nil
// test:
//
//     Fatal error. System.AccessViolationException
//        at go.ж`1[IpAdapterAddresses].op_Equality(ж`1<IpAdapterAddresses>, NilType)
//        at go.net_package.adapterAddresses()
//        at go.net_package.dnsReadConfig(string)      <- via getSystemDNSConfig, from lookupIP
//
// This is the same seam as GetTimeZoneInformation / win32finddata1 / WSAPROTOCOL_INFOW / ADDRINFOW,
// one structure size up, and it takes the same remedy: blittable [StructLayout(Sequential)] mirrors,
// the buffer held in NATIVE memory that never escapes this function, and an explicit transcription
// into managed records at the boundary. GetAddrInfoW (syscall/windows/zsyscall_windows_addrinfo_impl.cs)
// is the closest precedent and should be read first; what is new here is the SHAPE of the output --
// not one chain but a chain of records each carrying SIX chains of its own.
//
// WHY THE WHOLE CHAIN IS TRANSCRIBED, not just the top record. The same reason as ADDRINFOW, twice
// over. Every consumer reaches THROUGH a record: dnsReadConfig walks aa.FirstDnsServerAddress and
// reads dns.Address.Sockaddr.Sockaddr(); interfaceAddrTable walks FirstUnicastAddress and
// FirstAnycastAddress; interfaceMulticastAddrTable walks FirstMulticastAddress. Handing back a
// managed top-level record whose Next/First* fields still named native memory would leave every
// consumer reading exactly the memory this file exists to keep it away from -- and the fields it
// would read there (`SocketAddress.Sockaddr`, a `ж<syscall.RawSockaddrAny>`) are managed references
// again, so the fabrication would simply move one hop out.
//
// THE SOCKADDRS ARE TRANSCRIBED TOO, and they cross the boundary as an ORDINARY MANAGED POINTER --
// no ManagedPointerTokens here, unlike ADDRINFOW. That difference is worth stating because it looks
// like an omission and is not: Go declares this field as `Sockaddr *syscall.RawSockaddrAny`, a TYPED
// Go pointer, where ai_addr is an untyped `unsafe.Pointer` that net casts by hand. A typed pointer
// converts to a `ж<syscall.RawSockaddrAny>` field that carries a managed box directly, so there is
// no `unsafe.Pointer` round trip to survive and no token to mint. The consumers then call
// `.Sockaddr()` on it -- syscall's own hand-owned DECODE (syscall/windows/syscall_windows_impl.cs),
// which FLATTENS the managed RawSockaddrAny back to its 116-byte native image and decodes that. So
// the transcription below writes the managed image that decode reads, using the Go declaration's own
// mapping: Family at 0, Addr.Data covering bytes 2..15, Pad covering 16..115. It is the same
// inverse-flattening `syscall`'s GetAcceptExSockaddrs performs for the accept path, and the two are
// deliberately identical -- one native sockaddr in, one managed RawSockaddrAny out.
//
// WHAT IS TRANSCRIBED. Everything Go declares, and the discipline is deliberate: this record is not
// read by ONE call site whose field use can be enumerated, it is the public shape behind
// net.Interfaces. What the four consumers actually READ today is
//
//     Next, IfIndex, Ipv6IfIndex, FriendlyName, OperStatus, IfType, Mtu,
//     PhysicalAddressLength, PhysicalAddress, FirstUnicastAddress{Next, Address, OnLinkPrefixLength},
//     FirstAnycastAddress{Next, Address}, FirstMulticastAddress{Next, Address},
//     FirstDnsServerAddress{Next, Address}, and FirstGatewayAddress (nil-tested only)
//
// which leaves Length, AdapterName, DnsSuffix, Description, Flags, ZoneIndices, FirstPrefix,
// TransmitLinkSpeed, ReceiveLinkSpeed and FirstWinsServerAddress unread. They are copied anyway --
// each is cheap, and leaving a declared field nil or zero would be a SILENT divergence for the next
// consumer rather than a loud one (GAA_FLAG_INCLUDE_PREFIX is passed, so the prefix list genuinely
// exists in the native data; a nil FirstPrefix would be a lie about the host). The one thing NOT
// carried is the native record's tail past Go's declaration -- the "/* more fields might be present
// here. */" the Go source ends on -- because Go itself cannot name those fields and no consumer can
// reach them.
//
// THE NATIVE BUFFER IS FREED HERE, EAGERLY, in a finally. Every managed record is a complete copy,
// so nothing native escapes this function -- the same "the mirror is a LOCAL at the call site" rule
// the rest of the class follows, and the reason no free-side hand-own is owed (Go has none: the
// buffer was a garbage-collected []byte).
//
// THE GENERATED WRAPPER IS LEFT ALONE. windows.GetAdaptersAddresses is called below exactly as the
// auto conversion calls it, and it is CORRECT for what it is handed here: its
// `uintptr(unsafe.Pointer(adapterAddresses))` answers the exact address of a native-address box
// (ж<T>'s uintptr operator short-circuits on m_nativeAddr), so the kernel fills the native buffer
// this function owns. The defect was never in the wrapper -- it was in reinterpreting a managed
// buffer as a managed record -- so hand-owning the wrapper would have fixed nothing and frozen a
// faithful conversion for no gain.

using System;
using System.Runtime.InteropServices;

using NativeMemory = System.Runtime.InteropServices.NativeMemory;

// Hand-owned (no interface_windows_impl.go exists, so a reconvert never regenerates this file). The
// declaration it replaces is registered in the converter's manualConversionFuncs, which is what turns
// its generated body into a placeholder.
[module: go.GoManualConversion]

// The native mirrors and the whole transcription are pointer work throughout; `net`'s generated
// csproj emits AllowUnsafeBlocks=false, so the capability is DECLARED here and the csproj flipping to
// true on the next reconvert is part of the intended footprint.
[module: go.GoRequiresUnsafe]

namespace go;

using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;

partial class net_package
{
    // ---- Native mirrors ------------------------------------------------------------------------
    //
    // IP_ADAPTER_ADDRESSES_LH and its six list flavors, exactly as iphlpapi lays them out on a
    // 64-bit host. Each leading `union { ULONGLONG Alignment; struct { ULONG Length; ULONG …; }; }`
    // is spelled as its two ULONGs -- the union exists only to force 8-byte alignment, which
    // Sequential layout gives anyway once the record holds a pointer. Every field the converted
    // struct holds as a managed reference (`ж<T>`, `array<T>`) is a machine pointer or inline
    // storage here, which is the whole difference.

    // SOCKET_ADDRESS. 12 bytes of content, 16 with the trailing pad every embedding takes.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSocketAddress
    {
        public byte* Sockaddr;
        public int32 SockaddrLength;
    }

    // IP_ADAPTER_ANYCAST_ADDRESS_XP, _MULTICAST_ADDRESS_XP, _DNS_SERVER_ADDRESS_XP,
    // _WINS_SERVER_ADDRESS_LH and _GATEWAY_ADDRESS_LH are byte-for-byte the same shape -- {ULONG
    // Length; ULONG Flags-or-Reserved; PTR Next; SOCKET_ADDRESS Address;}, 32 bytes -- and share one
    // mirror. The MANAGED types stay distinct (Go declares five), so the five loops below differ
    // only in which one they build.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSocketAddressListEntry
    {
        public uint32 Length;
        public uint32 FlagsOrReserved;
        public NativeSocketAddressListEntry* Next;
        public NativeSocketAddress Address;
    }

    // IP_ADAPTER_UNICAST_ADDRESS_LH -- the one list flavor with its own tail.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeIpAdapterUnicastAddress
    {
        public uint32 Length;
        public uint32 Flags;
        public NativeIpAdapterUnicastAddress* Next;
        public NativeSocketAddress Address;
        public int32 PrefixOrigin;
        public int32 SuffixOrigin;
        public int32 DadState;
        public uint32 ValidLifetime;
        public uint32 PreferredLifetime;
        public uint32 LeaseLifetime;
        public uint8 OnLinkPrefixLength;
    }

    // IP_ADAPTER_PREFIX_XP -- the other, one ULONG longer.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeIpAdapterPrefix
    {
        public uint32 Length;
        public uint32 Flags;
        public NativeIpAdapterPrefix* Next;
        public NativeSocketAddress Address;
        public uint32 PrefixLength;
    }

    // IP_ADAPTER_ADDRESSES_LH, through FirstGatewayAddress -- which is exactly as far as Go's
    // declaration reaches. 216 bytes on x64; the native record is longer and the tail is
    // deliberately unnamed here for the reason the file header gives.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeIpAdapterAddresses
    {
        public uint32 Length;
        public uint32 IfIndex;
        public NativeIpAdapterAddresses* Next;
        public byte* AdapterName;
        public NativeIpAdapterUnicastAddress* FirstUnicastAddress;
        public NativeSocketAddressListEntry* FirstAnycastAddress;
        public NativeSocketAddressListEntry* FirstMulticastAddress;
        public NativeSocketAddressListEntry* FirstDnsServerAddress;
        public uint16* DnsSuffix;
        public uint16* Description;
        public uint16* FriendlyName;
        public fixed uint8 PhysicalAddress[maxAdapterAddressLength];
        public uint32 PhysicalAddressLength;
        public uint32 Flags;
        public uint32 Mtu;
        public uint32 IfType;
        public uint32 OperStatus;
        public uint32 Ipv6IfIndex;
        public fixed uint32 ZoneIndices[zoneIndicesLength];
        public NativeIpAdapterPrefix* FirstPrefix;
        public uint64 TransmitLinkSpeed;
        public uint64 ReceiveLinkSpeed;
        public NativeSocketAddressListEntry* FirstWinsServerAddress;
        public NativeSocketAddressListEntry* FirstGatewayAddress;
    }

    // syscall.MAX_ADAPTER_ADDRESS_LENGTH and the ZoneIndices arity, as compile-time constants
    // because a `fixed` buffer's length must be one. Both mirror the converted declarations
    // (internal/syscall/windows: `PhysicalAddress = new(syscall.MAX_ADAPTER_ADDRESS_LENGTH)`,
    // `ZoneIndices = new(16)`).
    private const int32 maxAdapterAddressLength = 8;
    private const int32 zoneIndicesLength = 16;

    // The managed RawSockaddrAny image is 2 + 14 + 100 bytes; the two run lengths appear in the
    // transcription and in syscall's decode, so they are named rather than repeated.
    private const nint rawSockaddrDataLength = 14;
    private const nint rawSockaddrPadLength = 100;

    // Windows' recommended initial buffer size, and Go's (interface_windows.go).
    private const uint32 initialBufferSize = 15000;

    // ---- The function --------------------------------------------------------------------------

    // adapterAddresses returns a list of IP adapter and address structures. The structure contains
    // an IP adapter and flattened multiple IP addresses including unicast, anycast and multicast
    // addresses.
    //
    // The control flow is Go's exactly -- the same grow-and-retry loop, the same three exits, the
    // same os.NewSyscallError("getadaptersaddresses", err) wrapping -- so a caller cannot tell the
    // two apart by behavior. Only the buffer's OWNERSHIP differs: native memory this function frees,
    // instead of a []byte the collector owns, because a managed buffer cannot be walked as a record.
    internal static unsafe (slice<ж<windows.IpAdapterAddresses>>, error) adapterAddresses() {
        ref var l = ref heap<uint32>(out var Ꮡl);
        l = initialBufferSize;

        void* buffer = null;
        uint32 bufferLength = 0;

        try {
            while (true) {
                if (buffer != null) {
                    NativeMemory.Free(buffer);
                    buffer = null;
                }

                bufferLength = l;
                // AllocZeroed rather than Alloc: Go's make([]byte, l) hands the kernel zeroed
                // memory, and a partially-filled record's unwritten tail must read as Go's would.
                buffer = NativeMemory.AllocZeroed(bufferLength == 0 ? 1 : (nuint)bufferLength);

                const uint32 flags = /* windows.GAA_FLAG_INCLUDE_PREFIX | windows.GAA_FLAG_INCLUDE_GATEWAYS */ 144;

                // The buffer crosses as a native-address box, which is what makes the generated
                // wrapper correct here (see the file header). `l` crosses as an ordinary golib heap
                // box, exactly as the auto conversion passed it -- it is a bare uint32, so its
                // managed layout IS its native one.
                var err = windows.GetAdaptersAddresses(syscall.AF_UNSPEC, flags, 0,
                    (ж<windows.IpAdapterAddresses>)(uintptr)buffer, Ꮡl);

                if (err == default!) {
                    if (l == 0) {
                        return (default!, default!);
                    }
                    break;
                }

                if (err._<syscall.Errno>() != syscall.ERROR_BUFFER_OVERFLOW) {
                    return (default!, os.NewSyscallError("getadaptersaddresses"u8, err));
                }

                // Go compares the required size against len(b); the native buffer's length is the
                // same number, tracked explicitly because there is no slice to ask.
                if (l <= bufferLength) {
                    return (default!, os.NewSyscallError("getadaptersaddresses"u8, err));
                }
            }

            // The walk is Go's, over MANAGED records: the chain is transcribed first, then linked
            // through Next exactly as the native one was, so this loop reads as it does in Go.
            slice<ж<windows.IpAdapterAddresses>> aas = default!;

            for (var aa = copyAdapterChain((NativeIpAdapterAddresses*)buffer); aa != nil; aa = aa.Value.Next) {
                aas = append(aas, aa);
            }

            return (aas, default!);
        }
        finally {
            // Freed here, always: nothing native escapes this function (see the file header).
            if (buffer != null) {
                NativeMemory.Free(buffer);
            }
        }
    }

    // ---- Transcription -------------------------------------------------------------------------

    // Transcribes the native adapter chain into managed records, preserving order and re-linking
    // Next so the caller's walk is the same walk.
    private static unsafe ж<windows.IpAdapterAddresses> copyAdapterChain(NativeIpAdapterAddresses* native) {
        ж<windows.IpAdapterAddresses> head = default!;
        ж<windows.IpAdapterAddresses> tail = default!;

        for (NativeIpAdapterAddresses* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterAddresses>();
            ref windows.IpAdapterAddresses record = ref box.Value;

            record.Length = cursor->Length;
            record.IfIndex = cursor->IfIndex;
            record.AdapterName = copyNativeAnsiString(cursor->AdapterName);
            record.FirstUnicastAddress = copyUnicastChain(cursor->FirstUnicastAddress);
            record.FirstAnycastAddress = copyAnycastChain(cursor->FirstAnycastAddress);
            record.FirstMulticastAddress = copyMulticastChain(cursor->FirstMulticastAddress);
            record.FirstDnsServerAddress = copyDnsServerChain(cursor->FirstDnsServerAddress);
            record.DnsSuffix = copyNativeUtf16String(cursor->DnsSuffix);
            record.Description = copyNativeUtf16String(cursor->Description);
            record.FriendlyName = copyNativeUtf16String(cursor->FriendlyName);

            for (nint i = 0; i < maxAdapterAddressLength; i++) {
                record.PhysicalAddress[i] = cursor->PhysicalAddress[i];
            }

            record.PhysicalAddressLength = cursor->PhysicalAddressLength;
            record.Flags = cursor->Flags;
            record.Mtu = cursor->Mtu;
            record.IfType = cursor->IfType;
            record.OperStatus = cursor->OperStatus;
            record.Ipv6IfIndex = cursor->Ipv6IfIndex;

            for (nint i = 0; i < zoneIndicesLength; i++) {
                record.ZoneIndices[i] = cursor->ZoneIndices[i];
            }

            record.FirstPrefix = copyPrefixChain(cursor->FirstPrefix);
            record.TransmitLinkSpeed = cursor->TransmitLinkSpeed;
            record.ReceiveLinkSpeed = cursor->ReceiveLinkSpeed;
            record.FirstWinsServerAddress = copyWinsServerChain(cursor->FirstWinsServerAddress);
            record.FirstGatewayAddress = copyGatewayChain(cursor->FirstGatewayAddress);

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    // The six nested lists. Five of them share one native mirror and differ only in the managed type
    // they build -- written out rather than folded behind a generic, because the managed types have
    // no common shape and the duplication is what makes each list greppable from its consumer.

    private static unsafe ж<windows.IpAdapterUnicastAddress> copyUnicastChain(NativeIpAdapterUnicastAddress* native) {
        ж<windows.IpAdapterUnicastAddress> head = default!;
        ж<windows.IpAdapterUnicastAddress> tail = default!;

        for (NativeIpAdapterUnicastAddress* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterUnicastAddress>();
            ref windows.IpAdapterUnicastAddress record = ref box.Value;

            record.Length = cursor->Length;
            record.Flags = cursor->Flags;
            record.Address = managedSocketAddress(cursor->Address);
            record.PrefixOrigin = cursor->PrefixOrigin;
            record.SuffixOrigin = cursor->SuffixOrigin;
            record.DadState = cursor->DadState;
            record.ValidLifetime = cursor->ValidLifetime;
            record.PreferredLifetime = cursor->PreferredLifetime;
            record.LeaseLifetime = cursor->LeaseLifetime;
            record.OnLinkPrefixLength = cursor->OnLinkPrefixLength;

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    private static unsafe ж<windows.IpAdapterAnycastAddress> copyAnycastChain(NativeSocketAddressListEntry* native) {
        ж<windows.IpAdapterAnycastAddress> head = default!;
        ж<windows.IpAdapterAnycastAddress> tail = default!;

        for (NativeSocketAddressListEntry* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterAnycastAddress>();
            ref windows.IpAdapterAnycastAddress record = ref box.Value;

            record.Length = cursor->Length;
            record.Flags = cursor->FlagsOrReserved;
            record.Address = managedSocketAddress(cursor->Address);

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    private static unsafe ж<windows.IpAdapterMulticastAddress> copyMulticastChain(NativeSocketAddressListEntry* native) {
        ж<windows.IpAdapterMulticastAddress> head = default!;
        ж<windows.IpAdapterMulticastAddress> tail = default!;

        for (NativeSocketAddressListEntry* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterMulticastAddress>();
            ref windows.IpAdapterMulticastAddress record = ref box.Value;

            record.Length = cursor->Length;
            record.Flags = cursor->FlagsOrReserved;
            record.Address = managedSocketAddress(cursor->Address);

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    private static unsafe ж<windows.IpAdapterDnsServerAdapter> copyDnsServerChain(NativeSocketAddressListEntry* native) {
        ж<windows.IpAdapterDnsServerAdapter> head = default!;
        ж<windows.IpAdapterDnsServerAdapter> tail = default!;

        for (NativeSocketAddressListEntry* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterDnsServerAdapter>();
            ref windows.IpAdapterDnsServerAdapter record = ref box.Value;

            record.Length = cursor->Length;
            record.Reserved = cursor->FlagsOrReserved;
            record.Address = managedSocketAddress(cursor->Address);

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    private static unsafe ж<windows.IpAdapterPrefix> copyPrefixChain(NativeIpAdapterPrefix* native) {
        ж<windows.IpAdapterPrefix> head = default!;
        ж<windows.IpAdapterPrefix> tail = default!;

        for (NativeIpAdapterPrefix* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterPrefix>();
            ref windows.IpAdapterPrefix record = ref box.Value;

            record.Length = cursor->Length;
            record.Flags = cursor->Flags;
            record.Address = managedSocketAddress(cursor->Address);
            record.PrefixLength = cursor->PrefixLength;

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    private static unsafe ж<windows.IpAdapterWinsServerAddress> copyWinsServerChain(NativeSocketAddressListEntry* native) {
        ж<windows.IpAdapterWinsServerAddress> head = default!;
        ж<windows.IpAdapterWinsServerAddress> tail = default!;

        for (NativeSocketAddressListEntry* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterWinsServerAddress>();
            ref windows.IpAdapterWinsServerAddress record = ref box.Value;

            record.Length = cursor->Length;
            record.Reserved = cursor->FlagsOrReserved;
            record.Address = managedSocketAddress(cursor->Address);

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    private static unsafe ж<windows.IpAdapterGatewayAddress> copyGatewayChain(NativeSocketAddressListEntry* native) {
        ж<windows.IpAdapterGatewayAddress> head = default!;
        ж<windows.IpAdapterGatewayAddress> tail = default!;

        for (NativeSocketAddressListEntry* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<windows.IpAdapterGatewayAddress>();
            ref windows.IpAdapterGatewayAddress record = ref box.Value;

            record.Length = cursor->Length;
            record.Reserved = cursor->FlagsOrReserved;
            record.Address = managedSocketAddress(cursor->Address);

            if (head is null) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    // Builds the managed SOCKET_ADDRESS. SockaddrLength is reported as the NATIVE length, which is
    // what a Go caller reading it back would mean by it -- it describes the sockaddr, not the
    // managed box now carrying it. A null or zero-length entry yields a nil Sockaddr, which is the
    // state Go would have had; both consumers of it (`.Sockaddr()`) are reached only through a live
    // list entry, and iphlpapi never emits one without an address.
    private static unsafe windows.SocketAddress managedSocketAddress(NativeSocketAddress native) {
        windows.SocketAddress address = default;

        address.SockaddrLength = native.SockaddrLength;

        if (native.Sockaddr != null && native.SockaddrLength > 0) {
            address.Sockaddr = toRawSockaddrAny(native.Sockaddr, native.SockaddrLength);
        }

        return address;
    }

    // Transcribes one native sockaddr into the MANAGED RawSockaddrAny image that
    // RawSockaddrAny.Sockaddr (syscall/windows/syscall_windows_impl.cs) reads -- the exact inverse of
    // the flattening that method performs. The Go declaration's own mapping: Family at 0, Addr.Data
    // covering bytes 2..15, Pad covering 16..115. Deliberately identical to syscall's own
    // toRawSockaddrAny (zsyscall_windows_wsa_impl.cs); it is duplicated rather than shared because
    // exposing it would put a non-Go symbol on a published package's public surface.
    private static unsafe ж<syscall.RawSockaddrAny> toRawSockaddrAny(byte* native, nint available) {
        var Ꮡany = @new<syscall.RawSockaddrAny>();
        ref syscall.RawSockaddrAny any = ref Ꮡany.Value;

        if (available >= 2) {
            any.Addr.Family = *(uint16*)native;
        }

        for (nint i = 0; i < rawSockaddrDataLength && 2 + i < available; i++) {
            any.Addr.Data[i] = unchecked((int8)native[2 + i]);
        }

        for (nint i = 0; i < rawSockaddrPadLength && 16 + i < available; i++) {
            any.Pad[i] = unchecked((int8)native[16 + i]);
        }

        return Ꮡany;
    }

    // Copies a NUL-terminated native UTF-16 run into managed storage and hands back a pointer to it,
    // so windows.UTF16PtrToString walks managed memory rather than the buffer this function is about
    // to free. The terminator is kept, because every reader of a *uint16 name in Go stops at one.
    private static unsafe ж<uint16> copyNativeUtf16String(uint16* source) {
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

    // The same for AdapterName, which iphlpapi writes as a NUL-terminated ANSI string (the adapter's
    // GUID in text form). No consumer in the corpus reads it today; it is copied for the reason the
    // file header gives -- a declared field that silently reads nil is worse than one that reads
    // right.
    private static unsafe ж<byte> copyNativeAnsiString(byte* source) {
        if (source == null) {
            return default!;
        }

        nint length = 0;

        while (source[length] != 0) {
            length++;
        }

        var name = new array<byte>(length + 1);

        for (nint i = 0; i < length; i++) {
            name[i] = source[i];
        }

        return Ꮡ(name, 0);
    }
}
