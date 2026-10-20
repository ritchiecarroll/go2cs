namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoInit] internal static void initΔ1() {
    fmt.Println("main.go init 1");
}

[MethodImpl(MethodImplOptions.NoInlining)] [GoInit] internal static void initΔ2() {
    fmt.Println("main.go init 2");
}

internal static void Main() {
    fmt.Println("main");
}

} // end main_package
