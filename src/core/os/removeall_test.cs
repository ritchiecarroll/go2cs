// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using static os_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using Δtesting = testing_package;
using @internal;
using exec = go.os.exec_package;
using fs = go.io.fs_package;
using go.os;
using go.path;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRemoveAllˢ => ref testRemoveAllˢᵇ;
private static readonly @string testRemoveAllˢᵇ = "_TestRemoveAll_"u8;

public static void TestRemoveAll(ж<Δtesting.T> t) {
    t.Parallel();

    @string tmpDir = t.TempDir();
    var errΔ1 = RemoveAll(""u8); if (errΔ1 != nil) {
        t.Errorf("RemoveAll(\"\"): %v; want nil", errΔ1);
    }

    @string @file = filepath.Join(tmpDir, fileˢ2);
    @string path = filepath.Join(tmpDir, testRemoveAllˢ);
    @string fpath = filepath.Join(path, fileˢ2);
    @string dpath = filepath.Join(path, dirˢ);

    // Make a regular file and remove
    var (fd, err) = Create(@file);
    if (err != nil) {
        t.Fatalf("create %q: %s", @file, err);
    }
    fd.Close();
    err = RemoveAll(@file); if (err != nil) {
        t.Fatalf("RemoveAll %q (first): %s", @file, err);
    }
    (_, err) = Lstat(@file); if (err == nil) {
        t.Fatalf("Lstat %q succeeded after RemoveAll (first)", @file);
    }

    // Make directory with 1 file and remove.
    var errΔ2 = MkdirAll(path, 511); if (errΔ2 != nil) {
        t.Fatalf("MkdirAll %q: %s", path, errΔ2);
    }
    (fd, err) = Create(fpath);
    if (err != nil) {
        t.Fatalf("create %q: %s", fpath, err);
    }
    fd.Close();
    err = RemoveAll(path); if (err != nil) {
        t.Fatalf("RemoveAll %q (second): %s", path, err);
    }
    (_, err) = Lstat(path); if (err == nil) {
        t.Fatalf("Lstat %q succeeded after RemoveAll (second)", path);
    }

    // Make directory with file and subdirectory and remove.
    err = MkdirAll(dpath, 511); if (err != nil) {
        t.Fatalf("MkdirAll %q: %s", dpath, err);
    }
    (fd, err) = Create(fpath);
    if (err != nil) {
        t.Fatalf("create %q: %s", fpath, err);
    }
    fd.Close();
    (fd, err) = Create(dpath + "/file"u8);
    if (err != nil) {
        t.Fatalf("create %q: %s", fpath, err);
    }
    fd.Close();
    err = RemoveAll(path); if (err != nil) {
        t.Fatalf("RemoveAll %q (third): %s", path, err);
    }
    var (_, errΔ3) = Lstat(path); if (errΔ3 == nil) {
        t.Fatalf("Lstat %q succeeded after RemoveAll (third)", path);
    }

    // Chmod is not supported under Windows or wasip1 and test fails as root.
    if (Δruntime.GOOS != "windows"u8 && Δruntime.GOOS != "wasip1"u8 && Getuid() != 0) {
        // Make directory with file and subdirectory and trigger error.
        err = MkdirAll(dpath, 511); if (err != nil) {
            t.Fatalf("MkdirAll %q: %s", dpath, err);
        }

        foreach (var (_, s) in range(slice<@string>([fpath, dpath + "/file1"u8, path + "/zzz"u8]))) {
            (fd, err) = Create(s);
            if (err != nil) {
                t.Fatalf("create %q: %s", s, err);
            }
            fd.Close();
        }
        err = Chmod(dpath, 0); if (err != nil) {
            t.Fatalf("Chmod %q 0: %s", dpath, err);
        }

        // No error checking here: either RemoveAll
        // will or won't be able to remove dpath;
        // either way we want to see if it removes fpath
        // and path/zzz. Reasons why RemoveAll might
        // succeed in removing dpath as well include:
        //	* running as root
        //	* running on a file system without permissions (FAT)
        RemoveAll(path);
        Chmod(dpath, 511);

        foreach (var (_, s) in range(slice<@string>([fpath, path + "/zzz"u8]))) {
            (_, err) = Lstat(s); if (err == nil) {
                t.Fatalf("Lstat %q succeeded after partial RemoveAll", s);
            }
        }
    }
    err = RemoveAll(path); if (err != nil) {
        t.Fatalf("RemoveAll %q after partial RemoveAll: %s", path, err);
    }
    (_, err) = Lstat(path); if (err == nil) {
        t.Fatalf("Lstat %q succeeded after RemoveAll (final)", path);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static ref readonly @string testRemoveAllLargeˢ => ref testRemoveAllLargeˢᵇ;
private static readonly @string testRemoveAllLargeˢᵇ = "_TestRemoveAllLarge_"u8;

// Test RemoveAll on a large directory.
public static void TestRemoveAllLarge(ж<Δtesting.T> t) {
    if (Δtesting.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    t.Parallel();

    @string tmpDir = t.TempDir();
    @string path = filepath.Join(tmpDir, testRemoveAllLargeˢ);

    // Make directory with 1000 files and remove.
    {
        var err = MkdirAll(path, 511); if (err != nil) {
            t.Fatalf("MkdirAll %q: %s", path, err);
        }
    }
    for (nint i = 0; i < 1000; i++) {
        @string fpath = fmt.Sprintf("%s/file%d"u8, path, i);
        var (fd, err) = Create(fpath);
        if (err != nil) {
            t.Fatalf("create %q: %s", fpath, err);
        }
        fd.Close();
    }
    {
        var err = RemoveAll(path); if (err != nil) {
            t.Fatalf("RemoveAll %q: %s", path, err);
        }
    }
    {
        var (_, err) = Lstat(path); if (err == nil) {
            t.Fatalf("Lstat %q succeeded after RemoveAll", path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingForNotˢ = (@string)"skipping for not implemented platforms"u8;

public static void TestRemoveAllLongPath(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8: {
        break;
        break;
    }
    default: {
        t.Skip(skippingForNotˢ);
        break;
    }}

    @string startPath = t.TempDir();
    t.Chdir(startPath);

    // Removing paths with over 4096 chars commonly fails.
    @string name = strings.Repeat("a", 100);
    for (nint i = 0; i < 41; i++) {
        var errΔ1 = Mkdir(name, 493); if (errΔ1 != nil) {
            t.Fatalf("Could not mkdir %s: %s", name, errΔ1);
        }
        var errΔ2 = Chdir(name); if (errΔ2 != nil) {
            t.Fatalf("Could not chdir %s: %s", name, errΔ2);
        }
    }

    // Chdir out of startPath before attempting to remove it,
    // otherwise RemoveAll fails on aix, illumos and solaris.
    var err = Chdir(filepath.Join(startPath, ".."));
    if (err != nil) {
        t.Fatalf("Could not chdir: %s", err);
    }

    err = RemoveAll(startPath);
    if (err != nil) {
        t.Errorf("RemoveAll could not remove long file path %s: %s", startPath, err);
    }
}

public static void TestRemoveAllDot(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());

    var err = RemoveAll("."); if (err == nil) {
        t.Errorf("RemoveAll succeed to remove .");
    }
}

public static void TestRemoveAllDotDot(ж<Δtesting.T> t) {
    t.Parallel();

    @string tempDir = t.TempDir();
    @string subdir = filepath.Join(tempDir, "x");
    @string subsubdir = filepath.Join(subdir, "y");
    {
        var err = MkdirAll(subsubdir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = RemoveAll(filepath.Join(subsubdir, "..")); if (err != nil) {
            t.Error(err);
        }
    }
    foreach (var (_, dir) in range(slice<@string>([subsubdir, subdir]))) {
        var (_, err) = Stat(dir); if (err == nil) {
            t.Errorf("%s: exists after RemoveAll", dir);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object subdirectoryWasNotˢ = (@string)"subdirectory was not removed"u8;

// Issue #29178.
public static void TestRemoveReadOnlyDir(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        @string tempDir = t.TempDir();
        @string subdir = filepath.Join(tempDir, "x");
        {
            var err = Mkdir(subdir, 0); if (err != nil) {
                t.Fatal(err);
            }
        }

        // If an error occurs make it more likely that removing the
        // temporary directory will succeed.
        defer(Chmod, subdir, (fs.FileMode)511, ref ᒐ);

        {
            var err = RemoveAll(subdir); if (err != nil) {
                t.Fatal(err);
            }
        }

        {
            var (_, err) = Stat(subdir); if (err == nil) {
                t.Error(subdirectoryWasNotˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object removeAllSucceededˢ = (@string)"RemoveAll succeeded unexpectedly"u8;

// Issue #29983.
public static void TestRemoveAllButReadOnlyAndPathError(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        testRequiresPermissions(t);

        t.Parallel();

        @string tempDir = t.TempDir();
        var dirs = slice<@string>([
            "a",
            "a/x",
            "a/x/1",
            "b",
            "b/y",
            "b/y/2",
            "c",
            "c/z",
            "c/z/3"
        ]);
        var @readonly = slice<@string>([
            "b"
        ]);
        var readonlyʗ1 = @readonly;
        bool inReadonly(@string d) {
            foreach (var (_, ro) in range(readonlyʗ1)) {
                if (d == ro) {
                    return true;
                }
                var (dd, _) = filepath.Split(d);
                if (filepath.Clean(dd) == ro) {
                    return true;
                }
            }
            return false;
        }

        foreach (var (_, dir) in range(dirs)) {
            var errΔ1 = Mkdir(filepath.Join(tempDir, dir), 511); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        foreach (var (_, dir) in range(@readonly)) {
            @string d = filepath.Join(tempDir, dir);
            var errΔ2 = Chmod(d, 365); if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }

            // Defer changing the mode back so that the deferred
            // RemoveAll(tempDir) can succeed.
            defer(Chmod, d, (fs.FileMode)511, ref ᒐ);
        }

        var err = RemoveAll(tempDir);
        if (err == nil) {
            t.Fatal(removeAllSucceededˢ);
        }

        // The error should be of type *PathError.
        // see issue 30491 for details.
        var (pathErr, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok) {
            @string want = filepath.Join(tempDir, "b", "y");
            if (pathErr.Path != want) {
                t.Errorf("RemoveAll(%q): err.Path=%q, want %q", tempDir, pathErr.Path, want);
            }
        } else {
            t.Errorf("RemoveAll(%q): error has type %T, want *fs.PathError", tempDir, err);
        }

        foreach (var (_, dir) in range(dirs)) {
            var (_, errΔ3) = Stat(filepath.Join(tempDir, dir));
            if (inReadonly(dir)) {
                if (errΔ3 != nil) {
                    t.Errorf("file %q was deleted but should still exist", dir);
                }
            } else {
                if (errΔ3 == nil) {
                    t.Errorf("file %q still exists but should have been deleted", dir);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRemoveUnreadableDir(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8: {
        t.Skipf("skipping test on %s", Δruntime.GOOS);
        break;
    }}

    if (Getuid() == 0) {
        t.Skip(skippingTestWhenRunningˢ);
    }

    t.Parallel();

    @string tempDir = t.TempDir();
    @string target = filepath.Join(tempDir, "d0", "d1", "d2");
    {
        var err = MkdirAll(target, 493); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = Chmod(target, 192); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = RemoveAll(filepath.Join(tempDir, "d0")); if (err != nil) {
            t.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object removeAllReadOnlyˢ = (@string)"RemoveAll(<read-only directory>) = nil; want error"u8;

// Issue 29921
public static void TestRemoveAllWithMoreErrorThanReqSize(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        if (Δtesting.Short()) {
            t.Skip(skippingInShortModeˢ);
        }
        t.Parallel();

        @string tmpDir = t.TempDir();
        @string path = filepath.Join(tmpDir, "_TestRemoveAllWithMoreErrorThanReqSize_");

        // Make directory with 1025 read-only files.
        var errΔ1 = MkdirAll(path, 511); if (errΔ1 != nil) {
            t.Fatalf("MkdirAll %q: %s", path, errΔ1);
        }
        for (nint i = 0; i < 1025; i++) {
            @string fpath = filepath.Join(path, fmt.Sprintf("file%d"u8, i));
            var (fd, errΔ2) = Create(fpath);
            if (errΔ2 != nil) {
                t.Fatalf("create %q: %s", fpath, errΔ2);
            }
            fd.Close();
        }

        // Make the parent directory read-only. On some platforms, this is what
        // prevents Remove from removing the files within that directory.
        var errΔ3 = Chmod(path, 365); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }
        defer(Chmod, path, (fs.FileMode)493, ref ᒐ);

        // This call should not hang, even on a platform that disallows file deletion
        // from read-only directories.
        var err = RemoveAll(path);

        if (Getuid() == 0) {
            // On many platforms, root can remove files from read-only directories.
            return;
        }
        if (err == nil) {
            if (Δruntime.GOOS == "windows"u8 || Δruntime.GOOS == "wasip1"u8) {
                // Marking a directory as read-only in Windows does not prevent the RemoveAll
                // from creating or removing files within it.
                //
                // For wasip1, there is no support for file permissions so we cannot prevent
                // RemoveAll from removing the files.
                return;
            }
            t.Fatal(removeAllReadOnlyˢ);
        }

        (var dir, err) = Open(path);
        if (err != nil) {
            t.Fatal(err);
        }
        var dirʗ1 = dir;
        defer(() => dirʗ1.Close(), ref ᒐ);

        var (names, _) = dir.Readdirnames(1025);
        if (len(names) < 1025) {
            t.Fatalf("RemoveAll(<read-only directory>) unexpectedly removed %d read-only files from that directory", 1025 - len(names));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string binStraceˢ => ref binStraceˢᵇ;
private static readonly @string binStraceˢᵇ = "/bin/strace"u8;
internal static ref readonly @string subdirˢ => ref subdirˢᵇ;
private static readonly @string subdirˢᵇ = "subdir"u8;
internal static ref readonly @string fcntlˢ => ref fcntlˢᵇ;
private static readonly @string fcntlˢᵇ = "fcntl"u8;
internal static ref readonly @string testRunˢ4 => ref testRunˢ4ᵇ;
private static readonly @string testRunˢ4ᵇ = "-test.run=^TestRemoveAllNoFcntl$"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string envᶜ => ref envᶜᵇ;
private static readonly @string envᶜᵇ = "GO_TEST_REMOVE_ALL_NO_FCNTL"u8;

public static void TestRemoveAllNoFcntl(ж<Δtesting.T> t) {
    if (Δtesting.Short()) {
        t.Skip(skippingInShortModeˢ);
    }

    @string env = envᶜ;
    @string dir = Getenv(env); if (dir != ""u8) {
        var errΔ1 = RemoveAll(dir); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        return;
    }

    // Only test on Linux so that we can assume we have strace.
    // The code is OS-independent so if it passes on Linux
    // it should pass on other Unix systems.
    if (Δruntime.GOOS != "linux"u8) {
        t.Skipf("skipping test on %s", Δruntime.GOOS);
    }
    var (_, errΔ2) = Stat(binStraceˢ); if (errΔ2 != nil) {
        t.Skipf("skipping test because /bin/strace not found: %v", errΔ2);
    }
    var (me, err) = Executable();
    if (err != nil) {
        t.Skipf("skipping because Executable failed: %v", err);
    }

    // Create 100 directories.
    // The test is that we can remove them without calling fcntl
    // on each one.
    @string tmpdir = t.TempDir();
    @string subdir = filepath.Join(tmpdir, subdirˢ);
    var errΔ3 = Mkdir(subdir, 493); if (errΔ3 != nil) {
        t.Fatal(errΔ3);
    }
    for (nint i = 0; i < 100; i++) {
        @string subsubdir = filepath.Join(subdir, strconv.Itoa(i));
        var errΔ4 = Mkdir(filepath.Join(subdir, strconv.Itoa(i)), 493); if (errΔ4 != nil) {
            t.Fatal(errΔ4);
        }
        var errΔ5 = WriteFile(filepath.Join(subsubdir, fileˢ2), default, 420); if (errΔ5 != nil) {
            t.Fatal(errΔ5);
        }
    }

    var cmd = testenv.Command(new os_test_package.testing_TжTB(t), binStraceˢ, "-f", "-e", fcntlˢ, me, testRunˢ4);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, env + "=" + subdir);
    (var @out, err) = cmd.CombinedOutput();
    if (len(@out) > 0) {
        t.Logf("%s", @out);
    }
    if (err != nil) {
        t.Fatal(err);
    }

    nint got = bytes.Count(@out, slice<byte>("fcntl"u8)); if (got >= 100) {
        t.Errorf("found %d fcntl calls, want < 100", got);
    }
}

public static void TestRemoveAllTrailingSlash(ж<Δtesting.T> t) {
    var slashesᴷ = new stackAppend<@string>();

    var slashes = slice<@string>(["/"]);
    if (Δruntime.GOOS == "windows"u8) {
        slashes = append(slashesᴷ, slashes, (@string)@"\");
    }
    foreach (var (_, slash) in range(slashes)) {
        @string dir = makefs(t, [
            "dir/a/file1",
            "dir/a/file2",
            "dir/file3"
        ]);
        @string path = dir + "/dir"u8;
        {
            var err = RemoveAll(path + slash); if (err != nil) {
                t.Fatal(err);
            }
        }
        {
            var (_, err) = Stat(path); if (!IsNotExist(err)) {
                t.Errorf("after RemoveAll(%q), directory still exists", path + slash);
            }
        }
    }
}

public static void TestRemoveAllSymlinkRemovalFailure(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        testRequiresPermissions(t);
        @string dir = makefs(t, [
            "parent/",
            "parent/link => target"
        ]);
        var errΔ1 = Chmod(dir + "/parent"u8, 365); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        defer(() => {
            Chmod(dir + "/parent"u8, 493);
        }, ref ᒐ);

        var err = RemoveAll(dir + "/parent/link"u8);
        if (!errors.Is(err, ErrPermission)) {
            t.Fatalf("RemoveAll = %v; want ErrPermission", err);
        }
        // Issue #78490: RemoveAll leaked errSymlink, which panics when printed.
        _ = err.Error();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestRunningAsˢ = (@string)"skipping test: running as root (which ignores file permissions)"u8;

internal static void testRequiresPermissions(ж<Δtesting.T> t) {
    t.Helper();
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8 || exprᴛ1 == "windows"u8: {
        t.Skipf("skipping test: %s does not support chmod", Δruntime.GOOS);
        break;
    }}
    if (Getuid() == 0) {
        t.Skip(skippingTestRunningAsˢ);
    }
}

public static void BenchmarkRemoveAll(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string tmpDir = filepath.Join(Ꮡb.TempDir(), targetˢ);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var err = CopyFS(tmpDir, DirFS("."));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        b.StartTimer();
        var errΔ1 = RemoveAll(tmpDir); if (errΔ1 != nil) {
            Ꮡb.Fatal(errΔ1);
        }
    }
}

} // end os_test_package
