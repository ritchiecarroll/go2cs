// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using static go.regexp_package;
using Δio = io_package;
using ꓸꓸꓸnint = Span<nint>;

partial class regexp_internal_test_package {

// For each pattern/text pair, what is the expected output of each function?
// We can derive the textual results from the indexed results, the non-submatch
// results from the submatched results, the single results from the 'all' results,
// and the byte results from the string results. Therefore the table includes
// only the FindAllStringSubmatchIndex result.
public partial struct FindTest {
    internal @string pat;
    internal @string text;
    internal slice<slice<nint>> matches;
}

public static @string String(this FindTest t) {
    return fmt.Sprintf("pat: %#q text: %#q"u8, t.pat, t.text);
}

internal static slice<FindTest> findTests = slice<FindTest>([
    new(@"", @"", build(1, 0, 0)),
    new(@"^abcdefg", "abcdefg", build(1, 0, 7)),
    new(@"a+", "baaab", build(1, 1, 4)),
    new("abcd..", "abcdef", build(1, 0, 6)),
    new(@"a", "a", build(1, 0, 1)),
    new(@"x", "y", default),
    new(@"b", "abc", build(1, 1, 2)),
    new(@".", "a", build(1, 0, 1)),
    new(@".*", "abcdef", build(1, 0, 6)),
    new(@"^", "abcde", build(1, 0, 0)),
    new(@"$", "abcde", build(1, 5, 5)),
    new(@"^abcd$", "abcd", build(1, 0, 4)),
    new(@"^bcd'", "abcdef", default),
    new(@"^abcd$", "abcde", default),
    new(@"a+", "baaab", build(1, 1, 4)),
    new(@"a*", "baaab", build(3, 0, 0, 1, 4, 5, 5)),
    new(@"[a-z]+", "abcd", build(1, 0, 4)),
    new(@"[^a-z]+", "ab1234cd", build(1, 2, 6)),
    new(@"[a\-\]z]+", "az]-bcz", build(2, 0, 4, 6, 7)),
    new(@"[^\n]+", "abcd\n", build(1, 0, 4)),
    new(@"[日本語]+", "日本語日本語", build(1, 0, 18)),
    new(@"日本語+", "日本語", build(1, 0, 9)),
    new(@"日本語+", "日本語語語語", build(1, 0, 18)),
    new(@"()", "", build(1, 0, 0, 0, 0)),
    new(@"(a)", "a", build(1, 0, 1, 0, 1)),
    new(@"(.)(.)", "日a", build(1, 0, 4, 0, 3, 3, 4)),
    new(@"(.*)", "", build(1, 0, 0, 0, 0)),
    new(@"(.*)", "abcd", build(1, 0, 4, 0, 4)),
    new(@"(..)(..)", "abcd", build(1, 0, 4, 0, 2, 2, 4)),
    new(@"(([^xyz]*)(d))", "abcd", build(1, 0, 4, 0, 4, 0, 3, 3, 4)),
    new(@"((a|b|c)*(d))", "abcd", build(1, 0, 4, 0, 4, 2, 3, 3, 4)),
    new(@"(((a|b|c)*)(d))", "abcd", build(1, 0, 4, 0, 4, 0, 3, 2, 3, 3, 4)),
    new(@"\a\f\n\r\t\v", "\a\f\n\r\t\v", build(1, 0, 6)),
    new(@"[\a\f\n\r\t\v]+", "\a\f\n\r\t\v", build(1, 0, 6)),

    new(@"a*(|(b))c*", "aacc", build(1, 0, 4, 2, 2, -1, -1)),
    new(@"(.*).*", "ab", build(1, 0, 2, 0, 2)),
    new(@"[.]", ".", build(1, 0, 1)),
    new(@"/$", "/abc/", build(1, 4, 5)),
    new(@"/$", "/abc", default),

    // multiple matches
    new(@".", "abc", build(3, 0, 1, 1, 2, 2, 3)),
    new(@"(.)", "abc", build(3, 0, 1, 0, 1, 1, 2, 1, 2, 2, 3, 2, 3)),
    new(@".(.)", "abcd", build(2, 0, 2, 1, 2, 2, 4, 3, 4)),
    new(@"ab*", "abbaab", build(3, 0, 3, 3, 4, 4, 6)),
    new(@"a(b*)", "abbaab", build(3, 0, 3, 1, 3, 3, 4, 4, 4, 4, 6, 5, 6)),

    // fixed bugs
    new(@"ab$", "cab", build(1, 1, 3)),
    new(@"axxb$", "axxcb", default),
    new(@"data", "daXY data", build(1, 5, 9)),
    new(@"da(.)a$", "daXY data", build(1, 5, 9, 7, 8)),
    new(@"zx+", "zzx", build(1, 1, 3)),
    new(@"ab$", "abcab", build(1, 3, 5)),
    new(@"(aa)*$", "a", build(1, 1, 1, -1, -1)),
    new(@"(?:.|(?:.a))", "", default),
    new(@"(?:A(?:A|a))", "Aa", build(1, 0, 2)),
    new(@"(?:A|(?:A|a))", "a", build(1, 0, 1)),
    new(@"(a){0}", "", build(1, 0, 0, -1, -1)),
    new(@"(?-s)(?:(?:^).)", "\n", default),
    new(@"(?s)(?:(?:^).)", "\n", build(1, 0, 1)),
    new(@"(?:(?:^).)", "\n", default),
    new(@"\b", "x", build(2, 0, 0, 1, 1)),
    new(@"\b", "xx", build(2, 0, 0, 2, 2)),
    new(@"\b", "x y", build(4, 0, 0, 1, 1, 2, 2, 3, 3)),
    new(@"\b", "xx yy", build(4, 0, 0, 2, 2, 3, 3, 5, 5)),
    new(@"\B", "x", default),
    new(@"\B", "xx", build(1, 1, 1)),
    new(@"\B", "x y", default),
    new(@"\B", "xx yy", build(2, 1, 1, 4, 4)),
    new(@"(|a)*", "aa", build(3, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2)),
    new(@"0A|0[aA]", "0a", build(1, 0, 2)),
    new(@"0[aA]|0A", "0a", build(1, 0, 2)),

    // RE2 tests
    new(@"[^\S\s]", "abcd", default),
    new(@"[^\S[:space:]]", "abcd", default),
    new(@"[^\D\d]", "abcd", default),
    new(@"[^\D[:digit:]]", "abcd", default),
    new(@"(?i)\W", "x", default),
    new(@"(?i)\W", "k", default),
    new(@"(?i)\W", "s", default),

    // can backslash-escape any punctuation
    new(@"\!\""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\_\{\|\}\~",
        @"!""#$%&'()*+,-./:;<=>?@[\]^_{|}~", build(1, 0, 31)),
    new(@"[\!\""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\_\{\|\}\~]+",
        @"!""#$%&'()*+,-./:;<=>?@[\]^_{|}~", build(1, 0, 31)),
    new("\\`", "`", build(1, 0, 1)),
    new("[\\`]+", "`", build(1, 0, 1)),

    new("\ufffd", ((@string)(new byte[]{0xff})), build(1, 0, 1)),
    new("\ufffd", ((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64})), build(1, 5, 6)),
    new(@".*", ((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64})), build(1, 0, 11)),
    new(@"\x{fffd}", ((@string)(new byte[]{0xc2, 0x00})), build(1, 0, 1)),
    new("[\ufffd]", ((@string)(new byte[]{0xff})), build(1, 0, 1)),
    new(@"[\x{fffd}]", ((@string)(new byte[]{0xc2, 0x00})), build(1, 0, 1)),

    // long set of matches (longer than startSize)
    new(
        ".",
        "qwertyuiopasdfghjklzxcvbnm1234567890",
        build(36, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10,
            10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20,
            20, 21, 21, 22, 22, 23, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 30,
            30, 31, 31, 32, 32, 33, 33, 34, 34, 35, 35, 36)
    )
]);

// build is a helper to construct a [][]int by extracting n sequences from x.
// This represents n matches with len(x)/n submatches each.
internal static slice<slice<nint>> build(nint n, params ꓸꓸꓸnint xʗp) {
    var x = xʗp.slice();

    var ret = new slice<slice<nint>>(n);
    nint runLength = len(x) / n;
    nint j = 0;
    foreach (var i in range(ret)) {
        ret[i] = new slice<nint>(runLength);
        copy(ret[i], x.slice(j));
        j += runLength;
        if (j > len(x)) {
            throw panic("invalid build entry");
        }
    }
    return ret;
}

// First the simple cases.
public static void TestFind(ж<testing.T> t) {
    foreach (var (_, test) in findTests) {
        var re = MustCompile(test.pat);
        if (re.String() != test.pat) {
            t.Errorf("re.String() = %q, want %q", re.String(), test.pat);
        }
        var result = re.Find(slice<byte>(test.text));
        switch (ᐧ) {
        case {} when len(test.matches) == 0 && len(result) == 0: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Errorf("got no match, want one: %s", test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            @string want = test.text.slice(test.matches[0][0], test.matches[0][1]);
            if (len(result) != cap(result)) {
                t.Errorf("got capacity %d, want %d: %s", cap(result), len(result), test);
            }
            if (want != ((sstring)result)) {
                t.Errorf("got %q, want %q: %s", result, want, test);
            }
            break;
        }}
    }
}

public static void TestFindString(ж<testing.T> t) {
    foreach (var (_, test) in findTests) {
        @string result = MustCompile(test.pat).FindString(test.text);
        switch (ᐧ) {
        case {} when len(test.matches) == 0 && len(result) == 0: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != ""u8: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == ""u8: {
            // Tricky because an empty result has two meanings: no match or empty match.
            if (test.matches[0][0] != test.matches[0][1]) {
                t.Errorf("got no match, want one: %s", test);
            }
            break;
        }
        case {} when test.matches != nil && result != ""u8: {
            @string want = test.text.slice(test.matches[0][0], test.matches[0][1]);
            if (want != result) {
                t.Errorf("got %q, want %q: %s", result, want, test);
            }
            break;
        }}
    }
}

internal static void testFindIndex(ж<FindTest> Ꮡtest, slice<nint> result, ж<testing.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when len(test.matches) == 0 && len(result) == 0: {
        // ok
        break;
    }
    case {} when test.matches == nil && result != nil: {
        t.Errorf("got match %v, want none: %s", result, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result == nil: {
        t.Errorf("got no match, want one: %s", Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result != nil: {
        var want = test.matches[0];
        if (want[0] != result[0] || want[1] != result[1]) {
            t.Errorf("got %v, want %v: %s", result, want, Ꮡtest.OrTypedNil());
        }
        break;
    }}
}

public static void TestFindIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindIndex(Ꮡtest, MustCompile(test.pat).FindIndex(slice<byte>(test.text)), t);
    }
}

public static void TestFindStringIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindIndex(Ꮡtest, MustCompile(test.pat).FindStringIndex(test.text), t);
    }
}

public static void TestFindReaderIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindIndex(Ꮡtest, MustCompile(test.pat).FindReaderIndex(new regexp_test_package.strings_ReaderжRuneReader(strings.NewReader(test.text))), t);
    }
}

// Now come the simple All cases.
public static void TestFindAll(ж<testing.T> t) {
    foreach (var (_, test) in findTests) {
        var result = MustCompile(test.pat).FindAll(slice<byte>(test.text), -1);
        switch (ᐧ) {
        case {} when test.matches == nil && result == nil: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Fatalf("got no match, want one: %s", test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            if (len(test.matches) != len(result)) {
                t.Errorf("got %d matches, want %d: %s", len(result), len(test.matches), test);
                continue;
            }
            foreach (var (k, e) in test.matches) {
                var got = result[k];
                if (len(got) != cap(got)) {
                    t.Errorf("match %d: got capacity %d, want %d: %s", k, cap(got), len(got), test);
                }
                @string want = test.text.slice(e[0], e[1]);
                if (want != ((sstring)got)) {
                    t.Errorf("match %d: got %q, want %q: %s", k, got, want, test);
                }
            }
            break;
        }}
    }
}

public static void TestFindAllString(ж<testing.T> t) {
    foreach (var (_, test) in findTests) {
        var result = MustCompile(test.pat).FindAllString(test.text, -1);
        switch (ᐧ) {
        case {} when test.matches == nil && result == nil: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Errorf("got no match, want one: %s", test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            if (len(test.matches) != len(result)) {
                t.Errorf("got %d matches, want %d: %s", len(result), len(test.matches), test);
                continue;
            }
            foreach (var (k, e) in test.matches) {
                @string want = test.text.slice(e[0], e[1]);
                if (want != result[k]) {
                    t.Errorf("got %q, want %q: %s", result[k], want, test);
                }
            }
            break;
        }}
    }
}

internal static void testFindAllIndex(ж<FindTest> Ꮡtest, slice<slice<nint>> result, ж<testing.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when test.matches == nil && result == nil: {
        // ok
        break;
    }
    case {} when test.matches == nil && result != nil: {
        t.Errorf("got match %v, want none: %s", result, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result == nil: {
        t.Errorf("got no match, want one: %s", Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result != nil: {
        if (len(test.matches) != len(result)) {
            t.Errorf("got %d matches, want %d: %s", len(result), len(test.matches), Ꮡtest.OrTypedNil());
            return;
        }
        foreach (var (k, e) in test.matches) {
            if (e[0] != result[k][0] || e[1] != result[k][1]) {
                t.Errorf("match %d: got %v, want %v: %s", k, result[k], e, Ꮡtest.OrTypedNil());
            }
        }
        break;
    }}
}

public static void TestFindAllIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllIndex(Ꮡtest, MustCompile(test.pat).FindAllIndex(slice<byte>(test.text), -1), t);
    }
}

public static void TestFindAllStringIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllIndex(Ꮡtest, MustCompile(test.pat).FindAllStringIndex(test.text, -1), t);
    }
}

// Now come the Submatch cases.
internal static void testSubmatchBytes(ж<FindTest> Ꮡtest, nint n, slice<nint> submatches, slice<slice<byte>> result, ж<testing.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    if (len(submatches) != len(result) * 2) {
        t.Errorf("match %d: got %d submatches, want %d: %s", n, len(result), len(submatches) / 2, Ꮡtest.OrTypedNil());
        return;
    }
    for (nint k = 0; k < len(submatches); k += 2) {
        if (submatches[k] == -1) {
            if (result[k / 2] != nil) {
                t.Errorf("match %d: got %q, want nil: %s", n, result, Ꮡtest.OrTypedNil());
            }
            continue;
        }
        var got = result[k / 2];
        if (len(got) != cap(got)) {
            t.Errorf("match %d: got capacity %d, want %d: %s", n, cap(got), len(got), Ꮡtest.OrTypedNil());
            return;
        }
        @string want = test.text.slice(submatches[k], submatches[k + 1]);
        if (want != ((sstring)got)) {
            t.Errorf("match %d: got %q, want %q: %s", n, got, want, Ꮡtest.OrTypedNil());
            return;
        }
    }
}

public static void TestFindSubmatch(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindSubmatch(slice<byte>(test.text));
        switch (ᐧ) {
        case {} when test.matches == nil && result == nil: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Errorf("got no match, want one: %s", test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            testSubmatchBytes(Ꮡtest, 0, test.matches[0], result, t);
            break;
        }}
    }
}

internal static void testSubmatchString(ж<FindTest> Ꮡtest, nint n, slice<nint> submatches, slice<@string> result, ж<testing.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    if (len(submatches) != len(result) * 2) {
        t.Errorf("match %d: got %d submatches, want %d: %s", n, len(result), len(submatches) / 2, Ꮡtest.OrTypedNil());
        return;
    }
    for (nint k = 0; k < len(submatches); k += 2) {
        if (submatches[k] == -1) {
            if (result[k / 2] != "") {
                t.Errorf("match %d: got %q, want empty string: %s", n, result, Ꮡtest.OrTypedNil());
            }
            continue;
        }
        @string want = test.text.slice(submatches[k], submatches[k + 1]);
        if (want != result[k / 2]) {
            t.Errorf("match %d: got %q, want %q: %s", n, result[k / 2], want, Ꮡtest.OrTypedNil());
            return;
        }
    }
}

public static void TestFindStringSubmatch(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindStringSubmatch(test.text);
        switch (ᐧ) {
        case {} when test.matches == nil && result == nil: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Errorf("got no match, want one: %s", test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            testSubmatchString(Ꮡtest, 0, test.matches[0], result, t);
            break;
        }}
    }
}

internal static void testSubmatchIndices(ж<FindTest> test, nint n, slice<nint> want, slice<nint> result, ж<testing.T> t) {
    if (len(want) != len(result)) {
        t.Errorf("match %d: got %d matches, want %d: %s", n, len(result) / 2, len(want) / 2, test.OrTypedNil());
        return;
    }
    foreach (var (k, e) in want) {
        if (e != result[k]) {
            t.Errorf("match %d: submatch error: got %v, want %v: %s", n, result, want, test.OrTypedNil());
        }
    }
}

internal static void testFindSubmatchIndex(ж<FindTest> Ꮡtest, slice<nint> result, ж<testing.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when test.matches == nil && result == nil: {
        // ok
        break;
    }
    case {} when test.matches == nil && result != nil: {
        t.Errorf("got match %v, want none: %s", result, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result == nil: {
        t.Errorf("got no match, want one: %s", Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result != nil: {
        testSubmatchIndices(Ꮡtest, 0, test.matches[0], result, t);
        break;
    }}
}

public static void TestFindSubmatchIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindSubmatchIndex(slice<byte>(test.text)), t);
    }
}

public static void TestFindStringSubmatchIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindStringSubmatchIndex(test.text), t);
    }
}

public static void TestFindReaderSubmatchIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindReaderSubmatchIndex(new regexp_test_package.strings_ReaderжRuneReader(strings.NewReader(test.text))), t);
    }
}

// Now come the monster AllSubmatch cases.
public static void TestFindAllSubmatch(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindAllSubmatch(slice<byte>(test.text), -1);
        switch (ᐧ) {
        case {} when test.matches == nil && result == nil: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Errorf("got no match, want one: %s", test);
            break;
        }
        case {} when len(test.matches) != len(result): {
            t.Errorf("got %d matches, want %d: %s", len(result), len(test.matches), test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            foreach (var (k, Δmatch) in test.matches) {
                testSubmatchBytes(Ꮡtest, k, Δmatch, result[k], t);
            }
            break;
        }}
    }
}

public static void TestFindAllStringSubmatch(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindAllStringSubmatch(test.text, -1);
        switch (ᐧ) {
        case {} when test.matches == nil && result == nil: {
            // ok
            break;
        }
        case {} when test.matches == nil && result != nil: {
            t.Errorf("got match %q, want none: %s", result, test);
            break;
        }
        case {} when test.matches != nil && result == nil: {
            t.Errorf("got no match, want one: %s", test);
            break;
        }
        case {} when len(test.matches) != len(result): {
            t.Errorf("got %d matches, want %d: %s", len(result), len(test.matches), test);
            break;
        }
        case {} when test.matches != nil && result != nil: {
            foreach (var (k, Δmatch) in test.matches) {
                testSubmatchString(Ꮡtest, k, Δmatch, result[k], t);
            }
            break;
        }}
    }
}

internal static void testFindAllSubmatchIndex(ж<FindTest> Ꮡtest, slice<slice<nint>> result, ж<testing.T> t) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when test.matches == nil && result == nil: {
        // ok
        break;
    }
    case {} when test.matches == nil && result != nil: {
        t.Errorf("got match %v, want none: %s", result, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result == nil: {
        t.Errorf("got no match, want one: %s", Ꮡtest.OrTypedNil());
        break;
    }
    case {} when len(test.matches) != len(result): {
        t.Errorf("got %d matches, want %d: %s", len(result), len(test.matches), Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != nil && result != nil: {
        foreach (var (k, Δmatch) in test.matches) {
            testSubmatchIndices(Ꮡtest, k, Δmatch, result[k], t);
        }
        break;
    }}
}

public static void TestFindAllSubmatchIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindAllSubmatchIndex(slice<byte>(test.text), -1), t);
    }
}

public static void TestFindAllStringSubmatchIndex(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindAllStringSubmatchIndex(test.text, -1), t);
    }
}

} // end regexp_internal_test_package
