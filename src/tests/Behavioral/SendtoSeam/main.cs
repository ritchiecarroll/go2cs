namespace go;

using fmt = fmt_package;
using syscall = syscall_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object failedˢ = (@string)"failed:"u8;

internal static void fatal(@string what, error err) {
    if (err != default!) {
        fmt.Println(what, failedˢ, err);
        throw panic(what);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string socketReceiverˢ = "socket(receiver)"u8;
private static readonly @string bindˢ = "bind"u8;
private static readonly @string getsocknameˢ = "getsockname"u8;
private static readonly object boundToTheDiscriminatingˢ = (@string)"bound to the discriminating address:"u8;
private static readonly object aPortWasAssignedˢ = (@string)"a port was assigned:"u8;
private static readonly @string socketSenderˢ = "socket(sender)"u8;
private static readonly @string sendtoAddressedˢ = "sendto(addressed)"u8;
private static readonly @string recvfromAddressedˢ = "recvfrom(addressed)"u8;
private static readonly object addressedPayloadˢ = (@string)"addressed payload:"u8;
private static readonly object addressedSenderIsIn1278ˢ = (@string)"addressed sender is in 127/8:"u8;
private static readonly object addressedSenderHasAPortˢ = (@string)"addressed sender has a port:"u8;
private static readonly @string connectˢ = "connect"u8;
private static readonly @string sendtoNilToˢ = "sendto(nil-to)"u8;
private static readonly @string recvfromNilToˢ = "recvfrom(nil-to)"u8;
private static readonly object nilToPayloadˢ = (@string)"nil-to payload:"u8;
private static readonly object nilToSenderEqualsˢ = (@string)"nil-to sender equals addressed sender:"u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        ref var receiverAddr = ref heap<array<byte>>(out var ᏑreceiverAddr);
        receiverAddr = new byte[]{127, 0, 0, 2}.array();
        var (receiver, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
        fatal(socketReceiverˢ, err);
        defer(syscall.Close, receiver, ref ᒐ);
        fatal(bindˢ, syscall.Bind(receiver, new syscall.SockaddrInet4жSockaddr(Ꮡ(new syscall.SockaddrInet4(Addr: receiverAddr.Clone(), Port: 0)))));
        (var bound, err) = syscall.Getsockname(receiver);
        fatal(getsocknameˢ, err);
        var dst = bound._<ж<syscall.SockaddrInet4>>();
        fmt.Println(boundToTheDiscriminatingˢ, (~dst).Addr == receiverAddr);
        fmt.Println(aPortWasAssignedˢ, (~dst).Port != 0);
        (var sender, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0);
        fatal(socketSenderˢ, err);
        defer(syscall.Close, sender, ref ᒐ);
        var to = Ꮡ(new syscall.SockaddrInet4(Addr: (~dst).Addr.Clone(), Port: (~dst).Port));
        fatal(sendtoAddressedˢ, syscall.Sendto(sender, slice<byte>("addressed"u8), 0, new syscall.SockaddrInet4жSockaddr(to)));
        var buf = new slice<byte>(64);
        (var n, var from, err) = syscall.Recvfrom(receiver, buf, 0);
        fatal(recvfromAddressedˢ, err);
        fmt.Println(addressedPayloadˢ, ((@string)(buf.slice(0, n))));
        var first = from._<ж<syscall.SockaddrInet4>>();
        fmt.Println(addressedSenderIsIn1278ˢ, (~first).Addr[0] == 127);
        fmt.Println(addressedSenderHasAPortˢ, (~first).Port != 0);
        fatal(connectˢ, syscall.Connect(sender, new syscall.SockaddrInet4жSockaddr(to)));
        fatal(sendtoNilToˢ, syscall.Sendto(sender, slice<byte>("connected"u8), 0, default!));
        (n, from, err) = syscall.Recvfrom(receiver, buf, 0);
        fatal(recvfromNilToˢ, err);
        fmt.Println(nilToPayloadˢ, ((@string)(buf.slice(0, n))));
        var second = from._<ж<syscall.SockaddrInet4>>();
        fmt.Println(nilToSenderEqualsˢ,
            (~second).Addr == (~first).Addr && (~second).Port == (~first).Port);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
