// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bytes_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using testenv = @internal.testenv_package;
using iter = iter_package;
using Δmath = math_package;
using rand = go.math.rand_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using bytes = bytes_package;
using go.math;
using go.unicode;
using static go.bytes_internal_test_package;

partial class bytes_test_package {

internal static slice<@string> sliceOfString(slice<slice<byte>> s) {
    var result = new slice<@string>(len(s));
    foreach (var (i, v) in range(s)) {
        result[i] = (@string)v;
    }
    return result;
}

internal static slice<slice<byte>> collect(ж<testing.T> t, iter.Seq<slice<byte>> seq) {
    var @out = slices.Collect(seq);
    var out1 = slices.Collect(seq);
    if (!slices.Equal<slice<@string>, @string>(sliceOfString(@out), sliceOfString(out1))) {
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
        var result = sliceOfString(slices.Collect(Lines(slice<byte>(s.a))));
        if (!slices.Equal<slice<@string>, @string>(result, s.b)) {
            t.Errorf(@"slices.Collect(Lines(%q)) = %q; want %q", s.a, result, s.b);
        }
    }
}

// For ease of reading, the test cases use strings that are converted to byte
// slices before invoking the functions.
internal static @string abcd = "abcd";
internal static @string faces = "☺☻☹";
internal static @string commas = "1,2,3,4";
internal static @string dots = "1....2....3....4";

partial struct BinOpTest {
    internal @string a;
    internal @string b;
    internal nint i;
}

public static void TestEqual(ж<testing.T> t) {
    // Run the tests and check for allocation at the same time.
    var allocs = testing.AllocsPerRun(10, () => {
        foreach (var (_, tt) in range(compareTests)) {
            var eql = Equal(tt.a, tt.b);
            if (eql != (tt.i == 0)) {
                t.Errorf(@"Equal(%q, %q) = %v", tt.a, tt.b, eql);
            }
        }
    });
    if (allocs > 0D) {
        t.Errorf("Equal allocated %v times", allocs);
    }
}

public static void TestEqualExhaustive(ж<testing.T> t) {
    nint size = 128;
    if (testing.Short()) {
        size = 32;
    }
    var a = new slice<byte>(size);
    var b = new slice<byte>(size);
    var b_init = new slice<byte>(size);
    // randomish but deterministic data
    for (nint i = 0; i < size; i++) {
        a[i] = (byte)(17 * i);
        b_init[i] = (byte)(23 * i + 100);
    }

    for (nint len = 0; len <= size; len++) {
        for (nint x = 0; x <= size - len; x++) {
            for (nint y = 0; y <= size - len; y++) {
                copy(b, b_init);
                copy(b.slice(y, y + len), a.slice(x, x + len));
                if (!Equal(a.slice(x, x + len), b.slice(y, y + len)) || !Equal(b.slice(y, y + len), a.slice(x, x + len))) {
                    t.Errorf("Equal(%d, %d, %d) = false", len, x, y);
                }
            }
        }
    }
}

// make sure Equal returns false for minimally different strings. The data
// is all zeros except for a single one in one location.
public static void TestNotEqual(ж<testing.T> t) {
    nint size = 128;
    if (testing.Short()) {
        size = 32;
    }
    var a = new slice<byte>(size);
    var b = new slice<byte>(size);

    for (nint len = 0; len <= size; len++) {
        for (nint x = 0; x <= size - len; x++) {
            for (nint y = 0; y <= size - len; y++) {
                for (nint diffpos = x; diffpos < x + len; diffpos++) {
                    a[diffpos] = 1;
                    if (Equal(a.slice(x, x + len), b.slice(y, y + len)) || Equal(b.slice(y, y + len), a.slice(x, x + len))) {
                        t.Errorf("NotEqual(%d, %d, %d, %d) = true", len, x, y, diffpos);
                    }
                    a[diffpos] = 0;
                }
            }
        }
    }
}

internal static slice<BinOpTest> indexTests = [
    new("", "", 0),
    new("", "a", -1),
    new("", "foo", -1),
    new("fo", "foo", -1),
    new("foo", "baz", -1),
    new("foo", "foo", 0),
    new("oofofoofooo", "f", 2),
    new("oofofoofooo", "foo", 4),
    new("barfoobarfoo", "foo", 3),
    new("foo", "", 0),
    new("foo", "o", 1),
    new("abcABCabc", "A", 3),
    // cases with one byte strings - test IndexByte and special case in Index()
    new("", "a", -1),
    new("x", "a", -1),
    new("x", "x", 0),
    new("abc", "a", 0),
    new("abc", "b", 1),
    new("abc", "c", 2),
    new("abc", "x", -1),
    new("barfoobarfooyyyzzzyyyzzzyyyzzzyyyxxxzzzyyy", "x", 33),
    new("fofofofooofoboo", "oo", 7),
    new("fofofofofofoboo", "ob", 11),
    new("fofofofofofoboo", "boo", 12),
    new("fofofofofofoboo", "oboo", 11),
    new("fofofofofoooboo", "fooo", 8),
    new("fofofofofofoboo", "foboo", 10),
    new("fofofofofofoboo", "fofob", 8),
    new("fofofofofofofoffofoobarfoo", "foffof", 12),
    new("fofofofofoofofoffofoobarfoo", "foffof", 13),
    new("fofofofofofofoffofoobarfoo", "foffofo", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofo", 13),
    new("fofofofofoofofoffofoobarfoo", "foffofoo", 13),
    new("fofofofofofofoffofoobarfoo", "foffofoo", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofoob", 13),
    new("fofofofofofofoffofoobarfoo", "foffofoob", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofooba", 13),
    new("fofofofofofofoffofoobarfoo", "foffofooba", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofoobar", 13),
    new("fofofofofofofoffofoobarfoo", "foffofoobar", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofoobarf", 13),
    new("fofofofofofofoffofoobarfoo", "foffofoobarf", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofoobarfo", 13),
    new("fofofofofofofoffofoobarfoo", "foffofoobarfo", 12),
    new("fofofofofoofofoffofoobarfoo", "foffofoobarfoo", 13),
    new("fofofofofofofoffofoobarfoo", "foffofoobarfoo", 12),
    new("fofofofofoofofoffofoobarfoo", "ofoffofoobarfoo", 12),
    new("fofofofofofofoffofoobarfoo", "ofoffofoobarfoo", 11),
    new("fofofofofoofofoffofoobarfoo", "fofoffofoobarfoo", 11),
    new("fofofofofofofoffofoobarfoo", "fofoffofoobarfoo", 10),
    new("fofofofofoofofoffofoobarfoo", "foobars", -1),
    new("foofyfoobarfoobar", "y", 4),
    new("oooooooooooooooooooooo", "r", -1),
    new("oxoxoxoxoxoxoxoxoxoxoxoy", "oy", 22),
    new("oxoxoxoxoxoxoxoxoxoxoxox", "oy", -1),
    // test fallback to Rabin-Karp.
    new("000000000000000000000000000000000000000000000000000000000000000000000001", "0000000000000000000000000000000000000000000000000000000000000000001", 5),
    // test fallback to IndexRune
    new("oxoxoxoxoxoxoxoxoxoxox☺", "☺", 22),
    // invalid UTF-8 byte sequence (must be longer than bytealg.MaxBruteForce to
    // test that we don't use IndexRune)
    new(((@string)(new byte[]{0x78, 0x78, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x78, 0x78, 0x78, 0xed, 0x9f, 0xc0})), ((@string)(new byte[]{0xed, 0x9f, 0xc0})), 105)
];

internal static slice<BinOpTest> lastIndexTests = [
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

internal static slice<BinOpTest> indexAnyTests = slice<BinOpTest>([
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

internal static slice<BinOpTest> lastIndexAnyTests = slice<BinOpTest>([
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

internal partial struct runIndexTests_type /*dyn*/ {
    internal slice<byte> a;
    internal slice<byte> b;
    internal nint i;
}

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runIndexTests(ж<testing.T> t, Func<slice<byte>, slice<byte>, nint> f, @string funcName, slice<BinOpTest> testCases) {
    foreach (var (_, test) in range(testCases)) {
        var a = slice<byte>(test.a);
        var b = slice<byte>(test.b);
        nint actual = f(a, b);
        if (actual != test.i) {
            t.Errorf("%s(%q,%q) = %v; want %v", funcName, a, b, actual, test.i);
        }
    }
    slice<runIndexTests_type> allocTests = slice<runIndexTests_type>([
        // case for function Index.
        new(slice<byte>("000000000000000000000000000000000000000000000000000000000000000000000001"u8), slice<byte>("0000000000000000000000000000000000000000000000000000000000000000001"u8), 5),
        // case for function LastIndex.
        new(slice<byte>("000000000000000000000000000000000000000000000000000000000000000010000"u8), slice<byte>("00000000000000000000000000000000000000000000000000000000000001"u8), 3)
    ]);
    var allocTestsʗ1 = allocTests;
    var allocs = testing.AllocsPerRun(100, () => {
        {
            nint i = Index(allocTestsʗ1[1].a, allocTestsʗ1[1].b); if (i != allocTestsʗ1[1].i) {
                t.Errorf("Index([]byte(%q), []byte(%q)) = %v; want %v", allocTestsʗ1[1].a, allocTestsʗ1[1].b, i, allocTestsʗ1[1].i);
            }
        }
        {
            nint i = LastIndex(allocTestsʗ1[0].a, allocTestsʗ1[0].b); if (i != allocTestsʗ1[0].i) {
                t.Errorf("LastIndex([]byte(%q), []byte(%q)) = %v; want %v", allocTestsʗ1[0].a, allocTestsʗ1[0].b, i, allocTestsʗ1[0].i);
            }
        }
    });
    if (allocs != 0D) {
        t.Errorf("expected no allocations, got %f", allocs);
    }
}

internal static void runIndexAnyTests(ж<testing.T> t, Func<slice<byte>, @string, nint> f, @string funcName, slice<BinOpTest> testCases) {
    foreach (var (_, test) in range(testCases)) {
        var a = slice<byte>(test.a);
        nint actual = f(a, test.b);
        if (actual != test.i) {
            t.Errorf("%s(%q,%q) = %v; want %v", funcName, a, test.b, actual, test.i);
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

public static void TestIndexAny(ж<testing.T> t) { runIndexAnyTests(t, IndexAny, indexAnyˢ, indexAnyTests); }
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lastIndexAnyˢ => ref lastIndexAnyˢᵇ;
private static readonly @string lastIndexAnyˢᵇ = "LastIndexAny"u8;

public static void TestLastIndexAny(ж<testing.T> t) {
    runIndexAnyTests(t, LastIndexAny, lastIndexAnyˢ, lastIndexAnyTests);
}

public static void TestIndexByte(ж<testing.T> t) {
    foreach (var (_, tt) in range(indexTests)) {
        if (len(tt.b) != 1) {
            continue;
        }
        var a = slice<byte>(tt.a);
        var b = tt.b[0];
        nint pos = IndexByte(a, b);
        if (pos != tt.i) {
            t.Errorf(@"IndexByte(%q, '%c') = %v", tt.a, b, pos);
        }
        nint posp = bytes_internal_test_package.IndexBytePortable(a, b);
        if (posp != tt.i) {
            t.Errorf(@"indexBytePortable(%q, '%c') = %v", tt.a, b, posp);
        }
    }
}

public static void TestLastIndexByte(ж<testing.T> t) {
    var testCases = slice<BinOpTest>([
        new("", "q", -1),
        new("abcdef", "q", -1),
        new("abcdefabcdef", "a", len("abcdef")),      // something in the middle
        new("abcdefabcdef", "f", len("abcdefabcde")), // last byte
        new("zabcdefabcdef", "z", 0),                 // first byte
        new("a☺b☻c☹d", "b", len("a☺"))                // non-ascii
    ]);
    foreach (var (_, test) in range(testCases)) {
        nint actual = LastIndexByte(slice<byte>(test.a), test.b[0]);
        if (actual != test.i) {
            t.Errorf("LastIndexByte(%q,%c) = %v; want %v", test.a, test.b[0], actual, test.i);
        }
    }
}

// test a larger buffer with different sizes and alignments
public static void TestIndexByteBig(ж<testing.T> t) {
    nint n = 1024;
    if (testing.Short()) {
        n = 128;
    }
    var b = new slice<byte>(n);
    for (nint i = 0; i < n; i++) {
        // different start alignments
        var b1 = b.slice(i);
        for (nint j = 0; j < len(b1); j++) {
            b1[j] = (byte)'x';
            nint pos = IndexByte(b1, (byte)'x');
            if (pos != j) {
                t.Errorf("IndexByte(%q, 'x') = %v", b1, pos);
            }
            b1[j] = 0;
            pos = IndexByte(b1, (byte)'x');
            if (pos != -1) {
                t.Errorf("IndexByte(%q, 'x') = %v", b1, pos);
            }
        }
        // different end alignments
        b1 = b.slice(0, i);
        for (nint j = 0; j < len(b1); j++) {
            b1[j] = (byte)'x';
            nint pos = IndexByte(b1, (byte)'x');
            if (pos != j) {
                t.Errorf("IndexByte(%q, 'x') = %v", b1, pos);
            }
            b1[j] = 0;
            pos = IndexByte(b1, (byte)'x');
            if (pos != -1) {
                t.Errorf("IndexByte(%q, 'x') = %v", b1, pos);
            }
        }
        // different start and end alignments
        b1 = b.slice(i / 2, n - (i + 1) / 2);
        for (nint j = 0; j < len(b1); j++) {
            b1[j] = (byte)'x';
            nint pos = IndexByte(b1, (byte)'x');
            if (pos != j) {
                t.Errorf("IndexByte(%q, 'x') = %v", b1, pos);
            }
            b1[j] = 0;
            pos = IndexByte(b1, (byte)'x');
            if (pos != -1) {
                t.Errorf("IndexByte(%q, 'x') = %v", b1, pos);
            }
        }
    }
}

// test a small index across all page offsets
public static void TestIndexByteSmall(ж<testing.T> t) {
    var b = new slice<byte>(5015); // bigger than a page
    // Make sure we find the correct byte even when straddling a page.
    for (nint i = 0; i <= len(b) - 15; i++) {
        for (nint j = 0; j < 15; j++) {
            b[i + j] = (byte)(100 + j);
        }
        for (nint j = 0; j < 15; j++) {
            nint p = IndexByte(b.slice(i, i + 15), (byte)(100 + j));
            if (p != j) {
                t.Errorf("IndexByte(%q, %d) = %d", b.slice(i, i + 15), 100 + j, p);
            }
        }
        for (nint j = 0; j < 15; j++) {
            b[i + j] = 0;
        }
    }
    // Make sure matches outside the slice never trigger.
    for (nint i = 0; i <= len(b) - 15; i++) {
        for (nint j = 0; j < 15; j++) {
            b[i + j] = 1;
        }
        for (nint j = 0; j < 15; j++) {
            nint p = IndexByte(b.slice(i, i + 15), (byte)0);
            if (p != -1) {
                t.Errorf("IndexByte(%q, %d) = %d", b.slice(i, i + 15), (nint)0, p);
            }
        }
        for (nint j = 0; j < 15; j++) {
            b[i + j] = 0;
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
        new("𠀳𠀗𠀾𠁄𠀧𠁆𠁂𠀫𠀖𠀪𠀲𠀴𠁀𠀨𠀿", (rune)0x2003F, 56),

        // 2 bytes
        new("ӆ", 'ӆ', 0),
        new("a", 'ӆ', -1),
        new("  ӆ", 'ӆ', 2),
        new("  a", 'ӆ', -1),
        new(strings.Repeat("ц", 64) + "ӆ"u8, 'ӆ', 128), // test cutover
        new(strings.Repeat("ц", 64), 'ӆ', -1),

        // 3 bytes
        new("Ꚁ", 'Ꚁ', 0),
        new("a", 'Ꚁ', -1),
        new("  Ꚁ", 'Ꚁ', 2),
        new("  a", 'Ꚁ', -1),
        new(strings.Repeat("Ꙁ", 64) + "Ꚁ"u8, 'Ꚁ', 192), // test cutover
        new(strings.Repeat("Ꙁ", 64) + "Ꚁ"u8, '䚀', -1),  // 'Ꚁ' and '䚀' share the same last two bytes

        // 4 bytes
        new("𡌀", (rune)0x21300, 0),
        new("a", (rune)0x21300, -1),
        new("  𡌀", (rune)0x21300, 2),
        new("  a", (rune)0x21300, -1),
        new(strings.Repeat("𡋀", 64) + "𡌀"u8, (rune)0x21300, 256), // test cutover
        new(strings.Repeat("𡋀", 64) + "𡌀"u8, (rune)0x23300, -1),  // '𡌀' and '𣌀' share the same last two bytes

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

        // Test the cutover to bytealg.Index when it is triggered in
        // the middle of rune that contains consecutive runs of equal bytes.
        new("aaaaaKKKK\U000bc104", (rune)0xBC104, 17), // cutover: (n + 16) / 8
        new("aaaaaKKKK鄄", '鄄', 17),
        new("aaKKKKKa\U000bc104", (rune)0xBC104, 18), // cutover: 4 + n>>4
        new("aaKKKKKa鄄", '鄄', 18)
    ]);
    foreach (var (_, tt) in range(tests)) {
        nint got = IndexRune(slice<byte>(tt.@in), tt.rune); if (got != tt.want) {
            t.Errorf("IndexRune(%q, %d) = %v; want %v", tt.@in, tt.rune, got, tt.want);
        }
    }

    var haystack = slice<byte>("test世界"u8);
    var haystackʗ1 = haystack;
    var allocs = testing.AllocsPerRun(1000, () => {
        {
            nint i = IndexRune(haystackʗ1, 's'); if (i != 2) {
                t.Fatalf("'s' at %d; want 2", i);
            }
        }
        {
            nint i = IndexRune(haystackʗ1, '世'); if (i != 4) {
                t.Fatalf("'世' at %d; want 4", i);
            }
        }
    });
    if (allocs != 0D) {
        t.Errorf("expected no allocations, got %f", allocs);
    }
}

// test count of a single byte across page offsets
public static void TestCountByte(ж<testing.T> t) {
    var b = new slice<byte>(5015); // bigger than a page
    var windows = slice<nint>([1, 2, 3, 4, 15, 16, 17, 31, 32, 33, 63, 64, 65, 128]);
    var bʗ1 = b;
    void testCountWindow(nint i, nint window) {
        for (nint j = 0; j < window; j++) {
            bʗ1[i + j] = (byte)100;
            nint p = Count(bʗ1.slice(i, i + window), [100]);
            if (p != j + 1) {
                t.Errorf("TestCountByte.Count(%q, 100) = %d", bʗ1.slice(i, i + window), p);
            }
        }
    }

    nint maxWnd = windows[len(windows) - 1];

    for (nint i = 0; i <= 2 * maxWnd; i++) {
        foreach (var (_, vᴛ1) in range(windows)) {
            var window = vᴛ1;

            if (window > len(b.slice(i))) {
                window = len(b.slice(i));
            }
            testCountWindow(i, window);
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)0;
            }
        }
    }
    for (nint i = 4096 - (maxWnd + 1); i < len(b); i++) {
        foreach (var (_, vᴛ2) in range(windows)) {
            var window = vᴛ2;

            if (window > len(b.slice(i))) {
                window = len(b.slice(i));
            }
            testCountWindow(i, window);
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)0;
            }
        }
    }
}

// Make sure we don't count bytes outside our window
public static void TestCountByteNoMatch(ж<testing.T> t) {
    var b = new slice<byte>(5015);
    var windows = slice<nint>([1, 2, 3, 4, 15, 16, 17, 31, 32, 33, 63, 64, 65, 128]);
    for (nint i = 0; i <= len(b); i++) {
        foreach (var (_, vᴛ1) in range(windows)) {
            var window = vᴛ1;

            if (window > len(b.slice(i))) {
                window = len(b.slice(i));
            }
            // Fill the window with non-match
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)100;
            }
            // Try to find something that doesn't exist
            nint p = Count(b.slice(i, i + window), [0]);
            if (p != 0) {
                t.Errorf("TestCountByteNoMatch(%q, 0) = %d", b.slice(i, i + window), p);
            }
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)0;
            }
        }
    }
}

internal static slice<byte> bmbuf;

internal static @string valName(nint x) {
    {
        nint s = x >> 20; if (s << 20 == x) {
            return fmt.Sprintf("%dM"u8, s);
        }
    }
    {
        nint s = x >> 10; if (s << 10 == x) {
            return fmt.Sprintf("%dK"u8, s);
        }
    }
    return fmt.Sprint(x);
}

internal static void benchBytes(ж<testing.B> b, slice<nint> sizes, Action<ж<testing.B>, nint> f) {
    foreach (var (_, n) in range(sizes)) {
        if (isRaceBuilder && n > 4 << 10) {
            continue;
        }
        b.Run(valName(n), (ж<testing.B> bΔ1) => {
            if (len(bmbuf) < n) {
                bmbuf = new slice<byte>(n);
            }
            bΔ1.SetBytes((int64)n);
            f(bΔ1, n);
        });
    }
}

internal static slice<nint> indexSizes = [10, 32, 4 << 10, 4 << 20, 64 << 20];

internal static bool isRaceBuilder = strings.HasSuffix(testenv.Builder(), "-race");

public static void BenchmarkIndexByte(ж<testing.B> b) {
    benchBytes(b, indexSizes, bmIndexByte(IndexByte));
}

public static void BenchmarkIndexBytePortable(ж<testing.B> b) {
    benchBytes(b, indexSizes, bmIndexByte(bytes_internal_test_package.IndexBytePortable));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badIndexˢ = (@string)"bad index"u8;

internal static Action<ж<testing.B>, nint> bmIndexByte(Func<slice<byte>, byte, nint> index) {
    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf.slice(0, n);
        buf[n - 1] = (byte)'x';
        for (nint i = 0; i < (~b).N; i++) {
            nint j = index(buf, (byte)'x');
            if (j != n - 1) {
                b.Fatal(badIndexˢ, j);
            }
        }
        buf[n - 1] = (byte)'\x00';
    };
}

public static void BenchmarkIndexRune(ж<testing.B> b) {
    benchBytes(b, indexSizes, bmIndexRune(IndexRune));
}

public static void BenchmarkIndexRuneASCII(ж<testing.B> b) {
    benchBytes(b, indexSizes, bmIndexRuneASCII(IndexRune));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string latinˢ => ref latinˢᵇ;
private static readonly @string latinˢᵇ = "Latin"u8;
internal static ref readonly @string cyrillicˢ => ref cyrillicˢᵇ;
private static readonly @string cyrillicˢᵇ = "Cyrillic"u8;
internal static ref readonly @string hanˢ => ref hanˢᵇ;
private static readonly @string hanˢᵇ = "Han"u8;

public static void BenchmarkIndexRuneUnicode(ж<testing.B> b) {
    b.Run(latinˢ, (ж<testing.B> bΔ1) => {
        // Latin is mostly 1, 2, 3 byte runes.
        benchBytes(bΔ1, indexSizes, bmIndexRuneUnicode(Δunicode.Latin, 'é'));
    });
    b.Run(cyrillicˢ, (ж<testing.B> bΔ2) => {
        // Cyrillic is mostly 2 and 3 byte runes.
        benchBytes(bΔ2, indexSizes, bmIndexRuneUnicode(Δunicode.Cyrillic, 'Ꙁ'));
    });
    b.Run(hanˢ, (ж<testing.B> bΔ3) => {
        // Han consists only of 3 and 4 byte runes.
        benchBytes(bΔ3, indexSizes, bmIndexRuneUnicode(Δunicode.Han, (rune)0x2003F));
    });
}

internal static Action<ж<testing.B>, nint> bmIndexRuneASCII(Func<slice<byte>, rune, nint> index) {
    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf.slice(0, n);
        buf[n - 1] = (byte)'x';
        for (nint i = 0; i < (~b).N; i++) {
            nint j = index(buf, 'x');
            if (j != n - 1) {
                b.Fatal(badIndexˢ, j);
            }
        }
        buf[n - 1] = (byte)'\x00';
    };
}

internal static Action<ж<testing.B>, nint> bmIndexRune(Func<slice<byte>, rune, nint> index) {
    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf.slice(0, n);
        utf8.EncodeRune(buf.slice(n - 3), '世');
        for (nint i = 0; i < (~b).N; i++) {
            nint j = index(buf, '世');
            if (j != n - 3) {
                b.Fatal(badIndexˢ, j);
            }
        }
        buf[n - 3] = (byte)'\x00';
        buf[n - 2] = (byte)'\x00';
        buf[n - 1] = (byte)'\x00';
    };
}

internal static Action<ж<testing.B>, nint> bmIndexRuneUnicode(ж<Δunicode.RangeTable> Ꮡrt, rune needle) {
    ref var rt = ref Ꮡrt.DerefOrNull();
    var rsᴷ = new stackAppend<rune>();

    slice<rune> rs = default;
    foreach (var (_, r16) in range(rt.R16)) {
        for (var r = (rune)r16.Lo; r <= (rune)r16.Hi; r += (rune)r16.Stride) {
            if (r != needle) {
                rs = append(rsᴷ, rs, r);
            }
        }
    }
    foreach (var (_, r32) in range(rt.R32)) {
        for (var r = (rune)r32.Lo; r <= (rune)r32.Hi; r += (rune)r32.Stride) {
            if (r != needle) {
                rs = append(rs, r);
            }
        }
    }
    // Shuffle the runes so that they are not in descending order.
    // The sort is deterministic since this is used for benchmarks,
    // which need to be repeatable.
    var rr = rand.New(rand.NewSource(1));
    var rsʗ1 = rs;
    rr.Shuffle(len(rs), (nint i, nint j) => {
        (rsʗ1[i], rsʗ1[j]) = (rsʗ1[j], rsʗ1[i]);
    });
    @string uchars = (@string)rs;

    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf.slice(0, n);
        nint o = copy(buf, uchars);
        while (o < len(buf)) {
            o += copy(buf.slice(o), uchars);
        }

        // Make space for the needle rune at the end of buf.
        nint m = utf8.RuneLen(needle);
        for (nint oΔ1 = m; oΔ1 > 0; ) {
            var (_, sz) = utf8.DecodeLastRune(buf);
            copy(buf.slice(len(buf) - sz), "\x00\x00\x00\x00"u8);
            buf = buf.slice(0, len(buf) - sz);
            oΔ1 -= sz;
        }
        buf = utf8.AppendRune(buf.slice(0, n - m), needle);

        n -= m; // adjust for rune len
        for (nint i = 0; i < (~b).N; i++) {
            nint j = IndexRune(buf, needle);
            if (j != n) {
                b.Fatal(badIndexˢ, j);
            }
        }
        foreach (var i in range(len(buf))) {
            buf[i] = (byte)'\x00';
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badEqualˢ = (@string)"bad equal"u8;
internal static ref readonly @string sameˢ => ref sameˢᵇ;
private static readonly @string sameˢᵇ = "same"u8;

public static void BenchmarkEqual(ж<testing.B> b) {
    b.Run("0", (ж<testing.B> bΔ1) => {
        array<byte> buf = new(4);
        var buf1 = buf[0..0];
        var buf2 = buf[1..1];
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var eq = Equal(buf1, buf2);
            if (!eq) {
                bΔ1.Fatal(badEqualˢ);
            }
        }
    });

    var sizes = slice<nint>([1, 6, 9, 15, 16, 20, 32, 4 << 10, 4 << 20, 64 << 20]);

    var sizesʗ1 = sizes;
    b.Run(sameˢ, (ж<testing.B> bΔ2) => {
        benchBytes(bΔ2, sizesʗ1, bmEqual((slice<byte> a, slice<byte> bΔ3) => Equal(a, a)));
    });

    benchBytes(b, sizes, bmEqual(Equal));
}

internal static Action<ж<testing.B>, nint> bmEqual(Func<slice<byte>, slice<byte>, bool> equal) {
    return (ж<testing.B> b, nint n) => {
        if (len(bmbuf) < 2 * n) {
            bmbuf = new slice<byte>(2 * n);
        }
        var buf1 = bmbuf.slice(0, n);
        var buf2 = bmbuf.slice(n, 2 * n);
        buf1[n - 1] = (byte)'x';
        buf2[n - 1] = (byte)'x';
        for (nint i = 0; i < (~b).N; i++) {
            var eq = equal(buf1, buf2);
            if (!eq) {
                b.Fatal(badEqualˢ);
            }
        }
        buf1[n - 1] = (byte)'\x00';
        buf2[n - 1] = (byte)'\x00';
    };
}

public static void BenchmarkEqualBothUnaligned(ж<testing.B> b) {
    var sizes = slice<nint>([64, 4 << 10]);
    if (!isRaceBuilder) {
        sizes = appendꓸꓸꓸ(sizes, slice<nint>([4 << 20, 64 << 20]));
    }
    nint maxSize = 2 * (sizes[len(sizes) - 1] + 8);
    if (len(bmbuf) < maxSize) {
        bmbuf = new slice<byte>(maxSize);
    }

    foreach (var (_, n) in range(sizes)) {
        foreach (var (_, off) in range(slice<nint>([0, 1, 4, 7]))) {
            var buf1 = bmbuf.slice(off, off + n);
            nint buf2Start = (len(bmbuf) / 2) + off;
            var buf2 = bmbuf.slice(buf2Start, buf2Start + n);
            buf1[n - 1] = (byte)'x';
            buf2[n - 1] = (byte)'x';
            var buf1ʗ1 = buf1;
            var buf2ʗ1 = buf2;
            b.Run(fmt.Sprint(n, off), (ж<testing.B> bΔ1) => {
                bΔ1.SetBytes((int64)n);
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    var eq = Equal(buf1ʗ1, buf2ʗ1);
                    if (!eq) {
                        bΔ1.Fatal(badEqualˢ);
                    }
                }
            });
            buf1[n - 1] = (byte)'\x00';
            buf2[n - 1] = (byte)'\x00';
        }
    }
}

public static void BenchmarkIndex(ж<testing.B> b) {
    benchBytes(b, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf.slice(0, n);
        buf[n - 1] = (byte)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Index(buf, buf.slice(n - 7));
            if (j != n - 7) {
                bΔ1.Fatal(badIndexˢ, j);
            }
        }
        buf[n - 1] = (byte)'\x00';
    });
}

public static void BenchmarkIndexEasy(ж<testing.B> b) {
    benchBytes(b, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf.slice(0, n);
        buf[n - 1] = (byte)'x';
        buf[n - 7] = (byte)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Index(buf, buf.slice(n - 7));
            if (j != n - 7) {
                bΔ1.Fatal(badIndexˢ, j);
            }
        }
        buf[n - 1] = (byte)'\x00';
        buf[n - 7] = (byte)'\x00';
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badCountˢ = (@string)"bad count"u8;

public static void BenchmarkCount(ж<testing.B> b) {
    benchBytes(b, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf.slice(0, n);
        buf[n - 1] = (byte)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Count(buf, buf.slice(n - 7));
            if (j != 1) {
                bΔ1.Fatal(badCountˢ, j);
            }
        }
        buf[n - 1] = (byte)'\x00';
    });
}

public static void BenchmarkCountEasy(ж<testing.B> b) {
    benchBytes(b, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf.slice(0, n);
        buf[n - 1] = (byte)'x';
        buf[n - 7] = (byte)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Count(buf, buf.slice(n - 7));
            if (j != 1) {
                bΔ1.Fatal(badCountˢ, j);
            }
        }
        buf[n - 1] = (byte)'\x00';
        buf[n - 7] = (byte)'\x00';
    });
}

public static void BenchmarkCountSingle(ж<testing.B> b) {
    benchBytes(b, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf.slice(0, n);
        nint step = 8;
        for (nint i = 0; i < len(buf); i += step) {
            buf[i] = 1;
        }
        nint expect = quo((len(buf) + (step - 1)), step);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Count(buf, [1]);
            if (j != expect) {
                bΔ1.Fatal(badCountˢ, j, expect);
            }
        }
        clear(buf);
    });
}

partial struct SplitTest {
    internal @string s;
    internal @string sep;
    internal nint n;
    internal slice<@string> a;
}

internal static slice<SplitTest> splittests = [
    new("", "", -1, []),
    new(abcd, "a", 0, default),
    new(abcd, "", 2, ["a", "bcd"]),
    new(abcd, "a", -1, ["", "bcd"]),
    new(abcd, "z", -1, ["abcd"]),
    new(abcd, "", -1, ["a", "b", "c", "d"]),
    new(commas, ",", -1, ["1", "2", "3", "4"]),
    new(dots, "...", -1, ["1", ".2", ".3", ".4"]),
    new(faces, "☹", -1, ["☺☻", ""]),
    new(faces, "~", -1, [faces]),
    new(faces, "", -1, ["☺", "☻", "☹"]),
    new("1 2 3 4", " ", 3, ["1", "2", "3 4"]),
    new("1 2", " ", 3, ["1", "2"]),
    new("123", "", 2, ["1", "23"]),
    new("123", "", 17, ["1", "2", "3"]),
    new("bT", "T", unchecked((nint)(2305843009213693951L)), ["b", ""]),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), "", -1, [((@string)(new byte[]{0xff})), "-", ((@string)(new byte[]{0xff}))]),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), "-", -1, [((@string)(new byte[]{0xff})), ((@string)(new byte[]{0xff}))])
];

public static void TestSplit(ж<testing.T> t) {
    foreach (var (_, tt) in range(splittests)) {
        var a = SplitN(slice<byte>(tt.s), slice<byte>(tt.sep), tt.n);

        // Appending to the results should not change future results.
        slice<byte> x = default;
        foreach (var (_, v) in range(a)) {
            x = append(v, (byte)'z');
        }

        var result = sliceOfString(a);
        if (!slices.Equal<slice<@string>, @string>(result, tt.a)) {
            t.Errorf(@"Split(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, result, tt.a);
            continue;
        }

        if (tt.n < 0) {
            var b = sliceOfString(slices.Collect(SplitSeq(slice<byte>(tt.s), slice<byte>(tt.sep))));
            if (!slices.Equal<slice<@string>, @string>(b, tt.a)) {
                t.Errorf(@"collect(SplitSeq(%q, %q)) = %v; want %v", tt.s, tt.sep, b, tt.a);
            }
        }

        if (tt.n == 0 || len(a) == 0) {
            continue;
        }

        @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
            t.Errorf("last appended result was %s; want %s", x, want);
        }

        var s = Join(a, slice<byte>(tt.sep));
        if (((sstring)s) != tt.s) {
            t.Errorf(@"Join(Split(%q, %q, %d), %q) = %q", tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = sliceOfString(Split(slice<byte>(tt.s), slice<byte>(tt.sep)));
            if (!slices.Equal<slice<@string>, @string>(result, b)) {
                t.Errorf("Split disagrees with SplitN(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, b, a);
            }
        }
        if (len(a) > 0) {
            var (@in, @out) = (a[0], s);
            if (cap(@in) == cap(@out) && Ꮡelem(@in[..1], 0) == Ꮡelem(@out[..1], 0)) {
                t.Errorf("Join(%#v, %q) didn't copy", a, tt.sep);
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
        var a = SplitAfterN(slice<byte>(tt.s), slice<byte>(tt.sep), tt.n);

        // Appending to the results should not change future results.
        slice<byte> x = default;
        foreach (var (_, v) in range(a)) {
            x = append(v, (byte)'z');
        }

        var result = sliceOfString(a);
        if (!slices.Equal<slice<@string>, @string>(result, tt.a)) {
            t.Errorf(@"Split(%q, %q, %d) = %v; want %v", tt.s, tt.sep, tt.n, result, tt.a);
            continue;
        }

        if (tt.n < 0) {
            var b = sliceOfString(slices.Collect(SplitAfterSeq(slice<byte>(tt.s), slice<byte>(tt.sep))));
            if (!slices.Equal<slice<@string>, @string>(b, tt.a)) {
                t.Errorf(@"collect(SplitAfterSeq(%q, %q)) = %v; want %v", tt.s, tt.sep, b, tt.a);
            }
        }

        @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
            t.Errorf("last appended result was %s; want %s", x, want);
        }

        var s = Join(a, default);
        if (((sstring)s) != tt.s) {
            t.Errorf(@"Join(Split(%q, %q, %d), %q) = %q", tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = sliceOfString(SplitAfter(slice<byte>(tt.s), slice<byte>(tt.sep)));
            if (!slices.Equal<slice<@string>, @string>(result, b)) {
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
    new("  abc  ", ["abc"]),
    new("1 2 3 4", ["1", "2", "3", "4"]),
    new("1  2  3  4", ["1", "2", "3", "4"]),
    new("1\t\t2\t\t3\t4", ["1", "2", "3", "4"]),
    new("1\u20002\u20013\u20024", ["1", "2", "3", "4"]),
    new("\u2000\u2001\u2002", []),
    new("\n™\t™\n", ["™", "™"]),
    new(faces, [faces])
];

public static void TestFields(ж<testing.T> t) {
    foreach (var (_, tt) in range(fieldstests)) {
        var b = slice<byte>(tt.s);
        var a = Fields(b);

        // Appending to the results should not change future results.
        slice<byte> x = default;
        foreach (var (_, v) in range(a)) {
            x = append(v, (byte)'z');
        }

        var result = sliceOfString(a);
        if (!slices.Equal<slice<@string>, @string>(result, tt.a)) {
            t.Errorf("Fields(%q) = %v; want %v", tt.s, a, tt.a);
            continue;
        }

        var result2 = sliceOfString(collect(t, FieldsSeq(slice<byte>(tt.s))));
        if (!slices.Equal<slice<@string>, @string>(result2, tt.a)) {
            t.Errorf(@"collect(FieldsSeq(%q)) = %v; want %v", tt.s, result2, tt.a);
        }

        if (((sstring)b) != tt.s) {
            t.Errorf("slice changed to %s; want %s", (@string)b, tt.s);
        }
        if (len(tt.a) > 0) {
            @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
                t.Errorf("last appended result was %s; want %s", x, want);
            }
        }
    }
}

public static void TestFieldsFunc(ж<testing.T> t) {
    foreach (var (_, tt) in range(fieldstests)) {
        var a = FieldsFunc(slice<byte>(tt.s), Δunicode.IsSpace);
        var result = sliceOfString(a);
        if (!slices.Equal<slice<@string>, @string>(result, tt.a)) {
            t.Errorf("FieldsFunc(%q, unicode.IsSpace) = %v; want %v", tt.s, a, tt.a);
            continue;
        }
    }
    var pred = (rune c) => c == 'X';
    slice<FieldsTest> fieldsFuncTests = slice<FieldsTest>([
        new("", []),
        new("XX", []),
        new("XXhiXXX", ["hi"]),
        new("aXXbXXXcX", ["a", "b", "c"])
    ]);
    foreach (var (_, tt) in range(fieldsFuncTests)) {
        var b = slice<byte>(tt.s);
        var a = FieldsFunc(b, pred);

        // Appending to the results should not change future results.
        slice<byte> x = default;
        foreach (var (_, v) in range(a)) {
            x = append(v, (byte)'z');
        }

        var result = sliceOfString(a);
        if (!slices.Equal<slice<@string>, @string>(result, tt.a)) {
            t.Errorf("FieldsFunc(%q) = %v, want %v", tt.s, a, tt.a);
        }

        var result2 = sliceOfString(collect(t, FieldsFuncSeq(slice<byte>(tt.s), pred)));
        if (!slices.Equal<slice<@string>, @string>(result2, tt.a)) {
            t.Errorf(@"collect(FieldsFuncSeq(%q)) = %v; want %v", tt.s, result2, tt.a);
        }

        if (((sstring)b) != tt.s) {
            t.Errorf("slice changed to %s; want %s", b, tt.s);
        }
        if (len(tt.a) > 0) {
            @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
                t.Errorf("last appended result was %s; want %s", x, want);
            }
        }
    }
}

// Test case for any function which accepts and returns a byte slice.
// For ease of creation, we write the input byte slice as a string.
partial struct StringTest {
    internal @string @in;
    internal slice<byte> @out;
}

internal static slice<StringTest> upperTests = [
    new("", slice<byte>(""u8)),
    new("ONLYUPPER", slice<byte>("ONLYUPPER"u8)),
    new("abc", slice<byte>("ABC"u8)),
    new("AbC123", slice<byte>("ABC123"u8)),
    new("azAZ09_", slice<byte>("AZAZ09_"u8)),
    new("longStrinGwitHmixofsmaLLandcAps", slice<byte>("LONGSTRINGWITHMIXOFSMALLANDCAPS"u8)),
    new("long\u0250string\u0250with\u0250nonascii\u2C6Fchars", slice<byte>("LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS"u8)),
    new("\u0250\u0250\u0250\u0250\u0250", slice<byte>("\u2C6F\u2C6F\u2C6F\u2C6F\u2C6F"u8)), // grows one byte per char
    new("a\u0080\U0010FFFF", slice<byte>("A\u0080\U0010FFFF"u8))                            // test utf8.RuneSelf and utf8.MaxRune
];

internal static slice<StringTest> lowerTests = [
    new("", slice<byte>(""u8)),
    new("abc", slice<byte>("abc"u8)),
    new("AbC123", slice<byte>("abc123"u8)),
    new("azAZ09_", slice<byte>("azaz09_"u8)),
    new("longStrinGwitHmixofsmaLLandcAps", slice<byte>("longstringwithmixofsmallandcaps"u8)),
    new("LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS", slice<byte>("long\u0250string\u0250with\u0250nonascii\u0250chars"u8)),
    new("\u2C6D\u2C6D\u2C6D\u2C6D\u2C6D", slice<byte>("\u0251\u0251\u0251\u0251\u0251"u8)), // shrinks one byte per char
    new("A\u0080\U0010FFFF", slice<byte>("a\u0080\U0010FFFF"u8))                            // test utf8.RuneSelf and utf8.MaxRune
];

internal static ref readonly @string space => ref spaceᵇ;
private static readonly @string spaceᵇ = "\t\v\r\f\n\u0085\u00a0\u2000\u3000"u8;

internal static slice<StringTest> trimSpaceTests = [
    new("", default),
    new("  a", slice<byte>("a"u8)),
    new("b  ", slice<byte>("b"u8)),
    new("abc", slice<byte>("abc"u8)),
    new(space + "abc" + space, slice<byte>("abc"u8)),
    new(" ", default),
    new("\u3000 ", default),
    new(" \u3000", default),
    new(" \t\r\n \t\t\r\r\n\n ", default),
    new(" \t\r\n x\t\t\r\r\n\n ", slice<byte>("x"u8)),
    new(" \u2000\t\r\n x\t\t\r\r\ny\n \u3000", slice<byte>("x\t\t\r\r\ny"u8)),
    new("1 \t\r\n2", slice<byte>("1 \t\r\n2"u8)),
    new(((@string)(new byte[]{0x20, 0x78, 0x80})), slice<byte>(((@string)(new byte[]{0x78, 0x80})))),
    new(((@string)(new byte[]{0x20, 0x78, 0xc0})), slice<byte>(((@string)(new byte[]{0x78, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0})))),
    new("x ☺ ", slice<byte>("x ☺"u8))
];

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runStringTests(ж<testing.T> t, Func<slice<byte>, slice<byte>> f, @string funcName, slice<StringTest> testCases) {
    foreach (var (_, tc) in range(testCases)) {
        var actual = f(slice<byte>(tc.@in));
        if (actual == nil && tc.@out != nil) {
            t.Errorf("%s(%q) = nil; want %q", funcName, tc.@in, tc.@out);
        }
        if (actual != nil && tc.@out == nil) {
            t.Errorf("%s(%q) = %q; want nil", funcName, tc.@in, actual);
        }
        if (!Equal(actual, tc.@out)) {
            t.Errorf("%s(%q) = %q; want %q", funcName, tc.@in, actual, tc.@out);
        }
    }
}

internal static @string tenRunes(rune r) {
    var runes = new slice<rune>(10);
    foreach (var i in range(len(runes))) {
        runes[i] = r;
    }
    return (@string)runes;
}

// User-defined self-inverse mapping function
internal static rune rot13(rune r) {
    const rune step = 13;
    if (r >= 'a' && r <= 'z') {
        return ((r - 'a' + step) % 26) + 'a';
    }
    if (r >= 'A' && r <= 'Z') {
        return ((r - 'A' + step) % 26) + 'A';
    }
    return r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nGbMrqˢ => ref nGbMrqˢᵇ;
private static readonly @string nGbMrqˢᵇ = "n gb mrq"u8;
internal static ref readonly @string aToZedˢ => ref aToZedˢᵇ;
private static readonly @string aToZedˢᵇ = "a to zed"u8;
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "Hello"u8;

public static void TestMap(ж<testing.T> t) {
    // Run a couple of awful growth/shrinkage tests
    @string a = tenRunes('a');

    // 1. Grow. This triggers two reallocations in Map.
    var maxRune = rune (rune r) => Δunicode.MaxRune;
    var m = Map(maxRune, slice<byte>(a));
    @string expect = tenRunes(Δunicode.MaxRune);
    if (((sstring)m) != expect) {
        t.Errorf("growing: expected %q got %q", expect, m);
    }

    // 2. Shrink
    var minRune = (rune r) => (rune)'a';
    m = Map(minRune, slice<byte>(tenRunes(Δunicode.MaxRune)));
    expect = a;
    if (((sstring)m) != expect) {
        t.Errorf("shrinking: expected %q got %q", expect, m);
    }

    // 3. Rot13
    m = Map(rot13, slice<byte>("a to zed"u8));
    expect = nGbMrqˢ;
    if (((sstring)m) != expect) {
        t.Errorf("rot13: expected %q got %q", expect, m);
    }

    // 4. Rot13^2
    m = Map(rot13, Map(rot13, slice<byte>("a to zed"u8)));
    expect = aToZedˢ;
    if (((sstring)m) != expect) {
        t.Errorf("rot13: expected %q got %q", expect, m);
    }

    // 5. Drop
    var dropNotLatin = (rune r) => {
        if (Δunicode.Is(Δunicode.Latin, r)) {
            return r;
        }
        return -1;
    };
    m = Map(dropNotLatin, slice<byte>("Hello, 세계"u8));
    expect = helloˢ;
    if (((sstring)m) != expect) {
        t.Errorf("drop: expected %q got %q", expect, m);
    }

    // 6. Invalid rune
    var invalidRune = rune (rune r) => utf8.MaxRune + 1;
    m = Map(invalidRune, slice<byte>("x"u8));
    expect = "\uFFFD";
    if (((sstring)m) != expect) {
        t.Errorf("invalidRune: expected %q got %q", expect, m);
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

public static void BenchmarkToUpper(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(upperTests)) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tin = slice<byte>(tc.@in);
        var tcʗ1 = tc;
        var tinʗ1 = tin;
        b.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var actual = ToUpper(tinʗ1);
                if (!Equal(actual, tcʗ1.@out)) {
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

        var tin = slice<byte>(tc.@in);
        var tcʗ1 = tc;
        var tinʗ1 = tin;
        b.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var actual = ToLower(tinʗ1);
                if (!Equal(actual, tcʗ1.@out)) {
                    bΔ1.Errorf("ToLower(%q) = %q; want %q", tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}

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
        var got = ToValidUTF8(slice<byte>(tc.@in), slice<byte>(tc.repl));
        if (!Equal(got, slice<byte>(tc.@out))) {
            t.Errorf("ToValidUTF8(%q, %q) = %q; want %q", tc.@in, tc.repl, got, tc.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trimSpaceˢ => ref trimSpaceˢᵇ;
private static readonly @string trimSpaceˢᵇ = "TrimSpace"u8;

public static void TestTrimSpace(ж<testing.T> t) { runStringTests(t, TrimSpace, trimSpaceˢ, trimSpaceTests); }

partial struct RepeatTest {
    internal @string @in, @out;
    internal nint count;
}

internal static @string longString = "a"u8 + ((sstring)new slice<byte>((1 << 16))) + "z"u8;

public static readonly ж<slice<RepeatTest>> ᏑRepeatTests = new StaticBox<slice<RepeatTest>>(slice<RepeatTest>([
    new("", "", 0),
    new("", "", 1),
    new("", "", 2),
    new("-", "", 0),
    new("-", "-", 1),
    new("-", "----------", 10),
    new("abc ", "abc abc abc ", 3),
    // Tests for results over the chunkLimit
    new((@string)(rune)0, (@string)new slice<byte>((1 << 16)), 1 << 16),
    new(longString, longString + longString, 2)
]));
public static ref slice<RepeatTest> RepeatTests => ref ((StaticBox<slice<RepeatTest>>)ᏑRepeatTests).ValueSlot;

public static void TestRepeat(ж<testing.T> t) {
    foreach (var (_, tt) in range(RepeatTests)) {
        var tin = slice<byte>(tt.@in);
        var tout = slice<byte>(tt.@out);
        var a = Repeat(tin, tt.count);
        if (!Equal(a, tout)) {
            t.Errorf("Repeat(%q, %d) = %q; want %q", tin, tt.count, a, tout);
            continue;
        }
    }
}

internal static error /*err*/ repeat(slice<byte> b, nint count) {
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

        Repeat(b, count);
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
            var err = repeat(slice<byte>(tt.s), tt.count);
            if (tt.errStr == ""u8) {
                if (err != nil) {
                    t.Errorf("#%d panicked %v", i, err);
                }
                continue;
            }

            if (err == nil || !strings.Contains(err.Error(), tt.errStr)) {
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

partial struct RunesTest {
    internal @string @in;
    internal slice<rune> @out;
    internal bool lossy;
}

public static readonly ж<slice<RunesTest>> ᏑRunesTests = new StaticBox<slice<RunesTest>>([
    new("", [], false),
    new(" ", [32], false),
    new("ABC", [65, 66, 67], false),
    new("abc", [97, 98, 99], false),
    new("\u65e5\u672c\u8a9e", [26085, 26412, 35486], false),
    new(((@string)(new byte[]{0x61, 0x62, 0x80, 0x63})), [97, 98, 0xFFFD, 99], true),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x63})), [97, 98, 0xFFFD, 99], true)
]);
public static ref slice<RunesTest> RunesTests => ref ((StaticBox<slice<RunesTest>>)ᏑRunesTests).ValueSlot;

public static void TestRunes(ж<testing.T> t) {
    foreach (var (_, tt) in range(RunesTests)) {
        var tin = slice<byte>(tt.@in);
        var a = Runes(tin);
        if (!slices.Equal<slice<rune>, rune>(a, tt.@out)) {
            t.Errorf("Runes(%q) = %v; want %v", tin, a, tt.@out);
            continue;
        }
        if (!tt.lossy) {
            // can only test reassembly if we didn't lose information
            @string s = (@string)a;
            if (s != tt.@in) {
                t.Errorf("string(Runes(%q)) = %x; want %x", tin, s, tin);
            }
        }
    }
}

partial struct TrimTest {
    internal @string f;
    internal @string @in, arg, @out;
}

internal static slice<TrimTest> trimTests = [
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

partial struct TrimNilTest {
    internal @string f;
    internal slice<byte> @in;
    internal @string arg;
    internal slice<byte> @out;
}

internal static slice<TrimNilTest> trimNilTests = [
    new("Trim", default, "", default),
    new("Trim", [], "", default),
    new("Trim", [(byte)'a'], "a", default),
    new("Trim", [(byte)'a', (byte)'a'], "a", default),
    new("Trim", [(byte)'a'], "ab", default),
    new("Trim", [(byte)'a', (byte)'b'], "ab", default),
    new("Trim", slice<byte>("☺"u8), "☺", default),
    new("TrimLeft", default, "", default),
    new("TrimLeft", [], "", default),
    new("TrimLeft", [(byte)'a'], "a", default),
    new("TrimLeft", [(byte)'a', (byte)'a'], "a", default),
    new("TrimLeft", [(byte)'a'], "ab", default),
    new("TrimLeft", [(byte)'a', (byte)'b'], "ab", default),
    new("TrimLeft", slice<byte>("☺"u8), "☺", default),
    new("TrimRight", default, "", default),
    new("TrimRight", [], "", []),
    new("TrimRight", [(byte)'a'], "a", []),
    new("TrimRight", [(byte)'a', (byte)'a'], "a", []),
    new("TrimRight", [(byte)'a'], "ab", []),
    new("TrimRight", [(byte)'a', (byte)'b'], "ab", []),
    new("TrimRight", slice<byte>("☺"u8), "☺", []),
    new("TrimPrefix", default, "", default),
    new("TrimPrefix", [], "", []),
    new("TrimPrefix", [(byte)'a'], "a", []),
    new("TrimPrefix", slice<byte>("☺"u8), "☺", []),
    new("TrimSuffix", default, "", default),
    new("TrimSuffix", [], "", []),
    new("TrimSuffix", [(byte)'a'], "a", []),
    new("TrimSuffix", slice<byte>("☺"u8), "☺", [])
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "nil"u8;

public static void TestTrim(ж<testing.T> t) {
    (Func<slice<byte>, @string, slice<byte>>, Func<slice<byte>, slice<byte>, slice<byte>>) toFn(@string name) {
        var exprᴛ1 = name;
        switch (ᐧ) {
        case {} when exprᴛ1 == "Trim"u8: {
            return (Trim, default);
        }
        case {} when exprᴛ1 == "TrimLeft"u8: {
            return (TrimLeft, default);
        }
        case {} when exprᴛ1 == "TrimRight"u8: {
            return (TrimRight, default);
        }
        case {} when exprᴛ1 == "TrimPrefix"u8: {
            return (default, TrimPrefix);
        }
        case {} when exprᴛ1 == "TrimSuffix"u8: {
            return (default, TrimSuffix);
        }
        default: {
            t.Errorf("Undefined trim function %s", name);
            return (default, default);
        }}
    }

    foreach (var (_, tc) in range(trimTests)) {
        @string name = tc.f;
        var (f, fb) = toFn(name);
        if (f == nil && fb == nil) {
            continue;
        }
        @string actual = default;
        if (f != nil) {
            actual = (@string)f(slice<byte>(tc.@in), tc.arg);
        } else {
            actual = (@string)fb(slice<byte>(tc.@in), slice<byte>(tc.arg));
        }
        if (actual != tc.@out) {
            t.Errorf("%s(%q, %q) = %q; want %q", name, tc.@in, tc.arg, actual, tc.@out);
        }
    }

    foreach (var (_, tc) in range(trimNilTests)) {
        @string name = tc.f;
        var (f, fb) = toFn(name);
        if (f == nil && fb == nil) {
            continue;
        }
        slice<byte> actual = default;
        if (f != nil) {
            actual = f(tc.@in, tc.arg);
        } else {
            actual = fb(tc.@in, slice<byte>(tc.arg));
        }
        @string report(slice<byte> s) {
            if (s == nil) {
                return nilˢ;
            } else {
                return fmt.Sprintf("%q"u8, s);
            }
        }
        if (len(actual) != 0) {
            t.Errorf("%s(%s, %q) returned non-empty value", name, report(tc.@in), tc.arg);
        } else {
            var actualNil = actual == nil;
            var outNil = tc.@out == nil;
            if (actualNil != outNil) {
                t.Errorf("%s(%s, %q) got nil %t; want nil %t", name, report(tc.@in), tc.arg, actualNil, outNil);
            }
        }
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

partial struct TrimFuncTest {
    internal predicate f;
    internal @string @in;
    internal slice<byte> trimOut;
    internal slice<byte> leftOut;
    internal slice<byte> rightOut;
}

internal static predicate not(predicate p) {
        var pʗ1 = p;
    return new predicate(
        (rune r) => !pʗ1.f(r),
        "not "u8 + p.name
    );
}

internal static slice<TrimFuncTest> trimFuncTests = slice<TrimFuncTest>([
    new(isSpace, space + " hello " + space,
        slice<byte>("hello"u8),
        slice<byte>("hello " + space),
        slice<byte>(space + " hello")),
    new(isDigit, "\u0e50\u0e5212hello34\u0e50\u0e51",
        slice<byte>("hello"u8),
        slice<byte>("hello34\u0e50\u0e51"u8),
        slice<byte>("\u0e50\u0e5212hello"u8)),
    new(isUpper, "\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F",
        slice<byte>("hello"u8),
        slice<byte>("helloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8),
        slice<byte>("\u2C6F\u2C6F\u2C6F\u2C6FABCDhello"u8)),
    new(not(isSpace), "hello" + space + "hello",
        slice<byte>(space),
        slice<byte>(space + "hello"),
        slice<byte>("hello" + space)),
    new(not(isDigit), "hello\u0e50\u0e521234\u0e50\u0e51helo",
        slice<byte>("\u0e50\u0e521234\u0e50\u0e51"u8),
        slice<byte>("\u0e50\u0e521234\u0e50\u0e51helo"u8),
        slice<byte>("hello\u0e50\u0e521234\u0e50\u0e51"u8)),
    new(isValidRune, ((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})),
        slice<byte>(((@string)(new byte[]{0xc0, 0x61, 0xc0}))),
        slice<byte>(((@string)(new byte[]{0xc0, 0x61, 0xc0, 0x63, 0x64}))),
        slice<byte>(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0})))),
    new(not(isValidRune), ((@string)(new byte[]{0xc0, 0x61, 0xc0})),
        slice<byte>("a"u8),
        slice<byte>(((@string)(new byte[]{0x61, 0xc0}))),
        slice<byte>(((@string)(new byte[]{0xc0, 0x61})))),
    // The nils returned by TrimLeftFunc are odd behavior, but we need
    // to preserve backwards compatibility.
    new(isSpace, "",
        default,
        default,
        slice<byte>(""u8)),
    new(isSpace, " ",
        default,
        default,
        slice<byte>(""u8))
]);

internal partial struct TestTrimFunc_trimmers /*dyn*/ {
    internal @string name;
    internal Func<slice<byte>, Func<rune, bool>, slice<byte>> trim;
    internal slice<byte> @out;
}

public static void TestTrimFunc(ж<testing.T> t) {
    foreach (var (_, tc) in range(trimFuncTests)) {
        var trimmers = slice<TestTrimFunc_trimmers>([
            new("TrimFunc", TrimFunc, tc.trimOut),
            new("TrimLeftFunc", TrimLeftFunc, tc.leftOut),
            new("TrimRightFunc", TrimRightFunc, tc.rightOut)
        ]);
        foreach (var (_, trimmer) in range(trimmers)) {
            var actual = trimmer.trim(slice<byte>(tc.@in), tc.f.f);
            if (actual == nil && trimmer.@out != nil) {
                t.Errorf("%s(%q, %q) = nil; want %q", trimmer.name, tc.@in, tc.f.name, trimmer.@out);
            }
            if (actual != nil && trimmer.@out == nil) {
                t.Errorf("%s(%q, %q) = %q; want nil", trimmer.name, tc.@in, tc.f.name, actual);
            }
            if (!Equal(actual, trimmer.@out)) {
                t.Errorf("%s(%q, %q) = %q; want %q", trimmer.name, tc.@in, tc.f.name, actual, trimmer.@out);
            }
        }
    }
}

partial struct IndexFuncTest {
    internal @string @in;
    internal predicate f;
    internal nint first, last;
}

internal static slice<IndexFuncTest> indexFuncTests = slice<IndexFuncTest>([
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
    new(((@string)(new byte[]{0x61, 0xe0, 0x80, 0x63, 0x64})), not(isValidRune), 1, 2)
]);

public static void TestIndexFunc(ж<testing.T> t) {
    foreach (var (_, tc) in range(indexFuncTests)) {
        nint first = IndexFunc(slice<byte>(tc.@in), tc.f.f);
        if (first != tc.first) {
            t.Errorf("IndexFunc(%q, %s) = %d; want %d", tc.@in, tc.f.name, first, tc.first);
        }
        nint last = LastIndexFunc(slice<byte>(tc.@in), tc.f.f);
        if (last != tc.last) {
            t.Errorf("LastIndexFunc(%q, %s) = %d; want %d", tc.@in, tc.f.name, last, tc.last);
        }
    }
}

partial struct ReplaceTest {
    internal @string @in;
    internal @string old, @new;
    internal nint n;
    internal @string @out;
}

public static readonly ж<slice<ReplaceTest>> ᏑReplaceTests = new StaticBox<slice<ReplaceTest>>([
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
public static ref slice<ReplaceTest> ReplaceTests => ref ((StaticBox<slice<ReplaceTest>>)ᏑReplaceTests).ValueSlot;

public static void TestReplace(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(ReplaceTests)) {
        ref var tt = ref heap(new ReplaceTest(), out var Ꮡtt);
        tt = vᴛ1;

        ref var @in = ref heap<slice<byte>>(out var Ꮡin);

        @in = slice<byte>(tt.@in);
        slice<byte> old = slice<byte>(tt.old);
        slice<byte> @new = slice<byte>(tt.@new);
        if (!asan.Enabled) {
            var newʗ1 = @new;
            var oldʗ1 = old;
            var ttʗ1 = tt;
            var allocs = testing.AllocsPerRun(10, () => {
                Replace(Ꮡin.ValueSlot, oldʗ1, newʗ1, ttʗ1.n);
            });
            if (allocs > 1D) {
                t.Errorf("Replace(%q, %q, %q, %d) allocates %.2f objects", tt.@in, tt.old, tt.@new, tt.n, allocs);
            }
        }
        @in = append(@in, ((@string)"<spare>").ꓸꓸꓸ);
        @in = @in.slice(0, len(tt.@in));
        var @out = Replace(@in, old, @new, tt.n);
        {
            @string s = (@string)@out; if (s != tt.@out) {
                t.Errorf("Replace(%q, %q, %q, %d) = %q, want %q", tt.@in, tt.old, tt.@new, tt.n, s, tt.@out);
            }
        }
        if (cap(@in) == cap(@out) && Ꮡelem(@in[..1], 0) == Ꮡelem(@out[..1], 0)) {
            t.Errorf("Replace(%q, %q, %q, %d) didn't copy", tt.@in, tt.old, tt.@new, tt.n);
        }
        if (tt.n == -1) {
            var outΔ1 = ReplaceAll(@in, old, @new);
            @string s = (@string)outΔ1; if (s != tt.@out) {
                t.Errorf("ReplaceAll(%q, %q, %q) = %q, want %q", tt.@in, tt.old, tt.@new, s, tt.@out);
            }
        }
    }
}

public static void FuzzReplace(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tt) in range(ReplaceTests)) {
        f.Add(slice<byte>(tt.@in), slice<byte>(tt.old), slice<byte>(tt.@new), tt.n);
    }
    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> @in, slice<byte> old, slice<byte> @new, nint n) => {
        slice<byte> differentImpl(slice<byte> inΔ1, slice<byte> oldΔ1, slice<byte> newΔ1, nint nΔ1) {
            bytes.Buffer @out = default;
            if (nΔ1 < 0) {
                nΔ1 = Δmath.MaxInt;
            }
            for (nint i = 0; i < len(inΔ1); ) {
                if (nΔ1 == 0) {
                    @out.Write(inΔ1.slice(i));
                    break;
                }
                if (HasPrefix(inΔ1.slice(i), oldΔ1)) {
                    @out.Write(newΔ1);
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
                    var (_, length) = utf8.DecodeRune(inΔ1.slice(i));
                    @out.Write(inΔ1.slice(i, i + length));
                    i += length;
                } else {
                    @out.WriteByte(inΔ1[i]);
                    i++;
                }
            }
            if (len(oldΔ1) == 0 && nΔ1 != 0) {
                @out.Write(newΔ1);
            }
            return @out.Bytes();
        }
        var (simple, replace) = (differentImpl(@in, old, @new, n), Replace(@in, old, @new, n)); if (!slices.Equal<slice<byte>, byte>(simple, replace)) {
            t.Errorf("The two implementations do not match %q != %q for Replace(%q, %q, %q, %d)", simple, replace, @in, old, @new, n);
        }
    });
}

public static void BenchmarkReplace(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(ReplaceTests)) {
        ref var tt = ref heap(new ReplaceTest(), out var Ꮡtt);
        tt = vᴛ1;

        @string desc = fmt.Sprintf("%q %q %q %d"u8, tt.@in, tt.old, tt.@new, tt.n);
        slice<byte> @in = slice<byte>(tt.@in);
        slice<byte> old = slice<byte>(tt.old);
        slice<byte> @new = slice<byte>(tt.@new);
        var inʗ1 = @in;
        var newʗ1 = @new;
        var oldʗ1 = old;
        var ttʗ1 = tt;
        b.Run(desc, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            while (bΔ1.Loop()) {
                Replace(inʗ1, oldʗ1, newʗ1, ttʗ1.n);
            }
        });
    }
}

partial struct TitleTest {
    internal @string @in, @out;
}

public static readonly ж<slice<TitleTest>> ᏑTitleTests = new StaticBox<slice<TitleTest>>([
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
public static ref slice<TitleTest> TitleTests => ref ((StaticBox<slice<TitleTest>>)ᏑTitleTests).ValueSlot;

public static void TestTitle(ж<testing.T> t) {
    foreach (var (_, tt) in range(TitleTests)) {
        @string s = (@string)Title(slice<byte>(tt.@in)); if (s != tt.@out) {
            t.Errorf("Title(%q) = %q, want %q", tt.@in, s, tt.@out);
        }
    }
}

public static readonly ж<slice<TitleTest>> ᏑToTitleTests = new StaticBox<slice<TitleTest>>([
    new("", ""),
    new("a", "A"),
    new(" aaa aaa aaa ", " AAA AAA AAA "),
    new(" Aaa Aaa Aaa ", " AAA AAA AAA "),
    new("123a456", "123A456"),
    new("double-blind", "DOUBLE-BLIND"),
    new("ÿøû", "ŸØÛ")
]);
public static ref slice<TitleTest> ToTitleTests => ref ((StaticBox<slice<TitleTest>>)ᏑToTitleTests).ValueSlot;

public static void TestToTitle(ж<testing.T> t) {
    foreach (var (_, tt) in range(ToTitleTests)) {
        @string s = (@string)ToTitle(slice<byte>(tt.@in)); if (s != tt.@out) {
            t.Errorf("ToTitle(%q) = %q, want %q", tt.@in, s, tt.@out);
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
    new("abcdefghijKz", "abcdefghij\u212Ay", false)
]);
public static ref slice<EqualFoldTestsᴛ1> EqualFoldTests => ref ((StaticBox<slice<EqualFoldTestsᴛ1>>)ᏑEqualFoldTests).ValueSlot;

public static void TestEqualFold(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in range(EqualFoldTests)) {
        {
            var @out = EqualFold(slice<byte>(tt.s), slice<byte>(tt.t)); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v", tt.s, tt.t, @out, tt.@out);
            }
        }
        {
            var @out = EqualFold(slice<byte>(tt.t), slice<byte>(tt.s)); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v", tt.t, tt.s, @out, tt.@out);
            }
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
        var (before, after, found) = Cut(slice<byte>(tt.s), slice<byte>(tt.sep)); if (((sstring)before) != tt.before || ((sstring)after) != tt.after || found != tt.found) {
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
        var (after, found) = CutPrefix(slice<byte>(tt.s), slice<byte>(tt.sep)); if (((sstring)after) != tt.after || found != tt.found) {
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
        var (before, found) = CutSuffix(slice<byte>(tt.s), slice<byte>(tt.sep)); if (((sstring)before) != tt.before || found != tt.found) {
            t.Errorf("CutSuffix(%q, %q) = %q, %v, want %q, %v", tt.s, tt.sep, before, found, tt.before, tt.found);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object grow1ShouldHavePanickedˢ = (@string)"Grow(-1) should have panicked"u8;

public static void TestBufferGrowNegative(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var err = recover(); if (err == nil) {
                t.Fatal(grow1ShouldHavePanickedˢ);
            }
        }, ref ᒐ);
        bytes.Buffer b = default;
        b.Grow(-1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object truncate1ShouldHaveˢ = (@string)"Truncate(-1) should have panicked"u8;

public static void TestBufferTruncateNegative(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var err = recover(); if (err == nil) {
                t.Fatal(truncate1ShouldHaveˢ);
            }
        }, ref ᒐ);
        bytes.Buffer b = default;
        b.Truncate(-1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object truncate20ShouldHaveˢ = (@string)"Truncate(20) should have panicked"u8;

public static void TestBufferTruncateOutOfRange(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var err = recover(); if (err == nil) {
                t.Fatal(truncate20ShouldHaveˢ);
            }
        }, ref ᒐ);
        bytes.Buffer b = default;
        b.Write(new slice<byte>(10));
        b.Truncate(20);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct containsTestsᴛ1 /*dyn*/ {
    internal slice<byte> b, subslice;
    internal bool want;
}
internal static slice<containsTestsᴛ1> containsTests = [
    new(slice<byte>("hello"u8), slice<byte>("hel"u8), true),
    new(slice<byte>("日本語"u8), slice<byte>("日本"u8), true),
    new(slice<byte>("hello"u8), slice<byte>("Hello, world"u8), false),
    new(slice<byte>("東京"u8), slice<byte>("京東"u8), false)
];

public static void TestContains(ж<testing.T> t) {
    foreach (var (_, tt) in range(containsTests)) {
        var got = Contains(tt.b, tt.subslice); if (got != tt.want) {
            t.Errorf("Contains(%q, %q) = %v, want %v", tt.b, tt.subslice, got, tt.want);
        }
    }
}

partial struct ContainsAnyTestsᴛ1 /*dyn*/ {
    internal slice<byte> b;
    internal @string substr;
    internal bool expected;
}
public static readonly ж<slice<ContainsAnyTestsᴛ1>> ᏑContainsAnyTests = new StaticBox<slice<ContainsAnyTestsᴛ1>>([
    new(slice<byte>(""u8), "", false),
    new(slice<byte>(""u8), "a", false),
    new(slice<byte>(""u8), "abc", false),
    new(slice<byte>("a"u8), "", false),
    new(slice<byte>("a"u8), "a", true),
    new(slice<byte>("aaa"u8), "a", true),
    new(slice<byte>("abc"u8), "xyz", false),
    new(slice<byte>("abc"u8), "xcz", true),
    new(slice<byte>("a☺b☻c☹d"u8), "uvw☻xyz", true),
    new(slice<byte>("aRegExp*"u8), ".(|)*+?^$[]", true),
    new(slice<byte>(dots + dots + dots), " ", false)
]);
public static ref slice<ContainsAnyTestsᴛ1> ContainsAnyTests => ref ((StaticBox<slice<ContainsAnyTestsᴛ1>>)ᏑContainsAnyTests).ValueSlot;

public static void TestContainsAny(ж<testing.T> t) {
    foreach (var (_, ct) in range(ContainsAnyTests)) {
        if (ContainsAny(ct.b, ct.substr) != ct.expected) {
            t.Errorf("ContainsAny(%s, %s) = %v, want %v",
                ct.b, ct.substr, !ct.expected, ct.expected);
        }
    }
}

partial struct ContainsRuneTestsᴛ1 /*dyn*/ {
    internal slice<byte> b;
    internal rune r;
    internal bool expected;
}
public static readonly ж<slice<ContainsRuneTestsᴛ1>> ᏑContainsRuneTests = new StaticBox<slice<ContainsRuneTestsᴛ1>>([
    new(slice<byte>(""u8), 'a', false),
    new(slice<byte>("a"u8), 'a', true),
    new(slice<byte>("aaa"u8), 'a', true),
    new(slice<byte>("abc"u8), 'y', false),
    new(slice<byte>("abc"u8), 'c', true),
    new(slice<byte>("a☺b☻c☹d"u8), 'x', false),
    new(slice<byte>("a☺b☻c☹d"u8), '☻', true),
    new(slice<byte>("aRegExp*"u8), '*', true)
]);
public static ref slice<ContainsRuneTestsᴛ1> ContainsRuneTests => ref ((StaticBox<slice<ContainsRuneTestsᴛ1>>)ᏑContainsRuneTests).ValueSlot;

public static void TestContainsRune(ж<testing.T> t) {
    foreach (var (_, ct) in range(ContainsRuneTests)) {
        if (ContainsRune(ct.b, ct.r) != ct.expected) {
            t.Errorf("ContainsRune(%q, %q) = %v, want %v",
                ct.b, ct.r, !ct.expected, ct.expected);
        }
    }
}

public static void TestContainsFunc(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(ContainsRuneTests)) {
        ref var ct = ref heap(new ContainsRuneTestsᴛ1(), out var Ꮡct);
        ct = vᴛ1;

        var ctʗ1 = ct;
        if (ContainsFunc(ct.b, (rune r) => ctʗ1.r == r) != ct.expected) {
            t.Errorf("ContainsFunc(%q, func(%q)) = %v, want %v",
                ct.b, ct.r, !ct.expected, ct.expected);
        }
    }
}

internal static Func<slice<byte>> makeFieldsInput = () => {
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
    return x;
};

internal static Func<slice<byte>> makeFieldsInputASCII = () => {
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
    return x;
};

partial struct bytesdataᴛ1 /*dyn*/ {
    internal @string name;
    internal slice<byte> data;
}
internal static slice<bytesdataᴛ1> bytesdata = slice<bytesdataᴛ1>([
    new("ASCII", makeFieldsInputASCII()),
    new("Mixed", makeFieldsInput())
]);

public static void BenchmarkFields(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(bytesdata)) {
        ref var sd = ref heap(new bytesdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        b.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = 1 << 4; jᴛ1 <= 1 << 20; jᴛ1 <<= 4) {
                var j = jᴛ1;
                var sdʗ2 = sdʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, j), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    bΔ2.SetBytes((int64)j);
                    var data = sdʗ2.data.slice(0, j);
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        Fields(data);
                    }
                });
            }
        });
    }
}

public static void BenchmarkFieldsFunc(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in range(bytesdata)) {
        ref var sd = ref heap(new bytesdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        b.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = 1 << 4; jᴛ1 <= 1 << 20; jᴛ1 <<= 4) {
                var j = jᴛ1;
                var sdʗ2 = sdʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, j), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    bΔ2.SetBytes((int64)j);
                    var data = sdʗ2.data.slice(0, j);
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        FieldsFunc(data, Δunicode.IsSpace);
                    }
                });
            }
        });
    }
}

internal partial struct BenchmarkTrimSpace_tests /*dyn*/ {
    internal @string name;
    internal slice<byte> input;
}

public static void BenchmarkTrimSpace(ж<testing.B> b) {
    var tests = slice<BenchmarkTrimSpace_tests>([
        new("NoTrim", slice<byte>("typical"u8)),
        new("ASCII", slice<byte>("  foo bar  "u8)),
        new("SomeNonASCII", slice<byte>("    \u2000\t\r\n x\t\t\r\r\ny\n \u3000    "u8)),
        new("JustNonASCII", slice<byte>("\u2000\u2000\u2000☺☺☺☺\u3000\u3000\u3000"u8))
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

internal partial struct BenchmarkToValidUTF8_tests /*dyn*/ {
    internal @string name;
    internal slice<byte> input;
}

public static void BenchmarkToValidUTF8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var tests = slice<BenchmarkToValidUTF8_tests>([
        new("Valid", slice<byte>("typical"u8)),
        new("InvalidASCII", slice<byte>(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xff, 0x62, 0x61, 0x72})))),
        new("InvalidNonASCII", slice<byte>(((@string)(new byte[]{0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e, 0xff, 0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e}))))
    ]);
    var replacement = slice<byte>("\uFFFD"u8);
    b.ResetTimer();
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new BenchmarkToValidUTF8_tests(), out var Ꮡtest);
        test = vᴛ1;

        var replacementʗ1 = replacement;
        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                ToValidUTF8(testʗ1.input, replacementʗ1);
            }
        });
    }
}

internal static slice<byte> makeBenchInputHard() {
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
    return x;
}

internal static slice<byte> benchInputHard = makeBenchInputHard();

internal static void benchmarkIndexHard(ж<testing.B> Ꮡb, slice<byte> sep) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint n = Index(benchInputHard, sep);
    if (n < 0) {
        n = len(benchInputHard);
    }
    b.SetBytes((int64)n);
    for (nint i = 0; i < b.N; i++) {
        Index(benchInputHard, sep);
    }
}

internal static void benchmarkLastIndexHard(ж<testing.B> Ꮡb, slice<byte> sep) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        LastIndex(benchInputHard, sep);
    }
}

internal static void benchmarkCountHard(ж<testing.B> Ꮡb, slice<byte> sep) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Count(benchInputHard, sep);
    }
}

public static void BenchmarkIndexHard1(ж<testing.B> b) { benchmarkIndexHard(b, slice<byte>("<>"u8)); }
public static void BenchmarkIndexHard2(ж<testing.B> b) { benchmarkIndexHard(b, slice<byte>("</pre>"u8)); }
public static void BenchmarkIndexHard3(ж<testing.B> b) { benchmarkIndexHard(b, slice<byte>("<b>hello world</b>"u8)); }
public static void BenchmarkIndexHard4(ж<testing.B> b) {
    benchmarkIndexHard(b, slice<byte>("<pre><b>hello</b><strong>world</strong></pre>"u8));
}

public static void BenchmarkLastIndexHard1(ж<testing.B> b) { benchmarkLastIndexHard(b, slice<byte>("<>"u8)); }
public static void BenchmarkLastIndexHard2(ж<testing.B> b) { benchmarkLastIndexHard(b, slice<byte>("</pre>"u8)); }
public static void BenchmarkLastIndexHard3(ж<testing.B> b) { benchmarkLastIndexHard(b, slice<byte>("<b>hello world</b>"u8)); }

public static void BenchmarkCountHard1(ж<testing.B> b) { benchmarkCountHard(b, slice<byte>("<>"u8)); }
public static void BenchmarkCountHard2(ж<testing.B> b) { benchmarkCountHard(b, slice<byte>("</pre>"u8)); }
public static void BenchmarkCountHard3(ж<testing.B> b) { benchmarkCountHard(b, slice<byte>("<b>hello world</b>"u8)); }

public static void BenchmarkSplitEmptySeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, default);
    }
}

public static void BenchmarkSplitSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var sep = slice<byte>("/"u8);
    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, sep);
    }
}

public static void BenchmarkSplitMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var sep = slice<byte>("hello"u8);
    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, sep);
    }
}

public static void BenchmarkSplitNSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var sep = slice<byte>("/"u8);
    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, sep, 10);
    }
}

public static void BenchmarkSplitNMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var sep = slice<byte>("hello"u8);
    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, sep, 10);
    }
}

public static void BenchmarkRepeat(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Repeat(slice<byte>("-"u8), 80);
    }
}

public static void BenchmarkRepeatLarge(ж<testing.B> b) {
    var s = Repeat(slice<byte>("@"u8), 8 * 1024);
    for (nint j = 8; j <= 30; j++) {
        foreach (var (_, k) in range(slice<nint>([1, 16, 4097]))) {
            var sΔ1 = s.slice(0, k);
            nint n = quo((((nint)1).Lsh((int64)(j))), k);
            if (n == 0) {
                continue;
            }
            var sʗ1 = sΔ1;
            b.Run(fmt.Sprintf("%d/%d"u8, ((nint)1).Lsh((int64)(j)), k), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Repeat(sʗ1, n);
                }
                bΔ1.SetBytes((int64)(n * len(sʗ1)));
            });
        }
    }
}

public static void BenchmarkBytesCompare(ж<testing.B> b) {
    for (nint nᴛ1 = 1; nᴛ1 <= 2048; nᴛ1 <<= 1) {
        var n = nᴛ1;
        b.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            slice<byte> x = new slice<byte>(n);
            slice<byte> y = new slice<byte>(n);

            for (nint i = 0; i < n; i++) {
                x[i] = (byte)'a';
            }

            for (nint i = 0; i < n; i++) {
                y[i] = (byte)'a';
            }

            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Compare(x, y);
            }
        });
    }
}

public static void BenchmarkIndexAnyASCII(ж<testing.B> b) {
    var x = Repeat([(byte)'#'], 2048); // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz";
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            var xʗ1 = x;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(xʗ1.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldHelloWorldˢ => ref helloWorldHelloWorldˢᵇ;
private static readonly @string helloWorldHelloWorldˢᵇ = "你好世界, hello world. 你好世界, hello world. 你好世界, hello world."u8;

public static void BenchmarkIndexAnyUTF8(ж<testing.B> b) {
    var x = Repeat([(byte)'#'], 2048); // Never matches set
    @string cs = helloWorldHelloWorldˢ;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            var xʗ1 = x;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(xʗ1.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyASCII(ж<testing.B> b) {
    var x = Repeat([(byte)'#'], 2048); // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz";
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            var xʗ1 = x;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(xʗ1.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyUTF8(ж<testing.B> b) {
    var x = Repeat([(byte)'#'], 2048); // Never matches set
    @string cs = helloWorldHelloWorldˢ;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= 4) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= 1) {
            var j = jᴛ1;
            var xʗ1 = x;
            b.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(xʗ1.slice(0, k), cs.slice(0, j));
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
                var x = Repeat(slice<byte>(cs.slice(0, j)), k); // Always matches set
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Trim(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkTrimByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = slice<byte>("  the quick brown fox   "u8);
    for (nint i = 0; i < b.N; i++) {
        Trim(x, " ");
    }
}

public static void BenchmarkIndexPeriodic(ж<testing.B> b) {
    var key = slice<byte>([1, 1]);
    foreach (var (_, skip) in range(new nint[]{2, 4, 8, 16, 32, 64}.array())) {
        var keyʗ1 = key;
        b.Run(fmt.Sprintf("IndexPeriodic%d"u8, skip), (ж<testing.B> bΔ1) => {
            var buf = new slice<byte>((1 << 16));
            for (nint i = 0; i < len(buf); i += skip) {
                buf[i] = 1;
            }
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Index(buf, keyʗ1);
            }
        });
    }
}

public static void TestClone(ж<testing.T> t) {
    slice<slice<byte>> cloneTests = slice<slice<byte>>([
        slice<byte>(default),
        [],
        Clone([]),
        slice<byte>(strings.Repeat("a", 42))[..0],
        slice<byte>(strings.Repeat("a", 42)).slice(0, 0, 0),
        slice<byte>("short"u8),
        slice<byte>(strings.Repeat("a", 42))
    ]);
    foreach (var (_, input) in range(cloneTests)) {
        var clone = Clone(input);
        if (!Equal(clone, input)) {
            t.Errorf("Clone(%q) = %q; want %q", input, clone, input);
        }

        if (input == nil && clone != nil) {
            t.Errorf("Clone(%#v) return value should be equal to nil slice.", input);
        }

        if (input != nil && clone == nil) {
            t.Errorf("Clone(%#v) return value should not be equal to nil slice.", input);
        }

        if (cap(input) != 0 && @unsafe.SliceData(input) == @unsafe.SliceData(clone)) {
            t.Errorf("Clone(%q) return value should not reference inputs backing memory.", input);
        }
    }
}

} // end bytes_test_package
