namespace go;

using fmt = fmt_package;
using Δnet = net_package;

partial class main_package {

internal partial struct main_queries /*dyn*/ {
    internal @string network;
    internal @string service;
}

internal static void Main() {
    var queries = slice<main_queries>([
        new("tcp", "http"),
        new("tcp", "https"),
        new("tcp", "domain"),
        new("udp", "domain"),
        new("tcp4", "https"),
        new("tcp6", "https"),
        new("tcp", "go2cs-no-such-service")
    ]);

    foreach (var (_, q) in queries) {
        var (port, err) = Δnet.LookupPort(q.network, q.service);

        if (err != nil) {
            fmt.Printf("%s/%s -> error\n"u8, q.network, q.service);
            continue;
        }

        fmt.Printf("%s/%s -> %d\n"u8, q.network, q.service, port);
    }
}

} // end main_package
