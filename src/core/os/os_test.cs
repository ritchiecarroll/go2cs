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
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using fstest = go.testing.fstest_package;
using time = time_package;
using @internal;
using go.io;
using go.os;
using go.path;
using go.runtime;
using go.testing;
using static go.os_internal_test_package;
using Δos = os_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goOsTestDrainStdinˢ => ref goOsTestDrainStdinˢᵇ;
private static readonly @string goOsTestDrainStdinˢᵇ = "GO_OS_TEST_DRAIN_STDIN"u8;

public static void TestMain(ж<Δtesting.M> m) {
    if (Getenv(goOsTestDrainStdinˢ) == "1"u8) {
        Stdout.Close();
        Δio.Copy(Δio.Discard, new os_test_package.os_FileжReader(Stdin));
        Exit(0);
    }

    Δlog.SetFlags((nint)((nint)Δlog.LstdFlags | (nint)Δlog.Lshortfile));

    Exit(m.Run());
}

internal static slice<@string> dot = [
    "dir_unix.go",
    "env.go",
    "error.go",
    "file.go",
    "os_test.go",
    "types.go",
    "stat_darwin.go",
    "stat_linux.go"
];

partial struct sysDir {
    internal @string name;
    internal slice<@string> files;
}

internal static ж<sysDir> sysdir = func(ж<sysDir> () => {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8: {
        return Ꮡ(new sysDir(
            "/system/lib",
            [
                "libmedia.so",
                "libpowermanager.so"
            ]
        ));
    }
    case {} when exprᴛ1 == "ios"u8: {
        var (wd, err) = syscall.Getwd();
        if (err != nil) {
            wd = err.Error();
        }
        var sd = Ꮡ(new sysDir(
            filepath.Join(wd, "..", ".."),
            [
                "ResourceRules.plist",
                "Info.plist"
            ]
        ));
        var found = true;
        foreach (var (_, f) in range(sd.files)) {
            @string path = filepath.Join(sd.name, f);
            var (_, errΔ1) = Stat(path); if (errΔ1 != nil) {
                found = false;
                break;
            }
        }
        if (found) {
            return sd;
        }
        // In a self-hosted iOS build the above files might
        // not exist. Look for system files instead below.
        break;
    }
    case {} when exprᴛ1 == "windows"u8: {
        return Ꮡ(new sysDir(
            Getenv("SystemRoot") + "\\system32\\drivers\\etc"u8,
            [
                "networks",
                "protocol",
                "services"
            ]
        ));
    }
    case {} when exprᴛ1 == "plan9"u8: {
        return Ꮡ(new sysDir(
            "/lib/ndb",
            [
                "common",
                "local"
            ]
        ));
    }
    case {} when exprᴛ1 == "wasip1"u8: {
        // wasmtime has issues resolving symbolic links that are often present
        // in directories like /etc/group below (e.g. private/etc/group on OSX).
        // For this reason we use files in the Go source tree instead.
        return Ꮡ(new sysDir(
            Δruntime.GOROOT(),
            [
                "go.env",
                "LICENSE",
                "CONTRIBUTING.md"
            ]
        ));
    }}
    return Ꮡ(new sysDir(
        "/etc",
        [
            "group",
            "hosts",
            "passwd"
        ]
    ));
})();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object openFailedˢ = (@string)"open failed:"u8;

internal static int64 size(@string name, ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var (@file, err) = Open(name);
        if (err != nil) {
            t.Fatal(openFailedˢ, err);
        }
        var fileʗ1 = @file;
        defer(() => {
            var errΔ1 = fileʗ1.Close(); if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
        }, ref ᒐ);
        (var n, err) = Δio.Copy(Δio.Discard, new os_test_package.os_FileжReader(@file));
        if (err != nil) {
            t.Fatal(err);
        }
        return n;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static bool /*r*/ equal(@string name1, @string name2) {
    bool r = default;

    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8: {
        r = strings.EqualFold(name1, name2);
        break;
    }
    default: {
        r = name1 == name2;
        break;
    }}
    return r;
}

internal static ж<Δos.File> /*f*/ newFile(ж<Δtesting.T> t) {
    ж<Δos.File> f = default;

    t.Helper();
    (f, var err) = CreateTemp(""u8, "_Go_"u8 + t.Name());
    if (err != nil) {
        t.Fatal(err);
    }
    var fʗ1 = f;
    t.Cleanup(() => {
        var errΔ1 = fʗ1.Close(); if (errΔ1 != nil && !errors.Is(errΔ1, ErrClosed)) {
            t.Fatal(errΔ1);
        }
        var errΔ2 = Remove(fʗ1.Name()); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    });
    return f;
}

internal static @string sfdir = sysdir.name;
internal static @string sfname = sysdir.files[0];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object statFailedˢ = (@string)"stat failed:"u8;
internal static readonly object nameShouldBeˢ = (@string)"name should be "u8;
internal static readonly object sizeShouldBeˢ = (@string)"size should be"u8;

public static void TestStat(ж<Δtesting.T> t) {
    t.Parallel();

    @string path = sfdir + "/"u8 + sfname;
    var (dir, err) = Stat(path);
    if (err != nil) {
        t.Fatal(statFailedˢ, err);
    }
    if (!equal(sfname, dir.Name())) {
        t.Error(nameShouldBeˢ, sfname, (@string)"; is", dir.Name());
    }
    var filesize = size(path, t);
    if (dir.Size() != filesize) {
        t.Error(sizeShouldBeˢ, filesize, (@string)"; is", dir.Size());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noSuchFileˢ => ref noSuchFileˢᵇ;
private static readonly @string noSuchFileˢᵇ = "no-such-file"u8;
internal static readonly object gotNilWantErrorˢ = (@string)"got nil, want error"u8;
internal static ref readonly @string symlinkˢ => ref symlinkˢᵇ;
private static readonly @string symlinkˢᵇ = "symlink"u8;

public static void TestStatError(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());

    @string path = noSuchFileˢ;

    var (fi, err) = Stat(path);
    if (err == nil) {
        t.Fatal(gotNilWantErrorˢ);
    }
    if (fi != nil) {
        t.Errorf("got %v, want nil", fi);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
            t.Errorf("got %T, want %T", err, perr.OrTypedNil());
        }
    }

    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));

    @string link = symlinkˢ;
    err = Symlink(path, link);
    if (err != nil) {
        t.Fatal(err);
    }

    (fi, err) = Stat(link);
    if (err == nil) {
        t.Fatal(gotNilWantErrorˢ);
    }
    if (fi != nil) {
        t.Errorf("got %v, want nil", fi);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
            t.Errorf("got %T, want %T", err, perr.OrTypedNil());
        }
    }
}

public static void TestStatSymlinkLoop(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
        t.Chdir(t.TempDir());

        var err = Symlink("x", "y");
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Remove, (@string)"y", ref ᒐ);

        err = Symlink("y", "x");
        if (err != nil) {
            t.Fatal(err);
        }
        defer(Remove, (@string)"x", ref ᒐ);

        (_, err) = Stat("x");
        var (_, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok) {
            t.Errorf("expected *PathError, got %T: %v\n", err, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fstatFailedˢ = (@string)"fstat failed:"u8;

public static void TestFstat(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        @string path = sfdir + "/"u8 + sfname;
        var (@file, err1) = Open(path);
        if (err1 != nil) {
            t.Fatal(openFailedˢ, err1);
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);
        var (dir, err2) = @file.Stat();
        if (err2 != nil) {
            t.Fatal(fstatFailedˢ, err2);
        }
        if (!equal(sfname, dir.Name())) {
            t.Error(nameShouldBeˢ, sfname, (@string)"; is", dir.Name());
        }
        var filesize = size(path, t);
        if (dir.Size() != filesize) {
            t.Error(sizeShouldBeˢ, filesize, (@string)"; is", dir.Size());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lstatFailedˢ = (@string)"lstat failed:"u8;

public static void TestLstat(ж<Δtesting.T> t) {
    t.Parallel();

    @string path = sfdir + "/"u8 + sfname;
    var (dir, err) = Lstat(path);
    if (err != nil) {
        t.Fatal(lstatFailedˢ, err);
    }
    if (!equal(sfname, dir.Name())) {
        t.Error(nameShouldBeˢ, sfname, (@string)"; is", dir.Name());
    }
    if ((fs.FileMode)(dir.Mode() & ModeSymlink) == 0) {
        var filesize = size(path, t);
        if (dir.Size() != filesize) {
            t.Error(sizeShouldBeˢ, filesize, (@string)"; is", dir.Size());
        }
    }
}

// Read with length 0 should not return EOF.
public static void TestRead0(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        @string path = sfdir + "/"u8 + sfname;
        var (f, err) = Open(path);
        if (err != nil) {
            t.Fatal(openFailedˢ, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var b = new slice<byte>(0);
        (var n, err) = f.Read(b);
        if (n != 0 || err != nil) {
            t.Errorf("Read(0) = %d, %v, want 0, nil", n, err);
        }
        b = new slice<byte>(100);
        (n, err) = f.Read(b);
        if (n <= 0 || err != nil) {
            t.Errorf("Read(100) = %d, %v, want >0, nil", n, err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Reading a closed file should return ErrClosed error
public static void TestReadClosed(ж<Δtesting.T> t) {
    t.Parallel();

    @string path = sfdir + "/"u8 + sfname;
    var (@file, err) = Open(path);
    if (err != nil) {
        t.Fatal(openFailedˢ, err);
    }
    @file.Close(); // close immediately

    var b = new slice<byte>(100);
    (_, err) = @file.Read(b);

    var (e, ok) = err._<ж<fs.PathError>>(ᐧ);
    if (!ok || !AreEqual(e.Err, ErrClosed)) {
        t.Fatalf("Read: got %T(%v), want %T(%v)", err, err, e.OrTypedNil(), ErrClosed);
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
            if (err != nil) {
                t.Fatalf("open %q failed: %v", dir, err);
            }
            var fileʗ1 = @file;
            defer(() => fileʗ1.Close(), ref ᒐ);
            var (s, err2) = @file.Readdirnames(-1);
            if (err2 != nil) {
                t.Fatalf("Readdirnames %q failed: %v", dir, err2);
            }
            foreach (var (_, m) in range(contentsʗ1)) {
                var found = false;
                foreach (var (_, n) in range(s)) {
                    if (n == "."u8 || n == ".."u8) {
                        t.Errorf("got %q in directory", n);
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
            if (s == nil) {
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
            if (err != nil) {
                t.Fatalf("open %q failed: %v", dir, err);
            }
            var fileʗ1 = @file;
            defer(() => fileʗ1.Close(), ref ᒐ);
            var (s, err2) = @file.Readdir(-1);
            if (err2 != nil) {
                t.Fatalf("Readdir %q failed: %v", dir, err2);
            }
            foreach (var (_, m) in range(contentsʗ1)) {
                var found = false;
                foreach (var (_, n) in range(s)) {
                    if (n.Name() == "."u8 || n.Name() == ".."u8) {
                        t.Errorf("got %q in directory", n.Name());
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
            if (s == nil) {
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
            if (err != nil) {
                t.Fatalf("open %q failed: %v", dir, err);
            }
            var fileʗ1 = @file;
            defer(() => fileʗ1.Close(), ref ᒐ);
            var (s, err2) = @file.ReadDir(-1);
            if (err2 != nil) {
                t.Fatalf("ReadDir %q failed: %v", dir, err2);
            }
            foreach (var (_, m) in range(contentsʗ1)) {
                var found = false;
                foreach (var (_, n) in range(s)) {
                    if (n.Name() == "."u8 || n.Name() == ".."u8) {
                        t.Errorf("got %q in directory", n);
                    }
                    if (!equal(m, n.Name())) {
                        continue;
                    }
                    if (found) {
                        t.Error(presentTwiceˢ, m);
                    }
                    found = true;
                    var (lstat, errΔ1) = Lstat(dir + "/"u8 + m);
                    if (errΔ1 != nil) {
                        t.Fatal(errΔ1);
                    }
                    if (n.IsDir() != lstat.IsDir()) {
                        t.Errorf("%s: IsDir=%v, want %v", m, n.IsDir(), lstat.IsDir());
                    }
                    if (n.Type() != lstat.Mode().Type()) {
                        t.Errorf("%s: IsDir=%v, want %v", m, n.Type(), lstat.Mode().Type());
                    }
                    (var info, errΔ1) = n.Info();
                    if (errΔ1 != nil) {
                        t.Errorf("%s: Info: %v", m, errΔ1);
                        continue;
                    }
                    if (!SameFile(info, lstat)) {
                        t.Errorf("%s: Info: SameFile(info, lstat) = false", m);
                    }
                }
                if (!found) {
                    t.Error(couldNotFindˢ, m);
                }
            }
            if (s == nil) {
                t.Error(readDirReturnedNilˢ);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sysdirˢ => ref sysdirˢᵇ;
private static readonly @string sysdirˢᵇ = "sysdir"u8;
internal static ref readonly @string tempDirˢ => ref tempDirˢᵇ;
private static readonly @string tempDirˢᵇ = "TempDir"u8;

public static void TestFileReaddirnames(ж<Δtesting.T> t) {
    t.Parallel();

    t.Run(".", testReaddirnames(".", dot));
    t.Run(sysdirˢ, testReaddirnames(sysdir.name, sysdir.files));
    t.Run(tempDirˢ, testReaddirnames(t.TempDir(), default));
}

public static void TestFileReaddir(ж<Δtesting.T> t) {
    t.Parallel();

    t.Run(".", testReaddir(".", dot));
    t.Run(sysdirˢ, testReaddir(sysdir.name, sysdir.files));
    t.Run(tempDirˢ, testReaddir(t.TempDir(), default));
}

public static void TestFileReadDir(ж<Δtesting.T> t) {
    t.Parallel();

    t.Run(".", testReadDir(".", dot));
    t.Run(sysdirˢ, testReadDir(sysdir.name, sysdir.files));
    t.Run(tempDirˢ, testReadDir(t.TempDir(), default));
}

internal static void benchmarkReaddirname(@string path, ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint nentries = default;
    for (nint i = 0; i < b.N; i++) {
        var (f, err) = Open(path);
        if (err != nil) {
            Ꮡb.Fatalf("open %q failed: %v", path, err);
        }
        (var ns, err) = f.Readdirnames(-1);
        f.Close();
        if (err != nil) {
            Ꮡb.Fatalf("readdirnames %q failed: %v", path, err);
        }
        nentries = len(ns);
    }
    Ꮡb.Logf("benchmarkReaddirname %q: %d entries", path, nentries);
}

internal static void benchmarkReaddir(@string path, ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint nentries = default;
    for (nint i = 0; i < b.N; i++) {
        var (f, err) = Open(path);
        if (err != nil) {
            Ꮡb.Fatalf("open %q failed: %v", path, err);
        }
        (var fs, err) = f.Readdir(-1);
        f.Close();
        if (err != nil) {
            Ꮡb.Fatalf("readdir %q failed: %v", path, err);
        }
        nentries = len(fs);
    }
    Ꮡb.Logf("benchmarkReaddir %q: %d entries", path, nentries);
}

internal static void benchmarkReadDir(@string path, ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint nentries = default;
    for (nint i = 0; i < b.N; i++) {
        var (f, err) = Open(path);
        if (err != nil) {
            Ꮡb.Fatalf("open %q failed: %v", path, err);
        }
        (var fs, err) = f.ReadDir(-1);
        f.Close();
        if (err != nil) {
            Ꮡb.Fatalf("readdir %q failed: %v", path, err);
        }
        nentries = len(fs);
    }
    Ꮡb.Logf("benchmarkReadDir %q: %d entries", path, nentries);
}

public static void BenchmarkReaddirname(ж<Δtesting.B> b) {
    benchmarkReaddirname(".", b);
}

public static void BenchmarkReaddir(ж<Δtesting.B> b) {
    benchmarkReaddir(".", b);
}

public static void BenchmarkReadDir(ж<Δtesting.B> b) {
    benchmarkReadDir(".", b);
}

internal static void benchmarkStat(ж<Δtesting.B> Ꮡb, @string path) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = Stat(path);
        if (err != nil) {
            Ꮡb.Fatalf("Stat(%q) failed: %v", path, err);
        }
    }
}

internal static void benchmarkLstat(ж<Δtesting.B> Ꮡb, @string path) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = Lstat(path);
        if (err != nil) {
            Ꮡb.Fatalf("Lstat(%q) failed: %v", path, err);
        }
    }
}

public static void BenchmarkStatDot(ж<Δtesting.B> b) {
    benchmarkStat(b, ".");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcOsOsTestGoˢ => ref srcOsOsTestGoˢᵇ;
private static readonly @string srcOsOsTestGoˢᵇ = "src/os/os_test.go"u8;

public static void BenchmarkStatFile(ж<Δtesting.B> b) {
    benchmarkStat(b, filepath.Join(Δruntime.GOROOT(), srcOsOsTestGoˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcOsˢ => ref srcOsˢᵇ;
private static readonly @string srcOsˢᵇ = "src/os"u8;

public static void BenchmarkStatDir(ж<Δtesting.B> b) {
    benchmarkStat(b, filepath.Join(Δruntime.GOROOT(), srcOsˢ));
}

public static void BenchmarkLstatDot(ж<Δtesting.B> b) {
    benchmarkLstat(b, ".");
}

public static void BenchmarkLstatFile(ж<Δtesting.B> b) {
    benchmarkLstat(b, filepath.Join(Δruntime.GOROOT(), srcOsOsTestGoˢ));
}

public static void BenchmarkLstatDir(ж<Δtesting.B> b) {
    benchmarkLstat(b, filepath.Join(Δruntime.GOROOT(), srcOsˢ));
}

// Read the directory one entry at a time.
internal static slice<@string> smallReaddirnames(ж<Δos.File> Ꮡfile, nint length, ж<Δtesting.T> t) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    var names = new slice<@string>(length);
    nint count = 0;
    for (;;) {
        var (d, err) = Ꮡfile.Readdirnames(1);
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
        if (err != nil) {
            t.Fatalf("readdirnames %q failed: %v", @file.Name(), err);
        }
        if (len(d) == 0) {
            t.Fatalf("readdirnames %q returned empty slice and no error", @file.Name());
        }
        names[count] = d[0];
        count++;
    }
    return names.slice(0, count);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string usrBinˢ => ref usrBinˢᵇ;
private static readonly @string usrBinˢᵇ = "/usr/bin"u8;
internal static ref readonly @string systemBinˢ => ref systemBinˢᵇ;
private static readonly @string systemBinˢᵇ = "/system/bin"u8;
internal static ref readonly @string binˢ => ref binˢᵇ;
private static readonly @string binˢᵇ = "/bin"u8;
internal static ref readonly @string systemRootˢ => ref systemRootˢᵇ;
private static readonly @string systemRootˢᵇ = "SystemRoot"u8;

// Check that reading a directory one entry at a time gives the same result
// as reading it all at once.
public static void TestReaddirnamesOneAtATime(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        // big directory that doesn't change often.
        @string dir = usrBinˢ;
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "android"u8: {
            dir = systemBinˢ;
            break;
        }
        case {} when exprᴛ1 == "ios"u8 || exprᴛ1 == "wasip1"u8: {
            var (wd, errΔ2) = Getwd();
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            dir = wd;
            break;
        }
        case {} when exprᴛ1 == "plan9"u8: {
            dir = binˢ;
            break;
        }
        case {} when exprᴛ1 == "windows"u8: {
            dir = Getenv(systemRootˢ) + "\\system32"u8;
            break;
        }}
        var (@file, err) = Open(dir);
        if (err != nil) {
            t.Fatalf("open %q failed: %v", dir, err);
        }
        var fileʗ1 = @file;
        defer(() => fileʗ1.Close(), ref ᒐ);
        var (all, err1) = @file.Readdirnames(-1);
        if (err1 != nil) {
            t.Fatalf("readdirnames %q failed: %v", dir, err1);
        }
        var (file1, err2) = Open(dir);
        if (err2 != nil) {
            t.Fatalf("open %q failed: %v", dir, err2);
        }
        var file1ʗ1 = file1;
        defer(() => file1ʗ1.Close(), ref ᒐ);
        var small = smallReaddirnames(file1, len(all) + 100, t); // +100 in case we screw up
        if (len(small) < len(all)) {
            t.Fatalf("len(small) is %d, less than %d", len(small), len(all));
        }
        foreach (var (i, n) in range(all)) {
            if (small[i] != n) {
                t.Errorf("small read %q mismatch: %v", small[i], n);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testShortSkippingˢ = (@string)"test.short; skipping"u8;

public static void TestReaddirNValues(ж<Δtesting.T> t) {
    if (Δtesting.Short()) {
        t.Skip(testShortSkippingˢ);
    }
    t.Parallel();

    @string dir = t.TempDir();
    for (nint i = 1; i <= 105; i++) {
        var (f, err) = Create(filepath.Join(dir, fmt.Sprintf("%d"u8, i)));
        if (err != nil) {
            t.Fatalf("Create: %v", err);
        }
        f.Write(slice<byte>(strings.Repeat("X", i)));
        f.Close();
    }

    ref var d = ref heap<ж<Δos.File>>(out var Ꮡd);
    void openDir() {
        error err = default;
        (Ꮡd.ValueSlot, err) = Open(dir);
        if (err != nil) {
            t.Fatalf("Open directory: %v", err);
        }
    }

    var readdirExpect = (nint n, nint want, error wantErr) => {
        t.Helper();
        var (fi, err) = Ꮡd.ValueSlot.Readdir(n);
        if (!AreEqual(err, wantErr)) {
            t.Fatalf("Readdir of %d got error %v, want %v", n, err, wantErr);
        }
        nint g = len(fi);
        nint e = want; if (g != e) {
            t.Errorf("Readdir of %d got %d files, want %d", n, g, e);
        }
    };

    var readDirExpect = (nint n, nint want, error wantErr) => {
        t.Helper();
        var (de, err) = Ꮡd.ValueSlot.ReadDir(n);
        if (!AreEqual(err, wantErr)) {
            t.Fatalf("ReadDir of %d got error %v, want %v", n, err, wantErr);
        }
        nint g = len(de);
        nint e = want; if (g != e) {
            t.Errorf("ReadDir of %d got %d files, want %d", n, g, e);
        }
    };

    var readdirnamesExpect = (nint n, nint want, error wantErr) => {
        t.Helper();
        var (fi, err) = Ꮡd.ValueSlot.Readdirnames(n);
        if (!AreEqual(err, wantErr)) {
            t.Fatalf("Readdirnames of %d got error %v, want %v", n, err, wantErr);
        }
        nint g = len(fi);
        nint e = want; if (g != e) {
            t.Errorf("Readdirnames of %d got %d files, want %d", n, g, e);
        }
    };

    foreach (var (_, fn) in range(slice<Action<nint, nint, error>>([readdirExpect, readdirnamesExpect, readDirExpect]))) {
        // Test the slurp case
        openDir();
        fn(0, 105, default);
        fn(0, 0, default);
        d.Close();

        // Slurp with -1 instead
        openDir();
        fn(-1, 105, default);
        fn(-2, 0, default);
        fn(0, 0, default);
        d.Close();

        // Test the bounded case
        openDir();
        fn(1, 1, default);
        fn(2, 2, default);
        fn(105, 102, default); // and tests buffer >100 case
        fn(3, 0, Δio.EOF);
        d.Close();
    }
}

internal static void touch(ж<Δtesting.T> t, @string name) {
    var (f, err) = Create(name);
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ1 = f.Close(); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string good1ˢ => ref good1ˢᵇ;
private static readonly @string good1ˢᵇ = "good1"u8;
internal static ref readonly @string good2ˢ => ref good2ˢᵇ;
private static readonly @string good2ˢᵇ = "good2"u8;
internal static ref readonly @string initialReaddirˢ => ref initialReaddirˢᵇ;
private static readonly @string initialReaddirˢᵇ = "initial readdir"u8;
internal static ref readonly @string withXDisappearingˢ => ref withXDisappearingˢᵇ;
private static readonly @string withXDisappearingˢᵇ = "with x disappearing"u8;
internal static ref readonly @string someRealErrorˢ => ref someRealErrorˢᵇ;
private static readonly @string someRealErrorˢᵇ = "some real error"u8;

public static void TestReaddirStatFailures(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8: {
        // Windows and Plan 9 already do this correctly,
        // but are structured with different syscalls such
        // that they don't use Lstat, so the hook below for
        // testing it wouldn't work.
        t.Skipf("skipping test on %v", Δruntime.GOOS);
        break;
    }}

    ref var xerr = ref heap<error>(out var Ꮡxerr); // error to return for x
    os_internal_test_package.SetStatHook(new os_test_package.testing_Tжcleanuper(t), (ж<Δos.File> f, @string path) => {
        if (Ꮡxerr.ValueSlot != nil && strings.HasSuffix(path, "x")) {
            return (default, Ꮡxerr.ValueSlot);
        }
        return (default, default);
    });

    @string dir = t.TempDir();
    touch(t, filepath.Join(dir, good1ˢ));
    touch(t, filepath.Join(dir, "x")); // will disappear or have an error
    touch(t, filepath.Join(dir, good2ˢ));
    (slice<fs.FileInfo>, error) readDir() {
        GoFrame ᒐ = default;
        try {
            var (d, err) = Open(dir);
            if (err != nil) {
                t.Fatal(err);
            }
            var dʗ1 = d;
            defer(() => dʗ1.Close(), ref ᒐ);
            return d.Readdir(-1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    }
    var readDirʗ1 = readDir;
    slice<fs.FileInfo> mustReadDir(@string testName) {
        var (fis, err) = readDirʗ1();
        if (err != nil) {
            t.Fatalf("%s: Readdir: %v", testName, err);
        }
        return fis;
    }
    slice<@string> names(slice<fs.FileInfo> fis) {
        var s = new slice<@string>(len(fis));
        foreach (var (i, fi) in range(fis)) {
            s[i] = fi.Name();
        }
        slices.Sort<slice<@string>, @string>(s);
        return s;
    }

    {
        var (got, want) = (names(mustReadDir(initialReaddirˢ)), slice<@string>(["good1", "good2", "x"])); if (!slices.Equal<slice<@string>, @string>(got, want)) {
            t.Errorf("initial readdir got %q; want %q", got, want);
        }
    }

    xerr = ErrNotExist;
    {
        var (got, want) = (names(mustReadDir(withXDisappearingˢ)), slice<@string>(["good1", "good2"])); if (!slices.Equal<slice<@string>, @string>(got, want)) {
            t.Errorf("with x disappearing, got %q; want %q", got, want);
        }
    }

    xerr = errors.New(someRealErrorˢ);
    {
        var (_, err) = readDir(); if (!AreEqual(err, xerr)) {
            t.Errorf("with a non-ErrNotExist error, got error %v; want %v", err, xerr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goReaddirOfFileˢ => ref goReaddirOfFileˢᵇ;
private static readonly @string goReaddirOfFileˢᵇ = "_Go_ReaddirOfFile"u8;
internal static readonly object readdirnamesSucceededˢ = (@string)"Readdirnames succeeded; want non-nil error"u8;

// Readdir on a regular file should fail.
public static void TestReaddirOfFile(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        var (f, err) = CreateTemp(t.TempDir(), goReaddirOfFileˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        f.Write(slice<byte>("foo"u8));
        f.Close();
        (var reg, err) = Open(f.Name());
        if (err != nil) {
            t.Fatal(err);
        }
        var regʗ1 = reg;
        defer(() => regʗ1.Close(), ref ᒐ);

        (var names, err) = reg.Readdirnames(-1);
        if (err == nil) {
            t.Error(readdirnamesSucceededˢ);
        }
        var (pe, ok) = errors.AsType<ж<fs.PathError>>(err); if (!ok || pe.Path != f.Name()) {
            t.Errorf("Readdirnames returned %q; want a PathError with path %q", err, f.Name());
        }
        if (len(names) > 0) {
            t.Errorf("unexpected dir names in regular file: %q", names);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestHardLink(ж<Δtesting.T> t) {
    testMaybeRooted(t, testHardLink);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linkatˢ => ref linkatˢᵇ;
private static readonly @string linkatˢᵇ = "linkat"u8;
internal static ref readonly @string hardlinktestfromˢ => ref hardlinktestfromˢᵇ;
private static readonly @string hardlinktestfromˢᵇ = "hardlinktestfrom"u8;
internal static ref readonly @string hardlinktesttoˢ => ref hardlinktesttoˢᵇ;
private static readonly @string hardlinktesttoˢᵇ = "hardlinktestto"u8;
internal static ref readonly @string hardlinktestnoneˢ => ref hardlinktestnoneˢᵇ;
private static readonly @string hardlinktestnoneˢᵇ = "hardlinktestnone"u8;
internal static readonly object fileExistsErrorˢ = (@string)"file exists error"u8;

internal static void testHardLink(ж<Δtesting.T> t, ж<Δos.Root> root) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(t));

    Func<@string, (ж<Δos.File>, error)> create = Create;
    Func<@string, @string, error> link = Link;
    Func<@string, (fs.FileInfo, error)> stat = Stat;
    @string op = linkˢ;
    if (root != nil) {
        create = (@string p1) => root.Create(p1);
        link = (@string p1, @string p2) => root.Link(p1, p2);
        stat = (@string p1) => root.Stat(p1);
        op = linkatˢ;
    }

    @string from = hardlinktestfromˢ;
    @string to = hardlinktesttoˢ;
    var (@file, err) = create(to);
    if (err != nil) {
        t.Fatalf("open %q failed: %v", to, err);
    }
    err = @file.Close(); if (err != nil) {
        t.Errorf("close %q failed: %v", to, err);
    }
    err = link(to, from);
    if (err != nil) {
        t.Fatalf("link %q, %q failed: %v", to, from, err);
    }

    @string none = hardlinktestnoneˢ;
    err = link(none, none);
    // Check the returned error is well-formed.
    var (lerr, ok) = err._<ж<Δos.LinkError>>(ᐧ); if (!ok || lerr.Error() == ""u8) {
        t.Errorf("link %q, %q failed to return a valid error", none, none);
    }

    (var tostat, err) = stat(to);
    if (err != nil) {
        t.Fatalf("stat %q failed: %v", to, err);
    }
    (var fromstat, err) = stat(from);
    if (err != nil) {
        t.Fatalf("stat %q failed: %v", from, err);
    }
    if (!SameFile(tostat, fromstat)) {
        t.Errorf("link %q, %q did not create hard link", to, from);
    }
    // We should not be able to perform the same Link() a second time
    err = link(to, from);
    switch (err.type()) {
    case ж<Δos.LinkError> errΔ1: {
        if (errΔ1.Op != op) {
            t.Errorf("Link(%q, %q) err.Op = %q; want %q", to, from, errΔ1.Op, op);
        }
        if (errΔ1.Old != to) {
            t.Errorf("Link(%q, %q) err.Old = %q; want %q", to, from, errΔ1.Old, to);
        }
        if (errΔ1.New != from) {
            t.Errorf("Link(%q, %q) err.New = %q; want %q", to, from, errΔ1.New, from);
        }
        if (!IsExist(errΔ1.Err)) {
            t.Errorf("Link(%q, %q) err.Err = %q; want %q", to, from, errΔ1.Err, fileExistsErrorˢ);
        }
        break;
    }
    case null: {
        t.Errorf("link %q, %q: expected error, got nil", from, to);
        break;
    }
    default: {
        var errΔ1 = err;
        t.Errorf("link %q, %q: expected %T, got %T %v", from, to, @new<Δos.LinkError>(), errΔ1, errΔ1);
        break;
    }}
}

public static void TestSymlink(ж<Δtesting.T> t) {
    testMaybeRooted(t, testSymlink);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string symlinktestfromˢ => ref symlinktestfromˢᵇ;
private static readonly @string symlinktestfromˢᵇ = "symlinktestfrom"u8;
internal static ref readonly @string symlinktesttoˢ => ref symlinktesttoˢᵇ;
private static readonly @string symlinktesttoˢᵇ = "symlinktestto"u8;

internal static void testSymlink(ж<Δtesting.T> t, ж<Δos.Root> root) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));

    Func<@string, (ж<Δos.File>, error)> create = Create;
    Func<@string, (ж<Δos.File>, error)> open = Open;
    Func<@string, @string, error> symlink = Symlink;
    Func<@string, (fs.FileInfo, error)> stat = Stat;
    Func<@string, (fs.FileInfo, error)> lstat = Lstat;
    Func<@string, (@string, error)> readlink = Readlink;
    if (root != nil) {
        create = (@string p1) => root.Create(p1);
        open = (@string p1) => root.Open(p1);
        symlink = (@string p1, @string p2) => root.Symlink(p1, p2);
        stat = (@string p1) => root.Stat(p1);
        lstat = (@string p1) => root.Lstat(p1);
        readlink = (@string p1) => root.Readlink(p1);
    }

    @string from = symlinktestfromˢ;
    @string to = symlinktesttoˢ;
    var (@file, err) = create(to);
    if (err != nil) {
        t.Fatalf("Create(%q) failed: %v", to, err);
    }
    err = @file.Close(); if (err != nil) {
        t.Errorf("Close(%q) failed: %v", to, err);
    }
    err = symlink(to, from);
    if (err != nil) {
        t.Fatalf("Symlink(%q, %q) failed: %v", to, from, err);
    }
    (var tostat, err) = lstat(to);
    if (err != nil) {
        t.Fatalf("Lstat(%q) failed: %v", to, err);
    }
    if ((fs.FileMode)(tostat.Mode() & ModeSymlink) != 0) {
        t.Fatalf("Lstat(%q).Mode()&ModeSymlink = %v, want 0", to, (fs.FileMode)(tostat.Mode() & ModeSymlink));
    }
    (var fromstat, err) = stat(from);
    if (err != nil) {
        t.Fatalf("Stat(%q) failed: %v", from, err);
    }
    if (!SameFile(tostat, fromstat)) {
        t.Errorf("Symlink(%q, %q) did not create symlink", to, from);
    }
    (fromstat, err) = lstat(from);
    if (err != nil) {
        t.Fatalf("Lstat(%q) failed: %v", from, err);
    }
    if ((fs.FileMode)(fromstat.Mode() & ModeSymlink) == 0) {
        t.Fatalf("Lstat(%q).Mode()&ModeSymlink = 0, want %v", from, ModeSymlink);
    }
    (fromstat, err) = stat(from);
    if (err != nil) {
        t.Fatalf("Stat(%q) failed: %v", from, err);
    }
    if (fromstat.Name() != from) {
        t.Errorf("Stat(%q).Name() = %q, want %q", from, fromstat.Name(), from);
    }
    if ((fs.FileMode)(fromstat.Mode() & ModeSymlink) != 0) {
        t.Fatalf("Stat(%q).Mode()&ModeSymlink = %v, want 0", from, (fs.FileMode)(fromstat.Mode() & ModeSymlink));
    }
    (var s, err) = readlink(from);
    if (err != nil) {
        t.Fatalf("Readlink(%q) failed: %v", from, err);
    }
    if (s != to) {
        t.Fatalf("Readlink(%q) = %q, want %q", from, s, to);
    }
    (@file, err) = open(from);
    if (err != nil) {
        t.Fatalf("Open(%q) failed: %v", from, err);
    }
    @file.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string longsymlinktestfromˢ => ref longsymlinktestfromˢᵇ;
private static readonly @string longsymlinktestfromˢᵇ = "longsymlinktestfrom"u8;

public static void TestLongSymlink(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    t.Chdir(t.TempDir());

    @string s = "0123456789abcdef";
    // Long, but not too long: a common limit is 255.
    s = s + s + s + s + s + s + s + s + s + s + s + s + s + s + s;
    @string from = longsymlinktestfromˢ;
    var err = Symlink(s, from);
    if (err != nil) {
        t.Fatalf("symlink %q, %q failed: %v", s, from, err);
    }
    (var r, err) = Readlink(from);
    if (err != nil) {
        t.Fatalf("readlink %q failed: %v", from, err);
    }
    if (r != s) {
        t.Fatalf("after symlink %q != %q", r, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string renamefromˢ => ref renamefromˢᵇ;
private static readonly @string renamefromˢᵇ = "renamefrom"u8;
internal static ref readonly @string renametoˢ => ref renametoˢᵇ;
private static readonly @string renametoˢᵇ = "renameto"u8;

public static void TestRename(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;

    var (@file, err) = Create(from);
    if (err != nil) {
        t.Fatalf("open %q failed: %v", from, err);
    }
    err = @file.Close(); if (err != nil) {
        t.Errorf("close %q failed: %v", from, err);
    }
    err = Rename(from, to);
    if (err != nil) {
        t.Fatalf("rename %q, %q failed: %v", to, from, err);
    }
    (_, err) = Stat(to);
    if (err != nil) {
        t.Errorf("stat %q failed: %v", to, err);
    }
}

public static void TestRenameOverwriteDest(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;

    var toData = slice<byte>("to"u8);
    var fromData = slice<byte>("from"u8);

    var err = WriteFile(to, toData, 511);
    if (err != nil) {
        t.Fatalf("write file %q failed: %v", to, err);
    }

    err = WriteFile(from, fromData, 511);
    if (err != nil) {
        t.Fatalf("write file %q failed: %v", from, err);
    }
    err = Rename(from, to);
    if (err != nil) {
        t.Fatalf("rename %q, %q failed: %v", to, from, err);
    }

    (_, err) = Stat(from);
    if (err == nil) {
        t.Errorf("from file %q still exists", from);
    }
    if (err != nil && !IsNotExist(err)) {
        t.Fatalf("stat from: %v", err);
    }
    (var toFi, err) = Stat(to);
    if (err != nil) {
        t.Fatalf("stat %q failed: %v", to, err);
    }
    if (toFi.Size() != (int64)len(fromData)) {
        t.Errorf(@"""to"" size = %d; want %d (old ""from"" size)", toFi.Size(), len(fromData));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object renameˢ = (@string)"rename"u8;

public static void TestRenameFailed(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;

    var err = Rename(from, to);
    switch (err.type()) {
    case ж<Δos.LinkError> errΔ1: {
        if (errΔ1.Op != "rename"u8) {
            t.Errorf("rename %q, %q: err.Op: want %q, got %q", from, to, renameˢ, errΔ1.Op);
        }
        if (errΔ1.Old != from) {
            t.Errorf("rename %q, %q: err.Old: want %q, got %q", from, to, from, errΔ1.Old);
        }
        if (errΔ1.New != to) {
            t.Errorf("rename %q, %q: err.New: want %q, got %q", from, to, to, errΔ1.New);
        }
        break;
    }
    case null: {
        t.Errorf("rename %q, %q: expected error, got nil", from, to);
        break;
    }
    default: {
        var errΔ1 = err;
        t.Errorf("rename %q, %q: expected %T, got %T %v", from, to, @new<Δos.LinkError>(), errΔ1, errΔ1);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doesntExistˢ => ref doesntExistˢᵇ;
private static readonly @string doesntExistˢᵇ = "doesnt-exist"u8;
internal static ref readonly @string destˢ => ref destˢᵇ;
private static readonly @string destˢᵇ = "dest"u8;

public static void TestRenameNotExisting(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());
    @string from = doesntExistˢ;
    @string to = destˢ;

    Mkdir(to, 511);

    var err = Rename(from, to); if (!IsNotExist(err)) {
        t.Errorf("Rename(%q, %q) = %v; want an IsNotExist error", from, to, err);
    }
}

public static void TestRenameToDirFailed(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());
    @string from = renamefromˢ;
    @string to = renametoˢ;

    Mkdir(from, 511);
    Mkdir(to, 511);

    var err = Rename(from, to);
    switch (err.type()) {
    case ж<Δos.LinkError> errΔ1: {
        if (errΔ1.Op != "rename"u8) {
            t.Errorf("rename %q, %q: err.Op: want %q, got %q", from, to, renameˢ, errΔ1.Op);
        }
        if (errΔ1.Old != from) {
            t.Errorf("rename %q, %q: err.Old: want %q, got %q", from, to, from, errΔ1.Old);
        }
        if (errΔ1.New != to) {
            t.Errorf("rename %q, %q: err.New: want %q, got %q", from, to, to, errΔ1.New);
        }
        break;
    }
    case null: {
        t.Errorf("rename %q, %q: expected error, got nil", from, to);
        break;
    }
    default: {
        var errΔ1 = err;
        t.Errorf("rename %q, %q: expected %T, got %T %v", from, to, @new<Δos.LinkError>(), errΔ1, errΔ1);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string renameFROMˢ => ref renameFROMˢᵇ;
private static readonly @string renameFROMˢᵇ = "renameFROM"u8;
internal static ref readonly @string renamEfromˢ => ref renamEfromˢᵇ;
private static readonly @string renamEfromˢᵇ = "RENAMEfrom"u8;

internal partial struct TestRenameCaseDifference_tests /*dyn*/ {
    internal @string name;
    internal Func<error> create;
}

public static void TestRenameCaseDifference(ж<Δtesting.T> pt) {
    @string from = renameFROMˢ;
    @string to = renamEfromˢ;
    var tests = slice<TestRenameCaseDifference_tests>([
        new("dir", () => Mkdir(from, 511)),
        new("file", () => {
            var (fd, err) = Create(from);
            if (err != nil) {
                return err;
            }
            return fd.Close();
        })
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestRenameCaseDifference_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        pt.Run(test.name, (ж<Δtesting.T> t) => {
            t.Chdir(t.TempDir());

            var errΔ1 = testʗ1.create(); if (errΔ1 != nil) {
                t.Fatalf("failed to create test file: %s", errΔ1);
            }

            var (_, errΔ2) = Stat(to); if (errΔ2 != nil) {
                // Sanity check that the underlying filesystem is not case sensitive.
                if (IsNotExist(errΔ2)) {
                    t.Skipf("case sensitive filesystem");
                }
                t.Fatalf("stat %q, got: %q", to, errΔ2);
            }

            var errΔ3 = Rename(from, to); if (errΔ3 != nil) {
                t.Fatalf("unexpected error when renaming from %q to %q: %s", from, to, errΔ3);
            }

            var (fd, err) = Open(".");
            if (err != nil) {
                t.Fatalf("Open .: %s", err);
            }

            // Stat does not return the real case of the file (it returns what the called asked for)
            // So we have to use readdir to get the real name of the file.
            (var dirNames, err) = fd.Readdirnames(-1);
            fd.Close();
            if (err != nil) {
                t.Fatalf("readdirnames: %s", err);
            }

            nint dirNamesLen = len(dirNames); if (dirNamesLen != 1) {
                t.Fatalf("unexpected dirNames len, got %d, want %d", dirNamesLen, (nint)1);
            }

            if (dirNames[0] != to) {
                t.Errorf("unexpected name, got %q, want %q", dirNames[0], to);
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
            if (err != nil) {
                t.Fatalf("Pipe: %v", err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
            var attr = Ꮡ(new ProcAttr(Dir: dir, Files: [default, w, Stderr]));
            (var p, err) = StartProcess(cmd, argsʗ1, attr);
            if (err != nil) {
                t.Fatalf("StartProcess: %v", err);
            }
            w.Close();

            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            Δio.Copy(new os_test_package.strings_BuilderжWriter(Ꮡb), new os_test_package.os_FileжReader(r));
            @string output = b.String();

            var (fi1, _) = Stat(strings.TrimSpace(output));
            var (fi2, _) = Stat(expect);
            if (!SameFile(fi1, fi2)) {
                t.Errorf("exec %q returned %q wanted %q",
                    strings.Join(appendꓸꓸꓸ(slice<@string>([cmd]), argsʗ1), " "), output, expect);
            }
            p.Wait();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object androidDoesnTHaveBinPwdˢ = (@string)"android doesn't have /bin/pwd"u8;
internal static ref readonly @string comspecˢ => ref comspecˢᵇ;
private static readonly @string comspecˢᵇ = "COMSPEC"u8;
internal static ref readonly @string pwdˢ => ref pwdˢᵇ;
private static readonly @string pwdˢᵇ = "pwd"u8;
internal static ref readonly @string absoluteˢ => ref absoluteˢᵇ;
private static readonly @string absoluteˢᵇ = "absolute"u8;
internal static ref readonly @string relativeˢ => ref relativeˢᵇ;
private static readonly @string relativeˢᵇ = "relative"u8;

public static void TestStartProcess(ж<Δtesting.T> t) {
    testenv.MustHaveExec(new os_test_package.testing_TжTB(t));
    t.Parallel();

    @string dir = default;
    @string cmd = default;
    slice<@string> args = default;
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8: {
        t.Skip(androidDoesnTHaveBinPwdˢ);
        break;
    }
    case {} when exprᴛ1 == "windows"u8: {
        cmd = Getenv(comspecˢ);
        dir = Getenv(systemRootˢ);
        args = ["/c", "cd"];
        break;
    }
    default: {
        error err = default;
        (cmd, err) = exec.LookPath(pwdˢ);
        if (err != nil) {
            t.Fatalf("Can't find pwd: %v", err);
        }
        dir = "/";
        args = [];
        t.Logf("Testing with %v", cmd);
        break;
    }}
    var (cmddir, cmdbase) = filepath.Split(cmd);
    args = appendꓸꓸꓸ(slice<@string>([cmdbase]), args);
    t.Run(absoluteˢ, testStartProcess(dir, cmd, args, dir));
    t.Run(relativeˢ, testStartProcess(cmddir, cmdbase, args, cmddir));
}

internal static void checkMode(ж<Δtesting.T> t, @string path, fs.FileMode mode) {
    var (dir, err) = Stat(path);
    if (err != nil) {
        t.Fatalf("Stat %q (looking for mode %#o): %s", path, mode, err);
    }
    if ((fs.FileMode)(dir.Mode() & ModePerm) != mode) {
        t.Errorf("Stat %q: mode %#o want %#o", path, dir.Mode(), mode);
    }
}

public static void TestChmod(ж<Δtesting.T> t) {
    // Chmod is not supported on wasip1.
    if (Δruntime.GOOS == "wasip1"u8) {
        t.Skip("Chmod is not supported on " + Δruntime.GOOS);
    }
    t.Parallel();

    var f = newFile(t);
    // Creation mode is read write

    var fm = (fs.FileMode)302;
    if (Δruntime.GOOS == "windows"u8) {
        fm = (fs.FileMode)292; // read-only file
    }
    {
        var err = Chmod(f.Name(), fm); if (err != nil) {
            t.Fatalf("chmod %s %#o: %s", f.Name(), fm, err);
        }
    }
    checkMode(t, f.Name(), fm);

    fm = (fs.FileMode)83;
    if (Δruntime.GOOS == "windows"u8) {
        fm = (fs.FileMode)438; // read-write file
    }
    {
        var err = f.Chmod(fm); if (err != nil) {
            t.Fatalf("chmod %s %#o: %s", f.Name(), fm, err);
        }
    }
    checkMode(t, f.Name(), fm);
}

internal static void checkSize(ж<Δtesting.T> t, ж<Δos.File> Ꮡf, int64 size) {
    ref var f = ref Ꮡf.DerefOrNull();

    t.Helper();
    var (dir, err) = Ꮡf.Stat();
    if (err != nil) {
        t.Fatalf("Stat %q (looking for size %d): %s", f.Name(), size, err);
    }
    if (dir.Size() != size) {
        t.Errorf("Stat %q: size %d want %d", f.Name(), dir.Size(), size);
    }
}

public static void TestFTruncate(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    checkSize(t, f, 0);
    f.Write(slice<byte>("hello, world\n"u8));
    checkSize(t, f, 13);
    f.Truncate(10);
    checkSize(t, f, 10);
    f.Truncate(1024);
    checkSize(t, f, 1024);
    f.Truncate(0);
    checkSize(t, f, 0);
    var (_, err) = f.Write(slice<byte>("surprise!"u8));
    if (err == nil) {
        checkSize(t, f, 13 + 9); // wrote at offset past where hello, world was.
    }
}

public static void TestTruncate(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    checkSize(t, f, 0);
    f.Write(slice<byte>("hello, world\n"u8));
    checkSize(t, f, 13);
    Truncate(f.Name(), 10);
    checkSize(t, f, 10);
    Truncate(f.Name(), 1024);
    checkSize(t, f, 1024);
    Truncate(f.Name(), 0);
    checkSize(t, f, 0);
    var (_, err) = f.Write(slice<byte>("surprise!"u8));
    if (err == nil) {
        checkSize(t, f, 13 + 9); // wrote at offset past where hello, world was.
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nonexistentˢ => ref nonexistentˢᵇ;
private static readonly @string nonexistentˢᵇ = "nonexistent"u8;

public static partial void TestTruncateNonexistentFile(ж<Δtesting.T> t) {
    t.Parallel();

    void assertPathError(Δtesting.TB tΔ1, @string pathΔ1, error errΔ1) {
        tΔ1.Helper();
        var (pe, ok) = errΔ1._<ж<fs.PathError>>(ᐧ); if (!ok || !IsNotExist(errΔ1) || pe.Path != pathΔ1) {
            tΔ1.Errorf("got error: %v\nwant an ErrNotExist PathError with path %q", errΔ1, pathΔ1);
        }
    }

    @string path = filepath.Join(t.TempDir(), nonexistentˢ);

    var err = Truncate(path, 1);
    assertPathError(new os_test_package.testing_TжTB(t), path, err);

    // Truncate shouldn't create any new file.
    (_, err) = Stat(path);
    assertPathError(new os_test_package.testing_TжTB(t), path, err);
}

internal static Func<bool> hasNoatime = Δsync.OnceValue(bool () => {
    // A sloppy way to check if noatime flag is set (as all filesystems are
    // checked, not just the one we're interested in). A correct way
    // would be to use statvfs syscall and check if flags has ST_NOATIME,
    // but the syscall is OS-specific and is not even wired into Go stdlib.
    //
    // Only used on NetBSD (which ignores explicit atime updates with noatime).
    if (Δruntime.GOOS != "netbsd"u8) {
        return false;
    }
    var (mounts, _) = ReadFile("/proc/mounts");
    return bytes.Contains(mounts, slice<byte>("noatime"u8));
});

public static void TestChtimes(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);
    // This should be an empty file (see #68687, #68663).
    f.Close();

    testChtimes(t, f.Name());
}

public static void TestChtimesOmit(ж<Δtesting.T> t) {
    t.Parallel();

    testChtimesOmit(t, true, false);
    testChtimesOmit(t, false, true);
    testChtimesOmit(t, true, true);
    testChtimesOmit(t, false, false); // Same as TestChtimes.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object knownDragonFlyBsdIssueˢ = (@string)"Known DragonFly BSD issue (won't work when both times are omitted); ignoring."u8;
internal static readonly object knownDragonFlyBsdIssueˢ2 = (@string)"Known DragonFly BSD issue (atime not supported on hammer2); ignoring."u8;
internal static readonly object knownNetBSDIssueAtimeNotˢ = (@string)"Known NetBSD issue (atime not changed on fs mounted with noatime); ignoring."u8;

internal static void testChtimesOmit(ж<Δtesting.T> t, bool omitAt, bool omitMt) {
    t.Logf("omit atime: %v, mtime: %v", omitAt, omitMt);
    var @file = newFile(t);
    // This should be an empty file (see #68687, #68663).
    @string name = @file.Name();
    var err = @file.Close();
    if (err != nil) {
        t.Error(err);
    }
    (var fs, err) = Stat(name);
    if (err != nil) {
        t.Fatal(err);
    }

    var wantAtime = os_internal_test_package.Atime(fs);
    var wantMtime = fs.ModTime();
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8: {
        wantAtime = wantAtime.Truncate(time.ΔSecond);
        wantMtime = wantMtime.Truncate(time.ΔSecond);
        break;
    }}

    time.Time setAtime = default; // Zero value means omit.
    time.Time setMtime = default;
    if (!omitAt) {
        wantAtime = wantAtime.Add(-1 * time.ΔSecond);
        setAtime = wantAtime;
    }
    if (!omitMt) {
        wantMtime = wantMtime.Add(-1 * time.ΔSecond);
        setMtime = wantMtime;
    }

    // Change the times accordingly.
    var errΔ1 = Chtimes(name, setAtime, setMtime); if (errΔ1 != nil) {
        t.Error(errΔ1);
    }

    // Verify the expectations.
    (fs, err) = Stat(name);
    if (err != nil) {
        t.Error(err);
    }
    var gotAtime = os_internal_test_package.Atime(fs);
    var gotMtime = fs.ModTime();

    // TODO: remove the dragonfly omitAt && omitMt exceptions below once the
    // fix (https://github.com/DragonFlyBSD/DragonFlyBSD/commit/c7c71870ed0)
    // is available generally and on CI runners.
    if (!gotAtime.Equal(wantAtime)) {
        @string errormsg = fmt.Sprintf("atime mismatch, got: %q, want: %q"u8, gotAtime, wantAtime);
        var exprᴛ2 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ2 == "plan9"u8: {
            // Mtime is the time of the last change of content.
            // Similarly, atime is set whenever the contents are
            // accessed; also, it is set whenever mtime is set.
            break;
        }
        case {} when exprᴛ2 == "dragonfly"u8: {
            if (omitAt && omitMt) {
                t.Log(errormsg);
                t.Log(knownDragonFlyBsdIssueˢ);
            } else {
                // Assume hammer2 fs; https://www.dragonflybsd.org/hammer/ says:
                // > Because HAMMER2 is a block copy-on-write filesystem,
                // > the "atime" field is not supported and will typically
                // > just reflect local system in-memory caches or mtime.
                //
                // TODO: if only can CI define TMPDIR to point to a tmpfs
                // (e.g. /var/run/shm), this exception can be removed.
                t.Log(errormsg);
                t.Log(knownDragonFlyBsdIssueˢ2);
            }
            break;
        }
        case {} when exprᴛ2 == "netbsd"u8: {
            if (!omitAt && hasNoatime()) {
                t.Log(errormsg);
                t.Log(knownNetBSDIssueAtimeNotˢ);
            } else {
                t.Error(errormsg);
            }
            break;
        }
        default: {
            t.Error(errormsg);
            break;
        }}
    }
    if (!gotMtime.Equal(wantMtime)) {
        @string errormsg = fmt.Sprintf("mtime mismatch, got: %q, want: %q"u8, gotMtime, wantMtime);
        var exprᴛ3 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ3 == "dragonfly"u8: {
            if (omitAt && omitMt) {
                t.Log(errormsg);
                t.Log(knownDragonFlyBsdIssueˢ);
            } else {
                t.Error(errormsg);
            }
            break;
        }
        default: {
            t.Error(errormsg);
            break;
        }}
    }
}

public static void TestChtimesDir(ж<Δtesting.T> t) {
    t.Parallel();

    testChtimes(t, t.TempDir());
}

internal static void testChtimes(ж<Δtesting.T> t, @string name) {
    var (st, err) = Stat(name);
    if (err != nil) {
        t.Fatalf("Stat %s: %s", name, err);
    }
    var preStat = st;

    // Move access and modification time back a second
    var at = os_internal_test_package.Atime(preStat);
    var mt = preStat.ModTime();
    err = Chtimes(name, at.Add(-time.ΔSecond), mt.Add(-time.ΔSecond));
    if (err != nil) {
        t.Fatalf("Chtimes %s: %s", name, err);
    }

    (st, err) = Stat(name);
    if (err != nil) {
        t.Fatalf("second Stat %s: %s", name, err);
    }
    var postStat = st;

    var pat = os_internal_test_package.Atime(postStat);
    var pmt = postStat.ModTime();
    if (!pat.Before(at)) {
        @string errormsg = fmt.Sprintf("AccessTime didn't go backwards; was=%v, after=%v"u8, at, pat);
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "plan9"u8: {
            // Mtime is the time of the last change of
            // content.  Similarly, atime is set whenever
            // the contents are accessed; also, it is set
            // whenever mtime is set.
            break;
        }
        case {} when exprᴛ1 == "netbsd"u8: {
            if (hasNoatime()) {
                t.Log(errormsg);
                t.Log(knownNetBSDIssueAtimeNotˢ);
            } else {
                t.Error(errormsg);
            }
            break;
        }
        default: {
            t.Error(errormsg);
            break;
        }}
    }

    if (!pmt.Before(mt)) {
        t.Errorf("ModTime didn't go backwards; was=%v, after=%v", mt, pmt);
    }
}

public static void TestChtimesToUnixZero(ж<Δtesting.T> t) {
    var @file = newFile(t);
    @string fn = @file.Name();
    var (_, errΔ1) = @file.Write(slice<byte>("hi"u8)); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var errΔ2 = @file.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }

    var unixZero = time.Unix(0, 0);
    var errΔ3 = Chtimes(fn, unixZero, unixZero); if (errΔ3 != nil) {
        t.Fatalf("Chtimes failed: %v", errΔ3);
    }

    var (st, err) = Stat(fn);
    if (err != nil) {
        t.Fatal(err);
    }

    var mt = st.ModTime(); if (mt != unixZero) {
        t.Errorf("mtime is %v, want %v", mt, unixZero);
    }
}

public static void TestFileChdir(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var (wd, err) = Getwd();
        if (err != nil) {
            t.Fatalf("Getwd: %s", err);
        }
        t.Chdir("."); // Ensure wd is restored after the test.

        (var fd, err) = Open(".");
        if (err != nil) {
            t.Fatalf("Open .: %s", err);
        }
        var fdʗ1 = fd;
        defer(() => fdʗ1.Close(), ref ᒐ);

        var errΔ1 = Chdir("/"); if (errΔ1 != nil) {
            t.Fatalf("Chdir /: %s", errΔ1);
        }

        var errΔ2 = fd.Chdir(); if (errΔ2 != nil) {
            t.Fatalf("fd.Chdir: %s", errΔ2);
        }

        (var wdNew, err) = Getwd();
        if (err != nil) {
            t.Fatalf("Getwd: %s", err);
        }

        (var wdInfo, err) = fd.Stat();
        if (err != nil) {
            t.Fatal(err);
        }
        (var newInfo, err) = Stat(wdNew);
        if (err != nil) {
            t.Fatal(err);
        }
        if (!SameFile(wdInfo, newInfo)) {
            t.Fatalf("fd.Chdir failed: got %s, want %s", wdNew, wd);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goTestFileChdirTestlogˢ => ref goTestFileChdirTestlogˢᵇ;
private static readonly @string goTestFileChdirTestlogˢᵇ = "GO_TEST_FILE_CHDIR_TESTLOG"u8;
internal static ref readonly @string testlogTxtˢ => ref testlogTxtˢᵇ;
private static readonly @string testlogTxtˢᵇ = "testlog.txt"u8;
internal static ref readonly @string testRunˢ2 => ref testRunˢ2ᵇ;
private static readonly @string testRunˢ2ᵇ = "-test.run=^TestFileChdirTestlog$"u8;
internal static ref readonly @string chdirˢ => ref chdirˢᵇ;
private static readonly @string chdirˢᵇ = "chdir "u8;

// TestFileChdirTestlog verifies that (*File).Chdir notifies the testlog,
// just like os.Chdir does. cmd/go's test cache relies on every working
// directory change being recorded so that relative paths logged by later
// Open/Stat calls can be resolved against the correct directory.
public static void TestFileChdirTestlog(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        if (Getenv(goTestFileChdirTestlogˢ) == "1"u8) {
            var (fd, errΔ1) = Open(".");
            if (errΔ1 != nil) {
                t.Fatalf("Open .: %s", errΔ1);
            }
            var fdʗ1 = fd;
            defer(() => fdʗ1.Close(), ref ᒐ);
            var errΔ2 = Chdir(TempDir()); if (errΔ2 != nil) {
                t.Fatalf("Chdir: %s", errΔ2);
            }
            var errΔ3 = fd.Chdir(); if (errΔ3 != nil) {
                t.Fatalf("fd.Chdir: %s", errΔ3);
            }
            return;
        }

        testenv.MustHaveExec(new os_test_package.testing_TжTB(t));
        @string exe = testenv.Executable(new os_test_package.testing_TжTB(t));
        @string logfile = filepath.Join(t.TempDir(), testlogTxtˢ);
        var cmd = testenv.Command(new os_test_package.testing_TжTB(t), exe,
            testRunˢ2,
            "-test.testlogfile=" + logfile);
        cmd = testenv.CleanCmdEnv(cmd);
        cmd.Env = append(cmd.Env, (@string)"GO_TEST_FILE_CHDIR_TESTLOG=1");
        var (@out, errΔ4) = cmd.CombinedOutput(); if (errΔ4 != nil) {
            t.Fatalf("helper failed: %v\n%s", errΔ4, @out);
        }

        var (data, err) = ReadFile(logfile);
        if (err != nil) {
            t.Fatal(err);
        }
        nint chdirs = 0;
        foreach (var (_, line) in range(strings.Split((@string)data, "\n"))) {
            if (strings.HasPrefix(line, chdirˢ)) {
                chdirs++;
            }
        }
        if (chdirs < 2) {
            t.Fatalf("got %d chdir testlog entries, want at least 2; testlog:\n%s", chdirs, data);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pwdˢ2 => ref pwdˢ2ᵇ;
private static readonly @string pwdˢ2ᵇ = "PWD"u8;
internal static ref readonly @string tmpˢ => ref tmpˢᵇ;
private static readonly @string tmpˢᵇ = "/tmp"u8;

public static void TestChdirAndGetwd(ж<Δtesting.T> t) {
    var dirsᴷ = new stackAppend<@string>();

    t.Chdir(t.TempDir()); // Ensure wd is restored after the test.

    // These are chosen carefully not to be symlinks on a Mac
    // (unlike, say, /var, /etc), except /tmp, which we handle below.
    var dirs = slice<@string>(["/", "/usr/bin", "/tmp"]);
    // /usr/bin does not usually exist on Plan 9 or Android.
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8: {
        dirs = ["/system/bin"];
        break;
    }
    case {} when exprᴛ1 == "plan9"u8: {
        dirs = ["/", "/usr"];
        break;
    }
    case {} when exprᴛ1 == "ios"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "wasip1"u8: {
        dirs = default;
        foreach (var (_, dir) in range(slice<@string>([t.TempDir(), t.TempDir()]))) {
            // Expand symlinks so path equality tests work.
            var (dirΔ1, err) = filepath.EvalSymlinks(dir);
            if (err != nil) {
                t.Fatalf("EvalSymlinks: %v", err);
            }
            dirs = append(dirsᴷ, dirs, dirΔ1);
        }
        break;
    }}
    for (nint mode = 0; mode < 2; mode++) {
        foreach (var (_, d) in range(dirs)) {
            error err = default;
            if (mode == 0) {
                err = Chdir(d);
            } else {
                var (fd1, err1Δ1) = Open(d);
                if (err1Δ1 != nil) {
                    t.Errorf("Open %s: %s", d, err1Δ1);
                    continue;
                }
                err = fd1.Chdir();
                fd1.Close();
            }
            if (d == "/tmp"u8) {
                Setenv(pwdˢ2, tmpˢ);
            }
            var (pwd, err1) = Getwd();
            if (err != nil) {
                t.Fatalf("Chdir %s: %s", d, err);
            }
            if (err1 != nil) {
                t.Fatalf("Getwd in %s: %s", d, err1);
            }
            if (!equal(pwd, d)) {
                t.Fatalf("Getwd returned %q want %q", pwd, d);
            }
        }
    }
}

// Test that Chdir+Getwd is program-wide.
public static partial void TestProgWideChdir(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        const nint N = 10;
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        var hold = new channel<EmptyStruct>(0);
        var done = new channel<EmptyStruct>(0);

        @string d = t.TempDir();
        t.Chdir(d);

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
                    var (f0, errΔ1) = Stat(".");
                    if (errΔ1 != nil) {
                        t.Error(errΔ1);
                        return;
                    }
                    (var pwd, errΔ1) = Getwd();
                    if (errΔ1 != nil) {
                        t.Errorf("Getwd: %v", errΔ1);
                        return;
                    }
                    if (pwd != d) {
                        t.Errorf("Getwd() = %q, want %q", pwd, d);
                        return;
                    }
                    (var f1, errΔ1) = Stat(pwd);
                    if (errΔ1 != nil) {
                        t.Error(errΔ1);
                        return;
                    }
                    if (!SameFile(f0, f1)) {
                        t.Errorf(@"Samefile(Stat("".""), Getwd()) reports false (%s != %s)", f0.Name(), f1.Name());
                        return;
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, i);
        }
        error err = default;
        err = Chdir(d); if (err != nil) {
            t.Fatalf("Chdir: %v", err);
        }
        // OS X sets TMPDIR to a symbolic link.
        // So we resolve our working directory again before the test.
        (d, err) = Getwd();
        if (err != nil) {
            t.Fatalf("Getwd: %v", err);
        }
        close(hold);
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ => ref dataᶜᵇ;
private static readonly @string dataᶜᵇ = "hello, world\n"u8;

internal partial struct TestSeek_test /*dyn*/ {
    internal int64 @in;
    internal nint whence;
    internal int64 @out;
}

public static void TestSeek(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    @string data = dataᶜ;
    Δio.WriteString(new Δos.FileжWriter(f), data);
    slice<TestSeek_test> tests = slice<TestSeek_test>([
        new(0, Δio.SeekCurrent, (int64)len(data)),
        new(0, Δio.SeekStart, 0),
        new(5, Δio.SeekStart, 5),
        new(0, Δio.SeekEnd, (int64)len(data)),
        new(0, Δio.SeekStart, 0),
        new(-1, Δio.SeekEnd, (int64)len(data) - 1),
        new(8589934592L, Δio.SeekStart, 8589934592L),
        new(8589934592L, Δio.SeekEnd, 8589934605L),

        // Issue 21681, Windows 4G-1, etc:
        new(4294967295L, Δio.SeekStart, 4294967295L),
        new(0, Δio.SeekCurrent, 4294967295L),
        new(8589934591L, Δio.SeekStart, 8589934591L),
        new(0, Δio.SeekCurrent, 8589934591L)
    ]);
    foreach (var (i, tt) in range(tests)) {
        var (off, err) = f.Seek(tt.@in, tt.whence);
        if (off != tt.@out || err != nil) {
            t.Errorf("#%d: Seek(%v, %v) = %v, %v want %v, nil", i, tt.@in, tt.whence, off, err, tt.@out);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object seekOnPipeShouldFailˢ = (@string)"Seek on pipe should fail"u8;

public static void TestSeekError(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skipf("skipping test on %v", Δruntime.GOOS);
        break;
    }}
    t.Parallel();

    var (r, w, err) = Pipe();
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = r.Seek(0, 0);
    if (err == nil) {
        t.Fatal(seekOnPipeShouldFailˢ);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !AreEqual(perr.Err, syscall.ESPIPE)) {
            t.Errorf("Seek returned error %v, want &PathError{Err: syscall.ESPIPE}", err);
        }
    }
    (_, err) = w.Seek(0, 0);
    if (err == nil) {
        t.Fatal(seekOnPipeShouldFailˢ);
    }
    {
        var (perr, ok) = err._<ж<fs.PathError>>(ᐧ); if (!ok || !AreEqual(perr.Err, syscall.ESPIPE)) {
            t.Errorf("Seek returned error %v, want &PathError{Err: syscall.ESPIPE}", err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noRootˢ => ref noRootˢᵇ;
private static readonly @string noRootˢᵇ = "NoRoot"u8;
internal static ref readonly @string inRootˢ => ref inRootˢᵇ;
private static readonly @string inRootˢᵇ = "InRoot"u8;

public static void TestOpenError(ж<Δtesting.T> t) {
    t.Parallel();
    @string dir = makefs(t, [
        "is-a-file",
        "is-a-dir/"
    ]);
    t.Run(noRootˢ, (ж<Δtesting.T> tΔ1) => {
        testOpenError(tΔ1, dir, false);
    });
    t.Run(inRootˢ, (ж<Δtesting.T> tΔ2) => {
        testOpenError(tΔ2, dir, true);
    });
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileˢ => ref fileˢᵇ;
private static readonly @string fileˢᵇ = "file "u8;

internal partial struct testOpenError_type /*dyn*/ {
    internal @string path;
    internal nint mode;
    internal error error;
}

internal static void testOpenError(ж<Δtesting.T> t, @string dir, bool rooted) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();
        ж<Δos.Root> r = default;
        if (rooted) {
            error err = default;
            (r, err) = OpenRoot(dir);
            if (err != nil) {
                t.Fatal(err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
        }
        foreach (var (_, tt) in range(slice<testOpenError_type>([new(
            "no-such-file",
            O_RDONLY,
            syscall.ENOENT
        ), new(
            "is-a-dir",
            O_WRONLY,
            syscall.EISDIR
        ), new(
            "is-a-file/no-such-file",
            O_WRONLY,
            syscall.ENOTDIR
        )
        ]))) {
            ж<Δos.File> f = default;
            error err = default;
            @string name = default;
            if (rooted) {
                name = fmt.Sprintf("Root(%q).OpenFile(%q, %d)"u8, dir, tt.path, tt.mode);
                (f, err) = r.OpenFile(tt.path, tt.mode, 0);
            } else {
                @string path = filepath.Join(dir, tt.path);
                name = fmt.Sprintf("OpenFile(%q, %d)"u8, path, tt.mode);
                (f, err) = OpenFile(path, tt.mode, 0);
            }
            if (err == nil) {
                t.Errorf("%v succeeded", name);
                f.Close();
                continue;
            }
            var (perr, ok) = err._<ж<fs.PathError>>(ᐧ);
            if (!ok) {
                t.Errorf("%v returns error of %T type; want *PathError", name, err);
            }
            if (!AreEqual(perr.Err, tt.error)) {
                if (Δruntime.GOOS == "plan9"u8) {
                    @string syscallErrStr = perr.Err.Error();
                    @string expectedErrStr = strings.Replace(tt.error.Error(), fileˢ, ""u8, 1);
                    if (!strings.HasSuffix(syscallErrStr, expectedErrStr)) {
                        // Some Plan 9 file servers incorrectly return
                        // EPERM or EACCES rather than EISDIR when a directory is
                        // opened for write.
                        if (AreEqual(tt.error, syscall.EISDIR) && (strings.HasSuffix(syscallErrStr, syscall.EPERM.Error()) || strings.HasSuffix(syscallErrStr, syscall.EACCES.Error()))) {
                            continue;
                        }
                        t.Errorf("%v = _, %q; want suffix %q", name, syscallErrStr, expectedErrStr);
                    }
                    continue;
                }
                if (Δruntime.GOOS == "dragonfly"u8) {
                    // DragonFly incorrectly returns EACCES rather
                    // EISDIR when a directory is opened for write.
                    if (AreEqual(tt.error, syscall.EISDIR) && AreEqual(perr.Err, syscall.EACCES)) {
                        continue;
                    }
                }
                t.Errorf("%v = _, %q; want %q", name, perr.Err.Error(), tt.error.Error());
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object openSucceededˢ = (@string)@"Open("""") succeeded"u8;

public static void TestOpenNoName(ж<Δtesting.T> t) {
    var (f, err) = Open(""u8);
    if (err == nil) {
        f.Close();
        t.Fatal(openSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hostnameˢ => ref hostnameˢᵇ;
private static readonly @string hostnameˢᵇ = "hostname"u8;
internal static readonly object skippingTestTestRequiresˢ = (@string)"skipping test; test requires hostname but it does not exist"u8;

internal static @string runBinHostname(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        // Run /bin/hostname and collect output.
        var (r, w, err) = Pipe();
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);

        (var path, err) = exec.LookPath(hostnameˢ);
        if (err != nil) {
            if (errors.Is(err, exec.ErrNotFound)) {
                t.Skip(skippingTestTestRequiresˢ);
            }
            t.Fatal(err);
        }

        var argv = slice<@string>(["hostname"]);
        if (Δruntime.GOOS == "aix"u8) {
            argv = ["hostname", "-s"];
        }
        (var p, err) = StartProcess(path, argv, Ꮡ(new ProcAttr(Files: [default, w, Stderr])));
        if (err != nil) {
            t.Fatal(err);
        }
        w.Close();

        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Δio.Copy(new os_test_package.strings_BuilderжWriter(Ꮡb), new os_test_package.os_FileжReader(r));
        (_, err) = p.Wait();
        if (err != nil) {
            t.Fatalf("run hostname Wait: %v", err);
        }
        err = p.Kill();
        if (err == nil) {
            t.Errorf("expected an error from Kill running 'hostname'");
        }
        @string output = b.String();
        nint n = len(output); if (n > 0 && output[n - 1] == '\n') {
            output = output.slice(0, n - 1);
        }
        if (output == ""u8) {
            t.Fatalf("/bin/hostname produced no output");
        }

        return output;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void testWindowsHostname(ж<Δtesting.T> t, @string hostname) {
    var cmd = testenv.Command(new os_test_package.testing_TжTB(t), hostnameˢ);
    var (@out, err) = cmd.Output();
    if (err != nil) {
        t.Fatalf("Failed to execute hostname command: %v %s", err, @out);
    }
    @string want = strings.Trim((@string)@out, "\r\n");
    if (hostname != want) {
        t.Fatalf("Hostname() = %q != system hostname of %q", hostname, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object hostnameReturnedEmptyˢ = (@string)"Hostname returned empty string and no error"u8;

public static void TestHostname(ж<Δtesting.T> t) {
    t.Parallel();

    var (hostname, err) = Hostname();
    if (err != nil) {
        t.Fatal(err);
    }
    if (hostname == ""u8) {
        t.Fatal(hostnameReturnedEmptyˢ);
    }
    if (strings.Contains(hostname, "\x00")) {
        t.Fatalf("unexpected zero byte in hostname: %q", hostname);
    }

    // There is no other way to fetch hostname on windows, but via winapi.
    // On Plan 9 it can be taken from #c/sysname as Hostname() does.
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8: {
        // No /bin/hostname to verify against.
        return;
    }
    case {} when exprᴛ1 == "windows"u8: {
        testWindowsHostname(t, hostname);
        return;
    }}

    testenv.MustHaveExec(new os_test_package.testing_TжTB(t));

    // Check internal Hostname() against the output of /bin/hostname.
    // Allow that the internal Hostname returns a Fully Qualified Domain Name
    // and the /bin/hostname only returns the first component
    @string want = runBinHostname(t);
    if (hostname != want) {
        var (host, _, ok) = strings.Cut(hostname, ".");
        if (!ok || host != want) {
            t.Errorf("Hostname() = %q, want %q", hostname, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object worldˢ = (@string)"world"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ1 => ref dataᶜ1ᵇ;
private static readonly @string dataᶜ1ᵇ = "hello, world\n"u8;

public static void TestReadAt(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    @string data = dataᶜ1;
    Δio.WriteString(new Δos.FileжWriter(f), data);

    var b = new slice<byte>(5);
    var (n, err) = f.ReadAt(b, 7);
    if (err != nil || n != len(b)) {
        t.Fatalf("ReadAt 7: %d, %v", n, err);
    }
    if (((sstring)b) != "world"u8) {
        t.Fatalf("ReadAt 7: have %q want %q", (@string)b, worldˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ2 => ref dataᶜ2ᵇ;
private static readonly @string dataᶜ2ᵇ = "hello, world\n"u8;

// Verify that ReadAt doesn't affect seek offset.
// In the Plan 9 kernel, there used to be a bug in the implementation of
// the pread syscall, where the channel offset was erroneously updated after
// calling pread on a file.
public static void TestReadAtOffset(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    @string data = dataᶜ2;
    Δio.WriteString(new Δos.FileжWriter(f), data);

    f.Seek(0, 0);
    var b = new slice<byte>(5);

    var (n, err) = f.ReadAt(b, 7);
    if (err != nil || n != len(b)) {
        t.Fatalf("ReadAt 7: %d, %v", n, err);
    }
    if (((sstring)b) != "world"u8) {
        t.Fatalf("ReadAt 7: have %q want %q", (@string)b, worldˢ);
    }

    (n, err) = f.Read(b);
    if (err != nil || n != len(b)) {
        t.Fatalf("Read: %d, %v", n, err);
    }
    if (((sstring)b) != "hello"u8) {
        t.Fatalf("Read: have %q want %q", (@string)b, helloˢ);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ3 => ref dataᶜ3ᵇ;
private static readonly @string dataᶜ3ᵇ = "hello, world\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantsubᶜ => ref wantsubᶜᵇ;
private static readonly @string wantsubᶜᵇ = "negative offset"u8;

// Verify that ReadAt doesn't allow negative offset.
public static void TestReadAtNegativeOffset(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    @string data = dataᶜ3;
    Δio.WriteString(new Δos.FileжWriter(f), data);

    f.Seek(0, 0);
    var b = new slice<byte>(5);

    var (n, err) = f.ReadAt(b, -10);

    @string wantsub = wantsubᶜ;
    if (!strings.Contains(fmt.Sprint(err), wantsub) || n != 0) {
        t.Errorf("ReadAt(-10) = %v, %v; want 0, ...%q...", n, err, wantsub);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWORldˢ => ref helloWORldˢᵇ;
private static readonly @string helloWORldˢᵇ = "hello, WORld!"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ4 => ref dataᶜ4ᵇ;
private static readonly @string dataᶜ4ᵇ = "hello, world"u8;

public static void TestWriteAt(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    @string data = dataᶜ4;
    Δio.WriteString(new Δos.FileжWriter(f), data);

    var (n, err) = f.WriteAt(slice<byte>("WOR"u8), 7);
    if (err != nil || n != 3) {
        t.Fatalf("WriteAt 7: %d, %v", n, err);
    }
    (n, err) = Δio.WriteString(new Δos.FileжWriter(f), "!"); // test that WriteAt doesn't change the file offset
    if (err != nil || n != 1) {
        t.Fatal(err);
    }

    (var got, err) = ReadFile(f.Name());
    if (err != nil) {
        t.Fatalf("ReadFile %s: %v", f.Name(), err);
    }
    @string want = helloWORldˢ;
    if (((sstring)got) != want) {
        t.Fatalf("after write: have %q want %q", (@string)got, want);
    }
}

public static partial void TestWriteAtConcurrent(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);
    Δio.WriteString(new Δos.FileжWriter(f), "0000000000");

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    foreach (var i in range(10)) {
        Ꮡwg.Add(1);
        var fʗ1 = f;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (n, errΔ1) = fʗ1.WriteAt(slice<byte>(strconv.Itoa(i)), (int64)i);
                if (errΔ1 != nil || n != 1) {
                    t.Errorf("WriteAt %d: %d, %v", i, n, errΔ1);
                }
                (n, errΔ1) = Δio.WriteString(new Δos.FileжWriter(fʗ1), "!"); // test that WriteAt doesn't change the file offset
                if (errΔ1 != nil || n != 1) {
                    t.Error(errΔ1);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();

    var (got, err) = ReadFile(f.Name());
    if (err != nil) {
        t.Fatalf("ReadFile %s: %v", f.Name(), err);
    }
    @string want = "0123456789!!!!!!!!!!";
    if (((sstring)got) != want) {
        t.Fatalf("after write: have %q want %q", (@string)got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantsubᶜ1 => ref wantsubᶜ1ᵇ;
private static readonly @string wantsubᶜ1ᵇ = "negative offset"u8;

// Verify that WriteAt doesn't allow negative offset.
public static void TestWriteAtNegativeOffset(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    var (n, err) = f.WriteAt(slice<byte>("WORLD"u8), -10);

    @string wantsub = wantsubᶜ1;
    if (!strings.Contains(fmt.Sprint(err), wantsub) || n != 0) {
        t.Errorf("WriteAt(-10) = %v, %v; want 0, ...%q...", n, err, wantsub);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeAtInAppendModeTxtˢ => ref writeAtInAppendModeTxtˢᵇ;
private static readonly @string writeAtInAppendModeTxtˢᵇ = "write_at_in_append_mode.txt"u8;

// Verify that WriteAt doesn't work in append mode.
public static void TestWriteAtInAppendMode(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Chdir(t.TempDir());
        var (f, err) = OpenFile(writeAtInAppendModeTxtˢ, (nint)(O_APPEND | O_CREATE), 438);
        if (err != nil) {
            t.Fatalf("OpenFile: %v", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (_, err) = f.WriteAt(slice<byte>(""u8), 1);
        if (!AreEqual(err, os_internal_test_package.ErrWriteAtInAppendMode)) {
            t.Fatalf("f.WriteAt returned %v, expected %v", err, os_internal_test_package.ErrWriteAtInAppendMode);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string writeFile(ж<Δtesting.T> t, ж<Δos.Root> r, @string fname, nint flag, @string text) {
    t.Helper();
    ж<Δos.File> f = default;
    error err = default;
    if (r == nil) {
        (f, err) = OpenFile(fname, flag, 438);
    } else {
        (f, err) = r.OpenFile(fname, flag, 438);
    }
    if (err != nil) {
        t.Fatalf("Open: %v", err);
    }
    (var n, err) = Δio.WriteString(new Δos.FileжWriter(f), text);
    if (err != nil) {
        t.Fatalf("WriteString: %d, %v", n, err);
    }
    f.Close();
    (var data, err) = ReadFile(fname);
    if (err != nil) {
        t.Fatalf("ReadFile: %v", err);
    }
    return (@string)data;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string newˢ => ref newˢᵇ;
private static readonly @string newˢᵇ = "new"u8;
internal static ref readonly @string appendˢ => ref appendˢᵇ;
private static readonly @string appendˢᵇ = "|append"u8;
internal static readonly object newAppendˢ = (@string)"new|append"u8;
internal static readonly object newAppendAppendˢ = (@string)"new|append|append"u8;
internal static ref readonly @string newAppendˢ2 => ref newAppendˢ2ᵇ;
private static readonly @string newAppendˢ2ᵇ = "new&append"u8;
internal static ref readonly @string oldˢ => ref oldˢᵇ;
private static readonly @string oldˢᵇ = "old"u8;
internal static readonly object oldAppendˢ = (@string)"old&append"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fᶜ => ref fᶜᵇ;
private static readonly @string fᶜᵇ = "append.txt"u8;

public static void TestAppend(ж<Δtesting.T> t) {
    testMaybeRooted(t, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        @string f = fᶜ;
        @string s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_TRUNC) | O_RDWR), newˢ);
        if (s != "new"u8) {
            tΔ1.Fatalf("writeFile: have %q want %q", s, newˢ);
        }
        s = writeFile(tΔ1, r, f, (nint)(O_APPEND | O_RDWR), appendˢ);
        if (s != "new|append"u8) {
            tΔ1.Fatalf("writeFile: have %q want %q", s, newAppendˢ);
        }
        s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_APPEND) | O_RDWR), appendˢ);
        if (s != "new|append|append"u8) {
            tΔ1.Fatalf("writeFile: have %q want %q", s, newAppendAppendˢ);
        }
        var err = Remove(f);
        if (err != nil) {
            tΔ1.Fatalf("Remove: %v", err);
        }
        s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_APPEND) | O_RDWR), newAppendˢ2);
        if (s != "new&append"u8) {
            tΔ1.Fatalf("writeFile: after append have %q want %q", s, newAppendˢ2);
        }
        s = writeFile(tΔ1, r, f, (nint)(O_CREATE | O_RDWR), oldˢ);
        if (s != "old&append"u8) {
            tΔ1.Fatalf("writeFile: after create have %q want %q", s, oldAppendˢ);
        }
        s = writeFile(tΔ1, r, f, (nint)((nint)(nint)(O_CREATE | O_TRUNC) | O_RDWR), newˢ);
        if (s != "new"u8) {
            tΔ1.Fatalf("writeFile: after truncate have %q want %q", s, newˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestWhenRunningˢ = (@string)"skipping test when running as root"u8;

internal partial struct TestFilePermissions_type /*dyn*/ {
    internal @string name;
    internal fs.FileMode mode;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ => ref filenameᶜᵇ;
private static readonly @string filenameᶜᵇ = "f"u8;

// TestFilePermissions tests setting Unix permission bits on file creation.
public static void TestFilePermissions(ж<Δtesting.T> t) {
    if (Getuid() == 0) {
        t.Skip(skippingTestWhenRunningˢ);
    }
    foreach (var (_, vᴛ1) in range(slice<TestFilePermissions_type>([
        new("r", 292),
        new("w", 146),
        new("rw", 438)
    ]))) {
        ref var test = ref heap(new TestFilePermissions_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<Δtesting.T> tΔ1) => {
            var exprᴛ1 = Δruntime.GOOS;
            switch (ᐧ) {
            case {} when exprᴛ1 == "windows"u8: {
                if ((fs.FileMode)(testʗ1.mode & 292) == 0) {
                    tΔ1.Skip("write-only files not supported on " + Δruntime.GOOS);
                }
                break;
            }
            case {} when exprᴛ1 == "wasip1"u8: {
                tΔ1.Skip("file permissions not supported on " + Δruntime.GOOS);
                break;
            }}
            var testʗ2 = testʗ1;
            testMaybeRooted(tΔ1, (ж<Δtesting.T> tΔ2, ж<Δos.Root> r) => {
                @string filename = filenameᶜ;
                ж<Δos.File> f = default;
                error err = default;
                if (r == nil) {
                    (f, err) = OpenFile(filename, (nint)((nint)(nint)(O_RDWR | O_CREATE) | O_EXCL), testʗ2.mode);
                } else {
                    (f, err) = r.OpenFile(filename, (nint)((nint)(nint)(O_RDWR | O_CREATE) | O_EXCL), testʗ2.mode);
                }
                if (err != nil) {
                    tΔ2.Fatal(err);
                }
                f.Close();
                (var b, err) = ReadFile(filename);
                if ((fs.FileMode)(testʗ2.mode & 292) != 0) {
                    if (err != nil) {
                        tΔ2.Errorf("ReadFile = %v; want success", err);
                    }
                } else {
                    if (err == nil) {
                        tΔ2.Errorf("ReadFile = %q, <nil>; want failure", (@string)b);
                    }
                }
                (_, err) = Stat(filename);
                if (err != nil) {
                    tΔ2.Errorf("Stat = %v; want success", err);
                }
                err = WriteFile(filename, default, 438);
                if ((fs.FileMode)(testʗ2.mode & 146) != 0) {
                    if (err != nil) {
                        tΔ2.Errorf("WriteFile = %v; want success", err);
                        var (bΔ1, errΔ1) = ReadFile(filename);
                        tΔ2.Errorf("ReadFile: %v", errΔ1);
                        tΔ2.Errorf("file contents: %q", bΔ1);
                    }
                } else {
                    if (err == nil) {
                        tΔ2.Errorf("WriteFile(%q) = <nil>; want failure", filename);
                        var (st, errΔ2) = Stat(filename);
                        if (errΔ2 == nil) {
                            tΔ2.Errorf("mode: %s", st.Mode());
                        }
                        (var bΔ2, errΔ2) = ReadFile(filename);
                        tΔ2.Errorf("ReadFile: %v", errΔ2);
                        tΔ2.Errorf("file contents: %q", bΔ2);
                    }
                }
            });
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string doesNotExistˢ => ref doesNotExistˢᵇ;
private static readonly @string doesNotExistˢᵇ = "does_not_exist"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string linkᶜ => ref linkᶜᵇ;
private static readonly @string linkᶜᵇ = "link"u8;

public static void TestOpenFileCreateExclDanglingSymlink(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    testMaybeRooted(t, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        @string link = linkᶜ;
        {
            var errΔ1 = Symlink(doesNotExistˢ, link); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
        }
        ж<Δos.File> f = default;
        error err = default;
        if (r == nil) {
            (f, err) = OpenFile(link, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_EXCL), 292);
        } else {
            (f, err) = r.OpenFile(link, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_EXCL), 292);
        }
        if (err == nil) {
            f.Close();
        }
        if (!errors.Is(err, ErrExist)) {
            tΔ1.Errorf("OpenFile of a dangling symlink with O_CREATE|O_EXCL = %v, want ErrExist", err);
        }
        {
            var (_, errΔ1) = Stat(link); if (errΔ1 == nil) {
                tΔ1.Errorf("OpenFile of a dangling symlink with O_CREATE|O_EXCL created a file");
            }
        }
    });
}

internal partial struct TestFileRDWRFlags_type /*dyn*/ {
    internal @string name;
    internal nint flag;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ1 => ref filenameᶜ1ᵇ;
private static readonly @string filenameᶜ1ᵇ = "f"u8;

// TestFileRDWRFlags tests the O_RDONLY, O_WRONLY, and O_RDWR flags.
public static void TestFileRDWRFlags(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestFileRDWRFlags_type>([
        new("O_RDONLY", O_RDONLY),
        new("O_WRONLY", O_WRONLY),
        new("O_RDWR", O_RDWR)
    ]))) {
        ref var test = ref heap(new TestFileRDWRFlags_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<Δtesting.T> tΔ1) => {
            var testʗ2 = testʗ1;
            testMaybeRooted(tΔ1, (ж<Δtesting.T> tΔ2, ж<Δos.Root> r) => {
                GoFrame ᒐ = default;
                try {
                    @string filename = filenameᶜ1;
                    var content = slice<byte>("content"u8);
                    {
                        var errΔ1 = WriteFile(filename, content, 438); if (errΔ1 != nil) {
                            tΔ2.Fatal(errΔ1);
                        }
                    }
                    ж<Δos.File> f = default;
                    error err = default;
                    if (r == nil) {
                        (f, err) = OpenFile(filename, testʗ2.flag, 0);
                    } else {
                        (f, err) = r.OpenFile(filename, testʗ2.flag, 0);
                    }
                    if (err != nil) {
                        tΔ2.Fatal(err);
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    (var got, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    if (testʗ2.flag == O_WRONLY) {
                        if (err == nil) {
                            tΔ2.Errorf("read file: %q, %v; want error", got, err);
                        }
                    } else {
                        if (err != nil || !bytes.Equal(got, content)) {
                            tΔ2.Errorf("read file: %q, %v; want %q, <nil>", got, err, content);
                        }
                    }
                    {
                        var (_, errΔ1) = f.Seek(0, 0); if (errΔ1 != nil) {
                            tΔ2.Fatalf("f.Seek: %v", errΔ1);
                        }
                    }
                    var newcontent = slice<byte>("CONTENT"u8);
                    (_, err) = f.Write(newcontent);
                    if (testʗ2.flag == O_RDONLY) {
                        if (err == nil) {
                            tΔ2.Errorf("write file: succeeded, want error");
                        }
                    } else {
                        if (err != nil) {
                            tΔ2.Errorf("write file: %v, want success", err);
                        }
                    }
                    f.Close();
                    (got, err) = ReadFile(filename);
                    if (err != nil) {
                        tΔ2.Fatal(err);
                    }
                    var want = content;
                    if (testʗ2.flag != O_RDONLY) {
                        want = newcontent;
                    }
                    if (!bytes.Equal(got, want)) {
                        tΔ2.Fatalf("after write, file contains %q, want %q", got, want);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        });
    }
}

public static void TestStatDirWithTrailingSlash(ж<Δtesting.T> t) {
    t.Parallel();

    // Create new temporary directory and arrange to clean it up.
    @string path = t.TempDir();

    // Stat of path should succeed.
    {
        var (_, err) = Stat(path); if (err != nil) {
            t.Fatalf("stat %s failed: %s", path, err);
        }
    }

    // Stat of path+"/" should succeed too.
    path += "/"u8;
    {
        var (_, err) = Stat(path); if (err != nil) {
            t.Fatalf("stat %s failed: %s", path, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nilˢ = (@string)"<nil>"u8;

public static void TestNilProcessStateString(ж<Δtesting.T> t) {
    ж<Δos.ProcessState> ps = default;
    @string s = ps.String();
    if (s != "<nil>"u8) {
        t.Errorf("(*ProcessState)(nil).String() = %q, want %q", s, nilˢ);
    }
}

public static void TestSameFile(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());
    var (fa, err) = Create("a");
    if (err != nil) {
        t.Fatalf("Create(a): %v", err);
    }
    fa.Close();
    (var fb, err) = Create("b");
    if (err != nil) {
        t.Fatalf("Create(b): %v", err);
    }
    fb.Close();

    (var ia1, err) = Stat("a");
    if (err != nil) {
        t.Fatalf("Stat(a): %v", err);
    }
    (var ia2, err) = Stat("a");
    if (err != nil) {
        t.Fatalf("Stat(a): %v", err);
    }
    if (!SameFile(ia1, ia2)) {
        t.Errorf("files should be same");
    }

    (var ib, err) = Stat("b");
    if (err != nil) {
        t.Fatalf("Stat(b): %v", err);
    }
    if (SameFile(ia1, ib)) {
        t.Errorf("files should be different");
    }
}

internal static void testDevNullFileInfo(ж<Δtesting.T> t, @string statname, @string devNullName, fs.FileInfo fi) {
    @string pre = fmt.Sprintf("%s(%q): "u8, statname, devNullName);
    if (fi.Size() != 0) {
        t.Errorf(pre + "wrong file size have %d want 0"u8, fi.Size());
    }
    if ((fs.FileMode)(fi.Mode() & ModeDevice) == 0) {
        t.Errorf(pre + "wrong file mode %q: ModeDevice is not set"u8, fi.Mode());
    }
    if ((fs.FileMode)(fi.Mode() & ModeCharDevice) == 0) {
        t.Errorf(pre + "wrong file mode %q: ModeCharDevice is not set"u8, fi.Mode());
    }
    if (fi.Mode().IsRegular()) {
        t.Errorf(pre + "wrong file mode %q: IsRegular returns true"u8, fi.Mode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fStatˢ => ref fStatˢᵇ;
private static readonly @string fStatˢᵇ = "f.Stat"u8;
internal static ref readonly @string statˢ => ref statˢᵇ;
private static readonly @string statˢᵇ = "Stat"u8;

internal static void testDevNullFile(ж<Δtesting.T> t, @string devNullName) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Open(devNullName);
        if (err != nil) {
            t.Fatalf("Open(%s): %v", devNullName, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var fi, err) = f.Stat();
        if (err != nil) {
            t.Fatalf("Stat(%s): %v", devNullName, err);
        }
        testDevNullFileInfo(t, fStatˢ, devNullName, fi);

        (fi, err) = Stat(devNullName);
        if (err != nil) {
            t.Fatalf("Stat(%s): %v", devNullName, err);
        }
        testDevNullFileInfo(t, statˢ, devNullName, fi);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nulˢ => ref nulˢᵇ;
private static readonly @string nulˢᵇ = "./nul"u8;
internal static ref readonly @string nulˢ2 => ref nulˢ2ᵇ;
private static readonly @string nulˢ2ᵇ = "//./nul"u8;

public static void TestDevNullFile(ж<Δtesting.T> t) {
    t.Parallel();

    testDevNullFile(t, DevNull);
    if (Δruntime.GOOS == "windows"u8) {
        testDevNullFile(t, nulˢ);
        testDevNullFile(t, nulˢ2);
    }
}

internal static ж<bool> testLargeWrite = flag.Bool("large_write", false, "run TestLargeWriteToConsole test that floods console with output");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingConsoleFloodingˢ = (@string)"skipping console-flooding test; enable with -large_write"u8;

public static void TestLargeWriteToConsole(ж<Δtesting.T> t) {
    if (!testLargeWrite.Value) {
        t.Skip(skippingConsoleFloodingˢ);
    }
    var b = new slice<byte>(32000);
    foreach (var i in range(len(b))) {
        b[i] = (byte)'.';
    }
    b[len(b) - 1] = (byte)'\n';
    var (n, err) = Stdout.Write(b);
    if (err != nil) {
        t.Fatalf("Write to os.Stdout failed: %v", err);
    }
    if (n != len(b)) {
        t.Errorf("Write to os.Stdout should return %d; got %d", len(b), n);
    }
    (n, err) = Stderr.Write(b);
    if (err != nil) {
        t.Fatalf("Write to os.Stderr failed: %v", err);
    }
    if (n != len(b)) {
        t.Errorf("Write to os.Stderr should return %d; got %d", len(b), n);
    }
}

public static void TestStatDirModeExec(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "wasip1"u8) {
        t.Skip("Chmod is not supported on " + Δruntime.GOOS);
    }
    t.Parallel();

    UntypedInt mode = /* 0111 */ 73;

    @string path = t.TempDir();
    var errΔ1 = Chmod(path, 511); if (errΔ1 != nil) {
        t.Fatalf("Chmod %q 0777: %v", path, errΔ1);
    }

    var (dir, err) = Stat(path);
    if (err != nil) {
        t.Fatalf("Stat %q (looking for mode %#o): %s", path, (nint)(mode), err);
    }
    if ((fs.FileMode)(dir.Mode() & (uint32)mode) != mode) {
        t.Errorf("Stat %q: mode %#o want %#o", path, (fs.FileMode)(dir.Mode() & (uint32)mode), (nint)(mode));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goWantHelperProcessˢ => ref goWantHelperProcessˢᵇ;
private static readonly @string goWantHelperProcessˢᵇ = "GO_WANT_HELPER_PROCESS"u8;
internal static ref readonly @string testRunTestStatStdinˢ => ref testRunTestStatStdinˢᵇ;
private static readonly @string testRunTestStatStdinˢᵇ = "-test.run=^TestStatStdin$"u8;
internal static ref readonly @string outputˢ => ref outputˢᵇ;
private static readonly @string outputˢᵇ = "output"u8;

public static void TestStatStdin(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "android"u8 || exprᴛ1 == "plan9"u8: {
        t.Skipf("%s doesn't have /bin/sh", Δruntime.GOOS);
        break;
    }}

    if (Getenv(goWantHelperProcessˢ) == "1"u8) {
        var (st, errΔ1) = Stdin.Stat();
        if (errΔ1 != nil) {
            t.Fatalf("Stat failed: %v", errΔ1);
        }
        fmt.Println((fs.FileMode)(st.Mode() & ModeNamedPipe));
        Exit(0);
    }

    t.Parallel();
    @string exe = testenv.Executable(new os_test_package.testing_TжTB(t));

    var (fi, err) = Stdin.Stat();
    if (err != nil) {
        t.Fatal(err);
    }
    var mode = fi.Mode();
    switch (ᐧ) {
    case {} when (fs.FileMode)(mode & ModeCharDevice) != 0 && (fs.FileMode)(mode & ModeDevice) != 0: {
        break;
    }
    case {} when (fs.FileMode)(mode & ModeNamedPipe) != 0: {
        break;
    }
    default: {
        t.Fatalf("unexpected Stdin mode (%v), want ModeCharDevice or ModeNamedPipe", mode);
        break;
    }}

    var cmd = testenv.Command(new os_test_package.testing_TжTB(t), exe, testRunTestStatStdinˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Env, (@string)"GO_WANT_HELPER_PROCESS=1");
    // This will make standard input a pipe.
    cmd.Stdin = new os_test_package.strings_ReaderжReader(strings.NewReader(outputˢ));

    (var output, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("Failed to spawn child process: %v %q", err, (@string)output);
    }

    // result will be like "prw-rw-rw"
    if (len(output) < 1 || output[0] != 'p') {
        t.Fatalf("Child process reports stdin is not pipe '%v'", (@string)output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string targetˢ => ref targetˢᵇ;
private static readonly @string targetˢᵇ = "target"u8;
internal static readonly object statDoesnTFollowRelativeˢ = (@string)"Stat doesn't follow relative symlink"u8;

public static void TestStatRelativeSymlink(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
        t.Parallel();

        @string tmpdir = t.TempDir();
        @string target = filepath.Join(tmpdir, targetˢ);
        var (f, err) = Create(target);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        (var st, err) = f.Stat();
        if (err != nil) {
            t.Fatal(err);
        }

        @string link = filepath.Join(tmpdir, linkˢ);
        err = Symlink(filepath.Base(target), link);
        if (err != nil) {
            t.Fatal(err);
        }

        (var st1, err) = Stat(link);
        if (err != nil) {
            t.Fatal(err);
        }

        if (!SameFile(st, st1)) {
            t.Error(statDoesnTFollowRelativeˢ);
        }

        if (Δruntime.GOOS == "windows"u8) {
            Remove(link);
            err = Symlink(target.slice(len(filepath.VolumeName(target))), link);
            if (err != nil) {
                t.Fatal(err);
            }

            var (st1Δ1, errΔ1) = Stat(link);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }

            if (!SameFile(st, st1Δ1)) {
                t.Error(statDoesnTFollowRelativeˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestReadAtEOF(ж<Δtesting.T> t) {
    t.Parallel();

    var f = newFile(t);

    var (_, err) = f.ReadAt(new slice<byte>(10), 0);
    switch (err) {
    case var ᴛ when AreEqual(ᴛ, Δio.EOF): {
        // all good
        break;
    }
    case var ᴛ when AreEqual(ᴛ, default): {
        t.Fatalf("ReadAt succeeded");
        break;
    }
    default: {
        t.Fatalf("ReadAt failed: %s", err);
        break;
    }}
}

public static void TestLongPath(ж<Δtesting.T> t) {
    t.Parallel();

    @string tmpdir = t.TempDir();

    // Test the boundary of 247 and fewer bytes (normal) and 248 and more bytes (adjusted).
    var sizes = slice<nint>([247, 248, 249, 400]);
    while (len(tmpdir) < 400) {
        tmpdir += "/dir3456789"u8;
    }
    foreach (var (_, sz) in range(sizes)) {
        t.Run(fmt.Sprintf("length=%d"u8, sz), (ж<Δtesting.T> tΔ1) => {
            var namesᴷ = new stackAppend<@string>();
            @string sizedTempDir = tmpdir.slice(0, sz - 1) + "x"; // Ensure it does not end with a slash.

            // The various sized runs are for this call to trigger the boundary
            // condition.
            {
                var err = MkdirAll(sizedTempDir, 493); if (err != nil) {
                    tΔ1.Fatalf("MkdirAll failed: %v", err);
                }
            }
            var data = slice<byte>("hello world\n"u8);
            {
                var err = WriteFile(sizedTempDir + "/foo.txt"u8, data, 420); if (err != nil) {
                    tΔ1.Fatalf("os.WriteFile() failed: %v", err);
                }
            }
            {
                var err = Rename(sizedTempDir + "/foo.txt"u8, sizedTempDir + "/bar.txt"u8); if (err != nil) {
                    tΔ1.Fatalf("Rename failed: %v", err);
                }
            }
            var mtime = time.Now().Truncate(time.ΔMinute);
            {
                var err = Chtimes(sizedTempDir + "/bar.txt"u8, mtime, mtime); if (err != nil) {
                    tΔ1.Fatalf("Chtimes failed: %v", err);
                }
            }
            var names = slice<@string>(["bar.txt"]);
            if (testenv.HasSymlink()) {
                var err = Symlink(sizedTempDir + "/bar.txt"u8, sizedTempDir + "/symlink.txt"u8); if (err != nil) {
                    tΔ1.Fatalf("Symlink failed: %v", err);
                }
                names = append(namesᴷ, names, (@string)"symlink.txt");
            }
            if (testenv.HasLink()) {
                var err = Link(sizedTempDir + "/bar.txt"u8, sizedTempDir + "/link.txt"u8); if (err != nil) {
                    tΔ1.Fatalf("Link failed: %v", err);
                }
                names = append(names, (@string)"link.txt");
            }
            foreach (var (_, wantSize) in range(slice<int64>([(int64)len(data), 0]))) {
                foreach (var (_, name) in range(names)) {
                    @string path = sizedTempDir + "/"u8 + name;
                    var (dir, err) = Stat(path);
                    if (err != nil) {
                        tΔ1.Fatalf("Stat(%q) failed: %v", path, err);
                    }
                    var filesize = size(path, tΔ1);
                    if (dir.Size() != filesize || filesize != wantSize) {
                        tΔ1.Errorf("Size(%q) is %d, len(ReadFile()) is %d, want %d", path, dir.Size(), filesize, wantSize);
                    }
                    if (Δruntime.GOOS != "wasip1"u8) { // Chmod is not supported on wasip1
                        err = Chmod(path, dir.Mode());
                        if (err != nil) {
                            tΔ1.Fatalf("Chmod(%q) failed: %v", path, err);
                        }
                    }
                }
                {
                    var err = Truncate(sizedTempDir + "/bar.txt"u8, 0); if (err != nil) {
                        tΔ1.Fatalf("Truncate failed: %v", err);
                    }
                }
            }
        });
    }
}

internal static void testKillProcess(ж<Δtesting.T> t, Action<ж<Δos.Process>> processKiller) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();

        // Re-exec the test binary to start a process that hangs until stdin is closed.
        var cmd = testenv.Command(new os_test_package.testing_TжTB(t), testenv.Executable(new os_test_package.testing_TжTB(t)));
        cmd.Env = append(cmd.Environ(), (@string)"GO_OS_TEST_DRAIN_STDIN=1");
        var (stdout, err) = cmd.StdoutPipe();
        if (err != nil) {
            t.Fatal(err);
        }
        (var stdin, err) = cmd.StdinPipe();
        if (err != nil) {
            t.Fatal(err);
        }
        err = cmd.Start();
        if (err != nil) {
            t.Fatalf("Failed to start test process: %v", err);
        }

        var cmdʗ1 = cmd;
        var stdinʗ1 = stdin;
        defer(() => {
            var errΔ1 = cmdʗ1.Wait(); if (errΔ1 == nil) {
                t.Errorf("Test process succeeded, but expected to fail");
            }
            stdinʗ1.Close(); // Keep stdin alive until the process has finished dying.
        }, ref ᒐ);

        // Wait for the process to be started.
        // (It will close its stdout when it reaches TestMain.)
        Δio.Copy(Δio.Discard, stdout);

        processKiller(cmd.Process);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestKillStartProcess(ж<Δtesting.T> t) {
    testKillProcess(t, (ж<Δos.Process> p) => {
        var err = p.Kill();
        if (err != nil) {
            t.Fatalf("Failed to kill test process: %v", err);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRunTestGetppidˢ => ref testRunTestGetppidˢᵇ;
private static readonly @string testRunTestGetppidˢᵇ = "-test.run=^TestGetppid$"u8;

public static void TestGetppid(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "plan9"u8) {
        // TODO: golang.org/issue/8206
        t.Skipf("skipping test on plan9; see issue 8206");
    }

    if (Getenv(goWantHelperProcessˢ) == "1"u8) {
        fmt.Print(Getppid());
        Exit(0);
    }

    t.Parallel();

    var cmd = testenv.Command(new os_test_package.testing_TжTB(t), testenv.Executable(new os_test_package.testing_TжTB(t)), testRunTestGetppidˢ);
    cmd.Env = append(Environ(), (@string)"GO_WANT_HELPER_PROCESS=1");

    // verify that Getppid() from the forked process reports our process id
    var (output, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("Failed to spawn child process: %v %q", err, (@string)output);
    }

    @string childPpid = (@string)output;
    @string ourPid = fmt.Sprintf("%d"u8, Getpid());
    if (childPpid != ourPid) {
        t.Fatalf("Child process reports parent process id '%v', expected '%v'", childPpid, ourPid);
    }
}

public static void TestKillFindProcess(ж<Δtesting.T> t) {
    testKillProcess(t, (ж<Δos.Process> p) => {
        var (p2, err) = FindProcess(p.Pid);
        if (err != nil) {
            t.Fatalf("Failed to find test process: %v", err);
        }
        err = p2.Kill();
        if (err != nil) {
            t.Fatalf("Failed to kill test process: %v", err);
        }
    });
}

partial struct nilFileMethodTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Func<ж<Δos.File>, error> f;
}
internal static slice<nilFileMethodTestsᴛ1> nilFileMethodTests = [
    new("Chdir", (ж<Δos.File> f) => f.Chdir()),
    new("Close", (ж<Δos.File> f) => f.Close()),
    new("Chmod", (ж<Δos.File> f) => f.Chmod(0)),
    new("Chown", (ж<Δos.File> f) => f.Chown(0, 0)),
    new("Read", (ж<Δos.File> f) => {
        var (_, err) = f.Read(new slice<byte>(0));
        return err;
    }),
    new("ReadAt", (ж<Δos.File> f) => {
        var (_, err) = f.ReadAt(new slice<byte>(0), 0);
        return err;
    }),
    new("Readdir", (ж<Δos.File> f) => {
        var (_, err) = f.Readdir(1);
        return err;
    }),
    new("Readdirnames", (ж<Δos.File> f) => {
        var (_, err) = f.Readdirnames(1);
        return err;
    }),
    new("Seek", (ж<Δos.File> f) => {
        var (_, err) = f.Seek(0, Δio.SeekStart);
        return err;
    }),
    new("Stat", (ж<Δos.File> f) => {
        var (_, err) = f.Stat();
        return err;
    }),
    new("Sync", (ж<Δos.File> f) => f.Sync()),
    new("Truncate", (ж<Δos.File> f) => f.Truncate(0)),
    new("Write", (ж<Δos.File> f) => {
        var (_, err) = f.Write(new slice<byte>(0));
        return err;
    }),
    new("WriteAt", (ж<Δos.File> f) => {
        var (_, err) = f.WriteAt(new slice<byte>(0), 0);
        return err;
    }),
    new("WriteString", (ж<Δos.File> f) => {
        var (_, err) = f.WriteString("");
        return err;
    })
];

// Test that all File methods give ErrInvalid if the receiver is nil.
public static void TestNilFileMethods(ж<Δtesting.T> t) {
    t.Parallel();

    foreach (var (_, tt) in range(nilFileMethodTests)) {
        ж<Δos.File> @file = default;
        var got = tt.f(@file);
        if (!AreEqual(got, ErrInvalid)) {
            t.Errorf("%v should fail when f is nil; got %v", tt.name, got);
        }
    }
}

internal static void mkdirTree(ж<Δtesting.T> t, @string root, nint level, nint max) {
    if (level >= max) {
        return;
    }
    level++;
    for (var i = (rune)'a'; i < 'c'; i++) {
        @string dir = filepath.Join(root, (@string)i);
        var err = Mkdir(dir, 448); if (err != nil) {
            t.Fatal(err);
        }
        mkdirTree(t, dir, level, max);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnWindowsˢ = (@string)"skipping on windows"u8;

// Test that simultaneous RemoveAll do not report an error.
// As long as it gets removed, we should be happy.
public static partial void TestRemoveAllRace(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "windows"u8) {
            // Windows has very strict rules about things like
            // removing directories while someone else has
            // them open. The racing doesn't work out nicely
            // like it does on Unix.
            t.Skip(skippingOnWindowsˢ);
        }
        if (Δruntime.GOOS == "dragonfly"u8) {
            testenv.SkipFlaky(new os_test_package.testing_TжTB(t), 52301);
        }

        nint n = Δruntime.GOMAXPROCS(16);
        defer(Δruntime.GOMAXPROCS, n, ref ᒐ);
        @string root = t.TempDir();
        mkdirTree(t, root, 1, 6);
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
                    if (err != nil) {
                        t.Errorf("unexpected error: %T, %q", err, err);
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
public static partial void TestPipeThreads(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "aix"u8: {
            t.Skip(skippingOnAixIssue70131ˢ);
            break;
        }
        case {} when exprᴛ1 == "illumos"u8 || exprᴛ1 == "solaris"u8: {
            t.Skip(skippingOnSolarisAndˢ);
            break;
        }
        case {} when exprᴛ1 == "windows"u8: {
            t.Skip(skippingOnWindowsIssueˢ);
            break;
        }
        case {} when exprᴛ1 == "plan9"u8: {
            t.Skip(skippingOnPlan9DoesNotˢ);
            break;
        }
        case {} when exprᴛ1 == "js"u8: {
            t.Skip(skippingOnJsNoSupportForˢ);
            break;
        }
        case {} when exprᴛ1 == "wasip1"u8: {
            t.Skip(skippingOnWasip1Noˢ);
            break;
        }}

        nint threads = 100;

        var r = new slice<ж<Δos.File>>(threads);
        var w = new slice<ж<Δos.File>>(threads);
        for (nint i = 0; i < threads; i++) {
            var (rp, wp, err) = Pipe();
            if (err != nil) {
                for (nint j = 0; j < i; j++) {
                    r[j].Close();
                    w[j].Close();
                }
                t.Fatal(err);
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
                    var (_, err) = rʗ1[iΔ1].Read(b[..]); if (err != nil) {
                        t.Error(err);
                    }
                }
                {
                    var err = rʗ1[iΔ1].Close(); if (err != nil) {
                        t.Error(err);
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
                var (_, err) = w[i].Write([0]); if (err != nil) {
                    t.Error(err);
                }
            }
            {
                var err = w[i].Close(); if (err != nil) {
                    t.Error(err);
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
        if (err != nil) {
            t.Fatal(err);
        }
        var errΔ1 = @file.Close(); if (errΔ1 != nil) {
            t.Fatalf("unexpected error from Close: %v", errΔ1);
        }
        var errΔ2 = @file.Close(); if (errΔ2 == nil) {
            t.Error(secondCloseDidNotFailˢ);
        } else if (!errΔ2._<ж<fs.PathError>>(out var pe)) {
            t.Errorf("second Close: got %T, want %T", errΔ2, pe.OrTypedNil());
        } else if (!AreEqual(pe.Err, ErrClosed)) {
            t.Errorf("second Close: got %q, want %q", pe.Err, ErrClosed);
        } else {
            t.Logf("second close returned expected error %q", errΔ2);
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileˢ2 => ref fileˢ2ᵇ;
private static readonly @string fileˢ2ᵇ = "file"u8;

public static partial void TestDoubleCloseError(ж<Δtesting.T> t) {
    t.Parallel();
    t.Run(fileˢ2, testDoubleCloseError(filepath.Join(sfdir, sfname)));
    t.Run(dirˢ, testDoubleCloseError(sfdir));
}

public static void TestUserCacheDir(ж<Δtesting.T> t) {
    t.Parallel();

    var (dir, err) = UserCacheDir();
    if (err != nil) {
        t.Skipf("skipping: %v", err);
    }
    if (dir == ""u8) {
        t.Fatalf("UserCacheDir returned %q; want non-empty path or error", dir);
    }

    (var fi, err) = Stat(dir);
    if (err != nil) {
        if (IsNotExist(err)) {
            t.Log(err);
            return;
        }
        t.Fatal(err);
    }
    if (!fi.IsDir()) {
        t.Fatalf("dir %s is not directory; type = %v", dir, fi.Mode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object xdgCacheHomeIsEffectiveˢ = (@string)"$XDG_CACHE_HOME is effective only on Unix systems"u8;
internal static ref readonly @string xdgCacheHomeˢ => ref xdgCacheHomeˢᵇ;
private static readonly @string xdgCacheHomeˢᵇ = "XDG_CACHE_HOME"u8;
internal static ref readonly @string someDirˢ => ref someDirˢᵇ;
private static readonly @string someDirˢᵇ = "some-dir"u8;
internal static readonly object userCacheDirSucceededˢ = (@string)"UserCacheDir succeeded though $XDG_CACHE_HOME contains a relative path"u8;

public static void TestUserCacheDirXDGConfigDirEnvVar(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "plan9"u8: {
        t.Skip(xdgCacheHomeIsEffectiveˢ);
        break;
    }}

    var (wd, err) = Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    t.Setenv(xdgCacheHomeˢ, wd);

    (var dir, err) = UserCacheDir();
    if (err != nil) {
        t.Fatal(err);
    }
    if (dir != wd) {
        t.Fatalf("UserCacheDir returned %q; want the value of $XDG_CACHE_HOME %q", dir, wd);
    }

    t.Setenv(xdgCacheHomeˢ, someDirˢ);
    (_, err) = UserCacheDir();
    if (err == nil) {
        t.Fatal(userCacheDirSucceededˢ);
    }
}

public static void TestUserConfigDir(ж<Δtesting.T> t) {
    t.Parallel();

    var (dir, err) = UserConfigDir();
    if (err != nil) {
        t.Skipf("skipping: %v", err);
    }
    if (dir == ""u8) {
        t.Fatalf("UserConfigDir returned %q; want non-empty path or error", dir);
    }

    (var fi, err) = Stat(dir);
    if (err != nil) {
        if (IsNotExist(err)) {
            t.Log(err);
            return;
        }
        t.Fatal(err);
    }
    if (!fi.IsDir()) {
        t.Fatalf("dir %s is not directory; type = %v", dir, fi.Mode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object xdgConfigHomeIsEffectiveˢ = (@string)"$XDG_CONFIG_HOME is effective only on Unix systems"u8;
internal static ref readonly @string xdgConfigHomeˢ => ref xdgConfigHomeˢᵇ;
private static readonly @string xdgConfigHomeˢᵇ = "XDG_CONFIG_HOME"u8;
internal static readonly object userConfigDirSucceededˢ = (@string)"UserConfigDir succeeded though $XDG_CONFIG_HOME contains a relative path"u8;

public static void TestUserConfigDirXDGConfigDirEnvVar(ж<Δtesting.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "darwin"u8 || exprᴛ1 == "plan9"u8: {
        t.Skip(xdgConfigHomeIsEffectiveˢ);
        break;
    }}

    var (wd, err) = Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    t.Setenv(xdgConfigHomeˢ, wd);

    (var dir, err) = UserConfigDir();
    if (err != nil) {
        t.Fatal(err);
    }
    if (dir != wd) {
        t.Fatalf("UserConfigDir returned %q; want the value of $XDG_CONFIG_HOME %q", dir, wd);
    }

    t.Setenv(xdgConfigHomeˢ, someDirˢ);
    (_, err) = UserConfigDir();
    if (err == nil) {
        t.Fatal(userConfigDirSucceededˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userHomeDirReturnedAnˢ = (@string)"UserHomeDir returned an empty string but no error"u8;

public static void TestUserHomeDir(ж<Δtesting.T> t) {
    t.Parallel();

    var (dir, err) = UserHomeDir();
    if (dir == ""u8 && err == nil) {
        t.Fatal(userHomeDirReturnedAnˢ);
    }
    if (err != nil) {
        // UserHomeDir may return a non-nil error if the environment variable
        // for the home directory is empty or unset in the environment.
        t.Skipf("skipping: %v", err);
    }

    (var fi, err) = Stat(dir);
    if (err != nil) {
        if (IsNotExist(err)) {
            // The user's home directory has a well-defined location, but does not
            // exist. (Maybe nothing has written to it yet? That could happen, for
            // example, on minimal VM images used for CI testing.)
            t.Log(err);
            return;
        }
        t.Fatal(err);
    }
    if (!fi.IsDir()) {
        t.Fatalf("dir %s is not directory; type = %v", dir, fi.Mode());
    }
}

public static void TestDirSeek(ж<Δtesting.T> t) {
    t.Parallel();

    var (wd, err) = Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    (var f, err) = Open(wd);
    if (err != nil) {
        t.Fatal(err);
    }
    (var dirnames1, err) = f.Readdirnames(0);
    if (err != nil) {
        t.Fatal(err);
    }

    (var ret, err) = f.Seek(0, 0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (ret != 0) {
        t.Fatalf("seek result not zero: %d", ret);
    }

    (var dirnames2, err) = f.Readdirnames(0);
    if (err != nil) {
        t.Fatal(err);
    }

    if (len(dirnames1) != len(dirnames2)) {
        t.Fatalf("listings have different lengths: %d and %d\n", len(dirnames1), len(dirnames2));
    }
    foreach (var (i, n1) in range(dirnames1)) {
        @string n2 = dirnames2[i];
        if (n1 != n2) {
            t.Fatalf("different name i=%d n1=%s n2=%s\n", i, n1, n2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string issue37161ˢ => ref issue37161ˢᵇ;
private static readonly @string issue37161ˢᵇ = "issue37161"u8;

public static void TestReaddirSmallSeek(ж<Δtesting.T> t) {
    // See issue 37161. Read only one entry from a directory,
    // seek to the beginning, and read again. We should not see
    // duplicate entries.
    t.Parallel();

    var (wd, err) = Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    (var df, err) = Open(filepath.Join(wd, testdataˢ, issue37161ˢ));
    if (err != nil) {
        t.Fatal(err);
    }
    (var names1, err) = df.Readdirnames(1);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = df.Seek(0, 0); if (err != nil) {
        t.Fatal(err);
    }
    (var names2, err) = df.Readdirnames(0);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(names2) != 3) {
        t.Fatalf("first names: %v, second names: %v", names1, names2);
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
internal static ref readonly @string openFileˢ => ref openFileˢᵇ;
private static readonly @string openFileˢᵇ = "OpenFile"u8;
internal static ref readonly @string rootOpenFileˢ => ref rootOpenFileˢᵇ;
private static readonly @string rootOpenFileˢᵇ = "RootOpenFile"u8;

// Test that opening a file does not change its permissions.  Issue 38225.
public static void TestOpenFileKeepsPermissions(ж<Δtesting.T> t) {
    t.Run(openFileˢ, (ж<Δtesting.T> tΔ1) => {
        testOpenFileKeepsPermissions(tΔ1, OpenFile);
    });
    t.Run(rootOpenFileˢ, (ж<Δtesting.T> tΔ2) => {
        testOpenFileKeepsPermissions(tΔ2, (@string name, nint flag, fs.FileMode perm) => {
            GoFrame ᒐ = default;
            try {
                var (dir, @file) = filepath.Split(name);
                var (r, err) = OpenRoot(dir);
                if (err != nil) {
                    return (default, err);
                }
                var rʗ1 = r;
                defer(() => rʗ1.Close(), ref ᒐ);
                return r.OpenFile(@file, flag, perm);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        });
    });
}
internal static void testOpenFileKeepsPermissions(ж<Δtesting.T> t, Func<@string, nint, fs.FileMode, (ж<Δos.File>, error)> openf) {
    t.Parallel();

    @string dir = t.TempDir();
    @string name = filepath.Join(dir, "x");
    var (f, err) = Create(name);
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ1 = f.Close(); if (errΔ1 != nil) {
        t.Error(errΔ1);
    }
    (f, err) = openf(name, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_TRUNC), 0);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        var (fi, errΔ2) = f.Stat(); if (errΔ2 != nil) {
            t.Error(errΔ2);
        } else if ((fs.FileMode)(fi.Mode() & 146) == 0) {
            t.Errorf("f.Stat.Mode after OpenFile is %v, should be writable", fi.Mode());
        }
    }
    var errΔ3 = f.Close(); if (errΔ3 != nil) {
        t.Error(errΔ3);
    }
    {
        var (fi, errΔ4) = Stat(name); if (errΔ4 != nil) {
            t.Error(errΔ4);
        } else if ((fs.FileMode)(fi.Mode() & 146) == 0) {
            t.Errorf("Stat after OpenFile is %v, should be writable", fi.Mode());
        }
    }
}

internal static void forceMFTUpdateOnWindows(ж<Δtesting.T> t, @string path) {
    t.Helper();

    if (Δruntime.GOOS != "windows"u8) {
        return;
    }

    // On Windows, we force the MFT to update by reading the actual metadata from GetFileInformationByHandle and then
    // explicitly setting that. Otherwise it might get out of sync with FindFirstFile. See golang.org/issues/42637.
    {
        var err = filepath.WalkDir(path, (@string pathΔ1, fs.DirEntry d, error errΔ1) => {
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            (var info, errΔ1) = d.Info();
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            (var stat, errΔ1) = Stat(pathΔ1); // This uses GetFileInformationByHandle internally.
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            if (stat.ModTime() == info.ModTime()) {
                return default;
            }
            var errΔ2 = Chtimes(pathΔ1, stat.ModTime(), stat.ModTime()); if (errΔ2 != nil) {
                t.Log(errΔ2); // We only log, not die, in case the test directory is not writable.
            }
            return default;
        }); if (err != nil) {
            t.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataDirfsˢ => ref testdataDirfsˢᵇ;
private static readonly @string testdataDirfsˢᵇ = "./testdata/dirfs"u8;

public static void TestDirFS(ж<Δtesting.T> t) {
    t.Parallel();
    testDirFS(t, DirFS(testdataDirfsˢ));
}

public static void TestRootDirFS(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Parallel();
        var (r, err) = OpenRoot(testdataDirfsˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        testDirFS(t, r.FS());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dirXˢ => ref dirXˢᵇ;
private static readonly @string dirXˢᵇ = "dir/x"u8;
internal static readonly object expectedDirFSResultToˢ = (@string)"expected DirFS result to implement fs.ReadDirFS"u8;
internal static readonly object fsReadDirOfNonexistentˢ = (@string)"fs.ReadDir of nonexistent directory succeeded"u8;
internal static readonly object fsOpenOfNonexistentFileˢ = (@string)"fs.Open of nonexistent file succeeded"u8;
internal static ref readonly @string testdataDirfsˢ2 => ref testdataDirfsˢ2ᵇ;
private static readonly @string testdataDirfsˢ2ᵇ = @"testdata\dirfs"u8;
internal static ref readonly @string nulˢ3 => ref nulˢ3ᵇ;
private static readonly @string nulˢ3ᵇ = @"NUL"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string nonesuchᶜ => ref nonesuchᶜᵇ;
private static readonly @string nonesuchᶜᵇ = "dir/nonesuch"u8;

internal static void testDirFS(ж<Δtesting.T> t, fs.FS fsys) {
    forceMFTUpdateOnWindows(t, testdataDirfsˢ);

    var errΔ1 = fstest.TestFS(fsys, "a", "b", dirXˢ); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }

    var (rdfs, ok) = fsys._<fs.ReadDirFS>(ᐧ);
    if (!ok) {
        t.Error(expectedDirFSResultToˢ);
    }
    var (_, errΔ2) = rdfs.ReadDir(nonexistentˢ); if (errΔ2 == nil) {
        t.Error(fsReadDirOfNonexistentˢ);
    }

    // Test that the error message does not contain a backslash,
    // and does not contain the DirFS argument.
    @string nonesuch = nonesuchᶜ;
    var (_, err) = fsys.Open(nonesuch);
    if (err == nil) {
        t.Error(fsOpenOfNonexistentFileˢ);
    } else {
        if (!strings.Contains(err.Error(), nonesuch)) {
            t.Errorf("error %q does not contain %q", err, nonesuch);
        }
        if (strings.Contains(err._<ж<fs.PathError>>().Path, testdataˢ)) {
            t.Errorf("error %q contains %q", err, testdataˢ);
        }
    }

    // Test that Open does not accept backslash as separator.
    var d = DirFS(".");
    (_, err) = d.Open(testdataDirfsˢ2);
    if (err == nil) {
        t.Fatalf(@"Open testdata\dirfs succeeded");
    }

    // Test that Open does not open Windows device files.
    (_, err) = d.Open(nulˢ3);
    if (err == nil) {
        t.Errorf(@"Open NUL succeeded");
    }
}

public static void TestDirFSRootDir(ж<Δtesting.T> t) {
    t.Parallel();

    var (cwd, err) = Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    cwd = cwd.slice(len(filepath.VolumeName(cwd))); // trim volume prefix (C:) on Windows
    cwd = filepath.ToSlash(cwd);                    // convert \ to /
    cwd = strings.TrimPrefix(cwd, "/");             // trim leading /

    // Test that Open can open a path starting at /.
    var d = DirFS("/");
    (var f, err) = d.Open(cwd + "/testdata/dirfs/a"u8);
    if (err != nil) {
        t.Fatal(err);
    }
    f.Close();
}

public static void TestDirFSEmptyDir(ж<Δtesting.T> t) {
    t.Parallel();

    var d = DirFS(""u8);
    var (cwd, _) = Getwd();
    foreach (var (_, path) in range(slice<@string>([
        "testdata/dirfs/a",                           // not DirFS(".")
        filepath.ToSlash(cwd) + "/testdata/dirfs/a"u8 // not DirFS("/")
    ]))) {
        var (_, err) = d.Open(path);
        if (err == nil) {
            t.Fatalf(@"DirFS("""").Open(%q) succeeded", path);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string controlTxtˢ => ref controlTxtˢᵇ;
private static readonly @string controlTxtˢᵇ = "control.txt"u8;
internal static ref readonly @string eXperiMentTxtˢ => ref eXperiMentTxtˢᵇ;
private static readonly @string eXperiMentTxtˢᵇ = @"e:xperi\ment.txt"u8;

public static void TestDirFSPathsValid(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "windows"u8) {
        t.Skipf("skipping on Windows");
    }
    t.Parallel();

    @string d = t.TempDir();
    var errΔ1 = WriteFile(filepath.Join(d, controlTxtˢ), slice<byte>((@string)"Hello, world!"u8), 420); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var errΔ2 = WriteFile(filepath.Join(d, eXperiMentTxtˢ), slice<byte>((@string)"Hello, colon and backslash!"u8), 420); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }

    var fsys = DirFS(d);
    var err = fs.WalkDir(fsys, ".", (@string path, fs.DirEntry e, error errΔ3) => {
        if (fs.ValidPath(e.Name())) {
            t.Logf("%q ok", e.Name());
        } else {
            t.Errorf("%q INVALID", e.Name());
        }
        return default;
    });
    if (err != nil) {
        t.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string procSysFsPipeMaxSizeˢ => ref procSysFsPipeMaxSizeˢᵇ;
private static readonly @string procSysFsPipeMaxSizeˢᵇ = "/proc/sys/fs/pipe-max-size"u8;

public static void TestReadFileProc(ж<Δtesting.T> t) {
    t.Parallel();

    // Linux files in /proc report 0 size,
    // but then if ReadFile reads just a single byte at offset 0,
    // the read at offset 1 returns EOF instead of more data.
    // ReadFile has a minimum read size of 512 to work around this,
    // but test explicitly that it's working.
    @string name = procSysFsPipeMaxSizeˢ;
    var (_, errΔ1) = Stat(name); if (errΔ1 != nil) {
        t.Skip(errΔ1);
    }
    var (data, err) = ReadFile(name);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(data) == 0 || data[len(data) - 1] != '\n') {
        t.Fatalf("read %s: not newline-terminated: %q", name, data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string procSysFsPipeMaxSizeˢ2 => ref procSysFsPipeMaxSizeˢ2ᵇ;
private static readonly @string procSysFsPipeMaxSizeˢ2ᵇ = "proc/sys/fs/pipe-max-size"u8;

public static void TestDirFSReadFileProc(ж<Δtesting.T> t) {
    t.Parallel();

    var fsys = DirFS("/");
    @string name = procSysFsPipeMaxSizeˢ2;
    var (_, errΔ1) = fs.Stat(fsys, name); if (errΔ1 != nil) {
        t.Skip();
    }
    var (data, err) = fs.ReadFile(fsys, name);
    if (err != nil) {
        t.Fatal(err);
    }
    if (len(data) == 0 || data[len(data) - 1] != '\n') {
        t.Fatalf("read %s: not newline-terminated: %q", name, data);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object jsAllocatesALotDuringˢ = (@string)"js allocates a lot during File.WriteString"u8;
internal static ref readonly @string whiteboardTxtˢ => ref whiteboardTxtˢᵇ;
private static readonly @string whiteboardTxtˢᵇ = "whiteboard.txt"u8;
internal static ref readonly @string iWillNotAllocateWhenˢ => ref iWillNotAllocateWhenˢᵇ;
private static readonly @string iWillNotAllocateWhenˢᵇ = "I will not allocate when passed a string longer than 32 bytes.\n"u8;

public static void TestWriteStringAlloc(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "js"u8) {
            t.Skip(jsAllocatesALotDuringˢ);
        }
        @string d = t.TempDir();
        var (f, err) = Create(filepath.Join(d, whiteboardTxtˢ));
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var fʗ2 = f;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            fʗ2.WriteString(iWillNotAllocateWhenˢ);
        });
        if (allocs != 0D) {
            t.Errorf("expected 0 allocs for File.WriteString, got %v", allocs);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testTxtˢ => ref testTxtˢᵇ;
private static readonly @string testTxtˢᵇ = "test.txt"u8;

// Test that it's OK to have parallel I/O and Close on a file.
public static void TestFileIOCloseRace(ж<Δtesting.T> t) {
    t.Parallel();
    var (@file, err) = Create(filepath.Join(t.TempDir(), testTxtˢ));
    if (err != nil) {
        t.Fatal(err);
    }
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var fileʗ1 = @file;
    Ꮡwg.Go(() => {
        array<byte> tmp = new(100);
        var (_, errΔ1) = fileʗ1.Write(tmp[..]); if (errΔ1 != nil && !errors.Is(errΔ1, ErrClosed)) {
            t.Error(errΔ1);
        }
    });
    var fileʗ2 = @file;
    Ꮡwg.Go(() => {
        array<byte> tmp = new(100);
        var (_, errΔ2) = fileʗ2.Read(tmp[..]); if (errΔ2 != nil && !AreEqual(errΔ2, Δio.EOF) && !errors.Is(errΔ2, ErrClosed)) {
            t.Error(errΔ2);
        }
    });
    var fileʗ3 = @file;
    Ꮡwg.Go(() => {
        var errΔ3 = fileʗ3.Close(); if (errΔ3 != nil) {
            t.Error(errΔ3);
        }
    });
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string brokenPipeˢ => ref brokenPipeˢᵇ;
private static readonly @string brokenPipeˢᵇ = "broken pipe"u8;
internal static ref readonly @string pipeIsBeingClosedˢ => ref pipeIsBeingClosedˢᵇ;
private static readonly @string pipeIsBeingClosedˢᵇ = "pipe is being closed"u8;
internal static ref readonly @string hungupChannelˢ => ref hungupChannelˢᵇ;
private static readonly @string hungupChannelˢᵇ = "hungup channel"u8;

// Test that it's OK to have parallel I/O and Close on a pipe.
public static partial void TestPipeIOCloseRace(ж<Δtesting.T> t) {
    // Skip on wasm, which doesn't have pipes.
    if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
        t.Skipf("skipping on %s: no pipes", Δruntime.GOOS);
    }
    t.Parallel();

    var (r, w, err) = Pipe();
    if (err != nil) {
        t.Fatal(err);
    }

    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(3);

    var wʗ1 = w;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            for (;;) {
                var (n, errΔ1) = wʗ1.Write(slice<byte>("hi"u8));
                if (errΔ1 != nil) {
                    // We look at error strings as the
                    // expected errors are OS-specific.
                    switch (ᐧ) {
                    case {} when (errors.Is(errΔ1, ErrClosed)) || (strings.Contains(errΔ1.Error(), brokenPipeˢ)) || (strings.Contains(errΔ1.Error(), pipeIsBeingClosedˢ)) || (strings.Contains(errΔ1.Error(), hungupChannelˢ)): {
                        // Ignore an expected error.
                        break;
                    }
                    default: {
                        // Unexpected error.
                        t.Error(errΔ1);
                        break;
                    }}
                    return;
                }
                if (n != 2) {
                    t.Errorf("wrote %d bytes, expected 2", n);
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
            for (;;) {
                ref var buf = ref heap(new array<byte>(2), out var Ꮡbuf);
                var (n, errΔ2) = rʗ1.Read(buf[..]);
                if (errΔ2 != nil) {
                    if (!AreEqual(errΔ2, Δio.EOF) && !errors.Is(errΔ2, ErrClosed)) {
                        t.Error(errΔ2);
                    }
                    return;
                }
                if (n != 2) {
                    t.Errorf("read %d bytes, want 2", n);
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

            var errΔ3 = rʗ2.Close(); if (errΔ3 != nil) {
                t.Error(errΔ3);
            }
            var errΔ4 = wʗ2.Close(); if (errΔ4 != nil) {
                t.Error(errΔ4);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });

    Ꮡwg.Wait();
}

// Test that it's OK to call Close concurrently on a pipe.
public static partial void TestPipeCloseRace(ж<Δtesting.T> t) {
    // Skip on wasm, which doesn't have pipes.
    if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
        t.Skipf("skipping on %s: no pipes", Δruntime.GOOS);
    }
    t.Parallel();

    var (r, w, err) = Pipe();
    if (err != nil) {
        t.Fatal(err);
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
        if (errΔ1 == nil) {
            nils++;
        } else {
            errs++;
        }
    }
    if (nils != 2 || errs != 2) {
        t.Errorf("got nils %d errs %d, want 2 2", nils, errs);
    }
}

public static void TestRandomLen(ж<Δtesting.T> t) {
    foreach (var _ᴛ1 in range(5)) {
        var (dir, err) = MkdirTemp(t.TempDir(), "*");
        if (err != nil) {
            t.Fatal(err);
        }
        @string @base = filepath.Base(dir);
        if (len(@base) > 10) {
            t.Errorf("MkdirTemp returned len %d: %s", len(@base), @base);
        }
    }
    foreach (var _ᴛ2 in range(5)) {
        var (f, err) = CreateTemp(t.TempDir(), "*");
        if (err != nil) {
            t.Fatal(err);
        }
        @string @base = filepath.Base(f.Name());
        f.Close();
        if (len(@base) > 10) {
            t.Errorf("CreateTemp returned len %d: %s", len(@base), @base);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object copyFSˢ = (@string)"CopyFS:"u8;
internal static readonly object testFSˢ = (@string)"TestFS:"u8;
internal static readonly object comparingTwoDirectoriesˢ = (@string)"comparing two directories:"u8;
internal static ref readonly @string williamˢ => ref williamˢᵇ;
private static readonly @string williamˢᵇ = "william"u8;
internal static ref readonly @string carlˢ => ref carlˢᵇ;
private static readonly @string carlˢᵇ = "carl"u8;
internal static ref readonly @string daVinciˢ => ref daVinciˢᵇ;
private static readonly @string daVinciˢᵇ = "daVinci"u8;
internal static ref readonly @string einsteinˢ => ref einsteinˢᵇ;
private static readonly @string einsteinˢᵇ = "einstein"u8;
internal static ref readonly @string dirNewtonˢ => ref dirNewtonˢᵇ;
private static readonly @string dirNewtonˢᵇ = "dir/newton"u8;

public static void TestCopyFS(ж<Δtesting.T> t) {
    t.Parallel();

    // Test with disk filesystem.
    forceMFTUpdateOnWindows(t, testdataDirfsˢ);
    var fsys = DirFS(testdataDirfsˢ);
    @string tmpDir = t.TempDir();
    {
        var err = CopyFS(tmpDir, fsys); if (err != nil) {
            t.Fatal(copyFSˢ, err);
        }
    }
    forceMFTUpdateOnWindows(t, tmpDir);
    var tmpFsys = DirFS(tmpDir);
    {
        var err = fstest.TestFS(tmpFsys, "a", "b", dirXˢ); if (err != nil) {
            t.Fatal(testFSˢ, err);
        }
    }
    {
        var err = verifyCopyFS(t, fsys, tmpFsys); if (err != nil) {
            t.Fatal(comparingTwoDirectoriesˢ, err);
        }
    }

    // Test whether CopyFS disallows copying for disk filesystem when there is any
    // existing file in the destination directory.
    {
        var err = CopyFS(tmpDir, fsys); if (!errors.Is(err, fs.ErrExist)) {
            t.Errorf("CopyFS should have failed and returned error when there is"u8 + "any existing file in the destination directory (in disk filesystem), "u8 + "got: %v, expected any error that indicates <file exists>"u8, err);
        }
    }

    // Test with memory filesystem.
    fsys = new os_test_package.fstest_MapFSᴠFS(new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["william"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Shakespeare\n"u8))),
        ["carl"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Gauss\n"u8))),
        ["daVinci"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Leonardo\n"u8))),
        ["einstein"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Albert\n"u8))),
        ["dir/newton"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("Sir Isaac\n"u8)))
    }));
    tmpDir = t.TempDir();
    {
        var err = CopyFS(tmpDir, fsys); if (err != nil) {
            t.Fatal(copyFSˢ, err);
        }
    }
    forceMFTUpdateOnWindows(t, tmpDir);
    tmpFsys = DirFS(tmpDir);
    {
        var err = fstest.TestFS(tmpFsys, williamˢ, carlˢ, daVinciˢ, einsteinˢ, dirNewtonˢ); if (err != nil) {
            t.Fatal(testFSˢ, err);
        }
    }
    {
        var err = verifyCopyFS(t, fsys, tmpFsys); if (err != nil) {
            t.Fatal(comparingTwoDirectoriesˢ, err);
        }
    }

    // Test whether CopyFS disallows copying for memory filesystem when there is any
    // existing file in the destination directory.
    {
        var err = CopyFS(tmpDir, fsys); if (!errors.Is(err, fs.ErrExist)) {
            t.Errorf("CopyFS should have failed and returned error when there is"u8 + "any existing file in the destination directory (in memory filesystem), "u8 + "got: %v, expected any error that indicates <file exists>"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testˢ => ref testˢᵇ;
private static readonly @string testˢᵇ = "test"u8;
internal static ref readonly @string tmpˢ2 => ref tmpˢ2ᵇ;
private static readonly @string tmpˢ2ᵇ = "tmp"u8;

// verifyCopyFS checks the content and permission of each file inside copied FS to ensure
// the copied files satisfy the convention stipulated in CopyFS.
internal static error verifyCopyFS(ж<Δtesting.T> t, fs.FS originFS, fs.FS copiedFS) {
    GoFrame ᒐ = default;
    try {
        @string testDir = filepath.Join(t.TempDir(), testˢ);
        // umask doesn't apply to the wasip and windows and there is no general way to get masked perm,
        // so create a dir and a file to compare the permission after umask if any
        var errΔ1 = Mkdir(testDir, ModePerm); if (errΔ1 != nil) {
            return fmt.Errorf("mkdir %q failed: %v"u8, testDir, errΔ1);
        }
        var (dirStat, err) = Stat(testDir);
        if (err != nil) {
            return fmt.Errorf("stat dir %q failed: %v"u8, testDir, err);
        }
        var wantDirMode = dirStat.Mode();

        (var f, err) = Create(filepath.Join(testDir, tmpˢ2));
        if (err != nil) {
            return fmt.Errorf("open %q failed: %v"u8, filepath.Join(testDir, tmpˢ2), err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var wantFileRWStat, err) = f.Stat();
        if (err != nil) {
            return fmt.Errorf("stat file %q failed: %v"u8, f.Name(), err);
        }
        var wantFileRWMode = wantFileRWStat.Mode();

        var wantFileRWStatʗ1 = wantFileRWStat;
        return fs.WalkDir(originFS, ".", error (@string path, fs.DirEntry d, error errΔ2) => {
            GoFrame ᒐ = default;
            try {
                if (d.IsDir()) {
                    // the dir . is not the dir created by CopyFS so skip checking its permission
                    if (d.Name() == "."u8) {
                        return default;
                    }

                    var (dinfo, errΔ3) = fs.Stat(copiedFS, path);
                    if (errΔ3 != nil) {
                        return errΔ3;
                    }

                    if (dinfo.Mode() != wantDirMode) {
                        return fmt.Errorf("dir %q mode is %v, want %v"u8,
                            d.Name(), dinfo.Mode(), wantDirMode);
                    }
                    return default;
                }

                (var fInfo, errΔ2) = originFS.Open(path);
                if (errΔ2 != nil) {
                    return errΔ2;
                }
                var fInfoʗ1 = fInfo;
                defer(() => fInfoʗ1.Close(), ref ᒐ);
                (var copiedInfo, errΔ2) = copiedFS.Open(path);
                if (errΔ2 != nil) {
                    return errΔ2;
                }
                var copiedInfoʗ1 = copiedInfo;
                defer(() => copiedInfoʗ1.Close(), ref ᒐ);

                // verify the file contents are the same
                (var data, errΔ2) = Δio.ReadAll(new os_test_package.fs_FileᴠReader(fInfo));
                if (errΔ2 != nil) {
                    return errΔ2;
                }
                (var newData, errΔ2) = Δio.ReadAll(new os_test_package.fs_FileᴠReader(copiedInfo));
                if (errΔ2 != nil) {
                    return errΔ2;
                }
                if (!bytes.Equal(data, newData)) {
                    return fmt.Errorf("file %q content is %s, want %s"u8, path, newData, data);
                }

                (var fStat, errΔ2) = fInfo.Stat();
                if (errΔ2 != nil) {
                    return errΔ2;
                }
                (var copiedStat, errΔ2) = copiedInfo.Stat();
                if (errΔ2 != nil) {
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
                return default;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string copyfsOutˢ => ref copyfsOutˢᵇ;
private static readonly @string copyfsOutˢᵇ = "copyfs_out"u8;
internal static ref readonly @string fileOutTxtˢ => ref fileOutTxtˢᵇ;
private static readonly @string fileOutTxtˢᵇ = "file.out.txt"u8;
internal static ref readonly @string copyfsInˢ => ref copyfsInˢᵇ;
private static readonly @string copyfsInˢᵇ = "copyfs_in"u8;
internal static ref readonly @string fileInTxtˢ => ref fileInTxtˢᵇ;
private static readonly @string fileInTxtˢᵇ = "file.in.txt"u8;
internal static ref readonly @string inSymlinksˢ => ref inSymlinksˢᵇ;
private static readonly @string inSymlinksˢᵇ = "in_symlinks"u8;
internal static ref readonly @string outSymlinksˢ => ref outSymlinksˢᵇ;
private static readonly @string outSymlinksˢᵇ = "out_symlinks"u8;
internal static ref readonly @string fileAbsOutLinkˢ => ref fileAbsOutLinkˢᵇ;
private static readonly @string fileAbsOutLinkˢᵇ = "file.abs.out.link"u8;
internal static ref readonly @string fileRelOutLinkˢ => ref fileRelOutLinkˢᵇ;
private static readonly @string fileRelOutLinkˢᵇ = "file.rel.out.link"u8;
internal static ref readonly @string fileRelInLinkˢ => ref fileRelInLinkˢᵇ;
private static readonly @string fileRelInLinkˢᵇ = "file.rel.in.link"u8;
internal static ref readonly @string copyfsDupˢ => ref copyfsDupˢᵇ;
private static readonly @string copyfsDupˢᵇ = "copyfs_dup"u8;
internal static ref readonly @string outSymlinksFileAbsOutˢ => ref outSymlinksFileAbsOutˢᵇ;
private static readonly @string outSymlinksFileAbsOutˢᵇ = "out_symlinks/file.abs.out.link"u8;
internal static ref readonly @string outSymlinksFileRelOutˢ => ref outSymlinksFileRelOutˢᵇ;
private static readonly @string outSymlinksFileRelOutˢᵇ = "out_symlinks/file.rel.out.link"u8;
internal static ref readonly @string inSymlinksFileRelInLinkˢ => ref inSymlinksFileRelInLinkˢᵇ;
private static readonly @string inSymlinksFileRelInLinkˢᵇ = "in_symlinks/file.rel.in.link"u8;

public static void TestCopyFSWithSymlinks(ж<Δtesting.T> t) {
    // Test it with absolute and relative symlinks that point inside and outside the tree.
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));

    // Create a directory and file outside.
    @string tmpDir = t.TempDir();
    @string outsideDir = filepath.Join(tmpDir, copyfsOutˢ);
    var errΔ1 = Mkdir(outsideDir, 493); if (errΔ1 != nil) {
        t.Fatalf("Mkdir: %v", errΔ1);
    }
    @string outsideFile = filepath.Join(outsideDir, fileOutTxtˢ);

    var errΔ2 = WriteFile(outsideFile, slice<byte>("Testing CopyFS outside"u8), 420); if (errΔ2 != nil) {
        t.Fatalf("WriteFile: %v", errΔ2);
    }

    // Create a directory and file inside.
    @string insideDir = filepath.Join(tmpDir, copyfsInˢ);
    var errΔ3 = Mkdir(insideDir, 493); if (errΔ3 != nil) {
        t.Fatalf("Mkdir: %v", errΔ3);
    }
    @string insideFile = filepath.Join(insideDir, fileInTxtˢ);
    var errΔ4 = WriteFile(insideFile, slice<byte>("Testing CopyFS inside"u8), 420); if (errΔ4 != nil) {
        t.Fatalf("WriteFile: %v", errΔ4);
    }

    // Create directories for symlinks.
    @string linkInDir = filepath.Join(insideDir, inSymlinksˢ);
    var errΔ5 = Mkdir(linkInDir, 493); if (errΔ5 != nil) {
        t.Fatalf("Mkdir: %v", errΔ5);
    }
    @string linkOutDir = filepath.Join(insideDir, outSymlinksˢ);
    var errΔ6 = Mkdir(linkOutDir, 493); if (errΔ6 != nil) {
        t.Fatalf("Mkdir: %v", errΔ6);
    }

    // First, we create the absolute symlink pointing outside.
    @string outLinkFile = filepath.Join(linkOutDir, fileAbsOutLinkˢ);
    var errΔ7 = Symlink(outsideFile, outLinkFile); if (errΔ7 != nil) {
        t.Fatalf("Symlink: %v", errΔ7);
    }

    // Then, we create the relative symlink pointing outside.
    var (relOutsideFile, err) = filepath.Rel(filepath.Join(linkOutDir, "."), outsideFile);
    if (err != nil) {
        t.Fatalf("filepath.Rel: %v", err);
    }
    @string relOutLinkFile = filepath.Join(linkOutDir, fileRelOutLinkˢ);
    var errΔ8 = Symlink(relOutsideFile, relOutLinkFile); if (errΔ8 != nil) {
        t.Fatalf("Symlink: %v", errΔ8);
    }

    // Last, we create the relative symlink pointing inside.
    (var relInsideFile, err) = filepath.Rel(filepath.Join(linkInDir, "."), insideFile);
    if (err != nil) {
        t.Fatalf("filepath.Rel: %v", err);
    }
    @string relInLinkFile = filepath.Join(linkInDir, fileRelInLinkˢ);
    var errΔ9 = Symlink(relInsideFile, relInLinkFile); if (errΔ9 != nil) {
        t.Fatalf("Symlink: %v", errΔ9);
    }

    // Copy the directory tree and verify.
    forceMFTUpdateOnWindows(t, insideDir);
    var fsys = DirFS(insideDir);
    @string tmpDupDir = filepath.Join(tmpDir, copyfsDupˢ);
    var errΔ10 = Mkdir(tmpDupDir, 493); if (errΔ10 != nil) {
        t.Fatalf("Mkdir: %v", errΔ10);
    }

    var errΔ11 = CopyFS(tmpDupDir, fsys); if (errΔ11 != nil) {
        t.Fatalf("CopyFS: %v", errΔ11);
    }

    forceMFTUpdateOnWindows(t, tmpDupDir);
    var tmpFsys = DirFS(tmpDupDir);
    var errΔ12 = fstest.TestFS(tmpFsys, fileInTxtˢ, outSymlinksFileAbsOutˢ, outSymlinksFileRelOutˢ, inSymlinksFileRelInLinkˢ); if (errΔ12 != nil) {
        t.Fatal(testFSˢ, errΔ12);
    }
    {
        var fsysʗ1 = fsys;
        var tmpFsysʗ1 = tmpFsys;
        var errΔ13 = fs.WalkDir(fsys, ".", error (@string path, fs.DirEntry d, error errΔ14) => {
            if (d.IsDir()) {
                return default;
            }

            (var fi, errΔ14) = d.Info();
            if (errΔ14 != nil) {
                return errΔ14;
            }
            if (filepath.Ext(path) == ".link"u8) {
                if ((fs.FileMode)(fi.Mode() & ModeSymlink) == 0) {
                    return errors.New("original file "u8 + path + " should be a symlink"u8);
                }
                var (tmpfi, errΔ15) = fs.Stat(tmpFsysʗ1, path);
                if (errΔ15 != nil) {
                    return errΔ15;
                }
                if ((fs.FileMode)(tmpfi.Mode() & ModeSymlink) != 0) {
                    return errors.New("copied file "u8 + path + " should not be a symlink"u8);
                }
            }

            (var data, errΔ14) = fs.ReadFile(fsysʗ1, path);
            if (errΔ14 != nil) {
                return errΔ14;
            }
            (var newData, errΔ14) = fs.ReadFile(tmpFsysʗ1, path);
            if (errΔ14 != nil) {
                return errΔ14;
            }
            if (!bytes.Equal(data, newData)) {
                return errors.New("file "u8 + path + " contents differ"u8);
            }

            @string target = default;
            @string fileName = filepath.Base(path);
            var exprᴛ1 = fileName;
            switch (ᐧ) {
            case {} when exprᴛ1 == "file.abs.out.link"u8 || exprᴛ1 == "file.rel.out.link"u8: {
                target = outsideFile;
                break;
            }
            case {} when exprᴛ1 == "file.rel.in.link"u8: {
                target = insideFile;
                break;
            }}
            if (len(target) > 0) {
                var (targetData, errΔ16) = ReadFile(target);
                if (errΔ16 != nil) {
                    return errΔ16;
                }
                if (!bytes.Equal(targetData, newData)) {
                    return errors.New("file "u8 + path + " contents differ from target"u8);
                }
            }

            return default;
        }); if (errΔ13 != nil) {
            t.Fatal(comparingTwoDirectoriesˢ, errΔ13);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;

public static void TestAppendDoesntOverwrite(ж<Δtesting.T> t) {
    testMaybeRooted(t, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        @string name = fileˢ2;
        {
            var errΔ1 = WriteFile(name, slice<byte>("hello"u8), 438); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
        }
        ж<Δos.File> f = default;
        error err = default;
        if (r == nil) {
            (f, err) = OpenFile(name, (nint)(O_APPEND | O_WRONLY), 0);
        } else {
            (f, err) = r.OpenFile(name, (nint)(O_APPEND | O_WRONLY), 0);
        }
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        {
            var (_, errΔ1) = f.Write(slice<byte>(" world"u8)); if (errΔ1 != nil) {
                f.Close();
                tΔ1.Fatal(errΔ1);
            }
        }
        var errΔ2 = f.Close(); if (errΔ2 != nil) {
            tΔ1.Fatal(errΔ2);
        }
        (var got, err) = ReadFile(name);
        if (err != nil) {
            tΔ1.Fatal(err);
        }
        @string want = helloWorldˢ;
        if (((sstring)got) != want) {
            tΔ1.Fatalf("got %q, want %q", got, want);
        }
    });
}

public static void TestRemoveReadOnlyFile(ж<Δtesting.T> t) {
    testMaybeRooted(t, (ж<Δtesting.T> tΔ1, ж<Δos.Root> r) => {
        {
            var errΔ1 = WriteFile(fileˢ2, slice<byte>("1"u8), 0); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
        }
        error err = default;
        if (r == nil) {
            err = Remove(fileˢ2);
        } else {
            err = r.Remove(fileˢ2);
        }
        if (err != nil) {
            tΔ1.Fatalf("Remove read-only file: %v", err);
        }
        {
            var (_, errΔ1) = Stat(fileˢ2); if (!IsNotExist(errΔ1)) {
                tΔ1.Fatalf("Stat read-only file after removal: %v (want IsNotExist)", errΔ1);
            }
        }
    });
}

public static void TestOpenFileDevNull(ж<Δtesting.T> t) {
    // See https://go.dev/issue/71752.
    t.Parallel();

    var (f, err) = OpenFile(DevNull, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_TRUNC), 420);
    if (err != nil) {
        t.Fatalf("OpenFile(DevNull): %v", err);
    }
    f.Close();
}

internal partial struct TestReadFileContents_readStep /*dyn*/ {
    internal nint bufSize; // non-zero to check length of buf to Read
    internal nint retN;    // result of Read call
    internal error retErr; // error result of Read call
}

internal partial struct TestReadFileContents_tests /*dyn*/ {
    internal @string name;
    internal int64 statSize; // size of file to read, per stat (may be 0 for /proc files)
    internal nint wantSize;  // wanted length of []byte from readFileContents
    internal error wantErr;  // wanted error from readFileContents
    internal slice<TestReadFileContents_readStep> reads;
}

public static void TestReadFileContents(ж<Δtesting.T> t) {
    var errFoo = errors.New(fooˢ);
    var tests = slice<TestReadFileContents_tests>([
        new(
            name: "big-file",
            statSize: 2000,
            wantSize: 2000,
            reads: [
                new(bufSize: 2001, retN: 21, retErr: default),
                new(bufSize: 1980, retN: 1979, retErr: Δio.EOF)
            ]
        ),
        new(
            name: "small-file",
            statSize: 100,
            wantSize: 100,
            reads: [
                new(bufSize: 512, retN: 100, retErr: Δio.EOF)
            ]
        ),
        new(
            name: "returning-error",
            statSize: 1000,
            wantSize: 50,
            wantErr: errFoo,
            reads: [
                new(bufSize: 1001, retN: 25, retErr: default),
                new(retN: 25, retErr: errFoo)
            ]
        ),
        new(
            name: "proc-file",
            statSize: 0,
            wantSize: 1023,
            reads: [
                new(bufSize: 512, retN: 512, retErr: default),
                new(retN: 511, retErr: Δio.EOF)
            ]
        ),
        new(
            name: "plan9-iproute-file", // Issue 72080
            statSize: 0,
            wantSize: 1032,
            reads: [
                new(bufSize: 512, retN: 511, retErr: default),
                new(retN: 511, retErr: default),
                new(retN: 10, retErr: Δio.EOF)
            ]
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestReadFileContents_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            ref var remain = ref heap<slice<TestReadFileContents_readStep>>(ttʗ1.reads, out var Ꮡremain);
            nint i = -1;
            var ttʗ2 = ttʗ1;
            var (got, err) = os_internal_test_package.ExportReadFileContents(ttʗ1.statSize, (slice<byte> buf) => {
                i++;
                tΔ1.Logf("read[%d] with buf size %d", i, len(buf));
                if (len(Ꮡremain.ValueSlot) == 0) {
                    tΔ1.Fatalf("unexpected read of length %d after %d expected reads", len(buf), len(ttʗ2.reads));
                }
                if (ttʗ2.statSize == 0 && len(buf) < 512) {
                    // Issue 72080: readFileContents should not do /proc reads with buffers
                    // smaller than 512.
                    tΔ1.Fatalf("read[%d] with buf size %d; want at least 512 for 0-sized file", i, len(buf));
                }
                var step = Ꮡremain.ValueSlot[0];
                Ꮡremain.ValueSlot = Ꮡremain.ValueSlot[1..];
                if (step.bufSize != 0 && len(buf) != step.bufSize) {
                    tΔ1.Fatalf("read[%d] has buffer size %d; want %d", i, len(buf), step.bufSize);
                }
                return (step.retN, step.retErr);
            });
            if (len(Ꮡremain.ValueSlot) > 0) {
                tΔ1.Fatalf("expected %d reads, got %d", len(ttʗ1.reads), i + 1);
            }
            if (fmt.Sprint(err) != fmt.Sprint(ttʗ1.wantErr)) {
                tΔ1.Errorf("got error %v; want %v", err, ttʗ1.wantErr);
            }
            if (len(got) != ttʗ1.wantSize) {
                tΔ1.Errorf("got size %d; want %d", len(got), ttʗ1.wantSize);
            }
        });
    }
}

} // end os_test_package
