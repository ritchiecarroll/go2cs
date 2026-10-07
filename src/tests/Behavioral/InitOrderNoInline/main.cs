namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoInit] internal static void initΔ1() {
    fmt.Println((@string)"main.go init 1"u8);
}

[MethodImpl(MethodImplOptions.NoInlining)] [GoInit] internal static void initΔ2() {
    fmt.Println((@string)"main.go init 2"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mainˢ = (@string)"main"u8;

internal static void Main() {
    fmt.Println(mainˢ);
}

} // end main_package
