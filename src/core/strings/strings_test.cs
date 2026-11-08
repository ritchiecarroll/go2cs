// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using Δio = io_package;
using iter = iter_package;
using Δmath = math_package;
using rand = go.math.rand_package;
using slices = slices_package;
using strconv = strconv_package;
using static strings_package;
using testing = testing_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using go.math;
using go.unicode;
using static go.strings_internal_test_package;
using strings = strings_package;

partial class strings_test_package {

internal static slice<@string> collect(ж<testing.T> t, iter.Seq<@string> seq) {
    var @out = slices.Collect(seq);
    var out1 = slices.Collect(seq);
    if (!slices.Equal<slice<@string>, @string>(@out, out1)) {
        t.Fatalf("inconsistent seq:\n%s\n%s", @out, out1);
    }
    return @out;
}

partial struct LinesTest {
    internal @string a;
    internal slice<@string> b;
}

internal static slice<LinesTest> linesTests = [
    new(a: "abc\nabc\n", b: ["abc\n", "abc\n"]),
    new(a: "abc\r\nabc", b: ["abc\r\n", "abc"]),
    new(a: "abc\r\n", b: ["abc\r\n"]),
    new(a: "\nabc", b: ["\n", "abc"]),
    new(a: "\nabc\n\n", b: ["\n", "abc\n", "\n"])
];

public static void TestLines(ж<testing.T> t) {
    foreach (var (_, s) in range(linesTests)) {
        var result = slices.Collect(Lines(s.a));
        if (!slices.Equal<slice<@string>, @string>(result, s.b)) {
            t.Errorf(@"slices.Collect(Lines(%q)) = %q; want %q", s.a, result, s.b);
        }
    }
}

internal static @string abcd = "abcd";
internal static @string faces = "☺☻☹";
internal static @string commas = "1,2,3,4";
internal static @string dots = "1....2....3....4";

partial struct IndexTest {
    internal @string s;
    internal @string sep;
    internal nint @out;
}

internal static slice<IndexTest> indexTests = [
    new("", "", 0),
    new("", "a", -1),
    new("", "foo", -1),
    new("fo", "foo", -1),
    new("foo", "foo", 0),
    new("oofofoofooo", "f", 2),
    new("oofofoofooo", "foo", 4),
    new("barfoobarfoo", "foo", 3),
    new("foo", "", 0),
    new("foo", "o", 1),
    new("abcABCabc", "A", 3),
    new("jrzm6jjhorimglljrea4w3rlgosts0w2gia17hno2td4qd1jz", "jz", 47),
    new("ekkuk5oft4eq0ocpacknhwouic1uua46unx12l37nioq9wbpnocqks6", "ks6", 52),
    new("999f2xmimunbuyew5vrkla9cpwhmxan8o98ec", "98ec", 33),
    new("9lpt9r98i04k8bz6c6dsrthb96bhi", "96bhi", 24),
    new("55u558eqfaod2r2gu42xxsu631xf0zobs5840vl", "5840vl", 33),
    // cases with one byte strings - test special case in Index()
    new("", "a", -1),
    new("x", "a", -1),
    new("x", "x", 0),
    new("abc", "a", 0),
    new("abc", "b", 1),
    new("abc", "c", 2),
    new("abc", "x", -1),
    // test special cases in Index() for short strings
    new("", "ab", -1),
    new("bc", "ab", -1),
    new("ab", "ab", 0),
    new("xab", "ab", 1),
    new("xab"u8[..2], "ab", -1),
    new("", "abc", -1),
    new("xbc", "abc", -1),
    new("abc", "abc", 0),
    new("xabc", "abc", 1),
    new("xabc"u8[..3], "abc", -1),
    new("xabxc", "abc", -1),
    new("", "abcd", -1),
    new("xbcd", "abcd", -1),
    new("abcd", "abcd", 0),
    new("xabcd", "abcd", 1),
    new("xyabcd"u8[..5], "abcd", -1),
    new("xbcqq", "abcqq", -1),
    new("abcqq", "abcqq", 0),
    new("xabcqq", "abcqq", 1),
    new("xyabcqq"u8[..6], "abcqq", -1),
    new("xabxcqq", "abcqq", -1),
    new("xabcqxq", "abcqq", -1),
    new("", "01234567", -1),
    new("32145678", "01234567", -1),
    new("01234567", "01234567", 0),
    new("x01234567", "01234567", 1),
    new("x0123456x01234567", "01234567", 9),
    new("xx01234567"u8[..9], "01234567", -1),
    new("", "0123456789", -1),
    new("3214567844", "0123456789", -1),
    new("0123456789", "0123456789", 0),
    new("x0123456789", "0123456789", 1),
    new("x012345678x0123456789", "0123456789", 11),
    new("xyz0123456789"u8[..12], "0123456789", -1),
    new("x01234567x89", "0123456789", -1),
    new("", "0123456789012345", -1),
    new("3214567889012345", "0123456789012345", -1),
    new("0123456789012345", "0123456789012345", 0),
    new("x0123456789012345", "0123456789012345", 1),
    new("x012345678901234x0123456789012345", "0123456789012345", 17),
    new("", "01234567890123456789", -1),
    new("32145678890123456789", "01234567890123456789", -1),
    new("01234567890123456789", "01234567890123456789", 0),
    new("x01234567890123456789", "01234567890123456789", 1),
    new("x0123456789012345678x01234567890123456789", "01234567890123456789", 21),
    new("xyz01234567890123456789"u8[..22], "01234567890123456789", -1),
    new("", "0123456789012345678901234567890", -1),
    new("321456788901234567890123456789012345678911", "0123456789012345678901234567890", -1),
    new("0123456789012345678901234567890", "0123456789012345678901234567890", 0),
    new("x0123456789012345678901234567890", "0123456789012345678901234567890", 1),
    new("x012345678901234567890123456789x0123456789012345678901234567890", "0123456789012345678901234567890", 32),
    new("xyz0123456789012345678901234567890"u8[..33], "0123456789012345678901234567890", -1),
    new("", "01234567890123456789012345678901", -1),
    new("32145678890123456789012345678901234567890211", "01234567890123456789012345678901", -1),
    new("01234567890123456789012345678901", "01234567890123456789012345678901", 0),
    new("x01234567890123456789012345678901", "01234567890123456789012345678901", 1),
    new("x0123456789012345678901234567890x01234567890123456789012345678901", "01234567890123456789012345678901", 33),
    new("xyz01234567890123456789012345678901"u8[..34], "01234567890123456789012345678901", -1),
    new("xxxxxx012345678901234567890123456789012345678901234567890123456789012", "012345678901234567890123456789012345678901234567890123456789012", 6),
    new("", "0123456789012345678901234567890123456789", -1),
    new("xx012345678901234567890123456789012345678901234567890123456789012", "0123456789012345678901234567890123456789", 2),
    new("xx012345678901234567890123456789012345678901234567890123456789012"u8[..41], "0123456789012345678901234567890123456789", -1),
    new("xx012345678901234567890123456789012345678901234567890123456789012", "0123456789012345678901234567890123456xxx", -1),
    new("xx0123456789012345678901234567890123456789012345678901234567890120123456789012345678901234567890123456xxx", "0123456789012345678901234567890123456xxx", 65),
    // test fallback to Rabin-Karp.
    new("oxoxoxoxoxoxoxoxoxoxoxoy", "oy", 22),
    new("oxoxoxoxoxoxoxoxoxoxoxox", "oy", -1),
    // test fallback to IndexRune
    new("oxoxoxoxoxoxoxoxoxoxox☺", "☺", 22),
    // invalid UTF-8 byte sequence (must be longer than bytealg.MaxBruteForce to
    // test that we don't use IndexRune)
    new(((@string)(new byte[]{0x78, 0x78, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x78, 0x78, 0x78, 0xed, 0x9f, 0xc0})), ((@string)(new byte[]{0xed, 0x9f, 0xc0})), 105)
];

internal static slice<IndexTest> lastIndexTests = [
    new("", "", 0),
    new("", "a", -1),
    new("", "foo", -1),
    new("fo", "foo", -1),
    new("foo", "foo", 0),
    new("foo", "f", 0),
    new("oofofoofooo", "f", 7),
    new("oofofoofooo", "foo", 7),
    new("barfoobarfoo", "foo", 9),
    new("foo", "", 3),
    new("foo", "o", 2),
    new("abcABCabc", "A", 3),
    new("abcABCabc", "a", 6)
];

internal static slice<IndexTest> indexAnyTests = slice<IndexTest>([
    new("", "", -1),
    new("", "a", -1),
    new("", "abc", -1),
    new("a", "", -1),
    new("a", "a", 0),
    new(((@string)(new byte[]{0x80})), ((@string)(new byte[]{0xff, 0x62})), 0),
    new("aaa", "a", 0),
    new("abc", "xyz", -1),
    new("abc", "xcz", 2),
    new("ab☺c", "x☺yz", 2),
    new("a☺b☻c☹d", "cx", len("a☺b☻")),
    new("a☺b☻c☹d", "uvw☻xyz", len("a☺b")),
    new("aRegExp*", ".(|)*+?^$[]", 7),
    new(dots + dots + dots, " ", -1),
    new("012abcba210", ((@string)(new byte[]{0xff, 0x62})), 4),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x80, 0x62, 0x63, 0x62, 0x80, 0x32, 0x31, 0x30})), ((@string)(new byte[]{0xff, 0x62})), 3),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0xcf, 0x80, 0x61, 0x62, 0x63})), ((@string)(new byte[]{0xcf, 0x62, 0x80})), 10)
]);

internal static slice<IndexTest> lastIndexAnyTests = slice<IndexTest>([
    new("", "", -1),
    new("", "a", -1),
    new("", "abc", -1),
    new("a", "", -1),
    new("a", "a", 0),
    new(((@string)(new byte[]{0x80})), ((@string)(new byte[]{0xff, 0x62})), 0),
    new("aaa", "a", 2),
    new("abc", "xyz", -1),
    new("abc", "ab", 1),
    new("ab☺c", "x☺yz", 2),
    new("a☺b☻c☹d", "cx", len("a☺b☻")),
    new("a☺b☻c☹d", "uvw☻xyz", len("a☺b")),
    new("a.RegExp*", ".(|)*+?^$[]", 8),
    new(dots + dots + dots, " ", -1),
    new("012abcba210", ((@string)(new byte[]{0xff, 0x62})), 6),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x80, 0x62, 0x63, 0x62, 0x80, 0x32, 0x31, 0x30})), ((@string)(new byte[]{0xff, 0x62})), 7),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0xcf, 0x80, 0x61, 0x62, 0x63})), ((@string)(new byte[]{0xcf, 0x62, 0x80})), 10)
]);

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runIndexTests(ж<testing.T> t, Func<@string, @string, nint> f, @string funcName, slice<IndexTest> testCases) {
    foreach (var (_, test) in range(testCases)) {
        nint actual = f(test.s, test.sep);
        if (actual != test.@out) {
            t.Errorf("%s(%q,%q) = %v; want %v", funcName, test.s, test.sep, actual, test.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string indexˢ => ref indexˢᵇ;
private static readonly @string indexˢᵇ = "Index"u8;

public static void TestIndex(ж<testing.T> t) { runIndexTests(t, Index, indexˢ, indexTests); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lastIndexˢ => ref lastIndexˢᵇ;
private static readonly @string lastIndexˢᵇ = "LastIndex"u8;

public static void TestLastIndex(ж<testing.T> t) { runIndexTests(t, LastIndex, lastIndexˢ, lastIndexTests); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string indexAnyˢ => ref indexAnyˢᵇ;
private static readonly @string indexAnyˢᵇ = "IndexAny"u8;

public static void TestIndexAny(ж<testing.T> t) { runIndexTests(t, IndexAny, indexAnyˢ, indexAnyTests); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lastIndexAnyˢ => ref lastIndexAnyˢᵇ;
private static readonly @string lastIndexAnyˢᵇ = "LastIndexAny"u8;

public static void TestLastIndexAny(ж<testing.T> t) {
    runIndexTests(t, LastIndexAny, lastIndexAnyˢ, lastIndexAnyTests);
}

public static void TestIndexByte(ж<testing.T> t) {
    foreach (var (_, tt) in range(indexTests)) {
        if (len(tt.sep) != 1) {
            continue;
        }
        nint pos = IndexByte(tt.s, tt.sep[0]);
        if (pos != tt.@out) {
            t.Errorf(@"IndexByte(%q, %q) = %v; want %v", tt.s, tt.sep[0], pos, tt.@out);
        }
    }
}

public static void TestLastIndexByte(ж<testing.T> t) {
    var testCases = slice<IndexTest>([
        new("", "q", -1),
        new("abcdef", "q", -1),
        new("abcdefabcdef", "a", len("abcdef")),      // something in the middle
        new("abcdefabcdef", "f", len("abcdefabcde")), // last byte
        new("zabcdefabcdef", "z", 0),                 // first byte
        new("a☺b☻c☹d", "b", len("a☺"))                // non-ascii
    ]);
    foreach (var (_, test) in range(testCases)) {
        nint actual = LastIndexByte(test.s, test.sep[0]);
        if (actual != test.@out) {
            t.Errorf("LastIndexByte(%q,%c) = %v; want %v", test.s, test.sep[0], actual, test.@out);
        }
    }
}

internal static nint simpleIndex(@string s, @string sep) {
    nint n = len(sep);
    for (nint i = n; i <= len(s); i++) {
        if (s.slice(i - n, i) == sep) {
            return i - n;
        }
    }
    return -1;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string charsᶜ => ref charsᶜᵇ;
private static readonly @string charsᶜᵇ = "abcdefghijklmnopqrstuvwxyz0123456789"u8;

public static void TestIndexRandom(ж<testing.T> t) {
    @string chars = charsᶜ;
    for (nint times = 0; times < 10; times++) {
        for (nint strLen = 5 + rand.Intn(5); strLen < 140; strLen += 10) { // Arbitrary
            var s1 = new slice<byte>(strLen);
            foreach (var i in range(len(s1))) {
                s1[i] = chars[rand.Intn(len(chars))];
            }
            @string s = (@string)s1;
            for (nint i = 0; i < 50; i++) {
                nint begin = rand.Intn(len(s) + 1);
                nint end = begin + rand.Intn(len(s) + 1 - begin);
                @string sep = s.slice(begin, end);
                if (i % 4 == 0) {
                    nint pos = rand.Intn(len(sep) + 1);
                    sep = sep.slice(0, pos) + "A" + sep.slice(pos);
                }
                nint want = simpleIndex(s, sep);
                nint res = Index(s, sep);
                if (res != want) {
                    t.Errorf("Index(%s,%s) = %d; want %d", s, sep, res, want);
                }
            }
        }
    }
}

internal partial struct TestIndexRune_tests /*dyn*/ {
    internal @string @in;
    internal rune rune;
    internal nint want;
}

public static void TestIndexRune(ж<testing.T> t) {
    var tests = slice<TestIndexRune_tests>([
        new("", 'a', -1),
        new("", '☺', -1),
        new("foo", '☹', -1),
        new("foo", 'o', 1),
        new("foo☺bar", '☺', 3),
        new("foo☺☻☹bar", '☹', 9),
        new("a A x", 'A', 2),
        new("some_text=some_value", '=', 9),
        new("☺a", 'a', 3),
        new("a☻☺b", '☺', 4),

        // RuneError should match any invalid UTF-8 byte sequence.
        new("�", '�', 0),
        new(((@string)(new byte[]{0xff})), '�', 0),
        new("☻x�", '�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98})), '�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0xef, 0xbf, 0xbd})), '�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0x78})), '�', len("☻x")),

        // Invalid rune values should never match.
        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), -1, -1),
        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), 0xD800, -1), // Surrogate pair
        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), utf8.MaxRune + 1, -1),

        // 2 bytes
        new("ӆ", 'ӆ', 0),
        new("a", 'ӆ', -1),
        new("  ӆ", 'ӆ', 2),
        new("  a", 'ӆ', -1),
        new(Repeat("ц", 64) + "ӆ"u8, 'ӆ', 128), // test cutover
        new(Repeat("Ꙁ", 64) + "Ꚁ"u8, '䚀', -1),  // 'Ꚁ' and '䚀' share the same last two bytes

        // 3 bytes
        new("Ꚁ", 'Ꚁ', 0),
        new("a", 'Ꚁ', -1),
        new("  Ꚁ", 'Ꚁ', 2),
        new("  a", 'Ꚁ', -1),
        new(Repeat("Ꙁ", 64) + "Ꚁ"u8, 'Ꚁ', 192),          // test cutover
        new(Repeat("𡋀", 64) + "𡌀"u8, (rune)0x23300, -1), // '𡌀' and '𣌀' share the same last two bytes

        // 4 bytes
        new("𡌀", (rune)0x21300, 0),
        new("a", (rune)0x21300, -1),
        new("  𡌀", (rune)0x21300, 2),
        new("  a", (rune)0x21300, -1),
        new(Repeat("𡋀", 64) + "𡌀"u8, (rune)0x21300, 256), // test cutover
        new(Repeat("𡋀", 64), (rune)0x21300, -1),

        // Test the cutover to bytealg.IndexString when it is triggered in
        // the middle of rune that contains consecutive runs of equal bytes.
        new("aaaaaKKKK\U000bc104", (rune)0xBC104, 17), // cutover: (n + 16) / 8
        new("aaaaaKKKK鄄", '鄄', 17),
        new("aaKKKKKa\U000bc104", (rune)0xBC104, 18), // cutover: 4 + n>>4
        new("aaKKKKKa鄄", '鄄', 18)
    ]);
    foreach (var (_, tt) in range(tests)) {
        nint got = IndexRune(tt.@in, tt.rune); if (got != tt.want) {
            t.Errorf("IndexRune(%q, %d) = %v; want %v", tt.@in, tt.rune, got, tt.want);
        }
    }

    // Make sure we trigger the cutover and string(rune) conversion.
    @string haystack = "test"u8 + Repeat("𡋀", 32) + "𡌀"u8;
    var allocs = testing.AllocsPerRun(1000, () => {
        {
            nint i = IndexRune(haystack, 's'); if (i != 2) {
                t.Fatalf("'s' at %d; want 2", i);
            }
        }
        {
            nint i = IndexRune(haystack, (rune)0x21300); if (i != 132) {
                t.Fatalf("'𡌀' at %d; want 4", i);
            }
        }
    });
    if (allocs != 0D && testing.CoverMode() == ""u8) {
        t.Errorf("expected no allocations, got %f", allocs);
    }
}

internal static ref readonly @string benchmarkString => ref benchmarkStringᵇ;
private static readonly @string benchmarkStringᵇ = "some_text=some☺value"u8;

public static void BenchmarkIndexRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint got = IndexRune(benchmarkString, '☺'); if (got != 14) {
        Ꮡb.Fatalf("wrong index: expected 14, got=%d", got);
    }
    for (nint i = 0; i < b.N; i++) {
        IndexRune(benchmarkString, '☺');
    }
}

internal static @string benchmarkLongString = Repeat(" ", 100) + benchmarkString;

public static void BenchmarkIndexRuneLongString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint got = IndexRune(benchmarkLongString, '☺'); if (got != 114) {
        Ꮡb.Fatalf("wrong index: expected 114, got=%d", got);
    }
    for (nint i = 0; i < b.N; i++) {
        IndexRune(benchmarkLongString, '☺');
    }
}

public static void BenchmarkIndexRuneFastPath(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint got = IndexRune(benchmarkString, 'v'); if (got != 17) {
        Ꮡb.Fatalf("wrong index: expected 17, got=%d", got);
    }
    for (nint i = 0; i < b.N; i++) {
        IndexRune(benchmarkString, 'v');
    }
}

public static void BenchmarkIndex(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint got = Index(benchmarkString, "v"); if (got != 17) {
        Ꮡb.Fatalf("wrong index: expected 17, got=%d", got);
    }
    for (nint i = 0; i < b.N; i++) {
        Index(benchmarkString, "v");
    }
}

public static void BenchmarkLastIndex(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint got = Index(benchmarkString, "v"); if (got != 17) {
        Ꮡb.Fatalf("wrong index: expected 17, got=%d", got);
    }
    for (nint i = 0; i < b.N; i++) {
        LastIndex(benchmarkString, "v");
    }
}

public static void BenchmarkIndexByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint got = IndexByte(benchmarkString, (byte)'v'); if (got != 17) {
        Ꮡb.Fatalf("wrong index: expected 17, got=%d", got);
    }
    for (nint i = 0; i < b.N; i++) {
        IndexByte(benchmarkString, (byte)'v');
    }
}

partial struct SplitTest {
    internal @string s;
    internal @string sep;
    internal nint n;
    internal slice<@string> a;
}

internal static slice<SplitTest> splittests = [
    new("", "", -1, []),
    new(abcd, "", 2, ["a", "bcd"]),
    new(abcd, "", 4, ["a", "b", "c", "d"]),
    new(abcd, "", -1, ["a", "b", "c", "d"]),
    new(faces, "", -1, ["☺", "☻", "☹"]),
    new(faces, "", 3, ["☺", "☻", "☹"]),
    new(faces, "", 17, ["☺", "☻", "☹"]),
    new("☺�☹", "", -1, ["☺", "�", "☹"]),
    new(abcd, "a", 0, default),
    new(abcd, "a", -1, ["", "bcd"]),
    new(abcd, "z", -1, ["abcd"]),
    new(commas, ",", -1, ["1", "2", "3", "4"]),
    new(dots, "...", -1, ["1", ".2", ".3", ".4"]),
    new(faces, "☹", -1, ["☺☻", ""]),
    new(faces, "~", -1, [faces]),
    new("1 2 3 4", " ", 3, ["1", "2", "3 4"]),
    new("1 2", " ", 3, ["1", "2"]),
    new("", "T", unchecked((nint)(2305843009213693951L)), [""]),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), "", -1, [((@string)(new byte[]{0xff})), "-", ((@string)(new byte[]{0xff}))]),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), "-", -1, [((@string)(new byte[]{0xff})), ((@string)(new byte[]{0xff}))])
];

public static void TestSplit(ж<testing.T> t) {
    foreach (var (_, tt) in range(splittests)) {
        var a = SplitN(tt.s, tt.sep, tt.n);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            t.Errorf("Split(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, a, tt.a);
            continue;
        }
        if (tt.n < 0) {
            var a2 = slices.Collect(SplitSeq(tt.s, tt.sep));
            if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
                t.Errorf(@"collect(SplitSeq(%q, %q)) = %v; want %v", tt.s, tt.sep, a2, tt.a);
            }
        }
        if (tt.n == 0) {
            continue;
        }
        @string s = Join(a, tt.sep);
        if (s != tt.s) {
            t.Errorf("Join(Split(%q, %q, %d), %q) = %q", tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = Split(tt.s, tt.sep);
            if (!slices.Equal<slice<@string>, @string>(a, b)) {
                t.Errorf("Split disagrees with SplitN(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, b, a);
            }
        }
    }
}

internal static slice<SplitTest> splitaftertests = [
    new(abcd, "a", -1, ["a", "bcd"]),
    new(abcd, "z", -1, ["abcd"]),
    new(abcd, "", -1, ["a", "b", "c", "d"]),
    new(commas, ",", -1, ["1,", "2,", "3,", "4"]),
    new(dots, "...", -1, ["1...", ".2...", ".3...", ".4"]),
    new(faces, "☹", -1, ["☺☻☹", ""]),
    new(faces, "~", -1, [faces]),
    new(faces, "", -1, ["☺", "☻", "☹"]),
    new("1 2 3 4", " ", 3, ["1 ", "2 ", "3 4"]),
    new("1 2 3", " ", 3, ["1 ", "2 ", "3"]),
    new("1 2", " ", 3, ["1 ", "2"]),
    new("123", "", 2, ["1", "23"]),
    new("123", "", 17, ["1", "2", "3"])
];

public static void TestSplitAfter(ж<testing.T> t) {
    foreach (var (_, tt) in range(splitaftertests)) {
        var a = SplitAfterN(tt.s, tt.sep, tt.n);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            t.Errorf(@"Split(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, a, tt.a);
            continue;
        }
        if (tt.n < 0) {
            var a2 = slices.Collect(SplitAfterSeq(tt.s, tt.sep));
            if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
                t.Errorf(@"collect(SplitAfterSeq(%q, %q)) = %v; want %v", tt.s, tt.sep, a2, tt.a);
            }
        }
        @string s = Join(a, ""u8);
        if (s != tt.s) {
            t.Errorf(@"Join(Split(%q, %q, %d), %q) = %q", tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = SplitAfter(tt.s, tt.sep);
            if (!slices.Equal<slice<@string>, @string>(a, b)) {
                t.Errorf("SplitAfter disagrees with SplitAfterN(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, b, a);
            }
        }
    }
}

partial struct FieldsTest {
    internal @string s;
    internal slice<@string> a;
}

internal static slice<FieldsTest> fieldstests = [
    new("", []),
    new(" ", []),
    new(" \t ", []),
    new("\u2000", []),
    new("  abc  ", ["abc"]),
    new("1 2 3 4", ["1", "2", "3", "4"]),
    new("1  2  3  4", ["1", "2", "3", "4"]),
    new("1\t\t2\t\t3\t4", ["1", "2", "3", "4"]),
    new("1\u20002\u20013\u20024", ["1", "2", "3", "4"]),
    new("\u2000\u2001\u2002", []),
    new("\n™\t™\n", ["™", "™"]),
    new("\n\u20001™2\u2000 \u2001 ™", ["1™2", "™"]),
    new("\n1\uFFFD \uFFFD2\u20003\uFFFD4", ["1\uFFFD", "\uFFFD2", "3\uFFFD4"]),
    new(((@string)(new byte[]{0x31, 0xff, 0xe2, 0x80, 0x80, 0xff, 0x32, 0xff, 0x20, 0xff})), [((@string)(new byte[]{0x31, 0xff})), ((@string)(new byte[]{0xff, 0x32, 0xff})), ((@string)(new byte[]{0xff}))]),
    new(faces, [faces])
];

public static void TestFields(ж<testing.T> t) {
    foreach (var (_, tt) in range(fieldstests)) {
        var a = Fields(tt.s);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            t.Errorf("Fields(%q) = %v; want %v", tt.s, a, tt.a);
            continue;
        }
        var a2 = collect(t, FieldsSeq(tt.s));
        if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
            t.Errorf(@"collect(FieldsSeq(%q)) = %v; want %v", tt.s, a2, tt.a);
        }
    }
}

public static readonly ж<slice<FieldsTest>> ᏑFieldsFuncTests = new StaticBox<slice<FieldsTest>>([
    new("", []),
    new("XX", []),
    new("XXhiXXX", ["hi"]),
    new("aXXbXXXcX", ["a", "b", "c"])
]);
public static ref slice<FieldsTest> FieldsFuncTests => ref ((StaticBox<slice<FieldsTest>>)ᏑFieldsFuncTests).ValueSlot;

public static void TestFieldsFunc(ж<testing.T> t) {
    foreach (var (_, tt) in range(fieldstests)) {
        var a = FieldsFunc(tt.s, Δunicode.IsSpace);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            t.Errorf("FieldsFunc(%q, unicode.IsSpace) = %v; want %v", tt.s, a, tt.a);
            continue;
        }
    }
    var pred = (rune c) => c == 'X';
    foreach (var (_, tt) in range(FieldsFuncTests)) {
        var a = FieldsFunc(tt.s, pred);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            t.Errorf("FieldsFunc(%q) = %v, want %v", tt.s, a, tt.a);
        }
        var a2 = collect(t, FieldsFuncSeq(tt.s, pred));
        if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
            t.Errorf(@"collect(FieldsFuncSeq(%q)) = %v; want %v", tt.s, a2, tt.a);
        }
    }
}

// Test case for any function which accepts and returns a single string.
partial struct StringTest {
    internal @string @in, @out;
}

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runStringTests(ж<testing.T> t, Func<@string, @string> f, @string funcName, slice<StringTest> testCases) {
    foreach (var (_, tc) in range(testCases)) {
        @string actual = f(tc.@in);
        if (actual != tc.@out) {
            t.Errorf("%s(%q) = %q; want %q", funcName, tc.@in, actual, tc.@out);
        }
    }
}

internal static slice<StringTest> upperTests = [
    new("", ""),
    new("ONLYUPPER", "ONLYUPPER"),
    new("abc", "ABC"),
    new("AbC123", "ABC123"),
    new("azAZ09_", "AZAZ09_"),
    new("longStrinGwitHmixofsmaLLandcAps", "LONGSTRINGWITHMIXOFSMALLANDCAPS"),
    new("RENAN BASTOS 93 AOSDAJDJAIDJAIDAJIaidsjjaidijadsjiadjiOOKKO", "RENAN BASTOS 93 AOSDAJDJAIDJAIDAJIAIDSJJAIDIJADSJIADJIOOKKO"),
    new("long\u0250string\u0250with\u0250nonascii\u2C6Fchars", "LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS"),
    new("\u0250\u0250\u0250\u0250\u0250", "\u2C6F\u2C6F\u2C6F\u2C6F\u2C6F"), // grows one byte per char
    new("a\u0080\U0010FFFF", "A\u0080\U0010FFFF")                            // test utf8.RuneSelf and utf8.MaxRune
];

internal static slice<StringTest> lowerTests = [
    new("", ""),
    new("abc", "abc"),
    new("AbC123", "abc123"),
    new("azAZ09_", "azaz09_"),
    new("longStrinGwitHmixofsmaLLandcAps", "longstringwithmixofsmallandcaps"),
    new("renan bastos 93 AOSDAJDJAIDJAIDAJIaidsjjaidijadsjiadjiOOKKO", "renan bastos 93 aosdajdjaidjaidajiaidsjjaidijadsjiadjiookko"),
    new("LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS", "long\u0250string\u0250with\u0250nonascii\u0250chars"),
    new("\u2C6D\u2C6D\u2C6D\u2C6D\u2C6D", "\u0251\u0251\u0251\u0251\u0251"), // shrinks one byte per char
    new("A\u0080\U0010FFFF", "a\u0080\U0010FFFF")                            // test utf8.RuneSelf and utf8.MaxRune
];

internal static ref readonly @string space => ref spaceᵇ;
private static readonly @string spaceᵇ = "\t\v\r\f\n\u0085\u00a0\u2000\u3000"u8;

internal static slice<StringTest> trimSpaceTests = [
    new("", ""),
    new("abc", "abc"),
    new(space + "abc" + space, "abc"),
    new(" ", ""),
    new(" \t\r\n \t\t\r\r\n\n ", ""),
    new(" \t\r\n x\t\t\r\r\n\n ", "x"),
    new(" \u2000\t\r\n x\t\t\r\r\ny\n \u3000", "x\t\t\r\r\ny"),
    new("1 \t\r\n2", "1 \t\r\n2"),
    new(((@string)(new byte[]{0x20, 0x78, 0x80})), ((@string)(new byte[]{0x78, 0x80}))),
    new(((@string)(new byte[]{0x20, 0x78, 0xc0})), ((@string)(new byte[]{0x78, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0})), ((@string)(new byte[]{0x78, 0x20, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0}))),
    new("x ☺ ", "x ☺")
];

internal static @string tenRunes(rune ch) {
    var r = new slice<rune>(10);
    foreach (var i in range(len(r))) {
        r[i] = ch;
    }
    return (@string)r;
}

// User-defined self-inverse mapping function
internal static rune rot13(rune r) {
    var step = (rune)13;
    if (r >= 'a' && r <= 'z') {
        return ((r - 'a' + step) % 26) + 'a';
    }
    if (r >= 'A' && r <= 'Z') {
        return ((r - 'A' + step) % 26) + 'A';
    }
    return r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aToZedˢ => ref aToZedˢᵇ;
private static readonly @string aToZedˢᵇ = "a to zed"u8;
internal static ref readonly @string nGbMrqˢ => ref nGbMrqˢᵇ;
private static readonly @string nGbMrqˢᵇ = "n gb mrq"u8;
internal static ref readonly @string helloˢ4 => ref helloˢ4ᵇ;
private static readonly @string helloˢ4ᵇ = "Hello, 세계"u8;
internal static ref readonly @string inputStringThatWeExpectˢ => ref inputStringThatWeExpectˢᵇ;
private static readonly @string inputStringThatWeExpectˢᵇ = "Input string that we expect not to be copied."u8;
internal static readonly object unexpectedCopyDuringˢ = (@string)"unexpected copy during identity map"u8;
internal static ref readonly @string helloWorldˢ2 => ref helloWorldˢ2ᵇ;
private static readonly @string helloWorldˢ2ᵇ = "Hello\uFFFDWorld"u8;
internal static ref readonly @string abc123ˢ => ref abc123ˢᵇ;
private static readonly @string abc123ˢᵇ = "   abc    123   "u8;
internal static ref readonly @string abc123ˢ2 => ref abc123ˢ2ᵇ;
private static readonly @string abc123ˢ2ᵇ = "abc123"u8;

public static void TestMap(ж<testing.T> t) {
    // Run a couple of awful growth/shrinkage tests
    @string a = tenRunes('a');
    // 1. Grow. This triggers two reallocations in Map.
    var maxRune = rune (rune _) => Δunicode.MaxRune;
    @string m = Map(maxRune, a);
    @string expect = tenRunes(Δunicode.MaxRune);
    if (m != expect) {
        t.Errorf("growing: expected %q got %q", expect, m);
    }

    // 2. Shrink
    var minRune = (rune _) => (rune)'a';
    m = Map(minRune, tenRunes(Δunicode.MaxRune));
    expect = a;
    if (m != expect) {
        t.Errorf("shrinking: expected %q got %q", expect, m);
    }

    // 3. Rot13
    m = Map(rot13, aToZedˢ);
    expect = nGbMrqˢ;
    if (m != expect) {
        t.Errorf("rot13: expected %q got %q", expect, m);
    }

    // 4. Rot13^2
    m = Map(rot13, Map(rot13, aToZedˢ));
    expect = aToZedˢ;
    if (m != expect) {
        t.Errorf("rot13: expected %q got %q", expect, m);
    }

    // 5. Drop
    var dropNotLatin = (rune rΔ1) => {
        if (Δunicode.Is(Δunicode.Latin, rΔ1)) {
            return rΔ1;
        }
        return -1;
    };
    m = Map(dropNotLatin, helloˢ4);
    expect = helloˢ3;
    if (m != expect) {
        t.Errorf("drop: expected %q got %q", expect, m);
    }

    // 6. Identity
    var identity = (rune rΔ2) => rΔ2;
    @string orig = inputStringThatWeExpectˢ;
    m = Map(identity, orig);
    if (@unsafe.StringData(orig) != @unsafe.StringData(m)) {
        t.Error(unexpectedCopyDuringˢ);
    }

    // 7. Handle invalid UTF-8 sequence
    var replaceNotLatin = rune (rune rΔ3) => {
        if (Δunicode.Is(Δunicode.Latin, rΔ3)) {
            return rΔ3;
        }
        return utf8.RuneError;
    };
    m = Map(replaceNotLatin, ((@string)(new byte[]{0x48, 0x65, 0x6c, 0x6c, 0x6f, 0xad, 0x57, 0x6f, 0x72, 0x6c, 0x64})));
    expect = helloWorldˢ2;
    if (m != expect) {
        t.Errorf("replace invalid sequence: expected %q got %q", expect, m);
    }

    // 8. Check utf8.RuneSelf and utf8.MaxRune encoding
    var encode = rune (rune rΔ4) => {
        switch (rΔ4) {
        case var ᴛ when ᴛ == utf8.RuneSelf: {
            return Δunicode.MaxRune;
        }
        case var ᴛ when ᴛ == Δunicode.MaxRune: {
            return utf8.RuneSelf;
        }}
        return rΔ4;
    };
    @string s = ((@string)(rune)utf8.RuneSelf) + ((@string)(rune)utf8.MaxRune);
    @string r = ((@string)(rune)utf8.MaxRune) + ((@string)(rune)utf8.RuneSelf); // reverse of s
    m = Map(encode, s);
    if (m != r) {
        t.Errorf("encoding not handled correctly: expected %q got %q", r, m);
    }
    m = Map(encode, r);
    if (m != s) {
        t.Errorf("encoding not handled correctly: expected %q got %q", s, m);
    }

    // 9. Check mapping occurs in the front, middle and back
    var trimSpaces = (rune rΔ5) => {
        if (Δunicode.IsSpace(rΔ5)) {
            return -1;
        }
        return rΔ5;
    };
    m = Map(trimSpaces, abc123ˢ);
    expect = abc123ˢ2;
    if (m != expect) {
        t.Errorf("trimSpaces: expected %q got %q", expect, m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string toUpperˢ => ref toUpperˢᵇ;
private static readonly @string toUpperˢᵇ = "ToUpper"u8;

public static void TestToUpper(ж<testing.T> t) { runStringTests(t, ToUpper, toUpperˢ, upperTests); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string toLowerˢ => ref toLowerˢᵇ;
private static readonly @string toLowerˢᵇ = "ToLower"u8;

public static void TestToLower(ж<testing.T> t) { runStringTests(t, ToLower, toLowerˢ, lowerTests); }

partial struct toValidUTF8Testsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string repl;
    internal @string @out;
}
internal static slice<toValidUTF8Testsᴛ1> toValidUTF8Tests = [
    new("", "\uFFFD", ""),
    new("abc", "\uFFFD", "abc"),
    new("\uFDDD", "\uFFFD", "\uFDDD"),
    new(((@string)(new byte[]{0x61, 0xff, 0x62})), "\uFFFD", "a\uFFFDb"),
    new(((@string)(new byte[]{0x61, 0xff, 0x62, 0xef, 0xbf, 0xbd})), "X", "aXb\uFFFD"),
    new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0xff, 0x62, 0xe2, 0x98, 0xba, 0xc0, 0xaf, 0x63, 0xe2, 0x98, 0xba, 0xff})), "", "a☺b☺c☺"),
    new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0xff, 0x62, 0xe2, 0x98, 0xba, 0xc0, 0xaf, 0x63, 0xe2, 0x98, 0xba, 0xff})), "日本語", "a☺日本語b☺日本語c☺日本語"),
    new(((@string)(new byte[]{0xc0, 0xaf})), "\uFFFD", "\uFFFD"),
    new(((@string)(new byte[]{0xe0, 0x80, 0xaf})), "\uFFFD", "\uFFFD"),
    new(((@string)(new byte[]{0xed, 0xa0, 0x80})), "abc", "abc"),
    new(((@string)(new byte[]{0xed, 0xbf, 0xbf})), "\uFFFD", "\uFFFD"),
    new(((@string)(new byte[]{0xf0, 0x80, 0x80, 0xaf})), "☺", "☺"),
    new(((@string)(new byte[]{0xf8, 0x80, 0x80, 0x80, 0xaf})), "\uFFFD", "\uFFFD"),
    new(((@string)(new byte[]{0xfc, 0x80, 0x80, 0x80, 0x80, 0xaf})), "\uFFFD", "\uFFFD")
];

public static void TestToValidUTF8(ж<testing.T> t) {
    foreach (var (_, tc) in range(toValidUTF8Tests)) {
        @string got = ToValidUTF8(tc.@in, tc.repl);
        if (got != tc.@out) {
            t.Errorf("ToValidUTF8(%q, %q) = %q; want %q", tc.@in, tc.repl, got, tc.@out);
        }
    }
}

public static void BenchmarkToUpper(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(upperTests)) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                @string actual = ToUpper(tcʗ1.@in);
                if (actual != tcʗ1.@out) {
                    bΔ1.Errorf("ToUpper(%q) = %q; want %q", tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}

public static void BenchmarkToLower(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(lowerTests)) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        b.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                @string actual = ToLower(tcʗ1.@in);
                if (actual != tcʗ1.@out) {
                    bΔ1.Errorf("ToLower(%q) = %q; want %q", tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someStringThatWonTBeˢ => ref someStringThatWonTBeˢᵇ;
private static readonly @string someStringThatWonTBeˢᵇ = "Some string that won't be modified."u8;

public static void BenchmarkMapNoChanges(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var identity = (rune r) => r;
    for (nint i = 0; i < b.N; i++) {
        Map(identity, someStringThatWonTBeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcDefgHIjklmnoPrsTuVyzˢ => ref abcDefgHIjklmnoPrsTuVyzˢᵇ;
private static readonly @string abcDefgHIjklmnoPrsTuVyzˢᵇ = "abcçdefgğhıijklmnoöprsştuüvyz"u8;
internal static ref readonly @string abcDefgHiJklmnoPrsTuVyzˢ => ref abcDefgHiJklmnoPrsTuVyzˢᵇ;
private static readonly @string abcDefgHiJklmnoPrsTuVyzˢᵇ = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ"u8;

public static void TestSpecialCase(ж<testing.T> t) {
    @string lower = abcDefgHIjklmnoPrsTuVyzˢ;
    @string upper = abcDefgHiJklmnoPrsTuVyzˢ;
    @string u = ToUpperSpecial(Δunicode.TurkishCase, upper);
    if (u != upper) {
        t.Errorf("Upper(upper) is %s not %s", u, upper);
    }
    u = ToUpperSpecial(Δunicode.TurkishCase, lower);
    if (u != upper) {
        t.Errorf("Upper(lower) is %s not %s", u, upper);
    }
    @string l = ToLowerSpecial(Δunicode.TurkishCase, lower);
    if (l != lower) {
        t.Errorf("Lower(lower) is %s not %s", l, lower);
    }
    l = ToLowerSpecial(Δunicode.TurkishCase, upper);
    if (l != lower) {
        t.Errorf("Lower(upper) is %s not %s", l, lower);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trimSpaceˢ => ref trimSpaceˢᵇ;
private static readonly @string trimSpaceˢᵇ = "TrimSpace"u8;

public static void TestTrimSpace(ж<testing.T> t) { runStringTests(t, TrimSpace, trimSpaceˢ, trimSpaceTests); }

partial struct trimTestsᴛ1 /*dyn*/ {
    internal @string f;
    internal @string @in, arg, @out;
}
internal static slice<trimTestsᴛ1> trimTests = [
    new("Trim", "abba", "a", "bb"),
    new("Trim", "abba", "ab", ""),
    new("TrimLeft", "abba", "ab", ""),
    new("TrimRight", "abba", "ab", ""),
    new("TrimLeft", "abba", "a", "bba"),
    new("TrimLeft", "abba", "b", "abba"),
    new("TrimRight", "abba", "a", "abb"),
    new("TrimRight", "abba", "b", "abba"),
    new("Trim", "<tag>", "<>", "tag"),
    new("Trim", "* listitem", " *", "listitem"),
    new("Trim", @"""quote""", @"""", "quote"),
    new("Trim", "\u2C6F\u2C6F\u0250\u0250\u2C6F\u2C6F", "\u2C6F", "\u0250\u0250"),
    new("Trim", ((@string)(new byte[]{0x80, 0x74, 0x65, 0x73, 0x74, 0xff})), ((@string)(new byte[]{0xff})), "test"),
    new("Trim", " Ġ ", " ", "Ġ"),
    new("Trim", " Ġİ0", "0 ", "Ġİ"),
    //empty string tests
    new("Trim", "abba", "", "abba"),
    new("Trim", "", "123", ""),
    new("Trim", "", "", ""),
    new("TrimLeft", "abba", "", "abba"),
    new("TrimLeft", "", "123", ""),
    new("TrimLeft", "", "", ""),
    new("TrimRight", "abba", "", "abba"),
    new("TrimRight", "", "123", ""),
    new("TrimRight", "", "", ""),
    new("TrimRight", ((@string)(new byte[]{0xe2, 0x98, 0xba, 0xc0})), "☺", ((@string)(new byte[]{0xe2, 0x98, 0xba, 0xc0}))),
    new("TrimPrefix", "aabb", "a", "abb"),
    new("TrimPrefix", "aabb", "b", "aabb"),
    new("TrimSuffix", "aabb", "a", "aabb"),
    new("TrimSuffix", "aabb", "b", "aab")
];

public static void TestTrim(ж<testing.T> t) {
    foreach (var (_, tc) in range(trimTests)) {
        @string name = tc.f;
        Func<@string, @string, @string> f = default;
        var exprᴛ1 = name;
        switch (ᐧ) {
        case {} when exprᴛ1 == "Trim"u8: {
            f = Trim;
            break;
        }
        case {} when exprᴛ1 == "TrimLeft"u8: {
            f = TrimLeft;
            break;
        }
        case {} when exprᴛ1 == "TrimRight"u8: {
            f = TrimRight;
            break;
        }
        case {} when exprᴛ1 == "TrimPrefix"u8: {
            f = TrimPrefix;
            break;
        }
        case {} when exprᴛ1 == "TrimSuffix"u8: {
            f = TrimSuffix;
            break;
        }
        default: {
            t.Errorf("Undefined trim function %s", name);
            break;
        }}
        @string actual = f(tc.@in, tc.arg);
        if (actual != tc.@out) {
            t.Errorf("%s(%q, %q) = %q; want %q", name, tc.@in, tc.arg, actual, tc.@out);
        }
    }
}

public static void BenchmarkTrim(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, tc) in range(trimTests)) {
            @string name = tc.f;
            Func<@string, @string, @string> f = default;
            var exprᴛ1 = name;
            switch (ᐧ) {
            case {} when exprᴛ1 == "Trim"u8: {
                f = Trim;
                break;
            }
            case {} when exprᴛ1 == "TrimLeft"u8: {
                f = TrimLeft;
                break;
            }
            case {} when exprᴛ1 == "TrimRight"u8: {
                f = TrimRight;
                break;
            }
            case {} when exprᴛ1 == "TrimPrefix"u8: {
                f = TrimPrefix;
                break;
            }
            case {} when exprᴛ1 == "TrimSuffix"u8: {
                f = TrimSuffix;
                break;
            }
            default: {
                Ꮡb.Errorf("Undefined trim function %s", name);
                break;
            }}
            @string actual = f(tc.@in, tc.arg);
            if (actual != tc.@out) {
                Ꮡb.Errorf("%s(%q, %q) = %q; want %q", name, tc.@in, tc.arg, actual, tc.@out);
            }
        }
    }
}

internal partial struct BenchmarkToValidUTF8_tests /*dyn*/ {
    internal @string name;
    internal @string input;
}

public static void BenchmarkToValidUTF8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var tests = slice<BenchmarkToValidUTF8_tests>([
        new("Valid", "typical"),
        new("InvalidASCII", ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xff, 0x62, 0x61, 0x72}))),
        new("InvalidNonASCII", ((@string)(new byte[]{0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e, 0xff, 0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e})))
    ]);
    @string replacement = "\uFFFD";
    b.ResetTimer();
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new BenchmarkToValidUTF8_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                ToValidUTF8(testʗ1.input, replacement);
            }
        });
    }
}

partial struct predicate {
    internal Func<rune, bool> f;
    internal @string name;
}

internal static predicate isSpace = new predicate(Δunicode.IsSpace, "IsSpace");
internal static predicate isDigit = new predicate(Δunicode.IsDigit, "IsDigit");
internal static predicate isUpper = new predicate(Δunicode.IsUpper, "IsUpper");
internal static predicate isValidRune = new predicate(
    (rune r) => r != utf8.RuneError,
    "IsValidRune"
);

internal static predicate not(predicate p) {
        var pʗ1 = p;
    return new predicate(
        (rune r) => !pʗ1.f(r),
        "not "u8 + p.name
    );
}

partial struct trimFuncTestsᴛ1 /*dyn*/ {
    internal predicate f;
    internal @string @in;
    internal @string trimOut;
    internal @string leftOut;
    internal @string rightOut;
}
internal static slice<trimFuncTestsᴛ1> trimFuncTests = slice<trimFuncTestsᴛ1>([
    new(isSpace, space + " hello " + space,
        "hello",
        "hello " + space,
        space + " hello"),
    new(isDigit, "\u0e50\u0e5212hello34\u0e50\u0e51",
        "hello",
        "hello34\u0e50\u0e51",
        "\u0e50\u0e5212hello"),
    new(isUpper, "\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F",
        "hello",
        "helloEF\u2C6F\u2C6FGH\u2C6F\u2C6F",
        "\u2C6F\u2C6F\u2C6F\u2C6FABCDhello"),
    new(not(isSpace), "hello" + space + "hello",
        space,
        space + "hello",
        "hello" + space),
    new(not(isDigit), "hello\u0e50\u0e521234\u0e50\u0e51helo",
        "\u0e50\u0e521234\u0e50\u0e51",
        "\u0e50\u0e521234\u0e50\u0e51helo",
        "hello\u0e50\u0e521234\u0e50\u0e51"),
    new(isValidRune, ((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})),
        ((@string)(new byte[]{0xc0, 0x61, 0xc0})),
        ((@string)(new byte[]{0xc0, 0x61, 0xc0, 0x63, 0x64})),
        ((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0}))),
    new(not(isValidRune), ((@string)(new byte[]{0xc0, 0x61, 0xc0})),
        "a",
        ((@string)(new byte[]{0x61, 0xc0})),
        ((@string)(new byte[]{0xc0, 0x61}))),
    new(isSpace, "",
        "",
        "",
        ""),
    new(isSpace, " ",
        "",
        "",
        "")
]);

internal partial struct TestTrimFunc_trimmers /*dyn*/ {
    internal @string name;
    internal Func<@string, Func<rune, bool>, @string> trim;
    internal @string @out;
}

public static void TestTrimFunc(ж<testing.T> t) {
    foreach (var (_, tc) in range(trimFuncTests)) {
        var trimmers = slice<TestTrimFunc_trimmers>([
            new("TrimFunc", TrimFunc, tc.trimOut),
            new("TrimLeftFunc", TrimLeftFunc, tc.leftOut),
            new("TrimRightFunc", TrimRightFunc, tc.rightOut)
        ]);
        foreach (var (_, trimmer) in range(trimmers)) {
            @string actual = trimmer.trim(tc.@in, tc.f.f);
            if (actual != trimmer.@out) {
                t.Errorf("%s(%q, %q) = %q; want %q", trimmer.name, tc.@in, tc.f.name, actual, trimmer.@out);
            }
        }
    }
}

partial struct indexFuncTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal predicate f;
    internal nint first, last;
}
internal static slice<indexFuncTestsᴛ1> indexFuncTests = slice<indexFuncTestsᴛ1>([
    new("", isValidRune, -1, -1),
    new("abc", isDigit, -1, -1),
    new("0123", isDigit, 0, 3),
    new("a1b", isDigit, 1, 1),
    new(space, isSpace, 0, len(space) - 3), // last rune in space is 3 bytes
    new("\u0e50\u0e5212hello34\u0e50\u0e51", isDigit, 0, 18),
    new("\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F", isUpper, 0, 34),
    new("12\u0e50\u0e52hello34\u0e50\u0e51", not(isDigit), 8, 12),

    // tests of invalid UTF-8
    new(((@string)(new byte[]{0x80, 0x31})), isDigit, 1, 1),
    new(((@string)(new byte[]{0x80, 0x61, 0x62, 0x63})), isDigit, -1, -1),
    new(((@string)(new byte[]{0xc0, 0x61, 0xc0})), isValidRune, 1, 1),
    new(((@string)(new byte[]{0xc0, 0x61, 0xc0})), not(isValidRune), 0, 2),
    new(((@string)(new byte[]{0xc0, 0xe2, 0x98, 0xba, 0xc0})), not(isValidRune), 0, 4),
    new(((@string)(new byte[]{0xc0, 0xe2, 0x98, 0xba, 0xc0, 0xc0})), not(isValidRune), 0, 5),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})), not(isValidRune), 2, 4),
    new(((@string)(new byte[]{0x61, 0xe0, 0x80, 0x63, 0x64})), not(isValidRune), 1, 2),
    new(((@string)(new byte[]{0x80, 0x80, 0x80, 0x80})), not(isValidRune), 0, 3)
]);

public static void TestIndexFunc(ж<testing.T> t) {
    foreach (var (_, tc) in range(indexFuncTests)) {
        nint first = IndexFunc(tc.@in, tc.f.f);
        if (first != tc.first) {
            t.Errorf("IndexFunc(%q, %s) = %d; want %d", tc.@in, tc.f.name, first, tc.first);
        }
        nint last = LastIndexFunc(tc.@in, tc.f.f);
        if (last != tc.last) {
            t.Errorf("LastIndexFunc(%q, %s) = %d; want %d", tc.@in, tc.f.name, last, tc.last);
        }
    }
}

internal static bool equal(@string m, @string s1, @string s2, ж<testing.T> t) {
    if (s1 == s2) {
        return true;
    }
    var e1 = Split(s1, ""u8);
    var e2 = Split(s2, ""u8);
    foreach (var (i, c1) in range(e1)) {
        if (i >= len(e2)) {
            break;
        }
        var (r1, _) = utf8.DecodeRuneInString(c1);
        var (r2, _) = utf8.DecodeRuneInString(e2[i]);
        if (r1 != r2) {
            t.Errorf("%s diff at %d: U+%04X U+%04X", m, i, r1, r2);
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object runeCountWrongInUpperˢ = (@string)"rune count wrong in upper:"u8;
internal static readonly object runeCountWrongInLowerˢ = (@string)"rune count wrong in lower:"u8;
internal static ref readonly @string toUpperUpperˢ => ref toUpperUpperˢᵇ;
private static readonly @string toUpperUpperˢᵇ = "ToUpper(upper)"u8;
internal static readonly object toUpperUpperConsistencyˢ = (@string)"ToUpper(upper) consistency fail"u8;
internal static ref readonly @string toLowerLowerˢ => ref toLowerLowerˢᵇ;
private static readonly @string toLowerLowerˢᵇ = "ToLower(lower)"u8;
internal static readonly object toLowerLowerConsistencyˢ = (@string)"ToLower(lower) consistency fail"u8;

public static void TestCaseConsistency(ж<testing.T> t) {
    // Make a string of all the runes.
    nint numRunes = (nint)(Δunicode.MaxRune + 1);
    if (testing.Short()) {
        numRunes = 1000;
    }
    var a = new slice<rune>(numRunes);
    foreach (var i in range(len(a))) {
        a[i] = (rune)i;
    }
    @string s = (@string)a;
    // convert the cases.
    @string upper = ToUpper(s);
    @string lower = ToLower(s);

    // Consistency checks
    {
        nint n = utf8.RuneCountInString(upper); if (n != numRunes) {
            t.Error(runeCountWrongInUpperˢ, n);
        }
    }
    {
        nint n = utf8.RuneCountInString(lower); if (n != numRunes) {
            t.Error(runeCountWrongInLowerˢ, n);
        }
    }
    if (!equal(toUpperUpperˢ, ToUpper(upper), upper, t)) {
        t.Error(toUpperUpperConsistencyˢ);
    }
    if (!equal(toLowerLowerˢ, ToLower(lower), lower, t)) {
        t.Error(toLowerLowerConsistencyˢ);
    }
    /*
		  These fail because of non-one-to-oneness of the data, such as multiple
		  upper case 'I' mapping to 'i'.  We comment them out but keep them for
		  interest.
		  For instance: CAPITAL LETTER I WITH DOT ABOVE:
			unicode.ToUpper(unicode.ToLower('\u0130')) != '\u0130'

		if !equal("ToUpper(lower)", ToUpper(lower), upper, t) {
			t.Error("ToUpper(lower) consistency fail");
		}
		if !equal("ToLower(upper)", ToLower(upper), lower, t) {
			t.Error("ToLower(upper) consistency fail");
		}
	*/
}

internal static @string longString = "a"u8 + ((sstring)new slice<byte>((1 << 16))) + "z"u8;
internal static @string longSpaces = func(@string () => {
    var b = new slice<byte>(200);
    foreach (var i in range(len(b))) {
        b[i] = (byte)' ';
    }
    return (@string)b;
})();

partial struct RepeatTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
    internal nint count;
}
public static readonly ж<slice<RepeatTestsᴛ1>> ᏑRepeatTests = new StaticBox<slice<RepeatTestsᴛ1>>(slice<RepeatTestsᴛ1>([
    new("", "", 0),
    new("", "", 1),
    new("", "", 2),
    new("-", "", 0),
    new("-", "-", 1),
    new("-", "----------", 10),
    new("abc ", "abc abc abc ", 3),
    new(" ", " ", 1),
    new("--", "----", 2),
    new("===", "======", 2),
    new("000", "000000000", 3),
    new("\t\t\t\t", "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t", 4),
    new(" ", longSpaces, len(longSpaces)),
    // Tests for results over the chunkLimit
    new((@string)(rune)0, (@string)new slice<byte>((1 << 16)), 1 << 16),
    new(longString, longString + longString, 2)
]));
public static ref slice<RepeatTestsᴛ1> RepeatTests => ref ((StaticBox<slice<RepeatTestsᴛ1>>)ᏑRepeatTests).ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string repeatSˢ => ref repeatSˢᵇ;
private static readonly @string repeatSˢᵇ = "Repeat(s)"u8;

public static void TestRepeat(ж<testing.T> t) {
    foreach (var (_, tt) in range(RepeatTests)) {
        @string a = Repeat(tt.@in, tt.count);
        if (!equal(repeatSˢ, a, tt.@out, t)) {
            t.Errorf("Repeat(%v, %d) = %v; want %v", tt.@in, tt.count, a, tt.@out);
            continue;
        }
    }
}

internal static error /*err*/ repeat(@string s, nint count) {
    error err = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                switch (r.type()) {
                case {} Δv when Δv._<error>(out var v): {
                    err = v;
                    break;
                }
                default: {
                    var v = r;
                    err = fmt.Errorf("%s"u8, v);
                    break;
                }}
            }
        }, ref ᒐ);

        Repeat(s, count);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bitˢ => ref bitˢᵇ;
private static readonly @string bitˢᵇ = "64-bit"u8;

internal partial struct TestRepeatCatchesOverflow_testCase /*dyn*/ {
    internal @string s;
    internal nint count;
    internal @string errStr;
}

// See Issue golang.org/issue/16237
public static void TestRepeatCatchesOverflow(ж<testing.T> t) {
    void runTestCases(@string prefix, slice<TestRepeatCatchesOverflow_testCase> tests) {
        foreach (var (i, tt) in range(tests)) {
            var err = repeat(tt.s, tt.count);
            if (tt.errStr == ""u8) {
                if (err != nil) {
                    t.Errorf("#%d panicked %v", i, err);
                }
                continue;
            }

            if (err == nil || !Contains(err.Error(), tt.errStr)) {
                t.Errorf("%s#%d got %q want %q", prefix, i, err, tt.errStr);
            }
        }
    }

    nint maxInt = /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);

    runTestCases(""u8, new slice<TestRepeatCatchesOverflow_testCase>(7){
        [0] = new("--", -2147483647, "negative"),
        [1] = new("", maxInt, ""),
        [2] = new("-", 10, ""),
        [3] = new("gopher", 0, ""),
        [4] = new("-", -1, "negative"),
        [5] = new("--", -102, "negative"),
        [6] = new((@string)new slice<byte>(255), (nint)((~(nuint)0) / 255 + 1), "overflow")
    });

    const bool is64Bit = /* 1<<(^uintptr(0)>>63)/2 != 0 */ true;
    if (!is64Bit) {
        return;
    }

    runTestCases(bitˢ, new slice<TestRepeatCatchesOverflow_testCase>(1){
        [0] = new("-", maxInt, "out of range")
    });
}

internal static bool runesEqual(slice<rune> a, slice<rune> b) {
    if (len(a) != len(b)) {
        return false;
    }
    foreach (var (i, r) in range(a)) {
        if (r != b[i]) {
            return false;
        }
    }
    return true;
}

partial struct RunesTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal slice<rune> @out;
    internal bool lossy;
}
public static readonly ж<slice<RunesTestsᴛ1>> ᏑRunesTests = new StaticBox<slice<RunesTestsᴛ1>>([
    new("", [], false),
    new(" ", [32], false),
    new("ABC", [65, 66, 67], false),
    new("abc", [97, 98, 99], false),
    new("\u65e5\u672c\u8a9e", [26085, 26412, 35486], false),
    new(((@string)(new byte[]{0x61, 0x62, 0x80, 0x63})), [97, 98, 0xFFFD, 99], true),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x63})), [97, 98, 0xFFFD, 99], true)
]);
public static ref slice<RunesTestsᴛ1> RunesTests => ref ((StaticBox<slice<RunesTestsᴛ1>>)ᏑRunesTests).ValueSlot;

public static void TestRunes(ж<testing.T> t) {
    foreach (var (_, tt) in range(RunesTests)) {
        var a = slice<rune>(tt.@in);
        if (!runesEqual(a, tt.@out)) {
            t.Errorf("[]rune(%q) = %v; want %v", tt.@in, a, tt.@out);
            continue;
        }
        if (!tt.lossy) {
            // can only test reassembly if we didn't lose information
            @string s = (@string)a;
            if (s != tt.@in) {
                t.Errorf("string([]rune(%q)) = %x; want %x", tt.@in, s, tt.@in);
            }
        }
    }
}

public static void TestReadByte(ж<testing.T> t) {
    var testStrings = slice<@string>(["", abcd, faces, commas]);
    foreach (var (_, s) in range(testStrings)) {
        var reader = NewReader(s);
        {
            var e = reader.UnreadByte(); if (e == nil) {
                t.Errorf("Unreading %q at beginning: expected error", s);
            }
        }
        ref var res = ref heap(new bytes.Buffer(), out var Ꮡres);
        for (;;) {
            var (b, e) = reader.ReadByte();
            if (AreEqual(e, Δio.EOF)) {
                break;
            }
            if (e != nil) {
                t.Errorf("Reading %q: %s", s, e);
                break;
            }
            res.WriteByte(b);
            // unread and read again
            e = reader.UnreadByte();
            if (e != nil) {
                t.Errorf("Unreading %q: %s", s, e);
                break;
            }
            (var b1, e) = reader.ReadByte();
            if (e != nil) {
                t.Errorf("Reading %q after unreading: %s", s, e);
                break;
            }
            if (b1 != b) {
                t.Errorf("Reading %q after unreading: want byte %q, got %q", s, b, b1);
                break;
            }
        }
        if (Ꮡres.String() != s) {
            t.Errorf("Reader(%q).ReadByte() produced %q", s, Ꮡres.String());
        }
    }
}

public static void TestReadRune(ж<testing.T> t) {
    var testStrings = slice<@string>(["", abcd, faces, commas]);
    foreach (var (_, s) in range(testStrings)) {
        var reader = NewReader(s);
        {
            var e = reader.UnreadRune(); if (e == nil) {
                t.Errorf("Unreading %q at beginning: expected error", s);
            }
        }
        @string res = ""u8;
        for (;;) {
            var (r, z, e) = reader.ReadRune();
            if (AreEqual(e, Δio.EOF)) {
                break;
            }
            if (e != nil) {
                t.Errorf("Reading %q: %s", s, e);
                break;
            }
            res += ((@string)r);
            // unread and read again
            e = reader.UnreadRune();
            if (e != nil) {
                t.Errorf("Unreading %q: %s", s, e);
                break;
            }
            (var r1, var z1, e) = reader.ReadRune();
            if (e != nil) {
                t.Errorf("Reading %q after unreading: %s", s, e);
                break;
            }
            if (r1 != r) {
                t.Errorf("Reading %q after unreading: want rune %q, got %q", s, r, r1);
                break;
            }
            if (z1 != z) {
                t.Errorf("Reading %q after unreading: want size %d, got %d", s, z, z1);
                break;
            }
        }
        if (res != s) {
            t.Errorf("Reader(%q).ReadRune() produced %q", s, res);
        }
    }
}

partial struct UnreadRuneErrorTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Action<ж<strings.Reader>> f;
}
public static readonly ж<slice<UnreadRuneErrorTestsᴛ1>> ᏑUnreadRuneErrorTests = new StaticBox<slice<UnreadRuneErrorTestsᴛ1>>([
    new("Read", (ж<strings.Reader> r) => {
        r.Read([0]);
    }),
    new("ReadByte", (ж<strings.Reader> r) => {
        r.ReadByte();
    }),
    new("UnreadRune", (ж<strings.Reader> r) => {
        r.UnreadRune();
    }),
    new("Seek", (ж<strings.Reader> r) => {
        r.Seek(0, Δio.SeekCurrent);
    }),
    new("WriteTo", (ж<strings.Reader> r) => {
        r.WriteTo(new strings_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer())));
    })
]);
public static ref slice<UnreadRuneErrorTestsᴛ1> UnreadRuneErrorTests => ref ((StaticBox<slice<UnreadRuneErrorTestsᴛ1>>)ᏑUnreadRuneErrorTests).ValueSlot;

public static void TestUnreadRuneError(ж<testing.T> t) {
    foreach (var (_, tt) in range(UnreadRuneErrorTests)) {
        var reader = NewReader("0123456789");
        var (_, _, errΔ1) = reader.ReadRune(); if (errΔ1 != nil) {
            // should not happen
            t.Fatal(errΔ1);
        }
        tt.f(reader);
        var err = reader.UnreadRune();
        if (err == nil) {
            t.Errorf("Unreading after %s: expected error", tt.name);
        }
    }
}

partial struct ReplaceTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string old, @new;
    internal nint n;
    internal @string @out;
}
public static readonly ж<slice<ReplaceTestsᴛ1>> ᏑReplaceTests = new StaticBox<slice<ReplaceTestsᴛ1>>([
    new("hello", "l", "L", 0, "hello"),
    new("hello", "l", "L", -1, "heLLo"),
    new("hello", "x", "X", -1, "hello"),
    new("", "x", "X", -1, ""),
    new("radar", "r", "<r>", -1, "<r>ada<r>"),
    new("", "", "<>", -1, "<>"),
    new("banana", "a", "<>", -1, "b<>n<>n<>"),
    new("banana", "a", "<>", 1, "b<>nana"),
    new("banana", "a", "<>", 1000, "b<>n<>n<>"),
    new("banana", "an", "<>", -1, "b<><>a"),
    new("banana", "ana", "<>", -1, "b<>na"),
    new("banana", "", "<>", -1, "<>b<>a<>n<>a<>n<>a<>"),
    new("banana", "", "<>", 10, "<>b<>a<>n<>a<>n<>a<>"),
    new("banana", "", "<>", 6, "<>b<>a<>n<>a<>n<>a"),
    new("banana", "", "<>", 5, "<>b<>a<>n<>a<>na"),
    new("banana", "", "<>", 1, "<>banana"),
    new("banana", "a", "a", -1, "banana"),
    new("banana", "a", "a", 1, "banana"),
    new("☺☻☹", "", "<>", -1, "<>☺<>☻<>☹<>")
]);
public static ref slice<ReplaceTestsᴛ1> ReplaceTests => ref ((StaticBox<slice<ReplaceTestsᴛ1>>)ᏑReplaceTests).ValueSlot;

public static void TestReplace(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(ReplaceTests)) {
        ref var tt = ref heap(new ReplaceTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        if (!asan.Enabled) { // See issue #72973.
            var ttʗ1 = tt;
            var allocs = testing.AllocsPerRun(10, () => {
                Replace(ttʗ1.@in, ttʗ1.old, ttʗ1.@new, ttʗ1.n);
            });
            if (allocs > 1D) {
                t.Errorf("Replace(%q, %q, %q, %d) allocates %.2f objects", tt.@in, tt.old, tt.@new, tt.n, allocs);
            }
        }
        {
            @string s = Replace(tt.@in, tt.old, tt.@new, tt.n); if (s != tt.@out) {
                t.Errorf("Replace(%q, %q, %q, %d) = %q, want %q", tt.@in, tt.old, tt.@new, tt.n, s, tt.@out);
            }
        }
        if (tt.n == -1) {
            @string s = ReplaceAll(tt.@in, tt.old, tt.@new);
            if (s != tt.@out) {
                t.Errorf("ReplaceAll(%q, %q, %q) = %q, want %q", tt.@in, tt.old, tt.@new, s, tt.@out);
            }
        }
    }
}

public static void FuzzReplace(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tt) in range(ReplaceTests)) {
        f.Add(tt.@in, tt.old, tt.@new, tt.n);
    }
    Ꮡf.Fuzz((ж<testing.T> t, @string @in, @string old, @string @new, nint n) => {
        @string differentImpl(@string inΔ1, @string oldΔ1, @string newΔ1, nint nΔ1) {
            ref var @out = ref heap(new strings.Builder(), out var Ꮡout);
            if (nΔ1 < 0) {
                nΔ1 = Δmath.MaxInt;
            }
            for (nint i = 0; i < len(inΔ1); ) {
                if (nΔ1 == 0) {
                    Ꮡout.WriteString(inΔ1.slice(i));
                    break;
                }
                if (HasPrefix(inΔ1.slice(i), oldΔ1)) {
                    Ꮡout.WriteString(newΔ1);
                    i += len(oldΔ1);
                    nΔ1--;
                    if (len(oldΔ1) != 0) {
                        continue;
                    }
                    if (i == len(inΔ1)) {
                        break;
                    }
                }
                if (len(oldΔ1) == 0) {
                    var (_, length) = utf8.DecodeRuneInString(inΔ1.slice(i));
                    Ꮡout.WriteString(inΔ1.slice(i, i + length));
                    i += length;
                } else {
                    Ꮡout.WriteByte(inΔ1[i]);
                    i++;
                }
            }
            if (len(oldΔ1) == 0 && nΔ1 != 0) {
                Ꮡout.WriteString(newΔ1);
            }
            return @out.String();
        }
        @string simple = differentImpl(@in, old, @new, n);
        @string replace = Replace(@in, old, @new, n); if (simple != replace) {
            t.Errorf("The two implementations do not match %q != %q for Replace(%q, %q, %q, %d)", simple, replace, @in, old, @new, n);
        }
    });
}

public static void BenchmarkReplace(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(ReplaceTests)) {
        ref var tt = ref heap(new ReplaceTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        @string desc = fmt.Sprintf("%q %q %q %d"u8, tt.@in, tt.old, tt.@new, tt.n);
        var ttʗ1 = tt;
        b.Run(desc, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            while (bΔ1.Loop()) {
                Replace(ttʗ1.@in, ttʗ1.old, ttʗ1.@new, ttʗ1.n);
            }
        });
    }
}

partial struct TitleTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
public static readonly ж<slice<TitleTestsᴛ1>> ᏑTitleTests = new StaticBox<slice<TitleTestsᴛ1>>([
    new("", ""),
    new("a", "A"),
    new(" aaa aaa aaa ", " Aaa Aaa Aaa "),
    new(" Aaa Aaa Aaa ", " Aaa Aaa Aaa "),
    new("123a456", "123a456"),
    new("double-blind", "Double-Blind"),
    new("ÿøû", "Ÿøû"),
    new("with_underscore", "With_underscore"),
    new(((@string)(new byte[]{0x75, 0x6e, 0x69, 0x63, 0x6f, 0x64, 0x65, 0x20, 0xe2, 0x80, 0xa8, 0x20, 0x6c, 0x69, 0x6e, 0x65, 0x20, 0x73, 0x65, 0x70, 0x61, 0x72, 0x61, 0x74, 0x6f, 0x72})), ((@string)(new byte[]{0x55, 0x6e, 0x69, 0x63, 0x6f, 0x64, 0x65, 0x20, 0xe2, 0x80, 0xa8, 0x20, 0x4c, 0x69, 0x6e, 0x65, 0x20, 0x53, 0x65, 0x70, 0x61, 0x72, 0x61, 0x74, 0x6f, 0x72})))
]);
public static ref slice<TitleTestsᴛ1> TitleTests => ref ((StaticBox<slice<TitleTestsᴛ1>>)ᏑTitleTests).ValueSlot;

public static void TestTitle(ж<testing.T> t) {
    foreach (var (_, tt) in range(TitleTests)) {
        @string s = Title(tt.@in); if (s != tt.@out) {
            t.Errorf("Title(%q) = %q, want %q", tt.@in, s, tt.@out);
        }
    }
}

partial struct ContainsTestsᴛ1 /*dyn*/ {
    internal @string str, substr;
    internal bool expected;
}
public static readonly ж<slice<ContainsTestsᴛ1>> ᏑContainsTests = new StaticBox<slice<ContainsTestsᴛ1>>([
    new("abc", "bc", true),
    new("abc", "bcd", false),
    new("abc", "", true),
    new("", "a", false),

    // cases to cover code in runtime/asm_amd64.s:indexShortStr
    // 2-byte needle
    new("xxxxxx", "01", false),
    new("01xxxx", "01", true),
    new("xx01xx", "01", true),
    new("xxxx01", "01", true),
    new("01xxxxx"u8[1..], "01", false),
    new("xxxxx01"u8[..6], "01", false),
    // 3-byte needle
    new("xxxxxxx", "012", false),
    new("012xxxx", "012", true),
    new("xx012xx", "012", true),
    new("xxxx012", "012", true),
    new("012xxxxx"u8[1..], "012", false),
    new("xxxxx012"u8[..7], "012", false),
    // 4-byte needle
    new("xxxxxxxx", "0123", false),
    new("0123xxxx", "0123", true),
    new("xx0123xx", "0123", true),
    new("xxxx0123", "0123", true),
    new("0123xxxxx"u8[1..], "0123", false),
    new("xxxxx0123"u8[..8], "0123", false),
    // 5-7-byte needle
    new("xxxxxxxxx", "01234", false),
    new("01234xxxx", "01234", true),
    new("xx01234xx", "01234", true),
    new("xxxx01234", "01234", true),
    new("01234xxxxx"u8[1..], "01234", false),
    new("xxxxx01234"u8[..9], "01234", false),
    // 8-byte needle
    new("xxxxxxxxxxxx", "01234567", false),
    new("01234567xxxx", "01234567", true),
    new("xx01234567xx", "01234567", true),
    new("xxxx01234567", "01234567", true),
    new("01234567xxxxx"u8[1..], "01234567", false),
    new("xxxxx01234567"u8[..12], "01234567", false),
    // 9-15-byte needle
    new("xxxxxxxxxxxxx", "012345678", false),
    new("012345678xxxx", "012345678", true),
    new("xx012345678xx", "012345678", true),
    new("xxxx012345678", "012345678", true),
    new("012345678xxxxx"u8[1..], "012345678", false),
    new("xxxxx012345678"u8[..13], "012345678", false),
    // 16-byte needle
    new("xxxxxxxxxxxxxxxxxxxx", "0123456789ABCDEF", false),
    new("0123456789ABCDEFxxxx", "0123456789ABCDEF", true),
    new("xx0123456789ABCDEFxx", "0123456789ABCDEF", true),
    new("xxxx0123456789ABCDEF", "0123456789ABCDEF", true),
    new("0123456789ABCDEFxxxxx"u8[1..], "0123456789ABCDEF", false),
    new("xxxxx0123456789ABCDEF"u8[..20], "0123456789ABCDEF", false),
    // 17-31-byte needle
    new("xxxxxxxxxxxxxxxxxxxxx", "0123456789ABCDEFG", false),
    new("0123456789ABCDEFGxxxx", "0123456789ABCDEFG", true),
    new("xx0123456789ABCDEFGxx", "0123456789ABCDEFG", true),
    new("xxxx0123456789ABCDEFG", "0123456789ABCDEFG", true),
    new("0123456789ABCDEFGxxxxx"u8[1..], "0123456789ABCDEFG", false),
    new("xxxxx0123456789ABCDEFG"u8[..21], "0123456789ABCDEFG", false),

    // partial match cases
    new("xx01x", "012", false),                            // 3
    new("xx0123x", "01234", false),                        // 5-7
    new("xx01234567x", "012345678", false),                // 9-15
    new("xx0123456789ABCDEFx", "0123456789ABCDEFG", false) // 17-31, issue 15679
]);
public static ref slice<ContainsTestsᴛ1> ContainsTests => ref ((StaticBox<slice<ContainsTestsᴛ1>>)ᏑContainsTests).ValueSlot;

public static void TestContains(ж<testing.T> t) {
    foreach (var (_, ct) in range(ContainsTests)) {
        if (Contains(ct.str, ct.substr) != ct.expected) {
            t.Errorf("Contains(%s, %s) = %v, want %v",
                ct.str, ct.substr, !ct.expected, ct.expected);
        }
    }
}

public static readonly ж<slice<ContainsTestsᴛ1>> ᏑContainsAnyTests = new StaticBox<slice<ContainsTestsᴛ1>>([
    new("", "", false),
    new("", "a", false),
    new("", "abc", false),
    new("a", "", false),
    new("a", "a", true),
    new("aaa", "a", true),
    new("abc", "xyz", false),
    new("abc", "xcz", true),
    new("a☺b☻c☹d", "uvw☻xyz", true),
    new("aRegExp*", ".(|)*+?^$[]", true),
    new(dots + dots + dots, " ", false)
]);
public static ref slice<ContainsTestsᴛ1> ContainsAnyTests => ref ((StaticBox<slice<ContainsTestsᴛ1>>)ᏑContainsAnyTests).ValueSlot;

public static void TestContainsAny(ж<testing.T> t) {
    foreach (var (_, ct) in range(ContainsAnyTests)) {
        if (ContainsAny(ct.str, ct.substr) != ct.expected) {
            t.Errorf("ContainsAny(%s, %s) = %v, want %v",
                ct.str, ct.substr, !ct.expected, ct.expected);
        }
    }
}

partial struct ContainsRuneTestsᴛ1 /*dyn*/ {
    internal @string str;
    internal rune r;
    internal bool expected;
}
public static readonly ж<slice<ContainsRuneTestsᴛ1>> ᏑContainsRuneTests = new StaticBox<slice<ContainsRuneTestsᴛ1>>([
    new("", 'a', false),
    new("a", 'a', true),
    new("aaa", 'a', true),
    new("abc", 'y', false),
    new("abc", 'c', true),
    new("a☺b☻c☹d", 'x', false),
    new("a☺b☻c☹d", '☻', true),
    new("aRegExp*", '*', true)
]);
public static ref slice<ContainsRuneTestsᴛ1> ContainsRuneTests => ref ((StaticBox<slice<ContainsRuneTestsᴛ1>>)ᏑContainsRuneTests).ValueSlot;

public static void TestContainsRune(ж<testing.T> t) {
    foreach (var (_, ct) in range(ContainsRuneTests)) {
        if (ContainsRune(ct.str, ct.r) != ct.expected) {
            t.Errorf("ContainsRune(%q, %q) = %v, want %v",
                ct.str, ct.r, !ct.expected, ct.expected);
        }
    }
}

public static void TestContainsFunc(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(ContainsRuneTests)) {
        ref var ct = ref heap(new ContainsRuneTestsᴛ1(), out var Ꮡct);
        ct = vᴛ1;

        var ctʗ1 = ct;
        if (ContainsFunc(ct.str, (rune r) => ctʗ1.r == r) != ct.expected) {
            t.Errorf("ContainsFunc(%q, func(%q)) = %v, want %v",
                ct.str, ct.r, !ct.expected, ct.expected);
        }
    }
}

partial struct EqualFoldTestsᴛ1 /*dyn*/ {
    internal @string s, t;
    internal bool @out;
}
public static readonly ж<slice<EqualFoldTestsᴛ1>> ᏑEqualFoldTests = new StaticBox<slice<EqualFoldTestsᴛ1>>([
    new("abc", "abc", true),
    new("ABcd", "ABcd", true),
    new("123abc", "123ABC", true),
    new("αβδ", "ΑΒΔ", true),
    new("abc", "xyz", false),
    new("abc", "XYZ", false),
    new("abcdefghijk", "abcdefghijX", false),
    new("abcdefghijk", "abcdefghij\u212A", true),
    new("abcdefghijK", "abcdefghij\u212A", true),
    new("abcdefghijkz", "abcdefghij\u212Ay", false),
    new("abcdefghijKz", "abcdefghij\u212Ay", false),
    new("1", "2", false),
    new("utf-8", "US-ASCII", false)
]);
public static ref slice<EqualFoldTestsᴛ1> EqualFoldTests => ref ((StaticBox<slice<EqualFoldTestsᴛ1>>)ᏑEqualFoldTests).ValueSlot;

public static void TestEqualFold(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in range(EqualFoldTests)) {
        {
            var @out = EqualFold(tt.s, tt.t); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v", tt.s, tt.t, @out, tt.@out);
            }
        }
        {
            var @out = EqualFold(tt.t, tt.s); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v", tt.t, tt.s, @out, tt.@out);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testsˢ => ref testsˢᵇ;
private static readonly @string testsˢᵇ = "Tests"u8;
internal static readonly object wrongResultˢ = (@string)"wrong result"u8;
internal static ref readonly @string asciiˢ => ref asciiˢᵇ;
private static readonly @string asciiˢᵇ = "ASCII"u8;
internal static ref readonly @string unicodePrefixˢ => ref unicodePrefixˢᵇ;
private static readonly @string unicodePrefixˢᵇ = "UnicodePrefix"u8;
internal static ref readonly @string unicodeSuffixˢ => ref unicodeSuffixˢᵇ;
private static readonly @string unicodeSuffixˢᵇ = "UnicodeSuffix"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string s1ᶜ => ref s1ᶜᵇ;
private static readonly @string s1ᶜᵇ = "abcdefghijKz"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string s2ᶜ => ref s2ᶜᵇ;
private static readonly @string s2ᶜᵇ = "abcDefGhijKz"u8;

public static void BenchmarkEqualFold(ж<testing.B> b) {
    b.Run(testsˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            foreach (var (_, tt) in range(EqualFoldTests)) {
                var @out = EqualFold(tt.s, tt.t); if (@out != tt.@out) {
                    bΔ1.Fatal(wrongResultˢ);
                }
            }
        }
    });

    @string s1 = s1ᶜ;
    @string s2 = s2ᶜ;

    b.Run(asciiˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            EqualFold(s1, s2);
        }
    });

    b.Run(unicodePrefixˢ, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            EqualFold("αβδ" + s1, "ΑΒΔ" + s2);
        }
    });

    b.Run(unicodeSuffixˢ, (ж<testing.B> bΔ4) => {
        for (nint i = 0; i < (~bΔ4).N; i++) {
            EqualFold(s1 + "αβδ", s2 + "ΑΒΔ");
        }
    });
}

partial struct CountTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal nint num;
}
public static readonly ж<slice<CountTestsᴛ1>> ᏑCountTests = new StaticBox<slice<CountTestsᴛ1>>([
    new("", "", 1),
    new("", "notempty", 0),
    new("notempty", "", 9),
    new("smaller", "not smaller", 0),
    new("12345678987654321", "6", 2),
    new("611161116", "6", 3),
    new("notequal", "NotEqual", 0),
    new("equal", "equal", 1),
    new("abc1231231123q", "123", 3),
    new("11111", "11", 2)
]);
public static ref slice<CountTestsᴛ1> CountTests => ref ((StaticBox<slice<CountTestsᴛ1>>)ᏑCountTests).ValueSlot;

public static void TestCount(ж<testing.T> t) {
    foreach (var (_, tt) in range(CountTests)) {
        nint num = Count(tt.s, tt.sep); if (num != tt.num) {
            t.Errorf("Count(%q, %q) = %d, want %d", tt.s, tt.sep, num, tt.num);
        }
    }
}

partial struct cutTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal @string before, after;
    internal bool found;
}
internal static slice<cutTestsᴛ1> cutTests = [
    new("abc", "b", "a", "c", true),
    new("abc", "a", "", "bc", true),
    new("abc", "c", "ab", "", true),
    new("abc", "abc", "", "", true),
    new("abc", "", "", "abc", true),
    new("abc", "d", "abc", "", false),
    new("", "d", "", "", false),
    new("", "", "", "", true)
];

public static void TestCut(ж<testing.T> t) {
    foreach (var (_, tt) in range(cutTests)) {
        var (before, after, found) = Cut(tt.s, tt.sep); if (before != tt.before || after != tt.after || found != tt.found) {
            t.Errorf("Cut(%q, %q) = %q, %q, %v, want %q, %q, %v", tt.s, tt.sep, before, after, found, tt.before, tt.after, tt.found);
        }
    }
}

partial struct cutPrefixTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal @string after;
    internal bool found;
}
internal static slice<cutPrefixTestsᴛ1> cutPrefixTests = [
    new("abc", "a", "bc", true),
    new("abc", "abc", "", true),
    new("abc", "", "abc", true),
    new("abc", "d", "abc", false),
    new("", "d", "", false),
    new("", "", "", true)
];

public static void TestCutPrefix(ж<testing.T> t) {
    foreach (var (_, tt) in range(cutPrefixTests)) {
        var (after, found) = CutPrefix(tt.s, tt.sep); if (after != tt.after || found != tt.found) {
            t.Errorf("CutPrefix(%q, %q) = %q, %v, want %q, %v", tt.s, tt.sep, after, found, tt.after, tt.found);
        }
    }
}

partial struct cutSuffixTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal @string before;
    internal bool found;
}
internal static slice<cutSuffixTestsᴛ1> cutSuffixTests = [
    new("abc", "bc", "a", true),
    new("abc", "abc", "", true),
    new("abc", "", "abc", true),
    new("abc", "d", "abc", false),
    new("", "d", "", false),
    new("", "", "", true)
];

public static void TestCutSuffix(ж<testing.T> t) {
    foreach (var (_, tt) in range(cutSuffixTests)) {
        var (before, found) = CutSuffix(tt.s, tt.sep); if (before != tt.before || found != tt.found) {
            t.Errorf("CutSuffix(%q, %q) = %q, %v, want %q, %v", tt.s, tt.sep, before, found, tt.before, tt.found);
        }
    }
}

internal static @string makeBenchInputHard() {
    var tokens = new @string[]{
        "<a>", "<p>", "<b>", "<strong>",
        "</a>", "</p>", "</b>", "</strong>",
        "hello", "world"
    }.array();
    var x = new slice<byte>(0, (1 << 20));
    var r = rand.New(rand.NewSource(99));
    for (;;) {
        nint i = r.Intn(len(tokens));
        if (len(x) + len(tokens[i]) >= 1 << 20) {
            break;
        }
        x = append(x, tokens[i].ꓸꓸꓸ);
    }
    return (@string)x;
}

internal static @string benchInputHard = makeBenchInputHard();

internal static void benchmarkIndexHard(ж<testing.B> Ꮡb, @string sep) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Index(benchInputHard, sep);
    }
}

internal static void benchmarkLastIndexHard(ж<testing.B> Ꮡb, @string sep) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        LastIndex(benchInputHard, sep);
    }
}

internal static void benchmarkCountHard(ж<testing.B> Ꮡb, @string sep) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Count(benchInputHard, sep);
    }
}

public static void BenchmarkIndexHard1(ж<testing.B> b) { benchmarkIndexHard(b, "<>"); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string preˢ => ref preˢᵇ;
private static readonly @string preˢᵇ = "</pre>"u8;

public static void BenchmarkIndexHard2(ж<testing.B> b) { benchmarkIndexHard(b, preˢ); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bHelloWorldBˢ => ref bHelloWorldBˢᵇ;
private static readonly @string bHelloWorldBˢᵇ = "<b>hello world</b>"u8;

public static void BenchmarkIndexHard3(ж<testing.B> b) { benchmarkIndexHard(b, bHelloWorldBˢ); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string preBHelloBStrongWorldˢ => ref preBHelloBStrongWorldˢᵇ;
private static readonly @string preBHelloBStrongWorldˢᵇ = "<pre><b>hello</b><strong>world</strong></pre>"u8;

public static void BenchmarkIndexHard4(ж<testing.B> b) {
    benchmarkIndexHard(b, preBHelloBStrongWorldˢ);
}

public static void BenchmarkLastIndexHard1(ж<testing.B> b) { benchmarkLastIndexHard(b, "<>"); }
public static void BenchmarkLastIndexHard2(ж<testing.B> b) { benchmarkLastIndexHard(b, preˢ); }
public static void BenchmarkLastIndexHard3(ж<testing.B> b) { benchmarkLastIndexHard(b, bHelloWorldBˢ); }

public static void BenchmarkCountHard1(ж<testing.B> b) { benchmarkCountHard(b, "<>"); }
public static void BenchmarkCountHard2(ж<testing.B> b) { benchmarkCountHard(b, preˢ); }
public static void BenchmarkCountHard3(ж<testing.B> b) { benchmarkCountHard(b, bHelloWorldBˢ); }

internal static @string benchInputTorture = Repeat("ABC", 1 << 10) + "123"u8 + Repeat("ABC", 1 << 10);
internal static @string benchNeedleTorture = Repeat("ABC", (1 << 10) + 1);

public static void BenchmarkIndexTorture(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Index(benchInputTorture, benchNeedleTorture);
    }
}

public static void BenchmarkCountTorture(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Count(benchInputTorture, benchNeedleTorture);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ2 => ref abcˢ2ᵇ;
private static readonly @string abcˢ2ᵇ = "ABC"u8;

public static void BenchmarkCountTortureOverlapping(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string A = Repeat(abcˢ2, 1 << 20);
    @string B = Repeat(abcˢ2, 1 << 10);
    for (nint i = 0; i < b.N; i++) {
        Count(A, B);
    }
}

public static void BenchmarkCountByte(ж<testing.B> b) {
    var indexSizes = slice<nint>([10, 32, 4 << 10, 4 << 20, 64 << 20]);
    @string benchStr = Repeat(benchmarkString,
        (indexSizes[len(indexSizes) - 1] + len(benchmarkString) - 1) / len(benchmarkString));
    void benchFunc(ж<testing.B> bΔ1, @string benchStrΔ1) {
        bΔ1.SetBytes((int64)len(benchStrΔ1));
        for (nint i = 0; i < (~bΔ1).N; i++) {
            Count(benchStrΔ1, "=");
        }
    }
    foreach (var (_, size) in range(indexSizes)) {
        var benchFuncʗ1 = benchFunc;
        b.Run(fmt.Sprintf("%d"u8, size), (ж<testing.B> bΔ2) => {
            benchFuncʗ1(bΔ2, benchStr.slice(0, size));
        });
    }
}

internal static Func<@string> makeFieldsInput = () => {
    var x = new slice<byte>((1 << 20));
    // Input is ~10% space, ~10% 2-byte UTF-8, rest ASCII non-space.
    var r = rand.New(rand.NewSource(99));
    foreach (var i in range(len(x))) {
        var exprᴛ1 = r.Intn(10);
        var matchᴛ1 = false;
        if (exprᴛ1 is 0) { matchᴛ1 = true;
            x[i] = (byte)' ';
        }
        else if (exprᴛ1 is 1) { matchᴛ1 = true;
            do {
                if (i > 0 && x[i - 1] == 'x') {
                    copy(x.slice(i - 1), "χ"u8);
                    break;
                }
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            x[i] = (byte)'x';
        }
    }
    return (@string)x;
};

internal static Func<@string> makeFieldsInputASCII = () => {
    var x = new slice<byte>((1 << 20));
    // Input is ~10% space, rest ASCII non-space.
    var r = rand.New(rand.NewSource(99));
    foreach (var i in range(len(x))) {
        if (r.Intn(10) == 0) {
            x[i] = (byte)' ';
        } else {
            x[i] = (byte)'x';
        }
    }
    return (@string)x;
};

internal static slice<mapdataᴛ1> stringdata = slice<mapdataᴛ1>([
    new("ASCII", makeFieldsInputASCII()),
    new("Mixed", makeFieldsInput())
]);

public static void BenchmarkFields(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(stringdata)) {
        ref var sd = ref heap(new mapdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        b.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = 1 << 4; jᴛ1 <= 1 << 20; jᴛ1 <<= 4) {
                var j = jᴛ1;
                var sdʗ2 = sdʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, j), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    bΔ2.SetBytes((int64)j);
                    @string data = sdʗ2.data.slice(0, j);
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        Fields(data);
                    }
                });
            }
        });
    }
}

public static void BenchmarkFieldsFunc(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(stringdata)) {
        ref var sd = ref heap(new mapdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        b.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = 1 << 4; jᴛ1 <= 1 << 20; jᴛ1 <<= 4) {
                var j = jᴛ1;
                var sdʗ2 = sdʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, j), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    bΔ2.SetBytes((int64)j);
                    @string data = sdʗ2.data.slice(0, j);
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        FieldsFunc(data, Δunicode.IsSpace);
                    }
                });
            }
        });
    }
}

public static void BenchmarkSplitEmptySeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, ""u8);
    }
}

public static void BenchmarkSplitSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, "/");
    }
}

public static void BenchmarkSplitMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, helloˢ);
    }
}

public static void BenchmarkSplitNSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, "/", 10);
    }
}

public static void BenchmarkSplitNMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, helloˢ, 10);
    }
}

public static void BenchmarkRepeat(ж<testing.B> b) {
    @string s = "0123456789";
    foreach (var (_, n) in range(slice<nint>([5, 10]))) {
        foreach (var (_, c) in range(slice<nint>([0, 1, 2, 6]))) {
            b.Run(fmt.Sprintf("%dx%d"u8, n, c), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Repeat(s.slice(0, n), c);
                }
            });
        }
    }
}

public static void BenchmarkRepeatLarge(ж<testing.B> b) {
    @string s = Repeat("@", 8 * 1024);
    for (nint j = 8; j <= 30; j++) {
        foreach (var (_, k) in range(slice<nint>([1, 16, 4097]))) {
            @string sΔ1 = s.slice(0, k);
            nint n = quo((((nint)1).Lsh((int64)(j))), k);
            if (n == 0) {
                continue;
            }
            b.Run(fmt.Sprintf("%d/%d"u8, ((nint)1).Lsh((int64)(j)), k), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Repeat(sΔ1, n);
                }
                bΔ1.SetBytes((int64)(n * len(sΔ1)));
            });
        }
    }
}

public static void BenchmarkRepeatSpaces(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        Repeat(" ", 2);
    }
}

public static void BenchmarkIndexAnyASCII(ж<testing.B> b) {
    @string x = Repeat("#", 2048); // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz";
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldHelloWorldˢ => ref helloWorldHelloWorldˢᵇ;
private static readonly @string helloWorldHelloWorldˢᵇ = "你好世界, hello world. 你好世界, hello world. 你好世界, hello world."u8;

public static void BenchmarkIndexAnyUTF8(ж<testing.B> b) {
    @string x = Repeat("#", 2048); // Never matches set
    @string cs = helloWorldHelloWorldˢ;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyASCII(ж<testing.B> b) {
    @string x = Repeat("#", 2048); // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz";
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyUTF8(ж<testing.B> b) {
    @string x = Repeat("#", 2048); // Never matches set
    @string cs = helloWorldHelloWorldˢ;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkTrimASCII(ж<testing.B> b) {
    @string cs = "0123456789abcdef";
    for (nint kᴛ1 = 1; kᴛ1 <= 4096; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 16; jᴛ1 <<= 1) {
            var j = jᴛ1;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                @string x = Repeat(cs.slice(0, j), k); // Always matches set
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Trim(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string theQuickBrownFoxˢ => ref theQuickBrownFoxˢᵇ;
private static readonly @string theQuickBrownFoxˢᵇ = "  the quick brown fox   "u8;

public static void BenchmarkTrimByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = theQuickBrownFoxˢ;
    for (nint i = 0; i < b.N; i++) {
        Trim(x, " ");
    }
}

public static void BenchmarkIndexPeriodic(ж<testing.B> b) {
    @string key = "aa";
    foreach (var (_, skip) in range(new nint[]{2, 4, 8, 16, 32, 64}.array())) {
        b.Run(fmt.Sprintf("IndexPeriodic%d"u8, skip), (ж<testing.B> bΔ1) => {
            @string s = Repeat("a"u8 + Repeat(" ", skip - 1), (1 << 16) / skip);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Index(s, key);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string andˢ => ref andˢᵇ;
private static readonly @string andˢᵇ = " and "u8;

public static void BenchmarkJoin(ж<testing.B> b) {
    var vals = slice<@string>(["red", "yellow", "pink", "green", "purple", "orange", "blue"]);
    for (nint lᴛ1 = 0; lᴛ1 <= len(vals); lᴛ1++) {
        var l = lᴛ1;
        var valsʗ1 = vals;
        b.Run(strconv.Itoa(l), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            var valsΔ1 = valsʗ1.slice(0, l);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Join(valsΔ1, andˢ);
            }
        });
    }
}

internal partial struct BenchmarkTrimSpace_tests /*dyn*/ {
    internal @string name, input;
}

public static void BenchmarkTrimSpace(ж<testing.B> b) {
    var tests = slice<BenchmarkTrimSpace_tests>([
        new("NoTrim", "typical"),
        new("ASCII", "  foo bar  "),
        new("SomeNonASCII", "    \u2000\t\r\n x\t\t\r\r\ny\n \u3000    "),
        new("JustNonASCII", "\u2000\u2000\u2000☺☺☺☺\u3000\u3000\u3000")
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new BenchmarkTrimSpace_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        b.Run(test.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                TrimSpace(testʗ1.input);
            }
        });
    }
}

internal static @string stringSink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bananaˢ => ref bananaˢᵇ;
private static readonly @string bananaˢᵇ = "banana"u8;

public static void BenchmarkReplaceAll(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        stringSink = ReplaceAll(bananaˢ, "a", "<>");
    }
}

} // end strings_test_package
