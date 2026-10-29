// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mlkemtest provides testing functions for the ML-KEM algorithm.
namespace go.crypto.mlkem;

using fips140mlkem = go.crypto.@internal.fips140.mlkem_package;
using fips140only = go.crypto.@internal.fips140only_package;
using mlkem = go.crypto.mlkem_package;
using errors = errors_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class mlkemtest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string mlkemtestEncapsulate768ˢ = "mlkemtest: Encapsulate768: random must be 32 bytes"u8;
private static readonly @string cryptoMlkemMlkemtestUseˢ = "crypto/mlkem/mlkemtest: use of derandomized encapsulation is not allowed in FIPS 140-only mode"u8;

// Encapsulate768 implements derandomized ML-KEM-768 encapsulation
// (ML-KEM.Encaps_internal from FIPS 203) using the provided encapsulation key
// ek and 32 bytes of randomness.
//
// It must only be used for known-answer tests.
public static (slice<byte> sharedKey, slice<byte> ciphertext, error err) Encapsulate768(ж<mlkem.EncapsulationKey768> Ꮡek, slice<byte> random) {
    slice<byte> sharedKey = default;
    slice<byte> ciphertext = default;
    error err = default;
    ref var ek = ref Ꮡek.DerefOrNull();

    if (len(random) != 32) {
        return (default, default, errors.New(mlkemtestEncapsulate768ˢ));
    }
    if (fips140only.Enforced()) {
        return (default, default, errors.New(cryptoMlkemMlkemtestUseˢ));
    }
    (var k, err) = fips140mlkem.NewEncapsulationKey768(ek.Bytes());
    if (err != nil) {
        return (default, default, errors.New("mlkemtest: Encapsulate768: failed to reconstruct key: "u8 + err.Error()));
    }
    (sharedKey, ciphertext) = k.EncapsulateInternal(Ꮡ(array<byte>.Alias(random, 32)));
    return (sharedKey, ciphertext, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string mlkemtestEncapsulate1024ˢ = "mlkemtest: Encapsulate1024: random must be 32 bytes"u8;

// Encapsulate1024 implements derandomized ML-KEM-1024 encapsulation
// (ML-KEM.Encaps_internal from FIPS 203) using the provided encapsulation key
// ek and 32 bytes of randomness.
//
// It must only be used for known-answer tests.
public static (slice<byte> sharedKey, slice<byte> ciphertext, error err) Encapsulate1024(ж<mlkem.EncapsulationKey1024> Ꮡek, slice<byte> random) {
    slice<byte> sharedKey = default;
    slice<byte> ciphertext = default;
    error err = default;
    ref var ek = ref Ꮡek.DerefOrNull();

    if (len(random) != 32) {
        return (default, default, errors.New(mlkemtestEncapsulate1024ˢ));
    }
    if (fips140only.Enforced()) {
        return (default, default, errors.New(cryptoMlkemMlkemtestUseˢ));
    }
    (var k, err) = fips140mlkem.NewEncapsulationKey1024(ek.Bytes());
    if (err != nil) {
        return (default, default, errors.New("mlkemtest: Encapsulate1024: failed to reconstruct key: "u8 + err.Error()));
    }
    (sharedKey, ciphertext) = k.EncapsulateInternal(Ꮡ(array<byte>.Alias(random, 32)));
    return (sharedKey, ciphertext, default);
}

} // end mlkemtest_package
