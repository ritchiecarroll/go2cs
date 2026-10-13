// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δboring = go.crypto.@internal.boring_package;
using asan = go.@internal.asan_package;
using msan = go.@internal.msan_package;
using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using runtime = runtime_package;
using testing = testing_package;
using go.@internal;
using go.crypto.@internal;

partial class cryptotest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingAllocationsTestˢ = (@string)"skipping allocations test with BoringCrypto"u8;
private static readonly object skippingAllocationsTestˢ2 = (@string)"skipping allocations test with sanitizers"u8;
private static readonly object skippingAllocationsTestˢ3 = (@string)"skipping allocations test on plan9"u8;
private static readonly object skippingAllocationsTestˢ4 = (@string)"skipping allocations test on s390x"u8;

// SkipTestAllocations skips the test if there are any factors that interfere
// with allocation optimizations.
public static void SkipTestAllocations(ж<testing.T> Ꮡt) {
    // Go+BoringCrypto uses cgo.
    if (Δboring.Enabled) {
        Ꮡt.Skip(skippingAllocationsTestˢ);
    }

    // The sanitizers sometimes cause allocations.
    if (race.Enabled || msan.Enabled || asan.Enabled) {
        Ꮡt.Skip(skippingAllocationsTestˢ2);
    }

    // The plan9 crypto/rand allocates.
    if (runtime.GOOS == "plan9"u8) {
        Ꮡt.Skip(skippingAllocationsTestˢ3);
    }

    // s390x deviates from other assembly implementations and is very hard to
    // test due to the lack of LUCI builders. See #67307.
    if (runtime.GOARCH == "s390x"u8) {
        Ꮡt.Skip(skippingAllocationsTestˢ4);
    }

    // Some APIs rely on inliner and devirtualization to allocate on the stack.
    testenv.SkipIfOptimizationOff(new testing_TжTB(Ꮡt));
}

} // end cryptotest_package
