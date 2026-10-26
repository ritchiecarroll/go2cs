// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using image = image_package;
using color = go.image.color_package;
using reflect = reflect_package;
using testing = testing_package;
using go.image;
using static go.image.draw_package;

partial class draw_internal_test_package {

internal static UntypedInt dstw => 640;
internal static UntypedInt dsth => 480;
internal static UntypedInt srcw => 400;
internal static UntypedInt srch => 300;

internal static color.Palette palette = new color.Palette(new color.Color[]{color.Black, color.White
}.slice());

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unknownDestinationColorˢ = (@string)"unknown destination color model"u8;
internal static readonly object unknownSourceColorModelˢ = (@string)"unknown source color model"u8;
internal static readonly object unknownMaskColorModelˢ = (@string)"unknown mask color model"u8;

// bench benchmarks drawing src and mask images onto a dst image with the
// given op and the color models to create those images from.
// The created images' pixels are initialized to non-zero values.
internal static void bench(ж<testing.B> Ꮡb, color.Model dcm, color.Model scm, color.Model mcm, global::go.image.draw_package.Op op) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();

    global::go.image.draw_package.Image dst = default!;
    var exprᴛ1 = dcm;
    if (AreEqual(exprᴛ1, color.RGBAModel)) {
        var dst1 = image.NewRGBA(image.Rect(0, 0, dstw, dsth));
        for (nint y = 0; y < dsth; y++) {
            for (nint x = 0; x < dstw; x++) {
                dst1.SetRGBA(x, y, new colorꓸRGBA(
                    (uint8)(5 * x % 0x100),
                    (uint8)(7 * y % 0x100),
                    (uint8)((7 * x + 5 * y) % 0x100),
                    0xff
                ));
            }
        }
        dst = new draw_test_package.image_ΔRGBAжImage(dst1);
    }
    else if (AreEqual(exprᴛ1, color.RGBA64Model)) {
        var dst1 = image.NewRGBA64(image.Rect(0, 0, dstw, dsth));
        for (nint y = 0; y < dsth; y++) {
            for (nint x = 0; x < dstw; x++) {
                dst1.SetRGBA64(x, y, new color.RGBA64(
                    (uint16)(53 * x % 0x10000),
                    (uint16)(59 * y % 0x10000),
                    (uint16)((59 * x + 53 * y) % 0x10000),
                    0xffff
                ));
            }
        }
        dst = new draw_test_package.image_RGBA64жImage(dst1);
    }
    else { /* default: */
        // The == operator isn't defined on a color.Palette (a slice), so we
        // use reflection.
        if (reflect.DeepEqual(dcm, palette)){
            var dst1 = image.NewPaletted(image.Rect(0, 0, dstw, dsth), palette);
            for (nint y = 0; y < dsth; y++) {
                for (nint x = 0; x < dstw; x++) {
                    dst1.SetColorIndex(x, y, (uint8)((uint8)(x ^ y) & 1));
                }
            }
            dst = new draw_test_package.image_PalettedжImage(dst1);
        } else {
            Ꮡb.Fatal(unknownDestinationColorˢ, dcm);
        }
    }

    image.Image src = default!;
    var exprᴛ2 = scm;
    if (AreEqual(exprᴛ2, default!)) {
        src = new image.UniformжImage(Ꮡ(new image.Uniform(C: new colorꓸRGBA(0x11, 0x22, 0x33, 0x44))));
    }
    else if (AreEqual(exprᴛ2, color.CMYKModel)) {
        var src1 = image.NewCMYK(image.Rect(0, 0, srcw, srch));
        for (nint y = 0; y < srch; y++) {
            for (nint x = 0; x < srcw; x++) {
                src1.SetCMYK(x, y, new color.CMYK(
                    (uint8)(13 * x % 0x100),
                    (uint8)(11 * y % 0x100),
                    (uint8)((11 * x + 13 * y) % 0x100),
                    (uint8)((31 * x + 37 * y) % 0x100)
                ));
            }
        }
        src = new image.CMYKжImage(src1);
    }
    else if (AreEqual(exprᴛ2, color.GrayModel)) {
        var src1 = image.NewGray(image.Rect(0, 0, srcw, srch));
        for (nint y = 0; y < srch; y++) {
            for (nint x = 0; x < srcw; x++) {
                src1.SetGray(x, y, new color.Gray(
                    (uint8)((11 * x + 13 * y) % 0x100)
                ));
            }
        }
        src = new image.GrayжImage(src1);
    }
    else if (AreEqual(exprᴛ2, color.RGBAModel)) {
        var src1 = image.NewRGBA(image.Rect(0, 0, srcw, srch));
        for (nint y = 0; y < srch; y++) {
            for (nint x = 0; x < srcw; x++) {
                src1.SetRGBA(x, y, new colorꓸRGBA(
                    (uint8)(13 * x % 0x80),
                    (uint8)(11 * y % 0x80),
                    (uint8)((11 * x + 13 * y) % 0x80),
                    0x7f
                ));
            }
        }
        src = new image.ΔRGBAжImage(src1);
    }
    else if (AreEqual(exprᴛ2, color.RGBA64Model)) {
        var src1 = image.NewRGBA64(image.Rect(0, 0, srcw, srch));
        for (nint y = 0; y < srch; y++) {
            for (nint x = 0; x < srcw; x++) {
                src1.SetRGBA64(x, y, new color.RGBA64(
                    (uint16)(103 * x % 0x8000),
                    (uint16)(101 * y % 0x8000),
                    (uint16)((101 * x + 103 * y) % 0x8000),
                    0x7fff
                ));
            }
        }
        src = new image.RGBA64жImage(src1);
    }
    else if (AreEqual(exprᴛ2, color.NRGBAModel)) {
        var src1 = image.NewNRGBA(image.Rect(0, 0, srcw, srch));
        for (nint y = 0; y < srch; y++) {
            for (nint x = 0; x < srcw; x++) {
                src1.SetNRGBA(x, y, new color.NRGBA(
                    (uint8)(13 * x % 0x100),
                    (uint8)(11 * y % 0x100),
                    (uint8)((11 * x + 13 * y) % 0x100),
                    0x7f
                ));
            }
        }
        src = new image.NRGBAжImage(src1);
    }
    else if (AreEqual(exprᴛ2, color.YCbCrModel)) {
        var yy = new slice<uint8>(srcw * srch);
        var cb = new slice<uint8>(srcw * srch);
        var cr = new slice<uint8>(srcw * srch);
        foreach (var i in range(yy)) {
            yy[i] = (uint8)(3 * i % 0x100);
            cb[i] = (uint8)(5 * i % 0x100);
            cr[i] = (uint8)(7 * i % 0x100);
        }
        src = new image.YCbCrжImage(Ꮡ(new image.YCbCr(
            Y: yy,
            Cb: cb,
            Cr: cr,
            YStride: srcw,
            CStride: srcw,
            SubsampleRatio: image.YCbCrSubsampleRatio444,
            Rect: image.Rect(0, 0, srcw, srch)
        )));
    }
    else { /* default: */
        Ꮡb.Fatal(unknownSourceColorModelˢ, scm);
    }

    image.Image mask = default!;
    var exprᴛ3 = mcm;
    if (AreEqual(exprᴛ3, default!)) {
        // No-op.
    }
    else if (AreEqual(exprᴛ3, color.AlphaModel)) {
        var mask1 = image.NewAlpha(image.Rect(0, 0, srcw, srch));
        for (nint y = 0; y < srch; y++) {
            for (nint x = 0; x < srcw; x++) {
                var a = (uint8)((23 * x + 29 * y) % 0x100);
                // Glyph masks are typically mostly zero,
                // so we only set a quarter of mask1's pixels.
                if (a >= 0xc0) {
                    mask1.SetAlpha(x, y, new color.Alpha(a));
                }
            }
        }
        mask = new image.AlphaжImage(mask1);
    }
    else { /* default: */
        Ꮡb.Fatal(unknownMaskColorModelˢ, mcm);
    }

    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        // Scatter the destination rectangle to draw into.
        nint x = 3 * i % (nint)(dstw - srcw);
        nint y = 7 * i % (nint)(dsth - srch);

        DrawMask(dst, dst.Bounds().Add(image.Pt(x, y)), src, new image.Point(nil), mask, new image.Point(nil), op);
    }
}

// The BenchmarkFoo functions exercise a drawFoo fast-path function in draw.go.
public static void BenchmarkFillOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, default!, default!, Over);
}

public static void BenchmarkFillSrc(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, default!, default!, Src);
}

public static void BenchmarkCopyOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.RGBAModel, default!, Over);
}

public static void BenchmarkCopySrc(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.RGBAModel, default!, Src);
}

public static void BenchmarkNRGBAOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.NRGBAModel, default!, Over);
}

public static void BenchmarkNRGBASrc(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.NRGBAModel, default!, Src);
}

public static void BenchmarkYCbCr(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.YCbCrModel, default!, Over);
}

public static void BenchmarkGray(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.GrayModel, default!, Over);
}

public static void BenchmarkCMYK(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.CMYKModel, default!, Over);
}

public static void BenchmarkGlyphOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, default!, color.AlphaModel, Over);
}

public static void BenchmarkRGBAMaskOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.RGBAModel, color.AlphaModel, Over);
}

public static void BenchmarkGrayMaskOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.GrayModel, color.AlphaModel, Over);
}

public static void BenchmarkRGBA64ImageMaskOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.RGBA64Model, color.AlphaModel, Over);
}

public static void BenchmarkRGBA(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBAModel, color.RGBA64Model, default!, Src);
}

public static void BenchmarkPalettedFill(ж<testing.B> Ꮡb) {
    bench(Ꮡb, palette, default!, default!, Src);
}

public static void BenchmarkPalettedRGBA(ж<testing.B> Ꮡb) {
    bench(Ꮡb, palette, color.RGBAModel, default!, Src);
}

// The BenchmarkGenericFoo functions exercise the generic, slow-path code.
public static void BenchmarkGenericOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBA64Model, color.RGBA64Model, default!, Over);
}

public static void BenchmarkGenericMaskOver(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBA64Model, color.RGBA64Model, color.AlphaModel, Over);
}

public static void BenchmarkGenericSrc(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBA64Model, color.RGBA64Model, default!, Src);
}

public static void BenchmarkGenericMaskSrc(ж<testing.B> Ꮡb) {
    bench(Ꮡb, color.RGBA64Model, color.RGBA64Model, color.AlphaModel, Src);
}

} // end draw_internal_test_package
