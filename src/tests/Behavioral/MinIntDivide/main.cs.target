namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using os = os_package;

partial class main_package {

[GoType("num:int64")] partial struct word;

[GoType("num:int32")] partial struct small;

internal static nint i = Δmath.MinInt;
internal static int32 i32 = Δmath.MinInt32;
internal static int64 i64 = Δmath.MinInt64;
internal static rune r = Δmath.MinInt32;
internal static word w = Δmath.MinInt64;
internal static small s = Δmath.MinInt32;
internal static int8 i8 = Δmath.MinInt8;
internal static int16 i16 = Δmath.MinInt16;
internal static nint m1 = -1;
internal static nint zero = 0;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nonzeroˢ = "nonzero"u8;

internal static @string localRem() {
    {
        var rem = builtin.rem(i64, (int64)m1); if (rem == 0) {
            return fmt.Sprint(rem);
        }
    }
    return nonzeroˢ;
}

internal static void arm(@string @class, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        if (len(os.Args) > 1 && os.Args[1] != @class) {
            return;
        }
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(@class + ": panic:", r);
                }
            }
        }, ref ᒐ);
        fmt.Println(@class + ":", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string intˢ = "int"u8;
private static readonly @string int32ˢ = "int32"u8;
private static readonly @string int64ˢ = "int64"u8;
private static readonly @string runeˢ = "rune"u8;
private static readonly @string namedInt64ˢ = "named int64"u8;
private static readonly @string namedInt32ˢ = "named int32"u8;
private static readonly @string narrowˢ = "narrow"u8;
private static readonly @string compoundˢ = "compound"u8;
private static readonly @string constant1ˢ = "constant -1"u8;
private static readonly @string ordinaryˢ = "ordinary"u8;
private static readonly @string localNamedRemˢ = "local named rem"u8;
private static readonly @string divideByZeroˢ = "divide by zero"u8;
private static readonly @string remainderByZeroˢ = "remainder by zero"u8;

internal static void Main() {
    arm(intˢ, () => fmt.Sprint(quo(i, m1) == Δmath.MinInt, (@string)" "u8, rem(i, m1)));
    arm(int32ˢ, () => fmt.Sprint(quo(i32, (int32)m1), (@string)" "u8, rem(i32, (int32)m1)));
    arm(int64ˢ, () => fmt.Sprint(quo(i64, (int64)m1), (@string)" "u8, rem(i64, (int64)m1)));
    arm(runeˢ, () => fmt.Sprint(quo(r, (rune)m1), (@string)" "u8, rem(r, (rune)m1)));
    arm(namedInt64ˢ, () => fmt.Sprint(w / ((word)(int64)m1), (@string)" "u8, w % ((word)(int64)m1)));
    arm(namedInt32ˢ, () => fmt.Sprint(s / ((small)(int32)m1), (@string)" "u8, s % ((small)(int32)m1)));
    arm(narrowˢ, () => fmt.Sprint((int8)(i8 / (int8)m1), (@string)" "u8, (int8)(i8 % (int8)m1), (@string)" "u8, (int16)(i16 / (int16)m1), (@string)" "u8, (int16)(i16 % (int16)m1)));
    arm(compoundˢ, () => {
        var (q, m) = (i64, i64);
        q = quo(q, (int64)m1);
        m = rem(m, (int64)m1);
        return fmt.Sprint(q, (@string)" "u8, m);
    });
    arm(constant1ˢ, () => {
        var (q, m) = (i32, i32);
        q = quo(q, -1);
        m = rem(m, -1);
        return fmt.Sprint(unchecked(-i) == Δmath.MinInt, (@string)" "u8, (nint)0, (@string)" "u8, unchecked(-i64), (@string)" "u8, (int64)0, (@string)" "u8, q, (@string)" "u8, m);
    });
    arm(ordinaryˢ, () => fmt.Sprint(i64 / (int64)7, (@string)" "u8, i64 % (int64)7, (@string)" "u8, quo(-7, m1), (@string)" "u8, rem(7, (m1 - 2))));
    arm(localNamedRemˢ, localRem);
    arm(divideByZeroˢ, () => fmt.Sprint(quo(i64, (int64)zero)));
    arm(remainderByZeroˢ, () => fmt.Sprint(rem(i, zero)));
}

} // end main_package
