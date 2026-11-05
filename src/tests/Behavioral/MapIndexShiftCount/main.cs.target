namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;

partial class main_package {

internal static nuint wide = 40;
internal static nuint nine = 9;
internal static nint neg = -1;
internal static int64 big = 1099511627776L;
internal static int64 minus = -8;
internal static nint calls;

internal static nint key() {
    calls++;
    return 1;
}

internal static void arm(@string @class, Func<@string> f) {
    GoFrame ᒐ = default;
    try {
        if (len(os.Args) > 1 && os.Args[1] != @class) {
            return;
        }

        defer(() => {
            var r = recover(); if (r != nil) {
                var (_, isRuntimeError) = r._<runtimeꓸError>(ᐧ);
                fmt.Println(@class + ": panic:", r, isRuntimeError);
            }
        }, ref ᒐ);

        fmt.Println(@class + ":", f());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static T shl<T>(T x, nuint n)
    where T : /* ~uint8 | ~uint32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    x = x.Lsh(n);
    return x;
}

internal static T shrSigned<T>(T x, nint n)
    where T : /* ~int8 | ~int64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return x.Rsh((int64)(n));
}

internal static T shrIndex<T>(map<nint, T> m, nuint n)
    where T : /* ~uint8 | ~uint32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    m[1] = m[1].Rsh(n);
    return m[1];
}

internal static void Main() {
    arm("uint32 const right", () => {
        var m = new map<nint, uint32>{[1] = 0xFFFFFFFFU};
        m[1] = m[1].Rsh((uint64)40);
        return fmt.Sprint(m[1]);
    });
    arm("uint32 const left", () => {
        var m = new map<nint, uint32>{[1] = 0xFF};
        m[1] = m[1].Lsh((uint64)40);
        return fmt.Sprint(m[1]);
    });
    arm("uint32 const in range", () => {
        var m = new map<nint, uint32>{[1] = 0xFF00};
        m[1] >>= 8;
        return fmt.Sprint(m[1]);
    });
    arm("uint32 unbounded right", () => {
        var m = new map<nint, uint32>{[1] = 0xFFFFFFFFU};
        m[1] = m[1].Rsh(wide);
        return fmt.Sprint(m[1]);
    });
    arm("uint8 const left", () => {
        var m = new map<nint, uint8>{[1] = 0xFF};
        m[1] = m[1].Lsh((uint64)33);
        return fmt.Sprint(m[1]);
    });
    arm("uint8 unbounded right", () => {
        var m = new map<nint, uint8>{[1] = 0xFF};
        m[1] = m[1].Rsh(wide);
        return fmt.Sprint(m[1]);
    });
    arm("uint8 unbounded in range", () => {
        var m = new map<nint, uint8>{[1] = 0xFF};
        m[1] >>= 4;
        m[1] = m[1].Lsh(nine - 6);
        return fmt.Sprint(m[1]);
    });
    arm("int64 unbounded right", () => {
        var m = new map<@string, int64>{["a"] = big, ["b"] = minus};
        m["a"] = m["a"].Rsh(wide + 30);
        m["b"] = m["b"].Rsh(wide + 30);
        return fmt.Sprint(m["a"], " ", m["b"]);
    });
    arm("missing key", () => {
        var m = new map<nint, uint32>{};
        m[7] = m[7].Lsh(wide);
        var (v, ok) = m[7, ꟷ];
        return fmt.Sprint(v, " ", ok);
    });
    arm("key evaluated once", () => {
        var m = new map<nint, uint32>{[1] = 0xFFFFFFFFU};
        calls = 0;
        m.RshAssign(key(), wide);
        return fmt.Sprint(m[1], " ", calls);
    });
    arm("negative count", () => {
        var m = new map<nint, uint32>{[1] = 0xFF};
        m[1] = m[1].Lsh((int64)(neg));
        return fmt.Sprint(m[1]);
    });
    arm("type parameter", () => fmt.Sprint(shl((uint8)0xFF, nine), " ", shl((uint32)0xFF, wide), " ", shl((uint8)1, 3)));
    arm("type parameter signed", () => fmt.Sprint(shrSigned((int8)(-8), (nint)nine), " ", shrSigned((int64)big, (nint)wide + 30), " ", shrSigned((int8)64, 3)));
    arm("type parameter negative count", () => fmt.Sprint(shrSigned((int8)1, neg)));
    arm("type parameter map", () => fmt.Sprint(shrIndex(new map<nint, uint8>{[1] = 0xFF}, nine), " ", shrIndex(new map<nint, uint32>{[1] = 0xFFFFFFFFU}, wide)));
}

} // end main_package
