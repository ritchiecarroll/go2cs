namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Hash /*[4]byte*/;

internal static array<byte> sum(slice<byte> b) {
    array<byte> @out = new(4);
    foreach (var (i, v) in b) {
        @out[i % 4] += v;
    }
    return @out.Clone();
}

internal static void Main() {
    Hash h = default;
    if (h != ((Hash)(sum(default)))) {
        fmt.Println("differs");
        return;
    }

    fmt.Println("equal");

    var g = (Hash)sum(new byte[]{1, 2, 3, 4}.slice());
    fmt.Println(g == ((Hash)(sum(new byte[]{1, 2, 3, 4}.slice()))), ((Hash)(sum(new byte[]{1, 2, 3, 4}.slice()))) == g);
    fmt.Println(g != ((Hash)(sum(default))), ((Hash)(sum(default))) != g, ((Hash)(sum(default))) == h);
}

} // end main_package
