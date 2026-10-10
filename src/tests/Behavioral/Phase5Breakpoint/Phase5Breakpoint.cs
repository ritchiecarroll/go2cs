namespace go;

using fmt = fmt_package;
using runtime = runtime_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object beforeˢ = (@string)"before"u8;
private static readonly object afterˢ = (@string)"after"u8;

internal static void Main() {
    fmt.Println(beforeˢ);
    runtime.Breakpoint();
    fmt.Println(afterˢ);
}

} // end main_package
