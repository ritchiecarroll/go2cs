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
[GoType] public partial struct FindTest {
    internal @string pat;
    internal @string text;
    internal slice<slice<nint>> matches;
}

public static @string String(this FindTest t) {
    return fmt.Sprintf("pat: %#q text: %#q"u8, t.pat, t.text);
}

// multiple matches
// fixed bugs
// RE2 tests
// can backslash-escape any punctuation
// long set of matches (longer than startSize)
internal static slice<FindTest> findTests = new FindTest[]{
    new(@""u8, @""u8, build(1, 0, 0)),
    new(@"^abcdefg"u8, "abcdefg"u8, build(1, 0, 7)),
    new(@"a+"u8, "baaab"u8, build(1, 1, 4)),
    new("abcd.."u8, "abcdef"u8, build(1, 0, 6)),
    new(@"a"u8, "a"u8, build(1, 0, 1)),
    new(@"x"u8, "y"u8, default!),
    new(@"b"u8, "abc"u8, build(1, 1, 2)),
    new(@"."u8, "a"u8, build(1, 0, 1)),
    new(@".*"u8, "abcdef"u8, build(1, 0, 6)),
    new(@"^"u8, "abcde"u8, build(1, 0, 0)),
    new(@"$"u8, "abcde"u8, build(1, 5, 5)),
    new(@"^abcd$"u8, "abcd"u8, build(1, 0, 4)),
    new(@"^bcd'"u8, "abcdef"u8, default!),
    new(@"^abcd$"u8, "abcde"u8, default!),
    new(@"a+"u8, "baaab"u8, build(1, 1, 4)),
    new(@"a*"u8, "baaab"u8, build(3, 0, 0, 1, 4, 5, 5)),
    new(@"[a-z]+"u8, "abcd"u8, build(1, 0, 4)),
    new(@"[^a-z]+"u8, "ab1234cd"u8, build(1, 2, 6)),
    new(@"[a\-\]z]+"u8, "az]-bcz"u8, build(2, 0, 4, 6, 7)),
    new(@"[^\n]+"u8, "abcd\n"u8, build(1, 0, 4)),
    new(@"[日本語]+"u8, "日本語日本語"u8, build(1, 0, 18)),
    new(@"日本語+"u8, "日本語"u8, build(1, 0, 9)),
    new(@"日本語+"u8, "日本語語語語"u8, build(1, 0, 18)),
    new(@"()"u8, ""u8, build(1, 0, 0, 0, 0)),
    new(@"(a)"u8, "a"u8, build(1, 0, 1, 0, 1)),
    new(@"(.)(.)"u8, "日a"u8, build(1, 0, 4, 0, 3, 3, 4)),
    new(@"(.*)"u8, ""u8, build(1, 0, 0, 0, 0)),
    new(@"(.*)"u8, "abcd"u8, build(1, 0, 4, 0, 4)),
    new(@"(..)(..)"u8, "abcd"u8, build(1, 0, 4, 0, 2, 2, 4)),
    new(@"(([^xyz]*)(d))"u8, "abcd"u8, build(1, 0, 4, 0, 4, 0, 3, 3, 4)),
    new(@"((a|b|c)*(d))"u8, "abcd"u8, build(1, 0, 4, 0, 4, 2, 3, 3, 4)),
    new(@"(((a|b|c)*)(d))"u8, "abcd"u8, build(1, 0, 4, 0, 4, 0, 3, 2, 3, 3, 4)),
    new(@"\a\f\n\r\t\v"u8, "\a\f\n\r\t\v"u8, build(1, 0, 6)),
    new(@"[\a\f\n\r\t\v]+"u8, "\a\f\n\r\t\v"u8, build(1, 0, 6)),
    new(@"a*(|(b))c*"u8, "aacc"u8, build(1, 0, 4, 2, 2, -1, -1)),
    new(@"(.*).*"u8, "ab"u8, build(1, 0, 2, 0, 2)),
    new(@"[.]"u8, "."u8, build(1, 0, 1)),
    new(@"/$"u8, "/abc/"u8, build(1, 4, 5)),
    new(@"/$"u8, "/abc"u8, default!),
    new(@"."u8, "abc"u8, build(3, 0, 1, 1, 2, 2, 3)),
    new(@"(.)"u8, "abc"u8, build(3, 0, 1, 0, 1, 1, 2, 1, 2, 2, 3, 2, 3)),
    new(@".(.)"u8, "abcd"u8, build(2, 0, 2, 1, 2, 2, 4, 3, 4)),
    new(@"ab*"u8, "abbaab"u8, build(3, 0, 3, 3, 4, 4, 6)),
    new(@"a(b*)"u8, "abbaab"u8, build(3, 0, 3, 1, 3, 3, 4, 4, 4, 4, 6, 5, 6)),
    new(@"ab$"u8, "cab"u8, build(1, 1, 3)),
    new(@"axxb$"u8, "axxcb"u8, default!),
    new(@"data"u8, "daXY data"u8, build(1, 5, 9)),
    new(@"da(.)a$"u8, "daXY data"u8, build(1, 5, 9, 7, 8)),
    new(@"zx+"u8, "zzx"u8, build(1, 1, 3)),
    new(@"ab$"u8, "abcab"u8, build(1, 3, 5)),
    new(@"(aa)*$"u8, "a"u8, build(1, 1, 1, -1, -1)),
    new(@"(?:.|(?:.a))"u8, ""u8, default!),
    new(@"(?:A(?:A|a))"u8, "Aa"u8, build(1, 0, 2)),
    new(@"(?:A|(?:A|a))"u8, "a"u8, build(1, 0, 1)),
    new(@"(a){0}"u8, ""u8, build(1, 0, 0, -1, -1)),
    new(@"(?-s)(?:(?:^).)"u8, "\n"u8, default!),
    new(@"(?s)(?:(?:^).)"u8, "\n"u8, build(1, 0, 1)),
    new(@"(?:(?:^).)"u8, "\n"u8, default!),
    new(@"\b"u8, "x"u8, build(2, 0, 0, 1, 1)),
    new(@"\b"u8, "xx"u8, build(2, 0, 0, 2, 2)),
    new(@"\b"u8, "x y"u8, build(4, 0, 0, 1, 1, 2, 2, 3, 3)),
    new(@"\b"u8, "xx yy"u8, build(4, 0, 0, 2, 2, 3, 3, 5, 5)),
    new(@"\B"u8, "x"u8, default!),
    new(@"\B"u8, "xx"u8, build(1, 1, 1)),
    new(@"\B"u8, "x y"u8, default!),
    new(@"\B"u8, "xx yy"u8, build(2, 1, 1, 4, 4)),
    new(@"(|a)*"u8, "aa"u8, build(3, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2)),
    new(@"[^\S\s]"u8, "abcd"u8, default!),
    new(@"[^\S[:space:]]"u8, "abcd"u8, default!),
    new(@"[^\D\d]"u8, "abcd"u8, default!),
    new(@"[^\D[:digit:]]"u8, "abcd"u8, default!),
    new(@"(?i)\W"u8, "x"u8, default!),
    new(@"(?i)\W"u8, "k"u8, default!),
    new(@"(?i)\W"u8, "s"u8, default!),
    new(@"\!\""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\_\{\|\}\~"u8,
        @"!""#$%&'()*+,-./:;<=>?@[\]^_{|}~"u8, build(1, 0, 31)),
    new(@"[\!\""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\_\{\|\}\~]+"u8,
        @"!""#$%&'()*+,-./:;<=>?@[\]^_{|}~"u8, build(1, 0, 31)),
    new("\\`"u8, "`"u8, build(1, 0, 1)),
    new("[\\`]+"u8, "`"u8, build(1, 0, 1)),
    new("\ufffd"u8, ((@string)(new byte[]{0xff})), build(1, 0, 1)),
    new("\ufffd"u8, ((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64})), build(1, 5, 6)),
    new(@".*"u8, ((@string)(new byte[]{0x68, 0x65, 0x6c, 0x6c, 0x6f, 0xff, 0x77, 0x6f, 0x72, 0x6c, 0x64})), build(1, 0, 11)),
    new(@"\x{fffd}"u8, ((@string)(new byte[]{0xc2, 0x00})), build(1, 0, 1)),
    new("[\ufffd]"u8, ((@string)(new byte[]{0xff})), build(1, 0, 1)),
    new(@"[\x{fffd}]"u8, ((@string)(new byte[]{0xc2, 0x00})), build(1, 0, 1)),
    new(
        "."u8,
        "qwertyuiopasdfghjklzxcvbnm1234567890"u8,
        build(36, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10,
            10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20,
            20, 21, 21, 22, 22, 23, 23, 24, 24, 25, 25, 26, 26, 27, 27, 28, 28, 29, 29, 30,
            30, 31, 31, 32, 32, 33, 33, 34, 34, 35, 35, 36)
    )
}.slice();

// build is a helper to construct a [][]int by extracting n sequences from x.
// This represents n matches with len(x)/n submatches each.
internal static slice<slice<nint>> build(nint n, params ꓸꓸꓸnint xʗp) {
    var x = xʗp.slice();

    var ret = new slice<slice<nint>>(n);
    nint runLength = len(x) / n;
    nint j = 0;
    foreach (var (i, _) in ret) {
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
public static void TestFind(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in findTests) {
        var re = MustCompile(test.pat);
        if (re.String() != test.pat) {
            Ꮡt.Errorf("String() = `%s`; should be `%s`"u8, re.String(), test.pat);
        }
        var result = re.Find(slice<byte>(test.text));
        switch (ᐧ) {
        case {} when len(test.matches) == 0 && len(result) == 0: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Errorf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            @string expect = test.text.slice(test.matches[0][0], test.matches[0][1]);
            if (len(result) != cap(result)) {
                Ꮡt.Errorf("expected capacity %d got %d: %s"u8, len(result), cap(result), test);
            }
            if (expect != ((sstring)result)) {
                Ꮡt.Errorf("expected %q got %q: %s"u8, expect, result, test);
            }
            break;
        }}

    }
}

public static void TestFindString(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in findTests) {
        @string result = MustCompile(test.pat).FindString(test.text);
        switch (ᐧ) {
        case {} when len(test.matches) == 0 && len(result) == 0: {
            break;
        }
        case {} when test.matches == default! && result != ""u8: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == ""u8: {
            if (test.matches[0][0] != test.matches[0][1]) {
                // Tricky because an empty result has two meanings: no match or empty match.
                Ꮡt.Errorf("expected match; got none: %s"u8, test);
            }
            break;
        }
        case {} when test.matches != default! && result != ""u8: {
            @string expect = test.text.slice(test.matches[0][0], test.matches[0][1]);
            if (expect != result) {
                Ꮡt.Errorf("expected %q got %q: %s"u8, expect, result, test);
            }
            break;
        }}

    }
}

internal static void testFindIndex(ж<FindTest> Ꮡtest, slice<nint> result, ж<testing.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when len(test.matches) == 0 && len(result) == 0: {
        break;
    }
    case {} when test.matches == default! && result != default!: {
        Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result == default!: {
        Ꮡt.Errorf("expected match; got none: %s"u8, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result != default!: {
        var expect = test.matches[0];
        if (expect[0] != result[0] || expect[1] != result[1]) {
            Ꮡt.Errorf("expected %v got %v: %s"u8, expect, result, Ꮡtest.OrTypedNil());
        }
        break;
    }}

}

public static void TestFindIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindIndex(Ꮡtest, MustCompile(test.pat).FindIndex(slice<byte>(test.text)), Ꮡt);
    }
}

public static void TestFindStringIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindIndex(Ꮡtest, MustCompile(test.pat).FindStringIndex(test.text), Ꮡt);
    }
}

public static void TestFindReaderIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindIndex(Ꮡtest, MustCompile(test.pat).FindReaderIndex(new regexp_test_package.strings_ReaderжRuneReader(strings.NewReader(test.text))), Ꮡt);
    }
}

// Now come the simple All cases.
public static void TestFindAll(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in findTests) {
        var result = MustCompile(test.pat).FindAll(slice<byte>(test.text), -1);
        switch (ᐧ) {
        case {} when test.matches == default! && result == default!: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Fatalf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            if (len(test.matches) != len(result)) {
                Ꮡt.Errorf("expected %d matches; got %d: %s"u8, len(test.matches), len(result), test);
                continue;
            }
            foreach (var (k, e) in test.matches) {
                var got = result[k];
                if (len(got) != cap(got)) {
                    Ꮡt.Errorf("match %d: expected capacity %d got %d: %s"u8, k, len(got), cap(got), test);
                }
                @string expect = test.text.slice(e[0], e[1]);
                if (expect != ((sstring)got)) {
                    Ꮡt.Errorf("match %d: expected %q got %q: %s"u8, k, expect, got, test);
                }
            }
            break;
        }}

    }
}

public static void TestFindAllString(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in findTests) {
        var result = MustCompile(test.pat).FindAllString(test.text, -1);
        switch (ᐧ) {
        case {} when test.matches == default! && result == default!: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Errorf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            if (len(test.matches) != len(result)) {
                Ꮡt.Errorf("expected %d matches; got %d: %s"u8, len(test.matches), len(result), test);
                continue;
            }
            foreach (var (k, e) in test.matches) {
                @string expect = test.text.slice(e[0], e[1]);
                if (expect != result[k]) {
                    Ꮡt.Errorf("expected %q got %q: %s"u8, expect, result, test);
                }
            }
            break;
        }}

    }
}

internal static void testFindAllIndex(ж<FindTest> Ꮡtest, slice<slice<nint>> result, ж<testing.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when test.matches == default! && result == default!: {
        break;
    }
    case {} when test.matches == default! && result != default!: {
        Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result == default!: {
        Ꮡt.Errorf("expected match; got none: %s"u8, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result != default!: {
        if (len(test.matches) != len(result)) {
            Ꮡt.Errorf("expected %d matches; got %d: %s"u8, len(test.matches), len(result), Ꮡtest.OrTypedNil());
            return;
        }
        foreach (var (k, e) in test.matches) {
            if (e[0] != result[k][0] || e[1] != result[k][1]) {
                Ꮡt.Errorf("match %d: expected %v got %v: %s"u8, k, e, result[k], Ꮡtest.OrTypedNil());
            }
        }
        break;
    }}

}

public static void TestFindAllIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllIndex(Ꮡtest, MustCompile(test.pat).FindAllIndex(slice<byte>(test.text), -1), Ꮡt);
    }
}

public static void TestFindAllStringIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllIndex(Ꮡtest, MustCompile(test.pat).FindAllStringIndex(test.text, -1), Ꮡt);
    }
}

// Now come the Submatch cases.
internal static void testSubmatchBytes(ж<FindTest> Ꮡtest, nint n, slice<nint> submatches, slice<slice<byte>> result, ж<testing.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    if (len(submatches) != len(result) * 2) {
        Ꮡt.Errorf("match %d: expected %d submatches; got %d: %s"u8, n, len(submatches) / 2, len(result), Ꮡtest.OrTypedNil());
        return;
    }
    for (nint k = 0; k < len(submatches); k += 2) {
        if (submatches[k] == -1) {
            if (result[k / 2] != default!) {
                Ꮡt.Errorf("match %d: expected nil got %q: %s"u8, n, result, Ꮡtest.OrTypedNil());
            }
            continue;
        }
        var got = result[k / 2];
        if (len(got) != cap(got)) {
            Ꮡt.Errorf("match %d: expected capacity %d got %d: %s"u8, n, len(got), cap(got), Ꮡtest.OrTypedNil());
            return;
        }
        @string expect = test.text.slice(submatches[k], submatches[k + 1]);
        if (expect != ((sstring)got)) {
            Ꮡt.Errorf("match %d: expected %q got %q: %s"u8, n, expect, got, Ꮡtest.OrTypedNil());
            return;
        }
    }
}

public static void TestFindSubmatch(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindSubmatch(slice<byte>(test.text));
        switch (ᐧ) {
        case {} when test.matches == default! && result == default!: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Errorf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            testSubmatchBytes(Ꮡtest, 0, test.matches[0], result, Ꮡt);
            break;
        }}

    }
}

internal static void testSubmatchString(ж<FindTest> Ꮡtest, nint n, slice<nint> submatches, slice<@string> result, ж<testing.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    if (len(submatches) != len(result) * 2) {
        Ꮡt.Errorf("match %d: expected %d submatches; got %d: %s"u8, n, len(submatches) / 2, len(result), Ꮡtest.OrTypedNil());
        return;
    }
    for (nint k = 0; k < len(submatches); k += 2) {
        if (submatches[k] == -1) {
            if (result[k / 2] != "") {
                Ꮡt.Errorf("match %d: expected nil got %q: %s"u8, n, result, Ꮡtest.OrTypedNil());
            }
            continue;
        }
        @string expect = test.text.slice(submatches[k], submatches[k + 1]);
        if (expect != result[k / 2]) {
            Ꮡt.Errorf("match %d: expected %q got %q: %s"u8, n, expect, result, Ꮡtest.OrTypedNil());
            return;
        }
    }
}

public static void TestFindStringSubmatch(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindStringSubmatch(test.text);
        switch (ᐧ) {
        case {} when test.matches == default! && result == default!: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Errorf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            testSubmatchString(Ꮡtest, 0, test.matches[0], result, Ꮡt);
            break;
        }}

    }
}

internal static void testSubmatchIndices(ж<FindTest> Ꮡtest, nint n, slice<nint> expect, slice<nint> result, ж<testing.T> Ꮡt) {
    if (len(expect) != len(result)) {
        Ꮡt.Errorf("match %d: expected %d matches; got %d: %s"u8, n, len(expect) / 2, len(result) / 2, Ꮡtest.OrTypedNil());
        return;
    }
    foreach (var (k, e) in expect) {
        if (e != result[k]) {
            Ꮡt.Errorf("match %d: submatch error: expected %v got %v: %s"u8, n, expect, result, Ꮡtest.OrTypedNil());
        }
    }
}

internal static void testFindSubmatchIndex(ж<FindTest> Ꮡtest, slice<nint> result, ж<testing.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when test.matches == default! && result == default!: {
        break;
    }
    case {} when test.matches == default! && result != default!: {
        Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result == default!: {
        Ꮡt.Errorf("expected match; got none: %s"u8, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result != default!: {
        testSubmatchIndices(Ꮡtest, 0, test.matches[0], result, Ꮡt);
        break;
    }}

}

public static void TestFindSubmatchIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindSubmatchIndex(slice<byte>(test.text)), Ꮡt);
    }
}

public static void TestFindStringSubmatchIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindStringSubmatchIndex(test.text), Ꮡt);
    }
}

public static void TestFindReaderSubmatchIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindReaderSubmatchIndex(new regexp_test_package.strings_ReaderжRuneReader(strings.NewReader(test.text))), Ꮡt);
    }
}

// Now come the monster AllSubmatch cases.
public static void TestFindAllSubmatch(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindAllSubmatch(slice<byte>(test.text), -1);
        switch (ᐧ) {
        case {} when test.matches == default! && result == default!: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Errorf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when len(test.matches) != len(result): {
            Ꮡt.Errorf("expected %d matches; got %d: %s"u8, len(test.matches), len(result), test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            foreach (var (k, Δmatch) in test.matches) {
                testSubmatchBytes(Ꮡtest, k, Δmatch, result[k], Ꮡt);
            }
            break;
        }}

    }
}

public static void TestFindAllStringSubmatch(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        var result = MustCompile(test.pat).FindAllStringSubmatch(test.text, -1);
        switch (ᐧ) {
        case {} when test.matches == default! && result == default!: {
            break;
        }
        case {} when test.matches == default! && result != default!: {
            Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 test);
            break;
        }
        case {} when test.matches != default! && result == default!: {
            Ꮡt.Errorf("expected match; got none: %s"u8, test);
            break;
        }
        case {} when len(test.matches) != len(result): {
            Ꮡt.Errorf("expected %d matches; got %d: %s"u8, len(test.matches), len(result), test);
            break;
        }
        case {} when test.matches != default! && result != default!: {
            foreach (var (k, Δmatch) in test.matches) {
                testSubmatchString(Ꮡtest, k, Δmatch, result[k], Ꮡt);
            }
            break;
        }}

    }
}

internal static void testFindAllSubmatchIndex(ж<FindTest> Ꮡtest, slice<slice<nint>> result, ж<testing.T> Ꮡt) {
    ref var test = ref Ꮡtest.DerefOrNull();

    switch (ᐧ) {
    case {} when test.matches == default! && result == default!: {
        break;
    }
    case {} when test.matches == default! && result != default!: {
        Ꮡt.Errorf("expected no match; got one: %s"u8, // ok
 Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result == default!: {
        Ꮡt.Errorf("expected match; got none: %s"u8, Ꮡtest.OrTypedNil());
        break;
    }
    case {} when len(test.matches) != len(result): {
        Ꮡt.Errorf("expected %d matches; got %d: %s"u8, len(test.matches), len(result), Ꮡtest.OrTypedNil());
        break;
    }
    case {} when test.matches != default! && result != default!: {
        foreach (var (k, Δmatch) in test.matches) {
            testSubmatchIndices(Ꮡtest, k, Δmatch, result[k], Ꮡt);
        }
        break;
    }}

}

public static void TestFindAllSubmatchIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindAllSubmatchIndex(slice<byte>(test.text), -1), Ꮡt);
    }
}

public static void TestFindAllStringSubmatchIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in findTests) {
        ref var test = ref heap(new FindTest(), out var Ꮡtest);
        test = vᴛ1;

        testFindAllSubmatchIndex(Ꮡtest, MustCompile(test.pat).FindAllStringSubmatchIndex(test.text, -1), Ꮡt);
    }
}

} // end regexp_internal_test_package
