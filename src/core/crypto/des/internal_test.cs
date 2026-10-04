// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using testing = testing_package;
using static go.crypto.des_package;

partial class des_internal_test_package {

public static void TestInitialPermute(ж<testing.T> Ꮡt) {
    for (nuint i = (nuint)0; i < 64; i++) {
        var bit = ((uint64)1).Lsh(i);
        var got = permuteInitialBlock(bit);
        var want = ((uint64)1).Lsh((uint64)(finalPermutation[63 - i]));
        if (got != want) {
            Ꮡt.Errorf("permute(%x) = %x, want %x"u8, bit, got, want);
        }
    }
}

public static void TestFinalPermute(ж<testing.T> Ꮡt) {
    for (nuint i = (nuint)0; i < 64; i++) {
        var bit = ((uint64)1).Lsh(i);
        var got = permuteFinalBlock(bit);
        var want = ((uint64)1).Lsh((uint64)(initialPermutation[63 - i]));
        if (got != want) {
            Ꮡt.Errorf("permute(%x) = %x, want %x"u8, bit, got, want);
        }
    }
}

} // end des_internal_test_package
