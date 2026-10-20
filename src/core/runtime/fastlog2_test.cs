// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestFastLog2(ж<testing.T> Ꮡt) {
    // Compute the euclidean distance between math.Log2 and the FastLog2
    // implementation over the range of interest for heap sampling.
    UntypedInt randomBitCount = 26;
    float64 e = default!;

    nint inc = 1;
    if (testing.Short()) {
        // Check 1K total values, down from 64M.
        inc = 1 << 16;
    }
    for (nint i = 1; i < (nint)(1 << (int)(randomBitCount)); i += inc) {
        var (l, fl) = (Δmath.Log2((float64)i), runtime_internal_test_package.Fastlog2((float64)i));
        var d = l - fl;
        e += d * d;
    }
    e = Δmath.Sqrt(e);

    if (e > 1.0D) {
        Ꮡt.Fatalf("imprecision on fastlog2 implementation, want <=1.0, got %f"u8, e);
    }
}

} // end runtime_test_package
