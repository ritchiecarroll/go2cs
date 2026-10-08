// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package png implements a PNG image decoder and encoder.
//
// The PNG specification is at https://www.w3.org/TR/PNG/.
namespace go.image;

using zlib = compress.zlib_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using hash = hash_package;
using crc32 = go.hash.crc32_package;
using image = image_package;
using color = go.image.color_package;
using io = io_package;
using compress;
using encoding;
using go.hash;
using go.image;

partial class png_package {

// Color type, as per the PNG spec.
internal static UntypedInt ctGrayscale => 0;

internal static UntypedInt ctTrueColor => 2;

internal static UntypedInt ctPaletted => 3;

internal static UntypedInt ctGrayscaleAlpha => 4;

internal static UntypedInt ctTrueColorAlpha => 6;

// A cb is a combination of color type and bit depth.
internal static UntypedInt cbInvalid => iota;

internal static UntypedInt cbG1 => 1;

internal static UntypedInt cbG2 => 2;

internal static UntypedInt cbG4 => 3;

internal static UntypedInt cbG8 => 4;

internal static UntypedInt cbGA8 => 5;

internal static UntypedInt cbTC8 => 6;

internal static UntypedInt cbP1 => 7;

internal static UntypedInt cbP2 => 8;

internal static UntypedInt cbP4 => 9;

internal static UntypedInt cbP8 => 10;

internal static UntypedInt cbTCA8 => 11;

internal static UntypedInt cbG16 => 12;

internal static UntypedInt cbGA16 => 13;

internal static UntypedInt cbTC16 => 14;

internal static UntypedInt cbTCA16 => 15;

internal static bool cbPaletted(nint cb) {
    return cbP1 <= cb && cb <= cbP8;
}

internal static bool cbTrueColor(nint cb) {
    return cb == cbTC8 || cb == cbTC16;
}

// Filter type, as per the PNG spec.
internal static UntypedInt ftNone => 0;

internal static UntypedInt ftSub => 1;

internal static UntypedInt ftUp => 2;

internal static UntypedInt ftAverage => 3;

internal static UntypedInt ftPaeth => 4;

internal static UntypedInt nFilter => 5;

// Interlace type.
internal static UntypedInt itNone => 0;

internal static UntypedInt itAdam7 => 1;

// interlaceScan defines the placement and size of a pass for Adam7 interlacing.
partial struct interlaceScan {
    internal nint xFactor, yFactor, xOffset, yOffset;
}

// interlacing defines Adam7 interlacing, with 7 passes of reduced images.
// See https://www.w3.org/TR/PNG/#8Interlace
internal static slice<interlaceScan> interlacing = new interlaceScan[]{
    new(8, 8, 0, 0),
    new(8, 8, 4, 0),
    new(4, 8, 0, 4),
    new(4, 4, 2, 0),
    new(2, 4, 0, 2),
    new(2, 2, 1, 0),
    new(1, 2, 0, 1)
}.slice();

// Decoding stage.
// The PNG specification says that the IHDR, PLTE (if present), tRNS (if
// present), IDAT and IEND chunks must appear in that order. There may be
// multiple IDAT chunks, and IDAT chunks must be sequential (i.e. they may not
// have any other chunks between them).
// https://www.w3.org/TR/PNG/#5ChunkOrdering
internal static UntypedInt dsStart => iota;

internal static UntypedInt dsSeenIHDR => 1;

internal static UntypedInt dsSeenPLTE => 2;

internal static UntypedInt dsSeentRNS => 3;

internal static UntypedInt dsSeenIDAT => 4;

internal static UntypedInt dsSeenIEND => 5;

internal static readonly @string pngHeader = ((@string)(new byte[]{0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a}));

partial struct decoder {
    internal io.Reader r;
    internal image.Image img;
    internal hash.Hash32 crc;
    internal nint width, height;
    internal nint depth;
    internal color.Palette palette;
    internal nint cb;
    internal nint stage;
    internal uint32 idatLength;
    internal array<byte> tmp = new(3 * 256);
    internal nint interlace;
    // useTransparent and transparent are used for grayscale and truecolor
    // transparency, as opposed to palette transparency.
    internal bool useTransparent;
    internal array<byte> transparent = new(6);
}

partial struct FormatError /*@string*/;

public static @string Error(this FormatError e) {
    return "png: invalid format: "u8 + ((@string)e);
}

internal static FormatError chunkOrderError = ((FormatError)(@string)"chunk out of order"u8);

partial struct UnsupportedError /*@string*/;

public static @string Error(this UnsupportedError e) {
    return "png: unsupported feature: "u8 + ((@string)e);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badIhdrLengthˢ = "bad IHDR length"u8;
internal static readonly @string compressionMethodˢ = "compression method"u8;
internal static readonly @string filterMethodˢ = "filter method"u8;
internal static readonly @string invalidInterlaceMethodˢ = "invalid interlace method"u8;
internal static readonly @string nonPositiveDimensionˢ = "non-positive dimension"u8;
internal static readonly @string dimensionOverflowˢ = "dimension overflow"u8;

internal static error parseIHDR(this ref decoder d, uint32 length) {
    if (length != 13) {
        return ((FormatError)(@string)badIhdrLengthˢ);
    }
    {
        var (_, err) = io.ReadFull(d.r, d.tmp[..13]); if (err != default!) {
            return err;
        }
    }
    d.crc.Write(d.tmp[..13]);
    if (d.tmp[10] != 0) {
        return ((UnsupportedError)(@string)compressionMethodˢ);
    }
    if (d.tmp[11] != 0) {
        return ((UnsupportedError)(@string)filterMethodˢ);
    }
    if (d.tmp[12] != itNone && d.tmp[12] != itAdam7) {
        return ((FormatError)(@string)invalidInterlaceMethodˢ);
    }
    d.interlace = (nint)d.tmp[12];
    var w = (int32)binary.BigEndian.Uint32(d.tmp[0..4]);
    var h = (int32)binary.BigEndian.Uint32(d.tmp[4..8]);
    if (w <= 0 || h <= 0) {
        return ((FormatError)(@string)nonPositiveDimensionˢ);
    }
    var nPixels64 = (int64)w * (int64)h;
    nint nPixels = (nint)nPixels64;
    if (nPixels64 != (int64)nPixels) {
        return ((UnsupportedError)(@string)dimensionOverflowˢ);
    }
    // There can be up to 8 bytes per pixel, for 16 bits per channel RGBA.
    if (nPixels != (nPixels * 8) / 8) {
        return ((UnsupportedError)(@string)dimensionOverflowˢ);
    }
    d.cb = cbInvalid;
    d.depth = (nint)d.tmp[8];
    switch (d.depth) {
    case 1: {
        var exprᴛ1 = d.tmp[9];
        if (exprᴛ1 == ctGrayscale) {
            d.cb = cbG1;
        }
        else if (exprᴛ1 == ctPaletted) {
            d.cb = cbP1;
        }

        break;
    }
    case 2: {
        var exprᴛ2 = d.tmp[9];
        if (exprᴛ2 == ctGrayscale) {
            d.cb = cbG2;
        }
        else if (exprᴛ2 == ctPaletted) {
            d.cb = cbP2;
        }

        break;
    }
    case 4: {
        var exprᴛ3 = d.tmp[9];
        if (exprᴛ3 == ctGrayscale) {
            d.cb = cbG4;
        }
        else if (exprᴛ3 == ctPaletted) {
            d.cb = cbP4;
        }

        break;
    }
    case 8: {
        var exprᴛ4 = d.tmp[9];
        if (exprᴛ4 == ctGrayscale) {
            d.cb = cbG8;
        }
        else if (exprᴛ4 == ctTrueColor) {
            d.cb = cbTC8;
        }
        else if (exprᴛ4 == ctPaletted) {
            d.cb = cbP8;
        }
        else if (exprᴛ4 == ctGrayscaleAlpha) {
            d.cb = cbGA8;
        }
        else if (exprᴛ4 == ctTrueColorAlpha) {
            d.cb = cbTCA8;
        }

        break;
    }
    case 16: {
        var exprᴛ5 = d.tmp[9];
        if (exprᴛ5 == ctGrayscale) {
            d.cb = cbG16;
        }
        else if (exprᴛ5 == ctTrueColor) {
            d.cb = cbTC16;
        }
        else if (exprᴛ5 == ctGrayscaleAlpha) {
            d.cb = cbGA16;
        }
        else if (exprᴛ5 == ctTrueColorAlpha) {
            d.cb = cbTCA16;
        }

        break;
    }}

    if (d.cb == cbInvalid) {
        return ((UnsupportedError)fmt.Sprintf("bit depth %d, color type %d"u8, d.tmp[8], d.tmp[9]));
    }
    (d.width, d.height) = ((nint)w, (nint)h);
    return d.verifyChecksum();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badPlteLengthˢ = "bad PLTE length"u8;
internal static readonly @string plteColorTypeMismatchˢ = "PLTE, color type mismatch"u8;

internal static error parsePLTE(this ref decoder d, uint32 length) {
    nint np = (nint)(length / 3); // The number of palette entries.
    if (length % 3 != 0 || np <= 0 || np > 256 || np > ((nint)1).Lsh((nuint)d.depth)) {
        return ((FormatError)(@string)badPlteLengthˢ);
    }
    var (n, err) = io.ReadFull(d.r, d.tmp.slice(0, 3 * np));
    if (err != default!) {
        return err;
    }
    d.crc.Write(d.tmp.slice(0, n));
    var exprᴛ1 = d.cb;
    if (exprᴛ1 == cbP1 || exprᴛ1 == cbP2 || exprᴛ1 == cbP4 || exprᴛ1 == cbP8) {
        d.palette = new color.Palette(256);
        for (nint i = 0; i < np; i++) {
            d.palette[i] = new colorꓸRGBA(d.tmp[3 * i + 0], d.tmp[3 * i + 1], d.tmp[3 * i + 2], 0xff);
        }
        for (nint i = np; i < 256; i++) {
            // Initialize the rest of the palette to opaque black. The spec (section
            // 11.2.3) says that "any out-of-range pixel value found in the image data
            // is an error", but some real-world PNG files have out-of-range pixel
            // values. We fall back to opaque black, the same as libpng 1.5.13;
            // ImageMagick 6.5.7 returns an error.
            d.palette[i] = new colorꓸRGBA(0x00, 0x00, 0x00, 0xff);
        }
        d.palette = d.palette.slice(0, np);
    }
    else if (exprᴛ1 == cbTC8 || exprᴛ1 == cbTCA8 || exprᴛ1 == cbTC16 || exprᴛ1 == cbTCA16) {
    }
    else { /* default: */
        return ((FormatError)(@string)plteColorTypeMismatchˢ);
    }

    // As per the PNG spec, a PLTE chunk is optional (and for practical purposes,
    // ignorable) for the ctTrueColor and ctTrueColorAlpha color types (section 4.1.2).
    return d.verifyChecksum();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTRNSLengthˢ = "bad tRNS length"u8;
internal static readonly @string tRNSColorTypeMismatchˢ = "tRNS, color type mismatch"u8;

internal static error parsetRNS(this ref decoder d, uint32 length) {
    var exprᴛ1 = d.cb;
    if (exprᴛ1 == cbG1 || exprᴛ1 == cbG2 || exprᴛ1 == cbG4 || exprᴛ1 == cbG8 || exprᴛ1 == cbG16) {
        if (length != 2) {
            return ((FormatError)(@string)badTRNSLengthˢ);
        }
        var (n, err) = io.ReadFull(d.r, d.tmp.slice(0, (nint)(length)));
        if (err != default!) {
            return err;
        }
        d.crc.Write(d.tmp.slice(0, n));
        copy(d.transparent[..], d.tmp.slice(0, (nint)(length)));
        var exprᴛ2 = d.cb;
        if (exprᴛ2 == cbG1) {
            d.transparent[1] *= 0xff;
        }
        else if (exprᴛ2 == cbG2) {
            d.transparent[1] *= 0x55;
        }
        else if (exprᴛ2 == cbG4) {
            d.transparent[1] *= 0x11;
        }

        d.useTransparent = true;
    }
    else if (exprᴛ1 == cbTC8 || exprᴛ1 == cbTC16) {
        if (length != 6) {
            return ((FormatError)(@string)badTRNSLengthˢ);
        }
        var (n, err) = io.ReadFull(d.r, d.tmp.slice(0, (nint)(length)));
        if (err != default!) {
            return err;
        }
        d.crc.Write(d.tmp.slice(0, n));
        copy(d.transparent[..], d.tmp.slice(0, (nint)(length)));
        d.useTransparent = true;
    }
    else if (exprᴛ1 == cbP1 || exprᴛ1 == cbP2 || exprᴛ1 == cbP4 || exprᴛ1 == cbP8) {
        if (length > 256) {
            return ((FormatError)(@string)badTRNSLengthˢ);
        }
        var (n, err) = io.ReadFull(d.r, d.tmp.slice(0, (nint)(length)));
        if (err != default!) {
            return err;
        }
        d.crc.Write(d.tmp.slice(0, n));
        if (len(d.palette) < n) {
            d.palette = d.palette.slice(0, n);
        }
        for (nint i = 0; i < n; i++) {
            var rgba = d.palette[i]._<colorꓸRGBA>();
            d.palette[i] = new color.NRGBA(rgba.R, rgba.G, rgba.B, d.tmp[i]);
        }
    }
    else { /* default: */
        return ((FormatError)(@string)tRNSColorTypeMismatchˢ);
    }

    return d.verifyChecksum();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notEnoughPixelDataˢ = "not enough pixel data"u8;
internal static readonly @string idatChunkLengthOverflowˢ = "IDAT chunk length overflow"u8;

// Read presents one or more IDAT chunks as one continuous stream (minus the
// intermediate chunk headers and footers). If the PNG data looked like:
//
//	... len0 IDAT xxx crc0 len1 IDAT yy crc1 len2 IEND crc2
//
// then this reader presents xxxyy. For well-formed PNG data, the decoder state
// immediately before the first Read call is that d.r is positioned between the
// first IDAT and xxx, and the decoder state immediately after the last Read
// call is that d.r is positioned between yy and crc1.
internal static (nint, error) Read(this ref decoder d, slice<byte> p) {
    if (len(p) == 0) {
        return (0, default!);
    }
    while (d.idatLength == 0) {
        // We have exhausted an IDAT chunk. Verify the checksum of that chunk.
        {
            var errΔ1 = d.verifyChecksum(); if (errΔ1 != default!) {
                return (0, errΔ1);
            }
        }
        // Read the length and chunk type of the next chunk, and check that
        // it is an IDAT chunk.
        {
            var (_, errΔ2) = io.ReadFull(d.r, d.tmp[..8]); if (errΔ2 != default!) {
                return (0, errΔ2);
            }
        }
        d.idatLength = binary.BigEndian.Uint32(d.tmp[..4]);
        if (((sstring)(d.tmp[4..8])) != "IDAT"u8) {
            return (0, ((FormatError)(@string)notEnoughPixelDataˢ));
        }
        d.crc.Reset();
        d.crc.Write(d.tmp[4..8]);
    }
    if ((nint)d.idatLength < 0) {
        return (0, ((UnsupportedError)(@string)idatChunkLengthOverflowˢ));
    }
    var (n, err) = d.r.Read(p.slice(0, min(len(p), (nint)d.idatLength)));
    d.crc.Write(p.slice(0, n));
    d.idatLength -= (uint32)n;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tooMuchPixelDataˢ = "too much pixel data"u8;

// decode decodes the IDAT data into an image.
internal static (image.Image, error) decode(this ж<decoder> Ꮡd) {
    GoFrame ᒐ = default;
    try {
        ref var d = ref Ꮡd.DerefOrNull();

        var (r, err) = zlib.NewReader(new decoderжReader(Ꮡd));
        if (err != default!) {
            return (default!, err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        image.Image img = default!;
        if (d.interlace == itNone){
            (img, err) = d.readImagePass(r, 0, false);
            if (err != default!) {
                return (default!, err);
            }
        } else 
        if (d.interlace == itAdam7) {
            // Allocate a blank image of the full size.
            (img, err) = d.readImagePass(default!, 0, true);
            if (err != default!) {
                return (default!, err);
            }
            for (nint pass = 0; pass < 7; pass++) {
                var (imagePass, errΔ1) = d.readImagePass(r, pass, false);
                if (errΔ1 != default!) {
                    return (default!, errΔ1);
                }
                if (imagePass != default!) {
                    d.mergePassInto(img, imagePass, pass);
                }
            }
        }
        // Check for EOF, to verify the zlib checksum.
        nint n = 0;
        for (nint i = 0; n == 0 && err == default!; i++) {
            if (i == 100) {
                return (default!, io.ErrNoProgress);
            }
            (n, err) = r.Read(d.tmp[..1]);
        }
        if (err != default! && !AreEqual(err, io.EOF)) {
            return (default!, ((FormatError)err.Error()));
        }
        if (n != 0 || d.idatLength != 0) {
            return (default!, ((FormatError)(@string)tooMuchPixelDataˢ));
        }
        return (img, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badFilterTypeˢ = "bad filter type"u8;

// readImagePass reads a single image pass, sized according to the pass number.
internal static (image.Image, error) readImagePass(this ref decoder d, io.Reader r, nint pass, bool allocateOnly) {
    nint bitsPerPixel = 0;
    nint pixOffset = 0;
    ж<image.Gray> gray = default!;
    ж<imageꓸRGBA> rgba = default!;
    ж<image.Paletted> paletted = default!;
    ж<image.NRGBA> nrgba = default!;
    ж<image.Gray16> gray16 = default!;
    ж<image.RGBA64> rgba64 = default!;
    ж<image.NRGBA64> nrgba64 = default!;
    image.Image img = default!;
    nint width = d.width;
    nint height = d.height;
    if (d.interlace == itAdam7 && !allocateOnly) {
        var p = interlacing[pass];
        // Add the multiplication factor and subtract one, effectively rounding up.
        width = quo((width - p.xOffset + p.xFactor - 1), p.xFactor);
        height = quo((height - p.yOffset + p.yFactor - 1), p.yFactor);
        // A PNG image can't have zero width or height, but for an interlaced
        // image, an individual pass might have zero width or height. If so, we
        // shouldn't even read a per-row filter type byte, so return early.
        if (width == 0 || height == 0) {
            return (default!, default!);
        }
    }
    var exprᴛ1 = d.cb;
    if (exprᴛ1 == cbG1 || exprᴛ1 == cbG2 || exprᴛ1 == cbG4 || exprᴛ1 == cbG8) {
        bitsPerPixel = d.depth;
        if (d.useTransparent){
            nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
            img = new image.NRGBAжImage(nrgba);
        } else {
            gray = image.NewGray(image.Rect(0, 0, width, height));
            img = new image.GrayжImage(gray);
        }
    }
    else if (exprᴛ1 == cbGA8) {
        bitsPerPixel = 16;
        nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
        img = new image.NRGBAжImage(nrgba);
    }
    else if (exprᴛ1 == cbTC8) {
        bitsPerPixel = 24;
        if (d.useTransparent){
            nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
            img = new image.NRGBAжImage(nrgba);
        } else {
            rgba = image.NewRGBA(image.Rect(0, 0, width, height));
            img = new image.ΔRGBAжImage(rgba);
        }
    }
    else if (exprᴛ1 == cbP1 || exprᴛ1 == cbP2 || exprᴛ1 == cbP4 || exprᴛ1 == cbP8) {
        bitsPerPixel = d.depth;
        paletted = image.NewPaletted(image.Rect(0, 0, width, height), d.palette);
        img = new image.PalettedжImage(paletted);
    }
    else if (exprᴛ1 == cbTCA8) {
        bitsPerPixel = 32;
        nrgba = image.NewNRGBA(image.Rect(0, 0, width, height));
        img = new image.NRGBAжImage(nrgba);
    }
    else if (exprᴛ1 == cbG16) {
        bitsPerPixel = 16;
        if (d.useTransparent){
            nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
            img = new image.NRGBA64жImage(nrgba64);
        } else {
            gray16 = image.NewGray16(image.Rect(0, 0, width, height));
            img = new image.Gray16жImage(gray16);
        }
    }
    else if (exprᴛ1 == cbGA16) {
        bitsPerPixel = 32;
        nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
        img = new image.NRGBA64жImage(nrgba64);
    }
    else if (exprᴛ1 == cbTC16) {
        bitsPerPixel = 48;
        if (d.useTransparent){
            nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
            img = new image.NRGBA64жImage(nrgba64);
        } else {
            rgba64 = image.NewRGBA64(image.Rect(0, 0, width, height));
            img = new image.RGBA64жImage(rgba64);
        }
    }
    else if (exprᴛ1 == cbTCA16) {
        bitsPerPixel = 64;
        nrgba64 = image.NewNRGBA64(image.Rect(0, 0, width, height));
        img = new image.NRGBA64жImage(nrgba64);
    }

    if (allocateOnly) {
        return (img, default!);
    }
    nint bytesPerPixel = (bitsPerPixel + 7) / 8;
    // The +1 is for the per-row filter type, which is at cr[0].
    var rowSize = 1 + ((int64)bitsPerPixel * (int64)width + 7) / 8;
    if (rowSize != (int64)(nint)rowSize) {
        return (default!, ((UnsupportedError)(@string)dimensionOverflowˢ));
    }
    // cr and pr are the bytes for the current and previous row.
    var cr = new slice<uint8>((nint)(rowSize));
    var pr = new slice<uint8>((nint)(rowSize));
    for (nint y = 0; y < height; y++) {
        // Read the decompressed bytes.
        var (_, err) = io.ReadFull(r, cr);
        if (err != default!) {
            if (AreEqual(err, io.EOF) || AreEqual(err, io.ErrUnexpectedEOF)) {
                return (default!, ((FormatError)(@string)notEnoughPixelDataˢ));
            }
            return (default!, err);
        }
        // Apply the filter.
        var cdat = cr[1..];
        var pdat = pr[1..];
        var exprᴛ2 = cr[0];
        if (exprᴛ2 == ftNone) {
        }
        else if (exprᴛ2 == ftSub) {
            for (nint i = bytesPerPixel; i < len(cdat); i++) {
                // No-op.
                cdat[i] += cdat[i - bytesPerPixel];
            }
        }
        else if (exprᴛ2 == ftUp) {
            foreach (var (i, p) in pdat) {
                cdat[i] += p;
            }
        }
        else if (exprᴛ2 == ftAverage) {
            for (nint i = 0; i < bytesPerPixel; i++) {
                // The first column has no column to the left of it, so it is a
                // special case. We know that the first column exists because we
                // check above that width != 0, and so len(cdat) != 0.
                cdat[i] += (uint8)(pdat[i] / 2);
            }
            for (nint i = bytesPerPixel; i < len(cdat); i++) {
                cdat[i] += (uint8)(((nint)cdat[i - bytesPerPixel] + (nint)pdat[i]) / 2);
            }
        }
        else if (exprᴛ2 == ftPaeth) {
            filterPaeth(cdat, pdat, bytesPerPixel);
        }
        else { /* default: */
            return (default!, ((FormatError)(@string)badFilterTypeˢ));
        }

        // Convert from bytes to colors.
        var exprᴛ3 = d.cb;
        if (exprᴛ3 == cbG1) {
            if (d.useTransparent){
                var ty = d.transparent[1];
                for (nint x = 0; x < width; x += 8) {
                    var b = cdat[x / 8];
                    for (nint x2 = 0; x2 < 8 && x + x2 < width; x2++) {
                        var ycol = (uint8)(((b >> (int)(7))) * 0xff);
                        var acol = (uint8)0xff;
                        if (ycol == ty) {
                            acol = 0x00;
                        }
                        nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol, ycol, ycol, acol));
                        b <<= (int)(1);
                    }
                }
            } else {
                for (nint x = 0; x < width; x += 8) {
                    var b = cdat[x / 8];
                    for (nint x2 = 0; x2 < 8 && x + x2 < width; x2++) {
                        gray.SetGray(x + x2, y, new color.Gray((uint8)(((b >> (int)(7))) * 0xff)));
                        b <<= (int)(1);
                    }
                }
            }
        }
        else if (exprᴛ3 == cbG2) {
            if (d.useTransparent){
                var ty = d.transparent[1];
                for (nint x = 0; x < width; x += 4) {
                    var b = cdat[x / 4];
                    for (nint x2 = 0; x2 < 4 && x + x2 < width; x2++) {
                        var ycol = (uint8)(((b >> (int)(6))) * 0x55);
                        var acol = (uint8)0xff;
                        if (ycol == ty) {
                            acol = 0x00;
                        }
                        nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol, ycol, ycol, acol));
                        b <<= (int)(2);
                    }
                }
            } else {
                for (nint x = 0; x < width; x += 4) {
                    var b = cdat[x / 4];
                    for (nint x2 = 0; x2 < 4 && x + x2 < width; x2++) {
                        gray.SetGray(x + x2, y, new color.Gray((uint8)(((b >> (int)(6))) * 0x55)));
                        b <<= (int)(2);
                    }
                }
            }
        }
        else if (exprᴛ3 == cbG4) {
            if (d.useTransparent){
                var ty = d.transparent[1];
                for (nint x = 0; x < width; x += 2) {
                    var b = cdat[x / 2];
                    for (nint x2 = 0; x2 < 2 && x + x2 < width; x2++) {
                        var ycol = (uint8)(((b >> (int)(4))) * 0x11);
                        var acol = (uint8)0xff;
                        if (ycol == ty) {
                            acol = 0x00;
                        }
                        nrgba.SetNRGBA(x + x2, y, new color.NRGBA(ycol, ycol, ycol, acol));
                        b <<= (int)(4);
                    }
                }
            } else {
                for (nint x = 0; x < width; x += 2) {
                    var b = cdat[x / 2];
                    for (nint x2 = 0; x2 < 2 && x + x2 < width; x2++) {
                        gray.SetGray(x + x2, y, new color.Gray((uint8)(((b >> (int)(4))) * 0x11)));
                        b <<= (int)(4);
                    }
                }
            }
        }
        else if (exprᴛ3 == cbG8) {
            if (d.useTransparent){
                var ty = d.transparent[1];
                for (nint x = 0; x < width; x++) {
                    var ycol = cdat[x];
                    var acol = (uint8)0xff;
                    if (ycol == ty) {
                        acol = 0x00;
                    }
                    nrgba.SetNRGBA(x, y, new color.NRGBA(ycol, ycol, ycol, acol));
                }
            } else {
                copy((~gray).Pix.slice(pixOffset), cdat);
                pixOffset += gray.Value.Stride;
            }
        }
        else if (exprᴛ3 == cbGA8) {
            for (nint x = 0; x < width; x++) {
                var ycol = cdat[2 * x + 0];
                nrgba.SetNRGBA(x, y, new color.NRGBA(ycol, ycol, ycol, cdat[2 * x + 1]));
            }
        }
        else if (exprᴛ3 == cbTC8) {
            if (d.useTransparent){
                var pix = nrgba.Value.Pix;
                nint i = pixOffset;
                nint j = 0;
                var (tr, tg, tb) = (d.transparent[1], d.transparent[3], d.transparent[5]);
                for (nint x = 0; x < width; x++) {
                    var rΔ2 = cdat[j + 0];
                    var g = cdat[j + 1];
                    var b = cdat[j + 2];
                    var a = (uint8)0xff;
                    if (rΔ2 == tr && g == tg && b == tb) {
                        a = 0x00;
                    }
                    pix[i + 0] = rΔ2;
                    pix[i + 1] = g;
                    pix[i + 2] = b;
                    pix[i + 3] = a;
                    i += 4;
                    j += 3;
                }
                pixOffset += nrgba.Value.Stride;
            } else {
                var pix = rgba.Value.Pix;
                nint i = pixOffset;
                nint j = 0;
                for (nint x = 0; x < width; x++) {
                    pix[i + 0] = cdat[j + 0];
                    pix[i + 1] = cdat[j + 1];
                    pix[i + 2] = cdat[j + 2];
                    pix[i + 3] = 0xff;
                    i += 4;
                    j += 3;
                }
                pixOffset += rgba.Value.Stride;
            }
        }
        else if (exprᴛ3 == cbP1) {
            for (nint x = 0; x < width; x += 8) {
                var b = cdat[x / 8];
                for (nint x2 = 0; x2 < 8 && x + x2 < width; x2++) {
                    var idx = (uint8)((b >> (int)(7)));
                    if (len((~paletted).Palette) <= (nint)idx) {
                        paletted.Value.Palette = (~paletted).Palette.slice(0, (nint)idx + 1);
                    }
                    paletted.SetColorIndex(x + x2, y, idx);
                    b <<= (int)(1);
                }
            }
        }
        else if (exprᴛ3 == cbP2) {
            for (nint x = 0; x < width; x += 4) {
                var b = cdat[x / 4];
                for (nint x2 = 0; x2 < 4 && x + x2 < width; x2++) {
                    var idx = (uint8)((b >> (int)(6)));
                    if (len((~paletted).Palette) <= (nint)idx) {
                        paletted.Value.Palette = (~paletted).Palette.slice(0, (nint)idx + 1);
                    }
                    paletted.SetColorIndex(x + x2, y, idx);
                    b <<= (int)(2);
                }
            }
        }
        else if (exprᴛ3 == cbP4) {
            for (nint x = 0; x < width; x += 2) {
                var b = cdat[x / 2];
                for (nint x2 = 0; x2 < 2 && x + x2 < width; x2++) {
                    var idx = (uint8)((b >> (int)(4)));
                    if (len((~paletted).Palette) <= (nint)idx) {
                        paletted.Value.Palette = (~paletted).Palette.slice(0, (nint)idx + 1);
                    }
                    paletted.SetColorIndex(x + x2, y, idx);
                    b <<= (int)(4);
                }
            }
        }
        else if (exprᴛ3 == cbP8) {
            if (len((~paletted).Palette) != 256) {
                for (nint x = 0; x < width; x++) {
                    if (len((~paletted).Palette) <= (nint)cdat[x]) {
                        paletted.Value.Palette = (~paletted).Palette.slice(0, (nint)cdat[x] + 1);
                    }
                }
            }
            copy((~paletted).Pix.slice(pixOffset), cdat);
            pixOffset += paletted.Value.Stride;
        }
        else if (exprᴛ3 == cbTCA8) {
            copy((~nrgba).Pix.slice(pixOffset), cdat);
            pixOffset += nrgba.Value.Stride;
        }
        else if (exprᴛ3 == cbG16) {
            if (d.useTransparent){
                var ty = (uint16)((uint16)((uint16)d.transparent[0] << (int)(8)) | (uint16)d.transparent[1]);
                for (nint x = 0; x < width; x++) {
                    var ycol = (uint16)((uint16)((uint16)cdat[2 * x + 0] << (int)(8)) | (uint16)cdat[2 * x + 1]);
                    var acol = (uint16)0xffff;
                    if (ycol == ty) {
                        acol = 0x0000;
                    }
                    nrgba64.SetNRGBA64(x, y, new color.NRGBA64(ycol, ycol, ycol, acol));
                }
            } else {
                for (nint x = 0; x < width; x++) {
                    var ycol = (uint16)((uint16)((uint16)cdat[2 * x + 0] << (int)(8)) | (uint16)cdat[2 * x + 1]);
                    gray16.SetGray16(x, y, new color.Gray16(ycol));
                }
            }
        }
        else if (exprᴛ3 == cbGA16) {
            for (nint x = 0; x < width; x++) {
                var ycol = (uint16)((uint16)((uint16)cdat[4 * x + 0] << (int)(8)) | (uint16)cdat[4 * x + 1]);
                var acol = (uint16)((uint16)((uint16)cdat[4 * x + 2] << (int)(8)) | (uint16)cdat[4 * x + 3]);
                nrgba64.SetNRGBA64(x, y, new color.NRGBA64(ycol, ycol, ycol, acol));
            }
        }
        else if (exprᴛ3 == cbTC16) {
            if (d.useTransparent){
                var tr = (uint16)((uint16)((uint16)d.transparent[0] << (int)(8)) | (uint16)d.transparent[1]);
                var tg = (uint16)((uint16)((uint16)d.transparent[2] << (int)(8)) | (uint16)d.transparent[3]);
                var tb = (uint16)((uint16)((uint16)d.transparent[4] << (int)(8)) | (uint16)d.transparent[5]);
                for (nint x = 0; x < width; x++) {
                    var rcol = (uint16)((uint16)((uint16)cdat[6 * x + 0] << (int)(8)) | (uint16)cdat[6 * x + 1]);
                    var gcol = (uint16)((uint16)((uint16)cdat[6 * x + 2] << (int)(8)) | (uint16)cdat[6 * x + 3]);
                    var bcol = (uint16)((uint16)((uint16)cdat[6 * x + 4] << (int)(8)) | (uint16)cdat[6 * x + 5]);
                    var acol = (uint16)0xffff;
                    if (rcol == tr && gcol == tg && bcol == tb) {
                        acol = 0x0000;
                    }
                    nrgba64.SetNRGBA64(x, y, new color.NRGBA64(rcol, gcol, bcol, acol));
                }
            } else {
                for (nint x = 0; x < width; x++) {
                    var rcol = (uint16)((uint16)((uint16)cdat[6 * x + 0] << (int)(8)) | (uint16)cdat[6 * x + 1]);
                    var gcol = (uint16)((uint16)((uint16)cdat[6 * x + 2] << (int)(8)) | (uint16)cdat[6 * x + 3]);
                    var bcol = (uint16)((uint16)((uint16)cdat[6 * x + 4] << (int)(8)) | (uint16)cdat[6 * x + 5]);
                    rgba64.SetRGBA64(x, y, new color.RGBA64(rcol, gcol, bcol, 0xffff));
                }
            }
        }
        else if (exprᴛ3 == cbTCA16) {
            for (nint x = 0; x < width; x++) {
                var rcol = (uint16)((uint16)((uint16)cdat[8 * x + 0] << (int)(8)) | (uint16)cdat[8 * x + 1]);
                var gcol = (uint16)((uint16)((uint16)cdat[8 * x + 2] << (int)(8)) | (uint16)cdat[8 * x + 3]);
                var bcol = (uint16)((uint16)((uint16)cdat[8 * x + 4] << (int)(8)) | (uint16)cdat[8 * x + 5]);
                var acol = (uint16)((uint16)((uint16)cdat[8 * x + 6] << (int)(8)) | (uint16)cdat[8 * x + 7]);
                nrgba64.SetNRGBA64(x, y, new color.NRGBA64(rcol, gcol, bcol, acol));
            }
        }

        // The current row for y is the previous row for y+1.
        (pr, cr) = (cr, pr);
    }
    return (img, default!);
}

// mergePassInto merges a single pass into a full sized image.
internal static void mergePassInto(this ref decoder d, image.Image dst, image.Image src, nint pass) {
    var p = interlacing[pass];
    slice<uint8> srcPix = default!;
    slice<uint8> dstPix = default!;
    nint stride = default!;
    image.Rectangle rect = default!;
    nint bytesPerPixel = default!;
    switch (dst.type()) {
    case ж<image.Alpha> target: {
        srcPix = src._<ж<image.Alpha>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 1;
        break;
    }
    case ж<image.Alpha16> target: {
        srcPix = src._<ж<image.Alpha16>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 2;
        break;
    }
    case ж<image.Gray> target: {
        srcPix = src._<ж<image.Gray>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 1;
        break;
    }
    case ж<image.Gray16> target: {
        srcPix = src._<ж<image.Gray16>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 2;
        break;
    }
    case ж<image.NRGBA> target: {
        srcPix = src._<ж<image.NRGBA>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 4;
        break;
    }
    case ж<image.NRGBA64> target: {
        srcPix = src._<ж<image.NRGBA64>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 8;
        break;
    }
    case ж<image.Paletted> target: {
        var source = src._<ж<image.Paletted>>();
        srcPix = source.Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 1;
        if (len((~target).Palette) < len((~source).Palette)) {
            // readImagePass can return a paletted image whose implicit palette
            // length (one more than the maximum Pix value) is larger than the
            // explicit palette length (what's in the PLTE chunk). Make the
            // same adjustment here.
            target.Value.Palette = source.Value.Palette;
        }
        break;
    }
    case ж<imageꓸRGBA> target: {
        srcPix = src._<ж<imageꓸRGBA>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 4;
        break;
    }
    case ж<image.RGBA64> target: {
        srcPix = src._<ж<image.RGBA64>>().Value.Pix;
        (dstPix, stride, rect) = (target.Value.Pix, target.Value.Stride, target.Value.Rect);
        bytesPerPixel = 8;
        break;
    }}
    nint s = 0;
    var bounds = src.Bounds();
    for (nint y = bounds.Min.Y; y < bounds.Max.Y; y++) {
        nint dBase = (y * p.yFactor + p.yOffset - rect.Min.Y) * stride + (p.xOffset - rect.Min.X) * bytesPerPixel;
        for (nint x = bounds.Min.X; x < bounds.Max.X; x++) {
            nint dΔ1 = dBase + x * p.xFactor * bytesPerPixel;
            copy(dstPix.slice(dΔ1), srcPix.slice(s, s + bytesPerPixel));
            s += bytesPerPixel;
        }
    }
}

internal static error /*err*/ parseIDAT(this ж<decoder> Ꮡd, uint32 length) {
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    d.idatLength = length;
    (d.img, err) = Ꮡd.decode();
    if (err != default!) {
        return err;
    }
    return d.verifyChecksum();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badIendLengthˢ = "bad IEND length"u8;

internal static error parseIEND(this ref decoder d, uint32 length) {
    if (length != 0) {
        return ((FormatError)(@string)badIendLengthˢ);
    }
    return d.verifyChecksum();
}

internal static error parseChunk(this ж<decoder> Ꮡd, bool configOnly) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Read the length and chunk type.
    {
        var (_, err) = io.ReadFull(d.r, d.tmp[..8]); if (err != default!) {
            return err;
        }
    }
    var length = binary.BigEndian.Uint32(d.tmp[..4]);
    d.crc.Reset();
    d.crc.Write(d.tmp[4..8]);
    // Read the chunk data.
    var exprᴛ1 = ((sstring)(d.tmp[4..8]));
    if (exprᴛ1 == "IHDR"u8) {
        if (d.stage != dsStart) {
            return chunkOrderError;
        }
        d.stage = dsSeenIHDR;
        return d.parseIHDR(length);
    }
    if (exprᴛ1 == "PLTE"u8) {
        if (d.stage != dsSeenIHDR) {
            return chunkOrderError;
        }
        d.stage = dsSeenPLTE;
        return d.parsePLTE(length);
    }
    if (exprᴛ1 == "tRNS"u8) {
        if (cbPaletted(d.cb)){
            if (d.stage != dsSeenPLTE) {
                return chunkOrderError;
            }
        } else 
        if (cbTrueColor(d.cb)){
            if (d.stage != dsSeenIHDR && d.stage != dsSeenPLTE) {
                return chunkOrderError;
            }
        } else 
        if (d.stage != dsSeenIHDR) {
            return chunkOrderError;
        }
        d.stage = dsSeentRNS;
        return d.parsetRNS(length);
    }
    if (exprᴛ1 == "IDAT"u8) {
        do {
            if (d.stage < dsSeenIHDR || d.stage > dsSeenIDAT || (d.stage == dsSeenIHDR && cbPaletted(d.cb))){
                return chunkOrderError;
            } else 
            if (d.stage == dsSeenIDAT) {
                // Ignore trailing zero-length or garbage IDAT chunks.
                //
                // This does not affect valid PNG images that contain multiple IDAT
                // chunks, since the first call to parseIDAT below will consume all
                // consecutive IDAT chunks required for decoding the image.
                break;
            }
            d.stage = dsSeenIDAT;
            if (configOnly) {
                return default!;
            }
            return Ꮡd.parseIDAT(length);
        } while (false);
    }
    else if (exprᴛ1 == "IEND"u8) {
        if (d.stage != dsSeenIDAT) {
            return chunkOrderError;
        }
        d.stage = dsSeenIEND;
        return d.parseIEND(length);
    }

    if (length > 0x7fffffff) {
        return ((FormatError)fmt.Sprintf("Bad chunk length: %d"u8, length));
    }
    // Ignore this chunk (of a known length).
    array<byte> ignored = new(4096);
    while (length > 0) {
        var (n, err) = io.ReadFull(d.r, ignored.slice(0, min(len(ignored), (nint)length)));
        if (err != default!) {
            return err;
        }
        d.crc.Write(ignored.slice(0, n));
        length -= (uint32)n;
    }
    return d.verifyChecksum();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidChecksumˢ = "invalid checksum"u8;

internal static error verifyChecksum(this ref decoder d) {
    {
        var (_, err) = io.ReadFull(d.r, d.tmp[..4]); if (err != default!) {
            return err;
        }
    }
    if (binary.BigEndian.Uint32(d.tmp[..4]) != d.crc.Sum32()) {
        return ((FormatError)(@string)invalidChecksumˢ);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notAPngFileˢ = "not a PNG file"u8;

internal static error checkHeader(this ref decoder d) {
    var (_, err) = io.ReadFull(d.r, d.tmp[..(int)(len(pngHeader))]);
    if (err != default!) {
        return err;
    }
    if (((sstring)(d.tmp[..(int)(len(pngHeader))])) != pngHeader) {
        return ((FormatError)(@string)notAPngFileˢ);
    }
    return default!;
}

// Decode reads a PNG image from r and returns it as an [image.Image].
// The type of Image returned depends on the PNG contents.
public static (image.Image, error) Decode(io.Reader r) {
    var d = Ꮡ(new decoder(
        r: r,
        crc: crc32.NewIEEE()
    ));
    {
        var err = d.checkHeader(); if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            return (default!, err);
        }
    }
    while ((~d).stage != dsSeenIEND) {
        {
            var err = d.parseChunk(false); if (err != default!) {
                if (AreEqual(err, io.EOF)) {
                    err = io.ErrUnexpectedEOF;
                }
                return (default!, err);
            }
        }
    }
    return ((~d).img, default!);
}

// DecodeConfig returns the color model and dimensions of a PNG image without
// decoding the entire image.
public static (image.Config, error) DecodeConfig(io.Reader r) {
    var d = Ꮡ(new decoder(
        r: r,
        crc: crc32.NewIEEE()
    ));
    {
        var err = d.checkHeader(); if (err != default!) {
            if (AreEqual(err, io.EOF)) {
                err = io.ErrUnexpectedEOF;
            }
            return (new image.Config(nil), err);
        }
    }
    while (ᐧ) {
        {
            var err = d.parseChunk(true); if (err != default!) {
                if (AreEqual(err, io.EOF)) {
                    err = io.ErrUnexpectedEOF;
                }
                return (new image.Config(nil), err);
            }
        }
        if (cbPaletted((~d).cb)){
            if ((~d).stage >= dsSeentRNS) {
                break;
            }
        } else {
            if ((~d).stage >= dsSeenIHDR) {
                break;
            }
        }
    }
    color.Model cm = default!;
    var exprᴛ1 = (~d).cb;
    if (exprᴛ1 == cbG1 || exprᴛ1 == cbG2 || exprᴛ1 == cbG4 || exprᴛ1 == cbG8) {
        cm = color.GrayModel;
    }
    else if (exprᴛ1 == cbGA8) {
        cm = color.NRGBAModel;
    }
    else if (exprᴛ1 == cbTC8) {
        cm = color.RGBAModel;
    }
    else if (exprᴛ1 == cbP1 || exprᴛ1 == cbP2 || exprᴛ1 == cbP4 || exprᴛ1 == cbP8) {
        cm = d.Value.palette;
    }
    else if (exprᴛ1 == cbTCA8) {
        cm = color.NRGBAModel;
    }
    else if (exprᴛ1 == cbG16) {
        cm = color.Gray16Model;
    }
    else if (exprᴛ1 == cbGA16) {
        cm = color.NRGBA64Model;
    }
    else if (exprᴛ1 == cbTC16) {
        cm = color.RGBA64Model;
    }
    else if (exprᴛ1 == cbTCA16) {
        cm = color.NRGBA64Model;
    }

    return (new image.Config(
        ColorModel: cm,
        Width: (~d).width,
        Height: (~d).height
    ), default!);
}

[GoInit] internal static void init() {
    image.RegisterFormat("png"u8, pngHeader, Decode, DecodeConfig);
}

} // end png_package
