// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using bytes = bytes_package;
using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using image = image_package;
using io = io_package;
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
using static go.encoding.json_package;

partial class json_internal_test_package {

public partial struct T {
    public @string X;
    public nint Y;
    public nint Z; /*`json:"-"`*/
}

public partial struct U {
    public @string Alphabet; /*`json:"alpha"`*/
}

public partial struct V {
    public any F1;
    public int32 F2;
    public global::go.encoding.json_package.Number F3;
    public ж<VOuter> F4;
}

public partial struct VOuter {
    public V V;
}

public partial struct W {
    public SS S;
}

public partial struct P {
    public PP PP;
}

public partial struct PP {
    public T T;
    public slice<T> Ts;
}

public partial struct SS /*@string*/;

public static error UnmarshalJSON(this ref SS _, slice<byte> data) {
    return new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number", Type: reflect.TypeFor<SS>())));
}

public partial struct TAlias /*T*/;

public static error UnmarshalJSON(this ref TAlias tt, slice<byte> data) {
    ref var t = ref heap<T>(new T(), out var Ꮡt);
    var err = Unmarshal(data, Ꮡt); if (err != nil) {
        return err;
    }
    tt = (TAlias)t;
    return default;
}

public partial struct TOuter {
    public TAlias T;
}

// ifaceNumAsFloat64/ifaceNumAsNumber are used to test unmarshaling with and
// without UseNumber
internal static map<@string, any> ifaceNumAsFloat64 = new map<@string, any>{
    ["k1"] = (float64)1D,
    ["k2"] = (@string)"s",
    ["k3"] = slice<any>([(float64)1D, (float64)2.0D, (float64)3e-3D]),
    ["k4"] = new map<@string, any>{["kk1"] = (@string)"s", ["kk2"] = (float64)2D}
};

internal static map<@string, any> ifaceNumAsNumber = new map<@string, any>{
    ["k1"] = ((global::go.encoding.json_package.Number)(@string)"1"),
    ["k2"] = (@string)"s",
    ["k3"] = slice<any>([(global::go.encoding.json_package.Number)(@string)"1", (global::go.encoding.json_package.Number)(@string)"2.0", (global::go.encoding.json_package.Number)(@string)"3e-3"]),
    ["k4"] = new map<@string, any>{["kk1"] = (@string)"s", ["kk2"] = ((global::go.encoding.json_package.Number)(@string)"2")}
};

internal partial struct tx {
    internal nint x;
}

internal partial struct u8 /*num:uint8*/;

// A type that can unmarshal itself.
public partial struct unmarshaler {
    public bool T;
}

public static error UnmarshalJSON(this ref unmarshaler u, slice<byte> b) {
    u = new unmarshaler(true); // All we need to see that UnmarshalJSON is called.
    return default;
}

internal partial struct ustruct {
    public unmarshaler M;
}

public partial struct unmarshalerText {
    public @string A, B;
}

// needed for re-marshaling tests
public static (slice<byte>, error) MarshalText(this unmarshalerText u) {
    return (slice<byte>(u.A + ":" + u.B), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string missingSeparatorˢ => ref missingSeparatorˢᵇ;
private static readonly @string missingSeparatorˢᵇ = "missing separator"u8;

public static error UnmarshalText(this ref unmarshalerText u, slice<byte> b) {
    nint pos = bytes.IndexByte(b, (byte)':');
    if (pos == -1) {
        return errors.New(missingSeparatorˢ);
    }
    (u.A, u.B) = ((@string)(b.slice(0, pos)), (@string)(b.slice(pos + 1)));
    return default;
}

internal static encoding.TextUnmarshaler _ᴛ1ʗ = new json_internal_test_package.unmarshalerTextжTextUnmarshaler(((ж<unmarshalerText>)nil));

internal partial struct ustructText {
    public unmarshalerText M;
}

// u8marshal is an integer type that can marshal/unmarshal itself.
internal partial struct u8marshal /*num:uint8*/;

internal static (slice<byte>, error) MarshalText(this u8marshal u8) {
    return (slice<byte>(fmt.Sprintf("u%d"u8, u8)), default);
}

internal static error errMissingU8Prefix = errors.New("missing 'u' prefix");

internal static error UnmarshalText(this ref u8marshal u8, slice<byte> b) {
    if (!bytes.HasPrefix(b, [(byte)'u'])) {
        return errMissingU8Prefix;
    }
    var (n, err) = strconv.Atoi((@string)(b[1..]));
    if (err != nil) {
        return err;
    }
    u8 = (u8marshal)(uint8)n;
    return default;
}

internal static encoding.TextUnmarshaler _ᴛ2ʗ = new json_internal_test_package.u8marshalжTextUnmarshaler(((ж<u8marshal>)nil));

internal static readonly ж<unmarshaler> Ꮡumtrue = new StaticBox<unmarshaler>(new unmarshaler(true));
internal static ref unmarshaler umtrue => ref ((StaticBox<unmarshaler>)Ꮡumtrue).ValueSlot;
internal static readonly ж<slice<unmarshaler>> Ꮡumslice = new StaticBox<slice<unmarshaler>>([new(true)]);
internal static ref slice<unmarshaler> umslice => ref ((StaticBox<slice<unmarshaler>>)Ꮡumslice).ValueSlot;
internal static ustruct umstruct = new ustruct(new unmarshaler(true));

internal static readonly ж<unmarshalerText> ᏑumtrueXY = new StaticBox<unmarshalerText>(new unmarshalerText("x", "y"));
internal static ref unmarshalerText umtrueXY => ref ((StaticBox<unmarshalerText>)ᏑumtrueXY).ValueSlot;
internal static readonly ж<slice<unmarshalerText>> ᏑumsliceXY = new StaticBox<slice<unmarshalerText>>([new("x", "y")]);
internal static ref slice<unmarshalerText> umsliceXY => ref ((StaticBox<slice<unmarshalerText>>)ᏑumsliceXY).ValueSlot;
internal static ustructText umstructXY = new ustructText(new unmarshalerText("x", "y"));

internal static map<unmarshalerText, bool> ummapXY = new map<unmarshalerText, bool>{[new("x", "y")] = true};

// Test data structures for anonymous fields.
public partial struct Point {
    public nint Z;
}

public partial struct Top {
    public nint Level0;
    /*embed*/ public Embed0 Embed0;
    /*embed*/ public ж<Embed0a> Embed0a;
    /*embed*/ public ж<Embed0b> Embed0b; /*`json:"e,omitempty"`*/ // treated as named
    /*embed*/ public Embed0c Embed0c; /*`json:"-"`*/              // ignored
    /*embed*/ public Loop Loop;
    /*embed*/ public Embed0p Embed0p; // has Point with X, Y, used
    /*embed*/ public Embed0q Embed0q; // has Point with Z, used
    /*embed*/ internal embed embed;   // contains exported field
}

public partial struct Embed0 {
    public nint Level1a; // overridden by Embed0a's Level1a with json tag
    public nint Level1b; // used because Embed0a's Level1b is renamed
    public nint Level1c; // used because Embed0a's Level1c is ignored
    public nint Level1d; // annihilated by Embed0a's Level1d
    public nint Level1e; /*`json:"x"`*/ // annihilated by Embed0a.Level1e
}

public partial struct Embed0a {
    public nint Level1a; /*`json:"Level1a,omitempty"`*/
    public nint Level1b; /*`json:"LEVEL1B,omitempty"`*/
    public nint Level1c; /*`json:"-"`*/
    public nint Level1d; // annihilated by Embed0's Level1d
    public nint Level1f; /*`json:"x"`*/ // annihilated by Embed0's Level1e
}

public partial struct Embed0b /*Embed0*/;

public partial struct Embed0c /*Embed0*/;

public partial struct Embed0p {
    /*embed*/ public image_package.Point Point;
}

public partial struct Embed0q {
    /*embed*/ public Point Point;
}

internal partial struct embed {
    public nint Q;
}

public partial struct Loop {
    public nint Loop1; /*`json:",omitempty"`*/
    public nint Loop2; /*`json:",omitempty"`*/
    /*embed*/ public ж<Loop> ΔLoop;
}

// From reflect test:
// The X in S6 and S7 annihilate, but they also block the X in S8.S9.
public partial struct S5 {
    /*embed*/ public S6 S6;
    /*embed*/ public S7 S7;
    /*embed*/ public S8 S8;
}

public partial struct S6 {
    public nint X;
}

public partial struct S7 /*S6*/;

public partial struct S8 {
    /*embed*/ public S9 S9;
}

public partial struct S9 {
    public nint X;
    public nint Y;
}

// From reflect test:
// The X in S11.S6 and S12.S6 annihilate, but they also block the X in S13.S8.S9.
public partial struct S10 {
    /*embed*/ public S11 S11;
    /*embed*/ public S12 S12;
    /*embed*/ public S13 S13;
}

public partial struct S11 {
    /*embed*/ public S6 S6;
}

public partial struct S12 {
    /*embed*/ public S6 S6;
}

public partial struct S13 {
    /*embed*/ public S8 S8;
}

public partial struct Ambig {
    // Given "hello", the first match should win.
    public nint First; /*`json:"HELLO"`*/
    public nint Second; /*`json:"Hello"`*/
}

public partial struct XYZ {
    public any X;
    public any Y;
    public any Z;
}

internal partial struct unexportedWithMethods {
}

internal static void F(this unexportedWithMethods _) { }

internal partial struct byteWithMarshalJSON /*num:byte*/;

internal static (slice<byte>, error) MarshalJSON(this byteWithMarshalJSON b) {
    return (slice<byte>(fmt.Sprintf(@"""Z%.2x"""u8, (byte)b)), default);
}

internal static error UnmarshalJSON(this ref byteWithMarshalJSON b, slice<byte> data) {
    if (len(data) != 5 || data[0] != '"' || data[1] != 'Z' || data[4] != '"') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt((@string)(data[2..4]), 16, 8);
    if (err != nil) {
        return fmt.Errorf("bad hex"u8);
    }
    b = (byteWithMarshalJSON)(byte)i;
    return default;
}

internal partial struct byteWithPtrMarshalJSON /*num:byte*/;

internal static (slice<byte>, error) MarshalJSON(this ref byteWithPtrMarshalJSON b) {
    return ((byteWithMarshalJSON)(byte)(b)).MarshalJSON();
}

internal static error UnmarshalJSON(this ж<byteWithPtrMarshalJSON> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<byteWithPtrMarshalJSON, byteWithMarshalJSON>()).UnmarshalJSON(data);
}

internal partial struct byteWithMarshalText /*num:byte*/;

internal static (slice<byte>, error) MarshalText(this byteWithMarshalText b) {
    return (slice<byte>(fmt.Sprintf(@"Z%.2x"u8, (byte)b)), default);
}

internal static error UnmarshalText(this ref byteWithMarshalText b, slice<byte> data) {
    if (len(data) != 3 || data[0] != 'Z') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt((@string)(data[1..3]), 16, 8);
    if (err != nil) {
        return fmt.Errorf("bad hex"u8);
    }
    b = (byteWithMarshalText)(byte)i;
    return default;
}

internal partial struct byteWithPtrMarshalText /*num:byte*/;

internal static (slice<byte>, error) MarshalText(this ref byteWithPtrMarshalText b) {
    return ((byteWithMarshalText)(byte)(b)).MarshalText();
}

internal static error UnmarshalText(this ж<byteWithPtrMarshalText> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<byteWithPtrMarshalText, byteWithMarshalText>()).UnmarshalText(data);
}

internal partial struct intWithMarshalJSON /*num:nint*/;

internal static (slice<byte>, error) MarshalJSON(this intWithMarshalJSON b) {
    return (slice<byte>(fmt.Sprintf(@"""Z%.2x"""u8, (nint)b)), default);
}

internal static error UnmarshalJSON(this ref intWithMarshalJSON b, slice<byte> data) {
    if (len(data) != 5 || data[0] != '"' || data[1] != 'Z' || data[4] != '"') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt((@string)(data[2..4]), 16, 8);
    if (err != nil) {
        return fmt.Errorf("bad hex"u8);
    }
    b = (intWithMarshalJSON)(nint)i;
    return default;
}

internal partial struct intWithPtrMarshalJSON /*num:nint*/;

internal static (slice<byte>, error) MarshalJSON(this ref intWithPtrMarshalJSON b) {
    return ((intWithMarshalJSON)(nint)(b)).MarshalJSON();
}

internal static error UnmarshalJSON(this ж<intWithPtrMarshalJSON> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<intWithPtrMarshalJSON, intWithMarshalJSON>()).UnmarshalJSON(data);
}

internal partial struct intWithMarshalText /*num:nint*/;

internal static (slice<byte>, error) MarshalText(this intWithMarshalText b) {
    return (slice<byte>(fmt.Sprintf(@"Z%.2x"u8, (nint)b)), default);
}

internal static error UnmarshalText(this ref intWithMarshalText b, slice<byte> data) {
    if (len(data) != 3 || data[0] != 'Z') {
        return fmt.Errorf("bad quoted string"u8);
    }
    var (i, err) = strconv.ParseInt((@string)(data[1..3]), 16, 8);
    if (err != nil) {
        return fmt.Errorf("bad hex"u8);
    }
    b = (intWithMarshalText)(nint)i;
    return default;
}

internal partial struct intWithPtrMarshalText /*num:nint*/;

internal static (slice<byte>, error) MarshalText(this ref intWithPtrMarshalText b) {
    return ((intWithMarshalText)(nint)(b)).MarshalText();
}

internal static error UnmarshalText(this ж<intWithPtrMarshalText> Ꮡb, slice<byte> data) {
    return (Ꮡb.Reinterpret<intWithPtrMarshalText, intWithMarshalText>()).UnmarshalText(data);
}

internal partial struct mapStringToStringData {
    public map<@string, @string> Data; /*`json:"data"`*/
}

public partial struct B {
    public bool ΔB; /*`json:",string"`*/
}

public partial struct DoublePtr {
    public ж<ж<nint>> I;
    public ж<ж<nint>> J;
}

partial struct NestedUnamed_F /*dyn*/ {
    public nint V;
}

public partial struct NestedUnamed {
    public NestedUnamed_F F;
}

partial struct unmarshalTestsᴛ1 /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal any ptr; // new(type)
    internal any @out;
    internal error err;
    internal bool useNumber;
    internal bool golden;
    internal bool disallowUnknownFields;
}
            partial struct Δtype /*dyn*/ {
                public global::go.encoding.json_package.Number A;
            }
            partial struct Δtypeᴛ1 /*dyn*/ {
                public global::go.encoding.json_package.Number A; /*`json:",string"`*/
            }
            partial struct Δtypeᴛ2 /*dyn*/ {
                public global::go.encoding.json_package.Number N;
            }
            partial struct Δtypeᴛ3 /*dyn*/ {
                public global::go.encoding.json_package.Number N; /*`json:",string"`*/
            }
            partial struct Δtypeᴛ4 /*dyn*/ {
                public @string F; /*`json:"-,"`*/
            }
            partial struct Δtypeᴛ5 /*dyn*/ {
                public @string F; /*`json:"-,omitempty"`*/
            }
            partial struct Δtypeᴛ6 /*dyn*/ {
                public int64 X; /*`json:",string"`*/
            }
            partial struct Δtypeᴛ7 /*dyn*/ {
                public uint64 X; /*`json:",string"`*/
            }
            partial struct Δtypeᴛ8 /*dyn*/ {
                public float64 X; /*`json:",string"`*/
            }
internal static slice<unmarshalTestsᴛ1> unmarshalTests;
internal static void initᴛunmarshalTests() { unmarshalTests = slice<unmarshalTestsᴛ1>([
    // basic types
    new(CaseName: Name(""), @in: @"true", ptr: @new<bool>(), @out: true),
    new(CaseName: Name(""), @in: @"1", ptr: @new<nint>(), @out: (nint)1),
    new(CaseName: Name(""), @in: @"1.2", ptr: @new<float64>(), @out: 1.2D),
    new(CaseName: Name(""), @in: @"-5", ptr: @new<int16>(), @out: (int16)(-5)),
    new(CaseName: Name(""), @in: @"2", ptr: @new<global::go.encoding.json_package.Number>(), @out: ((global::go.encoding.json_package.Number)(@string)"2"), useNumber: true),
    new(CaseName: Name(""), @in: @"2", ptr: @new<global::go.encoding.json_package.Number>(), @out: ((global::go.encoding.json_package.Number)(@string)"2")),
    new(CaseName: Name(""), @in: @"2", ptr: @new<any>(), @out: (float64)2.0D),
    new(CaseName: Name(""), @in: @"2", ptr: @new<any>(), @out: ((global::go.encoding.json_package.Number)(@string)"2"), useNumber: true),
    new(CaseName: Name(""), @in: @"""a\u1234""", ptr: @new<@string>(), @out: (@string)"a\u1234"),
    new(CaseName: Name(""), @in: @"""http:\/\/""", ptr: @new<@string>(), @out: (@string)"http://"),
    new(CaseName: Name(""), @in: @"""g-clef: \uD834\uDD1E""", ptr: @new<@string>(), @out: (@string)"g-clef: \U0001D11E"),
    new(CaseName: Name(""), @in: @"""invalid: \uD834x\uDD1E""", ptr: @new<@string>(), @out: (@string)"invalid: \uFFFDx\uFFFD"),
    new(CaseName: Name(""), @in: "null", ptr: @new<any>(), @out: default),
    new(CaseName: Name(""), @in: @"{""X"": [1,2,3], ""Y"": 4}", ptr: @new<T>(), @out: new T(Y: 4), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("array", reflect.TypeFor<@string>(), 7, "T", "X")))),
    new(CaseName: Name(""), @in: @"{""X"": 23}", ptr: @new<T>(), @out: new T(), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("number", reflect.TypeFor<@string>(), 8, "T", "X")))),
    new(CaseName: Name(""), @in: @"{""x"": 1}", ptr: @new<tx>(), @out: new tx()),
    new(CaseName: Name(""), @in: @"{""x"": 1}", ptr: @new<tx>(), @out: new tx()),
    new(CaseName: Name(""), @in: @"{""x"": 1}", ptr: @new<tx>(), err: fmt.Errorf("json: unknown field \"x\""u8), disallowUnknownFields: true),
    new(CaseName: Name(""), @in: @"{""S"": 23}", ptr: @new<W>(), @out: new W(), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("number", reflect.TypeFor<SS>(), 0, "W", "S")))),
    new(CaseName: Name(""), @in: @"{""T"": {""X"": 23}}", ptr: @new<TOuter>(), @out: new TOuter(), err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError("number", reflect.TypeFor<@string>(), 8, "TOuter", "T.X")))),
    new(CaseName: Name(""), @in: @"{""F1"":1,""F2"":2,""F3"":3}", ptr: @new<V>(), @out: new V(F1: (float64)1D, F2: (int32)2, F3: ((global::go.encoding.json_package.Number)(@string)"3"))),
    new(CaseName: Name(""), @in: @"{""F1"":1,""F2"":2,""F3"":3}", ptr: @new<V>(), @out: new V(F1: ((global::go.encoding.json_package.Number)(@string)"1"), F2: (int32)2, F3: ((global::go.encoding.json_package.Number)(@string)"3")), useNumber: true),
    new(CaseName: Name(""), @in: @"{""k1"":1,""k2"":""s"",""k3"":[1,2.0,3e-3],""k4"":{""kk1"":""s"",""kk2"":2}}", ptr: @new<any>(), @out: ifaceNumAsFloat64),
    new(CaseName: Name(""), @in: @"{""k1"":1,""k2"":""s"",""k3"":[1,2.0,3e-3],""k4"":{""kk1"":""s"",""kk2"":2}}", ptr: @new<any>(), @out: ifaceNumAsNumber, useNumber: true),

    // raw values with whitespace
    new(CaseName: Name(""), @in: "\n true ", ptr: @new<bool>(), @out: true),
    new(CaseName: Name(""), @in: "\t 1 ", ptr: @new<nint>(), @out: (nint)1),
    new(CaseName: Name(""), @in: "\r 1.2 ", ptr: @new<float64>(), @out: 1.2D),
    new(CaseName: Name(""), @in: "\t -5 \n", ptr: @new<int16>(), @out: (int16)(-5)),
    new(CaseName: Name(""), @in: "\t \"a\\u1234\" \n", ptr: @new<@string>(), @out: (@string)"a\u1234"),

    // Z has a "-" tag.
    new(CaseName: Name(""), @in: @"{""Y"": 1, ""Z"": 2}", ptr: @new<T>(), @out: new T(Y: 1)),
    new(CaseName: Name(""), @in: @"{""Y"": 1, ""Z"": 2}", ptr: @new<T>(), @out: new T(Y: 1), err: fmt.Errorf("json: unknown field \"Z\""u8), disallowUnknownFields: true),

    new(CaseName: Name(""), @in: @"{""alpha"": ""abc"", ""alphabet"": ""xyz""}", ptr: @new<U>(), @out: new U(Alphabet: "abc")),
    new(CaseName: Name(""), @in: @"{""alpha"": ""abc"", ""alphabet"": ""xyz""}", ptr: @new<U>(), @out: new U(Alphabet: "abc"), err: fmt.Errorf("json: unknown field \"alphabet\""u8), disallowUnknownFields: true),
    new(CaseName: Name(""), @in: @"{""alpha"": ""abc""}", ptr: @new<U>(), @out: new U(Alphabet: "abc")),
    new(CaseName: Name(""), @in: @"{""alphabet"": ""xyz""}", ptr: @new<U>(), @out: new U()),
    new(CaseName: Name(""), @in: @"{""alphabet"": ""xyz""}", ptr: @new<U>(), err: fmt.Errorf("json: unknown field \"alphabet\""u8), disallowUnknownFields: true),

    // syntax errors
    new(CaseName: Name(""), @in: @"", ptr: @new<any>(), err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("unexpected end of JSON input", 0)))),
    new(CaseName: Name(""), @in: " \n\r\t", ptr: @new<any>(), err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("unexpected end of JSON input", 4)))),
    new(CaseName: Name(""), @in: @"[2, 3", ptr: @new<any>(), err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("unexpected end of JSON input", 5)))),
    new(CaseName: Name(""), @in: @"{""X"": ""foo"", ""Y""}", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '}' after object key", 17)))),
    new(CaseName: Name(""), @in: @"[1, 2, 3+]", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '+' after array element", 9)))),
    new(CaseName: Name(""), @in: @"{""X"":12x}", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character 'x' after object key:value pair", 8))), useNumber: true),
    new(CaseName: Name(""), @in: @"{""F3"": -}", ptr: @new<V>(), err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '}' in numeric literal", 9)))),

    // raw value errors
    new(CaseName: Name(""), @in: "\x01 42", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value", 1)))),
    new(CaseName: Name(""), @in: " 42 \x01", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value", 5)))),
    new(CaseName: Name(""), @in: "\x01 true", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value", 1)))),
    new(CaseName: Name(""), @in: " false \x01", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value", 8)))),
    new(CaseName: Name(""), @in: "\x01 1.2", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value", 1)))),
    new(CaseName: Name(""), @in: " 3.4 \x01", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value", 6)))),
    new(CaseName: Name(""), @in: "\x01 \"string\"", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' looking for beginning of value", 1)))),
    new(CaseName: Name(""), @in: " \"string\" \x01", err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '\\x01' after top-level value", 11)))),

    // array tests
    new(CaseName: Name(""), @in: @"[1, 2, 3]", ptr: Ꮡ(new array<nint>(3)), @out: new nint[]{1, 2, 3}.array()),
    new(CaseName: Name(""), @in: @"[1, 2, 3]", ptr: Ꮡ(new array<nint>(1)), @out: new nint[]{1}.array()),
    new(CaseName: Name(""), @in: @"[1, 2, 3]", ptr: Ꮡ(new array<nint>(5)), @out: new nint[]{1, 2, 3, 0, 0}.array()),
    new(CaseName: Name(""), @in: @"[1, 2, 3]", ptr: @new<MustNotUnmarshalJSON>(), err: errors.New("MustNotUnmarshalJSON was used")),

    // empty array to interface test
    new(CaseName: Name(""), @in: @"[]", ptr: @new<slice<any>>(), @out: slice<any>([])),
    new(CaseName: Name(""), @in: @"null", ptr: @new<slice<any>>(), @out: slice<any>(default)),
    new(CaseName: Name(""), @in: @"{""T"":[]}", ptr: @new<map<@string, any>>(), @out: new map<@string, any>{["T"] = slice<any>([])}),
    new(CaseName: Name(""), @in: @"{""T"":null}", ptr: @new<map<@string, any>>(), @out: new map<@string, any>{["T"] = ((any)default)}),

    // composite tests
    new(CaseName: Name(""), @in: allValueIndent, ptr: @new<All>(), @out: allValue),
    new(CaseName: Name(""), @in: allValueCompact, ptr: @new<All>(), @out: allValue),
    new(CaseName: Name(""), @in: allValueIndent, ptr: @new<ж<All>>(), @out: ᏑallValue),
    new(CaseName: Name(""), @in: allValueCompact, ptr: @new<ж<All>>(), @out: ᏑallValue),
    new(CaseName: Name(""), @in: pallValueIndent, ptr: @new<All>(), @out: pallValue),
    new(CaseName: Name(""), @in: pallValueCompact, ptr: @new<All>(), @out: pallValue),
    new(CaseName: Name(""), @in: pallValueIndent, ptr: @new<ж<All>>(), @out: ᏑpallValue),
    new(CaseName: Name(""), @in: pallValueCompact, ptr: @new<ж<All>>(), @out: ᏑpallValue),

    // unmarshal interface test
    new(CaseName: Name(""), @in: @"{""T"":false}", ptr: @new<unmarshaler>(), @out: umtrue), // use "false" so test will fail if custom unmarshaler is not called
    new(CaseName: Name(""), @in: @"{""T"":false}", ptr: @new<ж<unmarshaler>>(), @out: Ꮡumtrue),
    new(CaseName: Name(""), @in: @"[{""T"":false}]", ptr: @new<slice<unmarshaler>>(), @out: umslice),
    new(CaseName: Name(""), @in: @"[{""T"":false}]", ptr: @new<ж<slice<unmarshaler>>>(), @out: Ꮡumslice),
    new(CaseName: Name(""), @in: @"{""M"":{""T"":""x:y""}}", ptr: @new<ustruct>(), @out: umstruct),

    // UnmarshalText interface test
    new(CaseName: Name(""), @in: @"""x:y""", ptr: @new<unmarshalerText>(), @out: umtrueXY),
    new(CaseName: Name(""), @in: @"""x:y""", ptr: @new<ж<unmarshalerText>>(), @out: ᏑumtrueXY),
    new(CaseName: Name(""), @in: @"[""x:y""]", ptr: @new<slice<unmarshalerText>>(), @out: umsliceXY),
    new(CaseName: Name(""), @in: @"[""x:y""]", ptr: @new<ж<slice<unmarshalerText>>>(), @out: ᏑumsliceXY),
    new(CaseName: Name(""), @in: @"{""M"":""x:y""}", ptr: @new<ustructText>(), @out: umstructXY),

    // integer-keyed map test
    new(
        CaseName: Name(""),
        @in: @"{""-1"":""a"",""0"":""b"",""1"":""c""}",
        ptr: @new<map<nint, @string>>(),
        @out: new map<nint, @string>{[-1] = "a", [0] = "b", [1] = "c"}
    ),
    new(
        CaseName: Name(""),
        @in: @"{""0"":""a"",""10"":""c"",""9"":""b""}",
        ptr: @new<map<u8, @string>>(),
        @out: new map<u8, @string>{[0] = "a", [9] = "b", [10] = "c"}
    ),
    new(
        CaseName: Name(""),
        @in: @"{""-9223372036854775808"":""min"",""9223372036854775807"":""max""}",
        ptr: @new<map<int64, @string>>(),
        @out: new map<int64, @string>{[math.MinInt64] = "min", [math.MaxInt64] = "max"}
    ),
    new(
        CaseName: Name(""),
        @in: @"{""18446744073709551615"":""max""}",
        ptr: @new<map<uint64, @string>>(),
        @out: new map<uint64, @string>{[math.MaxUint64] = "max"}
    ),
    new(
        CaseName: Name(""),
        @in: @"{""0"":false,""10"":true}",
        ptr: @new<map<uintptr, bool>>(),
        @out: new map<uintptr, bool>{[0] = false, [10] = true}
    ),

    // Check that MarshalText and UnmarshalText take precedence
    // over default integer handling in map keys.
    new(
        CaseName: Name(""),
        @in: @"{""u2"":4}",
        ptr: @new<map<u8marshal, nint>>(),
        @out: new map<u8marshal, nint>{[2] = 4}
    ),
    new(
        CaseName: Name(""),
        @in: @"{""2"":4}",
        ptr: @new<map<u8marshal, nint>>(),
        @out: new map<u8marshal, nint>{},
        err: errMissingU8Prefix
    ),

    // integer-keyed map errors
    new(
        CaseName: Name(""),
        @in: @"{""abc"":""abc""}",
        ptr: @new<map<nint, @string>>(),
        @out: new map<nint, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number abc", Type: reflect.TypeFor<nint>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""256"":""abc""}",
        ptr: @new<map<uint8, @string>>(),
        @out: new map<uint8, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 256", Type: reflect.TypeFor<uint8>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""128"":""abc""}",
        ptr: @new<map<int8, @string>>(),
        @out: new map<int8, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 128", Type: reflect.TypeFor<int8>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""-1"":""abc""}",
        ptr: @new<map<uint8, @string>>(),
        @out: new map<uint8, @string>{},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number -1", Type: reflect.TypeFor<uint8>(), Offset: 2)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""F"":{""a"":2,""3"":4}}",
        ptr: @new<map<@string, map<nint, nint>>>(),
        @out: new map<@string, map<nint, nint>>{["F"] = new map<nint, nint>{[3] = 4}},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number a", Type: reflect.TypeFor<nint>(), Offset: 7)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""F"":{""a"":2,""3"":4}}",
        ptr: @new<map<@string, map<nuint, nint>>>(),
        @out: new map<@string, map<nuint, nint>>{["F"] = new map<nuint, nint>{[3] = 4}},
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number a", Type: reflect.TypeFor<nuint>(), Offset: 7)))
    ),

    // Map keys can be encoding.TextUnmarshalers.
    new(CaseName: Name(""), @in: @"{""x:y"":true}", ptr: @new<map<unmarshalerText, bool>>(), @out: ummapXY),
    // If multiple values for the same key exists, only the most recent value is used.
    new(CaseName: Name(""), @in: @"{""x:y"":false,""x:y"":true}", ptr: @new<map<unmarshalerText, bool>>(), @out: ummapXY),

    new(
        CaseName: Name(""),
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
"""u8.Raw(),
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
        CaseName: Name(""),
        @in: @"{""hello"": 1}",
        ptr: @new<Ambig>(),
        @out: new Ambig(First: 1)
    ),

    new(
        CaseName: Name(""),
        @in: @"{""X"": 1,""Y"":2}",
        ptr: @new<S5>(),
        @out: new S5(S8: new S8(S9: new S9(Y: 2)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""X"": 1,""Y"":2}",
        ptr: @new<S5>(),
        @out: new S5(S8: new S8(new S9(Y: 2))),
        err: fmt.Errorf("json: unknown field \"X\""u8),
        disallowUnknownFields: true
    ),
    new(
        CaseName: Name(""),
        @in: @"{""X"": 1,""Y"":2}",
        ptr: @new<S10>(),
        @out: new S10(S13: new S13(S8: new S8(S9: new S9(Y: 2))))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""X"": 1,""Y"":2}",
        ptr: @new<S10>(),
        @out: new S10(S13: new S13(new S8(new S9(Y: 2)))),
        err: fmt.Errorf("json: unknown field \"X\""u8),
        disallowUnknownFields: true
    ),
    new(
        CaseName: Name(""),
        @in: @"{""I"": 0, ""I"": null, ""J"": null}",
        ptr: @new<DoublePtr>(),
        @out: new DoublePtr(I: nil, J: nil)
    ),

    // invalid UTF-8 is coerced to valid UTF-8.
    new(
        CaseName: Name(""),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffdworld"
    ),
    new(
        CaseName: Name(""),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xc2, 0xc2, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"
    ),
    new(
        CaseName: Name(""),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xc2, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"
    ),
    new(
        CaseName: Name(""),
        @in: "\"hello\\ud800world\"",
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffdworld"
    ),
    new(
        CaseName: Name(""),
        @in: "\"hello\\ud800\\ud800world\"",
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"
    ),
    new(
        CaseName: Name(""),
        @in: "\"hello\\ud800\\ud800world\"",
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffdworld"
    ),
    new(
        CaseName: Name(""),
        @in: ((@string)(new byte[]{0x22, 0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xed, 0xa0, 0x80, 0xed, 0xb0, 0x80, 0x77, 0x6f, 0x72, 0x6c, 0x64, 0x22})),
        ptr: @new<@string>(),
        @out: (@string)"hello\ufffd\ufffd\ufffd\ufffd\ufffd\ufffdworld"
    ),

    // Used to be issue 8305, but time.Time implements encoding.TextUnmarshaler so this works now.
    new(
        CaseName: Name(""),
        @in: @"{""2009-11-10T23:00:00Z"": ""hello world""}",
        ptr: @new<map<time.Time, @string>>(),
        @out: new map<time.Time, @string>{[time.Date(2009, 11, 10, 23, 0, 0, 0, time.ΔUTC)] = "hello world"}
    ),

    // issue 8305
    new(
        CaseName: Name(""),
        @in: @"{""2009-11-10T23:00:00Z"": ""hello world""}",
        ptr: @new<map<Point, @string>>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "object", Type: reflect.TypeFor<map<Point, @string>>(), Offset: 1)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""asdf"": ""hello world""}",
        ptr: @new<map<unmarshaler, @string>>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "object", Type: reflect.TypeFor<map<unmarshaler, @string>>(), Offset: 1)))
    ),

    // related to issue 13783.
    // Go 1.7 changed marshaling a slice of typed byte to use the methods on the byte type,
    // similar to marshaling a slice of typed int.
    // These tests check that, assuming the byte type also has valid decoding methods,
    // either the old base64 string encoding or the new per-element encoding can be
    // successfully unmarshaled. The custom unmarshalers were accessible in earlier
    // versions of Go, even though the custom marshaler was not.
    new(
        CaseName: Name(""),
        @in: @"""AQID""",
        ptr: @new<slice<byteWithMarshalJSON>>(),
        @out: slice<byteWithMarshalJSON>([1, 2, 3])
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<byteWithMarshalJSON>>(),
        @out: slice<byteWithMarshalJSON>([1, 2, 3]),
        golden: true
    ),
    new(
        CaseName: Name(""),
        @in: @"""AQID""",
        ptr: @new<slice<byteWithMarshalText>>(),
        @out: slice<byteWithMarshalText>([1, 2, 3])
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<byteWithMarshalText>>(),
        @out: slice<byteWithMarshalText>([1, 2, 3]),
        golden: true
    ),
    new(
        CaseName: Name(""),
        @in: @"""AQID""",
        ptr: @new<slice<byteWithPtrMarshalJSON>>(),
        @out: slice<byteWithPtrMarshalJSON>([1, 2, 3])
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<byteWithPtrMarshalJSON>>(),
        @out: slice<byteWithPtrMarshalJSON>([1, 2, 3]),
        golden: true
    ),
    new(
        CaseName: Name(""),
        @in: @"""AQID""",
        ptr: @new<slice<byteWithPtrMarshalText>>(),
        @out: slice<byteWithPtrMarshalText>([1, 2, 3])
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<byteWithPtrMarshalText>>(),
        @out: slice<byteWithPtrMarshalText>([1, 2, 3]),
        golden: true
    ),

    // ints work with the marshaler but not the base64 []byte case
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<intWithMarshalJSON>>(),
        @out: slice<intWithMarshalJSON>([1, 2, 3]),
        golden: true
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<intWithMarshalText>>(),
        @out: slice<intWithMarshalText>([1, 2, 3]),
        golden: true
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<intWithPtrMarshalJSON>>(),
        @out: slice<intWithPtrMarshalJSON>([1, 2, 3]),
        golden: true
    ),
    new(
        CaseName: Name(""),
        @in: @"[""Z01"",""Z02"",""Z03""]",
        ptr: @new<slice<intWithPtrMarshalText>>(),
        @out: slice<intWithPtrMarshalText>([1, 2, 3]),
        golden: true
    ),

    new(CaseName: Name(""), @in: @"0.000001", ptr: @new<float64>(), @out: 0.000001D, golden: true),
    new(CaseName: Name(""), @in: @"1e-7", ptr: @new<float64>(), @out: 1e-7D, golden: true),
    new(CaseName: Name(""), @in: @"100000000000000000000", ptr: @new<float64>(), @out: 100000000000000000000.0D, golden: true),
    new(CaseName: Name(""), @in: @"1e+21", ptr: @new<float64>(), @out: 1e21D, golden: true),
    new(CaseName: Name(""), @in: @"-0.000001", ptr: @new<float64>(), @out: -0.000001D, golden: true),
    new(CaseName: Name(""), @in: @"-1e-7", ptr: @new<float64>(), @out: -1e-7D, golden: true),
    new(CaseName: Name(""), @in: @"-100000000000000000000", ptr: @new<float64>(), @out: -100000000000000000000.0D, golden: true),
    new(CaseName: Name(""), @in: @"-1e+21", ptr: @new<float64>(), @out: -1e21D, golden: true),
    new(CaseName: Name(""), @in: @"999999999999999900000", ptr: @new<float64>(), @out: 999999999999999900000.0D, golden: true),
    new(CaseName: Name(""), @in: @"9007199254740992", ptr: @new<float64>(), @out: 9007199254740992.0D, golden: true),
    new(CaseName: Name(""), @in: @"9007199254740993", ptr: @new<float64>(), @out: 9007199254740992.0D, golden: false),

    new(
        CaseName: Name(""),
        @in: @"{""V"": {""F2"": ""hello""}}",
        ptr: @new<VOuter>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string",
            Struct: "V",
            Field: "V.F2",
            Type: reflect.TypeFor<int32>(),
            Offset: 20
        )))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""V"": {""F4"": {}, ""F2"": ""hello""}}",
        ptr: @new<VOuter>(),
        @out: new VOuter(V: new V(F4: Ꮡ(new VOuter()))),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string",
            Struct: "V",
            Field: "V.F2",
            Type: reflect.TypeFor<int32>(),
            Offset: 30
        )))
    ),

    new(
        CaseName: Name(""),
        @in: @"{""Level1a"": ""hello""}",
        ptr: @new<Top>(),
        @out: new Top(Embed0a: Ꮡ(new Embed0a())),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string",
            Struct: "Top",
            Field: "Embed0a.Level1a",
            Type: reflect.TypeFor<nint>(),
            Offset: 19
        )))
    ),

    // issue 15146.
    // invalid inputs in wrongStringTests below.
    new(CaseName: Name(""), @in: @"{""B"":""true""}", ptr: @new<B>(), @out: new B(true), golden: true),
    new(CaseName: Name(""), @in: @"{""B"":""false""}", ptr: @new<B>(), @out: new B(false), golden: true),
    new(CaseName: Name(""), @in: @"{""B"": ""maybe""}", ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""maybe"" into bool")),
    new(CaseName: Name(""), @in: @"{""B"": ""tru""}", ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""tru"" into bool")),
    new(CaseName: Name(""), @in: @"{""B"": ""False""}", ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""False"" into bool")),
    new(CaseName: Name(""), @in: @"{""B"": ""null""}", ptr: @new<B>(), @out: new B(false)),
    new(CaseName: Name(""), @in: @"{""B"": ""nul""}", ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal ""nul"" into bool")),
    new(CaseName: Name(""), @in: @"{""B"": [2, 3]}", ptr: @new<B>(), err: errors.New(@"json: invalid use of ,string struct tag, trying to unmarshal unquoted value into bool")),

    // additional tests for disallowUnknownFields
    new(
        CaseName: Name(""),
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
"""u8.Raw(),
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
        CaseName: Name(""),
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
"""u8.Raw(),
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
    // issue 26444
    // UnmarshalTypeError without field & struct values
    new(
        CaseName: Name(""),
        @in: @"{""data"":{""test1"": ""bob"", ""test2"": 123}}",
        ptr: @new<mapStringToStringData>(),
        @out: new mapStringToStringData(new map<@string, @string>{["test1"] = "bob", ["test2"] = ""}),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number", Type: reflect.TypeFor<@string>(), Offset: 37, Struct: "mapStringToStringData", Field: "data")))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""data"":{""test1"": 123, ""test2"": ""bob""}}",
        ptr: @new<mapStringToStringData>(),
        @out: new mapStringToStringData(Data: new map<@string, @string>{["test1"] = "", ["test2"] = "bob"}),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number", Type: reflect.TypeFor<@string>(), Offset: 21, Struct: "mapStringToStringData", Field: "data")))
    ),

    // trying to decode JSON arrays or objects via TextUnmarshaler
    new(
        CaseName: Name(""),
        @in: @"[1, 2, 3]",
        ptr: @new<MustNotUnmarshalText>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "array", Type: reflect.TypeFor<ж<MustNotUnmarshalText>>(), Offset: 1)))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""foo"": ""bar""}",
        ptr: @new<MustNotUnmarshalText>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "object", Type: reflect.TypeFor<ж<MustNotUnmarshalText>>(), Offset: 1)))
    ),
    // #22369
    new(
        CaseName: Name(""),
        @in: @"{""PP"": {""T"": {""Y"": ""bad-type""}}}",
        ptr: @new<P>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string",
            Struct: "T",
            Field: "PP.T.Y",
            Type: reflect.TypeFor<nint>(),
            Offset: 29
        )))
    ),
    new(
        CaseName: Name(""),
        @in: @"{""Ts"": [{""Y"": 1}, {""Y"": 2}, {""Y"": ""bad-type""}]}",
        ptr: @new<PP>(),
        @out: new PP(Ts: [new(Y: 1), new(Y: 2), new(Y: 0)]),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(
            Value: "string",
            Struct: "T",
            Field: "Ts.Y",
            Type: reflect.TypeFor<nint>(),
            Offset: 44
        )))
    ),
    // #14702
    new(
        CaseName: Name(""),
        @in: @"invalid",
        ptr: @new<global::go.encoding.json_package.Number>(),
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(
            msg: "invalid character 'i' looking for beginning of value",
            Offset: 1
        )))
    ),
    new(
        CaseName: Name(""),
        @in: @"""invalid""",
        ptr: @new<global::go.encoding.json_package.Number>(),
        err: fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, @"""invalid""")
    ),
    new(
        CaseName: Name(""),
        @in: @"{""A"":""invalid""}",
        ptr: @new<Δtype>(),
        err: fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, @"""invalid""")
    ),
    new(
        CaseName: Name(""),
        @in: @"{""A"":""invalid""}",
        ptr: @new<Δtypeᴛ1>(),
        err: fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal %q into json.Number"u8, @"invalid")
    ),
    new(
        CaseName: Name(""),
        @in: @"{""A"":""invalid""}",
        ptr: @new<map<@string, global::go.encoding.json_package.Number>>(),
        @out: new map<@string, global::go.encoding.json_package.Number>{},
        err: fmt.Errorf("json: invalid number literal, trying to unmarshal %q into Number"u8, @"""invalid""")
    ),

    new(
        CaseName: Name(""),
        @in: @"5",
        ptr: @new<global::go.encoding.json_package.Number>(),
        @out: ((global::go.encoding.json_package.Number)(@string)"5")
    ),
    new(
        CaseName: Name(""),
        @in: @"""5""",
        ptr: @new<global::go.encoding.json_package.Number>(),
        @out: ((global::go.encoding.json_package.Number)(@string)"5")
    ),
    new(
        CaseName: Name(""),
        @in: @"{""N"":5}",
        ptr: @new<Δtypeᴛ2>(),
        @out: new Δtypeᴛ2((@string)"5")
    ),
    new(
        CaseName: Name(""),
        @in: @"{""N"":""5""}",
        ptr: @new<Δtypeᴛ2>(),
        @out: new Δtypeᴛ2((@string)"5")
    ),
    new(
        CaseName: Name(""),
        @in: @"{""N"":5}",
        ptr: @new<Δtypeᴛ3>(),
        err: fmt.Errorf("json: invalid use of ,string struct tag, trying to unmarshal unquoted value into json.Number"u8)
    ),
    new(
        CaseName: Name(""),
        @in: @"{""N"":""5""}",
        ptr: @new<Δtypeᴛ3>(),
        @out: new Δtypeᴛ3((@string)"5")
    ),

    // Verify that syntactic errors are immediately fatal,
    // while semantic errors are lazily reported
    // (i.e., allow processing to continue).
    new(
        CaseName: Name(""),
        @in: @"[1,2,true,4,5}",
        ptr: @new<slice<nint>>(),
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(msg: "invalid character '}' after array element", Offset: 14)))
    ),
    new(
        CaseName: Name(""),
        @in: @"[1,2,true,4,5]",
        ptr: @new<slice<nint>>(),
        @out: slice<nint>([1, 2, 0, 4, 5]),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "bool", Type: reflect.TypeFor<nint>(), Offset: 9)))
    ),

    new(
        CaseName: Name("DashComma"),
        @in: @"{""-"":""hello""}",
        ptr: @new<Δtypeᴛ4>(),
        @out: new Δtypeᴛ4("hello")
    ),
    new(
        CaseName: Name("DashCommaOmitEmpty"),
        @in: @"{""-"":""hello""}",
        ptr: @new<Δtypeᴛ5>(),
        @out: new Δtypeᴛ5("hello")
    ),

    new(
        CaseName: Name("ErrorForNestedUnamed"),
        @in: @"{""F"":{""V"":""s""}}",
        ptr: @new<NestedUnamed>(),
        @out: new NestedUnamed(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "string", Type: reflect.TypeFor<nint>(), Offset: 13, Field: "F.V")))
    ),
    new(
        CaseName: Name("ErrorInterface"),
        @in: @"1",
        ptr: @new<error>(),
        @out: ((error)default),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number", Type: reflect.TypeFor<error>(), Offset: 1)))
    ),
    new(
        CaseName: Name("ErrorChan"),
        @in: @"1",
        ptr: @new<channel<nint>>(),
        @out: (channel<nint>)(default),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number", Type: reflect.TypeFor<channel<nint>>(), Offset: 1)))
    ),

    // #75619
    new(
        CaseName: Name("QuotedInt/GoSyntax"),
        @in: @"{""X"": ""-0000123""}",
        ptr: @new<Δtypeᴛ6>(),
        @out: new Δtypeᴛ6(-123)
    ),
    new(
        CaseName: Name("QuotedInt/Invalid"),
        @in: @"{""X"": ""123 ""}",
        ptr: @new<Δtypeᴛ6>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 123 ", Type: reflect.TypeFor<int64>(), Field: "X", Offset: (int64)len(@"{""X"": ""123 """))))
    ),
    new(
        CaseName: Name("QuotedUint/GoSyntax"),
        @in: @"{""X"": ""0000123""}",
        ptr: @new<Δtypeᴛ7>(),
        @out: new Δtypeᴛ7(123)
    ),
    new(
        CaseName: Name("QuotedUint/Invalid"),
        @in: @"{""X"": ""0x123""}",
        ptr: @new<Δtypeᴛ7>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 0x123", Type: reflect.TypeFor<uint64>(), Field: "X", Offset: (int64)len(@"{""X"": ""0x123"""))))
    ),
    new(
        CaseName: Name("QuotedFloat/GoSyntax"),
        @in: @"{""X"": ""0x1_4p-2""}",
        ptr: @new<Δtypeᴛ8>(),
        @out: new Δtypeᴛ8(5D)
    ),
    new(
        CaseName: Name("QuotedFloat/Invalid"),
        @in: @"{""X"": ""1.5e1_""}",
        ptr: @new<Δtypeᴛ8>(),
        err: new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number 1.5e1_", Type: reflect.TypeFor<float64>(), Field: "X", Offset: (int64)len(@"{""X"": ""1.5e1_"""))))
    )
]); }

public static void TestMarshal(ж<testing.T> t) {
    var (b, err) = Marshal(allValue);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (((sstring)b) != allValueCompact) {
        t.Errorf("Marshal:");
        diff(t, b, slice<byte>(allValueCompact));
        return;
    }

    (b, err) = Marshal(pallValue);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (((sstring)b) != pallValueCompact) {
        t.Errorf("Marshal:");
        diff(t, b, slice<byte>(pallValueCompact));
        return;
    }
}

internal partial struct TestMarshalInvalidUTF8_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal @string want;
}

public static void TestMarshalInvalidUTF8(ж<testing.T> t) {
    var tests = slice<TestMarshalInvalidUTF8_tests>([
        new(Name(""u8), ((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64})), @"""hello\ufffdworld"""),
        new(Name(""u8), "", @""""""),
        new(Name(""u8), ((@string)(new byte[]{0xff})), @"""\ufffd"""),
        new(Name(""u8), ((@string)(new byte[]{0xff, 0xff})), @"""\ufffd\ufffd"""),
        new(Name(""u8), ((@string)(new byte[]{0x61, 0xff, 0x62})), @"""a\ufffdb"""),
        new(Name(""u8), ((@string)(new byte[]{0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xff, 0xaa, 0x9e})), @"""日本\ufffd\ufffd\ufffd""")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestMarshalInvalidUTF8_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (got, err) = Marshal(ttʗ1.@in);
            if (((sstring)got) != ttʗ1.want || err != nil) {
                tΔ1.Errorf("%s: Marshal(%q):\n\tgot:  (%q, %v)\n\twant: (%q, nil)", ttʗ1.Where, ttʗ1.@in, got, err, ttʗ1.want);
            }
        });
    }
}

public static void TestMarshalNumberZeroVal(ж<testing.T> t) {
    global::go.encoding.json_package.Number n = default;
    var (@out, err) = Marshal(n);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string got = (@string)@out;
    if (got != "0"u8) {
        t.Fatalf("Marshal: got %s, want 0", got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string level01Level1b2Level1c3ˢ => ref level01Level1b2Level1c3ˢᵇ;
private static readonly @string level01Level1b2Level1c3ˢᵇ = "{\"Level0\":1,\"Level1b\":2,\"Level1c\":3,\"Level1a\":5,\"LEVEL1B\":6,\"e\":{\"Level1a\":8,\"Level1b\":9,\"Level1c\":10,\"Level1d\":11,\"x\":12},\"Loop1\":13,\"Loop2\":14,\"X\":15,\"Y\":16,\"Z\":17,\"Q\":18}"u8;

public static void TestMarshalEmbeds(ж<testing.T> t) {
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
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string want = level01Level1b2Level1c3ˢ;
    if (((sstring)got) != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
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

    if (a == nil || b == nil) {
        return a == nil && b == nil;
    }
    if (isJSONError(a) || isJSONError(b)) {
        return reflect.DeepEqual(a, b); // safe for locally defined error types
    }
    return a.Error() == b.Error();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedEndOfJsonInputˢ => ref unexpectedEndOfJsonInputˢᵇ;
private static readonly @string unexpectedEndOfJsonInputˢᵇ = "unexpected end of JSON input"u8;

public static void TestUnmarshal(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(unmarshalTests)) {
        ref var tt = ref heap(new unmarshalTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var @in = slice<byte>(ttʗ1.@in);
            ref var scan = ref heap(new global::go.encoding.json_package.scanner(), out var Ꮡscan);
            {
                var err = checkValid(@in, Ꮡscan); if (err != nil) {
                    if (!equalError(err, ttʗ1.err)) {
                        tΔ1.Fatalf("%s: checkValid error:\n\tgot  %#v\n\twant %#v", ttʗ1.Where, err, ttʗ1.err);
                    }
                }
            }
            if (ttʗ1.ptr == nil) {
                return;
            }

            var typ = reflect.TypeOf(ttʗ1.ptr);
            if (typ.Kind() != reflect.ΔPointer) {
                tΔ1.Fatalf("%s: unmarshalTest.ptr %T is not a pointer type", ttʗ1.Where, ttʗ1.ptr);
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
                tΔ1.Fatalf("%s: unmarshalTest.ptr %#v is not a pointer to a zero value", ttʗ1.Where, ttʗ1.ptr);
            }

            var dec = NewDecoder(new json_test_package.bytes_ReaderжReader(bytes.NewReader(@in)));
            if (ttʗ1.useNumber) {
                dec.UseNumber();
            }
            if (ttʗ1.disallowUnknownFields) {
                dec.DisallowUnknownFields();
            }
            if (ttʗ1.err != nil && strings.Contains(ttʗ1.err.Error(), unexpectedEndOfJsonInputˢ)) {
                // In streaming mode, we expect EOF or ErrUnexpectedEOF instead.
                if (strings.TrimSpace(ttʗ1.@in) == ""u8) {
                    ttʗ1.err = io.EOF;
                } else {
                    ttʗ1.err = io.ErrUnexpectedEOF;
                }
            }
            {
                var err = dec.Decode(v.Interface()); if (!equalError(err, ttʗ1.err)) {
                    tΔ1.Fatalf("%s: Decode error:\n\tgot:  %v\n\twant: %v\n\n\tgot:  %#v\n\twant: %#v", ttʗ1.Where, err, ttʗ1.err, err, ttʗ1.err);
                } else if (err != nil && ttʗ1.@out == nil) {
                    // Initialize tt.out during an error where there are no mutations,
                    // so the output is just the zero value of the input type.
                    ttʗ1.@out = reflect.Zero(v.Elem().Type()).Interface();
                }
            }
            var got = v.Elem().Interface(); if (!reflect.DeepEqual(got, ttʗ1.@out)) {
                var (gotJSON, _) = Marshal(got);
                var (wantJSON, _) = Marshal(ttʗ1.@out);
                tΔ1.Fatalf("%s: Decode:\n\tgot:  %#+v\n\twant: %#+v\n\n\tgotJSON:  %s\n\twantJSON: %s", ttʗ1.Where, got, ttʗ1.@out, gotJSON, wantJSON);
            }

            // Check round trip also decodes correctly.
            if (ttʗ1.err == nil) {
                var (enc, err) = Marshal(v.Interface());
                if (err != nil) {
                    tΔ1.Fatalf("%s: Marshal error after roundtrip: %v", ttʗ1.Where, err);
                }
                if (ttʗ1.golden && !bytes.Equal(enc, @in)) {
                    tΔ1.Errorf("%s: Marshal:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, enc, @in);
                }
                var vv = reflect.New(reflect.TypeOf(ttʗ1.ptr).Elem());
                dec = NewDecoder(new json_test_package.bytes_ReaderжReader(bytes.NewReader(enc)));
                if (ttʗ1.useNumber) {
                    dec.UseNumber();
                }
                var errΔ1 = dec.Decode(vv.Interface()); if (errΔ1 != nil) {
                    tΔ1.Fatalf("%s: Decode(%#q) error after roundtrip: %v", ttʗ1.Where, enc, errΔ1);
                }
                if (!reflect.DeepEqual(v.Elem().Interface(), vv.Elem().Interface())) {
                    tΔ1.Fatalf("%s: Decode:\n\tgot:  %#+v\n\twant: %#+v\n\n\tgotJSON:  %s\n\twantJSON: %s",
                        ttʗ1.Where, v.Elem().Interface(), vv.Elem().Interface(),
                        stripWhitespace((@string)enc), stripWhitespace((@string)@in));
                }
            }
        });
    }
}

public static void TestUnmarshalMarshal(ж<testing.T> t) {
    initBig();
    ref var v = ref heap<any>(out var Ꮡv);
    var errΔ1 = Unmarshal(jsonBig, Ꮡv); if (errΔ1 != nil) {
        t.Fatalf("Unmarshal error: %v", errΔ1);
    }
    var (b, err) = Marshal(v);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (!bytes.Equal(jsonBig, b)) {
        t.Errorf("Marshal:");
        diff(t, b, jsonBig);
        return;
    }
}

internal partial struct TestNumberAccessors_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal int64 i;
    internal @string intErr;
    internal float64 f;
    internal @string floatErr;
}

// Independent of Decode, basic coverage of the accessors in Number
public static void TestNumberAccessors(ж<testing.T> t) {
    var tests = slice<TestNumberAccessors_tests>([
        new(CaseName: Name(""u8), @in: "-1.23e1", intErr: "strconv.ParseInt: parsing \"-1.23e1\": invalid syntax", f: -1.23e1D),
        new(CaseName: Name(""u8), @in: "-12", i: -12, f: -12.0D),
        new(CaseName: Name(""u8), @in: "1e1000", intErr: "strconv.ParseInt: parsing \"1e1000\": invalid syntax", floatErr: "strconv.ParseFloat: parsing \"1e1000\": value out of range")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestNumberAccessors_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            global::go.encoding.json_package.Number n = (global::go.encoding.json_package.Number)ttʗ1.@in;
            @string got = n.String(); if (got != ttʗ1.@in) {
                tΔ1.Errorf("%s: Number(%q).String() = %s, want %s", ttʗ1.Where, ttʗ1.@in, got, ttʗ1.@in);
            }
            {
                var (i, err) = n.Int64(); if (err == nil && ttʗ1.intErr == ""u8 && i != ttʗ1.i) {
                    tΔ1.Errorf("%s: Number(%q).Int64() = %d, want %d", ttʗ1.Where, ttʗ1.@in, i, ttʗ1.i);
                } else if ((err == nil && ttʗ1.intErr != ""u8) || (err != nil && err.Error() != ttʗ1.intErr)) {
                    tΔ1.Errorf("%s: Number(%q).Int64() error:\n\tgot:  %v\n\twant: %v", ttʗ1.Where, ttʗ1.@in, err, ttʗ1.intErr);
                }
            }
            {
                var (f, err) = n.Float64(); if (err == nil && ttʗ1.floatErr == ""u8 && f != ttʗ1.f) {
                    tΔ1.Errorf("%s: Number(%q).Float64() = %g, want %g", ttʗ1.Where, ttʗ1.@in, f, ttʗ1.f);
                } else if ((err == nil && ttʗ1.floatErr != ""u8) || (err != nil && err.Error() != ttʗ1.floatErr)) {
                    tΔ1.Errorf("%s: Number(%q).Float64() error:\n\tgot  %v\n\twant: %v", ttʗ1.Where, ttʗ1.@in, err, ttʗ1.floatErr);
                }
            }
        });
    }
}

public static void TestLargeByteSlice(ж<testing.T> t) {
    var s0 = new slice<byte>(2000);
    foreach (var i in range(len(s0))) {
        s0[i] = (byte)i;
    }
    var (b, err) = Marshal(s0);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    ref var s1 = ref heap<slice<byte>>(out var Ꮡs1);
    var errΔ1 = Unmarshal(b, Ꮡs1); if (errΔ1 != nil) {
        t.Fatalf("Unmarshal error: %v", errΔ1);
    }
    if (!bytes.Equal(s0, s1)) {
        t.Errorf("Marshal:");
        diff(t, s0, s1);
    }
}

public partial struct Xint {
    public nint X;
}

public static void TestUnmarshalInterface(ж<testing.T> t) {
    ref var xint = ref heap(new Xint(), out var Ꮡxint);
    ref var i = ref heap<any>(out var Ꮡi);

    i = Ꮡxint;
    var err = Unmarshal(slice<byte>(@"{""X"":1}"u8), Ꮡi); if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    if (xint.X != 1) {
        t.Fatalf("xint.X = %d, want 1", xint.X);
    }
}

public static void TestUnmarshalPtrPtr(ж<testing.T> t) {
    ref var xint = ref heap(new Xint(), out var Ꮡxint);
    ref var pxint = ref heap<ж<Xint>>(Ꮡxint, out var Ꮡpxint);
    var err = Unmarshal(slice<byte>(@"{""X"":1}"u8), Ꮡpxint); if (err != nil) {
        t.Fatalf("Unmarshal: %v", err);
    }
    if (xint.X != 1) {
        t.Fatalf("xint.X = %d, want 1", xint.X);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = "\"foobar\"<html> [\u2028 \u2029]";

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = @"""\""foobar\""\u003chtml\u003e [\u2028 \u2029]"""u8;

public static void TestEscape(ж<testing.T> t) {
    @string input = inputᶜ;
    @string want = wantᶜ;
    var (got, err) = Marshal(input);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (((sstring)got) != want) {
        t.Errorf("Marshal(%#q):\n\tgot:  %s\n\twant: %s", input, got, want);
    }
}

// WrongString is a struct that's misusing the ,string modifier.
internal partial struct TestErrorMessageFromMisusedString_WrongString /*dyn*/ {
    public @string Message; /*`json:"result,string"`*/
}

internal partial struct TestErrorMessageFromMisusedString_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in, err;
}

// If people misuse the ,string modifier, the error message should be
// helpful, telling the user that they're doing it wrong.
public static void TestErrorMessageFromMisusedString(ж<testing.T> t) {
    var tests = slice<TestErrorMessageFromMisusedString_tests>([
        new(Name(""u8), @"{""result"":""x""}", @"json: invalid use of ,string struct tag, trying to unmarshal ""x"" into string"),
        new(Name(""u8), @"{""result"":""foo""}", @"json: invalid use of ,string struct tag, trying to unmarshal ""foo"" into string"),
        new(Name(""u8), @"{""result"":""123""}", @"json: invalid use of ,string struct tag, trying to unmarshal ""123"" into string"),
        new(Name(""u8), @"{""result"":123}", @"json: invalid use of ,string struct tag, trying to unmarshal unquoted value into string"),
        new(Name(""u8), @"{""result"":""\""""}", @"json: invalid use of ,string struct tag, trying to unmarshal ""\"""" into string"),
        new(Name(""u8), @"{""result"":""\""foo""}", @"json: invalid use of ,string struct tag, trying to unmarshal ""\""foo"" into string")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestErrorMessageFromMisusedString_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var r = strings.NewReader(ttʗ1.@in);
            ref var s = ref heap(new TestErrorMessageFromMisusedString_WrongString(), out var Ꮡs);
            var err = NewDecoder(new json_test_package.strings_ReaderжReader(r)).Decode(Ꮡs);
            @string got = fmt.Sprintf("%v"u8, err);
            if (got != ttʗ1.err) {
                tΔ1.Errorf("%s: Decode error:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, got, ttʗ1.err);
            }
        });
    }
}

public partial struct All {
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
    public @string Foo; /*`json:"bar"`*/
    public @string Foo2; /*`json:"bar2,dummyopt"`*/
    public int64 IntStr; /*`json:",string"`*/
    public uintptr UintptrStr; /*`json:",string"`*/
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

public partial struct Small {
    public @string Tag;
}

internal static readonly ж<All> ᏑallValue = new StaticBox<All>(new All(
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
    Foo: "foo",
    Foo2: "foo2",
    IntStr: 42,
    UintptrStr: 44,
    String: "16",
    Map: new map<@string, Small>{
        ["17"] = new(Tag: "tag17"),
        ["18"] = new(Tag: "tag18")
    },
    MapP: new map<@string, ж<Small>>{
        ["19"] = Ꮡ(new Small(Tag: "tag19")),
        ["20"] = default
    },
    EmptyMap: new map<@string, Small>{},
    Slice: [new(Tag: "tag20"), new(Tag: "tag21")],
    SliceP: [Ꮡ(new Small(Tag: "tag22")), default, Ꮡ(new Small(Tag: "tag23"))],
    EmptySlice: [],
    StringSlice: ["str24", "str25", "str26"],
    ByteSlice: [27, 28, 29],
    Small: new Small(Tag: "tag30"),
    PSmall: Ꮡ(new Small(Tag: "tag31")),
    Interface: 5.2D
));
internal static ref All allValue => ref ((StaticBox<All>)ᏑallValue).ValueSlot;

internal static readonly ж<All> ᏑpallValue = new StaticBox<All>(new All(
    PBool: ᏑallValue.ᏑBool,
    PInt: ᏑallValue.ᏑInt,
    PInt8: ᏑallValue.ᏑInt8,
    PInt16: ᏑallValue.ᏑInt16,
    PInt32: ᏑallValue.ᏑInt32,
    PInt64: ᏑallValue.ᏑInt64,
    PUint: ᏑallValue.ᏑUint,
    PUint8: ᏑallValue.ᏑUint8,
    PUint16: ᏑallValue.ᏑUint16,
    PUint32: ᏑallValue.ᏑUint32,
    PUint64: ᏑallValue.ᏑUint64,
    PUintptr: ᏑallValue.ᏑUintptr,
    PFloat32: ᏑallValue.ᏑFloat32,
    PFloat64: ᏑallValue.ᏑFloat64,
    PString: ᏑallValue.ᏑString,
    PMap: ᏑallValue.ᏑMap,
    PMapP: ᏑallValue.ᏑMapP,
    PSlice: ᏑallValue.ᏑSlice,
    PSliceP: ᏑallValue.ᏑSliceP,
    PPSmall: ᏑallValue.ᏑPSmall,
    PInterface: ᏑallValue.ᏑInterface
));
internal static ref All pallValue => ref ((StaticBox<All>)ᏑpallValue).ValueSlot;

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
"""u8.Raw();

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
"""u8.Raw();

internal static @string pallValueCompact = stripWhitespace(pallValueIndent);

internal partial struct TestRefUnmarshal_S /*dyn*/ {
    // Ref is defined in encode_test.go.
    public Ref R0;
    public ж<Ref> R1;
    public RefText R2;
    public ж<RefText> R3;
}

public static void TestRefUnmarshal(ж<testing.T> t) {
    var want = new TestRefUnmarshal_S(
        R0: 12,
        R1: @new<Ref>(),
        R2: 13,
        R3: @new<RefText>()
    );
    want.R1.Value = 12;
    want.R3.Value = 13;

    ref var got = ref heap(new TestRefUnmarshal_S(), out var Ꮡgot);
    var err = Unmarshal(slice<byte>(@"{""R0"":""ref"",""R1"":""ref"",""R2"":""ref"",""R3"":""ref""}"u8), Ꮡgot); if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    if (!reflect.DeepEqual(got, want)) {
        t.Errorf("Unmarsha:\n\tgot:  %+v\n\twant: %+v", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string number11Number2ˢ => ref number11Number2ˢᵇ;
private static readonly @string number11Number2ˢᵇ = @"{""Number1"":""1"", ""Number2"":""""}"u8;

internal partial struct TestEmptyString_T2 /*dyn*/ {
    public nint Number1; /*`json:",string"`*/
    public nint Number2; /*`json:",string"`*/
}

// Test that the empty string doesn't panic decoding when ,string is specified
// Issue 3450
public static void TestEmptyString(ж<testing.T> t) {
    @string data = number11Number2ˢ;
    var dec = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(data)));
    ref var got = ref heap(new TestEmptyString_T2(), out var Ꮡgot);
    var err = dec.Decode(Ꮡgot);
    switch (ᐧ) {
    case {} when err == nil: {
        t.Fatalf("Decode error: got nil, want non-nil");
        break;
    }
    case {} when got.Number1 is not 1: {
        t.Fatalf("Decode: got.Number1 = %d, want 1", got.Number1);
        break;
    }}
}

internal partial struct TestNullString_T /*dyn*/ {
    public nint A; /*`json:",string"`*/
    public nint B; /*`json:",string"`*/
    public ж<nint> C; /*`json:",string"`*/
}

// Test that a null for ,string is not replaced with the previous quoted string (issue 7046).
// It should also not be an error (issue 2540, issue 8587).
public static void TestNullString(ж<testing.T> t) {
    var data = slice<byte>(@"{""A"": ""1"", ""B"": null, ""C"": null}"u8);
    ref var s = ref heap(new TestNullString_T(), out var Ꮡs);
    s.B = 1;
    s.C = @new<nint>();
    s.C.Value = 2;
    var err = Unmarshal(data, Ꮡs);
    switch (ᐧ) {
    case {} when err != nil: {
        t.Fatalf("Unmarshal error: %v", err);
        break;
    }
    case {} when s.B is not 1: {
        t.Fatalf("Unmarshal: s.B = %d, want 1", s.B);
        break;
    }
    case {} when s.C != nil: {
        t.Fatalf("Unmarshal: s.C = %d, want non-nil", s.C.OrTypedNil());
        break;
    }}
}

internal static ж<T> addr<T>(T vʗp) {
    ref var v = ref heap(vʗp, out var Ꮡv);

    return Ꮡv;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static readonly object barˢ = (@string)"bar"u8;

internal partial struct TestInterfaceSet_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any pre;
    internal @string json;
    internal any post;
}

internal partial struct TestInterfaceSet_b /*dyn*/ {
    public any X;
}

public static void TestInterfaceSet(ж<testing.T> t) {
    var errUnmarshal = Ꮡ(new UnmarshalTypeError(Value: "object", Offset: 6, Type: reflect.TypeFor<nint>(), Field: "X"));
    var tests = slice<TestInterfaceSet_tests>([
        new(Name(""u8), fooˢ, @"""bar""", barˢ),
        new(Name(""u8), fooˢ, @"2", 2.0D),
        new(Name(""u8), fooˢ, @"true", true),
        new(Name(""u8), fooˢ, @"null", default),
        new(Name(""u8), new map<@string, any>{}, @"true", true),
        new(Name(""u8), slice<@string>([]), @"true", true),

        new(Name(""u8), (any)default, @"null", (any)default),
        new(Name(""u8), (ж<nint>)nil, @"null", (any)default),
        new(Name(""u8), addr((nint)0).OrTypedNil(), @"null", (any)default),
        new(Name(""u8), addr((nint)1).OrTypedNil(), @"null", (any)default),
        new(Name(""u8), (ж<ж<nint>>)nil, @"null", (any)default),
        new(Name(""u8), addr<ж<nint>>(nil).OrTypedNil(), @"null", addr<ж<nint>>(nil).OrTypedNil()),
        new(Name(""u8), addr(addr((nint)1)).OrTypedNil(), @"null", addr<ж<nint>>(nil).OrTypedNil()),
        new(Name(""u8), (ж<ж<ж<nint>>>)nil, @"null", (any)default),
        new(Name(""u8), addr<ж<ж<nint>>>(nil).OrTypedNil(), @"null", addr<ж<ж<nint>>>(nil).OrTypedNil()),
        new(Name(""u8), addr(addr<ж<nint>>(nil)).OrTypedNil(), @"null", addr<ж<ж<nint>>>(nil).OrTypedNil()),
        new(Name(""u8), addr(addr(addr((nint)1))).OrTypedNil(), @"null", addr<ж<ж<nint>>>(nil).OrTypedNil()),

        new(Name(""u8), (any)default, @"2", (float64)2D),
        new(Name(""u8), (nint)1, @"2", (float64)2D),
        new(Name(""u8), (ж<nint>)nil, @"2", (float64)2D),
        new(Name(""u8), addr((nint)0).OrTypedNil(), @"2", addr((nint)2).OrTypedNil()),
        new(Name(""u8), addr((nint)1).OrTypedNil(), @"2", addr((nint)2).OrTypedNil()),
        new(Name(""u8), (ж<ж<nint>>)nil, @"2", (float64)2D),
        new(Name(""u8), addr<ж<nint>>(nil).OrTypedNil(), @"2", addr(addr((nint)2)).OrTypedNil()),
        new(Name(""u8), addr(addr((nint)1)).OrTypedNil(), @"2", addr(addr((nint)2)).OrTypedNil()),
        new(Name(""u8), (ж<ж<ж<nint>>>)nil, @"2", (float64)2D),
        new(Name(""u8), addr<ж<ж<nint>>>(nil).OrTypedNil(), @"2", addr(addr(addr((nint)2))).OrTypedNil()),
        new(Name(""u8), addr(addr<ж<nint>>(nil)).OrTypedNil(), @"2", addr(addr(addr((nint)2))).OrTypedNil()),
        new(Name(""u8), addr(addr(addr((nint)1))).OrTypedNil(), @"2", addr(addr(addr((nint)2))).OrTypedNil()),

        new(Name(""u8), (any)default, @"{}", new map<@string, any>{}),
        new(Name(""u8), (nint)1, @"{}", new map<@string, any>{}),
        new(Name(""u8), (ж<nint>)nil, @"{}", new map<@string, any>{}),
        new(Name(""u8), addr((nint)0).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr((nint)1).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil()),
        new(Name(""u8), (ж<ж<nint>>)nil, @"{}", new map<@string, any>{}),
        new(Name(""u8), addr<ж<nint>>(nil).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr(addr((nint)1)).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil()),
        new(Name(""u8), (ж<ж<ж<nint>>>)nil, @"{}", new map<@string, any>{}),
        new(Name(""u8), addr<ж<ж<nint>>>(nil).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr(addr<ж<nint>>(nil)).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil()),
        new(Name(""u8), addr(addr(addr((nint)1))).OrTypedNil(), @"{}", errUnmarshal.OrTypedNil())
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestInterfaceSet_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            ref var b = ref heap<TestInterfaceSet_b>(new TestInterfaceSet_b(ttʗ1.pre), out var Ꮡb);
            @string blob = @"{""X"":"u8 + ttʗ1.json + @"}"u8;
            var err = Unmarshal(slice<byte>(blob), Ꮡb); if (err != nil) {
                var (wantErr, _) = ttʗ1.post._<error>(ᐧ); if (equalError(err, wantErr)) {
                    return;
                }
                tΔ1.Fatalf("%s: Unmarshal(%#q) error: %v", ttʗ1.Where, blob, err);
            }
            if (!reflect.DeepEqual(b.X, ttʗ1.post)) {
                tΔ1.Errorf("%s: Unmarshal(%#q):\n\tpre.X:  %#v\n\tgot.X:  %#v\n\twant.X: %#v", ttʗ1.Where, blob, ttʗ1.pre, b.X, ttʗ1.post);
            }
        });
    }
}

public partial struct NullTest {
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
public static void TestUnmarshalNulls(ж<testing.T> t) {
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
"""u8.Raw());
    ref var nulls = ref heap<NullTest>(new NullTest(
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
        String: "14",
        PBool: @new<bool>(),
        Map: new map<@string, @string>{},
        Slice: [],
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
    ), out var Ꮡnulls);

    @string before = nulls.Time.String();

    var err = Unmarshal(jsonData, Ꮡnulls);
    if (err != nil) {
        t.Errorf("Unmarshal of null values failed: %v", err);
    }
    if (!nulls.Bool || nulls.Int != 2 || nulls.Int8 != 3 || nulls.Int16 != 4 || nulls.Int32 != 5 || nulls.Int64 != 6 || nulls.Uint != 7 || nulls.Uint8 != 8 || nulls.Uint16 != 9 || nulls.Uint32 != 10 || nulls.Uint64 != 11 || nulls.Float32 != 12.1F || nulls.Float64 != 13.1D || nulls.String != "14"u8) {
        t.Errorf("Unmarshal of null values affected primitives");
    }

    if (nulls.PBool != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PBool");
    }
    if (nulls.Map != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.Map");
    }
    if (nulls.Slice != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.Slice");
    }
    if (nulls.Interface != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.Interface");
    }
    if (nulls.PRaw != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PRaw");
    }
    if (nulls.PTime != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PTime");
    }
    if (nulls.PBigInt != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PBigInt");
    }
    if (nulls.PText != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PText");
    }
    if (nulls.PBuffer != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PBuffer");
    }
    if (nulls.PStruct != nil) {
        t.Errorf("Unmarshal of null did not clear nulls.PStruct");
    }

    if (((@string)(slice<byte>)nulls.Raw) != "null"u8) {
        t.Errorf("Unmarshal of RawMessage null did not record null: %v", (@string)(slice<byte>)nulls.Raw);
    }
    if (nulls.Time.String() != before) {
        t.Errorf("Unmarshal of time.Time null set time to %v", nulls.Time.String());
    }
    if (Ꮡnulls.ᏑBigInt.String() != "123"u8) {
        t.Errorf("Unmarshal of big.Int null set int to %v", Ꮡnulls.ᏑBigInt.String());
    }
}

public partial struct MustNotUnmarshalJSON {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mustNotUnmarshalJSONWasˢ => ref mustNotUnmarshalJSONWasˢᵇ;
private static readonly @string mustNotUnmarshalJSONWasˢᵇ = "MustNotUnmarshalJSON was used"u8;

public static error UnmarshalJSON(this MustNotUnmarshalJSON x, slice<byte> data) {
    return errors.New(mustNotUnmarshalJSONWasˢ);
}

public partial struct MustNotUnmarshalText {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mustNotUnmarshalTextWasˢ => ref mustNotUnmarshalTextWasˢᵇ;
private static readonly @string mustNotUnmarshalTextWasˢᵇ = "MustNotUnmarshalText was used"u8;

public static error UnmarshalText(this MustNotUnmarshalText x, slice<byte> text) {
    return errors.New(mustNotUnmarshalTextWasˢ);
}

internal partial struct TestStringKind_stringKind /*@string*/;

public static void TestStringKind(ж<testing.T> t) {
    var want = new map<TestStringKind_stringKind, nint>{[(@string)"foo"] = 42};
    var (data, err) = Marshal(want);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    ref var got = ref heap<map<TestStringKind_stringKind, nint>>(out var Ꮡgot);
    err = Unmarshal(data, Ꮡgot);
    if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    if (!maps.Equal<map<TestStringKind_stringKind, nint>, map<TestStringKind_stringKind, nint>, TestStringKind_stringKind, nint>(got, want)) {
        t.Fatalf("Marshal/Unmarshal mismatch:\n\tgot:  %v\n\twant: %v", got, want);
    }
}

internal partial struct TestByteKind_byteKind /*[]byte*/;

// Custom types with []byte as underlying type could not be marshaled
// and then unmarshaled.
// Issue 8962.
public static void TestByteKind(ж<testing.T> t) {
    var want = (TestByteKind_byteKind)slice<byte>((@string)"hello"u8);
    var (data, err) = Marshal(want);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    ref var got = ref heap<TestByteKind_byteKind>(out var Ꮡgot);
    err = Unmarshal(data, Ꮡgot);
    if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    if (!slices.Equal<TestByteKind_byteKind, byte>(got, want)) {
        t.Fatalf("Marshal/Unmarshal mismatch:\n\tgot:  %v\n\twant: %v", got, want);
    }
}

internal partial struct TestSliceOfCustomByte_Uint8 /*num:uint8*/;

// The fix for issue 8962 introduced a regression.
// Issue 12921.
public static void TestSliceOfCustomByte(ж<testing.T> t) {
    var want = widen<uint8, TestSliceOfCustomByte_Uint8>(slice<uint8>((@string)"hello"u8), elemᴛ0 => (TestSliceOfCustomByte_Uint8)elemᴛ0);
    var (data, err) = Marshal(want);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    ref var got = ref heap<slice<TestSliceOfCustomByte_Uint8>>(out var Ꮡgot);
    err = Unmarshal(data, Ꮡgot);
    if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
    if (!slices.Equal<slice<TestSliceOfCustomByte_Uint8>, TestSliceOfCustomByte_Uint8>(got, want)) {
        t.Fatalf("Marshal/Unmarshal mismatch:\n\tgot:  %v\n\twant: %v", got, want);
    }
}

internal partial struct TestUnmarshalTypeError_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any dest;
    internal @string @in;
}

public static void TestUnmarshalTypeError(ж<testing.T> t) {
    var tests = slice<TestUnmarshalTypeError_tests>([
        new(Name(""u8), @new<@string>(), @"{""user"": ""name""}"), // issue 4628.
        new(Name(""u8), @new<error>(), @"{}"),                     // issue 4222
        new(Name(""u8), @new<error>(), @"[]"),
        new(Name(""u8), @new<error>(), @""""""),
        new(Name(""u8), @new<error>(), @"123"),
        new(Name(""u8), @new<error>(), @"true")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestUnmarshalTypeError_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var err = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.dest);
            var (_, ok) = err._<ж<global::go.encoding.json_package.UnmarshalTypeError>>(ᐧ); if (!ok) {
                tΔ1.Errorf("%s: Unmarshal(%#q, %T):\n\tgot:  %T\n\twant: %T",
                    ttʗ1.Where, ttʗ1.@in, ttʗ1.dest, err, @new<global::go.encoding.json_package.UnmarshalTypeError>());
            }
        });
    }
}

internal partial struct TestUnmarshalSyntax_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
}

public static void TestUnmarshalSyntax(ж<testing.T> t) {
    ref var x = ref heap<any>(out var Ꮡx);
    var tests = slice<TestUnmarshalSyntax_tests>([
        new(Name(""u8), "tru"),
        new(Name(""u8), "fals"),
        new(Name(""u8), "nul"),
        new(Name(""u8), "123e"),
        new(Name(""u8), @"""hello"),
        new(Name(""u8), @"[1,2,3"),
        new(Name(""u8), @"{""key"":1"),
        new(Name(""u8), @"{""key"":1,")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestUnmarshalSyntax_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var err = Unmarshal(slice<byte>(ttʗ1.@in), Ꮡx);
            var (_, ok) = err._<ж<global::go.encoding.json_package.SyntaxError>>(ᐧ); if (!ok) {
                tΔ1.Errorf("%s: Unmarshal(%#q, any):\n\tgot:  %T\n\twant: %T",
                    ttʗ1.Where, ttʗ1.@in, err, @new<global::go.encoding.json_package.SyntaxError>());
            }
        });
    }
}

// Test handling of unexported fields that should be ignored.
// Issue 4660
internal partial struct unexportedFields {
    public @string Name;
    internal map<@string, any> m; /*`json:"-"`*/
    internal map<@string, any> m2; /*`json:"abcd"`*/
    internal slice<nint> s; /*`json:"-"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameBobMX123M2Y456AbcdZˢ => ref nameBobMX123M2Y456AbcdZˢᵇ;
private static readonly @string nameBobMX123M2Y456AbcdZˢᵇ = @"{""Name"": ""Bob"", ""m"": {""x"": 123}, ""m2"": {""y"": 456}, ""abcd"": {""z"": 789}, ""s"": [2, 3]}"u8;

public static void TestUnmarshalUnexported(ж<testing.T> t) {
    @string input = nameBobMX123M2Y456AbcdZˢ;
    var want = Ꮡ(new unexportedFields(Name: "Bob"));

    var @out = Ꮡ(new unexportedFields());
    var err = Unmarshal(slice<byte>(input), @out.OrTypedNil());
    if (err != nil) {
        t.Errorf("Unmarshal error: %v", err);
    }
    if (!reflect.DeepEqual(@out.OrTypedNil(), want.OrTypedNil())) {
        t.Errorf("Unmarshal:\n\tgot:  %+v\n\twant: %+v", @out.OrTypedNil(), want.OrTypedNil());
    }
}

// Time3339 is a time.Time which encodes to and from JSON
// as an RFC 3339 time in UTC.
public partial struct Time3339 /*global::go.time_package.Time*/;

public static error UnmarshalJSON(this ref Time3339 t, slice<byte> b) {
    if (len(b) < 2 || b[0] != '"' || b[len(b) - 1] != '"') {
        return fmt.Errorf("types: failed to unmarshal non-string value %q as an RFC 3339 time"u8, b);
    }
    var (tm, err) = time.Parse(time.RFC3339, (@string)(b.slice(1, len(b) - 1)));
    if (err != nil) {
        return err;
    }
    t = (Time3339)tm;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rangeˢ => ref rangeˢᵇ;
private static readonly @string rangeˢᵇ = "range"u8;

public static void TestUnmarshalJSONLiteralError(ж<testing.T> t) {
    ref var t3 = ref heap(new Time3339(), out var Ꮡt3);
    var err = Unmarshal(slice<byte>(@"""0000-00-00T00:00:00Z"""u8), Ꮡt3);
    switch (ᐧ) {
    case {} when err == nil: {
        t.Fatalf("Unmarshal error: got nil, want non-nil");
        break;
    }
    case {} when !strings.Contains(err.Error(), rangeˢ): {
        t.Errorf("Unmarshal error:\n\tgot:  %v\n\twant: out of range", err);
        break;
    }}
}

// Test that extra object elements in an array do not result in a
// "data changing underfoot" error.
// Issue 3717
public static void TestSkipArrayObjects(ж<testing.T> t) {
    @string json = @"[{}]";
    ref var dest = ref heap(new array<any>(0), out var Ꮡdest);

    var err = Unmarshal(slice<byte>(json), Ꮡdest);
    if (err != nil) {
        t.Errorf("Unmarshal error: %v", err);
    }
}

internal partial struct TestPrefilled_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal any ptr;
    internal any @out;
}

// Test semantics of pre-filled data, such as struct fields, map elements,
// slices, and arrays.
// Issues 4900 and 8837, among others.
public static void TestPrefilled(ж<testing.T> t) {
    // Values here change, cannot reuse table across runs.
    var tests = slice<TestPrefilled_tests>([new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1, ""Y"": 2}",
        ptr: Ꮡ(new XYZ(X: (float32)3F, Y: (int16)4, Z: 1.5D)),
        @out: Ꮡ(new XYZ(X: (float64)1D, Y: (float64)2D, Z: 1.5D))
    ), new(
        CaseName: Name(""u8),
        @in: @"{""X"": 1, ""Y"": 2}",
        ptr: Ꮡ(new map<@string, any>{["X"] = (float32)3F, ["Y"] = (int16)4, ["Z"] = 1.5D}),
        @out: Ꮡ(new map<@string, any>{["X"] = (float64)1D, ["Y"] = (float64)2D, ["Z"] = 1.5D})
    ), new(
        CaseName: Name(""u8),
        @in: @"[2]",
        ptr: Ꮡ(slice<nint>([1])),
        @out: Ꮡ(slice<nint>([2]))
    ), new(
        CaseName: Name(""u8),
        @in: @"[2, 3]",
        ptr: Ꮡ(slice<nint>([1])),
        @out: Ꮡ(slice<nint>([2, 3]))
    ), new(
        CaseName: Name(""u8),
        @in: @"[2, 3]",
        ptr: Ꮡ(new nint[]{1}.array()),
        @out: Ꮡ(new nint[]{2}.array())
    ), new(
        CaseName: Name(""u8),
        @in: @"[3]",
        ptr: Ꮡ(new nint[]{1, 2}.array()),
        @out: Ꮡ(new nint[]{3, 0}.array())
    )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestPrefilled_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            @string ptrstr = fmt.Sprintf("%v"u8, ttʗ1.ptr);
            var err = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.ptr); // tt.ptr edited here
            if (err != nil) {
                tΔ1.Errorf("%s: Unmarshal error: %v", ttʗ1.Where, err);
            }
            if (!reflect.DeepEqual(ttʗ1.ptr, ttʗ1.@out)) {
                tΔ1.Errorf("%s: Unmarshal(%#q, %T):\n\tgot:  %v\n\twant: %v", ttʗ1.Where, ttʗ1.@in, ptrstr, ttʗ1.ptr, ttʗ1.@out);
            }
        });
    }
}

internal partial struct TestInvalidUnmarshal_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal any v;
    internal error wantErr;
}

public static void TestInvalidUnmarshal(ж<testing.T> t) {
    var tests = slice<TestInvalidUnmarshal_tests>([
        new(Name(""u8), @"{""a"":""1""}", default, new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError()))),
        new(Name(""u8), @"{""a"":""1""}", new EmptyStruct(), new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<EmptyStruct>())))),
        new(Name(""u8), @"{""a"":""1""}", (ж<nint>)nil, new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<ж<nint>>())))),
        new(Name(""u8), @"123", default, new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError()))),
        new(Name(""u8), @"123", new EmptyStruct(), new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<EmptyStruct>())))),
        new(Name(""u8), @"123", (ж<nint>)nil, new global::go.encoding.json_package.InvalidUnmarshalErrorжerror(Ꮡ(new InvalidUnmarshalError(reflect.TypeFor<ж<nint>>())))),
        new(Name(""u8), @"123", @new<net.IP>(), new global::go.encoding.json_package.UnmarshalTypeErrorжerror(Ꮡ(new UnmarshalTypeError(Value: "number", Type: reflect.TypeFor<ж<net.IP>>(), Offset: 3))))
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestInvalidUnmarshal_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var gotErr = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.v);
            switch (ᐧ) {
            case {} when gotErr == nil: {
                tΔ1.Fatalf("%s: Unmarshal error: got nil, want non-nil", ttʗ1.Where);
                break;
            }
            case {} when !reflect.DeepEqual(gotErr, ttʗ1.wantErr): {
                tΔ1.Errorf("%s: Unmarshal error:\n\tgot:  %#v\n\twant: %#v", ttʗ1.Where, gotErr, ttʗ1.wantErr);
                break;
            }}
        });
    }
}

internal partial struct TestInvalidStringOption_item /*dyn*/ {
    public time.Time T; /*`json:",string"`*/
    public map<@string, @string> M; /*`json:",string"`*/
    public slice<@string> S; /*`json:",string"`*/
    public array<@string> A = new(1); /*`json:",string"`*/
    public any I; /*`json:",string"`*/
    public ж<nint> P; /*`json:",string"`*/
}

// Test that string option is ignored for invalid types.
// Issue 9812.
public static void TestInvalidStringOption(ж<testing.T> t) {
    ref var num = ref heap<nint>(0, out var Ꮡnum);
    ref var item = ref heap<TestInvalidStringOption_item>(new TestInvalidStringOption_item(M: new map<@string, @string>(), S: new slice<@string>(0), I: num, P: Ꮡnum), out var Ꮡitem);

    var (data, err) = Marshal(item);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }

    err = Unmarshal(data, Ꮡitem);
    if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
}

internal partial struct TestUnmarshalEmbeddedUnexported_embed1 /*dyn*/ {
    public nint Q;
}

internal partial struct TestUnmarshalEmbeddedUnexported_embed2 /*dyn*/ {
    public nint Q;
}

internal partial struct TestUnmarshalEmbeddedUnexported_embed3 /*dyn*/ {
    public int64 Q; /*`json:",string"`*/
}

internal partial struct TestUnmarshalEmbeddedUnexported_S1 /*dyn*/ {
    /*embed*/ internal ж<TestUnmarshalEmbeddedUnexported_embed1> embed1;
    public nint R;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S2 /*dyn*/ {
    /*embed*/ internal ж<TestUnmarshalEmbeddedUnexported_embed1> embed1;
    public nint Q;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S3 /*dyn*/ {
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed1 embed1;
    public nint R;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S4 /*dyn*/ {
    /*embed*/ internal ж<TestUnmarshalEmbeddedUnexported_embed1> embed1;
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed2 embed2;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S5 /*dyn*/ {
    /*embed*/ internal ж<TestUnmarshalEmbeddedUnexported_embed3> embed3;
    public nint R;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S6 /*dyn*/ {
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed1 embed1; /*`json:"embed1"`*/
}

internal partial struct TestUnmarshalEmbeddedUnexported_S7 /*dyn*/ {
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed1 embed1; /*`json:"embed1"`*/
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed2 embed2;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S8 /*dyn*/ {
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed1 embed1; /*`json:"embed1"`*/
    /*embed*/ internal TestUnmarshalEmbeddedUnexported_embed2 embed2; /*`json:"embed2"`*/
    public nint Q;
}

internal partial struct TestUnmarshalEmbeddedUnexported_S9 /*dyn*/ {
    /*embed*/ internal unexportedWithMethods unexportedWithMethods; /*`json:"embed"`*/
}

internal partial struct TestUnmarshalEmbeddedUnexported_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
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
public static void TestUnmarshalEmbeddedUnexported(ж<testing.T> t) {
    var tests = slice<TestUnmarshalEmbeddedUnexported_tests>([new(
        // Error since we cannot set S1.embed1, but still able to set S1.R.
        CaseName: Name(""u8),
        @in: @"{""R"":2,""Q"":1}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S1>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S1(R: 2)),
        err: fmt.Errorf("json: cannot set embedded pointer to unexported struct: json.embed1"u8)
    ), new(
        // The top level Q field takes precedence.
        CaseName: Name(""u8),
        @in: @"{""Q"":1}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S2>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S2(Q: 1))
    ), new(
        // No issue with non-pointer variant.
        CaseName: Name(""u8),
        @in: @"{""R"":2,""Q"":1}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S3>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S3(embed1: new TestUnmarshalEmbeddedUnexported_embed1(Q: 1), R: 2))
    ), new(
        // No error since both embedded structs have field R, which annihilate each other.
        // Thus, no attempt is made at setting S4.embed1.
        CaseName: Name(""u8),
        @in: @"{""R"":2}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S4>(),
        @out: @new<TestUnmarshalEmbeddedUnexported_S4>()
    ), new(
        // Error since we cannot set S5.embed1, but still able to set S5.R.
        CaseName: Name(""u8),
        @in: @"{""R"":2,""Q"":1}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S5>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S5(R: 2)),
        err: fmt.Errorf("json: cannot set embedded pointer to unexported struct: json.embed3"u8)
    ), new(
        // Issue 24152, ensure decodeState.indirect does not panic.
        CaseName: Name(""u8),
        @in: @"{""embed1"": {""Q"": 1}}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S6>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S6(new TestUnmarshalEmbeddedUnexported_embed1(1)))
    ), new(
        // Issue 24153, check that we can still set forwarded fields even in
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
        CaseName: Name(""u8),
        @in: @"{""embed1"": {""Q"": 1}, ""Q"": 2}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S7>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S7(new TestUnmarshalEmbeddedUnexported_embed1(1), new TestUnmarshalEmbeddedUnexported_embed2(2)))
    ), new(
        // Issue 24153, similar to the S7 case.
        CaseName: Name(""u8),
        @in: @"{""embed1"": {""Q"": 1}, ""embed2"": {""Q"": 2}, ""Q"": 3}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S8>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S8(new TestUnmarshalEmbeddedUnexported_embed1(1), new TestUnmarshalEmbeddedUnexported_embed2(2), 3))
    ), new(
        // Issue 228145, similar to the cases above.
        CaseName: Name(""u8),
        @in: @"{""embed"": {}}",
        ptr: @new<TestUnmarshalEmbeddedUnexported_S9>(),
        @out: Ꮡ(new TestUnmarshalEmbeddedUnexported_S9())
    )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestUnmarshalEmbeddedUnexported_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var err = Unmarshal(slice<byte>(ttʗ1.@in), ttʗ1.ptr);
            if (!equalError(err, ttʗ1.err)) {
                tΔ1.Errorf("%s: Unmarshal error:\n\tgot:  %v\n\twant: %v", ttʗ1.Where, err, ttʗ1.err);
            }
            if (!reflect.DeepEqual(ttʗ1.ptr, ttʗ1.@out)) {
                tΔ1.Errorf("%s: Unmarshal:\n\tgot:  %#+v\n\twant: %#+v", ttʗ1.Where, ttʗ1.ptr, ttʗ1.@out);
            }
        });
    }
}

internal partial struct TestUnmarshalErrorAfterMultipleJSON_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string @in;
    internal error err;
}

public static void TestUnmarshalErrorAfterMultipleJSON(ж<testing.T> t) {
    var tests = slice<TestUnmarshalErrorAfterMultipleJSON_tests>([new(
        CaseName: Name(""u8),
        @in: @"1 false null :",
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ':' looking for beginning of value", 14)))
    ), new(
        CaseName: Name(""u8),
        @in: @"1 [] [,]",
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ',' looking for beginning of value", 7)))
    ), new(
        CaseName: Name(""u8),
        @in: @"1 [] [true:]",
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character ':' after array element", 11)))
    ), new(
        CaseName: Name(""u8),
        @in: @"1  {}    {""x""=}",
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '=' after object key", 14)))
    ), new(
        CaseName: Name(""u8),
        @in: @"falsetruenul#",
        err: new global::go.encoding.json_package.SyntaxErrorжerror(Ꮡ(new SyntaxError("invalid character '#' in literal null (expecting 'l')", 13)))
    )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestUnmarshalErrorAfterMultipleJSON_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var dec = NewDecoder(new json_test_package.strings_ReaderжReader(strings.NewReader(ttʗ1.@in)));
            error err = default;
            while (err == nil) {
                ref var v = ref heap<any>(out var Ꮡv);
                err = dec.Decode(Ꮡv);
            }
            if (!reflect.DeepEqual(err, ttʗ1.err)) {
                tΔ1.Errorf("%s: Decode error:\n\tgot:  %v\n\twant: %v", ttʗ1.Where, err, ttʗ1.err);
            }
        });
    }
}

internal partial struct unmarshalPanic {
}

internal static error UnmarshalJSON(this unmarshalPanic _Δp0, slice<byte> _Δp1) { throw panic((nint)0xdead); }

public static void TestUnmarshalPanic(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var got = recover(); if (!reflect.DeepEqual(got, (nint)0xdead)) {
                t.Errorf("panic() = (%T)(%v), want 0xdead", got, got);
            }
        }, ref ᒐ);
        Unmarshal(slice<byte>("{}"u8), Ꮡ(new unmarshalPanic()));
        t.Fatalf("Unmarshal should have panicked");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// The decoder used to hang if decoding into an interface pointing to its own address.
// See golang.org/issues/31740.
public static void TestUnmarshalRecursivePointer(ж<testing.T> t) {
    ref var v = ref heap<any>(out var Ꮡv);
    v = Ꮡv;
    var data = slice<byte>(@"{""a"": ""b""}"u8);

    var err = Unmarshal(data, v); if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }
}

internal partial struct textUnmarshalerString /*@string*/;

internal static error UnmarshalText(this ref textUnmarshalerString m, slice<byte> text) {
    m = (textUnmarshalerString)strings.ToLower((@string)text);
    return default;
}

// Test unmarshal to a map, where the map key is a user defined type.
// See golang.org/issues/34437.
public static void TestUnmarshalMapWithTextUnmarshalerStringKey(ж<testing.T> t) {
    ref var p = ref heap<map<textUnmarshalerString, @string>>(out var Ꮡp);
    var err = Unmarshal(slice<byte>(@"{""FOO"": ""1""}"u8), Ꮡp); if (err != nil) {
        t.Fatalf("Unmarshal error: %v", err);
    }

    var (_, ok) = p[fooˢ, ꟷ]; if (!ok) {
        t.Errorf(@"key ""foo"" missing in map: %v", p);
    }
}

// See golang.org/issues/38126.
internal partial struct TestUnmarshalRescanLiteralMangledUnquote_T /*dyn*/ {
    public @string F1; /*`json:"F1,string"`*/
}

public static void TestUnmarshalRescanLiteralMangledUnquote(ж<testing.T> t) {
    // See golang.org/issues/38105.
    ref var p = ref heap<map<textUnmarshalerString, @string>>(out var Ꮡp);
    var errΔ1 = Unmarshal(slice<byte>(@"{""开源"":""12345开源""}"u8), Ꮡp); if (errΔ1 != nil) {
        t.Fatalf("Unmarshal error: %v", errΔ1);
    }
    var (_, ok) = p[(@string)"开源", ꟷ]; if (!ok) {
        t.Errorf(@"key ""开源"" missing in map: %v", p);
    }
    var wantT = new TestUnmarshalRescanLiteralMangledUnquote_T("aaa\tbbb");

    var (b, err) = Marshal(wantT);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    ref var gotT = ref heap(new TestUnmarshalRescanLiteralMangledUnquote_T(), out var ᏑgotT);
    var errΔ2 = Unmarshal(b, ᏑgotT); if (errΔ2 != nil) {
        t.Fatalf("Unmarshal error: %v", errΔ2);
    }
    if (gotT != wantT) {
        t.Errorf("Marshal/Unmarshal roundtrip:\n\tgot:  %q\n\twant: %q", gotT, wantT);
    }

    // See golang.org/issues/39555.
    var input = new map<textUnmarshalerString, @string>{[(@string)"FOO"] = "", [(@string)@""""] = ""};

    (var encoded, err) = Marshal(input);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    ref var got = ref heap<map<textUnmarshalerString, @string>>(out var Ꮡgot);
    var errΔ3 = Unmarshal(encoded, Ꮡgot); if (errΔ3 != nil) {
        t.Fatalf("Unmarshal error: %v", errΔ3);
    }
    var want = new map<textUnmarshalerString, @string>{[(@string)"foo"] = "", [(@string)@""""] = ""};
    if (!maps.Equal<map<textUnmarshalerString, @string>, map<textUnmarshalerString, @string>, textUnmarshalerString, @string>(got, want)) {
        t.Errorf("Marshal/Unmarshal roundtrip:\n\tgot:  %q\n\twant: %q", gotT, wantT);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string arrayOverMaxNestingDepthˢ => ref arrayOverMaxNestingDepthˢᵇ;
private static readonly @string arrayOverMaxNestingDepthˢᵇ = "ArrayOverMaxNestingDepth"u8;
internal static ref readonly @string arrayOverStackDepthˢ => ref arrayOverStackDepthˢᵇ;
private static readonly @string arrayOverStackDepthˢᵇ = "ArrayOverStackDepth"u8;
internal static ref readonly @string objectOverStackDepthˢ => ref objectOverStackDepthˢᵇ;
private static readonly @string objectOverStackDepthˢᵇ = "ObjectOverStackDepth"u8;
internal static ref readonly @string unstructuredˢ => ref unstructuredˢᵇ;
private static readonly @string unstructuredˢᵇ = "unstructured"u8;
internal static ref readonly @string typedNamedFieldˢ => ref typedNamedFieldˢᵇ;
private static readonly @string typedNamedFieldˢᵇ = "typed named field"u8;
internal static ref readonly @string typedMissingFieldˢ => ref typedMissingFieldˢᵇ;
private static readonly @string typedMissingFieldˢᵇ = "typed missing field"u8;
internal static ref readonly @string customUnmarshalerˢ => ref customUnmarshalerˢᵇ;
private static readonly @string customUnmarshalerˢᵇ = "custom unmarshaler"u8;

internal partial struct TestUnmarshalMaxDepth_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal @string data;
    internal bool errMaxDepth;
}

internal partial struct TestUnmarshalMaxDepth_targets /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal Func<any> newValue;
}

internal partial struct TestUnmarshalMaxDepth_v /*dyn*/ {
    public any A; /*`json:"a"`*/
}

internal partial struct TestUnmarshalMaxDepth_vᴛ1 /*dyn*/ {
    public any B; /*`json:"b"`*/
}

public static void TestUnmarshalMaxDepth(ж<testing.T> t) {
    var tests = slice<TestUnmarshalMaxDepth_tests>([new(
        CaseName: Name("ArrayUnderMaxNestingDepth"),
        data: @"{""a"":"u8 + strings.Repeat(@"[", 10000 - 1) + strings.Repeat(@"]", 10000 - 1) + @"}"u8,
        errMaxDepth: false
    ), new(
        CaseName: Name(arrayOverMaxNestingDepthˢ),
        data: @"{""a"":"u8 + strings.Repeat(@"[", 10000) + strings.Repeat(@"]", 10000) + @"}"u8,
        errMaxDepth: true
    ), new(
        CaseName: Name(arrayOverStackDepthˢ),
        data: @"{""a"":"u8 + strings.Repeat(@"[", 3000000) + strings.Repeat(@"]", 3000000) + @"}"u8,
        errMaxDepth: true
    ), new(
        CaseName: Name("ObjectUnderMaxNestingDepth"),
        data: @"{""a"":"u8 + strings.Repeat(@"{""a"":", 10000 - 1) + @"0"u8 + strings.Repeat(@"}", 10000 - 1) + @"}"u8,
        errMaxDepth: false
    ), new(
        CaseName: Name("ObjectOverMaxNestingDepth"),
        data: @"{""a"":"u8 + strings.Repeat(@"{""a"":", 10000) + @"0"u8 + strings.Repeat(@"}", 10000) + @"}"u8,
        errMaxDepth: true
    ), new(
        CaseName: Name(objectOverStackDepthˢ),
        data: @"{""a"":"u8 + strings.Repeat(@"{""a"":", 3000000) + @"0"u8 + strings.Repeat(@"}", 3000000) + @"}"u8,
        errMaxDepth: true
    )
    ]);

    var targets = slice<TestUnmarshalMaxDepth_targets>([new(
        CaseName: Name(unstructuredˢ),
        newValue: () => {
            ref var v = ref heap<any>(out var Ꮡv);
            return Ꮡv;
        }
    ), new(
        CaseName: Name(typedNamedFieldˢ),
        newValue: () => {
            ref var v = ref heap<TestUnmarshalMaxDepth_v>(new TestUnmarshalMaxDepth_v(), out var Ꮡv);
            return Ꮡv;
        }
    ), new(
        CaseName: Name(typedMissingFieldˢ),
        newValue: () => {
            ref var v = ref heap<TestUnmarshalMaxDepth_vᴛ1>(new TestUnmarshalMaxDepth_vᴛ1(), out var Ꮡv);
            return Ꮡv;
        }
    ), new(
        CaseName: Name(customUnmarshalerˢ),
        newValue: () => {
            ref var v = ref heap<unmarshaler>(new unmarshaler(), out var Ꮡv);
            return Ꮡv;
        }
    )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestUnmarshalMaxDepth_tests(), out var Ꮡtt);
        tt = vᴛ1;

        foreach (var (_, vᴛ2) in range(targets)) {
            ref var target = ref heap(new TestUnmarshalMaxDepth_targets(), out var Ꮡtarget);
            target = vᴛ2;

            var targetʗ1 = target;
            var ttʗ1 = tt;
            t.Run(target.Name + "-"u8 + tt.Name, (ж<testing.T> tΔ1) => {
                var err = Unmarshal(slice<byte>(ttʗ1.data), targetʗ1.newValue());
                if (!ttʗ1.errMaxDepth) {
                    if (err != nil) {
                        tΔ1.Errorf("%s: %s: Unmarshal error: %v", ttʗ1.Where, targetʗ1.Where, err);
                    }
                } else {
                    if (err == nil || !strings.Contains(err.Error(), exceededMaxDepthˢ)) {
                        tΔ1.Errorf("%s: %s: Unmarshal error:\n\tgot:  %v\n\twant: exceeded max depth", ttʗ1.Where, targetʗ1.Where, err);
                    }
                }
            });
        }
    }
}

} // end json_internal_test_package
