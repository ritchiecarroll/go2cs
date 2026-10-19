namespace go;

using fmt = fmt_package;
using Δnet = net_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object netInterfacesˢ = (@string)"-- net.Interfaces --"u8;
private static readonly object errorˢ = (@string)"error:"u8;
private static readonly object interfacesReportedˢ = (@string)"interfaces reported:"u8;
private static readonly object indexesPositiveˢ = (@string)"indexes positive:"u8;
private static readonly object indexesUniqueˢ = (@string)"indexes unique:"u8;
private static readonly object namesNonEmptyˢ = (@string)"names non-empty:"u8;
private static readonly object namesPrintableˢ = (@string)"names printable:"u8;
private static readonly object namesUniqueˢ = (@string)"names unique:"u8;
private static readonly object mtUsPlausibleˢ = (@string)"MTUs plausible:"u8;
private static readonly object hardwareAddressesWithinˢ = (@string)"hardware addresses within MAX_ADAPTER_ADDRESS_LENGTH:"u8;
private static readonly object someAdapterReportsASixˢ = (@string)"some adapter reports a six-byte hardware address:"u8;
private static readonly object theLoopbackPseudoˢ = (@string)"-- the loopback pseudo-interface --"u8;
private static readonly object presentˢ = (@string)"present:"u8;
private static readonly object indexPositiveˢ = (@string)"index positive:"u8;
private static readonly object nameNonEmptyˢ = (@string)"name non-empty:"u8;
private static readonly object mtuIs1ˢ = (@string)"MTU is -1:"u8;
private static readonly object noHardwareAddressˢ = (@string)"no hardware address:"u8;
private static readonly object runningˢ = (@string)"running:"u8;
private static readonly object multicastˢ = (@string)"multicast:"u8;
private static readonly object notBroadcastˢ = (@string)"not broadcast:"u8;
private static readonly object notPointToPointˢ = (@string)"not point-to-point:"u8;
private static readonly object lookupRoundTripsˢ = (@string)"-- lookup round trips --"u8;
private static readonly object interfaceByIndexAgreesˢ = (@string)"InterfaceByIndex agrees:"u8;
private static readonly object interfaceByNameAgreesˢ = (@string)"InterfaceByName agrees:"u8;
private static readonly object unknownNameIsAnErrorˢ = (@string)"unknown name is an error:"u8;
private static readonly object netInterfaceAddrsˢ = (@string)"-- net.InterfaceAddrs --"u8;
private static readonly object addressesReportedˢ = (@string)"addresses reported:"u8;
private static readonly object unicastIPsAre4Or16Bytesˢ = (@string)"unicast IPs are 4 or 16 bytes:"u8;
private static readonly object unicastMasksAgreeWithTheˢ = (@string)"unicast masks agree with the address family:"u8;
private static readonly object anycastIPsAre4Or16Bytesˢ = (@string)"anycast IPs are 4 or 16 bytes:"u8;
private static readonly object presentˢ2 = (@string)"127.0.0.1/8 present:"u8;
private static readonly object presentˢ3 = (@string)"::1/128 present:"u8;
private static readonly object someNonLoopbackAddressˢ = (@string)"some non-loopback address present:"u8;
private static readonly object perInterfaceAddressesˢ = (@string)"-- per-interface addresses --"u8;
private static readonly object addrsErrorsˢ = (@string)"Addrs errors:"u8;
private static readonly object multicastAddrsErrorsˢ = (@string)"MulticastAddrs errors:"u8;
private static readonly object perInterfaceAddressesSumˢ = (@string)"per-interface addresses sum to InterfaceAddrs:"u8;
private static readonly object multicastAddressesˢ = (@string)"multicast addresses reported:"u8;
private static readonly object multicastIPsAre4Or16ˢ = (@string)"multicast IPs are 4 or 16 bytes:"u8;
private static readonly object multicastIPsAreMulticastˢ = (@string)"multicast IPs are multicast:"u8;

internal static void Main() {
    fmt.Println(netInterfacesˢ);

    var (ifs, err) = Δnet.Interfaces();
    fmt.Println(errorˢ, err);

    if (err != default!) {
        return;
    }

    fmt.Println(interfacesReportedˢ, len(ifs) > 0);

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

    Δnet.Interface loopback = default!;

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

    fmt.Println(indexesPositiveˢ, indexesPositive);
    fmt.Println(indexesUniqueˢ, indexesUnique);
    fmt.Println(namesNonEmptyˢ, namesNonEmpty);
    fmt.Println(namesPrintableˢ, namesPrintable);
    fmt.Println(namesUniqueˢ, namesUnique);
    fmt.Println(mtUsPlausibleˢ, mtusPlausible);
    fmt.Println(hardwareAddressesWithinˢ, hardwareAddrsWithinMax);
    fmt.Println(someAdapterReportsASixˢ, sixByteHardwareAddr);

    fmt.Println(theLoopbackPseudoˢ);
    fmt.Println(presentˢ, loopbacks > 0);
    fmt.Println(indexPositiveˢ, loopback.Index > 0);
    fmt.Println(nameNonEmptyˢ, loopback.Name != ""u8);
    fmt.Println(mtuIs1ˢ, loopback.MTU == -1);
    fmt.Println(noHardwareAddressˢ, len(loopback.HardwareAddr) == 0);
    fmt.Println((@string)"up:"u8, (Δnet.Flags)(loopback.Flags & Δnet.FlagUp) != 0);
    fmt.Println(runningˢ, (Δnet.Flags)(loopback.Flags & Δnet.FlagRunning) != 0);
    fmt.Println(multicastˢ, (Δnet.Flags)(loopback.Flags & Δnet.FlagMulticast) != 0);
    fmt.Println(notBroadcastˢ, (Δnet.Flags)(loopback.Flags & Δnet.FlagBroadcast) == 0);
    fmt.Println(notPointToPointˢ, (Δnet.Flags)(loopback.Flags & Δnet.FlagPointToPoint) == 0);

    fmt.Println(lookupRoundTripsˢ);
    var byIndexAgrees = true;
    var byNameAgrees = true;

    foreach (var (_, ifi) in ifs) {
        var (found, errΔ1) = Δnet.InterfaceByIndex(ifi.Index);
        if (errΔ1 != default! || found == nil || (~found).Index != ifi.Index || (~found).Name != ifi.Name) {
            byIndexAgrees = false;
        }

        (found, errΔ1) = Δnet.InterfaceByName(ifi.Name);
        if (errΔ1 != default! || found == nil || (~found).Index != ifi.Index || (~found).Name != ifi.Name) {
            byNameAgrees = false;
        }
    }

    fmt.Println(interfaceByIndexAgreesˢ, byIndexAgrees);
    fmt.Println(interfaceByNameAgreesˢ, byNameAgrees);

    (_, err) = Δnet.InterfaceByName("go2cs-no-such-adapter");
    fmt.Println(unknownNameIsAnErrorˢ, err != default!);

    fmt.Println(netInterfaceAddrsˢ);

    (var addrs, err) = Δnet.InterfaceAddrs();
    fmt.Println(errorˢ, err);

    if (err != default!) {
        return;
    }

    fmt.Println(addressesReportedˢ, len(addrs) > 0);

    var ipsValid = true;
    var masksValid = true;
    var anycastIPsValid = true;
    var loopback4 = false;
    var loopback6 = false;
    var nonLoopback = false;

    foreach (var (_, addr) in addrs) {
        switch (addr.type()) {
        case ж<Δnet.IPNet> a: {
            if (len((~a).IP) != Δnet.IPv4len && len((~a).IP) != Δnet.IPv6len) {
                ipsValid = false;
                continue;
            }
            var (ones, bits) = (~a).Mask.Size();
            if (bits == 0 || ones > bits) {
                masksValid = false;
                continue;
            }
            if ((~a).IP.To4() != default! && bits != (nint)(8 * Δnet.IPv4len)) {
                masksValid = false;
            }
            if ((~a).IP.To4() == default! && bits != (nint)(8 * Δnet.IPv6len)) {
                masksValid = false;
            }
            if ((~a).IP.IsLoopback()){
                if ((~a).IP.To4() != default! && ones == 8) {
                    loopback4 = true;
                }
                if ((~a).IP.To4() == default! && ones == 128) {
                    loopback6 = true;
                }
            } else {
                nonLoopback = true;
            }
            break;
        }
        case ж<Δnet.IPAddr> a: {
            if (len((~a).IP) != Δnet.IPv4len && len((~a).IP) != Δnet.IPv6len) {
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

    fmt.Println(unicastIPsAre4Or16Bytesˢ, ipsValid);
    fmt.Println(unicastMasksAgreeWithTheˢ, masksValid);
    fmt.Println(anycastIPsAre4Or16Bytesˢ, anycastIPsValid);
    fmt.Println(presentˢ2, loopback4);
    fmt.Println(presentˢ3, loopback6);
    fmt.Println(someNonLoopbackAddressˢ, nonLoopback);

    fmt.Println(perInterfaceAddressesˢ);

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
        if (errΔ2 != default!) {
            perInterfaceErr = true;
        }
        perInterface += len(ua);

        (var ma, errΔ2) = Ꮡifi.MulticastAddrs();
        if (errΔ2 != default!) {
            multicastErr = true;
        }
        multicast += len(ma);

        foreach (var (_, addr) in ma) {
            var (a, ok) = addr._<ж<Δnet.IPAddr>>(ᐧ);
            if (!ok) {
                multicastIPsValid = false;
                continue;
            }
            if (len((~a).IP) != Δnet.IPv4len && len((~a).IP) != Δnet.IPv6len) {
                multicastIPsValid = false;
                continue;
            }
            if (!(~a).IP.IsMulticast()) {
                multicastIsMulticast = false;
            }
        }
    }

    fmt.Println(addrsErrorsˢ, perInterfaceErr);
    fmt.Println(multicastAddrsErrorsˢ, multicastErr);
    fmt.Println(perInterfaceAddressesSumˢ, perInterface == len(addrs));
    fmt.Println(multicastAddressesˢ, multicast > 0);
    fmt.Println(multicastIPsAre4Or16ˢ, multicastIPsValid);
    fmt.Println(multicastIPsAreMulticastˢ, multicastIsMulticast);
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
