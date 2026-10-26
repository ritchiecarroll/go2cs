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
internal static readonly @string testMkdirAllDirDir2ˢ = "_TestMkdirAll_/dir/./dir2"u8;
internal static readonly @string testMkdirAllDirDir2ˢ2 = @"_TestMkdirAll_\dir\.\dir2\"u8;

public static void TestMkdirAll(ж<Δtesting.T> Ꮡt) {
    testMaybeRooted(Ꮡt, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
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
            if (err != default!) {
                tΔ1.Fatalf("MkdirAll %q: %s"u8, path, err);
            }

            // Already exists, should succeed.
            err = mkdirAll(path, 511);
            if (err != default!) {
                tΔ1.Fatalf("MkdirAll %q (second time): %s"u8, path, err);
            }

            // Make file.
            @string fpath = path + "/file"u8;
            (var f, err) = create(fpath);
            if (err != default!) {
                tΔ1.Fatalf("create %q: %s"u8, fpath, err);
            }
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);

            // Can't make directory named after file.
            err = mkdirAll(fpath, 511);
            if (err == default!) {
                tΔ1.Fatalf("MkdirAll %q: no error"u8, fpath);
            }
            var (perr, ok) = err._<ж<fs.PathError>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("MkdirAll %q returned %T, not *PathError"u8, fpath, err);
            }
            if (filepath.Clean((~perr).Path) != filepath.Clean(fpath)) {
                tΔ1.Fatalf("MkdirAll %q returned wrong error path: %q not %q"u8, fpath, filepath.Clean((~perr).Path), filepath.Clean(fpath));
            }

            // Can't make subdirectory of file.
            @string ffpath = fpath + "/subdir"u8;
            err = mkdirAll(ffpath, 511);
            if (err == default!) {
                tΔ1.Fatalf("MkdirAll %q: no error"u8, ffpath);
            }
            (perr, ok) = err._<ж<fs.PathError>>(ᐧ);
            if (!ok) {
                tΔ1.Fatalf("MkdirAll %q returned %T, not *PathError"u8, ffpath, err);
            }
            if (filepath.Clean((~perr).Path) != filepath.Clean(fpath)) {
                tΔ1.Fatalf("MkdirAll %q returned wrong error path: %q not %q"u8, ffpath, filepath.Clean((~perr).Path), filepath.Clean(fpath));
            }

            if (Δruntime.GOOS == "windows"u8) {
                @string pathΔ1 = testMkdirAllDirDir2ˢ2;
                var errΔ1 = mkdirAll(pathΔ1, 511);
                if (errΔ1 != default!) {
                    tΔ1.Fatalf("MkdirAll %q: %s"u8, pathΔ1, errΔ1);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aBCˢ = "/a/b/c"u8;

public static void TestMkdirAllAbsPath(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    @string path = filepath.Join(tmpDir, aBCˢ);
    {
        var errΔ1 = MkdirAll(path, 511); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var (st, err) = Stat(path);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!st.IsDir()) {
        Ꮡt.Fatalf("after MkdirAll(%q, 0o777), %q is not a directory"u8, path, path);
    }
}

public static void TestMkdirAllWithSymlink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();

    @string tmpDir = Ꮡt.TempDir();
    @string dir = tmpDir + "/dir"u8;
    {
        var err = Mkdir(dir, 493); if (err != default!) {
            Ꮡt.Fatalf("Mkdir %s: %s"u8, dir, err);
        }
    }

    @string link = tmpDir + "/link"u8;
    {
        var err = Symlink(dirˢ, link); if (err != default!) {
            Ꮡt.Fatalf("Symlink %s: %s"u8, link, err);
        }
    }

    @string path = link + "/foo"u8;
    {
        var err = MkdirAll(path, 493); if (err != default!) {
            Ꮡt.Errorf("MkdirAll %q: %s"u8, path, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goOsTestˢ = "/_go_os_test"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dirᶜ = "/_go_os_test/dir"u8;

public static void TestMkdirAllAtSlash(ж<Δtesting.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8) {
        Ꮡt.Skipf("skipping on %s"u8, Δruntime.GOOS);
    }
    if (testenv.Builder() == ""u8) {
        Ꮡt.Skipf("skipping non-hermetic test outside of Go builders"u8);
    }

    RemoveAll(goOsTestˢ);
    @string dir = dirᶜ;
    var err = MkdirAll(dir, 511);
    if (err != default!) {
        var (pathErr, ok) = err._<ж<fs.PathError>>(ᐧ);
        // common for users not to be able to write to /
        if (ok && (AreEqual((~pathErr).Err, syscall.EACCES) || isReadonlyError((~pathErr).Err))) {
            Ꮡt.Skipf("could not create %v: %v"u8, dir, err);
        }
        Ꮡt.Fatalf(@"MkdirAll ""/_go_os_test/dir"": %v, %s"u8, err, (~pathErr).Err);
    }
    RemoveAll(goOsTestˢ);
}

} // end os_test_package
