// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using hex = go.encoding.hex_package;
using errors = errors_package;
using fmt = fmt_package;
using math = math_package;
using big = go.math.big_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.encoding;
using go.math;
using static go.encoding.asn1_package;

partial class asn1_internal_test_package {

[GoType] internal partial struct boolTest {
    internal slice<byte> @in;
    internal bool ok;
    internal bool @out;
}

internal static slice<boolTest> boolTestData = new boolTest[]{
    new(new byte[]{0x00}.slice(), true, false),
    new(new byte[]{0xff}.slice(), true, true),
    new(new byte[]{0x00, 0x00}.slice(), false, false),
    new(new byte[]{0xff, 0xff}.slice(), false, false),
    new(new byte[]{0x01}.slice(), false, false)
}.slice();

public static void TestParseBool(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in boolTestData) {
        var (ret, err) = parseBool(test.@in);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (test.ok && ret != test.@out) {
            Ꮡt.Errorf("#%d: Bad result: %v (expected %v)"u8, i, ret, test.@out);
        }
    }
}

[GoType] internal partial struct int64Test {
    internal slice<byte> @in;
    internal bool ok;
    internal int64 @out;
}

internal static slice<int64Test> int64TestData = new int64Test[]{
    new(new byte[]{0x00}.slice(), true, 0),
    new(new byte[]{0x7f}.slice(), true, 127),
    new(new byte[]{0x00, 0x80}.slice(), true, 128),
    new(new byte[]{0x01, 0x00}.slice(), true, 256),
    new(new byte[]{0x80}.slice(), true, -128),
    new(new byte[]{0xff, 0x7f}.slice(), true, -129),
    new(new byte[]{0xff}.slice(), true, -1),
    new(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.slice(), true, -9223372036854775808L),
    new(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.slice(), false, 0),
    new(new byte[]{}.slice(), false, 0),
    new(new byte[]{0x00, 0x7f}.slice(), false, 0),
    new(new byte[]{0xff, 0xf0}.slice(), false, 0)
}.slice();

public static void TestParseInt64(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in int64TestData) {
        var (ret, err) = parseInt64(test.@in);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (test.ok && ret != test.@out) {
            Ꮡt.Errorf("#%d: Bad result: %v (expected %v)"u8, i, ret, test.@out);
        }
    }
}

[GoType] internal partial struct int32Test {
    internal slice<byte> @in;
    internal bool ok;
    internal int32 @out;
}

internal static slice<int32Test> int32TestData = new int32Test[]{
    new(new byte[]{0x00}.slice(), true, 0),
    new(new byte[]{0x7f}.slice(), true, 127),
    new(new byte[]{0x00, 0x80}.slice(), true, 128),
    new(new byte[]{0x01, 0x00}.slice(), true, 256),
    new(new byte[]{0x80}.slice(), true, -128),
    new(new byte[]{0xff, 0x7f}.slice(), true, -129),
    new(new byte[]{0xff}.slice(), true, -1),
    new(new byte[]{0x80, 0x00, 0x00, 0x00}.slice(), true, -2147483648),
    new(new byte[]{0x80, 0x00, 0x00, 0x00, 0x00}.slice(), false, 0),
    new(new byte[]{}.slice(), false, 0),
    new(new byte[]{0x00, 0x7f}.slice(), false, 0),
    new(new byte[]{0xff, 0xf0}.slice(), false, 0)
}.slice();

public static void TestParseInt32(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in int32TestData) {
        var (ret, err) = parseInt32(test.@in);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (test.ok && ret != test.@out) {
            Ꮡt.Errorf("#%d: Bad result: %v (expected %v)"u8, i, ret, test.@out);
        }
    }
}


[GoType("dyn")] partial struct bigIntTestsᴛ1 {
    internal slice<byte> @in;
    internal bool ok;
    internal @string base10;
}
internal static slice<bigIntTestsᴛ1> bigIntTests = new bigIntTestsᴛ1[]{
    new(new byte[]{0xff}.slice(), true, "-1"u8),
    new(new byte[]{0x00}.slice(), true, "0"u8),
    new(new byte[]{0x01}.slice(), true, "1"u8),
    new(new byte[]{0x00, 0xff}.slice(), true, "255"u8),
    new(new byte[]{0xff, 0x00}.slice(), true, "-256"u8),
    new(new byte[]{0x01, 0x00}.slice(), true, "256"u8),
    new(new byte[]{}.slice(), false, ""u8),
    new(new byte[]{0x00, 0x7f}.slice(), false, ""u8),
    new(new byte[]{0xff, 0xf0}.slice(), false, ""u8)
}.slice();

public static void TestParseBigInt(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in bigIntTests) {
        var (ret, err) = parseBigInt(test.@in);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (test.ok) {
            if (ret.String() != test.base10) {
                Ꮡt.Errorf("#%d: bad result from %x, got %s want %s"u8, i, test.@in, ret.String(), test.base10);
            }
            var (e, errΔ1) = makeBigInt(ret);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("%d: err=%q"u8, i, errΔ1);
                continue;
            }
            var result = new slice<byte>(e.Len());
            e.Encode(result);
            if (!bytes.Equal(result, test.@in)) {
                Ꮡt.Errorf("#%d: got %x from marshaling %s, want %x"u8, i, result, ret.OrTypedNil(), test.@in);
            }
        }
    }
}

[GoType] internal partial struct bitStringTest {
    internal slice<byte> @in;
    internal bool ok;
    internal slice<byte> @out;
    internal nint bitLength;
}

internal static slice<bitStringTest> bitStringTestData = new bitStringTest[]{
    new(new byte[]{}.slice(), false, new byte[]{}.slice(), 0),
    new(new byte[]{0x00}.slice(), true, new byte[]{}.slice(), 0),
    new(new byte[]{0x07, 0x00}.slice(), true, new byte[]{0x00}.slice(), 1),
    new(new byte[]{0x07, 0x01}.slice(), false, new byte[]{}.slice(), 0),
    new(new byte[]{0x07, 0x40}.slice(), false, new byte[]{}.slice(), 0),
    new(new byte[]{0x08, 0x00}.slice(), false, new byte[]{}.slice(), 0)
}.slice();

public static void TestBitString(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in bitStringTestData) {
        var (ret, err) = parseBitString(test.@in);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (err == default!) {
            if (test.bitLength != ret.BitLength || !bytes.Equal(ret.Bytes, test.@out)) {
                Ꮡt.Errorf("#%d: Bad result: %v (expected %v %v)"u8, i, ret, test.@out, test.bitLength);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedˢ = (@string)"#1: Failed"u8;
internal static readonly object failedˢ2 = (@string)"#2: Failed"u8;
internal static readonly object failedˢ3 = (@string)"#3: Failed"u8;
internal static readonly object failedˢ4 = (@string)"#4: Failed"u8;
internal static readonly object failedˢ5 = (@string)"#5: Failed"u8;
internal static readonly object failedˢ6 = (@string)"#6: Failed"u8;

public static void TestBitStringAt(ж<testing.T> Ꮡt) {
    var bs = new BitString(new byte[]{0x82, 0x40}.slice(), 16);
    if (bs.At(0) != 1) {
        Ꮡt.Error(failedˢ);
    }
    if (bs.At(1) != 0) {
        Ꮡt.Error(failedˢ2);
    }
    if (bs.At(6) != 1) {
        Ꮡt.Error(failedˢ3);
    }
    if (bs.At(9) != 1) {
        Ꮡt.Error(failedˢ4);
    }
    if (bs.At(-1) != 0) {
        Ꮡt.Error(failedˢ5);
    }
    if (bs.At(17) != 0) {
        Ꮡt.Error(failedˢ6);
    }
}

[GoType] internal partial struct bitStringRightAlignTest {
    internal slice<byte> @in;
    internal nint inlen;
    internal slice<byte> @out;
}

internal static slice<bitStringRightAlignTest> bitStringRightAlignTests = new bitStringRightAlignTest[]{
    new(new byte[]{0x80}.slice(), 1, new byte[]{0x01}.slice()),
    new(new byte[]{0x80, 0x80}.slice(), 9, new byte[]{0x01, 0x01}.slice()),
    new(new byte[]{}.slice(), 0, new byte[]{}.slice()),
    new(new byte[]{0xce}.slice(), 8, new byte[]{0xce}.slice()),
    new(new byte[]{0xce, 0x47}.slice(), 16, new byte[]{0xce, 0x47}.slice()),
    new(new byte[]{0x34, 0x50}.slice(), 12, new byte[]{0x03, 0x45}.slice())
}.slice();

public static void TestBitStringRightAlign(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in bitStringRightAlignTests) {
        var bs = new BitString(test.@in, test.inlen);
        var @out = bs.RightAlign();
        if (!bytes.Equal(@out, test.@out)) {
            Ꮡt.Errorf("#%d got: %x want: %x"u8, i, @out, test.@out);
        }
    }
}

[GoType] internal partial struct objectIdentifierTest {
    internal slice<byte> @in;
    internal bool ok;
    internal global::go.encoding.asn1_package.ObjectIdentifier @out; // has base type[]int
}

internal static slice<objectIdentifierTest> objectIdentifierTestData = new objectIdentifierTest[]{
    new(new byte[]{}.slice(), false, new nint[]{}.slice()),
    new(new byte[]{85}.slice(), true, new nint[]{2, 5}.slice()),
    new(new byte[]{85, 0x02}.slice(), true, new nint[]{2, 5, 2}.slice()),
    new(new byte[]{85, 0x02, 0xc0, 0x00}.slice(), true, new nint[]{2, 5, 2, 0x2000}.slice()),
    new(new byte[]{0x81, 0x34, 0x03}.slice(), true, new nint[]{2, 100, 3}.slice()),
    new(new byte[]{85, 0x02, 0xc0, 0x80, 0x80, 0x80, 0x80}.slice(), false, new nint[]{}.slice())
}.slice();

public static void TestObjectIdentifier(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in objectIdentifierTestData) {
        var (ret, err) = parseObjectIdentifier(test.@in);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (err == default!) {
            if (!reflect.DeepEqual(test.@out, ret)) {
                Ꮡt.Errorf("#%d: Bad result: %v (expected %v)"u8, i, ret, test.@out);
            }
        }
    }
    {
        @string s = ((global::go.encoding.asn1_package.ObjectIdentifier)new nint[]{1, 2, 3, 4}.slice()).String(); if (s != "1.2.3.4"u8) {
            Ꮡt.Errorf("bad ObjectIdentifier.String(). Got %s, want 1.2.3.4"u8, s);
        }
    }
}

[GoType] internal partial struct timeTest {
    internal @string @in;
    internal bool ok;
    internal time.Time @out;
}

/* These are invalid times. However, the time package normalises times
	 * and they were accepted in some versions. See #11134. */
internal static slice<timeTest> utcTestData = new timeTest[]{
    new("910506164540-0700"u8, true, time.Date(1991, 5, 6, 16, 45, 40, 0, time.FixedZone(""u8, -7 * 60 * 60))),
    new("910506164540+0730"u8, true, time.Date(1991, 5, 6, 16, 45, 40, 0, time.FixedZone(""u8, 7 * 60 * 60 + 30 * 60))),
    new("910506234540Z"u8, true, time.Date(1991, 5, 6, 23, 45, 40, 0, time.ΔUTC)),
    new("9105062345Z"u8, true, time.Date(1991, 5, 6, 23, 45, 0, 0, time.ΔUTC)),
    new("5105062345Z"u8, true, time.Date(1951, 5, 6, 23, 45, 0, 0, time.ΔUTC)),
    new("a10506234540Z"u8, false, new time.Time(nil)),
    new("91a506234540Z"u8, false, new time.Time(nil)),
    new("9105a6234540Z"u8, false, new time.Time(nil)),
    new("910506a34540Z"u8, false, new time.Time(nil)),
    new("910506334a40Z"u8, false, new time.Time(nil)),
    new("91050633444aZ"u8, false, new time.Time(nil)),
    new("910506334461Z"u8, false, new time.Time(nil)),
    new("910506334400Za"u8, false, new time.Time(nil)),
    new("000100000000Z"u8, false, new time.Time(nil)),
    new("101302030405Z"u8, false, new time.Time(nil)),
    new("100002030405Z"u8, false, new time.Time(nil)),
    new("100100030405Z"u8, false, new time.Time(nil)),
    new("100132030405Z"u8, false, new time.Time(nil)),
    new("100231030405Z"u8, false, new time.Time(nil)),
    new("100102240405Z"u8, false, new time.Time(nil)),
    new("100102036005Z"u8, false, new time.Time(nil)),
    new("100102030460Z"u8, false, new time.Time(nil)),
    new("-100102030410Z"u8, false, new time.Time(nil)),
    new("10-0102030410Z"u8, false, new time.Time(nil)),
    new("10-0002030410Z"u8, false, new time.Time(nil)),
    new("1001-02030410Z"u8, false, new time.Time(nil)),
    new("100102-030410Z"u8, false, new time.Time(nil)),
    new("10010203-0410Z"u8, false, new time.Time(nil)),
    new("1001020304-10Z"u8, false, new time.Time(nil))
}.slice();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string formatᶜ = "Jan _2 15:04:05 -0700 2006"u8;

public static void TestUTCTime(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in utcTestData) {
        var (ret, err) = parseUTCTime(slice<byte>(test.@in));
        if (err != default!) {
            if (test.ok) {
                Ꮡt.Errorf("#%d: parseUTCTime(%q) = error %v"u8, i, test.@in, err);
            }
            continue;
        }
        if (!test.ok) {
            Ꮡt.Errorf("#%d: parseUTCTime(%q) succeeded, should have failed"u8, i, test.@in);
            continue;
        }
        @string format = formatᶜ;            // ignore zone name, just offset
        @string have = ret.Format(format);
        @string want = test.@out.Format(format);
        if (have != want) {
            Ꮡt.Errorf("#%d: parseUTCTime(%q) = %s, want %s"u8, i, test.@in, have, want);
        }
    }
}

/* These are invalid times. However, the time package normalises times
	 * and they were accepted in some versions. See #11134. */
internal static slice<timeTest> generalizedTimeTestData = new timeTest[]{
    new("20100102030405Z"u8, true, time.Date(2010, 1, 2, 3, 4, 5, 0, time.ΔUTC)),
    new("20100102030405"u8, false, new time.Time(nil)),
    new("20100102030405.123456Z"u8, true, time.Date(2010, 1, 2, 3, 4, 5, 123456000, time.ΔUTC)),
    new("20100102030405.123456"u8, false, new time.Time(nil)),
    new("20100102030405.Z"u8, false, new time.Time(nil)),
    new("20100102030405."u8, false, new time.Time(nil)),
    new("20100102030405+0607"u8, true, time.Date(2010, 1, 2, 3, 4, 5, 0, time.FixedZone(""u8, 6 * 60 * 60 + 7 * 60))),
    new("20100102030405-0607"u8, true, time.Date(2010, 1, 2, 3, 4, 5, 0, time.FixedZone(""u8, -6 * 60 * 60 - 7 * 60))),
    new("00000100000000Z"u8, false, new time.Time(nil)),
    new("20101302030405Z"u8, false, new time.Time(nil)),
    new("20100002030405Z"u8, false, new time.Time(nil)),
    new("20100100030405Z"u8, false, new time.Time(nil)),
    new("20100132030405Z"u8, false, new time.Time(nil)),
    new("20100231030405Z"u8, false, new time.Time(nil)),
    new("20100102240405Z"u8, false, new time.Time(nil)),
    new("20100102036005Z"u8, false, new time.Time(nil)),
    new("20100102030460Z"u8, false, new time.Time(nil)),
    new("-20100102030410Z"u8, false, new time.Time(nil)),
    new("2010-0102030410Z"u8, false, new time.Time(nil)),
    new("2010-0002030410Z"u8, false, new time.Time(nil)),
    new("201001-02030410Z"u8, false, new time.Time(nil)),
    new("20100102-030410Z"u8, false, new time.Time(nil)),
    new("2010010203-0410Z"u8, false, new time.Time(nil)),
    new("201001020304-10Z"u8, false, new time.Time(nil))
}.slice();

public static void TestGeneralizedTime(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in generalizedTimeTestData) {
        var (ret, err) = parseGeneralizedTime(slice<byte>(test.@in));
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (err == default!) {
            if (!reflect.DeepEqual(test.@out, ret)) {
                Ꮡt.Errorf("#%d: Bad result: %q → %v (expected %v)"u8, i, test.@in, ret, test.@out);
            }
        }
    }
}

[GoType] internal partial struct tagAndLengthTest {
    internal slice<byte> @in;
    internal bool ok;
    internal global::go.encoding.asn1_package.tagAndLength @out;
}

// Superfluous zeros in the length should be an error.
// Lengths up to the maximum size of an int should work.
// Lengths that would overflow an int should be rejected.
// Long length form may not be used for lengths that fit in short form.
// Tag numbers which would overflow int32 are rejected. (The value below is 2^31.)
// Tag numbers that fit in an int32 are valid. (The value below is 2^31 - 1.)
// Long tag number form may not be used for tags that fit in short form.
internal static slice<tagAndLengthTest> tagAndLengthData = new tagAndLengthTest[]{
    new(new byte[]{0x80, 0x01}.slice(), true, new tagAndLength(2, 0, 1, false)),
    new(new byte[]{0xa0, 0x01}.slice(), true, new tagAndLength(2, 0, 1, true)),
    new(new byte[]{0x02, 0x00}.slice(), true, new tagAndLength(0, 2, 0, false)),
    new(new byte[]{0xfe, 0x00}.slice(), true, new tagAndLength(3, 30, 0, true)),
    new(new byte[]{0x1f, 0x1f, 0x00}.slice(), true, new tagAndLength(0, 31, 0, false)),
    new(new byte[]{0x1f, 0x81, 0x00, 0x00}.slice(), true, new tagAndLength(0, 128, 0, false)),
    new(new byte[]{0x1f, 0x81, 0x80, 0x01, 0x00}.slice(), true, new tagAndLength(0, 0x4001, 0, false)),
    new(new byte[]{0x00, 0x81, 0x80}.slice(), true, new tagAndLength(0, 0, 128, false)),
    new(new byte[]{0x00, 0x82, 0x01, 0x00}.slice(), true, new tagAndLength(0, 0, 256, false)),
    new(new byte[]{0x00, 0x83, 0x01, 0x00}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0x1f, 0x85}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0x30, 0x80}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0xa0, 0x82, 0x00, 0xff}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0xa0, 0x84, 0x7f, 0xff, 0xff, 0xff}.slice(), true, new tagAndLength(2, 0, 0x7fffffff, true)),
    new(new byte[]{0xa0, 0x84, 0x80, 0x00, 0x00, 0x00}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0xa0, 0x81, 0x7f}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0x1f, 0x88, 0x80, 0x80, 0x80, 0x00, 0x00}.slice(), false, new tagAndLength(nil)),
    new(new byte[]{0x1f, 0x87, 0xFF, 0xFF, 0xFF, 0x7F, 0x00}.slice(), true, new tagAndLength(tag: math.MaxInt32)),
    new(new byte[]{0x1f, 0x1e, 0x00}.slice(), false, new tagAndLength(nil))
}.slice();

public static void TestParseTagAndLength(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in tagAndLengthData) {
        var (tagAndLength, _, err) = parseTagAndLength(test.@in, 0);
        if ((err == default!) != test.ok) {
            Ꮡt.Errorf("#%d: Incorrect error result (did pass? %v, expected: %v)"u8, i, err == default!, test.ok);
        }
        if (err == default! && !reflect.DeepEqual(test.@out, tagAndLength)) {
            Ꮡt.Errorf("#%d: Bad result: %v (expected %v)"u8, i, tagAndLength, test.@out);
        }
    }
}

[GoType] internal partial struct parseFieldParametersTest {
    internal @string @in;
    internal global::go.encoding.asn1_package.fieldParameters @out;
}

internal static ж<nint> newInt(nint nʗp) {
    ref var n = ref heap(nʗp, out var Ꮡn);

    return Ꮡn;
}

internal static ж<int64> newInt64(int64 nʗp) {
    ref var n = ref heap(nʗp, out var Ꮡn);

    return Ꮡn;
}

internal static ж<@string> newString(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    return Ꮡs;
}

internal static ж<bool> newBool(bool bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    return Ꮡb;
}

internal static slice<parseFieldParametersTest> parseFieldParametersTestData = new parseFieldParametersTest[]{
    new(""u8, new fieldParameters(nil)),
    new("ia5"u8, new fieldParameters(stringType: TagIA5String)),
    new("generalized"u8, new fieldParameters(timeType: TagGeneralizedTime)),
    new("utc"u8, new fieldParameters(timeType: TagUTCTime)),
    new("printable"u8, new fieldParameters(stringType: TagPrintableString)),
    new("numeric"u8, new fieldParameters(stringType: TagNumericString)),
    new("optional"u8, new fieldParameters(optional: true)),
    new("explicit"u8, new fieldParameters(@explicit: true, tag: @new<nint>())),
    new("application"u8, new fieldParameters(application: true, tag: @new<nint>())),
    new("private"u8, new fieldParameters(@private: true, tag: @new<nint>())),
    new("optional,explicit"u8, new fieldParameters(optional: true, @explicit: true, tag: @new<nint>())),
    new("default:42"u8, new fieldParameters(defaultValue: newInt64(42))),
    new("tag:17"u8, new fieldParameters(tag: newInt(17))),
    new("optional,explicit,default:42,tag:17"u8, new fieldParameters(optional: true, @explicit: true, defaultValue: newInt64(42), tag: newInt(17))),
    new("optional,explicit,default:42,tag:17,rubbish1"u8, new fieldParameters(optional: true, @explicit: true, application: false, defaultValue: newInt64(42), tag: newInt(17), stringType: 0, timeType: 0, set: false, omitEmpty: false)),
    new("set"u8, new fieldParameters(set: true))
}.slice();

public static void TestParseFieldParameters(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in parseFieldParametersTestData) {
        var f = parseFieldParameters(test.@in);
        if (!reflect.DeepEqual(f, test.@out)) {
            Ꮡt.Errorf("#%d: Bad result: %v (expected %v)"u8, i, f, test.@out);
        }
    }
}

[GoType] public partial struct TestObjectIdentifierStruct {
    public global::go.encoding.asn1_package.ObjectIdentifier OID;
}

[GoType] public partial struct TestContextSpecificTags {
    [GoTag(@"asn1:""tag:1""")]
    public nint A;
}

[GoType] public partial struct TestContextSpecificTags2 {
    [GoTag(@"asn1:""explicit,tag:1""")]
    public nint A;
    public nint B;
}

[GoType] public partial struct TestContextSpecificTags3 {
    [GoTag(@"asn1:""tag:1,utf8""")]
    public @string S;
}

[GoType] public partial struct TestElementsAfterString {
    public @string S;
    public nint A, B;
}

[GoType] public partial struct TestBigInt {
    public ж<bigꓸInt> X;
}

[GoType] public partial struct TestSet {
    [GoTag(@"asn1:""set""")]
    public slice<nint> Ints;
}

// Ampersand is allowed in PrintableString due to mistakes by major CAs.

[GoType("dyn")] partial struct unmarshalTestDataᴛ1 {
    internal slice<byte> @in;
    internal any @out;
}
internal static slice<unmarshalTestDataᴛ1> unmarshalTestData = new unmarshalTestDataᴛ1[]{
    new(new byte[]{0x02, 0x01, 0x42}.slice(), newInt(0x42).OrTypedNil()),
    new(new byte[]{0x05, 0x00}.slice(), Ꮡ(new RawValue(0, 5, false, new byte[]{}.slice(), new byte[]{0x05, 0x00}.slice()))),
    new(new byte[]{0x30, 0x08, 0x06, 0x06, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d}.slice(), Ꮡ(new TestObjectIdentifierStruct(new nint[]{1, 2, 840, 113549}.slice()))),
    new(new byte[]{0x03, 0x04, 0x06, 0x6e, 0x5d, 0xc0}.slice(), Ꮡ(new BitString(new byte[]{110, 93, 192}.slice(), 18))),
    new(new byte[]{0x30, 0x09, 0x02, 0x01, 0x01, 0x02, 0x01, 0x02, 0x02, 0x01, 0x03}.slice(), Ꮡ(new nint[]{1, 2, 3}.slice())),
    new(new byte[]{0x02, 0x01, 0x10}.slice(), newInt(16).OrTypedNil()),
    new(new byte[]{0x13, 0x04, (rune)'t', (rune)'e', (rune)'s', (rune)'t'}.slice(), newString("test"u8).OrTypedNil()),
    new(new byte[]{0x16, 0x04, (rune)'t', (rune)'e', (rune)'s', (rune)'t'}.slice(), newString("test"u8).OrTypedNil()),
    new(new byte[]{0x13, 0x05, (rune)'t', (rune)'e', (rune)'s', (rune)'t', (rune)'&'}.slice(), newString("test&"u8).OrTypedNil()),
    new(new byte[]{0x16, 0x04, (rune)'t', (rune)'e', (rune)'s', (rune)'t'}.slice(), Ꮡ(new RawValue(0, 22, false, slice<byte>("test"u8), slice<byte>("\x16\x04test"u8)))),
    new(new byte[]{0x04, 0x04, 1, 2, 3, 4}.slice(), Ꮡ(new RawValue(0, 4, false, new byte[]{1, 2, 3, 4}.slice(), new byte[]{4, 4, 1, 2, 3, 4}.slice()))),
    new(new byte[]{0x30, 0x03, 0x81, 0x01, 0x01}.slice(), Ꮡ(new TestContextSpecificTags(1))),
    new(new byte[]{0x30, 0x08, 0xa1, 0x03, 0x02, 0x01, 0x01, 0x02, 0x01, 0x02}.slice(), Ꮡ(new TestContextSpecificTags2(1, 2))),
    new(new byte[]{0x30, 0x03, 0x81, 0x01, (rune)'@'}.slice(), Ꮡ(new TestContextSpecificTags3("@"u8))),
    new(new byte[]{0x01, 0x01, 0x00}.slice(), newBool(false).OrTypedNil()),
    new(new byte[]{0x01, 0x01, 0xff}.slice(), newBool(true).OrTypedNil()),
    new(new byte[]{0x30, 0x0b, 0x13, 0x03, 0x66, 0x6f, 0x6f, 0x02, 0x01, 0x22, 0x02, 0x01, 0x33}.slice(), Ꮡ(new TestElementsAfterString("foo"u8, 0x22, 0x33))),
    new(new byte[]{0x30, 0x05, 0x02, 0x03, 0x12, 0x34, 0x56}.slice(), Ꮡ(new TestBigInt(big.NewInt(0x123456)))),
    new(new byte[]{0x30, 0x0b, 0x31, 0x09, 0x02, 0x01, 0x01, 0x02, 0x01, 0x02, 0x02, 0x01, 0x03}.slice(), Ꮡ(new TestSet(Ints: new nint[]{1, 2, 3}.slice()))),
    new(new byte[]{0x12, 0x0b, (rune)'0', (rune)'1', (rune)'2', (rune)'3', (rune)'4', (rune)'5', (rune)'6', (rune)'7', (rune)'8', (rune)'9', (rune)' '}.slice(), newString("0123456789 "u8).OrTypedNil())
}.slice();

public static void TestUnmarshal(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in unmarshalTestData) {
        var pv = reflect.New(reflect.TypeOf(test.@out).Elem());
        var val = pv.Interface();
        var (_, err) = Unmarshal(test.@in, val);
        if (err != default!) {
            Ꮡt.Errorf("Unmarshal failed at index %d %v"u8, i, err);
        }
        if (!reflect.DeepEqual(val, test.@out)) {
            Ꮡt.Errorf("#%d:\nhave %#v\nwant %#v"u8, i, val, test.@out);
        }
    }
}

[GoType("dyn")] internal partial struct TestUnmarshalWithNilOrNonPointer_tests {
    internal slice<byte> b;
    internal any v;
    internal @string want;
}

public static void TestUnmarshalWithNilOrNonPointer(ж<testing.T> Ꮡt) {
    var tests = new TestUnmarshalWithNilOrNonPointer_tests[]{
        new(b: new byte[]{0x05, 0x00}.slice(), v: default!, want: "asn1: Unmarshal recipient value is nil"u8),
        new(b: new byte[]{0x05, 0x00}.slice(), v: new RawValue(nil), want: "asn1: Unmarshal recipient value is non-pointer asn1.RawValue"u8),
        new(b: new byte[]{0x05, 0x00}.slice(), v: ((ж<global::go.encoding.asn1_package.RawValue>)nil), want: "asn1: Unmarshal recipient value is nil *asn1.RawValue"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        var (_, err) = Unmarshal(test.b, test.v);
        if (err == default!) {
            Ꮡt.Errorf("Unmarshal expecting error, got nil"u8);
            continue;
        }
        {
            @string g = err.Error();
            @string w = test.want; if (g != w) {
                Ꮡt.Errorf("InvalidUnmarshalError mismatch\nGot:  %q\nWant: %q"u8, g, w);
            }
        }
    }
}

[GoType] public partial struct Certificate {
    public TBSCertificate TBSCertificate;
    public AlgorithmIdentifier SignatureAlgorithm;
    public global::go.encoding.asn1_package.BitString SignatureValue;
}

[GoType] public partial struct TBSCertificate {
    [GoTag(@"asn1:""optional,explicit,default:0,tag:0""")]
    public nint Version;
    public global::go.encoding.asn1_package.RawValue SerialNumber;
    public AlgorithmIdentifier SignatureAlgorithm;
    public RDNSequence Issuer;
    public Validity Validity;
    public RDNSequence Subject;
    public PublicKeyInfo PublicKey;
}

[GoType] public partial struct AlgorithmIdentifier {
    public global::go.encoding.asn1_package.ObjectIdentifier Algorithm;
}

[GoType("[]RelativeDistinguishedNameSET")] public partial struct RDNSequence;

[GoType("[]AttributeTypeAndValue")] public partial struct RelativeDistinguishedNameSET;

[GoType] public partial struct AttributeTypeAndValue {
    public global::go.encoding.asn1_package.ObjectIdentifier Type;
    public any Value;
}

[GoType] public partial struct Validity {
    public time.Time NotBefore, NotAfter;
}

[GoType] public partial struct PublicKeyInfo {
    public AlgorithmIdentifier Algorithm;
    public global::go.encoding.asn1_package.BitString PublicKey;
}

public static void TestCertificate(ж<testing.T> Ꮡt) {
    // This is a minimal, self-signed certificate that should parse correctly.
    ref var cert = ref heap(new Certificate(), out var Ꮡcert);
    {
        var (_, err) = Unmarshal(derEncodedSelfSignedCertBytes, Ꮡcert); if (err != default!) {
            Ꮡt.Errorf("Unmarshal failed: %v"u8, err);
        }
    }
    if (!reflect.DeepEqual(cert, derEncodedSelfSignedCert)) {
        Ꮡt.Errorf("Bad result:\ngot: %+v\nwant: %+v"u8, cert, derEncodedSelfSignedCert);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalSucceededShouldˢ = (@string)"Unmarshal succeeded, should not have"u8;

public static void TestCertificateWithNUL(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This is the paypal NUL-hack certificate. It should fail to parse because
    // NUL isn't a permitted character in a PrintableString.
    ref var cert = ref heap(new Certificate(), out var Ꮡcert);
    {
        var (_, err) = Unmarshal(derEncodedPaypalNULCertBytes, Ꮡcert); if (err == default!) {
            Ꮡt.Error(unmarshalSucceededShouldˢ);
        }
    }
}

[GoType] internal partial struct rawStructTest {
    public global::go.encoding.asn1_package.RawContent Raw;
    public nint A;
}

public static void TestRawStructs(ж<testing.T> Ꮡt) {
    ref var s = ref heap(new rawStructTest(), out var Ꮡs);
    var input = new byte[]{0x30, 0x03, 0x02, 0x01, 0x50}.slice();
    var (rest, err) = Unmarshal(input, Ꮡs);
    if (len(rest) != 0) {
        Ꮡt.Errorf("incomplete parse: %x"u8, rest);
        return;
    }
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    if (s.A != 0x50) {
        Ꮡt.Errorf("bad value for A: got %d want %d"u8, s.A, (nint)(0x50));
    }
    if (!bytes.Equal(((slice<byte>)s.Raw), input)) {
        Ꮡt.Errorf("bad value for Raw: got %x want %x"u8, s.Raw, input);
    }
}

[GoType] internal partial struct oiEqualTest {
    internal global::go.encoding.asn1_package.ObjectIdentifier first;
    internal global::go.encoding.asn1_package.ObjectIdentifier second;
    internal bool same;
}

internal static slice<oiEqualTest> oiEqualTests = new oiEqualTest[]{
    new(
        new ObjectIdentifier(new nint[]{1, 2, 3}.slice()),
        new ObjectIdentifier(new nint[]{1, 2, 3}.slice()),
        true
    ),
    new(
        new ObjectIdentifier(new nint[]{1}.slice()),
        new ObjectIdentifier(new nint[]{1, 2, 3}.slice()),
        false
    ),
    new(
        new ObjectIdentifier(new nint[]{1, 2, 3}.slice()),
        new ObjectIdentifier(new nint[]{10, 11, 12}.slice()),
        false
    )
}.slice();

public static void TestObjectIdentifierEqual(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, o) in oiEqualTests) {
        {
            var s = o.first.Equal(o.second); if (s != o.same) {
                Ꮡt.Errorf("ObjectIdentifier.Equal: got: %t want: %t"u8, s, o.same);
            }
        }
    }
}

internal static Certificate derEncodedSelfSignedCert = new Certificate(
    TBSCertificate: new TBSCertificate(
        Version: 0,
        SerialNumber: new RawValue(Class: 0, Tag: 2, IsCompound: false, Bytes: new uint8[]{0x0, 0x8c, 0xc3, 0x37, 0x92, 0x10, 0xec, 0x2c, 0x98}.slice(), FullBytes: new byte[]{2, 9, 0x0, 0x8c, 0xc3, 0x37, 0x92, 0x10, 0xec, 0x2c, 0x98}.slice()),
        SignatureAlgorithm: new AlgorithmIdentifier(Algorithm: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 5}.slice())),
        Issuer: new RDNSequence(new RelativeDistinguishedNameSET[]{
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 6}.slice()), Value: (@string)"XX"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 8}.slice()), Value: (@string)"Some-State"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 7}.slice()), Value: (@string)"City"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 10}.slice()), Value: (@string)"Internet Widgits Pty Ltd"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 3}.slice()), Value: (@string)"false.example.com"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 9, 1}.slice()), Value: (@string)"false@example.com"u8)}.slice())
        }.slice()),
        Validity: new Validity(
            NotBefore: time.Date(2009, 10, 8, 0, 25, 53, 0, time.ΔUTC),
            NotAfter: time.Date(2010, 10, 8, 0, 25, 53, 0, time.ΔUTC)
        ),
        Subject: new RDNSequence(new RelativeDistinguishedNameSET[]{
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 6}.slice()), Value: (@string)"XX"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 8}.slice()), Value: (@string)"Some-State"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 7}.slice()), Value: (@string)"City"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 10}.slice()), Value: (@string)"Internet Widgits Pty Ltd"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 3}.slice()), Value: (@string)"false.example.com"u8)}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 9, 1}.slice()), Value: (@string)"false@example.com"u8)}.slice())
        }.slice()),
        PublicKey: new PublicKeyInfo(
            Algorithm: new AlgorithmIdentifier(Algorithm: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 1}.slice())),
            PublicKey: new BitString(
                Bytes: new uint8[]{
                    0x30, 0x48, 0x2, 0x41, 0x0, 0xcd, 0xb7,
                    0x63, 0x9c, 0x32, 0x78, 0xf0, 0x6, 0xaa, 0x27, 0x7f, 0x6e, 0xaf, 0x42,
                    0x90, 0x2b, 0x59, 0x2d, 0x8c, 0xbc, 0xbe, 0x38, 0xa1, 0xc9, 0x2b, 0xa4,
                    0x69, 0x5a, 0x33, 0x1b, 0x1d, 0xea, 0xde, 0xad, 0xd8, 0xe9, 0xa5, 0xc2,
                    0x7e, 0x8c, 0x4c, 0x2f, 0xd0, 0xa8, 0x88, 0x96, 0x57, 0x72, 0x2a, 0x4f,
                    0x2a, 0xf7, 0x58, 0x9c, 0xf2, 0xc7, 0x70, 0x45, 0xdc, 0x8f, 0xde, 0xec,
                    0x35, 0x7d, 0x2, 0x3, 0x1, 0x0, 0x1
                }.slice(),
                BitLength: 592
            )
        )
    ),
    SignatureAlgorithm: new AlgorithmIdentifier(Algorithm: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 5}.slice())),
    SignatureValue: new BitString(
        Bytes: new uint8[]{
            0xa6, 0x7b, 0x6, 0xec, 0x5e, 0xce,
            0x92, 0x77, 0x2c, 0xa4, 0x13, 0xcb, 0xa3, 0xca, 0x12, 0x56, 0x8f, 0xdc, 0x6c,
            0x7b, 0x45, 0x11, 0xcd, 0x40, 0xa7, 0xf6, 0x59, 0x98, 0x4, 0x2, 0xdf, 0x2b,
            0x99, 0x8b, 0xb9, 0xa4, 0xa8, 0xcb, 0xeb, 0x34, 0xc0, 0xf0, 0xa7, 0x8c, 0xf8,
            0xd9, 0x1e, 0xde, 0x14, 0xa5, 0xed, 0x76, 0xbf, 0x11, 0x6f, 0xe3, 0x60, 0xaa,
            0xfa, 0x88, 0x21, 0x49, 0x4, 0x35
        }.slice(),
        BitLength: 512
    )
);

internal static slice<byte> derEncodedSelfSignedCertBytes = new byte[]{
    0x30, 0x82, 0x02, 0x18, 0x30,
    0x82, 0x01, 0xc2, 0x02, 0x09, 0x00, 0x8c, 0xc3, 0x37, 0x92, 0x10, 0xec, 0x2c,
    0x98, 0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01,
    0x05, 0x05, 0x00, 0x30, 0x81, 0x92, 0x31, 0x0b, 0x30, 0x09, 0x06, 0x03, 0x55,
    0x04, 0x06, 0x13, 0x02, 0x58, 0x58, 0x31, 0x13, 0x30, 0x11, 0x06, 0x03, 0x55,
    0x04, 0x08, 0x13, 0x0a, 0x53, 0x6f, 0x6d, 0x65, 0x2d, 0x53, 0x74, 0x61, 0x74,
    0x65, 0x31, 0x0d, 0x30, 0x0b, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x04, 0x43,
    0x69, 0x74, 0x79, 0x31, 0x21, 0x30, 0x1f, 0x06, 0x03, 0x55, 0x04, 0x0a, 0x13,
    0x18, 0x49, 0x6e, 0x74, 0x65, 0x72, 0x6e, 0x65, 0x74, 0x20, 0x57, 0x69, 0x64,
    0x67, 0x69, 0x74, 0x73, 0x20, 0x50, 0x74, 0x79, 0x20, 0x4c, 0x74, 0x64, 0x31,
    0x1a, 0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x11, 0x66, 0x61, 0x6c,
    0x73, 0x65, 0x2e, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65, 0x2e, 0x63, 0x6f,
    0x6d, 0x31, 0x20, 0x30, 0x1e, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d,
    0x01, 0x09, 0x01, 0x16, 0x11, 0x66, 0x61, 0x6c, 0x73, 0x65, 0x40, 0x65, 0x78,
    0x61, 0x6d, 0x70, 0x6c, 0x65, 0x2e, 0x63, 0x6f, 0x6d, 0x30, 0x1e, 0x17, 0x0d,
    0x30, 0x39, 0x31, 0x30, 0x30, 0x38, 0x30, 0x30, 0x32, 0x35, 0x35, 0x33, 0x5a,
    0x17, 0x0d, 0x31, 0x30, 0x31, 0x30, 0x30, 0x38, 0x30, 0x30, 0x32, 0x35, 0x35,
    0x33, 0x5a, 0x30, 0x81, 0x92, 0x31, 0x0b, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04,
    0x06, 0x13, 0x02, 0x58, 0x58, 0x31, 0x13, 0x30, 0x11, 0x06, 0x03, 0x55, 0x04,
    0x08, 0x13, 0x0a, 0x53, 0x6f, 0x6d, 0x65, 0x2d, 0x53, 0x74, 0x61, 0x74, 0x65,
    0x31, 0x0d, 0x30, 0x0b, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x04, 0x43, 0x69,
    0x74, 0x79, 0x31, 0x21, 0x30, 0x1f, 0x06, 0x03, 0x55, 0x04, 0x0a, 0x13, 0x18,
    0x49, 0x6e, 0x74, 0x65, 0x72, 0x6e, 0x65, 0x74, 0x20, 0x57, 0x69, 0x64, 0x67,
    0x69, 0x74, 0x73, 0x20, 0x50, 0x74, 0x79, 0x20, 0x4c, 0x74, 0x64, 0x31, 0x1a,
    0x30, 0x18, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x11, 0x66, 0x61, 0x6c, 0x73,
    0x65, 0x2e, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65, 0x2e, 0x63, 0x6f, 0x6d,
    0x31, 0x20, 0x30, 0x1e, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01,
    0x09, 0x01, 0x16, 0x11, 0x66, 0x61, 0x6c, 0x73, 0x65, 0x40, 0x65, 0x78, 0x61,
    0x6d, 0x70, 0x6c, 0x65, 0x2e, 0x63, 0x6f, 0x6d, 0x30, 0x5c, 0x30, 0x0d, 0x06,
    0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03,
    0x4b, 0x00, 0x30, 0x48, 0x02, 0x41, 0x00, 0xcd, 0xb7, 0x63, 0x9c, 0x32, 0x78,
    0xf0, 0x06, 0xaa, 0x27, 0x7f, 0x6e, 0xaf, 0x42, 0x90, 0x2b, 0x59, 0x2d, 0x8c,
    0xbc, 0xbe, 0x38, 0xa1, 0xc9, 0x2b, 0xa4, 0x69, 0x5a, 0x33, 0x1b, 0x1d, 0xea,
    0xde, 0xad, 0xd8, 0xe9, 0xa5, 0xc2, 0x7e, 0x8c, 0x4c, 0x2f, 0xd0, 0xa8, 0x88,
    0x96, 0x57, 0x72, 0x2a, 0x4f, 0x2a, 0xf7, 0x58, 0x9c, 0xf2, 0xc7, 0x70, 0x45,
    0xdc, 0x8f, 0xde, 0xec, 0x35, 0x7d, 0x02, 0x03, 0x01, 0x00, 0x01, 0x30, 0x0d,
    0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05, 0x05, 0x00,
    0x03, 0x41, 0x00, 0xa6, 0x7b, 0x06, 0xec, 0x5e, 0xce, 0x92, 0x77, 0x2c, 0xa4,
    0x13, 0xcb, 0xa3, 0xca, 0x12, 0x56, 0x8f, 0xdc, 0x6c, 0x7b, 0x45, 0x11, 0xcd,
    0x40, 0xa7, 0xf6, 0x59, 0x98, 0x04, 0x02, 0xdf, 0x2b, 0x99, 0x8b, 0xb9, 0xa4,
    0xa8, 0xcb, 0xeb, 0x34, 0xc0, 0xf0, 0xa7, 0x8c, 0xf8, 0xd9, 0x1e, 0xde, 0x14,
    0xa5, 0xed, 0x76, 0xbf, 0x11, 0x6f, 0xe3, 0x60, 0xaa, 0xfa, 0x88, 0x21, 0x49,
    0x04, 0x35
}.slice();

internal static slice<byte> derEncodedPaypalNULCertBytes = new byte[]{
    0x30, 0x82, 0x06, 0x44, 0x30,
    0x82, 0x05, 0xad, 0xa0, 0x03, 0x02, 0x01, 0x02, 0x02, 0x03, 0x00, 0xf0, 0x9b,
    0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05,
    0x05, 0x00, 0x30, 0x82, 0x01, 0x12, 0x31, 0x0b, 0x30, 0x09, 0x06, 0x03, 0x55,
    0x04, 0x06, 0x13, 0x02, 0x45, 0x53, 0x31, 0x12, 0x30, 0x10, 0x06, 0x03, 0x55,
    0x04, 0x08, 0x13, 0x09, 0x42, 0x61, 0x72, 0x63, 0x65, 0x6c, 0x6f, 0x6e, 0x61,
    0x31, 0x12, 0x30, 0x10, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x09, 0x42, 0x61,
    0x72, 0x63, 0x65, 0x6c, 0x6f, 0x6e, 0x61, 0x31, 0x29, 0x30, 0x27, 0x06, 0x03,
    0x55, 0x04, 0x0a, 0x13, 0x20, 0x49, 0x50, 0x53, 0x20, 0x43, 0x65, 0x72, 0x74,
    0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x41, 0x75, 0x74,
    0x68, 0x6f, 0x72, 0x69, 0x74, 0x79, 0x20, 0x73, 0x2e, 0x6c, 0x2e, 0x31, 0x2e,
    0x30, 0x2c, 0x06, 0x03, 0x55, 0x04, 0x0a, 0x14, 0x25, 0x67, 0x65, 0x6e, 0x65,
    0x72, 0x61, 0x6c, 0x40, 0x69, 0x70, 0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d,
    0x20, 0x43, 0x2e, 0x49, 0x2e, 0x46, 0x2e, 0x20, 0x20, 0x42, 0x2d, 0x42, 0x36,
    0x32, 0x32, 0x31, 0x30, 0x36, 0x39, 0x35, 0x31, 0x2e, 0x30, 0x2c, 0x06, 0x03,
    0x55, 0x04, 0x0b, 0x13, 0x25, 0x69, 0x70, 0x73, 0x43, 0x41, 0x20, 0x43, 0x4c,
    0x41, 0x53, 0x45, 0x41, 0x31, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69,
    0x63, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x41, 0x75, 0x74, 0x68, 0x6f, 0x72,
    0x69, 0x74, 0x79, 0x31, 0x2e, 0x30, 0x2c, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13,
    0x25, 0x69, 0x70, 0x73, 0x43, 0x41, 0x20, 0x43, 0x4c, 0x41, 0x53, 0x45, 0x41,
    0x31, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74, 0x69,
    0x6f, 0x6e, 0x20, 0x41, 0x75, 0x74, 0x68, 0x6f, 0x72, 0x69, 0x74, 0x79, 0x31,
    0x20, 0x30, 0x1e, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x09,
    0x01, 0x16, 0x11, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x61, 0x6c, 0x40, 0x69, 0x70,
    0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x30, 0x1e, 0x17, 0x0d, 0x30, 0x39,
    0x30, 0x32, 0x32, 0x34, 0x32, 0x33, 0x30, 0x34, 0x31, 0x37, 0x5a, 0x17, 0x0d,
    0x31, 0x31, 0x30, 0x32, 0x32, 0x34, 0x32, 0x33, 0x30, 0x34, 0x31, 0x37, 0x5a,
    0x30, 0x81, 0x94, 0x31, 0x0b, 0x30, 0x09, 0x06, 0x03, 0x55, 0x04, 0x06, 0x13,
    0x02, 0x55, 0x53, 0x31, 0x13, 0x30, 0x11, 0x06, 0x03, 0x55, 0x04, 0x08, 0x13,
    0x0a, 0x43, 0x61, 0x6c, 0x69, 0x66, 0x6f, 0x72, 0x6e, 0x69, 0x61, 0x31, 0x16,
    0x30, 0x14, 0x06, 0x03, 0x55, 0x04, 0x07, 0x13, 0x0d, 0x53, 0x61, 0x6e, 0x20,
    0x46, 0x72, 0x61, 0x6e, 0x63, 0x69, 0x73, 0x63, 0x6f, 0x31, 0x11, 0x30, 0x0f,
    0x06, 0x03, 0x55, 0x04, 0x0a, 0x13, 0x08, 0x53, 0x65, 0x63, 0x75, 0x72, 0x69,
    0x74, 0x79, 0x31, 0x14, 0x30, 0x12, 0x06, 0x03, 0x55, 0x04, 0x0b, 0x13, 0x0b,
    0x53, 0x65, 0x63, 0x75, 0x72, 0x65, 0x20, 0x55, 0x6e, 0x69, 0x74, 0x31, 0x2f,
    0x30, 0x2d, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x26, 0x77, 0x77, 0x77, 0x2e,
    0x70, 0x61, 0x79, 0x70, 0x61, 0x6c, 0x2e, 0x63, 0x6f, 0x6d, 0x00, 0x73, 0x73,
    0x6c, 0x2e, 0x73, 0x65, 0x63, 0x75, 0x72, 0x65, 0x63, 0x6f, 0x6e, 0x6e, 0x65,
    0x63, 0x74, 0x69, 0x6f, 0x6e, 0x2e, 0x63, 0x63, 0x30, 0x81, 0x9f, 0x30, 0x0d,
    0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00,
    0x03, 0x81, 0x8d, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xd2, 0x69,
    0xfa, 0x6f, 0x3a, 0x00, 0xb4, 0x21, 0x1b, 0xc8, 0xb1, 0x02, 0xd7, 0x3f, 0x19,
    0xb2, 0xc4, 0x6d, 0xb4, 0x54, 0xf8, 0x8b, 0x8a, 0xcc, 0xdb, 0x72, 0xc2, 0x9e,
    0x3c, 0x60, 0xb9, 0xc6, 0x91, 0x3d, 0x82, 0xb7, 0x7d, 0x99, 0xff, 0xd1, 0x29,
    0x84, 0xc1, 0x73, 0x53, 0x9c, 0x82, 0xdd, 0xfc, 0x24, 0x8c, 0x77, 0xd5, 0x41,
    0xf3, 0xe8, 0x1e, 0x42, 0xa1, 0xad, 0x2d, 0x9e, 0xff, 0x5b, 0x10, 0x26, 0xce,
    0x9d, 0x57, 0x17, 0x73, 0x16, 0x23, 0x38, 0xc8, 0xd6, 0xf1, 0xba, 0xa3, 0x96,
    0x5b, 0x16, 0x67, 0x4a, 0x4f, 0x73, 0x97, 0x3a, 0x4d, 0x14, 0xa4, 0xf4, 0xe2,
    0x3f, 0x8b, 0x05, 0x83, 0x42, 0xd1, 0xd0, 0xdc, 0x2f, 0x7a, 0xe5, 0xb6, 0x10,
    0xb2, 0x11, 0xc0, 0xdc, 0x21, 0x2a, 0x90, 0xff, 0xae, 0x97, 0x71, 0x5a, 0x49,
    0x81, 0xac, 0x40, 0xf3, 0x3b, 0xb8, 0x59, 0xb2, 0x4f, 0x02, 0x03, 0x01, 0x00,
    0x01, 0xa3, 0x82, 0x03, 0x21, 0x30, 0x82, 0x03, 0x1d, 0x30, 0x09, 0x06, 0x03,
    0x55, 0x1d, 0x13, 0x04, 0x02, 0x30, 0x00, 0x30, 0x11, 0x06, 0x09, 0x60, 0x86,
    0x48, 0x01, 0x86, 0xf8, 0x42, 0x01, 0x01, 0x04, 0x04, 0x03, 0x02, 0x06, 0x40,
    0x30, 0x0b, 0x06, 0x03, 0x55, 0x1d, 0x0f, 0x04, 0x04, 0x03, 0x02, 0x03, 0xf8,
    0x30, 0x13, 0x06, 0x03, 0x55, 0x1d, 0x25, 0x04, 0x0c, 0x30, 0x0a, 0x06, 0x08,
    0x2b, 0x06, 0x01, 0x05, 0x05, 0x07, 0x03, 0x01, 0x30, 0x1d, 0x06, 0x03, 0x55,
    0x1d, 0x0e, 0x04, 0x16, 0x04, 0x14, 0x61, 0x8f, 0x61, 0x34, 0x43, 0x55, 0x14,
    0x7f, 0x27, 0x09, 0xce, 0x4c, 0x8b, 0xea, 0x9b, 0x7b, 0x19, 0x25, 0xbc, 0x6e,
    0x30, 0x1f, 0x06, 0x03, 0x55, 0x1d, 0x23, 0x04, 0x18, 0x30, 0x16, 0x80, 0x14,
    0x0e, 0x07, 0x60, 0xd4, 0x39, 0xc9, 0x1b, 0x5b, 0x5d, 0x90, 0x7b, 0x23, 0xc8,
    0xd2, 0x34, 0x9d, 0x4a, 0x9a, 0x46, 0x39, 0x30, 0x09, 0x06, 0x03, 0x55, 0x1d,
    0x11, 0x04, 0x02, 0x30, 0x00, 0x30, 0x1c, 0x06, 0x03, 0x55, 0x1d, 0x12, 0x04,
    0x15, 0x30, 0x13, 0x81, 0x11, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x61, 0x6c, 0x40,
    0x69, 0x70, 0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x30, 0x72, 0x06, 0x09,
    0x60, 0x86, 0x48, 0x01, 0x86, 0xf8, 0x42, 0x01, 0x0d, 0x04, 0x65, 0x16, 0x63,
    0x4f, 0x72, 0x67, 0x61, 0x6e, 0x69, 0x7a, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x20,
    0x49, 0x6e, 0x66, 0x6f, 0x72, 0x6d, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x4e,
    0x4f, 0x54, 0x20, 0x56, 0x41, 0x4c, 0x49, 0x44, 0x41, 0x54, 0x45, 0x44, 0x2e,
    0x20, 0x43, 0x4c, 0x41, 0x53, 0x45, 0x41, 0x31, 0x20, 0x53, 0x65, 0x72, 0x76,
    0x65, 0x72, 0x20, 0x43, 0x65, 0x72, 0x74, 0x69, 0x66, 0x69, 0x63, 0x61, 0x74,
    0x65, 0x20, 0x69, 0x73, 0x73, 0x75, 0x65, 0x64, 0x20, 0x62, 0x79, 0x20, 0x68,
    0x74, 0x74, 0x70, 0x73, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x69, 0x70,
    0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x30, 0x2f, 0x06, 0x09, 0x60,
    0x86, 0x48, 0x01, 0x86, 0xf8, 0x42, 0x01, 0x02, 0x04, 0x22, 0x16, 0x20, 0x68,
    0x74, 0x74, 0x70, 0x73, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x69, 0x70,
    0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61,
    0x32, 0x30, 0x30, 0x32, 0x2f, 0x30, 0x43, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01,
    0x86, 0xf8, 0x42, 0x01, 0x04, 0x04, 0x36, 0x16, 0x34, 0x68, 0x74, 0x74, 0x70,
    0x73, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x69, 0x70, 0x73, 0x63, 0x61,
    0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61, 0x32, 0x30, 0x30,
    0x32, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61, 0x32, 0x30, 0x30, 0x32, 0x43, 0x4c,
    0x41, 0x53, 0x45, 0x41, 0x31, 0x2e, 0x63, 0x72, 0x6c, 0x30, 0x46, 0x06, 0x09,
    0x60, 0x86, 0x48, 0x01, 0x86, 0xf8, 0x42, 0x01, 0x03, 0x04, 0x39, 0x16, 0x37,
    0x68, 0x74, 0x74, 0x70, 0x73, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x69,
    0x70, 0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x69, 0x70, 0x73, 0x63,
    0x61, 0x32, 0x30, 0x30, 0x32, 0x2f, 0x72, 0x65, 0x76, 0x6f, 0x63, 0x61, 0x74,
    0x69, 0x6f, 0x6e, 0x43, 0x4c, 0x41, 0x53, 0x45, 0x41, 0x31, 0x2e, 0x68, 0x74,
    0x6d, 0x6c, 0x3f, 0x30, 0x43, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x86, 0xf8,
    0x42, 0x01, 0x07, 0x04, 0x36, 0x16, 0x34, 0x68, 0x74, 0x74, 0x70, 0x73, 0x3a,
    0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x69, 0x70, 0x73, 0x63, 0x61, 0x2e, 0x63,
    0x6f, 0x6d, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61, 0x32, 0x30, 0x30, 0x32, 0x2f,
    0x72, 0x65, 0x6e, 0x65, 0x77, 0x61, 0x6c, 0x43, 0x4c, 0x41, 0x53, 0x45, 0x41,
    0x31, 0x2e, 0x68, 0x74, 0x6d, 0x6c, 0x3f, 0x30, 0x41, 0x06, 0x09, 0x60, 0x86,
    0x48, 0x01, 0x86, 0xf8, 0x42, 0x01, 0x08, 0x04, 0x34, 0x16, 0x32, 0x68, 0x74,
    0x74, 0x70, 0x73, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x2e, 0x69, 0x70, 0x73,
    0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61, 0x32,
    0x30, 0x30, 0x32, 0x2f, 0x70, 0x6f, 0x6c, 0x69, 0x63, 0x79, 0x43, 0x4c, 0x41,
    0x53, 0x45, 0x41, 0x31, 0x2e, 0x68, 0x74, 0x6d, 0x6c, 0x30, 0x81, 0x83, 0x06,
    0x03, 0x55, 0x1d, 0x1f, 0x04, 0x7c, 0x30, 0x7a, 0x30, 0x39, 0xa0, 0x37, 0xa0,
    0x35, 0x86, 0x33, 0x68, 0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77,
    0x2e, 0x69, 0x70, 0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x69, 0x70,
    0x73, 0x63, 0x61, 0x32, 0x30, 0x30, 0x32, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61,
    0x32, 0x30, 0x30, 0x32, 0x43, 0x4c, 0x41, 0x53, 0x45, 0x41, 0x31, 0x2e, 0x63,
    0x72, 0x6c, 0x30, 0x3d, 0xa0, 0x3b, 0xa0, 0x39, 0x86, 0x37, 0x68, 0x74, 0x74,
    0x70, 0x3a, 0x2f, 0x2f, 0x77, 0x77, 0x77, 0x62, 0x61, 0x63, 0x6b, 0x2e, 0x69,
    0x70, 0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x69, 0x70, 0x73, 0x63,
    0x61, 0x32, 0x30, 0x30, 0x32, 0x2f, 0x69, 0x70, 0x73, 0x63, 0x61, 0x32, 0x30,
    0x30, 0x32, 0x43, 0x4c, 0x41, 0x53, 0x45, 0x41, 0x31, 0x2e, 0x63, 0x72, 0x6c,
    0x30, 0x32, 0x06, 0x08, 0x2b, 0x06, 0x01, 0x05, 0x05, 0x07, 0x01, 0x01, 0x04,
    0x26, 0x30, 0x24, 0x30, 0x22, 0x06, 0x08, 0x2b, 0x06, 0x01, 0x05, 0x05, 0x07,
    0x30, 0x01, 0x86, 0x16, 0x68, 0x74, 0x74, 0x70, 0x3a, 0x2f, 0x2f, 0x6f, 0x63,
    0x73, 0x70, 0x2e, 0x69, 0x70, 0x73, 0x63, 0x61, 0x2e, 0x63, 0x6f, 0x6d, 0x2f,
    0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x05,
    0x05, 0x00, 0x03, 0x81, 0x81, 0x00, 0x68, 0xee, 0x79, 0x97, 0x97, 0xdd, 0x3b,
    0xef, 0x16, 0x6a, 0x06, 0xf2, 0x14, 0x9a, 0x6e, 0xcd, 0x9e, 0x12, 0xf7, 0xaa,
    0x83, 0x10, 0xbd, 0xd1, 0x7c, 0x98, 0xfa, 0xc7, 0xae, 0xd4, 0x0e, 0x2c, 0x9e,
    0x38, 0x05, 0x9d, 0x52, 0x60, 0xa9, 0x99, 0x0a, 0x81, 0xb4, 0x98, 0x90, 0x1d,
    0xae, 0xbb, 0x4a, 0xd7, 0xb9, 0xdc, 0x88, 0x9e, 0x37, 0x78, 0x41, 0x5b, 0xf7,
    0x82, 0xa5, 0xf2, 0xba, 0x41, 0x25, 0x5a, 0x90, 0x1a, 0x1e, 0x45, 0x38, 0xa1,
    0x52, 0x58, 0x75, 0x94, 0x26, 0x44, 0xfb, 0x20, 0x07, 0xba, 0x44, 0xcc, 0xe5,
    0x4a, 0x2d, 0x72, 0x3f, 0x98, 0x47, 0xf6, 0x26, 0xdc, 0x05, 0x46, 0x05, 0x07,
    0x63, 0x21, 0xab, 0x46, 0x9b, 0x9c, 0x78, 0xd5, 0x54, 0x5b, 0x3d, 0x0c, 0x1e,
    0xc8, 0x64, 0x8c, 0xb5, 0x50, 0x23, 0x82, 0x6f, 0xdb, 0xb8, 0x22, 0x1c, 0x43,
    0x96, 0x07, 0xa8, 0xbb
}.slice();

internal static slice<slice<@string>> stringSliceTestData = new slice<@string>[]{
    new @string[]{"foo"u8, "bar"u8}.slice(),
    new @string[]{"foo"u8, "\\bar"u8}.slice(),
    new @string[]{"foo"u8, "\"bar\""u8}.slice(),
    new @string[]{"foo"u8, "åäö"u8}.slice()
}.slice();

public static void TestStringSlice(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in stringSliceTestData) {
        var (bs, err) = Marshal(test);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        ref var res = ref heap<slice<@string>>(out var Ꮡres);
        (_, err) = Unmarshal(bs, Ꮡres);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        if (fmt.Sprintf("%v"u8, res) != fmt.Sprintf("%v"u8, test)) {
            Ꮡt.Errorf("incorrect marshal/unmarshal; %v != %v"u8, res, test);
        }
    }
}

[GoType] internal partial struct explicitTaggedTimeTest {
    [GoTag(@"asn1:""explicit,tag:0""")]
    public time.Time Time;
}


[GoType("dyn")] partial struct explicitTaggedTimeTestDataᴛ1 {
    internal slice<byte> @in;
    internal explicitTaggedTimeTest @out;
}
internal static slice<explicitTaggedTimeTestDataᴛ1> explicitTaggedTimeTestData = new explicitTaggedTimeTestDataᴛ1[]{
    new(new byte[]{0x30, 0x11, 0xa0, 0xf, 0x17, 0xd, (rune)'9', (rune)'1', (rune)'0', (rune)'5', (rune)'0', (rune)'6', (rune)'1', (rune)'6', (rune)'4', (rune)'5', (rune)'4', (rune)'0', (rune)'Z'}.slice(),
        new explicitTaggedTimeTest(time.Date(1991, 5, 6, 16, 45, 40, 0, time.ΔUTC))),
    new(new byte[]{0x30, 0x17, 0xa0, 0xf, 0x18, 0x13, (rune)'2', (rune)'0', (rune)'1', (rune)'0', (rune)'0', (rune)'1', (rune)'0', (rune)'2', (rune)'0', (rune)'3', (rune)'0', (rune)'4', (rune)'0', (rune)'5', (rune)'+', (rune)'0', (rune)'6', (rune)'0', (rune)'7'}.slice(),
        new explicitTaggedTimeTest(time.Date(2010, 1, 2, 3, 4, 5, 0, time.FixedZone(""u8, 6 * 60 * 60 + 7 * 60))))
}.slice();

public static void TestExplicitTaggedTime(ж<testing.T> Ꮡt) {
    // Test that a time.Time will match either tagUTCTime or
    // tagGeneralizedTime.
    foreach (var (i, test) in explicitTaggedTimeTestData) {
        ref var got = ref heap(new explicitTaggedTimeTest(), out var Ꮡgot);
        var (_, err) = Unmarshal(test.@in, Ꮡgot);
        if (err != default!) {
            Ꮡt.Errorf("Unmarshal failed at index %d %v"u8, i, err);
        }
        if (!got.Time.Equal(test.@out.Time)) {
            Ꮡt.Errorf("#%d: got %v, want %v"u8, i, got.Time, test.@out.Time);
        }
    }
}

[GoType] internal partial struct implicitTaggedTimeTest {
    [GoTag(@"asn1:""tag:24""")]
    public time.Time Time;
}

public static void TestImplicitTaggedTime(ж<testing.T> Ꮡt) {
    // An implicitly tagged time value, that happens to have an implicit
    // tag equal to a GENERALIZEDTIME, should still be parsed as a UTCTime.
    // (There's no "timeType" in fieldParameters to determine what type of
    // time should be expected when implicitly tagged.)
    var der = new byte[]{0x30, 0x0f, (byte)(0x80 | 24), 0xd, (rune)'9', (rune)'1', (rune)'0', (rune)'5', (rune)'0', (rune)'6', (rune)'1', (rune)'6', (rune)'4', (rune)'5', (rune)'4', (rune)'0', (rune)'Z'}.slice();
    ref var result = ref heap(new implicitTaggedTimeTest(), out var Ꮡresult);
    {
        var (_, err) = Unmarshal(der, Ꮡresult); if (err != default!) {
            Ꮡt.Fatalf("Error while parsing: %s"u8, err);
        }
    }
    {
        var expected = time.Date(1991, 5, 6, 16, 45, 40, 0, time.ΔUTC); if (!result.Time.Equal(expected)) {
            Ꮡt.Errorf("Wrong result. Got %v, want %v"u8, result.Time, expected);
        }
    }
}

[GoType] internal partial struct truncatedExplicitTagTest {
    [GoTag(@"asn1:""explicit,tag:0""")]
    public nint Test;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalReturnedWithoutˢ = (@string)"Unmarshal returned without error"u8;

public static void TestTruncatedExplicitTag(ж<testing.T> Ꮡt) {
    // This crashed Unmarshal in the past. See #11154.
    var der = new byte[]{
        0x30, // SEQUENCE

        0x02, // two bytes long

        0xa0, // context-specific, tag 0

        0x30
    }.slice();
    // 48 bytes long
    ref var result = ref heap(new truncatedExplicitTagTest(), out var Ꮡresult);
    {
        var (_, err) = Unmarshal(der, Ꮡresult); if (err == default!) {
            Ꮡt.Error(unmarshalReturnedWithoutˢ);
        }
    }
}

[GoType] internal partial struct invalidUTF8Test {
    [GoTag(@"asn1:""utf8""")]
    public @string Str;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object successfullyUnmarshaledˢ = (@string)"Successfully unmarshaled invalid UTF-8 data"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedSubstringᶜ = "UTF"u8;

public static void TestUnmarshalInvalidUTF8(ж<testing.T> Ꮡt) {
    var data = slice<byte>(((@string)(new byte[]{0x30, 0x05, 0x0c, 0x03, 0x61, 0xc9, 0x63})));
    ref var result = ref heap(new invalidUTF8Test(), out var Ꮡresult);
    var (_, err) = Unmarshal(data, Ꮡresult);
    @string expectedSubstring = expectedSubstringᶜ;
    if (err == default!){
        Ꮡt.Fatal(successfullyUnmarshaledˢ);
    } else 
    if (!strings.Contains(err.Error(), expectedSubstring)) {
        Ꮡt.Fatalf("Expected error to mention %q but error was %q"u8, expectedSubstring, err.Error());
    }
}

[GoType("dyn")] internal partial struct TestMarshalNilValue_nilValueTestData {
    public any V;
}

public static void TestMarshalNilValue(ж<testing.T> Ꮡt) {
    var nilValueTestData = new any[]{
        default!,
        new TestMarshalNilValue_nilValueTestData()
    }.slice();
    foreach (var (i, test) in nilValueTestData) {
        {
            var (_, err) = Marshal(test); if (err == default!) {
                Ꮡt.Fatalf("#%d: successfully marshaled nil value"u8, i);
            }
        }
    }
}

[GoType] internal partial struct unexported {
    public nint X;
    internal nint y;
}

[GoType] internal partial struct exported {
    public nint X;
    public nint Y;
}

public static void TestUnexportedStructField(ж<testing.T> Ꮡt) {
    var want = new StructuralError("struct contains unexported fields"u8);
    var (_, err) = Marshal(new unexported(X: 5, y: 1));
    if (!AreEqual(err, want)) {
        Ꮡt.Errorf("got %v, want %v"u8, err, want);
    }
    (var bs, err) = Marshal(new exported(X: 5, Y: 1));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var u = ref heap(new unexported(), out var Ꮡu);
    (_, err) = Unmarshal(bs, Ꮡu);
    if (!AreEqual(err, want)) {
        Ꮡt.Errorf("got %v, want %v"u8, err, want);
    }
}

public static void TestNull(ж<testing.T> Ꮡt) {
    var (marshaled, err) = Marshal(NullRawValue);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(NullBytes, marshaled)) {
        Ꮡt.Errorf("Expected Marshal of NullRawValue to yield %x, got %x"u8, NullBytes, marshaled);
    }
    ref var unmarshaled = ref heap<global::go.encoding.asn1_package.RawValue>(out var Ꮡunmarshaled);
    unmarshaled = new RawValue(nil);
    {
        var (_, errΔ1) = Unmarshal(NullBytes, Ꮡunmarshaled); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    unmarshaled.FullBytes = NullRawValue.FullBytes;
    if (len(unmarshaled.Bytes) == 0) {
        // DeepEqual considers a nil slice and an empty slice to be different.
        unmarshaled.Bytes = NullRawValue.Bytes;
    }
    if (!reflect.DeepEqual(NullRawValue, unmarshaled)) {
        Ꮡt.Errorf("Expected Unmarshal of NullBytes to yield %v, got %v"u8, NullRawValue, unmarshaled);
    }
}

[GoType("dyn")] [GoLocalName("foo")] internal partial struct TestExplicitTagRawValueStruct_foo {
    [GoTag(@"asn1:""optional,explicit,tag:5""")]
    public global::go.encoding.asn1_package.RawValue A;
    [GoTag(@"asn1:""optional,explicit,tag:6""")]
    public slice<byte> B;
}

public static void TestExplicitTagRawValueStruct(ж<testing.T> Ꮡt) {
    var before = new TestExplicitTagRawValueStruct_foo(B: new byte[]{1, 2, 3}.slice());
    var (derBytes, err) = Marshal(before);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var after = ref heap(new TestExplicitTagRawValueStruct_foo(), out var Ꮡafter);
    {
        var (rest, errΔ1) = Unmarshal(derBytes, Ꮡafter); if (errΔ1 != default! || len(rest) != 0) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string got = fmt.Sprintf("%#v"u8, after);
    @string want = fmt.Sprintf("%#v"u8, before);
    if (got != want) {
        Ꮡt.Errorf("got %s, want %s (DER: %x)"u8, got, want, derBytes);
    }
}

[GoType("dyn")] [GoLocalName("taggedRawValue")] internal partial struct TestTaggedRawValue_taggedRawValue {
    [GoTag(@"asn1:""tag:5""")]
    public global::go.encoding.asn1_package.RawValue A;
}

[GoType("dyn")] [GoLocalName("untaggedRawValue")] internal partial struct TestTaggedRawValue_untaggedRawValue {
    public global::go.encoding.asn1_package.RawValue A;
}

[GoType("dyn")] internal partial struct TestTaggedRawValue_tests {
    internal bool shouldMatch;
    internal slice<byte> derBytes;
}

public static void TestTaggedRawValue(ж<testing.T> Ꮡt) {
    UntypedInt isCompound = 0x20;
    UntypedInt tag = 5;
    var tests = new TestTaggedRawValue_tests[]{
        new(false, new byte[]{0x30, 3, TagInteger, 1, 1}.slice()),
        new(true, new byte[]{0x30, 3, (byte)(((ClassContextSpecific << (int)(6))) | (byte)tag), 1, 1}.slice()),
        new(true, new byte[]{0x30, 3, (byte)((UntypedInt)(((ClassContextSpecific << (int)(6))) | tag) | (byte)isCompound), 1, 1}.slice()),
        new(false, new byte[]{0x30, 3, (byte)((UntypedInt)(((ClassApplication << (int)(6))) | tag) | (byte)isCompound), 1, 1}.slice()),
        new(false, new byte[]{0x30, 3, (byte)((UntypedInt)(((ClassPrivate << (int)(6))) | tag) | (byte)isCompound), 1, 1}.slice())
    }.slice();
    foreach (var (i, test) in tests) {
        ref var tagged = ref heap(new TestTaggedRawValue_taggedRawValue(), out var Ꮡtagged);
        {
            var (_, err) = Unmarshal(test.derBytes, Ꮡtagged); if ((err == default!) != test.shouldMatch) {
                Ꮡt.Errorf("#%d: unexpected result parsing %x: %s"u8, i, test.derBytes, err);
            }
        }
        // An untagged RawValue should accept anything.
        ref var untagged = ref heap(new TestTaggedRawValue_untaggedRawValue(), out var Ꮡuntagged);
        {
            var (_, err) = Unmarshal(test.derBytes, Ꮡuntagged); if (err != default!) {
                Ꮡt.Errorf("#%d: unexpected failure parsing %x with untagged RawValue: %s"u8, i, test.derBytes, err);
            }
        }
    }
}

// Example from https://tools.ietf.org/html/rfc7292#appendix-B.
// Some characters from the "Letterlike Symbols Unicode block".

[GoType("dyn")] partial struct bmpStringTestsᴛ1 {
    internal @string decoded;
    internal @string encodedHex;
}
internal static slice<bmpStringTestsᴛ1> bmpStringTests = new bmpStringTestsᴛ1[]{
    new(""u8, "0000"u8),
    new("Beavis"u8, "0042006500610076006900730000"u8),
    new("\u2115 - Double-struck N"u8, "21150020002d00200044006f00750062006c0065002d00730074007200750063006b0020004e0000"u8)
}.slice();

public static void TestBMPString(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in bmpStringTests) {
        var (encoded, err) = hex.DecodeString(test.encodedHex);
        if (err != default!) {
            Ꮡt.Fatalf("#%d: failed to decode from hex string"u8, i);
        }
        (var decoded, err) = parseBMPString(encoded);
        if (err != default!) {
            Ꮡt.Errorf("#%d: decoding output gave an error: %s"u8, i, err);
            continue;
        }
        if (decoded != test.decoded) {
            Ꮡt.Errorf("#%d: decoding output resulted in %q, but it should have been %q"u8, i, decoded, test.decoded);
            continue;
        }
    }
}

public static void TestNonMinimalEncodedOID(ж<testing.T> Ꮡt) {
    var (h, err) = hex.DecodeString("060a2a80864886f70d01010b"u8);
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode from hex string: %s"u8, err);
    }
    ref var oid = ref heap<global::go.encoding.asn1_package.ObjectIdentifier>(out var Ꮡoid);
    (_, err) = Unmarshal(h, Ꮡoid);
    if (err == default!) {
        Ꮡt.Fatalf("accepted non-minimally encoded oid"u8);
    }
}

public static void BenchmarkObjectIdentifierString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var oidPublicKeyRSA = new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 1}.slice());
    for (nint i = 0; i < b.N; i++) {
        _ = oidPublicKeyRSA.String();
    }
}

[GoType("dyn")] internal partial struct TestParsingMemoryConsumption_out {
    public slice<nint> Id;
    [GoTag(@"asn1:""optional""")]
    public bool Critical;
    public slice<byte> Value;
}

public static void TestParsingMemoryConsumption(ж<testing.T> Ꮡt) {
    // Craft a syntatically valid, but empty, ~10 MB DER bomb. A successful
    // unmarshal of this bomb should yield ~280 MB. However, the parsing should
    // fail due to the empty content; and, in such cases, we want to make sure
    // that we do not unnecessarily allocate memories.
    var derBomb = new slice<byte>(10_000_000);
    foreach (var (i, _) in derBomb) {
        derBomb[i] = 0x30;
    }
    derBomb = appendꓸꓸꓸ(new byte[]{0x30, 0x83, 0x98, 0x96, 0x80}.slice(), derBomb);
    ref var m = ref heap(new runtime.MemStats(), out var Ꮡm);
    runtime.GC();
    runtime.ReadMemStats(Ꮡm);
    var memBefore = m.TotalAlloc;
    ref var @out = ref heap<slice<TestParsingMemoryConsumption_out>>(out var Ꮡout);
    var (_, err) = Unmarshal(derBomb, Ꮡout);
    if (!errors.As(err, Ꮡ(new SyntaxError(nil)))) {
        Ꮡt.Fatalf("Incorrect error result: want (%v), but got (%v) instead"u8, Ꮡ(new SyntaxError(nil)), err);
    }
    runtime.ReadMemStats(Ꮡm);
    var memDiff = m.TotalAlloc - memBefore;
    // Ensure that the memory allocated does not exceed 10<<21 (~20 MB) when
    // the parsing fails.
    if (memDiff > ((uint64)10 << (int)(21))) {
        Ꮡt.Errorf("Too much memory allocated while parsing DER: %v MiB"u8, memDiff / 1024 / 1024);
    }
}

} // end asn1_internal_test_package
