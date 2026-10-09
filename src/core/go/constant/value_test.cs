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

// 0-octals
// decimals
// hexadecimals
// octals
// binaries
internal static slice<@string> intTests = new @string[]{
    @"0_123 = 0123"u8,
    @"0123_456 = 0123456"u8,
    @"1_234 = 1234"u8,
    @"1_234_567 = 1234567"u8,
    @"0X_0 = 0"u8,
    @"0X_1234 = 0x1234"u8,
    @"0X_CAFE_f00d = 0xcafef00d"u8,
    @"0o0 = 0"u8,
    @"0o1234 = 01234"u8,
    @"0o01234567 = 01234567"u8,
    @"0O0 = 0"u8,
    @"0O1234 = 01234"u8,
    @"0O01234567 = 01234567"u8,
    @"0o_0 = 0"u8,
    @"0o_1234 = 01234"u8,
    @"0o0123_4567 = 01234567"u8,
    @"0O_0 = 0"u8,
    @"0O_1234 = 01234"u8,
    @"0O0123_4567 = 01234567"u8,
    @"0b0 = 0"u8,
    @"0b1011 = 0xb"u8,
    @"0b00101101 = 0x2d"u8,
    @"0B0 = 0"u8,
    @"0B1011 = 0xb"u8,
    @"0B00101101 = 0x2d"u8,
    @"0b_0 = 0"u8,
    @"0b10_11 = 0xb"u8,
    @"0b_0010_1101 = 0x2d"u8
}.slice();

// decimal floats
// hexadecimal floats
// The RHS operand may be a floating-point quotient n/d of two integer values n and d.
internal static slice<@string> floatTests = new @string[]{
    @"1_2_3. = 123."u8,
    @"0_123. = 123."u8,
    @"0_0e0 = 0."u8,
    @"1_2_3e0 = 123."u8,
    @"0_123e0 = 123."u8,
    @"0e-0_0 = 0."u8,
    @"1_2_3E+0 = 123."u8,
    @"0123E1_2_3 = 123e123"u8,
    @"0.e+1 = 0."u8,
    @"123.E-1_0 = 123e-10"u8,
    @"01_23.e123 = 123e123"u8,
    @".0e-1 = .0"u8,
    @".123E+10 = .123e10"u8,
    @".0123E123 = .0123e123"u8,
    @"1_2_3.123 = 123.123"u8,
    @"0123.01_23 = 123.0123"u8,
    @"1e-1000000000 = 0"u8,
    @"1e+1000000000 = ?"u8,
    @"6e5518446744 = ?"u8,
    @"-6e5518446744 = ?"u8,
    @"0x0.p+0 = 0."u8,
    @"0Xdeadcafe.p-10 = 0xdeadcafe/1024"u8,
    @"0x1234.P84 = 0x1234000000000000000000000"u8,
    @"0x.1p-0 = 1/16"u8,
    @"0X.deadcafep4 = 0xdeadcafe/0x10000000"u8,
    @"0x.1234P+12 = 0x1234/0x10"u8,
    @"0x0p0 = 0."u8,
    @"0Xdeadcafep+1 = 0x1bd5b95fc"u8,
    @"0x1234P-10 = 0x1234/1024"u8,
    @"0x0.0p0 = 0."u8,
    @"0Xdead.cafep+1 = 0x1bd5b95fc/0x10000"u8,
    @"0x12.34P-10 = 0x1234/0x40000"u8,
    @"0Xdead_cafep+1 = 0xdeadcafep+1"u8,
    @"0x_1234P-10 = 0x1234p-10"u8,
    @"0X_dead_cafe.p-10 = 0xdeadcafe.p-10"u8,
    @"0x12_34.P1_2_3 = 0x1234.p123"u8
}.slice();

internal static slice<@string> imagTests = new @string[]{
    @"1_234i = 1234i"u8,
    @"1_234_567i = 1234567i"u8,
    @"0.i = 0i"u8,
    @"123.i = 123i"u8,
    @"0123.i = 123i"u8,
    @"0.e+1i = 0i"u8,
    @"123.E-1_0i = 123e-10i"u8,
    @"01_23.e123i = 123e123i"u8,
    @"1e-1000000000i = 0i"u8,
    @"1e+1000000000i = ?"u8,
    @"6e5518446744i = ?"u8,
    @"-6e5518446744i = ?"u8
}.slice();

internal static void testNumbers(ж<testing.T> Ꮡt, token.Token kind, slice<@string> tests) {
    foreach (var (_, test) in tests) {
        var a = strings.Split(test, " = "u8);
        if (len(a) != 2) {
            Ꮡt.Errorf("invalid test case: %s"u8, test);
            continue;
        }
        var x = MakeFromLiteral(a[0], kind, 0);
        global::go.go.constant_package.Value y = default!;
        if (a[1] == "?"){
            y = MakeUnknown();
        } else {
            {
                var (ns, ds, ok) = strings.Cut(a[1], "/"u8); if (ok && kind == token.FLOAT){
                    var n = MakeFromLiteral(ns, token.INT, 0);
                    var d = MakeFromLiteral(ds, token.INT, 0);
                    y = BinaryOp(n, token.QUO, d);
                } else {
                    y = MakeFromLiteral(a[1], kind, 0);
                }
            }
            if (y.Kind() == Unknown) {
                throw panic(fmt.Sprintf("invalid test case: %s %d"u8, test, y.Kind()));
            }
        }
        global::go.go.constant_package.ΔKind xk = x.Kind();
        global::go.go.constant_package.ΔKind yk = y.Kind();
        if (xk != yk) {
            Ꮡt.Errorf("%s: got kind %d != %d"u8, test, xk, yk);
            continue;
        }
        if (yk == Unknown) {
            continue;
        }
        if (!Compare(x, token.EQL, y)) {
            Ꮡt.Errorf("%s: %s != %s"u8, test, x, y);
        }
    }
}

// TestNumbers verifies that differently written literals
// representing the same number do have the same value.
public static void TestNumbers(ж<testing.T> Ꮡt) {
    testNumbers(Ꮡt, token.INT, intTests);
    testNumbers(Ꮡt, token.FLOAT, floatTests);
    testNumbers(Ꮡt, token.IMAG, imagTests);
}

// unary operations
// etc.
// binary operations
// TODO(gri) should be the same as for /
// etc.
// shifts
// etc.
// comparisons
// etc.
internal static slice<@string> opTests = new @string[]{
    @"+ 0 = 0"u8,
    @"+ ? = ?"u8,
    @"- 1 = -1"u8,
    @"- ? = ?"u8,
    @"^ 0 = -1"u8,
    @"^ ? = ?"u8,
    @"! true = false"u8,
    @"! false = true"u8,
    @"! ? = ?"u8,
    @""""" + """" = """""u8,
    @"""foo"" + """" = ""foo"""u8,
    @""""" + ""bar"" = ""bar"""u8,
    @"""foo"" + ""bar"" = ""foobar"""u8,
    @"0 + 0 = 0"u8,
    @"0 + 0.1 = 0.1"u8,
    @"0 + 0.1i = 0.1i"u8,
    @"0.1 + 0.9 = 1"u8,
    @"1e100 + 1e100 = 2e100"u8,
    @"? + 0 = ?"u8,
    @"0 + ? = ?"u8,
    @"0 - 0 = 0"u8,
    @"0 - 0.1 = -0.1"u8,
    @"0 - 0.1i = -0.1i"u8,
    @"1e100 - 1e100 = 0"u8,
    @"? - 0 = ?"u8,
    @"0 - ? = ?"u8,
    @"0 * 0 = 0"u8,
    @"1 * 0.1 = 0.1"u8,
    @"1 * 0.1i = 0.1i"u8,
    @"1i * 1i = -1"u8,
    @"? * 0 = ?"u8,
    @"0 * ? = ?"u8,
    @"0 * 1e+1000000000 = ?"u8,
    @"0 / 0 = ""division_by_zero"""u8,
    @"10 / 2 = 5"u8,
    @"5 / 3 = 5/3"u8,
    @"5i / 3i = 5/3"u8,
    @"? / 0 = ?"u8,
    @"0 / ? = ?"u8,
    @"0 * 1e+1000000000i = ?"u8,
    @"0 % 0 = ""runtime_error:_integer_divide_by_zero"""u8,
    @"10 % 3 = 1"u8,
    @"? % 0 = ?"u8,
    @"0 % ? = ?"u8,
    @"0 & 0 = 0"u8,
    @"12345 & 0 = 0"u8,
    @"0xff & 0xf = 0xf"u8,
    @"? & 0 = ?"u8,
    @"0 & ? = ?"u8,
    @"0 | 0 = 0"u8,
    @"12345 | 0 = 12345"u8,
    @"0xb | 0xa0 = 0xab"u8,
    @"? | 0 = ?"u8,
    @"0 | ? = ?"u8,
    @"0 ^ 0 = 0"u8,
    @"1 ^ -1 = -2"u8,
    @"? ^ 0 = ?"u8,
    @"0 ^ ? = ?"u8,
    @"0 &^ 0 = 0"u8,
    @"0xf &^ 1 = 0xe"u8,
    @"1 &^ 0xf = 0"u8,
    @"0 << 0 = 0"u8,
    @"1 << 10 = 1024"u8,
    @"0 >> 0 = 0"u8,
    @"1024 >> 10 == 1"u8,
    @"? << 0 == ?"u8,
    @"? >> 10 == ?"u8,
    @"false == false = true"u8,
    @"false == true = false"u8,
    @"true == false = false"u8,
    @"true == true = true"u8,
    @"false != false = false"u8,
    @"false != true = true"u8,
    @"true != false = true"u8,
    @"true != true = false"u8,
    @"""foo"" == ""bar"" = false"u8,
    @"""foo"" != ""bar"" = true"u8,
    @"""foo"" < ""bar"" = false"u8,
    @"""foo"" <= ""bar"" = false"u8,
    @"""foo"" > ""bar"" = true"u8,
    @"""foo"" >= ""bar"" = true"u8,
    @"0 == 0 = true"u8,
    @"0 != 0 = false"u8,
    @"0 < 10 = true"u8,
    @"10 <= 10 = true"u8,
    @"0 > 10 = false"u8,
    @"10 >= 10 = true"u8,
    @"1/123456789 == 1/123456789 == true"u8,
    @"1/123456789 != 1/123456789 == false"u8,
    @"1/123456789 < 1/123456788 == true"u8,
    @"1/123456788 <= 1/123456789 == false"u8,
    @"0.11 > 0.11 = false"u8,
    @"0.11 >= 0.11 = true"u8,
    @"? == 0 = false"u8,
    @"? != 0 = false"u8,
    @"? < 10 = false"u8,
    @"? <= 10 = false"u8,
    @"? > 10 = false"u8,
    @"? >= 10 = false"u8,
    @"0 == ? = false"u8,
    @"0 != ? = false"u8,
    @"0 < ? = false"u8,
    @"10 <= ? = false"u8,
    @"0 > ? = false"u8,
    @"10 >= ? = false"u8
}.slice();

public static void TestOps(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in opTests) {
        var a = strings.Split(test, " "u8);
        nint i = 0; // operator index
        global::go.go.constant_package.Value x = default!;
        global::go.go.constant_package.Value x0 = default!;
        switch (len(a)) {
        case 4: {
            break;
        }
        case 5: {
            (x, x0) = (val(a[0]), val(a[0]));
            i = 1;
            break;
        }
        default: {
            Ꮡt.Errorf("invalid test case: %s"u8, // unary operation
 // binary operation
 test);
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
            Ꮡt.Errorf("%s: got %s; want %s"u8, test, got, want);
            continue;
        }
        if (x0 != default! && !eql(x, x0)) {
            Ꮡt.Errorf("%s: x changed to %s"u8, test, x);
            continue;
        }
        if (!eql(y, y0)) {
            Ꮡt.Errorf("%s: y changed to %s"u8, test, y);
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
internal static @string xxx = strings.Repeat("x"u8, 68);

internal static @string issue14262 = @"""بموجب الشروط التالية نسب المصنف — يجب عليك أن تنسب العمل بالطريقة التي تحددها المؤلف أو المرخص (ولكن ليس بأي حال من الأحوال أن توحي وتقترح بتحول أو استخدامك للعمل).  المشاركة على قدم المساواة — إذا كنت يعدل ، والتغيير ، أو الاستفادة من هذا العمل ، قد ينتج عن توزيع العمل إلا في ظل تشابه او تطابق فى واحد لهذا الترخيص."""u8;

// Unknown
// Bool
// String
// Int
// Float
// issue #16176
// issue #20228
// Complex

partial struct stringTestsᴛ1 /*dyn*/ {
    internal @string input, @short, exact;
}
internal static slice<stringTestsᴛ1> stringTests = new stringTestsᴛ1[]{
    new(""u8, "unknown"u8, "unknown"u8),
    new("0x"u8, "unknown"u8, "unknown"u8),
    new("'"u8, "unknown"u8, "unknown"u8),
    new("1f0"u8, "unknown"u8, "unknown"u8),
    new("unknown"u8, "unknown"u8, "unknown"u8),
    new("true"u8, "true"u8, "true"u8),
    new("false"u8, "false"u8, "false"u8),
    new(@""""""u8, @""""""u8, @""""""u8),
    new(@"""foo"""u8, @"""foo"""u8, @"""foo"""u8),
    new(@""""u8 + xxx + @"xx"""u8, @""""u8 + xxx + @"xx"""u8, @""""u8 + xxx + @"xx"""u8),
    new(@""""u8 + xxx + @"xxx"""u8, @""""u8 + xxx + @"..."u8, @""""u8 + xxx + @"xxx"""u8),
    new(@""""u8 + xxx + xxx + @"xxx"""u8, @""""u8 + xxx + @"..."u8, @""""u8 + xxx + xxx + @"xxx"""u8),
    new(issue14262, @"""بموجب الشروط التالية نسب المصنف — يجب عليك أن تنسب العمل بالطريقة ال..."u8, issue14262),
    new("0"u8, "0"u8, "0"u8),
    new("-1"u8, "-1"u8, "-1"u8),
    new("12345"u8, "12345"u8, "12345"u8),
    new("-12345678901234567890"u8, "-12345678901234567890"u8, "-12345678901234567890"u8),
    new("12345678901234567890"u8, "12345678901234567890"u8, "12345678901234567890"u8),
    new("0."u8, "0"u8, "0"u8),
    new("-0.0"u8, "0"u8, "0"u8),
    new("10.0"u8, "10"u8, "10"u8),
    new("2.1"u8, "2.1"u8, "21/10"u8),
    new("-2.1"u8, "-2.1"u8, "-21/10"u8),
    new("1e9999"u8, "1e+9999"u8, "0x.f8d4a9da224650a8cb2959e10d985ad92adbd44c62917e608b1f24c0e1b76b6f61edffeb15c135a4b601637315f7662f325f82325422b244286a07663c9415d2p+33216"u8),
    new("1e-9999"u8, "1e-9999"u8, "0x.83b01ba6d8c0425eec1b21e96f7742d63c2653ed0a024cf8a2f9686df578d7b07d7a83d84df6a2ec70a921d1f6cd5574893a7eda4d28ee719e13a5dce2700759p-33215"u8),
    new("2.71828182845904523536028747135266249775724709369995957496696763"u8, "2.71828"u8, "271828182845904523536028747135266249775724709369995957496696763/100000000000000000000000000000000000000000000000000000000000000"u8),
    new("0e9999999999"u8, "0"u8, "0"u8),
    new("-6e-1886451601"u8, "0"u8, "0"u8),
    new("0i"u8, "(0 + 0i)"u8, "(0 + 0i)"u8),
    new("-0i"u8, "(0 + 0i)"u8, "(0 + 0i)"u8),
    new("10i"u8, "(0 + 10i)"u8, "(0 + 10i)"u8),
    new("-10i"u8, "(0 + -10i)"u8, "(0 + -10i)"u8),
    new("1e9999i"u8, "(0 + 1e+9999i)"u8, "(0 + 0x.f8d4a9da224650a8cb2959e10d985ad92adbd44c62917e608b1f24c0e1b76b6f61edffeb15c135a4b601637315f7662f325f82325422b244286a07663c9415d2p+33216i)"u8)
}.slice();

public static void TestString(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in stringTests) {
        var x = val(test.input);
        {
            @string got = x.String(); if (got != test.@short) {
                Ꮡt.Errorf("%s: got %q; want %q as short string"u8, test.input, got, test.@short);
            }
        }
        {
            @string got = x.ExactString(); if (got != test.exact) {
                Ꮡt.Errorf("%s: got %q; want %q as exact string"u8, test.input, got, test.exact);
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
    if (exprᴛ1 == "?"u8) {
        return MakeUnknown();
    }
    if (exprᴛ1 == "true"u8) {
        return MakeBool(true);
    }
    if (exprᴛ1 == "false"u8) {
        return MakeBool(false);
    }

    {
        var (@as, bs, ok) = strings.Cut(lit, "/"u8); if (ok) {
            // assume fraction
            var a = MakeFromLiteral(@as, token.INT, 0);
            var b = MakeFromLiteral(bs, token.INT, 0);
            return BinaryOp(a, token.QUO, b);
        }
    }
    token.Token tok = token.INT;
    {
        var (first, last) = (lit[0], lit[len(lit) - 1]);
        switch (ᐧ) {
        case {} when first == (rune)'"' || first == (rune)'`': {
            tok = token.STRING;
            lit = strings.ReplaceAll(lit, "_"u8, " "u8);
            break;
        }
        case {} when first is (rune)'\'': {
            tok = token.CHAR;
            break;
        }
        case {} when last is (rune)'i': {
            tok = token.IMAG;
            break;
        }
        default: {
            if (!strings.HasPrefix(lit, "0x"u8) && strings.ContainsAny(lit, "./Ee"u8)) {
                tok = token.FLOAT;
            }
            break;
        }}
    }

    return MakeFromLiteral(lit, tok, 0);
}

internal static map<@string, token.Token> optab = new map<@string, token.Token>{
    ["!"u8] = token.NOT,
    ["+"u8] = token.ADD,
    ["-"u8] = token.SUB,
    ["*"u8] = token.MUL,
    ["/"u8] = token.QUO,
    ["%"u8] = token.REM,
    ["<<"u8] = token.SHL,
    [">>"u8] = token.SHR,
    ["&"u8] = token.AND,
    ["|"u8] = token.OR,
    ["^"u8] = token.XOR,
    ["&^"u8] = token.AND_NOT,
    ["=="u8] = token.EQL,
    ["!="u8] = token.NEQ,
    ["<"u8] = token.LSS,
    ["<="u8] = token.LEQ,
    [">"u8] = token.GTR,
    [">="u8] = token.GEQ
};

internal static void panicHandler(ж<global::go.go.constant_package.Value> Ꮡv) {
    GoFrame ᒐ = default;
    try {
        ref var v = ref Ꮡv.DerefOrNull();

        var switchᴛ1 = recover();
        switch (switchᴛ1.type()) {
        case null: {
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

// nothing to do
internal static global::go.go.constant_package.Value /*z*/ doOp(global::go.go.constant_package.Value x, token.Token op, global::go.go.constant_package.Value y) {
    heap<global::go.go.constant_package.Value>(out var Ꮡz);
    GoFrame ᒐ = default;
    try {
        ref var z = ref Ꮡz.ValueSlot;

        defer(panicHandler, Ꮡz, ref ᒐ);
        if (x == default!) {
            z = UnaryOp(op, y, 0); goto ᒐdone;
        }
        var exprᴛ1 = op;
        if (exprᴛ1 == token.EQL || exprᴛ1 == token.NEQ || exprᴛ1 == token.LSS || exprᴛ1 == token.LEQ || exprᴛ1 == token.GTR || exprᴛ1 == token.GEQ) {
            z = MakeBool(Compare(x, op, y)); goto ᒐdone;
        }
        if (exprᴛ1 == token.SHL || exprᴛ1 == token.SHR) {
            var (s, _) = Int64Val(y);
            z = Shift(x, op, (nuint)s); goto ᒐdone;
        }
        { /* default: */
            z = BinaryOp(x, op, y); goto ᒐdone;
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡz.ValueSlot;
}

// ----------------------------------------------------------------------------
// Other tests
internal static slice<@string> fracTests = new @string[]{
    "0"u8,
    "1"u8,
    "-1"u8,
    "1.2"u8,
    "-0.991"u8,
    "2.718281828"u8,
    "3.14159265358979323e-10"u8,
    "1e100"u8,
    "1e1000"u8
}.slice();

public static void TestFractions(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in fracTests) {
        var x = val(test);
        // We don't check the actual numerator and denominator because they
        // are unlikely to be 100% correct due to floatVal rounding errors.
        // Instead, we compute the fraction again and compare the rounded
        // result.
        var q = BinaryOp(Num(x), token.QUO, Denom(x));
        @string got = q.String();
        @string want = x.String();
        if (got != want) {
            Ꮡt.Errorf("%s: got quotient %s, want %s"u8, x, got, want);
        }
    }
}

internal static slice<@string> bytesTests = new @string[]{
    "0"u8,
    "1"u8,
    "123456789"u8,
    "123456789012345678901234567890123456789012345678901234567890"u8
}.slice();

public static void TestBytes(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in bytesTests) {
        var x = val(test);
        var bytes = Bytes(x);
        // special case 0
        if (Sign(x) == 0 && len(bytes) != 0) {
            Ꮡt.Errorf("%s: got %v; want empty byte slice"u8, test, bytes);
        }
        {
            nint n = len(bytes); if (n > 0 && bytes[n - 1] == 0) {
                Ꮡt.Errorf("%s: got %v; want no leading 0 byte"u8, test, bytes);
            }
        }
        {
            var got = MakeFromBytes(bytes); if (!eql(got, x)) {
                Ꮡt.Errorf("%s: got %s; want %s (bytes = %v)"u8, test, got, x, bytes);
            }
        }
    }
}

public static void TestUnknown(ж<testing.T> Ꮡt) {
    var u = MakeUnknown();
// token.ADD ok below, operation is never considered
    slice<global::go.go.constant_package.Value> values = new global::go.go.constant_package.Value[]{u, MakeBool(false), MakeString(""u8), MakeInt64(1), MakeFromLiteral("''"u8, token.CHAR, 0), MakeFromLiteral("-1234567890123456789012345678901234567890"u8, token.INT, 0), MakeFloat64(1.2D), MakeImag(MakeFloat64(1.2D))
    }.slice();
    foreach (var (_, val) in values) {
        var (x, y) = (val, u);
        foreach (var (i, _) in new nint[]{}.array(2)) {
            if (i == 1) {
                (x, y) = (y, x);
            }
            {
                var got = BinaryOp(x, token.ADD, y); if (got.Kind() != Unknown) {
                    Ꮡt.Errorf("%s + %s: got %s; want %s"u8, x, y, got, u);
                }
            }
            {
                var got = Compare(x, token.EQL, y); if (got) {
                    Ꮡt.Errorf("%s == %s: got true; want false"u8, x, y);
                }
            }
        }
    }
}

public static void TestMakeFloat64(ж<testing.T> Ꮡt) {
    float64 zero = default!;
    foreach (var (_, arg) in new float64[]{
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
    }.slice()) {
        var val = MakeFloat64(arg);
        if (val.Kind() != Float) {
            Ꮡt.Errorf("%v: got kind = %d; want %d"u8, arg, val.Kind(), Float);
        }
        // -0.0 is mapped to 0.0
        var (got, exact) = Float64Val(val);
        if (!exact || math.Float64bits(got) != math.Float64bits(arg + 0D)) {
            Ꮡt.Errorf("%v: got %v (exact = %v)"u8, arg, got, exact);
        }
    }
    // infinity
    foreach (var (sign, _) in new nint[]{-1, 1}.slice()) {
        var arg = math.Inf(sign);
        var val = MakeFloat64(arg);
        if (val.Kind() != Unknown) {
            Ꮡt.Errorf("%v: got kind = %d; want %d"u8, arg, val.Kind(), Unknown);
        }
    }
}

internal partial struct makeTestCase {
    internal global::go.go.constant_package.ΔKind kind;
    internal any arg, want;
}

internal static makeTestCase dup(global::go.go.constant_package.ΔKind k, any x) {
    return new makeTestCase(k, x, x);
}

public static void TestMake(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new makeTestCase[]{
        new(Bool, false, false),
        new(ΔString, (@string)"hello"u8, (@string)"hello"u8),
        new(Int, (int64)1, (int64)1),
        new(Int, big.NewInt(10).OrTypedNil(), (int64)10),
        new(Int, @new<bigꓸInt>().Lsh(big.NewInt(1), 62).OrTypedNil(), (int64)(4611686018427387904L)),
        dup(Int, @new<bigꓸInt>().Lsh(big.NewInt(1), 63).OrTypedNil()),
        new(Float, big.NewFloat(0D).OrTypedNil(), floatVal0.val.OrTypedNil()),
        dup(Float, big.NewFloat(2.0D).OrTypedNil()),
        dup(Float, big.NewRat(1, 3).OrTypedNil())
    }.slice()) {
        var val = Make(test.arg);
        var got = Val(val);
        if (val.Kind() != test.kind || !AreEqual(got, test.want)) {
            Ꮡt.Errorf("got %v (%T, kind = %d); want %v (%T, kind = %d)"u8,
                got, got, val.Kind(), test.want, test.want, test.kind);
        }
    }
}

public static void BenchmarkStringAdd(ж<testing.B> Ꮡb) {
    for (nint sizeᴛ1 = 1; sizeᴛ1 <= 65536; sizeᴛ1 *= 4) {
        var size = sizeᴛ1;
        Ꮡb.Run(fmt.Sprint(size), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            var n = (int64)0;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var x = MakeString(strings.Repeat("x"u8, 100));
                var y = x;
                for (nint j = 0; j < size - 1; j++) {
                    y = BinaryOp(y, token.ADD, x);
                }
                n += (int64)len(StringVal(y));
            }
            if (n != (int64)(~bΔ1).N * (int64)size * 100) {
                bΔ1.Fatalf("bad string %d != %d"u8, n, (int64)(~bΔ1).N * (int64)size * 100);
            }
        });
    }
}


partial struct bitLenTestsᴛ1 /*dyn*/ {
    internal int64 val;
    internal nint want;
}
internal static slice<bitLenTestsᴛ1> bitLenTests = new bitLenTestsᴛ1[]{
    new(0, 0),
    new(1, 1),
    new(-16, 5),
    new(2305843009213693952L, 62),
    new(4611686018427387904L, 63),
    new(-4611686018427387904L, 63),
    new(-9223372036854775808L, 64)
}.slice();

public static void TestBitLen(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in bitLenTests) {
        {
            nint got = BitLen(MakeInt64(test.val)); if (got != test.want) {
                Ꮡt.Errorf("%v: got %v, want %v"u8, test.val, got, test.want);
            }
        }
    }
}

} // end constant_internal_test_package
