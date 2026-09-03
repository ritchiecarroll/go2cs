// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bufio = bufio_package;
using errors = errors_package;
using image = image_package;
using color = go.image.color_package;
using io = io_package;
using go.image;

partial class jpeg_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸbufio() {
    builtin.initPackage(typeof(bufio_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸerrors() {
    builtin.initPackage(typeof(errors_package));
}

// div returns a/b rounded to the nearest integer, instead of rounded to zero.
internal static int32 div(int32 a, int32 b) {
    if (a >= 0) {
        return (a + ((b >> (int)(1)))) / b;
    }
    return -((-a + ((b >> (int)(1)))) / b);
}

// bitCount counts the number of bits needed to hold an integer.
internal static array<byte> bitCount = new byte[]{
    0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
    5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
}.array();

[GoType("num:nint")] partial struct quantIndex;

internal static quantIndex quantIndexLuminance => /* iota */ 0;
internal static quantIndex quantIndexChrominance => 1;
internal static quantIndex nQuantIndex => 2;

// Luminance.
// Chrominance.
// unscaledQuant are the unscaled quantization tables in zig-zag order. Each
// encoder copies and scales the tables according to its quality parameter.
// The values are derived from section K.1 after converting from natural to
// zig-zag order.
internal static array<array<byte>> unscaledQuant = new array<byte>[]{
    new byte[]{
        16, 11, 12, 14, 12, 10, 16, 14,
        13, 14, 18, 17, 16, 19, 24, 40,
        26, 24, 22, 22, 24, 49, 35, 37,
        29, 40, 58, 51, 61, 60, 57, 51,
        56, 55, 64, 72, 92, 78, 64, 68,
        87, 69, 55, 56, 80, 109, 81, 87,
        95, 98, 103, 104, 103, 62, 77, 113,
        121, 112, 100, 120, 92, 101, 103, 99}.array(),
    new byte[]{
        17, 18, 18, 24, 21, 24, 47, 26,
        26, 47, 99, 66, 56, 66, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99}.array()
}.array();

[GoType("num:nint")] partial struct huffIndex;

internal static huffIndex huffIndexLuminanceDC => /* iota */ 0;
internal static huffIndex huffIndexLuminanceAC => 1;
internal static huffIndex huffIndexChrominanceDC => 2;
internal static huffIndex huffIndexChrominanceAC => 3;
internal static huffIndex nHuffIndex => 4;

// huffmanSpec specifies a Huffman encoding.
[GoType] partial struct huffmanSpec {
    // count[i] is the number of codes of length i bits.
    internal array<byte> count = new(16);
    // value[i] is the decoded value of the i'th codeword.
    internal slice<byte> value;
}

// Luminance DC.
// Luminance AC.
// Chrominance DC.
// Chrominance AC.
// theHuffmanSpec is the Huffman encoding specifications.
// This encoder uses the same Huffman encoding for all images.
internal static array<huffmanSpec> theHuffmanSpec = new huffmanSpec[]{
    new(
        new byte[]{0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0}.array(),
        new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.slice()
    ),
    new(
        new byte[]{0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 125}.array(),
        new byte[]{
            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12,
            0x21, 0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07,
            0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08,
            0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0,
            0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16,
            0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
            0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
            0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
            0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79,
            0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
            0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98,
            0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7,
            0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6,
            0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5,
            0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4,
            0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
            0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea,
            0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
            0xf9, 0xfa
        }.slice()
    ),
    new(
        new byte[]{0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}.array(),
        new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}.slice()
    ),
    new(
        new byte[]{0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 119}.array(),
        new byte[]{
            0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21,
            0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71,
            0x13, 0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91,
            0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0,
            0x15, 0x62, 0x72, 0xd1, 0x0a, 0x16, 0x24, 0x34,
            0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26,
            0x27, 0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
            0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
            0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
            0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96,
            0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5,
            0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4,
            0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3,
            0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2,
            0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
            0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9,
            0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
            0xf9, 0xfa
        }.slice()
    )
}.array();

[GoType("[]uint32")] partial struct huffmanLUT;

[GoRecv] internal static void init(this ref huffmanLUT h, huffmanSpec s) {
    s = s.ΔClone();

    nint maxValue = 0;
    foreach (var (_, v) in s.value) {
        if ((nint)v > maxValue) {
            maxValue = (nint)v;
        }
    }
    h = new slice<uint32>(maxValue + 1);
    var code = (uint32)0;
    nint k = 0;
    for (nint i = 0; i < len(s.count); i++) {
        var nBits = ((uint32)(i + 1) << (int)(24));
        for (var j = (uint8)0; j < s.count[i]; j++) {
            (h)[s.value[k]] = (uint32)(nBits | code);
            code++;
            k++;
        }
        code <<= (int)(1);
    }
}

// theHuffmanLUT are compiled representations of theHuffmanSpec.
internal static ж<array<huffmanLUT>> ᏑtheHuffmanLUT = new StandardBox<array<huffmanLUT>>(new array<huffmanLUT>(4));
internal static ref array<huffmanLUT> theHuffmanLUT => ref ᏑtheHuffmanLUT.Value;

[GoInit] internal static void initΔ1() {
    foreach (var (i, vᴛ1) in theHuffmanSpec.ΔRangeSnapshot()) {
        var s = vᴛ1.ΔClone();

        theHuffmanLUT[i].init(s);
    }
}

// writer is a buffered writer.
[GoType] partial interface writer :
    io.Writer,
    io.ByteWriter
{
    error Flush();
}

// encoder encodes an image to the JPEG format.
[GoType] partial struct encoder {
    // w is the writer to write to. err is the first error encountered during
    // writing. All attempted writes after the first error become no-ops.
    internal writer w;
    internal error err;
    // buf is a scratch buffer.
    internal array<byte> buf = new(16);
    // bits and nBits are accumulated bits to write to w.
    internal uint32 bits, nBits;
    // quant is the scaled quantization tables, in zig-zag order.
    internal array<array<byte>> quant = new(nQuantIndex, () => new(64));
}

[GoRecv] internal static void flush(this ref encoder e) {
    if (e.err != default!) {
        return;
    }
    e.err = e.w.Flush();
}

[GoRecv] internal static void write(this ref encoder e, slice<byte> p) {
    if (e.err != default!) {
        return;
    }
    (_, e.err) = e.w.Write(p);
}

[GoRecv] internal static void writeByte(this ref encoder e, byte b) {
    if (e.err != default!) {
        return;
    }
    e.err = e.w.WriteByte(b);
}

// emit emits the least significant nBits bits of bits to the bit-stream.
// The precondition is bits < 1<<nBits && nBits <= 16.
[GoRecv] internal static void emit(this ref encoder e, uint32 bits, uint32 nBits) {
    nBits += e.nBits;
    bits <<= (int)(32 - nBits);
    bits |= (uint32)(e.bits);
    while (nBits >= 8) {
        var b = (uint8)((bits >> (int)(24)));
        e.writeByte(b);
        if (b == 0xff) {
            e.writeByte(0x00);
        }
        bits <<= (int)(8);
        nBits -= 8;
    }
    (e.bits, e.nBits) = (bits, nBits);
}

// emitHuff emits the given value with the given Huffman encoder.
[GoRecv] internal static void emitHuff(this ref encoder e, huffIndex h, int32 value) {
    var x = theHuffmanLUT[h][value];
    e.emit((uint32)(x & ((1 << (int)(24)) - 1)), (x >> (int)(24)));
}

// emitHuffRLE emits a run of runLength copies of value encoded with the given
// Huffman encoder.
[GoRecv] internal static void emitHuffRLE(this ref encoder e, huffIndex h, int32 runLength, int32 value) {
    var (a, b) = (value, value);
    if (a < 0) {
        (a, b) = (-value, value - 1);
    }
    uint32 nBits = default!;
    if (a < 0x100){
        nBits = (uint32)bitCount[a];
    } else {
        nBits = 8 + (uint32)bitCount[(a >> (int)(8))];
    }
    e.emitHuff(h, (int32)((runLength << (int)(4)) | (int32)nBits));
    if (nBits > 0) {
        e.emit((uint32)((uint32)b & (((uint32)1).Lsh((uint64)(nBits)) - 1)), nBits);
    }
}

// writeMarkerHeader writes the header for a marker with the given length.
[GoRecv] internal static void writeMarkerHeader(this ref encoder e, uint8 marker, nint markerlen) {
    e.buf[0] = 0xff;
    e.buf[1] = marker;
    e.buf[2] = (uint8)((markerlen >> (int)(8)));
    e.buf[3] = (uint8)((nint)(markerlen & 0xff));
    e.write(e.buf[..4]);
}

// writeDQT writes the Define Quantization Table marker.
[GoRecv] internal static void writeDQT(this ref encoder e) {
    const nint markerlen = /* 2 + int(nQuantIndex)*(1+blockSize) */ 132;
    e.writeMarkerHeader(dqtMarker, markerlen);
    foreach (var (i, _) in e.quant) {
        e.writeByte((uint8)i);
        e.write(e.quant[i][..]);
    }
}

// writeSOF0 writes the Start Of Frame (Baseline Sequential) marker.
[GoRecv] internal static void writeSOF0(this ref encoder e, image.Point size, nint nComponent) {
    nint markerlen = 8 + 3 * nComponent;
    e.writeMarkerHeader(sof0Marker, markerlen);
    e.buf[0] = 8; // 8-bit color.
    e.buf[1] = (uint8)((size.Y >> (int)(8)));
    e.buf[2] = (uint8)((nint)(size.Y & 0xff));
    e.buf[3] = (uint8)((size.X >> (int)(8)));
    e.buf[4] = (uint8)((nint)(size.X & 0xff));
    e.buf[5] = (uint8)nComponent;
    if (nComponent == 1){
        e.buf[6] = 1;
        // No subsampling for grayscale image.
        e.buf[7] = 0x11;
        e.buf[8] = 0x00;
    } else {
        for (nint i = 0; i < nComponent; i++) {
            e.buf[3 * i + 6] = (uint8)(i + 1);
            // We use 4:2:0 chroma subsampling.
            e.buf[3 * i + 7] = "\x22\x11\x11"u8[(int)(i)];
            e.buf[3 * i + 8] = "\x00\x01\x01"u8[(int)(i)];
        }
    }
    e.write(e.buf[..(int)(3 * (nComponent - 1) + 9)]);
}

// writeDHT writes the Define Huffman Table marker.
[GoRecv] internal static void writeDHT(this ref encoder e, nint nComponent) {
    nint markerlen = 2;
    var specs = theHuffmanSpec[..];
    if (nComponent == 1) {
        // Drop the Chrominance tables.
        specs = specs[..2];
    }
    foreach (var (_, vᴛ1) in specs) {
        var s = vᴛ1.ΔClone();

        markerlen += 1 + 16 + len(s.value);
    }
    e.writeMarkerHeader(dhtMarker, markerlen);
    foreach (var (i, vᴛ2) in specs) {
        var s = vᴛ2.ΔClone();

        e.writeByte("\x00\x10\x01\x11"u8[(int)(i)]);
        e.write(s.count[..]);
        e.write(s.value);
    }
}

// writeBlock writes a block of pixel data using the given quantization table,
// returning the post-quantized DC value of the DCT-transformed block. b is in
// natural (not zig-zag) order.
[GoRecv] internal static int32 writeBlock(this ref encoder e, ж<block> Ꮡb, quantIndex q, int32 prevDC) {
    ref var b = ref Ꮡb.DerefOrNull();

    fdct(Ꮡb);
    // Emit the DC delta.
    var dc = div(b[0], 8 * (int32)e.quant[q][0]);
    e.emitHuffRLE(((huffIndex)(nint)(2 * q + 0)), 0, dc - prevDC);
    // Emit the AC components.
    huffIndex h = ((huffIndex)(nint)(2 * q + 1));
    var runLength = (int32)0;
    for (nint zig = 1; zig < blockSize; zig++) {
        var ac = div(b[unzig[zig]], 8 * (int32)e.quant[q][zig]);
        if (ac == 0){
            runLength++;
        } else {
            while (runLength > 15) {
                e.emitHuff(h, 0xf0);
                runLength -= 16;
            }
            e.emitHuffRLE(h, runLength, ac);
            runLength = 0;
        }
    }
    if (runLength > 0) {
        e.emitHuff(h, 0x00);
    }
    return dc;
}

// toYCbCr converts the 8x8 region of m whose top-left corner is p to its
// YCbCr values.
internal static void toYCbCr(image.Image m, image.Point p, ref block yBlock, ref block cbBlock, ref block crBlock) {
    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    for (nint j = 0; j < 8; j++) {
        for (nint i = 0; i < 8; i++) {
            var (r, g, bΔ1, _) = m.At(min(p.X + i, xmax), min(p.Y + j, ymax)).RGBA();
            var (yy, cb, cr) = color.RGBToYCbCr((uint8)((r >> (int)(8))), (uint8)((g >> (int)(8))), (uint8)((bΔ1 >> (int)(8))));
            yBlock[8 * j + i] = (int32)yy;
            cbBlock[8 * j + i] = (int32)cb;
            crBlock[8 * j + i] = (int32)cr;
        }
    }
}

// grayToY stores the 8x8 region of m whose top-left corner is p in yBlock.
internal static void grayToY(ж<image.Gray> Ꮡm, image.Point p, ref block yBlock) {
    ref var m = ref Ꮡm.DerefOrNull();

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    var pix = m.Pix;
    for (nint j = 0; j < 8; j++) {
        for (nint i = 0; i < 8; i++) {
            nint idx = m.PixOffset(min(p.X + i, xmax), min(p.Y + j, ymax));
            yBlock[8 * j + i] = (int32)pix[idx];
        }
    }
}

// rgbaToYCbCr is a specialized version of toYCbCr for image.RGBA images.
internal static void rgbaToYCbCr(ж<imageꓸRGBA> Ꮡm, image.Point p, ref block yBlock, ref block cbBlock, ref block crBlock) {
    ref var m = ref Ꮡm.DerefOrNull();

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    for (nint j = 0; j < 8; j++) {
        nint sj = p.Y + j;
        if (sj > ymax) {
            sj = ymax;
        }
        nint offset = (sj - b.Min.Y) * m.Stride - b.Min.X * 4;
        for (nint i = 0; i < 8; i++) {
            nint sx = p.X + i;
            if (sx > xmax) {
                sx = xmax;
            }
            var pix = m.Pix[(int)(offset + sx * 4)..];
            var (yy, cb, cr) = color.RGBToYCbCr(pix[0], pix[1], pix[2]);
            yBlock[8 * j + i] = (int32)yy;
            cbBlock[8 * j + i] = (int32)cb;
            crBlock[8 * j + i] = (int32)cr;
        }
    }
}

// yCbCrToYCbCr is a specialized version of toYCbCr for image.YCbCr images.
internal static void yCbCrToYCbCr(ж<image.YCbCr> Ꮡm, image.Point p, ref block yBlock, ref block cbBlock, ref block crBlock) {
    ref var m = ref Ꮡm.DerefOrNull();

    var b = m.Bounds();
    nint xmax = b.Max.X - 1;
    nint ymax = b.Max.Y - 1;
    for (nint j = 0; j < 8; j++) {
        nint sy = p.Y + j;
        if (sy > ymax) {
            sy = ymax;
        }
        for (nint i = 0; i < 8; i++) {
            nint sx = p.X + i;
            if (sx > xmax) {
                sx = xmax;
            }
            nint yi = m.YOffset(sx, sy);
            nint ci = m.COffset(sx, sy);
            yBlock[8 * j + i] = (int32)m.Y[yi];
            cbBlock[8 * j + i] = (int32)m.Cb[ci];
            crBlock[8 * j + i] = (int32)m.Cr[ci];
        }
    }
}

// scale scales the 16x16 region represented by the 4 src blocks to the 8x8
// dst block.
internal static void scale(ref block dst, ref array<block> src) {
    for (nint i = 0; i < 4; i++) {
        nint dstOff = (nint)((((nint)(i & 2)) << (int)(4)) | (((nint)(i & 1)) << (int)(2)));
        for (nint y = 0; y < 4; y++) {
            for (nint x = 0; x < 4; x++) {
                nint j = 16 * y + 2 * x;
                var sum = src[i][j] + src[i][j + 1] + src[i][j + 8] + src[i][j + 9];
                dst[8 * y + x + dstOff] = ((sum + 2) >> (int)(2));
            }
        }
    }
}

// sosHeaderY is the SOS marker "\xff\xda" followed by 8 bytes:
//   - the marker length "\x00\x08",
//   - the number of components "\x01",
//   - component 1 uses DC table 0 and AC table 0 "\x01\x00",
//   - the bytes "\x00\x3f\x00". Section B.2.3 of the spec says that for
//     sequential DCTs, those bytes (8-bit Ss, 8-bit Se, 4-bit Ah, 4-bit Al)
//     should be 0x00, 0x3f, 0x00<<4 | 0x00.
internal static slice<byte> sosHeaderY = new byte[]{
    0xff, 0xda, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3f, 0x00
}.slice();

// sosHeaderYCbCr is the SOS marker "\xff\xda" followed by 12 bytes:
//   - the marker length "\x00\x0c",
//   - the number of components "\x03",
//   - component 1 uses DC table 0 and AC table 0 "\x01\x00",
//   - component 2 uses DC table 1 and AC table 1 "\x02\x11",
//   - component 3 uses DC table 1 and AC table 1 "\x03\x11",
//   - the bytes "\x00\x3f\x00". Section B.2.3 of the spec says that for
//     sequential DCTs, those bytes (8-bit Ss, 8-bit Se, 4-bit Ah, 4-bit Al)
//     should be 0x00, 0x3f, 0x00<<4 | 0x00.
internal static slice<byte> sosHeaderYCbCr = new byte[]{
    0xff, 0xda, 0x00, 0x0c, 0x03, 0x01, 0x00, 0x02,
    0x11, 0x03, 0x11, 0x00, 0x3f, 0x00
}.slice();

// writeSOS writes the StartOfScan marker.
[GoRecv] internal static void writeSOS(this ref encoder e, image.Image m) {
    switch (m.type()) {
    case ж<image.Gray>: {
        e.write(sosHeaderY);
        break;
    }
    default: {
        e.write(sosHeaderYCbCr);
        break;
    }}

    ref var b = ref heap(new block(), out var Ꮡb);
    array<block> cb = new(4);
    array<block> cr = new(4);
    int32 prevDCY = default!;
    int32 prevDCCb = default!;
    int32 prevDCCr = default!;
    var bounds = m.Bounds();
    switch (m.type()) {
    case ж<image.Gray> mΔ1: {
        for (nint y = bounds.Min.Y; y < bounds.Max.Y; y += 8) {
            // TODO(wathiede): switch on m.ColorModel() instead of type.
            for (nint x = bounds.Min.X; x < bounds.Max.X; x += 8) {
                var p = image.Pt(x, y);
                grayToY(mΔ1, p, ref b);
                prevDCY = e.writeBlock(Ꮡb, 0, prevDCY);
            }
        }
        break;
    }
    default: {
        var mΔ1 = m;
        var (rgba, _) = mΔ1._<ж<imageꓸRGBA>>(ᐧ);
        var (ycbcr, _) = mΔ1._<ж<image.YCbCr>>(ᐧ);
        for (nint y = bounds.Min.Y; y < bounds.Max.Y; y += 16) {
            for (nint x = bounds.Min.X; x < bounds.Max.X; x += 16) {
                for (nint i = 0; i < 4; i++) {
                    nint xOff = ((nint)(i & 1)) * 8;
                    nint yOff = ((nint)(i & 2)) * 4;
                    var p = image.Pt(x + xOff, y + yOff);
                    if (rgba != nil){
                        rgbaToYCbCr(rgba, p, ref b, ref cb[i], ref cr[i]);
                    } else 
                    if (ycbcr != nil){
                        yCbCrToYCbCr(ycbcr, p, ref b, ref cb[i], ref cr[i]);
                    } else {
                        toYCbCr(mΔ1, p, ref b, ref cb[i], ref cr[i]);
                    }
                    prevDCY = e.writeBlock(Ꮡb, 0, prevDCY);
                }
                scale(ref b, ref cb);
                prevDCCb = e.writeBlock(Ꮡb, 1, prevDCCb);
                scale(ref b, ref cr);
                prevDCCr = e.writeBlock(Ꮡb, 1, prevDCCr);
            }
        }
        break;
    }}
    // Pad the last byte with 1's.
    e.emit(0x7f, 7);
}

// DefaultQuality is the default quality encoding parameter.
public static UntypedInt DefaultQuality => 75;

// Options are the encoding parameters.
// Quality ranges from 1 to 100 inclusive, higher is better.
[GoType] partial struct Options {
    public nint Quality;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string jpegImageIsTooLargeToˢ = "jpeg: image is too large to encode"u8;

// Encode writes the Image m to w in JPEG 4:2:0 baseline format with the given
// options. Default parameters are used if a nil *[Options] is passed.
public static error Encode(io.Writer w, image.Image m, ж<Options> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    var b = m.Bounds();
    if (b.Dx() >= (1 << (int)(16)) || b.Dy() >= (1 << (int)(16))) {
        return errors.New(jpegImageIsTooLargeToˢ);
    }
    encoder e = new();
    {
        var (ww, ok) = w._<writer>(ᐧ); if (ok){
            e.w = ww;
        } else {
            e.w = new bufio_Writerжwriter(bufio.NewWriter(w));
        }
    }
    // Clip quality to [1, 100].
    nint quality = DefaultQuality;
    if (Ꮡo != nil) {
        quality = o.Quality;
        if (quality < 1){
            quality = 1;
        } else 
        if (quality > 100) {
            quality = 100;
        }
    }
    // Convert from a quality rating to a scaling factor.
    nint scale = default!;
    if (quality < 50){
        scale = 5000 / quality;
    } else {
        scale = 200 - quality * 2;
    }
    // Initialize the quantization tables.
    foreach (var (i, _) in e.quant) {
        foreach (var (j, _) in e.quant[i]) {
            nint x = (nint)unscaledQuant[i][j];
            x = (x * scale + 50) / 100;
            if (x < 1){
                x = 1;
            } else 
            if (x > 255) {
                x = 255;
            }
            e.quant[i][j] = (uint8)x;
        }
    }
    // Compute number of components based on input image type.
    nint nComponent = 3;
    switch (m.type()) {
    case ж<image.Gray>: {
        nComponent = 1;
        break;
    }}

    // TODO(wathiede): switch on m.ColorModel() instead of type.
    // Write the Start Of Image marker.
    e.buf[0] = 0xff;
    e.buf[1] = 0xd8;
    e.write(e.buf[..2]);
    // Write the quantization tables.
    e.writeDQT();
    // Write the image dimensions.
    e.writeSOF0(b.Size(), nComponent);
    // Write the Huffman tables.
    e.writeDHT(nComponent);
    // Write the image data.
    e.writeSOS(m);
    // Write the End Of Image marker.
    e.buf[0] = 0xff;
    e.buf[1] = 0xd9;
    e.write(e.buf[..2]);
    e.flush();
    return e.err;
}

} // end jpeg_package
