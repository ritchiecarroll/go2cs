namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static nint run(nint n) {
    var m = new map<nint, nint>();
    for (nint i = 0; i < n; i++) {
        m[i * unchecked((nint)2654435761L) % n] = i;
    }
    nint total = len(m);
    for (nint i = 0; i < n; i++) {
        {
            var (v, ok) = m[i, ꟷ]; if (ok) {
                total += (nint)(v & 1023);
            }
        }
    }
    for (nint i = 0; i < n; i += 2) {
        delete(m, i);
    }
    return total + len(m);
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
