namespace go;

using fmt = fmt_package;
using Δnet = net_package;

partial class main_package {

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (ifis, err) = Δnet.Interfaces();
        if (err != nil) {
            fmt.Println("FATAL interfaces:", err);
            return;
        }

        ж<Δnet.Interface> chosen = default;
        foreach (var i in range(ifis)) {
            if ((Δnet.Flags)(ifis[i].Flags & Δnet.FlagUp) != 0 && (Δnet.Flags)(ifis[i].Flags & Δnet.FlagMulticast) != 0) {
                chosen = Ꮡ(ifis, i);
                break;
            }
        }
        if (chosen == nil) {
            fmt.Println("no multicast-capable interface: join NOT exercised on this host");
            return;
        }

        var gaddr = Ꮡ(new Δnet.UDPAddr(IP: Δnet.IPv4(224, 0, 0, 254), Port: 0));

        (var c, err) = Δnet.ListenMulticastUDP("udp4", chosen, gaddr);
        fmt.Println("join err is nil  =", err == nil);
        if (err != nil) {
            fmt.Println("  join error      =", err);
            return;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        var local = c.LocalAddr()._<ж<Δnet.UDPAddr>>();
        fmt.Println("local ip is unset =", (~local).IP.IsUnspecified());
        fmt.Println("local port bound  =", (~local).Port != 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
