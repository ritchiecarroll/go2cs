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

partial class tls12_package {

[GoInit] internal static void init() {
    fips140.CAST("TLSv1.2-SHA2-256"u8, error () => {
        var input = slice<byte>([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        ]);
        var transcript = slice<byte>([
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20
        ]);
        var want = slice<byte>([
            0x8c, 0x3e, 0xed, 0xa7, 0x1c, 0x1b, 0x4c, 0xc0,
            0xa0, 0x44, 0x90, 0x75, 0xa8, 0x8e, 0xbc, 0x7c,
            0x5e, 0x1c, 0x4b, 0x1e, 0x4f, 0xe3, 0xc1, 0x06,
            0xeb, 0xdc, 0xc0, 0x5d, 0xc0, 0xc8, 0xec, 0xf3,
            0xe2, 0xb9, 0xd1, 0x03, 0x5e, 0xb2, 0x60, 0x5d,
            0x12, 0x68, 0x4f, 0x49, 0xdf, 0xa9, 0x9d, 0xcc
        ]);
        {
            var got = MasterSecret<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new sha256_DigestжHash(elemᴛ0)), input, transcript); if (!bytes.Equal(got, want)) {
                return errors.New("unexpected result"u8);
            }
        }
        return default;
    });
}

} // end tls12_package
