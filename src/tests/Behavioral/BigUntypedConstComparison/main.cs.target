namespace go;

using fmt = fmt_package;

partial class main_package {

public static readonly GoBigConst Two129 = /* 1 << 129 */
    GoBigConst.Parse("680564733841876926926749214863536422912");
internal static readonly GoBigConst below1e23 = /* 99999999999999974834176 */
    GoBigConst.Parse("99999999999999974834176");
internal static readonly GoBigConst above1e23 = /* 100000000000000008388608 */
    GoBigConst.Parse("100000000000000008388608");
internal static UntypedInt small => 1000;

partial struct ftoaTest {
    internal float64 f;
    internal byte fmtc;
    internal nint prec;
    internal @string s;
}

internal static slice<ftoaTest> ftoatests = slice<ftoaTest>([
    new((float64)below1e23, (byte)'e', 17, "9.99999999999999748e+22"),
    new((float64)above1e23, (byte)'f', 17, "100000000000000008388608.00000000000000000"),
    new(small, (byte)'g', -1, "1000")
]);

internal static slice<float64> floats = slice<float64>([(float64)below1e23, (float64)above1e23, small]);

internal static array<float64> floatArr = new float64[]{(float64)below1e23, (float64)above1e23}.array();

internal static map<@string, float64> byName = new map<@string, float64>{["below"] = (float64)below1e23, ["above"] = (float64)above1e23, ["small"] = small};

internal static ftoaTest keyed = new ftoaTest(f: (float64)below1e23, fmtc: (byte)'g', prec: -1, s: "keyed");

internal static slice<slice<float64>> nested = slice<slice<float64>>([slice<float64>([(float64)below1e23]), slice<float64>([(float64)above1e23])]);

internal static float32 asFloat32 = (float32)below1e23;

internal static float64 take(float64 f) {
    return f;
}

internal static float64 give() {
    return (float64)above1e23;
}

internal static void Main() {
    float64 x = 1e40D;
    fmt.Println(x > (float64)Two129);
    fmt.Println(x < (float64)Two129);
    float64 y = 1e30D;
    fmt.Println(y >= (float64)Two129);

    fmt.Println(ftoatests[0].f, ftoatests[1].f, ftoatests[2].f);
    fmt.Println(floats[0], floats[1], floats[2]);
    fmt.Println(floatArr[0], floatArr[1]);
    fmt.Println(byName["below"], byName["above"], byName["small"]);
    fmt.Println(keyed.f, nested[0][0], nested[1][0], asFloat32);

    fmt.Println(take((float64)below1e23), give());

    float64 v = (float64)above1e23;
    v = (float64)below1e23;
    fmt.Println(v);

    var local = slice<float64>([(float64)below1e23, small]);
    fmt.Println(local[0], local[1]);

    var ch = new channel<float64>(1);
    ch.ᐸꟷ((float64)below1e23);
    fmt.Println(ᐸꟷ(ch));

    fmt.Println(take(small), (nint)(small * 2), (float64)((float64)small / 4D));

    foreach (var (_, u) in slice<uint64>([0, 999, 1000, 1001, (uint64)1 << 62, (uint64)1 << 63, 18446744073709551615UL])) {
        fmt.Println(u, u < small, u <= small, u > small, u >= small, u == small, u != small);
    }

    foreach (var (_, i) in slice<int64>([-9223372036854775808L, -1, 0, 1000, 9223372036854775807L])) {
        fmt.Println(i, i < small, i > small, i == small);
    }

    fmt.Println(fastPath(0), fastPath(999), fastPath(1000), fastPath((uint64)1 << 63), fastPath(18446744073709551615UL));
}

internal static @string fastPath(uint64 u) {
    if (u < small) {
        return "small";
    }
    return "big";
}

} // end main_package
