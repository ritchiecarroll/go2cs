namespace go;

using fmt = fmt_package;
using Δnet = net_package;
using reflect = reflect_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string l2tpˢ = "l2tp"u8;
private static readonly @string l2tpGreˢ = "l2tp:gre"u8;
private static readonly @string tcpˢ = "tcp"u8;

internal partial struct main_rows /*dyn*/ {
    internal @string network;
    internal @string addr;
    internal error want;
}

internal static void Main() {
    var rows = new main_rows[]{
        new("l2tp"u8, "127.0.0.1"u8, new net_UnknownNetworkErrorᴠerror(((Δnet.UnknownNetworkError)(@string)l2tpˢ))),
        new("l2tp:gre"u8, "127.0.0.1"u8, new net_UnknownNetworkErrorᴠerror(((Δnet.UnknownNetworkError)(@string)l2tpGreˢ))),
        new("tcp"u8, "1.2.3.4:123"u8, new net_UnknownNetworkErrorᴠerror(((Δnet.UnknownNetworkError)(@string)tcpˢ)))
    }.slice();
    foreach (var (_, r) in rows) {
        var (addr, err) = Δnet.ResolveIPAddr(r.network, r.addr);
        fmt.Printf("ResolveIPAddr(%q):\n"u8, r.network);
        fmt.Printf("   addr nil:                  %v\n"u8, addr == nil);
        fmt.Printf("   err text equal:            %v\n"u8, err != default! && err.Error() == r.want.Error());
        fmt.Printf("   err == want:               %v\n"u8, AreEqual(err, r.want));
        fmt.Printf("   DeepEqual(err,want):       %v\n"u8, reflect.DeepEqual(err, r.want));
        fmt.Printf("   TypeOf(err).String():      %v\n"u8, reflect.TypeOf(err).String());
        fmt.Printf("   TypeOf(want).String():     %v\n"u8, reflect.TypeOf(r.want).String());
        fmt.Printf("   TypeOf(err)==TypeOf(want): %v\n"u8, AreEqual(reflect.TypeOf(err), reflect.TypeOf(r.want)));
        fmt.Printf("   ValueOf(err).Kind():       %v\n"u8, reflect.ValueOf(err).Kind().String());
        fmt.Printf("   ValueOf(want).Kind():      %v\n"u8, reflect.ValueOf(r.want).Kind().String());
        fmt.Printf("   TypeOf(want).Kind():       %v\n"u8, reflect.TypeOf(r.want).Kind().String());
        fmt.Printf("   ValueOf(err).String():     %q\n"u8, reflect.ValueOf(err).String());
        fmt.Printf("   ValueOf(want).String():    %q\n"u8, reflect.ValueOf(r.want).String());
    }
}

} // end main_package
