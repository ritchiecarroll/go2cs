// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using testing = testing_package;
using static go.regexp.syntax_package;

partial class syntax_internal_test_package {

// Already-simple constructs
// Posix character classes
// Perl character classes
// Posix repetitions
// The next three are illegible because Simplify inserts (?:)
// parens instead of () parens to avoid creating extra
// captured subexpressions. The comments show a version with fewer parens.
//       (aa?)?
//   (a(a(aa?)?)?)?
// aa(a(a(aa?)?)?)?
//       (aa?)?
//   (a(a(aa?)?)?)?
// aa(a(a(aa?)?)?)?
// Test that operators simplify their arguments.
// Character class simplification
// Empty character classes
// Full character classes
// Unicode case folding.
// Empty string as a regular expression.
// The empty string must be preserved inside parens in order
// to make submatches work right, so these tests are less
// interesting than they might otherwise be. String inserts
// explicit (?:) in place of non-parenthesized empty strings,
// to make them easier to spot for other parsers.

partial struct simplifyTestsᴛ1 /*dyn*/ {
    public @string Regexp;
    public @string Simple;
}
internal static slice<simplifyTestsᴛ1> simplifyTests = new simplifyTestsᴛ1[]{
    new(@"a"u8, @"a"u8),
    new(@"ab"u8, @"ab"u8),
    new(@"a|b"u8, @"[ab]"u8),
    new(@"ab|cd"u8, @"ab|cd"u8),
    new(@"(ab)*"u8, @"(ab)*"u8),
    new(@"(ab)+"u8, @"(ab)+"u8),
    new(@"(ab)?"u8, @"(ab)?"u8),
    new(@"."u8, @"(?s:.)"u8),
    new(@"^"u8, @"(?m:^)"u8),
    new(@"$"u8, @"(?m:$)"u8),
    new(@"[ac]"u8, @"[ac]"u8),
    new(@"[^ac]"u8, @"[^ac]"u8),
    new(@"[[:alnum:]]"u8, @"[0-9A-Za-z]"u8),
    new(@"[[:alpha:]]"u8, @"[A-Za-z]"u8),
    new(@"[[:blank:]]"u8, @"[\t ]"u8),
    new(@"[[:cntrl:]]"u8, @"[\x00-\x1f\x7f]"u8),
    new(@"[[:digit:]]"u8, @"[0-9]"u8),
    new(@"[[:graph:]]"u8, @"[!-~]"u8),
    new(@"[[:lower:]]"u8, @"[a-z]"u8),
    new(@"[[:print:]]"u8, @"[ -~]"u8),
    new(@"[[:punct:]]"u8, "[!-/:-@\\[-`\\{-~]"u8),
    new(@"[[:space:]]"u8, @"[\t-\r ]"u8),
    new(@"[[:upper:]]"u8, @"[A-Z]"u8),
    new(@"[[:xdigit:]]"u8, @"[0-9A-Fa-f]"u8),
    new(@"\d"u8, @"[0-9]"u8),
    new(@"\s"u8, @"[\t\n\f\r ]"u8),
    new(@"\w"u8, @"[0-9A-Z_a-z]"u8),
    new(@"\D"u8, @"[^0-9]"u8),
    new(@"\S"u8, @"[^\t\n\f\r ]"u8),
    new(@"\W"u8, @"[^0-9A-Z_a-z]"u8),
    new(@"[\d]"u8, @"[0-9]"u8),
    new(@"[\s]"u8, @"[\t\n\f\r ]"u8),
    new(@"[\w]"u8, @"[0-9A-Z_a-z]"u8),
    new(@"[\D]"u8, @"[^0-9]"u8),
    new(@"[\S]"u8, @"[^\t\n\f\r ]"u8),
    new(@"[\W]"u8, @"[^0-9A-Z_a-z]"u8),
    new(@"a{1}"u8, @"a"u8),
    new(@"a{2}"u8, @"aa"u8),
    new(@"a{5}"u8, @"aaaaa"u8),
    new(@"a{0,1}"u8, @"a?"u8),
    new(@"(a){0,2}"u8, @"(?:(a)(a)?)?"u8),
    new(@"(a){0,4}"u8, @"(?:(a)(?:(a)(?:(a)(a)?)?)?)?"u8),
    new(@"(a){2,6}"u8, @"(a)(a)(?:(a)(?:(a)(?:(a)(a)?)?)?)?"u8),
    new(@"a{0,2}"u8, @"(?:aa?)?"u8),
    new(@"a{0,4}"u8, @"(?:a(?:a(?:aa?)?)?)?"u8),
    new(@"a{2,6}"u8, @"aa(?:a(?:a(?:aa?)?)?)?"u8),
    new(@"a{0,}"u8, @"a*"u8),
    new(@"a{1,}"u8, @"a+"u8),
    new(@"a{2,}"u8, @"aa+"u8),
    new(@"a{5,}"u8, @"aaaaa+"u8),
    new(@"(?:a{1,}){1,}"u8, @"a+"u8),
    new(@"(a{1,}b{1,})"u8, @"(a+b+)"u8),
    new(@"a{1,}|b{1,}"u8, @"a+|b+"u8),
    new(@"(?:a{1,})*"u8, @"(?:a+)*"u8),
    new(@"(?:a{1,})+"u8, @"a+"u8),
    new(@"(?:a{1,})?"u8, @"(?:a+)?"u8),
    new(@""u8, @"(?:)"u8),
    new(@"a{0}"u8, @"(?:)"u8),
    new(@"[ab]"u8, @"[ab]"u8),
    new(@"[abc]"u8, @"[a-c]"u8),
    new(@"[a-za-za-z]"u8, @"[a-z]"u8),
    new(@"[A-Za-zA-Za-z]"u8, @"[A-Za-z]"u8),
    new(@"[ABCDEFGH]"u8, @"[A-H]"u8),
    new(@"[AB-CD-EF-GH]"u8, @"[A-H]"u8),
    new(@"[W-ZP-XE-R]"u8, @"[E-Z]"u8),
    new(@"[a-ee-gg-m]"u8, @"[a-m]"u8),
    new(@"[a-ea-ha-m]"u8, @"[a-m]"u8),
    new(@"[a-ma-ha-e]"u8, @"[a-m]"u8),
    new(@"[a-zA-Z0-9 -~]"u8, @"[ -~]"u8),
    new(@"[^[:cntrl:][:^cntrl:]]"u8, @"[^\x00-\x{10FFFF}]"u8),
    new(@"[[:cntrl:][:^cntrl:]]"u8, @"(?s:.)"u8),
    new(@"(?i)A"u8, @"(?i:A)"u8),
    new(@"(?i)a"u8, @"(?i:A)"u8),
    new(@"(?i)[A]"u8, @"(?i:A)"u8),
    new(@"(?i)[a]"u8, @"(?i:A)"u8),
    new(@"(?i)K"u8, @"(?i:K)"u8),
    new(@"(?i)k"u8, @"(?i:K)"u8),
    new(@"(?i)\x{212a}"u8, "(?i:K)"u8),
    new(@"(?i)[K]"u8, "[Kk\u212A]"u8),
    new(@"(?i)[k]"u8, "[Kk\u212A]"u8),
    new(@"(?i)[\x{212a}]"u8, "[Kk\u212A]"u8),
    new(@"(?i)[a-z]"u8, "[A-Za-z\u017F\u212A]"u8),
    new(@"(?i)[\x00-\x{FFFD}]"u8, "[\\x00-\uFFFD]"u8),
    new(@"(?i)[\x00-\x{10FFFF}]"u8, @"(?s:.)"u8),
    new(@"(a|b|c|)"u8, @"([a-c]|(?:))"u8),
    new(@"(a|b|)"u8, @"([ab]|(?:))"u8),
    new(@"(|)"u8, @"()"u8),
    new(@"a()"u8, @"a()"u8),
    new(@"(()|())"u8, @"(()|())"u8),
    new(@"(a|)"u8, @"(a|(?:))"u8),
    new(@"ab()cd()"u8, @"ab()cd()"u8),
    new(@"()"u8, @"()"u8),
    new(@"()*"u8, @"()*"u8),
    new(@"()+"u8, @"()+"u8),
    new(@"()?"u8, @"()?"u8),
    new(@"(){0}"u8, @"(?:)"u8),
    new(@"(){1}"u8, @"()"u8),
    new(@"(){1,}"u8, @"()+"u8),
    new(@"(){0,2}"u8, @"(?:()()?)?"u8)
}.slice();

public static void TestSimplify(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in simplifyTests) {
        var (re, err) = Parse(tt.Regexp, (global::go.regexp.syntax_package.Flags)(MatchNL | (global::go.regexp.syntax_package.Flags)(Perl & ~OneLine)));
        if (err != default!) {
            Ꮡt.Errorf("Parse(%#q) = error %v"u8, tt.Regexp, err);
            continue;
        }
        @string s = re.Simplify().String();
        if (s != tt.Simple) {
            Ꮡt.Errorf("Simplify(%#q) = %#q, want %#q"u8, tt.Regexp, s, tt.Simple);
        }
    }
}

} // end syntax_internal_test_package
