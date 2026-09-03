// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bufio = bufio_package;
using bytes = bytes_package;
using lzw = compress.lzw_package;
using errors = errors_package;
using image = image_package;
using Δcolor = go.image.color_package;
using palette = go.image.color.palette_package;
using draw = go.image.draw_package;
using byteorder = go.@internal.byteorder_package;
using io = io_package;
using compress;
using go.@internal;
using go.image;
using go.image.color;

partial class gif_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸbytes() {
    builtin.initPackage(typeof(bytes_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸimageꓸcolorꓸpalette() {
    builtin.initPackage(typeof(go.image.color.palette_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸimageꓸdraw() {
    builtin.initPackage(typeof(go.image.draw_package));
}

// Graphic control extension fields.
internal static UntypedInt gcLabel => 0xF9;

internal static UntypedInt gcBlockSize => 0x04;

internal static array<nint> log2Lookup = new nint[]{2, 4, 8, 16, 32, 64, 128, 256}.array();

internal static nint log2(nint x) {
    foreach (var (i, v) in log2Lookup.ΔRangeSnapshot()) {
        if (x <= v) {
            return i;
        }
    }
    return -1;
}

// writer is a buffered writer.
[GoType] partial interface writer :
    io.Writer,
    io.ByteWriter
{
    error Flush();
}

// encoder encodes an image to the GIF format.
[GoType] partial struct encoder {
    // w is the writer to write to. err is the first error encountered during
    // writing. All attempted writes after the first error become no-ops.
    internal writer w;
    internal error err;
    // g is a reference to the data that is being encoded.
    internal GIF g;
    // globalCT is the size in bytes of the global color table.
    internal nint globalCT;
    // buf is a scratch buffer. It must be at least 256 for the blockWriter.
    internal array<byte> buf = new(256);
    internal array<byte> globalColorTable = new(3 * 256);
    internal array<byte> localColorTable = new(3 * 256);
}

// blockWriter writes the block structure of GIF image data, which
// comprises (n, (n bytes)) blocks, with 1 <= n <= 255. It is the
// writer given to the LZW encoder, which is thus immune to the
// blocking.
[GoType] partial struct blockWriter {
    internal ж<encoder> e;
}

internal static void setup(this blockWriter b) {
    b.e.Value.buf[0] = 0;
}

internal static error Flush(this blockWriter b) {
    return (~b.e).err;
}

internal static error WriteByte(this blockWriter b, byte c) {
    if ((~b.e).err != default!) {
        return (~b.e).err;
    }
    // Append c to buffered sub-block.
    b.e.Value.buf[0]++;
    b.e.Value.buf[(~b.e).buf[0]] = c;
    if ((~b.e).buf[0] < 255) {
        return default!;
    }
    // Flush block
    b.e.write((~b.e).buf[..256]);
    b.e.Value.buf[0] = 0;
    return (~b.e).err;
}

// blockWriter must be an io.Writer for lzw.NewWriter, but this is never
// actually called.
internal static (nint, error) Write(this blockWriter b, slice<byte> data) {
    foreach (var (i, c) in data) {
        {
            var err = b.WriteByte(c); if (err != default!) {
                return (i, err);
            }
        }
    }
    return (len(data), default!);
}

internal static void close(this blockWriter b) {
    // Write the block terminator (0x00), either by itself, or along with a
    // pending sub-block.
    if ((~b.e).buf[0] == 0){
        b.e.writeByte(0);
    } else {
        nuint n = (nuint)(~b.e).buf[0];
        b.e.Value.buf[(nint)(n + 1)] = 0;
        b.e.write((~b.e).buf[..(int)(n + 2)]);
    }
    b.e.flush();
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gif89aˢ = "GIF89a"u8;
internal static readonly @string netscape20ˢ = "NETSCAPE2.0"u8;

[GoRecv] internal static void writeHeader(this ref encoder e) {
    if (e.err != default!) {
        return;
    }
    (_, e.err) = io.WriteString(e.w, gif89aˢ);
    if (e.err != default!) {
        return;
    }
    // Logical screen width and height.
    byteorder.LePutUint16(e.buf[0..2], (uint16)e.g.Config.Width);
    byteorder.LePutUint16(e.buf[2..4], (uint16)e.g.Config.Height);
    e.write(e.buf[..4]);
    {
        var (p, ok) = e.g.Config.ColorModel._<Δcolor.Palette>(ᐧ); if (ok && len(p) > 0){
            nint paddedSize = log2(len(p)); // Size of Global Color Table: 2^(1+n).
            e.buf[0] = (byte)((uint8)((uint8)fColorTable | (uint8)paddedSize));
            e.buf[1] = e.g.BackgroundIndex;
            e.buf[2] = 0x00; // Pixel Aspect Ratio.
            e.write(e.buf[..3]);
            error err = default!;
            (e.globalCT, err) = encodeColorTable(e.globalColorTable[..], p, paddedSize);
            if (err != default! && e.err == default!) {
                e.err = err;
                return;
            }
            e.write(e.globalColorTable[..(int)(e.globalCT)]);
        } else {
            // All frames have a local color table, so a global color table
            // is not needed.
            e.buf[0] = 0x00;
            e.buf[1] = 0x00; // Background Color Index.
            e.buf[2] = 0x00; // Pixel Aspect Ratio.
            e.write(e.buf[..3]);
        }
    }
    // Add animation info if necessary.
    if (len(e.g.Image) > 1 && e.g.LoopCount >= 0) {
        e.buf[0] = 0x21; // Extension Introducer.
        e.buf[1] = 0xff; // Application Label.
        e.buf[2] = 0x0b; // Block Size.
        e.write(e.buf[..3]);
        var (_, err) = io.WriteString(e.w, netscape20ˢ); // Application Identifier.
        if (err != default! && e.err == default!) {
            e.err = err;
            return;
        }
        e.buf[0] = 0x03; // Block Size.
        e.buf[1] = 0x01; // Sub-block Index.
        byteorder.LePutUint16(e.buf[2..4], (uint16)e.g.LoopCount);
        e.buf[4] = 0x00; // Block Terminator.
        e.write(e.buf[..5]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifCannotEncodeColorˢ = "gif: cannot encode color table with more than 256 entries"u8;
internal static readonly @string gifCannotEncodeColorˢ2 = "gif: cannot encode color table with nil entries"u8;

internal static (nint, error) encodeColorTable(slice<byte> dst, Δcolor.Palette p, nint size) {
    if ((nuint)size >= (nuint)len(log2Lookup)) {
        return (0, errors.New(gifCannotEncodeColorˢ));
    }
    foreach (var (i, c) in p) {
        if (c == default!) {
            return (0, errors.New(gifCannotEncodeColorˢ2));
        }
        uint8 r = default!;
        uint8 g = default!;
        uint8 b = default!;
        // It is most likely that the palette is full of color.RGBAs, so they
        // get a fast path.
        {
            var (rgba, ok) = c._<colorꓸRGBA>(ᐧ); if (ok){
                (r, g, b) = (rgba.R, rgba.G, rgba.B);
            } else {
                var (rr, gg, bb, _) = c.RGBA();
                (r, g, b) = ((uint8)((rr >> (int)(8))), (uint8)((gg >> (int)(8))), (uint8)((bb >> (int)(8))));
            }
        }
        dst[3 * i + 0] = r;
        dst[3 * i + 1] = g;
        dst[3 * i + 2] = b;
    }
    nint n = log2Lookup[size];
    if (n > len(p)) {
        // Pad with black.
        clear(dst[(int)(3 * len(p))..(int)(3 * n)]);
    }
    return (3 * n, default!);
}

[GoRecv] internal static bool colorTablesMatch(this ref encoder e, nint localLen, nint transparentIndex) {
    nint localSize = 3 * localLen;
    if (transparentIndex >= 0) {
        nint trOff = 3 * transparentIndex;
        return bytes.Equal(e.globalColorTable[..(int)(trOff)], e.localColorTable[..(int)(trOff)]) && bytes.Equal(e.globalColorTable[(int)(trOff + 3)..(int)(localSize)], e.localColorTable[(int)(trOff + 3)..(int)(localSize)]);
    }
    return bytes.Equal(e.globalColorTable[..(int)(localSize)], e.localColorTable[..(int)(localSize)]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifCannotEncodeImageˢ = "gif: cannot encode image block with empty palette"u8;
internal static readonly @string gifImageBlockIsTooLargeˢ = "gif: image block is too large to encode"u8;
internal static readonly @string gifImageBlockIsOutOfˢ = "gif: image block is out of bounds"u8;

internal static void writeImageBlock(this ж<encoder> Ꮡe, ж<image.Paletted> Ꮡpm, nint delay, byte disposal) {
    ref var e = ref Ꮡe.DerefOrNull();
    ref var pm = ref Ꮡpm.DerefOrNull();

    if (e.err != default!) {
        return;
    }
    if (len(pm.Palette) == 0) {
        e.err = errors.New(gifCannotEncodeImageˢ);
        return;
    }
    var b = pm.Bounds();
    if (b.Min.X < 0 || b.Max.X >= (1 << (int)(16)) || b.Min.Y < 0 || b.Max.Y >= (1 << (int)(16))) {
        e.err = errors.New(gifImageBlockIsTooLargeˢ);
        return;
    }
    if (!b.In(new image.Rectangle(Max: new image.Point(e.g.Config.Width, e.g.Config.Height)))) {
        e.err = errors.New(gifImageBlockIsOutOfˢ);
        return;
    }
    nint transparentIndex = -1;
    foreach (var (i, c) in pm.Palette) {
        if (c == default!) {
            e.err = errors.New(gifCannotEncodeColorˢ2);
            return;
        }
        {
            var (_, _, _, a) = c.RGBA(); if (a == 0) {
                transparentIndex = i;
                break;
            }
        }
    }
    if (delay > 0 || disposal != 0 || transparentIndex != -1) {
        e.buf[0] = sExtension; // Extension Introducer.
        e.buf[1] = gcLabel; // Graphic Control Label.
        e.buf[2] = gcBlockSize; // Block Size.
        if (transparentIndex != -1){
            e.buf[3] = (byte)(0x01 | (byte)(disposal << (int)(2)));
        } else {
            e.buf[3] = (byte)(0x00 | (byte)(disposal << (int)(2)));
        }
        byteorder.LePutUint16(e.buf[4..6], (uint16)delay); // Delay Time (1/100ths of a second)
        // Transparent color index.
        if (transparentIndex != -1){
            e.buf[6] = (uint8)transparentIndex;
        } else {
            e.buf[6] = 0x00;
        }
        e.buf[7] = 0x00; // Block Terminator.
        e.write(e.buf[..8]);
    }
    e.buf[0] = sImageDescriptor;
    byteorder.LePutUint16(e.buf[1..3], (uint16)b.Min.X);
    byteorder.LePutUint16(e.buf[3..5], (uint16)b.Min.Y);
    byteorder.LePutUint16(e.buf[5..7], (uint16)b.Dx());
    byteorder.LePutUint16(e.buf[7..9], (uint16)b.Dy());
    e.write(e.buf[..9]);
    // To determine whether or not this frame's palette is the same as the
    // global palette, we can check a couple things. First, do they actually
    // point to the same []color.Color? If so, they are equal so long as the
    // frame's palette is not longer than the global palette...
    nint paddedSize = log2(len(pm.Palette)); // Size of Local Color Table: 2^(1+n).
    {
        var (gp, ok) = e.g.Config.ColorModel._<Δcolor.Palette>(ᐧ); if (ok && len(pm.Palette) <= len(gp) && Ꮡ(gp, 0) == Ꮡ(pm.Palette, 0)){
            e.writeByte(0); // Use the global color table.
        } else {
            var (ct, err) = encodeColorTable(e.localColorTable[..], pm.Palette, paddedSize);
            if (err != default!) {
                if (e.err == default!) {
                    e.err = err;
                }
                return;
            }
            // This frame's palette is not the very same slice as the global
            // palette, but it might be a copy, possibly with one value turned into
            // transparency by DecodeAll.
            if (ct <= e.globalCT && e.colorTablesMatch(len(pm.Palette), transparentIndex)){
                e.writeByte(0); // Use the global color table.
            } else {
                // Use a local color table.
                e.writeByte((byte)((uint8)((uint8)fColorTable | (uint8)paddedSize)));
                e.write(e.localColorTable[..(int)(ct)]);
            }
        }
    }
    nint litWidth = paddedSize + 1;
    if (litWidth < 2) {
        litWidth = 2;
    }
    e.writeByte((uint8)litWidth); // LZW Minimum Code Size.
    var bw = new blockWriter(e: Ꮡe);
    bw.setup();
    var lzww = lzw.NewWriter(bw, lzw.LSB, litWidth);
    {
        nint dx = b.Dx(); if (dx == pm.Stride){
            (_, e.err) = lzww.Write(pm.Pix[..(int)(dx * b.Dy())]);
            if (e.err != default!) {
                lzww.Close();
                return;
            }
        } else {
            for ((nint i, nint y) = (0, b.Min.Y); y < b.Max.Y; (i, y) = (i + pm.Stride, y + 1)) {
                (_, e.err) = lzww.Write(pm.Pix[(int)(i)..(int)(i + dx)]);
                if (e.err != default!) {
                    lzww.Close();
                    return;
                }
            }
        }
    }
    lzww.Close(); // flush to bw
    bw.close(); // flush to e.w
}

// Options are the encoding parameters.
[GoType] partial struct Options {
    // NumColors is the maximum number of colors used in the image.
    // It ranges from 1 to 256.
    public nint NumColors;
    // Quantizer is used to produce a palette with size NumColors.
    // palette.Plan9 is used in place of a nil Quantizer.
    public draw.Quantizer Quantizer;
    // Drawer is used to convert the source image to the desired palette.
    // draw.FloydSteinberg is used in place of a nil Drawer.
    public draw.Drawer Drawer;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifMustProvideAtLeastOneˢ = "gif: must provide at least one image"u8;
internal static readonly @string gifMismatchedImageAndˢ = "gif: mismatched image and delay lengths"u8;
internal static readonly @string gifMismatchedImageAndˢ2 = "gif: mismatched image and disposal lengths"u8;
internal static readonly @string gifGifColorModelMustBeAˢ = "gif: GIF color model must be a color.Palette"u8;

// EncodeAll writes the images in g to w in GIF format with the
// given loop count and delay between frames.
public static error EncodeAll(io.Writer w, ж<GIF> Ꮡg) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(g.Image) == 0) {
        return errors.New(gifMustProvideAtLeastOneˢ);
    }
    if (len(g.Image) != len(g.Delay)) {
        return errors.New(gifMismatchedImageAndˢ);
    }
    ref var e = ref heap<encoder>(out var Ꮡe);
    e = new encoder(g: g);
    // The GIF.Disposal, GIF.Config and GIF.BackgroundIndex fields were added
    // in Go 1.5. Valid Go 1.4 code, such as when the Disposal field is omitted
    // in a GIF struct literal, should still produce valid GIFs.
    if (e.g.Disposal != default! && len(e.g.Image) != len(e.g.Disposal)) {
        return errors.New(gifMismatchedImageAndˢ2);
    }
    if (e.g.Config == (new image.Config(nil))){
        var p = g.Image[0].Bounds().Max;
        e.g.Config.Width = p.X;
        e.g.Config.Height = p.Y;
    } else 
    if (e.g.Config.ColorModel != default!) {
        {
            var (_, ok) = e.g.Config.ColorModel._<Δcolor.Palette>(ᐧ); if (!ok) {
                return errors.New(gifGifColorModelMustBeAˢ);
            }
        }
    }
    {
        var (ww, ok) = w._<writer>(ᐧ); if (ok){
            e.w = ww;
        } else {
            e.w = new bufio_Writerжwriter(bufio.NewWriter(w));
        }
    }
    e.writeHeader();
    foreach (var (i, pm) in g.Image) {
        var disposal = (uint8)0;
        if (g.Disposal != default!) {
            disposal = g.Disposal[i];
        }
        Ꮡe.writeImageBlock(pm, g.Delay[i], disposal);
    }
    e.writeByte(sTrailer);
    e.flush();
    return e.err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gifImageIsTooLargeToˢ = "gif: image is too large to encode"u8;

// Encode writes the Image m to w in GIF format.
public static error Encode(io.Writer w, image.Image m, ж<Options> Ꮡo) {
    ref var o = ref Ꮡo.DerefOrNull();

    // Check for bounds and size restrictions.
    var b = m.Bounds();
    if (b.Dx() >= (1 << (int)(16)) || b.Dy() >= (1 << (int)(16))) {
        return errors.New(gifImageIsTooLargeToˢ);
    }
    var opts = new Options(nil);
    if (Ꮡo != nil) {
        opts = o;
    }
    if (opts.NumColors < 1 || 256 < opts.NumColors) {
        opts.NumColors = 256;
    }
    if (opts.Drawer == default!) {
        opts.Drawer = draw.FloydSteinberg;
    }
    var (pm, _) = m._<ж<image.Paletted>>(ᐧ);
    if (pm == nil) {
        {
            var (cp, ok) = m.ColorModel()._<Δcolor.Palette>(ᐧ); if (ok) {
                pm = image.NewPaletted(b, cp);
                for (nint y = b.Min.Y; y < b.Max.Y; y++) {
                    for (nint x = b.Min.X; x < b.Max.X; x++) {
                        pm.Set(x, y, cp.Convert(m.At(x, y)));
                    }
                }
            }
        }
    }
    if (pm == nil || len((~pm).Palette) > opts.NumColors) {
        // Set pm to be a palettedized copy of m, including its bounds, which
        // might not start at (0, 0).
        //
        // TODO: Pick a better sub-sample of the Plan 9 palette.
        pm = image.NewPaletted(b, palette.Plan9[..(int)(opts.NumColors)]);
        if (opts.Quantizer != default!) {
            pm.Value.Palette = opts.Quantizer.Quantize(new Δcolor.Palette(0, opts.NumColors), m);
        }
        opts.Drawer.Draw(new image_PalettedжImage(pm), b, m, b.Min);
    }
    // When calling Encode instead of EncodeAll, the single-frame image is
    // translated such that its top-left corner is (0, 0), so that the single
    // frame completely fills the overall GIF's bounds.
    if ((~pm).Rect.Min != (new image.Point(nil))) {
        ref var dup = ref heap<image.Paletted>(out var Ꮡdup);
        dup = pm.Value;
        dup.Rect = dup.Rect.Sub(dup.Rect.Min);
        pm = Ꮡdup;
    }
    return EncodeAll(w, Ꮡ(new GIF(
        Image: new ж<image.Paletted>[]{pm}.slice(),
        Delay: new nint[]{0}.slice(),
        Config: new image.Config(
            ColorModel: (~pm).Palette,
            Width: b.Dx(),
            Height: b.Dy()
        )
    )));
}

} // end gif_package
