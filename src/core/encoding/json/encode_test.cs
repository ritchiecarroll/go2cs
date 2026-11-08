// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using bytes = bytes_package;
using encoding = encoding_package;
using fmt = fmt_package;
using log = log_package;
using math = math_package;
using reflect = reflect_package;
using regexp = regexp_package;
using debug = go.runtime.debug_package;
using strconv = strconv_package;
using sync = sync_package;
using testing = testing_package;
using synctest = go.testing.synctest_package;
using time = time_package;
using go.runtime;
using go.testing;
using io = io_package;
using static go.encoding.json_package;
using ꓸꓸꓸany = Span<any>;

partial class json_internal_test_package {

public partial struct OptionalsEmpty {
    public @string Sr; /*`json:"sr"`*/
    public @string So; /*`json:"so,omitempty"`*/
    public @string Sw; /*`json:"-"`*/
    public nint Ir; /*`json:"omitempty"`*/ // actually named omitempty, not an option
    public nint Io; /*`json:"io,omitempty"`*/
    public slice<@string> Slr; /*`json:"slr,random"`*/
    public slice<@string> Slo; /*`json:"slo,omitempty"`*/
    public map<@string, any> Mr; /*`json:"mr"`*/
    public map<@string, any> Mo; /*`json:",omitempty"`*/
    public float64 Fr; /*`json:"fr"`*/
    public float64 Fo; /*`json:"fo,omitempty"`*/
    public bool Br; /*`json:"br"`*/
    public bool Bo; /*`json:"bo,omitempty"`*/
    public nuint Ur; /*`json:"ur"`*/
    public nuint Uo; /*`json:"uo,omitempty"`*/
    public EmptyStruct Str; /*`json:"str"`*/
    public EmptyStruct Sto; /*`json:"sto,omitempty"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string somethingˢ => ref somethingˢᵇ;
private static readonly @string somethingˢᵇ = "something"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = """
{
 "sr": "",
 "omitempty": 0,
 "slr": null,
 "mr": {},
 "fr": 0,
 "br": false,
 "ur": 0,
 "str": {},
 "sto": {}
}
"""u8.Raw();

public static void TestOmitEmpty(ж<testing.T> t) {
    @string want = wantᶜ1;
    ref var o = ref heap(new OptionalsEmpty(), out var Ꮡo);
    o.Sw = somethingˢ;
    o.Mr = new map<@string, any>{};
    o.Mo = new map<@string, any>{};

    var (got, err) = MarshalIndent(Ꮡo, ""u8, " ");
    if (err != nil) {
        t.Fatalf("MarshalIndent error: %v", err);
    }
    @string gotΔ1 = (@string)got; if (gotΔ1 != want) {
        t.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n", indentNewlines(gotΔ1), indentNewlines(want));
    }
}

public partial struct NonZeroStruct {
}

public static bool IsZero(this NonZeroStruct nzs) {
    return false;
}

public partial struct NoPanicStruct {
    public nint Int; /*`json:"int,omitzero"`*/
}

public static bool IsZero(this ref NoPanicStruct nps) {
    return nps.Int != 0;
}

public partial struct OptionalsZero {
    public @string Sr; /*`json:"sr"`*/
    public @string So; /*`json:"so,omitzero"`*/
    public @string Sw; /*`json:"-"`*/
    public nint Ir; /*`json:"omitzero"`*/ // actually named omitzero, not an option
    public nint Io; /*`json:"io,omitzero"`*/
    public slice<@string> Slr; /*`json:"slr,random"`*/
    public slice<@string> Slo; /*`json:"slo,omitzero"`*/
    public slice<@string> SloNonNil; /*`json:"slononnil,omitzero"`*/
    public map<@string, any> Mr; /*`json:"mr"`*/
    public map<@string, any> Mo; /*`json:",omitzero"`*/
    public map<@string, any> Moo; /*`json:"moo,omitzero"`*/
    public float64 Fr; /*`json:"fr"`*/
    public float64 Fo; /*`json:"fo,omitzero"`*/
    public float64 Foo; /*`json:"foo,omitzero"`*/
    public array<float64> Foo2 = new(2); /*`json:"foo2,omitzero"`*/
    public bool Br; /*`json:"br"`*/
    public bool Bo; /*`json:"bo,omitzero"`*/
    public nuint Ur; /*`json:"ur"`*/
    public nuint Uo; /*`json:"uo,omitzero"`*/
    public EmptyStruct Str; /*`json:"str"`*/
    public EmptyStruct Sto; /*`json:"sto,omitzero"`*/
    public time.Time Time; /*`json:"time,omitzero"`*/
    public time.Time TimeLocal; /*`json:"timelocal,omitzero"`*/
    public NonZeroStruct Nzs; /*`json:"nzs,omitzero"`*/
    public global::go.encoding.json_package.isZeroer NilIsZeroer; /*`json:"niliszeroer,omitzero"`*/       // nil interface
    public global::go.encoding.json_package.isZeroer NonNilIsZeroer; /*`json:"nonniliszeroer,omitzero"`*/ // non-nil interface
    public global::go.encoding.json_package.isZeroer NoPanicStruct0; /*`json:"nps0,omitzero"`*/           // non-nil interface with nil pointer
    public global::go.encoding.json_package.isZeroer NoPanicStruct1; /*`json:"nps1,omitzero"`*/           // non-nil interface with non-nil pointer
    public ж<NoPanicStruct> NoPanicStruct2; /*`json:"nps2,omitzero"`*/                                    // nil pointer
    public ж<NoPanicStruct> NoPanicStruct3; /*`json:"nps3,omitzero"`*/                                    // non-nil pointer
    public NoPanicStruct NoPanicStruct4; /*`json:"nps4,omitzero"`*/                                       // concrete type
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ2 => ref wantᶜ2ᵇ;
private static readonly @string wantᶜ2ᵇ = """
{
 "sr": "",
 "omitzero": 0,
 "slr": null,
 "slononnil": [],
 "mr": {},
 "Mo": {},
 "fr": 0,
 "br": false,
 "ur": 0,
 "str": {},
 "nzs": {},
 "nps1": {},
 "nps3": {},
 "nps4": {}
}
"""u8.Raw();

public static void TestOmitZero(ж<testing.T> t) {
    @string want = wantᶜ2;
    ref var o = ref heap(new OptionalsZero(), out var Ꮡo);
    o.Sw = somethingˢ;
    o.SloNonNil = new slice<@string>(0);
    o.Mr = new map<@string, any>{};
    o.Mo = new map<@string, any>{};

    o.Foo = -0D;
    o.Foo2 = new float64[]{+0D, -0D}.array();

    o.TimeLocal = new time.Time().Local();

    o.NonNilIsZeroer = new json_test_package.time_TimeᴠisZeroer(new time.Time());
    o.NoPanicStruct0 = new json_internal_test_package.NoPanicStructжisZeroer(((ж<NoPanicStruct>)nil));
    o.NoPanicStruct1 = new json_internal_test_package.NoPanicStructжisZeroer(Ꮡ(new NoPanicStruct()));
    o.NoPanicStruct3 = Ꮡ(new NoPanicStruct());

    var (got, err) = MarshalIndent(Ꮡo, ""u8, " ");
    if (err != nil) {
        t.Fatalf("MarshalIndent error: %v", err);
    }
    @string gotΔ1 = (@string)got; if (gotΔ1 != want) {
        t.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n", indentNewlines(gotΔ1), indentNewlines(want));
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ3 => ref wantᶜ3ᵇ;
private static readonly @string wantᶜ3ᵇ = """
{
 "foo": {
  "sr": "",
  "omitzero": 0,
  "slr": null,
  "mr": null,
  "fr": 0,
  "br": false,
  "ur": 0,
  "str": {},
  "nzs": {},
  "nps4": {}
 }
}
"""u8.Raw();

public static void TestOmitZeroMap(ж<testing.T> t) {
    @string want = wantᶜ3;
    var m = new map<@string, OptionalsZero>{["foo"] = new()};
    var (got, err) = MarshalIndent(m, ""u8, " ");
    if (err != nil) {
        t.Fatalf("MarshalIndent error: %v", err);
    }
    @string gotΔ1 = (@string)got; if (gotΔ1 != want) {
        fmt.Println(gotΔ1);
        t.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n", indentNewlines(gotΔ1), indentNewlines(want));
    }
}

public partial struct OptionalsEmptyZero {
    public @string Sr; /*`json:"sr"`*/
    public @string So; /*`json:"so,omitempty,omitzero"`*/
    public @string Sw; /*`json:"-"`*/
    public nint Io; /*`json:"io,omitempty,omitzero"`*/
    public slice<@string> Slr; /*`json:"slr,random"`*/
    public slice<@string> Slo; /*`json:"slo,omitempty,omitzero"`*/
    public slice<@string> SloNonNil; /*`json:"slononnil,omitempty,omitzero"`*/
    public map<@string, any> Mr; /*`json:"mr"`*/
    public map<@string, any> Mo; /*`json:",omitempty,omitzero"`*/
    public float64 Fr; /*`json:"fr"`*/
    public float64 Fo; /*`json:"fo,omitempty,omitzero"`*/
    public bool Br; /*`json:"br"`*/
    public bool Bo; /*`json:"bo,omitempty,omitzero"`*/
    public nuint Ur; /*`json:"ur"`*/
    public nuint Uo; /*`json:"uo,omitempty,omitzero"`*/
    public EmptyStruct Str; /*`json:"str"`*/
    public EmptyStruct Sto; /*`json:"sto,omitempty,omitzero"`*/
    public time.Time Time; /*`json:"time,omitempty,omitzero"`*/
    public NonZeroStruct Nzs; /*`json:"nzs,omitempty,omitzero"`*/
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ4 => ref wantᶜ4ᵇ;
private static readonly @string wantᶜ4ᵇ = """
{
 "sr": "",
 "slr": null,
 "mr": {},
 "fr": 0,
 "br": false,
 "ur": 0,
 "str": {},
 "nzs": {}
}
"""u8.Raw();

public static void TestOmitEmptyZero(ж<testing.T> t) {
    @string want = wantᶜ4;
    ref var o = ref heap(new OptionalsEmptyZero(), out var Ꮡo);
    o.Sw = somethingˢ;
    o.SloNonNil = new slice<@string>(0);
    o.Mr = new map<@string, any>{};
    o.Mo = new map<@string, any>{};

    var (got, err) = MarshalIndent(Ꮡo, ""u8, " ");
    if (err != nil) {
        t.Fatalf("MarshalIndent error: %v", err);
    }
    @string gotΔ1 = (@string)got; if (gotΔ1 != want) {
        t.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n", indentNewlines(gotΔ1), indentNewlines(want));
    }
}

public partial struct StringTag {
    public bool BoolStr; /*`json:",string"`*/
    public int64 IntStr; /*`json:",string"`*/
    public uintptr UintptrStr; /*`json:",string"`*/
    public @string StrStr; /*`json:",string"`*/
    public global::go.encoding.json_package.Number NumberStr; /*`json:",string"`*/
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string allTypesˢ => ref allTypesˢᵇ;
private static readonly @string allTypesˢᵇ = "AllTypes"u8;
internal static ref readonly @string stringDoubleEscapesˢ => ref stringDoubleEscapesˢᵇ;
private static readonly @string stringDoubleEscapesˢᵇ = "StringDoubleEscapes"u8;

internal partial struct TestRoundtripStringTag_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal StringTag @in;
    internal @string want; // empty to just test that we roundtrip
}

public static void TestRoundtripStringTag(ж<testing.T> t) {
    var tests = slice<TestRoundtripStringTag_tests>([new(
        CaseName: Name(allTypesˢ),
        @in: new StringTag(
            BoolStr: true,
            IntStr: 42,
            UintptrStr: 44,
            StrStr: "xzbit",
            NumberStr: (@string)"46"
        ),
        want: """
{
	"BoolStr": "true",
	"IntStr": "42",
	"UintptrStr": "44",
	"StrStr": "\"xzbit\"",
	"NumberStr": "46"
}
"""u8.Raw()
    ), new(
        // See golang.org/issues/38173.
        CaseName: Name(stringDoubleEscapesˢ),
        @in: new StringTag(
            StrStr: "\b\f\n\r\t\"\\",
            NumberStr: (@string)"0" // just to satisfy the roundtrip
        ),
        want: """
{
	"BoolStr": "false",
	"IntStr": "0",
	"UintptrStr": "0",
	"StrStr": "\"\\b\\f\\n\\r\\t\\\"\\\\\"",
	"NumberStr": "0"
}
"""u8.Raw()
    )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestRoundtripStringTag_tests(), out var Ꮡtt);
        tt = vᴛ1;

        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (got, err) = MarshalIndent(Ꮡtt.of(TestRoundtripStringTag_tests.Ꮡin), ""u8, "\t");
            if (err != nil) {
                tΔ1.Fatalf("%s: MarshalIndent error: %v", Ꮡtt.Value.Where, err);
            }
            @string gotΔ1 = (@string)got; if (gotΔ1 != Ꮡtt.Value.want) {
                tΔ1.Fatalf("%s: MarshalIndent:\n\tgot:  %s\n\twant: %s", Ꮡtt.Value.Where, stripWhitespace(gotΔ1), stripWhitespace(Ꮡtt.Value.want));
            }

            // Verify that it round-trips.
            ref var s2 = ref heap(new StringTag(), out var Ꮡs2);
            var errΔ1 = Unmarshal(got, Ꮡs2); if (errΔ1 != nil) {
                tΔ1.Fatalf("%s: Decode error: %v", Ꮡtt.Value.Where, errΔ1);
            }
            if (!reflect.DeepEqual(s2, Ꮡtt.Value.@in)) {
                tΔ1.Fatalf("%s: Decode:\n\tinput: %s\n\tgot:  %#v\n\twant: %#v", Ꮡtt.Value.Where, indentNewlines((@string)got), s2, Ꮡtt.Value.@in);
            }
        });
    }
}

// byte slices are special even if they're renamed types.
internal partial struct renamedByte /*num:byte*/;
internal partial struct renamedByteSlice /*[]byte*/;
internal partial struct renamedRenamedByteSlice /*[]renamedByte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ywJjˢ => ref ywJjˢᵇ;
private static readonly @string ywJjˢᵇ = @"""YWJj"""u8;

public static void TestEncodeRenamedByteSlice(ж<testing.T> t) {
    var s = (renamedByteSlice)slice<byte>((@string)"abc"u8);
    var (got, err) = Marshal(s);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string want = ywJjˢ;
    if (((sstring)got) != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
    var r = ((renamedRenamedByteSlice)widen<byte, renamedByte>(slice<byte>((@string)"abc"u8), elemᴛ0 => (renamedByte)elemᴛ0));
    (got, err) = Marshal(r);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    if (((sstring)got) != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

public partial struct SamePointerNoCycle {
    public ж<SamePointerNoCycle> Ptr1, Ptr2;
}

internal static ж<SamePointerNoCycle> samePointerNoCycle = Ꮡ(new SamePointerNoCycle());

public partial struct PointerCycle {
    public ж<PointerCycle> Ptr;
}

internal static ж<PointerCycle> pointerCycle = Ꮡ(new PointerCycle());

public partial struct PointerCycleIndirect {
    public slice<any> Ptrs;
}

public partial struct RecursiveSlice /*[]RecursiveSlice*/;

internal static ж<PointerCycleIndirect> pointerCycleIndirect = Ꮡ(new PointerCycleIndirect());
internal static map<@string, any> mapCycle = new map<@string, any>();
internal static slice<any> sliceCycle = [default];
internal static slice<any> sliceNoCycle = [default, default];
internal static slice<RecursiveSlice> recursiveSliceCycle = [default];

[GoInit] internal static void init() {
    var ptr = Ꮡ(new SamePointerNoCycle());
    samePointerNoCycle.Ptr1 = ptr;
    samePointerNoCycle.Ptr2 = ptr;

    pointerCycle.Ptr = pointerCycle;
    pointerCycleIndirect.Ptrs = [pointerCycleIndirect.OrTypedNil()];

    mapCycle["x"] = mapCycle;
    sliceCycle[0] = sliceCycle;
    sliceNoCycle[1] = sliceNoCycle[..1];
    for (nint i = startDetectingCyclesAfter; i > 0; i--) {
        sliceNoCycle = [sliceNoCycle];
    }
    recursiveSliceCycle[0] = recursiveSliceCycle;
}

public static partial void TestSamePointerNoCycle(ж<testing.T> t) {
    var (_, err) = Marshal(samePointerNoCycle.OrTypedNil()); if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
}

public static partial void TestSliceNoCycle(ж<testing.T> t) {
    var (_, err) = Marshal(sliceNoCycle); if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
}

internal partial struct TestUnsupportedValues_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any @in;
}

public static void TestUnsupportedValues(ж<testing.T> t) {
    var tests = slice<TestUnsupportedValues_tests>([
        new(Name(""u8), math.NaN()),
        new(Name(""u8), math.Inf(-1)),
        new(Name(""u8), math.Inf(1)),
        new(Name(""u8), pointerCycle.OrTypedNil()),
        new(Name(""u8), pointerCycleIndirect.OrTypedNil()),
        new(Name(""u8), mapCycle),
        new(Name(""u8), sliceCycle),
        new(Name(""u8), recursiveSliceCycle)
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestUnsupportedValues_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (_, err) = Marshal(ttʗ1.@in); if (err != nil) {
                var (_, ok) = err._<ж<global::go.encoding.json_package.UnsupportedValueError>>(ᐧ); if (!ok) {
                    tΔ1.Errorf("%s: Marshal error:\n\tgot:  %T\n\twant: %T", ttʗ1.Where, err, @new<global::go.encoding.json_package.UnsupportedValueError>());
                }
            } else {
                tΔ1.Errorf("%s: Marshal error: got nil, want non-nil", ttʗ1.Where);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tfNaN1TfNaN1ˢ => ref tfNaN1TfNaN1ˢᵇ;
private static readonly @string tfNaN1TfNaN1ˢᵇ = @"{""TF:NaN"":""1"",""TF:NaN"":""1""}"u8;

// Issue 43207
public static void TestMarshalTextFloatMap(ж<testing.T> t) {
    var m = new map<textfloat, @string>{
        [((textfloat)math.NaN())] = "1",
        [((textfloat)math.NaN())] = "1"
    };
    var (got, err) = Marshal(m);
    if (err != nil) {
        t.Errorf("Marshal error: %v", err);
    }
    @string want = tfNaN1TfNaN1ˢ;
    if (((sstring)got) != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

// Ref has Marshaler and Unmarshaler methods with pointer receiver.
public partial struct Ref /*num:nint*/;

public static (slice<byte>, error) MarshalJSON(this ref Ref _) {
    return (slice<byte>(@"""ref"""u8), default);
}

public static error UnmarshalJSON(this ref Ref r, slice<byte> _) {
    r = 12;
    return default;
}

// Val has Marshaler methods with value receiver.
public partial struct Val /*num:nint*/;

public static (slice<byte>, error) MarshalJSON(this Val _) {
    return (slice<byte>(@"""val"""u8), default);
}

// RefText has Marshaler and Unmarshaler methods with pointer receiver.
public partial struct RefText /*num:nint*/;

public static (slice<byte>, error) MarshalText(this ref RefText _) {
    return (slice<byte>(@"""ref"""u8), default);
}

public static error UnmarshalText(this ref RefText r, slice<byte> _) {
    r = 13;
    return default;
}

// ValText has Marshaler methods with value receiver.
public partial struct ValText /*num:nint*/;

public static (slice<byte>, error) MarshalText(this ValText _) {
    return (slice<byte>(@"""val"""u8), default);
}

internal partial struct TestRefValMarshal_type /*dyn*/ {
    public Ref R0;
    public ж<Ref> R1;
    public RefText R2;
    public ж<RefText> R3;
    public Val V0;
    public ж<Val> V1;
    public ValText V2;
    public ж<ValText> V3;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ5 => ref wantᶜ5ᵇ;
private static readonly @string wantᶜ5ᵇ = @"{""R0"":""ref"",""R1"":""ref"",""R2"":""\""ref\"""",""R3"":""\""ref\"""",""V0"":""val"",""V1"":""val"",""V2"":""\""val\"""",""V3"":""\""val\""""}"u8;

public static void TestRefValMarshal(ж<testing.T> t) {
    ref var s = ref heap(new TestRefValMarshal_type(), out var Ꮡs);

    s = new TestRefValMarshal_type(
        R0: 12,
        R1: @new<Ref>(),
        R2: 14,
        R3: @new<RefText>(),
        V0: 13,
        V1: @new<Val>(),
        V2: 15,
        V3: @new<ValText>()
    );
    @string want = wantᶜ5;
    var (b, err) = Marshal(Ꮡs);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string got = (@string)b; if (got != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

// C implements Marshaler and returns unescaped JSON.
public partial struct C /*num:nint*/;

public static (slice<byte>, error) MarshalJSON(this C _) {
    return (slice<byte>(@"""<&>"""u8), default);
}

// CText implements Marshaler and returns unescaped text.
public partial struct CText /*num:nint*/;

public static (slice<byte>, error) MarshalText(this CText _) {
    return (slice<byte>(@"""<&>"""u8), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string u003cU0026U003eˢ => ref u003cU0026U003eˢᵇ;
private static readonly @string u003cU0026U003eˢᵇ = @"""\u003c\u0026\u003e"""u8;
internal static ref readonly @string u003cU0026U003eˢ2 => ref u003cU0026U003eˢ2ᵇ;
private static readonly @string u003cU0026U003eˢ2ᵇ = @"""\""\u003c\u0026\u003e\"""""u8;

public static void TestMarshalerEscaping(ж<testing.T> t) {
    C c = default;
    @string want = u003cU0026U003eˢ;
    var (b, err) = Marshal(c);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    {
        @string got = (@string)b; if (got != want) {
            t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
        }
    }

    CText ct = default;
    want = u003cU0026U003eˢ2;
    (b, err) = Marshal(ct);
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    {
        @string got = (@string)b; if (got != want) {
            t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ambiguousFieldˢ => ref ambiguousFieldˢᵇ;
private static readonly @string ambiguousFieldˢᵇ = "AmbiguousField"u8;
internal static ref readonly @string dominantFieldˢ => ref dominantFieldˢᵇ;
private static readonly @string dominantFieldˢᵇ = "DominantField"u8;
internal static ref readonly @string unexportedEmbeddedIntˢ => ref unexportedEmbeddedIntˢᵇ;
private static readonly @string unexportedEmbeddedIntˢᵇ = "UnexportedEmbeddedInt"u8;
internal static ref readonly @string exportedEmbeddedIntˢ => ref exportedEmbeddedIntˢᵇ;
private static readonly @string exportedEmbeddedIntˢᵇ = "ExportedEmbeddedInt"u8;
internal static ref readonly @string embeddedStructˢ => ref embeddedStructˢᵇ;
private static readonly @string embeddedStructˢᵇ = "EmbeddedStruct"u8;
internal static ref readonly @string embeddedStructPointerˢ => ref embeddedStructPointerˢᵇ;
private static readonly @string embeddedStructPointerˢᵇ = "EmbeddedStructPointer"u8;
internal static ref readonly @string nestedStructAndIntsˢ => ref nestedStructAndIntsˢᵇ;
private static readonly @string nestedStructAndIntsˢᵇ = "NestedStructAndInts"u8;

internal partial struct TestAnonymousFields_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal Func<any> makeInput; // Function to create input value
    internal @string want;        // Expected JSON output
}

internal partial struct TestAnonymousFields_S1 /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_S2 /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_S /*dyn*/ {
    /*embed*/ public TestAnonymousFields_S1 S1;
    /*embed*/ public TestAnonymousFields_S2 S2;
}

internal partial struct TestAnonymousFields_S1ᴛ1 /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_S2ᴛ1 /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_Sᴛ1 /*dyn*/ {
    /*embed*/ public TestAnonymousFields_S1ᴛ1 S1;
    /*embed*/ public TestAnonymousFields_S2ᴛ1 S2;
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_myInt /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ2 /*dyn*/ {
    /*embed*/ internal TestAnonymousFields_myInt myInt;
}

internal partial struct TestAnonymousFields_MyInt /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ3 /*dyn*/ {
    /*embed*/ public TestAnonymousFields_MyInt MyInt;
}

internal partial struct TestAnonymousFields_myIntᴛ1 /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ4 /*dyn*/ {
    /*embed*/ internal ж<TestAnonymousFields_myIntᴛ1> myInt;
}

internal partial struct TestAnonymousFields_MyIntᴛ1 /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ5 /*dyn*/ {
    /*embed*/ public ж<TestAnonymousFields_MyIntᴛ1> MyInt;
}

internal partial struct TestAnonymousFields_s1 /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_S2ᴛ2 /*dyn*/ {
    internal nint y;
    public nint Y;
}

internal partial struct TestAnonymousFields_Sᴛ6 /*dyn*/ {
    /*embed*/ internal TestAnonymousFields_s1 s1;
    /*embed*/ public TestAnonymousFields_S2ᴛ2 S2;
}

internal partial struct TestAnonymousFields_s1ᴛ1 /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_S2ᴛ3 /*dyn*/ {
    internal nint y;
    public nint Y;
}

internal partial struct TestAnonymousFields_Sᴛ7 /*dyn*/ {
    /*embed*/ internal ж<TestAnonymousFields_s1ᴛ1> s1;
    /*embed*/ public ж<TestAnonymousFields_S2ᴛ3> S2;
}

internal partial struct TestAnonymousFields_MyInt1 /*num:nint*/;

internal partial struct TestAnonymousFields_MyInt2 /*num:nint*/;

internal partial struct TestAnonymousFields_myIntᴛ2 /*num:nint*/;

internal partial struct TestAnonymousFields_s2 /*dyn*/ {
    /*embed*/ public TestAnonymousFields_MyInt2 MyInt2;
    /*embed*/ internal TestAnonymousFields_myIntᴛ2 myInt;
}

internal partial struct TestAnonymousFields_s1ᴛ2 /*dyn*/ {
    /*embed*/ public TestAnonymousFields_MyInt1 MyInt1;
    /*embed*/ internal TestAnonymousFields_myIntᴛ2 myInt;
    /*embed*/ internal TestAnonymousFields_s2 s2;
}

internal partial struct TestAnonymousFields_Sᴛ8 /*dyn*/ {
    /*embed*/ internal TestAnonymousFields_s1ᴛ2 s1;
    /*embed*/ internal TestAnonymousFields_myIntᴛ2 myInt;
}

internal partial struct TestAnonymousFields_S2ᴛ4 /*dyn*/ {
    public @string Field;
}

internal partial struct TestAnonymousFields_Sᴛ9 /*dyn*/ {
    /*embed*/ public ж<TestAnonymousFields_S2ᴛ4> S2;
}

public static void TestAnonymousFields(ж<testing.T> t) {
    var tests = slice<TestAnonymousFields_tests>([new(
        // Both S1 and S2 have a field named X. From the perspective of S,
        // it is ambiguous which one X refers to.
        // This should not serialize either field.
        CaseName: Name(ambiguousFieldˢ),
        makeInput: () => {
            return new TestAnonymousFields_S(new TestAnonymousFields_S1(1, 2), new TestAnonymousFields_S2(3, 4));
        },
        want: @"{}"
    ), new(
        CaseName: Name(dominantFieldˢ),
        // Both S1 and S2 have a field named X, but since S has an X field as
        // well, it takes precedence over S1.X and S2.X.
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ1(new TestAnonymousFields_S1ᴛ1(1, 2), new TestAnonymousFields_S2ᴛ1(3, 4), 5, 6);
        },
        want: @"{""X"":6}"
    ), new(
        // Unexported embedded field of non-struct type should not be serialized.
        CaseName: Name(unexportedEmbeddedIntˢ),
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ2(5);
        },
        want: @"{}"
    ), new(
        // Exported embedded field of non-struct type should be serialized.
        CaseName: Name(exportedEmbeddedIntˢ),
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ3(5);
        },
        want: @"{""MyInt"":5}"
    ), new(
        // Unexported embedded field of pointer to non-struct type
        // should not be serialized.
        CaseName: Name("UnexportedEmbeddedIntPointer"),
        makeInput: () => {
            var s = new TestAnonymousFields_Sᴛ4(@new<TestAnonymousFields_myIntᴛ1>());
            s.myInt.Value = 5;
            return s;
        },
        want: @"{}"
    ), new(
        // Exported embedded field of pointer to non-struct type
        // should be serialized.
        CaseName: Name("ExportedEmbeddedIntPointer"),
        makeInput: () => {
            var s = new TestAnonymousFields_Sᴛ5(@new<TestAnonymousFields_MyIntᴛ1>());
            s.MyInt.Value = 5;
            return s;
        },
        want: @"{""MyInt"":5}"
    ), new(
        // Exported fields of embedded structs should have their
        // exported fields be serialized regardless of whether the struct types
        // themselves are exported.
        CaseName: Name(embeddedStructˢ),
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ6(new TestAnonymousFields_s1(1, 2), new TestAnonymousFields_S2ᴛ2(3, 4));
        },
        want: @"{""X"":2,""Y"":4}"
    ), new(
        // Exported fields of pointers to embedded structs should have their
        // exported fields be serialized regardless of whether the struct types
        // themselves are exported.
        CaseName: Name(embeddedStructPointerˢ),
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ7(Ꮡ(new TestAnonymousFields_s1ᴛ1(1, 2)), Ꮡ(new TestAnonymousFields_S2ᴛ3(3, 4)));
        },
        want: @"{""X"":2,""Y"":4}"
    ), new(
        // Exported fields on embedded unexported structs at multiple levels
        // of nesting should still be serialized.
        CaseName: Name(nestedStructAndIntsˢ),
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ8(new TestAnonymousFields_s1ᴛ2(1, 2, new TestAnonymousFields_s2(3, 4)), 6);
        },
        want: @"{""MyInt1"":1,""MyInt2"":3}"
    ), new(
        // If an anonymous struct pointer field is nil, we should ignore
        // the embedded fields behind it. Not properly doing so may
        // result in the wrong output or reflect panics.
        CaseName: Name("EmbeddedFieldBehindNilPointer"),
        makeInput: () => {
            return new TestAnonymousFields_Sᴛ9();
        },
        want: @"{}"
    )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestAnonymousFields_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.makeInput());
            if (err != nil) {
                tΔ1.Fatalf("%s: Marshal error: %v", ttʗ1.Where, err);
            }
            if (((sstring)b) != ttʗ1.want) {
                tΔ1.Fatalf("%s: Marshal:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, b, ttʗ1.want);
            }
        });
    }
}

public partial struct BugA {
    public @string S;
}

public partial struct BugB {
    /*embed*/ public BugA BugA;
    public @string S;
}

public partial struct BugC {
    public @string S;
}

// Legal Go: We never use the repeated embedded field (S).
public partial struct BugX {
    public nint A;
    /*embed*/ public BugA BugA;
    /*embed*/ public BugB BugB;
}

// golang.org/issue/16042.
// Even if a nil interface value is passed in, as long as
// it implements Marshaler, it should be marshaled.
internal partial struct nilJSONMarshaler /*@string*/;

internal static (slice<byte>, error) MarshalJSON(this ж<nilJSONMarshaler> Ꮡnm) {
    ref var nm = ref Ꮡnm.DerefOrNull();

    if (Ꮡnm == nil) {
        return Marshal("0zenil0");
    }
    return Marshal("zenil:" + ((@string)(nm)));
}

// golang.org/issue/34235.
// Even if a nil interface value is passed in, as long as
// it implements encoding.TextMarshaler, it should be marshaled.
internal partial struct nilTextMarshaler /*@string*/;

internal static (slice<byte>, error) MarshalText(this ж<nilTextMarshaler> Ꮡnm) {
    ref var nm = ref Ꮡnm.DerefOrNull();

    if (Ꮡnm == nil) {
        return (slice<byte>("0zenil0"u8), default);
    }
    return (slice<byte>("zenil:" + ((@string)(nm))), default);
}

internal partial struct TestNilMarshal_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any @in;
    internal @string want;
}

internal partial struct TestNilMarshal_type /*dyn*/ {
    public @string M;
}

internal partial struct TestNilMarshal_typeᴛ1 /*dyn*/ {
    public global::go.encoding.json_package.Marshaler M;
}

internal partial struct TestNilMarshal_typeᴛ2 /*dyn*/ {
    public any M;
}

internal partial struct TestNilMarshal_typeᴛ3 /*dyn*/ {
    public encoding.TextMarshaler M;
}

// See golang.org/issue/16042 and golang.org/issue/34235.
public static void TestNilMarshal(ж<testing.T> t) {
    var tests = slice<TestNilMarshal_tests>([
        new(Name(""u8), default, @"null"),
        new(Name(""u8), @new<float64>(), @"0"),
        new(Name(""u8), slice<any>(default), @"null"),
        new(Name(""u8), slice<@string>(default), @"null"),
        new(Name(""u8), (map<@string, @string>)default, @"null"),
        new(Name(""u8), slice<byte>(default), @"null"),
        new(Name(""u8), new TestNilMarshal_type("gopher"), @"{""M"":""gopher""}"),
        new(Name(""u8), new TestNilMarshal_typeᴛ1(), @"{""M"":null}"),
        new(Name(""u8), new TestNilMarshal_typeᴛ1(new json_internal_test_package.nilJSONMarshalerжMarshaler(((ж<nilJSONMarshaler>)nil))), @"{""M"":""0zenil0""}"),
        new(Name(""u8), new TestNilMarshal_typeᴛ2((ж<nilJSONMarshaler>)nil), @"{""M"":null}"),
        new(Name(""u8), new TestNilMarshal_typeᴛ3(), @"{""M"":null}"),
        new(Name(""u8), new TestNilMarshal_typeᴛ3(new json_internal_test_package.nilTextMarshalerжTextMarshaler(((ж<nilTextMarshaler>)nil))), @"{""M"":""0zenil0""}"),
        new(Name(""u8), new TestNilMarshal_typeᴛ2((ж<nilTextMarshaler>)nil), @"{""M"":null}")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestNilMarshal_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (got, err) = Marshal(ttʗ1.@in);
            switch (ᐧ) {
            case {} when err != nil: {
                tΔ1.Fatalf("%s: Marshal error: %v", ttʗ1.Where, err);
                break;
            }
            case {} when ((sstring)got) != ttʗ1.want: {
                tΔ1.Fatalf("%s: Marshal:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, got, ttʗ1.want);
                break;
            }}
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object marshalErrorˢ = (@string)"Marshal error:"u8;
internal static ref readonly @string a23ˢ => ref a23ˢᵇ;
private static readonly @string a23ˢᵇ = @"{""A"":23}"u8;

// Issue 5245.
public static void TestEmbeddedBug(ж<testing.T> t) {
    var v = new BugB(
        new BugA("A"),
        "B"
    );
    var (b, err) = Marshal(v);
    if (err != nil) {
        t.Fatal(marshalErrorˢ, err);
    }
    @string want = @"{""S"":""B""}";
    @string got = (@string)b;
    if (got != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
    // Now check that the duplicate field, S, does not appear.
    var x = new BugX(
        A: 23
    );
    (b, err) = Marshal(x);
    if (err != nil) {
        t.Fatal(marshalErrorˢ, err);
    }
    want = a23ˢ;
    got = (@string)b;
    if (got != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

public partial struct BugD {
// Same as BugA after tagging.
    public @string XXX; /*`json:"S"`*/
}

// BugD's tagged S field should dominate BugA's.
public partial struct BugY {
    /*embed*/ public BugA BugA;
    /*embed*/ public BugD BugD;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sBugDˢ => ref sBugDˢᵇ;
private static readonly @string sBugDˢᵇ = @"{""S"":""BugD""}"u8;

// Test that a field with a tag dominates untagged fields.
public static void TestTaggedFieldDominates(ж<testing.T> t) {
    var v = new BugY(
        new BugA("BugA"),
        new BugD("BugD")
    );
    var (b, err) = Marshal(v);
    if (err != nil) {
        t.Fatal(marshalErrorˢ, err);
    }
    @string want = sBugDˢ;
    @string got = (@string)b;
    if (got != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

// There are no tags here, so S should not appear.
public partial struct BugZ {
    /*embed*/ public BugA BugA;
    /*embed*/ public BugC BugC;
    /*embed*/ public BugY BugY; // Contains a tagged S field through BugD; should not dominate.
}

public static void TestDuplicatedFieldDisappears(ж<testing.T> t) {
    var v = new BugZ(
        new BugA("BugA"),
        new BugC("BugC"),
        new BugY(
            new BugA("nested BugA"),
            new BugD("nested BugD")
        )
    );
    var (b, err) = Marshal(v);
    if (err != nil) {
        t.Fatal(marshalErrorˢ, err);
    }
    @string want = @"{}";
    @string got = (@string)b;
    if (got != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidˢ => ref invalidˢᵇ;
private static readonly @string invalidˢᵇ = @"invalid"u8;

internal partial struct TestIssue10281_Foo /*dyn*/ {
    public global::go.encoding.json_package.Number N;
}

public static void TestIssue10281(ж<testing.T> t) {
    ref var x = ref heap<TestIssue10281_Foo>(new TestIssue10281_Foo((global::go.encoding.json_package.Number)(@string)invalidˢ), out var Ꮡx);

    var (_, err) = Marshal(Ꮡx); if (err == nil) {
        t.Fatalf("Marshal error: got nil, want non-nil");
    }
}

// Trigger an error in Marshal with cyclic data.
internal partial struct TestMarshalErrorAndReuseEncodeState_Dummy /*dyn*/ {
    public @string Name;
    public ж<TestMarshalErrorAndReuseEncodeState_Dummy> Next;
}

internal partial struct TestMarshalErrorAndReuseEncodeState_Data /*dyn*/ {
    public @string A;
    public nint I;
}

public static void TestMarshalErrorAndReuseEncodeState(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Disable the GC temporarily to prevent encodeState's in Pool being cleaned away during the test.
        nint percent = debug.SetGCPercent(-1);
        defer(debug.SetGCPercent, percent, ref ᒐ);
        ref var dummy = ref heap<TestMarshalErrorAndReuseEncodeState_Dummy>(new TestMarshalErrorAndReuseEncodeState_Dummy(Name: "Dummy"), out var Ꮡdummy);
        dummy.Next = Ꮡdummy;
        var (_, errΔ1) = Marshal(dummy); if (errΔ1 == nil) {
            t.Errorf("Marshal error: got nil, want non-nil");
        }
        var want = new TestMarshalErrorAndReuseEncodeState_Data(A: "a", I: 1);
        var (b, err) = Marshal(want);
        if (err != nil) {
            t.Errorf("Marshal error: %v", err);
        }

        ref var got = ref heap(new TestMarshalErrorAndReuseEncodeState_Data(), out var Ꮡgot);
        var errΔ2 = Unmarshal(b, Ꮡgot); if (errΔ2 != nil) {
            t.Errorf("Unmarshal error: %v", errΔ2);
        }
        if (got != want) {
            t.Errorf("Unmarshal:\n\tgot:  %v\n\twant: %v", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestHTMLEscape(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    bytes.Buffer want = default;
    @string m = @"{""M"":""<html>foo &"u8 + ((@string)(new byte[]{0xe2, 0x80, 0xa8, 0x20, 0xe2, 0x80, 0xa9})) + @"</html>""}"u8;
    want.Write(slice<byte>(@"{""M"":""\u003chtml\u003efoo \u0026\u2028 \u2029\u003c/html\u003e""}"u8));
    HTMLEscape(Ꮡb, slice<byte>(m));
    if (!bytes.Equal(b.Bytes(), want.Bytes())) {
        t.Errorf("HTMLEscape:\n\tgot:  %s\n\twant: %s", b.Bytes(), want.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string n42ˢ => ref n42ˢᵇ;
private static readonly @string n42ˢᵇ = @"{""n"":""42""}"u8;

internal partial struct TestEncodePointerString_stringPointer /*dyn*/ {
    public ж<int64> N; /*`json:"n,string"`*/
}

// golang.org/issue/8582
public static void TestEncodePointerString(ж<testing.T> t) {
    ref var n = ref heap(new int64(), out var Ꮡn);
    n = 42;
    var (b, err) = Marshal(new TestEncodePointerString_stringPointer(N: Ꮡn));
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string got = (@string)b;
    @string want = n42ˢ; if (got != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
    ref var back = ref heap(new TestEncodePointerString_stringPointer(), out var Ꮡback);
    err = Unmarshal(b, Ꮡback);
    switch (ᐧ) {
    case {} when err != nil: {
        t.Fatalf("Unmarshal error: %v", err);
        break;
    }
    case {} when back.N == nil: {
        t.Fatalf("Unmarshal: back.N = nil, want non-nil");
        break;
    }
    case {} when back.N.Value is not 42: {
        t.Fatalf("Unmarshal: *back.N = %d, want 42", back.N.Value);
        break;
    }}
}

partial struct encodeStringTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
}
internal static slice<encodeStringTestsᴛ1> encodeStringTests = [
    new("\x00", @"""\u0000"""),
    new("\x01", @"""\u0001"""),
    new("\x02", @"""\u0002"""),
    new("\x03", @"""\u0003"""),
    new("\x04", @"""\u0004"""),
    new("\x05", @"""\u0005"""),
    new("\x06", @"""\u0006"""),
    new("\x07", @"""\u0007"""),
    new("\x08", @"""\b"""),
    new("\x09", @"""\t"""),
    new("\x0a", @"""\n"""),
    new("\x0b", @"""\u000b"""),
    new("\x0c", @"""\f"""),
    new("\x0d", @"""\r"""),
    new("\x0e", @"""\u000e"""),
    new("\x0f", @"""\u000f"""),
    new("\x10", @"""\u0010"""),
    new("\x11", @"""\u0011"""),
    new("\x12", @"""\u0012"""),
    new("\x13", @"""\u0013"""),
    new("\x14", @"""\u0014"""),
    new("\x15", @"""\u0015"""),
    new("\x16", @"""\u0016"""),
    new("\x17", @"""\u0017"""),
    new("\x18", @"""\u0018"""),
    new("\x19", @"""\u0019"""),
    new("\x1a", @"""\u001a"""),
    new("\x1b", @"""\u001b"""),
    new("\x1c", @"""\u001c"""),
    new("\x1d", @"""\u001d"""),
    new("\x1e", @"""\u001e"""),
    new("\x1f", @"""\u001f""")
];

public static void TestEncodeString(ж<testing.T> t) {
    foreach (var (_, tt) in range(encodeStringTests)) {
        var (b, err) = Marshal(tt.@in);
        if (err != nil) {
            t.Errorf("Marshal(%q) error: %v", tt.@in, err);
            continue;
        }
        @string @out = (@string)b;
        if (@out != tt.@out) {
            t.Errorf("Marshal(%q) = %#q, want %#q", tt.@in, @out, tt.@out);
        }
    }
}

internal partial struct jsonbyte /*num:byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string jbDˢ => ref jbDˢᵇ;
private static readonly @string jbDˢᵇ = @"{""JB"":%d}"u8;

internal static (slice<byte>, error) MarshalJSON(this jsonbyte b) { return tenc(jbDˢ, b); }

internal partial struct textbyte /*num:byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tbDˢ => ref tbDˢᵇ;
private static readonly @string tbDˢᵇ = @"TB:%d"u8;

internal static (slice<byte>, error) MarshalText(this textbyte b) { return tenc(tbDˢ, b); }

internal partial struct jsonint /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string jiDˢ => ref jiDˢᵇ;
private static readonly @string jiDˢᵇ = @"{""JI"":%d}"u8;

internal static (slice<byte>, error) MarshalJSON(this jsonint i) { return tenc(jiDˢ, i); }

internal partial struct textint /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tiDˢ => ref tiDˢᵇ;
private static readonly @string tiDˢᵇ = @"TI:%d"u8;

internal static (slice<byte>, error) MarshalText(this textint i) { return tenc(tiDˢ, i); }

internal static (slice<byte>, error) tenc(@string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    fmt.Fprintf(new json_test_package.bytes_BufferжWriter(Ꮡbuf), format, a.ꓸꓸꓸ);
    return (buf.Bytes(), default);
}

internal partial struct textfloat /*num:float64*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tf02fˢ => ref tf02fˢᵇ;
private static readonly @string tf02fˢᵇ = @"TF:%0.2f"u8;

internal static (slice<byte>, error) MarshalText(this textfloat f) { return tenc(tf02fˢ, f); }

internal partial struct TestEncodeBytekind_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any @in;
    internal @string want;
}

// Issue 13783
public static void TestEncodeBytekind(ж<testing.T> t) {
    var tests = slice<TestEncodeBytekind_tests>([
        new(Name(""u8), (byte)7, "7"),
        new(Name(""u8), (jsonbyte)7, @"{""JB"":7}"),
        new(Name(""u8), (textbyte)4, @"""TB:4"""),
        new(Name(""u8), (jsonint)5, @"{""JI"":5}"),
        new(Name(""u8), (textint)1, @"""TI:1"""),
        new(Name(""u8), slice<byte>([0, 1]), @"""AAE="""),
        new(Name(""u8), slice<jsonbyte>([0, 1]), @"[{""JB"":0},{""JB"":1}]"),
        new(Name(""u8), slice<slice<jsonbyte>>([[0, 1], [3]]), @"[[{""JB"":0},{""JB"":1}],[{""JB"":3}]]"),
        new(Name(""u8), slice<textbyte>([2, 3]), @"[""TB:2"",""TB:3""]"),
        new(Name(""u8), slice<jsonint>([5, 4]), @"[{""JI"":5},{""JI"":4}]"),
        new(Name(""u8), slice<textint>([9, 3]), @"[""TI:9"",""TI:3""]"),
        new(Name(""u8), slice<nint>([9, 3]), @"[9,3]"),
        new(Name(""u8), slice<textfloat>([12D, 3D]), @"[""TF:12.00"",""TF:3.00""]")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestEncodeBytekind_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.@in);
            if (err != nil) {
                tΔ1.Errorf("%s: Marshal error: %v", ttʗ1.Where, err);
            }
            @string got = (@string)b;
            @string want = ttʗ1.want;
            if (got != want) {
                tΔ1.Errorf("%s: Marshal:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, got, want);
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ6 => ref wantᶜ6ᵇ;
private static readonly @string wantᶜ6ᵇ = @"{""a:z"":3,""x:y"":1,""y:x"":2,""z:a"":4}"u8;

public static void TestTextMarshalerMapKeysAreSorted(ж<testing.T> t) {
    var (got, err) = Marshal(new map<unmarshalerText, nint>{
        [new("x", "y")] = 1,
        [new("y", "x")] = 2,
        [new("a", "z")] = 3,
        [new("z", "a")] = 4
    });
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string want = wantᶜ6;
    if (((sstring)got) != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ7 => ref wantᶜ7ᵇ;
private static readonly @string wantᶜ7ᵇ = @"{"""":1,""A:B"":2}"u8;

// https://golang.org/issue/33675
public static void TestNilMarshalerTextMapKey(ж<testing.T> t) {
    var (got, err) = Marshal(new map<ж<unmarshalerText>, nint>{
        [((ж<unmarshalerText>)nil)] = 1,
        [Ꮡ(new unmarshalerText("A", "B"))] = 2
    });
    if (err != nil) {
        t.Fatalf("Marshal error: %v", err);
    }
    @string want = wantᶜ7;
    if (((sstring)got) != want) {
        t.Errorf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

internal static Func<@string, ж<regexp.Regexp>> re = regexp.MustCompile;

// syntactic checks on form of marshaled floating point numbers.
internal static slice<ж<regexp.Regexp>> badFloatREs = slice<ж<regexp.Regexp>>([
    re(@"p"),                     // no binary exponential notation
    re(@"^\+"),                   // no leading + sign
    re(@"^-?0[^.]"),              // no unnecessary leading zeros
    re(@"^-?\."),                 // leading zero required before decimal point
    re(@"\.(e|$)"),               // no trailing decimal
    re(@"\.[0-9]+0(e|$)"),        // no trailing zero in fraction
    re(@"^-?(0|[0-9]{2,})\..*e"), // exponential notation must have normalized mantissa
    re(@"e[0-9]"),                // positive exponent must be signed
    re(@"e[+-]0"),                // exponent must not have leading zeros
    re(@"e-[1-6]$"),              // not tiny enough for exponential notation
    re(@"e+(.|1.|20)$"),          // not big enough for exponential notation
    re(@"^-?0\.0000000"),         // too tiny, should use exponential notation
    re(@"^-?[0-9]{22}"),          // too big, should use exponential notation
    re(@"[1-9][0-9]{16}[1-9]"),   // too many significant digits in integer
    re(@"[1-9][0-9.]{17}[1-9]"),  // too many significant digits in decimal
    // below here for float32 only
    re(@"[1-9][0-9]{8}[1-9]"), // too many significant digits in integer
    re(@"[1-9][0-9.]{9}[1-9]") // too many significant digits in decimal
]);

public static void TestMarshalFloat(ж<testing.T> t) {
    t.Parallel();
    nint nfail = 0;
    void test(float64 f, nint bits) {
        var vf = (any)f;
        if (bits == 32) {
            f = (float64)(float32)f; // round
            vf = (float32)f;
        }
        var (bout, err) = Marshal(vf);
        if (err != nil) {
            t.Errorf("Marshal(%T(%g)) error: %v", vf, vf, err);
            nfail++;
            return;
        }
        @string @out = (@string)bout;

        // result must convert back to the same float
        (var g, err) = strconv.ParseFloat(@out, bits);
        if (err != nil) {
            t.Errorf("ParseFloat(%q) error: %v", @out, err);
            nfail++;
            return;
        }
        if (f != g || fmt.Sprint(f) != fmt.Sprint(g)) { // fmt.Sprint handles ±0
            t.Errorf("ParseFloat(%q):\n\tgot:  %g\n\twant: %g", @out, (float32)g, vf);
            nfail++;
            return;
        }

        var bad = badFloatREs;
        if (bits == 64) {
            bad = bad.slice(0, len(bad) - 2);
        }
        foreach (var (_, re) in range(bad)) {
            if (re.MatchString(@out)) {
                t.Errorf("Marshal(%T(%g)) = %q; must not match /%s/", vf, vf, @out, re.OrTypedNil());
                nfail++;
                return;
            }
        }
    }

    float64 bigger = math.Inf(+1);
    float64 smaller = math.Inf(-1);

    @string digits = "1.2345678901234567890123";
    for (nint i = len(digits); i >= 2; i--) {
        if (testing.Short() && i < len(digits) - 4) {
            break;
        }
        for (nint exp = -30; exp <= 30; exp++) {
            foreach (var (_, sign) in (@string)"+-") {
                for (nint bits = 32; bits <= 64; bits += 32) {
                    @string s = fmt.Sprintf("%c%se%d"u8, sign, digits.slice(0, i), exp);
                    var (f, err) = strconv.ParseFloat(s, bits);
                    if (err != nil) {
                        log.Fatal(err);
                    }
                    var next = math.Nextafter;
                    if (bits == 32) {
                        next = (float64 g, float64 h) => (float64)math.Nextafter32((float32)g, (float32)h);
                    }
                    test(f, bits);
                    test(next(f, bigger), bits);
                    test(next(f, smaller), bits);
                    if (nfail > 50) {
                        t.Fatalf("stopping test early");
                    }
                }
            }
        }
    }
    test(0D, 64);
    test(math.Copysign(0D, -1D), 64);
    test(0D, 32);
    test(math.Copysign(0D, -1D), 32);
}

internal partial struct TestMarshalRawMessageValue_T1 /*dyn*/ {
    public global::go.encoding.json_package.RawMessage M; /*`json:",omitempty"`*/
}

internal partial struct TestMarshalRawMessageValue_T2 /*dyn*/ {
    public ж<global::go.encoding.json_package.RawMessage> M; /*`json:",omitempty"`*/
}

internal partial struct TestMarshalRawMessageValue_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal any @in;
    internal @string want;
    internal bool ok;
}

internal partial struct TestMarshalRawMessageValue_type /*dyn*/ {
    public global::go.encoding.json_package.RawMessage M;
}

internal partial struct TestMarshalRawMessageValue_typeᴛ1 /*dyn*/ {
    public ж<global::go.encoding.json_package.RawMessage> M;
}

internal partial struct TestMarshalRawMessageValue_typeᴛ2 /*dyn*/ {
    public global::go.encoding.json_package.RawMessage X;
}

internal partial struct TestMarshalRawMessageValue_typeᴛ3 /*dyn*/ {
    public ж<global::go.encoding.json_package.RawMessage> X;
}

public static void TestMarshalRawMessageValue(ж<testing.T> t) {
    ref var rawNil = ref heap<global::go.encoding.json_package.RawMessage>(out var ᏑrawNil);

    rawNil = ((global::go.encoding.json_package.RawMessage)default);
    ref var rawEmpty = ref heap<global::go.encoding.json_package.RawMessage>(out var ᏑrawEmpty);

    rawEmpty = ((global::go.encoding.json_package.RawMessage)slice<byte>([]));
    ref var rawText = ref heap<global::go.encoding.json_package.RawMessage>(out var ᏑrawText);

    rawText = ((global::go.encoding.json_package.RawMessage)slice<byte>(@"""foo"""u8));

    var tests = slice<TestMarshalRawMessageValue_tests>([
        // Test with nil RawMessage.
        new(Name(""u8), rawNil, "null", true),
        new(Name(""u8), ᏑrawNil, "null", true),
        new(Name(""u8), slice<any>([rawNil]), "[null]", true),
        new(Name(""u8), Ꮡ(slice<any>([rawNil])), "[null]", true),
        new(Name(""u8), slice<any>([ᏑrawNil]), "[null]", true),
        new(Name(""u8), Ꮡ(slice<any>([ᏑrawNil])), "[null]", true),
        new(Name(""u8), new TestMarshalRawMessageValue_type(rawNil), @"{""M"":null}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_type(rawNil)), @"{""M"":null}", true),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ1(ᏑrawNil), @"{""M"":null}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ1(ᏑrawNil)), @"{""M"":null}", true),
        new(Name(""u8), new map<@string, any>{["M"] = rawNil}, @"{""M"":null}", true),
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"] = rawNil}), @"{""M"":null}", true),
        new(Name(""u8), new map<@string, any>{["M"] = ᏑrawNil}, @"{""M"":null}", true),
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"] = ᏑrawNil}), @"{""M"":null}", true),
        new(Name(""u8), new TestMarshalRawMessageValue_T1(rawNil), "{}", true),
        new(Name(""u8), new TestMarshalRawMessageValue_T2(ᏑrawNil), @"{""M"":null}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T1(rawNil)), "{}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T2(ᏑrawNil)), @"{""M"":null}", true),

        // Test with empty, but non-nil, RawMessage.
        new(Name(""u8), rawEmpty, "", false),
        new(Name(""u8), ᏑrawEmpty, "", false),
        new(Name(""u8), slice<any>([rawEmpty]), "", false),
        new(Name(""u8), Ꮡ(slice<any>([rawEmpty])), "", false),
        new(Name(""u8), slice<any>([ᏑrawEmpty]), "", false),
        new(Name(""u8), Ꮡ(slice<any>([ᏑrawEmpty])), "", false),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ2(rawEmpty), "", false),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ2(rawEmpty)), "", false),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ3(ᏑrawEmpty), "", false),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ3(ᏑrawEmpty)), "", false),
        new(Name(""u8), new map<@string, any>{["nil"] = rawEmpty}, "", false),
        new(Name(""u8), Ꮡ(new map<@string, any>{["nil"] = rawEmpty}), "", false),
        new(Name(""u8), new map<@string, any>{["nil"] = ᏑrawEmpty}, "", false),
        new(Name(""u8), Ꮡ(new map<@string, any>{["nil"] = ᏑrawEmpty}), "", false),
        new(Name(""u8), new TestMarshalRawMessageValue_T1(rawEmpty), "{}", true),
        new(Name(""u8), new TestMarshalRawMessageValue_T2(ᏑrawEmpty), "", false),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T1(rawEmpty)), "{}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T2(ᏑrawEmpty)), "", false),

        // Test with RawMessage with some text.
        //
        // The tests below marked with Issue6458 used to generate "ImZvbyI=" instead "foo".
        // This behavior was intentionally changed in Go 1.8.
        // See https://golang.org/issues/14493#issuecomment-255857318
        new(Name(""u8), rawText, @"""foo""", true), // Issue6458
        new(Name(""u8), ᏑrawText, @"""foo""", true),
        new(Name(""u8), slice<any>([rawText]), @"[""foo""]", true),    // Issue6458
        new(Name(""u8), Ꮡ(slice<any>([rawText])), @"[""foo""]", true), // Issue6458
        new(Name(""u8), slice<any>([ᏑrawText]), @"[""foo""]", true),
        new(Name(""u8), Ꮡ(slice<any>([ᏑrawText])), @"[""foo""]", true),
        new(Name(""u8), new TestMarshalRawMessageValue_type(rawText), @"{""M"":""foo""}", true), // Issue6458
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_type(rawText)), @"{""M"":""foo""}", true),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ1(ᏑrawText), @"{""M"":""foo""}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ1(ᏑrawText)), @"{""M"":""foo""}", true),
        new(Name(""u8), new map<@string, any>{["M"] = rawText}, @"{""M"":""foo""}", true),    // Issue6458
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"] = rawText}), @"{""M"":""foo""}", true), // Issue6458
        new(Name(""u8), new map<@string, any>{["M"] = ᏑrawText}, @"{""M"":""foo""}", true),
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"] = ᏑrawText}), @"{""M"":""foo""}", true),
        new(Name(""u8), new TestMarshalRawMessageValue_T1(rawText), @"{""M"":""foo""}", true), // Issue6458
        new(Name(""u8), new TestMarshalRawMessageValue_T2(ᏑrawText), @"{""M"":""foo""}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T1(rawText)), @"{""M"":""foo""}", true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T2(ᏑrawText)), @"{""M"":""foo""}", true)
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestMarshalRawMessageValue_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.@in);
            var ok = (err == nil); if (ok != ttʗ1.ok) {
                if (err != nil) {
                    tΔ1.Errorf("%s: Marshal error: %v", ttʗ1.Where, err);
                } else {
                    tΔ1.Errorf("%s: Marshal error: got nil, want non-nil", ttʗ1.Where);
                }
            }
            @string got = (@string)b; if (got != ttʗ1.want) {
                tΔ1.Errorf("%s: Marshal:\n\tinput: %#v\n\tgot:  %s\n\twant: %s", ttʗ1.Where, ttʗ1.@in, got, ttʗ1.want);
            }
        });
    }
}

internal partial struct marshalPanic {
}

internal static (slice<byte>, error) MarshalJSON(this marshalPanic _) { throw panic((nint)0xdead); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object marshalShouldHaveˢ = (@string)"Marshal should have panicked"u8;

public static void TestMarshalPanic(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var got = recover(); if (!reflect.DeepEqual(got, (nint)0xdead)) {
                t.Errorf("panic() = (%T)(%v), want 0xdead", got, got);
            }
        }, ref ᒐ);
        Marshal(Ꮡ(new marshalPanic()));
        t.Error(marshalShouldHaveˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string a000A0ˢ => ref a000A0ˢᵇ;
private static readonly @string a000A0ˢᵇ = @"{""A0"":0,""À"":0,""Aβ"":0}"u8;

internal partial struct TestMarshalUncommonFieldNames_v /*dyn*/ {
    public nint A0, À, Aβ;
}

public static void TestMarshalUncommonFieldNames(ж<testing.T> t) {
    var v = new TestMarshalUncommonFieldNames_v();
    var (b, err) = Marshal(v);
    if (err != nil) {
        t.Fatal(marshalErrorˢ, err);
    }
    @string want = a000A0ˢ;
    @string got = (@string)b;
    if (got != want) {
        t.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testVariableˢ => ref testVariableˢᵇ;
private static readonly @string testVariableˢᵇ = "test variable"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string errTextᶜ => ref errTextᶜᵇ;
private static readonly @string errTextᶜᵇ = "json: test error"u8;

internal partial struct TestMarshalerError_tests /*dyn*/ {
    /*embed*/ public CaseName CaseName;
    internal ж<global::go.encoding.json_package.MarshalerError> err;
    internal @string want;
}

public static void TestMarshalerError(ж<testing.T> t) {
    @string s = testVariableˢ;
    var st = reflect.TypeOf(s);
    @string errText = errTextᶜ;

    var tests = slice<TestMarshalerError_tests>([new(
        Name(""u8),
        Ꮡ(new MarshalerError(st, fmt.Errorf(errText), "")),
        "json: error calling MarshalJSON for type "u8 + st.String() + ": "u8 + errText
    ), new(
        Name(""u8),
        Ꮡ(new MarshalerError(st, fmt.Errorf(errText), "TestMarshalerError")),
        "json: error calling TestMarshalerError for type "u8 + st.String() + ": "u8 + errText
    )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestMarshalerError_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            @string got = ttʗ1.err.Error();
            if (got != ttʗ1.want) {
                tΔ1.Errorf("%s: Error:\n\tgot:  %s\n\twant: %s", ttʗ1.Where, got, ttʗ1.want);
            }
        });
    }
}

internal partial struct marshaledValue /*@string*/;

internal static (slice<byte>, error) MarshalJSON(this marshaledValue v) {
    return (slice<byte>((@string)v), default);
}

public static void TestIssue63379(ж<testing.T> t) {
    foreach (var (_, v) in range(slice<@string>([
        "[]<",
        "[]>",
        "[]&",
        "[]\u2028",
        "[]\u2029",
        "{}<",
        "{}>",
        "{}&",
        "{}\u2028",
        "{}\u2029"
    ]))) {
        var (_, err) = Marshal((marshaledValue)v);
        if (err == nil) {
            t.Errorf("expected error for %q", v);
        }
    }
}

// Issue #73733: encoding/json used a WaitGroup to coordinate access to cache entries.
// Since WaitGroup.Wait is durably blocking, this caused apparent deadlocks when
// multiple bubbles called json.Marshal at the same time.
public static void TestSynctestMarshal(ж<testing.T> t) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    foreach (var _ᴛ1 in range(5)) {
        Ꮡwg.Go(() => {
            synctest.Test(t, (ж<testing.T> tΔ1) => {
                var (_, err) = Marshal(slice<@string>([]));
                if (err != nil) {
                    tΔ1.Errorf("Marshal: %v", err);
                }
            });
        });
    }
    Ꮡwg.Wait();
}

} // end json_internal_test_package
