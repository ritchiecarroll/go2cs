// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
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
using go.math;
using go.unicode;
using static go.strings_internal_test_package;
using strings = strings_package;

partial class strings_test_package {

internal static slice<@string> collect(ж<testing.T> Ꮡt, iter.Seq<@string> seq) {
    var @out = slices.Collect(seq);
    var out1 = slices.Collect(seq);
    if (!slices.Equal<slice<@string>, @string>(@out, out1)) {
        Ꮡt.Fatalf("inconsistent seq:\n%s\n%s"u8, @out, out1);
    }
    return @out;
}

partial struct LinesTest {
    internal @string a;
    internal slice<@string> b;
}

internal static slice<LinesTest> linesTests = new LinesTest[]{
    new(a: "abc\nabc\n"u8, b: new @string[]{"abc\n"u8, "abc\n"u8}.slice()),
    new(a: "abc\r\nabc"u8, b: new @string[]{"abc\r\n"u8, "abc"u8}.slice()),
    new(a: "abc\r\n"u8, b: new @string[]{"abc\r\n"u8}.slice()),
    new(a: "\nabc"u8, b: new @string[]{"\n"u8, "abc"u8}.slice()),
    new(a: "\nabc\n\n"u8, b: new @string[]{"\n"u8, "abc\n"u8, "\n"u8}.slice())
}.slice();

public static void TestLines(ж<testing.T> Ꮡt) {
    foreach (var (_, s) in linesTests) {
        var result = slices.Collect(Lines(s.a));
        if (!slices.Equal<slice<@string>, @string>(result, s.b)) {
            Ꮡt.Errorf(@"slices.Collect(Lines(%q)) = %q; want %q"u8, s.a, result, s.b);
        }
    }
}

internal static @string abcd = "abcd"u8;

internal static @string faces = "☺☻☹"u8;

internal static @string commas = "1,2,3,4"u8;

internal static @string dots = "1....2....3....4"u8;

partial struct IndexTest {
    internal @string s;
    internal @string sep;
    internal nint @out;
}

// cases with one byte strings - test special case in Index()
// test special cases in Index() for short strings
// test fallback to Rabin-Karp.
// test fallback to IndexRune
// invalid UTF-8 byte sequence (must be longer than bytealg.MaxBruteForce to
// test that we don't use IndexRune)
internal static slice<IndexTest> indexTests = new IndexTest[]{
    new(""u8, ""u8, 0),
    new(""u8, "a"u8, -1),
    new(""u8, "foo"u8, -1),
    new("fo"u8, "foo"u8, -1),
    new("foo"u8, "foo"u8, 0),
    new("oofofoofooo"u8, "f"u8, 2),
    new("oofofoofooo"u8, "foo"u8, 4),
    new("barfoobarfoo"u8, "foo"u8, 3),
    new("foo"u8, ""u8, 0),
    new("foo"u8, "o"u8, 1),
    new("abcABCabc"u8, "A"u8, 3),
    new("jrzm6jjhorimglljrea4w3rlgosts0w2gia17hno2td4qd1jz"u8, "jz"u8, 47),
    new("ekkuk5oft4eq0ocpacknhwouic1uua46unx12l37nioq9wbpnocqks6"u8, "ks6"u8, 52),
    new("999f2xmimunbuyew5vrkla9cpwhmxan8o98ec"u8, "98ec"u8, 33),
    new("9lpt9r98i04k8bz6c6dsrthb96bhi"u8, "96bhi"u8, 24),
    new("55u558eqfaod2r2gu42xxsu631xf0zobs5840vl"u8, "5840vl"u8, 33),
    new(""u8, "a"u8, -1),
    new("x"u8, "a"u8, -1),
    new("x"u8, "x"u8, 0),
    new("abc"u8, "a"u8, 0),
    new("abc"u8, "b"u8, 1),
    new("abc"u8, "c"u8, 2),
    new("abc"u8, "x"u8, -1),
    new(""u8, "ab"u8, -1),
    new("bc"u8, "ab"u8, -1),
    new("ab"u8, "ab"u8, 0),
    new("xab"u8, "ab"u8, 1),
    new("xab"u8[..2], "ab"u8, -1),
    new(""u8, "abc"u8, -1),
    new("xbc"u8, "abc"u8, -1),
    new("abc"u8, "abc"u8, 0),
    new("xabc"u8, "abc"u8, 1),
    new("xabc"u8[..3], "abc"u8, -1),
    new("xabxc"u8, "abc"u8, -1),
    new(""u8, "abcd"u8, -1),
    new("xbcd"u8, "abcd"u8, -1),
    new("abcd"u8, "abcd"u8, 0),
    new("xabcd"u8, "abcd"u8, 1),
    new("xyabcd"u8[..5], "abcd"u8, -1),
    new("xbcqq"u8, "abcqq"u8, -1),
    new("abcqq"u8, "abcqq"u8, 0),
    new("xabcqq"u8, "abcqq"u8, 1),
    new("xyabcqq"u8[..6], "abcqq"u8, -1),
    new("xabxcqq"u8, "abcqq"u8, -1),
    new("xabcqxq"u8, "abcqq"u8, -1),
    new(""u8, "01234567"u8, -1),
    new("32145678"u8, "01234567"u8, -1),
    new("01234567"u8, "01234567"u8, 0),
    new("x01234567"u8, "01234567"u8, 1),
    new("x0123456x01234567"u8, "01234567"u8, 9),
    new("xx01234567"u8[..9], "01234567"u8, -1),
    new(""u8, "0123456789"u8, -1),
    new("3214567844"u8, "0123456789"u8, -1),
    new("0123456789"u8, "0123456789"u8, 0),
    new("x0123456789"u8, "0123456789"u8, 1),
    new("x012345678x0123456789"u8, "0123456789"u8, 11),
    new("xyz0123456789"u8[..12], "0123456789"u8, -1),
    new("x01234567x89"u8, "0123456789"u8, -1),
    new(""u8, "0123456789012345"u8, -1),
    new("3214567889012345"u8, "0123456789012345"u8, -1),
    new("0123456789012345"u8, "0123456789012345"u8, 0),
    new("x0123456789012345"u8, "0123456789012345"u8, 1),
    new("x012345678901234x0123456789012345"u8, "0123456789012345"u8, 17),
    new(""u8, "01234567890123456789"u8, -1),
    new("32145678890123456789"u8, "01234567890123456789"u8, -1),
    new("01234567890123456789"u8, "01234567890123456789"u8, 0),
    new("x01234567890123456789"u8, "01234567890123456789"u8, 1),
    new("x0123456789012345678x01234567890123456789"u8, "01234567890123456789"u8, 21),
    new("xyz01234567890123456789"u8[..22], "01234567890123456789"u8, -1),
    new(""u8, "0123456789012345678901234567890"u8, -1),
    new("321456788901234567890123456789012345678911"u8, "0123456789012345678901234567890"u8, -1),
    new("0123456789012345678901234567890"u8, "0123456789012345678901234567890"u8, 0),
    new("x0123456789012345678901234567890"u8, "0123456789012345678901234567890"u8, 1),
    new("x012345678901234567890123456789x0123456789012345678901234567890"u8, "0123456789012345678901234567890"u8, 32),
    new("xyz0123456789012345678901234567890"u8[..33], "0123456789012345678901234567890"u8, -1),
    new(""u8, "01234567890123456789012345678901"u8, -1),
    new("32145678890123456789012345678901234567890211"u8, "01234567890123456789012345678901"u8, -1),
    new("01234567890123456789012345678901"u8, "01234567890123456789012345678901"u8, 0),
    new("x01234567890123456789012345678901"u8, "01234567890123456789012345678901"u8, 1),
    new("x0123456789012345678901234567890x01234567890123456789012345678901"u8, "01234567890123456789012345678901"u8, 33),
    new("xyz01234567890123456789012345678901"u8[..34], "01234567890123456789012345678901"u8, -1),
    new("xxxxxx012345678901234567890123456789012345678901234567890123456789012"u8, "012345678901234567890123456789012345678901234567890123456789012"u8, 6),
    new(""u8, "0123456789012345678901234567890123456789"u8, -1),
    new("xx012345678901234567890123456789012345678901234567890123456789012"u8, "0123456789012345678901234567890123456789"u8, 2),
    new("xx012345678901234567890123456789012345678901234567890123456789012"u8[..41], "0123456789012345678901234567890123456789"u8, -1),
    new("xx012345678901234567890123456789012345678901234567890123456789012"u8, "0123456789012345678901234567890123456xxx"u8, -1),
    new("xx0123456789012345678901234567890123456789012345678901234567890120123456789012345678901234567890123456xxx"u8, "0123456789012345678901234567890123456xxx"u8, 65),
    new("oxoxoxoxoxoxoxoxoxoxoxoy"u8, "oy"u8, 22),
    new("oxoxoxoxoxoxoxoxoxoxoxox"u8, "oy"u8, -1),
    new("oxoxoxoxoxoxoxoxoxoxox☺"u8, "☺"u8, 22),
    new(((@string)(new byte[]{0x78, 0x78, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x78, 0x78, 0x78, 0xed, 0x9f, 0xc0})), ((@string)(new byte[]{0xed, 0x9f, 0xc0})), 105)
}.slice();

internal static slice<IndexTest> lastIndexTests = new IndexTest[]{
    new(""u8, ""u8, 0),
    new(""u8, "a"u8, -1),
    new(""u8, "foo"u8, -1),
    new("fo"u8, "foo"u8, -1),
    new("foo"u8, "foo"u8, 0),
    new("foo"u8, "f"u8, 0),
    new("oofofoofooo"u8, "f"u8, 7),
    new("oofofoofooo"u8, "foo"u8, 7),
    new("barfoobarfoo"u8, "foo"u8, 9),
    new("foo"u8, ""u8, 3),
    new("foo"u8, "o"u8, 2),
    new("abcABCabc"u8, "A"u8, 3),
    new("abcABCabc"u8, "a"u8, 6)
}.slice();

internal static slice<IndexTest> indexAnyTests = new IndexTest[]{
    new(""u8, ""u8, -1),
    new(""u8, "a"u8, -1),
    new(""u8, "abc"u8, -1),
    new("a"u8, ""u8, -1),
    new("a"u8, "a"u8, 0),
    new(((@string)(new byte[]{0x80})), ((@string)(new byte[]{0xff, 0x62})), 0),
    new("aaa"u8, "a"u8, 0),
    new("abc"u8, "xyz"u8, -1),
    new("abc"u8, "xcz"u8, 2),
    new("ab☺c"u8, "x☺yz"u8, 2),
    new("a☺b☻c☹d"u8, "cx"u8, len("a☺b☻")),
    new("a☺b☻c☹d"u8, "uvw☻xyz"u8, len("a☺b")),
    new("aRegExp*"u8, ".(|)*+?^$[]"u8, 7),
    new(dots + dots + dots, " "u8, -1),
    new("012abcba210"u8, ((@string)(new byte[]{0xff, 0x62})), 4),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x80, 0x62, 0x63, 0x62, 0x80, 0x32, 0x31, 0x30})), ((@string)(new byte[]{0xff, 0x62})), 3),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0xcf, 0x80, 0x61, 0x62, 0x63})), ((@string)(new byte[]{0xcf, 0x62, 0x80})), 10)
}.slice();

internal static slice<IndexTest> lastIndexAnyTests = new IndexTest[]{
    new(""u8, ""u8, -1),
    new(""u8, "a"u8, -1),
    new(""u8, "abc"u8, -1),
    new("a"u8, ""u8, -1),
    new("a"u8, "a"u8, 0),
    new(((@string)(new byte[]{0x80})), ((@string)(new byte[]{0xff, 0x62})), 0),
    new("aaa"u8, "a"u8, 2),
    new("abc"u8, "xyz"u8, -1),
    new("abc"u8, "ab"u8, 1),
    new("ab☺c"u8, "x☺yz"u8, 2),
    new("a☺b☻c☹d"u8, "cx"u8, len("a☺b☻")),
    new("a☺b☻c☹d"u8, "uvw☻xyz"u8, len("a☺b")),
    new("a.RegExp*"u8, ".(|)*+?^$[]"u8, 8),
    new(dots + dots + dots, " "u8, -1),
    new("012abcba210"u8, ((@string)(new byte[]{0xff, 0x62})), 6),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x80, 0x62, 0x63, 0x62, 0x80, 0x32, 0x31, 0x30})), ((@string)(new byte[]{0xff, 0x62})), 7),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0xcf, 0x80, 0x61, 0x62, 0x63})), ((@string)(new byte[]{0xcf, 0x62, 0x80})), 10)
}.slice();

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runIndexTests(ж<testing.T> Ꮡt, Func<@string, @string, nint> f, @string funcName, slice<IndexTest> testCases) {
    foreach (var (_, test) in testCases) {
        nint actual = f(test.s, test.sep);
        if (actual != test.@out) {
            Ꮡt.Errorf("%s(%q,%q) = %v; want %v"u8, funcName, test.s, test.sep, actual, test.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string indexˢ = "Index"u8;

public static void TestIndex(ж<testing.T> Ꮡt) {
    runIndexTests(Ꮡt, Index, indexˢ, indexTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lastIndexˢ = "LastIndex"u8;

public static void TestLastIndex(ж<testing.T> Ꮡt) {
    runIndexTests(Ꮡt, LastIndex, lastIndexˢ, lastIndexTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string indexAnyˢ = "IndexAny"u8;

public static void TestIndexAny(ж<testing.T> Ꮡt) {
    runIndexTests(Ꮡt, IndexAny, indexAnyˢ, indexAnyTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lastIndexAnyˢ = "LastIndexAny"u8;

public static void TestLastIndexAny(ж<testing.T> Ꮡt) {
    runIndexTests(Ꮡt, LastIndexAny, lastIndexAnyˢ, lastIndexAnyTests);
}

public static void TestIndexByte(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in indexTests) {
        if (len(tt.sep) != 1) {
            continue;
        }
        nint pos = IndexByte(tt.s, tt.sep[0]);
        if (pos != tt.@out) {
            Ꮡt.Errorf(@"IndexByte(%q, %q) = %v; want %v"u8, tt.s, tt.sep[0], pos, tt.@out);
        }
    }
}

public static void TestLastIndexByte(ж<testing.T> Ꮡt) {
    var testCases = new IndexTest[]{
        new(""u8, "q"u8, -1),
        new("abcdef"u8, "q"u8, -1),
        new("abcdefabcdef"u8, "a"u8, len("abcdef")), // something in the middle

        new("abcdefabcdef"u8, "f"u8, len("abcdefabcde")), // last byte

        new("zabcdefabcdef"u8, "z"u8, 0), // first byte

        new("a☺b☻c☹d"u8, "b"u8, len("a☺"))
    }.slice();
    // non-ascii
    foreach (var (_, test) in testCases) {
        nint actual = LastIndexByte(test.s, test.sep[0]);
        if (actual != test.@out) {
            Ꮡt.Errorf("LastIndexByte(%q,%c) = %v; want %v"u8, test.s, test.sep[0], actual, test.@out);
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
internal static readonly @string charsᶜ = "abcdefghijklmnopqrstuvwxyz0123456789"u8;

public static void TestIndexRandom(ж<testing.T> Ꮡt) {
    @string chars = charsᶜ;
    for (nint times = 0; times < 10; times++) {
        for (nint strLen = 5 + rand.Intn(5); strLen < 140; strLen += 10) {
            // Arbitrary
            var s1 = new slice<byte>(strLen);
            foreach (var (i, _) in s1) {
                s1[i] = chars[rand.Intn(len(chars))];
            }
            @string s = ((@string)s1);
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
                    Ꮡt.Errorf("Index(%s,%s) = %d; want %d"u8, s, sep, res, want);
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

public static void TestIndexRune(ж<testing.T> Ꮡt) {
    var tests = new TestIndexRune_tests[]{
        new(""u8, (rune)'a', -1),
        new(""u8, (rune)'☺', -1),
        new("foo"u8, (rune)'☹', -1),
        new("foo"u8, (rune)'o', 1),
        new("foo☺bar"u8, (rune)'☺', 3),
        new("foo☺☻☹bar"u8, (rune)'☹', 9),
        new("a A x"u8, (rune)'A', 2),
        new("some_text=some_value"u8, (rune)'=', 9),
        new("☺a"u8, (rune)'a', 3),
        new("a☻☺b"u8, (rune)'☺', 4), // RuneError should match any invalid UTF-8 byte sequence.

        new("�"u8, (rune)'�', 0),
        new(((@string)(new byte[]{0xff})), (rune)'�', 0),
        new("☻x�"u8, (rune)'�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98})), (rune)'�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0xef, 0xbf, 0xbd})), (rune)'�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0x78})), (rune)'�', len("☻x")), // Invalid rune values should never match.

        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), -1, -1),
        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), 0xD800, -1), // Surrogate pair

        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), utf8.MaxRune + 1, -1), // 2 bytes

        new("ӆ"u8, (rune)'ӆ', 0),
        new("a"u8, (rune)'ӆ', -1),
        new("  ӆ"u8, (rune)'ӆ', 2),
        new("  a"u8, (rune)'ӆ', -1),
        new(Repeat("ц"u8, 64) + "ӆ"u8, (rune)'ӆ', 128), // test cutover

        new(Repeat("Ꙁ"u8, 64) + "Ꚁ"u8, (rune)'䚀', -1), // 'Ꚁ' and '䚀' share the same last two bytes
 // 3 bytes

        new("Ꚁ"u8, (rune)'Ꚁ', 0),
        new("a"u8, (rune)'Ꚁ', -1),
        new("  Ꚁ"u8, (rune)'Ꚁ', 2),
        new("  a"u8, (rune)'Ꚁ', -1),
        new(Repeat("Ꙁ"u8, 64) + "Ꚁ"u8, (rune)'Ꚁ', 192), // test cutover

        new(Repeat("𡋀"u8, 64) + "𡌀"u8, (rune)0x23300, -1), // '𡌀' and '𣌀' share the same last two bytes
 // 4 bytes

        new("𡌀"u8, (rune)0x21300, 0),
        new("a"u8, (rune)0x21300, -1),
        new("  𡌀"u8, (rune)0x21300, 2),
        new("  a"u8, (rune)0x21300, -1),
        new(Repeat("𡋀"u8, 64) + "𡌀"u8, (rune)0x21300, 256), // test cutover

        new(Repeat("𡋀"u8, 64), (rune)0x21300, -1), // Test the cutover to bytealg.IndexString when it is triggered in
 // the middle of rune that contains consecutive runs of equal bytes.

        new("aaaaaKKKK\U000bc104"u8, (rune)0xBC104, 17), // cutover: (n + 16) / 8

        new("aaaaaKKKK鄄"u8, (rune)'鄄', 17),
        new("aaKKKKKa\U000bc104"u8, (rune)0xBC104, 18), // cutover: 4 + n>>4

        new("aaKKKKKa鄄"u8, (rune)'鄄', 18)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            nint got = IndexRune(tt.@in, tt.rune); if (got != tt.want) {
                Ꮡt.Errorf("IndexRune(%q, %d) = %v; want %v"u8, tt.@in, tt.rune, got, tt.want);
            }
        }
    }
    // Make sure we trigger the cutover and string(rune) conversion.
    @string haystack = "test"u8 + Repeat("𡋀"u8, 32) + "𡌀"u8;
    var allocs = testing.AllocsPerRun(1000, () => {
        {
            nint i = IndexRune(haystack, (rune)'s'); if (i != 2) {
                Ꮡt.Fatalf("'s' at %d; want 2"u8, i);
            }
        }
        {
            nint i = IndexRune(haystack, (rune)0x21300); if (i != 132) {
                Ꮡt.Fatalf("'𡌀' at %d; want 4"u8, i);
            }
        }
    });
    if (allocs != 0D && testing.CoverMode() == ""u8) {
        Ꮡt.Errorf("expected no allocations, got %f"u8, allocs);
    }
}

internal static readonly @string benchmarkString = "some_text=some☺value"u8;

public static void BenchmarkIndexRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        nint got = IndexRune(benchmarkString, (rune)'☺'); if (got != 14) {
            Ꮡb.Fatalf("wrong index: expected 14, got=%d"u8, got);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        IndexRune(benchmarkString, (rune)'☺');
    }
}

internal static @string benchmarkLongString = Repeat(" "u8, 100) + benchmarkString;

public static void BenchmarkIndexRuneLongString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        nint got = IndexRune(benchmarkLongString, (rune)'☺'); if (got != 114) {
            Ꮡb.Fatalf("wrong index: expected 114, got=%d"u8, got);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        IndexRune(benchmarkLongString, (rune)'☺');
    }
}

public static void BenchmarkIndexRuneFastPath(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        nint got = IndexRune(benchmarkString, (rune)'v'); if (got != 17) {
            Ꮡb.Fatalf("wrong index: expected 17, got=%d"u8, got);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        IndexRune(benchmarkString, (rune)'v');
    }
}

public static void BenchmarkIndex(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        nint got = Index(benchmarkString, "v"u8); if (got != 17) {
            Ꮡb.Fatalf("wrong index: expected 17, got=%d"u8, got);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        Index(benchmarkString, "v"u8);
    }
}

public static void BenchmarkLastIndex(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        nint got = Index(benchmarkString, "v"u8); if (got != 17) {
            Ꮡb.Fatalf("wrong index: expected 17, got=%d"u8, got);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        LastIndex(benchmarkString, "v"u8);
    }
}

public static void BenchmarkIndexByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    {
        nint got = IndexByte(benchmarkString, (rune)'v'); if (got != 17) {
            Ꮡb.Fatalf("wrong index: expected 17, got=%d"u8, got);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        IndexByte(benchmarkString, (rune)'v');
    }
}

partial struct SplitTest {
    internal @string s;
    internal @string sep;
    internal nint n;
    internal slice<@string> a;
}

internal static slice<SplitTest> splittests = new SplitTest[]{
    new(""u8, ""u8, -1, new @string[]{}.slice()),
    new(abcd, ""u8, 2, new @string[]{"a"u8, "bcd"u8}.slice()),
    new(abcd, ""u8, 4, new @string[]{"a"u8, "b"u8, "c"u8, "d"u8}.slice()),
    new(abcd, ""u8, -1, new @string[]{"a"u8, "b"u8, "c"u8, "d"u8}.slice()),
    new(faces, ""u8, -1, new @string[]{"☺"u8, "☻"u8, "☹"u8}.slice()),
    new(faces, ""u8, 3, new @string[]{"☺"u8, "☻"u8, "☹"u8}.slice()),
    new(faces, ""u8, 17, new @string[]{"☺"u8, "☻"u8, "☹"u8}.slice()),
    new("☺�☹"u8, ""u8, -1, new @string[]{"☺"u8, "�"u8, "☹"u8}.slice()),
    new(abcd, "a"u8, 0, default!),
    new(abcd, "a"u8, -1, new @string[]{""u8, "bcd"u8}.slice()),
    new(abcd, "z"u8, -1, new @string[]{"abcd"u8}.slice()),
    new(commas, ","u8, -1, new @string[]{"1"u8, "2"u8, "3"u8, "4"u8}.slice()),
    new(dots, "..."u8, -1, new @string[]{"1"u8, ".2"u8, ".3"u8, ".4"u8}.slice()),
    new(faces, "☹"u8, -1, new @string[]{"☺☻"u8, ""u8}.slice()),
    new(faces, "~"u8, -1, new @string[]{faces}.slice()),
    new("1 2 3 4"u8, " "u8, 3, new @string[]{"1"u8, "2"u8, "3 4"u8}.slice()),
    new("1 2"u8, " "u8, 3, new @string[]{"1"u8, "2"u8}.slice()),
    new(""u8, "T"u8, unchecked((nint)(2305843009213693951L)), new @string[]{""u8}.slice()),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), ""u8, -1, new @string[]{((@string)(new byte[]{0xff})), "-"u8, ((@string)(new byte[]{0xff}))}.slice()),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), "-"u8, -1, new @string[]{((@string)(new byte[]{0xff})), ((@string)(new byte[]{0xff}))}.slice())
}.slice();

public static void TestSplit(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in splittests) {
        var a = SplitN(tt.s, tt.sep, tt.n);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            Ꮡt.Errorf("Split(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, a, tt.a);
            continue;
        }
        if (tt.n < 0) {
            var a2 = slices.Collect(SplitSeq(tt.s, tt.sep));
            if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
                Ꮡt.Errorf(@"collect(SplitSeq(%q, %q)) = %v; want %v"u8, tt.s, tt.sep, a2, tt.a);
            }
        }
        if (tt.n == 0) {
            continue;
        }
        @string s = Join(a, tt.sep);
        if (s != tt.s) {
            Ꮡt.Errorf("Join(Split(%q, %q, %d), %q) = %q"u8, tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = Split(tt.s, tt.sep);
            if (!slices.Equal<slice<@string>, @string>(a, b)) {
                Ꮡt.Errorf("Split disagrees with SplitN(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, b, a);
            }
        }
    }
}

internal static slice<SplitTest> splitaftertests = new SplitTest[]{
    new(abcd, "a"u8, -1, new @string[]{"a"u8, "bcd"u8}.slice()),
    new(abcd, "z"u8, -1, new @string[]{"abcd"u8}.slice()),
    new(abcd, ""u8, -1, new @string[]{"a"u8, "b"u8, "c"u8, "d"u8}.slice()),
    new(commas, ","u8, -1, new @string[]{"1,"u8, "2,"u8, "3,"u8, "4"u8}.slice()),
    new(dots, "..."u8, -1, new @string[]{"1..."u8, ".2..."u8, ".3..."u8, ".4"u8}.slice()),
    new(faces, "☹"u8, -1, new @string[]{"☺☻☹"u8, ""u8}.slice()),
    new(faces, "~"u8, -1, new @string[]{faces}.slice()),
    new(faces, ""u8, -1, new @string[]{"☺"u8, "☻"u8, "☹"u8}.slice()),
    new("1 2 3 4"u8, " "u8, 3, new @string[]{"1 "u8, "2 "u8, "3 4"u8}.slice()),
    new("1 2 3"u8, " "u8, 3, new @string[]{"1 "u8, "2 "u8, "3"u8}.slice()),
    new("1 2"u8, " "u8, 3, new @string[]{"1 "u8, "2"u8}.slice()),
    new("123"u8, ""u8, 2, new @string[]{"1"u8, "23"u8}.slice()),
    new("123"u8, ""u8, 17, new @string[]{"1"u8, "2"u8, "3"u8}.slice())
}.slice();

public static void TestSplitAfter(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in splitaftertests) {
        var a = SplitAfterN(tt.s, tt.sep, tt.n);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            Ꮡt.Errorf(@"Split(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, a, tt.a);
            continue;
        }
        if (tt.n < 0) {
            var a2 = slices.Collect(SplitAfterSeq(tt.s, tt.sep));
            if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
                Ꮡt.Errorf(@"collect(SplitAfterSeq(%q, %q)) = %v; want %v"u8, tt.s, tt.sep, a2, tt.a);
            }
        }
        @string s = Join(a, ""u8);
        if (s != tt.s) {
            Ꮡt.Errorf(@"Join(Split(%q, %q, %d), %q) = %q"u8, tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = SplitAfter(tt.s, tt.sep);
            if (!slices.Equal<slice<@string>, @string>(a, b)) {
                Ꮡt.Errorf("SplitAfter disagrees with SplitAfterN(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, b, a);
            }
        }
    }
}

partial struct FieldsTest {
    internal @string s;
    internal slice<@string> a;
}

internal static slice<FieldsTest> fieldstests = new FieldsTest[]{
    new(""u8, new @string[]{}.slice()),
    new(" "u8, new @string[]{}.slice()),
    new(" \t "u8, new @string[]{}.slice()),
    new("\u2000"u8, new @string[]{}.slice()),
    new("  abc  "u8, new @string[]{"abc"u8}.slice()),
    new("1 2 3 4"u8, new @string[]{"1"u8, "2"u8, "3"u8, "4"u8}.slice()),
    new("1  2  3  4"u8, new @string[]{"1"u8, "2"u8, "3"u8, "4"u8}.slice()),
    new("1\t\t2\t\t3\t4"u8, new @string[]{"1"u8, "2"u8, "3"u8, "4"u8}.slice()),
    new("1\u20002\u20013\u20024"u8, new @string[]{"1"u8, "2"u8, "3"u8, "4"u8}.slice()),
    new("\u2000\u2001\u2002"u8, new @string[]{}.slice()),
    new("\n™\t™\n"u8, new @string[]{"™"u8, "™"u8}.slice()),
    new("\n\u20001™2\u2000 \u2001 ™"u8, new @string[]{"1™2"u8, "™"u8}.slice()),
    new("\n1\uFFFD \uFFFD2\u20003\uFFFD4"u8, new @string[]{"1\uFFFD"u8, "\uFFFD2"u8, "3\uFFFD4"u8}.slice()),
    new(((@string)(new byte[]{0x31, 0xff, 0xe2, 0x80, 0x80, 0xff, 0x32, 0xff, 0x20, 0xff})), new @string[]{((@string)(new byte[]{0x31, 0xff})), ((@string)(new byte[]{0xff, 0x32, 0xff})), ((@string)(new byte[]{0xff}))}.slice()),
    new(faces, new @string[]{faces}.slice())
}.slice();

public static void TestFields(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in fieldstests) {
        var a = Fields(tt.s);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            Ꮡt.Errorf("Fields(%q) = %v; want %v"u8, tt.s, a, tt.a);
            continue;
        }
        var a2 = collect(Ꮡt, FieldsSeq(tt.s));
        if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
            Ꮡt.Errorf(@"collect(FieldsSeq(%q)) = %v; want %v"u8, tt.s, a2, tt.a);
        }
    }
}

public static slice<FieldsTest> FieldsFuncTests = new FieldsTest[]{
    new(""u8, new @string[]{}.slice()),
    new("XX"u8, new @string[]{}.slice()),
    new("XXhiXXX"u8, new @string[]{"hi"u8}.slice()),
    new("aXXbXXXcX"u8, new @string[]{"a"u8, "b"u8, "c"u8}.slice())
}.slice();

public static void TestFieldsFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in fieldstests) {
        var a = FieldsFunc(tt.s, Δunicode.IsSpace);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            Ꮡt.Errorf("FieldsFunc(%q, unicode.IsSpace) = %v; want %v"u8, tt.s, a, tt.a);
            continue;
        }
    }
    var pred = (rune c) => c == (rune)'X';
    foreach (var (_, tt) in FieldsFuncTests) {
        var a = FieldsFunc(tt.s, pred);
        if (!slices.Equal<slice<@string>, @string>(a, tt.a)) {
            Ꮡt.Errorf("FieldsFunc(%q) = %v, want %v"u8, tt.s, a, tt.a);
        }
        var a2 = collect(Ꮡt, FieldsFuncSeq(tt.s, pred));
        if (!slices.Equal<slice<@string>, @string>(a2, tt.a)) {
            Ꮡt.Errorf(@"collect(FieldsFuncSeq(%q)) = %v; want %v"u8, tt.s, a2, tt.a);
        }
    }
}

// Test case for any function which accepts and returns a single string.
partial struct StringTest {
    internal @string @in, @out;
}

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runStringTests(ж<testing.T> Ꮡt, Func<@string, @string> f, @string funcName, slice<StringTest> testCases) {
    foreach (var (_, tc) in testCases) {
        @string actual = f(tc.@in);
        if (actual != tc.@out) {
            Ꮡt.Errorf("%s(%q) = %q; want %q"u8, funcName, tc.@in, actual, tc.@out);
        }
    }
}

// grows one byte per char
// test utf8.RuneSelf and utf8.MaxRune
internal static slice<StringTest> upperTests = new StringTest[]{
    new(""u8, ""u8),
    new("ONLYUPPER"u8, "ONLYUPPER"u8),
    new("abc"u8, "ABC"u8),
    new("AbC123"u8, "ABC123"u8),
    new("azAZ09_"u8, "AZAZ09_"u8),
    new("longStrinGwitHmixofsmaLLandcAps"u8, "LONGSTRINGWITHMIXOFSMALLANDCAPS"u8),
    new("RENAN BASTOS 93 AOSDAJDJAIDJAIDAJIaidsjjaidijadsjiadjiOOKKO"u8, "RENAN BASTOS 93 AOSDAJDJAIDJAIDAJIAIDSJJAIDIJADSJIADJIOOKKO"u8),
    new("long\u0250string\u0250with\u0250nonascii\u2C6Fchars"u8, "LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS"u8),
    new("\u0250\u0250\u0250\u0250\u0250"u8, "\u2C6F\u2C6F\u2C6F\u2C6F\u2C6F"u8),
    new("a\u0080\U0010FFFF"u8, "A\u0080\U0010FFFF"u8)
}.slice();

// shrinks one byte per char
// test utf8.RuneSelf and utf8.MaxRune
internal static slice<StringTest> lowerTests = new StringTest[]{
    new(""u8, ""u8),
    new("abc"u8, "abc"u8),
    new("AbC123"u8, "abc123"u8),
    new("azAZ09_"u8, "azaz09_"u8),
    new("longStrinGwitHmixofsmaLLandcAps"u8, "longstringwithmixofsmallandcaps"u8),
    new("renan bastos 93 AOSDAJDJAIDJAIDAJIaidsjjaidijadsjiadjiOOKKO"u8, "renan bastos 93 aosdajdjaidjaidajiaidsjjaidijadsjiadjiookko"u8),
    new("LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS"u8, "long\u0250string\u0250with\u0250nonascii\u0250chars"u8),
    new("\u2C6D\u2C6D\u2C6D\u2C6D\u2C6D"u8, "\u0251\u0251\u0251\u0251\u0251"u8),
    new("A\u0080\U0010FFFF"u8, "a\u0080\U0010FFFF"u8)
}.slice();

internal static readonly @string space = "\t\v\r\f\n\u0085\u00a0\u2000\u3000"u8;

internal static slice<StringTest> trimSpaceTests = new StringTest[]{
    new(""u8, ""u8),
    new("abc"u8, "abc"u8),
    new(space + "abc" + space, "abc"u8),
    new(" "u8, ""u8),
    new(" \t\r\n \t\t\r\r\n\n "u8, ""u8),
    new(" \t\r\n x\t\t\r\r\n\n "u8, "x"u8),
    new(" \u2000\t\r\n x\t\t\r\r\ny\n \u3000"u8, "x\t\t\r\r\ny"u8),
    new("1 \t\r\n2"u8, "1 \t\r\n2"u8),
    new(((@string)(new byte[]{0x20, 0x78, 0x80})), ((@string)(new byte[]{0x78, 0x80}))),
    new(((@string)(new byte[]{0x20, 0x78, 0xc0})), ((@string)(new byte[]{0x78, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0})), ((@string)(new byte[]{0x78, 0x20, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0}))),
    new(((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0, 0x20})), ((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0}))),
    new("x ☺ "u8, "x ☺"u8)
}.slice();

internal static @string tenRunes(rune ch) {
    var r = new slice<rune>(10);
    foreach (var (i, _) in r) {
        r[i] = ch;
    }
    return ((@string)r);
}

// User-defined self-inverse mapping function
internal static rune rot13(rune r) {
    var step = (rune)13;
    if (r >= (rune)'a' && r <= (rune)'z') {
        return ((r - (rune)'a' + step) % 26) + (rune)'a';
    }
    if (r >= (rune)'A' && r <= (rune)'Z') {
        return ((r - (rune)'A' + step) % 26) + (rune)'A';
    }
    return r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aToZedˢ = "a to zed"u8;
internal static readonly @string nGbMrqˢ = "n gb mrq"u8;
internal static readonly @string helloˢ4 = "Hello, 세계"u8;
internal static readonly @string inputStringThatWeExpectˢ = "Input string that we expect not to be copied."u8;
internal static readonly object unexpectedCopyDuringˢ = (@string)"unexpected copy during identity map"u8;
internal static readonly @string helloWorldˢ2 = "Hello\uFFFDWorld"u8;
internal static readonly @string abc123ˢ = "   abc    123   "u8;
internal static readonly @string abc123ˢ2 = "abc123"u8;

public static void TestMap(ж<testing.T> Ꮡt) {
    // Run a couple of awful growth/shrinkage tests
    @string a = tenRunes((rune)'a');
    // 1.  Grow. This triggers two reallocations in Map.
    var maxRune = rune (rune _) => Δunicode.MaxRune;
    @string m = Map(maxRune, a);
    @string expect = tenRunes(Δunicode.MaxRune);
    if (m != expect) {
        Ꮡt.Errorf("growing: expected %q got %q"u8, expect, m);
    }
    // 2. Shrink
    var minRune = (rune _) => (rune)'a';
    m = Map(minRune, tenRunes(Δunicode.MaxRune));
    expect = a;
    if (m != expect) {
        Ꮡt.Errorf("shrinking: expected %q got %q"u8, expect, m);
    }
    // 3. Rot13
    m = Map(rot13, aToZedˢ);
    expect = nGbMrqˢ;
    if (m != expect) {
        Ꮡt.Errorf("rot13: expected %q got %q"u8, expect, m);
    }
    // 4. Rot13^2
    m = Map(rot13, Map(rot13, aToZedˢ));
    expect = aToZedˢ;
    if (m != expect) {
        Ꮡt.Errorf("rot13: expected %q got %q"u8, expect, m);
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
        Ꮡt.Errorf("drop: expected %q got %q"u8, expect, m);
    }
    // 6. Identity
    var identity = (rune rΔ2) => rΔ2;
    @string orig = inputStringThatWeExpectˢ;
    m = Map(identity, orig);
    if (@unsafe.StringData(orig) != @unsafe.StringData(m)) {
        Ꮡt.Error(unexpectedCopyDuringˢ);
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
        Ꮡt.Errorf("replace invalid sequence: expected %q got %q"u8, expect, m);
    }
    // 8. Check utf8.RuneSelf and utf8.MaxRune encoding
    var encode = rune (rune rΔ4) => {
        var exprᴛ1 = rΔ4;
        if (exprᴛ1 == utf8.RuneSelf) {
            return Δunicode.MaxRune;
        }
        if (exprᴛ1 == Δunicode.MaxRune) {
            return utf8.RuneSelf;
        }

        return rΔ4;
    };
    @string s = ((@string)(rune)utf8.RuneSelf) + ((@string)(rune)utf8.MaxRune);
    @string r = ((@string)(rune)utf8.MaxRune) + ((@string)(rune)utf8.RuneSelf); // reverse of s
    m = Map(encode, s);
    if (m != r) {
        Ꮡt.Errorf("encoding not handled correctly: expected %q got %q"u8, r, m);
    }
    m = Map(encode, r);
    if (m != s) {
        Ꮡt.Errorf("encoding not handled correctly: expected %q got %q"u8, s, m);
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
        Ꮡt.Errorf("trimSpaces: expected %q got %q"u8, expect, m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string toUpperˢ = "ToUpper"u8;

public static void TestToUpper(ж<testing.T> Ꮡt) {
    runStringTests(Ꮡt, ToUpper, toUpperˢ, upperTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string toLowerˢ = "ToLower"u8;

public static void TestToLower(ж<testing.T> Ꮡt) {
    runStringTests(Ꮡt, ToLower, toLowerˢ, lowerTests);
}


partial struct toValidUTF8Testsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string repl;
    internal @string @out;
}
internal static slice<toValidUTF8Testsᴛ1> toValidUTF8Tests = new toValidUTF8Testsᴛ1[]{
    new(""u8, "\uFFFD"u8, ""u8),
    new("abc"u8, "\uFFFD"u8, "abc"u8),
    new("\uFDDD"u8, "\uFFFD"u8, "\uFDDD"u8),
    new(((@string)(new byte[]{0x61, 0xff, 0x62})), "\uFFFD"u8, "a\uFFFDb"u8),
    new(((@string)(new byte[]{0x61, 0xff, 0x62, 0xef, 0xbf, 0xbd})), "X"u8, "aXb\uFFFD"u8),
    new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0xff, 0x62, 0xe2, 0x98, 0xba, 0xc0, 0xaf, 0x63, 0xe2, 0x98, 0xba, 0xff})), ""u8, "a☺b☺c☺"u8),
    new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0xff, 0x62, 0xe2, 0x98, 0xba, 0xc0, 0xaf, 0x63, 0xe2, 0x98, 0xba, 0xff})), "日本語"u8, "a☺日本語b☺日本語c☺日本語"u8),
    new(((@string)(new byte[]{0xc0, 0xaf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xe0, 0x80, 0xaf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xed, 0xa0, 0x80})), "abc"u8, "abc"u8),
    new(((@string)(new byte[]{0xed, 0xbf, 0xbf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xf0, 0x80, 0x80, 0xaf})), "☺"u8, "☺"u8),
    new(((@string)(new byte[]{0xf8, 0x80, 0x80, 0x80, 0xaf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xfc, 0x80, 0x80, 0x80, 0x80, 0xaf})), "\uFFFD"u8, "\uFFFD"u8)
}.slice();

public static void TestToValidUTF8(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in toValidUTF8Tests) {
        @string got = ToValidUTF8(tc.@in, tc.repl);
        if (got != tc.@out) {
            Ꮡt.Errorf("ToValidUTF8(%q, %q) = %q; want %q"u8, tc.@in, tc.repl, got, tc.@out);
        }
    }
}

public static void BenchmarkToUpper(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in upperTests) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                @string actual = ToUpper(tcʗ1.@in);
                if (actual != tcʗ1.@out) {
                    bΔ1.Errorf("ToUpper(%q) = %q; want %q"u8, tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}

public static void BenchmarkToLower(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in lowerTests) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                @string actual = ToLower(tcʗ1.@in);
                if (actual != tcʗ1.@out) {
                    bΔ1.Errorf("ToLower(%q) = %q; want %q"u8, tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someStringThatWonTBeˢ = "Some string that won't be modified."u8;

public static void BenchmarkMapNoChanges(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var identity = (rune r) => r;
    for (nint i = 0; i < b.N; i++) {
        Map(identity, someStringThatWonTBeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcDefgHIjklmnoPrsTuVyzˢ = "abcçdefgğhıijklmnoöprsştuüvyz"u8;
internal static readonly @string abcDefgHiJklmnoPrsTuVyzˢ = "ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ"u8;

public static void TestSpecialCase(ж<testing.T> Ꮡt) {
    @string lower = abcDefgHIjklmnoPrsTuVyzˢ;
    @string upper = abcDefgHiJklmnoPrsTuVyzˢ;
    @string u = ToUpperSpecial(Δunicode.TurkishCase, upper);
    if (u != upper) {
        Ꮡt.Errorf("Upper(upper) is %s not %s"u8, u, upper);
    }
    u = ToUpperSpecial(Δunicode.TurkishCase, lower);
    if (u != upper) {
        Ꮡt.Errorf("Upper(lower) is %s not %s"u8, u, upper);
    }
    @string l = ToLowerSpecial(Δunicode.TurkishCase, lower);
    if (l != lower) {
        Ꮡt.Errorf("Lower(lower) is %s not %s"u8, l, lower);
    }
    l = ToLowerSpecial(Δunicode.TurkishCase, upper);
    if (l != lower) {
        Ꮡt.Errorf("Lower(upper) is %s not %s"u8, l, lower);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trimSpaceˢ = "TrimSpace"u8;

public static void TestTrimSpace(ж<testing.T> Ꮡt) {
    runStringTests(Ꮡt, TrimSpace, trimSpaceˢ, trimSpaceTests);
}

//empty string tests

partial struct trimTestsᴛ1 /*dyn*/ {
    internal @string f;
    internal @string @in, arg, @out;
}
internal static slice<trimTestsᴛ1> trimTests = new trimTestsᴛ1[]{
    new("Trim"u8, "abba"u8, "a"u8, "bb"u8),
    new("Trim"u8, "abba"u8, "ab"u8, ""u8),
    new("TrimLeft"u8, "abba"u8, "ab"u8, ""u8),
    new("TrimRight"u8, "abba"u8, "ab"u8, ""u8),
    new("TrimLeft"u8, "abba"u8, "a"u8, "bba"u8),
    new("TrimLeft"u8, "abba"u8, "b"u8, "abba"u8),
    new("TrimRight"u8, "abba"u8, "a"u8, "abb"u8),
    new("TrimRight"u8, "abba"u8, "b"u8, "abba"u8),
    new("Trim"u8, "<tag>"u8, "<>"u8, "tag"u8),
    new("Trim"u8, "* listitem"u8, " *"u8, "listitem"u8),
    new("Trim"u8, @"""quote"""u8, @""""u8, "quote"u8),
    new("Trim"u8, "\u2C6F\u2C6F\u0250\u0250\u2C6F\u2C6F"u8, "\u2C6F"u8, "\u0250\u0250"u8),
    new("Trim"u8, ((@string)(new byte[]{0x80, 0x74, 0x65, 0x73, 0x74, 0xff})), ((@string)(new byte[]{0xff})), "test"u8),
    new("Trim"u8, " Ġ "u8, " "u8, "Ġ"u8),
    new("Trim"u8, " Ġİ0"u8, "0 "u8, "Ġİ"u8),
    new("Trim"u8, "abba"u8, ""u8, "abba"u8),
    new("Trim"u8, ""u8, "123"u8, ""u8),
    new("Trim"u8, ""u8, ""u8, ""u8),
    new("TrimLeft"u8, "abba"u8, ""u8, "abba"u8),
    new("TrimLeft"u8, ""u8, "123"u8, ""u8),
    new("TrimLeft"u8, ""u8, ""u8, ""u8),
    new("TrimRight"u8, "abba"u8, ""u8, "abba"u8),
    new("TrimRight"u8, ""u8, "123"u8, ""u8),
    new("TrimRight"u8, ""u8, ""u8, ""u8),
    new("TrimRight"u8, ((@string)(new byte[]{0xe2, 0x98, 0xba, 0xc0})), "☺"u8, ((@string)(new byte[]{0xe2, 0x98, 0xba, 0xc0}))),
    new("TrimPrefix"u8, "aabb"u8, "a"u8, "abb"u8),
    new("TrimPrefix"u8, "aabb"u8, "b"u8, "aabb"u8),
    new("TrimSuffix"u8, "aabb"u8, "a"u8, "aabb"u8),
    new("TrimSuffix"u8, "aabb"u8, "b"u8, "aab"u8)
}.slice();

public static void TestTrim(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in trimTests) {
        @string name = tc.f;
        Func<@string, @string, @string> f = default!;
        var exprᴛ1 = name;
        if (exprᴛ1 == "Trim"u8) {
            f = Trim;
        }
        else if (exprᴛ1 == "TrimLeft"u8) {
            f = TrimLeft;
        }
        else if (exprᴛ1 == "TrimRight"u8) {
            f = TrimRight;
        }
        else if (exprᴛ1 == "TrimPrefix"u8) {
            f = TrimPrefix;
        }
        else if (exprᴛ1 == "TrimSuffix"u8) {
            f = TrimSuffix;
        }
        else { /* default: */
            Ꮡt.Errorf("Undefined trim function %s"u8, name);
        }

        @string actual = f(tc.@in, tc.arg);
        if (actual != tc.@out) {
            Ꮡt.Errorf("%s(%q, %q) = %q; want %q"u8, name, tc.@in, tc.arg, actual, tc.@out);
        }
    }
}

public static void BenchmarkTrim(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, tc) in trimTests) {
            @string name = tc.f;
            Func<@string, @string, @string> f = default!;
            var exprᴛ1 = name;
            if (exprᴛ1 == "Trim"u8) {
                f = Trim;
            }
            else if (exprᴛ1 == "TrimLeft"u8) {
                f = TrimLeft;
            }
            else if (exprᴛ1 == "TrimRight"u8) {
                f = TrimRight;
            }
            else if (exprᴛ1 == "TrimPrefix"u8) {
                f = TrimPrefix;
            }
            else if (exprᴛ1 == "TrimSuffix"u8) {
                f = TrimSuffix;
            }
            else { /* default: */
                Ꮡb.Errorf("Undefined trim function %s"u8, name);
            }

            @string actual = f(tc.@in, tc.arg);
            if (actual != tc.@out) {
                Ꮡb.Errorf("%s(%q, %q) = %q; want %q"u8, name, tc.@in, tc.arg, actual, tc.@out);
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

    var tests = new BenchmarkToValidUTF8_tests[]{
        new("Valid"u8, "typical"u8),
        new("InvalidASCII"u8, ((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xff, 0x62, 0x61, 0x72}))),
        new("InvalidNonASCII"u8, ((@string)(new byte[]{0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e, 0xff, 0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e})))
    }.slice();
    @string replacement = "\uFFFD"u8;
    b.ResetTimer();
    foreach (var (_, vᴛ1) in tests) {
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

internal static predicate isSpace = new predicate(Δunicode.IsSpace, "IsSpace"u8);

internal static predicate isDigit = new predicate(Δunicode.IsDigit, "IsDigit"u8);

internal static predicate isUpper = new predicate(Δunicode.IsUpper, "IsUpper"u8);

internal static predicate isValidRune = new predicate(
    (rune r) => r != utf8.RuneError,
    "IsValidRune"u8
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
internal static slice<trimFuncTestsᴛ1> trimFuncTests = new trimFuncTestsᴛ1[]{
    new(isSpace, space + " hello " + space,
        "hello"u8,
        "hello " + space,
        space + " hello"),
    new(isDigit, "\u0e50\u0e5212hello34\u0e50\u0e51"u8,
        "hello"u8,
        "hello34\u0e50\u0e51"u8,
        "\u0e50\u0e5212hello"u8),
    new(isUpper, "\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8,
        "hello"u8,
        "helloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8,
        "\u2C6F\u2C6F\u2C6F\u2C6FABCDhello"u8),
    new(not(isSpace), "hello" + space + "hello",
        space,
        space + "hello",
        "hello" + space),
    new(not(isDigit), "hello\u0e50\u0e521234\u0e50\u0e51helo"u8,
        "\u0e50\u0e521234\u0e50\u0e51"u8,
        "\u0e50\u0e521234\u0e50\u0e51helo"u8,
        "hello\u0e50\u0e521234\u0e50\u0e51"u8),
    new(isValidRune, ((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})),
        ((@string)(new byte[]{0xc0, 0x61, 0xc0})),
        ((@string)(new byte[]{0xc0, 0x61, 0xc0, 0x63, 0x64})),
        ((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0}))),
    new(not(isValidRune), ((@string)(new byte[]{0xc0, 0x61, 0xc0})),
        "a"u8,
        ((@string)(new byte[]{0x61, 0xc0})),
        ((@string)(new byte[]{0xc0, 0x61}))),
    new(isSpace, ""u8,
        ""u8,
        ""u8,
        ""u8),
    new(isSpace, " "u8,
        ""u8,
        ""u8,
        ""u8)
}.slice();

internal partial struct TestTrimFunc_trimmers /*dyn*/ {
    internal @string name;
    internal Func<@string, Func<rune, bool>, @string> trim;
    internal @string @out;
}

public static void TestTrimFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in trimFuncTests) {
        var trimmers = new TestTrimFunc_trimmers[]{
            new("TrimFunc"u8, TrimFunc, tc.trimOut),
            new("TrimLeftFunc"u8, TrimLeftFunc, tc.leftOut),
            new("TrimRightFunc"u8, TrimRightFunc, tc.rightOut)
        }.slice();
        foreach (var (_, trimmer) in trimmers) {
            @string actual = trimmer.trim(tc.@in, tc.f.f);
            if (actual != trimmer.@out) {
                Ꮡt.Errorf("%s(%q, %q) = %q; want %q"u8, trimmer.name, tc.@in, tc.f.name, actual, trimmer.@out);
            }
        }
    }
}

// last rune in space is 3 bytes
// tests of invalid UTF-8

partial struct indexFuncTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal predicate f;
    internal nint first, last;
}
internal static slice<indexFuncTestsᴛ1> indexFuncTests = new indexFuncTestsᴛ1[]{
    new(""u8, isValidRune, -1, -1),
    new("abc"u8, isDigit, -1, -1),
    new("0123"u8, isDigit, 0, 3),
    new("a1b"u8, isDigit, 1, 1),
    new(space, isSpace, 0, len(space) - 3),
    new("\u0e50\u0e5212hello34\u0e50\u0e51"u8, isDigit, 0, 18),
    new("\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8, isUpper, 0, 34),
    new("12\u0e50\u0e52hello34\u0e50\u0e51"u8, not(isDigit), 8, 12),
    new(((@string)(new byte[]{0x80, 0x31})), isDigit, 1, 1),
    new(((@string)(new byte[]{0x80, 0x61, 0x62, 0x63})), isDigit, -1, -1),
    new(((@string)(new byte[]{0xc0, 0x61, 0xc0})), isValidRune, 1, 1),
    new(((@string)(new byte[]{0xc0, 0x61, 0xc0})), not(isValidRune), 0, 2),
    new(((@string)(new byte[]{0xc0, 0xe2, 0x98, 0xba, 0xc0})), not(isValidRune), 0, 4),
    new(((@string)(new byte[]{0xc0, 0xe2, 0x98, 0xba, 0xc0, 0xc0})), not(isValidRune), 0, 5),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})), not(isValidRune), 2, 4),
    new(((@string)(new byte[]{0x61, 0xe0, 0x80, 0x63, 0x64})), not(isValidRune), 1, 2),
    new(((@string)(new byte[]{0x80, 0x80, 0x80, 0x80})), not(isValidRune), 0, 3)
}.slice();

public static void TestIndexFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in indexFuncTests) {
        nint first = IndexFunc(tc.@in, tc.f.f);
        if (first != tc.first) {
            Ꮡt.Errorf("IndexFunc(%q, %s) = %d; want %d"u8, tc.@in, tc.f.name, first, tc.first);
        }
        nint last = LastIndexFunc(tc.@in, tc.f.f);
        if (last != tc.last) {
            Ꮡt.Errorf("LastIndexFunc(%q, %s) = %d; want %d"u8, tc.@in, tc.f.name, last, tc.last);
        }
    }
}

internal static bool equal(@string m, @string s1, @string s2, ж<testing.T> Ꮡt) {
    if (s1 == s2) {
        return true;
    }
    var e1 = Split(s1, ""u8);
    var e2 = Split(s2, ""u8);
    foreach (var (i, c1) in e1) {
        if (i >= len(e2)) {
            break;
        }
        var (r1, _) = utf8.DecodeRuneInString(c1);
        var (r2, _) = utf8.DecodeRuneInString(e2[i]);
        if (r1 != r2) {
            Ꮡt.Errorf("%s diff at %d: U+%04X U+%04X"u8, m, i, r1, r2);
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object runeCountWrongInUpperˢ = (@string)"rune count wrong in upper:"u8;
internal static readonly object runeCountWrongInLowerˢ = (@string)"rune count wrong in lower:"u8;
internal static readonly @string toUpperUpperˢ = "ToUpper(upper)"u8;
internal static readonly object toUpperUpperConsistencyˢ = (@string)"ToUpper(upper) consistency fail"u8;
internal static readonly @string toLowerLowerˢ = "ToLower(lower)"u8;
internal static readonly object toLowerLowerConsistencyˢ = (@string)"ToLower(lower) consistency fail"u8;

public static void TestCaseConsistency(ж<testing.T> Ꮡt) {
    // Make a string of all the runes.
    nint numRunes = (nint)(Δunicode.MaxRune + 1);
    if (testing.Short()) {
        numRunes = 1000;
    }
    var a = new slice<rune>(numRunes);
    foreach (var (i, _) in a) {
        a[i] = (rune)i;
    }
    @string s = ((@string)a);
    // convert the cases.
    @string upper = ToUpper(s);
    @string lower = ToLower(s);
    // Consistency checks
    {
        nint n = utf8.RuneCountInString(upper); if (n != numRunes) {
            Ꮡt.Error(runeCountWrongInUpperˢ, n);
        }
    }
    {
        nint n = utf8.RuneCountInString(lower); if (n != numRunes) {
            Ꮡt.Error(runeCountWrongInLowerˢ, n);
        }
    }
    if (!equal(toUpperUpperˢ, ToUpper(upper), upper, Ꮡt)) {
        Ꮡt.Error(toUpperUpperConsistencyˢ);
    }
    if (!equal(toLowerLowerˢ, ToLower(lower), lower, Ꮡt)) {
        Ꮡt.Error(toLowerLowerConsistencyˢ);
    }
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
internal static @string longString = "a"u8 + ((sstring)new slice<byte>((1 << (int)(16)))) + "z"u8;

internal static @string longSpaces = ((Func<@string>)(() => {
    var b = new slice<byte>(200);
    foreach (var (i, _) in b) {
        b[i] = (rune)' ';
    }
    return ((@string)b);
}))();

// Tests for results over the chunkLimit

partial struct RepeatTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
    internal nint count;
}
public static slice<RepeatTestsᴛ1> RepeatTests = new RepeatTestsᴛ1[]{
    new(""u8, ""u8, 0),
    new(""u8, ""u8, 1),
    new(""u8, ""u8, 2),
    new("-"u8, ""u8, 0),
    new("-"u8, "-"u8, 1),
    new("-"u8, "----------"u8, 10),
    new("abc "u8, "abc abc abc "u8, 3),
    new(" "u8, " "u8, 1),
    new("--"u8, "----"u8, 2),
    new("==="u8, "======"u8, 2),
    new("000"u8, "000000000"u8, 3),
    new("\t\t\t\t"u8, "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t"u8, 4),
    new(" "u8, longSpaces, len(longSpaces)),
    new(((@string)(rune)0), ((@string)new slice<byte>((1 << (int)(16)))), (1 << (int)(16))),
    new(longString, longString + longString, 2)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string repeatSˢ = "Repeat(s)"u8;

public static void TestRepeat(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in RepeatTests) {
        @string a = Repeat(tt.@in, tt.count);
        if (!equal(repeatSˢ, a, tt.@out, Ꮡt)) {
            Ꮡt.Errorf("Repeat(%v, %d) = %v; want %v"u8, tt.@in, tt.count, a, tt.@out);
            continue;
        }
    }
}

internal static error /*err*/ repeat(@string s, nint count) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
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
            }
        }, ref ᒐ);
        Repeat(s, count);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bitˢ = "64-bit"u8;

internal partial struct TestRepeatCatchesOverflow_testCase /*dyn*/ {
    internal @string s;
    internal nint count;
    internal @string errStr;
}

// See Issue golang.org/issue/16237
public static void TestRepeatCatchesOverflow(ж<testing.T> Ꮡt) {
    void runTestCases(@string prefix, slice<TestRepeatCatchesOverflow_testCase> tests) {
        foreach (var (i, tt) in tests) {
            var err = repeat(tt.s, tt.count);
            if (tt.errStr == ""u8) {
                if (err != default!) {
                    Ꮡt.Errorf("#%d panicked %v"u8, i, err);
                }
                continue;
            }
            if (err == default! || !Contains(err.Error(), tt.errStr)) {
                Ꮡt.Errorf("%s#%d got %q want %q"u8, prefix, i, err, tt.errStr);
            }
        }
    }
    nint maxInt = /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);
    runTestCases(""u8, new slice<TestRepeatCatchesOverflow_testCase>(7){
        [0] = new("--"u8, -2147483647, "negative"u8),
        [1] = new(""u8, maxInt, ""u8),
        [2] = new("-"u8, 10, ""u8),
        [3] = new("gopher"u8, 0, ""u8),
        [4] = new("-"u8, -1, "negative"u8),
        [5] = new("--"u8, -102, "negative"u8),
        [6] = new(((@string)new slice<byte>(255)), (nint)((~(nuint)0) / 255 + 1), "overflow"u8)
    });
    const bool is64Bit = /* 1<<(^uintptr(0)>>63)/2 != 0 */ true;
    if (!is64Bit) {
        return;
    }
    runTestCases(bitˢ, new slice<TestRepeatCatchesOverflow_testCase>(1){
        [0] = new("-"u8, maxInt, "out of range"u8)
    });
}

internal static bool runesEqual(slice<rune> a, slice<rune> b) {
    if (len(a) != len(b)) {
        return false;
    }
    foreach (var (i, r) in a) {
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
public static slice<RunesTestsᴛ1> RunesTests = new RunesTestsᴛ1[]{
    new(""u8, new rune[]{}.slice(), false),
    new(" "u8, new rune[]{32}.slice(), false),
    new("ABC"u8, new rune[]{65, 66, 67}.slice(), false),
    new("abc"u8, new rune[]{97, 98, 99}.slice(), false),
    new("\u65e5\u672c\u8a9e"u8, new rune[]{26085, 26412, 35486}.slice(), false),
    new(((@string)(new byte[]{0x61, 0x62, 0x80, 0x63})), new rune[]{97, 98, 0xFFFD, 99}.slice(), true),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x63})), new rune[]{97, 98, 0xFFFD, 99}.slice(), true)
}.slice();

public static void TestRunes(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in RunesTests) {
        var a = slice<rune>(tt.@in);
        if (!runesEqual(a, tt.@out)) {
            Ꮡt.Errorf("[]rune(%q) = %v; want %v"u8, tt.@in, a, tt.@out);
            continue;
        }
        if (!tt.lossy) {
            // can only test reassembly if we didn't lose information
            @string s = ((@string)a);
            if (s != tt.@in) {
                Ꮡt.Errorf("string([]rune(%q)) = %x; want %x"u8, tt.@in, s, tt.@in);
            }
        }
    }
}

public static void TestReadByte(ж<testing.T> Ꮡt) {
    var testStrings = new @string[]{""u8, abcd, faces, commas}.slice();
    foreach (var (_, s) in testStrings) {
        var reader = NewReader(s);
        {
            var e = reader.UnreadByte(); if (e == default!) {
                Ꮡt.Errorf("Unreading %q at beginning: expected error"u8, s);
            }
        }
        ref var res = ref heap(new bytes.Buffer(), out var Ꮡres);
        while (ᐧ) {
            var (b, e) = reader.ReadByte();
            if (AreEqual(e, Δio.EOF)) {
                break;
            }
            if (e != default!) {
                Ꮡt.Errorf("Reading %q: %s"u8, s, e);
                break;
            }
            res.WriteByte(b);
            // unread and read again
            e = reader.UnreadByte();
            if (e != default!) {
                Ꮡt.Errorf("Unreading %q: %s"u8, s, e);
                break;
            }
            (var b1, e) = reader.ReadByte();
            if (e != default!) {
                Ꮡt.Errorf("Reading %q after unreading: %s"u8, s, e);
                break;
            }
            if (b1 != b) {
                Ꮡt.Errorf("Reading %q after unreading: want byte %q, got %q"u8, s, b, b1);
                break;
            }
        }
        if (Ꮡres.String() != s) {
            Ꮡt.Errorf("Reader(%q).ReadByte() produced %q"u8, s, Ꮡres.String());
        }
    }
}

public static void TestReadRune(ж<testing.T> Ꮡt) {
    var testStrings = new @string[]{""u8, abcd, faces, commas}.slice();
    foreach (var (_, s) in testStrings) {
        var reader = NewReader(s);
        {
            var e = reader.UnreadRune(); if (e == default!) {
                Ꮡt.Errorf("Unreading %q at beginning: expected error"u8, s);
            }
        }
        @string res = ""u8;
        while (ᐧ) {
            var (r, z, e) = reader.ReadRune();
            if (AreEqual(e, Δio.EOF)) {
                break;
            }
            if (e != default!) {
                Ꮡt.Errorf("Reading %q: %s"u8, s, e);
                break;
            }
            res += ((@string)r);
            // unread and read again
            e = reader.UnreadRune();
            if (e != default!) {
                Ꮡt.Errorf("Unreading %q: %s"u8, s, e);
                break;
            }
            (var r1, var z1, e) = reader.ReadRune();
            if (e != default!) {
                Ꮡt.Errorf("Reading %q after unreading: %s"u8, s, e);
                break;
            }
            if (r1 != r) {
                Ꮡt.Errorf("Reading %q after unreading: want rune %q, got %q"u8, s, r, r1);
                break;
            }
            if (z1 != z) {
                Ꮡt.Errorf("Reading %q after unreading: want size %d, got %d"u8, s, z, z1);
                break;
            }
        }
        if (res != s) {
            Ꮡt.Errorf("Reader(%q).ReadRune() produced %q"u8, s, res);
        }
    }
}


partial struct UnreadRuneErrorTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Action<ж<strings.Reader>> f;
}
public static slice<UnreadRuneErrorTestsᴛ1> UnreadRuneErrorTests = new UnreadRuneErrorTestsᴛ1[]{
    new("Read"u8, (ж<strings.Reader> r) => {
        r.Read(new byte[]{0}.slice());
    }),
    new("ReadByte"u8, (ж<strings.Reader> r) => {
        r.ReadByte();
    }),
    new("UnreadRune"u8, (ж<strings.Reader> r) => {
        r.UnreadRune();
    }),
    new("Seek"u8, (ж<strings.Reader> r) => {
        r.Seek(0, Δio.SeekCurrent);
    }),
    new("WriteTo"u8, (ж<strings.Reader> r) => {
        r.WriteTo(new strings_test_package.bytes_BufferжWriter(Ꮡ(new bytes.Buffer(nil))));
    })
}.slice();

public static void TestUnreadRuneError(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in UnreadRuneErrorTests) {
        var reader = NewReader("0123456789"u8);
        {
            var (_, _, errΔ1) = reader.ReadRune(); if (errΔ1 != default!) {
                // should not happen
                Ꮡt.Fatal(errΔ1);
            }
        }
        tt.f(reader);
        var err = reader.UnreadRune();
        if (err == default!) {
            Ꮡt.Errorf("Unreading after %s: expected error"u8, tt.name);
        }
    }
}


partial struct ReplaceTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string old, @new;
    internal nint n;
    internal @string @out;
}
public static slice<ReplaceTestsᴛ1> ReplaceTests = new ReplaceTestsᴛ1[]{
    new("hello"u8, "l"u8, "L"u8, 0, "hello"u8),
    new("hello"u8, "l"u8, "L"u8, -1, "heLLo"u8),
    new("hello"u8, "x"u8, "X"u8, -1, "hello"u8),
    new(""u8, "x"u8, "X"u8, -1, ""u8),
    new("radar"u8, "r"u8, "<r>"u8, -1, "<r>ada<r>"u8),
    new(""u8, ""u8, "<>"u8, -1, "<>"u8),
    new("banana"u8, "a"u8, "<>"u8, -1, "b<>n<>n<>"u8),
    new("banana"u8, "a"u8, "<>"u8, 1, "b<>nana"u8),
    new("banana"u8, "a"u8, "<>"u8, 1000, "b<>n<>n<>"u8),
    new("banana"u8, "an"u8, "<>"u8, -1, "b<><>a"u8),
    new("banana"u8, "ana"u8, "<>"u8, -1, "b<>na"u8),
    new("banana"u8, ""u8, "<>"u8, -1, "<>b<>a<>n<>a<>n<>a<>"u8),
    new("banana"u8, ""u8, "<>"u8, 10, "<>b<>a<>n<>a<>n<>a<>"u8),
    new("banana"u8, ""u8, "<>"u8, 6, "<>b<>a<>n<>a<>n<>a"u8),
    new("banana"u8, ""u8, "<>"u8, 5, "<>b<>a<>n<>a<>na"u8),
    new("banana"u8, ""u8, "<>"u8, 1, "<>banana"u8),
    new("banana"u8, "a"u8, "a"u8, -1, "banana"u8),
    new("banana"u8, "a"u8, "a"u8, 1, "banana"u8),
    new("☺☻☹"u8, ""u8, "<>"u8, -1, "<>☺<>☻<>☹<>"u8)
}.slice();

public static void TestReplace(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ReplaceTests) {
        {
            @string s = Replace(tt.@in, tt.old, tt.@new, tt.n); if (s != tt.@out) {
                Ꮡt.Errorf("Replace(%q, %q, %q, %d) = %q, want %q"u8, tt.@in, tt.old, tt.@new, tt.n, s, tt.@out);
            }
        }
        if (tt.n == -1) {
            @string s = ReplaceAll(tt.@in, tt.old, tt.@new);
            if (s != tt.@out) {
                Ꮡt.Errorf("ReplaceAll(%q, %q, %q) = %q, want %q"u8, tt.@in, tt.old, tt.@new, s, tt.@out);
            }
        }
    }
}


partial struct TitleTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
public static slice<TitleTestsᴛ1> TitleTests = new TitleTestsᴛ1[]{
    new(""u8, ""u8),
    new("a"u8, "A"u8),
    new(" aaa aaa aaa "u8, " Aaa Aaa Aaa "u8),
    new(" Aaa Aaa Aaa "u8, " Aaa Aaa Aaa "u8),
    new("123a456"u8, "123a456"u8),
    new("double-blind"u8, "Double-Blind"u8),
    new("ÿøû"u8, "Ÿøû"u8),
    new("with_underscore"u8, "With_underscore"u8),
    new(((@string)(new byte[]{0x75, 0x6e, 0x69, 0x63, 0x6f, 0x64, 0x65, 0x20, 0xe2, 0x80, 0xa8, 0x20, 0x6c, 0x69, 0x6e, 0x65, 0x20, 0x73, 0x65, 0x70, 0x61, 0x72, 0x61, 0x74, 0x6f, 0x72})), ((@string)(new byte[]{0x55, 0x6e, 0x69, 0x63, 0x6f, 0x64, 0x65, 0x20, 0xe2, 0x80, 0xa8, 0x20, 0x4c, 0x69, 0x6e, 0x65, 0x20, 0x53, 0x65, 0x70, 0x61, 0x72, 0x61, 0x74, 0x6f, 0x72})))
}.slice();

public static void TestTitle(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in TitleTests) {
        {
            @string s = Title(tt.@in); if (s != tt.@out) {
                Ꮡt.Errorf("Title(%q) = %q, want %q"u8, tt.@in, s, tt.@out);
            }
        }
    }
}

// cases to cover code in runtime/asm_amd64.s:indexShortStr
// 2-byte needle
// 3-byte needle
// 4-byte needle
// 5-7-byte needle
// 8-byte needle
// 9-15-byte needle
// 16-byte needle
// 17-31-byte needle
// partial match cases
// 3
// 5-7
// 9-15
// 17-31, issue 15679

partial struct ContainsTestsᴛ1 /*dyn*/ {
    internal @string str, substr;
    internal bool expected;
}
public static slice<ContainsTestsᴛ1> ContainsTests = new ContainsTestsᴛ1[]{
    new("abc"u8, "bc"u8, true),
    new("abc"u8, "bcd"u8, false),
    new("abc"u8, ""u8, true),
    new(""u8, "a"u8, false),
    new("xxxxxx"u8, "01"u8, false),
    new("01xxxx"u8, "01"u8, true),
    new("xx01xx"u8, "01"u8, true),
    new("xxxx01"u8, "01"u8, true),
    new("01xxxxx"u8[1..], "01"u8, false),
    new("xxxxx01"u8[..6], "01"u8, false),
    new("xxxxxxx"u8, "012"u8, false),
    new("012xxxx"u8, "012"u8, true),
    new("xx012xx"u8, "012"u8, true),
    new("xxxx012"u8, "012"u8, true),
    new("012xxxxx"u8[1..], "012"u8, false),
    new("xxxxx012"u8[..7], "012"u8, false),
    new("xxxxxxxx"u8, "0123"u8, false),
    new("0123xxxx"u8, "0123"u8, true),
    new("xx0123xx"u8, "0123"u8, true),
    new("xxxx0123"u8, "0123"u8, true),
    new("0123xxxxx"u8[1..], "0123"u8, false),
    new("xxxxx0123"u8[..8], "0123"u8, false),
    new("xxxxxxxxx"u8, "01234"u8, false),
    new("01234xxxx"u8, "01234"u8, true),
    new("xx01234xx"u8, "01234"u8, true),
    new("xxxx01234"u8, "01234"u8, true),
    new("01234xxxxx"u8[1..], "01234"u8, false),
    new("xxxxx01234"u8[..9], "01234"u8, false),
    new("xxxxxxxxxxxx"u8, "01234567"u8, false),
    new("01234567xxxx"u8, "01234567"u8, true),
    new("xx01234567xx"u8, "01234567"u8, true),
    new("xxxx01234567"u8, "01234567"u8, true),
    new("01234567xxxxx"u8[1..], "01234567"u8, false),
    new("xxxxx01234567"u8[..12], "01234567"u8, false),
    new("xxxxxxxxxxxxx"u8, "012345678"u8, false),
    new("012345678xxxx"u8, "012345678"u8, true),
    new("xx012345678xx"u8, "012345678"u8, true),
    new("xxxx012345678"u8, "012345678"u8, true),
    new("012345678xxxxx"u8[1..], "012345678"u8, false),
    new("xxxxx012345678"u8[..13], "012345678"u8, false),
    new("xxxxxxxxxxxxxxxxxxxx"u8, "0123456789ABCDEF"u8, false),
    new("0123456789ABCDEFxxxx"u8, "0123456789ABCDEF"u8, true),
    new("xx0123456789ABCDEFxx"u8, "0123456789ABCDEF"u8, true),
    new("xxxx0123456789ABCDEF"u8, "0123456789ABCDEF"u8, true),
    new("0123456789ABCDEFxxxxx"u8[1..], "0123456789ABCDEF"u8, false),
    new("xxxxx0123456789ABCDEF"u8[..20], "0123456789ABCDEF"u8, false),
    new("xxxxxxxxxxxxxxxxxxxxx"u8, "0123456789ABCDEFG"u8, false),
    new("0123456789ABCDEFGxxxx"u8, "0123456789ABCDEFG"u8, true),
    new("xx0123456789ABCDEFGxx"u8, "0123456789ABCDEFG"u8, true),
    new("xxxx0123456789ABCDEFG"u8, "0123456789ABCDEFG"u8, true),
    new("0123456789ABCDEFGxxxxx"u8[1..], "0123456789ABCDEFG"u8, false),
    new("xxxxx0123456789ABCDEFG"u8[..21], "0123456789ABCDEFG"u8, false),
    new("xx01x"u8, "012"u8, false),
    new("xx0123x"u8, "01234"u8, false),
    new("xx01234567x"u8, "012345678"u8, false),
    new("xx0123456789ABCDEFx"u8, "0123456789ABCDEFG"u8, false)
}.slice();

public static void TestContains(ж<testing.T> Ꮡt) {
    foreach (var (_, ct) in ContainsTests) {
        if (Contains(ct.str, ct.substr) != ct.expected) {
            Ꮡt.Errorf("Contains(%s, %s) = %v, want %v"u8,
                ct.str, ct.substr, !ct.expected, ct.expected);
        }
    }
}

public static slice<ContainsTestsᴛ1> ContainsAnyTests = new ContainsTestsᴛ1[]{
    new(""u8, ""u8, false),
    new(""u8, "a"u8, false),
    new(""u8, "abc"u8, false),
    new("a"u8, ""u8, false),
    new("a"u8, "a"u8, true),
    new("aaa"u8, "a"u8, true),
    new("abc"u8, "xyz"u8, false),
    new("abc"u8, "xcz"u8, true),
    new("a☺b☻c☹d"u8, "uvw☻xyz"u8, true),
    new("aRegExp*"u8, ".(|)*+?^$[]"u8, true),
    new(dots + dots + dots, " "u8, false)
}.slice();

public static void TestContainsAny(ж<testing.T> Ꮡt) {
    foreach (var (_, ct) in ContainsAnyTests) {
        if (ContainsAny(ct.str, ct.substr) != ct.expected) {
            Ꮡt.Errorf("ContainsAny(%s, %s) = %v, want %v"u8,
                ct.str, ct.substr, !ct.expected, ct.expected);
        }
    }
}


partial struct ContainsRuneTestsᴛ1 /*dyn*/ {
    internal @string str;
    internal rune r;
    internal bool expected;
}
public static slice<ContainsRuneTestsᴛ1> ContainsRuneTests = new ContainsRuneTestsᴛ1[]{
    new(""u8, (rune)'a', false),
    new("a"u8, (rune)'a', true),
    new("aaa"u8, (rune)'a', true),
    new("abc"u8, (rune)'y', false),
    new("abc"u8, (rune)'c', true),
    new("a☺b☻c☹d"u8, (rune)'x', false),
    new("a☺b☻c☹d"u8, (rune)'☻', true),
    new("aRegExp*"u8, (rune)'*', true)
}.slice();

public static void TestContainsRune(ж<testing.T> Ꮡt) {
    foreach (var (_, ct) in ContainsRuneTests) {
        if (ContainsRune(ct.str, ct.r) != ct.expected) {
            Ꮡt.Errorf("ContainsRune(%q, %q) = %v, want %v"u8,
                ct.str, ct.r, !ct.expected, ct.expected);
        }
    }
}

public static void TestContainsFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in ContainsRuneTests) {
        ref var ct = ref heap(new ContainsRuneTestsᴛ1(), out var Ꮡct);
        ct = vᴛ1;

        var ctʗ1 = ct;
        if (ContainsFunc(ct.str, (rune r) => ctʗ1.r == r) != ct.expected) {
            Ꮡt.Errorf("ContainsFunc(%q, func(%q)) = %v, want %v"u8,
                ct.str, ct.r, !ct.expected, ct.expected);
        }
    }
}


partial struct EqualFoldTestsᴛ1 /*dyn*/ {
    internal @string s, t;
    internal bool @out;
}
public static slice<EqualFoldTestsᴛ1> EqualFoldTests = new EqualFoldTestsᴛ1[]{
    new("abc"u8, "abc"u8, true),
    new("ABcd"u8, "ABcd"u8, true),
    new("123abc"u8, "123ABC"u8, true),
    new("αβδ"u8, "ΑΒΔ"u8, true),
    new("abc"u8, "xyz"u8, false),
    new("abc"u8, "XYZ"u8, false),
    new("abcdefghijk"u8, "abcdefghijX"u8, false),
    new("abcdefghijk"u8, "abcdefghij\u212A"u8, true),
    new("abcdefghijK"u8, "abcdefghij\u212A"u8, true),
    new("abcdefghijkz"u8, "abcdefghij\u212Ay"u8, false),
    new("abcdefghijKz"u8, "abcdefghij\u212Ay"u8, false),
    new("1"u8, "2"u8, false),
    new("utf-8"u8, "US-ASCII"u8, false)
}.slice();

public static void TestEqualFold(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in EqualFoldTests) {
        {
            var @out = EqualFold(tt.s, tt.t); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v"u8, tt.s, tt.t, @out, tt.@out);
            }
        }
        {
            var @out = EqualFold(tt.t, tt.s); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v"u8, tt.t, tt.s, @out, tt.@out);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testsˢ = "Tests"u8;
internal static readonly object wrongResultˢ = (@string)"wrong result"u8;
internal static readonly @string asciiˢ = "ASCII"u8;
internal static readonly @string unicodePrefixˢ = "UnicodePrefix"u8;
internal static readonly @string unicodeSuffixˢ = "UnicodeSuffix"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string s1ᶜ = "abcdefghijKz"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string s2ᶜ = "abcDefGhijKz"u8;

public static void BenchmarkEqualFold(ж<testing.B> Ꮡb) {
    Ꮡb.Run(testsˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            foreach (var (_, tt) in EqualFoldTests) {
                {
                    var @out = EqualFold(tt.s, tt.t); if (@out != tt.@out) {
                        bΔ1.Fatal(wrongResultˢ);
                    }
                }
            }
        }
    });
    @string s1 = s1ᶜ;
    @string s2 = s2ᶜ;
    Ꮡb.Run(asciiˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            EqualFold(s1, s2);
        }
    });
    Ꮡb.Run(unicodePrefixˢ, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            EqualFold("αβδ" + s1, "ΑΒΔ" + s2);
        }
    });
    Ꮡb.Run(unicodeSuffixˢ, (ж<testing.B> bΔ4) => {
        for (nint i = 0; i < (~bΔ4).N; i++) {
            EqualFold(s1 + "αβδ", s2 + "ΑΒΔ");
        }
    });
}


partial struct CountTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal nint num;
}
public static slice<CountTestsᴛ1> CountTests = new CountTestsᴛ1[]{
    new(""u8, ""u8, 1),
    new(""u8, "notempty"u8, 0),
    new("notempty"u8, ""u8, 9),
    new("smaller"u8, "not smaller"u8, 0),
    new("12345678987654321"u8, "6"u8, 2),
    new("611161116"u8, "6"u8, 3),
    new("notequal"u8, "NotEqual"u8, 0),
    new("equal"u8, "equal"u8, 1),
    new("abc1231231123q"u8, "123"u8, 3),
    new("11111"u8, "11"u8, 2)
}.slice();

public static void TestCount(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in CountTests) {
        {
            nint num = Count(tt.s, tt.sep); if (num != tt.num) {
                Ꮡt.Errorf("Count(%q, %q) = %d, want %d"u8, tt.s, tt.sep, num, tt.num);
            }
        }
    }
}


partial struct cutTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal @string before, after;
    internal bool found;
}
internal static slice<cutTestsᴛ1> cutTests = new cutTestsᴛ1[]{
    new("abc"u8, "b"u8, "a"u8, "c"u8, true),
    new("abc"u8, "a"u8, ""u8, "bc"u8, true),
    new("abc"u8, "c"u8, "ab"u8, ""u8, true),
    new("abc"u8, "abc"u8, ""u8, ""u8, true),
    new("abc"u8, ""u8, ""u8, "abc"u8, true),
    new("abc"u8, "d"u8, "abc"u8, ""u8, false),
    new(""u8, "d"u8, ""u8, ""u8, false),
    new(""u8, ""u8, ""u8, ""u8, true)
}.slice();

public static void TestCut(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cutTests) {
        {
            var (before, after, found) = Cut(tt.s, tt.sep); if (before != tt.before || after != tt.after || found != tt.found) {
                Ꮡt.Errorf("Cut(%q, %q) = %q, %q, %v, want %q, %q, %v"u8, tt.s, tt.sep, before, after, found, tt.before, tt.after, tt.found);
            }
        }
    }
}


partial struct cutPrefixTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal @string after;
    internal bool found;
}
internal static slice<cutPrefixTestsᴛ1> cutPrefixTests = new cutPrefixTestsᴛ1[]{
    new("abc"u8, "a"u8, "bc"u8, true),
    new("abc"u8, "abc"u8, ""u8, true),
    new("abc"u8, ""u8, "abc"u8, true),
    new("abc"u8, "d"u8, "abc"u8, false),
    new(""u8, "d"u8, ""u8, false),
    new(""u8, ""u8, ""u8, true)
}.slice();

public static void TestCutPrefix(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cutPrefixTests) {
        {
            var (after, found) = CutPrefix(tt.s, tt.sep); if (after != tt.after || found != tt.found) {
                Ꮡt.Errorf("CutPrefix(%q, %q) = %q, %v, want %q, %v"u8, tt.s, tt.sep, after, found, tt.after, tt.found);
            }
        }
    }
}


partial struct cutSuffixTestsᴛ1 /*dyn*/ {
    internal @string s, sep;
    internal @string before;
    internal bool found;
}
internal static slice<cutSuffixTestsᴛ1> cutSuffixTests = new cutSuffixTestsᴛ1[]{
    new("abc"u8, "bc"u8, "a"u8, true),
    new("abc"u8, "abc"u8, ""u8, true),
    new("abc"u8, ""u8, "abc"u8, true),
    new("abc"u8, "d"u8, "abc"u8, false),
    new(""u8, "d"u8, ""u8, false),
    new(""u8, ""u8, ""u8, true)
}.slice();

public static void TestCutSuffix(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cutSuffixTests) {
        {
            var (before, found) = CutSuffix(tt.s, tt.sep); if (before != tt.before || found != tt.found) {
                Ꮡt.Errorf("CutSuffix(%q, %q) = %q, %v, want %q, %v"u8, tt.s, tt.sep, before, found, tt.before, tt.found);
            }
        }
    }
}

internal static @string makeBenchInputHard() {
    var tokens = new @string[]{
        "<a>"u8, "<p>"u8, "<b>"u8, "<strong>"u8,
        "</a>"u8, "</p>"u8, "</b>"u8, "</strong>"u8,
        "hello"u8, "world"u8
    }.array();
    var x = new slice<byte>(0, (1 << (int)(20)));
    while (ᐧ) {
        nint i = rand.Intn(len(tokens));
        if (len(x) + len(tokens[i]) >= (1 << (int)(20))) {
            break;
        }
        x = append(x, tokens[i].ꓸꓸꓸ);
    }
    return ((@string)x);
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

public static void BenchmarkIndexHard1(ж<testing.B> Ꮡb) {
    benchmarkIndexHard(Ꮡb, "<>"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string preˢ = "</pre>"u8;

public static void BenchmarkIndexHard2(ж<testing.B> Ꮡb) {
    benchmarkIndexHard(Ꮡb, preˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bHelloWorldBˢ = "<b>hello world</b>"u8;

public static void BenchmarkIndexHard3(ж<testing.B> Ꮡb) {
    benchmarkIndexHard(Ꮡb, bHelloWorldBˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string preBHelloBStrongWorldˢ = "<pre><b>hello</b><strong>world</strong></pre>"u8;

public static void BenchmarkIndexHard4(ж<testing.B> Ꮡb) {
    benchmarkIndexHard(Ꮡb, preBHelloBStrongWorldˢ);
}

public static void BenchmarkLastIndexHard1(ж<testing.B> Ꮡb) {
    benchmarkLastIndexHard(Ꮡb, "<>"u8);
}

public static void BenchmarkLastIndexHard2(ж<testing.B> Ꮡb) {
    benchmarkLastIndexHard(Ꮡb, preˢ);
}

public static void BenchmarkLastIndexHard3(ж<testing.B> Ꮡb) {
    benchmarkLastIndexHard(Ꮡb, bHelloWorldBˢ);
}

public static void BenchmarkCountHard1(ж<testing.B> Ꮡb) {
    benchmarkCountHard(Ꮡb, "<>"u8);
}

public static void BenchmarkCountHard2(ж<testing.B> Ꮡb) {
    benchmarkCountHard(Ꮡb, preˢ);
}

public static void BenchmarkCountHard3(ж<testing.B> Ꮡb) {
    benchmarkCountHard(Ꮡb, bHelloWorldBˢ);
}

internal static @string benchInputTorture = Repeat("ABC"u8, (1 << (int)(10))) + "123"u8 + Repeat("ABC"u8, (1 << (int)(10)));

internal static @string benchNeedleTorture = Repeat("ABC"u8, (1 << (int)(10)) + 1);

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
internal static readonly @string abcˢ2 = "ABC"u8;

public static void BenchmarkCountTortureOverlapping(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string A = Repeat(abcˢ2, (1 << (int)(20)));
    @string B = Repeat(abcˢ2, (1 << (int)(10)));
    for (nint i = 0; i < b.N; i++) {
        Count(A, B);
    }
}

public static void BenchmarkCountByte(ж<testing.B> Ꮡb) {
    var indexSizes = new nint[]{10, 32, (4 << (int)(10)), (4 << (int)(20)), (64 << (int)(20))}.slice();
    @string benchStr = Repeat(benchmarkString,
        (indexSizes[len(indexSizes) - 1] + len(benchmarkString) - 1) / len(benchmarkString));
    void benchFunc(ж<testing.B> bΔ1, @string benchStrΔ1) {
        bΔ1.SetBytes((int64)len(benchStrΔ1));
        for (nint i = 0; i < (~bΔ1).N; i++) {
            Count(benchStrΔ1, "="u8);
        }
    }
    foreach (var (_, size) in indexSizes) {
        var benchFuncʗ1 = benchFunc;
        Ꮡb.Run(fmt.Sprintf("%d"u8, size), (ж<testing.B> bΔ2) => {
            benchFuncʗ1(bΔ2, benchStr.slice(0, size));
        });
    }
}

// Input is ~10% space, ~10% 2-byte UTF-8, rest ASCII non-space.
internal static Func<@string> makeFieldsInput = () => {
    var x = new slice<byte>((1 << (int)(20)));
    foreach (var (i, _) in x) {
        var exprᴛ1 = rand.Intn(10);
        var matchᴛ1 = false;
        if (exprᴛ1 is 0) { matchᴛ1 = true;
            x[i] = (rune)' ';
        }
        else if (exprᴛ1 is 1) { matchᴛ1 = true;
            do {
                if (i > 0 && x[i - 1] == (rune)'x') {
                    copy(x.slice(i - 1), "χ"u8);
                    break;
                }
                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            x[i] = (rune)'x';
        }

    }
    return ((@string)x);
};

// Input is ~10% space, rest ASCII non-space.
internal static Func<@string> makeFieldsInputASCII = () => {
    var x = new slice<byte>((1 << (int)(20)));
    foreach (var (i, _) in x) {
        if (rand.Intn(10) == 0){
            x[i] = (rune)' ';
        } else {
            x[i] = (rune)'x';
        }
    }
    return ((@string)x);
};

internal static slice<mapdataᴛ1> stringdata = new mapdataᴛ1[]{
    new("ASCII"u8, makeFieldsInputASCII()),
    new("Mixed"u8, makeFieldsInput())
}.slice();

public static void BenchmarkFields(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in stringdata) {
        ref var sd = ref heap(new mapdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        Ꮡb.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = (1 << (int)(4)); jᴛ1 <= (1 << (int)(20)); jᴛ1 <<= (int)(4)) {
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

public static void BenchmarkFieldsFunc(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in stringdata) {
        ref var sd = ref heap(new mapdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        Ꮡb.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = (1 << (int)(4)); jᴛ1 <= (1 << (int)(20)); jᴛ1 <<= (int)(4)) {
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
        Split(benchInputHard, "/"u8);
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
        SplitN(benchInputHard, "/"u8, 10);
    }
}

public static void BenchmarkSplitNMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, helloˢ, 10);
    }
}

public static void BenchmarkRepeat(ж<testing.B> Ꮡb) {
    @string s = "0123456789"u8;
    foreach (var (_, n) in new nint[]{5, 10}.slice()) {
        foreach (var (_, c) in new nint[]{0, 1, 2, 6}.slice()) {
            Ꮡb.Run(fmt.Sprintf("%dx%d"u8, n, c), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Repeat(s.slice(0, n), c);
                }
            });
        }
    }
}

public static void BenchmarkRepeatLarge(ж<testing.B> Ꮡb) {
    @string s = Repeat("@"u8, 8 * 1024);
    for (nint j = 8; j <= 30; j++) {
        foreach (var (_, k) in new nint[]{1, 16, 4097}.slice()) {
            @string sΔ1 = s.slice(0, k);
            nint n = quo((((nint)1).Lsh((int64)(j))), k);
            if (n == 0) {
                continue;
            }
            Ꮡb.Run(fmt.Sprintf("%d/%d"u8, ((nint)1).Lsh((int64)(j)), k), (ж<testing.B> bΔ1) => {
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
        Repeat(" "u8, 2);
    }
}

public static void BenchmarkIndexAnyASCII(ж<testing.B> Ꮡb) {
    @string x = Repeat("#"u8, 2048); // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz"u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldHelloWorldˢ = "你好世界, hello world. 你好世界, hello world. 你好世界, hello world."u8;

public static void BenchmarkIndexAnyUTF8(ж<testing.B> Ꮡb) {
    @string x = Repeat("#"u8, 2048); // Never matches set
    @string cs = helloWorldHelloWorldˢ;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyASCII(ж<testing.B> Ꮡb) {
    @string x = Repeat("#"u8, 2048); // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz"u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyUTF8(ж<testing.B> Ꮡb) {
    @string x = Repeat("#"u8, 2048); // Never matches set
    @string cs = helloWorldHelloWorldˢ;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

public static void BenchmarkTrimASCII(ж<testing.B> Ꮡb) {
    @string cs = "0123456789abcdef"u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 4096; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 16; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                @string x = Repeat(cs.slice(0, j), k); // Always matches set
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Trim(x.slice(0, k), cs.slice(0, j));
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string theQuickBrownFoxˢ = "  the quick brown fox   "u8;

public static void BenchmarkTrimByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string x = theQuickBrownFoxˢ;
    for (nint i = 0; i < b.N; i++) {
        Trim(x, " "u8);
    }
}

public static void BenchmarkIndexPeriodic(ж<testing.B> Ꮡb) {
    @string key = "aa"u8;
    foreach (var (_, skip) in new nint[]{2, 4, 8, 16, 32, 64}.array()) {
        Ꮡb.Run(fmt.Sprintf("IndexPeriodic%d"u8, skip), (ж<testing.B> bΔ1) => {
            @string s = Repeat("a"u8 + Repeat(" "u8, skip - 1), (1 << (int)(16)) / skip);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Index(s, key);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string andˢ = " and "u8;

public static void BenchmarkJoin(ж<testing.B> Ꮡb) {
    var vals = new @string[]{"red"u8, "yellow"u8, "pink"u8, "green"u8, "purple"u8, "orange"u8, "blue"u8}.slice();
    for (nint lᴛ1 = 0; lᴛ1 <= len(vals); lᴛ1++) {
        var l = lᴛ1;
        var valsʗ1 = vals;
        Ꮡb.Run(strconv.Itoa(l), (ж<testing.B> bΔ1) => {
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

public static void BenchmarkTrimSpace(ж<testing.B> Ꮡb) {
    var tests = new BenchmarkTrimSpace_tests[]{
        new("NoTrim"u8, "typical"u8),
        new("ASCII"u8, "  foo bar  "u8),
        new("SomeNonASCII"u8, "    \u2000\t\r\n x\t\t\r\r\ny\n \u3000    "u8),
        new("JustNonASCII"u8, "\u2000\u2000\u2000☺☺☺☺\u3000\u3000\u3000"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new BenchmarkTrimSpace_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                TrimSpace(testʗ1.input);
            }
        });
    }
}

internal static @string stringSink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bananaˢ = "banana"u8;

public static void BenchmarkReplaceAll(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        stringSink = ReplaceAll(bananaˢ, "a"u8, "<>"u8);
    }
}

} // end strings_test_package
