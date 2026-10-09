// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using color = image.color_package;
using palette = image.color.palette_package;
using testing = testing_package;
using image;
using image.color;
using static go.image_package;

partial class image_internal_test_package {

internal partial interface image :
    Image
{
    bool Opaque();
    void Set(nint _Δp0, nint _Δp1, color.Color _Δp2);
    global::go.image_package.Image SubImage(global::go.image_package.Rectangle _);
}

internal static bool cmp(color.Model cm, color.Color c0, color.Color c1) {
    var (r0, g0, b0, a0) = cm.Convert(c0).RGBA();
    var (r1, g1, b1, a1) = cm.Convert(c1).RGBA();
    return r0 == r1 && g0 == g1 && b0 == b1 && a0 == a1;
}


partial struct testImagesᴛ1 /*dyn*/ {
    internal @string name;
    internal Func<image> image;
}
internal static slice<testImagesᴛ1> testImages;
internal static void initᴛtestImages() { testImages = new testImagesᴛ1[]{
    new("rgba"u8, () => new image_internal_test_package.image_ΔRGBAжimage(NewRGBA(Rect(0, 0, 10, 10)))),
    new("rgba64"u8, () => new image_internal_test_package.image_RGBA64жimage(NewRGBA64(Rect(0, 0, 10, 10)))),
    new("nrgba"u8, () => new image_internal_test_package.image_NRGBAжimage(NewNRGBA(Rect(0, 0, 10, 10)))),
    new("nrgba64"u8, () => new image_internal_test_package.image_NRGBA64жimage(NewNRGBA64(Rect(0, 0, 10, 10)))),
    new("alpha"u8, () => new image_internal_test_package.image_Alphaжimage(NewAlpha(Rect(0, 0, 10, 10)))),
    new("alpha16"u8, () => new image_internal_test_package.image_Alpha16жimage(NewAlpha16(Rect(0, 0, 10, 10)))),
    new("gray"u8, () => new image_internal_test_package.image_Grayжimage(NewGray(Rect(0, 0, 10, 10)))),
    new("gray16"u8, () => new image_internal_test_package.image_Gray16жimage(NewGray16(Rect(0, 0, 10, 10)))),
    new("paletted"u8, () => new image_internal_test_package.image_Palettedжimage(NewPaletted(Rect(0, 0, 10, 10), new color.Palette(new color.Color[]{new image_test_package.image_UniformжColor(Transparent), new image_test_package.image_UniformжColor(go.image_package.ΔOpaque)
        }.slice()))))
}.slice(); }

public static void TestImage(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in testImages) {
        var m = tc.image();
        if (!Rect(0, 0, 10, 10).Eq(m.Bounds())) {
            Ꮡt.Errorf("%T: want bounds %v, got %v"u8, m, Rect(0, 0, 10, 10), m.Bounds());
            continue;
        }
        if (!cmp(m.ColorModel(), new image_test_package.image_UniformжColor(Transparent), m.At(6, 3))) {
            Ꮡt.Errorf("%T: at (6, 3), want a zero color, got %v"u8, m, m.At(6, 3));
            continue;
        }
        m.Set(6, 3, new image_test_package.image_UniformжColor(go.image_package.ΔOpaque));
        if (!cmp(m.ColorModel(), new image_test_package.image_UniformжColor(go.image_package.ΔOpaque), m.At(6, 3))) {
            Ꮡt.Errorf("%T: at (6, 3), want a non-zero color, got %v"u8, m, m.At(6, 3));
            continue;
        }
        if (!m.SubImage(Rect(6, 3, 7, 4))._<image>().Opaque()) {
            Ꮡt.Errorf("%T: at (6, 3) was not opaque"u8, m);
            continue;
        }
        m = m.SubImage(Rect(3, 2, 9, 8))._<image>();
        if (!Rect(3, 2, 9, 8).Eq(m.Bounds())) {
            Ꮡt.Errorf("%T: sub-image want bounds %v, got %v"u8, m, Rect(3, 2, 9, 8), m.Bounds());
            continue;
        }
        if (!cmp(m.ColorModel(), new image_test_package.image_UniformжColor(go.image_package.ΔOpaque), m.At(6, 3))) {
            Ꮡt.Errorf("%T: sub-image at (6, 3), want a non-zero color, got %v"u8, m, m.At(6, 3));
            continue;
        }
        if (!cmp(m.ColorModel(), new image_test_package.image_UniformжColor(Transparent), m.At(3, 3))) {
            Ꮡt.Errorf("%T: sub-image at (3, 3), want a zero color, got %v"u8, m, m.At(3, 3));
            continue;
        }
        m.Set(3, 3, new image_test_package.image_UniformжColor(go.image_package.ΔOpaque));
        if (!cmp(m.ColorModel(), new image_test_package.image_UniformжColor(go.image_package.ΔOpaque), m.At(3, 3))) {
            Ꮡt.Errorf("%T: sub-image at (3, 3), want a non-zero color, got %v"u8, m, m.At(3, 3));
            continue;
        }
        // Test that taking an empty sub-image starting at a corner does not panic.
        m.SubImage(Rect(0, 0, 0, 0));
        m.SubImage(Rect(10, 0, 10, 0));
        m.SubImage(Rect(0, 10, 0, 10));
        m.SubImage(Rect(10, 10, 10, 10));
    }
}

internal partial struct TestNewXxxBadRectangle_testCases /*dyn*/ {
    internal @string name;
    internal Action<global::go.image_package.Rectangle> f;
}

public static void TestNewXxxBadRectangle(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // call calls f(r) and reports whether it ran without panicking.
    bool /*ok*/ call(Action<global::go.image_package.Rectangle> f, global::go.image_package.Rectangle r) {
        bool ok = default!;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                if (recover() != default!) {
                    ok = false;
                }
            }, ref ᒐ);
            f(r);
            ok = true; goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return ok;
    }
    var testCases = new TestNewXxxBadRectangle_testCases[]{
        new("RGBA"u8, (global::go.image_package.Rectangle r) => {
            NewRGBA(r);
        }),
        new("RGBA64"u8, (global::go.image_package.Rectangle r) => {
            NewRGBA64(r);
        }),
        new("NRGBA"u8, (global::go.image_package.Rectangle r) => {
            NewNRGBA(r);
        }),
        new("NRGBA64"u8, (global::go.image_package.Rectangle r) => {
            NewNRGBA64(r);
        }),
        new("Alpha"u8, (global::go.image_package.Rectangle r) => {
            NewAlpha(r);
        }),
        new("Alpha16"u8, (global::go.image_package.Rectangle r) => {
            NewAlpha16(r);
        }),
        new("Gray"u8, (global::go.image_package.Rectangle r) => {
            NewGray(r);
        }),
        new("Gray16"u8, (global::go.image_package.Rectangle r) => {
            NewGray16(r);
        }),
        new("CMYK"u8, (global::go.image_package.Rectangle r) => {
            NewCMYK(r);
        }),
        new("Paletted"u8, (global::go.image_package.Rectangle r) => {
            NewPaletted(r, new color.Palette(new color.Color[]{color.Black, color.White}.slice()));
        }),
        new("YCbCr"u8, (global::go.image_package.Rectangle r) => {
            NewYCbCr(r, YCbCrSubsampleRatio422);
        }),
        new("NYCbCrA"u8, (global::go.image_package.Rectangle r) => {
            NewNYCbCrA(r, YCbCrSubsampleRatio444);
        })
    }.slice();
    foreach (var (_, tc) in testCases) {
        // Calling NewXxx(r) should fail (panic, since NewXxx doesn't return an
        // error) unless r's width and height are both non-negative.
        foreach (var (_, negDx) in new bool[]{false, true}.slice()) {
            foreach (var (_, negDy) in new bool[]{false, true}.slice()) {
                var r = new Rectangle(
                    Min: new Point(15, 28),
                    Max: new Point(16, 29)
                );
                if (negDx) {
                    r.Max.X = 14;
                }
                if (negDy) {
                    r.Max.Y = 27;
                }
                var got = call(tc.f, r);
                var want = !negDx && !negDy;
                if (got != want) {
                    Ꮡt.Errorf("New%s: negDx=%t, negDy=%t: got %t, want %t"u8,
                        tc.name, negDx, negDy, got, want);
                }
            }
        }
        // Passing a Rectangle whose width and height is MaxInt should also fail
        // (panic), due to overflow.
        {
            nuint zeroAsUint = (nuint)0;
            nuint maxUint = zeroAsUint - 1;
            nint maxInt = (nint)(maxUint / 2);
            var got = call(tc.f, new Rectangle(
                Min: new Point(0, 0),
                Max: new Point(maxInt, maxInt)
            ));
            if (got) {
                Ꮡt.Errorf("New%s: overflow: got ok, want !ok"u8, tc.name);
            }
        }
    }
}

public static void Test16BitsPerColorChannel(ж<testing.T> Ꮡt) {
    var testColorModel = new color.Model[]{color.RGBA64Model, color.NRGBA64Model, color.Alpha16Model, color.Gray16Model
    }.slice();
    foreach (var (_, cm) in testColorModel) {
        var c = cm.Convert(new color.RGBA64(0x1234, 0x1234, 0x1234, 0x1234)); // Premultiplied alpha.
        var (r, _, _, _) = c.RGBA();
        if (r != 0x1234) {
            Ꮡt.Errorf("%T: want red value 0x%04x got 0x%04x"u8, c, (nint)(0x1234), r);
            continue;
        }
    }
    var testImage = new image[]{new image_internal_test_package.image_RGBA64жimage(NewRGBA64(Rect(0, 0, 10, 10))), new image_internal_test_package.image_NRGBA64жimage(NewNRGBA64(Rect(0, 0, 10, 10))), new image_internal_test_package.image_Alpha16жimage(NewAlpha16(Rect(0, 0, 10, 10))), new image_internal_test_package.image_Gray16жimage(NewGray16(Rect(0, 0, 10, 10)))
    }.slice();
    foreach (var (_, m) in testImage) {
        m.Set(1, 2, new color.NRGBA64(0xffff, 0xffff, 0xffff, 0x1357)); // Non-premultiplied alpha.
        var (r, _, _, _) = m.At(1, 2).RGBA();
        if (r != 0x1357) {
            Ꮡt.Errorf("%T: want red value 0x%04x got 0x%04x"u8, m, (nint)(0x1357), r);
            continue;
        }
    }
}

// Most of the concrete image types in the testCases implement the
// draw.RGBA64Image interface: they have a SetRGBA64 method. We use an
// interface literal here, instead of importing "image/draw", to avoid
// an import cycle.
//
// The YCbCr and NYCbCrA types are special-cased. Chroma subsampling
// means that setting one pixel can modify neighboring pixels. They
// don't have Set or SetRGBA64 methods because that side effect could
// be surprising. Here, we just memset the channel buffers instead.
//
// The Uniform and Rectangle types are also special-cased, as they
// don't have a Set or SetRGBA64 method.
internal partial interface TestRGBA64Image_type /*dyn*/ {
    void SetRGBA64(nint x, nint y, color.RGBA64 c);
}

public static void TestRGBA64Image(ж<testing.T> Ꮡt) {
    // memset sets every element of s to v.
    void memset(slice<byte> s, byte v) {
        foreach (var (i, _) in s) {
            s[i] = v;
        }
    }
    var r = Rect(0, 0, 3, 2);
    var testCases = new global::go.image_package.Image[]{new global::go.image_package.AlphaжImage(NewAlpha(r)), new global::go.image_package.Alpha16жImage(NewAlpha16(r)), new global::go.image_package.CMYKжImage(NewCMYK(r)), new global::go.image_package.GrayжImage(NewGray(r)), new global::go.image_package.Gray16жImage(NewGray16(r)), new global::go.image_package.NRGBAжImage(NewNRGBA(r)), new global::go.image_package.NRGBA64жImage(NewNRGBA64(r)), new global::go.image_package.NYCbCrAжImage(NewNYCbCrA(r, YCbCrSubsampleRatio444)), new global::go.image_package.PalettedжImage(NewPaletted(r, palette.Plan9)), new image_test_package.image_ΔRGBAжImage(NewRGBA(r)), new global::go.image_package.RGBA64жImage(NewRGBA64(r)), new global::go.image_package.UniformжImage(NewUniform(new color.RGBA64(nil))), new global::go.image_package.YCbCrжImage(NewYCbCr(r, YCbCrSubsampleRatio444)), new image_test_package.image_RectangleᴠImage(r)
    }.slice();
    foreach (var (_, tc) in testCases) {
        switch (tc.type()) {
        case {} ΔtcΔ1 when ΔtcΔ1._<TestRGBA64Image_type>(out var tcΔ1): {
            tcΔ1.SetRGBA64(1, 1, new color.RGBA64(0x7FFF, 0x3FFF, 0x0000, 0x7FFF));
            break;
        }
        case ж<global::go.image_package.NYCbCrA> tcΔ1: {
            memset((~tcΔ1).YCbCr.Y, 0x77);
            memset((~tcΔ1).YCbCr.Cb, 0x88);
            memset((~tcΔ1).YCbCr.Cr, 0x99);
            memset((~tcΔ1).A, 0xAA);
            break;
        }
        case ж<global::go.image_package.Uniform> tcΔ1: {
            tcΔ1.Value.C = new color.RGBA64(0x7FFF, 0x3FFF, 0x0000, 0x7FFF);
            break;
        }
        case ж<global::go.image_package.YCbCr> tcΔ1: {
            memset((~tcΔ1).Y, 0x77);
            memset((~tcΔ1).Cb, 0x88);
            memset((~tcΔ1).Cr, 0x99);
            break;
        }
        case Rectangle tcΔ1: {
            break;
        }
        default: {
            var tcΔ1 = tc;
            Ꮡt.Errorf("could not initialize pixels for %T"u8, // No-op. Rectangle pixels' colors are immutable. They're always
 // color.Opaque.
 tcΔ1);
            continue;
            break;
        }}
        // Check that RGBA64At(x, y) is equivalent to At(x, y).RGBA().
        var (rgba64Image, ok) = tc._<RGBA64Image>(ᐧ);
        if (!ok) {
            Ꮡt.Errorf("%T is not an RGBA64Image"u8, tc);
            continue;
        }
        var got = rgba64Image.RGBA64At(1, 1);
        var (wantR, wantG, wantB, wantA) = tc.At(1, 1).RGBA();
        if (((uint32)got.R != wantR) || ((uint32)got.G != wantG) || ((uint32)got.B != wantB) || ((uint32)got.A != wantA)) {
            Ꮡt.Errorf("%T:\ngot  (0x%04X, 0x%04X, 0x%04X, 0x%04X)\n"u8 + "want (0x%04X, 0x%04X, 0x%04X, 0x%04X)"u8, tc,
                got.R, got.G, got.B, got.A,
                wantR, wantG, wantB, wantA);
            continue;
        }
    }
}

public static void BenchmarkAt(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in testImages) {
        ref var tc = ref heap(new testImagesᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡb.Run(tc.name, (ж<testing.B> bΔ1) => {
            var m = tcʗ1.image();
            bΔ1.ReportAllocs();
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                m.At(4, 5);
            }
        });
    }
}

public static void BenchmarkSet(ж<testing.B> Ꮡb) {
    ref var c = ref heap<color.Gray>(out var Ꮡc);
    c = new color.Gray(0xff);
    foreach (var (_, vᴛ1) in testImages) {
        ref var tc = ref heap(new testImagesᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var cʗ1 = c;
        var tcʗ1 = tc;
        Ꮡb.Run(tc.name, (ж<testing.B> bΔ1) => {
            var m = tcʗ1.image();
            bΔ1.ReportAllocs();
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                m.Set(4, 5, cʗ1);
            }
        });
    }
}

public static void BenchmarkRGBAAt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewRGBA(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.RGBAAt(4, 5);
    }
}

public static void BenchmarkRGBASetRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewRGBA(Rect(0, 0, 10, 10));
    var c = new colorꓸRGBA(0xff, 0xff, 0xff, 0x13);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetRGBA(4, 5, c);
    }
}

public static void BenchmarkRGBA64At(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewRGBA64(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.RGBA64At(4, 5);
    }
}

public static void BenchmarkRGBA64SetRGBA64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewRGBA64(Rect(0, 0, 10, 10));
    var c = new color.RGBA64(0xffff, 0xffff, 0xffff, 0x1357);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetRGBA64(4, 5, c);
    }
}

public static void BenchmarkNRGBAAt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewNRGBA(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.NRGBAAt(4, 5);
    }
}

public static void BenchmarkNRGBASetNRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewNRGBA(Rect(0, 0, 10, 10));
    var c = new color.NRGBA(0xff, 0xff, 0xff, 0x13);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetNRGBA(4, 5, c);
    }
}

public static void BenchmarkNRGBA64At(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewNRGBA64(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.NRGBA64At(4, 5);
    }
}

public static void BenchmarkNRGBA64SetNRGBA64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewNRGBA64(Rect(0, 0, 10, 10));
    var c = new color.NRGBA64(0xffff, 0xffff, 0xffff, 0x1357);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetNRGBA64(4, 5, c);
    }
}

public static void BenchmarkAlphaAt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewAlpha(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.AlphaAt(4, 5);
    }
}

public static void BenchmarkAlphaSetAlpha(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewAlpha(Rect(0, 0, 10, 10));
    var c = new color.Alpha(0x13);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetAlpha(4, 5, c);
    }
}

public static void BenchmarkAlpha16At(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewAlpha16(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.Alpha16At(4, 5);
    }
}

public static void BenchmarkAlphaSetAlpha16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewAlpha16(Rect(0, 0, 10, 10));
    var c = new color.Alpha16(0x13);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetAlpha16(4, 5, c);
    }
}

public static void BenchmarkGrayAt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewGray(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.GrayAt(4, 5);
    }
}

public static void BenchmarkGraySetGray(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewGray(Rect(0, 0, 10, 10));
    var c = new color.Gray(0x13);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetGray(4, 5, c);
    }
}

public static void BenchmarkGray16At(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewGray16(Rect(0, 0, 10, 10));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.Gray16At(4, 5);
    }
}

public static void BenchmarkGraySetGray16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = NewGray16(Rect(0, 0, 10, 10));
    var c = new color.Gray16(0x13);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        m.SetGray16(4, 5, c);
    }
}

} // end image_internal_test_package
