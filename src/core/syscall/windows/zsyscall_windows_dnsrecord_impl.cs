// zsyscall_windows_dnsrecord_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementation of the DNS RECORD pair -- _DnsQuery and DnsRecordListFree -- the
// third-fork member the ptrout census deferred by name ("it belongs to a `net` DNS arc"). This is
// that arc.
//
// TWO defects, one remedy.
//
// (1) THE OUT-PARAMETER. `_DnsQuery`'s `qrs` is a `**DNSRecord`. The generated wrapper passed
//     `(uintptr)Ꮡqrs`, and a heap-boxed pointer that is still nil converts to ZERO -- so
//     DnsQuery_W received a NULL ppQueryResults, answered ERROR_INVALID_PARAMETER, and every
//     record-type lookup (MX/NS/TXT/SRV/PTR/CNAME) reported "no record". That is the whole of the
//     17-verdict divergence the net residual inventory named.
//
// (2) WHY PUBLISHING THE ADDRESS IS NOT THE FIX, AND IS WORSE THAN THE BUG. The five safe members
//     in zsyscall_windows_ptrout_impl.cs publish the kernel's address into the caller's box. Doing
//     that here would hand `net` a NATIVE chain, and net/windows/lookup_windows.cs reads each
//     record's payload by REINTERPRETING the union buffer:
//
//         p.at(syscall.DNSRecord.ᏑData, 0).Reinterpret<byte, syscall.DNSSRVData>()
//
//     Every payload carries a MANAGED REFERENCE (`DNSSRVData.Target`, `DNSMXData.NameExchange`,
//     `DNSPTRData.Host`, `DNSTXTData.StringArray` are `ж<uint16>` / `array<ж<uint16>>`), and
//     golib's `ReinterpretAliasesStorage<byte, DNSSRVData>` refuses the safe managed-alias route on
//     the SIZE test alone -- `Unsafe.SizeOf<DNSSRVData>()` is 16, `Unsafe.SizeOf<byte>()` is 1 --
//     so the call falls through to `TryPinnedReinterpret ?? (ж<TDst>)(uintptr)box`, i.e. an
//     `Unsafe.AsRef<DNSSRVData>` over raw bytes that loads eight of them AS AN OBJECT REFERENCE.
//     golib names the consequence itself: "a fabricated managed reference is a CLR type-safety
//     break (an access violation, or silent heap corruption on a write), which is strictly worse
//     than the wrong-but-contained read the address route produces." A silent no-record is
//     contained; a fabricated reference is not.
//
//     The fabrication happens whether the source is native (NativeBox<T>.Value IS
//     `Unsafe.AsRef<T>(addr)`) or a managed byte array (TryPinnedReinterpret pins it and
//     fabricates over the pin). The syscall layer cannot reach it either way -- which is why this
//     file has a COMPANION hand-own at net/windows/lookup_windows.cs that reads the payloads from
//     the side channel below instead of reinterpreting them.
//
// THE CHAIN IS THEREFORE TRANSCRIBED, exactly as zsyscall_windows_addrinfo_impl.cs transcribes
// ADDRINFOW: each native DNS_RECORDW becomes a managed `ж<DNSRecord>` box, `Next` links the boxes
// so `for (var p = Ꮡr; p != nil; p = p.Value.Next)` reads as it does in Go, `Name` names a managed
// copy of the record's name, and the per-type payload becomes a REAL managed box registered in
// s_dnsPayloads. THE NATIVE CHAIN IS FREED HERE, EAGERLY, and DnsRecordListFree is therefore a
// no-op -- handing a managed object's address to dnsapi is what must not happen.
//
// RETIREMENT CONDITION: this file and its lookup_windows.cs companion retire when the
// pointer-bearing-union representation arc lands -- the named converter item that stops emitting a
// Go union buffer whose payloads carry pointers as a bare `array<byte>` -- and the auto-conversion
// of both files becomes sound. Until then the reinterpret above cannot be made safe at any layer,
// and these two hand-owns are the containment.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Hand-owned (no zsyscall_windows_dnsrecord_impl.go exists, so a reconvert never regenerates this
// file). The declarations it replaces are registered in the converter's manualConversionFuncs,
// which is what turns their generated bodies into placeholders.
[module: go.GoManualConversion]

// The native out-cell, the chain walk and the union reads are pointer work.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // DNS_RECORDW, x64: pNext 0, pName 8, wType 16, wDataLength 18, Flags 20, dwTtl 24,
    // dwReserved 28, Data 32 -- the same field order and offsets Go's DNSRecord declares.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeDnsRecord
    {
        public NativeDnsRecord* Next;
        public ushort* Name;
        public ushort Type;
        public ushort Length;
        public uint Dw;
        public uint Ttl;
        public uint Reserved;
    }

    private const int nativeDnsRecordDataOffset = 32;

    // DNS_PTR_DATAW -- also the shape NS and CNAME records use.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeDnsPtrData
    {
        public ushort* NameHost;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeDnsMxData
    {
        public ushort* NameExchange;
        public ushort Preference;
        public ushort Pad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeDnsSrvData
    {
        public ushort* Target;
        public ushort Priority;
        public ushort Weight;
        public ushort Port;
        public ushort Pad;
    }

    // DNS_TXT_DATAW declares `DWORD dwStringCount` where Go's DNSTXTData declares a uint16. The
    // difference is harmless and must not be "corrected" in either direction: pointer alignment
    // puts pStringArray at offset 8 under BOTH declarations, so only the count's width differs,
    // and a TXT string count never approaches 2^16. The native mirror follows Windows; the managed
    // record keeps Go's uint16, narrowed on the copy.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeDnsTxtData
    {
        public uint StringCount;
    }

    private const int nativeDnsTxtStringArrayOffset = 8;

    // The side channel the lookup_windows.cs companion reads instead of reinterpreting `Data`.
    // Keyed by the record's managed box, so a payload lives exactly as long as the record that
    // names it and no explicit lifetime is owed anywhere -- the same anchoring shape the addrinfo
    // transcription uses for its sockaddrs.
    private static readonly ConditionalWeakTable<object, object> s_dnsPayloads = new();

    /// <summary>
    /// The managed payload transcribed for a record, or the type's nil pointer when the record
    /// carries no payload of that shape. Replaces
    /// <c>p.at(DNSRecord.ᏑData, 0).Reinterpret&lt;byte, T&gt;()</c> at every call site -- see the
    /// file header for why that reinterpret cannot be made safe at any layer.
    /// </summary>
    public static ж<T> DnsRecordPayload<T>(ж<DNSRecord> Ꮡrec)
    {
        if (Ꮡrec is null || Ꮡrec.IsNilPointer)
            return ж<T>.NilBox;

        if (s_dnsPayloads.TryGetValue(Ꮡrec, out object? payload) && payload is ж<T> typed)
            return typed;

        return ж<T>.NilBox;
    }

    internal static unsafe error /*status*/ _DnsQuery(ж<uint16> Ꮡname, uint16 qtype, uint32 options, ж<byte> Ꮡextra, ж<ж<DNSRecord>> Ꮡqrs, ж<byte> Ꮡpr) {
        NativeDnsRecord* native = null;
        uintptr r0;

        // The name is the caller's NUL-terminated UTF-16 buffer behind a golib box, whose uintptr
        // conversion hands back only a TRANSIENT pinned address. Pin it here for the whole call,
        // exactly as GetAddrInfoW does.
        if (Ꮡname == nil) {
            r0 = callDnsQuery(null, qtype, options, (uintptr)Ꮡextra, &native, (uintptr)Ꮡpr);
        } else {
            fixed (uint16* name = &Ꮡname.Value) {
                r0 = callDnsQuery(name, qtype, options, (uintptr)Ꮡextra, &native, (uintptr)Ꮡpr);
            }
        }

        if (r0 != 0) {
            // A failed query leaves nothing to free: DnsQuery_W writes ppQueryResults only on
            // success, so `native` is still null.
            return ((Errno)r0);
        }

        try {
            if (Ꮡqrs is not null && !Ꮡqrs.IsNilPointer) {
                // ValueSlot, not Value: the pointee is itself a POINTER, so the box legitimately
                // holds null before this call and Value's value-peeking nil check would read that
                // as a nil dereference. Go's `*qrs = head` is a write THROUGH a real pointer.
                Ꮡqrs.ValueSlot = copyDnsChain(native);
            }
        }
        finally {
            // Freed here, always: nothing native escapes this function (see the file header).
            // DnsFreeRecordList == 1, the same value net's own `defer` passes.
            if (native != null) {
                Syscall(procDnsRecordListFree.Addr(), 2, (uintptr)(void*)native, 1, 0);
            }
        }

        return default!;
    }

    // DnsRecordListFree releases nothing, because _DnsQuery above already did. The chain a caller
    // holds is managed, so the collector owns it; net's `defer syscall.DnsRecordListFree(rec, 1)`
    // stays exactly where Go put it and simply has nothing to do. Handing this address to the real
    // DnsRecordListFree is what must NOT happen -- it is a managed object, and dnsapi would free
    // memory it does not own.
    public static void DnsRecordListFree(ж<DNSRecord> Ꮡrl, uint32 freetype) {
    }

    private static unsafe uintptr callDnsQuery(uint16* name, uint16 qtype, uint32 options, uintptr extra, NativeDnsRecord** qrs, uintptr pr) {
        var (r0, _, _) = Syscall6(procDnsQuery_W.Addr(), 6,
            (uintptr)(void*)name, (uintptr)qtype, (uintptr)options, extra, (uintptr)(void*)qrs, pr);

        return r0;
    }

    // Transcribes the native chain into managed records, preserving order, and registers each
    // record's typed payload in s_dnsPayloads.
    private static unsafe ж<DNSRecord> copyDnsChain(NativeDnsRecord* native) {
        ж<DNSRecord> head = default!;
        ж<DNSRecord> tail = default!;

        for (NativeDnsRecord* cursor = native; cursor != null; cursor = cursor->Next) {
            ref var record = ref heap(new DNSRecord(), out var Ꮡrecord);

            record.Name = copyNativeDnsName(cursor->Name);
            record.Type = cursor->Type;
            record.Length = cursor->Length;
            record.Dw = cursor->Dw;
            record.Ttl = cursor->Ttl;
            record.Reserved = cursor->Reserved;

            registerDnsPayload(Ꮡrecord, cursor);

            if (head == nil) {
                head = Ꮡrecord;
            } else {
                tail.Value.Next = Ꮡrecord;
            }

            tail = Ꮡrecord;
        }

        return head;
    }

    // Builds the record's payload as a REAL managed box, typed by the record's DNS type, and
    // anchors it to the record. Every name the payload points at is copied too, because the native
    // chain those pointers name is freed before this function's caller returns.
    private static unsafe void registerDnsPayload(ж<DNSRecord> Ꮡrecord, NativeDnsRecord* cursor) {
        byte* data = (byte*)cursor + nativeDnsRecordDataOffset;
        uint16 type = cursor->Type;

        // An if-chain, not a switch: go2cs emits the DNS_TYPE_* values as PROPERTIES
        // (`public static UntypedInt DNS_TYPE_NS => 0x0002;`), and a property is not a constant, so
        // it cannot be a case label (CS9135). Comparison is the form the converted corpus already
        // uses -- lookup_windows.cs's own `(~p).Type != dnstype` compares exactly this pair.
        if (type == DNS_TYPE_PTR || type == DNS_TYPE_NS || type == DNS_TYPE_CNAME) {
            NativeDnsPtrData* ptr = (NativeDnsPtrData*)data;
            ref var payload = ref heap(new DNSPTRData(), out var Ꮡpayload);
            payload.Host = copyNativeDnsName(ptr->NameHost);
            s_dnsPayloads.Add(Ꮡrecord, Ꮡpayload);
        }
        else if (type == DNS_TYPE_MX) {
            NativeDnsMxData* mx = (NativeDnsMxData*)data;
            ref var payload = ref heap(new DNSMXData(), out var Ꮡpayload);
            payload.NameExchange = copyNativeDnsName(mx->NameExchange);
            payload.Preference = mx->Preference;
            s_dnsPayloads.Add(Ꮡrecord, Ꮡpayload);
        }
        else if (type == DNS_TYPE_SRV) {
            NativeDnsSrvData* srv = (NativeDnsSrvData*)data;
            ref var payload = ref heap(new DNSSRVData(), out var Ꮡpayload);
            payload.Target = copyNativeDnsName(srv->Target);
            payload.Priority = srv->Priority;
            payload.Weight = srv->Weight;
            payload.Port = srv->Port;
            s_dnsPayloads.Add(Ꮡrecord, Ꮡpayload);
        }
        else if (type == DNS_TYPE_TEXT) {
            NativeDnsTxtData* txt = (NativeDnsTxtData*)data;
            nint count = (nint)txt->StringCount;
            ushort** strings = (ushort**)(data + nativeDnsTxtStringArrayOffset);

            ref var payload = ref heap(new DNSTXTData(), out var Ꮡpayload);
            payload.StringCount = (uint16)count;

            // The consumer aliases from &StringArray[0] and slices to StringCount, so the backing
            // store must hold at least that many elements -- Go's `[1]*uint16` declaration is the
            // union's minimum, never the record's real length.
            var managed = new array<ж<uint16>>(count < 1 ? 1 : count);

            for (nint i = 0; i < count; i++) {
                managed[i] = copyNativeDnsName(strings[i]);
            }

            payload.StringArray = managed;
            s_dnsPayloads.Add(Ꮡrecord, Ꮡpayload);
        }
    }

    // A native NUL-terminated UTF-16 name into a managed buffer, NUL kept. Modelled on the
    // addrinfo transcription's copyNativeCanonname.
    private static unsafe ж<uint16> copyNativeDnsName(ushort* source) {
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
