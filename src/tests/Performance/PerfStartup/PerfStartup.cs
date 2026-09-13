namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    fmt.Println(checksumˢ, (nint)(42));
    fmt.Println(elapsedNsˢ, (nint)(0));
}

} // end main_package
