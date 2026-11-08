// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// Derived from bytes/compare_test.go.
// Benchmarks omitted since the underlying implementation is identical.
using testenv = @internal.testenv_package;
using static strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using static go.strings_internal_test_package;

partial class strings_test_package {

partial struct compareTestsᴛ1 /*dyn*/ {
    internal @string a, b;
    internal nint i;
}
internal static slice<compareTestsᴛ1> compareTests = [
    new("", "", 0),
    new("a", "", 1),
    new("", "a", -1),
    new("abc", "abc", 0),
    new("ab", "abc", -1),
    new("abc", "ab", 1),
    new("x", "ab", 1),
    new("ab", "x", -1),
    new("x", "a", 1),
    new("b", "x", -1),
    // test runtime·memeq's chunked implementation
    new("abcdefgh", "abcdefgh", 0),
    new("abcdefghi", "abcdefghi", 0),
    new("abcdefghi", "abcdefghj", -1)
];

public static void TestCompare(ж<testing.T> t) {
    foreach (var (_, tt) in range(compareTests)) {
        nint numShifts = 16;
        for (nint offset = 0; offset <= numShifts; offset++) {
            @string shiftedB = (Repeat("*", offset) + tt.b).slice(offset);
            nint cmp = Compare(tt.a, shiftedB);
            if (cmp != tt.i) {
                t.Errorf(@"Compare(%q, %q), offset %d = %v; want %v", tt.a, tt.b, offset, cmp, tt.i);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloGophersˢ => ref helloGophersˢᵇ;
private static readonly @string helloGophersˢᵇ = "Hello Gophers!"u8;
internal static readonly object sS1Failedˢ = (@string)"s > s[:1] failed"u8;

public static void TestCompareIdenticalString(ж<testing.T> t) {
    @string s = helloGophersˢ;
    if (Compare(s, s) != 0) {
        t.Error((@string)"s != s");
    }
    if (Compare(s, s[..1]) != 1) {
        t.Error(sS1Failedˢ);
    }
}

public static void TestCompareStrings(ж<testing.T> t) {
    var lengthsᴷ = new stackAppend<nint>();

    // unsafeString converts a []byte to a string with no allocation.
    // The caller must not modify b while the result string is in use.
    @string unsafeString(slice<byte> bΔ1) => @unsafe.String(@unsafe.SliceData(bΔ1), len(bΔ1));

    var lengths = new slice<nint>(0); // lengths to test in ascending order
    for (nint i = 0; i <= 128; i++) {
        lengths = append(lengthsᴷ, lengths, i);
    }
    lengths = append(lengths, (nint)256, (nint)512, (nint)1024, (nint)1333, (nint)4095, (nint)4096, (nint)4097);

    if (!testing.Short() || testenv.Builder() != ""u8) {
        lengths = append(lengths, (nint)65535, (nint)65536, (nint)65537, (nint)99999);
    }

    nint n = lengths[len(lengths) - 1];
    var a = new slice<byte>(n + 1);
    var b = new slice<byte>(n + 1);
    nint lastLen = 0;
    foreach (var (_, lenΔ1) in range(lengths)) {
        // randomish but deterministic data. No 0 or 255.
        for (nint i = 0; i < lenΔ1; i++) {
            a[i] = (byte)(1 + 31 * i % 254);
            b[i] = (byte)(1 + 31 * i % 254);
        }
        // data past the end is different
        for (nint i = lenΔ1; i <= n; i++) {
            a[i] = 8;
            b[i] = 9;
        }

        @string sa = unsafeString(a);
        @string sb = unsafeString(b);
        nint cmp = Compare(sa.slice(0, lenΔ1), sb.slice(0, lenΔ1));
        if (cmp != 0) {
            t.Errorf(@"CompareIdentical(%d) = %d", lenΔ1, cmp);
        }
        if (lenΔ1 > 0) {
            cmp = Compare(sa.slice(0, lenΔ1 - 1), sb.slice(0, lenΔ1));
            if (cmp != -1) {
                t.Errorf(@"CompareAshorter(%d) = %d", lenΔ1, cmp);
            }
            cmp = Compare(sa.slice(0, lenΔ1), sb.slice(0, lenΔ1 - 1));
            if (cmp != 1) {
                t.Errorf(@"CompareBshorter(%d) = %d", lenΔ1, cmp);
            }
        }
        for (nint k = lastLen; k < lenΔ1; k++) {
            b[k] = (byte)(a[k] - 1);
            cmp = Compare(unsafeString(a.slice(0, lenΔ1)), unsafeString(b.slice(0, lenΔ1)));
            if (cmp != 1) {
                t.Errorf(@"CompareAbigger(%d,%d) = %d", lenΔ1, k, cmp);
            }
            b[k] = (byte)(a[k] + 1);
            cmp = Compare(unsafeString(a.slice(0, lenΔ1)), unsafeString(b.slice(0, lenΔ1)));
            if (cmp != -1) {
                t.Errorf(@"CompareBbigger(%d,%d) = %d", lenΔ1, k, cmp);
            }
            b[k] = a[k];
        }
        lastLen = lenΔ1;
    }
}

} // end strings_test_package
