// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using unix = go.@internal.syscall.unix_package;
using testing = testing_package;
using go.@internal.syscall;

partial class unix_test_package {

internal partial struct TestKernelVersionGE_tests /*dyn*/ {
    internal @string name;
    internal nint x, y;
    internal bool want;
}

public static void TestKernelVersionGE(ж<testing.T> t) {
    var (major, minor) = unix.KernelVersion();
    t.Logf("Running on kernel %d.%d", major, minor);

    var tests = slice<TestKernelVersionGE_tests>([
        new(
            name: "current version equals itself",
            x: major,
            y: minor,
            want: true
        ),
        new(
            name: "older major version",
            x: major - 1,
            y: 0,
            want: true
        ),
        new(
            name: "same major, older minor version",
            x: major,
            y: minor - 1,
            want: true
        ),
        new(
            name: "newer major version",
            x: major + 1,
            y: 0,
            want: false
        ),
        new(
            name: "same major, newer minor version",
            x: major,
            y: minor + 1,
            want: false
        ),
        new(
            name: "min version (0.0)",
            x: 0,
            y: 0,
            want: true
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestKernelVersionGE_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = unix.KernelVersionGE(ttʗ1.x, ttʗ1.y);
            if (got != ttʗ1.want) {
                tΔ1.Errorf("KernelVersionGE(%d, %d): got %v, want %v", ttʗ1.x, ttʗ1.y, got, ttʗ1.want);
            }
        });
    }
}

} // end unix_test_package
