// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using zlib = compress.zlib_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using image = image_package;
using color = go.image.color_package;
using draw = go.image.draw_package;
using io = io_package;
using testing = testing_package;
using compress;
using encoding;
using go.image;
using static go.image.png_package;

partial class png_internal_test_package {

internal static error diff(image.Image m0, image.Image m1) {
    var (b0, b1) = (m0.Bounds(), m1.Bounds());
    if (!b0.Size().Eq(b1.Size())) {
        return fmt.Errorf("dimensions differ: %v vs %v"u8, b0, b1);
    }
    nint dx = b1.Min.X - b0.Min.X;
    nint dy = b1.Min.Y - b0.Min.Y;
    for (nint y = b0.Min.Y; y < b0.Max.Y; y++) {
        for (nint x = b0.Min.X; x < b0.Max.X; x++) {
            var c0 = m0.At(x, y);
            var c1 = m1.At(x + dx, y + dy);
            var (r0, g0, b0Δ1, a0) = c0.RGBA();
            var (r1, g1, b1Δ1, a1) = c1.RGBA();
            if (r0 != r1 || g0 != g1 || b0Δ1 != b1Δ1 || a0 != a1) {
                return fmt.Errorf("colors differ at (%d, %d): %T%v vs %T%v"u8, x, y, c0, c0, c1, c1);
            }
        }
    }
    return default!;
}

internal static (image.Image, error) encodeDecode(image.Image m) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var err = Encode(new png_test_package.bytes_BufferжWriter(Ꮡb), m);
    if (err != default!) {
        return (default!, err);
    }
    return Decode(new png_test_package.bytes_BufferжReader(Ꮡb));
}

internal static ж<image.NRGBA> convertToNRGBA(image.Image m) {
    var b = m.Bounds();
    var ret = image.NewNRGBA(b);
    draw.Draw(new png_test_package.image_NRGBAжImage(ret), b, m, b.Min, draw.Src);
    return ret;
}

public static void TestWriter(ж<testing.T> Ꮡt) {
    // The filenames variable is declared in reader_test.go.
    var names = filenames;
    if (testing.Short()) {
        names = filenamesShort;
    }
    foreach (var (_, fn) in names) {
        @string qfn = "testdata/pngsuite/"u8 + fn + ".png"u8;
        // Read the image.
        var (m0, err) = readPNG(qfn);
        if (err != default!) {
            Ꮡt.Error(fn, err);
            continue;
        }
        // Read the image again, encode it, and decode it.
        (var m1, err) = readPNG(qfn);
        if (err != default!) {
            Ꮡt.Error(fn, err);
            continue;
        }
        (var m2, err) = encodeDecode(m1);
        if (err != default!) {
            Ꮡt.Error(fn, err);
            continue;
        }
        // Compare the two.
        err = diff(m0, m2);
        if (err != default!) {
            Ꮡt.Error(fn, err);
            continue;
        }
    }
}

internal partial struct TestWriterPaletted_testCases /*dyn*/ {
    internal nint plen;
    internal uint8 bitdepth;
    internal nint datalen;
}

public static void TestWriterPaletted(ж<testing.T> Ꮡt) {
    UntypedInt width = 32;
    UntypedInt height = 16;
    var testCases = new TestWriterPaletted_testCases[]{
        new(
            plen: 256,
            bitdepth: 8,
            datalen: (1 + width) * height
        ),
        new(
            plen: 128,
            bitdepth: 8,
            datalen: (1 + width) * height
        ),
        new(
            plen: 16,
            bitdepth: 4,
            datalen: (1 + width / 2) * height
        ),
        new(
            plen: 4,
            bitdepth: 2,
            datalen: (1 + width / 4) * height
        ),
        new(
            plen: 2,
            bitdepth: 1,
            datalen: (1 + width / 8) * height
        )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestWriterPaletted_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(fmt.Sprintf("plen-%d"u8, tc.plen), (ж<testing.T> tΔ1) => {
            // Create a paletted image with the correct palette length
            var palette = new color.Palette(tcʗ1.plen);
            foreach (var (iΔ1, _) in palette) {
                palette[iΔ1] = new color.NRGBA(
                    R: (uint8)iΔ1,
                    G: (uint8)iΔ1,
                    B: (uint8)iΔ1,
                    A: 255
                );
            }
            var m0 = image.NewPaletted(image.Rect(0, 0, width, height), palette);
            nint i = 0;
            for (nint y = 0; y < height; y++) {
                for (nint x = 0; x < width; x++) {
                    m0.SetColorIndex(x, y, (uint8)(rem(i, tcʗ1.plen)));
                    i++;
                }
            }
            // Encode the image
            ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
            {
                var err = Encode(new png_test_package.bytes_BufferжWriter(Ꮡb), new image.PalettedжImage(m0)); if (err != default!) {
                    tΔ1.Error(err);
                    return;
                }
            }
            const nint chunkFieldsLength = 12; // 4 bytes for length, name and crc
            var data = b.Bytes();
            i = len(pngHeader);
            while (i < len(data) - chunkFieldsLength) {
                var length = binary.BigEndian.Uint32(data.slice(i, i + 4));
                @string name = ((@string)(data.slice(i + 4, i + 8)));
                var exprᴛ1 = name;
                if (exprᴛ1 == "IHDR"u8) {
                    var bitdepth = data[i + 8 + 8];
                    if (bitdepth != tcʗ1.bitdepth) {
                        tΔ1.Errorf("got bitdepth %d, want %d"u8, bitdepth, tcʗ1.bitdepth);
                    }
                }
                else if (exprᴛ1 == "IDAT"u8) {
                    var (r, err) = zlib.NewReader(new png_test_package.bytes_ReaderжReader(bytes.NewReader(data.slice(i + 8, i + 8 + (nint)length))));
                    if (err != default!) {
                        // Uncompress the image data
                        tΔ1.Error(err);
                        return;
                    }
                    (var n, err) = io.Copy(io.Discard, r);
                    if (err != default!) {
                        tΔ1.Errorf("got error while reading image data: %v"u8, err);
                    }
                    if (n != (int64)tcʗ1.datalen) {
                        tΔ1.Errorf("got uncompressed data length %d, want %d"u8, n, tcʗ1.datalen);
                    }
                }

                i += chunkFieldsLength + (nint)length;
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object defaultCompressionˢ = (@string)"DefaultCompression encoding was larger than NoCompression encoding"u8;
internal static readonly object cannotDecodeˢ = (@string)"cannot decode DefaultCompression"u8;
internal static readonly object cannotDecodeˢ2 = (@string)"cannot decode NoCompression"u8;

public static void TestWriterLevels(ж<testing.T> Ꮡt) {
    var m = image.NewNRGBA(image.Rect(0, 0, 100, 100));
    ref var b1 = ref heap(new bytes.Buffer(), out var Ꮡb1);
    ref var b2 = ref heap(new bytes.Buffer(), out var Ꮡb2);
    {
        var err = (Ꮡ(new Encoder(nil))).Encode(new png_test_package.bytes_BufferжWriter(Ꮡb1), new image.NRGBAжImage(m)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var noenc = Ꮡ(new Encoder(CompressionLevel: NoCompression));
    {
        var err = noenc.Encode(new png_test_package.bytes_BufferжWriter(Ꮡb2), new image.NRGBAжImage(m)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    if (b2.Len() <= b1.Len()) {
        Ꮡt.Error(defaultCompressionˢ);
    }
    {
        var (_, err) = Decode(new png_test_package.bytes_BufferжReader(Ꮡb1)); if (err != default!) {
            Ꮡt.Error(cannotDecodeˢ);
        }
    }
    {
        var (_, err) = Decode(new png_test_package.bytes_BufferжReader(Ꮡb2)); if (err != default!) {
            Ꮡt.Error(cannotDecodeˢ2);
        }
    }
}

public static void TestSubImage(ж<testing.T> Ꮡt) {
    var m0 = image.NewRGBA(image.Rect(0, 0, 256, 256));
    for (nint y = 0; y < 256; y++) {
        for (nint x = 0; x < 256; x++) {
            m0.Set(x, y, new colorꓸRGBA((uint8)x, (uint8)y, 0, 255));
        }
    }
    m0 = m0.SubImage(image.Rect(50, 30, 250, 130))._<ж<imageꓸRGBA>>();
    var (m1, err) = encodeDecode(new image.ΔRGBAжImage(m0));
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    err = diff(new image.ΔRGBAжImage(m0), m1);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
}

internal partial struct TestWriteRGBA_testCases /*dyn*/ {
    internal @string name;
    internal image.Image img;
}

public static void TestWriteRGBA(ж<testing.T> Ꮡt) {
    const nint width = 640;
    const nint height = 480;
    var transparentImg = image.NewRGBA(image.Rect(0, 0, width, height));
    var opaqueImg = image.NewRGBA(image.Rect(0, 0, width, height));
    var mixedImg = image.NewRGBA(image.Rect(0, 0, width, height));
    var translucentImg = image.NewRGBA(image.Rect(0, 0, width, height));
    for (nint y = 0; y < height; y++) {
        for (nint x = 0; x < width; x++) {
            var opaqueColor = new colorꓸRGBA((uint8)x, (uint8)y, (uint8)(y + x), 255);
            var translucentColor = new colorꓸRGBA((uint8)((uint8)x % 128), (uint8)((uint8)y % 128), (uint8)((uint8)(y + x) % 128), 128);
            opaqueImg.Set(x, y, opaqueColor);
            translucentImg.Set(x, y, translucentColor);
            if (y % 2 == 0) {
                mixedImg.Set(x, y, opaqueColor);
            }
        }
    }
    var testCases = new TestWriteRGBA_testCases[]{
        new("Transparent RGBA"u8, new image.ΔRGBAжImage(transparentImg)),
        new("Opaque RGBA"u8, new image.ΔRGBAжImage(opaqueImg)),
        new("50/50 Transparent/Opaque RGBA"u8, new image.ΔRGBAжImage(mixedImg)),
        new("RGBA with variable alpha"u8, new image.ΔRGBAжImage(translucentImg))
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestWriteRGBA_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var m0 = tcʗ1.img;
            var (m1, err) = encodeDecode(m0);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            err = diff(new image.NRGBAжImage(convertToNRGBA(m0)), m1);
            if (err != default!) {
                tΔ1.Error(err);
            }
        });
    }
}

public static void BenchmarkEncodeGray(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewGray(image.Rect(0, 0, 640, 480));
    b.SetBytes(640 * 480 * 1);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.GrayжImage(img));
    }
}

internal partial struct pool {
    internal ж<global::go.image.png_package.EncoderBuffer> b;
}

internal static ж<global::go.image.png_package.EncoderBuffer> Get(this ref pool p) {
    return p.b;
}

internal static void Put(this ref pool p, ж<global::go.image.png_package.EncoderBuffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    p.b = Ꮡb;
}

public static void BenchmarkEncodeGrayWithBufferPool(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewGray(image.Rect(0, 0, 640, 480));
    ref var e = ref heap<global::go.image.png_package.Encoder>(out var Ꮡe);
    e = new Encoder(
        BufferPool: new png_internal_test_package.poolжEncoderBufferPool(Ꮡ(new pool(nil)))
    );
    b.SetBytes(640 * 480 * 1);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Ꮡe.Encode(io.Discard, new image.GrayжImage(img));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedImageToBeOpaqueˢ = (@string)"expected image to be opaque"u8;

public static void BenchmarkEncodeNRGBOpaque(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewNRGBA(image.Rect(0, 0, 640, 480));
    // Set all pixels to 0xFF alpha to force opaque mode.
    var bo = img.Bounds();
    for (nint y = bo.Min.Y; y < bo.Max.Y; y++) {
        for (nint x = bo.Min.X; x < bo.Max.X; x++) {
            img.Set(x, y, new color.NRGBA(0, 0, 0, 255));
        }
    }
    if (!img.Opaque()) {
        Ꮡb.Fatal(expectedImageToBeOpaqueˢ);
    }
    b.SetBytes(640 * 480 * 4);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.NRGBAжImage(img));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedImageNotToBeˢ = (@string)"expected image not to be opaque"u8;

public static void BenchmarkEncodeNRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewNRGBA(image.Rect(0, 0, 640, 480));
    if (img.Opaque()) {
        Ꮡb.Fatal(expectedImageNotToBeˢ);
    }
    b.SetBytes(640 * 480 * 4);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.NRGBAжImage(img));
    }
}

public static void BenchmarkEncodePaletted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewPaletted(image.Rect(0, 0, 640, 480), new color.Palette(new color.Color[]{new colorꓸRGBA(0, 0, 0, 255), new colorꓸRGBA(255, 255, 255, 255)
    }.slice()));
    b.SetBytes(640 * 480 * 1);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.PalettedжImage(img));
    }
}

public static void BenchmarkEncodeRGBOpaque(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewRGBA(image.Rect(0, 0, 640, 480));
    // Set all pixels to 0xFF alpha to force opaque mode.
    var bo = img.Bounds();
    for (nint y = bo.Min.Y; y < bo.Max.Y; y++) {
        for (nint x = bo.Min.X; x < bo.Max.X; x++) {
            img.Set(x, y, new colorꓸRGBA(0, 0, 0, 255));
        }
    }
    if (!img.Opaque()) {
        Ꮡb.Fatal(expectedImageToBeOpaqueˢ);
    }
    b.SetBytes(640 * 480 * 4);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.ΔRGBAжImage(img));
    }
}

public static void BenchmarkEncodeRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt width = 640;
    UntypedInt height = 480;
    var img = image.NewRGBA(image.Rect(0, 0, width, height));
    for (nint y = 0; y < height; y++) {
        for (nint x = 0; x < width; x++) {
            nint percent = (x + y) % 100;
            switch (ᐧ) {
            case {} when percent is < 10: {
                img.Set(x, // 10% of pixels are translucent (have alpha >0 and <255)
 y, new color.NRGBA((uint8)x, (uint8)y, (uint8)(x * y), (uint8)percent));
                break;
            }
            case {} when percent is < 40: {
                img.Set(x, // 30% of pixels are transparent (have alpha == 0)
 y, new color.NRGBA((uint8)x, (uint8)y, (uint8)(x * y), 0));
                break;
            }
            default: {
                img.Set(x, // 60% of pixels are opaque (have alpha == 255)
 y, new color.NRGBA((uint8)x, (uint8)y, (uint8)(x * y), 255));
                break;
            }}

        }
    }
    if (img.Opaque()) {
        Ꮡb.Fatal(expectedImageNotToBeˢ);
    }
    b.SetBytes(width * height * 4);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.ΔRGBAжImage(img));
    }
}

} // end png_internal_test_package
