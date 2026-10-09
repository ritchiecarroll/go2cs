// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using errors = errors_package;
using math = math_package;
using strings = strings_package;
using testing = testing_package;
using static go.html.template_package;

partial class template_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object blankTokensˢ = (@string)"Blank tokens"u8;

internal partial struct TestNextJsCtx_tests /*dyn*/ {
    internal global::go.html.template_package.jsCtx jsCtx;
    internal @string s;
}

public static void TestNextJsCtx(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestNextJsCtx_tests[]{ // Statement terminators precede regexps.

        new(jsCtxRegexp, ";"u8), // This is not airtight.
 //     ({ valueOf: function () { return 1 } } / 2)
 // is valid JavaScript but in practice, devs do not do this.
 // A block followed by a statement starting with a RegExp is
 // much more common:
 //     while (x) {...} /foo/.test(x) || panic()

        new(jsCtxRegexp, "}"u8), // But member, call, grouping, and array expression terminators
 // precede div ops.

        new(jsCtxDivOp, ")"u8),
        new(jsCtxDivOp, "]"u8), // At the start of a primary expression, array, or expression
 // statement, expect a regexp.

        new(jsCtxRegexp, "("u8),
        new(jsCtxRegexp, "["u8),
        new(jsCtxRegexp, "{"u8), // Assignment operators precede regexps as do all exclusively
 // prefix and binary operators.

        new(jsCtxRegexp, "="u8),
        new(jsCtxRegexp, "+="u8),
        new(jsCtxRegexp, "*="u8),
        new(jsCtxRegexp, "*"u8),
        new(jsCtxRegexp, "!"u8), // Whether the + or - is infix or prefix, it cannot precede a
 // div op.

        new(jsCtxRegexp, "+"u8),
        new(jsCtxRegexp, "-"u8), // An incr/decr op precedes a div operator.
 // This is not airtight. In (g = ++/h/i) a regexp follows a
 // pre-increment operator, but in practice devs do not try to
 // increment or decrement regular expressions.
 // (g++/h/i) where ++ is a postfix operator on g is much more
 // common.

        new(jsCtxDivOp, "--"u8),
        new(jsCtxDivOp, "++"u8),
        new(jsCtxDivOp, "x--"u8), // When we have many dashes or pluses, then they are grouped
 // left to right.

        new(jsCtxRegexp, "x---"u8), // A postfix -- then a -.
 // return followed by a slash returns the regexp literal or the
 // slash starts a regexp literal in an expression statement that
 // is dead code.

        new(jsCtxRegexp, "return"u8),
        new(jsCtxRegexp, "return "u8),
        new(jsCtxRegexp, "return\t"u8),
        new(jsCtxRegexp, "return\n"u8),
        new(jsCtxRegexp, "return\u2028"u8), // Identifiers can be divided and cannot validly be preceded by
 // a regular expressions. Semicolon insertion cannot happen
 // between an identifier and a regular expression on a new line
 // because the one token lookahead for semicolon insertion has
 // to conclude that it could be a div binary op and treat it as
 // such.

        new(jsCtxDivOp, "x"u8),
        new(jsCtxDivOp, "x "u8),
        new(jsCtxDivOp, "x\t"u8),
        new(jsCtxDivOp, "x\n"u8),
        new(jsCtxDivOp, "x\u2028"u8),
        new(jsCtxDivOp, "preturn"u8), // Numbers precede div ops.

        new(jsCtxDivOp, "0"u8), // Dots that are part of a number are div preceders.

        new(jsCtxDivOp, "0."u8), // Some JS interpreters treat NBSP as a normal space, so
 // we must too in order to properly escape things.

        new(jsCtxRegexp, "=\u00A0"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            var ctx = nextJSCtx(slice<byte>(test.s), jsCtxRegexp); if (ctx != test.jsCtx) {
                Ꮡt.Errorf("%q: want %s got %s"u8, test.s, test.jsCtx, ctx);
            }
        }
        {
            var ctx = nextJSCtx(slice<byte>(test.s), jsCtxDivOp); if (ctx != test.jsCtx) {
                Ꮡt.Errorf("%q: want %s got %s"u8, test.s, test.jsCtx, ctx);
            }
        }
    }
    if (nextJSCtx(slice<byte>("   "u8), jsCtxRegexp) != jsCtxRegexp) {
        Ꮡt.Error(blankTokensˢ);
    }
    if (nextJSCtx(slice<byte>("   "u8), jsCtxDivOp) != jsCtxDivOp) {
        Ꮡt.Error(blankTokensˢ);
    }
}

internal partial struct jsonErrType {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBScriptCScriptDESCrIpTFˢ = "a */ b <script c </script d <!-- e <sCrIpT f </sCrIpT"u8;

internal static (slice<byte>, error) MarshalJSON(this ref jsonErrType e) {
    return (default!, errors.New(aBScriptCScriptDESCrIpTFˢ));
}

internal partial struct TestJSValEscaper_tests /*dyn*/ {
    internal any x;
    internal @string js;
    internal bool skipNest;
}

internal partial struct TestJSValEscaper_type /*dyn*/ {
    public nint X, Y;
}

public static void TestJSValEscaper(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestJSValEscaper_tests[]{
        new((nint)42, " 42 "u8, false),
        new((nuint)42, " 42 "u8, false),
        new((int16)42, " 42 "u8, false),
        new((uint16)42, " 42 "u8, false),
        new((int32)(-42), " -42 "u8, false),
        new((uint32)42, " 42 "u8, false),
        new((int16)(-42), " -42 "u8, false),
        new((uint16)42, " 42 "u8, false),
        new((int64)(-42), " -42 "u8, false),
        new((uint64)42, " 42 "u8, false),
        new(((uint64)1 << (int)(53)), " 9007199254740992 "u8, false), // ulp(1 << 53) > 1 so this loses precision in JS
 // but it is still a representable integer literal.

        new(((uint64)1 << (int)(53)) + 1, " 9007199254740993 "u8, false),
        new((float32)1.0F, " 1 "u8, false),
        new((float32)(-1.0F), " -1 "u8, false),
        new((float32)0.5F, " 0.5 "u8, false),
        new((float32)(-0.5F), " -0.5 "u8, false),
        new((float32)1.0F / (float32)256F, " 0.00390625 "u8, false),
        new((float32)0F, " 0 "u8, false),
        new(math.Copysign(0D, -1D), " -0 "u8, false),
        new((float64)1.0D, " 1 "u8, false),
        new((float64)(-1.0D), " -1 "u8, false),
        new((float64)0.5D, " 0.5 "u8, false),
        new((float64)(-0.5D), " -0.5 "u8, false),
        new((float64)0D, " 0 "u8, false),
        new(math.Copysign(0D, -1D), " -0 "u8, false),
        new((@string)""u8, @""""""u8, false),
        new((@string)"foo"u8, @"""foo"""u8, false), // Newlines.

        new((@string)"\r\n\u2028\u2029"u8, @"""\r\n\u2028\u2029"""u8, false), // "\v" == "v" on IE 6 so use "\u000b" instead.

        new((@string)"\t\x0b"u8, @"""\t\u000b"""u8, false),
        new(new TestJSValEscaper_type(1, 2), @"{""X"":1,""Y"":2}"u8, false),
        new(new any[]{}.slice(), "[]"u8, false),
        new(new any[]{(nint)(42), (@string)"foo"u8, default!}.slice(), @"[42,""foo"",null]"u8, false),
        new(new @string[]{"<!--"u8, "</script>"u8, "-->"u8}.slice(), @"[""\u003c!--"",""\u003c/script\u003e"",""--\u003e""]"u8, false),
        new((@string)"<!--"u8, @"""\u003c!--"""u8, false),
        new((@string)"-->"u8, @"""--\u003e"""u8, false),
        new((@string)"<![CDATA["u8, @"""\u003c![CDATA["""u8, false),
        new((@string)"]]>"u8, @"""]]\u003e"""u8, false),
        new((@string)"</script"u8, @"""\u003c/script"""u8, false),
        new((@string)"\U0001D11E"u8, "\"\U0001D11E\""u8, false), // or "\uD834\uDD1E"

        new(default!, " null "u8, false),
        new(Ꮡ(new jsonErrType(nil)), " /* json: error calling MarshalJSON for type *template.jsonErrType: a * / b \\x3Cscript c \\x3C/script d \\x3C!-- e \\x3Cscript f \\x3C/script */null "u8, true)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string js = jsValEscaper(test.x); if (js != test.js) {
                Ꮡt.Errorf("%+v: want\n\t%q\ngot\n\t%q"u8, test.x, test.js, js);
            }
        }
        if (test.skipNest) {
            continue;
        }
        // Make sure that escaping corner cases are not broken
        // by nesting.
        var a = new any[]{test.x}.slice();
        @string want = "["u8 + strings.TrimSpace(test.js) + "]"u8;
        {
            @string js = jsValEscaper((any)(a)); if (js != want) {
                Ꮡt.Errorf("%+v: want\n\t%q\ngot\n\t%q"u8, a, want, js);
            }
        }
    }
}

internal partial struct TestJSStrEscaper_tests /*dyn*/ {
    internal any x;
    internal @string esc;
}

public static void TestJSStrEscaper(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestJSStrEscaper_tests[]{
        new((@string)""u8, @""u8),
        new((@string)"foo"u8, @"foo"u8),
        new((@string)"\u0000"u8, @"\u0000"u8),
        new((@string)"\t"u8, @"\t"u8),
        new((@string)"\n"u8, @"\n"u8),
        new((@string)"\r"u8, @"\r"u8),
        new((@string)"\u2028"u8, @"\u2028"u8),
        new((@string)"\u2029"u8, @"\u2029"u8),
        new((@string)"\\"u8, @"\\"u8),
        new((@string)"\\n"u8, @"\\n"u8),
        new((@string)"foo\r\nbar"u8, @"foo\r\nbar"u8), // Preserve attribute boundaries.

        new((@string)@""""u8, @"\u0022"u8),
        new((@string)@"'"u8, @"\u0027"u8), // Allow embedding in HTML without further escaping.

        new((@string)@"&amp;"u8, @"\u0026amp;"u8), // Prevent breaking out of text node and element boundaries.

        new((@string)"</script>"u8, @"\u003c\/script\u003e"u8),
        new((@string)"<![CDATA["u8, @"\u003c![CDATA["u8),
        new((@string)"]]>"u8, @"]]\u003e"u8), // https://dev.w3.org/html5/markup/aria/syntax.html#escaping-text-span
 //   "The text in style, script, title, and textarea elements
 //   must not have an escaping text span start that is not
 //   followed by an escaping text span end."
 // Furthermore, spoofing an escaping text span end could lead
 // to different interpretation of a </script> sequence otherwise
 // masked by the escaping text span, and spoofing a start could
 // allow regular text content to be interpreted as script
 // allowing script execution via a combination of a JS string
 // injection followed by an HTML text injection.

        new((@string)"<!--"u8, @"\u003c!--"u8),
        new((@string)"-->"u8, @"--\u003e"u8), // From https://code.google.com/p/doctype/wiki/ArticleUtf7

        new((@string)"+ADw-script+AD4-alert(1)+ADw-/script+AD4-"u8,
            @"\u002bADw-script\u002bAD4-alert(1)\u002bADw-\/script\u002bAD4-"u8
        ), // Invalid UTF-8 sequence

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xa0, 0x62, 0x61, 0x72})), ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xa0, 0x62, 0x61, 0x72}))), // Invalid unicode scalar value.

        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xed, 0xa0, 0x80, 0x62, 0x61, 0x72})), ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xed, 0xa0, 0x80, 0x62, 0x61, 0x72})))
    }.slice();
    foreach (var (_, test) in tests) {
        @string esc = jsStrEscaper(test.x);
        if (esc != test.esc) {
            Ꮡt.Errorf("%q: want %q got %q"u8, test.x, test.esc, esc);
        }
    }
}

internal partial struct TestJSRegexpEscaper_tests /*dyn*/ {
    internal any x;
    internal @string esc;
}

public static void TestJSRegexpEscaper(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestJSRegexpEscaper_tests[]{
        new((@string)""u8, @"(?:)"u8),
        new((@string)"foo"u8, @"foo"u8),
        new((@string)"\u0000"u8, @"\u0000"u8),
        new((@string)"\t"u8, @"\t"u8),
        new((@string)"\n"u8, @"\n"u8),
        new((@string)"\r"u8, @"\r"u8),
        new((@string)"\u2028"u8, @"\u2028"u8),
        new((@string)"\u2029"u8, @"\u2029"u8),
        new((@string)"\\"u8, @"\\"u8),
        new((@string)"\\n"u8, @"\\n"u8),
        new((@string)"foo\r\nbar"u8, @"foo\r\nbar"u8), // Preserve attribute boundaries.

        new((@string)@""""u8, @"\u0022"u8),
        new((@string)@"'"u8, @"\u0027"u8), // Allow embedding in HTML without further escaping.

        new((@string)@"&amp;"u8, @"\u0026amp;"u8), // Prevent breaking out of text node and element boundaries.

        new((@string)"</script>"u8, @"\u003c\/script\u003e"u8),
        new((@string)"<![CDATA["u8, @"\u003c!\[CDATA\["u8),
        new((@string)"]]>"u8, @"\]\]\u003e"u8), // Escaping text spans.

        new((@string)"<!--"u8, @"\u003c!\-\-"u8),
        new((@string)"-->"u8, @"\-\-\u003e"u8),
        new((@string)"*"u8, @"\*"u8),
        new((@string)"+"u8, @"\u002b"u8),
        new((@string)"?"u8, @"\?"u8),
        new((@string)"[](){}"u8, @"\[\]\(\)\{\}"u8),
        new((@string)"$foo|x.y"u8, @"\$foo\|x\.y"u8),
        new((@string)"x^y"u8, @"x\^y"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string esc = jsRegexpEscaper(test.x);
        if (esc != test.esc) {
            Ꮡt.Errorf("%q: want %q got %q"u8, test.x, test.esc, esc);
        }
    }
}

internal partial struct TestEscapersOnLower7AndSelectHighCodepoints_tests /*dyn*/ {
    internal @string name;
    internal Funcꓸꓸꓸ<any, @string> escaper;
    internal @string escaped;
}

public static void TestEscapersOnLower7AndSelectHighCodepoints(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string input = ("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !""#$%&'()*+,-./"u8 + @"0123456789:;<=>?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + "`abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\x7f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);
    var tests = new TestEscapersOnLower7AndSelectHighCodepoints_tests[]{
        new(
            "jsStrEscaper"u8,
            jsStrEscaper,
            @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007"u8 + @"\u0008\t\n\u000b\f\r\u000e\u000f"u8 + @"\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017"u8 + @"\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f"u8 + @" !\u0022#$%\u0026\u0027()*\u002b,-.\/"u8 + @"0123456789:;\u003c=\u003e?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\\]^_"u8 + "\\u0060abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\u007f"u8 + "\u00A0\u0100\\u2028\\u2029\ufeff\U0001D11E"u8
        ),
        new(
            "jsRegexpEscaper"u8,
            jsRegexpEscaper,
            @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007"u8 + @"\u0008\t\n\u000b\f\r\u000e\u000f"u8 + @"\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017"u8 + @"\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f"u8 + @" !\u0022#\$%\u0026\u0027\(\)\*\u002b,\-\.\/"u8 + @"0123456789:;\u003c=\u003e\?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ\[\\\]\^_"u8 + "`abcdefghijklmno"u8 + @"pqrstuvwxyz\{\|\}~"u8 + "\u007f"u8 + "\u00A0\u0100\\u2028\\u2029\ufeff\U0001D11E"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string s = test.escaper(input); if (s != test.escaped) {
                Ꮡt.Errorf("%s once: want\n\t%q\ngot\n\t%q"u8, test.name, test.escaped, s);
                continue;
            }
        }
        // Escape it rune by rune to make sure that any
        // fast-path checking does not break escaping.
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        foreach (var (_, c) in input) {
            Ꮡbuf.WriteString(test.escaper(((@string)c)));
        }
        {
            @string s = buf.String(); if (s != test.escaped) {
                Ꮡt.Errorf("%s rune-wise: want\n\t%q\ngot\n\t%q"u8, test.name, test.escaped, s);
                continue;
            }
        }
    }
}

internal partial struct TestIsJsMimeType_tests /*dyn*/ {
    internal @string @in;
    internal bool @out;
}

public static void TestIsJsMimeType(ж<testing.T> Ꮡt) {
    var tests = new TestIsJsMimeType_tests[]{
        new("application/javascript;version=1.8"u8, true),
        new("application/javascript;version=1.8;foo=bar"u8, true),
        new("application/javascript/version=1.8"u8, false),
        new("text/javascript"u8, true),
        new("application/json"u8, true),
        new("application/ld+json"u8, true),
        new("module"u8, true)
    }.slice();
    foreach (var (_, test) in tests) {
        if (isJSType(test.@in) != test.@out) {
            Ꮡt.Errorf("isJSType(%q) = %v, want %v"u8, test.@in, !test.@out, test.@out);
        }
    }
}

public static void BenchmarkJSValEscaperWithNum(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsValEscaper(3.141592654D);
    }
}

public static void BenchmarkJSValEscaperWithStr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsValEscaper(theIQuickISpanStyleColorˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object theQuickBrownFoxJumpsˢ3 = (@string)"The quick, brown fox jumps over the lazy dog"u8;

public static void BenchmarkJSValEscaperWithStrNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsValEscaper(theQuickBrownFoxJumpsˢ3);
    }
}

internal partial struct BenchmarkJSValEscaperWithObj_o /*dyn*/ {
    public @string S;
    public nint N;
}

public static void BenchmarkJSValEscaperWithObj(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var o = new BenchmarkJSValEscaperWithObj_o(
        "The <i>quick</i>,\r\n<span style='color:brown'>brown</span> fox jumps\u2028over the <canine class=\"lazy\">dog</canine>\u2028"u8,
        42
    );
    for (nint i = 0; i < b.N; i++) {
        jsValEscaper(o);
    }
}

internal partial struct BenchmarkJSValEscaperWithObjNoSpecials_o /*dyn*/ {
    public @string S;
    public nint N;
}

public static void BenchmarkJSValEscaperWithObjNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var o = new BenchmarkJSValEscaperWithObjNoSpecials_o(
        "The quick, brown fox jumps over the lazy dog"u8,
        42
    );
    for (nint i = 0; i < b.N; i++) {
        jsValEscaper(o);
    }
}

public static void BenchmarkJSStrEscaperNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsStrEscaper(theQuickBrownFoxJumpsˢ);
    }
}

public static void BenchmarkJSStrEscaper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsStrEscaper(theIQuickISpanStyleColorˢ);
    }
}

public static void BenchmarkJSRegexpEscaperNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsRegexpEscaper(theQuickBrownFoxJumpsˢ3);
    }
}

public static void BenchmarkJSRegexpEscaper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        jsRegexpEscaper(theIQuickISpanStyleColorˢ);
    }
}

} // end template_internal_test_package
