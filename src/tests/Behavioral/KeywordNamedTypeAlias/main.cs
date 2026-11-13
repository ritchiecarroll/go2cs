global using long = int;
global using ulong = uint;

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @long size(nint n) { return (@long)n; }

internal static void main() {
    @ulong u = 7;
    fmt.Println(size(3), u, (@long)(-1));
}

} // end main_package
