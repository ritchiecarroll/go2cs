namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt two32 => /* 1 << 32 */ 4294967296;

internal static int64 sum(int64 n) {
    return n;
}

internal static void Main() {
    uint64 a = 100;
    uint64 b = 3;
    fmt.Println(a * (uint64)two32 + b);
    fmt.Println((uint64)two32 * a);
    fmt.Println(a >= two32);
    fmt.Println(a - (uint64)two32 % a);

    int64 c = 1099511627783L;
    var d = (int64)(9223372036854775807L);
    fmt.Println(c);
    fmt.Println(d);
    fmt.Println(sum(12345000054321L));

    var n = (int64)7;
    var max = (int64)(9223372036854775807UL - ((uint64)1 << 63) % (uint64)n);
    fmt.Println(max);

    float64 hf = 9223372036854775808D;
    fmt.Println(hf / 1e18D);
    fmt.Println(hf / (1152921504606846976D));

    float32 sf = 1099511627776F;
    fmt.Println(sf / (1 << 30));

    float64 big = 1099511627776D;
    fmt.Println(1099511627776D * 1.5D / big);

    float64 small = (1 << 10);
    fmt.Println(small / (1 << 3));
    fmt.Println(1e18D * 10.0D / hf);
}

} // end main_package
