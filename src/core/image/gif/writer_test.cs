// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using image = image_package;
using Δcolor = go.image.color_package;
using palette = go.image.color.palette_package;
using draw = go.image.draw_package;
// blank import: go.image.png_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using io = io_package;
using rand = math.rand_package;
using os = os_package;
using reflect = reflect_package;
using testing = testing_package;
using go.image;
using go.image.color;
using math;
using static go.image.gif_package;

partial class gif_internal_test_package {

internal static (image.Image, error) readImg(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var m, _, err) = image.Decode(new gif_internal_test_package.os_FileжReader(f));
        return (m, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (ж<global::go.image.gif_package.GIF>, error) readGIF(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return DecodeAll(new gif_internal_test_package.os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static int64 delta(uint32 u0, uint32 u1) {
    var d = (int64)u0 - (int64)u1;
    if (d < 0) {
        return -d;
    }
    return d;
}

// averageDelta returns the average delta in RGB space. The two images must
// have the same bounds.
internal static int64 averageDelta(image.Image m0, image.Image m1) {
    var b = m0.Bounds();
    return averageDeltaBound(m0, m1, b, b);
}

// averageDeltaBound returns the average delta in RGB space. The average delta is
// calculated in the specified bounds.
internal static int64 averageDeltaBound(image.Image m0, image.Image m1, image.Rectangle b0, image.Rectangle b1) {
    int64 sum = default!;
    int64 n = default!;
    for (nint y = b0.Min.Y; y < b0.Max.Y; y++) {
        for (nint x = b0.Min.X; x < b0.Max.X; x++) {
            var c0 = m0.At(x, y);
            var c1 = m1.At(x - b0.Min.X + b1.Min.X, y - b0.Min.Y + b1.Min.Y);
            var (r0, g0, b0Δ1, _) = c0.RGBA();
            var (r1, g1, b1Δ1, _) = c1.RGBA();
            sum += delta(r0, r1);
            sum += delta(g0, g1);
            sum += delta(b0Δ1, b1Δ1);
            n += 3;
        }
    }
    return quo(sum, n);
}

// lzw.NewWriter wants an interface which is basically the same thing as gif's
// writer interface.  This ensures we're compatible.
internal static global::go.image.gif_package.writer _ᴛ2ʗ = new gif_internal_test_package.gif_blockWriterᴠwriter(new blockWriter(nil));


[GoType("dyn")] partial struct testCaseᴛ1 {
    internal @string filename;
    internal int64 tolerance;
}
internal static slice<testCaseᴛ1> testCase = new testCaseᴛ1[]{
    new("../testdata/video-001.png"u8, ((int64)1 << (int)(12))),
    new("../testdata/video-001.gif"u8, 0),
    new("../testdata/video-001.interlaced.gif"u8, 0)
}.slice();

public static void TestWriter(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in testCase) {
        var (m0, err) = readImg(tc.filename);
        if (err != default!) {
            Ꮡt.Error(tc.filename, err);
            continue;
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = Encode(new gif_internal_test_package.bytes_BufferжWriter(Ꮡbuf), m0, nil);
        if (err != default!) {
            Ꮡt.Error(tc.filename, err);
            continue;
        }
        (var m1, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(Ꮡbuf));
        if (err != default!) {
            Ꮡt.Error(tc.filename, err);
            continue;
        }
        if (m0.Bounds() != m1.Bounds()) {
            Ꮡt.Errorf("%s, bounds differ: %v and %v"u8, tc.filename, m0.Bounds(), m1.Bounds());
            continue;
        }
        // Compare the average delta to the tolerance level.
        var avgDelta = averageDelta(m0, m1);
        if (avgDelta > tc.tolerance) {
            Ꮡt.Errorf("%s: average delta is too high. expected: %d, got %d"u8, tc.filename, tc.tolerance, avgDelta);
            continue;
        }
    }
}

public static void TestSubImage(ж<testing.T> Ꮡt) {
    var (m0, err) = readImg(testdataVideo001Gifˢ);
    if (err != default!) {
        Ꮡt.Fatalf("readImg: %v"u8, err);
    }
    m0 = m0._<ж<image.Paletted>>().SubImage(image.Rect(0, 0, 50, 30));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = Encode(new gif_internal_test_package.bytes_BufferжWriter(Ꮡbuf), m0, nil);
    if (err != default!) {
        Ꮡt.Fatalf("Encode: %v"u8, err);
    }
    (var m1, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(Ꮡbuf));
    if (err != default!) {
        Ꮡt.Fatalf("Decode: %v"u8, err);
    }
    if (m0.Bounds() != m1.Bounds()) {
        Ꮡt.Fatalf("bounds differ: %v and %v"u8, m0.Bounds(), m1.Bounds());
    }
    if (averageDelta(m0, m1) != 0) {
        Ꮡt.Fatalf("images differ"u8);
    }
}

// palettesEqual reports whether two color.Palette values are equal, ignoring
// any trailing opaque-black palette entries.
internal static bool palettesEqual(Δcolor.Palette p, Δcolor.Palette q) {
    nint n = len(p);
    if (n > len(q)) {
        n = len(q);
    }
    for (nint i = 0; i < n; i++) {
        if (!AreEqual(p[i], q[i])) {
            return false;
        }
    }
    for (nint i = n; i < len(p); i++) {
        var (r, g, b, a) = p[i].RGBA();
        if (r != 0 || g != 0 || b != 0 || a != 0xffff) {
            return false;
        }
    }
    for (nint i = n; i < len(q); i++) {
        var (r, g, b, a) = q[i].RGBA();
        if (r != 0 || g != 0 || b != 0 || a != 0xffff) {
            return false;
        }
    }
    return true;
}

internal static slice<@string> frames = new @string[]{
    "../testdata/video-001.gif"u8,
    "../testdata/video-005.gray.gif"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeConfigˢ = (@string)"DecodeConfig:"u8;

internal static void testEncodeAll(ж<testing.T> Ꮡt, bool go1Dot5Fields, bool useGlobalColorModel) {
    const nint width = 150;
    const nint height = 103;
    var g0 = Ꮡ(new GIF(
        Image: new slice<ж<image.Paletted>>(len(frames)),
        Delay: new slice<nint>(len(frames)),
        LoopCount: 5
    ));
    foreach (var (i, f) in frames) {
        var (g, errΔ1) = readGIF(f);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(f, errΔ1);
        }
        var m = (~g).Image[0];
        if (m.Bounds().Dx() != width || m.Bounds().Dy() != height) {
            Ꮡt.Fatalf("frame %d had unexpected bounds: got %v, want width/height = %d/%d"u8,
                i, m.Bounds(), (nint)(width), (nint)(height));
        }
        g0.Value.Image[i] = m;
    }
    // The GIF.Disposal, GIF.Config and GIF.BackgroundIndex fields were added
    // in Go 1.5. Valid Go 1.4 or earlier code should still produce valid GIFs.
    //
    // On the following line, color.Model is an interface type, and
    // color.Palette is a concrete (slice) type.
    var (globalColorModel, backgroundIndex) = (((Δcolor.Model)((Δcolor.Palette)default!)), (uint8)0);
    if (useGlobalColorModel) {
        (globalColorModel, backgroundIndex) = (((Δcolor.Palette)palette.WebSafe), (uint8)1);
    }
    if (go1Dot5Fields) {
        g0.Value.Disposal = new slice<byte>(len((~g0).Image));
        foreach (var (i, _) in (~g0).Disposal) {
            g0.Value.Disposal[i] = DisposalNone;
        }
        g0.Value.Config = new image.Config(
            ColorModel: globalColorModel,
            Width: width,
            Height: height
        );
        g0.Value.BackgroundIndex = backgroundIndex;
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ2 = EncodeAll(new gif_internal_test_package.bytes_BufferжWriter(Ꮡbuf), g0); if (errΔ2 != default!) {
            Ꮡt.Fatal(encodeAllˢ, errΔ2);
        }
    }
    var encoded = buf.Bytes();
    var (config, err) = DecodeConfig(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(encoded)));
    if (err != default!) {
        Ꮡt.Fatal(decodeConfigˢ, err);
    }
    (var g1, err) = DecodeAll(new gif_internal_test_package.bytes_ReaderжReader(bytes.NewReader(encoded)));
    if (err != default!) {
        Ꮡt.Fatal(decodeAllˢ, err);
    }
    if (!reflect.DeepEqual(config, (~g1).Config)) {
        Ꮡt.Errorf("DecodeConfig inconsistent with DecodeAll"u8);
    }
    if (!palettesEqual((~g1).Config.ColorModel._<Δcolor.Palette>(), globalColorModel._<Δcolor.Palette>())) {
        Ꮡt.Errorf("unexpected global color model"u8);
    }
    {
        nint w = g1.Value.Config.Width;
        nint h = g1.Value.Config.Height; if (w != width || h != height) {
            Ꮡt.Errorf("got config width * height = %d * %d, want %d * %d"u8, w, h, (nint)(width), (nint)(height));
        }
    }
    if ((~g0).LoopCount != (~g1).LoopCount) {
        Ꮡt.Errorf("loop counts differ: %d and %d"u8, (~g0).LoopCount, (~g1).LoopCount);
    }
    if (backgroundIndex != (~g1).BackgroundIndex) {
        Ꮡt.Errorf("background indexes differ: %d and %d"u8, backgroundIndex, (~g1).BackgroundIndex);
    }
    if (len((~g0).Image) != len((~g1).Image)) {
        Ꮡt.Fatalf("image lengths differ: %d and %d"u8, len((~g0).Image), len((~g1).Image));
    }
    if (len((~g1).Image) != len((~g1).Delay)) {
        Ꮡt.Fatalf("image and delay lengths differ: %d and %d"u8, len((~g1).Image), len((~g1).Delay));
    }
    if (len((~g1).Image) != len((~g1).Disposal)) {
        Ꮡt.Fatalf("image and disposal lengths differ: %d and %d"u8, len((~g1).Image), len((~g1).Disposal));
    }
    foreach (var (i, _) in (~g0).Image) {
        var (m0, m1) = ((~g0).Image[i], (~g1).Image[i]);
        if (m0.Bounds() != m1.Bounds()) {
            Ꮡt.Errorf("frame %d: bounds differ: %v and %v"u8, i, m0.Bounds(), m1.Bounds());
        }
        nint d0 = (~g0).Delay[i];
        nint d1 = (~g1).Delay[i];
        if (d0 != d1) {
            Ꮡt.Errorf("frame %d: delay values differ: %d and %d"u8, i, d0, d1);
        }
        var (p0, p1) = ((uint8)0, (~g1).Disposal[i]);
        if (go1Dot5Fields) {
            p0 = DisposalNone;
        }
        if (p0 != p1) {
            Ꮡt.Errorf("frame %d: disposal values differ: %d and %d"u8, i, p0, p1);
        }
    }
}

public static void TestEncodeAllGo1Dot4(ж<testing.T> Ꮡt) {
    testEncodeAll(Ꮡt, false, false);
}

public static void TestEncodeAllGo1Dot5(ж<testing.T> Ꮡt) {
    testEncodeAll(Ꮡt, true, false);
}

public static void TestEncodeAllGo1Dot5GlobalColorModel(ж<testing.T> Ꮡt) {
    testEncodeAll(Ꮡt, true, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorFromˢ = (@string)"expected error from mismatched delay and image slice lengths"u8;
internal static readonly object expectedErrorFromˢ2 = (@string)"expected error from mismatched disposal and image slice lengths"u8;

public static void TestEncodeMismatchDelay(ж<testing.T> Ꮡt) {
    var images = new slice<ж<image.Paletted>>(2);
    foreach (var (i, _) in images) {
        images[i] = image.NewPaletted(image.Rect(0, 0, 5, 5), palette.Plan9);
    }
    var g0 = Ꮡ(new GIF(
        Image: images,
        Delay: new slice<nint>(1)
    ));
    {
        var err = EncodeAll(io.Discard, g0); if (err == default!) {
            Ꮡt.Error(expectedErrorFromˢ);
        }
    }
    var g1 = Ꮡ(new GIF(
        Image: images,
        Delay: new slice<nint>(len(images)),
        Disposal: new slice<byte>(1)
    ));
    foreach (var (i, _) in (~g1).Disposal) {
        g1.Value.Disposal[i] = DisposalNone;
    }
    {
        var err = EncodeAll(io.Discard, g1); if (err == default!) {
            Ꮡt.Error(expectedErrorFromˢ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorFromˢ3 = (@string)"expected error from providing empty gif"u8;

public static void TestEncodeZeroGIF(ж<testing.T> Ꮡt) {
    {
        var err = EncodeAll(io.Discard, Ꮡ(new GIF(nil))); if (err == default!) {
            Ꮡt.Error(expectedErrorFromˢ3);
        }
    }
}

public static void TestEncodeAllFramesOutOfBounds(ж<testing.T> Ꮡt) {
    var images = new ж<image.Paletted>[]{
        image.NewPaletted(image.Rect(0, 0, 5, 5), palette.Plan9),
        image.NewPaletted(image.Rect(2, 2, 8, 8), palette.Plan9),
        image.NewPaletted(image.Rect(3, 3, 4, 4), palette.Plan9)
    }.slice();
    foreach (var (_, upperBound) in new nint[]{6, 10}.slice()) {
        var g = Ꮡ(new GIF(
            Image: images,
            Delay: new slice<nint>(len(images)),
            Disposal: new slice<byte>(len(images)),
            Config: new image.Config(
                Width: upperBound,
                Height: upperBound
            )
        ));
        var err = EncodeAll(io.Discard, g);
        if (upperBound >= 8){
            if (err != default!) {
                Ꮡt.Errorf("upperBound=%d: %v"u8, upperBound, err);
            }
        } else {
            if (err == default!) {
                Ꮡt.Errorf("upperBound=%d: got nil error, want non-nil"u8, upperBound);
            }
        }
    }
}

public static void TestEncodeNonZeroMinPoint(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var points = new image.Point[]{
        new(-8, -9),
        new(-4, -4),
        new(-3, +3),
        new(+0, +0),
        new(+2, +2)
    }.slice();
    foreach (var (_, p) in points) {
        var src = image.NewPaletted(new image.Rectangle(
            Min: p,
            Max: p.Add(new image.Point(6, 6))
        ), palette.Plan9);
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        {
            var errΔ1 = Encode(new gif_internal_test_package.bytes_BufferжWriter(Ꮡbuf), new image.PalettedжImage(src), nil); if (errΔ1 != default!) {
                Ꮡt.Errorf("p=%v: Encode: %v"u8, p, errΔ1);
                continue;
            }
        }
        var (m, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(Ꮡbuf));
        if (err != default!) {
            Ꮡt.Errorf("p=%v: Decode: %v"u8, p, err);
            continue;
        }
        {
            var (got, want) = (m.Bounds(), image.Rect(0, 0, 6, 6)); if (got != want) {
                Ꮡt.Errorf("p=%v: got %v, want %v"u8, p, got, want);
            }
        }
    }
    // Also test having a source image (gray on the diagonal) that has a
    // non-zero Bounds().Min, but isn't an image.Paletted.
    {
        var p = new image.Point(+2, +2);
        var src = image.NewRGBA(new image.Rectangle(
            Min: p,
            Max: p.Add(new image.Point(6, 6))
        ));
        src.SetRGBA(2, 2, new colorꓸRGBA(0x22, 0x22, 0x22, 0xFF));
        src.SetRGBA(3, 3, new colorꓸRGBA(0x33, 0x33, 0x33, 0xFF));
        src.SetRGBA(4, 4, new colorꓸRGBA(0x44, 0x44, 0x44, 0xFF));
        src.SetRGBA(5, 5, new colorꓸRGBA(0x55, 0x55, 0x55, 0xFF));
        src.SetRGBA(6, 6, new colorꓸRGBA(0x66, 0x66, 0x66, 0xFF));
        src.SetRGBA(7, 7, new colorꓸRGBA(0x77, 0x77, 0x77, 0xFF));
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        {
            var errΔ1 = Encode(new gif_internal_test_package.bytes_BufferжWriter(Ꮡbuf), new image.ΔRGBAжImage(src), nil); if (errΔ1 != default!) {
                Ꮡt.Errorf("gray-diagonal: Encode: %v"u8, errΔ1);
                return;
            }
        }
        var (m, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(Ꮡbuf));
        if (err != default!) {
            Ꮡt.Errorf("gray-diagonal: Decode: %v"u8, err);
            return;
        }
        {
            var (got, want) = (m.Bounds(), image.Rect(0, 0, 6, 6)); if (got != want) {
                Ꮡt.Errorf("gray-diagonal: got %v, want %v"u8, got, want);
                return;
            }
        }
        var mʗ1 = m;
        uint32 rednessAt(nint x, nint y) {
            var (r, _, _, _) = mʗ1.At(x, y).RGBA();
            // Shift by 8 to convert from 16 bit color to 8 bit color.
            return (r >> (int)(8));
        }
        // Round-tripping a still (non-animated) image.Image through
        // Encode+Decode should shift the origin to (0, 0).
        {
            var (got, want) = (rednessAt(0, 0), (uint32)0x22); if (got != want) {
                Ꮡt.Errorf("gray-diagonal: rednessAt(0, 0): got 0x%02x, want 0x%02x"u8, got, want);
            }
        }
        {
            var (got, want) = (rednessAt(5, 5), (uint32)0x77); if (got != want) {
                Ꮡt.Errorf("gray-diagonal: rednessAt(5, 5): got 0x%02x, want 0x%02x"u8, got, want);
            }
        }
    }
}

public static void TestEncodeImplicitConfigSize(ж<testing.T> Ꮡt) {
    // For backwards compatibility for Go 1.4 and earlier code, the Config
    // field is optional, and if zero, the width and height is implied by the
    // first (and in this case only) frame's width and height.
    //
    // A Config only specifies a width and height (two integers) while an
    // image.Image's Bounds method returns an image.Rectangle (four integers).
    // For a gif.GIF, the overall bounds' top-left point is always implicitly
    // (0, 0), and any frame whose bounds have a negative X or Y will be
    // outside those overall bounds, so encoding should fail.
    foreach (var (_, lowerBound) in new nint[]{-1, 0, 1}.slice()) {
        var images = new ж<image.Paletted>[]{
            image.NewPaletted(image.Rect(lowerBound, lowerBound, 4, 4), palette.Plan9)
        }.slice();
        var g = Ꮡ(new GIF(
            Image: images,
            Delay: new slice<nint>(len(images))
        ));
        var err = EncodeAll(io.Discard, g);
        if (lowerBound >= 0){
            if (err != default!) {
                Ꮡt.Errorf("lowerBound=%d: %v"u8, lowerBound, err);
            }
        } else {
            if (err == default!) {
                Ꮡt.Errorf("lowerBound=%d: got nil error, want non-nil"u8, lowerBound);
            }
        }
    }
}

public static void TestEncodePalettes(ж<testing.T> Ꮡt) {
    const nint w = 5;
    const nint h = 5;
    var pals = new Δcolor.Palette[]{new Δcolor.Color[]{
        new colorꓸRGBA(0x00, 0x00, 0x00, 0xff),
        new colorꓸRGBA(0x01, 0x00, 0x00, 0xff),
        new colorꓸRGBA(0x02, 0x00, 0x00, 0xff)}.slice(), new Δcolor.Color[]{
        new colorꓸRGBA(0x00, 0x00, 0x00, 0xff),
        new colorꓸRGBA(0x00, 0x01, 0x00, 0xff)}.slice(), new Δcolor.Color[]{
        new colorꓸRGBA(0x00, 0x00, 0x03, 0xff),
        new colorꓸRGBA(0x00, 0x00, 0x02, 0xff),
        new colorꓸRGBA(0x00, 0x00, 0x01, 0xff),
        new colorꓸRGBA(0x00, 0x00, 0x00, 0xff)}.slice(), new Δcolor.Color[]{
        new colorꓸRGBA(0x10, 0x07, 0xf0, 0xff),
        new colorꓸRGBA(0x20, 0x07, 0xf0, 0xff),
        new colorꓸRGBA(0x30, 0x07, 0xf0, 0xff),
        new colorꓸRGBA(0x40, 0x07, 0xf0, 0xff),
        new colorꓸRGBA(0x50, 0x07, 0xf0, 0xff)}.slice()
    }.slice();
    var g0 = Ꮡ(new GIF(
        Image: new ж<image.Paletted>[]{
            image.NewPaletted(image.Rect(0, 0, w, h), pals[0]),
            image.NewPaletted(image.Rect(0, 0, w, h), pals[1]),
            image.NewPaletted(image.Rect(0, 0, w, h), pals[2]),
            image.NewPaletted(image.Rect(0, 0, w, h), pals[3])
        }.slice(),
        Delay: new slice<nint>(len(pals)),
        Disposal: new slice<byte>(len(pals)),
        Config: new image.Config(
            ColorModel: pals[2],
            Width: w,
            Height: h
        )
    ));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = EncodeAll(new gif_internal_test_package.bytes_BufferжWriter(Ꮡbuf), g0); if (errΔ1 != default!) {
            Ꮡt.Fatalf("EncodeAll: %v"u8, errΔ1);
        }
    }
    var (g1, err) = DecodeAll(new gif_internal_test_package.bytes_BufferжReader(Ꮡbuf));
    if (err != default!) {
        Ꮡt.Fatalf("DecodeAll: %v"u8, err);
    }
    if (len((~g0).Image) != len((~g1).Image)) {
        Ꮡt.Fatalf("image lengths differ: %d and %d"u8, len((~g0).Image), len((~g1).Image));
    }
    foreach (var (i, m) in (~g1).Image) {
        {
            var (got, want) = (m.Value.Palette, pals[i]); if (!palettesEqual(got, want)) {
                Ꮡt.Errorf("frame %d:\ngot  %v\nwant %v"u8, i, got, want);
            }
        }
    }
}

public static void TestEncodeBadPalettes(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    const nint w = 5;
    const nint h = 5;
    foreach (var (_, n) in new nint[]{256, 257}.slice()) {
        foreach (var (_, nilColors) in new bool[]{false, true}.slice()) {
            var pal = new Δcolor.Palette(n);
            if (!nilColors) {
                foreach (var (i, _) in pal) {
                    pal[i] = Δcolor.Black;
                }
            }
            var err = EncodeAll(io.Discard, Ꮡ(new GIF(
                Image: new ж<image.Paletted>[]{
                    image.NewPaletted(image.Rect(0, 0, w, h), pal)
                }.slice(),
                Delay: new slice<nint>(1),
                Disposal: new slice<byte>(1),
                Config: new image.Config(
                    ColorModel: pal,
                    Width: w,
                    Height: h
                )
            )));
            var got = err != default!;
            var want = n > 256 || nilColors;
            if (got != want) {
                Ꮡt.Errorf("n=%d, nilColors=%t: err != nil: got %t, want %t"u8, n, nilColors, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object encodedColorTablesAreˢ = (@string)"Encoded color tables are equal, expected mismatch"u8;
internal static readonly object colorTablesMatchFalseˢ = (@string)"colorTablesMatch() == false, expected true"u8;

public static void TestColorTablesMatch(ж<testing.T> Ꮡt) {
    const nint trIdx = 100;
    var global = ((Δcolor.Palette)palette.Plan9);
    {
        var rgb = global[trIdx]._<colorꓸRGBA>(); if (rgb.R == 0 && rgb.G == 0 && rgb.B == 0) {
            Ꮡt.Fatalf("trIdx (%d) is already black"u8, (nint)(trIdx));
        }
    }
    // Make a copy of the palette, substituting trIdx's slot with transparent,
    // just like decoder.decode.
    var local = appendꓸꓸꓸ(((Δcolor.Palette)default!), global);
    local[trIdx] = new colorꓸRGBA(nil);
    const nint testLen = /* 3 * 256 */ 768;
    const nint padded = 7;
    var e = @new<global::go.image.gif_package.encoder>();
    {
        var (l, err) = encodeColorTable((~e).globalColorTable[..], global, padded); if (err != default! || l != testLen) {
            Ꮡt.Fatalf("Failed to encode global color table: got %d, %v; want nil, %d"u8, l, err, (nint)(testLen));
        }
    }
    {
        var (l, err) = encodeColorTable((~e).localColorTable[..], local, padded); if (err != default! || l != testLen) {
            Ꮡt.Fatalf("Failed to encode local color table: got %d, %v; want nil, %d"u8, l, err, (nint)(testLen));
        }
    }
    if (bytes.Equal((~e).globalColorTable[..(int)(testLen)], (~e).localColorTable[..(int)(testLen)])) {
        Ꮡt.Fatal(encodedColorTablesAreˢ);
    }
    if (!e.colorTablesMatch(len(local), trIdx)) {
        Ꮡt.Fatal(colorTablesMatchFalseˢ);
    }
}

public static void TestEncodeCroppedSubImages(ж<testing.T> Ꮡt) {
    // This test means to ensure that Encode honors the Bounds and Strides of
    // images correctly when encoding.
    var whole = image.NewPaletted(image.Rect(0, 0, 100, 100), palette.Plan9);
    var subImages = new image.Rectangle[]{
        image.Rect(0, 0, 50, 50),
        image.Rect(50, 0, 100, 50),
        image.Rect(0, 50, 50, 50),
        image.Rect(50, 50, 100, 100),
        image.Rect(25, 25, 75, 75),
        image.Rect(0, 0, 100, 50),
        image.Rect(0, 50, 100, 100),
        image.Rect(0, 0, 50, 100),
        image.Rect(50, 0, 100, 100)
    }.slice();
    foreach (var (_, sr) in subImages) {
        var si = whole.SubImage(sr);
        var buf = bytes.NewBuffer(default!);
        {
            var err = Encode(new gif_internal_test_package.bytes_BufferжWriter(buf), si, nil); if (err != default!) {
                Ꮡt.Errorf("Encode: sr=%v: %v"u8, sr, err);
                continue;
            }
        }
        {
            var (_, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(buf)); if (err != default!) {
                Ꮡt.Errorf("Decode: sr=%v: %v"u8, sr, err);
            }
        }
    }
}

[GoType] internal partial struct offsetImage {
    [GoEmbedded] public image_package.Image Image;
    public image.Rectangle Rect;
}

internal static image.Rectangle Bounds(this offsetImage i) {
    return i.Rect;
}

public static void TestEncodeWrappedImage(ж<testing.T> Ꮡt) {
    var (m0, err) = readImg(testdataVideo001Gifˢ);
    if (err != default!) {
        Ꮡt.Fatalf("readImg: %v"u8, err);
    }
    // Case 1: Encode a wrapped image.Image
    var buf = @new<bytes.Buffer>();
    var w0 = new offsetImage(m0, m0.Bounds());
    err = Encode(new gif_internal_test_package.bytes_BufferжWriter(buf), w0, nil);
    if (err != default!) {
        Ꮡt.Fatalf("Encode: %v"u8, err);
    }
    (var w1, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(buf));
    if (err != default!) {
        Ꮡt.Fatalf("Dencode: %v"u8, err);
    }
    var avgDelta = averageDelta(m0, w1);
    if (avgDelta > 0) {
        Ꮡt.Fatalf("Wrapped: average delta is too high. expected: 0, got %d"u8, avgDelta);
    }
    // Case 2: Encode a wrapped image.Image with offset
    var b0 = new image.Rectangle(
        Min: new image.Point(
            X: 128,
            Y: 64
        ),
        Max: new image.Point(
            X: 256,
            Y: 128
        )
    );
    w0 = new offsetImage(m0, b0);
    buf = @new<bytes.Buffer>();
    err = Encode(new gif_internal_test_package.bytes_BufferжWriter(buf), w0, nil);
    if (err != default!) {
        Ꮡt.Fatalf("Encode: %v"u8, err);
    }
    (w1, err) = Decode(new gif_internal_test_package.bytes_BufferжReader(buf));
    if (err != default!) {
        Ꮡt.Fatalf("Dencode: %v"u8, err);
    }
    var b1 = new image.Rectangle(
        Min: new image.Point(
            X: 0,
            Y: 0
        ),
        Max: new image.Point(
            X: 128,
            Y: 64
        )
    );
    avgDelta = averageDeltaBound(m0, w1, b0, b1);
    if (avgDelta > 0) {
        Ꮡt.Fatalf("Wrapped and offset: average delta is too high. expected: 0, got %d"u8, avgDelta);
    }
}

public static void BenchmarkEncodeRandomPaletted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var paletted = image.NewPaletted(image.Rect(0, 0, 640, 480), palette.Plan9);
    var rnd = rand.New(rand.NewSource(123));
    foreach (var (i, _) in (~paletted).Pix) {
        paletted.Value.Pix[i] = (uint8)rnd.Intn(256);
    }
    b.SetBytes(640 * 480 * 1);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.PalettedжImage(paletted), nil);
    }
}

public static void BenchmarkEncodeRandomRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var rgba = image.NewRGBA(image.Rect(0, 0, 640, 480));
    var bo = rgba.Bounds();
    var rnd = rand.New(rand.NewSource(123));
    for (nint y = bo.Min.Y; y < bo.Max.Y; y++) {
        for (nint x = bo.Min.X; x < bo.Max.X; x++) {
            rgba.SetRGBA(x, y, new colorꓸRGBA(
                (uint8)rnd.Intn(256),
                (uint8)rnd.Intn(256),
                (uint8)rnd.Intn(256),
                255
            ));
        }
    }
    b.SetBytes(640 * 480 * 4);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.ΔRGBAжImage(rgba), nil);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataVideo001Pngˢ = "../testdata/video-001.png"u8;

public static void BenchmarkEncodeRealisticPaletted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (img, err) = readImg(testdataVideo001Pngˢ);
    if (err != default!) {
        Ꮡb.Fatalf("readImg: %v"u8, err);
    }
    var bo = img.Bounds();
    var paletted = image.NewPaletted(bo, palette.Plan9);
    draw.Draw(new gif_internal_test_package.image_PalettedжImage(paletted), bo, img, bo.Min, draw.Src);
    b.SetBytes((int64)(bo.Dx() * bo.Dy() * 1));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.PalettedжImage(paletted), nil);
    }
}

public static void BenchmarkEncodeRealisticRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (img, err) = readImg(testdataVideo001Pngˢ);
    if (err != default!) {
        Ꮡb.Fatalf("readImg: %v"u8, err);
    }
    var bo = img.Bounds();
    // Converting img to rgba is redundant for video-001.png, which is already
    // in the RGBA format, but for those copy/pasting this benchmark (but
    // changing the source image), the conversion ensures that we're still
    // benchmarking encoding an RGBA image.
    var rgba = image.NewRGBA(bo);
    draw.Draw(new gif_internal_test_package.image_ΔRGBAжImage(rgba), bo, img, bo.Min, draw.Src);
    b.SetBytes((int64)(bo.Dx() * bo.Dy() * 4));
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.ΔRGBAжImage(rgba), nil);
    }
}

} // end gif_internal_test_package
