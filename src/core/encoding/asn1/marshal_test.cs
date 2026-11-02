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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
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

public static ж<timeꓸLocation> PST = time.FixedZone("PST", -8 * 60 * 60);

internal partial struct marshalTest {
    internal any @in;
    internal @string @out; // hex encoded
}

internal static time.Time farFuture() {
    var (t, err) = time.Parse(time.RFC3339, "2100-04-05T12:01:01Z");
    if (err != nil) {
        throw panic(err);
    }
    return t;
}

internal static slice<marshalTest> marshalTests = slice<marshalTest>([
    new((nint)10, "02010a"),
    new((nint)127, "02017f"),
    new((nint)128, "02020080"),
    new((nint)(-128), "020180"),
    new((nint)(-129), "0202ff7f"),
    new(new intStruct(64), "3003020140"),
    new(new bigIntStruct(big.NewInt(0x123456)), "30050203123456"),
    new(new twoIntStruct(64, 65), "3006020140020141"),
    new(new nestedStruct(new intStruct(127)), "3005300302017f"),
    new(slice<byte>([1, 2, 3]), "0403010203"),
    new(new implicitTagTest(64), "3003850140"),
    new(new explicitTagTest(64), "3005a503020140"),
    new(new flagTest(true), "30028000"),
    new(new flagTest(false), "3000"),
    new(time.Unix(0, 0).UTC(), "170d3730303130313030303030305a"),
    new(time.Unix(1258325776, 0).UTC(), "170d3039313131353232353631365a"),
    new(time.Unix(1258325776, 0).In(PST), "17113039313131353134353631362d30383030"),
    new(farFuture(), "180f32313030303430353132303130315a"),
    new(new generalizedTimeTest(time.Unix(1258325776, 0).UTC()), "3011180f32303039313131353232353631365a"),
    new(new BitString(slice<byte>([0x80]), 1), "03020780"),
    new(new BitString(slice<byte>([0x81, 0xf0]), 12), "03030481f0"),
    new((global::go.encoding.asn1_package.ObjectIdentifier)slice<nint>([1, 2, 3, 4]), "06032a0304"),
    new((global::go.encoding.asn1_package.ObjectIdentifier)slice<nint>([1, 2, 840, 133549, 1, 1, 5]), "06092a864888932d010105"),
    new((global::go.encoding.asn1_package.ObjectIdentifier)slice<nint>([2, 100, 3]), "0603813403"),
    new((@string)"test", "130474657374"),
    new(
        (@string)(""u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8), // This is 127 times 'x'
        "137f"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "78787878787878787878787878787878787878787878787878787878787878"u8
    ),
    new(
        (@string)(""u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8 + "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"u8), // This is 128 times 'x'
        "138180"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8 + "7878787878787878787878787878787878787878787878787878787878787878"u8
    ),
    new(new ia5StringTest("test"), "3006160474657374"),
    new(new optionalRawValueTest(), "3000"),
    new(new printableStringTest("test"), "3006130474657374"),
    new(new printableStringTest("test*"), "30071305746573742a"),
    new(new genericStringTest("test"), "3006130474657374"),
    new(new genericStringTest("test*"), "30070c05746573742a"),
    new(new genericStringTest("test&"), "30070c057465737426"),
    new(new rawContentsStruct(default, 64), "3003020140"),
    new(new rawContentsStruct(slice<byte>([0x30, 3, 1, 2, 3]), 64), "3003010203"),
    new(new RawValue(Tag: 1, Class: 2, IsCompound: false, Bytes: slice<byte>([1, 2, 3])), "8103010203"),
    new((testSET)slice<nint>([10]), "310302010a"),
    new(new omitEmptyTest(slice<@string>([])), "3000"),
    new(new omitEmptyTest(slice<@string>(["1"])), "30053003130131"),
    new((@string)"Σ", "0c02cea3"),
    new(new defaultTest(0), "3003020100"),
    new(new defaultTest(1), "3000"),
    new(new defaultTest(2), "3003020102"),
    new(new applicationTest(1, 2), "30084001016103020102"),
    new(new privateTest(1, 2, 3, 4), "3011c00101e103020102df1f0103df81000104"),
    new(new numericStringTest("1 9"), "30051203312039")
]);

public static void TestMarshal(ж<testing.T> t) {
    foreach (var (i, test) in marshalTests) {
        var (data, err) = Marshal(test.@in);
        if (err != nil) {
            t.Errorf("#%d failed: %s", i, err);
        }
        var (@out, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(@out, data)) {
            t.Errorf("#%d got: %x want %x\n\t%q\n\t%q", i, data, @out, data, @out);
        }
    }
}

internal partial struct marshalWithParamsTest {
    internal any @in;
    internal @string @params;
    internal @string @out; // hex encoded
}

internal static slice<marshalWithParamsTest> marshalWithParamsTests = slice<marshalWithParamsTest>([
    new(new intStruct(10), "set", "310302010a"),
    new(new intStruct(10), "application", "600302010a"),
    new(new intStruct(10), "private", "e00302010a")
]);

public static void TestMarshalWithParams(ж<testing.T> t) {
    foreach (var (i, test) in marshalWithParamsTests) {
        var (data, err) = MarshalWithParams(test.@in, test.@params);
        if (err != nil) {
            t.Errorf("#%d failed: %s", i, err);
        }
        var (@out, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(@out, data)) {
            t.Errorf("#%d got: %x want %x\n\t%q\n\t%q", i, data, @out, data, @out);
        }
    }
}

internal partial struct marshalErrTest {
    internal any @in;
    internal @string err;
}

internal static slice<marshalErrTest> marshalErrTests = slice<marshalErrTest>([
    new(new bigIntStruct(nil), "empty integer"),
    new(new numericStringTest("a"), "invalid character"),
    new(new ia5StringTest(((@string)(new byte[]{0xb0}))), "invalid character"),
    new(new printableStringTest("!"), "invalid character")
]);

public static void TestMarshalError(ж<testing.T> t) {
    foreach (var (i, test) in marshalErrTests) {
        var (_, err) = Marshal(test.@in);
        if (err == nil) {
            t.Errorf("#%d should fail, but success", i);
            continue;
        }

        if (!strings.Contains(err.Error(), test.err)) {
            t.Errorf("#%d got: %v want %v", i, err, test.err);
        }
    }
}

public static partial void TestInvalidUTF8(ж<testing.T> t) {
    var (_, err) = Marshal((@string)slice<byte>([0xff, 0xff]));
    if (err == nil) {
        t.Errorf("invalid UTF8 string was accepted");
    }
}

public static void TestMarshalOID(ж<testing.T> t) {
    slice<marshalTest> marshalTestsOID = slice<marshalTest>([
        new(slice<byte>("\x06\x01\x30"u8), "0403060130"), // bytes format returns a byte sequence \x04
        // {ObjectIdentifier([]int{0}), "060100"}, // returns an error as OID 0.0 has the same encoding
        new(slice<byte>(((@string)(new byte[]{0x06, 0x01, 0x30}))), "0403060130"),                      // same as above "\x06\x010" = "\x06\x01" + "0"
        new((global::go.encoding.asn1_package.ObjectIdentifier)slice<nint>([2, 999, 3]), "0603883703"), // Example of ITU-T X.690
        new((global::go.encoding.asn1_package.ObjectIdentifier)slice<nint>([0, 0]), "060100")           // zero OID
    ]);
    foreach (var (i, test) in marshalTestsOID) {
        var (data, err) = Marshal(test.@in);
        if (err != nil) {
            t.Errorf("#%d failed: %s", i, err);
        }
        var (@out, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(@out, data)) {
            t.Errorf("#%d got: %x want %x\n\t%q\n\t%q", i, data, @out, data, @out);
        }
    }
}

public static void TestIssue11130(ж<testing.T> t) {
    var data = slice<byte>(((@string)(new byte[]{0x06, 0x01, 0x30}))); // == \x06\x01\x30 == OID = 0 (the figure)
    ref var v = ref heap<any>(out var Ꮡv);
    // v has Zero value here and Elem() would panic
    var (_, err) = Unmarshal(data, Ꮡv);
    if (err != nil) {
        t.Errorf("%v", err);
        return;
    }
    if (reflect.TypeOf(v).String() != reflect.TypeOf(new ObjectIdentifier(new nint[]{}.slice())).String()) {
        t.Errorf("marshal OID returned an invalid type");
        return;
    }

    (var data1, err) = Marshal(v);
    if (err != nil) {
        t.Errorf("%v", err);
        return;
    }

    if (!bytes.Equal(data, data1)) {
        t.Errorf("got: %q, want: %q \n", data1, data);
        return;
    }

    ref var v1 = ref heap<any>(out var Ꮡv1);
    (_, err) = Unmarshal(data1, Ꮡv1);
    if (err != nil) {
        t.Errorf("%v", err);
        return;
    }
    if (!reflect.DeepEqual(v, v1)) {
        t.Errorf("got: %#v data=%q, want : %#v data=%q\n ", v1, data1, v, data);
    }
}

public static void TestIssue68241(ж<testing.T> t) {
    foreach (var (i, want) in slice<any>([false, true])) {
        var (data, err) = Marshal(want);
        if (err != nil) {
            t.Errorf("cannot Marshal: %v", err);
            return;
        }

        ref var got = ref heap<any>(out var Ꮡgot);
        (_, err) = Unmarshal(data, Ꮡgot);
        if (err != nil) {
            t.Errorf("cannot Unmarshal: %v", err);
            return;
        }
        if (!reflect.DeepEqual(got, want)) {
            t.Errorf("#%d Unmarshal, got: %v, want: %v", i, got, want);
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

public static void TestSetEncoder(ж<testing.T> t) {
    var testStruct = new TestSetEncoder_testStruct(
        Strings: slice<@string>(["a", "aa", "b", "bb", "c", "cc"])
    );

    // Expected ordering of the SET should be:
    // a, b, c, aa, bb, cc

    var (output, err) = Marshal(testStruct);
    if (err != nil) {
        t.Errorf("%v", err);
    }

    var expectedOrder = slice<@string>(["a", "b", "c", "aa", "bb", "cc"]);
    ref var resultStruct = ref heap(new TestSetEncoder_testStruct(), out var ᏑresultStruct);
    (var rest, err) = Unmarshal(output, ᏑresultStruct);
    if (err != nil) {
        t.Errorf("%v", err);
    }
    if (len(rest) != 0) {
        t.Error(unmarshalReturnedExtraˢ);
    }
    if (!slices.Equal<slice<@string>, @string>(expectedOrder, resultStruct.Strings)) {
        t.Errorf("Unexpected SET content. got: %s, want: %s", resultStruct.Strings, expectedOrder);
    }
}

internal partial struct TestSetEncoderSETSliceSuffix_testSetSET /*[]@string*/;

public static void TestSetEncoderSETSliceSuffix(ж<testing.T> t) {
    var testSet = new TestSetEncoderSETSliceSuffix_testSetSET(new @string[]{"a", "aa", "b", "bb", "c", "cc"}.slice());

    // Expected ordering of the SET should be:
    // a, b, c, aa, bb, cc

    var (output, err) = Marshal(testSet);
    if (err != nil) {
        t.Errorf("%v", err);
    }

    var expectedOrder = new TestSetEncoderSETSliceSuffix_testSetSET(new @string[]{"a", "b", "c", "aa", "bb", "cc"}.slice());
    ref var resultSet = ref heap<TestSetEncoderSETSliceSuffix_testSetSET>(out var ᏑresultSet);
    (var rest, err) = Unmarshal(output, ᏑresultSet);
    if (err != nil) {
        t.Errorf("%v", err);
    }
    if (len(rest) != 0) {
        t.Error(unmarshalReturnedExtraˢ);
    }
    if (!reflect.DeepEqual(expectedOrder, resultSet)) {
        t.Errorf("Unexpected SET content. got: %s, want: %s", resultSet, expectedOrder);
    }
}

internal partial struct BenchmarkUnmarshal_testCase /*dyn*/ {
    internal slice<byte> @in;
    internal any @out;
}

public static void BenchmarkUnmarshal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    slice<BenchmarkUnmarshal_testCase> testData = default;
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
