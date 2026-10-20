// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using ecdh = go.crypto.ecdh_package;
using md5 = go.crypto.md5_package;
using rsa = go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using Δx509 = go.crypto.x509_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using go.@internal;
using go.crypto;
using hash = hash_package;

partial class tls_package {

// A keyAgreement implements the client and server side of a TLS 1.0–1.2 key
// agreement protocol by generating and processing key exchange messages.
partial interface keyAgreement {
// On the server side, the first two methods are called in order.

    // In the case that the key agreement protocol doesn't use a
    // ServerKeyExchange message, generateServerKeyExchange can return nil,
    // nil.
    (ж<serverKeyExchangeMsg>, error) generateServerKeyExchange(ж<Config> _Δp0, ж<Certificate> _Δp1, ж<clientHelloMsg> _Δp2, ж<serverHelloMsg> _Δp3);
    (slice<byte>, error) processClientKeyExchange(ж<Config> _Δp0, ж<Certificate> _Δp1, ж<clientKeyExchangeMsg> _Δp2, uint16 _Δp3);
// On the client side, the next two methods are called in order.

    // This method may not be called if the server doesn't send a
    // ServerKeyExchange message.
    error processServerKeyExchange(ж<Config> _Δp0, ж<clientHelloMsg> _Δp1, ж<serverHelloMsg> _Δp2, ж<Δx509.Certificate> _Δp3, ж<serverKeyExchangeMsg> _Δp4);
    (slice<byte>, ж<clientKeyExchangeMsg>, error) generateClientKeyExchange(ж<Config> _Δp0, ж<clientHelloMsg> _Δp1, ж<Δx509.Certificate> _Δp2);
}

internal static error errClientKeyExchange = errors.New("tls: invalid ClientKeyExchange message"u8);

internal static error errServerKeyExchange = errors.New("tls: invalid ServerKeyExchange message"u8);

// rsaKeyAgreement implements the standard TLS key agreement where the client
// encrypts the pre-master secret to the server's public key.
partial struct rsaKeyAgreement {
}

internal static (ж<serverKeyExchangeMsg>, error) generateServerKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> Ꮡhello) {
    return (default!, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsCertificatePrivateKeyˢ = "tls: certificate private key does not implement crypto.Decrypter"u8;

internal static (slice<byte>, error) processClientKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientKeyExchangeMsg> Ꮡckx, uint16 version) {
    ref var config = ref Ꮡconfig.DerefOrNull();
    ref var cert = ref Ꮡcert.DerefOrNull();
    ref var ckx = ref Ꮡckx.DerefOrNull();

    if (len(ckx.ciphertext) < 2) {
        return (default!, errClientKeyExchange);
    }
    nint ciphertextLen = (nint)ckx.ciphertext[0] << 8 | (nint)ckx.ciphertext[1];
    if (ciphertextLen != len(ckx.ciphertext) - 2) {
        return (default!, errClientKeyExchange);
    }
    var ciphertext = ckx.ciphertext[2..];

    var (priv, ok) = cert.PrivateKey._<crypto.Decrypter>(ᐧ);
    if (!ok) {
        return (default!, errors.New(tlsCertificatePrivateKeyˢ));
    }
    // Perform constant time RSA PKCS #1 v1.5 decryption
    var (preMasterSecret, err) = priv.Decrypt(config.rand(), ciphertext, Ꮡ(new rsa.PKCS1v15DecryptOptions(SessionKeyLen: 48)));
    if (err != default!) {
        return (default!, err);
    }
    // We don't check the version number in the premaster secret. For one,
    // by checking it, we would leak information about the validity of the
    // encrypted pre-master secret. Secondly, it provides only a small
    // benefit against a downgrade attack and some implementations send the
    // wrong version anyway. See the discussion at the end of section
    // 7.4.7.1 of RFC 4346.
    return (preMasterSecret, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsUnexpectedˢ = "tls: unexpected ServerKeyExchange"u8;

internal static error processServerKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> ᏑserverHello, ж<Δx509.Certificate> Ꮡcert, ж<serverKeyExchangeMsg> Ꮡskx) {
    return errors.New(tlsUnexpectedˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerCertificateˢ = "tls: server certificate contains incorrect key type for selected ciphersuite"u8;

internal static (slice<byte>, ж<clientKeyExchangeMsg>, error) generateClientKeyExchange(this rsaKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<Δx509.Certificate> Ꮡcert) {
    ref var config = ref Ꮡconfig.DerefOrNull();
    ref var clientHello = ref ᏑclientHello.DerefOrNull();
    ref var cert = ref Ꮡcert.DerefOrNull();

    var preMasterSecret = new slice<byte>(48);
    preMasterSecret[0] = (byte)(clientHello.vers >> 8);
    preMasterSecret[1] = (byte)clientHello.vers;
    var (_, err) = io.ReadFull(config.rand(), preMasterSecret[2..]);
    if (err != default!) {
        return (default!, default!, err);
    }

    var (rsaKey, ok) = cert.PublicKey._<ж<rsa.PublicKey>>(ᐧ);
    if (!ok) {
        return (default!, default!, errors.New(tlsServerCertificateˢ));
    }
    (var encrypted, err) = rsa.EncryptPKCS1v15(config.rand(), rsaKey, preMasterSecret);
    if (err != default!) {
        return (default!, default!, err);
    }
    var ckx = @new<clientKeyExchangeMsg>();
    ckx.Value.ciphertext = new slice<byte>(len(encrypted) + 2);
    ckx.Value.ciphertext[0] = (byte)(len(encrypted) >> 8);
    ckx.Value.ciphertext[1] = (byte)len(encrypted);
    copy((~ckx).ciphertext[2..], encrypted);
    return (preMasterSecret, ckx, default!);
}

// sha1Hash calculates a SHA1 hash over the given byte slices.
internal static slice<byte> sha1Hash(slice<slice<byte>> slices) {
    var hsha1 = sha1.New();
    foreach (var (_, Δslice) in slices) {
        hsha1.Write(Δslice);
    }
    return hsha1.Sum(default!);
}

// md5SHA1Hash implements TLS 1.0's hybrid hash function which consists of the
// concatenation of an MD5 and SHA1 hash.
internal static slice<byte> md5SHA1Hash(slice<slice<byte>> slices) {
    var md5sha1 = new slice<byte>(md5.ΔSize + sha1.ΔSize);
    var hmd5 = md5.New();
    foreach (var (_, Δslice) in slices) {
        hmd5.Write(Δslice);
    }
    copy(md5sha1, hmd5.Sum(default!));
    copy(md5sha1[(int)(md5.ΔSize)..], sha1Hash(slices));
    return md5sha1;
}

// hashForServerKeyExchange hashes the given slices and returns their digest
// using a hash based on the sigType. It can only be used for TLS 1.0 and 1.1.
internal static slice<byte> hashForServerKeyExchange(uint8 sigType, params Span<slice<byte>> slicesʗp) {
    var slices = slicesʗp.slice();

    if (sigType == signatureECDSA) {
        return sha1Hash(slices);
    }
    return md5SHA1Hash(slices);
}

// ecdheKeyAgreement implements a TLS key agreement where the server
// generates an ephemeral EC public/private key pair and signs it. The
// pre-master secret is then calculated using ECDH. The signature may
// be ECDSA, Ed25519 or RSA.
partial struct ecdheKeyAgreement {
    internal uint16 version;
    internal bool isRSA;
    // ckx and preMasterSecret are generated in processServerKeyExchange
    // and returned in generateClientKeyExchange.
    internal ж<clientKeyExchangeMsg> ckx;
    internal slice<byte> preMasterSecret;
    // curveID, signatureAlgorithm, and key are set by processServerKeyExchange
    // and generateServerKeyExchange.
    internal CurveID curveID;
    internal SignatureScheme signatureAlgorithm;
    internal ж<ecdh.PrivateKey> key;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsNoSupportedEllipticˢ2 = "tls: no supported elliptic curves offered"u8;
internal static readonly @string tlsCertificateCannotBeˢ = "tls: certificate cannot be used with the selected cipher suite"u8;

internal static (ж<serverKeyExchangeMsg>, error) generateServerKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> Ꮡhello) {
    ref var config = ref Ꮡconfig.DerefOrNull();
    ref var cert = ref Ꮡcert.DerefOrNull();
    ref var clientHello = ref ᏑclientHello.DerefOrNull();
    ref var hello = ref Ꮡhello.DerefOrNull();

    foreach (var (_, c) in clientHello.supportedCurves) {
        if (Ꮡconfig.supportsCurve(ka.version, c)) {
            ka.curveID = c;
            break;
        }
    }

    if (ka.curveID == 0) {
        return (default!, errors.New(tlsNoSupportedEllipticˢ2));
    }
    {
        var (_, okΔ1) = curveForCurveID(ka.curveID); if (!okΔ1) {
            return (default!, errors.New(tlsCurvePreferencesˢ));
        }
    }

    var (key, err) = generateECDHEKey(config.rand(), ka.curveID);
    if (err != default!) {
        return (default!, err);
    }
    ka.key = key;

    // See RFC 4492, Section 5.4.
    var ecdhePublic = key.PublicKey().Bytes();
    var serverECDHEParams = new slice<byte>(1 + 2 + 1 + len(ecdhePublic));
    serverECDHEParams[0] = 3; // named curve
    serverECDHEParams[1] = (byte)(uint16)((ka.curveID >> 8));
    serverECDHEParams[2] = (byte)(uint16)ka.curveID;
    serverECDHEParams[3] = (byte)len(ecdhePublic);
    copy(serverECDHEParams[4..], ecdhePublic);

    var (priv, ok) = cert.PrivateKey._<crypto.Signer>(ᐧ);
    if (!ok) {
        return (default!, fmt.Errorf("tls: certificate private key of type %T does not implement crypto.Signer"u8, cert.PrivateKey));
    }

    slice<byte> sig = default!;
    if (ka.version >= VersionTLS12){
        (ka.signatureAlgorithm, err) = selectSignatureScheme(ka.version, Ꮡcert, clientHello.supportedSignatureAlgorithms);
        if (err != default!) {
            return (default!, err);
        }
        ref var sigHash = ref heap<crypto.Hash>(out var ᏑsigHash);
        (var sigType, sigHash, var errΔ1) = typeAndHashFromSignatureScheme(ka.signatureAlgorithm);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        if (sigHash == crypto.SHA1) {
            tlssha1.Value(); // ensure godebug is initialized
            tlssha1.IncNonDefault();
        }
        var signed = slices.Concat<slice<byte>, byte>(clientHello.random, hello.random, serverECDHEParams);
        if ((sigType == signaturePKCS1v15 || sigType == signatureRSAPSS) != ka.isRSA) {
            return (default!, errors.New(tlsCertificateCannotBeˢ));
        }
        var signOpts = ((crypto.SignerOpts)sigHash);
        if (sigType == signatureRSAPSS) {
            signOpts = new rsa_PSSOptionsжSignerOpts(Ꮡ(new rsa.PSSOptions(SaltLength: rsa.PSSSaltLengthEqualsHash, Hash: sigHash)));
        }
        (sig, errΔ1) = crypto.SignMessage(priv, config.rand(), signed, signOpts);
        if (errΔ1 != default!) {
            return (default!, errors.New("tls: failed to sign ECDHE parameters: "u8 + errΔ1.Error()));
        }
    } else {
        var (sigType, sigHash, errΔ2) = legacyTypeAndHashFromPublicKey(priv.Public());
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        var signed = hashForServerKeyExchange(sigType, clientHello.random, hello.random, serverECDHEParams);
        if ((sigType == signaturePKCS1v15) != ka.isRSA) {
            return (default!, errors.New(tlsCertificateCannotBeˢ));
        }
        (sig, errΔ2) = priv.Sign(config.rand(), signed, sigHash);
        if (errΔ2 != default!) {
            return (default!, errors.New("tls: failed to sign ECDHE parameters: "u8 + errΔ2.Error()));
        }
    }

    var skx = @new<serverKeyExchangeMsg>();
    nint sigAndHashLen = 0;
    if (ka.version >= VersionTLS12) {
        sigAndHashLen = 2;
    }
    skx.Value.key = new slice<byte>(len(serverECDHEParams) + sigAndHashLen + 2 + len(sig));
    copy((~skx).key, serverECDHEParams);
    var k = (~skx).key.slice(len(serverECDHEParams));
    if (ka.version >= VersionTLS12) {
        k[0] = (byte)(uint16)((ka.signatureAlgorithm >> 8));
        k[1] = (byte)(uint16)ka.signatureAlgorithm;
        k = k[2..];
    }
    k[0] = (byte)(len(sig) >> 8);
    k[1] = (byte)len(sig);
    copy(k[2..], sig);

    return (skx, default!);
}

internal static (slice<byte>, error) processClientKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<Certificate> Ꮡcert, ж<clientKeyExchangeMsg> Ꮡckx, uint16 version) {
    ref var ckx = ref Ꮡckx.DerefOrNull();

    if (len(ckx.ciphertext) == 0 || (nint)ckx.ciphertext[0] != len(ckx.ciphertext) - 1) {
        return (default!, errClientKeyExchange);
    }

    var (peerKey, err) = ka.key.Curve().NewPublicKey(ckx.ciphertext[1..]);
    if (err != default!) {
        return (default!, errClientKeyExchange);
    }
    (var preMasterSecret, err) = ka.key.ECDH(peerKey);
    if (err != default!) {
        return (default!, errClientKeyExchange);
    }

    return (preMasterSecret, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsServerSelectedˢ4 = "tls: server selected unsupported curve"u8;
internal static readonly @string tlsServerSelectedˢ5 = "tls: server selected unoffered curve"u8;

internal static error processServerKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<serverHelloMsg> ᏑserverHello, ж<Δx509.Certificate> Ꮡcert, ж<serverKeyExchangeMsg> Ꮡskx) {
    ref var config = ref Ꮡconfig.DerefOrNull();
    ref var clientHello = ref ᏑclientHello.DerefOrNull();
    ref var serverHello = ref ᏑserverHello.DerefOrNull();
    ref var cert = ref Ꮡcert.DerefOrNull();
    ref var skx = ref Ꮡskx.DerefOrNull();

    if (len(skx.key) < 4) {
        return errServerKeyExchange;
    }
    if (skx.key[0] != 3) { // named curve
        return errors.New(tlsServerSelectedˢ4);
    }
    ka.curveID = (CurveID)((CurveID)(uint16)(((CurveID)(uint16)skx.key[1]) << 8) | ((CurveID)(uint16)skx.key[2]));

    nint publicLen = (nint)skx.key[3];
    if (publicLen + 4 > len(skx.key)) {
        return errServerKeyExchange;
    }
    var serverECDHEParams = skx.key.slice(0, 4 + publicLen);
    var publicKey = serverECDHEParams[4..];

    var sig = skx.key.slice(4 + publicLen);
    if (len(sig) < 2) {
        return errServerKeyExchange;
    }
    if (ka.version >= VersionTLS12) {
        ka.signatureAlgorithm = (SignatureScheme)((SignatureScheme)(uint16)(((SignatureScheme)(uint16)sig[0]) << 8) | ((SignatureScheme)(uint16)sig[1]));
        sig = sig[2..];
        if (len(sig) < 2) {
            return errServerKeyExchange;
        }
    }
    nint sigLen = (nint)sig[0] << 8 | (nint)sig[1];
    if (sigLen + 2 != len(sig)) {
        return errServerKeyExchange;
    }
    sig = sig[2..];

    if (!slices.Contains(clientHello.supportedCurves, ka.curveID)) {
        return errors.New(tlsServerSelectedˢ5);
    }

    {
        var (_, ok) = curveForCurveID(ka.curveID); if (!ok) {
            return errors.New(tlsServerSelectedˢ4);
        }
    }

    var (key, err) = generateECDHEKey(config.rand(), ka.curveID);
    if (err != default!) {
        return err;
    }
    ka.key = key;

    (var peerKey, err) = key.Curve().NewPublicKey(publicKey);
    if (err != default!) {
        return errServerKeyExchange;
    }
    (ka.preMasterSecret, err) = key.ECDH(peerKey);
    if (err != default!) {
        return errServerKeyExchange;
    }

    var ourPublicKey = key.PublicKey().Bytes();
    ka.ckx = @new<clientKeyExchangeMsg>();
    ka.ckx.Value.ciphertext = new slice<byte>(1 + len(ourPublicKey));
    ka.ckx.Value.ciphertext[0] = (byte)len(ourPublicKey);
    copy((~ka.ckx).ciphertext[1..], ourPublicKey);

    uint8 sigType = default!;
    crypto.Hash sigHash = default!;
    if (ka.version >= VersionTLS12){
        if (!isSupportedSignatureAlgorithm(ka.signatureAlgorithm, clientHello.supportedSignatureAlgorithms)) {
            return errors.New(tlsCertificateUsedWithˢ);
        }
        (sigType, sigHash, err) = typeAndHashFromSignatureScheme(ka.signatureAlgorithm);
        if (err != default!) {
            return err;
        }
        if (sigHash == crypto.SHA1) {
            tlssha1.Value(); // ensure godebug is initialized
            tlssha1.IncNonDefault();
        }
        if ((sigType == signaturePKCS1v15 || sigType == signatureRSAPSS) != ka.isRSA) {
            return errServerKeyExchange;
        }
        var signed = slices.Concat<slice<byte>, byte>(clientHello.random, serverHello.random, serverECDHEParams);
        {
            var errΔ1 = verifyHandshakeSignature(sigType, cert.PublicKey, sigHash, signed, sig); if (errΔ1 != default!) {
                return errors.New("tls: invalid signature by the server certificate: "u8 + errΔ1.Error());
            }
        }
    } else {
        (sigType, sigHash, err) = legacyTypeAndHashFromPublicKey(cert.PublicKey);
        if (err != default!) {
            return err;
        }
        if ((sigType == signaturePKCS1v15) != ka.isRSA) {
            return errServerKeyExchange;
        }
        var signed = hashForServerKeyExchange(sigType, clientHello.random, serverHello.random, serverECDHEParams);
        {
            var errΔ2 = verifyLegacyHandshakeSignature(sigType, cert.PublicKey, sigHash, signed, sig); if (errΔ2 != default!) {
                return errors.New("tls: invalid signature by the server certificate: "u8 + errΔ2.Error());
            }
        }
    }

    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tlsMissingˢ = "tls: missing ServerKeyExchange message"u8;

internal static (slice<byte>, ж<clientKeyExchangeMsg>, error) generateClientKeyExchange(this ref ecdheKeyAgreement ka, ж<Config> Ꮡconfig, ж<clientHelloMsg> ᏑclientHello, ж<Δx509.Certificate> Ꮡcert) {
    if (ka.ckx == nil) {
        return (default!, default!, errors.New(tlsMissingˢ));
    }

    return (ka.preMasterSecret, ka.ckx, default!);
}

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

} // end tls_package
