// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.unicode;

using testenv = @internal.testenv_package;
using slices = slices_package;
using testing = testing_package;
using unicode = unicode_package;
using static go.unicode.utf16_package;
using @internal;
using static go.unicode.utf16_internal_test_package;

partial class utf16_test_package {

// Validate the constants redefined from unicode.
public static void TestConstants(ж<testing.T> t) {
    if (utf16_internal_test_package.MaxRune != unicode.MaxRune) {
        t.Errorf("utf16.maxRune is wrong: %x should be %x", (int32)(utf16_internal_test_package.MaxRune), (int32)(unicode.MaxRune));
    }
    if (utf16_internal_test_package.ReplacementChar != unicode.ReplacementChar) {
        t.Errorf("utf16.replacementChar is wrong: %x should be %x", (int32)(utf16_internal_test_package.ReplacementChar), (int32)(unicode.ReplacementChar));
    }
}

internal partial struct TestRuneLen_type /*dyn*/ {
    internal rune r;
    internal nint length;
}

public static void TestRuneLen(ж<testing.T> t) {
    foreach (var (_, tt) in slice<TestRuneLen_type>([
        new(0, 1),
        new(utf16_internal_test_package.Surr1 - 1, 1),
        new(utf16_internal_test_package.Surr3, 1),
        new(utf16_internal_test_package.SurrSelf - 1, 1),
        new(utf16_internal_test_package.SurrSelf, 2),
        new(utf16_internal_test_package.MaxRune, 2),
        new(utf16_internal_test_package.MaxRune + 1, -1),
        new(-1, -1)
    ])) {
        {
            nint length = RuneLen(tt.r); if (length != tt.length) {
                t.Errorf("RuneLen(%#U) = %d, want %d", tt.r, length, tt.length);
            }
        }
    }
}

partial struct encodeTest {
    internal slice<rune> @in;
    internal slice<uint16> @out;
}

internal static slice<encodeTest> encodeTests = slice<encodeTest>([
    new(slice<rune>([1, 2, 3, 4]), slice<uint16>([1, 2, 3, 4])),
    new(slice<rune>([0xffff, 0x10000, 0x10001, 0x12345, 0x10ffff]),
        slice<uint16>([0xffff, 0xd800, 0xdc00, 0xd800, 0xdc01, 0xd808, 0xdf45, 0xdbff, 0xdfff])),
    new(slice<rune>(['a', 'b', 0xd7ff, 0xd800, 0xdfff, 0xe000, 0x110000, -1]),
        slice<uint16>(['a', 'b', 0xd7ff, 0xfffd, 0xfffd, 0xe000, 0xfffd, 0xfffd]))
]);

public static void TestEncode(ж<testing.T> t) {
    foreach (var (_, tt) in encodeTests) {
        var @out = Encode(tt.@in);
        if (!slices.Equal<slice<uint16>, uint16>(@out, tt.@out)) {
            t.Errorf("Encode(%x) = %x; want %x", tt.@in, @out, tt.@out);
        }
    }
}

public static void TestAppendRune(ж<testing.T> t) {
    foreach (var (_, tt) in encodeTests) {
        slice<uint16> @out = default;
        foreach (var (_, u) in tt.@in) {
            @out = AppendRune(@out, u);
        }
        if (!slices.Equal<slice<uint16>, uint16>(@out, tt.@out)) {
            t.Errorf("AppendRune(%x) = %x; want %x", tt.@in, @out, tt.@out);
        }
    }
}

public static void TestEncodeRune(ж<testing.T> t) {
    foreach (var (i, tt) in encodeTests) {
        nint j = 0;
        foreach (var (_, r) in tt.@in) {
            var (r1, r2) = EncodeRune(r);
            if (r < 0x10000 || r > unicode.MaxRune) {
                if (j >= len(tt.@out)) {
                    t.Errorf("#%d: ran out of tt.out", i);
                    break;
                }
                if (r1 != unicode.ReplacementChar || r2 != unicode.ReplacementChar) {
                    t.Errorf("EncodeRune(%#x) = %#x, %#x; want 0xfffd, 0xfffd", r, r1, r2);
                }
                j++;
            } else {
                if (j + 1 >= len(tt.@out)) {
                    t.Errorf("#%d: ran out of tt.out", i);
                    break;
                }
                if (r1 != (rune)tt.@out[j] || r2 != (rune)tt.@out[j + 1]) {
                    t.Errorf("EncodeRune(%#x) = %#x, %#x; want %#x, %#x", r, r1, r2, tt.@out[j], tt.@out[j + 1]);
                }
                j += 2;
                var dec = DecodeRune(r1, r2);
                if (dec != r) {
                    t.Errorf("DecodeRune(%#x, %#x) = %#x; want %#x", r1, r2, dec, r);
                }
            }
        }
        if (j != len(tt.@out)) {
            t.Errorf("#%d: EncodeRune didn't generate enough output", i);
        }
    }
}

partial struct decodeTest {
    internal slice<uint16> @in;
    internal slice<rune> @out;
}

internal static slice<decodeTest> decodeTests = slice<decodeTest>([
    new(slice<uint16>([1, 2, 3, 4]), slice<rune>([1, 2, 3, 4])),
    new(slice<uint16>([0xffff, 0xd800, 0xdc00, 0xd800, 0xdc01, 0xd808, 0xdf45, 0xdbff, 0xdfff]),
        slice<rune>([0xffff, 0x10000, 0x10001, 0x12345, 0x10ffff])),
    new(slice<uint16>([0xd800, 'a']), slice<rune>([0xfffd, 'a'])),
    new(slice<uint16>([0xdfff]), slice<rune>([0xfffd]))
]);

public static void TestAllocationsDecode(ж<testing.T> t) {
    testenv.SkipIfOptimizationOff(new utf16_test_package.testing_TжTB(t));

    foreach (var (_, vᴛ1) in decodeTests) {
        ref var tt = ref heap(new decodeTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        var allocs = testing.AllocsPerRun(10, () => {
            var @out = Decode(ttʗ1.@in);
            if (@out == nil) {
                t.Errorf("Decode(%x) = nil", ttʗ1.@in);
            }
        });
        if (allocs > 0D) {
            t.Errorf("Decode allocated %v times", allocs);
        }
    }
}

public static void TestDecode(ж<testing.T> t) {
    foreach (var (_, tt) in decodeTests) {
        var @out = Decode(tt.@in);
        if (!slices.Equal<slice<rune>, rune>(@out, tt.@out)) {
            t.Errorf("Decode(%x) = %x; want %x", tt.@in, @out, tt.@out);
        }
    }
}

partial struct decodeRuneTestsᴛ1 /*dyn*/ {
    internal rune r1, r2;
    internal rune want;
}
internal static slice<decodeRuneTestsᴛ1> decodeRuneTests = slice<decodeRuneTestsᴛ1>([
    new(0xd800, 0xdc00, 0x10000),
    new(0xd800, 0xdc01, 0x10001),
    new(0xd808, 0xdf45, 0x12345),
    new(0xdbff, 0xdfff, 0x10ffff),
    new(0xd800, 'a', 0xfffd) // illegal, replacement rune substituted
]);

public static void TestDecodeRune(ж<testing.T> t) {
    foreach (var (i, tt) in decodeRuneTests) {
        var got = DecodeRune(tt.r1, tt.r2);
        if (got != tt.want) {
            t.Errorf("%d: DecodeRune(%q, %q) = %v; want %v", i, tt.r1, tt.r2, got, tt.want);
        }
    }
}

partial struct surrogateTestsᴛ1 /*dyn*/ {
    internal rune r;
    internal bool want;
}
internal static slice<surrogateTestsᴛ1> surrogateTests = slice<surrogateTestsᴛ1>([
    // from https://en.wikipedia.org/wiki/UTF-16
    new('\u007A', false),       // LATIN SMALL LETTER Z
    new('\u6C34', false),       // CJK UNIFIED IDEOGRAPH-6C34 (water)
    new('\uFEFF', false),       // Byte Order Mark
    new((rune)0x10000, false),  // LINEAR B SYLLABLE B008 A (first non-BMP code point)
    new((rune)0x1D11E, false),  // MUSICAL SYMBOL G CLEF
    new((rune)0x10FFFD, false), // PRIVATE USE CHARACTER-10FFFD (last Unicode code point)

    new((rune)0xd7ff, false), // surr1-1
    new((rune)0xd800, true),  // surr1
    new((rune)0xdc00, true),  // surr2
    new((rune)0xe000, false), // surr3
    new((rune)0xdfff, true)   // surr3-1
]);

public static void TestIsSurrogate(ж<testing.T> t) {
    foreach (var (i, tt) in surrogateTests) {
        var got = IsSurrogate(tt.r);
        if (got != tt.want) {
            t.Errorf("%d: IsSurrogate(%q) = %v; want %v", i, tt.r, got, tt.want);
        }
    }
}

public static void BenchmarkDecodeValidASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // "hello world"
    var data = slice<uint16>([104, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100]);
    for (nint i = 0; i < b.N; i++) {
        Decode(data);
    }
}

public static void BenchmarkDecodeValidJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // "日本語日本語日本語"
    var data = slice<uint16>([26085, 26412, 35486, 26085, 26412, 35486, 26085, 26412, 35486]);
    for (nint i = 0; i < b.N; i++) {
        Decode(data);
    }
}

public static void BenchmarkDecodeRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var rs = new slice<rune>(10);
    // U+1D4D0 to U+1D4D4: MATHEMATICAL BOLD SCRIPT CAPITAL LETTERS
    foreach (var (i, u) in slice<rune>([(rune)0x1D4D0, (rune)0x1D4D1, (rune)0x1D4D2, (rune)0x1D4D3, (rune)0x1D4D4])) {
        (rs[2 * i], rs[2 * i + 1]) = EncodeRune(u);
    }

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < 5; j++) {
            DecodeRune(rs[2 * j], rs[2 * j + 1]);
        }
    }
}

public static void BenchmarkEncodeValidASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = slice<rune>(['h', 'e', 'l', 'l', 'o']);
    for (nint i = 0; i < b.N; i++) {
        Encode(data);
    }
}

public static void BenchmarkEncodeValidJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = slice<rune>(['日', '本', '語']);
    for (nint i = 0; i < b.N; i++) {
        Encode(data);
    }
}

public static void BenchmarkAppendRuneValidASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = slice<rune>(['h', 'e', 'l', 'l', 'o']);
    var a = new slice<uint16>(0, len(data) * 2);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, u) in data) {
            a = AppendRune(a, u);
        }
        a = a[..0];
    }
}

public static void BenchmarkAppendRuneValidJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = slice<rune>(['日', '本', '語']);
    var a = new slice<uint16>(0, len(data) * 2);
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, u) in data) {
            a = AppendRune(a, u);
        }
        a = a[..0];
    }
}

public static void BenchmarkEncodeRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, u) in slice<rune>([(rune)0x1D4D0, (rune)0x1D4D1, (rune)0x1D4D2, (rune)0x1D4D3, (rune)0x1D4D4])) {
            EncodeRune(u);
        }
    }
}

} // end utf16_test_package
