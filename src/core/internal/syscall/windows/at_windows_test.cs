// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using Δwindows = go.@internal.syscall.windows_package;
using os = os_package;
using filepath = path.filepath_package;
using syscall = syscall_package;
using testing = testing_package;
using go.@internal.syscall;
using path;

partial class windows_test_package {

internal partial struct TestOpen_tests /*dyn*/ {
    internal @string path;
    internal nint flag;
    internal error err;
}

public static void TestOpen(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string dir = Ꮡt.TempDir();
    @string @file = filepath.Join(dir, "a");
    var (f, err) = os.Create(@file);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    f.Close();
    var tests = new TestOpen_tests[]{
        new(dir, syscall.O_RDONLY, default!),
        new(dir, syscall.O_CREAT, default!),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_CREAT), default!),
        new(@file, (nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | os.O_CREATE), default!),
        new(@file, (nint)((nint)(nint)((nint)(nint)((nint)syscall.O_APPEND | (nint)syscall.O_WRONLY) | os.O_CREATE) | os.O_TRUNC), default!),
        new(dir, (nint)((nint)syscall.O_RDONLY | (nint)syscall.O_TRUNC), syscall.ERROR_ACCESS_DENIED),
        new(dir, (nint)((nint)syscall.O_WRONLY | (nint)syscall.O_RDWR), default!), // TODO: syscall.Open returns EISDIR here, we should reconcile this

        new(dir, syscall.O_WRONLY, syscall.EISDIR),
        new(dir, syscall.O_RDWR, syscall.EISDIR)
    }.slice();
    foreach (var (i, tt) in tests) {
        @string dirΔ1 = filepath.Dir(tt.path);
        var (dirfd, errΔ1) = syscall.Open(dirΔ1, syscall.O_RDONLY, 0);
        if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
            continue;
        }
        @string @base = filepath.Base(tt.path);
        (var h, errΔ1) = Δwindows.Openat(dirfd, @base, tt.flag, 432);
        syscall.CloseHandle(dirfd);
        if (errΔ1 == default!) {
            syscall.CloseHandle(h);
        }
        if (!AreEqual(errΔ1, tt.err)) {
            Ꮡt.Errorf("%d: Open got %q, want %q"u8, i, errΔ1, tt.err);
        }
    }
}

} // end windows_test_package
