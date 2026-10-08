// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class ecdsa_package {

// hmacDRBG is an SP 800-90A Rev. 1 HMAC_DRBG.
//
// It is only intended to be used to generate ECDSA nonces. Since it will be
// instantiated ex-novo for each signature, its Generate function will only be
// invoked once or twice (only for P-256, with probability 2⁻³²).
//
// Per Table 2, it has a reseed interval of 2^48 requests, and a maximum request
// size of 2^19 bits (2^16 bytes, 64 KiB).
public partial struct hmacDRBG {
    internal Func<slice<byte>, ж<hmac.HMAC>> newHMAC;
    internal ж<hmac.HMAC> hK;
    public slice<byte> V;
    internal uint64 reseedCounter;
}

internal static UntypedInt reseedInterval => /* 1 << 48 */ 281474976710656;
internal static UntypedInt maxRequestSize => /* (1 << 19) / 8 */ 65536;

partial struct plainPersonalizationString /*[]byte*/;

internal static void isPersonalizationString(this plainPersonalizationString _) {
}

partial struct blockAlignedPersonalizationString /*[]slice<byte>*/;

internal static void isPersonalizationString(this blockAlignedPersonalizationString _) {
}

partial interface personalizationString {
    void isPersonalizationString();
}

internal static ж<hmacDRBG> newDRBG<H>(Func<H> hash, slice<byte> entropy, slice<byte> nonce, personalizationString s)
    where H : fips140.Hash
{
    // HMAC_DRBG_Instantiate_algorithm, per Section 10.1.2.3.
    fips140.RecordApproved();
    var d = Ꮡ(new hmacDRBG(
        newHMAC: (slice<byte> key) => hmac.New(hash, key)
    ));
    nint size = hash().Size();
    // K = 0x00 0x00 0x00 ... 0x00
    var K = new slice<byte>(size);
    // V = 0x01 0x01 0x01 ... 0x01
    d.Value.V = bytes.Repeat(new byte[]{0x01}.slice(), size);
    // HMAC_DRBG_Update, per Section 10.1.2.2.
    // K = HMAC (K, V || 0x00 || provided_data)
    var h = hmac.New(hash, K);
    h.Write((~d).V);
    h.Write(new byte[]{0x00}.slice());
    h.Write(entropy);
    h.Write(nonce);
    switch (s.type()) {
    case plainPersonalizationString sΔ1: {
        h.Write(sΔ1);
        break;
    }
    case blockAlignedPersonalizationString sΔ1: {
        nint l = len((~d).V) + 1 + len(entropy) + len(nonce);
        foreach (var (_, b) in sΔ1) {
            pad000(h, l);
            h.Write(b);
            l = len(b);
        }
        break;
    }}
    K = h.Sum(K[..0]);
    // V = HMAC (K, V)
    h = hmac.New(hash, K);
    h.Write((~d).V);
    d.Value.V = h.Sum((~d).V[..0]);
    // K = HMAC (K, V || 0x01 || provided_data).
    h.Reset();
    h.Write((~d).V);
    h.Write(new byte[]{0x01}.slice());
    h.Write(entropy);
    h.Write(nonce);
    switch (s.type()) {
    case plainPersonalizationString sΔ2: {
        h.Write(sΔ2);
        break;
    }
    case blockAlignedPersonalizationString sΔ2: {
        nint l = len((~d).V) + 1 + len(entropy) + len(nonce);
        foreach (var (_, b) in sΔ2) {
            pad000(h, l);
            h.Write(b);
            l = len(b);
        }
        break;
    }}
    K = h.Sum(K[..0]);
    // V = HMAC (K, V)
    h = hmac.New(hash, K);
    h.Write((~d).V);
    d.Value.V = h.Sum((~d).V[..0]);
    d.Value.hK = h;
    d.Value.reseedCounter = 1;
    return d;
}

// TestingOnlyNewDRBG creates an SP 800-90A Rev. 1 HMAC_DRBG with a plain
// personalization string.
//
// This should only be used for ACVP testing. hmacDRBG is not intended to be
// used directly.
public static ж<hmacDRBG> TestingOnlyNewDRBG<H>(Func<H> hash, slice<byte> entropy, slice<byte> nonce, slice<byte> s)
    where H : fips140.Hash
{
    return newDRBG(hash, entropy, nonce, ((plainPersonalizationString)s));
}

internal static void pad000(ж<hmac.HMAC> Ꮡh, nint writtenSoFar) {
    ref var h = ref Ꮡh.DerefOrNull();

    nint blockSize = h.BlockSize();
    {
        nint rem = builtin.rem(writtenSoFar, blockSize); if (rem != 0) {
            h.Write(new slice<byte>(blockSize - rem));
        }
    }
}

// Generate produces at most maxRequestSize bytes of random data in out.
public static void Generate(this ref hmacDRBG d, slice<byte> @out) {
    // HMAC_DRBG_Generate_algorithm, per Section 10.1.2.5.
    fips140.RecordApproved();
    if (len(@out) > maxRequestSize) {
        throw panic("ecdsa: internal error: request size exceeds maximum");
    }
    if (d.reseedCounter > reseedInterval) {
        throw panic("ecdsa: reseed interval exceeded");
    }
    nint tlen = 0;
    while (tlen < len(@out)) {
        // V = HMAC_K(V)
        // T = T || V
        d.hK.Reset();
        d.hK.Write(d.V);
        d.V = d.hK.Sum(d.V[..0]);
        tlen += copy(@out.slice(tlen), d.V);
    }
    // Note that if this function shows up on ECDSA-level profiles, this can be
    // optimized in the common case by deferring the rest to the next Generate
    // call, which will never come in nearly all cases.
    // HMAC_DRBG_Update, per Section 10.1.2.2, without provided_data.
    // K = HMAC (K, V || 0x00)
    d.hK.Reset();
    d.hK.Write(d.V);
    d.hK.Write(new byte[]{0x00}.slice());
    var K = d.hK.Sum(default!);
    // V = HMAC (K, V)
    d.hK = d.newHMAC(K);
    d.hK.Write(d.V);
    d.V = d.hK.Sum(d.V[..0]);
    d.reseedCounter++;
}

} // end ecdsa_package
