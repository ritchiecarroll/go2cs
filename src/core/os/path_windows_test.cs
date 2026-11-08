// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using windows = @internal.syscall.windows_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using filepath = go.path.filepath_package;
using strings = strings_package;
using syscall = syscall_package;
using Δtesting = testing_package;
using @internal;
using @internal.syscall;
using fs = go.io.fs_package;
using go.path;
using static go.os_internal_test_package;

partial class os_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cannotGetCwdˢ = (@string)"cannot get cwd"u8;
internal static ref readonly @string longˢ => ref longˢᵇ;
private static readonly @string longˢᵇ = "long"u8;
internal static ref readonly @string cwdˢ => ref cwdˢᵇ;
private static readonly @string cwdˢᵇ = "cwd"u8;

internal partial struct TestAddExtendedPrefix_type /*dyn*/ {
    internal @string @in, want;
}

public static void TestAddExtendedPrefix(ж<Δtesting.T> t) {
    // Test addExtendedPrefix instead of fixLongPath so the path manipulation code
    // is exercised even if long path are supported by the system, else the
    // function might not be tested at all if/when all test builders support long paths.
    var (cwd, err) = Δos.Getwd();
    if (err != nil) {
        t.Fatal(cannotGetCwdˢ);
    }
    @string drive = strings.ToLower(filepath.VolumeName(cwd));
    cwd = strings.ToLower(cwd.slice(len(drive) + 1));
    // Build a very long pathname. Paths in Go are supposed to be arbitrarily long,
    // so let's make a long path which is comfortably bigger than MAX_PATH on Windows
    // (256) and thus requires fixLongPath to be correctly interpreted in I/O syscalls.
    @string veryLong = "l"u8 + strings.Repeat("o", 500) + "ng"u8;
    foreach (var (_, test) in range(slice<TestAddExtendedPrefix_type>([
        // Test cases use word substitutions:
        //   * "long" is replaced with a very long pathname
        //   * "c:" or "C:" are replaced with the drive of the current directory (preserving case)
        //   * "cwd" is replaced with the current directory

        // Drive Absolute
        new(@"C:\long\foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"C:/long/foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"C:\\\long///foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"C:\long\.\foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"C:\long\..\foo.txt", @"\\?\C:\foo.txt"),
        new(@"C:\long\..\..\foo.txt", @"\\?\C:\foo.txt"),

        // Drive Relative
        new(@"C:long\foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"C:long/foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"C:long///foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"C:long\.\foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"C:long\..\foo.txt", @"\\?\C:\cwd\foo.txt"),

        // Rooted
        new(@"\long\foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"/long/foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"\long///foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"\long\.\foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"\long\..\foo.txt", @"\\?\C:\foo.txt"),

        // Relative
        new(@"long\foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"long/foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"long///foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"long\.\foo.txt", @"\\?\C:\cwd\long\foo.txt"),
        new(@"long\..\foo.txt", @"\\?\C:\cwd\foo.txt"),
        new(@".\long\foo.txt", @"\\?\C:\cwd\long\foo.txt"),

        // UNC Absolute
        new(@"\\srv\share\long", @"\\?\UNC\srv\share\long"),
        new(@"//srv/share/long", @"\\?\UNC\srv\share\long"),
        new(@"/\srv/share/long", @"\\?\UNC\srv\share\long"),
        new(@"\\srv\share\long\", @"\\?\UNC\srv\share\long\"),
        new(@"\\srv\share\bar\.\long", @"\\?\UNC\srv\share\bar\long"),
        new(@"\\srv\share\bar\..\long", @"\\?\UNC\srv\share\long"),
        new(@"\\srv\share\bar\..\..\long", @"\\?\UNC\srv\share\long"), // share name is not removed by ".."

        // Local Device
        new(@"\\.\C:\long\foo.txt", @"\\.\C:\long\foo.txt"),
        new(@"//./C:/long/foo.txt", @"\\.\C:\long\foo.txt"),
        new(@"/\./C:/long/foo.txt", @"\\.\C:\long\foo.txt"),
        new(@"\\.\C:\long///foo.txt", @"\\.\C:\long\foo.txt"),
        new(@"\\.\C:\long\.\foo.txt", @"\\.\C:\long\foo.txt"),
        new(@"\\.\C:\long\..\foo.txt", @"\\.\C:\foo.txt"),

        // Misc tests
        new(@"C:\short.txt", @"C:\short.txt"),
        new(@"C:\", @"C:\"),
        new(@"C:", @"C:"),
        new(@"\\srv\path", @"\\srv\path"),
        new(@"long.txt", @"\\?\C:\cwd\long.txt"),
        new(@"C:long.txt", @"\\?\C:\cwd\long.txt"),
        new(@"C:\long\.\bar\baz", @"\\?\C:\long\bar\baz"),
        new(@"C:long\.\bar\baz", @"\\?\C:\cwd\long\bar\baz"),
        new(@"C:\long\..\bar\baz", @"\\?\C:\bar\baz"),
        new(@"C:long\..\bar\baz", @"\\?\C:\cwd\bar\baz"),
        new(@"C:\long\foo\\bar\.\baz\\", @"\\?\C:\long\foo\bar\baz\"),
        new(@"C:\long\..", @"\\?\C:\"),
        new(@"C:\.\long\..\.", @"\\?\C:\"),
        new(@"\\?\C:\long\foo.txt", @"\\?\C:\long\foo.txt"),
        new(@"\\?\C:\long/foo.txt", @"\\?\C:\long/foo.txt")
    ]))) {
        @string @in = strings.ReplaceAll(test.@in, longˢ, veryLong);
        @in = strings.ToLower(@in);
        @in = strings.ReplaceAll(@in, "c:", drive);

        @string want = strings.ReplaceAll(test.want, longˢ, veryLong);
        want = strings.ToLower(want);
        want = strings.ReplaceAll(want, "c:", drive);
        want = strings.ReplaceAll(want, cwdˢ, cwd);

        @string got = os_internal_test_package.AddExtendedPrefix(@in);
        got = strings.ToLower(got);
        if (got != want) {
            @in = strings.ReplaceAll(@in, veryLong, longˢ);
            got = strings.ReplaceAll(got, veryLong, longˢ);
            want = strings.ReplaceAll(want, veryLong, longˢ);
            t.Errorf("addExtendedPrefix(%#q) = %#q; want %#q", @in, got, want);
        }
    }
}

public static void TestMkdirAllLongPath(ж<Δtesting.T> t) {
    t.Parallel();

    @string tmpDir = t.TempDir();
    @string path = tmpDir;
    for (nint i = 0; i < 100; i++) {
        path += @"\another-path-component"u8;
    }
    {
        var err = Δos.MkdirAll(path, 511); if (err != nil) {
            t.Fatalf("MkdirAll(%q) failed; %v", path, err);
        }
    }
    {
        var err = Δos.RemoveAll(tmpDir); if (err != nil) {
            t.Fatalf("RemoveAll(%q) failed; %v", tmpDir, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prefixᶜ => ref prefixᶜᵇ;
private static readonly @string prefixᶜᵇ = @"\\?\"u8;

public static void TestMkdirAllExtendedLength(ж<Δtesting.T> t) {
    t.Parallel();
    @string tmpDir = t.TempDir();

    @string prefix = prefixᶜ;
    if (len(tmpDir) < 4 || tmpDir[..4] != prefix) {
        var (fullPath, err) = syscall.FullPath(tmpDir);
        if (err != nil) {
            t.Fatalf("FullPath(%q) fails: %v", tmpDir, err);
        }
        tmpDir = prefix + fullPath;
    }
    @string path = tmpDir + @"\dir\"u8;
    {
        var err = Δos.MkdirAll(path, 511); if (err != nil) {
            t.Fatalf("MkdirAll(%q) failed: %v", path, err);
        }
    }

    path = path + @".\dir2"u8;
    {
        var err = Δos.MkdirAll(path, 511); if (err == nil) {
            t.Fatalf("MkdirAll(%q) should have failed, but did not", path);
        }
    }
}

public static void TestOpenRootSlash(ж<Δtesting.T> t) {
    t.Parallel();

    var tests = slice<@string>([
        @"/",
        @"\"
    ]);

    foreach (var (_, test) in range(tests)) {
        var (dir, err) = Δos.Open(test);
        if (err != nil) {
            t.Fatalf("Open(%q) failed: %v", test, err);
        }
        dir.Close();
    }
}

internal static void testMkdirAllAtRoot(ж<Δtesting.T> t, @string root) {
    // Create a unique-enough directory name in root.
    @string @base = fmt.Sprintf("%s-%d"u8, t.Name(), Δos.Getpid());
    @string path = filepath.Join(root, @base);
    {
        var err = Δos.MkdirAll(path, 511); if (err != nil) {
            t.Fatalf("MkdirAll(%q) failed: %v", path, err);
        }
    }
    // Clean up
    {
        var err = Δos.RemoveAll(path); if (err != nil) {
            t.Fatal(err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prefixᶜ1 => ref prefixᶜ1ᵇ;
private static readonly @string prefixᶜ1ᵇ = @"\\?\"u8;

public static void TestMkdirAllExtendedLengthAtRoot(ж<Δtesting.T> t) {
    if (testenv.Builder() == ""u8) {
        t.Skipf("skipping non-hermetic test outside of Go builders");
    }

    @string prefix = prefixᶜ1;
    @string vol = filepath.VolumeName(t.TempDir()) + @"\"u8;
    if (len(vol) < 4 || vol[..4] != prefix) {
        vol = prefix + vol;
    }
    testMkdirAllAtRoot(t, vol);
}

public static void TestMkdirAllVolumeNameAtRoot(ж<Δtesting.T> t) {
    if (testenv.Builder() == ""u8) {
        t.Skipf("skipping non-hermetic test outside of Go builders");
    }

    var (vol, err) = syscall.UTF16PtrFromString(filepath.VolumeName(t.TempDir()) + @"\"u8);
    if (err != nil) {
        t.Fatal(err);
    }
    UntypedInt maxVolNameLen = 50;
    ref var buf = ref heap(new array<uint16>(50), out var Ꮡbuf);
    err = windows.GetVolumeNameForVolumeMountPoint(vol, Ꮡbuf.at<uint16>(0), maxVolNameLen);
    if (err != nil) {
        t.Fatal(err);
    }
    @string volName = syscall.UTF16ToString(buf[..]);
    testMkdirAllAtRoot(t, volName);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;

public static void TestRemoveAllLongPathRelative(ж<Δtesting.T> t) {
    // Test that RemoveAll doesn't hang with long relative paths.
    // See go.dev/issue/36375.
    @string tmp = t.TempDir();
    t.Chdir(tmp);
    @string dir = filepath.Join(tmp, fooˢ, barˢ, strings.Repeat("a", 150), strings.Repeat("b", 150));
    var err = Δos.MkdirAll(dir, 493);
    if (err != nil) {
        t.Fatal(err);
    }
    err = Δos.RemoveAll(fooˢ);
    if (err != nil) {
        t.Fatal(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string file1ˢ => ref file1ˢᵇ;
private static readonly @string file1ˢᵇ = "file1"u8;
internal static ref readonly @string file2ˢ => ref file2ˢᵇ;
private static readonly @string file2ˢᵇ = "file2"u8;

public static void TestRemoveAllFallback(ж<Δtesting.T> t) {
    windows.TestDeleteatFallback = true;
    t.Cleanup(() => {
        windows.TestDeleteatFallback = false;
    });

    @string dir = t.TempDir();
    {
        var err = Δos.WriteFile(filepath.Join(dir, file1ˢ), [], 448); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = Δos.WriteFile(filepath.Join(dir, file2ˢ), [], 256); if (err != nil) { // read-only file
            t.Fatal(err);
        }
    }

    {
        var err = Δos.RemoveAll(dir); if (err != nil) {
            t.Fatal(err);
        }
    }
}

internal static void testLongPathAbs(ж<Δtesting.T> t, @string target) {
    t.Helper();
    var testWalkFn = (@string path, fs.FileInfo info, error err) => {
        if (err != nil) {
            t.Error(err);
        }
        return err;
    };
    {
        var err = Δos.MkdirAll(target, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    // Test that Walk doesn't fail with long paths.
    // See go.dev/issue/21782.
    filepath.Walk(target, new Func<@string, fs.FileInfo, error, error>(testWalkFn));
    // Test that RemoveAll doesn't hang with long paths.
    // See go.dev/issue/36375.
    {
        var err = Δos.RemoveAll(target); if (err != nil) {
            t.Error(err);
        }
    }
}

public static void TestLongPathAbs(ж<Δtesting.T> t) {
    t.Parallel();

    @string target = t.TempDir() + "\\"u8 + strings.Repeat("a\\", 300);
    testLongPathAbs(t, target);
}

public static void TestLongPathRel(ж<Δtesting.T> t) {
    t.Chdir(t.TempDir());

    @string target = strings.Repeat("b\\", 300);
    testLongPathAbs(t, target);
}

public static void BenchmarkAddExtendedPrefix(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string veryLong = @"C:\l"u8 + strings.Repeat("o", 248) + "ng"u8;
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        os_internal_test_package.AddExtendedPrefix(veryLong);
    }
}

} // end os_test_package
