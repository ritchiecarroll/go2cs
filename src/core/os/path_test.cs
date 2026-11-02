// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testenv = @internal.testenv_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using @internal;
using fs = go.io.fs_package;
using go.io;
using go.path;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

internal static Func<error, bool> isReadonlyError = (error _) => false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testMkdirAllDirDir2ˢ => ref testMkdirAllDirDir2ˢᵇ;
private static readonly @string testMkdirAllDirDir2ˢᵇ = "_TestMkdirAll_/dir/./dir2"u8;
internal static ref readonly @string testMkdirAllDirDir2ˢ2 => ref testMkdirAllDirDir2ˢ2ᵇ;
private static readonly @string testMkdirAllDirDir2ˢ2ᵇ = @"_TestMkdirAll_\dir\.\dir2\"u8;

public static void TestMkdirAll(ж<Δtesting.T> t) {
    testMaybeRooted(t, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        GoFrame ᒐ = default;
        try {
            Func<@string, fs.FileMode, error> mkdirAll = MkdirAll;
            Func<@string, (ж<Δos.File>, error)> create = Create;
            if (r != nil) {
                mkdirAll = (@string p1, fs.FileMode p2) => r.MkdirAll(p1, p2);
                create = (@string p1) => r.Create(p1);
            }

            @string path = testMkdirAllDirDir2ˢ;
            var err = mkdirAll(path, 511);
            if (err != nil) {
                tΔ1.Fatalf("MkdirAll %q: %s", path, err);
            }

            // Already exists, should succeed.
            err = mkdirAll(path, 511);
            if (err != nil) {
                tΔ1.Fatalf("MkdirAll %q (second time): %s", path, err);
            }

            // Make file.
            @string fpath = path + "/file"u8;
            (var f, err) = create(fpath);
            if (err != nil) {
                tΔ1.Fatalf("create %q: %s", fpath, err);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);

            // Can't make directory named after file.
            err = mkdirAll(fpath, 511);
            if (err == nil) {
                tΔ1.Fatalf("MkdirAll %q: no error", fpath);
            }
            var (perr, ok) = err._<ж<fs.PathError>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("MkdirAll %q returned %T, not *PathError", fpath, err);
            }
            if (filepath.Clean(perr.Path) != filepath.Clean(fpath)) {
                tΔ1.Fatalf("MkdirAll %q returned wrong error path: %q not %q", fpath, filepath.Clean(perr.Path), filepath.Clean(fpath));
            }

            // Can't make subdirectory of file.
            @string ffpath = fpath + "/subdir"u8;
            err = mkdirAll(ffpath, 511);
            if (err == nil) {
                tΔ1.Fatalf("MkdirAll %q: no error", ffpath);
            }
            (perr, ok) = err._<ж<fs.PathError>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("MkdirAll %q returned %T, not *PathError", ffpath, err);
            }
            if (filepath.Clean(perr.Path) != filepath.Clean(fpath)) {
                tΔ1.Fatalf("MkdirAll %q returned wrong error path: %q not %q", ffpath, filepath.Clean(perr.Path), filepath.Clean(fpath));
            }

            if (Δruntime.GOOS == "windows"u8) {
                @string pathΔ1 = testMkdirAllDirDir2ˢ2;
                var errΔ1 = mkdirAll(pathΔ1, 511);
                if (errΔ1 != nil) {
                    tΔ1.Fatalf("MkdirAll %q: %s", pathΔ1, errΔ1);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aBCˢ => ref aBCˢᵇ;
private static readonly @string aBCˢᵇ = "/a/b/c"u8;

public static void TestMkdirAllAbsPath(ж<Δtesting.T> t) {
    t.Parallel();
    @string tmpDir = t.TempDir();
    @string path = filepath.Join(tmpDir, aBCˢ);
    {
        var errΔ1 = MkdirAll(path, 511); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    var (st, err) = Stat(path);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!st.IsDir()) {
        t.Fatalf("after MkdirAll(%q, 0o777), %q is not a directory", path, path);
    }
}

public static void TestMkdirAllWithSymlink(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    t.Parallel();

    @string tmpDir = t.TempDir();
    @string dir = tmpDir + "/dir"u8;
    {
        var err = Mkdir(dir, 493); if (err != nil) {
            t.Fatalf("Mkdir %s: %s", dir, err);
        }
    }

    @string link = tmpDir + "/link"u8;
    {
        var err = Symlink(dirˢ, link); if (err != nil) {
            t.Fatalf("Symlink %s: %s", link, err);
        }
    }

    @string path = link + "/foo"u8;
    {
        var err = MkdirAll(path, 493); if (err != nil) {
            t.Errorf("MkdirAll %q: %s", path, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goOsTestˢ => ref goOsTestˢᵇ;
private static readonly @string goOsTestˢᵇ = "/_go_os_test"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dirᶜ => ref dirᶜᵇ;
private static readonly @string dirᶜᵇ = "/_go_os_test/dir"u8;

public static void TestMkdirAllAtSlash(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8: {
        t.Skipf("skipping on %s", Δruntime.GOOS);
        break;
    }}
    if (testenv.Builder() == ""u8) {
        t.Skipf("skipping non-hermetic test outside of Go builders");
    }

    RemoveAll(goOsTestˢ);
    @string dir = dirᶜ;
    var err = MkdirAll(dir, 511);
    if (err != nil) {
        var (pathErr, ok) = err._<ж<fs.PathError>>(ᐧ);
        // common for users not to be able to write to /
        if (ok && (AreEqual(pathErr.Err, syscall.EACCES) || isReadonlyError(pathErr.Err))) {
            t.Skipf("could not create %v: %v", dir, err);
        }
        t.Fatalf(@"MkdirAll ""/_go_os_test/dir"": %v, %s", err, pathErr.Err);
    }
    RemoveAll(goOsTestˢ);
}

} // end os_test_package
