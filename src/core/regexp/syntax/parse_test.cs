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

// Base cases
// Malformed { } are treated as literals.
// alt{emp{}emp{}} but got factored
// Posix and Perl extensions
//	{ `\C`, `byte{}` },  // probably never
// Unicode, negatives, and a double negative.
// Hex, octal.
// More interesting regular expressions.
// utf-8
// Test precedences
// Test flattening.
// Test Perl quoted literals
// Test Perl \A and \z
// Test named captures
// Case-folded literals
// Strings
// Factoring.
// Bug fixes.
// RE2 prefix_tests
// Valid repetitions.
// Valid nesting.
// not nested at all
internal static slice<parseTest> parseTests;
internal static void initᴛparseTests() { parseTests = new parseTest[]{
    new(@"a"u8, @"lit{a}"u8),
    new(@"a."u8, @"cat{lit{a}dot{}}"u8),
    new(@"a.b"u8, @"cat{lit{a}dot{}lit{b}}"u8),
    new(@"ab"u8, @"str{ab}"u8),
    new(@"a.b.c"u8, @"cat{lit{a}dot{}lit{b}dot{}lit{c}}"u8),
    new(@"abc"u8, @"str{abc}"u8),
    new(@"a|^"u8, @"alt{lit{a}bol{}}"u8),
    new(@"a|b"u8, @"cc{0x61-0x62}"u8),
    new(@"(a)"u8, @"cap{lit{a}}"u8),
    new(@"(a)|b"u8, @"alt{cap{lit{a}}lit{b}}"u8),
    new(@"a*"u8, @"star{lit{a}}"u8),
    new(@"a+"u8, @"plus{lit{a}}"u8),
    new(@"a?"u8, @"que{lit{a}}"u8),
    new(@"a{2}"u8, @"rep{2,2 lit{a}}"u8),
    new(@"a{2,3}"u8, @"rep{2,3 lit{a}}"u8),
    new(@"a{2,}"u8, @"rep{2,-1 lit{a}}"u8),
    new(@"a*?"u8, @"nstar{lit{a}}"u8),
    new(@"a+?"u8, @"nplus{lit{a}}"u8),
    new(@"a??"u8, @"nque{lit{a}}"u8),
    new(@"a{2}?"u8, @"nrep{2,2 lit{a}}"u8),
    new(@"a{2,3}?"u8, @"nrep{2,3 lit{a}}"u8),
    new(@"a{2,}?"u8, @"nrep{2,-1 lit{a}}"u8),
    new(@"x{1001"u8, @"str{x{1001}"u8),
    new(@"x{9876543210"u8, @"str{x{9876543210}"u8),
    new(@"x{9876543210,"u8, @"str{x{9876543210,}"u8),
    new(@"x{2,1"u8, @"str{x{2,1}"u8),
    new(@"x{1,9876543210"u8, @"str{x{1,9876543210}"u8),
    new(@""u8, @"emp{}"u8),
    new(@"|"u8, @"emp{}"u8),
    new(@"|x|"u8, @"alt{emp{}lit{x}emp{}}"u8),
    new(@"."u8, @"dot{}"u8),
    new(@"^"u8, @"bol{}"u8),
    new(@"$"u8, @"eol{}"u8),
    new(@"\|"u8, @"lit{|}"u8),
    new(@"\("u8, @"lit{(}"u8),
    new(@"\)"u8, @"lit{)}"u8),
    new(@"\*"u8, @"lit{*}"u8),
    new(@"\+"u8, @"lit{+}"u8),
    new(@"\?"u8, @"lit{?}"u8),
    new(@"{"u8, @"lit{{}"u8),
    new(@"}"u8, @"lit{}}"u8),
    new(@"\."u8, @"lit{.}"u8),
    new(@"\^"u8, @"lit{^}"u8),
    new(@"\$"u8, @"lit{$}"u8),
    new(@"\\"u8, @"lit{\}"u8),
    new(@"[ace]"u8, @"cc{0x61 0x63 0x65}"u8),
    new(@"[abc]"u8, @"cc{0x61-0x63}"u8),
    new(@"[a-z]"u8, @"cc{0x61-0x7a}"u8),
    new(@"[a]"u8, @"lit{a}"u8),
    new(@"\-"u8, @"lit{-}"u8),
    new(@"-"u8, @"lit{-}"u8),
    new(@"\_"u8, @"lit{_}"u8),
    new(@"abc"u8, @"str{abc}"u8),
    new(@"abc|def"u8, @"alt{str{abc}str{def}}"u8),
    new(@"abc|def|ghi"u8, @"alt{str{abc}str{def}str{ghi}}"u8),
    new(@"[[:lower:]]"u8, @"cc{0x61-0x7a}"u8),
    new(@"[a-z]"u8, @"cc{0x61-0x7a}"u8),
    new(@"[^[:lower:]]"u8, @"cc{0x0-0x60 0x7b-0x10ffff}"u8),
    new(@"[[:^lower:]]"u8, @"cc{0x0-0x60 0x7b-0x10ffff}"u8),
    new(@"(?i)[[:lower:]]"u8, @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"u8),
    new(@"(?i)[a-z]"u8, @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"u8),
    new(@"(?i)[^[:lower:]]"u8, @"cc{0x0-0x40 0x5b-0x60 0x7b-0x17e 0x180-0x2129 0x212b-0x10ffff}"u8),
    new(@"(?i)[[:^lower:]]"u8, @"cc{0x0-0x40 0x5b-0x60 0x7b-0x17e 0x180-0x2129 0x212b-0x10ffff}"u8),
    new(@"\d"u8, @"cc{0x30-0x39}"u8),
    new(@"\D"u8, @"cc{0x0-0x2f 0x3a-0x10ffff}"u8),
    new(@"\s"u8, @"cc{0x9-0xa 0xc-0xd 0x20}"u8),
    new(@"\S"u8, @"cc{0x0-0x8 0xb 0xe-0x1f 0x21-0x10ffff}"u8),
    new(@"\w"u8, @"cc{0x30-0x39 0x41-0x5a 0x5f 0x61-0x7a}"u8),
    new(@"\W"u8, @"cc{0x0-0x2f 0x3a-0x40 0x5b-0x5e 0x60 0x7b-0x10ffff}"u8),
    new(@"(?i)\w"u8, @"cc{0x30-0x39 0x41-0x5a 0x5f 0x61-0x7a 0x17f 0x212a}"u8),
    new(@"(?i)\W"u8, @"cc{0x0-0x2f 0x3a-0x40 0x5b-0x5e 0x60 0x7b-0x17e 0x180-0x2129 0x212b-0x10ffff}"u8),
    new(@"[^\\]"u8, @"cc{0x0-0x5b 0x5d-0x10ffff}"u8),
    new(@"\p{Braille}"u8, @"cc{0x2800-0x28ff}"u8),
    new(@"\P{Braille}"u8, @"cc{0x0-0x27ff 0x2900-0x10ffff}"u8),
    new(@"\p{^Braille}"u8, @"cc{0x0-0x27ff 0x2900-0x10ffff}"u8),
    new(@"\P{^Braille}"u8, @"cc{0x2800-0x28ff}"u8),
    new(@"\pZ"u8, @"cc{0x20 0xa0 0x1680 0x2000-0x200a 0x2028-0x2029 0x202f 0x205f 0x3000}"u8),
    new(@"[\p{Braille}]"u8, @"cc{0x2800-0x28ff}"u8),
    new(@"[\P{Braille}]"u8, @"cc{0x0-0x27ff 0x2900-0x10ffff}"u8),
    new(@"[\p{^Braille}]"u8, @"cc{0x0-0x27ff 0x2900-0x10ffff}"u8),
    new(@"[\P{^Braille}]"u8, @"cc{0x2800-0x28ff}"u8),
    new(@"[\pZ]"u8, @"cc{0x20 0xa0 0x1680 0x2000-0x200a 0x2028-0x2029 0x202f 0x205f 0x3000}"u8),
    new(@"\p{Lu}"u8, mkCharClass(unicode.IsUpper)),
    new(@"[\p{Lu}]"u8, mkCharClass(unicode.IsUpper)),
    new(@"(?i)[\p{Lu}]"u8, mkCharClass(isUpperFold)),
    new(@"\p{Any}"u8, @"dot{}"u8),
    new(@"\p{^Any}"u8, @"cc{}"u8),
    new(@"[\012-\234]\141"u8, @"cat{cc{0xa-0x9c}lit{a}}"u8),
    new(@"[\x{41}-\x7a]\x61"u8, @"cat{cc{0x41-0x7a}lit{a}}"u8),
    new(@"a{,2}"u8, @"str{a{,2}}"u8),
    new(@"\.\^\$\\"u8, @"str{.^$\}"u8),
    new(@"[a-zABC]"u8, @"cc{0x41-0x43 0x61-0x7a}"u8),
    new(@"[^a]"u8, @"cc{0x0-0x60 0x62-0x10ffff}"u8),
    new(@"[α-ε☺]"u8, @"cc{0x3b1-0x3b5 0x263a}"u8),
    new(@"a*{"u8, @"cat{star{lit{a}}lit{{}}"u8),
    new(@"(?:ab)*"u8, @"star{str{ab}}"u8),
    new(@"(ab)*"u8, @"star{cap{str{ab}}}"u8),
    new(@"ab|cd"u8, @"alt{str{ab}str{cd}}"u8),
    new(@"a(b|c)d"u8, @"cat{lit{a}cap{cc{0x62-0x63}}lit{d}}"u8),
    new(@"(?:a)"u8, @"lit{a}"u8),
    new(@"(?:ab)(?:cd)"u8, @"str{abcd}"u8),
    new(@"(?:a+b+)(?:c+d+)"u8, @"cat{plus{lit{a}}plus{lit{b}}plus{lit{c}}plus{lit{d}}}"u8),
    new(@"(?:a+|b+)|(?:c+|d+)"u8, @"alt{plus{lit{a}}plus{lit{b}}plus{lit{c}}plus{lit{d}}}"u8),
    new(@"(?:a|b)|(?:c|d)"u8, @"cc{0x61-0x64}"u8),
    new(@"a|."u8, @"dot{}"u8),
    new(@".|a"u8, @"dot{}"u8),
    new(@"(?:[abc]|A|Z|hello|world)"u8, @"alt{cc{0x41 0x5a 0x61-0x63}str{hello}str{world}}"u8),
    new(@"(?:[abc]|A|Z)"u8, @"cc{0x41 0x5a 0x61-0x63}"u8),
    new(@"\Q+|*?{[\E"u8, @"str{+|*?{[}"u8),
    new(@"\Q+\E+"u8, @"plus{lit{+}}"u8),
    new(@"\Qab\E+"u8, @"cat{lit{a}plus{lit{b}}}"u8),
    new(@"\Q\\E"u8, @"lit{\}"u8),
    new(@"\Q\\\E"u8, @"str{\\}"u8),
    new(@"(?m)^"u8, @"bol{}"u8),
    new(@"(?m)$"u8, @"eol{}"u8),
    new(@"(?-m)^"u8, @"bot{}"u8),
    new(@"(?-m)$"u8, @"eot{}"u8),
    new(@"(?m)\A"u8, @"bot{}"u8),
    new(@"(?m)\z"u8, @"eot{\z}"u8),
    new(@"(?-m)\A"u8, @"bot{}"u8),
    new(@"(?-m)\z"u8, @"eot{\z}"u8),
    new(@"(?P<name>a)"u8, @"cap{name:lit{a}}"u8),
    new(@"(?<name>a)"u8, @"cap{name:lit{a}}"u8),
    new(@"[Aa]"u8, @"litfold{A}"u8),
    new(@"[\x{100}\x{101}]"u8, @"litfold{Ā}"u8),
    new(@"[Δδ]"u8, @"litfold{Δ}"u8),
    new(@"abcde"u8, @"str{abcde}"u8),
    new(@"[Aa][Bb]cd"u8, @"cat{strfold{AB}str{cd}}"u8),
    new(@"abc|abd|aef|bcx|bcy"u8, @"alt{cat{lit{a}alt{cat{lit{b}cc{0x63-0x64}}str{ef}}}cat{str{bc}cc{0x78-0x79}}}"u8),
    new(@"ax+y|ax+z|ay+w"u8, @"cat{lit{a}alt{cat{plus{lit{x}}lit{y}}cat{plus{lit{x}}lit{z}}cat{plus{lit{y}}lit{w}}}}"u8),
    new(@"(?:.)"u8, @"dot{}"u8),
    new(@"(?:x|(?:xa))"u8, @"cat{lit{x}alt{emp{}lit{a}}}"u8),
    new(@"(?:.|(?:.a))"u8, @"cat{dot{}alt{emp{}lit{a}}}"u8),
    new(@"(?:A(?:A|a))"u8, @"cat{lit{A}litfold{A}}"u8),
    new(@"(?:A|a)"u8, @"litfold{A}"u8),
    new(@"A|(?:A|a)"u8, @"litfold{A}"u8),
    new(@"(?s)."u8, @"dot{}"u8),
    new(@"(?-s)."u8, @"dnl{}"u8),
    new(@"(?:(?:^).)"u8, @"cat{bol{}dot{}}"u8),
    new(@"(?-s)(?:(?:^).)"u8, @"cat{bol{}dnl{}}"u8),
    new(@"[\s\S]a"u8, @"cat{cc{0x0-0x10ffff}lit{a}}"u8),
    new(@"abc|abd"u8, @"cat{str{ab}cc{0x63-0x64}}"u8),
    new(@"a(?:b)c|abd"u8, @"cat{str{ab}cc{0x63-0x64}}"u8),
    new(@"abc|abd|aef|bcx|bcy"u8,
        @"alt{cat{lit{a}alt{cat{lit{b}cc{0x63-0x64}}str{ef}}}"u8 + @"cat{str{bc}cc{0x78-0x79}}}"u8),
    new(@"abc|x|abd"u8, @"alt{str{abc}lit{x}str{abd}}"u8),
    new(@"(?i)abc|ABD"u8, @"cat{strfold{AB}cc{0x43-0x44 0x63-0x64}}"u8),
    new(@"[ab]c|[ab]d"u8, @"cat{cc{0x61-0x62}cc{0x63-0x64}}"u8),
    new(@".c|.d"u8, @"cat{dot{}cc{0x63-0x64}}"u8),
    new(@"x{2}|x{2}[0-9]"u8,
        @"cat{rep{2,2 lit{x}}alt{emp{}cc{0x30-0x39}}}"u8),
    new(@"x{2}y|x{2}[0-9]y"u8,
        @"cat{rep{2,2 lit{x}}alt{lit{y}cat{cc{0x30-0x39}lit{y}}}}"u8),
    new(@"a.*?c|a.*?b"u8,
        @"cat{lit{a}alt{cat{nstar{dot{}}lit{c}}cat{nstar{dot{}}lit{b}}}}"u8),
    new(@"((((((((((x{2}){2}){2}){2}){2}){2}){2}){2}){2}))"u8, @""u8),
    new(@"((((((((((x{1}){2}){2}){2}){2}){2}){2}){2}){2}){2})"u8, @""u8),
    new(strings.Repeat("("u8, 999) + strings.Repeat(")"u8, 999), @""u8),
    new(strings.Repeat("(?:"u8, 999) + strings.Repeat(")*"u8, 999), @""u8),
    new("("u8 + strings.Repeat("|"u8, 12345) + ")"u8, @""u8)
}.slice(); }

internal static global::go.regexp.syntax_package.Flags testFlags => /* MatchNL | PerlX | UnicodeGroups */ 204;

public static void TestParseSimple(ж<testing.T> Ꮡt) {
    testParseDump(Ꮡt, parseTests, testFlags);
}

// 0x17F is an old English long s (looks like an f) and folds to s.
// 0x212A is the Kelvin symbol and folds to k.
// [Aa][A-z...]
internal static slice<parseTest> foldcaseTests = new parseTest[]{
    new(@"AbCdE"u8, @"strfold{ABCDE}"u8),
    new(@"[Aa]"u8, @"litfold{A}"u8),
    new(@"a"u8, @"litfold{A}"u8),
    new(@"A[F-g]"u8, @"cat{litfold{A}cc{0x41-0x7a 0x17f 0x212a}}"u8),
    new(@"[[:upper:]]"u8, @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"u8),
    new(@"[[:lower:]]"u8, @"cc{0x41-0x5a 0x61-0x7a 0x17f 0x212a}"u8)
}.slice();

public static void TestParseFoldCase(ж<testing.T> Ꮡt) {
    testParseDump(Ꮡt, foldcaseTests, FoldCase);
}

internal static slice<parseTest> literalTests = new parseTest[]{
    new("(|)^$.[*+?]{5,10},\\"u8, "str{(|)^$.[*+?]{5,10},\\}"u8)
}.slice();

public static void TestParseLiteral(ж<testing.T> Ꮡt) {
    testParseDump(Ꮡt, literalTests, Literal);
}

internal static slice<parseTest> matchnlTests = new parseTest[]{
    new(@"."u8, @"dot{}"u8),
    new("\n"u8, "lit{\n}"u8),
    new(@"[^a]"u8, @"cc{0x0-0x60 0x62-0x10ffff}"u8),
    new(@"[a\n]"u8, @"cc{0xa 0x61}"u8)
}.slice();

public static void TestParseMatchNL(ж<testing.T> Ꮡt) {
    testParseDump(Ꮡt, matchnlTests, MatchNL);
}

internal static slice<parseTest> nomatchnlTests = new parseTest[]{
    new(@"."u8, @"dnl{}"u8),
    new("\n"u8, "lit{\n}"u8),
    new(@"[^a]"u8, @"cc{0x0-0x9 0xb-0x60 0x62-0x10ffff}"u8),
    new(@"[a\n]"u8, @"cc{0xa 0x61}"u8)
}.slice();

public static void TestParseNoMatchNL(ж<testing.T> Ꮡt) {
    testParseDump(Ꮡt, nomatchnlTests, 0);
}

// Test Parse -> Dump.
internal static void testParseDump(ж<testing.T> Ꮡt, slice<parseTest> tests, global::go.regexp.syntax_package.Flags flags) {
    foreach (var (_, tt) in tests) {
        var (re, err) = Parse(tt.Regexp, flags);
        if (err != default!) {
            Ꮡt.Errorf("Parse(%#q): %v"u8, tt.Regexp, err);
            continue;
        }
        if (tt.Dump == ""u8) {
            // It parsed. That's all we care about.
            continue;
        }
        @string d = dump(re);
        if (d != tt.Dump) {
            Ꮡt.Errorf("Parse(%#q).Dump() = %#q want %#q"u8, tt.Regexp, d, tt.Dump);
        }
    }
}

// dump prints a string representation of the regexp showing
// the structure explicitly.
internal static @string dump(ж<global::go.regexp.syntax_package.Regexp> Ꮡre) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    dumpRegexp(Ꮡb, Ꮡre);
    return b.String();
}

internal static slice<@string> opNames = new golib.SparseArray<@string>{
    [OpNoMatch] = "no"u8,
    [OpEmptyMatch] = "emp"u8,
    [OpLiteral] = "lit"u8,
    [OpCharClass] = "cc"u8,
    [OpAnyCharNotNL] = "dnl"u8,
    [OpAnyChar] = "dot"u8,
    [OpBeginLine] = "bol"u8,
    [OpEndLine] = "eol"u8,
    [OpBeginText] = "bot"u8,
    [OpEndText] = "eot"u8,
    [OpWordBoundary] = "wb"u8,
    [OpNoWordBoundary] = "nwb"u8,
    [OpCapture] = "cap"u8,
    [OpStar] = "star"u8,
    [OpPlus] = "plus"u8,
    [OpQuest] = "que"u8,
    [OpRepeat] = "rep"u8,
    [OpConcat] = "cat"u8,
    [OpAlternate] = "alt"u8
}.slice();

// dumpRegexp writes an encoding of the syntax tree for the regexp re to b.
// It is used during testing to distinguish between parses that might print
// the same using re's String method.
internal static void dumpRegexp(ж<strings.Builder> Ꮡb, ж<global::go.regexp.syntax_package.Regexp> Ꮡre) {
    ref var re = ref Ꮡre.DerefOrNull();

    if ((nint)(uint8)re.Op >= len(opNames) || opNames[re.Op] == ""){
        fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(Ꮡb), "op%d"u8, re.Op);
    } else {
        var exprᴛ1 = re.Op;
        if (exprᴛ1 == OpStar || exprᴛ1 == OpPlus || exprᴛ1 == OpQuest || exprᴛ1 == OpRepeat) {
            if ((global::go.regexp.syntax_package.Flags)(re.Flags & NonGreedy) != 0) {
                Ꮡb.WriteByte((rune)'n');
            }
            Ꮡb.WriteString(opNames[re.Op]);
        }
        else if (exprᴛ1 == OpLiteral) {
            if (len(re.Rune) > 1){
                Ꮡb.WriteString("str"u8);
            } else {
                Ꮡb.WriteString("lit"u8);
            }
            if ((global::go.regexp.syntax_package.Flags)(re.Flags & FoldCase) != 0) {
                foreach (var (_, r) in re.Rune) {
                    if (unicode.SimpleFold(r) != r) {
                        Ꮡb.WriteString("fold"u8);
                        break;
                    }
                }
            }
        }
        else { /* default: */
            Ꮡb.WriteString(opNames[re.Op]);
        }

    }
    Ꮡb.WriteByte((rune)'{');
    var exprᴛ2 = re.Op;
    if (exprᴛ2 == OpEndText) {
        if ((global::go.regexp.syntax_package.Flags)(re.Flags & WasDollar) == 0) {
            Ꮡb.WriteString(@"\z"u8);
        }
    }
    else if (exprᴛ2 == OpLiteral) {
        foreach (var (_, r) in re.Rune) {
            Ꮡb.WriteRune(r);
        }
    }
    else if (exprᴛ2 == OpConcat || exprᴛ2 == OpAlternate) {
        foreach (var (_, sub) in re.Sub) {
            dumpRegexp(Ꮡb, sub);
        }
    }
    else if (exprᴛ2 == OpStar || exprᴛ2 == OpPlus || exprᴛ2 == OpQuest) {
        dumpRegexp(Ꮡb, re.Sub[0]);
    }
    else if (exprᴛ2 == OpRepeat) {
        fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(Ꮡb), "%d,%d "u8, re.Min, re.Max);
        dumpRegexp(Ꮡb, re.Sub[0]);
    }
    else if (exprᴛ2 == OpCapture) {
        if (re.Name != ""u8) {
            Ꮡb.WriteString(re.Name);
            Ꮡb.WriteByte((rune)':');
        }
        dumpRegexp(Ꮡb, re.Sub[0]);
    }
    else if (exprᴛ2 == OpCharClass) {
        @string sep = ""u8;
        for (nint i = 0; i < len(re.Rune); i += 2) {
            Ꮡb.WriteString(sep);
            sep = " "u8;
            var (lo, hi) = (re.Rune[i], re.Rune[i + 1]);
            if (lo == hi){
                fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(Ꮡb), "%#x"u8, lo);
            } else {
                fmt.Fprintf(new syntax_internal_test_package.strings_BuilderжWriter(Ꮡb), "%#x-%#x"u8, lo, hi);
            }
        }
    }

    Ꮡb.WriteByte((rune)'}');
}

internal static @string mkCharClass(Func<rune, bool> f) {
    var re = Ꮡ(new Regexp(Op: OpCharClass));
    var lo = (rune)(-1);
    for (var i = (rune)0; i <= unicode.MaxRune; i++) {
        if (f(i)){
            if (lo < 0) {
                lo = i;
            }
        } else {
            if (lo >= 0) {
                re.Value.Rune = builtin.append((~re).Rune, lo, i - 1);
                lo = -1;
            }
        }
    }
    if (lo >= 0) {
        re.Value.Rune = builtin.append((~re).Rune, lo, (rune)(unicode.MaxRune));
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

public static void TestFoldConstants(ж<testing.T> Ꮡt) {
    var last = (rune)(-1);
    for (var i = (rune)0; i <= unicode.MaxRune; i++) {
        if (unicode.SimpleFold(i) == i) {
            continue;
        }
        if (last == -1 && minFold != i) {
            Ꮡt.Errorf("minFold=%#U should be %#U"u8, (nint)(minFold), i);
        }
        last = i;
    }
    if (maxFold != last) {
        Ꮡt.Errorf("maxFold=%#U should be %#U"u8, (nint)(maxFold), last);
    }
}

public static void TestAppendRangeCollapse(ж<testing.T> Ꮡt) {
    // AppendRange should collapse each of the new ranges
    // into the earlier ones (it looks back two ranges), so that
    // the slice never grows very large.
    // Note that we are not calling cleanClass.
    slice<rune> r = default!;
    for (var i = (rune)(rune)'A'; i <= (rune)'Z'; i++) {
        r = appendRange(r, i, i);
        r = appendRange(r, i + (rune)'a' - (rune)'A', i + (rune)'a' - (rune)'A');
    }
    if (((@string)r) != "AZaz"u8) {
        Ꮡt.Errorf("appendRange interlaced A-Z a-z = %s, want AZaz"u8, ((@string)r));
    }
}

// Invalid UTF-8
// too much repetition
// too much repetition
// too much repetition
// too deep
// too deep
// too long
// too long
// too many runes
internal static slice<@string> invalidRegexps = new @string[]{
    @"("u8,
    @")"u8,
    @"(a"u8,
    @"a)"u8,
    @"(a))"u8,
    @"(a|b|"u8,
    @"a|b|)"u8,
    @"(a|b|))"u8,
    @"(a|b"u8,
    @"a|b)"u8,
    @"(a|b))"u8,
    @"[a-z"u8,
    @"([a-z)"u8,
    @"[a-z)"u8,
    @"([a-z]))"u8,
    @"x{1001}"u8,
    @"x{9876543210}"u8,
    @"x{2,1}"u8,
    @"x{1,9876543210}"u8,
    ((@string)(new byte[]{0xff})),
    ((@string)(new byte[]{0x5b, 0xff, 0x5d})),
    ((@string)(new byte[]{0x5b, 0x5c, 0xff, 0x5d})),
    ((@string)(new byte[]{0x5c, 0xff})),
    @"(?P<name>a"u8,
    @"(?P<name>"u8,
    @"(?P<name"u8,
    @"(?P<x y>a)"u8,
    @"(?P<>a)"u8,
    @"(?<name>a"u8,
    @"(?<name>"u8,
    @"(?<name"u8,
    @"(?<x y>a)"u8,
    @"(?<>a)"u8,
    @"[a-Z]"u8,
    @"(?i)[a-Z]"u8,
    @"\Q\E*"u8,
    @"a{100000}"u8,
    @"a{100000,}"u8,
    "((((((((((x{2}){2}){2}){2}){2}){2}){2}){2}){2}){2})"u8,
    strings.Repeat("("u8, 1000) + strings.Repeat(")"u8, 1000),
    strings.Repeat("(?:"u8, 1000) + strings.Repeat(")*"u8, 1000),
    "("u8 + strings.Repeat("(xx?)"u8, 1000) + "){1000}"u8,
    strings.Repeat("(xx?){1000}"u8, 1000),
    strings.Repeat(@"\pL"u8, 27000)
}.slice();

internal static slice<@string> onlyPerl = new @string[]{
    @"[a-b-c]"u8,
    @"\Qabc\E"u8,
    @"\Q*+?{[\E"u8,
    @"\Q\\E"u8,
    @"\Q\\\E"u8,
    @"\Q\\\\E"u8,
    @"\Q\\\\\E"u8,
    @"(?:a)"u8,
    @"(?P<name>a)"u8
}.slice();

internal static slice<@string> onlyPOSIX = new @string[]{
    "a++"u8,
    "a**"u8,
    "a?*"u8,
    "a+*"u8,
    "a{1}*"u8,
    ".{1}{2}.{3}"u8
}.slice();

public static void TestParseInvalidRegexps(ж<testing.T> Ꮡt) {
    foreach (var (_, regexp) in invalidRegexps) {
        {
            var (re, err) = Parse(regexp, Perl); if (err == default!) {
                Ꮡt.Errorf("Parse(%#q, Perl) = %s, should have failed"u8, regexp, dump(re));
            }
        }
        {
            var (re, err) = Parse(regexp, POSIX); if (err == default!) {
                Ꮡt.Errorf("Parse(%#q, POSIX) = %s, should have failed"u8, regexp, dump(re));
            }
        }
    }
    foreach (var (_, regexp) in onlyPerl) {
        {
            var (_, err) = Parse(regexp, Perl); if (err != default!) {
                Ꮡt.Errorf("Parse(%#q, Perl): %v"u8, regexp, err);
            }
        }
        {
            var (re, err) = Parse(regexp, POSIX); if (err == default!) {
                Ꮡt.Errorf("Parse(%#q, POSIX) = %s, should have failed"u8, regexp, dump(re));
            }
        }
    }
    foreach (var (_, regexp) in onlyPOSIX) {
        {
            var (re, err) = Parse(regexp, Perl); if (err == default!) {
                Ꮡt.Errorf("Parse(%#q, Perl) = %s, should have failed"u8, regexp, dump(re));
            }
        }
        {
            var (_, err) = Parse(regexp, POSIX); if (err != default!) {
                Ꮡt.Errorf("Parse(%#q, POSIX): %v"u8, regexp, err);
            }
        }
    }
}

public static void TestToStringEquivalentParse(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in parseTests) {
        var (re, err) = Parse(tt.Regexp, testFlags);
        if (err != default!) {
            Ꮡt.Errorf("Parse(%#q): %v"u8, tt.Regexp, err);
            continue;
        }
        if (tt.Dump == ""u8) {
            // It parsed. That's all we care about.
            continue;
        }
        @string d = dump(re);
        if (d != tt.Dump) {
            Ꮡt.Errorf("Parse(%#q).Dump() = %#q want %#q"u8, tt.Regexp, d, tt.Dump);
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
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Parse(%#q.String() = %#q): %v"u8, tt.Regexp, s, errΔ1);
                continue;
            }
            @string nd = dump(nre);
            if (d != nd) {
                Ꮡt.Errorf("Parse(%#q) -> %#q; %#q vs %#q"u8, tt.Regexp, s, d, nd);
            }
            @string ns = nre.String();
            if (s != ns) {
                Ꮡt.Errorf("Parse(%#q) -> %#q -> %#q"u8, tt.Regexp, s, ns);
            }
        }
    }
}


partial struct stringTestsᴛ1 /*dyn*/ {
    internal @string re;
    internal @string @out;
}
internal static slice<stringTestsᴛ1> stringTests = new stringTestsᴛ1[]{
    new(@"x(?i:ab*c|d?e)1"u8, @"x(?i:AB*C|D?E)1"u8),
    new(@"x(?i:ab*cd?e)1"u8, @"x(?i:AB*CD?E)1"u8),
    new(@"0(?i:ab*c|d?e)1"u8, @"(?i:0(?:AB*C|D?E)1)"u8),
    new(@"0(?i:ab*cd?e)1"u8, @"(?i:0AB*CD?E1)"u8),
    new(@"x(?i:ab*c|d?e)"u8, @"x(?i:AB*C|D?E)"u8),
    new(@"x(?i:ab*cd?e)"u8, @"x(?i:AB*CD?E)"u8),
    new(@"0(?i:ab*c|d?e)"u8, @"(?i:0(?:AB*C|D?E))"u8),
    new(@"0(?i:ab*cd?e)"u8, @"(?i:0AB*CD?E)"u8),
    new(@"(?i:ab*c|d?e)1"u8, @"(?i:(?:AB*C|D?E)1)"u8),
    new(@"(?i:ab*cd?e)1"u8, @"(?i:AB*CD?E1)"u8),
    new(@"(?i:ab)[123](?i:cd)"u8, @"(?i:AB[1-3]CD)"u8),
    new(@"(?i:ab*c|d?e)"u8, @"(?i:AB*C|D?E)"u8),
    new(@"[Aa][Bb]"u8, @"(?i:AB)"u8),
    new(@"[Aa][Bb]*[Cc]"u8, @"(?i:AB*C)"u8),
    new(@"A(?:[Bb][Cc]|[Dd])[Zz]"u8, @"A(?i:(?:BC|D)Z)"u8),
    new(@"[Aa](?:[Bb][Cc]|[Dd])Z"u8, @"(?i:A(?:BC|D))Z"u8)
}.slice();

public static void TestString(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in stringTests) {
        var (re, err) = Parse(tt.re, Perl);
        if (err != default!) {
            Ꮡt.Errorf("Parse(%#q): %v"u8, tt.re, err);
            continue;
        }
        @string @out = re.String();
        if (@out != tt.@out) {
            Ꮡt.Errorf("Parse(%#q).String() = %#q, want %#q"u8, tt.re, @out, tt.@out);
        }
    }
}

} // end syntax_internal_test_package
