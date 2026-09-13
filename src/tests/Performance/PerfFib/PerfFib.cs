namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static nint fib(nint n) {
    if (n < 2) {
        return n;
    }
    return fib(n - 1) + fib(n - 2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    nint sum = 0;
    for (nint i = 0; i < 5; i++) {
        sum += fib(34);
    }
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, sum);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package
