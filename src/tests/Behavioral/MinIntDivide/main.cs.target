namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using os = os_package;

partial class main_package {

partial struct word /*num:int64*/;

partial struct small /*num:int32*/;

internal static nint i = Δmath.MinInt;
internal static int32 i32 = Δmath.MinInt32;
internal static readonly ж<int64> Ꮡi64 = new StaticBox<int64>(Δmath.MinInt64);
internal static ref int64 i64 => ref ((StaticBox<int64>)Ꮡi64).ValueSlot;
internal static rune r = Δmath.MinInt32;
internal static word w = Δmath.MinInt64;
internal static small s = Δmath.MinInt32;
internal static int8 i8 = Δmath.MinInt8;
internal static int16 i16 = Δmath.MinInt16;
internal static nint m1 = -1;
internal static nint zero = 0;
internal static nint calls = 0;

partial struct cell {
    internal int64 x;
    internal nint n;
}

partial struct holder {
    /*embed*/ internal ж<cell> cell;
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
    return "k";
}

internal static ж<cell> fresh() {
    calls++;
    return Ꮡ(new cell(x: Δmath.MinInt64));
}

internal static slice<nint> ints() {
    calls++;
    return slice<nint>([Δmath.MinInt, 7]);
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

internal static @string localRem() {
    {
        var rem = builtin.rem(i64, (int64)m1); if (rem == 0) {
            return fmt.Sprint(rem);
        }
    }

    return "nonzero";
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
                var r = recover(); if (r != nil) {
                    fmt.Println(@class + ": panic:", r);
                }
            }
        }, ref ᒐ);

        fmt.Println(@class + ":", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void Main() {
    arm("int", () => fmt.Sprint(quo(i, m1) == Δmath.MinInt, " ", rem(i, m1)));
    arm("int32", () => fmt.Sprint(quo(i32, (int32)m1), " ", rem(i32, (int32)m1)));
    arm("int64", () => fmt.Sprint(quo(i64, (int64)m1), " ", rem(i64, (int64)m1)));
    arm("rune", () => fmt.Sprint(quo(r, (rune)m1), " ", rem(r, (rune)m1)));
    arm("named int64", () => fmt.Sprint(w / ((word)(int64)m1), " ", w % ((word)(int64)m1)));
    arm("named int32", () => fmt.Sprint(s / ((small)(int32)m1), " ", s % ((small)(int32)m1)));
    arm("narrow", () => fmt.Sprint((int8)(i8 / (int8)m1), " ", (int8)(i8 % (int8)m1), " ", (int16)(i16 / (int16)m1), " ", (int16)(i16 % (int16)m1)));
    arm("compound", () => {
        var (q, m) = (i64, i64);
        q = quo(q, (int64)m1);
        m = rem(m, (int64)m1);
        return fmt.Sprint(q, " ", m);
    });
    arm("constant -1", () => {
        var (q, m) = (i32, i32);
        q *= -1;
        m = 0;
        return fmt.Sprint(unchecked(-i) == Δmath.MinInt, " ", (nint)0, " ", unchecked(-i64), " ", (int64)0, " ", q, " ", m);
    });
    arm("ordinary", () => fmt.Sprint(i64 / (int64)7, " ", i64 % (int64)7, " ", -7 / m1, " ", 7 % (m1 - 2)));
    arm("compound pointer", () => {
        var p = Ꮡi64;
        ref var v = ref heap<int64>(p.Value, out var Ꮡv);
        p = Ꮡv;
        p.Value.QuoAssign((int64)m1);
        return fmt.Sprint(v);
    });
    arm("compound index expression", () => {
        var sl = slice<int32>([0, Δmath.MinInt32, Δmath.MinInt32]);
        nint j = 0;
        sl[j + 1].QuoAssign((int32)m1);
        sl[len(sl) - 1].RemAssign((int32)m1);
        return fmt.Sprint(sl[1], " ", sl[2]);
    });
    arm("compound call index", () => {
        calls = 0;
        var sl = slice<nint>([0, Δmath.MinInt]);
        sl[counted(1)].QuoAssign(m1);
        return fmt.Sprint(sl[1], " calls ", calls);
    });
    arm("compound map key", () => {
        calls = 0;
        var m = new map<@string, int64>{["k"] = Δmath.MinInt64};
        m.QuoAssign(key(), (int64)m1);
        m.RemAssign(key(), (int64)m1);
        return fmt.Sprint(m["k"], " calls ", calls);
    });
    arm("compound call selector", () => {
        calls = 0;
        ref var c = ref heap<ж<cell>>(fresh(), out var Ꮡc);
        var p = Ꮡc;
        (p.ValueSlot).x.RemAssign((int64)m1);
        fresh().x.QuoAssign((int64)m1);
        return fmt.Sprint(c.x, " calls ", calls);
    });
    arm("compound promoted", () => {
        var h = new holder(Ꮡ(new cell(x: Δmath.MinInt64)));
        h.x = quo(h.x, (int64)m1);
        return fmt.Sprint(h);
    });
    arm("generic", () => fmt.Sprint(div(i64, (int64)m1), " ", div(w, (word)(int64)m1), " ", mod(i32, (int32)m1), " ", mod(i8, (int8)m1),
            " ", quoAny(i, m1), " ", quoAny((uint8)200, (uint8)3)));
    arm("constant -1 remainder of a nil selector", () => {
        ж<cell> c = default;
        var rΔ1 = c.x * 0;
        return fmt.Sprint(rΔ1);
    });
    arm("constant -1 remainder of a promoted nil selector", () => {
        holder h = new(nil);
        return fmt.Sprint(h.n * 0 == 0);
    });
    arm("constant -1 of an effectful dividend", () => {
        calls = 0;
        var sl = slice<nint>([Δmath.MinInt]);
        nint rΔ2 = ints()[counted(0)] * 0;
        nint q = unchecked(-ints()[counted(0)]);
        return fmt.Sprint(rΔ2, " ", q == Δmath.MinInt, " ", sl[0] * 0, " calls ", calls);
    });
    arm("constant -1 remainder out of range", () => {
        var sl = slice<nint>([1]);
        nint j = 3;
        return fmt.Sprint(sl[j] * 0);
    });
    arm("compound constant -1", () => {
        var sl = slice<int64>([Δmath.MinInt64, 5]);
        nint j = 0;
        sl[j + 0] *= -1;
        sl[j + 1] = 0;
        return fmt.Sprint(sl[0], " ", sl[1]);
    });
    arm("constant dividend", () => {
        const int64 big = /* math.MaxInt64 */ 9223372036854775807;
        int64 d64 = Δmath.MinInt64;
        var b = slice<byte>("abc"u8);
        return fmt.Sprint(5000 / (int64)m1, " ", len(b) % m1, " ", cap(b) / m1, " ", big / (int64)m1, " ", quo((int64)Δmath.MinInt64, (int64)m1), " ",
            quo(d64, (int64)m1));
    });
    arm("local named rem", localRem);
    arm("local constant or type switch named quo", () => fmt.Sprint(localConst(i64, (int64)m1), " ", laterConst(i32, (int32)m1), " ", typeSwitch(i, m1)));
    arm("divide by zero", () => fmt.Sprint(quo(i64, (int64)zero)));
    arm("remainder by zero", () => fmt.Sprint(rem(i, zero)));
}

} // end main_package
