namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static float64 matmul(nint n) {
    var a = new slice<slice<float64>>(n);
    var b = new slice<slice<float64>>(n);
    var c = new slice<slice<float64>>(n);
    for (nint i = 0; i < n; i++) {
        a[i] = new slice<float64>(n);
        b[i] = new slice<float64>(n);
        c[i] = new slice<float64>(n);
        for (nint j = 0; j < n; j++) {
            a[i][j] = (float64)((i * n + j) % 100) * 0.5D;
            b[i][j] = (float64)((i + j) % 100) * 0.25D;
        }
    }
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < n; j++) {
            var sum = 0.0D;
            for (nint k = 0; k < n; k++) {
                sum += a[i][k] * b[k][j];
            }
            c[i][j] = sum;
        }
    }
    var trace = 0.0D;
    for (nint i = 0; i < n; i++) {
        trace += c[i][i];
    }
    return trace;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    var total = 0.0D;
    for (nint r = 0; r < 4; r++) {
        total += matmul(256);
    }
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, (int64)total);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package
