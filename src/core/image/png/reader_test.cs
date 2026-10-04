// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using image = image_package;
using color = go.image.color_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.image;
using static go.image.png_package;

partial class png_internal_test_package {

internal static slice<@string> filenames = new @string[]{
    "basn0g01"u8,
    "basn0g01-30"u8,
    "basn0g02"u8,
    "basn0g02-29"u8,
    "basn0g04"u8,
    "basn0g04-31"u8,
    "basn0g08"u8,
    "basn0g16"u8,
    "basn2c08"u8,
    "basn2c16"u8,
    "basn3p01"u8,
    "basn3p02"u8,
    "basn3p04"u8,
    "basn3p04-31i"u8,
    "basn3p08"u8,
    "basn3p08-trns"u8,
    "basn4a08"u8,
    "basn4a16"u8,
    "basn6a08"u8,
    "basn6a16"u8,
    "ftbbn0g01"u8,
    "ftbbn0g02"u8,
    "ftbbn0g04"u8,
    "ftbbn2c16"u8,
    "ftbbn3p08"u8,
    "ftbgn2c16"u8,
    "ftbgn3p08"u8,
    "ftbrn2c08"u8,
    "ftbwn0g16"u8,
    "ftbwn3p08"u8,
    "ftbyn3p08"u8,
    "ftp0n0g08"u8,
    "ftp0n2c08"u8,
    "ftp0n3p08"u8,
    "ftp1n3p08"u8
}.slice();

internal static slice<@string> filenamesPaletted = new @string[]{
    "basn3p01"u8,
    "basn3p02"u8,
    "basn3p04"u8,
    "basn3p08"u8,
    "basn3p08-trns"u8
}.slice();

internal static slice<@string> filenamesShort = new @string[]{
    "basn0g01"u8,
    "basn0g04-31"u8,
    "basn6a16"u8
}.slice();

internal static (image.Image, error) readPNG(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return Decode(new png_test_package.os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// fakebKGDs maps from filenames to fake bKGD chunks for our approximation to
// the sng command-line tool. Package png doesn't keep that metadata when
// png.Decode returns an image.Image.
internal static map<@string, @string> fakebKGDs = new map<@string, @string>{
    ["ftbbn0g01"u8] = "bKGD {gray: 0;}\n"u8,
    ["ftbbn0g02"u8] = "bKGD {gray: 0;}\n"u8,
    ["ftbbn0g04"u8] = "bKGD {gray: 0;}\n"u8,
    ["ftbbn2c16"u8] = "bKGD {red: 0;  green: 0;  blue: 65535;}\n"u8,
    ["ftbbn3p08"u8] = "bKGD {index: 245}\n"u8,
    ["ftbgn2c16"u8] = "bKGD {red: 0;  green: 65535;  blue: 0;}\n"u8,
    ["ftbgn3p08"u8] = "bKGD {index: 245}\n"u8,
    ["ftbrn2c08"u8] = "bKGD {red: 255;  green: 0;  blue: 0;}\n"u8,
    ["ftbwn0g16"u8] = "bKGD {gray: 65535;}\n"u8,
    ["ftbwn3p08"u8] = "bKGD {index: 0}\n"u8,
    ["ftbyn3p08"u8] = "bKGD {index: 245}\n"u8
};

// fakegAMAs maps from filenames to fake gAMA chunks for our approximation to
// the sng command-line tool. Package png doesn't keep that metadata when
// png.Decode returns an image.Image.
internal static map<@string, @string> fakegAMAs = new map<@string, @string>{
    ["ftbbn0g01"u8] = ""u8,
    ["ftbbn0g02"u8] = "gAMA {0.45455}\n"u8
};

// fakeIHDRUsings maps from filenames to fake IHDR "using" lines for our
// approximation to the sng command-line tool. The PNG model is that
// transparency (in the tRNS chunk) is separate to the color/grayscale/palette
// color model (in the IHDR chunk). The Go model is that the concrete
// image.Image type returned by png.Decode, such as image.RGBA (with all pixels
// having 100% alpha) or image.NRGBA, encapsulates whether or not the image has
// transparency. This map is a hack to work around the fact that the Go model
// can't otherwise discriminate PNG's "IHDR says color (with no alpha) but tRNS
// says alpha" and "IHDR says color with alpha".
internal static map<@string, @string> fakeIHDRUsings = new map<@string, @string>{
    ["ftbbn0g01"u8] = "    using grayscale;\n"u8,
    ["ftbbn0g02"u8] = "    using grayscale;\n"u8,
    ["ftbbn0g04"u8] = "    using grayscale;\n"u8,
    ["ftbbn2c16"u8] = "    using color;\n"u8,
    ["ftbgn2c16"u8] = "    using color;\n"u8,
    ["ftbrn2c08"u8] = "    using color;\n"u8,
    ["ftbwn0g16"u8] = "    using grayscale;\n"u8
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usingColorˢ = "    using color;\n"u8;
internal static readonly @string usingColorAlphaˢ = "    using color alpha;\n"u8;
internal static readonly @string usingGrayscaleˢ = "    using grayscale;\n"u8;
internal static readonly @string usingColorPaletteˢ = "    using color palette;\n"u8;
internal static readonly @string unknownPngDecoderColorˢ = "unknown PNG decoder color model\n"u8;
internal static readonly @string gAMA10000ˢ = "gAMA {1.0000}\n"u8;
internal static readonly @string plteˢ2 = "PLTE {\n"u8;
internal static readonly @string tRNSˢ2 = "tRNS {\n"u8;
internal static readonly @string imagePixelsHexˢ = "IMAGE {\n    pixels hex\n"u8;

// An approximation of the sng command-line tool.
internal static void sng(io.WriteCloser w, @string filename, image.Image png) {
    GoFrame ᒐ = default;
    try {
        defer(() => w.Close(), ref ᒐ);
        var bounds = png.Bounds();
        var cm = png.ColorModel();
        nint bitdepth = default!;
        var exprᴛ1 = cm;
        if (AreEqual(exprᴛ1, color.RGBAModel) || AreEqual(exprᴛ1, color.NRGBAModel) || AreEqual(exprᴛ1, color.AlphaModel) || AreEqual(exprᴛ1, color.GrayModel)) {
            bitdepth = 8;
        }
        else { /* default: */
            bitdepth = 16;
        }

        var (cpm, _) = cm._<color.Palette>(ᐧ);
        ж<image.Paletted> paletted = default!;
        if (cpm != default!) {
            switch (ᐧ) {
            case {} when len(cpm) is <= 2: {
                bitdepth = 1;
                break;
            }
            case {} when len(cpm) is <= 4: {
                bitdepth = 2;
                break;
            }
            case {} when len(cpm) is <= 16: {
                bitdepth = 4;
                break;
            }
            default: {
                bitdepth = 8;
                break;
            }}

            paletted = png._<ж<image.Paletted>>();
        }
        // Write the filename and IHDR.
        io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "#SNG: from "u8 + filename + ".png\nIHDR {\n"u8);
        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "    width: %d; height: %d; bitdepth: %d;\n"u8, bounds.Dx(), bounds.Dy(), bitdepth);
        {
            var (s, ok) = fakeIHDRUsings[filename, ꟷ]; if (ok){
                io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), s);
            } else {
                switch (ᐧ) {
                case {} when (AreEqual(cm, color.RGBAModel)) || (AreEqual(cm, color.RGBA64Model)): {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), usingColorˢ);
                    break;
                }
                case {} when (AreEqual(cm, color.NRGBAModel)) || (AreEqual(cm, color.NRGBA64Model)): {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), usingColorAlphaˢ);
                    break;
                }
                case {} when (AreEqual(cm, color.GrayModel)) || (AreEqual(cm, color.Gray16Model)): {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), usingGrayscaleˢ);
                    break;
                }
                case {} when cpm != default!: {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), usingColorPaletteˢ);
                    break;
                }
                default: {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), unknownPngDecoderColorˢ);
                    break;
                }}

            }
        }
        io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "}\n"u8);
        // We fake a gAMA chunk. The test files have a gAMA chunk but the go PNG
        // parser ignores it (the PNG spec section 11.3 says "Ancillary chunks may
        // be ignored by a decoder").
        {
            var (s, ok) = fakegAMAs[filename, ꟷ]; if (ok){
                io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), s);
            } else {
                io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), gAMA10000ˢ);
            }
        }
        // Write the PLTE and tRNS (if applicable).
        var useTransparent = false;
        if (cpm != default!){
            nint lastAlpha = -1;
            io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), plteˢ2);
            foreach (var (i, c) in cpm) {
                uint8 r = default!;
                uint8 g = default!;
                uint8 b = default!;
                uint8 a = default!;
                switch (c.type()) {
                case colorꓸRGBA cΔ1: {
                    (r, g, b, a) = (cΔ1.R, cΔ1.G, cΔ1.B, 0xff);
                    break;
                }
                case color.NRGBA cΔ1: {
                    (r, g, b, a) = (cΔ1.R, cΔ1.G, cΔ1.B, cΔ1.A);
                    break;
                }
                default: {
                    var cΔ1 = c;
                    throw panic("unknown palette color type");
                    break;
                }}
                if (a != 0xff) {
                    lastAlpha = i;
                }
                fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "    (%3d,%3d,%3d)     # rgb = (0x%02x,0x%02x,0x%02x)\n"u8, r, g, b, r, g, b);
            }
            io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "}\n"u8);
            {
                var (s, ok) = fakebKGDs[filename, ꟷ]; if (ok) {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), s);
                }
            }
            if (lastAlpha != -1) {
                io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), tRNSˢ2);
                for (nint i = 0; i <= lastAlpha; i++) {
                    var (_, _, _, a) = cpm[i].RGBA();
                    a >>= (int)(8);
                    fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), " %d"u8, a);
                }
                io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "}\n"u8);
            }
        } else 
        if (strings.HasPrefix(filename, "ft"u8)) {
            {
                var (s, ok) = fakebKGDs[filename, ꟷ]; if (ok) {
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), s);
                }
            }
            // We fake a tRNS chunk. The test files' grayscale and truecolor
            // transparent images all have their top left corner transparent.
            switch (png.At(0, 0).type()) {
            case color.NRGBA c: {
                if (c.A == 0) {
                    useTransparent = true;
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), tRNSˢ2);
                    var exprᴛ2 = filename;
                    if (exprᴛ2 == "ftbbn0g01"u8 || exprᴛ2 == "ftbbn0g02"u8 || exprᴛ2 == "ftbbn0g04"u8) {
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), // The standard image package doesn't have a "gray with
 // alpha" type. Instead, we use an image.NRGBA.
 "    gray: %d;\n"u8, c.R);
                    }
                    else { /* default: */
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "    red: %d; green: %d; blue: %d;\n"u8, c.R, c.G, c.B);
                    }

                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "}\n"u8);
                }
                break;
            }
            case color.NRGBA64 c: {
                if (c.A == 0) {
                    useTransparent = true;
                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), tRNSˢ2);
                    var exprᴛ3 = filename;
                    if (exprᴛ3 == "ftbwn0g16"u8) {
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), // The standard image package doesn't have a "gray16 with
 // alpha" type. Instead, we use an image.NRGBA64.
 "    gray: %d;\n"u8, c.R);
                    }
                    else { /* default: */
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "    red: %d; green: %d; blue: %d;\n"u8, c.R, c.G, c.B);
                    }

                    io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "}\n"u8);
                }
                break;
            }}
        }
        // Write the IMAGE.
        io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), imagePixelsHexˢ);
        for (nint y = bounds.Min.Y; y < bounds.Max.Y; y++) {
            switch (ᐧ) {
            case {} when AreEqual(cm, color.GrayModel): {
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    var gray = png.At(x, y)._<color.Gray>();
                    fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x"u8, gray.Y);
                }
                break;
            }
            case {} when AreEqual(cm, color.Gray16Model): {
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    var gray16 = png.At(x, y)._<color.Gray16>();
                    fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%04x "u8, gray16.Y);
                }
                break;
            }
            case {} when AreEqual(cm, color.RGBAModel): {
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    var rgba = png.At(x, y)._<colorꓸRGBA>();
                    fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x%02x%02x "u8, rgba.R, rgba.G, rgba.B);
                }
                break;
            }
            case {} when AreEqual(cm, color.RGBA64Model): {
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    var rgba64 = png.At(x, y)._<color.RGBA64>();
                    fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%04x%04x%04x "u8, rgba64.R, rgba64.G, rgba64.B);
                }
                break;
            }
            case {} when AreEqual(cm, color.NRGBAModel): {
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    var nrgba = png.At(x, y)._<color.NRGBA>();
                    var exprᴛ4 = filename;
                    if (exprᴛ4 == "ftbbn0g01"u8 || exprᴛ4 == "ftbbn0g02"u8 || exprᴛ4 == "ftbbn0g04"u8) {
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x"u8, nrgba.R);
                    }
                    else { /* default: */
                        if (useTransparent){
                            fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x%02x%02x "u8, nrgba.R, nrgba.G, nrgba.B);
                        } else {
                            fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x%02x%02x%02x "u8, nrgba.R, nrgba.G, nrgba.B, nrgba.A);
                        }
                    }

                }
                break;
            }
            case {} when AreEqual(cm, color.NRGBA64Model): {
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    var nrgba64 = png.At(x, y)._<color.NRGBA64>();
                    var exprᴛ5 = filename;
                    if (exprᴛ5 == "ftbwn0g16"u8) {
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%04x "u8, nrgba64.R);
                    }
                    else { /* default: */
                        if (useTransparent){
                            fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%04x%04x%04x "u8, nrgba64.R, nrgba64.G, nrgba64.B);
                        } else {
                            fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%04x%04x%04x%04x "u8, nrgba64.R, nrgba64.G, nrgba64.B, nrgba64.A);
                        }
                    }

                }
                break;
            }
            case {} when cpm != default!: {
                nint b = default!;
                nint c = default!;
                for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
                    b = (nint)(b.Lsh((nuint)bitdepth) | (nint)paletted.ColorIndexAt(x, y));
                    c++;
                    if (c == 8 / bitdepth) {
                        fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x"u8, b);
                        b = 0;
                        c = 0;
                    }
                }
                if (c != 0) {
                    while (c != 8 / bitdepth) {
                        b = b.Lsh((nuint)bitdepth);
                        c++;
                    }
                    fmt.Fprintf(new png_test_package.io_WriteCloserᴠWriter(w), "%02x"u8, b);
                }
                break;
            }}

            io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "\n"u8);
        }
        io.WriteString(new png_test_package.io_WriteCloserᴠWriter(w), "}\n"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rgbˢ = "# rgb = ("u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestReader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var names = filenames;
        if (testing.Short()) {
            names = filenamesShort;
        }
        foreach (var (_, fn) in names) {
            // Read the .png file.
            var (img, err) = readPNG("testdata/pngsuite/"u8 + fn + ".png"u8);
            if (err != default!) {
                Ꮡt.Error(fn, err);
                continue;
            }
            if (fn == "basn4a16"u8) {
                // basn4a16.sng is gray + alpha but sng() will produce true color + alpha
                // so we just check a single random pixel.
                var c = img.At(2, 1)._<color.NRGBA64>();
                if (c.R != 0x11a7 || c.G != 0x11a7 || c.B != 0x11a7 || c.A != 0x1085) {
                    Ꮡt.Error(fn, fmt.Errorf("wrong pixel value at (2, 1): %x"u8, c));
                }
                continue;
            }
            var (piper, pipew) = io.Pipe();
            var pb = bufio.NewScanner(new io.PipeReaderжReader(piper));
            goǃ(sng, new io.PipeWriterжWriteCloser(pipew), fn, img);
            var piperʗ1 = piper;
            defer(() => piperʗ1.Close(), ref ᒐ);
            // Read the .sng file.
            (var sf, err) = os.Open("testdata/pngsuite/"u8 + fn + ".sng"u8);
            if (err != default!) {
                Ꮡt.Error(fn, err);
                continue;
            }
            var sfʗ1 = sf;
            defer(() => sfʗ1.Close(), ref ᒐ);
            var sb = bufio.NewScanner(new png_test_package.os_FileжReader(sf));
            // Compare the two, in SNG format, line by line.
            while (ᐧ) {
                var pdone = !pb.Scan();
                var sdone = !sb.Scan();
                if (pdone && sdone) {
                    break;
                }
                if (pdone || sdone) {
                    Ꮡt.Errorf("%s: Different sizes"u8, fn);
                    break;
                }
                @string ps = pb.Text();
                @string ss = sb.Text();
                // Newer versions of the sng command line tool append an optional
                // color name to the RGB tuple. For example:
                //	# rgb = (0xff,0xff,0xff) grey100
                //	# rgb = (0x00,0x00,0xff) blue1
                // instead of the older version's plainer:
                //	# rgb = (0xff,0xff,0xff)
                //	# rgb = (0x00,0x00,0xff)
                // We strip any such name.
                if (strings.Contains(ss, rgbˢ) && !strings.HasSuffix(ss, ")"u8)) {
                    {
                        nint i = strings.LastIndex(ss, ") "u8); if (i >= 0) {
                            ss = ss.slice(0, i + 1);
                        }
                    }
                }
                if (ps != ss) {
                    Ꮡt.Errorf("%s: Mismatch\n%s\nversus\n%s\n"u8, fn, ps, ss);
                    break;
                }
            }
            if (pb.Err() != default!) {
                Ꮡt.Error(fn, pb.Err());
            }
            if (sb.Err() != default!) {
                Ꮡt.Error(fn, sb.Err());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}


[GoType("dyn")] partial struct readerErrorsᴛ1 {
    internal @string @file;
    internal @string err;
}
internal static slice<readerErrorsᴛ1> readerErrors = new readerErrorsᴛ1[]{
    new("invalid-zlib.png"u8, "zlib: invalid checksum"u8),
    new("invalid-crc32.png"u8, "invalid checksum"u8),
    new("invalid-noend.png"u8, "unexpected EOF"u8),
    new("invalid-trunc.png"u8, "unexpected EOF"u8)
}.slice();

public static void TestReaderError(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in readerErrors) {
        var (img, err) = readPNG("testdata/"u8 + tt.@file);
        if (err == default!) {
            Ꮡt.Errorf("decoding %s: missing error"u8, tt.@file);
            continue;
        }
        if (!strings.Contains(err.Error(), tt.err)) {
            Ꮡt.Errorf("decoding %s: %s, want %s"u8, tt.@file, err, tt.err);
        }
        if (img != default!) {
            Ꮡt.Errorf("decoding %s: have image + error"u8, tt.@file);
        }
    }
}

public static void TestPalettedDecodeConfig(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, fn) in filenamesPaletted) {
            var (f, err) = os.Open("testdata/pngsuite/"u8 + fn + ".png"u8);
            if (err != default!) {
                Ꮡt.Errorf("%s: open failed: %v"u8, fn, err);
                continue;
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            (var cfg, err) = DecodeConfig(new png_test_package.os_FileжReader(f));
            if (err != default!) {
                Ꮡt.Errorf("%s: %v"u8, fn, err);
                continue;
            }
            var (pal, ok) = cfg.ColorModel._<color.Palette>(ᐧ);
            if (!ok) {
                Ꮡt.Errorf("%s: expected paletted color model"u8, fn);
                continue;
            }
            if (pal == default!) {
                Ꮡt.Errorf("%s: palette not initialized"u8, fn);
                continue;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGrayGradientPngˢ = "testdata/gray-gradient.png"u8;
internal static readonly @string testdataGrayGradientˢ = "testdata/gray-gradient.interlaced.png"u8;

public static void TestInterlaced(ж<testing.T> Ꮡt) {
    var (a, err) = readPNG(testdataGrayGradientPngˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var b, err) = readPNG(testdataGrayGradientˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!reflect.DeepEqual(a, b)) {
        Ꮡt.Fatalf("decodings differ:\nnon-interlaced:\n%#v\ninterlaced:\n%#v"u8, a, b);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gotNilErrorWantNonNilˢ = (@string)"got nil error, want non-nil"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string ihdrᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x08, 0x00, 0x00, 0x00, 0x00, 0xbc, 0xea, 0xe9, 0xfb}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string idatᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0e, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x62, 0x00, 0x04, 0x00, 0x00, 0xff, 0xff, 0x00, 0x06, 0x00, 0x03, 0xfa, 0xd0, 0x59, 0xae}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string iendᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82}));

public static void TestIncompleteIDATOnRowBoundary(ж<testing.T> Ꮡt) {
    // The following is an invalid 1x2 grayscale PNG image. The header is OK,
    // but the zlib-compressed IDAT payload contains two bytes "\x02\x00",
    // which is only one row of data (the leading "\x02" is a row filter).
    @string ihdr = ihdrᶜ;
    
    @string idat = idatᶜ;
    
    @string iend = iendᶜ;
    var (_, err) = Decode(new png_test_package.strings_ReaderжReader(strings.NewReader(pngHeader + ihdr + idat + iend)));
    if (err == default!) {
        Ꮡt.Fatal(gotNilErrorWantNonNilˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodedImageFromTrailingˢ = (@string)"decoded image from trailing IDAT chunk"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string ihdrᶜ1 = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x00, 0x00, 0x3a, 0x7e, 0x9b, 0x55}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string idatWhiteᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0e, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0xfa, 0x0f, 0x08, 0x00, 0x00, 0xff, 0xff, 0x01, 0x05, 0x01, 0x02, 0x5a, 0xdd, 0x39, 0xcd}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string idatZeroᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x49, 0x44, 0x41, 0x54, 0x35, 0xaf, 0x06, 0x1e}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string iendᶜ1 = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string idatBlackᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0e, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x62, 0x00, 0x04, 0x00, 0x00, 0xff, 0xff, 0x00, 0x06, 0x00, 0x03, 0xfa, 0xd0, 0x59, 0xae}));

public static void TestTrailingIDATChunks(ж<testing.T> Ꮡt) {
    // The following is a valid 1x1 PNG image containing color.Gray{255} and
    // a trailing zero-length IDAT chunk (see PNG specification section 12.9):
    @string ihdr = ihdrᶜ1;
    
    @string idatWhite = idatWhiteᶜ;
    
    @string idatZero = idatZeroᶜ;
    
    @string iend = iendᶜ1;
    var (_, err) = Decode(new png_test_package.strings_ReaderжReader(strings.NewReader(pngHeader + ihdr + idatWhite + idatZero + iend)));
    if (err != default!) {
        Ꮡt.Fatalf("decoding valid image: %v"u8, err);
    }
    // Non-zero-length trailing IDAT chunks should be ignored (recoverable error).
    // The following chunk contains a single pixel with color.Gray{0}.
    @string idatBlack = idatBlackᶜ;
    (var img, err) = Decode(new png_test_package.strings_ReaderжReader(strings.NewReader(pngHeader + ihdr + idatWhite + idatBlack + iend)));
    if (err != default!) {
        Ꮡt.Fatalf("trailing IDAT not ignored: %v"u8, err);
    }
    if (AreEqual(img.At(0, 0), (new color.Gray(0)))) {
        Ꮡt.Fatal(decodedImageFromTrailingˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string ihdrᶜ2 = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x03, 0x00, 0x00, 0x00, 0x28, 0xcb, 0x34, 0xbb}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string plteᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x03, 0x50, 0x4c, 0x54, 0x45, 0xff, 0x00, 0x00, 0x19, 0xe2, 0x09, 0x37}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string trnsᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x01, 0x74, 0x52, 0x4e, 0x53, 0x7f, 0x80, 0x5c, 0xb4, 0xcb}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string idatᶜ1 = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x0e, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x62, 0x00, 0x04, 0x00, 0x00, 0xff, 0xff, 0x00, 0x06, 0x00, 0x03, 0xfa, 0xd0, 0x59, 0xae}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string iendᶜ2 = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82}));

public static void TestMultipletRNSChunks(ж<testing.T> Ꮡt) {
    /*
		The following is a valid 1x1 paletted PNG image with a 1-element palette
		containing color.NRGBA{0xff, 0x00, 0x00, 0x7f}:
			0000000: 8950 4e47 0d0a 1a0a 0000 000d 4948 4452  .PNG........IHDR
			0000010: 0000 0001 0000 0001 0803 0000 0028 cb34  .............(.4
			0000020: bb00 0000 0350 4c54 45ff 0000 19e2 0937  .....PLTE......7
			0000030: 0000 0001 7452 4e53 7f80 5cb4 cb00 0000  ....tRNS..\.....
			0000040: 0e49 4441 5478 9c62 6200 0400 00ff ff00  .IDATx.bb.......
			0000050: 0600 03fa d059 ae00 0000 0049 454e 44ae  .....Y.....IEND.
			0000060: 4260 82                                  B`.
		Dropping the tRNS chunk makes that color's alpha 0xff instead of 0x7f.
	*/
    @string ihdr = ihdrᶜ2;
    
    @string plte = plteᶜ;
    
    @string trns = trnsᶜ;
    
    @string idat = idatᶜ1;
    
    @string iend = iendᶜ2;
    for (nint i = 0; i < 4; i++) {
        slice<byte> b = default!;
        b = append(b, pngHeader.ꓸꓸꓸ);
        b = append(b, ihdr.ꓸꓸꓸ);
        b = append(b, plte.ꓸꓸꓸ);
        for (nint j = 0; j < i; j++) {
            b = append(b, trns.ꓸꓸꓸ);
        }
        b = append(b, idat.ꓸꓸꓸ);
        b = append(b, iend.ꓸꓸꓸ);
        color.Color want = default!;
        var (m, err) = Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        switch (i) {
        case 0: {
            if (err != default!) {
                Ꮡt.Errorf("%d tRNS chunks: %v"u8, i, err);
                continue;
            }
            want = new colorꓸRGBA(0xff, 0x00, 0x00, 0xff);
            break;
        }
        case 1: {
            if (err != default!) {
                Ꮡt.Errorf("%d tRNS chunks: %v"u8, i, err);
                continue;
            }
            want = new color.NRGBA(0xff, 0x00, 0x00, 0x7f);
            break;
        }
        default: {
            if (err == default!) {
                Ꮡt.Errorf("%d tRNS chunks: got nil error, want non-nil"u8, i);
            }
            continue;
            break;
        }}

        {
            var got = m.At(0, 0); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("%d tRNS chunks: got %T %v, want %T %v"u8, i, got, got, want, want);
            }
        }
    }
}

public static void TestUnknownChunkLengthUnderflow(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var data = new byte[]{0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x06, 0xf4, 0x7c, 0x55, 0x04, 0x1a,
        0xd3, 0x11, 0x9a, 0x73, 0x00, 0x00, 0xf8, 0x1e, 0xf3, 0x2e, 0x00, 0x00,
        0x01, 0x00, 0xff, 0xff, 0xff, 0xff, 0x07, 0xf4, 0x7c, 0x55, 0x04, 0x1a,
        0xd3}.slice();
    var (_, err) = Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    if (err == default!) {
        Ꮡt.Errorf("Didn't fail reading an unknown chunk with length 0xffffffff"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataInvalidPaletteˢ = "testdata/invalid-palette.png"u8;

public static void TestPaletted8OutOfRangePixel(ж<testing.T> Ꮡt) {
    // IDAT contains a reference to a palette index that does not exist in the file.
    var (img, err) = readPNG(testdataInvalidPaletteˢ);
    if (err != default!) {
        Ꮡt.Errorf("decoding invalid-palette.png: unexpected error %v"u8, err);
        return;
    }
    // Expect that the palette is extended with opaque black.
    var want = new colorꓸRGBA(0x00, 0x00, 0x00, 0xff);
    {
        var got = img.At(15, 15); if (!AreEqual(got, want)) {
            Ꮡt.Errorf("got %F %v, expected %T %v"u8, got, got, want, want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string hexᶜ = "0123456789abcdef"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = ".. .. .. ce bd bd bd bd bd bd bd bd bd bd e6 \n.. .. .. 7b 84 94 94 94 94 94 94 94 94 6b bd \n.. .. .. 7b d6 .. .. .. .. .. .. .. .. 8c bd \n.. .. .. 7b d6 .. .. .. .. .. .. .. .. 8c bd \n.. .. .. 7b d6 .. .. .. .. .. .. .. .. 8c bd \ne6 bd bd 7b a5 bd bd f7 .. .. .. .. .. 8c bd \nbd 6b 94 94 94 94 5a ef .. .. .. .. .. 8c bd \nbd 8c .. .. .. .. 63 ad ad ad ad ad ad 73 bd \nbd 8c .. .. .. .. 63 9c 9c 9c 9c 9c 9c 9c de \nbd 6b 94 94 94 94 5a ef .. .. .. .. .. .. .. \ne6 b5 b5 b5 b5 b5 b5 f7 .. .. .. .. .. .. .. \n";

public static void TestGray8Transparent(ж<testing.T> Ꮡt) {
    // These bytes come from https://golang.org/issues/19553
    var (m, err) = Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(new byte[]{
        0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x0b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x85, 0x2c, 0x88,
        0x80, 0x00, 0x00, 0x00, 0x02, 0x74, 0x52, 0x4e, 0x53, 0x00, 0xff, 0x5b, 0x91, 0x22, 0xb5, 0x00,
        0x00, 0x00, 0x02, 0x62, 0x4b, 0x47, 0x44, 0x00, 0xff, 0x87, 0x8f, 0xcc, 0xbf, 0x00, 0x00, 0x00,
        0x09, 0x70, 0x48, 0x59, 0x73, 0x00, 0x00, 0x0a, 0xf0, 0x00, 0x00, 0x0a, 0xf0, 0x01, 0x42, 0xac,
        0x34, 0x98, 0x00, 0x00, 0x00, 0x07, 0x74, 0x49, 0x4d, 0x45, 0x07, 0xd5, 0x04, 0x02, 0x12, 0x11,
        0x11, 0xf7, 0x65, 0x3d, 0x8b, 0x00, 0x00, 0x00, 0x4f, 0x49, 0x44, 0x41, 0x54, 0x08, 0xd7, 0x63,
        0xf8, 0xff, 0xff, 0xff, 0xb9, 0xbd, 0x70, 0xf0, 0x8c, 0x01, 0xc8, 0xaf, 0x6e, 0x99, 0x02, 0x05,
        0xd9, 0x7b, 0xc1, 0xfc, 0x6b, 0xff, 0xa1, 0xa0, 0x87, 0x30, 0xff, 0xd9, 0xde, 0xbd, 0xd5, 0x4b,
        0xf7, 0xee, 0xfd, 0x0e, 0xe3, 0xef, 0xcd, 0x06, 0x19, 0x14, 0xf5, 0x1e, 0xce, 0xef, 0x01, 0x31,
        0x92, 0xd7, 0x82, 0x41, 0x31, 0x9c, 0x3f, 0x07, 0x02, 0xee, 0xa1, 0xaa, 0xff, 0xff, 0x9f, 0xe1,
        0xd9, 0x56, 0x30, 0xf8, 0x0e, 0xe5, 0x03, 0x00, 0xa9, 0x42, 0x84, 0x3d, 0xdf, 0x8f, 0xa6, 0x8f,
        0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
    }.slice())));
    if (err != default!) {
        Ꮡt.Fatalf("Decode: %v"u8, err);
    }
    @string hex = hexᶜ;
    slice<byte> got = default!;
    var bounds = m.Bounds();
    for (nint y = bounds.Min.Y; y < bounds.Max.Y; y++) {
        for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
            {
                var (r, _, _, a) = m.At(x, y).RGBA(); if (a != 0){
                    got = append(got,
                        hex[(uint32)(0x0f & ((r >> (int)(12))))],
                        hex[(uint32)(0x0f & ((r >> (int)(8))))],
                        (byte)((rune)' '));
                } else {
                    got = append(got,
                        (byte)((rune)'.'),
                        (byte)((rune)'.'),
                        (byte)((rune)' '));
                }
            }
        }
        got = append(got, (byte)((rune)'\n'));
    }
    @string want = wantᶜ;
    if (((sstring)got) != want) {
        Ꮡt.Errorf("got:\n%swant:\n%s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestsWhichˢ = (@string)"skipping tests which allocate large pixel buffers"u8;

[GoType("dyn")] internal partial struct TestDimensionOverflow_testCases {
    internal slice<byte> src;
    internal bool unsupportedConfig;
    internal nint width;
    internal nint height;
}

public static void TestDimensionOverflow(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint maxInt32AsInt = (nint)((2147483648L) - 1);
    var have32BitInts = 0 > (1 + maxInt32AsInt);
    var testCases = new TestDimensionOverflow_testCases[]{ // These bytes come from https://golang.org/issues/22304
 //
 // It encodes a 2147483646 × 2147483646 (i.e. 0x7ffffffe × 0x7ffffffe)
 // NRGBA image. The (width × height) per se doesn't overflow an int64, but
 // (width × height × bytesPerPixel) will.

        new(
            src: new byte[]{
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                0x7f, 0xff, 0xff, 0xfe, 0x7f, 0xff, 0xff, 0xfe, 0x08, 0x06, 0x00, 0x00, 0x00, 0x30, 0x57, 0xb3,
                0xfd, 0x00, 0x00, 0x00, 0x15, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x62, 0x20, 0x12, 0x8c,
                0x2a, 0xa4, 0xb3, 0x42, 0x40, 0x00, 0x00, 0x00, 0xff, 0xff, 0x13, 0x38, 0x00, 0x15, 0x2d, 0xef,
                0x5f, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
            }.slice(), // It's debatable whether DecodeConfig (which does not allocate a
 // pixel buffer, unlike Decode) should fail in this case. The Go
 // standard library has made its choice, and the standard library
 // has compatibility constraints.

            unsupportedConfig: true,
            width: 0x7ffffffe,
            height: 0x7ffffffe
        ), // The next three cases come from https://golang.org/issues/38435

        new(
            src: new byte[]{
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                0x00, 0x00, 0xb5, 0x04, 0x00, 0x00, 0xb5, 0x04, 0x08, 0x06, 0x00, 0x00, 0x00, 0xf5, 0x60, 0x2c,
                0xb8, 0x00, 0x00, 0x00, 0x15, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x62, 0x20, 0x12, 0x8c,
                0x2a, 0xa4, 0xb3, 0x42, 0x40, 0x00, 0x00, 0x00, 0xff, 0xff, 0x13, 0x38, 0x00, 0x15, 0x2d, 0xef,
                0x5f, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
            }.slice(), // Here, width * height = 0x7ffea810, just under MaxInt32, but at 4
 // bytes per pixel, the number of pixels overflows an int32.

            unsupportedConfig: have32BitInts,
            width: 0x0000b504,
            height: 0x0000b504
        ),
        new(
            src: new byte[]{
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x30, 0x6e, 0xc5,
                0x21, 0x00, 0x00, 0x00, 0x15, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x62, 0x20, 0x12, 0x8c,
                0x2a, 0xa4, 0xb3, 0x42, 0x40, 0x00, 0x00, 0x00, 0xff, 0xff, 0x13, 0x38, 0x00, 0x15, 0x2d, 0xef,
                0x5f, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
            }.slice(),
            unsupportedConfig: false,
            width: 0x04000000,
            height: 0x00000001
        ),
        new(
            src: new byte[]{
                0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
                0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0xaa, 0xd4, 0x7c,
                0xda, 0x00, 0x00, 0x00, 0x15, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9c, 0x62, 0x66, 0x20, 0x12, 0x30,
                0x8d, 0x2a, 0xa4, 0xaf, 0x42, 0x40, 0x00, 0x00, 0x00, 0xff, 0xff, 0x14, 0xd2, 0x00, 0x16, 0x00,
                0x00, 0x00
            }.slice(),
            unsupportedConfig: false,
            width: 0x08000000,
            height: 0x00000001
        )
    }.slice();
    foreach (var (i, tc) in testCases) {
        var (cfg, err) = DecodeConfig(new png_test_package.bytes_ReaderжReader(bytes.NewReader(tc.src)));
        if (tc.unsupportedConfig){
            if (err == default!){
                Ꮡt.Errorf("i=%d: DecodeConfig: got nil error, want non-nil"u8, i);
            } else 
            {
                var (_, ok) = err._<UnsupportedError>(ᐧ); if (!ok) {
                    Ꮡt.Fatalf("Decode: got %v (of type %T), want non-nil error (of type png.UnsupportedError)"u8, err, err);
                }
            }
            continue;
        } else 
        if (err != default!){
            Ꮡt.Errorf("i=%d: DecodeConfig: %v"u8, i, err);
            continue;
        } else 
        if (cfg.Width != tc.width){
            Ꮡt.Errorf("i=%d: width: got %d, want %d"u8, i, cfg.Width, tc.width);
            continue;
        } else 
        if (cfg.Height != tc.height) {
            Ꮡt.Errorf("i=%d: height: got %d, want %d"u8, i, cfg.Height, tc.height);
            continue;
        }
        {
            var nPixels = (int64)cfg.Width * (int64)cfg.Height; if (nPixels > 0x7f000000){
                // In theory, calling Decode would succeed, given several gigabytes
                // of memory. In practice, trying to make a []uint8 big enough to
                // hold all of the pixels can often result in OOM (out of memory).
                // OOM is unrecoverable; we can't write a test that passes when OOM
                // happens. Instead we skip the Decode call (and its tests).
                continue;
            } else 
            if (testing.Short()) {
                // Even for smaller image dimensions, calling Decode might allocate
                // 1 GiB or more of memory. This is usually feasible, and we want
                // to check that calling Decode doesn't panic if there's enough
                // memory, but we provide a runtime switch (testing.Short) to skip
                // these if it would OOM. See also http://golang.org/issue/5050
                // "decoding... images can cause huge memory allocations".
                continue;
            }
        }
        // Even if we don't panic, these aren't valid PNG images.
        {
            var (_, errΔ1) = Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(tc.src))); if (errΔ1 == default!) {
                Ꮡt.Errorf("i=%d: Decode: got nil error, want non-nil"u8, i);
            }
        }
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingTestsWhichˢ);
    }
}

public static void TestDecodePalettedWithTransparency(ж<testing.T> Ꮡt) {
    // These bytes come from https://go.dev/issue/54325
    //
    // Per the PNG spec, a PLTE chunk contains 3 (not 4) bytes per palette
    // entry: RGB (not RGBA). The alpha value comes from the optional tRNS
    // chunk. Here, the PLTE chunk (0x50, 0x4c, 0x54, 0x45, etc) has 16 entries
    // (0x30 = 48 bytes) and the tRNS chunk (0x74, 0x52, 0x4e, 0x53, etc) has 1
    // entry (0x01 = 1 byte) that sets the first palette entry's alpha to zero.
    //
    // Both Decode and DecodeConfig should pick up that the first palette
    // entry's alpha is zero.
    var src = new byte[]{
        0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a, 0x00, 0x00, 0x00, 0x0d, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x20, 0x04, 0x03, 0x00, 0x00, 0x00, 0x81, 0x54, 0x67,
        0xc7, 0x00, 0x00, 0x00, 0x30, 0x50, 0x4c, 0x54, 0x45, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0x0e,
        0x00, 0x23, 0x27, 0x7b, 0xb1, 0x2d, 0x0a, 0x49, 0x3f, 0x19, 0x78, 0x5f, 0xcd, 0xe4, 0x69, 0x69,
        0xe4, 0x71, 0x59, 0x53, 0x80, 0x11, 0x14, 0x8b, 0x00, 0xa9, 0x8d, 0x95, 0xcb, 0x99, 0x2f, 0x6b,
        0xd7, 0x29, 0x91, 0xd7, 0x7b, 0xba, 0xff, 0xe3, 0xd7, 0x13, 0xc6, 0xd3, 0x58, 0x00, 0x00, 0x00,
        0x01, 0x74, 0x52, 0x4e, 0x53, 0x00, 0x40, 0xe6, 0xd8, 0x66, 0x00, 0x00, 0x00, 0xfd, 0x49, 0x44,
        0x41, 0x54, 0x28, 0xcf, 0x63, 0x60, 0x00, 0x83, 0x55, 0x0c, 0x68, 0x60, 0x9d, 0x02, 0x9a, 0x80,
        0xde, 0x23, 0x74, 0x15, 0xef, 0x50, 0x94, 0x70, 0x2d, 0xd2, 0x7b, 0x87, 0xa2, 0x84, 0xeb, 0xee,
        0xbb, 0x77, 0x6f, 0x51, 0x94, 0xe8, 0xbd, 0x7d, 0xf7, 0xee, 0x12, 0xb2, 0x80, 0xd2, 0x3d, 0x54,
        0x01, 0x26, 0x10, 0x1f, 0x59, 0x40, 0x0f, 0xc8, 0xd7, 0x7e, 0x84, 0x70, 0x1c, 0xd7, 0xba, 0xb7,
        0x4a, 0xda, 0xda, 0x77, 0x11, 0xf6, 0xac, 0x5a, 0xa5, 0xf4, 0xf9, 0xbf, 0xfd, 0x3d, 0x24, 0x6b,
        0x98, 0x94, 0xf4, 0xff, 0x7f, 0x52, 0x42, 0x16, 0x30, 0x0e, 0xd9, 0xed, 0x6a, 0x8c, 0xec, 0x10,
        0x65, 0x53, 0x97, 0x60, 0x23, 0x64, 0x1d, 0x8a, 0x2e, 0xc6, 0x2e, 0x42, 0x08, 0x3d, 0x4c, 0xca,
        0x81, 0xc1, 0x82, 0xa6, 0xa2, 0x46, 0x08, 0x3d, 0x4a, 0xa1, 0x82, 0xc6, 0x82, 0xa1, 0x4a, 0x08,
        0x3d, 0xfa, 0xa6, 0x81, 0xa1, 0xa2, 0xc1, 0x9f, 0x10, 0x66, 0xd4, 0x2b, 0x87, 0x0a, 0x86, 0x1a,
        0x7d, 0x57, 0x80, 0x9b, 0x99, 0xaf, 0x62, 0x1a, 0x1a, 0xec, 0xf0, 0x0d, 0x66, 0x2a, 0x7b, 0x5a,
        0xba, 0xd2, 0x64, 0x63, 0x4b, 0xa6, 0xb2, 0xb4, 0x02, 0xa8, 0x12, 0xb5, 0x24, 0xa5, 0x99, 0x2e,
        0x33, 0x95, 0xd4, 0x92, 0x10, 0xee, 0xd0, 0x59, 0xb9, 0x6a, 0xd6, 0x21, 0x24, 0xb7, 0x33, 0x9d,
        0x01, 0x01, 0x64, 0xbf, 0xac, 0x59, 0xb2, 0xca, 0xeb, 0x14, 0x92, 0x80, 0xd6, 0x9a, 0x53, 0x4a,
        0x6b, 0x4e, 0x2d, 0x42, 0x52, 0xa1, 0x73, 0x28, 0x54, 0xe7, 0x90, 0x6a, 0x00, 0x92, 0x92, 0x45,
        0xa1, 0x40, 0x84, 0x2c, 0xe0, 0xc4, 0xa0, 0xb2, 0x28, 0x14, 0xc1, 0x67, 0xe9, 0x50, 0x60, 0x60,
        0xea, 0x70, 0x40, 0x12, 0x00, 0x79, 0x54, 0x09, 0x22, 0x00, 0x00, 0x30, 0xf3, 0x52, 0x87, 0xc6,
        0xe4, 0xbd, 0x70, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82
    }.slice();
    var (cfg, err) = DecodeConfig(new png_test_package.bytes_ReaderжReader(bytes.NewReader(src)));
    if (err != default!){
        Ꮡt.Fatalf("DecodeConfig: %v"u8, err);
    } else 
    {
        var (_, _, _, alpha) = cfg.ColorModel._<color.Palette>()[0].RGBA(); if (alpha != 0) {
            Ꮡt.Errorf("DecodeConfig: got %d, want 0"u8, alpha);
        }
    }
    (var img, err) = Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(src)));
    if (err != default!){
        Ꮡt.Fatalf("Decode: %v"u8, err);
    } else 
    {
        var (_, _, _, alpha) = img.ColorModel()._<color.Palette>()[0].RGBA(); if (alpha != 0) {
            Ꮡt.Errorf("Decode: got %d, want 0"u8, alpha);
        }
    }
}

internal static void benchmarkDecode(ж<testing.B> Ꮡb, @string filename, nint bytesPerPixel) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (data, err) = os.ReadFile(filename);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    (var cfg, err) = DecodeConfig(new png_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    b.SetBytes((int64)(cfg.Width * cfg.Height * bytesPerPixel));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Decode(new png_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBenchGrayPngˢ = "testdata/benchGray.png"u8;

public static void BenchmarkDecodeGray(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataBenchGrayPngˢ, 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBenchNRGBAˢ = "testdata/benchNRGBA-gradient.png"u8;

public static void BenchmarkDecodeNRGBAGradient(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataBenchNRGBAˢ, 4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBenchNRGBAOpaqueˢ = "testdata/benchNRGBA-opaque.png"u8;

public static void BenchmarkDecodeNRGBAOpaque(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataBenchNRGBAOpaqueˢ, 4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBenchPalettedPngˢ = "testdata/benchPaletted.png"u8;

public static void BenchmarkDecodePaletted(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataBenchPalettedPngˢ, 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBenchRGBPngˢ = "testdata/benchRGB.png"u8;

public static void BenchmarkDecodeRGB(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataBenchRGBPngˢ, 4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBenchRGBˢ = "testdata/benchRGB-interlace.png"u8;

public static void BenchmarkDecodeInterlacing(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, testdataBenchRGBˢ, 4);
}

} // end png_internal_test_package
