// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using utf8 = unicode.utf8_package;
using static global::go.runtime_internal_test_package;
using unicode;

partial class runtime_test_package {

// Strings and slices that don't escape and fit into tmpBuf are stack allocated,
// which defeats using AllocsPerRun to test other optimizations.
internal static UntypedInt sizeNoStack => 100;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object s1S2ˢ = (@string)"s1 != s2"u8;

public static void BenchmarkCompareStringEqual(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bytes = slice<byte>("Hello Gophers!"u8);
    @string s1 = (@string)bytes;
    @string s2 = (@string)bytes;
    for (nint i = 0; i < b.N; i++) {
        if (s1 != s2) {
            Ꮡb.Fatal(s1S2ˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloGophersˢ => ref helloGophersˢᵇ;
private static readonly @string helloGophersˢᵇ = "Hello Gophers!"u8;

public static void BenchmarkCompareStringIdentical(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s1 = helloGophersˢ;
    @string s2 = s1;
    for (nint i = 0; i < b.N; i++) {
        if (s1 != s2) {
            Ꮡb.Fatal(s1S2ˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloGophersˢ2 => ref helloGophersˢ2ᵇ;
private static readonly @string helloGophersˢ2ᵇ = "Hello, Gophers"u8;
internal static readonly object s1S2ˢ2 = (@string)"s1 == s2"u8;

public static void BenchmarkCompareStringSameLength(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s1 = helloGophersˢ;
    @string s2 = helloGophersˢ2;
    for (nint i = 0; i < b.N; i++) {
        if (s1 == s2) {
            Ꮡb.Fatal(s1S2ˢ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloGophersˢ3 => ref helloGophersˢ3ᵇ;
private static readonly @string helloGophersˢ3ᵇ = "Hello, Gophers!"u8;

public static void BenchmarkCompareStringDifferentLength(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s1 = helloGophersˢ;
    @string s2 = helloGophersˢ3;
    for (nint i = 0; i < b.N; i++) {
        if (s1 == s2) {
            Ꮡb.Fatal(s1S2ˢ2);
        }
    }
}

public static void BenchmarkCompareStringBigUnaligned(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bytes = new slice<byte>(0, (1 << 20));
    while (len(bytes) < 1 << 20) {
        bytes = append(bytes, ((@string)"Hello Gophers!").ꓸꓸꓸ);
    }
    @string s1 = (@string)bytes;
    @string s2 = "hello"u8 + ((sstring)bytes);
    for (nint i = 0; i < b.N; i++) {
        if (s1 != s2[(int)(len("hello"))..]) {
            Ꮡb.Fatal(s1S2ˢ);
        }
    }
    b.SetBytes((int64)len(s1));
}

public static void BenchmarkCompareStringBig(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bytes = new slice<byte>(0, (1 << 20));
    while (len(bytes) < 1 << 20) {
        bytes = append(bytes, ((@string)"Hello Gophers!").ꓸꓸꓸ);
    }
    @string s1 = (@string)bytes;
    @string s2 = (@string)bytes;
    for (nint i = 0; i < b.N; i++) {
        if (s1 != s2) {
            Ꮡb.Fatal(s1S2ˢ);
        }
    }
    b.SetBytes((int64)len(s1));
}

public static void BenchmarkConcatStringAndBytes(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s1 = slice<byte>("Gophers!"u8);
    sstring s1ᴛ1 = ((sstring)s1);
    for (nint i = 0; i < b.N; i++) {
        _ = "Hello "u8 + s1ᴛ1;
    }
}

internal static @string escapeString;

public static void BenchmarkSliceByteToString(ж<testing.B> b) {
    ref var buf = ref heap<slice<byte>>(slice<byte>([(byte)'!']), out var Ꮡbuf);
    for (nint n = 0; n < 8; n++) {
        b.Run(strconv.Itoa(len(buf)), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                escapeString = (@string)Ꮡbuf.ValueSlot;
            }
        });
        buf = appendꓸꓸꓸ(buf, buf);
    }
}

partial struct stringdataᴛ1 /*dyn*/ {
    internal @string name, data;
}
internal static slice<stringdataᴛ1> stringdata = [
    new("ASCII", "01234567890"),
    new("Japanese", "日本語日本語日本語"),
    new("MixedLength", "$Ѐࠀက퀀𐀀\U00040000\U0010FFFF")
];

internal static nint sinkInt;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lenrunesliceˢ => ref lenrunesliceˢᵇ;
private static readonly @string lenrunesliceˢᵇ = "lenruneslice"u8;
internal static ref readonly @string rangeloopˢ => ref rangeloopˢᵇ;
private static readonly @string rangeloopˢᵇ = "rangeloop"u8;
internal static ref readonly @string utf8RuneCountInStringˢ => ref utf8RuneCountInStringˢᵇ;
private static readonly @string utf8RuneCountInStringˢᵇ = "utf8.RuneCountInString"u8;

public static void BenchmarkRuneCount(ж<testing.B> b) {
    // Each sub-benchmark counts the runes in a string in a different way.
    b.Run(lenrunesliceˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in range(stringdata)) {
            ref var sd = ref heap(new stringdataᴛ1(), out var Ꮡsd);
            sd = vᴛ1;

            var sdʗ1 = sd;
            bΔ1.Run(sd.name, (ж<testing.B> bΔ2) => {
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    sinkInt += len(slice<rune>(sdʗ1.data));
                }
            });
        }
    });
    b.Run(rangeloopˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in range(stringdata)) {
            ref var sd = ref heap(new stringdataᴛ1(), out var Ꮡsd);
            sd = vᴛ2;

            var sdʗ2 = sd;
            bΔ3.Run(sd.name, (ж<testing.B> bΔ4) => {
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    nint n = 0;
                    foreach ((_, _) in sdʗ2.data) {
                        n++;
                    }
                    sinkInt += n;
                }
            });
        }
    });
    b.Run(utf8RuneCountInStringˢ, (ж<testing.B> bΔ5) => {
        foreach (var (_, vᴛ3) in range(stringdata)) {
            ref var sd = ref heap(new stringdataᴛ1(), out var Ꮡsd);
            sd = vᴛ3;

            var sdʗ3 = sd;
            bΔ5.Run(sd.name, (ж<testing.B> bΔ6) => {
                for (nint i = 0; i < (~bΔ6).N; i++) {
                    sinkInt += utf8.RuneCountInString(sdʗ3.data);
                }
            });
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rangeˢ => ref rangeˢᵇ;
private static readonly @string rangeˢᵇ = "range"u8;
internal static ref readonly @string range1ˢ => ref range1ˢᵇ;
private static readonly @string range1ˢᵇ = "range1"u8;
internal static ref readonly @string range2ˢ => ref range2ˢᵇ;
private static readonly @string range2ˢᵇ = "range2"u8;

public static void BenchmarkRuneIterate(ж<testing.B> b) {
    b.Run(rangeˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in range(stringdata)) {
            ref var sd = ref heap(new stringdataᴛ1(), out var Ꮡsd);
            sd = vᴛ1;

            var sdʗ1 = sd;
            bΔ1.Run(sd.name, (ж<testing.B> bΔ2) => {
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    foreach ((_, _) in sdʗ1.data) {
                    }
                }
            });
        }
    });
    b.Run(range1ˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in range(stringdata)) {
            ref var sd = ref heap(new stringdataᴛ1(), out var Ꮡsd);
            sd = vᴛ2;

            var sdʗ2 = sd;
            bΔ3.Run(sd.name, (ж<testing.B> bΔ4) => {
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    foreach ((_, _) in sdʗ2.data) {
                    }
                }
            });
        }
    });
    b.Run(range2ˢ, (ж<testing.B> bΔ5) => {
        foreach (var (_, vᴛ3) in range(stringdata)) {
            ref var sd = ref heap(new stringdataᴛ1(), out var Ꮡsd);
            sd = vᴛ3;

            var sdʗ3 = sd;
            bΔ5.Run(sd.name, (ж<testing.B> bΔ6) => {
                for (nint i = 0; i < (~bΔ6).N; i++) {
                    foreach ((_, _) in sdʗ3.data) {
                    }
                }
            });
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notEqualˢ = (@string)"not equal"u8;

public static void BenchmarkArrayEqual(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var a1 = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}.array();
    var a2 = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}.array();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        if (a1 != a2) {
            Ꮡb.Fatal(notEqualˢ);
        }
    }
}

public static void TestStringW(ж<testing.T> t) {
    var strings = slice<@string>([
        "hello",
        "a\u5566\u7788b"
    ]);

    foreach (var (_, s) in range(strings)) {
        slice<uint16> b = default;
        foreach (var (_, c) in s) {
            b = append(b, (uint16)c);
            if (c != (rune)(uint16)c) {
                t.Errorf("bad test: stringW can't handle >16 bit runes");
            }
        }
        b = append(b, (uint16)0);
        @string r = runtime_internal_test_package.GostringW(b);
        if (r != s) {
            t.Errorf("gostringW(%v) = %s, want %s", b, r, s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringconcatˢ => ref stringconcatˢᵇ;
private static readonly @string stringconcatˢᵇ = "stringconcat"u8;

public static void TestLargeStringConcat(ж<testing.T> t) {
    @string output = runTestProg(t, testprogˢ, stringconcatˢ);
    @string want = "panic: "u8 + strings.Repeat("0", 1 << 10) + strings.Repeat("1", 1 << 10) + strings.Repeat("2", 1 << 10) + strings.Repeat("3", 1 << 10);
    if (!strings.HasPrefix(output, want)) {
        t.Fatalf("output does not start with %q:\n%s", want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bytesˢ2 => ref bytesˢ2ᵇ;
private static readonly @string bytesˢ2ᵇ = "bytes"u8;
internal static readonly object prefixBytesSuffixˢ = (@string)"prefix bytes suffix"u8;

public static void TestConcatTempString(ж<testing.T> t) {
    @string s = bytesˢ2;
    var b = slice<byte>(s);
    var bʗ1 = b;
    var n = testing.AllocsPerRun(1000, () => {
        if ("prefix "u8 + ((sstring)bʗ1) + " suffix"u8 != "prefix bytes suffix"u8) {
            t.Fatalf("strings are not equal: '%v' and '%v'", "prefix " + ((sstring)bʗ1) + " suffix", prefixBytesSuffixˢ);
        }
    });
    if (n != 0D) {
        t.Fatalf("want 0 allocs, got %v", n);
    }
}

public static void TestCompareTempString(ж<testing.T> t) {
    @string s = strings.Repeat("x", sizeNoStack);
    var b = slice<byte>(s);
    var bʗ1 = b;
    var n = testing.AllocsPerRun(1000, () => {
        if (((sstring)bʗ1) != s) {
            t.Fatalf("strings are not equal: '%v' and '%v'", (@string)bʗ1, s);
        }
        if (((sstring)bʗ1) < s) {
            t.Fatalf("strings are not equal: '%v' and '%v'", (@string)bʗ1, s);
        }
        if (((sstring)bʗ1) > s) {
            t.Fatalf("strings are not equal: '%v' and '%v'", (@string)bʗ1, s);
        }
        if (((sstring)bʗ1) == s) {
        } else {
            t.Fatalf("strings are not equal: '%v' and '%v'", (@string)bʗ1, s);
        }
        if (((sstring)bʗ1) <= s) {
        } else {
            t.Fatalf("strings are not equal: '%v' and '%v'", (@string)bʗ1, s);
        }
        if (((sstring)bʗ1) >= s) {
        } else {
            t.Fatalf("strings are not equal: '%v' and '%v'", (@string)bʗ1, s);
        }
    });
    if (n != 0D) {
        t.Fatalf("want 0 allocs, got %v", n);
    }
}

public static void TestStringIndexHaystack(ж<testing.T> t) {
    // See issue 25864.
    var haystack = slice<byte>("hello"u8);
    @string needle = "ll";
    var haystackʗ1 = haystack;
    var n = testing.AllocsPerRun(1000, () => {
        if (strings.Index((@string)haystackʗ1, needle) != 2) {
            t.Fatalf("needle not found");
        }
    });
    if (n != 0D) {
        t.Fatalf("want 0 allocs, got %v", n);
    }
}

public static void TestStringIndexNeedle(ж<testing.T> t) {
    // See issue 25864.
    @string haystack = helloˢ;
    var needle = slice<byte>("ll"u8);
    var needleʗ1 = needle;
    var n = testing.AllocsPerRun(1000, () => {
        if (strings.Index(haystack, (@string)needleʗ1) != 2) {
            t.Fatalf("needle not found");
        }
    });
    if (n != 0D) {
        t.Fatalf("want 0 allocs, got %v", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aaabcbabccbaabcbabcccˢ => ref aaabcbabccbaabcbabcccˢᵇ;
private static readonly @string aaabcbabccbaabcbabcccˢᵇ = "aaabcbabccbaabcbabccc"u8;

public static void TestStringOnStack(ж<testing.T> t) {
    @string s = ""u8;
    for (nint i = 0; i < 3; i++) {
        s = "a"u8 + s + "b"u8 + s + "c"u8;
    }

    @string want = aaabcbabccbaabcbabcccˢ; if (s != want) {
        t.Fatalf("want: '%v', got '%v'", want, s);
    }
}

public static void TestIntString(ж<testing.T> t) {
    // Non-escaping result of intstring.
    @string s = ""u8;
    for (var i = (rune)0; i < 4; i++) {
        s += ((@string)(i + '0')) + ((@string)(i + '0' + 1));
    }
    {
        @string want = "01122334"; if (s != want) {
            t.Fatalf("want '%v', got '%v'", want, s);
        }
    }

    // Escaping result of intstring.
    array<@string> a = new(4);
    for (var i = (rune)0; i < 4; i++) {
        a[i] = (@string)(i + '0');
    }
    s = a[0] + a[1] + a[2] + a[3];
    {
        @string want = "0123"; if (s != want) {
            t.Fatalf("want '%v', got '%v'", want, s);
        }
    }
}

public static void TestIntStringAllocs(ж<testing.T> t) {
    var unknown = (rune)'0';
    var n = testing.AllocsPerRun(1000, () => {
        @string s1 = (@string)unknown;
        @string s2 = (@string)(unknown + 1);
        if (s1 == s2) {
            t.Fatalf("bad");
        }
    });
    if (n != 0D) {
        t.Fatalf("want 0 allocs, got %v", n);
    }
}

public static void TestRangeStringCast(ж<testing.T> t) {
    @string s = strings.Repeat("x", sizeNoStack);
    var n = testing.AllocsPerRun(1000, () => {
        foreach (var (i, c) in range(slice<byte>(s))) {
            if (c != s[i]) {
                t.Fatalf("want '%c' at pos %v, got '%c'", s[i], i, c);
            }
        }
    });
    if (n != 0D) {
        t.Fatalf("want 0 allocs, got %v", n);
    }
}

internal static bool isZeroed(slice<byte> b) {
    foreach (var (_, x) in range(b)) {
        if (x != 0) {
            return false;
        }
    }
    return true;
}

internal static bool isZeroedR(slice<rune> r) {
    foreach (var (_, x) in range(r)) {
        if (x != 0) {
            return false;
        }
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ3 => ref fooˢ3ᵇ;
private static readonly @string fooˢ3ᵇ = "foož"u8;

public static void TestString2Slice(ж<testing.T> t) {
    // Make sure we don't return slices that expose
    // an unzeroed section of stack-allocated temp buf
    // between len and cap. See issue 14232.
    @string s = fooˢ3;
    var b = (slice<byte>)(s);
    if (!isZeroed(b.slice(len(b), cap(b)))) {
        t.Errorf("extra bytes not zeroed");
    }
    var r = (slice<rune>)(s);
    if (!isZeroedR(r.slice(len(r), cap(r)))) {
        t.Errorf("extra runes not zeroed");
    }
}

internal partial struct TestParseByteCount_type /*dyn*/ {
    internal @string @in;
    internal int64 @out;
    internal bool ok;
}

public static void TestParseByteCount(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestParseByteCount_type>([
        // Good numeric inputs.
        new("1", 1, true),
        new("12345", 12345, true),
        new("012345", 12345, true),
        new("98765432100", 98765432100L, true),
        new("9223372036854775807", 9223372036854775807L, true),

        // Good trivial suffix inputs.
        new("1B", 1, true),
        new("12345B", 12345, true),
        new("012345B", 12345, true),
        new("98765432100B", 98765432100L, true),
        new("9223372036854775807B", 9223372036854775807L, true),

        // Good binary suffix inputs.
        new("1KiB", (int64)1 << 10, true),
        new("05KiB", (int64)5 << 10, true),
        new("1MiB", (int64)1 << 20, true),
        new("10MiB", (int64)10 << 20, true),
        new("1GiB", (int64)1 << 30, true),
        new("100GiB", 107374182400L, true),
        new("1TiB", 1099511627776L, true),
        new("99TiB", 108851651149824L, true),

        // Good zero inputs.
        //
        // -0 is an edge case, but no harm in supporting it.
        new("-0", 0, true),
        new("0", 0, true),
        new("0B", 0, true),
        new("0KiB", 0, true),
        new("0MiB", 0, true),
        new("0GiB", 0, true),
        new("0TiB", 0, true),

        // Bad inputs.
        new("", 0, false),
        new("-1", 0, false),
        new("a12345", 0, false),
        new("a12345B", 0, false),
        new("12345x", 0, false),
        new("0x12345", 0, false),

        // Bad numeric inputs.
        new("9223372036854775808", 0, false),
        new("9223372036854775809", 0, false),
        new("18446744073709551615", 0, false),
        new("20496382327982653440", 0, false),
        new("18446744073709551616", 0, false),
        new("18446744073709551617", 0, false),
        new("9999999999999999999999", 0, false),

        // Bad trivial suffix inputs.
        new("9223372036854775808B", 0, false),
        new("9223372036854775809B", 0, false),
        new("18446744073709551615B", 0, false),
        new("20496382327982653440B", 0, false),
        new("18446744073709551616B", 0, false),
        new("18446744073709551617B", 0, false),
        new("9999999999999999999999B", 0, false),

        // Bad binary suffix inputs.
        new("1Ki", 0, false),
        new("05Ki", 0, false),
        new("10Mi", 0, false),
        new("100Gi", 0, false),
        new("99Ti", 0, false),
        new("22iB", 0, false),
        new("B", 0, false),
        new("iB", 0, false),
        new("KiB", 0, false),
        new("MiB", 0, false),
        new("GiB", 0, false),
        new("TiB", 0, false),
        new("-120KiB", 0, false),
        new("-891MiB", 0, false),
        new("-704GiB", 0, false),
        new("-42TiB", 0, false),
        new("99999999999999999999KiB", 0, false),
        new("99999999999999999MiB", 0, false),
        new("99999999999999GiB", 0, false),
        new("99999999999TiB", 0, false),
        new("555EiB", 0, false),

        // Mistaken SI suffix inputs.
        new("0KB", 0, false),
        new("0MB", 0, false),
        new("0GB", 0, false),
        new("0TB", 0, false),
        new("1KB", 0, false),
        new("05KB", 0, false),
        new("1MB", 0, false),
        new("10MB", 0, false),
        new("1GB", 0, false),
        new("100GB", 0, false),
        new("1TB", 0, false),
        new("99TB", 0, false),
        new("1K", 0, false),
        new("05K", 0, false),
        new("10M", 0, false),
        new("100G", 0, false),
        new("99T", 0, false),
        new("99999999999999999999KB", 0, false),
        new("99999999999999999MB", 0, false),
        new("99999999999999GB", 0, false),
        new("99999999999TB", 0, false),
        new("99999999999TiB", 0, false),
        new("555EB", 0, false)
    ]))) {
        var (@out, ok) = runtime_internal_test_package.ParseByteCount(test.@in);
        if (test.@out != @out || test.ok != ok) {
            t.Errorf("parseByteCount(%q) = (%v, %v) want (%v, %v)",
                test.@in, @out, ok, test.@out, test.ok);
        }
    }
}

} // end runtime_test_package
