namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static (nint, nint) sieve(nint n) {
    var composite = new slice<bool>(n);
    nint count = 0;
    nint sum = 0;
    for (nint i = 2; i < n; i++) {
        if (!composite[i]) {
            count++;
            sum += i;
            for (nint j = i * i; j < n; j += i) {
                composite[j] = true;
            }
        }
    }
    return (count, sum);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    nint count = 0;
    nint sum = 0;
    for (nint r = 0; r < 3; r++) {
        var (c, s) = sieve(10000000);
        count += c;
        sum += s;
    }
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, count, sum);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package
