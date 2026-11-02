// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using testenv = @internal.testenv_package;
using fs = go.io.fs_package;
using Δos = os_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using Δtesting = testing_package;
using @internal;
using go.io;
using go.path;
using static go.os_internal_test_package;

partial class os_test_package {

partial struct testStatAndLstatParams {
    internal bool isLink;
    internal Action<ж<Δtesting.T>, @string, fs.FileInfo> statCheck;
    internal Action<ж<Δtesting.T>, @string, fs.FileInfo> lstatCheck;
}

// testStatAndLstat verifies that all os.Stat, os.Lstat os.File.Stat and os.Readdir work.
internal static void testStatAndLstat(ж<Δtesting.T> t, @string path, testStatAndLstatParams @params) {
    GoFrame ᒐ = default;
    try {
        // test os.Stat
        var (sfi, err) = Δos.Stat(path);
        if (err != nil) {
            t.Error(err);
            return;
        }
        @params.statCheck(t, path, sfi);

        // test os.Lstat
        (var lsfi, err) = Δos.Lstat(path);
        if (err != nil) {
            t.Error(err);
            return;
        }
        @params.lstatCheck(t, path, lsfi);

        if (@params.isLink) {
            if (Δos.SameFile(sfi, lsfi)) {
                t.Errorf("stat and lstat of %q should not be the same", path);
            }
        } else {
            if (!Δos.SameFile(sfi, lsfi)) {
                t.Errorf("stat and lstat of %q should be the same", path);
            }
        }

        // test os.File.Stat
        (var f, err) = Δos.Open(path);
        if (err != nil) {
            t.Error(err);
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var sfi2, err) = f.Stat();
        if (err != nil) {
            t.Error(err);
            return;
        }
        @params.statCheck(t, path, sfi2);

        if (!Δos.SameFile(sfi, sfi2)) {
            t.Errorf("stat of open %q file and stat of %q should be the same", path, path);
        }

        if (@params.isLink) {
            if (Δos.SameFile(sfi2, lsfi)) {
                t.Errorf("stat of opened %q file and lstat of %q should not be the same", path, path);
            }
        } else {
            if (!Δos.SameFile(sfi2, lsfi)) {
                t.Errorf("stat of opened %q file and lstat of %q should be the same", path, path);
            }
        }

        var (parentdir, @base) = filepath.Split(path);
        if (parentdir == ""u8 || @base == ""u8) {
            // skip os.Readdir test of files without directory or file name component,
            // such as directories with slash at the end or Windows device names.
            return;
        }

        (var parent, err) = Δos.Open(parentdir);
        if (err != nil) {
            t.Error(err);
            return;
        }
        var parentʗ1 = parent;
        defer(() => parentʗ1.Close(), ref ᒐ);

        (var fis, err) = parent.Readdir(-1);
        if (err != nil) {
            t.Error(err);
            return;
        }
        fs.FileInfo lsfi2 = default;
        foreach (var (_, fi2) in fis) {
            if (fi2.Name() == @base) {
                lsfi2 = fi2;
                break;
            }
        }
        if (lsfi2 == nil) {
            t.Errorf("failed to find %q in its parent", path);
            return;
        }
        @params.lstatCheck(t, path, lsfi2);

        if (!Δos.SameFile(lsfi, lsfi2)) {
            t.Errorf("lstat of %q file in %q directory and %q should be the same", lsfi2.Name(), parentdir, path);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testIsDir verifies that fi refers to directory.
internal static void testIsDir(ж<Δtesting.T> t, @string path, fs.FileInfo fi) {
    t.Helper();
    if (!fi.IsDir()) {
        t.Errorf("%q should be a directory", path);
    }
    if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) != 0) {
        t.Errorf("%q should not be a symlink", path);
    }
}

// testIsSymlink verifies that fi refers to symlink.
internal static void testIsSymlink(ж<Δtesting.T> t, @string path, fs.FileInfo fi) {
    t.Helper();
    if (fi.IsDir()) {
        t.Errorf("%q should not be a directory", path);
    }
    if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) == 0) {
        t.Errorf("%q should be a symlink", path);
    }
}

// testIsFile verifies that fi refers to file.
internal static void testIsFile(ж<Δtesting.T> t, @string path, fs.FileInfo fi) {
    t.Helper();
    if (fi.IsDir()) {
        t.Errorf("%q should not be a directory", path);
    }
    if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) != 0) {
        t.Errorf("%q should not be a symlink", path);
    }
}

internal static void testDirStats(ж<Δtesting.T> t, @string path) {
    var @params = new testStatAndLstatParams(
        isLink: false,
        statCheck: testIsDir,
        lstatCheck: testIsDir
    );
    testStatAndLstat(t, path, @params);
}

internal static void testFileStats(ж<Δtesting.T> t, @string path) {
    var @params = new testStatAndLstatParams(
        isLink: false,
        statCheck: testIsFile,
        lstatCheck: testIsFile
    );
    testStatAndLstat(t, path, @params);
}

internal static void testSymlinkStats(ж<Δtesting.T> t, @string path, bool isdir) {
    var @params = new testStatAndLstatParams(
        isLink: true,
        lstatCheck: testIsSymlink
    );
    if (isdir) {
        @params.statCheck = testIsDir;
    } else {
        @params.statCheck = testIsFile;
    }
    testStatAndLstat(t, path, @params);
}

internal static void testSymlinkSameFile(ж<Δtesting.T> t, @string path, @string link) {
    var (pathfi, err) = Δos.Stat(path);
    if (err != nil) {
        t.Error(err);
        return;
    }

    (var linkfi, err) = Δos.Stat(link);
    if (err != nil) {
        t.Error(err);
        return;
    }
    if (!Δos.SameFile(pathfi, linkfi)) {
        t.Errorf("os.Stat(%q) and os.Stat(%q) are not the same file", path, link);
    }

    (linkfi, err) = Δos.Lstat(link);
    if (err != nil) {
        t.Error(err);
        return;
    }
    if (Δos.SameFile(pathfi, linkfi)) {
        t.Errorf("os.Stat(%q) and os.Lstat(%q) are the same file", path, link);
    }
}

internal static void testSymlinkSameFileOpen(ж<Δtesting.T> t, @string link) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Δos.Open(link);
        if (err != nil) {
            t.Error(err);
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var fi, err) = f.Stat();
        if (err != nil) {
            t.Error(err);
            return;
        }

        (var fi2, err) = Δos.Stat(link);
        if (err != nil) {
            t.Error(err);
            return;
        }

        if (!Δos.SameFile(fi, fi2)) {
            t.Errorf("os.Open(%q).Stat() and os.Stat(%q) are not the same file", link, link);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linklinkˢ => ref linklinkˢᵇ;
private static readonly @string linklinkˢᵇ = "linklink"u8;

public static void TestDirAndSymlinkStats(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    t.Parallel();

    @string tmpdir = t.TempDir();
    @string dir = filepath.Join(tmpdir, dirˢ);
    {
        var err = Δos.Mkdir(dir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    testDirStats(t, dir);

    @string dirlink = filepath.Join(tmpdir, linkˢ);
    {
        var err = Δos.Symlink(dir, dirlink); if (err != nil) {
            t.Fatal(err);
        }
    }
    testSymlinkStats(t, dirlink, true);
    testSymlinkSameFile(t, dir, dirlink);
    testSymlinkSameFileOpen(t, dirlink);

    @string linklink = filepath.Join(tmpdir, linklinkˢ);
    {
        var err = Δos.Symlink(dirlink, linklink); if (err != nil) {
            t.Fatal(err);
        }
    }
    testSymlinkStats(t, linklink, true);
    testSymlinkSameFile(t, dir, linklink);
    testSymlinkSameFileOpen(t, linklink);
}

public static void TestFileAndSymlinkStats(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    t.Parallel();

    @string tmpdir = t.TempDir();
    @string @file = filepath.Join(tmpdir, fileˢ2);
    {
        var err = Δos.WriteFile(@file, slice<byte>(""u8), 420); if (err != nil) {
            t.Fatal(err);
        }
    }
    testFileStats(t, @file);

    @string filelink = filepath.Join(tmpdir, linkˢ);
    {
        var err = Δos.Symlink(@file, filelink); if (err != nil) {
            t.Fatal(err);
        }
    }
    testSymlinkStats(t, filelink, false);
    testSymlinkSameFile(t, @file, filelink);
    testSymlinkSameFileOpen(t, filelink);

    @string linklink = filepath.Join(tmpdir, linklinkˢ);
    {
        var err = Δos.Symlink(filelink, linklink); if (err != nil) {
            t.Fatal(err);
        }
    }
    testSymlinkStats(t, linklink, false);
    testSymlinkSameFile(t, @file, linklink);
    testSymlinkSameFileOpen(t, linklink);
}

// see issue 27225 for details
public static void TestSymlinkWithTrailingSlash(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    t.Parallel();

    @string tmpdir = t.TempDir();
    @string dir = filepath.Join(tmpdir, dirˢ);
    {
        var errΔ1 = Δos.Mkdir(dir, 511); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    @string dirlink = filepath.Join(tmpdir, linkˢ);
    {
        var errΔ2 = Δos.Symlink(dir, dirlink); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
    @string dirlinkWithSlash = dirlink + ((@string)(rune)Δos.PathSeparator);

    testDirStats(t, dirlinkWithSlash);

    var (fi1, err) = Δos.Stat(dir);
    if (err != nil) {
        t.Error(err);
        return;
    }
    (var fi2, err) = Δos.Stat(dirlinkWithSlash);
    if (err != nil) {
        t.Error(err);
        return;
    }
    if (!Δos.SameFile(fi1, fi2)) {
        t.Errorf("os.Stat(%q) and os.Stat(%q) are not the same file", dir, dirlinkWithSlash);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnNonWindowsˢ = (@string)"skipping on non-Windows"u8;

public static void TestStatConsole(ж<Δtesting.T> t) {
    if (Δruntime.GOOS != "windows"u8) {
        t.Skip(skippingOnNonWindowsˢ);
    }
    t.Parallel();
    var consoleNames = slice<@string>([
        "CONIN$",
        "CONOUT$",
        "CON"
    ]);
    foreach (var (_, name) in consoleNames) {
        var @params = new testStatAndLstatParams(
            isLink: false,
            statCheck: testIsFile,
            lstatCheck: testIsFile
        );
        testStatAndLstat(t, name, @params);
        testStatAndLstat(t, @"\\.\"u8 + name, @params);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataHelloˢ => ref testdataHelloˢᵇ;
private static readonly @string testdataHelloˢᵇ = "testdata/hello"u8;
internal static readonly object statSucceededOnClosedˢ = (@string)"Stat succeeded on closed File"u8;

public static void TestClosedStat(ж<Δtesting.T> t) {
    // Historically we do not seem to match ErrClosed on non-Unix systems.
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8: {
        t.Skipf("skipping on %s", Δruntime.GOOS);
        break;
    }}

    t.Parallel();
    var (f, err) = Δos.Open(testdataHelloˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var errΔ1 = f.Close(); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    (_, err) = f.Stat();
    if (err == nil) {
        t.Error(statSucceededOnClosedˢ);
    } else if (!errors.Is(err, Δos.ErrClosed)) {
        t.Errorf("error from Stat on closed file did not match ErrClosed: %q, type %T", err, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string notfoundˢ => ref notfoundˢᵇ;
private static readonly @string notfoundˢᵇ = "notfound"u8;
internal static ref readonly @string notfounddirˢ => ref notfounddirˢᵇ;
private static readonly @string notfounddirˢᵇ = "notfounddir"u8;

public static void TestStatNotExist(ж<Δtesting.T> t) {
    t.Parallel();
    @string name = filepath.Join(t.TempDir(), notfoundˢ);
    var (_, err) = Δos.Stat(name);
    if (!errors.Is(err, fs.ErrNotExist)) {
        t.Errorf("os.Stat(%q) = %v; want fs.ErrNotExist", name, err);
    }

    name = filepath.Join(t.TempDir(), notfounddirˢ, notfoundˢ);
    (_, err) = Δos.Stat(name);
    if (!errors.Is(err, fs.ErrNotExist)) {
        t.Errorf("os.Stat(%q) = %v; want fs.ErrNotExist", name, err);
    }
}

} // end os_test_package
