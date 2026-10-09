// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ed25519 = go.crypto.ed25519_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using hex = encoding.hex_package;
using json = encoding.json_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal;
using path;
using static go.crypto.ed25519_internal_test_package;

partial class ed25519_test_package {

internal partial struct TestEd25519Vectors_vectors /*dyn*/ {
    public @string A, R, S, M;
    public slice<@string> Flags;
}

// TestEd25519Vectors runs a very large set of test vectors that exercise all
// combinations of low-order points, low-order components, and non-canonical
// encodings. These vectors lock in unspecified and spec-divergent behaviors in
// edge cases that are not security relevant in most contexts, but that can
// cause issues in consensus applications if changed.
//
// Our behavior matches the "classic" unwritten verification rules of the
// "ref10" reference implementation.
//
// Note that although we test for these edge cases, they are not covered by the
// Go 1 Compatibility Promise. Applications that need stable verification rules
// should use github.com/hdevalence/ed25519consensus.
//
// See https://hdevalence.ca/blog/2020-10-04-its-25519am for more details.
public static void TestEd25519Vectors(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var jsonVectors = downloadEd25519Vectors(Ꮡt);
    ref var vectors = ref heap<slice<TestEd25519Vectors_vectors>>(out var Ꮡvectors);
    {
        var err = json.Unmarshal(jsonVectors, Ꮡvectors); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    foreach (var (i, v) in vectors) {
        var expectedToVerify = true;
        foreach (var (_, f) in v.Flags) {
            var exprᴛ1 = f;
            if (exprᴛ1 == "LowOrderResidue"u8) {
                expectedToVerify = false;
            }
            else if (exprᴛ1 == "NonCanonicalR"u8) {
                expectedToVerify = false;
            }

        }
        // We use the simplified verification formula that doesn't multiply
        // by the cofactor, so any low order residue will cause the
        // signature not to verify.
        //
        // This is allowed, but not required, by RFC 8032.
        // Our point decoding allows non-canonical encodings (in violation
        // of RFC 8032) but R is not decoded: instead, R is recomputed and
        // compared bytewise against the canonical encoding.
        var publicKey = decodeHex(Ꮡt, v.A);
        var signature = appendꓸꓸꓸ(decodeHex(Ꮡt, v.R), decodeHex(Ꮡt, v.S));
        var message = slice<byte>(v.M);
        var didVerify = ed25519.Verify(publicKey, message, signature);
        if (didVerify && !expectedToVerify) {
            Ꮡt.Errorf("#%d: vector with flags %s unexpectedly verified"u8, i, v.Flags);
        }
        if (!didVerify && expectedToVerify) {
            Ꮡt.Errorf("#%d: vector with flags %s unexpectedly rejected"u8, i, v.Flags);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string filippoIoMostlyHarmlessˢ = "filippo.io/mostly-harmless/ed25519vectors"u8;
internal static readonly @string v00020210322192420ˢ = "v0.0.0-20210322192420-30a2d7243a94"u8;
internal static readonly @string ed25519vectorsJsonˢ = "ed25519vectors.json"u8;

internal static slice<byte> downloadEd25519Vectors(ж<testing.T> Ꮡt) {
    // Download the JSON test file from the GOPROXY with `go mod download`,
    // pinning the version so test and module caching works as expected.
    @string path = filippoIoMostlyHarmlessˢ;
    @string version = v00020210322192420ˢ;
    @string dir = cryptotest.FetchModule(Ꮡt, path, version);
    var (jsonVectors, err) = os.ReadFile(filepath.Join(dir, ed25519vectorsJsonˢ));
    if (err != default!) {
        Ꮡt.Fatalf("failed to read ed25519vectors.json: %v"u8, err);
    }
    return jsonVectors;
}

internal static slice<byte> decodeHex(ж<testing.T> Ꮡt, @string s) {
    Ꮡt.Helper();
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        Ꮡt.Errorf("invalid hex: %v"u8, err);
    }
    return b;
}

} // end ed25519_test_package
