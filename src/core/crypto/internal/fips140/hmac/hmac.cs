// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hmac implements HMAC according to [FIPS 198-1].
//
// [FIPS 198-1]: https://doi.org/10.6028/NIST.FIPS.198-1
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class hmac_package {

// key is zero padded to the block size of the hash function
// ipad = 0x36 byte repeated for key length
// opad = 0x5c byte repeated for key length
// hmac = H([key ^ opad] H([key ^ ipad] text))

// marshalable is the combination of encoding.BinaryMarshaler and
// encoding.BinaryUnmarshaler. Their method definitions are repeated here to
// avoid a dependency on the encoding package.
[GoType] partial interface marshalable {
    (slice<byte>, error) MarshalBinary();
    error UnmarshalBinary(slice<byte> _);
}

[GoType] partial struct HMAC {
    internal slice<byte> opad, ipad;
    internal fips140.Hash outer, inner;
    // If marshaled is true, then opad and ipad do not contain a padded
    // copy of the key, but rather the marshaled state of outer/inner after
    // opad/ipad has been fed into it.
    internal bool marshaled;
    // forHKDF and keyLen are stored to inform the service indicator decision.
    internal bool forHKDF;
    internal nint keyLen;
}

[GoRecv] public static slice<byte> Sum(this ref HMAC h, slice<byte> @in) {
    // Per FIPS 140-3 IG C.M, key lengths below 112 bits are only allowed for
    // legacy use (i.e. verification only) and we don't support that. However,
    // HKDF uses the HMAC key for the salt, which is allowed to be shorter.
    if (h.keyLen < 112 / 8 && !h.forHKDF) {
        fips140.RecordNonApproved();
    }
    switch (h.inner.type()) {
    case ж<sha256.Digest> _:
    case ж<sha512.Digest> _:
    case ж<sha3.Digest> _: {
        break;
    }
    default: {
        fips140.RecordNonApproved();
        break;
    }}

    nint origLen = len(@in);
    @in = h.inner.Sum(@in);
    if (h.marshaled){
        {
            var err = h.outer._<marshalable>().UnmarshalBinary(h.opad); if (err != default!) {
                throw panic(err);
            }
        }
    } else {
        h.outer.Reset();
        h.outer.Write(h.opad);
    }
    h.outer.Write(@in.slice(origLen));
    return h.outer.Sum(@in.slice(0, origLen));
}

[GoRecv] public static (nint n, error err) Write(this ref HMAC h, slice<byte> p) {
    return h.inner.Write(p);
}

[GoRecv] public static nint Size(this ref HMAC h) {
    return h.outer.Size();
}

[GoRecv] public static nint BlockSize(this ref HMAC h) {
    return h.inner.BlockSize();
}

[GoRecv] public static void Reset(this ref HMAC h) {
    if (h.marshaled) {
        {
            var errΔ1 = h.inner._<marshalable>().UnmarshalBinary(h.ipad); if (errΔ1 != default!) {
                throw panic(errΔ1);
            }
        }
        return;
    }
    h.inner.Reset();
    h.inner.Write(h.ipad);
    // If the underlying hash is marshalable, we can save some time by saving a
    // copy of the hash state now, and restoring it on future calls to Reset and
    // Sum instead of writing ipad/opad every time.
    //
    // We do this on Reset to avoid slowing down the common single-use case.
    //
    // This is allowed by FIPS 198-1, Section 6: "Conceptually, the intermediate
    // results of the compression function on the B-byte blocks (K0 ⊕ ipad) and
    // (K0 ⊕ opad) can be precomputed once, at the time of generation of the key
    // K, or before its first use. These intermediate results can be stored and
    // then used to initialize H each time that a message needs to be
    // authenticated using the same key. [...] These stored intermediate values
    // shall be treated and protected in the same manner as secret keys."
    var (marshalableInner, innerOK) = h.inner._<marshalable>(ᐧ);
    if (!innerOK) {
        return;
    }
    var (marshalableOuter, outerOK) = h.outer._<marshalable>(ᐧ);
    if (!outerOK) {
        return;
    }
    var (imarshal, err) = marshalableInner.MarshalBinary();
    if (err != default!) {
        return;
    }
    h.outer.Reset();
    h.outer.Write(h.opad);
    (var omarshal, err) = marshalableOuter.MarshalBinary();
    if (err != default!) {
        return;
    }
    // Marshaling succeeded; save the marshaled state for later
    h.ipad = imarshal;
    h.opad = omarshal;
    h.marshaled = true;
}

// New returns a new HMAC hash using the given [fips140.Hash] type and key.
public static ж<HMAC> New<H>(Func<H> h, slice<byte> key)
    where H : fips140.Hash
{
    var hm = Ꮡ(new HMAC(keyLen: len(key)));
    hm.Value.outer = h();
    hm.Value.inner = h();
    var unique = true;
    var hmʗ1 = hm;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                // The comparison might panic if the underlying types are not comparable.
                _ = recover();
            }, ref ᒐ);
            if (AreEqual((~hmʗ1).outer, (~hmʗ1).inner)) {
                unique = false;
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    if (!unique) {
        throw panic("crypto/hmac: hash generation function does not produce unique values");
    }
    nint blocksize = (~hm).inner.BlockSize();
    hm.Value.ipad = new slice<byte>(blocksize);
    hm.Value.opad = new slice<byte>(blocksize);
    if (len(key) > blocksize) {
        // If key is too big, hash it.
        (~hm).outer.Write(key);
        key = (~hm).outer.Sum(default!);
    }
    copy((~hm).ipad, key);
    copy((~hm).opad, key);
    foreach (var (i, _) in (~hm).ipad) {
        hm.Value.ipad[i] ^= (byte)(0x36);
    }
    foreach (var (i, _) in (~hm).opad) {
        hm.Value.opad[i] ^= (byte)(0x5c);
    }
    (~hm).inner.Write((~hm).ipad);
    return hm;
}

// MarkAsUsedInKDF records that this HMAC instance is used as part of a KDF.
public static void MarkAsUsedInKDF(ж<HMAC> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    h.forHKDF = true;
}

} // end hmac_package
