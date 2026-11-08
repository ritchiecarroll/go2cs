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
public static void TestEd25519Vectors(ж<testing.T> t) {
    var jsonVectors = downloadEd25519Vectors(t);
    ref var vectors = ref heap<slice<TestEd25519Vectors_vectors>>(out var Ꮡvectors);
    var err = json.Unmarshal(jsonVectors, Ꮡvectors); if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (i, v) in range(vectors)) {
        var expectedToVerify = true;
        foreach (var (_, f) in range(v.Flags)) {
            var exprᴛ1 = f;
            switch (ᐧ) {
            // We use the simplified verification formula that doesn't multiply
            // by the cofactor, so any low order residue will cause the
            // signature not to verify.
            //
            // This is allowed, but not required, by RFC 8032.
            case {} when exprᴛ1 == "LowOrderResidue"u8: {
                expectedToVerify = false;
                break;
            }
            // Our point decoding allows non-canonical encodings (in violation
            // of RFC 8032) but R is not decoded: instead, R is recomputed and
            // compared bytewise against the canonical encoding.
            case {} when exprᴛ1 == "NonCanonicalR"u8: {
                expectedToVerify = false;
                break;
            }}
        }

        var publicKey = decodeHex(t, v.A);
        var signature = appendꓸꓸꓸ(decodeHex(t, v.R), decodeHex(t, v.S));
        var message = slice<byte>(v.M);

        var didVerify = ed25519.Verify(publicKey, message, signature);
        if (didVerify && !expectedToVerify) {
            t.Errorf("#%d: vector with flags %s unexpectedly verified", i, v.Flags);
        }
        if (!didVerify && expectedToVerify) {
            t.Errorf("#%d: vector with flags %s unexpectedly rejected", i, v.Flags);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string filippoIoMostlyHarmlessˢ => ref filippoIoMostlyHarmlessˢᵇ;
private static readonly @string filippoIoMostlyHarmlessˢᵇ = "filippo.io/mostly-harmless/ed25519vectors"u8;
internal static ref readonly @string v00020210322192420ˢ => ref v00020210322192420ˢᵇ;
private static readonly @string v00020210322192420ˢᵇ = "v0.0.0-20210322192420-30a2d7243a94"u8;
internal static ref readonly @string ed25519vectorsJsonˢ => ref ed25519vectorsJsonˢᵇ;
private static readonly @string ed25519vectorsJsonˢᵇ = "ed25519vectors.json"u8;

internal static slice<byte> downloadEd25519Vectors(ж<testing.T> t) {
    // Download the JSON test file from the GOPROXY with `go mod download`,
    // pinning the version so test and module caching works as expected.
    @string path = filippoIoMostlyHarmlessˢ;
    @string version = v00020210322192420ˢ;
    @string dir = cryptotest.FetchModule(t, path, version);

    var (jsonVectors, err) = os.ReadFile(filepath.Join(dir, ed25519vectorsJsonˢ));
    if (err != nil) {
        t.Fatalf("failed to read ed25519vectors.json: %v", err);
    }
    return jsonVectors;
}

internal static slice<byte> decodeHex(ж<testing.T> t, @string s) {
    t.Helper();
    var (b, err) = hex.DecodeString(s);
    if (err != nil) {
        t.Errorf("invalid hex: %v", err);
    }
    return b;
}

} // end ed25519_test_package
