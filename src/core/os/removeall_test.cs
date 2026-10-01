// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
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
internal static readonly @string testRemoveAllˢ = "_TestRemoveAll_"u8;

public static void TestRemoveAll(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    {
        var errΔ1 = RemoveAll(""u8); if (errΔ1 != default!) {
            Ꮡt.Errorf("RemoveAll(\"\"): %v; want nil"u8, errΔ1);
        }
    }
    @string @file = filepath.Join(tmpDir, fileˢ2);
    @string path = filepath.Join(tmpDir, testRemoveAllˢ);
    @string fpath = filepath.Join(path, fileˢ2);
    @string dpath = filepath.Join(path, dirˢ);
    // Make a regular file and remove
    var (fd, err) = Create(@file);
    if (err != default!) {
        Ꮡt.Fatalf("create %q: %s"u8, @file, err);
    }
    fd.Close();
    {
        err = RemoveAll(@file); if (err != default!) {
            Ꮡt.Fatalf("RemoveAll %q (first): %s"u8, @file, err);
        }
    }
    {
        (_, err) = Lstat(@file); if (err == default!) {
            Ꮡt.Fatalf("Lstat %q succeeded after RemoveAll (first)"u8, @file);
        }
    }
    // Make directory with 1 file and remove.
    {
        var errΔ2 = MkdirAll(path, 511); if (errΔ2 != default!) {
            Ꮡt.Fatalf("MkdirAll %q: %s"u8, path, errΔ2);
        }
    }
    (fd, err) = Create(fpath);
    if (err != default!) {
        Ꮡt.Fatalf("create %q: %s"u8, fpath, err);
    }
    fd.Close();
    {
        err = RemoveAll(path); if (err != default!) {
            Ꮡt.Fatalf("RemoveAll %q (second): %s"u8, path, err);
        }
    }
    {
        (_, err) = Lstat(path); if (err == default!) {
            Ꮡt.Fatalf("Lstat %q succeeded after RemoveAll (second)"u8, path);
        }
    }
    // Make directory with file and subdirectory and remove.
    {
        err = MkdirAll(dpath, 511); if (err != default!) {
            Ꮡt.Fatalf("MkdirAll %q: %s"u8, dpath, err);
        }
    }
    (fd, err) = Create(fpath);
    if (err != default!) {
        Ꮡt.Fatalf("create %q: %s"u8, fpath, err);
    }
    fd.Close();
    (fd, err) = Create(dpath + "/file"u8);
    if (err != default!) {
        Ꮡt.Fatalf("create %q: %s"u8, fpath, err);
    }
    fd.Close();
    {
        err = RemoveAll(path); if (err != default!) {
            Ꮡt.Fatalf("RemoveAll %q (third): %s"u8, path, err);
        }
    }
    {
        var (_, errΔ3) = Lstat(path); if (errΔ3 == default!) {
            Ꮡt.Fatalf("Lstat %q succeeded after RemoveAll (third)"u8, path);
        }
    }
    // Chmod is not supported under Windows or wasip1 and test fails as root.
    if (Δruntime.GOOS != "windows"u8 && Δruntime.GOOS != "wasip1"u8 && Getuid() != 0) {
        // Make directory with file and subdirectory and trigger error.
        {
            err = MkdirAll(dpath, 511); if (err != default!) {
                Ꮡt.Fatalf("MkdirAll %q: %s"u8, dpath, err);
            }
        }
        foreach (var (_, s) in new @string[]{fpath, dpath + "/file1"u8, path + "/zzz"u8}.slice()) {
            (fd, err) = Create(s);
            if (err != default!) {
                Ꮡt.Fatalf("create %q: %s"u8, s, err);
            }
            fd.Close();
        }
        {
            err = Chmod(dpath, 0); if (err != default!) {
                Ꮡt.Fatalf("Chmod %q 0: %s"u8, dpath, err);
            }
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
        foreach (var (_, s) in new @string[]{fpath, path + "/zzz"u8}.slice()) {
            {
                (_, err) = Lstat(s); if (err == default!) {
                    Ꮡt.Fatalf("Lstat %q succeeded after partial RemoveAll"u8, s);
                }
            }
        }
    }
    {
        err = RemoveAll(path); if (err != default!) {
            Ꮡt.Fatalf("RemoveAll %q after partial RemoveAll: %s"u8, path, err);
        }
    }
    {
        (_, err) = Lstat(path); if (err == default!) {
            Ꮡt.Fatalf("Lstat %q succeeded after RemoveAll (final)"u8, path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static readonly @string testRemoveAllLargeˢ = "_TestRemoveAllLarge_"u8;

// Test RemoveAll on a large directory.
public static void TestRemoveAllLarge(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    @string path = filepath.Join(tmpDir, testRemoveAllLargeˢ);
    // Make directory with 1000 files and remove.
    {
        var err = MkdirAll(path, 511); if (err != default!) {
            Ꮡt.Fatalf("MkdirAll %q: %s"u8, path, err);
        }
    }
    for (nint i = 0; i < 1000; i++) {
        @string fpath = fmt.Sprintf("%s/file%d"u8, path, i);
        var (fd, err) = Create(fpath);
        if (err != default!) {
            Ꮡt.Fatalf("create %q: %s"u8, fpath, err);
        }
        fd.Close();
    }
    {
        var err = RemoveAll(path); if (err != default!) {
            Ꮡt.Fatalf("RemoveAll %q: %s"u8, path, err);
        }
    }
    {
        var (_, err) = Lstat(path); if (err == default!) {
            Ꮡt.Fatalf("Lstat %q succeeded after RemoveAll"u8, path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingForNotˢ = (@string)"skipping for not implemented platforms"u8;

public static void TestRemoveAllLongPath(ж<Δtesting.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "aix"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8 || exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8) {
        do {
            break;
        } while (false);
    }
    else { /* default: */
        Ꮡt.Skip(skippingForNotˢ);
    }

    @string startPath = Ꮡt.TempDir();
    Ꮡt.Chdir(startPath);
    // Removing paths with over 4096 chars commonly fails.
    @string name = strings.Repeat("a"u8, 100);
    for (nint i = 0; i < 41; i++) {
        {
            var errΔ1 = Mkdir(name, 493); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Could not mkdir %s: %s"u8, name, errΔ1);
            }
        }
        {
            var errΔ2 = Chdir(name); if (errΔ2 != default!) {
                Ꮡt.Fatalf("Could not chdir %s: %s"u8, name, errΔ2);
            }
        }
    }
    // Chdir out of startPath before attempting to remove it,
    // otherwise RemoveAll fails on aix, illumos and solaris.
    var err = Chdir(filepath.Join(startPath, ".."));
    if (err != default!) {
        Ꮡt.Fatalf("Could not chdir: %s"u8, err);
    }
    err = RemoveAll(startPath);
    if (err != default!) {
        Ꮡt.Errorf("RemoveAll could not remove long file path %s: %s"u8, startPath, err);
    }
}

public static void TestRemoveAllDot(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    {
        var err = RemoveAll("."u8); if (err == default!) {
            Ꮡt.Errorf("RemoveAll succeed to remove ."u8);
        }
    }
}

public static void TestRemoveAllDotDot(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tempDir = Ꮡt.TempDir();
    @string subdir = filepath.Join(tempDir, "x");
    @string subsubdir = filepath.Join(subdir, "y");
    {
        var err = MkdirAll(subsubdir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = RemoveAll(filepath.Join(subsubdir, "..")); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    foreach (var (_, dir) in new @string[]{subsubdir, subdir}.slice()) {
        {
            var (_, err) = Stat(dir); if (err == default!) {
                Ꮡt.Errorf("%s: exists after RemoveAll"u8, dir);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object subdirectoryWasNotˢ = (@string)"subdirectory was not removed"u8;

// Issue #29178.
public static void TestRemoveReadOnlyDir(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        @string tempDir = Ꮡt.TempDir();
        @string subdir = filepath.Join(tempDir, "x");
        {
            var err = Mkdir(subdir, 0); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        // If an error occurs make it more likely that removing the
        // temporary directory will succeed.
        defer(Chmod, subdir, (fs.FileMode)(511), ref ᒐ);
        {
            var err = RemoveAll(subdir); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        {
            var (_, err) = Stat(subdir); if (err == default!) {
                Ꮡt.Error(subdirectoryWasNotˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object removeAllSucceededˢ = (@string)"RemoveAll succeeded unexpectedly"u8;

// Issue #29983.
public static void TestRemoveAllButReadOnlyAndPathError(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8 || exprᴛ1 == "windows"u8) {
            Ꮡt.Skipf("skipping test on %s"u8, Δruntime.GOOS);
        }

        if (Getuid() == 0) {
            Ꮡt.Skip(skippingTestWhenRunningˢ);
        }
        Ꮡt.Parallel();
        @string tempDir = Ꮡt.TempDir();
        var dirs = new @string[]{
            "a"u8,
            "a/x"u8,
            "a/x/1"u8,
            "b"u8,
            "b/y"u8,
            "b/y/2"u8,
            "c"u8,
            "c/z"u8,
            "c/z/3"u8
        }.slice();
        var @readonly = new @string[]{
            "b"u8
        }.slice();
        var readonlyʗ1 = @readonly;
        bool inReadonly(@string d) {
            foreach (var (_, ro) in readonlyʗ1) {
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
        foreach (var (_, dir) in dirs) {
            {
                var errΔ1 = Mkdir(filepath.Join(tempDir, dir), 511); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }
        foreach (var (_, dir) in @readonly) {
            @string d = filepath.Join(tempDir, dir);
            {
                var errΔ2 = Chmod(d, 365); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
            // Defer changing the mode back so that the deferred
            // RemoveAll(tempDir) can succeed.
            defer(Chmod, d, (fs.FileMode)(511), ref ᒐ);
        }
        var err = RemoveAll(tempDir);
        if (err == default!) {
            Ꮡt.Fatal(removeAllSucceededˢ);
        }
        // The error should be of type *PathError.
        // see issue 30491 for details.
        {
            var (pathErr, ok) = err._<ж<fs.PathError>>(ᐧ); if (ok){
                @string want = filepath.Join(tempDir, "b", "y");
                if ((~pathErr).Path != want) {
                    Ꮡt.Errorf("RemoveAll(%q): err.Path=%q, want %q"u8, tempDir, (~pathErr).Path, want);
                }
            } else {
                Ꮡt.Errorf("RemoveAll(%q): error has type %T, want *fs.PathError"u8, tempDir, err);
            }
        }
        foreach (var (_, dir) in dirs) {
            var (_, errΔ3) = Stat(filepath.Join(tempDir, dir));
            if (inReadonly(dir)){
                if (errΔ3 != default!) {
                    Ꮡt.Errorf("file %q was deleted but should still exist"u8, dir);
                }
            } else {
                if (errΔ3 == default!) {
                    Ꮡt.Errorf("file %q still exists but should have been deleted"u8, dir);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRemoveUnreadableDir(ж<Δtesting.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8) {
        Ꮡt.Skipf("skipping test on %s"u8, Δruntime.GOOS);
    }

    if (Getuid() == 0) {
        Ꮡt.Skip(skippingTestWhenRunningˢ);
    }
    Ꮡt.Parallel();
    @string tempDir = Ꮡt.TempDir();
    @string target = filepath.Join(tempDir, "d0", "d1", "d2");
    {
        var err = MkdirAll(target, 493); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = Chmod(target, 192); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = RemoveAll(filepath.Join(tempDir, "d0")); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object removeAllReadOnlyˢ = (@string)"RemoveAll(<read-only directory>) = nil; want error"u8;

// Issue 29921
public static void TestRemoveAllWithMoreErrorThanReqSize(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δtesting.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        Ꮡt.Parallel();
        @string tmpDir = Ꮡt.TempDir();
        @string path = filepath.Join(tmpDir, "_TestRemoveAllWithMoreErrorThanReqSize_");
        // Make directory with 1025 read-only files.
        {
            var errΔ1 = MkdirAll(path, 511); if (errΔ1 != default!) {
                Ꮡt.Fatalf("MkdirAll %q: %s"u8, path, errΔ1);
            }
        }
        for (nint i = 0; i < 1025; i++) {
            @string fpath = filepath.Join(path, fmt.Sprintf("file%d"u8, i));
            var (fd, errΔ2) = Create(fpath);
            if (errΔ2 != default!) {
                Ꮡt.Fatalf("create %q: %s"u8, fpath, errΔ2);
            }
            fd.Close();
        }
        // Make the parent directory read-only. On some platforms, this is what
        // prevents Remove from removing the files within that directory.
        {
            var errΔ3 = Chmod(path, 365); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        defer(Chmod, path, (fs.FileMode)(493), ref ᒐ);
        // This call should not hang, even on a platform that disallows file deletion
        // from read-only directories.
        var err = RemoveAll(path);
        if (Getuid() == 0) {
            // On many platforms, root can remove files from read-only directories.
            return;
        }
        if (err == default!) {
            if (Δruntime.GOOS == "windows"u8 || Δruntime.GOOS == "wasip1"u8) {
                // Marking a directory as read-only in Windows does not prevent the RemoveAll
                // from creating or removing files within it.
                //
                // For wasip1, there is no support for file permissions so we cannot prevent
                // RemoveAll from removing the files.
                return;
            }
            Ꮡt.Fatal(removeAllReadOnlyˢ);
        }
        (var dir, err) = Open(path);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dirʗ1 = dir;
        defer(() => dirʗ1.Close(), ref ᒐ);
        var (names, _) = dir.Readdirnames(1025);
        if (len(names) < 1025) {
            Ꮡt.Fatalf("RemoveAll(<read-only directory>) unexpectedly removed %d read-only files from that directory"u8, 1025 - len(names));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string binStraceˢ = "/bin/strace"u8;
internal static readonly @string subdirˢ = "subdir"u8;
internal static readonly @string fcntlˢ = "fcntl"u8;
internal static readonly @string testRunˢ3 = "-test.run=^TestRemoveAllNoFcntl$"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string envᶜ = "GO_TEST_REMOVE_ALL_NO_FCNTL"u8;

public static void TestRemoveAllNoFcntl(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    @string env = envᶜ;
    {
        @string dir = Getenv(env); if (dir != ""u8) {
            {
                var errΔ1 = RemoveAll(dir); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            return;
        }
    }
    // Only test on Linux so that we can assume we have strace.
    // The code is OS-independent so if it passes on Linux
    // it should pass on other Unix systems.
    if (Δruntime.GOOS != "linux"u8) {
        Ꮡt.Skipf("skipping test on %s"u8, Δruntime.GOOS);
    }
    {
        var (_, errΔ2) = Stat(binStraceˢ); if (errΔ2 != default!) {
            Ꮡt.Skipf("skipping test because /bin/strace not found: %v"u8, errΔ2);
        }
    }
    var (me, err) = Executable();
    if (err != default!) {
        Ꮡt.Skipf("skipping because Executable failed: %v"u8, err);
    }
    // Create 100 directories.
    // The test is that we can remove them without calling fcntl
    // on each one.
    @string tmpdir = Ꮡt.TempDir();
    @string subdir = filepath.Join(tmpdir, subdirˢ);
    {
        var errΔ3 = Mkdir(subdir, 493); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    for (nint i = 0; i < 100; i++) {
        @string subsubdir = filepath.Join(subdir, strconv.Itoa(i));
        {
            var errΔ4 = Mkdir(filepath.Join(subdir, strconv.Itoa(i)), 493); if (errΔ4 != default!) {
                Ꮡt.Fatal(errΔ4);
            }
        }
        {
            var errΔ5 = WriteFile(filepath.Join(subsubdir, fileˢ2), default!, 420); if (errΔ5 != default!) {
                Ꮡt.Fatal(errΔ5);
            }
        }
    }
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), binStraceˢ, "-f"u8, "-e", fcntlˢ, me, testRunˢ3);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, env + "=" + subdir);
    (var @out, err) = cmd.CombinedOutput();
    if (len(@out) > 0) {
        Ꮡt.Logf("%s"u8, @out);
    }
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        nint got = bytes.Count(@out, slice<byte>("fcntl"u8)); if (got >= 100) {
            Ꮡt.Errorf("found %d fcntl calls, want < 100"u8, got);
        }
    }
}

public static void BenchmarkRemoveAll(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string tmpDir = filepath.Join(Ꮡb.TempDir(), targetˢ);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        b.StopTimer();
        var err = CopyFS(tmpDir, DirFS("."u8));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        b.StartTimer();
        {
            var errΔ1 = RemoveAll(tmpDir); if (errΔ1 != default!) {
                Ꮡb.Fatal(errΔ1);
            }
        }
    }
}

} // end os_test_package
