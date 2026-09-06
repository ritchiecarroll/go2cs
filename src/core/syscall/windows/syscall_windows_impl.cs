// syscall_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the SOCKET-ADDRESS surface -- the sockaddr family of the syscall
// STRUCT-PASSING seam catalogued in docs/phase4/BOARD-next-validation-candidates.md. This is the
// member `net` forces: net.Listen on Windows died before any test logic ran, which walled net/smtp,
// net/http/cgi, net/http/httptest, net/rpc and eventually net itself.
//
// TWO defects sit on this path, and only fixing both makes a socket work.
//
// (1) THE PORT ALIAS. Go writes the port in network byte order through a two-byte alias over the
// raw struct's port field:
//
//     p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))
//     p[0] = byte(sa.Port >> 8)
//     p[1] = byte(sa.Port)
//
// The auto conversion of that is `var p = (ж<array<byte>>)(uintptr)(new @unsafe.Pointer(...))`, and
// an `array<T>` reconstructed from a raw address materializes `default(array<byte>)` -- a
// LENGTH-ZERO array -- so `p[0]` panics with `index out of range [0] with length 0` (golib
// array.cs:280 via syscall_windows.cs:881). `array<T>` is a managed container with its own header,
// not two inline bytes, so NO address reinterpret can produce one; the remedy is to stop aliasing
// and write the field arithmetically, which is what the sockaddr/Sockaddr methods below do.
//
// (2) THE STRUCT-PASSING SEAM. Even with the port written, `Bind` hands the kernel
// `unsafe.Pointer(&sa.raw)`. Native `sockaddr_in` is 16 bytes with the address and zero padding
// INLINE; the converted RawSockaddrInet4 holds `Addr [4]byte` / `Zero [8]uint8` as golib
// `array<byte>` MANAGED REFERENCES, so its C# layout is ~24 bytes with object references where
// Windows expects address octets. golib states the consequence itself, in ж.cs's note on why a
// reference-bearing pointee has no pinnable storage: "such a value's C# layout is not a native
// layout either, so no syscall can meaningfully be handed its address."
//
// The remedy is the established one -- a blittable [StructLayout(LayoutKind.Sequential)] mirror
// with `fixed` buffers for the inline arrays and an explicit field-for-field copy at the boundary,
// worked out for GetTimeZoneInformation / findFirstFile1 / Process32First in the sibling
// zsyscall_windows_impl.cs. Two things differ here and are worth stating:
//
//   - The mirror is a LOCAL at the call site, never a field. A sockaddr's native image is needed
//     only for the duration of one call, and a stack buffer is trivially stable for exactly that
//     long -- where a managed field's address would need a pin whose lifetime nothing owns.
//   - No new [LibraryImport] is declared. Because golib models `unsafe.Pointer` as a box over a
//     plain address, the package's OWN generated wrappers (`bind`, `connect`, `connectEx`) already
//     accept any address at all -- they were never the broken part. Handing them the mirror's
//     address reuses their existing errno handling verbatim and keeps the hand-owned surface to
//     the layout translation, which is the only thing that was actually wrong.
//     Getsockname/Getpeername are the exception: their generated wrappers take a typed
//     `ж<RawSockaddrAny>` rather than an address, so those two go through the package's Syscall
//     trampoline directly, mirroring the generated wrappers' error handling exactly.
//
// DELIBERATELY NOT COVERED, and each for its own measured reason.
//
//   - WAS deliberately excluded, and is NOW covered: RawSockaddrAny.Sockaddr, the DECODE. It
//     carries the same port alias as the encoders and panics identically wherever it is reached.
//     Hand-owning it was REJECTED at L10 on measurement, not on effort: the only casts of the three
//     Sockaddr types to ΔSockaddr in the package lived in ITS body, so skipping its emission dropped
//     the `[assembly: GoImplement<…>(Pointer = true)]` records from package_info.cs, and a reconvert
//     of `net` against that shortened package_info showed net minting its own
//     `syscall_SockaddrInet4жΔSockaddr` adapters instead of using syscall's -- the SECOND-IDENTITY
//     regression samePackageImplements.go exists to prevent (reflect and fmt see the wrapper where
//     the value's own type belongs, and a direct-boxed value compares unequal to an adapter-wrapped
//     one). Declaring the records in this file does not help either: a DEPENDENT package's converter
//     run reads package_info.cs, not this file.
//
//     The converter increment L10 named as the real answer has since landed --
//     recordSamePackageImplements records the POINTER method set as well as the value one, so the
//     three records are sourced from types.Implements(*T, Sockaddr) and survive the body's
//     suppression. Re-measured on this lane's own build before the decode was taken (see the method
//     below): records present, `net` still referencing syscall's adapters.
//   - WSASendto / wsaSendtoInet4 / wsaSendtoInet6 -- the UDP send path -- passed the address
//     returned by `sockaddr()`, which for the reasons above is not a native image. All three are
//     ANSWERED now, and by three different routes, which is why this entry stays rather than being
//     deleted: the Inet4/Inet6 pair are hand-owned in internal/syscall/windows (where their
//     linkname declarations live) and `syscall`'s own copies of them are MEASURED DEAD -- no call
//     site anywhere in the corpus, guarded by wsaSendtoNoCallers_test.go; WSASendto itself is
//     hand-owned in the sibling zsyscall_windows_wsa_impl.cs, because it is an overlapped submit and
//     three of its four defects belong to that file's async family. All three consume
//     writeNativeSockaddr below, which is what this note predicted they would need
//     (docs/phase4/DESIGN-windows-udp-send.md).

using System;
using System.Runtime.InteropServices;

// The same two aliases syscall_windows.cs declares for itself -- the declarations replaced below
// are its neighbors and use both. A converted file's aliases are file-scoped, so a hand-owned
// companion restates the ones it needs.
using @unsafe = go.unsafe_package;
using errorspkg = go.errors_package;

// Hand-owned (no syscall_windows_impl.go exists, so a reconvert never regenerates this file);
// marked for consistency with the other hand-owned operational files in this package. The
// declarations it replaces are registered in the converter's manualConversionFuncs, which is what
// turns their generated bodies into placeholders.
[module: go.GoManualConversion]

// The blittable mirrors below need `fixed` buffers, and the encode/decode helpers take raw
// pointers into stack buffers. Declared rather than inherited -- see zsyscall_windows_impl.cs.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // sockaddr_storage is the largest address any of these calls can carry (128 bytes); every
    // encode and decode below works in a buffer of this size, so one constant covers the stack
    // allocations and the `addrlen` the kernel is told it has.
    private const int nativeSockaddrLen = 128;

    // sockaddr_in exactly as Windows lays it out: 16 bytes, the address and the trailing pad
    // INLINE. `fixed` is what keeps them inline -- a C# array field would be another managed
    // reference, which is the whole bug.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSockaddrInet4
    {
        public uint16 Family;
        public uint16 Port;             // network byte order
        public fixed byte Addr[4];
        public fixed byte Zero[8];
    }

    // sockaddr_in6: 28 bytes, with the 16-byte address inline between the flow info and scope id.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSockaddrInet6
    {
        public uint16 Family;
        public uint16 Port;             // network byte order
        public uint32 Flowinfo;
        public fixed byte Addr[16];
        public uint32 Scope_id;
    }

    // Go stores the port as the two bytes `p[0] = hi, p[1] = lo` -- i.e. network byte order IN
    // MEMORY -- so a little-endian load of that field is the byte-SWAPPED port, which is exactly
    // what sockaddr_in.sin_port carries on the wire. The swap is its own inverse, so encode and
    // decode share it.
    private static uint16 swapBytes(uint16 value) {
        return (uint16)((value >> 8) | (value << 8));
    }

    // (1) THE PORT ALIAS, IPv4. Identical to Go's body except that the port is written to the
    // field instead of through a two-byte alias over it; `raw` is left in exactly the state Go
    // leaves it, so anything that reads it afterwards reads Go's answer.
    internal static (@unsafe.Pointer, int32, error) sockaddr(this ж<SockaddrInet4> Ꮡsa) {
        ref var sa = ref Ꮡsa.Value;

        if (sa.Port < 0 || sa.Port > 0xFFFF) {
            return (default!, 0, EINVAL);
        }

        sa.raw.Family = AF_INET;
        sa.raw.Port = swapBytes((uint16)sa.Port);
        sa.raw.Addr = sa.Addr.Clone();

        // The returned pointer keeps the Go shape and the Go meaning -- the address of `sa.raw`.
        // It is NOT a native image, for the layout reason in the file header, which is why every
        // in-package caller that actually reaches the kernel builds one with writeNativeSockaddr
        // instead of consuming this.
        return (new @unsafe.Pointer(Ꮡsa.of(SockaddrInet4.Ꮡraw)), (int32)16, default!);
    }

    // (1) THE PORT ALIAS, IPv6. See the IPv4 method above.
    internal static (@unsafe.Pointer, int32, error) sockaddr(this ж<SockaddrInet6> Ꮡsa) {
        ref var sa = ref Ꮡsa.Value;

        if (sa.Port < 0 || sa.Port > 0xFFFF) {
            return (default!, 0, EINVAL);
        }

        sa.raw.Family = AF_INET6;
        sa.raw.Port = swapBytes((uint16)sa.Port);
        sa.raw.Scope_id = sa.ZoneId;
        sa.raw.Addr = sa.Addr.Clone();

        return (new @unsafe.Pointer(Ꮡsa.of(SockaddrInet6.Ꮡraw)), (int32)28, default!);
    }

    // Encodes a Sockaddr into the caller's stack buffer as the native sockaddr Windows expects,
    // returning the byte length to pass as `namelen`. Go's own validation and raw-filling logic is
    // reused by calling sockaddr() first -- so there is ONE definition of what a Sockaddr means and
    // this function does nothing but translate the layout, which is the only thing the conversion
    // gets wrong.
    private static unsafe (int32 len, error err) writeNativeSockaddr(ΔSockaddr sa, byte* buffer) {
        // The interface value wraps the receiver box; IжAdapter.Box is how a converted interface
        // hands back the `*T` it holds (see the go2cs-gen ImplementGenerator adapters).
        switch ((sa as IжAdapter)?.Box) {
        case ж<SockaddrInet4> box: {
            var (_, _, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrInet4* native = (NativeSockaddrInet4*)buffer;

            native->Family = raw.Family;
            native->Port = raw.Port;                    // already network order -- see sockaddr

            for (nint i = 0; i < 4; i++) {
                native->Addr[i] = raw.Addr[i];
            }

            for (nint i = 0; i < 8; i++) {
                native->Zero[i] = 0;
            }

            return (16, default!);
        }
        case ж<SockaddrInet6> box: {
            var (_, _, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrInet6* native = (NativeSockaddrInet6*)buffer;

            native->Family = raw.Family;
            native->Port = raw.Port;                    // already network order -- see sockaddr
            native->Flowinfo = raw.Flowinfo;
            native->Scope_id = raw.Scope_id;

            for (nint i = 0; i < 16; i++) {
                native->Addr[i] = raw.Addr[i];
            }

            return (28, default!);
        }
        case ж<SockaddrUnix> box: {
            // AF_UNIX needs no mirror STRUCT -- sun_path is just bytes following the family -- but
            // it does need the same copy, and its length is the one Go computed (which encodes the
            // abstract-socket and unnamed-socket conventions).
            var (_, sl, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;

            *(uint16*)buffer = raw.Family;

            for (nint i = 0; i < (nint)sl - 2; i++) {
                buffer[2 + i] = (byte)raw.Path[i];
            }

            return (sl, default!);
        }
        default:
            return (0, EAFNOSUPPORT);
        }
    }

    // Decodes the native sockaddr the kernel just wrote into the Sockaddr the Go caller expects.
    // The inverse of writeNativeSockaddr, and the one definition of that decode: Getsockname,
    // Getpeername and RawSockaddrAny.Sockaddr all land here.
    private static unsafe (ΔSockaddr, error) readNativeSockaddr(byte* buffer, int32 len) {
        uint16 family = *(uint16*)buffer;

        if (family == AF_INET) {
            NativeSockaddrInet4* native = (NativeSockaddrInet4*)buffer;
            var sa = @new<SockaddrInet4>();

            sa.Value.Port = (nint)swapBytes(native->Port);

            var addr = new array<byte>(4);

            for (nint i = 0; i < 4; i++) {
                addr[i] = native->Addr[i];
            }

            sa.Value.Addr = addr;

            return (new SockaddrInet4жΔSockaddr(sa), default!);
        }

        if (family == AF_INET6) {
            NativeSockaddrInet6* native = (NativeSockaddrInet6*)buffer;
            var sa = @new<SockaddrInet6>();

            sa.Value.Port = (nint)swapBytes(native->Port);
            sa.Value.ZoneId = native->Scope_id;

            var addr = new array<byte>(16);

            for (nint i = 0; i < 16; i++) {
                addr[i] = native->Addr[i];
            }

            sa.Value.Addr = addr;

            return (new SockaddrInet6жΔSockaddr(sa), default!);
        }

        if (family == AF_UNIX) {
            var sa = @new<SockaddrUnix>();
            // sun_path runs from offset 2 to the reported length; Go rewrites a leading NUL as '@'
            // for textual display of an abstract socket, and otherwise stops at the first NUL.
            nint pathMax = (nint)len - 2;

            if (pathMax > (nint)UNIX_PATH_MAX) {
                pathMax = (nint)UNIX_PATH_MAX;
            }

            nint n = 0;

            while (n < pathMax && buffer[2 + n] != 0) {
                n++;
            }

            if (n == 0 && pathMax > 0 && buffer[2] == 0) {
                // Abstract socket: leading NUL displayed as '@', then the name up to the length.
                var abstractName = new array<byte>(pathMax);
                abstractName[0] = (byte)'@';

                nint m = 1;

                while (m < pathMax && buffer[2 + m] != 0) {
                    abstractName[m] = buffer[2 + m];
                    m++;
                }

                sa.Value.Name = ((@string)@unsafe.Slice(Ꮡ(abstractName, 0), m));

                return (new SockaddrUnixжΔSockaddr(sa), default!);
            }

            var name = new array<byte>(n);

            for (nint i = 0; i < n; i++) {
                name[i] = buffer[2 + i];
            }

            sa.Value.Name = ((@string)@unsafe.Slice(Ꮡ(name, 0), n));

            return (new SockaddrUnixжΔSockaddr(sa), default!);
        }

        return (default!, EAFNOSUPPORT);
    }

    // (2) THE STRUCT-PASSING SEAM. Bind/Connect/ConnectEx each build the native image in a stack
    // buffer and hand its address to the package's own generated wrapper, which already does the
    // right thing with an address (see the file header) -- so the errno handling, the trap lookup
    // and the call shape all stay exactly where the converter put them.
    public static unsafe error /*err*/ Bind(ΔHandle fd, ΔSockaddr sa) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        var (n, err) = writeNativeSockaddr(sa, buffer);

        if (err != default!) {
            return err;
        }

        return bind(fd, new @unsafe.Pointer((uintptr)(void*)buffer), n);
    }

    public static unsafe error /*err*/ Connect(ΔHandle fd, ΔSockaddr sa) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        var (n, err) = writeNativeSockaddr(sa, buffer);

        if (err != default!) {
            return err;
        }

        return connect(fd, new @unsafe.Pointer((uintptr)(void*)buffer), n);
    }

    // `ip_mreq` is two INLINE in_addr -- 8 bytes, `imr_multiaddr` then `imr_interface`.
    //
    // The same struct-passing seam as the sockaddrs above, reached from the WRITE side. Converted,
    // `IPMreq` holds `Multiaddr [4]byte` / `Interface [4]byte` as golib `array<byte>` MANAGED
    // REFERENCES, and the generated wrapper handed those to the kernel through
    // `Ꮡmreq.Reinterpret<IPMreq, byte>()`. That reinterpret cannot alias: golib's
    // ReinterpretAliasesStorage refuses a reference-bearing pointee precisely so it never fabricates
    // one, so it falls to the address route and setsockopt receives eight bytes that are two OBJECT
    // REFERENCES rather than two addresses. Windows answers WSAEINVAL, which surfaces on every
    // IP_ADD_MEMBERSHIP as `setsockopt: The requested address is not valid in its context` --
    // net's TestIPv4MulticastListener.
    //
    // No mirror STRUCT is needed, unlike GetTimeZoneInformation's: the option's whole native image
    // is eight bytes of two four-byte fields, so the stack buffer IS the layout. A LOCAL buffer for
    // the duration of one call, never a field, for the reason stated in this file's header.
    //
    // SetsockoptIPv6Mreq is deliberately NOT taken: Go returns EWINDOWS there, so there is no
    // behaviour to preserve and a "fix" would invent one.
    public static unsafe error /*err*/ SetsockoptIPMreq(ΔHandle fd, nint level, nint opt, ж<IPMreq> Ꮡmreq) {
        byte* buffer = stackalloc byte[nativeIPMreqLen];

        // A nil *IPMreq reaches the kernel as a zeroed option in Go's own wrapper too (it derefs
        // into the same eight bytes), so the zeroed stack buffer is the faithful image, not a
        // special case.
        if (Ꮡmreq is not null && !Ꮡmreq.IsNilPointer) {
            ref var mreq = ref Ꮡmreq.Value;

            for (nint i = 0; i < 4; i++) {
                buffer[i] = mreq.Multiaddr[i];
                buffer[4 + i] = mreq.Interface[i];
            }
        }

        return Setsockopt(fd, (int32)level, (int32)opt, (ж<byte>)(uintptr)(void*)buffer, nativeIPMreqLen);
    }

    // sizeof(struct ip_mreq): two in_addr, no padding.
    private const int nativeIPMreqLen = 8;

    // ConnectEx is the DIAL half of the submit seam and was hand-owned here (for the sockaddr layout
    // alone) before that seam existed. Two things changed when it landed.
    //
    // The overlapped is now the OPERATION RECORD's native control block rather than the caller's
    // `&o.o` -- for exactly the reasons zsyscall_windows_wsa_impl.cs's header gives, and with the
    // same consequence: execIO's CancelIoEx and WSAGetOverlappedResult resolve to this same address,
    // because all three name one record. ConnectEx is internal/poll's WRITE operation (it runs on
    // fd.wop), which is the mode the completion signals.
    //
    // The generated `connectEx` below is bypassed for the same reason Getsockname bypasses its
    // wrapper: it takes a typed ж<Overlapped>, and a native control block is not one. The error
    // handling is that wrapper's, verbatim (r1 == 0 -> e1, else EINVAL).
    //
    // THE TWO OPTIONAL BUFFER ARGUMENTS ARE RETAINED ON THE OPERATION RECORD, not KeepAlive'd. Both
    // cross as `(uintptr)<box>`, which pins managed storage for that BOX's lifetime and no longer
    // (golib's m_pin is a GCHandle the box owns and its finalizer frees), and nothing else holds
    // them: a hand-own receives none of convSyscallFunnelCall's `var ᴋN`/`GC.KeepAlive` emission,
    // because the converter drops a [module: go.GoManualConversion] file from the convert set. A
    // KeepAlive after the call would be the wrong closure anyway -- ConnectEx's lpOverlapped may not
    // be NULL, so the send is asynchronous and the kernel reads lpSendBuffer AFTER this returns.
    // rearmOverlapped therefore takes both boxes and parks them in the record's m_pins for the
    // flight, exactly as stageBuffers does for WSARecv/WSASend.
    //
    // internal/poll passes nil for both today (fd_windows.cs's ConnectEx submits `nil, 0, nil`), so
    // 0 crosses and the pins are skipped; net's socktest hook forwards whatever it is given, and the
    // wrapper is public API besides. The retention is what makes a non-nil caller sound rather than
    // lucky -- it is not a fix for anything the corpus exercises today, and is stated as such.
    public static unsafe error ConnectEx(ΔHandle fd, ΔSockaddr sa, ж<byte> ᏑsendBuf, uint32 sendDataLen, ж<uint32> ᏑbytesSent, ж<Overlapped> Ꮡoverlapped) {
        var err = LoadConnectEx();

        if (err != default!) {
            return errorspkg.New("failed to find ConnectEx: "u8 + err.Error());
        }

        byte* buffer = stackalloc byte[nativeSockaddrLen];
        int32 n;
        (n, err) = writeNativeSockaddr(sa, buffer);

        if (err != default!) {
            return err;
        }

        uintptr overlapped = rearmOverlapped(fd, Ꮡoverlapped, wsaModeWrite, ᏑsendBuf, ᏑbytesSent);

        var (r1, _, e1) = Syscall9(connectExFunc.addr, 7, (uintptr)fd, (uintptr)(void*)buffer, (uintptr)n, (uintptr)ᏑsendBuf, (uintptr)sendDataLen, (uintptr)ᏑbytesSent, overlapped, 0, 0);

        if (r1 == 0) {
            if (e1 != 0) {
                return ((error)e1);
            }

            return EINVAL;
        }

        return default!;
    }

    // THE DECODE, and the third consumer readNativeSockaddr was written for. Go reinterprets the
    // RawSockaddrAny as a RawSockaddrInet4/6/Unix and then reads the port through the SAME two-byte
    // alias the encoders write it through -- so the auto conversion panics identically
    // (`index out of range [0] with length 0`), and net's ACCEPT path is the one route that reaches
    // it: netFD.accept decodes the GetAcceptExSockaddrs output with it
    // (net/windows/fd_windows.cs:255-256). Getsockname/Getpeername above never go near it.
    //
    // Neither the alias NOR the reinterpret survives the boundary, and the second is the deeper
    // reason this is hand-owned rather than patched. `Ꮡrsa.Reinterpret<RawSockaddrAny,
    // RawSockaddrInet4>()` asks golib to alias one reference-bearing struct as another, which it
    // correctly refuses (the two managed layouts share no field offsets at all -- RawSockaddrAny
    // holds an int8[14] and an int8[100] object reference where sockaddr_in has four inline octets).
    // So the decode is written the only way that is true on both sides: FLATTEN the managed struct
    // back to the 116-byte native image its fields are a transcription of, and hand that to the one
    // definition of the decode. The mapping is the Go declaration's own -- Family at 0, Addr.Data
    // covering 2..15, Pad covering 16..115 -- and nothing else in the corpus knows it, which is why
    // it is spelled out here rather than derived at the call site.
    //
    // WHO FILLS THE MANAGED STRUCT is the other half, and it is the submit seam's: the hand-owned
    // GetAcceptExSockaddrs (zsyscall_windows_wsa_impl.cs) transcribes the kernel's native accept
    // buffer INTO managed RawSockaddrAny values field for field, precisely so this method has a
    // faithful managed image to read. The two are a pair; neither is meaningful alone.
    public static unsafe (ΔSockaddr, error) Sockaddr(this ж<RawSockaddrAny> Ꮡrsa) {
        ref var rsa = ref Ꮡrsa.Value;

        // Go rewrites a leading NUL as '@' IN PLACE for an abstract Unix socket, with its own note
        // that "the callers below don't care" -- reproduced anyway so the observable state of the
        // caller's struct after the call is Go's, not merely the return value.
        if (rsa.Addr.Family == AF_UNIX && rsa.Addr.Data[0] == 0) {
            rsa.Addr.Data[0] = (int8)'@';
        }

        byte* buffer = stackalloc byte[nativeSockaddrLen];

        flattenRawSockaddr(ref rsa, buffer);

        // The length matters only to the AF_UNIX arm, which scans sun_path for a NUL: Go bounds that
        // scan by len(RawSockaddrUnix.Path), so the equivalent bound here is 2 + UNIX_PATH_MAX.
        return readNativeSockaddr(buffer, (int32)(2 + UNIX_PATH_MAX));
    }

    // THE MANAGED RawSockaddrAny IS A TRANSCRIPTION, and this pair is the whole of what that means.
    //
    // Its fields are a field-for-field record of the 116-byte native sockaddr the kernel reads and
    // writes -- Family at 0, Addr.Data covering 2..15, Pad covering 16..115 (2 + 14 + 100 = 116,
    // which is what Go's unsafe.Sizeof(RawSockaddrAny{}) reports and what internal/poll hard-codes as
    // the AcceptEx per-address length; nativeSockaddrLen, 128, a sockaddr_storage, covers it with
    // room to spare). Family is a plain uint16 in host order on both sides -- the port, inside Data,
    // is the field that is not, which is why neither direction interprets these bytes at all.
    //
    // The mapping is the Go declaration's own, so only ONE function in the corpus states it in each
    // direction. Who USES each: MANAGED->NATIVE (here) serves the decode (Sockaddr above, and through
    // it net's accept path) and the WriteMsg encode; NATIVE->MANAGED is `fillRawSockaddrAny` in the
    // sibling zsyscall_windows_wsa_impl.cs, which the AcceptEx and WSARecvFrom harvests already own
    // and which this file therefore CALLS rather than restates -- they are one partial class, and a
    // second copy of a layout is exactly what this pair exists to prevent. (The first cut of the
    // WriteMsg arc did write a second copy, `unflattenRawSockaddr`; it was folded into
    // fillRawSockaddrAny when the RecvMsg direction needed the bounded form the harvest already had.)
    private static unsafe void flattenRawSockaddr(ref RawSockaddrAny rsa, byte* buffer) {
        *(uint16*)buffer = rsa.Addr.Family;

        for (nint i = 0; i < 14; i++) {
            buffer[2 + i] = (byte)rsa.Addr.Data[i];
        }

        for (nint i = 0; i < 100; i++) {
            buffer[16 + i] = (byte)rsa.Pad[i];
        }
    }

    // Getsockname/Getpeername go through the Syscall trampoline directly rather than their
    // generated wrappers, because those take a typed `ж<RawSockaddrAny>` -- the very managed struct
    // that cannot cross the boundary -- rather than an address. The error handling below mirrors
    // the generated wrappers exactly (`socket_error` result, errnoErr of the trap's errno).
    public static unsafe (ΔSockaddr sa, error err) Getsockname(ΔHandle fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        int32 addrlen = nativeSockaddrLen;

        var (r1, _, e1) = Syscall(procgetsockname.Addr(), 3, (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));

        if (r1 == socket_error) {
            return (default!, errnoErr(e1));
        }

        return readNativeSockaddr(buffer, addrlen);
    }

    public static unsafe (ΔSockaddr sa, error err) Getpeername(ΔHandle fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        int32 addrlen = nativeSockaddrLen;

        var (r1, _, e1) = Syscall(procgetpeername.Addr(), 3, (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));

        if (r1 == socket_error) {
            return (default!, errnoErr(e1));
        }

        return readNativeSockaddr(buffer, addrlen);
    }

    // ---- the datagram seam: what internal/syscall/windows's UDP hand-own consumes ----------------
    //
    // The header above named this exact need and left it: "WSASendto / wsaSendtoInet4 /
    // wsaSendtoInet6 -- the UDP send path -- still pass the address returned by `sockaddr()`, which
    // is not a native image... writeNativeSockaddr is what they would need." A suite has now REACHED
    // them (the UdpLoopbackRoundTrip guard), which is the board's own trigger for fixing a censused
    // wrapper, so this exposes the encode rather than duplicating it.
    //
    // Symmetric with syscall/linux/sockaddr_linux_impl.cs's seam and ruled the same way
    // (DESIGN-linux-udp.md ⟨OQ-2⟩): ONE definition of what a Go Sockaddr looks like to the kernel,
    // reachable across the assembly boundary, spelled `Go…` so it reads as go2cs machinery rather
    // than Go API. Typed to the two INET families so the caller carries no layout knowledge.
    public const int GoNativeSockaddrLen = nativeSockaddrLen;

    public static unsafe (int32 len, error err) GoWriteNativeSockaddrInet4(ж<SockaddrInet4> sa, byte* buffer) =>
        writeNativeSockaddr(new SockaddrInet4жΔSockaddr(sa), buffer);

    public static unsafe (int32 len, error err) GoWriteNativeSockaddrInet6(ж<SockaddrInet6> sa, byte* buffer) =>
        writeNativeSockaddr(new SockaddrInet6жΔSockaddr(sa), buffer);

    // ---- the RAW-SOCKADDR ENCODE: internal/poll's WriteMsg seam, and Sockaddr's exact inverse -----
    //
    // internal/poll's sockaddrInet4ToRaw / sockaddrInet6ToRaw (fd_windows.go) fill a CALLER-OWNED
    // RawSockaddrAny by the same two mechanisms rawToSockaddrInet4/6 read one back through, run in
    // reverse -- and writing is by far the worse direction:
    //
    //     raw := (*syscall.RawSockaddrInet6)(unsafe.Pointer(rsa))
    //     raw.Family = syscall.AF_INET6
    //     p := (*[2]byte)(unsafe.Pointer(&raw.Port))
    //
    // The reinterpret aliases one reference-bearing struct as another whose managed layout shares no
    // offset with it, so `raw.Value.Family = AF_INET6` deposits a uint16 over the LOW HALF OF A LIVE
    // OBJECT REFERENCE; the two-byte view then fabricates an `array<byte>` out of the bytes that
    // follow. Reading the same wrong offsets merely returns a wrong answer -- writing them corrupts
    // the heap, which is why this pair killed the whole `net` host rather than failing a test.
    //
    // The remedy is the one the DECODE already uses, run the other way: build the native image
    // through the single definition of it (writeNativeSockaddr), then transcribe that image into the
    // managed struct's own field encoding (unflattenRawSockaddr). No caller learns a layout, and the
    // encode and decode cannot drift because they name the same two helpers.
    //
    // Typed to the two INET families and taking Go FIELDS rather than a struct, for the same reason
    // GoWriteNativeSockaddrInet4/6 above are typed: internal/poll's hand-own then reads exactly the
    // fields its decode sibling writes (Port, Addr, ZoneId) and carries no layout knowledge at all.
    public static unsafe int32 GoRawSockaddrFromInet4(ж<RawSockaddrAny> Ꮡrsa, nint port, array<byte> addr) {
        var sa = @new<SockaddrInet4>();

        sa.Value.Port = port;
        sa.Value.Addr = addr.Clone();

        return writeRawSockaddr(Ꮡrsa, new SockaddrInet4жΔSockaddr(sa));
    }

    public static unsafe int32 GoRawSockaddrFromInet6(ж<RawSockaddrAny> Ꮡrsa, nint port, uint32 zoneId, array<byte> addr) {
        var sa = @new<SockaddrInet6>();

        sa.Value.Port = port;
        sa.Value.ZoneId = zoneId;
        sa.Value.Addr = addr.Clone();

        return writeRawSockaddr(Ꮡrsa, new SockaddrInet6жΔSockaddr(sa));
    }

    // The flatten, exposed. internal/syscall/windows's WSASendMsg hand-own is handed a RawSockaddrAny
    // that internal/poll has ALREADY encoded (through the two seams above), so what it needs is not
    // another encode but the native image of a record it did not build -- the same flatten the decode
    // performs, reached across the assembly boundary. The caller supplies its own `namelen`: the
    // record is 116 bytes but the family's image inside it is 16 or 28, and only the record's owner
    // knows which, so reporting a length here would invite the wrong one to be used.
    public static unsafe void GoWriteNativeRawSockaddr(ж<RawSockaddrAny> Ꮡrsa, byte* buffer) =>
        flattenRawSockaddr(ref Ꮡrsa.Value, buffer);

    // The transcription, exposed -- the mirror of the seam above and what internal/syscall/windows's
    // WSARecvMsg harvest owes the caller. `available` is what the KERNEL wrote, not what the record
    // can hold: a sockaddr_in is 16 bytes inside a 116-byte record, and reading past the write would
    // transcribe whatever the staging held from a previous receive. The bound is the same one
    // WSARecvFrom's own harvest passes, because this IS that function.
    public static unsafe void GoReadNativeRawSockaddr(ж<RawSockaddrAny> Ꮡrsa, byte* native, nint available) =>
        fillRawSockaddrAny(Ꮡrsa, native, available);

    // The 116-byte record length, exposed for a caller that must tell the kernel how much room the
    // address has and then clamp what came back. GoNativeSockaddrLen (128, a sockaddr_storage) is the
    // BUFFER size; this is the size of the Go struct that buffer is transcribed into, and conflating
    // them would let a 128-byte write land in a 116-byte record.
    public const int GoRawSockaddrAnyLen = rawSockaddrAnyLen;

    private static unsafe int32 writeRawSockaddr(ж<RawSockaddrAny> Ꮡrsa, ΔSockaddr sa) {
        ref var rsa = ref Ꮡrsa.Value;

        // Go's own first line in both encoders -- `*rsa = syscall.RawSockaddrAny{}` -- and it is
        // load-bearing rather than tidy: the family's image is 16 or 28 bytes and the trailing
        // 100-or-88 must read as zero to whoever hands the record to the kernel.
        rsa = new RawSockaddrAny(nil);

        byte* buffer = stackalloc byte[nativeSockaddrLen];

        // Zeroed explicitly rather than relying on the JIT's locals-init, because the transcribe
        // below reads all 116 bytes while the encode writes only the family's own 16 or 28.
        for (nint i = 0; i < nativeSockaddrLen; i++) {
            buffer[i] = 0;
        }

        var (len, err) = writeNativeSockaddr(sa, buffer);

        if (err != default!) {
            // Go's encoders have no error return -- their only failure mode, a port outside
            // 0..65535, is unreachable from every caller (a netip.AddrPort or *UDPAddr port is a
            // uint16 by construction). A zero LENGTH is the honest answer that keeps the signature,
            // and it leaves `rsa` zeroed, which is exactly the state Go's first line put it in.
            return 0;
        }

        // The whole record: the encode zeroed the buffer above, so the trailing bytes transcribe as
        // the zeros Go's `*rsa = RawSockaddrAny{}` put there.
        fillRawSockaddrAny(Ꮡrsa, buffer, rawSockaddrAnyLen);

        // Go returns int32(unsafe.Sizeof(*raw)) -- 16 for sockaddr_in, 28 for sockaddr_in6 -- which
        // is the same number writeNativeSockaddr reports as the image it just wrote.
        return len;
    }
}
