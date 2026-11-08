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

internal partial struct boolTest {
    internal slice<byte> @in;
    internal bool ok;
    internal bool @out;
}

internal static slice<boolTest> boolTestData = [
    new([0x00], true, false),
    new([0xff], true, true),
    new([0x00, 0x00], false, false),
    new([0xff, 0xff], false, false),
    new([0x01], false, false)
];

public static void TestParseBool(ж<testing.T> t) {
    foreach (var (i, test) in range(boolTestData)) {
        var (ret, err) = parseBool(test.@in);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (test.ok && ret != test.@out) {
            t.Errorf("#%d: Bad result: %v (expected %v)", i, ret, test.@out);
        }
    }
}

internal partial struct int64Test {
    internal slice<byte> @in;
    internal bool ok;
    internal int64 @out;
}

internal static slice<int64Test> int64TestData = [
    new([0x00], true, 0),
    new([0x7f], true, 127),
    new([0x00, 0x80], true, 128),
    new([0x01, 0x00], true, 256),
    new([0x80], true, -128),
    new([0xff, 0x7f], true, -129),
    new([0xff], true, -1),
    new([0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], true, -9223372036854775808L),
    new([0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], false, 0),
    new([], false, 0),
    new([0x00, 0x7f], false, 0),
    new([0xff, 0xf0], false, 0)
];

public static void TestParseInt64(ж<testing.T> t) {
    foreach (var (i, test) in range(int64TestData)) {
        var (ret, err) = parseInt64(test.@in);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (test.ok && ret != test.@out) {
            t.Errorf("#%d: Bad result: %v (expected %v)", i, ret, test.@out);
        }
    }
}

internal partial struct int32Test {
    internal slice<byte> @in;
    internal bool ok;
    internal int32 @out;
}

internal static slice<int32Test> int32TestData = [
    new([0x00], true, 0),
    new([0x7f], true, 127),
    new([0x00, 0x80], true, 128),
    new([0x01, 0x00], true, 256),
    new([0x80], true, -128),
    new([0xff, 0x7f], true, -129),
    new([0xff], true, -1),
    new([0x80, 0x00, 0x00, 0x00], true, -2147483648),
    new([0x80, 0x00, 0x00, 0x00, 0x00], false, 0),
    new([], false, 0),
    new([0x00, 0x7f], false, 0),
    new([0xff, 0xf0], false, 0)
];

public static void TestParseInt32(ж<testing.T> t) {
    foreach (var (i, test) in range(int32TestData)) {
        var (ret, err) = parseInt32(test.@in);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (test.ok && ret != test.@out) {
            t.Errorf("#%d: Bad result: %v (expected %v)", i, ret, test.@out);
        }
    }
}

partial struct bigIntTestsᴛ1 /*dyn*/ {
    internal slice<byte> @in;
    internal bool ok;
    internal @string base10;
}
internal static slice<bigIntTestsᴛ1> bigIntTests = [
    new([0xff], true, "-1"),
    new([0x00], true, "0"),
    new([0x01], true, "1"),
    new([0x00, 0xff], true, "255"),
    new([0xff, 0x00], true, "-256"),
    new([0x01, 0x00], true, "256"),
    new([], false, ""),
    new([0x00, 0x7f], false, ""),
    new([0xff, 0xf0], false, "")
];

public static void TestParseBigInt(ж<testing.T> t) {
    foreach (var (i, test) in range(bigIntTests)) {
        var (ret, err) = parseBigInt(test.@in);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (test.ok) {
            if (ret.String() != test.base10) {
                t.Errorf("#%d: bad result from %x, got %s want %s", i, test.@in, ret.String(), test.base10);
            }
            var (e, errΔ1) = makeBigInt(ret);
            if (errΔ1 != nil) {
                t.Errorf("%d: err=%q", i, errΔ1);
                continue;
            }
            var result = new slice<byte>(e.Len());
            e.Encode(result);
            if (!bytes.Equal(result, test.@in)) {
                t.Errorf("#%d: got %x from marshaling %s, want %x", i, result, ret.OrTypedNil(), test.@in);
            }
        }
    }
}

internal partial struct bitStringTest {
    internal slice<byte> @in;
    internal bool ok;
    internal slice<byte> @out;
    internal nint bitLength;
}

internal static slice<bitStringTest> bitStringTestData = [
    new([], false, [], 0),
    new([0x00], true, [], 0),
    new([0x07, 0x00], true, [0x00], 1),
    new([0x07, 0x01], false, [], 0),
    new([0x07, 0x40], false, [], 0),
    new([0x08, 0x00], false, [], 0)
];

public static void TestBitString(ж<testing.T> t) {
    foreach (var (i, test) in range(bitStringTestData)) {
        var (ret, err) = parseBitString(test.@in);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (err == nil) {
            if (test.bitLength != ret.BitLength || !bytes.Equal(ret.Bytes, test.@out)) {
                t.Errorf("#%d: Bad result: %v (expected %v %v)", i, ret, test.@out, test.bitLength);
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

public static void TestBitStringAt(ж<testing.T> t) {
    var bs = new BitString([0x82, 0x40], 16);
    if (bs.At(0) != 1) {
        t.Error(failedˢ);
    }
    if (bs.At(1) != 0) {
        t.Error(failedˢ2);
    }
    if (bs.At(6) != 1) {
        t.Error(failedˢ3);
    }
    if (bs.At(9) != 1) {
        t.Error(failedˢ4);
    }
    if (bs.At(-1) != 0) {
        t.Error(failedˢ5);
    }
    if (bs.At(17) != 0) {
        t.Error(failedˢ6);
    }
}

internal partial struct bitStringRightAlignTest {
    internal slice<byte> @in;
    internal nint inlen;
    internal slice<byte> @out;
}

internal static slice<bitStringRightAlignTest> bitStringRightAlignTests = [
    new([0x80], 1, [0x01]),
    new([0x80, 0x80], 9, [0x01, 0x01]),
    new([], 0, []),
    new([0xce], 8, [0xce]),
    new([0xce, 0x47], 16, [0xce, 0x47]),
    new([0x34, 0x50], 12, [0x03, 0x45])
];

public static void TestBitStringRightAlign(ж<testing.T> t) {
    foreach (var (i, test) in range(bitStringRightAlignTests)) {
        var bs = new BitString(test.@in, test.inlen);
        var @out = bs.RightAlign();
        if (!bytes.Equal(@out, test.@out)) {
            t.Errorf("#%d got: %x want: %x", i, @out, test.@out);
        }
    }
}

internal partial struct objectIdentifierTest {
    internal slice<byte> @in;
    internal bool ok;
    internal global::go.encoding.asn1_package.ObjectIdentifier @out; // has base type[]int
}

internal static slice<objectIdentifierTest> objectIdentifierTestData = [
    new([], false, slice<nint>([])),
    new([85], true, slice<nint>([2, 5])),
    new([85, 0x02], true, slice<nint>([2, 5, 2])),
    new([85, 0x02, 0xc0, 0x00], true, slice<nint>([2, 5, 2, 0x2000])),
    new([0x81, 0x34, 0x03], true, slice<nint>([2, 100, 3])),
    new([85, 0x02, 0xc0, 0x80, 0x80, 0x80, 0x80], false, slice<nint>([]))
];

public static void TestObjectIdentifier(ж<testing.T> t) {
    foreach (var (i, test) in range(objectIdentifierTestData)) {
        var (ret, err) = parseObjectIdentifier(test.@in);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (err == nil) {
            if (!reflect.DeepEqual(test.@out, ret)) {
                t.Errorf("#%d: Bad result: %v (expected %v)", i, ret, test.@out);
            }
        }
    }

    @string s = ((global::go.encoding.asn1_package.ObjectIdentifier)slice<nint>([1, 2, 3, 4])).String(); if (s != "1.2.3.4"u8) {
        t.Errorf("bad ObjectIdentifier.String(). Got %s, want 1.2.3.4", s);
    }
}

internal partial struct timeTest {
    internal @string @in;
    internal bool ok;
    internal time.Time @out;
}

internal static slice<timeTest> utcTestData = slice<timeTest>([
    new("910506164540-0700", true, time.Date(1991, 5, 6, 16, 45, 40, 0, time.FixedZone("", -7 * 60 * 60))),
    new("910506164540+0730", true, time.Date(1991, 5, 6, 16, 45, 40, 0, time.FixedZone("", 7 * 60 * 60 + 30 * 60))),
    new("910506234540Z", true, time.Date(1991, 5, 6, 23, 45, 40, 0, time.ΔUTC)),
    new("9105062345Z", true, time.Date(1991, 5, 6, 23, 45, 0, 0, time.ΔUTC)),
    new("5105062345Z", true, time.Date(1951, 5, 6, 23, 45, 0, 0, time.ΔUTC)),
    new("a10506234540Z", false, new time.Time()),
    new("91a506234540Z", false, new time.Time()),
    new("9105a6234540Z", false, new time.Time()),
    new("910506a34540Z", false, new time.Time()),
    new("910506334a40Z", false, new time.Time()),
    new("91050633444aZ", false, new time.Time()),
    new("910506334461Z", false, new time.Time()),
    new("910506334400Za", false, new time.Time()),
    /* These are invalid times. However, the time package normalises times
	 * and they were accepted in some versions. See #11134. */
    new("000100000000Z", false, new time.Time()),
    new("101302030405Z", false, new time.Time()),
    new("100002030405Z", false, new time.Time()),
    new("100100030405Z", false, new time.Time()),
    new("100132030405Z", false, new time.Time()),
    new("100231030405Z", false, new time.Time()),
    new("100102240405Z", false, new time.Time()),
    new("100102036005Z", false, new time.Time()),
    new("100102030460Z", false, new time.Time()),
    new("-100102030410Z", false, new time.Time()),
    new("10-0102030410Z", false, new time.Time()),
    new("10-0002030410Z", false, new time.Time()),
    new("1001-02030410Z", false, new time.Time()),
    new("100102-030410Z", false, new time.Time()),
    new("10010203-0410Z", false, new time.Time()),
    new("1001020304-10Z", false, new time.Time())
]);

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string formatᶜ => ref formatᶜᵇ;
private static readonly @string formatᶜᵇ = "Jan _2 15:04:05 -0700 2006"u8;

public static void TestUTCTime(ж<testing.T> t) {
    foreach (var (i, test) in range(utcTestData)) {
        var (ret, err) = parseUTCTime(slice<byte>(test.@in));
        if (err != nil) {
            if (test.ok) {
                t.Errorf("#%d: parseUTCTime(%q) = error %v", i, test.@in, err);
            }
            continue;
        }
        if (!test.ok) {
            t.Errorf("#%d: parseUTCTime(%q) succeeded, should have failed", i, test.@in);
            continue;
        }
        @string format = formatᶜ; // ignore zone name, just offset
        @string have = ret.Format(format);
        @string want = test.@out.Format(format);
        if (have != want) {
            t.Errorf("#%d: parseUTCTime(%q) = %s, want %s", i, test.@in, have, want);
        }
    }
}

internal static slice<timeTest> generalizedTimeTestData = slice<timeTest>([
    new("20100102030405Z", true, time.Date(2010, 1, 2, 3, 4, 5, 0, time.ΔUTC)),
    new("20100102030405", false, new time.Time()),
    new("20100102030405.123456Z", true, time.Date(2010, 1, 2, 3, 4, 5, 123456000, time.ΔUTC)),
    new("20100102030405.123456", false, new time.Time()),
    new("20100102030405.Z", false, new time.Time()),
    new("20100102030405.", false, new time.Time()),
    new("20100102030405+0607", true, time.Date(2010, 1, 2, 3, 4, 5, 0, time.FixedZone("", 6 * 60 * 60 + 7 * 60))),
    new("20100102030405-0607", true, time.Date(2010, 1, 2, 3, 4, 5, 0, time.FixedZone("", -6 * 60 * 60 - 7 * 60))),
    /* These are invalid times. However, the time package normalises times
	 * and they were accepted in some versions. See #11134. */
    new("00000100000000Z", false, new time.Time()),
    new("20101302030405Z", false, new time.Time()),
    new("20100002030405Z", false, new time.Time()),
    new("20100100030405Z", false, new time.Time()),
    new("20100132030405Z", false, new time.Time()),
    new("20100231030405Z", false, new time.Time()),
    new("20100102240405Z", false, new time.Time()),
    new("20100102036005Z", false, new time.Time()),
    new("20100102030460Z", false, new time.Time()),
    new("-20100102030410Z", false, new time.Time()),
    new("2010-0102030410Z", false, new time.Time()),
    new("2010-0002030410Z", false, new time.Time()),
    new("201001-02030410Z", false, new time.Time()),
    new("20100102-030410Z", false, new time.Time()),
    new("2010010203-0410Z", false, new time.Time()),
    new("201001020304-10Z", false, new time.Time())
]);

public static void TestGeneralizedTime(ж<testing.T> t) {
    foreach (var (i, test) in range(generalizedTimeTestData)) {
        var (ret, err) = parseGeneralizedTime(slice<byte>(test.@in));
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did fail? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (err == nil) {
            if (!reflect.DeepEqual(test.@out, ret)) {
                t.Errorf("#%d: Bad result: %q → %v (expected %v)", i, test.@in, ret, test.@out);
            }
        }
    }
}

internal partial struct tagAndLengthTest {
    internal slice<byte> @in;
    internal bool ok;
    internal global::go.encoding.asn1_package.tagAndLength @out;
}

internal static slice<tagAndLengthTest> tagAndLengthData = [
    new([0x80, 0x01], true, new tagAndLength(2, 0, 1, false)),
    new([0xa0, 0x01], true, new tagAndLength(2, 0, 1, true)),
    new([0x02, 0x00], true, new tagAndLength(0, 2, 0, false)),
    new([0xfe, 0x00], true, new tagAndLength(3, 30, 0, true)),
    new([0x1f, 0x1f, 0x00], true, new tagAndLength(0, 31, 0, false)),
    new([0x1f, 0x81, 0x00, 0x00], true, new tagAndLength(0, 128, 0, false)),
    new([0x1f, 0x81, 0x80, 0x01, 0x00], true, new tagAndLength(0, 0x4001, 0, false)),
    new([0x00, 0x81, 0x80], true, new tagAndLength(0, 0, 128, false)),
    new([0x00, 0x82, 0x01, 0x00], true, new tagAndLength(0, 0, 256, false)),
    new([0x00, 0x83, 0x01, 0x00], false, new tagAndLength()),
    new([0x1f, 0x85], false, new tagAndLength()),
    new([0x30, 0x80], false, new tagAndLength()),
    // Superfluous zeros in the length should be an error.
    new([0xa0, 0x82, 0x00, 0xff], false, new tagAndLength()),
    // Lengths up to the maximum size of an int should work.
    new([0xa0, 0x84, 0x7f, 0xff, 0xff, 0xff], true, new tagAndLength(2, 0, 0x7fffffff, true)),
    // Lengths that would overflow an int should be rejected.
    new([0xa0, 0x84, 0x80, 0x00, 0x00, 0x00], false, new tagAndLength()),
    // Long length form may not be used for lengths that fit in short form.
    new([0xa0, 0x81, 0x7f], false, new tagAndLength()),
    // Tag numbers which would overflow int32 are rejected. (The value below is 2^31.)
    new([0x1f, 0x88, 0x80, 0x80, 0x80, 0x00, 0x00], false, new tagAndLength()),
    // Tag numbers that fit in an int32 are valid. (The value below is 2^31 - 1.)
    new([0x1f, 0x87, 0xFF, 0xFF, 0xFF, 0x7F, 0x00], true, new tagAndLength(tag: math.MaxInt32)),
    // Long tag number form may not be used for tags that fit in short form.
    new([0x1f, 0x1e, 0x00], false, new tagAndLength())
];

public static void TestParseTagAndLength(ж<testing.T> t) {
    foreach (var (i, test) in range(tagAndLengthData)) {
        var (tagAndLength, _, err) = parseTagAndLength(test.@in, 0);
        if ((err == nil) != test.ok) {
            t.Errorf("#%d: Incorrect error result (did pass? %v, expected: %v)", i, err == nil, test.ok);
        }
        if (err == nil && !reflect.DeepEqual(test.@out, tagAndLength)) {
            t.Errorf("#%d: Bad result: %v (expected %v)", i, tagAndLength, test.@out);
        }
    }
}

internal partial struct parseFieldParametersTest {
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

internal static slice<parseFieldParametersTest> parseFieldParametersTestData = slice<parseFieldParametersTest>([
    new("", new fieldParameters()),
    new("ia5", new fieldParameters(stringType: TagIA5String)),
    new("generalized", new fieldParameters(timeType: TagGeneralizedTime)),
    new("utc", new fieldParameters(timeType: TagUTCTime)),
    new("printable", new fieldParameters(stringType: TagPrintableString)),
    new("numeric", new fieldParameters(stringType: TagNumericString)),
    new("optional", new fieldParameters(optional: true)),
    new("explicit", new fieldParameters(@explicit: true, tag: @new<nint>())),
    new("application", new fieldParameters(application: true, tag: @new<nint>())),
    new("private", new fieldParameters(@private: true, tag: @new<nint>())),
    new("optional,explicit", new fieldParameters(optional: true, @explicit: true, tag: @new<nint>())),
    new("default:42", new fieldParameters(defaultValue: newInt64(42))),
    new("tag:17", new fieldParameters(tag: newInt(17))),
    new("optional,explicit,default:42,tag:17", new fieldParameters(optional: true, @explicit: true, defaultValue: newInt64(42), tag: newInt(17))),
    new("optional,explicit,default:42,tag:17,rubbish1", new fieldParameters(optional: true, @explicit: true, application: false, defaultValue: newInt64(42), tag: newInt(17), stringType: 0, timeType: 0, set: false, omitEmpty: false)),
    new("set", new fieldParameters(set: true))
]);

public static void TestParseFieldParameters(ж<testing.T> t) {
    foreach (var (i, test) in range(parseFieldParametersTestData)) {
        var f = parseFieldParameters(test.@in);
        if (!reflect.DeepEqual(f, test.@out)) {
            t.Errorf("#%d: Bad result: %v (expected %v)", i, f, test.@out);
        }
    }
}

public partial struct TestObjectIdentifierStruct {
    public global::go.encoding.asn1_package.ObjectIdentifier OID;
}

public partial struct TestContextSpecificTags {
    public nint A; /*`asn1:"tag:1"`*/
}

public partial struct TestContextSpecificTags2 {
    public nint A; /*`asn1:"explicit,tag:1"`*/
    public nint B;
}

public partial struct TestContextSpecificTags3 {
    public @string S; /*`asn1:"tag:1,utf8"`*/
}

public partial struct TestElementsAfterString {
    public @string S;
    public nint A, B;
}

public partial struct TestBigInt {
    public ж<bigꓸInt> X;
}

public partial struct TestSet {
    public slice<nint> Ints; /*`asn1:"set"`*/
}

partial struct unmarshalTestDataᴛ1 /*dyn*/ {
    internal slice<byte> @in;
    internal any @out;
}
internal static slice<unmarshalTestDataᴛ1> unmarshalTestData = slice<unmarshalTestDataᴛ1>([
    new([0x02, 0x01, 0x42], newInt(0x42).OrTypedNil()),
    new([0x05, 0x00], Ꮡ(new RawValue(0, 5, false, [], [0x05, 0x00]))),
    new([0x30, 0x08, 0x06, 0x06, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d], Ꮡ(new TestObjectIdentifierStruct(slice<nint>([1, 2, 840, 113549])))),
    new([0x03, 0x04, 0x06, 0x6e, 0x5d, 0xc0], Ꮡ(new BitString([110, 93, 192], 18))),
    new([0x30, 0x09, 0x02, 0x01, 0x01, 0x02, 0x01, 0x02, 0x02, 0x01, 0x03], Ꮡ(slice<nint>([1, 2, 3]))),
    new([0x02, 0x01, 0x10], newInt(16).OrTypedNil()),
    new([0x13, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t'], newString("test").OrTypedNil()),
    new([0x16, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t'], newString("test").OrTypedNil()),
    // Ampersand is allowed in PrintableString due to mistakes by major CAs.
    new([0x13, 0x05, (byte)'t', (byte)'e', (byte)'s', (byte)'t', (byte)'&'], newString("test&").OrTypedNil()),
    new([0x16, 0x04, (byte)'t', (byte)'e', (byte)'s', (byte)'t'], Ꮡ(new RawValue(0, 22, false, slice<byte>("test"u8), slice<byte>("\x16\x04test"u8)))),
    new([0x04, 0x04, 1, 2, 3, 4], Ꮡ(new RawValue(0, 4, false, [1, 2, 3, 4], [4, 4, 1, 2, 3, 4]))),
    new([0x30, 0x03, 0x81, 0x01, 0x01], Ꮡ(new TestContextSpecificTags(1))),
    new([0x30, 0x08, 0xa1, 0x03, 0x02, 0x01, 0x01, 0x02, 0x01, 0x02], Ꮡ(new TestContextSpecificTags2(1, 2))),
    new([0x30, 0x03, 0x81, 0x01, (byte)'@'], Ꮡ(new TestContextSpecificTags3("@"))),
    new([0x01, 0x01, 0x00], newBool(false).OrTypedNil()),
    new([0x01, 0x01, 0xff], newBool(true).OrTypedNil()),
    new([0x30, 0x0b, 0x13, 0x03, 0x66, 0x6f, 0x6f, 0x02, 0x01, 0x22, 0x02, 0x01, 0x33], Ꮡ(new TestElementsAfterString("foo", 0x22, 0x33))),
    new([0x30, 0x05, 0x02, 0x03, 0x12, 0x34, 0x56], Ꮡ(new TestBigInt(big.NewInt(0x123456)))),
    new([0x30, 0x0b, 0x31, 0x09, 0x02, 0x01, 0x01, 0x02, 0x01, 0x02, 0x02, 0x01, 0x03], Ꮡ(new TestSet(Ints: [1, 2, 3]))),
    new([0x12, 0x0b, (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', (byte)' '], newString("0123456789 ").OrTypedNil()),
    new([0x14, 0x03, 0xbf, 0x61, 0x3f], newString("¿a?").OrTypedNil())
]);

public static void TestUnmarshal(ж<testing.T> t) {
    foreach (var (i, test) in range(unmarshalTestData)) {
        var pv = reflect.New(reflect.TypeOf(test.@out).Elem());
        var val = pv.Interface();
        var (_, err) = Unmarshal(test.@in, val);
        if (err != nil) {
            t.Errorf("Unmarshal failed at index %d %v", i, err);
        }
        if (!reflect.DeepEqual(val, test.@out)) {
            t.Errorf("#%d:\nhave %#v\nwant %#v", i, val, test.@out);
        }
    }
}

internal partial struct TestUnmarshalWithNilOrNonPointer_tests /*dyn*/ {
    internal slice<byte> b;
    internal any v;
    internal @string want;
}

public static void TestUnmarshalWithNilOrNonPointer(ж<testing.T> t) {
    var tests = slice<TestUnmarshalWithNilOrNonPointer_tests>([
        new(b: [0x05, 0x00], v: default, want: "asn1: Unmarshal recipient value is nil"),
        new(b: [0x05, 0x00], v: new RawValue(), want: "asn1: Unmarshal recipient value is non-pointer asn1.RawValue"),
        new(b: [0x05, 0x00], v: ((ж<global::go.encoding.asn1_package.RawValue>)nil), want: "asn1: Unmarshal recipient value is nil *asn1.RawValue")
    ]);

    foreach (var (_, test) in range(tests)) {
        var (_, err) = Unmarshal(test.b, test.v);
        if (err == nil) {
            t.Errorf("Unmarshal expecting error, got nil");
            continue;
        }
        @string g = err.Error();
        @string w = test.want; if (g != w) {
            t.Errorf("InvalidUnmarshalError mismatch\nGot:  %q\nWant: %q", g, w);
        }
    }
}

public partial struct Certificate {
    public TBSCertificate TBSCertificate;
    public AlgorithmIdentifier SignatureAlgorithm;
    public global::go.encoding.asn1_package.BitString SignatureValue;
}

public partial struct TBSCertificate {
    public nint Version; /*`asn1:"optional,explicit,default:0,tag:0"`*/
    public global::go.encoding.asn1_package.RawValue SerialNumber;
    public AlgorithmIdentifier SignatureAlgorithm;
    public RDNSequence Issuer;
    public Validity Validity;
    public RDNSequence Subject;
    public PublicKeyInfo PublicKey;
}

public partial struct AlgorithmIdentifier {
    public global::go.encoding.asn1_package.ObjectIdentifier Algorithm;
}

public partial struct RDNSequence /*[]RelativeDistinguishedNameSET*/;

public partial struct RelativeDistinguishedNameSET /*[]AttributeTypeAndValue*/;

public partial struct AttributeTypeAndValue {
    public global::go.encoding.asn1_package.ObjectIdentifier Type;
    public any Value;
}

public partial struct Validity {
    public time.Time NotBefore, NotAfter;
}

public partial struct PublicKeyInfo {
    public AlgorithmIdentifier Algorithm;
    public global::go.encoding.asn1_package.BitString PublicKey;
}

public static void TestCertificate(ж<testing.T> t) {
    // This is a minimal, self-signed certificate that should parse correctly.
    ref var cert = ref heap(new Certificate(), out var Ꮡcert);
    var (_, err) = Unmarshal(derEncodedSelfSignedCertBytes, Ꮡcert); if (err != nil) {
        t.Errorf("Unmarshal failed: %v", err);
    }
    if (!reflect.DeepEqual(cert, derEncodedSelfSignedCert)) {
        t.Errorf("Bad result:\ngot: %+v\nwant: %+v", cert, derEncodedSelfSignedCert);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalSucceededShouldˢ = (@string)"Unmarshal succeeded, should not have"u8;

public static void TestCertificateWithNUL(ж<testing.T> t) {
    // This is the paypal NUL-hack certificate. It should fail to parse because
    // NUL isn't a permitted character in a PrintableString.

    ref var cert = ref heap(new Certificate(), out var Ꮡcert);
    var (_, err) = Unmarshal(derEncodedPaypalNULCertBytes, Ꮡcert); if (err == nil) {
        t.Error(unmarshalSucceededShouldˢ);
    }
}

internal partial struct rawStructTest {
    public global::go.encoding.asn1_package.RawContent Raw;
    public nint A;
}

public static void TestRawStructs(ж<testing.T> t) {
    ref var s = ref heap(new rawStructTest(), out var Ꮡs);
    var input = slice<byte>([0x30, 0x03, 0x02, 0x01, 0x50]);

    var (rest, err) = Unmarshal(input, Ꮡs);
    if (len(rest) != 0) {
        t.Errorf("incomplete parse: %x", rest);
        return;
    }
    if (err != nil) {
        t.Error(err);
        return;
    }
    if (s.A != 0x50) {
        t.Errorf("bad value for A: got %d want %d", s.A, (nint)0x50);
    }
    if (!bytes.Equal((slice<byte>)s.Raw, input)) {
        t.Errorf("bad value for Raw: got %x want %x", s.Raw, input);
    }
}

internal partial struct oiEqualTest {
    internal global::go.encoding.asn1_package.ObjectIdentifier first;
    internal global::go.encoding.asn1_package.ObjectIdentifier second;
    internal bool same;
}

internal static slice<oiEqualTest> oiEqualTests = [
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
];

public static void TestObjectIdentifierEqual(ж<testing.T> t) {
    foreach (var (_, o) in range(oiEqualTests)) {
        var s = o.first.Equal(o.second); if (s != o.same) {
            t.Errorf("ObjectIdentifier.Equal: got: %t want: %t", s, o.same);
        }
    }
}

internal static Certificate derEncodedSelfSignedCert = new Certificate(
    TBSCertificate: new TBSCertificate(
        Version: 0,
        SerialNumber: new RawValue(Class: 0, Tag: 2, IsCompound: false, Bytes: [0x0, 0x8c, 0xc3, 0x37, 0x92, 0x10, 0xec, 0x2c, 0x98], FullBytes: [2, 9, 0x0, 0x8c, 0xc3, 0x37, 0x92, 0x10, 0xec, 0x2c, 0x98]),
        SignatureAlgorithm: new AlgorithmIdentifier(Algorithm: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 5}.slice())),
        Issuer: new RDNSequence(new RelativeDistinguishedNameSET[]{
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 6}.slice()), Value: (@string)"XX")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 8}.slice()), Value: (@string)"Some-State")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 7}.slice()), Value: (@string)"City")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 10}.slice()), Value: (@string)"Internet Widgits Pty Ltd")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 3}.slice()), Value: (@string)"false.example.com")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 9, 1}.slice()), Value: (@string)"false@example.com")}.slice())
        }.slice()),
        Validity: new Validity(
            NotBefore: time.Date(2009, 10, 8, 0, 25, 53, 0, time.ΔUTC),
            NotAfter: time.Date(2010, 10, 8, 0, 25, 53, 0, time.ΔUTC)
        ),
        Subject: new RDNSequence(new RelativeDistinguishedNameSET[]{
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 6}.slice()), Value: (@string)"XX")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 8}.slice()), Value: (@string)"Some-State")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 7}.slice()), Value: (@string)"City")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 10}.slice()), Value: (@string)"Internet Widgits Pty Ltd")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{2, 5, 4, 3}.slice()), Value: (@string)"false.example.com")}.slice()),
            new RelativeDistinguishedNameSET(new AttributeTypeAndValue[]{new AttributeTypeAndValue(Type: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 9, 1}.slice()), Value: (@string)"false@example.com")}.slice())
        }.slice()),
        PublicKey: new PublicKeyInfo(
            Algorithm: new AlgorithmIdentifier(Algorithm: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 1}.slice())),
            PublicKey: new BitString(
                Bytes: [
                    0x30, 0x48, 0x2, 0x41, 0x0, 0xcd, 0xb7,
                    0x63, 0x9c, 0x32, 0x78, 0xf0, 0x6, 0xaa, 0x27, 0x7f, 0x6e, 0xaf, 0x42,
                    0x90, 0x2b, 0x59, 0x2d, 0x8c, 0xbc, 0xbe, 0x38, 0xa1, 0xc9, 0x2b, 0xa4,
                    0x69, 0x5a, 0x33, 0x1b, 0x1d, 0xea, 0xde, 0xad, 0xd8, 0xe9, 0xa5, 0xc2,
                    0x7e, 0x8c, 0x4c, 0x2f, 0xd0, 0xa8, 0x88, 0x96, 0x57, 0x72, 0x2a, 0x4f,
                    0x2a, 0xf7, 0x58, 0x9c, 0xf2, 0xc7, 0x70, 0x45, 0xdc, 0x8f, 0xde, 0xec,
                    0x35, 0x7d, 0x2, 0x3, 0x1, 0x0, 0x1
                ],
                BitLength: 592
            )
        )
    ),
    SignatureAlgorithm: new AlgorithmIdentifier(Algorithm: new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 5}.slice())),
    SignatureValue: new BitString(
        Bytes: [
            0xa6, 0x7b, 0x6, 0xec, 0x5e, 0xce,
            0x92, 0x77, 0x2c, 0xa4, 0x13, 0xcb, 0xa3, 0xca, 0x12, 0x56, 0x8f, 0xdc, 0x6c,
            0x7b, 0x45, 0x11, 0xcd, 0x40, 0xa7, 0xf6, 0x59, 0x98, 0x4, 0x2, 0xdf, 0x2b,
            0x99, 0x8b, 0xb9, 0xa4, 0xa8, 0xcb, 0xeb, 0x34, 0xc0, 0xf0, 0xa7, 0x8c, 0xf8,
            0xd9, 0x1e, 0xde, 0x14, 0xa5, 0xed, 0x76, 0xbf, 0x11, 0x6f, 0xe3, 0x60, 0xaa,
            0xfa, 0x88, 0x21, 0x49, 0x4, 0x35
        ],
        BitLength: 512
    )
);

internal static slice<byte> derEncodedSelfSignedCertBytes = [
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
];

internal static slice<byte> derEncodedPaypalNULCertBytes = [
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
];

internal static slice<slice<@string>> stringSliceTestData = [
    ["foo", "bar"],
    ["foo", "\\bar"],
    ["foo", "\"bar\""],
    ["foo", "åäö"]
];

public static void TestStringSlice(ж<testing.T> t) {
    foreach (var (_, test) in range(stringSliceTestData)) {
        var (bs, err) = Marshal(test);
        if (err != nil) {
            t.Error(err);
        }

        ref var res = ref heap<slice<@string>>(out var Ꮡres);
        (_, err) = Unmarshal(bs, Ꮡres);
        if (err != nil) {
            t.Error(err);
        }

        if (fmt.Sprintf("%v"u8, res) != fmt.Sprintf("%v"u8, test)) {
            t.Errorf("incorrect marshal/unmarshal; %v != %v", res, test);
        }
    }
}

internal partial struct explicitTaggedTimeTest {
    public time.Time Time; /*`asn1:"explicit,tag:0"`*/
}

partial struct explicitTaggedTimeTestDataᴛ1 /*dyn*/ {
    internal slice<byte> @in;
    internal explicitTaggedTimeTest @out;
}
internal static slice<explicitTaggedTimeTestDataᴛ1> explicitTaggedTimeTestData = slice<explicitTaggedTimeTestDataᴛ1>([
    new([0x30, 0x11, 0xa0, 0xf, 0x17, 0xd, (byte)'9', (byte)'1', (byte)'0', (byte)'5', (byte)'0', (byte)'6', (byte)'1', (byte)'6', (byte)'4', (byte)'5', (byte)'4', (byte)'0', (byte)'Z'],
        new explicitTaggedTimeTest(time.Date(1991, 5, 6, 16, 45, 40, 0, time.ΔUTC))),
    new([0x30, 0x17, 0xa0, 0xf, 0x18, 0x13, (byte)'2', (byte)'0', (byte)'1', (byte)'0', (byte)'0', (byte)'1', (byte)'0', (byte)'2', (byte)'0', (byte)'3', (byte)'0', (byte)'4', (byte)'0', (byte)'5', (byte)'+', (byte)'0', (byte)'6', (byte)'0', (byte)'7'],
        new explicitTaggedTimeTest(time.Date(2010, 1, 2, 3, 4, 5, 0, time.FixedZone("", 6 * 60 * 60 + 7 * 60))))
]);

public static void TestExplicitTaggedTime(ж<testing.T> t) {
    // Test that a time.Time will match either tagUTCTime or
    // tagGeneralizedTime.
    foreach (var (i, test) in range(explicitTaggedTimeTestData)) {
        ref var got = ref heap(new explicitTaggedTimeTest(), out var Ꮡgot);
        var (_, err) = Unmarshal(test.@in, Ꮡgot);
        if (err != nil) {
            t.Errorf("Unmarshal failed at index %d %v", i, err);
        }
        if (!got.Time.Equal(test.@out.Time)) {
            t.Errorf("#%d: got %v, want %v", i, got.Time, test.@out.Time);
        }
    }
}

internal partial struct implicitTaggedTimeTest {
    public time.Time Time; /*`asn1:"tag:24"`*/
}

public static void TestImplicitTaggedTime(ж<testing.T> t) {
    // An implicitly tagged time value, that happens to have an implicit
    // tag equal to a GENERALIZEDTIME, should still be parsed as a UTCTime.
    // (There's no "timeType" in fieldParameters to determine what type of
    // time should be expected when implicitly tagged.)
    var der = slice<byte>([0x30, 0x0f, (byte)(0x80 | 24), 0xd, (byte)'9', (byte)'1', (byte)'0', (byte)'5', (byte)'0', (byte)'6', (byte)'1', (byte)'6', (byte)'4', (byte)'5', (byte)'4', (byte)'0', (byte)'Z']);
    ref var result = ref heap(new implicitTaggedTimeTest(), out var Ꮡresult);
    var (_, err) = Unmarshal(der, Ꮡresult); if (err != nil) {
        t.Fatalf("Error while parsing: %s", err);
    }
    var expected = time.Date(1991, 5, 6, 16, 45, 40, 0, time.ΔUTC); if (!result.Time.Equal(expected)) {
        t.Errorf("Wrong result. Got %v, want %v", result.Time, expected);
    }
}

internal partial struct truncatedExplicitTagTest {
    public nint Test; /*`asn1:"explicit,tag:0"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalReturnedWithoutˢ = (@string)"Unmarshal returned without error"u8;

public static void TestTruncatedExplicitTag(ж<testing.T> t) {
    // This crashed Unmarshal in the past. See #11154.
    var der = slice<byte>([
        0x30, // SEQUENCE
        0x02, // two bytes long
        0xa0, // context-specific, tag 0
        0x30  // 48 bytes long
    ]);

    ref var result = ref heap(new truncatedExplicitTagTest(), out var Ꮡresult);
    var (_, err) = Unmarshal(der, Ꮡresult); if (err == nil) {
        t.Error(unmarshalReturnedWithoutˢ);
    }
}

internal partial struct invalidUTF8Test {
    public @string Str; /*`asn1:"utf8"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object successfullyUnmarshaledˢ = (@string)"Successfully unmarshaled invalid UTF-8 data"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectedSubstringᶜ => ref expectedSubstringᶜᵇ;
private static readonly @string expectedSubstringᶜᵇ = "UTF"u8;

public static void TestUnmarshalInvalidUTF8(ж<testing.T> t) {
    var data = slice<byte>(((@string)(new byte[]{0x30, 0x05, 0x0c, 0x03, 0x61, 0xc9, 0x63})));
    ref var result = ref heap(new invalidUTF8Test(), out var Ꮡresult);
    var (_, err) = Unmarshal(data, Ꮡresult);

    @string expectedSubstring = expectedSubstringᶜ;
    if (err == nil) {
        t.Fatal(successfullyUnmarshaledˢ);
    } else if (!strings.Contains(err.Error(), expectedSubstring)) {
        t.Fatalf("Expected error to mention %q but error was %q", expectedSubstring, err.Error());
    }
}

internal partial struct TestMarshalNilValue_nilValueTestData /*dyn*/ {
    public any V;
}

public static void TestMarshalNilValue(ж<testing.T> t) {
    var nilValueTestData = slice<any>([
        default,
        new TestMarshalNilValue_nilValueTestData()
    ]);
    foreach (var (i, test) in range(nilValueTestData)) {
        var (_, err) = Marshal(test); if (err == nil) {
            t.Fatalf("#%d: successfully marshaled nil value", i);
        }
    }
}

internal partial struct unexported {
    public nint X;
    internal nint y;
}

internal partial struct exported {
    public nint X;
    public nint Y;
}

public static void TestUnexportedStructField(ж<testing.T> t) {
    var want = new StructuralError("struct contains unexported fields");

    var (_, err) = Marshal(new unexported(X: 5, y: 1));
    if (!AreEqual(err, want)) {
        t.Errorf("got %v, want %v", err, want);
    }

    (var bs, err) = Marshal(new exported(X: 5, Y: 1));
    if (err != nil) {
        t.Fatal(err);
    }
    ref var u = ref heap(new unexported(), out var Ꮡu);
    (_, err) = Unmarshal(bs, Ꮡu);
    if (!AreEqual(err, want)) {
        t.Errorf("got %v, want %v", err, want);
    }
}

public static void TestNull(ж<testing.T> t) {
    var (marshaled, err) = Marshal(NullRawValue);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(NullBytes, marshaled)) {
        t.Errorf("Expected Marshal of NullRawValue to yield %x, got %x", NullBytes, marshaled);
    }

    ref var unmarshaled = ref heap<global::go.encoding.asn1_package.RawValue>(new RawValue(), out var Ꮡunmarshaled);
    var (_, errΔ1) = Unmarshal(NullBytes, Ꮡunmarshaled); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }

    unmarshaled.FullBytes = NullRawValue.FullBytes;
    if (len(unmarshaled.Bytes) == 0) {
        // DeepEqual considers a nil slice and an empty slice to be different.
        unmarshaled.Bytes = NullRawValue.Bytes;
    }

    if (!reflect.DeepEqual(NullRawValue, unmarshaled)) {
        t.Errorf("Expected Unmarshal of NullBytes to yield %v, got %v", NullRawValue, unmarshaled);
    }
}

internal partial struct TestExplicitTagRawValueStruct_foo /*dyn*/ {
    public global::go.encoding.asn1_package.RawValue A; /*`asn1:"optional,explicit,tag:5"`*/
    public slice<byte> B; /*`asn1:"optional,explicit,tag:6"`*/
}

public static void TestExplicitTagRawValueStruct(ж<testing.T> t) {
    var before = new TestExplicitTagRawValueStruct_foo(B: [1, 2, 3]);
    var (derBytes, err) = Marshal(before);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var after = ref heap(new TestExplicitTagRawValueStruct_foo(), out var Ꮡafter);
    var (rest, errΔ1) = Unmarshal(derBytes, Ꮡafter); if (errΔ1 != nil || len(rest) != 0) {
        t.Fatal(errΔ1);
    }

    @string got = fmt.Sprintf("%#v"u8, after);
    @string want = fmt.Sprintf("%#v"u8, before);
    if (got != want) {
        t.Errorf("got %s, want %s (DER: %x)", got, want, derBytes);
    }
}

internal partial struct TestTaggedRawValue_taggedRawValue /*dyn*/ {
    public global::go.encoding.asn1_package.RawValue A; /*`asn1:"tag:5"`*/
}

internal partial struct TestTaggedRawValue_untaggedRawValue /*dyn*/ {
    public global::go.encoding.asn1_package.RawValue A;
}

internal partial struct TestTaggedRawValue_tests /*dyn*/ {
    internal bool shouldMatch;
    internal slice<byte> derBytes;
}

public static void TestTaggedRawValue(ж<testing.T> t) {
    UntypedInt isCompound = 0x20;
    UntypedInt tag = 5;

    var tests = slice<TestTaggedRawValue_tests>([
        new(false, [0x30, 3, TagInteger, 1, 1]),
        new(true, [0x30, 3, (byte)((byte)(ClassContextSpecific << 6) | (byte)tag), 1, 1]),
        new(true, [0x30, 3, (byte)((UntypedInt)((ClassContextSpecific << 6) | tag) | (byte)isCompound), 1, 1]),
        new(false, [0x30, 3, (byte)((UntypedInt)((ClassApplication << 6) | tag) | (byte)isCompound), 1, 1]),
        new(false, [0x30, 3, (byte)((UntypedInt)((ClassPrivate << 6) | tag) | (byte)isCompound), 1, 1])
    ]);

    foreach (var (i, test) in range(tests)) {
        ref var tagged = ref heap(new TestTaggedRawValue_taggedRawValue(), out var Ꮡtagged);
        {
            var (_, err) = Unmarshal(test.derBytes, Ꮡtagged); if ((err == nil) != test.shouldMatch) {
                t.Errorf("#%d: unexpected result parsing %x: %s", i, test.derBytes, err);
            }
        }

        // An untagged RawValue should accept anything.
        ref var untagged = ref heap(new TestTaggedRawValue_untaggedRawValue(), out var Ꮡuntagged);
        {
            var (_, err) = Unmarshal(test.derBytes, Ꮡuntagged); if (err != nil) {
                t.Errorf("#%d: unexpected failure parsing %x with untagged RawValue: %s", i, test.derBytes, err);
            }
        }
    }
}

partial struct bmpStringTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string decoded;
    internal @string encodedHex;
    internal bool invalid;
}
internal static slice<bmpStringTestsᴛ1> bmpStringTests = [
    new("empty string", "", "0000", false),
    // Example from https://tools.ietf.org/html/rfc7292#appendix-B.
    new("rfc7292 example", "Beavis", "0042006500610076006900730000", false),
    // Some characters from the "Letterlike Symbols Unicode block".
    new("letterlike symbols", "\u2115 - Double-struck N", "21150020002d00200044006f00750062006c0065002d00730074007200750063006b0020004e0000", false),
    new("invalid length", "", "ff", true),
    new("invalid surrogate", "", "5051d801", true),
    new("invalid noncharacter 0xfdd1", "", "5051fdd1", true),
    new("invalid noncharacter 0xffff", "", "5051ffff", true),
    new("invalid noncharacter 0xfffe", "", "5051fffe", true)
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object parseBMPStringDidnTFailˢ = (@string)"parseBMPString didn't fail as expected"u8;

public static void TestBMPString(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(bmpStringTests)) {
        ref var test = ref heap(new bmpStringTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var (encoded, err) = hex.DecodeString(testʗ1.encodedHex);
            if (err != nil) {
                tΔ1.Fatalf("failed to decode from hex string: %s", err);
            }

            (var decoded, err) = parseBMPString(encoded);

            if (err != nil && !testʗ1.invalid) {
                tΔ1.Errorf("parseBMPString failed: %s", err);
            } else if (testʗ1.invalid && err == nil) {
                tΔ1.Error(parseBMPStringDidnTFailˢ);
            }

            if (decoded != testʗ1.decoded) {
                tΔ1.Errorf("parseBMPString(%q): got %q, want %q", encoded, decoded, testʗ1.decoded);
            }
        });
    }
}

public static void TestNonMinimalEncodedOID(ж<testing.T> t) {
    var (h, err) = hex.DecodeString("060a2a80864886f70d01010b");
    if (err != nil) {
        t.Fatalf("failed to decode from hex string: %s", err);
    }
    ref var oid = ref heap<global::go.encoding.asn1_package.ObjectIdentifier>(out var Ꮡoid);
    (_, err) = Unmarshal(h, Ꮡoid);
    if (err == nil) {
        t.Fatalf("accepted non-minimally encoded oid");
    }
}

public static void BenchmarkObjectIdentifierString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var oidPublicKeyRSA = new ObjectIdentifier(new nint[]{1, 2, 840, 113549, 1, 1, 1}.slice());
    for (nint i = 0; i < b.N; i++) {
        _ = oidPublicKeyRSA.String();
    }
}

internal partial struct TestImplicitTypeRoundtrip_tagged /*dyn*/ {
    public @string IA5; /*`asn1:"tag:1,ia5"`*/
    public @string Printable; /*`asn1:"tag:2,printable"`*/
    public @string UTF8; /*`asn1:"tag:3,utf8"`*/
    public @string Numeric; /*`asn1:"tag:4,numeric"`*/
    public time.Time UTC; /*`asn1:"tag:5,utc"`*/
    public time.Time Generalized; /*`asn1:"tag:6,generalized"`*/
}

public static void TestImplicitTypeRoundtrip(ж<testing.T> t) {
    var a = new TestImplicitTypeRoundtrip_tagged(
        IA5: "ia5",
        Printable: "printable",
        UTF8: "utf8",
        Numeric: "123 456",
        UTC: time.Now().UTC().Truncate(time.ΔSecond),
        Generalized: time.Now().UTC().Truncate(time.ΔSecond)
    );
    var (enc, err) = Marshal(a);
    if (err != nil) {
        t.Fatalf("Marshal failed: %s", err);
    }
    ref var b = ref heap(new TestImplicitTypeRoundtrip_tagged(), out var Ꮡb);
    var (_, errΔ1) = Unmarshal(enc, Ꮡb); if (errΔ1 != nil) {
        t.Fatalf("Unmarshal failed: %s", errΔ1);
    }

    if (!reflect.DeepEqual(a, b)) {
        t.Fatalf("Unexpected diff after roundtripping struct\na: %#v\nb: %#v", a, b);
    }
}

internal partial struct TestParsingMemoryConsumption_out /*dyn*/ {
    public slice<nint> Id;
    public bool Critical; /*`asn1:"optional"`*/
    public slice<byte> Value;
}

public static void TestParsingMemoryConsumption(ж<testing.T> t) {
    // Craft a syntatically valid, but empty, ~10 MB DER bomb. A successful
    // unmarshal of this bomb should yield ~280 MB. However, the parsing should
    // fail due to the empty content; and, in such cases, we want to make sure
    // that we do not unnecessarily allocate memories.
    var derBomb = new slice<byte>(10_000_000);
    foreach (var i in range(len(derBomb))) {
        derBomb[i] = 0x30;
    }
    derBomb = appendꓸꓸꓸ(slice<byte>([0x30, 0x83, 0x98, 0x96, 0x80]), derBomb);

    ref var m = ref heap(new runtime.MemStats(), out var Ꮡm);
    runtime.GC();
    runtime.ReadMemStats(Ꮡm);
    var memBefore = m.TotalAlloc;

    ref var @out = ref heap<slice<TestParsingMemoryConsumption_out>>(out var Ꮡout);
    var (_, err) = Unmarshal(derBomb, Ꮡout);
    if (!errors.As(err, Ꮡ(new SyntaxError()))) {
        t.Fatalf("Incorrect error result: want (%v), but got (%v) instead", Ꮡ(new SyntaxError()), err);
    }

    runtime.ReadMemStats(Ꮡm);
    var memDiff = m.TotalAlloc - memBefore;

    // Ensure that the memory allocated does not exceed 10<<21 (~20 MB) when
    // the parsing fails.
    if (memDiff > (uint64)10 << 21) {
        t.Errorf("Too much memory allocated while parsing DER: %v MiB", memDiff / 1024 / 1024);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string belowLimitˢ => ref belowLimitˢᵇ;
private static readonly @string belowLimitˢᵇ = "below limit"u8;
internal static ref readonly @string aboveLimitˢ => ref aboveLimitˢᵇ;
private static readonly @string aboveLimitˢᵇ = "above limit"u8;
internal static ref readonly @string asn1StructureErrorˢ => ref asn1StructureErrorˢᵇ;
private static readonly @string asn1StructureErrorˢᵇ = "asn1: structure error: nesting depth exceeded"u8;

internal partial struct TestUnmarshalNestingLimitSlice_Recursive /*[]TestUnmarshalNestingLimitSlice_Recursive*/;

public static void TestUnmarshalNestingLimitSlice(ж<testing.T> t) {
    nint limit = 10000;
    if (runtime.GOARCH == "wasm"u8) {
        limit = 5000;
    }

    slice<byte> makeData(ж<testing.T> tΔ1, nint depth) {
        TestUnmarshalNestingLimitSlice_Recursive r = default;
        foreach (var _ᴛ1 in range(depth - 1)) {
            r = new TestUnmarshalNestingLimitSlice_Recursive(new TestUnmarshalNestingLimitSlice_Recursive[]{r}.slice());
        }
        var (data, err) = Marshal(r);
        if (err != nil) {
            tΔ1.Fatalf("Marshal failed: %v", err);
        }
        return data;
    }

    var makeDataʗ1 = makeData;
    t.Run(belowLimitˢ, (ж<testing.T> tΔ2) => {
        var data = makeDataʗ1(tΔ2, limit);
        ref var r = ref heap<TestUnmarshalNestingLimitSlice_Recursive>(out var Ꮡr);
        var (_, err) = Unmarshal(data, Ꮡr); if (err != nil) {
            tΔ2.Errorf("Unmarshal failed at depth %d: %v", limit, err);
        }
    });

    var makeDataʗ2 = makeData;
    t.Run(aboveLimitˢ, (ж<testing.T> tΔ3) => {
        var data = makeDataʗ2(tΔ3, limit + 1);
        ref var r = ref heap<TestUnmarshalNestingLimitSlice_Recursive>(out var Ꮡr);
        var (_, err) = Unmarshal(data, Ꮡr);
        if (err == nil) {
            tΔ3.Fatalf("Unmarshal succeeded at depth %d, want error", limit + 1);
        }
        @string got = err.Error();
        @string want = asn1StructureErrorˢ; if (got != want) {
            tΔ3.Errorf("Unmarshal error mismatch\ngot:  %q\nwant: %q", got, want);
        }
    });
}

internal partial struct TestUnmarshalNestingLimitStruct_Recursive /*dyn*/ {
    public slice<TestUnmarshalNestingLimitStruct_Recursive> Next; /*`asn1:"optional"`*/
}

// Note that recursive structs fail in half the normal limit because each level
// of nesting in a struct (with a slice field) involves two depth increments
// (one for the struct and one for the slice).
public static void TestUnmarshalNestingLimitStruct(ж<testing.T> t) {
    nint limit = 5000;
    if (runtime.GOARCH == "wasm"u8) {
        limit = 2500;
    }

    slice<byte> makeData(ж<testing.T> tΔ1, nint depth) {
        TestUnmarshalNestingLimitStruct_Recursive r = default;
        foreach (var _ᴛ1 in range(depth - 1)) {
            r = new TestUnmarshalNestingLimitStruct_Recursive(Next: [r]);
        }
        var (data, err) = Marshal(r);
        if (err != nil) {
            tΔ1.Fatalf("Marshal failed: %v", err);
        }
        return data;
    }

    var makeDataʗ1 = makeData;
    t.Run(belowLimitˢ, (ж<testing.T> tΔ2) => {
        var data = makeDataʗ1(tΔ2, limit);
        ref var r = ref heap(new TestUnmarshalNestingLimitStruct_Recursive(), out var Ꮡr);
        var (_, err) = Unmarshal(data, Ꮡr); if (err != nil) {
            tΔ2.Errorf("Unmarshal failed at depth %d: %v", limit, err);
        }
    });

    var makeDataʗ2 = makeData;
    t.Run(aboveLimitˢ, (ж<testing.T> tΔ3) => {
        var data = makeDataʗ2(tΔ3, limit + 1);
        ref var r = ref heap(new TestUnmarshalNestingLimitStruct_Recursive(), out var Ꮡr);
        var (_, err) = Unmarshal(data, Ꮡr);
        if (err == nil) {
            tΔ3.Fatalf("Unmarshal succeeded at depth %d, want error", limit + 1);
        }
        @string got = err.Error();
        @string want = asn1StructureErrorˢ; if (got != want) {
            tΔ3.Errorf("Unmarshal error mismatch\ngot:  %q\nwant: %q", got, want);
        }
    });
}

} // end asn1_internal_test_package
