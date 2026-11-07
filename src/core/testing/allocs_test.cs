// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;

partial class testing_test_package {

internal static any global;

partial struct allocsPerRunTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Action fn;
    internal float64 allocs;
}
internal static slice<allocsPerRunTestsᴛ1> allocsPerRunTests = slice<allocsPerRunTestsᴛ1>([
    new("alloc *byte", () => {
        global = @new<ж<byte>>();
    }, 1D),
    new("alloc complex128", () => {
        global = @new<complex128>();
    }, 1D),
    new("alloc float64", () => {
        global = @new<float64>();
    }, 1D),
    new("alloc int32", () => {
        global = @new<int32>();
    }, 1D),
    new("alloc byte", () => {
        global = @new<byte>();
    }, 1D)
]);

public static void TestAllocsPerRun(ж<testing.T> t) {
    foreach (var (_, tt) in range(allocsPerRunTests)) {
        {
            var allocs = testing.AllocsPerRun(100, tt.fn); if (allocs != tt.allocs) {
                t.Errorf("AllocsPerRun(100, %s) = %v, want %v", tt.name, allocs, tt.allocs);
            }
        }
    }
}

} // end testing_test_package
