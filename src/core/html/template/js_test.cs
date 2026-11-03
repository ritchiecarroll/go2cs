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

public static void TestNextJsCtx(ж<testing.T> t) {
    var tests = slice<TestNextJsCtx_tests>([
        // Statement terminators precede regexps.
        new(jsCtxRegexp, ";"),
        // This is not airtight.
        //     ({ valueOf: function () { return 1 } } / 2)
        // is valid JavaScript but in practice, devs do not do this.
        // A block followed by a statement starting with a RegExp is
        // much more common:
        //     while (x) {...} /foo/.test(x) || panic()
        new(jsCtxRegexp, "}"),
        // But member, call, grouping, and array expression terminators
        // precede div ops.
        new(jsCtxDivOp, ")"),
        new(jsCtxDivOp, "]"),
        // At the start of a primary expression, array, or expression
        // statement, expect a regexp.
        new(jsCtxRegexp, "("),
        new(jsCtxRegexp, "["),
        new(jsCtxRegexp, "{"),
        // Assignment operators precede regexps as do all exclusively
        // prefix and binary operators.
        new(jsCtxRegexp, "="),
        new(jsCtxRegexp, "+="),
        new(jsCtxRegexp, "*="),
        new(jsCtxRegexp, "*"),
        new(jsCtxRegexp, "!"),
        // Whether the + or - is infix or prefix, it cannot precede a
        // div op.
        new(jsCtxRegexp, "+"),
        new(jsCtxRegexp, "-"),
        // An incr/decr op precedes a div operator.
        // This is not airtight. In (g = ++/h/i) a regexp follows a
        // pre-increment operator, but in practice devs do not try to
        // increment or decrement regular expressions.
        // (g++/h/i) where ++ is a postfix operator on g is much more
        // common.
        new(jsCtxDivOp, "--"),
        new(jsCtxDivOp, "++"),
        new(jsCtxDivOp, "x--"),
        // When we have many dashes or pluses, then they are grouped
        // left to right.
        new(jsCtxRegexp, "x---"), // A postfix -- then a -.
        // return followed by a slash returns the regexp literal or the
        // slash starts a regexp literal in an expression statement that
        // is dead code.
        new(jsCtxRegexp, "return"),
        new(jsCtxRegexp, "return "),
        new(jsCtxRegexp, "return\t"),
        new(jsCtxRegexp, "return\n"),
        new(jsCtxRegexp, "return\u2028"),
        new(jsCtxRegexp, "yield"),
        // A keyword after property access is a property name.
        new(jsCtxDivOp, "x.yield"),
        new(jsCtxDivOp, "x?.yield"),
        new(jsCtxDivOp, "x.\nyield"),
        new(jsCtxDivOp, "x.in"),
        // Identifiers can be divided and cannot validly be preceded by
        // a regular expressions. Semicolon insertion cannot happen
        // between an identifier and a regular expression on a new line
        // because the one token lookahead for semicolon insertion has
        // to conclude that it could be a div binary op and treat it as
        // such.
        new(jsCtxDivOp, "x"),
        new(jsCtxDivOp, "x "),
        new(jsCtxDivOp, "x\t"),
        new(jsCtxDivOp, "x\n"),
        new(jsCtxDivOp, "x\u2028"),
        new(jsCtxDivOp, "preturn"),
        // Numbers precede div ops.
        new(jsCtxDivOp, "0"),
        // Dots that are part of a number are div preceders.
        new(jsCtxDivOp, "0."),
        // Some JS interpreters treat NBSP as a normal space, so
        // we must too in order to properly escape things.
        new(jsCtxRegexp, "=\u00A0")
    ]);

    foreach (var (_, test) in tests) {
        {
            var ctx = nextJSCtx(slice<byte>(test.s), jsCtxRegexp); if (ctx != test.jsCtx) {
                t.Errorf("%q: want %s got %s", test.s, test.jsCtx, ctx);
            }
        }
        {
            var ctx = nextJSCtx(slice<byte>(test.s), jsCtxDivOp); if (ctx != test.jsCtx) {
                t.Errorf("%q: want %s got %s", test.s, test.jsCtx, ctx);
            }
        }
    }

    if (nextJSCtx(slice<byte>("   "u8), jsCtxRegexp) != jsCtxRegexp) {
        t.Error(blankTokensˢ);
    }

    if (nextJSCtx(slice<byte>("   "u8), jsCtxDivOp) != jsCtxDivOp) {
        t.Error(blankTokensˢ);
    }
}

internal partial struct jsonErrType {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBScriptCScriptDESCrIpTFˢ => ref aBScriptCScriptDESCrIpTFˢᵇ;
private static readonly @string aBScriptCScriptDESCrIpTFˢᵇ = "a */ b <script c </script d <!-- e <sCrIpT f </sCrIpT"u8;

internal static (slice<byte>, error) MarshalJSON(this ref jsonErrType e) {
    return (default, errors.New(aBScriptCScriptDESCrIpTFˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cdataˢ = (@string)"<![CDATA["u8;
internal static readonly object scriptˢ = (@string)"</script"u8;

internal partial struct TestJSValEscaper_tests /*dyn*/ {
    internal any x;
    internal @string js;
    internal bool skipNest;
}

internal partial struct TestJSValEscaper_type /*dyn*/ {
    public nint X, Y;
}

public static void TestJSValEscaper(ж<testing.T> t) {
    var tests = slice<TestJSValEscaper_tests>([
        new((nint)42, " 42 ", false),
        new((nuint)42, " 42 ", false),
        new((int16)42, " 42 ", false),
        new((uint16)42, " 42 ", false),
        new((int32)(-42), " -42 ", false),
        new((uint32)42, " 42 ", false),
        new((int16)(-42), " -42 ", false),
        new((uint16)42, " 42 ", false),
        new((int64)(-42), " -42 ", false),
        new((uint64)42, " 42 ", false),
        new((uint64)1 << 53, " 9007199254740992 ", false),
        // ulp(1 << 53) > 1 so this loses precision in JS
        // but it is still a representable integer literal.
        new(((uint64)1 << 53) + 1, " 9007199254740993 ", false),
        new((float32)1.0F, " 1 ", false),
        new((float32)(-1.0F), " -1 ", false),
        new((float32)0.5F, " 0.5 ", false),
        new((float32)(-0.5F), " -0.5 ", false),
        new((float32)1.0F / (float32)256F, " 0.00390625 ", false),
        new((float32)0F, " 0 ", false),
        new(math.Copysign(0D, -1D), " -0 ", false),
        new((float64)1.0D, " 1 ", false),
        new((float64)(-1.0D), " -1 ", false),
        new((float64)0.5D, " 0.5 ", false),
        new((float64)(-0.5D), " -0.5 ", false),
        new((float64)0D, " 0 ", false),
        new(math.Copysign(0D, -1D), " -0 ", false),
        new((@string)""u8, @"""""", false),
        new(fooˢ2, @"""foo""", false),
        // Newlines.
        new((@string)"\r\n\u2028\u2029", @"""\r\n\u2028\u2029""", false),
        // "\v" == "v" on IE 6 so use "\u000b" instead.
        new((@string)"\t\x0b", @"""\t\u000b""", false),
        new(new TestJSValEscaper_type(1, 2), @"{""X"":1,""Y"":2}", false),
        new(slice<any>([]), "[]", false),
        new(slice<any>([(nint)42, fooˢ2, default]), @"[42,""foo"",null]", false),
        new(slice<@string>(["<!--", "</script>", "-->"]), @"[""\u003c!--"",""\u003c/script\u003e"",""--\u003e""]", false),
        new((@string)"<!--", @"""\u003c!--""", false),
        new((@string)"-->", @"""--\u003e""", false),
        new(cdataˢ, @"""\u003c![CDATA[""", false),
        new((@string)"]]>", @"""]]\u003e""", false),
        new(scriptˢ, @"""\u003c/script""", false),
        new((@string)"\U0001D11E", "\"\U0001D11E\"", false), // or "\uD834\uDD1E"
        new(default, " null ", false),
        new(Ꮡ(new jsonErrType()), " /* json: error calling MarshalJSON for type *template.jsonErrType: a * / b \\x3Cscript c \\x3C/script d \\x3C!-- e \\x3Cscript f \\x3C/script */null ", true)
    ]);

    foreach (var (_, test) in tests) {
        {
            @string js = jsValEscaper(test.x); if (js != test.js) {
                t.Errorf("%+v: want\n\t%q\ngot\n\t%q", test.x, test.js, js);
            }
        }
        if (test.skipNest) {
            continue;
        }
        // Make sure that escaping corner cases are not broken
        // by nesting.
        var a = slice<any>([test.x]);
        @string want = "["u8 + strings.TrimSpace(test.js) + "]"u8;
        {
            @string js = jsValEscaper((any)(a)); if (js != want) {
                t.Errorf("%+v: want\n\t%q\ngot\n\t%q", a, want, js);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fooBarˢ = (@string)"foo\r\nbar"u8;
internal static readonly object ampˢ = (@string)@"&amp;"u8;
internal static readonly object scriptˢ2 = (@string)"</script>"u8;
internal static readonly object aDwScriptAd4Alert1ADwˢ = (@string)"+ADw-script+AD4-alert(1)+ADw-/script+AD4-"u8;

internal partial struct TestJSStrEscaper_tests /*dyn*/ {
    internal any x;
    internal @string esc;
}

public static void TestJSStrEscaper(ж<testing.T> t) {
    var tests = slice<TestJSStrEscaper_tests>([
        new((@string)""u8, @""),
        new(fooˢ2, @"foo"),
        new((@string)"\u0000", @"\u0000"),
        new((@string)"\t", @"\t"),
        new((@string)"\n", @"\n"),
        new((@string)"\r", @"\r"),
        new((@string)"\u2028", @"\u2028"),
        new((@string)"\u2029", @"\u2029"),
        new((@string)"\\", @"\\"),
        new((@string)"\\n", @"\\n"),
        new(fooBarˢ, @"foo\r\nbar"),
        // Preserve attribute boundaries.
        new((@string)@"""", @"\u0022"),
        new((@string)@"'", @"\u0027"),
        // Allow embedding in HTML without further escaping.
        new(ampˢ, @"\u0026amp;"),
        // Prevent breaking out of text node and element boundaries.
        new(scriptˢ2, @"\u003c\/script\u003e"),
        new(cdataˢ, @"\u003c![CDATA["),
        new((@string)"]]>", @"]]\u003e"),
        // https://dev.w3.org/html5/markup/aria/syntax.html#escaping-text-span
        //   "The text in style, script, title, and textarea elements
        //   must not have an escaping text span start that is not
        //   followed by an escaping text span end."
        // Furthermore, spoofing an escaping text span end could lead
        // to different interpretation of a </script> sequence otherwise
        // masked by the escaping text span, and spoofing a start could
        // allow regular text content to be interpreted as script
        // allowing script execution via a combination of a JS string
        // injection followed by an HTML text injection.
        new((@string)"<!--", @"\u003c!--"),
        new((@string)"-->", @"--\u003e"),
        // From https://code.google.com/p/doctype/wiki/ArticleUtf7
        new(aDwScriptAd4Alert1ADwˢ,
            @"\u002bADw-script\u002bAD4-alert(1)\u002bADw-\/script\u002bAD4-"
        ),
        // Invalid UTF-8 sequence
        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xa0, 0x62, 0x61, 0x72})), ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xa0, 0x62, 0x61, 0x72}))),
        // Invalid unicode scalar value.
        new(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xed, 0xa0, 0x80, 0x62, 0x61, 0x72})), ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xed, 0xa0, 0x80, 0x62, 0x61, 0x72})))
    ]);

    foreach (var (_, test) in tests) {
        @string esc = jsStrEscaper(test.x);
        if (esc != test.esc) {
            t.Errorf("%q: want %q got %q", test.x, test.esc, esc);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fooXYˢ = (@string)"$foo|x.y"u8;

internal partial struct TestJSRegexpEscaper_tests /*dyn*/ {
    internal any x;
    internal @string esc;
}

public static void TestJSRegexpEscaper(ж<testing.T> t) {
    var tests = slice<TestJSRegexpEscaper_tests>([
        new((@string)""u8, @"(?:)"),
        new(fooˢ2, @"foo"),
        new((@string)"\u0000", @"\u0000"),
        new((@string)"\t", @"\t"),
        new((@string)"\n", @"\n"),
        new((@string)"\r", @"\r"),
        new((@string)"\u2028", @"\u2028"),
        new((@string)"\u2029", @"\u2029"),
        new((@string)"\\", @"\\"),
        new((@string)"\\n", @"\\n"),
        new(fooBarˢ, @"foo\r\nbar"),
        // Preserve attribute boundaries.
        new((@string)@"""", @"\u0022"),
        new((@string)@"'", @"\u0027"),
        // Allow embedding in HTML without further escaping.
        new(ampˢ, @"\u0026amp;"),
        // Prevent breaking out of text node and element boundaries.
        new(scriptˢ2, @"\u003c\/script\u003e"),
        new(cdataˢ, @"\u003c!\[CDATA\["),
        new((@string)"]]>", @"\]\]\u003e"),
        // Escaping text spans.
        new((@string)"<!--", @"\u003c!\-\-"),
        new((@string)"-->", @"\-\-\u003e"),
        new((@string)"*", @"\*"),
        new((@string)"+", @"\u002b"),
        new((@string)"?", @"\?"),
        new((@string)"[](){}", @"\[\]\(\)\{\}"),
        new(fooXYˢ, @"\$foo\|x\.y"),
        new((@string)"x^y", @"x\^y")
    ]);

    foreach (var (_, test) in tests) {
        @string esc = jsRegexpEscaper(test.x);
        if (esc != test.esc) {
            t.Errorf("%q: want %q got %q", test.x, test.esc, esc);
        }
    }
}

internal partial struct TestEscapersOnLower7AndSelectHighCodepoints_tests /*dyn*/ {
    internal @string name;
    internal Funcꓸꓸꓸ<any, @string> escaper;
    internal @string escaped;
}

public static void TestEscapersOnLower7AndSelectHighCodepoints(ж<testing.T> t) {
    @string input = ("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !""#$%&'()*+,-./"u8 + @"0123456789:;<=>?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + "`abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\x7f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);

    var tests = slice<TestEscapersOnLower7AndSelectHighCodepoints_tests>([
        new(
            "jsStrEscaper",
            jsStrEscaper,
            @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007"u8 + @"\u0008\t\n\u000b\f\r\u000e\u000f"u8 + @"\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017"u8 + @"\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f"u8 + @" !\u0022#$%\u0026\u0027()*\u002b,-.\/"u8 + @"0123456789:;\u003c=\u003e?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\\]^_"u8 + "\\u0060abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\u007f"u8 + "\u00A0\u0100\\u2028\\u2029\ufeff\U0001D11E"u8
        ),
        new(
            "jsRegexpEscaper",
            jsRegexpEscaper,
            @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007"u8 + @"\u0008\t\n\u000b\f\r\u000e\u000f"u8 + @"\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017"u8 + @"\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f"u8 + @" !\u0022#\$%\u0026\u0027\(\)\*\u002b,\-\.\/"u8 + @"0123456789:;\u003c=\u003e\?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ\[\\\]\^_"u8 + "`abcdefghijklmno"u8 + @"pqrstuvwxyz\{\|\}~"u8 + "\u007f"u8 + "\u00A0\u0100\\u2028\\u2029\ufeff\U0001D11E"u8
        )
    ]);

    foreach (var (_, test) in tests) {
        {
            @string s = test.escaper(input); if (s != test.escaped) {
                t.Errorf("%s once: want\n\t%q\ngot\n\t%q", test.name, test.escaped, s);
                continue;
            }
        }

        // Escape it rune by rune to make sure that any
        // fast-path checking does not break escaping.
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        foreach (var (_, c) in input) {
            Ꮡbuf.WriteString(test.escaper((@string)c));
        }

        {
            @string s = buf.String(); if (s != test.escaped) {
                t.Errorf("%s rune-wise: want\n\t%q\ngot\n\t%q", test.name, test.escaped, s);
                continue;
            }
        }
    }
}

internal partial struct TestIsJsMimeType_tests /*dyn*/ {
    internal @string @in;
    internal bool @out;
}

public static void TestIsJsMimeType(ж<testing.T> t) {
    var tests = slice<TestIsJsMimeType_tests>([
        new("application/javascript;version=1.8", true),
        new("application/javascript;version=1.8;foo=bar", true),
        new("application/javascript/version=1.8", false),
        new("text/javascript", true),
        new("application/json", true),
        new("application/ld+json", true),
        new("module", true)
    ]);

    foreach (var (_, test) in tests) {
        if (isJSType(test.@in) != test.@out) {
            t.Errorf("isJSType(%q) = %v, want %v", test.@in, !test.@out, test.@out);
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
        "The <i>quick</i>,\r\n<span style='color:brown'>brown</span> fox jumps\u2028over the <canine class=\"lazy\">dog</canine>\u2028",
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
        "The quick, brown fox jumps over the lazy dog",
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
