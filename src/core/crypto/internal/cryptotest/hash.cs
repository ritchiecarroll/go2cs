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

// type MakeHash is a methodless func type — rendered inline as its base delegate

// TestHash performs a set of tests on hash.Hash implementations, checking the
// documented requirements of Write, Sum, Reset, Size, and BlockSize.
public static void TestHash(ж<testing.T> Ꮡt, Func<hash.Hash> mh) {
    if (Δboring.Enabled || Δfips140.Version() == "v1.0.0"u8) {
        testhash.TestHashWithoutClone(Ꮡt, NilSafeDelegateConversion<Func<hash.Hash>, Func<hash.Hash>>(mh));
        return;
    }
    testhash.TestHash(Ꮡt, NilSafeDelegateConversion<Func<hash.Hash>, Func<hash.Hash>>(mh));
}

internal static io.Reader newRandReader(ж<testing.T> Ꮡt) {
    var seed = time.Now().UnixNano();
    Ꮡt.Logf("Deterministic RNG seed: 0x%x"u8, seed);
    return new rand_RandжReader(rand.New(rand.NewSource(seed)));
}

} // end cryptotest_package
