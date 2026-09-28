// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bufio = bufio_package;
using zlib = compress.zlib_package;
using binary = encoding.binary_package;
using crc32 = go.hash.crc32_package;
using image = image_package;
using color = go.image.color_package;
using io = io_package;
using strconv = strconv_package;
using compress;
using encoding;
using go.hash;
using go.image;
using hash = hash_package;

partial class png_package {

// Encoder configures encoding PNG images.
[GoType] partial struct Encoder {
    public CompressionLevel CompressionLevel;
    // BufferPool optionally specifies a buffer pool to get temporary
    // EncoderBuffers when encoding an image.
    public EncoderBufferPool BufferPool;
}

// EncoderBufferPool is an interface for getting and returning temporary
// instances of the [EncoderBuffer] struct. This can be used to reuse buffers
// when encoding multiple images.
[GoType] partial interface EncoderBufferPool {
    ж<EncoderBuffer> Get();
    void Put(ж<EncoderBuffer> _);
}

[GoType("encoder")] partial struct EncoderBuffer;

[GoType] public partial struct encoder {
    internal ж<Encoder> enc;
    internal io.Writer w;
    internal image.Image m;
    internal nint cb;
    internal error err;
    internal array<byte> header = new(8);
    internal array<byte> footer = new(4);
    internal array<byte> tmp = new(4 * 256);
    internal array<slice<uint8>> cr = new(nFilter);
    internal slice<uint8> pr;
    internal ж<zlib.Writer> zw;
    internal nint zwLevel;
    internal ж<bufio.Writer> bw;
}

[GoType("num:nint")] partial struct CompressionLevel;

public static CompressionLevel DefaultCompression => 0;
public static CompressionLevel NoCompression => -1;
public static CompressionLevel BestSpeed => -2;
public static CompressionLevel BestCompression => -3;

// Positive CompressionLevel values are reserved to mean a numeric zlib
// compression level, although that is not implemented yet.
[GoType] partial interface opaquer {
    bool Opaque();
}

// Returns whether or not the image is fully opaque.
internal static bool opaque(image.Image m) {
    {
        var (o, ok) = m._<opaquer>(ᐧ); if (ok) {
            return o.Opaque();
        }
    }
    var b = m.Bounds();
    for (nint y = b.Min.Y; y < b.Max.Y; y++) {
        for (nint x = b.Min.X; x < b.Max.X; x++) {
            var (_, _, _, a) = m.At(x, y).RGBA();
            if (a != 0xffff) {
                return false;
            }
        }
    }
    return true;
}

// The absolute value of a byte interpreted as a signed int8.
internal static nint abs8(uint8 d) {
    if (d < 128) {
        return (nint)d;
    }
    return 256 - (nint)d;
}

[GoRecv] internal static void writeChunk(this ref encoder e, slice<byte> b, @string name) {
    if (e.err != default!) {
        return;
    }
    var n = (uint32)len(b);
    if ((nint)n != len(b)) {
        e.err = ((UnsupportedError)(name + " chunk is too large: "u8 + strconv.Itoa(len(b))));
        return;
    }
    binary.BigEndian.PutUint32(e.header[..4], n);
    e.header[4] = name[0];
    e.header[5] = name[1];
    e.header[6] = name[2];
    e.header[7] = name[3];
    var crc = crc32.NewIEEE();
    crc.Write(e.header[4..8]);
    crc.Write(b);
    binary.BigEndian.PutUint32(e.footer[..4], crc.Sum32());
    (_, e.err) = e.w.Write(e.header[..8]);
    if (e.err != default!) {
        return;
    }
    (_, e.err) = e.w.Write(b);
    if (e.err != default!) {
        return;
    }
    (_, e.err) = e.w.Write(e.footer[..4]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ihdrˢ = "IHDR"u8;

[GoRecv] internal static void writeIHDR(this ref encoder e) {
    var b = e.m.Bounds();
    binary.BigEndian.PutUint32(e.tmp[0..4], (uint32)b.Dx());
    binary.BigEndian.PutUint32(e.tmp[4..8], (uint32)b.Dy());
    // Set bit depth and color type.
    var exprᴛ1 = e.cb;
    if (exprᴛ1 == cbG8) {
        e.tmp[8] = 8;
        e.tmp[9] = ctGrayscale;
    }
    else if (exprᴛ1 == cbTC8) {
        e.tmp[8] = 8;
        e.tmp[9] = ctTrueColor;
    }
    else if (exprᴛ1 == cbP8) {
        e.tmp[8] = 8;
        e.tmp[9] = ctPaletted;
    }
    else if (exprᴛ1 == cbP4) {
        e.tmp[8] = 4;
        e.tmp[9] = ctPaletted;
    }
    else if (exprᴛ1 == cbP2) {
        e.tmp[8] = 2;
        e.tmp[9] = ctPaletted;
    }
    else if (exprᴛ1 == cbP1) {
        e.tmp[8] = 1;
        e.tmp[9] = ctPaletted;
    }
    else if (exprᴛ1 == cbTCA8) {
        e.tmp[8] = 8;
        e.tmp[9] = ctTrueColorAlpha;
    }
    else if (exprᴛ1 == cbG16) {
        e.tmp[8] = 16;
        e.tmp[9] = ctGrayscale;
    }
    else if (exprᴛ1 == cbTC16) {
        e.tmp[8] = 16;
        e.tmp[9] = ctTrueColor;
    }
    else if (exprᴛ1 == cbTCA16) {
        e.tmp[8] = 16;
        e.tmp[9] = ctTrueColorAlpha;
    }

    e.tmp[10] = 0; // default compression method
    e.tmp[11] = 0; // default filter method
    e.tmp[12] = 0; // non-interlaced
    e.writeChunk(e.tmp[..13], ihdrˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string plteˢ = "PLTE"u8;
internal static readonly @string tRNSˢ = "tRNS"u8;

[GoRecv] internal static void writePLTEAndTRNS(this ref encoder e, color.Palette p) {
    if (len(p) < 1 || len(p) > 256) {
        e.err = ((FormatError)("bad palette length: "u8 + strconv.Itoa(len(p))));
        return;
    }
    nint last = -1;
    foreach (var (i, c) in p) {
        var c1 = color.NRGBAModel.Convert(c)._<color.NRGBA>();
        e.tmp[3 * i + 0] = c1.R;
        e.tmp[3 * i + 1] = c1.G;
        e.tmp[3 * i + 2] = c1.B;
        if (c1.A != 0xff) {
            last = i;
        }
        e.tmp[3 * 256 + i] = c1.A;
    }
    e.writeChunk(e.tmp[..(int)(3 * len(p))], plteˢ);
    if (last != -1) {
        e.writeChunk(e.tmp[(int)(3 * 256)..(int)(3 * 256 + 1 + last)], tRNSˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string idatˢ = "IDAT"u8;

// An encoder is an io.Writer that satisfies writes by writing PNG IDAT chunks,
// including an 8-byte header and 4-byte CRC checksum per Write call. Such calls
// should be relatively infrequent, since writeIDATs uses a [bufio.Writer].
//
// This method should only be called from writeIDATs (via writeImage).
// No other code should treat an encoder as an io.Writer.
[GoRecv] public static (nint, error) Write(this ref encoder e, slice<byte> b) {
    e.writeChunk(b, idatˢ);
    if (e.err != default!) {
        return (0, e.err);
    }
    return (len(b), default!);
}

// Chooses the filter to use for encoding the current row, and applies it.
// The return value is the index of the filter and also of the row in cr that has had it applied.
internal static nint filter(ref array<slice<byte>> cr, slice<byte> pr, nint bpp) {
    // We try all five filter types, and pick the one that minimizes the sum of absolute differences.
    // This is the same heuristic that libpng uses, although the filters are attempted in order of
    // estimated most likely to be minimal (ftUp, ftPaeth, ftNone, ftSub, ftAverage), rather than
    // in their enumeration order (ftNone, ftSub, ftUp, ftAverage, ftPaeth).
    var cdat0 = cr[0][1..];
    var cdat1 = cr[1][1..];
    var cdat2 = cr[2][1..];
    var cdat3 = cr[3][1..];
    var cdat4 = cr[4][1..];
    var pdat = pr[1..];
    nint n = len(cdat0);
    // The up filter.
    nint sum = 0;
    for (nint i = 0; i < n; i++) {
        cdat2[i] = (byte)(cdat0[i] - pdat[i]);
        sum += abs8(cdat2[i]);
    }
    nint best = sum;
    nint filter = ftUp;
    // The Paeth filter.
    sum = 0;
    for (nint i = 0; i < bpp; i++) {
        cdat4[i] = (byte)(cdat0[i] - pdat[i]);
        sum += abs8(cdat4[i]);
    }
    for (nint i = bpp; i < n; i++) {
        cdat4[i] = (byte)(cdat0[i] - paeth(cdat0[i - bpp], pdat[i], pdat[i - bpp]));
        sum += abs8(cdat4[i]);
        if (sum >= best) {
            break;
        }
    }
    if (sum < best) {
        best = sum;
        filter = ftPaeth;
    }
    // The none filter.
    sum = 0;
    for (nint i = 0; i < n; i++) {
        sum += abs8(cdat0[i]);
        if (sum >= best) {
            break;
        }
    }
    if (sum < best) {
        best = sum;
        filter = ftNone;
    }
    // The sub filter.
    sum = 0;
    for (nint i = 0; i < bpp; i++) {
        cdat1[i] = cdat0[i];
        sum += abs8(cdat1[i]);
    }
    for (nint i = bpp; i < n; i++) {
        cdat1[i] = (byte)(cdat0[i] - cdat0[i - bpp]);
        sum += abs8(cdat1[i]);
        if (sum >= best) {
            break;
        }
    }
    if (sum < best) {
        best = sum;
        filter = ftSub;
    }
    // The average filter.
    sum = 0;
    for (nint i = 0; i < bpp; i++) {
        cdat3[i] = (byte)(cdat0[i] - pdat[i] / 2);
        sum += abs8(cdat3[i]);
    }
    for (nint i = bpp; i < n; i++) {
        cdat3[i] = (byte)(cdat0[i] - (uint8)(((nint)cdat0[i - bpp] + (nint)pdat[i]) / 2));
        sum += abs8(cdat3[i]);
        if (sum >= best) {
            break;
        }
    }
    if (sum < best) {
        filter = ftAverage;
    }
    return filter;
}

internal static error writeImage(this ж<encoder> Ꮡe, io.Writer w, image.Image m, nint cb, nint level) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var e = ref Ꮡe.DerefOrNull();

        if (e.zw == nil || e.zwLevel != level){
            var (zw, err) = zlib.NewWriterLevel(w, level);
            if (err != default!) {
                return err;
            }
            e.zw = zw;
            e.zwLevel = level;
        } else {
            e.zw.Reset(w);
        }
        ᒐd1 = true;
        nint bitsPerPixel = 0;
        var exprᴛ1 = cb;
        if (exprᴛ1 == cbG8) {
            bitsPerPixel = 8;
        }
        else if (exprᴛ1 == cbTC8) {
            bitsPerPixel = 24;
        }
        else if (exprᴛ1 == cbP8) {
            bitsPerPixel = 8;
        }
        else if (exprᴛ1 == cbP4) {
            bitsPerPixel = 4;
        }
        else if (exprᴛ1 == cbP2) {
            bitsPerPixel = 2;
        }
        else if (exprᴛ1 == cbP1) {
            bitsPerPixel = 1;
        }
        else if (exprᴛ1 == cbTCA8) {
            bitsPerPixel = 32;
        }
        else if (exprᴛ1 == cbTC16) {
            bitsPerPixel = 48;
        }
        else if (exprᴛ1 == cbTCA16) {
            bitsPerPixel = 64;
        }
        else if (exprᴛ1 == cbG16) {
            bitsPerPixel = 16;
        }

        // cr[*] and pr are the bytes for the current and previous row.
        // cr[0] is unfiltered (or equivalently, filtered with the ftNone filter).
        // cr[ft], for non-zero filter types ft, are buffers for transforming cr[0] under the
        // other PNG filter types. These buffers are allocated once and re-used for each row.
        // The +1 is for the per-row filter type, which is at cr[*][0].
        var b = m.Bounds();
        nint sz = 1 + (bitsPerPixel * b.Dx() + 7) / 8;
        foreach (var (i, _) in e.cr) {
            if (cap(e.cr[i]) < sz){
                e.cr[i] = new slice<uint8>(sz);
            } else {
                e.cr[i] = e.cr[i][..(int)(sz)];
            }
            e.cr[i][0] = (uint8)i;
        }
        var cr = e.cr.Clone();
        if (cap(e.pr) < sz){
            e.pr = new slice<uint8>(sz);
        } else {
            e.pr = e.pr[..(int)(sz)];
            clear(e.pr);
        }
        var pr = e.pr;
        var (gray, _) = m._<ж<image.Gray>>(ᐧ);
        var (rgba, _) = m._<ж<imageꓸRGBA>>(ᐧ);
        var (paletted, _) = m._<ж<image.Paletted>>(ᐧ);
        var (nrgba, _) = m._<ж<image.NRGBA>>(ᐧ);
        for (nint y = b.Min.Y; y < b.Max.Y; y++) {
            // Convert from colors to bytes.
            nint i = 1;
            var exprᴛ2 = cb;
            if (exprᴛ2 == cbG8) {
                if (gray != nil){
                    nint offset = (y - b.Min.Y) * (~gray).Stride;
                    copy(cr[0][1..], (~gray).Pix[(int)(offset)..(int)(offset + b.Dx())]);
                } else {
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        var c = color.GrayModel.Convert(m.At(x, y))._<color.Gray>();
                        cr[0][i] = c.Y;
                        i++;
                    }
                }
            }
            else if (exprᴛ2 == cbTC8) {
                var cr0 = cr[0];
                nint stride = 0;
                var pix = slice<byte>(default!);
                if (rgba != nil){
                    // We have previously verified that the alpha value is fully opaque.
                    (stride, pix) = (rgba.Value.Stride, rgba.Value.Pix);
                } else 
                if (nrgba != nil) {
                    (stride, pix) = (nrgba.Value.Stride, nrgba.Value.Pix);
                }
                if (stride != 0){
                    nint j0 = (y - b.Min.Y) * stride;
                    nint j1 = j0 + b.Dx() * 4;
                    for (nint j = j0; j < j1; j += 4) {
                        cr0[i + 0] = pix[j + 0];
                        cr0[i + 1] = pix[j + 1];
                        cr0[i + 2] = pix[j + 2];
                        i += 3;
                    }
                } else {
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        var (r, g, bΔ3, _) = m.At(x, y).RGBA();
                        cr0[i + 0] = (uint8)((r >> (int)(8)));
                        cr0[i + 1] = (uint8)((g >> (int)(8)));
                        cr0[i + 2] = (uint8)((bΔ3 >> (int)(8)));
                        i += 3;
                    }
                }
            }
            else if (exprᴛ2 == cbP8) {
                if (paletted != nil){
                    nint offset = (y - b.Min.Y) * (~paletted).Stride;
                    copy(cr[0][1..], (~paletted).Pix[(int)(offset)..(int)(offset + b.Dx())]);
                } else {
                    var pi = m._<image.PalettedImage>();
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        cr[0][i] = pi.ColorIndexAt(x, y);
                        i += 1;
                    }
                }
            }
            else if (exprᴛ2 == cbP4 || exprᴛ2 == cbP2 || exprᴛ2 == cbP1) {
                var pi = m._<image.PalettedImage>();
                uint8 a = default!;
                nint c = default!;
                nint pixelsPerByte = 8 / bitsPerPixel;
                for (nint x = b.Min.X; x < b.Max.X; x++) {
                    a = (uint8)(a.Lsh((nuint)bitsPerPixel) | pi.ColorIndexAt(x, y));
                    c++;
                    if (c == pixelsPerByte) {
                        cr[0][i] = a;
                        i += 1;
                        a = 0;
                        c = 0;
                    }
                }
                if (c != 0) {
                    while (c != pixelsPerByte) {
                        a = (uint8)(a.Lsh((nuint)bitsPerPixel));
                        c++;
                    }
                    cr[0][i] = a;
                }
            }
            else if (exprᴛ2 == cbTCA8) {
                if (nrgba != nil){
                    nint offset = (y - b.Min.Y) * (~nrgba).Stride;
                    copy(cr[0][1..], (~nrgba).Pix[(int)(offset)..(int)(offset + b.Dx() * 4)]);
                } else 
                if (rgba != nil){
                    var dst = cr[0][1..];
                    var src = (~rgba).Pix[(int)(rgba.PixOffset(b.Min.X, y))..(int)(rgba.PixOffset(b.Max.X, y))];
                    for (; len(src) >= 4; (dst, src) = (dst[4..], src[4..])) {
                        var d = Ꮡ(array<byte>.Alias(dst, 4));
                        var s = Ꮡ(array<byte>.Alias(src, 4));
                        if (s.Value[3] == 0x00){
                            d.Value[0] = 0;
                            d.Value[1] = 0;
                            d.Value[2] = 0;
                            d.Value[3] = 0;
                        } else 
                        if (s.Value[3] == 0xff){
                            copy((~d)[..], (~s)[..]);
                        } else {
                            // This code does the same as color.NRGBAModel.Convert(
                            // rgba.At(x, y)).(color.NRGBA) but with no extra memory
                            // allocations or interface/function call overhead.
                            //
                            // The multiplier m combines 0x101 (which converts
                            // 8-bit color to 16-bit color) and 0xffff (which, when
                            // combined with the division-by-a, converts from
                            // alpha-premultiplied to non-alpha-premultiplied).
                            const uint32 mΔ2 = /* 0x101 * 0xffff */ 16842495;
                            var a = (uint32)s.Value[3] * 0x101;
                            d.Value[0] = (uint8)((((uint32)s.Value[0] * mΔ2 / a) >> (int)(8)));
                            d.Value[1] = (uint8)((((uint32)s.Value[1] * mΔ2 / a) >> (int)(8)));
                            d.Value[2] = (uint8)((((uint32)s.Value[2] * mΔ2 / a) >> (int)(8)));
                            d.Value[3] = s.Value[3];
                        }
                    }
                } else {
                    // Convert from image.Image (which is alpha-premultiplied) to PNG's non-alpha-premultiplied.
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        var c = color.NRGBAModel.Convert(m.At(x, y))._<color.NRGBA>();
                        cr[0][i + 0] = c.R;
                        cr[0][i + 1] = c.G;
                        cr[0][i + 2] = c.B;
                        cr[0][i + 3] = c.A;
                        i += 4;
                    }
                }
            }
            else if (exprᴛ2 == cbG16) {
                for (nint x = b.Min.X; x < b.Max.X; x++) {
                    var c = color.Gray16Model.Convert(m.At(x, y))._<color.Gray16>();
                    cr[0][i + 0] = (uint8)((c.Y >> (int)(8)));
                    cr[0][i + 1] = (uint8)c.Y;
                    i += 2;
                }
            }
            else if (exprᴛ2 == cbTC16) {
                for (nint x = b.Min.X; x < b.Max.X; x++) {
                    // We have previously verified that the alpha value is fully opaque.
                    var (r, g, bΔ4, _) = m.At(x, y).RGBA();
                    cr[0][i + 0] = (uint8)((r >> (int)(8)));
                    cr[0][i + 1] = (uint8)r;
                    cr[0][i + 2] = (uint8)((g >> (int)(8)));
                    cr[0][i + 3] = (uint8)g;
                    cr[0][i + 4] = (uint8)((bΔ4 >> (int)(8)));
                    cr[0][i + 5] = (uint8)bΔ4;
                    i += 6;
                }
            }
            else if (exprᴛ2 == cbTCA16) {
                for (nint x = b.Min.X; x < b.Max.X; x++) {
                    // Convert from image.Image (which is alpha-premultiplied) to PNG's non-alpha-premultiplied.
                    var c = color.NRGBA64Model.Convert(m.At(x, y))._<color.NRGBA64>();
                    cr[0][i + 0] = (uint8)((c.R >> (int)(8)));
                    cr[0][i + 1] = (uint8)c.R;
                    cr[0][i + 2] = (uint8)((c.G >> (int)(8)));
                    cr[0][i + 3] = (uint8)c.G;
                    cr[0][i + 4] = (uint8)((c.B >> (int)(8)));
                    cr[0][i + 5] = (uint8)c.B;
                    cr[0][i + 6] = (uint8)((c.A >> (int)(8)));
                    cr[0][i + 7] = (uint8)c.A;
                    i += 8;
                }
            }

            // Apply the filter.
            // Skip filter for NoCompression and paletted images (cbP8) as
            // "filters are rarely useful on palette images" and will result
            // in larger files (see http://www.libpng.org/pub/png/book/chapter09.html).
            nint f = ftNone;
            if (level != zlib.NoCompression && cb != cbP8 && cb != cbP4 && cb != cbP2 && cb != cbP1) {
                // Since we skip paletted images we don't have to worry about
                // bitsPerPixel not being a multiple of 8
                nint bpp = bitsPerPixel / 8;
                f = filter(ref cr, pr, bpp);
            }
            // Write the compressed bytes.
            {
                var (_, err) = e.zw.Write(cr[f]); if (err != default!) {
                    return err;
                }
            }
            // The current row for y is the previous row for y+1.
            (pr, cr[0]) = (cr[0], pr);
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡe.DerefOrNull().zw.Close(); ᒐ.Run(); }
}

// Write the actual image data to one or more IDAT chunks.
internal static void writeIDATs(this ж<encoder> Ꮡe) {
    ref var e = ref Ꮡe.DerefOrNull();

    if (e.err != default!) {
        return;
    }
    if (e.bw == nil){
        e.bw = bufio.NewWriterSize(new encoderжWriter(Ꮡe), (1 << (int)(15)));
    } else {
        e.bw.Reset(new encoderжWriter(Ꮡe));
    }
    e.err = Ꮡe.writeImage(new bufio_WriterжWriter(e.bw), e.m, e.cb, levelToZlib((~e.enc).CompressionLevel));
    if (e.err != default!) {
        return;
    }
    e.err = e.bw.Flush();
}

// This function is required because we want the zero value of
// Encoder.CompressionLevel to map to zlib.DefaultCompression.
internal static nint levelToZlib(CompressionLevel l) {
    var exprᴛ1 = l;
    if (exprᴛ1 == DefaultCompression) {
        return zlib.DefaultCompression;
    }
    if (exprᴛ1 == NoCompression) {
        return zlib.NoCompression;
    }
    if (exprᴛ1 == BestSpeed) {
        return zlib.BestSpeed;
    }
    if (exprᴛ1 == BestCompression) {
        return zlib.BestCompression;
    }
    { /* default: */
        return zlib.DefaultCompression;
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string iendˢ = "IEND"u8;

[GoRecv] internal static void writeIEND(this ref encoder e) {
    e.writeChunk(default!, iendˢ);
}

// Encode writes the Image m to w in PNG format. Any Image may be
// encoded, but images that are not [image.NRGBA] might be encoded lossily.
public static error Encode(io.Writer w, image.Image m) {
    ref var e = ref heap(new Encoder(), out var Ꮡe);
    return Ꮡe.Encode(w, m);
}

// Encode writes the Image m to w in PNG format.
public static error Encode(this ж<Encoder> Ꮡenc, io.Writer w, image.Image m) {
    GoFrame ᒐ = default;
    try {
        ref var enc = ref Ꮡenc.DerefOrNull();

        // Obviously, negative widths and heights are invalid. Furthermore, the PNG
        // spec section 11.2.2 says that zero is invalid. Excessively large images are
        // also rejected.
        var (mw, mh) = ((int64)m.Bounds().Dx(), (int64)m.Bounds().Dy());
        if (mw <= 0 || mh <= 0 || mw >= 4294967296L || mh >= 4294967296L) {
            return ((FormatError)("invalid image size: "u8 + strconv.FormatInt(mw, 10) + "x"u8 + strconv.FormatInt(mh, 10)));
        }
        ж<encoder> e = default!;
        if (enc.BufferPool != default!) {
            var buffer = enc.BufferPool.Get();
            e = buffer.Reinterpret<EncoderBuffer, encoder>();
        }
        if (e == nil) {
            e = Ꮡ(new encoder(nil));
        }
        if (enc.BufferPool != default!) {
            defer(Ꮡenc.Value.BufferPool.Put, e.Reinterpret<encoder, EncoderBuffer>(), ref ᒐ);
        }
        e.Value.enc = Ꮡenc;
        e.Value.w = w;
        e.Value.m = m;
        color.Palette pal = default!;
        // cbP8 encoding needs PalettedImage's ColorIndexAt method.
        {
            var (_, ok) = m._<image.PalettedImage>(ᐧ); if (ok) {
                (pal, _) = m.ColorModel()._<color.Palette>(ᐧ);
            }
        }
        if (pal != default!){
            if (len(pal) <= 2){
                e.Value.cb = cbP1;
            } else 
            if (len(pal) <= 4){
                e.Value.cb = cbP2;
            } else 
            if (len(pal) <= 16){
                e.Value.cb = cbP4;
            } else {
                e.Value.cb = cbP8;
            }
        } else {
            var exprᴛ1 = m.ColorModel();
            if (AreEqual(exprᴛ1, color.GrayModel)) {
                e.Value.cb = cbG8;
            }
            else if (AreEqual(exprᴛ1, color.Gray16Model)) {
                e.Value.cb = cbG16;
            }
            else if (AreEqual(exprᴛ1, color.RGBAModel) || AreEqual(exprᴛ1, color.NRGBAModel) || AreEqual(exprᴛ1, color.AlphaModel)) {
                if (opaque(m)){
                    e.Value.cb = cbTC8;
                } else {
                    e.Value.cb = cbTCA8;
                }
            }
            else { /* default: */
                if (opaque(m)){
                    e.Value.cb = cbTC16;
                } else {
                    e.Value.cb = cbTCA16;
                }
            }

        }
        (_, e.Value.err) = io.WriteString(w, pngHeader);
        e.writeIHDR();
        if (pal != default!) {
            e.writePLTEAndTRNS(pal);
        }
        e.writeIDATs();
        e.writeIEND();
        return (~e).err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end png_package
