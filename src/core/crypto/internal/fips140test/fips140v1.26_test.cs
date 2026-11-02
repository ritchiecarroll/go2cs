// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto.@internal.fips140test;

using mldsa = go.crypto.@internal.fips140.mldsa_package;
using testing = testing_package;
using go.crypto.@internal.fips140;

partial class fipstest_internal_test_package {

internal static void fips140v126Conditionals() {
    // ML-DSA sign and verify PCT
    var kMLDSA = mldsa.GenerateKey44();
    // ML-DSA-44
    mldsa.SignDeterministic(kMLDSA, new slice<byte>(32), ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mlDsaKeyGenSigGenSigVerˢ => ref mlDsaKeyGenSigGenSigVerˢᵇ;
private static readonly @string mlDsaKeyGenSigGenSigVerˢᵇ = "ML-DSA KeyGen, SigGen, SigVer"u8;

internal static void testFIPS140v126(ж<testing.T> t, slice<byte> plaintext) {
    var plaintextʗ1 = plaintext;
    t.Run(mlDsaKeyGenSigGenSigVerˢ, (ж<testing.T> tΔ1) => {
        ensureServiceIndicator(tΔ1);
        var k = mldsa.GenerateKey44();

        var (sig, err) = mldsa.SignDeterministic(k, plaintextʗ1, ""u8);
        fatalIfErr(tΔ1, err);
        tΔ1.Logf("ML-DSA signature: %x", sig);

        err = mldsa.Verify(k.PublicKey(), plaintextʗ1, sig, ""u8);
        fatalIfErr(tΔ1, err);
    });
}

} // end fipstest_internal_test_package
