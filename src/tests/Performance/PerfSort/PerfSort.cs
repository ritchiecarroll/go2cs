namespace go;

using fmt = fmt_package;
using sort = sort_package;
using time = time_package;

partial class main_package {

internal static nint run(nint n) {
    var a = new slice<nint>(n);
    uint64 x = 88172645463325252UL;
    for (nint i = 0; i < n; i++) {
        x ^= (uint64)((x << (int)(13)));
        x ^= (uint64)((x >> (int)(7)));
        x ^= (uint64)((x << (int)(17)));
        a[i] = (nint)(x % 1000000007);
    }
    sort.Ints(a);
    return a[0] + a[n / 4] + a[n / 2] + a[3 * n / 4] + a[n - 1];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    nint total = run(2000000);
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, total);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package
