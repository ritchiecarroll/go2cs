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

// test runtime·memeq's chunked implementation

partial struct compareTestsᴛ1 /*dyn*/ {
    internal @string a, b;
    internal nint i;
}
internal static slice<compareTestsᴛ1> compareTests = new compareTestsᴛ1[]{
    new(""u8, ""u8, 0),
    new("a"u8, ""u8, 1),
    new(""u8, "a"u8, -1),
    new("abc"u8, "abc"u8, 0),
    new("ab"u8, "abc"u8, -1),
    new("abc"u8, "ab"u8, 1),
    new("x"u8, "ab"u8, 1),
    new("ab"u8, "x"u8, -1),
    new("x"u8, "a"u8, 1),
    new("b"u8, "x"u8, -1),
    new("abcdefgh"u8, "abcdefgh"u8, 0),
    new("abcdefghi"u8, "abcdefghi"u8, 0),
    new("abcdefghi"u8, "abcdefghj"u8, -1)
}.slice();

public static void TestCompare(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in compareTests) {
        nint cmp = Compare(tt.a, tt.b);
        if (cmp != tt.i) {
            Ꮡt.Errorf(@"Compare(%q, %q) = %v"u8, tt.a, tt.b, cmp);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloGophersˢ = "Hello Gophers!"u8;
internal static readonly object sS1Failedˢ = (@string)"s > s[:1] failed"u8;

public static void TestCompareIdenticalString(ж<testing.T> Ꮡt) {
    @string s = helloGophersˢ;
    if (Compare(s, s) != 0) {
        Ꮡt.Error((@string)"s != s"u8);
    }
    if (Compare(s, s[..1]) != 1) {
        Ꮡt.Error(sS1Failedˢ);
    }
}

public static void TestCompareStrings(ж<testing.T> Ꮡt) {
    // unsafeString converts a []byte to a string with no allocation.
    // The caller must not modify b while the result string is in use.
    @string unsafeString(slice<byte> bΔ1) => @unsafe.String(@unsafe.SliceData(bΔ1), len(bΔ1));
    var lengths = new slice<nint>(0); // lengths to test in ascending order
    for (nint i = 0; i <= 128; i++) {
        lengths = append(lengths, i);
    }
    lengths = append(lengths, (nint)(256), (nint)(512), (nint)(1024), (nint)(1333), (nint)(4095), (nint)(4096), (nint)(4097));
    if (!testing.Short() || testenv.Builder() != ""u8) {
        lengths = append(lengths, (nint)(65535), (nint)(65536), (nint)(65537), (nint)(99999));
    }
    nint n = lengths[len(lengths) - 1];
    var a = new slice<byte>(n + 1);
    var b = new slice<byte>(n + 1);
    nint lastLen = 0;
    foreach (var (_, lenΔ1) in lengths) {
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
            Ꮡt.Errorf(@"CompareIdentical(%d) = %d"u8, lenΔ1, cmp);
        }
        if (lenΔ1 > 0) {
            cmp = Compare(sa.slice(0, lenΔ1 - 1), sb.slice(0, lenΔ1));
            if (cmp != -1) {
                Ꮡt.Errorf(@"CompareAshorter(%d) = %d"u8, lenΔ1, cmp);
            }
            cmp = Compare(sa.slice(0, lenΔ1), sb.slice(0, lenΔ1 - 1));
            if (cmp != 1) {
                Ꮡt.Errorf(@"CompareBshorter(%d) = %d"u8, lenΔ1, cmp);
            }
        }
        for (nint k = lastLen; k < lenΔ1; k++) {
            b[k] = (byte)(a[k] - 1);
            cmp = Compare(unsafeString(a.slice(0, lenΔ1)), unsafeString(b.slice(0, lenΔ1)));
            if (cmp != 1) {
                Ꮡt.Errorf(@"CompareAbigger(%d,%d) = %d"u8, lenΔ1, k, cmp);
            }
            b[k] = (byte)(a[k] + 1);
            cmp = Compare(unsafeString(a.slice(0, lenΔ1)), unsafeString(b.slice(0, lenΔ1)));
            if (cmp != -1) {
                Ꮡt.Errorf(@"CompareBbigger(%d,%d) = %d"u8, lenΔ1, k, cmp);
            }
            b[k] = a[k];
        }
        lastLen = lenΔ1;
    }
}

} // end strings_test_package
