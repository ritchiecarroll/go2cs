namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void swap(ref array<uint64> buf) {
    foreach (var (i, x) in buf) {
        buf[i] = x >> 32 | x << 32;
    }
}

internal static void Main() {
    var b = new uint64[]{0x100000002UL, 0x300000004UL, 5, 0}.array();
    swap(ref b);
    fmt.Println(b[0], b[1], b[2], b[3]);
}

} // end main_package
