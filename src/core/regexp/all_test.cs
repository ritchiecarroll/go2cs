// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using syntax = regexp.syntax_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using regexp;
using static go.regexp_package;

partial class regexp_internal_test_package {

internal static slice<@string> goodRe = new @string[]{
    @""u8,
    @"."u8,
    @"^.$"u8,
    @"a"u8,
    @"a*"u8,
    @"a+"u8,
    @"a?"u8,
    @"a|b"u8,
    @"a*|b*"u8,
    @"(a*|b)(c*|d)"u8,
    @"[a-z]"u8,
    @"[a-abc-c\-\]\[]"u8,
    @"[a-z]+"u8,
    @"[abc]"u8,
    @"[^1234]"u8,
    @"[^\n]"u8,
    @"\!\\"u8
}.slice();

internal partial struct stringError {
    internal @string re;
    internal @string err;
}

internal static slice<stringError> badRe = new stringError[]{
    new(@"*"u8, "missing argument to repetition operator: `*`"u8),
    new(@"+"u8, "missing argument to repetition operator: `+`"u8),
    new(@"?"u8, "missing argument to repetition operator: `?`"u8),
    new(@"(abc"u8, "missing closing ): `(abc`"u8),
    new(@"abc)"u8, "unexpected ): `abc)`"u8),
    new(@"x[a-z"u8, "missing closing ]: `[a-z`"u8),
    new(@"[z-a]"u8, "invalid character class range: `z-a`"u8),
    new(@"abc\"u8, "trailing backslash at end of expression"u8),
    new(@"a**"u8, "invalid nested repetition operator: `**`"u8),
    new(@"a*+"u8, "invalid nested repetition operator: `*+`"u8),
    new(@"\x"u8, "invalid escape sequence: `\\x`"u8),
    new(strings.Repeat(@"\pL"u8, 27000), "expression too large"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object compilingˢ = (@string)"compiling `"u8;
internal static readonly object unexpectedErrorˢ = (@string)"`; unexpected error: "u8;
internal static readonly object missingErrorˢ = (@string)"`; missing error"u8;
internal static readonly object wrongErrorˢ = (@string)"`; wrong error: "u8;
internal static readonly object wantˢ = (@string)"; want "u8;

internal static ж<global::go.regexp_package.Regexp> compileTest(ж<testing.T> Ꮡt, @string expr, @string error) {
    var (re, err) = Compile(expr);
    if (error == ""u8 && err != default!) {
        Ꮡt.Error(compilingˢ, expr, unexpectedErrorˢ, err.Error());
    }
    if (error != ""u8 && err == default!){
        Ꮡt.Error(compilingˢ, expr, missingErrorˢ);
    } else 
    if (error != ""u8 && !strings.Contains(err.Error(), error)) {
        Ꮡt.Error(compilingˢ, expr, wrongErrorˢ, err.Error(), wantˢ, error);
    }
    return re;
}

public static void TestGoodCompile(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(goodRe); i++) {
        compileTest(Ꮡt, goodRe[i], ""u8);
    }
}

public static void TestBadCompile(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(badRe); i++) {
        compileTest(Ꮡt, badRe[i].re, badRe[i].err);
    }
}

internal static void matchTest(ж<testing.T> Ꮡt, ж<FindTest> Ꮡtest) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var test = ref Ꮡtest.DerefOrNull();

    var re = compileTest(Ꮡt, test.pat, ""u8);
    if (re == nil) {
        return;
    }
    var m = re.MatchString(test.text);
    if (m != (len(test.matches) > 0)) {
        Ꮡt.Errorf("MatchString failure on %s: %t should be %t"u8, Ꮡtest.OrTypedNil(), m, len(test.matches) > 0);
    }
    // now try bytes
    m = re.Match(slice<byte>(test.text));
    if (m != (len(test.matches) > 0)) {
        Ꮡt.Errorf("Match failure on %s: %t should be %t"u8, Ꮡtest.OrTypedNil(), m, len(test.matches) > 0);
    }
}

public static void TestMatch(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        matchTest(Ꮡt, Ꮡtest);
    }
}

internal static void matchFunctionTest(ж<testing.T> Ꮡt, ж<FindTest> Ꮡtest) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var test = ref Ꮡtest.DerefOrNull();

    var (m, err) = MatchString(test.pat, test.text);
    if (err == default!) {
        return;
    }
    if (m != (len(test.matches) > 0)) {
        Ꮡt.Errorf("Match failure on %s: %t should be %t"u8, Ꮡtest.OrTypedNil(), m, len(test.matches) > 0);
    }
}

public static void TestMatchFunction(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        matchFunctionTest(Ꮡt, Ꮡtest);
    }
}

internal static void copyMatchTest(ж<testing.T> Ꮡt, ж<FindTest> Ꮡtest) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var test = ref Ꮡtest.DerefOrNull();

    var re = compileTest(Ꮡt, test.pat, ""u8);
    if (re == nil) {
        return;
    }
    var m1 = re.MatchString(test.text);
    var m2 = re.Copy().MatchString(test.text);
    if (m1 != m2) {
        Ꮡt.Errorf("Copied Regexp match failure on %s: original gave %t; copy gave %t; should be %t"u8,
            Ꮡtest.OrTypedNil(), m1, m2, len(test.matches) > 0);
    }
}

public static void TestCopyMatch(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        copyMatchTest(Ꮡt, Ꮡtest);
    }
}

public partial struct ReplaceTest {
    internal @string pattern, replacement, input, output;
}

// Test empty input and/or replacement, with pattern that matches the empty string.
// Test empty input and/or replacement, with pattern that does not match the empty string.
// Multibyte characters -- verify that we don't try to match in the middle
// of a character.
// Start and end of a string.
// Other cases.
// Substitutions
// Substitution when subexpression isn't found
// Substitutions involving a (x){0}
internal static slice<ReplaceTest> replaceTests = new ReplaceTest[]{
    new(""u8, ""u8, ""u8, ""u8),
    new(""u8, "x"u8, ""u8, "x"u8),
    new(""u8, ""u8, "abc"u8, "abc"u8),
    new(""u8, "x"u8, "abc"u8, "xaxbxcx"u8),
    new("b"u8, ""u8, ""u8, ""u8),
    new("b"u8, "x"u8, ""u8, ""u8),
    new("b"u8, ""u8, "abc"u8, "ac"u8),
    new("b"u8, "x"u8, "abc"u8, "axc"u8),
    new("y"u8, ""u8, ""u8, ""u8),
    new("y"u8, "x"u8, ""u8, ""u8),
    new("y"u8, ""u8, "abc"u8, "abc"u8),
    new("y"u8, "x"u8, "abc"u8, "abc"u8),
    new("[a-c]*"u8, "x"u8, "\u65e5"u8, "x\u65e5x"u8),
    new("[^\u65e5]"u8, "x"u8, "abc\u65e5def"u8, "xxx\u65e5xxx"u8),
    new("^[a-c]*"u8, "x"u8, "abcdabc"u8, "xdabc"u8),
    new("[a-c]*$"u8, "x"u8, "abcdabc"u8, "abcdx"u8),
    new("^[a-c]*$"u8, "x"u8, "abcdabc"u8, "abcdabc"u8),
    new("^[a-c]*"u8, "x"u8, "abc"u8, "x"u8),
    new("[a-c]*$"u8, "x"u8, "abc"u8, "x"u8),
    new("^[a-c]*$"u8, "x"u8, "abc"u8, "x"u8),
    new("^[a-c]*"u8, "x"u8, "dabce"u8, "xdabce"u8),
    new("[a-c]*$"u8, "x"u8, "dabce"u8, "dabcex"u8),
    new("^[a-c]*$"u8, "x"u8, "dabce"u8, "dabce"u8),
    new("^[a-c]*"u8, "x"u8, ""u8, "x"u8),
    new("[a-c]*$"u8, "x"u8, ""u8, "x"u8),
    new("^[a-c]*$"u8, "x"u8, ""u8, "x"u8),
    new("^[a-c]+"u8, "x"u8, "abcdabc"u8, "xdabc"u8),
    new("[a-c]+$"u8, "x"u8, "abcdabc"u8, "abcdx"u8),
    new("^[a-c]+$"u8, "x"u8, "abcdabc"u8, "abcdabc"u8),
    new("^[a-c]+"u8, "x"u8, "abc"u8, "x"u8),
    new("[a-c]+$"u8, "x"u8, "abc"u8, "x"u8),
    new("^[a-c]+$"u8, "x"u8, "abc"u8, "x"u8),
    new("^[a-c]+"u8, "x"u8, "dabce"u8, "dabce"u8),
    new("[a-c]+$"u8, "x"u8, "dabce"u8, "dabce"u8),
    new("^[a-c]+$"u8, "x"u8, "dabce"u8, "dabce"u8),
    new("^[a-c]+"u8, "x"u8, ""u8, ""u8),
    new("[a-c]+$"u8, "x"u8, ""u8, ""u8),
    new("^[a-c]+$"u8, "x"u8, ""u8, ""u8),
    new("abc"u8, "def"u8, "abcdefg"u8, "defdefg"u8),
    new("bc"u8, "BC"u8, "abcbcdcdedef"u8, "aBCBCdcdedef"u8),
    new("abc"u8, ""u8, "abcdabc"u8, "d"u8),
    new("x"u8, "xXx"u8, "xxxXxxx"u8, "xXxxXxxXxXxXxxXxxXx"u8),
    new("abc"u8, "d"u8, ""u8, ""u8),
    new("abc"u8, "d"u8, "abc"u8, "d"u8),
    new(".+"u8, "x"u8, "abc"u8, "x"u8),
    new("[a-c]*"u8, "x"u8, "def"u8, "xdxexfx"u8),
    new("[a-c]+"u8, "x"u8, "abcbcdcdedef"u8, "xdxdedef"u8),
    new("[a-c]*"u8, "x"u8, "abcbcdcdedef"u8, "xdxdxexdxexfx"u8),
    new("a+"u8, "($0)"u8, "banana"u8, "b(a)n(a)n(a)"u8),
    new("a+"u8, "(${0})"u8, "banana"u8, "b(a)n(a)n(a)"u8),
    new("a+"u8, "(${0})$0"u8, "banana"u8, "b(a)an(a)an(a)a"u8),
    new("a+"u8, "(${0})$0"u8, "banana"u8, "b(a)an(a)an(a)a"u8),
    new("hello, (.+)"u8, "goodbye, ${1}"u8, "hello, world"u8, "goodbye, world"u8),
    new("hello, (.+)"u8, "goodbye, $1x"u8, "hello, world"u8, "goodbye, "u8),
    new("hello, (.+)"u8, "goodbye, ${1}x"u8, "hello, world"u8, "goodbye, worldx"u8),
    new("hello, (.+)"u8, "<$0><$1><$2><$3>"u8, "hello, world"u8, "<hello, world><world><><>"u8),
    new("hello, (?P<noun>.+)"u8, "goodbye, $noun!"u8, "hello, world"u8, "goodbye, world!"u8),
    new("hello, (?P<noun>.+)"u8, "goodbye, ${noun}"u8, "hello, world"u8, "goodbye, world"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "$x$x$x"u8, "hi"u8, "hihihi"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "$x$x$x"u8, "bye"u8, "byebyebye"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "$xyz"u8, "hi"u8, ""u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "${x}yz"u8, "hi"u8, "hiyz"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "hello $$x"u8, "hi"u8, "hello $x"u8),
    new("a+"u8, "${oops"u8, "aaa"u8, "${oops"u8),
    new("a+"u8, "$$"u8, "aaa"u8, "$"u8),
    new("a+"u8, "$"u8, "aaa"u8, "$"u8),
    new("(x)?"u8, "$1"u8, "123"u8, "123"u8),
    new("abc"u8, "$1"u8, "123"u8, "123"u8),
    new("(a)(b){0}(c)"u8, ".$1|$3."u8, "xacxacx"u8, "x.a|c.x.a|c.x"u8),
    new("(a)(((b))){0}c"u8, ".$1."u8, "xacxacx"u8, "x.a.x.a.x"u8),
    new("((a(b){0}){3}){5}(h)"u8, "y caramb$2"u8, "say aaaaaaaaaaaaaaaah"u8, "say ay caramba"u8),
    new("((a(b){0}){3}){5}h"u8, "y caramb$2"u8, "say aaaaaaaaaaaaaaaah"u8, "say ay caramba"u8)
}.slice();

// Substitutions
internal static slice<ReplaceTest> replaceLiteralTests = new ReplaceTest[]{
    new("a+"u8, "($0)"u8, "banana"u8, "b($0)n($0)n($0)"u8),
    new("a+"u8, "(${0})"u8, "banana"u8, "b(${0})n(${0})n(${0})"u8),
    new("a+"u8, "(${0})$0"u8, "banana"u8, "b(${0})$0n(${0})$0n(${0})$0"u8),
    new("a+"u8, "(${0})$0"u8, "banana"u8, "b(${0})$0n(${0})$0n(${0})$0"u8),
    new("hello, (.+)"u8, "goodbye, ${1}"u8, "hello, world"u8, "goodbye, ${1}"u8),
    new("hello, (?P<noun>.+)"u8, "goodbye, $noun!"u8, "hello, world"u8, "goodbye, $noun!"u8),
    new("hello, (?P<noun>.+)"u8, "goodbye, ${noun}"u8, "hello, world"u8, "goodbye, ${noun}"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "$x$x$x"u8, "hi"u8, "$x$x$x"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "$x$x$x"u8, "bye"u8, "$x$x$x"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "$xyz"u8, "hi"u8, "$xyz"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "${x}yz"u8, "hi"u8, "${x}yz"u8),
    new("(?P<x>hi)|(?P<x>bye)"u8, "hello $$x"u8, "hi"u8, "hello $$x"u8),
    new("a+"u8, "${oops"u8, "aaa"u8, "${oops"u8),
    new("a+"u8, "$$"u8, "aaa"u8, "$$"u8),
    new("a+"u8, "$"u8, "aaa"u8, "$"u8)
}.slice();

public partial struct ReplaceFuncTest {
    internal @string pattern;
    internal Func<@string, @string> replacement;
    internal @string input, output;
}

internal static slice<ReplaceFuncTest> replaceFuncTests = new ReplaceFuncTest[]{
    new("[a-c]"u8, (@string s) => "x"u8 + s + "y"u8, "defabcdef"u8, "defxayxbyxcydef"u8),
    new("[a-c]+"u8, (@string s) => "x"u8 + s + "y"u8, "defabcdef"u8, "defxabcydef"u8),
    new("[a-c]*"u8, (@string s) => "x"u8 + s + "y"u8, "defabcdef"u8, "xydxyexyfxabcydxyexyfxy"u8)
}.slice();

public static void TestReplaceAll(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in replaceTests) {
        var (re, err) = Compile(tc.pattern);
        if (err != default!) {
            Ꮡt.Errorf("Unexpected error compiling %q: %v"u8, tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllString(tc.input, tc.replacement);
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceAllString(%q,%q) = %q; want %q"u8,
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
        // now try bytes
        actual = ((@string)re.ReplaceAll(slice<byte>(tc.input), slice<byte>(tc.replacement)));
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceAll(%q,%q) = %q; want %q"u8,
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
    }
}

public static void TestReplaceAllLiteral(ж<testing.T> Ꮡt) {
    // Run ReplaceAll tests that do not have $ expansions.
    foreach (var (_, tc) in replaceTests) {
        if (strings.Contains(tc.replacement, "$"u8)) {
            continue;
        }
        var (re, err) = Compile(tc.pattern);
        if (err != default!) {
            Ꮡt.Errorf("Unexpected error compiling %q: %v"u8, tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllLiteralString(tc.input, tc.replacement);
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceAllLiteralString(%q,%q) = %q; want %q"u8,
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
        // now try bytes
        actual = ((@string)re.ReplaceAllLiteral(slice<byte>(tc.input), slice<byte>(tc.replacement)));
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceAllLiteral(%q,%q) = %q; want %q"u8,
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
    }
    // Run literal-specific tests.
    foreach (var (_, tc) in replaceLiteralTests) {
        var (re, err) = Compile(tc.pattern);
        if (err != default!) {
            Ꮡt.Errorf("Unexpected error compiling %q: %v"u8, tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllLiteralString(tc.input, tc.replacement);
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceAllLiteralString(%q,%q) = %q; want %q"u8,
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
        // now try bytes
        actual = ((@string)re.ReplaceAllLiteral(slice<byte>(tc.input), slice<byte>(tc.replacement)));
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceAllLiteral(%q,%q) = %q; want %q"u8,
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
    }
}

public static void TestReplaceAllFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in replaceFuncTests) {
        ref var tc = ref heap(new ReplaceFuncTest(), out var Ꮡtc);
        tc = vᴛ1;

        var (re, err) = Compile(tc.pattern);
        if (err != default!) {
            Ꮡt.Errorf("Unexpected error compiling %q: %v"u8, tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllStringFunc(tc.input, tc.replacement);
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceFunc(%q,fn) = %q; want %q"u8,
                tc.pattern, tc.input, actual, tc.output);
        }
        // now try bytes
        var tcʗ1 = tc;
        actual = ((@string)re.ReplaceAllFunc(slice<byte>(tc.input), (slice<byte> s) => slice<byte>(tcʗ1.replacement(((@string)s)))));
        if (actual != tc.output) {
            Ꮡt.Errorf("%q.ReplaceFunc(%q,fn) = %q; want %q"u8,
                tc.pattern, tc.input, actual, tc.output);
        }
    }
}

public partial struct MetaTest {
    internal @string pattern, output, literal;
    internal bool isLiteral;
}

// has meta but no operator
// has escaped operators and real operators
internal static slice<MetaTest> metaTests = new MetaTest[]{
    new(@""u8, @""u8, @""u8, true),
    new(@"foo"u8, @"foo"u8, @"foo"u8, true),
    new(@"日本語+"u8, @"日本語\+"u8, @"日本語"u8, false),
    new(@"foo\.\$"u8, @"foo\\\.\\\$"u8, @"foo.$"u8, true),
    new(@"foo.\$"u8, @"foo\.\\\$"u8, @"foo"u8, false),
    new(@"!@#$%^&*()_+-=[{]}\|,<.>/?~"u8, @"!@#\$%\^&\*\(\)_\+-=\[\{\]\}\\\|,<\.>/\?~"u8, @"!@#"u8, false)
}.slice();

// See golang.org/issue/11175.
// output is unused.
internal static slice<MetaTest> literalPrefixTests = new MetaTest[]{
    new(@"^0^0$"u8, @""u8, @"0"u8, false),
    new(@"^0^"u8, @""u8, @""u8, false),
    new(@"^0$"u8, @""u8, @"0"u8, true),
    new(@"$0^"u8, @""u8, @""u8, false),
    new(@"$0$"u8, @""u8, @""u8, false),
    new(@"^^0$$"u8, @""u8, @""u8, false),
    new(@"^$^$"u8, @""u8, @""u8, false),
    new(@"$$0^^"u8, @""u8, @""u8, false),
    new(@"a\x{fffd}b"u8, @""u8, @"a"u8, false),
    new(@"\x{fffd}b"u8, @""u8, @""u8, false),
    new("\ufffd"u8, @""u8, @""u8, false)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xyzˢ = "xyz"u8;
internal static readonly @string abcxyzdefˢ = "abcxyzdef"u8;

public static void TestQuoteMeta(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in metaTests) {
        // Verify that QuoteMeta returns the expected string.
        @string quoted = QuoteMeta(tc.pattern);
        if (quoted != tc.output) {
            Ꮡt.Errorf("QuoteMeta(`%s`) = `%s`; want `%s`"u8,
                tc.pattern, quoted, tc.output);
            continue;
        }
        // Verify that the quoted string is in fact treated as expected
        // by Compile -- i.e. that it matches the original, unquoted string.
        if (tc.pattern != ""u8) {
            var (re, err) = Compile(quoted);
            if (err != default!) {
                Ꮡt.Errorf("Unexpected error compiling QuoteMeta(`%s`): %v"u8, tc.pattern, err);
                continue;
            }
            @string src = "abc"u8 + tc.pattern + "def"u8;
            @string repl = xyzˢ;
            @string replaced = re.ReplaceAllString(src, repl);
            @string expected = abcxyzdefˢ;
            if (replaced != expected) {
                Ꮡt.Errorf("QuoteMeta(`%s`).Replace(`%s`,`%s`) = `%s`; want `%s`"u8,
                    tc.pattern, src, repl, replaced, expected);
            }
        }
    }
}

public static void TestLiteralPrefix(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tc) in appendꓸꓸꓸ(metaTests, literalPrefixTests)) {
        // Literal method needs to scan the pattern.
        var re = MustCompile(tc.pattern);
        var (str, complete) = re.LiteralPrefix();
        if (complete != tc.isLiteral) {
            Ꮡt.Errorf("LiteralPrefix(`%s`) = %t; want %t"u8, tc.pattern, complete, tc.isLiteral);
        }
        if (str != tc.literal) {
            Ꮡt.Errorf("LiteralPrefix(`%s`) = `%s`; want `%s`"u8, tc.pattern, str, tc.literal);
        }
    }
}

internal partial struct subexpIndex {
    internal @string name;
    internal nint index;
}

internal partial struct subexpCase {
    internal @string input;
    internal nint num;
    internal slice<@string> names;
    internal slice<subexpIndex> indices;
}

internal static slice<subexpIndex> emptySubexpIndices = new subexpIndex[]{new(""u8, -1), new("missing"u8, -1)}.slice();

internal static slice<subexpCase> subexpCases = new subexpCase[]{
    new(@""u8, 0, default!, emptySubexpIndices),
    new(@".*"u8, 0, default!, emptySubexpIndices),
    new(@"abba"u8, 0, default!, emptySubexpIndices),
    new(@"ab(b)a"u8, 1, new @string[]{""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"ab(.*)a"u8, 1, new @string[]{""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"(.*)ab(.*)a"u8, 2, new @string[]{""u8, ""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"(.*)(ab)(.*)a"u8, 3, new @string[]{""u8, ""u8, ""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"(.*)((a)b)(.*)a"u8, 4, new @string[]{""u8, ""u8, ""u8, ""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"(.*)(\(ab)(.*)a"u8, 3, new @string[]{""u8, ""u8, ""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"(.*)(\(a\)b)(.*)a"u8, 3, new @string[]{""u8, ""u8, ""u8, ""u8}.slice(), emptySubexpIndices),
    new(@"(?P<foo>.*)(?P<bar>(a)b)(?P<foo>.*)a"u8, 4, new @string[]{""u8, "foo"u8, "bar"u8, ""u8, "foo"u8}.slice(), new subexpIndex[]{new(""u8, -1), new("missing"u8, -1), new("foo"u8, 1), new("bar"u8, 2)}.slice())
}.slice();

public static void TestSubexp(ж<testing.T> Ꮡt) {
    foreach (var (_, c) in subexpCases) {
        var re = MustCompile(c.input);
        nint n = re.NumSubexp();
        if (n != c.num) {
            Ꮡt.Errorf("%q: NumSubexp = %d, want %d"u8, c.input, n, c.num);
            continue;
        }
        var names = re.SubexpNames();
        if (len(names) != 1 + n) {
            Ꮡt.Errorf("%q: len(SubexpNames) = %d, want %d"u8, c.input, len(names), n);
            continue;
        }
        if (c.names != default!) {
            for (nint i = 0; i < 1 + n; i++) {
                if (names[i] != c.names[i]) {
                    Ꮡt.Errorf("%q: SubexpNames[%d] = %q, want %q"u8, c.input, i, names[i], c.names[i]);
                }
            }
        }
        foreach (var (_, subexp) in c.indices) {
            nint index = re.SubexpIndex(subexp.name);
            if (index != subexp.index) {
                Ꮡt.Errorf("%q: SubexpIndex(%q) = %d, want %d"u8, c.input, subexp.name, index, subexp.index);
            }
        }
    }
}


partial struct splitTestsᴛ1 /*dyn*/ {
    internal @string s;
    internal @string r;
    internal nint n;
    internal slice<@string> @out;
}
internal static slice<splitTestsᴛ1> splitTests = new splitTestsᴛ1[]{
    new("foo:and:bar"u8, ":"u8, -1, new @string[]{"foo"u8, "and"u8, "bar"u8}.slice()),
    new("foo:and:bar"u8, ":"u8, 1, new @string[]{"foo:and:bar"u8}.slice()),
    new("foo:and:bar"u8, ":"u8, 2, new @string[]{"foo"u8, "and:bar"u8}.slice()),
    new("foo:and:bar"u8, "foo"u8, -1, new @string[]{""u8, ":and:bar"u8}.slice()),
    new("foo:and:bar"u8, "bar"u8, -1, new @string[]{"foo:and:"u8, ""u8}.slice()),
    new("foo:and:bar"u8, "baz"u8, -1, new @string[]{"foo:and:bar"u8}.slice()),
    new("baabaab"u8, "a"u8, -1, new @string[]{"b"u8, ""u8, "b"u8, ""u8, "b"u8}.slice()),
    new("baabaab"u8, "a*"u8, -1, new @string[]{"b"u8, "b"u8, "b"u8}.slice()),
    new("baabaab"u8, "ba*"u8, -1, new @string[]{""u8, ""u8, ""u8, ""u8}.slice()),
    new("foobar"u8, "f*b*"u8, -1, new @string[]{""u8, "o"u8, "o"u8, "a"u8, "r"u8}.slice()),
    new("foobar"u8, "f+.*b+"u8, -1, new @string[]{""u8, "ar"u8}.slice()),
    new("foobooboar"u8, "o{2}"u8, -1, new @string[]{"f"u8, "b"u8, "boar"u8}.slice()),
    new("a,b,c,d,e,f"u8, ","u8, 3, new @string[]{"a"u8, "b"u8, "c,d,e,f"u8}.slice()),
    new("a,b,c,d,e,f"u8, ","u8, 0, default!),
    new(","u8, ","u8, -1, new @string[]{""u8, ""u8}.slice()),
    new(",,,"u8, ","u8, -1, new @string[]{""u8, ""u8, ""u8, ""u8}.slice()),
    new(""u8, ","u8, -1, new @string[]{""u8}.slice()),
    new(""u8, ".*"u8, -1, new @string[]{""u8}.slice()),
    new(""u8, ".+"u8, -1, new @string[]{""u8}.slice()),
    new(""u8, ""u8, -1, new @string[]{}.slice()),
    new("foobar"u8, ""u8, -1, new @string[]{"f"u8, "o"u8, "o"u8, "b"u8, "a"u8, "r"u8}.slice()),
    new("abaabaccadaaae"u8, "a*"u8, 5, new @string[]{""u8, "b"u8, "b"u8, "c"u8, "cadaaae"u8}.slice()),
    new(":x:y:z:"u8, ":"u8, -1, new @string[]{""u8, "x"u8, "y"u8, "z"u8, ""u8}.slice())
}.slice();

public static void TestSplit(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in splitTests) {
        var (re, err) = Compile(test.r);
        if (err != default!) {
            Ꮡt.Errorf("#%d: %q: compile error: %s"u8, i, test.r, err.Error());
            continue;
        }
        var split = re.Split(test.s, test.n);
        if (!slices.Equal<slice<@string>, @string>(split, test.@out)) {
            Ꮡt.Errorf("#%d: %q: got %q; want %q"u8, i, test.r, split, test.@out);
        }
        if (QuoteMeta(test.r) == test.r) {
            var strsplit = strings.SplitN(test.s, test.r, test.n);
            if (!slices.Equal<slice<@string>, @string>(split, strsplit)) {
                Ꮡt.Errorf("#%d: Split(%q, %q, %d): regexp vs strings mismatch\nregexp=%q\nstrings=%q"u8, i, test.s, test.r, test.n, split, strsplit);
            }
        }
    }
}

internal partial struct TestParseAndCompile_type /*dyn*/ {
    internal syntax.Flags reFlags;
    internal bool expMatch;
}

// The following sequence of Match calls used to panic. See issue #12980.
public static void TestParseAndCompile(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string expr = "a$"u8;
    @string s = "a\nb"u8;
    foreach (var (i, tc) in new TestParseAndCompile_type[]{
        new((syntax.Flags)(syntax.Perl | syntax.OneLine), false),
        new((syntax.Flags)(syntax.Perl & ~syntax.OneLine), true)
    }.slice()) {
        var (parsed, err) = syntax.Parse(expr, tc.reFlags);
        if (err != default!) {
            Ꮡt.Fatalf("%d: parse: %v"u8, i, err);
        }
        (var re, err) = Compile(parsed.String());
        if (err != default!) {
            Ꮡt.Fatalf("%d: compile: %v"u8, i, err);
        }
        {
            var Δmatch = re.MatchString(s); if (Δmatch != tc.expMatch) {
                Ꮡt.Errorf("%d: %q.MatchString(%q)=%t; expected=%t"u8, i, re.OrTypedNil(), s, Δmatch, tc.expMatch);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x11000Y11000ˢ = @"^x{1,1000}y{1,1000}$"u8;

// Check that one-pass cutoff does trigger.
public static void TestOnePassCutoff(ж<testing.T> Ꮡt) {
    var (re, err) = syntax.Parse(x11000Y11000ˢ, syntax.Perl);
    if (err != default!) {
        Ꮡt.Fatalf("parse: %v"u8, err);
    }
    (var p, err) = syntax.Compile(re.Simplify());
    if (err != default!) {
        Ꮡt.Fatalf("compile: %v"u8, err);
    }
    if (compileOnePass(ref (p).DerefOrNull()) != nil) {
        Ꮡt.Fatalf("makeOnePass succeeded; wanted nil"u8);
    }
}

// Check that the same machine can be used with the standard matcher
// and then the backtracker when there are no captures.
public static void TestSwitchBacktrack(ж<testing.T> Ꮡt) {
    var re = MustCompile(@"a|b"u8);
    var @long = new slice<byte>(maxBacktrackVector + 1);
    // The following sequence of Match calls used to panic. See issue #10319.
    re.Match(@long); // triggers standard matcher
    re.Match(@long[..1]); // triggers backtracker
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aaabbˢ = "aaabb"u8;

public static void BenchmarkFind(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var re = MustCompile("a+b+"u8);
    @string wantSubs = aaabbˢ;
    var s = slice<byte>("acbb" + wantSubs + "dd");
    b.StartTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var subs = re.Find(s);
        if (((sstring)subs) != wantSubs) {
            Ꮡb.Fatalf("Find(%q) = %q; want %q"u8, s, subs, wantSubs);
        }
    }
}

public static void BenchmarkFindAllNoMatches(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var re = MustCompile("a+b+"u8);
    var s = slice<byte>("acddee"u8);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var all = re.FindAll(s, -1);
        if (all != default!) {
            Ꮡb.Fatalf("FindAll(%q) = %q; want nil"u8, s, all);
        }
    }
}

public static void BenchmarkFindString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var re = MustCompile("a+b+"u8);
    @string wantSubs = aaabbˢ;
    @string s = "acbb"u8 + wantSubs + "dd"u8;
    b.StartTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        @string subs = re.FindString(s);
        if (subs != wantSubs) {
            Ꮡb.Fatalf("FindString(%q) = %q; want %q"u8, s, subs, wantSubs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aABBˢ = "a(a+b+)b"u8;
internal static readonly object aabˢ = (@string)"aab"u8;

public static void BenchmarkFindSubmatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var re = MustCompile(aABBˢ);
    @string wantSubs = aaabbˢ;
    var s = slice<byte>("acbb" + wantSubs + "dd");
    b.StartTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var subs = re.FindSubmatch(s);
        if (((sstring)subs[0]) != wantSubs) {
            Ꮡb.Fatalf("FindSubmatch(%q)[0] = %q; want %q"u8, s, subs[0], wantSubs);
        }
        if (((sstring)subs[1]) != "aab"u8) {
            Ꮡb.Fatalf("FindSubmatch(%q)[1] = %q; want %q"u8, s, subs[1], aabˢ);
        }
    }
}

public static void BenchmarkFindStringSubmatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var re = MustCompile(aABBˢ);
    @string wantSubs = aaabbˢ;
    @string s = "acbb"u8 + wantSubs + "dd"u8;
    b.StartTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var subs = re.FindStringSubmatch(s);
        if (subs[0] != wantSubs) {
            Ꮡb.Fatalf("FindStringSubmatch(%q)[0] = %q; want %q"u8, s, subs[0], wantSubs);
        }
        if (subs[1] != "aab") {
            Ꮡb.Fatalf("FindStringSubmatch(%q)[1] = %q; want %q"u8, s, subs[1], aabˢ);
        }
    }
}

public static void BenchmarkLiteral(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = strings.Repeat("x"u8, 50) + "y"u8;
    b.StopTimer();
    var re = MustCompile("y"u8);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!"u8);
        }
    }
}

public static void BenchmarkNotLiteral(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = strings.Repeat("x"u8, 50) + "y"u8;
    b.StopTimer();
    var re = MustCompile(".y"u8);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string xxxxˢ = "xxxx"u8;
internal static readonly @string abcdwˢ = "[abcdw]"u8;

public static void BenchmarkMatchClass(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    @string x = strings.Repeat(xxxxˢ, 20) + "w"u8;
    var re = MustCompile(abcdwˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bbbbˢ = "bbbb"u8;

public static void BenchmarkMatchClass_InRange(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    // 'b' is between 'a' and 'c', so the charclass
    // range checking is no help here.
    @string x = strings.Repeat(bbbbˢ, 20) + "c"u8;
    var re = MustCompile("[ac]"u8);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cjrwˢ = "[cjrw]"u8;

public static void BenchmarkReplaceAll(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = "abcdefghijklmnopqrstuvwxyz"u8;
    b.StopTimer();
    var re = MustCompile(cjrwˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.ReplaceAllString(x, ""u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zbcDEˢ = "^zbc(d|e)"u8;

public static void BenchmarkAnchoredLiteralShortNonMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    var re = MustCompile(zbcDEˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

public static void BenchmarkAnchoredLiteralLongNonMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    for (nint i = 0; i < 15; i++) {
        x = appendꓸꓸꓸ(x, x);
    }
    var re = MustCompile(zbcDEˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bcDEˢ = "^.bc(d|e)"u8;

public static void BenchmarkAnchoredShortMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    var re = MustCompile(bcDEˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

public static void BenchmarkAnchoredLongMatch(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    for (nint i = 0; i < 15; i++) {
        x = appendꓸꓸꓸ(x, x);
    }
    var re = MustCompile(bcDEˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bcDEˢ2 = "^.bc(d|e)*$"u8;

public static void BenchmarkOnePassShortA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcddddddeeeededd"u8);
    var re = MustCompile(bcDEˢ2);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bcDEˢ3 = ".bc(d|e)*$"u8;

public static void BenchmarkNotOnePassShortA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcddddddeeeededd"u8);
    var re = MustCompile(bcDEˢ3);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bcDEˢ4 = "^.bc(?:d|e)*$"u8;

public static void BenchmarkOnePassShortB(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcddddddeeeededd"u8);
    var re = MustCompile(bcDEˢ4);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bcDEˢ5 = ".bc(?:d|e)*$"u8;

public static void BenchmarkNotOnePassShortB(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcddddddeeeededd"u8);
    var re = MustCompile(bcDEˢ5);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

public static void BenchmarkOnePassLongPrefix(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    var re = MustCompile("^abcdefghijklmnopqrstuvwxyz.*$"u8);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

public static void BenchmarkOnePassLongNotPrefix(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    var re = MustCompile("^.bcdefghijklmnopqrstuvwxyz.*$"u8);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBaRBazˢ = "foo (ba+r)? baz"u8;

public static void BenchmarkMatchParallelShared(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = slice<byte>("this is a long line that contains foo bar baz"u8);
    var re = MustCompile(fooBaRBazˢ);
    b.ResetTimer();
    var reʗ1 = re;
    var xʗ1 = x;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            reʗ1.Match(xʗ1);
        }
    });
}

public static void BenchmarkMatchParallelCopied(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = slice<byte>("this is a long line that contains foo bar baz"u8);
    var re = MustCompile(fooBaRBazˢ);
    b.ResetTimer();
    var reʗ1 = re;
    var xʗ1 = x;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var reΔ1 = reʗ1.Copy();
        while (pb.Next()) {
            reΔ1.Match(xʗ1);
        }
    });
}

internal static @string sink;

public static void BenchmarkQuoteMetaAll(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var specials = new slice<byte>(0);
    for (var i = (byte)0; i < utf8.RuneSelf; i++) {
        if (special(i)) {
            specials = append(specials, i);
        }
    }
    @string s = ((@string)specials);
    b.SetBytes((int64)len(s));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sink = QuoteMeta(s);
    }
}

public static void BenchmarkQuoteMetaNone(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = "abcdefghijklmnopqrstuvwxyz"u8;
    b.SetBytes((int64)len(s));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sink = QuoteMeta(s);
    }
}


partial struct compileBenchDataᴛ1 /*dyn*/ {
    internal @string name, re;
}
internal static slice<compileBenchDataᴛ1> compileBenchData = new compileBenchDataᴛ1[]{
    new("Onepass"u8, @"^a.[l-nA-Cg-j]?e$"u8),
    new("Medium"u8, @"^((a|b|[d-z0-9])*(日){4,5}.)+$"u8),
    new("Hard"u8, strings.Repeat(@"((abc)*|"u8, 50) + strings.Repeat(@")"u8, 50))
}.slice();

public static void BenchmarkCompile(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in compileBenchData) {
        ref var data = ref heap(new compileBenchDataᴛ1(), out var Ꮡdata);
        data = vᴛ1;

        var dataʗ1 = data;
        Ꮡb.Run(data.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                {
                    var (_, err) = Compile(dataʗ1.re); if (err != default!) {
                        bΔ1.Fatal(err);
                    }
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBCDˢ = "a.*b.*c.*d"u8;
internal static readonly @string abcdefghijklmnˢ = "abcdefghijklmn"u8;

public static void TestDeepEqual(ж<testing.T> Ꮡt) {
    var re1 = MustCompile(aBCDˢ);
    var re2 = MustCompile(aBCDˢ);
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        // has always been true, since Go 1.
        Ꮡt.Errorf("DeepEqual(re1, re2) = false, want true"u8);
    }
    re1.MatchString(abcdefghijklmnˢ);
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        Ꮡt.Errorf("DeepEqual(re1, re2) = false, want true"u8);
    }
    re2.MatchString(abcdefghijklmnˢ);
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        Ꮡt.Errorf("DeepEqual(re1, re2) = false, want true"u8);
    }
    re2.MatchString(strings.Repeat(abcdefghijklmnˢ, 100));
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        Ꮡt.Errorf("DeepEqual(re1, re2) = false, want true"u8);
    }
}


partial struct minInputLenTestsᴛ1 /*dyn*/ {
    public @string Regexp;
    internal nint min;
}
internal static slice<minInputLenTestsᴛ1> minInputLenTests = new minInputLenTestsᴛ1[]{
    new(@""u8, 0),
    new(@"a"u8, 1),
    new(@"aa"u8, 2),
    new(@"(aa)a"u8, 3),
    new(@"(?:aa)a"u8, 3),
    new(@"a?a"u8, 1),
    new(@"(aaa)|(aa)"u8, 2),
    new(@"(aa)+a"u8, 3),
    new(@"(aa)*a"u8, 1),
    new(@"(aa){3,5}"u8, 6),
    new(@"[a-z]"u8, 1),
    new(@"日"u8, 3)
}.slice();

public static void TestMinInputLen(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in minInputLenTests) {
        var (re, _) = syntax.Parse(tt.Regexp, syntax.Perl);
        nint m = minInputLen(ref (re).DerefOrNull());
        if (m != tt.min) {
            Ꮡt.Errorf("regexp %#q has minInputLen %d, should be %d"u8, tt.Regexp, m, tt.min);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPatternˢ = "invalid pattern"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

public static void TestUnmarshalText(ж<testing.T> Ꮡt) {
    var unmarshaled = @new<global::go.regexp_package.Regexp>();
    foreach (var (i, _) in goodRe) {
        var re = compileTest(Ꮡt, goodRe[i], ""u8);
        var (marshaled, err) = re.MarshalText();
        if (err != default!) {
            Ꮡt.Errorf("regexp %#q failed to marshal: %s"u8, re.OrTypedNil(), err);
            continue;
        }
        {
            var errΔ1 = unmarshaled.UnmarshalText(marshaled); if (errΔ1 != default!) {
                Ꮡt.Errorf("regexp %#q failed to unmarshal: %s"u8, re.OrTypedNil(), errΔ1);
                continue;
            }
        }
        if (unmarshaled.String() != goodRe[i]) {
            Ꮡt.Errorf("UnmarshalText returned unexpected value: %s"u8, unmarshaled.String());
        }
        var buf = new slice<byte>(4, 32);
        (var marshalAppend, err) = re.AppendText(buf);
        if (err != default!) {
            Ꮡt.Errorf("regexp %#q failed to marshal: %s"u8, re.OrTypedNil(), err);
            continue;
        }
        marshalAppend = marshalAppend[4..];
        {
            var errΔ2 = unmarshaled.UnmarshalText(marshalAppend); if (errΔ2 != default!) {
                Ꮡt.Errorf("regexp %#q failed to unmarshal: %s"u8, re.OrTypedNil(), errΔ2);
                continue;
            }
        }
        if (unmarshaled.String() != goodRe[i]) {
            Ꮡt.Errorf("UnmarshalText returned unexpected value: %s"u8, unmarshaled.String());
        }
    }
    Ꮡt.Run(invalidPatternˢ, (ж<testing.T> tΔ1) => {
        var re = @new<global::go.regexp_package.Regexp>();
        var err = re.UnmarshalText(slice<byte>(@"\"u8));
        if (err == default!) {
            tΔ1.Error(unexpectedSuccessˢ);
        }
    });
}

} // end regexp_internal_test_package
