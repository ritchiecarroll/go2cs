// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package hmac implements the Keyed-Hash Message Authentication Code (HMAC) as
defined in U.S. Federal Information Processing Standards Publication 198.
An HMAC is a cryptographic hash that uses a key to sign a message.
The receiver verifies the hash by recomputing it using the same key.

Receivers should be careful to use Equal to compare MACs in order to avoid
timing side-channels:

	// ValidMAC reports whether messageMAC is a valid HMAC tag for message.
	func ValidMAC(message, messageMAC, key []byte) bool {
		mac := hmac.New(sha256.New, key)
		mac.Write(message)
		expectedMAC := mac.Sum(nil)
		return hmac.Equal(messageMAC, expectedMAC)
	}
*/
namespace go.crypto;

using boring = go.crypto.@internal.boring_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using fips140hash = go.crypto.@internal.fips140hash_package;
using fips140only = go.crypto.@internal.fips140only_package;
using subtle = go.crypto.subtle_package;
using hash = hash_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class hmac_package {

// New returns a new HMAC hash using the given [hash.Hash] type and key.
// New functions like [crypto/sha256.New] can be used as h.
// h must return a new Hash every time it is called.
// Note that unlike other hash implementations in the standard library,
// the returned Hash does not implement [encoding.BinaryMarshaler]
// or [encoding.BinaryUnmarshaler].
public static hash.Hash New(Func<hash.Hash> h, slice<byte> key) {
    if (boring.Enabled) {
        var hm = boring.NewHMAC(h, key);
        if (hm != default!) {
            return hm;
        }
        // BoringCrypto did not recognize h, so fall through to standard Go code.
    }
    h = fips140hash.UnwrapNew(h);
    if (fips140only.Enforced()) {
        if (len(key) < 112 / 8) {
            throw panic("crypto/hmac: use of keys shorter than 112 bits is not allowed in FIPS 140-only mode");
        }
        if (!fips140only.ApprovedHash(h())) {
            throw panic("crypto/hmac: use of hash functions other than SHA-2 or SHA-3 is not allowed in FIPS 140-only mode");
        }
    }
    return new hmac_HMACжHash(hmac.New(h, key));
}

// Equal compares two MACs for equality without leaking timing information.
public static bool Equal(slice<byte> mac1, slice<byte> mac2) {
    // We don't have to be constant time if the lengths of the MACs are
    // different as that suggests that a completely different hash function
    // was used.
    return subtle.ConstantTimeCompare(mac1, mac2) == 1;
}

} // end hmac_package
