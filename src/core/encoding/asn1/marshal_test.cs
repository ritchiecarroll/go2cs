// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using hex = go.encoding.hex_package;
using big = go.math.big_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.encoding;
using go.math;
using static go.encoding.asn1_package;

partial class asn1_internal_test_package {

public partial struct intStruct {
    public nint A;
}

internal partial struct twoIntStruct {
    public nint A;
    public nint B;
}

internal partial struct bigIntStruct {
    public ж<bigꓸInt> A;
}

internal partial struct nestedStruct {
    public intStruct A;
}

internal partial struct rawContentsStruct {
    public global::go.encoding.asn1_package.RawContent Raw;
    public nint A;
}

internal partial struct implicitTagTest {
    public nint A; /*`asn1:"implicit,tag:5"`*/
}

internal partial struct explicitTagTest {
    public nint A; /*`asn1:"explicit,tag:5"`*/
}

internal partial struct flagTest {
    public global::go.encoding.asn1_package.Flag A; /*`asn1:"tag:0,optional"`*/
}

internal partial struct generalizedTimeTest {
    public time.Time A; /*`asn1:"generalized"`*/
}

internal partial struct ia5StringTest {
    public @string A; /*`asn1:"ia5"`*/
}

internal partial struct printableStringTest {
    public @string A; /*`asn1:"printable"`*/
}

internal partial struct genericStringTest {
    public @string A;
}

internal partial struct optionalRawValueTest {
    public global::go.encoding.asn1_package.RawValue A; /*`asn1:"optional"`*/
}

internal partial struct omitEmptyTest {
    public slice<@string> A; /*`asn1:"omitempty"`*/
}

internal partial struct defaultTest {
    public nint A; /*`asn1:"optional,default:1"`*/
}

internal partial struct applicationTest {
    public nint A; /*`asn1:"application,tag:0"`*/
    public nint B; /*`asn1:"application,tag:1,explicit"`*/
}

internal partial struct privateTest {
    public nint A; /*`asn1:"private,tag:0"`*/
    public nint B; /*`asn1:"private,tag:1,explicit"`*/
    public nint C; /*`asn1:"private,tag:31"`*/  // tag size should be 2 octet
    public nint D; /*`asn1:"private,tag:128"`*/ // tag size should be 3 octet
}

internal partial struct numericStringTest {
    public @string A; /*`asn1:"numeric"`*/
}

internal partial struct testSET /*[]nint*/;

public static ж<timeꓸLocation> PST = time.FixedZone("PST"u8, -8 * 60 * 60);

internal partial struct marshalTest {
    internal any @in;
    internal @string @out; // hex encoded
}

internal static time.Time farFuture() {
    var (t, err) = time.Parse(time.RFC3339, "2100-04-05T12:01:01Z"u8);
    if (err != default!) {
        throw panic(err);
    }
    return t;
}

// This is 127 times 'x'
// This is 128 times 'x'
internal static slice<marshalTest> marshalTests = new marshalTest[]{
    new((nint)(10), "02010a"u8),
    new((nint)(127), "02017f"u8),
    new((nint)(128), "02020080"u8),
    new((nint)(-128), "020180"u8),
    new((nint)(-129), "0202ff7f"u8),
    new(new intStruct(64), "3003020140"u8),
    new(new bigIntStruct(big.NewInt(0x123456)), "30050203123456"u8),
    new(new twoIntStruct(64, 65), "3006020140020141"u8),
    new(new nestedStruct(new intStruct(127)), "3005300302017f"u8),
    new(new byte[]{1, 2, 3}.slice(), "0403010203"u8),
    new(new implicitTagTest(64), "3003850140"u8),
    new(new explicitTagTest(64), "3005a503020140"u8),
    new(new flagTest(true), "30028000"u8),
    new(new flagTest(false), "3000"u8),
    new(time.Unix(0, 0).UTC(), "170d3730303130313030303030305a"u8),
    new(time.Unix(1258325776, 0).UTC(), "170d3039313131353232353631365a"u8),
    new(time.Unix(1258325776, 0).In(PST), "17113039313131353134353631362d30383030"u8),
    new(farFuture(), "180f32313030303430353132303130315a"u8),
    new(new generalizedTimeTest(time.Unix(1258325776, 0).UTC()), "3011180f32303039313131353232353631365a"u8),
    new(new BitString(new byte[]{0x80}.slice(), 1), "03020780"u8),
    new(new BitString(new byte[]{0x81, 0xf0}.slice(), 12), "03030481f0"u8),
    new(((global::go.encoding.asn1_package.ObjectIdentifier)new nint[]{1, 2, 3, 4}.slice()), "06032a0304"u8),
    new(((global::go.encoding.asn1_package.ObjectIdentifier)new nint[]{1, 2, 840, 133549, 1, 1, 5}.slice()), "06092a864888932d010105"u8),
    new(((global::go.encoding.asn1_package.ObjectIdentifier)new nint[]{2, 100, 3}.slice()), "0603813403"u8),
    new((@string)"test"u8, "130474657374"u8),
    new(
        (@string)(""u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8),
        "137f"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "78787878787878787878787878787878787878787878787878787878787878"u8
    ),
    new(
        (@string)(""u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8),
        "138180"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8
    ),
    new(new ia5StringTest("test"u8), "3006160474657374"u8),
    new(new optionalRawValueTest(nil), "3000"u8),
    new(new printableStringTest("test"u8), "3006130474657374"u8),
    new(new printableStringTest("test*"u8), "30071305746573742a"u8),
    new(new genericStringTest("test"u8), "3006130474657374"u8),
    new(new genericStringTest("test*"u8), "30070c05746573742a"u8),
    new(new genericStringTest("test&"u8), "30070c057465737426"u8),
    new(new rawContentsStruct(default!, 64), "3003020140"u8),
    new(new rawContentsStruct(new byte[]{0x30, 3, 1, 2, 3}.slice(), 64), "3003010203"u8),
    new(new RawValue(Tag: 1, Class: 2, IsCompound: false, Bytes: new byte[]{1, 2, 3}.slice()), "8103010203"u8),
    new(((testSET)new nint[]{10}.slice()), "310302010a"u8),
    new(new omitEmptyTest(new @string[]{}.slice()), "3000"u8),
    new(new omitEmptyTest(new @string[]{"1"u8}.slice()), "30053003130131"u8),
    new((@string)"Σ"u8, "0c02cea3"u8),
    new(new defaultTest(0), "3003020100"u8),
    new(new defaultTest(1), "3000"u8),
    new(new defaultTest(2), "3003020102"u8),
    new(new applicationTest(1, 2), "30084001016103020102"u8),
    new(new privateTest(1, 2, 3, 4), "3011c00101e103020102df1f0103df81000104"u8),
    new(new numericStringTest("1 9"u8), "30051203312039"u8)
}.slice();

public static void TestMarshal(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, test) in marshalTests) {
        var (data, err) = Marshal(test.@in);
        if (err != default!) {
            Ꮡt.Errorf("#%d failed: %s"u8, i, err);
        }
        var (@out, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(@out, data)) {
            Ꮡt.Errorf("#%d got: %x want %x\n\t%q\n\t%q"u8, i, data, @out, data, @out);
        }
    }
}

internal partial struct marshalWithParamsTest {
    internal any @in;
    internal @string @params;
    internal @string @out; // hex encoded
}

internal static slice<marshalWithParamsTest> marshalWithParamsTests = new marshalWithParamsTest[]{
    new(new intStruct(10), "set"u8, "310302010a"u8),
    new(new intStruct(10), "application"u8, "600302010a"u8),
    new(new intStruct(10), "private"u8, "e00302010a"u8)
}.slice();

public static void TestMarshalWithParams(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, test) in marshalWithParamsTests) {
        var (data, err) = MarshalWithParams(test.@in, test.@params);
        if (err != default!) {
            Ꮡt.Errorf("#%d failed: %s"u8, i, err);
        }
        var (@out, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(@out, data)) {
            Ꮡt.Errorf("#%d got: %x want %x\n\t%q\n\t%q"u8, i, data, @out, data, @out);
        }
    }
}

internal partial struct marshalErrTest {
    internal any @in;
    internal @string err;
}

internal static slice<marshalErrTest> marshalErrTests = new marshalErrTest[]{
    new(new bigIntStruct(nil), "empty integer"u8),
    new(new numericStringTest("a"u8), "invalid character"u8),
    new(new ia5StringTest(((@string)(new byte[]{0xb0}))), "invalid character"u8),
    new(new printableStringTest("!"u8), "invalid character"u8)
}.slice();

public static void TestMarshalError(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in marshalErrTests) {
        var (_, err) = Marshal(test.@in);
        if (err == default!) {
            Ꮡt.Errorf("#%d should fail, but success"u8, i);
            continue;
        }
        if (!strings.Contains(err.Error(), test.err)) {
            Ꮡt.Errorf("#%d got: %v want %v"u8, i, err, test.err);
        }
    }
}

public static void TestInvalidUTF8(ж<testing.T> Ꮡt) {
    var (_, err) = Marshal(((@string)new byte[]{0xff, 0xff}.slice()));
    if (err == default!) {
        Ꮡt.Errorf("invalid UTF8 string was accepted"u8);
    }
}

public static void TestMarshalOID(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// bytes format returns a byte sequence \x04
// {ObjectIdentifier([]int{0}), "060100"}, // returns an error as OID 0.0 has the same encoding
// same as above "\x06\x010" = "\x06\x01" + "0"
// Example of ITU-T X.690
// zero OID
    slice<marshalTest> marshalTestsOID = new marshalTest[]{
        new(slice<byte>("\x06\x01\x30"u8), "0403060130"u8),
        new(slice<byte>(((@string)(new byte[]{0x06, 0x01, 0x30}))), "0403060130"u8),
        new(((global::go.encoding.asn1_package.ObjectIdentifier)new nint[]{2, 999, 3}.slice()), "0603883703"u8),
        new(((global::go.encoding.asn1_package.ObjectIdentifier)new nint[]{0, 0}.slice()), "060100"u8)
    }.slice();
    foreach (var (i, test) in marshalTestsOID) {
        var (data, err) = Marshal(test.@in);
        if (err != default!) {
            Ꮡt.Errorf("#%d failed: %s"u8, i, err);
        }
        var (@out, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(@out, data)) {
            Ꮡt.Errorf("#%d got: %x want %x\n\t%q\n\t%q"u8, i, data, @out, data, @out);
        }
    }
}

public static void TestIssue11130(ж<testing.T> Ꮡt) {
    var data = slice<byte>(((@string)(new byte[]{0x06, 0x01, 0x30}))); // == \x06\x01\x30 == OID = 0 (the figure)
    ref var v = ref heap<any>(out var Ꮡv);
    // v has Zero value here and Elem() would panic
    var (_, err) = Unmarshal(data, Ꮡv);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
        return;
    }
    if (reflect.TypeOf(v).String() != reflect.TypeOf(new ObjectIdentifier(new nint[]{}.slice())).String()) {
        Ꮡt.Errorf("marshal OID returned an invalid type"u8);
        return;
    }
    (var data1, err) = Marshal(v);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
        return;
    }
    if (!bytes.Equal(data, data1)) {
        Ꮡt.Errorf("got: %q, want: %q \n"u8, data1, data);
        return;
    }
    ref var v1 = ref heap<any>(out var Ꮡv1);
    (_, err) = Unmarshal(data1, Ꮡv1);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
        return;
    }
    if (!reflect.DeepEqual(v, v1)) {
        Ꮡt.Errorf("got: %#v data=%q, want : %#v data=%q\n "u8, v1, data1, v, data);
    }
}

public static void TestIssue68241(ж<testing.T> Ꮡt) {
    foreach (var (i, want) in new any[]{false, true}.slice()) {
        var (data, err) = Marshal(want);
        if (err != default!) {
            Ꮡt.Errorf("cannot Marshal: %v"u8, err);
            return;
        }
        ref var got = ref heap<any>(out var Ꮡgot);
        (_, err) = Unmarshal(data, Ꮡgot);
        if (err != default!) {
            Ꮡt.Errorf("cannot Unmarshal: %v"u8, err);
            return;
        }
        if (!reflect.DeepEqual(got, want)) {
            Ꮡt.Errorf("#%d Unmarshal, got: %v, want: %v"u8, i, got, want);
        }
    }
}

public static void BenchmarkMarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, test) in marshalTests) {
            Marshal(test.@in);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unmarshalReturnedExtraˢ = (@string)"Unmarshal returned extra garbage"u8;

internal partial struct TestSetEncoder_testStruct /*dyn*/ {
    public slice<@string> Strings; /*`asn1:"set"`*/
}

public static void TestSetEncoder(ж<testing.T> Ꮡt) {
    var testStruct = new TestSetEncoder_testStruct(
        Strings: new @string[]{"a"u8, "aa"u8, "b"u8, "bb"u8, "c"u8, "cc"u8}.slice()
    );
    // Expected ordering of the SET should be:
    // a, b, c, aa, bb, cc
    var (output, err) = Marshal(testStruct);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
    }
    var expectedOrder = new @string[]{"a"u8, "b"u8, "c"u8, "aa"u8, "bb"u8, "cc"u8}.slice();
    ref var resultStruct = ref heap(new TestSetEncoder_testStruct(), out var ᏑresultStruct);
    (var rest, err) = Unmarshal(output, ᏑresultStruct);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
    }
    if (len(rest) != 0) {
        Ꮡt.Error(unmarshalReturnedExtraˢ);
    }
    if (!slices.Equal<slice<@string>, @string>(expectedOrder, resultStruct.Strings)) {
        Ꮡt.Errorf("Unexpected SET content. got: %s, want: %s"u8, resultStruct.Strings, expectedOrder);
    }
}

internal partial struct TestSetEncoderSETSliceSuffix_testSetSET /*[]@string*/;

public static void TestSetEncoderSETSliceSuffix(ж<testing.T> Ꮡt) {
    var testSet = new TestSetEncoderSETSliceSuffix_testSetSET(new @string[]{"a"u8, "aa"u8, "b"u8, "bb"u8, "c"u8, "cc"u8}.slice());
    // Expected ordering of the SET should be:
    // a, b, c, aa, bb, cc
    var (output, err) = Marshal(testSet);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
    }
    var expectedOrder = new TestSetEncoderSETSliceSuffix_testSetSET(new @string[]{"a"u8, "b"u8, "c"u8, "aa"u8, "bb"u8, "cc"u8}.slice());
    ref var resultSet = ref heap<TestSetEncoderSETSliceSuffix_testSetSET>(out var ᏑresultSet);
    (var rest, err) = Unmarshal(output, ᏑresultSet);
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
    }
    if (len(rest) != 0) {
        Ꮡt.Error(unmarshalReturnedExtraˢ);
    }
    if (!reflect.DeepEqual(expectedOrder, resultSet)) {
        Ꮡt.Errorf("Unexpected SET content. got: %s, want: %s"u8, resultSet, expectedOrder);
    }
}

internal partial struct BenchmarkUnmarshal_testCase /*dyn*/ {
    internal slice<byte> @in;
    internal any @out;
}

public static void BenchmarkUnmarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    slice<BenchmarkUnmarshal_testCase> testData = default!;
    foreach (var (_, test) in unmarshalTestData) {
        var pv = reflect.New(reflect.TypeOf(test.@out).Elem());
        var inCopy = new slice<byte>(len(test.@in));
        copy(inCopy, test.@in);
        var outCopy = pv.Interface();
        testData = append(testData, new BenchmarkUnmarshal_testCase(
            @in: inCopy,
            @out: outCopy
        ));
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, testCase) in testData) {
            (_, _) = Unmarshal(testCase.@in, testCase.@out);
        }
    }
}

} // end asn1_internal_test_package
