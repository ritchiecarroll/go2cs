// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
// blank import: go.crypto.@internal.fips140.check_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using sha256 = go.crypto.@internal.fips140.sha256_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using hash = hash_package;

partial class hkdf_package {

[GoInit] internal static void init() {
    fips140.CAST("HKDF-SHA2-256"u8, error () => {
        var input = new byte[]{
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        }.slice();
        var want = new byte[]{
            0xb6, 0x53, 0x00, 0x5b, 0x51, 0x6d, 0x2b, 0xc9,
            0x4a, 0xe4, 0xf9, 0x51, 0x73, 0x1f, 0x71, 0x21,
            0xa6, 0xc1, 0xde, 0x42, 0x4f, 0x2c, 0x99, 0x60,
            0x64, 0xdb, 0x66, 0x3e, 0xec, 0xa6, 0x37, 0xff
        }.slice();
        var got = Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new sha256_DigestжHash(elemᴛ0)), input, input, ((@string)input), len(want));
        if (!bytes.Equal(got, want)) {
            return errors.New("unexpected result"u8);
        }
        return default!;
    });
}

} // end hkdf_package
