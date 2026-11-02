// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using iter = iter_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using parse = go.text.template.parse_package;
using static go.text.template_package;
using ꓸꓸꓸnint = Span<nint>;
using ꓸꓸꓸstring = Span<@string>;

partial class template_internal_test_package {

internal static ж<bool> debug = flag.Bool("debug", false, "show the errors produced by the tests");

// T has lots of interesting pieces to use to test execution.
public partial struct T {
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
    public /*[3]*/ ж<array<nint>> PAI; // pointer to array
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
    public @unsafe.Pointer UPI;
    public @unsafe.Pointer EmptyUPI;
    // Function (not method)
    public Func<@string, @string, @string> BinaryFunc;
    public Funcꓸꓸꓸ<@string, @string> VariadicFunc;
    public Funcꓸꓸꓸ<nint, @string, @string> VariadicFuncInt;
    public Func<ж<nint>, bool> NilOKFunc;
    public Func<(@string, error)> ErrFunc;
    public Func<@string> PanicFunc;
    public Action TooFewReturnCountFunc;
    public Func<(@string, error, nint)> TooManyReturnCountFunc;
    public Func<(@string, bool)> InvalidReturnTypeFunc;
    // Template to test evaluation of templates.
    public ж<global::go.text.template_package.Template> Tmpl;
    // Unexported field; cannot be accessed by template.
    internal nint unexported;
}

public partial struct S /*[]@string*/;

public static @string Method0(this S _) {
    return "M0";
}

public partial struct U {
    public @string V;
}

public partial struct V {
    internal nint j;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilVˢ => ref nilVˢᵇ;
private static readonly @string nilVˢᵇ = "nilV"u8;

public static @string String(this ж<V> Ꮡv) {
    ref var v = ref Ꮡv.DerefOrNull();

    if (Ꮡv == nil) {
        return nilVˢ;
    }
    return fmt.Sprintf("<%d>"u8, v.j);
}

public partial struct W {
    internal nint k;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilWˢ => ref nilWˢᵇ;
private static readonly @string nilWˢᵇ = "nilW"u8;

public static @string Error(this ж<W> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (Ꮡw == nil) {
        return nilWˢ;
    }
    return fmt.Sprintf("[%d]"u8, w.k);
}

internal static readonly ж<I> ᏑsiVal = new StaticBox<I>(((I)new S(new @string[]{"a", "b"}.slice())));
internal static ref I siVal => ref ((StaticBox<I>)ᏑsiVal).ValueSlot;

internal static ж<T> tVal;
internal static void initᴛtVal() {
    var (ᴛ1, ᴛ2) = New("x").Parse("test template");
    tVal = Ꮡ(new T(
    True: true,
    I: 17,
    U16: 16,
    X: "x",
    S: "xyz",
    U: Ꮡ(new U("v")),
    V0: new V(6666),
    V1: Ꮡ(new V(7777)), // leave V2 as nil
    W0: new W(888),
    W1: Ꮡ(new W(999)), // leave W2 as nil
    SI: slice<nint>([3, 4, 5]),
    SICap: new slice<nint>(5, 10),
    AI: new nint[]{3, 4, 5}.array(),
    PAI: Ꮡ(new nint[]{3, 4, 5}.array()),
    SB: slice<bool>([true, false]),
    MSI: new map<@string, nint>{["one"] = 1, ["two"] = 2, ["three"] = 3},
    MSIone: new map<@string, nint>{["one"] = 1},
    MXI: new map<any, nint>{[(@string)"one"] = 1},
    MII: new map<nint, nint>{[1] = 1},
    MI32S: new map<int32, @string>{[1] = "one", [2] = "two"},
    MI64S: new map<int64, @string>{[2] = "i642", [3] = "i643"},
    MUI32S: new map<uint32, @string>{[2] = "u322", [3] = "u323"},
    MUI64S: new map<uint64, @string>{[2] = "ui642", [3] = "ui643"},
    MI8S: new map<int8, @string>{[2] = "i82", [3] = "i83"},
    MUI8S: new map<uint8, @string>{[2] = "u82", [3] = "u83"},
    SMSI: slice<map<@string, nint>>([
        new map<@string, nint>{["one"] = 1, ["two"] = 2},
        new map<@string, nint>{["eleven"] = 11, ["twelve"] = 12}
    ]),
    Empty1: (nint)3,
    Empty2: (@string)"empty2",
    Empty3: slice<nint>([7, 8]),
    Empty4: Ꮡ(new U("UinEmpty")),
    NonEmptyInterface: new template_internal_test_package.TжI(Ꮡ(new T(X: "x"))),
    NonEmptyInterfacePtS: ᏑsiVal,
    NonEmptyInterfaceTypedNil: new template_internal_test_package.TжI(((ж<T>)nil)),
    Str: new template_test_package.bytes_BufferжStringer(bytes.NewBuffer(slice<byte>("foozle"u8))),
    Err: errors.New("erroozle"),
    PI: newInt(23),
    PS: newString("a string"),
    PSI: newIntSlice(21, 22, 23),
    UPI: (uintptr)newUnsafePointer(23),
    BinaryFunc: (@string a, @string b) => fmt.Sprintf("[%s=%s]"u8, a, b),
    VariadicFunc: (params ꓸꓸꓸstring sʗp) => {
        var s = sʗp.slice();
        return fmt.Sprint("<", strings.Join(s, "+"), ">");
    },
    VariadicFuncInt: (nint a, params ꓸꓸꓸstring sʗp) => {
        var s = sʗp.slice();
        return fmt.Sprint(a, "=<", strings.Join(s, "+"), ">");
    },
    NilOKFunc: (ж<nint> s) => s == nil,
    ErrFunc: () => ("bla", default),
    PanicFunc: () => {
        throw panic("test panic");
    },
    TooFewReturnCountFunc: () => {
    },
    TooManyReturnCountFunc: () => ("", default, 0),
    InvalidReturnTypeFunc: () => ("", false),
    Tmpl: Must(ᴛ1, ᴛ2) // "x" is the value of .X
));
}

internal static slice<ж<T>> tSliceOfNil = slice<ж<T>>([default]);

// A non-empty interface.
public partial interface I {
    @string Method0();
}

internal static readonly ж<I> ᏑiVal = new StaticBox<I>(default(I));
internal static ref I iVal => ref ((StaticBox<I>)ᏑiVal).ValueSlot;
internal static void initᴛiVal() { iVal = new template_internal_test_package.TжI(tVal); }

// Helpers for creation.
internal static ж<nint> newInt(nint nʗp) {
    ref var n = ref heap(nʗp, out var Ꮡn);

    return Ꮡn;
}

internal static @unsafe.Pointer newUnsafePointer(nint nʗp) {
    ref var n = ref heap(nʗp, out var Ꮡn);

    return @unsafe.Pointer.FromPinnedBox(Ꮡn);
}

internal static ж<@string> newString(@string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    return Ꮡs;
}

internal static ж<slice<nint>> newIntSlice(params ꓸꓸꓸnint nʗp) {
    var n = nʗp.sslice();

    var p = @new<slice<nint>>();
    p.ValueSlot = new slice<nint>(len(n));
    builtin.copy(p.ValueSlot, n);
    return p;
}

// Simple methods with and without arguments.
public static @string Method0(this ref T t) {
    return "M0";
}

public static nint Method1(this ref T t, nint a) {
    return a;
}

public static @string Method2(this ref T t, uint16 a, @string b) {
    return fmt.Sprintf("Method2: %d %s"u8, a, b);
}

public static @string Method3(this ref T t, any v) {
    return fmt.Sprintf("Method3: %v"u8, v);
}

public static ж<T> Copy(this ref T t) {
    var n = @new<T>();
    n.Value = t.ΔClone();
    return n;
}

public static slice<nint> MAdd(this ref T t, nint a, slice<nint> b) {
    var v = new slice<nint>(len(b));
    foreach (var (i, x) in b) {
        v[i] = x + a;
    }
    return v;
}

internal static error myError = errors.New("my error");

// MyError returns a value and an error according to its argument.
public static (bool, error) MyError(this ref T t, bool error) {
    if (error) {
        return (true, myError);
    }
    return (false, default);
}

// A few methods to test chaining.
public static ж<U> GetU(this ref T t) {
    return t.U;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;

public static @string TrueFalse(this ref U u, bool b) {
    if (b) {
        return trueˢ;
    }
    return ""u8;
}

internal static @string typeOf(any arg) {
    return fmt.Sprintf("%T"u8, arg);
}

internal partial struct execTest {
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

    partial struct Δtype /*dyn*/ {
        internal nint a;
        internal @string b;
    }
internal static slice<execTest> execTests;
internal static void initᴛexecTests() { execTests = slice<execTest>([
    // Trivial cases.
    new("empty", "", "", default, true),
    new("text", "some text", "some text", default, true),
    new("nil action", "{{nil}}", "", default, false),

    // Ideal constants.
    new("ideal int", "{{typeOf 3}}", "int", (nint)0, true),
    new("ideal float", "{{typeOf 1.0}}", "float64", (nint)0, true),
    new("ideal exp float", "{{typeOf 1e1}}", "float64", (nint)0, true),
    new("ideal complex", "{{typeOf 1i}}", "complex128", (nint)0, true),
    new("ideal int", "{{typeOf "u8 + bigInt + "}}"u8, "int", (nint)0, true),
    new("ideal too big", "{{typeOf "u8 + bigUint + "}}"u8, "", (nint)0, false),
    new("ideal nil without type", "{{nil}}", "", (nint)0, false),

    // Fields of structs.
    new(".X", "-{{.X}}-", "-x-", tVal.OrTypedNil(), true),
    new(".U.V", "-{{.U.V}}-", "-v-", tVal.OrTypedNil(), true),
    new(".unexported", "{{.unexported}}", "", tVal.OrTypedNil(), false),

    // Fields on maps.
    new("map .one", "{{.MSI.one}}", "1", tVal.OrTypedNil(), true),
    new("map .two", "{{.MSI.two}}", "2", tVal.OrTypedNil(), true),
    new("map .NO", "{{.MSI.NO}}", "<no value>", tVal.OrTypedNil(), true),
    new("map .one interface", "{{.MXI.one}}", "1", tVal.OrTypedNil(), true),
    new("map .WRONG args", "{{.MSI.one 1}}", "", tVal.OrTypedNil(), false),
    new("map .WRONG type", "{{.MII.one}}", "", tVal.OrTypedNil(), false),

    // Dots of all kinds to test basic evaluation.
    new("dot int", "<{{.}}>", "<13>", (nint)13, true),
    new("dot uint", "<{{.}}>", "<14>", (nuint)14, true),
    new("dot float", "<{{.}}>", "<15.1>", 15.1D, true),
    new("dot bool", "<{{.}}>", "<true>", true, true),
    new("dot complex", "<{{.}}>", "<(16.2-17i)>", 16.2D + -17D.i(), true),
    new("dot string", "<{{.}}>", "<hello>", (@string)"hello", true),
    new("dot slice", "<{{.}}>", "<[-1 -2 -3]>", slice<nint>([-1, -2, -3]), true),
    new("dot map", "<{{.}}>", "<map[two:22]>", new map<@string, nint>{["two"] = 22}, true),
    new("dot struct", "<{{.}}>", "<{7 seven}>", new Δtype(7, "seven"), true),

    // Variables.
    new("$ int", "{{$}}", "123", (nint)123, true),
    new("$.I", "{{$.I}}", "17", tVal.OrTypedNil(), true),
    new("$.U.V", "{{$.U.V}}", "v", tVal.OrTypedNil(), true),
    new("declare in action", "{{$x := $.U.V}}{{$x}}", "v", tVal.OrTypedNil(), true),
    new("simple assignment", "{{$x := 2}}{{$x = 3}}{{$x}}", "3", tVal.OrTypedNil(), true),
    new("nested assignment",
        "{{$x := 2}}{{if true}}{{$x = 3}}{{end}}{{$x}}",
        "3", tVal.OrTypedNil(), true),
    new("nested assignment changes the last declaration",
        "{{$x := 1}}{{if true}}{{$x := 2}}{{if true}}{{$x = 3}}{{end}}{{end}}{{$x}}",
        "1", tVal.OrTypedNil(), true),

    // Type with String method.
    new("V{6666}.String()", "-{{.V0}}-", "-<6666>-", tVal.OrTypedNil(), true),
    new("&V{7777}.String()", "-{{.V1}}-", "-<7777>-", tVal.OrTypedNil(), true),
    new("(*V)(nil).String()", "-{{.V2}}-", "-nilV-", tVal.OrTypedNil(), true),

    // Type with Error method.
    new("W{888}.Error()", "-{{.W0}}-", "-[888]-", tVal.OrTypedNil(), true),
    new("&W{999}.Error()", "-{{.W1}}-", "-[999]-", tVal.OrTypedNil(), true),
    new("(*W)(nil).Error()", "-{{.W2}}-", "-nilW-", tVal.OrTypedNil(), true),

    // Pointers.
    new("*int", "{{.PI}}", "23", tVal.OrTypedNil(), true),
    new("*string", "{{.PS}}", "a string", tVal.OrTypedNil(), true),
    new("*[]int", "{{.PSI}}", "[21 22 23]", tVal.OrTypedNil(), true),
    new("*[]int[1]", "{{index .PSI 1}}", "22", tVal.OrTypedNil(), true),
    new("NIL", "{{.NIL}}", "<nil>", tVal.OrTypedNil(), true),

    // Empty interfaces holding values.
    new("empty nil", "{{.Empty0}}", "<no value>", tVal.OrTypedNil(), true),
    new("empty with int", "{{.Empty1}}", "3", tVal.OrTypedNil(), true),
    new("empty with string", "{{.Empty2}}", "empty2", tVal.OrTypedNil(), true),
    new("empty with slice", "{{.Empty3}}", "[7 8]", tVal.OrTypedNil(), true),
    new("empty with struct", "{{.Empty4}}", "{UinEmpty}", tVal.OrTypedNil(), true),
    new("empty with struct, field", "{{.Empty4.V}}", "UinEmpty", tVal.OrTypedNil(), true),

    // Edge cases with <no value> with an interface value
    new("field on interface", "{{.foo}}", "<no value>", default, true),
    new("field on parenthesized interface", "{{(.).foo}}", "<no value>", default, true),

    // Issue 31810: Parenthesized first element of pipeline with arguments.
    // See also TestIssue31810.
    new("unparenthesized non-function", "{{1 2}}", "", default, false),
    new("parenthesized non-function", "{{(1) 2}}", "", default, false),
    new("parenthesized non-function with no args", "{{(1)}}", "1", default, true), // This is fine.

    // Method calls.
    new(".Method0", "-{{.Method0}}-", "-M0-", tVal.OrTypedNil(), true),
    new(".Method1(1234)", "-{{.Method1 1234}}-", "-1234-", tVal.OrTypedNil(), true),
    new(".Method1(.I)", "-{{.Method1 .I}}-", "-17-", tVal.OrTypedNil(), true),
    new(".Method2(3, .X)", "-{{.Method2 3 .X}}-", "-Method2: 3 x-", tVal.OrTypedNil(), true),
    new(".Method2(.U16, `str`)", "-{{.Method2 .U16 `str`}}-", "-Method2: 16 str-", tVal.OrTypedNil(), true),
    new(".Method2(.U16, $x)", "{{if $x := .X}}-{{.Method2 .U16 $x}}{{end}}-", "-Method2: 16 x-", tVal.OrTypedNil(), true),
    new(".Method3(nil constant)", "-{{.Method3 nil}}-", "-Method3: <nil>-", tVal.OrTypedNil(), true),
    new(".Method3(nil value)", "-{{.Method3 .MXI.unset}}-", "-Method3: <nil>-", tVal.OrTypedNil(), true),
    new("method on var", "{{if $x := .}}-{{$x.Method2 .U16 $x.X}}{{end}}-", "-Method2: 16 x-", tVal.OrTypedNil(), true),
    new("method on chained var",
        "{{range .MSIone}}{{if $.U.TrueFalse $.True}}{{$.U.TrueFalse $.True}}{{else}}WRONG{{end}}{{end}}",
        "true", tVal.OrTypedNil(), true),
    new("chained method",
        "{{range .MSIone}}{{if $.GetU.TrueFalse $.True}}{{$.U.TrueFalse $.True}}{{else}}WRONG{{end}}{{end}}",
        "true", tVal.OrTypedNil(), true),
    new("chained method on variable",
        "{{with $x := .}}{{with .SI}}{{$.GetU.TrueFalse $.True}}{{end}}{{end}}",
        "true", tVal.OrTypedNil(), true),
    new(".NilOKFunc not nil", "{{call .NilOKFunc .PI}}", "false", tVal.OrTypedNil(), true),
    new(".NilOKFunc nil", "{{call .NilOKFunc nil}}", "true", tVal.OrTypedNil(), true),
    new("method on nil value from slice", "-{{range .}}{{.Method1 1234}}{{end}}-", "-1234-", tSliceOfNil, true),
    new("method on typed nil interface value", "{{.NonEmptyInterfaceTypedNil.Method0}}", "M0", tVal.OrTypedNil(), true),

    // Function call builtin.
    new(".BinaryFunc", "{{call .BinaryFunc `1` `2`}}", "[1=2]", tVal.OrTypedNil(), true),
    new(".VariadicFunc0", "{{call .VariadicFunc}}", "<>", tVal.OrTypedNil(), true),
    new(".VariadicFunc2", "{{call .VariadicFunc `he` `llo`}}", "<he+llo>", tVal.OrTypedNil(), true),
    new(".VariadicFuncInt", "{{call .VariadicFuncInt 33 `he` `llo`}}", "33=<he+llo>", tVal.OrTypedNil(), true),
    new("if .BinaryFunc call", "{{ if .BinaryFunc}}{{call .BinaryFunc `1` `2`}}{{end}}", "[1=2]", tVal.OrTypedNil(), true),
    new("if not .BinaryFunc call", "{{ if not .BinaryFunc}}{{call .BinaryFunc `1` `2`}}{{else}}No{{end}}", "No", tVal.OrTypedNil(), true),
    new("Interface Call", @"{{stringer .S}}", "foozle", new map<@string, any>{["S"] = bytes.NewBufferString("foozle").OrTypedNil()}, true),
    new(".ErrFunc", "{{call .ErrFunc}}", "bla", tVal.OrTypedNil(), true),
    new("call nil", "{{call nil}}", "", tVal.OrTypedNil(), false),
    new("empty call", "{{call}}", "", tVal.OrTypedNil(), false),
    new("empty call after pipe valid", "{{.ErrFunc | call}}", "bla", tVal.OrTypedNil(), true),
    new("empty call after pipe invalid", "{{1 | call}}", "", tVal.OrTypedNil(), false),

    // Erroneous function calls (check args).
    new(".BinaryFuncTooFew", "{{call .BinaryFunc `1`}}", "", tVal.OrTypedNil(), false),
    new(".BinaryFuncTooMany", "{{call .BinaryFunc `1` `2` `3`}}", "", tVal.OrTypedNil(), false),
    new(".BinaryFuncBad0", "{{call .BinaryFunc 1 3}}", "", tVal.OrTypedNil(), false),
    new(".BinaryFuncBad1", "{{call .BinaryFunc `1` 3}}", "", tVal.OrTypedNil(), false),
    new(".VariadicFuncBad0", "{{call .VariadicFunc 3}}", "", tVal.OrTypedNil(), false),
    new(".VariadicFuncIntBad0", "{{call .VariadicFuncInt}}", "", tVal.OrTypedNil(), false),
    new(".VariadicFuncIntBad`", "{{call .VariadicFuncInt `x`}}", "", tVal.OrTypedNil(), false),
    new(".VariadicFuncNilBad", "{{call .VariadicFunc nil}}", "", tVal.OrTypedNil(), false),

    // Pipelines.
    new("pipeline", "-{{.Method0 | .Method2 .U16}}-", "-Method2: 16 M0-", tVal.OrTypedNil(), true),
    new("pipeline func", "-{{call .VariadicFunc `llo` | call .VariadicFunc `he` }}-", "-<he+<llo>>-", tVal.OrTypedNil(), true),

    // Nil values aren't missing arguments.
    new("nil pipeline", "{{ .Empty0 | call .NilOKFunc }}", "true", tVal.OrTypedNil(), true),
    new("nil call arg", "{{ call .NilOKFunc .Empty0 }}", "true", tVal.OrTypedNil(), true),
    new("bad nil pipeline", "{{ .Empty0 | .VariadicFunc }}", "", tVal.OrTypedNil(), false),

    // Parenthesized expressions
    new("parens in pipeline", "{{printf `%d %d %d` (1) (2 | add 3) (add 4 (add 5 6))}}", "1 5 15", tVal.OrTypedNil(), true),

    // Parenthesized expressions with field accesses
    new("parens: $ in paren", "{{($).X}}", "x", tVal.OrTypedNil(), true),
    new("parens: $.GetU in paren", "{{($.GetU).V}}", "v", tVal.OrTypedNil(), true),
    new("parens: $ in paren in pipe", "{{($ | echo).X}}", "x", tVal.OrTypedNil(), true),
    new("parens: spaces and args", @"{{(makemap ""up"" ""down"" ""left"" ""right"").left}}", "right", tVal.OrTypedNil(), true),

    // If.
    new("if true", "{{if true}}TRUE{{end}}", "TRUE", tVal.OrTypedNil(), true),
    new("if false", "{{if false}}TRUE{{else}}FALSE{{end}}", "FALSE", tVal.OrTypedNil(), true),
    new("if nil", "{{if nil}}TRUE{{end}}", "", tVal.OrTypedNil(), false),
    new("if on typed nil interface value", "{{if .NonEmptyInterfaceTypedNil}}TRUE{{ end }}", "", tVal.OrTypedNil(), true),
    new("if 1", "{{if 1}}NON-ZERO{{else}}ZERO{{end}}", "NON-ZERO", tVal.OrTypedNil(), true),
    new("if 0", "{{if 0}}NON-ZERO{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if 1.5", "{{if 1.5}}NON-ZERO{{else}}ZERO{{end}}", "NON-ZERO", tVal.OrTypedNil(), true),
    new("if 0.0", "{{if .FloatZero}}NON-ZERO{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if 1.5i", "{{if 1.5i}}NON-ZERO{{else}}ZERO{{end}}", "NON-ZERO", tVal.OrTypedNil(), true),
    new("if 0.0i", "{{if .ComplexZero}}NON-ZERO{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if nonNilPointer", "{{if .PI}}NON-ZERO{{else}}ZERO{{end}}", "NON-ZERO", tVal.OrTypedNil(), true),
    new("if nilPointer", "{{if .NIL}}NON-ZERO{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if UPI", "{{if .UPI}}NON-ZERO{{else}}ZERO{{end}}", "NON-ZERO", tVal.OrTypedNil(), true),
    new("if EmptyUPI", "{{if .EmptyUPI}}NON-ZERO{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if emptystring", "{{if ``}}NON-EMPTY{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("if string", "{{if `notempty`}}NON-EMPTY{{else}}EMPTY{{end}}", "NON-EMPTY", tVal.OrTypedNil(), true),
    new("if emptyslice", "{{if .SIEmpty}}NON-EMPTY{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("if slice", "{{if .SI}}NON-EMPTY{{else}}EMPTY{{end}}", "NON-EMPTY", tVal.OrTypedNil(), true),
    new("if emptymap", "{{if .MSIEmpty}}NON-EMPTY{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("if map", "{{if .MSI}}NON-EMPTY{{else}}EMPTY{{end}}", "NON-EMPTY", tVal.OrTypedNil(), true),
    new("if map unset", "{{if .MXI.none}}NON-ZERO{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if map not unset", "{{if not .MXI.none}}ZERO{{else}}NON-ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("if $x with $y int", "{{if $x := true}}{{with $y := .I}}{{$x}},{{$y}}{{end}}{{end}}", "true,17", tVal.OrTypedNil(), true),
    new("if $x with $x int", "{{if $x := true}}{{with $x := .I}}{{$x}},{{end}}{{$x}}{{end}}", "17,true", tVal.OrTypedNil(), true),
    new("if else if", "{{if false}}FALSE{{else if true}}TRUE{{end}}", "TRUE", tVal.OrTypedNil(), true),
    new("if else chain", "{{if eq 1 3}}1{{else if eq 2 3}}2{{else if eq 3 3}}3{{end}}", "3", tVal.OrTypedNil(), true),

    // Print etc.
    new("print", @"{{print ""hello, print""}}", "hello, print", tVal.OrTypedNil(), true),
    new("print 123", @"{{print 1 2 3}}", "1 2 3", tVal.OrTypedNil(), true),
    new("print nil", @"{{print nil}}", "<nil>", tVal.OrTypedNil(), true),
    new("println", @"{{println 1 2 3}}", "1 2 3\n", tVal.OrTypedNil(), true),
    new("printf int", @"{{printf ""%04x"" 127}}", "007f", tVal.OrTypedNil(), true),
    new("printf float", @"{{printf ""%g"" 3.5}}", "3.5", tVal.OrTypedNil(), true),
    new("printf complex", @"{{printf ""%g"" 1+7i}}", "(1+7i)", tVal.OrTypedNil(), true),
    new("printf string", @"{{printf ""%s"" ""hello""}}", "hello", tVal.OrTypedNil(), true),
    new("printf function", @"{{printf ""%#q"" zeroArgs}}", "`zeroArgs`", tVal.OrTypedNil(), true),
    new("printf field", @"{{printf ""%s"" .U.V}}", "v", tVal.OrTypedNil(), true),
    new("printf method", @"{{printf ""%s"" .Method0}}", "M0", tVal.OrTypedNil(), true),
    new("printf dot", @"{{with .I}}{{printf ""%d"" .}}{{end}}", "17", tVal.OrTypedNil(), true),
    new("printf var", @"{{with $x := .I}}{{printf ""%d"" $x}}{{end}}", "17", tVal.OrTypedNil(), true),
    new("printf lots", @"{{printf ""%d %s %g %s"" 127 ""hello"" 7-3i .Method0}}", "127 hello (7-3i) M0", tVal.OrTypedNil(), true),

    // HTML.
    new("html", @"{{html ""<script>alert(\""XSS\"");</script>""}}",
        "&lt;script&gt;alert(&#34;XSS&#34;);&lt;/script&gt;", default, true),
    new("html pipeline", @"{{printf ""<script>alert(\""XSS\"");</script>"" | html}}",
        "&lt;script&gt;alert(&#34;XSS&#34;);&lt;/script&gt;", default, true),
    new("html", @"{{html .PS}}", "a string", tVal.OrTypedNil(), true),
    new("html typed nil", @"{{html .NIL}}", "&lt;nil&gt;", tVal.OrTypedNil(), true),
    new("html untyped nil", @"{{html .Empty0}}", "&lt;no value&gt;", tVal.OrTypedNil(), true),

    // JavaScript.
    new("js", @"{{js .}}", @"It\'d be nice.", (@string)@"It'd be nice.", true),

    // URL query.
    new("urlquery", @"{{""http://www.example.org/""|urlquery}}", "http%3A%2F%2Fwww.example.org%2F", default, true),

    // Booleans
    new("not", "{{not true}} {{not false}}", "false true", default, true),
    new("and", "{{and false 0}} {{and 1 0}} {{and 0 true}} {{and 1 1}}", "false 0 0 1", default, true),
    new("or", "{{or 0 0}} {{or 1 0}} {{or 0 true}} {{or 1 1}}", "0 1 true 1", default, true),
    new("or short-circuit", "{{or 0 1 (die)}}", "1", default, true),
    new("and short-circuit", "{{and 1 0 (die)}}", "0", default, true),
    new("or short-circuit2", "{{or 0 0 (die)}}", "", default, false),
    new("and short-circuit2", "{{and 1 1 (die)}}", "", default, false),
    new("and pipe-true", "{{1 | and 1}}", "1", default, true),
    new("and pipe-false", "{{0 | and 1}}", "0", default, true),
    new("or pipe-true", "{{1 | or 0}}", "1", default, true),
    new("or pipe-false", "{{0 | or 0}}", "0", default, true),
    new("and undef", "{{and 1 .Unknown}}", "<no value>", default, true),
    new("or undef", "{{or 0 .Unknown}}", "<no value>", default, true),
    new("boolean if", "{{if and true 1 `hi`}}TRUE{{else}}FALSE{{end}}", "TRUE", tVal.OrTypedNil(), true),
    new("boolean if not", "{{if and true 1 `hi` | not}}TRUE{{else}}FALSE{{end}}", "FALSE", default, true),
    new("boolean if pipe", "{{if true | not | and 1}}TRUE{{else}}FALSE{{end}}", "FALSE", default, true),

    // Indexing.
    new("slice[0]", "{{index .SI 0}}", "3", tVal.OrTypedNil(), true),
    new("slice[1]", "{{index .SI 1}}", "4", tVal.OrTypedNil(), true),
    new("slice[HUGE]", "{{index .SI 10}}", "", tVal.OrTypedNil(), false),
    new("slice[WRONG]", "{{index .SI `hello`}}", "", tVal.OrTypedNil(), false),
    new("slice[nil]", "{{index .SI nil}}", "", tVal.OrTypedNil(), false),
    new("map[one]", "{{index .MSI `one`}}", "1", tVal.OrTypedNil(), true),
    new("map[two]", "{{index .MSI `two`}}", "2", tVal.OrTypedNil(), true),
    new("map[NO]", "{{index .MSI `XXX`}}", "0", tVal.OrTypedNil(), true),
    new("map[nil]", "{{index .MSI nil}}", "", tVal.OrTypedNil(), false),
    new("map[``]", "{{index .MSI ``}}", "0", tVal.OrTypedNil(), true),
    new("map[WRONG]", "{{index .MSI 10}}", "", tVal.OrTypedNil(), false),
    new("double index", "{{index .SMSI 1 `eleven`}}", "11", tVal.OrTypedNil(), true),
    new("nil[1]", "{{index nil 1}}", "", tVal.OrTypedNil(), false),
    new("map MI64S", "{{index .MI64S 2}}", "i642", tVal.OrTypedNil(), true),
    new("map MI32S", "{{index .MI32S 2}}", "two", tVal.OrTypedNil(), true),
    new("map MUI64S", "{{index .MUI64S 3}}", "ui643", tVal.OrTypedNil(), true),
    new("map MI8S", "{{index .MI8S 3}}", "i83", tVal.OrTypedNil(), true),
    new("map MUI8S", "{{index .MUI8S 2}}", "u82", tVal.OrTypedNil(), true),
    new("index of an interface field", "{{index .Empty3 0}}", "7", tVal.OrTypedNil(), true),

    // Slicing.
    new("slice[:]", "{{slice .SI}}", "[3 4 5]", tVal.OrTypedNil(), true),
    new("slice[1:]", "{{slice .SI 1}}", "[4 5]", tVal.OrTypedNil(), true),
    new("slice[1:2]", "{{slice .SI 1 2}}", "[4]", tVal.OrTypedNil(), true),
    new("slice[-1:]", "{{slice .SI -1}}", "", tVal.OrTypedNil(), false),
    new("slice[1:-2]", "{{slice .SI 1 -2}}", "", tVal.OrTypedNil(), false),
    new("slice[1:2:-1]", "{{slice .SI 1 2 -1}}", "", tVal.OrTypedNil(), false),
    new("slice[2:1]", "{{slice .SI 2 1}}", "", tVal.OrTypedNil(), false),
    new("slice[2:2:1]", "{{slice .SI 2 2 1}}", "", tVal.OrTypedNil(), false),
    new("out of range", "{{slice .SI 4 5}}", "", tVal.OrTypedNil(), false),
    new("out of range", "{{slice .SI 2 2 5}}", "", tVal.OrTypedNil(), false),
    new("len(s) < indexes < cap(s)", "{{slice .SICap 6 10}}", "[0 0 0 0]", tVal.OrTypedNil(), true),
    new("len(s) < indexes < cap(s)", "{{slice .SICap 6 10 10}}", "[0 0 0 0]", tVal.OrTypedNil(), true),
    new("indexes > cap(s)", "{{slice .SICap 10 11}}", "", tVal.OrTypedNil(), false),
    new("indexes > cap(s)", "{{slice .SICap 6 10 11}}", "", tVal.OrTypedNil(), false),
    new("array[:]", "{{slice .AI}}", "[3 4 5]", tVal.OrTypedNil(), true),
    new("array[1:]", "{{slice .AI 1}}", "[4 5]", tVal.OrTypedNil(), true),
    new("array[1:2]", "{{slice .AI 1 2}}", "[4]", tVal.OrTypedNil(), true),
    new("pointer to array[:]", "{{slice .PAI}}", "[3 4 5]", tVal.OrTypedNil(), true),
    new("pointer to array[1:]", "{{slice .PAI 1}}", "[4 5]", tVal.OrTypedNil(), true),
    new("pointer to array[1:2]", "{{slice .PAI 1 2}}", "[4]", tVal.OrTypedNil(), true),
    new("string[:]", "{{slice .S}}", "xyz", tVal.OrTypedNil(), true),
    new("string[0:1]", "{{slice .S 0 1}}", "x", tVal.OrTypedNil(), true),
    new("string[1:]", "{{slice .S 1}}", "yz", tVal.OrTypedNil(), true),
    new("string[1:2]", "{{slice .S 1 2}}", "y", tVal.OrTypedNil(), true),
    new("out of range", "{{slice .S 1 5}}", "", tVal.OrTypedNil(), false),
    new("3-index slice of string", "{{slice .S 1 2 2}}", "", tVal.OrTypedNil(), false),
    new("slice of an interface field", "{{slice .Empty3 0 1}}", "[7]", tVal.OrTypedNil(), true),

    // Len.
    new("slice", "{{len .SI}}", "3", tVal.OrTypedNil(), true),
    new("map", "{{len .MSI }}", "3", tVal.OrTypedNil(), true),
    new("len of int", "{{len 3}}", "", tVal.OrTypedNil(), false),
    new("len of nothing", "{{len .Empty0}}", "", tVal.OrTypedNil(), false),
    new("len of an interface field", "{{len .Empty3}}", "2", tVal.OrTypedNil(), true),

    // With.
    new("with true", "{{with true}}{{.}}{{end}}", "true", tVal.OrTypedNil(), true),
    new("with false", "{{with false}}{{.}}{{else}}FALSE{{end}}", "FALSE", tVal.OrTypedNil(), true),
    new("with 1", "{{with 1}}{{.}}{{else}}ZERO{{end}}", "1", tVal.OrTypedNil(), true),
    new("with 0", "{{with 0}}{{.}}{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("with 1.5", "{{with 1.5}}{{.}}{{else}}ZERO{{end}}", "1.5", tVal.OrTypedNil(), true),
    new("with 0.0", "{{with .FloatZero}}{{.}}{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("with 1.5i", "{{with 1.5i}}{{.}}{{else}}ZERO{{end}}", "(0+1.5i)", tVal.OrTypedNil(), true),
    new("with 0.0i", "{{with .ComplexZero}}{{.}}{{else}}ZERO{{end}}", "ZERO", tVal.OrTypedNil(), true),
    new("with emptystring", "{{with ``}}{{.}}{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("with string", "{{with `notempty`}}{{.}}{{else}}EMPTY{{end}}", "notempty", tVal.OrTypedNil(), true),
    new("with emptyslice", "{{with .SIEmpty}}{{.}}{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("with slice", "{{with .SI}}{{.}}{{else}}EMPTY{{end}}", "[3 4 5]", tVal.OrTypedNil(), true),
    new("with emptymap", "{{with .MSIEmpty}}{{.}}{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("with map", "{{with .MSIone}}{{.}}{{else}}EMPTY{{end}}", "map[one:1]", tVal.OrTypedNil(), true),
    new("with empty interface, struct field", "{{with .Empty4}}{{.V}}{{end}}", "UinEmpty", tVal.OrTypedNil(), true),
    new("with $x int", "{{with $x := .I}}{{$x}}{{end}}", "17", tVal.OrTypedNil(), true),
    new("with $x struct.U.V", "{{with $x := $}}{{$x.U.V}}{{end}}", "v", tVal.OrTypedNil(), true),
    new("with variable and action", "{{with $x := $}}{{$y := $.U.V}}{{$y}}{{end}}", "v", tVal.OrTypedNil(), true),
    new("with on typed nil interface value", "{{with .NonEmptyInterfaceTypedNil}}TRUE{{ end }}", "", tVal.OrTypedNil(), true),
    new("with else with", "{{with 0}}{{.}}{{else with true}}{{.}}{{end}}", "true", tVal.OrTypedNil(), true),
    new("with else with chain", "{{with 0}}{{.}}{{else with false}}{{.}}{{else with `notempty`}}{{.}}{{end}}", "notempty", tVal.OrTypedNil(), true),

    // Range.
    new("range []int", "{{range .SI}}-{{.}}-{{end}}", "-3--4--5-", tVal.OrTypedNil(), true),
    new("range empty no else", "{{range .SIEmpty}}-{{.}}-{{end}}", "", tVal.OrTypedNil(), true),
    new("range []int else", "{{range .SI}}-{{.}}-{{else}}EMPTY{{end}}", "-3--4--5-", tVal.OrTypedNil(), true),
    new("range empty else", "{{range .SIEmpty}}-{{.}}-{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("range []int break else", "{{range .SI}}-{{.}}-{{break}}NOTREACHED{{else}}EMPTY{{end}}", "-3-", tVal.OrTypedNil(), true),
    new("range []int continue else", "{{range .SI}}-{{.}}-{{continue}}NOTREACHED{{else}}EMPTY{{end}}", "-3--4--5-", tVal.OrTypedNil(), true),
    new("range []bool", "{{range .SB}}-{{.}}-{{end}}", "-true--false-", tVal.OrTypedNil(), true),
    new("range []int method", "{{range .SI | .MAdd .I}}-{{.}}-{{end}}", "-20--21--22-", tVal.OrTypedNil(), true),
    new("range map", "{{range .MSI}}-{{.}}-{{end}}", "-1--3--2-", tVal.OrTypedNil(), true),
    new("range empty map no else", "{{range .MSIEmpty}}-{{.}}-{{end}}", "", tVal.OrTypedNil(), true),
    new("range map else", "{{range .MSI}}-{{.}}-{{else}}EMPTY{{end}}", "-1--3--2-", tVal.OrTypedNil(), true),
    new("range empty map else", "{{range .MSIEmpty}}-{{.}}-{{else}}EMPTY{{end}}", "EMPTY", tVal.OrTypedNil(), true),
    new("range empty interface", "{{range .Empty3}}-{{.}}-{{else}}EMPTY{{end}}", "-7--8-", tVal.OrTypedNil(), true),
    new("range empty nil", "{{range .Empty0}}-{{.}}-{{end}}", "", tVal.OrTypedNil(), true),
    new("range $x SI", "{{range $x := .SI}}<{{$x}}>{{end}}", "<3><4><5>", tVal.OrTypedNil(), true),
    new("range $x $y SI", "{{range $x, $y := .SI}}<{{$x}}={{$y}}>{{end}}", "<0=3><1=4><2=5>", tVal.OrTypedNil(), true),
    new("range $x MSIone", "{{range $x := .MSIone}}<{{$x}}>{{end}}", "<1>", tVal.OrTypedNil(), true),
    new("range $x $y MSIone", "{{range $x, $y := .MSIone}}<{{$x}}={{$y}}>{{end}}", "<one=1>", tVal.OrTypedNil(), true),
    new("range $x PSI", "{{range $x := .PSI}}<{{$x}}>{{end}}", "<21><22><23>", tVal.OrTypedNil(), true),
    new("declare in range", "{{range $x := .PSI}}<{{$foo:=$x}}{{$x}}>{{end}}", "<21><22><23>", tVal.OrTypedNil(), true),
    new("range count", @"{{range $i, $x := count 5}}[{{$i}}]{{$x}}{{end}}", "[0]a[1]b[2]c[3]d[4]e", tVal.OrTypedNil(), true),
    new("range nil count", @"{{range $i, $x := count 0}}{{else}}empty{{end}}", "empty", tVal.OrTypedNil(), true),
    new("range iter.Seq[int]", @"{{range $i := .}}{{$i}}{{end}}", "01", (fVal1(2)).OrTypedNilFunc(), true),
    new("i = range iter.Seq[int]", @"{{$i := 0}}{{range $i = .}}{{$i}}{{end}}", "01", (fVal1(2)).OrTypedNilFunc(), true),
    new("range iter.Seq[int] over two var", @"{{range $i, $c := .}}{{$c}}{{end}}", "", (fVal1(2)).OrTypedNilFunc(), false),
    new("i, c := range iter.Seq2[int,int]", @"{{range $i, $c := .}}{{$i}}{{$c}}{{end}}", "0112", (fVal2(2)).OrTypedNilFunc(), true),
    new("i, c = range iter.Seq2[int,int]", @"{{$i := 0}}{{$c := 0}}{{range $i, $c = .}}{{$i}}{{$c}}{{end}}", "0112", (fVal2(2)).OrTypedNilFunc(), true),
    new("i = range iter.Seq2[int,int]", @"{{$i := 0}}{{range $i = .}}{{$i}}{{end}}", "01", (fVal2(2)).OrTypedNilFunc(), true),
    new("i := range iter.Seq2[int,int]", @"{{range $i := .}}{{$i}}{{end}}", "01", (fVal2(2)).OrTypedNilFunc(), true),
    new("i,c,x range iter.Seq2[int,int]", @"{{$i := 0}}{{$c := 0}}{{$x := 0}}{{range $i, $c = .}}{{$i}}{{$c}}{{end}}", "0112", (fVal2(2)).OrTypedNilFunc(), true),
    new("i,x range iter.Seq[int]", @"{{$i := 0}}{{$x := 0}}{{range $i = .}}{{$i}}{{end}}", "01", (fVal1(2)).OrTypedNilFunc(), true),
    new("range iter.Seq[int] else", @"{{range $i := .}}{{$i}}{{else}}empty{{end}}", "empty", (fVal1(0)).OrTypedNilFunc(), true),
    new("range iter.Seq2[int,int] else", @"{{range $i := .}}{{$i}}{{else}}empty{{end}}", "empty", (fVal2(0)).OrTypedNilFunc(), true),
    new("range int8", rangeTestInt, rangeTestData<int8>(), (int8)5, true),
    new("range int16", rangeTestInt, rangeTestData<int16>(), (int16)5, true),
    new("range int32", rangeTestInt, rangeTestData<int32>(), (int32)5, true),
    new("range int64", rangeTestInt, rangeTestData<int64>(), (int64)5, true),
    new("range int", rangeTestInt, rangeTestData<nint>(), (nint)5, true),
    new("range uint8", rangeTestInt, rangeTestData<uint8>(), (uint8)5, true),
    new("range uint16", rangeTestInt, rangeTestData<uint16>(), (uint16)5, true),
    new("range uint32", rangeTestInt, rangeTestData<uint32>(), (uint32)5, true),
    new("range uint64", rangeTestInt, rangeTestData<uint64>(), (uint64)5, true),
    new("range uint", rangeTestInt, rangeTestData<nuint>(), (nuint)5, true),
    new("range uintptr", rangeTestInt, rangeTestData<uintptr>(), (uintptr)5, true),
    new("range uintptr(0)", @"{{range $v := .}}{{print $v}}{{else}}empty{{end}}", "empty", (uintptr)0, true),
    new("range 5", @"{{range $v := 5}}{{printf ""%T%d"" $v $v}}{{end}}", rangeTestData<nint>(), default, true),

    // Cute examples.
    new("or as if true", @"{{or .SI ""slice is empty""}}", "[3 4 5]", tVal.OrTypedNil(), true),
    new("or as if false", @"{{or .SIEmpty ""slice is empty""}}", "slice is empty", tVal.OrTypedNil(), true),

    // Error handling.
    new("error method, error", "{{.MyError true}}", "", tVal.OrTypedNil(), false),
    new("error method, no error", "{{.MyError false}}", "false", tVal.OrTypedNil(), true),

    // Numbers
    new("decimal", "{{print 1234}}", "1234", tVal.OrTypedNil(), true),
    new("decimal _", "{{print 12_34}}", "1234", tVal.OrTypedNil(), true),
    new("binary", "{{print 0b101}}", "5", tVal.OrTypedNil(), true),
    new("binary _", "{{print 0b_1_0_1}}", "5", tVal.OrTypedNil(), true),
    new("BINARY", "{{print 0B101}}", "5", tVal.OrTypedNil(), true),
    new("octal0", "{{print 0377}}", "255", tVal.OrTypedNil(), true),
    new("octal", "{{print 0o377}}", "255", tVal.OrTypedNil(), true),
    new("octal _", "{{print 0o_3_7_7}}", "255", tVal.OrTypedNil(), true),
    new("OCTAL", "{{print 0O377}}", "255", tVal.OrTypedNil(), true),
    new("hex", "{{print 0x123}}", "291", tVal.OrTypedNil(), true),
    new("hex _", "{{print 0x1_23}}", "291", tVal.OrTypedNil(), true),
    new("HEX", "{{print 0X123ABC}}", "1194684", tVal.OrTypedNil(), true),
    new("float", "{{print 123.4}}", "123.4", tVal.OrTypedNil(), true),
    new("float _", "{{print 0_0_1_2_3.4}}", "123.4", tVal.OrTypedNil(), true),
    new("hex float", "{{print +0x1.ep+2}}", "7.5", tVal.OrTypedNil(), true),
    new("hex float _", "{{print +0x_1.e_0p+0_2}}", "7.5", tVal.OrTypedNil(), true),
    new("HEX float", "{{print +0X1.EP+2}}", "7.5", tVal.OrTypedNil(), true),
    new("print multi", "{{print 1_2_3_4 7.5_00_00_00}}", "1234 7.5", tVal.OrTypedNil(), true),
    new("print multi2", "{{print 1234 0x0_1.e_0p+02}}", "1234 7.5", tVal.OrTypedNil(), true),

    // Fixed bugs.
    // Must separate dot and receiver; otherwise args are evaluated with dot set to variable.
    new("bug0", "{{range .MSIone}}{{if $.Method1 .}}X{{end}}{{end}}", "X", tVal.OrTypedNil(), true),
    // Do not loop endlessly in indirect for non-empty interfaces.
    // The bug appears with *interface only; looped forever.
    new("bug1", "{{.Method0}}", "M0", ᏑiVal, true),
    // Was taking address of interface field, so method set was empty.
    new("bug2", "{{$.NonEmptyInterface.Method0}}", "M0", tVal.OrTypedNil(), true),
    // Struct values were not legal in with - mere oversight.
    new("bug3", "{{with $}}{{.Method0}}{{end}}", "M0", tVal.OrTypedNil(), true),
    // Nil interface values in if.
    new("bug4", "{{if .Empty0}}non-nil{{else}}nil{{end}}", "nil", tVal.OrTypedNil(), true),
    // Stringer.
    new("bug5", "{{.Str}}", "foozle", tVal.OrTypedNil(), true),
    new("bug5a", "{{.Err}}", "erroozle", tVal.OrTypedNil(), true),
    // Args need to be indirected and dereferenced sometimes.
    new("bug6a", "{{vfunc .V0 .V1}}", "vfunc", tVal.OrTypedNil(), true),
    new("bug6b", "{{vfunc .V0 .V0}}", "vfunc", tVal.OrTypedNil(), true),
    new("bug6c", "{{vfunc .V1 .V0}}", "vfunc", tVal.OrTypedNil(), true),
    new("bug6d", "{{vfunc .V1 .V1}}", "vfunc", tVal.OrTypedNil(), true),
    // Legal parse but illegal execution: non-function should have no arguments.
    new("bug7a", "{{3 2}}", "", tVal.OrTypedNil(), false),
    new("bug7b", "{{$x := 1}}{{$x 2}}", "", tVal.OrTypedNil(), false),
    new("bug7c", "{{$x := 1}}{{3 | $x}}", "", tVal.OrTypedNil(), false),
    // Pipelined arg was not being type-checked.
    new("bug8a", "{{3|oneArg}}", "", tVal.OrTypedNil(), false),
    new("bug8b", "{{4|dddArg 3}}", "", tVal.OrTypedNil(), false),
    // A bug was introduced that broke map lookups for lower-case names.
    new("bug9", "{{.cause}}", "neglect", new map<@string, @string>{["cause"] = "neglect"}, true),
    // Field chain starting with function did not work.
    new("bug10", "{{mapOfThree.three}}-{{(mapOfThree).three}}", "3-3", (nint)0, true),
    // Dereferencing nil pointer while evaluating function arguments should not panic. Issue 7333.
    new("bug11", "{{valueString .PS}}", "", new T(), false),
    // 0xef gave constant type float64. Issue 8622.
    new("bug12xe", "{{printf `%T` 0xef}}", "int", new T(), true),
    new("bug12xE", "{{printf `%T` 0xEE}}", "int", new T(), true),
    new("bug12Xe", "{{printf `%T` 0Xef}}", "int", new T(), true),
    new("bug12XE", "{{printf `%T` 0XEE}}", "int", new T(), true),
    // Chained nodes did not work as arguments. Issue 8473.
    new("bug13", "{{print (.Copy).I}}", "17", tVal.OrTypedNil(), true),
    // Didn't protect against nil or literal values in field chains.
    new("bug14a", "{{(nil).True}}", "", tVal.OrTypedNil(), false),
    new("bug14b", "{{$x := nil}}{{$x.anything}}", "", tVal.OrTypedNil(), false),
    new("bug14c", @"{{$x := (1.0)}}{{$y := (""hello"")}}{{$x.anything}}{{$y.true}}", "", tVal.OrTypedNil(), false),
    // Didn't call validateType on function results. Issue 10800.
    new("bug15", "{{valueString returnInt}}", "", tVal.OrTypedNil(), false),
    // Variadic function corner cases. Issue 10946.
    new("bug16a", "{{true|printf}}", "", tVal.OrTypedNil(), false),
    new("bug16b", "{{1|printf}}", "", tVal.OrTypedNil(), false),
    new("bug16c", "{{1.1|printf}}", "", tVal.OrTypedNil(), false),
    new("bug16d", "{{'x'|printf}}", "", tVal.OrTypedNil(), false),
    new("bug16e", "{{0i|printf}}", "", tVal.OrTypedNil(), false),
    new("bug16f", "{{true|twoArgs \"xxx\"}}", "", tVal.OrTypedNil(), false),
    new("bug16g", "{{\"aaa\" |twoArgs \"bbb\"}}", "twoArgs=bbbaaa", tVal.OrTypedNil(), true),
    new("bug16h", "{{1|oneArg}}", "", tVal.OrTypedNil(), false),
    new("bug16i", "{{\"aaa\"|oneArg}}", "oneArg=aaa", tVal.OrTypedNil(), true),
    new("bug16j", "{{1+2i|printf \"%v\"}}", "(1+2i)", tVal.OrTypedNil(), true),
    new("bug16k", "{{\"aaa\"|printf }}", "aaa", tVal.OrTypedNil(), true),
    new("bug17a", "{{.NonEmptyInterface.X}}", "x", tVal.OrTypedNil(), true),
    new("bug17b", "-{{.NonEmptyInterface.Method1 1234}}-", "-1234-", tVal.OrTypedNil(), true),
    new("bug17c", "{{len .NonEmptyInterfacePtS}}", "2", tVal.OrTypedNil(), true),
    new("bug17d", "{{index .NonEmptyInterfacePtS 0}}", "a", tVal.OrTypedNil(), true),
    new("bug17e", "{{range .NonEmptyInterfacePtS}}-{{.}}-{{end}}", "-a--b-", tVal.OrTypedNil(), true),

    // More variadic function corner cases. Some runes would get evaluated
    // as constant floats instead of ints. Issue 34483.
    new("bug18a", "{{eq . '.'}}", "true", (rune)'.', true),
    new("bug18b", "{{eq . 'e'}}", "true", (rune)'e', true),
    new("bug18c", "{{eq . 'P'}}", "true", (rune)'P', true),

    new("issue56490", "{{$i := 0}}{{$x := 0}}{{range $i = .AI}}{{end}}{{$i}}", "5", tVal.OrTypedNil(), true),
    new("issue60801", "{{$k := 0}}{{$v := 0}}{{range $k, $v = .AI}}{{$k}}={{$v}} {{end}}", "0=3 1=4 2=5 ", tVal.OrTypedNil(), true)
]); }

internal static iter.Seq<nint> fVal1(nint i) {
    return (Func<nint, bool> yield) => {
        foreach (var v in range(i)) {
            if (!yield(v)) {
                break;
            }
        }
    };
}

internal static iter.Seq2<nint, nint> fVal2(nint i) {
    return (Func<nint, nint, bool> yield) => {
        foreach (var v in range(i)) {
            if (!yield(v, v + 1)) {
                break;
            }
        }
    };
}

internal static ref readonly @string rangeTestInt => ref rangeTestIntᵇ;
private static readonly @string rangeTestIntᵇ = @"{{range $v := .}}{{printf ""%T%d"" $v $v}}{{end}}"u8;

internal static @string rangeTestData<T>()
    where T : /* int | int8 | int16 | int32 | int64 | uint | uint8 | uint16 | uint32 | uint64 | uintptr */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var I = ConvertToType<T>(5);
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    for (var i = ConvertToType<T>(0); i < I; i++) {
        fmt.Fprintf(new template_test_package.strings_BuilderжWriter(Ꮡbuf), "%T%d"u8, i, i);
    }
    return buf.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string zeroArgsˢ => ref zeroArgsˢᵇ;
private static readonly @string zeroArgsˢᵇ = "zeroArgs"u8;

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
internal static partial channel<@string> count(nint n) {
    if (n == 0) {
        return default;
    }
    var c = new channel<@string>(0);
    var cʗ1 = c;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            cʗ1.ᐸꟷ("abcdefghijklmnop"u8.slice(i, i + 1));
        }
        close(cʗ1);
    }, GoStack.Bounded);
    return c;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string vfuncˢ => ref vfuncˢᵇ;
private static readonly @string vfuncˢᵇ = "vfunc"u8;

// vfunc takes a *V and a V
internal static @string vfunc(V _Δp0, ж<V> _Δp1) {
    return vfuncˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string valueIsIgnoredˢ => ref valueIsIgnoredˢᵇ;
private static readonly @string valueIsIgnoredˢᵇ = "value is ignored"u8;

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
    return new map<@string, nint>{["three"] = 3};
}

internal static void testExecute(slice<execTest> execTests, ж<global::go.text.template_package.Template> Ꮡtemplate, ж<testing.T> t) {
    ref var template = ref Ꮡtemplate.DerefOrNull();

    var b = @new<strings.Builder>();
    var funcs = new FuncMap(new map<@string, any>{
        ["add"] = ((Funcꓸꓸꓸ<nint, nint>)(add)),
        ["count"] = count,
        ["dddArg"] = ((Funcꓸꓸꓸ<nint, @string, @string>)(dddArg)),
        ["die"] = bool () => {
            throw panic("die");
        },
        ["echo"] = echo,
        ["makemap"] = ((Funcꓸꓸꓸ<@string, map<@string, @string>>)(makemap)),
        ["mapOfThree"] = mapOfThree,
        ["oneArg"] = oneArg,
        ["returnInt"] = returnInt,
        ["stringer"] = stringer,
        ["twoArgs"] = twoArgs,
        ["typeOf"] = typeOf,
        ["valueString"] = valueString,
        ["vfunc"] = vfunc,
        ["zeroArgs"] = zeroArgs
    });
    foreach (var (_, test) in execTests) {
        ж<global::go.text.template_package.Template> tmpl = default;
        error err = default;
        if (Ꮡtemplate == nil) {
            (tmpl, err) = New(test.name).Funcs(funcs).Parse(test.input);
        } else {
            (tmpl, err) = template.New(test.name).Funcs(funcs).Parse(test.input);
        }
        if (err != nil) {
            t.Errorf("%s: parse error: %s", test.name, err);
            continue;
        }
        b.Reset();
        err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), test.data);
        switch (ᐧ) {
        case {} when !test.ok && err == nil: {
            t.Errorf("%s: expected error; got none", test.name);
            continue;
            break;
        }
        case {} when test.ok && err != nil: {
            t.Errorf("%s: unexpected execute error: %s", test.name, err);
            continue;
            break;
        }
        case {} when !test.ok && err != nil: {
            // expected error, got one
            if (debug.Value) {
                fmt.Printf("%s: %s\n\t%s\n"u8, test.name, test.input, err);
            }
            break;
        }}
        @string result = b.String();
        if (result != test.output) {
            t.Errorf("%s: expected\n\t%q\ngot\n\t%q", test.name, test.output, result);
        }
    }
}

public static void TestExecute(ж<testing.T> t) {
    testExecute(execTests, nil, t);
}

internal static slice<@string> delimPairs = slice<@string>([
    "", "", // default
    "{{", "}}", // same as default
    "<<", ">>", // distinct
    "|", "|", // same
    "(日)", "(本)" // peculiar
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string strˢ => ref strˢᵇ;
private static readonly @string strˢᵇ = ".Str"u8;
internal static ref readonly @string delimsˢ => ref delimsˢᵇ;
private static readonly @string delimsˢᵇ = "delims"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string helloᶜ => ref helloᶜᵇ;
private static readonly @string helloᶜᵇ = "Hello, world"u8;

internal partial struct TestDelims_type /*dyn*/ {
    public @string Str;
}

public static void TestDelims(ж<testing.T> t) {
    @string hello = helloᶜ;
    TestDelims_type value = new TestDelims_type(hello);
    for (nint i = 0; i < len(delimPairs); i += 2) {
        @string text = strˢ;
        @string left = delimPairs[i + 0];
        @string trueLeft = left;
        @string right = delimPairs[i + 1];
        @string trueRight = right;
        if (left == ""u8) { // default case
            trueLeft = "{{";
        }
        if (right == ""u8) { // default case
            trueRight = "}}";
        }
        text = trueLeft + text + trueRight;
        // Now add a comment
        text += trueLeft + "/*comment*/"u8 + trueRight;
        // Now add  an action containing a string.
        text += trueLeft + @""""u8 + trueLeft + @""""u8 + trueRight;
        // At this point text looks like `{{.Str}}{{/*comment*/}}{{"{{"}}`.
        var (tmpl, err) = New(delimsˢ).Delims(left, right).Parse(text);
        if (err != nil) {
            t.Fatalf("delim %q text %q parse err %s", left, text, err);
        }
        ж<strings.Builder> b = @new<strings.Builder>();
        err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), value);
        if (err != nil) {
            t.Fatalf("delim %q exec err %s", left, err);
        }
        if (b.String() != hello + trueLeft) {
            t.Errorf("expected %q got %q", hello + trueLeft, b.String());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = "error"u8;
internal static ref readonly @string myErrorTrueˢ => ref myErrorTrueˢᵇ;
private static readonly @string myErrorTrueˢᵇ = "{{.MyError true}}"u8;

// Check that an error from a method flows back to the top.
public static void TestExecuteError(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var tmpl = New(errorˢ);
    var (_, err) = tmpl.Parse(myErrorTrueˢ);
    if (err != nil) {
        t.Fatalf("parse error: %s", err);
    }
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(b), tVal.OrTypedNil());
    if (err == nil) {
        t.Errorf("expected error; got none");
    } else if (!strings.Contains(err.Error(), myError.Error())) {
        if (debug.Value) {
            fmt.Printf("test execute error: %s\n"u8, err);
        }
        t.Errorf("expected myError; got %s", err);
    }
}

internal static ref readonly @string execErrorText => ref execErrorTextᵇ;
private static readonly @string execErrorTextᵇ = """
line 1
line 2
line 3
{{template "one" .}}
{{define "one"}}{{template "two" .}}{{end}}
{{define "two"}}{{template "three" .}}{{end}}
{{define "three"}}{{index "hi" $}}{{end}}
"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string topˢ => ref topˢᵇ;
private static readonly @string topˢᵇ = "top"u8;
internal static readonly object parseErrorˢ = (@string)"parse error:"u8;
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = @"template: top:7:20: executing ""three"" at <index ""hi"" $>: error calling index: index out of range: 5"u8;

// Check that an error from a nested template contains all the relevant information.
public static void TestExecError(ж<testing.T> t) {
    var (tmpl, err) = New(topˢ).Parse(execErrorText);
    if (err != nil) {
        t.Fatal(parseErrorˢ, err);
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)5); // 5 is out of range indexing "hi"
    if (err == nil) {
        t.Fatal(expectedErrorˢ);
    }
    @string want = wantᶜ;
    @string got = err.Error();
    if (got != want) {
        t.Errorf("expected\n%q\ngot\n%q", want, got);
    }
}

public partial struct CustomError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string heyoˢ => ref heyoˢᵇ;
private static readonly @string heyoˢᵇ = "heyo !"u8;

public static @string Error(this ref CustomError _) {
    return heyoˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errˢ => ref errˢᵇ;
private static readonly @string errˢᵇ = "{{ err }}"u8;

// Check that a custom error can be returned.
public static void TestExecError_CustomError(ж<testing.T> t) {
    var failingFunc = (@string, error) () => ("", new template_internal_test_package.CustomErrorжerror(Ꮡ(new CustomError())));
    var (ᴛ3, ᴛ4) = New(topˢ).Funcs(new FuncMap(new map<@string, any>{
        ["err"] = (failingFunc).OrTypedNilFunc()
    })).Parse(errˢ);
    var tmpl = Must(ᴛ3, ᴛ4);

    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), default);

    {
        var (_, ok) = errors.AsType<ж<CustomError>>(err); if (!ok) {
            t.Fatalf("expected custom error; got %s", err);
        }
    }
}

internal partial struct TestJSEscaping_testCases /*dyn*/ {
    internal @string @in, exp;
}

public static void TestJSEscaping(ж<testing.T> t) {
    var testCases = slice<TestJSEscaping_testCases>([
        new(@"a", @"a"),
        new(@"'foo", @"\'foo"),
        new(@"Go ""jump"" \", @"Go \""jump\"" \\"),
        new(@"Yukihiro says ""今日は世界""", @"Yukihiro says \""今日は世界\"""),
        new("unprintable \uFFFE", @"unprintable \uFFFE"),
        new(@"<html>", @"\u003Chtml\u003E"),
        new(@"no = in attributes", @"no \u003D in attributes"),
        new(@"&#x27; does not become HTML entity", @"\u0026#x27; does not become HTML entity")
    ]);
    foreach (var (_, tc) in testCases) {
        @string s = JSEscapeString(tc.@in);
        if (s != tc.exp) {
            t.Errorf("JS escaping [%s] got [%s] want [%s]", tc.@in, s, tc.exp);
        }
    }
}

// A nice example: walk a binary tree.
public partial struct Tree {
    public nint Val;
    public ж<Tree> Left, Right;
}

// Use different delimiters to test Set.Delims.
// Also test the trimming of leading and trailing spaces.
internal static ref readonly @string treeTemplate => ref treeTemplateᵇ;
private static readonly @string treeTemplateᵇ = """

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

"""u8.Raw();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootˢ => ref rootˢᵇ;
private static readonly @string rootˢᵇ = "root"u8;
internal static ref readonly @string treeˢ => ref treeˢᵇ;
private static readonly @string treeˢᵇ = "tree"u8;
internal static readonly object execErrorˢ = (@string)"exec error:"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string expectᶜ => ref expectᶜᵇ;
private static readonly @string expectᶜᵇ = "[1[2[3[4]][5[6]]][7[8[9]][10[11]]]]"u8;

public static void TestTree(ж<testing.T> t) {
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
    var (tmpl, err) = New(rootˢ).Delims("(", ")").Parse(treeTemplate);
    if (err != nil) {
        t.Fatal(parseErrorˢ, err);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    @string expect = expectᶜ;
    // First by looking up the template.
    err = tmpl.Lookup(treeˢ).Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), tree.OrTypedNil());
    if (err != nil) {
        t.Fatal(execErrorˢ, err);
    }
    @string result = b.String();
    if (result != expect) {
        t.Errorf("expected %q got %q", expect, result);
    }
    // Then direct to execution.
    b.Reset();
    err = tmpl.ExecuteTemplate(new template_test_package.strings_BuilderжWriter(Ꮡb), treeˢ, tree.OrTypedNil());
    if (err != nil) {
        t.Fatal(execErrorˢ, err);
    }
    result = b.String();
    if (result != expect) {
        t.Errorf("expected %q got %q", expect, result);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "Name"u8;
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
internal static ref readonly @string xxxˢ => ref xxxˢᵇ;
private static readonly @string xxxˢᵇ = "XXX"u8;

public static void TestExecuteOnNewTemplate(ж<testing.T> t) {
    // This is issue 3872.
    New(nameˢ).Templates();
    // This is issue 11379.
    @new<global::go.text.template_package.Template>().Templates();
    @new<global::go.text.template_package.Template>().Parse(""u8);
    @new<global::go.text.template_package.Template>().New(abcˢ).Parse(""u8);
    @new<global::go.text.template_package.Template>().Execute(default, default);               // returns an error (but does not crash)
    @new<global::go.text.template_package.Template>().ExecuteTemplate(default, xxxˢ, default); // returns an error (but does not crash)
}

internal static ref readonly @string testTemplates => ref testTemplatesᵇ;
private static readonly @string testTemplatesᵇ = @"{{define ""one""}}one{{end}}{{define ""two""}}two{{end}}"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string emptyˢ => ref emptyˢᵇ;
private static readonly @string emptyˢᵇ = "empty"u8;
internal static readonly object expectedInitialErrorˢ = (@string)"expected initial error"u8;
internal static ref readonly @string templateEmptyEmptyIsAnˢ => ref templateEmptyEmptyIsAnˢᵇ;
private static readonly @string templateEmptyEmptyIsAnˢᵇ = @"template: empty: ""empty"" is an incomplete or empty template"u8;
internal static ref readonly @string secondaryˢ => ref secondaryˢᵇ;
private static readonly @string secondaryˢᵇ = "secondary"u8;
internal static readonly object expectedSecondErrorˢ = (@string)"expected second error"u8;

public static void TestMessageForExecuteEmpty(ж<testing.T> t) {
    // Test a truly empty template.
    var tmpl = New(emptyˢ);
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)0);
    if (err == nil) {
        t.Fatal(expectedInitialErrorˢ);
    }
    @string got = err.Error();
    @string want = templateEmptyEmptyIsAnˢ;
    if (got != want) {
        t.Errorf("expected error %s got %s", want, got);
    }
    // Add a non-empty template to check that the error is helpful.
    (var tests, err) = New(""u8).Parse(testTemplates);
    if (err != nil) {
        t.Fatal(err);
    }
    tmpl.AddParseTree(secondaryˢ, tests.Tree);
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)0);
    if (err == nil) {
        t.Fatal(expectedSecondErrorˢ);
    }
    got = err.Error();
    want = templateEmptyEmptyIsAnˢ;
    if (got != want) {
        t.Errorf("expected error %s got %s", want, got);
    }
    // Make sure we can execute the secondary.
    err = tmpl.ExecuteTemplate(new template_test_package.bytes_BufferжWriter(Ꮡb), secondaryˢ, (nint)0);
    if (err != nil) {
        t.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xPrintfˢ => ref xPrintfˢᵇ;
private static readonly @string xPrintfˢᵇ = @"{{""x"" | printf}}"u8;

public static void TestFinalForPrintf(ж<testing.T> t) {
    var (tmpl, err) = New(""u8).Parse(xPrintfˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), (nint)0);
    if (err != nil) {
        t.Fatal(err);
    }
}

internal partial struct cmpTest {
    internal @string expr;
    internal @string truth;
    internal bool ok;
}

internal static slice<cmpTest> cmpTests = slice<cmpTest>([
    new("eq true true", "true", true),
    new("eq true false", "false", true),
    new("eq 1+2i 1+2i", "true", true),
    new("eq 1+2i 1+3i", "false", true),
    new("eq 1.5 1.5", "true", true),
    new("eq 1.5 2.5", "false", true),
    new("eq 1 1", "true", true),
    new("eq 1 2", "false", true),
    new("eq `xy` `xy`", "true", true),
    new("eq `xy` `xyz`", "false", true),
    new("eq .Uthree .Uthree", "true", true),
    new("eq .Uthree .Ufour", "false", true),
    new("eq 3 4 5 6 3", "true", true),
    new("eq 3 4 5 6 7", "false", true),
    new("ne true true", "false", true),
    new("ne true false", "true", true),
    new("ne 1+2i 1+2i", "false", true),
    new("ne 1+2i 1+3i", "true", true),
    new("ne 1.5 1.5", "false", true),
    new("ne 1.5 2.5", "true", true),
    new("ne 1 1", "false", true),
    new("ne 1 2", "true", true),
    new("ne `xy` `xy`", "false", true),
    new("ne `xy` `xyz`", "true", true),
    new("ne .Uthree .Uthree", "false", true),
    new("ne .Uthree .Ufour", "true", true),
    new("lt 1.5 1.5", "false", true),
    new("lt 1.5 2.5", "true", true),
    new("lt 1 1", "false", true),
    new("lt 1 2", "true", true),
    new("lt `xy` `xy`", "false", true),
    new("lt `xy` `xyz`", "true", true),
    new("lt .Uthree .Uthree", "false", true),
    new("lt .Uthree .Ufour", "true", true),
    new("le 1.5 1.5", "true", true),
    new("le 1.5 2.5", "true", true),
    new("le 2.5 1.5", "false", true),
    new("le 1 1", "true", true),
    new("le 1 2", "true", true),
    new("le 2 1", "false", true),
    new("le `xy` `xy`", "true", true),
    new("le `xy` `xyz`", "true", true),
    new("le `xyz` `xy`", "false", true),
    new("le .Uthree .Uthree", "true", true),
    new("le .Uthree .Ufour", "true", true),
    new("le .Ufour .Uthree", "false", true),
    new("gt 1.5 1.5", "false", true),
    new("gt 1.5 2.5", "false", true),
    new("gt 1 1", "false", true),
    new("gt 2 1", "true", true),
    new("gt 1 2", "false", true),
    new("gt `xy` `xy`", "false", true),
    new("gt `xy` `xyz`", "false", true),
    new("gt .Uthree .Uthree", "false", true),
    new("gt .Uthree .Ufour", "false", true),
    new("gt .Ufour .Uthree", "true", true),
    new("ge 1.5 1.5", "true", true),
    new("ge 1.5 2.5", "false", true),
    new("ge 2.5 1.5", "true", true),
    new("ge 1 1", "true", true),
    new("ge 1 2", "false", true),
    new("ge 2 1", "true", true),
    new("ge `xy` `xy`", "true", true),
    new("ge `xy` `xyz`", "false", true),
    new("ge `xyz` `xy`", "true", true),
    new("ge .Uthree .Uthree", "true", true),
    new("ge .Uthree .Ufour", "false", true),
    new("ge .Ufour .Uthree", "true", true),
    // Mixing signed and unsigned integers.
    new("eq .Uthree .Three", "true", true),
    new("eq .Three .Uthree", "true", true),
    new("le .Uthree .Three", "true", true),
    new("le .Three .Uthree", "true", true),
    new("ge .Uthree .Three", "true", true),
    new("ge .Three .Uthree", "true", true),
    new("lt .Uthree .Three", "false", true),
    new("lt .Three .Uthree", "false", true),
    new("gt .Uthree .Three", "false", true),
    new("gt .Three .Uthree", "false", true),
    new("eq .Ufour .Three", "false", true),
    new("lt .Ufour .Three", "false", true),
    new("gt .Ufour .Three", "true", true),
    new("eq .NegOne .Uthree", "false", true),
    new("eq .Uthree .NegOne", "false", true),
    new("ne .NegOne .Uthree", "true", true),
    new("ne .Uthree .NegOne", "true", true),
    new("lt .NegOne .Uthree", "true", true),
    new("lt .Uthree .NegOne", "false", true),
    new("le .NegOne .Uthree", "true", true),
    new("le .Uthree .NegOne", "false", true),
    new("gt .NegOne .Uthree", "false", true),
    new("gt .Uthree .NegOne", "true", true),
    new("ge .NegOne .Uthree", "false", true),
    new("ge .Uthree .NegOne", "true", true),
    new("eq (index `x` 0) 'x'", "true", true), // The example that triggered this rule.
    new("eq (index `x` 0) 'y'", "false", true),
    new("eq .V1 .V2", "true", true),
    new("eq .Ptr .Ptr", "true", true),
    new("eq .Ptr .NilPtr", "false", true),
    new("eq .NilPtr .NilPtr", "true", true),
    new("eq .Iface1 .Iface1", "true", true),
    new("eq .Iface1 .NilIface", "false", true),
    new("eq .NilIface .NilIface", "true", true),
    new("eq .NilIface .Iface1", "false", true),
    new("eq .NilIface 0", "false", true),
    new("eq 0 .NilIface", "false", true),
    new("eq .Map .Map", "true", true),        // Uncomparable types but nil is OK.
    new("eq .Map nil", "true", true),         // Uncomparable types but nil is OK.
    new("eq nil .Map", "true", true),         // Uncomparable types but nil is OK.
    new("eq .Map .NonNilMap", "false", true), // Uncomparable types but nil is OK.
    // Errors
    new("eq `xy` 1", "", false),               // Different types.
    new("eq 2 2.0", "", false),                // Different types.
    new("lt true true", "", false),            // Unordered types.
    new("lt 1+0i 1+0i", "", false),            // Unordered types.
    new("eq .Ptr 1", "", false),               // Incompatible types.
    new("eq .Ptr .NegOne", "", false),         // Incompatible types.
    new("eq .Map .V1", "", false),             // Uncomparable types.
    new("eq .NonNilMap .NonNilMap", "", false) // Uncomparable types.
]);

internal partial struct TestComparison_type /*dyn*/ {
    public nuint Uthree, Ufour;
    public nint NegOne, Three;
    public ж<nint> Ptr, NilPtr;
    public map<nint, nint> NonNilMap;
    public map<nint, nint> Map;
    public V V1, V2;
    public fmt.Stringer Iface1, NilIface;
}

public static void TestComparison(ж<testing.T> t) {
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
        if (err != nil) {
            t.Fatalf("%q: %s", test.expr, err);
        }
        b.Reset();
        err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(b), ᏑcmpStruct);
        if (test.ok && err != nil) {
            t.Errorf("%s errored incorrectly: %s", test.expr, err);
            continue;
        }
        if (!test.ok && err == nil) {
            t.Errorf("%s did not error", test.expr);
            continue;
        }
        if (b.String() != test.truth) {
            t.Errorf("%s: want %s; got %s", test.expr, test.truth, b.String());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noValueˢ => ref noValueˢᵇ;
private static readonly @string noValueˢᵇ = "99 <no value>"u8;
internal static ref readonly @string missingkeyDefaultˢ => ref missingkeyDefaultˢᵇ;
private static readonly @string missingkeyDefaultˢᵇ = "missingkey=default"u8;
internal static readonly object defaultˢ = (@string)"default:"u8;
internal static ref readonly @string missingkeyZeroˢ => ref missingkeyZeroˢᵇ;
private static readonly @string missingkeyZeroˢᵇ = "missingkey=zero"u8;
internal static readonly object zeroˢ = (@string)"zero:"u8;
internal static ref readonly @string missingkeyErrorˢ => ref missingkeyErrorˢᵇ;
private static readonly @string missingkeyErrorˢᵇ = "missingkey=error"u8;

public static void TestMissingMapKey(ж<testing.T> t) {
    var data = new map<@string, nint>{
        ["x"] = 99
    };
    var (tmpl, err) = New("t1").Parse("{{.x}} {{.y}}");
    if (err != nil) {
        t.Fatal(err);
    }
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    // By default, just get "<no value>"
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err != nil) {
        t.Fatal(err);
    }
    @string want = noValueˢ;
    @string got = b.String();
    if (got != want) {
        t.Errorf("got %q; expected %q", got, want);
    }
    // Same if we set the option explicitly to the default.
    tmpl.Option(missingkeyDefaultˢ);
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err != nil) {
        t.Fatal(defaultˢ, err);
    }
    want = noValueˢ;
    got = b.String();
    if (got != want) {
        t.Errorf("got %q; expected %q", got, want);
    }
    // Next we ask for a zero value
    tmpl.Option(missingkeyZeroˢ);
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err != nil) {
        t.Fatal(zeroˢ, err);
    }
    want = "99 0";
    got = b.String();
    if (got != want) {
        t.Errorf("got %q; expected %q", got, want);
    }
    // Now we ask for an error.
    tmpl.Option(missingkeyErrorˢ);
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), data);
    if (err == nil) {
        t.Errorf("expected error; got none");
    }
    // same Option, but now a nil interface: ask for an error
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), default);
    t.Log(err);
    if (err == nil) {
        t.Errorf("expected error for nil-interface; got none");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloUnterminatedSomeˢ => ref helloUnterminatedSomeˢᵇ;
private static readonly @string helloUnterminatedSomeˢᵇ = "hello\n\n{{`unterminated\n\n\n\n}}\n some more\n\n"u8;
internal static ref readonly @string x3UnterminatedRawQuotedˢ => ref x3UnterminatedRawQuotedˢᵇ;
private static readonly @string x3UnterminatedRawQuotedˢᵇ = "X:3: unterminated raw quoted string"u8;

// Test that the error message for multiline unterminated string
// refers to the line number of the opening quote.
public static void TestUnterminatedStringError(ж<testing.T> t) {
    var (_, err) = New("X").Parse(helloUnterminatedSomeˢ);
    if (err == nil) {
        t.Fatal(expectedErrorˢ);
    }
    @string str = err.Error();
    if (!strings.Contains(str, x3UnterminatedRawQuotedˢ)) {
        t.Fatalf("unexpected error: %s", str);
    }
}

internal static ref readonly @string alwaysErrorText => ref alwaysErrorTextᵇ;
private static readonly @string alwaysErrorTextᵇ = "always be failing"u8;

internal static error alwaysError = errors.New(alwaysErrorText);

public partial struct ErrorWriter /*num:nint*/;

public static (nint, error) Write(this ErrorWriter e, slice<byte> p) {
    return (0, alwaysError);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;
internal static readonly object expectedErrorGotNoneˢ = (@string)"expected error; got none"u8;
internal static ref readonly @string helloXYˢ => ref helloXYˢᵇ;
private static readonly @string helloXYˢᵇ = "hello, {{.X.Y}}"u8;
internal static ref readonly @string fieldXInTypeIntˢ => ref fieldXInTypeIntˢᵇ;
private static readonly @string fieldXInTypeIntˢᵇ = "field X in type int"u8;

public static void TestExecuteGivesExecError(ж<testing.T> t) {
    // First, a non-execution error shouldn't be an ExecError.
    var (tmpl, err) = New("X").Parse(helloˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    err = tmpl.Execute((ErrorWriter)0, (nint)0);
    if (err == nil) {
        t.Fatal(expectedErrorGotNoneˢ);
    }
    if (err.Error() != alwaysErrorText) {
        t.Errorf("expected %q error; got %q", alwaysErrorText, err);
    }
    // This one should be an ExecError.
    (tmpl, err) = New("X").Parse(helloXYˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    err = tmpl.Execute(io.Discard, (nint)0);
    if (err == nil) {
        t.Fatal(expectedErrorGotNoneˢ);
    }
    var (eerr, ok) = err._<ExecError>(ᐧ);
    if (!ok) {
        t.Fatalf("did not expect ExecError %s", eerr);
    }
    @string expect = fieldXInTypeIntˢ;
    if (!strings.Contains(err.Error(), expect)) {
        t.Errorf("expected %q; got %q", expect, err);
    }
}

internal static nint funcNameTestFunc() {
    return 0;
}

public static void TestGoodFuncNames(ж<testing.T> t) {
    var names = slice<@string>([
        "_",
        "a",
        "a1",
        "a1",
        "Ӵ"
    ]);
    foreach (var (_, name) in names) {
        var tmpl = New("X").Funcs(
            new FuncMap(new map<@string, any>{
                [name] = funcNameTestFunc
            }));
        if (tmpl == nil) {
            t.Fatalf("nil result for %q", name);
        }
    }
}

public static void TestBadFuncNames(ж<testing.T> t) {
    var names = slice<@string>([
        "",
        "2",
        "a-b"
    ]);
    foreach (var (_, name) in names) {
        testBadFuncName(name, t);
    }
}

internal partial struct TestIsTrue_tests /*dyn*/ {
    internal any v;
    internal bool want;
}

public static void TestIsTrue(ж<testing.T> t) {
    ж<nint> nil_ptr = default;
    channel<nint> nil_chan = default;
    var tests = slice<TestIsTrue_tests>([
        new((nint)1, true),
        new((nint)0, false),
        new((uint8)1, true),
        new((uint8)0, false),
        new((float64)1.0D, true),
        new((float64)0.0D, false),
        new((complex64)1.0F, true),
        new((complex64)0.0F, false),
        new(true, true),
        new(false, false),
        new(new nint[]{1, 2}.array(), true),
        new(new nint[]{}.array(), false),
        new(slice<byte>("abc"u8), true),
        new(slice<byte>(""u8), false),
        new(new map<@string, nint>{["a"] = 1, ["b"] = 2}, true),
        new(new map<@string, nint>{}, false),
        new(new channel<nint>(0), true),
        new(nil_chan, false),
        new(@new<nint>(), true),
        new(nil_ptr.OrTypedNil(), false),
        new(@unsafe.Pointer.FromPinnedBox(@new<nint>()), true),
        new(@unsafe.Pointer.FromPinnedBox(nil_ptr), false)
    ]);
    foreach (var (_, test_case) in tests) {
        var (got, _) = IsTrue(test_case.v);
        if (got != test_case.want) {
            t.Fatalf("expect result %v, got %v", test_case.want, got);
        }
    }
}

internal static void testBadFuncName(@string name, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Helper();
        defer(() => {
            recover();
        }, ref ᒐ);
        New("X").Funcs(
            new FuncMap(new map<@string, any>{
                [name] = funcNameTestFunc
            }));
        // If we get here, the name did not cause a panic, which is how Funcs
        // reports an error.
        t.Errorf("%q succeeded incorrectly as function name", name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string outerˢ => ref outerˢᵇ;
private static readonly @string outerˢᵇ = "outer"u8;
internal static readonly object goodbyeˢ = (@string)"goodbye"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = @"a({{block ""inner"" .}}bar({{.}})baz{{end}})b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = @"a(bar(hello)baz)b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string overlayᶜ => ref overlayᶜᵇ;
private static readonly @string overlayᶜᵇ = @"{{define ""inner""}}foo({{.}})bar{{end}}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string want2ᶜ => ref want2ᶜᵇ;
private static readonly @string want2ᶜᵇ = @"a(foo(goodbye)bar)b"u8;

public static void TestBlock(ж<testing.T> t) {
    @string input = inputᶜ;
    @string want = wantᶜ1;
    @string overlay = overlayᶜ;
    @string want2 = want2ᶜ;
    var (tmpl, err) = New(outerˢ).Parse(input);
    if (err != nil) {
        t.Fatal(err);
    }
    var (ᴛ5, ᴛ6) = tmpl.Clone();
    (var tmpl2, err) = Must(ᴛ5, ᴛ6).Parse(overlay);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    {
        var errΔ1 = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), helloˢ); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    {
        @string got = buf.String(); if (got != want) {
            t.Errorf("got %q, want %q", got, want);
        }
    }

    buf.Reset();
    {
        var errΔ2 = tmpl2.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), goodbyeˢ); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
    {
        @string got = buf.String(); if (got != want2) {
            t.Errorf("got %q, want %q", got, want2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmplˢ => ref tmplˢᵇ;
private static readonly @string tmplˢᵇ = "tmpl"u8;
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;

internal partial struct TestEvalFieldErrors_tests /*dyn*/ {
    internal @string name, src;
    internal any value;
    internal @string want;
}

public static void TestEvalFieldErrors(ж<testing.T> t) {
    var tests = slice<TestEvalFieldErrors_tests>([
        new(
            // Check that calling an invalid field on nil pointer
            // prints a field error instead of a distracting nil
            // pointer error. https://golang.org/issue/15125
            "MissingFieldOnNil",
            "{{.MissingField}}",
            (ж<T>)nil,
            "can't evaluate field MissingField in type *template.T"
        ),
        new(
            "MissingFieldOnNonNil",
            "{{.MissingField}}",
            Ꮡ(new T()),
            "can't evaluate field MissingField in type *template.T"
        ),
        new(
            "ExistingFieldOnNil",
            "{{.X}}",
            (ж<T>)nil,
            "nil pointer evaluating *template.T.X"
        ),
        new(
            "MissingKeyOnNilMap",
            "{{.MissingKey}}",
            (ж<map<@string, @string>>)nil,
            "nil pointer evaluating *map[string]string.MissingKey"
        ),
        new(
            "MissingKeyOnNilMapPtr",
            "{{.MissingKey}}",
            (ж<map<@string, @string>>)nil,
            "nil pointer evaluating *map[string]string.MissingKey"
        ),
        new(
            "MissingKeyOnMapPtrToNil",
            "{{.MissingKey}}",
            Ꮡ(new map<@string, @string>{}),
            "<nil>"
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestEvalFieldErrors_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (ᴛ7, ᴛ8) = New(tmplˢ).Parse(tcʗ1.src);
            var tmpl = Must(ᴛ7, ᴛ8);
            var err = tmpl.Execute(io.Discard, tcʗ1.value);
            @string got = nilˢ;
            if (err != nil) {
                got = err.Error();
            }
            if (!strings.HasSuffix(got, tcʗ1.want)) {
                tΔ1.Fatalf("got error %q, want %q", got, tcʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;
internal static ref readonly @string templateTmplˢ => ref templateTmplˢᵇ;
private static readonly @string templateTmplˢᵇ = @"{{template ""tmpl"" .}}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ2 => ref wantᶜ2ᵇ;
private static readonly @string wantᶜ2ᵇ = "exceeded maximum template depth"u8;

public static void TestMaxExecDepth(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    var (ᴛ9, ᴛ10) = New(tmplˢ).Parse(templateTmplˢ);
    var tmpl = Must(ᴛ9, ᴛ10);
    var err = tmpl.Execute(io.Discard, default);
    @string got = nilˢ;
    if (err != nil) {
        got = err.Error();
    }
    @string want = wantᶜ2;
    if (!strings.Contains(got, want)) {
        t.Errorf("got error %q; want %q", got, want);
    }
}

public static void TestAddrOfIndex(ж<testing.T> t) {
    // golang.org/issue/14916.
    // Before index worked on reflect.Values, the .String could not be
    // found on the (incorrectly unaddressable) V value,
    // in contrast to range, which worked fine.
    // Also testing that passing a reflect.Value to tmpl.Execute works.
    var texts = slice<@string>([
        @"{{range .}}{{.String}}{{end}}",
        @"{{with index . 0}}{{.String}}{{end}}"
    ]);
    foreach (var (_, text) in texts) {
        var (ᴛ11, ᴛ12) = New(tmplˢ).Parse(text);
        var tmpl = Must(ᴛ11, ᴛ12);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), reflect.ValueOf(slice<V>([new(1)])));
        if (err != nil) {
            t.Fatalf("%s: Execute: %v", text, err);
        }
        if (buf.String() != "<1>"u8) {
            t.Fatalf("%s: template output = %q, want %q", text, Ꮡbuf, (@string)"<1>");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorˢ2 => ref errorˢ2ᵇ;
private static readonly @string errorˢ2ᵇ = "ERROR:"u8;

internal partial struct TestInterfaceValues_tests /*dyn*/ {
    internal @string text;
    internal @string @out;
}

public static void TestInterfaceValues(ж<testing.T> t) {
    // golang.org/issue/17714.
    // Before index worked on reflect.Values, interface values
    // were always implicitly promoted to the underlying value,
    // except that nil interfaces were promoted to the zero reflect.Value.
    // Eliminating a round trip to interface{} and back to reflect.Value
    // eliminated this promotion, breaking these cases.
    var tests = slice<TestInterfaceValues_tests>([
        new(@"{{index .Nil 1}}", "ERROR: index of untyped nil"),
        new(@"{{index .Slice 2}}", "2"),
        new(@"{{index .Slice .Two}}", "2"),
        new(@"{{call .Nil 1}}", "ERROR: call of nil"),
        new(@"{{call .PlusOne 1}}", "2"),
        new(@"{{call .PlusOne .One}}", "2"),
        new(@"{{and (index .Slice 0) true}}", "0"),
        new(@"{{and .Zero true}}", "0"),
        new(@"{{and (index .Slice 1) false}}", "false"),
        new(@"{{and .One false}}", "false"),
        new(@"{{or (index .Slice 0) false}}", "false"),
        new(@"{{or .Zero false}}", "false"),
        new(@"{{or (index .Slice 1) true}}", "1"),
        new(@"{{or .One true}}", "1"),
        new(@"{{not (index .Slice 0)}}", "true"),
        new(@"{{not .Zero}}", "true"),
        new(@"{{not (index .Slice 1)}}", "false"),
        new(@"{{not .One}}", "false"),
        new(@"{{eq (index .Slice 0) .Zero}}", "true"),
        new(@"{{eq (index .Slice 1) .One}}", "true"),
        new(@"{{ne (index .Slice 0) .Zero}}", "false"),
        new(@"{{ne (index .Slice 1) .One}}", "false"),
        new(@"{{ge (index .Slice 0) .One}}", "false"),
        new(@"{{ge (index .Slice 1) .Zero}}", "true"),
        new(@"{{gt (index .Slice 0) .One}}", "false"),
        new(@"{{gt (index .Slice 1) .Zero}}", "true"),
        new(@"{{le (index .Slice 0) .One}}", "true"),
        new(@"{{le (index .Slice 1) .Zero}}", "false"),
        new(@"{{lt (index .Slice 0) .One}}", "true"),
        new(@"{{lt (index .Slice 1) .Zero}}", "false")
    ]);

    foreach (var (_, tt) in tests) {
        var (ᴛ13, ᴛ14) = New(tmplˢ).Parse(tt.text);
        var tmpl = Must(ᴛ13, ᴛ14);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡbuf), new map<@string, any>{
            ["PlusOne"] = nint (nint n) => n + 1,
            ["Slice"] = slice<nint>([0, 1, 2, 3]),
            ["One"] = (nint)1,
            ["Two"] = (nint)2,
            ["Nil"] = default,
            ["Zero"] = (nint)0
        });
        if (strings.HasPrefix(tt.@out, errorˢ2)) {
            @string e = strings.TrimSpace(strings.TrimPrefix(tt.@out, errorˢ2));
            if (err == nil || !strings.Contains(err.Error(), e)) {
                t.Errorf("%s: Execute: %v, want error %q", tt.text, err, e);
            }
            continue;
        }
        if (err != nil) {
            t.Errorf("%s: Execute: %v", tt.text, err);
            continue;
        }
        if (buf.String() != tt.@out) {
            t.Errorf("%s: template output = %q, want %q", tt.text, Ꮡbuf, tt.@out);
        }
    }
}

internal partial struct TestExecutePanicDuringCall_tests /*dyn*/ {
    internal @string name;
    internal @string input;
    internal any data;
    internal @string wantErr;
}

// Check that panics during calls are recovered and returned as errors.
public static void TestExecutePanicDuringCall(ж<testing.T> t) {
    var funcs = new map<@string, any>{
        ["doPanic"] = @string () => {
            throw panic("custom panic string");
        }
    };
    var tests = slice<TestExecutePanicDuringCall_tests>([
        new(
            "direct func call panics",
            "{{doPanic}}", (ж<T>)nil,
            @"template: t:1:2: executing ""t"" at <doPanic>: error calling doPanic: custom panic string"
        ),
        new(
            "indirect func call panics",
            "{{call doPanic}}", (ж<T>)nil,
            @"template: t:1:7: executing ""t"" at <doPanic>: error calling doPanic: custom panic string"
        ),
        new(
            "direct method call panics",
            "{{.GetU}}", (ж<T>)nil,
            @"template: t:1:2: executing ""t"" at <.GetU>: error calling GetU: runtime error: invalid memory address or nil pointer dereference"
        ),
        new(
            "indirect method call panics",
            "{{call .GetU}}", (ж<T>)nil,
            @"template: t:1:7: executing ""t"" at <.GetU>: error calling GetU: runtime error: invalid memory address or nil pointer dereference"
        ),
        new(
            "func field call panics",
            "{{call .PanicFunc}}", tVal.OrTypedNil(),
            @"template: t:1:2: executing ""t"" at <call .PanicFunc>: error calling call: test panic"
        ),
        new(
            "method call on nil interface",
            "{{.NonEmptyInterfaceNil.Method0}}", tVal.OrTypedNil(),
            @"template: t:1:23: executing ""t"" at <.NonEmptyInterfaceNil.Method0>: nil pointer evaluating template.I.Method0"
        )
    ]);
    foreach (var (_, tc) in tests) {
        var b = @new<bytes.Buffer>();
        var (tmpl, err) = New("t").Funcs(funcs).Parse(tc.input);
        if (err != nil) {
            t.Fatalf("parse error: %s", err);
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(b), tc.data);
        if (err == nil) {
            t.Errorf("%s: expected error; got none", tc.name);
        } else if (!strings.Contains(err.Error(), tc.wantErr)) {
            if (debug.Value) {
                fmt.Printf("%s: test execute error: %s\n"u8, tc.name, err);
            }
            t.Errorf("%s: expected error:\n%s\ngot:\n%s", tc.name, tc.wantErr, err);
        }
    }
}

internal partial struct TestFunctionCheckDuringCall_tests /*dyn*/ {
    internal @string name;
    internal @string input;
    internal any data;
    internal @string wantErr;
}

public static void TestFunctionCheckDuringCall(ж<testing.T> t) {
    var tests = slice<TestFunctionCheckDuringCall_tests>([new(
        name: "call nothing",
        input: @"{{call}}",
        data: tVal.OrTypedNil(),
        wantErr: "wrong number of args for call: want at least 1 got 0"
    ),
        new(
            name: "call non-function",
            input: "{{call .True}}",
            data: tVal.OrTypedNil(),
            wantErr: "error calling call: non-function .True of type bool"
        ),
        new(
            name: "call func with wrong argument",
            input: "{{call .BinaryFunc 1}}",
            data: tVal.OrTypedNil(),
            wantErr: "error calling call: wrong number of args for .BinaryFunc: got 1 want 2"
        ),
        new(
            name: "call variadic func with wrong argument",
            input: @"{{call .VariadicFuncInt}}",
            data: tVal.OrTypedNil(),
            wantErr: "error calling call: wrong number of args for .VariadicFuncInt: got 0 want at least 1"
        ),
        new(
            name: "call too few return number func",
            input: @"{{call .TooFewReturnCountFunc}}",
            data: tVal.OrTypedNil(),
            wantErr: "error calling call: function .TooFewReturnCountFunc has 0 return values; should be 1 or 2"
        ),
        new(
            name: "call too many return number func",
            input: @"{{call .TooManyReturnCountFunc}}",
            data: tVal.OrTypedNil(),
            wantErr: "error calling call: function .TooManyReturnCountFunc has 3 return values; should be 1 or 2"
        ),
        new(
            name: "call invalid return type func",
            input: @"{{call .InvalidReturnTypeFunc}}",
            data: tVal.OrTypedNil(),
            wantErr: "error calling call: invalid function signature for .InvalidReturnTypeFunc: second return value should be error; is bool"
        ),
        new(
            name: "call pipeline",
            input: @"{{call (len ""test"")}}",
            data: default,
            wantErr: "error calling call: non-function len \"test\" of type int"
        )
    ]);

    foreach (var (_, tc) in tests) {
        var b = @new<bytes.Buffer>();
        var (tmpl, err) = New("t").Parse(tc.input);
        if (err != nil) {
            t.Fatalf("parse error: %s", err);
        }
        err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(b), tc.data);
        if (err == nil) {
            t.Errorf("%s: expected error; got none", tc.name);
        } else if (tc.wantErr == ""u8 || !strings.Contains(err.Error(), tc.wantErr)) {
            if (debug.Value) {
                fmt.Printf("%s: test execute error: %s\n"u8, tc.name, err);
            }
            t.Errorf("%s: expected error:\n%s\ngot:\n%s", tc.name, tc.wantErr, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string resultˢ => ref resultˢᵇ;
private static readonly @string resultˢᵇ = "result"u8;
internal static readonly object expectedErrorWithNoCallˢ = (@string)"expected error with no call, got none"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string textᶜ => ref textᶜᵇ;
private static readonly @string textᶜᵇ = "{{ (.)  }}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string textCallᶜ => ref textCallᶜᵇ;
private static readonly @string textCallᶜᵇ = "{{ (call .)  }}"u8;

// Issue 31810. Check that a parenthesized first argument behaves properly.
public static void TestIssue31810(ж<testing.T> t) {
    // A simple value with no arguments is fine.
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    @string text = textᶜ;
    var (tmpl, err) = New(""u8).Parse(text);
    if (err != nil) {
        t.Error(err);
    }
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), resultˢ);
    if (err != nil) {
        t.Error(err);
    }
    if (b.String() != "result"u8) {
        t.Errorf("%s got %q, expected %q", text, b.String(), resultˢ);
    }

    // Even a plain function fails - need to use call.
    var f = @string () => resultˢ;
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), (f).OrTypedNilFunc());
    if (err == nil) {
        t.Error(expectedErrorWithNoCallˢ);
    }

    // Works if the function is explicitly called.
    @string textCall = textCallᶜ;
    (tmpl, err) = New(""u8).Parse(textCall);
    b.Reset();
    err = tmpl.Execute(new template_test_package.strings_BuilderжWriter(Ꮡb), (f).OrTypedNilFunc());
    if (err != nil) {
        t.Error(err);
    }
    if (b.String() != "result"u8) {
        t.Errorf("%s got %q, expected %q", textCall, b.String(), resultˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rangeEndˢ => ref rangeEndˢᵇ;
private static readonly @string rangeEndˢᵇ = @"{{range .}}{{end}}"u8;
internal static readonly object expectedErrGotNilˢ = (@string)"expected err got nil"u8;
internal static ref readonly @string rangeOverSendOnlyChannelˢ => ref rangeOverSendOnlyChannelˢᵇ;
private static readonly @string rangeOverSendOnlyChannelˢᵇ = "range over send-only channel"u8;

// Issue 43065, range over send only channel
public static void TestIssue43065(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (ᴛ15, ᴛ16) = New(""u8).Parse(rangeEndˢ);
    var tmp = Must(ᴛ15, ᴛ16);
    var ch = new channel/*<-*/<nint>(0, GoChanDir.Send);
    var err = tmp.Execute(new template_test_package.bytes_BufferжWriter(Ꮡb), ch);
    if (err == nil) {
        t.Error(expectedErrGotNilˢ);
    } else if (!strings.Contains(err.Error(), rangeOverSendOnlyChannelˢ)) {
        t.Errorf("%s", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string templateBarˢ => ref templateBarˢᵇ;
private static readonly @string templateBarˢᵇ = @"{{ template ""bar"" . }}"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;

// Issue 39807: data race in html/template & text/template
public static partial void TestIssue39807(ж<testing.T> t) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);

    var (tplFoo, err) = New(fooˢ).Parse(templateBarˢ);
    if (err != nil) {
        t.Error(err);
    }

    (var tplBar, err) = New(barˢ).Parse(barˢ);
    if (err != nil) {
        t.Error(err);
    }

    nint gofuncs = 10;
    nint numTemplates = 10;

    for (nint i = 1; i <= gofuncs; i++) {
        Ꮡwg.Add(1);
        var tplBarʗ1 = tplBar;
        var tplFooʗ1 = tplFoo;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint j = 0; j < numTemplates; j++) {
                    var (_, errΔ1) = tplFooʗ1.AddParseTree(tplBarʗ1.Name(), tplBarʗ1.Tree);
                    if (errΔ1 != nil) {
                        t.Error(errΔ1);
                    }
                    errΔ1 = tplFooʗ1.Execute(io.Discard, default);
                    if (errΔ1 != nil) {
                        t.Error(errΔ1);
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
internal static readonly object didNotGetErrorForNilˢ = (@string)"did not get error for nil embedded struct"u8;
internal static ref readonly @string reflectIndirectionˢ => ref reflectIndirectionˢᵇ;
private static readonly @string reflectIndirectionˢᵇ = "reflect: indirection through nil pointer to embedded struct field A"u8;

internal partial struct TestIssue48215_A /*dyn*/ {
    public @string S;
}

internal partial struct TestIssue48215_B /*dyn*/ {
    /*embed*/ public ж<TestIssue48215_A> A;
}

// Issue 48215: embedded nil pointer causes panic.
// Fixed by adding FieldByIndexErr to the reflect package.
public static void TestIssue48215(ж<testing.T> t) {
    var (tmpl, err) = New(""u8).Parse(@"{{ .S }}");
    if (err != nil) {
        t.Fatal(err);
    }
    err = tmpl.Execute(io.Discard, new TestIssue48215_B());
    // We expect an error, not a panic.
    if (err == nil) {
        t.Fatal(didNotGetErrorForNilˢ);
    }
    if (!strings.Contains(err.Error(), reflectIndirectionˢ)) {
        t.Fatal(err);
    }
}

} // end template_internal_test_package
