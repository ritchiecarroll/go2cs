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
internal static ref readonly @string abf7158809cf4f3cˢ => ref abf7158809cf4f3cˢᵇ;
private static readonly @string abf7158809cf4f3cˢᵇ = "2B7E1516 28AED2A6 ABF71588 09CF4F3C"u8;

internal partial struct TestCMAC_tests /*dyn*/ {
    internal @string @in, @out;
}

public static void TestCMAC(ж<testing.T> t) {
    // https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/AES_CMAC.pdf
    @string key = abf7158809cf4f3cˢ;
    var tests = slice<TestCMAC_tests>([
        new(
            "",
            "BB1D6929 E9593728 7FA37D12 9B756746"
        ),
        new(
            "6BC1BEE2 2E409F96 E93D7E11 7393172A",
            "070A16B4 6B4D4144 F79BDD9D D04A287C"
        ),
        new(
            "6BC1BEE2 2E409F96 E93D7E11 7393172A AE2D8A57",
            "7D85449E A6EA19C8 23A7BF78 837DFADE"
        )
    ]);

    var (b, err) = aes.New(decodeHex(t, key));
    if (err != nil) {
        t.Fatal(err);
    }
    var c = gcm.NewCMAC(b);
    foreach (var (i, test) in range(tests)) {
        var @in = decodeHex(t, test.@in);
        var @out = decodeHex(t, test.@out);
        var got = c.MAC(@in);
        if (!bytes.Equal(got[..], @out)) {
            t.Errorf("test %d: got %x, want %x", i, got, @out);
        }
    }
}

} // end fipstest_internal_test_package
