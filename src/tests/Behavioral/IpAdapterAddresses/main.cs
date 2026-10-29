namespace go;

using fmt = fmt_package;
using Δnet = net_package;

partial class main_package {

internal static void Main() {
    fmt.Println("-- net.Interfaces --");

    var (ifs, err) = Δnet.Interfaces();
    fmt.Println("error:", err);

    if (err != nil) {
        return;
    }

    fmt.Println("interfaces reported:", len(ifs) > 0);

    var indexesPositive = true;
    var indexesUnique = true;
    var namesNonEmpty = true;
    var namesPrintable = true;
    var namesUnique = true;
    var mtusPlausible = true;
    var hardwareAddrsWithinMax = true;
    var sixByteHardwareAddr = false;
    nint loopbacks = 0;

    var seenIndex = new map<nint, bool>{};
    var seenName = new map<@string, bool>{};

    Δnet.Interface loopback = default;

    foreach (var (_, ifi) in ifs) {
        if (ifi.Index <= 0) {
            indexesPositive = false;
        }
        if (seenIndex[ifi.Index]) {
            indexesUnique = false;
        }
        seenIndex[ifi.Index] = true;

        if (ifi.Name == ""u8) {
            namesNonEmpty = false;
        }
        if (!printable(ifi.Name)) {
            namesPrintable = false;
        }
        if (seenName[ifi.Name]) {
            namesUnique = false;
        }
        seenName[ifi.Name] = true;

        if (ifi.MTU != -1 && (ifi.MTU <= 0 || ifi.MTU > 65536)) {
            mtusPlausible = false;
        }

        if (len(ifi.HardwareAddr) > 8) {
            hardwareAddrsWithinMax = false;
        }
        if (len(ifi.HardwareAddr) == 6) {
            sixByteHardwareAddr = true;
        }

        if ((Δnet.Flags)(ifi.Flags & Δnet.FlagLoopback) != 0) {
            if (loopbacks == 0) {
                loopback = ifi;
            }
            loopbacks++;
        }
    }

    fmt.Println("indexes positive:", indexesPositive);
    fmt.Println("indexes unique:", indexesUnique);
    fmt.Println("names non-empty:", namesNonEmpty);
    fmt.Println("names printable:", namesPrintable);
    fmt.Println("names unique:", namesUnique);
    fmt.Println("MTUs plausible:", mtusPlausible);
    fmt.Println("hardware addresses within MAX_ADAPTER_ADDRESS_LENGTH:", hardwareAddrsWithinMax);
    fmt.Println("some adapter reports a six-byte hardware address:", sixByteHardwareAddr);

    fmt.Println("-- the loopback pseudo-interface --");
    fmt.Println("present:", loopbacks > 0);
    fmt.Println("index positive:", loopback.Index > 0);
    fmt.Println("name non-empty:", loopback.Name != ""u8);
    fmt.Println("MTU is -1:", loopback.MTU == -1);
    fmt.Println("no hardware address:", len(loopback.HardwareAddr) == 0);
    fmt.Println("up:", (Δnet.Flags)(loopback.Flags & Δnet.FlagUp) != 0);
    fmt.Println("running:", (Δnet.Flags)(loopback.Flags & Δnet.FlagRunning) != 0);
    fmt.Println("multicast:", (Δnet.Flags)(loopback.Flags & Δnet.FlagMulticast) != 0);
    fmt.Println("not broadcast:", (Δnet.Flags)(loopback.Flags & Δnet.FlagBroadcast) == 0);
    fmt.Println("not point-to-point:", (Δnet.Flags)(loopback.Flags & Δnet.FlagPointToPoint) == 0);

    fmt.Println("-- lookup round trips --");
    var byIndexAgrees = true;
    var byNameAgrees = true;

    foreach (var (_, ifi) in ifs) {
        var (found, errΔ1) = Δnet.InterfaceByIndex(ifi.Index);
        if (errΔ1 != nil || found == nil || found.Index != ifi.Index || found.Name != ifi.Name) {
            byIndexAgrees = false;
        }

        (found, errΔ1) = Δnet.InterfaceByName(ifi.Name);
        if (errΔ1 != nil || found == nil || found.Index != ifi.Index || found.Name != ifi.Name) {
            byNameAgrees = false;
        }
    }

    fmt.Println("InterfaceByIndex agrees:", byIndexAgrees);
    fmt.Println("InterfaceByName agrees:", byNameAgrees);

    (_, err) = Δnet.InterfaceByName("go2cs-no-such-adapter");
    fmt.Println("unknown name is an error:", err != nil);

    fmt.Println("-- net.InterfaceAddrs --");

    (var addrs, err) = Δnet.InterfaceAddrs();
    fmt.Println("error:", err);

    if (err != nil) {
        return;
    }

    fmt.Println("addresses reported:", len(addrs) > 0);

    var ipsValid = true;
    var masksValid = true;
    var anycastIPsValid = true;
    var loopback4 = false;
    var loopback6 = false;
    var nonLoopback = false;

    foreach (var (_, addr) in addrs) {
        switch (addr.type()) {
        case ж<Δnet.IPNet> a: {
            if (len(a.IP) != Δnet.IPv4len && len(a.IP) != Δnet.IPv6len) {
                ipsValid = false;
                continue;
            }
            var (ones, bits) = a.Mask.Size();
            if (bits == 0 || ones > bits) {
                masksValid = false;
                continue;
            }
            if (a.IP.To4() != nil && bits != (nint)(8 * Δnet.IPv4len)) {
                masksValid = false;
            }
            if (a.IP.To4() == nil && bits != (nint)(8 * Δnet.IPv6len)) {
                masksValid = false;
            }
            if (a.IP.IsLoopback()) {
                if (a.IP.To4() != nil && ones == 8) {
                    loopback4 = true;
                }
                if (a.IP.To4() == nil && ones == 128) {
                    loopback6 = true;
                }
            } else {
                nonLoopback = true;
            }
            break;
        }
        case ж<Δnet.IPAddr> a: {
            if (len(a.IP) != Δnet.IPv4len && len(a.IP) != Δnet.IPv6len) {
                anycastIPsValid = false;
            }
            break;
        }
        default: {
            var a = addr;
            ipsValid = false;
            break;
        }}
    }

    fmt.Println("unicast IPs are 4 or 16 bytes:", ipsValid);
    fmt.Println("unicast masks agree with the address family:", masksValid);
    fmt.Println("anycast IPs are 4 or 16 bytes:", anycastIPsValid);
    fmt.Println("127.0.0.1/8 present:", loopback4);
    fmt.Println("::1/128 present:", loopback6);
    fmt.Println("some non-loopback address present:", nonLoopback);

    fmt.Println("-- per-interface addresses --");

    nint perInterface = 0;
    var perInterfaceErr = false;
    nint multicast = 0;
    var multicastErr = false;
    var multicastIsMulticast = true;
    var multicastIPsValid = true;

    foreach (var (_, vᴛ1) in ifs) {
        ref var ifi = ref heap(new Δnet.Interface(), out var Ꮡifi);
        ifi = vᴛ1;

        var (ua, errΔ2) = Ꮡifi.Addrs();
        if (errΔ2 != nil) {
            perInterfaceErr = true;
        }
        perInterface += len(ua);

        (var ma, errΔ2) = Ꮡifi.MulticastAddrs();
        if (errΔ2 != nil) {
            multicastErr = true;
        }
        multicast += len(ma);

        foreach (var (_, addr) in ma) {
            var (a, ok) = addr._<ж<Δnet.IPAddr>>(ᐧ);
            if (!ok) {
                multicastIPsValid = false;
                continue;
            }
            if (len(a.IP) != Δnet.IPv4len && len(a.IP) != Δnet.IPv6len) {
                multicastIPsValid = false;
                continue;
            }
            if (!a.IP.IsMulticast()) {
                multicastIsMulticast = false;
            }
        }
    }

    fmt.Println("Addrs errors:", perInterfaceErr);
    fmt.Println("MulticastAddrs errors:", multicastErr);
    fmt.Println("per-interface addresses sum to InterfaceAddrs:", perInterface == len(addrs));
    fmt.Println("multicast addresses reported:", multicast > 0);
    fmt.Println("multicast IPs are 4 or 16 bytes:", multicastIPsValid);
    fmt.Println("multicast IPs are multicast:", multicastIsMulticast);
}

internal static bool printable(@string s) {
    if (s == ""u8) {
        return false;
    }

    foreach (var (_, r) in s) {
        if (r < 0x20 || r == 0x7f) {
            return false;
        }
    }

    return true;
}

} // end main_package
