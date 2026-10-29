namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var i32 = slice<int32>([0, 1, (int32)(1 << 20), (1 << 20) + 1, (int32)(2147483648L - 2), (int32)(2147483648L - 1)]);

    var i64 = slice<int64>([1152921504606846976L, 4611686018427387905L, 9223372036854775807L]);

    var i16 = slice<int16>([(int16)((1 << 15) - 1)]);
    var u32 = slice<uint32>([unchecked((uint32)(4294967295UL))]);
    var u64 = slice<uint64>([(uint64)(1099511627776L + 1), 18446744073709551615UL]);
    var ptr = slice<uintptr>([(uintptr)(1099511627776L + 1)]);
    var ints = slice<nint>([(nint)(2147483648L - 1)]);

    int32 n32 = (int32)(2147483648L - 1);
    var c32 = (int32)(2147483648L - 1);
    uintptr nptr = (uintptr)(1099511627776L + 1);

    foreach (var (_, v) in i32) {
        fmt.Println(v);
    }

    foreach (var (_, v) in i64) {
        fmt.Println(v);
    }

    foreach (var (_, v) in i16) {
        fmt.Println(v);
    }

    foreach (var (_, v) in u32) {
        fmt.Println(v);
    }

    foreach (var (_, v) in u64) {
        fmt.Println(v);
    }

    foreach (var (_, v) in ptr) {
        fmt.Println(v);
    }

    foreach (var (_, v) in ints) {
        fmt.Println(v);
    }

    fmt.Println(n32, c32, nptr);
    showInt32((int32)(2147483648L - 2));
    showUint32(unchecked((uint32)(4294967295UL)));

    var words = slice<Word>([unchecked((nuint)(9223372036854775809UL)), unchecked((nuint)(9223372036854775807UL))]);
    var wide = slice<uintptr>([unchecked((nuint)(9223372036854775809UL)), unchecked((nuint)(9223372036854775807UL))]);
    var uns = slice<nuint>([unchecked((nuint)(9223372036854775809UL))]);

    var u64wide = slice<uint64>([9223372036854775809UL]);

    foreach (var (_, v) in words) {
        fmt.Println(v);
    }

    foreach (var (_, v) in wide) {
        fmt.Println(v);
    }

    foreach (var (_, v) in uns) {
        fmt.Println(v);
    }

    foreach (var (_, v) in u64wide) {
        fmt.Println(v);
    }

    var negs = slice<int32>([(int32)(-(2147483648L - 1)), (int32)(-(2147483648L - 2)), (int32)(~(2147483648L - 1))]);
    var negInts = slice<nint>([(nint)(-(2147483648L - 1))]);

    var neg16 = slice<int16>([(int16)(-((1 << 15) - 1))]);

    var deep = slice<int32>([(int32)((2147483648L - 1) - 1)]);
    var deeper = slice<int32>([(int32)((1099511627776L >> 20) - 1)]);

    int32 negAssign = (int32)(-(2147483648L - 1));
    var negConv = (int32)(-(2147483648L - 1));
    var rows = slice<row>([new("-2147483647", (int32)(-(2147483648L - 1)))]);

    foreach (var (_, v) in negs) {
        fmt.Println(v);
    }

    foreach (var (_, v) in negInts) {
        fmt.Println(v);
    }

    foreach (var (_, v) in neg16) {
        fmt.Println(v);
    }

    foreach (var (_, v) in deep) {
        fmt.Println(v);
    }

    foreach (var (_, v) in deeper) {
        fmt.Println(v);
    }

    fmt.Println(negAssign, negConv, rows);
    showInt32((int32)(-(2147483648L - 1)));
    showInt32((int32)((2147483648L - 1) - 2));
}

partial struct row {
    internal @string @in;
    internal int32 @out;
}

partial struct Word /*num:uintptr*/;

internal static UntypedInt _W => 64;
internal static readonly GoBigConst _B = /* 1 << _W */
    GoBigConst.Parse("18446744073709551616");
internal static UntypedInt _M => /* _B - 1 */ 18446744073709551615;

internal static void showInt32(int32 v) {
    fmt.Println(v);
}

internal static void showUint32(uint32 v) {
    fmt.Println(v);
}

} // end main_package
