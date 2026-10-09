// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using math = math_package;
using static go.math.rand_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using iotest = go.testing.iotest_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using go.testing;
using rand = go.math.rand_package;
using static go.math.rand_internal_test_package;

partial class rand_test_package {

internal static UntypedInt numTestSamples => 10000;

internal static (float64, array<uint32>, array<float32>, array<float32>) tupleᴛ1ʗ = rand_internal_test_package.GetNormalDistributionParameters();
internal static float64 rn = tupleᴛ1ʗ.Item1;
internal static array<uint32> kn = tupleᴛ1ʗ.Item2;
internal static array<float32> wn = tupleᴛ1ʗ.Item3;
internal static array<float32> fn = tupleᴛ1ʗ.Item4;

internal static (float64, array<uint32>, array<float32>, array<float32>) tupleᴛ2ʗ = rand_internal_test_package.GetExponentialDistributionParameters();
internal static float64 re = tupleᴛ2ʗ.Item1;
internal static array<uint32> ke = tupleᴛ2ʗ.Item2;
internal static array<float32> we = tupleᴛ2ʗ.Item3;
internal static array<float32> fe = tupleᴛ2ʗ.Item4;

partial struct statsResults {
    internal float64 mean;
    internal float64 stddev;
    internal float64 closeEnough;
    internal float64 maxError;
}

internal static bool nearEqual(float64 a, float64 b, float64 closeEnough, float64 maxError) {
    var absDiff = math.Abs(a - b);
    if (absDiff < closeEnough) {
        // Necessary when one value is zero and one value is close to zero.
        return true;
    }
    return absDiff / max(math.Abs(a), math.Abs(b)) < maxError;
}

internal static slice<int64> testSeeds = new int64[]{1, 1754801282, 1698661970, 1550503961}.slice();

// checkSimilarDistribution returns success if the mean and stddev of the
// two statsResults are similar.
internal static error checkSimilarDistribution(this ref statsResults sr, ж<statsResults> Ꮡexpected) {
    ref var expected = ref Ꮡexpected.DerefOrNull();

    if (!nearEqual(sr.mean, expected.mean, expected.closeEnough, expected.maxError)) {
        @string s = fmt.Sprintf("mean %v != %v (allowed error %v, %v)"u8, sr.mean, expected.mean, expected.closeEnough, expected.maxError);
        fmt.Println(s);
        return errors.New(s);
    }
    if (!nearEqual(sr.stddev, expected.stddev, expected.closeEnough, expected.maxError)) {
        @string s = fmt.Sprintf("stddev %v != %v (allowed error %v, %v)"u8, sr.stddev, expected.stddev, expected.closeEnough, expected.maxError);
        fmt.Println(s);
        return errors.New(s);
    }
    return default!;
}

internal static ж<statsResults> getStatsResults(slice<float64> samples) {
    var res = @new<statsResults>();
    float64 sum = default!;
    float64 squaresum = default!;
    foreach (var (_, s) in samples) {
        sum += s;
        squaresum += s * s;
    }
    res.Value.mean = sum / (float64)len(samples);
    res.Value.stddev = math.Sqrt(squaresum / (float64)len(samples) - (~res).mean * (~res).mean);
    return res;
}

internal static void checkSampleDistribution(ж<testing.T> Ꮡt, slice<float64> samples, ж<statsResults> Ꮡexpected) {
    Ꮡt.Helper();
    var actual = getStatsResults(samples);
    var err = actual.checkSimilarDistribution(Ꮡexpected);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

internal static void checkSampleSliceDistributions(ж<testing.T> Ꮡt, slice<float64> samples, nint nslices, ж<statsResults> Ꮡexpected) {
    Ꮡt.Helper();
    nint chunk = len(samples) / nslices;
    for (nint i = 0; i < nslices; i++) {
        nint low = i * chunk;
        nint high = default!;
        if (i == nslices - 1){
            high = len(samples) - 1;
        } else {
            high = (i + 1) * chunk;
        }
        checkSampleDistribution(Ꮡt, samples.slice(low, high), Ꮡexpected);
    }
}

//
// Normal distribution tests
//
internal static slice<float64> generateNormalSamples(nint nsamples, float64 mean, float64 stddev, int64 seed) {
    var r = New(NewSource(seed));
    var samples = new slice<float64>(nsamples);
    foreach (var (i, _) in samples) {
        samples[i] = r.NormFloat64() * stddev + mean;
    }
    return samples;
}

internal static void testNormalDistribution(ж<testing.T> Ꮡt, nint nsamples, float64 mean, float64 stddev, int64 seed) {
    //fmt.Printf("testing nsamples=%v mean=%v stddev=%v seed=%v\n", nsamples, mean, stddev, seed);
    var samples = generateNormalSamples(nsamples, mean, stddev, seed);
    var errorScale = max(1.0D, stddev); // Error scales with stddev
    var expected = Ꮡ(new statsResults(mean, stddev, 0.10D * errorScale, 0.08D * errorScale));
    // Make sure that the entire set matches the expected distribution.
    checkSampleDistribution(Ꮡt, samples, expected);
    // Make sure that each half of the set matches the expected distribution.
    checkSampleSliceDistributions(Ꮡt, samples, 2, expected);
    // Make sure that each 7th of the set matches the expected distribution.
    checkSampleSliceDistributions(Ꮡt, samples, 7, expected);
}

// Actual tests
public static void TestStandardNormalValues(ж<testing.T> Ꮡt) {
    foreach (var (_, seed) in testSeeds) {
        testNormalDistribution(Ꮡt, numTestSamples, 0D, 1D, seed);
    }
}

public static void TestNonStandardNormalValues(ж<testing.T> Ꮡt) {
    var sdmax = 1000.0D;
    var mmax = 1000.0D;
    if (testing.Short()) {
        sdmax = 5D;
        mmax = 5D;
    }
    for (var sd = 0.5D; sd < sdmax; sd *= 2D) {
        for (var m = 0.5D; m < mmax; m *= 2D) {
            foreach (var (_, seed) in testSeeds) {
                testNormalDistribution(Ꮡt, numTestSamples, m, sd, seed);
                if (testing.Short()) {
                    break;
                }
            }
        }
    }
}

//
// Exponential distribution tests
//
internal static slice<float64> generateExponentialSamples(nint nsamples, float64 rate, int64 seed) {
    var r = New(NewSource(seed));
    var samples = new slice<float64>(nsamples);
    foreach (var (i, _) in samples) {
        samples[i] = r.ExpFloat64() / rate;
    }
    return samples;
}

internal static void testExponentialDistribution(ж<testing.T> Ꮡt, nint nsamples, float64 rate, int64 seed) {
    //fmt.Printf("testing nsamples=%v rate=%v seed=%v\n", nsamples, rate, seed);
    ref var mean = ref heap<float64>(out var Ꮡmean);
    mean = 1D / rate;
    ref var stddev = ref heap<float64>(out var Ꮡstddev);
    stddev = mean;
    var samples = generateExponentialSamples(nsamples, rate, seed);
    var errorScale = max(1.0D, 1D / rate); // Error scales with the inverse of the rate
    var expected = Ꮡ(new statsResults(mean, stddev, 0.10D * errorScale, 0.20D * errorScale));
    // Make sure that the entire set matches the expected distribution.
    checkSampleDistribution(Ꮡt, samples, expected);
    // Make sure that each half of the set matches the expected distribution.
    checkSampleSliceDistributions(Ꮡt, samples, 2, expected);
    // Make sure that each 7th of the set matches the expected distribution.
    checkSampleSliceDistributions(Ꮡt, samples, 7, expected);
}

// Actual tests
public static void TestStandardExponentialValues(ж<testing.T> Ꮡt) {
    foreach (var (_, seed) in testSeeds) {
        testExponentialDistribution(Ꮡt, numTestSamples, 1D, seed);
    }
}

public static void TestNonStandardExponentialValues(ж<testing.T> Ꮡt) {
    for (var rate = 0.05D; rate < 10D; rate *= 2D) {
        foreach (var (_, seed) in testSeeds) {
            testExponentialDistribution(Ꮡt, numTestSamples, rate, seed);
            if (testing.Short()) {
                break;
            }
        }
    }
}

//
// Table generation tests
//
internal static (slice<uint32> testKn, slice<float32> testWn, slice<float32> testFn) initNorm() {
    slice<uint32> testKn = default!;
    slice<float32> testWn = default!;
    slice<float32> testFn = default!;

    const float64 m1 = /* 1 << 31 */ 2147483648;
    float64 dn = rn;
    float64 tn = dn;
    float64 vn = 9.91256303526217e-3D;
    testKn = new slice<uint32>(128);
    testWn = new slice<float32>(128);
    testFn = new slice<float32>(128);
    var q = vn / math.Exp(-0.5D * dn * dn);
    testKn[0] = (uint32)((dn / q) * m1);
    testKn[1] = 0;
    testWn[0] = (float32)(q / m1);
    testWn[127] = (float32)(dn / m1);
    testFn[0] = 1.0F;
    testFn[127] = (float32)math.Exp(-0.5D * dn * dn);
    for (nint i = 126; i >= 1; i--) {
        dn = math.Sqrt(-2.0D * math.Log(vn / dn + math.Exp(-0.5D * dn * dn)));
        testKn[i + 1] = (uint32)((dn / tn) * m1);
        tn = dn;
        testFn[i] = (float32)math.Exp(-0.5D * dn * dn);
        testWn[i] = (float32)(dn / m1);
    }
    return (testKn, testWn, testFn);
}

internal static (slice<uint32> testKe, slice<float32> testWe, slice<float32> testFe) initExp() {
    slice<uint32> testKe = default!;
    slice<float32> testWe = default!;
    slice<float32> testFe = default!;

    const float64 m2 = /* 1 << 32 */ 4294967296;
    float64 de = re;
    float64 te = de;
    float64 ve = 3.9496598225815571993e-3D;
    testKe = new slice<uint32>(256);
    testWe = new slice<float32>(256);
    testFe = new slice<float32>(256);
    var q = ve / math.Exp(-de);
    testKe[0] = (uint32)((de / q) * m2);
    testKe[1] = 0;
    testWe[0] = (float32)(q / m2);
    testWe[255] = (float32)(de / m2);
    testFe[0] = 1.0F;
    testFe[255] = (float32)math.Exp(-de);
    for (nint i = 254; i >= 1; i--) {
        de = -math.Log(ve / de + math.Exp(-de));
        testKe[i + 1] = (uint32)((de / te) * m2);
        te = de;
        testFe[i] = (float32)math.Exp(-de);
        testWe[i] = (float32)(de / m2);
    }
    return (testKe, testWe, testFe);
}

// compareUint32Slices returns the first index where the two slices
// disagree, or <0 if the lengths are the same and all elements
// are identical.
internal static nint compareUint32Slices(slice<uint32> s1, slice<uint32> s2) {
    if (len(s1) != len(s2)) {
        if (len(s1) > len(s2)) {
            return len(s2) + 1;
        }
        return len(s1) + 1;
    }
    foreach (var (i, _) in s1) {
        if (s1[i] != s2[i]) {
            return i;
        }
    }
    return -1;
}

// compareFloat32Slices returns the first index where the two slices
// disagree, or <0 if the lengths are the same and all elements
// are identical.
internal static nint compareFloat32Slices(slice<float32> s1, slice<float32> s2) {
    if (len(s1) != len(s2)) {
        if (len(s1) > len(s2)) {
            return len(s2) + 1;
        }
        return len(s1) + 1;
    }
    foreach (var (i, _) in s1) {
        if (!nearEqual((float64)s1[i], (float64)s2[i], 0D, 1e-7D)) {
            return i;
        }
    }
    return -1;
}

public static void TestNormTables(ж<testing.T> Ꮡt) {
    var (testKn, testWn, testFn) = initNorm();
    {
        nint i = compareUint32Slices(kn[0..], testKn); if (i >= 0) {
            Ꮡt.Errorf("kn disagrees at index %v; %v != %v"u8, i, kn[i], testKn[i]);
        }
    }
    {
        nint i = compareFloat32Slices(wn[0..], testWn); if (i >= 0) {
            Ꮡt.Errorf("wn disagrees at index %v; %v != %v"u8, i, wn[i], testWn[i]);
        }
    }
    {
        nint i = compareFloat32Slices(fn[0..], testFn); if (i >= 0) {
            Ꮡt.Errorf("fn disagrees at index %v; %v != %v"u8, i, fn[i], testFn[i]);
        }
    }
}

public static void TestExpTables(ж<testing.T> Ꮡt) {
    var (testKe, testWe, testFe) = initExp();
    {
        nint i = compareUint32Slices(ke[0..], testKe); if (i >= 0) {
            Ꮡt.Errorf("ke disagrees at index %v; %v != %v"u8, i, ke[i], testKe[i]);
        }
    }
    {
        nint i = compareFloat32Slices(we[0..], testWe); if (i >= 0) {
            Ꮡt.Errorf("we disagrees at index %v; %v != %v"u8, i, we[i], testWe[i]);
        }
    }
    {
        nint i = compareFloat32Slices(fe[0..], testFe); if (i >= 0) {
            Ꮡt.Errorf("fe disagrees at index %v; %v != %v"u8, i, fe[i], testFe[i]);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goarmˢ = "GOARM"u8;
internal static readonly @string softfloatˢ = ",softfloat"u8;

internal static bool hasSlowFloatingPoint() {
    var exprᴛ1 = runtime.GOARCH;
    if (exprᴛ1 == "arm"u8) {
        return os.Getenv(goarmˢ) == "5"u8 || strings.HasSuffix(os.Getenv(goarmˢ), softfloatˢ);
    }
    if (exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8) {
        return true;
    }

    // Be conservative and assume that all mips boards
    // have emulated floating point.
    // TODO: detect what it actually has.
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object float32ShouldBeInRange01ˢ = (@string)"Float32() should be in range [0,1). ct:"u8;

public static void TestFloat32(ж<testing.T> Ꮡt) {
    // For issue 6721, the problem came after 7533753 calls, so check 10e6.
    nint num = (nint)10000000;
    // But do the full amount only on builders (not locally).
    // But ARM5 floating point emulation is slow (Issue 10749), so
    // do less for that builder:
    if (testing.Short() && (testenv.Builder() == ""u8 || hasSlowFloatingPoint())) {
        num /= 100; // 1.72 seconds instead of 172 seconds
    }
    var r = New(NewSource(1));
    for (nint ct = 0; ct < num; ct++) {
        var f = r.Float32();
        if (f >= 1F) {
            Ꮡt.Fatal(float32ShouldBeInRange01ˢ, ct, (@string)"f:"u8, f);
        }
    }
}

internal static void testReadUniformity(ж<testing.T> Ꮡt, nint n, int64 seed) {
    var r = New(NewSource(seed));
    var buf = new slice<byte>(n);
    var (nRead, err) = r.Read(buf);
    if (err != default!) {
        Ꮡt.Errorf("Read err %v"u8, err);
    }
    if (nRead != n) {
        Ꮡt.Errorf("Read returned unexpected n; %d != %d"u8, nRead, n);
    }
    // Expect a uniform distribution of byte values, which lie in [0, 255].
    ref var mean = ref heap(new float64(), out var Ꮡmean);
    mean = 255.0D / 2D;
    
    ref var stddev = ref heap(new float64(), out var Ꮡstddev);

    stddev = 256.0D / math.Sqrt(12.0D);
    
    float64 errorScale = stddev / math.Sqrt((float64)n);
    var expected = Ꮡ(new statsResults(mean, stddev, 0.10D * errorScale, 0.08D * errorScale));
    // Cast bytes as floats to use the common distribution-validity checks.
    var samples = new slice<float64>(n);
    foreach (var (i, val) in buf) {
        samples[i] = (float64)val;
    }
    // Make sure that the entire set matches the expected distribution.
    checkSampleDistribution(Ꮡt, samples, expected);
}

public static void TestReadUniformity(ж<testing.T> Ꮡt) {
    var testBufferSizes = new nint[]{
        2, 4, 7, 64, 1024, (1 << (int)(16)), (1 << (int)(20))
    }.slice();
    foreach (var (_, seed) in testSeeds) {
        foreach (var (_, n) in testBufferSizes) {
            testReadUniformity(Ꮡt, n, seed);
        }
    }
}

public static void TestReadEmpty(ж<testing.T> Ꮡt) {
    var r = New(NewSource(1));
    var buf = new slice<byte>(0);
    var (n, err) = r.Read(buf);
    if (err != default!) {
        Ꮡt.Errorf("Read err into empty buffer; %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("Read into empty buffer returned unexpected n of %d"u8, n);
    }
}

public static void TestReadByOneByte(ж<testing.T> Ꮡt) {
    var r = New(NewSource(1));
    var b1 = new slice<byte>(100);
    var (_, err) = io.ReadFull(iotest.OneByteReader(new rand_test_package.rand_RandжReader(r)), b1);
    if (err != default!) {
        Ꮡt.Errorf("read by one byte: %v"u8, err);
    }
    r = New(NewSource(1));
    var b2 = new slice<byte>(100);
    (_, err) = r.Read(b2);
    if (err != default!) {
        Ꮡt.Errorf("read: %v"u8, err);
    }
    if (!bytes.Equal(b1, b2)) {
        Ꮡt.Errorf("read by one byte vs single read:\n%x\n%x"u8, b1, b2);
    }
}

public static void TestReadSeedReset(ж<testing.T> Ꮡt) {
    var r = New(NewSource(42));
    var b1 = new slice<byte>(128);
    var (_, err) = r.Read(b1);
    if (err != default!) {
        Ꮡt.Errorf("read: %v"u8, err);
    }
    r.Seed(42);
    var b2 = new slice<byte>(128);
    (_, err) = r.Read(b2);
    if (err != default!) {
        Ꮡt.Errorf("read: %v"u8, err);
    }
    if (!bytes.Equal(b1, b2)) {
        Ꮡt.Errorf("mismatch after re-seed:\n%x\n%x"u8, b1, b2);
    }
}

public static void TestShuffleSmall(ж<testing.T> Ꮡt) {
    // Check that Shuffle allows n=0 and n=1, but that swap is never called for them.
    var r = New(NewSource(1));
    for (nint nᴛ1 = 0; nᴛ1 <= 1; nᴛ1++) {
        var n = nᴛ1;
        r.Shuffle(n, (nint i, nint j) => {
            Ꮡt.Fatalf("swap called, n=%d i=%d j=%d"u8, n, i, j);
        });
    }
}

// encodePerm converts from a permuted slice of length n, such as Perm generates, to an int in [0, n!).
// See https://en.wikipedia.org/wiki/Lehmer_code.
// encodePerm modifies the input slice.
internal static nint encodePerm(slice<nint> s) {
    // Convert to Lehmer code.
    foreach (var (i, x) in s) {
        var r = s.slice(i + 1);
        foreach (var (j, y) in r) {
            if (y > x) {
                r[j]--;
            }
        }
    }
    // Convert to int in [0, n!).
    nint m = 0;
    nint fact = 1;
    for (nint i = len(s) - 1; i >= 0; i--) {
        m += s[i] * fact;
        fact *= len(s) - i;
    }
    return m;
}

internal partial struct TestUniformFactorial_tests /*dyn*/ {
    internal @string name;
    internal Func<nint> fn;
}

// TestUniformFactorial tests several ways of generating a uniform value in [0, n!).
public static void TestUniformFactorial(ж<testing.T> Ꮡt) {
    var r = New(NewSource(testSeeds[0]));
    nint top = 6;
    if (testing.Short()) {
        top = 3;
    }
    for (nint nᴛ1 = 3; nᴛ1 <= top; nᴛ1++) {
        var n = nᴛ1;
        var rʗ1 = r;
        Ꮡt.Run(fmt.Sprintf("n=%d"u8, n), (ж<testing.T> tΔ1) => {
            // Calculate n!.
            nint nfact = 1;
            for (nint i = 2; i <= n; i++) {
                nfact *= i;
            }
            // Test a few different ways to generate a uniform distribution.
            var p = new slice<nint>(n); // re-usable slice for Shuffle generator
                var rʗ2 = rʗ1;

                var rʗ3 = rʗ1;

                var rʗ4 = rʗ1;

                var pʗ1 = p;
                var rʗ5 = rʗ1;
            var tests = new TestUniformFactorial_tests[]{
                new(name: "Int31n"u8, fn: () => (nint)rʗ2.Int31n((int32)nfact)),
                new(name: "int31n"u8, fn: () => (nint)rand_internal_test_package.Int31nForTest(rʗ3, (int32)nfact)),
                new(name: "Perm"u8, fn: () => encodePerm(rʗ4.Perm(n))),
                new(name: "Shuffle"u8, fn: () => {
                    // Generate permutation using Shuffle.
                    foreach (var (i, _) in pʗ1) {
                        pʗ1[i] = i;
                    }
                    var pʗ2 = pʗ1;
                    rʗ5.Shuffle(n, (nint i, nint j) => {
                        (pʗ2[i], pʗ2[j]) = (pʗ2[j], pʗ2[i]);
                    });
                    return encodePerm(pʗ1);
                })
            }.array();
            foreach (var (_, vᴛ1) in tests.ΔRangeSnapshot()) {
                ref var test = ref heap(new TestUniformFactorial_tests(), out var Ꮡtest);
                test = vᴛ1;

                var testʗ1 = test;
                tΔ1.Run(test.name, (ж<testing.T> tΔ2) => {
                    // Gather chi-squared values and check that they follow
                    // the expected normal distribution given n!-1 degrees of freedom.
                    // See https://en.wikipedia.org/wiki/Pearson%27s_chi-squared_test and
                    // https://www.johndcook.com/Beautiful_Testing_ch10.pdf.
                    nint nsamples = 10 * nfact;
                    if (nsamples < 200) {
                        nsamples = 200;
                    }
                    var samples = new slice<float64>(nsamples);
                    foreach (var (i, _) in samples) {
                        // Generate some uniformly distributed values and count their occurrences.
                        UntypedInt iters = 1000;
                        var counts = new slice<nint>(nfact);
                        for (nint iΔ1 = 0; iΔ1 < iters; iΔ1++) {
                            counts[testʗ1.fn()]++;
                        }
                        // Calculate chi-squared and add to samples.
                        var want = (float64)iters / (float64)nfact;
                        float64 χ2 = default!;
                        foreach (var (_, have) in counts) {
                            var err = (float64)have - want;
                            χ2 += err * err;
                        }
                        χ2 /= want;
                        samples[i] = χ2;
                    }
                    // Check that our samples approximate the appropriate normal distribution.
                    ref var dof = ref heap<float64>(out var Ꮡdof);
                    dof = (float64)(nfact - 1);
                    var expected = Ꮡ(new statsResults(mean: dof, stddev: math.Sqrt(2D * dof)));
                    var errorScale = max(1.0D, (~expected).stddev);
                    expected.Value.closeEnough = 0.10D * errorScale;
                    expected.Value.maxError = 0.08D; // TODO: What is the right value here? See issue 21211.
                    checkSampleDistribution(tΔ2, samples, expected);
                });
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string randseednop0ˢ = "randseednop=0"u8;
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly object globalSeedShouldTakeˢ = (@string)"global Seed should take effect"u8;
internal static readonly @string randseednop1ˢ = "randseednop=1"u8;
internal static readonly object globalSeedShouldBeANoOpˢ = (@string)"global Seed should be a no-op"u8;
internal static readonly @string godebugUnsetˢ = "GODEBUG unset"u8;
internal static readonly object globalSeedShouldDefaultˢ = (@string)"global Seed should default to being a no-op"u8;

public static void TestSeedNop(ж<testing.T> Ꮡt) {
    // If the global Seed takes effect, then resetting it to a certain value
    // should provide predictable output to functions using it.
    Ꮡt.Run(randseednop0ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Setenv(godebugˢ, randseednop0ˢ);
        Seed(1);
        var before = Int63();
        Seed(1);
        var after = Int63();
        if (before != after) {
            tΔ1.Fatal(globalSeedShouldTakeˢ);
        }
    });
    // If calls to the global Seed are no-op then functions using it should
    // provide different output, even if it was reset to the same value.
    Ꮡt.Run(randseednop1ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Setenv(godebugˢ, randseednop1ˢ);
        Seed(1);
        var before = Int63();
        Seed(1);
        var after = Int63();
        if (before == after) {
            tΔ2.Fatal(globalSeedShouldBeANoOpˢ);
        }
    });
    Ꮡt.Run(godebugUnsetˢ, (ж<testing.T> tΔ3) => {
        Seed(1);
        var before = Int63();
        Seed(1);
        var after = Int63();
        if (before == after) {
            tΔ3.Fatal(globalSeedShouldDefaultˢ);
        }
    });
}

// Benchmarks
public static void BenchmarkInt63Threadsafe(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint n = b.N; n > 0; n--) {
        Int63();
    }
}

public static void BenchmarkInt63ThreadsafeParallel(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            Int63();
        }
    });
}

public static void BenchmarkInt63Unthreadsafe(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Int63();
    }
}

public static void BenchmarkIntn1000(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Intn(1000);
    }
}

public static void BenchmarkInt63n1000(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Int63n(1000);
    }
}

public static void BenchmarkInt31n1000(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Int31n(1000);
    }
}

public static void BenchmarkFloat32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Float32();
    }
}

public static void BenchmarkFloat64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Float64();
    }
}

public static void BenchmarkPerm3(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Perm(3);
    }
}

public static void BenchmarkPerm30(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Perm(30);
    }
}

public static void BenchmarkPerm30ViaShuffle(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        var p = new slice<nint>(30);
        foreach (var (i, _) in p) {
            p[i] = i;
        }
        var pʗ1 = p;
        r.Shuffle(30, (nint i, nint j) => {
            (pʗ1[i], pʗ1[j]) = (pʗ1[j], pʗ1[i]);
        });
    }
}

// BenchmarkShuffleOverhead uses a minimal swap function
// to measure just the shuffling overhead.
public static void BenchmarkShuffleOverhead(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    for (nint n = b.N; n > 0; n--) {
        r.Shuffle(52, (nint i, nint j) => {
            if (i < 0 || i >= 52 || j < 0 || j >= 52) {
                Ꮡb.Fatalf("bad swap(%d, %d)"u8, i, j);
            }
        });
    }
}

public static void BenchmarkRead3(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    var buf = new slice<byte>(3);
    b.ResetTimer();
    for (nint n = b.N; n > 0; n--) {
        r.Read(buf);
    }
}

public static void BenchmarkRead64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    var buf = new slice<byte>(64);
    b.ResetTimer();
    for (nint n = b.N; n > 0; n--) {
        r.Read(buf);
    }
}

public static void BenchmarkRead1000(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = New(NewSource(1));
    var buf = new slice<byte>(1000);
    b.ResetTimer();
    for (nint n = b.N; n > 0; n--) {
        r.Read(buf);
    }
}

public static partial void BenchmarkConcurrent(ж<testing.B> Ꮡb) {
    const nint goroutines = 4;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(goroutines);
    for (nint i = 0; i < goroutines; i++) {
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint n = Ꮡb.Value.N; n > 0; n--) {
                    Int63();
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

} // end rand_test_package
