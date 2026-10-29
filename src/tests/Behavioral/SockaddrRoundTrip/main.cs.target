namespace go;

using fmt = fmt_package;
using syscall = syscall_package;

partial class main_package {

internal static (@string addr, nint port, bool ok) describe(syscallꓸSockaddr sa) {
    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> s: {
        return (fmt.Sprintf("%d.%d.%d.%d"u8, s.Addr[0], s.Addr[1], s.Addr[2], s.Addr[3]), s.Port, true);
    }
    case ж<syscall.SockaddrInet6> s: {
        return (fmt.Sprintf("%x"u8, s.Addr), s.Port, true);
    }}
    return ("", 0, false);
}

internal static bool sameAddr(syscallꓸSockaddr a, syscallꓸSockaddr b) {
    switch (a.type()) {
    case ж<syscall.SockaddrInet4> x: {
        var (y, ok) = b._<ж<syscall.SockaddrInet4>>(ᐧ);
        return ok && x.Addr == y.Addr;
    }
    case ж<syscall.SockaddrInet6> x: {
        var (y, ok) = b._<ж<syscall.SockaddrInet6>>(ᐧ);
        return ok && x.Addr == y.Addr;
    }}
    return false;
}

internal static void roundTrip(@string label, nint family, syscallꓸSockaddr bindTo) {
    GoFrame ᒐ = default;
    try {
        var (server, err) = syscall.Socket(family, syscall.SOCK_STREAM, syscall.IPPROTO_TCP);
        if (err != nil) {
            fmt.Printf("%s: socket failed\n"u8, label);
            return;
        }
        defer(syscall.Closesocket, server, ref ᒐ);

        {
            var errΔ1 = syscall.Bind(server, bindTo); if (errΔ1 != nil) {
                fmt.Printf("%s: bind failed\n"u8, label);
                return;
            }
        }

        (var bound, err) = syscall.Getsockname(server);
        if (err != nil) {
            fmt.Printf("%s: getsockname failed\n"u8, label);
            return;
        }

        var (boundAddr, boundPort, ok) = describe(bound);
        if (!ok) {
            fmt.Printf("%s: getsockname returned an unexpected Sockaddr type\n"u8, label);
            return;
        }
        fmt.Printf("%s bound: addr=%s portAssigned=%v\n"u8, label, boundAddr, boundPort > 0);

        {
            var errΔ2 = syscall.Listen(server, 1); if (errΔ2 != nil) {
                fmt.Printf("%s: listen failed\n"u8, label);
                return;
            }
        }

        (var client, err) = syscall.Socket(family, syscall.SOCK_STREAM, syscall.IPPROTO_TCP);
        if (err != nil) {
            fmt.Printf("%s: client socket failed\n"u8, label);
            return;
        }
        defer(syscall.Closesocket, client, ref ᒐ);

        {
            var errΔ3 = syscall.Connect(client, bound); if (errΔ3 != nil) {
                fmt.Printf("%s: connect failed\n"u8, label);
                return;
            }
        }

        (var peer, err) = syscall.Getpeername(client);
        if (err != nil) {
            fmt.Printf("%s: getpeername failed\n"u8, label);
            return;
        }

        (var peerAddr, var peerPort, ok) = describe(peer);
        if (!ok) {
            fmt.Printf("%s: getpeername returned an unexpected Sockaddr type\n"u8, label);
            return;
        }

        fmt.Printf("%s peer matches bound: addr=%v port=%v\n"u8,
            label, sameAddr(peer, bound) && peerAddr == boundAddr, peerPort == boundPort);

        (var local, err) = syscall.Getsockname(client);
        if (err != nil) {
            fmt.Printf("%s: client getsockname failed\n"u8, label);
            return;
        }

        (var localAddr, var localPort, ok) = describe(local);
        if (!ok) {
            fmt.Printf("%s: client getsockname returned an unexpected Sockaddr type\n"u8, label);
            return;
        }

        fmt.Printf("%s client local: addr=%s portAssigned=%v distinctFromListener=%v\n"u8,
            label, localAddr, localPort > 0, localPort != boundPort);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void putRaw(ref syscall.RawSockaddrAny rsa, nint off, byte v) {
    if (off < 16) {
        rsa.Addr.Data[off - 2] = (int8)v;
        return;
    }
    rsa.Pad[off - 16] = (int8)v;
}

internal static void decodeRawAny(@string label, uint16 family, Action<Action<nint, byte>> fill) {
    ref var rsa = ref heap(new syscall.RawSockaddrAny(), out var Ꮡrsa);
    rsa.Addr.Family = family;
    fill((nint off, byte v) => {
        putRaw(ref (Ꮡrsa).DerefOrNull(), off, v);
    });

    var (sa, err) = Ꮡrsa.Sockaddr();
    if (err != nil) {
        fmt.Printf("%s decode: err=%v\n"u8, label, err);
        return;
    }

    switch (sa.type()) {
    case ж<syscall.SockaddrInet4> s: {
        fmt.Printf("%s decode: inet4 addr=%d.%d.%d.%d port=%d\n"u8,
            label, s.Addr[0], s.Addr[1], s.Addr[2], s.Addr[3], s.Port);
        break;
    }
    case ж<syscall.SockaddrInet6> s: {
        fmt.Printf("%s decode: inet6 addr=%x port=%d zone=%d\n"u8, label, s.Addr, s.Port, s.ZoneId);
        break;
    }
    case ж<syscall.SockaddrUnix> s: {
        fmt.Printf("%s decode: unix name=%q\n"u8, label, s.Name);
        break;
    }
    default: {
        var s = sa;
        fmt.Printf("%s decode: unexpected Sockaddr type\n"u8, label);
        break;
    }}
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        ref var data = ref heap(new syscall.WSAData(), out var Ꮡdata);
        {
            var err = syscall.WSAStartup((uint32)0x202, Ꮡdata); if (err != nil) {
                fmt.Println("WSAStartup failed");
                return;
            }
        }
        defer(() => syscall.WSACleanup(), ref ᒐ);

        roundTrip("tcp4", syscall.AF_INET, new syscall.SockaddrInet4жΔSockaddr(Ꮡ(new syscall.SockaddrInet4(Addr: new byte[]{127, 0, 0, 1}.array()))));
        roundTrip("tcp6", syscall.AF_INET6, new syscall.SockaddrInet6жΔSockaddr(Ꮡ(new syscall.SockaddrInet6(Addr: new array<byte>(16){[15] = 1}))));

        decodeRawAny("raw4", syscall.AF_INET, (Action<nint, byte> put) => {
            put(2, 0x1F);
            put(3, 0x90);
            put(4, 203);
            put(5, 0);
            put(6, 113);
            put(7, 42);
        });

        decodeRawAny("raw6", syscall.AF_INET6, (Action<nint, byte> put) => {
            put(2, 0x01);
            put(3, 0xF4);
            for (nint i = 0; i < 16; i++) {
                put(8 + i, (byte)(0x10 + i));
            }
            put(24, 7);
        });

        decodeRawAny("rawunix", syscall.AF_UNIX, (Action<nint, byte> put) => {
            foreach (var (i, c) in slice<byte>("go2cs.sock"u8)) {
                put(2 + i, c);
            }
        });

        decodeRawAny("rawbad", 0xFEED, (Action<nint, byte> put) => {
        });

        fmt.Println("done");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
