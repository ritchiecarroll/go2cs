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

partial class pbkdf2_package {

[GoInit] internal static void init() {
    // Per IG 10.3.A:
    //   "if the module implements an approved PBKDF (SP 800-132), the module
    //    shall perform a CAST, at minimum, on the derivation of the Master
    //   Key (MK) as specified in Section 5.3 of SP 800-132"
    //   "The Iteration Count parameter does not need to be among those
    //   supported by the module in the approved mode but shall be at least
    //   two."
    fips140.CAST("PBKDF2"u8, error () => {
        var salt = new byte[]{
            0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11,
            0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19
        }.slice();
        var want = new byte[]{
            0xC7, 0x58, 0x76, 0xC0, 0x71, 0x1C, 0x29, 0x75,
            0x2D, 0x3A, 0xA6, 0xDF, 0x29, 0x96
        }.slice();

        var (mk, err) = Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new sha256_DigestжHash(elemᴛ0)), "password"u8, salt, 2, 14);
        if (err != default!) {
            return err;
        }
        if (!bytes.Equal(mk, want)) {
            return errors.New("unexpected result"u8);
        }

        return default!;
    });
}

} // end pbkdf2_package
