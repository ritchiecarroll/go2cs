// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using image = image_package;
using color = go.image.color_package;
using png = go.image.png_package;
using os = os_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using go.image;
using go.testing;
using io = io_package;
using static go.image.draw_package;

partial class draw_internal_test_package {

// slowestRGBA is a draw.Image like image.RGBA, but it is a different type and
// therefore does not trigger the draw.go fastest code paths.
//
// Unlike slowerRGBA, it does not implement the draw.RGBA64Image interface.
internal partial struct slowestRGBA {
    public slice<uint8> Pix;
    public nint Stride;
    public image.Rectangle Rect;
}

internal static color.Model ColorModel(this ref slowestRGBA p) {
    return color.RGBAModel;
}

internal static image.Rectangle Bounds(this ref slowestRGBA p) {
    return p.Rect;
}

internal static color.Color At(this ref slowestRGBA p, nint x, nint y) {
    return p.RGBA64At(x, y);
}

internal static color.RGBA64 RGBA64At(this ref slowestRGBA p, nint x, nint y) {
    if (!(new image.Point(x, y).In(p.Rect))) {
        return new color.RGBA64(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
    var r = (uint16)s[0];
    var g = (uint16)s[1];
    var b = (uint16)s[2];
    var a = (uint16)s[3];
    return new color.RGBA64(
        (uint16)(((uint16)(r << (int)(8))) | r),
        (uint16)(((uint16)(g << (int)(8))) | g),
        (uint16)(((uint16)(b << (int)(8))) | b),
        (uint16)(((uint16)(a << (int)(8))) | a)
    );
}

internal static void Set(this ref slowestRGBA p, nint x, nint y, color.Color c) {
    if (!(new image.Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.RGBAModel.Convert(c)._<colorꓸRGBA>();
    var s = p.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c1.R;
    s[1] = c1.G;
    s[2] = c1.B;
    s[3] = c1.A;
}

internal static nint PixOffset(this ref slowestRGBA p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4;
}

internal static ж<slowestRGBA> convertToSlowestRGBA(image.Image m) {
    {
        var (rgbaΔ1, ok) = m._<ж<imageꓸRGBA>>(ᐧ); if (ok) {
            return Ꮡ(new slowestRGBA(
                Pix: appendꓸꓸꓸ(slice<byte>(default!), (~rgbaΔ1).Pix),
                Stride: (~rgbaΔ1).Stride,
                Rect: (~rgbaΔ1).Rect
            ));
        }
    }
    var rgba = image.NewRGBA(m.Bounds());
    Draw(new draw_test_package.image_ΔRGBAжImage(rgba), rgba.Bounds(), m, m.Bounds().Min, Src);
    return Ꮡ(new slowestRGBA(
        Pix: (~rgba).Pix,
        Stride: (~rgba).Stride,
        Rect: (~rgba).Rect
    ));
}

[GoInit] internal static void init() {
    any p = ((ж<slowestRGBA>)nil);
    {
        var (_, ok) = p._<RGBA64Image>(ᐧ); if (ok) {
            throw panic("slowestRGBA should not be an RGBA64Image");
        }
    }
}

// slowerRGBA is a draw.Image like image.RGBA but it is a different type and
// therefore does not trigger the draw.go fastest code paths.
//
// Unlike slowestRGBA, it still implements the draw.RGBA64Image interface.
internal partial struct slowerRGBA {
    public slice<uint8> Pix;
    public nint Stride;
    public image.Rectangle Rect;
}

internal static color.Model ColorModel(this ref slowerRGBA p) {
    return color.RGBAModel;
}

internal static image.Rectangle Bounds(this ref slowerRGBA p) {
    return p.Rect;
}

internal static color.Color At(this ref slowerRGBA p, nint x, nint y) {
    return p.RGBA64At(x, y);
}

internal static color.RGBA64 RGBA64At(this ref slowerRGBA p, nint x, nint y) {
    if (!(new image.Point(x, y).In(p.Rect))) {
        return new color.RGBA64(nil);
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
    var r = (uint16)s[0];
    var g = (uint16)s[1];
    var b = (uint16)s[2];
    var a = (uint16)s[3];
    return new color.RGBA64(
        (uint16)(((uint16)(r << (int)(8))) | r),
        (uint16)(((uint16)(g << (int)(8))) | g),
        (uint16)(((uint16)(b << (int)(8))) | b),
        (uint16)(((uint16)(a << (int)(8))) | a)
    );
}

internal static void Set(this ref slowerRGBA p, nint x, nint y, color.Color c) {
    if (!(new image.Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var c1 = color.RGBAModel.Convert(c)._<colorꓸRGBA>();
    var s = p.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = c1.R;
    s[1] = c1.G;
    s[2] = c1.B;
    s[3] = c1.A;
}

internal static void SetRGBA64(this ref slowerRGBA p, nint x, nint y, color.RGBA64 c) {
    if (!(new image.Point(x, y).In(p.Rect))) {
        return;
    }
    nint i = p.PixOffset(x, y);
    var s = p.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
    s[0] = (uint8)((c.R >> (int)(8)));
    s[1] = (uint8)((c.G >> (int)(8)));
    s[2] = (uint8)((c.B >> (int)(8)));
    s[3] = (uint8)((c.A >> (int)(8)));
}

internal static nint PixOffset(this ref slowerRGBA p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.Stride + (x - p.Rect.Min.X) * 4;
}

internal static ж<slowerRGBA> convertToSlowerRGBA(image.Image m) {
    {
        var (rgbaΔ1, ok) = m._<ж<imageꓸRGBA>>(ᐧ); if (ok) {
            return Ꮡ(new slowerRGBA(
                Pix: appendꓸꓸꓸ(slice<byte>(default!), (~rgbaΔ1).Pix),
                Stride: (~rgbaΔ1).Stride,
                Rect: (~rgbaΔ1).Rect
            ));
        }
    }
    var rgba = image.NewRGBA(m.Bounds());
    Draw(new draw_test_package.image_ΔRGBAжImage(rgba), rgba.Bounds(), m, m.Bounds().Min, Src);
    return Ꮡ(new slowerRGBA(
        Pix: (~rgba).Pix,
        Stride: (~rgba).Stride,
        Rect: (~rgba).Rect
    ));
}

[GoInit] internal static void initΔ1() {
    any p = ((ж<slowerRGBA>)nil);
    {
        var (_, ok) = p._<RGBA64Image>(ᐧ); if (!ok) {
            throw panic("slowerRGBA should be an RGBA64Image");
        }
    }
}

internal static bool eq(color.Color c0, color.Color c1) {
    var (r0, g0, b0, a0) = c0.RGBA();
    var (r1, g1, b1, a1) = c1.RGBA();
    return r0 == r1 && g0 == g1 && b0 == b1 && a0 == a1;
}

internal static image.Image fillBlue(nint alpha) {
    return new image.UniformжImage(image.NewUniform(new colorꓸRGBA(0, 0, (uint8)alpha, (uint8)alpha)));
}

internal static image.Image fillAlpha(nint alpha) {
    return new image.UniformжImage(image.NewUniform(new color.Alpha((uint8)alpha)));
}

internal static image.Image vgradGreen(nint alpha) {
    var m = image.NewRGBA(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new colorꓸRGBA(0, (uint8)(y * alpha / 15), 0, (uint8)alpha));
        }
    }
    return new image.ΔRGBAжImage(m);
}

internal static image.Image vgradAlpha(nint alpha) {
    var m = image.NewAlpha(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new color.Alpha((uint8)(y * alpha / 15)));
        }
    }
    return new image.AlphaжImage(m);
}

internal static image.Image vgradGreenNRGBA(nint alpha) {
    var m = image.NewNRGBA(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new colorꓸRGBA(0, (uint8)(y * 0x11), 0, (uint8)alpha));
        }
    }
    return new image.NRGBAжImage(m);
}

internal static image.Image vgradCr() {
    var m = Ꮡ(new image.YCbCr(
        Y: new slice<byte>(16 * 16),
        Cb: new slice<byte>(16 * 16),
        Cr: new slice<byte>(16 * 16),
        YStride: 16,
        CStride: 16,
        SubsampleRatio: image.YCbCrSubsampleRatio444,
        Rect: image.Rect(0, 0, 16, 16)
    ));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Value.Cr[y * (~m).CStride + x] = (uint8)(y * 0x11);
        }
    }
    return new image.YCbCrжImage(m);
}

internal static image.Image vgradGray() {
    var m = image.NewGray(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new color.Gray((uint8)(y * 0x11)));
        }
    }
    return new image.GrayжImage(m);
}

internal static image.Image vgradMagenta() {
    var m = image.NewCMYK(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new color.CMYK(0, (uint8)(y * 0x11), 0, 0x3f));
        }
    }
    return new image.CMYKжImage(m);
}

internal static global::go.image.draw_package.Image hgradRed(nint alpha) {
    var m = image.NewRGBA(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new colorꓸRGBA((uint8)(x * alpha / 15), 0, 0, (uint8)alpha));
        }
    }
    return new draw_test_package.image_ΔRGBAжImage(m);
}

internal static global::go.image.draw_package.Image gradYellow(nint alpha) {
    var m = image.NewRGBA(image.Rect(0, 0, 16, 16));
    for (nint y = 0; y < 16; y++) {
        for (nint x = 0; x < 16; x++) {
            m.Set(x, y, new colorꓸRGBA((uint8)(x * alpha / 15), (uint8)(y * alpha / 15), 0, (uint8)alpha));
        }
    }
    return new draw_test_package.image_ΔRGBAжImage(m);
}

internal partial struct drawTest {
    internal @string desc;
    internal image.Image src;
    internal image.Image mask;
    internal global::go.image.draw_package.Op op;
    internal color.Color expected;
}

// Uniform mask (0% opaque).
// Uniform mask (100%, 75%, nil) and uniform source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {0, 0, 90, 90}.
// Uniform mask (100%, 75%, nil) and variable source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {0, 48, 0, 90}.
// Uniform mask (100%, 75%, nil) and variable NRGBA source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {0, 136, 0, 90} in NRGBA-space, which is {0, 48, 0, 90} in RGBA-space.
// The result pixel is different than in the "copy*" test cases because of rounding errors.
// Uniform mask (100%, 75%, nil) and variable YCbCr source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {0, 0, 136} in YCbCr-space, which is {11, 38, 0, 255} in RGB-space.
// Uniform mask (100%, 75%, nil) and variable Gray source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {136} in Gray-space, which is {136, 136, 136, 255} in RGBA-space.
// Same again, but with a slowerRGBA source.
// Same again, but with a slowestRGBA source.
// Uniform mask (100%, 75%, nil) and variable CMYK source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {0, 136, 0, 63} in CMYK-space, which is {192, 89, 192} in RGB-space.
// Variable mask and uniform source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is {0, 0, 255, 255}.
// The mask pixel's alpha is 102, or 40%.
// Same again, but with a slowerRGBA mask.
// Same again, but with a slowestRGBA mask.
// Variable mask and variable source.
// At (x, y) == (8, 8):
// The destination pixel is {136, 0, 0, 255}.
// The source pixel is:
//   - {0, 48, 0, 90}.
//   - {136} in Gray-space, which is {136, 136, 136, 255} in RGBA-space.
// The mask pixel's alpha is 102, or 40%.
internal static slice<drawTest> drawTests = new drawTest[]{
    new("nop"u8, vgradGreen(255), fillAlpha(0), Over, new colorꓸRGBA(136, 0, 0, 255)),
    new("clear"u8, vgradGreen(255), fillAlpha(0), Src, new colorꓸRGBA(0, 0, 0, 0)),
    new("fill"u8, fillBlue(90), fillAlpha(255), Over, new colorꓸRGBA(88, 0, 90, 255)),
    new("fillSrc"u8, fillBlue(90), fillAlpha(255), Src, new colorꓸRGBA(0, 0, 90, 90)),
    new("fillAlpha"u8, fillBlue(90), fillAlpha(192), Over, new colorꓸRGBA(100, 0, 68, 255)),
    new("fillAlphaSrc"u8, fillBlue(90), fillAlpha(192), Src, new colorꓸRGBA(0, 0, 68, 68)),
    new("fillNil"u8, fillBlue(90), default!, Over, new colorꓸRGBA(88, 0, 90, 255)),
    new("fillNilSrc"u8, fillBlue(90), default!, Src, new colorꓸRGBA(0, 0, 90, 90)),
    new("copy"u8, vgradGreen(90), fillAlpha(255), Over, new colorꓸRGBA(88, 48, 0, 255)),
    new("copySrc"u8, vgradGreen(90), fillAlpha(255), Src, new colorꓸRGBA(0, 48, 0, 90)),
    new("copyAlpha"u8, vgradGreen(90), fillAlpha(192), Over, new colorꓸRGBA(100, 36, 0, 255)),
    new("copyAlphaSrc"u8, vgradGreen(90), fillAlpha(192), Src, new colorꓸRGBA(0, 36, 0, 68)),
    new("copyNil"u8, vgradGreen(90), default!, Over, new colorꓸRGBA(88, 48, 0, 255)),
    new("copyNilSrc"u8, vgradGreen(90), default!, Src, new colorꓸRGBA(0, 48, 0, 90)),
    new("nrgba"u8, vgradGreenNRGBA(90), fillAlpha(255), Over, new colorꓸRGBA(88, 46, 0, 255)),
    new("nrgbaSrc"u8, vgradGreenNRGBA(90), fillAlpha(255), Src, new colorꓸRGBA(0, 46, 0, 90)),
    new("nrgbaAlpha"u8, vgradGreenNRGBA(90), fillAlpha(192), Over, new colorꓸRGBA(100, 34, 0, 255)),
    new("nrgbaAlphaSrc"u8, vgradGreenNRGBA(90), fillAlpha(192), Src, new colorꓸRGBA(0, 34, 0, 68)),
    new("nrgbaNil"u8, vgradGreenNRGBA(90), default!, Over, new colorꓸRGBA(88, 46, 0, 255)),
    new("nrgbaNilSrc"u8, vgradGreenNRGBA(90), default!, Src, new colorꓸRGBA(0, 46, 0, 90)),
    new("ycbcr"u8, vgradCr(), fillAlpha(255), Over, new colorꓸRGBA(11, 38, 0, 255)),
    new("ycbcrSrc"u8, vgradCr(), fillAlpha(255), Src, new colorꓸRGBA(11, 38, 0, 255)),
    new("ycbcrAlpha"u8, vgradCr(), fillAlpha(192), Over, new colorꓸRGBA(42, 28, 0, 255)),
    new("ycbcrAlphaSrc"u8, vgradCr(), fillAlpha(192), Src, new colorꓸRGBA(8, 28, 0, 192)),
    new("ycbcrNil"u8, vgradCr(), default!, Over, new colorꓸRGBA(11, 38, 0, 255)),
    new("ycbcrNilSrc"u8, vgradCr(), default!, Src, new colorꓸRGBA(11, 38, 0, 255)),
    new("gray"u8, vgradGray(), fillAlpha(255), Over, new colorꓸRGBA(136, 136, 136, 255)),
    new("graySrc"u8, vgradGray(), fillAlpha(255), Src, new colorꓸRGBA(136, 136, 136, 255)),
    new("grayAlpha"u8, vgradGray(), fillAlpha(192), Over, new colorꓸRGBA(136, 102, 102, 255)),
    new("grayAlphaSrc"u8, vgradGray(), fillAlpha(192), Src, new colorꓸRGBA(102, 102, 102, 192)),
    new("grayNil"u8, vgradGray(), default!, Over, new colorꓸRGBA(136, 136, 136, 255)),
    new("grayNilSrc"u8, vgradGray(), default!, Src, new colorꓸRGBA(136, 136, 136, 255)),
    new("graySlower"u8, new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradGray())), fillAlpha(255),
        Over, new colorꓸRGBA(136, 136, 136, 255)),
    new("graySrcSlower"u8, new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradGray())), fillAlpha(255),
        Src, new colorꓸRGBA(136, 136, 136, 255)),
    new("grayAlphaSlower"u8, new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradGray())), fillAlpha(192),
        Over, new colorꓸRGBA(136, 102, 102, 255)),
    new("grayAlphaSrcSlower"u8, new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradGray())), fillAlpha(192),
        Src, new colorꓸRGBA(102, 102, 102, 192)),
    new("grayNilSlower"u8, new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradGray())), default!,
        Over, new colorꓸRGBA(136, 136, 136, 255)),
    new("grayNilSrcSlower"u8, new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradGray())), default!,
        Src, new colorꓸRGBA(136, 136, 136, 255)),
    new("graySlowest"u8, new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradGray())), fillAlpha(255),
        Over, new colorꓸRGBA(136, 136, 136, 255)),
    new("graySrcSlowest"u8, new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradGray())), fillAlpha(255),
        Src, new colorꓸRGBA(136, 136, 136, 255)),
    new("grayAlphaSlowest"u8, new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradGray())), fillAlpha(192),
        Over, new colorꓸRGBA(136, 102, 102, 255)),
    new("grayAlphaSrcSlowest"u8, new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradGray())), fillAlpha(192),
        Src, new colorꓸRGBA(102, 102, 102, 192)),
    new("grayNilSlowest"u8, new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradGray())), default!,
        Over, new colorꓸRGBA(136, 136, 136, 255)),
    new("grayNilSrcSlowest"u8, new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradGray())), default!,
        Src, new colorꓸRGBA(136, 136, 136, 255)),
    new("cmyk"u8, vgradMagenta(), fillAlpha(255), Over, new colorꓸRGBA(192, 89, 192, 255)),
    new("cmykSrc"u8, vgradMagenta(), fillAlpha(255), Src, new colorꓸRGBA(192, 89, 192, 255)),
    new("cmykAlpha"u8, vgradMagenta(), fillAlpha(192), Over, new colorꓸRGBA(178, 67, 145, 255)),
    new("cmykAlphaSrc"u8, vgradMagenta(), fillAlpha(192), Src, new colorꓸRGBA(145, 67, 145, 192)),
    new("cmykNil"u8, vgradMagenta(), default!, Over, new colorꓸRGBA(192, 89, 192, 255)),
    new("cmykNilSrc"u8, vgradMagenta(), default!, Src, new colorꓸRGBA(192, 89, 192, 255)),
    new("generic"u8, fillBlue(255), vgradAlpha(192), Over, new colorꓸRGBA(81, 0, 102, 255)),
    new("genericSrc"u8, fillBlue(255), vgradAlpha(192), Src, new colorꓸRGBA(0, 0, 102, 102)),
    new("genericSlower"u8, fillBlue(255), new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradAlpha(192))),
        Over, new colorꓸRGBA(81, 0, 102, 255)),
    new("genericSrcSlower"u8, fillBlue(255), new draw_internal_test_package.slowerRGBAжimage_Image(convertToSlowerRGBA(vgradAlpha(192))),
        Src, new colorꓸRGBA(0, 0, 102, 102)),
    new("genericSlowest"u8, fillBlue(255), new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradAlpha(192))),
        Over, new colorꓸRGBA(81, 0, 102, 255)),
    new("genericSrcSlowest"u8, fillBlue(255), new draw_internal_test_package.slowestRGBAжimage_Image(convertToSlowestRGBA(vgradAlpha(192))),
        Src, new colorꓸRGBA(0, 0, 102, 102)),
    new("rgbaVariableMaskOver"u8, vgradGreen(90), vgradAlpha(192), Over, new colorꓸRGBA(117, 19, 0, 255)),
    new("grayVariableMaskOver"u8, vgradGray(), vgradAlpha(192), Over, new colorꓸRGBA(136, 54, 54, 255))
}.slice();

internal static image.Image makeGolden(image.Image dst, image.Rectangle r, image.Image src, image.Point sp, image.Image mask, image.Point mp, global::go.image.draw_package.Op op) {
    // Since golden is a newly allocated image, we don't have to check if the
    // input source and mask images and the output golden image overlap.
    var b = dst.Bounds();
    var sb = src.Bounds();
    var mb = image.Rect(-1000000000, -1000000000, 1000000000, 1000000000);
    if (mask != default!) {
        mb = mask.Bounds();
    }
    var golden = image.NewRGBA(image.Rect(0, 0, b.Max.X, b.Max.Y));
    for (nint y = r.Min.Y; y < r.Max.Y; y++) {
        nint sy = y + sp.Y - r.Min.Y;
        nint my = y + mp.Y - r.Min.Y;
        for (nint x = r.Min.X; x < r.Max.X; x++) {
            if (!(image.Pt(x, y).In(b))) {
                continue;
            }
            nint sx = x + sp.X - r.Min.X;
            if (!(image.Pt(sx, sy).In(sb))) {
                continue;
            }
            nint mx = x + mp.X - r.Min.X;
            if (!(image.Pt(mx, my).In(mb))) {
                continue;
            }
            UntypedInt M = /* 1<<16 - 1 */ 65535;
            uint32 dr = default!;
            uint32 dg = default!;
            uint32 db = default!;
            uint32 da = default!;
            if (op == Over) {
                (dr, dg, db, da) = dst.At(x, y).RGBA();
            }
            var (sr, sg, sbΔ1, sa) = src.At(sx, sy).RGBA();
            var ma = (uint32)M;
            if (mask != default!) {
                (_, _, _, ma) = mask.At(mx, my).RGBA();
            }
            var a = (uint32)M - (sa * ma / (uint32)M);
            golden.Set(x, y, new color.RGBA64(
                (uint16)((dr * a + sr * ma) / (uint32)M),
                (uint16)((dg * a + sg * ma) / (uint32)M),
                (uint16)((db * a + sbΔ1 * ma) / (uint32)M),
                (uint16)((da * a + sa * ma) / (uint32)M)
            ));
        }
    }
    return golden.SubImage(b);
}

public static void TestDraw(ж<testing.T> Ꮡt) {
    var rr = new image.Rectangle[]{
        image.Rect(0, 0, 0, 0),
        image.Rect(0, 0, 16, 16),
        image.Rect(3, 5, 12, 10),
        image.Rect(0, 0, 9, 9),
        image.Rect(8, 8, 16, 16),
        image.Rect(8, 0, 9, 16),
        image.Rect(0, 8, 16, 9),
        image.Rect(8, 8, 9, 9),
        image.Rect(8, 8, 8, 8)
    }.slice();
    foreach (var (_, r) in rr) {
loop:
        foreach (var (_, test) in drawTests) {
            for (nint i = 0; i < 3; i++) {
                var dst = hgradRed(255)._<ж<imageꓸRGBA>>().SubImage(r)._<Image>();
                // For i != 0, substitute a different-typed dst that will take
                // us off the fastest code paths. We should still get the same
                // result, in terms of final pixel RGBA values.
                switch (i) {
                case 1: {
                    dst = new draw_internal_test_package.slowerRGBAжdraw_Image(convertToSlowerRGBA(dst));
                    break;
                }
                case 2: {
                    dst = new draw_internal_test_package.slowestRGBAжdraw_Image(convertToSlowestRGBA(dst));
                    break;
                }}

                // Draw the (src, mask, op) onto a copy of dst using a slow but obviously correct implementation.
                var golden = makeGolden(dst, image.Rect(0, 0, 16, 16), test.src, new image.Point(nil), test.mask, new image.Point(nil), test.op);
                var b = dst.Bounds();
                if (!b.Eq(golden.Bounds())) {
                    Ꮡt.Errorf("draw %v %s on %T: bounds %v versus %v"u8,
                        r, test.desc, dst, dst.Bounds(), golden.Bounds());
                    continue;
                }
                // Draw the same combination onto the actual dst using the optimized DrawMask implementation.
                DrawMask(dst, image.Rect(0, 0, 16, 16), test.src, new image.Point(nil), test.mask, new image.Point(nil), test.op);
                if (image.Pt(8, 8).In(r)) {
                    // Check that the resultant pixel at (8, 8) matches what we expect
                    // (the expected value can be verified by hand).
                    if (!eq(dst.At(8, 8), test.expected)) {
                        Ꮡt.Errorf("draw %v %s on %T: at (8, 8) %v versus %v"u8,
                            r, test.desc, dst, dst.At(8, 8), test.expected);
                        continue;
                    }
                }
                // Check that the resultant dst image matches the golden output.
                for (nint y = b.Min.Y; y < b.Max.Y; y++) {
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        if (!eq(dst.At(x, y), golden.At(x, y))) {
                            Ꮡt.Errorf("draw %v %s on %T: at (%d, %d), %v versus golden %v"u8,
                                r, test.desc, dst, x, y, dst.At(x, y), golden.At(x, y));
                            goto continue_loop;
                        }
                    }
                }
            }
continue_loop:;
        }
break_loop:;
    }
}

public static void TestDrawOverlap(ж<testing.T> Ꮡt) {
    foreach (var (_, op) in new global::go.image.draw_package.Op[]{Over, Src}.slice()) {
        for (nint yoff = -2; yoff <= 2; yoff++) {
loop:
            for (nint xoff = -2; xoff <= 2; xoff++) {
                var m = gradYellow(127)._<ж<imageꓸRGBA>>();
                var dst = m.SubImage(image.Rect(5, 5, 10, 10))._<ж<imageꓸRGBA>>();
                var src = m.SubImage(image.Rect(5 + xoff, 5 + yoff, 10 + xoff, 10 + yoff))._<ж<imageꓸRGBA>>();
                var b = dst.Bounds();
                // Draw the (src, mask, op) onto a copy of dst using a slow but obviously correct implementation.
                var golden = makeGolden(new image.ΔRGBAжImage(dst), b, new image.ΔRGBAжImage(src), src.Bounds().Min, default!, new image.Point(nil), op);
                if (!b.Eq(golden.Bounds())) {
                    Ꮡt.Errorf("drawOverlap xoff=%d,yoff=%d: bounds %v versus %v"u8, xoff, yoff, dst.Bounds(), golden.Bounds());
                    continue;
                }
                // Draw the same combination onto the actual dst using the optimized DrawMask implementation.
                DrawMask(new draw_test_package.image_ΔRGBAжImage(dst), b, new image.ΔRGBAжImage(src), src.Bounds().Min, default!, new image.Point(nil), op);
                // Check that the resultant dst image matches the golden output.
                for (nint y = b.Min.Y; y < b.Max.Y; y++) {
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        if (!eq(dst.At(x, y), golden.At(x, y))) {
                            Ꮡt.Errorf("drawOverlap xoff=%d,yoff=%d: at (%d, %d), %v versus golden %v"u8, xoff, yoff, x, y, dst.At(x, y), golden.At(x, y));
                            goto continue_loop;
                        }
                    }
                }
continue_loop:;
            }
break_loop:;
        }
    }
}

// TestNonZeroSrcPt checks drawing with a non-zero src point parameter.
public static void TestNonZeroSrcPt(ж<testing.T> Ꮡt) {
    var a = image.NewRGBA(image.Rect(0, 0, 1, 1));
    var b = image.NewRGBA(image.Rect(0, 0, 2, 2));
    b.Set(0, 0, new colorꓸRGBA(0, 0, 0, 5));
    b.Set(1, 0, new colorꓸRGBA(0, 0, 5, 5));
    b.Set(0, 1, new colorꓸRGBA(0, 5, 0, 5));
    b.Set(1, 1, new colorꓸRGBA(5, 0, 0, 5));
    Draw(new draw_test_package.image_ΔRGBAжImage(a), image.Rect(0, 0, 1, 1), new image.ΔRGBAжImage(b), image.Pt(1, 1), Over);
    if (!eq(new colorꓸRGBA(5, 0, 0, 5), a.At(0, 0))) {
        Ꮡt.Errorf("non-zero src pt: want %v got %v"u8, new colorꓸRGBA(5, 0, 0, 5), a.At(0, 0));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pixelˢ = "pixel"u8;
internal static readonly @string rowˢ = "row"u8;
internal static readonly @string columnˢ = "column"u8;
internal static readonly @string wholeˢ = "whole"u8;

public static void TestFill(ж<testing.T> Ꮡt) {
    var rr = new image.Rectangle[]{
        image.Rect(0, 0, 0, 0),
        image.Rect(0, 0, 40, 30),
        image.Rect(10, 0, 40, 30),
        image.Rect(0, 20, 40, 30),
        image.Rect(10, 20, 40, 30),
        image.Rect(10, 20, 15, 25),
        image.Rect(10, 0, 35, 30),
        image.Rect(0, 15, 40, 16),
        image.Rect(24, 24, 25, 25),
        image.Rect(23, 23, 26, 26),
        image.Rect(22, 22, 27, 27),
        image.Rect(21, 21, 28, 28),
        image.Rect(20, 20, 29, 29)
    }.slice();
    foreach (var (_, vᴛ1) in rr) {
        ref var r = ref heap(new image.Rectangle(), out var Ꮡr);
        r = vᴛ1;

        var m = image.NewRGBA(image.Rect(0, 0, 40, 30)).SubImage(r)._<ж<imageꓸRGBA>>();
        ref var b = ref heap<image.Rectangle>(out var Ꮡb);
        b = m.Bounds();
        ref var c = ref heap<colorꓸRGBA>(out var Ꮡc);
        c = new colorꓸRGBA(11, 0, 0, 255);
        var src = Ꮡ(new image.Uniform(C: c));
        var bʗ1 = b;
        var mʗ1 = m;
        var rʗ1 = r;
        void check(@string desc) {
            for (nint y = bʗ1.Min.Y; y < bʗ1.Max.Y; y++) {
                for (nint x = bʗ1.Min.X; x < bʗ1.Max.X; x++) {
                    if (!eq(Ꮡc.Value, mʗ1.At(x, y))) {
                        Ꮡt.Errorf("%s fill: at (%d, %d), sub-image bounds=%v: want %v got %v"u8, desc, x, y, rʗ1, Ꮡc.Value, mʗ1.At(x, y));
                        return;
                    }
                }
            }
        }
        // Draw 1 pixel at a time.
        for (nint y = b.Min.Y; y < b.Max.Y; y++) {
            for (nint x = b.Min.X; x < b.Max.X; x++) {
                DrawMask(new draw_test_package.image_ΔRGBAжImage(m), image.Rect(x, y, x + 1, y + 1), new image.UniformжImage(src), new image.Point(nil), default!, new image.Point(nil), Src);
            }
        }
        check(pixelˢ);
        // Draw 1 row at a time.
        c = new colorꓸRGBA(0, 22, 0, 255);
        src = Ꮡ(new image.Uniform(C: c));
        for (nint y = b.Min.Y; y < b.Max.Y; y++) {
            DrawMask(new draw_test_package.image_ΔRGBAжImage(m), image.Rect(b.Min.X, y, b.Max.X, y + 1), new image.UniformжImage(src), new image.Point(nil), default!, new image.Point(nil), Src);
        }
        check(rowˢ);
        // Draw 1 column at a time.
        c = new colorꓸRGBA(0, 0, 33, 255);
        src = Ꮡ(new image.Uniform(C: c));
        for (nint x = b.Min.X; x < b.Max.X; x++) {
            DrawMask(new draw_test_package.image_ΔRGBAжImage(m), image.Rect(x, b.Min.Y, x + 1, b.Max.Y), new image.UniformжImage(src), new image.Point(nil), default!, new image.Point(nil), Src);
        }
        check(columnˢ);
        // Draw the whole image at once.
        c = new colorꓸRGBA(44, 55, 66, 77);
        src = Ꮡ(new image.Uniform(C: c));
        DrawMask(new draw_test_package.image_ΔRGBAжImage(m), b, new image.UniformжImage(src), new image.Point(nil), default!, new image.Point(nil), Src);
        check(wholeˢ);
    }
}

public static void TestDrawSrcNonpremultiplied(ж<testing.T> Ꮡt) {
    color.NRGBA opaqueGray = new color.NRGBA(0x99, 0x99, 0x99, 0xff);
    color.NRGBA transparentBlue = new color.NRGBA(0x00, 0x00, 0xff, 0x00);
    color.NRGBA transparentGreen = new color.NRGBA(0x00, 0xff, 0x00, 0x00);
    color.NRGBA transparentRed = new color.NRGBA(0xff, 0x00, 0x00, 0x00);
    color.NRGBA64 opaqueGray64 = new color.NRGBA64(0x9999, 0x9999, 0x9999, 0xffff);
    color.NRGBA64 transparentPurple64 = new color.NRGBA64(0xfedc, 0x0000, 0x7654, 0x0000);
    // dst and src are 1x3 images but the dr rectangle (and hence the overlap)
    // is only 1x2. The Draw call should affect dst's pixels at (1, 10) and (2,
    // 10) but the pixel at (0, 10) should be untouched.
    //
    // The src image is entirely transparent (and the Draw operator is Src) so
    // the two touched pixels should be set to transparent colors.
    //
    // In general, Go's color.Color type (and specifically the Color.RGBA
    // method) works in premultiplied alpha, where there's no difference
    // between "transparent blue" and "transparent red". It's all "just
    // transparent" and canonically "transparent black" (all zeroes).
    //
    // However, since the operator is Src (so the pixels are 'copied', not
    // 'blended') and both dst and src images are *image.NRGBA (N stands for
    // Non-premultiplied alpha which *does* distinguish "transparent blue" and
    // "transparent red"), we prefer that this distinction carries through and
    // dst's touched pixels should be transparent blue and transparent green,
    // not just transparent black.
    {
        var dst = image.NewNRGBA(image.Rect(0, 10, 3, 11));
        dst.SetNRGBA(0, 10, opaqueGray);
        var src = image.NewNRGBA(image.Rect(1, 20, 4, 21));
        src.SetNRGBA(1, 20, transparentBlue);
        src.SetNRGBA(2, 20, transparentGreen);
        src.SetNRGBA(3, 20, transparentRed);
        var dr = image.Rect(1, 10, 3, 11);
        Draw(new draw_test_package.image_NRGBAжImage(dst), dr, new image.NRGBAжImage(src), new image.Point(1, 20), Src);
        {
            var (got, want) = (dst.At(0, 10), opaqueGray); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("At(0, 10):\ngot  %#v\nwant %#v"u8, got, want);
            }
        }
        {
            var (got, want) = (dst.At(1, 10), transparentBlue); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("At(1, 10):\ngot  %#v\nwant %#v"u8, got, want);
            }
        }
        {
            var (got, want) = (dst.At(2, 10), transparentGreen); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("At(2, 10):\ngot  %#v\nwant %#v"u8, got, want);
            }
        }
    }
    // Check image.NRGBA64 (not image.NRGBA) similarly.
    {
        var dst = image.NewNRGBA64(image.Rect(0, 0, 1, 1));
        dst.SetNRGBA64(0, 0, opaqueGray64);
        var src = image.NewNRGBA64(image.Rect(0, 0, 1, 1));
        src.SetNRGBA64(0, 0, transparentPurple64);
        Draw(new draw_test_package.image_NRGBA64жImage(dst), dst.Bounds(), new image.NRGBA64жImage(src), new image.Point(0, 0), Src);
        {
            var (got, want) = (dst.At(0, 0), transparentPurple64); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("At(0, 0):\ngot  %#v\nwant %#v"u8, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thereMayBeMoreErrorsˢ = (@string)"there may be more errors"u8;

// TestFloydSteinbergCheckerboard tests that the result of Floyd-Steinberg
// error diffusion of a uniform 50% gray source image with a black-and-white
// palette is a checkerboard pattern.
public static void TestFloydSteinbergCheckerboard(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var b = image.Rect(0, 0, 640, 480);
    // We can't represent 50% exactly, but 0x7fff / 0xffff is close enough.
    var src = Ꮡ(new image.Uniform(new color.Gray16(0x7fff)));
    var dst = image.NewPaletted(b, new color.Palette(new color.Color[]{color.Black, color.White}.slice()));
    FloydSteinberg.Draw(new draw_test_package.image_PalettedжImage(dst), b, new image.UniformжImage(src), new image.Point(nil));
    nint nErr = 0;
    for (nint y = b.Min.Y; y < b.Max.Y; y++) {
        for (nint x = b.Min.X; x < b.Max.X; x++) {
            var got = (~dst).Pix[dst.PixOffset(x, y)];
            var want = (uint8)((uint8)(x + y) % 2);
            if (got != want) {
                Ꮡt.Errorf("at (%d, %d): got %d, want %d"u8, x, y, got, want);
                {
                    nErr++; if (nErr == 10) {
                        Ꮡt.Fatal(thereMayBeMoreErrorsˢ);
                    }
                }
            }
        }
    }
}

// embeddedPaletted is an Image that behaves like an *image.Paletted but whose
// type is not *image.Paletted.
internal partial struct embeddedPaletted {
    public partial ref ж<image_package.Paletted> Paletted { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo001Pngˢ = "../testdata/video-001.png"u8;

// TestPaletted tests that the drawPaletted function behaves the same
// regardless of whether dst is an *image.Paletted.
public static void TestPaletted(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(testdataVideo001Pngˢ);
        if (err != default!) {
            Ꮡt.Fatalf("open: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var video001, err) = png.Decode(new draw_test_package.os_FileжReader(f));
        if (err != default!) {
            Ꮡt.Fatalf("decode: %v"u8, err);
        }
        var b = video001.Bounds();
        var cgaPalette = new color.Palette(new color.Color[]{new colorꓸRGBA(0x00, 0x00, 0x00, 0xff), new colorꓸRGBA(0x55, 0xff, 0xff, 0xff), new colorꓸRGBA(0xff, 0x55, 0xff, 0xff), new colorꓸRGBA(0xff, 0xff, 0xff, 0xff)
        }.slice());
        var drawers = new map<@string, global::go.image.draw_package.Drawer>{["src"u8] = Src, ["floyd-steinberg"u8] = FloydSteinberg
        };
        var sources = new map<@string, image.Image>{["uniform"u8] = new image.UniformжImage(Ꮡ(new image.Uniform(new colorꓸRGBA(0xff, 0x7f, 0xff, 0xff)))), ["video001"u8] = video001
        };
        foreach (var (dName, d) in drawers) {
loop:
            foreach (var (sName, src) in sources) {
                var dst0 = image.NewPaletted(b, cgaPalette);
                var dst1 = image.NewPaletted(b, cgaPalette);
                d.Draw(new draw_test_package.image_PalettedжImage(dst0), b, src, new image.Point(nil));
                d.Draw(new embeddedPaletted(dst1), b, src, new image.Point(nil));
                for (nint y = b.Min.Y; y < b.Max.Y; y++) {
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        if (!eq(dst0.At(x, y), dst1.At(x, y))) {
                            Ꮡt.Errorf("%s / %s: at (%d, %d), %v versus %v"u8,
                                dName, sName, x, y, dst0.At(x, y), dst1.At(x, y));
                            goto continue_loop;
                        }
                    }
                }
continue_loop:;
            }
break_loop:;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSqDiff(ж<testing.T> Ꮡt) {
    // This test is similar to the one from the image/color package, but
    // sqDiff in this package accepts int32 instead of uint32, so test it
    // for appropriate input.
    // canonical sqDiff implementation
    var orig = (int32 x, int32 y) => {
        uint32 d = default!;
        if (x > y){
            d = (uint32)(x - y);
        } else {
            d = (uint32)(y - x);
        }
        return ((d * d) >> (int)(2));
    };
    var testCases = new int32[]{
        0,
        1,
        2,
        0x0fffd,
        0x0fffe,
        0x0ffff,
        0x10000,
        0x10001,
        0x10002,
        0x7ffffffd,
        0x7ffffffe,
        0x7fffffff,
        -0x7ffffffd,
        -0x7ffffffe,
        -2147483648
    }.slice();
    foreach (var (_, x) in testCases) {
        foreach (var (_, y) in testCases) {
            {
                var (got, want) = (sqDiff(x, y), orig(x, y)); if (got != want) {
                    Ꮡt.Fatalf("sqDiff(%#x, %#x): got %d, want %d"u8, x, y, got, want);
                }
            }
        }
    }
    {
        var err = quick.CheckEqual((orig).OrTypedNilFunc(), sqDiff, Ꮡ(new quick.Config(MaxCountScale: 10D))); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

} // end draw_internal_test_package
