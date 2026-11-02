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

internal static ж<bool> debug = flag.Bool("debug", false, "show the errors produced by the main tests");

internal partial struct numberTest {
    internal @string text;
    internal bool isInt;
    internal bool isUint;
    internal bool isFloat;
    internal bool isComplex;
    /*embed*/ internal int64 int64;
    /*embed*/ internal uint64 uint64;
    /*embed*/ internal float64 float64;
    /*embed*/ internal complex128 complex128;
}

internal static slice<numberTest> numberTests = slice<numberTest>([
    // basics
    new("0", true, true, true, false, 0, 0, 0D, 0D),
    new("-0", true, true, true, false, 0, 0, 0D, 0D), // check that -0 is a uint.
    new("73", true, true, true, false, 73, 73, 73D, 0D),
    new("7_3", true, true, true, false, 73, 73, 73D, 0D),
    new("0b10_010_01", true, true, true, false, 73, 73, 73D, 0D),
    new("0B10_010_01", true, true, true, false, 73, 73, 73D, 0D),
    new("073", true, true, true, false, 59, 59, 59D, 0D),
    new("0o73", true, true, true, false, 59, 59, 59D, 0D),
    new("0O73", true, true, true, false, 59, 59, 59D, 0D),
    new("0x73", true, true, true, false, 0x73, 0x73, 115D, 0D),
    new("0X73", true, true, true, false, 0x73, 0x73, 115D, 0D),
    new("0x7_3", true, true, true, false, 0x73, 0x73, 115D, 0D),
    new("-73", true, false, true, false, -73, 0, -73D, 0D),
    new("+73", true, false, true, false, 73, 0, 73D, 0D),
    new("100", true, true, true, false, 100, 100, 100D, 0D),
    new("1e9", true, true, true, false, 1000000000, 1000000000, 1e9D, 0D),
    new("-1e9", true, false, true, false, -1000000000, 0, -1e9D, 0D),
    new("-1.2", false, false, true, false, 0, 0, -1.2D, 0D),
    new("1e19", false, true, true, false, 0, 10000000000000000000, 1e19D, 0D),
    new("1e1_9", false, true, true, false, 0, 10000000000000000000, 1e19D, 0D),
    new("1E19", false, true, true, false, 0, 10000000000000000000, 1e19D, 0D),
    new("-1e19", false, false, true, false, 0, 0, -1e19D, 0D),
    new("0x_1p4", true, true, true, false, 16, 16, 16D, 0D),
    new("0X_1P4", true, true, true, false, 16, 16, 16D, 0D),
    new("0x_1p-4", false, false, true, false, 0, 0, 1D / 16D, 0D),
    new("4i", false, false, false, true, 0, 0, 0D, 4D.i()),
    new("-1.2+4.2i", false, false, false, true, 0, 0, 0D, -1.2D + 4.2D.i()),
    new("073i", false, false, false, true, 0, 0, 0D, 73D.i()), // not octal!
    // complex with 0 imaginary are float (and maybe integer)
    new("0i", true, true, true, true, 0, 0, 0D, 0D),
    new("-1.2+0i", false, false, true, true, 0, 0, -1.2D, -1.2D),
    new("-12+0i", true, false, true, true, -12, 0, -12D, -12D),
    new("13+0i", true, true, true, true, 13, 13, 13D, 13D),
    // funny bases
    new("0123", true, true, true, false, 83, 83, 83D, 0D),
    new("-0x0", true, true, true, false, 0, 0, 0D, 0D),
    new("0xdeadbeef", true, true, true, false, 0xdeadbeefL, 0xdeadbeefU, 3735928559D, 0D),
    // character constants
    new(@"'a'", true, true, true, false, 'a', 'a', 'a', 0D),
    new(@"'\n'", true, true, true, false, '\n', '\n', '\n', 0D),
    new(@"'\\'", true, true, true, false, '\\', '\\', '\\', 0D),
    new(@"'\''", true, true, true, false, '\'', '\'', '\'', 0D),
    new(@"'\xFF'", true, true, true, false, 0xFF, 0xFF, 255D, 0D),
    new(@"'パ'", true, true, true, false, 0x30d1, 0x30d1, 12497D, 0D),
    new(@"'\u30d1'", true, true, true, false, 0x30d1, 0x30d1, 12497D, 0D),
    new(@"'\U000030d1'", true, true, true, false, 0x30d1, 0x30d1, 12497D, 0D),
    // some broken syntax
    new(text: "+-2"),
    new(text: "0x123."),
    new(text: "1e."),
    new(text: "0xi."),
    new(text: "1+2."),
    new(text: "'x"),
    new(text: "'xx'"),
    new(text: "'433937734937734969526500969526500'"), // Integer too large - issue 10634.
    // Issue 8622 - 0xe parsed as floating point. Very embarrassing.
    new("0xef", true, true, true, false, 0xef, 0xef, 239D, 0D)
]);

[GoInit] internal static void init() {
    // Use a small stack limit for testing to avoid creating huge expressions.
    maxStackDepth = 3;
}

public static void TestNumberParse(ж<testing.T> t) {
    foreach (var (_, test) in numberTests) {
        // If fmt.Sscan thinks it's complex, it's complex. We can't trust the output
        // because imaginary comes out as a number.
        ref var c = ref heap(new complex128(), out var Ꮡc);
        global::go.text.template.parse_package.itemType typ = itemNumber;
        ж<global::go.text.template.parse_package.Tree> tree = default;
        if (test.text[0] == '\'') {
            typ = itemCharConstant;
        } else {
            var (_, errΔ1) = fmt.Sscan(test.text, Ꮡc);
            if (errΔ1 == nil) {
                typ = itemComplex;
            }
        }
        var (n, err) = tree.newNumber(0, test.text, typ);
        var ok = test.isInt || test.isUint || test.isFloat || test.isComplex;
        if (ok && err != nil) {
            t.Errorf("unexpected error for %q: %s", test.text, err);
            continue;
        }
        if (!ok && err == nil) {
            t.Errorf("expected error for %q", test.text);
            continue;
        }
        if (!ok) {
            if (debug.Value) {
                fmt.Printf("%s\n\t%s\n"u8, test.text, err);
            }
            continue;
        }
        if (n.IsComplex != test.isComplex) {
            t.Errorf("complex incorrect for %q; should be %t", test.text, test.isComplex);
        }
        if (test.isInt) {
            if (!n.IsInt) {
                t.Errorf("expected integer for %q", test.text);
            }
            if (n.Int64 != test.int64) {
                t.Errorf("int64 for %q should be %d Is %d", test.text, test.int64, n.Int64);
            }
        } else if (n.IsInt) {
            t.Errorf("did not expect integer for %q", test.text);
        }
        if (test.isUint) {
            if (!n.IsUint) {
                t.Errorf("expected unsigned integer for %q", test.text);
            }
            if (n.Uint64 != test.uint64) {
                t.Errorf("uint64 for %q should be %d Is %d", test.text, test.uint64, n.Uint64);
            }
        } else if (n.IsUint) {
            t.Errorf("did not expect unsigned integer for %q", test.text);
        }
        if (test.isFloat) {
            if (!n.IsFloat) {
                t.Errorf("expected float for %q", test.text);
            }
            if (n.Float64 != test.float64) {
                t.Errorf("float64 for %q should be %g Is %g", test.text, test.float64, n.Float64);
            }
        } else if (n.IsFloat) {
            t.Errorf("did not expect float for %q", test.text);
        }
        if (test.isComplex) {
            if (!n.IsComplex) {
                t.Errorf("expected complex for %q", test.text);
            }
            if (n.Complex128 != test.complex128) {
                t.Errorf("complex128 for %q should be %g Is %g", test.text, test.complex128, n.Complex128);
            }
        } else if (n.IsComplex) {
            t.Errorf("did not expect complex for %q", test.text);
        }
    }
}

internal partial struct parseTest {
    internal @string name;
    internal @string input;
    internal bool ok;
    internal @string result; // what the user would see in an error message.
}

internal const bool noError = true;
internal const bool hasError = false;

internal static slice<parseTest> parseTests = slice<parseTest>([
    new("empty", "", noError,
        @""),
    new("comment", "{{/*\n\n\n*/}}", noError,
        @""),
    new("spaces", " \t\n", noError,
        @""" \t\n"""),
    new("text", "some text", noError,
        @"""some text"""),
    new("emptyAction", "{{}}", hasError,
        @"{{}}"),
    new("field", "{{.X}}", noError,
        @"{{.X}}"),
    new("simple command", "{{printf}}", noError,
        @"{{printf}}"),
    new("$ invocation", "{{$}}", noError,
        "{{$}}"),
    new("variable invocation", "{{with $x := 3}}{{$x 23}}{{end}}", noError,
        "{{with $x := 3}}{{$x 23}}{{end}}"),
    new("variable with fields", "{{$.I}}", noError,
        "{{$.I}}"),
    new("multi-word command", "{{printf `%d` 23}}", noError,
        "{{printf `%d` 23}}"),
    new("pipeline", "{{.X|.Y}}", noError,
        @"{{.X | .Y}}"),
    new("pipeline with decl", "{{$x := .X|.Y}}", noError,
        @"{{$x := .X | .Y}}"),
    new("nested pipeline", "{{.X (.Y .Z) (.A | .B .C) (.E)}}", noError,
        @"{{.X (.Y .Z) (.A | .B .C) (.E)}}"),
    new("field applied to parentheses", "{{(.Y .Z).Field}}", noError,
        @"{{(.Y .Z).Field}}"),
    new("simple if", "{{if .X}}hello{{end}}", noError,
        @"{{if .X}}""hello""{{end}}"),
    new("if with else", "{{if .X}}true{{else}}false{{end}}", noError,
        @"{{if .X}}""true""{{else}}""false""{{end}}"),
    new("if with else if", "{{if .X}}true{{else if .Y}}false{{end}}", noError,
        @"{{if .X}}""true""{{else}}{{if .Y}}""false""{{end}}{{end}}"),
    new("if else chain", "+{{if .X}}X{{else if .Y}}Y{{else if .Z}}Z{{end}}+", noError,
        @"""+""{{if .X}}""X""{{else}}{{if .Y}}""Y""{{else}}{{if .Z}}""Z""{{end}}{{end}}{{end}}""+"""),
    new("simple range", "{{range .X}}hello{{end}}", noError,
        @"{{range .X}}""hello""{{end}}"),
    new("chained field range", "{{range .X.Y.Z}}hello{{end}}", noError,
        @"{{range .X.Y.Z}}""hello""{{end}}"),
    new("nested range", "{{range .X}}hello{{range .Y}}goodbye{{end}}{{end}}", noError,
        @"{{range .X}}""hello""{{range .Y}}""goodbye""{{end}}{{end}}"),
    new("range with else", "{{range .X}}true{{else}}false{{end}}", noError,
        @"{{range .X}}""true""{{else}}""false""{{end}}"),
    new("range over pipeline", "{{range .X|.M}}true{{else}}false{{end}}", noError,
        @"{{range .X | .M}}""true""{{else}}""false""{{end}}"),
    new("range []int", "{{range .SI}}{{.}}{{end}}", noError,
        @"{{range .SI}}{{.}}{{end}}"),
    new("range 1 var", "{{range $x := .SI}}{{.}}{{end}}", noError,
        @"{{range $x := .SI}}{{.}}{{end}}"),
    new("range 2 vars", "{{range $x, $y := .SI}}{{.}}{{end}}", noError,
        @"{{range $x, $y := .SI}}{{.}}{{end}}"),
    new("range with break", "{{range .SI}}{{.}}{{break}}{{end}}", noError,
        @"{{range .SI}}{{.}}{{break}}{{end}}"),
    new("range with continue", "{{range .SI}}{{.}}{{continue}}{{end}}", noError,
        @"{{range .SI}}{{.}}{{continue}}{{end}}"),
    new("constants", "{{range .SI 1 -3.2i true false 'a' nil}}{{end}}", noError,
        @"{{range .SI 1 -3.2i true false 'a' nil}}{{end}}"),
    new("template", "{{template `x`}}", noError,
        @"{{template ""x""}}"),
    new("template with arg", "{{template `x` .Y}}", noError,
        @"{{template ""x"" .Y}}"),
    new("with", "{{with .X}}hello{{end}}", noError,
        @"{{with .X}}""hello""{{end}}"),
    new("with with else", "{{with .X}}hello{{else}}goodbye{{end}}", noError,
        @"{{with .X}}""hello""{{else}}""goodbye""{{end}}"),
    new("with with else with", "{{with .X}}hello{{else with .Y}}goodbye{{end}}", noError,
        @"{{with .X}}""hello""{{else}}{{with .Y}}""goodbye""{{end}}{{end}}"),
    new("with else chain", "{{with .X}}X{{else with .Y}}Y{{else with .Z}}Z{{end}}", noError,
        @"{{with .X}}""X""{{else}}{{with .Y}}""Y""{{else}}{{with .Z}}""Z""{{end}}{{end}}{{end}}"),
    // Trimming spaces.
    new("trim left", "x \r\n\t{{- 3}}", noError, @"""x""{{3}}"),
    new("trim right", "{{3 -}}\n\n\ty", noError, @"{{3}}""y"""),
    new("trim left and right", "x \r\n\t{{- 3 -}}\n\n\ty", noError, @"""x""{{3}}""y"""),
    new("trim with extra spaces", "x\n{{-  3   -}}\ny", noError, @"""x""{{3}}""y"""),
    new("comment trim left", "x \r\n\t{{- /* hi */}}", noError, @"""x"""),
    new("comment trim right", "{{/* hi */ -}}\n\n\ty", noError, @"""y"""),
    new("comment trim left and right", "x \r\n\t{{- /* */ -}}\n\n\ty", noError, @"""x""""y"""),
    new("block definition", @"{{block ""foo"" .}}hello{{end}}", noError,
        @"{{template ""foo"" .}}"),

    new("newline in assignment", "{{ $x \n := \n 1 \n }}", noError, "{{$x := 1}}"),
    new("newline in empty action", "{{\n}}", hasError, "{{\n}}"),
    new("newline in pipeline", "{{\n\"x\"\n|\nprintf\n}}", noError, @"{{""x"" | printf}}"),
    new("newline in comment", "{{/*\nhello\n*/}}", noError, ""),
    new("newline in comment", "{{-\n/*\nhello\n*/\n-}}", noError, ""),
    new("spaces around continue", "{{range .SI}}{{.}}{{ continue }}{{end}}", noError,
        @"{{range .SI}}{{.}}{{continue}}{{end}}"),
    new("spaces around break", "{{range .SI}}{{.}}{{ break }}{{end}}", noError,
        @"{{range .SI}}{{.}}{{break}}{{end}}"),

    // Errors.
    new("unclosed action", "hello{{range", hasError, ""),
    new("unmatched end", "{{end}}", hasError, ""),
    new("unmatched else", "{{else}}", hasError, ""),
    new("unmatched else after if", "{{if .X}}hello{{end}}{{else}}", hasError, ""),
    new("multiple else", "{{if .X}}1{{else}}2{{else}}3{{end}}", hasError, ""),
    new("missing end", "hello{{range .x}}", hasError, ""),
    new("missing end after else", "hello{{range .x}}{{else}}", hasError, ""),
    new("undefined function", "hello{{undefined}}", hasError, ""),
    new("undefined variable", "{{$x}}", hasError, ""),
    new("variable undefined after end", "{{with $x := 4}}{{end}}{{$x}}", hasError, ""),
    new("variable undefined in template", "{{template $v}}", hasError, ""),
    new("declare with field", "{{with $x.Y := 4}}{{end}}", hasError, ""),
    new("template with field ref", "{{template .X}}", hasError, ""),
    new("template with var", "{{template $v}}", hasError, ""),
    new("invalid punctuation", "{{printf 3, 4}}", hasError, ""),
    new("multidecl outside range", "{{with $v, $u := 3}}{{end}}", hasError, ""),
    new("too many decls in range", "{{range $u, $v, $w := 3}}{{end}}", hasError, ""),
    new("dot applied to parentheses", "{{printf (printf .).}}", hasError, ""),
    new("adjacent args", "{{printf 3`x`}}", hasError, ""),
    new("adjacent args with .", "{{printf `x`.}}", hasError, ""),
    new("extra end after if", "{{if .X}}a{{else if .Y}}b{{end}}{{end}}", hasError, ""),
    new("break outside range", "{{range .}}{{end}} {{break}}", hasError, ""),
    new("continue outside range", "{{range .}}{{end}} {{continue}}", hasError, ""),
    new("break in range else", "{{range .}}{{else}}{{break}}{{end}}", hasError, ""),
    new("continue in range else", "{{range .}}{{else}}{{continue}}{{end}}", hasError, ""),
    // Other kinds of assignments and operators aren't available yet.
    new("bug0a", "{{$x := 0}}{{$x}}", noError, "{{$x := 0}}{{$x}}"),
    new("bug0b", "{{$x += 1}}{{$x}}", hasError, ""),
    new("bug0c", "{{$x ! 2}}{{$x}}", hasError, ""),
    new("bug0d", "{{$x % 3}}{{$x}}", hasError, ""),
    // Check the parse fails for := rather than comma.
    new("bug0e", "{{range $x := $y := 3}}{{end}}", hasError, ""),
    // Another bug: variable read must ignore following punctuation.
    new("bug1a", "{{$x:=.}}{{$x!2}}", hasError, ""),                     // ! is just illegal here.
    new("bug1b", "{{$x:=.}}{{$x+2}}", hasError, ""),                     // $x+2 should not parse as ($x) (+2).
    new("bug1c", "{{$x:=.}}{{$x +2}}", noError, "{{$x := .}}{{$x +2}}"), // It's OK with a space.
    // Check the range handles assignment vs. declaration properly.
    new("bug2a", "{{range $x := 0}}{{$x}}{{end}}", noError, "{{range $x := 0}}{{$x}}{{end}}"),
    new("bug2b", "{{range $x = 0}}{{$x}}{{end}}", noError, "{{range $x = 0}}{{$x}}{{end}}"),
    // dot following a literal value
    new("dot after integer", "{{1.E}}", hasError, ""),
    new("dot after float", "{{0.1.E}}", hasError, ""),
    new("dot after boolean", "{{true.E}}", hasError, ""),
    new("dot after char", "{{'a'.any}}", hasError, ""),
    new("dot after string", @"{{""hello"".guys}}", hasError, ""),
    new("dot after dot", "{{..E}}", hasError, ""),
    new("dot after nil", "{{nil.E}}", hasError, ""),
    // Wrong pipeline
    new("wrong pipeline dot", "{{12|.}}", hasError, ""),
    new("wrong pipeline number", "{{.|12|printf}}", hasError, ""),
    new("wrong pipeline string", "{{.|printf|\"error\"}}", hasError, ""),
    new("wrong pipeline char", "{{12|printf|'e'}}", hasError, ""),
    new("wrong pipeline boolean", "{{.|true}}", hasError, ""),
    new("wrong pipeline nil", "{{'c'|nil}}", hasError, ""),
    new("empty pipeline", @"{{printf ""%d"" ( ) }}", hasError, ""),
    // Missing pipeline in block
    new("block definition", @"{{block ""foo""}}hello{{end}}", hasError, ""),

    // Expression nested depth tests.
    new("paren nesting normal", "{{ (( 1 )) }}", noError, "{{((1))}}"),
    new("paren nesting at limit", "{{ ((( 1 ))) }}", noError, "{{(((1)))}}"),
    new("paren nesting exceeds limit", "{{ (((( 1 )))) }}", hasError, "template: test:1: max expression depth exceeded"),
    new("paren nesting in pipeline", "{{ ((( 1 ))) | printf }}", noError, "{{(((1))) | printf}}"),
    new("paren nesting in pipeline exceeds limit", "{{ (((( 1 )))) | printf }}", hasError, "template: test:1: max expression depth exceeded"),
    new("paren nesting with other constructs", "{{ if ((( true ))) }}YES{{ end }}", noError, "{{if (((true)))}}\"YES\"{{end}}"),
    new("paren nesting with other constructs exceeds limit", "{{ if (((( true )))) }}YES{{ end }}", hasError, "template: test:1: max expression depth exceeded")
]);

internal static map<@string, any> builtins = new map<@string, any>{
    ["printf"] = ((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintfᶠ)),
    ["contains"] = strings.Contains
};

internal static void testParse(bool doCopy, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        textFormat = "%q";
        defer(() => {
            textFormat = "%s";
        }, ref ᒐ);
        foreach (var (_, test) in parseTests) {
            var (tmpl, err) = New(test.name).Parse(test.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
            switch (ᐧ) {
            case {} when err == nil && !test.ok: {
                t.Errorf("%q: expected error; got none", test.name);
                continue;
                break;
            }
            case {} when err != nil && test.ok: {
                t.Errorf("%q: unexpected error: %v", test.name, err);
                continue;
                break;
            }
            case {} when err != nil && !test.ok: {
                // expected error, got one
                if (debug.Value) {
                    fmt.Printf("%s: %s\n\t%s\n"u8, test.name, test.input, err);
                }
                continue;
                break;
            }}
            @string result = default;
            if (doCopy) {
                result = tmpl.Root.Copy().String();
            } else {
                result = tmpl.Root.String();
            }
            if (result != test.result) {
                t.Errorf("%s=(%q): got\n\t%v\nexpected\n\t%v", test.name, test.input, result, test.result);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestParse(ж<testing.T> t) {
    testParse(false, t);
}

// Same as TestParse, but we copy the node first
public static void TestParseCopy(ж<testing.T> t) {
    testParse(true, t);
}

public static void TestParseWithComments(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        textFormat = "%q";
        defer(() => {
            textFormat = "%s";
        }, ref ᒐ);
        var tests = new parseTest[]{
            new("comment", "{{/*\n\n\n*/}}", noError, "{{/*\n\n\n*/}}"),
            new("comment trim left", "x \r\n\t{{- /* hi */}}", noError, @"""x""{{/* hi */}}"),
            new("comment trim right", "{{/* hi */ -}}\n\n\ty", noError, @"{{/* hi */}}""y"""),
            new("comment trim left and right", "x \r\n\t{{- /* */ -}}\n\n\ty", noError, @"""x""{{/* */}}""y""")
        }.array();
        foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
            ref var test = ref heap(new parseTest(), out var Ꮡtest);
            test = vᴛ1;

            var testʗ1 = test;
            t.Run(test.name, (ж<testing.T> tΔ1) => {
                var tr = New(testʗ1.name);
                tr.Mode = ParseComments;
                var (tmpl, err) = tr.Parse(testʗ1.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>());
                if (err != nil) {
                    tΔ1.Errorf("%q: expected error; got none", testʗ1.name);
                }
                {
                    @string result = tmpl.Root.String(); if (result != testʗ1.result) {
                        tΔ1.Errorf("%s=(%q): got\n\t%v\nexpected\n\t%v", testʗ1.name, testʗ1.input, result, testʗ1.result);
                    }
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rangeXBreak20Endˢ => ref rangeXBreak20Endˢᵇ;
private static readonly @string rangeXBreak20Endˢᵇ = @"{{range .X}}{{break 20}}{{end}}"u8;

public static void TestKeywordsAndFuncs(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Check collisions between functions and new keywords like 'break'. When a
        // break function is provided, the parser should treat 'break' as a function,
        // not a keyword.
        textFormat = "%q";
        defer(() => {
            textFormat = "%s";
        }, ref ᒐ);

        @string inp = rangeXBreak20Endˢ;
        {
            // 'break' is a defined function, don't treat it as a keyword: it should
            // accept an argument successfully.
            map<@string, any> funcsWithKeywordFunc = new map<@string, any>{
                ["break"] = any (any @in) => @in
            };
            var (tmpl, err) = New(""u8).Parse(inp, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), funcsWithKeywordFunc);
            if (err != nil || tmpl == nil) {
                t.Errorf("with break func: unexpected error: %v", err);
            }
        }

        {
            // No function called 'break'; treat it as a keyword. Results in a parse
            // error.
            var (tmpl, err) = New(""u8).Parse(inp, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), new map<@string, any>());
            if (err == nil || tmpl != nil) {
                t.Errorf("without break func: expected error; got none");
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string skipFuncCheckˢ => ref skipFuncCheckˢᵇ;
private static readonly @string skipFuncCheckˢᵇ = "skip func check"u8;
internal static ref readonly @string fn12ˢ => ref fn12ˢᵇ;
private static readonly @string fn12ˢᵇ = "{{fn 1 2}}"u8;

public static void TestSkipFuncCheck(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        @string oldTextFormat = textFormat;
        textFormat = "%q";
        defer(() => {
            textFormat = oldTextFormat;
        }, ref ᒐ);
        var tr = New(skipFuncCheckˢ);
        tr.Mode = SkipFuncCheck;
        var (tmpl, err) = tr.Parse(fn12ˢ, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>());
        if (err != nil) {
            t.Fatalf("unexpected error: %v", err);
        }
        @string expected = fn12ˢ;
        {
            @string result = tmpl.Root.String(); if (result != expected) {
                t.Errorf("got\n\t%v\nexpected\n\t%v", result, expected);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct isEmptyTest {
    internal @string name;
    internal @string input;
    internal bool empty;
}

internal static slice<isEmptyTest> isEmptyTests = slice<isEmptyTest>([
    new("empty", @"", true),
    new("nonempty", @"hello", false),
    new("spaces only", " \t\n \t\n", true),
    new("comment only", "{{/* comment */}}", true),
    new("definition", @"{{define ""x""}}something{{end}}", true),
    new("definitions and space", "{{define `x`}}something{{end}}\n\n{{define `y`}}something{{end}}\n\n", true),
    new("definitions and text", "{{define `x`}}something{{end}}\nx\n{{define `y`}}something{{end}}\ny\n", false),
    new("definition and action", "{{define `x`}}something{{end}}{{if 3}}foo{{end}}", false)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootˢ => ref rootˢᵇ;
private static readonly @string rootˢᵇ = "root"u8;

public static void TestIsEmpty(ж<testing.T> t) {
    if (!IsEmptyTree(default)) {
        t.Errorf("nil tree is not empty");
    }
    foreach (var (_, test) in isEmptyTests) {
        var (tree, err) = New(rootˢ).Parse(test.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), (map<@string, any>)(default));
        if (err != nil) {
            t.Errorf("%q: unexpected error: %v", test.name, err);
            continue;
        }
        {
            var empty = IsEmptyTree(new global::go.text.template.parse_package.ListNodeжNode(tree.Root)); if (empty != test.empty) {
                t.Errorf("%q: expected %t got %t", test.name, test.empty, empty);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ifTrueEndˢ => ref ifTrueEndˢᵇ;
private static readonly @string ifTrueEndˢᵇ = "{{if true}}{{end}}"u8;

public static void TestErrorContextWithTreeCopy(ж<testing.T> t) {
    var (tree, err) = New(rootˢ).Parse(ifTrueEndˢ, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), (map<@string, any>)(default));
    if (err != nil) {
        t.Fatalf("unexpected tree parse failure: %v", err);
    }
    var treeCopy = tree.Copy();
    var (wantLocation, wantContext) = tree.ErrorContext(tree.Root.Nodes[0]);
    var (gotLocation, gotContext) = treeCopy.ErrorContext(treeCopy.Root.Nodes[0]);
    if (wantLocation != gotLocation) {
        t.Errorf("wrong error location want %q got %q", wantLocation, gotLocation);
    }
    if (wantContext != gotContext) {
        t.Errorf("wrong error location want %q got %q", wantContext, gotContext);
    }
}

// All failures, and the result is a string that must appear in the error message.
internal static slice<parseTest> errorTests = slice<parseTest>([
    // Check line numbers are accurate.
    new("unclosed1",
        "line1\n{{",
        hasError, @"unclosed1:2: unclosed action"),
    new("unclosed2",
        "line1\n{{define `x`}}line2\n{{",
        hasError, @"unclosed2:3: unclosed action"),
    new("unclosed3",
        "line1\n{{\"x\"\n\"y\"\n",
        hasError, @"unclosed3:4: unclosed action started at unclosed3:2"),
    new("unclosed4",
        "{{\n\n\n\n\n",
        hasError, @"unclosed4:6: unclosed action started at unclosed4:1"),
    new("var1",
        "line1\n{{\nx\n}}",
        hasError, @"var1:3: function ""x"" not defined"),
    // Specific errors.
    new("function",
        "{{foo}}",
        hasError, @"function ""foo"" not defined"),
    new("comment1",
        "{{/*}}",
        hasError, @"comment1:1: unclosed comment"),
    new("comment2",
        "{{/*\nhello\n}}",
        hasError, @"comment2:1: unclosed comment"),
    new("lparen",
        "{{.X (1 2 3}}",
        hasError, @"unclosed left paren"),
    new("rparen",
        "{{.X 1 2 3 ) }}",
        hasError, "unexpected right paren"),
    new("rparen2",
        "{{(.X 1 2 3",
        hasError, @"unclosed action"),
    new("space",
        "{{`x`3}}",
        hasError, @"in operand"),
    new("idchar",
        "{{a#}}",
        hasError, @"'#'"),
    new("charconst",
        "{{'a}}",
        hasError, @"unterminated character constant"),
    new("stringconst",
        @"{{""a}}",
        hasError, @"unterminated quoted string"),
    new("rawstringconst",
        "{{`a}}",
        hasError, @"unterminated raw quoted string"),
    new("number",
        "{{0xi}}",
        hasError, @"number syntax"),
    new("multidefine",
        "{{define `a`}}a{{end}}{{define `a`}}b{{end}}",
        hasError, @"multiple definition of template"),
    new("eof",
        "{{range .X}}",
        hasError, @"unexpected EOF"),
    new("variable",
        // Declare $x so it's defined, to avoid that error, and then check we don't parse a declaration.
        "{{$x := 23}}{{with $x.y := 3}}{{$x 23}}{{end}}",
        hasError, @"unexpected "":="""),
    new("multidecl",
        "{{$a,$b,$c := 23}}",
        hasError, @"too many declarations"),
    new("undefvar",
        "{{$a}}",
        hasError, @"undefined variable"),
    new("wrongdot",
        "{{true.any}}",
        hasError, @"unexpected . after term"),
    new("wrongpipeline",
        "{{12|false}}",
        hasError, @"non executable command in pipeline"),
    new("emptypipeline",
        @"{{ ( ) }}",
        hasError, @"missing value for parenthesized pipeline"),
    new("multilinerawstring",
        "{{ $v := `\n` }} {{",
        hasError, @"multilinerawstring:2: unclosed action"),
    new("rangeundefvar",
        "{{range $k}}{{end}}",
        hasError, @"undefined variable"),
    new("rangeundefvars",
        "{{range $k, $v}}{{end}}",
        hasError, @"undefined variable"),
    new("rangemissingvalue1",
        "{{range $k,}}{{end}}",
        hasError, @"missing value for range"),
    new("rangemissingvalue2",
        "{{range $k, $v := }}{{end}}",
        hasError, @"missing value for range"),
    new("rangenotvariable1",
        "{{range $k, .}}{{end}}",
        hasError, @"range can only initialize variables"),
    new("rangenotvariable2",
        "{{range $k, 123 := .}}{{end}}",
        hasError, @"range can only initialize variables")
]);

public static void TestErrors(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in errorTests) {
        ref var test = ref heap(new parseTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var (_, err) = New(testʗ1.name).Parse(testʗ1.input, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>());
            if (err == nil) {
                tΔ1.Fatalf("expected error %q, got nil", testʗ1.result);
            }
            if (!strings.Contains(err.Error(), testʗ1.result)) {
                tΔ1.Fatalf("error %q does not contain %q", err, testʗ1.result);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string outerˢ => ref outerˢᵇ;
private static readonly @string outerˢᵇ = "outer"u8;
internal static ref readonly @string innerˢ => ref innerˢᵇ;
private static readonly @string innerˢᵇ = "inner"u8;
internal static readonly object blockDidNotDefineˢ = (@string)"block did not define template"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = @"a{{block ""inner"" .}}bar{{.}}baz{{end}}b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string outerᶜ => ref outerᶜᵇ;
private static readonly @string outerᶜᵇ = @"a{{template ""inner"" .}}b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string innerᶜ => ref innerᶜᵇ;
private static readonly @string innerᶜᵇ = @"bar{{.}}baz"u8;

public static void TestBlock(ж<testing.T> t) {
    @string input = inputᶜ;
    @string outer = outerᶜ;
    @string inner = innerᶜ;
    var treeSet = new map<@string, ж<global::go.text.template.parse_package.Tree>>();
    var (tmpl, err) = New(outerˢ).Parse(input, ""u8, ""u8, treeSet, (map<@string, any>)(default));
    if (err != nil) {
        t.Fatal(err);
    }
    {
        @string g = tmpl.Root.String();
        @string w = outer; if (g != w) {
            t.Errorf("outer template = %q, want %q", g, w);
        }
    }
    var inTmpl = treeSet[innerˢ];
    if (inTmpl == nil) {
        t.Fatal(blockDidNotDefineˢ);
    }
    {
        @string g = inTmpl.Root.String();
        @string w = inner; if (g != w) {
            t.Errorf("inner template = %q, want %q", g, w);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string printf1234ˢ => ref printf1234ˢᵇ;
private static readonly @string printf1234ˢᵇ = "{{printf 1234}}\n"u8;
internal static ref readonly @string benchˢ => ref benchˢᵇ;
private static readonly @string benchˢᵇ = "bench"u8;

public static void TestLineNum(ж<testing.T> t) {
    // const count = 100
    const nint count = 3;
    @string text = strings.Repeat(printf1234ˢ, count);
    var (tree, err) = New(benchˢ).Parse(text, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
    if (err != nil) {
        t.Fatal(err);
    }
    // Check the line numbers. Each line is an action containing a template, followed by text.
    // That's two nodes per line.
    var nodes = tree.Root.Nodes;
    for (nint i = 0; i < len(nodes); i += 2) {
        nint line = 1 + i / 2;
        // Action first.
        var action = nodes[i]._<ж<global::go.text.template.parse_package.ActionNode>>();
        if (action.Line != line) {
            t.Errorf("line %d: action is line %d", line, action.Line);
        }
        var pipe = action.Pipe;
        if (pipe.Line != line) {
            t.Errorf("line %d: pipe is line %d", line, pipe.Line);
        }
    }
}

public static void BenchmarkParseLarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string text = strings.Repeat("{{1234}}\n", 10000);
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = New(benchˢ).Parse(text, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
        if (err != nil) {
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
        Ident: slice<@string>(["$", "A", "BB", "CCC", "THIS_IS_THE_VARIABLE_BEING_PROCESSED"])
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
internal static ref readonly @string printfField1Field2Field3ˢ => ref printfField1Field2Field3ˢᵇ;
private static readonly @string printfField1Field2Field3ˢᵇ = """

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

"""u8.Raw();

public static void BenchmarkListString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string text = printfField1Field2Field3ˢ;
    var (tree, err) = New(benchˢ).Parse(text, ""u8, ""u8, new map<@string, ж<global::go.text.template.parse_package.Tree>>(), builtins);
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    b.ResetTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        sinkl = tree.Root.String();
    }
    if (sinkl == ""u8) {
        Ꮡb.Fatal(benchmarkWasNotRunˢ);
    }
}

} // end parse_internal_test_package
