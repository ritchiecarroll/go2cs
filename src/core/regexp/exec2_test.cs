// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !race
namespace go;

using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.regexp_package;

partial class regexp_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingˢ = (@string)"skipping TestRE2Exhaustive during short test"u8;
internal static readonly @string testdataRe2ExhaustiveTxtˢ = "testdata/re2-exhaustive.txt.bz2"u8;

// This test is excluded when running under the race detector because
// it is a very expensive test and takes too long.
public static partial void TestRE2Exhaustive(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingˢ);
    }
    testRE2(Ꮡt, testdataRe2ExhaustiveTxtˢ);
}

} // end regexp_internal_test_package
