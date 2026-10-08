// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using hmac = go.crypto.hmac_package;
using tls12 = go.crypto.@internal.fips140.tls12_package;
using md5 = go.crypto.md5_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using sha512 = go.crypto.sha512_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using fips140 = go.crypto.@internal.fips140_package;
using go.crypto;
using go.crypto.@internal.fips140;

partial class tls_package {

// type prfFunc is a methodless func type — rendered inline as its base delegate

// Split a premaster secret in two as specified in RFC 4346, Section 5.
internal static (slice<byte> s1, slice<byte> s2) splitPreMasterSecret(slice<byte> secret) {
    slice<byte> s1 = default!;
    slice<byte> s2 = default!;

    s1 = secret.slice(0, (len(secret) + 1) / 2);
    s2 = secret.slice(len(secret) / 2);
    return (s1, s2);
}

// pHash implements the P_hash function, as defined in RFC 4346, Section 5.
internal static void pHash(slice<byte> result, slice<byte> secret, slice<byte> seed, Func<hash.Hash> hashΔ1) {
    var h = hmac.New(hashΔ1, secret);
    h.Write(seed);
    var a = h.Sum(default!);
    nint j = 0;
    while (j < len(result)) {
        h.Reset();
        h.Write(a);
        h.Write(seed);
        var b = h.Sum(default!);
        copy(result.slice(j), b);
        j += len(b);
        h.Reset();
        h.Write(a);
        a = h.Sum(default!);
    }
}

// prf10 implements the TLS 1.0 pseudo-random function, as defined in RFC 2246, Section 5.
internal static slice<byte> prf10(slice<byte> secret, @string label, slice<byte> seed, nint keyLen) {
    var result = new slice<byte>(keyLen);
    var hashSHA1 = sha1.New;
    var hashMD5 = md5.New;
    var labelAndSeed = new slice<byte>(len(label) + len(seed));
    copy(labelAndSeed, label);
    copy(labelAndSeed.slice(len(label)), seed);
    var (s1, s2) = splitPreMasterSecret(secret);
    pHash(result, s1, labelAndSeed, hashMD5);
    var result2 = new slice<byte>(len(result));
    pHash(result2, s2, labelAndSeed, hashSHA1);
    foreach (var (i, b) in result2) {
        result[i] ^= (byte)(b);
    }
    return result;
}

// prf12 implements the TLS 1.2 pseudo-random function, as defined in RFC 5246, Section 5.
internal static Func<slice<byte>, @string, slice<byte>, nint, slice<byte>> prf12(Func<hash.Hash> hashFunc) {
    return (slice<byte> secret, @string label, slice<byte> seed, nint keyLen) => tls12.PRF(widen<hash.Hash, fips140.Hash>(hashFunc, elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), secret, label, seed, keyLen);
}

internal static UntypedInt masterSecretLength => 48; // Length of a master secret in TLS 1.1.
internal static UntypedInt finishedVerifyLength => 12; // Length of verify_data in a Finished message.

internal static readonly @string masterSecretLabel = "master secret"u8;

internal static readonly @string extendedMasterSecretLabel = "extended master secret"u8;

internal static readonly @string keyExpansionLabel = "key expansion"u8;

internal static readonly @string clientFinishedLabel = "client finished"u8;

internal static readonly @string serverFinishedLabel = "server finished"u8;

internal static (Func<slice<byte>, @string, slice<byte>, nint, slice<byte>>, crypto.Hash) prfAndHashForVersion(uint16 version, ref cipherSuite suite) {
    var exprᴛ1 = version;
    if (exprᴛ1 == VersionTLS10 || exprᴛ1 == VersionTLS11) {
        return (prf10, ((crypto.Hash)0));
    }
    if (exprᴛ1 == VersionTLS12) {
        if ((nint)(suite.flags & (nint)suiteSHA384) != 0) {
            return (prf12(sha512.New384), crypto.SHA384);
        }
        return (prf12(sha256.New), crypto.SHA256);
    }
    { /* default: */
        throw panic("unknown version");
    }

}

internal static Func<slice<byte>, @string, slice<byte>, nint, slice<byte>> prfForVersion(uint16 version, ref cipherSuite suite) {
    var (prf, _) = prfAndHashForVersion(version, ref suite);
    return prf;
}

// masterFromPreMasterSecret generates the master secret from the pre-master
// secret. See RFC 5246, Section 8.1.
internal static slice<byte> masterFromPreMasterSecret(uint16 version, ref cipherSuite suite, slice<byte> preMasterSecret, slice<byte> clientRandom, slice<byte> serverRandom) {
    var seed = new slice<byte>(0, len(clientRandom) + len(serverRandom));
    seed = appendꓸꓸꓸ(seed, clientRandom);
    seed = appendꓸꓸꓸ(seed, serverRandom);
    return prfForVersion(version, ref suite)(preMasterSecret, masterSecretLabel, seed, masterSecretLength);
}

// extMasterFromPreMasterSecret generates the extended master secret from the
// pre-master secret. See RFC 7627.
internal static slice<byte> extMasterFromPreMasterSecret(uint16 version, ref cipherSuite suite, slice<byte> preMasterSecret, slice<byte> transcript) {
    var (prf, hash) = prfAndHashForVersion(version, ref suite);
    if (version == VersionTLS12) {
        // Use the FIPS 140-3 module only for TLS 1.2 with EMS, which is the
        // only TLS 1.0-1.2 approved mode per IG D.Q.
        var hashʗ1 = hash;
        return tls12.MasterSecret<fips140.Hash>(widen<hash.Hash, fips140.Hash>(() => hashʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), preMasterSecret, transcript);
    }
    return prf(preMasterSecret, extendedMasterSecretLabel, transcript, masterSecretLength);
}

// keysFromMasterSecret generates the connection keys from the master
// secret, given the lengths of the MAC key, cipher key and IV, as defined in
// RFC 2246, Section 6.3.
internal static (slice<byte> clientMAC, slice<byte> serverMAC, slice<byte> clientKey, slice<byte> serverKey, slice<byte> clientIV, slice<byte> serverIV) keysFromMasterSecret(uint16 version, ref cipherSuite suite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom, nint macLen, nint keyLen, nint ivLen) {
    slice<byte> clientMAC = default!;
    slice<byte> serverMAC = default!;
    slice<byte> clientKey = default!;
    slice<byte> serverKey = default!;
    slice<byte> clientIV = default!;
    slice<byte> serverIV = default!;

    var seed = new slice<byte>(0, len(serverRandom) + len(clientRandom));
    seed = appendꓸꓸꓸ(seed, serverRandom);
    seed = appendꓸꓸꓸ(seed, clientRandom);
    nint n = 2 * macLen + 2 * keyLen + 2 * ivLen;
    var keyMaterial = prfForVersion(version, ref suite)(masterSecret, keyExpansionLabel, seed, n);
    clientMAC = keyMaterial.slice(0, macLen);
    keyMaterial = keyMaterial.slice(macLen);
    serverMAC = keyMaterial.slice(0, macLen);
    keyMaterial = keyMaterial.slice(macLen);
    clientKey = keyMaterial.slice(0, keyLen);
    keyMaterial = keyMaterial.slice(keyLen);
    serverKey = keyMaterial.slice(0, keyLen);
    keyMaterial = keyMaterial.slice(keyLen);
    clientIV = keyMaterial.slice(0, ivLen);
    keyMaterial = keyMaterial.slice(ivLen);
    serverIV = keyMaterial.slice(0, ivLen);
    return (clientMAC, serverMAC, clientKey, serverKey, clientIV, serverIV);
}

internal static ΔfinishedHash newFinishedHash(uint16 version, ref cipherSuite cipherSuite) {
    slice<byte> buffer = default!;
    if (version >= VersionTLS12) {
        buffer = new byte[]{}.slice();
    }
    var (prf, hash) = prfAndHashForVersion(version, ref cipherSuite);
    if (hash != 0) {
        return new ΔfinishedHash(hash.New(), hash.New(), default!, default!, buffer, version, prf);
    }
    return new ΔfinishedHash(sha1.New(), sha1.New(), md5.New(), md5.New(), buffer, version, prf);
}

// A finishedHash calculates the hash of a set of handshake messages suitable
// for including in a Finished message.
partial struct ΔfinishedHash {
    internal hash.Hash client;
    internal hash.Hash server;
    // Prior to TLS 1.2, an additional MD5 hash is required.
    internal hash.Hash clientMD5;
    internal hash.Hash serverMD5;
    // In TLS 1.2, a full buffer is sadly required.
    internal slice<byte> buffer;
    internal uint16 version;
    internal Func<slice<byte>, @string, slice<byte>, nint, slice<byte>> prf;
}

internal static (nint n, error err) Write(this ref ΔfinishedHash h, slice<byte> msg) {
    h.client.Write(msg);
    h.server.Write(msg);
    if (h.version < VersionTLS12) {
        h.clientMD5.Write(msg);
        h.serverMD5.Write(msg);
    }
    if (h.buffer != default!) {
        h.buffer = appendꓸꓸꓸ(h.buffer, msg);
    }
    return (len(msg), default!);
}

internal static slice<byte> Sum(this ΔfinishedHash h) {
    if (h.version >= VersionTLS12) {
        return h.client.Sum(default!);
    }
    var @out = new slice<byte>(0, md5.ΔSize + sha1.ΔSize);
    @out = h.clientMD5.Sum(@out);
    return h.client.Sum(@out);
}

// clientSum returns the contents of the verify_data member of a client's
// Finished message.
internal static slice<byte> clientSum(this ΔfinishedHash h, slice<byte> masterSecret) {
    return h.prf(masterSecret, clientFinishedLabel, h.Sum(), finishedVerifyLength);
}

// serverSum returns the contents of the verify_data member of a server's
// Finished message.
internal static slice<byte> serverSum(this ΔfinishedHash h, slice<byte> masterSecret) {
    return h.prf(masterSecret, serverFinishedLabel, h.Sum(), finishedVerifyLength);
}

// hashForClientCertificate returns the handshake messages so far, pre-hashed if
// necessary, suitable for signing by a TLS client certificate.
internal static slice<byte> hashForClientCertificate(this ΔfinishedHash h, uint8 sigType, crypto.Hash hashAlg) {
    if ((h.version >= VersionTLS12 || sigType == signatureEd25519) && h.buffer == default!) {
        throw panic("tls: handshake hash for a client certificate requested after discarding the handshake buffer");
    }
    if (sigType == signatureEd25519) {
        return h.buffer;
    }
    if (h.version >= VersionTLS12) {
        var hash = hashAlg.New();
        hash.Write(h.buffer);
        return hash.Sum(default!);
    }
    if (sigType == signatureECDSA) {
        return h.server.Sum(default!);
    }
    return h.Sum();
}

// discardHandshakeBuffer is called when there is no more need to
// buffer the entirety of the handshake messages.
internal static void discardHandshakeBuffer(this ref ΔfinishedHash h) {
    h.buffer = default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoTlsˢ = "crypto/tls: ExportKeyingMaterial is unavailable when renegotiation is enabled"u8;

// noEKMBecauseRenegotiation is used as a value of
// ConnectionState.ekm when renegotiation is enabled and thus
// we wish to fail all key-material export requests.
internal static (slice<byte>, error) noEKMBecauseRenegotiation(@string label, slice<byte> context, nint length) {
    return (default!, errors.New(cryptoTlsˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoTlsˢ2 = "crypto/tls: ExportKeyingMaterial is unavailable when neither TLS 1.3 nor Extended Master Secret are negotiated; override with GODEBUG=tlsunsafeekm=1"u8;

// noEKMBecauseNoEMS is used as a value of ConnectionState.ekm when Extended
// Master Secret is not negotiated and thus we wish to fail all key-material
// export requests.
internal static (slice<byte>, error) noEKMBecauseNoEMS(@string label, slice<byte> context, nint length) {
    return (default!, errors.New(cryptoTlsˢ2));
}

// ekmFromMasterSecret generates exported keying material as defined in RFC 5705.
internal static Func<@string, slice<byte>, nint, (slice<byte>, error)> ekmFromMasterSecret(uint16 version, ж<cipherSuite> Ꮡsuite, slice<byte> masterSecret, slice<byte> clientRandom, slice<byte> serverRandom) {
    var clientRandomʗ1 = clientRandom;
    var masterSecretʗ1 = masterSecret;
    var serverRandomʗ1 = serverRandom;
    return (@string label, slice<byte> context, nint length) => {
        var exprᴛ1 = label;
        if (exprᴛ1 == "client finished"u8 || exprᴛ1 == "server finished"u8 || exprᴛ1 == "master secret"u8 || exprᴛ1 == "key expansion"u8) {
            return (default!, fmt.Errorf("crypto/tls: reserved ExportKeyingMaterial label: %s"u8, // These values are reserved and may not be used.
 label));
        }

        nint seedLen = len(serverRandomʗ1) + len(clientRandomʗ1);
        if (context != default!) {
            seedLen += 2 + len(context);
        }
        var seed = new slice<byte>(0, seedLen);
        seed = appendꓸꓸꓸ(seed, clientRandomʗ1);
        seed = appendꓸꓸꓸ(seed, serverRandomʗ1);
        if (context != default!) {
            if (len(context) >= (1 << (int)(16))) {
                return (default!, fmt.Errorf("crypto/tls: ExportKeyingMaterial context too long"u8));
            }
            seed = append(seed, (byte)((len(context) >> (int)(8))), (byte)len(context));
            seed = appendꓸꓸꓸ(seed, context);
        }
        return (prfForVersion(version, ref (Ꮡsuite).DerefOrNull())(masterSecretʗ1, label, seed, length), default!);
    };
}

} // end tls_package
