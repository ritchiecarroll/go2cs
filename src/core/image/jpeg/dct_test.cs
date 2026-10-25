// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using fmt = fmt_package;
using math = math_package;
using big = go.math.big_package;
using rand = go.math.rand_package;
using strings = strings_package;
using testing = testing_package;
using go.math;
using io = io_package;
using static go.image.jpeg_package;

partial class jpeg_internal_test_package {

internal static void benchmarkDCT(ж<testing.B> Ꮡb, Action<ж<global::go.image.jpeg_package.block>> f) {
    ref var blk = ref heap(new global::go.image.jpeg_package.block(), out var Ꮡblk);                                       // avoid potential allocation in loop
    while (Ꮡb.Loop()) {
        foreach (var (_, vᴛ1) in testBlocks.ΔRangeSnapshot()) {
            blk = vᴛ1.Clone();

            f(Ꮡblk);
        }
    }
}

public static void BenchmarkFDCT(ж<testing.B> Ꮡb) {
    benchmarkDCT(Ꮡb, fdct);
}

public static void BenchmarkIDCT(ж<testing.B> Ꮡb) {
    benchmarkDCT(Ꮡb, idct);
}

internal const bool testSlowVsBig = true;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string idctFdctˢ = "IDCT(FDCT)"u8;
internal static readonly @string slowFDCTˢ = "slowFDCT"u8;
internal static readonly @string slowIDCTˢ = "slowIDCT"u8;
internal static readonly @string fdctˢ = "FDCT"u8;
internal static readonly @string idctˢ = "IDCT"u8;

public static void TestDCT(ж<testing.T> Ꮡt) {
    var blocks = GoReflect.WithElemDims(new slice<global::go.image.jpeg_package.block>(len(testBlocks)), 64);
    copy(blocks, GoReflect.WithElemDims(testBlocks[..], 64));

    // All zeros
    blocks = append(blocks, new block(new int32[64].array()));

    // Every possible unit impulse.
    foreach (var i in range(blockSize)) {
        global::go.image.jpeg_package.block b = default!;
        b[i] = 255;
        blocks = append(blocks, b.Clone());
    }

    // All ones.
    global::go.image.jpeg_package.block ones = default!;
    foreach (var i in range(ones)) {
        ones[i] = 255;
    }
    blocks = append(blocks, ones.Clone());

    // Every possible inverted unit impulse.
    foreach (var i in range(blockSize)) {
        ones[i] = 0;
        blocks = append(blocks, ones.Clone());
        ones[i] = 255;
    }

    // Some randomly generated blocks of varying sparseness.
    var r = rand.New(rand.NewSource(123));
    for (nint i = 0; i < 100; i++) {
        var b = new block(new int32[64].array());
        nint n = r.Int() % 64;
        for (nint j = 0; j < n; j++) {
            b[r.Int() % len(b)] = r.Int31() % 256;
        }
        blocks = append(blocks, b.Clone());
    }

    // Check that the slow FDCT and IDCT functions are inverses,
    // after a scale and level shift.
    // Scaling reduces the rounding errors in the conversion.
    // The “fast” ones are not inverses because the fast IDCT
    // is optimized for 8-bit inputs, not full 16-bit ones.
    var slowRoundTrip = (ж<global::go.image.jpeg_package.block> b) => {
        slowFDCT(b);
        slowIDCT(b);
        foreach (var j in range(b, 64)) {
            b.Value[j] = b.Value[j] / 8 + 128;
        }
    };
    var nop = (ж<global::go.image.jpeg_package.block> _) => {
    };
    testDCT(Ꮡt, idctFdctˢ, blocks, slowRoundTrip, nop, 1, 8);

    if (testSlowVsBig) {
        testDCT(Ꮡt, slowFDCTˢ, blocks, slowFDCT, slowerFDCT, 0, 64);
        testDCT(Ꮡt, slowIDCTˢ, blocks, slowIDCT, slowerIDCT, 0, 64);
    }

    // Check that the optimized and slow FDCT implementations agree.
    testDCT(Ꮡt, fdctˢ, blocks, fdct, slowFDCT, 1, 8);
    testDCT(Ꮡt, idctˢ, blocks, idct, slowIDCT, 1, 8);
}

internal static void testDCT(ж<testing.T> Ꮡt, @string name, slice<global::go.image.jpeg_package.block> blocks, Action<ж<global::go.image.jpeg_package.block>> fhave, Action<ж<global::go.image.jpeg_package.block>> fwant, int32 tolerance, nint maxCloseCalls) {
    var blocksʗ1 = blocks;
    Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
        nint totalClose = 0;
        foreach (var (i, vᴛ1) in blocksʗ1) {
            ref var b = ref heap(new global::go.image.jpeg_package.block(), out var Ꮡb);
            b = vᴛ1.Clone();

            ref var have = ref heap<global::go.image.jpeg_package.block>(out var Ꮡhave);
            have = b.Clone();
            ref var want = ref heap<global::go.image.jpeg_package.block>(out var Ꮡwant);
            want = b.Clone();
            fhave(Ꮡhave);
            fwant(Ꮡwant);
            var (d, n) = differ(Ꮡhave, Ꮡwant, tolerance);
            if (d >= 0 || n > maxCloseCalls) {
                @string fail = ""u8;
                if (d >= 0) {
                    fail = fmt.Sprintf("diff at %d,%d"u8, d / 8, d % 8);
                }
                if (n > maxCloseCalls) {
                    if (fail != ""u8) {
                        fail += "; "u8;
                    }
                    fail += fmt.Sprintf("%d close calls"u8, n);
                }
                tΔ1.Errorf("i=%d: %s (%s)\nsrc\n%s\nhave\n%s\nwant\n%s\n"u8,
                    i, name, fail, Ꮡb, Ꮡhave, Ꮡwant);
            }
            totalClose += n;
        }
        if (tolerance > 0) {
            tΔ1.Logf("%d/%d total close calls"u8, totalClose, len(blocksʗ1) * (nint)blockSize);
        }
    });
}

// differ returns the index of the first pair-wise elements in b0 and b1
// that differ by more than 'ok', along with the total number of elements
// that differ by at least ok ("close calls").
//
// There isn't a single definitive decoding of a given JPEG image,
// even before the YCbCr to RGB conversion; implementations
// can have different IDCT rounding errors.
//
// If there are no differences, differ returns -1, 0.
internal static (nint index, nint closeCalls) differ(ж<global::go.image.jpeg_package.block> Ꮡb0, ж<global::go.image.jpeg_package.block> Ꮡb1, int32 ok) {
    nint index = default!;
    nint closeCalls = default!;
    ref var b0 = ref Ꮡb0.DerefOrNull();
    ref var b1 = ref Ꮡb1.DerefOrNull();

    index = -1;
    foreach (var i in range(b0)) {
        var delta = b0[i] - b1[i];
        if (delta < -ok || ok < delta) {
            if (index < 0) {
                index = i;
            }
        }
        if (delta <= -ok || ok <= delta) {
            closeCalls++;
        }
    }
    return (index, closeCalls);
}

// alpha returns 1 if i is 0 and returns √2 otherwise.
internal static float64 alpha(nint i) {
    if (i == 0) {
        return 1D;
    }
    return math.Sqrt2;
}

// bigAlpha returns 1 if i is 0 and returns √2 otherwise.
internal static ж<big.Float> bigAlpha(nint i) {
    if (i == 0) {
        return bigFloat1;
    }
    return bigFloatSqrt2;
}

internal static array<float64> cosines = new float64[]{
    +1.0000000000000000000000000000000000000000000000000000000000000000D, // cos(π/16 *  0)
    +0.9807852804032304491261822361342390369739337308933360950029160885D, // cos(π/16 *  1)
    +0.9238795325112867561281831893967882868224166258636424861150977312D, // cos(π/16 *  2)
    +0.8314696123025452370787883776179057567385608119872499634461245902D, // cos(π/16 *  3)
    +0.7071067811865475244008443621048490392848359376884740365883398689D, // cos(π/16 *  4)
    +0.5555702330196022247428308139485328743749371907548040459241535282D, // cos(π/16 *  5)
    +0.3826834323650897717284599840303988667613445624856270414338006356D, // cos(π/16 *  6)
    +0.1950903220161282678482848684770222409276916177519548077545020894D, // cos(π/16 *  7)

    -0.0000000000000000000000000000000000000000000000000000000000000000D, // cos(π/16 *  8)
    -0.1950903220161282678482848684770222409276916177519548077545020894D, // cos(π/16 *  9)
    -0.3826834323650897717284599840303988667613445624856270414338006356D, // cos(π/16 * 10)
    -0.5555702330196022247428308139485328743749371907548040459241535282D, // cos(π/16 * 11)
    -0.7071067811865475244008443621048490392848359376884740365883398689D, // cos(π/16 * 12)
    -0.8314696123025452370787883776179057567385608119872499634461245902D, // cos(π/16 * 13)
    -0.9238795325112867561281831893967882868224166258636424861150977312D, // cos(π/16 * 14)
    -0.9807852804032304491261822361342390369739337308933360950029160885D, // cos(π/16 * 15)

    -1.0000000000000000000000000000000000000000000000000000000000000000D, // cos(π/16 * 16)
    -0.9807852804032304491261822361342390369739337308933360950029160885D, // cos(π/16 * 17)
    -0.9238795325112867561281831893967882868224166258636424861150977312D, // cos(π/16 * 18)
    -0.8314696123025452370787883776179057567385608119872499634461245902D, // cos(π/16 * 19)
    -0.7071067811865475244008443621048490392848359376884740365883398689D, // cos(π/16 * 20)
    -0.5555702330196022247428308139485328743749371907548040459241535282D, // cos(π/16 * 21)
    -0.3826834323650897717284599840303988667613445624856270414338006356D, // cos(π/16 * 22)
    -0.1950903220161282678482848684770222409276916177519548077545020894D, // cos(π/16 * 23)

    +0.0000000000000000000000000000000000000000000000000000000000000000D, // cos(π/16 * 24)
    +0.1950903220161282678482848684770222409276916177519548077545020894D, // cos(π/16 * 25)
    +0.3826834323650897717284599840303988667613445624856270414338006356D, // cos(π/16 * 26)
    +0.5555702330196022247428308139485328743749371907548040459241535282D, // cos(π/16 * 27)
    +0.7071067811865475244008443621048490392848359376884740365883398689D, // cos(π/16 * 28)
    +0.8314696123025452370787883776179057567385608119872499634461245902D, // cos(π/16 * 29)
    +0.9238795325112867561281831893967882868224166258636424861150977312D, // cos(π/16 * 30)
    +0.9807852804032304491261822361342390369739337308933360950029160885D // cos(π/16 * 31)
}.array();

internal static ж<big.Float> bigFloat(@string s) {
    var (f, ok) = @new<big.Float>().SetString(s);
    if (!ok) {
        throw panic("bad float");
    }
    return f;
}

internal static ж<big.Float> bigFloat1 = big.NewFloat(1D);
internal static ж<big.Float> bigFloatSqrt2 = bigFloat("1.41421356237309504880168872420969807856967187537694807317667974"u8);

internal static array<ж<big.Float>> bigCosines = new ж<big.Float>[]{
    bigFloat("+1.0000000000000000000000000000000000000000000000000000000000000000"u8), // cos(π/16 *  0)
    bigFloat("+0.9807852804032304491261822361342390369739337308933360950029160885"u8), // cos(π/16 *  1)
    bigFloat("+0.9238795325112867561281831893967882868224166258636424861150977312"u8), // cos(π/16 *  2)
    bigFloat("+0.8314696123025452370787883776179057567385608119872499634461245902"u8), // cos(π/16 *  3)
    bigFloat("+0.7071067811865475244008443621048490392848359376884740365883398689"u8), // cos(π/16 *  4)
    bigFloat("+0.5555702330196022247428308139485328743749371907548040459241535282"u8), // cos(π/16 *  5)
    bigFloat("+0.3826834323650897717284599840303988667613445624856270414338006356"u8), // cos(π/16 *  6)
    bigFloat("+0.1950903220161282678482848684770222409276916177519548077545020894"u8), // cos(π/16 *  7)

    bigFloat("-0.0000000000000000000000000000000000000000000000000000000000000000"u8), // cos(π/16 *  8)
    bigFloat("-0.1950903220161282678482848684770222409276916177519548077545020894"u8), // cos(π/16 *  9)
    bigFloat("-0.3826834323650897717284599840303988667613445624856270414338006356"u8), // cos(π/16 * 10)
    bigFloat("-0.5555702330196022247428308139485328743749371907548040459241535282"u8), // cos(π/16 * 11)
    bigFloat("-0.7071067811865475244008443621048490392848359376884740365883398689"u8), // cos(π/16 * 12)
    bigFloat("-0.8314696123025452370787883776179057567385608119872499634461245902"u8), // cos(π/16 * 13)
    bigFloat("-0.9238795325112867561281831893967882868224166258636424861150977312"u8), // cos(π/16 * 14)
    bigFloat("-0.9807852804032304491261822361342390369739337308933360950029160885"u8), // cos(π/16 * 15)

    bigFloat("-1.0000000000000000000000000000000000000000000000000000000000000000"u8), // cos(π/16 * 16)
    bigFloat("-0.9807852804032304491261822361342390369739337308933360950029160885"u8), // cos(π/16 * 17)
    bigFloat("-0.9238795325112867561281831893967882868224166258636424861150977312"u8), // cos(π/16 * 18)
    bigFloat("-0.8314696123025452370787883776179057567385608119872499634461245902"u8), // cos(π/16 * 19)
    bigFloat("-0.7071067811865475244008443621048490392848359376884740365883398689"u8), // cos(π/16 * 20)
    bigFloat("-0.5555702330196022247428308139485328743749371907548040459241535282"u8), // cos(π/16 * 21)
    bigFloat("-0.3826834323650897717284599840303988667613445624856270414338006356"u8), // cos(π/16 * 22)
    bigFloat("-0.1950903220161282678482848684770222409276916177519548077545020894"u8), // cos(π/16 * 23)

    bigFloat("+0.0000000000000000000000000000000000000000000000000000000000000000"u8), // cos(π/16 * 24)
    bigFloat("+0.1950903220161282678482848684770222409276916177519548077545020894"u8), // cos(π/16 * 25)
    bigFloat("+0.3826834323650897717284599840303988667613445624856270414338006356"u8), // cos(π/16 * 26)
    bigFloat("+0.5555702330196022247428308139485328743749371907548040459241535282"u8), // cos(π/16 * 27)
    bigFloat("+0.7071067811865475244008443621048490392848359376884740365883398689"u8), // cos(π/16 * 28)
    bigFloat("+0.8314696123025452370787883776179057567385608119872499634461245902"u8), // cos(π/16 * 29)
    bigFloat("+0.9238795325112867561281831893967882868224166258636424861150977312"u8), // cos(π/16 * 30)
    bigFloat("+0.9807852804032304491261822361342390369739337308933360950029160885"u8) // cos(π/16 * 31)
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

    global::go.image.jpeg_package.block dst = default!;
    for (nint v = 0; v < 8; v++) {
        for (nint u = 0; u < 8; u++) {
            var sum = 0.0D;
            for (nint y = 0; y < 8; y++) {
                for (nint x = 0; x < 8; x++) {
                    sum += alpha(u) * alpha(v) * (float64)(b[8 * y + x] - 128) * cosines[((2 * x + 1) * u) % 32] * cosines[((2 * y + 1) * v) % 32];
                }
            }
            dst[8 * v + u] = (int32)math.Round(sum);
        }
    }
    b = dst.Clone();
}

// slowerFDCT is slowFDCT but using big.Floats to validate slowFDCT.
internal static void slowerFDCT(ж<global::go.image.jpeg_package.block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    global::go.image.jpeg_package.block dst = default!;
    for (nint v = 0; v < 8; v++) {
        for (nint u = 0; u < 8; u++) {
            var sum = big.NewFloat(0D);
            for (nint y = 0; y < 8; y++) {
                for (nint x = 0; x < 8; x++) {
                    var f = big.NewFloat((float64)(b[8 * y + x] - 128));
                    f = @new<big.Float>().Mul(f, bigAlpha(u));
                    f = @new<big.Float>().Mul(f, bigAlpha(v));
                    f = @new<big.Float>().Mul(f, bigCosines[((2 * x + 1) * u) % 32]);
                    f = @new<big.Float>().Mul(f, bigCosines[((2 * y + 1) * v) % 32]);
                    sum = @new<big.Float>().Add(sum, f);
                }
            }
            // Int64 truncates toward zero, so add ±0.5
            // as needed to round
            if (sum.Sign() > 0){
                sum = @new<big.Float>().Add(sum, big.NewFloat(+0.5D));
            } else {
                sum = @new<big.Float>().Add(sum, big.NewFloat(-0.5D));
            }
            var (i, _) = sum.Int64();
            dst[8 * v + u] = (int32)i;
        }
    }
    b = dst.Clone();
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

    global::go.image.jpeg_package.block dst = default!;
    for (nint y = 0; y < 8; y++) {
        for (nint x = 0; x < 8; x++) {
            var sum = 0.0D;
            for (nint v = 0; v < 8; v++) {
                for (nint u = 0; u < 8; u++) {
                    sum += alpha(u) * alpha(v) * (float64)b[8 * v + u] * cosines[((2 * x + 1) * u) % 32] * cosines[((2 * y + 1) * v) % 32];
                }
            }
            dst[8 * y + x] = (int32)math.Round(sum / 8D);
        }
    }
    b = dst.Clone();
}

// slowerIDCT is slowIDCT but using big.Floats to validate slowIDCT.
internal static void slowerIDCT(ж<global::go.image.jpeg_package.block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    global::go.image.jpeg_package.block dst = default!;
    for (nint y = 0; y < 8; y++) {
        for (nint x = 0; x < 8; x++) {
            var sum = big.NewFloat(0D);
            for (nint v = 0; v < 8; v++) {
                for (nint u = 0; u < 8; u++) {
                    var f = big.NewFloat((float64)b[8 * v + u]);
                    f = @new<big.Float>().Mul(f, bigAlpha(u));
                    f = @new<big.Float>().Mul(f, bigAlpha(v));
                    f = @new<big.Float>().Mul(f, bigCosines[((2 * x + 1) * u) % 32]);
                    f = @new<big.Float>().Mul(f, bigCosines[((2 * y + 1) * v) % 32]);
                    f = @new<big.Float>().Quo(f, big.NewFloat(8D));
                    sum = @new<big.Float>().Add(sum, f);
                }
            }
            // Int64 truncates toward zero, so add ±0.5
            // as needed to round
            if (sum.Sign() > 0){
                sum = @new<big.Float>().Add(sum, big.NewFloat(+0.5D));
            } else {
                sum = @new<big.Float>().Add(sum, big.NewFloat(-0.5D));
            }
            var (i, _) = sum.Int64();
            dst[8 * y + x] = (int32)i;
        }
    }
    b = dst.Clone();
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
