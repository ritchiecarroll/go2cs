namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial interface Addr :
    fmt.Stringer
{
    @string Network();
}

partial struct UnixAddr {
    public @string Name;
    public @string Net;
}

public static @string Network(this ref UnixAddr a) {
    return a.Net;
}

public static @string String(this ref UnixAddr a) {
    return a.Name;
}

internal partial struct main_type /*dyn*/ {
    internal Addr got, want;
}

internal partial struct main_named /*dyn*/ {
    internal Addr got;
}

internal static void Main() {
    var ta = Ꮡ(new UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8));
    array<main_type> connAddrs = new main_type[]{
        new(new UnixAddrжAddr(ta), new UnixAddrжAddr(ta)),
        new(new UnixAddrжAddr(Ꮡ(new UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8))), new UnixAddrжAddr(Ꮡ(new UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8)))),
        new(new UnixAddrжAddr(ta), new UnixAddrжAddr(Ꮡ(new UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8))))
    }.array();
    foreach (var (i, ca) in connAddrs.ΔRangeSnapshot()) {
        fmt.Printf("%d got  T=%T v=%#v\n"u8, i, ca.got, ca.got);
        fmt.Printf("%d want T=%T v=%#v\n"u8, i, ca.want, ca.want);
        fmt.Printf("%d deepequal=%v\n"u8, i, reflect.DeepEqual(ca.got, ca.want));
    }
    fmt.Printf("direct T=%T v=%#v\n"u8, ta.OrTypedNil(), ta.OrTypedNil());
    var n = new main_named(got: new UnixAddrжAddr(ta));
    fmt.Printf("named T=%T v=%#v\n"u8, n.got, n.got);
    Addr nilAddr = default!;
    fmt.Printf("nil T=%T v=%v\n"u8, nilAddr, nilAddr);
}

} // end main_package
