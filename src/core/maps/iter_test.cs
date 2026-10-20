// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using slices = slices_package;
using testing = testing_package;
using iter = iter_package;
using static go.maps_package;

partial class maps_internal_test_package {

public static void TestAll(ж<testing.T> Ꮡt) {
    for (nint size = 0; size < 10; size++) {
        var m = new map<nint, nint>();
        foreach (var i in range(size)) {
            m[i] = i;
        }
        nint cnt = 0;
        foreach (var (i, v) in range<nint, nint>(All<map<nint, nint>, nint, nint>(m).Invoke)) {
            var (v1, ok) = m[i, ꟷ];
            if (!ok || v != v1) {
                Ꮡt.Errorf("at iteration %d got %d, %d want %d, %d"u8, cnt, i, v, i, v1);
            }
            cnt++;
        }
        if (cnt != size) {
            Ꮡt.Errorf("read %d values expected %d"u8, cnt, size);
        }
    }
}

public static void TestKeys(ж<testing.T> Ꮡt) {
    for (nint size = 0; size < 10; size++) {
        slice<nint> want = default!;
        var m = new map<nint, nint>();
        foreach (var i in range(size)) {
            m[i] = i;
            want = append(want, i);
        }

        slice<nint> got = default!;
        foreach (var k in range<nint>(Keys<map<nint, nint>, nint, nint>(m).Invoke)) {
            got = append(got, k);
        }
        slices.Sort<slice<nint>, nint>(got);
        if (!slices.Equal<slice<nint>, nint>(got, want)) {
            Ꮡt.Errorf("Keys(%v) = %v, want %v"u8, m, got, want);
        }
    }
}

public static void TestValues(ж<testing.T> Ꮡt) {
    for (nint size = 0; size < 10; size++) {
        slice<nint> want = default!;
        var m = new map<nint, nint>();
        foreach (var i in range(size)) {
            m[i] = i;
            want = append(want, i);
        }

        slice<nint> got = default!;
        foreach (var v in range<nint>(Values<map<nint, nint>, nint, nint>(m).Invoke)) {
            got = append(got, v);
        }
        slices.Sort<slice<nint>, nint>(got);
        if (!slices.Equal<slice<nint>, nint>(got, want)) {
            Ꮡt.Errorf("Values(%v) = %v, want %v"u8, m, got, want);
        }
    }
}

public static void TestInsert(ж<testing.T> Ꮡt) {
    var got = new map<nint, nint>{
        [1] = 1,
        [2] = 1
    };
    Insert(got, (Func<nint, nint, bool> yield) => {
        for (nint i = 0; i < 10; i += 2) {
            if (!yield(i, i + 1)) {
                return;
            }
        }
    });

    var want = new map<nint, nint>{
        [1] = 1,
        [2] = 1
    };
    foreach (var (i, v) in new map<nint, nint>{
        [0] = 1,
        [2] = 3,
        [4] = 5,
        [6] = 7,
        [8] = 9
    }) {
        want[i] = v;
    }

    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(got, want)) {
        Ꮡt.Errorf("Insert got: %v, want: %v"u8, got, want);
    }
}

public static void TestCollect(ж<testing.T> Ꮡt) {
    var m = new map<nint, nint>{
        [0] = 1,
        [2] = 3,
        [4] = 5,
        [6] = 7,
        [8] = 9
    };
    var got = Collect(All<map<nint, nint>, nint, nint>(m));
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(got, m)) {
        Ꮡt.Errorf("Collect got: %v, want: %v"u8, got, m);
    }
}

} // end maps_internal_test_package
