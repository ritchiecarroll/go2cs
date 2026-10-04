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
internal static readonly @string comSpecˢ = "ComSpec"u8;
internal static readonly object comSpecMustBeSetˢ = (@string)"%ComSpec% must be set"u8;

public static void TestWinSplitListTestsAreValid(ж<testing.T> Ꮡt) {
    @string comspec = os.Getenv(comSpecˢ);
    if (comspec == ""u8) {
        Ꮡt.Fatal(comSpecMustBeSetˢ);
    }
    foreach (var (ti, tt) in winsplitlisttests) {
        testWinSplitListTestIsValid(Ꮡt, ti, tt, comspec);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string systemRootˢ = "SystemRoot"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string cmdfileᶜ = @"printdir.cmd"u8;

internal static void testWinSplitListTestIsValid(ж<testing.T> Ꮡt, nint ti, SplitListTest tt, @string comspec) {
    @string cmdfile = cmdfileᶜ;
    fs.FileMode perm = /* 0700 */ 448;
    ref var tmp = ref heap<@string>(out var Ꮡtmp);
    tmp = Ꮡt.TempDir();
    foreach (var (i, d) in tt.result) {
        if (d == ""u8) {
            continue;
        }
        {
            @string cd = filepath.Clean(d); if (filepath.VolumeName(cd) != ""u8 || cd[0] == (rune)'\\' || cd == ".."u8 || (len(cd) >= 3 && cd[0..3] == @"..\")) {
                Ꮡt.Errorf("%d,%d: %#q refers outside working directory"u8, ti, i, d);
                return;
            }
        }
        @string dd = filepath.Join(tmp, d);
        {
            var (_, err) = os.Stat(dd); if (err == default!) {
                Ꮡt.Errorf("%d,%d: %#q already exists"u8, ti, i, d);
                return;
            }
        }
        {
            var err = os.MkdirAll(dd, perm); if (err != default!) {
                Ꮡt.Errorf("%d,%d: MkdirAll(%#q) failed: %v"u8, ti, i, dd, err);
                return;
            }
        }
        @string fn = filepath.Join(dd, cmdfile);
        var data = slice<byte>("@echo " + d + "\r\n");
        {
            var err = os.WriteFile(fn, data, perm); if (err != default!) {
                Ꮡt.Errorf("%d,%d: WriteFile(%#q) failed: %v"u8, ti, i, fn, err);
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
            Args: new @string[]{@"/c"u8, cmdfile}.slice(),
            Env: new @string[]{@"Path="u8 + systemRoot + "/System32;"u8 + tt.list, @"SystemRoot="u8 + systemRoot}.slice(),
            Dir: tmp
        ));
        var (@out, err) = cmd.CombinedOutput();
        switch (ᐧ) {
        case {} when err != default!: {
            Ꮡt.Errorf("%d,%d: execution error %v\n%q"u8, ti, i, err, @out);
            return;
        }
        case {} when !slices.Equal<slice<byte>, byte>(@out, exp): {
            Ꮡt.Errorf("%d,%d: expected %#q, got %#q"u8, ti, i, exp, @out);
            return;
        }
        default: {
            err = os.Remove(filepath.Join(tmp, // unshadow cmdfile in next directory
 d, cmdfile));
            if (err != default!) {
                Ꮡt.Fatalf("Remove test command failed: %v"u8, err);
            }
            break;
        }}

    }
}

public static void TestWindowsEvalSymlinks(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string tmpDir = tempDirCanonical(Ꮡt);
    if (len(tmpDir) < 3) {
        Ꮡt.Fatalf("tmpDir path %q is too short"u8, tmpDir);
    }
    if (tmpDir[1] != (rune)':') {
        Ꮡt.Fatalf("tmpDir path %q must have drive letter in it"u8, tmpDir);
    }
    var test = new EvalSymlinksTest("test/linkabswin"u8, tmpDir[..3]);
    // Create the symlink farm using relative paths.
    var testdirs = append(EvalSymlinksTestDirs, test);
    foreach (var (_, d) in testdirs) {
        error err = default!;
        @string pathΔ1 = simpleJoin(tmpDir, d.path);
        if (d.dest == ""u8){
            err = os.Mkdir(pathΔ1, 493);
        } else {
            err = os.Symlink(d.dest, pathΔ1);
        }
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string path = simpleJoin(tmpDir, test.path);
    testEvalSymlinks(Ꮡt, path, test.dest);
    testEvalSymlinksAfterChdir(Ꮡt, path, "."u8, test.dest);
    testEvalSymlinksAfterChdir(Ꮡt,
        path,
        filepath.VolumeName(tmpDir) + "."u8,
        test.dest);
    testEvalSymlinksAfterChdir(Ꮡt,
        simpleJoin(tmpDir, testˢ),
        simpleJoin(".."u8, test.path),
        test.dest);
    testEvalSymlinksAfterChdir(Ꮡt, tmpDir, test.path, test.dest);
}

// TestEvalSymlinksCanonicalNames verify that EvalSymlinks
// returns "canonical" path names on windows.
public static void TestEvalSymlinksCanonicalNames(ж<testing.T> Ꮡt) {
    @string ctmp = tempDirCanonical(Ꮡt);
    var dirs = new @string[]{
        "test"u8,
        "test/dir"u8,
        "testing_long_dir"u8,
        "TEST2"u8
    }.slice();
    foreach (var (_, d) in dirs) {
        @string dir = filepath.Join(ctmp, d);
        var err = os.Mkdir(dir, 493);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var cname, err) = filepath.EvalSymlinks(dir);
        if (err != default!) {
            Ꮡt.Errorf("EvalSymlinks(%q) error: %v"u8, dir, err);
            continue;
        }
        if (dir != cname) {
            Ꮡt.Errorf("EvalSymlinks(%q) returns %q, but should return %q"u8, dir, cname, dir);
            continue;
        }
        // test non-canonical names
        @string test = strings.ToUpper(dir);
        (var p, err) = filepath.EvalSymlinks(test);
        if (err != default!) {
            Ꮡt.Errorf("EvalSymlinks(%q) error: %v"u8, test, err);
            continue;
        }
        if (p != cname) {
            Ꮡt.Errorf("EvalSymlinks(%q) returns %q, but should return %q"u8, test, p, cname);
            continue;
        }
        // another test
        test = strings.ToLower(dir);
        (p, err) = filepath.EvalSymlinks(test);
        if (err != default!) {
            Ꮡt.Errorf("EvalSymlinks(%q) error: %v"u8, test, err);
            continue;
        }
        if (p != cname) {
            Ꮡt.Errorf("EvalSymlinks(%q) returns %q, but should return %q"u8, test, p, cname);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fsutilˢ = "fsutil"u8;
internal static readonly @string queryˢ = "query"u8;
internal static readonly @string theRegistryStateOfˢ = "The registry state of NtfsDisable8dot3NameCreation is 2, the default (Volume level setting)"u8;
internal static readonly @string theRegistryStateIs2Perˢ = "The registry state is: 2 (Per volume setting - the default)"u8;
internal static readonly @string basedOnTheAboveTwoˢ = "Based on the above two settings, 8dot3 name creation is %s on %s"u8;
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
    var (@out, _) = exec.Command(fsutilˢ, "8dot3name"u8, queryˢ, vol).CombinedOutput();
    // Check that system has "Volume level setting" set.
    @string expected = theRegistryStateOfˢ;
    if (!strings.Contains(((@string)@out), expected)) {
        // Windows 10 version of fsutil has different output message.
        @string expectedWindow10 = theRegistryStateIs2Perˢ;
        if (!strings.Contains(((@string)@out), expectedWindow10)) {
            return fmt.Errorf("fsutil output should contain %q, but is %q"u8, expected, ((@string)@out));
        }
    }
    // Now check the volume setting.
    expected = basedOnTheAboveTwoˢ;
    if (enabled){
        expected = fmt.Sprintf(expected, enabledˢ, vol);
    } else {
        expected = fmt.Sprintf(expected, disabledˢ, vol);
    }
    if (!strings.Contains(((@string)@out), expected)) {
        return fmt.Errorf("unexpected fsutil output: %q"u8, ((@string)@out));
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string successfullyS8dot3nameˢ = "Successfully %s 8dot3name generation on %s\r\n"u8;

internal static error setVolume8dot3Setting(@string vol, bool enabled) {
    var cmd = new @string[]{"fsutil"u8, "8dot3name"u8, "set"u8, vol}.slice();
    if (enabled){
        cmd = append(cmd, "0"u8);
    } else {
        cmd = append(cmd, "1"u8);
    }
    // It appears, on some systems "fsutil 8dot3name set ..." command always
    // exits with error. Ignore exit code, and look at fsutil output instead.
    var (@out, _) = exec.Command(cmd[0], cmd[1..].ꓸꓸꓸ).CombinedOutput();
    sstring @outᴛ1 = ((sstring)@out);
    if (@outᴛ1 != "\r\nSuccessfully set 8dot3name behavior.\r\n"u8) {
        // Windows 10 version of fsutil has different output message.
        @string expectedWindow10 = successfullyS8dot3nameˢ;
        if (enabled){
            expectedWindow10 = fmt.Sprintf(expectedWindow10, enabledˢ, vol);
        } else {
            expectedWindow10 = fmt.Sprintf(expectedWindow10, disabledˢ, vol);
        }
        if (@outᴛ1 != expectedWindow10) {
            return fmt.Errorf("%v command failed: %q"u8, cmd, ((@string)@out));
        }
    }
    return default!;
}

internal static ж<bool> runFSModifyTests = flag.Bool("run_fs_modify_tests"u8, false, "run tests which modify filesystem parameters"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestThatModifiesˢ = (@string)"skipping test that modifies file system setting; enable with -run_fs_modify_tests"u8;

// This test assumes registry state of NtfsDisable8dot3NameCreation is 2,
// the default (Volume level setting).
public static void TestEvalSymlinksCanonicalNamesWith8dot3Disabled(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (!runFSModifyTests.Value) {
            Ꮡt.Skip(skippingTestThatModifiesˢ);
        }
        @string tempVol = filepath.VolumeName(os.TempDir());
        if (len(tempVol) != 2) {
            Ꮡt.Fatalf("unexpected temp volume name %q"u8, tempVol);
        }
        var err = checkVolume8dot3Setting(tempVol, true);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        err = setVolume8dot3Setting(tempVol, false);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(() => {
            var errΔ1 = setVolume8dot3Setting(tempVol, true);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            errΔ1 = checkVolume8dot3Setting(tempVol, true);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }, ref ᒐ);
        err = checkVolume8dot3Setting(tempVol, false);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        TestEvalSymlinksCanonicalNames(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tmpTestFooBarˢ = @"{{tmp}}\test\foo\bar"u8;
internal static readonly @string tmpˢ = "{{tmp}}"u8;
internal static readonly @string tmpvolˢ = "{{tmpvol}}"u8;
internal static readonly @string tmpnovolˢ = "{{tmpnovol}}"u8;

[GoType("dyn")] internal partial struct TestToNorm_tests {
    internal @string arg;
    internal @string want;
}

[GoType("dyn")] internal partial struct TestToNorm_testsDir {
    internal @string wd;
    internal @string arg;
    internal @string want;
}

public static void TestToNorm(ж<testing.T> Ꮡt) {
    var stubBase = (@string, error) (@string path) => {
        @string vol = filepath.VolumeName(path);
        path = path.slice(len(vol));
        if (strings.Contains(path, "/"u8)) {
            return ("", fmt.Errorf("invalid path is given to base: %s"u8, vol + path));
        }
        if (path == ""u8 || path == "."u8 || path == @"\"u8) {
            return ("", fmt.Errorf("invalid path is given to base: %s"u8, vol + path));
        }
        nint i = strings.LastIndexByte(path, filepath.Separator);
        if (i == len(path) - 1) {
            // trailing '\' is invalid
            return ("", fmt.Errorf("invalid path is given to base: %s"u8, vol + path));
        }
        if (i == -1) {
            return (strings.ToUpper(path), default!);
        }
        return (strings.ToUpper(path.slice(i + 1)), default!);
    };
    // On this test, toNorm should be same as string.ToUpper(filepath.Clean(path)) except empty string.
    var tests = new TestToNorm_tests[]{
        new(""u8, ""u8),
        new("."u8, "."u8),
        new("./foo/bar"u8, @"FOO\BAR"u8),
        new("/"u8, @"\"u8),
        new("/foo/bar"u8, @"\FOO\BAR"u8),
        new("/foo/bar/baz/qux"u8, @"\FOO\BAR\BAZ\QUX"u8),
        new("foo/bar"u8, @"FOO\BAR"u8),
        new("C:/foo/bar"u8, @"C:\FOO\BAR"u8),
        new("C:foo/bar"u8, @"C:FOO\BAR"u8),
        new("c:/foo/bar"u8, @"C:\FOO\BAR"u8),
        new("C:/foo/bar"u8, @"C:\FOO\BAR"u8),
        new("C:/foo/bar/"u8, @"C:\FOO\BAR"u8),
        new(@"C:\foo\bar"u8, @"C:\FOO\BAR"u8),
        new(@"C:\foo/bar\"u8, @"C:\FOO\BAR"u8),
        new("C:/ふー/バー"u8, @"C:\ふー\バー"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string path = default!;
        if (test.arg != ""u8) {
            path = filepath.Clean(test.arg);
        }
        var (got, errΔ1) = filepath_internal_test_package.ToNorm(path, stubBase);
        if (errΔ1 != default!){
            Ꮡt.Errorf("toNorm(%s) failed: %v\n"u8, test.arg, errΔ1);
        } else 
        if (got != test.want) {
            Ꮡt.Errorf("toNorm(%s) returns %s, but %s expected\n"u8, test.arg, got, test.want);
        }
    }
    @string testPath = tmpTestFooBarˢ;
    var testsDir = new TestToNorm_testsDir[]{ // test absolute paths

        new("."u8, @"{{tmp}}\test\foo\bar"u8, @"{{tmp}}\test\foo\bar"u8),
        new("."u8, @"{{tmp}}\.\test/foo\bar"u8, @"{{tmp}}\test\foo\bar"u8),
        new("."u8, @"{{tmp}}\test\..\test\foo\bar"u8, @"{{tmp}}\test\foo\bar"u8),
        new("."u8, @"{{tmp}}\TEST\FOO\BAR"u8, @"{{tmp}}\test\foo\bar"u8), // test relative paths begin with drive letter

        new(@"{{tmp}}\test"u8, @"{{tmpvol}}."u8, @"{{tmpvol}}."u8),
        new(@"{{tmp}}\test"u8, @"{{tmpvol}}.."u8, @"{{tmpvol}}.."u8),
        new(@"{{tmp}}\test"u8, @"{{tmpvol}}foo\bar"u8, @"{{tmpvol}}foo\bar"u8),
        new(@"{{tmp}}\test"u8, @"{{tmpvol}}.\foo\bar"u8, @"{{tmpvol}}foo\bar"u8),
        new(@"{{tmp}}\test"u8, @"{{tmpvol}}foo\..\foo\bar"u8, @"{{tmpvol}}foo\bar"u8),
        new(@"{{tmp}}\test"u8, @"{{tmpvol}}FOO\BAR"u8, @"{{tmpvol}}foo\bar"u8), // test relative paths begin with '\'

        new("{{tmp}}"u8, @"{{tmpnovol}}\test\foo\bar"u8, @"{{tmpnovol}}\test\foo\bar"u8),
        new("{{tmp}}"u8, @"{{tmpnovol}}\.\test\foo\bar"u8, @"{{tmpnovol}}\test\foo\bar"u8),
        new("{{tmp}}"u8, @"{{tmpnovol}}\test\..\test\foo\bar"u8, @"{{tmpnovol}}\test\foo\bar"u8),
        new("{{tmp}}"u8, @"{{tmpnovol}}\TEST\FOO\BAR"u8, @"{{tmpnovol}}\test\foo\bar"u8), // test relative paths begin without '\'

        new(@"{{tmp}}\test"u8, "."u8, @"."u8),
        new(@"{{tmp}}\test"u8, ".."u8, @".."u8),
        new(@"{{tmp}}\test"u8, @"foo\bar"u8, @"foo\bar"u8),
        new(@"{{tmp}}\test"u8, @".\foo\bar"u8, @"foo\bar"u8),
        new(@"{{tmp}}\test"u8, @"foo\..\foo\bar"u8, @"foo\bar"u8),
        new(@"{{tmp}}\test"u8, @"FOO\BAR"u8, @"foo\bar"u8), // test UNC paths

        new("."u8, @"\\localhost\c$"u8, @"\\localhost\c$"u8)
    }.slice();
    @string ctmp = tempDirCanonical(Ꮡt);
    {
        var errΔ2 = os.MkdirAll(strings.ReplaceAll(testPath, tmpˢ, ctmp), 511); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    var (cwd, err) = os.Getwd();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Chdir("."u8); // Ensure cwd is restored after the test.
    @string tmpVol = filepath.VolumeName(ctmp);
    if (len(tmpVol) != 2) {
        Ꮡt.Fatalf("unexpected temp volume name %q"u8, tmpVol);
    }
    @string tmpNoVol = ctmp.slice(len(tmpVol));
    var replacer = strings.NewReplacer(tmpˢ, ctmp, tmpvolˢ, tmpVol, tmpnovolˢ, tmpNoVol);
    foreach (var (_, test) in testsDir) {
        @string wd = replacer.Replace(test.wd);
        @string arg = replacer.Replace(test.arg);
        @string want = replacer.Replace(test.want);
        if (test.wd == "."u8){
            var errΔ3 = os.Chdir(cwd);
            if (errΔ3 != default!) {
                Ꮡt.Error(errΔ3);
                continue;
            }
        } else {
            var errΔ4 = os.Chdir(wd);
            if (errΔ4 != default!) {
                Ꮡt.Error(errΔ4);
                continue;
            }
        }
        if (arg != ""u8) {
            arg = filepath.Clean(arg);
        }
        var (got, errΔ5) = filepath_internal_test_package.ToNorm(arg, filepath_internal_test_package.NormBase);
        if (errΔ5 != default!){
            Ꮡt.Errorf("toNorm(%s) failed: %v (wd=%s)\n"u8, arg, errΔ5, wd);
        } else 
        if (got != want) {
            Ꮡt.Errorf("toNorm(%s) returns %s, but %s expected (wd=%s)\n"u8, arg, got, want, wd);
        }
    }
}

public static void TestUNC(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Test that this doesn't go into an infinite recursion.
        // See golang.org/issue/15879.
        defer(debug.SetMaxStack, debug.SetMaxStack(1000000), ref ᒐ);
        filepath.Glob(@"\\?\c:\*"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmdˢ = "cmd"u8;
internal static readonly @string mklinkˢ = "mklink"u8;

internal static void testWalkMklink(ж<testing.T> Ꮡt, @string linktype) {
    var (output, _) = exec.Command(cmdˢ, "/c"u8, mklinkˢ, "/?").Output();
    if (!strings.Contains(((@string)output), fmt.Sprintf(" /%s "u8, linktype))) {
        Ꮡt.Skipf(@"skipping test; mklink does not supports /%s parameter"u8, linktype);
    }
    testWalkSymlink(Ꮡt, error (@string target, @string link) => {
        var (outputΔ1, err) = exec.Command(cmdˢ, "/c"u8, mklinkˢ, "/" + linktype, link, target).CombinedOutput();
        if (err != default!) {
            return fmt.Errorf(@"""mklink /%s %v %v"" command failed: %v\n%v"u8, linktype, link, target, err, ((@string)outputΔ1));
        }
        return default!;
    });
}

public static void TestWalkDirectoryJunction(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    testWalkMklink(Ꮡt, "J"u8);
}

public static void TestWalkDirectorySymlink(ж<testing.T> Ꮡt) {
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    testWalkMklink(Ꮡt, "D"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string powershellˢ = "powershell"u8;
internal static readonly @string commandˢ = "-Command"u8;
internal static readonly @string testPs1ˢ = "test.ps1"u8;
internal static readonly @string fileˢ2 = "-File"u8;
internal static readonly object skippingTestBecauseˢ = (@string)"skipping test because failed to create VHD: "u8;

internal static slice<byte> createMountPartition(ж<testing.T> Ꮡt, @string vhd, @string args) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExecPath(new filepath_test_package.testing_TжTB(Ꮡt), powershellˢ);
    Ꮡt.Cleanup(() => {
        var cmdΔ1 = testenv.Command(new filepath_test_package.testing_TжTB(Ꮡt), powershellˢ, commandˢ, fmt.Sprintf("Dismount-VHD %q"u8, vhd));
        var (@out, errΔ1) = cmdΔ1.CombinedOutput();
        if (errΔ1 != default!) {
            if (Ꮡt.Skipped()){
                // Probably failed to dismount because we never mounted it in
                // the first place. Log the error, but ignore it.
                Ꮡt.Logf("%v: %v (skipped)\n%s"u8, cmdΔ1.OrTypedNil(), errΔ1, @out);
            } else {
                // Something went wrong, and we don't want to leave dangling VHDs.
                // Better to fail the test than to just log the error and continue.
                Ꮡt.Errorf("%v: %v\n%s"u8, cmdΔ1.OrTypedNil(), errΔ1, @out);
            }
        }
    });
    @string script = filepath.Join(Ꮡt.TempDir(), testPs1ˢ);
    @string cmd = strings.Join(new @string[]{
        "$ErrorActionPreference = \"Stop\""u8,
        fmt.Sprintf("$vhd = New-VHD -Path %q -SizeBytes 3MB -Fixed"u8, vhd),
        "$vhd | Mount-VHD"u8,
        fmt.Sprintf("$vhd = Get-VHD %q"u8, vhd),
        "$vhd | Get-Disk | Initialize-Disk -PartitionStyle GPT"u8,
        "$part = $vhd | Get-Disk | New-Partition -UseMaximumSize -AssignDriveLetter:$false"u8,
        "$vol = $part | Format-Volume -FileSystem NTFS"u8,
        args
    }.slice(), "\n"u8);
    var err = os.WriteFile(script, slice<byte>(cmd), 438);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var output, err) = testenv.Command(new filepath_test_package.testing_TжTB(Ꮡt), powershellˢ, fileˢ2, script).CombinedOutput();
    if (err != default!) {
        // This can happen if Hyper-V is not installed or enabled.
        Ꮡt.Skip(skippingTestBecauseˢ, err, ((@string)output));
    }
    return output;
}

internal static ж<godebug.Setting> winsymlink = godebug.New("winsymlink"u8);

internal static ж<godebug.Setting> winreadlinkvolume = godebug.New("winreadlinkvolume"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestBecauseˢ2 = (@string)"skipping test because winsymlink is not enabled"u8;
internal static readonly object skippingTestBecauseˢ3 = (@string)"skipping test because mklink command does not support junctions"u8;
internal static readonly @string testVhdxˢ = "Test.vhdx"u8;
internal static readonly @string writeHostVolPathˢ = "Write-Host $vol.Path -NoNewline"u8;
internal static readonly @string dirlinkˢ = "dirlink"u8;

public static void TestEvalSymlinksJunctionToVolumeID(ж<testing.T> Ꮡt) {
    // Test that EvalSymlinks resolves a directory junction which
    // is mapped to volumeID (instead of drive letter). See go.dev/issue/39786.
    if (winsymlink.Value() == "0"u8) {
        Ꮡt.Skip(skippingTestBecauseˢ2);
    }
    Ꮡt.Parallel();
    var (output, _) = exec.Command(cmdˢ, "/c"u8, mklinkˢ, "/?").Output();
    if (!strings.Contains(((@string)output), " /J "u8)) {
        Ꮡt.Skip(skippingTestBecauseˢ3);
    }
    @string tmpdir = tempDirCanonical(Ꮡt);
    @string vhd = filepath.Join(tmpdir, testVhdxˢ);
    output = createMountPartition(Ꮡt, vhd, writeHostVolPathˢ);
    @string vol = ((@string)output);
    @string dirlink = filepath.Join(tmpdir, dirlinkˢ);
    (output, var err) = testenv.Command(new filepath_test_package.testing_TжTB(Ꮡt), cmdˢ, "/c"u8, mklinkˢ, "/J", dirlink, vol).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to run mklink %v %v: %v %q"u8, dirlink, vol, err, output);
    }
    (var got, err) = filepath.EvalSymlinks(dirlink);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (got != dirlink) {
        Ꮡt.Errorf(@"EvalSymlinks(%q): got %q, want %q"u8, dirlink, got, dirlink);
    }
}

public static void TestEvalSymlinksMountPointRecursion(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that EvalSymlinks doesn't follow recursive mount points.
    // See go.dev/issue/40176.
    if (winsymlink.Value() == "0"u8) {
        Ꮡt.Skip(skippingTestBecauseˢ2);
    }
    Ꮡt.Parallel();
    @string tmpdir = tempDirCanonical(Ꮡt);
    @string dirlink = filepath.Join(tmpdir, dirlinkˢ);
    var err = os.Mkdir(dirlink, 493);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string vhd = filepath.Join(tmpdir, testVhdxˢ);
    createMountPartition(Ꮡt, vhd, fmt.Sprintf("$part | Add-PartitionAccessPath -AccessPath %q\n"u8, dirlink));
    (var got, err) = filepath.EvalSymlinks(dirlink);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (got != dirlink) {
        Ꮡt.Errorf(@"EvalSymlinks(%q): got %q, want %q"u8, dirlink, got, dirlink);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mountvolˢ = "mountvol"u8;
internal static readonly @string filelinkˢ = "filelink"u8;

public static void TestNTNamespaceSymlink(ж<testing.T> Ꮡt) {
    var (output, _) = exec.Command(cmdˢ, "/c"u8, mklinkˢ, "/?").Output();
    if (!strings.Contains(((@string)output), " /J "u8)) {
        Ꮡt.Skip(skippingTestBecauseˢ3);
    }
    @string tmpdir = tempDirCanonical(Ꮡt);
    @string vol = filepath.VolumeName(tmpdir);
    (output, var err) = exec.Command(cmdˢ, "/c"u8, mountvolˢ, vol, "/L").CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to run mountvol %v /L: %v %q"u8, vol, err, output);
    }
    @string target = strings.Trim(((@string)output), " \n\r"u8);
    @string dirlink = filepath.Join(tmpdir, dirlinkˢ);
    (output, err) = exec.Command(cmdˢ, "/c"u8, mklinkˢ, "/J", dirlink, target).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to run mklink %v %v: %v %q"u8, dirlink, target, err, output);
    }
    (var got, err) = filepath.EvalSymlinks(dirlink);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string want = default!;
    if (winsymlink.Value() == "0"u8){
        if (winreadlinkvolume.Value() == "0"u8){
            want = vol + @"\"u8;
        } else {
            want = target;
        }
    } else {
        want = dirlink;
    }
    if (got != want) {
        Ꮡt.Errorf(@"EvalSymlinks(%q): got %q, want %q"u8, dirlink, got, want);
    }
    // Make sure we have sufficient privilege to run mklink command.
    testenv.MustHaveSymlink(new filepath_test_package.testing_TжTB(Ꮡt));
    @string @file = filepath.Join(tmpdir, fileˢ);
    err = os.WriteFile(@file, slice<byte>(""u8), 438);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    target = filepath.Join(target, @file.slice(len(filepath.VolumeName(@file))));
    @string filelink = filepath.Join(tmpdir, filelinkˢ);
    (output, err) = exec.Command(cmdˢ, "/c"u8, mklinkˢ, filelink, target).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("failed to run mklink %v %v: %v %q"u8, filelink, target, err, output);
    }
    (got, err) = filepath.EvalSymlinks(filelink);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (winreadlinkvolume.Value() == "0"u8){
        want = @file;
    } else {
        want = target;
    }
    if (got != want) {
        Ꮡt.Errorf(@"EvalSymlinks(%q): got %q, want %q"u8, filelink, got, want);
    }
}

[GoType("dyn")] internal partial struct TestIssue52476_tests {
    internal @string lhs, rhs;
    internal @string want;
}

public static void TestIssue52476(ж<testing.T> Ꮡt) {
    var tests = new TestIssue52476_tests[]{
        new(@"..\."u8, @"C:"u8, @"..\C:"u8),
        new(@".."u8, @"C:"u8, @"..\C:"u8),
        new(@"."u8, @":"u8, @".\:"u8),
        new(@"."u8, @"C:"u8, @".\C:"u8),
        new(@"."u8, @"C:/a/b/../c"u8, @".\C:\a\c"u8),
        new(@"."u8, @"\C:"u8, @".\C:"u8),
        new(@"C:\"u8, @"."u8, @"C:\"u8),
        new(@"C:\"u8, @"C:\"u8, @"C:\C:"u8),
        new(@"C"u8, @":"u8, @"C\:"u8),
        new(@"\."u8, @"C:"u8, @"\C:"u8),
        new(@"\"u8, @"C:"u8, @"\C:"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string got = filepath.Join(test.lhs, test.rhs);
        if (got != test.want) {
            Ꮡt.Errorf(@"Join(%q, %q): got %q, want %q"u8, test.lhs, test.rhs, got, test.want);
        }
    }
}

[GoType("dyn")] internal partial struct TestAbsWindows_type {
    internal @string path;
    internal @string want;
}

public static void TestAbsWindows(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestAbsWindows_type[]{
        new(@"C:\foo"u8, @"C:\foo"u8),
        new(@"\\host\share\foo"u8, @"\\host\share\foo"u8),
        new(@"\\host"u8, @"\\host"u8),
        new(@"\\.\NUL"u8, @"\\.\NUL"u8),
        new(@"NUL"u8, @"\\.\NUL"u8),
        new(@"COM1"u8, @"\\.\COM1"u8),
        new(@"a/NUL"u8, @"\\.\NUL"u8)
    }.slice()) {
        var (got, err) = filepath.Abs(test.path);
        if (err != default! || got != test.want) {
            Ꮡt.Errorf("Abs(%q) = %q, %v; want %q, nil"u8, test.path, got, err, test.want);
        }
    }
}

} // end filepath_test_package
