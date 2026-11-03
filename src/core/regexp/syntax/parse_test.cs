// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using unicode = unicode_package;
using io = io_package;
using static go.regexp.syntax_package;

partial class syntax_internal_test_package {

internal partial struct parseTest {
    public @string Regexp;
    public @string Dump;
}

internal static slice<parseTest> parseTests;
internal static void initᴛparseTests() { parseTests = slice<parseTest>([
    // Base cases
    new(@"a", @"lit{a}"),
    new(@"a.", @"cat{lit{a}dot{}}"),
    new(@"a.b", @"cat{lit{a}dot{}lit{b}}"),
    new(@"ab", @"str{ab}"),
    new(@"a.b.c", @"cat{lit{a}dot{}lit{b}dot{}lit{c}}"),
    new(@"abc", @"str{abc}"),
    new(@"a|^", @"alt{lit{a}bol{}}"),
    new(@"a|b", @"cc{0x61-0x62}"),
    new(@"(a)", @"cap{lit{a}}"),
    new(@"(a)|b", @"alt{cap{lit{a}}lit{b}}"),
    new(@"a*", @"star{lit{a}}"),
    new(@"a+", @"plus{lit{a}}"),
    new(@"a?", @"que{lit{a}}"),
    new(@"a{2}", @"rep{2,2 lit{a}}"),
    new(@"a{2,3}", @"rep{2,3 lit{a}}"),
    new(@"a{2,}", @"rep{2,-1 lit{a}}"),
    new(@"a*?", @"nstar{lit{a}}"),
    new(@"a+?", @"nplus{lit{a}}"),
    new(@"a??", @"nque{lit{a}}"),
    new(@"a{2}?", @"nrep{2,2 lit{a}}"),
    new(@"a{2,3}?", @"nrep{2,3 lit{a}}"),
    new(@"a{2,}?", @"nrep{2,-1 lit{a}}"),
    // Malformed { } are treated as literals.
    new(@"x{1001", @"str{x{1001}"),
    new(@"x{9876543210", @"str{x{9876543210}"),
    new(@"x{9876543210,", @"str{x{9876543210,}"),
    new(@"x{2,1", @"str{x{2,1}"),
    new(@"x{1,9876543210", @"str{x{1,9876543210}"),
    new(@"", @"emp{}"),
    new(@"|", @"emp{}"), // alt{emp{}emp{}} but got factored
    new(@"|x|", @"alt{emp{}lit{x}emp{}}"),
    new(@".", @"dot{}"),
    new(@"^", @"bol{}"),
    new(@"$", @"eol{}"),
    new(@"\|", @"lit{|}"),
    new(@"\(", @"lit{(}"),
    new(@"\)", @"lit{)}"),
    new(@"\*", @"lit{*}"),
    new(@"\+", @"lit{+}"),
    new(@"\?", @"lit{?}"),
    new(@"{", @"lit{{}"),
    new(@"}", @"lit{}}"),
    new(@"\.", @"lit{.}"),
    new(@"\^", @"lit{^}"),
    new(@"\$", @"lit{$}"),
    new(@"\\", @"lit{\}"),
    new(@"[ace]", @"cc{0x61 0x63 0x65}"),
    new(@"[abc]", @"cc{0x61-0x63}"),
    new(@"[a-z]", @"cc{0x61-0x7a}"),
    new(@"[a]", @"lit{a}"),
    new(@"\-", @"lit{-}"),
    new(@"-", @"lit{-}"),
    new(@"\_", @"lit{_}"),
    new(@"abc", @"str{abc}"),
    new(@"abc|def", @"alt{str{abc}str{def}}"),
    new(@"abc|def|ghi", @"alt{str{abc}str{def}str{ghi}}"),

    // Posix and Perl extensions
    new(@"[[:lower:]]", @"cc{0x61-0x7a}"),
    new(@"[a-z]", @"cc{0x61-0x7a}"),
    new(@"[^[:lower:]]", @"cc{0x0-0x60 0x7b-0x10ffff}"),
    new(@"[[:^lower:]]", @"cc{0x0-0x60 0x7b-0x10ffff}"),
    new(@"(?i)[[:lower:]]", @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"),
    new(@"(?i)[a-z]", @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"),
    new(@"(?i)[^[:lower:]]", @"cc{0x0-0x40 0x5b-0x60 0x7b-0x17e 0x180-0x2129 0x212b-0x10ffff}"),
    new(@"(?i)[[:^lower:]]", @"cc{0x0-0x40 0x5b-0x60 0x7b-0x17e 0x180-0x2129 0x212b-0x10ffff}"),
    new(@"\d", @"cc{0x30-0x39}"),
    new(@"\D", @"cc{0x0-0x2f 0x3a-0x10ffff}"),
    new(@"\s", @"cc{0x9-0xa 0xc-0xd 0x20}"),
    new(@"\S", @"cc{0x0-0x8 0xb 0xe-0x1f 0x21-0x10ffff}"),
    new(@"\w", @"cc{0x30-0x39 0x41-0x5a 0x5f 0x61-0x7a}"),
    new(@"\W", @"cc{0x0-0x2f 0x3a-0x40 0x5b-0x5e 0x60 0x7b-0x10ffff}"),
    new(@"(?i)\w", @"cc{0x30-0x39 0x41-0x5a 0x5f 0x61-0x7a 0x17f 0x212a}"),
    new(@"(?i)\W", @"cc{0x0-0x2f 0x3a-0x40 0x5b-0x5e 0x60 0x7b-0x17e 0x180-0x2129 0x212b-0x10ffff}"),
    new(@"[^\\]", @"cc{0x0-0x5b 0x5d-0x10ffff}"),
    //	{ `\C`, `byte{}` },  // probably never

    // Unicode, negatives, and a double negative.
    new(@"\p{Braille}", @"cc{0x2800-0x28ff}"),
    new(@"\P{Braille}", @"cc{0x0-0x27ff 0x2900-0x10ffff}"),
    new(@"\p{^Braille}", @"cc{0x0-0x27ff 0x2900-0x10ffff}"),
    new(@"\P{^Braille}", @"cc{0x2800-0x28ff}"),
    new(@"\pZ", @"cc{0x20 0xa0 0x1680 0x2000-0x200a 0x2028-0x2029 0x202f 0x205f 0x3000}"),
    new(@"[\p{Braille}]", @"cc{0x2800-0x28ff}"),
    new(@"[\P{Braille}]", @"cc{0x0-0x27ff 0x2900-0x10ffff}"),
    new(@"[\p{^Braille}]", @"cc{0x0-0x27ff 0x2900-0x10ffff}"),
    new(@"[\P{^Braille}]", @"cc{0x2800-0x28ff}"),
    new(@"[\pZ]", @"cc{0x20 0xa0 0x1680 0x2000-0x200a 0x2028-0x2029 0x202f 0x205f 0x3000}"),
    new(@"\p{Lu}", mkCharClass(unicode.IsUpper)),
    new(@"\p{Uppercase_Letter}", mkCharClass(unicode.IsUpper)),
    new(@"\p{upper case-let ter}", mkCharClass(unicode.IsUpper)),
    new(@"\p{__upper case-let ter}", mkCharClass(unicode.IsUpper)),
    new(@"[\p{Lu}]", mkCharClass(unicode.IsUpper)),
    new(@"(?i)[\p{Lu}]", mkCharClass(isUpperFold)),
    new(@"\p{Any}", @"dot{}"),
    new(@"\p{^Any}", @"cc{}"),
    new(@"(?i)\p{ascii}", @"cc{0x0-0x7f 0x17f 0x212a}"),
    new(@"\p{Assigned}", mkCharClass((rune r) => !unicode.In(r, unicode.Cn))),
    new(@"\p{^Assigned}", mkCharClass((rune r) => unicode.In(r, unicode.Cn))),

    // Hex, octal.
    new(@"[\012-\234]\141", @"cat{cc{0xa-0x9c}lit{a}}"),
    new(@"[\x{41}-\x7a]\x61", @"cat{cc{0x41-0x7a}lit{a}}"),

    // More interesting regular expressions.
    new(@"a{,2}", @"str{a{,2}}"),
    new(@"\.\^\$\\", @"str{.^$\}"),
    new(@"[a-zABC]", @"cc{0x41-0x43 0x61-0x7a}"),
    new(@"[^a]", @"cc{0x0-0x60 0x62-0x10ffff}"),
    new(@"[α-ε☺]", @"cc{0x3b1-0x3b5 0x263a}"), // utf-8
    new(@"a*{", @"cat{star{lit{a}}lit{{}}"),

    // Test precedences
    new(@"(?:ab)*", @"star{str{ab}}"),
    new(@"(ab)*", @"star{cap{str{ab}}}"),
    new(@"ab|cd", @"alt{str{ab}str{cd}}"),
    new(@"a(b|c)d", @"cat{lit{a}cap{cc{0x62-0x63}}lit{d}}"),

    // Test flattening.
    new(@"(?:a)", @"lit{a}"),
    new(@"(?:ab)(?:cd)", @"str{abcd}"),
    new(@"(?:a+b+)(?:c+d+)", @"cat{plus{lit{a}}plus{lit{b}}plus{lit{c}}plus{lit{d}}}"),
    new(@"(?:a+|b+)|(?:c+|d+)", @"alt{plus{lit{a}}plus{lit{b}}plus{lit{c}}plus{lit{d}}}"),
    new(@"(?:a|b)|(?:c|d)", @"cc{0x61-0x64}"),
    new(@"a|.", @"dot{}"),
    new(@".|a", @"dot{}"),
    new(@"(?:[abc]|A|Z|hello|world)", @"alt{cc{0x41 0x5a 0x61-0x63}str{hello}str{world}}"),
    new(@"(?:[abc]|A|Z)", @"cc{0x41 0x5a 0x61-0x63}"),

    // Test Perl quoted literals
    new(@"\Q+|*?{[\E", @"str{+|*?{[}"),
    new(@"\Q+\E+", @"plus{lit{+}}"),
    new(@"\Qab\E+", @"cat{lit{a}plus{lit{b}}}"),
    new(@"\Q\\E", @"lit{\}"),
    new(@"\Q\\\E", @"str{\\}"),

    // Test Perl \A and \z
    new(@"(?m)^", @"bol{}"),
    new(@"(?m)$", @"eol{}"),
    new(@"(?-m)^", @"bot{}"),
    new(@"(?-m)$", @"eot{}"),
    new(@"(?m)\A", @"bot{}"),
    new(@"(?m)\z", @"eot{\z}"),
    new(@"(?-m)\A", @"bot{}"),
    new(@"(?-m)\z", @"eot{\z}"),

    // Test named captures
    new(@"(?P<name>a)", @"cap{name:lit{a}}"),
    new(@"(?<name>a)", @"cap{name:lit{a}}"),

    // Case-folded literals
    new(@"[Aa]", @"litfold{A}"),
    new(@"[\x{100}\x{101}]", @"litfold{Ā}"),
    new(@"[Δδ]", @"litfold{Δ}"),

    // Strings
    new(@"abcde", @"str{abcde}"),
    new(@"[Aa][Bb]cd", @"cat{strfold{AB}str{cd}}"),

    // Factoring.
    new(@"abc|abd|aef|bcx|bcy", @"alt{cat{lit{a}alt{cat{lit{b}cc{0x63-0x64}}str{ef}}}cat{str{bc}cc{0x78-0x79}}}"),
    new(@"ax+y|ax+z|ay+w", @"cat{lit{a}alt{cat{plus{lit{x}}lit{y}}cat{plus{lit{x}}lit{z}}cat{plus{lit{y}}lit{w}}}}"),

    // Bug fixes.
    new(@"(?:.)", @"dot{}"),
    new(@"(?:x|(?:xa))", @"cat{lit{x}alt{emp{}lit{a}}}"),
    new(@"(?:.|(?:.a))", @"cat{dot{}alt{emp{}lit{a}}}"),
    new(@"(?:A(?:A|a))", @"cat{lit{A}litfold{A}}"),
    new(@"(?:A|a)", @"litfold{A}"),
    new(@"A|(?:A|a)", @"litfold{A}"),
    new(@"(?s).", @"dot{}"),
    new(@"(?-s).", @"dnl{}"),
    new(@"(?:(?:^).)", @"cat{bol{}dot{}}"),
    new(@"(?-s)(?:(?:^).)", @"cat{bol{}dnl{}}"),
    new(@"[\s\S]a", @"cat{cc{0x0-0x10ffff}lit{a}}"),

    // RE2 prefix_tests
    new(@"abc|abd", @"cat{str{ab}cc{0x63-0x64}}"),
    new(@"a(?:b)c|abd", @"cat{str{ab}cc{0x63-0x64}}"),
    new(@"abc|abd|aef|bcx|bcy",
        @"alt{cat{lit{a}alt{cat{lit{b}cc{0x63-0x64}}str{ef}}}"u8 + @"cat{str{bc}cc{0x78-0x79}}}"u8),
    new(@"abc|x|abd", @"alt{str{abc}lit{x}str{abd}}"),
    new(@"(?i)abc|ABD", @"cat{strfold{AB}cc{0x43-0x44 0x63-0x64}}"),
    new(@"[ab]c|[ab]d", @"cat{cc{0x61-0x62}cc{0x63-0x64}}"),
    new(@".c|.d", @"cat{dot{}cc{0x63-0x64}}"),
    new(@"x{2}|x{2}[0-9]",
        @"cat{rep{2,2 lit{x}}alt{emp{}cc{0x30-0x39}}}"),
    new(@"x{2}y|x{2}[0-9]y",
        @"cat{rep{2,2 lit{x}}alt{lit{y}cat{cc{0x30-0x39}lit{y}}}}"),
    new(@"a.*?c|a.*?b",
        @"cat{lit{a}alt{cat{nstar{dot{}}lit{c}}cat{nstar{dot{}}lit{b}}}}"),

    // Valid repetitions.
    new(@"((((((((((x{2}){2}){2}){2}){2}){2}){2}){2}){2}))", @""),
    new(@"((((((((((x{1}){2}){2}){2}){2}){2}){2}){2}){2}){2})", @""),

    // Valid nesting.
    new(strings.Repeat("(", 999) + strings.Repeat(")", 999), @""),
    new(strings.Repeat("(?:", 999) + strings.Repeat(")*", 999), @""),
    new("("u8 + strings.Repeat("|", 12345) + ")"u8, @"") // not nested at all
]); }

internal static global::go.regexp.syntax_package.Flags testFlags => /* MatchNL | PerlX | UnicodeGroups */ 204;

public static void TestParseSimple(ж<testing.T> t) {
    testParseDump(t, parseTests, testFlags);
}

internal static slice<parseTest> foldcaseTests = slice<parseTest>([
    new(@"AbCdE", @"strfold{ABCDE}"),
    new(@"[Aa]", @"litfold{A}"),
    new(@"a", @"litfold{A}"),

    // 0x17F is an old English long s (looks like an f) and folds to s.
    // 0x212A is the Kelvin symbol and folds to k.
    new(@"A[F-g]", @"cat{litfold{A}cc{0x41-0x7a 0x17f 0x212a}}"), // [Aa][A-z...]
    new(@"[[:upper:]]", @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"),
    new(@"[[:lower:]]", @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}")
]);

public static void TestParseFoldCase(ж<testing.T> t) {
    testParseDump(t, foldcaseTests, FoldCase);
}

internal static slice<parseTest> literalTests = slice<parseTest>([
    new("(|)^$.[*+?]{5,10},\\", "str{(|)^$.[*+?]{5,10},\\}")
]);

public static void TestParseLiteral(ж<testing.T> t) {
    testParseDump(t, literalTests, Literal);
}

internal static slice<parseTest> matchnlTests = slice<parseTest>([
    new(@".", @"dot{}"),
    new("\n", "lit{\n}"),
    new(@"[^a]", @"cc{0x0-0x60 0x62-0x10ffff}"),
    new(@"[a\n]", @"cc{0xa 0x61}")
]);

public static void TestParseMatchNL(ж<testing.T> t) {
    testParseDump(t, matchnlTests, MatchNL);
}

internal static slice<parseTest> nomatchnlTests = slice<parseTest>([
    new(@".", @"dnl{}"),
    new("\n", "lit{\n}"),
    new(@"[^a]", @"cc{0x0-0x9 0xb-0x60 0x62-0x10ffff}"),
    new(@"[a\n]", @"cc{0xa 0x61}")
]);

public static void TestParseNoMatchNL(ж<testing.T> t) {
    testParseDump(t, nomatchnlTests, 0);
}

// Test Parse -> Dump.
internal static void testParseDump(ж<testing.T> t, slice<parseTest> tests, global::go.regexp.syntax_package.Flags flags) {
    foreach (var (_, tt) in tests) {
        var (re, err) = Parse(tt.Regexp, flags);
        if (err != nil) {
            t.Errorf("Parse(%#q): %v", tt.Regexp, err);
            continue;
        }
        if (tt.Dump == ""u8) {
            // It parsed. That's all we care about.
            continue;
        }
        @string d = dump(re);
        if (d != tt.Dump) {
            t.Errorf("Parse(%#q).Dump() = %#q want %#q", tt.Regexp, d, tt.Dump);
        }
    }
}

// dump prints a string representation of the regexp showing
// the structure explicitly.
internal static @string dump(ж<global::go.regexp.syntax_package.Regexp> re) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    dumpRegexp(Ꮡb, re);
    return b.String();
}

internal static slice<@string> opNames = new golib.SparseArray<@string>{
    [OpNoMatch] = "no",
    [OpEmptyMatch] = "emp",
    [OpLiteral] = "lit",
    [OpCharClass] = "cc",
    [OpAnyCharNotNL] = "dnl",
    [OpAnyChar] = "dot",
    [OpBeginLine] = "bol",
    [OpEndLine] = "eol",
    [OpBeginText] = "bot",
    [OpEndText] = "eot",
    [OpWordBoundary] = "wb",
    [OpNoWordBoundary] = "nwb",
    [OpCapture] = "cap",
    [OpStar] = "star",
    [OpPlus] = "plus",
    [OpQuest] = "que",
    [OpRepeat] = "rep",
    [OpConcat] = "cat",
    [OpAlternate] = "alt"
}.slice();

// dumpRegexp writes an encoding of the syntax tree for the regexp re to b.
// It is used during testing to distinguish between parses that might print
// the same using re's String method.
internal static void dumpRegexp(ж<strings.Builder> b, ж<global::go.regexp.syntax_package.Regexp> Ꮡre) {
    ref var re = ref Ꮡre.DerefOrNull();

    if ((nint)re.Op >= len(opNames) || opNames[re.Op] == "") {
        fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(b), "op%d"u8, re.Op);
    } else {
        switch (re.Op) {
        default: {
            b.WriteString(opNames[re.Op]);
            break;
        }
        case var ᴛ when ᴛ == OpStar || ᴛ == OpPlus || ᴛ == OpQuest || ᴛ == OpRepeat: {
            if ((global::go.regexp.syntax_package.Flags)(re.Flags & NonGreedy) != 0) {
                b.WriteByte((byte)'n');
            }
            b.WriteString(opNames[re.Op]);
            break;
        }
        case var ᴛ when ᴛ == OpLiteral: {
            if (len(re.Rune) > 1) {
                b.WriteString("str");
            } else {
                b.WriteString("lit");
            }
            if ((global::go.regexp.syntax_package.Flags)(re.Flags & FoldCase) != 0) {
                foreach (var (_, r) in re.Rune) {
                    if (unicode.SimpleFold(r) != r) {
                        b.WriteString("fold");
                        break;
                    }
                }
            }
            break;
        }}
    }
    b.WriteByte((byte)'{');
    switch (re.Op) {
    case var ᴛ when ᴛ == OpEndText: {
        if ((global::go.regexp.syntax_package.Flags)(re.Flags & WasDollar) == 0) {
            b.WriteString(@"\z");
        }
        break;
    }
    case var ᴛ when ᴛ == OpLiteral: {
        foreach (var (_, r) in re.Rune) {
            b.WriteRune(r);
        }
        break;
    }
    case var ᴛ when ᴛ == OpConcat || ᴛ == OpAlternate: {
        foreach (var (_, sub) in re.Sub) {
            dumpRegexp(b, sub);
        }
        break;
    }
    case var ᴛ when ᴛ == OpStar || ᴛ == OpPlus || ᴛ == OpQuest: {
        dumpRegexp(b, re.Sub[0]);
        break;
    }
    case var ᴛ when ᴛ == OpRepeat: {
        fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(b), "%d,%d "u8, re.Min, re.Max);
        dumpRegexp(b, re.Sub[0]);
        break;
    }
    case var ᴛ when ᴛ == OpCapture: {
        if (re.Name != ""u8) {
            b.WriteString(re.Name);
            b.WriteByte((byte)':');
        }
        dumpRegexp(b, re.Sub[0]);
        break;
    }
    case var ᴛ when ᴛ == OpCharClass: {
        @string sep = "";
        for (nint i = 0; i < len(re.Rune); i += 2) {
            b.WriteString(sep);
            sep = " ";
            var (lo, hi) = (re.Rune[i], re.Rune[i + 1]);
            if (lo == hi) {
                fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(b), "%#x"u8, lo);
            } else {
                fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(b), "%#x-%#x"u8, lo, hi);
            }
        }
        break;
    }}
    b.WriteByte((byte)'}');
}

internal static @string mkCharClass(Func<rune, bool> f) {
    var re = Ꮡ(new Regexp(Op: OpCharClass));
    var lo = (rune)(-1);
    for (var i = (rune)0; i <= unicode.MaxRune; i++) {
        if (f(i)) {
            if (lo < 0) {
                lo = i;
            }
        } else {
            if (lo >= 0) {
                re.Rune = builtin.append(re.Rune, lo, i - 1);
                lo = -1;
            }
        }
    }
    if (lo >= 0) {
        re.Rune = builtin.append(re.Rune, lo, (rune)(unicode.MaxRune));
    }
    return dump(re);
}

internal static bool isUpperFold(rune r) {
    if (unicode.IsUpper(r)) {
        return true;
    }
    var c = unicode.SimpleFold(r);
    while (c != r) {
        if (unicode.IsUpper(c)) {
            return true;
        }
        c = unicode.SimpleFold(c);
    }
    return false;
}

public static void TestFoldConstants(ж<testing.T> t) {
    var last = (rune)(-1);
    for (var i = (rune)0; i <= unicode.MaxRune; i++) {
        if (unicode.SimpleFold(i) == i) {
            continue;
        }
        if (last == -1 && minFold != i) {
            t.Errorf("minFold=%#U should be %#U", (nint)(minFold), i);
        }
        last = i;
    }
    if (maxFold != last) {
        t.Errorf("maxFold=%#U should be %#U", (nint)(maxFold), last);
    }
}

public static void TestAppendRangeCollapse(ж<testing.T> t) {
    // AppendRange should collapse each of the new ranges
    // into the earlier ones (it looks back two ranges), so that
    // the slice never grows very large.
    // Note that we are not calling cleanClass.
    slice<rune> r = default;
    for (var i = (rune)'A'; i <= 'Z'; i++) {
        r = appendRange(r, i, i);
        r = appendRange(r, i + 'a' - 'A', i + 'a' - 'A');
    }
    if (((@string)r) != "AZaz"u8) {
        t.Errorf("appendRange interlaced A-Z a-z = %s, want AZaz", (@string)r);
    }
}

internal static slice<@string> invalidRegexps = slice<@string>([
    @"(",
    @")",
    @"(a",
    @"a)",
    @"(a))",
    @"(a|b|",
    @"a|b|)",
    @"(a|b|))",
    @"(a|b",
    @"a|b)",
    @"(a|b))",
    @"[a-z",
    @"([a-z)",
    @"[a-z)",
    @"([a-z]))",
    @"x{1001}",
    @"x{9876543210}",
    @"x{2,1}",
    @"x{1,9876543210}",
    ((@string)(new byte[]{0xff})), // Invalid UTF-8
    ((@string)(new byte[]{0x5b, 0xff, 0x5d})),
    ((@string)(new byte[]{0x5b, 0x5c, 0xff, 0x5d})),
    ((@string)(new byte[]{0x5c, 0xff})),
    @"(?P<name>a",
    @"(?P<name>",
    @"(?P<name",
    @"(?P<x y>a)",
    @"(?P<>a)",
    @"(?<name>a",
    @"(?<name>",
    @"(?<name",
    @"(?<x y>a)",
    @"(?<>a)",
    @"[a-Z]",
    @"(?i)[a-Z]",
    @"\Q\E*",
    @"a{100000}",  // too much repetition
    @"a{100000,}", // too much repetition
    "((((((((((x{2}){2}){2}){2}){2}){2}){2}){2}){2}){2})",    // too much repetition
    strings.Repeat("(", 1000) + strings.Repeat(")", 1000),    // too deep
    strings.Repeat("(?:", 1000) + strings.Repeat(")*", 1000), // too deep
    "("u8 + strings.Repeat("(xx?)", 1000) + "){1000}"u8,      // too long
    strings.Repeat("(xx?){1000}", 1000),                      // too long
    strings.Repeat(@"\pL", 27000)                             // too many runes
]);

internal static slice<@string> onlyPerl = slice<@string>([
    @"[a-b-c]",
    @"\Qabc\E",
    @"\Q*+?{[\E",
    @"\Q\\E",
    @"\Q\\\E",
    @"\Q\\\\E",
    @"\Q\\\\\E",
    @"(?:a)",
    @"(?P<name>a)"
]);

internal static slice<@string> onlyPOSIX = slice<@string>([
    "a++",
    "a**",
    "a?*",
    "a+*",
    "a{1}*",
    ".{1}{2}.{3}"
]);

public static void TestParseInvalidRegexps(ж<testing.T> t) {
    foreach (var (_, regexp) in invalidRegexps) {
        {
            var (re, err) = Parse(regexp, Perl); if (err == nil) {
                t.Errorf("Parse(%#q, Perl) = %s, should have failed", regexp, dump(re));
            }
        }
        {
            var (re, err) = Parse(regexp, POSIX); if (err == nil) {
                t.Errorf("Parse(%#q, POSIX) = %s, should have failed", regexp, dump(re));
            }
        }
    }
    foreach (var (_, regexp) in onlyPerl) {
        {
            var (_, err) = Parse(regexp, Perl); if (err != nil) {
                t.Errorf("Parse(%#q, Perl): %v", regexp, err);
            }
        }
        {
            var (re, err) = Parse(regexp, POSIX); if (err == nil) {
                t.Errorf("Parse(%#q, POSIX) = %s, should have failed", regexp, dump(re));
            }
        }
    }
    foreach (var (_, regexp) in onlyPOSIX) {
        {
            var (re, err) = Parse(regexp, Perl); if (err == nil) {
                t.Errorf("Parse(%#q, Perl) = %s, should have failed", regexp, dump(re));
            }
        }
        {
            var (_, err) = Parse(regexp, POSIX); if (err != nil) {
                t.Errorf("Parse(%#q, POSIX): %v", regexp, err);
            }
        }
    }
}

public static void TestToStringEquivalentParse(ж<testing.T> t) {
    foreach (var (_, tt) in parseTests) {
        var (re, err) = Parse(tt.Regexp, testFlags);
        if (err != nil) {
            t.Errorf("Parse(%#q): %v", tt.Regexp, err);
            continue;
        }
        if (tt.Dump == ""u8) {
            // It parsed. That's all we care about.
            continue;
        }
        @string d = dump(re);
        if (d != tt.Dump) {
            t.Errorf("Parse(%#q).Dump() = %#q want %#q", tt.Regexp, d, tt.Dump);
            continue;
        }

        @string s = re.String();
        if (s != tt.Regexp) {
            // If ToString didn't return the original regexp,
            // it must have found one with fewer parens.
            // Unfortunately we can't check the length here, because
            // ToString produces "\\{" for a literal brace,
            // but "{" is a shorter equivalent in some contexts.
            var (nre, errΔ1) = Parse(s, testFlags);
            if (errΔ1 != nil) {
                t.Errorf("Parse(%#q.String() = %#q): %v", tt.Regexp, s, errΔ1);
                continue;
            }
            @string nd = dump(nre);
            if (d != nd) {
                t.Errorf("Parse(%#q) -> %#q; %#q vs %#q", tt.Regexp, s, d, nd);
            }

            @string ns = nre.String();
            if (s != ns) {
                t.Errorf("Parse(%#q) -> %#q -> %#q", tt.Regexp, s, ns);
            }
        }
    }
}

partial struct stringTestsᴛ1 /*dyn*/ {
    internal @string re;
    internal @string @out;
}
internal static slice<stringTestsᴛ1> stringTests = slice<stringTestsᴛ1>([
    new(@"x(?i:ab*c|d?e)1", @"x(?i:AB*C|D?E)1"),
    new(@"x(?i:ab*cd?e)1", @"x(?i:AB*CD?E)1"),
    new(@"0(?i:ab*c|d?e)1", @"(?i:0(?:AB*C|D?E)1)"),
    new(@"0(?i:ab*cd?e)1", @"(?i:0AB*CD?E1)"),
    new(@"x(?i:ab*c|d?e)", @"x(?i:AB*C|D?E)"),
    new(@"x(?i:ab*cd?e)", @"x(?i:AB*CD?E)"),
    new(@"0(?i:ab*c|d?e)", @"(?i:0(?:AB*C|D?E))"),
    new(@"0(?i:ab*cd?e)", @"(?i:0AB*CD?E)"),
    new(@"(?i:ab*c|d?e)1", @"(?i:(?:AB*C|D?E)1)"),
    new(@"(?i:ab*cd?e)1", @"(?i:AB*CD?E1)"),
    new(@"(?i:ab)[123](?i:cd)", @"(?i:AB[1-3]CD)"),
    new(@"(?i:ab*c|d?e)", @"(?i:AB*C|D?E)"),
    new(@"[Aa][Bb]", @"(?i:AB)"),
    new(@"[Aa][Bb]*[Cc]", @"(?i:AB*C)"),
    new(@"A(?:[Bb][Cc]|[Dd])[Zz]", @"A(?i:(?:BC|D)Z)"),
    new(@"[Aa](?:[Bb][Cc]|[Dd])Z", @"(?i:A(?:BC|D))Z")
]);

public static void TestString(ж<testing.T> t) {
    foreach (var (_, tt) in stringTests) {
        var (re, err) = Parse(tt.re, Perl);
        if (err != nil) {
            t.Errorf("Parse(%#q): %v", tt.re, err);
            continue;
        }
        @string @out = re.String();
        if (@out != tt.@out) {
            t.Errorf("Parse(%#q).String() = %#q, want %#q", tt.re, @out, tt.@out);
        }
    }
}

} // end syntax_internal_test_package
