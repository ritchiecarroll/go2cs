// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hpke implements Hybrid Public Key Encryption (HPKE) as defined in
// [RFC 9180].
//
// [RFC 9180]: https://www.rfc-editor.org/rfc/rfc9180.html
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using go.@internal;
using go.crypto;

partial class hpke_package {

partial struct context {
    internal slice<byte> suiteID;
    internal Func<@string, uint16, (slice<byte>, error)> export;
    internal cipher.AEAD aead;
    internal slice<byte> baseNonce;
    // seqNum starts at zero and is incremented for each Seal/Open call.
    // 64 bits are enough not to overflow for 500 years at 1ns per operation.
    internal uint64 seqNum;
}

// Sender is a sending HPKE context. It is instantiated with a specific KEM
// encapsulation key (i.e. the public key), and it is stateful, incrementing the
// nonce counter for each [Sender.Seal] call.
partial struct Sender {
    /*embed*/ internal ж<context> context;
}

// Recipient is a receiving HPKE context. It is instantiated with a specific KEM
// decapsulation key (i.e. the secret key), and it is stateful, incrementing the
// nonce counter for each successful [Recipient.Open] call.
partial struct Recipient {
    /*embed*/ internal ж<context> context;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string secretˢ = "secret"u8;
internal static readonly @string secˢ = "sec"u8;
internal static readonly @string pskIdHashˢ = "psk_id_hash"u8;
internal static readonly @string infoHashˢ = "info_hash"u8;
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string baseNonceˢ = "base_nonce"u8;
internal static readonly @string expˢ = "exp"u8;

internal static (ж<context>, error) newContext(slice<byte> sharedSecret, uint16 kemID, KDF kdf, AEAD Δaead, slice<byte> info) {
    var sid = suiteID(kemID, kdf.ID(), Δaead.ID());

    if (kdf.oneStage()) {
        var secrets = new slice<byte>(0, 2 + 2 + len(sharedSecret));
        secrets = byteorder.BEAppendUint16(secrets, 0); // empty psk
        secrets = byteorder.BEAppendUint16(secrets, (uint16)len(sharedSecret));
        secrets = appendꓸꓸꓸ(secrets, sharedSecret);

        var ksContextΔ1 = new slice<byte>(0, 1 + 2 + 2 + len(info));
        ksContextΔ1 = append(ksContextΔ1, (byte)(0)); // mode 0
        ksContextΔ1 = byteorder.BEAppendUint16(ksContextΔ1, 0); // empty psk_id
        ksContextΔ1 = byteorder.BEAppendUint16(ksContextΔ1, (uint16)len(info));
        ksContextΔ1 = appendꓸꓸꓸ(ksContextΔ1, info);

        var (secretΔ1, errΔ1) = kdf.labeledDerive(sid, secrets, secretˢ, ksContextΔ1,
            (uint16)(Δaead.keySize() + Δaead.nonceSize() + kdf.size()));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var keyΔ1 = secretΔ1.slice(0, Δaead.keySize());
        var baseNonceΔ1 = secretΔ1.slice(Δaead.keySize(), Δaead.keySize() + Δaead.nonceSize());
        var expSecretΔ1 = secretΔ1.slice(Δaead.keySize() + Δaead.nonceSize());

        (var aΔ1, errΔ1) = Δaead.aead(keyΔ1);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        var expSecretʗ1 = expSecretΔ1;
        var sidʗ1 = sid;
        var exportΔ1 = (@string exporterContext, uint16 length) => kdf.labeledDerive(sidʗ1, expSecretʗ1, secˢ, slice<byte>(exporterContext), length);

        return (Ꮡ(new context(
            aead: aΔ1,
            suiteID: sid,
            export: exportΔ1,
            baseNonce: baseNonceΔ1
        )), default!);
    }

    var (pskIDHash, err) = kdf.labeledExtract(sid, default!, pskIdHashˢ, default!);
    if (err != default!) {
        return (default!, err);
    }
    (var infoHash, err) = kdf.labeledExtract(sid, default!, infoHashˢ, info);
    if (err != default!) {
        return (default!, err);
    }
    var ksContext = appendꓸꓸꓸ(new byte[]{0}.slice(), pskIDHash);
    ksContext = appendꓸꓸꓸ(ksContext, infoHash);

    (var secret, err) = kdf.labeledExtract(sid, sharedSecret, secretˢ, default!);
    if (err != default!) {
        return (default!, err);
    }
    (var key, err) = kdf.labeledExpand(sid, secret, keyˢ, ksContext, (uint16)Δaead.keySize());
    if (err != default!) {
        return (default!, err);
    }
    (var a, err) = Δaead.aead(key);
    if (err != default!) {
        return (default!, err);
    }
    (var baseNonce, err) = kdf.labeledExpand(sid, secret, baseNonceˢ, ksContext, (uint16)Δaead.nonceSize());
    if (err != default!) {
        return (default!, err);
    }
    (var expSecret, err) = kdf.labeledExpand(sid, secret, expˢ, ksContext, (uint16)kdf.size());
    if (err != default!) {
        return (default!, err);
    }
    var expSecretʗ2 = expSecret;
    var sidʗ2 = sid;
    var export = (@string exporterContext, uint16 length) => kdf.labeledExpand(sidʗ2, expSecretʗ2, secˢ, slice<byte>(exporterContext), length);

    return (Ꮡ(new context(
        aead: a,
        suiteID: sid,
        export: export,
        baseNonce: baseNonce
    )), default!);
}

// NewSender returns a sending HPKE context for the provided KEM encapsulation
// key (i.e. the public key), and using the ciphersuite defined by the
// combination of KEM, KDF, and AEAD.
//
// The info parameter is additional public information that must match between
// sender and recipient.
//
// The returned enc ciphertext can be used to instantiate a matching receiving
// HPKE context with the corresponding KEM decapsulation key.
public static (slice<byte> enc, ж<Sender> s, error err) NewSender(ΔPublicKey pk, KDF kdf, AEAD Δaead, slice<byte> info) {
    error err = default!;

    (var sharedSecret, var encapsulatedKey, err) = pk.encap();
    if (err != default!) {
        return (default!, default!, err);
    }
    (var context, err) = newContext(sharedSecret, pk.KEM().ID(), kdf, Δaead, info);
    if (err != default!) {
        return (default!, default!, err);
    }
    return (encapsulatedKey, Ꮡ(new Sender(context)), default!);
}

// NewRecipient returns a receiving HPKE context for the provided KEM
// decapsulation key (i.e. the secret key), and using the ciphersuite defined by
// the combination of KEM, KDF, and AEAD.
//
// The enc parameter must have been produced by a matching sending HPKE context
// with the corresponding KEM encapsulation key. The info parameter is
// additional public information that must match between sender and recipient.
public static (ж<Recipient>, error) NewRecipient(slice<byte> enc, PrivateKey k, KDF kdf, AEAD Δaead, slice<byte> info) {
    var (sharedSecret, err) = k.decap(enc);
    if (err != default!) {
        return (default!, err);
    }
    (var context, err) = newContext(sharedSecret, k.KEM().ID(), kdf, Δaead, info);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new Recipient(context)), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exportOnlyInstantiationˢ = "export-only instantiation"u8;

// Seal encrypts the provided plaintext, optionally binding to the additional
// public data aad.
//
// Seal uses incrementing counters for each call, and Open on the receiving side
// must be called in the same order as Seal.
public static (slice<byte>, error) Seal(this ref Sender s, slice<byte> aad, slice<byte> plaintext) {
    if (s.aead == default!) {
        return (default!, errors.New(exportOnlyInstantiationˢ));
    }
    var ciphertext = s.aead.Seal(default!, s.nextNonce(), plaintext, aad);
    s.seqNum++;
    return (ciphertext, default!);
}

// Seal instantiates a single-use HPKE sending HPKE context like [NewSender],
// and then encrypts the provided plaintext like [Sender.Seal] (with no aad).
// Seal returns the concatenation of the encapsulated key and the ciphertext.
public static (slice<byte>, error) Seal(ΔPublicKey pk, KDF kdf, AEAD Δaead, slice<byte> info, slice<byte> plaintext) {
    var (enc, s, err) = NewSender(pk, kdf, Δaead, info);
    if (err != default!) {
        return (default!, err);
    }
    (var ct, err) = s.Seal(default!, plaintext);
    if (err != default!) {
        return (default!, err);
    }
    return (appendꓸꓸꓸ(enc, ct), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidLengthˢ = "invalid length"u8;

// Export produces a secret value derived from the shared key between sender and
// recipient. length must be at most 65,535.
public static (slice<byte>, error) Export(this ref Sender s, @string exporterContext, nint length) {
    if (length < 0 || length > 0xFFFF) {
        return (default!, errors.New(invalidLengthˢ));
    }
    return s.export(exporterContext, (uint16)length);
}

// Open decrypts the provided ciphertext, optionally binding to the additional
// public data aad, or returns an error if decryption fails.
//
// Open uses incrementing counters for each successful call, and must be called
// in the same order as Seal on the sending side.
public static (slice<byte>, error) Open(this ref Recipient r, slice<byte> aad, slice<byte> ciphertext) {
    if (r.aead == default!) {
        return (default!, errors.New(exportOnlyInstantiationˢ));
    }
    var (plaintext, err) = r.aead.Open(default!, r.nextNonce(), ciphertext, aad);
    if (err != default!) {
        return (default!, err);
    }
    r.seqNum++;
    return (plaintext, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ciphertextTooShortˢ = "ciphertext too short"u8;

// Open instantiates a single-use HPKE receiving HPKE context like [NewRecipient],
// and then decrypts the provided ciphertext like [Recipient.Open] (with no aad).
// ciphertext must be the concatenation of the encapsulated key and the actual ciphertext.
public static (slice<byte>, error) Open(PrivateKey k, KDF kdf, AEAD Δaead, slice<byte> info, slice<byte> ciphertext) {
    nint encSize = k.KEM().encSize();
    if (len(ciphertext) < encSize) {
        return (default!, errors.New(ciphertextTooShortˢ));
    }
    var enc = ciphertext.slice(0, encSize);
    ciphertext = ciphertext.slice(encSize);
    var (r, err) = NewRecipient(enc, k, kdf, Δaead, info);
    if (err != default!) {
        return (default!, err);
    }
    return r.Open(default!, ciphertext);
}

// Export produces a secret value derived from the shared key between sender and
// recipient. length must be at most 65,535.
public static (slice<byte>, error) Export(this ref Recipient r, @string exporterContext, nint length) {
    if (length < 0 || length > 0xFFFF) {
        return (default!, errors.New(invalidLengthˢ));
    }
    return r.export(exporterContext, (uint16)length);
}

internal static slice<byte> nextNonce(this ref context ctx) {
    var nonce = new slice<byte>(ctx.aead.NonceSize());
    byteorder.BEPutUint64(nonce.slice(len(nonce) - 8), ctx.seqNum);
    foreach (var (i, _) in ctx.baseNonce) {
        nonce[i] ^= (byte)(ctx.baseNonce[i]);
    }
    return nonce;
}

internal static slice<byte> suiteID(uint16 kemID, uint16 kdfID, uint16 aeadID) {
    var suiteID = new slice<byte>(0, 4 + 2 + 2 + 2);
    suiteID = appendꓸꓸꓸ(suiteID, slice<byte>("HPKE"u8));
    suiteID = byteorder.BEAppendUint16(suiteID, kemID);
    suiteID = byteorder.BEAppendUint16(suiteID, kdfID);
    suiteID = byteorder.BEAppendUint16(suiteID, aeadID);
    return suiteID;
}

} // end hpke_package
