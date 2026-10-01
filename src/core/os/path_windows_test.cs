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
internal static readonly @string longˢ = "long"u8;
internal static readonly @string cwdˢ = "cwd"u8;

[GoType("dyn")] internal partial struct TestAddExtendedPrefix_type {
    internal @string @in, want;
}

public static void TestAddExtendedPrefix(ж<Δtesting.T> Ꮡt) {
    // Test addExtendedPrefix instead of fixLongPath so the path manipulation code
    // is exercised even if long path are supported by the system, else the
    // function might not be tested at all if/when all test builders support long paths.
    var (cwd, err) = Δos.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(cannotGetCwdˢ);
    }
    @string drive = strings.ToLower(filepath.VolumeName(cwd));
    cwd = strings.ToLower(cwd.slice(len(drive) + 1));
    // Build a very long pathname. Paths in Go are supposed to be arbitrarily long,
    // so let's make a long path which is comfortably bigger than MAX_PATH on Windows
    // (256) and thus requires fixLongPath to be correctly interpreted in I/O syscalls.
    @string veryLong = "l"u8 + strings.Repeat("o"u8, 500) + "ng"u8;
    foreach (var (_, test) in new TestAddExtendedPrefix_type[]{ // Test cases use word substitutions:
 //   * "long" is replaced with a very long pathname
 //   * "c:" or "C:" are replaced with the drive of the current directory (preserving case)
 //   * "cwd" is replaced with the current directory
 // Drive Absolute

        new(@"C:\long\foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"C:/long/foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"C:\\\long///foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"C:\long\.\foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"C:\long\..\foo.txt"u8, @"\\?\C:\foo.txt"u8),
        new(@"C:\long\..\..\foo.txt"u8, @"\\?\C:\foo.txt"u8), // Drive Relative

        new(@"C:long\foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"C:long/foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"C:long///foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"C:long\.\foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"C:long\..\foo.txt"u8, @"\\?\C:\cwd\foo.txt"u8), // Rooted

        new(@"\long\foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"/long/foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"\long///foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"\long\.\foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"\long\..\foo.txt"u8, @"\\?\C:\foo.txt"u8), // Relative

        new(@"long\foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"long/foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"long///foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"long\.\foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8),
        new(@"long\..\foo.txt"u8, @"\\?\C:\cwd\foo.txt"u8),
        new(@".\long\foo.txt"u8, @"\\?\C:\cwd\long\foo.txt"u8), // UNC Absolute

        new(@"\\srv\share\long"u8, @"\\?\UNC\srv\share\long"u8),
        new(@"//srv/share/long"u8, @"\\?\UNC\srv\share\long"u8),
        new(@"/\srv/share/long"u8, @"\\?\UNC\srv\share\long"u8),
        new(@"\\srv\share\long\"u8, @"\\?\UNC\srv\share\long\"u8),
        new(@"\\srv\share\bar\.\long"u8, @"\\?\UNC\srv\share\bar\long"u8),
        new(@"\\srv\share\bar\..\long"u8, @"\\?\UNC\srv\share\long"u8),
        new(@"\\srv\share\bar\..\..\long"u8, @"\\?\UNC\srv\share\long"u8), // share name is not removed by ".."
 // Local Device

        new(@"\\.\C:\long\foo.txt"u8, @"\\.\C:\long\foo.txt"u8),
        new(@"//./C:/long/foo.txt"u8, @"\\.\C:\long\foo.txt"u8),
        new(@"/\./C:/long/foo.txt"u8, @"\\.\C:\long\foo.txt"u8),
        new(@"\\.\C:\long///foo.txt"u8, @"\\.\C:\long\foo.txt"u8),
        new(@"\\.\C:\long\.\foo.txt"u8, @"\\.\C:\long\foo.txt"u8),
        new(@"\\.\C:\long\..\foo.txt"u8, @"\\.\C:\foo.txt"u8), // Misc tests

        new(@"C:\short.txt"u8, @"C:\short.txt"u8),
        new(@"C:\"u8, @"C:\"u8),
        new(@"C:"u8, @"C:"u8),
        new(@"\\srv\path"u8, @"\\srv\path"u8),
        new(@"long.txt"u8, @"\\?\C:\cwd\long.txt"u8),
        new(@"C:long.txt"u8, @"\\?\C:\cwd\long.txt"u8),
        new(@"C:\long\.\bar\baz"u8, @"\\?\C:\long\bar\baz"u8),
        new(@"C:long\.\bar\baz"u8, @"\\?\C:\cwd\long\bar\baz"u8),
        new(@"C:\long\..\bar\baz"u8, @"\\?\C:\bar\baz"u8),
        new(@"C:long\..\bar\baz"u8, @"\\?\C:\cwd\bar\baz"u8),
        new(@"C:\long\foo\\bar\.\baz\\"u8, @"\\?\C:\long\foo\bar\baz\"u8),
        new(@"C:\long\.."u8, @"\\?\C:\"u8),
        new(@"C:\.\long\..\."u8, @"\\?\C:\"u8),
        new(@"\\?\C:\long\foo.txt"u8, @"\\?\C:\long\foo.txt"u8),
        new(@"\\?\C:\long/foo.txt"u8, @"\\?\C:\long/foo.txt"u8)
    }.slice()) {
        @string @in = strings.ReplaceAll(test.@in, longˢ, veryLong);
        @in = strings.ToLower(@in);
        @in = strings.ReplaceAll(@in, "c:"u8, drive);
        @string want = strings.ReplaceAll(test.want, longˢ, veryLong);
        want = strings.ToLower(want);
        want = strings.ReplaceAll(want, "c:"u8, drive);
        want = strings.ReplaceAll(want, cwdˢ, cwd);
        @string got = os_internal_test_package.AddExtendedPrefix(@in);
        got = strings.ToLower(got);
        if (got != want) {
            @in = strings.ReplaceAll(@in, veryLong, longˢ);
            got = strings.ReplaceAll(got, veryLong, longˢ);
            want = strings.ReplaceAll(want, veryLong, longˢ);
            Ꮡt.Errorf("addExtendedPrefix(%#q) = %#q; want %#q"u8, @in, got, want);
        }
    }
}

public static void TestMkdirAllLongPath(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    @string path = tmpDir;
    for (nint i = 0; i < 100; i++) {
        path += @"\another-path-component"u8;
    }
    {
        var err = Δos.MkdirAll(path, 511); if (err != default!) {
            Ꮡt.Fatalf("MkdirAll(%q) failed; %v"u8, path, err);
        }
    }
    {
        var err = Δos.RemoveAll(tmpDir); if (err != default!) {
            Ꮡt.Fatalf("RemoveAll(%q) failed; %v"u8, tmpDir, err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ = @"\\?\"u8;

public static void TestMkdirAllExtendedLength(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string tmpDir = Ꮡt.TempDir();
    @string prefix = prefixᶜ;
    if (len(tmpDir) < 4 || tmpDir[..4] != prefix) {
        var (fullPath, err) = syscall.FullPath(tmpDir);
        if (err != default!) {
            Ꮡt.Fatalf("FullPath(%q) fails: %v"u8, tmpDir, err);
        }
        tmpDir = prefix + fullPath;
    }
    @string path = tmpDir + @"\dir\"u8;
    {
        var err = Δos.MkdirAll(path, 511); if (err != default!) {
            Ꮡt.Fatalf("MkdirAll(%q) failed: %v"u8, path, err);
        }
    }
    path = path + @".\dir2"u8;
    {
        var err = Δos.MkdirAll(path, 511); if (err == default!) {
            Ꮡt.Fatalf("MkdirAll(%q) should have failed, but did not"u8, path);
        }
    }
}

public static void TestOpenRootSlash(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var tests = new @string[]{
        @"/"u8,
        @"\"u8
    }.slice();
    foreach (var (_, test) in tests) {
        var (dir, err) = Δos.Open(test);
        if (err != default!) {
            Ꮡt.Fatalf("Open(%q) failed: %v"u8, test, err);
        }
        dir.Close();
    }
}

internal static void testMkdirAllAtRoot(ж<Δtesting.T> Ꮡt, @string root) {
    // Create a unique-enough directory name in root.
    @string @base = fmt.Sprintf("%s-%d"u8, Ꮡt.Name(), Δos.Getpid());
    @string path = filepath.Join(root, @base);
    {
        var err = Δos.MkdirAll(path, 511); if (err != default!) {
            Ꮡt.Fatalf("MkdirAll(%q) failed: %v"u8, path, err);
        }
    }
    // Clean up
    {
        var err = Δos.RemoveAll(path); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ1 = @"\\?\"u8;

public static void TestMkdirAllExtendedLengthAtRoot(ж<Δtesting.T> Ꮡt) {
    if (testenv.Builder() == ""u8) {
        Ꮡt.Skipf("skipping non-hermetic test outside of Go builders"u8);
    }
    @string prefix = prefixᶜ1;
    @string vol = filepath.VolumeName(Ꮡt.TempDir()) + @"\"u8;
    if (len(vol) < 4 || vol[..4] != prefix) {
        vol = prefix + vol;
    }
    testMkdirAllAtRoot(Ꮡt, vol);
}

public static void TestMkdirAllVolumeNameAtRoot(ж<Δtesting.T> Ꮡt) {
    if (testenv.Builder() == ""u8) {
        Ꮡt.Skipf("skipping non-hermetic test outside of Go builders"u8);
    }
    var (vol, err) = syscall.UTF16PtrFromString(filepath.VolumeName(Ꮡt.TempDir()) + @"\"u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    UntypedInt maxVolNameLen = 50;
    ref var buf = ref heap(new array<uint16>(50), out var Ꮡbuf);
    err = windows.GetVolumeNameForVolumeMountPoint(vol, Ꮡbuf.at<uint16>(0), maxVolNameLen);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string volName = syscall.UTF16ToString(buf[..]);
    testMkdirAllAtRoot(Ꮡt, volName);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;

public static void TestRemoveAllLongPathRelative(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that RemoveAll doesn't hang with long relative paths.
    // See go.dev/issue/36375.
    @string tmp = Ꮡt.TempDir();
    Ꮡt.Chdir(tmp);
    @string dir = filepath.Join(tmp, fooˢ, barˢ, strings.Repeat("a"u8, 150), strings.Repeat("b"u8, 150));
    var err = Δos.MkdirAll(dir, 493);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    err = Δos.RemoveAll(fooˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

internal static void testLongPathAbs(ж<Δtesting.T> Ꮡt, @string target) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Helper();
    var testWalkFn = (@string path, fs.FileInfo info, error err) => {
        if (err != default!) {
            Ꮡt.Error(err);
        }
        return err;
    };
    {
        var err = Δos.MkdirAll(target, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    // Test that Walk doesn't fail with long paths.
    // See go.dev/issue/21782.
    filepath.Walk(target, new Func<@string, fs.FileInfo, error, error>(testWalkFn));
    // Test that RemoveAll doesn't hang with long paths.
    // See go.dev/issue/36375.
    {
        var err = Δos.RemoveAll(target); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestLongPathAbs(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    @string target = Ꮡt.TempDir() + "\\"u8 + strings.Repeat("a\\"u8, 300);
    testLongPathAbs(Ꮡt, target);
}

public static void TestLongPathRel(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Chdir(Ꮡt.TempDir());
    @string target = strings.Repeat("b\\"u8, 300);
    testLongPathAbs(Ꮡt, target);
}

public static void BenchmarkAddExtendedPrefix(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string veryLong = @"C:\l"u8 + strings.Repeat("o"u8, 248) + "ng"u8;
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        os_internal_test_package.AddExtendedPrefix(veryLong);
    }
}

} // end os_test_package
