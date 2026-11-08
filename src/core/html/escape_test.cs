// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;
using testing = testing_package;
using static go.html_package;

partial class html_internal_test_package {

internal partial struct unescapeTest {
    // A short description of the test case.
    internal @string desc;
    // The HTML text.
    internal @string html;
    // The unescaped text.
    internal @string unescaped;
}

internal static slice<unescapeTest> unescapeTests = [
    // Handle no entities.
    new(
        "copy",
        "A\ttext\nstring",
        "A\ttext\nstring"
    ),
    // Handle simple named entities.
    new(
        "simple",
        "&amp; &gt; &lt;",
        "& > <"
    ),
    // Handle hitting the end of the string.
    new(
        "stringEnd",
        "&amp &amp",
        "& &"
    ),
    // Handle entities with two codepoints.
    new(
        "multiCodepoint",
        "text &gesl; blah",
        "text \u22db\ufe00 blah"
    ),
    // Handle decimal numeric entities.
    new(
        "decimalEntity",
        "Delta = &#916; ",
        "Delta = Δ "
    ),
    // Handle hexadecimal numeric entities.
    new(
        "hexadecimalEntity",
        "Lambda = &#x3bb; = &#X3Bb ",
        "Lambda = λ = λ "
    ),
    // Handle numeric early termination.
    new(
        "numericEnds",
        "&# &#x &#128;43 &copy = &#169f = &#xa9",
        "&# &#x €43 © = ©f = ©"
    ),
    // Handle numeric ISO-8859-1 entity replacements.
    new(
        "numericReplacements",
        "Footnote&#x87;",
        "Footnote‡"
    ),
    // Handle single ampersand.
    new(
        "copySingleAmpersand",
        "&",
        "&"
    ),
    // Handle ampersand followed by non-entity.
    new(
        "copyAmpersandNonEntity",
        "text &test",
        "text &test"
    ),
    // Handle "&#".
    new(
        "copyAmpersandHash",
        "text &#",
        "text &#"
    )
];

public static void TestUnescape(ж<testing.T> t) {
    foreach (var (_, tt) in range(unescapeTests)) {
        @string unescaped = UnescapeString(tt.html);
        if (unescaped != tt.unescaped) {
            t.Errorf("TestUnescape %s: want %q, got %q", tt.desc, tt.unescaped, unescaped);
        }
    }
}

public static void TestUnescapeEscape(ж<testing.T> t) {
    var ss = slice<@string>([
        @"",
        @"abc def",
        @"a & b",
        @"a&amp;b",
        @"a &amp b",
        @"&quot;",
        @"""",
        @"""<&>""",
        @"&quot;&lt;&amp;&gt;&quot;",
        @"3&5==1 && 0<1, ""0&lt;1"", a+acute=&aacute;",
        @"The special characters are: <, >, &, ' and """
    ]);
    foreach (var (_, s) in range(ss)) {
        @string got = UnescapeString(EscapeString(s)); if (got != s) {
            t.Errorf("got %q want %q", got, s);
        }
    }
}

internal static @string benchEscapeData = strings.Repeat("AAAAA < BBBBB > CCCCC & DDDDD ' EEEEE \" ", 100);
internal static @string benchEscapeNone = strings.Repeat("AAAAA x BBBBB x CCCCC x DDDDD x EEEEE x ", 100);
internal static @string benchUnescapeSparse = strings.Repeat(strings.Repeat("AAAAA x BBBBB x CCCCC x DDDDD x EEEEE x ", 10) + "&amp;"u8, 10);
internal static @string benchUnescapeDense = strings.Repeat("&amp;&lt; &amp; &lt;", 100);

public static void BenchmarkEscape(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = 0;
    for (nint i = 0; i < b.N; i++) {
        n += len(EscapeString(benchEscapeData));
    }
}

public static void BenchmarkEscapeNone(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = 0;
    for (nint i = 0; i < b.N; i++) {
        n += len(EscapeString(benchEscapeNone));
    }
}

public static void BenchmarkUnescape(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = EscapeString(benchEscapeData);
    nint n = 0;
    for (nint i = 0; i < b.N; i++) {
        n += len(UnescapeString(s));
    }
}

public static void BenchmarkUnescapeNone(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = EscapeString(benchEscapeNone);
    nint n = 0;
    for (nint i = 0; i < b.N; i++) {
        n += len(UnescapeString(s));
    }
}

public static void BenchmarkUnescapeSparse(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = 0;
    for (nint i = 0; i < b.N; i++) {
        n += len(UnescapeString(benchUnescapeSparse));
    }
}

public static void BenchmarkUnescapeDense(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = 0;
    for (nint i = 0; i < b.N; i++) {
        n += len(UnescapeString(benchUnescapeDense));
    }
}

} // end html_internal_test_package
