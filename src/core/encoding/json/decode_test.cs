// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using image = image_package;
using maps = maps_package;
using math = math_package;
using big = go.math.big_package;
using net = net_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using go.math;
using io = io_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

[GoType] public partial struct T {
    public @string X;
    public nint Y;
    [GoTag(@"json:""-""")]
    public nint Z;
}

[GoType] public partial struct U {
    [GoTag(@"json:""alpha""")]
    public @string Alphabet;
}

[GoType] public partial struct V {
    public any F1;
    public int32 F2;
    public global::go.encoding.json_package.Number F3;
    public ж<VOuter> F4;
}

[GoType] public partial struct VOuter {
    public V V;
}

[GoType] public partial struct W {
    public SS S;
}

[GoType] public partial struct P {
    public PP PP;
}

[GoType] public partial struct PP {
    public T T;
    public slice<T> Ts;
}

[GoType("@string")] public partial struct SS;

[GoRecv] public static error UnmarshalJSON(this ref SS _, slice<byte> data) {
    return new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number"u8, Type: reflect.TypeFor<SS>())));
}

[GoType("T")] public partial struct TAlias;

[GoRecv] public static error UnmarshalJSON(this ref TAlias tt, slice<byte> data) {
    ref var t = ref heap<T>(out var Ꮡt);
    t = new T(nil);
    {
        var err = Unmarshal(data, Ꮡt); if (err != default!) {
            return err;
        }
    }
    tt = ((TAlias)t);
    return default!;
}

[GoType] public partial struct TOuter {
    public TAlias T;
}

// ifaceNumAsFloat64/ifaceNumAsNumber are used to test unmarshaling with and
// without UseNumber
internal static map<@string, any> ifaceNumAsFloat64 = new map<@string, any>{
    ["k1"u8] = (float64)1D,
    ["k2"u8] = (@string)"s"u8,
    ["k3"u8] = new any[]{(float64)1D, (float64)2.0D, (float64)3e-3D}.slice(),
    ["k4"u8] = new map<@string, any>{["kk1"u8] = (@string)"s"u8, ["kk2"u8] = (float64)2D}
};

internal static map<@string, any> ifaceNumAsNumber = new map<@string, any>{
    ["k1"u8] = ((global::go.encoding.json_package.Number)(@string)"1"u8),
    ["k2"u8] = (@string)"s"u8,
    ["k3"u8] = new any[]{((global::go.encoding.json_package.Number)(@string)"1"u8), ((global::go.encoding.json_package.Number)(@string)"2.0"u8), ((global::go.encoding.json_package.Number)(@string)"3e-3"u8)}.slice(),
    ["k4"u8] = new map<@string, any>{["kk1"u8] = (@string)"s"u8, ["kk2"u8] = ((global::go.encoding.json_package.Number)(@string)"2"u8)}
};

[GoType] internal partial struct tx {
    internal nint x;
}

[GoType("num:uint8")] internal partial struct u8;

// A type that can unmarshal itself.
[GoType] public partial struct unmarshaler {
    public bool T;
}

[GoRecv] public static error UnmarshalJSON(this ref unmarshaler u, slice<byte> b) {
    u = new unmarshaler(true); // All we need to see that UnmarshalJSON is called.
    return default!;
}

[GoType] internal partial struct ustruct {
    public unmarshaler M;
}

[GoType] public partial struct unmarshalerText {
    public @string A, B;
}

// needed for re-marshaling tests
public static (slice<byte>, error) MarshalText(this unmarshalerText u) {
    return (slice<byte>(u.A + ":" + u.B), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingSeparatorˢ = "missing separator"u8;

[GoRecv] public static error UnmarshalText(this ref unmarshalerText u, slice<byte> b) {
    nint pos = bytes.IndexByte(b, (rune)':');
    if (pos == -1) {
        return errors.New(missingSeparatorˢ);
    }
    (u.A, u.B) = (((@string)(b.slice(0, pos))), ((@string)(b.slice(pos + 1))));
    return default!;
}

internal static encoding.TextUnmarshaler _ᴛ1ʗ = new json_internal_test_package.unmarshalerTextжTextUnmarshaler(((ж<unmarshalerText>)nil));

[GoType] internal partial struct ustructText {
    public unmarshalerText M;
}

[GoType("num:uint8")] internal partial struct u8marshal;

internal static (slice<byte>, error) MarshalText(this u8marshal u8) {
    return (slice<byte>(fmt.Sprintf("u%d"u8, u8)), default!);
}

internal static error errMissingU8Prefix = errors.New("missing 'u' prefix"u8);

[GoRecv] internal static error UnmarshalText(this ref u8marshal u8, slice<byte> b) {
    if (!bytes.HasPrefix(b, new byte[]{(rune)'u'}.slice())) {
        return errMissingU8Prefix;
    }
    var (n, err) = strconv.Atoi(((@string)(b[1..])));
    if (err != default!) {
        return err;
    }
    u8 = ((u8marshal)(uint8)n);
    return default!;
}

internal static encoding.TextUnmarshaler _ᴛ2ʗ = new json_internal_test_package.u8marshalжTextUnmarshaler(((ж<u8marshal>)nil));

internal static ж<unmarshaler> Ꮡumtrue = new StandardBox<unmarshaler>(new unmarshaler(true));
internal static ref unmarshaler umtrue => ref Ꮡumtrue.Value;
internal static ж<slice<unmarshaler>> Ꮡumslice = new StandardBox<slice<unmarshaler>>(new unmarshaler[]{new(true)}.slice());
internal static ref slice<unmarshaler> umslice => ref Ꮡumslice.ValueSlot;
internal static ustruct umstruct = new ustruct(new unmarshaler(true));
internal static ж<unmarshalerText> ᏑumtrueXY = new StandardBox<unmarshalerText>(new unmarshalerText("x"u8, "y"u8));
internal static ref unmarshalerText umtrueXY => ref ᏑumtrueXY.Value;
internal static ж<slice<unmarshalerText>> ᏑumsliceXY = new StandardBox<slice<unmarshalerText>>(new unmarshalerText[]{new("x"u8, "y"u8)}.slice());
internal static ref slice<unmarshalerText> umsliceXY => ref ᏑumsliceXY.ValueSlot;
internal static ustructText umstructXY = new ustructText(new unmarshalerText("x"u8, "y"u8));
internal static map<unmarshalerText, bool> ummapXY = new map<unmarshalerText, bool>{[new("x"u8, "y"u8)] = true};

// Test data structures for anonymous fields.
[GoType] public partial struct Point {
    public nint Z;
}

[GoType] public partial struct Top {
    public nint Level0;
    public partial ref Embed0 Embed0 { get; }
    public partial ref ж<Embed0a> Embed0a { get; }
    [GoTag(@"json:""e,omitempty""")]
    public partial ref ж<Embed0b> Embed0b { get; }                   // treated as named
    [GoTag(@"json:""-""")]
    public partial ref Embed0c Embed0c { get; }                       // ignored
    public partial ref Loop Loop { get; }
    public partial ref Embed0p Embed0p { get; } // has Point with X, Y, used
    public partial ref Embed0q Embed0q { get; } // has Point with Z, used
    internal partial ref embed embed { get; }   // contains exported field
}

[GoType] public partial struct Embed0 {
    public nint Level1a; // overridden by Embed0a's Level1a with json tag
    public nint Level1b; // used because Embed0a's Level1b is renamed
    public nint Level1c; // used because Embed0a's Level1c is ignored
    public nint Level1d; // annihilated by Embed0a's Level1d
    [GoTag(@"json:""x""")]
    public nint Level1e;           // annihilated by Embed0a.Level1e
}

[GoType] public partial struct Embed0a {
    [GoTag(@"json:""Level1a,omitempty""")]
    public nint Level1a;
    [GoTag(@"json:""LEVEL1B,omitempty""")]
    public nint Level1b;
    [GoTag(@"json:""-""")]
    public nint Level1c;
    public nint Level1d; // annihilated by Embed0's Level1d
    [GoTag(@"json:""x""")]
    public nint Level1f;           // annihilated by Embed0's Level1e
}

[GoType("Embed0")] public partial struct Embed0b;

[GoType("Embed0")] public partial struct Embed0c;

[GoType] public partial struct Embed0p {
    public partial ref image_package.Point Point { get; }
}

[GoType] public partial struct Embed0q {
    public partial ref Point Point { get; }
}

[GoType] internal partial struct embed {
    public nint Q;
}

[GoType] public partial struct Loop {
    [GoTag(@"json:"",omitempty""")]
    public nint Loop1;
    [GoTag(@"json:"",omitempty""")]
    public nint Loop2;
    public partial ref ж<Loop> ΔLoop { get; }
}

// From reflect test:
// The X in S6 and S7 annihilate, but they also block the X in S8.S9.
[GoType] public partial struct S5 {
    public partial ref S6 S6 { get; }
    public partial ref S7 S7 { get; }
    public partial ref S8 S8 { get; }
}

[GoType] public partial struct S6 {
    public nint X;
}

[GoType("S6")] public partial struct S7;

[GoType] public partial struct S8 {
    public partial ref S9 S9 { get; }
}

[GoType] public partial struct S9 {
    public nint X;
    public nint Y;
}

// From reflect test:
// The X in S11.S6 and S12.S6 annihilate, but they also block the X in S13.S8.S9.
[GoType] public partial struct S10 {
    public partial ref S11 S11 { get; }
    public partial ref S12 S12 { get; }
    public partial ref S13 S13 { get; }
}

[GoType] public partial struct S11 {
    public partial ref S6 S6 { get; }
}

[GoType] public partial struct S12 {
    public partial ref S6 S6 { get; }
}

[GoType] public partial struct S13 {
    public partial ref S8 S8 { get; }
}

[GoType] public partial struct Ambig {
    // Given "hello", the first match should win.
    [GoTag(@"json:""HELLO""")]
    public nint First;
    [GoTag(@"json:""Hello""")]
    public nint Second;
}

[GoType] public partial struct XYZ {
    public any X;
    public any Y;
    public any Z;
}

[GoType] internal partial struct unexportedWithMethods {
}

internal static void F(this unexportedWithMethods _) {
}

[GoType("num:byte")] internal partial struct byteWithMarshalJSON;

internal static (slice<byte>, error) MarshalJSON(this byteWithMarshalJSON b) {
    return (slice<byte>(fmt.Sprintf(@"""Z%.2x"""u8, (byte)b)), default!);
}

[GoRecv] internal static error UnmarshalJSON(this ref byteWithMarshalJSON b, slice<byte> data) {
    if (len(data) != 5 || data[0] != (rune)'"' || data[1] != (rune)'Z' || data[4] != (rune)'"') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt(((@string)(data[2..4])), 16, 8);
    if (err != default!) {
        return fmt.Errorf("bad hex"u8);
    }
    b = ((byteWithMarshalJSON)(byte)i);
    return default!;
}

[GoType("num:byte")] internal partial struct byteWithPtrMarshalJSON;

[GoRecv] internal static (slice<byte>, error) MarshalJSON(this ref byteWithPtrMarshalJSON b) {
    return ((byteWithMarshalJSON)(byte)(b)).MarshalJSON();
}

internal static error UnmarshalJSON(this ж<byteWithPtrMarshalJSON> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<byteWithPtrMarshalJSON, byteWithMarshalJSON>()).UnmarshalJSON(data);
}

[GoType("num:byte")] internal partial struct byteWithMarshalText;

internal static (slice<byte>, error) MarshalText(this byteWithMarshalText b) {
    return (slice<byte>(fmt.Sprintf(@"Z%.2x"u8, (byte)b)), default!);
}

[GoRecv] internal static error UnmarshalText(this ref byteWithMarshalText b, slice<byte> data) {
    if (len(data) != 3 || data[0] != (rune)'Z') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt(((@string)(data[1..3])), 16, 8);
    if (err != default!) {
        return fmt.Errorf("bad hex"u8);
    }
    b = ((byteWithMarshalText)(byte)i);
    return default!;
}

[GoType("num:byte")] internal partial struct byteWithPtrMarshalText;

[GoRecv] internal static (slice<byte>, error) MarshalText(this ref byteWithPtrMarshalText b) {
    return ((byteWithMarshalText)(byte)(b)).MarshalText();
}

internal static error UnmarshalText(this ж<byteWithPtrMarshalText> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<byteWithPtrMarshalText, byteWithMarshalText>()).UnmarshalText(data);
}

[GoType("num:nint")] internal partial struct intWithMarshalJSON;

internal static (slice<byte>, error) MarshalJSON(this intWithMarshalJSON b) {
    return (slice<byte>(fmt.Sprintf(@"""Z%.2x"""u8, (nint)b)), default!);
}

[GoRecv] internal static error UnmarshalJSON(this ref intWithMarshalJSON b, slice<byte> data) {
    if (len(data) != 5 || data[0] != (rune)'"' || data[1] != (rune)'Z' || data[4] != (rune)'"') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt(((@string)(data[2..4])), 16, 8);
    if (err != default!) {
        return fmt.Errorf("bad hex"u8);
    }
    b = ((intWithMarshalJSON)(nint)i);
    return default!;
}

[GoType("num:nint")] internal partial struct intWithPtrMarshalJSON;

[GoRecv] internal static (slice<byte>, error) MarshalJSON(this ref intWithPtrMarshalJSON b) {
    return ((intWithMarshalJSON)(nint)(b)).MarshalJSON();
}

internal static error UnmarshalJSON(this ж<intWithPtrMarshalJSON> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<intWithPtrMarshalJSON, intWithMarshalJSON>()).UnmarshalJSON(data);
}

[GoType("num:nint")] internal partial struct intWithMarshalText;

internal static (slice<byte>, error) MarshalText(this intWithMarshalText b) {
    return (slice<byte>(fmt.Sprintf(@"Z%.2x"u8, (nint)b)), default!);
}

[GoRecv] internal static error UnmarshalText(this ref intWithMarshalText b, slice<byte> data) {
    if (len(data) != 3 || data[0] != (rune)'Z') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt(((@string)(data[1..3])), 16, 8);
    if (err != default!) {
        return fmt.Errorf("bad hex"u8);
    }
    b = ((intWithMarshalText)(nint)i);
    return default!;
}

[GoType("num:nint")] internal partial struct intWithPtrMarshalText;

[GoRecv] internal static (slice<byte>, error) MarshalText(this ref intWithPtrMarshalText b) {
    return ((intWithMarshalText)(nint)(b)).MarshalText();
}

internal static error UnmarshalText(this ж<intWithPtrMarshalText> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<intWithPtrMarshalText, intWithMarshalText>()).UnmarshalText(data);
}

[GoType] internal partial struct mapStringToStringData {
    [GoTag(@"json:""data""")]
    public map<@string, @string> Data;
}

[GoType] public partial struct B {
    [GoTag(@"json:"",string""")]
    public bool ΔB;
}

[GoType] public partial struct DoublePtr {
    public ж<ж<nint>> I;
    public ж<ж<nint>> J;
}

// basic types
// raw values with whitespace
// Z has a "-" tag.
// syntax errors
// raw value errors
// array tests
// empty array to interface test
// composite tests
// unmarshal interface test
// use "false" so test will fail if custom unmarshaler is not called
// UnmarshalText interface test
// integer-keyed map test
// Check that MarshalText and UnmarshalText take precedence
// over default integer handling in map keys.
// integer-keyed map errors
// Map keys can be encoding.TextUnmarshalers.
// If multiple values for the same key exists, only the most recent value is used.
// invalid UTF-8 is coerced to valid UTF-8.
// Used to be issue 8305, but time.Time implements encoding.TextUnmarshaler so this works now.
// issue 8305
// related to issue 13783.
// Go 1.7 changed marshaling a slice of typed byte to use the methods on the byte type,
// similar to marshaling a slice of typed int.
// These tests check that, assuming the byte type also has valid decoding methods,
// either the old base64 string encoding or the new per-element encoding can be
// successfully unmarshaled. The custom unmarshalers were accessible in earlier
// versions of Go, even though the custom marshaler was not.
// ints work with the marshaler but not the base64 []byte case
// issue 15146.
// invalid inputs in wrongStringTests below.
// additional tests for disallowUnknownFields
// issue 26444
// UnmarshalTypeError without field & struct values
// trying to decode JSON arrays or objects via TextUnmarshaler
// #22369
// #14702
// Verify that syntactic errors are immediately fatal,
// while semantic errors are lazily reported
// (i.e., allow processing to continue).

[GoType("dyn")] partial struct unmarshalTestsᴛ1 {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal any ptr; // new(type)
    internal any @out;
    internal error err;
    internal bool useNumber;
    internal bool golden;
    internal bool disallowUnknownFields;
}

            [GoType("dyn")] partial struct Δtype {
                public global::go.encoding.json_package.Number A;
            }

            [GoType("dyn")] partial struct Δtypeᴛ1 {
                [GoTag(@"json:"",string""")]
                public global::go.encoding.json_package.Number A;
            }

            [GoType("dyn")] partial struct Δtypeᴛ2 {
                public global::go.encoding.json_package.Number N;
            }

            [GoType("dyn")] partial struct Δtypeᴛ3 {
                [GoTag(@"json:"",string""")]
                public global::go.encoding.json_package.Number N;
            }
internal static slice<unmarshalTestsᴛ1> unmarshalTests;
internal static void initᴛunmarshalTests() { unmarshalTests = new unmarshalTestsᴛ1[]{
    new(CaseName: Name(""u8), @in: @"true"u8, ptr: @new<bool>(), @out: true),
    new(CaseName: Name(""u8), @in: @"1"u8, ptr: @new<nint>(), @out: (nint)(1)),
    new(CaseName: Name(""u8), @in: @"1.2"u8, ptr: @new<float64>(), @out: 1.2D),
    new(CaseName: Name(""u8), @in: @"-5"u8, ptr: @new<int16>(), @out: (int16)(-5)),
    new(CaseName: Name(""u8), @in: @"2"u8, ptr: @new<global::go.encoding.json_package.Number>(), @out: ((global::go.encoding.json_package.Number)(@string)"2"u8), useNumber: true),
    new(CaseName: Name(""u8), @in: @"2"u8, ptr: @new<global::go.encoding.json_package.Number>(), @out: ((global::go.encoding.json_package.Number)(@string)"2"u8)),
    new(CaseName: Name(""u8), @in: @"2"u8, ptr: @new<any>(), @out: (float64)2.0D),
    new(CaseName: Name(""u8), @in: @"2"u8, ptr: @new<any>(), @out: ((global::go.encoding.json_package.Number)(@string)"2"u8), useNumber: true),
    new(CaseName: Name(""u8), @in: @"""a\u1234"""u8, ptr: @new<@string>(), @out: (@string)"a\u1234"u8),
    new(CaseName: Name(""u8), @in: @"""http:\/\/"""u8, ptr: @new<@string>(), @out: (@string)"http://"u8),
    new(CaseName: Name(""u8), @in: @"""g-clef: \uD834\uDD1E"""u8, ptr: @new<@string>(), @out: (@string)"g-clef: \U0001D11E"u8),
    new(CaseName: Name(""u8), @in: @"""invalid: \uD834x\uDD1E"""u8, ptr: @new<@string>(), @out: (@string)"invalid: \uFFFDx\uFFFD"u8),
    new(CaseName: Name(""u8), @in: "null"u8, ptr: @new<any>(), @out: default!),
    new(CaseName: Name(""u8), @in: @"{""X"": [1,2,3], ""Y"": 4}"u8, ptr: @new<T>(), @out: new T(Y: 4), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("array"u8, reflect.TypeFor<@string>(), 7, "T"u8, "X"u8)))),
    new(CaseName: Name(""u8), @in: @"{""X"": 23}"u8, ptr: @new<T>(), @out: new T(nil), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("number"u8, reflect.TypeFor<@string>(), 8, "T"u8, "X"u8)))),
    new(CaseName: Name(""u8), @in: @"{""x"": 1}"u8, ptr: @new<tx>(), @out: new tx(nil)),
    new(CaseName: Name(""u8), @in: @"{""x"": 1}"u8, ptr: @new<tx>(), @out: new tx(nil)),
    new(CaseName: Name(""u8), @in: @"{""x"": 1}"u8, ptr: @new<tx>(), err: fmt.Errorf("json: unknown field \"x\""u8), disallowUnknownFields: true),
    new(CaseName: Name(""u8), @in: @"{""S"": 23}"u8, ptr: @new<W>(), @out: new W(nil), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("number"u8, reflect.TypeFor<SS>(), 0, "W"u8, "S"u8)))),
    new(CaseName: Name(""u8), @in: @"{""T"": {""X"": 23}}"u8, ptr: @new<TOuter>(), @out: new TOuter(nil), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("number"u8, reflect.TypeFor<@string>(), 8, "TOuter"u8, "T.X"u8)))),
    new(CaseName: Name(""u8), @in: @"{""F1"":1,""F2"":2,""F3"":3}"u8, ptr: @new<V>(), @out: new V(F1: (float64)1D, F2: (int32)2, F3: ((global::go.encoding.json_package.Number)(@string)"3"u8))),
    new(CaseName: Name(""u8), @in: @"{""F1"":1,""F2"":2,""F3"":3}"u8, ptr: @new<V>(), @out: new V(F1: ((global::go.encoding.json_package.Number)(@string)"1"u8), F2: (int32)2, F3: ((global::go.encoding.json_package.Number)(@string)"3"u8)), useNumber: true),
    new(CaseName: Name(""u8), @in: @"{""k1"":1,""k2"":""s"",""k3"":[1,2.0,3e-3],""k4"":{""kk1"":""s"",""kk2"":2}}"u8, ptr: @new<any>(), @out: ifaceNumAsFloat64),
    new(CaseName: Name(""u8), @in: @"{""k1"":1,""k2"":""s"",""k3"":[1,2.0,3e-3],""k4"":{""kk1"":""s"",""kk2"":2}}"u8, ptr: @new<any>(), @out: ifaceNumAsNumber, useNumber: true),
    new(CaseName: Name(""u8), @in: "\n true "u8, ptr: @new<bool>(), @out: true),
    new(CaseName: Name(""u8), @in: "\t 1 "u8, ptr: @new<nint>(), @out: (nint)(1)),
    new(CaseName: Name(""u8), @in: "\r 1.2 "u8, ptr: @new<float64>(), @out: 1.2D),
    new(CaseName: Name(""u8), @in: "\t -5 \n"u8, ptr: @new<int16>(), @out: (int16)(-5)),
    new(CaseName: Name(""u8), @in: "\t \"a\\u1234\" \n"u8, ptr: @new<@string>(), @out: (@string)"a\u1234"u8),
    new(CaseName: Name(""u8), @in: @"{""Y"": 1, ""Z"": 2}"u8, ptr: @new<T>(), @out: new T(Y: 1)),
    new(CaseName: Name(""u8), @in: @"{""Y"": 1, ""Z"": 2}"u8, ptr: @new<T>(), @out: new T(Y: 1), err: fmt.Errorf("json: unknown field \"Z\""u8), disallowUnknownFields: true),
    new(CaseName: Name(""u8), @in: @"{""alpha"": ""abc"", ""alphabet"": ""xyz""}"u8, ptr: @new<U>(), @out: new U(Alphabet: "abc"u8)),
    new(CaseName: Name(""u8), @in: @"{""alpha"": ""abc"", ""alphabet"": ""xyz""}"u8, ptr: @new<U>(), @out: new U(Alphabet: "abc"u8), err: fmt.Errorf("json: unknown field \"alphabet\""u8), disallowUnknownFields: true),
    new(CaseName: Name(""u8), @in: @"{""alpha"": ""abc""}"u8, ptr: @new<U>(), @out: new U(Alphabet: "abc"u8)),
    new(CaseName: Name(""u8), @in: @"{""alphabet"": ""xyz""}"u8, ptr: @new<U>(), @out: new U(nil)),
    new(CaseName: Name(""u8), @in: @"{""alphabet"": ""xyz""}"u8, ptr: @new<U>(), err: fmt.Errorf("json: unknown field \"alphabet\""u8), disallowUnknownFields: true),
    new(CaseName: Name(""u8), @in: @"{""X"": ""foo"", ""Y""}"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '}' after object key"u8, 17)))),
    new(CaseName: Name(""u8), @in: @"[1, 2, 3+]"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '+' after array element"u8, 9)))),
    new(CaseName: Name(""u8), @in: @"{""X"":12x}"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character 'x' after object key:value pair"u8, 8))), useNumber: true),
    new(CaseName: Name(""u8), @in: @"[2, 3"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(msg: "unexpected end of JSON input"u8, Offset: 5)))),
    new(CaseName: Name(""u8), @in: @"{""F3"": -}"u8, ptr: @new<V>(), err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(msg: "invalid character '}' in numeric literal"u8, Offset: 9)))),
    new(CaseName: Name(""u8), @in: "\x01 42"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value"u8, 1)))),
    new(CaseName: Name(""u8), @in: " 42 \x01"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value"u8, 5)))),
    new(CaseName: Name(""u8), @in: "\x01 true"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value"u8, 1)))),
    new(CaseName: Name(""u8), @in: " false \x01"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value"u8, 8)))),
    new(CaseName: Name(""u8), @in: "\x01 1.2"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value"u8, 1)))),
    new(CaseName: Name(""u8), @in: " 3.4 \x01"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value"u8, 6)))),
    new(CaseName: Name(""u8), @in: "\x01 \"string\""u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value"u8, 1)))),
    new(CaseName: Name(""u8), @in: " \"string\" \x01"u8, err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value"u8, 11)))),
    new(CaseName: Name(""u8), @in: @"[1, 2, 3]"u8, ptr: Ꮡ(new array<nint>(3)), @out: new nint[]{1, 2, 3}.array()),
    new(CaseName: Name(""u8), @in: @"[1, 2, 3]"u8, ptr: Ꮡ(new array<nint>(1)), @out: new nint[]{1}.array()),
    new(CaseName: Name(""u8), @in: @"[1, 2, 3]"u8, ptr: Ꮡ(new array<nint>(5)), @out: new nint[]{1, 2, 3, 0, 0}.array()),
    new(CaseName: Name(""u8), @in: @"[1, 2, 3]"u8, ptr: @new<MustNotUnmarshalJSON>(), err: errors.New("MustNotUnmarshalJSON was used"u8)),
    new(CaseName: Name(""u8), @in: @"[]"u8, ptr: @new<slice<any>>(), @out: new any[]{}.slice()),
    new(CaseName: Name(""u8), @in: @"null"u8, ptr: @new<slice<any>>(), @out: slice<any>(default!)),
    new(CaseName: Name(""u8), @in: @"{""T"":[]}"u8, ptr: @new<map<@string, any>>(), @out: new map<@string, any>{["T"u8] = new any[]{}.slice()}),
    new(CaseName: Name(""u8), @in: @"{""T"":null}"u8, ptr: @new<map<@string, any>>(), @out: new map<@string, any>{["T"u8] = ((any)default!)}),
    new(CaseName: Name(""u8), @in: allValueIndent, ptr: @new<All>(), @out: allValue),
    new(CaseName: Name(""u8), @in: allValueCompact, ptr: @new<All>(), @out: allValue),
    new(CaseName: Name(""u8), @in: allValueIndent, ptr: @new<ж<All>>(), @out: ᏑallValue),
    new(CaseName: Name(""u8), @in: allValueCompact, ptr: @new<ж<All>>(), @out: ᏑallValue),
    new(CaseName: Name(""u8), @in: pallValueIndent, ptr: @new<All>(), @out: pallValue),
    new(CaseName: Name(""u8), @in: pallValueCompact, ptr: @new<All>(), @out: pallValue),
    new(CaseName: Name(""u8), @in: pallValueIndent, ptr: @new<ж<All>>(), @out: ᏑpallValue),
    new(CaseName: Name(""u8), @in: pallValueCompact, ptr: @new<ж<All>>(), @out: ᏑpallValue),
    new(CaseName: Name(""u8), @in: @"{""T"":false}"u8, ptr: @new<unmarshaler>(), @out: umtrue),
    new(CaseName: Name(""u8), @in: @"{""T"":false}"u8, ptr: @new<ж<unmarshaler>>(), @out: Ꮡumtrue),
    new(CaseName: Name(""u8), @in: @"[{""T"":false}]"u8, ptr: @new<slice<unmarshaler>>(), @out: umslice),
    new(CaseName: Name(""u8), @in: @"[{""T"":false}]"u8, ptr: @new<ж<slice<unmarshaler>>>(), @out: Ꮡumslice),
    new(CaseName: Name(""u8), @in: @"{""M"":{""T"":""x:y""}}"u8, ptr: @new<ustruct>(), @out: umstruct),
    new(CaseName: Name(""u8), @in: @"""x:y"""u8, ptr: @new<unmarshalerText>(), @out: umtrueXY),
    new(CaseName: Name(""u8), @in: @"""x:y"""u8, ptr: @new<ж<unmarshalerText>>(), @out: ᏑumtrueXY),
    new(CaseName: Name(""u8), @in: @"[""x:y""]"u8, ptr: @new<slice<unmarshalerText>>(), @out: umsliceXY),
    new(CaseName: Name(""u8), @in: @"[""x:y""]"u8, ptr: @new<ж<slice<unmarshalerText>>>(), @out: ᏑumsliceXY),
    new(CaseName: Name(""u8), @in: @"{""M"":""x:y""}"u8, ptr: @new<ustructText>(), @out: umstructXY),
    new(
        CaseName: Name(""u8),
        @in: @"{""-1"":""a"",""0"":""b"",""1"":""c""}"u8,
        ptr: @new<map<nint, @string>>(),
        @out: new map<nint, @string>{[-1] = "a"u8, [0] = "b"u8, [1] = "c"u8}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""0"":""a"",""10"":""c"",""9"":""b""}"u8,
        ptr: @new<map<u8, @string>>(),
        @out: new map<u8, @string>{[0] = "a"u8, [9] = "b"u8, [10] = "c"u8}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""-9223372036854775808"":""min"",""9223372036854775807"":""max""}"u8,
        ptr: @new<map<int64, @string>>(),
        @out: new map<int64, @string>{[math.MinInt64] = "min"u8, [math.MaxInt64] = "max"u8}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""18446744073709551615"":""max""}"u8,
        ptr: @new<map<uint64, @string>>(),
        @out: new map<uint64, @string>{[math.MaxUint64] = "max"u8}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""0"":false,""10"":true}"u8,
        ptr: @new<map<uintptr, bool>>(),
        @out: new map<uintptr, bool>{[0] = false, [10] = true}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""u2"":4}"u8,
        ptr: @new<map<u8marshal, nint>>(),
        @out: new map<u8marshal, nint>{[2] = 4}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""2"":4}"u8,
        ptr: @new<map<u8marshal, nint>>(),
        @out: new map<u8marshal, nint>{},
        err: errMissingU8Prefix
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""abc"":""abc""}"u8,
        ptr: @new<map<nint, @string>>(),
        @out: new map<nint, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number abc"u8, Type: reflect.TypeFor<nint>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""256"":""abc""}"u8,
        ptr: @new<map<uint8, @string>>(),
        @out: new map<uint8, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 256"u8, Type: reflect.TypeFor<uint8>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""128"":""abc""}"u8,
        ptr: @new<map<int8, @string>>(),
        @out: new map<int8, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 128"u8, Type: reflect.TypeFor<int8>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""-1"":""abc""}"u8,
        ptr: @new<map<uint8, @string>>(),
        @out: new map<uint8, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number -1"u8, Type: reflect.TypeFor<uint8>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""F"":{""a"":2,""3"":4}}"u8,
        ptr: @new<map<@string, map<nint, nint>>>(),
        @out: new map<@string, map<nint, nint>>{["F"u8] = new map<nint, nint>{[3] = 4}},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number a"u8, Type: reflect.TypeFor<nint>(), Offset: 7)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""F"":{""a"":2,""3"":4}}"u8,
        ptr: @new<map<@string, map<nuint, nint>>>(),
        @out: new map<@string, map<nuint, nint>>{["F"u8] = new map<nuint, nint>{[3] = 4}},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number a"u8, Type: reflect.TypeFor<nuint>(), Offset: 7)))
    ),
    new(CaseName: Name(""u8), @in: @"{""x:y"":true}"u8, ptr: @new<map<unmarshalerText, bool>>(), @out: ummapXY),
    new(CaseName: Name(""u8), @in: @"{""x:y"":false,""x:y"":true}"u8, ptr: @new<map<unmarshalerText, bool>>(), @out: ummapXY),
    new(
        CaseName: Name(""u8),
        @in: """
{
			"Level0": 1,
			"Level1b": 2,
			"Level1c": 3,
			"x": 4,
			"Level1a": 5,
			"LEVEL1B": 6,
			"e": {
				"Level1a": 8,
				"Level1b": 9,
				"Level1c": 10,
				"Level1d": 11,
				"x": 12
			},
			"Loop1": 13,
			"Loop2": 14,
			"X": 15,
			"Y": 16,
			"Z": 17,
			"Q": 18
		}
"""u8,
        ptr: @new<Top>(),
        @out: new Top(
            Level0: 1,
            Embed0: new Embed0(
                Level1b: 2,
                Level1c: 3
            ),
            Embed0a: Ꮡ(new Embed0a(
                Level1a: 5,
                Level1b: 6
            )),
            Embed0b: Ꮡ(new Embed0b(new Embed0(
                Level1a: 8,
                Level1b: 9,
                Level1c: 10,
                Level1d: 11,
                Level1e: 12
            ))),
            Loop: new Loop(
                Loop1: 13,
                Loop2: 14
            ),
            Embed0p: new Embed0p(
                Point: new image.Point(X: 15, Y: 16)
            ),
            Embed0q: new Embed0q(
                Point: new Point(Z: 17)
            ),
            embed: new embed(
                Q: 18
            )
        )
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""hello"": 1}"u8,
        ptr: @new<Ambig>(),
        @out: new Ambig(First: 1)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1,""Y"":2}"u8,
        ptr: @new<S5>(),
        @out: new S5(S8: new S8(S9: new S9(Y: 2)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1,""Y"":2}"u8,
        ptr: @new<S5>(),
        @out: new S5(S8: new S8(new S9(Y: 2))),
        err: fmt.Errorf("json: unknown field \"X\""u8),
        disallowUnknownFields: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1,""Y"":2}"u8,
        ptr: @new<S10>(),
        @out: new S10(S13: new S13(S8: new S8(S9: new S9(Y: 2))))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1,""Y"":2}"u8,
        ptr: @new<S10>(),
        @out: new S10(S13: new S13(new S8(new S9(Y: 2)))),
        err: fmt.Errorf("json: unknown field \"X\""u8),
        disallowUnknownFields: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""I"": 0, ""I"": null, ""J"": null}"u8,
        ptr: @new<DoublePtr>(),
        @out: new DoublePtr(I: nil, J: nil)
    ),
    new(
        CaseName: Name(""u8),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xc2, 0xc2, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xc2, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: "\"hello\\ud800world\""u8,
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: "\"hello\\ud800\\ud800world\""u8,
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: "\"hello\\ud800\\ud800world\""u8,
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xed, 0xa0, 0x80, 0xed, 0xb0, 0x80, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffd\ufffd\ufffd\ufffd\ufffdworld"u8
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""2009-11-10T23:00:00Z"": ""hello world""}"u8,
        ptr: @new<map<time.Time, @string>>(),
        @out: new map<time.Time, @string>{[time.Date(2009, 11, 10, 23, 0, 0, 0, time.ΔUTC)] = "hello world"u8}
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""2009-11-10T23:00:00Z"": ""hello world""}"u8,
        ptr: @new<map<Point, @string>>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "object"u8, Type: reflect.TypeFor<map<Point, @string>>(), Offset: 1)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""asdf"": ""hello world""}"u8,
        ptr: @new<map<unmarshaler, @string>>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "object"u8, Type: reflect.TypeFor<map<unmarshaler, @string>>(), Offset: 1)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"""AQID"""u8,
        ptr: @new<slice<byteWithMarshalJSON>>(),
        @out: new byteWithMarshalJSON[]{1, 2, 3}.slice()
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<byteWithMarshalJSON>>(),
        @out: new byteWithMarshalJSON[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"""AQID"""u8,
        ptr: @new<slice<byteWithMarshalText>>(),
        @out: new byteWithMarshalText[]{1, 2, 3}.slice()
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<byteWithMarshalText>>(),
        @out: new byteWithMarshalText[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"""AQID"""u8,
        ptr: @new<slice<byteWithPtrMarshalJSON>>(),
        @out: new byteWithPtrMarshalJSON[]{1, 2, 3}.slice()
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<byteWithPtrMarshalJSON>>(),
        @out: new byteWithPtrMarshalJSON[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"""AQID"""u8,
        ptr: @new<slice<byteWithPtrMarshalText>>(),
        @out: new byteWithPtrMarshalText[]{1, 2, 3}.slice()
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<byteWithPtrMarshalText>>(),
        @out: new byteWithPtrMarshalText[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<intWithMarshalJSON>>(),
        @out: new intWithMarshalJSON[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<intWithMarshalText>>(),
        @out: new intWithMarshalText[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<intWithPtrMarshalJSON>>(),
        @out: new intWithPtrMarshalJSON[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[""Z01"",""Z02"",""Z03""]"u8,
        ptr: @new<slice<intWithPtrMarshalText>>(),
        @out: new intWithPtrMarshalText[]{1, 2, 3}.slice(),
        golden: true
    ),
    new(CaseName: Name(""u8), @in: @"0.000001"u8, ptr: @new<float64>(), @out: 0.000001D, golden: true),
    new(CaseName: Name(""u8), @in: @"1e-7"u8, ptr: @new<float64>(), @out: 1e-7D, golden: true),
    new(CaseName: Name(""u8), @in: @"100000000000000000000"u8, ptr: @new<float64>(), @out: 100000000000000000000.0D, golden: true),
    new(CaseName: Name(""u8), @in: @"1e+21"u8, ptr: @new<float64>(), @out: 1e21D, golden: true),
    new(CaseName: Name(""u8), @in: @"-0.000001"u8, ptr: @new<float64>(), @out: -0.000001D, golden: true),
    new(CaseName: Name(""u8), @in: @"-1e-7"u8, ptr: @new<float64>(), @out: -1e-7D, golden: true),
    new(CaseName: Name(""u8), @in: @"-100000000000000000000"u8, ptr: @new<float64>(), @out: -100000000000000000000.0D, golden: true),
    new(CaseName: Name(""u8), @in: @"-1e+21"u8, ptr: @new<float64>(), @out: -1e21D, golden: true),
    new(CaseName: Name(""u8), @in: @"999999999999999900000"u8, ptr: @new<float64>(), @out: 999999999999999900000.0D, golden: true),
    new(CaseName: Name(""u8), @in: @"9007199254740992"u8, ptr: @new<float64>(), @out: 9007199254740992.0D, golden: true),
    new(CaseName: Name(""u8), @in: @"9007199254740993"u8, ptr: @new<float64>(), @out: 9007199254740992.0D, golden: false),
    new(
        CaseName: Name(""u8),
        @in: @"{""V"": {""F2"": ""hello""}}"u8,
        ptr: @new<VOuter>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string"u8,
            Struct: "V"u8,
            Field: "V.F2"u8,
            Type: reflect.TypeFor<int32>(),
            Offset: 20
        )))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""V"": {""F4"": {}, ""F2"": ""hello""}}"u8,
        ptr: @new<VOuter>(),
        @out: new VOuter(V: new V(F4: Ꮡ(new VOuter(nil)))),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string"u8,
            Struct: "V"u8,
            Field: "V.F2"u8,
            Type: reflect.TypeFor<int32>(),
            Offset: 30
        )))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""Level1a"": ""hello""}"u8,
        ptr: @new<Top>(),
        @out: new Top(Embed0a: Ꮡ(new Embed0a(nil))),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string"u8,
            Struct: "Top"u8,
            Field: "Embed0a.Level1a"u8,
            Type: reflect.TypeFor<nint>(),
            Offset: 19
        )))
    ),
    new(CaseName: Name(""u8), @in: @"{""B"":""true""}"u8, ptr: @new<B>(), @out: new B(true), golden: true),
    new(CaseName: Name(""u8), @in: @"{""B"":""false""}"u8, ptr: @new<B>(), @out: new B(false), golden: true),
    new(CaseName: Name(""u8), @in: @"{""B"": ""maybe""}"u8, ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""maybe"" into bool"u8)),
    new(CaseName: Name(""u8), @in: @"{""B"": ""tru""}"u8, ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""tru"" into bool"u8)),
    new(CaseName: Name(""u8), @in: @"{""B"": ""False""}"u8, ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""False"" into bool"u8)),
    new(CaseName: Name(""u8), @in: @"{""B"": ""null""}"u8, ptr: @new<B>(), @out: new B(false)),
    new(CaseName: Name(""u8), @in: @"{""B"": ""nul""}"u8, ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""nul"" into bool"u8)),
    new(CaseName: Name(""u8), @in: @"{""B"": [2, 3]}"u8, ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal unquoted value into bool"u8)),
    new(
        CaseName: Name(""u8),
        @in: """
{
			"Level0": 1,
			"Level1b": 2,
			"Level1c": 3,
			"x": 4,
			"Level1a": 5,
			"LEVEL1B": 6,
			"e": {
				"Level1a": 8,
				"Level1b": 9,
				"Level1c": 10,
				"Level1d": 11,
				"x": 12
			},
			"Loop1": 13,
			"Loop2": 14,
			"X": 15,
			"Y": 16,
			"Z": 17,
			"Q": 18,
			"extra": true
		}
"""u8,
        ptr: @new<Top>(),
        @out: new Top(
            Level0: 1,
            Embed0: new Embed0(
                Level1b: 2,
                Level1c: 3
            ),
            Embed0a: Ꮡ(new Embed0a(Level1a: 5, Level1b: 6)),
            Embed0b: Ꮡ(new Embed0b(new Embed0(Level1a: 8, Level1b: 9, Level1c: 10, Level1d: 11, Level1e: 12))),
            Loop: new Loop(
                Loop1: 13,
                Loop2: 14,
                ΔLoop: nil
            ),
            Embed0p: new Embed0p(
                Point: new image.Point(
                    X: 15,
                    Y: 16
                )
            ),
            Embed0q: new Embed0q(Point: new Point(Z: 17)),
            embed: new embed(Q: 18)
        ),
        err: fmt.Errorf("json: unknown field \"extra\""u8),
        disallowUnknownFields: true
    ),
    new(
        CaseName: Name(""u8),
        @in: """
{
			"Level0": 1,
			"Level1b": 2,
			"Level1c": 3,
			"x": 4,
			"Level1a": 5,
			"LEVEL1B": 6,
			"e": {
				"Level1a": 8,
				"Level1b": 9,
				"Level1c": 10,
				"Level1d": 11,
				"x": 12,
				"extra": null
			},
			"Loop1": 13,
			"Loop2": 14,
			"X": 15,
			"Y": 16,
			"Z": 17,
			"Q": 18
		}
"""u8,
        ptr: @new<Top>(),
        @out: new Top(
            Level0: 1,
            Embed0: new Embed0(
                Level1b: 2,
                Level1c: 3
            ),
            Embed0a: Ꮡ(new Embed0a(Level1a: 5, Level1b: 6)),
            Embed0b: Ꮡ(new Embed0b(new Embed0(Level1a: 8, Level1b: 9, Level1c: 10, Level1d: 11, Level1e: 12))),
            Loop: new Loop(
                Loop1: 13,
                Loop2: 14,
                ΔLoop: nil
            ),
            Embed0p: new Embed0p(
                Point: new image.Point(
                    X: 15,
                    Y: 16
                )
            ),
            Embed0q: new Embed0q(Point: new Point(Z: 17)),
            embed: new embed(Q: 18)
        ),
        err: fmt.Errorf("json: unknown field \"extra\""u8),
        disallowUnknownFields: true
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""data"":{""test1"": ""bob"", ""test2"": 123}}"u8,
        ptr: @new<mapStringToStringData>(),
        @out: new mapStringToStringData(new map<@string, @string>{["test1"u8] = "bob"u8, ["test2"u8] = ""u8}),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number"u8, Type: reflect.TypeFor<@string>(), Offset: 37, Struct: "mapStringToStringData"u8, Field: "data"u8)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""data"":{""test1"": 123, ""test2"": ""bob""}}"u8,
        ptr: @new<mapStringToStringData>(),
        @out: new mapStringToStringData(Data: new map<@string, @string>{["test1"u8] = ""u8, ["test2"u8] = "bob"u8}),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number"u8, Type: reflect.TypeFor<@string>(), Offset: 21, Struct: "mapStringToStringData"u8, Field: "data"u8)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[1, 2, 3]"u8,
        ptr: @new<MustNotUnmarshalText>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "array"u8, Type: reflect.TypeFor<ж<MustNotUnmarshalText>>(), Offset: 1)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""foo"": ""bar""}"u8,
        ptr: @new<MustNotUnmarshalText>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "object"u8, Type: reflect.TypeFor<ж<MustNotUnmarshalText>>(), Offset: 1)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""PP"": {""T"": {""Y"": ""bad-type""}}}"u8,
        ptr: @new<P>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string"u8,
            Struct: "T"u8,
            Field: "PP.T.Y"u8,
            Type: reflect.TypeFor<nint>(),
            Offset: 29
        )))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""Ts"": [{""Y"": 1}, {""Y"": 2}, {""Y"": ""bad-type""}]}"u8,
        ptr: @new<PP>(),
        @out: new PP(Ts: new T[]{new(Y: 1), new(Y: 2), new(Y: 0)}.slice()),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string"u8,
            Struct: "T"u8,
            Field: "Ts.Y"u8,
            Type: reflect.TypeFor<nint>(),
            Offset: 44
        )))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"invalid"u8,
        ptr: @new<global::go.encoding.json_package.Number>(),
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(
            msg: "invalid character 'i' looking for beginning of value"u8,
            Offset: 1
        )))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"""invalid"""u8,
        ptr: @new<global::go.encoding.json_package.Number>(),
        err: fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, (@string)@"""invalid"""u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""A"":""invalid""}"u8,
        ptr: @new<Δtype>(),
        err: fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, (@string)@"""invalid"""u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""A"":""invalid""}"u8,
        ptr: @new<Δtypeᴛ1>(),
        err: fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into json.Number"u8, (@string)@"invalid"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""A"":""invalid""}"u8,
        ptr: @new<map<@string, global::go.encoding.json_package.Number>>(),
        @out: new map<@string, global::go.encoding.json_package.Number>{},
        err: fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, (@string)@"""invalid"""u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"5"u8,
        ptr: @new<global::go.encoding.json_package.Number>(),
        @out: ((global::go.encoding.json_package.Number)(@string)"5"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"""5"""u8,
        ptr: @new<global::go.encoding.json_package.Number>(),
        @out: ((global::go.encoding.json_package.Number)(@string)"5"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""N"":5}"u8,
        ptr: @new<Δtypeᴛ2>(),
        @out: new Δtypeᴛ2("5"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""N"":""5""}"u8,
        ptr: @new<Δtypeᴛ2>(),
        @out: new Δtypeᴛ2("5"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""N"":5}"u8,
        ptr: @new<Δtypeᴛ3>(),
        err: fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal unquoted value into json.Number"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"{""N"":""5""}"u8,
        ptr: @new<Δtypeᴛ3>(),
        @out: new Δtypeᴛ3("5"u8)
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[1,2,true,4,5}"u8,
        ptr: @new<slice<nint>>(),
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(msg: "invalid character '}' after array element"u8, Offset: 14)))
    ),
    new(
        CaseName: Name(""u8),
        @in: @"[1,2,true,4,5]"u8,
        ptr: @new<slice<nint>>(),
        @out: new nint[]{1, 2, 0, 4, 5}.slice(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "bool"u8, Type: reflect.TypeFor<nint>(), Offset: 9)))
    )
}.slice(); }

public static void TestMarshal(ж<testing.T> Ꮡt) {
    var (b, err) = Marshal(allValue);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (((sstring)b) != allValueCompact) {
        Ꮡt.Errorf("Marshal:"u8);
        diff(Ꮡt, b, slice<byte>(allValueCompact));
        return;
    }
    (b, err) = Marshal(pallValue);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (((sstring)b) != pallValueCompact) {
        Ꮡt.Errorf("Marshal:"u8);
        diff(Ꮡt, b, slice<byte>(pallValueCompact));
        return;
    }
}

[GoType("dyn")] internal partial struct TestMarshalInvalidUTF8_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal @string want;
}

public static void TestMarshalInvalidUTF8(ж<testing.T> Ꮡt) {
    var tests = new TestMarshalInvalidUTF8_tests[]{
        new(Name(""u8), ((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64})), @"""hello\ufffdworld"""u8),
        new(Name(""u8), ""u8, @""""""u8),
        new(Name(""u8), ((@string)(new byte[]{0xff})), @"""\ufffd"""u8),
        new(Name(""u8), ((@string)(new byte[]{0xff, 0xff})), @"""\ufffd\ufffd"""u8),
        new(Name(""u8), ((@string)(new byte[]{0x61, 0xff, 0x62})), @"""a\ufffdb"""u8),
        new(Name(""u8), ((@string)(new byte[]{0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xff, 0xaa, 0x9e})), @"""日本\ufffd\ufffd\ufffd"""u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestMarshalInvalidUTF8_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (got, err) = Marshal(ttʗ1.@in);
            if (((sstring)got) != ttʗ1.want || err != default!) {
                tΔ1.Errorf("%s: Marshal(%q):\n\tgot:  (%q, %v)\n\twant: (%q, nil)"u8, ttʗ1.Where, ttʗ1.@in, got, err, ttʗ1.want);
            }
        });
    }
}

public static void TestMarshalNumberZeroVal(ж<testing.T> Ꮡt) {
    global::go.encoding.json_package.Number n = default!;
    var (@out, err) = Marshal(n);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    @string got = ((@string)@out);
    if (got != "0"u8) {
        Ꮡt.Fatalf("Marshal: got %s, want 0"u8, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string level01Level1b2Level1c3ˢ = "{\"Level0\":1,\"Level1b\":2,\"Level1c\":3,\"Level1a\":5,\"LEVEL1B\":6,\"e\":{\"Level1a\":8,\"Level1b\":9,\"Level1c\":10,\"Level1d\":11,\"x\":12},\"Loop1\":13,\"Loop2\":14,\"X\":15,\"Y\":16,\"Z\":17,\"Q\":18}"u8;

public static void TestMarshalEmbeds(ж<testing.T> Ꮡt) {
    var top = Ꮡ(new Top(
        Level0: 1,
        Embed0: new Embed0(
            Level1b: 2,
            Level1c: 3
        ),
        Embed0a: Ꮡ(new Embed0a(
            Level1a: 5,
            Level1b: 6
        )),
        Embed0b: Ꮡ(new Embed0b(new Embed0(
            Level1a: 8,
            Level1b: 9,
            Level1c: 10,
            Level1d: 11,
            Level1e: 12
        ))),
        Loop: new Loop(
            Loop1: 13,
            Loop2: 14
        ),
        Embed0p: new Embed0p(
            Point: new image.Point(X: 15, Y: 16)
        ),
        Embed0q: new Embed0q(
            Point: new Point(Z: 17)
        ),
        embed: new embed(
            Q: 18
        )
    ));
    var (got, err) = Marshal(top.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    @string want = level01Level1b2Level1c3ˢ;
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

internal static bool equalError(error a, error b) {
    bool isJSONError(error err) {
        switch (err.type()) {
        case ж<global::go.encoding.json_package.InvalidUTF8Error> _:
        case ж<global::go.encoding.json_package.InvalidUnmarshalError> _:
        case ж<global::go.encoding.json_package.MarshalerError> _:
        case ж<global::go.encoding.json_package.SyntaxError> _:
        case ж<global::go.encoding.json_package.UnmarshalFieldError> _:
        case ж<global::go.encoding.json_package.UnmarshalTypeError> _:
        case ж<global::go.encoding.json_package.UnsupportedTypeError> _:
        case ж<global::go.encoding.json_package.UnsupportedValueError> _: {
            return true;
        }}

        return false;
    }
    if (a == default! || b == default!) {
        return a == default! && b == default!;
    }
    if (isJSONError(a) || isJSONError(b)) {
        return reflect.DeepEqual(a, b); // safe for locally defined error types
    }
    return a.Error() == b.Error();
}

public static void TestUnmarshal(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in unmarshalTests) {
        ref var tt = ref heap(new unmarshalTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var @in = slice<byte>(ttʗ1.@in);
            ref var scan = ref heap(new global::go.encoding.json_package.scanner(), out var Ꮡscan);
            {
                var err = checkValid(@in, Ꮡscan); if (err != default!) {
                    if (!equalError(err, ttʗ1.err)) {
                        tΔ1.Fatalf("%s: checkValid error:\n\tgot  %#v\n\twant %#v"u8, ttʗ1.Where, err, ttʗ1.err);
                    }
                }
            }
            if (ttʗ1.ptr == default!) {
                return;
            }
            var typ = reflect.TypeOf(ttʗ1.ptr);
            if (typ.Kind() != reflect.ΔPointer) {
                tΔ1.Fatalf("%s: unmarshalTest.ptr %T is not a pointer type"u8, ttʗ1.Where, ttʗ1.ptr);
            }
            typ = typ.Elem();
            // v = new(right-type)
            var v = reflect.New(typ);
            if (!reflect.DeepEqual(ttʗ1.ptr, v.Interface())) {
                // There's no reason for ptr to point to non-zero data,
                // as we decode into new(right-type), so the data is
                // discarded.
                // This can easily mean tests that silently don't test
                // what they should. To test decoding into existing
                // data, see TestPrefilled.
                tΔ1.Fatalf("%s: unmarshalTest.ptr %#v is not a pointer to a zero value"u8, ttʗ1.Where, ttʗ1.ptr);
            }
            var dec = NewDecoder(new json_test_package.bytes_ReaderжReader(bytes.NewReader(@in)));
            if (ttʗ1.useNumber) {
                dec.UseNumber();
            }
            if (ttʗ1.disallowUnknownFields) {
                dec.DisallowUnknownFields();
            }
            {
                var err = dec.Decode(v.Interface()); if (!equalError(err, ttʗ1.err)){
                    tΔ1.Fatalf("%s: Decode error:\n\tgot:  %v\n\twant: %v\n\n\tgot:  %#v\n\twant: %#v"u8, ttʗ1.Where, err, ttʗ1.err, err, ttʗ1.err);
                } else 
                if (err != default! && ttʗ1.@out == default!) {
                    // Initialize tt.out during an error where there are no mutations,
                    // so the output is just the zero value of the input type.
                    ttʗ1.@out = reflect.Zero(v.Elem().Type()).Interface();
                }
            }
            {
                var got = v.Elem().Interface(); if (!reflect.DeepEqual(got, ttʗ1.@out)) {
                    var (gotJSON, _) = Marshal(got);
                    var (wantJSON, _) = Marshal(ttʗ1.@out);
                    tΔ1.Fatalf("%s: Decode:\n\tgot:  %#+v\n\twant: %#+v\n\n\tgotJSON:  %s\n\twantJSON: %s"u8, ttʗ1.Where, got, ttʗ1.@out, gotJSON, wantJSON);
                }
            }
            // Check round trip also decodes correctly.
            if (ttʗ1.err == default!) {
                var (enc, err) = Marshal(v.Interface());
                if (err != default!) {
                    tΔ1.Fatalf("%s: Marshal error after roundtrip: %v"u8, ttʗ1.Where, err);
                }
                if (ttʗ1.golden && !bytes.Equal(enc, @in)) {
                    tΔ1.Errorf("%s: Marshal:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, enc, @in);
                }
                var vv = reflect.New(reflect.TypeOf(ttʗ1.ptr).Elem());
                dec = NewDecoder(new json_test_package.bytes_ReaderжReader(bytes.NewReader(enc)));
                if (ttʗ1.useNumber) {
                    dec.UseNumber();
                }
                {
                    var errΔ1 = dec.Decode(vv.Interface()); if (errΔ1 != default!) {
                        tΔ1.Fatalf("%s: Decode(%#q) error after roundtrip: %v"u8, ttʗ1.Where, enc, errΔ1);
                    }
                }
                if (!reflect.DeepEqual(v.Elem().Interface(), vv.Elem().Interface())) {
                    tΔ1.Fatalf("%s: Decode:\n\tgot:  %#+v\n\twant: %#+v\n\n\tgotJSON:  %s\n\twantJSON: %s"u8,
                        ttʗ1.Where, v.Elem().Interface(), vv.Elem().Interface(),
                        stripWhitespace(((@string)enc)), stripWhitespace(((@string)@in)));
                }
            }
        });
    }
}

public static void TestUnmarshalMarshal(ж<testing.T> Ꮡt) {
    initBig();
    ref var v = ref heap<any>(out var Ꮡv);
    {
        var errΔ1 = Unmarshal(jsonBig, Ꮡv); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, errΔ1);
        }
    }
    var (b, err) = Marshal(v);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (!bytes.Equal(jsonBig, b)) {
        Ꮡt.Errorf("Marshal:"u8);
        diff(Ꮡt, b, jsonBig);
        return;
    }
}

[GoType("dyn")] internal partial struct TestNumberAccessors_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal int64 i;
    internal @string intErr;
    internal float64 f;
    internal @string floatErr;
}

// Independent of Decode, basic coverage of the accessors in Number
public static void TestNumberAccessors(ж<testing.T> Ꮡt) {
    var tests = new TestNumberAccessors_tests[]{
        new(CaseName: Name(""u8), @in: "-1.23e1"u8, intErr: "strconv.ParseInt: parsing \"-1.23e1\": invalid syntax"u8, f: -1.23e1D),
        new(CaseName: Name(""u8), @in: "-12"u8, i: -12, f: -12.0D),
        new(CaseName: Name(""u8), @in: "1e1000"u8, intErr: "strconv.ParseInt: parsing \"1e1000\": invalid syntax"u8, floatErr: "strconv.ParseFloat: parsing \"1e1000\": value out of range"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestNumberAccessors_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            global::go.encoding.json_package.Number n = ((global::go.encoding.json_package.Number)ttʗ1.@in);
            {
                @string got = n.String(); if (got != ttʗ1.@in) {
                    tΔ1.Errorf("%s: Number(%q).String() = %s, want %s"u8, ttʗ1.Where, ttʗ1.@in, got, ttʗ1.@in);
                }
            }
            {
                var (i, err) = n.Int64(); if (err == default! && ttʗ1.intErr == ""u8 && i != ttʗ1.i){
                    tΔ1.Errorf("%s: Number(%q).Int64() = %d, want %d"u8, ttʗ1.Where, ttʗ1.@in, i, ttʗ1.i);
                } else 
                if ((err == default! && ttʗ1.intErr != ""u8) || (err != default! && err.Error() != ttʗ1.intErr)) {
                    tΔ1.Errorf("%s: Number(%q).Int64() error:\n\tgot:  %v\n\twant: %v"u8, ttʗ1.Where, ttʗ1.@in, err, ttʗ1.intErr);
                }
            }
            {
                var (f, err) = n.Float64(); if (err == default! && ttʗ1.floatErr == ""u8 && f != ttʗ1.f){
                    tΔ1.Errorf("%s: Number(%q).Float64() = %g, want %g"u8, ttʗ1.Where, ttʗ1.@in, f, ttʗ1.f);
                } else 
                if ((err == default! && ttʗ1.floatErr != ""u8) || (err != default! && err.Error() != ttʗ1.floatErr)) {
                    tΔ1.Errorf("%s: Number(%q).Float64() error:\n\tgot  %v\n\twant: %v"u8, ttʗ1.Where, ttʗ1.@in, err, ttʗ1.floatErr);
                }
            }
        });
    }
}

public static void TestLargeByteSlice(ж<testing.T> Ꮡt) {
    var s0 = new slice<byte>(2000);
    foreach (var (i, _) in s0) {
        s0[i] = (byte)i;
    }
    var (b, err) = Marshal(s0);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    ref var s1 = ref heap<slice<byte>>(out var Ꮡs1);
    {
        var errΔ1 = Unmarshal(b, Ꮡs1); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, errΔ1);
        }
    }
    if (!bytes.Equal(s0, s1)) {
        Ꮡt.Errorf("Marshal:"u8);
        diff(Ꮡt, s0, s1);
    }
}

[GoType] public partial struct Xint {
    public nint X;
}

public static void TestUnmarshalInterface(ж<testing.T> Ꮡt) {
    ref var xint = ref heap(new Xint(), out var Ꮡxint);
    ref var i = ref heap<any>(out var Ꮡi);

    i = Ꮡxint;
    {
        var err = Unmarshal(slice<byte>(@"{""X"":1}"u8), Ꮡi); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
        }
    }
    if (xint.X != 1) {
        Ꮡt.Fatalf("xint.X = %d, want 1"u8, xint.X);
    }
}

public static void TestUnmarshalPtrPtr(ж<testing.T> Ꮡt) {
    ref var xint = ref heap(new Xint(), out var Ꮡxint);
    ref var pxint = ref heap<ж<Xint>>(out var Ꮡpxint);
    pxint = Ꮡxint;
    {
        var err = Unmarshal(slice<byte>(@"{""X"":1}"u8), Ꮡpxint); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal: %v"u8, err);
        }
    }
    if (xint.X != 1) {
        Ꮡt.Fatalf("xint.X = %d, want 1"u8, xint.X);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string inputᶜ = "\"foobar\"<html> [\u2028 \u2029]";

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = @"""\""foobar\""\u003chtml\u003e [\u2028 \u2029]"""u8;

public static void TestEscape(ж<testing.T> Ꮡt) {
    @string input = inputᶜ;
    @string want = wantᶜ;
    var (got, err) = Marshal(input);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal(%#q):\n\tgot:  %s\n\twant: %s"u8, input, got, want);
    }
}

// WrongString is a struct that's misusing the ,string modifier.
[GoType("dyn")] [GoLocalName("WrongString")] internal partial struct TestErrorMessageFromMisusedString_WrongString {
    [GoTag(@"json:""result,string""")]
    public @string Message;
}

[GoType("dyn")] internal partial struct TestErrorMessageFromMisusedString_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in, err;
}

// If people misuse the ,string modifier, the error message should be
// helpful, telling the user that they're doing it wrong.
public static void TestErrorMessageFromMisusedString(ж<testing.T> Ꮡt) {
    var tests = new TestErrorMessageFromMisusedString_tests[]{
        new(Name(""u8), @"{""result"":""x""}"u8, @"json: invalid use of ,string struct tag, trying to unmarshal ""x"" into string"u8),
        new(Name(""u8), @"{""result"":""foo""}"u8, @"json: invalid use of ,string struct tag, trying to unmarshal ""foo"" into string"u8),
        new(Name(""u8), @"{""result"":""123""}"u8, @"json: invalid use of ,string struct tag, trying to unmarshal ""123"" into string"u8),
        new(Name(""u8), @"{""result"":123}"u8, @"json: invalid use of ,string struct tag, trying to unmarshal unquoted value into string"u8),
        new(Name(""u8), @"{""result"":""\""""}"u8, @"json: invalid use of ,string struct tag, trying to unmarshal ""\"""" into string"u8),
        new(Name(""u8), @"{""result"":""\""foo""}"u8, @"json: invalid use of ,string struct tag, trying to unmarshal ""\""foo"" into string"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestErrorMessageFromMisusedString_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var r = strings.NewReader(ttʗ1.@in);
            ref var s = ref heap(new TestErrorMessageFromMisusedString_WrongString(), out var Ꮡs);
            var err = NewDecoder(new json_test_package.strings_ReaderжReader(r)).Decode(Ꮡs);
            @string got = fmt.Sprintf("%v"u8, err);
            if (got != ttʗ1.err) {
                tΔ1.Errorf("%s: Decode error:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, got, ttʗ1.err);
            }
        });
    }
}

[GoType] public partial struct All {
    public bool Bool;
    public nint Int;
    public int8 Int8;
    public int16 Int16;
    public int32 Int32;
    public int64 Int64;
    public nuint Uint;
    public uint8 Uint8;
    public uint16 Uint16;
    public uint32 Uint32;
    public uint64 Uint64;
    public uintptr Uintptr;
    public float32 Float32;
    public float64 Float64;
    [GoTag(@"json:""bar""")]
    public @string Foo;
    [GoTag(@"json:""bar2,dummyopt""")]
    public @string Foo2;
    [GoTag(@"json:"",string""")]
    public int64 IntStr;
    [GoTag(@"json:"",string""")]
    public uintptr UintptrStr;
    public ж<bool> PBool;
    public ж<nint> PInt;
    public ж<int8> PInt8;
    public ж<int16> PInt16;
    public ж<int32> PInt32;
    public ж<int64> PInt64;
    public ж<nuint> PUint;
    public ж<uint8> PUint8;
    public ж<uint16> PUint16;
    public ж<uint32> PUint32;
    public ж<uint64> PUint64;
    public ж<uintptr> PUintptr;
    public ж<float32> PFloat32;
    public ж<float64> PFloat64;
    public @string String;
    public ж<@string> PString;
    public map<@string, Small> Map;
    public map<@string, ж<Small>> MapP;
    public ж<map<@string, Small>> PMap;
    public ж<map<@string, ж<Small>>> PMapP;
    public map<@string, Small> EmptyMap;
    public map<@string, Small> NilMap;
    public slice<Small> Slice;
    public slice<ж<Small>> SliceP;
    public ж<slice<Small>> PSlice;
    public ж<slice<ж<Small>>> PSliceP;
    public slice<Small> EmptySlice;
    public slice<Small> NilSlice;
    public slice<@string> StringSlice;
    public slice<byte> ByteSlice;
    public Small Small;
    public ж<Small> PSmall;
    public ж<ж<Small>> PPSmall;
    public any Interface;
    public ж<any> PInterface;
    internal nint unexported;
}

[GoType] public partial struct Small {
    public @string Tag;
}

internal static ж<All> ᏑallValue = new StandardBox<All>(new All(
    Bool: true,
    Int: 2,
    Int8: 3,
    Int16: 4,
    Int32: 5,
    Int64: 6,
    Uint: 7,
    Uint8: 8,
    Uint16: 9,
    Uint32: 10,
    Uint64: 11,
    Uintptr: 12,
    Float32: 14.1F,
    Float64: 15.1D,
    Foo: "foo"u8,
    Foo2: "foo2"u8,
    IntStr: 42,
    UintptrStr: 44,
    String: "16"u8,
    Map: new map<@string, Small>{
        ["17"u8] = new(Tag: "tag17"u8),
        ["18"u8] = new(Tag: "tag18"u8)
    },
    MapP: new map<@string, ж<Small>>{
        ["19"u8] = Ꮡ(new Small(Tag: "tag19"u8)),
        ["20"u8] = default!
    },
    EmptyMap: new map<@string, Small>{},
    Slice: new Small[]{new(Tag: "tag20"u8), new(Tag: "tag21"u8)}.slice(),
    SliceP: new ж<Small>[]{Ꮡ(new Small(Tag: "tag22"u8)), default!, Ꮡ(new Small(Tag: "tag23"u8))}.slice(),
    EmptySlice: new Small[]{}.slice(),
    StringSlice: new @string[]{"str24"u8, "str25"u8, "str26"u8}.slice(),
    ByteSlice: new byte[]{27, 28, 29}.slice(),
    Small: new Small(Tag: "tag30"u8),
    PSmall: Ꮡ(new Small(Tag: "tag31"u8)),
    Interface: 5.2D
));
internal static ref All allValue => ref ᏑallValue.Value;

internal static ж<All> ᏑpallValue = new StandardBox<All>(new All(
    PBool: ᏑallValue.of(All.ᏑBool),
    PInt: ᏑallValue.of(All.ᏑInt),
    PInt8: ᏑallValue.of(All.ᏑInt8),
    PInt16: ᏑallValue.of(All.ᏑInt16),
    PInt32: ᏑallValue.of(All.ᏑInt32),
    PInt64: ᏑallValue.of(All.ᏑInt64),
    PUint: ᏑallValue.of(All.ᏑUint),
    PUint8: ᏑallValue.of(All.ᏑUint8),
    PUint16: ᏑallValue.of(All.ᏑUint16),
    PUint32: ᏑallValue.of(All.ᏑUint32),
    PUint64: ᏑallValue.of(All.ᏑUint64),
    PUintptr: ᏑallValue.of(All.ᏑUintptr),
    PFloat32: ᏑallValue.of(All.ᏑFloat32),
    PFloat64: ᏑallValue.of(All.ᏑFloat64),
    PString: ᏑallValue.of(All.ᏑString),
    PMap: ᏑallValue.of(All.ᏑMap),
    PMapP: ᏑallValue.of(All.ᏑMapP),
    PSlice: ᏑallValue.of(All.ᏑSlice),
    PSliceP: ᏑallValue.of(All.ᏑSliceP),
    PPSmall: ᏑallValue.of(All.ᏑPSmall),
    PInterface: ᏑallValue.of(All.ᏑInterface)
));
internal static ref All pallValue => ref ᏑpallValue.Value;

internal static @string allValueIndent = """
{
	"Bool": true,
	"Int": 2,
	"Int8": 3,
	"Int16": 4,
	"Int32": 5,
	"Int64": 6,
	"Uint": 7,
	"Uint8": 8,
	"Uint16": 9,
	"Uint32": 10,
	"Uint64": 11,
	"Uintptr": 12,
	"Float32": 14.1,
	"Float64": 15.1,
	"bar": "foo",
	"bar2": "foo2",
	"IntStr": "42",
	"UintptrStr": "44",
	"PBool": null,
	"PInt": null,
	"PInt8": null,
	"PInt16": null,
	"PInt32": null,
	"PInt64": null,
	"PUint": null,
	"PUint8": null,
	"PUint16": null,
	"PUint32": null,
	"PUint64": null,
	"PUintptr": null,
	"PFloat32": null,
	"PFloat64": null,
	"String": "16",
	"PString": null,
	"Map": {
		"17": {
			"Tag": "tag17"
		},
		"18": {
			"Tag": "tag18"
		}
	},
	"MapP": {
		"19": {
			"Tag": "tag19"
		},
		"20": null
	},
	"PMap": null,
	"PMapP": null,
	"EmptyMap": {},
	"NilMap": null,
	"Slice": [
		{
			"Tag": "tag20"
		},
		{
			"Tag": "tag21"
		}
	],
	"SliceP": [
		{
			"Tag": "tag22"
		},
		null,
		{
			"Tag": "tag23"
		}
	],
	"PSlice": null,
	"PSliceP": null,
	"EmptySlice": [],
	"NilSlice": null,
	"StringSlice": [
		"str24",
		"str25",
		"str26"
	],
	"ByteSlice": "Gxwd",
	"Small": {
		"Tag": "tag30"
	},
	"PSmall": {
		"Tag": "tag31"
	},
	"PPSmall": null,
	"Interface": 5.2,
	"PInterface": null
}
"""u8;

internal static @string allValueCompact = stripWhitespace(allValueIndent);

internal static @string pallValueIndent = """
{
	"Bool": false,
	"Int": 0,
	"Int8": 0,
	"Int16": 0,
	"Int32": 0,
	"Int64": 0,
	"Uint": 0,
	"Uint8": 0,
	"Uint16": 0,
	"Uint32": 0,
	"Uint64": 0,
	"Uintptr": 0,
	"Float32": 0,
	"Float64": 0,
	"bar": "",
	"bar2": "",
        "IntStr": "0",
	"UintptrStr": "0",
	"PBool": true,
	"PInt": 2,
	"PInt8": 3,
	"PInt16": 4,
	"PInt32": 5,
	"PInt64": 6,
	"PUint": 7,
	"PUint8": 8,
	"PUint16": 9,
	"PUint32": 10,
	"PUint64": 11,
	"PUintptr": 12,
	"PFloat32": 14.1,
	"PFloat64": 15.1,
	"String": "",
	"PString": "16",
	"Map": null,
	"MapP": null,
	"PMap": {
		"17": {
			"Tag": "tag17"
		},
		"18": {
			"Tag": "tag18"
		}
	},
	"PMapP": {
		"19": {
			"Tag": "tag19"
		},
		"20": null
	},
	"EmptyMap": null,
	"NilMap": null,
	"Slice": null,
	"SliceP": null,
	"PSlice": [
		{
			"Tag": "tag20"
		},
		{
			"Tag": "tag21"
		}
	],
	"PSliceP": [
		{
			"Tag": "tag22"
		},
		null,
		{
			"Tag": "tag23"
		}
	],
	"EmptySlice": null,
	"NilSlice": null,
	"StringSlice": null,
	"ByteSlice": null,
	"Small": {
		"Tag": ""
	},
	"PSmall": null,
	"PPSmall": {
		"Tag": "tag31"
	},
	"Interface": null,
	"PInterface": 5.2
}
"""u8;

internal static @string pallValueCompact = stripWhitespace(pallValueIndent);

[GoType("dyn")] [GoLocalName("S")] internal partial struct TestRefUnmarshal_S {
    // Ref is defined in encode_test.go.
    public Ref R0;
    public ж<Ref> R1;
    public RefText R2;
    public ж<RefText> R3;
}

public static void TestRefUnmarshal(ж<testing.T> Ꮡt) {
    var want = new TestRefUnmarshal_S(
        R0: 12,
        R1: @new<Ref>(),
        R2: 13,
        R3: @new<RefText>()
    );
    want.R1.Value = 12;
    want.R3.Value = 13;
    ref var got = ref heap(new TestRefUnmarshal_S(), out var Ꮡgot);
    {
        var err = Unmarshal(slice<byte>(@"{""R0"":""ref"",""R1"":""ref"",""R2"":""ref"",""R3"":""ref""}"u8), Ꮡgot); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
        }
    }
    if (!reflect.DeepEqual(got, want)) {
        Ꮡt.Errorf("Unmarsha:\n\tgot:  %+v\n\twant: %+v"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string number11Number2ˢ = @"{""Number1"":""1"", ""Number2"":""""}"u8;

[GoType("dyn")] [GoLocalName("T2")] internal partial struct TestEmptyString_T2 {
    [GoTag(@"json:"",string""")]
    public nint Number1;
    [GoTag(@"json:"",string""")]
    public nint Number2;
}

// Test that the empty string doesn't panic decoding when ,string is specified
// Issue 3450
public static void TestEmptyString(ж<testing.T> Ꮡt) {
    @string data = number11Number2ˢ;
    var dec = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(data)));
    ref var got = ref heap(new TestEmptyString_T2(), out var Ꮡgot);
    {
        var err = dec.Decode(Ꮡgot);
        switch (ᐧ) {
        case {} when err == default!: {
            Ꮡt.Fatalf("Decode error: got nil, want non-nil"u8);
            break;
        }
        case {} when got.Number1 is not 1: {
            Ꮡt.Fatalf("Decode: got.Number1 = %d, want 1"u8, got.Number1);
            break;
        }}
    }

}

[GoType("dyn")] [GoLocalName("T")] internal partial struct TestNullString_T {
    [GoTag(@"json:"",string""")]
    public nint A;
    [GoTag(@"json:"",string""")]
    public nint B;
    [GoTag(@"json:"",string""")]
    public ж<nint> C;
}

// Test that a null for ,string is not replaced with the previous quoted string (issue 7046).
// It should also not be an error (issue 2540, issue 8587).
public static void TestNullString(ж<testing.T> Ꮡt) {
    var data = slice<byte>(@"{""A"": ""1"", ""B"": null, ""C"": null}"u8);
    ref var s = ref heap(new TestNullString_T(), out var Ꮡs);
    s.B = 1;
    s.C = @new<nint>();
    s.C.Value = 2;
    {
        var err = Unmarshal(data, Ꮡs);
        switch (ᐧ) {
        case {} when err != default!: {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
            break;
        }
        case {} when s.B is not 1: {
            Ꮡt.Fatalf("Unmarshal: s.B = %d, want 1"u8, s.B);
            break;
        }
        case {} when s.C != nil: {
            Ꮡt.Fatalf("Unmarshal: s.C = %d, want non-nil"u8, s.C.OrTypedNil());
            break;
        }}
    }

}

internal static ж<T> addr<T>(T vʗp) {
    ref var v = ref heap(vʗp, out var Ꮡv);

    return Ꮡv;
}

[GoType("dyn")] internal partial struct TestInterfaceSet_tests {
    public partial ref CaseName CaseName { get; }
    internal any pre;
    internal @string json;
    internal any post;
}

[GoType("dyn")] internal partial struct TestInterfaceSet_b {
    public any X;
}

public static void TestInterfaceSet(ж<testing.T> Ꮡt) {
    var errUnmarshal = Ꮡ(new UnmarshalTypeError(Value: "object"u8, Offset: 6, Type: reflect.TypeFor<nint>(), Field: "X"u8));
    var tests = new TestInterfaceSet_tests[]{
        new(Name(""u8), (@string)"foo"u8, @"""bar"""u8, (@string)"bar"u8),
        new(Name(""u8), (@string)"foo"u8, @"2"u8, 2.0D),
        new(Name(""u8), (@string)"foo"u8, @"true"u8, true),
        new(Name(""u8), (@string)"foo"u8, @"null"u8, default!),
        new(Name(""u8), new map<@string, any>{}, @"true"u8, true),
        new(Name(""u8), new @string[]{}.slice(), @"true"u8, true),
        new(Name(""u8), ((any)default!), @"null"u8, ((any)default!)),
        new(Name(""u8), ((ж<nint>)nil), @"null"u8, ((any)default!)),
        new(Name(""u8), addr((nint)(0)).OrTypedNil(), @"null"u8, ((any)default!)),
        new(Name(""u8), addr((nint)(1)).OrTypedNil(), @"null"u8, ((any)default!)),
        new(Name(""u8), ((ж<ж<nint>>)nil), @"null"u8, ((any)default!)),
        new(Name(""u8), addr<ж<nint>>(nil).OrTypedNil(), @"null"u8, addr<ж<nint>>(nil).OrTypedNil()),
        new(Name(""u8), addr(addr((nint)(1))).OrTypedNil(), @"null"u8, addr<ж<nint>>(nil).OrTypedNil()),
        new(Name(""u8), ((ж<ж<ж<nint>>>)nil), @"null"u8, ((any)default!)),
        new(Name(""u8), addr<ж<ж<nint>>>(nil).OrTypedNil(), @"null"u8, addr<ж<ж<nint>>>(nil).OrTypedNil()),
        new(Name(""u8), addr(addr<ж<nint>>(nil)).OrTypedNil(), @"null"u8, addr<ж<ж<nint>>>(nil).OrTypedNil()),
        new(Name(""u8), addr(addr(addr((nint)(1)))).OrTypedNil(), @"null"u8, addr<ж<ж<nint>>>(nil).OrTypedNil()),
        new(Name(""u8), ((any)default!), @"2"u8, (float64)2D),
        new(Name(""u8), (nint)1, @"2"u8, (float64)2D),
        new(Name(""u8), ((ж<nint>)nil), @"2"u8, (float64)2D),
        new(Name(""u8), addr((nint)(0)).OrTypedNil(), @"2"u8, addr((nint)(2)).OrTypedNil()),
        new(Name(""u8), addr((nint)(1)).OrTypedNil(), @"2"u8, addr((nint)(2)).OrTypedNil()),
        new(Name(""u8), ((ж<ж<nint>>)nil), @"2"u8, (float64)2D),
        new(Name(""u8), addr<ж<nint>>(nil).OrTypedNil(), @"2"u8, addr(addr((nint)(2))).OrTypedNil()),
        new(Name(""u8), addr(addr((nint)(1))).OrTypedNil(), @"2"u8, addr(addr((nint)(2))).OrTypedNil()),
        new(Name(""u8), ((ж<ж<ж<nint>>>)nil), @"2"u8, (float64)2D),
        new(Name(""u8), addr<ж<ж<nint>>>(nil).OrTypedNil(), @"2"u8, addr(addr(addr((nint)(2)))).OrTypedNil()),
        new(Name(""u8), addr(addr<ж<nint>>(nil)).OrTypedNil(), @"2"u8, addr(addr(addr((nint)(2)))).OrTypedNil()),
        new(Name(""u8), addr(addr(addr((nint)(1)))).OrTypedNil(), @"2"u8, addr(addr(addr((nint)(2)))).OrTypedNil()),
        new(Name(""u8), ((any)default!), @"{}"u8, new map<@string, any>{}),
        new(Name(""u8), (nint)1, @"{}"u8, new map<@string, any>{}),
        new(Name(""u8), ((ж<nint>)nil), @"{}"u8, new map<@string, any>{}),
        new(Name(""u8), addr((nint)(0)).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr((nint)(1)).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil()),
        new(Name(""u8), ((ж<ж<nint>>)nil), @"{}"u8, new map<@string, any>{}),
        new(Name(""u8), addr<ж<nint>>(nil).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr(addr((nint)(1))).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil()),
        new(Name(""u8), ((ж<ж<ж<nint>>>)nil), @"{}"u8, new map<@string, any>{}),
        new(Name(""u8), addr<ж<ж<nint>>>(nil).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr(addr<ж<nint>>(nil)).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr(addr(addr((nint)(1)))).OrTypedNil(), @"{}"u8, errUnmarshal.OrTypedNil())
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestInterfaceSet_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            ref var b = ref heap<TestInterfaceSet_b>(out var Ꮡb);
            b = new TestInterfaceSet_b(ttʗ1.pre);
            @string blob = @"{""X"":"u8 + ttʗ1.json + @"}"u8;
            {
                var err = Unmarshal(slice<byte>(blob), Ꮡb); if (err != default!) {
                    {
                        var (wantErr, _) = ttʗ1.post._<error>(ᐧ); if (equalError(err, wantErr)) {
                            return;
                        }
                    }
                    tΔ1.Fatalf("%s: Unmarshal(%#q) error: %v"u8, ttʗ1.Where, blob, err);
                }
            }
            if (!reflect.DeepEqual(b.X, ttʗ1.post)) {
                tΔ1.Errorf("%s: Unmarshal(%#q):\n\tpre.X:  %#v\n\tgot.X:  %#v\n\twant.X: %#v"u8, ttʗ1.Where, blob, ttʗ1.pre, b.X, ttʗ1.post);
            }
        });
    }
}

[GoType] public partial struct NullTest {
    public bool Bool;
    public nint Int;
    public int8 Int8;
    public int16 Int16;
    public int32 Int32;
    public int64 Int64;
    public nuint Uint;
    public uint8 Uint8;
    public uint16 Uint16;
    public uint32 Uint32;
    public uint64 Uint64;
    public float32 Float32;
    public float64 Float64;
    public @string String;
    public ж<bool> PBool;
    public map<@string, @string> Map;
    public slice<@string> Slice;
    public any Interface;
    public ж<global::go.encoding.json_package.RawMessage> PRaw;
    public ж<time.Time> PTime;
    public ж<bigꓸInt> PBigInt;
    public ж<MustNotUnmarshalText> PText;
    public ж<bytes.Buffer> PBuffer; // has methods, just not relevant ones
    public ж<EmptyStruct> PStruct;
    public global::go.encoding.json_package.RawMessage Raw;
    public time.Time Time;
    public bigꓸInt BigInt;
    public MustNotUnmarshalText Text;
    public bytes.Buffer Buffer;
    public EmptyStruct Struct;
}

// JSON null values should be ignored for primitives and string values instead of resulting in an error.
// Issue 2540
public static void TestUnmarshalNulls(ж<testing.T> Ꮡt) {
    // Unmarshal docs:
    // The JSON null value unmarshals into an interface, map, pointer, or slice
    // by setting that Go value to nil. Because null is often used in JSON to mean
    // ``not present,'' unmarshaling a JSON null into any other Go type has no effect
    // on the value and produces no error.
    var jsonData = slice<byte>("""
{
				"Bool"    : null,
				"Int"     : null,
				"Int8"    : null,
				"Int16"   : null,
				"Int32"   : null,
				"Int64"   : null,
				"Uint"    : null,
				"Uint8"   : null,
				"Uint16"  : null,
				"Uint32"  : null,
				"Uint64"  : null,
				"Float32" : null,
				"Float64" : null,
				"String"  : null,
				"PBool": null,
				"Map": null,
				"Slice": null,
				"Interface": null,
				"PRaw": null,
				"PTime": null,
				"PBigInt": null,
				"PText": null,
				"PBuffer": null,
				"PStruct": null,
				"Raw": null,
				"Time": null,
				"BigInt": null,
				"Text": null,
				"Buffer": null,
				"Struct": null
			}
"""u8);
    ref var nulls = ref heap<NullTest>(out var Ꮡnulls);
    nulls = new NullTest(
        Bool: true,
        Int: 2,
        Int8: 3,
        Int16: 4,
        Int32: 5,
        Int64: 6,
        Uint: 7,
        Uint8: 8,
        Uint16: 9,
        Uint32: 10,
        Uint64: 11,
        Float32: 12.1F,
        Float64: 13.1D,
        String: "14"u8,
        PBool: @new<bool>(),
        Map: new map<@string, @string>{},
        Slice: new @string[]{}.slice(),
        Interface: @new<MustNotUnmarshalJSON>(),
        PRaw: @new<global::go.encoding.json_package.RawMessage>(),
        PTime: @new<time.Time>(),
        PBigInt: @new<bigꓸInt>(),
        PText: @new<MustNotUnmarshalText>(),
        PStruct: @new<EmptyStruct>(),
        PBuffer: @new<bytes.Buffer>(),
        Raw: ((global::go.encoding.json_package.RawMessage)slice<byte>((@string)"123"u8)),
        Time: time.Unix(123456789, 0),
        BigInt: big.NewInt(123).Value
    );
    @string before = nulls.Time.String();
    var err = Unmarshal(jsonData, Ꮡnulls);
    if (err != default!) {
        Ꮡt.Errorf("Unmarshal of null values failed: %v"u8, err);
    }
    if (!nulls.Bool || nulls.Int != 2 || nulls.Int8 != 3 || nulls.Int16 != 4 || nulls.Int32 != 5 || nulls.Int64 != 6 || nulls.Uint != 7 || nulls.Uint8 != 8 || nulls.Uint16 != 9 || nulls.Uint32 != 10 || nulls.Uint64 != 11 || nulls.Float32 != 12.1F || nulls.Float64 != 13.1D || nulls.String != "14"u8) {
        Ꮡt.Errorf("Unmarshal of null values affected primitives"u8);
    }
    if (nulls.PBool != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PBool"u8);
    }
    if (nulls.Map != default!) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.Map"u8);
    }
    if (nulls.Slice != default!) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.Slice"u8);
    }
    if (nulls.Interface != default!) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.Interface"u8);
    }
    if (nulls.PRaw != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PRaw"u8);
    }
    if (nulls.PTime != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PTime"u8);
    }
    if (nulls.PBigInt != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PBigInt"u8);
    }
    if (nulls.PText != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PText"u8);
    }
    if (nulls.PBuffer != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PBuffer"u8);
    }
    if (nulls.PStruct != nil) {
        Ꮡt.Errorf("Unmarshal of null did not clear nulls.PStruct"u8);
    }
    if (((@string)(slice<byte>)nulls.Raw) != "null"u8) {
        Ꮡt.Errorf("Unmarshal of RawMessage null did not record null: %v"u8, ((@string)(slice<byte>)nulls.Raw));
    }
    if (nulls.Time.String() != before) {
        Ꮡt.Errorf("Unmarshal of time.Time null set time to %v"u8, nulls.Time.String());
    }
    if (Ꮡnulls.of(NullTest.ᏑBigInt).String() != "123"u8) {
        Ꮡt.Errorf("Unmarshal of big.Int null set int to %v"u8, Ꮡnulls.of(NullTest.ᏑBigInt).String());
    }
}

[GoType] public partial struct MustNotUnmarshalJSON {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mustNotUnmarshalJSONWasˢ = "MustNotUnmarshalJSON was used"u8;

public static error UnmarshalJSON(this MustNotUnmarshalJSON x, slice<byte> data) {
    return errors.New(mustNotUnmarshalJSONWasˢ);
}

[GoType] public partial struct MustNotUnmarshalText {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mustNotUnmarshalTextWasˢ = "MustNotUnmarshalText was used"u8;

public static error UnmarshalText(this MustNotUnmarshalText x, slice<byte> text) {
    return errors.New(mustNotUnmarshalTextWasˢ);
}

[GoLocalName("stringKind")] [GoType("@string")] internal partial struct TestStringKind_stringKind;

public static void TestStringKind(ж<testing.T> Ꮡt) {
    var want = new map<TestStringKind_stringKind, nint>{["foo"u8] = 42};
    var (data, err) = Marshal(want);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    ref var got = ref heap<map<TestStringKind_stringKind, nint>>(out var Ꮡgot);
    err = Unmarshal(data, Ꮡgot);
    if (err != default!) {
        Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
    }
    if (!maps.Equal<map<TestStringKind_stringKind, nint>, map<TestStringKind_stringKind, nint>, TestStringKind_stringKind, nint>(got, want)) {
        Ꮡt.Fatalf("Marshal/Unmarshal mismatch:\n\tgot:  %v\n\twant: %v"u8, got, want);
    }
}

[GoLocalName("byteKind")] [GoType("[]byte")] internal partial struct TestByteKind_byteKind;

// Custom types with []byte as underlying type could not be marshaled
// and then unmarshaled.
// Issue 8962.
public static void TestByteKind(ж<testing.T> Ꮡt) {
    var want = ((TestByteKind_byteKind)slice<byte>((@string)"hello"u8));
    var (data, err) = Marshal(want);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    ref var got = ref heap<TestByteKind_byteKind>(out var Ꮡgot);
    err = Unmarshal(data, Ꮡgot);
    if (err != default!) {
        Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
    }
    if (!slices.Equal<TestByteKind_byteKind, byte>(got, want)) {
        Ꮡt.Fatalf("Marshal/Unmarshal mismatch:\n\tgot:  %v\n\twant: %v"u8, got, want);
    }
}

[GoLocalName("Uint8")] [GoType("num:uint8")] internal partial struct TestSliceOfCustomByte_Uint8;

// The fix for issue 8962 introduced a regression.
// Issue 12921.
public static void TestSliceOfCustomByte(ж<testing.T> Ꮡt) {
    var want = widen<uint8, TestSliceOfCustomByte_Uint8>(slice<uint8>((@string)"hello"u8), elemᴛ0 => (TestSliceOfCustomByte_Uint8)elemᴛ0);
    var (data, err) = Marshal(want);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    ref var got = ref heap<slice<TestSliceOfCustomByte_Uint8>>(out var Ꮡgot);
    err = Unmarshal(data, Ꮡgot);
    if (err != default!) {
        Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
    }
    if (!slices.Equal<slice<TestSliceOfCustomByte_Uint8>, TestSliceOfCustomByte_Uint8>(got, want)) {
        Ꮡt.Fatalf("Marshal/Unmarshal mismatch:\n\tgot:  %v\n\twant: %v"u8, got, want);
    }
}

[GoType("dyn")] internal partial struct TestUnmarshalTypeError_tests {
    public partial ref CaseName CaseName { get; }
    internal any dest;
    internal @string @in;
}

public static void TestUnmarshalTypeError(ж<testing.T> Ꮡt) {
    var tests = new TestUnmarshalTypeError_tests[]{
        new(Name(""u8), @new<@string>(), @"{""user"": ""name""}"u8), // issue 4628.

        new(Name(""u8), @new<error>(), @"{}"u8), // issue 4222

        new(Name(""u8), @new<error>(), @"[]"u8),
        new(Name(""u8), @new<error>(), @""""""u8),
        new(Name(""u8), @new<error>(), @"123"u8),
        new(Name(""u8), @new<error>(), @"true"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestUnmarshalTypeError_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var err = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.dest);
            {
                var (_, ok) = err._<ж<global::go.encoding.json_package.UnmarshalTypeError>>(ᐧ); if (!ok) {
                    tΔ1.Errorf("%s: Unmarshal(%#q, %T):\n\tgot:  %T\n\twant: %T"u8,
                        ttʗ1.Where, ttʗ1.@in, ttʗ1.dest, err, @new<global::go.encoding.json_package.UnmarshalTypeError>());
                }
            }
        });
    }
}

[GoType("dyn")] internal partial struct TestUnmarshalSyntax_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
}

public static void TestUnmarshalSyntax(ж<testing.T> Ꮡt) {
    ref var x = ref heap<any>(out var Ꮡx);
    var tests = new TestUnmarshalSyntax_tests[]{
        new(Name(""u8), "tru"u8),
        new(Name(""u8), "fals"u8),
        new(Name(""u8), "nul"u8),
        new(Name(""u8), "123e"u8),
        new(Name(""u8), @"""hello"u8),
        new(Name(""u8), @"[1,2,3"u8),
        new(Name(""u8), @"{""key"":1"u8),
        new(Name(""u8), @"{""key"":1,"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestUnmarshalSyntax_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var err = Unmarshal(slice<byte>(ttʗ1.@in), Ꮡx);
            {
                var (_, ok) = err._<ж<global::go.encoding.json_package.SyntaxError>>(ᐧ); if (!ok) {
                    tΔ1.Errorf("%s: Unmarshal(%#q, any):\n\tgot:  %T\n\twant: %T"u8,
                        ttʗ1.Where, ttʗ1.@in, err, @new<global::go.encoding.json_package.SyntaxError>());
                }
            }
        });
    }
}

// Test handling of unexported fields that should be ignored.
// Issue 4660
[GoType] internal partial struct unexportedFields {
    public @string Name;
    [GoTag(@"json:""-""")]
    internal map<@string, any> m;
    [GoTag(@"json:""abcd""")]
    internal map<@string, any> m2;
    [GoTag(@"json:""-""")]
    internal slice<nint> s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameBobMX123M2Y456AbcdZˢ = @"{""Name"": ""Bob"", ""m"": {""x"": 123}, ""m2"": {""y"": 456}, ""abcd"": {""z"": 789}, ""s"": [2, 3]}"u8;

public static void TestUnmarshalUnexported(ж<testing.T> Ꮡt) {
    @string input = nameBobMX123M2Y456AbcdZˢ;
    var want = Ꮡ(new unexportedFields(Name: "Bob"u8));
    var @out = Ꮡ(new unexportedFields(nil));
    var err = Unmarshal(slice<byte>(input), @out.OrTypedNil());
    if (err != default!) {
        Ꮡt.Errorf("Unmarshal error: %v"u8, err);
    }
    if (!reflect.DeepEqual(@out.OrTypedNil(), want.OrTypedNil())) {
        Ꮡt.Errorf("Unmarshal:\n\tgot:  %+v\n\twant: %+v"u8, @out.OrTypedNil(), want.OrTypedNil());
    }
}

[GoType("global::go.time_package.Time")] public partial struct Time3339;

[GoRecv] public static error UnmarshalJSON(this ref Time3339 t, slice<byte> b) {
    if (len(b) < 2 || b[0] != (rune)'"' || b[len(b) - 1] != (rune)'"') {
        return fmt.Errorf("types: failed to unmarshal non-string value %q as an RFC 3339 time"u8, b);
    }
    var (tm, err) = time.Parse(time.RFC3339, ((@string)(b.slice(1, len(b) - 1))));
    if (err != default!) {
        return err;
    }
    t = ((Time3339)tm);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangeˢ = "range"u8;

public static void TestUnmarshalJSONLiteralError(ж<testing.T> Ꮡt) {
    ref var t3 = ref heap(new Time3339(), out var Ꮡt3);
    {
        var err = Unmarshal(slice<byte>(@"""0000-00-00T00:00:00Z"""u8), Ꮡt3);
        switch (ᐧ) {
        case {} when err == default!: {
            Ꮡt.Fatalf("Unmarshal error: got nil, want non-nil"u8);
            break;
        }
        case {} when !strings.Contains(err.Error(), rangeˢ): {
            Ꮡt.Errorf("Unmarshal error:\n\tgot:  %v\n\twant: out of range"u8, err);
            break;
        }}
    }

}

// Test that extra object elements in an array do not result in a
// "data changing underfoot" error.
// Issue 3717
public static void TestSkipArrayObjects(ж<testing.T> Ꮡt) {
    @string json = @"[{}]"u8;
    ref var dest = ref heap(new array<any>(0), out var Ꮡdest);
    var err = Unmarshal(slice<byte>(json), Ꮡdest);
    if (err != default!) {
        Ꮡt.Errorf("Unmarshal error: %v"u8, err);
    }
}

[GoType("dyn")] internal partial struct TestPrefilled_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal any ptr;
    internal any @out;
}

// Test semantics of pre-filled data, such as struct fields, map elements,
// slices, and arrays.
// Issues 4900 and 8837, among others.
public static void TestPrefilled(ж<testing.T> Ꮡt) {
    // Values here change, cannot reuse table across runs.
    var tests = new TestPrefilled_tests[]{new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1, ""Y"": 2}"u8,
        ptr: Ꮡ(new XYZ(X: (float32)3F, Y: (int16)4, Z: 1.5D)),
        @out: Ꮡ(new XYZ(X: (float64)1D, Y: (float64)2D, Z: 1.5D))
    ), new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1, ""Y"": 2}"u8,
        ptr: Ꮡ(new map<@string, any>{["X"u8] = (float32)3F, ["Y"u8] = (int16)4, ["Z"u8] = 1.5D}),
        @out: Ꮡ(new map<@string, any>{["X"u8] = (float64)1D, ["Y"u8] = (float64)2D, ["Z"u8] = 1.5D})
    ), new(
        CaseName: Name(""u8),
        @in: @"[2]"u8,
        ptr: Ꮡ(new nint[]{1}.slice()),
        @out: Ꮡ(new nint[]{2}.slice())
    ), new(
        CaseName: Name(""u8),
        @in: @"[2, 3]"u8,
        ptr: Ꮡ(new nint[]{1}.slice()),
        @out: Ꮡ(new nint[]{2, 3}.slice())
    ), new(
        CaseName: Name(""u8),
        @in: @"[2, 3]"u8,
        ptr: Ꮡ(new nint[]{1}.array()),
        @out: Ꮡ(new nint[]{2}.array())
    ), new(
        CaseName: Name(""u8),
        @in: @"[3]"u8,
        ptr: Ꮡ(new nint[]{1, 2}.array()),
        @out: Ꮡ(new nint[]{3, 0}.array())
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestPrefilled_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            @string ptrstr = fmt.Sprintf("%v"u8, ttʗ1.ptr);
            var err = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.ptr); // tt.ptr edited here
            if (err != default!) {
                tΔ1.Errorf("%s: Unmarshal error: %v"u8, ttʗ1.Where, err);
            }
            if (!reflect.DeepEqual(ttʗ1.ptr, ttʗ1.@out)) {
                tΔ1.Errorf("%s: Unmarshal(%#q, %T):\n\tgot:  %v\n\twant: %v"u8, ttʗ1.Where, ttʗ1.@in, ptrstr, ttʗ1.ptr, ttʗ1.@out);
            }
        });
    }
}

[GoType("dyn")] internal partial struct TestInvalidUnmarshal_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal any v;
    internal error wantErr;
}

public static void TestInvalidUnmarshal(ж<testing.T> Ꮡt) {
    var tests = new TestInvalidUnmarshal_tests[]{
        new(Name(""u8), @"{""a"":""1""}"u8, default!, new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(nil)))),
        new(Name(""u8), @"{""a"":""1""}"u8, new EmptyStruct(), new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<EmptyStruct>())))),
        new(Name(""u8), @"{""a"":""1""}"u8, ((ж<nint>)nil), new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<ж<nint>>())))),
        new(Name(""u8), @"123"u8, default!, new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(nil)))),
        new(Name(""u8), @"123"u8, new EmptyStruct(), new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<EmptyStruct>())))),
        new(Name(""u8), @"123"u8, ((ж<nint>)nil), new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<ж<nint>>())))),
        new(Name(""u8), @"123"u8, @new<net.IP>(), new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number"u8, Type: reflect.TypeFor<ж<net.IP>>(), Offset: 3))))
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestInvalidUnmarshal_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            {
                var gotErr = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.v);
                switch (ᐧ) {
                case {} when gotErr == default!: {
                    tΔ1.Fatalf("%s: Unmarshal error: got nil, want non-nil"u8, ttʗ1.Where);
                    break;
                }
                case {} when !reflect.DeepEqual(gotErr, ttʗ1.wantErr): {
                    tΔ1.Errorf("%s: Unmarshal error:\n\tgot:  %#v\n\twant: %#v"u8, ttʗ1.Where, gotErr, ttʗ1.wantErr);
                    break;
                }}
            }

        });
    }
}

[GoType("dyn")] [GoValueClone("A")] internal partial struct TestInvalidStringOption_item {
    [GoTag(@"json:"",string""")]
    public time.Time T;
    [GoTag(@"json:"",string""")]
    public map<@string, @string> M;
    [GoTag(@"json:"",string""")]
    public slice<@string> S;
    [GoTag(@"json:"",string""")]
    public array<@string> A = new(1);
    [GoTag(@"json:"",string""")]
    public any I;
    [GoTag(@"json:"",string""")]
    public ж<nint> P;
}

// Test that string option is ignored for invalid types.
// Issue 9812.
public static void TestInvalidStringOption(ж<testing.T> Ꮡt) {
    ref var num = ref heap<nint>(out var Ꮡnum);
    num = 0;
    ref var item = ref heap<TestInvalidStringOption_item>(out var Ꮡitem);
    item = new TestInvalidStringOption_item(M: new map<@string, @string>(), S: new slice<@string>(0), I: num, P: Ꮡnum);
    var (data, err) = Marshal(item);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    err = Unmarshal(data, Ꮡitem);
    if (err != default!) {
        Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
    }
}

[GoType("dyn")] [GoLocalName("embed1")] internal partial struct TestUnmarshalEmbeddedUnexported_embed1 {
    public nint Q;
}

[GoType("dyn")] [GoLocalName("embed2")] internal partial struct TestUnmarshalEmbeddedUnexported_embed2 {
    public nint Q;
}

[GoType("dyn")] [GoLocalName("embed3")] internal partial struct TestUnmarshalEmbeddedUnexported_embed3 {
    [GoTag(@"json:"",string""")]
    public int64 Q;
}

[GoType("dyn")] [GoLocalName("S1")] internal partial struct TestUnmarshalEmbeddedUnexported_S1 {
    internal partial ref ж<TestUnmarshalEmbeddedUnexported_embed1> embed1 { get; }
    public nint R;
}

[GoType("dyn")] [GoLocalName("S2")] internal partial struct TestUnmarshalEmbeddedUnexported_S2 {
    internal partial ref ж<TestUnmarshalEmbeddedUnexported_embed1> embed1 { get; }
    public nint Q;
}

[GoType("dyn")] [GoLocalName("S3")] internal partial struct TestUnmarshalEmbeddedUnexported_S3 {
    internal partial ref TestUnmarshalEmbeddedUnexported_embed1 embed1 { get; }
    public nint R;
}

[GoType("dyn")] [GoLocalName("S4")] internal partial struct TestUnmarshalEmbeddedUnexported_S4 {
    internal partial ref ж<TestUnmarshalEmbeddedUnexported_embed1> embed1 { get; }
    internal partial ref TestUnmarshalEmbeddedUnexported_embed2 embed2 { get; }
}

[GoType("dyn")] [GoLocalName("S5")] internal partial struct TestUnmarshalEmbeddedUnexported_S5 {
    internal partial ref ж<TestUnmarshalEmbeddedUnexported_embed3> embed3 { get; }
    public nint R;
}

[GoType("dyn")] [GoLocalName("S6")] internal partial struct TestUnmarshalEmbeddedUnexported_S6 {
    [GoTag(@"json:""embed1""")]
    internal partial ref TestUnmarshalEmbeddedUnexported_embed1 embed1 { get; }
}

[GoType("dyn")] [GoLocalName("S7")] internal partial struct TestUnmarshalEmbeddedUnexported_S7 {
    [GoTag(@"json:""embed1""")]
    internal partial ref TestUnmarshalEmbeddedUnexported_embed1 embed1 { get; }
    internal partial ref TestUnmarshalEmbeddedUnexported_embed2 embed2 { get; }
}

[GoType("dyn")] [GoLocalName("S8")] internal partial struct TestUnmarshalEmbeddedUnexported_S8 {
    [GoTag(@"json:""embed1""")]
    internal partial ref TestUnmarshalEmbeddedUnexported_embed1 embed1 { get; }
    [GoTag(@"json:""embed2""")]
    internal partial ref TestUnmarshalEmbeddedUnexported_embed2 embed2 { get; }
    public nint Q;
}

[GoType("dyn")] [GoLocalName("S9")] internal partial struct TestUnmarshalEmbeddedUnexported_S9 {
    [GoTag(@"json:""embed""")]
    internal partial ref unexportedWithMethods unexportedWithMethods { get; }
}

[GoType("dyn")] internal partial struct TestUnmarshalEmbeddedUnexported_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal any ptr;
    internal any @out;
    internal error err;
}

// Test unmarshal behavior with regards to embedded unexported structs.
//
// (Issue 21357) If the embedded struct is a pointer and is unallocated,
// this returns an error because unmarshal cannot set the field.
//
// (Issue 24152) If the embedded struct is given an explicit name,
// ensure that the normal unmarshal logic does not panic in reflect.
//
// (Issue 28145) If the embedded struct is given an explicit name and has
// exported methods, don't cause a panic trying to get its value.
public static void TestUnmarshalEmbeddedUnexported(ж<testing.T> Ꮡt) {
    var tests = new TestUnmarshalEmbeddedUnexported_tests[]{new(
        CaseName: Name(""u8), // Error since we cannot set S1.embed1, but still able to set S1.R.

        @in: @"{""R"":2,""Q"":1}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S1>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S1(R: 2)),
        err: fmt.Errorf("json: cannot set embedded pointer to unexported struct: json.embed1"u8)
    ), new(
        CaseName: Name(""u8), // The top level Q field takes precedence.

        @in: @"{""Q"":1}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S2>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S2(Q: 1))
    ), new(
        CaseName: Name(""u8), // No issue with non-pointer variant.

        @in: @"{""R"":2,""Q"":1}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S3>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S3(embed1: new TestUnmarshalEmbeddedUnexported_embed1(Q: 1), R: 2))
    ), new(
        CaseName: Name(""u8), // No error since both embedded structs have field R, which annihilate each other.
 // Thus, no attempt is made at setting S4.embed1.

        @in: @"{""R"":2}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S4>(),
        @out: @new<TestUnmarshalEmbeddedUnexported_S4>()
    ), new(
        CaseName: Name(""u8), // Error since we cannot set S5.embed1, but still able to set S5.R.

        @in: @"{""R"":2,""Q"":1}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S5>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S5(R: 2)),
        err: fmt.Errorf("json: cannot set embedded pointer to unexported struct: json.embed3"u8)
    ), new(
        CaseName: Name(""u8), // Issue 24152, ensure decodeState.indirect does not panic.

        @in: @"{""embed1"": {""Q"": 1}}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S6>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S6(new TestUnmarshalEmbeddedUnexported_embed1(1)))
    ), new(
        CaseName: Name(""u8), // Issue 24153, check that we can still set forwarded fields even in
 // the presence of a name conflict.
 //
 // This relies on obscure behavior of reflect where it is possible
 // to set a forwarded exported field on an unexported embedded struct
 // even though there is a name conflict, even when it would have been
 // impossible to do so according to Go visibility rules.
 // Go forbids this because it is ambiguous whether S7.Q refers to
 // S7.embed1.Q or S7.embed2.Q. Since embed1 and embed2 are unexported,
 // it should be impossible for an external package to set either Q.
 //
 // It is probably okay for a future reflect change to break this.

        @in: @"{""embed1"": {""Q"": 1}, ""Q"": 2}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S7>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S7(new TestUnmarshalEmbeddedUnexported_embed1(1), new TestUnmarshalEmbeddedUnexported_embed2(2)))
    ), new(
        CaseName: Name(""u8), // Issue 24153, similar to the S7 case.

        @in: @"{""embed1"": {""Q"": 1}, ""embed2"": {""Q"": 2}, ""Q"": 3}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S8>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S8(new TestUnmarshalEmbeddedUnexported_embed1(1), new TestUnmarshalEmbeddedUnexported_embed2(2), 3))
    ), new(
        CaseName: Name(""u8), // Issue 228145, similar to the cases above.

        @in: @"{""embed"": {}}"u8,
        ptr: @new<TestUnmarshalEmbeddedUnexported_S9>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S9(nil))
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestUnmarshalEmbeddedUnexported_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var err = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.ptr);
            if (!equalError(err, ttʗ1.err)) {
                tΔ1.Errorf("%s: Unmarshal error:\n\tgot:  %v\n\twant: %v"u8, ttʗ1.Where, err, ttʗ1.err);
            }
            if (!reflect.DeepEqual(ttʗ1.ptr, ttʗ1.@out)) {
                tΔ1.Errorf("%s: Unmarshal:\n\tgot:  %#+v\n\twant: %#+v"u8, ttʗ1.Where, ttʗ1.ptr, ttʗ1.@out);
            }
        });
    }
}

[GoType("dyn")] internal partial struct TestUnmarshalErrorAfterMultipleJSON_tests {
    public partial ref CaseName CaseName { get; }
    internal @string @in;
    internal error err;
}

public static void TestUnmarshalErrorAfterMultipleJSON(ж<testing.T> Ꮡt) {
    var tests = new TestUnmarshalErrorAfterMultipleJSON_tests[]{new(
        CaseName: Name(""u8),
        @in: @"1 false null :"u8,
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ':' looking for beginning of value"u8, 14)))
    ), new(
        CaseName: Name(""u8),
        @in: @"1 [] [,]"u8,
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ',' looking for beginning of value"u8, 7)))
    ), new(
        CaseName: Name(""u8),
        @in: @"1 [] [true:]"u8,
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ':' after array element"u8, 11)))
    ), new(
        CaseName: Name(""u8),
        @in: @"1  {}    {""x""=}"u8,
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '=' after object key"u8, 14)))
    ), new(
        CaseName: Name(""u8),
        @in: @"falsetruenul#"u8,
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '#' in literal null (expecting 'l')"u8, 13)))
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestUnmarshalErrorAfterMultipleJSON_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var dec = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(ttʗ1.@in)));
            error err = default!;
            while (err == default!) {
                ref var v = ref heap<any>(out var Ꮡv);
                err = dec.Decode(Ꮡv);
            }
            if (!reflect.DeepEqual(err, ttʗ1.err)) {
                tΔ1.Errorf("%s: Decode error:\n\tgot:  %v\n\twant: %v"u8, ttʗ1.Where, err, ttʗ1.err);
            }
        });
    }
}

[GoType] internal partial struct unmarshalPanic {
}

internal static error UnmarshalJSON(this unmarshalPanic _Δp0, slice<byte> _Δp1) {
    throw panic((nint)(0xdead));
}

public static void TestUnmarshalPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var got = recover(); if (!reflect.DeepEqual(got, (nint)(0xdead))) {
                    Ꮡt.Errorf("panic() = (%T)(%v), want 0xdead"u8, got, got);
                }
            }
        }, ref ᒐ);
        Unmarshal(slice<byte>("{}"u8), Ꮡ(new unmarshalPanic(nil)));
        Ꮡt.Fatalf("Unmarshal should have panicked"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// The decoder used to hang if decoding into an interface pointing to its own address.
// See golang.org/issues/31740.
public static void TestUnmarshalRecursivePointer(ж<testing.T> Ꮡt) {
    ref var v = ref heap<any>(out var Ꮡv);
    v = Ꮡv;
    var data = slice<byte>(@"{""a"": ""b""}"u8);
    {
        var err = Unmarshal(data, v); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
        }
    }
}

[GoType("@string")] internal partial struct textUnmarshalerString;

[GoRecv] internal static error UnmarshalText(this ref textUnmarshalerString m, slice<byte> text) {
    m = ((textUnmarshalerString)strings.ToLower(((@string)text)));
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

// Test unmarshal to a map, where the map key is a user defined type.
// See golang.org/issues/34437.
public static void TestUnmarshalMapWithTextUnmarshalerStringKey(ж<testing.T> Ꮡt) {
    ref var p = ref heap<map<textUnmarshalerString, @string>>(out var Ꮡp);
    {
        var err = Unmarshal(slice<byte>(@"{""FOO"": ""1""}"u8), Ꮡp); if (err != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
        }
    }
    {
        var (_, ok) = p[fooˢ, ꟷ]; if (!ok) {
            Ꮡt.Errorf(@"key ""foo"" missing in map: %v"u8, p);
        }
    }
}

// See golang.org/issues/38126.
[GoType("dyn")] [GoLocalName("T")] internal partial struct TestUnmarshalRescanLiteralMangledUnquote_T {
    [GoTag(@"json:""F1,string""")]
    public @string F1;
}

public static void TestUnmarshalRescanLiteralMangledUnquote(ж<testing.T> Ꮡt) {
    // See golang.org/issues/38105.
    ref var p = ref heap<map<textUnmarshalerString, @string>>(out var Ꮡp);
    {
        var errΔ1 = Unmarshal(slice<byte>(@"{""开源"":""12345开源""}"u8), Ꮡp); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, errΔ1);
        }
    }
    {
        var (_, ok) = p["开源"u8, ꟷ]; if (!ok) {
            Ꮡt.Errorf(@"key ""开源"" missing in map: %v"u8, p);
        }
    }
    var wantT = new TestUnmarshalRescanLiteralMangledUnquote_T("aaa\tbbb"u8);
    var (b, err) = Marshal(wantT);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    ref var gotT = ref heap(new TestUnmarshalRescanLiteralMangledUnquote_T(), out var ᏑgotT);
    {
        var errΔ2 = Unmarshal(b, ᏑgotT); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, errΔ2);
        }
    }
    if (gotT != wantT) {
        Ꮡt.Errorf("Marshal/Unmarshal roundtrip:\n\tgot:  %q\n\twant: %q"u8, gotT, wantT);
    }
    // See golang.org/issues/39555.
    var input = new map<textUnmarshalerString, @string>{["FOO"u8] = ""u8, [@""""u8] = ""u8};
    (var encoded, err) = Marshal(input);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    ref var got = ref heap<map<textUnmarshalerString, @string>>(out var Ꮡgot);
    {
        var errΔ3 = Unmarshal(encoded, Ꮡgot); if (errΔ3 != default!) {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, errΔ3);
        }
    }
    var want = new map<textUnmarshalerString, @string>{["foo"u8] = ""u8, [@""""u8] = ""u8};
    if (!maps.Equal<map<textUnmarshalerString, @string>, map<textUnmarshalerString, @string>, textUnmarshalerString, @string>(got, want)) {
        Ꮡt.Errorf("Marshal/Unmarshal roundtrip:\n\tgot:  %q\n\twant: %q"u8, gotT, wantT);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string arrayOverMaxNestingDepthˢ = "ArrayOverMaxNestingDepth"u8;
internal static readonly @string arrayOverStackDepthˢ = "ArrayOverStackDepth"u8;
internal static readonly @string objectOverStackDepthˢ = "ObjectOverStackDepth"u8;
internal static readonly @string unstructuredˢ = "unstructured"u8;
internal static readonly @string typedNamedFieldˢ = "typed named field"u8;
internal static readonly @string typedMissingFieldˢ = "typed missing field"u8;
internal static readonly @string customUnmarshalerˢ = "custom unmarshaler"u8;

[GoType("dyn")] internal partial struct TestUnmarshalMaxDepth_tests {
    public partial ref CaseName CaseName { get; }
    internal @string data;
    internal bool errMaxDepth;
}

[GoType("dyn")] internal partial struct TestUnmarshalMaxDepth_targets {
    public partial ref CaseName CaseName { get; }
    internal Func<any> newValue;
}

[GoType("dyn")] internal partial struct TestUnmarshalMaxDepth_v {
    [GoTag(@"json:""a""")]
    public any A;
}

[GoType("dyn")] internal partial struct TestUnmarshalMaxDepth_vᴛ1 {
    [GoTag(@"json:""b""")]
    public any B;
}

public static void TestUnmarshalMaxDepth(ж<testing.T> Ꮡt) {
    var tests = new TestUnmarshalMaxDepth_tests[]{new(
        CaseName: Name("ArrayUnderMaxNestingDepth"u8),
        data: @"{""a"":"u8 + strings.Repeat(@"["u8, 10000 - 1) + strings.Repeat(@"]"u8, 10000 - 1) + @"}"u8,
        errMaxDepth: false
    ), new(
        CaseName: Name(arrayOverMaxNestingDepthˢ),
        data: @"{""a"":"u8 + strings.Repeat(@"["u8, 10000) + strings.Repeat(@"]"u8, 10000) + @"}"u8,
        errMaxDepth: true
    ), new(
        CaseName: Name(arrayOverStackDepthˢ),
        data: @"{""a"":"u8 + strings.Repeat(@"["u8, 3000000) + strings.Repeat(@"]"u8, 3000000) + @"}"u8,
        errMaxDepth: true
    ), new(
        CaseName: Name("ObjectUnderMaxNestingDepth"u8),
        data: @"{""a"":"u8 + strings.Repeat(@"{""a"":"u8, 10000 - 1) + @"0"u8 + strings.Repeat(@"}"u8, 10000 - 1) + @"}"u8,
        errMaxDepth: false
    ), new(
        CaseName: Name("ObjectOverMaxNestingDepth"u8),
        data: @"{""a"":"u8 + strings.Repeat(@"{""a"":"u8, 10000) + @"0"u8 + strings.Repeat(@"}"u8, 10000) + @"}"u8,
        errMaxDepth: true
    ), new(
        CaseName: Name(objectOverStackDepthˢ),
        data: @"{""a"":"u8 + strings.Repeat(@"{""a"":"u8, 3000000) + @"0"u8 + strings.Repeat(@"}"u8, 3000000) + @"}"u8,
        errMaxDepth: true
    )
    }.slice();
    var targets = new TestUnmarshalMaxDepth_targets[]{new(
        CaseName: Name(unstructuredˢ),
        newValue: () => {
            ref var v = ref heap<any>(out var Ꮡv);
            return Ꮡv;
        }
    ), new(
        CaseName: Name(typedNamedFieldˢ),
        newValue: () => {
            ref var v = ref heap<TestUnmarshalMaxDepth_v>(out var Ꮡv);
            v = new TestUnmarshalMaxDepth_v();
            return Ꮡv;
        }
    ), new(
        CaseName: Name(typedMissingFieldˢ),
        newValue: () => {
            ref var v = ref heap<TestUnmarshalMaxDepth_vᴛ1>(out var Ꮡv);
            v = new TestUnmarshalMaxDepth_vᴛ1();
            return Ꮡv;
        }
    ), new(
        CaseName: Name(customUnmarshalerˢ),
        newValue: () => {
            ref var v = ref heap<unmarshaler>(out var Ꮡv);
            v = new unmarshaler(nil);
            return Ꮡv;
        }
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestUnmarshalMaxDepth_tests(), out var Ꮡtt);
        tt = vᴛ1;

        foreach (var (_, vᴛ2) in targets) {
            ref var target = ref heap(new TestUnmarshalMaxDepth_targets(), out var Ꮡtarget);
            target = vᴛ2;

            var targetʗ1 = target;
            var ttʗ1 = tt;
            Ꮡt.Run(target.Name + "-"u8 + tt.Name, (ж<testing.T> tΔ1) => {
                var err = Unmarshal(slice<byte>(ttʗ1.data), targetʗ1.newValue());
                if (!ttʗ1.errMaxDepth){
                    if (err != default!) {
                        tΔ1.Errorf("%s: %s: Unmarshal error: %v"u8, ttʗ1.Where, targetʗ1.Where, err);
                    }
                } else {
                    if (err == default! || !strings.Contains(err.Error(), exceededMaxDepthˢ)) {
                        tΔ1.Errorf("%s: %s: Unmarshal error:\n\tgot:  %v\n\twant: exceeded max depth"u8, ttʗ1.Where, targetʗ1.Where, err);
                    }
                }
            });
        }
    }
}

} // end json_internal_test_package
