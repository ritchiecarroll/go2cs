namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal static error errSeek = errors.New("cannot seek"u8);

internal static @string take64(Func<(int64, error)> f) {
    var (n, err) = f();
    if (err != default!) {
        return "err:"u8 + err.Error();
    }
    return fmt.Sprintf("ok:%d"u8, n);
}

internal static @string takeF64(Func<(float64, error)> f) {
    var (x, err) = f();
    if (err != default!) {
        return "err:"u8 + err.Error();
    }
    return fmt.Sprintf("ok:%g"u8, x);
}

internal static (int64, error) seekEnd(bool ok) {
    if (!ok) {
        return (0, errSeek);
    }
    return (4096, default!);
}

internal static @string sizeFuncShape(bool ok) {
    var sizeFunc = (int64, error) () => {
        var (size, err) = seekEnd(ok);
        if (err != default!) {
            return (0, errSeek);
        }
        return (size, default!);
    };
    return take64(sizeFunc);
}

internal static @string negatedArm(bool ok) {
    var probe = (int64, error) () => {
        var (size, err) = seekEnd(ok);
        if (err != default!) {
            return (-1, default!);
        }
        return (size, default!);
    };
    return take64(probe);
}

internal static @string floatShape(bool ok) {
    var ratio = (float64, error) () => {
        if (!ok) {
            return (0D, errSeek);
        }
        return (2.5D, default!);
    };
    return takeF64(ratio);
}

internal static @string floatControl() {
    var half = () => (1.5D, errSeek);
    return takeF64(half);
}

internal static @string intControl(bool ok) {
    (nint, error) count() {
        if (!ok) {
            return (0, errSeek);
        }
        return (21, default!);
    }
    var (n, err) = count();
    if (err != default!) {
        return "err:"u8 + err.Error();
    }
    return fmt.Sprintf("ok:%d"u8, n);
}

internal static @string takeSplit(Func<slice<byte>, bool, (nint, slice<byte>, error)> f, slice<byte> data, bool atEOF) {
    var (a, t, err) = f(data, atEOF);
    if (err != default!) {
        return fmt.Sprintf("adv:%d tok:[%s] err:%s"u8, a, ((@string)t), err.Error());
    }
    return fmt.Sprintf("adv:%d tok:[%s]"u8, a, ((@string)t));
}

internal static @string mixedIntArms(slice<byte> data, bool atEOF) {
    var onComma = (nint advance, slice<byte> token, error err) (slice<byte> dataΔ1, bool atEOFΔ1) => {
        for (nint i = 0; i < len(dataΔ1); i++) {
            if (dataΔ1[i] == (rune)',') {
                return (i + 1, dataΔ1.slice(0, i), default!);
            }
        }
        if (!atEOFΔ1) {
            return (0, default!, default!);
        }
        return (0, dataΔ1, errSeek);
    };
    return takeSplit(onComma, data, atEOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sizeFuncShapeTrueˢ = (@string)"sizeFuncShape(true):"u8;
private static readonly object sizeFuncShapeFalseˢ = (@string)"sizeFuncShape(false):"u8;
private static readonly object negatedArmTrueˢ = (@string)"negatedArm(true):"u8;
private static readonly object negatedArmFalseˢ = (@string)"negatedArm(false):"u8;
private static readonly object floatShapeTrueˢ = (@string)"floatShape(true):"u8;
private static readonly object floatShapeFalseˢ = (@string)"floatShape(false):"u8;
private static readonly object floatControlˢ = (@string)"floatControl:"u8;
private static readonly object intControlTrueˢ = (@string)"intControl(true):"u8;
private static readonly object intControlFalseˢ = (@string)"intControl(false):"u8;
private static readonly object mixedIntArmsHiByeTrueˢ = (@string)"mixedIntArms(hi,bye/true):"u8;
private static readonly object mixedIntArmsTailTrueˢ = (@string)"mixedIntArms(tail/true):"u8;
private static readonly object mixedIntArmsTailFalseˢ = (@string)"mixedIntArms(tail/false):"u8;

internal static void Main() {
    fmt.Println(sizeFuncShapeTrueˢ, sizeFuncShape(true));
    fmt.Println(sizeFuncShapeFalseˢ, sizeFuncShape(false));
    fmt.Println(negatedArmTrueˢ, negatedArm(true));
    fmt.Println(negatedArmFalseˢ, negatedArm(false));
    fmt.Println(floatShapeTrueˢ, floatShape(true));
    fmt.Println(floatShapeFalseˢ, floatShape(false));
    fmt.Println(floatControlˢ, floatControl());
    fmt.Println(intControlTrueˢ, intControl(true));
    fmt.Println(intControlFalseˢ, intControl(false));
    fmt.Println(mixedIntArmsHiByeTrueˢ, mixedIntArms(slice<byte>("hi,bye"u8), true));
    fmt.Println(mixedIntArmsTailTrueˢ, mixedIntArms(slice<byte>("tail"u8), true));
    fmt.Println(mixedIntArmsTailFalseˢ, mixedIntArms(slice<byte>("tail"u8), false));
}

} // end main_package
