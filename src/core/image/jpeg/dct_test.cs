// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using fmt = fmt_package;
using math = math_package;
using rand = go.math.rand_package;
using strings = strings_package;
using testing = testing_package;
using go.math;
using io = io_package;
using static go.image.jpeg_package;

partial class jpeg_internal_test_package {

internal static void benchmarkDCT(ж<testing.B> Ꮡb, Action<ж<global::go.image.jpeg_package.block>> f) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var blocks = GoReflect.WithElemDims(new slice<global::go.image.jpeg_package.block>(0, b.N * len(testBlocks)), 64);
    for (nint i = 0; i < b.N; i++) {
        blocks = appendꓸꓸꓸ(blocks, GoReflect.WithElemDims(testBlocks[..], 64));
    }
    b.StartTimer();
    foreach (var (i, _) in blocks) {
        f(Ꮡ(blocks, i));
    }
}

public static void BenchmarkFDCT(ж<testing.B> Ꮡb) {
    benchmarkDCT(Ꮡb, fdct);
}

public static void BenchmarkIDCT(ж<testing.B> Ꮡb) {
    benchmarkDCT(Ꮡb, idct);
}

public static void TestDCT(ж<testing.T> Ꮡt) {
    var blocks = GoReflect.WithElemDims(new slice<global::go.image.jpeg_package.block>(len(testBlocks)), 64);
    copy(blocks, GoReflect.WithElemDims(testBlocks[..], 64));
    // Append some randomly generated blocks of varying sparseness.
    var r = rand.New(rand.NewSource(123));
    for (nint i = 0; i < 100; i++) {
        var b = new block(new int32[64].array());
        nint n = r.Int() % 64;
        for (nint j = 0; j < n; j++) {
            b[r.Int() % len(b)] = r.Int31() % 256;
        }
        blocks = append(blocks, b.Clone());
    }
    // Check that the FDCT and IDCT functions are inverses, after a scale and
    // level shift. Scaling reduces the rounding errors in the conversion from
    // floats to ints.
    foreach (var (i, vᴛ1) in blocks) {
        ref var b = ref heap(new global::go.image.jpeg_package.block(), out var Ꮡb);
        b = vᴛ1.Clone();

        ref var got = ref heap<global::go.image.jpeg_package.block>(out var Ꮡgot);
        got = b.Clone();
        ref var want = ref heap<global::go.image.jpeg_package.block>(out var Ꮡwant);
        want = b.Clone();
        foreach (var (j, _) in got) {
            got[j] = (got[j] - 128) * 8;
        }
        slowFDCT(Ꮡgot);
        slowIDCT(Ꮡgot);
        foreach (var (j, _) in got) {
            got[j] = got[j] / 8 + 128;
        }
        if (differ(Ꮡgot, Ꮡwant)) {
            Ꮡt.Errorf("i=%d: IDCT(FDCT)\nsrc\n%s\ngot\n%s\nwant\n%s\n"u8, i, Ꮡb, Ꮡgot, Ꮡwant);
        }
    }
    // Check that the optimized and slow FDCT implementations agree.
    // The fdct function already does a scale and level shift.
    foreach (var (i, vᴛ2) in blocks) {
        ref var b = ref heap(new global::go.image.jpeg_package.block(), out var Ꮡb);
        b = vᴛ2.Clone();

        ref var got = ref heap<global::go.image.jpeg_package.block>(out var Ꮡgot);
        got = b.Clone();
        ref var want = ref heap<global::go.image.jpeg_package.block>(out var Ꮡwant);
        want = b.Clone();
        fdct(Ꮡgot);
        foreach (var (j, _) in want) {
            want[j] = (want[j] - 128) * 8;
        }
        slowFDCT(Ꮡwant);
        if (differ(Ꮡgot, Ꮡwant)) {
            Ꮡt.Errorf("i=%d: FDCT\nsrc\n%s\ngot\n%s\nwant\n%s\n"u8, i, Ꮡb, Ꮡgot, Ꮡwant);
        }
    }
    // Check that the optimized and slow IDCT implementations agree.
    foreach (var (i, vᴛ3) in blocks) {
        ref var b = ref heap(new global::go.image.jpeg_package.block(), out var Ꮡb);
        b = vᴛ3.Clone();

        ref var got = ref heap<global::go.image.jpeg_package.block>(out var Ꮡgot);
        got = b.Clone();
        ref var want = ref heap<global::go.image.jpeg_package.block>(out var Ꮡwant);
        want = b.Clone();
        idct(Ꮡgot);
        slowIDCT(Ꮡwant);
        if (differ(Ꮡgot, Ꮡwant)) {
            Ꮡt.Errorf("i=%d: IDCT\nsrc\n%s\ngot\n%s\nwant\n%s\n"u8, i, Ꮡb, Ꮡgot, Ꮡwant);
        }
    }
}

// differ reports whether any pair-wise elements in b0 and b1 differ by 2 or
// more. That tolerance is because there isn't a single definitive decoding of
// a given JPEG image, even before the YCbCr to RGB conversion; implementations
// can have different IDCT rounding errors.
internal static bool differ(ж<global::go.image.jpeg_package.block> Ꮡb0, ж<global::go.image.jpeg_package.block> Ꮡb1) {
    ref var b0 = ref Ꮡb0.DerefOrNull();
    ref var b1 = ref Ꮡb1.DerefOrNull();

    foreach (var (i, _) in b0) {
        var delta = b0[i] - b1[i];
        if (delta < -2 || +2 < delta) {
            return true;
        }
    }
    return false;
}

// alpha returns 1 if i is 0 and returns √2 otherwise.
internal static float64 alpha(nint i) {
    if (i == 0) {
        return 1D;
    }
    return math.Sqrt2;
}

// cos(π/16 *  0)
// cos(π/16 *  1)
// cos(π/16 *  2)
// cos(π/16 *  3)
// cos(π/16 *  4)
// cos(π/16 *  5)
// cos(π/16 *  6)
// cos(π/16 *  7)
// cos(π/16 *  8)
// cos(π/16 *  9)
// cos(π/16 * 10)
// cos(π/16 * 11)
// cos(π/16 * 12)
// cos(π/16 * 13)
// cos(π/16 * 14)
// cos(π/16 * 15)
// cos(π/16 * 16)
// cos(π/16 * 17)
// cos(π/16 * 18)
// cos(π/16 * 19)
// cos(π/16 * 20)
// cos(π/16 * 21)
// cos(π/16 * 22)
// cos(π/16 * 23)
// cos(π/16 * 24)
// cos(π/16 * 25)
// cos(π/16 * 26)
// cos(π/16 * 27)
// cos(π/16 * 28)
// cos(π/16 * 29)
// cos(π/16 * 30)
// cos(π/16 * 31)
internal static array<float64> cosines = new float64[]{
    +1.0000000000000000000000000000000000000000000000000000000000000000D,
    +0.9807852804032304491261822361342390369739337308933360950029160885D,
    +0.9238795325112867561281831893967882868224166258636424861150977312D,
    +0.8314696123025452370787883776179057567385608119872499634461245902D,
    +0.7071067811865475244008443621048490392848359376884740365883398689D,
    +0.5555702330196022247428308139485328743749371907548040459241535282D,
    +0.3826834323650897717284599840303988667613445624856270414338006356D,
    +0.1950903220161282678482848684770222409276916177519548077545020894D,
    -0.0000000000000000000000000000000000000000000000000000000000000000D,
    -0.1950903220161282678482848684770222409276916177519548077545020894D,
    -0.3826834323650897717284599840303988667613445624856270414338006356D,
    -0.5555702330196022247428308139485328743749371907548040459241535282D,
    -0.7071067811865475244008443621048490392848359376884740365883398689D,
    -0.8314696123025452370787883776179057567385608119872499634461245902D,
    -0.9238795325112867561281831893967882868224166258636424861150977312D,
    -0.9807852804032304491261822361342390369739337308933360950029160885D,
    -1.0000000000000000000000000000000000000000000000000000000000000000D,
    -0.9807852804032304491261822361342390369739337308933360950029160885D,
    -0.9238795325112867561281831893967882868224166258636424861150977312D,
    -0.8314696123025452370787883776179057567385608119872499634461245902D,
    -0.7071067811865475244008443621048490392848359376884740365883398689D,
    -0.5555702330196022247428308139485328743749371907548040459241535282D,
    -0.3826834323650897717284599840303988667613445624856270414338006356D,
    -0.1950903220161282678482848684770222409276916177519548077545020894D,
    +0.0000000000000000000000000000000000000000000000000000000000000000D,
    +0.1950903220161282678482848684770222409276916177519548077545020894D,
    +0.3826834323650897717284599840303988667613445624856270414338006356D,
    +0.5555702330196022247428308139485328743749371907548040459241535282D,
    +0.7071067811865475244008443621048490392848359376884740365883398689D,
    +0.8314696123025452370787883776179057567385608119872499634461245902D,
    +0.9238795325112867561281831893967882868224166258636424861150977312D,
    +0.9807852804032304491261822361342390369739337308933360950029160885D
}.array();

// slowFDCT performs the 8*8 2-dimensional forward discrete cosine transform:
//
//	dst[u,v] = (1/8) * Σ_x Σ_y alpha(u) * alpha(v) * src[x,y] *
//		cos((π/2) * (2*x + 1) * u / 8) *
//		cos((π/2) * (2*y + 1) * v / 8)
//
// x and y are in pixel space, and u and v are in transform space.
//
// b acts as both dst and src.
internal static void slowFDCT(ж<global::go.image.jpeg_package.block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<float64> dst = new(64); /* blockSize */
    for (nint v = 0; v < 8; v++) {
        for (nint u = 0; u < 8; u++) {
            var sum = 0.0D;
            for (nint y = 0; y < 8; y++) {
                for (nint x = 0; x < 8; x++) {
                    sum += alpha(u) * alpha(v) * (float64)b[8 * y + x] * cosines[((2 * x + 1) * u) % 32] * cosines[((2 * y + 1) * v) % 32];
                }
            }
            dst[8 * v + u] = sum / 8D;
        }
    }
    // Convert from float64 to int32.
    foreach (var (i, _) in dst) {
        b[i] = (int32)(dst[i] + 0.5D);
    }
}

// slowIDCT performs the 8*8 2-dimensional inverse discrete cosine transform:
//
//	dst[x,y] = (1/8) * Σ_u Σ_v alpha(u) * alpha(v) * src[u,v] *
//		cos((π/2) * (2*x + 1) * u / 8) *
//		cos((π/2) * (2*y + 1) * v / 8)
//
// x and y are in pixel space, and u and v are in transform space.
//
// b acts as both dst and src.
internal static void slowIDCT(ж<global::go.image.jpeg_package.block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    array<float64> dst = new(64); /* blockSize */
    for (nint y = 0; y < 8; y++) {
        for (nint x = 0; x < 8; x++) {
            var sum = 0.0D;
            for (nint v = 0; v < 8; v++) {
                for (nint u = 0; u < 8; u++) {
                    sum += alpha(u) * alpha(v) * (float64)b[8 * v + u] * cosines[((2 * x + 1) * u) % 32] * cosines[((2 * y + 1) * v) % 32];
                }
            }
            dst[8 * y + x] = sum / 8D;
        }
    }
    // Convert from float64 to int32.
    foreach (var (i, _) in dst) {
        b[i] = (int32)(dst[i] + 0.5D);
    }
}

internal static @string String(this ref global::go.image.jpeg_package.block b) {
    var s = Ꮡ(new strings.Builder(nil));
    fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "{\n"u8);
    for (nint y = 0; y < 8; y++) {
        fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "\t"u8);
        for (nint x = 0; x < 8; x++) {
            fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "0x%04x, "u8, (uint16)b.Value[8 * y + x]);
        }
        fmt.Fprintln(new jpeg_internal_test_package.strings_BuilderжWriter(s));
    }
    fmt.Fprintf(new jpeg_internal_test_package.strings_BuilderжWriter(s), "}"u8);
    return s.String();
}

// testBlocks are the first 10 pre-IDCT blocks from ../testdata/video-001.jpeg.
internal static array<global::go.image.jpeg_package.block> testBlocks = new global::go.image.jpeg_package.block[]{
    new int32[]{
        0x7f, 0xf6, 0x01, 0x07, 0xff, 0x00, 0x00, 0x00,
        0xf5, 0x01, 0xfa, 0x01, 0xfe, 0x00, 0x01, 0x00,
        0x05, 0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0xff, 0xf8, 0x00, 0x01, 0xff, 0x00, 0x00,
        0x00, 0x01, 0x00, 0x01, 0x00, 0xff, 0xff, 0x00,
        0xff, 0x0c, 0x00, 0x00, 0x00, 0x00, 0xff, 0x01,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x01, 0xff, 0x01, 0x00, 0xfe}.array(),
    new int32[]{
        0x29, 0x07, 0x00, 0xfc, 0x01, 0x01, 0x00, 0x00,
        0x07, 0x00, 0x03, 0x00, 0x01, 0x00, 0xff, 0xff,
        0xff, 0xfd, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x04, 0x00, 0xff, 0x01, 0x00, 0x00,
        0x01, 0x00, 0x01, 0xff, 0x00, 0x00, 0x00, 0x00,
        0x01, 0xfa, 0x01, 0x00, 0x01, 0x00, 0x01, 0xff,
        0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0xff, 0x00, 0xff, 0x00, 0x02}.array(),
    new int32[]{
        0xc5, 0xfa, 0x01, 0x00, 0x00, 0x01, 0x00, 0xff,
        0x02, 0xff, 0x01, 0x00, 0x01, 0x00, 0xff, 0x00,
        0xff, 0xff, 0x00, 0xff, 0x01, 0x00, 0x00, 0x00,
        0xff, 0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff,
        0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.array(),
    new int32[]{
        0x86, 0x05, 0x00, 0x02, 0x00, 0x00, 0x01, 0x00,
        0xf2, 0x06, 0x00, 0x00, 0x01, 0x02, 0x00, 0x00,
        0xf6, 0xfa, 0xf9, 0x00, 0xff, 0x01, 0x00, 0x00,
        0xf9, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00,
        0x00, 0xff, 0x00, 0xff, 0xff, 0xff, 0x00, 0x00,
        0xff, 0x00, 0x00, 0x01, 0x00, 0xff, 0x01, 0x00,
        0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x01,
        0x00, 0x01, 0xff, 0x01, 0x00, 0xff, 0x00, 0x00}.array(),
    new int32[]{
        0x24, 0xfe, 0x00, 0xff, 0x00, 0xff, 0xff, 0x00,
        0x08, 0xfd, 0x00, 0x01, 0x01, 0x00, 0x01, 0x00,
        0x06, 0x03, 0x03, 0xff, 0x00, 0x00, 0x00, 0x00,
        0x04, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01,
        0x01, 0x00, 0x01, 0xff, 0x00, 0x01, 0x00, 0x00,
        0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0x01}.array(),
    new int32[]{
        0xcd, 0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
        0x03, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0xff}.array(),
    new int32[]{
        0x81, 0xfe, 0x05, 0xff, 0x01, 0xff, 0x01, 0x00,
        0xef, 0xf9, 0x00, 0xf9, 0x00, 0xff, 0x00, 0xff,
        0x05, 0xf9, 0x00, 0xf8, 0x01, 0xff, 0x01, 0xff,
        0x00, 0xff, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x01,
        0xff, 0x01, 0x01, 0x00, 0xff, 0x00, 0x00, 0x00,
        0x01, 0x01, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff}.array(),
    new int32[]{
        0x28, 0x00, 0xfe, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x0b, 0x02, 0x01, 0x03, 0x00, 0xff, 0x00, 0x01,
        0xfe, 0x02, 0x01, 0x03, 0xff, 0x00, 0x00, 0x00,
        0x01, 0x00, 0xfd, 0x00, 0x01, 0x00, 0xff, 0x00,
        0x01, 0xff, 0x00, 0xff, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0xff, 0x01, 0x01, 0x00, 0xff,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0x01}.array(),
    new int32[]{
        0xdf, 0xf9, 0xfe, 0x00, 0x03, 0x01, 0xff, 0xff,
        0x04, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
        0xff, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x00, 0x00, 0xfe, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0xff, 0x01, 0x00, 0x00, 0x00, 0x01,
        0xff, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
        0x00, 0xff, 0x00, 0xff, 0x01, 0x00, 0x00, 0x01,
        0xff, 0xff, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00}.array(),
    new int32[]{
        0x88, 0xfd, 0x00, 0x00, 0xff, 0x00, 0x01, 0xff,
        0xe1, 0x06, 0x06, 0x01, 0xff, 0x00, 0x01, 0x00,
        0x08, 0x00, 0xfa, 0x00, 0xff, 0xff, 0xff, 0xff,
        0x08, 0x01, 0x00, 0xff, 0x01, 0xff, 0x00, 0x00,
        0xf5, 0xff, 0x00, 0x01, 0xff, 0x01, 0x01, 0x00,
        0xff, 0xff, 0x01, 0xff, 0x01, 0x00, 0x01, 0x00,
        0x00, 0x01, 0x01, 0xff, 0x00, 0xff, 0x00, 0x01,
        0x02, 0x00, 0x00, 0xff, 0xff, 0x00, 0xff, 0x00}.array()
}.array();

} // end jpeg_internal_test_package
