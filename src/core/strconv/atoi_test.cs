// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using static strconv_package;
using testing = testing_package;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

partial struct parseUint64Test {
    internal @string @in;
    internal uint64 @out;
    internal error err;
}

// base=10 so no underscores allowed
internal static ж<slice<parseUint64Test>> ᏑparseUint64Tests = new StandardBox<slice<parseUint64Test>>(new parseUint64Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("12345"u8, 12345, default!),
    new("012345"u8, 12345, default!),
    new("12345x"u8, 0, ErrSyntax),
    new("98765432100"u8, 98765432100UL, default!),
    new("18446744073709551615"u8, 18446744073709551615UL, default!),
    new("18446744073709551616"u8, 18446744073709551615UL, ErrRange),
    new("18446744073709551620"u8, 18446744073709551615UL, ErrRange),
    new("1_2_3_4_5"u8, 0, ErrSyntax),
    new("_12345"u8, 0, ErrSyntax),
    new("1__2345"u8, 0, ErrSyntax),
    new("12345_"u8, 0, ErrSyntax),
    new("-0"u8, 0, ErrSyntax),
    new("-1"u8, 0, ErrSyntax),
    new("+1"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseUint64Test> parseUint64Tests => ref ᏑparseUint64Tests.ValueSlot;

partial struct parseUint64BaseTest {
    internal @string @in;
    internal nint @base;
    internal uint64 @out;
    internal error err;
}

// underscores allowed with base == 0 only
// base 0 => 10
// base 10
// base 0 => 16
// base 16
// base 0 => 8 (0377)
// base 0 => 8 (0o377)
// base 8
// base 0 => 2 (0b101)
// base 2
internal static ж<slice<parseUint64BaseTest>> ᏑparseUint64BaseTests = new StandardBox<slice<parseUint64BaseTest>>(new parseUint64BaseTest[]{
    new(""u8, 0, 0, ErrSyntax),
    new("0"u8, 0, 0, default!),
    new("0x"u8, 0, 0, ErrSyntax),
    new("0X"u8, 0, 0, ErrSyntax),
    new("1"u8, 0, 1, default!),
    new("12345"u8, 0, 12345, default!),
    new("012345"u8, 0, 5349, default!),
    new("0x12345"u8, 0, 0x12345, default!),
    new("0X12345"u8, 0, 0x12345, default!),
    new("12345x"u8, 0, 0, ErrSyntax),
    new("0xabcdefg123"u8, 0, 0, ErrSyntax),
    new("123456789abc"u8, 0, 0, ErrSyntax),
    new("98765432100"u8, 0, 98765432100UL, default!),
    new("18446744073709551615"u8, 0, 18446744073709551615UL, default!),
    new("18446744073709551616"u8, 0, 18446744073709551615UL, ErrRange),
    new("18446744073709551620"u8, 0, 18446744073709551615UL, ErrRange),
    new("0xFFFFFFFFFFFFFFFF"u8, 0, 18446744073709551615UL, default!),
    new("0x10000000000000000"u8, 0, 18446744073709551615UL, ErrRange),
    new("01777777777777777777777"u8, 0, 18446744073709551615UL, default!),
    new("01777777777777777777778"u8, 0, 0, ErrSyntax),
    new("02000000000000000000000"u8, 0, 18446744073709551615UL, ErrRange),
    new("0200000000000000000000"u8, 0, ((uint64)1 << (int)(61)), default!),
    new("0b"u8, 0, 0, ErrSyntax),
    new("0B"u8, 0, 0, ErrSyntax),
    new("0b101"u8, 0, 5, default!),
    new("0B101"u8, 0, 5, default!),
    new("0o"u8, 0, 0, ErrSyntax),
    new("0O"u8, 0, 0, ErrSyntax),
    new("0o377"u8, 0, 255, default!),
    new("0O377"u8, 0, 255, default!),
    new("1_2_3_4_5"u8, 0, 12345, default!),
    new("_12345"u8, 0, 0, ErrSyntax),
    new("1__2345"u8, 0, 0, ErrSyntax),
    new("12345_"u8, 0, 0, ErrSyntax),
    new("1_2_3_4_5"u8, 10, 0, ErrSyntax),
    new("_12345"u8, 10, 0, ErrSyntax),
    new("1__2345"u8, 10, 0, ErrSyntax),
    new("12345_"u8, 10, 0, ErrSyntax),
    new("0x_1_2_3_4_5"u8, 0, 0x12345, default!),
    new("_0x12345"u8, 0, 0, ErrSyntax),
    new("0x__12345"u8, 0, 0, ErrSyntax),
    new("0x1__2345"u8, 0, 0, ErrSyntax),
    new("0x1234__5"u8, 0, 0, ErrSyntax),
    new("0x12345_"u8, 0, 0, ErrSyntax),
    new("1_2_3_4_5"u8, 16, 0, ErrSyntax),
    new("_12345"u8, 16, 0, ErrSyntax),
    new("1__2345"u8, 16, 0, ErrSyntax),
    new("1234__5"u8, 16, 0, ErrSyntax),
    new("12345_"u8, 16, 0, ErrSyntax),
    new("0_1_2_3_4_5"u8, 0, 5349, default!),
    new("_012345"u8, 0, 0, ErrSyntax),
    new("0__12345"u8, 0, 0, ErrSyntax),
    new("01234__5"u8, 0, 0, ErrSyntax),
    new("012345_"u8, 0, 0, ErrSyntax),
    new("0o_1_2_3_4_5"u8, 0, 5349, default!),
    new("_0o12345"u8, 0, 0, ErrSyntax),
    new("0o__12345"u8, 0, 0, ErrSyntax),
    new("0o1234__5"u8, 0, 0, ErrSyntax),
    new("0o12345_"u8, 0, 0, ErrSyntax),
    new("0_1_2_3_4_5"u8, 8, 0, ErrSyntax),
    new("_012345"u8, 8, 0, ErrSyntax),
    new("0__12345"u8, 8, 0, ErrSyntax),
    new("01234__5"u8, 8, 0, ErrSyntax),
    new("012345_"u8, 8, 0, ErrSyntax),
    new("0b_1_0_1"u8, 0, 5, default!),
    new("_0b101"u8, 0, 0, ErrSyntax),
    new("0b__101"u8, 0, 0, ErrSyntax),
    new("0b1__01"u8, 0, 0, ErrSyntax),
    new("0b10__1"u8, 0, 0, ErrSyntax),
    new("0b101_"u8, 0, 0, ErrSyntax),
    new("1_0_1"u8, 2, 0, ErrSyntax),
    new("_101"u8, 2, 0, ErrSyntax),
    new("1_01"u8, 2, 0, ErrSyntax),
    new("10_1"u8, 2, 0, ErrSyntax),
    new("101_"u8, 2, 0, ErrSyntax)
}.slice());
internal static ref slice<parseUint64BaseTest> parseUint64BaseTests => ref ᏑparseUint64BaseTests.ValueSlot;

partial struct parseInt64Test {
    internal @string @in;
    internal int64 @out;
    internal error err;
}

// base=10 so no underscores allowed
internal static ж<slice<parseInt64Test>> ᏑparseInt64Tests = new StandardBox<slice<parseInt64Test>>(new parseInt64Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("-0"u8, 0, default!),
    new("+0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("-1"u8, -1, default!),
    new("+1"u8, 1, default!),
    new("12345"u8, 12345, default!),
    new("-12345"u8, -12345, default!),
    new("012345"u8, 12345, default!),
    new("-012345"u8, -12345, default!),
    new("98765432100"u8, 98765432100L, default!),
    new("-98765432100"u8, -98765432100L, default!),
    new("9223372036854775807"u8, 9223372036854775807L, default!),
    new("-9223372036854775807"u8, -(9223372036854775807L), default!),
    new("9223372036854775808"u8, 9223372036854775807L, ErrRange),
    new("-9223372036854775808"u8, -9223372036854775808L, default!),
    new("9223372036854775809"u8, 9223372036854775807L, ErrRange),
    new("-9223372036854775809"u8, -9223372036854775808L, ErrRange),
    new("-1_2_3_4_5"u8, 0, ErrSyntax),
    new("-_12345"u8, 0, ErrSyntax),
    new("_12345"u8, 0, ErrSyntax),
    new("1__2345"u8, 0, ErrSyntax),
    new("12345_"u8, 0, ErrSyntax),
    new("123%45"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseInt64Test> parseInt64Tests => ref ᏑparseInt64Tests.ValueSlot;

partial struct parseInt64BaseTest {
    internal @string @in;
    internal nint @base;
    internal int64 @out;
    internal error err;
}

// other bases
// base 2
// base 8
// base 16
// underscores
// octal
// octal
internal static ж<slice<parseInt64BaseTest>> ᏑparseInt64BaseTests = new StandardBox<slice<parseInt64BaseTest>>(new parseInt64BaseTest[]{
    new(""u8, 0, 0, ErrSyntax),
    new("0"u8, 0, 0, default!),
    new("-0"u8, 0, 0, default!),
    new("1"u8, 0, 1, default!),
    new("-1"u8, 0, -1, default!),
    new("12345"u8, 0, 12345, default!),
    new("-12345"u8, 0, -12345, default!),
    new("012345"u8, 0, 5349, default!),
    new("-012345"u8, 0, -5349, default!),
    new("0x12345"u8, 0, 0x12345, default!),
    new("-0X12345"u8, 0, -0x12345, default!),
    new("12345x"u8, 0, 0, ErrSyntax),
    new("-12345x"u8, 0, 0, ErrSyntax),
    new("98765432100"u8, 0, 98765432100L, default!),
    new("-98765432100"u8, 0, -98765432100L, default!),
    new("9223372036854775807"u8, 0, 9223372036854775807L, default!),
    new("-9223372036854775807"u8, 0, -(9223372036854775807L), default!),
    new("9223372036854775808"u8, 0, 9223372036854775807L, ErrRange),
    new("-9223372036854775808"u8, 0, -9223372036854775808L, default!),
    new("9223372036854775809"u8, 0, 9223372036854775807L, ErrRange),
    new("-9223372036854775809"u8, 0, -9223372036854775808L, ErrRange),
    new("g"u8, 17, 16, default!),
    new("10"u8, 25, 25, default!),
    new("holycow"u8, 35, 32544027072L, default!),
    new("holycow"u8, 36, 38493362624L, default!),
    new("0"u8, 2, 0, default!),
    new("-1"u8, 2, -1, default!),
    new("1010"u8, 2, 10, default!),
    new("1000000000000000"u8, 2, ((int64)1 << (int)(15)), default!),
    new("111111111111111111111111111111111111111111111111111111111111111"u8, 2, 9223372036854775807L, default!),
    new("1000000000000000000000000000000000000000000000000000000000000000"u8, 2, 9223372036854775807L, ErrRange),
    new("-1000000000000000000000000000000000000000000000000000000000000000"u8, 2, -9223372036854775808L, default!),
    new("-1000000000000000000000000000000000000000000000000000000000000001"u8, 2, -9223372036854775808L, ErrRange),
    new("-10"u8, 8, -8, default!),
    new("57635436545"u8, 8, 6416645477L, default!),
    new("100000000"u8, 8, ((int64)1 << (int)(24)), default!),
    new("10"u8, 16, 16, default!),
    new("-123456789abcdef"u8, 16, -0x123456789abcdefL, default!),
    new("7fffffffffffffff"u8, 16, 9223372036854775807L, default!),
    new("-0x_1_2_3_4_5"u8, 0, -0x12345, default!),
    new("0x_1_2_3_4_5"u8, 0, 0x12345, default!),
    new("-_0x12345"u8, 0, 0, ErrSyntax),
    new("_-0x12345"u8, 0, 0, ErrSyntax),
    new("_0x12345"u8, 0, 0, ErrSyntax),
    new("0x__12345"u8, 0, 0, ErrSyntax),
    new("0x1__2345"u8, 0, 0, ErrSyntax),
    new("0x1234__5"u8, 0, 0, ErrSyntax),
    new("0x12345_"u8, 0, 0, ErrSyntax),
    new("-0_1_2_3_4_5"u8, 0, -5349, default!),
    new("0_1_2_3_4_5"u8, 0, 5349, default!),
    new("-_012345"u8, 0, 0, ErrSyntax),
    new("_-012345"u8, 0, 0, ErrSyntax),
    new("_012345"u8, 0, 0, ErrSyntax),
    new("0__12345"u8, 0, 0, ErrSyntax),
    new("01234__5"u8, 0, 0, ErrSyntax),
    new("012345_"u8, 0, 0, ErrSyntax),
    new("+0xf"u8, 0, 0xf, default!),
    new("-0xf"u8, 0, -0xf, default!),
    new("0x+f"u8, 0, 0, ErrSyntax),
    new("0x-f"u8, 0, 0, ErrSyntax)
}.slice());
internal static ref slice<parseInt64BaseTest> parseInt64BaseTests => ref ᏑparseInt64BaseTests.ValueSlot;

partial struct parseUint32Test {
    internal @string @in;
    internal uint32 @out;
    internal error err;
}

// base=10 so no underscores allowed
internal static ж<slice<parseUint32Test>> ᏑparseUint32Tests = new StandardBox<slice<parseUint32Test>>(new parseUint32Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("12345"u8, 12345, default!),
    new("012345"u8, 12345, default!),
    new("12345x"u8, 0, ErrSyntax),
    new("987654321"u8, 987654321, default!),
    new("4294967295"u8, unchecked((uint32)(4294967295UL)), default!),
    new("4294967296"u8, unchecked((uint32)(4294967295UL)), ErrRange),
    new("1_2_3_4_5"u8, 0, ErrSyntax),
    new("_12345"u8, 0, ErrSyntax),
    new("_12345"u8, 0, ErrSyntax),
    new("1__2345"u8, 0, ErrSyntax),
    new("12345_"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseUint32Test> parseUint32Tests => ref ᏑparseUint32Tests.ValueSlot;

partial struct parseInt32Test {
    internal @string @in;
    internal int32 @out;
    internal error err;
}

// base=10 so no underscores allowed
internal static ж<slice<parseInt32Test>> ᏑparseInt32Tests = new StandardBox<slice<parseInt32Test>>(new parseInt32Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("-0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("-1"u8, -1, default!),
    new("12345"u8, 12345, default!),
    new("-12345"u8, -12345, default!),
    new("012345"u8, 12345, default!),
    new("-012345"u8, -12345, default!),
    new("12345x"u8, 0, ErrSyntax),
    new("-12345x"u8, 0, ErrSyntax),
    new("987654321"u8, 987654321, default!),
    new("-987654321"u8, -987654321, default!),
    new("2147483647"u8, (int32)(2147483648L - 1), default!),
    new("-2147483647"u8, (int32)(-(2147483648L - 1)), default!),
    new("2147483648"u8, (int32)(2147483648L - 1), ErrRange),
    new("-2147483648"u8, (int32)(-1 << (int)(31)), default!),
    new("2147483649"u8, (int32)(2147483648L - 1), ErrRange),
    new("-2147483649"u8, (int32)(-1 << (int)(31)), ErrRange),
    new("-1_2_3_4_5"u8, 0, ErrSyntax),
    new("-_12345"u8, 0, ErrSyntax),
    new("_12345"u8, 0, ErrSyntax),
    new("1__2345"u8, 0, ErrSyntax),
    new("12345_"u8, 0, ErrSyntax),
    new("123%45"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseInt32Test> parseInt32Tests => ref ᏑparseInt32Tests.ValueSlot;

partial struct numErrorTest {
    internal @string num, want;
}

internal static slice<numErrorTest> numErrorTests = new numErrorTest[]{
    new("0"u8, @"strconv.ParseFloat: parsing ""0"": failed"u8),
    new("`"u8, "strconv.ParseFloat: parsing \"`\": failed"u8),
    new("1\x00.2"u8, @"strconv.ParseFloat: parsing ""1\x00.2"": failed"u8)
}.slice();

[GoInit] internal static void init() {
    // The parse routines return NumErrors wrapping
    // the error and the string. Convert the tables above.
    foreach (var (i, _) in parseUint64Tests) {
        var test = Ꮡ(parseUint64Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var (i, _) in parseUint64BaseTests) {
        var test = Ꮡ(parseUint64BaseTests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var (i, _) in parseInt64Tests) {
        var test = Ꮡ(parseInt64Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var (i, _) in parseInt64BaseTests) {
        var test = Ꮡ(parseInt64BaseTests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var (i, _) in parseUint32Tests) {
        var test = Ꮡ(parseUint32Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var (i, _) in parseInt32Tests) {
        var test = Ꮡ(parseInt32Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt"u8, (~test).@in, (~test).err)));
        }
    }
}

public static void TestParseUint32(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseUint32Tests) {
        var test = Ꮡ(parseUint32Tests, i);
        var (@out, err) = ParseUint((~test).@in, 10, 32);
        if ((uint64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseUint(%q, 10, 32) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseUint64(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseUint64Tests) {
        var test = Ꮡ(parseUint64Tests, i);
        var (@out, err) = ParseUint((~test).@in, 10, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseUint(%q, 10, 64) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseUint64Base(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseUint64BaseTests) {
        var test = Ꮡ(parseUint64BaseTests, i);
        var (@out, err) = ParseUint((~test).@in, (~test).@base, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseUint(%q, %v, 64) = %v, %v want %v, %v"u8,
                (~test).@in, (~test).@base, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseInt32(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseInt32Tests) {
        var test = Ꮡ(parseInt32Tests, i);
        var (@out, err) = ParseInt((~test).@in, 10, 32);
        if ((int64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseInt(%q, 10 ,32) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseInt64(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseInt64Tests) {
        var test = Ꮡ(parseInt64Tests, i);
        var (@out, err) = ParseInt((~test).@in, 10, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseInt(%q, 10, 64) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseInt64Base(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseInt64BaseTests) {
        var test = Ꮡ(parseInt64BaseTests, i);
        var (@out, err) = ParseInt((~test).@in, (~test).@base, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseInt(%q, %v, 64) = %v, %v want %v, %v"u8,
                (~test).@in, (~test).@base, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseUint(ж<testing.T> Ꮡt) {
    var exprᴛ1 = IntSize;
    if (exprᴛ1 == 32) {
        foreach (var (i, _) in parseUint32Tests) {
            var test = Ꮡ(parseUint32Tests, i);
            var (@out, err) = ParseUint((~test).@in, 10, 0);
            if ((uint64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseUint(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var (i, _) in parseUint64Tests) {
            var test = Ꮡ(parseUint64Tests, i);
            var (@out, err) = ParseUint((~test).@in, 10, 0);
            if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseUint(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }

}

public static void TestParseInt(ж<testing.T> Ꮡt) {
    var exprᴛ1 = IntSize;
    if (exprᴛ1 == 32) {
        foreach (var (i, _) in parseInt32Tests) {
            var test = Ꮡ(parseInt32Tests, i);
            var (@out, err) = ParseInt((~test).@in, 10, 0);
            if ((int64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseInt(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var (i, _) in parseInt64Tests) {
            var test = Ꮡ(parseInt64Tests, i);
            var (@out, err) = ParseInt((~test).@in, 10, 0);
            if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseInt(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }

}

public static void TestAtoi(ж<testing.T> Ꮡt) {
    var exprᴛ1 = IntSize;
    if (exprᴛ1 == 32) {
        foreach (var (i, _) in parseInt32Tests) {
            var test = Ꮡ(parseInt32Tests, i);
            var (@out, err) = Atoi((~test).@in);
            error testErr = default!;
            if ((~test).err != default!) {
                testErr = new strconv.NumErrorжerror(Ꮡ(new NumError("Atoi"u8, (~test).@in, (~(~test).err._<ж<strconv.NumError>>()).Err)));
            }
            if ((nint)(~test).@out != @out || !reflect.DeepEqual(testErr, err)) {
                Ꮡt.Errorf("Atoi(%q) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, testErr);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var (i, _) in parseInt64Tests) {
            var test = Ꮡ(parseInt64Tests, i);
            var (@out, err) = Atoi((~test).@in);
            error testErr = default!;
            if ((~test).err != default!) {
                testErr = new strconv.NumErrorжerror(Ꮡ(new NumError("Atoi"u8, (~test).@in, (~(~test).err._<ж<strconv.NumError>>()).Err)));
            }
            if ((~test).@out != (int64)@out || !reflect.DeepEqual(testErr, err)) {
                Ꮡt.Errorf("Atoi(%q) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, testErr);
            }
        }
    }

}

internal static error bitSizeErrStub(@string name, nint bitSize) {
    return new strconv.NumErrorжerror(strconv_internal_test_package.BitSizeError(name, "0"u8, bitSize));
}

internal static error baseErrStub(@string name, nint @base) {
    return new strconv.NumErrorжerror(strconv_internal_test_package.BaseError(name, "0"u8, @base));
}

internal static error noErrStub(@string name, nint arg) {
    return default!;
}

partial struct parseErrorTest {
    internal nint arg;
    internal Func<@string, nint, error> errStub;
}

internal static ж<slice<parseErrorTest>> ᏑparseBitSizeTests = new StandardBox<slice<parseErrorTest>>(new parseErrorTest[]{
    new(-1, bitSizeErrStub),
    new(0, noErrStub),
    new(64, noErrStub),
    new(65, bitSizeErrStub)
}.slice());
internal static ref slice<parseErrorTest> parseBitSizeTests => ref ᏑparseBitSizeTests.ValueSlot;

internal static ж<slice<parseErrorTest>> ᏑparseBaseTests = new StandardBox<slice<parseErrorTest>>(new parseErrorTest[]{
    new(-1, baseErrStub),
    new(0, noErrStub),
    new(1, baseErrStub),
    new(2, noErrStub),
    new(36, noErrStub),
    new(37, baseErrStub)
}.slice());
internal static ref slice<parseErrorTest> parseBaseTests => ref ᏑparseBaseTests.ValueSlot;

internal static bool equalError(error a, error b) {
    if (a == default!) {
        return b == default!;
    }
    if (b == default!) {
        return a == default!;
    }
    return a.Error() == b.Error();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseIntˢ = "ParseInt"u8;

public static void TestParseIntBitSize(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseBitSizeTests) {
        var test = Ꮡ(parseBitSizeTests, i);
        var testErr = (~test).errStub(parseIntˢ, (~test).arg);
        var (_, err) = ParseInt("0"u8, 0, (~test).arg);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseInt(\"0\", 0, %v) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseUintˢ = "ParseUint"u8;

public static void TestParseUintBitSize(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseBitSizeTests) {
        var test = Ꮡ(parseBitSizeTests, i);
        var testErr = (~test).errStub(parseUintˢ, (~test).arg);
        var (_, err) = ParseUint("0"u8, 0, (~test).arg);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseUint(\"0\", 0, %v) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

public static void TestParseIntBase(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseBaseTests) {
        var test = Ꮡ(parseBaseTests, i);
        var testErr = (~test).errStub(parseIntˢ, (~test).arg);
        var (_, err) = ParseInt("0"u8, (~test).arg, 0);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseInt(\"0\", %v, 0) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

public static void TestParseUintBase(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in parseBaseTests) {
        var test = Ꮡ(parseBaseTests, i);
        var testErr = (~test).errStub(parseUintˢ, (~test).arg);
        var (_, err) = ParseUint("0"u8, (~test).arg, 0);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseUint(\"0\", %v, 0) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedˢ = "failed"u8;

public static void TestNumError(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in numErrorTests) {
        var err = Ꮡ(new NumError(
            Func: "ParseFloat"u8,
            Num: test.num,
            Err: errors.New(failedˢ)
        ));
        {
            @string got = err.Error(); if (got != test.want) {
                Ꮡt.Errorf(@"(&NumError{""ParseFloat"", %q, ""failed""}).Error() = %v, want %v"u8, test.num, got, test.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorsIsFailedWantedˢ = (@string)"errors.Is failed, wanted success"u8;

public static void TestNumErrorUnwrap(ж<testing.T> Ꮡt) {
    var err = Ꮡ(new NumError(Err: ErrSyntax));
    if (!errors.Is(new strconv.NumErrorжerror(err), ErrSyntax)) {
        Ꮡt.Error(errorsIsFailedWantedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string posˢ = "Pos"u8;
internal static readonly @string negˢ = "Neg"u8;

public static void BenchmarkParseInt(ж<testing.B> Ꮡb) {
    Ꮡb.Run(posˢ, (ж<testing.B> bΔ1) => {
        benchmarkParseInt(bΔ1, 1);
    });
    Ꮡb.Run(negˢ, (ж<testing.B> bΔ2) => {
        benchmarkParseInt(bΔ2, -1);
    });
}

partial struct benchCase {
    internal @string name;
    internal int64 num;
}

internal static void benchmarkParseInt(ж<testing.B> Ꮡb, nint neg) {
    var cases = new benchCase[]{
        new("7bit"u8, (1 << (int)(7)) - 1),
        new("26bit"u8, (1 << (int)(26)) - 1),
        new("31bit"u8, 2147483648L - 1),
        new("56bit"u8, 72057594037927935L),
        new("63bit"u8, 9223372036854775807L)
    }.slice();
    foreach (var (_, vᴛ1) in cases) {
        ref var cs = ref heap(new benchCase(), out var Ꮡcs);
        cs = vᴛ1;

        var csʗ1 = cs;
        Ꮡb.Run(cs.name, (ж<testing.B> bΔ1) => {
            @string s = fmt.Sprintf("%d"u8, csʗ1.num * (int64)neg);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (@out, _) = ParseInt(s, 10, 64);
                BenchSink += (nint)@out;
            }
        });
    }
}

public static void BenchmarkAtoi(ж<testing.B> Ꮡb) {
    Ꮡb.Run(posˢ, (ж<testing.B> bΔ1) => {
        benchmarkAtoi(bΔ1, 1);
    });
    Ꮡb.Run(negˢ, (ж<testing.B> bΔ2) => {
        benchmarkAtoi(bΔ2, -1);
    });
}

internal static void benchmarkAtoi(ж<testing.B> Ꮡb, nint neg) {
    var cases = new benchCase[]{
        new("7bit"u8, (1 << (int)(7)) - 1),
        new("26bit"u8, (1 << (int)(26)) - 1),
        new("31bit"u8, 2147483648L - 1)
    }.slice();
    if (IntSize == 64) {
        cases = appendꓸꓸꓸ(cases, new benchCase[]{
            new("56bit"u8, 72057594037927935L),
            new("63bit"u8, 9223372036854775807L)
        }.slice());
    }
    foreach (var (_, vᴛ1) in cases) {
        ref var cs = ref heap(new benchCase(), out var Ꮡcs);
        cs = vᴛ1;

        var csʗ1 = cs;
        Ꮡb.Run(cs.name, (ж<testing.B> bΔ1) => {
            @string s = fmt.Sprintf("%d"u8, csʗ1.num * (int64)neg);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (@out, _) = Atoi(s);
                BenchSink += @out;
            }
        });
    }
}

} // end strconv_test_package
