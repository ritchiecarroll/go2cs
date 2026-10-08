// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using ecdh = go.crypto.ecdh_package;
using hmac = go.crypto.hmac_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using errors = errors_package;
using hash = hash_package;
using io = io_package;
using fips140 = go.crypto.@internal.fips140_package;
using go.crypto;
using go.crypto.@internal.fips140;

partial class tls_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trafficUpdˢ = "traffic upd"u8;

// This file contains the functions necessary to compute the TLS 1.3 key
// schedule. See RFC 8446, Section 7.

// nextTrafficSecret generates the next traffic secret, given the current one,
// according to RFC 8446, Section 7.2.
internal static slice<byte> nextTrafficSecret(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> trafficSecret) {
    ref var c = ref Ꮡc.DerefOrNull();

    var recvʗ1 = c.hash;
    return tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), trafficSecret, trafficUpdˢ, default!, c.hash.Size());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyˢ = "key"u8;

// trafficKey generates traffic keys according to RFC 8446, Section 7.3.
internal static (slice<byte> key, slice<byte> iv) trafficKey(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> trafficSecret) {
    slice<byte> key = default!;
    slice<byte> iv = default!;

    ref var c = ref Ꮡc.DerefOrNull();
    var recvʗ1 = c.hash;
    key = tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), trafficSecret, keyˢ, default!, c.keyLen);
    var recvʗ2 = c.hash;
    iv = tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ2.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), trafficSecret, "iv"u8, default!, aeadNonceLength);
    return (key, iv);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string finishedˢ = "finished"u8;

// finishedHash generates the Finished verify_data or PskBinderEntry according
// to RFC 8446, Section 4.4.4. See sections 4.4 and 4.2.11.2 for the baseKey
// selection.
internal static slice<byte> finishedHash(this ж<cipherSuiteTLS13> Ꮡc, slice<byte> baseKey, hash.Hash transcript) {
    ref var c = ref Ꮡc.DerefOrNull();

    var recvʗ1 = c.hash;
    var finishedKey = tls13.ExpandLabel<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), baseKey, finishedˢ, default!, c.hash.Size());
    var recvʗ2 = c.hash;
    var verifyData = hmac.New(() => recvʗ2.New(), finishedKey);
    verifyData.Write(transcript.Sum(default!));
    return verifyData.Sum(default!);
}

// exportKeyingMaterial implements RFC5705 exporters for TLS 1.3 according to
// RFC 8446, Section 7.5.
internal static Func<@string, slice<byte>, nint, (slice<byte>, error)> exportKeyingMaterial(this ref cipherSuiteTLS13 c, ж<tls13ꓸMasterSecret> Ꮡs, hash.Hash transcript) {
    ref var s = ref Ꮡs.DerefOrNull();

    var expMasterSecret = s.ExporterMasterSecret(new hash_HashᴠHash(transcript));
    var expMasterSecretʗ1 = expMasterSecret;
    return (@string label, slice<byte> context, nint length) => (expMasterSecretʗ1.Exporter(label, context, length), default!);
}

partial struct keySharePrivateKeys {
    internal CurveID curveID;
    internal ж<ecdh.PrivateKey> ecdhe;
    internal ж<mlkem.DecapsulationKey768> mlkem;
}

internal static UntypedInt x25519PublicKeySize => 32;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsInternalErrorˢ6 = "tls: internal error: unsupported curve"u8;

// generateECDHEKey returns a PrivateKey that implements Diffie-Hellman
// according to RFC 8446, Section 4.2.8.2.
internal static (ж<ecdh.PrivateKey>, error) generateECDHEKey(io.Reader rand, CurveID curveID) {
    var (curve, ok) = curveForCurveID(curveID);
    if (!ok) {
        return (default!, errors.New(tlsInternalErrorˢ6));
    }
    return curve.GenerateKey(rand);
}

internal static (ecdhꓸCurve, bool) curveForCurveID(CurveID id) {
    var exprᴛ1 = id;
    if (exprᴛ1 == X25519) {
        return (ecdh.X25519(), true);
    }
    if (exprᴛ1 == CurveP256) {
        return (ecdh.P256(), true);
    }
    if (exprᴛ1 == CurveP384) {
        return (ecdh.P384(), true);
    }
    if (exprᴛ1 == CurveP521) {
        return (ecdh.P521(), true);
    }
    { /* default: */
        return (default!, false);
    }

}

internal static (CurveID, bool) curveIDForCurve(ecdhꓸCurve curve) {
    var exprᴛ1 = curve;
    if (AreEqual(exprᴛ1, ecdh.X25519())) {
        return (X25519, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P256())) {
        return (CurveP256, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P384())) {
        return (CurveP384, true);
    }
    if (AreEqual(exprᴛ1, ecdh.P521())) {
        return (CurveP521, true);
    }
    { /* default: */
        return (0, false);
    }

}

} // end tls_package
