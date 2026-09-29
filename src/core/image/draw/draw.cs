// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package draw provides image composition functions.
//
// See "The Go image/draw package" for an introduction to this package:
// https://golang.org/doc/articles/image_draw.html
namespace go.image;

using image = image_package;
using color = go.image.color_package;
using imageutil = go.image.@internal.imageutil_package;
using go.image;
using go.image.@internal;

partial class draw_package {

// m is the maximum color value returned by image.Color.RGBA.
internal static UntypedInt m => /* 1<<16 - 1 */ 65535;

// Image is an image.Image with a Set method to change a single pixel.
[GoType] partial interface Image :
    image.Image
{
    void Set(nint x, nint y, color.Color c);
}

// RGBA64Image extends both the [Image] and [image.RGBA64Image] interfaces with a
// SetRGBA64 method to change a single pixel. SetRGBA64 is equivalent to
// calling Set, but it can avoid allocations from converting concrete color
// types to the [color.Color] interface type.
[GoType] partial interface RGBA64Image :
    image.RGBA64Image
{
    void Set(nint x, nint y, color.Color c);
    void SetRGBA64(nint x, nint y, color.RGBA64 c);
}

// Quantizer produces a palette for an image.
[GoType] partial interface Quantizer {
    // Quantize appends up to cap(p) - len(p) colors to p and returns the
    // updated palette suitable for converting m to a paletted image.
    color.Palette Quantize(color.Palette p, image.Image m);
}

[GoType("num:nint")] partial struct Op;

public static Op Over => /* iota */ 0;
public static Op Src => 1;

// Draw implements the [Drawer] interface by calling the Draw function with this
// [Op].
public static void Draw(this Op op, Image dst, image.Rectangle r, image.Image src, image.Point sp) {
    DrawMask(dst, r, src, sp, default!, new image.Point(nil), op);
}

// Drawer contains the [Draw] method.
[GoType] partial interface Drawer {
    // Draw aligns r.Min in dst with sp in src and then replaces the
    // rectangle r in dst with the result of drawing src on dst.
    void Draw(Image dst, image.Rectangle r, image.Image src, image.Point sp);
}

// FloydSteinberg is a [Drawer] that is the [Src] [Op] with Floyd-Steinberg error
// diffusion.
public static Drawer FloydSteinberg = new floydSteinberg(nil);

[GoType] partial struct floydSteinberg {
}

internal static void Draw(this floydSteinberg _, Image dst, image.Rectangle rʗp, image.Image src, image.Point sp) {
    ref var r = ref heap(rʗp, out var Ꮡr);

    clip(dst, Ꮡr, src, ref sp, default!, nil);
    if (r.Empty()) {
        return;
    }
    drawPaletted(dst, r, src, sp, true);
}

// clip clips r against each image's bounds (after translating into the
// destination image's coordinate space) and shifts the points sp and mp by
// the same amount as the change in r.Min.
internal static void clip(Image dst, ж<image.Rectangle> Ꮡr, image.Image src, ref image.Point sp, image.Image mask, ж<image.Point> Ꮡmp) {
    ref var r = ref Ꮡr.DerefOrNull();
    ref var mp = ref Ꮡmp.DerefOrNull();

    var orig = r.Min;
    r = r.Intersect(dst.Bounds());
    r = r.Intersect(src.Bounds().Add(orig.Sub(sp)));
    if (mask != default!) {
        r = r.Intersect(mask.Bounds().Add(orig.Sub(mp)));
    }
    nint dx = r.Min.X - orig.X;
    nint dy = r.Min.Y - orig.Y;
    if (dx == 0 && dy == 0) {
        return;
    }
    sp.X += dx;
    sp.Y += dy;
    if (Ꮡmp != nil) {
        mp.X += dx;
        mp.Y += dy;
    }
}

internal static bool processBackward(image.Image dst, image.Rectangle r, image.Image src, image.Point sp) {
    return AreEqual(dst, src) && r.Overlaps(r.Add(sp.Sub(r.Min))) && (sp.Y < r.Min.Y || (sp.Y == r.Min.Y && sp.X < r.Min.X));
}

// Draw calls [DrawMask] with a nil mask.
public static void Draw(Image dst, image.Rectangle r, image.Image src, image.Point sp, Op op) {
    DrawMask(dst, r, src, sp, default!, new image.Point(nil), op);
}

// DrawMask aligns r.Min in dst with sp in src and mp in mask and then replaces the rectangle r
// in dst with the result of a Porter-Duff composition. A nil mask is treated as opaque.
public static void DrawMask(Image dst, image.Rectangle rʗp, image.Image src, image.Point sp, image.Image mask, image.Point mpʗp, Op op) {
    ref var r = ref heap(rʗp, out var Ꮡr);
    ref var mp = ref heap(mpʗp, out var Ꮡmp);

    clip(dst, Ꮡr, src, ref sp, mask, Ꮡmp);
    if (r.Empty()) {
        return;
    }
    // Fast paths for special cases. If none of them apply, then we fall back
    // to general but slower implementations.
    //
    // For NRGBA and NRGBA64 image types, the code paths aren't just faster.
    // They also avoid the information loss that would otherwise occur from
    // converting non-alpha-premultiplied color to and from alpha-premultiplied
    // color. See TestDrawSrcNonpremultiplied.
    switch (dst.type()) {
    case ж<imageꓸRGBA> dst0: {
        if (op == Over){
            if (mask == default!){
                switch (src.type()) {
                case ж<image.Uniform> src0: {
                    var (sr, sg, sb, sa) = src0.RGBA();
                    if (sa == 0xffff){
                        drawFillSrc(dst0, r, sr, sg, sb, sa);
                    } else {
                        drawFillOver(dst0, r, sr, sg, sb, sa);
                    }
                    return;
                }
                case ж<imageꓸRGBA> src0: {
                    drawCopyOver(dst0, r, src0, sp);
                    return;
                }
                case ж<image.NRGBA> src0: {
                    drawNRGBAOver(ref (dst0).DerefOrNull(), r, ref (src0).DerefOrNull(), sp);
                    return;
                }
                case ж<image.YCbCr> src0: {
                    if (imageutil.DrawYCbCr(dst0, // An image.YCbCr is always fully opaque, and so if the
 // mask is nil (i.e. fully opaque) then the op is
 // effectively always Src. Similarly for image.Gray and
 // image.CMYK.
 r, src0, sp)) {
                        return;
                    }
                    break;
                }
                case ж<image.Gray> src0: {
                    drawGray(ref (dst0).DerefOrNull(), r, ref (src0).DerefOrNull(), sp);
                    return;
                }
                case ж<image.CMYK> src0: {
                    drawCMYK(ref (dst0).DerefOrNull(), r, ref (src0).DerefOrNull(), sp);
                    return;
                }}
            } else 
            {
                var (mask0, ok) = mask._<ж<image.Alpha>>(ᐧ); if (ok) {
                    switch (src.type()) {
                    case ж<image.Uniform> src0: {
                        drawGlyphOver(dst0, r, src0, mask0, mp);
                        return;
                    }
                    case ж<imageꓸRGBA> src0: {
                        drawRGBAMaskOver(dst0, r, src0, sp, mask0, mp);
                        return;
                    }
                    case ж<image.Gray> src0: {
                        drawGrayMaskOver(dst0, r, src0, sp, mask0, mp);
                        return;
                    }
                    case {} Δsrc0 when Δsrc0._<image.RGBA64Image>(out var src0): {
                        drawRGBA64ImageMaskOver(dst0, // Case order matters. The next case (image.RGBA64Image) is an
 // interface type that the concrete types above also implement.
 r, src0, sp, mask0, mp);
                        return;
                    }}
                }
            }
        } else {
            if (mask == default!) {
                switch (src.type()) {
                case ж<image.Uniform> src0: {
                    var (sr, sg, sb, sa) = src0.RGBA();
                    drawFillSrc(dst0, r, sr, sg, sb, sa);
                    return;
                }
                case ж<imageꓸRGBA> src0: {
                    nint d0 = dst0.PixOffset(r.Min.X, r.Min.Y);
                    nint s0 = src0.PixOffset(sp.X, sp.Y);
                    drawCopySrc(
                        (~dst0).Pix.slice(d0), (~dst0).Stride, r, (~src0).Pix.slice(s0), (~src0).Stride, sp, 4 * r.Dx());
                    return;
                }
                case ж<image.NRGBA> src0: {
                    drawNRGBASrc(ref (dst0).DerefOrNull(), r, ref (src0).DerefOrNull(), sp);
                    return;
                }
                case ж<image.YCbCr> src0: {
                    if (imageutil.DrawYCbCr(dst0, r, src0, sp)) {
                        return;
                    }
                    break;
                }
                case ж<image.Gray> src0: {
                    drawGray(ref (dst0).DerefOrNull(), r, ref (src0).DerefOrNull(), sp);
                    return;
                }
                case ж<image.CMYK> src0: {
                    drawCMYK(ref (dst0).DerefOrNull(), r, ref (src0).DerefOrNull(), sp);
                    return;
                }}
            }
        }
        drawRGBA(dst0, r, src, sp, mask, mp, op);
        return;
    }
    case ж<image.Paletted> dst0: {
        if (op == Src && mask == default!) {
            {
                var (src0, ok) = src._<ж<image.Uniform>>(ᐧ); if (ok){
                    var colorIndex = (uint8)(~dst0).Palette.Index((~src0).C);
                    nint i0 = dst0.PixOffset(r.Min.X, r.Min.Y);
                    nint i1 = i0 + r.Dx();
                    for (nint i = i0; i < i1; i++) {
                        dst0.Value.Pix[i] = colorIndex;
                    }
                    var firstRow = (~dst0).Pix.slice(i0, i1);
                    for (nint y = r.Min.Y + 1; y < r.Max.Y; y++) {
                        i0 += dst0.Value.Stride;
                        i1 += dst0.Value.Stride;
                        copy((~dst0).Pix.slice(i0, i1), firstRow);
                    }
                    return;
                } else 
                if (!processBackward(dst, r, src, sp)) {
                    drawPaletted(new image_PalettedжImage(dst0), r, src, sp, false);
                    return;
                }
            }
        }
        break;
    }
    case ж<image.NRGBA> dst0: {
        if (op == Src && mask == default!) {
            {
                var (src0, ok) = src._<ж<image.NRGBA>>(ᐧ); if (ok) {
                    nint d0 = dst0.PixOffset(r.Min.X, r.Min.Y);
                    nint s0 = src0.PixOffset(sp.X, sp.Y);
                    drawCopySrc(
                        (~dst0).Pix.slice(d0), (~dst0).Stride, r, (~src0).Pix.slice(s0), (~src0).Stride, sp, 4 * r.Dx());
                    return;
                }
            }
        }
        break;
    }
    case ж<image.NRGBA64> dst0: {
        if (op == Src && mask == default!) {
            {
                var (src0, ok) = src._<ж<image.NRGBA64>>(ᐧ); if (ok) {
                    nint d0 = dst0.PixOffset(r.Min.X, r.Min.Y);
                    nint s0 = src0.PixOffset(sp.X, sp.Y);
                    drawCopySrc(
                        (~dst0).Pix.slice(d0), (~dst0).Stride, r, (~src0).Pix.slice(s0), (~src0).Stride, sp, 8 * r.Dx());
                    return;
                }
            }
        }
        break;
    }}
    nint x0 = r.Min.X;
    nint x1 = r.Max.X;
    nint dx = 1;
    nint y0 = r.Min.Y;
    nint y1 = r.Max.Y;
    nint dy = 1;
    if (processBackward(dst, r, src, sp)) {
        (x0, x1, dx) = (x1 - 1, x0 - 1, -1);
        (y0, y1, dy) = (y1 - 1, y0 - 1, -1);
    }
    // FALLBACK1.17
    //
    // Try the draw.RGBA64Image and image.RGBA64Image interfaces, part of the
    // standard library since Go 1.17. These are like the draw.Image and
    // image.Image interfaces but they can avoid allocations from converting
    // concrete color types to the color.Color interface type.
    {
        var (dst0, _) = dst._<RGBA64Image>(ᐧ); if (dst0 != default!) {
            {
                var (src0, _) = src._<image.RGBA64Image>(ᐧ); if (src0 != default!) {
                    if (mask == default!){
                        nint syΔ1 = sp.Y + y0 - r.Min.Y;
                        nint myΔ1 = mp.Y + y0 - r.Min.Y;
                        for (nint y = y0; y != y1; (y, syΔ1, myΔ1) = (y + dy, syΔ1 + dy, myΔ1 + dy)) {
                            nint sx = sp.X + x0 - r.Min.X;
                            nint mx = mp.X + x0 - r.Min.X;
                            for (nint x = x0; x != x1; (x, sx, mx) = (x + dx, sx + dx, mx + dx)) {
                                if (op == Src){
                                    dst0.SetRGBA64(x, y, src0.RGBA64At(sx, syΔ1));
                                } else {
                                    var srgba = src0.RGBA64At(sx, syΔ1);
                                    var a = (uint32)m - (uint32)srgba.A;
                                    var drgba = dst0.RGBA64At(x, y);
                                    dst0.SetRGBA64(x, y, new color.RGBA64(
                                        R: (uint16)((uint16)(((uint32)drgba.R * a) / (uint32)m) + srgba.R),
                                        G: (uint16)((uint16)(((uint32)drgba.G * a) / (uint32)m) + srgba.G),
                                        B: (uint16)((uint16)(((uint32)drgba.B * a) / (uint32)m) + srgba.B),
                                        A: (uint16)((uint16)(((uint32)drgba.A * a) / (uint32)m) + srgba.A)
                                    ));
                                }
                            }
                        }
                        return;
                    } else 
                    {
                        var (mask0, _) = mask._<image.RGBA64Image>(ᐧ); if (mask0 != default!) {
                            nint syΔ2 = sp.Y + y0 - r.Min.Y;
                            nint myΔ2 = mp.Y + y0 - r.Min.Y;
                            for (nint y = y0; y != y1; (y, syΔ2, myΔ2) = (y + dy, syΔ2 + dy, myΔ2 + dy)) {
                                nint sx = sp.X + x0 - r.Min.X;
                                nint mx = mp.X + x0 - r.Min.X;
                                for (nint x = x0; x != x1; (x, sx, mx) = (x + dx, sx + dx, mx + dx)) {
                                    var ma = (uint32)mask0.RGBA64At(mx, myΔ2).A;
                                    switch (ᐧ) {
                                    case {} when ma is 0: {
                                        if (op == Over){
                                        } else {
                                            // No-op.
                                            dst0.SetRGBA64(x, y, new color.RGBA64(nil));
                                        }
                                        break;
                                    }
                                    case {} when ma == m && op == Src: {
                                        dst0.SetRGBA64(x, y, src0.RGBA64At(sx, syΔ2));
                                        break;
                                    }
                                    default: {
                                        var srgba = src0.RGBA64At(sx, syΔ2);
                                        if (op == Over){
                                            var drgba = dst0.RGBA64At(x, y);
                                            var a = (uint32)m - ((uint32)srgba.A * ma / (uint32)m);
                                            dst0.SetRGBA64(x, y, new color.RGBA64(
                                                R: (uint16)(((uint32)drgba.R * a + (uint32)srgba.R * ma) / (uint32)m),
                                                G: (uint16)(((uint32)drgba.G * a + (uint32)srgba.G * ma) / (uint32)m),
                                                B: (uint16)(((uint32)drgba.B * a + (uint32)srgba.B * ma) / (uint32)m),
                                                A: (uint16)(((uint32)drgba.A * a + (uint32)srgba.A * ma) / (uint32)m)
                                            ));
                                        } else {
                                            dst0.SetRGBA64(x, y, new color.RGBA64(
                                                R: (uint16)((uint32)srgba.R * ma / (uint32)m),
                                                G: (uint16)((uint32)srgba.G * ma / (uint32)m),
                                                B: (uint16)((uint32)srgba.B * ma / (uint32)m),
                                                A: (uint16)((uint32)srgba.A * ma / (uint32)m)
                                            ));
                                        }
                                        break;
                                    }}

                                }
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
    // FALLBACK1.0
    //
    // If none of the faster code paths above apply, use the draw.Image and
    // image.Image interfaces, part of the standard library since Go 1.0.
    ref var @out = ref heap(new color.RGBA64(), out var Ꮡout);
    nint sy = sp.Y + y0 - r.Min.Y;
    nint my = mp.Y + y0 - r.Min.Y;
    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
        nint sx = sp.X + x0 - r.Min.X;
        nint mx = mp.X + x0 - r.Min.X;
        for (nint x = x0; x != x1; (x, sx, mx) = (x + dx, sx + dx, mx + dx)) {
            var ma = (uint32)m;
            if (mask != default!) {
                (_, _, _, ma) = mask.At(mx, my).RGBA();
            }
            switch (ᐧ) {
            case {} when ma is 0: {
                if (op == Over){
                } else {
                    // No-op.
                    dst.Set(x, y, color.Transparent);
                }
                break;
            }
            case {} when ma == m && op == Src: {
                dst.Set(x, y, src.At(sx, sy));
                break;
            }
            default: {
                var (sr, sg, sb, sa) = src.At(sx, sy).RGBA();
                if (op == Over){
                    var (dr, dg, db, da) = dst.At(x, y).RGBA();
                    var a = (uint32)m - (sa * ma / (uint32)m);
                    @out.R = (uint16)((dr * a + sr * ma) / (uint32)m);
                    @out.G = (uint16)((dg * a + sg * ma) / (uint32)m);
                    @out.B = (uint16)((db * a + sb * ma) / (uint32)m);
                    @out.A = (uint16)((da * a + sa * ma) / (uint32)m);
                } else {
                    @out.R = (uint16)(sr * ma / (uint32)m);
                    @out.G = (uint16)(sg * ma / (uint32)m);
                    @out.B = (uint16)(sb * ma / (uint32)m);
                    @out.A = (uint16)(sa * ma / (uint32)m);
                }
                dst.Set(x, // The third argument is &out instead of out (and out is
 // declared outside of the inner loop) to avoid the implicit
 // conversion to color.Color here allocating memory in the
 // inner loop if sizeof(color.RGBA64) > sizeof(uintptr).
 y, new color.RGBA64жColor(Ꮡout));
                break;
            }}

        }
    }
}

internal static void drawFillOver(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, uint32 sr, uint32 sg, uint32 sb, uint32 sa) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    // The 0x101 is here for the same reason as in drawRGBA.
    var a = ((uint32)m - sa) * 0x101;
    nint i0 = dst.PixOffset(r.Min.X, r.Min.Y);
    nint i1 = i0 + r.Dx() * 4;
    for (nint y = r.Min.Y; y != r.Max.Y; y++) {
        for (nint i = i0; i < i1; i += 4) {
            var dr = Ꮡ(dst.Pix, i + 0);
            var dg = Ꮡ(dst.Pix, i + 1);
            var db = Ꮡ(dst.Pix, i + 2);
            var da = Ꮡ(dst.Pix, i + 3);
            dr.Value = (uint8)((((uint32)(dr.Value) * a / (uint32)m + sr) >> (int)(8)));
            dg.Value = (uint8)((((uint32)(dg.Value) * a / (uint32)m + sg) >> (int)(8)));
            db.Value = (uint8)((((uint32)(db.Value) * a / (uint32)m + sb) >> (int)(8)));
            da.Value = (uint8)((((uint32)(da.Value) * a / (uint32)m + sa) >> (int)(8)));
        }
        i0 += dst.Stride;
        i1 += dst.Stride;
    }
}

internal static void drawFillSrc(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, uint32 sr, uint32 sg, uint32 sb, uint32 sa) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    var sr8 = (uint8)((sr >> (int)(8)));
    var sg8 = (uint8)((sg >> (int)(8)));
    var sb8 = (uint8)((sb >> (int)(8)));
    var sa8 = (uint8)((sa >> (int)(8)));
    // The built-in copy function is faster than a straightforward for loop to fill the destination with
    // the color, but copy requires a slice source. We therefore use a for loop to fill the first row, and
    // then use the first row as the slice source for the remaining rows.
    nint i0 = dst.PixOffset(r.Min.X, r.Min.Y);
    nint i1 = i0 + r.Dx() * 4;
    for (nint i = i0; i < i1; i += 4) {
        dst.Pix[i + 0] = sr8;
        dst.Pix[i + 1] = sg8;
        dst.Pix[i + 2] = sb8;
        dst.Pix[i + 3] = sa8;
    }
    var firstRow = dst.Pix.slice(i0, i1);
    for (nint y = r.Min.Y + 1; y < r.Max.Y; y++) {
        i0 += dst.Stride;
        i1 += dst.Stride;
        copy(dst.Pix.slice(i0, i1), firstRow);
    }
}

internal static void drawCopyOver(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, ж<imageꓸRGBA> Ꮡsrc, image.Point sp) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    nint dx = r.Dx();
    nint dy = r.Dy();
    nint d0 = dst.PixOffset(r.Min.X, r.Min.Y);
    nint s0 = src.PixOffset(sp.X, sp.Y);
    nint ddelta = default!;
    nint sdelta = default!;
    nint i0 = default!;
    nint i1 = default!;
    nint idelta = default!;
    if (r.Min.Y < sp.Y || r.Min.Y == sp.Y && r.Min.X <= sp.X){
        ddelta = dst.Stride;
        sdelta = src.Stride;
        (i0, i1, idelta) = (0, dx * 4, +4);
    } else {
        // If the source start point is higher than the destination start point, or equal height but to the left,
        // then we compose the rows in right-to-left, bottom-up order instead of left-to-right, top-down.
        d0 += (dy - 1) * dst.Stride;
        s0 += (dy - 1) * src.Stride;
        ddelta = -dst.Stride;
        sdelta = -src.Stride;
        (i0, i1, idelta) = ((dx - 1) * 4, -4, -4);
    }
    for (; dy > 0; dy--) {
        var dpix = dst.Pix.slice(d0);
        var spix = src.Pix.slice(s0);
        for (nint i = i0; i != i1; i += idelta) {
            var s = spix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var sr = (uint32)s[0] * 0x101;
            var sg = (uint32)s[1] * 0x101;
            var sb = (uint32)s[2] * 0x101;
            var sa = (uint32)s[3] * 0x101;
            // The 0x101 is here for the same reason as in drawRGBA.
            var a = ((uint32)m - sa) * 0x101;
            var d = dpix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            d[0] = (uint8)((((uint32)d[0] * a / (uint32)m + sr) >> (int)(8)));
            d[1] = (uint8)((((uint32)d[1] * a / (uint32)m + sg) >> (int)(8)));
            d[2] = (uint8)((((uint32)d[2] * a / (uint32)m + sb) >> (int)(8)));
            d[3] = (uint8)((((uint32)d[3] * a / (uint32)m + sa) >> (int)(8)));
        }
        d0 += ddelta;
        s0 += sdelta;
    }
}

// drawCopySrc copies bytes to dstPix from srcPix. These arguments roughly
// correspond to the Pix fields of the image package's concrete image.Image
// implementations, but are offset (dstPix is dst.Pix[dpOffset:] not dst.Pix).
internal static void drawCopySrc(slice<byte> dstPix, nint dstStride, image.Rectangle r, slice<byte> srcPix, nint srcStride, image.Point sp, nint bytesPerRow) {
    nint d0 = 0;
    nint s0 = 0;
    nint ddelta = dstStride;
    nint sdelta = srcStride;
    nint dy = r.Dy();
    if (r.Min.Y > sp.Y) {
        // If the source start point is higher than the destination start
        // point, then we compose the rows in bottom-up order instead of
        // top-down. Unlike the drawCopyOver function, we don't have to check
        // the x coordinates because the built-in copy function can handle
        // overlapping slices.
        d0 = (dy - 1) * dstStride;
        s0 = (dy - 1) * srcStride;
        ddelta = -dstStride;
        sdelta = -srcStride;
    }
    for (; dy > 0; dy--) {
        copy(dstPix.slice(d0, d0 + bytesPerRow), srcPix.slice(s0, s0 + bytesPerRow));
        d0 += ddelta;
        s0 += sdelta;
    }
}

internal static void drawNRGBAOver(ref imageꓸRGBA dst, image.Rectangle r, ref image.NRGBA src, image.Point sp) {
    nint i0 = (r.Min.X - dst.Rect.Min.X) * 4;
    nint i1 = (r.Max.X - dst.Rect.Min.X) * 4;
    nint si0 = (sp.X - src.Rect.Min.X) * 4;
    nint yMax = r.Max.Y - dst.Rect.Min.Y;
    nint y = r.Min.Y - dst.Rect.Min.Y;
    nint sy = sp.Y - src.Rect.Min.Y;
    for (; y != yMax; (y, sy) = (y + 1, sy + 1)) {
        var dpix = dst.Pix.slice(y * dst.Stride);
        var spix = src.Pix.slice(sy * src.Stride);
        for ((nint i, nint si) = (i0, si0); i < i1; (i, si) = (i + 4, si + 4)) {
            // Convert from non-premultiplied color to pre-multiplied color.
            var s = spix.slice(si, si + 4, si + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var sa = (uint32)s[3] * 0x101;
            var sr = (uint32)s[0] * sa / 0xff;
            var sg = (uint32)s[1] * sa / 0xff;
            var sb = (uint32)s[2] * sa / 0xff;
            var d = dpix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var dr = (uint32)d[0];
            var dg = (uint32)d[1];
            var db = (uint32)d[2];
            var da = (uint32)d[3];
            // The 0x101 is here for the same reason as in drawRGBA.
            var a = ((uint32)m - sa) * 0x101;
            d[0] = (uint8)(((dr * a / (uint32)m + sr) >> (int)(8)));
            d[1] = (uint8)(((dg * a / (uint32)m + sg) >> (int)(8)));
            d[2] = (uint8)(((db * a / (uint32)m + sb) >> (int)(8)));
            d[3] = (uint8)(((da * a / (uint32)m + sa) >> (int)(8)));
        }
    }
}

internal static void drawNRGBASrc(ref imageꓸRGBA dst, image.Rectangle r, ref image.NRGBA src, image.Point sp) {
    nint i0 = (r.Min.X - dst.Rect.Min.X) * 4;
    nint i1 = (r.Max.X - dst.Rect.Min.X) * 4;
    nint si0 = (sp.X - src.Rect.Min.X) * 4;
    nint yMax = r.Max.Y - dst.Rect.Min.Y;
    nint y = r.Min.Y - dst.Rect.Min.Y;
    nint sy = sp.Y - src.Rect.Min.Y;
    for (; y != yMax; (y, sy) = (y + 1, sy + 1)) {
        var dpix = dst.Pix.slice(y * dst.Stride);
        var spix = src.Pix.slice(sy * src.Stride);
        for ((nint i, nint si) = (i0, si0); i < i1; (i, si) = (i + 4, si + 4)) {
            // Convert from non-premultiplied color to pre-multiplied color.
            var s = spix.slice(si, si + 4, si + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var sa = (uint32)s[3] * 0x101;
            var sr = (uint32)s[0] * sa / 0xff;
            var sg = (uint32)s[1] * sa / 0xff;
            var sb = (uint32)s[2] * sa / 0xff;
            var d = dpix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            d[0] = (uint8)((sr >> (int)(8)));
            d[1] = (uint8)((sg >> (int)(8)));
            d[2] = (uint8)((sb >> (int)(8)));
            d[3] = (uint8)((sa >> (int)(8)));
        }
    }
}

internal static void drawGray(ref imageꓸRGBA dst, image.Rectangle r, ref image.Gray src, image.Point sp) {
    nint i0 = (r.Min.X - dst.Rect.Min.X) * 4;
    nint i1 = (r.Max.X - dst.Rect.Min.X) * 4;
    nint si0 = (sp.X - src.Rect.Min.X) * 1;
    nint yMax = r.Max.Y - dst.Rect.Min.Y;
    nint y = r.Min.Y - dst.Rect.Min.Y;
    nint sy = sp.Y - src.Rect.Min.Y;
    for (; y != yMax; (y, sy) = (y + 1, sy + 1)) {
        var dpix = dst.Pix.slice(y * dst.Stride);
        var spix = src.Pix.slice(sy * src.Stride);
        for ((nint i, nint si) = (i0, si0); i < i1; (i, si) = (i + 4, si + 1)) {
            var p = spix[si];
            var d = dpix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            d[0] = p;
            d[1] = p;
            d[2] = p;
            d[3] = 255;
        }
    }
}

internal static void drawCMYK(ref imageꓸRGBA dst, image.Rectangle r, ref image.CMYK src, image.Point sp) {
    nint i0 = (r.Min.X - dst.Rect.Min.X) * 4;
    nint i1 = (r.Max.X - dst.Rect.Min.X) * 4;
    nint si0 = (sp.X - src.Rect.Min.X) * 4;
    nint yMax = r.Max.Y - dst.Rect.Min.Y;
    nint y = r.Min.Y - dst.Rect.Min.Y;
    nint sy = sp.Y - src.Rect.Min.Y;
    for (; y != yMax; (y, sy) = (y + 1, sy + 1)) {
        var dpix = dst.Pix.slice(y * dst.Stride);
        var spix = src.Pix.slice(sy * src.Stride);
        for ((nint i, nint si) = (i0, si0); i < i1; (i, si) = (i + 4, si + 4)) {
            var s = spix.slice(si, si + 4, si + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var d = dpix.slice(i, i + 4, i + 4);
            (d[0], d[1], d[2]) = color.CMYKToRGB(s[0], s[1], s[2], s[3]);
            d[3] = 255;
        }
    }
}

internal static void drawGlyphOver(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, ж<image.Uniform> Ꮡsrc, ж<image.Alpha> Ꮡmask, image.Point mp) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();
    ref var mask = ref Ꮡmask.DerefOrNull();

    nint i0 = dst.PixOffset(r.Min.X, r.Min.Y);
    nint i1 = i0 + r.Dx() * 4;
    nint mi0 = mask.PixOffset(mp.X, mp.Y);
    var (sr, sg, sb, sa) = src.RGBA();
    for ((nint y, nint my) = (r.Min.Y, mp.Y); y != r.Max.Y; (y, my) = (y + 1, my + 1)) {
        for ((nint i, nint mi) = (i0, mi0); i < i1; (i, mi) = (i + 4, mi + 1)) {
            var ma = (uint32)mask.Pix[mi];
            if (ma == 0) {
                continue;
            }
            ma |= (uint32)((ma << (int)(8)));
            // The 0x101 is here for the same reason as in drawRGBA.
            var a = ((uint32)m - (sa * ma / (uint32)m)) * 0x101;
            var d = dst.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            d[0] = (uint8)((((uint32)d[0] * a + sr * ma) / (uint32)m >> (int)(8)));
            d[1] = (uint8)((((uint32)d[1] * a + sg * ma) / (uint32)m >> (int)(8)));
            d[2] = (uint8)((((uint32)d[2] * a + sb * ma) / (uint32)m >> (int)(8)));
            d[3] = (uint8)((((uint32)d[3] * a + sa * ma) / (uint32)m >> (int)(8)));
        }
        i0 += dst.Stride;
        i1 += dst.Stride;
        mi0 += mask.Stride;
    }
}

internal static void drawGrayMaskOver(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, ж<image.Gray> Ꮡsrc, image.Point sp, ж<image.Alpha> Ꮡmask, image.Point mp) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();
    ref var mask = ref Ꮡmask.DerefOrNull();

    nint x0 = r.Min.X;
    nint x1 = r.Max.X;
    nint dx = 1;
    nint y0 = r.Min.Y;
    nint y1 = r.Max.Y;
    nint dy = 1;
    if (r.Overlaps(r.Add(sp.Sub(r.Min)))) {
        if (sp.Y < r.Min.Y || sp.Y == r.Min.Y && sp.X < r.Min.X) {
            (x0, x1, dx) = (x1 - 1, x0 - 1, -1);
            (y0, y1, dy) = (y1 - 1, y0 - 1, -1);
        }
    }
    nint sy = sp.Y + y0 - r.Min.Y;
    nint my = mp.Y + y0 - r.Min.Y;
    nint sx0 = sp.X + x0 - r.Min.X;
    nint mx0 = mp.X + x0 - r.Min.X;
    nint sx1 = sx0 + (x1 - x0);
    nint i0 = dst.PixOffset(x0, y0);
    nint di = dx * 4;
    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
        for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
            nint mi = mask.PixOffset(mx, my);
            var ma = (uint32)mask.Pix[mi];
            ma |= (uint32)((ma << (int)(8)));
            nint si = src.PixOffset(sx, sy);
            var syΔ1 = (uint32)src.Pix[si];
            syΔ1 |= (uint32)((syΔ1 << (int)(8)));
            var sa = (uint32)0xffff;
            var d = dst.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var dr = (uint32)d[0];
            var dg = (uint32)d[1];
            var db = (uint32)d[2];
            var da = (uint32)d[3];
            // dr, dg, db and da are all 8-bit color at the moment, ranging in [0,255].
            // We work in 16-bit color, and so would normally do:
            // dr |= dr << 8
            // and similarly for dg, db and da, but instead we multiply a
            // (which is a 16-bit color, ranging in [0,65535]) by 0x101.
            // This yields the same result, but is fewer arithmetic operations.
            var a = ((uint32)m - (sa * ma / (uint32)m)) * 0x101;
            d[0] = (uint8)(((dr * a + syΔ1 * ma) / (uint32)m >> (int)(8)));
            d[1] = (uint8)(((dg * a + syΔ1 * ma) / (uint32)m >> (int)(8)));
            d[2] = (uint8)(((db * a + syΔ1 * ma) / (uint32)m >> (int)(8)));
            d[3] = (uint8)(((da * a + sa * ma) / (uint32)m >> (int)(8)));
        }
        i0 += dy * dst.Stride;
    }
}

internal static void drawRGBAMaskOver(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, ж<imageꓸRGBA> Ꮡsrc, image.Point sp, ж<image.Alpha> Ꮡmask, image.Point mp) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();
    ref var mask = ref Ꮡmask.DerefOrNull();

    nint x0 = r.Min.X;
    nint x1 = r.Max.X;
    nint dx = 1;
    nint y0 = r.Min.Y;
    nint y1 = r.Max.Y;
    nint dy = 1;
    if (Ꮡdst == Ꮡsrc && r.Overlaps(r.Add(sp.Sub(r.Min)))) {
        if (sp.Y < r.Min.Y || sp.Y == r.Min.Y && sp.X < r.Min.X) {
            (x0, x1, dx) = (x1 - 1, x0 - 1, -1);
            (y0, y1, dy) = (y1 - 1, y0 - 1, -1);
        }
    }
    nint sy = sp.Y + y0 - r.Min.Y;
    nint my = mp.Y + y0 - r.Min.Y;
    nint sx0 = sp.X + x0 - r.Min.X;
    nint mx0 = mp.X + x0 - r.Min.X;
    nint sx1 = sx0 + (x1 - x0);
    nint i0 = dst.PixOffset(x0, y0);
    nint di = dx * 4;
    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
        for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
            nint mi = mask.PixOffset(mx, my);
            var ma = (uint32)mask.Pix[mi];
            ma |= (uint32)((ma << (int)(8)));
            nint si = src.PixOffset(sx, sy);
            var sr = (uint32)src.Pix[si + 0];
            var sg = (uint32)src.Pix[si + 1];
            var sb = (uint32)src.Pix[si + 2];
            var sa = (uint32)src.Pix[si + 3];
            sr |= (uint32)((sr << (int)(8)));
            sg |= (uint32)((sg << (int)(8)));
            sb |= (uint32)((sb << (int)(8)));
            sa |= (uint32)((sa << (int)(8)));
            var d = dst.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var dr = (uint32)d[0];
            var dg = (uint32)d[1];
            var db = (uint32)d[2];
            var da = (uint32)d[3];
            // dr, dg, db and da are all 8-bit color at the moment, ranging in [0,255].
            // We work in 16-bit color, and so would normally do:
            // dr |= dr << 8
            // and similarly for dg, db and da, but instead we multiply a
            // (which is a 16-bit color, ranging in [0,65535]) by 0x101.
            // This yields the same result, but is fewer arithmetic operations.
            var a = ((uint32)m - (sa * ma / (uint32)m)) * 0x101;
            d[0] = (uint8)(((dr * a + sr * ma) / (uint32)m >> (int)(8)));
            d[1] = (uint8)(((dg * a + sg * ma) / (uint32)m >> (int)(8)));
            d[2] = (uint8)(((db * a + sb * ma) / (uint32)m >> (int)(8)));
            d[3] = (uint8)(((da * a + sa * ma) / (uint32)m >> (int)(8)));
        }
        i0 += dy * dst.Stride;
    }
}

internal static void drawRGBA64ImageMaskOver(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, image.RGBA64Image src, image.Point sp, ж<image.Alpha> Ꮡmask, image.Point mp) {
    ref var dst = ref Ꮡdst.DerefOrNull();
    ref var mask = ref Ꮡmask.DerefOrNull();

    nint x0 = r.Min.X;
    nint x1 = r.Max.X;
    nint dx = 1;
    nint y0 = r.Min.Y;
    nint y1 = r.Max.Y;
    nint dy = 1;
    if (AreEqual(((image.Image)new image.ΔRGBAжImage(Ꮡdst)), src) && r.Overlaps(r.Add(sp.Sub(r.Min)))) {
        if (sp.Y < r.Min.Y || sp.Y == r.Min.Y && sp.X < r.Min.X) {
            (x0, x1, dx) = (x1 - 1, x0 - 1, -1);
            (y0, y1, dy) = (y1 - 1, y0 - 1, -1);
        }
    }
    nint sy = sp.Y + y0 - r.Min.Y;
    nint my = mp.Y + y0 - r.Min.Y;
    nint sx0 = sp.X + x0 - r.Min.X;
    nint mx0 = mp.X + x0 - r.Min.X;
    nint sx1 = sx0 + (x1 - x0);
    nint i0 = dst.PixOffset(x0, y0);
    nint di = dx * 4;
    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
        for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
            nint mi = mask.PixOffset(mx, my);
            var ma = (uint32)mask.Pix[mi];
            ma |= (uint32)((ma << (int)(8)));
            var srgba = src.RGBA64At(sx, sy);
            var d = dst.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            var dr = (uint32)d[0];
            var dg = (uint32)d[1];
            var db = (uint32)d[2];
            var da = (uint32)d[3];
            // dr, dg, db and da are all 8-bit color at the moment, ranging in [0,255].
            // We work in 16-bit color, and so would normally do:
            // dr |= dr << 8
            // and similarly for dg, db and da, but instead we multiply a
            // (which is a 16-bit color, ranging in [0,65535]) by 0x101.
            // This yields the same result, but is fewer arithmetic operations.
            var a = ((uint32)m - ((uint32)srgba.A * ma / (uint32)m)) * 0x101;
            d[0] = (uint8)(((dr * a + (uint32)srgba.R * ma) / (uint32)m >> (int)(8)));
            d[1] = (uint8)(((dg * a + (uint32)srgba.G * ma) / (uint32)m >> (int)(8)));
            d[2] = (uint8)(((db * a + (uint32)srgba.B * ma) / (uint32)m >> (int)(8)));
            d[3] = (uint8)(((da * a + (uint32)srgba.A * ma) / (uint32)m >> (int)(8)));
        }
        i0 += dy * dst.Stride;
    }
}

internal static void drawRGBA(ж<imageꓸRGBA> Ꮡdst, image.Rectangle r, image.Image src, image.Point sp, image.Image mask, image.Point mp, Op op) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    nint x0 = r.Min.X;
    nint x1 = r.Max.X;
    nint dx = 1;
    nint y0 = r.Min.Y;
    nint y1 = r.Max.Y;
    nint dy = 1;
    if (AreEqual(((image.Image)new image.ΔRGBAжImage(Ꮡdst)), src) && r.Overlaps(r.Add(sp.Sub(r.Min)))) {
        if (sp.Y < r.Min.Y || sp.Y == r.Min.Y && sp.X < r.Min.X) {
            (x0, x1, dx) = (x1 - 1, x0 - 1, -1);
            (y0, y1, dy) = (y1 - 1, y0 - 1, -1);
        }
    }
    nint sy = sp.Y + y0 - r.Min.Y;
    nint my = mp.Y + y0 - r.Min.Y;
    nint sx0 = sp.X + x0 - r.Min.X;
    nint mx0 = mp.X + x0 - r.Min.X;
    nint sx1 = sx0 + (x1 - x0);
    nint i0 = dst.PixOffset(x0, y0);
    nint di = dx * 4;
    // Try the image.RGBA64Image interface, part of the standard library since
    // Go 1.17.
    //
    // This optimization is similar to how FALLBACK1.17 optimizes FALLBACK1.0
    // in DrawMask, except here the concrete type of dst is known to be
    // *image.RGBA.
    {
        var (src0, _) = src._<image.RGBA64Image>(ᐧ); if (src0 != default!) {
            if (mask == default!){
                if (op == Over){
                    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
                        for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
                            var srgba = src0.RGBA64At(sx, sy);
                            var d = dst.Pix.slice(i, i + 4, i + 4);
                            var dr = (uint32)d[0];
                            var dg = (uint32)d[1];
                            var db = (uint32)d[2];
                            var da = (uint32)d[3];
                            var a = ((uint32)m - (uint32)srgba.A) * 0x101;
                            d[0] = (uint8)(((dr * a / (uint32)m + (uint32)srgba.R) >> (int)(8)));
                            d[1] = (uint8)(((dg * a / (uint32)m + (uint32)srgba.G) >> (int)(8)));
                            d[2] = (uint8)(((db * a / (uint32)m + (uint32)srgba.B) >> (int)(8)));
                            d[3] = (uint8)(((da * a / (uint32)m + (uint32)srgba.A) >> (int)(8)));
                        }
                        i0 += dy * dst.Stride;
                    }
                } else {
                    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
                        for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
                            var srgba = src0.RGBA64At(sx, sy);
                            var d = dst.Pix.slice(i, i + 4, i + 4);
                            d[0] = (uint8)((srgba.R >> (int)(8)));
                            d[1] = (uint8)((srgba.G >> (int)(8)));
                            d[2] = (uint8)((srgba.B >> (int)(8)));
                            d[3] = (uint8)((srgba.A >> (int)(8)));
                        }
                        i0 += dy * dst.Stride;
                    }
                }
                return;
            } else 
            {
                var (mask0, _) = mask._<image.RGBA64Image>(ᐧ); if (mask0 != default!) {
                    if (op == Over){
                        for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
                            for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
                                var ma = (uint32)mask0.RGBA64At(mx, my).A;
                                var srgba = src0.RGBA64At(sx, sy);
                                var d = dst.Pix.slice(i, i + 4, i + 4);
                                var dr = (uint32)d[0];
                                var dg = (uint32)d[1];
                                var db = (uint32)d[2];
                                var da = (uint32)d[3];
                                var a = ((uint32)m - ((uint32)srgba.A * ma / (uint32)m)) * 0x101;
                                d[0] = (uint8)(((dr * a + (uint32)srgba.R * ma) / (uint32)m >> (int)(8)));
                                d[1] = (uint8)(((dg * a + (uint32)srgba.G * ma) / (uint32)m >> (int)(8)));
                                d[2] = (uint8)(((db * a + (uint32)srgba.B * ma) / (uint32)m >> (int)(8)));
                                d[3] = (uint8)(((da * a + (uint32)srgba.A * ma) / (uint32)m >> (int)(8)));
                            }
                            i0 += dy * dst.Stride;
                        }
                    } else {
                        for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
                            for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
                                var ma = (uint32)mask0.RGBA64At(mx, my).A;
                                var srgba = src0.RGBA64At(sx, sy);
                                var d = dst.Pix.slice(i, i + 4, i + 4);
                                d[0] = (uint8)(((uint32)srgba.R * ma / (uint32)m >> (int)(8)));
                                d[1] = (uint8)(((uint32)srgba.G * ma / (uint32)m >> (int)(8)));
                                d[2] = (uint8)(((uint32)srgba.B * ma / (uint32)m >> (int)(8)));
                                d[3] = (uint8)(((uint32)srgba.A * ma / (uint32)m >> (int)(8)));
                            }
                            i0 += dy * dst.Stride;
                        }
                    }
                    return;
                }
            }
        }
    }
    // Use the image.Image interface, part of the standard library since Go
    // 1.0.
    //
    // This is similar to FALLBACK1.0 in DrawMask, except here the concrete
    // type of dst is known to be *image.RGBA.
    for (nint y = y0; y != y1; (y, sy, my) = (y + dy, sy + dy, my + dy)) {
        for ((nint i, nint sx, nint mx) = (i0, sx0, mx0); sx != sx1; (i, sx, mx) = (i + di, sx + dx, mx + dx)) {
            var ma = (uint32)m;
            if (mask != default!) {
                (_, _, _, ma) = mask.At(mx, my).RGBA();
            }
            var (sr, sg, sb, sa) = src.At(sx, sy).RGBA();
            var d = dst.Pix.slice(i, i + 4, i + 4); // Small cap improves performance, see https://golang.org/issue/27857
            if (op == Over){
                var dr = (uint32)d[0];
                var dg = (uint32)d[1];
                var db = (uint32)d[2];
                var da = (uint32)d[3];
                // dr, dg, db and da are all 8-bit color at the moment, ranging in [0,255].
                // We work in 16-bit color, and so would normally do:
                // dr |= dr << 8
                // and similarly for dg, db and da, but instead we multiply a
                // (which is a 16-bit color, ranging in [0,65535]) by 0x101.
                // This yields the same result, but is fewer arithmetic operations.
                var a = ((uint32)m - (sa * ma / (uint32)m)) * 0x101;
                d[0] = (uint8)(((dr * a + sr * ma) / (uint32)m >> (int)(8)));
                d[1] = (uint8)(((dg * a + sg * ma) / (uint32)m >> (int)(8)));
                d[2] = (uint8)(((db * a + sb * ma) / (uint32)m >> (int)(8)));
                d[3] = (uint8)(((da * a + sa * ma) / (uint32)m >> (int)(8)));
            } else {
                d[0] = (uint8)((sr * ma / (uint32)m >> (int)(8)));
                d[1] = (uint8)((sg * ma / (uint32)m >> (int)(8)));
                d[2] = (uint8)((sb * ma / (uint32)m >> (int)(8)));
                d[3] = (uint8)((sa * ma / (uint32)m >> (int)(8)));
            }
        }
        i0 += dy * dst.Stride;
    }
}

// clamp clamps i to the interval [0, 0xffff].
internal static int32 clamp(int32 i) {
    if (i < 0) {
        return 0;
    }
    if (i > 0xffff) {
        return 0xffff;
    }
    return i;
}

// sqDiff returns the squared-difference of x and y, shifted by 2 so that
// adding four of those won't overflow a uint32.
//
// x and y are both assumed to be in the range [0, 0xffff].
internal static uint32 sqDiff(int32 x, int32 y) {
    // This is an optimized code relying on the overflow/wrap around
    // properties of unsigned integers operations guaranteed by the language
    // spec. See sqDiff from the image/color package for more details.
    var d = (uint32)(x - y);
    return ((d * d) >> (int)(2));
}

internal static void drawPaletted(Image dst, image.Rectangle r, image.Image src, image.Point sp, bool floydSteinberg) {
    // TODO(nigeltao): handle the case where the dst and src overlap.
    // Does it even make sense to try and do Floyd-Steinberg whilst
    // walking the image backward (right-to-left bottom-to-top)?
    // If dst is an *image.Paletted, we have a fast path for dst.Set and
    // dst.At. The dst.Set equivalent is a batch version of the algorithm
    // used by color.Palette's Index method in image/color/color.go, plus
    // optional Floyd-Steinberg error diffusion.
    var palette = slice<array<int32>>(default!);
    var pix = slice<byte>(default!);
    nint stride = 0;
    {
        var (p, ok) = dst._<ж<image.Paletted>>(ᐧ); if (ok) {
            palette = GoReflect.WithElemDims(new slice<array<int32>>(len((~p).Palette), () => new(4)), 4);
            foreach (var (i, col) in (~p).Palette) {
                var (rΔ1, g, b, a) = col.RGBA();
                palette[i][0] = (int32)rΔ1;
                palette[i][1] = (int32)g;
                palette[i][2] = (int32)b;
                palette[i][3] = (int32)a;
            }
            (pix, stride) = ((~p).Pix.slice(p.PixOffset(r.Min.X, r.Min.Y)), p.Value.Stride);
        }
    }
    // quantErrorCurr and quantErrorNext are the Floyd-Steinberg quantization
    // errors that have been propagated to the pixels in the current and next
    // rows. The +2 simplifies calculation near the edges.
    slice<array<int32>> quantErrorCurr = default!;
    slice<array<int32>> quantErrorNext = default!;
    if (floydSteinberg) {
        quantErrorCurr = GoReflect.WithElemDims(new slice<array<int32>>(r.Dx() + 2, () => new(4)), 4);
        quantErrorNext = GoReflect.WithElemDims(new slice<array<int32>>(r.Dx() + 2, () => new(4)), 4);
    }
    var pxRGBA = (nint x, nint y) => {
        return src.At(x, y).RGBA();
    };
    // Fast paths for special cases to avoid excessive use of the color.Color
    // interface which escapes to the heap but need to be discovered for
    // each pixel on r. See also https://golang.org/issues/15759.
    switch (src.type()) {
    case ж<imageꓸRGBA> src0: {
        pxRGBA = (nint x, nint y) => {
            return src0.RGBAAt(x, y).RGBA();
        };
        break;
    }
    case ж<image.NRGBA> src0: {
        pxRGBA = (nint x, nint y) => {
            return src0.NRGBAAt(x, y).RGBA();
        };
        break;
    }
    case ж<image.YCbCr> src0: {
        pxRGBA = (nint x, nint y) => {
            return src0.YCbCrAt(x, y).RGBA();
        };
        break;
    }}
    // Loop over each source pixel.
    ref var @out = ref heap<color.RGBA64>(out var Ꮡout);
    @out = new color.RGBA64(A: 0xffff);
    for (nint y = 0; y != r.Dy(); y++) {
        for (nint x = 0; x != r.Dx(); x++) {
            // er, eg and eb are the pixel's R,G,B values plus the
            // optional Floyd-Steinberg error.
            var (sr, sg, sb, sa) = pxRGBA(sp.X + x, sp.Y + y);
            var (er, eg, eb, ea) = ((int32)sr, (int32)sg, (int32)sb, (int32)sa);
            if (floydSteinberg) {
                er = clamp(er + quantErrorCurr[x + 1][0] / 16);
                eg = clamp(eg + quantErrorCurr[x + 1][1] / 16);
                eb = clamp(eb + quantErrorCurr[x + 1][2] / 16);
                ea = clamp(ea + quantErrorCurr[x + 1][3] / 16);
            }
            if (palette != default!){
                // Find the closest palette color in Euclidean R,G,B,A space:
                // the one that minimizes sum-squared-difference.
                // TODO(nigeltao): consider smarter algorithms.
                nint bestIndex = 0;
                var bestSum = unchecked((uint32)(4294967295UL));
                foreach (var (index, vᴛ1) in palette) {
                    var p = vᴛ1.Clone();

                    var sum = sqDiff(er, p[0]) + sqDiff(eg, p[1]) + sqDiff(eb, p[2]) + sqDiff(ea, p[3]);
                    if (sum < bestSum) {
                        (bestIndex, bestSum) = (index, sum);
                        if (sum == 0) {
                            break;
                        }
                    }
                }
                pix[y * stride + x] = (byte)bestIndex;
                if (!floydSteinberg) {
                    continue;
                }
                er -= palette[bestIndex][0];
                eg -= palette[bestIndex][1];
                eb -= palette[bestIndex][2];
                ea -= palette[bestIndex][3];
            } else {
                @out.R = (uint16)er;
                @out.G = (uint16)eg;
                @out.B = (uint16)eb;
                @out.A = (uint16)ea;
                // The third argument is &out instead of out (and out is
                // declared outside of the inner loop) to avoid the implicit
                // conversion to color.Color here allocating memory in the
                // inner loop if sizeof(color.RGBA64) > sizeof(uintptr).
                dst.Set(r.Min.X + x, r.Min.Y + y, new color.RGBA64жColor(Ꮡout));
                if (!floydSteinberg) {
                    continue;
                }
                (sr, sg, sb, sa) = dst.At(r.Min.X + x, r.Min.Y + y).RGBA();
                er -= (int32)sr;
                eg -= (int32)sg;
                eb -= (int32)sb;
                ea -= (int32)sa;
            }
            // Propagate the Floyd-Steinberg quantization error.
            quantErrorNext[x + 0][0] += er * 3;
            quantErrorNext[x + 0][1] += eg * 3;
            quantErrorNext[x + 0][2] += eb * 3;
            quantErrorNext[x + 0][3] += ea * 3;
            quantErrorNext[x + 1][0] += er * 5;
            quantErrorNext[x + 1][1] += eg * 5;
            quantErrorNext[x + 1][2] += eb * 5;
            quantErrorNext[x + 1][3] += ea * 5;
            quantErrorNext[x + 2][0] += er * 1;
            quantErrorNext[x + 2][1] += eg * 1;
            quantErrorNext[x + 2][2] += eb * 1;
            quantErrorNext[x + 2][3] += ea * 1;
            quantErrorCurr[x + 2][0] += er * 7;
            quantErrorCurr[x + 2][1] += eg * 7;
            quantErrorCurr[x + 2][2] += eb * 7;
            quantErrorCurr[x + 2][3] += ea * 7;
        }
        // Recycle the quantization error buffers.
        if (floydSteinberg) {
            (quantErrorCurr, quantErrorNext) = (quantErrorNext, quantErrorCurr);
            clear(quantErrorNext);
        }
    }
}

} // end draw_package
