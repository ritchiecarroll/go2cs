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

internal static slice<@string> goodRe = slice<@string>([
    @"",
    @".",
    @"^.$",
    @"a",
    @"a*",
    @"a+",
    @"a?",
    @"a|b",
    @"a*|b*",
    @"(a*|b)(c*|d)",
    @"[a-z]",
    @"[a-abc-c\-\]\[]",
    @"[a-z]+",
    @"[abc]",
    @"[^1234]",
    @"[^\n]",
    @"\!\\"
]);

internal partial struct stringError {
    internal @string re;
    internal @string err;
}

internal static slice<stringError> badRe = slice<stringError>([
    new(@"*", "missing argument to repetition operator: `*`"),
    new(@"+", "missing argument to repetition operator: `+`"),
    new(@"?", "missing argument to repetition operator: `?`"),
    new(@"(abc", "missing closing ): `(abc`"),
    new(@"abc)", "unexpected ): `abc)`"),
    new(@"x[a-z", "missing closing ]: `[a-z`"),
    new(@"[z-a]", "invalid character class range: `z-a`"),
    new(@"abc\", "trailing backslash at end of expression"),
    new(@"a**", "invalid nested repetition operator: `**`"),
    new(@"a*+", "invalid nested repetition operator: `*+`"),
    new(@"\x", "invalid escape sequence: `\\x`"),
    new(strings.Repeat(@"\pL", 27000), "expression too large")
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object compilingˢ = (@string)"compiling `"u8;
internal static readonly object unexpectedErrorˢ = (@string)"`; unexpected error: "u8;
internal static readonly object missingErrorˢ = (@string)"`; missing error"u8;
internal static readonly object wrongErrorˢ = (@string)"`; wrong error: "u8;
internal static readonly object wantˢ = (@string)"; want "u8;

internal static ж<global::go.regexp_package.Regexp> compileTest(ж<testing.T> t, @string expr, @string error) {
    var (re, err) = Compile(expr);
    if (error == ""u8 && err != nil) {
        t.Error(compilingˢ, expr, unexpectedErrorˢ, err.Error());
    }
    if (error != ""u8 && err == nil) {
        t.Error(compilingˢ, expr, missingErrorˢ);
    } else if (error != ""u8 && !strings.Contains(err.Error(), error)) {
        t.Error(compilingˢ, expr, wrongErrorˢ, err.Error(), wantˢ, error);
    }
    return re;
}

public static void TestGoodCompile(ж<testing.T> t) {
    for (nint i = 0; i < len(goodRe); i++) {
        compileTest(t, goodRe[i], ""u8);
    }
}

public static void TestBadCompile(ж<testing.T> t) {
    for (nint i = 0; i < len(badRe); i++) {
        compileTest(t, badRe[i].re, badRe[i].err);
    }
}

internal static void matchTest(ж<testing.T> t, ж<FindTest> Ꮡtest) {
    ref var test = ref Ꮡtest.DerefOrNull();

    var re = compileTest(t, test.pat, ""u8);
    if (re == nil) {
        return;
    }
    var m = re.MatchString(test.text);
    if (m != (len(test.matches) > 0)) {
        t.Errorf("MatchString failure on %s: %t should be %t", Ꮡtest.OrTypedNil(), m, len(test.matches) > 0);
    }
    // now try bytes
    m = re.Match(slice<byte>(test.text));
    if (m != (len(test.matches) > 0)) {
        t.Errorf("Match failure on %s: %t should be %t", Ꮡtest.OrTypedNil(), m, len(test.matches) > 0);
    }
}

public static void TestMatch(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        matchTest(t, Ꮡtest);
    }
}

internal static void matchFunctionTest(ж<testing.T> t, ж<FindTest> Ꮡtest) {
    ref var test = ref Ꮡtest.DerefOrNull();

    var (m, err) = MatchString(test.pat, test.text);
    if (err == nil) {
        return;
    }
    if (m != (len(test.matches) > 0)) {
        t.Errorf("Match failure on %s: %t should be %t", Ꮡtest.OrTypedNil(), m, len(test.matches) > 0);
    }
}

public static void TestMatchFunction(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        matchFunctionTest(t, Ꮡtest);
    }
}

internal static void copyMatchTest(ж<testing.T> t, ж<FindTest> Ꮡtest) {
    ref var test = ref Ꮡtest.DerefOrNull();

    var re = compileTest(t, test.pat, ""u8);
    if (re == nil) {
        return;
    }
    var m1 = re.MatchString(test.text);
    var m2 = re.Copy().MatchString(test.text);
    if (m1 != m2) {
        t.Errorf("Copied Regexp match failure on %s: original gave %t; copy gave %t; should be %t",
            Ꮡtest.OrTypedNil(), m1, m2, len(test.matches) > 0);
    }
}

public static void TestCopyMatch(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        copyMatchTest(t, Ꮡtest);
    }
}

public partial struct ReplaceTest {
    internal @string pattern, replacement, input, output;
}

internal static slice<ReplaceTest> replaceTests = slice<ReplaceTest>([
    // Test empty input and/or replacement, with pattern that matches the empty string.
    new("", "", "", ""),
    new("", "x", "", "x"),
    new("", "", "abc", "abc"),
    new("", "x", "abc", "xaxbxcx"),

    // Test empty input and/or replacement, with pattern that does not match the empty string.
    new("b", "", "", ""),
    new("b", "x", "", ""),
    new("b", "", "abc", "ac"),
    new("b", "x", "abc", "axc"),
    new("y", "", "", ""),
    new("y", "x", "", ""),
    new("y", "", "abc", "abc"),
    new("y", "x", "abc", "abc"),

    // Multibyte characters -- verify that we don't try to match in the middle
    // of a character.
    new("[a-c]*", "x", "\u65e5", "x\u65e5x"),
    new("[^\u65e5]", "x", "abc\u65e5def", "xxx\u65e5xxx"),

    // Start and end of a string.
    new("^[a-c]*", "x", "abcdabc", "xdabc"),
    new("[a-c]*$", "x", "abcdabc", "abcdx"),
    new("^[a-c]*$", "x", "abcdabc", "abcdabc"),
    new("^[a-c]*", "x", "abc", "x"),
    new("[a-c]*$", "x", "abc", "x"),
    new("^[a-c]*$", "x", "abc", "x"),
    new("^[a-c]*", "x", "dabce", "xdabce"),
    new("[a-c]*$", "x", "dabce", "dabcex"),
    new("^[a-c]*$", "x", "dabce", "dabce"),
    new("^[a-c]*", "x", "", "x"),
    new("[a-c]*$", "x", "", "x"),
    new("^[a-c]*$", "x", "", "x"),

    new("^[a-c]+", "x", "abcdabc", "xdabc"),
    new("[a-c]+$", "x", "abcdabc", "abcdx"),
    new("^[a-c]+$", "x", "abcdabc", "abcdabc"),
    new("^[a-c]+", "x", "abc", "x"),
    new("[a-c]+$", "x", "abc", "x"),
    new("^[a-c]+$", "x", "abc", "x"),
    new("^[a-c]+", "x", "dabce", "dabce"),
    new("[a-c]+$", "x", "dabce", "dabce"),
    new("^[a-c]+$", "x", "dabce", "dabce"),
    new("^[a-c]+", "x", "", ""),
    new("[a-c]+$", "x", "", ""),
    new("^[a-c]+$", "x", "", ""),

    // Other cases.
    new("abc", "def", "abcdefg", "defdefg"),
    new("bc", "BC", "abcbcdcdedef", "aBCBCdcdedef"),
    new("abc", "", "abcdabc", "d"),
    new("x", "xXx", "xxxXxxx", "xXxxXxxXxXxXxxXxxXx"),
    new("abc", "d", "", ""),
    new("abc", "d", "abc", "d"),
    new(".+", "x", "abc", "x"),
    new("[a-c]*", "x", "def", "xdxexfx"),
    new("[a-c]+", "x", "abcbcdcdedef", "xdxdedef"),
    new("[a-c]*", "x", "abcbcdcdedef", "xdxdxexdxexfx"),

    // Substitutions
    new("a+", "($0)", "banana", "b(a)n(a)n(a)"),
    new("a+", "(${0})", "banana", "b(a)n(a)n(a)"),
    new("a+", "(${0})$0", "banana", "b(a)an(a)an(a)a"),
    new("a+", "(${0})$0", "banana", "b(a)an(a)an(a)a"),
    new("hello, (.+)", "goodbye, ${1}", "hello, world", "goodbye, world"),
    new("hello, (.+)", "goodbye, $1x", "hello, world", "goodbye, "),
    new("hello, (.+)", "goodbye, ${1}x", "hello, world", "goodbye, worldx"),
    new("hello, (.+)", "<$0><$1><$2><$3>", "hello, world", "<hello, world><world><><>"),
    new("hello, (?P<noun>.+)", "goodbye, $noun!", "hello, world", "goodbye, world!"),
    new("hello, (?P<noun>.+)", "goodbye, ${noun}", "hello, world", "goodbye, world"),
    new("(?P<x>hi)|(?P<x>bye)", "$x$x$x", "hi", "hihihi"),
    new("(?P<x>hi)|(?P<x>bye)", "$x$x$x", "bye", "byebyebye"),
    new("(?P<x>hi)|(?P<x>bye)", "$xyz", "hi", ""),
    new("(?P<x>hi)|(?P<x>bye)", "${x}yz", "hi", "hiyz"),
    new("(?P<x>hi)|(?P<x>bye)", "hello $$x", "hi", "hello $x"),
    new("a+", "${oops", "aaa", "${oops"),
    new("a+", "$$", "aaa", "$"),
    new("a+", "$", "aaa", "$"),

    // Substitution when subexpression isn't found
    new("(x)?", "$1", "123", "123"),
    new("abc", "$1", "123", "123"),

    // Substitutions involving a (x){0}
    new("(a)(b){0}(c)", ".$1|$3.", "xacxacx", "x.a|c.x.a|c.x"),
    new("(a)(((b))){0}c", ".$1.", "xacxacx", "x.a.x.a.x"),
    new("((a(b){0}){3}){5}(h)", "y caramb$2", "say aaaaaaaaaaaaaaaah", "say ay caramba"),
    new("((a(b){0}){3}){5}h", "y caramb$2", "say aaaaaaaaaaaaaaaah", "say ay caramba")
]);

internal static slice<ReplaceTest> replaceLiteralTests = slice<ReplaceTest>([
    // Substitutions
    new("a+", "($0)", "banana", "b($0)n($0)n($0)"),
    new("a+", "(${0})", "banana", "b(${0})n(${0})n(${0})"),
    new("a+", "(${0})$0", "banana", "b(${0})$0n(${0})$0n(${0})$0"),
    new("a+", "(${0})$0", "banana", "b(${0})$0n(${0})$0n(${0})$0"),
    new("hello, (.+)", "goodbye, ${1}", "hello, world", "goodbye, ${1}"),
    new("hello, (?P<noun>.+)", "goodbye, $noun!", "hello, world", "goodbye, $noun!"),
    new("hello, (?P<noun>.+)", "goodbye, ${noun}", "hello, world", "goodbye, ${noun}"),
    new("(?P<x>hi)|(?P<x>bye)", "$x$x$x", "hi", "$x$x$x"),
    new("(?P<x>hi)|(?P<x>bye)", "$x$x$x", "bye", "$x$x$x"),
    new("(?P<x>hi)|(?P<x>bye)", "$xyz", "hi", "$xyz"),
    new("(?P<x>hi)|(?P<x>bye)", "${x}yz", "hi", "${x}yz"),
    new("(?P<x>hi)|(?P<x>bye)", "hello $$x", "hi", "hello $$x"),
    new("a+", "${oops", "aaa", "${oops"),
    new("a+", "$$", "aaa", "$$"),
    new("a+", "$", "aaa", "$")
]);

public partial struct ReplaceFuncTest {
    internal @string pattern;
    internal Func<@string, @string> replacement;
    internal @string input, output;
}

internal static slice<ReplaceFuncTest> replaceFuncTests = slice<ReplaceFuncTest>([
    new("[a-c]", (@string s) => "x"u8 + s + "y"u8, "defabcdef", "defxayxbyxcydef"),
    new("[a-c]+", (@string s) => "x"u8 + s + "y"u8, "defabcdef", "defxabcydef"),
    new("[a-c]*", (@string s) => "x"u8 + s + "y"u8, "defabcdef", "xydxyexyfxabcydxyexyfxy")
]);

public static void TestReplaceAll(ж<testing.T> t) {
    foreach (var (_, tc) in replaceTests) {
        var (re, err) = Compile(tc.pattern);
        if (err != nil) {
            t.Errorf("Unexpected error compiling %q: %v", tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllString(tc.input, tc.replacement);
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceAllString(%q,%q) = %q; want %q",
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
        // now try bytes
        actual = (@string)re.ReplaceAll(slice<byte>(tc.input), slice<byte>(tc.replacement));
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceAll(%q,%q) = %q; want %q",
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
    }
}

public static void TestReplaceAllLiteral(ж<testing.T> t) {
    // Run ReplaceAll tests that do not have $ expansions.
    foreach (var (_, tc) in replaceTests) {
        if (strings.Contains(tc.replacement, "$")) {
            continue;
        }
        var (re, err) = Compile(tc.pattern);
        if (err != nil) {
            t.Errorf("Unexpected error compiling %q: %v", tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllLiteralString(tc.input, tc.replacement);
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceAllLiteralString(%q,%q) = %q; want %q",
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
        // now try bytes
        actual = (@string)re.ReplaceAllLiteral(slice<byte>(tc.input), slice<byte>(tc.replacement));
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceAllLiteral(%q,%q) = %q; want %q",
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
    }

    // Run literal-specific tests.
    foreach (var (_, tc) in replaceLiteralTests) {
        var (re, err) = Compile(tc.pattern);
        if (err != nil) {
            t.Errorf("Unexpected error compiling %q: %v", tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllLiteralString(tc.input, tc.replacement);
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceAllLiteralString(%q,%q) = %q; want %q",
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
        // now try bytes
        actual = (@string)re.ReplaceAllLiteral(slice<byte>(tc.input), slice<byte>(tc.replacement));
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceAllLiteral(%q,%q) = %q; want %q",
                tc.pattern, tc.input, tc.replacement, actual, tc.output);
        }
    }
}

public static void TestReplaceAllFunc(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in replaceFuncTests) {
        ref var tc = ref heap(new ReplaceFuncTest(), out var Ꮡtc);
        tc = vᴛ1;

        var (re, err) = Compile(tc.pattern);
        if (err != nil) {
            t.Errorf("Unexpected error compiling %q: %v", tc.pattern, err);
            continue;
        }
        @string actual = re.ReplaceAllStringFunc(tc.input, tc.replacement);
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceFunc(%q,fn) = %q; want %q",
                tc.pattern, tc.input, actual, tc.output);
        }
        // now try bytes
        var tcʗ1 = tc;
        actual = (@string)re.ReplaceAllFunc(slice<byte>(tc.input), (slice<byte> s) => slice<byte>(tcʗ1.replacement((@string)s)));
        if (actual != tc.output) {
            t.Errorf("%q.ReplaceFunc(%q,fn) = %q; want %q",
                tc.pattern, tc.input, actual, tc.output);
        }
    }
}

public partial struct MetaTest {
    internal @string pattern, output, literal;
    internal bool isLiteral;
}

internal static slice<MetaTest> metaTests = slice<MetaTest>([
    new(@"", @"", @"", true),
    new(@"foo", @"foo", @"foo", true),
    new(@"日本語+", @"日本語\+", @"日本語", false),
    new(@"foo\.\$", @"foo\\\.\\\$", @"foo.$", true), // has meta but no operator
    new(@"foo.\$", @"foo\.\\\$", @"foo", false),     // has escaped operators and real operators
    new(@"!@#$%^&*()_+-=[{]}\|,<.>/?~", @"!@#\$%\^&\*\(\)_\+-=\[\{\]\}\\\|,<\.>/\?~", @"!@#", false)
]);

internal static slice<MetaTest> literalPrefixTests = slice<MetaTest>([
    // See golang.org/issue/11175.
    // output is unused.
    new(@"^0^0$", @"", @"0", false),
    new(@"^0^", @"", @"", false),
    new(@"^0$", @"", @"0", true),
    new(@"$0^", @"", @"", false),
    new(@"$0$", @"", @"", false),
    new(@"^^0$$", @"", @"", false),
    new(@"^$^$", @"", @"", false),
    new(@"$$0^^", @"", @"", false),
    new(@"a\x{fffd}b", @"", @"a", false),
    new(@"\x{fffd}b", @"", @"", false),
    new("\ufffd", @"", @"", false)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xyzˢ => ref xyzˢᵇ;
private static readonly @string xyzˢᵇ = "xyz"u8;
internal static ref readonly @string abcxyzdefˢ => ref abcxyzdefˢᵇ;
private static readonly @string abcxyzdefˢᵇ = "abcxyzdef"u8;

public static void TestQuoteMeta(ж<testing.T> t) {
    foreach (var (_, tc) in metaTests) {
        // Verify that QuoteMeta returns the expected string.
        @string quoted = QuoteMeta(tc.pattern);
        if (quoted != tc.output) {
            t.Errorf("QuoteMeta(`%s`) = `%s`; want `%s`",
                tc.pattern, quoted, tc.output);
            continue;
        }

        // Verify that the quoted string is in fact treated as expected
        // by Compile -- i.e. that it matches the original, unquoted string.
        if (tc.pattern != ""u8) {
            var (re, err) = Compile(quoted);
            if (err != nil) {
                t.Errorf("Unexpected error compiling QuoteMeta(`%s`): %v", tc.pattern, err);
                continue;
            }
            @string src = "abc"u8 + tc.pattern + "def"u8;
            @string repl = xyzˢ;
            @string replaced = re.ReplaceAllString(src, repl);
            @string expected = abcxyzdefˢ;
            if (replaced != expected) {
                t.Errorf("QuoteMeta(`%s`).Replace(`%s`,`%s`) = `%s`; want `%s`",
                    tc.pattern, src, repl, replaced, expected);
            }
        }
    }
}

public static void TestLiteralPrefix(ж<testing.T> t) {
    foreach (var (_, tc) in appendꓸꓸꓸ(metaTests, literalPrefixTests)) {
        // Literal method needs to scan the pattern.
        var re = MustCompile(tc.pattern);
        var (str, complete) = re.LiteralPrefix();
        if (complete != tc.isLiteral) {
            t.Errorf("LiteralPrefix(`%s`) = %t; want %t", tc.pattern, complete, tc.isLiteral);
        }
        if (str != tc.literal) {
            t.Errorf("LiteralPrefix(`%s`) = `%s`; want `%s`", tc.pattern, str, tc.literal);
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

internal static slice<subexpIndex> emptySubexpIndices = slice<subexpIndex>([new("", -1), new("missing", -1)]);

internal static slice<subexpCase> subexpCases = slice<subexpCase>([
    new(@"", 0, default, emptySubexpIndices),
    new(@".*", 0, default, emptySubexpIndices),
    new(@"abba", 0, default, emptySubexpIndices),
    new(@"ab(b)a", 1, slice<@string>(["", ""]), emptySubexpIndices),
    new(@"ab(.*)a", 1, slice<@string>(["", ""]), emptySubexpIndices),
    new(@"(.*)ab(.*)a", 2, slice<@string>(["", "", ""]), emptySubexpIndices),
    new(@"(.*)(ab)(.*)a", 3, slice<@string>(["", "", "", ""]), emptySubexpIndices),
    new(@"(.*)((a)b)(.*)a", 4, slice<@string>(["", "", "", "", ""]), emptySubexpIndices),
    new(@"(.*)(\(ab)(.*)a", 3, slice<@string>(["", "", "", ""]), emptySubexpIndices),
    new(@"(.*)(\(a\)b)(.*)a", 3, slice<@string>(["", "", "", ""]), emptySubexpIndices),
    new(@"(?P<foo>.*)(?P<bar>(a)b)(?P<foo>.*)a", 4, slice<@string>(["", "foo", "bar", "", "foo"]), slice<subexpIndex>([new("", -1), new("missing", -1), new("foo", 1), new("bar", 2)]))
]);

public static void TestSubexp(ж<testing.T> t) {
    foreach (var (_, c) in subexpCases) {
        var re = MustCompile(c.input);
        nint n = re.NumSubexp();
        if (n != c.num) {
            t.Errorf("%q: NumSubexp = %d, want %d", c.input, n, c.num);
            continue;
        }
        var names = re.SubexpNames();
        if (len(names) != 1 + n) {
            t.Errorf("%q: len(SubexpNames) = %d, want %d", c.input, len(names), n);
            continue;
        }
        if (c.names != nil) {
            for (nint i = 0; i < 1 + n; i++) {
                if (names[i] != c.names[i]) {
                    t.Errorf("%q: SubexpNames[%d] = %q, want %q", c.input, i, names[i], c.names[i]);
                }
            }
        }
        foreach (var (_, subexp) in c.indices) {
            nint index = re.SubexpIndex(subexp.name);
            if (index != subexp.index) {
                t.Errorf("%q: SubexpIndex(%q) = %d, want %d", c.input, subexp.name, index, subexp.index);
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
internal static slice<splitTestsᴛ1> splitTests = slice<splitTestsᴛ1>([
    new("foo:and:bar", ":", -1, slice<@string>(["foo", "and", "bar"])),
    new("foo:and:bar", ":", 1, slice<@string>(["foo:and:bar"])),
    new("foo:and:bar", ":", 2, slice<@string>(["foo", "and:bar"])),
    new("foo:and:bar", "foo", -1, slice<@string>(["", ":and:bar"])),
    new("foo:and:bar", "bar", -1, slice<@string>(["foo:and:", ""])),
    new("foo:and:bar", "baz", -1, slice<@string>(["foo:and:bar"])),
    new("baabaab", "a", -1, slice<@string>(["b", "", "b", "", "b"])),
    new("baabaab", "a*", -1, slice<@string>(["b", "b", "b"])),
    new("baabaab", "ba*", -1, slice<@string>(["", "", "", ""])),
    new("foobar", "f*b*", -1, slice<@string>(["", "o", "o", "a", "r"])),
    new("foobar", "f+.*b+", -1, slice<@string>(["", "ar"])),
    new("foobooboar", "o{2}", -1, slice<@string>(["f", "b", "boar"])),
    new("a,b,c,d,e,f", ",", 3, slice<@string>(["a", "b", "c,d,e,f"])),
    new("a,b,c,d,e,f", ",", 0, default),
    new(",", ",", -1, slice<@string>(["", ""])),
    new(",,,", ",", -1, slice<@string>(["", "", "", ""])),
    new("", ",", -1, slice<@string>([""])),
    new("", ".*", -1, slice<@string>([""])),
    new("", ".+", -1, slice<@string>([""])),
    new("", "", -1, slice<@string>([])),
    new("foobar", "", -1, slice<@string>(["f", "o", "o", "b", "a", "r"])),
    new("abaabaccadaaae", "a*", 5, slice<@string>(["", "b", "b", "c", "cadaaae"])),
    new(":x:y:z:", ":", -1, slice<@string>(["", "x", "y", "z", ""]))
]);

public static void TestSplit(ж<testing.T> t) {
    foreach (var (i, test) in splitTests) {
        var (re, err) = Compile(test.r);
        if (err != nil) {
            t.Errorf("#%d: %q: compile error: %s", i, test.r, err.Error());
            continue;
        }

        var split = re.Split(test.s, test.n);
        if (!slices.Equal<slice<@string>, @string>(split, test.@out)) {
            t.Errorf("#%d: %q: got %q; want %q", i, test.r, split, test.@out);
        }

        if (QuoteMeta(test.r) == test.r) {
            var strsplit = strings.SplitN(test.s, test.r, test.n);
            if (!slices.Equal<slice<@string>, @string>(split, strsplit)) {
                t.Errorf("#%d: Split(%q, %q, %d): regexp vs strings mismatch\nregexp=%q\nstrings=%q", i, test.s, test.r, test.n, split, strsplit);
            }
        }
    }
}

internal partial struct TestParseAndCompile_type /*dyn*/ {
    internal syntax.Flags reFlags;
    internal bool expMatch;
}

// The following sequence of Match calls used to panic. See issue #12980.
public static void TestParseAndCompile(ж<testing.T> t) {
    @string expr = "a$";
    @string s = "a\nb";

    foreach (var (i, tc) in slice<TestParseAndCompile_type>([
        new((syntax.Flags)(syntax.Perl | syntax.OneLine), false),
        new((syntax.Flags)(syntax.Perl & ~syntax.OneLine), true)
    ])) {
        var (parsed, err) = syntax.Parse(expr, tc.reFlags);
        if (err != nil) {
            t.Fatalf("%d: parse: %v", i, err);
        }
        (var re, err) = Compile(parsed.String());
        if (err != nil) {
            t.Fatalf("%d: compile: %v", i, err);
        }
        {
            var Δmatch = re.MatchString(s); if (Δmatch != tc.expMatch) {
                t.Errorf("%d: %q.MatchString(%q)=%t; expected=%t", i, re.OrTypedNil(), s, Δmatch, tc.expMatch);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string x11000Y11000ˢ => ref x11000Y11000ˢᵇ;
private static readonly @string x11000Y11000ˢᵇ = @"^x{1,1000}y{1,1000}$"u8;

// Check that one-pass cutoff does trigger.
public static void TestOnePassCutoff(ж<testing.T> t) {
    var (re, err) = syntax.Parse(x11000Y11000ˢ, syntax.Perl);
    if (err != nil) {
        t.Fatalf("parse: %v", err);
    }
    (var p, err) = syntax.Compile(re.Simplify());
    if (err != nil) {
        t.Fatalf("compile: %v", err);
    }
    if (compileOnePass(ref (p).DerefOrNull()) != nil) {
        t.Fatalf("makeOnePass succeeded; wanted nil");
    }
}

// Check that the same machine can be used with the standard matcher
// and then the backtracker when there are no captures.
public static void TestSwitchBacktrack(ж<testing.T> t) {
    var re = MustCompile(@"a|b");
    var @long = new slice<byte>(maxBacktrackVector + 1);

    // The following sequence of Match calls used to panic. See issue #10319.
    re.Match(@long);      // triggers standard matcher
    re.Match(@long[..1]); // triggers backtracker
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aaabbˢ => ref aaabbˢᵇ;
private static readonly @string aaabbˢᵇ = "aaabb"u8;

public static void BenchmarkFind(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var re = MustCompile("a+b+");
    @string wantSubs = aaabbˢ;
    var s = slice<byte>("acbb" + wantSubs + "dd");
    b.StartTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var subs = re.Find(s);
        if (((sstring)subs) != wantSubs) {
            Ꮡb.Fatalf("Find(%q) = %q; want %q", s, subs, wantSubs);
        }
    }
}

public static void BenchmarkFindAllNoMatches(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var re = MustCompile("a+b+");
    var s = slice<byte>("acddee"u8);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var all = re.FindAll(s, -1);
        if (all != nil) {
            Ꮡb.Fatalf("FindAll(%q) = %q; want nil", s, all);
        }
    }
}

public static void BenchmarkFindString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var re = MustCompile("a+b+");
    @string wantSubs = aaabbˢ;
    @string s = "acbb"u8 + wantSubs + "dd"u8;
    b.StartTimer();
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        @string subs = re.FindString(s);
        if (subs != wantSubs) {
            Ꮡb.Fatalf("FindString(%q) = %q; want %q", s, subs, wantSubs);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aABBˢ => ref aABBˢᵇ;
private static readonly @string aABBˢᵇ = "a(a+b+)b"u8;
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
            Ꮡb.Fatalf("FindSubmatch(%q)[0] = %q; want %q", s, subs[0], wantSubs);
        }
        if (((sstring)subs[1]) != "aab"u8) {
            Ꮡb.Fatalf("FindSubmatch(%q)[1] = %q; want %q", s, subs[1], aabˢ);
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
            Ꮡb.Fatalf("FindStringSubmatch(%q)[0] = %q; want %q", s, subs[0], wantSubs);
        }
        if (subs[1] != "aab") {
            Ꮡb.Fatalf("FindStringSubmatch(%q)[1] = %q; want %q", s, subs[1], aabˢ);
        }
    }
}

public static void BenchmarkLiteral(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = strings.Repeat("x", 50) + "y"u8;
    b.StopTimer();
    var re = MustCompile("y");
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!");
        }
    }
}

public static void BenchmarkNotLiteral(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = strings.Repeat("x", 50) + "y"u8;
    b.StopTimer();
    var re = MustCompile(".y");
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxxˢ => ref xxxxˢᵇ;
private static readonly @string xxxxˢᵇ = "xxxx"u8;
internal static ref readonly @string abcdwˢ => ref abcdwˢᵇ;
private static readonly @string abcdwˢᵇ = "[abcdw]"u8;

public static void BenchmarkMatchClass(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    @string x = strings.Repeat(xxxxˢ, 20) + "w"u8;
    var re = MustCompile(abcdwˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bbbbˢ => ref bbbbˢᵇ;
private static readonly @string bbbbˢᵇ = "bbbb"u8;

public static void BenchmarkMatchClass_InRange(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    // 'b' is between 'a' and 'c', so the charclass
    // range checking is no help here.
    @string x = strings.Repeat(bbbbˢ, 20) + "c"u8;
    var re = MustCompile("[ac]");
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (!re.MatchString(x)) {
            Ꮡb.Fatalf("no match!");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cjrwˢ => ref cjrwˢᵇ;
private static readonly @string cjrwˢᵇ = "[cjrw]"u8;

public static void BenchmarkReplaceAll(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = "abcdefghijklmnopqrstuvwxyz";
    b.StopTimer();
    var re = MustCompile(cjrwˢ);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.ReplaceAllString(x, ""u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string zbcDEˢ => ref zbcDEˢᵇ;
private static readonly @string zbcDEˢᵇ = "^zbc(d|e)"u8;

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
internal static ref readonly @string bcDEˢ => ref bcDEˢᵇ;
private static readonly @string bcDEˢᵇ = "^.bc(d|e)"u8;

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
internal static ref readonly @string bcDEˢ2 => ref bcDEˢ2ᵇ;
private static readonly @string bcDEˢ2ᵇ = "^.bc(d|e)*$"u8;

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
internal static ref readonly @string bcDEˢ3 => ref bcDEˢ3ᵇ;
private static readonly @string bcDEˢ3ᵇ = ".bc(d|e)*$"u8;

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
internal static ref readonly @string bcDEˢ4 => ref bcDEˢ4ᵇ;
private static readonly @string bcDEˢ4ᵇ = "^.bc(?:d|e)*$"u8;

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
internal static ref readonly @string bcDEˢ5 => ref bcDEˢ5ᵇ;
private static readonly @string bcDEˢ5ᵇ = ".bc(?:d|e)*$"u8;

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
    var re = MustCompile("^abcdefghijklmnopqrstuvwxyz.*$");
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

public static void BenchmarkOnePassLongNotPrefix(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = slice<byte>("abcdefghijklmnopqrstuvwxyz"u8);
    var re = MustCompile("^.bcdefghijklmnopqrstuvwxyz.*$");
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        re.Match(x);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooBaRBazˢ => ref fooBaRBazˢᵇ;
private static readonly @string fooBaRBazˢᵇ = "foo (ba+r)? baz"u8;

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
    var specialsᴷ = new stackAppend<byte>();

    var specials = new slice<byte>(0);
    for (var i = (byte)0; i < utf8.RuneSelf; i++) {
        if (special(i)) {
            specials = append(specialsᴷ, specials, i);
        }
    }
    @string s = (@string)specials;
    b.SetBytes((int64)len(s));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sink = QuoteMeta(s);
    }
}

public static void BenchmarkQuoteMetaNone(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = "abcdefghijklmnopqrstuvwxyz";
    b.SetBytes((int64)len(s));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        sink = QuoteMeta(s);
    }
}

partial struct compileBenchDataᴛ1 /*dyn*/ {
    internal @string name, re;
}
internal static slice<compileBenchDataᴛ1> compileBenchData = slice<compileBenchDataᴛ1>([
    new("Onepass", @"^a.[l-nA-Cg-j]?e$"),
    new("Medium", @"^((a|b|[d-z0-9])*(日){4,5}.)+$"),
    new("Hard", strings.Repeat(@"((abc)*|", 50) + strings.Repeat(@")", 50))
]);

public static void BenchmarkCompile(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in compileBenchData) {
        ref var data = ref heap(new compileBenchDataᴛ1(), out var Ꮡdata);
        data = vᴛ1;

        var dataʗ1 = data;
        b.Run(data.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                {
                    var (_, err) = Compile(dataʗ1.re); if (err != nil) {
                        bΔ1.Fatal(err);
                    }
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBCDˢ => ref aBCDˢᵇ;
private static readonly @string aBCDˢᵇ = "a.*b.*c.*d"u8;
internal static ref readonly @string abcdefghijklmnˢ => ref abcdefghijklmnˢᵇ;
private static readonly @string abcdefghijklmnˢᵇ = "abcdefghijklmn"u8;

public static void TestDeepEqual(ж<testing.T> t) {
    var re1 = MustCompile(aBCDˢ);
    var re2 = MustCompile(aBCDˢ);
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) { // has always been true, since Go 1.
        t.Errorf("DeepEqual(re1, re2) = false, want true");
    }

    re1.MatchString(abcdefghijklmnˢ);
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        t.Errorf("DeepEqual(re1, re2) = false, want true");
    }

    re2.MatchString(abcdefghijklmnˢ);
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        t.Errorf("DeepEqual(re1, re2) = false, want true");
    }

    re2.MatchString(strings.Repeat(abcdefghijklmnˢ, 100));
    if (!reflect.DeepEqual(re1.OrTypedNil(), re2.OrTypedNil())) {
        t.Errorf("DeepEqual(re1, re2) = false, want true");
    }
}

partial struct minInputLenTestsᴛ1 /*dyn*/ {
    public @string Regexp;
    internal nint min;
}
internal static slice<minInputLenTestsᴛ1> minInputLenTests = slice<minInputLenTestsᴛ1>([
    new(@"", 0),
    new(@"a", 1),
    new(@"aa", 2),
    new(@"(aa)a", 3),
    new(@"(?:aa)a", 3),
    new(@"a?a", 1),
    new(@"(aaa)|(aa)", 2),
    new(@"(aa)+a", 3),
    new(@"(aa)*a", 1),
    new(@"(aa){3,5}", 6),
    new(@"[a-z]", 1),
    new(@"日", 3)
]);

public static void TestMinInputLen(ж<testing.T> t) {
    foreach (var (_, tt) in minInputLenTests) {
        var (re, _) = syntax.Parse(tt.Regexp, syntax.Perl);
        nint m = minInputLen(ref (re).DerefOrNull());
        if (m != tt.min) {
            t.Errorf("regexp %#q has minInputLen %d, should be %d", tt.Regexp, m, tt.min);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidPatternˢ => ref invalidPatternˢᵇ;
private static readonly @string invalidPatternˢᵇ = "invalid pattern"u8;
internal static readonly object unexpectedSuccessˢ = (@string)"unexpected success"u8;

public static void TestUnmarshalText(ж<testing.T> t) {
    var unmarshaled = @new<global::go.regexp_package.Regexp>();
    foreach (var i in range(goodRe)) {
        var re = compileTest(t, goodRe[i], ""u8);
        var (marshaled, err) = re.MarshalText();
        if (err != nil) {
            t.Errorf("regexp %#q failed to marshal: %s", re.OrTypedNil(), err);
            continue;
        }
        {
            var errΔ1 = unmarshaled.UnmarshalText(marshaled); if (errΔ1 != nil) {
                t.Errorf("regexp %#q failed to unmarshal: %s", re.OrTypedNil(), errΔ1);
                continue;
            }
        }
        if (unmarshaled.String() != goodRe[i]) {
            t.Errorf("UnmarshalText returned unexpected value: %s", unmarshaled.String());
        }

        var buf = new slice<byte>(4, 32);
        (var marshalAppend, err) = re.AppendText(buf);
        if (err != nil) {
            t.Errorf("regexp %#q failed to marshal: %s", re.OrTypedNil(), err);
            continue;
        }
        marshalAppend = marshalAppend[4..];
        {
            var errΔ2 = unmarshaled.UnmarshalText(marshalAppend); if (errΔ2 != nil) {
                t.Errorf("regexp %#q failed to unmarshal: %s", re.OrTypedNil(), errΔ2);
                continue;
            }
        }
        if (unmarshaled.String() != goodRe[i]) {
            t.Errorf("UnmarshalText returned unexpected value: %s", unmarshaled.String());
        }
    }
    t.Run(invalidPatternˢ, (ж<testing.T> tΔ1) => {
        var re = @new<global::go.regexp_package.Regexp>();
        var err = re.UnmarshalText(slice<byte>(@"\"u8));
        if (err == nil) {
            tΔ1.Error(unexpectedSuccessˢ);
        }
    });
}

} // end regexp_internal_test_package
