// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using fmt = fmt_package;
using image = image_package;
using color = go.image.color_package;
using png = go.image.png_package;
using io = io_package;
using rand = go.math.rand_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using go.image;
using go.math;
using static go.image.jpeg_package;

partial class jpeg_internal_test_package {

// zigzag maps from the natural ordering to the zig-zag ordering. For example,
// zigzag[0*8 + 3] is the zig-zag sequence number of the element in the fourth
// column and first row.
internal static array<nint> zigzag = new nint[]{
    0, 1, 5, 6, 14, 15, 27, 28,
    2, 4, 7, 13, 16, 26, 29, 42,
    3, 8, 12, 17, 25, 30, 41, 43,
    9, 11, 18, 24, 31, 40, 44, 53,
    10, 19, 23, 32, 39, 45, 52, 54,
    20, 22, 33, 38, 46, 51, 55, 60,
    21, 34, 37, 47, 50, 56, 59, 61,
    35, 36, 48, 49, 57, 58, 62, 63
}.array();

public static void TestZigUnzig(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < blockSize; i++) {
        if (unzig[zigzag[i]] != i) {
            Ꮡt.Errorf("unzig[zigzag[%d]] == %d"u8, i, unzig[zigzag[i]]);
        }
        if (zigzag[unzig[i]] != i) {
            Ꮡt.Errorf("zigzag[unzig[%d]] == %d"u8, i, zigzag[unzig[i]]);
        }
    }
}

// Luminance.
// Chrominance.
// unscaledQuantInNaturalOrder are the unscaled quantization tables in
// natural (not zig-zag) order, as specified in section K.1.
internal static array<array<byte>> unscaledQuantInNaturalOrder = new array<byte>[]{
    new byte[]{
        16, 11, 10, 16, 24, 40, 51, 61,
        12, 12, 14, 19, 26, 58, 60, 55,
        14, 13, 16, 24, 40, 57, 69, 56,
        14, 17, 22, 29, 51, 87, 80, 62,
        18, 22, 37, 56, 68, 109, 103, 77,
        24, 35, 55, 64, 81, 104, 113, 92,
        49, 64, 78, 87, 103, 121, 120, 101,
        72, 92, 95, 98, 112, 100, 103, 99}.array(),
    new byte[]{
        17, 18, 24, 47, 99, 99, 99, 99,
        18, 21, 26, 66, 99, 99, 99, 99,
        24, 26, 56, 99, 99, 99, 99, 99,
        47, 66, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99}.array()
}.array();

public static void TestUnscaledQuant(ж<testing.T> Ꮡt) {
    var bad = false;
    for (global::go.image.jpeg_package.quantIndex i = ((global::go.image.jpeg_package.quantIndex)0); i < nQuantIndex; i++) {
        for (nint zig = 0; zig < blockSize; zig++) {
            var got = unscaledQuant[i][zig];
            var want = unscaledQuantInNaturalOrder[i][unzig[zig]];
            if (got != want) {
                Ꮡt.Errorf("i=%d, zig=%d: got %d, want %d"u8, i, zig, got, want);
                bad = true;
            }
        }
    }
    if (bad) {
        var names = new @string[]{"Luminance"u8, "Chrominance"u8}.array();
        var buf = Ꮡ(new strings.Builder(nil));
        foreach (var (i, name) in names.ΔRangeSnapshot()) {
            fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(buf), "// %s.\n{\n"u8, name);
            for (nint zig = 0; zig < blockSize; zig++) {
                fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(buf), "%d, "u8, unscaledQuantInNaturalOrder[i][unzig[zig]]);
                if (zig % 8 == 7) {
                    buf.WriteString("\n"u8);
                }
            }
            buf.WriteString("},\n"u8);
        }
        Ꮡt.Logf("expected unscaledQuant values:\n%s"u8, buf.String());
    }
}


[GoType("dyn")] partial struct testCaseᴛ1 {
    internal @string filename;
    internal nint quality;
    internal int64 tolerance;
}
internal static slice<testCaseᴛ1> testCase = new testCaseᴛ1[]{
    new("../testdata/video-001.png"u8, 1, ((int64)24 << (int)(8))),
    new("../testdata/video-001.png"u8, 20, ((int64)12 << (int)(8))),
    new("../testdata/video-001.png"u8, 60, ((int64)8 << (int)(8))),
    new("../testdata/video-001.png"u8, 80, ((int64)6 << (int)(8))),
    new("../testdata/video-001.png"u8, 90, ((int64)4 << (int)(8))),
    new("../testdata/video-001.png"u8, 100, ((int64)2 << (int)(8)))
}.slice();

internal static int64 delta(uint32 u0, uint32 u1) {
    var d = (int64)u0 - (int64)u1;
    if (d < 0) {
        return -d;
    }
    return d;
}

internal static (image.Image, error) readPng(@string filename) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(filename);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return png.Decode(new jpeg_internal_test_package.os_FileжReader(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static void TestWriter(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in testCase) {
        // Read the image.
        var (m0, err) = readPng(tc.filename);
        if (err != default!) {
            Ꮡt.Error(tc.filename, err);
            continue;
        }
        // Encode that image as JPEG.
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = Encode(new jpeg_internal_test_package.bytes_BufferжWriter(Ꮡbuf), m0, Ꮡ(new Options(Quality: tc.quality)));
        if (err != default!) {
            Ꮡt.Error(tc.filename, err);
            continue;
        }
        // Decode that JPEG.
        (var m1, err) = Decode(new jpeg_internal_test_package.bytes_BufferжReader(Ꮡbuf));
        if (err != default!) {
            Ꮡt.Error(tc.filename, err);
            continue;
        }
        if (m0.Bounds() != m1.Bounds()) {
            Ꮡt.Errorf("%s, bounds differ: %v and %v"u8, tc.filename, m0.Bounds(), m1.Bounds());
            continue;
        }
        // Compare the average delta to the tolerance level.
        if (averageDelta(m0, m1) > tc.tolerance) {
            Ꮡt.Errorf("%s, quality=%d: average delta is too high"u8, tc.filename, tc.quality);
            continue;
        }
    }
}

// TestWriteGrayscale tests that a grayscale images survives a round-trip
// through encode/decode cycle.
public static void TestWriteGrayscale(ж<testing.T> Ꮡt) {
    var m0 = image.NewGray(image.Rect(0, 0, 32, 32));
    foreach (var (i, _) in (~m0).Pix) {
        m0.Value.Pix[i] = (uint8)i;
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = Encode(new jpeg_internal_test_package.bytes_BufferжWriter(Ꮡbuf), new image.GrayжImage(m0), nil); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var (m1, err) = Decode(new jpeg_internal_test_package.bytes_BufferжReader(Ꮡbuf));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (m0.Bounds() != m1.Bounds()) {
        Ꮡt.Fatalf("bounds differ: %v and %v"u8, m0.Bounds(), m1.Bounds());
    }
    {
        var (_, ok) = m1._<ж<image.Gray>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("got %T, want *image.Gray"u8, m1);
        }
    }
    // Compare the average delta to the tolerance level.
    var want = (int64)(((int64)2 << (int)(8)));
    {
        var got = averageDelta(new image.GrayжImage(m0), m1); if (got > want) {
            Ꮡt.Errorf("average delta too high; got %d, want <= %d"u8, got, want);
        }
    }
}

// averageDelta returns the average delta in RGB space. The two images must
// have the same bounds.
internal static int64 averageDelta(image.Image m0, image.Image m1) {
    var b = m0.Bounds();
    int64 sum = default!;
    int64 n = default!;
    for (nint y = b.Min.Y; y < b.Max.Y; y++) {
        for (nint x = b.Min.X; x < b.Max.X; x++) {
            var c0 = m0.At(x, y);
            var c1 = m1.At(x, y);
            var (r0, g0, b0, _) = c0.RGBA();
            var (r1, g1, b1, _) = c1.RGBA();
            sum += delta(r0, r1);
            sum += delta(g0, g1);
            sum += delta(b0, b1);
            n += 3;
        }
    }
    return quo(sum, n);
}

public static void TestEncodeYCbCr(ж<testing.T> Ꮡt) {
    var bo = image.Rect(0, 0, 640, 480);
    var imgRGBA = image.NewRGBA(bo);
    // Must use 444 subsampling to avoid lossy RGBA to YCbCr conversion.
    var imgYCbCr = image.NewYCbCr(bo, image.YCbCrSubsampleRatio444);
    var rnd = rand.New(rand.NewSource(123));
    // Create identical rgba and ycbcr images.
    for (nint y = bo.Min.Y; y < bo.Max.Y; y++) {
        for (nint x = bo.Min.X; x < bo.Max.X; x++) {
            var col = new colorꓸRGBA(
                (uint8)rnd.Intn(256),
                (uint8)rnd.Intn(256),
                (uint8)rnd.Intn(256),
                255
            );
            imgRGBA.SetRGBA(x, y, col);
            nint yo = imgYCbCr.YOffset(x, y);
            nint co = imgYCbCr.COffset(x, y);
            var (cy, ccr, ccb) = color.RGBToYCbCr(col.R, col.G, col.B);
            imgYCbCr.Value.Y[yo] = cy;
            imgYCbCr.Value.Cb[co] = ccr;
            imgYCbCr.Value.Cr[co] = ccb;
        }
    }
    // Now check that both images are identical after an encode.
    ref var bufRGBA = ref heap(new bytes.Buffer(), out var ᏑbufRGBA);
    ref var bufYCbCr = ref heap(new bytes.Buffer(), out var ᏑbufYCbCr);
    Encode(new jpeg_internal_test_package.bytes_BufferжWriter(ᏑbufRGBA), new image.ΔRGBAжImage(imgRGBA), nil);
    Encode(new jpeg_internal_test_package.bytes_BufferжWriter(ᏑbufYCbCr), new image.YCbCrжImage(imgYCbCr), nil);
    if (!bytes.Equal(bufRGBA.Bytes(), bufYCbCr.Bytes())) {
        Ꮡt.Errorf("RGBA and YCbCr encoded bytes differ"u8);
    }
}

public static void BenchmarkEncodeRGBA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewRGBA(image.Rect(0, 0, 640, 480));
    var bo = img.Bounds();
    var rnd = rand.New(rand.NewSource(123));
    for (nint y = bo.Min.Y; y < bo.Max.Y; y++) {
        for (nint x = bo.Min.X; x < bo.Max.X; x++) {
            img.SetRGBA(x, y, new colorꓸRGBA(
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
    var options = Ꮡ(new Options(Quality: 90));
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.ΔRGBAжImage(img), options);
    }
}

public static void BenchmarkEncodeYCbCr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var img = image.NewYCbCr(image.Rect(0, 0, 640, 480), image.YCbCrSubsampleRatio420);
    var bo = img.Bounds();
    var rnd = rand.New(rand.NewSource(123));
    for (nint y = bo.Min.Y; y < bo.Max.Y; y++) {
        for (nint x = bo.Min.X; x < bo.Max.X; x++) {
            nint cy = img.YOffset(x, y);
            nint ci = img.COffset(x, y);
            img.Value.Y[cy] = (uint8)rnd.Intn(256);
            img.Value.Cb[ci] = (uint8)rnd.Intn(256);
            img.Value.Cr[ci] = (uint8)rnd.Intn(256);
        }
    }
    b.SetBytes(640 * 480 * 3);
    b.ReportAllocs();
    b.ResetTimer();
    var options = Ꮡ(new Options(Quality: 90));
    for (nint i = 0; i < b.N; i++) {
        Encode(io.Discard, new image.YCbCrжImage(img), options);
    }
}

} // end jpeg_internal_test_package
