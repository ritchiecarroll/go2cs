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
    @string s1 = ((@string)bytes);
    @string s2 = ((@string)bytes);
    for (nint i = 0; i < b.N; i++) {
        if (s1 != s2) {
            Ꮡb.Fatal(s1S2ˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloGophersˢ = "Hello Gophers!"u8;

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
internal static readonly @string helloGophersˢ2 = "Hello, Gophers"u8;
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
internal static readonly @string helloGophersˢ3 = "Hello, Gophers!"u8;

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

    var bytes = new slice<byte>(0, (1 << (int)(20)));
    while (len(bytes) < (1 << (int)(20))) {
        bytes = append(bytes, ((@string)"Hello Gophers!"u8).ꓸꓸꓸ);
    }
    @string s1 = ((@string)bytes);
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

    var bytes = new slice<byte>(0, (1 << (int)(20)));
    while (len(bytes) < (1 << (int)(20))) {
        bytes = append(bytes, ((@string)"Hello Gophers!"u8).ꓸꓸꓸ);
    }
    @string s1 = ((@string)bytes);
    @string s2 = ((@string)bytes);
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

public static void BenchmarkSliceByteToString(ж<testing.B> Ꮡb) {
    ref var buf = ref heap<slice<byte>>(out var Ꮡbuf);
    buf = new byte[]{(rune)'!'}.slice();
    for (nint n = 0; n < 8; n++) {
        Ꮡb.Run(strconv.Itoa(len(buf)), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                escapeString = ((@string)Ꮡbuf.ValueSlot);
            }
        });
        buf = appendꓸꓸꓸ(buf, buf);
    }
}


[GoType("dyn")] partial struct stringdataᴛ1 {
    internal @string name, data;
}
internal static slice<stringdataᴛ1> stringdata = new stringdataᴛ1[]{
    new("ASCII"u8, "01234567890"u8),
    new("Japanese"u8, "日本語日本語日本語"u8),
    new("MixedLength"u8, "$Ѐࠀက퀀𐀀\U00040000\U0010FFFF"u8)
}.slice();

internal static nint sinkInt;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lenrunesliceˢ = "lenruneslice"u8;
internal static readonly @string rangeloopˢ = "rangeloop"u8;
internal static readonly @string utf8RuneCountInStringˢ = "utf8.RuneCountInString"u8;

public static void BenchmarkRuneCount(ж<testing.B> Ꮡb) {
    // Each sub-benchmark counts the runes in a string in a different way.
    Ꮡb.Run(lenrunesliceˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in stringdata) {
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
    Ꮡb.Run(rangeloopˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in stringdata) {
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
    Ꮡb.Run(utf8RuneCountInStringˢ, (ж<testing.B> bΔ5) => {
        foreach (var (_, vᴛ3) in stringdata) {
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
internal static readonly @string rangeˢ = "range"u8;
internal static readonly @string range1ˢ = "range1"u8;
internal static readonly @string range2ˢ = "range2"u8;

public static void BenchmarkRuneIterate(ж<testing.B> Ꮡb) {
    Ꮡb.Run(rangeˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in stringdata) {
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
    Ꮡb.Run(range1ˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in stringdata) {
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
    Ꮡb.Run(range2ˢ, (ж<testing.B> bΔ5) => {
        foreach (var (_, vᴛ3) in stringdata) {
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

public static void TestStringW(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var strings = new @string[]{
        "hello"u8,
        "a\u5566\u7788b"u8
    }.slice();
    foreach (var (_, s) in strings) {
        slice<uint16> b = default!;
        foreach (var (_, c) in s) {
            b = append(b, (uint16)c);
            if (c != (rune)(uint16)c) {
                Ꮡt.Errorf("bad test: stringW can't handle >16 bit runes"u8);
            }
        }
        b = append(b, (uint16)(0));
        @string r = runtime_internal_test_package.GostringW(b);
        if (r != s) {
            Ꮡt.Errorf("gostringW(%v) = %s, want %s"u8, b, r, s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringconcatˢ = "stringconcat"u8;

public static void TestLargeStringConcat(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, stringconcatˢ);
    @string want = "panic: "u8 + strings.Repeat("0"u8, (1 << (int)(10))) + strings.Repeat("1"u8, (1 << (int)(10))) + strings.Repeat("2"u8, (1 << (int)(10))) + strings.Repeat("3"u8, (1 << (int)(10)));
    if (!strings.HasPrefix(output, want)) {
        Ꮡt.Fatalf("output does not start with %q:\n%s"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bytesˢ2 = "bytes"u8;
internal static readonly object prefixBytesSuffixˢ = (@string)"prefix bytes suffix"u8;

public static void TestConcatTempString(ж<testing.T> Ꮡt) {
    @string s = bytesˢ2;
    var b = slice<byte>(s);
    var bʗ1 = b;
    var n = testing.AllocsPerRun(1000, () => {
        if ("prefix "u8 + ((sstring)bʗ1) + " suffix"u8 != "prefix bytes suffix"u8) {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, "prefix " + ((sstring)bʗ1) + " suffix", prefixBytesSuffixˢ);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

public static void TestCompareTempString(ж<testing.T> Ꮡt) {
    @string s = strings.Repeat("x"u8, sizeNoStack);
    var b = slice<byte>(s);
    var bʗ1 = b;
    var n = testing.AllocsPerRun(1000, () => {
        if (((sstring)bʗ1) != s) {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, ((@string)bʗ1), s);
        }
        if (((sstring)bʗ1) < s) {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, ((@string)bʗ1), s);
        }
        if (((sstring)bʗ1) > s) {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, ((@string)bʗ1), s);
        }
        if (((sstring)bʗ1) == s){
        } else {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, ((@string)bʗ1), s);
        }
        if (((sstring)bʗ1) <= s){
        } else {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, ((@string)bʗ1), s);
        }
        if (((sstring)bʗ1) >= s){
        } else {
            Ꮡt.Fatalf("strings are not equal: '%v' and '%v'"u8, ((@string)bʗ1), s);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

public static void TestStringIndexHaystack(ж<testing.T> Ꮡt) {
    // See issue 25864.
    var haystack = slice<byte>("hello"u8);
    @string needle = "ll"u8;
    var haystackʗ1 = haystack;
    var n = testing.AllocsPerRun(1000, () => {
        if (strings.Index(((@string)haystackʗ1), needle) != 2) {
            Ꮡt.Fatalf("needle not found"u8);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

public static void TestStringIndexNeedle(ж<testing.T> Ꮡt) {
    // See issue 25864.
    @string haystack = helloˢ;
    var needle = slice<byte>("ll"u8);
    var needleʗ1 = needle;
    var n = testing.AllocsPerRun(1000, () => {
        if (strings.Index(haystack, ((@string)needleʗ1)) != 2) {
            Ꮡt.Fatalf("needle not found"u8);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aaabcbabccbaabcbabcccˢ = "aaabcbabccbaabcbabccc"u8;

public static void TestStringOnStack(ж<testing.T> Ꮡt) {
    @string s = ""u8;
    for (nint i = 0; i < 3; i++) {
        s = "a"u8 + s + "b"u8 + s + "c"u8;
    }
    {
        @string want = aaabcbabccbaabcbabcccˢ; if (s != want) {
            Ꮡt.Fatalf("want: '%v', got '%v'"u8, want, s);
        }
    }
}

public static void TestIntString(ж<testing.T> Ꮡt) {
    // Non-escaping result of intstring.
    @string s = ""u8;
    for (var i = (rune)0; i < 4; i++) {
        s += ((@string)(i + (rune)'0')) + ((@string)(i + (rune)'0' + 1));
    }
    {
        @string want = "01122334"u8; if (s != want) {
            Ꮡt.Fatalf("want '%v', got '%v'"u8, want, s);
        }
    }
    // Escaping result of intstring.
    array<@string> a = new(4);
    for (var i = (rune)0; i < 4; i++) {
        a[i] = ((@string)(i + (rune)'0'));
    }
    s = a[0] + a[1] + a[2] + a[3];
    {
        @string want = "0123"u8; if (s != want) {
            Ꮡt.Fatalf("want '%v', got '%v'"u8, want, s);
        }
    }
}

public static void TestIntStringAllocs(ж<testing.T> Ꮡt) {
    var unknown = (rune)'0';
    var n = testing.AllocsPerRun(1000, () => {
        @string s1 = ((@string)unknown);
        @string s2 = ((@string)(unknown + 1));
        if (s1 == s2) {
            Ꮡt.Fatalf("bad"u8);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

public static void TestRangeStringCast(ж<testing.T> Ꮡt) {
    @string s = strings.Repeat("x"u8, sizeNoStack);
    var n = testing.AllocsPerRun(1000, () => {
        foreach (var (i, c) in slice<byte>(s)) {
            if (c != s[i]) {
                Ꮡt.Fatalf("want '%c' at pos %v, got '%c'"u8, s[i], i, c);
            }
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

internal static bool isZeroed(slice<byte> b) {
    foreach (var (_, x) in b) {
        if (x != 0) {
            return false;
        }
    }
    return true;
}

internal static bool isZeroedR(slice<rune> r) {
    foreach (var (_, x) in r) {
        if (x != 0) {
            return false;
        }
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ3 = "foož"u8;

public static void TestString2Slice(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Make sure we don't return slices that expose
    // an unzeroed section of stack-allocated temp buf
    // between len and cap. See issue 14232.
    @string s = fooˢ3;
    var b = (slice<byte>)(s);
    if (!isZeroed(b.slice(len(b), cap(b)))) {
        Ꮡt.Errorf("extra bytes not zeroed"u8);
    }
    var r = (slice<rune>)(s);
    if (!isZeroedR(r.slice(len(r), cap(r)))) {
        Ꮡt.Errorf("extra runes not zeroed"u8);
    }
}

internal static UntypedInt intSize => /* 32 << (^uint(0) >> 63) */ 64;

[GoType] partial struct atoi64Test {
    internal @string @in;
    internal int64 @out;
    internal bool ok;
}

internal static ж<slice<atoi64Test>> Ꮡatoi64tests = new StandardBox<slice<atoi64Test>>(new atoi64Test[]{
    new(""u8, 0, false),
    new("0"u8, 0, true),
    new("-0"u8, 0, true),
    new("1"u8, 1, true),
    new("-1"u8, -1, true),
    new("12345"u8, 12345, true),
    new("-12345"u8, -12345, true),
    new("012345"u8, 12345, true),
    new("-012345"u8, -12345, true),
    new("12345x"u8, 0, false),
    new("-12345x"u8, 0, false),
    new("98765432100"u8, 98765432100L, true),
    new("-98765432100"u8, -98765432100L, true),
    new("20496382327982653440"u8, 0, false),
    new("-20496382327982653440"u8, 0, false),
    new("9223372036854775807"u8, 9223372036854775807L, true),
    new("-9223372036854775807"u8, -(9223372036854775807L), true),
    new("9223372036854775808"u8, 0, false),
    new("-9223372036854775808"u8, -9223372036854775808L, true),
    new("9223372036854775809"u8, 0, false),
    new("-9223372036854775809"u8, 0, false)
}.slice());
internal static ref slice<atoi64Test> atoi64tests => ref Ꮡatoi64tests.ValueSlot;

public static void TestAtoi(ж<testing.T> Ꮡt) {
    var exprᴛ1 = intSize;
    if (exprᴛ1 == 32) {
        foreach (var (i, _) in atoi32tests) {
            var test = Ꮡ(atoi32tests, i);
            var (@out, ok) = runtime_internal_test_package.Atoi((~test).@in);
            if ((~test).@out != (int32)@out || (~test).ok != ok) {
                Ꮡt.Errorf("atoi(%q) = (%v, %v) want (%v, %v)"u8,
                    (~test).@in, @out, ok, (~test).@out, (~test).ok);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var (i, _) in atoi64tests) {
            var test = Ꮡ(atoi64tests, i);
            var (@out, ok) = runtime_internal_test_package.Atoi((~test).@in);
            if ((~test).@out != (int64)@out || (~test).ok != ok) {
                Ꮡt.Errorf("atoi(%q) = (%v, %v) want (%v, %v)"u8,
                    (~test).@in, @out, ok, (~test).@out, (~test).ok);
            }
        }
    }

}

[GoType] partial struct atoi32Test {
    internal @string @in;
    internal int32 @out;
    internal bool ok;
}

internal static ж<slice<atoi32Test>> Ꮡatoi32tests = new StandardBox<slice<atoi32Test>>(new atoi32Test[]{
    new(""u8, 0, false),
    new("0"u8, 0, true),
    new("-0"u8, 0, true),
    new("1"u8, 1, true),
    new("-1"u8, -1, true),
    new("12345"u8, 12345, true),
    new("-12345"u8, -12345, true),
    new("012345"u8, 12345, true),
    new("-012345"u8, -12345, true),
    new("12345x"u8, 0, false),
    new("-12345x"u8, 0, false),
    new("987654321"u8, 987654321, true),
    new("-987654321"u8, -987654321, true),
    new("2147483647"u8, (int32)(2147483648L - 1), true),
    new("-2147483647"u8, (int32)(-(2147483648L - 1)), true),
    new("2147483648"u8, 0, false),
    new("-2147483648"u8, (int32)(-1 << (int)(31)), true),
    new("2147483649"u8, 0, false),
    new("-2147483649"u8, 0, false)
}.slice());
internal static ref slice<atoi32Test> atoi32tests => ref Ꮡatoi32tests.ValueSlot;

public static void TestAtoi32(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in atoi32tests) {
        var test = Ꮡ(atoi32tests, i);
        var (@out, ok) = runtime_internal_test_package.Atoi32((~test).@in);
        if ((~test).@out != @out || (~test).ok != ok) {
            Ꮡt.Errorf("atoi32(%q) = (%v, %v) want (%v, %v)"u8,
                (~test).@in, @out, ok, (~test).@out, (~test).ok);
        }
    }
}

[GoType("dyn")] internal partial struct TestParseByteCount_type {
    internal @string @in;
    internal int64 @out;
    internal bool ok;
}

public static void TestParseByteCount(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestParseByteCount_type[]{ // Good numeric inputs.

        new("1"u8, 1, true),
        new("12345"u8, 12345, true),
        new("012345"u8, 12345, true),
        new("98765432100"u8, 98765432100L, true),
        new("9223372036854775807"u8, 9223372036854775807L, true), // Good trivial suffix inputs.

        new("1B"u8, 1, true),
        new("12345B"u8, 12345, true),
        new("012345B"u8, 12345, true),
        new("98765432100B"u8, 98765432100L, true),
        new("9223372036854775807B"u8, 9223372036854775807L, true), // Good binary suffix inputs.

        new("1KiB"u8, ((int64)1 << (int)(10)), true),
        new("05KiB"u8, ((int64)5 << (int)(10)), true),
        new("1MiB"u8, ((int64)1 << (int)(20)), true),
        new("10MiB"u8, ((int64)10 << (int)(20)), true),
        new("1GiB"u8, ((int64)1 << (int)(30)), true),
        new("100GiB"u8, 107374182400L, true),
        new("1TiB"u8, 1099511627776L, true),
        new("99TiB"u8, 108851651149824L, true), // Good zero inputs.
 //
 // -0 is an edge case, but no harm in supporting it.

        new("-0"u8, 0, true),
        new("0"u8, 0, true),
        new("0B"u8, 0, true),
        new("0KiB"u8, 0, true),
        new("0MiB"u8, 0, true),
        new("0GiB"u8, 0, true),
        new("0TiB"u8, 0, true), // Bad inputs.

        new(""u8, 0, false),
        new("-1"u8, 0, false),
        new("a12345"u8, 0, false),
        new("a12345B"u8, 0, false),
        new("12345x"u8, 0, false),
        new("0x12345"u8, 0, false), // Bad numeric inputs.

        new("9223372036854775808"u8, 0, false),
        new("9223372036854775809"u8, 0, false),
        new("18446744073709551615"u8, 0, false),
        new("20496382327982653440"u8, 0, false),
        new("18446744073709551616"u8, 0, false),
        new("18446744073709551617"u8, 0, false),
        new("9999999999999999999999"u8, 0, false), // Bad trivial suffix inputs.

        new("9223372036854775808B"u8, 0, false),
        new("9223372036854775809B"u8, 0, false),
        new("18446744073709551615B"u8, 0, false),
        new("20496382327982653440B"u8, 0, false),
        new("18446744073709551616B"u8, 0, false),
        new("18446744073709551617B"u8, 0, false),
        new("9999999999999999999999B"u8, 0, false), // Bad binary suffix inputs.

        new("1Ki"u8, 0, false),
        new("05Ki"u8, 0, false),
        new("10Mi"u8, 0, false),
        new("100Gi"u8, 0, false),
        new("99Ti"u8, 0, false),
        new("22iB"u8, 0, false),
        new("B"u8, 0, false),
        new("iB"u8, 0, false),
        new("KiB"u8, 0, false),
        new("MiB"u8, 0, false),
        new("GiB"u8, 0, false),
        new("TiB"u8, 0, false),
        new("-120KiB"u8, 0, false),
        new("-891MiB"u8, 0, false),
        new("-704GiB"u8, 0, false),
        new("-42TiB"u8, 0, false),
        new("99999999999999999999KiB"u8, 0, false),
        new("99999999999999999MiB"u8, 0, false),
        new("99999999999999GiB"u8, 0, false),
        new("99999999999TiB"u8, 0, false),
        new("555EiB"u8, 0, false), // Mistaken SI suffix inputs.

        new("0KB"u8, 0, false),
        new("0MB"u8, 0, false),
        new("0GB"u8, 0, false),
        new("0TB"u8, 0, false),
        new("1KB"u8, 0, false),
        new("05KB"u8, 0, false),
        new("1MB"u8, 0, false),
        new("10MB"u8, 0, false),
        new("1GB"u8, 0, false),
        new("100GB"u8, 0, false),
        new("1TB"u8, 0, false),
        new("99TB"u8, 0, false),
        new("1K"u8, 0, false),
        new("05K"u8, 0, false),
        new("10M"u8, 0, false),
        new("100G"u8, 0, false),
        new("99T"u8, 0, false),
        new("99999999999999999999KB"u8, 0, false),
        new("99999999999999999MB"u8, 0, false),
        new("99999999999999GB"u8, 0, false),
        new("99999999999TB"u8, 0, false),
        new("99999999999TiB"u8, 0, false),
        new("555EB"u8, 0, false)
    }.slice()) {
        var (@out, ok) = runtime_internal_test_package.ParseByteCount(test.@in);
        if (test.@out != @out || test.ok != ok) {
            Ꮡt.Errorf("parseByteCount(%q) = (%v, %v) want (%v, %v)"u8,
                test.@in, @out, ok, test.@out, test.ok);
        }
    }
}

} // end runtime_test_package
