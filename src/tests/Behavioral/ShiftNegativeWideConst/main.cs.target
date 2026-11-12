namespace go;

using fmt = fmt_package;

partial class main_package {

internal static int64 mask(nint bits) {
    return ((int64)(-1)).Lsh((nuint)bits);
}

internal static uint64 umask(nuint bits) {
    return (~(uint64)0).Lsh(bits);
}

internal static void main() {
    fmt.Println(mask(4));
    fmt.Println(mask(8));
    fmt.Println(umask(60));
}

} // end main_package
