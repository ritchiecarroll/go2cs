// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using fmt = fmt_package;
using testing = testing_package;
using static go.image.color_package;

partial class color_internal_test_package {

internal static uint8 delta(uint8 x, uint8 y) {
    if (x >= y) {
        return (uint8)(x - y);
    }
    return (uint8)(y - x);
}

internal static error eq(global::go.image.color_package.Color c0, global::go.image.color_package.Color c1) {
    var (r0, g0, b0, a0) = c0.RGBA();
    var (r1, g1, b1, a1) = c1.RGBA();
    if (r0 != r1 || g0 != g1 || b0 != b1 || a0 != a1) {
        return fmt.Errorf("got  0x%04x 0x%04x 0x%04x 0x%04x\nwant 0x%04x 0x%04x 0x%04x 0x%04x"u8,
            r0, g0, b0, a0, r1, g1, b1, a1);
    }
    return default!;
}

// TestYCbCrRoundtrip tests that a subset of RGB space can be converted to YCbCr
// and back to within 2/256 tolerance.
public static void TestYCbCrRoundtrip(ж<testing.T> Ꮡt) {
    for (nint r = 0; r < 256; r += 7) {
        for (nint g = 0; g < 256; g += 5) {
            for (nint b = 0; b < 256; b += 3) {
                var (r0, g0, b0) = ((uint8)r, (uint8)g, (uint8)b);
                var (y, cb, cr) = RGBToYCbCr(r0, g0, b0);
                var (r1, g1, b1) = YCbCrToRGB(y, cb, cr);
                if (delta(r0, r1) > 2 || delta(g0, g1) > 2 || delta(b0, b1) > 2) {
                    Ꮡt.Fatalf("\nr0, g0, b0 = %d, %d, %d\ny,  cb, cr = %d, %d, %d\nr1, g1, b1 = %d, %d, %d"u8,
                        r0, g0, b0, y, cb, cr, r1, g1, b1);
                }
            }
        }
    }
}

// TestYCbCrToRGBConsistency tests that calling the RGBA method (16 bit color)
// then truncating to 8 bits is equivalent to calling the YCbCrToRGB function (8
// bit color).
public static void TestYCbCrToRGBConsistency(ж<testing.T> Ꮡt) {
    for (nint y = 0; y < 256; y += 7) {
        for (nint cb = 0; cb < 256; cb += 5) {
            for (nint cr = 0; cr < 256; cr += 3) {
                var x = new YCbCr((uint8)y, (uint8)cb, (uint8)cr);
                var (r0, g0, b0, _) = x.RGBA();
                var (r1, g1, b1) = ((uint8)(r0 >> 8), (uint8)(g0 >> 8), (uint8)(b0 >> 8));
                var (r2, g2, b2) = YCbCrToRGB(x.Y, x.Cb, x.Cr);
                if (r1 != r2 || g1 != g2 || b1 != b2) {
                    Ꮡt.Fatalf("y, cb, cr = %d, %d, %d\nr1, g1, b1 = %d, %d, %d\nr2, g2, b2 = %d, %d, %d"u8,
                        y, cb, cr, r1, g1, b1, r2, g2, b2);
                }
            }
        }
    }
}

// TestYCbCrGray tests that YCbCr colors are a superset of Gray colors.
public static void TestYCbCrGray(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        var c0 = new YCbCr((uint8)i, 0x80, 0x80);
        var c1 = new Gray((uint8)i);
        {
            var err = eq(c0, c1); if (err != default!) {
                Ꮡt.Errorf("i=0x%02x:\n%v"u8, i, err);
            }
        }
    }
}

// TestNYCbCrAAlpha tests that NYCbCrA colors are a superset of Alpha colors.
public static void TestNYCbCrAAlpha(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        var c0 = new NYCbCrA(new YCbCr(0xff, 0x80, 0x80), (uint8)i);
        var c1 = new Alpha((uint8)i);
        {
            var err = eq(c0, c1); if (err != default!) {
                Ꮡt.Errorf("i=0x%02x:\n%v"u8, i, err);
            }
        }
    }
}

// TestNYCbCrAYCbCr tests that NYCbCrA colors are a superset of YCbCr colors.
public static void TestNYCbCrAYCbCr(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        var c0 = new NYCbCrA(new YCbCr((uint8)i, 0x40, 0xc0), 0xff);
        var c1 = new YCbCr((uint8)i, 0x40, 0xc0);
        {
            var err = eq(c0, c1); if (err != default!) {
                Ꮡt.Errorf("i=0x%02x:\n%v"u8, i, err);
            }
        }
    }
}

// TestCMYKRoundtrip tests that a subset of RGB space can be converted to CMYK
// and back to within 1/256 tolerance.
public static void TestCMYKRoundtrip(ж<testing.T> Ꮡt) {
    for (nint r = 0; r < 256; r += 7) {
        for (nint g = 0; g < 256; g += 5) {
            for (nint b = 0; b < 256; b += 3) {
                var (r0, g0, b0) = ((uint8)r, (uint8)g, (uint8)b);
                var (c, m, y, k) = RGBToCMYK(r0, g0, b0);
                var (r1, g1, b1) = CMYKToRGB(c, m, y, k);
                if (delta(r0, r1) > 1 || delta(g0, g1) > 1 || delta(b0, b1) > 1) {
                    Ꮡt.Fatalf("\nr0, g0, b0 = %d, %d, %d\nc, m, y, k = %d, %d, %d, %d\nr1, g1, b1 = %d, %d, %d"u8,
                        r0, g0, b0, c, m, y, k, r1, g1, b1);
                }
            }
        }
    }
}

// TestCMYKToRGBConsistency tests that calling the RGBA method (16 bit color)
// then truncating to 8 bits is equivalent to calling the CMYKToRGB function (8
// bit color).
public static void TestCMYKToRGBConsistency(ж<testing.T> Ꮡt) {
    for (nint c = 0; c < 256; c += 7) {
        for (nint m = 0; m < 256; m += 5) {
            for (nint y = 0; y < 256; y += 3) {
                for (nint k = 0; k < 256; k += 11) {
                    var x = new CMYK((uint8)c, (uint8)m, (uint8)y, (uint8)k);
                    var (r0, g0, b0, _) = x.RGBA();
                    var (r1, g1, b1) = ((uint8)(r0 >> 8), (uint8)(g0 >> 8), (uint8)(b0 >> 8));
                    var (r2, g2, b2) = CMYKToRGB(x.C, x.M, x.Y, x.K);
                    if (r1 != r2 || g1 != g2 || b1 != b2) {
                        Ꮡt.Fatalf("c, m, y, k = %d, %d, %d, %d\nr1, g1, b1 = %d, %d, %d\nr2, g2, b2 = %d, %d, %d"u8,
                            c, m, y, k, r1, g1, b1, r2, g2, b2);
                    }
                }
            }
        }
    }
}

// TestCMYKGray tests that CMYK colors are a superset of Gray colors.
public static void TestCMYKGray(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        {
            var err = eq(new CMYK(0x00, 0x00, 0x00, (uint8)(255 - i)), new Gray((uint8)i)); if (err != default!) {
                Ꮡt.Errorf("i=0x%02x:\n%v"u8, i, err);
            }
        }
    }
}

public static void TestPalette(ж<testing.T> Ꮡt) {
    var p = new Palette(new global::go.image.color_package.Color[]{new ΔRGBA(0xff, 0xff, 0xff, 0xff), new ΔRGBA(0x80, 0x00, 0x00, 0xff), new ΔRGBA(0x7f, 0x00, 0x00, 0x7f), new ΔRGBA(0x00, 0x00, 0x00, 0x7f), new ΔRGBA(0x00, 0x00, 0x00, 0x00), new ΔRGBA(0x40, 0x40, 0x40, 0x40)
    }.slice());
    // Check that, for a Palette with no repeated colors, the closest color to
    // each element is itself.
    foreach (var (i, c) in p) {
        nint j = p.Index(c);
        if (i != j) {
            Ꮡt.Errorf("Index(%v): got %d (color = %v), want %d"u8, c, j, p[j], i);
        }
    }
    // Check that finding the closest color considers alpha, not just red,
    // green and blue.
    var got = p.Convert(new ΔRGBA(0x80, 0x00, 0x00, 0x80));
    var want = new ΔRGBA(0x7f, 0x00, 0x00, 0x7f);
    if (!AreEqual(got, want)) {
        Ꮡt.Errorf("got %v, want %v"u8, got, want);
    }
}

internal static uint8 sink8;

internal static uint32 sink32;

public static void BenchmarkYCbCrToRGB(ж<testing.B> Ꮡb) {
    // YCbCrToRGB does saturating arithmetic.
    // Low, middle, and high values can take
    // different paths through the generated code.
    Ꮡb.Run("0"u8, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            (sink8, sink8, sink8) = YCbCrToRGB(0, 0, 0);
        }
    });
    Ꮡb.Run("128"u8, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            (sink8, sink8, sink8) = YCbCrToRGB(128, 128, 128);
        }
    });
    Ꮡb.Run("255"u8, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            (sink8, sink8, sink8) = YCbCrToRGB(255, 255, 255);
        }
    });
}

public static void BenchmarkRGBToYCbCr(ж<testing.B> Ꮡb) {
    // RGBToYCbCr does saturating arithmetic.
    // Different values can take different paths
    // through the generated code.
    Ꮡb.Run("0"u8, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            (sink8, sink8, sink8) = RGBToYCbCr(0, 0, 0);
        }
    });
    Ꮡb.Run("Cb"u8, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            (sink8, sink8, sink8) = RGBToYCbCr(0, 0, 255);
        }
    });
    Ꮡb.Run("Cr"u8, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            (sink8, sink8, sink8) = RGBToYCbCr(255, 0, 0);
        }
    });
}

public static void BenchmarkYCbCrToRGBA(ж<testing.B> Ꮡb) {
    // RGB does saturating arithmetic.
    // Low, middle, and high values can take
    // different paths through the generated code.
    Ꮡb.Run("0"u8, (ж<testing.B> bΔ1) => {
        var c = new YCbCr(0, 0, 0);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            (sink32, sink32, sink32, sink32) = c.RGBA();
        }
    });
    Ꮡb.Run("128"u8, (ж<testing.B> bΔ2) => {
        var c = new YCbCr(128, 128, 128);
        for (nint i = 0; i < (~bΔ2).N; i++) {
            (sink32, sink32, sink32, sink32) = c.RGBA();
        }
    });
    Ꮡb.Run("255"u8, (ж<testing.B> bΔ3) => {
        var c = new YCbCr(255, 255, 255);
        for (nint i = 0; i < (~bΔ3).N; i++) {
            (sink32, sink32, sink32, sink32) = c.RGBA();
        }
    });
}

public static void BenchmarkNYCbCrAToRGBA(ж<testing.B> Ꮡb) {
    // RGBA does saturating arithmetic.
    // Low, middle, and high values can take
    // different paths through the generated code.
    Ꮡb.Run("0"u8, (ж<testing.B> bΔ1) => {
        var c = new NYCbCrA(new YCbCr(0, 0, 0), 0xff);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            (sink32, sink32, sink32, sink32) = c.RGBA();
        }
    });
    Ꮡb.Run("128"u8, (ж<testing.B> bΔ2) => {
        var c = new NYCbCrA(new YCbCr(128, 128, 128), 0xff);
        for (nint i = 0; i < (~bΔ2).N; i++) {
            (sink32, sink32, sink32, sink32) = c.RGBA();
        }
    });
    Ꮡb.Run("255"u8, (ж<testing.B> bΔ3) => {
        var c = new NYCbCrA(new YCbCr(255, 255, 255), 0xff);
        for (nint i = 0; i < (~bΔ3).N; i++) {
            (sink32, sink32, sink32, sink32) = c.RGBA();
        }
    });
}

} // end color_internal_test_package
