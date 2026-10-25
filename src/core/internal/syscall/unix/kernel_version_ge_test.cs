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

public static void TestKernelVersionGE(ж<testing.T> Ꮡt) {
    var (major, minor) = unix.KernelVersion();
    Ꮡt.Logf("Running on kernel %d.%d"u8, major, minor);

    var tests = new TestKernelVersionGE_tests[]{
        new(
            name: "current version equals itself"u8,
            x: major,
            y: minor,
            want: true
        ),
        new(
            name: "older major version"u8,
            x: major - 1,
            y: 0,
            want: true
        ),
        new(
            name: "same major, older minor version"u8,
            x: major,
            y: minor - 1,
            want: true
        ),
        new(
            name: "newer major version"u8,
            x: major + 1,
            y: 0,
            want: false
        ),
        new(
            name: "same major, newer minor version"u8,
            x: major,
            y: minor + 1,
            want: false
        ),
        new(
            name: "min version (0.0)"u8,
            x: 0,
            y: 0,
            want: true
        )
    }.slice();

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestKernelVersionGE_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            var got = unix.KernelVersionGE(ttʗ1.x, ttʗ1.y);
            if (got != ttʗ1.want) {
                tΔ1.Errorf("KernelVersionGE(%d, %d): got %v, want %v"u8, ttʗ1.x, ttʗ1.y, got, ttʗ1.want);
            }
        });
    }
}

} // end unix_test_package
