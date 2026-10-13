// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using drbg = go.crypto.@internal.fips140.drbg_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using hash = hash_package;
using godebug = go.@internal.godebug_package;
using io = io_package;
using go.@internal;
using go.crypto.@internal.fips140;

partial class fips140only_package {

// Enabled reports whether FIPS 140-only mode is enabled, in which non-approved
// cryptography returns an error or panics.
public static bool Enabled = godebug.New("fips140"u8).Value() == "only"u8;

public static bool ApprovedHash(hash.Hash h) {
    switch (h.type()) {
    case ж<sha256.Digest> _:
    case ж<sha512.Digest> _:
    case ж<sha3.Digest> _: {
        return true;
    }
    default: {
        return false;
    }}
}

public static bool ApprovedRandomReader(io.Reader r) {
    var (_, ok) = r._<drbg.DefaultReader>(ᐧ);
    return ok;
}

} // end fips140only_package
