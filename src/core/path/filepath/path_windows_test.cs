// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using flag = flag_package;
using fmt = fmt_package;
using godebug = @internal.godebug_package;
using testenv = @internal.testenv_package;
using fs = io.fs_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using debug = go.runtime.debug_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.os;
using go.path;
using go.runtime;
using io;
using static go.path.filepath_internal_test_package;

partial class filepath_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string comSpecˢ => ref comSpecˢᵇ;
private static readonly @string comSpecˢᵇ = "ComSpec"u8;
internal static readonly object comSpecMustBeSetˢ = (@string)"%ComSpec% must be set"u8;

public static void TestWinSplitListTestsAreValid(ж<testing.T> t) {
    @string comspec = os.Getenv(comSpecˢ);
    if (comspec == ""u8) {
        t.Fatal(comSpecMustBeSetˢ);
    }

    foreach (var (ti, tt) in winsplitlisttests) {
        testWinSplitListTestIsValid(t, ti, tt, comspec);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string systemRootˢ => ref systemRootˢᵇ;
private static readonly @string systemRootˢᵇ = "SystemRoot"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string cmdfileᶜ => ref cmdfileᶜᵇ;
private static readonly @string cmdfileᶜᵇ = @"printdir.cmd"u8;

internal static void testWinSplitListTestIsValid(ж<testing.T> t, nint ti, SplitListTest tt, @string comspec) {
    @string cmdfile = cmdfileᶜ;
    fs.FileMode perm = /* 0700 */ 448;

    ref var tmp = ref heap<@string>(t.TempDir(), out var Ꮡtmp);
    foreach (var (i, d) in tt.result) {
        if (d == ""u8) {
            continue;
        }
        {
            @string cd = filepath.Clean(d); if (filepath.VolumeName(cd) != ""u8 || cd[0] == '\\' || cd == ".."u8 || (len(cd) >= 3 && cd[0..3] == @"..\")) {
                t.Errorf("%d,%d: %#q refers outside working directory", ti, i, d);
                return;
            }
        }
        @string dd = filepath.Join(tmp, d);
        {
            var (_, err) = os.Stat(dd); if (err == nil) {
                t.Errorf("%d,%d: %#q already exists", ti, i, d);
                return;
            }
        }
        {
            var err = os.MkdirAll(dd, perm); if (err != nil) {
                t.Errorf("%d,%d: MkdirAll(%#q) failed: %v", ti, i, dd, err);
                return;
            }
        }
        @string fn = filepath.Join(dd, cmdfile);
        var data = slice<byte>("@echo " + d + "\r\n");
        {
            var err = os.WriteFile(fn, data, perm); if (err != nil) {
                t.Errorf("%d,%d: WriteFile(%#q) failed: %v", ti, i, fn, err);
                return;
            }
        }
    }

    // on some systems, SystemRoot is required for cmd to work
    @string systemRoot = os.Getenv(systemRootˢ);

    foreach (var (i, d) in tt.result) {
        if (d == ""u8) {
            continue;
        }
        var exp = slice<byte>(d + "\r\n");
        var cmd = Ꮡ(new exec.Cmd(
            Path: comspec,
            Args: slice<@string>([@"/c", cmdfile]),
            Env: slice<@string>([@"Path="u8 + systemRoot + "/System32;"u8 + tt.list, @"SystemRoot="u8 + systemRoot]),
            Dir: tmp
        ));
        var (@out, err) = cmd.CombinedOutput();
        switch (ᐧ) {
        case {} when err != nil: {
            t.Errorf("%d,%d: execution error %v\n%q", ti, i, err, @out);
            return;
        }
        case {} when !slices.Equal<slice<byte>, byte>(@out, exp): {
            t.Errorf("%d,%d: expected %#q, got %#q", ti, i, exp, @out);
            return;
        }
        default: {
            // unshadow cmdfile in next directory
            err = os.Remove(filepath.Join(tmp, d, cmdfile));
            if (err != nil) {
                t.Fatalf("Remove test command failed: %v", err);
            }
            break;
        }}
    }
}

public static void TestWindowsEvalSymlinks(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    @string tmpDir = tempDirCanonical(t);

    if (len(tmpDir) < 3) {
        t.Fatalf("tmpDir path %q is too short", tmpDir);
    }
    if (tmpDir[1] != ':') {
        t.Fatalf("tmpDir path %q must have drive letter in it", tmpDir);
    }
    var test = new EvalSymlinksTest("test/linkabswin", tmpDir[..3]);

    // Create the symlink farm using relative paths.
    var testdirs = append(EvalSymlinksTestDirs, test);
    foreach (var (_, d) in testdirs) {
        error err = default;
        @string pathΔ1 = simpleJoin(tmpDir, d.path);
        if (d.dest == ""u8) {
            err = os.Mkdir(pathΔ1, 493);
        } else {
            err = os.Symlink(d.dest, pathΔ1);
        }
        if (err != nil) {
            t.Fatal(err);
        }
    }

    @string path = simpleJoin(tmpDir, test.path);

    testEvalSymlinks(t, path, test.dest);

    testEvalSymlinksAfterChdir(t, path, ".", test.dest);

    testEvalSymlinksAfterChdir(t,
        path,
        filepath.VolumeName(tmpDir) + "."u8,
        test.dest);

    testEvalSymlinksAfterChdir(t,
        simpleJoin(tmpDir, testˢ),
        simpleJoin("..", test.path),
        test.dest);

    testEvalSymlinksAfterChdir(t, tmpDir, test.path, test.dest);
}

// TestEvalSymlinksCanonicalNames verify that EvalSymlinks
// returns "canonical" path names on windows.
public static void TestEvalSymlinksCanonicalNames(ж<testing.T> t) {
    @string ctmp = tempDirCanonical(t);
    var dirs = slice<@string>([
        "test",
        "test/dir",
        "testing_long_dir",
        "TEST2"
    ]);

    foreach (var (_, d) in dirs) {
        @string dir = filepath.Join(ctmp, d);
        var err = os.Mkdir(dir, 493);
        if (err != nil) {
            t.Fatal(err);
        }
        (var cname, err) = filepath.EvalSymlinks(dir);
        if (err != nil) {
            t.Errorf("EvalSymlinks(%q) error: %v", dir, err);
            continue;
        }
        if (dir != cname) {
            t.Errorf("EvalSymlinks(%q) returns %q, but should return %q", dir, cname, dir);
            continue;
        }
        // test non-canonical names
        @string test = strings.ToUpper(dir);
        (var p, err) = filepath.EvalSymlinks(test);
        if (err != nil) {
            t.Errorf("EvalSymlinks(%q) error: %v", test, err);
            continue;
        }
        if (p != cname) {
            t.Errorf("EvalSymlinks(%q) returns %q, but should return %q", test, p, cname);
            continue;
        }
        // another test
        test = strings.ToLower(dir);
        (p, err) = filepath.EvalSymlinks(test);
        if (err != nil) {
            t.Errorf("EvalSymlinks(%q) error: %v", test, err);
            continue;
        }
        if (p != cname) {
            t.Errorf("EvalSymlinks(%q) returns %q, but should return %q", test, p, cname);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fsutilˢ => ref fsutilˢᵇ;
private static readonly @string fsutilˢᵇ = "fsutil"u8;
internal static ref readonly @string queryˢ => ref queryˢᵇ;
private static readonly @string queryˢᵇ = "query"u8;
internal static ref readonly @string theRegistryStateOfˢ => ref theRegistryStateOfˢᵇ;
private static readonly @string theRegistryStateOfˢᵇ = "The registry state of NtfsDisable8dot3NameCreation is 2, the default (Volume level setting)"u8;
internal static ref readonly @string theRegistryStateIs2Perˢ => ref theRegistryStateIs2Perˢᵇ;
private static readonly @string theRegistryStateIs2Perˢᵇ = "The registry state is: 2 (Per volume setting - the default)"u8;
internal static ref readonly @string basedOnTheAboveTwoˢ => ref basedOnTheAboveTwoˢᵇ;
private static readonly @string basedOnTheAboveTwoˢᵇ = "Based on the above two settings, 8dot3 name creation is %s on %s"u8;
internal static readonly object enabledˢ = (@string)"enabled"u8;
internal static readonly object disabledˢ = (@string)"disabled"u8;

// checkVolume8dot3Setting runs "fsutil 8dot3name query c:" command
// (where c: is vol parameter) to discover "8dot3 name creation state".
// The state is combination of 2 flags. The global flag controls if it
// is per volume or global setting:
//
//	0 - Enable 8dot3 name creation on all volumes on the system
//	1 - Disable 8dot3 name creation on all volumes on the system
//	2 - Set 8dot3 name creation on a per volume basis
//	3 - Disable 8dot3 name creation on all volumes except the system volume
//
// If global flag is set to 2, then per-volume flag needs to be examined:
//
//	0 - Enable 8dot3 name creation on this volume
//	1 - Disable 8dot3 name creation on this volume
//
// checkVolume8dot3Setting verifies that "8dot3 name creation" flags
// are set to 2 and 0, if enabled parameter is true, or 2 and 1, if enabled
// is false. Otherwise checkVolume8dot3Setting returns error.
internal static error checkVolume8dot3Setting(@string vol, bool enabled) {
    // It appears, on some systems "fsutil 8dot3name query ..." command always
    // exits with error. Ignore exit code, and look at fsutil output instead.
    var (@out, _) = exec.Command(fsutilˢ, "8dot3name", queryˢ, vol).CombinedOutput();
    // Check that system has "Volume level setting" set.
    @string expected = theRegistryStateOfˢ;
    if (!strings.Contains((@string)@out, expected)) {
        // Windows 10 version of fsutil has different output message.
        @string expectedWindow10 = theRegistryStateIs2Perˢ;
        if (!strings.Contains((@string)@out, expectedWindow10)) {
            return fmt.Errorf("fsutil output should contain %q, but is %q"u8, expected, (@string)@out);
        }
    }
    // Now check the volume setting.
    expected = basedOnTheAboveTwoˢ;
    if (enabled) {
        expected = fmt.Sprintf(expected, enabledˢ, vol);
    } else {
        expected = fmt.Sprintf(expected, disabledˢ, vol);
    }
    if (!strings.Contains((@string)@out, expected)) {
        return fmt.Errorf("unexpected fsutil output: %q"u8, (@string)@out);
    }
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string successfullyS8dot3nameˢ => ref successfullyS8dot3nameˢᵇ;
private static readonly @string successfullyS8dot3nameˢᵇ = "Successfully %s 8dot3name generation on %s\r\n"u8;

internal static error setVolume8dot3Setting(@string vol, bool enabled) {
    var cmd = slice<@string>(["fsutil", "8dot3name", "set", vol]);
    if (enabled) {
        cmd = append(cmd, (@string)"0");
    } else {
        cmd = append(cmd, (@string)"1");
    }
    // It appears, on some systems "fsutil 8dot3name set ..." command always
    // exits with error. Ignore exit code, and look at fsutil output instead.
    var (@out, _) = exec.Command(cmd[0], cmd[1..].ꓸꓸꓸ).CombinedOutput();
    sstring @outᴛ1 = ((sstring)@out);
    if (@outᴛ1 != "\r\nSuccessfully set 8dot3name behavior.\r\n"u8) {
        // Windows 10 version of fsutil has different output message.
        @string expectedWindow10 = successfullyS8dot3nameˢ;
        if (enabled) {
            expectedWindow10 = fmt.Sprintf(expectedWindow10, enabledˢ, vol);
        } else {
            expectedWindow10 = fmt.Sprintf(expectedWindow10, disabledˢ, vol);
        }
        if (@outᴛ1 != expectedWindow10) {
            return fmt.Errorf("%v command failed: %q"u8, cmd, (@string)@out);
        }
    }
    return default;
}

internal static ж<bool> runFSModifyTests = flag.Bool("run_fs_modify_tests", false, "run tests which modify filesystem parameters");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestThatModifiesˢ = (@string)"skipping test that modifies file system setting; enable with -run_fs_modify_tests"u8;

// This test assumes registry state of NtfsDisable8dot3NameCreation is 2,
// the default (Volume level setting).
public static void TestEvalSymlinksCanonicalNamesWith8dot3Disabled(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (!runFSModifyTests.Value) {
            t.Skip(skippingTestThatModifiesˢ);
        }
        @string tempVol = filepath.VolumeName(os.TempDir());
        if (len(tempVol) != 2) {
            t.Fatalf("unexpected temp volume name %q", tempVol);
        }

        var err = checkVolume8dot3Setting(tempVol, true);
        if (err != nil) {
            t.Fatal(err);
        }
        err = setVolume8dot3Setting(tempVol, false);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(() => {
            var errΔ1 = setVolume8dot3Setting(tempVol, true);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            errΔ1 = checkVolume8dot3Setting(tempVol, true);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }, ref ᒐ);
        err = checkVolume8dot3Setting(tempVol, false);
        if (err != nil) {
            t.Fatal(err);
        }
        TestEvalSymlinksCanonicalNames(t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpTestFooBarˢ => ref tmpTestFooBarˢᵇ;
private static readonly @string tmpTestFooBarˢᵇ = @"{{tmp}}\test\foo\bar"u8;
internal static ref readonly @string tmpˢ => ref tmpˢᵇ;
private static readonly @string tmpˢᵇ = "{{tmp}}"u8;
internal static ref readonly @string tmpvolˢ => ref tmpvolˢᵇ;
private static readonly @string tmpvolˢᵇ = "{{tmpvol}}"u8;
internal static ref readonly @string tmpnovolˢ => ref tmpnovolˢᵇ;
private static readonly @string tmpnovolˢᵇ = "{{tmpnovol}}"u8;

internal partial struct TestToNorm_tests /*dyn*/ {
    internal @string arg;
    internal @string want;
}

internal partial struct TestToNorm_testsDir /*dyn*/ {
    internal @string wd;
    internal @string arg;
    internal @string want;
}

public static void TestToNorm(ж<testing.T> t) {
    var stubBase = (@string, error) (@string path) => {
        @string vol = filepath.VolumeName(path);
        path = path.slice(len(vol));

        if (strings.Contains(path, "/")) {
            return ("", fmt.Errorf("invalid path is given to base: %s"u8, vol + path));
        }

        if (path == ""u8 || path == "."u8 || path == @"\"u8) {
            return ("", fmt.Errorf("invalid path is given to base: %s"u8, vol + path));
        }

        nint i = strings.LastIndexByte(path, filepath.Separator);
        if (i == len(path) - 1) { // trailing '\' is invalid
            return ("", fmt.Errorf("invalid path is given to base: %s"u8, vol + path));
        }
        if (i == -1) {
            return (strings.ToUpper(path), default);
        }

        return (strings.ToUpper(path.slice(i + 1)), default);
    };

    // On this test, toNorm should be same as string.ToUpper(filepath.Clean(path)) except empty string.
    var tests = slice<TestToNorm_tests>([
        new("", ""),
        new(".", "."),
        new("./foo/bar", @"FOO\BAR"),
        new("/", @"\"),
        new("/foo/bar", @"\FOO\BAR"),
        new("/foo/bar/baz/qux", @"\FOO\BAR\BAZ\QUX"),
        new("foo/bar", @"FOO\BAR"),
        new("C:/foo/bar", @"C:\FOO\BAR"),
        new("C:foo/bar", @"C:FOO\BAR"),
        new("c:/foo/bar", @"C:\FOO\BAR"),
        new("C:/foo/bar", @"C:\FOO\BAR"),
        new("C:/foo/bar/", @"C:\FOO\BAR"),
        new(@"C:\foo\bar", @"C:\FOO\BAR"),
        new(@"C:\foo/bar\", @"C:\FOO\BAR"),
        new("C:/ふー/バー", @"C:\ふー\バー")
    ]);

    foreach (var (_, test) in tests) {
        @string path = default;
        if (test.arg != ""u8) {
            path = filepath.Clean(test.arg);
        }
        var (got, errΔ1) = filepath_internal_test_package.ToNorm(path, stubBase);
        if (errΔ1 != nil) {
            t.Errorf("toNorm(%s) failed: %v\n", test.arg, errΔ1);
        } else if (got != test.want) {
            t.Errorf("toNorm(%s) returns %s, but %s expected\n", test.arg, got, test.want);
        }
    }

    @string testPath = tmpTestFooBarˢ;

    var testsDir = slice<TestToNorm_testsDir>([
        // test absolute paths
        new(".", @"{{tmp}}\test\foo\bar", @"{{tmp}}\test\foo\bar"),
        new(".", @"{{tmp}}\.\test/foo\bar", @"{{tmp}}\test\foo\bar"),
        new(".", @"{{tmp}}\test\..\test\foo\bar", @"{{tmp}}\test\foo\bar"),
        new(".", @"{{tmp}}\TEST\FOO\BAR", @"{{tmp}}\test\foo\bar"),

        // test relative paths begin with drive letter
        new(@"{{tmp}}\test", @"{{tmpvol}}.", @"{{tmpvol}}."),
        new(@"{{tmp}}\test", @"{{tmpvol}}..", @"{{tmpvol}}.."),
        new(@"{{tmp}}\test", @"{{tmpvol}}foo\bar", @"{{tmpvol}}foo\bar"),
        new(@"{{tmp}}\test", @"{{tmpvol}}.\foo\bar", @"{{tmpvol}}foo\bar"),
        new(@"{{tmp}}\test", @"{{tmpvol}}foo\..\foo\bar", @"{{tmpvol}}foo\bar"),
        new(@"{{tmp}}\test", @"{{tmpvol}}FOO\BAR", @"{{tmpvol}}foo\bar"),

        // test relative paths begin with '\'
        new("{{tmp}}", @"{{tmpnovol}}\test\foo\bar", @"{{tmpnovol}}\test\foo\bar"),
        new("{{tmp}}", @"{{tmpnovol}}\.\test\foo\bar", @"{{tmpnovol}}\test\foo\bar"),
        new("{{tmp}}", @"{{tmpnovol}}\test\..\test\foo\bar", @"{{tmpnovol}}\test\foo\bar"),
        new("{{tmp}}", @"{{tmpnovol}}\TEST\FOO\BAR", @"{{tmpnovol}}\test\foo\bar"),

        // test relative paths begin without '\'
        new(@"{{tmp}}\test", ".", @"."),
        new(@"{{tmp}}\test", "..", @".."),
        new(@"{{tmp}}\test", @"foo\bar", @"foo\bar"),
        new(@"{{tmp}}\test", @".\foo\bar", @"foo\bar"),
        new(@"{{tmp}}\test", @"foo\..\foo\bar", @"foo\bar"),
        new(@"{{tmp}}\test", @"FOO\BAR", @"foo\bar"),

        // test UNC paths
        new(".", @"\\localhost\c$", @"\\localhost\c$")
    ]);

    @string ctmp = tempDirCanonical(t);
    {
        var errΔ2 = os.MkdirAll(strings.ReplaceAll(testPath, tmpˢ, ctmp), 511); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }

    var (cwd, err) = os.Getwd();
    if (err != nil) {
        t.Fatal(err);
    }
    t.Chdir("."); // Ensure cwd is restored after the test.

    @string tmpVol = filepath.VolumeName(ctmp);
    if (len(tmpVol) != 2) {
        t.Fatalf("unexpected temp volume name %q", tmpVol);
    }

    @string tmpNoVol = ctmp.slice(len(tmpVol));

    var replacer = strings.NewReplacer(tmpˢ, ctmp, tmpvolˢ, tmpVol, tmpnovolˢ, tmpNoVol);

    foreach (var (_, test) in testsDir) {
        @string wd = replacer.Replace(test.wd);
        @string arg = replacer.Replace(test.arg);
        @string want = replacer.Replace(test.want);

        if (test.wd == "."u8) {
            var errΔ3 = os.Chdir(cwd);
            if (errΔ3 != nil) {
                t.Error(errΔ3);

                continue;
            }
        } else {
            var errΔ4 = os.Chdir(wd);
            if (errΔ4 != nil) {
                t.Error(errΔ4);

                continue;
            }
        }
        if (arg != ""u8) {
            arg = filepath.Clean(arg);
        }
        var (got, errΔ5) = filepath_internal_test_package.ToNorm(arg, filepath_internal_test_package.NormBase);
        if (errΔ5 != nil) {
            t.Errorf("toNorm(%s) failed: %v (wd=%s)\n", arg, errΔ5, wd);
        } else if (got != want) {
            t.Errorf("toNorm(%s) returns %s, but %s expected (wd=%s)\n", arg, got, want, wd);
        }
    }
}

public static void TestUNC(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // Test that this doesn't go into an infinite recursion.
        // See golang.org/issue/15879.
        defer(debug.SetMaxStack, debug.SetMaxStack(1000000), ref ᒐ);
        filepath.Glob(@"\\?\c:\*");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cmdˢ => ref cmdˢᵇ;
private static readonly @string cmdˢᵇ = "cmd"u8;
internal static ref readonly @string mklinkˢ => ref mklinkˢᵇ;
private static readonly @string mklinkˢᵇ = "mklink"u8;

internal static void testWalkMklink(ж<testing.T> t, @string linktype) {
    var (output, _) = exec.Command(cmdˢ, "/c", mklinkˢ, "/?").Output();
    if (!strings.Contains((@string)output, fmt.Sprintf(" /%s "u8, linktype))) {
        t.Skipf(@"skipping test; mklink does not supports /%s parameter", linktype);
    }
    testWalkSymlink(t, error (@string target, @string link) => {
        var (outputΔ1, err) = exec.Command(cmdˢ, "/c", mklinkˢ, "/" + linktype, link, target).CombinedOutput();
        if (err != nil) {
            return fmt.Errorf(@"""mklink /%s %v %v"" command failed: %v\n%v"u8, linktype, link, target, err, (@string)outputΔ1);
        }
        return default;
    });
}

public static void TestWalkDirectoryJunction(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
    testWalkMklink(t, "J");
}

public static void TestWalkDirectorySymlink(ж<testing.T> t) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));
    testWalkMklink(t, "D");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string powershellˢ => ref powershellˢᵇ;
private static readonly @string powershellˢᵇ = "powershell"u8;
internal static ref readonly @string commandˢ => ref commandˢᵇ;
private static readonly @string commandˢᵇ = "-Command"u8;
internal static ref readonly @string testPs1ˢ => ref testPs1ˢᵇ;
private static readonly @string testPs1ˢᵇ = "test.ps1"u8;
internal static ref readonly @string fileˢ2 => ref fileˢ2ᵇ;
private static readonly @string fileˢ2ᵇ = "-File"u8;
internal static readonly object skippingTestBecauseˢ = (@string)"skipping test because failed to create VHD: "u8;

internal static slice<byte> createMountPartition(ж<testing.T> t, @string vhd, @string args) {
    testenv.MustHaveExecPath(new filepath_test_package.testing_TжTB(t), powershellˢ);
    t.Cleanup(() => {
        var cmdΔ1 = testenv.Command(new filepath_test_package.testing_TжTB(t), powershellˢ, commandˢ, fmt.Sprintf("Dismount-VHD %q"u8, vhd));
        var (@out, errΔ1) = cmdΔ1.CombinedOutput();
        if (errΔ1 != nil) {
            if (t.Skipped()) {
                // Probably failed to dismount because we never mounted it in
                // the first place. Log the error, but ignore it.
                t.Logf("%v: %v (skipped)\n%s", cmdΔ1.OrTypedNil(), errΔ1, @out);
            } else {
                // Something went wrong, and we don't want to leave dangling VHDs.
                // Better to fail the test than to just log the error and continue.
                t.Errorf("%v: %v\n%s", cmdΔ1.OrTypedNil(), errΔ1, @out);
            }
        }
    });

    @string script = filepath.Join(t.TempDir(), testPs1ˢ);
    @string cmd = strings.Join(slice<@string>([
        "$ErrorActionPreference = \"Stop\"",
        fmt.Sprintf("$vhd = New-VHD -Path %q -SizeBytes 3MB -Fixed"u8, vhd),
        "$vhd | Mount-VHD",
        fmt.Sprintf("$vhd = Get-VHD %q"u8, vhd),
        "$vhd | Get-Disk | Initialize-Disk -PartitionStyle GPT",
        "$part = $vhd | Get-Disk | New-Partition -UseMaximumSize -AssignDriveLetter:$false",
        "$vol = $part | Format-Volume -FileSystem NTFS",
        args
    ]), "\n");

    var err = os.WriteFile(script, slice<byte>(cmd), 438);
    if (err != nil) {
        t.Fatal(err);
    }
    (var output, err) = testenv.Command(new filepath_test_package.testing_TжTB(t), powershellˢ, fileˢ2, script).CombinedOutput();
    if (err != nil) {
        // This can happen if Hyper-V is not installed or enabled.
        t.Skip(skippingTestBecauseˢ, err, (@string)output);
    }
    return output;
}

internal static ж<godebug.Setting> winsymlink = godebug.New("winsymlink");

internal static ж<godebug.Setting> winreadlinkvolume = godebug.New("winreadlinkvolume");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestBecauseˢ2 = (@string)"skipping test because winsymlink is not enabled"u8;
internal static readonly object skippingTestBecauseˢ3 = (@string)"skipping test because mklink command does not support junctions"u8;
internal static ref readonly @string testVhdxˢ => ref testVhdxˢᵇ;
private static readonly @string testVhdxˢᵇ = "Test.vhdx"u8;
internal static ref readonly @string writeHostVolPathˢ => ref writeHostVolPathˢᵇ;
private static readonly @string writeHostVolPathˢᵇ = "Write-Host $vol.Path -NoNewline"u8;
internal static ref readonly @string dirlinkˢ => ref dirlinkˢᵇ;
private static readonly @string dirlinkˢᵇ = "dirlink"u8;

public static void TestEvalSymlinksJunctionToVolumeID(ж<testing.T> t) {
    // Test that EvalSymlinks resolves a directory junction which
    // is mapped to volumeID (instead of drive letter). See go.dev/issue/39786.
    if (winsymlink.Value() == "0"u8) {
        t.Skip(skippingTestBecauseˢ2);
    }
    t.Parallel();

    var (output, _) = exec.Command(cmdˢ, "/c", mklinkˢ, "/?").Output();
    if (!strings.Contains((@string)output, " /J ")) {
        t.Skip(skippingTestBecauseˢ3);
    }

    @string tmpdir = tempDirCanonical(t);
    @string vhd = filepath.Join(tmpdir, testVhdxˢ);
    output = createMountPartition(t, vhd, writeHostVolPathˢ);
    @string vol = (@string)output;

    @string dirlink = filepath.Join(tmpdir, dirlinkˢ);
    (output, var err) = testenv.Command(new filepath_test_package.testing_TжTB(t), cmdˢ, "/c", mklinkˢ, "/J", dirlink, vol).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to run mklink %v %v: %v %q", dirlink, vol, err, output);
    }
    (var got, err) = filepath.EvalSymlinks(dirlink);
    if (err != nil) {
        t.Fatal(err);
    }
    if (got != dirlink) {
        t.Errorf(@"EvalSymlinks(%q): got %q, want %q", dirlink, got, dirlink);
    }
}

public static void TestEvalSymlinksMountPointRecursion(ж<testing.T> t) {
    // Test that EvalSymlinks doesn't follow recursive mount points.
    // See go.dev/issue/40176.
    if (winsymlink.Value() == "0"u8) {
        t.Skip(skippingTestBecauseˢ2);
    }
    t.Parallel();

    @string tmpdir = tempDirCanonical(t);
    @string dirlink = filepath.Join(tmpdir, dirlinkˢ);
    var err = os.Mkdir(dirlink, 493);
    if (err != nil) {
        t.Fatal(err);
    }

    @string vhd = filepath.Join(tmpdir, testVhdxˢ);
    createMountPartition(t, vhd, fmt.Sprintf("$part | Add-PartitionAccessPath -AccessPath %q\n"u8, dirlink));

    (var got, err) = filepath.EvalSymlinks(dirlink);
    if (err != nil) {
        t.Fatal(err);
    }
    if (got != dirlink) {
        t.Errorf(@"EvalSymlinks(%q): got %q, want %q", dirlink, got, dirlink);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mountvolˢ => ref mountvolˢᵇ;
private static readonly @string mountvolˢᵇ = "mountvol"u8;
internal static ref readonly @string filelinkˢ => ref filelinkˢᵇ;
private static readonly @string filelinkˢᵇ = "filelink"u8;

public static void TestNTNamespaceSymlink(ж<testing.T> t) {
    var (output, _) = exec.Command(cmdˢ, "/c", mklinkˢ, "/?").Output();
    if (!strings.Contains((@string)output, " /J ")) {
        t.Skip(skippingTestBecauseˢ3);
    }

    @string tmpdir = tempDirCanonical(t);

    @string vol = filepath.VolumeName(tmpdir);
    (output, var err) = exec.Command(cmdˢ, "/c", mountvolˢ, vol, "/L").CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to run mountvol %v /L: %v %q", vol, err, output);
    }
    @string target = strings.Trim((@string)output, " \n\r");

    @string dirlink = filepath.Join(tmpdir, dirlinkˢ);
    (output, err) = exec.Command(cmdˢ, "/c", mklinkˢ, "/J", dirlink, target).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to run mklink %v %v: %v %q", dirlink, target, err, output);
    }

    (var got, err) = filepath.EvalSymlinks(dirlink);
    if (err != nil) {
        t.Fatal(err);
    }
    @string want = default;
    if (winsymlink.Value() == "0"u8) {
        if (winreadlinkvolume.Value() == "0"u8) {
            want = vol + @"\"u8;
        } else {
            want = target;
        }
    } else {
        want = dirlink;
    }
    if (got != want) {
        t.Errorf(@"EvalSymlinks(%q): got %q, want %q", dirlink, got, want);
    }

    // Make sure we have sufficient privilege to run mklink command.
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(t));

    @string @file = filepath.Join(tmpdir, fileˢ);
    err = os.WriteFile(@file, slice<byte>(""u8), 438);
    if (err != nil) {
        t.Fatal(err);
    }

    target = filepath.Join(target, @file.slice(len(filepath.VolumeName(@file))));

    @string filelink = filepath.Join(tmpdir, filelinkˢ);
    (output, err) = exec.Command(cmdˢ, "/c", mklinkˢ, filelink, target).CombinedOutput();
    if (err != nil) {
        t.Fatalf("failed to run mklink %v %v: %v %q", filelink, target, err, output);
    }

    (got, err) = filepath.EvalSymlinks(filelink);
    if (err != nil) {
        t.Fatal(err);
    }

    if (winreadlinkvolume.Value() == "0"u8) {
        want = @file;
    } else {
        want = target;
    }
    if (got != want) {
        t.Errorf(@"EvalSymlinks(%q): got %q, want %q", filelink, got, want);
    }
}

internal partial struct TestIssue52476_tests /*dyn*/ {
    internal @string lhs, rhs;
    internal @string want;
}

public static void TestIssue52476(ж<testing.T> t) {
    var tests = slice<TestIssue52476_tests>([
        new(@"..\.", @"C:", @"..\C:"),
        new(@"..", @"C:", @"..\C:"),
        new(@".", @":", @".\:"),
        new(@".", @"C:", @".\C:"),
        new(@".", @"C:/a/b/../c", @".\C:\a\c"),
        new(@".", @"\C:", @".\C:"),
        new(@"C:\", @".", @"C:\"),
        new(@"C:\", @"C:\", @"C:\C:"),
        new(@"C", @":", @"C\:"),
        new(@"\.", @"C:", @"\C:"),
        new(@"\", @"C:", @"\C:")
    ]);

    foreach (var (_, test) in tests) {
        @string got = filepath.Join(test.lhs, test.rhs);
        if (got != test.want) {
            t.Errorf(@"Join(%q, %q): got %q, want %q", test.lhs, test.rhs, got, test.want);
        }
    }
}

internal partial struct TestAbsWindows_type /*dyn*/ {
    internal @string path;
    internal @string want;
}

public static void TestAbsWindows(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestAbsWindows_type>([
        new(@"C:\foo", @"C:\foo"),
        new(@"\\host\share\foo", @"\\host\share\foo"),
        new(@"\\host", @"\\host"),
        new(@"\\.\NUL", @"\\.\NUL"),
        new(@"NUL", @"\\.\NUL"),
        new(@"COM1", @"\\.\COM1"),
        new(@"a/NUL", @"\\.\NUL")
    ])) {
        var (got, err) = filepath.Abs(test.path);
        if (err != nil || got != test.want) {
            t.Errorf("Abs(%q) = %q, %v; want %q, nil", test.path, got, err, test.want);
        }
    }
}

} // end filepath_test_package
