// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bufio = bufio_package;
using fmt = fmt_package;
using Δimage = image_package;
using color = go.image.color_package;
using os = os_package;
using testing = testing_package;
// blank import: go.image.gif_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.image.jpeg_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.image.png_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.image;
using static go.image_internal_test_package;
using Δio = io_package;

partial class image_test_package {

partial struct imageTest {
    internal @string goldenFilename;
    internal @string filename;
    internal nint tolerance;
}

// GIF images are restricted to a 256-color palette and the conversion
// to GIF loses significant image quality.
// JPEG is a lossy format and hence needs a non-zero tolerance.
// Grayscale images.
internal static slice<imageTest> imageTests = new imageTest[]{
    new("testdata/video-001.png"u8, "testdata/video-001.png"u8, 0),
    new("testdata/video-001.png"u8, "testdata/video-001.gif"u8, (64 << (int)(8))),
    new("testdata/video-001.png"u8, "testdata/video-001.interlaced.gif"u8, (64 << (int)(8))),
    new("testdata/video-001.png"u8, "testdata/video-001.5bpp.gif"u8, (128 << (int)(8))),
    new("testdata/video-001.png"u8, "testdata/video-001.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-001.png"u8, "testdata/video-001.progressive.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-001.221212.png"u8, "testdata/video-001.221212.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-001.cmyk.png"u8, "testdata/video-001.cmyk.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-001.rgb.png"u8, "testdata/video-001.rgb.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-001.progressive.truncated.png"u8, "testdata/video-001.progressive.truncated.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-005.gray.png"u8, "testdata/video-005.gray.jpeg"u8, (8 << (int)(8))),
    new("testdata/video-005.gray.png"u8, "testdata/video-005.gray.png"u8, 0)
}.slice();

internal static (Δimage.Image, @string, error) decode(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (default!, "", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return Δimage.Decode(new image_test_package.bufio_ReaderжReader(bufio.NewReader(new image_test_package.os_FileжReader(f))));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (Δimage.Config, @string, error) decodeConfig(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (new Δimage.Config(nil), "", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return Δimage.DecodeConfig(new image_test_package.bufio_ReaderжReader(bufio.NewReader(new image_test_package.os_FileжReader(f))));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static nint delta(uint32 u0, uint32 u1) {
    nint d = (nint)u0 - (nint)u1;
    if (d < 0) {
        return -d;
    }
    return d;
}

internal static bool withinTolerance(color.Color c0, color.Color c1, nint tolerance) {
    var (r0, g0, b0, a0) = c0.RGBA();
    var (r1, g1, b1, a1) = c1.RGBA();
    nint r = delta(r0, r1);
    nint g = delta(g0, g1);
    nint b = delta(b0, b1);
    nint a = delta(a0, a1);
    return r <= tolerance && g <= tolerance && b <= tolerance && a <= tolerance;
}

public static void TestDecode(ж<testing.T> Ꮡt) {
    @string rgba(color.Color c) {
        var (r, g, b, a) = c.RGBA();
        return fmt.Sprintf("rgba = 0x%04x, 0x%04x, 0x%04x, 0x%04x for %T%v"u8, r, g, b, a, c, c);
    }
    var golden = new map<@string, Δimage.Image>();
loop:
    foreach (var (_, it) in imageTests) {
        var g = golden[it.goldenFilename];
        if (g == default!) {
            error errΔ1 = default!;
            (g, _, errΔ1) = decode(it.goldenFilename);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("%s: %v"u8, it.goldenFilename, errΔ1);
                goto continue_loop;
            }
            golden[it.goldenFilename] = g;
        }
        var (m, imageFormat, err) = decode(it.filename);
        if (err != default!) {
            Ꮡt.Errorf("%s: %v"u8, it.filename, err);
            goto continue_loop;
        }
        var b = g.Bounds();
        if (!b.Eq(m.Bounds())) {
            Ꮡt.Errorf("%s: got bounds %v want %v"u8, it.filename, m.Bounds(), b);
            goto continue_loop;
        }
        for (nint y = b.Min.Y; y < b.Max.Y; y++) {
            for (nint x = b.Min.X; x < b.Max.X; x++) {
                if (!withinTolerance(g.At(x, y), m.At(x, y), it.tolerance)) {
                    Ꮡt.Errorf("%s: at (%d, %d):\ngot  %v\nwant %v"u8,
                        it.filename, x, y, rgba(m.At(x, y)), rgba(g.At(x, y)));
                    goto continue_loop;
                }
            }
        }
        if (imageFormat == "gif"u8) {
            // Each frame of a GIF can have a frame-local palette override the
            // GIF-global palette. Thus, image.Decode can yield a different ColorModel
            // than image.DecodeConfig.
            continue;
        }
        (var c, _, err) = decodeConfig(it.filename);
        if (err != default!) {
            Ꮡt.Errorf("%s: %v"u8, it.filename, err);
            goto continue_loop;
        }
        if (!AreEqual(m.ColorModel(), c.ColorModel)) {
            Ꮡt.Errorf("%s: color models differ"u8, it.filename);
            goto continue_loop;
        }
continue_loop:;
    }
break_loop:;
}

} // end image_test_package
