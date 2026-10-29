// Copyright 2024 The Go Authors. All rights reserved.
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

partial class mlkem_package {

internal static Action fipsSelfTest;
internal static void initᴛfipsSelfTest() { fipsSelfTest = sync.OnceFunc(() => {
    fips140.CAST("ML-KEM-768"u8, fips140CAST);
}); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedPrivateKeyHashˢ = "unexpected private key hash"u8;
internal static readonly @string unexpectedRejectionˢ = "unexpected rejection result"u8;
internal static readonly @string unexpectedResultˢ = "unexpected result"u8;

// fips140CAST covers both the successful and the implicit rejection paths, as
// required by IG 10.3.A, Resolution 14, and as tested by TestCASTRejectionPath.
// It tests only one parameter set as allowed by Note22. It compares dk and not
// ek, because ek is part of dk, as allowed by the same Resolution. It compares
// dk with an expected hash to avoid embedding several kilobytes of test vectors
// in every binary, as allowed by GeneralNote7.
internal static error fips140CAST() {
    ж<array<byte>> d = Ꮡ(new byte[]{
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10,
        0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
        0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20
    }.array());
    ж<array<byte>> z = Ꮡ(new byte[]{
        0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
        0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30,
        0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
        0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40
    }.array());
    slice<byte> dkHash = slice<byte>([
        0xa1, 0x32, 0x4a, 0x71, 0xf5, 0x7e, 0x26, 0x85,
        0x37, 0x7e, 0x4f, 0x7d, 0xe2, 0xd1, 0xf0, 0x58,
        0x53, 0x8e, 0x08, 0x7e, 0xe3, 0x9b, 0x48, 0x2b,
        0x3d, 0xbf, 0x96, 0xf9, 0xf7, 0x69, 0xc7, 0x85
    ]);
    ж<array<byte>> m = Ꮡ(new byte[]{
        0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
        0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50,
        0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
        0x59, 0x5a, 0x5b, 0x5c, 0x5d, 0x5e, 0x5f, 0x60
    }.array());
    slice<byte> K = slice<byte>([
        0x55, 0x01, 0xfc, 0x52, 0x3b, 0x74, 0x5f, 0x41,
        0x76, 0x2a, 0x18, 0x8d, 0xe4, 0x4a, 0x59, 0xb9,
        0x20, 0xf4, 0x30, 0x14, 0x62, 0x04, 0xee, 0x4e,
        0x79, 0x37, 0x32, 0x39, 0x6d, 0xf7, 0xaa, 0x48
    ]);
    slice<byte> Kr = slice<byte>([
        0x01, 0xa1, 0x03, 0x3a, 0x2e, 0x68, 0x33, 0x80,
        0x7f, 0xd3, 0x91, 0x80, 0xbe, 0x3b, 0x4f, 0x0a,
        0x1f, 0x6f, 0x50, 0x51, 0x67, 0x56, 0x9d, 0x0e,
        0x5e, 0x5e, 0x1f, 0xdb, 0x52, 0xa9, 0x10, 0xec
    ]);
    var dk = Ꮡ(new DecapsulationKey768());
    kemKeyGen(dk, d, ref (z).DerefOrNull());
    var ek = dk.EncapsulationKey();
    var H = sha256.New();
    H.Write(TestingOnlyExpandedBytes768(dk));
    if (!bytes_package.Equal(H.Sum(default), dkHash)) {
        return errors.New(unexpectedPrivateKeyHashˢ);
    }
    ref var cc = ref heap(new array<byte>(1088), out var Ꮡcc);
    var Kd = kemDecaps(ref (dk).DerefOrNull(), Ꮡcc);
    if (!bytes_package.Equal(Kd, Kr)) {
        return errors.New(unexpectedRejectionˢ);
    }
    var (Ke, _) = kemEncaps(Ꮡcc, ref (ek).DerefOrNull(), m);
    Kd = kemDecaps(ref (dk).DerefOrNull(), Ꮡcc);
    if (!bytes_package.Equal(Ke, K) || !bytes_package.Equal(Kd, K)) {
        return errors.New(unexpectedResultˢ);
    }
    return default;
}

} // end mlkem_package
