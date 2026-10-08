// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using color = image.color_package;
using image;

partial class image_package {

partial struct YCbCrSubsampleRatio /*num:nint*/;

public static YCbCrSubsampleRatio YCbCrSubsampleRatio444 => /* iota */ 0;
public static YCbCrSubsampleRatio YCbCrSubsampleRatio422 => 1;
public static YCbCrSubsampleRatio YCbCrSubsampleRatio420 => 2;
public static YCbCrSubsampleRatio YCbCrSubsampleRatio440 => 3;
public static YCbCrSubsampleRatio YCbCrSubsampleRatio411 => 4;
public static YCbCrSubsampleRatio YCbCrSubsampleRatio410 => 5;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string yCbCrSubsampleRatio444ˢ = "YCbCrSubsampleRatio444"u8;
internal static readonly @string yCbCrSubsampleRatio422ˢ = "YCbCrSubsampleRatio422"u8;
internal static readonly @string yCbCrSubsampleRatio420ˢ = "YCbCrSubsampleRatio420"u8;
internal static readonly @string yCbCrSubsampleRatio440ˢ = "YCbCrSubsampleRatio440"u8;
internal static readonly @string yCbCrSubsampleRatio411ˢ = "YCbCrSubsampleRatio411"u8;
internal static readonly @string yCbCrSubsampleRatio410ˢ = "YCbCrSubsampleRatio410"u8;

public static @string String(this YCbCrSubsampleRatio s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == YCbCrSubsampleRatio444) {
        return yCbCrSubsampleRatio444ˢ;
    }
    if (exprᴛ1 == YCbCrSubsampleRatio422) {
        return yCbCrSubsampleRatio422ˢ;
    }
    if (exprᴛ1 == YCbCrSubsampleRatio420) {
        return yCbCrSubsampleRatio420ˢ;
    }
    if (exprᴛ1 == YCbCrSubsampleRatio440) {
        return yCbCrSubsampleRatio440ˢ;
    }
    if (exprᴛ1 == YCbCrSubsampleRatio411) {
        return yCbCrSubsampleRatio411ˢ;
    }
    if (exprᴛ1 == YCbCrSubsampleRatio410) {
        return yCbCrSubsampleRatio410ˢ;
    }

    return "YCbCrSubsampleRatioUnknown"u8;
}

// YCbCr is an in-memory image of Y'CbCr colors. There is one Y sample per
// pixel, but each Cb and Cr sample can span one or more pixels.
// YStride is the Y slice index delta between vertically adjacent pixels.
// CStride is the Cb and Cr slice index delta between vertically adjacent pixels
// that map to separate chroma samples.
// It is not an absolute requirement, but YStride and len(Y) are typically
// multiples of 8, and:
//
//	For 4:4:4, CStride == YStride/1 && len(Cb) == len(Cr) == len(Y)/1.
//	For 4:2:2, CStride == YStride/2 && len(Cb) == len(Cr) == len(Y)/2.
//	For 4:2:0, CStride == YStride/2 && len(Cb) == len(Cr) == len(Y)/4.
//	For 4:4:0, CStride == YStride/1 && len(Cb) == len(Cr) == len(Y)/2.
//	For 4:1:1, CStride == YStride/4 && len(Cb) == len(Cr) == len(Y)/4.
//	For 4:1:0, CStride == YStride/4 && len(Cb) == len(Cr) == len(Y)/8.
partial struct YCbCr {
    public slice<uint8> Y, Cb, Cr;
    public nint YStride;
    public nint CStride;
    public YCbCrSubsampleRatio SubsampleRatio;
    public Rectangle Rect;
}

public static color.Model ColorModel(this ref YCbCr p) {
    return color.YCbCrModel;
}

public static Rectangle Bounds(this ref YCbCr p) {
    return p.Rect;
}

public static color.Color At(this ref YCbCr p, nint x, nint y) {
    return p.YCbCrAt(x, y);
}

public static color.RGBA64 RGBA64At(this ref YCbCr p, nint x, nint y) {
    var (r, g, b, a) = p.YCbCrAt(x, y).RGBA();
    return new color.RGBA64((uint16)r, (uint16)g, (uint16)b, (uint16)a);
}

public static color.YCbCr YCbCrAt(this ref YCbCr p, nint x, nint y) {
    if (!(new Point(x, y).In(p.Rect))) {
        return new color.YCbCr(nil);
    }
    nint yi = p.YOffset(x, y);
    nint ci = p.COffset(x, y);
    return new color.YCbCr(
        p.Y[yi],
        p.Cb[ci],
        p.Cr[ci]
    );
}

// YOffset returns the index of the first element of Y that corresponds to
// the pixel at (x, y).
public static nint YOffset(this ref YCbCr p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.YStride + (x - p.Rect.Min.X);
}

// COffset returns the index of the first element of Cb or Cr that corresponds
// to the pixel at (x, y).
public static nint COffset(this ref YCbCr p, nint x, nint y) {
    var exprᴛ1 = p.SubsampleRatio;
    if (exprᴛ1 == YCbCrSubsampleRatio422) {
        return (y - p.Rect.Min.Y) * p.CStride + (x / 2 - p.Rect.Min.X / 2);
    }
    if (exprᴛ1 == YCbCrSubsampleRatio420) {
        return (y / 2 - p.Rect.Min.Y / 2) * p.CStride + (x / 2 - p.Rect.Min.X / 2);
    }
    if (exprᴛ1 == YCbCrSubsampleRatio440) {
        return (y / 2 - p.Rect.Min.Y / 2) * p.CStride + (x - p.Rect.Min.X);
    }
    if (exprᴛ1 == YCbCrSubsampleRatio411) {
        return (y - p.Rect.Min.Y) * p.CStride + (x / 4 - p.Rect.Min.X / 4);
    }
    if (exprᴛ1 == YCbCrSubsampleRatio410) {
        return (y / 2 - p.Rect.Min.Y / 2) * p.CStride + (x / 4 - p.Rect.Min.X / 4);
    }

    // Default to 4:4:4 subsampling.
    return (y - p.Rect.Min.Y) * p.CStride + (x - p.Rect.Min.X);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
public static Image SubImage(this ref YCbCr p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new YCbCrжImage(Ꮡ(new YCbCr(
            SubsampleRatio: p.SubsampleRatio
        )));
    }
    nint yi = p.YOffset(r.Min.X, r.Min.Y);
    nint ci = p.COffset(r.Min.X, r.Min.Y);
    return new YCbCrжImage(Ꮡ(new YCbCr(
        Y: p.Y.slice(yi),
        Cb: p.Cb.slice(ci),
        Cr: p.Cr.slice(ci),
        SubsampleRatio: p.SubsampleRatio,
        YStride: p.YStride,
        CStride: p.CStride,
        Rect: r
    )));
}

public static bool Opaque(this ref YCbCr p) {
    return true;
}

internal static (nint w, nint h, nint cw, nint ch) yCbCrSize(Rectangle r, YCbCrSubsampleRatio subsampleRatio) {
    nint w = default!;
    nint h = default!;
    nint cw = default!;
    nint ch = default!;

    (w, h) = (r.Dx(), r.Dy());
    var exprᴛ1 = subsampleRatio;
    if (exprᴛ1 == YCbCrSubsampleRatio422) {
        cw = (r.Max.X + 1) / 2 - r.Min.X / 2;
        ch = h;
    }
    else if (exprᴛ1 == YCbCrSubsampleRatio420) {
        cw = (r.Max.X + 1) / 2 - r.Min.X / 2;
        ch = (r.Max.Y + 1) / 2 - r.Min.Y / 2;
    }
    else if (exprᴛ1 == YCbCrSubsampleRatio440) {
        cw = w;
        ch = (r.Max.Y + 1) / 2 - r.Min.Y / 2;
    }
    else if (exprᴛ1 == YCbCrSubsampleRatio411) {
        cw = (r.Max.X + 3) / 4 - r.Min.X / 4;
        ch = h;
    }
    else if (exprᴛ1 == YCbCrSubsampleRatio410) {
        cw = (r.Max.X + 3) / 4 - r.Min.X / 4;
        ch = (r.Max.Y + 1) / 2 - r.Min.Y / 2;
    }
    else { /* default: */
        cw = w;
        ch = h;
    }

    // Default to 4:4:4 subsampling.
    return (w, h, cw, ch);
}

// NewYCbCr returns a new YCbCr image with the given bounds and subsample
// ratio.
public static ж<YCbCr> NewYCbCr(Rectangle r, YCbCrSubsampleRatio subsampleRatio) {
    ref var w = ref heap<nint>(out var Ꮡw);
    ref var cw = ref heap<nint>(out var Ꮡcw);
    (w, var h, cw, var ch) = yCbCrSize(r, subsampleRatio);
    // totalLength should be the same as i2, below, for a valid Rectangle r.
    nint totalLength = add2NonNeg(
        mul3NonNeg(1, w, h),
        mul3NonNeg(2, cw, ch));
    if (totalLength < 0) {
        throw panic("image: NewYCbCr Rectangle has huge or negative dimensions");
    }
    nint i0 = w * h + 0 * cw * ch;
    nint i1 = w * h + 1 * cw * ch;
    nint i2 = w * h + 2 * cw * ch;
    var b = new slice<byte>(i2);
    return Ꮡ(new YCbCr(
        Y: b.slice(0, i0, i0),
        Cb: b.slice(i0, i1, i1),
        Cr: b.slice(i1, i2, i2),
        SubsampleRatio: subsampleRatio,
        YStride: w,
        CStride: cw,
        Rect: r
    ));
}

// NYCbCrA is an in-memory image of non-alpha-premultiplied Y'CbCr-with-alpha
// colors. A and AStride are analogous to the Y and YStride fields of the
// embedded YCbCr.
partial struct NYCbCrA {
    public partial ref YCbCr YCbCr { get; }
    public slice<uint8> A;
    public nint AStride;
}

public static color.Model ColorModel(this ref NYCbCrA p) {
    return color.NYCbCrAModel;
}

public static color.Color At(this ref NYCbCrA p, nint x, nint y) {
    return p.NYCbCrAAt(x, y);
}

public static color.RGBA64 RGBA64At(this ref NYCbCrA p, nint x, nint y) {
    var (r, g, b, a) = p.NYCbCrAAt(x, y).RGBA();
    return new color.RGBA64((uint16)r, (uint16)g, (uint16)b, (uint16)a);
}

public static color.NYCbCrA NYCbCrAAt(this ref NYCbCrA p, nint x, nint y) {
    if (!(new Point(X: x, Y: y).In(p.Rect))) {
        return new color.NYCbCrA(nil);
    }
    nint yi = p.YCbCr.YOffset(x, y);
    nint ci = p.YCbCr.COffset(x, y);
    nint ai = p.AOffset(x, y);
    return new color.NYCbCrA(
        new color.YCbCr(
            Y: p.Y[yi],
            Cb: p.Cb[ci],
            Cr: p.Cr[ci]
        ),
        p.A[ai]
    );
}

// AOffset returns the index of the first element of A that corresponds to the
// pixel at (x, y).
public static nint AOffset(this ref NYCbCrA p, nint x, nint y) {
    return (y - p.Rect.Min.Y) * p.AStride + (x - p.Rect.Min.X);
}

// SubImage returns an image representing the portion of the image p visible
// through r. The returned value shares pixels with the original image.
public static Image SubImage(this ref NYCbCrA p, Rectangle r) {
    r = r.Intersect(p.Rect);
    // If r1 and r2 are Rectangles, r1.Intersect(r2) is not guaranteed to be inside
    // either r1 or r2 if the intersection is empty. Without explicitly checking for
    // this, the Pix[i:] expression below can panic.
    if (r.Empty()) {
        return new NYCbCrAжImage(Ꮡ(new NYCbCrA(
            YCbCr: new YCbCr(
                SubsampleRatio: p.SubsampleRatio
            )
        )));
    }
    nint yi = p.YCbCr.YOffset(r.Min.X, r.Min.Y);
    nint ci = p.YCbCr.COffset(r.Min.X, r.Min.Y);
    nint ai = p.AOffset(r.Min.X, r.Min.Y);
    return new NYCbCrAжImage(Ꮡ(new NYCbCrA(
        YCbCr: new YCbCr(
            Y: p.Y.slice(yi),
            Cb: p.Cb.slice(ci),
            Cr: p.Cr.slice(ci),
            SubsampleRatio: p.SubsampleRatio,
            YStride: p.YStride,
            CStride: p.CStride,
            Rect: r
        ),
        A: p.A.slice(ai),
        AStride: p.AStride
    )));
}

// Opaque scans the entire image and reports whether it is fully opaque.
public static bool Opaque(this ref NYCbCrA p) {
    if (p.Rect.Empty()) {
        return true;
    }
    nint i0 = 0;
    nint i1 = p.Rect.Dx();
    for (nint y = p.Rect.Min.Y; y < p.Rect.Max.Y; y++) {
        foreach (var (_, a) in p.A.slice(i0, i1)) {
            if (a != 0xff) {
                return false;
            }
        }
        i0 += p.AStride;
        i1 += p.AStride;
    }
    return true;
}

// NewNYCbCrA returns a new [NYCbCrA] image with the given bounds and subsample
// ratio.
public static ж<NYCbCrA> NewNYCbCrA(Rectangle r, YCbCrSubsampleRatio subsampleRatio) {
    ref var w = ref heap<nint>(out var Ꮡw);
    (w, var h, var cw, var ch) = yCbCrSize(r, subsampleRatio);
    // totalLength should be the same as i3, below, for a valid Rectangle r.
    nint totalLength = add2NonNeg(
        mul3NonNeg(2, w, h),
        mul3NonNeg(2, cw, ch));
    if (totalLength < 0) {
        throw panic("image: NewNYCbCrA Rectangle has huge or negative dimension");
    }
    nint i0 = 1 * w * h + 0 * cw * ch;
    nint i1 = 1 * w * h + 1 * cw * ch;
    nint i2 = 1 * w * h + 2 * cw * ch;
    nint i3 = 2 * w * h + 2 * cw * ch;
    var b = new slice<byte>(i3);
    return Ꮡ(new NYCbCrA(
        YCbCr: new YCbCr(
            Y: b.slice(0, i0, i0),
            Cb: b.slice(i0, i1, i1),
            Cr: b.slice(i1, i2, i2),
            SubsampleRatio: subsampleRatio,
            YStride: w,
            CStride: cw,
            Rect: r
        ),
        A: b.slice(i2),
        AStride: w
    ));
}

} // end image_package
