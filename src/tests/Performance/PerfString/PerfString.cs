namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static nint run(nint n) {
    nint total = 0;
    var buf = new slice<byte>(0, 32);
    for (nint i = 0; i < n; i++) {
        buf = buf[..0];
        nint v = i;
        if (v == 0) {
            buf = append(buf, (byte)((rune)'0'));
        }
        while (v > 0) {
            buf = append(buf, (byte)((rune)'0' + v % 10));
            v /= 10;
        }
        @string s = ((@string)buf);
        total += len(s);
        total += (nint)s[0];
        if (i % 1000 == 0) {
            @string t = s + "-"u8 + s;
            total += len(t);
            total += (nint)t[len(t) - 1];
        }
    }
    return total;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    nint total = run(10000000);
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, total);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package
