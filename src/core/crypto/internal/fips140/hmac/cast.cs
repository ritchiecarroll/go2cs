// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using hash = hash_package;

partial class hmac_package {

[GoInit] internal static void init() {
    fips140.CAST("HMAC-SHA2-256"u8, error () => {
        var input = slice<byte>([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        ]);
        var want = slice<byte>([
            0xf0, 0x8d, 0x82, 0x8d, 0x4c, 0x9e, 0xad, 0x3d,
            0xdc, 0x12, 0x9c, 0x4e, 0x70, 0xc4, 0x19, 0x2a,
            0x4f, 0x12, 0x73, 0x23, 0x73, 0x77, 0x66, 0x05,
            0x10, 0xee, 0x57, 0x6b, 0x3a, 0xc7, 0x14, 0x41
        ]);
        var h = New<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new sha256_DigestжHash(elemᴛ0)), input);
        h.Write(input);
        h.Write(input);
        {
            var got = h.Sum(default); if (!bytes.Equal(got, want)) {
                return errors.New("unexpected result"u8);
            }
        }
        return default;
    });
}

} // end hmac_package
