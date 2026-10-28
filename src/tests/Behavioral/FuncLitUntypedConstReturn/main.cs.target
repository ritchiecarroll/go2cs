namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt maxRune => /* '\U0010FFFF' */ 1114111;

internal static UntypedInt runeSelf => 0x80;

internal static UntypedInt bigConst => /* 1 << 40 */ 1099511627776;

internal static rune apply(Func<rune, rune> f, rune r) {
    return f(r);
}

internal static nint compareWith(uint16 a, uint16 b, Func<uint16, uint16, nint> cmp) {
    return cmp(a, b);
}

internal static int64 applyInt64(Func<bool, int64> f, bool v) {
    return f(v);
}

internal static int32 applyInt32(Func<bool, int32> f, bool v) {
    return f(v);
}

internal static float32 applyFloat32(Func<float32> f) {
    return f();
}

internal static float64 applyFloat64(Func<float64> f) {
    return f();
}

internal static void Main() {
    var maxFn = rune (rune _) => maxRune;
    fmt.Println(apply(maxFn, 'a'));

    var encode = rune (rune r) => {
        if (r == runeSelf) {
            return maxRune;
        }
        if (r == maxRune) {
            return runeSelf;
        }
        return r;
    };
    fmt.Println(apply(encode, runeSelf));
    fmt.Println(apply(encode, maxRune));
    fmt.Println(apply(encode, 'x'));

    int64 pick(bool neg) {
        if (neg) {
            return -1;
        }
        return bigConst;
    }
    fmt.Println(pick(false), pick(true));

    var invalid = rune (rune r) => maxRune + 1;
    fmt.Println(apply(invalid, 'c'));

    var shrink = (rune r) => (rune)'a';
    fmt.Println(apply(shrink, maxRune));

    var isBetter = nint (uint16 a, uint16 b) => {
        if (a < b) {
            return -1;
        } else if (a > b) {
            return +1;
        }
        return 0;
    };
    fmt.Println(compareWith(3, 9, isBetter), compareWith(9, 3, isBetter), compareWith(4, 4, isBetter));

    var scale = int64 (bool on) => {
        if (on) {
            return 9;
        }
        return 0;
    };
    fmt.Println(applyInt64(scale, true), applyInt64(scale, false));

    var half = () => 0.5F;
    fmt.Println(applyFloat32(half));

    var whole = () => 3D;
    fmt.Println(applyFloat64(whole));

    var rank = (bool hi) => {
        if (hi) {
            return 100;
        }
        return -100;
    };
    fmt.Println(applyInt32(rank, true), applyInt32(rank, false));

    fmt.Println(apply((rune _) => maxRune, 'b'));
}

} // end main_package
