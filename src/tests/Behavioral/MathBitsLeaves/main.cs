namespace go;

using fmt = fmt_package;
using bits = math.bits_package;
using math;

partial class main_package {

[GoLazy] partial class lazyᴛedges { internal static slice<uint64> edges = new uint64[]{
    0, 1, 2, 9223372036854775807UL, (uint64)1 << 63, 9223372036854775809UL, 18446744073709551614UL, 18446744073709551615UL,
    (uint64)(4294967296L - 1), 18446744069414584320UL, 0x5555555555555555UL, 0xAAAAAAAAAAAAAAAAUL
}.slice(); }

internal static void Main() {
    foreach (var (_, x) in edges) {
        foreach (var (_, y) in edges) {
            var (s0, c0) = bits.Add64(x, y, 0);
            var (s1, c1) = bits.Add64(x, y, 1);
            var (d0, b0) = bits.Sub64(x, y, 0);
            var (d1, b1) = bits.Sub64(x, y, 1);
            var (hi, lo) = bits.Mul64(x, y);
            fmt.Printf("%x %x: add %x/%d %x/%d sub %x/%d %x/%d mul %x:%x\n"u8, x, y, s0, c0, s1, c1, d0, b0, d1, b1, hi, lo);
        }
    }

    uint64 state = 0x9E3779B97F4A7C15UL;
    uint64 next() {
        state ^= (uint64)((state << 13));
        state ^= (uint64)((state >> 7));
        state ^= (uint64)((state << 17));
        return state;
    }
    uint64 addSum = default!;
    uint64 subSum = default!;
    uint64 mulSum = default!;
    for (nint i = 0; i < 100000; i++) {
        var (x, y, bit) = (next(), next(), (uint64)(i & 1));
        var (s, c) = bits.Add64(x, y, bit);
        var (d, b) = bits.Sub64(x, y, bit);
        var (hi, lo) = bits.Mul64(x, y);
        addSum = addSum * 31 + s + c;
        subSum = subSum * 31 + d + b;
        mulSum = mulSum * 31 + hi ^ lo;
    }
    fmt.Printf("sweep: add %x sub %x mul %x\n"u8, addSum, subSum, mulSum);
}

} // end main_package
