namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct holder {
    internal ж<slice<nint>> xs;
    internal ж<nint> cnt;
}

internal static nint sumRange(ref holder h) {
    nint s = 0;
    foreach (var (_, x) in h.xs.ValueSlot) {
        s += x;
    }
    return s;
}

internal static nint readVal(ref holder h) {
    return h.cnt.Value;
}

internal static void Main() {
    ref var xs = ref heap<slice<nint>>(slice<nint>([10, 20, 30]), out var Ꮡxs);
    ref var c = ref heap<nint>(7, out var Ꮡc);
    var h = Ꮡ(new holder(xs: Ꮡxs, cnt: Ꮡc));
    fmt.Println(sumRange(ref (h).DerefOrNull()));
    fmt.Println(readVal(ref (h).DerefOrNull()));
}

} // end main_package
