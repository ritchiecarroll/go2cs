// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class tls12_package {

// PRF implements the TLS 1.2 pseudo-random function, as defined in RFC 5246,
// Section 5 and allowed by SP 800-135, Revision 1, Section 4.2.2.
public static slice<byte> PRF<H>(Func<H> hash, slice<byte> secret, @string label, slice<byte> seed, nint keyLen)
    where H : fips140.Hash
{
    var labelAndSeed = new slice<byte>(len(label) + len(seed));
    copy(labelAndSeed, label);
    copy(labelAndSeed.slice(len(label)), seed);
    var result = new slice<byte>(keyLen);
    pHash(hash, result, secret, labelAndSeed);
    return result;
}

// pHash implements the P_hash function, as defined in RFC 5246, Section 5.
internal static void pHash<H>(Func<H> hash, slice<byte> result, slice<byte> secret, slice<byte> seed)
    where H : fips140.Hash
{
    var h = hmac.New(hash, secret);
    h.Write(seed);
    var a = h.Sum(default!);
    while (len(result) > 0) {
        h.Reset();
        h.Write(a);
        h.Write(seed);
        var b = h.Sum(default!);
        nint n = copy(result, b);
        result = result.slice(n);
        h.Reset();
        h.Write(a);
        a = h.Sum(default!);
    }
}

internal static UntypedInt masterSecretLength => 48;

internal static readonly @string extendedMasterSecretLabel = "extended master secret"u8;

// MasterSecret implements the TLS 1.2 extended master secret derivation, as
// defined in RFC 7627 and allowed by SP 800-135, Revision 1, Section 4.2.2.
public static slice<byte> MasterSecret<H>(Func<H> hash, slice<byte> preMasterSecret, slice<byte> transcript)
    where H : fips140.Hash
{
    // "The TLS 1.2 KDF is an approved KDF when the following conditions are
    // satisfied: [...] (3) P_HASH uses either SHA-256, SHA-384 or SHA-512."
    var h = hash();
    switch (((any)h).type()) {
    case ж<sha256.Digest>: {
        if (h.Size() != 32) {
            fips140.RecordNonApproved();
        }
        break;
    }
    case ж<sha512.Digest>: {
        if (h.Size() != 46 && h.Size() != 64) {
            fips140.RecordNonApproved();
        }
        break;
    }
    default: {
        fips140.RecordNonApproved();
        break;
    }}

    return PRF(hash, preMasterSecret, extendedMasterSecretLabel, transcript, masterSecretLength);
}

} // end tls12_package
