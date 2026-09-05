// sockaddr_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the SOCKET-ADDRESS surface on the DARWIN flavor -- the sockaddr
// family of the syscall STRUCT-PASSING seam, the darwin TWIN of the linux mirror
// (syscall/linux/sockaddr_linux_impl.cs, whose header carries the class's write-up and its
// measured history; the windows original is syscall/windows/syscall_windows_impl.cs, L10). It is
// darwin increment 8 (2026-09-05, Q56 §2.1's door): the five behavioral net rows -- NetDeadlineMatrix,
// NetListenSmoke, TcpLoopbackRoundTrip, UdpLoopbackRoundTrip, UdpWriteMsgAddrPort -- died on BOTH
// mac legs with the SAME AccessViolation in SockaddrInet4.sockaddr() <- Bind (exit 134, five
// SIGABRT reports on x64, measured at train 27), before any socket call reached the kernel.
//
// THE SAME TWO DEFECTS, ON DARWIN, WITH ONE LAYOUT DIFFERENCE THAT IS THE WHOLE REASON THIS IS A
// SEPARATE FILE RATHER THAN A COPY.
//
// (1) THE PORT ALIAS. syscall_bsd.go writes the port in network byte order through a two-byte
// alias over the raw struct's port field -- `p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))` -- and
// anyToSockaddr reads it back through the same alias. The auto conversion of that alias is
// `(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(...))`, an `array<T>` reconstructed from a raw
// address, which is a LENGTH-ZERO array, so `p[0]` dies. Under Q44's token the address is not even
// an address. The remedy is the mirror's: write and read the field arithmetically.
//
// (2) THE STRUCT-PASSING SEAM. `Bind`/`Connect`/`Sendto` hand libc `unsafe.Pointer(&sa.raw)`, and
// `accept`/`getsockname`/`getpeername`/`recvfrom` hand it `&rsa` for the kernel to FILL. The BSD
// sockaddr_in is 16 bytes with the address and zero padding INLINE; the converted RawSockaddrInet4
// holds `Addr [4]byte` / `Zero [8]int8` as golib `array<>` MANAGED REFERENCES, RawSockaddrAny holds
// `Data [14]int8` + `Pad [92]int8` the same way, so neither has a native layout and no address of
// either means anything to the kernel. The remedy is the class's established one: a blittable
// mirror in a STACK buffer for the duration of one call, an explicit field copy in each direction.
//
// THE BSD LAYOUT, which the linux file cannot be copied across: every BSD sockaddr opens with a
// one-byte LENGTH (`sa_len`) and a one-byte FAMILY where linux has a two-byte family --
//   sockaddr_in     16: Len, Family, Port, Addr[4], Zero[8]
//   sockaddr_in6    28: Len, Family, Port, Flowinfo, Addr[16], Scope_id
//   sockaddr_un    106: Len, Family, Path[104]
//   sockaddr_dl     20: Len, Family, Index, Type, Nlen, Alen, Slen, Data[12]
//   sockaddr_any   108: {Len, Family, Data[14]} + Pad[92]        (SizeofSockaddrAny = 0x6c)
// -- and Go's darwin encoders SET sa_len (`sa.raw.Len = SizeofSockaddrInet4`) and return it as the
// socklen, its decoders READ it (the AF_UNIX arm bounds the path scan by pp.Len, not by addrlen),
// and Accept treats a ZERO returned length as ECONNABORTED (an xnu quirk Go names in its own body).
// Every one of those clauses is reproduced below against the native image, arm for arm.
//
// WHAT IS COVERED -- the TCP listen/dial/accept path and the UDP pair, the linux mirror's set
// translated to BSD calls, cut together rather than as they are reached because the five rows'
// paths are KNOWN (Listen -> Bind/Getsockname; Dial -> Connect; Accept -> anyToSockaddr; the UDP
// rows -> Sendto/Recvfrom and the msghdr pair through internal/syscall/unix's linkname helpers):
//   - the two INET encoders (the port alias; sa_len set as Go sets it);
//   - Bind / Connect / Sendto: build the native image with writeNativeSockaddr and hand ITS
//     address to the package's OWN generated `bind`/`connect`/`sendto`, which take an
//     unsafe.Pointer and dispatch it as a register through the keystone -- never the broken part;
//   - Getsockname / Getpeername / Accept / Recvfrom: their generated wrappers take a typed
//     `ж<RawSockaddrAny>`, so these call the libc trampolines directly (`syscall`/`rawSyscall`/
//     `syscall6` with `abi.FuncPCABI0(libc_x_trampoline)`, exactly the generated wrappers' own
//     shape) with a stack buffer, and decode with readNativeSockaddr;
//   - anyToSockaddr, the decode Go's darwin sources declare as a free function: FLATTEN the managed
//     RawSockaddrAny back to the 108-byte native image its fields transcribe (Len at 0, Family at
//     1, Data at 2..15, Pad at 16..107) and hand that to the one decode;
//   - recvmsgRaw and SendmsgN, the ANCILLARY pair, over a darwin msghdr (48 bytes: Name 8,
//     Namelen 4, pad 4, Iov 8, Iovlen 4, pad 4, Control 8, Controllen 4, Flags 4 -- Go's
//     SizeofMsghdr 0x30, and the two pads are REAL) -- with the BSD dummy-byte rule kept as Go's
//     darwin body has it: UNCONDITIONAL, no SOCK_DGRAM check (that check is linux's);
//   - the `Go…` seams the darwin companion in internal/syscall/unix consumes (its eight linkname
//     helpers cannot reach this package's trampolines from another assembly, and darwin has no
//     syscall NUMBERS to call by): GoRecvfromNative / GoSendtoNative / GoRecvmsgNative /
//     GoSendmsgNative beside the mirror's GoWrite/GoReadNativeSockaddrInet4/6.
//
// DELIBERATELY NOT COVERED, named: SockaddrUnix.sockaddr and SockaddrDatalink.sockaddr stay AUTO
// (this file transcribes both natively in writeNativeSockaddr, which calls them for Go's own
// validation and length rules; the address they return is never handed to the kernel by a covered
// wrapper); sendmsgN / sendmsgNInet4/6 / sendtoInet4/6 / recvfromInet4/6 / recvmsgInet4/6 stay auto
// for the reason the linux file records -- internal/poll reaches the //go:linkname copies in
// internal/syscall/unix, and with SendmsgN/Sendto/Recvfrom hand-owned here nothing calls them.
//
// THE DEPENDENCY, STATED UP FRONT AND PREDICTED RATHER THAN HOPED: this file moves the socket wall,
// it does not open the gate. With increment 7 (libcCall returns the C result) kqueue() answers a real
// descriptor, so once Bind succeeds net's listenStream reaches fd.init -> netpollGenericInit ->
// netpollinit -> kevent(kq, &ev, ...) -- a //go:cgo_unsafe_args &first site (shape (a), the stale
// registers Q56 §1 censused) -- and Go's own netpollinit throws `runtime: kevent failed`. That is
// the next wall this twin is predicted to expose on both mac legs; the lift (DESIGN-cgo-unsafe-args-
// block-lift.md) or a kevent mirror is what opens it.

using System;
using System.Runtime.InteropServices;

// The aliases the converted neighbours declare for themselves -- a converted file's aliases are
// file-scoped, so a hand-owned companion restates the ones it uses.
using @unsafe = go.unsafe_package;
using abi = go.@internal.abi_package;

// Hand-owned (no sockaddr_darwin_impl.go exists, so a reconvert never regenerates this file); the
// declarations it replaces are registered in the converter's manualConversionFuncs: the encoders and
// Bind/Connect/Getsockname/Getpeername under the any-flavor scope (each flavor's file the authority
// on its own body), anyToSockaddr/Recvfrom/Sendto/recvmsgRaw/SendmsgN under linux+darwin, and Accept
// under darwin alone (Go's darwin sources declare it in syscall_bsd.go; linux's is pure Go over
// Accept4 and stays auto).
[module: go.GoManualConversion]
// The blittable mirrors need `fixed` buffers and the helpers take raw pointers into stack buffers.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // sockaddr_storage is the largest address any of these calls can carry (128 bytes on darwin
    // too); every encode and decode below works in a buffer of this size, so one constant covers
    // the stack allocations and the `addrlen` the kernel is told it has.
    private const int nativeSockaddrLen = 128;

    // sockaddr_in exactly as darwin lays it out: 16 bytes, sa_len first, the address and the trailing
    // pad INLINE. `fixed` is what keeps them inline -- a C# array field would be another managed
    // reference, which is the whole bug.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSockaddrInet4
    {
        public byte Len;
        public byte Family;
        public uint16 Port;             // network byte order
        public fixed byte Addr[4];
        public fixed byte Zero[8];
    }

    // sockaddr_in6: 28 bytes, with the 16-byte address inline between the flow info and scope id.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSockaddrInet6
    {
        public byte Len;
        public byte Family;
        public uint16 Port;             // network byte order
        public uint32 Flowinfo;
        public fixed byte Addr[16];
        public uint32 Scope_id;
    }

    // sockaddr_dl: 20 bytes, the 12-byte link-level data inline at the end.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSockaddrDatalink
    {
        public byte Len;
        public byte Family;
        public uint16 Index;
        public byte Type;
        public byte Nlen;
        public byte Alen;
        public byte Slen;
        public fixed byte Data[12];
    }

    // msghdr and iovec exactly as darwin lays them out on amd64/arm64 -- the ancillary seam's
    // mirrors. Go's own SizeofMsghdr is 0x30 (48), reproduced field for field: 8 Name + 4 Namelen
    // + 4 pad + 8 Iov + 4 Iovlen + 4 pad + 8 Control + 4 Controllen + 4 Flags. The two pads are
    // Go's Pad_cgo_0 / Pad_cgo_1 and they are REAL -- the kernel reads the fields after them at
    // their padded offsets. Every pointer here is a RAW pointer: the converted Msghdr holds
    // `ж<byte> Name` / `ж<Iovec> Iov` / `ж<byte> Control`, OBJECT REFERENCES, and a struct holding a
    // reference gets AUTO layout from the CLR besides -- so handing the kernel that struct's
    // address makes it read heap addresses, in the wrong order, as user pointers.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeIovec
    {
        public byte* Base;
        public nuint Len;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeMsghdr
    {
        public byte* Name;
        public uint32 Namelen;
        public uint32 Pad0;
        public NativeIovec* Iov;
        public int32 Iovlen;
        public int32 Pad1;
        public byte* Control;
        public uint32 Controllen;
        public int32 Flags;
    }

    // Go stores the port as the two bytes `p[0] = hi, p[1] = lo` -- network byte order IN MEMORY --
    // so a little-endian load of that field is the byte-SWAPPED port, which is exactly what
    // sin_port carries on the wire. The swap is its own inverse, so encode and decode share it.
    private static uint16 swapBytes(uint16 value) {
        return (uint16)((value >> 8) | (value << 8));
    }

    // (1) THE PORT ALIAS, IPv4. Identical to Go's darwin body except that the port is written to the
    // field instead of through a two-byte alias over it; `raw` is left in exactly the state Go
    // leaves it -- sa_len included -- so anything that reads it afterwards reads Go's answer.
    internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrInet4> Ꮡsa) {
        ref var sa = ref Ꮡsa.Value;

        if (sa.Port < 0 || sa.Port > 0xFFFF) {
            return (default!, 0, EINVAL);
        }

        sa.raw.Len = (uint8)SizeofSockaddrInet4;
        sa.raw.Family = (uint8)AF_INET;
        sa.raw.Port = swapBytes((uint16)sa.Port);
        sa.raw.Addr = sa.Addr.Clone();

        // The returned pointer keeps the Go shape and the Go meaning -- the address of `sa.raw`.
        // It is NOT a native image, for the layout reason in the file header, which is why every
        // in-package caller that actually reaches the kernel builds one with writeNativeSockaddr.
        return (new @unsafe.Pointer(Ꮡsa.of(SockaddrInet4.Ꮡraw)), ((_Socklen)(uint32)sa.raw.Len), default!);
    }

    // (1) THE PORT ALIAS, IPv6. See the IPv4 method above.
    internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrInet6> Ꮡsa) {
        ref var sa = ref Ꮡsa.Value;

        if (sa.Port < 0 || sa.Port > 0xFFFF) {
            return (default!, 0, EINVAL);
        }

        sa.raw.Len = (uint8)SizeofSockaddrInet6;
        sa.raw.Family = (uint8)AF_INET6;
        sa.raw.Port = swapBytes((uint16)sa.Port);
        sa.raw.Scope_id = sa.ZoneId;
        sa.raw.Addr = sa.Addr.Clone();

        return (new @unsafe.Pointer(Ꮡsa.of(SockaddrInet6.Ꮡraw)), ((_Socklen)(uint32)sa.raw.Len), default!);
    }

    // Encodes a Sockaddr into the caller's stack buffer as the native sockaddr darwin expects,
    // returning the byte length to pass as `addrlen`. Go's own validation and raw-filling logic is
    // reused by calling sockaddr() first -- so there is ONE definition of what a Sockaddr means and
    // this function does nothing but translate the layout, which is the only thing the conversion
    // gets wrong.
    private static unsafe (_Socklen len, error err) writeNativeSockaddr(Sockaddr sa, byte* buffer) {
        // The interface value wraps the receiver box; IжAdapter.Box is how a converted interface
        // hands back the `*T` it holds (see the go2cs-gen ImplementGenerator adapters).
        switch ((sa as IжAdapter)?.Box) {
        case ж<SockaddrInet4> box: {
            var (_, sl, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrInet4* native = (NativeSockaddrInet4*)buffer;

            native->Len = raw.Len;
            native->Family = raw.Family;
            native->Port = raw.Port;                    // already network order -- see sockaddr

            for (nint i = 0; i < 4; i++) {
                native->Addr[i] = raw.Addr[i];
            }

            for (nint i = 0; i < 8; i++) {
                native->Zero[i] = 0;
            }

            return (sl, default!);
        }
        case ж<SockaddrInet6> box: {
            var (_, sl, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrInet6* native = (NativeSockaddrInet6*)buffer;

            native->Len = raw.Len;
            native->Family = raw.Family;
            native->Port = raw.Port;                    // already network order -- see sockaddr
            native->Flowinfo = raw.Flowinfo;
            native->Scope_id = raw.Scope_id;

            for (nint i = 0; i < 16; i++) {
                native->Addr[i] = raw.Addr[i];
            }

            return (sl, default!);
        }
        case ж<SockaddrUnix> box: {
            // AF_UNIX needs no mirror STRUCT -- sun_path is just bytes following the two header
            // bytes -- but it does need the same copy, and its length is the one Go computed
            // (`3 + n`: Len, Family, the name and its NUL), which the kernel reads from sa_len.
            var (_, sl, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;

            buffer[0] = raw.Len;
            buffer[1] = raw.Family;

            nint pathBytes = (nint)(uint32)sl - 2;

            for (nint i = 0; i < pathBytes; i++) {
                buffer[2 + i] = (byte)raw.Path[i];
            }

            return (sl, default!);
        }
        case ж<SockaddrDatalink> box: {
            var (_, sl, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrDatalink* native = (NativeSockaddrDatalink*)buffer;

            native->Len = raw.Len;
            native->Family = raw.Family;
            native->Index = raw.Index;
            native->Type = raw.Type;
            native->Nlen = raw.Nlen;
            native->Alen = raw.Alen;
            native->Slen = raw.Slen;

            for (nint i = 0; i < 12; i++) {
                native->Data[i] = (byte)raw.Data[i];
            }

            return (sl, default!);
        }
        default:
            return (0, EAFNOSUPPORT);
        }
    }

    // Decodes the native sockaddr the kernel just wrote into the Sockaddr the Go caller expects --
    // Go's darwin anyToSockaddr, arm for arm, over a native image instead of a reinterpreted managed
    // struct. The one definition of that decode: Getsockname, Getpeername, Accept, Recvfrom and
    // anyToSockaddr all land here. The family is the SECOND byte on BSD and the length the FIRST;
    // Go's AF_UNIX arm bounds its scan by that sa_len, never by addrlen, so `len` is carried for
    // the record and the arms read the image exactly as Go reads the struct.
    private static unsafe (Sockaddr, error) readNativeSockaddr(byte* buffer, _Socklen len, nint capacity) {
        byte family = buffer[1];

        if (family == AF_LINK) {
            NativeSockaddrDatalink* native = (NativeSockaddrDatalink*)buffer;
            var sa = @new<SockaddrDatalink>();

            sa.Value.Len = native->Len;
            sa.Value.Family = native->Family;
            sa.Value.Index = native->Index;
            sa.Value.Type = native->Type;
            sa.Value.Nlen = native->Nlen;
            sa.Value.Alen = native->Alen;
            sa.Value.Slen = native->Slen;

            var data = new array<int8>(12);

            for (nint i = 0; i < 12; i++) {
                data[i] = (int8)native->Data[i];
            }

            sa.Value.Data = data;

            return (new SockaddrDatalinkжSockaddr(sa), default!);
        }

        if (family == AF_UNIX) {
            // Go's own arm, verbatim in its rules: reject a length outside [2, SizeofSockaddrUnix];
            // take n = Len - 2 (the family and length bytes); and stop early at a NUL inside n,
            // because some BSDs count the terminator in sa_len and some do not.
            nint rawLen = buffer[0];

            if (rawLen < 2 || rawLen > SizeofSockaddrUnix) {
                return (default!, EINVAL);
            }

            var sa = @new<SockaddrUnix>();
            nint n = rawLen - 2;

            if (n > capacity - 2) {
                n = capacity - 2;
            }

            for (nint i = 0; i < n; i++) {
                if (buffer[2 + i] == 0) {
                    n = i;
                    break;
                }
            }

            sa.Value.Name = new @string(new ReadOnlySpan<byte>(buffer + 2, (int)n));

            return (new SockaddrUnixжSockaddr(sa), default!);
        }

        if (family == AF_INET) {
            NativeSockaddrInet4* native = (NativeSockaddrInet4*)buffer;
            var sa = @new<SockaddrInet4>();

            sa.Value.Port = (nint)swapBytes(native->Port);

            var addr = new array<byte>(4);

            for (nint i = 0; i < 4; i++) {
                addr[i] = native->Addr[i];
            }

            sa.Value.Addr = addr;

            return (new SockaddrInet4жSockaddr(sa), default!);
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

            return (new SockaddrInet6жSockaddr(sa), default!);
        }

        return (default!, EAFNOSUPPORT);
    }

    // (2) THE STRUCT-PASSING SEAM. Bind/Connect build the native image in a stack buffer and hand
    // its address to the package's own generated wrapper, which already does the right thing with
    // an address (it is dispatched as a register through the keystone) -- so the errno handling and
    // the call shape stay exactly where the converter put them.
    public static unsafe error /*err*/ Bind(nint fd, Sockaddr sa) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        var (n, err) = writeNativeSockaddr(sa, buffer);

        if (err != default!) {
            return err;
        }

        return bind(fd, new @unsafe.Pointer((uintptr)(void*)buffer), n);
    }

    public static unsafe error /*err*/ Connect(nint fd, Sockaddr sa) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        var (n, err) = writeNativeSockaddr(sa, buffer);

        if (err != default!) {
            return err;
        }

        return connect(fd, new @unsafe.Pointer((uintptr)(void*)buffer), n);
    }

    // Getsockname/Getpeername call the libc trampolines directly rather than their generated
    // wrappers, because those take a typed `ж<RawSockaddrAny>` -- the very managed struct that
    // cannot cross the boundary -- rather than an address. The shape below is the generated
    // wrappers' own (rawSyscall over abi.FuncPCABI0 of the same trampoline, errnoErr of the errno);
    // the kernel writes the address into the stack buffer and its length into `addrlen`.
    public static unsafe (Sockaddr sa, error err) Getsockname(nint fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = nativeSockaddrLen;

        var (_, _, e1) = rawSyscall(abi.FuncPCABI0(libc_getsockname_trampoline), (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));

        if (e1 != 0) {
            return (default!, errnoErr(e1));
        }

        return readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);
    }

    public static unsafe (Sockaddr sa, error err) Getpeername(nint fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = nativeSockaddrLen;

        var (_, _, e1) = rawSyscall(abi.FuncPCABI0(libc_getpeername_trampoline), (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));

        if (e1 != 0) {
            return (default!, errnoErr(e1));
        }

        return readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);
    }

    // Accept is Go's own darwin body (syscall_bsd.go) over the trampoline instead of the typed
    // generated wrapper: the kernel fills the stack buffer; a ZERO returned length is the xnu quirk
    // Go closes the socket for and reports as ECONNABORTED; a decode failure closes it too.
    public static unsafe (nint nfd, Sockaddr sa, error err) Accept(nint fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = SizeofSockaddrAny;

        var (r0, _, e1) = syscall(abi.FuncPCABI0(libc_accept_trampoline), (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));
        nint nfd = (nint)r0;

        if (e1 != 0) {
            return (nfd, default!, errnoErr(e1));
        }

        if (addrlen == 0) {
            // Accepted socket has no address. This is likely due to a bug in xnu kernels, where
            // instead of ECONNABORTED error socket is accepted, but has no address.
            Close(nfd);
            return (0, default!, ECONNABORTED);
        }

        var (sa, err) = readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);

        if (err != default!) {
            Close(nfd);
            nfd = 0;
        }

        return (nfd, sa, err);
    }

    // THE DECODE. Go reinterprets the RawSockaddrAny as a RawSockaddrInet4/6/Unix/Datalink and reads
    // the port through the same two-byte alias the encoders write it through, so the auto conversion
    // dies identically; and `Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>()` asks golib to
    // alias one reference-bearing struct as another, which it correctly refuses. So the decode is
    // written the only way that is true on both sides: FLATTEN the managed struct back to the
    // 108-byte native image its fields are a transcription of -- Len at 0, Family at 1, Addr.Data
    // at 2..15, Pad at 16..107 (unsafe.Sizeof(RawSockaddrAny{}) = SizeofSockaddrAny) -- and hand
    // that to the one definition of the decode.
    internal static unsafe (Sockaddr, error) anyToSockaddr(ж<RawSockaddrAny> Ꮡrsa) {
        ref var rsa = ref Ꮡrsa.Value;

        byte* buffer = stackalloc byte[nativeSockaddrLen];

        buffer[0] = rsa.Addr.Len;
        buffer[1] = rsa.Addr.Family;

        for (nint i = 0; i < 14; i++) {
            buffer[2 + i] = (byte)rsa.Addr.Data[i];
        }

        for (nint i = 0; i < 92; i++) {
            buffer[16 + i] = (byte)rsa.Pad[i];
        }

        return readNativeSockaddr(buffer, SizeofSockaddrAny, nativeSockaddrLen);
    }

    // Recvfrom: the kernel writes the sender's address over the stack image (the write-over-managed
    // half of the class -- the linux mirror measured it as a process KILL, AccessViolation in
    // array<int8>'s indexer, when the managed RawSockaddrAny was handed over by address). Go's own
    // shape is reproduced exactly (syscall_unix.go): recvfrom, then decode ONLY when the kernel
    // actually wrote an address family -- `if rsa.Addr.Family != AF_UNSPEC`. The buffer's family
    // byte is cleared first so "the kernel wrote nothing" reads as AF_UNSPEC rather than as whatever
    // the stack happened to hold, which is the same question Go's zero-valued struct answers.
    public static unsafe (nint n, Sockaddr from, error err) Recvfrom(nint fd, slice<byte> p, nint flags) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        uint32 addrlen = nativeSockaddrLen;

        buffer[0] = 0;
        buffer[1] = (byte)AF_UNSPEC;

        var (n, err) = GoRecvfromNative(fd, p, flags, buffer, ref addrlen);

        if (err != default!) {
            return (n, default!, err);
        }

        if (buffer[1] == AF_UNSPEC) {
            return (n, default!, default!);
        }

        var (sa, decodeErr) = readNativeSockaddr(buffer, ((_Socklen)addrlen), nativeSockaddrLen);
        return (n, sa, decodeErr);
    }

    // Sendto is Recvfrom's direction reversed and takes Bind/Connect's shape: the kernel READS the
    // address here, so there is no stack buffer to decode afterwards -- just the native image built
    // once and handed to the package's own generated `sendto`. A nil `to` is NOT an error and must
    // not go through writeNativeSockaddr: Go leaves `ptr` nil and `salen` zero and calls sendto
    // anyway, which is how a datagram goes out on a CONNECTED socket; `@unsafe.Pointer`'s uintptr
    // bridge answers 0 for the default value, so the kernel gets the null address Go sends.
    public static unsafe error /*err*/ Sendto(nint fd, slice<byte> p, nint flags, Sockaddr to) {
        if (to == default!) {
            return sendto(fd, p, flags, default!, 0);
        }

        byte* buffer = stackalloc byte[nativeSockaddrLen];
        var (n, err) = writeNativeSockaddr(to, buffer);

        if (err != default!) {
            return err;
        }

        return sendto(fd, p, flags, new @unsafe.Pointer((uintptr)(void*)buffer), n);
    }

    // ---- the ANCILLARY seam: recvmsgRaw / SendmsgN ------------------------------------------------
    //
    // ONE body each covers the entry points Go funnels through them: recvmsgRaw is called by Recvmsg,
    // recvmsgInet4 and recvmsgInet6; the send side takes the public SendmsgN, for the reason the linux
    // file states -- the raw helper's `ptr` parameter is already the address of a MANAGED raw
    // sockaddr, so only the public function still holds the typed Sockaddr that can be re-encoded.
    //
    // recvmsgRaw keeps its `ж<RawSockaddrAny>` OUT-parameter and FILLS it rather than decoding to a
    // Sockaddr: its callers read `rsa.Addr.Family` and hand `&rsa` to anyToSockaddr, so a faithful
    // drop-in leaves that contract alone. The transcription below is the exact inverse of
    // anyToSockaddr's flatten (Len at 0, Family at 1, Data at 2..15, Pad at 16..107).
    internal static unsafe (nint n, nint oobn, nint recvflags, error err) recvmsgRaw(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<RawSockaddrAny> Ꮡrsa) {
        byte* nameBuf = stackalloc byte[nativeSockaddrLen];

        uint32 nameLen = (uint32)SizeofSockaddrAny;

        var (n, oobn, recvflags, err) = GoRecvmsgNative(fd, p, oob, flags, nameBuf, ref nameLen);

        if (err != default!) {
            return (0, 0, 0, err);
        }

        ref var rsa = ref Ꮡrsa.Value;
        rsa.Addr.Len = nameBuf[0];
        rsa.Addr.Family = nameBuf[1];

        for (nint i = 0; i < 14; i++) {
            rsa.Addr.Data[i] = (int8)nameBuf[2 + i];
        }

        for (nint i = 0; i < 92; i++) {
            rsa.Pad[i] = (int8)nameBuf[16 + i];
        }

        return (n, oobn, recvflags, default!);
    }

    // SendmsgN is recvmsgRaw's direction reversed, and it is the PUBLIC function rather than the raw
    // helper (see above). Go passes NULL for a connected socket, and a connected socket REJECTS a
    // sendmsg carrying an address with EISCONN; the generated body turns that NULL into an object --
    // `(ж<byte>)(uintptr)0` is `new NativeBox<byte>(0)` -- which is the failure the linux row measured
    // control-first (ScmRightsSeam). Here a nil `to` simply leaves msg_name NULL.
    public static unsafe (nint n, error err) SendmsgN(nint fd, slice<byte> p, slice<byte> oob, Sockaddr to, nint flags) {
        byte* nameBuf = null;
        uint32 nameLen = 0;
        byte* buffer = stackalloc byte[nativeSockaddrLen];

        if (to != default!) {
            var (encoded, nameErr) = writeNativeSockaddr(to, buffer);

            if (nameErr != default!) {
                return (0, nameErr);
            }

            nameBuf = buffer;
            nameLen = (uint32)encoded;
        }

        return GoSendmsgNative(fd, p, oob, nameBuf, nameLen, flags);
    }

    // ---- the CROSS-ASSEMBLY seam -------------------------------------------------------------------
    //
    // internal/syscall/unix's eight //go:linkname datagram helpers are what internal/poll reaches,
    // and they live in a DIFFERENT ASSEMBLY that can neither name this package's trampolines nor --
    // on darwin, which has no syscall numbers -- call the kernel by number as the linux companion
    // does. So the four calls they need are exported here as `Go`-prefixed PUBLIC helpers with the
    // native mirrors staying PRIVATE: no native type crosses the assembly line, and there stays
    // exactly ONE definition of what a Go Sockaddr, a msghdr and an iovec look like to the kernel.
    // The two sockaddr pairs are the linux mirror's seam, unchanged in shape.

    // The size every caller's stack buffer must be: sockaddr_storage, which fits every family above.
    public const int GoNativeSockaddrLen = nativeSockaddrLen;

    // IN direction (sendto/sendmsg): managed box -> native image, returning the addrlen to pass.
    public static unsafe (_Socklen len, error err) GoWriteNativeSockaddrInet4(ж<SockaddrInet4> sa, byte* buffer) =>
        writeNativeSockaddr(new SockaddrInet4жSockaddr(sa), buffer);

    public static unsafe (_Socklen len, error err) GoWriteNativeSockaddrInet6(ж<SockaddrInet6> sa, byte* buffer) =>
        writeNativeSockaddr(new SockaddrInet6жSockaddr(sa), buffer);

    // OUT direction (recvfrom/recvmsg): native image -> the caller's OWN box, filled by assignment.
    // Go's recvfromInet4 assigns Port and Addr into the caller's struct and leaves everything else
    // alone; these do the same. A datagram that decodes to another family is a kernel contract
    // violation on an AF_INET socket, and is reported rather than silently ignored.
    public static unsafe error GoReadNativeSockaddrInet4(byte* buffer, _Socklen len, ж<SockaddrInet4> into) {
        var (sa, err) = readNativeSockaddr(buffer, len, (nint)(uint32)len);

        if (err != default!) {
            return err;
        }

        if ((sa as IжAdapter)?.Box is not ж<SockaddrInet4> box) {
            return EAFNOSUPPORT;
        }

        into.Value.Port = box.Value.Port;
        into.Value.Addr = box.Value.Addr;
        return default!;
    }

    public static unsafe error GoReadNativeSockaddrInet6(byte* buffer, _Socklen len, ж<SockaddrInet6> into) {
        var (sa, err) = readNativeSockaddr(buffer, len, (nint)(uint32)len);

        if (err != default!) {
            return err;
        }

        if ((sa as IжAdapter)?.Box is not ж<SockaddrInet6> box) {
            return EAFNOSUPPORT;
        }

        into.Value.Port = box.Value.Port;
        into.Value.ZoneId = box.Value.ZoneId;
        into.Value.Addr = box.Value.Addr;
        return default!;
    }

    // recvfrom over the trampoline with a caller-supplied stack image: the payload travels by pinned
    // slice-element address exactly as the generated wrapper's does (the pin is the BOX's and lasts
    // its reachability, so the box is held across the call), an empty slice offers a valid address
    // of a zero-length region, and the kernel rewrites `addrlen` with what it wrote.
    public static unsafe (nint n, error err) GoRecvfromNative(nint fd, slice<byte> p, nint flags, byte* buffer, ref uint32 addrlen) {
        byte zero = 0;
        ж<byte> ᴋp = default!;
        uintptr payload;

        if (len(p) > 0) {
            ᴋp = Ꮡ(p, 0);
            payload = (uintptr)ᴋp;
        } else {
            payload = (uintptr)(void*)(&zero);
        }

        fixed (uint32* lenp = &addrlen) {
            var (r0, _, e1) = syscall6(abi.FuncPCABI0(libc_recvfrom_trampoline), (uintptr)fd, payload, (uintptr)len(p),
                                       (uintptr)flags, (uintptr)(void*)buffer, (uintptr)(void*)lenp);
            System.GC.KeepAlive(ᴋp);

            if (e1 != 0) {
                return (0, errnoErr(e1));
            }

            return ((nint)r0, default!);
        }
    }

    // sendto over the trampoline with an ALREADY-ENCODED native image of addrlen bytes.
    public static unsafe error GoSendtoNative(nint fd, slice<byte> p, nint flags, byte* addr, _Socklen addrlen) {
        byte zero = 0;
        ж<byte> ᴋp = default!;
        uintptr payload;

        if (len(p) > 0) {
            ᴋp = Ꮡ(p, 0);
            payload = (uintptr)ᴋp;
        } else {
            payload = (uintptr)(void*)(&zero);
        }

        var (_, _, e1) = syscall6(abi.FuncPCABI0(libc_sendto_trampoline), (uintptr)fd, payload, (uintptr)len(p),
                                   (uintptr)flags, (uintptr)(void*)addr, (uintptr)(uint32)addrlen);
        System.GC.KeepAlive(ᴋp);

        if (e1 != 0) {
            return errnoErr(e1);
        }

        return default!;
    }

    // recvmsg with a caller-supplied name image: Go's darwin recvmsgRaw over a native msghdr. The
    // BSD dummy-byte rule is Go's own darwin body and it is UNCONDITIONAL there -- a control-only
    // receive offers one normal byte whatever the socket type (linux's SOCK_DGRAM exception is
    // linux's).
    public static unsafe (nint n, nint oobn, nint recvflags, error err) GoRecvmsgNative(nint fd, slice<byte> p, slice<byte> oob, nint flags, byte* nameBuf, ref uint32 nameLen) {
        byte dummy = 0;

        NativeIovec iov = default;
        NativeMsghdr msg = default;

        // Declared out here rather than inside the blocks below: the pin lasts the BOX's lifetime,
        // and the call is past the end of both blocks.
        ж<byte> ᴋp = default!;
        ж<byte> ᴋoob = default!;

        msg.Name = nameBuf;
        msg.Namelen = nameLen;

        if (len(p) > 0) {
            ᴋp = Ꮡ(p, 0);
            iov.Base = (byte*)(nint)(uintptr)ᴋp;
            iov.Len = (nuint)len(p);
        }

        if (len(oob) > 0) {
            // receive at least one normal byte
            if (len(p) == 0) {
                iov.Base = &dummy;
                iov.Len = 1;
            }

            ᴋoob = Ꮡ(oob, 0);
            msg.Control = (byte*)(nint)(uintptr)ᴋoob;
            msg.Controllen = (uint32)len(oob);
        }

        msg.Iov = &iov;
        msg.Iovlen = 1;

        var (r0, _, e1) = syscall(abi.FuncPCABI0(libc_recvmsg_trampoline), (uintptr)fd, (uintptr)(void*)(&msg), (uintptr)flags);
        System.GC.KeepAlive(ᴋp);
        System.GC.KeepAlive(ᴋoob);

        if (e1 != 0) {
            return (0, 0, 0, errnoErr(e1));
        }

        // The kernel rewrites msg_namelen with what it actually wrote; hand it back so a caller
        // that DECODES the image passes the real length rather than the buffer's capacity.
        nameLen = msg.Namelen;

        return ((nint)r0, (nint)msg.Controllen, (nint)msg.Flags, default!);
    }

    // nameBuf holds an ALREADY-ENCODED native sockaddr image of nameLen bytes, or is null for a
    // connected socket -- Go's nil `to`.
    public static unsafe (nint n, error err) GoSendmsgNative(nint fd, slice<byte> p, slice<byte> oob, byte* nameBuf, uint32 nameLen, nint flags) {
        byte dummy = 0;

        NativeIovec iov = default;
        NativeMsghdr msg = default;

        ж<byte> ᴋp = default!;
        ж<byte> ᴋoob = default!;

        if (nameBuf != null) {
            msg.Name = nameBuf;
            msg.Namelen = nameLen;
        }

        if (len(p) > 0) {
            ᴋp = Ꮡ(p, 0);
            iov.Base = (byte*)(nint)(uintptr)ᴋp;
            iov.Len = (nuint)len(p);
        }

        if (len(oob) > 0) {
            // send at least one normal byte
            if (len(p) == 0) {
                iov.Base = &dummy;
                iov.Len = 1;
            }

            ᴋoob = Ꮡ(oob, 0);
            msg.Control = (byte*)(nint)(uintptr)ᴋoob;
            msg.Controllen = (uint32)len(oob);
        }

        msg.Iov = &iov;
        msg.Iovlen = 1;

        var (r0, _, e1) = syscall(abi.FuncPCABI0(libc_sendmsg_trampoline), (uintptr)fd, (uintptr)(void*)(&msg), (uintptr)flags);
        System.GC.KeepAlive(ᴋp);
        System.GC.KeepAlive(ᴋoob);

        if (e1 != 0) {
            return (0, errnoErr(e1));
        }

        nint n = (nint)r0;

        // Go's own tail (syscall_bsd.go sendmsgN): the byte counted by the kernel on a control-only
        // send is the DUMMY the block above supplies, not the caller's payload.
        if (len(oob) > 0 && len(p) == 0) {
            n = 0;
        }

        return (n, default!);
    }
}
