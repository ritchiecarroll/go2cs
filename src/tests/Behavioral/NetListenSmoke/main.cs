namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using strconv = strconv_package;
using time = time_package;

partial class main_package {

internal static (nint, bool) portOf(Δnet.Listener l) {
    var (addr, ok) = l.Addr()._<ж<Δnet.TCPAddr>>(ᐧ);
    if (!ok) {
        return (0, false);
    }
    return ((~addr).Port, true);
}

internal static void Main() {
    var (first, err) = Δnet.Listen("tcp", "127.0.0.1:0");
    if (err != default!) {
        fmt.Println("listener1: listen failed");
        return;
    }

    var (firstPort, ok1) = portOf(first);
    fmt.Printf("listener1: network=%s portAssigned=%v\n"u8, first.Addr().Network(), ok1 && firstPort > 0);

    (var second, err) = Δnet.Listen("tcp", "127.0.0.1:0");
    if (err != default!) {
        fmt.Println("listener2: listen failed");
        first.Close();
        return;
    }

    var (secondPort, ok2) = portOf(second);
    fmt.Printf("listener2: portAssigned=%v distinctFromFirst=%v\n"u8,
        ok2 && secondPort > 0, ok1 && ok2 && firstPort != secondPort);

    var armErr = first._<ж<Δnet.TCPListener>>().SetDeadline(time.Now().Add(time.ΔHour));
    var clearErr = first._<ж<Δnet.TCPListener>>().SetDeadline(new time.Time(nil));
    fmt.Printf("listener1: deadlineAccepted=%v deadlineClearAccepted=%v\n"u8, armErr == default!, clearErr == default!);

    var closeErr = second.Close();
    fmt.Printf("listener2: closed=%v\n"u8, closeErr == default!);

    (var rebound, err) = Δnet.Listen("tcp", "127.0.0.1:"u8 + strconv.Itoa(secondPort));
    if (err != default!) {
        fmt.Println("rebound: listen failed");
        first.Close();
        return;
    }

    var (reboundPort, ok3) = portOf(rebound);
    fmt.Printf("rebound: reboundSamePort=%v\n"u8, ok3 && reboundPort == secondPort);

    fmt.Printf("listener2: closedIsSticky=%v\n"u8, second.Close() != default!);

    fmt.Printf("cleanup: %v %v\n"u8, rebound.Close() == default!, first.Close() == default!);
    fmt.Println("done");
}

} // end main_package
