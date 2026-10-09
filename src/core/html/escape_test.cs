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

// Handle no entities.
// Handle simple named entities.
// Handle hitting the end of the string.
// Handle entities with two codepoints.
// Handle decimal numeric entities.
// Handle hexadecimal numeric entities.
// Handle numeric early termination.
// Handle numeric ISO-8859-1 entity replacements.
// Handle single ampersand.
// Handle ampersand followed by non-entity.
// Handle "&#".
internal static slice<unescapeTest> unescapeTests = new unescapeTest[]{
    new(
        "copy"u8,
        "A\ttext\nstring"u8,
        "A\ttext\nstring"u8
    ),
    new(
        "simple"u8,
        "&amp; &gt; &lt;"u8,
        "& > <"u8
    ),
    new(
        "stringEnd"u8,
        "&amp &amp"u8,
        "& &"u8
    ),
    new(
        "multiCodepoint"u8,
        "text &gesl; blah"u8,
        "text \u22db\ufe00 blah"u8
    ),
    new(
        "decimalEntity"u8,
        "Delta = &#916; "u8,
        "Delta = Δ "u8
    ),
    new(
        "hexadecimalEntity"u8,
        "Lambda = &#x3bb; = &#X3Bb "u8,
        "Lambda = λ = λ "u8
    ),
    new(
        "numericEnds"u8,
        "&# &#x &#128;43 &copy = &#169f = &#xa9"u8,
        "&# &#x €43 © = ©f = ©"u8
    ),
    new(
        "numericReplacements"u8,
        "Footnote&#x87;"u8,
        "Footnote‡"u8
    ),
    new(
        "copySingleAmpersand"u8,
        "&"u8,
        "&"u8
    ),
    new(
        "copyAmpersandNonEntity"u8,
        "text &test"u8,
        "text &test"u8
    ),
    new(
        "copyAmpersandHash"u8,
        "text &#"u8,
        "text &#"u8
    )
}.slice();

public static void TestUnescape(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in unescapeTests) {
        @string unescaped = UnescapeString(tt.html);
        if (unescaped != tt.unescaped) {
            Ꮡt.Errorf("TestUnescape %s: want %q, got %q"u8, tt.desc, tt.unescaped, unescaped);
        }
    }
}

public static void TestUnescapeEscape(ж<testing.T> Ꮡt) {
    var ss = new @string[]{
        @""u8,
        @"abc def"u8,
        @"a & b"u8,
        @"a&amp;b"u8,
        @"a &amp b"u8,
        @"&quot;"u8,
        @""""u8,
        @"""<&>"""u8,
        @"&quot;&lt;&amp;&gt;&quot;"u8,
        @"3&5==1 && 0<1, ""0&lt;1"", a+acute=&aacute;"u8,
        @"The special characters are: <, >, &, ' and """u8
    }.slice();
    foreach (var (_, s) in ss) {
        {
            @string got = UnescapeString(EscapeString(s)); if (got != s) {
                Ꮡt.Errorf("got %q want %q"u8, got, s);
            }
        }
    }
}

internal static @string benchEscapeData = strings.Repeat("AAAAA < BBBBB > CCCCC & DDDDD ' EEEEE \" "u8, 100);
internal static @string benchEscapeNone = strings.Repeat("AAAAA x BBBBB x CCCCC x DDDDD x EEEEE x "u8, 100);
internal static @string benchUnescapeSparse = strings.Repeat(strings.Repeat("AAAAA x BBBBB x CCCCC x DDDDD x EEEEE x "u8, 10) + "&amp;"u8, 10);
internal static @string benchUnescapeDense = strings.Repeat("&amp;&lt; &amp; &lt;"u8, 100);

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
