namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt replacementChar => /* '�' */ 65533;

internal static UntypedFloat scale => 2.5;

internal static UntypedFloat cbrtC => 5.42857142857142815906e-01;

internal static UntypedFloat cbrtD => -7.05306122448979611050e-01;

internal static UntypedFloat folded => /* 19.0 / 35.0 */ 0.5428571428571428;

internal static UntypedInt fsize => 5;

internal static void Main() {
    rune codepoint = replacementChar;
    @string s = ((@string)codepoint);
    fmt.Println(len(s), codepoint);

    float64 factor = scale;
    fmt.Println(factor * 2D);

    fmt.Println((float64)(cbrtC));
    fmt.Println((float64)(cbrtD));
    fmt.Println((float64)(folded));
    fmt.Println(localPrecision(2.0D));
    fmt.Println(fmt.Sprintf("size=%d"u8, (nint)(fsize + 1)));
    tightenGuards();
}

internal static void tightenGuards() {
    GoFrame ᒐ = default;
    try {
        var u16ᴷ = new stackAppend<uint16>();

        UntypedFloat mixed = 0.25;
        float64 f64 = mixed;
        float32 f32 = mixed;
        fmt.Println(f64, f32);

        UntypedFloat feeder = 3.5;
        const float64 derived = /* feeder * 2 */ 7;
        fmt.Println((float64)(derived));

        UntypedInt repetitions = 100000;
        nint loopBound = repetitions;
        var mean = /* .5 * repetitions */ 50000D;
        float64 quarterMean = /* .25 * repetitions */ 25000D;
        fmt.Println(loopBound, mean, quarterMean);

        const int64 big = /* 1 << 62 */ 4611686018427387904;
        int64 n = 1;
        fmt.Println(n + big);

        UntypedInt sh = 3;
        uint64 v2 = 1;
        fmt.Println((v2 << (int)(sh)));

        const nint shifted = 3;
        nuint k = 2;
        fmt.Println(shifted.Lsh(k));

        const uint16 localMarker = 0xFFFD;
        slice<uint16> u16 = default!;
        u16 = append(u16ᴷ, u16, localMarker);
        fmt.Println(u16[0]);

        const byte cb = 200;
        nuint sh1 = 1;
        byte b = 1;
        fmt.Println((byte)(b + ((byte)cb).Lsh(sh1)));

        const int16 c16 = 30000;
        int16 i16 = 1;
        fmt.Println((int16)(i16 + ((int16)c16).Lsh(sh1)));

        const uint16 cu16 = 60000;
        uint16 w16 = 1;
        fmt.Println((uint16)(w16 + ((uint16)cu16).Lsh(sh1)));

        const nint infinity = 1000000;
        nint lineCount = 3;
        if (lineCount < infinity) {
            fmt.Println(lineCount + infinity);
        }

        const nint localDefer = 42;
        defer(ᴛ1 => fmt.Println(ᴛ1), (nint)(localDefer), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static float64 localPrecision(float64 x) {
    const float64 c = 5.42857142857142815906e-01;
    const float64 d = -7.05306122448979611050e-01;
    const float64 smallestNormal = 2.22507385850720138309e-308;
    if (x < smallestNormal) {
        return d;
    }
    return c + x * d;
}

} // end main_package
