// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using static global::go.math.rand.rand_package;
using testing = testing_package;
using global::go.math.rand;
using rand = global::go.math.rand.rand_package;
using static global::go.math.rand.rand_internal_test_package;

partial class rand_test_package {

// This test is first, in its own file with an alphabetically early name,
// to try to make sure that it runs early. It has the best chance of
// detecting deterministic seeding if it's the first test that runs.
public static void TestAuto(ж<testing.T> Ꮡt) {
    // Pull out 10 int64s from the global source
    // and then check that they don't appear in that
    // order in the deterministic seeded result.
    slice<int64> @out = default!;
    for (nint i = 0; i < 10; i++) {
        @out = append(@out, Int64());
    }

    // Look for out in seeded output.
    // Strictly speaking, we should look for them in order,
    // but this is good enough and not significantly more
    // likely to have a false positive.
    var r = New(new rand.PCGжSource(NewPCG(1, 0)));
    nint found = 0;
    for (nint i = 0; i < 1000; i++) {
        var x = r.Int64();
        if (x == @out[found]) {
            found++;
            if (found == len(@out)) {
                Ꮡt.Fatalf("found unseeded output in Seed(1) output"u8);
            }
        }
    }
}

} // end rand_test_package
