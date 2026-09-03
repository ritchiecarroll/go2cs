namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sumˢ = (@string)"sum:"u8;
private static readonly object totalˢ = (@string)"total:"u8;
private static readonly object indexˢ = (@string)"index:"u8;
private static readonly object numˢ = (@string)"num:"u8;
private static readonly object totalˢ2 = (@string)"Total:"u8;
private static readonly object keyˢ = (@string)"key:"u8;
private static readonly object valueˢ = (@string)"value:"u8;
private static readonly @string subTestˢ = "sub-test"u8;
private static readonly object valˢ = (@string)"val:"u8;
private static readonly @string testˢ = "test"u8;

internal static void Main() {
    var nums = new nint[]{2, 3, 4}.slice();
    nint sum = 0;
    nint i = default!;
    nint num = default!;
    nint total = default!;
    foreach (var (iᴛ1, vᴛ1) in nums) {
        i = iᴛ1;
        num = vᴛ1;

        sum += num;
        total += i;
    }
    fmt.Println(sumˢ, sum, totalˢ, total);
    foreach (var (iΔ1, numΔ1) in nums) {
        if (numΔ1 == 3) {
            fmt.Println(indexˢ, iΔ1);
        }
    }
    foreach (var (_, numΔ2) in nums) {
        fmt.Println(numˢ, numΔ2);
    }
    foreach (var (iΔ2, _) in nums) {
        fmt.Println(indexˢ, iΔ2);
    }
    total = 0;
    foreach ((_, _) in nums) {
        total++;
    }
    fmt.Println(totalˢ2, total);
    var kvs = new map<@string, @string>{["a"u8] = "apple"u8, ["b"u8] = "banana"u8};
    foreach (var (kΔ1, vΔ1) in kvs) {
        fmt.Printf("%s -> %s\n"u8, kΔ1, vΔ1);
    }
    foreach (var (kΔ2, _) in kvs) {
        fmt.Println(keyˢ, kΔ2);
    }
    foreach (var (vΔ2, _) in kvs) {
        fmt.Println(valueˢ, vΔ2);
    }
    @string k = default!;
    @string v = default!;
    foreach (var (kᴛ1, vᴛ2) in kvs) {
        k = kᴛ1;
        v = vᴛ2;

        fmt.Printf("%s2 -> %s\n"u8, k, v);
        foreach (var (kΔ3, vΔ3) in kvs) {
            fmt.Printf("%s -> %s\n"u8, kΔ3, vΔ3);
        }
        @string strΔ1 = subTestˢ;
        nint i1Δ1 = default!;
        rune c1Δ1 = default!;
        foreach (var (iᴛ2, rᴛ1) in strΔ1) {
            i1Δ1 = iᴛ2;
            c1Δ1 = rᴛ1;

            fmt.Println(i1Δ1, c1Δ1);
        }
    }
    foreach (var (kᴛ2, _) in kvs) {
        k = kᴛ2;

        fmt.Println(keyˢ, k);
    }
    foreach (var (_, vᴛ3) in kvs) {
        v = vᴛ3;

        fmt.Println(valˢ, v);
    }
    total = 0;
    foreach ((_, _) in kvs) {
        total++;
    }
    fmt.Println(totalˢ2, total);
    foreach (var (iΔ3, c) in (@string)"go"u8) {
        fmt.Println(iΔ3, c);
    }
    @string str = testˢ;
    nint i1 = default!;
    rune c1 = default!;
    foreach (var (iᴛ3, rᴛ2) in str) {
        i1 = iᴛ3;
        c1 = rᴛ2;

        fmt.Println(i1, c1);
    }
    var arr = new array<nint>(5){[2] = 42, [4] = 100};
    foreach (var (iΔ4, vΔ4) in arr.ΔRangeSnapshot()) {
        fmt.Println(iΔ4, vΔ4);
    }
    var Δslice = new slice<nint>(5){[2] = 42, [4] = 100};
    foreach (var (iΔ5, vΔ5) in Δslice) {
        fmt.Println(iΔ5, vΔ5);
    }
    nint v1 = default!;
    foreach (var (iᴛ4, vᴛ4) in Δslice) {
        i1 = iᴛ4;
        v1 = vᴛ4;

        fmt.Println(i1, v1);
    }
    var farr = new float32[]{1.1F, 2.2F, 3.3F}.array();
    foreach (var (iΔ6, vΔ6) in farr.ΔRangeSnapshot()) {
        fmt.Println(iΔ6, vΔ6);
    }
    for (nint iΔ7 = 0; iΔ7 < 10; iΔ7++) {
        nint j = default!;
        for (j = 0; j < 5; j++) {
            fmt.Println(iΔ7, j);
        }
    }
    for (nint x = 0; x < 10; x++) {
        for (nint xΔ1 = 0; xΔ1 < 5; xΔ1++) {
            for (nint xΔ2 = 0; xΔ2 < 3; xΔ2++) {
                for (nint xΔ3 = 0; xΔ3 < 2; xΔ3++) {
                    fmt.Println(xΔ3);
                }
                fmt.Println(xΔ2);
            }
            fmt.Println(xΔ1);
        }
        fmt.Println(x);
    }
    total = 0;
    foreach (var _ᴛ1 in range(3)) {
        _ = len(kvs);
        total++;
    }
    fmt.Println(totalˢ2, total);
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(7);
    ch.ᐸꟷ(8);
    close(ch);
    foreach (var _ᴛ2 in ch) {
        _ = len(kvs);
        total++;
    }
    fmt.Println(totalˢ2, total);
}

internal static void calculate(nint x) {
    nint y = x * 2;
    {
        nint xΔ1 = y - 1; if (xΔ1 > 0) {
            fmt.Println(xΔ1);
        }
    }
}

} // end main_package
