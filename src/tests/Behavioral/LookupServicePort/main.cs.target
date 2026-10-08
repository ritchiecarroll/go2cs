namespace go;

using fmt = fmt_package;
using Δnet = net_package;

partial class main_package {

internal partial struct main_queries /*dyn*/ {
    internal @string network;
    internal @string service;
}

internal static void Main() {
    var queries = new main_queries[]{
        new("tcp"u8, "http"u8),
        new("tcp"u8, "https"u8),
        new("tcp"u8, "domain"u8),
        new("udp"u8, "domain"u8),
        new("tcp4"u8, "https"u8),
        new("tcp6"u8, "https"u8),
        new("tcp"u8, "go2cs-no-such-service"u8)
    }.slice();
    foreach (var (_, q) in queries) {
        var (port, err) = Δnet.LookupPort(q.network, q.service);
        if (err != default!) {
            fmt.Printf("%s/%s -> error\n"u8, q.network, q.service);
            continue;
        }
        fmt.Printf("%s/%s -> %d\n"u8, q.network, q.service, port);
    }
}

} // end main_package
