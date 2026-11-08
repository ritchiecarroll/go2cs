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

public static void TestFinderNext(ж<testing.T> t) {
    var testCases = slice<TestFinderNext_testCases>([
        new("", "", 0),
        new("", "abc", 0),
        new("abc", "", -1),
        new("abc", "abc", 0),
        new("d", "abcdefg", 3),
        new("nan", "banana", 2),
        new("pan", "anpanman", 2),
        new("nnaaman", "anpanmanam", -1),
        new("abcd", "abc", -1),
        new("abcd", "bcd", -1),
        new("bcd", "abcd", 1),
        new("abc", "acca", -1),
        new("aa", "aaa", 0),
        new("baa", "aaaaa", -1),
        new("at that", "which finally halts.  at that point", 22)
    ]);

    foreach (var (_, tc) in range(testCases)) {
        nint got = strings_internal_test_package.StringFind(tc.pat, tc.text);
        nint want = tc.index;
        if (got != want) {
            t.Errorf("stringFind(%q, %q) got %d, want %d\n", tc.pat, tc.text, got, want);
        }
    }
}

internal partial struct TestFinderCreation_testCases /*dyn*/ {
    internal @string pattern;
    internal array<nint> bad = new(256);
    internal slice<nint> suf;
}

public static void TestFinderCreation(ж<testing.T> t) {
    var testCases = slice<TestFinderCreation_testCases>([
        new(
            "abc",
            new array<nint>(256){['a'] = 2, ['b'] = 1, ['c'] = 3},
            [5, 4, 1]
        ),
        new(
            "mississi",
            new array<nint>(256){['i'] = 3, ['m'] = 7, ['s'] = 1},
            [15, 14, 13, 7, 11, 10, 7, 1]
        ),
        // From https://www.cs.utexas.edu/~moore/publications/fstrpos.pdf
        new(
            "abcxxxabc",
            new array<nint>(256){['a'] = 2, ['b'] = 1, ['c'] = 6, ['x'] = 3},
            [14, 13, 12, 11, 10, 9, 11, 10, 1]
        ),
        new(
            "abyxcdeyx",
            new array<nint>(256){['a'] = 8, ['b'] = 7, ['c'] = 4, ['d'] = 3, ['e'] = 2, ['y'] = 1, ['x'] = 5},
            [17, 16, 15, 14, 13, 12, 7, 10, 1]
        )
    ]);

    foreach (var (_, vᴛ1) in range(testCases)) {
        var tc = vᴛ1.ΔClone();

        var (bad, good) = strings_internal_test_package.DumpTables(tc.pattern);

        foreach (var (i, got) in range(bad)) {
            nint want = tc.bad[i];
            if (want == 0) {
                want = len(tc.pattern);
            }
            if (got != want) {
                t.Errorf("boyerMoore(%q) bad['%c']: got %d want %d", tc.pattern, i, got, want);
            }
        }

        if (!slices.Equal<slice<nint>, nint>(good, tc.suf)) {
            t.Errorf("boyerMoore(%q) got %v want %v", tc.pattern, good, tc.suf);
        }
    }
}

} // end strings_test_package
