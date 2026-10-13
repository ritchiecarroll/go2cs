namespace go;

using fmt = fmt_package;
using Δnet = net_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fatalInterfacesˢ = (@string)"FATAL interfaces:"u8;
private static readonly object noMulticastCapableˢ = (@string)"no multicast-capable interface: join NOT exercised on this host"u8;
private static readonly @string udp4ˢ = "udp4"u8;
private static readonly object joinErrIsNilˢ = (@string)"join err is nil  ="u8;
private static readonly object joinErrorˢ = (@string)"  join error      ="u8;
private static readonly object localIpIsUnsetˢ = (@string)"local ip is unset ="u8;
private static readonly object localPortBoundˢ = (@string)"local port bound  ="u8;

internal static void Main() {
    GoFrame ᒐ = default;
    try {
        var (ifis, err) = Δnet.Interfaces();
        if (err != default!) {
            fmt.Println(fatalInterfacesˢ, err);
            return;
        }

        ж<Δnet.Interface> chosen = default!;
        foreach (var (i, _) in ifis) {
            if ((Δnet.Flags)(ifis[i].Flags & Δnet.FlagUp) != 0 && (Δnet.Flags)(ifis[i].Flags & Δnet.FlagMulticast) != 0) {
                chosen = Ꮡ(ifis, i);
                break;
            }
        }
        if (chosen == nil) {
            fmt.Println(noMulticastCapableˢ);
            return;
        }

        var gaddr = Ꮡ(new Δnet.UDPAddr(IP: Δnet.IPv4(224, 0, 0, 254), Port: 0));

        (var c, err) = Δnet.ListenMulticastUDP(udp4ˢ, chosen, gaddr);
        fmt.Println(joinErrIsNilˢ, err == default!);
        if (err != default!) {
            fmt.Println(joinErrorˢ, err);
            return;
        }
        var cʗ1 = c;
        defer(() => cʗ1.Close(), ref ᒐ);

        var local = c.LocalAddr()._<ж<Δnet.UDPAddr>>();
        fmt.Println(localIpIsUnsetˢ, (~local).IP.IsUnspecified());
        fmt.Println(localPortBoundˢ, (~local).Port != 0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
