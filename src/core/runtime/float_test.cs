// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestIssue48807(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, i) in new uint64[]{
        0x8234508000000001UL, // from issue48807
        (uint64)(72057598332895232L + 1)
    }.slice()) {
        var got = (float32)i;
        var dontwant = (float32)(float64)i;
        if (got == dontwant) {
            // The test cases above should be uint64s such that
            // this equality doesn't hold. These examples trigger
            // the case where using an intermediate float64 doesn't work.
            Ꮡt.Errorf("direct float32 conversion doesn't work: arg=%x got=%x dontwant=%x"u8, i, got, dontwant);
        }
    }
}

} // end runtime_test_package
