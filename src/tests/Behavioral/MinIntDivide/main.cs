namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using os = os_package;

partial class main_package {

[GoType("num:int64")] partial struct word;

[GoType("num:int32")] partial struct small;

internal static nint i = Δmath.MinInt;
internal static int32 i32 = Δmath.MinInt32;
internal static ж<int64> Ꮡi64 = new StandardBox<int64>(Δmath.MinInt64);
internal static ref int64 i64 => ref Ꮡi64.Value;
internal static rune r = Δmath.MinInt32;
internal static word w = Δmath.MinInt64;
internal static small s = Δmath.MinInt32;
internal static int8 i8 = Δmath.MinInt8;
internal static int16 i16 = Δmath.MinInt16;
internal static nint m1 = -1;
internal static nint zero = 0;
internal static nint calls = 0;

[GoType] partial struct cell {
    internal int64 x;
    internal nint n;
}

[GoType] partial struct holder {
    internal partial ref ж<cell> cell { get; }
}

internal static @string String(this holder h) {
    return fmt.Sprint(h.x);
}

internal static nint counted(nint i) {
    calls++;
    return i;
}

internal static @string key() {
    calls++;
    return "k"u8;
}

internal static ж<cell> fresh() {
    calls++;
    return Ꮡ(new cell(x: Δmath.MinInt64));
}

internal static slice<nint> ints() {
    calls++;
    return new nint[]{Δmath.MinInt, 7}.slice();
}

internal static T div<T>(T a, T b)
    where T : /* ~int64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return quo(a, b);
}

internal static T mod<T>(T a, T b)
    where T : /* ~int8 | ~int16 | ~int32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return rem(a, b);
}

internal static T quoAny<T>(T a, T b)
    where T : /* ~int | ~int64 | ~uint8 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return quo(a, b);
}

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

internal static int64 localConst(int64 a, int64 b) {
    const int64 quo = 3;
    return builtin.quo(a, b) + quo;
}

internal static int32 laterConst(int32 a, int32 b) {
    var r = builtin.rem(a, b);
    const int32 rem = 10;
    return r + rem;
}

internal static nint typeSwitch(any a, nint b) {
    switch (a.type()) {
    case nint quo: {
        return builtin.quo(quo, b);
    }
    case int32 quo: {
        return builtin.quo(quo, b);
    }}
    return 0;
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
private static readonly @string compoundPointerˢ = "compound pointer"u8;
private static readonly @string compoundIndexExpressionˢ = "compound index expression"u8;
private static readonly @string compoundCallIndexˢ = "compound call index"u8;
private static readonly object callsˢ = (@string)" calls "u8;
private static readonly @string compoundMapKeyˢ = "compound map key"u8;
private static readonly @string compoundCallSelectorˢ = "compound call selector"u8;
private static readonly @string compoundPromotedˢ = "compound promoted"u8;
private static readonly @string genericˢ = "generic"u8;
private static readonly @string constant1RemainderOfANilˢ = "constant -1 remainder of a nil selector"u8;
private static readonly @string constant1RemainderOfAˢ = "constant -1 remainder of a promoted nil selector"u8;
private static readonly @string constant1OfAnEffectfulˢ = "constant -1 of an effectful dividend"u8;
private static readonly @string constant1RemainderOutOfˢ = "constant -1 remainder out of range"u8;
private static readonly @string compoundConstant1ˢ = "compound constant -1"u8;
private static readonly @string constantDividendˢ = "constant dividend"u8;
private static readonly @string localNamedRemˢ = "local named rem"u8;
private static readonly @string localConstantOrTypeˢ = "local constant or type switch named quo"u8;
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
        q *= -1;
        m = 0;
        return fmt.Sprint(unchecked(-i) == Δmath.MinInt, (@string)" "u8, (nint)0, (@string)" "u8, unchecked(-i64), (@string)" "u8, (int64)0, (@string)" "u8, q, (@string)" "u8, m);
    });
    arm(ordinaryˢ, () => fmt.Sprint(i64 / (int64)7, (@string)" "u8, i64 % (int64)7, (@string)" "u8, -7 / m1, (@string)" "u8, 7 % (m1 - 2)));
    arm(compoundPointerˢ, () => {
        var p = Ꮡi64;
        ref var v = ref heap<int64>(out var Ꮡv);
        v = p.Value;
        p = Ꮡv;
        p.Value.QuoAssign((int64)m1);
        return fmt.Sprint(v);
    });
    arm(compoundIndexExpressionˢ, () => {
        var sl = new int32[]{0, Δmath.MinInt32, Δmath.MinInt32}.slice();
        nint j = 0;
        sl[j + 1].QuoAssign((int32)m1);
        sl[len(sl) - 1].RemAssign((int32)m1);
        return fmt.Sprint(sl[1], (@string)" "u8, sl[2]);
    });
    arm(compoundCallIndexˢ, () => {
        calls = 0;
        var sl = new nint[]{0, Δmath.MinInt}.slice();
        sl[counted(1)].QuoAssign(m1);
        return fmt.Sprint(sl[1], callsˢ, calls);
    });
    arm(compoundMapKeyˢ, () => {
        calls = 0;
        var m = new map<@string, int64>{["k"u8] = Δmath.MinInt64};
        m.QuoAssign(key(), (int64)m1);
        m.RemAssign(key(), (int64)m1);
        return fmt.Sprint(m["k"u8], callsˢ, calls);
    });
    arm(compoundCallSelectorˢ, () => {
        calls = 0;
        ref var c = ref heap<ж<cell>>(out var Ꮡc);
        c = fresh();
        var p = Ꮡc;
        (p.ValueSlot).Value.x.RemAssign((int64)m1);
        fresh().Value.x.QuoAssign((int64)m1);
        return fmt.Sprint((~c).x, callsˢ, calls);
    });
    arm(compoundPromotedˢ, () => {
        var h = new holder(Ꮡ(new cell(x: Δmath.MinInt64)));
        h.x = quo(h.x, (int64)m1);
        return fmt.Sprint(h);
    });
    arm(genericˢ, () => fmt.Sprint(div(i64, (int64)m1), (@string)" "u8, div(w, ((word)(int64)m1)), (@string)" "u8, mod(i32, (int32)m1), (@string)" "u8, mod(i8, (int8)m1),
            (@string)" "u8, quoAny(i, m1), (@string)" "u8, quoAny((uint8)200, (uint8)3)));
    arm(constant1RemainderOfANilˢ, () => {
        ж<cell> c = default!;
        var rΔ1 = (~c).x * 0;
        return fmt.Sprint(rΔ1);
    });
    arm(constant1RemainderOfAˢ, () => {
        holder h = new(nil);
        return fmt.Sprint(h.n * 0 == 0);
    });
    arm(constant1OfAnEffectfulˢ, () => {
        calls = 0;
        var sl = new nint[]{Δmath.MinInt}.slice();
        nint rΔ2 = ints()[counted(0)] * 0;
        nint q = unchecked(-ints()[counted(0)]);
        return fmt.Sprint(rΔ2, (@string)" "u8, q == Δmath.MinInt, (@string)" "u8, sl[0] * 0, callsˢ, calls);
    });
    arm(constant1RemainderOutOfˢ, () => {
        var sl = new nint[]{1}.slice();
        nint j = 3;
        return fmt.Sprint(sl[j] * 0);
    });
    arm(compoundConstant1ˢ, () => {
        var sl = new int64[]{Δmath.MinInt64, 5}.slice();
        nint j = 0;
        sl[j + 0] *= -1;
        sl[j + 1] = 0;
        return fmt.Sprint(sl[0], (@string)" "u8, sl[1]);
    });
    arm(constantDividendˢ, () => {
        const int64 big = /* math.MaxInt64 */ 9223372036854775807;
        int64 d64 = Δmath.MinInt64;
        var b = slice<byte>("abc"u8);
        return fmt.Sprint(5000 / (int64)m1, (@string)" "u8, len(b) % m1, (@string)" "u8, cap(b) / m1, (@string)" "u8, big / (int64)m1, (@string)" "u8, quo((int64)Δmath.MinInt64, (int64)m1), (@string)" "u8,
            quo(d64, (int64)m1));
    });
    arm(localNamedRemˢ, localRem);
    arm(localConstantOrTypeˢ, () => fmt.Sprint(localConst(i64, (int64)m1), (@string)" "u8, laterConst(i32, (int32)m1), (@string)" "u8, typeSwitch(i, m1)));
    arm(divideByZeroˢ, () => fmt.Sprint(quo(i64, (int64)zero)));
    arm(remainderByZeroˢ, () => fmt.Sprint(rem(i, zero)));
}

} // end main_package
