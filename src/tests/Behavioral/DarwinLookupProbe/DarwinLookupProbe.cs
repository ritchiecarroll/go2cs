namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using sort = sort_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string localhostˢ = "localhost"u8;

internal static void Main() {
    var (ips, err) = Δnet.LookupIP(localhostˢ);
    if (err != default!){
        fmt.Printf("LookupIP error: %T %v\n"u8, err, err);
    } else {
        slice<@string> s = default!;
        foreach (var (_, ip) in ips) {
            s = append(s, ip.String());
        }
        sort.Strings(s);
        fmt.Printf("LookupIP: %v\n"u8, s);
    }
    (var hosts, err) = Δnet.LookupHost(localhostˢ);
    if (err != default!){
        fmt.Printf("LookupHost error: %T %v\n"u8, err, err);
    } else {
        sort.Strings(hosts);
        fmt.Printf("LookupHost: %v\n"u8, hosts);
    }
}

} // end main_package
