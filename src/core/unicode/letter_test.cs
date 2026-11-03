// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using Δruntime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using testing = testing_package;
using static unicode_package;
using Δunicode = unicode_package;

partial class unicode_test_package {

internal static slice<rune> upperTest = slice<rune>([
    0x41,
    0xc0,
    0xd8,
    0x100,
    0x139,
    0x14a,
    0x178,
    0x181,
    0x376,
    0x3cf,
    0x13bd,
    0x1f2a,
    0x2102,
    0x2c00,
    0x2c10,
    0x2c20,
    0xa650,
    0xa722,
    0xff3a,
    0x10400,
    0x1d400,
    0x1d7ca
]);

internal static slice<rune> notupperTest = slice<rune>([
    0x40,
    0x5b,
    0x61,
    0x185,
    0x1b0,
    0x377,
    0x387,
    0x2150,
    0xab7d,
    0xffff,
    0x10000
]);

internal static slice<rune> letterTest = slice<rune>([
    0x41,
    0x61,
    0xaa,
    0xba,
    0xc8,
    0xdb,
    0xf9,
    0x2ec,
    0x535,
    0x620,
    0x6e6,
    0x93d,
    0xa15,
    0xb99,
    0xdc0,
    0xedd,
    0x1000,
    0x1200,
    0x1312,
    0x1401,
    0x2c00,
    0xa800,
    0xf900,
    0xfa30,
    0xffda,
    0xffdc,
    0x10000,
    0x10300,
    0x10400,
    0x20000,
    0x2f800,
    0x2fa1d
]);

internal static slice<rune> notletterTest = slice<rune>([
    0x20,
    0x35,
    0x375,
    0x619,
    0x700,
    0x1885,
    0xfffe,
    0x1ffff,
    0x10ffff
]);

// Contains all the special cased Latin-1 chars.
internal static slice<rune> spaceTest = slice<rune>([
    0x09,
    0x0a,
    0x0b,
    0x0c,
    0x0d,
    0x20,
    0x85,
    0xA0,
    0x2000,
    0x3000
]);

partial struct caseT {
    internal nint cas;
    internal rune @in, @out;
}

internal static slice<caseT> caseTest = slice<caseT>([
    // errors
    new(-1, '\n', 0xFFFD),
    new(UpperCase, -1, -1),
    new(UpperCase, (rune)(1 << 30), (rune)(1 << 30)),

    // ASCII (special-cased so test carefully)
    new(UpperCase, '\n', '\n'),
    new(UpperCase, 'a', 'A'),
    new(UpperCase, 'A', 'A'),
    new(UpperCase, '7', '7'),
    new(LowerCase, '\n', '\n'),
    new(LowerCase, 'a', 'a'),
    new(LowerCase, 'A', 'a'),
    new(LowerCase, '7', '7'),
    new(TitleCase, '\n', '\n'),
    new(TitleCase, 'a', 'A'),
    new(TitleCase, 'A', 'A'),
    new(TitleCase, '7', '7'),

    // Latin-1: easy to read the tests!
    new(UpperCase, 0x80, 0x80),
    new(UpperCase, 'Å', 'Å'),
    new(UpperCase, 'å', 'Å'),
    new(LowerCase, 0x80, 0x80),
    new(LowerCase, 'Å', 'å'),
    new(LowerCase, 'å', 'å'),
    new(TitleCase, 0x80, 0x80),
    new(TitleCase, 'Å', 'Å'),
    new(TitleCase, 'å', 'Å'),

    // 0131;LATIN SMALL LETTER DOTLESS I;Ll;0;L;;;;;N;;;0049;;0049
    new(UpperCase, 0x0131, 'I'),
    new(LowerCase, 0x0131, 0x0131),
    new(TitleCase, 0x0131, 'I'),

    // 0133;LATIN SMALL LIGATURE IJ;Ll;0;L;<compat> 0069 006A;;;;N;LATIN SMALL LETTER I J;;0132;;0132
    new(UpperCase, 0x0133, 0x0132),
    new(LowerCase, 0x0133, 0x0133),
    new(TitleCase, 0x0133, 0x0132),

    // 212A;KELVIN SIGN;Lu;0;L;004B;;;;N;DEGREES KELVIN;;;006B;
    new(UpperCase, 0x212A, 0x212A),
    new(LowerCase, 0x212A, 'k'),
    new(TitleCase, 0x212A, 0x212A),

    // From an UpperLower sequence
    // A640;CYRILLIC CAPITAL LETTER ZEMLYA;Lu;0;L;;;;;N;;;;A641;
    new(UpperCase, 0xA640, 0xA640),
    new(LowerCase, 0xA640, 0xA641),
    new(TitleCase, 0xA640, 0xA640),
    // A641;CYRILLIC SMALL LETTER ZEMLYA;Ll;0;L;;;;;N;;;A640;;A640
    new(UpperCase, 0xA641, 0xA640),
    new(LowerCase, 0xA641, 0xA641),
    new(TitleCase, 0xA641, 0xA640),
    // A64E;CYRILLIC CAPITAL LETTER NEUTRAL YER;Lu;0;L;;;;;N;;;;A64F;
    new(UpperCase, 0xA64E, 0xA64E),
    new(LowerCase, 0xA64E, 0xA64F),
    new(TitleCase, 0xA64E, 0xA64E),
    // A65F;CYRILLIC SMALL LETTER YN;Ll;0;L;;;;;N;;;A65E;;A65E
    new(UpperCase, 0xA65F, 0xA65E),
    new(LowerCase, 0xA65F, 0xA65F),
    new(TitleCase, 0xA65F, 0xA65E),

    // From another UpperLower sequence
    // 0139;LATIN CAPITAL LETTER L WITH ACUTE;Lu;0;L;004C 0301;;;;N;LATIN CAPITAL LETTER L ACUTE;;;013A;
    new(UpperCase, 0x0139, 0x0139),
    new(LowerCase, 0x0139, 0x013A),
    new(TitleCase, 0x0139, 0x0139),
    // 013F;LATIN CAPITAL LETTER L WITH MIDDLE DOT;Lu;0;L;<compat> 004C 00B7;;;;N;;;;0140;
    new(UpperCase, 0x013f, 0x013f),
    new(LowerCase, 0x013f, 0x0140),
    new(TitleCase, 0x013f, 0x013f),
    // 0148;LATIN SMALL LETTER N WITH CARON;Ll;0;L;006E 030C;;;;N;LATIN SMALL LETTER N HACEK;;0147;;0147
    new(UpperCase, 0x0148, 0x0147),
    new(LowerCase, 0x0148, 0x0148),
    new(TitleCase, 0x0148, 0x0147),

    // Lowercase lower than uppercase.
    // AB78;CHEROKEE SMALL LETTER GE;Ll;0;L;;;;;N;;;13A8;;13A8
    new(UpperCase, 0xab78, 0x13a8),
    new(LowerCase, 0xab78, 0xab78),
    new(TitleCase, 0xab78, 0x13a8),
    new(UpperCase, 0x13a8, 0x13a8),
    new(LowerCase, 0x13a8, 0xab78),
    new(TitleCase, 0x13a8, 0x13a8),

    // Last block in the 5.1.0 table
    // 10400;DESERET CAPITAL LETTER LONG I;Lu;0;L;;;;;N;;;;10428;
    new(UpperCase, 0x10400, 0x10400),
    new(LowerCase, 0x10400, 0x10428),
    new(TitleCase, 0x10400, 0x10400),
    // 10427;DESERET CAPITAL LETTER EW;Lu;0;L;;;;;N;;;;1044F;
    new(UpperCase, 0x10427, 0x10427),
    new(LowerCase, 0x10427, 0x1044F),
    new(TitleCase, 0x10427, 0x10427),
    // 10428;DESERET SMALL LETTER LONG I;Ll;0;L;;;;;N;;;10400;;10400
    new(UpperCase, 0x10428, 0x10400),
    new(LowerCase, 0x10428, 0x10428),
    new(TitleCase, 0x10428, 0x10400),
    // 1044F;DESERET SMALL LETTER EW;Ll;0;L;;;;;N;;;10427;;10427
    new(UpperCase, 0x1044F, 0x10427),
    new(LowerCase, 0x1044F, 0x1044F),
    new(TitleCase, 0x1044F, 0x10427),

    // First one not in the 5.1.0 table
    // 10450;SHAVIAN LETTER PEEP;Lo;0;L;;;;;N;;;;;
    new(UpperCase, 0x10450, 0x10450),
    new(LowerCase, 0x10450, 0x10450),
    new(TitleCase, 0x10450, 0x10450),

    // Non-letters with case.
    new(LowerCase, 0x2161, 0x2171),
    new(UpperCase, 0x0345, 0x0399)
]);

public static void TestIsLetter(ж<testing.T> t) {
    foreach (var (_, r) in upperTest) {
        if (!IsLetter(r)) {
            t.Errorf("IsLetter(U+%04X) = false, want true", r);
        }
    }
    foreach (var (_, r) in letterTest) {
        if (!IsLetter(r)) {
            t.Errorf("IsLetter(U+%04X) = false, want true", r);
        }
    }
    foreach (var (_, r) in notletterTest) {
        if (IsLetter(r)) {
            t.Errorf("IsLetter(U+%04X) = true, want false", r);
        }
    }
}

public static void TestIsUpper(ж<testing.T> t) {
    foreach (var (_, r) in upperTest) {
        if (!IsUpper(r)) {
            t.Errorf("IsUpper(U+%04X) = false, want true", r);
        }
    }
    foreach (var (_, r) in notupperTest) {
        if (IsUpper(r)) {
            t.Errorf("IsUpper(U+%04X) = true, want false", r);
        }
    }
    foreach (var (_, r) in notletterTest) {
        if (IsUpper(r)) {
            t.Errorf("IsUpper(U+%04X) = true, want false", r);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string upperCaseˢ => ref upperCaseˢᵇ;
private static readonly @string upperCaseˢᵇ = "UpperCase"u8;
private static ref readonly @string lowerCaseˢ => ref lowerCaseˢᵇ;
private static readonly @string lowerCaseˢᵇ = "LowerCase"u8;
private static ref readonly @string titleCaseˢ => ref titleCaseˢᵇ;
private static readonly @string titleCaseˢᵇ = "TitleCase"u8;
private static ref readonly @string errorCaseˢ => ref errorCaseˢᵇ;
private static readonly @string errorCaseˢᵇ = "ErrorCase"u8;

internal static @string caseString(nint c) {
    switch (c) {
    case var ᴛ when ᴛ == UpperCase: {
        return upperCaseˢ;
    }
    case var ᴛ when ᴛ == LowerCase: {
        return lowerCaseˢ;
    }
    case var ᴛ when ᴛ == TitleCase: {
        return titleCaseˢ;
    }}
    return errorCaseˢ;
}

public static void TestTo(ж<testing.T> t) {
    foreach (var (_, c) in caseTest) {
        var r = To(c.cas, c.@in);
        if (c.@out != r) {
            t.Errorf("To(U+%04X, %s) = U+%04X want U+%04X", c.@in, caseString(c.cas), r, c.@out);
        }
    }
}

public static void TestToUpperCase(ж<testing.T> t) {
    foreach (var (_, c) in caseTest) {
        if (c.cas != UpperCase) {
            continue;
        }
        var r = ToUpper(c.@in);
        if (c.@out != r) {
            t.Errorf("ToUpper(U+%04X) = U+%04X want U+%04X", c.@in, r, c.@out);
        }
    }
}

public static void TestToLowerCase(ж<testing.T> t) {
    foreach (var (_, c) in caseTest) {
        if (c.cas != LowerCase) {
            continue;
        }
        var r = ToLower(c.@in);
        if (c.@out != r) {
            t.Errorf("ToLower(U+%04X) = U+%04X want U+%04X", c.@in, r, c.@out);
        }
    }
}

public static void TestToTitleCase(ж<testing.T> t) {
    foreach (var (_, c) in caseTest) {
        if (c.cas != TitleCase) {
            continue;
        }
        var r = ToTitle(c.@in);
        if (c.@out != r) {
            t.Errorf("ToTitle(U+%04X) = U+%04X want U+%04X", c.@in, r, c.@out);
        }
    }
}

public static void TestIsSpace(ж<testing.T> t) {
    foreach (var (_, c) in spaceTest) {
        if (!IsSpace(c)) {
            t.Errorf("IsSpace(U+%04X) = false; want true", c);
        }
    }
    foreach (var (_, c) in letterTest) {
        if (IsSpace(c)) {
            t.Errorf("IsSpace(U+%04X) = true; want false", c);
        }
    }
}

// Check that the optimizations for IsLetter etc. agree with the tables.
// We only need to check the Latin-1 range.
public static void TestLetterOptimizations(ж<testing.T> t) {
    for (var i = (rune)0; i <= MaxLatin1; i++) {
        if (Is(Letter, i) != IsLetter(i)) {
            t.Errorf("IsLetter(U+%04X) disagrees with Is(Letter)", i);
        }
        if (Is(Upper, i) != IsUpper(i)) {
            t.Errorf("IsUpper(U+%04X) disagrees with Is(Upper)", i);
        }
        if (Is(Lower, i) != IsLower(i)) {
            t.Errorf("IsLower(U+%04X) disagrees with Is(Lower)", i);
        }
        if (Is(Title, i) != IsTitle(i)) {
            t.Errorf("IsTitle(U+%04X) disagrees with Is(Title)", i);
        }
        if (Is(White_Space, i) != IsSpace(i)) {
            t.Errorf("IsSpace(U+%04X) disagrees with Is(White_Space)", i);
        }
        if (To(UpperCase, i) != ToUpper(i)) {
            t.Errorf("ToUpper(U+%04X) disagrees with To(Upper)", i);
        }
        if (To(LowerCase, i) != ToLower(i)) {
            t.Errorf("ToLower(U+%04X) disagrees with To(Lower)", i);
        }
        if (To(TitleCase, i) != ToTitle(i)) {
            t.Errorf("ToTitle(U+%04X) disagrees with To(Title)", i);
        }
    }
}

public static void TestTurkishCase(ж<testing.T> t) {
    var lower = slice<rune>((@string)"abcçdefgğhıijklmnoöprsştuüvyz");
    var upper = slice<rune>((@string)"ABCÇDEFGĞHIİJKLMNOÖPRSŞTUÜVYZ");
    foreach (var (i, l) in lower) {
        var u = upper[i];
        if (TurkishCase.ToLower(l) != l) {
            t.Errorf("lower(U+%04X) is U+%04X not U+%04X", l, TurkishCase.ToLower(l), l);
        }
        if (TurkishCase.ToUpper(u) != u) {
            t.Errorf("upper(U+%04X) is U+%04X not U+%04X", u, TurkishCase.ToUpper(u), u);
        }
        if (TurkishCase.ToUpper(l) != u) {
            t.Errorf("upper(U+%04X) is U+%04X not U+%04X", l, TurkishCase.ToUpper(l), u);
        }
        if (TurkishCase.ToLower(u) != l) {
            t.Errorf("lower(U+%04X) is U+%04X not U+%04X", u, TurkishCase.ToLower(l), l);
        }
        if (TurkishCase.ToTitle(u) != u) {
            t.Errorf("title(U+%04X) is U+%04X not U+%04X", u, TurkishCase.ToTitle(u), u);
        }
        if (TurkishCase.ToTitle(l) != u) {
            t.Errorf("title(U+%04X) is U+%04X not U+%04X", l, TurkishCase.ToTitle(l), u);
        }
    }
}

internal static slice<@string> simpleFoldTests = slice<@string>([
    // SimpleFold(x) returns the next equivalent rune > x or wraps
    // around to smaller values.

    // Easy cases.
    "Aa",
    "δΔ",

    // ASCII special cases.
    "KkK",
    "Ssſ",

    // Non-ASCII special cases.
    "ρϱΡ",
    "ͅΙιι",

    // Extra special cases: has lower/upper but no case fold.
    "İ",
    "ı",

    // Upper comes before lower (Cherokee).
    "\u13b0\uab80"
]);

public static void TestSimpleFold(ж<testing.T> t) {
    foreach (var (_, tt) in simpleFoldTests) {
        var cycle = slice<rune>(tt);
        var r = cycle[len(cycle) - 1];
        foreach (var (_, @out) in cycle) {
            {
                var rΔ1 = SimpleFold(r); if (rΔ1 != @out) {
                    t.Errorf("SimpleFold(%#U) = %#U, want %#U", rΔ1, rΔ1, @out);
                }
            }
            r = @out;
        }
    }

    {
        var r = SimpleFold(-42); if (r != -42) {
            t.Errorf("SimpleFold(-42) = %v, want -42", r);
        }
    }
}

// Running 'go test -calibrate' runs the calibration to find a plausible
// cutoff point for linear search of a range list vs. binary search.
// We create a fake table and then time how long it takes to do a
// sequence of searches within that table, for all possible inputs
// relative to the ranges (something before all, in each, between each, after all).
// This assumes that all possible runes are equally likely.
// In practice most runes are ASCII so this is a conservative estimate
// of an effective cutoff value. In practice we could probably set it higher
// than what this function recommends.
internal static ж<bool> calibrate = flag.Bool("calibrate", false, "compute crossover for linear vs. binary search");

public static void TestCalibrate(ж<testing.T> t) {
    if (!calibrate.Value) {
        return;
    }

    if (Δruntime.GOARCH == "amd64"u8) {
        fmt.Printf("warning: running calibration on %s\n"u8, Δruntime.GOARCH);
    }

    // Find the point where binary search wins by more than 10%.
    // The 10% bias gives linear search an edge when they're close,
    // because on predominantly ASCII inputs linear search is even
    // better than our benchmarks measure.
    nint n = sort.Search(64, (nint nΔ1) => {
        var tab = fakeTable(nΔ1);
        var tabʗ1 = tab;
        var blinear = (ж<testing.B> b) => {
            var tabΔ1 = tabʗ1;
            nint max = nΔ1 * 5 + 20;
            for (nint i = 0; i < (~b).N; i++) {
                for (nint j = 0; j <= max; j++) {
                    linear(tabΔ1, (uint16)j);
                }
            }
        };
        var tabʗ2 = tab;
        var bbinary = (ж<testing.B> b) => {
            var tabΔ2 = tabʗ2;
            nint max = nΔ1 * 5 + 20;
            for (nint i = 0; i < (~b).N; i++) {
                for (nint j = 0; j <= max; j++) {
                    binary(tabΔ2, (uint16)j);
                }
            }
        };
        var bmlinear = testing.Benchmark(blinear);
        var bmbinary = testing.Benchmark(bbinary);
        fmt.Printf("n=%d: linear=%d binary=%d\n"u8, nΔ1, bmlinear.NsPerOp(), bmbinary.NsPerOp());
        return bmlinear.NsPerOp() * 100 > bmbinary.NsPerOp() * 110;
    });
    fmt.Printf("calibration: linear cutoff = %d\n"u8, n);
}

internal static slice<Δunicode.Range16> fakeTable(nint n) {
    slice<Δunicode.Range16> r16 = default;
    for (nint i = 0; i < n; i++) {
        r16 = append(r16, new Range16((uint16)(i * 5 + 10), (uint16)(i * 5 + 12), 1));
    }
    return r16;
}

internal static bool linear(slice<Δunicode.Range16> ranges, uint16 r) {
    foreach (var i in range(ranges)) {
        var range_ = Ꮡ(ranges, i);
        if (r < range_.Lo) {
            return false;
        }
        if (r <= range_.Hi) {
            return (uint16)((uint16)(r - range_.Lo) % range_.Stride) == 0;
        }
    }
    return false;
}

internal static bool binary(slice<Δunicode.Range16> ranges, uint16 r) {
    // binary search over ranges
    nint lo = 0;
    nint hi = len(ranges);
    while (lo < hi) {
        nint m = (nint)((nuint)(lo + hi) >> 1);
        var range_ = Ꮡ(ranges, m);
        if (range_.Lo <= r && r <= range_.Hi) {
            return (uint16)((uint16)(r - range_.Lo) % range_.Stride) == 0;
        }
        if (r < range_.Lo) {
            hi = m;
        } else {
            lo = m + 1;
        }
    }
    return false;
}

public static void TestLatinOffset(ж<testing.T> t) {
    slice<map<@string, ж<Δunicode.RangeTable>>> maps = slice<map<@string, ж<Δunicode.RangeTable>>>([
        Categories,
        FoldCategory,
        FoldScript,
        Properties,
        Scripts
    ]);
    foreach (var (_, m) in maps) {
        foreach (var (name, tab) in m) {
            nint i = 0;
            while (i < len(tab.R16) && tab.R16[i].Hi <= MaxLatin1) {
                i++;
            }
            if (tab.LatinOffset != i) {
                t.Errorf("%s: LatinOffset=%d, want %d", name, tab.LatinOffset, i);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "ABC"u8;
private static ref readonly @string abcˢ2 => ref abcˢ2ᵇ;
private static readonly @string abcˢ2ᵇ = "Abc"u8;

public static void TestSpecialCaseNoMapping(ж<testing.T> t) {
    // Issue 25636
    // no change for rune 'A', zero delta, under upper/lower/title case change.
    Δunicode.CaseRange noChangeForCapitalA = new CaseRange('A', 'A', new rune[]{0, 0, 0}.array());
    @string got = strings.ToLowerSpecial((Δunicode.SpecialCase)slice<Δunicode.CaseRange>([noChangeForCapitalA.ΔClone()]), abcˢ);
    @string want = abcˢ2;
    if (got != want) {
        t.Errorf("got %q; want %q", got, want);
    }
}

public static void TestNegativeRune(ж<testing.T> t) {
    // Issue 43254
    // These tests cover negative rune handling by testing values which,
    // when cast to uint8 or uint16, look like a particular valid rune.
    // This package has Latin-1-specific optimizations, so we test all of
    // Latin-1 and representative non-Latin-1 values in the character
    // categories covered by IsGraphic, etc.
    var nonLatin1 = slice<uint32>([
        // Lu: LATIN CAPITAL LETTER A WITH MACRON
        0x0100,
        // Ll: LATIN SMALL LETTER A WITH MACRON
        0x0101,
        // Lt: LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON
        0x01C5,
        // M: COMBINING GRAVE ACCENT
        0x0300,
        // Nd: ARABIC-INDIC DIGIT ZERO
        0x0660,
        // P: GREEK QUESTION MARK
        0x037E,
        // S: MODIFIER LETTER LEFT ARROWHEAD
        0x02C2,
        // Z: OGHAM SPACE MARK
        0x1680
    ]);
    for (nint i = 0; i < (nint)MaxLatin1 + len(nonLatin1); i++) {
        var @base = (uint32)i;
        if (i >= MaxLatin1) {
            @base = nonLatin1[i - (nint)MaxLatin1];
        }

        // Note r is negative, but uint8(r) == uint8(base) and
        // uint16(r) == uint16(base).
        var r = (rune)(@base - ((uint32)1 << 31));
        if (Is(Letter, r)) {
            t.Errorf("Is(Letter, 0x%x - 1<<31) = true, want false", @base);
        }
        if (IsControl(r)) {
            t.Errorf("IsControl(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsDigit(r)) {
            t.Errorf("IsDigit(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsGraphic(r)) {
            t.Errorf("IsGraphic(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsLetter(r)) {
            t.Errorf("IsLetter(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsLower(r)) {
            t.Errorf("IsLower(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsMark(r)) {
            t.Errorf("IsMark(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsNumber(r)) {
            t.Errorf("IsNumber(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsPrint(r)) {
            t.Errorf("IsPrint(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsPunct(r)) {
            t.Errorf("IsPunct(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsSpace(r)) {
            t.Errorf("IsSpace(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsSymbol(r)) {
            t.Errorf("IsSymbol(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsTitle(r)) {
            t.Errorf("IsTitle(0x%x - 1<<31) = true, want false", @base);
        }
        if (IsUpper(r)) {
            t.Errorf("IsUpper(0x%x - 1<<31) = true, want false", @base);
        }
    }
}

public static void BenchmarkToUpper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        _ = ToUpper('δ');
    }
}

public static void BenchmarkToLower(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        _ = ToLower('Δ');
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string upperˢ => ref upperˢᵇ;
private static readonly @string upperˢᵇ = "Upper"u8;
private static ref readonly @string lowerˢ => ref lowerˢᵇ;
private static readonly @string lowerˢᵇ = "Lower"u8;
private static ref readonly @string foldˢ => ref foldˢᵇ;
private static readonly @string foldˢᵇ = "Fold"u8;
private static ref readonly @string noFoldˢ => ref noFoldˢᵇ;
private static readonly @string noFoldˢᵇ = "NoFold"u8;

public static void BenchmarkSimpleFold(ж<testing.B> b) {
    void bench(@string name, rune r) {
        b.Run(name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                _ = SimpleFold(r);
            }
        });
    }
    bench(upperˢ, 'Δ');
    bench(lowerˢ, 'δ');
    bench(foldˢ, '\u212A');
    bench(noFoldˢ, '習');
}

} // end unicode_test_package
