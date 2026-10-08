// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package color implements a basic color library.
namespace go.image;

partial class color_package {

// Color can convert itself to alpha-premultiplied 16-bits per channel RGBA.
// The conversion may be lossy.
partial interface Color {
    // RGBA returns the alpha-premultiplied red, green, blue and alpha values
    // for the color. Each value ranges within [0, 0xffff], but is represented
    // by a uint32 so that multiplying by a blend factor up to 0xffff will not
    // overflow.
    //
    // An alpha-premultiplied color component c has been scaled by alpha (a),
    // so has valid values 0 <= c <= a.
    (uint32 r, uint32 g, uint32 b, uint32 a) RGBA();
}

// RGBA represents a traditional 32-bit alpha-premultiplied color, having 8
// bits for each of red, green, blue and alpha.
//
// An alpha-premultiplied color component C has been scaled by alpha (A), so
// has valid values 0 <= C <= A.
partial struct ΔRGBA {
    public uint8 R, G, B, A;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this ΔRGBA c) {
    uint32 r = default!;
    uint32 g = default!;
    uint32 b = default!;
    uint32 a = default!;

    r = (uint32)c.R;
    r |= (uint32)((r << (int)(8)));
    g = (uint32)c.G;
    g |= (uint32)((g << (int)(8)));
    b = (uint32)c.B;
    b |= (uint32)((b << (int)(8)));
    a = (uint32)c.A;
    a |= (uint32)((a << (int)(8)));
    return (r, g, b, a);
}

// RGBA64 represents a 64-bit alpha-premultiplied color, having 16 bits for
// each of red, green, blue and alpha.
//
// An alpha-premultiplied color component C has been scaled by alpha (A), so
// has valid values 0 <= C <= A.
partial struct RGBA64 {
    public uint16 R, G, B, A;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this RGBA64 c) {
    return ((uint32)c.R, (uint32)c.G, (uint32)c.B, (uint32)c.A);
}

// NRGBA represents a non-alpha-premultiplied 32-bit color.
partial struct NRGBA {
    public uint8 R, G, B, A;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this NRGBA c) {
    uint32 r = default!;
    uint32 g = default!;
    uint32 b = default!;
    uint32 a = default!;

    r = (uint32)c.R;
    r |= (uint32)((r << (int)(8)));
    r *= (uint32)c.A;
    r /= 0xff;
    g = (uint32)c.G;
    g |= (uint32)((g << (int)(8)));
    g *= (uint32)c.A;
    g /= 0xff;
    b = (uint32)c.B;
    b |= (uint32)((b << (int)(8)));
    b *= (uint32)c.A;
    b /= 0xff;
    a = (uint32)c.A;
    a |= (uint32)((a << (int)(8)));
    return (r, g, b, a);
}

// NRGBA64 represents a non-alpha-premultiplied 64-bit color,
// having 16 bits for each of red, green, blue and alpha.
partial struct NRGBA64 {
    public uint16 R, G, B, A;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this NRGBA64 c) {
    uint32 r = default!;
    uint32 g = default!;
    uint32 b = default!;
    uint32 a = default!;

    r = (uint32)c.R;
    r *= (uint32)c.A;
    r /= 0xffff;
    g = (uint32)c.G;
    g *= (uint32)c.A;
    g /= 0xffff;
    b = (uint32)c.B;
    b *= (uint32)c.A;
    b /= 0xffff;
    a = (uint32)c.A;
    return (r, g, b, a);
}

// Alpha represents an 8-bit alpha color.
partial struct Alpha {
    public uint8 A;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this Alpha c) {
    uint32 a = default!;

    a = (uint32)c.A;
    a |= (uint32)((a << (int)(8)));
    return (a, a, a, a);
}

// Alpha16 represents a 16-bit alpha color.
partial struct Alpha16 {
    public uint16 A;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this Alpha16 c) {
    uint32 a = default!;

    a = (uint32)c.A;
    return (a, a, a, a);
}

// Gray represents an 8-bit grayscale color.
partial struct Gray {
    public uint8 Y;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this Gray c) {
    var y = (uint32)c.Y;
    y |= (uint32)((y << (int)(8)));
    return (y, y, y, 0xffff);
}

// Gray16 represents a 16-bit grayscale color.
partial struct Gray16 {
    public uint16 Y;
}

public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this Gray16 c) {
    var y = (uint32)c.Y;
    return (y, y, y, 0xffff);
}

// Model can convert any [Color] to one from its own color model. The conversion
// may be lossy.
partial interface Model {
    Color Convert(Color c);
}

// ModelFunc returns a [Model] that invokes f to implement the conversion.
public static Model ModelFunc(Func<Color, Color> f) {
    // Note: using *modelFunc as the implementation
    // means that callers can still use comparisons
    // like m == RGBAModel. This is not possible if
    // we use the func value directly, because funcs
    // are no longer comparable.
    return new modelFuncжModel(Ꮡ(new modelFunc(f)));
}

partial struct modelFunc {
    internal Func<Color, Color> f;
}

internal static Color Convert(this ref modelFunc m, Color c) {
    return m.f(c);
}

// Models for the standard color types.
public static Model RGBAModel = ModelFunc(rgbaModel);

public static Model RGBA64Model = ModelFunc(rgba64Model);

public static Model NRGBAModel = ModelFunc(nrgbaModel);

public static Model NRGBA64Model = ModelFunc(nrgba64Model);

public static Model AlphaModel = ModelFunc(alphaModel);

public static Model Alpha16Model = ModelFunc(alpha16Model);

public static Model GrayModel = ModelFunc(grayModel);

public static Model Gray16Model = ModelFunc(gray16Model);

internal static Color rgbaModel(Color c) {
    {
        var (_, ok) = c._<ΔRGBA>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    return new ΔRGBA((uint8)((r >> (int)(8))), (uint8)((g >> (int)(8))), (uint8)((b >> (int)(8))), (uint8)((a >> (int)(8))));
}

internal static Color rgba64Model(Color c) {
    {
        var (_, ok) = c._<RGBA64>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    return new RGBA64((uint16)r, (uint16)g, (uint16)b, (uint16)a);
}

internal static Color nrgbaModel(Color c) {
    {
        var (_, ok) = c._<NRGBA>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    if (a == 0xffff) {
        return new NRGBA((uint8)((r >> (int)(8))), (uint8)((g >> (int)(8))), (uint8)((b >> (int)(8))), 0xff);
    }
    if (a == 0) {
        return new NRGBA(0, 0, 0, 0);
    }
    // Since Color.RGBA returns an alpha-premultiplied color, we should have r <= a && g <= a && b <= a.
    r = (r * 0xffff) / a;
    g = (g * 0xffff) / a;
    b = (b * 0xffff) / a;
    return new NRGBA((uint8)((r >> (int)(8))), (uint8)((g >> (int)(8))), (uint8)((b >> (int)(8))), (uint8)((a >> (int)(8))));
}

internal static Color nrgba64Model(Color c) {
    {
        var (_, ok) = c._<NRGBA64>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, a) = c.RGBA();
    if (a == 0xffff) {
        return new NRGBA64((uint16)r, (uint16)g, (uint16)b, 0xffff);
    }
    if (a == 0) {
        return new NRGBA64(0, 0, 0, 0);
    }
    // Since Color.RGBA returns an alpha-premultiplied color, we should have r <= a && g <= a && b <= a.
    r = (r * 0xffff) / a;
    g = (g * 0xffff) / a;
    b = (b * 0xffff) / a;
    return new NRGBA64((uint16)r, (uint16)g, (uint16)b, (uint16)a);
}

internal static Color alphaModel(Color c) {
    {
        var (_, ok) = c._<Alpha>(ᐧ); if (ok) {
            return c;
        }
    }
    var (_, _, _, a) = c.RGBA();
    return new Alpha((uint8)((a >> (int)(8))));
}

internal static Color alpha16Model(Color c) {
    {
        var (_, ok) = c._<Alpha16>(ᐧ); if (ok) {
            return c;
        }
    }
    var (_, _, _, a) = c.RGBA();
    return new Alpha16((uint16)a);
}

internal static Color grayModel(Color c) {
    {
        var (_, ok) = c._<Gray>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, _) = c.RGBA();
    // These coefficients (the fractions 0.299, 0.587 and 0.114) are the same
    // as those given by the JFIF specification and used by func RGBToYCbCr in
    // ycbcr.go.
    //
    // Note that 19595 + 38470 + 7471 equals 65536.
    //
    // The 24 is 16 + 8. The 16 is the same as used in RGBToYCbCr. The 8 is
    // because the return value is 8 bit color, not 16 bit color.
    var y = ((19595 * r + 38470 * g + 7471 * b + ((uint32)1 << (int)(15))) >> (int)(24));
    return new Gray((uint8)y);
}

internal static Color gray16Model(Color c) {
    {
        var (_, ok) = c._<Gray16>(ᐧ); if (ok) {
            return c;
        }
    }
    var (r, g, b, _) = c.RGBA();
    // These coefficients (the fractions 0.299, 0.587 and 0.114) are the same
    // as those given by the JFIF specification and used by func RGBToYCbCr in
    // ycbcr.go.
    //
    // Note that 19595 + 38470 + 7471 equals 65536.
    var y = ((19595 * r + 38470 * g + 7471 * b + ((uint32)1 << (int)(15))) >> (int)(16));
    return new Gray16((uint16)y);
}

partial struct Palette /*[]Color*/;

// Convert returns the palette color closest to c in Euclidean R,G,B space.
public static Color Convert(this Palette p, Color c) {
    if (len(p) == 0) {
        return default!;
    }
    return p[p.Index(c)];
}

// Index returns the index of the palette color closest to c in Euclidean
// R,G,B,A space.
public static nint Index(this Palette p, Color c) {
    // A batch version of this computation is in image/draw/draw.go.
    var (cr, cg, cb, ca) = c.RGBA();
    nint ret = 0;
    var bestSum = unchecked((uint32)(4294967295UL));
    foreach (var (i, v) in p) {
        var (vr, vg, vb, va) = v.RGBA();
        var sum = sqDiff(cr, vr) + sqDiff(cg, vg) + sqDiff(cb, vb) + sqDiff(ca, va);
        if (sum < bestSum) {
            if (sum == 0) {
                return i;
            }
            (ret, bestSum) = (i, sum);
        }
    }
    return ret;
}

// sqDiff returns the squared-difference of x and y, shifted by 2 so that
// adding four of those won't overflow a uint32.
//
// x and y are both assumed to be in the range [0, 0xffff].
internal static uint32 sqDiff(uint32 x, uint32 y) {
    // The canonical code of this function looks as follows:
    //
    //	var d uint32
    //	if x > y {
    //		d = x - y
    //	} else {
    //		d = y - x
    //	}
    //	return (d * d) >> 2
    //
    // Language spec guarantees the following properties of unsigned integer
    // values operations with respect to overflow/wrap around:
    //
    // > For unsigned integer values, the operations +, -, *, and << are
    // > computed modulo 2n, where n is the bit width of the unsigned
    // > integer's type. Loosely speaking, these unsigned integer operations
    // > discard high bits upon overflow, and programs may rely on ``wrap
    // > around''.
    //
    // Considering these properties and the fact that this function is
    // called in the hot paths (x,y loops), it is reduced to the below code
    // which is slightly faster. See TestSqDiff for correctness check.
    var d = x - y;
    return ((d * d) >> (int)(2));
}

// Standard colors.
public static Gray16 Black = new Gray16(0);

public static Gray16 White = new Gray16(0xffff);

public static Alpha16 Transparent = new Alpha16(0);

public static Alpha16 Opaque = new Alpha16(0xffff);

} // end color_package
