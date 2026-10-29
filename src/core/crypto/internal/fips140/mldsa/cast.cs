// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
// blank import: go.crypto.@internal.fips140.check_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using sha256 = go.crypto.@internal.fips140.sha256_package;
using errors = errors_package;
using sync = sync_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class mldsa_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mlDsaSignAndVerifyPctˢ = "ML-DSA sign and verify PCT"u8;

internal static void fipsPCT(ж<PrivateKey> priv) {
    fips140.PCT(mlDsaSignAndVerifyPctˢ, () => {
        var μ = new slice<byte>(64);
        var (sig, err) = SignExternalMuDeterministic(priv, μ);
        if (err != nil) {
            return err;
        }
        return VerifyExternalMu(priv.PublicKey(), μ, sig);
    });
}

internal static Action fipsSelfTest;
internal static void initᴛfipsSelfTest() { fipsSelfTest = sync.OnceFunc(() => {
    fips140.CAST("ML-DSA-44"u8, fips140CAST);
}); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedPrivateKeyHashˢ = "unexpected private key hash"u8;
internal static readonly @string unexpectedSignatureHashˢ = "unexpected signature hash"u8;

// fips140CAST covers all rejection sampling paths, as recommended by IG 10.3.A,
// and as tested by TestCASTRejectionPaths. It tests only one parameter set as
// allowed by Note26. It tests the modified version of Algorithm 7 and 8 with a
// fixed mu/μ, as allowed by IG 10.3.A, Resolution 15. It compares sk and not
// pk, because H(pk) is part of sk, as allowed by the same Resolution. It
// compares the results with hashes instead of values, to avoid embedding several
// kilobytes of test vectors in every binary, as allowed by GeneralNote7.
internal static error fips140CAST() {
    // From https://pages.nist.gov/ACVP/draft-celi-acvp-ml-dsa.html#table-1.
    ж<array<byte>> seed = Ꮡ(new byte[]{
        0x5c, 0x62, 0x4f, 0xcc, 0x18, 0x62, 0x45, 0x24,
        0x52, 0xd0, 0xc6, 0x65, 0x84, 0x0d, 0x82, 0x37,
        0xf4, 0x31, 0x08, 0xe5, 0x49, 0x9e, 0xdc, 0xdc,
        0x10, 0x8f, 0xbc, 0x49, 0xd5, 0x96, 0xe4, 0xb7
    }.array());
    ж<array<byte>> μ = Ꮡ(new byte[]{
        0x2a, 0xd1, 0xc7, 0x2b, 0xb0, 0xfc, 0xbe, 0x28,
        0x09, 0x9c, 0xe8, 0xbd, 0x2e, 0xd8, 0x36, 0xdf,
        0xeb, 0xe5, 0x20, 0xaa, 0xd3, 0x8f, 0xba, 0xc6,
        0x6e, 0xf7, 0x85, 0xa3, 0xcf, 0xb1, 0x0f, 0xb4,
        0x19, 0x32, 0x7f, 0xa5, 0x78, 0x18, 0xee, 0x4e,
        0x37, 0x18, 0xda, 0x4b, 0xe4, 0x8d, 0x24, 0xb5,
        0x9a, 0x20, 0x8f, 0x88, 0x07, 0x27, 0x1f, 0xdb,
        0x7e, 0xda, 0x6e, 0x60, 0x14, 0x1b, 0xd2, 0x63
    }.array());
    slice<byte> skHash = slice<byte>([
        0x29, 0x37, 0x49, 0x51, 0xcb, 0x2b, 0xc3, 0xcd,
        0xa7, 0x31, 0x5c, 0xe7, 0xf0, 0xab, 0x99, 0xc7,
        0xd2, 0xd6, 0x52, 0x92, 0xe6, 0xc5, 0x15, 0x6e,
        0x8a, 0xa6, 0x2a, 0xc1, 0x4b, 0x14, 0x12, 0xaf
    ]);
    slice<byte> sigHash = slice<byte>([
        0xdc, 0xc7, 0x1a, 0x42, 0x1b, 0xc6, 0xff, 0xaf,
        0xb7, 0xdf, 0x0c, 0x7f, 0x6d, 0x01, 0x8a, 0x19,
        0xad, 0xa1, 0x54, 0xd1, 0xe2, 0xee, 0x36, 0x0e,
        0xd5, 0x33, 0xce, 0xcd, 0x5d, 0xc9, 0x80, 0xad
    ]);
    var priv = newPrivateKey(seed, params44);
    var H = sha256.New();
    H.Write(TestingOnlyPrivateKeySemiExpandedBytes(priv));
    if (!bytes.Equal(H.Sum(default), skHash)) {
        return errors.New(unexpectedPrivateKeyHashˢ);
    }
    ref var random = ref heap(new array<byte>(32), out var Ꮡrandom);
    var sig = signInternal(ref (priv).DerefOrNull(), μ, Ꮡrandom);
    H.Reset();
    H.Write(sig);
    if (!bytes.Equal(H.Sum(default), sigHash)) {
        return errors.New(unexpectedSignatureHashˢ);
    }
    return verifyInternal(ref (priv.PublicKey()).DerefOrNull(), μ, sig);
}

} // end mldsa_package
