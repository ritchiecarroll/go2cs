// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests for template execution, copied from text/template.
namespace go.html;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using template = text.template_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using parse = text.template.parse_package;
using static go.html.template_package;
using text;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class template_internal_test_package {

internal static ж<bool> debug = flag.Bool("debug"u8, false, "show the errors produced by the tests"u8);

// T has lots of interesting pieces to use to test execution.
[GoType] [GoValueClone("AI")] public partial struct T {
    // Basics
    public bool True;
    public nint I;
    public uint16 U16;
    public @string X, S;
    public float64 FloatZero;
    public complex128 ComplexZero;
    // Nested structs.
    public ж<U> U;
    // Struct with String method.
    public V V0;
    public ж<V> V1, V2;
    // Struct with Error method.
    public W W0;
    public ж<W> W1, W2;
    // Slices
    public slice<nint> SI;
    public slice<nint> SICap;
    public slice<nint> SIEmpty;
    public slice<bool> SB;
    // Arrays
    public array<nint> AI = new(3);
    // Maps
    public map<@string, nint> MSI;
    public map<@string, nint> MSIone; // one element, for deterministic output
    public map<@string, nint> MSIEmpty;
    public map<any, nint> MXI;
    public map<nint, nint> MII;
    public map<int32, @string> MI32S;
    public map<int64, @string> MI64S;
    public map<uint32, @string> MUI32S;
    public map<uint64, @string> MUI64S;
    public map<int8, @string> MI8S;
    public map<uint8, @string> MUI8S;
    public slice<map<@string, nint>> SMSI;
    // Empty interfaces; used to see if we can dig inside one.
    public any Empty0; // nil
    public any Empty1;
    public any Empty2;
    public any Empty3;
    public any Empty4;
    // Non-empty interfaces.
    public I NonEmptyInterface;
    public ж<I> NonEmptyInterfacePtS;
    public I NonEmptyInterfaceNil;
    public I NonEmptyInterfaceTypedNil;
    // Stringer.
    public fmt.Stringer Str;
    public error Err;
    // Pointers
    public ж<nint> PI;
    public ж<@string> PS;
    public ж<slice<nint>> PSI;
    public ж<nint> NIL;
    // Function (not method)
    public Func<@string, @string, @string> BinaryFunc;
    public Funcꓸꓸꓸ<@string, @string> VariadicFunc;
    public Funcꓸꓸꓸ<nint, @string, @string> VariadicFuncInt;
    public Func<ж<nint>, bool> NilOKFunc;
    public Func<(@string, error)> ErrFunc;
    public Func<@string> PanicFunc;
    // Template to test evaluation of templates.
    public ж<global::go.html.template_package.Template> Tmpl;
    // Unexported field; cannot be accessed by template.
    internal nint unexported;
}

[GoType("[]@string")] public partial struct S;

public static @string Method0(this S _) {
    return "M0"u8;
}

[GoType] public partial struct U {
    public @string V;
}

[GoType] public partial struct V {
    internal nint j;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilVˢ = "nilV"u8;

public static @string String(this ж<V> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    if (Ꮡv == nil) {
        return nilVˢ;
    }
    return fmt.Sprintf("<%d>"u8, v.j);
}

[GoType] public partial struct W {
    internal nint k;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilWˢ = "nilW"u8;

public static @string Error(this ж<W> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (Ꮡw == nil) {
        return nilWˢ;
    }
    return fmt.Sprintf("[%d]"u8, w.k);
}

internal static ж<I> ᏑsiVal = new StandardBox<I>(((I)new S(new @string[]{"a"u8, "b"u8}.slice())));
internal static ref I siVal => ref ᏑsiVal.ValueSlot;

// leave V2 as nil
// leave W2 as nil
// "x" is the value of .X
internal static (ж<global::go.html.template_package.Template>, error) tupleᴛ1ʗ = New("x"u8).Parse("test template"u8);
internal static ж<T> tVal = Ꮡ(new T(
    True: true,
    I: 17,
    U16: 16,
    X: "x"u8,
    S: "xyz"u8,
    U: Ꮡ(new U("v"u8)),
    V0: new V(6666),
    V1: Ꮡ(new V(7777)),
    W0: new W(888),
    W1: Ꮡ(new W(999)),
    SI: new nint[]{3, 4, 5}.slice(),
    SICap: new slice<nint>(5, 10),
    AI: new nint[]{3, 4, 5}.array(),
    SB: new bool[]{true, false}.slice(),
    MSI: new map<@string, nint>{["one"u8] = 1, ["two"u8] = 2, ["three"u8] = 3},
    MSIone: new map<@string, nint>{["one"u8] = 1},
    MXI: new map<any, nint>{[(@string)"one"u8] = 1},
    MII: new map<nint, nint>{[1] = 1},
    MI32S: new map<int32, @string>{[1] = "one"u8, [2] = "two"u8},
    MI64S: new map<int64, @string>{[2] = "i642"u8, [3] = "i643"u8},
    MUI32S: new map<uint32, @string>{[2] = "u322"u8, [3] = "u323"u8},
    MUI64S: new map<uint64, @string>{[2] = "ui642"u8, [3] = "ui643"u8},
    MI8S: new map<int8, @string>{[2] = "i82"u8, [3] = "i83"u8},
    MUI8S: new map<uint8, @string>{[2] = "u82"u8, [3] = "u83"u8},
    SMSI: new map<@string, nint>[]{
        new map<@string, nint>{["one"u8] = 1, ["two"u8] = 2},
        new map<@string, nint>{["eleven"u8] = 11, ["twelve"u8] = 12}
    }.slice(),
    Empty1: (nint)(3),
    Empty2: (@string)"empty2"u8,
    Empty3: new nint[]{7, 8}.slice(),
    Empty4: Ꮡ(new U("UinEmpty"u8)),
    NonEmptyInterface: new template_internal_test_package.TжI(Ꮡ(new T(X: "x"u8))),
    NonEmptyInterfacePtS: ᏑsiVal,
    NonEmptyInterfaceTypedNil: new template_internal_test_package.TжI(((ж<T>)nil)),
    Str: new template_test_package.bytes_BufferжStringer(bytes.NewBuffer(slice<byte>("foozle"u8))),
    Err: errors.New("erroozle"u8),
    PI: newInt(23),
    PS: newString("a string"u8),
    PSI: newIntSlice(21, 22, 23),
    BinaryFunc: (@string a, @string b) => fmt.Sprintf("[%s=%s]"u8, a, b),
    VariadicFunc: (params ꓸꓸꓸstring sʗp) => {
        var s = sʗp.slice();
        return fmt.Sprint((@string)"<"u8, strings.Join(s, "+"u8), (@string)">"u8);
    },
    VariadicFuncInt: (nint a, params ꓸꓸꓸstring sʗp) => {
        var s = sʗp.slice();
        return fmt.Sprint(a, (@string)"=<"u8, strings.Join(s, "+"u8), (@string)">"u8);
    },
    NilOKFunc: (ж<nint> s) => s == nil,
    ErrFunc: () => ("bla", default!),
    PanicFunc: () => {
        throw panic("test panic");
    },
    Tmpl: Must(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2)
));

internal static slice<ж<T>> tSliceOfNil = new ж<T>[]{default!}.slice();

// A non-empty interface.
[GoType] public partial interface I {
    @string Method0();
}

internal static ж<I> ᏑiVal = new StandardBox<I>(new template_internal_test_package.TжI(tVal));
internal static ref I iVal => ref ᏑiVal.ValueSlot;

// Helpers for creation.
internal static ж<nint> newInt(nint nʗp) {
    ref var n = ref heap(nʗp, out var Ꮡn);

    return Ꮡn;
}

internal static ж<@string> newString(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    return Ꮡs;
}

internal static ж<slice<nint>> newIntSlice(params ꓸꓸꓸnint nʗp) {
    var n = nʗp.sslice();

    var p = @new<slice<nint>>();
    p.ValueSlot = new slice<nint>(len(n));
    copy(p.ValueSlot, n);
    return p;
}

// Simple methods with and without arguments.
[GoRecv] public static @string Method0(this ref T t) {
    return "M0"u8;
}

[GoRecv] public static nint Method1(this ref T t, nint a) {
    return a;
}

[GoRecv] public static @string Method2(this ref T t, uint16 a, @string b) {
    return fmt.Sprintf("Method2: %d %s"u8, a, b);
}

[GoRecv] public static @string Method3(this ref T t, any v) {
    return fmt.Sprintf("Method3: %v"u8, v);
}

[GoRecv] public static ж<T> Copy(this ref T t) {
    var n = @new<T>();
    n.Value = t.ΔClone();
    return n;
}

[GoRecv] public static slice<nint> MAdd(this ref T t, nint a, slice<nint> b) {
    var v = new slice<nint>(len(b));
    foreach (var (i, x) in b) {
        v[i] = x + a;
    }
    return v;
}

internal static error myError = errors.New("my error"u8);

// MyError returns a value and an error according to its argument.
[GoRecv] public static (bool, error) MyError(this ref T t, bool error) {
    if (error) {
        return (true, myError);
    }
    return (false, default!);
}

// A few methods to test chaining.
[GoRecv] public static ж<U> GetU(this ref T t) {
    return t.U;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ = "true"u8;

[GoRecv] public static @string TrueFalse(this ref U u, bool b) {
    if (b) {
        return trueˢ;
    }
    return ""u8;
}

internal static @string typeOf(any arg) {
    return fmt.Sprintf("%T"u8, arg);
}

[GoType] internal partial struct execTest {
    internal @string name;
    internal @string input;
    internal @string output;
    internal any data;
    internal bool ok;
}

// bigInt and bigUint are hex string representing numbers either side
// of the max int boundary.
// We do it this way so the test doesn't depend on ints being 32 bits.
internal static @string bigInt = fmt.Sprintf("0x%x"u8, (nint)(((nint)1).Lsh((nuint)(reflect.TypeFor<nint>().Bits() - 1)) - 1));

internal static @string bigUint = fmt.Sprintf("0x%x"u8, (nuint)(((nuint)1).Lsh((nuint)(reflect.TypeFor<nint>().Bits() - 1))));

// Trivial cases.
// Ideal constants.
// Fields of structs.
// Fields on maps.
// NOTE: <no value> in text/template
// Dots of all kinds to test basic evaluation.
// Variables.
// Type with String method.
//  NOTE: -<6666>- in text/template
// Type with Error method.
// NOTE: -[888] in text/template
// Pointers.
// Empty interfaces holding values.
// NOTE: <no value> in text/template
// Edge cases with <no value> with an interface value
// NOTE: <no value> in text/template
// NOTE: <no value> in text/template
// Issue 31810: Parenthesized first element of pipeline with arguments.
// See also TestIssue31810.
// This is fine.
// Method calls.
// Function call builtin.
// Erroneous function calls (check args).
// Pipelines.
// Nil values aren't missing arguments.
// Parenthesized expressions
// Parenthesized expressions with field accesses
// If.
// Print etc.
// HTML.
// NOTE: "&lt;no value&gt;" in text/template
// JavaScript.
// URL query.
// Booleans
// Indexing.
// Slicing.
// Len.
// With.
// Range.
// Cute examples.
// Error handling.
// Numbers
// Fixed bugs.
// Must separate dot and receiver; otherwise args are evaluated with dot set to variable.
// Do not loop endlessly in indirect for non-empty interfaces.
// The bug appears with *interface only; looped forever.
// Was taking address of interface field, so method set was empty.
// Struct values were not legal in with - mere oversight.
// Nil interface values in if.
// Stringer.
// Args need to be indirected and dereferenced sometimes.
// Legal parse but illegal execution: non-function should have no arguments.
// Pipelined arg was not being type-checked.
// A bug was introduced that broke map lookups for lower-case names.
// Field chain starting with function did not work.
// Dereferencing nil pointer while evaluating function arguments should not panic. Issue 7333.
// 0xef gave constant type float64. Issue 8622.
// Chained nodes did not work as arguments. Issue 8473.
// Didn't protect against nil or literal values in field chains.
// Didn't call validateType on function results. Issue 10800.
// Variadic function corner cases. Issue 10946.
// More variadic function corner cases. Some runes would get evaluated
// as constant floats instead of ints. Issue 34483.

    [GoType("dyn")] partial struct Δtype {
        internal nint a;
        internal @string b;
    }
internal static slice<execTest> execTests = new execTest[]{
    new("empty"u8, ""u8, ""u8, default!, true),
    new("text"u8, "some text"u8, "some text"u8, default!, true),
    new("nil action"u8, "{{nil}}"u8, ""u8, default!, false),
    new("ideal int"u8, "{{typeOf 3}}"u8, "int"u8, (nint)(0), true),
    new("ideal float"u8, "{{typeOf 1.0}}"u8, "float64"u8, (nint)(0), true),
    new("ideal exp float"u8, "{{typeOf 1e1}}"u8, "float64"u8, (nint)(0), true),
    new("ideal complex"u8, "{{typeOf 1i}}"u8, "complex128"u8, (nint)(0), true),
    new("ideal int"u8, "{{typeOf "u8 + bigInt + "}}"u8, "int"u8, (nint)(0), true),
    new("ideal too big"u8, "{{typeOf "u8 + bigUint + "}}"u8, ""u8, (nint)(0), false),
    new("ideal nil without type"u8, "{{nil}}"u8, ""u8, (nint)(0), false),
    new(".X"u8, "-{{.X}}-"u8, "-x-"u8, tVal.OrTypedNil(), true),
    new(".U.V"u8, "-{{.U.V}}-"u8, "-v-"u8, tVal.OrTypedNil(), true),
    new(".unexported"u8, "{{.unexported}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("map .one"u8, "{{.MSI.one}}"u8, "1"u8, tVal.OrTypedNil(), true),
    new("map .two"u8, "{{.MSI.two}}"u8, "2"u8, tVal.OrTypedNil(), true),
    new("map .NO"u8, "{{.MSI.NO}}"u8, ""u8, tVal.OrTypedNil(), true),
    new("map .one interface"u8, "{{.MXI.one}}"u8, "1"u8, tVal.OrTypedNil(), true),
    new("map .WRONG args"u8, "{{.MSI.one 1}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("map .WRONG type"u8, "{{.MII.one}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("dot int"u8, "<{{.}}>"u8, "&lt;13>"u8, (nint)(13), true),
    new("dot uint"u8, "<{{.}}>"u8, "&lt;14>"u8, (nuint)14, true),
    new("dot float"u8, "<{{.}}>"u8, "&lt;15.1>"u8, 15.1D, true),
    new("dot bool"u8, "<{{.}}>"u8, "&lt;true>"u8, true, true),
    new("dot complex"u8, "<{{.}}>"u8, "&lt;(16.2-17i)>"u8, 16.2D + -17D.i(), true),
    new("dot string"u8, "<{{.}}>"u8, "&lt;hello>"u8, (@string)"hello"u8, true),
    new("dot slice"u8, "<{{.}}>"u8, "&lt;[-1 -2 -3]>"u8, new nint[]{-1, -2, -3}.slice(), true),
    new("dot map"u8, "<{{.}}>"u8, "&lt;map[two:22]>"u8, new map<@string, nint>{["two"u8] = 22}, true),
    new("dot struct"u8, "<{{.}}>"u8, "&lt;{7 seven}>"u8, new Δtype(7, "seven"u8), true),
    new("$ int"u8, "{{$}}"u8, "123"u8, (nint)(123), true),
    new("$.I"u8, "{{$.I}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("$.U.V"u8, "{{$.U.V}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("declare in action"u8, "{{$x := $.U.V}}{{$x}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("simple assignment"u8, "{{$x := 2}}{{$x = 3}}{{$x}}"u8, "3"u8, tVal.OrTypedNil(), true),
    new("nested assignment"u8,
        "{{$x := 2}}{{if true}}{{$x = 3}}{{end}}{{$x}}"u8,
        "3"u8, tVal.OrTypedNil(), true),
    new("nested assignment changes the last declaration"u8,
        "{{$x := 1}}{{if true}}{{$x := 2}}{{if true}}{{$x = 3}}{{end}}{{end}}{{$x}}"u8,
        "1"u8, tVal.OrTypedNil(), true),
    new("V{6666}.String()"u8, "-{{.V0}}-"u8, "-{6666}-"u8, tVal.OrTypedNil(), true),
    new("&V{7777}.String()"u8, "-{{.V1}}-"u8, "-&lt;7777&gt;-"u8, tVal.OrTypedNil(), true),
    new("(*V)(nil).String()"u8, "-{{.V2}}-"u8, "-nilV-"u8, tVal.OrTypedNil(), true),
    new("W{888}.Error()"u8, "-{{.W0}}-"u8, "-{888}-"u8, tVal.OrTypedNil(), true),
    new("&W{999}.Error()"u8, "-{{.W1}}-"u8, "-[999]-"u8, tVal.OrTypedNil(), true),
    new("(*W)(nil).Error()"u8, "-{{.W2}}-"u8, "-nilW-"u8, tVal.OrTypedNil(), true),
    new("*int"u8, "{{.PI}}"u8, "23"u8, tVal.OrTypedNil(), true),
    new("*string"u8, "{{.PS}}"u8, "a string"u8, tVal.OrTypedNil(), true),
    new("*[]int"u8, "{{.PSI}}"u8, "[21 22 23]"u8, tVal.OrTypedNil(), true),
    new("*[]int[1]"u8, "{{index .PSI 1}}"u8, "22"u8, tVal.OrTypedNil(), true),
    new("NIL"u8, "{{.NIL}}"u8, "&lt;nil&gt;"u8, tVal.OrTypedNil(), true),
    new("empty nil"u8, "{{.Empty0}}"u8, ""u8, tVal.OrTypedNil(), true),
    new("empty with int"u8, "{{.Empty1}}"u8, "3"u8, tVal.OrTypedNil(), true),
    new("empty with string"u8, "{{.Empty2}}"u8, "empty2"u8, tVal.OrTypedNil(), true),
    new("empty with slice"u8, "{{.Empty3}}"u8, "[7 8]"u8, tVal.OrTypedNil(), true),
    new("empty with struct"u8, "{{.Empty4}}"u8, "{UinEmpty}"u8, tVal.OrTypedNil(), true),
    new("empty with struct, field"u8, "{{.Empty4.V}}"u8, "UinEmpty"u8, tVal.OrTypedNil(), true),
    new("field on interface"u8, "{{.foo}}"u8, ""u8, default!, true),
    new("field on parenthesized interface"u8, "{{(.).foo}}"u8, ""u8, default!, true),
    new("unparenthesized non-function"u8, "{{1 2}}"u8, ""u8, default!, false),
    new("parenthesized non-function"u8, "{{(1) 2}}"u8, ""u8, default!, false),
    new("parenthesized non-function with no args"u8, "{{(1)}}"u8, "1"u8, default!, true),
    new(".Method0"u8, "-{{.Method0}}-"u8, "-M0-"u8, tVal.OrTypedNil(), true),
    new(".Method1(1234)"u8, "-{{.Method1 1234}}-"u8, "-1234-"u8, tVal.OrTypedNil(), true),
    new(".Method1(.I)"u8, "-{{.Method1 .I}}-"u8, "-17-"u8, tVal.OrTypedNil(), true),
    new(".Method2(3, .X)"u8, "-{{.Method2 3 .X}}-"u8, "-Method2: 3 x-"u8, tVal.OrTypedNil(), true),
    new(".Method2(.U16, `str`)"u8, "-{{.Method2 .U16 `str`}}-"u8, "-Method2: 16 str-"u8, tVal.OrTypedNil(), true),
    new(".Method2(.U16, $x)"u8, "{{if $x := .X}}-{{.Method2 .U16 $x}}{{end}}-"u8, "-Method2: 16 x-"u8, tVal.OrTypedNil(), true),
    new(".Method3(nil constant)"u8, "-{{.Method3 nil}}-"u8, "-Method3: &lt;nil&gt;-"u8, tVal.OrTypedNil(), true),
    new(".Method3(nil value)"u8, "-{{.Method3 .MXI.unset}}-"u8, "-Method3: &lt;nil&gt;-"u8, tVal.OrTypedNil(), true),
    new("method on var"u8, "{{if $x := .}}-{{$x.Method2 .U16 $x.X}}{{end}}-"u8, "-Method2: 16 x-"u8, tVal.OrTypedNil(), true),
    new("method on chained var"u8,
        "{{range .MSIone}}{{if $.U.TrueFalse $.True}}{{$.U.TrueFalse $.True}}{{else}}WRONG{{end}}{{end}}"u8,
        "true"u8, tVal.OrTypedNil(), true),
    new("chained method"u8,
        "{{range .MSIone}}{{if $.GetU.TrueFalse $.True}}{{$.U.TrueFalse $.True}}{{else}}WRONG{{end}}{{end}}"u8,
        "true"u8, tVal.OrTypedNil(), true),
    new("chained method on variable"u8,
        "{{with $x := .}}{{with .SI}}{{$.GetU.TrueFalse $.True}}{{end}}{{end}}"u8,
        "true"u8, tVal.OrTypedNil(), true),
    new(".NilOKFunc not nil"u8, "{{call .NilOKFunc .PI}}"u8, "false"u8, tVal.OrTypedNil(), true),
    new(".NilOKFunc nil"u8, "{{call .NilOKFunc nil}}"u8, "true"u8, tVal.OrTypedNil(), true),
    new("method on nil value from slice"u8, "-{{range .}}{{.Method1 1234}}{{end}}-"u8, "-1234-"u8, tSliceOfNil, true),
    new("method on typed nil interface value"u8, "{{.NonEmptyInterfaceTypedNil.Method0}}"u8, "M0"u8, tVal.OrTypedNil(), true),
    new(".BinaryFunc"u8, "{{call .BinaryFunc `1` `2`}}"u8, "[1=2]"u8, tVal.OrTypedNil(), true),
    new(".VariadicFunc0"u8, "{{call .VariadicFunc}}"u8, "&lt;&gt;"u8, tVal.OrTypedNil(), true),
    new(".VariadicFunc2"u8, "{{call .VariadicFunc `he` `llo`}}"u8, "&lt;he&#43;llo&gt;"u8, tVal.OrTypedNil(), true),
    new(".VariadicFuncInt"u8, "{{call .VariadicFuncInt 33 `he` `llo`}}"u8, "33=&lt;he&#43;llo&gt;"u8, tVal.OrTypedNil(), true),
    new("if .BinaryFunc call"u8, "{{ if .BinaryFunc}}{{call .BinaryFunc `1` `2`}}{{end}}"u8, "[1=2]"u8, tVal.OrTypedNil(), true),
    new("if not .BinaryFunc call"u8, "{{ if not .BinaryFunc}}{{call .BinaryFunc `1` `2`}}{{else}}No{{end}}"u8, "No"u8, tVal.OrTypedNil(), true),
    new("Interface Call"u8, @"{{stringer .S}}"u8, "foozle"u8, new map<@string, any>{["S"u8] = bytes.NewBufferString("foozle"u8).OrTypedNil()}, true),
    new(".ErrFunc"u8, "{{call .ErrFunc}}"u8, "bla"u8, tVal.OrTypedNil(), true),
    new("call nil"u8, "{{call nil}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".BinaryFuncTooFew"u8, "{{call .BinaryFunc `1`}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".BinaryFuncTooMany"u8, "{{call .BinaryFunc `1` `2` `3`}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".BinaryFuncBad0"u8, "{{call .BinaryFunc 1 3}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".BinaryFuncBad1"u8, "{{call .BinaryFunc `1` 3}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".VariadicFuncBad0"u8, "{{call .VariadicFunc 3}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".VariadicFuncIntBad0"u8, "{{call .VariadicFuncInt}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".VariadicFuncIntBad`"u8, "{{call .VariadicFuncInt `x`}}"u8, ""u8, tVal.OrTypedNil(), false),
    new(".VariadicFuncNilBad"u8, "{{call .VariadicFunc nil}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("pipeline"u8, "-{{.Method0 | .Method2 .U16}}-"u8, "-Method2: 16 M0-"u8, tVal.OrTypedNil(), true),
    new("pipeline func"u8, "-{{call .VariadicFunc `llo` | call .VariadicFunc `he` }}-"u8, "-&lt;he&#43;&lt;llo&gt;&gt;-"u8, tVal.OrTypedNil(), true),
    new("nil pipeline"u8, "{{ .Empty0 | call .NilOKFunc }}"u8, "true"u8, tVal.OrTypedNil(), true),
    new("nil call arg"u8, "{{ call .NilOKFunc .Empty0 }}"u8, "true"u8, tVal.OrTypedNil(), true),
    new("bad nil pipeline"u8, "{{ .Empty0 | .VariadicFunc }}"u8, ""u8, tVal.OrTypedNil(), false),
    new("parens in pipeline"u8, "{{printf `%d %d %d` (1) (2 | add 3) (add 4 (add 5 6))}}"u8, "1 5 15"u8, tVal.OrTypedNil(), true),
    new("parens: $ in paren"u8, "{{($).X}}"u8, "x"u8, tVal.OrTypedNil(), true),
    new("parens: $.GetU in paren"u8, "{{($.GetU).V}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("parens: $ in paren in pipe"u8, "{{($ | echo).X}}"u8, "x"u8, tVal.OrTypedNil(), true),
    new("parens: spaces and args"u8, @"{{(makemap ""up"" ""down"" ""left"" ""right"").left}}"u8, "right"u8, tVal.OrTypedNil(), true),
    new("if true"u8, "{{if true}}TRUE{{end}}"u8, "TRUE"u8, tVal.OrTypedNil(), true),
    new("if false"u8, "{{if false}}TRUE{{else}}FALSE{{end}}"u8, "FALSE"u8, tVal.OrTypedNil(), true),
    new("if nil"u8, "{{if nil}}TRUE{{end}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("if on typed nil interface value"u8, "{{if .NonEmptyInterfaceTypedNil}}TRUE{{ end }}"u8, ""u8, tVal.OrTypedNil(), true),
    new("if 1"u8, "{{if 1}}NON-ZERO{{else}}ZERO{{end}}"u8, "NON-ZERO"u8, tVal.OrTypedNil(), true),
    new("if 0"u8, "{{if 0}}NON-ZERO{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("if 1.5"u8, "{{if 1.5}}NON-ZERO{{else}}ZERO{{end}}"u8, "NON-ZERO"u8, tVal.OrTypedNil(), true),
    new("if 0.0"u8, "{{if .FloatZero}}NON-ZERO{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("if 1.5i"u8, "{{if 1.5i}}NON-ZERO{{else}}ZERO{{end}}"u8, "NON-ZERO"u8, tVal.OrTypedNil(), true),
    new("if 0.0i"u8, "{{if .ComplexZero}}NON-ZERO{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("if emptystring"u8, "{{if ``}}NON-EMPTY{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("if string"u8, "{{if `notempty`}}NON-EMPTY{{else}}EMPTY{{end}}"u8, "NON-EMPTY"u8, tVal.OrTypedNil(), true),
    new("if emptyslice"u8, "{{if .SIEmpty}}NON-EMPTY{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("if slice"u8, "{{if .SI}}NON-EMPTY{{else}}EMPTY{{end}}"u8, "NON-EMPTY"u8, tVal.OrTypedNil(), true),
    new("if emptymap"u8, "{{if .MSIEmpty}}NON-EMPTY{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("if map"u8, "{{if .MSI}}NON-EMPTY{{else}}EMPTY{{end}}"u8, "NON-EMPTY"u8, tVal.OrTypedNil(), true),
    new("if map unset"u8, "{{if .MXI.none}}NON-ZERO{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("if map not unset"u8, "{{if not .MXI.none}}ZERO{{else}}NON-ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("if $x with $y int"u8, "{{if $x := true}}{{with $y := .I}}{{$x}},{{$y}}{{end}}{{end}}"u8, "true,17"u8, tVal.OrTypedNil(), true),
    new("if $x with $x int"u8, "{{if $x := true}}{{with $x := .I}}{{$x}},{{end}}{{$x}}{{end}}"u8, "17,true"u8, tVal.OrTypedNil(), true),
    new("if else if"u8, "{{if false}}FALSE{{else if true}}TRUE{{end}}"u8, "TRUE"u8, tVal.OrTypedNil(), true),
    new("if else chain"u8, "{{if eq 1 3}}1{{else if eq 2 3}}2{{else if eq 3 3}}3{{end}}"u8, "3"u8, tVal.OrTypedNil(), true),
    new("print"u8, @"{{print ""hello, print""}}"u8, "hello, print"u8, tVal.OrTypedNil(), true),
    new("print 123"u8, @"{{print 1 2 3}}"u8, "1 2 3"u8, tVal.OrTypedNil(), true),
    new("print nil"u8, @"{{print nil}}"u8, "&lt;nil&gt;"u8, tVal.OrTypedNil(), true),
    new("println"u8, @"{{println 1 2 3}}"u8, "1 2 3\n"u8, tVal.OrTypedNil(), true),
    new("printf int"u8, @"{{printf ""%04x"" 127}}"u8, "007f"u8, tVal.OrTypedNil(), true),
    new("printf float"u8, @"{{printf ""%g"" 3.5}}"u8, "3.5"u8, tVal.OrTypedNil(), true),
    new("printf complex"u8, @"{{printf ""%g"" 1+7i}}"u8, "(1&#43;7i)"u8, tVal.OrTypedNil(), true),
    new("printf string"u8, @"{{printf ""%s"" ""hello""}}"u8, "hello"u8, tVal.OrTypedNil(), true),
    new("printf function"u8, @"{{printf ""%#q"" zeroArgs}}"u8, "`zeroArgs`"u8, tVal.OrTypedNil(), true),
    new("printf field"u8, @"{{printf ""%s"" .U.V}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("printf method"u8, @"{{printf ""%s"" .Method0}}"u8, "M0"u8, tVal.OrTypedNil(), true),
    new("printf dot"u8, @"{{with .I}}{{printf ""%d"" .}}{{end}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("printf var"u8, @"{{with $x := .I}}{{printf ""%d"" $x}}{{end}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("printf lots"u8, @"{{printf ""%d %s %g %s"" 127 ""hello"" 7-3i .Method0}}"u8, "127 hello (7-3i) M0"u8, tVal.OrTypedNil(), true),
    new("html"u8, @"{{html ""<script>alert(\""XSS\"");</script>""}}"u8,
        "&lt;script&gt;alert(&#34;XSS&#34;);&lt;/script&gt;"u8, default!, true),
    new("html pipeline"u8, @"{{printf ""<script>alert(\""XSS\"");</script>"" | html}}"u8,
        "&lt;script&gt;alert(&#34;XSS&#34;);&lt;/script&gt;"u8, default!, true),
    new("html"u8, @"{{html .PS}}"u8, "a string"u8, tVal.OrTypedNil(), true),
    new("html typed nil"u8, @"{{html .NIL}}"u8, "&lt;nil&gt;"u8, tVal.OrTypedNil(), true),
    new("html untyped nil"u8, @"{{html .Empty0}}"u8, "&lt;nil&gt;"u8, tVal.OrTypedNil(), true),
    new("js"u8, @"{{js .}}"u8, @"It\&#39;d be nice."u8, (@string)@"It'd be nice."u8, true),
    new("urlquery"u8, @"{{""http://www.example.org/""|urlquery}}"u8, "http%3A%2F%2Fwww.example.org%2F"u8, default!, true),
    new("not"u8, "{{not true}} {{not false}}"u8, "false true"u8, default!, true),
    new("and"u8, "{{and false 0}} {{and 1 0}} {{and 0 true}} {{and 1 1}}"u8, "false 0 0 1"u8, default!, true),
    new("or"u8, "{{or 0 0}} {{or 1 0}} {{or 0 true}} {{or 1 1}}"u8, "0 1 true 1"u8, default!, true),
    new("boolean if"u8, "{{if and true 1 `hi`}}TRUE{{else}}FALSE{{end}}"u8, "TRUE"u8, tVal.OrTypedNil(), true),
    new("boolean if not"u8, "{{if and true 1 `hi` | not}}TRUE{{else}}FALSE{{end}}"u8, "FALSE"u8, default!, true),
    new("slice[0]"u8, "{{index .SI 0}}"u8, "3"u8, tVal.OrTypedNil(), true),
    new("slice[1]"u8, "{{index .SI 1}}"u8, "4"u8, tVal.OrTypedNil(), true),
    new("slice[HUGE]"u8, "{{index .SI 10}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice[WRONG]"u8, "{{index .SI `hello`}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice[nil]"u8, "{{index .SI nil}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("map[one]"u8, "{{index .MSI `one`}}"u8, "1"u8, tVal.OrTypedNil(), true),
    new("map[two]"u8, "{{index .MSI `two`}}"u8, "2"u8, tVal.OrTypedNil(), true),
    new("map[NO]"u8, "{{index .MSI `XXX`}}"u8, "0"u8, tVal.OrTypedNil(), true),
    new("map[nil]"u8, "{{index .MSI nil}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("map[``]"u8, "{{index .MSI ``}}"u8, "0"u8, tVal.OrTypedNil(), true),
    new("map[WRONG]"u8, "{{index .MSI 10}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("double index"u8, "{{index .SMSI 1 `eleven`}}"u8, "11"u8, tVal.OrTypedNil(), true),
    new("nil[1]"u8, "{{index nil 1}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("map MI64S"u8, "{{index .MI64S 2}}"u8, "i642"u8, tVal.OrTypedNil(), true),
    new("map MI32S"u8, "{{index .MI32S 2}}"u8, "two"u8, tVal.OrTypedNil(), true),
    new("map MUI64S"u8, "{{index .MUI64S 3}}"u8, "ui643"u8, tVal.OrTypedNil(), true),
    new("map MI8S"u8, "{{index .MI8S 3}}"u8, "i83"u8, tVal.OrTypedNil(), true),
    new("map MUI8S"u8, "{{index .MUI8S 2}}"u8, "u82"u8, tVal.OrTypedNil(), true),
    new("index of an interface field"u8, "{{index .Empty3 0}}"u8, "7"u8, tVal.OrTypedNil(), true),
    new("slice[:]"u8, "{{slice .SI}}"u8, "[3 4 5]"u8, tVal.OrTypedNil(), true),
    new("slice[1:]"u8, "{{slice .SI 1}}"u8, "[4 5]"u8, tVal.OrTypedNil(), true),
    new("slice[1:2]"u8, "{{slice .SI 1 2}}"u8, "[4]"u8, tVal.OrTypedNil(), true),
    new("slice[-1:]"u8, "{{slice .SI -1}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice[1:-2]"u8, "{{slice .SI 1 -2}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice[1:2:-1]"u8, "{{slice .SI 1 2 -1}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice[2:1]"u8, "{{slice .SI 2 1}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice[2:2:1]"u8, "{{slice .SI 2 2 1}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("out of range"u8, "{{slice .SI 4 5}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("out of range"u8, "{{slice .SI 2 2 5}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("len(s) < indexes < cap(s)"u8, "{{slice .SICap 6 10}}"u8, "[0 0 0 0]"u8, tVal.OrTypedNil(), true),
    new("len(s) < indexes < cap(s)"u8, "{{slice .SICap 6 10 10}}"u8, "[0 0 0 0]"u8, tVal.OrTypedNil(), true),
    new("indexes > cap(s)"u8, "{{slice .SICap 10 11}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("indexes > cap(s)"u8, "{{slice .SICap 6 10 11}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("array[:]"u8, "{{slice .AI}}"u8, "[3 4 5]"u8, tVal.OrTypedNil(), true),
    new("array[1:]"u8, "{{slice .AI 1}}"u8, "[4 5]"u8, tVal.OrTypedNil(), true),
    new("array[1:2]"u8, "{{slice .AI 1 2}}"u8, "[4]"u8, tVal.OrTypedNil(), true),
    new("string[:]"u8, "{{slice .S}}"u8, "xyz"u8, tVal.OrTypedNil(), true),
    new("string[0:1]"u8, "{{slice .S 0 1}}"u8, "x"u8, tVal.OrTypedNil(), true),
    new("string[1:]"u8, "{{slice .S 1}}"u8, "yz"u8, tVal.OrTypedNil(), true),
    new("string[1:2]"u8, "{{slice .S 1 2}}"u8, "y"u8, tVal.OrTypedNil(), true),
    new("out of range"u8, "{{slice .S 1 5}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("3-index slice of string"u8, "{{slice .S 1 2 2}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("slice of an interface field"u8, "{{slice .Empty3 0 1}}"u8, "[7]"u8, tVal.OrTypedNil(), true),
    new("slice"u8, "{{len .SI}}"u8, "3"u8, tVal.OrTypedNil(), true),
    new("map"u8, "{{len .MSI }}"u8, "3"u8, tVal.OrTypedNil(), true),
    new("len of int"u8, "{{len 3}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("len of nothing"u8, "{{len .Empty0}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("len of an interface field"u8, "{{len .Empty3}}"u8, "2"u8, tVal.OrTypedNil(), true),
    new("with true"u8, "{{with true}}{{.}}{{end}}"u8, "true"u8, tVal.OrTypedNil(), true),
    new("with false"u8, "{{with false}}{{.}}{{else}}FALSE{{end}}"u8, "FALSE"u8, tVal.OrTypedNil(), true),
    new("with 1"u8, "{{with 1}}{{.}}{{else}}ZERO{{end}}"u8, "1"u8, tVal.OrTypedNil(), true),
    new("with 0"u8, "{{with 0}}{{.}}{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("with 1.5"u8, "{{with 1.5}}{{.}}{{else}}ZERO{{end}}"u8, "1.5"u8, tVal.OrTypedNil(), true),
    new("with 0.0"u8, "{{with .FloatZero}}{{.}}{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("with 1.5i"u8, "{{with 1.5i}}{{.}}{{else}}ZERO{{end}}"u8, "(0&#43;1.5i)"u8, tVal.OrTypedNil(), true),
    new("with 0.0i"u8, "{{with .ComplexZero}}{{.}}{{else}}ZERO{{end}}"u8, "ZERO"u8, tVal.OrTypedNil(), true),
    new("with emptystring"u8, "{{with ``}}{{.}}{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("with string"u8, "{{with `notempty`}}{{.}}{{else}}EMPTY{{end}}"u8, "notempty"u8, tVal.OrTypedNil(), true),
    new("with emptyslice"u8, "{{with .SIEmpty}}{{.}}{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("with slice"u8, "{{with .SI}}{{.}}{{else}}EMPTY{{end}}"u8, "[3 4 5]"u8, tVal.OrTypedNil(), true),
    new("with emptymap"u8, "{{with .MSIEmpty}}{{.}}{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("with map"u8, "{{with .MSIone}}{{.}}{{else}}EMPTY{{end}}"u8, "map[one:1]"u8, tVal.OrTypedNil(), true),
    new("with empty interface, struct field"u8, "{{with .Empty4}}{{.V}}{{end}}"u8, "UinEmpty"u8, tVal.OrTypedNil(), true),
    new("with $x int"u8, "{{with $x := .I}}{{$x}}{{end}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("with $x struct.U.V"u8, "{{with $x := $}}{{$x.U.V}}{{end}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("with variable and action"u8, "{{with $x := $}}{{$y := $.U.V}}{{$y}}{{end}}"u8, "v"u8, tVal.OrTypedNil(), true),
    new("with on typed nil interface value"u8, "{{with .NonEmptyInterfaceTypedNil}}TRUE{{ end }}"u8, ""u8, tVal.OrTypedNil(), true),
    new("with else with"u8, "{{with 0}}{{.}}{{else with true}}{{.}}{{end}}"u8, "true"u8, tVal.OrTypedNil(), true),
    new("with else with chain"u8, "{{with 0}}{{.}}{{else with false}}{{.}}{{else with `notempty`}}{{.}}{{end}}"u8, "notempty"u8, tVal.OrTypedNil(), true),
    new("range []int"u8, "{{range .SI}}-{{.}}-{{end}}"u8, "-3--4--5-"u8, tVal.OrTypedNil(), true),
    new("range empty no else"u8, "{{range .SIEmpty}}-{{.}}-{{end}}"u8, ""u8, tVal.OrTypedNil(), true),
    new("range []int else"u8, "{{range .SI}}-{{.}}-{{else}}EMPTY{{end}}"u8, "-3--4--5-"u8, tVal.OrTypedNil(), true),
    new("range empty else"u8, "{{range .SIEmpty}}-{{.}}-{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("range []int break else"u8, "{{range .SI}}-{{.}}-{{break}}NOTREACHED{{else}}EMPTY{{end}}"u8, "-3-"u8, tVal.OrTypedNil(), true),
    new("range []int continue else"u8, "{{range .SI}}-{{.}}-{{continue}}NOTREACHED{{else}}EMPTY{{end}}"u8, "-3--4--5-"u8, tVal.OrTypedNil(), true),
    new("range []bool"u8, "{{range .SB}}-{{.}}-{{end}}"u8, "-true--false-"u8, tVal.OrTypedNil(), true),
    new("range []int method"u8, "{{range .SI | .MAdd .I}}-{{.}}-{{end}}"u8, "-20--21--22-"u8, tVal.OrTypedNil(), true),
    new("range map"u8, "{{range .MSI}}-{{.}}-{{end}}"u8, "-1--3--2-"u8, tVal.OrTypedNil(), true),
    new("range empty map no else"u8, "{{range .MSIEmpty}}-{{.}}-{{end}}"u8, ""u8, tVal.OrTypedNil(), true),
    new("range map else"u8, "{{range .MSI}}-{{.}}-{{else}}EMPTY{{end}}"u8, "-1--3--2-"u8, tVal.OrTypedNil(), true),
    new("range empty map else"u8, "{{range .MSIEmpty}}-{{.}}-{{else}}EMPTY{{end}}"u8, "EMPTY"u8, tVal.OrTypedNil(), true),
    new("range empty interface"u8, "{{range .Empty3}}-{{.}}-{{else}}EMPTY{{end}}"u8, "-7--8-"u8, tVal.OrTypedNil(), true),
    new("range empty nil"u8, "{{range .Empty0}}-{{.}}-{{end}}"u8, ""u8, tVal.OrTypedNil(), true),
    new("range $x SI"u8, "{{range $x := .SI}}<{{$x}}>{{end}}"u8, "&lt;3>&lt;4>&lt;5>"u8, tVal.OrTypedNil(), true),
    new("range $x $y SI"u8, "{{range $x, $y := .SI}}<{{$x}}={{$y}}>{{end}}"u8, "&lt;0=3>&lt;1=4>&lt;2=5>"u8, tVal.OrTypedNil(), true),
    new("range $x MSIone"u8, "{{range $x := .MSIone}}<{{$x}}>{{end}}"u8, "&lt;1>"u8, tVal.OrTypedNil(), true),
    new("range $x $y MSIone"u8, "{{range $x, $y := .MSIone}}<{{$x}}={{$y}}>{{end}}"u8, "&lt;one=1>"u8, tVal.OrTypedNil(), true),
    new("range $x PSI"u8, "{{range $x := .PSI}}<{{$x}}>{{end}}"u8, "&lt;21>&lt;22>&lt;23>"u8, tVal.OrTypedNil(), true),
    new("declare in range"u8, "{{range $x := .PSI}}<{{$foo:=$x}}{{$x}}>{{end}}"u8, "&lt;21>&lt;22>&lt;23>"u8, tVal.OrTypedNil(), true),
    new("range count"u8, @"{{range $i, $x := count 5}}[{{$i}}]{{$x}}{{end}}"u8, "[0]a[1]b[2]c[3]d[4]e"u8, tVal.OrTypedNil(), true),
    new("range nil count"u8, @"{{range $i, $x := count 0}}{{else}}empty{{end}}"u8, "empty"u8, tVal.OrTypedNil(), true),
    new("or as if true"u8, @"{{or .SI ""slice is empty""}}"u8, "[3 4 5]"u8, tVal.OrTypedNil(), true),
    new("or as if false"u8, @"{{or .SIEmpty ""slice is empty""}}"u8, "slice is empty"u8, tVal.OrTypedNil(), true),
    new("error method, error"u8, "{{.MyError true}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("error method, no error"u8, "{{.MyError false}}"u8, "false"u8, tVal.OrTypedNil(), true),
    new("decimal"u8, "{{print 1234}}"u8, "1234"u8, tVal.OrTypedNil(), true),
    new("decimal _"u8, "{{print 12_34}}"u8, "1234"u8, tVal.OrTypedNil(), true),
    new("binary"u8, "{{print 0b101}}"u8, "5"u8, tVal.OrTypedNil(), true),
    new("binary _"u8, "{{print 0b_1_0_1}}"u8, "5"u8, tVal.OrTypedNil(), true),
    new("BINARY"u8, "{{print 0B101}}"u8, "5"u8, tVal.OrTypedNil(), true),
    new("octal0"u8, "{{print 0377}}"u8, "255"u8, tVal.OrTypedNil(), true),
    new("octal"u8, "{{print 0o377}}"u8, "255"u8, tVal.OrTypedNil(), true),
    new("octal _"u8, "{{print 0o_3_7_7}}"u8, "255"u8, tVal.OrTypedNil(), true),
    new("OCTAL"u8, "{{print 0O377}}"u8, "255"u8, tVal.OrTypedNil(), true),
    new("hex"u8, "{{print 0x123}}"u8, "291"u8, tVal.OrTypedNil(), true),
    new("hex _"u8, "{{print 0x1_23}}"u8, "291"u8, tVal.OrTypedNil(), true),
    new("HEX"u8, "{{print 0X123ABC}}"u8, "1194684"u8, tVal.OrTypedNil(), true),
    new("float"u8, "{{print 123.4}}"u8, "123.4"u8, tVal.OrTypedNil(), true),
    new("float _"u8, "{{print 0_0_1_2_3.4}}"u8, "123.4"u8, tVal.OrTypedNil(), true),
    new("hex float"u8, "{{print +0x1.ep+2}}"u8, "7.5"u8, tVal.OrTypedNil(), true),
    new("hex float _"u8, "{{print +0x_1.e_0p+0_2}}"u8, "7.5"u8, tVal.OrTypedNil(), true),
    new("HEX float"u8, "{{print +0X1.EP+2}}"u8, "7.5"u8, tVal.OrTypedNil(), true),
    new("print multi"u8, "{{print 1_2_3_4 7.5_00_00_00}}"u8, "1234 7.5"u8, tVal.OrTypedNil(), true),
    new("print multi2"u8, "{{print 1234 0x0_1.e_0p+02}}"u8, "1234 7.5"u8, tVal.OrTypedNil(), true),
    new("bug0"u8, "{{range .MSIone}}{{if $.Method1 .}}X{{end}}{{end}}"u8, "X"u8, tVal.OrTypedNil(), true),
    new("bug1"u8, "{{.Method0}}"u8, "M0"u8, ᏑiVal, true),
    new("bug2"u8, "{{$.NonEmptyInterface.Method0}}"u8, "M0"u8, tVal.OrTypedNil(), true),
    new("bug3"u8, "{{with $}}{{.Method0}}{{end}}"u8, "M0"u8, tVal.OrTypedNil(), true),
    new("bug4"u8, "{{if .Empty0}}non-nil{{else}}nil{{end}}"u8, "nil"u8, tVal.OrTypedNil(), true),
    new("bug5"u8, "{{.Str}}"u8, "foozle"u8, tVal.OrTypedNil(), true),
    new("bug5a"u8, "{{.Err}}"u8, "erroozle"u8, tVal.OrTypedNil(), true),
    new("bug6a"u8, "{{vfunc .V0 .V1}}"u8, "vfunc"u8, tVal.OrTypedNil(), true),
    new("bug6b"u8, "{{vfunc .V0 .V0}}"u8, "vfunc"u8, tVal.OrTypedNil(), true),
    new("bug6c"u8, "{{vfunc .V1 .V0}}"u8, "vfunc"u8, tVal.OrTypedNil(), true),
    new("bug6d"u8, "{{vfunc .V1 .V1}}"u8, "vfunc"u8, tVal.OrTypedNil(), true),
    new("bug7a"u8, "{{3 2}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug7b"u8, "{{$x := 1}}{{$x 2}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug7c"u8, "{{$x := 1}}{{3 | $x}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug8a"u8, "{{3|oneArg}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug8b"u8, "{{4|dddArg 3}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug9"u8, "{{.cause}}"u8, "neglect"u8, new map<@string, @string>{["cause"u8] = "neglect"u8}, true),
    new("bug10"u8, "{{mapOfThree.three}}-{{(mapOfThree).three}}"u8, "3-3"u8, (nint)(0), true),
    new("bug11"u8, "{{valueString .PS}}"u8, ""u8, new T(nil), false),
    new("bug12xe"u8, "{{printf `%T` 0xef}}"u8, "int"u8, new T(nil), true),
    new("bug12xE"u8, "{{printf `%T` 0xEE}}"u8, "int"u8, new T(nil), true),
    new("bug12Xe"u8, "{{printf `%T` 0Xef}}"u8, "int"u8, new T(nil), true),
    new("bug12XE"u8, "{{printf `%T` 0XEE}}"u8, "int"u8, new T(nil), true),
    new("bug13"u8, "{{print (.Copy).I}}"u8, "17"u8, tVal.OrTypedNil(), true),
    new("bug14a"u8, "{{(nil).True}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug14b"u8, "{{$x := nil}}{{$x.anything}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug14c"u8, @"{{$x := (1.0)}}{{$y := (""hello"")}}{{$x.anything}}{{$y.true}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug15"u8, "{{valueString returnInt}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16a"u8, "{{true|printf}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16b"u8, "{{1|printf}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16c"u8, "{{1.1|printf}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16d"u8, "{{'x'|printf}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16e"u8, "{{0i|printf}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16f"u8, "{{true|twoArgs \"xxx\"}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16g"u8, "{{\"aaa\" |twoArgs \"bbb\"}}"u8, "twoArgs=bbbaaa"u8, tVal.OrTypedNil(), true),
    new("bug16h"u8, "{{1|oneArg}}"u8, ""u8, tVal.OrTypedNil(), false),
    new("bug16i"u8, "{{\"aaa\"|oneArg}}"u8, "oneArg=aaa"u8, tVal.OrTypedNil(), true),
    new("bug16j"u8, "{{1+2i|printf \"%v\"}}"u8, "(1&#43;2i)"u8, tVal.OrTypedNil(), true),
    new("bug16k"u8, "{{\"aaa\"|printf }}"u8, "aaa"u8, tVal.OrTypedNil(), true),
    new("bug17a"u8, "{{.NonEmptyInterface.X}}"u8, "x"u8, tVal.OrTypedNil(), true),
    new("bug17b"u8, "-{{.NonEmptyInterface.Method1 1234}}-"u8, "-1234-"u8, tVal.OrTypedNil(), true),
    new("bug17c"u8, "{{len .NonEmptyInterfacePtS}}"u8, "2"u8, tVal.OrTypedNil(), true),
    new("bug17d"u8, "{{index .NonEmptyInterfacePtS 0}}"u8, "a"u8, tVal.OrTypedNil(), true),
    new("bug17e"u8, "{{range .NonEmptyInterfacePtS}}-{{.}}-{{end}}"u8, "-a--b-"u8, tVal.OrTypedNil(), true),
    new("bug18a"u8, "{{eq . '.'}}"u8, "true"u8, (rune)'.', true),
    new("bug18b"u8, "{{eq . 'e'}}"u8, "true"u8, (rune)'e', true),
    new("bug18c"u8, "{{eq . 'P'}}"u8, "true"u8, (rune)'P', true)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zeroArgsˢ = "zeroArgs"u8;

internal static @string zeroArgs() {
    return zeroArgsˢ;
}

internal static @string oneArg(@string a) {
    return "oneArg="u8 + a;
}

internal static @string twoArgs(@string a, @string b) {
    return "twoArgs="u8 + a + b;
}

internal static @string dddArg(nint a, params ꓸꓸꓸstring bʗp) {
    var b = bʗp.slice();

    return fmt.Sprintln(a, b);
}

// count returns a channel that will deliver n sequential 1-letter strings starting at "a"
[MethodImpl(MethodImplOptions.NoInlining)] internal static channel<@string> count(nint n) {
    if (n == 0) {
        return default!;
    }
    var c = new channel<@string>(0);
    var cʗ1 = c;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            cʗ1.ᐸꟷ("abcdefghijklmnop"u8.slice(i, i + 1));
        }
        close(cʗ1);
    });
    return c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vfuncˢ = "vfunc"u8;

// vfunc takes a *V and a V
internal static @string vfunc(V _Δp0, ж<V> _Δp1) {
    return vfuncˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string valueIsIgnoredˢ = "value is ignored"u8;

// valueString takes a string, not a pointer.
internal static @string valueString(@string v) {
    return valueIsIgnoredˢ;
}

// returnInt returns an int
internal static nint returnInt() {
    return 7;
}

internal static nint add(params ꓸꓸꓸnint argsʗp) {
    var args = argsʗp.sslice();

    nint sum = 0;
    foreach (var (_, x) in args) {
        sum += x;
    }
    return sum;
}

internal static any echo(any arg) {
    return arg;
}

internal static map<@string, @string> makemap(params ꓸꓸꓸstring argʗp) {
    var arg = argʗp.sslice();

    if (len(arg) % 2 != 0) {
        throw panic("bad makemap");
    }
    var m = new map<@string, @string>();
    for (nint i = 0; i < len(arg); i += 2) {
        m[arg[i]] = arg[i + 1];
    }
    return m;
}

internal static @string stringer(fmt.Stringer s) {
    return s.String();
}

internal static any mapOfThree() {
    return new map<@string, nint>{["three"u8] = 3};
}

internal static void testExecute(slice<execTest> execTests, ж<global::go.html.template_package.Template> Ꮡtemplate, ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var b = @new<strings.Builder>();
    var funcs = new FuncMap(new map<@string, any>{
        ["add"u8] = ((Funcꓸꓸꓸ<nint, nint>)(add)),
        ["count"u8] = count,
        ["dddArg"u8] = ((Funcꓸꓸꓸ<nint, @string, @string>)(dddArg)),
        ["echo"u8] = echo,
        ["makemap"u8] = ((Funcꓸꓸꓸ<@string, map<@string, @string>>)(makemap)),
        ["mapOfThree"u8] = mapOfThree,
        ["oneArg"u8] = oneArg,
        ["returnInt"u8] = returnInt,
        ["stringer"u8] = stringer,
        ["twoArgs"u8] = twoArgs,
        ["typeOf"u8] = typeOf,
        ["valueString"u8] = valueString,
        ["vfunc"u8] = vfunc,
        ["zeroArgs"u8] = zeroArgs
    });
    foreach (var (_, test) in execTests) {
        ж<global::go.html.template_package.Template> tmpl = default!;
        error err = default!;
        if (Ꮡtemplate == nil){
            (tmpl, err) = New(test.name).Funcs(funcs).Parse(test.input);
        } else {
            (tmpl, err) = Ꮡtemplate.Clone();
            if (err != default!) {
                Ꮡt.Errorf("%s: clone error: %s"u8, test.name, err);
                continue;
            }
            (tmpl, err) = tmpl.New(test.name).Funcs(funcs).Parse(test.input);
        }
        if (err != default!) {
            Ꮡt.Errorf("%s: parse error: %s"u8, test.name, err);
            continue;
        }
        b.Reset();
        err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), test.data);
        switch (ᐧ) {
        case {} when !test.ok && err == default!: {
            Ꮡt.Errorf("%s: expected error; got none"u8, test.name);
            continue;
            break;
        }
        case {} when test.ok && err != default!: {
            Ꮡt.Errorf("%s: unexpected execute error: %s"u8, test.name, err);
            continue;
            break;
        }
        case {} when !test.ok && err != default!: {
            if (debug.Value) {
                // expected error, got one
                fmt.Printf("%s: %s\n\t%s\n"u8, test.name, test.input, err);
            }
            break;
        }}

        @string result = b.String();
        if (result != test.output) {
            Ꮡt.Errorf("%s: expected\n\t%q\ngot\n\t%q"u8, test.name, test.output, result);
        }
    }
}

public static void TestExecute(ж<testing.T> Ꮡt) {
    testExecute(execTests, nil, Ꮡt);
}

// default
// same as default
// same
// peculiar
internal static slice<@string> delimPairs = new @string[]{
    ""u8, ""u8,
    "{{"u8, "}}"u8,
    "|"u8, "|"u8,
    "(日)"u8, "(本)"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string strˢ = ".Str"u8;
internal static readonly @string delimsˢ = "delims"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string helloᶜ = "Hello, world"u8;

[GoType("dyn")] internal partial struct TestDelims_type {
    public @string Str;
}

public static void TestDelims(ж<testing.T> Ꮡt) {
    @string hello = helloᶜ;
    TestDelims_type value = new TestDelims_type(hello);
    for (nint i = 0; i < len(delimPairs); i += 2) {
        @string text = strˢ;
        @string left = delimPairs[i + 0];
        @string trueLeft = left;
        @string right = delimPairs[i + 1];
        @string trueRight = right;
        if (left == ""u8) {
            // default case
            trueLeft = "{{"u8;
        }
        if (right == ""u8) {
            // default case
            trueRight = "}}"u8;
        }
        text = trueLeft + text + trueRight;
        // Now add a comment
        text += trueLeft + "/*comment*/"u8 + trueRight;
        // Now add  an action containing a string.
        text += trueLeft + @""""u8 + trueLeft + @""""u8 + trueRight;
        // At this point text looks like `{{.Str}}{{/*comment*/}}{{"{{"}}`.
        var (tmpl, err) = New(delimsˢ).Delims(left, right).Parse(text);
        if (err != default!) {
            Ꮡt.Fatalf("delim %q text %q parse err %s"u8, left, text, err);
        }
        ж<strings.Builder> b = @new<strings.Builder>();
        err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), value);
        if (err != default!) {
            Ꮡt.Fatalf("delim %q exec err %s"u8, left, err);
        }
        if (b.String() != hello + trueLeft) {
            Ꮡt.Errorf("expected %q got %q"u8, hello + trueLeft, b.String());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ = "error"u8;
internal static readonly @string myErrorTrueˢ = "{{.MyError true}}"u8;

// Check that an error from a method flows back to the top.
public static void TestExecuteError(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var tmpl = New(errorˢ);
    var (_, err) = tmpl.Parse(myErrorTrueˢ);
    if (err != default!) {
        Ꮡt.Fatalf("parse error: %s"u8, err);
    }
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(b), tVal.OrTypedNil());
    if (err == default!){
        Ꮡt.Errorf("expected error; got none"u8);
    } else 
    if (!strings.Contains(err.Error(), myError.Error())) {
        if (debug.Value) {
            fmt.Printf("test execute error: %s\n"u8, err);
        }
        Ꮡt.Errorf("expected myError; got %s"u8, err);
    }
}

internal static readonly @string execErrorText = """
line 1
line 2
line 3
{{template "one" .}}
{{define "one"}}{{template "two" .}}{{end}}
{{define "two"}}{{template "three" .}}{{end}}
{{define "three"}}{{index "hi" $}}{{end}}
"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string topˢ = "top"u8;
internal static readonly object parseErrorˢ = (@string)"parse error:"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ2 = @"template: top:7:20: executing ""three"" at <index ""hi"" $>: error calling index: index out of range: 5"u8;

// Check that an error from a nested template contains all the relevant information.
public static void TestExecError(ж<testing.T> Ꮡt) {
    var (tmpl, err) = New(topˢ).Parse(execErrorText);
    if (err != default!) {
        Ꮡt.Fatal(parseErrorˢ, err);
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)(5)); // 5 is out of range indexing "hi"
    if (err == default!) {
        Ꮡt.Fatal(expectedErrorˢ);
    }
    @string want = wantᶜ2;
    @string got = err.Error();
    if (got != want) {
        Ꮡt.Errorf("expected\n%q\ngot\n%q"u8, want, got);
    }
}

[GoType("dyn")] internal partial struct TestJSEscaping_testCases {
    internal @string @in, exp;
}

public static void TestJSEscaping(ж<testing.T> Ꮡt) {
    var testCases = new TestJSEscaping_testCases[]{
        new(@"a"u8, @"a"u8),
        new(@"'foo"u8, @"\'foo"u8),
        new(@"Go ""jump"" \"u8, @"Go \""jump\"" \\"u8),
        new(@"Yukihiro says ""今日は世界"""u8, @"Yukihiro says \""今日は世界\"""u8),
        new("unprintable \uFFFE"u8, @"unprintable \uFFFE"u8),
        new(@"<html>"u8, @"\u003Chtml\u003E"u8),
        new(@"no = in attributes"u8, @"no \u003D in attributes"u8),
        new(@"&#x27; does not become HTML entity"u8, @"\u0026#x27; does not become HTML entity"u8)
    }.slice();
    foreach (var (_, tc) in testCases) {
        @string s = JSEscapeString(tc.@in);
        if (s != tc.exp) {
            Ꮡt.Errorf("JS escaping [%s] got [%s] want [%s]"u8, tc.@in, s, tc.exp);
        }
    }
}

// A nice example: walk a binary tree.
[GoType] public partial struct Tree {
    public nint Val;
    public ж<Tree> Left, Right;
}

// Use different delimiters to test Set.Delims.
// Also test the trimming of leading and trailing spaces.
internal static readonly @string treeTemplate = """

	(- define "tree" -)
	[
		(- .Val -)
		(- with .Left -)
			(template "tree" . -)
		(- end -)
		(- with .Right -)
			(- template "tree" . -)
		(- end -)
	]
	(- end -)

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string treeˢ = "tree"u8;
internal static readonly object execErrorˢ = (@string)"exec error:"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectᶜ1 = "[1[2[3[4]][5[6]]][7[8[9]][10[11]]]]"u8;

public static void TestTree(ж<testing.T> Ꮡt) {
    ж<Tree> tree = Ꮡ(new Tree(
        1,
        Ꮡ(new Tree(
            2, Ꮡ(new Tree(
            3,
            Ꮡ(new Tree(
                4, nil, nil
            )),
            nil
        )),
            Ꮡ(new Tree(
                5,
                Ꮡ(new Tree(
                    6, nil, nil
                )),
                nil
            ))
        )),
        Ꮡ(new Tree(
            7,
            Ꮡ(new Tree(
                8,
                Ꮡ(new Tree(
                    9, nil, nil
                )),
                nil
            )),
            Ꮡ(new Tree(
                10,
                Ꮡ(new Tree(
                    11, nil, nil
                )),
                nil
            ))
        ))
    ));
    var (tmpl, err) = New(rootˢ).Delims("("u8, ")"u8).Parse(treeTemplate);
    if (err != default!) {
        Ꮡt.Fatal(parseErrorˢ, err);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    @string expect = expectᶜ1;
    // First by looking up the template.
    err = tmpl.Lookup(treeˢ).Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), tree.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(execErrorˢ, err);
    }
    @string result = b.String();
    if (result != expect) {
        Ꮡt.Errorf("expected %q got %q"u8, expect, result);
    }
    // Then direct to execution.
    b.Reset();
    err = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), treeˢ, tree.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(execErrorˢ, err);
    }
    result = b.String();
    if (result != expect) {
        Ꮡt.Errorf("expected %q got %q"u8, expect, result);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nameˢ = "Name"u8;

public static void TestExecuteOnNewTemplate(ж<testing.T> Ꮡt) {
    // This is issue 3872.
    New(nameˢ).Templates();
}

// This is issue 11379.
// new(Template).Templates() // TODO: crashes
// new(Template).Parse("") // TODO: crashes
// new(Template).New("abc").Parse("") // TODO: crashes
// new(Template).Execute(nil, nil)                // TODO: crashes; returns an error (but does not crash)
// new(Template).ExecuteTemplate(nil, "XXX", nil) // TODO: crashes; returns an error (but does not crash)
internal static readonly @string testTemplates = @"{{define ""one""}}one{{end}}{{define ""two""}}two{{end}}"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyˢ = "empty"u8;
internal static readonly object expectedInitialErrorˢ = (@string)"expected initial error"u8;
internal static readonly @string templateEmptyIsAnˢ = @"template: ""empty"" is an incomplete or empty template"u8;
internal static readonly @string secondaryˢ = "secondary"u8;
internal static readonly object expectedSecondErrorˢ = (@string)"expected second error"u8;

public static void TestMessageForExecuteEmpty(ж<testing.T> Ꮡt) {
    // Test a truly empty template.
    var tmpl = New(emptyˢ);
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)(0));
    if (err == default!) {
        Ꮡt.Fatal(expectedInitialErrorˢ);
    }
    @string got = err.Error();
    @string want = templateEmptyIsAnˢ; // NOTE: text/template has extra "empty: " in message
    if (got != want) {
        Ꮡt.Errorf("expected error %s got %s"u8, want, got);
    }
    // Add a non-empty template to check that the error is helpful.
    tmpl = New(emptyˢ);
    (var tests, err) = New(""u8).Parse(testTemplates);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    tmpl.AddParseTree(secondaryˢ, (~tests).Tree);
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)(0));
    if (err == default!) {
        Ꮡt.Fatal(expectedSecondErrorˢ);
    }
    got = err.Error();
    if (got != want) {
        Ꮡt.Errorf("expected error %s got %s"u8, want, got);
    }
    // Make sure we can execute the secondary.
    err = tmpl.ExecuteTemplate(new template_test_package.bytes_BufferжWriter(Ꮡb), secondaryˢ, (nint)(0));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xPrintfˢ = @"{{""x"" | printf}}"u8;

public static void TestFinalForPrintf(ж<testing.T> Ꮡt) {
    var (tmpl, err) = New(""u8).Parse(xPrintfˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)(0));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

[GoType] internal partial struct cmpTest {
    internal @string expr;
    internal @string truth;
    internal bool ok;
}

// Mixing signed and unsigned integers.
// The example that triggered this rule.
// Uncomparable types but nil is OK.
// Uncomparable types but nil is OK.
// Uncomparable types but nil is OK.
// Uncomparable types but nil is OK.
// Errors
// Different types.
// Different types.
// Unordered types.
// Unordered types.
// Incompatible types.
// Incompatible types.
// Uncomparable types.
// Uncomparable types.
internal static slice<cmpTest> cmpTests = new cmpTest[]{
    new("eq true true"u8, "true"u8, true),
    new("eq true false"u8, "false"u8, true),
    new("eq 1+2i 1+2i"u8, "true"u8, true),
    new("eq 1+2i 1+3i"u8, "false"u8, true),
    new("eq 1.5 1.5"u8, "true"u8, true),
    new("eq 1.5 2.5"u8, "false"u8, true),
    new("eq 1 1"u8, "true"u8, true),
    new("eq 1 2"u8, "false"u8, true),
    new("eq `xy` `xy`"u8, "true"u8, true),
    new("eq `xy` `xyz`"u8, "false"u8, true),
    new("eq .Uthree .Uthree"u8, "true"u8, true),
    new("eq .Uthree .Ufour"u8, "false"u8, true),
    new("eq 3 4 5 6 3"u8, "true"u8, true),
    new("eq 3 4 5 6 7"u8, "false"u8, true),
    new("ne true true"u8, "false"u8, true),
    new("ne true false"u8, "true"u8, true),
    new("ne 1+2i 1+2i"u8, "false"u8, true),
    new("ne 1+2i 1+3i"u8, "true"u8, true),
    new("ne 1.5 1.5"u8, "false"u8, true),
    new("ne 1.5 2.5"u8, "true"u8, true),
    new("ne 1 1"u8, "false"u8, true),
    new("ne 1 2"u8, "true"u8, true),
    new("ne `xy` `xy`"u8, "false"u8, true),
    new("ne `xy` `xyz`"u8, "true"u8, true),
    new("ne .Uthree .Uthree"u8, "false"u8, true),
    new("ne .Uthree .Ufour"u8, "true"u8, true),
    new("lt 1.5 1.5"u8, "false"u8, true),
    new("lt 1.5 2.5"u8, "true"u8, true),
    new("lt 1 1"u8, "false"u8, true),
    new("lt 1 2"u8, "true"u8, true),
    new("lt `xy` `xy`"u8, "false"u8, true),
    new("lt `xy` `xyz`"u8, "true"u8, true),
    new("lt .Uthree .Uthree"u8, "false"u8, true),
    new("lt .Uthree .Ufour"u8, "true"u8, true),
    new("le 1.5 1.5"u8, "true"u8, true),
    new("le 1.5 2.5"u8, "true"u8, true),
    new("le 2.5 1.5"u8, "false"u8, true),
    new("le 1 1"u8, "true"u8, true),
    new("le 1 2"u8, "true"u8, true),
    new("le 2 1"u8, "false"u8, true),
    new("le `xy` `xy`"u8, "true"u8, true),
    new("le `xy` `xyz`"u8, "true"u8, true),
    new("le `xyz` `xy`"u8, "false"u8, true),
    new("le .Uthree .Uthree"u8, "true"u8, true),
    new("le .Uthree .Ufour"u8, "true"u8, true),
    new("le .Ufour .Uthree"u8, "false"u8, true),
    new("gt 1.5 1.5"u8, "false"u8, true),
    new("gt 1.5 2.5"u8, "false"u8, true),
    new("gt 1 1"u8, "false"u8, true),
    new("gt 2 1"u8, "true"u8, true),
    new("gt 1 2"u8, "false"u8, true),
    new("gt `xy` `xy`"u8, "false"u8, true),
    new("gt `xy` `xyz`"u8, "false"u8, true),
    new("gt .Uthree .Uthree"u8, "false"u8, true),
    new("gt .Uthree .Ufour"u8, "false"u8, true),
    new("gt .Ufour .Uthree"u8, "true"u8, true),
    new("ge 1.5 1.5"u8, "true"u8, true),
    new("ge 1.5 2.5"u8, "false"u8, true),
    new("ge 2.5 1.5"u8, "true"u8, true),
    new("ge 1 1"u8, "true"u8, true),
    new("ge 1 2"u8, "false"u8, true),
    new("ge 2 1"u8, "true"u8, true),
    new("ge `xy` `xy`"u8, "true"u8, true),
    new("ge `xy` `xyz`"u8, "false"u8, true),
    new("ge `xyz` `xy`"u8, "true"u8, true),
    new("ge .Uthree .Uthree"u8, "true"u8, true),
    new("ge .Uthree .Ufour"u8, "false"u8, true),
    new("ge .Ufour .Uthree"u8, "true"u8, true),
    new("eq .Uthree .Three"u8, "true"u8, true),
    new("eq .Three .Uthree"u8, "true"u8, true),
    new("le .Uthree .Three"u8, "true"u8, true),
    new("le .Three .Uthree"u8, "true"u8, true),
    new("ge .Uthree .Three"u8, "true"u8, true),
    new("ge .Three .Uthree"u8, "true"u8, true),
    new("lt .Uthree .Three"u8, "false"u8, true),
    new("lt .Three .Uthree"u8, "false"u8, true),
    new("gt .Uthree .Three"u8, "false"u8, true),
    new("gt .Three .Uthree"u8, "false"u8, true),
    new("eq .Ufour .Three"u8, "false"u8, true),
    new("lt .Ufour .Three"u8, "false"u8, true),
    new("gt .Ufour .Three"u8, "true"u8, true),
    new("eq .NegOne .Uthree"u8, "false"u8, true),
    new("eq .Uthree .NegOne"u8, "false"u8, true),
    new("ne .NegOne .Uthree"u8, "true"u8, true),
    new("ne .Uthree .NegOne"u8, "true"u8, true),
    new("lt .NegOne .Uthree"u8, "true"u8, true),
    new("lt .Uthree .NegOne"u8, "false"u8, true),
    new("le .NegOne .Uthree"u8, "true"u8, true),
    new("le .Uthree .NegOne"u8, "false"u8, true),
    new("gt .NegOne .Uthree"u8, "false"u8, true),
    new("gt .Uthree .NegOne"u8, "true"u8, true),
    new("ge .NegOne .Uthree"u8, "false"u8, true),
    new("ge .Uthree .NegOne"u8, "true"u8, true),
    new("eq (index `x` 0) 'x'"u8, "true"u8, true),
    new("eq (index `x` 0) 'y'"u8, "false"u8, true),
    new("eq .V1 .V2"u8, "true"u8, true),
    new("eq .Ptr .Ptr"u8, "true"u8, true),
    new("eq .Ptr .NilPtr"u8, "false"u8, true),
    new("eq .NilPtr .NilPtr"u8, "true"u8, true),
    new("eq .Iface1 .Iface1"u8, "true"u8, true),
    new("eq .Iface1 .Iface2"u8, "false"u8, true),
    new("eq .Iface2 .Iface2"u8, "true"u8, true),
    new("eq .Map .Map"u8, "true"u8, true),
    new("eq .Map nil"u8, "true"u8, true),
    new("eq nil .Map"u8, "true"u8, true),
    new("eq .Map .NonNilMap"u8, "false"u8, true),
    new("eq `xy` 1"u8, ""u8, false),
    new("eq 2 2.0"u8, ""u8, false),
    new("lt true true"u8, ""u8, false),
    new("lt 1+0i 1+0i"u8, ""u8, false),
    new("eq .Ptr 1"u8, ""u8, false),
    new("eq .Ptr .NegOne"u8, ""u8, false),
    new("eq .Map .V1"u8, ""u8, false),
    new("eq .NonNilMap .NonNilMap"u8, ""u8, false)
}.slice();

[GoType("dyn")] internal partial struct TestComparison_type {
    public nuint Uthree, Ufour;
    public nint NegOne, Three;
    public ж<nint> Ptr, NilPtr;
    public map<nint, nint> NonNilMap;
    public map<nint, nint> Map;
    public V V1, V2;
    public fmt.Stringer Iface1, Iface2;
}

public static void TestComparison(ж<testing.T> Ꮡt) {
    var b = @new<strings.Builder>();
    ref var cmpStruct = ref heap(new TestComparison_type(), out var ᏑcmpStruct);

    cmpStruct = new TestComparison_type(
        Uthree: 3,
        Ufour: 4,
        NegOne: -1,
        Three: 3,
        Ptr: @new<nint>(),
        NonNilMap: new map<nint, nint>(),
        Iface1: new template_test_package.strings_BuilderжStringer(b)
    );
    foreach (var (_, test) in cmpTests) {
        @string text = fmt.Sprintf("{{if %s}}true{{else}}false{{end}}"u8, test.expr);
        var (tmpl, err) = New(emptyˢ).Parse(text);
        if (err != default!) {
            Ꮡt.Fatalf("%q: %s"u8, test.expr, err);
        }
        b.Reset();
        err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), ᏑcmpStruct);
        if (test.ok && err != default!) {
            Ꮡt.Errorf("%s errored incorrectly: %s"u8, test.expr, err);
            continue;
        }
        if (!test.ok && err == default!) {
            Ꮡt.Errorf("%s did not error"u8, test.expr);
            continue;
        }
        if (b.String() != test.truth) {
            Ꮡt.Errorf("%s: want %s; got %s"u8, test.expr, test.truth, b.String());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingkeyDefaultˢ = "missingkey=default"u8;
internal static readonly object defaultˢ = (@string)"default:"u8;
internal static readonly @string missingkeyZeroˢ = "missingkey=zero"u8;
internal static readonly object zeroˢ = (@string)"zero:"u8;
internal static readonly @string missingkeyErrorˢ = "missingkey=error"u8;

public static void TestMissingMapKey(ж<testing.T> Ꮡt) {
    var data = new map<@string, nint>{
        ["x"u8] = 99
    };
    var (tmpl, err) = New("t1"u8).Parse("{{.x}} {{.y}}"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    // By default, just get "<no value>" // NOTE: not in html/template, get empty string
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string want = "99 "u8;
    @string got = b.String();
    if (got != want) {
        Ꮡt.Errorf("got %q; expected %q"u8, got, want);
    }
    // Same if we set the option explicitly to the default.
    tmpl.Option(missingkeyDefaultˢ);
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err != default!) {
        Ꮡt.Fatal(defaultˢ, err);
    }
    got = b.String();
    if (got != want) {
        Ꮡt.Errorf("got %q; expected %q"u8, got, want);
    }
    // Next we ask for a zero value
    tmpl.Option(missingkeyZeroˢ);
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err != default!) {
        Ꮡt.Fatal(zeroˢ, err);
    }
    want = "99 0"u8;
    got = b.String();
    if (got != want) {
        Ꮡt.Errorf("got %q; expected %q"u8, got, want);
    }
    // Now we ask for an error.
    tmpl.Option(missingkeyErrorˢ);
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err == default!) {
        Ꮡt.Errorf("expected error; got none"u8);
    }
    // same Option, but now a nil interface: ask for an error
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), default!);
    Ꮡt.Log(err);
    if (err == default!) {
        Ꮡt.Errorf("expected error for nil-interface; got none"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloUnterminatedSomeˢ = "hello\n\n{{`unterminated\n\n\n\n}}\n some more\n\n"u8;
internal static readonly @string x3UnterminatedRawQuotedˢ = "X:3: unterminated raw quoted string"u8;

// Test that the error message for multiline unterminated string
// refers to the line number of the opening quote.
public static void TestUnterminatedStringError(ж<testing.T> Ꮡt) {
    var (_, err) = New("X"u8).Parse(helloUnterminatedSomeˢ);
    if (err == default!) {
        Ꮡt.Fatal(expectedErrorˢ);
    }
    @string str = err.Error();
    if (!strings.Contains(str, x3UnterminatedRawQuotedˢ)) {
        Ꮡt.Fatalf("unexpected error: %s"u8, str);
    }
}

internal static readonly @string alwaysErrorText = "always be failing"u8;

internal static error alwaysError = errors.New(alwaysErrorText);

[GoType("num:nint")] public partial struct ErrorWriter;

public static (nint, error) Write(this ErrorWriter e, slice<byte> p) {
    return (0, alwaysError);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error; got none"u8;
internal static readonly @string helloXYˢ = "hello, {{.X.Y}}"u8;
internal static readonly @string fieldXInTypeIntˢ = "field X in type int"u8;

public static void TestExecuteGivesExecError(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // First, a non-execution error shouldn't be an ExecError.
    var (tmpl, err) = New("X"u8).Parse(helloˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = tmpl.Execute(((ErrorWriter)0), (nint)(0));
    if (err == default!) {
        Ꮡt.Fatal(expectedErrorGotNoneˢ);
    }
    if (err.Error() != alwaysErrorText) {
        Ꮡt.Errorf("expected %q error; got %q"u8, alwaysErrorText, err);
    }
    // This one should be an ExecError.
    (tmpl, err) = New("X"u8).Parse(helloXYˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = tmpl.Execute(io.Discard, (nint)(0));
    if (err == default!) {
        Ꮡt.Fatal(expectedErrorGotNoneˢ);
    }
    var (eerr, ok) = err._<text.template_package.ExecError>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("did not expect ExecError %s"u8, eerr);
    }
    @string expect = fieldXInTypeIntˢ;
    if (!strings.Contains(err.Error(), expect)) {
        Ꮡt.Errorf("expected %q; got %q"u8, expect, err);
    }
}

internal static nint funcNameTestFunc() {
    return 0;
}

public static void TestGoodFuncNames(ж<testing.T> Ꮡt) {
    var names = new @string[]{
        "_"u8,
        "a"u8,
        "a1"u8,
        "a1"u8,
        "Ӵ"u8
    }.slice();
    foreach (var (_, name) in names) {
        var tmpl = New("X"u8).Funcs(
            new FuncMap(new map<@string, any>{
                [name] = funcNameTestFunc
            }));
        if (tmpl == nil) {
            Ꮡt.Fatalf("nil result for %q"u8, name);
        }
    }
}

public static void TestBadFuncNames(ж<testing.T> Ꮡt) {
    var names = new @string[]{
        ""u8,
        "2"u8,
        "a-b"u8
    }.slice();
    foreach (var (_, name) in names) {
        testBadFuncName(name, Ꮡt);
    }
}

internal static void testBadFuncName(@string name, ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        defer(() => {
            recover();
        }, ref ᒐ);
        New("X"u8).Funcs(
            new FuncMap(new map<@string, any>{
                [name] = funcNameTestFunc
            }));
        // If we get here, the name did not cause a panic, which is how Funcs
        // reports an error.
        Ꮡt.Errorf("%q succeeded incorrectly as function name"u8, name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goodbyeˢ = (@string)"goodbye"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string inputᶜ1 = @"a({{block ""inner"" .}}bar({{.}})baz{{end}})b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ3 = @"a(bar(hello)baz)b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string overlayᶜ1 = @"{{define ""inner""}}foo({{.}})bar{{end}}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string want2ᶜ = @"a(foo(goodbye)bar)b"u8;

public static void TestBlock(ж<testing.T> Ꮡt) {
    @string input = inputᶜ1;
    @string want = wantᶜ3;
    @string overlay = overlayᶜ1;
    @string want2 = want2ᶜ;
    var (tmpl, err) = New(outerˢ).Parse(input);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var (ᴛ1, ᴛ2) = tmpl.Clone();
    (var tmpl2, err) = Must(ᴛ1, ᴛ2).Parse(overlay);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    {
        var errΔ1 = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), helloˢ); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        @string got = buf.String(); if (got != want) {
            Ꮡt.Errorf("got %q, want %q"u8, got, want);
        }
    }
    buf.Reset();
    {
        var errΔ2 = tmpl2.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), goodbyeˢ); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    {
        @string got = buf.String(); if (got != want2) {
            Ꮡt.Errorf("got %q, want %q"u8, got, want2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmplˢ = "tmpl"u8;
internal static readonly @string nilˢ = "<nil>"u8;

[GoType("dyn")] internal partial struct TestEvalFieldErrors_tests {
    internal @string name, src;
    internal any value;
    internal @string want;
}

public static void TestEvalFieldErrors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestEvalFieldErrors_tests[]{
        new(
            "MissingFieldOnNil"u8, // Check that calling an invalid field on nil pointer
 // prints a field error instead of a distracting nil
 // pointer error. https://golang.org/issue/15125

            "{{.MissingField}}"u8,
            ((ж<T>)nil),
            "can't evaluate field MissingField in type *template.T"u8
        ),
        new(
            "MissingFieldOnNonNil"u8,
            "{{.MissingField}}"u8,
            Ꮡ(new T(nil)),
            "can't evaluate field MissingField in type *template.T"u8
        ),
        new(
            "ExistingFieldOnNil"u8,
            "{{.X}}"u8,
            ((ж<T>)nil),
            "nil pointer evaluating *template.T.X"u8
        ),
        new(
            "MissingKeyOnNilMap"u8,
            "{{.MissingKey}}"u8,
            ((ж<map<@string, @string>>)nil),
            "nil pointer evaluating *map[string]string.MissingKey"u8
        ),
        new(
            "MissingKeyOnNilMapPtr"u8,
            "{{.MissingKey}}"u8,
            ((ж<map<@string, @string>>)nil),
            "nil pointer evaluating *map[string]string.MissingKey"u8
        ),
        new(
            "MissingKeyOnMapPtrToNil"u8,
            "{{.MissingKey}}"u8,
            Ꮡ(new map<@string, @string>{}),
            "<nil>"u8
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestEvalFieldErrors_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (ᴛ3, ᴛ4) = New(tmplˢ).Parse(tcʗ1.src);
            var tmpl = Must(ᴛ3, ᴛ4);
            var err = tmpl.Execute(io.Discard, tcʗ1.value);
            @string got = nilˢ;
            if (err != default!) {
                got = err.Error();
            }
            if (!strings.HasSuffix(got, tcʗ1.want)) {
                tΔ1.Fatalf("got error %q, want %q"u8, got, tcʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;
internal static readonly @string templateTmplˢ = @"{{template ""tmpl"" .}}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ4 = "exceeded maximum template depth"u8;

public static void TestMaxExecDepth(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    var (ᴛ5, ᴛ6) = New(tmplˢ).Parse(templateTmplˢ);
    var tmpl = Must(ᴛ5, ᴛ6);
    var err = tmpl.Execute(io.Discard, default!);
    @string got = nilˢ;
    if (err != default!) {
        got = err.Error();
    }
    @string want = wantᶜ4;
    if (!strings.Contains(got, want)) {
        Ꮡt.Errorf("got error %q; want %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lt1Gtˢ = (@string)"&lt;1&gt;"u8;

public static void TestAddrOfIndex(ж<testing.T> Ꮡt) {
    // golang.org/issue/14916.
    // Before index worked on reflect.Values, the .String could not be
    // found on the (incorrectly unaddressable) V value,
    // in contrast to range, which worked fine.
    // Also testing that passing a reflect.Value to tmpl.Execute works.
    var texts = new @string[]{
        @"{{range .}}{{.String}}{{end}}"u8,
        @"{{with index . 0}}{{.String}}{{end}}"u8
    }.slice();
    foreach (var (_, text) in texts) {
        var (ᴛ7, ᴛ8) = New(tmplˢ).Parse(text);
        var tmpl = Must(ᴛ7, ᴛ8);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), reflect.ValueOf(new V[]{new(1)}.slice()));
        if (err != default!) {
            Ꮡt.Fatalf("%s: Execute: %v"u8, text, err);
        }
        if (buf.String() != "&lt;1&gt;"u8) {
            Ꮡt.Fatalf("%s: template output = %q, want %q"u8, text, Ꮡbuf, lt1Gtˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ2 = "ERROR:"u8;

[GoType("dyn")] internal partial struct TestInterfaceValues_tests {
    internal @string text;
    internal @string @out;
}

public static void TestInterfaceValues(ж<testing.T> Ꮡt) {
    // golang.org/issue/17714.
    // Before index worked on reflect.Values, interface values
    // were always implicitly promoted to the underlying value,
    // except that nil interfaces were promoted to the zero reflect.Value.
    // Eliminating a round trip to interface{} and back to reflect.Value
    // eliminated this promotion, breaking these cases.
    var tests = new TestInterfaceValues_tests[]{
        new(@"{{index .Nil 1}}"u8, "ERROR: index of untyped nil"u8),
        new(@"{{index .Slice 2}}"u8, "2"u8),
        new(@"{{index .Slice .Two}}"u8, "2"u8),
        new(@"{{call .Nil 1}}"u8, "ERROR: call of nil"u8),
        new(@"{{call .PlusOne 1}}"u8, "2"u8),
        new(@"{{call .PlusOne .One}}"u8, "2"u8),
        new(@"{{and (index .Slice 0) true}}"u8, "0"u8),
        new(@"{{and .Zero true}}"u8, "0"u8),
        new(@"{{and (index .Slice 1) false}}"u8, "false"u8),
        new(@"{{and .One false}}"u8, "false"u8),
        new(@"{{or (index .Slice 0) false}}"u8, "false"u8),
        new(@"{{or .Zero false}}"u8, "false"u8),
        new(@"{{or (index .Slice 1) true}}"u8, "1"u8),
        new(@"{{or .One true}}"u8, "1"u8),
        new(@"{{not (index .Slice 0)}}"u8, "true"u8),
        new(@"{{not .Zero}}"u8, "true"u8),
        new(@"{{not (index .Slice 1)}}"u8, "false"u8),
        new(@"{{not .One}}"u8, "false"u8),
        new(@"{{eq (index .Slice 0) .Zero}}"u8, "true"u8),
        new(@"{{eq (index .Slice 1) .One}}"u8, "true"u8),
        new(@"{{ne (index .Slice 0) .Zero}}"u8, "false"u8),
        new(@"{{ne (index .Slice 1) .One}}"u8, "false"u8),
        new(@"{{ge (index .Slice 0) .One}}"u8, "false"u8),
        new(@"{{ge (index .Slice 1) .Zero}}"u8, "true"u8),
        new(@"{{gt (index .Slice 0) .One}}"u8, "false"u8),
        new(@"{{gt (index .Slice 1) .Zero}}"u8, "true"u8),
        new(@"{{le (index .Slice 0) .One}}"u8, "true"u8),
        new(@"{{le (index .Slice 1) .Zero}}"u8, "false"u8),
        new(@"{{lt (index .Slice 0) .One}}"u8, "true"u8),
        new(@"{{lt (index .Slice 1) .Zero}}"u8, "false"u8)
    }.slice();
    foreach (var (_, tt) in tests) {
        var (ᴛ9, ᴛ10) = New(tmplˢ).Parse(tt.text);
        var tmpl = Must(ᴛ9, ᴛ10);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), new map<@string, any>{
            ["PlusOne"u8] = nint (nint n) => n + 1,
            ["Slice"u8] = new nint[]{0, 1, 2, 3}.slice(),
            ["One"u8] = (nint)(1),
            ["Two"u8] = (nint)(2),
            ["Nil"u8] = default!,
            ["Zero"u8] = (nint)(0)
        });
        if (strings.HasPrefix(tt.@out, errorˢ2)) {
            @string e = strings.TrimSpace(strings.TrimPrefix(tt.@out, errorˢ2));
            if (err == default! || !strings.Contains(err.Error(), e)) {
                Ꮡt.Errorf("%s: Execute: %v, want error %q"u8, tt.text, err, e);
            }
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("%s: Execute: %v"u8, tt.text, err);
            continue;
        }
        if (buf.String() != tt.@out) {
            Ꮡt.Errorf("%s: template output = %q, want %q"u8, tt.text, Ꮡbuf, tt.@out);
        }
    }
}

[GoType("dyn")] internal partial struct TestExecutePanicDuringCall_tests {
    internal @string name;
    internal @string input;
    internal any data;
    internal @string wantErr;
}

// Check that panics during calls are recovered and returned as errors.
public static void TestExecutePanicDuringCall(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var funcs = new map<@string, any>{
        ["doPanic"u8] = @string () => {
            throw panic("custom panic string");
        }
    };
    var tests = new TestExecutePanicDuringCall_tests[]{
        new(
            "direct func call panics"u8,
            "{{doPanic}}"u8, ((ж<T>)nil),
            @"template: t:1:2: executing ""t"" at <doPanic>: error calling doPanic: custom panic string"u8
        ),
        new(
            "indirect func call panics"u8,
            "{{call doPanic}}"u8, ((ж<T>)nil),
            @"template: t:1:7: executing ""t"" at <doPanic>: error calling doPanic: custom panic string"u8
        ),
        new(
            "direct method call panics"u8,
            "{{.GetU}}"u8, ((ж<T>)nil),
            @"template: t:1:2: executing ""t"" at <.GetU>: error calling GetU: runtime error: invalid memory address or nil pointer dereference"u8
        ),
        new(
            "indirect method call panics"u8,
            "{{call .GetU}}"u8, ((ж<T>)nil),
            @"template: t:1:7: executing ""t"" at <.GetU>: error calling GetU: runtime error: invalid memory address or nil pointer dereference"u8
        ),
        new(
            "func field call panics"u8,
            "{{call .PanicFunc}}"u8, tVal.OrTypedNil(),
            @"template: t:1:2: executing ""t"" at <call .PanicFunc>: error calling call: test panic"u8
        ),
        new(
            "method call on nil interface"u8,
            "{{.NonEmptyInterfaceNil.Method0}}"u8, tVal.OrTypedNil(),
            @"template: t:1:23: executing ""t"" at <.NonEmptyInterfaceNil.Method0>: nil pointer evaluating template.I.Method0"u8
        )
    }.slice();
    foreach (var (_, tc) in tests) {
        var b = @new<bytes.Buffer>();
        var (tmpl, err) = New("t"u8).Funcs(funcs).Parse(tc.input);
        if (err != default!) {
            Ꮡt.Fatalf("parse error: %s"u8, err);
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(b), tc.data);
        if (err == default!){
            Ꮡt.Errorf("%s: expected error; got none"u8, tc.name);
        } else 
        if (!strings.Contains(err.Error(), tc.wantErr)) {
            if (debug.Value) {
                fmt.Printf("%s: test execute error: %s\n"u8, tc.name, err);
            }
            Ꮡt.Errorf("%s: expected error:\n%s\ngot:\n%s"u8, tc.name, tc.wantErr, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object brokenInHtmlTemplateˢ = (@string)"broken in html/template"u8;
internal static readonly @string resultˢ = "result"u8;
internal static readonly object expectedErrorWithNoCallˢ = (@string)"expected error with no call, got none"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ = "{{ (.)  }}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textCallᶜ = "{{ (call .)  }}"u8;

// Issue 31810. Check that a parenthesized first argument behaves properly.
public static void TestIssue31810(ж<testing.T> Ꮡt) {
    Ꮡt.Skip(brokenInHtmlTemplateˢ);
    // A simple value with no arguments is fine.
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    @string text = textᶜ;
    var (tmpl, err) = New(""u8).Parse(text);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), resultˢ);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (b.String() != "result"u8) {
        Ꮡt.Errorf("%s got %q, expected %q"u8, text, b.String(), resultˢ);
    }
    // Even a plain function fails - need to use call.
    var f = @string () => resultˢ;
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), (f).OrTypedNilFunc());
    if (err == default!) {
        Ꮡt.Error(expectedErrorWithNoCallˢ);
    }
    // Works if the function is explicitly called.
    @string textCall = textCallᶜ;
    (tmpl, err) = New(""u8).Parse(textCall);
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), (f).OrTypedNilFunc());
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (b.String() != "result"u8) {
        Ꮡt.Errorf("%s got %q, expected %q"u8, textCall, b.String(), resultˢ);
    }
}

// Issue 39807. There was a race applying escapeTemplate.
internal static readonly @string raceText = """

{{- define "jstempl" -}}
var v = "v";
{{- end -}}
<script type="application/javascript">
{{ template "jstempl" $ }}
</script>

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string templHtmlˢ = "templ.html"u8;
internal static readonly @string templateTemplHtmlˢ = @"{{ template ""templ.html"" .}}"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestEscapeRace(ж<testing.T> Ꮡt) {
    var tmpl = New(""u8);
    var (_, err) = tmpl.New(templHtmlˢ).Parse(raceText);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    const nint count = 20;
    for (nint i = 0; i < count; i++) {
        var (_, errΔ1) = tmpl.New(fmt.Sprintf("x%d.html"u8, i)).Parse(templateTemplHtmlˢ);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 10; i++) {
        Ꮡwg.Add(1);
        var tmplʗ1 = tmpl;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint j = 0; j < count; j++) {
                    var sub = tmplʗ1.Lookup(fmt.Sprintf("x%d.html"u8, j));
                    {
                        var errΔ2 = sub.Execute(io.Discard, default!); if (errΔ2 != default!) {
                            Ꮡt.Error(errΔ2);
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string subroutineˢ = "subroutine"u8;
internal static readonly @string xHtmlˢ = "x.html"u8;
internal static readonly @string recurˢ = @"{{recur}}"u8;
internal static readonly @string aHrefXPABˢ = @"<a href=""/x?p={{""'a<b'""}}"">"u8;

public static void TestRecursiveExecute(ж<testing.T> Ꮡt) {
    var tmpl = New(""u8);
    var tmplʗ1 = tmpl;
    var recur = (global::go.html.template_package.HTML, error) () => {
        ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
        {
            var errΔ1 = tmplʗ1.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡsb), subroutineˢ, default!); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        return (((global::go.html.template_package.HTML)sb.String()), default!);
    };
    var m = new FuncMap(new map<@string, any>{
        ["recur"u8] = (recur).OrTypedNilFunc()
    });
    var (top, err) = tmpl.New(xHtmlˢ).Funcs(m).Parse(recurˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = tmpl.New(subroutineˢ).Parse(aHrefXPABˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ2 = top.Execute(io.Discard, default!); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
}

// recursiveInvoker is for TestRecursiveExecuteViaMethod.
[GoType] internal partial struct recursiveInvoker {
    internal ж<testing.T> t;
    internal ж<global::go.html.template_package.Template> tmpl;
}

[GoRecv] internal static (@string, error) Recur(this ref recursiveInvoker r) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    {
        var err = r.tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡsb), subroutineˢ, default!); if (err != default!) {
            r.t.Fatal(err);
        }
    }
    return (sb.String(), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string recurˢ2 = @"{{.Recur}}"u8;

public static void TestRecursiveExecuteViaMethod(ж<testing.T> Ꮡt) {
    var tmpl = New(""u8);
    var (top, err) = tmpl.New(xHtmlˢ).Parse(recurˢ2);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = tmpl.New(subroutineˢ).Parse(aHrefXPABˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var r = Ꮡ(new recursiveInvoker(
        t: Ꮡt,
        tmpl: tmpl
    ));
    {
        var errΔ1 = top.Execute(io.Discard, r.OrTypedNil()); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string origˢ = "orig"u8;
internal static readonly @string childˢ = "child"u8;

// Issue 43295.
public static void TestTemplateFuncsAfterClone(ж<testing.T> Ꮡt) {
    @string s = @"{{ f . }}"u8;
    @string want = testˢ;
    var orig = New(origˢ).Funcs(new map<@string, any>{
        ["f"u8] = @string (@string @in) => @in
    }).New(childˢ);
    var (ᴛ11, ᴛ12) = orig.Clone();

    var (ᴛ13, ᴛ14) = Must(ᴛ11, ᴛ12).Parse(s);
    var overviewTmpl = Must(ᴛ13, ᴛ14);
    ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
    {
        var err = overviewTmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡout), want); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = @out.String(); if (got != want) {
            Ꮡt.Fatalf("got %q; want %q"u8, got, want);
        }
    }
}

} // end template_internal_test_package
