namespace go;

using fmt = fmt_package;
using os = os_package;
using syscall = syscall_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pageSizeˢ = (@string)"page size:"u8;
private static readonly object matchesSyscallˢ = (@string)"matches syscall.Getpagesize:"u8;
private static readonly object positivePowerOfTwoˢ = (@string)"positive power of two:"u8;

internal static void Main() {
    nint size = os.Getpagesize();
    fmt.Println(pageSizeˢ, size);
    fmt.Println(matchesSyscallˢ, size == syscall.Getpagesize());
    fmt.Println(positivePowerOfTwoˢ, size > 0 && (nint)(size & (size - 1)) == 0);
}

} // end main_package
