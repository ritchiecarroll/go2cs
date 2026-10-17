namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object matchˢ = (@string)"match"u8;
private static readonly object nomatchˢ = (@string)"nomatch"u8;

internal static Action<T, bool> wantValue<T>(T want) {
    return (T got, bool ok) => {
        if (ok && AreEqual(got, want)){
            fmt.Println(matchˢ, got);
        } else {
            fmt.Println(nomatchˢ, got, want, ok);
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object presentˢ = (@string)"present"u8;

internal static Action<bool> wantPresent<T>(T want) {
    return (bool ok) => {
        fmt.Println(presentˢ, want, ok);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tookˢ = (@string)"took"u8;

internal static void takeOnly<T>(T want) {
    fmt.Println(tookˢ, want);
}

internal static T bareResult<T>(T v) {
    return v;
}

internal static slice<T> sliceOf<T>(T v) {
    return new T[]{v}.slice();
}

internal static map<K, V> mapTo<K, V>(K k, V v) {
    return new map<K, V>{[k] = v};
}

internal static channel<T> chanOf<T>(T v) {
    var ch = new channel<T>(1);
    ch.ᐸꟷ(v);
    return ch;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pairˢ = (@string)"pair"u8;

internal static Action<V, bool> pairValue<K, V>(K k, V v) {
    return (V got, bool ok) => {
        fmt.Println(pairˢ, k, got, ok);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object likeˢ = (@string)"like"u8;

internal static Action<T, bool> wantLike<T>(T proto, T want) {
    return (T got, bool ok) => {
        fmt.Println(likeˢ, got, want, ok);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object i64ˢ = (@string)"i64"u8;

internal static Action<T, bool> wantInt64<T>(T want)
    where T : /* int64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return (T got, bool ok) => {
        fmt.Println(i64ˢ, got, want, ok);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object bareˢ = (@string)"bare"u8;
private static readonly object sliceˢ = (@string)"slice"u8;
private static readonly object mapˢ = (@string)"map"u8;
private static readonly @string oneˢ = "one"u8;
private static readonly object chanˢ = (@string)"chan"u8;

internal static void Main() {
    nint i = 7;

    wantValue((nint)(0))(i, false);
    wantValue((nint)(7))(i, true);

    wantPresent<nint>(15)(true);
    takeOnly<nint>(15);

    wantValue<nint>(0)(i, false);

    wantValue(i)(i, true);

    fmt.Println(bareˢ, bareResult<nint>(42));

    float64 f = 2.5D;
    wantValue(0.0D)(f, false);
    wantValue(2.5D)(f, true);

    rune r = (rune)'q';
    wantValue((rune)'a')(r, false);
    wantValue((rune)'q')(r, true);

    @string s = "hi"u8;
    wantValue((@string)"")(s, false);
    wantValue((@string)"hi")(s, true);

    bool b = true;
    wantValue(true)(b, true);

    pairValue((@string)"k", (nint)(0))(i, false);

    wantValue((nint)(3 + 4))(i, true);
    wantValue((nint)((1 << (int)(10))))((1 << (int)(10)), true);

    fmt.Println(sliceˢ, sliceOf((nint)(9)));
    fmt.Println(mapˢ, mapTo(oneˢ, (nint)(1))[oneˢ]);
    fmt.Println(chanˢ, ᐸꟷ(chanOf((nint)(11))));

    wantLike((byte)0, (byte)(3))(3, true);
    wantInt64((int64)(1234567890123L))(1234567890123L, true);
}

} // end main_package
