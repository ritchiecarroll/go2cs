// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto.@internal.fips140test;

using bytes = bytes_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using entropy = go.crypto.@internal.entropy.v1_0_0.entropy_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using rand = go.crypto.rand_package;
using sha256 = go.crypto.sha256_package;
using sha512 = go.crypto.sha512_package;
using hex = encoding.hex_package;
using flag = flag_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using fs = go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.io;
using go.os;
using hash = hash_package;
using io = io_package;
using path;

partial class fipstest_internal_test_package {

internal static ж<@string> flagEntropySamples = flag.String("entropy-samples"u8, ""u8, "store entropy samples with the provided `suffix`"u8);

internal static ж<bool> flagNISTSP80090B = flag.Bool("nist-sp800-90b"u8, false, "run NIST SP 800-90B tests (requires docker)"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dockerˢ = "docker"u8;
internal static readonly @string imageˢ = "image"u8;
internal static readonly @string inspectˢ = "inspect"u8;
internal static readonly @string nistSp80090bˢ = "nist-sp800-90b"u8;
internal static readonly @string dockerfileSp80090bˢ = "Dockerfile.SP800-90B_EntropyAssessment"u8;
internal static readonly @string varEmptyˢ = "/var/empty"u8;
internal static readonly @string runˢ = "run"u8;
internal static readonly @string eaNonIidˢ = "ea_non_iid"u8;
internal static readonly @string eaRestartˢ = "ea_restart"u8;

public static void TestEntropySamples(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    cryptotest.MustSupportFIPS140(Ꮡt);
    var now = time.Now().UTC();

    nint seqSampleCount = 1_000_000;
    if (flagEntropySamples.Value != ""u8) {
        // The lab requested 300 million samples for a new heuristic procedure.
        seqSampleCount = 300_000_000;
    }
    var seqSamples = new slice<uint8>(seqSampleCount);
    samplesOrTryAgain(Ꮡt, seqSamples);
    @string seqSamplesName = fmt.Sprintf("entropy_samples_sequential_%s_%s_%s_%s_%s.bin"u8, entropy.Version(),
        runtime.GOOS, runtime.GOARCH, flagEntropySamples.Value, now.Format("20060102T150405Z"u8));
    if (flagEntropySamples.Value != ""u8) {
        {
            var err = os.WriteFile(seqSamplesName, seqSamples, 420); if (err != default!) {
                Ꮡt.Fatalf("failed to write samples to %q: %v"u8, seqSamplesName, err);
            }
        }
        Ꮡt.Logf("wrote %s"u8, seqSamplesName);
    }

    array<array<uint8>> restartSamples = new(1000, () => new(1000));
    foreach (var i in range(restartSamples)) {
        array<uint8> samples = new(1024);
        samplesOrTryAgain(Ꮡt, samples[..]);
        copy(restartSamples[i][..], samples[..]);
    }
    @string restartSamplesName = fmt.Sprintf("entropy_samples_restart_%s_%s_%s_%s_%s.bin"u8, entropy.Version(),
        runtime.GOOS, runtime.GOARCH, flagEntropySamples.Value, now.Format("20060102T150405Z"u8));
    if (flagEntropySamples.Value != ""u8) {
        var (f, err) = os.Create(restartSamplesName);
        if (err != default!) {
            Ꮡt.Fatalf("failed to create %q: %v"u8, restartSamplesName, err);
        }
        foreach (var i in range(restartSamples)) {
            {
                var (_, errΔ1) = f.Write(restartSamples[i][..]); if (errΔ1 != default!) {
                    Ꮡt.Fatalf("failed to write samples to %q: %v"u8, restartSamplesName, errΔ1);
                }
            }
        }
        {
            var errΔ2 = f.Close(); if (errΔ2 != default!) {
                Ꮡt.Fatalf("failed to close %q: %v"u8, restartSamplesName, errΔ2);
            }
        }
        Ꮡt.Logf("wrote %s"u8, restartSamplesName);
    }

    if (flagNISTSP80090B.Value) {
        if (flagEntropySamples.Value == ""u8) {
            Ꮡt.Fatalf("-nist-sp800-90b requires -entropy-samples to be set too"u8);
        }

        // Check if the nist-sp800-90b docker image is already present,
        // and build it otherwise.
        {
            var errΔ3 = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt),
                dockerˢ, imageˢ, inspectˢ, nistSp80090bˢ).Run(); if (errΔ3 != default!) {
                Ꮡt.Logf("building nist-sp800-90b docker image"u8);
                @string dockerfile = filepath.Join(Ꮡt.TempDir(), dockerfileSp80090bˢ);
                {
                    var errΔ4 = os.WriteFile(dockerfile, slice<byte>(NISTSP80090BDockerfile), 420); if (errΔ4 != default!) {
                        Ꮡt.Fatalf("failed to write Dockerfile: %v"u8, errΔ4);
                    }
                }
                var (outΔ1, errΔ5) = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt),
                    dockerˢ, buildˢ, "-t", nistSp80090bˢ, "-f", dockerfile, varEmptyˢ).CombinedOutput();
                if (errΔ5 != default!) {
                    Ꮡt.Fatalf("failed to build nist-sp800-90b docker image: %v\n%s"u8, errΔ5, outΔ1);
                }
            }
        }

        var (pwd, err) = os.Getwd();
        if (err != default!) {
            Ꮡt.Fatalf("failed to get current working directory: %v"u8, err);
        }
        Ꮡt.Logf("running ea_non_iid analysis"u8);
        (var @out, err) = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt),
            dockerˢ, runˢ, "--rm", "-v", fmt.Sprintf("%s:%s"u8, pwd, pwd), "-w", pwd,
            nistSp80090bˢ, eaNonIidˢ, seqSamplesName, "8").CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("ea_non_iid failed: %v\n%s"u8, err, @out);
        }
        Ꮡt.Logf("\n%s"u8, @out);

        @string H_I = (@string)@out;
        H_I = strings.TrimSpace(H_I.slice(strings.LastIndexByte(H_I, (rune)' ') + 1));
        Ꮡt.Logf("running ea_restart analysis with H_I = %s"u8, H_I);
        (@out, err) = testenv.Command(new fipstest_internal_test_package.testing_TжTB(Ꮡt),
            dockerˢ, runˢ, "--rm", "-v", fmt.Sprintf("%s:%s"u8, pwd, pwd), "-w", pwd,
            nistSp80090bˢ, eaRestartˢ, restartSamplesName, "8", H_I).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("ea_restart failed: %v\n%s"u8, err, @out);
        }
        Ꮡt.Logf("\n%s"u8, @out);
    }
}

public static @string NISTSP80090BDockerfile = """

FROM ubuntu:24.04
RUN apt-get update && apt-get install -y build-essential git \
    libbz2-dev libdivsufsort-dev libjsoncpp-dev libgmp-dev libmpfr-dev libssl-dev \
    && rm -rf /var/lib/apt/lists/*
RUN git clone --depth 1 https://github.com/usnistgov/SP800-90B_EntropyAssessment.git
RUN cd SP800-90B_EntropyAssessment && git checkout 8924f158c97e7b805e0f95247403ad4c44b9cd6f
WORKDIR ./SP800-90B_EntropyAssessment/cpp/
RUN make all
RUN cd selftest && ./selftest
RUN cp ea_non_iid ea_restart /usr/local/bin/

"""u8.Raw();

internal static ж<entropy.ScratchBuffer> Ꮡmemory = new StandardBox<entropy.ScratchBuffer>(default(entropy.ScratchBuffer));
internal static ref entropy.ScratchBuffer memory => ref Ꮡmemory.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object entropySamplesFailed10ˢ = (@string)"entropy.Samples() failed 10 times in a row"u8;

// samplesOrTryAgain calls entropy.Samples up to 10 times until it succeeds.
// Samples has a non-negligible chance of failing the health tests, as required
// by SP 800-90B.
internal static void samplesOrTryAgain(ж<testing.T> Ꮡt, slice<uint8> samples) {
    Ꮡt.Helper();
    foreach (var _ᴛ1 in range(10)) {
        {
            var err = entropy.Samples(samples, Ꮡmemory); if (err != default!) {
                Ꮡt.Logf("entropy.Samples() failed: %v"u8, err);
                continue;
            }
        }
        return;
    }
    Ꮡt.Fatal(entropySamplesFailed10ˢ);
}

public static void TestEntropySHA384(ж<testing.T> Ꮡt) {
    ref var input = ref heap(new array<uint8>(1024), out var Ꮡinput);
    foreach (var i in range(input)) {
        input[i] = (uint8)i;
    }
    var want = sha512.Sum384(input[..]);
    var got = entropy.SHA384(Ꮡinput);
    if (got != want) {
        Ꮡt.Errorf("SHA384() = %x, want %x"u8, got, want);
    }

    foreach (var l in range(1024 * 3 + 1)) {
        var inputΔ1 = new slice<byte>(l);
        rand.Read(inputΔ1);
        var wantΔ1 = sha512.Sum384(inputΔ1);
        var gotΔ1 = entropy.TestingOnlySHA384(inputΔ1);
        if (gotΔ1 != wantΔ1) {
            Ꮡt.Errorf("TestingOnlySHA384(%d bytes) = %x, want %x"u8, l, gotΔ1, wantΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object repetitionCountTestBadˢ = (@string)"RepetitionCountTest(bad) = nil, want error"u8;

public static void TestEntropyRepetitionCountTest(ж<testing.T> Ꮡt) {
    var good = bytes.Repeat(append(bytes.Repeat(new uint8[]{42}.slice(), 40), (byte)1), 100);
    {
        var err = entropy.RepetitionCountTest(good); if (err != default!) {
            Ꮡt.Errorf("RepetitionCountTest(good) = %v, want nil"u8, err);
        }
    }

    var bad = bytes.Repeat(new uint8[]{0}.slice(), 40);
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(new uint8[]{1}.slice(), 40));
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(new uint8[]{42}.slice(), 41));
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(new uint8[]{2}.slice(), 40));
    {
        var err = entropy.RepetitionCountTest(bad); if (err == default!) {
            Ꮡt.Error(repetitionCountTestBadˢ);
        }
    }

    bad = bytes.Repeat(new uint8[]{42}.slice(), 41);
    {
        var err = entropy.RepetitionCountTest(bad); if (err == default!) {
            Ꮡt.Error(repetitionCountTestBadˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object adaptiveProportionTestˢ = (@string)"AdaptiveProportionTest(bad) = nil, want error"u8;

public static void TestEntropyAdaptiveProportionTest(ж<testing.T> Ꮡt) {
    var good = bytes.Repeat(new uint8[]{0}.slice(), 409);
    good = appendꓸꓸꓸ(good, bytes.Repeat(new uint8[]{1}.slice(), 512 - 409));
    good = appendꓸꓸꓸ(good, bytes.Repeat(new uint8[]{0}.slice(), 409));
    {
        var err = entropy.AdaptiveProportionTest(good); if (err != default!) {
            Ꮡt.Errorf("AdaptiveProportionTest(good) = %v, want nil"u8, err);
        }
    }

    // These fall out of the window.
    var bad = bytes.Repeat(new uint8[]{1}.slice(), 100);
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(new uint8[]{1, 2, 3, 4, 5, 6}.slice(), 100));
    // These are in the window.
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(new uint8[]{42}.slice(), 410));
    {
        var err = entropy.AdaptiveProportionTest(bad.slice(0, len(bad) - 1)); if (err != default!) {
            Ꮡt.Errorf("AdaptiveProportionTest(bad[:len(bad)-1]) = %v, want nil"u8, err);
        }
    }
    {
        var err = entropy.AdaptiveProportionTest(bad); if (err == default!) {
            Ꮡt.Error(adaptiveProportionTestˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string entropyV100ˢ = "../entropy/v1.0.0"u8;

public static void TestEntropyUnchanged(ж<testing.T> Ꮡt) {
    testenv.MustHaveSource(new fipstest_internal_test_package.testing_TжTB(Ꮡt));

    var h = sha256.New();
    var root = os.DirFS(entropyV100ˢ);
    {
        var hʗ1 = h;
        var rootʗ1 = root;
        var err = fs.WalkDir(root, "."u8, error (@string path, fs.DirEntry d, error errΔ1) => {
            if (errΔ1 != default!) {
                return errΔ1;
            }
            if (d.IsDir()) {
                return default!;
            }
            (var data, errΔ1) = fs.ReadFile(rootʗ1, path);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            Ꮡt.Logf("Hashing %s (%d bytes)"u8, path, len(data));
            fmt.Fprintf(hʗ1, "%s %d\n"u8, path, len(data));
            hʗ1.Write(data);
            return default!;
        }); if (err != default!) {
            Ꮡt.Fatalf("WalkDir: %v"u8, err);
        }
    }

    // The crypto/internal/entropy/v1.0.0 package is certified as a FIPS 140-3
    // entropy source through the Entropy Source Validation program,
    // independently of the FIPS 140-3 module. It must not change even across
    // FIPS 140-3 module versions, in order to reuse the ESV certificate.
    @string exp = "2541273241ae8aafe55026328354ed3799df1e2fb308b2097833203a42911b53"u8;
    {
        @string got = hex.EncodeToString(h.Sum(default!)); if (got != exp) {
            Ꮡt.Errorf("hash of crypto/internal/entropy/v1.0.0 = %s, want %s"u8, got, exp);
        }
    }
}

public static partial void TestEntropyRace(ж<testing.T> Ꮡt) {
    // Check that concurrent calls to Seed don't trigger the race detector.
    foreach (var _ᴛ1 in range(16)) {
        goǃ(() => {
            (_, _) = entropy.Seed(Ꮡmemory);
        });
    }
    // Same, with the higher-level DRBG.
    foreach (var _ᴛ2 in range(16)) {
        goǃ(() => {
            ref var b = ref heap(new array<byte>(64), out var Ꮡb);
            drbg.Read(b[..]);
        });
    }
}

internal static byte sink;

public static void BenchmarkEntropySeed(ж<testing.B> Ꮡb) {
    while (Ꮡb.Loop()) {
        var (seed, err) = entropy.Seed(Ꮡmemory);
        if (err != default!) {
            Ꮡb.Fatalf("entropy.Seed() failed: %v"u8, err);
        }
        sink ^= (byte)(seed[0]);
    }
}

} // end fipstest_internal_test_package
