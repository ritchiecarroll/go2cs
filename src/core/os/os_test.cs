// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using fs = go.io.fs_package;
using Δlog = log_package;
using static os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using debug = go.runtime.debug_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using fstest = go.testing.fstest_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.io;
using go.os;
using go.path;
using go.runtime;
using go.testing;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goOsTestDrainStdinˢ = "GO_OS_TEST_DRAIN_STDIN"u8;

public static void TestMain(ж<Δtesting.M> Ꮡm) {
    if (Getenv(goOsTestDrainStdinˢ) == "1"u8) {
        Stdout.Close();
        Δio.Copy(Δio.Discard, new os_test_package.os_FileжReader(Stdin));
        Exit(0);
    }
    Δlog.SetFlags((nint)((nint)Δlog.LstdFlags | (nint)Δlog.Lshortfile));
    Exit(Ꮡm.Run());
}

internal static slice<@string> dot = new @string[]{
    "dir_unix.go"u8,
    "env.go"u8,
    "error.go"u8,
    "file.go"u8,
    "os_test.go"u8,
    "types.go"u8,
    "stat_darwin.go"u8,
    "stat_linux.go"u8
}.slice();

[GoType] partial struct sysDir {
    internal @string name;
    internal slice<@string> files;
}

// In a self-hosted iOS build the above files might
// not exist. Look for system files instead below.
// wasmtime has issues resolving symbolic links that are often present
// in directories like /etc/group below (e.g. private/etc/group on OSX).
// For this reason we use files in the Go source tree instead.
internal static ж<sysDir> sysdir = ((Func<ж<sysDir>>)(() => {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8) {
        return Ꮡ(new sysDir(
            "/system/lib"u8,
            new @string[]{
                "libmedia.so"u8,
                "libpowermanager.so"u8
            }.slice()
        ));
    }
    if (exprᴛ1 == "ios"u8) {
        var (wd, err) = syscall.Getwd();
        if (err != default!) {
            wd = err.Error();
        }
        var sd = Ꮡ(new sysDir(
            filepath.Join(wd, "..", ".."),
            new @string[]{
                "ResourceRules.plist"u8,
                "Info.plist"u8
            }.slice()
        ));
        var found = true;
        foreach (var (_, f) in (~sd).files) {
            @string path = filepath.Join((~sd).name, f);
            {
                var (_, errΔ1) = Stat(path); if (errΔ1 != default!) {
                    found = false;
                    break;
                }
            }
        }
        if (found) {
            return sd;
        }
    }
    else if (exprᴛ1 == "windows"u8) {
        return Ꮡ(new sysDir(
            Getenv("SystemRoot"u8) + "\\system32\\drivers\\etc"u8,
            new @string[]{
                "networks"u8,
                "protocol"u8,
                "services"u8
            }.slice()
        ));
    }
    else if (exprᴛ1 == "plan9"u8) {
        return Ꮡ(new sysDir(
            "/lib/ndb"u8,
            new @string[]{
                "common"u8,
                "local"u8
            }.slice()
        ));
    }
    else if (exprᴛ1 == "wasip1"u8) {
        return Ꮡ(new sysDir(
            Δruntime.GOROOT(),
            new @string[]{
                "go.env"u8,
                "LICENSE"u8,
                "CONTRIBUTING.md"u8
            }.slice()
        ));
    }

    return Ꮡ(new sysDir(
        "/etc"u8,
        new @string[]{
            "group"u8,
            "hosts"u8,
            "passwd"u8
        }.slice()
    ));
}))();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object openFailedˢ = (@string)"open failed:"u8;

internal static int64 size(@string name, ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (@file, err) = Open(name);
        if (err != default!) {
            Ꮡt.Fatal(openFailedˢ, err);
        }
        var fileʗ1 = @file;
        defer(() => {
            {
                var errΔ1 = fileʗ1.Close(); if (errΔ1 != default!) {
                    Ꮡt.Error(errΔ1);
                }
            }
        }, ref ᒐ);
        (var n, err) = Δio.Copy(Δio.Discard, new os_test_package.os_FileжReader(@file));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        return n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static bool /*r*/ equal(@string name1, @string name2) {
    bool r = default!;

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        r = strings.EqualFold(name1, name2);
    }
    else { /* default: */
        r = name1 == name2;
    }

    return r;
}

internal static ж<Δos.File> /*f*/ newFile(ж<Δtesting.T> Ꮡt) {
    ж<Δos.File> f = default!;

    Ꮡt.Helper();
    (f, var err) = CreateTemp(""u8, "_Go_"u8 + Ꮡt.Name());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var fʗ1 = f;
    Ꮡt.Cleanup(() => {
        {
            var errΔ1 = fʗ1.Close(); if (errΔ1 != default! && !errors.Is(errΔ1, ErrClosed)) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        {
            var errΔ2 = Remove(fʗ1.Name()); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    });
    return f;
}

internal static @string sfdir = (~sysdir).name;

internal static @string sfname = (~sysdir).files[0];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object statFailedˢ = (@string)"stat failed:"u8;
internal static readonly object nameShouldBeˢ = (@string)"name should be "u8;
internal static readonly object sizeShouldBeˢ = (@string)"size should be"u8;

public static void TestStat(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string path = sfdir + "/"u8 + sfname;
    var (dir, err) = Stat(path);
    if (err != default!) {
        Ꮡt.Fatal(statFailedˢ, err);
    }
    if (!equal(sfname, dir.Name())) {
        Ꮡt.Error(nameShouldBeˢ, sfname, (@string)"; is"u8, dir.Name());
    }
    var filesize = size(path, Ꮡt);
    if (dir.Size() != filesize) {
        Ꮡt.Error(sizeShouldBeˢ, filesize, (@string)"; is"u8, dir.Size());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noSuchFileˢ = "no-such-file"u8;
internal static readonly object gotNilWantErrorˢ = (@string)"got nil, want error"u8;
internal static readonly @string symlinkˢ = "symlink"u8;

public static void TestStatError(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string path = noSuchFileˢ;
    var (fi, err) = Stat(path);
    if (err == default!) {
        Ꮡt.Fatal(gotNilWantErrorˢ);
    }
    if (fi != default!) {
        Ꮡt.Errorf("got %v, want nil"u8, fi);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("got %T, want %T"u8, err, perr.OrTypedNil());
        }
    }
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    @string link = symlinkˢ;
    err = Symlink(path, link);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (fi, err) = Stat(link);
    if (err == default!) {
        Ꮡt.Fatal(gotNilWantErrorˢ);
    }
    if (fi != default!) {
        Ꮡt.Errorf("got %v, want nil"u8, fi);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("got %T, want %T"u8, err, perr.OrTypedNil());
        }
    }
}

public static void TestStatSymlinkLoop(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Chdir(Ꮡt.TempDir());
        var err = Symlink("x"u8, "y"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(Remove, (@string)"y", ref ᒐ);
        err = Symlink("y"u8, "x"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(Remove, (@string)"x", ref ᒐ);
        (_, err) = Stat("x"u8);
        {
            var (_, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
                Ꮡt.Errorf("expected *PathError, got %T: %v\n"u8, err, err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fstatFailedˢ = (@string)"fstat failed:"u8;

public static void TestFstat(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        @string path = sfdir + "/"u8 + sfname;
        var (@file, err1) = Open(path);
        if (err1 != default!) {
            Ꮡt.Fatal(openFailedˢ, err1);
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);
        var (dir, err2) = @file.Stat();
        if (err2 != default!) {
            Ꮡt.Fatal(fstatFailedˢ, err2);
        }
        if (!equal(sfname, dir.Name())) {
            Ꮡt.Error(nameShouldBeˢ, sfname, (@string)"; is"u8, dir.Name());
        }
        var filesize = size(path, Ꮡt);
        if (dir.Size() != filesize) {
            Ꮡt.Error(sizeShouldBeˢ, filesize, (@string)"; is"u8, dir.Size());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lstatFailedˢ = (@string)"lstat failed:"u8;

public static void TestLstat(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string path = sfdir + "/"u8 + sfname;
    var (dir, err) = Lstat(path);
    if (err != default!) {
        Ꮡt.Fatal(lstatFailedˢ, err);
    }
    if (!equal(sfname, dir.Name())) {
        Ꮡt.Error(nameShouldBeˢ, sfname, (@string)"; is"u8, dir.Name());
    }
    if ((fs.FileMode)(dir.Mode() & ModeSymlink) == 0) {
        var filesize = size(path, Ꮡt);
        if (dir.Size() != filesize) {
            Ꮡt.Error(sizeShouldBeˢ, filesize, (@string)"; is"u8, dir.Size());
        }
    }
}

// Read with length 0 should not return EOF.
public static void TestRead0(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        @string path = sfdir + "/"u8 + sfname;
        var (f, err) = Open(path);
        if (err != default!) {
            Ꮡt.Fatal(openFailedˢ, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var b = new slice<byte>(0);
        (var n, err) = f.Read(b);
        if (n != 0 || err != default!) {
            Ꮡt.Errorf("Read(0) = %d, %v, want 0, nil"u8, n, err);
        }
        b = new slice<byte>(100);
        (n, err) = f.Read(b);
        if (n <= 0 || err != default!) {
            Ꮡt.Errorf("Read(100) = %d, %v, want >0, nil"u8, n, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Reading a closed file should return ErrClosed error
public static void TestReadClosed(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string path = sfdir + "/"u8 + sfname;
    var (@file, err) = Open(path);
    if (err != default!) {
        Ꮡt.Fatal(openFailedˢ, err);
    }
    @file.Close(); // close immediately
    var b = new slice<byte>(100);
    (_, err) = @file.Read(b);
    var (e, ok) = err._<ж<fs.PathError>>(ᐧ);
    if (!ok || !AreEqual((~e).Err, ErrClosed)) {
        Ꮡt.Fatalf("Read: got %T(%v), want %T(%v)"u8, err, err, e.OrTypedNil(), ErrClosed);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object presentTwiceˢ = (@string)"present twice:"u8;
internal static readonly object couldNotFindˢ = (@string)"could not find"u8;
internal static readonly object readdirnamesReturnedNilˢ = (@string)"Readdirnames returned nil instead of empty slice"u8;

internal static Action<ж<Δtesting.T>> testReaddirnames(@string dir, slice<@string> contents) {
    var contentsʗ1 = contents;
    return (ж<Δtesting.T> t) => {
        GoFrame ᒐ = default;
        try {
            t.Parallel();
            var (@file, err) = Open(dir);
            if (err != default!) {
                t.Fatalf("open %q failed: %v"u8, dir, err);
            }
            var fileʗ1 = @file;
            defer(() => fileʗ1.Close(), ref ᒐ);
            var (s, err2) = @file.Readdirnames(-1);
            if (err2 != default!) {
                t.Fatalf("Readdirnames %q failed: %v"u8, dir, err2);
            }
            foreach (var (_, m) in contentsʗ1) {
                var found = false;
                foreach (var (_, n) in s) {
                    if (n == "."u8 || n == ".."u8) {
                        t.Errorf("got %q in directory"u8, n);
                    }
                    if (!equal(m, n)) {
                        continue;
                    }
                    if (found) {
                        t.Error(presentTwiceˢ, m);
                    }
                    found = true;
                }
                if (!found) {
                    t.Error(couldNotFindˢ, m);
                }
            }
            if (s == default!) {
                t.Error(readdirnamesReturnedNilˢ);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readdirReturnedNilˢ = (@string)"Readdir returned nil instead of empty slice"u8;

internal static Action<ж<Δtesting.T>> testReaddir(@string dir, slice<@string> contents) {
    var contentsʗ1 = contents;
    return (ж<Δtesting.T> t) => {
        GoFrame ᒐ = default;
        try {
            t.Parallel();
            var (@file, err) = Open(dir);
            if (err != default!) {
                t.Fatalf("open %q failed: %v"u8, dir, err);
            }
            var fileʗ1 = @file;
            defer(() => fileʗ1.Close(), ref ᒐ);
            var (s, err2) = @file.Readdir(-1);
            if (err2 != default!) {
                t.Fatalf("Readdir %q failed: %v"u8, dir, err2);
            }
            foreach (var (_, m) in contentsʗ1) {
                var found = false;
                foreach (var (_, n) in s) {
                    if (n.Name() == "."u8 || n.Name() == ".."u8) {
                        t.Errorf("got %q in directory"u8, n.Name());
                    }
                    if (!equal(m, n.Name())) {
                        continue;
                    }
                    if (found) {
                        t.Error(presentTwiceˢ, m);
                    }
                    found = true;
                }
                if (!found) {
                    t.Error(couldNotFindˢ, m);
                }
            }
            if (s == default!) {
                t.Error(readdirReturnedNilˢ);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readDirReturnedNilˢ = (@string)"ReadDir returned nil instead of empty slice"u8;

internal static Action<ж<Δtesting.T>> testReadDir(@string dir, slice<@string> contents) {
    var contentsʗ1 = contents;
    return (ж<Δtesting.T> t) => {
        GoFrame ᒐ = default;
        try {
            t.Parallel();
            var (@file, err) = Open(dir);
            if (err != default!) {
                t.Fatalf("open %q failed: %v"u8, dir, err);
            }
            var fileʗ1 = @file;
            defer(() => fileʗ1.Close(), ref ᒐ);
            var (s, err2) = @file.ReadDir(-1);
            if (err2 != default!) {
                t.Fatalf("ReadDir %q failed: %v"u8, dir, err2);
            }
            foreach (var (_, m) in contentsʗ1) {
                var found = false;
                foreach (var (_, n) in s) {
                    if (n.Name() == "."u8 || n.Name() == ".."u8) {
                        t.Errorf("got %q in directory"u8, n);
                    }
                    if (!equal(m, n.Name())) {
                        continue;
                    }
                    if (found) {
                        t.Error(presentTwiceˢ, m);
                    }
                    found = true;
                    var (lstat, errΔ1) = Lstat(dir + "/"u8 + m);
                    if (errΔ1 != default!) {
                        t.Fatal(errΔ1);
                    }
                    if (n.IsDir() != lstat.IsDir()) {
                        t.Errorf("%s: IsDir=%v, want %v"u8, m, n.IsDir(), lstat.IsDir());
                    }
                    if (n.Type() != lstat.Mode().Type()) {
                        t.Errorf("%s: IsDir=%v, want %v"u8, m, n.Type(), lstat.Mode().Type());
                    }
                    (var info, errΔ1) = n.Info();
                    if (errΔ1 != default!) {
                        t.Errorf("%s: Info: %v"u8, m, errΔ1);
                        continue;
                    }
                    if (!SameFile(info, lstat)) {
                        t.Errorf("%s: Info: SameFile(info, lstat) = false"u8, m);
                    }
                }
                if (!found) {
                    t.Error(couldNotFindˢ, m);
                }
            }
            if (s == default!) {
                t.Error(readDirReturnedNilˢ);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sysdirˢ = "sysdir"u8;
internal static readonly @string tempDirˢ = "TempDir"u8;

public static void TestFileReaddirnames(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    Ꮡt.Run("."u8, testReaddirnames("."u8, dot));
    Ꮡt.Run(sysdirˢ, testReaddirnames((~sysdir).name, (~sysdir).files));
    Ꮡt.Run(tempDirˢ, testReaddirnames(Ꮡt.TempDir(), default!));
}

public static void TestFileReaddir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    Ꮡt.Run("."u8, testReaddir("."u8, dot));
    Ꮡt.Run(sysdirˢ, testReaddir((~sysdir).name, (~sysdir).files));
    Ꮡt.Run(tempDirˢ, testReaddir(Ꮡt.TempDir(), default!));
}

public static void TestFileReadDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    Ꮡt.Run("."u8, testReadDir("."u8, dot));
    Ꮡt.Run(sysdirˢ, testReadDir((~sysdir).name, (~sysdir).files));
    Ꮡt.Run(tempDirˢ, testReadDir(Ꮡt.TempDir(), default!));
}

internal static void benchmarkReaddirname(@string path, ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint nentries = default!;
    for (nint i = 0; i < b.N; i++) {
        var (f, err) = Open(path);
        if (err != default!) {
            Ꮡb.Fatalf("open %q failed: %v"u8, path, err);
        }
        (var ns, err) = f.Readdirnames(-1);
        f.Close();
        if (err != default!) {
            Ꮡb.Fatalf("readdirnames %q failed: %v"u8, path, err);
        }
        nentries = len(ns);
    }
    Ꮡb.Logf("benchmarkReaddirname %q: %d entries"u8, path, nentries);
}

internal static void benchmarkReaddir(@string path, ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint nentries = default!;
    for (nint i = 0; i < b.N; i++) {
        var (f, err) = Open(path);
        if (err != default!) {
            Ꮡb.Fatalf("open %q failed: %v"u8, path, err);
        }
        (var fs, err) = f.Readdir(-1);
        f.Close();
        if (err != default!) {
            Ꮡb.Fatalf("readdir %q failed: %v"u8, path, err);
        }
        nentries = len(fs);
    }
    Ꮡb.Logf("benchmarkReaddir %q: %d entries"u8, path, nentries);
}

internal static void benchmarkReadDir(@string path, ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint nentries = default!;
    for (nint i = 0; i < b.N; i++) {
        var (f, err) = Open(path);
        if (err != default!) {
            Ꮡb.Fatalf("open %q failed: %v"u8, path, err);
        }
        (var fs, err) = f.ReadDir(-1);
        f.Close();
        if (err != default!) {
            Ꮡb.Fatalf("readdir %q failed: %v"u8, path, err);
        }
        nentries = len(fs);
    }
    Ꮡb.Logf("benchmarkReadDir %q: %d entries"u8, path, nentries);
}

public static void BenchmarkReaddirname(ж<Δtesting.B> Ꮡb) {
    benchmarkReaddirname("."u8, Ꮡb);
}

public static void BenchmarkReaddir(ж<Δtesting.B> Ꮡb) {
    benchmarkReaddir("."u8, Ꮡb);
}

public static void BenchmarkReadDir(ж<Δtesting.B> Ꮡb) {
    benchmarkReadDir("."u8, Ꮡb);
}

internal static void benchmarkStat(ж<Δtesting.B> Ꮡb, @string path) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = Stat(path);
        if (err != default!) {
            Ꮡb.Fatalf("Stat(%q) failed: %v"u8, path, err);
        }
    }
}

internal static void benchmarkLstat(ж<Δtesting.B> Ꮡb, @string path) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = Lstat(path);
        if (err != default!) {
            Ꮡb.Fatalf("Lstat(%q) failed: %v"u8, path, err);
        }
    }
}

public static void BenchmarkStatDot(ж<Δtesting.B> Ꮡb) {
    benchmarkStat(Ꮡb, "."u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcOsOsTestGoˢ = "src/os/os_test.go"u8;

public static void BenchmarkStatFile(ж<Δtesting.B> Ꮡb) {
    benchmarkStat(Ꮡb, filepath.Join(Δruntime.GOROOT(), srcOsOsTestGoˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcOsˢ = "src/os"u8;

public static void BenchmarkStatDir(ж<Δtesting.B> Ꮡb) {
    benchmarkStat(Ꮡb, filepath.Join(Δruntime.GOROOT(), srcOsˢ));
}

public static void BenchmarkLstatDot(ж<Δtesting.B> Ꮡb) {
    benchmarkLstat(Ꮡb, "."u8);
}

public static void BenchmarkLstatFile(ж<Δtesting.B> Ꮡb) {
    benchmarkLstat(Ꮡb, filepath.Join(Δruntime.GOROOT(), srcOsOsTestGoˢ));
}

public static void BenchmarkLstatDir(ж<Δtesting.B> Ꮡb) {
    benchmarkLstat(Ꮡb, filepath.Join(Δruntime.GOROOT(), srcOsˢ));
}

// Read the directory one entry at a time.
internal static slice<@string> smallReaddirnames(ж<Δos.File> Ꮡfile, nint length, ж<Δtesting.T> Ꮡt) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    var names = new slice<@string>(length);
    nint count = 0;
    while (ᐧ) {
        var (d, err) = Ꮡfile.Readdirnames(1);
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
        if (err != default!) {
            Ꮡt.Fatalf("readdirnames %q failed: %v"u8, @file.Name(), err);
        }
        if (len(d) == 0) {
            Ꮡt.Fatalf("readdirnames %q returned empty slice and no error"u8, @file.Name());
        }
        names[count] = d[0];
        count++;
    }
    return names.slice(0, count);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usrBinˢ = "/usr/bin"u8;
internal static readonly @string systemBinˢ = "/system/bin"u8;
internal static readonly @string binˢ = "/bin"u8;
internal static readonly @string systemRootˢ = "SystemRoot"u8;

// Check that reading a directory one entry at a time gives the same result
// as reading it all at once.
public static void TestReaddirnamesOneAtATime(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.Parallel();
        // big directory that doesn't change often.
        @string dir = usrBinˢ;
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "android"u8) {
            dir = systemBinˢ;
        }
        else if (exprᴛ1 == "ios"u8 || exprᴛ1 == "wasip1"u8) {
            var (wd, errΔ2) = Getwd();
            if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
            dir = wd;
        }
        else if (exprᴛ1 == "plan9"u8) {
            dir = binˢ;
        }
        else if (exprᴛ1 == "windows"u8) {
            dir = Getenv(systemRootˢ) + "\\system32"u8;
        }

        var (@file, err) = Open(dir);
        if (err != default!) {
            Ꮡt.Fatalf("open %q failed: %v"u8, dir, err);
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);
        var (all, err1) = @file.Readdirnames(-1);
        if (err1 != default!) {
            Ꮡt.Fatalf("readdirnames %q failed: %v"u8, dir, err1);
        }
        var (file1, err2) = Open(dir);
        if (err2 != default!) {
            Ꮡt.Fatalf("open %q failed: %v"u8, dir, err2);
        }
        var file1ʗ1 = file1;
        defer(() => file1ʗ1.Close(), ref ᒐ);
        var small = smallReaddirnames(file1, len(all) + 100, Ꮡt); // +100 in case we screw up
        if (len(small) < len(all)) {
            Ꮡt.Fatalf("len(small) is %d, less than %d"u8, len(small), len(all));
        }
        foreach (var (i, n) in all) {
            if (small[i] != n) {
                Ꮡt.Errorf("small read %q mismatch: %v"u8, small[i], n);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testShortSkippingˢ = (@string)"test.short; skipping"u8;

public static void TestReaddirNValues(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(testShortSkippingˢ);
    }
    Ꮡt.Parallel();
    @string dir = Ꮡt.TempDir();
    for (nint i = 1; i <= 105; i++) {
        var (f, err) = Create(filepath.Join(dir, fmt.Sprintf("%d"u8, i)));
        if (err != default!) {
            Ꮡt.Fatalf("Create: %v"u8, err);
        }
        f.Write(slice<byte>(strings.Repeat("X"u8, i)));
        f.Close();
    }
    ref var d = ref heap<ж<Δos.File>>(out var Ꮡd);
    void openDir() {
        error err = default!;
        (Ꮡd.ValueSlot, err) = Open(dir);
        if (err != default!) {
            Ꮡt.Fatalf("Open directory: %v"u8, err);
        }
    }
    var readdirExpect = (nint n, nint want, error wantErr) => {
        Ꮡt.Helper();
        var (fi, err) = Ꮡd.ValueSlot.Readdir(n);
        if (!AreEqual(err, wantErr)) {
            Ꮡt.Fatalf("Readdir of %d got error %v, want %v"u8, n, err, wantErr);
        }
        {
            nint g = len(fi);
            nint e = want; if (g != e) {
                Ꮡt.Errorf("Readdir of %d got %d files, want %d"u8, n, g, e);
            }
        }
    };
    var readDirExpect = (nint n, nint want, error wantErr) => {
        Ꮡt.Helper();
        var (de, err) = Ꮡd.ValueSlot.ReadDir(n);
        if (!AreEqual(err, wantErr)) {
            Ꮡt.Fatalf("ReadDir of %d got error %v, want %v"u8, n, err, wantErr);
        }
        {
            nint g = len(de);
            nint e = want; if (g != e) {
                Ꮡt.Errorf("ReadDir of %d got %d files, want %d"u8, n, g, e);
            }
        }
    };
    var readdirnamesExpect = (nint n, nint want, error wantErr) => {
        Ꮡt.Helper();
        var (fi, err) = Ꮡd.ValueSlot.Readdirnames(n);
        if (!AreEqual(err, wantErr)) {
            Ꮡt.Fatalf("Readdirnames of %d got error %v, want %v"u8, n, err, wantErr);
        }
        {
            nint g = len(fi);
            nint e = want; if (g != e) {
                Ꮡt.Errorf("Readdirnames of %d got %d files, want %d"u8, n, g, e);
            }
        }
    };
    foreach (var (_, fn) in new Action<nint, nint, error>[]{readdirExpect, readdirnamesExpect, readDirExpect}.slice()) {
        // Test the slurp case
        openDir();
        fn(0, 105, default!);
        fn(0, 0, default!);
        d.Close();
        // Slurp with -1 instead
        openDir();
        fn(-1, 105, default!);
        fn(-2, 0, default!);
        fn(0, 0, default!);
        d.Close();
        // Test the bounded case
        openDir();
        fn(1, 1, default!);
        fn(2, 2, default!);
        fn(105, 102, default!); // and tests buffer >100 case
        fn(3, 0, Δio.EOF);
        d.Close();
    }
}

internal static void touch(ж<Δtesting.T> Ꮡt, @string name) {
    var (f, err) = Create(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = f.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string good1ˢ = "good1"u8;
internal static readonly @string good2ˢ = "good2"u8;
internal static readonly @string initialReaddirˢ = "initial readdir"u8;
internal static readonly @string withXDisappearingˢ = "with x disappearing"u8;
internal static readonly @string someRealErrorˢ = "some real error"u8;

public static void TestReaddirStatFailures(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
            Ꮡt.Skipf("skipping test on %v"u8, // Windows and Plan 9 already do this correctly,
 // but are structured with different syscalls such
 // that they don't use Lstat, so the hook below for
 // testing it wouldn't work.
 Δruntime.GOOS);
        }

        ref var xerr = ref heap<error>(out var Ꮡxerr);        // error to return for x
        os_internal_test_package.LstatP.ValueSlot = (fs.FileInfo, error) (@string path) => {
            if (Ꮡxerr.ValueSlot != default! && strings.HasSuffix(path, "x"u8)) {
                return (default!, Ꮡxerr.ValueSlot);
            }
            return Lstat(path);
        };
        defer(() => {
            os_internal_test_package.LstatP.ValueSlot = Lstat;
        }, ref ᒐ);
        @string dir = Ꮡt.TempDir();
        touch(Ꮡt, filepath.Join(dir, good1ˢ));
        touch(Ꮡt, filepath.Join(dir, "x")); // will disappear or have an error
        touch(Ꮡt, filepath.Join(dir, good2ˢ));
        (slice<fs.FileInfo>, error) readDir() {
            GoFrame ᒐ = default;
            try {
                var (d, err) = Open(dir);
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
                var dʗ1 = d;
                defer(() => dʗ1.Close(), ref ᒐ);
                return d.Readdir(-1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        }
        var readDirʗ1 = readDir;
        slice<fs.FileInfo> mustReadDir(@string testName) {
            var (fis, err) = readDirʗ1();
            if (err != default!) {
                Ꮡt.Fatalf("%s: Readdir: %v"u8, testName, err);
            }
            return fis;
        }
        slice<@string> names(slice<fs.FileInfo> fis) {
            var s = new slice<@string>(len(fis));
            foreach (var (i, fi) in fis) {
                s[i] = fi.Name();
            }
            slices.Sort<slice<@string>, @string>(s);
            return s;
        }
        {
            var (got, want) = (names(mustReadDir(initialReaddirˢ)), new @string[]{"good1"u8, "good2"u8, "x"u8}.slice()); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                Ꮡt.Errorf("initial readdir got %q; want %q"u8, got, want);
            }
        }
        xerr = ErrNotExist;
        {
            var (got, want) = (names(mustReadDir(withXDisappearingˢ)), new @string[]{"good1"u8, "good2"u8}.slice()); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                Ꮡt.Errorf("with x disappearing, got %q; want %q"u8, got, want);
            }
        }
        xerr = errors.New(someRealErrorˢ);
        {
            var (_, err) = readDir(); if (!AreEqual(err, xerr)) {
                Ꮡt.Errorf("with a non-ErrNotExist error, got error %v; want %v"u8, err, xerr);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goReaddirOfFileˢ = "_Go_ReaddirOfFile"u8;
internal static readonly object readdirnamesSucceededˢ = (@string)"Readdirnames succeeded; want non-nil error"u8;

// Readdir on a regular file should fail.
public static void TestReaddirOfFile(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (f, err) = CreateTemp(Ꮡt.TempDir(), goReaddirOfFileˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        f.Write(slice<byte>("foo"u8));
        f.Close();
        (var reg, err) = Open(f.Name());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var regʗ1 = reg;
        defer(() => regʗ1.Close(), ref ᒐ);
        (var names, err) = reg.Readdirnames(-1);
        if (err == default!) {
            Ꮡt.Error(readdirnamesSucceededˢ);
        }
        ref var pe = ref heap<ж<fs.PathError>>(out var Ꮡpe);
        if (!errors.As(err, Ꮡpe) || (~pe).Path != f.Name()) {
            Ꮡt.Errorf("Readdirnames returned %q; want a PathError with path %q"u8, err, f.Name());
        }
        if (len(names) > 0) {
            Ꮡt.Errorf("unexpected dir names in regular file: %q"u8, names);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hardlinktestfromˢ = "hardlinktestfrom"u8;
internal static readonly @string hardlinktesttoˢ = "hardlinktestto"u8;
internal static readonly @string hardlinktestnoneˢ = "hardlinktestnone"u8;
internal static readonly @string linkˢ = "link"u8;
internal static readonly object fileExistsErrorˢ = (@string)"file exists error"u8;

public static void TestHardLink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = hardlinktestfromˢ;
    @string to = hardlinktesttoˢ;
    var (@file, err) = Create(to);
    if (err != default!) {
        Ꮡt.Fatalf("open %q failed: %v"u8, to, err);
    }
    {
        err = @file.Close(); if (err != default!) {
            Ꮡt.Errorf("close %q failed: %v"u8, to, err);
        }
    }
    err = Link(to, from);
    if (err != default!) {
        Ꮡt.Fatalf("link %q, %q failed: %v"u8, to, from, err);
    }
    @string none = hardlinktestnoneˢ;
    err = Link(none, none);
    // Check the returned error is well-formed.
    {
        var (lerr, ok) = err._<ж<Δos.LinkError>>(ᐧ); if (!ok || lerr.Error() == ""u8) {
            Ꮡt.Errorf("link %q, %q failed to return a valid error"u8, none, none);
        }
    }
    (var tostat, err) = Stat(to);
    if (err != default!) {
        Ꮡt.Fatalf("stat %q failed: %v"u8, to, err);
    }
    (var fromstat, err) = Stat(from);
    if (err != default!) {
        Ꮡt.Fatalf("stat %q failed: %v"u8, from, err);
    }
    if (!SameFile(tostat, fromstat)) {
        Ꮡt.Errorf("link %q, %q did not create hard link"u8, to, from);
    }
    // We should not be able to perform the same Link() a second time
    err = Link(to, from);
    switch (err.type()) {
    case ж<Δos.LinkError> errΔ1: {
        if ((~errΔ1).Op != "link"u8) {
            Ꮡt.Errorf("Link(%q, %q) err.Op = %q; want %q"u8, to, from, (~errΔ1).Op, linkˢ);
        }
        if ((~errΔ1).Old != to) {
            Ꮡt.Errorf("Link(%q, %q) err.Old = %q; want %q"u8, to, from, (~errΔ1).Old, to);
        }
        if ((~errΔ1).New != from) {
            Ꮡt.Errorf("Link(%q, %q) err.New = %q; want %q"u8, to, from, (~errΔ1).New, from);
        }
        if (!IsExist((~errΔ1).Err)) {
            Ꮡt.Errorf("Link(%q, %q) err.Err = %q; want %q"u8, to, from, (~errΔ1).Err, fileExistsErrorˢ);
        }
        break;
    }
    case null: {
        Ꮡt.Errorf("link %q, %q: expected error, got nil"u8, from, to);
        break;
    }
    default: {
        var errΔ1 = err;
        Ꮡt.Errorf("link %q, %q: expected %T, got %T %v"u8, from, to, @new<Δos.LinkError>(), errΔ1, errΔ1);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string symlinktestfromˢ = "symlinktestfrom"u8;
internal static readonly @string symlinktesttoˢ = "symlinktestto"u8;

public static void TestSymlink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = symlinktestfromˢ;
    @string to = symlinktesttoˢ;
    var (@file, err) = Create(to);
    if (err != default!) {
        Ꮡt.Fatalf("Create(%q) failed: %v"u8, to, err);
    }
    {
        err = @file.Close(); if (err != default!) {
            Ꮡt.Errorf("Close(%q) failed: %v"u8, to, err);
        }
    }
    err = Symlink(to, from);
    if (err != default!) {
        Ꮡt.Fatalf("Symlink(%q, %q) failed: %v"u8, to, from, err);
    }
    (var tostat, err) = Lstat(to);
    if (err != default!) {
        Ꮡt.Fatalf("Lstat(%q) failed: %v"u8, to, err);
    }
    if ((fs.FileMode)(tostat.Mode() & ModeSymlink) != 0) {
        Ꮡt.Fatalf("Lstat(%q).Mode()&ModeSymlink = %v, want 0"u8, to, (fs.FileMode)(tostat.Mode() & ModeSymlink));
    }
    (var fromstat, err) = Stat(from);
    if (err != default!) {
        Ꮡt.Fatalf("Stat(%q) failed: %v"u8, from, err);
    }
    if (!SameFile(tostat, fromstat)) {
        Ꮡt.Errorf("Symlink(%q, %q) did not create symlink"u8, to, from);
    }
    (fromstat, err) = Lstat(from);
    if (err != default!) {
        Ꮡt.Fatalf("Lstat(%q) failed: %v"u8, from, err);
    }
    if ((fs.FileMode)(fromstat.Mode() & ModeSymlink) == 0) {
        Ꮡt.Fatalf("Lstat(%q).Mode()&ModeSymlink = 0, want %v"u8, from, ModeSymlink);
    }
    (fromstat, err) = Stat(from);
    if (err != default!) {
        Ꮡt.Fatalf("Stat(%q) failed: %v"u8, from, err);
    }
    if (fromstat.Name() != from) {
        Ꮡt.Errorf("Stat(%q).Name() = %q, want %q"u8, from, fromstat.Name(), from);
    }
    if ((fs.FileMode)(fromstat.Mode() & ModeSymlink) != 0) {
        Ꮡt.Fatalf("Stat(%q).Mode()&ModeSymlink = %v, want 0"u8, from, (fs.FileMode)(fromstat.Mode() & ModeSymlink));
    }
    (var s, err) = Readlink(from);
    if (err != default!) {
        Ꮡt.Fatalf("Readlink(%q) failed: %v"u8, from, err);
    }
    if (s != to) {
        Ꮡt.Fatalf("Readlink(%q) = %q, want %q"u8, from, s, to);
    }
    (@file, err) = Open(from);
    if (err != default!) {
        Ꮡt.Fatalf("Open(%q) failed: %v"u8, from, err);
    }
    @file.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string longsymlinktestfromˢ = "longsymlinktestfrom"u8;

public static void TestLongSymlink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string s = "0123456789abcdef"u8;
    // Long, but not too long: a common limit is 255.
    s = s + s + s + s + s + s + s + s + s + s + s + s + s + s + s;
    @string from = longsymlinktestfromˢ;
    var err = Symlink(s, from);
    if (err != default!) {
        Ꮡt.Fatalf("symlink %q, %q failed: %v"u8, s, from, err);
    }
    (var r, err) = Readlink(from);
    if (err != default!) {
        Ꮡt.Fatalf("readlink %q failed: %v"u8, from, err);
    }
    if (r != s) {
        Ꮡt.Fatalf("after symlink %q != %q"u8, r, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string renamefromˢ = "renamefrom"u8;
internal static readonly @string renametoˢ = "renameto"u8;

public static void TestRename(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;
    var (@file, err) = Create(from);
    if (err != default!) {
        Ꮡt.Fatalf("open %q failed: %v"u8, from, err);
    }
    {
        err = @file.Close(); if (err != default!) {
            Ꮡt.Errorf("close %q failed: %v"u8, from, err);
        }
    }
    err = Rename(from, to);
    if (err != default!) {
        Ꮡt.Fatalf("rename %q, %q failed: %v"u8, to, from, err);
    }
    (_, err) = Stat(to);
    if (err != default!) {
        Ꮡt.Errorf("stat %q failed: %v"u8, to, err);
    }
}

public static void TestRenameOverwriteDest(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;
    var toData = slice<byte>("to"u8);
    var fromData = slice<byte>("from"u8);
    var err = WriteFile(to, toData, 511);
    if (err != default!) {
        Ꮡt.Fatalf("write file %q failed: %v"u8, to, err);
    }
    err = WriteFile(from, fromData, 511);
    if (err != default!) {
        Ꮡt.Fatalf("write file %q failed: %v"u8, from, err);
    }
    err = Rename(from, to);
    if (err != default!) {
        Ꮡt.Fatalf("rename %q, %q failed: %v"u8, to, from, err);
    }
    (_, err) = Stat(from);
    if (err == default!) {
        Ꮡt.Errorf("from file %q still exists"u8, from);
    }
    if (err != default! && !IsNotExist(err)) {
        Ꮡt.Fatalf("stat from: %v"u8, err);
    }
    (var toFi, err) = Stat(to);
    if (err != default!) {
        Ꮡt.Fatalf("stat %q failed: %v"u8, to, err);
    }
    if (toFi.Size() != (int64)len(fromData)) {
        Ꮡt.Errorf(@"""to"" size = %d; want %d (old ""from"" size)"u8, toFi.Size(), len(fromData));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object renameˢ = (@string)"rename"u8;

public static void TestRenameFailed(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;
    var err = Rename(from, to);
    switch (err.type()) {
    case ж<Δos.LinkError> errΔ1: {
        if ((~errΔ1).Op != "rename"u8) {
            Ꮡt.Errorf("rename %q, %q: err.Op: want %q, got %q"u8, from, to, renameˢ, (~errΔ1).Op);
        }
        if ((~errΔ1).Old != from) {
            Ꮡt.Errorf("rename %q, %q: err.Old: want %q, got %q"u8, from, to, from, (~errΔ1).Old);
        }
        if ((~errΔ1).New != to) {
            Ꮡt.Errorf("rename %q, %q: err.New: want %q, got %q"u8, from, to, to, (~errΔ1).New);
        }
        break;
    }
    case null: {
        Ꮡt.Errorf("rename %q, %q: expected error, got nil"u8, from, to);
        break;
    }
    default: {
        var errΔ1 = err;
        Ꮡt.Errorf("rename %q, %q: expected %T, got %T %v"u8, from, to, @new<Δos.LinkError>(), errΔ1, errΔ1);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doesntExistˢ = "doesnt-exist"u8;
internal static readonly @string destˢ = "dest"u8;

public static void TestRenameNotExisting(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = doesntExistˢ;
    @string to = destˢ;
    Mkdir(to, 511);
    {
        var err = Rename(from, to); if (!IsNotExist(err)) {
            Ꮡt.Errorf("Rename(%q, %q) = %v; want an IsNotExist error"u8, from, to, err);
        }
    }
}

public static void TestRenameToDirFailed(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;
    Mkdir(from, 511);
    Mkdir(to, 511);
    var err = Rename(from, to);
    switch (err.type()) {
    case ж<Δos.LinkError> errΔ1: {
        if ((~errΔ1).Op != "rename"u8) {
            Ꮡt.Errorf("rename %q, %q: err.Op: want %q, got %q"u8, from, to, renameˢ, (~errΔ1).Op);
        }
        if ((~errΔ1).Old != from) {
            Ꮡt.Errorf("rename %q, %q: err.Old: want %q, got %q"u8, from, to, from, (~errΔ1).Old);
        }
        if ((~errΔ1).New != to) {
            Ꮡt.Errorf("rename %q, %q: err.New: want %q, got %q"u8, from, to, to, (~errΔ1).New);
        }
        break;
    }
    case null: {
        Ꮡt.Errorf("rename %q, %q: expected error, got nil"u8, from, to);
        break;
    }
    default: {
        var errΔ1 = err;
        Ꮡt.Errorf("rename %q, %q: expected %T, got %T %v"u8, from, to, @new<Δos.LinkError>(), errΔ1, errΔ1);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string renameFROMˢ = "renameFROM"u8;
internal static readonly @string renamEfromˢ = "RENAMEfrom"u8;

[GoType("dyn")] internal partial struct TestRenameCaseDifference_tests {
    internal @string name;
    internal Func<error> create;
}

public static void TestRenameCaseDifference(ж<Δtesting.T> Ꮡpt) {
    @string from = renameFROMˢ;
    @string to = renamEfromˢ;
    var tests = new TestRenameCaseDifference_tests[]{
        new("dir"u8, () => Mkdir(from, 511)),
        new("file"u8, () => {
            var (fd, err) = Create(from);
            if (err != default!) {
                return err;
            }
            return fd.Close();
        })
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestRenameCaseDifference_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡpt.Run(test.name, (ж<Δtesting.T> t) => {
            t.Chdir(t.TempDir());
            {
                var errΔ1 = testʗ1.create(); if (errΔ1 != default!) {
                    t.Fatalf("failed to create test file: %s"u8, errΔ1);
                }
            }
            {
                var (_, errΔ2) = Stat(to); if (errΔ2 != default!) {
                    // Sanity check that the underlying filesystem is not case sensitive.
                    if (IsNotExist(errΔ2)) {
                        t.Skipf("case sensitive filesystem"u8);
                    }
                    t.Fatalf("stat %q, got: %q"u8, to, errΔ2);
                }
            }
            {
                var errΔ3 = Rename(from, to); if (errΔ3 != default!) {
                    t.Fatalf("unexpected error when renaming from %q to %q: %s"u8, from, to, errΔ3);
                }
            }
            var (fd, err) = Open("."u8);
            if (err != default!) {
                t.Fatalf("Open .: %s"u8, err);
            }
            // Stat does not return the real case of the file (it returns what the called asked for)
            // So we have to use readdir to get the real name of the file.
            (var dirNames, err) = fd.Readdirnames(-1);
            fd.Close();
            if (err != default!) {
                t.Fatalf("readdirnames: %s"u8, err);
            }
            {
                nint dirNamesLen = len(dirNames); if (dirNamesLen != 1) {
                    t.Fatalf("unexpected dirNames len, got %q, want %q"u8, dirNamesLen, (nint)(1));
                }
            }
            if (dirNames[0] != to) {
                t.Errorf("unexpected name, got %q, want %q"u8, dirNames[0], to);
            }
        });
    }
}

internal static Action<ж<Δtesting.T>> testStartProcess(@string dir, @string cmd, slice<@string> args, @string expect) {
    var argsʗ1 = args;
    return (ж<Δtesting.T> t) => {
        GoFrame ᒐ = default;
        try {
            t.Parallel();
            var (r, w, err) = Pipe();
            if (err != default!) {
                t.Fatalf("Pipe: %v"u8, err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
            var attr = Ꮡ(new ProcAttr(Dir: dir, Files: new ж<Δos.File>[]{default!, w, Stderr}.slice()));
            (var p, err) = StartProcess(cmd, argsʗ1, attr);
            if (err != default!) {
                t.Fatalf("StartProcess: %v"u8, err);
            }
            w.Close();
            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            Δio.Copy(new os_test_package.strings_BuilderжWriter(Ꮡb), new os_test_package.os_FileжReader(r));
            @string output = b.String();
            var (fi1, _) = Stat(strings.TrimSpace(output));
            var (fi2, _) = Stat(expect);
            if (!SameFile(fi1, fi2)) {
                t.Errorf("exec %q returned %q wanted %q"u8,
                    strings.Join(appendꓸꓸꓸ(new @string[]{cmd}.slice(), argsʗ1), " "u8), output, expect);
            }
            p.Wait();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object androidDoesnTHaveBinPwdˢ = (@string)"android doesn't have /bin/pwd"u8;
internal static readonly @string comspecˢ = "COMSPEC"u8;
internal static readonly @string pwdˢ = "pwd"u8;
internal static readonly @string absoluteˢ = "absolute"u8;
internal static readonly @string relativeˢ = "relative"u8;

public static void TestStartProcess(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new os_test_package.testing_TжTB(Ꮡt));
    Ꮡt.Parallel();
    @string dir = default!;
    @string cmd = default!;
    slice<@string> args = default!;
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8) {
        Ꮡt.Skip(androidDoesnTHaveBinPwdˢ);
    }
    else if (exprᴛ1 == "windows"u8) {
        cmd = Getenv(comspecˢ);
        dir = Getenv(systemRootˢ);
        args = new @string[]{"/c"u8, "cd"u8}.slice();
    }
    else { /* default: */
        error err = default!;
        (cmd, err) = exec.LookPath(pwdˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Can't find pwd: %v"u8, err);
        }
        dir = "/"u8;
        args = new @string[]{}.slice();
        Ꮡt.Logf("Testing with %v"u8, cmd);
    }

    var (cmddir, cmdbase) = filepath.Split(cmd);
    args = appendꓸꓸꓸ(new @string[]{cmdbase}.slice(), args);
    Ꮡt.Run(absoluteˢ, testStartProcess(dir, cmd, args, dir));
    Ꮡt.Run(relativeˢ, testStartProcess(cmddir, cmdbase, args, cmddir));
}

internal static void checkMode(ж<Δtesting.T> Ꮡt, @string path, fs.FileMode mode) {
    var (dir, err) = Stat(path);
    if (err != default!) {
        Ꮡt.Fatalf("Stat %q (looking for mode %#o): %s"u8, path, mode, err);
    }
    if ((fs.FileMode)(dir.Mode() & ModePerm) != mode) {
        Ꮡt.Errorf("Stat %q: mode %#o want %#o"u8, path, dir.Mode(), mode);
    }
}

public static void TestChmod(ж<Δtesting.T> Ꮡt) {
    // Chmod is not supported on wasip1.
    if (Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skip("Chmod is not supported on " + Δruntime.GOOS);
    }
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    // Creation mode is read write
    var fm = ((fs.FileMode)302);
    if (Δruntime.GOOS == "windows"u8) {
        fm = ((fs.FileMode)292); // read-only file
    }
    {
        var err = Chmod(f.Name(), fm); if (err != default!) {
            Ꮡt.Fatalf("chmod %s %#o: %s"u8, f.Name(), fm, err);
        }
    }
    checkMode(Ꮡt, f.Name(), fm);
    fm = ((fs.FileMode)83);
    if (Δruntime.GOOS == "windows"u8) {
        fm = ((fs.FileMode)438); // read-write file
    }
    {
        var err = f.Chmod(fm); if (err != default!) {
            Ꮡt.Fatalf("chmod %s %#o: %s"u8, f.Name(), fm, err);
        }
    }
    checkMode(Ꮡt, f.Name(), fm);
}

internal static void checkSize(ж<Δtesting.T> Ꮡt, ж<Δos.File> Ꮡf, int64 size) {
    ref var f = ref Ꮡf.DerefOrNull();

    Ꮡt.Helper();
    var (dir, err) = Ꮡf.Stat();
    if (err != default!) {
        Ꮡt.Fatalf("Stat %q (looking for size %d): %s"u8, f.Name(), size, err);
    }
    if (dir.Size() != size) {
        Ꮡt.Errorf("Stat %q: size %d want %d"u8, f.Name(), dir.Size(), size);
    }
}

public static void TestFTruncate(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    checkSize(Ꮡt, f, 0);
    f.Write(slice<byte>("hello, world\n"u8));
    checkSize(Ꮡt, f, 13);
    f.Truncate(10);
    checkSize(Ꮡt, f, 10);
    f.Truncate(1024);
    checkSize(Ꮡt, f, 1024);
    f.Truncate(0);
    checkSize(Ꮡt, f, 0);
    var (_, err) = f.Write(slice<byte>("surprise!"u8));
    if (err == default!) {
        checkSize(Ꮡt, f, 13 + 9); // wrote at offset past where hello, world was.
    }
}

public static void TestTruncate(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    checkSize(Ꮡt, f, 0);
    f.Write(slice<byte>("hello, world\n"u8));
    checkSize(Ꮡt, f, 13);
    Truncate(f.Name(), 10);
    checkSize(Ꮡt, f, 10);
    Truncate(f.Name(), 1024);
    checkSize(Ꮡt, f, 1024);
    Truncate(f.Name(), 0);
    checkSize(Ꮡt, f, 0);
    var (_, err) = f.Write(slice<byte>("surprise!"u8));
    if (err == default!) {
        checkSize(Ꮡt, f, 13 + 9); // wrote at offset past where hello, world was.
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonexistentˢ = "nonexistent"u8;

public static void TestTruncateNonexistentFile(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    void assertPathError(Δtesting.TB tΔ1, @string pathΔ1, error errΔ1) {
        tΔ1.Helper();
        {
            var (pe, ok) = errΔ1._<ж<fs.PathError>>(ᐧ); if (!ok || !IsNotExist(errΔ1) || (~pe).Path != pathΔ1) {
                tΔ1.Errorf("got error: %v\nwant an ErrNotExist PathError with path %q"u8, errΔ1, pathΔ1);
            }
        }
    }
    @string path = filepath.Join(Ꮡt.TempDir(), nonexistentˢ);
    var err = Truncate(path, 1);
    assertPathError(new os_test_package.testing_TжTB(Ꮡt), path, err);
    // Truncate shouldn't create any new file.
    (_, err) = Stat(path);
    assertPathError(new os_test_package.testing_TжTB(Ꮡt), path, err);
}

// A sloppy way to check if noatime flag is set (as all filesystems are
// checked, not just the one we're interested in). A correct way
// would be to use statvfs syscall and check if flags has ST_NOATIME,
// but the syscall is OS-specific and is not even wired into Go stdlib.
//
// Only used on NetBSD (which ignores explicit atime updates with noatime).
internal static Func<bool> hasNoatime = Δsync.OnceValue(bool () => {
    var (mounts, _) = ReadFile("/proc/mounts"u8);
    return bytes.Contains(mounts, slice<byte>("noatime"u8));
});

public static void TestChtimes(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    // This should be an empty file (see #68687, #68663).
    f.Close();
    testChtimes(Ꮡt, f.Name());
}

public static void TestChtimesOmit(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    testChtimesOmit(Ꮡt, true, false);
    testChtimesOmit(Ꮡt, false, true);
    testChtimesOmit(Ꮡt, true, true);
    testChtimesOmit(Ꮡt, false, false); // Same as TestChtimes.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object knownDragonFlyBsdIssueˢ = (@string)"Known DragonFly BSD issue (won't work when both times are omitted); ignoring."u8;
internal static readonly object knownDragonFlyBsdIssueˢ2 = (@string)"Known DragonFly BSD issue (atime not supported on hammer2); ignoring."u8;
internal static readonly object knownNetBSDIssueAtimeNotˢ = (@string)"Known NetBSD issue (atime not changed on fs mounted with noatime); ignoring."u8;

internal static void testChtimesOmit(ж<Δtesting.T> Ꮡt, bool omitAt, bool omitMt) {
    Ꮡt.Logf("omit atime: %v, mtime: %v"u8, omitAt, omitMt);
    var @file = newFile(Ꮡt);
    // This should be an empty file (see #68687, #68663).
    @string name = @file.Name();
    var err = @file.Close();
    if (err != default!) {
        Ꮡt.Error(err);
    }
    (var fs, err) = Stat(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var wantAtime = os_internal_test_package.Atime(fs);
    var wantMtime = fs.ModTime();
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8) {
        wantAtime = wantAtime.Truncate(time.ΔSecond);
        wantMtime = wantMtime.Truncate(time.ΔSecond);
    }

    time.Time setAtime = default!;                      // Zero value means omit.
    time.Time setMtime = default!;
    if (!omitAt) {
        wantAtime = wantAtime.Add(-1 * time.ΔSecond);
        setAtime = wantAtime;
    }
    if (!omitMt) {
        wantMtime = wantMtime.Add(-1 * time.ΔSecond);
        setMtime = wantMtime;
    }
    // Change the times accordingly.
    {
        var errΔ1 = Chtimes(name, setAtime, setMtime); if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
        }
    }
    // Verify the expectations.
    (fs, err) = Stat(name);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    var gotAtime = os_internal_test_package.Atime(fs);
    var gotMtime = fs.ModTime();
    // TODO: remove the dragonfly omitAt && omitMt exceptions below once the
    // fix (https://github.com/DragonFlyBSD/DragonFlyBSD/commit/c7c71870ed0)
    // is available generally and on CI runners.
    if (!gotAtime.Equal(wantAtime)) {
        @string errormsg = fmt.Sprintf("atime mismatch, got: %q, want: %q"u8, gotAtime, wantAtime);
        var exprᴛ2 = Δruntime.GOOS;
        if (exprᴛ2 == "plan9"u8) {
        }
        else if (exprᴛ2 == "dragonfly"u8) {
            if (omitAt && omitMt){
                // Mtime is the time of the last change of content.
                // Similarly, atime is set whenever the contents are
                // accessed; also, it is set whenever mtime is set.
                Ꮡt.Log(errormsg);
                Ꮡt.Log(knownDragonFlyBsdIssueˢ);
            } else {
                // Assume hammer2 fs; https://www.dragonflybsd.org/hammer/ says:
                // > Because HAMMER2 is a block copy-on-write filesystem,
                // > the "atime" field is not supported and will typically
                // > just reflect local system in-memory caches or mtime.
                //
                // TODO: if only can CI define TMPDIR to point to a tmpfs
                // (e.g. /var/run/shm), this exception can be removed.
                Ꮡt.Log(errormsg);
                Ꮡt.Log(knownDragonFlyBsdIssueˢ2);
            }
        }
        else if (exprᴛ2 == "netbsd"u8) {
            if (!omitAt && hasNoatime()){
                Ꮡt.Log(errormsg);
                Ꮡt.Log(knownNetBSDIssueAtimeNotˢ);
            } else {
                Ꮡt.Error(errormsg);
            }
        }
        else { /* default: */
            Ꮡt.Error(errormsg);
        }

    }
    if (!gotMtime.Equal(wantMtime)) {
        @string errormsg = fmt.Sprintf("mtime mismatch, got: %q, want: %q"u8, gotMtime, wantMtime);
        var exprᴛ3 = Δruntime.GOOS;
        if (exprᴛ3 == "dragonfly"u8) {
            if (omitAt && omitMt){
                Ꮡt.Log(errormsg);
                Ꮡt.Log(knownDragonFlyBsdIssueˢ);
            } else {
                Ꮡt.Error(errormsg);
            }
        }
        else { /* default: */
            Ꮡt.Error(errormsg);
        }

    }
}

public static void TestChtimesDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    testChtimes(Ꮡt, Ꮡt.TempDir());
}

internal static void testChtimes(ж<Δtesting.T> Ꮡt, @string name) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (st, err) = Stat(name);
    if (err != default!) {
        Ꮡt.Fatalf("Stat %s: %s"u8, name, err);
    }
    var preStat = st;
    // Move access and modification time back a second
    var at = os_internal_test_package.Atime(preStat);
    var mt = preStat.ModTime();
    err = Chtimes(name, at.Add(-time.ΔSecond), mt.Add(-time.ΔSecond));
    if (err != default!) {
        Ꮡt.Fatalf("Chtimes %s: %s"u8, name, err);
    }
    (st, err) = Stat(name);
    if (err != default!) {
        Ꮡt.Fatalf("second Stat %s: %s"u8, name, err);
    }
    var postStat = st;
    var pat = os_internal_test_package.Atime(postStat);
    var pmt = postStat.ModTime();
    if (!pat.Before(at)) {
        @string errormsg = fmt.Sprintf("AccessTime didn't go backwards; was=%v, after=%v"u8, at, pat);
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "plan9"u8) {
        }
        else if (exprᴛ1 == "netbsd"u8) {
            if (hasNoatime()){
                // Mtime is the time of the last change of
                // content.  Similarly, atime is set whenever
                // the contents are accessed; also, it is set
                // whenever mtime is set.
                Ꮡt.Log(errormsg);
                Ꮡt.Log(knownNetBSDIssueAtimeNotˢ);
            } else {
                Ꮡt.Error(errormsg);
            }
        }
        else { /* default: */
            Ꮡt.Error(errormsg);
        }

    }
    if (!pmt.Before(mt)) {
        Ꮡt.Errorf("ModTime didn't go backwards; was=%v, after=%v"u8, mt, pmt);
    }
}

public static void TestChtimesToUnixZero(ж<Δtesting.T> Ꮡt) {
    var @file = newFile(Ꮡt);
    @string fn = @file.Name();
    {
        var (_, errΔ1) = @file.Write(slice<byte>("hi"u8)); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        var errΔ2 = @file.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    var unixZero = time.Unix(0, 0);
    {
        var errΔ3 = Chtimes(fn, unixZero, unixZero); if (errΔ3 != default!) {
            Ꮡt.Fatalf("Chtimes failed: %v"u8, errΔ3);
        }
    }
    var (st, err) = Stat(fn);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var mt = st.ModTime(); if (mt != unixZero) {
            Ꮡt.Errorf("mtime is %v, want %v"u8, mt, unixZero);
        }
    }
}

public static void TestFileChdir(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (wd, err) = Getwd();
        if (err != default!) {
            Ꮡt.Fatalf("Getwd: %s"u8, err);
        }
        Ꮡt.Chdir("."u8); // Ensure wd is restored after the test.
        (var fd, err) = Open("."u8);
        if (err != default!) {
            Ꮡt.Fatalf("Open .: %s"u8, err);
        }
        var fdʗ1 = fd;
        defer(() => fdʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = Chdir("/"u8); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Chdir /: %s"u8, errΔ1);
            }
        }
        {
            var errΔ2 = fd.Chdir(); if (errΔ2 != default!) {
                Ꮡt.Fatalf("fd.Chdir: %s"u8, errΔ2);
            }
        }
        (var wdNew, err) = Getwd();
        if (err != default!) {
            Ꮡt.Fatalf("Getwd: %s"u8, err);
        }
        (var wdInfo, err) = fd.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var newInfo, err) = Stat(wdNew);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!SameFile(wdInfo, newInfo)) {
            Ꮡt.Fatalf("fd.Chdir failed: got %s, want %s"u8, wdNew, wd);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pwdˢ2 = "PWD"u8;
internal static readonly @string tmpˢ = "/tmp"u8;

public static void TestChdirAndGetwd(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir()); // Ensure wd is restored after the test.
    // These are chosen carefully not to be symlinks on a Mac
    // (unlike, say, /var, /etc), except /tmp, which we handle below.
    var dirs = new @string[]{"/"u8, "/usr/bin"u8, "/tmp"u8}.slice();
    // /usr/bin does not usually exist on Plan 9 or Android.
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8) {
        dirs = new @string[]{"/system/bin"u8}.slice();
    }
    else if (exprᴛ1 == "plan9"u8) {
        dirs = new @string[]{"/"u8, "/usr"u8}.slice();
    }
    else if (exprᴛ1 == "ios"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "wasip1"u8) {
        dirs = default!;
        foreach (var (_, dir) in new @string[]{Ꮡt.TempDir(), Ꮡt.TempDir()}.slice()) {
            // Expand symlinks so path equality tests work.
            var (dirΔ1, err) = filepath.EvalSymlinks(dir);
            if (err != default!) {
                Ꮡt.Fatalf("EvalSymlinks: %v"u8, err);
            }
            dirs = append(dirs, dirΔ1);
        }
    }

    for (nint mode = 0; mode < 2; mode++) {
        foreach (var (_, d) in dirs) {
            error err = default!;
            if (mode == 0){
                err = Chdir(d);
            } else {
                var (fd1, err1Δ1) = Open(d);
                if (err1Δ1 != default!) {
                    Ꮡt.Errorf("Open %s: %s"u8, d, err1Δ1);
                    continue;
                }
                err = fd1.Chdir();
                fd1.Close();
            }
            if (d == "/tmp"u8) {
                Setenv(pwdˢ2, tmpˢ);
            }
            var (pwd, err1) = Getwd();
            if (err != default!) {
                Ꮡt.Fatalf("Chdir %s: %s"u8, d, err);
            }
            if (err1 != default!) {
                Ꮡt.Fatalf("Getwd in %s: %s"u8, d, err1);
            }
            if (!equal(pwd, d)) {
                Ꮡt.Fatalf("Getwd returned %q want %q"u8, pwd, d);
            }
        }
    }
}

// Test that Chdir+Getwd is program-wide.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestProgWideChdir(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        const nint N = 10;
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        var hold = new channel<EmptyStruct>(0);
        var done = new channel<EmptyStruct>(0);
        @string d = Ꮡt.TempDir();
        Ꮡt.Chdir(d);
        // Note the deferred Wait must be called after the deferred close(done),
        // to ensure the N goroutines have been released even if the main goroutine
        // calls Fatalf. It must be called before the Chdir back to the original
        // directory, and before the deferred deletion implied by TempDir,
        // so as not to interfere while the N goroutines are still running.
        defer(Ꮡwg.Wait, ref ᒐ);
        defer(ᴛ1 => close(ᴛ1), done, ref ᒐ);
        for (nint i = 0; i < N; i++) {
            Ꮡwg.Add(1);
            var doneʗ1 = done;
            var holdʗ1 = hold;
            goǃ((nint iΔ1) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    // Lock half the goroutines in their own operating system
                    // thread to exercise more scheduler possibilities.
                    if (iΔ1 % 2 == 1) {
                        // On Plan 9, after calling LockOSThread, the goroutines
                        // run on different processes which don't share the working
                        // directory. This used to be an issue because Go expects
                        // the working directory to be program-wide.
                        // See issue 9428.
                        Δruntime.LockOSThread();
                    }
                    var selᴛ3 = doneʗ1;
                    var selᴛ4 = holdʗ1;
                    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
                    case 0 when selᴛ3.ꟷᐳ(out _): {
                        return;
                    }
                    case 1 when selᴛ4.ꟷᐳ(out _): {
                        break;
                    }}
                    // Getwd might be wrong
                    var (f0, errΔ1) = Stat("."u8);
                    if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                        return;
                    }
                    (var pwd, errΔ1) = Getwd();
                    if (errΔ1 != default!) {
                        Ꮡt.Errorf("Getwd: %v"u8, errΔ1);
                        return;
                    }
                    if (pwd != d) {
                        Ꮡt.Errorf("Getwd() = %q, want %q"u8, pwd, d);
                        return;
                    }
                    (var f1, errΔ1) = Stat(pwd);
                    if (errΔ1 != default!) {
                        Ꮡt.Error(errΔ1);
                        return;
                    }
                    if (!SameFile(f0, f1)) {
                        Ꮡt.Errorf(@"Samefile(Stat("".""), Getwd()) reports false (%s != %s)"u8, f0.Name(), f1.Name());
                        return;
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, i);
        }
        error err = default!;
        {
            err = Chdir(d); if (err != default!) {
                Ꮡt.Fatalf("Chdir: %v"u8, err);
            }
        }
        // OS X sets TMPDIR to a symbolic link.
        // So we resolve our working directory again before the test.
        (d, err) = Getwd();
        if (err != default!) {
            Ꮡt.Fatalf("Getwd: %v"u8, err);
        }
        close(hold);
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ = "hello, world\n"u8;

[GoType("dyn")] internal partial struct TestSeek_test {
    internal int64 @in;
    internal nint whence;
    internal int64 @out;
}

public static void TestSeek(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    @string data = dataᶜ;
    Δio.WriteString(new Δos.FileжWriter(f), data);
// Issue 21681, Windows 4G-1, etc:
    slice<TestSeek_test> tests = new TestSeek_test[]{
        new(0, Δio.SeekCurrent, (int64)len(data)),
        new(0, Δio.SeekStart, 0),
        new(5, Δio.SeekStart, 5),
        new(0, Δio.SeekEnd, (int64)len(data)),
        new(0, Δio.SeekStart, 0),
        new(-1, Δio.SeekEnd, (int64)len(data) - 1),
        new(8589934592L, Δio.SeekStart, 8589934592L),
        new(8589934592L, Δio.SeekEnd, 8589934605L),
        new(4294967295L, Δio.SeekStart, 4294967295L),
        new(0, Δio.SeekCurrent, 4294967295L),
        new(8589934591L, Δio.SeekStart, 8589934591L),
        new(0, Δio.SeekCurrent, 8589934591L)
    }.slice();
    foreach (var (i, tt) in tests) {
        var (off, err) = f.Seek(tt.@in, tt.whence);
        if (off != tt.@out || err != default!) {
            Ꮡt.Errorf("#%d: Seek(%v, %v) = %v, %v want %v, nil"u8, i, tt.@in, tt.whence, off, err, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object seekOnPipeShouldFailˢ = (@string)"Seek on pipe should fail"u8;

public static void TestSeekError(ж<Δtesting.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skipf("skipping test on %v"u8, Δruntime.GOOS);
    }

    Ꮡt.Parallel();
    var (r, w, err) = Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = r.Seek(0, 0);
    if (err == default!) {
        Ꮡt.Fatal(seekOnPipeShouldFailˢ);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !AreEqual((~perr).Err, syscall.ESPIPE)) {
            Ꮡt.Errorf("Seek returned error %v, want &PathError{Err: syscall.ESPIPE}"u8, err);
        }
    }
    (_, err) = w.Seek(0, 0);
    if (err == default!) {
        Ꮡt.Fatal(seekOnPipeShouldFailˢ);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !AreEqual((~perr).Err, syscall.ESPIPE)) {
            Ꮡt.Errorf("Seek returned error %v, want &PathError{Err: syscall.ESPIPE}"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noRootˢ = "NoRoot"u8;
internal static readonly @string inRootˢ = "InRoot"u8;

public static void TestOpenError(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string dir = makefs(Ꮡt, new @string[]{
        "is-a-file"u8,
        "is-a-dir/"u8
    }.slice());
    Ꮡt.Run(noRootˢ, (ж<Δtesting.T> tΔ1) => {
        testOpenError(tΔ1, dir, false);
    });
    Ꮡt.Run(inRootˢ, (ж<Δtesting.T> tΔ2) => {
        testOpenError(tΔ2, dir, true);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileˢ = "file "u8;

[GoType("dyn")] internal partial struct testOpenError_type {
    internal @string path;
    internal nint mode;
    internal error error;
}

internal static void testOpenError(ж<Δtesting.T> Ꮡt, @string dir, bool rooted) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        ж<Δos.Root> r = default!;
        if (rooted) {
            error err = default!;
            (r, err) = OpenRoot(dir);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
        }
        foreach (var (_, tt) in new testOpenError_type[]{new(
            "no-such-file"u8,
            O_RDONLY,
            syscall.ENOENT
        ), new(
            "is-a-dir"u8,
            O_WRONLY,
            syscall.EISDIR
        ), new(
            "is-a-file/no-such-file"u8,
            O_WRONLY,
            syscall.ENOTDIR
        )
        }.slice()) {
            ж<Δos.File> f = default!;
            error err = default!;
            @string name = default!;
            if (rooted){
                name = fmt.Sprintf("Root(%q).OpenFile(%q, %d)"u8, dir, tt.path, tt.mode);
                (f, err) = r.OpenFile(tt.path, tt.mode, 0);
            } else {
                @string path = filepath.Join(dir, tt.path);
                name = fmt.Sprintf("OpenFile(%q, %d)"u8, path, tt.mode);
                (f, err) = OpenFile(path, tt.mode, 0);
            }
            if (err == default!) {
                Ꮡt.Errorf("%v succeeded"u8, name);
                f.Close();
                continue;
            }
            var (perr, ok) = err._<ж<fs.PathError>>(ᐧ);
            if (!ok) {
                Ꮡt.Errorf("%v returns error of %T type; want *PathError"u8, name, err);
            }
            if (!AreEqual((~perr).Err, tt.error)) {
                if (Δruntime.GOOS == "plan9"u8) {
                    @string syscallErrStr = (~perr).Err.Error();
                    @string expectedErrStr = strings.Replace(tt.error.Error(), fileˢ, ""u8, 1);
                    if (!strings.HasSuffix(syscallErrStr, expectedErrStr)) {
                        // Some Plan 9 file servers incorrectly return
                        // EPERM or EACCES rather than EISDIR when a directory is
                        // opened for write.
                        if (AreEqual(tt.error, syscall.EISDIR) && (strings.HasSuffix(syscallErrStr, syscall.EPERM.Error()) || strings.HasSuffix(syscallErrStr, syscall.EACCES.Error()))) {
                            continue;
                        }
                        Ꮡt.Errorf("%v = _, %q; want suffix %q"u8, name, syscallErrStr, expectedErrStr);
                    }
                    continue;
                }
                if (Δruntime.GOOS == "dragonfly"u8) {
                    // DragonFly incorrectly returns EACCES rather
                    // EISDIR when a directory is opened for write.
                    if (AreEqual(tt.error, syscall.EISDIR) && AreEqual((~perr).Err, syscall.EACCES)) {
                        continue;
                    }
                }
                Ꮡt.Errorf("%v = _, %q; want %q"u8, name, (~perr).Err.Error(), tt.error.Error());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object openSucceededˢ = (@string)@"Open("""") succeeded"u8;

public static void TestOpenNoName(ж<Δtesting.T> Ꮡt) {
    var (f, err) = Open(""u8);
    if (err == default!) {
        f.Close();
        Ꮡt.Fatal(openSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hostnameˢ = "hostname"u8;
internal static readonly object skippingTestTestRequiresˢ = (@string)"skipping test; test requires hostname but it does not exist"u8;

internal static @string runBinHostname(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Run /bin/hostname and collect output.
        var (r, w, err) = Pipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        (var path, err) = exec.LookPath(hostnameˢ);
        if (err != default!) {
            if (errors.Is(err, exec.ErrNotFound)) {
                Ꮡt.Skip(skippingTestTestRequiresˢ);
            }
            Ꮡt.Fatal(err);
        }
        var argv = new @string[]{"hostname"u8}.slice();
        if (Δruntime.GOOS == "aix"u8) {
            argv = new @string[]{"hostname"u8, "-s"u8}.slice();
        }
        (var p, err) = StartProcess(path, argv, Ꮡ(new ProcAttr(Files: new ж<Δos.File>[]{default!, w, Stderr}.slice())));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        w.Close();
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Δio.Copy(new os_test_package.strings_BuilderжWriter(Ꮡb), new os_test_package.os_FileжReader(r));
        (_, err) = p.Wait();
        if (err != default!) {
            Ꮡt.Fatalf("run hostname Wait: %v"u8, err);
        }
        err = p.Kill();
        if (err == default!) {
            Ꮡt.Errorf("expected an error from Kill running 'hostname'"u8);
        }
        @string output = b.String();
        {
            nint n = len(output); if (n > 0 && output[n - 1] == (rune)'\n') {
                output = output.slice(0, n - 1);
            }
        }
        if (output == ""u8) {
            Ꮡt.Fatalf("/bin/hostname produced no output"u8);
        }
        return output;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static void testWindowsHostname(ж<Δtesting.T> Ꮡt, @string hostname) {
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), hostnameˢ);
    var (@out, err) = cmd.Output();
    if (err != default!) {
        Ꮡt.Fatalf("Failed to execute hostname command: %v %s"u8, err, @out);
    }
    @string want = strings.Trim(((@string)@out), "\r\n"u8);
    if (hostname != want) {
        Ꮡt.Fatalf("Hostname() = %q != system hostname of %q"u8, hostname, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object hostnameReturnedEmptyˢ = (@string)"Hostname returned empty string and no error"u8;

public static void TestHostname(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (hostname, err) = Hostname();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (hostname == ""u8) {
        Ꮡt.Fatal(hostnameReturnedEmptyˢ);
    }
    if (strings.Contains(hostname, "\x00"u8)) {
        Ꮡt.Fatalf("unexpected zero byte in hostname: %q"u8, hostname);
    }
    // There is no other way to fetch hostname on windows, but via winapi.
    // On Plan 9 it can be taken from #c/sysname as Hostname() does.
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8) {
        return;
    }
    if (exprᴛ1 == "windows"u8) {
        testWindowsHostname(Ꮡt, // No /bin/hostname to verify against.
 hostname);
        return;
    }

    testenv.MustHaveExec(new os_test_package.testing_TжTB(Ꮡt));
    // Check internal Hostname() against the output of /bin/hostname.
    // Allow that the internal Hostname returns a Fully Qualified Domain Name
    // and the /bin/hostname only returns the first component
    @string want = runBinHostname(Ꮡt);
    if (hostname != want) {
        var (host, _, ok) = strings.Cut(hostname, "."u8);
        if (!ok || host != want) {
            Ꮡt.Errorf("Hostname() = %q, want %q"u8, hostname, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object worldˢ = (@string)"world"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ1 = "hello, world\n"u8;

public static void TestReadAt(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    @string data = dataᶜ1;
    Δio.WriteString(new Δos.FileжWriter(f), data);
    var b = new slice<byte>(5);
    var (n, err) = f.ReadAt(b, 7);
    if (err != default! || n != len(b)) {
        Ꮡt.Fatalf("ReadAt 7: %d, %v"u8, n, err);
    }
    if (((sstring)b) != "world"u8) {
        Ꮡt.Fatalf("ReadAt 7: have %q want %q"u8, ((@string)b), worldˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ2 = "hello, world\n"u8;

// Verify that ReadAt doesn't affect seek offset.
// In the Plan 9 kernel, there used to be a bug in the implementation of
// the pread syscall, where the channel offset was erroneously updated after
// calling pread on a file.
public static void TestReadAtOffset(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    @string data = dataᶜ2;
    Δio.WriteString(new Δos.FileжWriter(f), data);
    f.Seek(0, 0);
    var b = new slice<byte>(5);
    var (n, err) = f.ReadAt(b, 7);
    if (err != default! || n != len(b)) {
        Ꮡt.Fatalf("ReadAt 7: %d, %v"u8, n, err);
    }
    if (((sstring)b) != "world"u8) {
        Ꮡt.Fatalf("ReadAt 7: have %q want %q"u8, ((@string)b), worldˢ);
    }
    (n, err) = f.Read(b);
    if (err != default! || n != len(b)) {
        Ꮡt.Fatalf("Read: %d, %v"u8, n, err);
    }
    if (((sstring)b) != "hello"u8) {
        Ꮡt.Fatalf("Read: have %q want %q"u8, ((@string)b), helloˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ3 = "hello, world\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantsubᶜ = "negative offset"u8;

// Verify that ReadAt doesn't allow negative offset.
public static void TestReadAtNegativeOffset(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    @string data = dataᶜ3;
    Δio.WriteString(new Δos.FileжWriter(f), data);
    f.Seek(0, 0);
    var b = new slice<byte>(5);
    var (n, err) = f.ReadAt(b, -10);
    @string wantsub = wantsubᶜ;
    if (!strings.Contains(fmt.Sprint(err), wantsub) || n != 0) {
        Ꮡt.Errorf("ReadAt(-10) = %v, %v; want 0, ...%q..."u8, n, err, wantsub);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloWorldˢ = (@string)"hello, WORLD\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ4 = "hello, world\n"u8;

public static void TestWriteAt(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    @string data = dataᶜ4;
    Δio.WriteString(new Δos.FileжWriter(f), data);
    var (n, err) = f.WriteAt(slice<byte>("WORLD"u8), 7);
    if (err != default! || n != 5) {
        Ꮡt.Fatalf("WriteAt 7: %d, %v"u8, n, err);
    }
    (var b, err) = ReadFile(f.Name());
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile %s: %v"u8, f.Name(), err);
    }
    if (((sstring)b) != "hello, WORLD\n"u8) {
        Ꮡt.Fatalf("after write: have %q want %q"u8, ((@string)b), helloWorldˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantsubᶜ1 = "negative offset"u8;

// Verify that WriteAt doesn't allow negative offset.
public static void TestWriteAtNegativeOffset(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    var (n, err) = f.WriteAt(slice<byte>("WORLD"u8), -10);
    @string wantsub = wantsubᶜ1;
    if (!strings.Contains(fmt.Sprint(err), wantsub) || n != 0) {
        Ꮡt.Errorf("WriteAt(-10) = %v, %v; want 0, ...%q..."u8, n, err, wantsub);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeAtInAppendModeTxtˢ = "write_at_in_append_mode.txt"u8;

// Verify that WriteAt doesn't work in append mode.
public static void TestWriteAtInAppendMode(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Chdir(Ꮡt.TempDir());
        var (f, err) = OpenFile(writeAtInAppendModeTxtˢ, (nint)(O_APPEND | O_CREATE), 438);
        if (err != default!) {
            Ꮡt.Fatalf("OpenFile: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (_, err) = f.WriteAt(slice<byte>(""u8), 1);
        if (!AreEqual(err, os_internal_test_package.ErrWriteAtInAppendMode)) {
            Ꮡt.Fatalf("f.WriteAt returned %v, expected %v"u8, err, os_internal_test_package.ErrWriteAtInAppendMode);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string writeFile(ж<Δtesting.T> Ꮡt, ж<Δos.Root> Ꮡr, @string fname, nint flag, @string text) {
    Ꮡt.Helper();
    ж<Δos.File> f = default!;
    error err = default!;
    if (Ꮡr == nil){
        (f, err) = OpenFile(fname, flag, 438);
    } else {
        (f, err) = Ꮡr.OpenFile(fname, flag, 438);
    }
    if (err != default!) {
        Ꮡt.Fatalf("Open: %v"u8, err);
    }
    (var n, err) = Δio.WriteString(new Δos.FileжWriter(f), text);
    if (err != default!) {
        Ꮡt.Fatalf("WriteString: %d, %v"u8, n, err);
    }
    f.Close();
    (var data, err) = ReadFile(fname);
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile: %v"u8, err);
    }
    return ((@string)data);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newˢ = "new"u8;
internal static readonly @string appendˢ = "|append"u8;
internal static readonly object newAppendˢ = (@string)"new|append"u8;
internal static readonly object newAppendAppendˢ = (@string)"new|append|append"u8;
internal static readonly @string newAppendˢ2 = "new&append"u8;
internal static readonly @string oldˢ = "old"u8;
internal static readonly object oldAppendˢ = (@string)"old&append"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fᶜ = "append.txt"u8;

public static void TestAppend(ж<Δtesting.T> Ꮡt) {
    testMaybeRooted(Ꮡt, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        @string f = fᶜ;
        @string s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_TRUNC) | O_RDWR), newˢ);
        if (s != "new"u8) {
            tΔ1.Fatalf("writeFile: have %q want %q"u8, s, newˢ);
        }
        s = writeFile(tΔ1, r, f, (nint)(O_APPEND | O_RDWR), appendˢ);
        if (s != "new|append"u8) {
            tΔ1.Fatalf("writeFile: have %q want %q"u8, s, newAppendˢ);
        }
        s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_APPEND) | O_RDWR), appendˢ);
        if (s != "new|append|append"u8) {
            tΔ1.Fatalf("writeFile: have %q want %q"u8, s, newAppendAppendˢ);
        }
        var err = Remove(f);
        if (err != default!) {
            tΔ1.Fatalf("Remove: %v"u8, err);
        }
        s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_APPEND) | O_RDWR), newAppendˢ2);
        if (s != "new&append"u8) {
            tΔ1.Fatalf("writeFile: after append have %q want %q"u8, s, newAppendˢ2);
        }
        s = writeFile(tΔ1, r, f, (nint)(O_CREATE | O_RDWR), oldˢ);
        if (s != "old&append"u8) {
            tΔ1.Fatalf("writeFile: after create have %q want %q"u8, s, oldAppendˢ);
        }
        s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_TRUNC) | O_RDWR), newˢ);
        if (s != "new"u8) {
            tΔ1.Fatalf("writeFile: after truncate have %q want %q"u8, s, newˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestWhenRunningˢ = (@string)"skipping test when running as root"u8;

[GoType("dyn")] internal partial struct TestFilePermissions_type {
    internal @string name;
    internal fs.FileMode mode;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ = "f"u8;

// TestFilePermissions tests setting Unix permission bits on file creation.
public static void TestFilePermissions(ж<Δtesting.T> Ꮡt) {
    if (Getuid() == 0) {
        Ꮡt.Skip(skippingTestWhenRunningˢ);
    }
    foreach (var (_, vᴛ1) in new TestFilePermissions_type[]{
        new("r"u8, 292),
        new("w"u8, 146),
        new("rw"u8, 438)
    }.slice()) {
        ref var test = ref heap(new TestFilePermissions_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<Δtesting.T> tΔ1) => {
            var exprᴛ1 = Δruntime.GOOS;
            if (exprᴛ1 == "windows"u8) {
                if ((fs.FileMode)(testʗ1.mode & 292) == 0) {
                    tΔ1.Skip("write-only files not supported on " + Δruntime.GOOS);
                }
            }
            else if (exprᴛ1 == "wasip1"u8) {
                tΔ1.Skip("file permissions not supported on " + Δruntime.GOOS);
            }

            var testʗ2 = testʗ1;
            testMaybeRooted(tΔ1, (ж<Δtesting.T> tΔ2, ж<Δos.Root> r) => {
                @string filename = filenameᶜ;
                ж<Δos.File> f = default!;
                error err = default!;
                if (r == nil){
                    (f, err) = OpenFile(filename, (nint)((nint)(nint)(O_RDWR | O_CREATE) | O_EXCL), testʗ2.mode);
                } else {
                    (f, err) = r.OpenFile(filename, (nint)((nint)(nint)(O_RDWR | O_CREATE) | O_EXCL), testʗ2.mode);
                }
                if (err != default!) {
                    tΔ2.Fatal(err);
                }
                f.Close();
                (var b, err) = ReadFile(filename);
                if ((fs.FileMode)(testʗ2.mode & 292) != 0){
                    if (err != default!) {
                        tΔ2.Errorf("ReadFile = %v; want success"u8, err);
                    }
                } else {
                    if (err == default!) {
                        tΔ2.Errorf("ReadFile = %q, <nil>; want failure"u8, ((@string)b));
                    }
                }
                (_, err) = Stat(filename);
                if (err != default!) {
                    tΔ2.Errorf("Stat = %v; want success"u8, err);
                }
                err = WriteFile(filename, default!, 438);
                if ((fs.FileMode)(testʗ2.mode & 146) != 0){
                    if (err != default!) {
                        tΔ2.Errorf("WriteFile = %v; want success"u8, err);
                        var (bΔ1, errΔ1) = ReadFile(filename);
                        tΔ2.Errorf("ReadFile: %v"u8, errΔ1);
                        tΔ2.Errorf("file contents: %q"u8, bΔ1);
                    }
                } else {
                    if (err == default!) {
                        tΔ2.Errorf("WriteFile(%q) = <nil>; want failure"u8, filename);
                        var (st, errΔ2) = Stat(filename);
                        if (errΔ2 == default!) {
                            tΔ2.Errorf("mode: %s"u8, st.Mode());
                        }
                        (var bΔ2, errΔ2) = ReadFile(filename);
                        tΔ2.Errorf("ReadFile: %v"u8, errΔ2);
                        tΔ2.Errorf("file contents: %q"u8, bΔ2);
                    }
                }
            });
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doesNotExistˢ = "does_not_exist"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string linkᶜ = "link"u8;

public static void TestOpenFileCreateExclDanglingSymlink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    testMaybeRooted(Ꮡt, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        @string link = linkᶜ;
        {
            var errΔ1 = Symlink(doesNotExistˢ, link); if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
        }
        ж<Δos.File> f = default!;
        error err = default!;
        if (r == nil){
            (f, err) = OpenFile(link, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_EXCL), 292);
        } else {
            (f, err) = r.OpenFile(link, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_EXCL), 292);
        }
        if (err == default!) {
            f.Close();
        }
        if (!errors.Is(err, ErrExist)) {
            tΔ1.Errorf("OpenFile of a dangling symlink with O_CREATE|O_EXCL = %v, want ErrExist"u8, err);
        }
        {
            var (_, errΔ1) = Stat(link); if (errΔ1 == default!) {
                tΔ1.Errorf("OpenFile of a dangling symlink with O_CREATE|O_EXCL created a file"u8);
            }
        }
    });
}

[GoType("dyn")] internal partial struct TestFileRDWRFlags_type {
    internal @string name;
    internal nint flag;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ1 = "f"u8;

// TestFileRDWRFlags tests the O_RDONLY, O_WRONLY, and O_RDWR flags.
public static void TestFileRDWRFlags(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestFileRDWRFlags_type[]{
        new("O_RDONLY"u8, O_RDONLY),
        new("O_WRONLY"u8, O_WRONLY),
        new("O_RDWR"u8, O_RDWR)
    }.slice()) {
        ref var test = ref heap(new TestFileRDWRFlags_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<Δtesting.T> tΔ1) => {
            var testʗ2 = testʗ1;
            testMaybeRooted(tΔ1, (ж<Δtesting.T> tΔ2, ж<Δos.Root> r) => {
                GoFrame ᒐ = default;
                try {
                    @string filename = filenameᶜ1;
                    var content = slice<byte>("content"u8);
                    {
                        var errΔ1 = WriteFile(filename, content, 438); if (errΔ1 != default!) {
                            tΔ2.Fatal(errΔ1);
                        }
                    }
                    ж<Δos.File> f = default!;
                    error err = default!;
                    if (r == nil){
                        (f, err) = OpenFile(filename, testʗ2.flag, 0);
                    } else {
                        (f, err) = r.OpenFile(filename, testʗ2.flag, 0);
                    }
                    if (err != default!) {
                        tΔ2.Fatal(err);
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    (var got, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    if (testʗ2.flag == O_WRONLY){
                        if (err == default!) {
                            tΔ2.Errorf("read file: %q, %v; want error"u8, got, err);
                        }
                    } else {
                        if (err != default! || !bytes.Equal(got, content)) {
                            tΔ2.Errorf("read file: %q, %v; want %q, <nil>"u8, got, err, content);
                        }
                    }
                    {
                        var (_, errΔ1) = f.Seek(0, 0); if (errΔ1 != default!) {
                            tΔ2.Fatalf("f.Seek: %v"u8, errΔ1);
                        }
                    }
                    var newcontent = slice<byte>("CONTENT"u8);
                    (_, err) = f.Write(newcontent);
                    if (testʗ2.flag == O_RDONLY){
                        if (err == default!) {
                            tΔ2.Errorf("write file: succeeded, want error"u8);
                        }
                    } else {
                        if (err != default!) {
                            tΔ2.Errorf("write file: %v, want success"u8, err);
                        }
                    }
                    f.Close();
                    (got, err) = ReadFile(filename);
                    if (err != default!) {
                        tΔ2.Fatal(err);
                    }
                    var want = content;
                    if (testʗ2.flag != O_RDONLY) {
                        want = newcontent;
                    }
                    if (!bytes.Equal(got, want)) {
                        tΔ2.Fatalf("after write, file contains %q, want %q"u8, got, want);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        });
    }
}

public static void TestStatDirWithTrailingSlash(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    // Create new temporary directory and arrange to clean it up.
    @string path = Ꮡt.TempDir();
    // Stat of path should succeed.
    {
        var (_, err) = Stat(path); if (err != default!) {
            Ꮡt.Fatalf("stat %s failed: %s"u8, path, err);
        }
    }
    // Stat of path+"/" should succeed too.
    path += "/"u8;
    {
        var (_, err) = Stat(path); if (err != default!) {
            Ꮡt.Fatalf("stat %s failed: %s"u8, path, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nilˢ = (@string)"<nil>"u8;

public static void TestNilProcessStateString(ж<Δtesting.T> Ꮡt) {
    ж<Δos.ProcessState> ps = default!;
    @string s = ps.String();
    if (s != "<nil>"u8) {
        Ꮡt.Errorf("(*ProcessState)(nil).String() = %q, want %q"u8, s, nilˢ);
    }
}

public static void TestSameFile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    var (fa, err) = Create("a"u8);
    if (err != default!) {
        Ꮡt.Fatalf("Create(a): %v"u8, err);
    }
    fa.Close();
    (var fb, err) = Create("b"u8);
    if (err != default!) {
        Ꮡt.Fatalf("Create(b): %v"u8, err);
    }
    fb.Close();
    (var ia1, err) = Stat("a"u8);
    if (err != default!) {
        Ꮡt.Fatalf("Stat(a): %v"u8, err);
    }
    (var ia2, err) = Stat("a"u8);
    if (err != default!) {
        Ꮡt.Fatalf("Stat(a): %v"u8, err);
    }
    if (!SameFile(ia1, ia2)) {
        Ꮡt.Errorf("files should be same"u8);
    }
    (var ib, err) = Stat("b"u8);
    if (err != default!) {
        Ꮡt.Fatalf("Stat(b): %v"u8, err);
    }
    if (SameFile(ia1, ib)) {
        Ꮡt.Errorf("files should be different"u8);
    }
}

internal static void testDevNullFileInfo(ж<Δtesting.T> Ꮡt, @string statname, @string devNullName, fs.FileInfo fi) {
    @string pre = fmt.Sprintf("%s(%q): "u8, statname, devNullName);
    if (fi.Size() != 0) {
        Ꮡt.Errorf(pre + "wrong file size have %d want 0"u8, fi.Size());
    }
    if ((fs.FileMode)(fi.Mode() & ModeDevice) == 0) {
        Ꮡt.Errorf(pre + "wrong file mode %q: ModeDevice is not set"u8, fi.Mode());
    }
    if ((fs.FileMode)(fi.Mode() & ModeCharDevice) == 0) {
        Ꮡt.Errorf(pre + "wrong file mode %q: ModeCharDevice is not set"u8, fi.Mode());
    }
    if (fi.Mode().IsRegular()) {
        Ꮡt.Errorf(pre + "wrong file mode %q: IsRegular returns true"u8, fi.Mode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fStatˢ = "f.Stat"u8;
internal static readonly @string statˢ = "Stat"u8;

internal static void testDevNullFile(ж<Δtesting.T> Ꮡt, @string devNullName) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Open(devNullName);
        if (err != default!) {
            Ꮡt.Fatalf("Open(%s): %v"u8, devNullName, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var fi, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatalf("Stat(%s): %v"u8, devNullName, err);
        }
        testDevNullFileInfo(Ꮡt, fStatˢ, devNullName, fi);
        (fi, err) = Stat(devNullName);
        if (err != default!) {
            Ꮡt.Fatalf("Stat(%s): %v"u8, devNullName, err);
        }
        testDevNullFileInfo(Ꮡt, statˢ, devNullName, fi);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nulˢ = "./nul"u8;
internal static readonly @string nulˢ2 = "//./nul"u8;

public static void TestDevNullFile(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    testDevNullFile(Ꮡt, DevNull);
    if (Δruntime.GOOS == "windows"u8) {
        testDevNullFile(Ꮡt, nulˢ);
        testDevNullFile(Ꮡt, nulˢ2);
    }
}

internal static ж<bool> testLargeWrite = flag.Bool("large_write"u8, false, "run TestLargeWriteToConsole test that floods console with output"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingConsoleFloodingˢ = (@string)"skipping console-flooding test; enable with -large_write"u8;

public static void TestLargeWriteToConsole(ж<Δtesting.T> Ꮡt) {
    if (!testLargeWrite.Value) {
        Ꮡt.Skip(skippingConsoleFloodingˢ);
    }
    var b = new slice<byte>(32000);
    foreach (var (i, _) in b) {
        b[i] = (rune)'.';
    }
    b[len(b) - 1] = (rune)'\n';
    var (n, err) = Stdout.Write(b);
    if (err != default!) {
        Ꮡt.Fatalf("Write to os.Stdout failed: %v"u8, err);
    }
    if (n != len(b)) {
        Ꮡt.Errorf("Write to os.Stdout should return %d; got %d"u8, len(b), n);
    }
    (n, err) = Stderr.Write(b);
    if (err != default!) {
        Ꮡt.Fatalf("Write to os.Stderr failed: %v"u8, err);
    }
    if (n != len(b)) {
        Ꮡt.Errorf("Write to os.Stderr should return %d; got %d"u8, len(b), n);
    }
}

public static void TestStatDirModeExec(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skip("Chmod is not supported on " + Δruntime.GOOS);
    }
    Ꮡt.Parallel();
    UntypedInt mode = /* 0111 */ 73;
    @string path = Ꮡt.TempDir();
    {
        var errΔ1 = Chmod(path, 511); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Chmod %q 0777: %v"u8, path, errΔ1);
        }
    }
    var (dir, err) = Stat(path);
    if (err != default!) {
        Ꮡt.Fatalf("Stat %q (looking for mode %#o): %s"u8, path, (nint)(mode), err);
    }
    if ((fs.FileMode)(dir.Mode() & (uint32)mode) != mode) {
        Ꮡt.Errorf("Stat %q: mode %#o want %#o"u8, path, (fs.FileMode)(dir.Mode() & (uint32)mode), (nint)(mode));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
internal static readonly @string testRunTestStatStdinˢ = "-test.run=^TestStatStdin$"u8;
internal static readonly @string outputˢ = "output"u8;

public static void TestStatStdin(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8) {
        Ꮡt.Skipf("%s doesn't have /bin/sh"u8, Δruntime.GOOS);
    }

    if (Getenv(goWantHelperProcessˢ) == "1"u8) {
        var (st, errΔ1) = Stdin.Stat();
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("Stat failed: %v"u8, errΔ1);
        }
        fmt.Println((fs.FileMode)(st.Mode() & ModeNamedPipe));
        Exit(0);
    }
    Ꮡt.Parallel();
    @string exe = testenv.Executable(new os_test_package.testing_TжTB(Ꮡt));
    var (fi, err) = Stdin.Stat();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var mode = fi.Mode();
        switch (ᐧ) {
        case {} when (fs.FileMode)(mode & ModeCharDevice) != 0 && (fs.FileMode)(mode & ModeDevice) != 0: {
            break;
        }
        case {} when (fs.FileMode)(mode & ModeNamedPipe) != 0: {
            break;
        }
        default: {
            Ꮡt.Fatalf("unexpected Stdin mode (%v), want ModeCharDevice or ModeNamedPipe"u8, mode);
            break;
        }}
    }

    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), exe, testRunTestStatStdinˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    // This will make standard input a pipe.
    cmd.Value.Stdin = new os_test_package.strings_ReaderжReader(strings.NewReader(outputˢ));
    (var output, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("Failed to spawn child process: %v %q"u8, err, ((@string)output));
    }
    // result will be like "prw-rw-rw"
    if (len(output) < 1 || output[0] != (rune)'p') {
        Ꮡt.Fatalf("Child process reports stdin is not pipe '%v'"u8, ((@string)output));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string targetˢ = "target"u8;
internal static readonly object statDoesnTFollowRelativeˢ = (@string)"Stat doesn't follow relative symlink"u8;

public static void TestStatRelativeSymlink(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
        Ꮡt.Parallel();
        @string tmpdir = Ꮡt.TempDir();
        @string target = filepath.Join(tmpdir, targetˢ);
        var (f, err) = Create(target);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var st, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string link = filepath.Join(tmpdir, linkˢ);
        err = Symlink(filepath.Base(target), link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var st1, err) = Stat(link);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!SameFile(st, st1)) {
            Ꮡt.Error(statDoesnTFollowRelativeˢ);
        }
        if (Δruntime.GOOS == "windows"u8) {
            Remove(link);
            err = Symlink(target.slice(len(filepath.VolumeName(target))), link);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var (st1Δ1, errΔ1) = Stat(link);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (!SameFile(st, st1Δ1)) {
                Ꮡt.Error(statDoesnTFollowRelativeˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReadAtEOF(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var f = newFile(Ꮡt);
    var (_, err) = f.ReadAt(new slice<byte>(10), 0);
    var exprᴛ1 = err;
    if (AreEqual(exprᴛ1, Δio.EOF)) {
    }
    else if (AreEqual(exprᴛ1, default!)) {
        Ꮡt.Fatalf("ReadAt succeeded"u8);
    }
    else { /* default: */
        Ꮡt.Fatalf("ReadAt failed: %s"u8, // all good
 err);
    }

}

public static void TestLongPath(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpdir = Ꮡt.TempDir();
    // Test the boundary of 247 and fewer bytes (normal) and 248 and more bytes (adjusted).
    var sizes = new nint[]{247, 248, 249, 400}.slice();
    while (len(tmpdir) < 400) {
        tmpdir += "/dir3456789"u8;
    }
    foreach (var (_, sz) in sizes) {
        Ꮡt.Run(fmt.Sprintf("length=%d"u8, sz), (ж<Δtesting.T> tΔ1) => {
            @string sizedTempDir = tmpdir.slice(0, sz - 1) + "x"; // Ensure it does not end with a slash.
            // The various sized runs are for this call to trigger the boundary
            // condition.
            {
                var err = MkdirAll(sizedTempDir, 493); if (err != default!) {
                    tΔ1.Fatalf("MkdirAll failed: %v"u8, err);
                }
            }
            var data = slice<byte>("hello world\n"u8);
            {
                var err = WriteFile(sizedTempDir + "/foo.txt"u8, data, 420); if (err != default!) {
                    tΔ1.Fatalf("os.WriteFile() failed: %v"u8, err);
                }
            }
            {
                var err = Rename(sizedTempDir + "/foo.txt"u8, sizedTempDir + "/bar.txt"u8); if (err != default!) {
                    tΔ1.Fatalf("Rename failed: %v"u8, err);
                }
            }
            var mtime = time.Now().Truncate(time.ΔMinute);
            {
                var err = Chtimes(sizedTempDir + "/bar.txt"u8, mtime, mtime); if (err != default!) {
                    tΔ1.Fatalf("Chtimes failed: %v"u8, err);
                }
            }
            var names = new @string[]{"bar.txt"u8}.slice();
            if (testenv.HasSymlink()) {
                {
                    var err = Symlink(sizedTempDir + "/bar.txt"u8, sizedTempDir + "/symlink.txt"u8); if (err != default!) {
                        tΔ1.Fatalf("Symlink failed: %v"u8, err);
                    }
                }
                names = append(names, "symlink.txt"u8);
            }
            if (testenv.HasLink()) {
                {
                    var err = Link(sizedTempDir + "/bar.txt"u8, sizedTempDir + "/link.txt"u8); if (err != default!) {
                        tΔ1.Fatalf("Link failed: %v"u8, err);
                    }
                }
                names = append(names, "link.txt"u8);
            }
            foreach (var (_, wantSize) in new int64[]{(int64)len(data), 0}.slice()) {
                foreach (var (_, name) in names) {
                    @string path = sizedTempDir + "/"u8 + name;
                    var (dir, err) = Stat(path);
                    if (err != default!) {
                        tΔ1.Fatalf("Stat(%q) failed: %v"u8, path, err);
                    }
                    var filesize = size(path, tΔ1);
                    if (dir.Size() != filesize || filesize != wantSize) {
                        tΔ1.Errorf("Size(%q) is %d, len(ReadFile()) is %d, want %d"u8, path, dir.Size(), filesize, wantSize);
                    }
                    if (Δruntime.GOOS != "wasip1"u8) {
                        // Chmod is not supported on wasip1
                        err = Chmod(path, dir.Mode());
                        if (err != default!) {
                            tΔ1.Fatalf("Chmod(%q) failed: %v"u8, path, err);
                        }
                    }
                }
                {
                    var err = Truncate(sizedTempDir + "/bar.txt"u8, 0); if (err != default!) {
                        tΔ1.Fatalf("Truncate failed: %v"u8, err);
                    }
                }
            }
        });
    }
}

internal static void testKillProcess(ж<Δtesting.T> Ꮡt, Action<ж<Δos.Process>> processKiller) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        // Re-exec the test binary to start a process that hangs until stdin is closed.
        var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), testenv.Executable(new os_test_package.testing_TжTB(Ꮡt)));
        cmd.Value.Env = append(cmd.Environ(), "GO_OS_TEST_DRAIN_STDIN=1"u8);
        var (stdout, err) = cmd.StdoutPipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var stdin, err) = cmd.StdinPipe();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = cmd.Start();
        if (err != default!) {
            Ꮡt.Fatalf("Failed to start test process: %v"u8, err);
        }
        var cmdʗ1 = cmd;
        var stdinʗ1 = stdin;
        defer(() => {
            {
                var errΔ1 = cmdʗ1.Wait(); if (errΔ1 == default!) {
                    Ꮡt.Errorf("Test process succeeded, but expected to fail"u8);
                }
            }
            stdinʗ1.Close(); // Keep stdin alive until the process has finished dying.
        }, ref ᒐ);
        // Wait for the process to be started.
        // (It will close its stdout when it reaches TestMain.)
        Δio.Copy(Δio.Discard, stdout);
        processKiller((~cmd).Process);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestKillStartProcess(ж<Δtesting.T> Ꮡt) {
    testKillProcess(Ꮡt, (ж<Δos.Process> p) => {
        var err = p.Kill();
        if (err != default!) {
            Ꮡt.Fatalf("Failed to kill test process: %v"u8, err);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunTestGetppidˢ = "-test.run=^TestGetppid$"u8;

public static void TestGetppid(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "plan9"u8) {
        // TODO: golang.org/issue/8206
        Ꮡt.Skipf("skipping test on plan9; see issue 8206"u8);
    }
    if (Getenv(goWantHelperProcessˢ) == "1"u8) {
        fmt.Print(Getppid());
        Exit(0);
    }
    Ꮡt.Parallel();
    var cmd = testenv.Command(new os_test_package.testing_TжTB(Ꮡt), testenv.Executable(new os_test_package.testing_TжTB(Ꮡt)), testRunTestGetppidˢ);
    cmd.Value.Env = append(Environ(), "GO_WANT_HELPER_PROCESS=1"u8);
    // verify that Getppid() from the forked process reports our process id
    var (output, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("Failed to spawn child process: %v %q"u8, err, ((@string)output));
    }
    @string childPpid = ((@string)output);
    @string ourPid = fmt.Sprintf("%d"u8, Getpid());
    if (childPpid != ourPid) {
        Ꮡt.Fatalf("Child process reports parent process id '%v', expected '%v'"u8, childPpid, ourPid);
    }
}

public static void TestKillFindProcess(ж<Δtesting.T> Ꮡt) {
    testKillProcess(Ꮡt, (ж<Δos.Process> p) => {
        var (p2, err) = FindProcess((~p).Pid);
        if (err != default!) {
            Ꮡt.Fatalf("Failed to find test process: %v"u8, err);
        }
        err = p2.Kill();
        if (err != default!) {
            Ꮡt.Fatalf("Failed to kill test process: %v"u8, err);
        }
    });
}


[GoType("dyn")] partial struct nilFileMethodTestsᴛ1 {
    internal @string name;
    internal Func<ж<Δos.File>, error> f;
}
internal static slice<nilFileMethodTestsᴛ1> nilFileMethodTests = new nilFileMethodTestsᴛ1[]{
    new("Chdir"u8, (ж<Δos.File> f) => f.Chdir()),
    new("Close"u8, (ж<Δos.File> f) => f.Close()),
    new("Chmod"u8, (ж<Δos.File> f) => f.Chmod(0)),
    new("Chown"u8, (ж<Δos.File> f) => f.Chown(0, 0)),
    new("Read"u8, (ж<Δos.File> f) => {
        var (_, err) = f.Read(new slice<byte>(0));
        return err;
    }),
    new("ReadAt"u8, (ж<Δos.File> f) => {
        var (_, err) = f.ReadAt(new slice<byte>(0), 0);
        return err;
    }),
    new("Readdir"u8, (ж<Δos.File> f) => {
        var (_, err) = f.Readdir(1);
        return err;
    }),
    new("Readdirnames"u8, (ж<Δos.File> f) => {
        var (_, err) = f.Readdirnames(1);
        return err;
    }),
    new("Seek"u8, (ж<Δos.File> f) => {
        var (_, err) = f.Seek(0, Δio.SeekStart);
        return err;
    }),
    new("Stat"u8, (ж<Δos.File> f) => {
        var (_, err) = f.Stat();
        return err;
    }),
    new("Sync"u8, (ж<Δos.File> f) => f.Sync()),
    new("Truncate"u8, (ж<Δos.File> f) => f.Truncate(0)),
    new("Write"u8, (ж<Δos.File> f) => {
        var (_, err) = f.Write(new slice<byte>(0));
        return err;
    }),
    new("WriteAt"u8, (ж<Δos.File> f) => {
        var (_, err) = f.WriteAt(new slice<byte>(0), 0);
        return err;
    }),
    new("WriteString"u8, (ж<Δos.File> f) => {
        var (_, err) = f.WriteString(""u8);
        return err;
    })
}.slice();

// Test that all File methods give ErrInvalid if the receiver is nil.
public static void TestNilFileMethods(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, tt) in nilFileMethodTests) {
        ж<Δos.File> @file = default!;
        var got = tt.f(@file);
        if (!AreEqual(got, ErrInvalid)) {
            Ꮡt.Errorf("%v should fail when f is nil; got %v"u8, tt.name, got);
        }
    }
}

internal static void mkdirTree(ж<Δtesting.T> Ꮡt, @string root, nint level, nint max) {
    if (level >= max) {
        return;
    }
    level++;
    for (var i = (rune)'a'; i < (rune)'c'; i++) {
        @string dir = filepath.Join(root, ((@string)i));
        {
            var err = Mkdir(dir, 448); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        mkdirTree(Ꮡt, dir, level, max);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnWindowsˢ = (@string)"skipping on windows"u8;

// Test that simultaneous RemoveAll do not report an error.
// As long as it gets removed, we should be happy.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestRemoveAllRace(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.GOOS == "windows"u8) {
            // Windows has very strict rules about things like
            // removing directories while someone else has
            // them open. The racing doesn't work out nicely
            // like it does on Unix.
            Ꮡt.Skip(skippingOnWindowsˢ);
        }
        if (Δruntime.GOOS == "dragonfly"u8) {
            testenv.SkipFlaky(new os_test_package.testing_TжTB(Ꮡt), 52301);
        }
        nint n = Δruntime.GOMAXPROCS(16);
        defer(Δruntime.GOMAXPROCS, n, ref ᒐ);
        @string root = Ꮡt.TempDir();
        mkdirTree(Ꮡt, root, 1, 6);
        var hold = new channel<EmptyStruct>(0);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        for (nint i = 0; i < 4; i++) {
            Ꮡwg.Add(1);
            var holdʗ1 = hold;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    ᐸꟷ(holdʗ1);
                    var err = RemoveAll(root);
                    if (err != default!) {
                        Ꮡt.Errorf("unexpected error: %T, %q"u8, err, err);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        close(hold); // let workers race to remove root
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnAixIssue70131ˢ = (@string)"skipping on aix; issue 70131"u8;
internal static readonly object skippingOnSolarisAndˢ = (@string)"skipping on Solaris and illumos; issue 19111"u8;
internal static readonly object skippingOnWindowsIssueˢ = (@string)"skipping on Windows; issue 19098"u8;
internal static readonly object skippingOnPlan9DoesNotˢ = (@string)"skipping on Plan 9; does not support runtime poller"u8;
internal static readonly object skippingOnJsNoSupportForˢ = (@string)"skipping on js; no support for os.Pipe"u8;
internal static readonly object skippingOnWasip1Noˢ = (@string)"skipping on wasip1; no support for os.Pipe"u8;

// Test that reading from a pipe doesn't use up a thread.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPipeThreads(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "aix"u8) {
            Ꮡt.Skip(skippingOnAixIssue70131ˢ);
        }
        else if (exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8) {
            Ꮡt.Skip(skippingOnSolarisAndˢ);
        }
        else if (exprᴛ1 == "windows"u8) {
            Ꮡt.Skip(skippingOnWindowsIssueˢ);
        }
        else if (exprᴛ1 == "plan9"u8) {
            Ꮡt.Skip(skippingOnPlan9DoesNotˢ);
        }
        else if (exprᴛ1 == "js"u8) {
            Ꮡt.Skip(skippingOnJsNoSupportForˢ);
        }
        else if (exprᴛ1 == "wasip1"u8) {
            Ꮡt.Skip(skippingOnWasip1Noˢ);
        }

        nint threads = 100;
        var r = new slice<ж<Δos.File>>(threads);
        var w = new slice<ж<Δos.File>>(threads);
        for (nint i = 0; i < threads; i++) {
            var (rp, wp, err) = Pipe();
            if (err != default!) {
                for (nint j = 0; j < i; j++) {
                    r[j].Close();
                    w[j].Close();
                }
                Ꮡt.Fatal(err);
            }
            r[i] = rp;
            w[i] = wp;
        }
        defer(debug.SetMaxThreads, debug.SetMaxThreads(threads / 2), ref ᒐ);
        var creading = new channel<bool>(threads);
        var cdone = new channel<bool>(threads);
        for (nint i = 0; i < threads; i++) {
            var cdoneʗ1 = cdone;
            var creadingʗ1 = creading;
            var rʗ1 = r;
            goǃ((nint iΔ1) => {
                ref var b = ref heap(new array<byte>(1), out var Ꮡb);
                creadingʗ1.ᐸꟷ(true);
                {
                    var (_, err) = rʗ1[iΔ1].Read(b[..]); if (err != default!) {
                        Ꮡt.Error(err);
                    }
                }
                {
                    var err = rʗ1[iΔ1].Close(); if (err != default!) {
                        Ꮡt.Error(err);
                    }
                }
                cdoneʗ1.ᐸꟷ(true);
            }, i);
        }
        for (nint i = 0; i < threads; i++) {
            ᐸꟷ(creading);
        }
        // If we are still alive, it means that the 100 goroutines did
        // not require 100 threads.
        for (nint i = 0; i < threads; i++) {
            {
                var (_, err) = w[i].Write(new byte[]{0}.slice()); if (err != default!) {
                    Ꮡt.Error(err);
                }
            }
            {
                var err = w[i].Close(); if (err != default!) {
                    Ꮡt.Error(err);
                }
            }
            ᐸꟷ(cdone);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object secondCloseDidNotFailˢ = (@string)"second Close did not fail"u8;

internal static Action<ж<Δtesting.T>> testDoubleCloseError(@string path) {
    return (ж<Δtesting.T> t) => {
        t.Parallel();
        var (@file, err) = Open(path);
        if (err != default!) {
            t.Fatal(err);
        }
        {
            var errΔ1 = @file.Close(); if (errΔ1 != default!) {
                t.Fatalf("unexpected error from Close: %v"u8, errΔ1);
            }
        }
        {
            var errΔ2 = @file.Close(); if (errΔ2 == default!){
                t.Error(secondCloseDidNotFailˢ);
            } else 
            {
                var (pe, ok) = errΔ2._<ж<fs.PathError>>(ᐧ); if (!ok){
                    t.Errorf("second Close: got %T, want %T"u8, errΔ2, pe.OrTypedNil());
                } else 
                if (!AreEqual((~pe).Err, ErrClosed)){
                    t.Errorf("second Close: got %q, want %q"u8, (~pe).Err, ErrClosed);
                } else {
                    t.Logf("second close returned expected error %q"u8, errΔ2);
                }
            }
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileˢ2 = "file"u8;
internal static readonly @string dirˢ = "dir"u8;

public static void TestDoubleCloseError(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    Ꮡt.Run(fileˢ2, testDoubleCloseError(filepath.Join(sfdir, sfname)));
    Ꮡt.Run(dirˢ, testDoubleCloseError(sfdir));
}

public static void TestUserCacheDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (dir, err) = UserCacheDir();
    if (err != default!) {
        Ꮡt.Skipf("skipping: %v"u8, err);
    }
    if (dir == ""u8) {
        Ꮡt.Fatalf("UserCacheDir returned %q; want non-empty path or error"u8, dir);
    }
    (var fi, err) = Stat(dir);
    if (err != default!) {
        if (IsNotExist(err)) {
            Ꮡt.Log(err);
            return;
        }
        Ꮡt.Fatal(err);
    }
    if (!fi.IsDir()) {
        Ꮡt.Fatalf("dir %s is not directory; type = %v"u8, dir, fi.Mode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object xdgCacheHomeIsEffectiveˢ = (@string)"$XDG_CACHE_HOME is effective only on Unix systems"u8;
internal static readonly @string xdgCacheHomeˢ = "XDG_CACHE_HOME"u8;
internal static readonly @string someDirˢ = "some-dir"u8;
internal static readonly object userCacheDirSucceededˢ = (@string)"UserCacheDir succeeded though $XDG_CACHE_HOME contains a relative path"u8;

public static void TestUserCacheDirXDGConfigDirEnvVar(ж<Δtesting.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "plan9"u8) {
        Ꮡt.Skip(xdgCacheHomeIsEffectiveˢ);
    }

    var (wd, err) = Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Setenv(xdgCacheHomeˢ, wd);
    (var dir, err) = UserCacheDir();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (dir != wd) {
        Ꮡt.Fatalf("UserCacheDir returned %q; want the value of $XDG_CACHE_HOME %q"u8, dir, wd);
    }
    Ꮡt.Setenv(xdgCacheHomeˢ, someDirˢ);
    (_, err) = UserCacheDir();
    if (err == default!) {
        Ꮡt.Fatal(userCacheDirSucceededˢ);
    }
}

public static void TestUserConfigDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (dir, err) = UserConfigDir();
    if (err != default!) {
        Ꮡt.Skipf("skipping: %v"u8, err);
    }
    if (dir == ""u8) {
        Ꮡt.Fatalf("UserConfigDir returned %q; want non-empty path or error"u8, dir);
    }
    (var fi, err) = Stat(dir);
    if (err != default!) {
        if (IsNotExist(err)) {
            Ꮡt.Log(err);
            return;
        }
        Ꮡt.Fatal(err);
    }
    if (!fi.IsDir()) {
        Ꮡt.Fatalf("dir %s is not directory; type = %v"u8, dir, fi.Mode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object xdgConfigHomeIsEffectiveˢ = (@string)"$XDG_CONFIG_HOME is effective only on Unix systems"u8;
internal static readonly @string xdgConfigHomeˢ = "XDG_CONFIG_HOME"u8;
internal static readonly object userConfigDirSucceededˢ = (@string)"UserConfigDir succeeded though $XDG_CONFIG_HOME contains a relative path"u8;

public static void TestUserConfigDirXDGConfigDirEnvVar(ж<Δtesting.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "plan9"u8) {
        Ꮡt.Skip(xdgConfigHomeIsEffectiveˢ);
    }

    var (wd, err) = Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Setenv(xdgConfigHomeˢ, wd);
    (var dir, err) = UserConfigDir();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (dir != wd) {
        Ꮡt.Fatalf("UserConfigDir returned %q; want the value of $XDG_CONFIG_HOME %q"u8, dir, wd);
    }
    Ꮡt.Setenv(xdgConfigHomeˢ, someDirˢ);
    (_, err) = UserConfigDir();
    if (err == default!) {
        Ꮡt.Fatal(userConfigDirSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userHomeDirReturnedAnˢ = (@string)"UserHomeDir returned an empty string but no error"u8;

public static void TestUserHomeDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (dir, err) = UserHomeDir();
    if (dir == ""u8 && err == default!) {
        Ꮡt.Fatal(userHomeDirReturnedAnˢ);
    }
    if (err != default!) {
        // UserHomeDir may return a non-nil error if the environment variable
        // for the home directory is empty or unset in the environment.
        Ꮡt.Skipf("skipping: %v"u8, err);
    }
    (var fi, err) = Stat(dir);
    if (err != default!) {
        if (IsNotExist(err)) {
            // The user's home directory has a well-defined location, but does not
            // exist. (Maybe nothing has written to it yet? That could happen, for
            // example, on minimal VM images used for CI testing.)
            Ꮡt.Log(err);
            return;
        }
        Ꮡt.Fatal(err);
    }
    if (!fi.IsDir()) {
        Ꮡt.Fatalf("dir %s is not directory; type = %v"u8, dir, fi.Mode());
    }
}

public static void TestDirSeek(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (wd, err) = Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var f, err) = Open(wd);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var dirnames1, err) = f.Readdirnames(0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var ret, err) = f.Seek(0, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (ret != 0) {
        Ꮡt.Fatalf("seek result not zero: %d"u8, ret);
    }
    (var dirnames2, err) = f.Readdirnames(0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(dirnames1) != len(dirnames2)) {
        Ꮡt.Fatalf("listings have different lengths: %d and %d\n"u8, len(dirnames1), len(dirnames2));
    }
    foreach (var (i, n1) in dirnames1) {
        @string n2 = dirnames2[i];
        if (n1 != n2) {
            Ꮡt.Fatalf("different name i=%d n1=%s n2=%s\n"u8, i, n1, n2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string issue37161ˢ = "issue37161"u8;

public static void TestReaddirSmallSeek(ж<Δtesting.T> Ꮡt) {
    // See issue 37161. Read only one entry from a directory,
    // seek to the beginning, and read again. We should not see
    // duplicate entries.
    Ꮡt.Parallel();
    var (wd, err) = Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var df, err) = Open(filepath.Join(wd, testdataˢ, issue37161ˢ));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var names1, err) = df.Readdirnames(1);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        (_, err) = df.Seek(0, 0); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    (var names2, err) = df.Readdirnames(0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(names2) != 3) {
        Ꮡt.Fatalf("first names: %v, second names: %v"u8, names1, names2);
    }
}

// isDeadlineExceeded reports whether err is or wraps ErrDeadlineExceeded.
// We also check that the error has a Timeout method that returns true.
internal static bool isDeadlineExceeded(error err) {
    if (!IsTimeout(err)) {
        return false;
    }
    if (!errors.Is(err, ErrDeadlineExceeded)) {
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openFileˢ = "OpenFile"u8;
internal static readonly @string rootOpenFileˢ = "RootOpenFile"u8;

// Test that opening a file does not change its permissions.  Issue 38225.
public static void TestOpenFileKeepsPermissions(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Run(openFileˢ, (ж<Δtesting.T> tΔ1) => {
        testOpenFileKeepsPermissions(tΔ1, OpenFile);
    });
    Ꮡt.Run(rootOpenFileˢ, (ж<Δtesting.T> tΔ2) => {
        testOpenFileKeepsPermissions(tΔ2, (@string name, nint flag, fs.FileMode perm) => {
            GoFrame ᒐ = default;
            try {
                var (dir, @file) = filepath.Split(name);
                var (r, err) = OpenRoot(dir);
                if (err != default!) {
                    return (default!, err);
                }
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                return r.OpenFile(@file, flag, perm);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        });
    });
}

internal static void testOpenFileKeepsPermissions(ж<Δtesting.T> Ꮡt, Func<@string, nint, fs.FileMode, (ж<Δos.File>, error)> openf) {
    Ꮡt.Parallel();
    @string dir = Ꮡt.TempDir();
    @string name = filepath.Join(dir, "x");
    var (f, err) = Create(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = f.Close(); if (errΔ1 != default!) {
            Ꮡt.Error(errΔ1);
        }
    }
    (f, err) = openf(name, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_TRUNC), 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (fi, errΔ2) = f.Stat(); if (errΔ2 != default!){
            Ꮡt.Error(errΔ2);
        } else 
        if ((fs.FileMode)(fi.Mode() & 146) == 0) {
            Ꮡt.Errorf("f.Stat.Mode after OpenFile is %v, should be writable"u8, fi.Mode());
        }
    }
    {
        var errΔ3 = f.Close(); if (errΔ3 != default!) {
            Ꮡt.Error(errΔ3);
        }
    }
    {
        var (fi, errΔ4) = Stat(name); if (errΔ4 != default!){
            Ꮡt.Error(errΔ4);
        } else 
        if ((fs.FileMode)(fi.Mode() & 146) == 0) {
            Ꮡt.Errorf("Stat after OpenFile is %v, should be writable"u8, fi.Mode());
        }
    }
}

internal static void forceMFTUpdateOnWindows(ж<Δtesting.T> Ꮡt, @string path) {
    Ꮡt.Helper();
    if (Δruntime.GOOS != "windows"u8) {
        return;
    }
    // On Windows, we force the MFT to update by reading the actual metadata from GetFileInformationByHandle and then
    // explicitly setting that. Otherwise it might get out of sync with FindFirstFile. See golang.org/issues/42637.
    {
        var err = filepath.WalkDir(path, (@string pathΔ1, fs.DirEntry d, error errΔ1) => {
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            (var info, errΔ1) = d.Info();
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            (var stat, errΔ1) = Stat(pathΔ1); // This uses GetFileInformationByHandle internally.
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (stat.ModTime() == info.ModTime()) {
                return default!;
            }
            {
                var errΔ2 = Chtimes(pathΔ1, stat.ModTime(), stat.ModTime()); if (errΔ2 != default!) {
                    Ꮡt.Log(errΔ2); // We only log, not die, in case the test directory is not writable.
                }
            }
            return default!;
        }); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataDirfsˢ = "./testdata/dirfs"u8;

public static void TestDirFS(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    testDirFS(Ꮡt, DirFS(testdataDirfsˢ));
}

public static void TestRootDirFS(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (r, err) = OpenRoot(testdataDirfsˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    testDirFS(Ꮡt, r.FS());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirXˢ = "dir/x"u8;
internal static readonly object expectedDirFSResultToˢ = (@string)"expected DirFS result to implement fs.ReadDirFS"u8;
internal static readonly object fsReadDirOfNonexistentˢ = (@string)"fs.ReadDir of nonexistent directory succeeded"u8;
internal static readonly object fsOpenOfNonexistentFileˢ = (@string)"fs.Open of nonexistent file succeeded"u8;
internal static readonly @string testdataDirfsˢ2 = @"testdata\dirfs"u8;
internal static readonly @string nulˢ3 = @"NUL"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string nonesuchᶜ = "dir/nonesuch"u8;

internal static void testDirFS(ж<Δtesting.T> Ꮡt, fs.FS fsys) {
    forceMFTUpdateOnWindows(Ꮡt, testdataDirfsˢ);
    {
        var errΔ1 = fstest.TestFS(fsys, "a"u8, "b", dirXˢ); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var (rdfs, ok) = fsys._<fs.ReadDirFS>(ᐧ);
    if (!ok) {
        Ꮡt.Error(expectedDirFSResultToˢ);
    }
    {
        var (_, errΔ2) = rdfs.ReadDir(nonexistentˢ); if (errΔ2 == default!) {
            Ꮡt.Error(fsReadDirOfNonexistentˢ);
        }
    }
    // Test that the error message does not contain a backslash,
    // and does not contain the DirFS argument.
    @string nonesuch = nonesuchᶜ;
    var (_, err) = fsys.Open(nonesuch);
    if (err == default!){
        Ꮡt.Error(fsOpenOfNonexistentFileˢ);
    } else {
        if (!strings.Contains(err.Error(), nonesuch)) {
            Ꮡt.Errorf("error %q does not contain %q"u8, err, nonesuch);
        }
        if (strings.Contains((~err._<ж<fs.PathError>>()).Path, testdataˢ)) {
            Ꮡt.Errorf("error %q contains %q"u8, err, testdataˢ);
        }
    }
    // Test that Open does not accept backslash as separator.
    var d = DirFS("."u8);
    (_, err) = d.Open(testdataDirfsˢ2);
    if (err == default!) {
        Ꮡt.Fatalf(@"Open testdata\dirfs succeeded"u8);
    }
    // Test that Open does not open Windows device files.
    (_, err) = d.Open(nulˢ3);
    if (err == default!) {
        Ꮡt.Errorf(@"Open NUL succeeded"u8);
    }
}

public static void TestDirFSRootDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var (cwd, err) = Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    cwd = cwd.slice(len(filepath.VolumeName(cwd))); // trim volume prefix (C:) on Windows
    cwd = filepath.ToSlash(cwd); // convert \ to /
    cwd = strings.TrimPrefix(cwd, "/"u8); // trim leading /
    // Test that Open can open a path starting at /.
    var d = DirFS("/"u8);
    (var f, err) = d.Open(cwd + "/testdata/dirfs/a"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    f.Close();
}

public static void TestDirFSEmptyDir(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var d = DirFS(""u8);
    var (cwd, _) = Getwd();
    foreach (var (_, path) in new @string[]{
        "testdata/dirfs/a"u8, // not DirFS(".")

        filepath.ToSlash(cwd) + "/testdata/dirfs/a"u8
    }.slice()) {
        // not DirFS("/")
        var (_, err) = d.Open(path);
        if (err == default!) {
            Ꮡt.Fatalf(@"DirFS("""").Open(%q) succeeded"u8, path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string controlTxtˢ = "control.txt"u8;
internal static readonly @string eXperiMentTxtˢ = @"e:xperi\ment.txt"u8;

public static void TestDirFSPathsValid(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "windows"u8) {
        Ꮡt.Skipf("skipping on Windows"u8);
    }
    Ꮡt.Parallel();
    @string d = Ꮡt.TempDir();
    {
        var errΔ1 = WriteFile(filepath.Join(d, controlTxtˢ), slice<byte>(((@string)"Hello, world!"u8)), 420); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        var errΔ2 = WriteFile(filepath.Join(d, eXperiMentTxtˢ), slice<byte>(((@string)"Hello, colon and backslash!"u8)), 420); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    var fsys = DirFS(d);
    var err = fs.WalkDir(fsys, "."u8, (@string path, fs.DirEntry e, error errΔ3) => {
        if (fs.ValidPath(e.Name())){
            Ꮡt.Logf("%q ok"u8, e.Name());
        } else {
            Ꮡt.Errorf("%q INVALID"u8, e.Name());
        }
        return default!;
    });
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procSysFsPipeMaxSizeˢ = "/proc/sys/fs/pipe-max-size"u8;

public static void TestReadFileProc(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    // Linux files in /proc report 0 size,
    // but then if ReadFile reads just a single byte at offset 0,
    // the read at offset 1 returns EOF instead of more data.
    // ReadFile has a minimum read size of 512 to work around this,
    // but test explicitly that it's working.
    @string name = procSysFsPipeMaxSizeˢ;
    {
        var (_, errΔ1) = Stat(name); if (errΔ1 != default!) {
            Ꮡt.Skip(errΔ1);
        }
    }
    var (data, err) = ReadFile(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(data) == 0 || data[len(data) - 1] != (rune)'\n') {
        Ꮡt.Fatalf("read %s: not newline-terminated: %q"u8, name, data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string procSysFsPipeMaxSizeˢ2 = "proc/sys/fs/pipe-max-size"u8;

public static void TestDirFSReadFileProc(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var fsys = DirFS("/"u8);
    @string name = procSysFsPipeMaxSizeˢ2;
    {
        var (_, errΔ1) = fs.Stat(fsys, name); if (errΔ1 != default!) {
            Ꮡt.Skip();
        }
    }
    var (data, err) = fs.ReadFile(fsys, name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (len(data) == 0 || data[len(data) - 1] != (rune)'\n') {
        Ꮡt.Fatalf("read %s: not newline-terminated: %q"u8, name, data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object jsAllocatesALotDuringˢ = (@string)"js allocates a lot during File.WriteString"u8;
internal static readonly @string whiteboardTxtˢ = "whiteboard.txt"u8;
internal static readonly @string iWillNotAllocateWhenˢ = "I will not allocate when passed a string longer than 32 bytes.\n"u8;

public static void TestWriteStringAlloc(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "js"u8) {
            Ꮡt.Skip(jsAllocatesALotDuringˢ);
        }
        @string d = Ꮡt.TempDir();
        var (f, err) = Create(filepath.Join(d, whiteboardTxtˢ));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var fʗ2 = f;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            fʗ2.WriteString(iWillNotAllocateWhenˢ);
        });
        if (allocs != 0D) {
            Ꮡt.Errorf("expected 0 allocs for File.WriteString, got %v"u8, allocs);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string brokenPipeˢ = "broken pipe"u8;
internal static readonly @string pipeIsBeingClosedˢ = "pipe is being closed"u8;
internal static readonly @string hungupChannelˢ = "hungup channel"u8;

// Test that it's OK to have parallel I/O and Close on a pipe.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPipeIOCloseRace(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Skip on wasm, which doesn't have pipes.
    if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skipf("skipping on %s: no pipes"u8, Δruntime.GOOS);
    }
    Ꮡt.Parallel();
    var (r, w, err) = Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(3);
    var wʗ1 = w;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            while (ᐧ) {
                var (n, errΔ1) = wʗ1.Write(slice<byte>("hi"u8));
                if (errΔ1 != default!) {
                    // We look at error strings as the
                    // expected errors are OS-specific.
                    switch (ᐧ) {
                    case {} when (errors.Is(errΔ1, ErrClosed)) || (strings.Contains(errΔ1.Error(), brokenPipeˢ)) || (strings.Contains(errΔ1.Error(), pipeIsBeingClosedˢ)) || (strings.Contains(errΔ1.Error(), hungupChannelˢ)): {
                        break;
                    }
                    default: {
                        Ꮡt.Error(errΔ1);
                        break;
                    }}

                    // Ignore an expected error.
                    // Unexpected error.
                    return;
                }
                if (n != 2) {
                    Ꮡt.Errorf("wrote %d bytes, expected 2"u8, n);
                    return;
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var rʗ1 = r;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            while (ᐧ) {
                ref var buf = ref heap(new array<byte>(2), out var Ꮡbuf);
                var (n, errΔ2) = rʗ1.Read(buf[..]);
                if (errΔ2 != default!) {
                    if (!AreEqual(errΔ2, Δio.EOF) && !errors.Is(errΔ2, ErrClosed)) {
                        Ꮡt.Error(errΔ2);
                    }
                    return;
                }
                if (n != 2) {
                    Ꮡt.Errorf("read %d bytes, want 2"u8, n);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var rʗ2 = r;
    var wʗ2 = w;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            // Let the other goroutines start. This is just to get
            // a better test, the test will still pass if they
            // don't start.
            time.Sleep(time.Millisecond);
            {
                var errΔ3 = rʗ2.Close(); if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                }
            }
            {
                var errΔ4 = wʗ2.Close(); if (errΔ4 != default!) {
                    Ꮡt.Error(errΔ4);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    Ꮡwg.Wait();
}

// Test that it's OK to call Close concurrently on a pipe.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPipeCloseRace(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Skip on wasm, which doesn't have pipes.
    if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skipf("skipping on %s: no pipes"u8, Δruntime.GOOS);
    }
    Ꮡt.Parallel();
    var (r, w, err) = Pipe();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var c = new channel<error>(4);
    var cʗ1 = c;
    var rʗ1 = r;
    var wʗ1 = w;
    void f() {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            cʗ1.ᐸꟷ(rʗ1.Close());
            cʗ1.ᐸꟷ(wʗ1.Close());
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    Ꮡwg.Add(2);
    var fʗ1 = f;
    goǃ(fʗ1);
    var fʗ2 = f;
    goǃ(fʗ2);
    nint nils = 0;
    nint errs = 0;
    for (nint i = 0; i < 4; i++) {
        var errΔ1 = ᐸꟷ(c);
        if (errΔ1 == default!){
            nils++;
        } else {
            errs++;
        }
    }
    if (nils != 2 || errs != 2) {
        Ꮡt.Errorf("got nils %d errs %d, want 2 2"u8, nils, errs);
    }
}

public static void TestRandomLen(ж<Δtesting.T> Ꮡt) {
    foreach (var _ᴛ1 in range(5)) {
        var (dir, err) = MkdirTemp(Ꮡt.TempDir(), "*"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string @base = filepath.Base(dir);
        if (len(@base) > 10) {
            Ꮡt.Errorf("MkdirTemp returned len %d: %s"u8, len(@base), @base);
        }
    }
    foreach (var _ᴛ2 in range(5)) {
        var (f, err) = CreateTemp(Ꮡt.TempDir(), "*"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string @base = filepath.Base(f.Name());
        f.Close();
        if (len(@base) > 10) {
            Ꮡt.Errorf("CreateTemp returned len %d: %s"u8, len(@base), @base);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyFSˢ = (@string)"CopyFS:"u8;
internal static readonly object testFSˢ = (@string)"TestFS:"u8;
internal static readonly object comparingTwoDirectoriesˢ = (@string)"comparing two directories:"u8;
internal static readonly @string williamˢ = "william"u8;
internal static readonly @string carlˢ = "carl"u8;
internal static readonly @string daVinciˢ = "daVinci"u8;
internal static readonly @string einsteinˢ = "einstein"u8;
internal static readonly @string dirNewtonˢ = "dir/newton"u8;

public static void TestCopyFS(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    // Test with disk filesystem.
    forceMFTUpdateOnWindows(Ꮡt, testdataDirfsˢ);
    var fsys = DirFS(testdataDirfsˢ);
    @string tmpDir = Ꮡt.TempDir();
    {
        var err = CopyFS(tmpDir, fsys); if (err != default!) {
            Ꮡt.Fatal(copyFSˢ, err);
        }
    }
    forceMFTUpdateOnWindows(Ꮡt, tmpDir);
    var tmpFsys = DirFS(tmpDir);
    {
        var err = fstest.TestFS(tmpFsys, "a"u8, "b", dirXˢ); if (err != default!) {
            Ꮡt.Fatal(testFSˢ, err);
        }
    }
    {
        var err = verifyCopyFS(Ꮡt, fsys, tmpFsys); if (err != default!) {
            Ꮡt.Fatal(comparingTwoDirectoriesˢ, err);
        }
    }
    // Test whether CopyFS disallows copying for disk filesystem when there is any
    // existing file in the destination directory.
    {
        var err = CopyFS(tmpDir, fsys); if (!errors.Is(err, fs.ErrExist)) {
            Ꮡt.Errorf("CopyFS should have failed and returned error when there is"u8 + "any existing file in the destination directory (in disk filesystem), "u8 + "got: %v, expected any error that indicates <file exists>"u8, err);
        }
    }
    // Test with memory filesystem.
    fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["william"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Shakespeare\n"u8))),
        ["carl"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Gauss\n"u8))),
        ["daVinci"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Leonardo\n"u8))),
        ["einstein"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Albert\n"u8))),
        ["dir/newton"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Sir Isaac\n"u8)))
    });
    tmpDir = Ꮡt.TempDir();
    {
        var err = CopyFS(tmpDir, fsys); if (err != default!) {
            Ꮡt.Fatal(copyFSˢ, err);
        }
    }
    forceMFTUpdateOnWindows(Ꮡt, tmpDir);
    tmpFsys = DirFS(tmpDir);
    {
        var err = fstest.TestFS(tmpFsys, williamˢ, carlˢ, daVinciˢ, einsteinˢ, dirNewtonˢ); if (err != default!) {
            Ꮡt.Fatal(testFSˢ, err);
        }
    }
    {
        var err = verifyCopyFS(Ꮡt, fsys, tmpFsys); if (err != default!) {
            Ꮡt.Fatal(comparingTwoDirectoriesˢ, err);
        }
    }
    // Test whether CopyFS disallows copying for memory filesystem when there is any
    // existing file in the destination directory.
    {
        var err = CopyFS(tmpDir, fsys); if (!errors.Is(err, fs.ErrExist)) {
            Ꮡt.Errorf("CopyFS should have failed and returned error when there is"u8 + "any existing file in the destination directory (in memory filesystem), "u8 + "got: %v, expected any error that indicates <file exists>"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "test"u8;
internal static readonly @string tmpˢ2 = "tmp"u8;

// verifyCopyFS checks the content and permission of each file inside copied FS to ensure
// the copied files satisfy the convention stipulated in CopyFS.
internal static error verifyCopyFS(ж<Δtesting.T> Ꮡt, fs.FS originFS, fs.FS copiedFS) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string testDir = filepath.Join(Ꮡt.TempDir(), testˢ);
        // umask doesn't apply to the wasip and windows and there is no general way to get masked perm,
        // so create a dir and a file to compare the permission after umask if any
        {
            var errΔ1 = Mkdir(testDir, ModePerm); if (errΔ1 != default!) {
                return fmt.Errorf("mkdir %q failed: %v"u8, testDir, errΔ1);
            }
        }
        var (dirStat, err) = Stat(testDir);
        if (err != default!) {
            return fmt.Errorf("stat dir %q failed: %v"u8, testDir, err);
        }
        var wantDirMode = dirStat.Mode();
        (var f, err) = Create(filepath.Join(testDir, tmpˢ2));
        if (err != default!) {
            return fmt.Errorf("open %q failed: %v"u8, filepath.Join(testDir, tmpˢ2), err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var wantFileRWStat, err) = f.Stat();
        if (err != default!) {
            return fmt.Errorf("stat file %q failed: %v"u8, f.Name(), err);
        }
        var wantFileRWMode = wantFileRWStat.Mode();
        var wantFileRWStatʗ1 = wantFileRWStat;
        return fs.WalkDir(originFS, "."u8, error (@string path, fs.DirEntry d, error errΔ2) => {
            GoFrame ᒐ = default;
            try {
                if (d.IsDir()) {
                    // the dir . is not the dir created by CopyFS so skip checking its permission
                    if (d.Name() == "."u8) {
                        return default!;
                    }
                    var (dinfo, errΔ3) = fs.Stat(copiedFS, path);
                    if (errΔ3 != default!) {
                        return errΔ3;
                    }
                    if (dinfo.Mode() != wantDirMode) {
                        return fmt.Errorf("dir %q mode is %v, want %v"u8,
                            d.Name(), dinfo.Mode(), wantDirMode);
                    }
                    return default!;
                }
                (var fInfo, errΔ2) = originFS.Open(path);
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                var fInfoʗ1 = fInfo;
                defer(() => fInfoʗ1.Close(), ref ᒐ);
                (var copiedInfo, errΔ2) = copiedFS.Open(path);
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                var copiedInfoʗ1 = copiedInfo;
                defer(() => copiedInfoʗ1.Close(), ref ᒐ);
                // verify the file contents are the same
                (var data, errΔ2) = Δio.ReadAll(new os_test_package.fs_FileᴠReader(fInfo));
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                (var newData, errΔ2) = Δio.ReadAll(new os_test_package.fs_FileᴠReader(copiedInfo));
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                if (!bytes.Equal(data, newData)) {
                    return fmt.Errorf("file %q content is %s, want %s"u8, path, newData, data);
                }
                (var fStat, errΔ2) = fInfo.Stat();
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                (var copiedStat, errΔ2) = copiedInfo.Stat();
                if (errΔ2 != default!) {
                    return errΔ2;
                }
                // check whether the execute permission is inherited from original FS
                if ((fs.FileMode)((fs.FileMode)(copiedStat.Mode() & 73) & wantFileRWMode) != (fs.FileMode)((fs.FileMode)(fStat.Mode() & 73) & wantFileRWMode)) {
                    return fmt.Errorf("file %q execute mode is %v, want %v"u8,
                        path, (fs.FileMode)(copiedStat.Mode() & 73), (fs.FileMode)(fStat.Mode() & 73));
                }
                var rwMode = (fs.FileMode)(copiedStat.Mode() & ~73); // unset the executable permission from file mode
                if (rwMode != wantFileRWMode) {
                    return fmt.Errorf("file %q rw mode is %v, want %v"u8,
                        path, rwMode, wantFileRWStatʗ1.Mode());
                }
                return default!;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string copyfsOutˢ = "copyfs_out"u8;
internal static readonly @string fileOutTxtˢ = "file.out.txt"u8;
internal static readonly @string copyfsInˢ = "copyfs_in"u8;
internal static readonly @string fileInTxtˢ = "file.in.txt"u8;
internal static readonly @string inSymlinksˢ = "in_symlinks"u8;
internal static readonly @string outSymlinksˢ = "out_symlinks"u8;
internal static readonly @string fileAbsOutLinkˢ = "file.abs.out.link"u8;
internal static readonly @string fileRelOutLinkˢ = "file.rel.out.link"u8;
internal static readonly @string fileRelInLinkˢ = "file.rel.in.link"u8;
internal static readonly @string copyfsDupˢ = "copyfs_dup"u8;
internal static readonly object skipTheSubsequentTestAndˢ = (@string)"skip the subsequent test and wait for #49580"u8;
internal static readonly @string outSymlinksFileAbsOutˢ = "out_symlinks/file.abs.out.link"u8;
internal static readonly @string outSymlinksFileRelOutˢ = "out_symlinks/file.rel.out.link"u8;
internal static readonly @string inSymlinksFileRelInLinkˢ = "in_symlinks/file.rel.in.link"u8;

public static void TestCopyFSWithSymlinks(ж<Δtesting.T> Ꮡt) {
    // Test it with absolute and relative symlinks that point inside and outside the tree.
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    // Create a directory and file outside.
    @string tmpDir = Ꮡt.TempDir();
    @string outsideDir = filepath.Join(tmpDir, copyfsOutˢ);
    {
        var errΔ1 = Mkdir(outsideDir, 493); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Mkdir: %v"u8, errΔ1);
        }
    }
    @string outsideFile = filepath.Join(outsideDir, fileOutTxtˢ);
    {
        var errΔ2 = WriteFile(outsideFile, slice<byte>("Testing CopyFS outside"u8), 420); if (errΔ2 != default!) {
            Ꮡt.Fatalf("WriteFile: %v"u8, errΔ2);
        }
    }
    // Create a directory and file inside.
    @string insideDir = filepath.Join(tmpDir, copyfsInˢ);
    {
        var errΔ3 = Mkdir(insideDir, 493); if (errΔ3 != default!) {
            Ꮡt.Fatalf("Mkdir: %v"u8, errΔ3);
        }
    }
    @string insideFile = filepath.Join(insideDir, fileInTxtˢ);
    {
        var errΔ4 = WriteFile(insideFile, slice<byte>("Testing CopyFS inside"u8), 420); if (errΔ4 != default!) {
            Ꮡt.Fatalf("WriteFile: %v"u8, errΔ4);
        }
    }
    // Create directories for symlinks.
    @string linkInDir = filepath.Join(insideDir, inSymlinksˢ);
    {
        var errΔ5 = Mkdir(linkInDir, 493); if (errΔ5 != default!) {
            Ꮡt.Fatalf("Mkdir: %v"u8, errΔ5);
        }
    }
    @string linkOutDir = filepath.Join(insideDir, outSymlinksˢ);
    {
        var errΔ6 = Mkdir(linkOutDir, 493); if (errΔ6 != default!) {
            Ꮡt.Fatalf("Mkdir: %v"u8, errΔ6);
        }
    }
    // First, we create the absolute symlink pointing outside.
    @string outLinkFile = filepath.Join(linkOutDir, fileAbsOutLinkˢ);
    {
        var errΔ7 = Symlink(outsideFile, outLinkFile); if (errΔ7 != default!) {
            Ꮡt.Fatalf("Symlink: %v"u8, errΔ7);
        }
    }
    // Then, we create the relative symlink pointing outside.
    var (relOutsideFile, err) = filepath.Rel(filepath.Join(linkOutDir, "."), outsideFile);
    if (err != default!) {
        Ꮡt.Fatalf("filepath.Rel: %v"u8, err);
    }
    @string relOutLinkFile = filepath.Join(linkOutDir, fileRelOutLinkˢ);
    {
        var errΔ8 = Symlink(relOutsideFile, relOutLinkFile); if (errΔ8 != default!) {
            Ꮡt.Fatalf("Symlink: %v"u8, errΔ8);
        }
    }
    // Last, we create the relative symlink pointing inside.
    (var relInsideFile, err) = filepath.Rel(filepath.Join(linkInDir, "."), insideFile);
    if (err != default!) {
        Ꮡt.Fatalf("filepath.Rel: %v"u8, err);
    }
    @string relInLinkFile = filepath.Join(linkInDir, fileRelInLinkˢ);
    {
        var errΔ9 = Symlink(relInsideFile, relInLinkFile); if (errΔ9 != default!) {
            Ꮡt.Fatalf("Symlink: %v"u8, errΔ9);
        }
    }
    // Copy the directory tree and verify.
    forceMFTUpdateOnWindows(Ꮡt, insideDir);
    var fsys = DirFS(insideDir);
    @string tmpDupDir = filepath.Join(tmpDir, copyfsDupˢ);
    {
        var errΔ10 = Mkdir(tmpDupDir, 493); if (errΔ10 != default!) {
            Ꮡt.Fatalf("Mkdir: %v"u8, errΔ10);
        }
    }
    // TODO(panjf2000): symlinks are currently not supported, and a specific error
    // 			will be returned. Verify that error and skip the subsequent test,
    //			revisit this once #49580 is closed.
    {
        var errΔ11 = CopyFS(tmpDupDir, fsys); if (!errors.Is(errΔ11, ErrInvalid)) {
            Ꮡt.Fatalf("got %v, want ErrInvalid"u8, errΔ11);
        }
    }
    Ꮡt.Skip(skipTheSubsequentTestAndˢ);
    forceMFTUpdateOnWindows(Ꮡt, tmpDupDir);
    var tmpFsys = DirFS(tmpDupDir);
    {
        var errΔ12 = fstest.TestFS(tmpFsys, fileInTxtˢ, outSymlinksFileAbsOutˢ, outSymlinksFileRelOutˢ, inSymlinksFileRelInLinkˢ); if (errΔ12 != default!) {
            Ꮡt.Fatal(testFSˢ, errΔ12);
        }
    }
    {
        var fsysʗ1 = fsys;
        var tmpFsysʗ1 = tmpFsys;
        var errΔ13 = fs.WalkDir(fsys, "."u8, error (@string path, fs.DirEntry d, error errΔ14) => {
            if (d.IsDir()) {
                return default!;
            }
            (var fi, errΔ14) = d.Info();
            if (errΔ14 != default!) {
                return errΔ14;
            }
            if (filepath.Ext(path) == ".link"u8) {
                if ((fs.FileMode)(fi.Mode() & ModeSymlink) == 0) {
                    return errors.New("original file "u8 + path + " should be a symlink"u8);
                }
                var (tmpfi, errΔ15) = fs.Stat(tmpFsysʗ1, path);
                if (errΔ15 != default!) {
                    return errΔ15;
                }
                if ((fs.FileMode)(tmpfi.Mode() & ModeSymlink) != 0) {
                    return errors.New("copied file "u8 + path + " should not be a symlink"u8);
                }
            }
            (var data, errΔ14) = fs.ReadFile(fsysʗ1, path);
            if (errΔ14 != default!) {
                return errΔ14;
            }
            (var newData, errΔ14) = fs.ReadFile(tmpFsysʗ1, path);
            if (errΔ14 != default!) {
                return errΔ14;
            }
            if (!bytes.Equal(data, newData)) {
                return errors.New("file "u8 + path + " contents differ"u8);
            }
            @string target = default!;
            {
                @string fileName = filepath.Base(path);
                var exprᴛ1 = fileName;
                if (exprᴛ1 == "file.abs.out.link"u8 || exprᴛ1 == "file.rel.out.link"u8) {
                    target = outsideFile;
                }
                else if (exprᴛ1 == "file.rel.in.link"u8) {
                    target = insideFile;
                }
            }

            if (len(target) > 0) {
                var (targetData, errΔ16) = ReadFile(target);
                if (errΔ16 != default!) {
                    return errΔ16;
                }
                if (!bytes.Equal(targetData, newData)) {
                    return errors.New("file "u8 + path + " contents differ from target"u8);
                }
            }
            return default!;
        }); if (errΔ13 != default!) {
            Ꮡt.Fatal(comparingTwoDirectoriesˢ, errΔ13);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ2 = "hello world"u8;

public static void TestAppendDoesntOverwrite(ж<Δtesting.T> Ꮡt) {
    testMaybeRooted(Ꮡt, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        @string name = fileˢ2;
        {
            var errΔ1 = WriteFile(name, slice<byte>("hello"u8), 438); if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
        }
        ж<Δos.File> f = default!;
        error err = default!;
        if (r == nil){
            (f, err) = OpenFile(name, (nint)(O_APPEND | O_WRONLY), 0);
        } else {
            (f, err) = r.OpenFile(name, (nint)(O_APPEND | O_WRONLY), 0);
        }
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        {
            var (_, errΔ1) = f.Write(slice<byte>(" world"u8)); if (errΔ1 != default!) {
                f.Close();
                tΔ1.Fatal(errΔ1);
            }
        }
        {
            var errΔ2 = f.Close(); if (errΔ2 != default!) {
                tΔ1.Fatal(errΔ2);
            }
        }
        (var got, err) = ReadFile(name);
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        @string want = helloWorldˢ2;
        if (((sstring)got) != want) {
            tΔ1.Fatalf("got %q, want %q"u8, got, want);
        }
    });
}

public static void TestRemoveReadOnlyFile(ж<Δtesting.T> Ꮡt) {
    testMaybeRooted(Ꮡt, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        {
            var errΔ1 = WriteFile(fileˢ2, slice<byte>("1"u8), 0); if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
        }
        error err = default!;
        if (r == nil){
            err = Remove(fileˢ2);
        } else {
            err = r.Remove(fileˢ2);
        }
        if (err != default!) {
            tΔ1.Fatalf("Remove read-only file: %v"u8, err);
        }
        {
            var (_, errΔ1) = Stat(fileˢ2); if (!IsNotExist(errΔ1)) {
                tΔ1.Fatalf("Stat read-only file after removal: %v (want IsNotExist)"u8, errΔ1);
            }
        }
    });
}

public static void TestOpenFileDevNull(ж<Δtesting.T> Ꮡt) {
    // See https://go.dev/issue/71752.
    Ꮡt.Parallel();
    var (f, err) = OpenFile(DevNull, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_TRUNC), 420);
    if (err != default!) {
        Ꮡt.Fatalf("OpenFile(DevNull): %v"u8, err);
    }
    f.Close();
}

} // end os_test_package
