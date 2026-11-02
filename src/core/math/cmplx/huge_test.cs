// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Disabled for s390x because it uses assembly routines that are not
// accurate for huge arguments.
//go:build !s390x
namespace go.math;

using testing = testing_package;
using static go.math.cmplx_package;

partial class cmplx_internal_test_package {

public static void TestTanHuge(ж<testing.T> t) {
    foreach (var (i, x) in hugeIn) {
        {
            var f = Tan(x); if (!cSoclose(tanHuge[i], f, 3e-15D)) {
                t.Errorf("Tan(%g) = %g, want %g", x, f, tanHuge[i]);
            }
        }
    }
}

} // end cmplx_internal_test_package
