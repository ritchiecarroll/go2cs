// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains tests of the GobEncoder/GobDecoder support.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using net = net_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using static go.encoding.gob_package;

partial class gob_internal_test_package {

// Types that implement the GobEncoder/Decoder interfaces.
public partial struct ByteStruct {
    internal byte a; // not an exported field
}

public partial struct StringStruct {
    internal @string s; // not an exported field
}

public partial struct ArrayStruct {
    internal array<byte> a = new(8192); // not an exported field
}

public partial struct Gobber /*num:nint*/;

public partial struct ValueGobber /*@string*/;

public partial struct BinaryGobber /*num:nint*/;

public partial struct BinaryValueGobber /*@string*/;

public partial struct TextGobber /*num:nint*/;

public partial struct TextValueGobber /*@string*/;

// The relevant methods
public static (slice<byte>, error) GobEncode(this ref ByteStruct g) {
    var b = new slice<byte>(3);
    b[0] = g.a;
    b[1] = (byte)(g.a + 1);
    b[2] = (byte)(g.a + 2);
    return (b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilReceiverˢ = "NIL RECEIVER"u8;
internal static readonly @string invalidDataSequenceˢ = "invalid data sequence"u8;

public static error GobDecode(this ж<ByteStruct> Ꮡg, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (Ꮡg == nil) {
        return errors.New(nilReceiverˢ);
    }
    // Expect N sequential-valued bytes.
    if (len(data) == 0) {
        return io.EOF;
    }
    g.a = data[0];
    foreach (var (i, c) in data) {
        if (c != (byte)(g.a + (byte)i)) {
            return errors.New(invalidDataSequenceˢ);
        }
    }
    return default!;
}

public static (slice<byte>, error) GobEncode(this ref StringStruct g) {
    return (slice<byte>(g.s), default!);
}

public static error GobDecode(this ref StringStruct g, slice<byte> data) {
    // Expect N sequential-valued bytes.
    if (len(data) == 0) {
        return io.EOF;
    }
    var a = data[0];
    foreach (var (i, c) in data) {
        if (c != (byte)(a + (byte)i)) {
            return errors.New(invalidDataSequenceˢ);
        }
    }
    g.s = ((@string)data);
    return default!;
}

public static (slice<byte>, error) GobEncode(this ref ArrayStruct a) {
    return (a.a[..], default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wrongLengthInArrayDecodeˢ = "wrong length in array decode"u8;

public static error GobDecode(this ref ArrayStruct a, slice<byte> data) {
    if (len(data) != len(a.a)) {
        return errors.New(wrongLengthInArrayDecodeˢ);
    }
    copy(a.a[..], data);
    return default!;
}

public static (slice<byte>, error) GobEncode(this ref Gobber g) {
    return (slice<byte>(fmt.Sprintf("VALUE=%d"u8, g)), default!);
}

public static error GobDecode(this ж<Gobber> Ꮡg, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%d"u8, Ꮡg.Reinterpret<Gobber, nint>().OrTypedNil());
    return err;
}

public static (slice<byte>, error) MarshalBinary(this ref BinaryGobber g) {
    return (slice<byte>(fmt.Sprintf("VALUE=%d"u8, g)), default!);
}

public static error UnmarshalBinary(this ж<BinaryGobber> Ꮡg, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%d"u8, Ꮡg.Reinterpret<BinaryGobber, nint>().OrTypedNil());
    return err;
}

public static (slice<byte>, error) MarshalText(this ref TextGobber g) {
    return (slice<byte>(fmt.Sprintf("VALUE=%d"u8, g)), default!);
}

public static error UnmarshalText(this ж<TextGobber> Ꮡg, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%d"u8, Ꮡg.Reinterpret<TextGobber, nint>().OrTypedNil());
    return err;
}

public static (slice<byte>, error) GobEncode(this ValueGobber v) {
    return (slice<byte>(fmt.Sprintf("VALUE=%s"u8, v)), default!);
}

public static error GobDecode(this ж<ValueGobber> Ꮡv, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%s"u8, Ꮡv.Reinterpret<ValueGobber, @string>().OrTypedNil());
    return err;
}

public static (slice<byte>, error) MarshalBinary(this BinaryValueGobber v) {
    return (slice<byte>(fmt.Sprintf("VALUE=%s"u8, v)), default!);
}

public static error UnmarshalBinary(this ж<BinaryValueGobber> Ꮡv, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%s"u8, Ꮡv.Reinterpret<BinaryValueGobber, @string>().OrTypedNil());
    return err;
}

public static (slice<byte>, error) MarshalText(this TextValueGobber v) {
    return (slice<byte>(fmt.Sprintf("VALUE=%s"u8, v)), default!);
}

public static error UnmarshalText(this ж<TextValueGobber> Ꮡv, slice<byte> data) {
    var (_, err) = fmt.Sscanf(((@string)data), "VALUE=%s"u8, Ꮡv.Reinterpret<TextValueGobber, @string>().OrTypedNil());
    return err;
}

// Structs that include GobEncodable fields.
public partial struct GobTest0 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ж<ByteStruct> G;
}

public partial struct GobTest1 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ж<StringStruct> G;
}

public partial struct GobTest2 {
    public nint X;   // guarantee we have  something in common with GobTest*
    public @string G; // not a GobEncoder - should give us errors
}

public partial struct GobTest3 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ж<Gobber> G;
    public ж<BinaryGobber> B;
    public ж<TextGobber> T;
}

public partial struct GobTest4 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ValueGobber V;
    public BinaryValueGobber BV;
    public TextValueGobber TV;
}

public partial struct GobTest5 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ж<ValueGobber> V;
    public ж<BinaryValueGobber> BV;
    public ж<TextValueGobber> TV;
}

public partial struct GobTest6 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ValueGobber V;
    public ж<ValueGobber> W;
    public BinaryValueGobber BV;
    public ж<BinaryValueGobber> BW;
    public TextValueGobber TV;
    public ж<TextValueGobber> TW;
}

public partial struct GobTest7 {
    public nint X; // guarantee we have  something in common with GobTest*
    public ж<ValueGobber> V;
    public ValueGobber W;
    public ж<BinaryValueGobber> BV;
    public BinaryValueGobber BW;
    public ж<TextValueGobber> TV;
    public TextValueGobber TW;
}

public partial struct GobTestIgnoreEncoder {
    public nint X; // guarantee we have  something in common with GobTest*
}

public partial struct GobTestValueEncDec {
    public nint X;         // guarantee we have  something in common with GobTest*
    public StringStruct G; // not a pointer.
}

public partial struct GobTestIndirectEncDec {
    public nint X;            // guarantee we have  something in common with GobTest*
    public ж<ж<ж<StringStruct>>> G; // indirections to the receiver.
}

public partial struct GobTestArrayEncDec {
    public nint X;        // guarantee we have  something in common with GobTest*
    public ArrayStruct A; // not a pointer.
}

public partial struct GobTestIndirectArrayEncDec {
    public nint X;           // guarantee we have  something in common with GobTest*
    public ж<ж<ж<ArrayStruct>>> A; // indirections to a large receiver.
}

public static void TestGobEncoderField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // First a field that's a structure.
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(new GobTest0(17, Ꮡ(new ByteStruct((rune)'A'))));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTest0>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~(~x).G).a != (rune)'A') {
        Ꮡt.Errorf("expected 'A' got %c"u8, (~(~x).G).a);
    }
    // Now a field that's not a structure.
    b.Reset();
    ref var gobber = ref heap<Gobber>(out var Ꮡgobber);
    gobber = ((Gobber)23);
    ref var bgobber = ref heap<BinaryGobber>(out var Ꮡbgobber);
    bgobber = ((BinaryGobber)24);
    ref var tgobber = ref heap<TextGobber>(out var Ꮡtgobber);
    tgobber = ((TextGobber)25);
    err = enc.Encode(new GobTest3(17, Ꮡgobber, Ꮡbgobber, Ꮡtgobber));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var y = @new<GobTest3>();
    err = dec.Decode(y.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~y).G.Value != 23 || (~y).B.Value != 24 || (~y).T.Value != 25) {
        Ꮡt.Errorf("expected '23 got %d"u8, (~y).G.Value);
    }
}

// Even though the field is a value, we can still take its address
// and should be able to call the methods.
public static void TestGobEncoderValueField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // First a field that's a structure.
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(Ꮡ(new GobTestValueEncDec(17, new StringStruct("HIJKL"u8))));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTestValueEncDec>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~x).G.s != "HIJKL"u8) {
        Ꮡt.Errorf("expected `HIJKL` got %s"u8, (~x).G.s);
    }
}

// GobEncode/Decode should work even if the value is
// more indirect than the receiver.
public static void TestGobEncoderIndirectField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // First a field that's a structure.
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    ref var s = ref heap<ж<StringStruct>>(out var Ꮡs);
    s = Ꮡ(new StringStruct("HIJKL"u8));
    ref var sp = ref heap<ж<ж<StringStruct>>>(out var Ꮡsp);
    sp = Ꮡs;
    var err = enc.Encode(new GobTestIndirectEncDec(17, Ꮡsp));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTestIndirectEncDec>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if (((((~x).G.ValueSlot).ValueSlot).Value).s != "HIJKL"u8) {
        Ꮡt.Errorf("expected `HIJKL` got %s"u8, ((((~x).G.ValueSlot).ValueSlot).Value).s);
    }
}

// Test with a large field with methods.
public static void TestGobEncoderArrayField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    ref var a = ref heap(new GobTestArrayEncDec(), out var Ꮡa);
    a.X = 17;
    foreach (var (i, _) in a.A.a) {
        a.A.a[i] = (byte)i;
    }
    var err = enc.Encode(Ꮡa);
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTestArrayEncDec>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    foreach (var (i, v) in (~x).A.a.ΔRangeSnapshot()) {
        if (v != (byte)i) {
            Ꮡt.Errorf("expected %x got %x"u8, (byte)i, v);
            break;
        }
    }
}

// Test an indirection to a large field with methods.
public static void TestGobEncoderIndirectArrayField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    GobTestIndirectArrayEncDec a = default!;
    a.X = 17;
    ref var Δarray = ref heap(new ArrayStruct(), out var Ꮡarray);
    ref var ap = ref heap<ж<ArrayStruct>>(out var Ꮡap);
    ap = Ꮡarray;
    ref var app = ref heap<ж<ж<ArrayStruct>>>(out var Ꮡapp);
    app = Ꮡap;
    a.A = Ꮡapp;
    foreach (var (i, _) in Δarray.a) {
        Δarray.a[i] = (byte)i;
    }
    var err = enc.Encode(a);
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTestIndirectArrayEncDec>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    foreach (var (i, v) in ((((~x).A.ValueSlot).ValueSlot).Value).a.ΔRangeSnapshot()) {
        if (v != (byte)i) {
            Ꮡt.Errorf("expected %x got %x"u8, (byte)i, v);
            break;
        }
    }
}

// As long as the fields have the same name and implement the
// interface, we can cross-connect them. Not sure it's useful
// and may even be bad but it works and it's hard to prevent
// without exposing the contents of the object, which would
// defeat the purpose.
public static void TestGobEncoderFieldsOfDifferentType(ж<testing.T> Ꮡt) {
    // first, string in field to byte in field
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(new GobTest1(17, Ꮡ(new StringStruct("ABC"u8))));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTest0>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~(~x).G).a != (rune)'A') {
        Ꮡt.Errorf("expected 'A' got %c"u8, (~(~x).G).a);
    }
    // now the other direction, byte in field to string in field
    b.Reset();
    err = enc.Encode(new GobTest0(17, Ꮡ(new ByteStruct((rune)'X'))));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var y = @new<GobTest1>();
    err = dec.Decode(y.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~(~y).G).s != "XYZ"u8) {
        Ꮡt.Fatalf("expected `XYZ` got %q"u8, (~(~y).G).s);
    }
}

// Test that we can encode a value and decode into a pointer.
public static void TestGobEncoderValueEncoder(ж<testing.T> Ꮡt) {
    // first, string in field to byte in field
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(new GobTest4(17, ((ValueGobber)(@string)helloˢ), ((BinaryValueGobber)(@string)"Καλημέρα"u8), ((TextValueGobber)(@string)"こんにちは"u8)));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTest5>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~x).V.Value != "hello"u8 || (~x).BV.Value != "Καλημέρα"u8 || (~x).TV.Value != "こんにちは"u8) {
        Ꮡt.Errorf("expected `hello` got %s"u8, (~x).V.Value);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fortyTwoˢ = "forty-two"u8;
internal static readonly @string sixByNineˢ = "six-by-nine"u8;
internal static readonly @string secondsˢ = "πseconds"u8;
internal static readonly @string ftSˢ = "π²ft/s²"u8;

// Test that we can use a value then a pointer type of a GobEncoder
// in the same encoded value. Bug 4647.
public static void TestGobEncoderValueThenPointer(ж<testing.T> Ꮡt) {
    ValueGobber v = ((ValueGobber)(@string)fortyTwoˢ);
    ref var w = ref heap<ValueGobber>(out var Ꮡw);
    w = ((ValueGobber)(@string)sixByNineˢ);
    BinaryValueGobber bv = ((BinaryValueGobber)(@string)"1nanocentury"u8);
    ref var bw = ref heap<BinaryValueGobber>(out var Ꮡbw);
    bw = ((BinaryValueGobber)(@string)secondsˢ);
    TextValueGobber tv = ((TextValueGobber)(@string)"gravitationalacceleration"u8);
    ref var tw = ref heap<TextValueGobber>(out var Ꮡtw);
    tw = ((TextValueGobber)(@string)ftSˢ);
    // this was a bug: encoding a GobEncoder by value before a GobEncoder
    // pointer would cause duplicate type definitions to be sent.
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    {
        var err = enc.Encode(new GobTest6(42, v, Ꮡw, bv, Ꮡbw, tv, Ꮡtw)); if (err != default!) {
            Ꮡt.Fatal(encodeErrorˢ, err);
        }
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTest6>();
    {
        var err = dec.Decode(x.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(decodeErrorˢ, err);
        }
    }
    {
        ValueGobber got = x.Value.V;
        ValueGobber want = v; if (got != want) {
            Ꮡt.Errorf("v = %q, want %q"u8, got, want);
        }
    }
    {
        var got = x.Value.W;
        ValueGobber want = w; if (got == nil){
            Ꮡt.Errorf("w = nil, want %q"u8, want);
        } else 
        if (got.Value != want) {
            Ꮡt.Errorf("w = %q, want %q"u8, got.Value, want);
        }
    }
    {
        BinaryValueGobber got = x.Value.BV;
        BinaryValueGobber want = bv; if (got != want) {
            Ꮡt.Errorf("bv = %q, want %q"u8, got, want);
        }
    }
    {
        var got = x.Value.BW;
        BinaryValueGobber want = bw; if (got == nil){
            Ꮡt.Errorf("bw = nil, want %q"u8, want);
        } else 
        if (got.Value != want) {
            Ꮡt.Errorf("bw = %q, want %q"u8, got.Value, want);
        }
    }
    {
        TextValueGobber got = x.Value.TV;
        TextValueGobber want = tv; if (got != want) {
            Ꮡt.Errorf("tv = %q, want %q"u8, got, want);
        }
    }
    {
        var got = x.Value.TW;
        TextValueGobber want = tw; if (got == nil){
            Ꮡt.Errorf("tw = nil, want %q"u8, want);
        } else 
        if (got.Value != want) {
            Ꮡt.Errorf("tw = %q, want %q"u8, got.Value, want);
        }
    }
}

// Test that we can use a pointer then a value type of a GobEncoder
// in the same encoded value.
public static void TestGobEncoderPointerThenValue(ж<testing.T> Ꮡt) {
    ref var v = ref heap<ValueGobber>(out var Ꮡv);
    v = ((ValueGobber)(@string)fortyTwoˢ);
    ValueGobber w = ((ValueGobber)(@string)sixByNineˢ);
    ref var bv = ref heap<BinaryValueGobber>(out var Ꮡbv);
    bv = ((BinaryValueGobber)(@string)"1nanocentury"u8);
    BinaryValueGobber bw = ((BinaryValueGobber)(@string)secondsˢ);
    ref var tv = ref heap<TextValueGobber>(out var Ꮡtv);
    tv = ((TextValueGobber)(@string)"gravitationalacceleration"u8);
    TextValueGobber tw = ((TextValueGobber)(@string)ftSˢ);
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    {
        var err = enc.Encode(new GobTest7(42, Ꮡv, w, Ꮡbv, bw, Ꮡtv, tw)); if (err != default!) {
            Ꮡt.Fatal(encodeErrorˢ, err);
        }
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTest7>();
    {
        var err = dec.Decode(x.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(decodeErrorˢ, err);
        }
    }
    {
        var got = x.Value.V;
        ValueGobber want = v; if (got == nil){
            Ꮡt.Errorf("v = nil, want %q"u8, want);
        } else 
        if (got.Value != want) {
            Ꮡt.Errorf("v = %q, want %q"u8, got.Value, want);
        }
    }
    {
        ValueGobber got = x.Value.W;
        ValueGobber want = w; if (got != want) {
            Ꮡt.Errorf("w = %q, want %q"u8, got, want);
        }
    }
    {
        var got = x.Value.BV;
        BinaryValueGobber want = bv; if (got == nil){
            Ꮡt.Errorf("bv = nil, want %q"u8, want);
        } else 
        if (got.Value != want) {
            Ꮡt.Errorf("bv = %q, want %q"u8, got.Value, want);
        }
    }
    {
        BinaryValueGobber got = x.Value.BW;
        BinaryValueGobber want = bw; if (got != want) {
            Ꮡt.Errorf("bw = %q, want %q"u8, got, want);
        }
    }
    {
        var got = x.Value.TV;
        TextValueGobber want = tv; if (got == nil){
            Ꮡt.Errorf("tv = nil, want %q"u8, want);
        } else 
        if (got.Value != want) {
            Ꮡt.Errorf("tv = %q, want %q"u8, got.Value, want);
        }
    }
    {
        TextValueGobber got = x.Value.TW;
        TextValueGobber want = tw; if (got != want) {
            Ꮡt.Errorf("tw = %q, want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedDecodeErrorForˢ = (@string)"expected decode error for mismatched fields (encoder to non-decoder)"u8;
internal static readonly object expectedTypeErrorGotˢ = (@string)"expected type error; got"u8;
internal static readonly object expectedDecodeErrorForˢ2 = (@string)"expected decode error for mismatched fields (non-encoder to decoder)"u8;

public static void TestGobEncoderFieldTypeError(ж<testing.T> Ꮡt) {
    // GobEncoder to non-decoder: error
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(new GobTest1(17, Ꮡ(new StringStruct("ABC"u8))));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = Ꮡ(new GobTest2(nil));
    err = dec.Decode(x.OrTypedNil());
    if (err == default!) {
        Ꮡt.Fatal(expectedDecodeErrorForˢ);
    }
    if (!strings.Contains(err.Error(), typeˢ)) {
        Ꮡt.Fatal(expectedTypeErrorGotˢ, err);
    }
    // Non-encoder to GobDecoder: error
    b.Reset();
    err = enc.Encode(new GobTest2(17, "ABC"u8));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var y = Ꮡ(new GobTest1(nil));
    err = dec.Decode(y.OrTypedNil());
    if (err == default!) {
        Ꮡt.Fatal(expectedDecodeErrorForˢ2);
    }
    if (!strings.Contains(err.Error(), typeˢ)) {
        Ꮡt.Fatal(expectedTypeErrorGotˢ, err);
    }
}

// Even though ByteStruct is a struct, it's treated as a singleton at the top level.
public static void TestGobEncoderStructSingleton(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(Ꮡ(new ByteStruct((rune)'A')));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<ByteStruct>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~x).a != (rune)'A') {
        Ꮡt.Errorf("expected 'A' got %c"u8, (~x).a);
    }
}

public static void TestGobEncoderNonStructSingleton(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    ref var g = ref heap(new Gobber(), out var Ꮡg);
    g = 1234;
    var err = enc.Encode(Ꮡg);
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var x = ref heap(new Gobber(), out var Ꮡx);
    err = dec.Decode(Ꮡx);
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if (x != 1234) {
        Ꮡt.Errorf("expected 1234 got %d"u8, x);
    }
}

public static void TestGobEncoderIgnoreStructField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // First a field that's a structure.
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(new GobTest0(17, Ꮡ(new ByteStruct((rune)'A'))));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTestIgnoreEncoder>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~x).X != 17) {
        Ꮡt.Errorf("expected 17 got %c"u8, (~x).X);
    }
}

public static void TestGobEncoderIgnoreNonStructField(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // First a field that's a structure.
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    ref var gobber = ref heap<Gobber>(out var Ꮡgobber);
    gobber = ((Gobber)23);
    ref var bgobber = ref heap<BinaryGobber>(out var Ꮡbgobber);
    bgobber = ((BinaryGobber)24);
    ref var tgobber = ref heap<TextGobber>(out var Ꮡtgobber);
    tgobber = ((TextGobber)25);
    var err = enc.Encode(new GobTest3(17, Ꮡgobber, Ꮡbgobber, Ꮡtgobber));
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTestIgnoreEncoder>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~x).X != 17) {
        Ꮡt.Errorf("expected 17 got %c"u8, (~x).X);
    }
}

public static void TestGobEncoderIgnoreNilEncoder(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // First a field that's a structure.
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(new GobTest0(X: 18)); // G is nil
    if (err != default!) {
        Ꮡt.Fatal(encodeErrorˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var x = @new<GobTest0>();
    err = dec.Decode(x.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(decodeErrorˢ, err);
    }
    if ((~x).X != 18) {
        Ꮡt.Errorf("expected x.X = 18, got %v"u8, (~x).X);
    }
    if ((~x).G != nil) {
        Ꮡt.Errorf("expected x.G = nil, got %v"u8, (~x).G.OrTypedNil());
    }
}

internal partial struct gobDecoderBug0 {
    internal @string foo, bar;
}

internal static @string String(this ref gobDecoderBug0 br) {
    return br.foo + "-"u8 + br.bar;
}

internal static (slice<byte>, error) GobEncode(this ref gobDecoderBug0 br) {
    return (slice<byte>(br.String()), default!);
}

internal static error GobDecode(this ref gobDecoderBug0 br, slice<byte> b) {
    br.foo = fooˢ;
    br.bar = barˢ;
    return default!;
}

// This was a bug: the receiver has a different indirection level
// than the variable.
public static void TestGobEncoderExtraIndirect(ж<testing.T> Ꮡt) {
    var gdb = Ꮡ(new gobDecoderBug0("foo"u8, "bar"u8));
    var buf = @new<bytes.Buffer>();
    var e = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf));
    {
        var err = e.Encode(gdb.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("encode: %v"u8, err);
        }
    }
    var d = NewDecoder(new gob_test_package.bytes_BufferжReader(buf));
    ref var got = ref heap<ж<gobDecoderBug0>>(out var Ꮡgot);
    {
        var err = d.Decode(Ꮡgot); if (err != default!) {
            Ꮡt.Fatalf("decode: %v"u8, err);
        }
    }
    if ((~got).foo != (~gdb).foo || (~got).bar != (~gdb).bar) {
        Ꮡt.Errorf("got = %q, want %q"u8, got.OrTypedNil(), gdb.OrTypedNil());
    }
}

// Another bug: this caused a crash with the new Go1 Time type.
// We throw in a gob-encoding array, to test another case of isZero,
// and a struct containing a nil interface, to test a third.
internal partial struct isZeroBug {
    public time.Time T;
    public @string S;
    public nint I;
    public isZeroBugArray A;
    public isZeroBugInterface F;
}

public partial struct isZeroBugArray /*[2]uint8*/;

// Receiver is value, not pointer, to test isZero of array.
public static (slice<byte> b, error e) GobEncode(this isZeroBugArray a) {
    slice<byte> b = default!;

    a = a.Clone();
    b = appendꓸꓸꓸ(b, a[..]);
    return (b, default!);
}

public static error GobDecode(this ж<isZeroBugArray> Ꮡa, slice<byte> data) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (len(data) != 2) {
        return io.EOF;
    }
    a.Value[0] = data[0];
    a.Value[1] = data[1];
    return default!;
}

public partial struct isZeroBugInterface {
    public any I;
}

public static (slice<byte> b, error e) GobEncode(this isZeroBugInterface i) {
    return (new byte[]{}.slice(), default!);
}

public static error GobDecode(this ref isZeroBugInterface i, slice<byte> data) {
    return default!;
}

public static void TestGobEncodeIsZero(ж<testing.T> Ꮡt) {
    var x = new isZeroBug(time.Unix(1000000000, 0), "hello"u8, -55, new isZeroBugArray(new uint8[]{1, 2}.array()), new isZeroBugInterface(nil));
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(x);
    if (err != default!) {
        Ꮡt.Fatal(encodeˢ, err);
    }
    ref var y = ref heap(new isZeroBug(), out var Ꮡy);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    err = dec.Decode(Ꮡy);
    if (err != default!) {
        Ꮡt.Fatal(decodeˢ, err);
    }
    if (x != y) {
        Ꮡt.Fatalf("%v != %v"u8, x, y);
    }
}

public static void TestGobEncodePtrError(ж<testing.T> Ꮡt) {
    ref var err = ref heap<error>(out var Ꮡerr);
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    err = enc.Encode(Ꮡerr);
    if (err != default!) {
        Ꮡt.Fatal(encodeˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var err2 = ref heap<error>(out var Ꮡerr2);
    err2 = fmt.Errorf("foo"u8);
    err = dec.Decode(Ꮡerr2);
    if (err != default!) {
        Ꮡt.Fatal(decodeˢ, err);
    }
    if (err2 != default!) {
        Ꮡt.Fatalf("expected nil, got %v"u8, err2);
    }
}

public static void TestNetIP(ж<testing.T> Ꮡt) {
    // Encoding of net.IP{1,2,3,4} in Go 1.1.
    var enc = new byte[]{0x07, 0x0a, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04}.slice();
    ref var ip = ref heap<net.IP>(out var Ꮡip);
    var err = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(enc))).Decode(Ꮡip);
    if (err != default!) {
        Ꮡt.Fatalf("decode: %v"u8, err);
    }
    if (ip.String() != "1.2.3.4"u8) {
        Ꮡt.Errorf("decoded to %v, want 1.2.3.4"u8, ip.String());
    }
}

internal partial struct TestIgnoreDepthLimit_output /*dyn*/ {
    public nint Hello;
}

public static void TestIgnoreDepthLimit(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // We don't test the actual depth limit because it requires building an
        // extremely large message, which takes quite a while.
        nint oldNestingDepth = maxIgnoreNestingDepth;
        maxIgnoreNestingDepth = 100;
        defer(() => {
            maxIgnoreNestingDepth = oldNestingDepth;
        }, ref ᒐ);
        var b = @new<bytes.Buffer>();
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
        // Nested slice
        var typ = reflect.TypeFor<nint>();
        var nested = reflect.ArrayOf(1, typ);
        for (nint i = 0; i < 100; i++) {
            nested = reflect.ArrayOf(1, nested);
        }
        var badStruct = reflect.New(reflect.StructOf(new reflect.StructField[]{new(Name: "F"u8, Type: nested)}.slice()));
        enc.Encode(badStruct.Interface());
        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
        ref var output = ref heap(new TestIgnoreDepthLimit_output(), out var Ꮡoutput);
        @string expectedErr = invalidNestingDepthˢ;
        {
            var err = dec.Decode(Ꮡoutput); if (err == default! || err.Error() != expectedErr) {
                Ꮡt.Errorf("Decode didn't fail with depth limit of 100: want %q, got %q"u8, expectedErr, err);
            }
        }
        // Nested struct
        nested = reflect.StructOf(new reflect.StructField[]{new(Name: "F"u8, Type: typ)}.slice());
        for (nint i = 0; i < 100; i++) {
            nested = reflect.StructOf(new reflect.StructField[]{new(Name: "F"u8, Type: nested)}.slice());
        }
        badStruct = reflect.New(reflect.StructOf(new reflect.StructField[]{new(Name: "F"u8, Type: nested)}.slice()));
        enc.Encode(badStruct.Interface());
        dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
        {
            var err = dec.Decode(Ꮡoutput); if (err == default! || err.Error() != expectedErr) {
                Ꮡt.Errorf("Decode didn't fail with depth limit of 100: want %q, got %q"u8, expectedErr, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end gob_internal_test_package
