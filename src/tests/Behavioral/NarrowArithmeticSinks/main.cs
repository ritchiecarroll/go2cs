global using I = object;

namespace go;

using fmt = fmt_package;
using os = os_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

partial struct holder {
    internal any v;
}

// type byteFunc is a methodless func type — rendered inline as its base delegate

internal static int8 a = 100;
internal static uint8 u = 200;
internal static int16 w = 30000;
internal static uint16 z = 60000;
internal static byte c = (byte)'/';
internal static uint8 d = 250;
internal static int8 m8 = -128;
internal static int8 n1 = -1;
internal static int8 h = 101;
internal static nuint n = 1;
internal static uint8 w8 = 100;

internal static @string show(any x) {
    return fmt.Sprintf("%v %T"u8, x, x);
}

internal static any ret(int8 a) {
    return (int8)(a + a);
}

internal static @string variadic(params ꓸꓸꓸany xsʗp) {
    var xs = xsʗp.sslice();

    return fmt.Sprint(xs.ꓸꓸꓸ);
}

internal static @string gen<T>(T x) {
    return fmt.Sprintf("%v %T"u8, (x).OrTypedNilOf(), (x).OrTypedNilOf());
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
    arm("fmt arg", () => fmt.Sprint((int8)(a + a), " ", (uint8)(u + u), " ", (int16)(w + w), " ", (uint16)(z + z)));
    arm("fmt %T", () => fmt.Sprintf("%T %T %T %T"u8, (int8)(a + a), (uint8)(u + u), (int8)(-a - a), ((uint8)(~u))));
    arm("any var", () => {
        any x = (int8)(a + a);
        return show(x);
    });
    arm("any param", () => show((uint8)(u * 2)) + " "u8 + show((uint8)(((uint8)0 - u))));
    arm("any return", () => show(ret(a)));
    arm("variadic any", () => variadic((int8)(a + a), (uint8)(u + u)));
    arm("composite any", () => {
        var s = new any[]{(int8)(a + a), (uint8)(u + u)}.slice();
        var m = new map<@string, any>{["k"] = (int16)(w + w)};
        var hΔ1 = new holder(v: (uint16)(z + z));
        return fmt.Sprint(s, m, hΔ1.v, (any)((int8)(a + a)));
    });
    arm("chan any", () => {
        var ch = new channel<any>(1);
        ch.ᐸꟷ((uint8)(u + u));
        return show(ᐸꟷ(ch));
    });
    arm("type only", () => show((uint8)(u / 3)) + " "u8 + show((int8)(a % 7)) + " "u8 + show((uint8)((u >> 1))) + " "u8 + show((int8)(+a)) + " "u8 + show((int8)(~a)));

    arm("named any", () => {
        I i = (int8)(a + a);
        return show(i);
    });

    arm("generic", () => gen((int8)(a + a)) + " "u8 + gen((uint8)(u + u)));

    arm("widening", () => fmt.Sprint((nint)((uint8)(u + u)), " ", (int64)((int8)(a + a)), " ", (float64)((int8)(a + a)), " ", (nint)((byte)(c - '0')), " ", (uint32)((uint16)(z + z)), " ", (int32)((int16)(w + w))));

    var tbl = new slice<nint>(256);
    foreach (var k in range(tbl)) {
        tbl[k] = k;
    }
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new array<nint>(256){[144] = 7};
    @string str = "0123456789";
    var tblʗ1 = tbl;
    arm("slice index", () => fmt.Sprint(tblʗ1[(uint8)(u + u)]));
    var arrʗ1 = arr;
    arm("array index", () => fmt.Sprint(arrʗ1[(uint8)(u + u)]));
    arm("string index", () => fmt.Sprint(str[(uint8)(d + 10)]));
    var tblʗ2 = tbl;
    arm("slice bound", () => fmt.Sprint(len(tblʗ2.slice((uint8)(u + u))), " ", len(tblʗ2.slice(0, (uint8)(u + u))), " ", str.slice((uint8)(d + 10))));

    arm("shift count", () => {
        var y = (uint32)1;
        y.LshAssign((uint64)((uint8)(d + 10)));
        return fmt.Sprint(((nint)1).Lsh((uint64)((uint8)(u + u - 140))), " ", ((uint32)1).Lsh((uint64)((uint8)(d + 10))), " ", y);
    });

    arm("divrem operand", () => fmt.Sprint((int8)((int8)(a + a) / 2), " ", (int8)((int8)(a + a) % 7), " ", (uint8)((uint8)(u + u) / 3)));
    arm("shr operand", () => fmt.Sprint((int8)(((int8)(a + a) >> 1)), " ", (uint8)(((uint8)(u + u) >> 4))));
    arm("minint8 / -1", () => fmt.Sprint((int8)(m8 / n1), " ", (int8)(m8 % n1)));

    arm("paren compare", () => fmt.Sprint((int8)(a + a) < 0, " ", (uint8)(u + u) == 144));

    arm("switch tag", () => {
        switch ((uint8)(u + u)) {
        case 144: {
            return "case 144";
        }}

        return "default";
    });
    arm("case comparison", () => {
        switch (ᐧ) {
        case {} when (uint8)(u + u) is < 150: {
            return "wrapped";
        }}

        return "unwrapped";
    });
    arm("case value", () => {
        uint8 v = 144;
        switch (v) {
        case var ᴛ when ᴛ == (uint8)(u + u): {
            return "wrapped";
        }}

        return "unwrapped";
    });

    arm("interface case", () => {
        any x = (int8)50;
        any y = (uint8)66;
        @string r = "nomatch";
        switch (x) {
        case var ᴛ when AreEqual(ᴛ, (int8)((h >> 1))): {
            r = "match";
            break;
        }}

        switch (y) {
        case var ᴛ when AreEqual(ᴛ, (uint8)(u / 3)): {
            r += " match"u8;
            break;
        }}

        return r;
    });

    arm("guarded shr", () => {
        int8 x = (int8)(((int8)(a + a)).Rsh(n));
        any y = ((uint8)(u + u)).Rsh(n);
        return fmt.Sprint(x, " ", y, " ", (int8)(((int8)(a + a)).Rsh(n)) == -28, " ", ((int8)(m8 / n1)).Rsh(n), " ", ((int8)(-(a + a))).Rsh(n), " ", ((int8)(a + a)).Rsh((uint64)10));
    });
    arm("guarded shl", () => {
        any y = ((int8)(a + a)).Lsh(n);
        return fmt.Sprint((nint)(((int8)(a + a)).Lsh(n)), " ", y, " ", (int8)((((int8)(a + a)).Lsh(n)) / 3), " ", (nint)(((int8)((h >> 1))).Lsh((n + 1))), " ", ((uint8)(u + w8)).Lsh((uint64)8));
    });

    var tblʗ3 = tbl;
    arm("unary xor plus", () => {
        int8 q = (int8)((int8)(~(a + a)) / 2);
        return fmt.Sprint((nint)((int8)(~(a + a))), " ", q, " ", (int8)(((int8)(~(a + a)) >> 1)), " ", tblʗ3[(uint8)(+(u + u))], " ", (nint)((int8)(+(a * a))));
    });

    var tblʗ4 = tbl;
    arm("make and bounds", () => fmt.Sprint(len(new slice<nint>((uint8)(u + u))), " ", cap(new slice<nint>(0, (uint8)(u + u))), " ", cap(tblʗ4.slice(0, 1, (uint8)(u + u)))));
    arm("divisor", () => {
        var q = d;
        q /= (uint8)(u + u);
        return fmt.Sprint((uint8)(d / (uint8)(u + u)), " ", (uint8)(d % (uint8)(u + u)), " ", q);
    });
    arm("negated minimum", () => fmt.Sprint((nint)((int8)(-m8)), " ", (int64)((int8)(-m8))));
    arm("named func param", () => {

        Func<byte, @string> f = (byte b) => fmt.Sprint(b);
        return f((byte)(u + w8)) + " "u8 + f((byte)((u >> 1)));
    });

    arm("invariant", () => fmt.Sprint((int8)(u + u), " ", (int8)(a + a + a), " ", (uint8)(w + w)));

    arm("builtin arg", () => {
        var b = append(new byte[]{}.slice(), (byte)(u + u), (byte)(u * 2), (byte)(u - 1));
        return fmt.Sprint(min((int8)(a + a), (int8)0), " ", max((uint8)(u + u), (uint8)0), " ", b);
    });

    arm("paren define", () => {
        var t2 = (int8)(a + a);
        return show(t2);
    });
}

} // end main_package
