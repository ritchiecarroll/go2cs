namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct handle<T> {
    internal ж<T> p;
}

partial struct box<T> {
    internal T v;
}

partial struct meta<T> {
    internal nint hits;
}

partial struct optval<T> {
    internal T v;
    internal bool valid;
}

partial struct outer<T> {
    internal box<T> inner;
    internal nint n;
}

internal static void Main() {
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 5;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 5;
    var (h1, h2, h3) = (new handle<nint>(Ꮡx), new handle<nint>(Ꮡx), new handle<nint>(Ꮡy));
    fmt.Println(h1 == h2, h1 == h3, h1 == new handle<nint>(nil));
    var b1 = new box<@string>("go"u8);
    fmt.Println(b1 == new box<@string>("go"u8), b1 == new box<@string>("cs"u8));
    fmt.Println(new box<nint>(7) == new box<nint>(7), new box<nint>(7) == new box<nint>(8));
    var (m1, m2) = (new meta<@string>(3), new meta<float64>(3));
    fmt.Println(m1 == new meta<@string>(3), m2 == new meta<float64>(4));
    var (n1, n2) = (new optval<@string>("a"u8, true), new optval<@string>("a"u8, false));
    fmt.Println(n1 == new optval<@string>("a"u8, true), n1 == n2);
    var (o1, o2) = (new outer<@string>(new box<@string>("k"u8), 1), new outer<@string>(new box<@string>("k"u8), 1));
    fmt.Println(o1 == o2, o1 == new outer<@string>(new box<@string>("k"u8), 2));
    var counts = new map<box<@string>, nint>{};
    counts[new box<@string>("k"u8)]++;
    counts[new box<@string>("k"u8)]++;
    counts[new box<@string>("j"u8)]++;
    fmt.Println(counts[new box<@string>("k"u8)], counts[new box<@string>("j"u8)], len(counts));
}

} // end main_package
