// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using crypto = crypto_package;
using fips140 = go.crypto.@internal.fips140_package;
using rand = go.crypto.rand_package;
using fmt = fmt_package;
using testenv = go.@internal.testenv_package;
using fs = go.io.fs_package;
using os = os_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
// blank import: go.crypto.@internal.fips140.aes_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.@internal.fips140.aes.gcm_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.@internal.fips140.drbg_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using ecdh = go.crypto.@internal.fips140.ecdh_package;
using ecdsa = go.crypto.@internal.fips140.ecdsa_package;
using ed25519 = go.crypto.@internal.fips140.ed25519_package;
// blank import: go.crypto.@internal.fips140.hkdf_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.@internal.fips140.hmac_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using rsa = go.crypto.@internal.fips140.rsa_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
// blank import: go.crypto.@internal.fips140.sha3_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.@internal.fips140.sha512_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.@internal.fips140.tls12_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
// blank import: go.crypto.@internal.fips140.tls13_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using exec = go.os.exec_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.io;
using go.os;
using hash = hash_package;
using io = io_package;
using nistec = go.crypto.@internal.fips140.nistec_package;

partial class fipstest_internal_test_package {

internal static slice<@string> allCASTs = [
    "AES-CBC",
    "CTR_DRBG",
    "CounterKDF",
    "DetECDSA P-256 SHA2-512 sign",
    "ECDH PCT",
    "ECDSA P-256 SHA2-512 sign and verify",
    "ECDSA PCT",
    "Ed25519 sign and verify",
    "Ed25519 sign and verify PCT",
    "HKDF-SHA2-256",
    "HMAC-SHA2-256",
    "KAS-ECC-SSC P-256",
    "ML-DSA sign and verify PCT",
    "ML-DSA-44",
    "ML-KEM PCT", // -768
    "ML-KEM PCT", // -1024
    "ML-KEM-768",
    "PBKDF2",
    "RSA sign and verify PCT",
    "RSASSA-PKCS-v1.5 2048-bit sign and verify",
    "SHA2-256",
    "SHA2-512",
    "TLSv1.2-SHA2-256",
    "TLSv1.3-SHA2-256",
    "cSHAKE128"
];

[GoInit] internal static void initΔ1() {
    if (fips140.Version() == "v1.0.0"u8) {
        allCASTs = slices.DeleteFunc(allCASTs, (@string s) => strings.HasPrefix(s, "ML-DSA"));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string listˢ => ref listˢᵇ;
private static readonly @string listˢᵇ = "list"u8;
internal static ref readonly @string dirˢ => ref dirˢᵇ;
private static readonly @string dirˢᵇ = @"{{.Dir}}"u8;
internal static ref readonly @string cryptoInternalFips140ˢ => ref cryptoInternalFips140ˢᵇ;
private static readonly @string cryptoInternalFips140ˢᵇ = "crypto/internal/fips140"u8;
internal static ref readonly @string fips140CastPctˢ => ref fips140CastPctˢᵇ;
private static readonly @string fips140CastPctˢᵇ = @"fips140\.(CAST|PCT)\(""([^""]+)"""u8;

public static void TestAllCASTs(ж<testing.T> t) {
    testenv.MustHaveSource(new fipstest_internal_test_package.testing_TжTB(t));

    // Ask "go list" for the location of the crypto/internal/fips140 tree, as it
    // might be the unpacked frozen tree selected with GOFIPS140.
    var cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(t), testenv.GoToolPath(new fipstest_internal_test_package.testing_TжTB(t)), listˢ, "-f", dirˢ, cryptoInternalFips140ˢ);
    var (@out, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go list: %v\n%s", err, @out);
    }
    @string fipsDir = strings.TrimSpace((@string)@out);
    t.Logf("FIPS module directory: %s", fipsDir);

    // Find all invocations of fips140.CAST or fips140.PCT.
    ref var foundCASTs = ref heap<slice<@string>>(out var ᏑfoundCASTs);
    var castRe = regexp.MustCompile(fips140CastPctˢ);
    {
        var castReʗ1 = castRe;
        var errΔ1 = fs.WalkDir(os.DirFS(fipsDir), ".", error (@string path, fs.DirEntry d, error errΔ2) => {
            if (errΔ2 != nil) {
                return errΔ2;
            }
            if (d.IsDir() || !strings.HasSuffix(path, ".go")) {
                return default;
            }
            (var data, errΔ2) = os.ReadFile(fipsDir + "/"u8 + path);
            if (errΔ2 != nil) {
                return errΔ2;
            }
            foreach (var (_, m) in range(castReʗ1.FindAllSubmatch(data, -1))) {
                ᏑfoundCASTs.ValueSlot = append(ᏑfoundCASTs.ValueSlot, (@string)m[2]);
            }
            return default;
        }); if (errΔ1 != nil) {
            t.Fatalf("WalkDir: %v", errΔ1);
        }
    }

    slices.Sort<slice<@string>, @string>(foundCASTs);
    if (!slices.Equal<slice<@string>, @string>(foundCASTs, allCASTs)) {
        t.Errorf("AllCASTs is out of date. Found CASTs: %#v", foundCASTs);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string completedSuccessfullyˢ => ref completedSuccessfullyˢᵇ;
private static readonly @string completedSuccessfullyˢᵇ = "completed successfully"u8;

// TestConditionals causes the conditional CASTs and PCTs to be invoked.
public static void TestConditionals(ж<testing.T> t) {
    fips140v126Conditionals();
    // ML-KEM PCT
    var (kMLKEM, err) = mlkem.GenerateKey768();
    if (err != nil) {
        t.Error(err);
    } else {
        // ML-KEM-768
        kMLKEM.EncapsulationKey().Encapsulate();
    }
    // ECDH PCT
    (var kDH, err) = ecdh.GenerateKey<ecdh.P256PointжPoint>(ecdh.P256(), rand.Reader);
    if (err != nil) {
        t.Error(err);
    } else {
        // KAS-ECC-SSC P-256
        ecdh.ECDH<ecdh.P256PointжPoint>(ecdh.P256(), kDH, kDH.PublicKey());
    }
    // ECDSA PCT
    (var kDSA, err) = ecdsa.GenerateKey<ecdsa.P256PointжPoint>(ecdsa.P256(), rand.Reader);
    if (err != nil) {
        t.Error(err);
    } else {
        // ECDSA P-256 SHA2-512 sign and verify
        ecdsa.SignDeterministic<ecdsa.P256PointжPoint, hash.Hash>(ecdsa.P256(), widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ1 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ1)), kDSA, new slice<byte>(32));
    }
    // Ed25519 sign and verify PCT
    (var k25519, err) = ed25519.GenerateKey();
    if (err != nil) {
        t.Error(err);
    } else {
        // Ed25519 sign and verify
        ed25519.Sign(k25519, new slice<byte>(32));
    }
    // RSA sign and verify PCT
    (var kRSA, err) = rsa.GenerateKey(rand.Reader, 2048);
    if (err != nil) {
        t.Error(err);
    } else {
        // RSASSA-PKCS-v1.5 2048-bit sign and verify
        rsa.SignPKCS1v15(kRSA, crypto.SHA256.String(), new slice<byte>(32));
    }
    t.Log(completedSuccessfullyˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRunTestConditionalsˢ => ref testRunTestConditionalsˢᵇ;
private static readonly @string testRunTestConditionalsˢᵇ = "-test.run=^TestConditionals$"u8;
internal static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;

public static void TestCASTPasses(ж<testing.T> t) {
    moduleStatus(t);
    cryptotest.MustSupportFIPS140(t);

    var cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(t), testenv.Executable(new fipstest_internal_test_package.testing_TжTB(t)), testRunTestConditionalsˢ, testVˢ);
    cmd.Env = append(cmd.Environ(), (@string)"GODEBUG=fips140=debug");
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("running with GODEBUG=fips140=debug:\n%s", @out);
    if (err != nil || !strings.Contains((@string)@out, completedSuccessfullyˢ)) {
        t.Errorf("TestConditionals did not complete successfully");
    }

    foreach (var (_, name) in range(allCASTs)) {
        var outʗ1 = @out;
        t.Run(name, (ж<testing.T> tΔ1) => {
            if (!strings.Contains((@string)outʗ1, fmt.Sprintf("passed: %s\n"u8, name))) {
                tΔ1.Errorf("CAST/PCT %s success was not logged", name);
            } else {
                tΔ1.Logf("CAST/PCT succeeded: %s", name);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testDidNotFailAsExpectedˢ = (@string)"test did not fail as expected"u8;

public static void TestCASTFailures(ж<testing.T> t) {
    moduleStatus(t);
    cryptotest.MustSupportFIPS140(t);

    foreach (var (_, name) in range(allCASTs)) {
        t.Run(name, (ж<testing.T> tΔ1) => {
            // Don't parallelize if running in verbose mode, to produce a less
            // confusing recoding for the validation lab.
            if (!testing.Verbose()) {
                tΔ1.Parallel();
            }
            tΔ1.Logf("Testing CAST/PCT failure...");
            var cmd = testenv.Command(new fipstest_internal_test_package.testing_TжTB(tΔ1), testenv.Executable(new fipstest_internal_test_package.testing_TжTB(tΔ1)), testRunTestConditionalsˢ, testVˢ);
            @string GODEBUG = fmt.Sprintf("GODEBUG=failfipscast=%s,fips140=on"u8, name);
            cmd.Env = append(cmd.Environ(), GODEBUG);
            var (@out, err) = cmd.CombinedOutput();
            tΔ1.Logf("running with %s:\n%s", GODEBUG, @out);
            if (err == nil) {
                tΔ1.Fatal(testDidNotFailAsExpectedˢ);
            }
            if (strings.Contains((@string)@out, completedSuccessfullyˢ)) {
                tΔ1.Errorf("CAST/PCT %s failure did not stop the program", name);
            } else if (!strings.Contains((@string)@out, "self-test failed: "u8 + name)) {
                tΔ1.Errorf("CAST/PCT %s failure did not log the expected message", name);
            } else {
                tΔ1.Logf("CAST/PCT %s failed as expected and caused the program to exit", name);
            }
        });
    }
}

} // end fipstest_internal_test_package
