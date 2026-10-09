// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using bytes = bytes_package;
using aes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using testing = testing_package;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.aes;

partial class fipstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abf7158809cf4f3cˢ = "2B7E1516 28AED2A6 ABF71588 09CF4F3C"u8;

internal partial struct TestCMAC_tests /*dyn*/ {
    internal @string @in, @out;
}

public static void TestCMAC(ж<testing.T> Ꮡt) {
    // https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/AES_CMAC.pdf
    @string key = abf7158809cf4f3cˢ;
    var tests = new TestCMAC_tests[]{
        new(
            ""u8,
            "BB1D6929 E9593728 7FA37D12 9B756746"u8
        ),
        new(
            "6BC1BEE2 2E409F96 E93D7E11 7393172A"u8,
            "070A16B4 6B4D4144 F79BDD9D D04A287C"u8
        ),
        new(
            "6BC1BEE2 2E409F96 E93D7E11 7393172A AE2D8A57"u8,
            "7D85449E A6EA19C8 23A7BF78 837DFADE"u8
        )
    }.slice();
    var (b, err) = aes.New(decodeHex(Ꮡt, key));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var c = gcm.NewCMAC(b);
    foreach (var (i, test) in tests) {
        var @in = decodeHex(Ꮡt, test.@in);
        var @out = decodeHex(Ꮡt, test.@out);
        var got = c.MAC(@in);
        if (!bytes.Equal(got[..], @out)) {
            Ꮡt.Errorf("test %d: got %x, want %x"u8, i, got, @out);
        }
    }
}

} // end fipstest_internal_test_package
