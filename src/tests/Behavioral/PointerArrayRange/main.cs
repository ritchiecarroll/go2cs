namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void swap(ref array<uint64> buf) {
    foreach (var (i, x) in buf) {
        buf[i] = x >> 32 | x << 32;
    }
}

internal static nint /*n*/ lowered(ref array<nint> p) {
    nint n = default;

    foreach (var i in range(3)) {
        n += i + 1;
    }
    return n;
}

internal static nint /*n*/ aliased(/*[6]*/ ж<array<nint>> Ꮡp) {
    nint n = default;

    report(Ꮡp);
    foreach (var i in range(6)) {
        n += i;
    }
    return n;
}

internal static void report(/*[6]*/ ж<array<nint>> Ꮡp) {
    fmt.Println("aliased nil:", Ꮡp == nil);
}

partial struct words /*[5]uint64*/;

internal static nint /*n*/ count(this ref words w) {
    nint n = default;

    foreach (var _ᴛ1 in range(5)) {
        n++;
    }
    return n;
}

internal static void Main() {
    var b = new uint64[]{0x100000002UL, 0x300000004UL, 5, 0}.array();
    swap(ref b);
    fmt.Println(b[0], b[1], b[2], b[3]);

    ж<words> w = default;
    fmt.Println("lowered:", lowered(ref ((ж<array<nint>>)default).DerefOrNull()), "aliased:", aliased(ж<array<nint>>.NilBoxOfDims(6L)), "receiver:", w.count());
}

} // end main_package
