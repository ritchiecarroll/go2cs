// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using errors = errors_package;
using go.crypto.@internal;

partial class sha256_package {

[GoInit] internal static void init() {
    fips140.CAST("SHA2-256"u8, error () => {
        var input = slice<byte>([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        ]);
        var want = slice<byte>([
            0x5d, 0xfb, 0xab, 0xee, 0xdf, 0x31, 0x8b, 0xf3,
            0x3c, 0x09, 0x27, 0xc4, 0x3d, 0x76, 0x30, 0xf5,
            0x1b, 0x82, 0xf3, 0x51, 0x74, 0x03, 0x01, 0x35,
            0x4f, 0xa3, 0xd7, 0xfc, 0x51, 0xf0, 0x13, 0x2e
        ]);
        var h = New();
        h.Write(input);
        {
            var got = h.Sum(default); if (!bytes.Equal(got, want)) {
                return errors.New("unexpected result"u8);
            }
        }
        return default;
    });
}

} // end sha256_package
