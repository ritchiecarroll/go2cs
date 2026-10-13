// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using static go.math.rand_package;
using testing = testing_package;
using static go.math.rand_internal_test_package;

partial class rand_test_package {

// This test is first, in its own file with an alphabetically early name,
// to try to make sure that it runs early. It has the best chance of
// detecting deterministic seeding if it's the first test that runs.
public static void TestAuto(ж<testing.T> Ꮡt) {
    // Pull out 10 int64s from the global source
    // and then check that they don't appear in that
    // order in the deterministic Seed(1) result.
    slice<int64> @out = default!;
    for (nint i = 0; i < 10; i++) {
        @out = append(@out, Int63());
    }

    // Look for out in Seed(1)'s output.
    // Strictly speaking, we should look for them in order,
    // but this is good enough and not significantly more
    // likely to have a false positive.
    Seed(1);
    nint found = 0;
    for (nint i = 0; i < 1000; i++) {
        var x = Int63();
        if (x == @out[found]) {
            found++;
            if (found == len(@out)) {
                Ꮡt.Fatalf("found unseeded output in Seed(1) output"u8);
            }
        }
    }
}

} // end rand_test_package
