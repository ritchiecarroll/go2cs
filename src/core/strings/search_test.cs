// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using slices = slices_package;
using static strings_package;
using testing = testing_package;
using static go.strings_internal_test_package;

partial class strings_test_package {

internal partial struct TestFinderNext_testCases /*dyn*/ {
    internal @string pat, text;
    internal nint index;
}

public static void TestFinderNext(ж<testing.T> Ꮡt) {
    var testCases = new TestFinderNext_testCases[]{
        new(""u8, ""u8, 0),
        new(""u8, "abc"u8, 0),
        new("abc"u8, ""u8, -1),
        new("abc"u8, "abc"u8, 0),
        new("d"u8, "abcdefg"u8, 3),
        new("nan"u8, "banana"u8, 2),
        new("pan"u8, "anpanman"u8, 2),
        new("nnaaman"u8, "anpanmanam"u8, -1),
        new("abcd"u8, "abc"u8, -1),
        new("abcd"u8, "bcd"u8, -1),
        new("bcd"u8, "abcd"u8, 1),
        new("abc"u8, "acca"u8, -1),
        new("aa"u8, "aaa"u8, 0),
        new("baa"u8, "aaaaa"u8, -1),
        new("at that"u8, "which finally halts.  at that point"u8, 22)
    }.slice();
    foreach (var (_, tc) in testCases) {
        nint got = strings_internal_test_package.StringFind(tc.pat, tc.text);
        nint want = tc.index;
        if (got != want) {
            Ꮡt.Errorf("stringFind(%q, %q) got %d, want %d\n"u8, tc.pat, tc.text, got, want);
        }
    }
}

internal partial struct TestFinderCreation_testCases /*dyn*/ {
    internal @string pattern;
    internal array<nint> bad = new(256);
    internal slice<nint> suf;
}

public static void TestFinderCreation(ж<testing.T> Ꮡt) {
    var testCases = new TestFinderCreation_testCases[]{
        new(
            "abc"u8,
            new array<nint>(256){[(rune)'a'] = 2, [(rune)'b'] = 1, [(rune)'c'] = 3},
            new nint[]{5, 4, 1}.slice()
        ),
        new(
            "mississi"u8,
            new array<nint>(256){[(rune)'i'] = 3, [(rune)'m'] = 7, [(rune)'s'] = 1},
            new nint[]{15, 14, 13, 7, 11, 10, 7, 1}.slice()
        ), // From https://www.cs.utexas.edu/~moore/publications/fstrpos.pdf

        new(
            "abcxxxabc"u8,
            new array<nint>(256){[(rune)'a'] = 2, [(rune)'b'] = 1, [(rune)'c'] = 6, [(rune)'x'] = 3},
            new nint[]{14, 13, 12, 11, 10, 9, 11, 10, 1}.slice()
        ),
        new(
            "abyxcdeyx"u8,
            new array<nint>(256){[(rune)'a'] = 8, [(rune)'b'] = 7, [(rune)'c'] = 4, [(rune)'d'] = 3, [(rune)'e'] = 2, [(rune)'y'] = 1, [(rune)'x'] = 5},
            new nint[]{17, 16, 15, 14, 13, 12, 7, 10, 1}.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        var tc = vᴛ1.ΔClone();

        var (bad, good) = strings_internal_test_package.DumpTables(tc.pattern);
        foreach (var (i, got) in bad) {
            nint want = tc.bad[i];
            if (want == 0) {
                want = len(tc.pattern);
            }
            if (got != want) {
                Ꮡt.Errorf("boyerMoore(%q) bad['%c']: got %d want %d"u8, tc.pattern, i, got, want);
            }
        }
        if (!slices.Equal<slice<nint>, nint>(good, tc.suf)) {
            Ꮡt.Errorf("boyerMoore(%q) got %v want %v"u8, tc.pattern, good, tc.suf);
        }
    }
}

} // end strings_test_package
