// net_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of net.go's //go:linkname datagram helpers for the DARWIN flavor --
// the darwin companion of internal/syscall/unix/linux/net_linux_impl.cs (whose header carries the
// write-up: why the eight helpers are the ENTIRE datagram surface of the converted corpus, why
// syscall's own copies are dead code that stays auto, and the payload/EAGAIN rules), cut with the
// darwin sockaddr twin (syscall/darwin/sockaddr_darwin_impl.cs, increment 8, 2026-09-05).
//
// WHAT DIFFERS FROM LINUX, and it is the reason this is a separate file: darwin has no syscall
// NUMBERS. The linux companion calls the kernel through syscall.Syscall6(45/44, ...); on darwin every
// call goes through a libc trampoline the syscall package resolves by //go:cgo_import_dynamic, and
// those trampolines are `internal` to that assembly. So this file owns NO call of its own: the four
// it needs are the twin's `Go…Native` seams (GoRecvfromNative / GoSendtoNative / GoRecvmsgNative /
// GoSendmsgNative), beside the sockaddr pairs both companions share (GoWrite/GoReadNativeSockaddr
// Inet4/6). No native type and no trampoline crosses the assembly line, and there stays exactly ONE
// definition of what a Go Sockaddr, a msghdr and an iovec look like to the kernel.
//
// SCOPE. All eight datagram helpers, plus -- since darwin increment 10 (a), 2026-09-05 -- the five
// //go:linkname PULLS net_darwin.go makes of syscall's keystone family (syscall_syscall,
// syscall_syscallPtr, syscall_syscall6, syscall_syscall6X, syscall_syscall9), at the bottom of the
// file. The eight: RecvfromInet4/6, SendtoInet4/6, RecvmsgInet4/6, SendmsgNInet4/6 -- S1 and S2
// together, on the evidence linux already measured for S2 (net's TestUDPIPVersionReadMsg,
// TestUDPConnSpecificMethods and TestAllocs dying on PartialStubGenerator's body) and on the five
// darwin behavioral rows whose paths are known (UdpLoopbackRoundTrip -> ReadFrom/WriteTo ->
// RecvfromInet4/SendtoInet4; UdpWriteMsgAddrPort -> WriteMsg -> SendmsgNInet4). There is no
// registration and no converter change: each helper is a `partial` declaration carrying its
// //go:linkname, and PartialStubGenerator skips any partial whose PartialImplementationPart is
// non-null, so writing a body displaces its stub by construction.
//
// THE FILE NAME IS LOAD-BEARING, the other way round from linux's: `net_darwin.cs` IS an emitted
// darwin-only principal (Go's net_darwin.go), so an `net_darwin_impl.cs` companion is routed by the
// L3 merge to darwin/ alone -- exactly where it belongs -- where the linux file had to stay
// principal-less to avoid being copied here.

using System;

// Hand-owned (no net_darwin_impl.go exists, so a reconvert never regenerates this file); marked per
// the hand-own rules so a -stdlib run cannot emit a Go version over it.
[module: go.GoManualConversion]
// Stack images of native sockaddrs are built here and handed to the twin's seams by address, so the
// package's emitted .csproj must allow unsafe -- the marker is how a hand-own declares that.
[module: go.GoRequiresUnsafe]

namespace go.@internal.syscall;

using abi = go.@internal.abi_package;
using syscall = syscall_package;
using System.Runtime.InteropServices;

partial class unix_package
{
    // ---- RecvfromInet4 / RecvfromInet6 -----------------------------------------------------------

    // Go: recvfrom(fd, p, flags, &rsa, &socklen) then decode rsa into `from` (syscall_unix.go).
    // Here the kernel writes into a stack image through the twin's seam and the twin decodes it;
    // `from` is filled by ASSIGNMENT, exactly as Go's helper does, so no managed address is exposed.
    public static partial (nint, error) RecvfromInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 addrlen = syscall.GoNativeSockaddrLen;

            var (n, err) = syscall.GoRecvfromNative(fd, p, flags, buffer, ref addrlen);

            if (err != default!) {
                return (0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet4(buffer, (syscall._Socklen)addrlen, from);

            if (decodeErr != default!) {
                return (n, decodeErr);
            }

            return (n, default!);
        }
    }

    public static partial (nint n, error err) RecvfromInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 addrlen = syscall.GoNativeSockaddrLen;

            var (n, err) = syscall.GoRecvfromNative(fd, p, flags, buffer, ref addrlen);

            if (err != default!) {
                return (0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet6(buffer, (syscall._Socklen)addrlen, from);

            if (decodeErr != default!) {
                return (n, decodeErr);
            }

            return (n, default!);
        }
    }

    // ---- SendtoInet4 / SendtoInet6 ---------------------------------------------------------------

    // Go: to.sockaddr() then sendto(fd, p, flags, ptr, n). The encode half is the twin's
    // `sockaddr()`; the only thing the converted version gets wrong is the last step -- it hands
    // `sendto` the pointer the encoder returns, which names a MANAGED raw box. Writing the image
    // into a stack buffer and passing THAT address is what the twin's Bind and Connect already do.
    public static partial error /*err*/ SendtoInet4(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet4> to) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (addrlen, err) = syscall.GoWriteNativeSockaddrInet4(to, buffer);

            if (err != default!) {
                return err;
            }

            return syscall.GoSendtoNative(fd, p, flags, buffer, addrlen);
        }
    }

    public static partial error /*err*/ SendtoInet6(nint fd, slice<byte> p, nint flags, ж<syscall.SockaddrInet6> to) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (addrlen, err) = syscall.GoWriteNativeSockaddrInet6(to, buffer);

            if (err != default!) {
                return err;
            }

            return syscall.GoSendtoNative(fd, p, flags, buffer, addrlen);
        }
    }

    // ---- the four msghdr helpers ----------------------------------------------------------------

    // Each body is Go's own (syscall_unix.go recvmsgInet4/sendmsgNInet4), composed from the two
    // seams the twin exports: the sockaddr encode/decode and the msghdr/iovec/control machinery.
    public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet4(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet4> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 nameLen = syscall.GoNativeSockaddrLen;

            var (n, oobn, recvflags, err) = syscall.GoRecvmsgNative(fd, p, oob, flags, buffer, ref nameLen);

            if (err != default!) {
                return (0, 0, 0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet4(buffer, (syscall._Socklen)nameLen, from);

            if (decodeErr != default!) {
                return (n, oobn, recvflags, decodeErr);
            }

            return (n, oobn, recvflags, default!);
        }
    }

    public static partial (nint n, nint oobn, nint recvflags, error err) RecvmsgInet6(nint fd, slice<byte> p, slice<byte> oob, nint flags, ж<syscall.SockaddrInet6> from) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            uint32 nameLen = syscall.GoNativeSockaddrLen;

            var (n, oobn, recvflags, err) = syscall.GoRecvmsgNative(fd, p, oob, flags, buffer, ref nameLen);

            if (err != default!) {
                return (0, 0, 0, err);
            }

            var decodeErr = syscall.GoReadNativeSockaddrInet6(buffer, (syscall._Socklen)nameLen, from);

            if (decodeErr != default!) {
                return (n, oobn, recvflags, decodeErr);
            }

            return (n, oobn, recvflags, default!);
        }
    }

    public static partial (nint n, error err) SendmsgNInet4(nint fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet4> to, nint flags) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (nameLen, err) = syscall.GoWriteNativeSockaddrInet4(to, buffer);

            if (err != default!) {
                return (0, err);
            }

            return syscall.GoSendmsgNative(fd, p, oob, buffer, (uint32)nameLen, flags);
        }
    }

    public static partial (nint n, error err) SendmsgNInet6(nint fd, slice<byte> p, slice<byte> oob, ж<syscall.SockaddrInet6> to, nint flags) {
        unsafe {
            byte* buffer = stackalloc byte[syscall.GoNativeSockaddrLen];
            var (nameLen, err) = syscall.GoWriteNativeSockaddrInet6(to, buffer);

            if (err != default!) {
                return (0, err);
            }

            return syscall.GoSendmsgNative(fd, p, oob, buffer, (uint32)nameLen, flags);
        }
    }

    // ---- syscall's keystone family, pulled by //go:linkname (net_darwin.go) --------------------

    // Go: `//go:linkname syscall_syscall6 syscall.syscall6` and four siblings -- two-argument PULLS
    // of the syscall package's runtime-provided family (syscall/darwin/syscall_darwin_impl.cs, the
    // keystone hand-own). Their targets are unexported in that assembly and carry no one-arg handle,
    // so no forwarding property can reach them across the assembly boundary: the converter emits
    // each as a bodyless partial and PartialStubGenerator's throw filled it -- the wall the
    // SigIgnoreDisposition probe found on both mac legs (2026-09-05): exec.Command by bare name ->
    // os/exec.LookPath -> unix.Eaccess -> faccessat -> syscall_syscall6 -> NotImplementedException,
    // with os/user, the pty family and net's resolver path behind the same five names. Same shape
    // as the eight above: a body here displaces the stub by construction, no registration and no
    // converter change. The bodies stand on the syscall assembly's PUBLIC family -- Syscall,
    // Syscall6 and Syscall9 carry the same Int32MinusOne failure test as their lowercase twins (the
    // keystone hand-own: raw and cooked are the same call there) -- and on the two Go-prefixed doors
    // added beside them for the rules that have no exported twin: syscall6X (all 64 bits == -1) and
    // syscallPtr (NULL is the error). Darwin increment 10 (a).
    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
        return syscall.Syscall(fn, a1, a2, a3);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscallPtr(uintptr fn, uintptr a1, uintptr a2, uintptr a3) {
        return syscall.GoSyscallPtr(fn, a1, a2, a3);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall6(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
        return syscall.Syscall6(fn, a1, a2, a3, a4, a5, a6);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall6X(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
        return syscall.GoSyscall6X(fn, a1, a2, a3, a4, a5, a6);
    }

    internal static partial (uintptr r1, uintptr r2, syscall.Errno err) syscall_syscall9(uintptr fn, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
        return syscall.Syscall9(fn, a1, a2, a3, a4, a5, a6, a7, a8, a9);
    }

    // gostring is the SECOND member of that family and the one nothing had ever CALLED, which is why
    // it outlived increment 10 (a) as a live throw. This package's darwin flavour declares it
    // BODYLESS (net_darwin.cs) as the destination of runtime's
    // `//go:linkname internal_syscall_gostring internal/syscall/unix.gostring`, and that push cannot
    // be emitted: this package already references `runtime` in its own csproj, so the edge the push
    // would add closes a two-project cycle. That is W1's boundary, where the remedy is a body at the
    // DESTINATION rather than an edge the graph cannot carry
    // (docs/phase4/DESIGN-linkname-push-cycles.md); a body displaces the stub by construction, with
    // no registration and no converter change.
    //
    // Nothing reached it until darwin increment 11. `unix.GoString` has exactly two darwin consumers
    // -- GaiStrerror's error path, and os/user's `_C_GoString` -- and the second sat behind the
    // getpwuid_r ERANGE that increment answered, so the first caller ever to arrive was buildUser,
    // one frame past the fix. The acceptance run read it as `C# 2 vs Go 0` with
    // "gostring: external (assembly or cgo) function is not implemented" on stderr.
    //
    // The body is runtime's own two steps (string.cs: findnull, then copy), reached through the
    // pointer's ADDRESS rather than through runtime's helper, which is `internal` and invisible to
    // this assembly. `(uintptr)Ꮡp` is the operator both pointer kinds answer correctly here: `byte`
    // is reference-free, so a managed element box has pinnable storage and is pinned for the box's
    // life, and a native box already carries an address -- there is no token case to consider at
    // this element type. The KeepAlive holds that pin across the scan and the copy, which is the
    // whole window the address is read in.
    internal static partial @string gostring(ж<byte> Ꮡp) {
        if (Ꮡp == nil) {
            return ""u8;
        }

        unsafe {
            byte* q = (byte*)(void*)(uintptr)Ꮡp;

            if (q == null) {
                return ""u8;
            }

            nint n = 0;

            while (q[n] != 0) {
                n++;
            }

            var b = new slice<byte>(n);

            for (nint i = 0; i < n; i++) {
                b[i] = q[i];
            }

            System.GC.KeepAlive(Ꮡp);

            return ((@string)b);
        }
    }

    // ---- Getaddrinfo / Freeaddrinfo --------------------------------------------------------------

    // Darwin increment 12: the FIFTH member of this package's PTROUT class and the one increment 11
    // deferred by name. `Getaddrinfo` carries BOTH halves of the struct-passing seam at once -- the
    // generated wrapper hands libc `(uintptr)Ꮡhints`, the address of a managed `Addrinfo` whose six
    // fields include three managed references and which therefore has CLR AUTO layout, and
    // `(uintptr)Ꮡres`, the `**Addrinfo` out-parameter that value-peeks a heap-boxed null exactly as
    // getpwuid_r's did. Neither is an address libc can use.
    //
    // THE WINDOWS TWIN IS THE PRECEDENT AND IT DOES NOT TRANSFER, which is worth stating because it
    // looks like it should. `syscall/windows/zsyscall_windows_addrinfo_impl.cs` transcribes the whole
    // chain into MANAGED records and frees the native chain eagerly, so `FreeAddrInfoW` is a no-op;
    // its consumer recovers the sockaddr through an `unsafe.Pointer` round trip that a managed box
    // serves. The darwin consumer asks a different question:
    //
    //     var sa = _C_ai_addr(r).ValueSlot.Reinterpret<_C_struct_sockaddr, syscall.RawSockaddrInet6>();
    //
    // and `ж.Reinterpret`'s managed-alias route is gated on `ReinterpretAliasesStorage<T,TDst>`, which
    // needs `SizeOf(TDst) <= SizeOf(T)` AND either both sides reference-free or `LayoutCompatible`.
    // The field counts settle it without running anything: `RawSockaddr` is {Len, Family, Data[14]},
    // THREE fields; `RawSockaddrInet4` {Len, Family, Port, Addr[4], Zero[8]}, FIVE; `RawSockaddrInet6`
    // {Len, Family, Port, Flowinfo, Addr[16], Scope_id}, SIX. `LayoutCompatible` wants the same fields
    // in the same order all the way down and returns false on the first length mismatch, neither type
    // is a single-field wrapper of the other, and all three carry `array<T>` so the reference-free
    // branch is out too. A MANAGED `ж<RawSockaddr>` therefore falls through to the raw-address route
    // with no pin (a reference-bearing pointee has no `PinnableStorage`) and no remembered source
    // (`RememberReinterpretSource` narrows by DESTINATION, and `RawSockaddrInet4` is reference-
    // BEARING). That is a wild address by construction -- the windows header's fabricated-reference
    // paragraph, reached from the other direction.
    //
    // SO `Addr` IS A NATIVE-BACKED BOX, and `Reinterpret` already does the right thing with one: it
    // short-circuits on `box.IsNative` and answers `new NativeBox<TDst>(box.NativeAddress)` -- the same
    // address, aliased at the DESTINATION's size, which is what a 28-byte `sockaddr_in6` read out of a
    // 16-byte `sockaddr` slot needs. `Canonname` is native for the same reason and for consistency:
    // `gostring` above reads it through `(uintptr)Ꮡp`, which a native box answers directly.
    //
    // THAT INVERTS THE LIFETIME HALF. The native chain must OUTLIVE the call, so unlike windows
    // `Freeaddrinfo` frees the REAL chain and this file remembers the native head against the managed
    // one. A caller that drops the head without freeing leaks the native chain -- which is precisely
    // what the same Go program does, and Go's callers use `defer` (net's `_C_freeaddrinfo`).
    //
    // WHAT IS DELIBERATELY NOT COVERED HERE. The consumer's PORT ALIAS -- `net/darwin/cgo_unix.cs`
    // reading `(ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(sa.of(...ᏑPort)))` -- is the
    // shape increment 8's header names as defect (1), in CONVERTED code that no hand-own covers, on a
    // file (`cgo_unix.cs`) that exists on the darwin flavour ONLY, so no banked row on either other
    // platform speaks for it in either direction. It is unreachable until this increment lands and is
    // measured immediately after; if it dies it is its own increment with its own evidence rather than
    // an unmeasured second seam folded into this one's claim.

    // darwin's `struct addrinfo`, 48 bytes: four ints, a socklen_t, and three machine pointers. The
    // one field that differs from ADDRINFOW is `Addrlen` -- `socklen_t` here against windows' `size_t`
    // -- so the four bytes after it are C's padding to the pointer alignment and are declared rather
    // than left implicit. Canonname/Addr/Next land at 24/32/40 on both platforms by coincidence.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NativeAddrinfo
    {
        public int32 Flags;             //  0
        public int32 Family;            //  4
        public int32 Socktype;          //  8
        public int32 Protocol;          // 12
        public uint32 Addrlen;          // 16   socklen_t, NOT windows' size_t
        public uint32 Pad;              // 20   C's padding to the pointer alignment
        public byte* Canonname;         // 24
        public byte* Addr;              // 32
        public NativeAddrinfo* Next;    // 40
    }                                   // 48

    // The native chain each managed head was transcribed from, so Freeaddrinfo can release memory the
    // records still alias. Weak on the head, so a caller that drops it without freeing leaks the
    // native chain exactly as the same Go program would rather than pinning it for the process.
    private sealed class NativeChain
    {
        internal uintptr Head;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, NativeChain> s_nativeChains = new();

    public static unsafe (nint, error) Getaddrinfo(ж<byte> Ꮡhostname, ж<byte> Ꮡservname, ж<Addrinfo> Ꮡhints, ж<ж<Addrinfo>> Ꮡres) {
        // The hints go out through a blittable LOCAL, never the caller's managed box. Only the four
        // scalars are carried, and that is READ OFF GO'S OWN CALLERS rather than assumed: both
        // Getaddrinfo call sites in net/cgo_unix.go declare `var hints _C_struct_addrinfo` zero-valued
        // and set only ai_socktype/ai_protocol (lines 99-107) and ai_flags/ai_socktype/ai_family
        // (lines 165-173). Canonname/Addr/Next are set by no caller, so dropping them is not a
        // narrowing -- and a hints record carrying any of them would be a different call than the
        // one Go makes.
        NativeAddrinfo hints = default;
        NativeAddrinfo* pHints = null;

        if (Ꮡhints != nil) {
            ref var h = ref Ꮡhints.Value;

            hints.Flags = h.Flags;
            hints.Family = h.Family;
            hints.Socktype = h.Socktype;
            hints.Protocol = h.Protocol;
            pHints = &hints;
        }

        // The out-cell is EIGHT BYTES OF NATIVE STORAGE, which is the whole of the ptrout remedy: a
        // `ж<ж<Addrinfo>>` is a managed box whose storage is an object reference, so it has no slot
        // for libc to write an address into.
        NativeAddrinfo* cell = null;

        var (gerrno, _, errno) = syscall_syscall6(abi.FuncPCABI0(libc_getaddrinfo_trampoline),
            (uintptr)Ꮡhostname,
            (uintptr)Ꮡservname,
            (uintptr)(void*)pHints,
            (uintptr)(void*)(&cell),
            0,
            0);

        // Both name buffers are read by libc for the duration of the call and their addresses were
        // taken above; the pin those conversions took is held only while its holder is reachable.
        System.GC.KeepAlive(Ꮡhostname);
        System.GC.KeepAlive(Ꮡservname);

        error err = default!;

        if (errno != 0) {
            err = errno;
        }

        if (cell != null && Ꮡres != nil) {
            ж<Addrinfo> head = transcribeChain(cell);

            if (head != nil) {
                s_nativeChains.Add(head, new NativeChain{ Head = (uintptr)(void*)cell });
            }

            Ꮡres.ValueSlot = head;
        }

        return ((nint)gerrno, err);
    }

    // Transcribes the native chain into managed records, preserving order. Only the SCALARS are
    // copied: Canonname and Addr name the native memory, so the chain the caller walks is managed
    // while the bytes it reads through are libc's -- which is what makes the consumer's reinterpret
    // to a larger sockaddr type sound, and what makes Freeaddrinfo's release load-bearing.
    private static unsafe ж<Addrinfo> transcribeChain(NativeAddrinfo* native) {
        ж<Addrinfo> head = default!;
        ж<Addrinfo> tail = default!;

        for (NativeAddrinfo* cursor = native; cursor != null; cursor = cursor->Next) {
            var box = @new<Addrinfo>();
            ref Addrinfo record = ref box.Value;

            record.Flags = cursor->Flags;
            record.Family = cursor->Family;
            record.Socktype = cursor->Socktype;
            record.Protocol = cursor->Protocol;
            record.Addrlen = cursor->Addrlen;

            // A NULL native pointer must stay Go's nil rather than becoming a native box over address
            // zero: `(ж<T>)(uintptr)0` mints an OBJECT, and `record.Canonname != nil` would then be
            // true for a record libc left empty (the linux sockaddr file's msg_name lesson).
            record.Canonname = cursor->Canonname == null
                ? default!
                : (ж<byte>)(uintptr)(void*)cursor->Canonname;

            record.Addr = cursor->Addr == null
                ? default!
                : (ж<syscall.RawSockaddr>)(uintptr)(void*)cursor->Addr;

            if (head == nil) {
                head = box;
            } else {
                tail.Value.Next = box;
            }

            tail = box;
        }

        return head;
    }

    // Frees the NATIVE chain the managed head was transcribed from -- not a no-op as on windows,
    // because the records' Addr and Canonname alias that memory. A head this file did not build has
    // no entry and is left alone: Getaddrinfo is Go's only producer of an `*Addrinfo` here, so that
    // case is a caller error rather than a chain someone else owns.
    public static unsafe void Freeaddrinfo(ж<Addrinfo> Ꮡai) {
        if (Ꮡai == nil) {
            return;
        }

        if (!s_nativeChains.TryGetValue(Ꮡai, out NativeChain? chain)) {
            return;
        }

        s_nativeChains.Remove(Ꮡai);

        if (chain.Head != 0) {
            syscall_syscall6(abi.FuncPCABI0(libc_freeaddrinfo_trampoline),
                chain.Head,
                0, 0, 0, 0, 0);
        }
    }
}
