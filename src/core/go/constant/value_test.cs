// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using token = global::go.go.token_package;
using math = math_package;
using big = global::go.math.big_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using global::go.math;
using static global::go.go.constant_package;

partial class constant_internal_test_package {

internal static slice<@string> intTests = [
    // 0-octals
    @"0_123 = 0123",
    @"0123_456 = 0123456",

    // decimals
    @"1_234 = 1234",
    @"1_234_567 = 1234567",

    // hexadecimals
    @"0X_0 = 0",
    @"0X_1234 = 0x1234",
    @"0X_CAFE_f00d = 0xcafef00d",

    // octals
    @"0o0 = 0",
    @"0o1234 = 01234",
    @"0o01234567 = 01234567",

    @"0O0 = 0",
    @"0O1234 = 01234",
    @"0O01234567 = 01234567",

    @"0o_0 = 0",
    @"0o_1234 = 01234",
    @"0o0123_4567 = 01234567",

    @"0O_0 = 0",
    @"0O_1234 = 01234",
    @"0O0123_4567 = 01234567",

    // binaries
    @"0b0 = 0",
    @"0b1011 = 0xb",
    @"0b00101101 = 0x2d",

    @"0B0 = 0",
    @"0B1011 = 0xb",
    @"0B00101101 = 0x2d",

    @"0b_0 = 0",
    @"0b10_11 = 0xb",
    @"0b_0010_1101 = 0x2d"
];

// The RHS operand may be a floating-point quotient n/d of two integer values n and d.
internal static slice<@string> floatTests = [
    // decimal floats
    @"1_2_3. = 123.",
    @"0_123. = 123.",

    @"0_0e0 = 0.",
    @"1_2_3e0 = 123.",
    @"0_123e0 = 123.",

    @"0e-0_0 = 0.",
    @"1_2_3E+0 = 123.",
    @"0123E1_2_3 = 123e123",

    @"0.e+1 = 0.",
    @"123.E-1_0 = 123e-10",
    @"01_23.e123 = 123e123",

    @".0e-1 = .0",
    @".123E+10 = .123e10",
    @".0123E123 = .0123e123",

    @"1_2_3.123 = 123.123",
    @"0123.01_23 = 123.0123",

    @"1e-1000000000 = 0",
    @"1e+1000000000 = ?",
    @"6e5518446744 = ?",
    @"-6e5518446744 = ?",

    // hexadecimal floats
    @"0x0.p+0 = 0.",
    @"0Xdeadcafe.p-10 = 0xdeadcafe/1024",
    @"0x1234.P84 = 0x1234000000000000000000000",

    @"0x.1p-0 = 1/16",
    @"0X.deadcafep4 = 0xdeadcafe/0x10000000",
    @"0x.1234P+12 = 0x1234/0x10",

    @"0x0p0 = 0.",
    @"0Xdeadcafep+1 = 0x1bd5b95fc",
    @"0x1234P-10 = 0x1234/1024",

    @"0x0.0p0 = 0.",
    @"0Xdead.cafep+1 = 0x1bd5b95fc/0x10000",
    @"0x12.34P-10 = 0x1234/0x40000",

    @"0Xdead_cafep+1 = 0xdeadcafep+1",
    @"0x_1234P-10 = 0x1234p-10",

    @"0X_dead_cafe.p-10 = 0xdeadcafe.p-10",
    @"0x12_34.P1_2_3 = 0x1234.p123"
];

internal static slice<@string> imagTests = [
    @"1_234i = 1234i",
    @"1_234_567i = 1234567i",

    @"0.i = 0i",
    @"123.i = 123i",
    @"0123.i = 123i",

    @"0.e+1i = 0i",
    @"123.E-1_0i = 123e-10i",
    @"01_23.e123i = 123e123i",

    @"1e-1000000000i = 0i",
    @"1e+1000000000i = ?",
    @"6e5518446744i = ?",
    @"-6e5518446744i = ?"
];

internal static void testNumbers(ж<testing.T> t, token.Token kind, slice<@string> tests) {
    foreach (var (_, test) in range(tests)) {
        var a = strings.Split(test, " = ");
        if (len(a) != 2) {
            t.Errorf("invalid test case: %s", test);
            continue;
        }

        var x = MakeFromLiteral(a[0], kind, 0);
        global::go.go.constant_package.Value y = default;
        if (a[1] == "?") {
            y = MakeUnknown();
        } else {
            var (ns, ds, ok) = strings.Cut(a[1], "/"); if (ok && kind == token.FLOAT) {
                var n = MakeFromLiteral(ns, token.INT, 0);
                var d = MakeFromLiteral(ds, token.INT, 0);
                y = BinaryOp(n, token.QUO, d);
            } else {
                y = MakeFromLiteral(a[1], kind, 0);
            }
            if (y.Kind() == Unknown) {
                throw panic(fmt.Sprintf("invalid test case: %s %d"u8, test, y.Kind()));
            }
        }

        global::go.go.constant_package.ΔKind xk = x.Kind();
        global::go.go.constant_package.ΔKind yk = y.Kind();
        if (xk != yk) {
            t.Errorf("%s: got kind %d != %d", test, xk, yk);
            continue;
        }

        if (yk == Unknown) {
            continue;
        }

        if (!Compare(x, token.EQL, y)) {
            t.Errorf("%s: %s != %s", test, x, y);
        }
    }
}

// TestNumbers verifies that differently written literals
// representing the same number do have the same value.
public static void TestNumbers(ж<testing.T> t) {
    testNumbers(t, token.INT, intTests);
    testNumbers(t, token.FLOAT, floatTests);
    testNumbers(t, token.IMAG, imagTests);
}

internal static slice<@string> opTests = [
    // unary operations
    @"+ 0 = 0",
    @"+ ? = ?",
    @"- 1 = -1",
    @"- ? = ?",
    @"^ 0 = -1",
    @"^ ? = ?",

    @"! true = false",
    @"! false = true",
    @"! ? = ?",

    // etc.

    // binary operations
    @""""" + """" = """"",
    @"""foo"" + """" = ""foo""",
    @""""" + ""bar"" = ""bar""",
    @"""foo"" + ""bar"" = ""foobar""",

    @"0 + 0 = 0",
    @"0 + 0.1 = 0.1",
    @"0 + 0.1i = 0.1i",
    @"0.1 + 0.9 = 1",
    @"1e100 + 1e100 = 2e100",
    @"? + 0 = ?",
    @"0 + ? = ?",

    @"0 - 0 = 0",
    @"0 - 0.1 = -0.1",
    @"0 - 0.1i = -0.1i",
    @"1e100 - 1e100 = 0",
    @"? - 0 = ?",
    @"0 - ? = ?",

    @"0 * 0 = 0",
    @"1 * 0.1 = 0.1",
    @"1 * 0.1i = 0.1i",
    @"1i * 1i = -1",
    @"? * 0 = ?",
    @"0 * ? = ?",
    @"0 * 1e+1000000000 = ?",

    @"0 / 0 = ""division_by_zero""",
    @"10 / 2 = 5",
    @"5 / 3 = 5/3",
    @"5i / 3i = 5/3",
    @"? / 0 = ?",
    @"0 / ? = ?",
    @"0 * 1e+1000000000i = ?",

    @"0 % 0 = ""runtime_error:_integer_divide_by_zero""", // TODO(gri) should be the same as for /
    @"10 % 3 = 1",
    @"? % 0 = ?",
    @"0 % ? = ?",

    @"0 & 0 = 0",
    @"12345 & 0 = 0",
    @"0xff & 0xf = 0xf",
    @"? & 0 = ?",
    @"0 & ? = ?",

    @"0 | 0 = 0",
    @"12345 | 0 = 12345",
    @"0xb | 0xa0 = 0xab",
    @"? | 0 = ?",
    @"0 | ? = ?",

    @"0 ^ 0 = 0",
    @"1 ^ -1 = -2",
    @"? ^ 0 = ?",
    @"0 ^ ? = ?",

    @"0 &^ 0 = 0",
    @"0xf &^ 1 = 0xe",
    @"1 &^ 0xf = 0",
    // etc.

    // shifts
    @"0 << 0 = 0",
    @"1 << 10 = 1024",
    @"0 >> 0 = 0",
    @"1024 >> 10 == 1",
    @"? << 0 == ?",
    @"? >> 10 == ?",
    // etc.

    // comparisons
    @"false == false = true",
    @"false == true = false",
    @"true == false = false",
    @"true == true = true",

    @"false != false = false",
    @"false != true = true",
    @"true != false = true",
    @"true != true = false",

    @"""foo"" == ""bar"" = false",
    @"""foo"" != ""bar"" = true",
    @"""foo"" < ""bar"" = false",
    @"""foo"" <= ""bar"" = false",
    @"""foo"" > ""bar"" = true",
    @"""foo"" >= ""bar"" = true",

    @"0 == 0 = true",
    @"0 != 0 = false",
    @"0 < 10 = true",
    @"10 <= 10 = true",
    @"0 > 10 = false",
    @"10 >= 10 = true",

    @"1/123456789 == 1/123456789 == true",
    @"1/123456789 != 1/123456789 == false",
    @"1/123456789 < 1/123456788 == true",
    @"1/123456788 <= 1/123456789 == false",
    @"0.11 > 0.11 = false",
    @"0.11 >= 0.11 = true",

    @"? == 0 = false",
    @"? != 0 = false",
    @"? < 10 = false",
    @"? <= 10 = false",
    @"? > 10 = false",
    @"? >= 10 = false",

    @"0 == ? = false",
    @"0 != ? = false",
    @"0 < ? = false",
    @"10 <= ? = false",
    @"0 > ? = false",
    @"10 >= ? = false"

    // etc.
];

public static void TestOps(ж<testing.T> t) {
    foreach (var (_, test) in range(opTests)) {
        var a = strings.Split(test, " ");
        nint i = 0; // operator index

        global::go.go.constant_package.Value x = default;
        global::go.go.constant_package.Value x0 = default;
        switch (len(a)) {
        case 4: {
            // unary operation
            break;
        }
        case 5: {
            // binary operation
            (x, x0) = (val(a[0]), val(a[0]));
            i = 1;
            break;
        }
        default: {
            t.Errorf("invalid test case: %s", test);
            continue;
            break;
        }}

        var (op, ok) = optab[a[i], ꟷ];
        if (!ok) {
            throw panic("missing optab entry for " + a[i]);
        }

        var (y, y0) = (val(a[i + 1]), val(a[i + 1]));

        var got = doOp(x, op, y);
        var want = val(a[i + 3]);
        if (!eql(got, want)) {
            t.Errorf("%s: got %s; want %s", test, got, want);
            continue;
        }

        if (x0 != nil && !eql(x, x0)) {
            t.Errorf("%s: x changed to %s", test, x);
            continue;
        }

        if (!eql(y, y0)) {
            t.Errorf("%s: y changed to %s", test, y);
            continue;
        }
    }
}

internal static bool eql(global::go.go.constant_package.Value x, global::go.go.constant_package.Value y) {
    var (_, ux) = x._<unknownVal>(ᐧ);
    var (_, uy) = y._<unknownVal>(ᐧ);
    if (ux || uy) {
        return ux == uy;
    }
    return Compare(x, token.EQL, y);
}

// ----------------------------------------------------------------------------
// String tests
internal static @string xxx = strings.Repeat("x", 68);
internal static @string issue14262 = @"""بموجب الشروط التالية نسب المصنف — يجب عليك أن تنسب العمل بالطريقة التي تحددها المؤلف أو المرخص (ولكن ليس بأي حال من الأحوال أن توحي وتقترح بتحول أو استخدامك للعمل).  المشاركة على قدم المساواة — إذا كنت يعدل ، والتغيير ، أو الاستفادة من هذا العمل ، قد ينتج عن توزيع العمل إلا في ظل تشابه او تطابق فى واحد لهذا الترخيص.""";

partial struct stringTestsᴛ1 /*dyn*/ {
    internal @string input, @short, exact;
}
internal static slice<stringTestsᴛ1> stringTests = [
    // Unknown
    new("", "unknown", "unknown"),
    new("0x", "unknown", "unknown"),
    new("'", "unknown", "unknown"),
    new("1f0", "unknown", "unknown"),
    new("unknown", "unknown", "unknown"),

    // Bool
    new("true", "true", "true"),
    new("false", "false", "false"),

    // String
    new(@"""""", @"""""", @""""""),
    new(@"""foo""", @"""foo""", @"""foo"""),
    new(@""""u8 + xxx + @"xx"""u8, @""""u8 + xxx + @"xx"""u8, @""""u8 + xxx + @"xx"""u8),
    new(@""""u8 + xxx + @"xxx"""u8, @""""u8 + xxx + @"..."u8, @""""u8 + xxx + @"xxx"""u8),
    new(@""""u8 + xxx + xxx + @"xxx"""u8, @""""u8 + xxx + @"..."u8, @""""u8 + xxx + xxx + @"xxx"""u8),
    new(issue14262, @"""بموجب الشروط التالية نسب المصنف — يجب عليك أن تنسب العمل بالطريقة ال...", issue14262),

    // Int
    new("0", "0", "0"),
    new("-1", "-1", "-1"),
    new("12345", "12345", "12345"),
    new("-12345678901234567890", "-12345678901234567890", "-12345678901234567890"),
    new("12345678901234567890", "12345678901234567890", "12345678901234567890"),

    // Float
    new("0.", "0", "0"),
    new("-0.0", "0", "0"),
    new("10.0", "10", "10"),
    new("2.1", "2.1", "21/10"),
    new("-2.1", "-2.1", "-21/10"),
    new("1e9999", "1e+9999", "0x.f8d4a9da224650a8cb2959e10d985ad92adbd44c62917e608b1f24c0e1b76b6f61edffeb15c135a4b601637315f7662f325f82325422b244286a07663c9415d2p+33216"),
    new("1e-9999", "1e-9999", "0x.83b01ba6d8c0425eec1b21e96f7742d63c2653ed0a024cf8a2f9686df578d7b07d7a83d84df6a2ec70a921d1f6cd5574893a7eda4d28ee719e13a5dce2700759p-33215"),
    new("2.71828182845904523536028747135266249775724709369995957496696763", "2.71828", "271828182845904523536028747135266249775724709369995957496696763/100000000000000000000000000000000000000000000000000000000000000"),
    new("0e9999999999", "0", "0"),   // issue #16176
    new("-6e-1886451601", "0", "0"), // issue #20228

    // Complex
    new("0i", "(0 + 0i)", "(0 + 0i)"),
    new("-0i", "(0 + 0i)", "(0 + 0i)"),
    new("10i", "(0 + 10i)", "(0 + 10i)"),
    new("-10i", "(0 + -10i)", "(0 + -10i)"),
    new("1e9999i", "(0 + 1e+9999i)", "(0 + 0x.f8d4a9da224650a8cb2959e10d985ad92adbd44c62917e608b1f24c0e1b76b6f61edffeb15c135a4b601637315f7662f325f82325422b244286a07663c9415d2p+33216i)")
];

public static void TestString(ж<testing.T> t) {
    foreach (var (_, test) in range(stringTests)) {
        var x = val(test.input);
        {
            @string got = x.String(); if (got != test.@short) {
                t.Errorf("%s: got %q; want %q as short string", test.input, got, test.@short);
            }
        }
        {
            @string got = x.ExactString(); if (got != test.exact) {
                t.Errorf("%s: got %q; want %q as exact string", test.input, got, test.exact);
            }
        }
    }
}

// ----------------------------------------------------------------------------
// Support functions
internal static global::go.go.constant_package.Value val(@string lit) {
    if (len(lit) == 0) {
        return MakeUnknown();
    }

    var exprᴛ1 = lit;
    switch (ᐧ) {
    case {} when exprᴛ1 == "?"u8: {
        return MakeUnknown();
    }
    case {} when exprᴛ1 == "true"u8: {
        return MakeBool(true);
    }
    case {} when exprᴛ1 == "false"u8: {
        return MakeBool(false);
    }}

    var (@as, bs, ok) = strings.Cut(lit, "/"); if (ok) {
        // assume fraction
        var a = MakeFromLiteral(@as, token.INT, 0);
        var b = MakeFromLiteral(bs, token.INT, 0);
        return BinaryOp(a, token.QUO, b);
    }

    token.Token tok = token.INT;
    var (first, last) = (lit[0], lit[len(lit) - 1]);
    switch (ᐧ) {
    case {} when first == (byte)'"' || first == (byte)'`': {
        tok = token.STRING;
        lit = strings.ReplaceAll(lit, "_", " ");
        break;
    }
    case {} when first is (byte)'\'': {
        tok = token.CHAR;
        break;
    }
    case {} when last is (byte)'i': {
        tok = token.IMAG;
        break;
    }
    default: {
        if (!strings.HasPrefix(lit, "0x") && strings.ContainsAny(lit, "./Ee")) {
            tok = token.FLOAT;
        }
        break;
    }}

    return MakeFromLiteral(lit, tok, 0);
}

internal static map<@string, token.Token> optab = new map<@string, token.Token>{
    ["!"] = token.NOT,

    ["+"] = token.ADD,
    ["-"] = token.SUB,
    ["*"] = token.MUL,
    ["/"] = token.QUO,
    ["%"] = token.REM,

    ["<<"] = token.SHL,
    [">>"] = token.SHR,

    ["&"] = token.AND,
    ["|"] = token.OR,
    ["^"] = token.XOR,
    ["&^"] = token.AND_NOT,

    ["=="] = token.EQL,
    ["!="] = token.NEQ,
    ["<"] = token.LSS,
    ["<="] = token.LEQ,
    [">"] = token.GTR,
    [">="] = token.GEQ
};

internal static void panicHandler(ж<global::go.go.constant_package.Value> Ꮡv) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
        ref var v = ref Ꮡv.DerefOrNull();

        var switchᴛ1 = recover(calledByDefer);
        switch (switchᴛ1.type()) {
        case null: {
            // nothing to do
            break;
        }
        case @string p: {
            v = MakeString(p);
            break;
        }
        case {} Δp when Δp._<error>(out var p): {
            v = MakeString(p.Error());
            break;
        }
        default: {
            var p = switchᴛ1;
            throw panic(p);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static global::go.go.constant_package.Value /*z*/ doOp(global::go.go.constant_package.Value x, token.Token op, global::go.go.constant_package.Value y) {
    heap<global::go.go.constant_package.Value>(out var Ꮡz);
    GoFrame ᒐ = default;
    try {
        ref var z = ref Ꮡz.ValueSlot;

        defer(panicHandler, Ꮡz, ref ᒐ, recovers: true);

        if (x == nil) {
            z = UnaryOp(op, y, 0); goto ᒐdone;
        }

        switch (op) {
        case var ᴛ when ᴛ == token.EQL || ᴛ == token.NEQ || ᴛ == token.LSS || ᴛ == token.LEQ || ᴛ == token.GTR || ᴛ == token.GEQ: {
            z = MakeBool(Compare(x, op, y)); goto ᒐdone;
        }
        case var ᴛ when ᴛ == token.SHL || ᴛ == token.SHR: {
            var (s, _) = Int64Val(y);
            z = Shift(x, op, (nuint)s); goto ᒐdone;
        }
        default: {
            z = BinaryOp(x, op, y); goto ᒐdone;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡz.ValueSlot;
}

// ----------------------------------------------------------------------------
// Other tests
internal static slice<@string> fracTests = [
    "0",
    "1",
    "-1",
    "1.2",
    "-0.991",
    "2.718281828",
    "3.14159265358979323e-10",
    "1e100",
    "1e1000"
];

public static void TestFractions(ж<testing.T> t) {
    foreach (var (_, test) in range(fracTests)) {
        var x = val(test);
        // We don't check the actual numerator and denominator because they
        // are unlikely to be 100% correct due to floatVal rounding errors.
        // Instead, we compute the fraction again and compare the rounded
        // result.
        var q = BinaryOp(Num(x), token.QUO, Denom(x));
        @string got = q.String();
        @string want = x.String();
        if (got != want) {
            t.Errorf("%s: got quotient %s, want %s", x, got, want);
        }
    }
}

internal static slice<@string> bytesTests = [
    "0",
    "1",
    "123456789",
    "123456789012345678901234567890123456789012345678901234567890"
];

public static void TestBytes(ж<testing.T> t) {
    foreach (var (_, test) in range(bytesTests)) {
        var x = val(test);
        var bytes = Bytes(x);

        // special case 0
        if (Sign(x) == 0 && len(bytes) != 0) {
            t.Errorf("%s: got %v; want empty byte slice", test, bytes);
        }

        nint n = len(bytes); if (n > 0 && bytes[n - 1] == 0) {
            t.Errorf("%s: got %v; want no leading 0 byte", test, bytes);
        }

        var got = MakeFromBytes(bytes); if (!eql(got, x)) {
            t.Errorf("%s: got %s; want %s (bytes = %v)", test, got, x, bytes);
        }
    }
}

public static void TestUnknown(ж<testing.T> t) {
    var u = MakeUnknown();
// token.ADD ok below, operation is never considered
    slice<global::go.go.constant_package.Value> values = slice<global::go.go.constant_package.Value>([u, MakeBool(false), MakeString(""u8), MakeInt64(1), MakeFromLiteral("''", token.CHAR, 0), MakeFromLiteral("-1234567890123456789012345678901234567890", token.INT, 0), MakeFloat64(1.2D), MakeImag(MakeFloat64(1.2D))
    ]);
    foreach (var (_, val) in range(values)) {
        var (x, y) = (val, u);
        foreach (var i in range(len(new nint[]{}.array(2)))) {
            if (i == 1) {
                (x, y) = (y, x);
            }
            {
                var got = BinaryOp(x, token.ADD, y); if (got.Kind() != Unknown) {
                    t.Errorf("%s + %s: got %s; want %s", x, y, got, u);
                }
            }
            {
                var got = Compare(x, token.EQL, y); if (got) {
                    t.Errorf("%s == %s: got true; want false", x, y);
                }
            }
            {
                var got = Compare(x, token.NEQ, y); if (got) {
                    t.Errorf("%s != %s: got true; want false", x, y);
                }
            }
        }
    }
}

public static void TestMakeFloat64(ж<testing.T> t) {
    float64 zero = default;
    foreach (var (_, arg) in range(slice<float64>([
        -math.MaxFloat32,
        -10D,
        -0.5D,
        -zero,
        zero,
        1D,
        10D,
        123456789.87654321e-23D,
        1e10D,
        math.MaxFloat64
    ]))) {
        var val = MakeFloat64(arg);
        if (val.Kind() != Float) {
            t.Errorf("%v: got kind = %d; want %d", arg, val.Kind(), Float);
        }

        // -0.0 is mapped to 0.0
        var (got, exact) = Float64Val(val);
        if (!exact || math.Float64bits(got) != math.Float64bits(arg + 0D)) {
            t.Errorf("%v: got %v (exact = %v)", arg, got, exact);
        }
    }

    // infinity
    foreach (var sign in range(len(slice<nint>([-1, 1])))) {
        var arg = math.Inf(sign);
        var val = MakeFloat64(arg);
        if (val.Kind() != Unknown) {
            t.Errorf("%v: got kind = %d; want %d", arg, val.Kind(), Unknown);
        }
    }
}

internal partial struct makeTestCase {
    internal global::go.go.constant_package.ΔKind kind;
    internal any arg, want;
}

internal static makeTestCase dup(global::go.go.constant_package.ΔKind k, any x) { return new makeTestCase(k, x, x); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloˢ = (@string)"hello"u8;

public static void TestMake(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<makeTestCase>([
        new(Bool, false, false),
        new(ΔString, helloˢ, helloˢ),

        new(Int, (int64)1, (int64)1),
        new(Int, big.NewInt(10).OrTypedNil(), (int64)10),
        new(Int, @new<bigꓸInt>().Lsh(big.NewInt(1), 62).OrTypedNil(), (int64)(4611686018427387904L)),
        dup(Int, @new<bigꓸInt>().Lsh(big.NewInt(1), 63).OrTypedNil()),

        new(Float, big.NewFloat(0D).OrTypedNil(), floatVal0.val.OrTypedNil()),
        dup(Float, big.NewFloat(2.0D).OrTypedNil()),
        dup(Float, big.NewRat(1, 3).OrTypedNil())
    ]))) {
        var val = Make(test.arg);
        var got = Val(val);
        if (val.Kind() != test.kind || !AreEqual(got, test.want)) {
            t.Errorf("got %v (%T, kind = %d); want %v (%T, kind = %d)",
                got, got, val.Kind(), test.want, test.want, test.kind);
        }
    }
}

public static void BenchmarkStringAdd(ж<testing.B> b) {
    for (nint sizeᴛ1 = 1; sizeᴛ1 <= 65536; sizeᴛ1 *= 4) {
        var size = sizeᴛ1;
        b.Run(fmt.Sprint(size), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            var n = (int64)0;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var x = MakeString(strings.Repeat("x", 100));
                var y = x;
                for (nint j = 0; j < size - 1; j++) {
                    y = BinaryOp(y, token.ADD, x);
                }
                n += (int64)len(StringVal(y));
            }
            if (n != (int64)(~bΔ1).N * (int64)size * 100) {
                bΔ1.Fatalf("bad string %d != %d", n, (int64)(~bΔ1).N * (int64)size * 100);
            }
        });
    }
}

partial struct bitLenTestsᴛ1 /*dyn*/ {
    internal int64 val;
    internal nint want;
}
internal static slice<bitLenTestsᴛ1> bitLenTests = [
    new(0, 0),
    new(1, 1),
    new(-16, 5),
    new(2305843009213693952L, 62),
    new(4611686018427387904L, 63),
    new(-4611686018427387904L, 63),
    new(-9223372036854775808L, 64)
];

public static void TestBitLen(ж<testing.T> t) {
    foreach (var (_, test) in range(bitLenTests)) {
        nint got = BitLen(MakeInt64(test.val)); if (got != test.want) {
            t.Errorf("%v: got %v, want %v", test.val, got, test.want);
        }
    }
}

} // end constant_internal_test_package
