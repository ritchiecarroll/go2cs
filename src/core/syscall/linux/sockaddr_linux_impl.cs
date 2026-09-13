// sockaddr_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of the SOCKET-ADDRESS surface on the LINUX flavor -- the sockaddr
// family of the syscall STRUCT-PASSING seam, mirrored from the Windows lane's L10 hand-own
// (syscall/windows/syscall_windows_impl.cs, whose header carries the full write-up of the class).
// It is R5 of the 2026-08-22 Linux roster measurement: encoding/json's TestHTTPDecoding and
// crypto/tls's TestMain both died in SockaddrInet4.sockaddr before any socket call was made.
//
// THE SAME TWO DEFECTS, ON LINUX.
//
// (1) THE PORT ALIAS. syscall_linux.go writes the port in network byte order through a two-byte
// alias over the raw struct's port field -- `p := (*[2]byte)(unsafe.Pointer(&sa.raw.Port))` -- and
// anyToSockaddr reads it back through the same alias. The auto conversion of that alias is
// `(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(...))`, and an `array<T>` reconstructed from a raw
// address is a LENGTH-ZERO array, so `p[0]` panics with `index out of range [0] with length 0`
// (golib array.cs via syscall_linux.cs:549 -- the stack the poll-seam lane measured). The remedy is
// the L10 one: write and read the field arithmetically.
//
// (2) THE STRUCT-PASSING SEAM. `Bind`/`Connect` hand the kernel `unsafe.Pointer(&sa.raw)`, and
// `accept4`/`getsockname`/`getpeername` hand it `&rsa` for the kernel to FILL. sockaddr_in is 16
// bytes with the address and zero padding INLINE; the converted RawSockaddrInet4 holds `Addr [4]byte`
// / `Zero [8]uint8` as golib `array<byte>` MANAGED REFERENCES, RawSockaddrAny holds `Data [14]int8`
// + `Pad [96]int8` the same way, so neither has a native layout and no address of either means
// anything to the kernel. The remedy is the class's established one: a blittable mirror in a STACK
// buffer for the duration of one call, an explicit field copy in each direction at the boundary.
//
// WHAT IS COVERED -- the TCP listen/dial/accept path, exactly L10's set plus Linux's own decode:
//   - the two INET encoders (the port alias);
//   - Bind / Connect: build the native image with writeNativeSockaddr and hand its address to the
//     package's OWN generated `bind`/`connect`, which take an `unsafe.Pointer` and were never the
//     broken part -- their errno handling stays where the converter put it;
//   - Getsockname / Getpeername / Accept4: their generated wrappers take a typed `ж<RawSockaddrAny>`,
//     so these go through the Syscall/RawSyscall trampoline directly with a stack buffer, mirroring
//     the generated wrappers' error handling exactly (RawSyscall for the two getters, Syscall6 for
//     accept4, as zsyscall_linux_amd64.cs has them), and decode with readNativeSockaddr;
//   - anyToSockaddr, Linux's decode (Go's is a free function here, the method form is Windows's):
//     FLATTEN the managed RawSockaddrAny back to the 112-byte native image its fields transcribe
//     (Family at 0, Data at 2..15, Pad at 16..111) and hand that to the one decode -- so any
//     remaining auto caller (Recvmsg) decodes correctly once ITS fill is;
//   - Recvfrom (2026-08-30) and Sendto (2026-09-02): the UDP pair, added when each was REACHED,
//     which is the rule this file's scope has followed throughout rather than an exception to it;
//   - recvmsgRaw and SendmsgN (2026-09-02): the ANCILLARY pair, and the one place this file's
//     coverage is ASYMMETRIC between the two directions. That asymmetry is worth stating because
//     it is not a preference and reading the file will not otherwise explain it.
//
//     RECEIVE displaces the RAW helper. recvmsgRaw is called by Recvmsg, recvmsgInet4 and
//     recvmsgInet6, so one body covers three entry points; and it keeps its `ж<RawSockaddrAny>`
//     OUT-parameter and FILLS it rather than decoding to a Sockaddr, because those three callers
//     each read `rsa.Addr.Family` and hand `&rsa` to anyToSockaddr. A faithful drop-in leaves that
//     contract alone, so the transcription back into the managed struct is the exact INVERSE of
//     anyToSockaddr's flatten -- which is what keeps the two in step and why they are adjacent.
//
//     SEND displaces the PUBLIC function, and could not do otherwise. sendmsgN was written first
//     and abandoned: its `ptr` parameter is ALREADY the address of a managed raw sockaddr (whatever
//     `to.sockaddr()` returned), so there is nothing there to transcribe faithfully -- the typed
//     Sockaddr has to be re-encoded through writeNativeSockaddr, and only SendmsgN still holds it.
//     That is Bind/Connect's shape, arrived at by hitting the wall rather than by choosing it.
//     sendmsgN / sendmsgNInet4 / sendmsgNInet6 therefore stay auto for the sendtoInet4/6 reason
//     below: with SendmsgN bypassing it, their only remaining callers are each other.
//
//     The defect they share announces itself unusually loudly for this class. Both hand the kernel
//     a MANAGED Msghdr whose Name/Iov/Control are `ж<T>` object references, and on the send side
//     `msg.Name = (ж<byte>)(uintptr)(ptr)` turns Go's NULL into `new NativeBox<byte>(0)` -- an
//     OBJECT -- so a connected socket answers EISCONN rather than misdirecting silently. On an
//     UNCONNECTED socket the same line sends to a garbage destination, which is the quiet variant.
//     Measured by ScmRightsSeam before either body existed (control-first).
//
// DELIBERATELY NOT COVERED, named rather than left to be rediscovered: Recvmsg and Sendmsg
// themselves need nothing further -- Recvmsg's fill comes from the covered recvmsgRaw and Sendmsg
// delegates to the covered SendmsgN -- so what remains uncovered here is Go's OWN sockaddr()
// encoder METHODS on SockaddrUnix / SockaddrLinklayer / SockaddrNetlink, which stay auto (this
// file transcribes all three natively in writeNativeSockaddr; what stays auto is the Go-side
// method each of them carries). They have no port alias, their raw structs are consumed here
// only through writeNativeSockaddr -- which calls them for Go's own validation and length
// rules -- and the address they return is never handed to the kernel by a covered wrapper.
//
// AND syscall's OWN sendtoInet4 / sendtoInet6 / recvfromInet4 / recvfromInet6 STAY AUTO, which is a
// decision already on the record and worth not re-litigating: internal/syscall/unix/linux/
// net_linux_impl.cs's header censused the linux flavor and found ZERO call sites for them, because
// internal/poll reaches its //go:linkname copies instead. They carry this file's exact defect in
// eleven lines -- and they are dead code, so hand-owning them would be corpus surface for no
// behavior. Sendto is covered here and they are not for one reason only: netlink_linux.cs's
// NetlinkRIB CALLS Sendto, and nothing calls them.
//
// THE DEPENDENCY, MEASURED AND STATED UP FRONT. This file moves the socket wall; it does not open
// the gate. On the Linux flavor a socket is un-armable -- internal/poll/linux/runtime_netpoll_impl.cs
// answers EPERM from runtime_pollOpen for EVERY descriptor, the regular-file fallback applied
// package-wide -- so once Bind/Connect succeed, net's listenStream/dial reach FD.Init -> pollDesc.init
// and return "operation not permitted": an honest error, not a working socket, until a Linux
// readiness poller exists (the separate design DESIGN-netpoll-managed-poller.md §8 names). This
// file is that poller's prerequisite: with it, the first thing the poller will see on Linux is a
// correctly-encoded bind and a correctly-decoded accept.

using System;
using System.Runtime.InteropServices;

// The alias syscall_linux.cs declares for itself -- the declarations replaced below are its
// neighbors. A converted file's aliases are file-scoped, so a hand-owned companion restates it.
using @unsafe = go.unsafe_package;

// Hand-owned (no sockaddr_linux_impl.go exists, so a reconvert never regenerates this file); the
// declarations it replaces are registered in the converter's manualConversionFuncs under the
// windows+linux scope (the encoders, Bind/Connect/Getsockname/Getpeername) and the linux-only scope
// (Accept4, anyToSockaddr), which is what turns their generated bodies into placeholders.
[module: go.GoManualConversion]

// The blittable mirrors need `fixed` buffers and the helpers take raw pointers into stack buffers.
[module: go.GoRequiresUnsafe]

namespace go;

partial class syscall_package
{
    // sockaddr_storage is the largest address any of these calls can carry (128 bytes); every
    // encode and decode below works in a buffer of this size, so one constant covers the stack
    // allocations and the `addrlen` the kernel is told it has.
    private const int nativeSockaddrLen = 128;

    // sockaddr_in exactly as Linux lays it out: 16 bytes, the address and the trailing pad INLINE.
    // `fixed` is what keeps them inline -- a C# array field would be another managed reference,
    // which is the whole bug.
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

    // sockaddr_ll: 20 bytes, the 8-byte hardware address inline at the end.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeSockaddrLinklayer
    {
        public uint16 Family;
        public uint16 Protocol;
        public int32 Ifindex;
        public uint16 Hatype;
        public uint8 Pkttype;
        public uint8 Halen;
        public fixed byte Addr[8];
    }

    // msghdr and iovec exactly as Linux lays them out on amd64 -- the ancillary seam's mirrors.
    // Go's own SizeofMsghdr is 0x38 (56), which this reproduces field for field: 8 Name + 4
    // Namelen + 4 pad + 8 Iov + 8 Iovlen + 8 Control + 8 Controllen + 4 Flags + 4 pad. The two
    // pads are Go's Pad_cgo_0 / Pad_cgo_1 and they are REAL -- the kernel reads the fields after
    // them at their padded offsets, so leaving them out would shift Iov by four bytes.
    //
    // Every pointer here is a RAW pointer, which is the whole point: the converted Msghdr holds
    // `ж<byte> Name` / `ж<Iovec> Iov` / `ж<byte> Control`, i.e. OBJECT REFERENCES, and handing the
    // kernel that struct's address makes it read heap addresses as user pointers. It also makes a
    // NIL stop being nil: `(ж<byte>)(uintptr)0` is `new NativeBox<byte>(0)`, an object, so a Go
    // `msg_name == NULL` arrives non-NULL and a connected socket answers EISCONN. That is the
    // failure ScmRightsSeam measured before this file grew these two structs.
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
        public nuint Iovlen;
        public byte* Control;
        public nuint Controllen;
        public int32 Flags;
        public int32 Pad1;
    }

    // sockaddr_nl: 12 bytes of scalars -- already blittable, mirrored so the layout is stated here.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSockaddrNetlink
    {
        public uint16 Family;
        public uint16 Pad;
        public uint32 Pid;
        public uint32 Groups;
    }

    // Go stores the port as the two bytes `p[0] = hi, p[1] = lo` -- network byte order IN MEMORY --
    // so a little-endian load of that field is the byte-SWAPPED port, which is exactly what
    // sockaddr_in.sin_port carries on the wire. The swap is its own inverse, so encode and decode
    // share it.
    private static uint16 swapBytes(uint16 value) {
        return (uint16)((value >> 8) | (value << 8));
    }

    // (1) THE PORT ALIAS, IPv4. Identical to Go's body except that the port is written to the
    // field instead of through a two-byte alias over it; `raw` is left in exactly the state Go
    // leaves it, so anything that reads it afterwards reads Go's answer.
    internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrInet4> Ꮡsa) {
        ref var sa = ref Ꮡsa.Value;

        if (sa.Port < 0 || sa.Port > 0xFFFF) {
            return (default!, 0, EINVAL);
        }

        sa.raw.Family = AF_INET;
        sa.raw.Port = swapBytes((uint16)sa.Port);
        sa.raw.Addr = sa.Addr.Clone();

        // The returned pointer keeps the Go shape and the Go meaning -- the address of `sa.raw`.
        // It is NOT a native image, for the layout reason in the file header, which is why every
        // in-package caller that actually reaches the kernel builds one with writeNativeSockaddr.
        return (new @unsafe.Pointer(Ꮡsa.of(SockaddrInet4.Ꮡraw)), SizeofSockaddrInet4, default!);
    }

    // (1) THE PORT ALIAS, IPv6. See the IPv4 method above.
    internal static (@unsafe.Pointer, _Socklen, error) sockaddr(this ж<SockaddrInet6> Ꮡsa) {
        ref var sa = ref Ꮡsa.Value;

        if (sa.Port < 0 || sa.Port > 0xFFFF) {
            return (default!, 0, EINVAL);
        }

        sa.raw.Family = AF_INET6;
        sa.raw.Port = swapBytes((uint16)sa.Port);
        sa.raw.Scope_id = sa.ZoneId;
        sa.raw.Addr = sa.Addr.Clone();

        return (new @unsafe.Pointer(Ꮡsa.of(SockaddrInet6.Ꮡraw)), SizeofSockaddrInet6, default!);
    }

    // Encodes a Sockaddr into the caller's stack buffer as the native sockaddr Linux expects,
    // returning the byte length to pass as `addrlen`. Go's own validation and raw-filling logic is
    // reused by calling sockaddr() first -- so there is ONE definition of what a Sockaddr means and
    // this function does nothing but translate the layout, which is the only thing the conversion
    // gets wrong.
    private static unsafe (_Socklen len, error err) writeNativeSockaddr(Sockaddr sa, byte* buffer) {
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

            return (SizeofSockaddrInet4, default!);
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

            return (SizeofSockaddrInet6, default!);
        }
        case ж<SockaddrUnix> box: {
            // AF_UNIX needs no mirror STRUCT -- sun_path is just bytes following the family -- but
            // it does need the same copy, and its length is the one Go computed (which encodes the
            // abstract-socket and unnamed-socket conventions, including the leading NUL it wrote
            // into raw.Path[0] for an '@' name).
            var (_, sl, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;

            *(uint16*)buffer = raw.Family;

            nint pathBytes = (nint)(uint32)sl - 2;

            for (nint i = 0; i < pathBytes; i++) {
                buffer[2 + i] = (byte)raw.Path[i];
            }

            return (sl, default!);
        }
        case ж<SockaddrLinklayer> box: {
            var (_, _, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrLinklayer* native = (NativeSockaddrLinklayer*)buffer;

            native->Family = raw.Family;
            native->Protocol = raw.Protocol;
            native->Ifindex = raw.Ifindex;
            native->Hatype = raw.Hatype;
            native->Pkttype = raw.Pkttype;
            native->Halen = raw.Halen;

            for (nint i = 0; i < 8; i++) {
                native->Addr[i] = raw.Addr[i];
            }

            return (SizeofSockaddrLinklayer, default!);
        }
        case ж<SockaddrNetlink> box: {
            var (_, _, err) = box.sockaddr();

            if (err != default!) {
                return (0, err);
            }

            ref var raw = ref box.Value.raw;
            NativeSockaddrNetlink* native = (NativeSockaddrNetlink*)buffer;

            native->Family = raw.Family;
            native->Pad = raw.Pad;
            native->Pid = raw.Pid;
            native->Groups = raw.Groups;

            return (SizeofSockaddrNetlink, default!);
        }
        default:
            return (0, EAFNOSUPPORT);
        }
    }

    // Decodes the native sockaddr the kernel just wrote into the Sockaddr the Go caller expects --
    // Go's anyToSockaddr, arm for arm, over a native image instead of a reinterpreted managed
    // struct. The one definition of that decode: Getsockname, Getpeername, Accept4 and
    // anyToSockaddr all land here.
    // `capacity` is how many bytes of `buffer` the CALLER guarantees are readable and zero-
    // initialised past what the kernel wrote. It is NOT addrlen, and the difference is the
    // whole AF_UNIX arm below: Go reads sun_path as a FIXED 108-byte array and ignores the
    // reported length entirely, so a bound taken from addrlen silently changes the answer for
    // an abstract socket, whose addrlen is 2.
    private static unsafe (Sockaddr, error) readNativeSockaddr(byte* buffer, _Socklen len, nint capacity) {
        uint16 family = *(uint16*)buffer;

        if (family == AF_NETLINK) {
            NativeSockaddrNetlink* native = (NativeSockaddrNetlink*)buffer;
            var sa = @new<SockaddrNetlink>();

            sa.Value.Family = native->Family;
            sa.Value.Pad = native->Pad;
            sa.Value.Pid = native->Pid;
            sa.Value.Groups = native->Groups;

            return (new SockaddrNetlinkжSockaddr(sa), default!);
        }

        if (family == AF_PACKET) {
            NativeSockaddrLinklayer* native = (NativeSockaddrLinklayer*)buffer;
            var sa = @new<SockaddrLinklayer>();

            sa.Value.Protocol = native->Protocol;
            sa.Value.Ifindex = (nint)native->Ifindex;
            sa.Value.Hatype = native->Hatype;
            sa.Value.Pkttype = native->Pkttype;
            sa.Value.Halen = native->Halen;

            var addr = new array<byte>(8);

            for (nint i = 0; i < 8; i++) {
                addr[i] = native->Addr[i];
            }

            sa.Value.Addr = addr;

            return (new SockaddrLinklayerжSockaddr(sa), default!);
        }

        if (family == AF_UNIX) {
            var sa = @new<SockaddrUnix>();

            // Go rewrites a leading NUL as '@' for textual display of an abstract socket and then
            // reads the name up to the first NUL, bounded by len(Path) = 108; the bound here is the
            // same, clipped to the length the kernel reported.
            // MEASURED against `go run` on Linux (autobound abstract socket): Go answers "@"
            // -- name length 1, first byte 64 -- where bounding on the reported length answers
            // the empty string. The kernel reports addrlen == 2 for such a socket, so a bound
            // of len-2 is ZERO: the '@' rewrite is skipped and the scan stops before it starts.
            // Go's own arm never consults addrlen here -- it rewrites Path[0] and scans to the
            // first NUL over the fixed 108-byte array -- so the bound has to come from the
            // BUFFER. unixsock_test.go's TestUnix{,gram}ConnLocalAndRemoteNames are the rows.
            nint pathMax = capacity - 2;

            if (pathMax > 108) {
                pathMax = 108;
            }

            if (pathMax > 0 && buffer[2] == 0) {
                buffer[2] = (byte)'@';
            }

            nint n = 0;

            while (n < pathMax && buffer[2 + n] != 0) {
                n++;
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
    // an address -- so the errno handling and the call shape stay exactly where the converter put
    // them.
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

    // Getsockname/Getpeername go through the RawSyscall trampoline directly rather than their
    // generated wrappers, because those take a typed `ж<RawSockaddrAny>` -- the very managed struct
    // that cannot cross the boundary -- rather than an address. The error handling below mirrors
    // the generated wrappers exactly (errnoErr of the trap's errno); the kernel writes the address
    // into the stack buffer and its length into `addrlen`.
    public static unsafe (Sockaddr sa, error err) Getsockname(nint fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = nativeSockaddrLen;

        var (_, _, e1) = RawSyscall(SYS_GETSOCKNAME, (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));

        if (e1 != 0) {
            return (default!, errnoErr(e1));
        }

        return readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);
    }

    public static unsafe (Sockaddr sa, error err) Getpeername(nint fd) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = nativeSockaddrLen;

        var (_, _, e1) = RawSyscall(SYS_GETPEERNAME, (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));

        if (e1 != 0) {
            return (default!, errnoErr(e1));
        }

        return readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);
    }

    // Recvfrom is the FIFTH confirmed instance of the kernel-writes-over-a-managed-array class
    // (Timezoneinformation, win32finddata1, ProcessEntry32, SiginfoChild, and this), and the first
    // one measured as a process KILL rather than a wrong value:
    //
    //   System.AccessViolationException: Attempted to read or write protected memory...
    //     at go.array<SByte>.get_Item(IntPtr)
    //     at syscall.anyToSockaddr(ж<RawSockaddrAny>)
    //     at syscall.Recvfrom  ->  syscall.NetlinkRIB  ->  net.interfaceTable  ->  net.Interfaces()
    //
    // MECHANISM, and the tell that distinguishes it from an ordinary empty-array bug. The generated
    // body did `ref var rsa = ref heap(new RawSockaddrAny(), out var Ꮡrsa)` and passed `Ꮡrsa` to
    // recvfrom(2) -- so the kernel wrote a native sockaddr over MANAGED memory whose `array<int8>`
    // fields are eight-byte OBJECT REFERENCES where the OS expects inline storage. The reference
    // itself becomes raw sockaddr bytes, and the next index follows it into unmapped memory. golib's
    // array<T> indexer is bounds-CHECKED and would panic cleanly on a merely-empty array, so an
    // AccessViolation specifically means corrupted state -- that is the diagnostic tell, and it is
    // why this presents as a crash rather than as garbage.
    //
    // net.Interfaces() reaches it through NetlinkRIB on every call, so the public API killed the
    // process on Linux at master until this body replaced it.
    //
    // THE REMEDY IS THE MIRROR'S, for the fourth time: a native image this file owns, plus a typed
    // decode. Go's own shape is reproduced exactly (syscall_unix.go): recvfrom, then decode ONLY when
    // the kernel actually wrote an address family -- `if rsa.Addr.Family != AF_UNSPEC`. The buffer's
    // family word is cleared first so "the kernel wrote nothing" reads as AF_UNSPEC rather than as
    // whatever the stack happened to hold, which is the same question Go's zero-valued struct answers.
    // Syscall6, not RawSyscall, because Go's generated recvfrom uses the bracketed form: this call
    // can block.
    //
    // SCOPE, per the commissioning ruling: Recvfrom ALONE closes the AV. Recvmsg/Sendmsg are NOT the
    // same lines -- they need a native msghdr plus an iovec array and two-way control-message
    // handling -- so they stay behind DESIGN-linux-udp.md's S2 evidence gate rather than riding along.
    public static unsafe (nint n, Sockaddr from, error err) Recvfrom(nint fd, slice<byte> p, nint flags) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = nativeSockaddrLen;
        byte zero = 0;

        // "No address written" must be distinguishable from a stale stack value; Go gets that from a
        // zero-valued RawSockaddrAny, this gets it by clearing the family word the check reads.
        *(uint16*)buffer = AF_UNSPEC;

        // The payload travels by pinned slice-element address, exactly as every generated zsyscall
        // wrapper does. An empty slice has no element to take, so it passes a valid address of a
        // zero-length region. The pin is NOT unconditional -- this comment used to say the address
        // was "the one managed storage the runtime can be asked to hold still", which reads as a
        // permanent guarantee and is wrong: see the box-lifetime note below.
        // The BOX, not merely the address. `(uintptr)` pins through a GCHandle stored on the box,
        // so the pin lasts the BOX's reachability -- hold it across the call or the pin ends and the
        // address may move (measured on an unheld box: the old address reads 0x00000000). This is
        // the converter's own ᴋN + GC.KeepAlive shape, hand-written because the ternary keeps Go's
        // zero-length path.
        ж<byte> ᴋp = default!;
        uintptr payload;

        if (len(p) > 0) {
            ᴋp = Ꮡ(p, 0);
            payload = (uintptr)ᴋp;
        } else {
            payload = (uintptr)(void*)(&zero);
        }

        var (r1, _, e1) = Syscall6(SYS_RECVFROM, (uintptr)fd, payload, (uintptr)len(p),
                                   (uintptr)flags, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen));
        System.GC.KeepAlive(ᴋp);

        if (e1 != 0) {
            return ((nint)r1, default!, errnoErr(e1));
        }

        if (*(uint16*)buffer == AF_UNSPEC) {
            return ((nint)r1, default!, default!);
        }

        var (sa, err) = readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);
        return ((nint)r1, sa, err);
    }

    // Sendto is Recvfrom's direction reversed and takes Bind/Connect's shape, not Recvfrom's: the
    // kernel READS the address here, so there is no stack buffer to decode afterwards and no
    // AF_UNSPEC sentinel to distinguish "nothing written" -- just the native image built once and
    // handed to the package's own generated `sendto`.
    //
    // What was wrong with the generated body: Go's Sendto calls `to.sockaddr()` and passes the
    // `unsafe.Pointer` it returns. That pointer addresses a MANAGED raw struct -- SockaddrInet4's
    // `raw`, whose `Addr` and `Zero` are `array<byte>` OBJECT REFERENCES rather than inline bytes --
    // so the kernel reads two references where it expects four address bytes and eight zero bytes,
    // and the datagram goes to whatever those references decode to. It cannot corrupt the managed
    // heap the way Recvfrom's write did (the kernel only reads), which is why this is the LAYOUT
    // half of the wall rather than the write-over-managed half, and why it was safe to leave until
    // a caller reached it.
    //
    // A nil `to` is NOT an error and must not go through writeNativeSockaddr: Go leaves `ptr` nil
    // and `salen` zero and calls sendto anyway, which is how a datagram goes out on a CONNECTED
    // socket. `@unsafe.Pointer`'s uintptr bridge answers 0 for both nil representations, so the
    // default value reaches the kernel as the null address Go sends.
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

    // ---- the ANCILLARY seam: recvmsgRaw / sendmsgN ------------------------------------------------
    //
    // ONE body each covers SIX entry points. recvmsgRaw is called by Recvmsg, recvmsgInet4 and
    // recvmsgInet6, so ONE body covers all three. The send side takes the public SendmsgN instead,
    // for the reason stated on it: the raw helper's pointer parameter is already managed, and only
    // the public function still holds the typed Sockaddr that can be re-encoded.
    //
    // recvmsgRaw is the CORRUPTING half of this class and sendmsgN the misdirecting half: the
    // kernel WRITES through msg.Name and msg.Control, and reads through them on the send side.
    // Both hand the kernel a managed Msghdr today, whose Name/Iov/Control are object references.
    //
    // recvmsgRaw keeps its `ж<RawSockaddrAny>` OUT-parameter and fills it, rather than decoding to
    // a Sockaddr here: its three callers each read `rsa.Addr.Family` and hand `&rsa` to
    // anyToSockaddr, so a faithful drop-in has to leave that contract alone. The transcription
    // below is the exact inverse of anyToSockaddr's flatten (Family at 0, Data at 2..15, Pad at
    // 16..111), which is why the two stay in step.
    internal static unsafe (nint n, nint oobn, nint recvflags, error err) recvmsgRaw(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<RawSockaddrAny> Ꮡrsa) {
        byte* nameBuf = stackalloc byte[nativeSockaddrLen];

        uint32 nameLen = (uint32)SizeofSockaddrAny;

        var (n, oobn, recvflags, err) = GoRecvmsgNative(fd, p, oob, flags, nameBuf, ref nameLen);

        if (err != default!) {
            return (0, 0, 0, err);
        }

        // The kernel wrote the sender's address into the stack buffer; transcribe it back into the
        // caller's managed RawSockaddrAny, which is what its three callers read. This is the exact
        // INVERSE of anyToSockaddr's flatten (Family at 0, Data at 2..15, Pad at 16..111), which is
        // what keeps the two in step.
        ref var rsa = ref Ꮡrsa.Value;
        rsa.Addr.Family = *(uint16*)nameBuf;

        for (nint i = 0; i < 14; i++) {
            rsa.Addr.Data[i] = (int8)nameBuf[2 + i];
        }

        for (nint i = 0; i < 96; i++) {
            rsa.Pad[i] = (int8)nameBuf[16 + i];
        }

        return (n, oobn, recvflags, default!);
    }


    // SendmsgN is recvmsgRaw's direction reversed, and it is the PUBLIC function rather than the
    // raw helper for a reason the receive side does not have: sendmsgN's `ptr` parameter is
    // already the address of a MANAGED raw sockaddr (whatever `to.sockaddr()` returned), so there
    // is nothing there to transcribe faithfully -- the typed Sockaddr has to be re-encoded, and
    // only SendmsgN still holds it. Bind/Connect take the same shape for the same reason.
    //
    // Go passes NULL for a connected socket, and a connected socket REJECTS a sendmsg carrying an
    // address with EISCONN. The generated body turns that NULL into an object -- `(ж<byte>)(uintptr)0`
    // is `new NativeBox<byte>(0)` -- so the kernel reads a non-NULL msg_name and answers EISCONN.
    // That is the failure ScmRightsSeam measured before this body existed; here a nil `to` simply
    // leaves msg.Name null.
    //
    // sendmsgN itself, and sendmsgNInet4/sendmsgNInet6, stay AUTO: with this body in place their
    // only remaining callers are each other, and internal/poll reaches the //go:linkname copies in
    // internal/syscall/unix/linux/net_linux_impl.cs instead -- the same reason sendtoInet4/6 are
    // left alone (see the file header).
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

        // A nil `to` leaves nameBuf NULL, which is the whole point: the generated body turned Go's
        // NULL into `new NativeBox<byte>(0)` -- an object -- and a connected socket answered EISCONN.
        return GoSendmsgNative(fd, p, oob, nameBuf, nameLen, flags);
    }


    // ---- the CROSS-ASSEMBLY seam for the msghdr family ------------------------------------------
    //
    // internal/syscall/unix's RecvmsgInet4/6 and SendmsgNInet4/6 are the //go:linkname copies
    // internal/poll reaches, and they live in a DIFFERENT ASSEMBLY. S1 already settled how this
    // file exports across that boundary and these two follow it rather than inventing a second
    // convention: a `Go`-prefixed PUBLIC helper, with the native mirrors staying PRIVATE. The
    // existing members of that set are GoNativeSockaddrLen, GoWriteNativeSockaddrInet4/6 and
    // GoReadNativeSockaddrInet4/6; these are the msghdr family's, so NativeMsghdr and NativeIovec
    // never leave this file and no native type crosses the assembly line.
    //
    // They are recvmsgRaw's and SendmsgN's bodies with the ADDRESS handling lifted out -- the
    // caller supplies the native sockaddr image (send) or the buffer to receive one into
    // (receive). That is the shape BOTH sides of the boundary already hold: syscall's own callers
    // through writeNativeSockaddr and the RawSockaddrAny transcription, and
    // internal/syscall/unix's through GoWriteNativeSockaddrInet4 / GoReadNativeSockaddrInet4.
    //
    // Factored rather than duplicated deliberately: ScmRightsSeam already proves these exact
    // lines through recvmsgRaw and SendmsgN, and a second copy would be a second thing to keep
    // right. The two callers below are re-proven by that guard after the factoring.

    // writev's iovec ARRAY -- the SAME managed-reference defect as Msghdr.Iov, one syscall over, and
    // the first instance measured on the plain (non-msghdr) path. The converted `Iovec` is
    //
    //     [GoType] partial struct Iovec { public ж<byte> Base; public uint64 Len; }
    //
    // so `Base` is an OBJECT REFERENCE. A struct holding a reference gets AUTO layout from the CLR
    // and is non-blittable, so handing the kernel `&iovecs[0]` makes it read 16 bytes per element
    // that are neither `{void*; size_t}` nor even in that field order. The symptom is diagnostic
    // and is what identified this: writev with ten one-byte iovecs carrying 0x00..0x09 delivered
    // ten 0x38s -- the RIGHT COUNT of iovecs with GARBAGE contents, which is what an auto-laid-out
    // mirror produces and what a short write never does. Measured by G on the net row
    // (TestBuffers_WriteTo x9); rooted here 2026-09-02.
    //
    // The mirror is NativeIovec, already declared in this file for the msghdr family, and the
    // conversion PINS rather than marshals: `(uintptr)` on a p-box is not a bare address read --
    // its managed arm calls EnsureStableAddress() (a pin that outlives the statement, deliberately
    // not `fixed`, whose own comment reads "fixed pins only for its own statement, and the address
    // outlives that statement by definition") and then registers the pin with ManagedPointerTokens.
    // So the bytes the kernel writes are the CALLER'S bytes at a stable address for the call -- the
    // audit rule this class is held to: no copy in, no copy out, nothing to miss on the way back.
    //
    // WITH THE CLAUSE THAT WAS MISSING, and it is load-bearing: the GCHandle lives ON THE BOX, so
    // the pin lasts the BOX's reachability and not one instant longer. "DURABLE" was read here as
    // unconditional; it is not. A measured probe with only the uintptr retained saw the address
    // MOVE and the old one read 0x00000000. Every call below therefore holds its box across the
    // syscall -- that is what the GC.KeepAlive lines are, and without them this paragraph would be
    // describing a guarantee the code does not have.
    //
    // ONE syscall per call, with the EINTR retry left where Go has it (internal/poll's writev): the
    // caller owns the loop, this owns the layout. Go caps the vector at IOV_MAX/1024, so the native
    // image is a stack buffer for the ordinary handful and a heap one only past the threshold.
    public static unsafe (uintptr wrote, Errno errno) GoWritevNative(nint fd, slice<Iovec> iovecs) {
        nint count = len(iovecs);

        if (count == 0) {
            return (0, 0);
        }

        NativeIovec* native;
        NativeIovec* heap = null;
        NativeIovec* stack = stackalloc NativeIovec[(int)(count <= 32 ? count : 0)];

        if (count <= 32) {
            native = stack;
        } else {
            heap = (NativeIovec*)System.Runtime.InteropServices.NativeMemory.Alloc(
                (nuint)count, (nuint)sizeof(NativeIovec));
            native = heap;
        }

        try {
            for (nint i = 0; i < count; i++) {
                // (uintptr) on the box is the pin AND the address -- see above -- for as long as the
                // BOX is reachable, which is why iovecs is kept alive across the call below.
                native[i].Base = (byte*)(nint)(uintptr)iovecs[i].Base;
                native[i].Len = (nuint)iovecs[i].Len;
            }

            var (r, _, e) = Syscall(SYS_WRITEV, (uintptr)fd, (uintptr)(nint)native, (uintptr)count);
            // Every native[i].Base is a pinned address into a managed buffer reached through
            // iovecs[i].Base; keeping the SLICE alive keeps all of those boxes reachable across the
            // call, so this function OWNS its safety rather than borrowing it from internal/poll's
            // fd.iovecs three frames up.
            System.GC.KeepAlive(iovecs);
            return (r, e);
        } finally {
            if (heap != null) {
                System.Runtime.InteropServices.NativeMemory.Free(heap);
            }
        }
    }

    // nameBuf receives the sender's address image; nameLen is the buffer's capacity going in.
    // Returns the kernel's n / control length / flags, exactly as recvmsgRaw reports them.
    public static unsafe (nint n, nint oobn, nint recvflags, error err) GoRecvmsgNative(nint fd, slice<byte> p, slice<byte> oob, nint flags, byte* nameBuf, ref uint32 nameLen) {
        byte zero = 0;

        NativeIovec iov = default;
        NativeMsghdr msg = default;

        // Declared out here rather than inside the blocks below: the pin lasts the BOX's lifetime,
        // and the syscall is past the end of both blocks.
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
            // Go's own rule, kept verbatim: a control-only receive on a stream socket must still
            // offer one normal byte, or the kernel returns nothing at all.
            if (len(p) == 0) {
                var (sockType, sockErr) = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);

                if (sockErr != default!) {
                    return (0, 0, 0, sockErr);
                }

                if (sockType != SOCK_DGRAM) {
                    iov.Base = &zero;
                    iov.Len = 1;
                }
            }

            ᴋoob = Ꮡ(oob, 0);
            msg.Control = (byte*)(nint)(uintptr)ᴋoob;
            msg.Controllen = (nuint)len(oob);
        }

        msg.Iov = &iov;
        msg.Iovlen = 1;

        var (r0, _, e1) = Syscall(SYS_RECVMSG, (uintptr)fd, (uintptr)(void*)(&msg), (uintptr)flags);
        System.GC.KeepAlive(ᴋp);
        System.GC.KeepAlive(ᴋoob);

        if (e1 != 0) {
            return (0, 0, 0, errnoErr(e1));
        }

        // The kernel rewrites msg_namelen with what it actually wrote; hand it back so a caller
        // that DECODES the image (internal/syscall/unix's Inet4/6 helpers) passes the real length
        // to readNativeSockaddr rather than the buffer's capacity -- S1's Recvfrom reads its
        // addrlen back for the same reason.
        nameLen = msg.Namelen;

        return ((nint)r0, (nint)msg.Controllen, (nint)msg.Flags, default!);
    }

    // nameBuf holds an ALREADY-ENCODED native sockaddr image of nameLen bytes, or is null for a
    // connected socket -- Go's nil `to`, and the case whose generated form answered EISCONN.
    public static unsafe (nint n, error err) GoSendmsgNative(nint fd, slice<byte> p, slice<byte> oob, byte* nameBuf, uint32 nameLen, nint flags) {
        byte zero = 0;

        NativeIovec iov = default;
        NativeMsghdr msg = default;

        // Declared out here rather than inside the blocks below: the pin lasts the BOX's lifetime,
        // and the syscall is past the end of both blocks.
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
            // Go's own rule, kept verbatim: a control-only send on a stream socket must still
            // carry one normal byte.
            if (len(p) == 0) {
                var (sockType, sockErr) = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);

                if (sockErr != default!) {
                    return (0, sockErr);
                }

                if (sockType != SOCK_DGRAM) {
                    iov.Base = &zero;
                    iov.Len = 1;
                }
            }

            ᴋoob = Ꮡ(oob, 0);
            msg.Control = (byte*)(nint)(uintptr)ᴋoob;
            msg.Controllen = (nuint)len(oob);
        }

        msg.Iov = &iov;
        msg.Iovlen = 1;

        var (r0, _, e1) = Syscall(SYS_SENDMSG, (uintptr)fd, (uintptr)(void*)(&msg), (uintptr)flags);
        System.GC.KeepAlive(ᴋp);
        System.GC.KeepAlive(ᴋoob);

        if (e1 != 0) {
            return (0, errnoErr(e1));
        }

        nint n = (nint)r0;

        // Go's own tail (syscall_linux.go:830), and it belongs HERE for the reason Go puts it
        // here: sendmsgN is the single funnel for SendmsgN, sendmsgNInet4 and sendmsgNInet6, so
        // one copy covers all three. The byte counted by the kernel on a control-only send is
        // the DUMMY the block above supplies, not the caller's payload -- reporting it would
        // tell the caller a byte of its data went out when it passed none.
        if (len(oob) > 0 && len(p) == 0) {
            n = 0;
        }

        return (n, default!);
    }

    // Accept4 is Go's own body (syscall_linux.go) over the trampoline instead of the typed generated
    // wrapper: the kernel fills the stack buffer, the "RawSockaddrAny too small" panic and the
    // close-on-decode-failure are kept verbatim. Accept (syscall_linux_accept4.go) is pure Go over
    // this and stays auto.
    public static unsafe (nint nfd, Sockaddr sa, error err) Accept4(nint fd, nint flags) {
        byte* buffer = stackalloc byte[nativeSockaddrLen];
        _Socklen addrlen = nativeSockaddrLen;

        var (r0, _, e1) = Syscall6(SYS_ACCEPT4, (uintptr)fd, (uintptr)(void*)buffer, (uintptr)(void*)(&addrlen), (uintptr)flags, 0, 0);
        nint nfd = (nint)r0;

        if (e1 != 0) {
            return (nfd, default!, errnoErr(e1));
        }

        if (addrlen > SizeofSockaddrAny) {
            throw panic("RawSockaddrAny too small");
        }

        var (sa, err) = readNativeSockaddr(buffer, addrlen, nativeSockaddrLen);

        if (err != default!) {
            Close(nfd);
            nfd = 0;
        }

        return (nfd, sa, err);
    }

    // THE DECODE. Go reinterprets the RawSockaddrAny as a RawSockaddrInet4/6/Unix/… and reads the
    // port through the same two-byte alias the encoders write it through, so the auto conversion
    // panics identically; and `Ꮡrsa.Reinterpret<RawSockaddrAny, RawSockaddrInet4>()` asks golib to
    // alias one reference-bearing struct as another, which it correctly refuses. So the decode is
    // written the only way that is true on both sides: FLATTEN the managed struct back to the
    // 112-byte native image its fields are a transcription of -- Family at 0, Addr.Data at 2..15,
    // Pad at 16..111 (unsafe.Sizeof(RawSockaddrAny{}) = SizeofSockaddrAny) -- and hand that to the
    // one definition of the decode. Go's in-place '@' rewrite of an abstract socket's leading NUL
    // is reproduced on the managed struct too, so the observable state after the call is Go's.
    internal static unsafe (Sockaddr, error) anyToSockaddr(ж<RawSockaddrAny> Ꮡrsa) {
        ref var rsa = ref Ꮡrsa.Value;

        if (rsa.Addr.Family == AF_UNIX && rsa.Addr.Data[0] == 0) {
            rsa.Addr.Data[0] = (int8)'@';
        }

        byte* buffer = stackalloc byte[nativeSockaddrLen];

        *(uint16*)buffer = rsa.Addr.Family;

        for (nint i = 0; i < 14; i++) {
            buffer[2 + i] = (byte)rsa.Addr.Data[i];
        }

        for (nint i = 0; i < 96; i++) {
            buffer[16 + i] = (byte)rsa.Pad[i];
        }

        return readNativeSockaddr(buffer, SizeofSockaddrAny, nativeSockaddrLen);
    }

    // ---- the datagram seam: what internal/syscall/unix's hand-own consumes ------------------------
    //
    // DESIGN-linux-udp.md ⟨OQ-2⟩, RULED: the eight //go:linkname datagram helpers
    // (internal/syscall/unix/linux/net_impl.cs) need this file's native encode/decode, and they live
    // in a DIFFERENT assembly. The ruling is to expose them here rather than duplicate the layout
    // there, for the reason the mirror exists at all: there must be ONE definition of what a Go
    // Sockaddr looks like to the kernel. These four wrappers are that seam and nothing else --
    // deliberately spelled `Go…` so they read as go2cs machinery rather than as Go API (Go's syscall
    // package has no such functions), and typed to the two INET families so the caller carries no
    // layout knowledge at all: it hands over a box and a buffer.
    //
    // Keep in step with net_impl.cs; if a family is added there, add its pair here rather than
    // reaching into the private helpers from outside.

    // The size every caller's stack buffer must be: sockaddr_storage, which fits every family below.
    public const int GoNativeSockaddrLen = nativeSockaddrLen;

    // IN direction (sendto/sendmsg): managed box -> native image, returning the addrlen to pass.
    public static unsafe (_Socklen len, error err) GoWriteNativeSockaddrInet4(ж<SockaddrInet4> sa, byte* buffer) =>
        writeNativeSockaddr(new SockaddrInet4жSockaddr(sa), buffer);

    public static unsafe (_Socklen len, error err) GoWriteNativeSockaddrInet6(ж<SockaddrInet6> sa, byte* buffer) =>
        writeNativeSockaddr(new SockaddrInet6жSockaddr(sa), buffer);

    // OUT direction (recvfrom/recvmsg): native image -> the caller's OWN box, filled by assignment.
    // Go's recvfromInet4 assigns Port and Addr into the caller's struct and leaves everything else
    // alone; these do the same, so a caller that reused a box sees exactly Go's fields change. A
    // datagram that decodes to another family is a kernel contract violation on an AF_INET socket,
    // and is reported rather than silently ignored.
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
}
