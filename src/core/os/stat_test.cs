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
internal static void testStatAndLstat(ж<Δtesting.T> Ꮡt, @string path, testStatAndLstatParams @params) {
    GoFrame ᒐ = default;
    try {
        // test os.Stat
        var (sfi, err) = Δos.Stat(path);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        @params.statCheck(Ꮡt, path, sfi);
        // test os.Lstat
        (var lsfi, err) = Δos.Lstat(path);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        @params.lstatCheck(Ꮡt, path, lsfi);
        if (@params.isLink){
            if (Δos.SameFile(sfi, lsfi)) {
                Ꮡt.Errorf("stat and lstat of %q should not be the same"u8, path);
            }
        } else {
            if (!Δos.SameFile(sfi, lsfi)) {
                Ꮡt.Errorf("stat and lstat of %q should be the same"u8, path);
            }
        }
        // test os.File.Stat
        (var f, err) = Δos.Open(path);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var sfi2, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        @params.statCheck(Ꮡt, path, sfi2);
        if (!Δos.SameFile(sfi, sfi2)) {
            Ꮡt.Errorf("stat of open %q file and stat of %q should be the same"u8, path, path);
        }
        if (@params.isLink){
            if (Δos.SameFile(sfi2, lsfi)) {
                Ꮡt.Errorf("stat of opened %q file and lstat of %q should not be the same"u8, path, path);
            }
        } else {
            if (!Δos.SameFile(sfi2, lsfi)) {
                Ꮡt.Errorf("stat of opened %q file and lstat of %q should be the same"u8, path, path);
            }
        }
        var (parentdir, @base) = filepath.Split(path);
        if (parentdir == ""u8 || @base == ""u8) {
            // skip os.Readdir test of files without directory or file name component,
            // such as directories with slash at the end or Windows device names.
            return;
        }
        (var parent, err) = Δos.Open(parentdir);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        var parentʗ1 = parent;
        defer(() => parentʗ1.Close(), ref ᒐ);
        (var fis, err) = parent.Readdir(-1);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        fs.FileInfo lsfi2 = default!;
        foreach (var (_, fi2) in fis) {
            if (fi2.Name() == @base) {
                lsfi2 = fi2;
                break;
            }
        }
        if (lsfi2 == default!) {
            Ꮡt.Errorf("failed to find %q in its parent"u8, path);
            return;
        }
        @params.lstatCheck(Ꮡt, path, lsfi2);
        if (!Δos.SameFile(lsfi, lsfi2)) {
            Ꮡt.Errorf("lstat of %q file in %q directory and %q should be the same"u8, lsfi2.Name(), parentdir, path);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testIsDir verifies that fi refers to directory.
internal static void testIsDir(ж<Δtesting.T> Ꮡt, @string path, fs.FileInfo fi) {
    Ꮡt.Helper();
    if (!fi.IsDir()) {
        Ꮡt.Errorf("%q should be a directory"u8, path);
    }
    if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) != 0) {
        Ꮡt.Errorf("%q should not be a symlink"u8, path);
    }
}

// testIsSymlink verifies that fi refers to symlink.
internal static void testIsSymlink(ж<Δtesting.T> Ꮡt, @string path, fs.FileInfo fi) {
    Ꮡt.Helper();
    if (fi.IsDir()) {
        Ꮡt.Errorf("%q should not be a directory"u8, path);
    }
    if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) == 0) {
        Ꮡt.Errorf("%q should be a symlink"u8, path);
    }
}

// testIsFile verifies that fi refers to file.
internal static void testIsFile(ж<Δtesting.T> Ꮡt, @string path, fs.FileInfo fi) {
    Ꮡt.Helper();
    if (fi.IsDir()) {
        Ꮡt.Errorf("%q should not be a directory"u8, path);
    }
    if ((fs.FileMode)(fi.Mode() & fs.ModeSymlink) != 0) {
        Ꮡt.Errorf("%q should not be a symlink"u8, path);
    }
}

internal static void testDirStats(ж<Δtesting.T> Ꮡt, @string path) {
    var @params = new testStatAndLstatParams(
        isLink: false,
        statCheck: testIsDir,
        lstatCheck: testIsDir
    );
    testStatAndLstat(Ꮡt, path, @params);
}

internal static void testFileStats(ж<Δtesting.T> Ꮡt, @string path) {
    var @params = new testStatAndLstatParams(
        isLink: false,
        statCheck: testIsFile,
        lstatCheck: testIsFile
    );
    testStatAndLstat(Ꮡt, path, @params);
}

internal static void testSymlinkStats(ж<Δtesting.T> Ꮡt, @string path, bool isdir) {
    var @params = new testStatAndLstatParams(
        isLink: true,
        lstatCheck: testIsSymlink
    );
    if (isdir){
        @params.statCheck = testIsDir;
    } else {
        @params.statCheck = testIsFile;
    }
    testStatAndLstat(Ꮡt, path, @params);
}

internal static void testSymlinkSameFile(ж<Δtesting.T> Ꮡt, @string path, @string link) {
    var (pathfi, err) = Δos.Stat(path);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    (var linkfi, err) = Δos.Stat(link);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    if (!Δos.SameFile(pathfi, linkfi)) {
        Ꮡt.Errorf("os.Stat(%q) and os.Stat(%q) are not the same file"u8, path, link);
    }
    (linkfi, err) = Δos.Lstat(link);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    if (Δos.SameFile(pathfi, linkfi)) {
        Ꮡt.Errorf("os.Stat(%q) and os.Lstat(%q) are the same file"u8, path, link);
    }
}

internal static void testSymlinkSameFileOpen(ж<Δtesting.T> Ꮡt, @string link) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Δos.Open(link);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var fi, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        (var fi2, err) = Δos.Stat(link);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        if (!Δos.SameFile(fi, fi2)) {
            Ꮡt.Errorf("os.Open(%q).Stat() and os.Stat(%q) are not the same file"u8, link, link);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string linklinkˢ = "linklink"u8;

public static void TestDirAndSymlinkStats(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    @string dir = filepath.Join(tmpdir, dirˢ);
    {
        var err = Δos.Mkdir(dir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testDirStats(Ꮡt, dir);
    @string dirlink = filepath.Join(tmpdir, linkˢ);
    {
        var err = Δos.Symlink(dir, dirlink); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testSymlinkStats(Ꮡt, dirlink, true);
    testSymlinkSameFile(Ꮡt, dir, dirlink);
    testSymlinkSameFileOpen(Ꮡt, dirlink);
    @string linklink = filepath.Join(tmpdir, linklinkˢ);
    {
        var err = Δos.Symlink(dirlink, linklink); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testSymlinkStats(Ꮡt, linklink, true);
    testSymlinkSameFile(Ꮡt, dir, linklink);
    testSymlinkSameFileOpen(Ꮡt, linklink);
}

public static void TestFileAndSymlinkStats(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    @string @file = filepath.Join(tmpdir, fileˢ2);
    {
        var err = Δos.WriteFile(@file, slice<byte>(""u8), 420); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testFileStats(Ꮡt, @file);
    @string filelink = filepath.Join(tmpdir, linkˢ);
    {
        var err = Δos.Symlink(@file, filelink); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testSymlinkStats(Ꮡt, filelink, false);
    testSymlinkSameFile(Ꮡt, @file, filelink);
    testSymlinkSameFileOpen(Ꮡt, filelink);
    @string linklink = filepath.Join(tmpdir, linklinkˢ);
    {
        var err = Δos.Symlink(filelink, linklink); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    testSymlinkStats(Ꮡt, linklink, false);
    testSymlinkSameFile(Ꮡt, @file, linklink);
    testSymlinkSameFileOpen(Ꮡt, linklink);
}

// see issue 27225 for details
public static void TestSymlinkWithTrailingSlash(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    @string dir = filepath.Join(tmpdir, dirˢ);
    {
        var errΔ1 = Δos.Mkdir(dir, 511); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string dirlink = filepath.Join(tmpdir, linkˢ);
    {
        var errΔ2 = Δos.Symlink(dir, dirlink); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    @string dirlinkWithSlash = dirlink + ((@string)(rune)Δos.PathSeparator);
    testDirStats(Ꮡt, dirlinkWithSlash);
    var (fi1, err) = Δos.Stat(dir);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    (var fi2, err) = Δos.Stat(dirlinkWithSlash);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    if (!Δos.SameFile(fi1, fi2)) {
        Ꮡt.Errorf("os.Stat(%q) and os.Stat(%q) are not the same file"u8, dir, dirlinkWithSlash);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnNonWindowsˢ = (@string)"skipping on non-Windows"u8;

public static void TestStatConsole(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS != "windows"u8) {
        Ꮡt.Skip(skippingOnNonWindowsˢ);
    }
    Ꮡt.Parallel();
    var consoleNames = new @string[]{
        "CONIN$"u8,
        "CONOUT$"u8,
        "CON"u8
    }.slice();
    foreach (var (_, name) in consoleNames) {
        var @params = new testStatAndLstatParams(
            isLink: false,
            statCheck: testIsFile,
            lstatCheck: testIsFile
        );
        testStatAndLstat(Ꮡt, name, @params);
        testStatAndLstat(Ꮡt, @"\\.\"u8 + name, @params);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataHelloˢ = "testdata/hello"u8;
internal static readonly object statSucceededOnClosedˢ = (@string)"Stat succeeded on closed File"u8;

public static void TestClosedStat(ж<Δtesting.T> Ꮡt) {
    // Historically we do not seem to match ErrClosed on non-Unix systems.
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("skipping on %s"u8, Δruntime.GOOS);
    }

    Ꮡt.Parallel();
    var (f, err) = Δos.Open(testdataHelloˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = f.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    (_, err) = f.Stat();
    if (err == default!){
        Ꮡt.Error(statSucceededOnClosedˢ);
    } else 
    if (!errors.Is(err, Δos.ErrClosed)) {
        Ꮡt.Errorf("error from Stat on closed file did not match ErrClosed: %q, type %T"u8, err, err);
    }
}

} // end os_test_package
