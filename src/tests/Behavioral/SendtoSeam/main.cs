namespace go;

using fmt = fmt_package;
using syscall = syscall_package;

partial class main_package {

internal static void fatal(@string what, error err) {
    if (err != default!) {
        fmt.Println(what, "failed:", err);
        throw panic(what);
    }
}

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        ref var receiverAddr = ref heap<array<byte>>(out var ᏑreceiverAddr);
        receiverAddr = new byte[]{127, 0, 0, 2}.array();

        var (receiver, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
        fatal("socket(receiver)", err);
        defer(syscall.Close, receiver, ref ᒐ);

        fatal("bind", syscall.Bind(receiver, new syscall.SockaddrInet4жSockaddr(Ꮡ(new syscall.SockaddrInet4(Addr: receiverAddr.Clone(), Port: 0)))));

        (var bound, err) = syscall.Getsockname(receiver);
        fatal("getsockname", err);
        var dst = bound._<ж<syscall.SockaddrInet4>>();
        fmt.Println("bound to the discriminating address:", (~dst).Addr == receiverAddr);
        fmt.Println("a port was assigned:", (~dst).Port != 0);

        (var sender, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
        fatal("socket(sender)", err);
        defer(syscall.Close, sender, ref ᒐ);

        var to = Ꮡ(new syscall.SockaddrInet4(Addr: (~dst).Addr.Clone(), Port: (~dst).Port));
        fatal("sendto(addressed)", syscall.Sendto(sender, slice<byte>("addressed"u8), 0, new syscall.SockaddrInet4жSockaddr(to)));

        var buf = new slice<byte>(64);
        (var n, var from, err) = syscall.Recvfrom(receiver, buf, 0);
        fatal("recvfrom(addressed)", err);
        fmt.Println("addressed payload:", (@string)(buf.slice(0, n)));

        var first = from._<ж<syscall.SockaddrInet4>>();
        fmt.Println("addressed sender is in 127/8:", (~first).Addr[0] == 127);
        fmt.Println("addressed sender has a port:", (~first).Port != 0);

        fatal("connect", syscall.Connect(sender, new syscall.SockaddrInet4жSockaddr(to)));
        fatal("sendto(nil-to)", syscall.Sendto(sender, slice<byte>("connected"u8), 0, default!));

        (n, from, err) = syscall.Recvfrom(receiver, buf, 0);
        fatal("recvfrom(nil-to)", err);
        fmt.Println("nil-to payload:", (@string)(buf.slice(0, n)));

        var second = from._<ж<syscall.SockaddrInet4>>();
        fmt.Println("nil-to sender equals addressed sender:",
            (~second).Addr == (~first).Addr && (~second).Port == (~first).Port);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
