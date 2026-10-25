// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using testing = testing_package;
using static go.crypto.@internal.fips140.mlkem_package;

partial class mlkem_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nonRejectionPathHitˢ = (@string)"non-rejection path hit"u8;
internal static readonly object rejectionPathHitˢ = (@string)"rejection path hit"u8;
internal static readonly object rejectionPathNotHitˢ = (@string)"rejection path not hit"u8;
internal static readonly object nonRejectionPathNotHitˢ = (@string)"non-rejection path not hit"u8;

public static void TestCASTRejectionPaths(ж<testing.T> Ꮡt) {
    bool rejected = default!;
    bool notRejected = default!;
    testingOnlyRejectionOutcome = (nint compare) => {
        if (compare == 1){
            Ꮡt.Log(nonRejectionPathHitˢ);
            notRejected = true;
        } else {
            Ꮡt.Log(rejectionPathHitˢ);
            rejected = true;
        }
    };
    Ꮡt.Cleanup(() => {
        testingOnlyRejectionOutcome = default!;
    });

    {
        var err = fips140CAST(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }

    if (!rejected) {
        Ꮡt.Error(rejectionPathNotHitˢ);
    }
    if (!notRejected) {
        Ꮡt.Error(nonRejectionPathNotHitˢ);
    }
}

} // end mlkem_internal_test_package
