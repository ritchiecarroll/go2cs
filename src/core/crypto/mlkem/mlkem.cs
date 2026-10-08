// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package mlkem implements the quantum-resistant key encapsulation method
// ML-KEM (formerly known as Kyber), as specified in [NIST FIPS 203].
//
// Most applications should use the ML-KEM-768 parameter set, as implemented by
// [DecapsulationKey768] and [EncapsulationKey768].
//
// [NIST FIPS 203]: https://doi.org/10.6028/NIST.FIPS.203
namespace go.crypto;

using mlkem = go.crypto.@internal.fips140.mlkem_package;
using go.crypto.@internal.fips140;

partial class mlkem_package {

public static UntypedInt SharedKeySize => 32;
public static UntypedInt SeedSize => 64;
public static UntypedInt CiphertextSize768 => 1088;
public static UntypedInt EncapsulationKeySize768 => 1184;
public static UntypedInt CiphertextSize1024 => 1568;
public static UntypedInt EncapsulationKeySize1024 => 1568;

// DecapsulationKey768 is the secret key used to decapsulate a shared key
// from a ciphertext. It includes various precomputed values.
partial struct DecapsulationKey768 {
    internal ж<mlkem.DecapsulationKey768> key;
}

// GenerateKey768 generates a new decapsulation key, drawing random bytes from
// the default crypto/rand source. The decapsulation key must be kept secret.
public static (ж<DecapsulationKey768>, error) GenerateKey768() {
    var (key, err) = mlkem.GenerateKey768();
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new DecapsulationKey768(key)), default!);
}

// NewDecapsulationKey768 expands a decapsulation key from a 64-byte seed in the
// "d || z" form. The seed must be uniformly random.
public static (ж<DecapsulationKey768>, error) NewDecapsulationKey768(slice<byte> seed) {
    var (key, err) = mlkem.NewDecapsulationKey768(seed);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new DecapsulationKey768(key)), default!);
}

// Bytes returns the decapsulation key as a 64-byte seed in the "d || z" form.
//
// The decapsulation key must be kept secret.
public static slice<byte> Bytes(this ref DecapsulationKey768 dk) {
    return dk.key.Bytes();
}

// Decapsulate generates a shared key from a ciphertext and a decapsulation
// key. If the ciphertext is not valid, Decapsulate returns an error.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, error err) Decapsulate(this ref DecapsulationKey768 dk, slice<byte> ciphertext) {
    return dk.key.Decapsulate(ciphertext);
}

// EncapsulationKey returns the public encapsulation key necessary to produce
// ciphertexts.
public static ж<EncapsulationKey768> EncapsulationKey(this ref DecapsulationKey768 dk) {
    return Ꮡ(new EncapsulationKey768(dk.key.EncapsulationKey()));
}

// An EncapsulationKey768 is the public key used to produce ciphertexts to be
// decapsulated by the corresponding DecapsulationKey768.
partial struct EncapsulationKey768 {
    internal ж<mlkem.EncapsulationKey768> key;
}

// NewEncapsulationKey768 parses an encapsulation key from its encoded form. If
// the encapsulation key is not valid, NewEncapsulationKey768 returns an error.
public static (ж<EncapsulationKey768>, error) NewEncapsulationKey768(slice<byte> encapsulationKey) {
    var (key, err) = mlkem.NewEncapsulationKey768(encapsulationKey);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new EncapsulationKey768(key)), default!);
}

// Bytes returns the encapsulation key as a byte slice.
public static slice<byte> Bytes(this ref EncapsulationKey768 ek) {
    return ek.key.Bytes();
}

// Encapsulate generates a shared key and an associated ciphertext from an
// encapsulation key, drawing random bytes from the default crypto/rand source.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, slice<byte> ciphertext) Encapsulate(this ref EncapsulationKey768 ek) {
    return ek.key.Encapsulate();
}

// DecapsulationKey1024 is the secret key used to decapsulate a shared key
// from a ciphertext. It includes various precomputed values.
partial struct DecapsulationKey1024 {
    internal ж<mlkem.DecapsulationKey1024> key;
}

// GenerateKey1024 generates a new decapsulation key, drawing random bytes from
// the default crypto/rand source. The decapsulation key must be kept secret.
public static (ж<DecapsulationKey1024>, error) GenerateKey1024() {
    var (key, err) = mlkem.GenerateKey1024();
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new DecapsulationKey1024(key)), default!);
}

// NewDecapsulationKey1024 expands a decapsulation key from a 64-byte seed in the
// "d || z" form. The seed must be uniformly random.
public static (ж<DecapsulationKey1024>, error) NewDecapsulationKey1024(slice<byte> seed) {
    var (key, err) = mlkem.NewDecapsulationKey1024(seed);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new DecapsulationKey1024(key)), default!);
}

// Bytes returns the decapsulation key as a 64-byte seed in the "d || z" form.
//
// The decapsulation key must be kept secret.
public static slice<byte> Bytes(this ref DecapsulationKey1024 dk) {
    return dk.key.Bytes();
}

// Decapsulate generates a shared key from a ciphertext and a decapsulation
// key. If the ciphertext is not valid, Decapsulate returns an error.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, error err) Decapsulate(this ref DecapsulationKey1024 dk, slice<byte> ciphertext) {
    return dk.key.Decapsulate(ciphertext);
}

// EncapsulationKey returns the public encapsulation key necessary to produce
// ciphertexts.
public static ж<EncapsulationKey1024> EncapsulationKey(this ref DecapsulationKey1024 dk) {
    return Ꮡ(new EncapsulationKey1024(dk.key.EncapsulationKey()));
}

// An EncapsulationKey1024 is the public key used to produce ciphertexts to be
// decapsulated by the corresponding DecapsulationKey1024.
partial struct EncapsulationKey1024 {
    internal ж<mlkem.EncapsulationKey1024> key;
}

// NewEncapsulationKey1024 parses an encapsulation key from its encoded form. If
// the encapsulation key is not valid, NewEncapsulationKey1024 returns an error.
public static (ж<EncapsulationKey1024>, error) NewEncapsulationKey1024(slice<byte> encapsulationKey) {
    var (key, err) = mlkem.NewEncapsulationKey1024(encapsulationKey);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new EncapsulationKey1024(key)), default!);
}

// Bytes returns the encapsulation key as a byte slice.
public static slice<byte> Bytes(this ref EncapsulationKey1024 ek) {
    return ek.key.Bytes();
}

// Encapsulate generates a shared key and an associated ciphertext from an
// encapsulation key, drawing random bytes from the default crypto/rand source.
//
// The shared key must be kept secret.
public static (slice<byte> sharedKey, slice<byte> ciphertext) Encapsulate(this ref EncapsulationKey1024 ek) {
    return ek.key.Encapsulate();
}

} // end mlkem_package
