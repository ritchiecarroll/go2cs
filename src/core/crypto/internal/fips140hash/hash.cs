// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using fsha3 = go.crypto.@internal.fips140.sha3_package;
using sha3 = go.crypto.sha3_package;
using hash = hash_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.crypto;

partial class fips140hash_package {

//go:linkname sha3Unwrap
[global::System.Diagnostics.StackTraceHidden] internal static ж<fsha3.Digest> sha3Unwrap(ж<sha3.SHA3> _) {
    return sha3.fips140hash_sha3Unwrap(_);
}

// Unwrap returns h, or a crypto/internal/fips140 inner implementation of h.
//
// The return value can be type asserted to one of
// [crypto/internal/fips140/sha256.Digest],
// [crypto/internal/fips140/sha512.Digest], or
// [crypto/internal/fips140/sha3.Digest] if it is a FIPS 140-3 approved hash.
public static hash.Hash Unwrap(hash.Hash h) {
    {
        var (sha3Δ1, ok) = h._<ж<sha3.SHA3>>(ᐧ); if (ok) {
            return new sha3_DigestжHash(sha3Unwrap(sha3Δ1));
        }
    }
    return h;
}

// UnwrapNew returns a function that calls newHash and applies [Unwrap] to the
// return value.
public static Func<hash.Hash> UnwrapNew<Hash>(Func<Hash> newHash)
    where Hash : hash.Hash
{
    return () => Unwrap(newHash());
}

} // end fips140hash_package
