// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using lzw = compress.lzw_package;
using hex = encoding.hex_package;
using image = image_package;
using Δcolor = go.image.color_package;
using palette = go.image.color.palette_package;
using io = io_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using testing = testing_package;
using compress;
using encoding;
using go.image;
using go.image.color;
using go.runtime;
using static go.image.gif_package;

partial class gif_internal_test_package {

// width=2, height=1
// header, palette and trailer are parts of a valid 2x1 GIF image.
internal static readonly @string headerStr = ((@string)(new byte[]{0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x02, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00})); // headerFields=(a color table of 2 pixels), backgroundIndex, aspect

internal static readonly @string paletteStr = "\x10\x20\x30\x40\x50\x60"u8; // the color table, also known as a palette

internal static readonly @string trailerStr = "\x3b"u8;

// lzw.NewReader wants an io.ByteReader, this ensures we're compatible.
internal static io.ByteReader _ᴛ1ʗ = new gif_internal_test_package.gif_blockReaderжByteReader(((ж<global::go.image.gif_package.blockReader>)nil));

// lzwEncode returns an LZW encoding (with 2-bit literals) of in.
internal static slice<byte> lzwEncode(slice<byte> @in) {
    var b = Ꮡ(new bytes.Buffer(nil));
    var w = lzw.NewWriter(new gif_internal_test_package.bytes_BufferжWriter(b), lzw.LSB, 2);
    {
        var (_, err) = w.Write(@in); if (err != default!) {
            throw panic(err);
        }
    }
    {
        var err = w.Close(); if (err != default!) {
            throw panic(err);
        }
    }
    return b.Bytes();
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string extraᶜ = "\x02\x02\x02\x02"u8;

[GoType("dyn")] internal partial struct TestDecode_testCases {
    internal nint nPix; // The number of pixels in the image data.
    // If non-zero, write this many extra bytes inside the data sub-block
    // containing the LZW end code.
    internal nint extraExisting;
    // If non-zero, write an extra block of this many bytes.
    internal nint extraSeparate;
    internal error wantErr;
}

public static void TestDecode(ж<testing.T> Ꮡt) {
    // extra contains superfluous bytes to inject into the GIF, either at the end
    // of an existing data sub-block (past the LZW End of Information code) or in
    // a separate data sub-block. The 0x02 values are arbitrary.
    @string extra = extraᶜ;
    var testCases = new TestDecode_testCases[]{
        new(0, 0, 0, errNotEnough),
        new(1, 0, 0, errNotEnough),
        new(2, 0, 0, default!), // An extra data sub-block after the compressed section with 1 byte which we
 // silently skip.

        new(2, 0, 1, default!), // An extra data sub-block after the compressed section with 2 bytes. In
 // this case we complain that there is too much data.

        new(2, 0, 2, errTooMuch), // Too much pixel data.

        new(3, 0, 0, errTooMuch), // An extra byte after LZW data, but inside the same data sub-block.

        new(2, 1, 0, default!), // Two extra bytes after LZW data, but inside the same data sub-block.

        new(2, 2, 0, default!), // Extra data exists in the final sub-block with LZW data, AND there is
 // a bogus sub-block following.

        new(2, 1, 1, errTooMuch)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var b = Ꮡ(new bytes.Buffer(nil));
        b.WriteString(headerStr);
        b.WriteString(paletteStr);
        // Write an image with bounds 2x1 but tc.nPix pixels. If tc.nPix != 2
        // then this should result in an invalid GIF image. First, write a
        // magic 0x2c (image descriptor) byte, bounds=(0,0)-(2,1), a flags
        // byte, and 2-bit LZW literals.
        b.WriteString("\x2c\x00\x00\x00\x00\x02\x00\x01\x00\x00\x02"u8);
        if (tc.nPix > 0) {
            var enc = lzwEncode(new slice<byte>(tc.nPix));
            if (len(enc) + tc.extraExisting > 0xff) {
                Ꮡt.Errorf("nPix=%d, extraExisting=%d, extraSeparate=%d: compressed length %d is too large"u8,
                    tc.nPix, tc.extraExisting, tc.extraSeparate, len(enc));
                continue;
            }
            // Write the size of the data sub-block containing the LZW data.
            b.WriteByte((byte)(len(enc) + tc.extraExisting));
            // Write the LZW data.
            b.Write(enc);
            // Write extra bytes inside the same data sub-block where LZW data
            // ended. Each arbitrarily 0x02.
            b.WriteString(extra.slice(0, tc.extraExisting));
        }
        if (tc.extraSeparate > 0) {
            // Data sub-block size. This indicates how many extra bytes follow.
            b.WriteByte((byte)tc.extraSeparate);
            b.WriteString(extra.slice(0, tc.extraSeparate));
        }
        b.WriteByte(0x00); // An empty block signifies the end of the image data.
        b.WriteString(trailerStr);
        var (got, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(b));
        if (!AreEqual(err, tc.wantErr)) {
            Ꮡt.Errorf("nPix=%d, extraExisting=%d, extraSeparate=%d\ngot  %v\nwant %v"u8,
                tc.nPix, tc.extraExisting, tc.extraSeparate, err, tc.wantErr);
        }
        if (tc.wantErr != default!) {
            continue;
        }
        var want = Ꮡ(new image.Paletted(
            Pix: new uint8[]{0, 0}.slice(),
            Stride: 2,
            Rect: image.Rect(0, 0, 2, 1),
            Palette: new Δcolor.Palette(new Δcolor.Color[]{new colorꓸRGBA(0x10, 0x20, 0x30, 0xff), new colorꓸRGBA(0x40, 0x50, 0x60, 0xff)
            }.slice())
        ));
        if (!reflect.DeepEqual(got, want.OrTypedNil())) {
            Ꮡt.Errorf("nPix=%d, extraExisting=%d, extraSeparate=%d\ngot  %v\nwant %v"u8,
                tc.nPix, tc.extraExisting, tc.extraSeparate, got, want.OrTypedNil());
        }
    }
}

public static void TestTransparentIndex(ж<testing.T> Ꮡt) {
    var b = Ꮡ(new bytes.Buffer(nil));
    b.WriteString(headerStr);
    b.WriteString(paletteStr);
    for (nint transparentIndex = 0; transparentIndex < 3; transparentIndex++) {
        if (transparentIndex < 2) {
            // Write the graphic control for the transparent index.
            b.WriteString(((@string)(new byte[]{0x21, 0xf9, 0x04, 0x01, 0x00, 0x00})));
            b.WriteByte((byte)transparentIndex);
            b.WriteByte(0);
        }
        // Write an image with bounds 2x1, as per TestDecode.
        b.WriteString("\x2c\x00\x00\x00\x00\x02\x00\x01\x00\x00\x02"u8);
        var enc = lzwEncode(new byte[]{0x00, 0x00}.slice());
        if (len(enc) > 0xff) {
            Ꮡt.Fatalf("compressed length %d is too large"u8, len(enc));
        }
        b.WriteByte((byte)len(enc));
        b.Write(enc);
        b.WriteByte(0x00);
    }
    b.WriteString(trailerStr);
    var (g, err) = DecodeAll(new gif_internal_test_package.bytes_BufferжReader(b));
    if (err != default!) {
        Ꮡt.Fatalf("DecodeAll: %v"u8, err);
    }
    var c0 = new colorꓸRGBA(paletteStr[0], paletteStr[1], paletteStr[2], 0xff);
    var c1 = new colorꓸRGBA(paletteStr[3], paletteStr[4], paletteStr[5], 0xff);
    var cz = new colorꓸRGBA(nil);
    var wants = new Δcolor.Palette[]{
        new Δcolor.Color[]{cz, c1}.slice(),
        new Δcolor.Color[]{c0, cz}.slice(),
        new Δcolor.Color[]{c0, c1}.slice()
    }.slice();
    if (len((~g).Image) != len(wants)) {
        Ꮡt.Fatalf("got %d images, want %d"u8, len((~g).Image), len(wants));
    }
    foreach (var (i, want) in wants) {
        var got = (~g).Image[i].Value.Palette;
        if (!reflect.DeepEqual(got, want)) {
            Ꮡt.Errorf("palette #%d:\ngot  %v\nwant %v"u8, i, got, want);
        }
    }
}

// w=1, h=1 (6)
// headerFields, bg, aspect (10)
// color table and graphics control (13)
// (19)
// frame 1 (0,0 - 1,1)
// (32)
// lzw pixels
// trailer
// testGIF is a simple GIF that we can modify to test different scenarios.
internal static slice<byte> testGIF = new byte[]{
    (rune)'G', (rune)'I', (rune)'F', (rune)'8', (rune)'9', (rune)'a',
    1, 0, 1, 0,
    128, 0, 0,
    0, 0, 0, 1, 1, 1,
    0x21, 0xf9, 0x04, 0x00, 0x00, 0x00, 0xff, 0x00,
    0x2c,
    0x00, 0x00, 0x00, 0x00,
    0x01, 0x00, 0x01, 0x00,
    0x00,
    0x02, 0x02, 0x4c, 0x01, 0x00,
    0x3b
}.slice();

internal static void @try(ж<testing.T> Ꮡt, slice<byte> b, @string want) {
    var (_, err) = DecodeAll(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
    @string got = default!;
    if (err != default!) {
        got = err.Error();
    }
    if (got != want) {
        Ꮡt.Fatalf("got %v, want %v"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifTooMuchImageDataˢ = "gif: too much image data"u8;

public static void TestBounds(ж<testing.T> Ꮡt) {
    // Make a local copy of testGIF.
    var gif = new slice<byte>(len(testGIF));
    copy(gif, testGIF);
    // Make the bounds too big, just by one.
    gif[32] = 2;
    @string want = gifFrameBoundsLargerThanˢ;
    @try(Ꮡt, gif, want);
    // Make the bounds too small; does not trigger bounds
    // check, but now there's too much data.
    gif[32] = 0;
    want = gifTooMuchImageDataˢ;
    @try(Ꮡt, gif, want);
    gif[32] = 1;
    // Make the bounds really big, expect an error.
    want = gifFrameBoundsLargerThanˢ;
    for (nint i = 0; i < 4; i++) {
        gif[32 + i] = 0xff;
    }
    @try(Ꮡt, gif, want);
}

public static void TestNoPalette(ж<testing.T> Ꮡt) {
    var b = Ꮡ(new bytes.Buffer(nil));
    // Manufacture a GIF with no palette, so any pixel at all
    // will be invalid.
    b.WriteString(headerStr[..(int)(len(headerStr) - 3)]);
    b.WriteString("\x00\x00\x00"u8); // No global palette.
    // Image descriptor: 2x1, no local palette, and 2-bit LZW literals.
    b.WriteString("\x2c\x00\x00\x00\x00\x02\x00\x01\x00\x00\x02"u8);
    // Encode the pixels: neither is in range, because there is no palette.
    var enc = lzwEncode(new byte[]{0x00, 0x03}.slice());
    b.WriteByte((byte)len(enc));
    b.Write(enc);
    b.WriteByte(0x00); // An empty block signifies the end of the image data.
    b.WriteString(trailerStr);
    @try(Ꮡt, b.Bytes(), gifNoColorTableˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifInvalidPixelValueˢ = "gif: invalid pixel value"u8;

public static void TestPixelOutsidePaletteRange(ж<testing.T> Ꮡt) {
    foreach (var (_, pval) in new byte[]{0, 1, 2, 3}.slice()) {
        var b = Ꮡ(new bytes.Buffer(nil));
        // Manufacture a GIF with a 2 color palette.
        b.WriteString(headerStr);
        b.WriteString(paletteStr);
        // Image descriptor: 2x1, no local palette, and 2-bit LZW literals.
        b.WriteString("\x2c\x00\x00\x00\x00\x02\x00\x01\x00\x00\x02"u8);
        // Encode the pixels; some pvals trigger the expected error.
        var enc = lzwEncode(new byte[]{pval, pval}.slice());
        b.WriteByte((byte)len(enc));
        b.Write(enc);
        b.WriteByte(0x00); // An empty block signifies the end of the image data.
        b.WriteString(trailerStr);
        // No error expected, unless the pixels are beyond the 2 color palette.
        @string want = ""u8;
        if (pval >= 2) {
            want = gifInvalidPixelValueˢ;
        }
        @try(Ꮡt, b.Bytes(), want);
    }
}

public static void TestTransparentPixelOutsidePaletteRange(ж<testing.T> Ꮡt) {
    var b = Ꮡ(new bytes.Buffer(nil));
    // Manufacture a GIF with a 2 color palette.
    b.WriteString(headerStr);
    b.WriteString(paletteStr);
    // Graphic Control Extension: transparency, transparent color index = 3.
    //
    // This index, 3, is out of range of the global palette and there is no
    // local palette in the subsequent image descriptor. This is an error
    // according to the spec, but Firefox and Google Chrome seem OK with this.
    //
    // See golang.org/issue/15059.
    b.WriteString(((@string)(new byte[]{0x21, 0xf9, 0x04, 0x01, 0x00, 0x00, 0x03, 0x00})));
    // Image descriptor: 2x1, no local palette, and 2-bit LZW literals.
    b.WriteString("\x2c\x00\x00\x00\x00\x02\x00\x01\x00\x00\x02"u8);
    // Encode the pixels.
    var enc = lzwEncode(new byte[]{0x03, 0x03}.slice());
    b.WriteByte((byte)len(enc));
    b.Write(enc);
    b.WriteByte(0x00); // An empty block signifies the end of the image data.
    b.WriteString(trailerStr);
    @try(Ꮡt, b.Bytes(), ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeAllˢ = (@string)"DecodeAll:"u8;
internal static readonly object encodeAllˢ = (@string)"EncodeAll:"u8;

[GoType("dyn")] internal partial struct TestLoopCount_testCases {
    internal @string name;
    internal slice<byte> data;
    internal nint loopCount;
}

public static void TestLoopCount(ж<testing.T> Ꮡt) {
    var testCases = new TestLoopCount_testCases[]{
        new(
            "loopcount-missing"u8,
            slice<byte>(((@string)(new byte[]{0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x30, 0x30, 0x30, 0x00, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x2c, 0x30, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x80, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x02, 0x08, 0xf0, 0x31, 0x75, 0xb9, 0xfd, 0x61, 0x6c, 0x05, 0x00, 0x3b}))), // image 0 descriptor & color table
 // image 0 image data & trailer

            -1
        ),
        new(
            "loopcount-0"u8,
            slice<byte>(((@string)(new byte[]{0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x30, 0x30, 0x30, 0x00, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x21, 0xff, 0x0b, 0x4e, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2e, 0x30, 0x03, 0x01, 0x00, 0x00, 0x00})) + ((@string)(new byte[]{0x2c, 0x30, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x80, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x02, 0x08, 0xf0, 0x31, 0x75, 0xb9, 0xfd, 0x61, 0x6c, 0x05, 0x00})) + ((@string)(new byte[]{0x2c, 0x30, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x80, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x02, 0x08, 0xf0, 0x31, 0x75, 0xb9, 0xfd, 0x61, 0x6c, 0x05, 0x00, 0x3b}))), // loop count = 0
 // image 0 descriptor & color table
 // image 0 image data
 // image 1 descriptor & color table
 // image 1 image data & trailer

            0
        ),
        new(
            "loopcount-1"u8,
            slice<byte>(((@string)(new byte[]{0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x30, 0x30, 0x30, 0x00, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x21, 0xff, 0x0b, 0x4e, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2e, 0x30, 0x03, 0x01, 0x01, 0x00, 0x00})) + ((@string)(new byte[]{0x2c, 0x30, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x80, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x02, 0x08, 0xf0, 0x31, 0x75, 0xb9, 0xfd, 0x61, 0x6c, 0x05, 0x00})) + ((@string)(new byte[]{0x2c, 0x30, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x80, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x02, 0x08, 0xf0, 0x31, 0x75, 0xb9, 0xfd, 0x61, 0x6c, 0x05, 0x00, 0x3b}))), // loop count = 1
 // image 0 descriptor & color table
 // image 0 image data
 // image 1 descriptor & color table
 // image 1 image data & trailer

            1
        )
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestLoopCount_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (img, err) = DecodeAll(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(tcʗ1.data)));
            if (err != default!) {
                tΔ1.Fatal(decodeAllˢ, err);
            }
            var w = @new<bytes.Buffer>();
            err = EncodeAll(new gif_internal_test_package.bytes_BufferжWriter(w), img);
            if (err != default!) {
                tΔ1.Fatal(encodeAllˢ, err);
            }
            (var img1, err) = DecodeAll(new gif_internal_test_package.bytes_BufferжReader(w));
            if (err != default!) {
                tΔ1.Fatal(decodeAllˢ, err);
            }
            if ((~img).LoopCount != tcʗ1.loopCount) {
                tΔ1.Errorf("loop count mismatch: %d vs %d"u8, (~img).LoopCount, tcʗ1.loopCount);
            }
            if ((~img).LoopCount != (~img1).LoopCount) {
                tΔ1.Errorf("loop count failed round-trip: %d vs %d"u8, (~img).LoopCount, (~img1).LoopCount);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifˢ2 = "gif:"u8;
internal static readonly @string unexpectedEofˢ = ": unexpected EOF"u8;

public static void TestUnexpectedEOF(ж<testing.T> Ꮡt) {
    for (nint i = len(testGIF) - 1; i >= 0; i--) {
        var (_, err) = DecodeAll(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(testGIF.slice(0, i))));
        if (AreEqual(err, errNotEnough)) {
            continue;
        }
        @string text = ""u8;
        if (err != default!) {
            text = err.Error();
        }
        if (!strings.HasPrefix(text, gifˢ2) || !strings.HasSuffix(text, unexpectedEofˢ)) {
            Ꮡt.Errorf("Decode(testGIF[:%d]) = %v, want gif: ...: unexpected EOF"u8, i, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeˢ = (@string)"Decode:"u8;

// See golang.org/issue/22237
public static void TestDecodeMemoryConsumption(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        const nint frames = 3000;
        var img = image.NewPaletted(new image.Rectangle(Max: new image.Point(1, 1)), palette.WebSafe);
        var hugeGIF = Ꮡ(new GIF(
            Image: new slice<ж<image.Paletted>>(frames),
            Delay: new slice<nint>(frames),
            Disposal: new slice<byte>(frames)
        ));
        for (nint i = 0; i < frames; i++) {
            hugeGIF.Value.Image[i] = img;
            hugeGIF.Value.Delay[i] = 60;
        }
        var buf = @new<bytes.Buffer>();
        {
            var err = EncodeAll(new gif_internal_test_package.bytes_BufferжWriter(buf), hugeGIF); if (err != default!) {
                Ꮡt.Fatal(encodeAllˢ, err);
            }
        }
        var (s0, s1) = (@new<runtime.MemStats>(), @new<runtime.MemStats>());
        runtime.GC();
        defer(debug.SetGCPercent, debug.SetGCPercent(5), ref ᒐ);
        runtime.ReadMemStats(s0);
        {
            var (_, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(buf)); if (err != default!) {
                Ꮡt.Fatal(decodeˢ, err);
            }
        }
        runtime.ReadMemStats(s1);
        {
            var heapDiff = (int64)((~s1).HeapAlloc - (~s0).HeapAlloc); if (heapDiff > ((int64)30 << (int)(20))) {
                Ꮡt.Fatalf("Decode of %d frames increased heap by %dMB"u8, (nint)(frames), (heapDiff >> (int)(20)));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo001Gifˢ = "../testdata/video-001.gif"u8;

public static void BenchmarkDecode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (data, err) = os.ReadFile(testdataVideo001Gifˢ);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    (var cfg, err) = DecodeConfig(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    b.SetBytes((int64)(cfg.Width * cfg.Height));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Decode(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    }
}

public static void TestReencodeExtendedPalette(ж<testing.T> Ꮡt) {
    var (data, err) = hex.DecodeString("4749463839616c02020157220221ff0b280154ffffffff00000021474946306127dc213000ff84ff840000000000800021ffffffff8f4e4554530041508f8f0202020000000000000000000000000202020202020207020202022f31050000000000000021f904ab2c3826002c00000000c00001009800462b07fc1f02061202020602020202220202930202020202020202020202020286090222202222222222222222222222222222222222222222222222222220222222222222222222222222222222222222222222222222221a22222222332223222222222222222222222222222222222222224b222222222222002200002b474946312829021f0000000000cbff002f0202073121f904ab2c2c000021f92c3803002c00e0c0000000f932"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var img, err) = Decode(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = Encode(io.Discard, img, Ꮡ(new Options(NumColors: 1)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

} // end gif_internal_test_package
