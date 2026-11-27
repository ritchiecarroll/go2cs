// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δboring = go.crypto.@internal.boring_package;
using Δfips140 = go.crypto.@internal.fips140_package;
using hash = hash_package;
using testhash = go.@internal.testhash_package;
using io = io_package;
using rand = math.rand_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using go.crypto.@internal;
using math;

partial class cryptotest_package {

public delegate hash.Hash MakeHash();

// TestHash performs a set of tests on hash.Hash implementations, checking the
// documented requirements of Write, Sum, Reset, Size, and BlockSize.
public static void TestHash(ж<testing.T> t, MakeHash mh) {
    if (Δboring.Enabled || Δfips140.Version() == "v1.0.0"u8) {
        testhash.TestHashWithoutClone(t, builtin.NilSafeDelegateConversion<testhash.MakeHash, MakeHash>(mh));
        return;
    }
    testhash.TestHash(t, builtin.NilSafeDelegateConversion<testhash.MakeHash, MakeHash>(mh));
}

internal static partial io.Reader newRandReader(ж<testing.T> t) {
    var seed = time.Now().UnixNano();
    t.Logf("Deterministic RNG seed: 0x%x", seed);
    return new rand_RandжReader(rand.New(rand.NewSource(seed)));
}

} // end cryptotest_package
