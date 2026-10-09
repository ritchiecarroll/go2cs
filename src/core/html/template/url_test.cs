// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using testing = testing_package;
using static go.html.template_package;

partial class template_internal_test_package {

internal partial struct TestURLNormalizer_tests /*dyn*/ {
    internal @string url, want;
}

public static void TestURLNormalizer(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestURLNormalizer_tests[]{
        new(""u8, ""u8),
        new(
            "http://example.com:80/foo/bar?q=foo%20&bar=x+y#frag"u8,
            "http://example.com:80/foo/bar?q=foo%20&bar=x+y#frag"u8
        ),
        new(" "u8, "%20"u8),
        new("%7c"u8, "%7c"u8),
        new("%7C"u8, "%7C"u8),
        new("%2"u8, "%252"u8),
        new("%"u8, "%25"u8),
        new("%z"u8, "%25z"u8),
        new("/foo|bar/%5c\u1234"u8, "/foo%7cbar/%5c%e1%88%b4"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string got = urlNormalizer(test.url); if (test.want != got) {
                Ꮡt.Errorf("%q: want\n\t%q\nbut got\n\t%q"u8, test.url, test.want, got);
            }
        }
        if (test.want != urlNormalizer(test.want)) {
            Ꮡt.Errorf("not idempotent: %q"u8, test.want);
        }
    }
}

internal partial struct TestURLFilters_tests /*dyn*/ {
    internal @string name;
    internal Funcꓸꓸꓸ<any, @string> escaper;
    internal @string escaped;
}

public static void TestURLFilters(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string input = ("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !""#$%&'()*+,-./"u8 + @"0123456789:;<=>?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + "`abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\x7f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);
    var tests = new TestURLFilters_tests[]{
        new(
            "urlEscaper"u8,
            urlEscaper,
            "%00%01%02%03%04%05%06%07%08%09%0a%0b%0c%0d%0e%0f"u8 + "%10%11%12%13%14%15%16%17%18%19%1a%1b%1c%1d%1e%1f"u8 + "%20%21%22%23%24%25%26%27%28%29%2a%2b%2c-.%2f"u8 + "0123456789%3a%3b%3c%3d%3e%3f"u8 + "%40ABCDEFGHIJKLMNO"u8 + "PQRSTUVWXYZ%5b%5c%5d%5e_"u8 + "%60abcdefghijklmno"u8 + "pqrstuvwxyz%7b%7c%7d~%7f"u8 + "%c2%a0%c4%80%e2%80%a8%e2%80%a9%ef%bb%bf%f0%9d%84%9e"u8
        ),
        new(
            "urlNormalizer"u8,
            urlNormalizer,
            "%00%01%02%03%04%05%06%07%08%09%0a%0b%0c%0d%0e%0f"u8 + "%10%11%12%13%14%15%16%17%18%19%1a%1b%1c%1d%1e%1f"u8 + "%20!%22#$%25&%27%28%29*+,-./"u8 + "0123456789:;%3c=%3e?"u8 + "@ABCDEFGHIJKLMNO"u8 + "PQRSTUVWXYZ[%5c]%5e_"u8 + "%60abcdefghijklmno"u8 + "pqrstuvwxyz%7b%7c%7d~%7f"u8 + "%c2%a0%c4%80%e2%80%a8%e2%80%a9%ef%bb%bf%f0%9d%84%9e"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string s = test.escaper(input); if (s != test.escaped) {
                Ꮡt.Errorf("%s: want\n\t%q\ngot\n\t%q"u8, test.name, test.escaped, s);
                continue;
            }
        }
    }
}

internal partial struct TestSrcsetFilter_tests /*dyn*/ {
    internal @string name;
    internal @string input;
    internal @string want;
}

public static void TestSrcsetFilter(ж<testing.T> Ꮡt) {
    var tests = new TestSrcsetFilter_tests[]{
        new(
            "one ok"u8,
            "http://example.com/img.png"u8,
            "http://example.com/img.png"u8
        ),
        new(
            "one ok with metadata"u8,
            " /img.png 200w"u8,
            " /img.png 200w"u8
        ),
        new(
            "one bad"u8,
            "javascript:alert(1) 200w"u8,
            "#ZgotmplZ"u8
        ),
        new(
            "two ok"u8,
            "foo.png, bar.png"u8,
            "foo.png, bar.png"u8
        ),
        new(
            "left bad"u8,
            "javascript:alert(1), /foo.png"u8,
            "#ZgotmplZ, /foo.png"u8
        ),
        new(
            "right bad"u8,
            "/bogus#, javascript:alert(1)"u8,
            "/bogus#,#ZgotmplZ"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string got = srcsetFilterAndEscaper(test.input); if (got != test.want) {
                Ꮡt.Errorf("%s: srcsetFilterAndEscaper(%q) want %q != %q"u8, test.name, test.input, test.want, got);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpExampleCom80FooQBarˢ = (@string)"http://example.com:80/foo?q=bar%20&baz=x+y#frag"u8;

public static void BenchmarkURLEscaper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        urlEscaper(httpExampleCom80FooQBarˢ);
    }
}

public static void BenchmarkURLEscaperNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        urlEscaper((@string)"TheQuickBrownFoxJumpsOverTheLazyDog."u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object theQuickBrownFoxJumpsˢ4 = (@string)"The quick brown fox jumps over the lazy dog.\n"u8;

public static void BenchmarkURLNormalizer(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        urlNormalizer(theQuickBrownFoxJumpsˢ4);
    }
}

public static void BenchmarkURLNormalizerNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        urlNormalizer(httpExampleCom80FooQBarˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fooBarPng200wBazBoo1Pngˢ = (@string)" /foo/bar.png 200w, /baz/boo(1).png"u8;

public static void BenchmarkSrcsetFilter(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        srcsetFilterAndEscaper(fooBarPng200wBazBoo1Pngˢ);
    }
}

public static void BenchmarkSrcsetFilterNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        srcsetFilterAndEscaper(httpExampleCom80FooQBarˢ);
    }
}

} // end template_internal_test_package
