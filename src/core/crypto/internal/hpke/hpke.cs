// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using crypto = crypto_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using ecdh = go.crypto.ecdh_package;
using hkdf = go.crypto.@internal.fips140.hkdf_package;
using rand = go.crypto.rand_package;
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using bits = math.bits_package;
using chacha20poly1305 = vendor.golang.org.x.crypto.chacha20poly1305_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal.fips140;
using hash = hash_package;
using io = io_package;
using math;
using vendor.golang.org.x.crypto;
using Δfips140 = go.crypto.@internal.fips140_package;

partial class hpke_package {

// testingOnlyGenerateKey is only used during testing, to provide
// a fixed test key to use when checking the RFC 9180 vectors.
internal static Func<(ж<ecdh.PrivateKey>, error)> testingOnlyGenerateKey;

[GoType] public partial struct hkdfKDF {
    internal crypto.Hash hash;
}

public static slice<byte> LabeledExtract(this ж<hkdfKDF> Ꮡkdf, slice<byte> sid, slice<byte> salt, @string label, slice<byte> inputKey) {
    ref var kdf = ref Ꮡkdf.DerefOrNull();

    var labeledIKM = new slice<byte>(0, 7 + len(sid) + len(label) + len(inputKey));
    labeledIKM = appendꓸꓸꓸ(labeledIKM, slice<byte>("HPKE-v1"u8));
    labeledIKM = appendꓸꓸꓸ(labeledIKM, sid);
    labeledIKM = append(labeledIKM, label.ꓸꓸꓸ);
    labeledIKM = appendꓸꓸꓸ(labeledIKM, inputKey);
    var recvʗ1 = kdf.hash;
    return hkdf.Extract<Δfips140.Hash>(widen<hash.Hash, Δfips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), labeledIKM, salt);
}

public static slice<byte> LabeledExpand(this ж<hkdfKDF> Ꮡkdf, slice<byte> suiteID, slice<byte> randomKey, @string label, slice<byte> info, uint16 length) {
    ref var kdf = ref Ꮡkdf.DerefOrNull();

    var labeledInfo = new slice<byte>(0, 2 + 7 + len(suiteID) + len(label) + len(info));
    labeledInfo = byteorder.BEAppendUint16(labeledInfo, length);
    labeledInfo = appendꓸꓸꓸ(labeledInfo, slice<byte>("HPKE-v1"u8));
    labeledInfo = appendꓸꓸꓸ(labeledInfo, suiteID);
    labeledInfo = append(labeledInfo, label.ꓸꓸꓸ);
    labeledInfo = appendꓸꓸꓸ(labeledInfo, info);
    var recvʗ1 = kdf.hash;
    return hkdf.Expand<Δfips140.Hash>(widen<hash.Hash, Δfips140.Hash>(() => recvʗ1.New(), elemᴛ0 => new hash_HashᴠHash(elemᴛ0)), randomKey, ((@string)labeledInfo), (nint)length);
}

// dhKEM implements the KEM specified in RFC 9180, Section 4.1.
[GoType] partial struct dhKEM {
    internal ecdhꓸCurve dh;
    internal hkdfKDF kdf;
    internal slice<byte> suiteID;
    internal uint16 nSecret;
}

[GoType("num:uint16")] partial struct KemID;

public static UntypedInt DHKEM_X25519_HKDF_SHA256 => 0x0020;

// RFC 9180 Section 7.1

[GoType("dyn")] partial struct SupportedKEMsᴛ1 {
    internal ecdhꓸCurve curve;
    internal crypto.Hash hash;
    internal uint16 nSecret;
}
public static map<uint16, SupportedKEMsᴛ1> SupportedKEMs = new map<uint16, SupportedKEMsᴛ1>{
    [DHKEM_X25519_HKDF_SHA256] = new(ecdh.X25519(), crypto.SHA256, 32)
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedSuiteIdˢ = "unsupported suite ID"u8;

internal static (ж<dhKEM>, error) newDHKem(uint16 kemID) {
    var (suite, ok) = SupportedKEMs[kemID, ꟷ];
    if (!ok) {
        return (default!, errors.New(unsupportedSuiteIdˢ));
    }
    return (Ꮡ(new dhKEM(
        dh: suite.curve,
        kdf: new hkdfKDF(suite.hash),
        suiteID: byteorder.BEAppendUint16(slice<byte>("KEM"u8), kemID),
        nSecret: suite.nSecret
    )), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string eaePrkˢ = "eae_prk"u8;
internal static readonly @string sharedSecretˢ = "shared_secret"u8;

internal static slice<byte> ExtractAndExpand(this ж<dhKEM> Ꮡdh, slice<byte> dhKey, slice<byte> kemContext) {
    ref var dh = ref Ꮡdh.DerefOrNull();

    var eaePRK = Ꮡdh.of(dhKEM.Ꮡkdf).LabeledExtract(dh.suiteID[..], default!, eaePrkˢ, dhKey);
    return Ꮡdh.of(dhKEM.Ꮡkdf).LabeledExpand(dh.suiteID[..], eaePRK, sharedSecretˢ, kemContext, dh.nSecret);
}

internal static (slice<byte> sharedSecret, slice<byte> encapPub, error err) Encap(this ж<dhKEM> Ꮡdh, ж<ecdhꓸPublicKey> ᏑpubRecipient) {
    error err = default!;

    ref var dh = ref Ꮡdh.DerefOrNull();
    ref var pubRecipient = ref ᏑpubRecipient.DerefOrNull();
    ж<ecdh.PrivateKey> privEph = default!;
    if (testingOnlyGenerateKey != default!){
        (privEph, err) = testingOnlyGenerateKey();
    } else {
        (privEph, err) = dh.dh.GenerateKey(rand.Reader);
    }
    if (err != default!) {
        return (default!, default!, err);
    }
    (var dhVal, err) = privEph.ECDH(ᏑpubRecipient);
    if (err != default!) {
        return (default!, default!, err);
    }
    var encPubEph = privEph.PublicKey().Bytes();
    var encPubRecip = pubRecipient.Bytes();
    var kemContext = appendꓸꓸꓸ(encPubEph, encPubRecip);
    return (Ꮡdh.ExtractAndExpand(dhVal, kemContext), encPubEph, default!);
}

internal static (slice<byte>, error) Decap(this ж<dhKEM> Ꮡdh, slice<byte> encPubEph, ж<ecdh.PrivateKey> ᏑsecRecipient) {
    ref var dh = ref Ꮡdh.DerefOrNull();
    ref var secRecipient = ref ᏑsecRecipient.DerefOrNull();

    var (pubEph, err) = dh.dh.NewPublicKey(encPubEph);
    if (err != default!) {
        return (default!, err);
    }
    (var dhVal, err) = ᏑsecRecipient.ECDH(pubEph);
    if (err != default!) {
        return (default!, err);
    }
    var kemContext = appendꓸꓸꓸ(encPubEph, secRecipient.PublicKey().Bytes());
    return (Ꮡdh.ExtractAndExpand(dhVal, kemContext), default!);
}

[GoType] partial struct context {
    internal cipher.AEAD aead;
    internal slice<byte> sharedSecret;
    internal slice<byte> suiteID;
    internal slice<byte> key;
    internal slice<byte> baseNonce;
    internal slice<byte> exporterSecret;
    internal uint128 seqNum;
}

[GoType] partial struct Sender {
    internal partial ref ж<context> context { get; }
}

[GoType] partial struct Receipient {
    internal partial ref ж<context> context { get; }
}

internal static Func<slice<byte>, (cipher.AEAD, error)> aesGCMNew = (slice<byte> key) => {
    var (block, err) = aes.NewCipher(key);
    if (err != default!) {
        return (default!, err);
    }
    return cipher.NewGCM(block);
};

[GoType("num:uint16")] partial struct AEADID;

public static UntypedInt AEAD_AES_128_GCM => 0x0001;
public static UntypedInt AEAD_AES_256_GCM => 0x0002;
public static UntypedInt AEAD_ChaCha20Poly1305 => 0x0003;

// RFC 9180, Section 7.3

[GoType("dyn")] partial struct SupportedAEADsᴛ1 {
    internal nint keySize;
    internal nint nonceSize;
    internal Func<slice<byte>, (cipher.AEAD, error)> aead;
}
public static map<uint16, SupportedAEADsᴛ1> SupportedAEADs = new map<uint16, SupportedAEADsᴛ1>{
    [AEAD_AES_128_GCM] = new(keySize: 16, nonceSize: 12, aead: aesGCMNew),
    [AEAD_AES_256_GCM] = new(keySize: 32, nonceSize: 12, aead: aesGCMNew),
    [AEAD_ChaCha20Poly1305] = new(keySize: chacha20poly1305.KeySize, nonceSize: chacha20poly1305.ΔNonceSize, aead: chacha20poly1305.New)
};

[GoType("num:uint16")] partial struct KDFID;

public static UntypedInt KDF_HKDF_SHA256 => 0x0001;

// RFC 9180, Section 7.2
public static map<uint16, Func<ж<hkdfKDF>>> SupportedKDFs = new map<uint16, Func<ж<hkdfKDF>>>{
    [KDF_HKDF_SHA256] = () => Ꮡ(new hkdfKDF(crypto.SHA256))
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedKdfIdˢ = "unsupported KDF id"u8;
internal static readonly @string unsupportedAeadIdˢ = "unsupported AEAD id"u8;
internal static readonly @string pskIdHashˢ = "psk_id_hash"u8;
internal static readonly @string infoHashˢ = "info_hash"u8;
internal static readonly @string secretˢ = "secret"u8;
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string baseNonceˢ = "base_nonce"u8;
internal static readonly @string expˢ = "exp"u8;

internal static (ж<context>, error) newContext(slice<byte> sharedSecret, uint16 kemID, uint16 kdfID, uint16 aeadID, slice<byte> info) {
    var sid = suiteID(kemID, kdfID, aeadID);
    var (kdfInit, ok) = SupportedKDFs[kdfID, ꟷ];
    if (!ok) {
        return (default!, errors.New(unsupportedKdfIdˢ));
    }
    var kdf = kdfInit();
    (var aeadInfo, ok) = SupportedAEADs[aeadID, ꟷ];
    if (!ok) {
        return (default!, errors.New(unsupportedAeadIdˢ));
    }
    var pskIDHash = kdf.LabeledExtract(sid, default!, pskIdHashˢ, default!);
    var infoHash = kdf.LabeledExtract(sid, default!, infoHashˢ, info);
    var ksContext = appendꓸꓸꓸ(new byte[]{0}.slice(), pskIDHash);
    ksContext = appendꓸꓸꓸ(ksContext, infoHash);
    var secret = kdf.LabeledExtract(sid, sharedSecret, secretˢ, default!);
    var key = kdf.LabeledExpand(sid, secret, keyˢ, ksContext, (uint16)aeadInfo.keySize);
    /* Nk - key size for AEAD */
    var baseNonce = kdf.LabeledExpand(sid, secret, baseNonceˢ, ksContext, (uint16)aeadInfo.nonceSize);
    /* Nn - nonce size for AEAD */
    var exporterSecret = kdf.LabeledExpand(sid, secret, expˢ, ksContext, (uint16)(~kdf).hash.Size());
    /* Nh - hash output size of the kdf*/
    var (aead, err) = aeadInfo.aead(key);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new context(
        aead: aead,
        sharedSecret: sharedSecret,
        suiteID: sid,
        key: key,
        baseNonce: baseNonce,
        exporterSecret: exporterSecret
    )), default!);
}

public static (slice<byte>, ж<Sender>, error) SetupSender(uint16 kemID, uint16 kdfID, uint16 aeadID, ж<ecdhꓸPublicKey> Ꮡpub, slice<byte> info) {
    var (kem, err) = newDHKem(kemID);
    if (err != default!) {
        return (default!, default!, err);
    }
    (var sharedSecret, var encapsulatedKey, err) = kem.Encap(Ꮡpub);
    if (err != default!) {
        return (default!, default!, err);
    }
    (var context, err) = newContext(sharedSecret, kemID, kdfID, aeadID, info);
    if (err != default!) {
        return (default!, default!, err);
    }
    return (encapsulatedKey, Ꮡ(new Sender(context)), default!);
}

public static (ж<Receipient>, error) SetupReceipient(uint16 kemID, uint16 kdfID, uint16 aeadID, ж<ecdh.PrivateKey> Ꮡpriv, slice<byte> info, slice<byte> encPubEph) {
    var (kem, err) = newDHKem(kemID);
    if (err != default!) {
        return (default!, err);
    }
    (var sharedSecret, err) = kem.Decap(encPubEph, Ꮡpriv);
    if (err != default!) {
        return (default!, err);
    }
    (var context, err) = newContext(sharedSecret, kemID, kdfID, aeadID, info);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new Receipient(context)), default!);
}

[GoRecv] internal static slice<byte> nextNonce(this ref context ctx) {
    var nonce = ctx.seqNum.bytes().slice(16 - ctx.aead.NonceSize());
    foreach (var (i, _) in ctx.baseNonce) {
        nonce[i] ^= (byte)(ctx.baseNonce[i]);
    }
    return nonce;
}

[GoRecv] internal static void incrementNonce(this ref context ctx) {
    // Message limit is, according to the RFC, 2^95+1, which
    // is somewhat confusing, but we do as we're told.
    if (ctx.seqNum.bitLen() >= (ctx.aead.NonceSize() * 8) - 1) {
        throw panic("message limit reached");
    }
    ctx.seqNum = ctx.seqNum.addOne();
}

[GoRecv] public static (slice<byte>, error) Seal(this ref Sender s, slice<byte> aad, slice<byte> plaintext) {
    var ciphertext = s.aead.Seal(default!, s.nextNonce(), plaintext, aad);
    s.incrementNonce();
    return (ciphertext, default!);
}

[GoRecv] public static (slice<byte>, error) Open(this ref Receipient r, slice<byte> aad, slice<byte> ciphertext) {
    var (plaintext, err) = r.aead.Open(default!, r.nextNonce(), ciphertext, aad);
    if (err != default!) {
        return (default!, err);
    }
    r.incrementNonce();
    return (plaintext, default!);
}

internal static slice<byte> suiteID(uint16 kemID, uint16 kdfID, uint16 aeadID) {
    var suiteID = new slice<byte>(0, 4 + 2 + 2 + 2);
    suiteID = appendꓸꓸꓸ(suiteID, slice<byte>("HPKE"u8));
    suiteID = byteorder.BEAppendUint16(suiteID, kemID);
    suiteID = byteorder.BEAppendUint16(suiteID, kdfID);
    suiteID = byteorder.BEAppendUint16(suiteID, aeadID);
    return suiteID;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedKemIdˢ = "unsupported KEM id"u8;

public static (ж<ecdhꓸPublicKey>, error) ParseHPKEPublicKey(uint16 kemID, slice<byte> bytes) {
    var (kemInfo, ok) = SupportedKEMs[kemID, ꟷ];
    if (!ok) {
        return (default!, errors.New(unsupportedKemIdˢ));
    }
    return kemInfo.curve.NewPublicKey(bytes);
}

public static (ж<ecdh.PrivateKey>, error) ParseHPKEPrivateKey(uint16 kemID, slice<byte> bytes) {
    var (kemInfo, ok) = SupportedKEMs[kemID, ꟷ];
    if (!ok) {
        return (default!, errors.New(unsupportedKemIdˢ));
    }
    return kemInfo.curve.NewPrivateKey(bytes);
}

[GoType] partial struct uint128 {
    internal uint64 hi, lo;
}

internal static uint128 addOne(this uint128 u) {
    var (lo, carry) = bits.Add64(u.lo, 1, 0);
    return new uint128(u.hi + carry, lo);
}

internal static nint bitLen(this uint128 u) {
    return bits.Len64(u.hi) + bits.Len64(u.lo);
}

internal static slice<byte> bytes(this uint128 u) {
    var b = new slice<byte>(16);
    byteorder.BEPutUint64(b[0..], u.hi);
    byteorder.BEPutUint64(b[8..], u.lo);
    return b;
}

} // end hpke_package
