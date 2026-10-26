// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δreflect = reflect_package;
using Δtesting = testing_package;
using static global::go.reflect_internal_test_package;

partial class reflect_test_package {

// See also runtime_test.TestGroupSizeZero.
public static void TestGroupSizeZero(ж<Δtesting.T> Ꮡt) {
    var st = Δreflect.TypeFor<EmptyStruct>();
    var grp = reflect_internal_test_package.MapGroupOf(st, st);

    // internal/runtime/maps when create pointers to slots, even if slots
    // are size 0. We should have reserved an extra word to ensure that
    // pointers to the zero-size type at the end of group are valid.
    if (grp.Size() <= 8) {
        Ꮡt.Errorf("Group size got %d want >8"u8, grp.Size());
    }
}

} // end reflect_test_package
