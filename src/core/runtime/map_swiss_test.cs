// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build goexperiment.swissmap
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using maps = @internal.runtime.maps_package;
using slices = slices_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestHmapSize(ж<testing.T> Ꮡt) {
    // The structure of Map is defined in internal/runtime/maps/map.go
    // and in cmd/compile/internal/reflectdata/map_swiss.go and must be in sync.
    // The size of Map should be 48 bytes on 64 bit and 32 bytes on 32 bit platforms.
    var wantSize = (uintptr)(2 * 8 + 4 * goarch.PtrSize);
    var gotSize = /* unsafe.Sizeof(maps.Map{}) */ (uintptr)48;
    if (gotSize != wantSize) {
        Ꮡt.Errorf("sizeof(maps.Map{})==%d, want %d"u8, gotSize, wantSize);
    }
}

// See also reflect_test.TestGroupSizeZero.
public static void TestGroupSizeZero(ж<testing.T> Ꮡt) {
    map<EmptyStruct, EmptyStruct> m = default!;
    var mTyp = abi.TypeOf(m);
    var mt = mTyp.Reinterpret<abi.Type, abi.SwissMapType>();

    // internal/runtime/maps when create pointers to slots, even if slots
    // are size 0. The compiler should have reserved an extra word to
    // ensure that pointers to the zero-size type at the end of group are
    // valid.
    if ((~mt).Group.Size() <= 8) {
        Ꮡt.Errorf("Group size got %d want >8"u8, (~mt).Group.Size());
    }
}

public static void TestMapIterOrder(ж<testing.T> Ꮡt) {
    var sizes = new nint[]{3, 7, 9, 15}.slice();
    foreach (var (_, n) in sizes) {
        for (nint i = 0; i < 1000; i++) {
            // Make m be {0: true, 1: true, ..., n-1: true}.
            var m = new map<nint, bool>();
            for (nint iΔ1 = 0; iΔ1 < n; iΔ1++) {
                m[iΔ1] = true;
            }
            // Check that iterating over the map produces at least two different orderings.
            var mʗ1 = m;
            slice<nint> ord() {
                slice<nint> s = default!;
                foreach (var (key, _) in mʗ1) {
                    s = append(s, key);
                }
                return s;
            }
            var first = ord();
            var ok = false;
            for (nint @try = 0; @try < 100; @try++) {
                if (!slices.Equal<slice<nint>, nint>(first, ord())) {
                    ok = true;
                    break;
                }
            }
            if (!ok) {
                Ꮡt.Errorf("Map with n=%d elements had consistent iteration order: %v"u8, n, first);
                break;
            }
        }
    }
}

} // end runtime_test_package
