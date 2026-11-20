namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct box {
    internal nint n;
}

} partial class mainᴛbox {

internal static ж<box> get(this ж<box> Ꮡb) { return Ꮡb; }

} partial class main_package {

internal static partial nint run() {
    ref var arr = ref heap(new array<box>(3), out var Ꮡarr);
    for (nint i = 0; i < 3; i++) {
        var xΔ1 = Ꮡarr.at<box>(i);
        xΔ1.get().n = i * 10;
    }
    var x = Ꮡarr.at<box>(1);
    return x.get().n + arr[0].n + arr[2].n;
}

internal static void main() {
    fmt.Println(run());
}

} // end main_package
