namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using addrlib = ForeignIfaceFieldPointer.addrlib_package;
using ForeignIfaceFieldPointer;

partial class main_package {

internal partial struct main_type /*dyn*/ {
    internal addrlib.Addr got, want;
}

internal static void Main() {
    var ta = Ꮡ(new addrlib.UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8));
    array<main_type> connAddrs = new main_type[]{
        new(new addrlib.UnixAddrжAddr(ta), new addrlib.UnixAddrжAddr(ta)),
        new(new addrlib.UnixAddrжAddr(Ꮡ(new addrlib.UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8))), new addrlib.UnixAddrжAddr(Ꮡ(new addrlib.UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8)))),
        new(new addrlib.UnixAddrжAddr(ta), new addrlib.UnixAddrжAddr(Ꮡ(new addrlib.UnixAddr(Name: "/tmp/sock"u8, Net: "unix"u8))))
    }.array();
    foreach (var (i, ca) in connAddrs.ΔRangeSnapshot()) {
        fmt.Printf("%d got  T=%T v=%#v\n"u8, i, ca.got, ca.got);
        fmt.Printf("%d want T=%T v=%#v\n"u8, i, ca.want, ca.want);
        fmt.Printf("%d deepequal=%v\n"u8, i, reflect.DeepEqual(ca.got, ca.want));
    }
    fmt.Printf("direct T=%T v=%#v\n"u8, ta.OrTypedNil(), ta.OrTypedNil());
    addrlib.Addr nilAddr = default!;
    fmt.Printf("nil T=%T v=%v\n"u8, nilAddr, nilAddr);
}

} // end main_package
