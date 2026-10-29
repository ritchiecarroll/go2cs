// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using fips140 = go.crypto.@internal.fips140_package;
using errors = errors_package;
using go.crypto.@internal;

partial class sha3_package {

[GoInit] internal static void init() {
    fips140.CAST("cSHAKE128"u8, error () => {
        var input = slice<byte>([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        ]);
        var want = slice<byte>([
            0xd2, 0x17, 0x37, 0x39, 0xf6, 0xa1, 0xe4, 0x6e,
            0x81, 0xe5, 0x70, 0xe3, 0x1b, 0x10, 0x4c, 0x82,
            0xc5, 0x48, 0xee, 0xe6, 0x09, 0xf5, 0x89, 0x52,
            0x52, 0xa4, 0x69, 0xd4, 0xd0, 0x76, 0x68, 0x6b
        ]);
        var h = NewCShake128(input, input);
        h.Write(input);
        {
            var got = h.Sum(default); if (!bytes.Equal(got, want)) {
                return errors.New("unexpected result"u8);
            }
        }
        return default;
    });
}

} // end sha3_package
