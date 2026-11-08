// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.regexp;

using testing = testing_package;
using static go.regexp.syntax_package;

partial class syntax_internal_test_package {

partial struct simplifyTestsᴛ1 /*dyn*/ {
    public @string Regexp;
    public @string Simple;
}
internal static slice<simplifyTestsᴛ1> simplifyTests = [
    // Already-simple constructs
    new(@"a", @"a"),
    new(@"ab", @"ab"),
    new(@"a|b", @"[ab]"),
    new(@"ab|cd", @"ab|cd"),
    new(@"(ab)*", @"(ab)*"),
    new(@"(ab)+", @"(ab)+"),
    new(@"(ab)?", @"(ab)?"),
    new(@".", @"(?s:.)"),
    new(@"^", @"(?m:^)"),
    new(@"$", @"(?m:$)"),
    new(@"[ac]", @"[ac]"),
    new(@"[^ac]", @"[^ac]"),

    // Posix character classes
    new(@"[[:alnum:]]", @"[0-9A-Za-z]"),
    new(@"[[:alpha:]]", @"[A-Za-z]"),
    new(@"[[:blank:]]", @"[\t ]"),
    new(@"[[:cntrl:]]", @"[\x00-\x1f\x7f]"),
    new(@"[[:digit:]]", @"[0-9]"),
    new(@"[[:graph:]]", @"[!-~]"),
    new(@"[[:lower:]]", @"[a-z]"),
    new(@"[[:print:]]", @"[ -~]"),
    new(@"[[:punct:]]", "[!-/:-@\\[-`\\{-~]"),
    new(@"[[:space:]]", @"[\t-\r ]"),
    new(@"[[:upper:]]", @"[A-Z]"),
    new(@"[[:xdigit:]]", @"[0-9A-Fa-f]"),

    // Perl character classes
    new(@"\d", @"[0-9]"),
    new(@"\s", @"[\t\n\f\r ]"),
    new(@"\w", @"[0-9A-Z_a-z]"),
    new(@"\D", @"[^0-9]"),
    new(@"\S", @"[^\t\n\f\r ]"),
    new(@"\W", @"[^0-9A-Z_a-z]"),
    new(@"[\d]", @"[0-9]"),
    new(@"[\s]", @"[\t\n\f\r ]"),
    new(@"[\w]", @"[0-9A-Z_a-z]"),
    new(@"[\D]", @"[^0-9]"),
    new(@"[\S]", @"[^\t\n\f\r ]"),
    new(@"[\W]", @"[^0-9A-Z_a-z]"),

    // Posix repetitions
    new(@"a{1}", @"a"),
    new(@"a{2}", @"aa"),
    new(@"a{5}", @"aaaaa"),
    new(@"a{0,1}", @"a?"),
    // The next three are illegible because Simplify inserts (?:)
    // parens instead of () parens to avoid creating extra
    // captured subexpressions. The comments show a version with fewer parens.
    new(@"(a){0,2}", @"(?:(a)(a)?)?"),                       //       (aa?)?
    new(@"(a){0,4}", @"(?:(a)(?:(a)(?:(a)(a)?)?)?)?"),       //   (a(a(aa?)?)?)?
    new(@"(a){2,6}", @"(a)(a)(?:(a)(?:(a)(?:(a)(a)?)?)?)?"), // aa(a(a(aa?)?)?)?
    new(@"a{0,2}", @"(?:aa?)?"),                             //       (aa?)?
    new(@"a{0,4}", @"(?:a(?:a(?:aa?)?)?)?"),                 //   (a(a(aa?)?)?)?
    new(@"a{2,6}", @"aa(?:a(?:a(?:aa?)?)?)?"),               // aa(a(a(aa?)?)?)?
    new(@"a{0,}", @"a*"),
    new(@"a{1,}", @"a+"),
    new(@"a{2,}", @"aa+"),
    new(@"a{5,}", @"aaaaa+"),

    // Test that operators simplify their arguments.
    new(@"(?:a{1,}){1,}", @"a+"),
    new(@"(a{1,}b{1,})", @"(a+b+)"),
    new(@"a{1,}|b{1,}", @"a+|b+"),
    new(@"(?:a{1,})*", @"(?:a+)*"),
    new(@"(?:a{1,})+", @"a+"),
    new(@"(?:a{1,})?", @"(?:a+)?"),
    new(@"", @"(?:)"),
    new(@"a{0}", @"(?:)"),

    // Character class simplification
    new(@"[ab]", @"[ab]"),
    new(@"[abc]", @"[a-c]"),
    new(@"[a-za-za-z]", @"[a-z]"),
    new(@"[A-Za-zA-Za-z]", @"[A-Za-z]"),
    new(@"[ABCDEFGH]", @"[A-H]"),
    new(@"[AB-CD-EF-GH]", @"[A-H]"),
    new(@"[W-ZP-XE-R]", @"[E-Z]"),
    new(@"[a-ee-gg-m]", @"[a-m]"),
    new(@"[a-ea-ha-m]", @"[a-m]"),
    new(@"[a-ma-ha-e]", @"[a-m]"),
    new(@"[a-zA-Z0-9 -~]", @"[ -~]"),

    // Empty character classes
    new(@"[^[:cntrl:][:^cntrl:]]", @"[^\x00-\x{10FFFF}]"),

    // Full character classes
    new(@"[[:cntrl:][:^cntrl:]]", @"(?s:.)"),

    // Unicode case folding.
    new(@"(?i)A", @"(?i:A)"),
    new(@"(?i)a", @"(?i:A)"),
    new(@"(?i)[A]", @"(?i:A)"),
    new(@"(?i)[a]", @"(?i:A)"),
    new(@"(?i)K", @"(?i:K)"),
    new(@"(?i)k", @"(?i:K)"),
    new(@"(?i)\x{212a}", "(?i:K)"),
    new(@"(?i)[K]", "[Kk\u212A]"),
    new(@"(?i)[k]", "[Kk\u212A]"),
    new(@"(?i)[\x{212a}]", "[Kk\u212A]"),
    new(@"(?i)[a-z]", "[A-Za-z\u017F\u212A]"),
    new(@"(?i)[\x00-\x{FFFD}]", "[\\x00-\uFFFD]"),
    new(@"(?i)[\x00-\x{10FFFF}]", @"(?s:.)"),

    // Empty string as a regular expression.
    // The empty string must be preserved inside parens in order
    // to make submatches work right, so these tests are less
    // interesting than they might otherwise be. String inserts
    // explicit (?:) in place of non-parenthesized empty strings,
    // to make them easier to spot for other parsers.
    new(@"(a|b|c|)", @"([a-c]|(?:))"),
    new(@"(a|b|)", @"([ab]|(?:))"),
    new(@"(|)", @"()"),
    new(@"a()", @"a()"),
    new(@"(()|())", @"(()|())"),
    new(@"(a|)", @"(a|(?:))"),
    new(@"ab()cd()", @"ab()cd()"),
    new(@"()", @"()"),
    new(@"()*", @"()*"),
    new(@"()+", @"()+"),
    new(@"()?", @"()?"),
    new(@"(){0}", @"(?:)"),
    new(@"(){1}", @"()"),
    new(@"(){1,}", @"()+"),
    new(@"(){0,2}", @"(?:()()?)?")
];

public static void TestSimplify(ж<testing.T> t) {
    foreach (var (_, tt) in range(simplifyTests)) {
        var (re, err) = Parse(tt.Regexp, (global::go.regexp.syntax_package.Flags)(MatchNL | (global::go.regexp.syntax_package.Flags)(Perl & ~OneLine)));
        if (err != nil) {
            t.Errorf("Parse(%#q) = error %v", tt.Regexp, err);
            continue;
        }
        @string s = re.Simplify().String();
        if (s != tt.Simple) {
            t.Errorf("Simplify(%#q) = %#q, want %#q", tt.Regexp, s, tt.Simple);
        }
    }
}

} // end syntax_internal_test_package
