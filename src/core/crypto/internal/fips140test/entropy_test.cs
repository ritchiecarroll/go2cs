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

internal static ж<@string> flagEntropySamples = flag.String("entropy-samples", "", "store entropy samples with the provided `suffix`");

internal static ж<bool> flagNISTSP80090B = flag.Bool("nist-sp800-90b", false, "run NIST SP 800-90B tests (requires docker)");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dockerˢ => ref dockerˢᵇ;
private static readonly @string dockerˢᵇ = "docker"u8;
internal static ref readonly @string imageˢ => ref imageˢᵇ;
private static readonly @string imageˢᵇ = "image"u8;
internal static ref readonly @string inspectˢ => ref inspectˢᵇ;
private static readonly @string inspectˢᵇ = "inspect"u8;
internal static ref readonly @string nistSp80090bˢ => ref nistSp80090bˢᵇ;
private static readonly @string nistSp80090bˢᵇ = "nist-sp800-90b"u8;
internal static ref readonly @string dockerfileSp80090bˢ => ref dockerfileSp80090bˢᵇ;
private static readonly @string dockerfileSp80090bˢᵇ = "Dockerfile.SP800-90B_EntropyAssessment"u8;
internal static ref readonly @string varEmptyˢ => ref varEmptyˢᵇ;
private static readonly @string varEmptyˢᵇ = "/var/empty"u8;
internal static ref readonly @string runˢ => ref runˢᵇ;
private static readonly @string runˢᵇ = "run"u8;
internal static ref readonly @string eaNonIidˢ => ref eaNonIidˢᵇ;
private static readonly @string eaNonIidˢᵇ = "ea_non_iid"u8;
internal static ref readonly @string eaRestartˢ => ref eaRestartˢᵇ;
private static readonly @string eaRestartˢᵇ = "ea_restart"u8;

public static void TestEntropySamples(ж<testing.T> t) {
    cryptotest.MustSupportFIPS140(t);
    var now = time.Now().UTC();

    nint seqSampleCount = 1_000_000;
    if (flagEntropySamples.Value != ""u8) {
        // The lab requested 300 million samples for a new heuristic procedure.
        seqSampleCount = 300_000_000;
    }
    var seqSamples = new slice<uint8>(seqSampleCount);
    samplesOrTryAgain(t, seqSamples);
    @string seqSamplesName = fmt.Sprintf("entropy_samples_sequential_%s_%s_%s_%s_%s.bin"u8, entropy.Version(),
        runtime.GOOS, runtime.GOARCH, flagEntropySamples.Value, now.Format("20060102T150405Z"));
    if (flagEntropySamples.Value != ""u8) {
        {
            var err = os.WriteFile(seqSamplesName, seqSamples, 420); if (err != nil) {
                t.Fatalf("failed to write samples to %q: %v", seqSamplesName, err);
            }
        }
        t.Logf("wrote %s", seqSamplesName);
    }

    array<array<uint8>> restartSamples = new(1000, () => new(1000));
    foreach (var i in range(restartSamples)) {
        array<uint8> samples = new(1024);
        samplesOrTryAgain(t, samples[..]);
        copy(restartSamples[i][..], samples[..]);
    }
    @string restartSamplesName = fmt.Sprintf("entropy_samples_restart_%s_%s_%s_%s_%s.bin"u8, entropy.Version(),
        runtime.GOOS, runtime.GOARCH, flagEntropySamples.Value, now.Format("20060102T150405Z"));
    if (flagEntropySamples.Value != ""u8) {
        var (f, err) = os.Create(restartSamplesName);
        if (err != nil) {
            t.Fatalf("failed to create %q: %v", restartSamplesName, err);
        }
        foreach (var i in range(restartSamples)) {
            {
                var (_, errΔ1) = f.Write(restartSamples[i][..]); if (errΔ1 != nil) {
                    t.Fatalf("failed to write samples to %q: %v", restartSamplesName, errΔ1);
                }
            }
        }
        {
            var errΔ2 = f.Close(); if (errΔ2 != nil) {
                t.Fatalf("failed to close %q: %v", restartSamplesName, errΔ2);
            }
        }
        t.Logf("wrote %s", restartSamplesName);
    }

    if (flagNISTSP80090B.Value) {
        if (flagEntropySamples.Value == ""u8) {
            t.Fatalf("-nist-sp800-90b requires -entropy-samples to be set too");
        }

        // Check if the nist-sp800-90b docker image is already present,
        // and build it otherwise.
        {
            var errΔ3 = testenv.Command(new fipstest_internal_test_package.testing_TжTB(t),
                dockerˢ, imageˢ, inspectˢ, nistSp80090bˢ).Run(); if (errΔ3 != nil) {
                t.Logf("building nist-sp800-90b docker image");
                @string dockerfile = filepath.Join(t.TempDir(), dockerfileSp80090bˢ);
                {
                    var errΔ4 = os.WriteFile(dockerfile, slice<byte>(NISTSP80090BDockerfile), 420); if (errΔ4 != nil) {
                        t.Fatalf("failed to write Dockerfile: %v", errΔ4);
                    }
                }
                var (outΔ1, errΔ5) = testenv.Command(new fipstest_internal_test_package.testing_TжTB(t),
                    dockerˢ, buildˢ, "-t", nistSp80090bˢ, "-f", dockerfile, varEmptyˢ).CombinedOutput();
                if (errΔ5 != nil) {
                    t.Fatalf("failed to build nist-sp800-90b docker image: %v\n%s", errΔ5, outΔ1);
                }
            }
        }

        var (pwd, err) = os.Getwd();
        if (err != nil) {
            t.Fatalf("failed to get current working directory: %v", err);
        }
        t.Logf("running ea_non_iid analysis");
        (var @out, err) = testenv.Command(new fipstest_internal_test_package.testing_TжTB(t),
            dockerˢ, runˢ, "--rm", "-v", fmt.Sprintf("%s:%s"u8, pwd, pwd), "-w", pwd,
            nistSp80090bˢ, eaNonIidˢ, seqSamplesName, "8").CombinedOutput();
        if (err != nil) {
            t.Fatalf("ea_non_iid failed: %v\n%s", err, @out);
        }
        t.Logf("\n%s", @out);

        @string H_I = (@string)@out;
        H_I = strings.TrimSpace(H_I.slice(strings.LastIndexByte(H_I, (byte)' ') + 1));
        t.Logf("running ea_restart analysis with H_I = %s", H_I);
        (@out, err) = testenv.Command(new fipstest_internal_test_package.testing_TжTB(t),
            dockerˢ, runˢ, "--rm", "-v", fmt.Sprintf("%s:%s"u8, pwd, pwd), "-w", pwd,
            nistSp80090bˢ, eaRestartˢ, restartSamplesName, "8", H_I).CombinedOutput();
        if (err != nil) {
            t.Fatalf("ea_restart failed: %v\n%s", err, @out);
        }
        t.Logf("\n%s", @out);
    }
}

public static readonly ж<@string> ᏑNISTSP80090BDockerfile = new StaticBox<@string>("""

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

"""u8.Raw());
public static ref @string NISTSP80090BDockerfile => ref ((StaticBox<@string>)ᏑNISTSP80090BDockerfile).ValueSlot;

internal static readonly ж<entropy.ScratchBuffer> Ꮡmemory = new StaticBox<entropy.ScratchBuffer>(default(entropy.ScratchBuffer));
internal static ref entropy.ScratchBuffer memory => ref ((StaticBox<entropy.ScratchBuffer>)Ꮡmemory).ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object entropySamplesFailed10ˢ = (@string)"entropy.Samples() failed 10 times in a row"u8;

// samplesOrTryAgain calls entropy.Samples up to 10 times until it succeeds.
// Samples has a non-negligible chance of failing the health tests, as required
// by SP 800-90B.
internal static void samplesOrTryAgain(ж<testing.T> t, slice<uint8> samples) {
    t.Helper();
    foreach (var _ᴛ1 in range(10)) {
        {
            var err = entropy.Samples(samples, Ꮡmemory); if (err != nil) {
                t.Logf("entropy.Samples() failed: %v", err);
                continue;
            }
        }
        return;
    }
    t.Fatal(entropySamplesFailed10ˢ);
}

public static void TestEntropySHA384(ж<testing.T> t) {
    ref var input = ref heap(new array<uint8>(1024), out var Ꮡinput);
    foreach (var i in range(input)) {
        input[i] = (uint8)i;
    }
    var want = sha512.Sum384(input[..]);
    var got = entropy.SHA384(Ꮡinput);
    if (got != want) {
        t.Errorf("SHA384() = %x, want %x", got, want);
    }

    foreach (var l in range(1024 * 3 + 1)) {
        var inputΔ1 = new slice<byte>(l);
        rand.Read(inputΔ1);
        var wantΔ1 = sha512.Sum384(inputΔ1);
        var gotΔ1 = entropy.TestingOnlySHA384(inputΔ1);
        if (gotΔ1 != wantΔ1) {
            t.Errorf("TestingOnlySHA384(%d bytes) = %x, want %x", l, gotΔ1, wantΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object repetitionCountTestBadˢ = (@string)"RepetitionCountTest(bad) = nil, want error"u8;

public static void TestEntropyRepetitionCountTest(ж<testing.T> t) {
    var good = bytes.Repeat(append(bytes.Repeat(slice<uint8>([42]), 40), (byte)1), 100);
    {
        var err = entropy.RepetitionCountTest(good); if (err != nil) {
            t.Errorf("RepetitionCountTest(good) = %v, want nil", err);
        }
    }

    var bad = bytes.Repeat(slice<uint8>([0]), 40);
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(slice<uint8>([1]), 40));
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(slice<uint8>([42]), 41));
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(slice<uint8>([2]), 40));
    {
        var err = entropy.RepetitionCountTest(bad); if (err == nil) {
            t.Error(repetitionCountTestBadˢ);
        }
    }

    bad = bytes.Repeat(slice<uint8>([42]), 41);
    {
        var err = entropy.RepetitionCountTest(bad); if (err == nil) {
            t.Error(repetitionCountTestBadˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object adaptiveProportionTestˢ = (@string)"AdaptiveProportionTest(bad) = nil, want error"u8;

public static void TestEntropyAdaptiveProportionTest(ж<testing.T> t) {
    var good = bytes.Repeat(slice<uint8>([0]), 409);
    good = appendꓸꓸꓸ(good, bytes.Repeat(slice<uint8>([1]), 512 - 409));
    good = appendꓸꓸꓸ(good, bytes.Repeat(slice<uint8>([0]), 409));
    {
        var err = entropy.AdaptiveProportionTest(good); if (err != nil) {
            t.Errorf("AdaptiveProportionTest(good) = %v, want nil", err);
        }
    }

    // These fall out of the window.
    var bad = bytes.Repeat(slice<uint8>([1]), 100);
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(slice<uint8>([1, 2, 3, 4, 5, 6]), 100));
    // These are in the window.
    bad = appendꓸꓸꓸ(bad, bytes.Repeat(slice<uint8>([42]), 410));
    {
        var err = entropy.AdaptiveProportionTest(bad.slice(0, len(bad) - 1)); if (err != nil) {
            t.Errorf("AdaptiveProportionTest(bad[:len(bad)-1]) = %v, want nil", err);
        }
    }
    {
        var err = entropy.AdaptiveProportionTest(bad); if (err == nil) {
            t.Error(adaptiveProportionTestˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string entropyV100ˢ => ref entropyV100ˢᵇ;
private static readonly @string entropyV100ˢᵇ = "../entropy/v1.0.0"u8;

public static void TestEntropyUnchanged(ж<testing.T> t) {
    testenv.MustHaveSource(new fipstest_internal_test_package.testing_TжTB(t));

    var h = sha256.New();
    var root = os.DirFS(entropyV100ˢ);
    {
        var hʗ1 = h;
        var rootʗ1 = root;
        var err = fs.WalkDir(root, ".", error (@string path, fs.DirEntry d, error errΔ1) => {
            if (errΔ1 != nil) {
                return errΔ1;
            }
            if (d.IsDir()) {
                return default;
            }
            (var data, errΔ1) = fs.ReadFile(rootʗ1, path);
            if (errΔ1 != nil) {
                return errΔ1;
            }
            t.Logf("Hashing %s (%d bytes)", path, len(data));
            fmt.Fprintf(hʗ1, "%s %d\n"u8, path, len(data));
            hʗ1.Write(data);
            return default;
        }); if (err != nil) {
            t.Fatalf("WalkDir: %v", err);
        }
    }

    // The crypto/internal/entropy/v1.0.0 package is certified as a FIPS 140-3
    // entropy source through the Entropy Source Validation program,
    // independently of the FIPS 140-3 module. It must not change even across
    // FIPS 140-3 module versions, in order to reuse the ESV certificate.
    @string exp = "2541273241ae8aafe55026328354ed3799df1e2fb308b2097833203a42911b53";
    {
        @string got = hex.EncodeToString(h.Sum(default)); if (got != exp) {
            t.Errorf("hash of crypto/internal/entropy/v1.0.0 = %s, want %s", got, exp);
        }
    }
}

public static partial void TestEntropyRace(ж<testing.T> t) {
    // Check that concurrent calls to Seed don't trigger the race detector.
    foreach (var _ᴛ1 in range(16)) {
        goǃ(() => {
            (_, _) = entropy.Seed(Ꮡmemory);
        }, GoStack.Bounded);
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

public static void BenchmarkEntropySeed(ж<testing.B> b) {
    while (b.Loop()) {
        var (seed, err) = entropy.Seed(Ꮡmemory);
        if (err != nil) {
            b.Fatalf("entropy.Seed() failed: %v", err);
        }
        sink ^= (byte)(seed[0]);
    }
}

} // end fipstest_internal_test_package
