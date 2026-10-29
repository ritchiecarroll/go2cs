// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
// blank import: go.crypto.@internal.fips140.check_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class gcm_package {

[GoInit] internal static void init() {
    // Counter KDF covers CMAC per IG 10.3.B, and CMAC covers GCM per IG 10.3.A
    // Resolution 1.d(i). AES decryption is covered by the CBC CAST in package
    // crypto/internal/fips140/aes.
    fips140.CAST("CounterKDF"u8, error () => {
        var key = slice<byte>([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10
        ]);
        var context = new byte[]{
            0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2a, 0x2b, 0x2c
        }.array();
        var want = new byte[]{
            0xe6, 0x86, 0x96, 0x97, 0x08, 0xfc, 0x90, 0x30,
            0x36, 0x1c, 0x65, 0x94, 0xb2, 0x62, 0xa5, 0xf7,
            0xcb, 0x9d, 0x93, 0x94, 0xda, 0xf1, 0x94, 0x09,
            0x6a, 0x27, 0x5e, 0x85, 0x22, 0x5e, 0x7a, 0xee
        }.array();
        var (b, err) = aes.New(key);
        if (err != nil) {
            return err;
        }
        var got = NewCounterKDF(b).DeriveKey(0xFF, context);
        if (got != want) {
            return errors.New("unexpected result"u8);
        }
        return default;
    });
}

} // end gcm_package
