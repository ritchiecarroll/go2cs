// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using ecdh = go.crypto.ecdh_package;
using fips140 = go.crypto.fips140_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using rand = go.crypto.@internal.rand_package;
using mlkem = go.crypto.mlkem_package;
using sha3 = go.crypto.sha3_package;
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using io = io_package;

partial class hpke_package {

internal static ж<hybridKEM> mlkem768X25519 = Ꮡ(new hybridKEM(
    id: 0x647a,
    label: @"\./"u8 +
        /*   */ @"/^\"u8,
    /**/
    curve: ecdh.X25519(),

    curveSeedSize: 32,
    curvePointSize: 32,
    pqEncapsKeySize: mlkem.EncapsulationKeySize768,
    pqCiphertextSize: mlkem.CiphertextSize768,

    pqNewPublicKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewEncapsulationKey768(data);
        return (new mlkem.EncapsulationKey768жEncapsulator(ᴛ1), ᴛ2);
    },
    pqNewPrivateKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewDecapsulationKey768(data);
        return (new mlkem.DecapsulationKey768жDecapsulator(ᴛ1), ᴛ2);
    }
));

// MLKEM768X25519 returns a KEM implementing MLKEM768-X25519 (a.k.a. X-Wing)
// from draft-ietf-hpke-pq.
public static ΔKEM MLKEM768X25519() {
    return new hybridKEMжΔKEM(mlkem768X25519);
}

internal static ж<hybridKEM> mlkem768P256 = Ꮡ(new hybridKEM(
    id: 0x0050,
    label: "MLKEM768-P256"u8,
    curve: ecdh.P256(),

    curveSeedSize: 32,
    curvePointSize: 65,
    pqEncapsKeySize: mlkem.EncapsulationKeySize768,
    pqCiphertextSize: mlkem.CiphertextSize768,

    pqNewPublicKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewEncapsulationKey768(data);
        return (new mlkem.EncapsulationKey768жEncapsulator(ᴛ1), ᴛ2);
    },
    pqNewPrivateKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewDecapsulationKey768(data);
        return (new mlkem.DecapsulationKey768жDecapsulator(ᴛ1), ᴛ2);
    }
));

// MLKEM768P256 returns a KEM implementing MLKEM768-P256 from draft-ietf-hpke-pq.
public static ΔKEM MLKEM768P256() {
    return new hybridKEMжΔKEM(mlkem768P256);
}

internal static ж<hybridKEM> mlkem1024P384 = Ꮡ(new hybridKEM(
    id: 0x0051,
    label: "MLKEM1024-P384"u8,
    curve: ecdh.P384(),

    curveSeedSize: 48,
    curvePointSize: 97,
    pqEncapsKeySize: mlkem.EncapsulationKeySize1024,
    pqCiphertextSize: mlkem.CiphertextSize1024,

    pqNewPublicKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewEncapsulationKey1024(data);
        return (new mlkem.EncapsulationKey1024жEncapsulator(ᴛ1), ᴛ2);
    },
    pqNewPrivateKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewDecapsulationKey1024(data);
        return (new mlkem.DecapsulationKey1024жDecapsulator(ᴛ1), ᴛ2);
    }
));

// MLKEM1024P384 returns a KEM implementing MLKEM1024-P384 from draft-ietf-hpke-pq.
public static ΔKEM MLKEM1024P384() {
    return new hybridKEMжΔKEM(mlkem1024P384);
}

partial struct hybridKEM {
    internal uint16 id;
    internal @string label;
    internal ecdhꓸCurve curve;
    internal nint curveSeedSize;
    internal nint curvePointSize;
    internal nint pqEncapsKeySize;
    internal nint pqCiphertextSize;
    internal Func<slice<byte>, (crypto.Encapsulator, error)> pqNewPublicKey;
    internal Func<slice<byte>, (crypto.Decapsulator, error)> pqNewPrivateKey;
}

internal static uint16 ID(this ref hybridKEM kem) {
    return kem.id;
}

internal static nint encSize(this ref hybridKEM kem) {
    return kem.pqCiphertextSize + kem.curvePointSize;
}

internal static slice<byte> sharedSecret(this ref hybridKEM kem, slice<byte> ssPQ, slice<byte> ssT, slice<byte> ctT, slice<byte> ekT) {
    var h = sha3.New256();
    h.Write(ssPQ);
    h.Write(ssT);
    h.Write(ctT);
    h.Write(ekT);
    h.Write(slice<byte>(kem.label));
    return h.Sum(default);
}

partial struct hybridPublicKey {
    internal ж<hybridKEM> kem;
    internal ж<ecdhꓸPublicKey> t;
    internal crypto.Encapsulator pq;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPqKemForX25519ˢ = "invalid PQ KEM for X25519 hybrid"u8;
internal static readonly @string invalidPqKemForP256ˢ = "invalid PQ KEM for P-256 hybrid"u8;
internal static readonly @string invalidPqKemForP384ˢ = "invalid PQ KEM for P-384 hybrid"u8;

// NewHybridPublicKey returns a PublicKey implementing one of
//
//   - MLKEM768-X25519 (a.k.a. X-Wing)
//   - MLKEM768-P256
//   - MLKEM1024-P384
//
// from draft-ietf-hpke-pq, depending on the underlying curve of t
// ([ecdh.X25519], [ecdh.P256], or [ecdh.P384]) and the type of pq (either
// *[mlkem.EncapsulationKey768] or *[mlkem.EncapsulationKey1024]).
//
// This function is meant for applications that already have instantiated
// crypto/ecdh and crypto/mlkem public keys. Otherwise, applications should use
// the [KEM.NewPublicKey] method of e.g. [MLKEM768X25519].
public static (ΔPublicKey, error) NewHybridPublicKey(crypto.Encapsulator pq, ж<ecdhꓸPublicKey> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    switch (t.Curve()) {
    case var ᴛ when AreEqual(ᴛ, ecdh.X25519()): {
        {
            var (_, ok) = pq._<ж<mlkem.EncapsulationKey768>>(ᐧ); if (!ok) {
                return (default, errors.New(invalidPqKemForX25519ˢ));
            }
        }
        return (new hybridPublicKeyжΔPublicKey(Ꮡ(new hybridPublicKey(mlkem768X25519, Ꮡt, pq))), default);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.P256()): {
        {
            var (_, ok) = pq._<ж<mlkem.EncapsulationKey768>>(ᐧ); if (!ok) {
                return (default, errors.New(invalidPqKemForP256ˢ));
            }
        }
        return (new hybridPublicKeyжΔPublicKey(Ꮡ(new hybridPublicKey(mlkem768P256, Ꮡt, pq))), default);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.P384()): {
        {
            var (_, ok) = pq._<ж<mlkem.EncapsulationKey1024>>(ᐧ); if (!ok) {
                return (default, errors.New(invalidPqKemForP384ˢ));
            }
        }
        return (new hybridPublicKeyжΔPublicKey(Ꮡ(new hybridPublicKey(mlkem1024P384, Ꮡt, pq))), default);
    }
    default: {
        return (default, errors.New(unsupportedCurveˢ));
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPublicKeySizeˢ = "invalid public key size"u8;

internal static (ΔPublicKey, error) NewPublicKey(this ж<hybridKEM> Ꮡkem, slice<byte> data) {
    ref var kem = ref Ꮡkem.DerefOrNull();

    if (len(data) != kem.pqEncapsKeySize + kem.curvePointSize) {
        return (default, errors.New(invalidPublicKeySizeˢ));
    }
    ref var err = ref heap<error>(out var Ꮡerr);
    (var pq, err) = kem.pqNewPublicKey(data.slice(0, kem.pqEncapsKeySize));
    if (err != nil) {
        return (default, err);
    }
    ref var k = ref heap<ж<ecdhꓸPublicKey>>(out var Ꮡk);
    var dataʗ1 = data;
    fips140.WithoutEnforcement(() => { // Hybrid of ML-KEM, which is Approved.
        (Ꮡk.ValueSlot, Ꮡerr.ValueSlot) = Ꮡkem.Value.curve.NewPublicKey(dataʗ1.slice(Ꮡkem.Value.pqEncapsKeySize));
    });
    if (err != nil) {
        return (default, err);
    }
    return NewHybridPublicKey(pq, k);
}

internal static ΔKEM KEM(this ref hybridPublicKey pk) {
    return new hybridKEMжΔKEM(pk.kem);
}

internal static slice<byte> Bytes(this ref hybridPublicKey pk) {
    return appendꓸꓸꓸ(pk.pq.Bytes(), pk.t.Bytes());
}

internal static Func<(slice<byte>, slice<byte>)> testingOnlyEncapsulate;

internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) encap(this ж<hybridPublicKey> Ꮡpk) {
    error err = default;
    ref var pk = ref Ꮡpk.DerefOrNull();

    ref var skE = ref heap<ж<ecdh.PrivateKey>>(out var ᏑskE);
    fips140.WithoutEnforcement(() => { // Hybrid of ML-KEM, which is Approved.
        (ᏑskE.ValueSlot, err) = Ꮡpk.Value.t.Curve().GenerateKey(rand.Reader);
    });
    if (err != nil) {
        return (default, default, err);
    }
    if (testingOnlyGenerateKey != nil) {
        skE = testingOnlyGenerateKey();
    }
    ref var ssT = ref heap<slice<byte>>(out var ᏑssT);
    fips140.WithoutEnforcement(() => {
        (ᏑssT.ValueSlot, err) = ᏑskE.ValueSlot.ECDH(Ꮡpk.Value.t);
    });
    if (err != nil) {
        return (default, default, err);
    }
    var ctT = skE.PublicKey().Bytes();

    var (ssPQ, ctPQ) = pk.pq.Encapsulate();
    if (testingOnlyEncapsulate != nil) {
        (ssPQ, ctPQ) = testingOnlyEncapsulate();
    }

    var ss = pk.kem.sharedSecret(ssPQ, ssT, ctT, pk.t.Bytes());
    var ct = appendꓸꓸꓸ(ctPQ, ctT);
    return (ss, ct, default);
}

partial struct hybridPrivateKey {
    internal ж<hybridKEM> kem;
    internal slice<byte> seed; // can be nil
    internal ecdh.KeyExchanger t;
    internal crypto.Decapsulator pq;
}

// NewHybridPrivateKey returns a PrivateKey implementing
//
//   - MLKEM768-X25519 (a.k.a. X-Wing)
//   - MLKEM768-P256
//   - MLKEM1024-P384
//
// from draft-ietf-hpke-pq, depending on the underlying curve of t
// ([ecdh.X25519], [ecdh.P256], or [ecdh.P384]) and the type of pq.Encapsulator()
// (either *[mlkem.EncapsulationKey768] or *[mlkem.EncapsulationKey1024]).
//
// This function is meant for applications that already have instantiated
// crypto/ecdh and crypto/mlkem private keys, or another implementation of a
// [ecdh.KeyExchanger] and [crypto.Decapsulator] (e.g. a hardware key).
// Otherwise, applications should use the [KEM.NewPrivateKey] method of e.g.
// [MLKEM768X25519].
public static (PrivateKey, error) NewHybridPrivateKey(crypto.Decapsulator pq, ecdh.KeyExchanger t) {
    return newHybridPrivateKey(pq, t, default);
}

internal static (PrivateKey, error) GenerateKey(this ж<hybridKEM> Ꮡkem) {
    var seed = new slice<byte>(32);
    drbg.Read(seed);
    return Ꮡkem.NewPrivateKey(seed);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hpkeInvalidHybridKemˢ = "hpke: invalid hybrid KEM secret length"u8;

internal static (PrivateKey, error) NewPrivateKey(this ж<hybridKEM> Ꮡkem, slice<byte> priv) {
    ref var kem = ref Ꮡkem.DerefOrNull();

    if (len(priv) != 32) {
        return (default, errors.New(hpkeInvalidHybridKemˢ));
    }

    var s = sha3.NewSHAKE256();
    s.Write(priv);

    var seedPQ = new slice<byte>(mlkem.SeedSize);
    s.Read(seedPQ);
    ref var err = ref heap<error>(out var Ꮡerr);
    (var pq, err) = kem.pqNewPrivateKey(seedPQ);
    if (err != nil) {
        return (default, err);
    }

    var seedT = new slice<byte>(kem.curveSeedSize);
    for (;;) {
        s.Read(seedT);
        ref var k = ref heap<ecdh.KeyExchanger>(out var Ꮡk);
        var seedTʗ1 = seedT;
        fips140.WithoutEnforcement(() => { // Hybrid of ML-KEM, which is Approved.
            var (ᴛ1, ᴛ2) = Ꮡkem.Value.curve.NewPrivateKey(seedTʗ1);
            (Ꮡk.ValueSlot, Ꮡerr.ValueSlot) = (new ecdh.PrivateKeyжKeyExchanger(ᴛ1), ᴛ2);
        });
        if (err != nil) {
            continue;
        }
        return newHybridPrivateKey(pq, k, priv);
    }
}

internal static (PrivateKey, error) newHybridPrivateKey(crypto.Decapsulator pq, ecdh.KeyExchanger t, slice<byte> seed) {
    switch (t.Curve()) {
    case var ᴛ when AreEqual(ᴛ, ecdh.X25519()): {
        {
            var (_, ok) = pq.Encapsulator()._<ж<mlkem.EncapsulationKey768>>(ᐧ); if (!ok) {
                return (default, errors.New(invalidPqKemForX25519ˢ));
            }
        }
        return (new hybridPrivateKeyжPrivateKey(Ꮡ(new hybridPrivateKey(mlkem768X25519, bytes.Clone(seed), t, pq))), default);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.P256()): {
        {
            var (_, ok) = pq.Encapsulator()._<ж<mlkem.EncapsulationKey768>>(ᐧ); if (!ok) {
                return (default, errors.New(invalidPqKemForP256ˢ));
            }
        }
        return (new hybridPrivateKeyжPrivateKey(Ꮡ(new hybridPrivateKey(mlkem768P256, bytes.Clone(seed), t, pq))), default);
    }
    case var ᴛ when AreEqual(ᴛ, ecdh.P384()): {
        {
            var (_, ok) = pq.Encapsulator()._<ж<mlkem.EncapsulationKey1024>>(ᐧ); if (!ok) {
                return (default, errors.New(invalidPqKemForP384ˢ));
            }
        }
        return (new hybridPrivateKeyжPrivateKey(Ꮡ(new hybridPrivateKey(mlkem1024P384, bytes.Clone(seed), t, pq))), default);
    }
    default: {
        return (default, errors.New(unsupportedCurveˢ));
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string deriveKeyPairˢ = "DeriveKeyPair"u8;

internal static (PrivateKey, error) DeriveKeyPair(this ж<hybridKEM> Ꮡkem, slice<byte> ikm) {
    ref var kem = ref Ꮡkem.DerefOrNull();

    var suiteID = byteorder.BEAppendUint16(slice<byte>("KEM"u8), kem.id);
    var (dk, err) = SHAKE256().labeledDerive(suiteID, ikm, deriveKeyPairˢ, default, 32);
    if (err != nil) {
        return (default, err);
    }
    return Ꮡkem.NewPrivateKey(dk);
}

internal static ΔKEM KEM(this ref hybridPrivateKey k) {
    return new hybridKEMжΔKEM(k.kem);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string privateKeySeedNotˢ = "private key seed not available"u8;

internal static (slice<byte>, error) Bytes(this ref hybridPrivateKey k) {
    if (k.seed == nil) {
        return (default, errors.New(privateKeySeedNotˢ));
    }
    return (k.seed, default);
}

internal static ΔPublicKey PublicKey(this ref hybridPrivateKey k) {
    return new hybridPublicKeyжΔPublicKey(Ꮡ(new hybridPublicKey(
        kem: k.kem,
        t: k.t.PublicKey(),
        pq: k.pq.Encapsulator()
    )));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidEncapsulatedKeyˢ = "invalid encapsulated key size"u8;

internal static (slice<byte>, error) decap(this ж<hybridPrivateKey> Ꮡk, slice<byte> enc) {
    ref var k = ref Ꮡk.DerefOrNull();

    if (len(enc) != k.kem.pqCiphertextSize + k.kem.curvePointSize) {
        return (default, errors.New(invalidEncapsulatedKeyˢ));
    }
    var (ctPQ, ctT) = (enc.slice(0, k.kem.pqCiphertextSize), enc.slice(k.kem.pqCiphertextSize));
    ref var err = ref heap<error>(out var Ꮡerr);
    (var ssPQ, err) = k.pq.Decapsulate(ctPQ);
    if (err != nil) {
        return (default, err);
    }
    ref var pub = ref heap<ж<ecdhꓸPublicKey>>(out var Ꮡpub);
    var ctTʗ1 = ctT;
    fips140.WithoutEnforcement(() => { // Hybrid of ML-KEM, which is Approved.
        (Ꮡpub.ValueSlot, Ꮡerr.ValueSlot) = Ꮡk.Value.t.Curve().NewPublicKey(ctTʗ1);
    });
    if (err != nil) {
        return (default, err);
    }
    ref var ssT = ref heap<slice<byte>>(out var ᏑssT);
    fips140.WithoutEnforcement(() => {
        (ᏑssT.ValueSlot, Ꮡerr.ValueSlot) = Ꮡk.Value.t.ECDH(Ꮡpub.ValueSlot);
    });
    if (err != nil) {
        return (default, err);
    }
    var ss = k.kem.sharedSecret(ssPQ, ssT, ctT, k.t.PublicKey().Bytes());
    return (ss, default);
}

[GoLazy] partial class lazyᴛmlkem768 { internal static ж<mlkemKEM> mlkem768 = Ꮡ(new mlkemKEM(
    id: 0x0041,
    ciphertextSize: mlkem.CiphertextSize768,
    newPublicKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewEncapsulationKey768(data);
        return (new mlkem.EncapsulationKey768жEncapsulator(ᴛ1), ᴛ2);
    },
    newPrivateKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewDecapsulationKey768(data);
        return (new mlkem.DecapsulationKey768жDecapsulator(ᴛ1), ᴛ2);
    },
    generateKey: () => {
        var (ᴛ1, ᴛ2) = mlkem.GenerateKey768();
        return (new mlkem.DecapsulationKey768жDecapsulator(ᴛ1), ᴛ2);
    }
)); }

// MLKEM768 returns a KEM implementing ML-KEM-768 from draft-ietf-hpke-pq.
public static ΔKEM MLKEM768() {
    return new mlkemKEMжΔKEM(mlkem768);
}

[GoLazy] partial class lazyᴛmlkem1024 { internal static ж<mlkemKEM> mlkem1024 = Ꮡ(new mlkemKEM(
    id: 0x0042,
    ciphertextSize: mlkem.CiphertextSize1024,
    newPublicKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewEncapsulationKey1024(data);
        return (new mlkem.EncapsulationKey1024жEncapsulator(ᴛ1), ᴛ2);
    },
    newPrivateKey: (slice<byte> data) => {
        var (ᴛ1, ᴛ2) = mlkem.NewDecapsulationKey1024(data);
        return (new mlkem.DecapsulationKey1024жDecapsulator(ᴛ1), ᴛ2);
    },
    generateKey: () => {
        var (ᴛ1, ᴛ2) = mlkem.GenerateKey1024();
        return (new mlkem.DecapsulationKey1024жDecapsulator(ᴛ1), ᴛ2);
    }
)); }

// MLKEM1024 returns a KEM implementing ML-KEM-1024 from draft-ietf-hpke-pq.
public static ΔKEM MLKEM1024() {
    return new mlkemKEMжΔKEM(mlkem1024);
}

partial struct mlkemKEM {
    internal uint16 id;
    internal nint ciphertextSize;
    internal Func<slice<byte>, (crypto.Encapsulator, error)> newPublicKey;
    internal Func<slice<byte>, (crypto.Decapsulator, error)> newPrivateKey;
    internal Func<(crypto.Decapsulator, error)> generateKey;
}

internal static uint16 ID(this ref mlkemKEM kem) {
    return kem.id;
}

internal static nint encSize(this ref mlkemKEM kem) {
    return kem.ciphertextSize;
}

partial struct mlkemPublicKey {
    internal ж<mlkemKEM> kem;
    internal crypto.Encapsulator pq;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedPublicKeyTypeˢ = "unsupported public key type"u8;

// NewMLKEMPublicKey returns a KEMPublicKey implementing
//
//   - ML-KEM-768
//   - ML-KEM-1024
//
// from draft-ietf-hpke-pq, depending on the type of pub
// (*[mlkem.EncapsulationKey768] or *[mlkem.EncapsulationKey1024]).
//
// This function is meant for applications that already have an instantiated
// crypto/mlkem public key. Otherwise, applications should use the
// [KEM.NewPublicKey] method of e.g. [MLKEM768].
public static (ΔPublicKey, error) NewMLKEMPublicKey(crypto.Encapsulator pub) {
    switch (pub.type()) {
    case ж<mlkem.EncapsulationKey768>: {
        return (new mlkemPublicKeyжΔPublicKey(Ꮡ(new mlkemPublicKey(mlkem768, pub))), default);
    }
    case ж<mlkem.EncapsulationKey1024>: {
        return (new mlkemPublicKeyжΔPublicKey(Ꮡ(new mlkemPublicKey(mlkem1024, pub))), default);
    }
    default: {
        return (default, errors.New(unsupportedPublicKeyTypeˢ));
    }}
}

internal static (ΔPublicKey, error) NewPublicKey(this ref mlkemKEM kem, slice<byte> data) {
    var (pq, err) = kem.newPublicKey(data);
    if (err != nil) {
        return (default, err);
    }
    return NewMLKEMPublicKey(pq);
}

internal static ΔKEM KEM(this ref mlkemPublicKey pk) {
    return new mlkemKEMжΔKEM(pk.kem);
}

internal static slice<byte> Bytes(this ref mlkemPublicKey pk) {
    return pk.pq.Bytes();
}

internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) encap(this ref mlkemPublicKey pk) {
    var (ss, ct) = pk.pq.Encapsulate();
    if (testingOnlyEncapsulate != nil) {
        (ss, ct) = testingOnlyEncapsulate();
    }
    return (ss, ct, default);
}

partial struct mlkemPrivateKey {
    internal ж<mlkemKEM> kem;
    internal crypto.Decapsulator pq;
}

// NewMLKEMPrivateKey returns a KEMPrivateKey implementing
//
//   - ML-KEM-768
//   - ML-KEM-1024
//
// from draft-ietf-hpke-pq, depending on the type of priv.Encapsulator()
// (either *[mlkem.EncapsulationKey768] or *[mlkem.EncapsulationKey1024]).
//
// This function is meant for applications that already have an instantiated
// crypto/mlkem private key. Otherwise, applications should use the
// [KEM.NewPrivateKey] method of e.g. [MLKEM768].
public static (PrivateKey, error) NewMLKEMPrivateKey(crypto.Decapsulator priv) {
    switch (priv.Encapsulator().type()) {
    case ж<mlkem.EncapsulationKey768>: {
        return (new mlkemPrivateKeyжPrivateKey(Ꮡ(new mlkemPrivateKey(mlkem768, priv))), default);
    }
    case ж<mlkem.EncapsulationKey1024>: {
        return (new mlkemPrivateKeyжPrivateKey(Ꮡ(new mlkemPrivateKey(mlkem1024, priv))), default);
    }
    default: {
        return (default, errors.New(unsupportedPublicKeyTypeˢ));
    }}
}

internal static (PrivateKey, error) GenerateKey(this ref mlkemKEM kem) {
    var (pq, err) = kem.generateKey();
    if (err != nil) {
        return (default, err);
    }
    return NewMLKEMPrivateKey(pq);
}

internal static (PrivateKey, error) NewPrivateKey(this ref mlkemKEM kem, slice<byte> priv) {
    var (pq, err) = kem.newPrivateKey(priv);
    if (err != nil) {
        return (default, err);
    }
    return NewMLKEMPrivateKey(pq);
}

internal static (PrivateKey, error) DeriveKeyPair(this ref mlkemKEM kem, slice<byte> ikm) {
    var suiteID = byteorder.BEAppendUint16(slice<byte>("KEM"u8), kem.id);
    var (dk, err) = SHAKE256().labeledDerive(suiteID, ikm, deriveKeyPairˢ, default, 64);
    if (err != nil) {
        return (default, err);
    }
    return kem.NewPrivateKey(dk);
}

internal static ΔKEM KEM(this ref mlkemPrivateKey k) {
    return new mlkemKEMжΔKEM(k.kem);
}

internal partial interface Bytes_type /*dyn*/ {
    slice<byte> Bytes();
}

internal static (slice<byte>, error) Bytes(this ref mlkemPrivateKey k) {
    var (pq, ok) = k.pq._<Bytes_type>(ᐧ);
    if (!ok) {
        return (default, errors.New(privateKeySeedNotˢ));
    }
    return (pq.Bytes(), default);
}

internal static ΔPublicKey PublicKey(this ref mlkemPrivateKey k) {
    return new mlkemPublicKeyжΔPublicKey(Ꮡ(new mlkemPublicKey(
        kem: k.kem,
        pq: k.pq.Encapsulator()
    )));
}

internal static (slice<byte>, error) decap(this ref mlkemPrivateKey k, slice<byte> enc) {
    return k.pq.Decapsulate(enc);
}

} // end hpke_package
