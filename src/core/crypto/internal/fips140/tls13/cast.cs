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

partial class tls13_package {

[GoInit] internal static void init() {
    fips140.CAST("TLSv1.3-SHA2-256"u8, error () => {
        var input = slice<byte>([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        ]);
        var want = slice<byte>([
            0x78, 0x20, 0x71, 0x75, 0x52, 0xfd, 0x47, 0x67,
            0xe1, 0x07, 0x5c, 0x83, 0x74, 0x2e, 0x49, 0x43,
            0xf7, 0xe3, 0x08, 0x6a, 0x2a, 0xcb, 0x96, 0xc7,
            0xa3, 0x1f, 0xe3, 0x23, 0x56, 0x6e, 0x14, 0x5b
        ]);
        var es = NewEarlySecret<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new sha256_DigestжHash(elemᴛ0)), default);
        var hs = es.HandshakeSecret(default);
        var ms = hs.MasterSecret();
        var transcript = sha256.New();
        transcript.Write(input);
        {
            var got = ms.ResumptionMasterSecret(new sha256_DigestжHash(transcript)); if (!bytes.Equal(got, want)) {
                return errors.New("unexpected result"u8);
            }
        }
        return default;
    });
}

} // end tls13_package
