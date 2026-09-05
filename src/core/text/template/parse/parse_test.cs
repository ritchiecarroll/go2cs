// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text.template;

using flag = flag_package;
using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using static go.text.template.parse_package;

partial class parse_internal_test_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸflag() {
    builtin.initPackage(typeof(flag_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸstrings() {
    builtin.initPackage(typeof(strings_package));
}

internal static ж<bool> debug = flag.Bool("debug"u8, false, "show the errors produced by the main tests"u8);

[GoType] internal partial struct numberTest {
    internal @string text;
    internal bool isInt;
    internal bool isUint;
    internal bool isFloat;
    internal bool isComplex;
    [GoEmbedded] internal int64 int64;
    [GoEmbedded] internal uint64 uint64;
    [GoEmbedded] internal float64 float64;
    [GoEmbedded] internal complex128 complex128;
}

// basics
// check that -0 is a uint.
// not octal!
// complex with 0 imaginary are float (and maybe integer)
// funny bases
// character constants
// some broken syntax
// Integer too large - issue 10634.
// Issue 8622 - 0xe parsed as floating point. Very embarrassing.
internal static slice<numberTest> numberTests = new numberTest[]{
    new("0"u8, true, true, true, false, 0, 0, 0D, 0D),
    new("-0"u8, true, true, true, false, 0, 0, 0D, 0D),
    new("73"u8, true, true, true, false, 73, 73, 73D, 0D),
    new("7_3"u8, true, true, true, false, 73, 73, 73D, 0D),
    new("0b10_010_01"u8, true, true, true, false, 73, 73, 73D, 0D),
    new("0B10_010_01"u8, true, true, true, false, 73, 73, 73D, 0D),
    new("073"u8, true, true, true, false, 59, 59, 59D, 0D),
    new("0o73"u8, true, true, true, false, 59, 59, 59D, 0D),
    new("0O73"u8, true, true, true, false, 59, 59, 59D, 0D),
    new("0x73"u8, true, true, true, false, 0x73, 0x73, 115D, 0D),
    new("0X73"u8, true, true, true, false, 0x73, 0x73, 115D, 0D),
    new("0x7_3"u8, true, true, true, false, 0x73, 0x73, 115D, 0D),
    new("-73"u8, true, false, true, false, -73, 0, -73D, 0D),
    new("+73"u8, true, false, true, false, 73, 0, 73D, 0D),
    new("100"u8, true, true, true, false, 100, 100, 100D, 0D),
    new("1e9"u8, true, true, true, false, 1000000000, 1000000000, 1e9D, 0D),
    new("-1e9"u8, true, false, true, false, -1000000000, 0, -1e9D, 0D),
    new("-1.2"u8, false, false, true, false, 0, 0, -1.2D, 0D),
    new("1e19"u8, false, true, true, false, 0, 10000000000000000000, 1e19D, 0D),
    new("1e1_9"u8, false, true, true, false, 0, 10000000000000000000, 1e19D, 0D),
    new("1E19"u8, false, true, true, false, 0, 10000000000000000000, 1e19D, 0D),
    new("-1e19"u8, false, false, true, false, 0, 0, -1e19D, 0D),
    new("0x_1p4"u8, true, true, true, false, 16, 16, 16D, 0D),
    new("0X_1P4"u8, true, true, true, false, 16, 16, 16D, 0D),
    new("0x_1p-4"u8, false, false, true, false, 0, 0, 1D / 16D, 0D),
    new("4i"u8, false, false, false, true, 0, 0, 0D, 4D.i()),
    new("-1.2+4.2i"u8, false, false, false, true, 0, 0, 0D, -1.2D + 4.2D.i()),
    new("073i"u8, false, false, false, true, 0, 0, 0D, 73D.i()),
    new("0i"u8, true, true, true, true, 0, 0, 0D, 0D),
    new("-1.2+0i"u8, false, false, true, true, 0, 0, -1.2D, -1.2D),
    new("-12+0i"u8, true, false, true, true, -12, 0, -12D, -12D),
    new("13+0i"u8, true, true, true, true, 13, 13, 13D, 13D),
    new("0123"u8, true, true, true, false, 83, 83, 83D, 0D),
    new("-0x0"u8, true, true, true, false, 0, 0, 0D, 0D),
    new("0xdeadbeef"u8, true, true, true, false, 0xdeadbeefL, 0xdeadbeefU, 3735928559D, 0D),
    new(@"'a'"u8, true, true, true, false, (rune)'a', (rune)'a', (rune)'a', 0D),
    new(@"'\n'"u8, true, true, true, false, (rune)'\n', (rune)'\n', (rune)'\n', 0D),
    new(@"'\\'"u8, true, true, true, false, (rune)'\\', (rune)'\\', (rune)'\\', 0D),
    new(@"'\''"u8, true, true, true, false, (rune)'\'', (rune)'\'', (rune)'\'', 0D),
    new(@"'\xFF'"u8, true, true, true, false, 0xFF, 0xFF, 255D, 0D),
    new(@"'パ'"u8, true, true, true, false, 0x30d1, 0x30d1, 12497D, 0D),
    new(@"'\u30d1'"u8, true, true, true, false, 0x30d1, 0x30d1, 12497D, 0D),
    new(@"'\U000030d1'"u8, true, true, true, false, 0x30d1, 0x30d1, 12497D, 0D),
    new(text: "+-2"u8),
    new(text: "0x123."u8),
    new(text: "1e."u8),
    new(text: "0xi."u8),
    new(text: "1+2."u8),
    new(text: "'x"u8),
    new(text: "'xx'"u8),
    new(text: "'433937734937734969526500969526500'"u8),
    new("0xef"u8, true, true, true, false, 0xef, 0xef, 239D, 0D)
}.slice();

public static void TestNumberParse(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in numberTests) {
        // If fmt.Sscan thinks it's complex, it's complex. We can't trust the output
        // because imaginary comes out as a number.
        ref var c = ref heap(new complex128(), out var Ꮡc);
        global::go.text.template.parse_package.itemType typ = itemNumber;
        ж<global::go.text.template.parse_package.Tree> tree = default!;
        if (test.text[0] == (rune)'\''){
            typ = itemCharConstant;
        } else {
            var (_, errΔ1) = fmt.Sscan(test.text, Ꮡc);
            if (errΔ1 == default!) {
                typ = itemComplex;
            }
        }
        var (n, err) = tree.newNumber(0, test.text, typ);
        var ok = test.isInt || test.isUint || test.isFloat || test.isComplex;
        if (ok && err != default!) {
            Ꮡt.Errorf("unexpected error for %q: %s"u8, test.text, err);
            continue;
        }
        if (!ok && err == default!) {
            Ꮡt.Errorf("expected error for %q"u8, test.text);
            continue;
        }
        if (!ok) {
            if (debug.Value) {
                fmt.Printf("%s\n\t%s\n"u8, test.text, err);
            }
            continue;
        }
        if ((~n).IsComplex != test.isComplex) {
            Ꮡt.Errorf("complex incorrect for %q; should be %t"u8, test.text, test.isComplex);
        }
        if (test.isInt){
            if (!(~n).IsInt) {
                Ꮡt.Errorf("expected integer for %q"u8, test.text);
            }
            if ((~n).Int64 != test.int64) {
                Ꮡt.Errorf("int64 for %q should be %d Is %d"u8, test.text, test.int64, (~n).Int64);
            }
        } else 
        if ((~n).IsInt) {
            Ꮡt.Errorf("did not expect integer for %q"u8, test.text);
        }
        if (test.isUint){
            if (!(~n).IsUint) {
                Ꮡt.Errorf("expected unsigned integer for %q"u8, test.text);
            }
            if ((~n).Uint64 != test.uint64) {
                Ꮡt.Errorf("uint64 for %q should be %d Is %d"u8, test.text, test.uint64, (~n).Uint64);
            }
        } else 
        if ((~n).IsUint) {
            Ꮡt.Errorf("did not expect unsigned integer for %q"u8, test.text);
        }
        if (test.isFloat){
            if (!(~n).IsFloat) {
                Ꮡt.Errorf("expected float for %q"u8, test.text);
            }
            if ((~n).Float64 != test.float64) {
                Ꮡt.Errorf("float64 for %q should be %g Is %g"u8, test.text, test.float64, (~n).Float64);
            }
        } else 
        if ((~n).IsFloat) {
            Ꮡt.Errorf("did not expect float for %q"u8, test.text);
        }
        if (test.isComplex){
            if (!(~n).IsComplex) {
                Ꮡt.Errorf("expected complex for %q"u8, test.text);
            }
            if ((~n).Complex128 != test.complex128) {
                Ꮡt.Errorf("complex128 for %q should be %g Is %g"u8, test.text, test.complex128, (~n).Complex128);
            }
        } else 
        if ((~n).IsComplex) {
            Ꮡt.Errorf("did not expect complex for %q"u8, test.text);
        }
    }
}

[GoType] internal partial struct parseTest {
    internal @string name;
    internal @string input;
    internal bool ok;
    internal @string result; // what the user would see in an error message.
}

internal const bool noError = true;
internal const bool hasError = false;

// Trimming spaces.
// Errors.
// Other kinds of assignments and operators aren't available yet.
// Check the parse fails for := rather than comma.
// Another bug: variable read must ignore following punctuation.
// ! is just illegal here.
// $x+2 should not parse as ($x) (+2).
// It's OK with a space.
// Check the range handles assignment vs. declaration properly.
// dot following a literal value
// Wrong pipeline
// Missing pipeline in block
internal static slice<parseTest> parseTests = new parseTest[]{
    new("empty"u8, ""u8, noError,
        @""u8),
    new("comment"u8, "{{/*\n\n\n*/}}"u8, noError,
        @""u8),
    new("spaces"u8, " \t\n"u8, noError,
        @""" \t\n"""u8),
    new("text"u8, "some text"u8, noError,
        @"""some text"""u8),
    new("emptyAction"u8, "{{}}"u8, hasError,
        @"{{}}"u8),
    new("field"u8, "{{.X}}"u8, noError,
        @"{{.X}}"u8),
    new("simple command"u8, "{{printf}}"u8, noError,
        @"{{printf}}"u8),
    new("$ invocation"u8, "{{$}}"u8, noError,
        "{{$}}"u8),
    new("variable invocation"u8, "{{with $x := 3}}{{$x 23}}{{end}}"u8, noError,
        "{{with $x := 3}}{{$x 23}}{{end}}"u8),
    new("variable with fields"u8, "{{$.I}}"u8, noError,
        "{{$.I}}"u8),
    new("multi-word command"u8, "{{printf `%d` 23}}"u8, noError,
        "{{printf `%d` 23}}"u8),
    new("pipeline"u8, "{{.X|.Y}}"u8, noError,
        @"{{.X | .Y}}"u8),
    new("pipeline with decl"u8, "{{$x := .X|.Y}}"u8, noError,
        @"{{$x := .X | .Y}}"u8),
    new("nested pipeline"u8, "{{.X (.Y .Z) (.A | .B .C) (.E)}}"u8, noError,
        @"{{.X (.Y .Z) (.A | .B .C) (.E)}}"u8),
    new("field applied to parentheses"u8, "{{(.Y .Z).Field}}"u8, noError,
        @"{{(.Y .Z).Field}}"u8),
    new("simple if"u8, "{{if .X}}hello{{end}}"u8, noError,
        @"{{if .X}}""hello""{{end}}"u8),
    new("if with else"u8, "{{if .X}}true{{else}}false{{end}}"u8, noError,
        @"{{if .X}}""true""{{else}}""false""{{end}}"u8),
    new("if with else if"u8, "{{if .X}}true{{else if .Y}}false{{end}}"u8, noError,
        @"{{if .X}}""true""{{else}}{{if .Y}}""false""{{end}}{{end}}"u8),
    new("if else chain"u8, "+{{if .X}}X{{else if .Y}}Y{{else if .Z}}Z{{end}}+"u8, noError,
        @"""+""{{if .X}}""X""{{else}}{{if .Y}}""Y""{{else}}{{if .Z}}""Z""{{end}}{{end}}{{end}}""+"""u8),
    new("simple range"u8, "{{range .X}}hello{{end}}"u8, noError,
        @"{{range .X}}""hello""{{end}}"u8),
    new("chained field range"u8, "{{range .X.Y.Z}}hello{{end}}"u8, noError,
        @"{{range .X.Y.Z}}""hello""{{end}}"u8),
    new("nested range"u8, "{{range .X}}hello{{range .Y}}goodbye{{end}}{{end}}"u8, noError,
        @"{{range .X}}""hello""{{range .Y}}""goodbye""{{end}}{{end}}"u8),
    new("range with else"u8, "{{range .X}}true{{else}}false{{end}}"u8, noError,
        @"{{range .X}}""true""{{else}}""false""{{end}}"u8),
    new("range over pipeline"u8, "{{range .X|.M}}true{{else}}false{{end}}"u8, noError,
        @"{{range .X | .M}}""true""{{else}}""false""{{end}}"u8),
    new("range []int"u8, "{{range .SI}}{{.}}{{end}}"u8, noError,
        @"{{range .SI}}{{.}}{{end}}"u8),
    new("range 1 var"u8, "{{range $x := .SI}}{{.}}{{end}}"u8, noError,
        @"{{range $x := .SI}}{{.}}{{end}}"u8),
    new("range 2 vars"u8, "{{range $x, $y := .SI}}{{.}}{{end}}"u8, noError,
        @"{{range $x, $y := .SI}}{{.}}{{end}}"u8),
    new("range with break"u8, "{{range .SI}}{{.}}{{break}}{{end}}"u8, noError,
        @"{{range .SI}}{{.}}{{break}}{{end}}"u8),
    new("range with continue"u8, "{{range .SI}}{{.}}{{continue}}{{end}}"u8, noError,
        @"{{range .SI}}{{.}}{{continue}}{{end}}"u8),
    new("constants"u8, "{{range .SI 1 -3.2i true false 'a' nil}}{{end}}"u8, noError,
        @"{{range .SI 1 -3.2i true false 'a' nil}}{{end}}"u8),
    new("template"u8, "{{template `x`}}"u8, noError,
        @"{{template ""x""}}"u8),
    new("template with arg"u8, "{{template `x` .Y}}"u8, noError,
        @"{{template ""x"" .Y}}"u8),
    new("with"u8, "{{with .X}}hello{{end}}"u8, noError,
        @"{{with .X}}""hello""{{end}}"u8),
    new("with with else"u8, "{{with .X}}hello{{else}}goodbye{{end}}"u8, noError,
        @"{{with .X}}""hello""{{else}}""goodbye""{{end}}"u8),
    new("with with else with"u8, "{{with .X}}hello{{else with .Y}}goodbye{{end}}"u8, noError,
        @"{{with .X}}""hello""{{else}}{{with .Y}}""goodbye""{{end}}{{end}}"u8),
    new("with else chain"u8, "{{with .X}}X{{else with .Y}}Y{{else with .Z}}Z{{end}}"u8, noError,
        @"{{with .X}}""X""{{else}}{{with .Y}}""Y""{{else}}{{with .Z}}""Z""{{end}}{{end}}{{end}}"u8),
    new("trim left"u8, "x \r\n\t{{- 3}}"u8, noError, @"""x""{{3}}"u8),
    new("trim right"u8, "{{3 -}}\n\n\ty"u8, noError, @"{{3}}""y"""u8),
    new("trim left and right"u8, "x \r\n\t{{- 3 -}}\n\n\ty"u8, noError, @"""x""{{3}}""y"""u8),
    new("trim with extra spaces"u8, "x\n{{-  3   -}}\ny"u8, noError, @"""x""{{3}}""y"""u8),
    new("comment trim left"u8, "x \r\n\t{{- /* hi */}}"u8, noError, @"""x"""u8),
    new("comment trim right"u8, "{{/* hi */ -}}\n\n\ty"u8, noError, @"""y"""u8),
    new("comment trim left and right"u8, "x \r\n\t{{- /* */ -}}\n\n\ty"u8, noError, @"""x""""y"""u8),
    new("block definition"u8, @"{{block ""foo"" .}}hello{{end}}"u8, noError,
        @"{{template ""foo"" .}}"u8),
    new("newline in assignment"u8, "{{ $x \n := \n 1 \n }}"u8, noError, "{{$x := 1}}"u8),
    new("newline in empty action"u8, "{{\n}}"u8, hasError, "{{\n}}"u8),
    new("newline in pipeline"u8, "{{\n\"x\"\n|\nprintf\n}}"u8, noError, @"{{""x"" | printf}}"u8),
    new("newline in comment"u8, "{{/*\nhello\n*/}}"u8, noError, ""u8),
    new("newline in comment"u8, "{{-\n/*\nhello\n*/\n-}}"u8, noError, ""u8),
    new("spaces around continue"u8, "{{range .SI}}{{.}}{{ continue }}{{end}}"u8, noError,
        @"{{range .SI}}{{.}}{{continue}}{{end}}"u8),
    new("spaces around break"u8, "{{range .SI}}{{.}}{{ break }}{{end}}"u8, noError,
        @"{{range .SI}}{{.}}{{break}}{{end}}"u8),
    new("unclosed action"u8, "hello{{range"u8, hasError, ""u8),
    new("unmatched end"u8, "{{end}}"u8, hasError, ""u8),
    new("unmatched else"u8, "{{else}}"u8, hasError, ""u8),
    new("unmatched else after if"u8, "{{if .X}}hello{{end}}{{else}}"u8, hasError, ""u8),
    new("multiple else"u8, "{{if .X}}1{{else}}2{{else}}3{{end}}"u8, hasError, ""u8),
    new("missing end"u8, "hello{{range .x}}"u8, hasError, ""u8),
    new("missing end after else"u8, "hello{{range .x}}{{else}}"u8, hasError, ""u8),
    new("undefined function"u8, "hello{{undefined}}"u8, hasError, ""u8),
    new("undefined variable"u8, "{{$x}}"u8, hasError, ""u8),
    new("variable undefined after end"u8, "{{with $x := 4}}{{end}}{{$x}}"u8, hasError, ""u8),
    new("variable undefined in template"u8, "{{template $v}}"u8, hasError, ""u8),
    new("declare with field"u8, "{{with $x.Y := 4}}{{end}}"u8, hasError, ""u8),
    new("template with field ref"u8, "{{template .X}}"u8, hasError, ""u8),
    new("template with var"u8, "{{template $v}}"u8, hasError, ""u8),
    new("invalid punctuation"u8, "{{printf 3, 4}}"u8, hasError, ""u8),
    new("multidecl outside range"u8, "{{with $v, $u := 3}}{{end}}"u8, hasError, ""u8),
    new("too many decls in range"u8, "{{range $u, $v, $w := 3}}{{end}}"u8, hasError, ""u8),
    new("dot applied to parentheses"u8, "{{printf (printf .).}}"u8, hasError, ""u8),
    new("adjacent args"u8, "{{printf 3`x`}}"u8, hasError, ""u8),
    new("adjacent args with ."u8, "{{printf `x`.}}"u8, hasError, ""u8),
    new("extra end after if"u8, "{{if .X}}a{{else if .Y}}b{{end}}{{end}}"u8, hasError, ""u8),
    new("break outside range"u8, "{{range .}}{{end}} {{break}}"u8, hasError, ""u8),
    new("continue outside range"u8, "{{range .}}{{end}} {{continue}}"u8, hasError, ""u8),
    new("break in range else"u8, "{{range .}}{{else}}{{break}}{{end}}"u8, hasError, ""u8),
    new("continue in range else"u8, "{{range .}}{{else}}{{continue}}{{end}}"u8, hasError, ""u8),
    new("bug0a"u8, "{{$x := 0}}{{$x}}"u8, noError, "{{$x := 0}}{{$x}}"u8),
    new("bug0b"u8, "{{$x += 1}}{{$x}}"u8, hasError, ""u8),
    new("bug0c"u8, "{{$x ! 2}}{{$x}}"u8, hasError, ""u8),
    new("bug0d"u8, "{{$x % 3}}{{$x}}"u8, hasError, ""u8),
    new("bug0e"u8, "{{range $x := $y := 3}}{{end}}"u8, hasError, ""u8),
    new("bug1a"u8, "{{$x:=.}}{{$x!2}}"u8, hasError, ""u8),
    new("bug1b"u8, "{{$x:=.}}{{$x+2}}"u8, hasError, ""u8),
    new("bug1c"u8, "{{$x:=.}}{{$x +2}}"u8, noError, "{{$x := .}}{{$x +2}}"u8),
    new("bug2a"u8, "{{range $x := 0}}{{$x}}{{end}}"u8, noError, "{{range $x := 0}}{{$x}}{{end}}"u8),
    new("bug2b"u8, "{{range $x = 0}}{{$x}}{{end}}"u8, noError, "{{range $x = 0}}{{$x}}{{end}}"u8),
    new("dot after integer"u8, "{{1.E}}"u8, hasError, ""u8),
    new("dot after float"u8, "{{0.1.E}}"u8, hasError, ""u8),
    new("dot after boolean"u8, "{{true.E}}"u8, hasError, ""u8),
    new("dot after char"u8, "{{'a'.any}}"u8, hasError, ""u8),
    new("dot after string"u8, @"{{""hello"".guys}}"u8, hasError, ""u8),
    new("dot after dot"u8, "{{..E}}"u8, hasError, ""u8),
    new("dot after nil"u8, "{{nil.E}}"u8, hasError, ""u8),
    new("wrong pipeline dot"u8, "{{12|.}}"u8, hasError, ""u8),
    new("wrong pipeline number"u8, "{{.|12|printf}}"u8, hasError, ""u8),
    new("wrong pipeline string"u8, "{{.|printf|\"error\"}}"u8, hasError, ""u8),
    new("wrong pipeline char"u8, "{{12|printf|'e'}}"u8, hasError, ""u8),
    new("wrong pipeline boolean"u8, "{{.|true}}"u8, hasError, ""u8),
    new("wrong pipeline nil"u8, "{{'c'|nil}}"u8, hasError, ""u8),
    new("empty pipeline"u8, @"{{printf ""%d"" ( ) }}"u8, hasError, ""u8),
    new("block definition"u8, @"{{block ""foo""}}hello{{end}}"u8, hasError, ""u8)
}.slice();

internal static map<@string, any> builtins = new map<@string, any>{
    ["printf"u8] = ((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintf)),
    ["contains"u8] = strings.Contains
};

internal static void testParse(bool doCopy, ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        textFormat = "%q"u8;
        defer(() => {
            textFormat = "%s"u8;
        }, ref ᒐ);
        foreach (var (_, test) in parseTests) {
            var (tmpl, err) = New(test.name).Parse(test.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
            switch (ᐧ) {
            case {} when err == default! && !test.ok: {
                Ꮡt.Errorf("%q: expected error; got none"u8, test.name);
                continue;
                break;
            }
            case {} when err != default! && test.ok: {
                Ꮡt.Errorf("%q: unexpected error: %v"u8, test.name, err);
                continue;
                break;
            }
            case {} when err != default! && !test.ok: {
                if (debug.Value) {
                    // expected error, got one
                    fmt.Printf("%s: %s\n\t%s\n"u8, test.name, test.input, err);
                }
                continue;
                break;
            }}

            @string result = default!;
            if (doCopy){
                result = (~tmpl).Root.Copy().String();
            } else {
                result = (~tmpl).Root.String();
            }
            if (result != test.result) {
                Ꮡt.Errorf("%s=(%q): got\n\t%v\nexpected\n\t%v"u8, test.name, test.input, result, test.result);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestParse(ж<testing.T> Ꮡt) {
    testParse(false, Ꮡt);
}

// Same as TestParse, but we copy the node first
public static void TestParseCopy(ж<testing.T> Ꮡt) {
    testParse(true, Ꮡt);
}

public static void TestParseWithComments(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        textFormat = "%q"u8;
        defer(() => {
            textFormat = "%s"u8;
        }, ref ᒐ);
        var tests = new parseTest[]{
            new("comment"u8, "{{/*\n\n\n*/}}"u8, noError, "{{/*\n\n\n*/}}"u8),
            new("comment trim left"u8, "x \r\n\t{{- /* hi */}}"u8, noError, @"""x""{{/* hi */}}"u8),
            new("comment trim right"u8, "{{/* hi */ -}}\n\n\ty"u8, noError, @"{{/* hi */}}""y"""u8),
            new("comment trim left and right"u8, "x \r\n\t{{- /* */ -}}\n\n\ty"u8, noError, @"""x""{{/* */}}""y"""u8)
        }.array();
        foreach (var (_, vᴛ1) in tests) {
            ref var test = ref heap(new parseTest(), out var Ꮡtest);
            test = vᴛ1;

            var testʗ1 = test;
            Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
                var tr = New(testʗ1.name);
                tr.Value.Mode = ParseComments;
                var (tmpl, err) = tr.Parse(testʗ1.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>());
                if (err != default!) {
                    tΔ1.Errorf("%q: expected error; got none"u8, testʗ1.name);
                }
                {
                    @string result = (~tmpl).Root.String(); if (result != testʗ1.result) {
                        tΔ1.Errorf("%s=(%q): got\n\t%v\nexpected\n\t%v"u8, testʗ1.name, testʗ1.input, result, testʗ1.result);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangeXBreak20Endˢ = @"{{range .X}}{{break 20}}{{end}}"u8;

public static void TestKeywordsAndFuncs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Check collisions between functions and new keywords like 'break'. When a
        // break function is provided, the parser should treat 'break' as a function,
        // not a keyword.
        textFormat = "%q"u8;
        defer(() => {
            textFormat = "%s"u8;
        }, ref ᒐ);
        @string inp = rangeXBreak20Endˢ;
        {
            // 'break' is a defined function, don't treat it as a keyword: it should
            // accept an argument successfully.
            map<@string, any> funcsWithKeywordFunc = new map<@string, any>{
                ["break"u8] = any (any @in) => @in
            };
            var (tmpl, err) = New(""u8).Parse(inp, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), funcsWithKeywordFunc);
            if (err != default! || tmpl == nil) {
                Ꮡt.Errorf("with break func: unexpected error: %v"u8, err);
            }
        }
        {
            // No function called 'break'; treat it as a keyword. Results in a parse
            // error.
            var (tmpl, err) = New(""u8).Parse(inp, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), new map<@string, any>());
            if (err == default! || tmpl != nil) {
                Ꮡt.Errorf("without break func: expected error; got none"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string skipFuncCheckˢ = "skip func check"u8;
internal static readonly @string fn12ˢ = "{{fn 1 2}}"u8;

public static void TestSkipFuncCheck(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string oldTextFormat = textFormat;
        textFormat = "%q"u8;
        defer(() => {
            textFormat = oldTextFormat;
        }, ref ᒐ);
        var tr = New(skipFuncCheckˢ);
        tr.Value.Mode = SkipFuncCheck;
        var (tmpl, err) = tr.Parse(fn12ˢ, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>());
        if (err != default!) {
            Ꮡt.Fatalf("unexpected error: %v"u8, err);
        }
        @string expected = fn12ˢ;
        {
            @string result = (~tmpl).Root.String(); if (result != expected) {
                Ꮡt.Errorf("got\n\t%v\nexpected\n\t%v"u8, result, expected);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct isEmptyTest {
    internal @string name;
    internal @string input;
    internal bool empty;
}

internal static slice<isEmptyTest> isEmptyTests = new isEmptyTest[]{
    new("empty"u8, @""u8, true),
    new("nonempty"u8, @"hello"u8, false),
    new("spaces only"u8, " \t\n \t\n"u8, true),
    new("comment only"u8, "{{/* comment */}}"u8, true),
    new("definition"u8, @"{{define ""x""}}something{{end}}"u8, true),
    new("definitions and space"u8, "{{define `x`}}something{{end}}\n\n{{define `y`}}something{{end}}\n\n"u8, true),
    new("definitions and text"u8, "{{define `x`}}something{{end}}\nx\n{{define `y`}}something{{end}}\ny\n"u8, false),
    new("definition and action"u8, "{{define `x`}}something{{end}}{{if 3}}foo{{end}}"u8, false)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootˢ = "root"u8;

public static void TestIsEmpty(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (!IsEmptyTree(default!)) {
        Ꮡt.Errorf("nil tree is not empty"u8);
    }
    foreach (var (_, test) in isEmptyTests) {
        var (tree, err) = New(rootˢ).Parse(test.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), (map<@string, any>)(default!));
        if (err != default!) {
            Ꮡt.Errorf("%q: unexpected error: %v"u8, test.name, err);
            continue;
        }
        {
            var empty = IsEmptyTree(new global::go.text.template.parse_package.ListNodeжNode((~tree).Root)); if (empty != test.empty) {
                Ꮡt.Errorf("%q: expected %t got %t"u8, test.name, test.empty, empty);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ifTrueEndˢ = "{{if true}}{{end}}"u8;

public static void TestErrorContextWithTreeCopy(ж<testing.T> Ꮡt) {
    var (tree, err) = New(rootˢ).Parse(ifTrueEndˢ, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), (map<@string, any>)(default!));
    if (err != default!) {
        Ꮡt.Fatalf("unexpected tree parse failure: %v"u8, err);
    }
    var treeCopy = tree.Copy();
    var (wantLocation, wantContext) = tree.ErrorContext((~(~tree).Root).Nodes[0]);
    var (gotLocation, gotContext) = treeCopy.ErrorContext((~(~treeCopy).Root).Nodes[0]);
    if (wantLocation != gotLocation) {
        Ꮡt.Errorf("wrong error location want %q got %q"u8, wantLocation, gotLocation);
    }
    if (wantContext != gotContext) {
        Ꮡt.Errorf("wrong error location want %q got %q"u8, wantContext, gotContext);
    }
}

// Check line numbers are accurate.
// Specific errors.
// Declare $x so it's defined, to avoid that error, and then check we don't parse a declaration.
// All failures, and the result is a string that must appear in the error message.
internal static slice<parseTest> errorTests = new parseTest[]{
    new("unclosed1"u8,
        "line1\n{{"u8,
        hasError, @"unclosed1:2: unclosed action"u8),
    new("unclosed2"u8,
        "line1\n{{define `x`}}line2\n{{"u8,
        hasError, @"unclosed2:3: unclosed action"u8),
    new("unclosed3"u8,
        "line1\n{{\"x\"\n\"y\"\n"u8,
        hasError, @"unclosed3:4: unclosed action started at unclosed3:2"u8),
    new("unclosed4"u8,
        "{{\n\n\n\n\n"u8,
        hasError, @"unclosed4:6: unclosed action started at unclosed4:1"u8),
    new("var1"u8,
        "line1\n{{\nx\n}}"u8,
        hasError, @"var1:3: function ""x"" not defined"u8),
    new("function"u8,
        "{{foo}}"u8,
        hasError, @"function ""foo"" not defined"u8),
    new("comment1"u8,
        "{{/*}}"u8,
        hasError, @"comment1:1: unclosed comment"u8),
    new("comment2"u8,
        "{{/*\nhello\n}}"u8,
        hasError, @"comment2:1: unclosed comment"u8),
    new("lparen"u8,
        "{{.X (1 2 3}}"u8,
        hasError, @"unclosed left paren"u8),
    new("rparen"u8,
        "{{.X 1 2 3 ) }}"u8,
        hasError, "unexpected right paren"u8),
    new("rparen2"u8,
        "{{(.X 1 2 3"u8,
        hasError, @"unclosed action"u8),
    new("space"u8,
        "{{`x`3}}"u8,
        hasError, @"in operand"u8),
    new("idchar"u8,
        "{{a#}}"u8,
        hasError, @"'#'"u8),
    new("charconst"u8,
        "{{'a}}"u8,
        hasError, @"unterminated character constant"u8),
    new("stringconst"u8,
        @"{{""a}}"u8,
        hasError, @"unterminated quoted string"u8),
    new("rawstringconst"u8,
        "{{`a}}"u8,
        hasError, @"unterminated raw quoted string"u8),
    new("number"u8,
        "{{0xi}}"u8,
        hasError, @"number syntax"u8),
    new("multidefine"u8,
        "{{define `a`}}a{{end}}{{define `a`}}b{{end}}"u8,
        hasError, @"multiple definition of template"u8),
    new("eof"u8,
        "{{range .X}}"u8,
        hasError, @"unexpected EOF"u8),
    new("variable"u8,
        "{{$x := 23}}{{with $x.y := 3}}{{$x 23}}{{end}}"u8,
        hasError, @"unexpected "":="""u8),
    new("multidecl"u8,
        "{{$a,$b,$c := 23}}"u8,
        hasError, @"too many declarations"u8),
    new("undefvar"u8,
        "{{$a}}"u8,
        hasError, @"undefined variable"u8),
    new("wrongdot"u8,
        "{{true.any}}"u8,
        hasError, @"unexpected . after term"u8),
    new("wrongpipeline"u8,
        "{{12|false}}"u8,
        hasError, @"non executable command in pipeline"u8),
    new("emptypipeline"u8,
        @"{{ ( ) }}"u8,
        hasError, @"missing value for parenthesized pipeline"u8),
    new("multilinerawstring"u8,
        "{{ $v := `\n` }} {{"u8,
        hasError, @"multilinerawstring:2: unclosed action"u8),
    new("rangeundefvar"u8,
        "{{range $k}}{{end}}"u8,
        hasError, @"undefined variable"u8),
    new("rangeundefvars"u8,
        "{{range $k, $v}}{{end}}"u8,
        hasError, @"undefined variable"u8),
    new("rangemissingvalue1"u8,
        "{{range $k,}}{{end}}"u8,
        hasError, @"missing value for range"u8),
    new("rangemissingvalue2"u8,
        "{{range $k, $v := }}{{end}}"u8,
        hasError, @"missing value for range"u8),
    new("rangenotvariable1"u8,
        "{{range $k, .}}{{end}}"u8,
        hasError, @"range can only initialize variables"u8),
    new("rangenotvariable2"u8,
        "{{range $k, 123 := .}}{{end}}"u8,
        hasError, @"range can only initialize variables"u8)
}.slice();

public static void TestErrors(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in errorTests) {
        ref var test = ref heap(new parseTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var (_, err) = New(testʗ1.name).Parse(testʗ1.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>());
            if (err == default!) {
                tΔ1.Fatalf("expected error %q, got nil"u8, testʗ1.result);
            }
            if (!strings.Contains(err.Error(), testʗ1.result)) {
                tΔ1.Fatalf("error %q does not contain %q"u8, err, testʗ1.result);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string outerˢ = "outer"u8;
internal static readonly @string innerˢ = "inner"u8;
internal static readonly object blockDidNotDefineˢ = (@string)"block did not define template"u8;

public static void TestBlock(ж<testing.T> Ꮡt) {
    @string input = @"a{{block ""inner"" .}}bar{{.}}baz{{end}}b"u8;
    @string outer = @"a{{template ""inner"" .}}b"u8;
    @string inner = @"bar{{.}}baz"u8;
    var treeSet = new map<@string, ж<global::go.text.template.parse_package.Tree>>();
    var (tmpl, err) = New(outerˢ).Parse(input, ""u8, ""u8, treeSet, (map<@string, any>)(default!));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string g = (~tmpl).Root.String();
        @string w = outer; if (g != w) {
            Ꮡt.Errorf("outer template = %q, want %q"u8, g, w);
        }
    }
    var inTmpl = treeSet[innerˢ];
    if (inTmpl == nil) {
        Ꮡt.Fatal(blockDidNotDefineˢ);
    }
    {
        @string g = (~inTmpl).Root.String();
        @string w = inner; if (g != w) {
            Ꮡt.Errorf("inner template = %q, want %q"u8, g, w);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string printf1234ˢ = "{{printf 1234}}\n"u8;
internal static readonly @string benchˢ = "bench"u8;

public static void TestLineNum(ж<testing.T> Ꮡt) {
    // const count = 100
    const nint count = 3;
    @string text = strings.Repeat(printf1234ˢ, count);
    var (tree, err) = New(benchˢ).Parse(text, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Check the line numbers. Each line is an action containing a template, followed by text.
    // That's two nodes per line.
    var nodes = tree.Value.Root.Value.Nodes;
    for (nint i = 0; i < len(nodes); i += 2) {
        nint line = 1 + i / 2;
        // Action first.
        var action = nodes[i]._<ж<global::go.text.template.parse_package.ActionNode>>();
        if ((~action).Line != line) {
            Ꮡt.Errorf("line %d: action is line %d"u8, line, (~action).Line);
        }
        var pipe = action.Value.Pipe;
        if ((~pipe).Line != line) {
            Ꮡt.Errorf("line %d: pipe is line %d"u8, line, (~pipe).Line);
        }
    }
}

public static void BenchmarkParseLarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string text = strings.Repeat("{{1234}}\n"u8, 10000);
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = New(benchˢ).Parse(text, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
}

internal static @string sinkv;
internal static @string sinkl;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object benchmarkWasNotRunˢ = (@string)"Benchmark was not run"u8;

public static void BenchmarkVariableString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var v = Ꮡ(new VariableNode(
        Ident: new @string[]{"$"u8, "A"u8, "BB"u8, "CCC"u8, "THIS_IS_THE_VARIABLE_BEING_PROCESSED"u8}.slice()
    ));
    b.ResetTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        sinkv = v.String();
    }
    if (sinkv == ""u8) {
        Ꮡb.Fatal(benchmarkWasNotRunˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string printfField1Field2Field3ˢ = """

{{(printf .Field1.Field2.Field3).Value}}
{{$x := (printf .Field1.Field2.Field3).Value}}
{{$y := (printf $x.Field1.Field2.Field3).Value}}
{{$z := $y.Field1.Field2.Field3}}
{{if contains $y $z}}
	{{printf "%q" $y}}
{{else}}
	{{printf "%q" $x}}
{{end}}
{{with $z.Field1 | contains "boring"}}
	{{printf "%q" . | printf "%s"}}
{{else}}
	{{printf "%d %d %d" 11 11 11}}
	{{printf "%d %d %s" 22 22 $x.Field1.Field2.Field3 | printf "%s"}}
	{{printf "%v" (contains $z.Field1.Field2 $y)}}
{{end}}

"""u8;

public static void BenchmarkListString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string text = printfField1Field2Field3ˢ;
    var (tree, err) = New(benchˢ).Parse(text, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    b.ResetTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        sinkl = (~tree).Root.String();
    }
    if (sinkl == ""u8) {
        Ꮡb.Fatal(benchmarkWasNotRunˢ);
    }
}

} // end parse_internal_test_package
