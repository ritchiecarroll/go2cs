namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint accumulate(ж<nint> counter, slice<nint> vals) {
    void add(nint v) {
        if (counter == nil) {
            counter = @new<nint>();
        }
        counter.Value += v;
    }

    foreach (var (_, v) in range(vals)) {
        add(v);
    }

    return counter.Value;
}

internal static void main() {
    ref var n = ref heap<nint>(0, out var Ꮡn);
    fmt.Println(accumulate(Ꮡn, [1, 2, 3, 4]));
    fmt.Println(n);
}

} // end main_package
