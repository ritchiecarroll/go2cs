// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using hkdf = go.crypto.hkdf_package;
using sha256 = go.crypto.sha256_package;
using sha3 = go.crypto.sha3_package;
using sha512 = go.crypto.sha512_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using byteorder = go.@internal.byteorder_package;
using go.@internal;
using go.crypto;

partial class hpke_package {

// The KDF is one of the three components of an HPKE ciphersuite, implementing
// key derivation.
partial interface KDF {
    uint16 ID();
    bool oneStage();
    nint size(); // Nh
    (slice<byte>, error) labeledDerive(slice<byte> suiteID, slice<byte> inputKey, @string label, slice<byte> context, uint16 length);
    (slice<byte>, error) labeledExtract(slice<byte> suiteID, slice<byte> salt, @string label, slice<byte> inputKey);
    (slice<byte>, error) labeledExpand(slice<byte> suiteID, slice<byte> randomKey, @string label, slice<byte> info, uint16 length);
}

// NewKDF returns the KDF implementation for the given KDF ID.
//
// Applications are encouraged to use specific implementations like [HKDFSHA256]
// instead, unless runtime agility is required.
public static (KDF, error) NewKDF(uint16 id) {
    switch (id) {
    case 0x0001: { // HKDF-SHA256
        return (HKDFSHA256(), default!);
    }
    case 0x0002: { // HKDF-SHA384
        return (HKDFSHA384(), default!);
    }
    case 0x0003: { // HKDF-SHA512
        return (HKDFSHA512(), default!);
    }
    case 0x0010: { // SHAKE128
        return (SHAKE128(), default!);
    }
    case 0x0011: { // SHAKE256
        return (SHAKE256(), default!);
    }
    default: {
        return (default!, fmt.Errorf("unsupported KDF %04x"u8, id));
    }}
}

// HKDFSHA256 returns an HKDF-SHA256 KDF implementation.
public static KDF HKDFSHA256() {
    return new hkdfKDFжKDF(hkdfSHA256);
}

// HKDFSHA384 returns an HKDF-SHA384 KDF implementation.
public static KDF HKDFSHA384() {
    return new hkdfKDFжKDF(hkdfSHA384);
}

// HKDFSHA512 returns an HKDF-SHA512 KDF implementation.
public static KDF HKDFSHA512() {
    return new hkdfKDFжKDF(hkdfSHA512);
}

partial struct hkdfKDF {
    internal Func<hash.Hash> hash;
    internal uint16 id;
    internal nint nH;
}

internal static ж<hkdfKDF> hkdfSHA256 = Ꮡ(new hkdfKDF(hash: sha256.New, id: 0x0001, nH: sha256.Size));

internal static ж<hkdfKDF> hkdfSHA384 = Ꮡ(new hkdfKDF(hash: sha512.New384, id: 0x0002, nH: sha512.Size384));

internal static ж<hkdfKDF> hkdfSHA512 = Ꮡ(new hkdfKDF(hash: sha512.New, id: 0x0003, nH: sha512.Size));

internal static uint16 ID(this ref hkdfKDF kdf) {
    return kdf.id;
}

internal static nint size(this ref hkdfKDF kdf) {
    return kdf.nH;
}

internal static bool oneStage(this ref hkdfKDF kdf) {
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hpkeInternalErrorˢ = "hpke: internal error: labeledDerive called on two-stage KDF"u8;

internal static (slice<byte>, error) labeledDerive(this ref hkdfKDF kdf, slice<byte> _Δp1, slice<byte> _Δp2, @string _Δp3, slice<byte> _Δp4, uint16 _Δp5) {
    return (default!, errors.New(hpkeInternalErrorˢ));
}

internal static (slice<byte>, error) labeledExtract(this ref hkdfKDF kdf, slice<byte> suiteID, slice<byte> salt, @string label, slice<byte> inputKey) {
    var labeledIKM = new slice<byte>(0, 7 + len(suiteID) + len(label) + len(inputKey));
    labeledIKM = appendꓸꓸꓸ(labeledIKM, slice<byte>("HPKE-v1"u8));
    labeledIKM = appendꓸꓸꓸ(labeledIKM, suiteID);
    labeledIKM = append(labeledIKM, label.ꓸꓸꓸ);
    labeledIKM = appendꓸꓸꓸ(labeledIKM, inputKey);
    return hkdf.Extract(kdf.hash, labeledIKM, salt);
}

internal static (slice<byte>, error) labeledExpand(this ref hkdfKDF kdf, slice<byte> suiteID, slice<byte> randomKey, @string label, slice<byte> info, uint16 length) {
    var labeledInfo = new slice<byte>(0, 2 + 7 + len(suiteID) + len(label) + len(info));
    labeledInfo = byteorder.BEAppendUint16(labeledInfo, length);
    labeledInfo = appendꓸꓸꓸ(labeledInfo, slice<byte>("HPKE-v1"u8));
    labeledInfo = appendꓸꓸꓸ(labeledInfo, suiteID);
    labeledInfo = append(labeledInfo, label.ꓸꓸꓸ);
    labeledInfo = appendꓸꓸꓸ(labeledInfo, info);
    return hkdf.Expand(kdf.hash, randomKey, ((@string)labeledInfo), (nint)length);
}

// SHAKE128 returns a SHAKE128 KDF implementation.
public static KDF SHAKE128() {
    return new shakeKDFжKDF(shake128KDF);
}

// SHAKE256 returns a SHAKE256 KDF implementation.
public static KDF SHAKE256() {
    return new shakeKDFжKDF(shake256KDF);
}

partial struct shakeKDF {
    internal Func<ж<sha3.SHAKE>> hash;
    internal uint16 id;
    internal nint nH;
}

internal static ж<shakeKDF> shake128KDF = Ꮡ(new shakeKDF(hash: sha3.NewSHAKE128, id: 0x0010, nH: 32));

internal static ж<shakeKDF> shake256KDF = Ꮡ(new shakeKDF(hash: sha3.NewSHAKE256, id: 0x0011, nH: 64));

internal static uint16 ID(this ref shakeKDF kdf) {
    return kdf.id;
}

internal static nint size(this ref shakeKDF kdf) {
    return kdf.nH;
}

internal static bool oneStage(this ref shakeKDF kdf) {
    return true;
}

internal static (slice<byte>, error) labeledDerive(this ref shakeKDF kdf, slice<byte> suiteID, slice<byte> inputKey, @string label, slice<byte> context, uint16 length) {
    var H = kdf.hash();
    H.Write(inputKey);
    H.Write(slice<byte>("HPKE-v1"u8));
    H.Write(suiteID);
    H.Write(new byte[]{(byte)((len(label) >> (int)(8))), (byte)len(label)}.slice());
    H.Write(slice<byte>(label));
    H.Write(new byte[]{(byte)((length >> (int)(8))), (byte)length}.slice());
    H.Write(context);
    var @out = new slice<byte>(length);
    H.Read(@out);
    return (@out, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hpkeInternalErrorˢ2 = "hpke: internal error: labeledExtract called on one-stage KDF"u8;

internal static (slice<byte>, error) labeledExtract(this ref shakeKDF kdf, slice<byte> _Δp1, slice<byte> _Δp2, @string _Δp3, slice<byte> _Δp4) {
    return (default!, errors.New(hpkeInternalErrorˢ2));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hpkeInternalErrorˢ3 = "hpke: internal error: labeledExpand called on one-stage KDF"u8;

internal static (slice<byte>, error) labeledExpand(this ref shakeKDF kdf, slice<byte> _Δp1, slice<byte> _Δp2, @string _Δp3, slice<byte> _Δp4, uint16 _Δp5) {
    return (default!, errors.New(hpkeInternalErrorˢ3));
}

} // end hpke_package
