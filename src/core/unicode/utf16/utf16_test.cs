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
public static void TestConstants(ж<testing.T> Ꮡt) {
    if (utf16_internal_test_package.MaxRune != unicode.MaxRune) {
        Ꮡt.Errorf("utf16.maxRune is wrong: %x should be %x"u8, (int32)(utf16_internal_test_package.MaxRune), (int32)(unicode.MaxRune));
    }
    if (utf16_internal_test_package.ReplacementChar != unicode.ReplacementChar) {
        Ꮡt.Errorf("utf16.replacementChar is wrong: %x should be %x"u8, (int32)(utf16_internal_test_package.ReplacementChar), (int32)(unicode.ReplacementChar));
    }
}

internal partial struct TestRuneLen_type /*dyn*/ {
    internal rune r;
    internal nint length;
}

public static void TestRuneLen(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in new TestRuneLen_type[]{
        new(0, 1),
        new(utf16_internal_test_package.Surr1 - 1, 1),
        new(utf16_internal_test_package.Surr3, 1),
        new(utf16_internal_test_package.SurrSelf - 1, 1),
        new(utf16_internal_test_package.SurrSelf, 2),
        new(utf16_internal_test_package.MaxRune, 2),
        new(utf16_internal_test_package.MaxRune + 1, -1),
        new(-1, -1)
    }.slice()) {
        {
            nint length = RuneLen(tt.r); if (length != tt.length) {
                Ꮡt.Errorf("RuneLen(%#U) = %d, want %d"u8, tt.r, length, tt.length);
            }
        }
    }
}

partial struct encodeTest {
    internal slice<rune> @in;
    internal slice<uint16> @out;
}

internal static slice<encodeTest> encodeTests = new encodeTest[]{
    new(new rune[]{1, 2, 3, 4}.slice(), new uint16[]{1, 2, 3, 4}.slice()),
    new(new rune[]{0xffff, 0x10000, 0x10001, 0x12345, 0x10ffff}.slice(),
        new uint16[]{0xffff, 0xd800, 0xdc00, 0xd800, 0xdc01, 0xd808, 0xdf45, 0xdbff, 0xdfff}.slice()),
    new(new rune[]{(rune)'a', (rune)'b', 0xd7ff, 0xd800, 0xdfff, 0xe000, 0x110000, -1}.slice(),
        new uint16[]{(rune)'a', (rune)'b', 0xd7ff, 0xfffd, 0xfffd, 0xe000, 0xfffd, 0xfffd}.slice())
}.slice();

public static void TestEncode(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in encodeTests) {
        var @out = Encode(tt.@in);
        if (!slices.Equal<slice<uint16>, uint16>(@out, tt.@out)) {
            Ꮡt.Errorf("Encode(%x) = %x; want %x"u8, tt.@in, @out, tt.@out);
        }
    }
}

public static void TestAppendRune(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in encodeTests) {
        slice<uint16> @out = default!;
        foreach (var (_, u) in tt.@in) {
            @out = AppendRune(@out, u);
        }
        if (!slices.Equal<slice<uint16>, uint16>(@out, tt.@out)) {
            Ꮡt.Errorf("AppendRune(%x) = %x; want %x"u8, tt.@in, @out, tt.@out);
        }
    }
}

public static void TestEncodeRune(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, tt) in encodeTests) {
        nint j = 0;
        foreach (var (_, r) in tt.@in) {
            var (r1, r2) = EncodeRune(r);
            if (r < 0x10000 || r > unicode.MaxRune){
                if (j >= len(tt.@out)) {
                    Ꮡt.Errorf("#%d: ran out of tt.out"u8, i);
                    break;
                }
                if (r1 != unicode.ReplacementChar || r2 != unicode.ReplacementChar) {
                    Ꮡt.Errorf("EncodeRune(%#x) = %#x, %#x; want 0xfffd, 0xfffd"u8, r, r1, r2);
                }
                j++;
            } else {
                if (j + 1 >= len(tt.@out)) {
                    Ꮡt.Errorf("#%d: ran out of tt.out"u8, i);
                    break;
                }
                if (r1 != (rune)tt.@out[j] || r2 != (rune)tt.@out[j + 1]) {
                    Ꮡt.Errorf("EncodeRune(%#x) = %#x, %#x; want %#x, %#x"u8, r, r1, r2, tt.@out[j], tt.@out[j + 1]);
                }
                j += 2;
                var dec = DecodeRune(r1, r2);
                if (dec != r) {
                    Ꮡt.Errorf("DecodeRune(%#x, %#x) = %#x; want %#x"u8, r1, r2, dec, r);
                }
            }
        }
        if (j != len(tt.@out)) {
            Ꮡt.Errorf("#%d: EncodeRune didn't generate enough output"u8, i);
        }
    }
}

partial struct decodeTest {
    internal slice<uint16> @in;
    internal slice<rune> @out;
}

internal static slice<decodeTest> decodeTests = new decodeTest[]{
    new(new uint16[]{1, 2, 3, 4}.slice(), new rune[]{1, 2, 3, 4}.slice()),
    new(new uint16[]{0xffff, 0xd800, 0xdc00, 0xd800, 0xdc01, 0xd808, 0xdf45, 0xdbff, 0xdfff}.slice(),
        new rune[]{0xffff, 0x10000, 0x10001, 0x12345, 0x10ffff}.slice()),
    new(new uint16[]{0xd800, (rune)'a'}.slice(), new rune[]{0xfffd, (rune)'a'}.slice()),
    new(new uint16[]{0xdfff}.slice(), new rune[]{0xfffd}.slice())
}.slice();

public static void TestAllocationsDecode(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new utf16_test_package.testing_TжTB(Ꮡt));
    foreach (var (_, vᴛ1) in decodeTests) {
        ref var tt = ref heap(new decodeTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        var allocs = testing.AllocsPerRun(10, () => {
            var @out = Decode(ttʗ1.@in);
            if (@out == default!) {
                Ꮡt.Errorf("Decode(%x) = nil"u8, ttʗ1.@in);
            }
        });
        if (allocs > 0D) {
            Ꮡt.Errorf("Decode allocated %v times"u8, allocs);
        }
    }
}

public static void TestDecode(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in decodeTests) {
        var @out = Decode(tt.@in);
        if (!slices.Equal<slice<rune>, rune>(@out, tt.@out)) {
            Ꮡt.Errorf("Decode(%x) = %x; want %x"u8, tt.@in, @out, tt.@out);
        }
    }
}

// illegal, replacement rune substituted

partial struct decodeRuneTestsᴛ1 /*dyn*/ {
    internal rune r1, r2;
    internal rune want;
}
internal static slice<decodeRuneTestsᴛ1> decodeRuneTests = new decodeRuneTestsᴛ1[]{
    new(0xd800, 0xdc00, 0x10000),
    new(0xd800, 0xdc01, 0x10001),
    new(0xd808, 0xdf45, 0x12345),
    new(0xdbff, 0xdfff, 0x10ffff),
    new(0xd800, (rune)'a', 0xfffd)
}.slice();

public static void TestDecodeRune(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in decodeRuneTests) {
        var got = DecodeRune(tt.r1, tt.r2);
        if (got != tt.want) {
            Ꮡt.Errorf("%d: DecodeRune(%q, %q) = %v; want %v"u8, i, tt.r1, tt.r2, got, tt.want);
        }
    }
}

// from https://en.wikipedia.org/wiki/UTF-16
// LATIN SMALL LETTER Z
// CJK UNIFIED IDEOGRAPH-6C34 (water)
// Byte Order Mark
// LINEAR B SYLLABLE B008 A (first non-BMP code point)
// MUSICAL SYMBOL G CLEF
// PRIVATE USE CHARACTER-10FFFD (last Unicode code point)
// surr1-1
// surr1
// surr2
// surr3
// surr3-1

partial struct surrogateTestsᴛ1 /*dyn*/ {
    internal rune r;
    internal bool want;
}
internal static slice<surrogateTestsᴛ1> surrogateTests = new surrogateTestsᴛ1[]{
    new((rune)'\u007A', false),
    new((rune)'\u6C34', false),
    new((rune)'\uFEFF', false),
    new((rune)0x10000, false),
    new((rune)0x1D11E, false),
    new((rune)0x10FFFD, false),
    new((rune)0xd7ff, false),
    new((rune)0xd800, true),
    new((rune)0xdc00, true),
    new((rune)0xe000, false),
    new((rune)0xdfff, true)
}.slice();

public static void TestIsSurrogate(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in surrogateTests) {
        var got = IsSurrogate(tt.r);
        if (got != tt.want) {
            Ꮡt.Errorf("%d: IsSurrogate(%q) = %v; want %v"u8, i, tt.r, got, tt.want);
        }
    }
}

public static void BenchmarkDecodeValidASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // "hello world"
    var data = new uint16[]{104, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100}.slice();
    for (nint i = 0; i < b.N; i++) {
        Decode(data);
    }
}

public static void BenchmarkDecodeValidJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // "日本語日本語日本語"
    var data = new uint16[]{26085, 26412, 35486, 26085, 26412, 35486, 26085, 26412, 35486}.slice();
    for (nint i = 0; i < b.N; i++) {
        Decode(data);
    }
}

public static void BenchmarkDecodeRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var rs = new slice<rune>(10);
    // U+1D4D0 to U+1D4D4: MATHEMATICAL BOLD SCRIPT CAPITAL LETTERS
    foreach (var (i, u) in new rune[]{(rune)0x1D4D0, (rune)0x1D4D1, (rune)0x1D4D2, (rune)0x1D4D3, (rune)0x1D4D4}.slice()) {
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

    var data = new rune[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice();
    for (nint i = 0; i < b.N; i++) {
        Encode(data);
    }
}

public static void BenchmarkEncodeValidJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = new rune[]{(rune)'日', (rune)'本', (rune)'語'}.slice();
    for (nint i = 0; i < b.N; i++) {
        Encode(data);
    }
}

public static void BenchmarkAppendRuneValidASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = new rune[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o'}.slice();
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

    var data = new rune[]{(rune)'日', (rune)'本', (rune)'語'}.slice();
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
        foreach (var (_, u) in new rune[]{(rune)0x1D4D0, (rune)0x1D4D1, (rune)0x1D4D2, (rune)0x1D4D3, (rune)0x1D4D4}.slice()) {
            EncodeRune(u);
        }
    }
}

} // end utf16_test_package
