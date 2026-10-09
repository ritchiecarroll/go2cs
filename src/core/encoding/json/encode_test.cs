// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
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
using testing = testing_package;
using time = time_package;
using go.runtime;
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
internal static readonly @string somethingˢ = "something"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ1 = """
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
"""u8;

public static void TestOmitEmpty(ж<testing.T> Ꮡt) {
    @string want = wantᶜ1;
    ref var o = ref heap(new OptionalsEmpty(), out var Ꮡo);
    o.Sw = somethingˢ;
    o.Mr = new map<@string, any>{};
    o.Mo = new map<@string, any>{};
    var (got, err) = MarshalIndent(Ꮡo, ""u8, " "u8);
    if (err != default!) {
        Ꮡt.Fatalf("MarshalIndent error: %v"u8, err);
    }
    {
        @string gotΔ1 = ((@string)got); if (gotΔ1 != want) {
            Ꮡt.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n"u8, indentNewlines(gotΔ1), indentNewlines(want));
        }
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
    public global::go.encoding.json_package.isZeroer NilIsZeroer; /*`json:"niliszeroer,omitzero"`*/    // nil interface
    public global::go.encoding.json_package.isZeroer NonNilIsZeroer; /*`json:"nonniliszeroer,omitzero"`*/ // non-nil interface
    public global::go.encoding.json_package.isZeroer NoPanicStruct0; /*`json:"nps0,omitzero"`*/           // non-nil interface with nil pointer
    public global::go.encoding.json_package.isZeroer NoPanicStruct1; /*`json:"nps1,omitzero"`*/           // non-nil interface with non-nil pointer
    public ж<NoPanicStruct> NoPanicStruct2; /*`json:"nps2,omitzero"`*/           // nil pointer
    public ж<NoPanicStruct> NoPanicStruct3; /*`json:"nps3,omitzero"`*/           // non-nil pointer
    public NoPanicStruct NoPanicStruct4; /*`json:"nps4,omitzero"`*/           // concrete type
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ2 = """
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
"""u8;

public static void TestOmitZero(ж<testing.T> Ꮡt) {
    @string want = wantᶜ2;
    ref var o = ref heap(new OptionalsZero(), out var Ꮡo);
    o.Sw = somethingˢ;
    o.SloNonNil = new slice<@string>(0);
    o.Mr = new map<@string, any>{};
    o.Mo = new map<@string, any>{};
    o.Foo = -0D;
    o.Foo2 = new float64[]{+0D, -0D}.array();
    o.TimeLocal = new time.Time(nil).Local();
    o.NonNilIsZeroer = new json_test_package.time_TimeᴠisZeroer(new time.Time(nil));
    o.NoPanicStruct0 = new json_internal_test_package.NoPanicStructжisZeroer(((ж<NoPanicStruct>)nil));
    o.NoPanicStruct1 = new json_internal_test_package.NoPanicStructжisZeroer(Ꮡ(new NoPanicStruct(nil)));
    o.NoPanicStruct3 = Ꮡ(new NoPanicStruct(nil));
    var (got, err) = MarshalIndent(Ꮡo, ""u8, " "u8);
    if (err != default!) {
        Ꮡt.Fatalf("MarshalIndent error: %v"u8, err);
    }
    {
        @string gotΔ1 = ((@string)got); if (gotΔ1 != want) {
            Ꮡt.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n"u8, indentNewlines(gotΔ1), indentNewlines(want));
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ3 = """
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
"""u8;

public static void TestOmitZeroMap(ж<testing.T> Ꮡt) {
    @string want = wantᶜ3;
    var m = new map<@string, OptionalsZero>{["foo"u8] = new()};
    var (got, err) = MarshalIndent(m, ""u8, " "u8);
    if (err != default!) {
        Ꮡt.Fatalf("MarshalIndent error: %v"u8, err);
    }
    {
        @string gotΔ1 = ((@string)got); if (gotΔ1 != want) {
            fmt.Println(gotΔ1);
            Ꮡt.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n"u8, indentNewlines(gotΔ1), indentNewlines(want));
        }
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
internal static readonly @string wantᶜ4 = """
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
"""u8;

public static void TestOmitEmptyZero(ж<testing.T> Ꮡt) {
    @string want = wantᶜ4;
    ref var o = ref heap(new OptionalsEmptyZero(), out var Ꮡo);
    o.Sw = somethingˢ;
    o.SloNonNil = new slice<@string>(0);
    o.Mr = new map<@string, any>{};
    o.Mo = new map<@string, any>{};
    var (got, err) = MarshalIndent(Ꮡo, ""u8, " "u8);
    if (err != default!) {
        Ꮡt.Fatalf("MarshalIndent error: %v"u8, err);
    }
    {
        @string gotΔ1 = ((@string)got); if (gotΔ1 != want) {
            Ꮡt.Errorf("MarshalIndent:\n\tgot:  %s\n\twant: %s\n"u8, indentNewlines(gotΔ1), indentNewlines(want));
        }
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
internal static readonly @string allTypesˢ = "AllTypes"u8;
internal static readonly @string stringDoubleEscapesˢ = "StringDoubleEscapes"u8;

internal partial struct TestRoundtripStringTag_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
    internal StringTag @in;
    internal @string want; // empty to just test that we roundtrip
}

public static void TestRoundtripStringTag(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestRoundtripStringTag_tests[]{new(
        CaseName: Name(allTypesˢ),
        @in: new StringTag(
            BoolStr: true,
            IntStr: 42,
            UintptrStr: 44,
            StrStr: "xzbit"u8,
            NumberStr: "46"u8
        ),
        want: """
{
	"BoolStr": "true",
	"IntStr": "42",
	"UintptrStr": "44",
	"StrStr": "\"xzbit\"",
	"NumberStr": "46"
}
"""u8
    ), new(
        CaseName: Name(stringDoubleEscapesˢ), // See golang.org/issues/38173.

        @in: new StringTag(
            StrStr: "\b\f\n\r\t\"\\"u8,
            NumberStr: "0"u8
        ), // just to satisfy the roundtrip

        want: """
{
	"BoolStr": "false",
	"IntStr": "0",
	"UintptrStr": "0",
	"StrStr": "\"\\b\\f\\n\\r\\t\\\"\\\\\"",
	"NumberStr": "0"
}
"""u8
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestRoundtripStringTag_tests(), out var Ꮡtt);
        tt = vᴛ1;

        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (got, err) = MarshalIndent(Ꮡtt.of(TestRoundtripStringTag_tests.Ꮡin), ""u8, "\t"u8);
            if (err != default!) {
                tΔ1.Fatalf("%s: MarshalIndent error: %v"u8, Ꮡtt.Value.Where, err);
            }
            {
                @string gotΔ1 = ((@string)got); if (gotΔ1 != Ꮡtt.Value.want) {
                    tΔ1.Fatalf("%s: MarshalIndent:\n\tgot:  %s\n\twant: %s"u8, Ꮡtt.Value.Where, stripWhitespace(gotΔ1), stripWhitespace(Ꮡtt.Value.want));
                }
            }
            // Verify that it round-trips.
            ref var s2 = ref heap(new StringTag(), out var Ꮡs2);
            {
                var errΔ1 = Unmarshal(got, Ꮡs2); if (errΔ1 != default!) {
                    tΔ1.Fatalf("%s: Decode error: %v"u8, Ꮡtt.Value.Where, errΔ1);
                }
            }
            if (!reflect.DeepEqual(s2, Ꮡtt.Value.@in)) {
                tΔ1.Fatalf("%s: Decode:\n\tinput: %s\n\tgot:  %#v\n\twant: %#v"u8, Ꮡtt.Value.Where, indentNewlines(((@string)got)), s2, Ꮡtt.Value.@in);
            }
        });
    }
}

internal partial struct renamedByte /*num:byte*/;

internal partial struct renamedByteSlice /*[]byte*/;

internal partial struct renamedRenamedByteSlice /*[]renamedByte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ywJjˢ = @"""YWJj"""u8;

public static void TestEncodeRenamedByteSlice(ж<testing.T> Ꮡt) {
    var s = ((renamedByteSlice)slice<byte>((@string)"abc"u8));
    var (got, err) = Marshal(s);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    @string want = ywJjˢ;
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
    var r = ((renamedRenamedByteSlice)widen<byte, renamedByte>(slice<byte>((@string)"abc"u8), elemᴛ0 => (renamedByte)elemᴛ0));
    (got, err) = Marshal(r);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

public partial struct SamePointerNoCycle {
    public ж<SamePointerNoCycle> Ptr1, Ptr2;
}

internal static ж<SamePointerNoCycle> samePointerNoCycle = Ꮡ(new SamePointerNoCycle(nil));

public partial struct PointerCycle {
    public ж<PointerCycle> Ptr;
}

internal static ж<PointerCycle> pointerCycle = Ꮡ(new PointerCycle(nil));

public partial struct PointerCycleIndirect {
    public slice<any> Ptrs;
}

public partial struct RecursiveSlice /*[]RecursiveSlice*/;

internal static ж<PointerCycleIndirect> pointerCycleIndirect = Ꮡ(new PointerCycleIndirect(nil));
internal static map<@string, any> mapCycle = new map<@string, any>();
internal static slice<any> sliceCycle = new any[]{default!}.slice();
internal static slice<any> sliceNoCycle = new any[]{default!, default!}.slice();
internal static slice<RecursiveSlice> recursiveSliceCycle = new RecursiveSlice[]{default!}.slice();

[GoInit] internal static void init() {
    var ptr = Ꮡ(new SamePointerNoCycle(nil));
    samePointerNoCycle.Value.Ptr1 = ptr;
    samePointerNoCycle.Value.Ptr2 = ptr;
    pointerCycle.Value.Ptr = pointerCycle;
    pointerCycleIndirect.Value.Ptrs = new any[]{pointerCycleIndirect.OrTypedNil()}.slice();
    mapCycle["x"u8] = mapCycle;
    sliceCycle[0] = sliceCycle;
    sliceNoCycle[1] = sliceNoCycle[..1];
    for (nint i = startDetectingCyclesAfter; i > 0; i--) {
        sliceNoCycle = new any[]{sliceNoCycle}.slice();
    }
    recursiveSliceCycle[0] = recursiveSliceCycle;
}

public static void TestSamePointerNoCycle(ж<testing.T> Ꮡt) {
    {
        var (_, err) = Marshal(samePointerNoCycle.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatalf("Marshal error: %v"u8, err);
        }
    }
}

public static void TestSliceNoCycle(ж<testing.T> Ꮡt) {
    {
        var (_, err) = Marshal(sliceNoCycle); if (err != default!) {
            Ꮡt.Fatalf("Marshal error: %v"u8, err);
        }
    }
}

internal partial struct TestUnsupportedValues_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
    internal any @in;
}

public static void TestUnsupportedValues(ж<testing.T> Ꮡt) {
    var tests = new TestUnsupportedValues_tests[]{
        new(Name(""u8), math.NaN()),
        new(Name(""u8), math.Inf(-1)),
        new(Name(""u8), math.Inf(1)),
        new(Name(""u8), pointerCycle.OrTypedNil()),
        new(Name(""u8), pointerCycleIndirect.OrTypedNil()),
        new(Name(""u8), mapCycle),
        new(Name(""u8), sliceCycle),
        new(Name(""u8), recursiveSliceCycle)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestUnsupportedValues_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            {
                var (_, err) = Marshal(ttʗ1.@in); if (err != default!){
                    {
                        var (_, ok) = err._<ж<global::go.encoding.json_package.UnsupportedValueError>>(ᐧ); if (!ok) {
                            tΔ1.Errorf("%s: Marshal error:\n\tgot:  %T\n\twant: %T"u8, ttʗ1.Where, err, @new<global::go.encoding.json_package.UnsupportedValueError>());
                        }
                    }
                } else {
                    tΔ1.Errorf("%s: Marshal error: got nil, want non-nil"u8, ttʗ1.Where);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tfNaN1TfNaN1ˢ = @"{""TF:NaN"":""1"",""TF:NaN"":""1""}"u8;

// Issue 43207
public static void TestMarshalTextFloatMap(ж<testing.T> Ꮡt) {
    var m = new map<textfloat, @string>{
        [((textfloat)math.NaN())] = "1"u8,
        [((textfloat)math.NaN())] = "1"u8
    };
    var (got, err) = Marshal(m);
    if (err != default!) {
        Ꮡt.Errorf("Marshal error: %v"u8, err);
    }
    @string want = tfNaN1TfNaN1ˢ;
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

public partial struct Ref /*num:nint*/;

public static (slice<byte>, error) MarshalJSON(this ref Ref _) {
    return (slice<byte>(@"""ref"""u8), default!);
}

public static error UnmarshalJSON(this ref Ref r, slice<byte> _) {
    r = 12;
    return default!;
}

public partial struct Val /*num:nint*/;

public static (slice<byte>, error) MarshalJSON(this Val _) {
    return (slice<byte>(@"""val"""u8), default!);
}

public partial struct RefText /*num:nint*/;

public static (slice<byte>, error) MarshalText(this ref RefText _) {
    return (slice<byte>(@"""ref"""u8), default!);
}

public static error UnmarshalText(this ref RefText r, slice<byte> _) {
    r = 13;
    return default!;
}

public partial struct ValText /*num:nint*/;

public static (slice<byte>, error) MarshalText(this ValText _) {
    return (slice<byte>(@"""val"""u8), default!);
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
internal static readonly @string wantᶜ5 = @"{""R0"":""ref"",""R1"":""ref"",""R2"":""\""ref\"""",""R3"":""\""ref\"""",""V0"":""val"",""V1"":""val"",""V2"":""\""val\"""",""V3"":""\""val\""""}"u8;

public static void TestRefValMarshal(ж<testing.T> Ꮡt) {
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
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    {
        @string got = ((@string)b); if (got != want) {
            Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
        }
    }
}

public partial struct C /*num:nint*/;

public static (slice<byte>, error) MarshalJSON(this C _) {
    return (slice<byte>(@"""<&>"""u8), default!);
}

public partial struct CText /*num:nint*/;

public static (slice<byte>, error) MarshalText(this CText _) {
    return (slice<byte>(@"""<&>"""u8), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string u003cU0026U003eˢ = @"""\u003c\u0026\u003e"""u8;
internal static readonly @string u003cU0026U003eˢ2 = @"""\""\u003c\u0026\u003e\"""""u8;

public static void TestMarshalerEscaping(ж<testing.T> Ꮡt) {
    C c = default!;
    @string want = u003cU0026U003eˢ;
    var (b, err) = Marshal(c);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    {
        @string got = ((@string)b); if (got != want) {
            Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
        }
    }
    CText ct = default!;
    want = u003cU0026U003eˢ2;
    (b, err) = Marshal(ct);
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    {
        @string got = ((@string)b); if (got != want) {
            Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ambiguousFieldˢ = "AmbiguousField"u8;
internal static readonly @string dominantFieldˢ = "DominantField"u8;
internal static readonly @string unexportedEmbeddedIntˢ = "UnexportedEmbeddedInt"u8;
internal static readonly @string exportedEmbeddedIntˢ = "ExportedEmbeddedInt"u8;
internal static readonly @string embeddedStructˢ = "EmbeddedStruct"u8;
internal static readonly @string embeddedStructPointerˢ = "EmbeddedStructPointer"u8;
internal static readonly @string nestedStructAndIntsˢ = "NestedStructAndInts"u8;

internal partial struct TestAnonymousFields_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
    internal Func<any> makeInput;  // Function to create input value
    internal @string want;    // Expected JSON output
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
    public partial ref TestAnonymousFields_S1 S1 { get; }
    public partial ref TestAnonymousFields_S2 S2 { get; }
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
    public partial ref TestAnonymousFields_S1ᴛ1 S1 { get; }
    public partial ref TestAnonymousFields_S2ᴛ1 S2 { get; }
    internal nint x;
    public nint X;
}

internal partial struct TestAnonymousFields_myInt /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ2 /*dyn*/ {
    internal partial ref TestAnonymousFields_myInt myInt { get; }
}

internal partial struct TestAnonymousFields_MyInt /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ3 /*dyn*/ {
    public partial ref TestAnonymousFields_MyInt MyInt { get; }
}

internal partial struct TestAnonymousFields_myIntᴛ1 /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ4 /*dyn*/ {
    internal partial ref ж<TestAnonymousFields_myIntᴛ1> myInt { get; }
}

internal partial struct TestAnonymousFields_MyIntᴛ1 /*num:nint*/;

internal partial struct TestAnonymousFields_Sᴛ5 /*dyn*/ {
    public partial ref ж<TestAnonymousFields_MyIntᴛ1> MyInt { get; }
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
    internal partial ref TestAnonymousFields_s1 s1 { get; }
    public partial ref TestAnonymousFields_S2ᴛ2 S2 { get; }
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
    internal partial ref ж<TestAnonymousFields_s1ᴛ1> s1 { get; }
    public partial ref ж<TestAnonymousFields_S2ᴛ3> S2 { get; }
}

internal partial struct TestAnonymousFields_MyInt1 /*num:nint*/;

internal partial struct TestAnonymousFields_MyInt2 /*num:nint*/;

internal partial struct TestAnonymousFields_myIntᴛ2 /*num:nint*/;

internal partial struct TestAnonymousFields_s2 /*dyn*/ {
    public partial ref TestAnonymousFields_MyInt2 MyInt2 { get; }
    internal partial ref TestAnonymousFields_myIntᴛ2 myInt { get; }
}

internal partial struct TestAnonymousFields_s1ᴛ2 /*dyn*/ {
    public partial ref TestAnonymousFields_MyInt1 MyInt1 { get; }
    internal partial ref TestAnonymousFields_myIntᴛ2 myInt { get; }
    internal partial ref TestAnonymousFields_s2 s2 { get; }
}

internal partial struct TestAnonymousFields_Sᴛ8 /*dyn*/ {
    internal partial ref TestAnonymousFields_s1ᴛ2 s1 { get; }
    internal partial ref TestAnonymousFields_myIntᴛ2 myInt { get; }
}

internal partial struct TestAnonymousFields_S2ᴛ4 /*dyn*/ {
    public @string Field;
}

internal partial struct TestAnonymousFields_Sᴛ9 /*dyn*/ {
    public partial ref ж<TestAnonymousFields_S2ᴛ4> S2 { get; }
}

public static void TestAnonymousFields(ж<testing.T> Ꮡt) {
    var tests = new TestAnonymousFields_tests[]{new(
        CaseName: Name(ambiguousFieldˢ), // Both S1 and S2 have a field named X. From the perspective of S,
 // it is ambiguous which one X refers to.
 // This should not serialize either field.

        makeInput: () => {
            return new TestAnonymousFields_S(new TestAnonymousFields_S1(1, 2), new TestAnonymousFields_S2(3, 4));
        },
        want: @"{}"u8
    ), new(
        CaseName: Name(dominantFieldˢ), // Both S1 and S2 have a field named X, but since S has an X field as
 // well, it takes precedence over S1.X and S2.X.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ1(new TestAnonymousFields_S1ᴛ1(1, 2), new TestAnonymousFields_S2ᴛ1(3, 4), 5, 6);
        },
        want: @"{""X"":6}"u8
    ), new(
        CaseName: Name(unexportedEmbeddedIntˢ), // Unexported embedded field of non-struct type should not be serialized.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ2(5);
        },
        want: @"{}"u8
    ), new(
        CaseName: Name(exportedEmbeddedIntˢ), // Exported embedded field of non-struct type should be serialized.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ3(5);
        },
        want: @"{""MyInt"":5}"u8
    ), new(
        CaseName: Name("UnexportedEmbeddedIntPointer"u8), // Unexported embedded field of pointer to non-struct type
 // should not be serialized.

        makeInput: () => {
            var s = new TestAnonymousFields_Sᴛ4(@new<TestAnonymousFields_myIntᴛ1>());
            s.myInt.Value = 5;
            return s;
        },
        want: @"{}"u8
    ), new(
        CaseName: Name("ExportedEmbeddedIntPointer"u8), // Exported embedded field of pointer to non-struct type
 // should be serialized.

        makeInput: () => {
            var s = new TestAnonymousFields_Sᴛ5(@new<TestAnonymousFields_MyIntᴛ1>());
            s.MyInt.Value = 5;
            return s;
        },
        want: @"{""MyInt"":5}"u8
    ), new(
        CaseName: Name(embeddedStructˢ), // Exported fields of embedded structs should have their
 // exported fields be serialized regardless of whether the struct types
 // themselves are exported.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ6(new TestAnonymousFields_s1(1, 2), new TestAnonymousFields_S2ᴛ2(3, 4));
        },
        want: @"{""X"":2,""Y"":4}"u8
    ), new(
        CaseName: Name(embeddedStructPointerˢ), // Exported fields of pointers to embedded structs should have their
 // exported fields be serialized regardless of whether the struct types
 // themselves are exported.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ7(Ꮡ(new TestAnonymousFields_s1ᴛ1(1, 2)), Ꮡ(new TestAnonymousFields_S2ᴛ3(3, 4)));
        },
        want: @"{""X"":2,""Y"":4}"u8
    ), new(
        CaseName: Name(nestedStructAndIntsˢ), // Exported fields on embedded unexported structs at multiple levels
 // of nesting should still be serialized.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ8(new TestAnonymousFields_s1ᴛ2(1, 2, new TestAnonymousFields_s2(3, 4)), 6);
        },
        want: @"{""MyInt1"":1,""MyInt2"":3}"u8
    ), new(
        CaseName: Name("EmbeddedFieldBehindNilPointer"u8), // If an anonymous struct pointer field is nil, we should ignore
 // the embedded fields behind it. Not properly doing so may
 // result in the wrong output or reflect panics.

        makeInput: () => {
            return new TestAnonymousFields_Sᴛ9(nil);
        },
        want: @"{}"u8
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestAnonymousFields_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.makeInput());
            if (err != default!) {
                tΔ1.Fatalf("%s: Marshal error: %v"u8, ttʗ1.Where, err);
            }
            if (((sstring)b) != ttʗ1.want) {
                tΔ1.Fatalf("%s: Marshal:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, b, ttʗ1.want);
            }
        });
    }
}

public partial struct BugA {
    public @string S;
}

public partial struct BugB {
    public partial ref BugA BugA { get; }
    public @string S;
}

public partial struct BugC {
    public @string S;
}

// Legal Go: We never use the repeated embedded field (S).
public partial struct BugX {
    public nint A;
    public partial ref BugA BugA { get; }
    public partial ref BugB BugB { get; }
}

internal partial struct nilJSONMarshaler /*@string*/;

internal static (slice<byte>, error) MarshalJSON(this ж<nilJSONMarshaler> Ꮡnm) {
    ref var nm = ref Ꮡnm.DerefOrNull();

    if (Ꮡnm == nil) {
        return Marshal((@string)"0zenil0"u8);
    }
    return Marshal("zenil:" + ((@string)(nm)));
}

internal partial struct nilTextMarshaler /*@string*/;

internal static (slice<byte>, error) MarshalText(this ж<nilTextMarshaler> Ꮡnm) {
    ref var nm = ref Ꮡnm.DerefOrNull();

    if (Ꮡnm == nil) {
        return (slice<byte>("0zenil0"u8), default!);
    }
    return (slice<byte>("zenil:" + ((@string)(nm))), default!);
}

internal partial struct TestNilMarshal_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
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
public static void TestNilMarshal(ж<testing.T> Ꮡt) {
    var tests = new TestNilMarshal_tests[]{
        new(Name(""u8), default!, @"null"u8),
        new(Name(""u8), @new<float64>(), @"0"u8),
        new(Name(""u8), slice<any>(default!), @"null"u8),
        new(Name(""u8), slice<@string>(default!), @"null"u8),
        new(Name(""u8), ((map<@string, @string>)default!), @"null"u8),
        new(Name(""u8), slice<byte>(default!), @"null"u8),
        new(Name(""u8), new TestNilMarshal_type("gopher"u8), @"{""M"":""gopher""}"u8),
        new(Name(""u8), new TestNilMarshal_typeᴛ1(), @"{""M"":null}"u8),
        new(Name(""u8), new TestNilMarshal_typeᴛ1(new json_internal_test_package.nilJSONMarshalerжMarshaler(((ж<nilJSONMarshaler>)nil))), @"{""M"":""0zenil0""}"u8),
        new(Name(""u8), new TestNilMarshal_typeᴛ2(((ж<nilJSONMarshaler>)nil)), @"{""M"":null}"u8),
        new(Name(""u8), new TestNilMarshal_typeᴛ3(), @"{""M"":null}"u8),
        new(Name(""u8), new TestNilMarshal_typeᴛ3(new json_internal_test_package.nilTextMarshalerжTextMarshaler(((ж<nilTextMarshaler>)nil))), @"{""M"":""0zenil0""}"u8),
        new(Name(""u8), new TestNilMarshal_typeᴛ2(((ж<nilTextMarshaler>)nil)), @"{""M"":null}"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestNilMarshal_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            {
                var (got, err) = Marshal(ttʗ1.@in);
                switch (ᐧ) {
                case {} when err != default!: {
                    tΔ1.Fatalf("%s: Marshal error: %v"u8, ttʗ1.Where, err);
                    break;
                }
                case {} when ((sstring)got) != ttʗ1.want: {
                    tΔ1.Fatalf("%s: Marshal:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, got, ttʗ1.want);
                    break;
                }}
            }

        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object marshalErrorˢ = (@string)"Marshal error:"u8;
internal static readonly @string a23ˢ = @"{""A"":23}"u8;

// Issue 5245.
public static void TestEmbeddedBug(ж<testing.T> Ꮡt) {
    var v = new BugB(
        new BugA("A"u8),
        "B"u8
    );
    var (b, err) = Marshal(v);
    if (err != default!) {
        Ꮡt.Fatal(marshalErrorˢ, err);
    }
    @string want = @"{""S"":""B""}"u8;
    @string got = ((@string)b);
    if (got != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
    // Now check that the duplicate field, S, does not appear.
    var x = new BugX(
        A: 23
    );
    (b, err) = Marshal(x);
    if (err != default!) {
        Ꮡt.Fatal(marshalErrorˢ, err);
    }
    want = a23ˢ;
    got = ((@string)b);
    if (got != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

public partial struct BugD {
// Same as BugA after tagging.
    public @string XXX; /*`json:"S"`*/
}

// BugD's tagged S field should dominate BugA's.
public partial struct BugY {
    public partial ref BugA BugA { get; }
    public partial ref BugD BugD { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sBugDˢ = @"{""S"":""BugD""}"u8;

// Test that a field with a tag dominates untagged fields.
public static void TestTaggedFieldDominates(ж<testing.T> Ꮡt) {
    var v = new BugY(
        new BugA("BugA"u8),
        new BugD("BugD"u8)
    );
    var (b, err) = Marshal(v);
    if (err != default!) {
        Ꮡt.Fatal(marshalErrorˢ, err);
    }
    @string want = sBugDˢ;
    @string got = ((@string)b);
    if (got != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

// There are no tags here, so S should not appear.
public partial struct BugZ {
    public partial ref BugA BugA { get; }
    public partial ref BugC BugC { get; }
    public partial ref BugY BugY { get; } // Contains a tagged S field through BugD; should not dominate.
}

public static void TestDuplicatedFieldDisappears(ж<testing.T> Ꮡt) {
    var v = new BugZ(
        new BugA("BugA"u8),
        new BugC("BugC"u8),
        new BugY(
            new BugA("nested BugA"u8),
            new BugD("nested BugD"u8)
        )
    );
    var (b, err) = Marshal(v);
    if (err != default!) {
        Ꮡt.Fatal(marshalErrorˢ, err);
    }
    @string want = @"{}"u8;
    @string got = ((@string)b);
    if (got != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidˢ = @"invalid"u8;

internal partial struct TestIssue10281_Foo /*dyn*/ {
    public global::go.encoding.json_package.Number N;
}

public static void TestIssue10281(ж<testing.T> Ꮡt) {
    ref var x = ref heap<TestIssue10281_Foo>(out var Ꮡx);
    x = new TestIssue10281_Foo(((global::go.encoding.json_package.Number)(@string)invalidˢ));
    {
        var (_, err) = Marshal(Ꮡx); if (err == default!) {
            Ꮡt.Fatalf("Marshal error: got nil, want non-nil"u8);
        }
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

public static void TestMarshalErrorAndReuseEncodeState(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Disable the GC temporarily to prevent encodeState's in Pool being cleaned away during the test.
        nint percent = debug.SetGCPercent(-1);
        defer(debug.SetGCPercent, percent, ref ᒐ);
        ref var dummy = ref heap<TestMarshalErrorAndReuseEncodeState_Dummy>(out var Ꮡdummy);
        dummy = new TestMarshalErrorAndReuseEncodeState_Dummy(Name: "Dummy"u8);
        dummy.Next = Ꮡdummy;
        {
            var (_, errΔ1) = Marshal(dummy); if (errΔ1 == default!) {
                Ꮡt.Errorf("Marshal error: got nil, want non-nil"u8);
            }
        }
        var want = new TestMarshalErrorAndReuseEncodeState_Data(A: "a"u8, I: 1);
        var (b, err) = Marshal(want);
        if (err != default!) {
            Ꮡt.Errorf("Marshal error: %v"u8, err);
        }
        ref var got = ref heap(new TestMarshalErrorAndReuseEncodeState_Data(), out var Ꮡgot);
        {
            var errΔ2 = Unmarshal(b, Ꮡgot); if (errΔ2 != default!) {
                Ꮡt.Errorf("Unmarshal error: %v"u8, errΔ2);
            }
        }
        if (got != want) {
            Ꮡt.Errorf("Unmarshal:\n\tgot:  %v\n\twant: %v"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestHTMLEscape(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    bytes.Buffer want = default!;
    @string m = @"{""M"":""<html>foo &"u8 + ((@string)(new byte[]{0xe2, 0x80, 0xa8, 0x20, 0xe2, 0x80, 0xa9})) + @"</html>""}"u8;
    want.Write(slice<byte>(@"{""M"":""\u003chtml\u003efoo \u0026\u2028 \u2029\u003c/html\u003e""}"u8));
    HTMLEscape(Ꮡb, slice<byte>(m));
    if (!bytes.Equal(b.Bytes(), want.Bytes())) {
        Ꮡt.Errorf("HTMLEscape:\n\tgot:  %s\n\twant: %s"u8, b.Bytes(), want.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string n42ˢ = @"{""n"":""42""}"u8;

internal partial struct TestEncodePointerString_stringPointer /*dyn*/ {
    public ж<int64> N; /*`json:"n,string"`*/
}

// golang.org/issue/8582
public static void TestEncodePointerString(ж<testing.T> Ꮡt) {
    ref var n = ref heap(new int64(), out var Ꮡn);
    n = 42;
    var (b, err) = Marshal(new TestEncodePointerString_stringPointer(N: Ꮡn));
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    {
        @string got = ((@string)b);
        @string want = n42ˢ; if (got != want) {
            Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
        }
    }
    ref var back = ref heap(new TestEncodePointerString_stringPointer(), out var Ꮡback);
    {
        err = Unmarshal(b, Ꮡback);
        switch (ᐧ) {
        case {} when err != default!: {
            Ꮡt.Fatalf("Unmarshal error: %v"u8, err);
            break;
        }
        case {} when back.N == nil: {
            Ꮡt.Fatalf("Unmarshal: back.N = nil, want non-nil"u8);
            break;
        }
        case {} when back.N.Value is not 42: {
            Ꮡt.Fatalf("Unmarshal: *back.N = %d, want 42"u8, back.N.Value);
            break;
        }}
    }

}


partial struct encodeStringTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
}
internal static slice<encodeStringTestsᴛ1> encodeStringTests = new encodeStringTestsᴛ1[]{
    new("\x00"u8, @"""\u0000"""u8),
    new("\x01"u8, @"""\u0001"""u8),
    new("\x02"u8, @"""\u0002"""u8),
    new("\x03"u8, @"""\u0003"""u8),
    new("\x04"u8, @"""\u0004"""u8),
    new("\x05"u8, @"""\u0005"""u8),
    new("\x06"u8, @"""\u0006"""u8),
    new("\x07"u8, @"""\u0007"""u8),
    new("\x08"u8, @"""\b"""u8),
    new("\x09"u8, @"""\t"""u8),
    new("\x0a"u8, @"""\n"""u8),
    new("\x0b"u8, @"""\u000b"""u8),
    new("\x0c"u8, @"""\f"""u8),
    new("\x0d"u8, @"""\r"""u8),
    new("\x0e"u8, @"""\u000e"""u8),
    new("\x0f"u8, @"""\u000f"""u8),
    new("\x10"u8, @"""\u0010"""u8),
    new("\x11"u8, @"""\u0011"""u8),
    new("\x12"u8, @"""\u0012"""u8),
    new("\x13"u8, @"""\u0013"""u8),
    new("\x14"u8, @"""\u0014"""u8),
    new("\x15"u8, @"""\u0015"""u8),
    new("\x16"u8, @"""\u0016"""u8),
    new("\x17"u8, @"""\u0017"""u8),
    new("\x18"u8, @"""\u0018"""u8),
    new("\x19"u8, @"""\u0019"""u8),
    new("\x1a"u8, @"""\u001a"""u8),
    new("\x1b"u8, @"""\u001b"""u8),
    new("\x1c"u8, @"""\u001c"""u8),
    new("\x1d"u8, @"""\u001d"""u8),
    new("\x1e"u8, @"""\u001e"""u8),
    new("\x1f"u8, @"""\u001f"""u8)
}.slice();

public static void TestEncodeString(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in encodeStringTests) {
        var (b, err) = Marshal(tt.@in);
        if (err != default!) {
            Ꮡt.Errorf("Marshal(%q) error: %v"u8, tt.@in, err);
            continue;
        }
        @string @out = ((@string)b);
        if (@out != tt.@out) {
            Ꮡt.Errorf("Marshal(%q) = %#q, want %#q"u8, tt.@in, @out, tt.@out);
        }
    }
}

internal partial struct jsonbyte /*num:byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jbDˢ = @"{""JB"":%d}"u8;

internal static (slice<byte>, error) MarshalJSON(this jsonbyte b) {
    return tenc(jbDˢ, b);
}

internal partial struct textbyte /*num:byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tbDˢ = @"TB:%d"u8;

internal static (slice<byte>, error) MarshalText(this textbyte b) {
    return tenc(tbDˢ, b);
}

internal partial struct jsonint /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jiDˢ = @"{""JI"":%d}"u8;

internal static (slice<byte>, error) MarshalJSON(this jsonint i) {
    return tenc(jiDˢ, i);
}

internal partial struct textint /*num:nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tiDˢ = @"TI:%d"u8;

internal static (slice<byte>, error) MarshalText(this textint i) {
    return tenc(tiDˢ, i);
}

internal static (slice<byte>, error) tenc(@string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    fmt.Fprintf(new json_test_package.bytes_BufferжWriter(Ꮡbuf), format, a.ꓸꓸꓸ);
    return (buf.Bytes(), default!);
}

internal partial struct textfloat /*num:float64*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tf02fˢ = @"TF:%0.2f"u8;

internal static (slice<byte>, error) MarshalText(this textfloat f) {
    return tenc(tf02fˢ, f);
}

internal partial struct TestEncodeBytekind_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
    internal any @in;
    internal @string want;
}

// Issue 13783
public static void TestEncodeBytekind(ж<testing.T> Ꮡt) {
    var tests = new TestEncodeBytekind_tests[]{
        new(Name(""u8), (byte)7, "7"u8),
        new(Name(""u8), ((jsonbyte)7), @"{""JB"":7}"u8),
        new(Name(""u8), ((textbyte)4), @"""TB:4"""u8),
        new(Name(""u8), ((jsonint)5), @"{""JI"":5}"u8),
        new(Name(""u8), ((textint)1), @"""TI:1"""u8),
        new(Name(""u8), new byte[]{0, 1}.slice(), @"""AAE="""u8),
        new(Name(""u8), new jsonbyte[]{0, 1}.slice(), @"[{""JB"":0},{""JB"":1}]"u8),
        new(Name(""u8), new slice<jsonbyte>[]{new jsonbyte[]{0, 1}.slice(), new jsonbyte[]{3}.slice()}.slice(), @"[[{""JB"":0},{""JB"":1}],[{""JB"":3}]]"u8),
        new(Name(""u8), new textbyte[]{2, 3}.slice(), @"[""TB:2"",""TB:3""]"u8),
        new(Name(""u8), new jsonint[]{5, 4}.slice(), @"[{""JI"":5},{""JI"":4}]"u8),
        new(Name(""u8), new textint[]{9, 3}.slice(), @"[""TI:9"",""TI:3""]"u8),
        new(Name(""u8), new nint[]{9, 3}.slice(), @"[9,3]"u8),
        new(Name(""u8), new textfloat[]{12D, 3D}.slice(), @"[""TF:12.00"",""TF:3.00""]"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestEncodeBytekind_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.@in);
            if (err != default!) {
                tΔ1.Errorf("%s: Marshal error: %v"u8, ttʗ1.Where, err);
            }
            @string got = ((@string)b);
            @string want = ttʗ1.want;
            if (got != want) {
                tΔ1.Errorf("%s: Marshal:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, got, want);
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ6 = @"{""a:z"":3,""x:y"":1,""y:x"":2,""z:a"":4}"u8;

public static void TestTextMarshalerMapKeysAreSorted(ж<testing.T> Ꮡt) {
    var (got, err) = Marshal(new map<unmarshalerText, nint>{
        [new("x"u8, "y"u8)] = 1,
        [new("y"u8, "x"u8)] = 2,
        [new("a"u8, "z"u8)] = 3,
        [new("z"u8, "a"u8)] = 4
    });
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    @string want = wantᶜ6;
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ7 = @"{"""":1,""A:B"":2}"u8;

// https://golang.org/issue/33675
public static void TestNilMarshalerTextMapKey(ж<testing.T> Ꮡt) {
    var (got, err) = Marshal(new map<ж<unmarshalerText>, nint>{
        [((ж<unmarshalerText>)nil)] = 1,
        [Ꮡ(new unmarshalerText("A"u8, "B"u8))] = 2
    });
    if (err != default!) {
        Ꮡt.Fatalf("Marshal error: %v"u8, err);
    }
    @string want = wantᶜ7;
    if (((sstring)got) != want) {
        Ꮡt.Errorf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

internal static Func<@string, ж<regexp.Regexp>> re = regexp.MustCompile;

// no binary exponential notation
// no leading + sign
// no unnecessary leading zeros
// leading zero required before decimal point
// no trailing decimal
// no trailing zero in fraction
// exponential notation must have normalized mantissa
// positive exponent must be signed
// exponent must not have leading zeros
// not tiny enough for exponential notation
// not big enough for exponential notation
// too tiny, should use exponential notation
// too big, should use exponential notation
// too many significant digits in integer
// too many significant digits in decimal
// below here for float32 only
// too many significant digits in integer
// too many significant digits in decimal
// syntactic checks on form of marshaled floating point numbers.
internal static slice<ж<regexp.Regexp>> badFloatREs = new ж<regexp.Regexp>[]{
    re(@"p"u8),
    re(@"^\+"u8),
    re(@"^-?0[^.]"u8),
    re(@"^-?\."u8),
    re(@"\.(e|$)"u8),
    re(@"\.[0-9]+0(e|$)"u8),
    re(@"^-?(0|[0-9]{2,})\..*e"u8),
    re(@"e[0-9]"u8),
    re(@"e[+-]0"u8),
    re(@"e-[1-6]$"u8),
    re(@"e+(.|1.|20)$"u8),
    re(@"^-?0\.0000000"u8),
    re(@"^-?[0-9]{22}"u8),
    re(@"[1-9][0-9]{16}[1-9]"u8),
    re(@"[1-9][0-9.]{17}[1-9]"u8),
    re(@"[1-9][0-9]{8}[1-9]"u8),
    re(@"[1-9][0-9.]{9}[1-9]"u8)
}.slice();

public static void TestMarshalFloat(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    nint nfail = 0;
    void test(float64 f, nint bits) {
        var vf = ((any)f);
        if (bits == 32) {
            f = (float64)(float32)f; // round
            vf = (float32)f;
        }
        var (bout, err) = Marshal(vf);
        if (err != default!) {
            Ꮡt.Errorf("Marshal(%T(%g)) error: %v"u8, vf, vf, err);
            nfail++;
            return;
        }
        @string @out = ((@string)bout);
        // result must convert back to the same float
        (var g, err) = strconv.ParseFloat(@out, bits);
        if (err != default!) {
            Ꮡt.Errorf("ParseFloat(%q) error: %v"u8, @out, err);
            nfail++;
            return;
        }
        if (f != g || fmt.Sprint(f) != fmt.Sprint(g)) {
            // fmt.Sprint handles ±0
            Ꮡt.Errorf("ParseFloat(%q):\n\tgot:  %g\n\twant: %g"u8, @out, (float32)g, vf);
            nfail++;
            return;
        }
        var bad = badFloatREs;
        if (bits == 64) {
            bad = bad.slice(0, len(bad) - 2);
        }
        foreach (var (_, re) in bad) {
            if (re.MatchString(@out)) {
                Ꮡt.Errorf("Marshal(%T(%g)) = %q; must not match /%s/"u8, vf, vf, @out, re.OrTypedNil());
                nfail++;
                return;
            }
        }
    }
    float64 bigger = math.Inf(+1);
    float64 smaller = math.Inf(-1);
    @string digits = "1.2345678901234567890123"u8;
    for (nint i = len(digits); i >= 2; i--) {
        if (testing.Short() && i < len(digits) - 4) {
            break;
        }
        for (nint exp = -30; exp <= 30; exp++) {
            foreach (var (_, sign) in (@string)"+-"u8) {
                for (nint bits = 32; bits <= 64; bits += 32) {
                    @string s = fmt.Sprintf("%c%se%d"u8, sign, digits.slice(0, i), exp);
                    var (f, err) = strconv.ParseFloat(s, bits);
                    if (err != default!) {
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
                        Ꮡt.Fatalf("stopping test early"u8);
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
    public partial ref CaseName CaseName { get; }
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

public static void TestMarshalRawMessageValue(ж<testing.T> Ꮡt) {
    ref var rawNil = ref heap<global::go.encoding.json_package.RawMessage>(out var ᏑrawNil);

    rawNil = ((global::go.encoding.json_package.RawMessage)default!);
    ref var rawEmpty = ref heap<global::go.encoding.json_package.RawMessage>(out var ᏑrawEmpty);

    rawEmpty = ((global::go.encoding.json_package.RawMessage)new byte[]{}.slice());
    ref var rawText = ref heap<global::go.encoding.json_package.RawMessage>(out var ᏑrawText);

    rawText = ((global::go.encoding.json_package.RawMessage)slice<byte>(@"""foo"""u8));
    var tests = new TestMarshalRawMessageValue_tests[]{ // Test with nil RawMessage.

        new(Name(""u8), rawNil, "null"u8, true),
        new(Name(""u8), ᏑrawNil, "null"u8, true),
        new(Name(""u8), new any[]{rawNil}.slice(), "[null]"u8, true),
        new(Name(""u8), Ꮡ(new any[]{rawNil}.slice()), "[null]"u8, true),
        new(Name(""u8), new any[]{ᏑrawNil}.slice(), "[null]"u8, true),
        new(Name(""u8), Ꮡ(new any[]{ᏑrawNil}.slice()), "[null]"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_type(rawNil), @"{""M"":null}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_type(rawNil)), @"{""M"":null}"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ1(ᏑrawNil), @"{""M"":null}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ1(ᏑrawNil)), @"{""M"":null}"u8, true),
        new(Name(""u8), new map<@string, any>{["M"u8] = rawNil}, @"{""M"":null}"u8, true),
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"u8] = rawNil}), @"{""M"":null}"u8, true),
        new(Name(""u8), new map<@string, any>{["M"u8] = ᏑrawNil}, @"{""M"":null}"u8, true),
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"u8] = ᏑrawNil}), @"{""M"":null}"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_T1(rawNil), "{}"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_T2(ᏑrawNil), @"{""M"":null}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T1(rawNil)), "{}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T2(ᏑrawNil)), @"{""M"":null}"u8, true), // Test with empty, but non-nil, RawMessage.

        new(Name(""u8), rawEmpty, ""u8, false),
        new(Name(""u8), ᏑrawEmpty, ""u8, false),
        new(Name(""u8), new any[]{rawEmpty}.slice(), ""u8, false),
        new(Name(""u8), Ꮡ(new any[]{rawEmpty}.slice()), ""u8, false),
        new(Name(""u8), new any[]{ᏑrawEmpty}.slice(), ""u8, false),
        new(Name(""u8), Ꮡ(new any[]{ᏑrawEmpty}.slice()), ""u8, false),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ2(rawEmpty), ""u8, false),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ2(rawEmpty)), ""u8, false),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ3(ᏑrawEmpty), ""u8, false),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ3(ᏑrawEmpty)), ""u8, false),
        new(Name(""u8), new map<@string, any>{["nil"u8] = rawEmpty}, ""u8, false),
        new(Name(""u8), Ꮡ(new map<@string, any>{["nil"u8] = rawEmpty}), ""u8, false),
        new(Name(""u8), new map<@string, any>{["nil"u8] = ᏑrawEmpty}, ""u8, false),
        new(Name(""u8), Ꮡ(new map<@string, any>{["nil"u8] = ᏑrawEmpty}), ""u8, false),
        new(Name(""u8), new TestMarshalRawMessageValue_T1(rawEmpty), "{}"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_T2(ᏑrawEmpty), ""u8, false),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T1(rawEmpty)), "{}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T2(ᏑrawEmpty)), ""u8, false), // Test with RawMessage with some text.
 //
 // The tests below marked with Issue6458 used to generate "ImZvbyI=" instead "foo".
 // This behavior was intentionally changed in Go 1.8.
 // See https://golang.org/issues/14493#issuecomment-255857318

        new(Name(""u8), rawText, @"""foo"""u8, true), // Issue6458

        new(Name(""u8), ᏑrawText, @"""foo"""u8, true),
        new(Name(""u8), new any[]{rawText}.slice(), @"[""foo""]"u8, true), // Issue6458

        new(Name(""u8), Ꮡ(new any[]{rawText}.slice()), @"[""foo""]"u8, true), // Issue6458

        new(Name(""u8), new any[]{ᏑrawText}.slice(), @"[""foo""]"u8, true),
        new(Name(""u8), Ꮡ(new any[]{ᏑrawText}.slice()), @"[""foo""]"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_type(rawText), @"{""M"":""foo""}"u8, true), // Issue6458

        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_type(rawText)), @"{""M"":""foo""}"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_typeᴛ1(ᏑrawText), @"{""M"":""foo""}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_typeᴛ1(ᏑrawText)), @"{""M"":""foo""}"u8, true),
        new(Name(""u8), new map<@string, any>{["M"u8] = rawText}, @"{""M"":""foo""}"u8, true), // Issue6458

        new(Name(""u8), Ꮡ(new map<@string, any>{["M"u8] = rawText}), @"{""M"":""foo""}"u8, true), // Issue6458

        new(Name(""u8), new map<@string, any>{["M"u8] = ᏑrawText}, @"{""M"":""foo""}"u8, true),
        new(Name(""u8), Ꮡ(new map<@string, any>{["M"u8] = ᏑrawText}), @"{""M"":""foo""}"u8, true),
        new(Name(""u8), new TestMarshalRawMessageValue_T1(rawText), @"{""M"":""foo""}"u8, true), // Issue6458

        new(Name(""u8), new TestMarshalRawMessageValue_T2(ᏑrawText), @"{""M"":""foo""}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T1(rawText)), @"{""M"":""foo""}"u8, true),
        new(Name(""u8), Ꮡ(new TestMarshalRawMessageValue_T2(ᏑrawText)), @"{""M"":""foo""}"u8, true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestMarshalRawMessageValue_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.@in);
            {
                var ok = (err == default!); if (ok != ttʗ1.ok) {
                    if (err != default!){
                        tΔ1.Errorf("%s: Marshal error: %v"u8, ttʗ1.Where, err);
                    } else {
                        tΔ1.Errorf("%s: Marshal error: got nil, want non-nil"u8, ttʗ1.Where);
                    }
                }
            }
            {
                @string got = ((@string)b); if (got != ttʗ1.want) {
                    tΔ1.Errorf("%s: Marshal:\n\tinput: %#v\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, ttʗ1.@in, got, ttʗ1.want);
                }
            }
        });
    }
}

internal partial struct marshalPanic {
}

internal static (slice<byte>, error) MarshalJSON(this marshalPanic _) {
    throw panic((nint)(0xdead));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object marshalShouldHaveˢ = (@string)"Marshal should have panicked"u8;

public static void TestMarshalPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var got = recover(); if (!reflect.DeepEqual(got, (nint)(0xdead))) {
                    Ꮡt.Errorf("panic() = (%T)(%v), want 0xdead"u8, got, got);
                }
            }
        }, ref ᒐ);
        Marshal(Ꮡ(new marshalPanic(nil)));
        Ꮡt.Error(marshalShouldHaveˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string a000A0ˢ = @"{""A0"":0,""À"":0,""Aβ"":0}"u8;

internal partial struct TestMarshalUncommonFieldNames_v /*dyn*/ {
    public nint A0, À, Aβ;
}

public static void TestMarshalUncommonFieldNames(ж<testing.T> Ꮡt) {
    var v = new TestMarshalUncommonFieldNames_v();
    var (b, err) = Marshal(v);
    if (err != default!) {
        Ꮡt.Fatal(marshalErrorˢ, err);
    }
    @string want = a000A0ˢ;
    @string got = ((@string)b);
    if (got != want) {
        Ꮡt.Fatalf("Marshal:\n\tgot:  %s\n\twant: %s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testVariableˢ = "test variable"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string errTextᶜ = "json: test error"u8;

internal partial struct TestMarshalerError_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
    internal ж<global::go.encoding.json_package.MarshalerError> err;
    internal @string want;
}

public static void TestMarshalerError(ж<testing.T> Ꮡt) {
    @string s = testVariableˢ;
    var st = reflect.TypeOf(s);
    @string errText = errTextᶜ;
    var tests = new TestMarshalerError_tests[]{new(
        Name(""u8),
        Ꮡ(new MarshalerError(st, fmt.Errorf(errText), ""u8)),
        "json: error calling MarshalJSON for type "u8 + st.String() + ": "u8 + errText
    ), new(
        Name(""u8),
        Ꮡ(new MarshalerError(st, fmt.Errorf(errText), "TestMarshalerError"u8)),
        "json: error calling TestMarshalerError for type "u8 + st.String() + ": "u8 + errText
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestMarshalerError_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            @string got = ttʗ1.err.Error();
            if (got != ttʗ1.want) {
                tΔ1.Errorf("%s: Error:\n\tgot:  %s\n\twant: %s"u8, ttʗ1.Where, got, ttʗ1.want);
            }
        });
    }
}

internal partial struct marshaledValue /*@string*/;

internal static (slice<byte>, error) MarshalJSON(this marshaledValue v) {
    return (slice<byte>((@string)v), default!);
}

public static void TestIssue63379(ж<testing.T> Ꮡt) {
    foreach (var (_, v) in new @string[]{
        "[]<"u8,
        "[]>"u8,
        "[]&"u8,
        "[]\u2028"u8,
        "[]\u2029"u8,
        "{}<"u8,
        "{}>"u8,
        "{}&"u8,
        "{}\u2028"u8,
        "{}\u2029"u8
    }.slice()) {
        var (_, err) = Marshal(((marshaledValue)v));
        if (err == default!) {
            Ꮡt.Errorf("expected error for %q"u8, v);
        }
    }
}

} // end json_internal_test_package
