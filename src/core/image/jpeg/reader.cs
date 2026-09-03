// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package jpeg implements a JPEG image decoder and encoder.
//
// JPEG is defined in ITU-T T.81: https://www.w3.org/Graphics/JPEG/itu-t81.pdf.
namespace go.image;

using image = image_package;
using color = go.image.color_package;
using imageutil = go.image.@internal.imageutil_package;
using io = io_package;
using go.image;
using go.image.@internal;

partial class jpeg_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸimage() {
    builtin.initPackage(typeof(image_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸimageꓸcolor() {
    builtin.initPackage(typeof(go.image.color_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸimageꓸinternalꓸimageutil() {
    builtin.initPackage(typeof(go.image.@internal.imageutil_package));
}

[GoType("@string")] partial struct FormatError;

public static @string Error(this FormatError e) {
    return "invalid JPEG format: "u8 + ((@string)e);
}

[GoType("@string")] partial struct UnsupportedError;

public static @string Error(this UnsupportedError e) {
    return "unsupported JPEG feature: "u8 + ((@string)e);
}

internal static UnsupportedError errUnsupportedSubsamplingRatio = ((UnsupportedError)(@string)"luma/chroma subsampling ratio"u8);

// Component specification, specified in section B.2.2.
[GoType] partial struct component {
    internal nint h;  // Horizontal sampling factor.
    internal nint v;  // Vertical sampling factor.
    internal uint8 c; // Component identifier.
    internal uint8 tq; // Quantization table destination selector.
}

internal static UntypedInt dcTable => 0;
internal static UntypedInt acTable => 1;
internal static UntypedInt maxTc => 1;
internal static UntypedInt maxTh => 3;
internal static UntypedInt maxTq => 3;
internal static UntypedInt maxComponents => 4;

internal static UntypedInt sof0Marker => 0xc0; // Start Of Frame (Baseline Sequential).
internal static UntypedInt sof1Marker => 0xc1; // Start Of Frame (Extended Sequential).
internal static UntypedInt sof2Marker => 0xc2; // Start Of Frame (Progressive).
internal static UntypedInt dhtMarker => 0xc4; // Define Huffman Table.
internal static UntypedInt rst0Marker => 0xd0; // ReSTart (0).
internal static UntypedInt rst7Marker => 0xd7; // ReSTart (7).
internal static UntypedInt soiMarker => 0xd8; // Start Of Image.
internal static UntypedInt eoiMarker => 0xd9; // End Of Image.
internal static UntypedInt sosMarker => 0xda; // Start Of Scan.
internal static UntypedInt dqtMarker => 0xdb; // Define Quantization Table.
internal static UntypedInt driMarker => 0xdd; // Define Restart Interval.
internal static UntypedInt comMarker => 0xfe; // COMment.
internal static UntypedInt app0Marker => 0xe0;
internal static UntypedInt app14Marker => 0xee;
internal static UntypedInt app15Marker => 0xef;

// See https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
internal static UntypedInt adobeTransformUnknown => 0;

internal static UntypedInt adobeTransformYCbCr => 1;

internal static UntypedInt adobeTransformYCbCrK => 2;

// unzig maps from the zig-zag ordering to the natural ordering. For example,
// unzig[3] is the column and row of the fourth element in zig-zag order. The
// value is 16, which means first column (16%8 == 0) and third row (16/8 == 2).
internal static array<nint> unzig = new nint[]{
    0, 1, 8, 16, 9, 2, 3, 10,
    17, 24, 32, 25, 18, 11, 4, 5,
    12, 19, 26, 33, 40, 48, 41, 34,
    27, 20, 13, 6, 7, 14, 21, 28,
    35, 42, 49, 56, 57, 50, 43, 36,
    29, 22, 15, 23, 30, 37, 44, 51,
    58, 59, 52, 45, 38, 31, 39, 46,
    53, 60, 61, 54, 47, 55, 62, 63
}.array();

// Deprecated: Reader is not used by the [image/jpeg] package and should
// not be used by others. It is kept for compatibility.
[GoType] partial interface Reader :
    io.ByteReader,
    io.Reader
{
}

// bits holds the unprocessed bits that have been taken from the byte-stream.
// The n least significant bits of a form the unread bits, to be read in MSB to
// LSB order.
[GoType] partial struct bits {
    internal uint32 a; // accumulator.
    internal uint32 m; // mask. m==1<<(n-1) when n>0, with m==0 when n==0.
    internal int32 n;  // the number of unread bits in a.
}

[GoType("dyn")] partial struct decoder_bytes {
    // buf[i:j] are the buffered bytes read from the underlying
    // io.Reader that haven't yet been passed further on.
    internal array<byte> buf = new(4096);
    internal nint i, j;
    // nUnreadable is the number of bytes to back up i after
    // overshooting. It can be 0, 1 or 2.
    internal nint nUnreadable;
}

[GoType] partial struct decoder {
    internal io.Reader r;
    internal bits bits;
    // bytes is a byte buffer, similar to a bufio.Reader, except that it
    // has to be able to unread more than 1 byte, due to byte stuffing.
    // Byte stuffing is specified in section F.1.2.3.
    internal decoder_bytes bytes;
    internal nint width, height;
    internal ж<image.Gray> img1;
    internal ж<image.YCbCr> img3;
    internal slice<byte> blackPix;
    internal nint blackStride;
    internal nint ri; // Restart Interval.
    internal nint nComp;
    // As per section 4.5, there are four modes of operation (selected by the
    // SOF? markers): sequential DCT, progressive DCT, lossless and
    // hierarchical, although this implementation does not support the latter
    // two non-DCT modes. Sequential DCT is further split into baseline and
    // extended, as per section 4.11.
    internal bool baseline;
    internal bool progressive;
    internal bool jfif;
    internal bool adobeTransformValid;
    internal uint8 adobeTransform;
    internal uint16 eobRun; // End-of-Band run, specified in section G.1.2.2.
    internal array<component> comp = new(maxComponents);
    internal array<slice<block>> progCoeffs = new(maxComponents); // Saved state between progressive-mode scans.
    internal array<array<huffman>> huff = new(maxTc + 1, () => new(4, () => new()));
    internal array<block> quant = new(maxTq + 1); // Quantization tables, in zig-zag order.
    internal array<byte> tmp = new(2 * blockSize);
}

// fill fills up the d.bytes.buf buffer from the underlying io.Reader. It
// should only be called when there are no unread bytes in d.bytes.
[GoRecv] internal static error fill(this ref decoder d) {
    if (d.bytes.i != d.bytes.j) {
        throw panic("jpeg: fill called when unread bytes exist");
    }
    // Move the last 2 bytes to the start of the buffer, in case we need
    // to call unreadByteStuffedByte.
    if (d.bytes.j > 2) {
        d.bytes.buf[0] = d.bytes.buf[d.bytes.j - 2];
        d.bytes.buf[1] = d.bytes.buf[d.bytes.j - 1];
        (d.bytes.i, d.bytes.j) = (2, 2);
    }
    // Fill in the rest of the buffer.
    var (n, err) = d.r.Read(d.bytes.buf[(int)(d.bytes.j)..]);
    d.bytes.j += n;
    if (n > 0) {
        return default!;
    }
    if (AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    return err;
}

// unreadByteStuffedByte undoes the most recent readByteStuffedByte call,
// giving a byte of data back from d.bits to d.bytes. The Huffman look-up table
// requires at least 8 bits for look-up, which means that Huffman decoding can
// sometimes overshoot and read one or two too many bytes. Two-byte overshoot
// can happen when expecting to read a 0xff 0x00 byte-stuffed byte.
[GoRecv] internal static void unreadByteStuffedByte(this ref decoder d) {
    d.bytes.i -= d.bytes.nUnreadable;
    d.bytes.nUnreadable = 0;
    if (d.bits.n >= 8) {
        d.bits.a >>= (int)(8);
        d.bits.n -= 8;
        d.bits.m >>= (int)(8);
    }
}

// readByte returns the next byte, whether buffered or not buffered. It does
// not care about byte stuffing.
[GoRecv] internal static (byte x, error err) readByte(this ref decoder d) {
    byte x = default!;
    error err = default!;

    while (d.bytes.i == d.bytes.j) {
        {
            err = d.fill(); if (err != default!) {
                return (0, err);
            }
        }
    }
    x = d.bytes.buf[d.bytes.i];
    d.bytes.i++;
    d.bytes.nUnreadable = 0;
    return (x, default!);
}

// errMissingFF00 means that readByteStuffedByte encountered an 0xff byte (a
// marker byte) that wasn't the expected byte-stuffed sequence 0xff, 0x00.
internal static FormatError errMissingFF00 = ((FormatError)(@string)"missing 0xff00 sequence"u8);

// readByteStuffedByte is like readByte but is for byte-stuffed Huffman data.
[GoRecv] internal static (byte x, error err) readByteStuffedByte(this ref decoder d) {
    byte x = default!;
    error err = default!;

    // Take the fast path if d.bytes.buf contains at least two bytes.
    if (d.bytes.i + 2 <= d.bytes.j) {
        x = d.bytes.buf[d.bytes.i];
        d.bytes.i++;
        d.bytes.nUnreadable = 1;
        if (x != 0xff) {
            return (x, err);
        }
        if (d.bytes.buf[d.bytes.i] != 0x00) {
            return (0, errMissingFF00);
        }
        d.bytes.i++;
        d.bytes.nUnreadable = 2;
        return (0xff, default!);
    }
    d.bytes.nUnreadable = 0;
    (x, err) = d.readByte();
    if (err != default!) {
        return (0, err);
    }
    d.bytes.nUnreadable = 1;
    if (x != 0xff) {
        return (x, default!);
    }
    (x, err) = d.readByte();
    if (err != default!) {
        return (0, err);
    }
    d.bytes.nUnreadable = 2;
    if (x != 0x00) {
        return (0, errMissingFF00);
    }
    return (0xff, default!);
}

// readFull reads exactly len(p) bytes into p. It does not care about byte
// stuffing.
[GoRecv] internal static error readFull(this ref decoder d, slice<byte> p) {
    // Unread the overshot bytes, if any.
    if (d.bytes.nUnreadable != 0) {
        if (d.bits.n >= 8) {
            d.unreadByteStuffedByte();
        }
        d.bytes.nUnreadable = 0;
    }
    while (ᐧ) {
        nint n = copy(p, d.bytes.buf[(int)(d.bytes.i)..(int)(d.bytes.j)]);
        p = p[(int)(n)..];
        d.bytes.i += n;
        if (len(p) == 0) {
            break;
        }
        {
            var err = d.fill(); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// ignore ignores the next n bytes.
[GoRecv] internal static error ignore(this ref decoder d, nint n) {
    // Unread the overshot bytes, if any.
    if (d.bytes.nUnreadable != 0) {
        if (d.bits.n >= 8) {
            d.unreadByteStuffedByte();
        }
        d.bytes.nUnreadable = 0;
    }
    while (ᐧ) {
        nint m = d.bytes.j - d.bytes.i;
        if (m > n) {
            m = n;
        }
        d.bytes.i += m;
        n -= m;
        if (n == 0) {
            break;
        }
        {
            var err = d.fill(); if (err != default!) {
                return err;
            }
        }
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string multipleSofMarkersˢ = "multiple SOF markers"u8;
internal static readonly @string numberOfComponentsˢ = "number of components"u8;
internal static readonly @string precisionˢ = "precision"u8;
internal static readonly @string sofHasWrongLengthˢ = "SOF has wrong length"u8;
internal static readonly @string repeatedComponentˢ = "repeated component identifier"u8;
internal static readonly @string badTqValueˢ = "bad Tq value"u8;
internal static readonly @string lumaChromaSubsamplingˢ = "luma/chroma subsampling ratio"u8;

// Specified in section B.2.2.
[GoRecv] internal static error processSOF(this ref decoder d, nint n) {
    if (d.nComp != 0) {
        return ((FormatError)(@string)multipleSofMarkersˢ);
    }
    var exprᴛ1 = n;
    if (exprᴛ1 == 6 + 3 * 1) {
        d.nComp = 1;
    }
    else if (exprᴛ1 == 6 + 3 * 3) {
        d.nComp = 3;
    }
    else if (exprᴛ1 == 6 + 3 * 4) {
        d.nComp = 4;
    }
    else { /* default: */
        return ((UnsupportedError)(@string)numberOfComponentsˢ);
    }

    // Grayscale image.
    // YCbCr or RGB image.
    // YCbCrK or CMYK image.
    {
        var err = d.readFull(d.tmp[..(int)(n)]); if (err != default!) {
            return err;
        }
    }
    // We only support 8-bit precision.
    if (d.tmp[0] != 8) {
        return ((UnsupportedError)(@string)precisionˢ);
    }
    d.height = ((nint)d.tmp[1] << (int)(8)) + (nint)d.tmp[2];
    d.width = ((nint)d.tmp[3] << (int)(8)) + (nint)d.tmp[4];
    if ((nint)d.tmp[5] != d.nComp) {
        return ((FormatError)(@string)sofHasWrongLengthˢ);
    }
    for (nint i = 0; i < d.nComp; i++) {
        d.comp[i].c = d.tmp[6 + 3 * i];
        // Section B.2.2 states that "the value of C_i shall be different from
        // the values of C_1 through C_(i-1)".
        for (nint j = 0; j < i; j++) {
            if (d.comp[i].c == d.comp[j].c) {
                return ((FormatError)(@string)repeatedComponentˢ);
            }
        }
        d.comp[i].tq = d.tmp[8 + 3 * i];
        if (d.comp[i].tq > maxTq) {
            return ((FormatError)(@string)badTqValueˢ);
        }
        var hv = d.tmp[7 + 3 * i];
        nint h = (nint)((hv >> (int)(4)));
        nint v = (nint)((byte)(hv & 0x0f));
        if (h < 1 || 4 < h || v < 1 || 4 < v) {
            return ((FormatError)(@string)lumaChromaSubsamplingˢ);
        }
        if (h == 3 || v == 3) {
            return errUnsupportedSubsamplingRatio;
        }
        switch (d.nComp) {
        case 1: {
            (h, v) = (1, 1);
            break;
        }
        case 3: {
            switch (i) {
            case 0: {
                if (v == 4) {
                    // If a JPEG image has only one component, section A.2 says "this data
                    // is non-interleaved by definition" and section A.2.2 says "[in this
                    // case...] the order of data units within a scan shall be left-to-right
                    // and top-to-bottom... regardless of the values of H_1 and V_1". Section
                    // 4.8.2 also says "[for non-interleaved data], the MCU is defined to be
                    // one data unit". Similarly, section A.1.1 explains that it is the ratio
                    // of H_i to max_j(H_j) that matters, and similarly for V. For grayscale
                    // images, H_1 is the maximum H_j for all components j, so that ratio is
                    // always 1. The component's (h, v) is effectively always (1, 1): even if
                    // the nominal (h, v) is (2, 1), a 20x5 image is encoded in three 8x8
                    // MCUs, not two 16x8 MCUs.
                    // For YCbCr images, we only support 4:4:4, 4:4:0, 4:2:2, 4:2:0,
                    // 4:1:1 or 4:1:0 chroma subsampling ratios. This implies that the
                    // (h, v) values for the Y component are either (1, 1), (1, 2),
                    // (2, 1), (2, 2), (4, 1) or (4, 2), and the Y component's values
                    // must be a multiple of the Cb and Cr component's values. We also
                    // assume that the two chroma components have the same subsampling
                    // ratio.
                    // Y.
                    // We have already verified, above, that h and v are both
                    // either 1, 2 or 4, so invalid (h, v) combinations are those
                    // with v == 4.
                    return errUnsupportedSubsamplingRatio;
                }
                break;
            }
            case 1: {
                if (d.comp[0].h % h != 0 || d.comp[0].v % v != 0) {
                    // Cb.
                    return errUnsupportedSubsamplingRatio;
                }
                break;
            }
            case 2: {
                if (d.comp[1].h != h || d.comp[1].v != v) {
                    // Cr.
                    return errUnsupportedSubsamplingRatio;
                }
                break;
            }}

            break;
        }
        case 4: {
            switch (i) {
            case 0: {
                if (hv != 0x11 && hv != 0x22) {
                    // For 4-component images (either CMYK or YCbCrK), we only support two
                    // hv vectors: [0x11 0x11 0x11 0x11] and [0x22 0x11 0x11 0x22].
                    // Theoretically, 4-component JPEG images could mix and match hv values
                    // but in practice, those two combinations are the only ones in use,
                    // and it simplifies the applyBlack code below if we can assume that:
                    //	- for CMYK, the C and K channels have full samples, and if the M
                    //	  and Y channels subsample, they subsample both horizontally and
                    //	  vertically.
                    //	- for YCbCrK, the Y and K channels have full samples.
                    return errUnsupportedSubsamplingRatio;
                }
                break;
            }
            case 1 or 2: {
                if (hv != 0x11) {
                    return errUnsupportedSubsamplingRatio;
                }
                break;
            }
            case 3: {
                if (d.comp[0].h != h || d.comp[0].v != v) {
                    return errUnsupportedSubsamplingRatio;
                }
                break;
            }}

            break;
        }}

        d.comp[i].h = h;
        d.comp[i].v = v;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badPqValueˢ = "bad Pq value"u8;
internal static readonly @string dqtHasWrongLengthˢ = "DQT has wrong length"u8;

// Specified in section B.2.4.1.
[GoRecv] internal static error processDQT(this ref decoder d, nint n) {
loop:
    while (n > 0) {
        n--;
        var (x, err) = d.readByte();
        if (err != default!) {
            return err;
        }
        var tq = (byte)(x & 0x0f);
        if (tq > maxTq) {
            return ((FormatError)(@string)badTqValueˢ);
        }
        switch ((x >> (int)(4))) {
        default: {
            return ((FormatError)(@string)badPqValueˢ);
        }
        case 0: {
            if (n < blockSize) {
                goto break_loop;
            }
            n -= blockSize;
            {
                var errΔ3 = d.readFull(d.tmp[..(int)(blockSize)]); if (errΔ3 != default!) {
                    return errΔ3;
                }
            }
            foreach (var (i, _) in d.quant[tq]) {
                d.quant[tq][i] = (int32)d.tmp[i];
            }
            break;
        }
        case 1: {
            if (n < 2 * blockSize) {
                goto break_loop;
            }
            n -= 2 * blockSize;
            {
                var errΔ4 = d.readFull(d.tmp[..(int)(2 * blockSize)]); if (errΔ4 != default!) {
                    return errΔ4;
                }
            }
            foreach (var (i, _) in d.quant[tq]) {
                d.quant[tq][i] = (int32)(((int32)d.tmp[2 * i] << (int)(8)) | (int32)d.tmp[2 * i + 1]);
            }
            break;
        }}

continue_loop:;
    }
break_loop:;
    if (n != 0) {
        return ((FormatError)(@string)dqtHasWrongLengthˢ);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string driHasWrongLengthˢ = "DRI has wrong length"u8;

// Specified in section B.2.4.4.
[GoRecv] internal static error processDRI(this ref decoder d, nint n) {
    if (n != 2) {
        return ((FormatError)(@string)driHasWrongLengthˢ);
    }
    {
        var err = d.readFull(d.tmp[..2]); if (err != default!) {
            return err;
        }
    }
    d.ri = ((nint)d.tmp[0] << (int)(8)) + (nint)d.tmp[1];
    return default!;
}

[GoRecv] internal static error processApp0Marker(this ref decoder d, nint n) {
    if (n < 5) {
        return d.ignore(n);
    }
    {
        var err = d.readFull(d.tmp[..5]); if (err != default!) {
            return err;
        }
    }
    n -= 5;
    d.jfif = d.tmp[0] == (rune)'J' && d.tmp[1] == (rune)'F' && d.tmp[2] == (rune)'I' && d.tmp[3] == (rune)'F' && d.tmp[4] == (rune)'\x00';
    if (n > 0) {
        return d.ignore(n);
    }
    return default!;
}

[GoRecv] internal static error processApp14Marker(this ref decoder d, nint n) {
    if (n < 12) {
        return d.ignore(n);
    }
    {
        var err = d.readFull(d.tmp[..12]); if (err != default!) {
            return err;
        }
    }
    n -= 12;
    if (d.tmp[0] == (rune)'A' && d.tmp[1] == (rune)'d' && d.tmp[2] == (rune)'o' && d.tmp[3] == (rune)'b' && d.tmp[4] == (rune)'e') {
        d.adobeTransformValid = true;
        d.adobeTransform = d.tmp[11];
    }
    if (n > 0) {
        return d.ignore(n);
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingSoiMarkerˢ = "missing SOI marker"u8;
internal static readonly @string shortSegmentLengthˢ = "short segment length"u8;
internal static readonly @string unknownMarkerˢ = "unknown marker"u8;
internal static readonly @string missingSosMarkerˢ = "missing SOS marker"u8;

// decode reads a JPEG image from r and returns it as an image.Image.
[GoRecv] internal static (image.Image, error) decode(this ref decoder d, io.Reader r, bool configOnly) {
    d.r = r;
    // Check for the Start Of Image marker.
    {
        var err = d.readFull(d.tmp[..2]); if (err != default!) {
            return (default!, err);
        }
    }
    if (d.tmp[0] != 0xff || d.tmp[1] != soiMarker) {
        return (default!, ((FormatError)(@string)missingSoiMarkerˢ));
    }
    // Process the remaining segments until the End Of Image marker.
    while (ᐧ) {
        var err = d.readFull(d.tmp[..2]);
        if (err != default!) {
            return (default!, err);
        }
        while (d.tmp[0] != 0xff) {
            // Strictly speaking, this is a format error. However, libjpeg is
            // liberal in what it accepts. As of version 9, next_marker in
            // jdmarker.c treats this as a warning (JWRN_EXTRANEOUS_DATA) and
            // continues to decode the stream. Even before next_marker sees
            // extraneous data, jpeg_fill_bit_buffer in jdhuff.c reads as many
            // bytes as it can, possibly past the end of a scan's data. It
            // effectively puts back any markers that it overscanned (e.g. an
            // "\xff\xd9" EOI marker), but it does not put back non-marker data,
            // and thus it can silently ignore a small number of extraneous
            // non-marker bytes before next_marker has a chance to see them (and
            // print a warning).
            //
            // We are therefore also liberal in what we accept. Extraneous data
            // is silently ignored.
            //
            // This is similar to, but not exactly the same as, the restart
            // mechanism within a scan (the RST[0-7] markers).
            //
            // Note that extraneous 0xff bytes in e.g. SOS data are escaped as
            // "\xff\x00", and so are detected a little further down below.
            d.tmp[0] = d.tmp[1];
            (d.tmp[1], err) = d.readByte();
            if (err != default!) {
                return (default!, err);
            }
        }
        var marker = d.tmp[1];
        if (marker == 0) {
            // Treat "\xff\x00" as extraneous data.
            continue;
        }
        while (marker == 0xff) {
            // Section B.1.1.2 says, "Any marker may optionally be preceded by any
            // number of fill bytes, which are bytes assigned code X'FF'".
            (marker, err) = d.readByte();
            if (err != default!) {
                return (default!, err);
            }
        }
        if (marker == eoiMarker) {
            // End Of Image.
            break;
        }
        if (rst0Marker <= marker && marker <= rst7Marker) {
            // Figures B.2 and B.16 of the specification suggest that restart markers should
            // only occur between Entropy Coded Segments and not after the final ECS.
            // However, some encoders may generate incorrect JPEGs with a final restart
            // marker. That restart marker will be seen here instead of inside the processSOS
            // method, and is ignored as a harmless error. Restart markers have no extra data,
            // so we check for this before we read the 16-bit length of the segment.
            continue;
        }
        // Read the 16-bit length of the segment. The value includes the 2 bytes for the
        // length itself, so we subtract 2 to get the number of remaining bytes.
        {
            err = d.readFull(d.tmp[..2]); if (err != default!) {
                return (default!, err);
            }
        }
        nint n = ((nint)d.tmp[0] << (int)(8)) + (nint)d.tmp[1] - 2;
        if (n < 0) {
            return (default!, ((FormatError)(@string)shortSegmentLengthˢ));
        }
        var exprᴛ1 = marker;
        if (exprᴛ1 == sof0Marker || exprᴛ1 == sof1Marker || exprᴛ1 == sof2Marker) {
            d.baseline = marker == sof0Marker;
            d.progressive = marker == sof2Marker;
            err = d.processSOF(n);
            if (configOnly && d.jfif) {
                return (default!, err);
            }
        }
        else if (exprᴛ1 == dhtMarker) {
            if (configOnly){
                err = d.ignore(n);
            } else {
                err = d.processDHT(n);
            }
        }
        else if (exprᴛ1 == dqtMarker) {
            if (configOnly){
                err = d.ignore(n);
            } else {
                err = d.processDQT(n);
            }
        }
        else if (exprᴛ1 == sosMarker) {
            if (configOnly) {
                return (default!, default!);
            }
            err = d.processSOS(n);
        }
        else if (exprᴛ1 == driMarker) {
            if (configOnly){
                err = d.ignore(n);
            } else {
                err = d.processDRI(n);
            }
        }
        else if (exprᴛ1 == app0Marker) {
            err = d.processApp0Marker(n);
        }
        else if (exprᴛ1 == app14Marker) {
            err = d.processApp14Marker(n);
        }
        else { /* default: */
            if (app0Marker <= marker && marker <= app15Marker || marker == comMarker){
                err = d.ignore(n);
            } else 
            if (marker < 0xc0){
                // See Table B.1 "Marker code assignments".
                err = ((FormatError)(@string)unknownMarkerˢ);
            } else {
                err = ((UnsupportedError)(@string)unknownMarkerˢ);
            }
        }

        if (err != default!) {
            return (default!, err);
        }
    }
    if (d.progressive) {
        {
            var err = d.reconstructProgressiveImage(); if (err != default!) {
                return (default!, err);
            }
        }
    }
    if (d.img1 != nil) {
        return (new image.GrayжImage(d.img1), default!);
    }
    if (d.img3 != nil) {
        if (d.blackPix != default!){
            return d.applyBlack();
        } else 
        if (d.isRGB()) {
            return d.convertToRGB();
        }
        return (new image.YCbCrжImage(d.img3), default!);
    }
    return (default!, ((FormatError)(@string)missingSosMarkerˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownColorModel4ˢ = "unknown color model: 4-component JPEG doesn't have Adobe APP14 metadata"u8;

[GoType("dyn")] internal partial struct applyBlack_translations {
    internal slice<byte> src;
    internal nint stride;
}

// applyBlack combines d.img3 and d.blackPix into a CMYK image. The formula
// used depends on whether the JPEG image is stored as CMYK or YCbCrK,
// indicated by the APP14 (Adobe) metadata.
//
// Adobe CMYK JPEG images are inverted, where 255 means no ink instead of full
// ink, so we apply "v = 255 - v" at various points. Note that a double
// inversion is a no-op, so inversions might be implicit in the code below.
[GoRecv] internal static (image.Image, error) applyBlack(this ref decoder d) {
    if (!d.adobeTransformValid) {
        return (default!, ((UnsupportedError)(@string)unknownColorModel4ˢ));
    }
    // If the 4-component JPEG image isn't explicitly marked as "Unknown (RGB
    // or CMYK)" as per
    // https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
    // we assume that it is YCbCrK. This matches libjpeg's jdapimin.c.
    if (d.adobeTransform != adobeTransformUnknown) {
        // Convert the YCbCr part of the YCbCrK to RGB, invert the RGB to get
        // CMY, and patch in the original K. The RGB to CMY inversion cancels
        // out the 'Adobe inversion' described in the applyBlack doc comment
        // above, so in practice, only the fourth channel (black) is inverted.
        var boundsΔ1 = d.img3.Bounds();
        var imgΔ1 = image.NewRGBA(boundsΔ1);
        imageutil.DrawYCbCr(imgΔ1, boundsΔ1, d.img3, boundsΔ1.Min);
        for ((nint iBase, nint y) = (0, boundsΔ1.Min.Y); y < boundsΔ1.Max.Y; (iBase, y) = (iBase + (~imgΔ1).Stride, y + 1)) {
            for ((nint i, nint x) = (iBase + 3, boundsΔ1.Min.X); x < boundsΔ1.Max.X; (i, x) = (i + 4, x + 1)) {
                imgΔ1.Value.Pix[i] = (uint8)(255 - d.blackPix[(y - boundsΔ1.Min.Y) * d.blackStride + (x - boundsΔ1.Min.X)]);
            }
        }
        return (new image.CMYKжImage(Ꮡ(new image.CMYK(
            Pix: (~imgΔ1).Pix,
            Stride: (~imgΔ1).Stride,
            Rect: (~imgΔ1).Rect
        ))), default!);
    }
    // The first three channels (cyan, magenta, yellow) of the CMYK
    // were decoded into d.img3, but each channel was decoded into a separate
    // []byte slice, and some channels may be subsampled. We interleave the
    // separate channels into an image.CMYK's single []byte slice containing 4
    // contiguous bytes per pixel.
    var bounds = d.img3.Bounds();
    var img = image.NewCMYK(bounds);
    var translations = new applyBlack_translations[]{
        new((~d.img3).Y, (~d.img3).YStride),
        new((~d.img3).Cb, (~d.img3).CStride),
        new((~d.img3).Cr, (~d.img3).CStride),
        new(d.blackPix, d.blackStride)
    }.array();
    foreach (var (t, translation) in translations.ΔRangeSnapshot()) {
        var subsample = d.comp[t].h != d.comp[0].h || d.comp[t].v != d.comp[0].v;
        for ((nint iBase, nint y) = (0, bounds.Min.Y); y < bounds.Max.Y; (iBase, y) = (iBase + (~img).Stride, y + 1)) {
            nint sy = y - bounds.Min.Y;
            if (subsample) {
                sy /= 2;
            }
            for ((nint i, nint x) = (iBase + t, bounds.Min.X); x < bounds.Max.X; (i, x) = (i + 4, x + 1)) {
                nint sx = x - bounds.Min.X;
                if (subsample) {
                    sx /= 2;
                }
                img.Value.Pix[i] = (uint8)(255 - translation.src[sy * translation.stride + sx]);
            }
        }
    }
    return (new image.CMYKжImage(img), default!);
}

[GoRecv] internal static bool isRGB(this ref decoder d) {
    if (d.jfif) {
        return false;
    }
    if (d.adobeTransformValid && d.adobeTransform == adobeTransformUnknown) {
        // https://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/JPEG.html#Adobe
        // says that 0 means Unknown (and in practice RGB) and 1 means YCbCr.
        return true;
    }
    return d.comp[0].c == (rune)'R' && d.comp[1].c == (rune)'G' && d.comp[2].c == (rune)'B';
}

[GoRecv] internal static (image.Image, error) convertToRGB(this ref decoder d) {
    nint cScale = d.comp[0].h / d.comp[1].h;
    var bounds = d.img3.Bounds();
    var img = image.NewRGBA(bounds);
    for (nint y = bounds.Min.Y; y < bounds.Max.Y; y++) {
        nint po = img.PixOffset(bounds.Min.X, y);
        nint yo = d.img3.YOffset(bounds.Min.X, y);
        nint co = d.img3.COffset(bounds.Min.X, y);
        for ((nint i, nint iMax) = (0, bounds.Max.X - bounds.Min.X); i < iMax; i++) {
            img.Value.Pix[po + 4 * i + 0] = (~d.img3).Y[yo + i];
            img.Value.Pix[po + 4 * i + 1] = (~d.img3).Cb[co + i / cScale];
            img.Value.Pix[po + 4 * i + 2] = (~d.img3).Cr[co + i / cScale];
            img.Value.Pix[po + 4 * i + 3] = 255;
        }
    }
    return (new image.ΔRGBAжImage(img), default!);
}

// Decode reads a JPEG image from r and returns it as an [image.Image].
public static (image.Image, error) Decode(io.Reader r) {
    decoder d = new();
    return d.decode(r, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingSofMarkerˢ = "missing SOF marker"u8;

// DecodeConfig returns the color model and dimensions of a JPEG image without
// decoding the entire image.
public static (image.Config, error) DecodeConfig(io.Reader r) {
    decoder d = new();
    {
        var (_, err) = d.decode(r, true); if (err != default!) {
            return (new image.Config(nil), err);
        }
    }
    switch (d.nComp) {
    case 1: {
        return (new image.Config(
            ColorModel: color.GrayModel,
            Width: d.width,
            Height: d.height
        ), default!);
    }
    case 3: {
        var cm = color.YCbCrModel;
        if (d.isRGB()) {
            cm = color.RGBAModel;
        }
        return (new image.Config(
            ColorModel: cm,
            Width: d.width,
            Height: d.height
        ), default!);
    }
    case 4: {
        return (new image.Config(
            ColorModel: color.CMYKModel,
            Width: d.width,
            Height: d.height
        ), default!);
    }}

    return (new image.Config(nil), ((FormatError)(@string)missingSofMarkerˢ));
}

[GoInit] internal static void init() {
    image.RegisterFormat("jpeg"u8, ((@string)(new byte[]{0xff, 0xd8})), Decode, DecodeConfig);
}

} // end jpeg_package
