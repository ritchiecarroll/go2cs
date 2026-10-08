// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Linux socket filter
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

// Deprecated: Use golang.org/x/net/bpf instead.
public static ж<SockFilter> LsfStmt(nint code, nint k) {
    return Ꮡ(new SockFilter(Code: (uint16)code, K: (uint32)k));
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static ж<SockFilter> LsfJump(nint code, nint k, nint jt, nint jf) {
    return Ꮡ(new SockFilter(Code: (uint16)code, Jt: (uint8)jt, Jf: (uint8)jf, K: (uint32)k));
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static (nint, error) LsfSocket(nint ifindex, nint proto) {
    ref var lsall = ref heap(new SockaddrLinklayer(), out var Ꮡlsall);
    // This is missing SOCK_CLOEXEC, but adding the flag
    // could break callers.
    var (s, e) = Socket(AF_PACKET, SOCK_RAW, proto);
    if (e != default!) {
        return (0, e);
    }
    var p = (ж<array<byte>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡlsall.of(SockaddrLinklayer.ᏑProtocol)));
    p.Value[0] = (byte)((proto >> (int)(8)));
    p.Value[1] = (byte)proto;
    lsall.Ifindex = ifindex;
    e = Bind(s, new SockaddrLinklayerжSockaddr(Ꮡlsall));
    if (e != default!) {
        Close(s);
        return (0, e);
    }
    return (s, default!);
}

partial struct iflags {
    internal array<byte> name = new(IFNAMSIZ);
    internal uint16 flags;
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error SetLsfPromisc(@string name, bool m) {
    GoFrame ᒐ = default;
    try {
        var (s, e) = Socket(AF_INET, (nint)((nint)SOCK_DGRAM | (nint)SOCK_CLOEXEC), 0);
        if (e != default!) {
            return e;
        }
        defer(Close, s, ref ᒐ);
        ref var ifl = ref heap(new iflags(), out var Ꮡifl);
        copy(ifl.name[..], slice<byte>(name));
        var ᴋ0 = Ꮡifl;
                var (_, _, ep) = Syscall(SYS_IOCTL, (uintptr)s, SIOCGIFFLAGS, (uintptr)ᴋ0);
        System.GC.KeepAlive(ᴋ0);
        if (ep != 0) {
            return ep;
        }
        if (m){
            ifl.flags |= (uint16)((uint16)IFF_PROMISC);
        } else {
            ifl.flags &= unchecked((uint16)~(uint16)((uint16)IFF_PROMISC));
        }
        var ᴋ1 = Ꮡifl;
                (_, _, ep) = Syscall(SYS_IOCTL, (uintptr)s, SIOCSIFFLAGS, (uintptr)ᴋ1);
        System.GC.KeepAlive(ᴋ1);
        if (ep != 0) {
            return ep;
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error AttachLsf(nint fd, slice<SockFilter> i) {
    ref var p = ref heap(new SockFprog(), out var Ꮡp);
    p.Len = (uint16)len(i);
    p.Filter = Ꮡ(i, 0);
    return setsockopt(fd, SOL_SOCKET, SO_ATTACH_FILTER, @unsafe.Pointer.FromPinnedBox(Ꮡp), /* unsafe.Sizeof(p) */ (uintptr)16);
}

// Deprecated: Use golang.org/x/net/bpf instead.
public static error DetachLsf(nint fd) {
    ref var dummy = ref heap(new nint(), out var Ꮡdummy);
    return setsockopt(fd, SOL_SOCKET, SO_DETACH_FILTER, @unsafe.Pointer.FromPinnedBox(Ꮡdummy), /* unsafe.Sizeof(dummy) */ (uintptr)8);
}

} // end syscall_package
