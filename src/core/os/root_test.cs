// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using fs = go.io.fs_package;
using Δnet = net_package;
using Δos = os_package;
using Δpath = path_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using Δtesting = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.io;
using go.path;
using static go.os_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class os_test_package {

// testMaybeRooted calls f in two subtests,
// one with a Root and one with a nil r.
internal static void testMaybeRooted(ж<Δtesting.T> Ꮡt, Action<ж<Δtesting.T>, ж<Δos.Root>> f) {
    Ꮡt.Run(noRootˢ, (ж<Δtesting.T> tΔ1) => {
        tΔ1.Chdir(tΔ1.TempDir());
        f(tΔ1, nil);
    });
    Ꮡt.Run(inRootˢ, (ж<Δtesting.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            tΔ2.Chdir(tΔ2.TempDir());
            var (r, err) = Δos.OpenRoot("."u8);
            if (err != default!) {
                tΔ2.Fatal(err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
            f(tΔ2, r);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootˢ = "ROOT"u8;
internal static readonly @string absˢ = "$ABS"u8;

// makefs creates a test filesystem layout and returns the path to its root.
//
// Each entry in the slice is a file, directory, or symbolic link to create:
//
//   - "d/": directory d
//   - "f": file f with contents f
//   - "a => b": symlink a with target b
//
// The directory containing the filesystem is always named ROOT.
// $ABS is replaced with the absolute path of the directory containing the filesystem.
//
// Parent directories are automatically created as needed.
//
// makefs calls t.Skip if the layout contains features not supported by the current GOOS.
internal static @string makefs(ж<Δtesting.T> Ꮡt, slice<@string> fs) {
    @string root = Δpath.Join(Ꮡt.TempDir(), rootˢ);
    {
        var err = Δos.Mkdir(root, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    foreach (var (_, vᴛ1) in fs) {
        var ent = vᴛ1;

        ent = strings.ReplaceAll(ent, absˢ, root);
        var (@base, link, isLink) = strings.Cut(ent, " => "u8);
        if (isLink) {
            if (Δruntime.GOOS == "wasip1"u8 && Δpath.IsAbs(link)) {
                Ꮡt.Skip("absolute link targets not supported on " + Δruntime.GOOS);
            }
            if (Δruntime.GOOS == "plan9"u8) {
                Ꮡt.Skip("symlinks not supported on " + Δruntime.GOOS);
            }
            ent = @base;
        }
        {
            var err = Δos.MkdirAll(Δpath.Join(root, Δpath.Dir(@base)), 511); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        if (isLink){
            {
                var err = Δos.Symlink(link, Δpath.Join(root, @base)); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        } else 
        if (strings.HasSuffix(ent, "/"u8)){
            {
                var err = Δos.MkdirAll(Δpath.Join(root, ent), 511); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        } else {
            {
                var err = Δos.WriteFile(Δpath.Join(root, ent), slice<byte>(ent), 438); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        }
    }
    return root;
}

// A rootTest is a test case for os.Root.
partial struct rootTest {
    internal @string name;
    // fs is the test filesystem layout. See makefs above.
    internal slice<@string> fs;
    // open is the filename to access in the test.
    internal @string open;
    // target is the filename that we expect to be accessed, after resolving all symlinks.
    // For test cases where the operation fails due to an escaping path such as ../ROOT/x,
    // the target is the filename that should not have been opened.
    internal @string target;
    // ltarget is the filename that we expect to accessed, after resolving all symlinks
    // except the last one. This is the file we expect to be removed by Remove or statted
    // by Lstat.
    //
    // If the last path component in open is not a symlink, ltarget should be "".
    internal @string ltarget;
    // wantError is true if accessing the file should fail.
    internal bool wantError;
    // alwaysFails is true if the open operation is expected to fail
    // even when using non-openat operations.
    //
    // This lets us check that tests that are expected to fail because (for example)
    // a path escapes the directory root will succeed when the escaping checks are not
    // performed.
    internal bool alwaysFails;
}

// run sets up the test filesystem layout, os.OpenDirs the root, and calls f.
internal static void run(this ж<rootTest> Ꮡtest, ж<Δtesting.T> Ꮡt, Action<ж<Δtesting.T>, @string, ж<Δos.Root>> f) {
    ref var test = ref Ꮡtest.DerefOrNull();

    Ꮡt.Run(test.name, (ж<Δtesting.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            @string root = makefs(tΔ1, Ꮡtest.Value.fs);
            var (d, err) = Δos.OpenRoot(root);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var dʗ1 = d;
            defer(() => dʗ1.Close(), ref ᒐ);
            // The target is a file that will be accessed,
            // or a file that should not be accessed
            // (because doing so escapes the root).
            @string target = Ꮡtest.Value.target;
            if (Ꮡtest.Value.target != ""u8) {
                target = filepath.Join(root, Ꮡtest.Value.target);
            }
            f(tΔ1, target, d);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// errEndsTest checks the error result of a test,
// verifying that it succeeded or failed as expected.
//
// It returns true if the test is done due to encountering an expected error.
// false if the test should continue.
internal static bool errEndsTest(ж<Δtesting.T> Ꮡt, error err, bool wantError, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Helper();
    if (wantError){
        if (err == default!) {
            @string op = fmt.Sprintf(format, args.ꓸꓸꓸ);
            Ꮡt.Fatalf("%v = nil; want error"u8, op);
        }
        return true;
    } else {
        if (err != default!) {
            @string op = fmt.Sprintf(format, args.ꓸꓸꓸ);
            Ꮡt.Fatalf("%v = %v; want success"u8, op, err);
        }
        return false;
    }
}

// On Windows, the path is cleaned before symlink resolution.
internal static slice<rootTest> rootTestCases = new rootTest[]{new(
    name: "plain path"u8,
    fs: new @string[]{}.slice(),
    open: "target"u8,
    target: "target"u8
), new(
    name: "path in directory"u8,
    fs: new @string[]{
        "a/b/c/"u8
    }.slice(),
    open: "a/b/c/target"u8,
    target: "a/b/c/target"u8
), new(
    name: "symlink"u8,
    fs: new @string[]{
        "link => target"u8
    }.slice(),
    open: "link"u8,
    target: "target"u8,
    ltarget: "link"u8
), new(
    name: "symlink dotdot slash"u8,
    fs: new @string[]{
        "link => ../"u8
    }.slice(),
    open: "link"u8,
    ltarget: "link"u8,
    wantError: true
), new(
    name: "symlink ending in slash"u8,
    fs: new @string[]{
        "dir/"u8,
        "link => dir/"u8
    }.slice(),
    open: "link/target"u8,
    target: "dir/target"u8
), new(
    name: "symlink dotdot dotdot slash"u8,
    fs: new @string[]{
        "dir/link => ../../"u8
    }.slice(),
    open: "dir/link"u8,
    ltarget: "dir/link"u8,
    wantError: true
), new(
    name: "symlink chain"u8,
    fs: new @string[]{
        "link => a/b/c/target"u8,
        "a/b => e"u8,
        "a/e => ../f"u8,
        "f => g/h/i"u8,
        "g/h/i => .."u8,
        "g/c/"u8
    }.slice(),
    open: "link"u8,
    target: "g/c/target"u8,
    ltarget: "link"u8
), new(
    name: "path with dot"u8,
    fs: new @string[]{
        "a/b/"u8
    }.slice(),
    open: "./a/./b/./target"u8,
    target: "a/b/target"u8
), new(
    name: "path with dotdot"u8,
    fs: new @string[]{
        "a/b/"u8
    }.slice(),
    open: "a/../a/b/../../a/b/../b/target"u8,
    target: "a/b/target"u8
), new(
    name: "path with dotdot slash"u8,
    fs: new @string[]{}.slice(),
    open: "../"u8,
    wantError: true
), new(
    name: "path with dotdot dotdot slash"u8,
    fs: new @string[]{}.slice(),
    open: "a/../../"u8,
    wantError: true
), new(
    name: "dotdot no symlink"u8,
    fs: new @string[]{
        "a/"u8
    }.slice(),
    open: "a/../target"u8,
    target: "target"u8
), new(
    name: "dotdot after symlink"u8,
    fs: new @string[]{
        "a => b/c"u8,
        "b/c/"u8
    }.slice(),
    open: "a/../target"u8,
    target: ((Func<@string>)(() => {
        if (Δruntime.GOOS == "windows"u8) {
            return "target"u8;
        }
        return "b/target"u8;
    }))()
), new(
    name: "dotdot before symlink"u8,
    fs: new @string[]{
        "a => b/c"u8,
        "b/c/"u8
    }.slice(),
    open: "b/../a/target"u8,
    target: "b/c/target"u8
), new(
    name: "symlink ends in dot"u8,
    fs: new @string[]{
        "a => b/."u8,
        "b/"u8
    }.slice(),
    open: "a/target"u8,
    target: "b/target"u8
), new(
    name: "directory does not exist"u8,
    fs: new @string[]{}.slice(),
    open: "a/file"u8,
    wantError: true,
    alwaysFails: true
), new(
    name: "empty path"u8,
    fs: new @string[]{}.slice(),
    open: ""u8,
    wantError: true,
    alwaysFails: true
), new(
    name: "symlink cycle"u8,
    fs: new @string[]{
        "a => a"u8
    }.slice(),
    open: "a"u8,
    ltarget: "a"u8,
    wantError: true,
    alwaysFails: true
), new(
    name: "path escapes"u8,
    fs: new @string[]{}.slice(),
    open: "../ROOT/target"u8,
    target: "target"u8,
    wantError: true
), new(
    name: "long path escapes"u8,
    fs: new @string[]{
        "a/"u8
    }.slice(),
    open: "a/../../ROOT/target"u8,
    target: "target"u8,
    wantError: true
), new(
    name: "absolute symlink"u8,
    fs: new @string[]{
        "link => $ABS/target"u8
    }.slice(),
    open: "link"u8,
    ltarget: "link"u8,
    target: "target"u8,
    wantError: true
), new(
    name: "relative symlink"u8,
    fs: new @string[]{
        "link => ../ROOT/target"u8
    }.slice(),
    open: "link"u8,
    target: "target"u8,
    ltarget: "link"u8,
    wantError: true
), new(
    name: "symlink chain escapes"u8,
    fs: new @string[]{
        "link => a/b/c/target"u8,
        "a/b => e"u8,
        "a/e => ../../ROOT"u8,
        "c/"u8
    }.slice(),
    open: "link"u8,
    target: "c/target"u8,
    ltarget: "link"u8,
    wantError: true
)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootOpenQˢ = "root.Open(%q)"u8;

public static void TestRootOpen_File(ж<Δtesting.T> Ꮡt) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            GoFrame ᒐ = default;
            try {
                if (target != ""u8) {
                    {
                        var errΔ1 = Δos.WriteFile(target, wantʗ1, 438); if (errΔ1 != default!) {
                            tΔ1.Fatal(errΔ1);
                        }
                    }
                }
                var (f, err) = root.Open(testʗ1.open);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootOpenQˢ, testʗ1.open)) {
                    return;
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                (var got, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                if (err != default! || !bytes.Equal(got, wantʗ1)) {
                    tΔ1.Errorf(@"Dir.Open(%q): read content %q, %v; want %q"u8, testʗ1.open, ((@string)got), err, ((@string)wantʗ1));
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestRootOpen_Directory(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            GoFrame ᒐ = default;
            try {
                if (target != ""u8) {
                    {
                        var errΔ1 = Δos.Mkdir(target, 511); if (errΔ1 != default!) {
                            tΔ1.Fatal(errΔ1);
                        }
                    }
                    {
                        var errΔ2 = Δos.WriteFile(target + "/found"u8, default!, 438); if (errΔ2 != default!) {
                            tΔ1.Fatal(errΔ2);
                        }
                    }
                }
                var (f, err) = root.Open(testʗ1.open);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootOpenQˢ, testʗ1.open)) {
                    return;
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                (var got, err) = f.Readdirnames(-1);
                if (err != default!) {
                    tΔ1.Errorf(@"Dir.Open(%q).Readdirnames: %v"u8, testʗ1.open, err);
                }
                {
                    var want = new @string[]{"found"u8}.slice(); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                        tΔ1.Errorf(@"Dir.Open(%q).Readdirnames: %q, want %q"u8, testʗ1.open, got, want);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootCreateQˢ = "root.Create(%q)"u8;

public static void TestRootCreate(ж<Δtesting.T> Ꮡt) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var (f, err) = root.Create(testʗ1.open);
            if (errEndsTest(tΔ1, err, testʗ1.wantError, rootCreateQˢ, testʗ1.open)) {
                return;
            }
            {
                var (_, errΔ1) = f.Write(wantʗ1); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            f.Close();
            (var got, err) = Δos.ReadFile(target);
            if (err != default!) {
                tΔ1.Fatalf(@"reading file created with root.Create(%q): %v"u8, testʗ1.open, err);
            }
            if (!bytes.Equal(got, wantʗ1)) {
                tΔ1.Fatalf(@"reading file created with root.Create(%q): got %q; want %q"u8, testʗ1.open, got, wantʗ1);
            }
        });
    }
}

public static void TestRootMkdir(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (!wantError) {
                var (fiΔ1, errΔ1) = Δos.Lstat(filepath.Join(root.Name(), testʗ1.open));
                if (errΔ1 == default! && fiΔ1.Mode().Type() == fs.ModeSymlink) {
                    // This case is trying to mkdir("some symlink"),
                    // which is an error.
                    wantError = true;
                }
            }
            var err = root.Mkdir(testʗ1.open, 511);
            if (errEndsTest(tΔ1, err, wantError, rootCreateQˢ, testʗ1.open)) {
                return;
            }
            (var fi, err) = Δos.Lstat(target);
            if (err != default!) {
                tΔ1.Fatalf(@"stat file created with Root.Mkdir(%q): %v"u8, testʗ1.open, err);
            }
            if (!fi.IsDir()) {
                tΔ1.Fatalf(@"stat file created with Root.Mkdir(%q): not a directory"u8, testʗ1.open);
            }
            {
                var mode = fi.Mode(); if ((fs.FileMode)(mode & 511) == 0) {
                    // Issue #73559: We're not going to worry about the exact
                    // mode bits (which will have been modified by umask),
                    // but there should be mode bits.
                    tΔ1.Fatalf(@"stat file created with Root.Mkdir(%q): mode=%v, want non-zero"u8, testʗ1.open, mode);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootOpenRootQˢ = "root.OpenRoot(%q)"u8;

public static void TestRootOpenRoot(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            GoFrame ᒐ = default;
            try {
                if (target != ""u8) {
                    {
                        var errΔ1 = Δos.Mkdir(target, 511); if (errΔ1 != default!) {
                            tΔ1.Fatal(errΔ1);
                        }
                    }
                    {
                        var errΔ2 = Δos.WriteFile(target + "/f"u8, default!, 438); if (errΔ2 != default!) {
                            tΔ1.Fatal(errΔ2);
                        }
                    }
                }
                var (rr, err) = root.OpenRoot(testʗ1.open);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootOpenRootQˢ, testʗ1.open)) {
                    return;
                }
                var rrʗ1 = rr;
                defer(() => rrʗ1.Close(), ref ᒐ);
                (var f, err) = rr.Open("f"u8);
                if (err != default!) {
                    tΔ1.Fatalf(@"root.OpenRoot(%q).Open(""f"") = %v"u8, testʗ1.open, err);
                }
                f.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootRemoveQˢ = "root.Remove(%q)"u8;

public static void TestRootRemoveFile(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8){
                // Remove doesn't follow symlinks in the final path component,
                // so it will successfully remove ltarget.
                wantError = false;
                target = filepath.Join(root.Name(), testʗ1.ltarget);
            } else 
            if (target != ""u8) {
                {
                    var errΔ1 = Δos.WriteFile(target, default!, 438); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            }
            var err = root.Remove(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootRemoveQˢ, testʗ1.open)) {
                return;
            }
            (_, err) = Δos.Lstat(target);
            if (!errors.Is(err, Δos.ErrNotExist)) {
                tΔ1.Fatalf(@"stat file removed with Root.Remove(%q): %v, want ErrNotExist"u8, testʗ1.open, err);
            }
        });
    }
}

public static void TestRootRemoveDirectory(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8){
                // Remove doesn't follow symlinks in the final path component,
                // so it will successfully remove ltarget.
                wantError = false;
                target = filepath.Join(root.Name(), testʗ1.ltarget);
            } else 
            if (target != ""u8) {
                {
                    var errΔ1 = Δos.Mkdir(target, 511); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            }
            var err = root.Remove(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootRemoveQˢ, testʗ1.open)) {
                return;
            }
            (_, err) = Δos.Lstat(target);
            if (!errors.Is(err, Δos.ErrNotExist)) {
                tΔ1.Fatalf(@"stat file removed with Root.Remove(%q): %v, want ErrNotExist"u8, testʗ1.open, err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object osOpenRootFileSucceededˢ = (@string)"os.OpenRoot(file) succeeded; want failure"u8;
internal static readonly object rootOpenRootFileˢ = (@string)"Root.OpenRoot(file) succeeded; want failure"u8;

public static void TestRootOpenFileAsRoot(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        @string target = filepath.Join(dir, targetˢ);
        {
            var errΔ1 = Δos.WriteFile(target, default!, 438); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var (_, err) = Δos.OpenRoot(target);
        if (err == default!) {
            Ꮡt.Fatal(osOpenRootFileSucceededˢ);
        }
        (var r, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        (_, err) = r.OpenRoot(targetˢ);
        if (err == default!) {
            Ꮡt.Fatal(rootOpenRootFileˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootStatQˢ = "root.Stat(%q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ = "content"u8;

public static void TestRootStat(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string content = contentᶜ;
            if (target != ""u8) {
                {
                    var errΔ1 = Δos.WriteFile(target, slice<byte>(content), 438); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            }
            var (fi, err) = root.Stat(testʗ1.open);
            if (errEndsTest(tΔ1, err, testʗ1.wantError, rootStatQˢ, testʗ1.open)) {
                return;
            }
            {
                @string got = fi.Name();
                @string want = filepath.Base(testʗ1.open); if (got != want) {
                    tΔ1.Errorf("root.Stat(%q).Name() = %q, want %q"u8, testʗ1.open, got, want);
                }
            }
            {
                var (got, want) = (fi.Size(), (int64)len(content)); if (got != want) {
                    tΔ1.Errorf("root.Stat(%q).Size() = %v, want %v"u8, testʗ1.open, got, want);
                }
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ1 = "content"u8;

public static void TestRootLstat(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string content = contentᶜ1;
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8){
                // Lstat will stat the final link, rather than following it.
                wantError = false;
            } else 
            if (target != ""u8) {
                {
                    var errΔ1 = Δos.WriteFile(target, slice<byte>(content), 438); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            }
            var (fi, err) = root.Lstat(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootStatQˢ, testʗ1.open)) {
                return;
            }
            {
                @string got = fi.Name();
                @string want = filepath.Base(testʗ1.open); if (got != want) {
                    tΔ1.Errorf("root.Stat(%q).Name() = %q, want %q"u8, testʗ1.open, got, want);
                }
            }
            if (testʗ1.ltarget == ""u8){
                {
                    var got = fi.Mode(); if ((fs.FileMode)(got & Δos.ModeSymlink) != 0) {
                        tΔ1.Errorf("root.Stat(%q).Mode() = %v, want non-symlink"u8, testʗ1.open, got);
                    }
                }
                {
                    var (got, want) = (fi.Size(), (int64)len(content)); if (got != want) {
                        tΔ1.Errorf("root.Stat(%q).Size() = %v, want %v"u8, testʗ1.open, got, want);
                    }
                }
            } else {
                {
                    var got = fi.Mode(); if ((fs.FileMode)(got & Δos.ModeSymlink) == 0) {
                        tΔ1.Errorf("root.Stat(%q).Mode() = %v, want symlink"u8, testʗ1.open, got);
                    }
                }
            }
        });
    }
}

// A rootConsistencyTest is a test case comparing os.Root behavior with
// the corresponding non-Root function.
//
// These tests verify that, for example, Root.Open("file/./") and os.Open("file/./")
// have the same result, although the specific result may vary by platform.
partial struct rootConsistencyTest {
    internal @string name;
    // fs is the test filesystem layout. See makefs above.
    // fsFunc is called to modify the test filesystem, or replace it.
    internal slice<@string> fs;
    internal Func<ж<Δtesting.T>, @string, @string> fsFunc;
    // open is the filename to access in the test.
    internal @string open;
    // detailedErrorMismatch indicates that os.Root and the corresponding non-Root
    // function return different errors for this test.
    internal Func<ж<Δtesting.T>, bool> detailedErrorMismatch;
}

// FreeBSD returns EPERM in the non-Root case.
// os.Create returns ENOTDIR or EISDIR depending on the platform.
// os.Create returns ENOTDIR or EISDIR depending on the platform.
// On Windows, os.Root.Open returns "The directory name is invalid."
// and os.Open returns "The file cannot be accessed by the system.".
internal static slice<rootConsistencyTest> rootConsistencyTestCases = new rootConsistencyTest[]{new(
    name: "file"u8,
    fs: new @string[]{
        "target"u8
    }.slice(),
    open: "target"u8
), new(
    name: "dir slash dot"u8,
    fs: new @string[]{
        "target/file"u8
    }.slice(),
    open: "target/."u8
), new(
    name: "dot"u8,
    fs: new @string[]{
        "file"u8
    }.slice(),
    open: "."u8
), new(
    name: "file slash dot"u8,
    fs: new @string[]{
        "target"u8
    }.slice(),
    open: "target/."u8,
    detailedErrorMismatch: (ж<Δtesting.T> t) => Δruntime.GOOS == "freebsd"u8 && strings.HasPrefix(t.Name(), "TestRootConsistencyRemove"u8)
), new(
    name: "dir slash"u8,
    fs: new @string[]{
        "target/file"u8
    }.slice(),
    open: "target/"u8
), new(
    name: "dot slash"u8,
    fs: new @string[]{
        "file"u8
    }.slice(),
    open: "./"u8
), new(
    name: "file slash"u8,
    fs: new @string[]{
        "target"u8
    }.slice(),
    open: "target/"u8,
    detailedErrorMismatch: (ж<Δtesting.T> t) => Δruntime.GOOS == "js"u8
), new(
    name: "file in path"u8,
    fs: new @string[]{
        "file"u8
    }.slice(),
    open: "file/target"u8
), new(
    name: "directory in path missing"u8,
    open: "dir/target"u8
), new(
    name: "target does not exist"u8,
    open: "target"u8
), new(
    name: "symlink slash"u8,
    fs: new @string[]{
        "target/file"u8,
        "link => target"u8
    }.slice(),
    open: "link/"u8
), new(
    name: "symlink slash dot"u8,
    fs: new @string[]{
        "target/file"u8,
        "link => target"u8
    }.slice(),
    open: "link/."u8
), new(
    name: "file symlink slash"u8,
    fs: new @string[]{
        "target"u8,
        "link => target"u8
    }.slice(),
    open: "link/"u8,
    detailedErrorMismatch: (ж<Δtesting.T> t) => Δruntime.GOOS == "js"u8
), new(
    name: "unresolved symlink"u8,
    fs: new @string[]{
        "link => target"u8
    }.slice(),
    open: "link"u8
), new(
    name: "resolved symlink"u8,
    fs: new @string[]{
        "link => target"u8,
        "target"u8
    }.slice(),
    open: "link"u8
), new(
    name: "dotdot in path after symlink"u8,
    fs: new @string[]{
        "a => b/c"u8,
        "b/c/"u8,
        "b/target"u8
    }.slice(),
    open: "a/../target"u8
), new(
    name: "long file name"u8,
    open: strings.Repeat("a"u8, 500)
), new(
    name: "unreadable directory"u8,
    fs: new @string[]{
        "dir/target"u8
    }.slice(),
    fsFunc: (ж<Δtesting.T> t, @string dir) => {
        Δos.Chmod(filepath.Join(dir, "dir"), 0);
        t.Cleanup(() => {
            Δos.Chmod(filepath.Join(dir, "dir"), 448);
        });
        return dir;
    },
    open: "dir/target"u8
), new(
    name: "unix domain socket target"u8,
    fsFunc: (ж<Δtesting.T> t, @string dir) => tempDirWithUnixSocket(t, "a"u8),
    open: "a"u8
), new(
    name: "unix domain socket in path"u8,
    fsFunc: (ж<Δtesting.T> t, @string dir) => tempDirWithUnixSocket(t, "a"u8),
    open: "a/b"u8,
    detailedErrorMismatch: (ж<Δtesting.T> t) => Δruntime.GOOS == "windows"u8
), new(
    name: "question mark"u8,
    open: "?"u8
), new(
    name: "nul byte"u8,
    open: "\x00"u8
)
}.slice();

internal static @string tempDirWithUnixSocket(ж<Δtesting.T> Ꮡt, @string name) {
    var (dir, err) = Δos.MkdirTemp(""u8, ""u8);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Cleanup(() => {
        {
            var errΔ1 = Δos.RemoveAll(dir); if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
            }
        }
    });
    (var addr, err) = Δnet.ResolveUnixAddr("unix"u8, filepath.Join(dir, name));
    if (err != default!) {
        Ꮡt.Skipf("net.ResolveUnixAddr: %v"u8, err);
    }
    (var conn, err) = Δnet.ListenUnix("unix"u8, addr);
    if (err != default!) {
        Ꮡt.Skipf("net.ListenUnix: %v"u8, err);
    }
    var connʗ1 = conn;
    Ꮡt.Cleanup(() => {
        connʗ1.Close();
    });
    return dir;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object inconsistentResultsOnˢ = (@string)"#69509: inconsistent results on wasip1"u8;

internal static void run(this rootConsistencyTest test, ж<Δtesting.T> Ꮡt, Func<ж<Δtesting.T>, @string, ж<Δos.Root>, (@string, error)> f) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Δruntime.GOOS == "wasip1"u8) {
        // On wasip, non-Root functions clean paths before opening them,
        // resulting in inconsistent behavior.
        // https://go.dev/issue/69509
        Ꮡt.Skip(inconsistentResultsOnˢ);
    }
    var testʗ1 = test;
    Ꮡt.Run(test.name, (ж<Δtesting.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            @string dir1 = makefs(tΔ1, testʗ1.fs);
            @string dir2 = makefs(tΔ1, testʗ1.fs);
            if (testʗ1.fsFunc != default!) {
                dir1 = testʗ1.fsFunc(tΔ1, dir1);
                dir2 = testʗ1.fsFunc(tΔ1, dir2);
            }
            var (r, err) = Δos.OpenRoot(dir1);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
            var (res1, err1) = f(tΔ1, testʗ1.open, r);
            var (res2, err2) = f(tΔ1, dir2 + "/"u8 + testʗ1.open, nil);
            if (res1 != res2 || ((err1 == default!) != (err2 == default!))) {
                tΔ1.Errorf("with root:    res=%v"u8, res1);
                tΔ1.Errorf("              err=%v"u8, err1);
                tΔ1.Errorf("without root: res=%v"u8, res2);
                tΔ1.Errorf("              err=%v"u8, err2);
                tΔ1.Errorf("want consistent results, got mismatch"u8);
            }
            if (err1 != default! || err2 != default!) {
                var (e1, ok) = err1._<ж<osꓸPathError>>(ᐧ);
                if (!ok) {
                    tΔ1.Fatalf("with root, expected PathError; got: %v"u8, err1);
                }
                (var e2, ok) = err2._<ж<osꓸPathError>>(ᐧ);
                if (!ok) {
                    tΔ1.Fatalf("without root, expected PathError; got: %v"u8, err1);
                }
                var detailedErrorMismatch = false;
                {
                    var fΔ1 = testʗ1.detailedErrorMismatch; if (fΔ1 != default!) {
                        detailedErrorMismatch = fΔ1(tΔ1);
                    }
                }
                if (Δruntime.GOOS == "plan9"u8) {
                    // Plan9 syscall errors aren't comparable.
                    detailedErrorMismatch = true;
                }
                if (!detailedErrorMismatch && !AreEqual((~e1).Err, (~e2).Err)) {
                    tΔ1.Errorf("with root:    err=%v"u8, (~e1).Err);
                    tΔ1.Errorf("without root: err=%v"u8, (~e2).Err);
                    tΔ1.Errorf("want consistent results, got mismatch"u8);
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

public static void TestRootConsistencyOpen(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            GoFrame ᒐ = default;
            try {
                ж<Δos.File> f = default!;
                error err = default!;
                if (r == nil){
                    (f, err) = Δos.Open(path);
                } else {
                    (f, err) = r.Open(path);
                }
                if (err != default!) {
                    return ("", err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                (var fi, err) = f.Stat();
                if (err == default! && !fi.IsDir()){
                    var (b, errΔ1) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    return (((@string)b), errΔ1);
                } else {
                    var (names, errΔ2) = f.Readdirnames(-1);
                    slices.Sort<slice<@string>, @string>(names);
                    return (fmt.Sprintf("%q"u8, names), errΔ2);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestRootConsistencyCreate(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            ж<Δos.File> f = default!;
            error err = default!;
            if (r == nil){
                (f, err) = Δos.Create(path);
            } else {
                (f, err) = r.Create(path);
            }
            if (err == default!) {
                f.Write(slice<byte>("file contents"u8));
                f.Close();
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyMkdir(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default!;
            if (r == nil){
                err = Δos.Mkdir(path, 511);
            } else {
                err = r.Mkdir(path, 511);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyRemove(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in rootConsistencyTestCases) {
        if (test.open == "."u8 || test.open == "./"u8) {
            continue; // can't remove the root itself
        }
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default!;
            if (r == nil){
                err = Δos.Remove(path);
            } else {
                err = r.Remove(path);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyStat(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            fs.FileInfo fi = default!;
            error err = default!;
            if (r == nil){
                (fi, err) = Δos.Stat(path);
            } else {
                (fi, err) = r.Stat(path);
            }
            if (err != default!) {
                return ("", err);
            }
            return (fmt.Sprintf("name:%q size:%v mode:%v isdir:%v"u8, fi.Name(), fi.Size(), fi.Mode(), fi.IsDir()), default!);
        });
    }
}

public static void TestRootConsistencyLstat(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            fs.FileInfo fi = default!;
            error err = default!;
            if (r == nil){
                (fi, err) = Δos.Lstat(path);
            } else {
                (fi, err) = r.Lstat(path);
            }
            if (err != default!) {
                return ("", err);
            }
            return (fmt.Sprintf("name:%q size:%v mode:%v isdir:%v"u8, fi.Name(), fi.Size(), fi.Mode(), fi.IsDir()), default!);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gowasiruntimeˢ = "GOWASIRUNTIME"u8;
internal static readonly object wazeroDoesNotTrackˢ = (@string)"wazero does not track renamed directories"u8;

public static void TestRootRenameAfterOpen(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "windows"u8) {
            Ꮡt.Skip("renaming open files not supported on " + Δruntime.GOOS);
        }
        else if (exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8) {
            Ꮡt.Skip("openat not supported on " + Δruntime.GOOS);
        }
        else if (exprᴛ1 == "wasip1"u8) {
            if (Δos.Getenv(gowasiruntimeˢ) == "wazero"u8) {
                Ꮡt.Skip(wazeroDoesNotTrackˢ);
            }
        }

        @string dir = Ꮡt.TempDir();
        // Create directory "a" and open it.
        {
            var errΔ1 = Δos.Mkdir(filepath.Join(dir, "a"), 511); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var (dirf, err) = Δos.OpenRoot(filepath.Join(dir, "a"));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dirfʗ1 = dirf;
        defer(() => dirfʗ1.Close(), ref ᒐ);
        // Rename "a" => "b", and create "b/f".
        {
            var errΔ2 = Δos.Rename(filepath.Join(dir, "a"), filepath.Join(dir, "b")); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        {
            var errΔ3 = Δos.WriteFile(filepath.Join(dir, "b/f"), slice<byte>("hello"u8), 438); if (errΔ3 != default!) {
                Ꮡt.Fatal(errΔ3);
            }
        }
        // Open "f", and confirm that we see it.
        (var f, err) = dirf.OpenFile("f"u8, Δos.O_RDONLY, 0);
        if (err != default!) {
            Ꮡt.Fatalf("reading file after renaming parent: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var b, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        {
            @string got = ((@string)b);
            @string want = helloˢ; if (got != want) {
                Ꮡt.Fatalf("file contents: %q, want %q"u8, got, want);
            }
        }
        // f.Name reflects the original path we opened the directory under (".../a"), not "b".
        {
            @string got = f.Name();
            @string want = dirf.Name() + ((@string)(rune)Δos.PathSeparator) + "f"u8; if (got != want) {
                Ꮡt.Errorf("f.Name() = %q, want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRootNonPermissionMode(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (r, err) = Δos.OpenRoot(Ꮡt.TempDir());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = r.OpenFile(fileˢ2, (nint)(Δos.O_RDWR | Δos.O_CREATE), 1023); if (errΔ1 == default!) {
                Ꮡt.Errorf("r.OpenFile(file, O_RDWR|O_CREATE, 0o1777) succeeded; want error"u8);
            }
        }
        {
            var errΔ2 = r.Mkdir(fileˢ2, 1023); if (errΔ2 == default!) {
                Ꮡt.Errorf("r.Mkdir(file, 0o1777) succeeded; want error"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestRootUseAfterClose_type /*dyn*/ {
    internal @string name;
    internal Func<ж<Δos.Root>, @string, error> f;
}

public static void TestRootUseAfterClose(ж<Δtesting.T> Ꮡt) {
    var (r, err) = Δos.OpenRoot(Ꮡt.TempDir());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    r.Close();
    foreach (var (_, test) in new TestRootUseAfterClose_type[]{new(
        name: "Open"u8,
        f: (ж<Δos.Root> rΔ1, @string filename) => {
            var (_, errΔ1) = rΔ1.Open(filename);
            return errΔ1;
        }
    ), new(
        name: "Create"u8,
        f: (ж<Δos.Root> rΔ2, @string filename) => {
            var (_, errΔ2) = rΔ2.Create(filename);
            return errΔ2;
        }
    ), new(
        name: "OpenFile"u8,
        f: (ж<Δos.Root> rΔ3, @string filename) => {
            var (_, errΔ3) = rΔ3.OpenFile(filename, Δos.O_RDWR, 438);
            return errΔ3;
        }
    ), new(
        name: "OpenRoot"u8,
        f: (ж<Δos.Root> rΔ4, @string filename) => {
            var (_, errΔ4) = rΔ4.OpenRoot(filename);
            return errΔ4;
        }
    ), new(
        name: "Mkdir"u8,
        f: (ж<Δos.Root> rΔ5, @string filename) => rΔ5.Mkdir(filename, 511)
    )
    }.slice()) {
        var errΔ5 = test.f(r, targetˢ);
        var (pe, ok) = errΔ5._<ж<osꓸPathError>>(ᐧ);
        if (!ok || (~pe).Path != "target"u8 || !AreEqual((~pe).Err, Δos.ErrClosed)) {
            Ꮡt.Errorf(@"r.%v = %v; want &PathError{Path: ""target"", Err: ErrClosed}"u8, test.name, errΔ5);
        }
    }
}

public static partial void TestRootConcurrentClose(ж<Δtesting.T> Ꮡt) {
    var (r, err) = Δos.OpenRoot(Ꮡt.TempDir());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ch = new channel<error>(1);
    var chʗ1 = ch;
    var rʗ1 = r;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), chʗ1, ref ᒐ);
            var first = true;
            while (ᐧ) {
                var (f, errΔ1) = rʗ1.OpenFile(fileˢ2, (nint)(Δos.O_RDWR | Δos.O_CREATE), 438);
                if (errΔ1 != default!) {
                    chʗ1.ᐸꟷ(errΔ1);
                    return;
                }
                if (first) {
                    chʗ1.ᐸꟷ(default!);
                    first = false;
                }
                f.Close();
                if (Δruntime.GOARCH == "wasm"u8) {
                    // TODO(go.dev/issue/71134) can lead to goroutine starvation.
                    Δruntime.Gosched();
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    {
        var errΔ2 = ᐸꟷ(ch); if (errΔ2 != default!) {
            Ꮡt.Errorf("OpenFile: %v, want success"u8, errΔ2);
        }
    }
    r.Close();
    {
        var errΔ3 = ᐸꟷ(ch); if (!errors.Is(errΔ3, Δos.ErrClosed)) {
            Ꮡt.Errorf("OpenFile: %v, want ErrClosed"u8, errΔ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object publicˢ = (@string)"public"u8;

// TestRootRaceRenameDir attempts to escape a Root by renaming a path component mid-parse.
//
// We create a deeply nested directory:
//
//	base/a/a/a/a/ [...] /a
//
// And a path that descends into the tree, then returns to the top using ..:
//
//	base/a/a/a/a/ [...] /a/../../../ [..] /../a/f
//
// While opening this file, we rename base/a/a to base/b.
// A naive lookup operation will resolve the path to base/f.
public static partial void TestRootRaceRenameDir(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string dir = Ꮡt.TempDir();
        var (r, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        const nint depth = 4;
        Δos.MkdirAll(dir + "/base/"u8 + strings.Repeat("/a"u8, depth), 511);
        @string path = "base/"u8 + strings.Repeat("a/"u8, depth) + strings.Repeat("../"u8, depth) + "a/f"u8;
        Δos.WriteFile(dir + "/f"u8, slice<byte>("secret"u8), 438);
        Δos.WriteFile(dir + "/base/a/f"u8, slice<byte>("public"u8), 438);
        // Compute how long it takes to open the path in the common case.
        UntypedInt tries = 10;
        time.Duration total = default!;
        foreach (var _ᴛ1 in range(tries)) {
            var start = time.Now();
            var (f, errΔ1) = r.Open(path);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            (var b, errΔ1) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (((sstring)b) != "public"u8) {
                Ꮡt.Fatalf("read %q, want %q"u8, b, publicˢ);
            }
            f.Close();
            total += time.Since(start);
        }
        var avg = total / (int64)tries;
        // We're trying to exploit a race, so try this a number of times.
        foreach (var _ᴛ2 in range(100)) {
            // Start a goroutine to open the file.
            var gotc = new channel<slice<byte>>(0);
            var gotcʗ1 = gotc;
            var rʗ2 = r;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    var (f, errΔ2) = rʗ2.Open(path);
                    if (errΔ2 != default!) {
                        gotcʗ1.ᐸꟷ(default!);
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    var (b, _) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    gotcʗ1.ᐸꟷ(b);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            // Wait for the open operation to partially complete,
            // and then rename a directory near the root.
            time.Sleep(avg / 4);
            {
                var errΔ3 = Δos.Rename(dir + "/base/a"u8, dir + "/b"u8); if (errΔ3 != default!) {
                    // Windows and Plan9 won't let us rename a directory if we have
                    // an open handle for it, so an error here is expected.
                    var exprᴛ1 = Δruntime.GOOS;
                    if (exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8) {
                    }
                    else { /* default: */
                        Ꮡt.Fatal(errΔ3);
                    }

                }
            }
            var got = ᐸꟷ(gotc);
            Δos.Rename(dir + "/b"u8, dir + "/base/a"u8);
            if (len(got) > 0 && ((sstring)got) != "public"u8) {
                Ꮡt.Errorf("read file: %q; want error or 'public'"u8, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dDNewˢ = "d/d/new"u8;

public static void TestRootSymlinkToRoot(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = makefs(Ꮡt, new @string[]{
            "d/d => .."u8
        }.slice());
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        {
            var errΔ1 = root.Mkdir(dDNewˢ, 511); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        (var f, err) = root.Open("d/d"u8);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var names, err) = f.Readdirnames(-1);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        slices.Sort<slice<@string>, @string>(names);
        {
            var (got, want) = (names, new @string[]{"d"u8, "new"u8}.slice()); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                Ꮡt.Errorf("root contains: %q, want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestOpenInRoot(ж<Δtesting.T> Ꮡt) {
    @string dir = makefs(Ꮡt, new @string[]{
        "file"u8,
        "link => ../ROOT/file"u8
    }.slice());
    var (f, err) = Δos.OpenInRoot(dir, fileˢ2);
    if (err != default!) {
        Ꮡt.Fatalf("OpenInRoot(`file`) = %v, want success"u8, err);
    }
    f.Close();
    foreach (var (_, name) in new @string[]{
        "link"u8,
        "../ROOT/file"u8,
        dir + "/file"u8
    }.slice()) {
        var (fΔ1, errΔ1) = Δos.OpenInRoot(dir, name);
        if (errΔ1 == default!) {
            fΔ1.Close();
            Ꮡt.Fatalf("OpenInRoot(%q) = nil, want error"u8, name);
        }
    }
}

public static void TestRootName(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        {
            @string got = root.Name();
            @string want = dir; if (got != want) {
                Ꮡt.Errorf("root.Name() = %q, want %q"u8, got, want);
            }
        }
        (var f, err) = root.Create(fileˢ2);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            @string got = f.Name();
            @string want = filepath.Join(dir, fileˢ2); if (got != want) {
                Ꮡt.Errorf(@"root.Create(""file"").Name() = %q, want %q"u8, got, want);
            }
        }
        {
            var errΔ1 = root.Mkdir(dirˢ, 511); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        (var subroot, err) = root.OpenRoot(dirˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var subrootʗ1 = subroot;
        defer(() => subrootʗ1.Close(), ref ᒐ);
        {
            @string got = subroot.Name();
            @string want = filepath.Join(dir, dirˢ); if (got != want) {
                Ꮡt.Errorf(@"root.OpenRoot(""dir"").Name() = %q, want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end os_test_package
