// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using goarch = go.@internal.goarch_package;
using gc = go.@internal.runtime.gc_package;
using scan = go.@internal.runtime.gc.scan_package;
using testing = testing_package;
using go.@internal;
using go.@internal.runtime;
using go.@internal.runtime.gc;

partial class scan_test_package {

// type expandFunc is a methodless func type — rendered inline as its base delegate

internal static void testExpand(ж<testing.T> t, Action<nint, ж<gc.ObjMask>, ж<gc.PtrMask>> expF) {
    var expR = scan.ExpandReference;

    var expRʗ1 = expR;
    testObjs(t, (ж<testing.T> tΔ1, nint sizeClass, ж<gc.ObjMask> objs) => {
        ref var want = ref heap(new gc.PtrMask(), out var Ꮡwant);
        ref var got = ref heap(new gc.PtrMask(), out var Ꮡgot);
        expRʗ1(sizeClass, objs, Ꮡwant);
        expF(sizeClass, objs, Ꮡgot);

        foreach (var i in range(len(want))) {
            if (got[i] != want[i]) {
                tΔ1.Errorf("expansion differs from reference at bit %d", i * (nint)goarch.PtrSize);
                if (goarch.PtrSize == 4) {
                    tΔ1.Logf("got:  %032b", got[i]);
                    tΔ1.Logf("want: %032b", want[i]);
                } else {
                    tΔ1.Logf("got:  %064b", got[i]);
                    tΔ1.Logf("want: %064b", want[i]);
                }
            }
        }
    });
}

} // end scan_test_package
