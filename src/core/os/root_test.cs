// Copyright 2024 The Go Authors. All rights reserved.
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
using iter = iter_package;
using Δnet = net_package;
using Δos = os_package;
using Δpath = path_package;
using filepath = go.path.filepath_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using Δtesting = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.io;
using go.path;
using static go.os_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class os_test_package {

// testMaybeRooted calls f in two subtests,
// one with a Root and one with a nil r.
internal static void testMaybeRooted(ж<Δtesting.T> t, Action<ж<Δtesting.T>, ж<Δos.Root>> f) {
    t.Run(noRootˢ, (ж<Δtesting.T> tΔ1) => {
        tΔ1.Chdir(tΔ1.TempDir());
        f(tΔ1, nil);
    });
    t.Run(inRootˢ, (ж<Δtesting.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            tΔ2.Chdir(tΔ2.TempDir());
            var (r, err) = Δos.OpenRoot(".");
            if (err != nil) {
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
internal static ref readonly @string rootˢ => ref rootˢᵇ;
private static readonly @string rootˢᵇ = "ROOT"u8;
internal static ref readonly @string absˢ => ref absˢᵇ;
private static readonly @string absˢᵇ = "$ABS"u8;

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
internal static @string makefs(ж<Δtesting.T> t, slice<@string> fs) {
    @string root = filepath.Join(t.TempDir(), rootˢ);
    {
        var err = Δos.Mkdir(root, 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    foreach (var (_, vᴛ1) in range(fs)) {
        var ent = vᴛ1;

        ent = strings.ReplaceAll(ent, absˢ, root);
        var (@base, link, isLink) = strings.Cut(ent, " => ");
        if (isLink) {
            if (Δruntime.GOOS == "wasip1"u8 && Δpath.IsAbs(link)) {
                t.Skip("absolute link targets not supported on " + Δruntime.GOOS);
            }
            if (Δruntime.GOOS == "plan9"u8) {
                t.Skip("symlinks not supported on " + Δruntime.GOOS);
            }
            ent = @base;
        }
        {
            var err = Δos.MkdirAll(Δpath.Join(root, Δpath.Dir(@base)), 511); if (err != nil) {
                t.Fatal(err);
            }
        }
        if (isLink) {
            var err = Δos.Symlink(link, Δpath.Join(root, @base)); if (err != nil) {
                t.Fatal(err);
            }
        } else if (strings.HasSuffix(ent, "/")) {
            var err = Δos.MkdirAll(Δpath.Join(root, ent), 511); if (err != nil) {
                t.Fatal(err);
            }
        } else {
            var err = Δos.WriteFile(Δpath.Join(root, ent), slice<byte>(ent), 438); if (err != nil) {
                t.Fatal(err);
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
internal static void run(this ж<rootTest> Ꮡtest, ж<Δtesting.T> t, Action<ж<Δtesting.T>, @string, ж<Δos.Root>> f) {
    ref var test = ref Ꮡtest.DerefOrNull();

    t.Run(test.name, (ж<Δtesting.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            @string root = makefs(tΔ1, Ꮡtest.Value.fs);
            var (d, err) = Δos.OpenRoot(root);
            if (err != nil) {
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
internal static bool errEndsTest(ж<Δtesting.T> t, error err, bool wantError, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    if (wantError) {
        if (err == nil) {
            @string op = fmt.Sprintf(format, args.ꓸꓸꓸ);
            t.Fatalf("%v = nil; want error", op);
        }
        return true;
    } else {
        if (err != nil) {
            @string op = fmt.Sprintf(format, args.ꓸꓸꓸ);
            t.Fatalf("%v = %v; want success", op, err);
        }
        return false;
    }
}

internal static slice<rootTest> rootTestCases = slice<rootTest>([new(
    name: "plain path",
    fs: [],
    open: "target",
    target: "target"
), new(
    name: "path in directory",
    fs: [
        "a/b/c/"
    ],
    open: "a/b/c/target",
    target: "a/b/c/target"
), new(
    name: "symlink",
    fs: [
        "link => target"
    ],
    open: "link",
    target: "target",
    ltarget: "link"
), new(
    name: "symlink dotdot slash",
    fs: [
        "link => ../"
    ],
    open: "link",
    ltarget: "link",
    wantError: true
), new(
    name: "symlink ending in slash",
    fs: [
        "dir/",
        "link => dir/"
    ],
    open: "link/target",
    target: "dir/target"
), new(
    name: "slash after symlink to file",
    fs: [
        "link => ../ROOT/target"
    ],
    open: "link/",
    target: "target",
    wantError: true
), new(
    name: "slash after symlink to dir",
    fs: [
        "link => ../ROOT/target",
        "target/"
    ],
    open: "link/",
    wantError: true
), new(
    name: "symlink dotdot dotdot slash",
    fs: [
        "dir/link => ../../"
    ],
    open: "dir/link",
    ltarget: "dir/link",
    wantError: true
), new(
    name: "symlink chain",
    fs: [
        "link => a/b/c/target",
        "a/b => e",
        "a/e => ../f",
        "f => g/h/i",
        "g/h/i => ..",
        "g/c/"
    ],
    open: "link",
    target: "g/c/target",
    ltarget: "link"
), new(
    name: "path with dot",
    fs: [
        "a/b/"
    ],
    open: "./a/./b/./target",
    target: "a/b/target"
), new(
    name: "path with dotdot",
    fs: [
        "a/b/"
    ],
    open: "a/../a/b/../../a/b/../b/target",
    target: "a/b/target"
), new(
    name: "path with dotdot slash",
    fs: [],
    open: "../",
    wantError: true
), new(
    name: "path with dotdot dotdot slash",
    fs: [
        "a/"
    ],
    open: "a/../../",
    wantError: true
), new(
    name: "dotdot no symlink",
    fs: [
        "a/"
    ],
    open: "a/../target",
    target: "target"
), new(
    name: "dotdot after symlink",
    fs: [
        "a => b/c",
        "b/c/"
    ],
    open: "a/../target",
    target: func(@string () => {
        if (Δruntime.GOOS == "windows"u8) {
            // On Windows, the path is cleaned before symlink resolution.
            return "target";
        }
        return "b/target";
    })()
), new(
    name: "dotdot before symlink",
    fs: [
        "a => b/c",
        "b/c/"
    ],
    open: "b/../a/target",
    target: "b/c/target"
), new(
    name: "symlink ends in dot",
    fs: [
        "a => b/.",
        "b/"
    ],
    open: "a/target",
    target: "b/target"
), new(
    name: "directory does not exist",
    fs: [],
    open: "a/file",
    wantError: true,
    alwaysFails: true
), new(
    name: "empty path",
    fs: [],
    open: "",
    wantError: true,
    alwaysFails: true
), new(
    name: "symlink cycle",
    fs: [
        "a => a"
    ],
    open: "a",
    ltarget: "a",
    wantError: true,
    alwaysFails: true
), new(
    name: "path escapes",
    fs: [],
    open: "../ROOT/target",
    target: "target",
    wantError: true
), new(
    name: "long path escapes",
    fs: [
        "a/"
    ],
    open: "a/../../ROOT/target",
    target: "target",
    wantError: true
), new(
    name: "absolute symlink",
    fs: [
        "link => $ABS/target"
    ],
    open: "link",
    ltarget: "link",
    target: "target",
    wantError: true
), new(
    name: "relative symlink",
    fs: [
        "link => ../ROOT/target"
    ],
    open: "link",
    target: "target",
    ltarget: "link",
    wantError: true
), new(
    name: "symlink chain escapes",
    fs: [
        "link => a/b/c/target",
        "a/b => e",
        "a/e => ../../ROOT",
        "c/"
    ],
    open: "link",
    target: "c/target",
    ltarget: "link",
    wantError: true
)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootOpenQˢ => ref rootOpenQˢᵇ;
private static readonly @string rootOpenQˢᵇ = "root.Open(%q)"u8;

public static void TestRootOpen_File(ж<Δtesting.T> t) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            GoFrame ᒐ = default;
            try {
                if (target != ""u8) {
                    var errΔ1 = Δos.WriteFile(target, wantʗ1, 438); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
                var (f, err) = root.Open(testʗ1.open);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootOpenQˢ, testʗ1.open)) {
                    return;
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                (var got, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                if (err != nil || !bytes.Equal(got, wantʗ1)) {
                    tΔ1.Errorf(@"Dir.Open(%q): read content %q, %v; want %q", testʗ1.open, (@string)got, err, (@string)wantʗ1);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestRootOpen_Directory(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            GoFrame ᒐ = default;
            try {
                if (target != ""u8) {
                    var errΔ1 = Δos.Mkdir(target, 511); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                    var errΔ2 = Δos.WriteFile(target + "/found"u8, default, 438); if (errΔ2 != nil) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
                var (f, err) = root.Open(testʗ1.open);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootOpenQˢ, testʗ1.open)) {
                    return;
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                (var got, err) = f.Readdirnames(-1);
                if (err != nil) {
                    tΔ1.Errorf(@"Dir.Open(%q).Readdirnames: %v", testʗ1.open, err);
                }
                var want = slice<@string>(["found"]); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                    tΔ1.Errorf(@"Dir.Open(%q).Readdirnames: %q, want %q", testʗ1.open, got, want);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootCreateQˢ => ref rootCreateQˢᵇ;
private static readonly @string rootCreateQˢᵇ = "root.Create(%q)"u8;

public static void TestRootCreate(ж<Δtesting.T> t) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var (f, err) = root.Create(testʗ1.open);
            if (errEndsTest(tΔ1, err, testʗ1.wantError, rootCreateQˢ, testʗ1.open)) {
                return;
            }
            var (_, errΔ1) = f.Write(wantʗ1); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            f.Close();
            (var got, err) = Δos.ReadFile(target);
            if (err != nil) {
                tΔ1.Fatalf(@"reading file created with root.Create(%q): %v", testʗ1.open, err);
            }
            if (!bytes.Equal(got, wantʗ1)) {
                tΔ1.Fatalf(@"reading file created with root.Create(%q): got %q; want %q", testʗ1.open, got, wantʗ1);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue71492ˢ = (@string)"https://go.dev/issue/71492"u8;
internal static ref readonly @string rootChmodQˢ => ref rootChmodQˢᵇ;
private static readonly @string rootChmodQˢᵇ = "root.Chmod(%q)"u8;

public static void TestRootChmod(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "wasip1"u8) {
        t.Skip("Chmod not supported on " + Δruntime.GOOS);
    }
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (target != ""u8) {
                // Create a file with no read/write permissions,
                // to ensure we can use Chmod on an inaccessible file.
                var errΔ1 = Δos.WriteFile(target, default, 0); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            if (Δruntime.GOOS == "windows"u8) {
                // On Windows, Chmod("symlink") affects the link, not its target.
                // See issue 71492.
                var (fi, errΔ2) = root.Lstat(testʗ1.open);
                if (errΔ2 == nil && !fi.Mode().IsRegular()) {
                    tΔ1.Skip(httpsGoDevIssue71492ˢ);
                }
            }
            var want = (fs.FileMode)438;
            var err = root.Chmod(testʗ1.open, want);
            if (errEndsTest(tΔ1, err, testʗ1.wantError, rootChmodQˢ, testʗ1.open)) {
                return;
            }
            (var st, err) = Δos.Stat(target);
            if (err != nil) {
                tΔ1.Fatalf("os.Stat(%q) = %v", target, err);
            }
            var got = st.Mode(); if (got != want) {
                tΔ1.Errorf("after root.Chmod(%q, %v): file mode = %v, want %v", testʗ1.open, want, got, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootChtimesQˢ => ref rootChtimesQˢᵇ;
private static readonly @string rootChtimesQˢᵇ = "root.Chtimes(%q)"u8;

internal partial struct TestRootChtimes_type /*dyn*/ {
    internal time.Time atime, mtime;
}

public static void TestRootChtimes(ж<Δtesting.T> t) {
    // Don't check atimes if the fs is mounted noatime,
    // or on Plan 9 which does not permit changing atimes to arbitrary values.
    var checkAtimes = !hasNoatime() && Δruntime.GOOS != "plan9"u8;
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (target != ""u8) {
                var err = Δos.WriteFile(target, default, 438); if (err != nil) {
                    tΔ1.Fatal(err);
                }
            }
            foreach (var (_, vᴛ2) in range(slice<TestRootChtimes_type>([new(
                atime: time.Now().Add((time.Duration)(-60000000000L)),
                mtime: time.Now().Add((time.Duration)(-60000000000L))
            ), new(
                atime: time.Now().Add((time.Duration)(60000000000L)),
                mtime: time.Now().Add((time.Duration)(60000000000L))
            ), new(
                atime: new time.Time(),
                mtime: time.Now()
            ), new(
                atime: time.Now(),
                mtime: new time.Time()
            )
            ]))) {
                var times = vᴛ2;

                var exprᴛ1 = Δruntime.GOOS;
                switch (ᐧ) {
                case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8: {
                    times.atime = times.atime.Truncate(1 * time.ΔSecond);
                    times.mtime = times.mtime.Truncate(1 * time.ΔSecond);
                    break;
                }
                case {} when exprᴛ1 == "illumos"u8: {
                    times.atime = times.atime.Truncate(1 * time.Microsecond);
                    times.mtime = times.mtime.Truncate(1 * time.Microsecond);
                    break;
                }}

                var err = root.Chtimes(testʗ1.open, times.atime, times.mtime);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootChtimesQˢ, testʗ1.open)) {
                    return;
                }
                (var st, err) = Δos.Stat(target);
                if (err != nil) {
                    tΔ1.Fatalf("os.Stat(%q) = %v", target, err);
                }
                {
                    var got = st.ModTime(); if (!times.mtime.IsZero() && !got.Equal(times.mtime)) {
                        tΔ1.Errorf("after root.Chtimes(%q, %v, %v): got mtime=%v, want %v", testʗ1.open, times.atime, times.mtime, got, times.mtime);
                    }
                }
                if (checkAtimes) {
                    var got = os_internal_test_package.Atime(st); if (!times.atime.IsZero() && !got.Equal(times.atime)) {
                        tΔ1.Errorf("after root.Chtimes(%q, %v, %v): got atime=%v, want %v", testʗ1.open, times.atime, times.mtime, got, times.atime);
                    }
                }
            }
        });
    }
}

public static void TestRootMkdir(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // This case is trying to mkdir("some symlink"),
                // which is an error (but not an escape).
                wantError = true;
            }

            var err = root.Mkdir(testʗ1.open, 511);
            if (errEndsTest(tΔ1, err, wantError, rootCreateQˢ, testʗ1.open)) {
                return;
            }
            (var fi, err) = Δos.Lstat(target);
            if (err != nil) {
                tΔ1.Fatalf(@"stat file created with Root.Mkdir(%q): %v", testʗ1.open, err);
            }
            if (!fi.IsDir()) {
                tΔ1.Fatalf(@"stat file created with Root.Mkdir(%q): not a directory", testʗ1.open);
            }
            var mode = fi.Mode(); if ((fs.FileMode)(mode & 511) == 0) {
                // Issue #73559: We're not going to worry about the exact
                // mode bits (which will have been modified by umask),
                // but there should be mode bits.
                tΔ1.Fatalf(@"stat file created with Root.Mkdir(%q): mode=%v, want non-zero", testʗ1.open, mode);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootMkdirAllQˢ => ref rootMkdirAllQˢᵇ;
private static readonly @string rootMkdirAllQˢᵇ = "root.MkdirAll(%q)"u8;

public static void TestRootMkdirAll(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        if (test.name == "directory does not exist"u8) {
            // Test expects error, mkdirall creates the missing directory.
            // TestRootMultiMkdirAll covers this case better anyway, just skip.
            continue;
        }
        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // This case is trying to mkdir("some symlink"),
                // which is an error (but not an escape).
                wantError = true;
            }

            var err = root.MkdirAll(testʗ1.open, 511);
            if (errEndsTest(tΔ1, err, wantError, rootMkdirAllQˢ, testʗ1.open)) {
                return;
            }
            (var fi, err) = Δos.Lstat(target);
            if (err != nil) {
                tΔ1.Fatalf(@"stat file created with Root.MkdirAll(%q): %v", testʗ1.open, err);
            }
            if (!fi.IsDir()) {
                tΔ1.Fatalf(@"stat file created with Root.MkdirAll(%q): not a directory", testʗ1.open);
            }
            var mode = fi.Mode(); if ((fs.FileMode)(mode & 511) == 0) {
                // Issue #73559: We're not going to worry about the exact
                // mode bits (which will have been modified by umask),
                // but there should be mode bits.
                tΔ1.Fatalf(@"stat file created with Root.MkdirAll(%q): mode=%v, want non-zero", testʗ1.open, mode);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootOpenRootQˢ => ref rootOpenRootQˢᵇ;
private static readonly @string rootOpenRootQˢᵇ = "root.OpenRoot(%q)"u8;

public static void TestRootOpenRoot(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            GoFrame ᒐ = default;
            try {
                if (target != ""u8) {
                    var errΔ1 = Δos.Mkdir(target, 511); if (errΔ1 != nil) {
                        tΔ1.Fatal(errΔ1);
                    }
                    var errΔ2 = Δos.WriteFile(target + "/f"u8, default, 438); if (errΔ2 != nil) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
                var (rr, err) = root.OpenRoot(testʗ1.open);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootOpenRootQˢ, testʗ1.open)) {
                    return;
                }
                var rrʗ1 = rr;
                defer(() => rrʗ1.Close(), ref ᒐ);
                (var f, err) = rr.Open("f");
                if (err != nil) {
                    tΔ1.Fatalf(@"root.OpenRoot(%q).Open(""f"") = %v", testʗ1.open, err);
                }
                f.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootRemoveQˢ => ref rootRemoveQˢᵇ;
private static readonly @string rootRemoveQˢᵇ = "root.Remove(%q)"u8;

public static void TestRootRemoveFile(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // Remove doesn't follow symlinks in the final path component,
                // so it will successfully remove ltarget.
                wantError = false;
                target = filepath.Join(root.Name(), testʗ1.ltarget);
            } else if (target != ""u8) {
                var errΔ1 = Δos.WriteFile(target, default, 438); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }

            var err = root.Remove(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootRemoveQˢ, testʗ1.open)) {
                return;
            }
            (_, err) = Δos.Lstat(target);
            if (!errors.Is(err, Δos.ErrNotExist)) {
                tΔ1.Fatalf(@"stat file removed with Root.Remove(%q): %v, want ErrNotExist", testʗ1.open, err);
            }
        });
    }
}

public static void TestRootRemoveDirectory(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // Remove doesn't follow symlinks in the final path component,
                // so it will successfully remove ltarget.
                wantError = false;
                target = filepath.Join(root.Name(), testʗ1.ltarget);
            } else if (target != ""u8) {
                var errΔ1 = Δos.Mkdir(target, 511); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }

            var err = root.Remove(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootRemoveQˢ, testʗ1.open)) {
                return;
            }
            (_, err) = Δos.Lstat(target);
            if (!errors.Is(err, Δos.ErrNotExist)) {
                tΔ1.Fatalf(@"stat file removed with Root.Remove(%q): %v, want ErrNotExist", testʗ1.open, err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootRemoveAllQˢ => ref rootRemoveAllQˢᵇ;
private static readonly @string rootRemoveAllQˢᵇ = "root.RemoveAll(%q)"u8;

public static void TestRootRemoveAll(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (strings.HasSuffix(testʗ1.open, "/")) {
                // The test is removing a file with a trailing /.
                // RemoveAll ignores trailing /s
                // If the file is a symlink, it will remove the symlink.
                @string fullname = filepath.Join(root.Name(), testʗ1.open);
                var (st, errΔ1) = Δos.Lstat(fullname); if (errΔ1 == nil && st.Mode().Type() == fs.ModeSymlink) {
                    testʗ1.ltarget = testʗ1.open;
                }
            }
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // Remove doesn't follow symlinks in the final path component,
                // so it will successfully remove ltarget.
                wantError = false;
                target = filepath.Join(root.Name(), testʗ1.ltarget);
            } else if (target != ""u8) {
                var errΔ2 = Δos.Mkdir(target, 511); if (errΔ2 != nil) {
                    tΔ1.Fatal(errΔ2);
                }
                var errΔ3 = Δos.WriteFile(filepath.Join(target, fileˢ2), default, 438); if (errΔ3 != nil) {
                    tΔ1.Fatal(errΔ3);
                }
            }
            var targetExists = true;
            var (_, errΔ4) = root.Lstat(testʗ1.open); if (errors.Is(errΔ4, Δos.ErrNotExist)) {
                // If the target doesn't exist, RemoveAll succeeds rather
                // than returning ErrNotExist.
                targetExists = false;
                wantError = false;
            }

            var err = root.RemoveAll(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootRemoveAllQˢ, testʗ1.open)) {
                return;
            }
            if (!targetExists) {
                return;
            }
            (_, err) = Δos.Lstat(target);
            if (!errors.Is(err, Δos.ErrNotExist)) {
                tΔ1.Fatalf(@"stat file removed with Root.Remove(%q): %v, want ErrNotExist", testʗ1.open, err);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object osOpenRootFileSucceededˢ = (@string)"os.OpenRoot(file) succeeded; want failure"u8;
internal static readonly object rootOpenRootFileˢ = (@string)"Root.OpenRoot(file) succeeded; want failure"u8;

public static void TestRootOpenFileAsRoot(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = t.TempDir();
        @string target = filepath.Join(dir, targetˢ);
        var errΔ1 = Δos.WriteFile(target, default, 438); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var (r, err) = Δos.OpenRoot(target);
        if (err == nil) {
            r.Close();
            t.Fatal(osOpenRootFileSucceededˢ);
        }
        (r, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        (var rr, err) = r.OpenRoot(targetˢ);
        if (err == nil) {
            rr.Close();
            t.Fatal(rootOpenRootFileˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootStatQˢ => ref rootStatQˢᵇ;
private static readonly @string rootStatQˢᵇ = "root.Stat(%q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentᶜ => ref contentᶜᵇ;
private static readonly @string contentᶜᵇ = "content"u8;

public static void TestRootStat(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string content = contentᶜ;
            if (target != ""u8) {
                var errΔ1 = Δos.WriteFile(target, slice<byte>(content), 438); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }

            var (fi, err) = root.Stat(testʗ1.open);
            if (errEndsTest(tΔ1, err, testʗ1.wantError, rootStatQˢ, testʗ1.open)) {
                return;
            }
            {
                @string got = fi.Name();
                @string want = filepath.Base(testʗ1.open); if (got != want) {
                    tΔ1.Errorf("root.Stat(%q).Name() = %q, want %q", testʗ1.open, got, want);
                }
            }
            {
                var (got, want) = (fi.Size(), (int64)len(content)); if (got != want) {
                    tΔ1.Errorf("root.Stat(%q).Size() = %v, want %v", testʗ1.open, got, want);
                }
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentᶜ1 => ref contentᶜ1ᵇ;
private static readonly @string contentᶜ1ᵇ = "content"u8;

public static void TestRootLstat(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string content = contentᶜ1;
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // Lstat will stat the final link, rather than following it.
                wantError = false;
            } else if (target != ""u8) {
                var errΔ1 = Δos.WriteFile(target, slice<byte>(content), 438); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }

            var (fi, err) = root.Lstat(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootStatQˢ, testʗ1.open)) {
                return;
            }
            {
                @string got = fi.Name();
                @string want = filepath.Base(testʗ1.open); if (got != want) {
                    tΔ1.Errorf("root.Stat(%q).Name() = %q, want %q", testʗ1.open, got, want);
                }
            }
            if (testʗ1.ltarget == ""u8) {
                {
                    var got = fi.Mode(); if ((fs.FileMode)(got & Δos.ModeSymlink) != 0) {
                        tΔ1.Errorf("root.Stat(%q).Mode() = %v, want non-symlink", testʗ1.open, got);
                    }
                }
                {
                    var (got, want) = (fi.Size(), (int64)len(content)); if (got != want) {
                        tΔ1.Errorf("root.Stat(%q).Size() = %v, want %v", testʗ1.open, got, want);
                    }
                }
            } else {
                var got = fi.Mode(); if ((fs.FileMode)(got & Δos.ModeSymlink) == 0) {
                    tΔ1.Errorf("root.Stat(%q).Mode() = %v, want symlink", testʗ1.open, got);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootReadlinkQˢ => ref rootReadlinkQˢᵇ;
private static readonly @string rootReadlinkQˢᵇ = "root.Readlink(%q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string contentᶜ2 => ref contentᶜ2ᵇ;
private static readonly @string contentᶜ2ᵇ = "content"u8;

public static void TestRootReadlink(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string content = contentᶜ2;
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // Readlink will read the final link, rather than following it.
                wantError = false;
            } else {
                // Readlink fails on non-link targets.
                wantError = true;
            }

            var (got, err) = root.Readlink(testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootReadlinkQˢ, testʗ1.open)) {
                return;
            }

            (var want, err) = Δos.Readlink(filepath.Join(root.Name(), testʗ1.ltarget));
            if (err != nil) {
                tΔ1.Fatalf("os.Readlink(%q) = %v, want success", testʗ1.ltarget, err);
            }
            if (got != want) {
                tΔ1.Errorf("root.Readlink(%q) = %q, want %q", testʗ1.open, got, want);
            }
        });
    }
}

// TestRootRenameFrom tests renaming the test case target to a known-good path.
public static void TestRootRenameFrom(ж<Δtesting.T> t) {
    testRootMoveFrom(t, true);
}

// TestRootRenameFrom tests linking the test case target to a known-good path.
public static void TestRootLinkFrom(ж<Δtesting.T> t) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(t));
    testRootMoveFrom(t, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string renameˢ2 => ref renameˢ2ᵇ;
private static readonly @string renameˢ2ᵇ = "Rename"u8;
internal static ref readonly @string linkˢ2 => ref linkˢ2ᵇ;
private static readonly @string linkˢ2ᵇ = "Link"u8;
internal static ref readonly @string rootVQQˢ => ref rootVQQˢᵇ;
private static readonly @string rootVQQˢᵇ = "root.%v(%q, %q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dstPathᶜ => ref dstPathᶜᵇ;
private static readonly @string dstPathᶜᵇ = "destination"u8;

internal static void testRootMoveFrom(ж<Δtesting.T> t, bool rename) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (target != ""u8) {
                var errΔ1 = Δos.WriteFile(target, wantʗ1, 438); if (errΔ1 != nil) {
                    tΔ1.Fatal(errΔ1);
                }
            }
            var wantError = testʗ1.wantError;
            @string linkTarget = default;
            if (testʗ1.ltarget != ""u8) {
                // Rename will rename the link, not the file linked to.
                wantError = false;
                error errΔ2 = default;
                (linkTarget, errΔ2) = root.Readlink(testʗ1.ltarget);
                if (errΔ2 != nil) {
                    tΔ1.Fatalf("root.Readlink(%q) = %v, want success", testʗ1.ltarget, errΔ2);
                }

                // When GOOS=js, creating a hard link to a symlink fails.
                if (!rename && Δruntime.GOOS == "js"u8) {
                    wantError = true;
                }

                // Windows allows creating a hard link to a file symlink,
                // but not to a directory symlink.
                //
                // This uses os.Stat to check the link target, because this
                // is easier than figuring out whether the link itself is a
                // directory link. The link was created with os.Symlink,
                // which creates directory links when the target is a directory,
                // so this is good enough for a test.
                if (!rename && Δruntime.GOOS == "windows"u8) {
                    var (st, errΔ3) = Δos.Stat(filepath.Join(root.Name(), testʗ1.ltarget));
                    if (errΔ3 == nil && st.IsDir()) {
                        wantError = true;
                    }
                }
            }

            @string dstPath = dstPathᶜ;

            // Plan 9 doesn't allow cross-directory renames.
            if (Δruntime.GOOS == "plan9"u8 && strings.Contains(testʗ1.open, "/")) {
                wantError = true;
            }

            @string op = default;
            error err = default;
            if (rename) {
                op = renameˢ2;
                err = root.Rename(testʗ1.open, dstPath);
            } else {
                op = linkˢ2;
                err = root.Link(testʗ1.open, dstPath);
            }
            if (errEndsTest(tΔ1, err, wantError, rootVQQˢ, op, testʗ1.open, dstPath)) {
                return;
            }

            @string origPath = target;
            if (testʗ1.ltarget != ""u8) {
                origPath = filepath.Join(root.Name(), testʗ1.ltarget);
            }
            (_, err) = Δos.Lstat(origPath);
            if (rename) {
                if (!errors.Is(err, Δos.ErrNotExist)) {
                    tΔ1.Errorf("after renaming file, Lstat(%q) = %v, want ErrNotExist", origPath, err);
                }
            } else {
                if (err != nil) {
                    tΔ1.Errorf("after linking file, error accessing original: %v", err);
                }
            }

            @string dstFullPath = filepath.Join(root.Name(), dstPath);
            if (testʗ1.ltarget != ""u8) {
                var (got, errΔ1) = Δos.Readlink(dstFullPath);
                if (errΔ1 != nil || got != linkTarget) {
                    tΔ1.Errorf("os.Readlink(%q) = %q, %v, want %q", dstFullPath, got, errΔ1, linkTarget);
                }
            } else {
                var (got, errΔ2) = Δos.ReadFile(dstFullPath);
                if (errΔ2 != nil || !bytes.Equal(got, wantʗ1)) {
                    tΔ1.Errorf(@"os.ReadFile(%q): read content %q, %v; want %q", dstFullPath, (@string)got, errΔ2, (@string)wantʗ1);
                }
                (var st, errΔ2) = Δos.Lstat(dstFullPath);
                if (errΔ2 != nil || (fs.FileMode)(st.Mode() & fs.ModeSymlink) != 0) {
                    tΔ1.Errorf(@"os.Lstat(%q) = %v, %v; want non-symlink", dstFullPath, st.Mode(), errΔ2);
                }
            }
        });
    }
}

// TestRootRenameTo tests renaming a known-good path to the test case target.
public static void TestRootRenameTo(ж<Δtesting.T> t) {
    testRootMoveTo(t, true);
}

// TestRootLinkTo tests renaming a known-good path to the test case target.
public static void TestRootLinkTo(ж<Δtesting.T> t) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(t));
    testRootMoveTo(t, true);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcPathᶜ => ref srcPathᶜᵇ;
private static readonly @string srcPathᶜᵇ = "source"u8;

internal static void testRootMoveTo(ж<Δtesting.T> t, bool rename) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string srcPath = srcPathᶜ;
            var errΔ1 = Δos.WriteFile(filepath.Join(root.Name(), srcPath), wantʗ1, 438); if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }

            if (Δruntime.GOOS == "windows"u8 && strings.HasSuffix(testʗ1.open, "/")) {
                // Windows will ignore trailing slashes in the rename/link target.
                @string p = strings.TrimSuffix(testʗ1.open, "/");
                var (st, errΔ2) = root.Lstat(p);
                if (errΔ2 == nil && st.Mode().Type() == fs.ModeSymlink) {
                    testʗ1.ltarget = p;
                }
            }

            target = testʗ1.target;
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // Rename will overwrite the final link rather than follow it.
                target = testʗ1.ltarget;
                wantError = false;
            }

            // Plan 9 doesn't allow cross-directory renames.
            if (Δruntime.GOOS == "plan9"u8 && strings.Contains(testʗ1.open, "/")) {
                wantError = true;
            }

            error err = default;
            @string op = default;
            if (rename) {
                op = renameˢ2;
                err = root.Rename(srcPath, testʗ1.open);
            } else {
                op = linkˢ2;
                err = root.Link(srcPath, testʗ1.open);
            }
            if (errEndsTest(tΔ1, err, wantError, rootVQQˢ, op, srcPath, testʗ1.open)) {
                return;
            }

            (_, err) = Δos.Lstat(filepath.Join(root.Name(), srcPath));
            if (rename) {
                if (!errors.Is(err, Δos.ErrNotExist)) {
                    tΔ1.Errorf("after renaming file, Lstat(%q) = %v, want ErrNotExist", srcPath, err);
                }
            } else {
                if (err != nil) {
                    tΔ1.Errorf("after linking file, error accessing original: %v", err);
                }
            }

            (var got, err) = Δos.ReadFile(filepath.Join(root.Name(), target));
            if (err != nil || !bytes.Equal(got, wantʗ1)) {
                tΔ1.Errorf(@"os.ReadFile(%q): read content %q, %v; want %q", target, (@string)got, err, (@string)wantʗ1);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootSymlinkQˢ => ref rootSymlinkQˢᵇ;
private static readonly @string rootSymlinkQˢᵇ = "root.Symlink(%q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantTargetᶜ => ref wantTargetᶜᵇ;
private static readonly @string wantTargetᶜᵇ = "linktarget"u8;

public static void TestRootSymlink(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    foreach (var (_, vᴛ1) in range(rootTestCases)) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(t, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8) {
                // We can't create a symlink over an existing symlink.
                wantError = true;
            }

            @string wantTarget = wantTargetᶜ;
            var err = root.Symlink(wantTarget, testʗ1.open);
            if (errEndsTest(tΔ1, err, wantError, rootSymlinkQˢ, testʗ1.open)) {
                return;
            }
            (var got, err) = Δos.Readlink(target);
            if (err != nil || got != wantTarget) {
                tΔ1.Fatalf("ReadLink(%q) = %q, %v; want %q, nil", target, got, err, wantTarget);
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
    // check is called before the test starts, and may t.Skip if necessary.
    internal Action<ж<Δtesting.T>> check;
}

internal static slice<rootConsistencyTest> rootConsistencyTestCases = slice<rootConsistencyTest>([new(
    name: "file",
    fs: [
        "target"
    ],
    open: "target"
), new(
    name: "dir slash dot",
    fs: [
        "target/file"
    ],
    open: "target/."
), new(
    name: "dot",
    fs: [
        "file"
    ],
    open: "."
), new(
    name: "file slash dot",
    fs: [
        "target"
    ],
    open: "target/.",
    detailedErrorMismatch: (ж<Δtesting.T> t) => {
        // FreeBSD returns EPERM in the non-Root case.
        return Δruntime.GOOS == "freebsd"u8 && strings.HasPrefix(t.Name(), "TestRootConsistencyRemove");
    }
), new(
    name: "dir slash",
    fs: [
        "target/file"
    ],
    open: "target/"
), new(
    name: "dot slash",
    fs: [
        "file"
    ],
    open: "./"
), new(
    name: "file slash",
    fs: [
        "target"
    ],
    open: "target/",
    detailedErrorMismatch: (ж<Δtesting.T> t) => {
        // os.Create returns ENOTDIR or EISDIR depending on the platform.
        return Δruntime.GOOS == "js"u8;
    }
), new(
    name: "file in path",
    fs: [
        "file"
    ],
    open: "file/target"
), new(
    name: "directory in path missing",
    open: "dir/target"
), new(
    name: "target does not exist",
    open: "target"
), new(
    name: "symlink slash",
    fs: [
        "target/file",
        "link => target"
    ],
    open: "link/",
    check: (ж<Δtesting.T> t) => {
        if (Δruntime.GOOS == "linux"u8 && strings.HasPrefix(t.Name(), "TestRootConsistencyRename/")) {
            // Linux does not resolve "symlink" in rename("symlink/", "target").
            t.Skip((@string)"known inconsistency on linux");
        }
        if (strings.HasPrefix(t.Name(), "TestRootConsistencyRemoveAll/")) {
            // Root.RemoveAll and os.RemoveAll are not always consistent here.
            t.Skip((@string)"known inconsistency in RemoveAll");
        }
    }
), new(
    name: "symlink slash dot",
    fs: [
        "target/file",
        "link => target"
    ],
    open: "link/."
), new(
    name: "unresolved symlink",
    fs: [
        "link => target"
    ],
    open: "link"
), new(
    name: "resolved symlink",
    fs: [
        "link => target",
        "target"
    ],
    open: "link"
), new(
    name: "dotdot in path after symlink",
    fs: [
        "a => b/c",
        "b/c/",
        "b/target"
    ],
    open: "a/../target"
), new(
    name: "symlink to dir ends in slash",
    fs: [
        "dir/",
        "link => dir/"
    ],
    open: "link"
), new(
    name: "symlink to file ends in slash",
    fs: [
        "file",
        "link => file/"
    ],
    open: "link"
), new(
    name: "long file name",
    open: strings.Repeat("a", 500)
), new(
    name: "unreadable directory",
    fs: [
        "dir/target"
    ],
    fsFunc: (ж<Δtesting.T> t, @string dir) => {
        Δos.Chmod(filepath.Join(dir, "dir"), 0);
        t.Cleanup(() => {
            Δos.Chmod(filepath.Join(dir, "dir"), 448);
        });
        return dir;
    },
    open: "dir/target"
), new(
    name: "unix domain socket target",
    fsFunc: (ж<Δtesting.T> t, @string dir) => tempDirWithUnixSocket(t, "a"),
    open: "a"
), new(
    name: "unix domain socket in path",
    fsFunc: (ж<Δtesting.T> t, @string dir) => tempDirWithUnixSocket(t, "a"),
    open: "a/b",
    detailedErrorMismatch: (ж<Δtesting.T> t) => {
        // On Windows, os.Root.Open returns "The directory name is invalid."
        // and os.Open returns "The file cannot be accessed by the system.".
        return Δruntime.GOOS == "windows"u8;
    },
    check: (ж<Δtesting.T> t) => {
        if (strings.HasPrefix(t.Name(), "TestRootConsistencyRemoveAll/")) {
            var exprᴛ1 = Δruntime.GOOS;
            switch (ᐧ) {
            case {} when exprᴛ1 == "windows"u8: {
                // Root.RemoveAll notices that a/ is not a directory,
                // and returns success.
                // os.RemoveAll tries to open a/ and fails because
                // it is not a regular file.
                // The inconsistency here isn't worth fixing, so just skip this test.
                t.Skip((@string)"known inconsistency on windows");
                break;
            }
            case {} when exprᴛ1 == "js"u8: {
                // GOOS=js behavior varies with what the underlying OS is.
                t.Skip((@string)"known inconsistency with GOOS=js");
                break;
            }}
        }
    }
), new(
    name: "question mark",
    open: "?"
), new(
    name: "nul byte",
    open: "\x00"
)
]);

internal static @string tempDirWithUnixSocket(ж<Δtesting.T> t, @string name) {
    var (dir, err) = Δos.MkdirTemp("", "");
    if (err != nil) {
        t.Fatal(err);
    }
    t.Cleanup(() => {
        var errΔ1 = Δos.RemoveAll(dir); if (errΔ1 != nil) {
            t.Error(errΔ1);
        }
    });
    (var addr, err) = Δnet.ResolveUnixAddr("unix", filepath.Join(dir, name));
    if (err != nil) {
        t.Skipf("net.ResolveUnixAddr: %v", err);
    }
    (var conn, err) = Δnet.ListenUnix("unix", addr);
    if (err != nil) {
        t.Skipf("net.ListenUnix: %v", err);
    }
    var connʗ1 = conn;
    t.Cleanup(() => {
        connʗ1.Close();
    });
    return dir;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object inconsistentResultsOnˢ = (@string)"#69509: inconsistent results on wasip1"u8;
internal static ref readonly @string withRootˢ => ref withRootˢᵇ;
private static readonly @string withRootˢᵇ = "with root"u8;
internal static ref readonly @string withoutRootˢ => ref withoutRootˢᵇ;
private static readonly @string withoutRootˢᵇ = "without root"u8;

internal static void run(this rootConsistencyTest test, ж<Δtesting.T> t, Func<ж<Δtesting.T>, @string, ж<Δos.Root>, (@string, error)> f) {
    if (Δruntime.GOOS == "wasip1"u8) {
        // On wasip, non-Root functions clean paths before opening them,
        // resulting in inconsistent behavior.
        // https://go.dev/issue/69509
        t.Skip(inconsistentResultsOnˢ);
    }

    var testʗ1 = test;
    t.Run(test.name, (ж<Δtesting.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            if (testʗ1.check != nil) {
                testʗ1.check(tΔ1);
            }

            @string dir1 = makefs(tΔ1, testʗ1.fs);
            @string dir2 = makefs(tΔ1, testʗ1.fs);
            if (testʗ1.fsFunc != nil) {
                dir1 = testʗ1.fsFunc(tΔ1, dir1);
                dir2 = testʗ1.fsFunc(tΔ1, dir2);
            }

            var (r, err) = Δos.OpenRoot(dir1);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);

            var (res1, err1) = f(tΔ1, testʗ1.open, r);
            var (res2, err2) = f(tΔ1, dir2 + "/"u8 + testʗ1.open, nil);

            if (res1 != res2 || ((err1 == nil) != (err2 == nil))) {
                tΔ1.Errorf("with root:    res=%v", res1);
                tΔ1.Errorf("              err=%v", err1);
                tΔ1.Errorf("without root: res=%v", res2);
                tΔ1.Errorf("              err=%v", err2);
                tΔ1.Errorf("want consistent results, got mismatch");
            }

            if (err1 != nil || err2 != nil) {
                var err1ʗ1 = err1;
                error underlyingError(@string how, error errΔ1) {
                    switch (err1ʗ1.type()) {
                    case ж<osꓸPathError> e: {
                        return e.Err;
                    }
                    case ж<Δos.LinkError> e: {
                        return e.Err;
                    }
                    default: {
                        var e = err1ʗ1;
                        tΔ1.Fatalf("%v, expected PathError or LinkError; got: %v", how, errΔ1);
                        break;
                    }}
                    return default;
                }
                var e1 = underlyingError(withRootˢ, err1);
                var e2 = underlyingError(withoutRootˢ, err1);
                var detailedErrorMismatch = false;
                var fΔ1 = testʗ1.detailedErrorMismatch; if (fΔ1 != nil) {
                    detailedErrorMismatch = fΔ1(tΔ1);
                }
                if (Δruntime.GOOS == "plan9"u8) {
                    // Plan9 syscall errors aren't comparable.
                    detailedErrorMismatch = true;
                }
                if (!detailedErrorMismatch && !AreEqual(e1, e2)) {
                    tΔ1.Errorf("with root:    err=%v", e1);
                    tΔ1.Errorf("without root: err=%v", e2);
                    tΔ1.Errorf("want consistent results, got mismatch");
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

public static void TestRootConsistencyOpen(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            GoFrame ᒐ = default;
            try {
                ж<Δos.File> f = default;
                error err = default;
                if (r == nil) {
                    (f, err) = Δos.Open(path);
                } else {
                    (f, err) = r.Open(path);
                }
                if (err != nil) {
                    return ("", err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                (var fi, err) = f.Stat();
                if (err == nil && !fi.IsDir()) {
                    var (b, errΔ1) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    return ((@string)b, errΔ1);
                } else {
                    var (names, errΔ2) = f.Readdirnames(-1);
                    slices.Sort<slice<@string>, @string>(names);
                    return (fmt.Sprintf("%q"u8, names), errΔ2);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestRootConsistencyCreate(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            ж<Δos.File> f = default;
            error err = default;
            if (r == nil) {
                (f, err) = Δos.Create(path);
            } else {
                (f, err) = r.Create(path);
            }
            if (err == nil) {
                f.Write(slice<byte>("file contents"u8));
                f.Close();
            }
            return ("", err);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chmod0o555ˢ => ref chmod0o555ˢᵇ;
private static readonly @string chmod0o555ˢᵇ = "chmod 0o555"u8;
internal static ref readonly @string chmod0o777ˢ => ref chmod0o777ˢᵇ;
private static readonly @string chmod0o777ˢᵇ = "chmod 0o777"u8;

public static void TestRootConsistencyChmod(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "wasip1"u8) {
        t.Skip("Chmod not supported on " + Δruntime.GOOS);
    }
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            Func<@string, fs.FileMode, error> chmod = Δos.Chmod;
            Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
            if (r != nil) {
                chmod = (@string p1, fs.FileMode p2) => r.Chmod(p1, p2);
                lstat = (@string p1) => r.Lstat(p1);
            }

            fs.FileMode m1 = default;
            fs.FileMode m2 = default;
            var errΔ1 = chmod(path, 365); if (errΔ1 != nil) {
                return (chmod0o555ˢ, errΔ1);
            }
            var (fi, err) = lstat(path);
            if (err == nil) {
                m1 = fi.Mode();
            }
            err = chmod(path, 511); if (err != nil) {
                return (chmod0o777ˢ, err);
            }
            (fi, err) = lstat(path);
            if (err == nil) {
                m2 = fi.Mode();
            }
            return (fmt.Sprintf("%v %v"u8, m1, m2), err);
        });
    }
}

public static void TestRootConsistencyMkdir(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default;
            if (r == nil) {
                err = Δos.Mkdir(path, 511);
            } else {
                err = r.Mkdir(path, 511);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyMkdirAll(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default;
            if (r == nil) {
                err = Δos.MkdirAll(path, 511);
            } else {
                err = r.MkdirAll(path, 511);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyRemove(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        if (test.open == "."u8 || test.open == "./"u8) {
            continue; // can't remove the root itself
        }
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default;
            if (r == nil) {
                err = Δos.Remove(path);
            } else {
                err = r.Remove(path);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyRemoveAll(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        if (test.open == "."u8 || test.open == "./"u8) {
            continue; // can't remove the root itself
        }
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default;
            if (r == nil) {
                err = Δos.RemoveAll(path);
            } else {
                err = r.RemoveAll(path);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyStat(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            fs.FileInfo fi = default;
            error err = default;
            if (r == nil) {
                (fi, err) = Δos.Stat(path);
            } else {
                (fi, err) = r.Stat(path);
            }
            if (err != nil) {
                return ("", err);
            }
            return (fmt.Sprintf("name:%q size:%v mode:%v isdir:%v"u8, fi.Name(), fi.Size(), fi.Mode(), fi.IsDir()), default);
        });
    }
}

public static void TestRootConsistencyLstat(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            fs.FileInfo fi = default;
            error err = default;
            if (r == nil) {
                (fi, err) = Δos.Lstat(path);
            } else {
                (fi, err) = r.Lstat(path);
            }
            if (err != nil) {
                return ("", err);
            }
            return (fmt.Sprintf("name:%q size:%v mode:%v isdir:%v"u8, fi.Name(), fi.Size(), fi.Mode(), fi.IsDir()), default);
        });
    }
}

public static void TestRootConsistencyReadlink(ж<Δtesting.T> t) {
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            if (r == nil) {
                return Δos.Readlink(path);
            } else {
                return r.Readlink(path);
            }
        });
    }
}

public static void TestRootConsistencyRename(ж<Δtesting.T> t) {
    testRootConsistencyMove(t, true);
}

public static void TestRootConsistencyLink(ж<Δtesting.T> t) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(t));
    testRootConsistencyMove(t, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object plan9DoesNotSupportCrossˢ = (@string)"Plan 9 does not support cross-directory renames"u8;
internal static ref readonly @string otherˢ => ref otherˢᵇ;
private static readonly @string otherˢᵇ = "other"u8;
internal static ref readonly @string statErrorˢ => ref statErrorˢᵇ;
private static readonly @string statErrorˢᵇ = "stat error"u8;

internal static void testRootConsistencyMove(ж<Δtesting.T> t, bool rename) {
    if (Δruntime.GOOS == "plan9"u8) {
        // This test depends on moving files between directories.
        t.Skip(plan9DoesNotSupportCrossˢ);
    }
    // Run this test in two directions:
    // Renaming the test path to a known-good path (from),
    // and renaming a known-good path to the test path (to).
    foreach (var (_, name) in range(slice<@string>(["from", "to"]))) {
        t.Run(name, (ж<Δtesting.T> tΔ1) => {
            foreach (var (_, test) in range(rootConsistencyTestCases)) {
                if (Δruntime.GOOS == "windows"u8) {
                    // On Windows, Rename("/path/to/.", x) succeeds,
                    // because Windows cleans the path to just "/path/to".
                    // Root.Rename(".", x) fails as expected.
                    // Don't run this consistency test on Windows.
                    if (test.open == "."u8 || test.open == "./"u8) {
                        continue;
                    }
                }

                test.run(tΔ1, (ж<Δtesting.T> tΔ2, @string path, ж<Δos.Root> r) => {
                    Func<@string, @string, error> move = default;
                    switch (ᐧ) {
                    case {} when rename && r == nil: {
                        move = Δos.Rename;
                        break;
                    }
                    case {} when rename && r != nil: {
                        move = (@string p1, @string p2) => r.Rename(p1, p2);
                        break;
                    }
                    case {} when !rename && r == nil: {
                        move = Δos.Link;
                        break;
                    }
                    case {} when !rename && r != nil: {
                        move = (@string p1, @string p2) => r.Link(p1, p2);
                        break;
                    }}
                    Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
                    if (r != nil) {
                        lstat = (@string p1) => r.Lstat(p1);
                    }

                    @string otherPath = otherˢ;
                    if (r == nil) {
                        otherPath = filepath.Join(tΔ2.TempDir(), otherPath);
                    }

                    @string srcPath = default;
                    @string dstPath = default;
                    if (name == "from"u8) {
                        srcPath = path;
                        dstPath = otherPath;
                    } else {
                        srcPath = otherPath;
                        dstPath = path;
                    }

                    if (!rename) {
                        // When the source is a symlink, Root.Link creates
                        // a hard link to the symlink.
                        // os.Link does whatever the link syscall does,
                        // which varies between operating systems and
                        // their versions.
                        // Skip running the consistency test when
                        // the source is a symlink.
                        var (fiΔ1, errΔ1) = lstat(srcPath);
                        if (errΔ1 == nil && (fs.FileMode)(fiΔ1.Mode() & Δos.ModeSymlink) != 0) {
                            return ("", default);
                        }
                    }

                    var errΔ2 = move(srcPath, dstPath); if (errΔ2 != nil) {
                        return ("", errΔ2);
                    }
                    var (fi, err) = lstat(dstPath);
                    if (err != nil) {
                        tΔ2.Errorf("stat(%q) after successful copy: %v", dstPath, err);
                        return (statErrorˢ, err);
                    }
                    return (fmt.Sprintf("name:%q size:%v mode:%v isdir:%v"u8, fi.Name(), fi.Size(), fi.Mode(), fi.IsDir()), default);
                });
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string targetᶜ1 => ref targetᶜ1ᵇ;
private static readonly @string targetᶜ1ᵇ = "linktarget"u8;

public static void TestRootConsistencySymlink(ж<Δtesting.T> t) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(t));
    foreach (var (_, test) in range(rootConsistencyTestCases)) {
        test.run(t, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            @string target = targetᶜ1;
            error err = default;
            @string got = default;
            if (r == nil) {
                err = Δos.Symlink(target, path);
                (got, _) = Δos.Readlink(target);
            } else {
                err = r.Symlink(target, path);
                (got, _) = r.Readlink(target);
            }
            return (got, err);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gowasiruntimeˢ => ref gowasiruntimeˢᵇ;
private static readonly @string gowasiruntimeˢᵇ = "GOWASIRUNTIME"u8;
internal static readonly object wazeroDoesNotTrackˢ = (@string)"wazero does not track renamed directories"u8;

public static void TestRootRenameAfterOpen(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "windows"u8: {
            t.Skip("renaming open files not supported on " + Δruntime.GOOS);
            break;
        }
        case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8: {
            t.Skip("openat not supported on " + Δruntime.GOOS);
            break;
        }
        case {} when exprᴛ1 == "wasip1"u8: {
            if (Δos.Getenv(gowasiruntimeˢ) == "wazero"u8) {
                t.Skip(wazeroDoesNotTrackˢ);
            }
            break;
        }}

        @string dir = t.TempDir();

        // Create directory "a" and open it.
        var errΔ1 = Δos.Mkdir(filepath.Join(dir, "a"), 511); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var (dirf, err) = Δos.OpenRoot(filepath.Join(dir, "a"));
        if (err != nil) {
            t.Fatal(err);
        }
        var dirfʗ1 = dirf;
        defer(() => dirfʗ1.Close(), ref ᒐ);

        // Rename "a" => "b", and create "b/f".
        var errΔ2 = Δos.Rename(filepath.Join(dir, "a"), filepath.Join(dir, "b")); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
        var errΔ3 = Δos.WriteFile(filepath.Join(dir, "b/f"), slice<byte>("hello"u8), 438); if (errΔ3 != nil) {
            t.Fatal(errΔ3);
        }

        // Open "f", and confirm that we see it.
        (var f, err) = dirf.OpenFile("f", Δos.O_RDONLY, 0);
        if (err != nil) {
            t.Fatalf("reading file after renaming parent: %v", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var b, err) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
        if (err != nil) {
            t.Fatal(err);
        }
        {
            @string got = (@string)b;
            @string want = helloˢ; if (got != want) {
                t.Fatalf("file contents: %q, want %q", got, want);
            }
        }

        // f.Name reflects the original path we opened the directory under (".../a"), not "b".
        {
            @string got = f.Name();
            @string want = dirf.Name() + ((@string)(rune)Δos.PathSeparator) + "f"u8; if (got != want) {
                t.Errorf("f.Name() = %q, want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRootNonPermissionMode(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var (r, err) = Δos.OpenRoot(t.TempDir());
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var (_, errΔ1) = r.OpenFile(fileˢ2, (nint)(Δos.O_RDWR | Δos.O_CREATE), 1023); if (errΔ1 == nil) {
            t.Errorf("r.OpenFile(file, O_RDWR|O_CREATE, 0o1777) succeeded; want error");
        }
        var errΔ2 = r.Mkdir(fileˢ2, 1023); if (errΔ2 == nil) {
            t.Errorf("r.Mkdir(file, 0o1777) succeeded; want error");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestRootUseAfterClose_type /*dyn*/ {
    internal @string name;
    internal Func<ж<Δos.Root>, @string, error> f;
}

public static void TestRootUseAfterClose(ж<Δtesting.T> t) {
    var (r, err) = Δos.OpenRoot(t.TempDir());
    if (err != nil) {
        t.Fatal(err);
    }
    r.Close();
    foreach (var (_, test) in range(slice<TestRootUseAfterClose_type>([new(
        name: "Open",
        f: (ж<Δos.Root> rΔ1, @string filename) => {
            var (_, errΔ1) = rΔ1.Open(filename);
            return errΔ1;
        }
    ), new(
        name: "Create",
        f: (ж<Δos.Root> rΔ2, @string filename) => {
            var (_, errΔ2) = rΔ2.Create(filename);
            return errΔ2;
        }
    ), new(
        name: "OpenFile",
        f: (ж<Δos.Root> rΔ3, @string filename) => {
            var (_, errΔ3) = rΔ3.OpenFile(filename, Δos.O_RDWR, 438);
            return errΔ3;
        }
    ), new(
        name: "OpenRoot",
        f: (ж<Δos.Root> rΔ4, @string filename) => {
            var (_, errΔ4) = rΔ4.OpenRoot(filename);
            return errΔ4;
        }
    ), new(
        name: "Mkdir",
        f: (ж<Δos.Root> rΔ5, @string filename) => rΔ5.Mkdir(filename, 511)
    )
    ]))) {
        var errΔ5 = test.f(r, targetˢ);
        var (pe, ok) = errΔ5._<ж<osꓸPathError>>(ᐧ);
        if (!ok || pe.Path != "target"u8 || !AreEqual(pe.Err, Δos.ErrClosed)) {
            t.Errorf(@"r.%v = %v; want &PathError{Path: ""target"", Err: ErrClosed}", test.name, errΔ5);
        }
    }
}

public static partial void TestRootConcurrentClose(ж<Δtesting.T> t) {
    var (r, err) = Δos.OpenRoot(t.TempDir());
    if (err != nil) {
        t.Fatal(err);
    }
    var ch = new channel<error>(1);
    var chʗ1 = ch;
    var rʗ1 = r;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(ᴛ1 => close(ᴛ1), chʗ1, ref ᒐ);
            var first = true;
            for (;;) {
                var (f, errΔ1) = rʗ1.OpenFile(fileˢ2, (nint)(Δos.O_RDWR | Δos.O_CREATE), 438);
                if (errΔ1 != nil) {
                    chʗ1.ᐸꟷ(errΔ1);
                    return;
                }
                if (first) {
                    chʗ1.ᐸꟷ(default);
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
    var errΔ2 = ᐸꟷ(ch); if (errΔ2 != nil) {
        t.Errorf("OpenFile: %v, want success", errΔ2);
    }
    r.Close();
    var errΔ3 = ᐸꟷ(ch); if (!errors.Is(errΔ3, Δos.ErrClosed)) {
        t.Errorf("OpenFile: %v, want ErrClosed", errΔ3);
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
public static partial void TestRootRaceRenameDir(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = t.TempDir();
        var (r, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);

        const nint depth = 4;

        Δos.MkdirAll(dir + "/base/"u8 + strings.Repeat("/a", depth), 511);

        @string path = "base/"u8 + strings.Repeat("a/", depth) + strings.Repeat("../", depth) + "a/f"u8;
        Δos.WriteFile(dir + "/f"u8, slice<byte>("secret"u8), 438);
        Δos.WriteFile(dir + "/base/a/f"u8, slice<byte>("public"u8), 438);

        // Compute how long it takes to open the path in the common case.
        UntypedInt tries = 10;
        time.Duration total = default;
        foreach (var _ᴛ1 in range(tries)) {
            var start = time.Now();
            var (f, errΔ1) = r.Open(path);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            (var b, errΔ1) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            if (((sstring)b) != "public"u8) {
                t.Fatalf("read %q, want %q", b, publicˢ);
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
                    if (errΔ2 != nil) {
                        gotcʗ1.ᐸꟷ(default);
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
            var errΔ3 = Δos.Rename(dir + "/base/a"u8, dir + "/b"u8); if (errΔ3 != nil) {
                // Windows and Plan9 won't let us rename a directory if we have
                // an open handle for it, so an error here is expected.
                var exprᴛ1 = Δruntime.GOOS;
                switch (ᐧ) {
                case {} when exprᴛ1 == "windows"u8 || exprᴛ1 == "plan9"u8: {
                    break;
                }
                default: {
                    t.Fatal(errΔ3);
                    break;
                }}
            }

            var got = ᐸꟷ(gotc);
            Δos.Rename(dir + "/b"u8, dir + "/base/a"u8);
            if (len(got) > 0 && ((sstring)got) != "public"u8) {
                t.Errorf("read file: %q; want error or 'public'", got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dDNewˢ => ref dDNewˢᵇ;
private static readonly @string dDNewˢᵇ = "d/d/new"u8;

public static void TestRootSymlinkToRoot(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = makefs(t, [
            "d/d => .."
        ]);
        var (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        var errΔ1 = root.Mkdir(dDNewˢ, 511); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        (var f, err) = root.Open("d/d");
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var names, err) = f.Readdirnames(-1);
        if (err != nil) {
            t.Fatal(err);
        }
        slices.Sort<slice<@string>, @string>(names);
        var (got, want) = (names, slice<@string>(["d", "new"])); if (!slices.Equal<slice<@string>, @string>(got, want)) {
            t.Errorf("root contains: %q, want %q", got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestOpenInRoot(ж<Δtesting.T> t) {
    @string dir = makefs(t, [
        "file",
        "link => ../ROOT/file"
    ]);
    var (f, err) = Δos.OpenInRoot(dir, fileˢ2);
    if (err != nil) {
        t.Fatalf("OpenInRoot(`file`) = %v, want success", err);
    }
    f.Close();
    foreach (var (_, name) in range(slice<@string>([
        "link",
        "../ROOT/file",
        dir + "/file"u8
    ]))) {
        var (fΔ1, errΔ1) = Δos.OpenInRoot(dir, name);
        if (errΔ1 == nil) {
            fΔ1.Close();
            t.Fatalf("OpenInRoot(%q) = nil, want error", name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rootRemoveAllRemovedTheˢ = (@string)@"root.Remove(All)?(""."") removed the root"u8;

public static void TestRootRemoveDot(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = t.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        var errΔ1 = root.Remove("."); if (errΔ1 == nil) {
            t.Errorf(@"root.Remove(""."") = %v, want error", errΔ1);
        }
        var errΔ2 = root.RemoveAll("."); if (errΔ2 == nil) {
            t.Errorf(@"root.RemoveAll(""."") = %v, want error", errΔ2);
        }
        var (_, errΔ3) = Δos.Stat(dir); if (errΔ3 != nil) {
            t.Error(rootRemoveAllRemovedTheˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string filenameˢ => ref filenameˢᵇ;
private static readonly @string filenameˢᵇ = "filename"u8;

public static void TestRootWriteReadFile(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = t.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        @string name = filenameˢ;
        var want = slice<byte>("file contents"u8);
        var errΔ1 = root.WriteFile(name, want, 438); if (errΔ1 != nil) {
            t.Fatalf("root.WriteFile(%q, %q, 0o666) = %v; want nil", name, want, errΔ1);
        }

        (var got, err) = root.ReadFile(name);
        if (err != nil) {
            t.Fatalf("root.ReadFile(%q) = %q, %v; want %q, nil", name, got, err, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRootName(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string dir = t.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        {
            @string got = root.Name();
            @string want = dir; if (got != want) {
                t.Errorf("root.Name() = %q, want %q", got, want);
            }
        }

        (var f, err) = root.Create(fileˢ2);
        if (err != nil) {
            t.Fatal(err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            @string got = f.Name();
            @string want = filepath.Join(dir, fileˢ2); if (got != want) {
                t.Errorf(@"root.Create(""file"").Name() = %q, want %q", got, want);
            }
        }

        var errΔ1 = root.Mkdir(dirˢ, 511); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        (var subroot, err) = root.OpenRoot(dirˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var subrootʗ1 = subroot;
        defer(() => subrootʗ1.Close(), ref ᒐ);
        {
            @string got = subroot.Name();
            @string want = filepath.Join(dir, dirˢ); if (got != want) {
                t.Errorf(@"root.OpenRoot(""dir"").Name() = %q, want %q", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wasmLacksFstatatˢ = (@string)"wasm lacks fstatat"u8;
internal static ref readonly @string readDirEntryˢ => ref readDirEntryˢᵇ;
private static readonly @string readDirEntryˢᵇ = "ReadDirEntry"u8;
internal static ref readonly @string readFileInfoˢ => ref readFileInfoˢᵇ;
private static readonly @string readFileInfoˢᵇ = "ReadFileInfo"u8;
internal static ref readonly @string readdirnamesˢ => ref readdirnamesˢᵇ;
private static readonly @string readdirnamesˢᵇ = "Readdirnames"u8;

// TestRootNoLstat verifies that we do not use lstat (possibly escaping the root)
// when reading directories in a Root.
public static void TestRootNoLstat(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH == "wasm"u8) {
            t.Skip(wasmLacksFstatatˢ);
        }

        @string dir = makefs(t, [
            "subdir/"
        ]);
        UntypedInt size = 42;
        @string contents = strings.Repeat("x", size);
        var errΔ1 = Δos.WriteFile(dir + "/subdir/file"u8, slice<byte>(contents), 438); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        var (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        var rootʗ2 = root;
        void test(@string name, Action<ж<Δtesting.T>, ж<Δos.File>> fn) {
            var rootʗ3 = rootʗ2;
            t.Run(name, (ж<Δtesting.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    os_internal_test_package.SetStatHook(new os_test_package.testing_Tжcleanuper(tΔ1), (ж<Δos.File> fΔ1, @string nameΔ1) => {
                        if (fΔ1 == nil) {
                            tΔ1.Errorf("unexpected Lstat(%q)", nameΔ1);
                        }
                        return (default, default);
                    });
                    var (f, errΔ2) = rootʗ3.Open(subdirˢ);
                    if (errΔ2 != nil) {
                        tΔ1.Fatal(errΔ2);
                    }
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    fn(tΔ1, f);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }

        void checkFileInfo(ж<Δtesting.T> tΔ2, fs.FileInfo fi) {
            tΔ2.Helper();
            {
                @string got = fi.Name();
                @string want = fileˢ2; if (got != want) {
                    tΔ2.Errorf("FileInfo.Name() = %q, want %q", got, want);
                }
            }
            {
                var (got, want) = (fi.Size(), (int64)size); if (got != want) {
                    tΔ2.Errorf("FileInfo.Size() = %v, want %v", got, want);
                }
            }
        }
        var checkFileInfoʗ1 = checkFileInfo;
        [MethodImpl(MethodImplOptions.NoInlining)] void checkDirEntry(ж<Δtesting.T> tΔ3, fs.DirEntry d) {
            tΔ3.Helper();
            {
                @string got = d.Name();
                @string want = fileˢ2; if (got != want) {
                    tΔ3.Errorf("DirEntry.Name() = %q, want %q", got, want);
                }
            }
            {
                var (got, want) = (d.IsDir(), false); if (got != want) {
                    tΔ3.Errorf("DirEntry.IsDir() = %v, want %v", got, want);
                }
            }
            var (fi, errΔ3) = d.Info();
            if (errΔ3 != nil) {
                tΔ3.Fatalf("DirEntry.Info() = _, %v", errΔ3);
            }
            checkFileInfoʗ1(tΔ3, fi);
        }

        test(statˢ, (ж<Δtesting.T> tΔ4, ж<Δos.File> subdir) => {
            var (fi, errΔ4) = subdir.Stat();
            if (errΔ4 != nil) {
                tΔ4.Fatal(errΔ4);
            }
            if (!fi.IsDir()) {
                tΔ4.Fatalf(@"Open(""subdir"").Stat().IsDir() = false, want true");
            }
        });
        // File.ReadDir, returning []DirEntry
        var checkDirEntryʗ1 = checkDirEntry;
        test(readDirEntryˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ5, ж<Δos.File> subdir) => {
            var (dirents, errΔ5) = subdir.ReadDir(-1);
            if (errΔ5 != nil) {
                tΔ5.Fatal(errΔ5);
            }
            if (len(dirents) != 1) {
                tΔ5.Fatalf(@"Open(""subdir"").ReadDir(-1) = {%v}, want {file}", dirents);
            }
            checkDirEntryʗ1(tΔ5, dirents[0]);
        });
        // File.Readdir, returning []FileInfo
        var checkFileInfoʗ2 = checkFileInfo;
        test(readFileInfoˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ6, ж<Δos.File> subdir) => {
            var (fileinfos, errΔ6) = subdir.Readdir(-1);
            if (errΔ6 != nil) {
                tΔ6.Fatal(errΔ6);
            }
            if (len(fileinfos) != 1) {
                tΔ6.Fatalf(@"Open(""subdir"").Readdir(-1) = {%v}, want {file}", fileinfos);
            }
            checkFileInfoʗ2(tΔ6, fileinfos[0]);
        });
        // File.Readdirnames, returning []string
        test(readdirnamesˢ, (ж<Δtesting.T> tΔ7, ж<Δos.File> subdir) => {
            var (names, errΔ7) = subdir.Readdirnames(-1);
            if (errΔ7 != nil) {
                tΔ7.Fatal(errΔ7);
            }
            var (got, want) = (names, slice<@string>(["file"])); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                tΔ7.Fatalf(@"Open(""subdir"").Readdirnames(-1) = %q, want %q", got, want);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// A rootMultiTest is state for testing an os.Root operation in one configuration among many.
// Each execution of a rootMultiTest varies in several ways:
//
//   - With or without an *os.Root, to check consistency between root/non-root operations.
//   - With a target that may be a file, directory, symlink, or entirely absent.
//   - With various paths referencing the target: "target", "DIR/../target", etc.
//   - When the target is a symlink, with various link target paths.
//
// For example, a single test execution might be:
// In an *os.Root, copy "source" to "DIR/../target".
// "source" is a file, and "target" is a symlink to "../ROOT/s_target". "s_target" is a directory.
// (In this case, we expect the test to fail due to the path escape in the symlink.)
partial struct rootMultiTest {
    // dir is the directory containing the test.
    // dir will always contain a directory named "ROOT"
    // and a subdir named "ROOT/DIR".
    internal @string dir;
    // root is the *Root for the test. May be nil.
    internal ж<Δos.Root> root;
    // source and target are files acted on by the test.
    // target is always set; source is only set for tests which request two files.
    internal testFileDesc source;
    internal testFileDesc target;
    // sourcePath and targetPath are the paths which should be used to acceess
    // the source/target.
    internal @string sourcePath;
    internal @string targetPath;
    internal fs.FileInfo sourceInfo;
    internal fs.FileInfo targetInfo;
    // op is the operation being performed, used for reporting errors.
    internal @string op;
}

internal static ж<bool> testVerbose = flag.Bool("verbose", false, "verbose");

// A rootMultiTest function may return this error to disable
// the check that in-root and out-of-root functions have the same outcome.
internal static error errSkipRootConsistencyCheck = errors.New("skip root consistency check");

// runRootMultiTest runs f in a variety of configurations.
// See above.
internal static void runRootMultiTest(ж<Δtesting.T> t, Func<ж<Δtesting.T>, ж<rootMultiTest>, (@string, error)> f) {
    foreach (var iᴛ1 in range<testFileDesc>(allTestFileDescs().Invoke)) {
        ref var target = ref heap(new testFileDesc(), out var Ꮡtarget);
        target = iᴛ1;

        var targetʗ1 = target;
        t.Run(target.String(), (ж<Δtesting.T> tΔ1) => {
            testFileDesc source = default; // unused
            runRootMultiTestDescs(tΔ1, source, targetʗ1, f);
        });
    }
}

// runRootMultiTest2 runs f in a variety of configurations,
// with both source and target files.
// See above.
internal static void runRootMultiTest2(ж<Δtesting.T> t, Func<ж<Δtesting.T>, ж<rootMultiTest>, (@string, error)> f) {
    // A "simple" desc is one which contains only direct references.
    // When not running the comprehensive (but slow) set of test variations,
    // we only test variations where at least one of source and target is simple.
    bool isSimple(testFileDesc desc) {
        if (desc.@ref.template != "BASE"u8) {
            return false;
        }
        if (desc.kind.isLink() && desc.target.@ref.template != "BASE"u8) {
            return false;
        }
        return true;
    }
    foreach (var iᴛ1 in range<testFileDesc>(allTestFileDescs().Invoke)) {
        ref var source = ref heap(new testFileDesc(), out var Ꮡsource);
        source = iᴛ1;

        foreach (var iᴛ2 in range<testFileDesc>(allTestFileDescs().Invoke)) {
            ref var target = ref heap(new testFileDesc(), out var Ꮡtarget);
            target = iᴛ2;

            if (!rootComprehensive.Value && !isSimple(source) && !isSimple(target)) {
                continue;
            }
            @string name = fmt.Sprintf("%s_to_%s"u8, source, target);
            var sourceʗ1 = source;
            var targetʗ1 = target;
            t.Run(name, (ж<Δtesting.T> tΔ1) => {
                runRootMultiTestDescs(tΔ1, sourceʗ1, targetʗ1, f);
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rootˢ2 => ref rootˢ2ᵇ;
private static readonly @string rootˢ2ᵇ = "root."u8;

// setOp sets the operation performed by the test (logged in errors).
//
// This currently assumes the operation will be a method of os.Root and a function in os
// (e.g., root.Open/os.Open).
internal static void setOp(this ref rootMultiTest test, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    if (test.root != nil) {
        test.op = rootˢ2;
    } else {
        test.op = "os.";
    }
    test.op += fmt.Sprintf(format, a.ꓸꓸꓸ);
}

internal static error errAny = errors.New("any error");

internal static partial void errorf(this ref rootMultiTest test, ж<Δtesting.T> t, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Errorf("%v:", test.op);
    t.Fatalf("  "u8 + format, args.ꓸꓸꓸ);
}

// wantError tests whether got matches want.
// If want is errAny, got may be any non-nil error.
internal static void wantError(this ref rootMultiTest test, ж<Δtesting.T> t, error got, error want) {
    t.Helper();
    if (errors.Is(got, want) || (got != nil && AreEqual(want, errAny))) {
        return;
    }
    t.Fatalf("%v:\ngot error:  %v\nwant error: %v", test.op, got, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object initialDirectoryContentsˢ = (@string)"Initial directory contents:"u8;

internal static void runRootMultiTestDescs(ж<Δtesting.T> t, testFileDesc source, testFileDesc target, Func<ж<Δtesting.T>, ж<rootMultiTest>, (@string, error)> f) {
    var rootTest = newRootTest(t, source, target, true);
    var osTest = newRootTest(t, source, target, false);

    var initialContent = dirTreeContents(t, rootTest.dir);
    var initialContentʗ1 = initialContent;
    t.Cleanup(() => {
        if (t.Failed()) {
            t.Log(initialDirectoryContentsˢ);
            foreach (var (_, line) in range(initialContentʗ1)) {
                t.Logf("  %v", line);
            }
        }
    });

    var (rootResult, rootErr) = f(t, rootTest);

    if (Δruntime.GOOS == "darwin"u8) {
        // Darwin appears to have a kernel bug which causes restrictions on paths
        // with a trailing / to not be applied during uncached path lookups.
        // These restrictions are applied during cached lookups, so the results
        // of operating on /-suffixed paths are inconsistent.
        //
        // An example of this Darwin behavior (as of 25.4.0) is:
        //   $ mkdir -p test/dir
        //   $ echo hello > test/file
        //   $ ln -s dir/../file test/link
        //   $ cat test/link/
        //   hello
        //   $ cat test/link/
        //   cat: test/link/: Not a directory
        //
        // Since Darwin isn't consistent with itself, we can't verify that we're
        // consistent with it.
        if (rootTest.source.anySlashSuffix() || rootTest.target.anySlashSuffix()) {
            return;
        }
    }

    if (Δruntime.GOOS == "wasip1"u8 || Δruntime.GOOS == "js"u8) {
        // WASI runtimes don't have any consistent behavior for handling paths with
        // a trailing /, so skip consistency tests for these paths.
        if (rootTest.source.anySlashSuffix() || rootTest.target.anySlashSuffix()) {
            return;
        }
    }

    var (osResult, osErr) = f(t, osTest);

    var initialContentʗ2 = initialContent;
    var osErrʗ1 = osErr;
    var osTestʗ1 = osTest;
    var rootErrʗ1 = rootErr;
    var rootTestʗ1 = rootTest;
    t.Cleanup(() => {
        if (t.Failed() || !testVerbose.Value) {
            return;
        }
        var rootContentΔ1 = dirTreeContents(t, rootTestʗ1.dir);
        var osContentΔ1 = dirTreeContents(t, osTestʗ1.dir);
        t.Log(initialDirectoryContentsˢ);
        foreach (var (_, line) in range(initialContentʗ2)) {
            t.Logf("  %v", line);
        }
        t.Logf("%v:", rootTestʗ1.op);
        t.Logf("  result: %v", rootResult);
        t.Logf("  error: %v", rootErrʗ1);
        foreach (var (_, line) in range(rootContentΔ1)) {
            t.Logf("  %v", line);
        }
        t.Logf("%v:", osTestʗ1.op);
        t.Logf("  result: %v", osResult);
        t.Logf("  error: %v", osErrʗ1);
        foreach (var (_, line) in range(osContentΔ1)) {
            t.Logf("  %v", line);
        }
    });

    if (errors.Is(rootErr, os_internal_test_package.ErrPathEscapes)) {
        // os.Root forbids this operation (and is therefore not consistent with
        // the non-root version).
        return;
    }

    if (AreEqual(rootErr, errSkipRootConsistencyCheck) || AreEqual(osErr, errSkipRootConsistencyCheck)) {
        return;
    }

    // Consistency check: Performing the same operation in and out of a root
    // should produce the same results.
    if (rootResult != osResult) {
        t.Errorf("inconsistent results in/out of root");
        t.Errorf("%v:", rootTest.op);
        t.Errorf("  result: %v", rootResult);
        t.Errorf("%v:", osTest.op);
        t.Errorf("  result: %v", osResult);
    }
    if ((rootErr == nil) != (osErr == nil)) {
        t.Errorf("inconsistent errors in/out of root");
        t.Errorf("%v:", rootTest.op);
        t.Errorf("  error: %v", rootErr);
        t.Errorf("%v:", osTest.op);
        t.Errorf("  error: %v", osErr);
    }

    // Filesystem consistency check: Same files in the same places.
    var rootContent = dirTreeContents(t, rootTest.dir);
    var osContent = dirTreeContents(t, osTest.dir);
    if (!slices.Equal<slice<@string>, @string>(rootContent, osContent)) {
        t.Errorf("inconsistent filesystem after running in/out of root");
        t.Errorf("%v:", rootTest.op);
        foreach (var (_, line) in range(rootContent)) {
            t.Errorf("  %v", line);
        }
        t.Errorf("%v:", osTest.op);
        foreach (var (_, line) in range(osContent)) {
            t.Errorf("  %v", line);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sourceˢ => ref sourceˢᵇ;
private static readonly @string sourceˢᵇ = "source"u8;

internal static ж<rootMultiTest> newRootTest(ж<Δtesting.T> t, testFileDesc source, testFileDesc target, bool inRoot) {
    ref var dir = ref heap<@string>(makefs(t, [
        "DIR/"
    ]), out var Ꮡdir);
    ж<Δos.Root> root = default;
    if (inRoot) {
        error err = default;
        (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        t.Cleanup(() => {
            rootʗ1.Close();
        });
    }
    var test = Ꮡ(new rootMultiTest(
        dir: dir,
        root: root,
        source: source,
        target: target
    ));
    var dirʗ1 = dir;
    (@string path, fs.FileInfo fi) createFile(@string name, testFileDesc desc) {
        @string path = default;
        fs.FileInfo fi = default;
        if (desc.kind == testFileUnused) {
            return ("", default);
        }
        fi = desc.create(t, dirʗ1, name, name);
        path = desc.@ref.path(dirʗ1, name);
        if (!inRoot && !filepath.IsAbs(path)) {
            path = dirʗ1 + "/"u8 + path;
        }
        return (path, fi);
    }
    (test.sourcePath, test.sourceInfo) = createFile(sourceˢ, source);
    (test.targetPath, test.targetInfo) = createFile(targetˢ, target);
    return test;
}

// testFileKind is a kind of file.
partial struct testFileKind /*num:nint*/;

internal static testFileKind testFileUnused => /* testFileKind(iota) */ 0;
internal static testFileKind testFileAbsent => 1;   // file does not exist
internal static testFileKind testFileFile => 2;     // regular file
internal static testFileKind testFileDir => 3;      // directory
internal static testFileKind testFileSymlink => 4;  // symlink
internal static testFileKind testFileJunction => 5; // Windows directory junction
internal static testFileKind testFileMax => 6;

// testFileError represents a path which fails during resolution,
// such as "a/b" where "a" does not exist.
internal static testFileKind testFileError => 7;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unusedˢ => ref unusedˢᵇ;
private static readonly @string unusedˢᵇ = "unused"u8;
internal static ref readonly @string absentˢ => ref absentˢᵇ;
private static readonly @string absentˢᵇ = "absent"u8;
internal static ref readonly @string errorˢ2 => ref errorˢ2ᵇ;
private static readonly @string errorˢ2ᵇ = "error"u8;

internal static @string String(this testFileKind kind) {
    switch (kind) {
    case var ᴛ when ᴛ == testFileUnused: {
        return unusedˢ;
    }
    case var ᴛ when ᴛ == testFileAbsent: {
        return absentˢ;
    }
    case var ᴛ when ᴛ == testFileFile: {
        return fileˢ2;
    }
    case var ᴛ when ᴛ == testFileDir: {
        return dirˢ;
    }
    case var ᴛ when ᴛ == testFileSymlink: {
        return symlinkˢ;
    }
    case var ᴛ when ᴛ == testFileJunction: {
        return junctionˢ;
    }
    case var ᴛ when ᴛ == testFileError: {
        return errorˢ2;
    }
    default: {
        return fmt.Sprintf("testFileKind(%d)"u8, kind);
    }}
}

internal static bool isLink(this testFileKind kind) {
    return kind == testFileSymlink || kind == testFileJunction;
}

// testFileRef is a kind of reference to a file.
//
// Many path names can refer to the same file: f, ./f, /abs/path/to/f, somedir/../f, etc.
// A testFileRef describes some form of reference.
partial struct testFileRef {
    // name is the name of the reference (not the file name).
    // These are a bit cryptic to keep test names short:
    // s (/ slash), p (.. parent), b (base), d (directory), r (root)
    internal @string name;
    // template is a template path.
    //
    // templates assume that the file is contained in a directory named "ROOT",
    // and that "ROOT/DIR" exists and is a directory.
    //
    // The string BASE in the template may be replaced with the file's basename.
    //
    // Absolute path templates start with /ROOT.
    internal @string template;
    // escapes indicates whether the path escapes the current directory.
    internal bool escapes;
}

internal static slice<testFileRef> testFileRefs = [
    new(escapes: false, name: "b", template: "BASE"),
    new(escapes: false, name: "bs", template: "BASE/"),
    new(escapes: false, name: "dpb", template: "DIR/../BASE"),
    new(escapes: false, name: "dpbs", template: "DIR/../BASE/"),
    new(escapes: true, name: "prb", template: "../ROOT/BASE"),
    new(escapes: true, name: "prbs", template: "../ROOT/BASE/"),
    new(escapes: true, name: "srb", template: "/ROOT/BASE"),
    new(escapes: true, name: "srbs", template: "/ROOT/BASE/")
];

// testFileLimitedRefs is a smaller set of references which do not exercise path escapes
// (see allTestFileDescs).
internal static slice<testFileRef> testFileLimitedRefs = testFileRefs[0..2];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string baseˢ => ref baseˢᵇ;
private static readonly @string baseˢᵇ = "BASE"u8;
internal static ref readonly @string rootˢ3 => ref rootˢ3ᵇ;
private static readonly @string rootˢ3ᵇ = "/ROOT"u8;

// path creates a path using the template.
//
// dir is the absolute path to the root directory (which must be named "ROOT").
// base is the name of the target file within the root directory.
internal static @string path(this testFileRef @ref, @string dir, @string @base) {
    @string p = @ref.template;
    p = strings.ReplaceAll(p, baseˢ, @base);
    var (trim, ok) = strings.CutPrefix(p, rootˢ3); if (ok) {
        p = dir + trim;
    }
    return p;
}

// hasSlashSuffix reports whether the file reference ends in a /.
internal static bool hasSlashSuffix(this testFileRef @ref) {
    return strings.HasSuffix(@ref.template, "/");
}

// testFileDesc is a description of a type of file, combining the kind and reference type.
//
// Some sample testFileDescs:
//   - "name", a plain file.
//   - "DIR/../name", a directory
//   - "name/", where name is a symlink to "DIR/../target/", where target is a plain file.
partial struct testFileDesc {
    internal testFileKind kind;
    internal testFileRef @ref;
    internal ж<testFileDesc> target; // symlink or junction target, nil when !kind.isLink()
}

internal static ж<bool> rootComprehensive = flag.Bool("root_comprehensive", false,
    "run many more os.Root test variations (slow, uncertain value)");

internal static Action<ж<Δtesting.T>, @string, @string> createJunction;

// allTestFileDescs returns an iterator over all the testFileDescs we use in tests.
internal static iter.Seq<testFileDesc> allTestFileDescs() {
    // A testFileDesc contains a reference type ("name", "d/../name", "../r/name", etc.) and
    // a file kind (file, directory, symlink, etc.).
    //
    // When the kind is symlink, the desc contains a reference type and file kind for
    // the link target as well. We only exercise one level of symlink (although we
    // could do more), so this means a testFileDesc effectively contains four axes of
    // variation: ref, kind, link ref, link kind.
    //
    // For example:
    //
    //   - "name" is a file
    //   - "d/../name" is a directory
    //   - "name" is a symlink to "name2" which is a file
    //   - "d/../name" is a symlink to "d/../name2" which is a directory
    //   - etc.
    //
    // It is feasible to test every possible variation of these four axes,
    // but this is quite a few tests and gets quite slow. So by default we exclude
    // some variations. We test:
    //
    //   - direct and direct/ references to a symlink or junction
    //     to every reference to a file
    //   - a direct reference to a symlink or junction
    //     to a direct reference to every kind (except file)
    //
    // The full set of variations may be enabled with the -comprehensive_root_tests flag.

    return (Func<testFileDesc, bool> yield) => {
        // Every type of reference to every type of file, except symlink.
        foreach (var (_, @ref) in range(testFileRefs)) {
            foreach (var kind in range(testFileMax)) {
                if (kind == testFileUnused || kind.isLink()) {
                    continue;
                }
                var desc = new testFileDesc(
                    kind: kind,
                    @ref: @ref
                );
                if (!yield(desc)) {
                    return;
                }
            }
        }

        // Unless we're being comprehensive, only direct references to symlinks.
        var refs = testFileRefs;
        if (!rootComprehensive.Value) {
            refs = testFileLimitedRefs;
        }
        foreach (var (_, kind) in range(slice<testFileKind>([testFileSymlink, testFileJunction]))) {
            if (kind == testFileJunction && Δruntime.GOOS != "windows"u8) {
                continue;
            }
            foreach (var (_, @ref) in range(refs)) {
                foreach (var iᴛ1 in range(testFileMax)) {
                    ref var linkKind = ref heap(new testFileKind(), out var ᏑlinkKind);
                    linkKind = iᴛ1;

                    if (linkKind == testFileUnused || linkKind.isLink()) {
                        continue;
                    }

                    var linkRefs = testFileRefs;
                    if (!rootComprehensive.Value && linkKind != testFileFile && linkKind != testFileDir) {
                        linkRefs = testFileLimitedRefs;
                    }
                    foreach (var (_, vᴛ1) in range(linkRefs)) {
                        ref var linkRef = ref heap(new testFileRef(), out var ᏑlinkRef);
                        linkRef = vᴛ1;

                        var desc = new testFileDesc(
                            kind: kind,
                            @ref: @ref,
                            target: Ꮡ(new testFileDesc(
                                kind: linkKind,
                                @ref: linkRef
                            ))
                        );
                        if (!yield(desc)) {
                            return;
                        }
                    }
                }
            }
        }
    };
}

// String returns the target name.
//
// These are somewhat cryptic to keep test names short.
// For example, "bsSdpbD" is:
//
//	bs  - "BASE/"
//	S   - symlink
//	dpb - "DIR/../BASE"
//	D   - directory
//
// So, open "file1/", where file1 is a symlink to "DIR/../file2", where file2 is a directory.
internal static @string String(this testFileDesc desc) {
    @string s = desc.@ref.name + strings.ToUpper(desc.kind.String()[..1]);
    if (desc.kind.isLink()) {
        s += desc.target.String();
    }
    return s;
}

// escapes reports whether accessing this file escapes the root,
// either because the file name escapes or because some element of a symlink chain escapes.
internal static bool escapes(this testFileDesc desc) {
    if (desc.@ref.escapes) {
        return true;
    }
    if (desc.kind == testFileSymlink) {
        return desc.target.escapes();
    }
    if (desc.kind == testFileJunction) {
        return true;
    }
    return false;
}

internal static bool lescapes(this testFileDesc desc) {
    if (desc.@ref.escapes) {
        return true;
    }
    if (Δruntime.GOOS == "windows"u8) {
        // On POSIX filesystems, a trailing slash at the end of a path causes
        // symlinks in the last path component to be resolved.
        // On Windows, a trailing slash does not cause symlink resolution.
        return false;
    }
    if (desc.@ref.hasSlashSuffix() && desc.kind.isLink()) {
        return desc.target.escapes();
    }
    return false;
}

// finalKind reports the kind of the file after following all symlinks.
internal static testFileKind finalKind(this testFileDesc desc) {
    if (desc.kind.isLink()) {
        return desc.target.finalKind();
    }
    return desc.kind;
}

internal static testFileKind lfinalKind(this testFileDesc desc) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "windows"u8: {
        if (desc.@ref.hasSlashSuffix() && desc.kind.isLink() && desc.target.kind != testFileDir) {
            return testFileError;
        }
        break;
    }
    default: {
        if (desc.@ref.hasSlashSuffix() && desc.kind.isLink()) {
            return desc.target.finalKind();
        }
        break;
    }}
    return desc.kind;
}

internal static bool isError(this testFileDesc desc) {
    if (Δruntime.GOOS == "js"u8) {
        return false;
    }
    ref var isError = ref heap<Func<testFileDesc, bool, bool>>(out var ᏑisError);
    isError = (testFileDesc descΔ1, bool hasSuffix) => {
        if (descΔ1.@ref.escapes) {
            return false;
        }
        if (descΔ1.@ref.hasSlashSuffix()) {
            hasSuffix = true;
        }
        switch (descΔ1.kind) {
        case var ᴛ when ᴛ == testFileDir: {
            return false;
        }
        case var ᴛ when ᴛ == testFileSymlink: {
            if (Δruntime.GOOS == "windows"u8 && hasSuffix && descΔ1.target.kind != testFileDir) {
                return true;
            }
            return ᏑisError.ValueSlot(descΔ1.target.Value, hasSuffix);
        }
        case var ᴛ when ᴛ == testFileJunction: {
            if (descΔ1.target.kind == testFileFile || (hasSuffix && descΔ1.target.kind != testFileDir)) {
                return true;
            }
            return ᏑisError.ValueSlot(descΔ1.target.Value, hasSuffix);
        }
        default: {
            return hasSuffix;
        }}
    };
    return isError(desc, false);
}

internal static bool isSymlinkToDir(this testFileDesc desc) {
    if (!desc.kind.isLink()) {
        return false;
    }
    if (desc.@ref.escapes) {
        return false;
    }
    if (desc.kind == testFileJunction) {
        // Windows junctions are always directory links, regardless of what the
        // target of the junction might be.
        return true;
    }
    if (desc.finalKind() == testFileDir) {
        return true;
    }
    return false;
}

// anySlashSuffix reports whether any of the names in the file
// (either the initial name, or a symlink target)
// include a trailing /.
internal static bool anySlashSuffix(this testFileDesc desc) {
    @string name = desc.@ref.template;
    if (len(name) > 0 && Δos.IsPathSeparator(name[len(name) - 1])) {
        return true;
    }
    if (desc.kind.isLink()) {
        return desc.target.anySlashSuffix();
    }
    return false;
}

// anySlashSuffix reports whether the name of the file includes a trailing /.
internal static bool slashSuffix(this testFileDesc desc) {
    @string name = desc.@ref.template;
    if (len(name) > 0 && Δos.IsPathSeparator(name[len(name) - 1])) {
        return true;
    }
    return false;
}

// create creates the file(s) for this descriptor.
//
// dir is the test root directory.
// base is the base name of the file we will open within the root.
// (If there are symlinks, base is the start of the symlink chain.)
//
// Tests may create, delete, or move files, which makes it useful to have a way to identify
// and track the files that existed at the start of the test. The token parameter identifies
// which file we're creating. When symlinks are involved, the token is used in creating the
// final, non-symlink file.
internal static fs.FileInfo /*fi*/ create(this testFileDesc desc, ж<Δtesting.T> t, @string dir, @string @base, @string token) {
    fs.FileInfo fi = default;

    @string path = filepath.Join(dir, @base);
    switch (desc.kind) {
    case var ᴛ when ᴛ == testFileAbsent: {
        // File does not exist.
        break;
    }
    case var ᴛ when ᴛ == testFileFile: {
        // Regular file. We use the token as the file contents.
        {
            var err = Δos.WriteFile(path, slice<byte>(token), 438); if (err != nil) {
                t.Fatal(err);
            }
        }
        break;
    }
    case var ᴛ when ᴛ == testFileDir: {
        // Directory. We create a subdir within the directory named "c_"+token.
        // (The "c_" prefix is to distinguish this subdir from any files that may
        // have the same name as the token.)
        {
            var err = Δos.Mkdir(path, 511); if (err != nil) {
                t.Fatal(err);
            }
        }
        break;
    }
    case var ᴛ when ᴛ == testFileSymlink: {
        // Symlink. We create a symlink target named "s_"+base.
        if (Δruntime.GOOS == "plan9"u8) {
            t.Skip("symlinks not supported on " + Δruntime.GOOS);
        }
        @string linktarget = desc.target.@ref.path(dir, "s_"u8 + @base);
        if (Δruntime.GOOS == "wasip1"u8 && filepath.IsAbs(linktarget)) {
            t.Skip("absolute link targets not supported on " + Δruntime.GOOS);
        }
        fi = desc.target.create(t, dir, "s_"u8 + @base, token);
        {
            var err = Δos.Symlink(linktarget, path); if (err != nil) {
                t.Fatal(err);
            }
        }
        break;
    }
    case var ᴛ when ᴛ == testFileJunction: {
        // Directory junction. We create a target named "s_"+base.
        if (Δruntime.GOOS != "windows"u8) {
            t.Skip("junctions not supported on " + Δruntime.GOOS);
        }
        @string linktarget = desc.target.@ref.path(dir, "s_"u8 + @base);
        fi = desc.target.create(t, dir, "s_"u8 + @base, token);
        createJunction(t, path, linktarget);
        break;
    }
    default: {
        t.Fatalf("can't create file of kind: %v", desc.kind);
        break;
    }}
    if (desc.kind == testFileFile || desc.kind == testFileDir) {
        error err = default;
        (fi, err) = Δos.Lstat(path);
        if (err != nil) {
            t.Fatal(err);
        }
    }
    return fi;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownFileˢ => ref unknownFileˢᵇ;
private static readonly @string unknownFileˢᵇ = "unknown file"u8;

// testRootDescribeFile returns a string identifying a file.
//
// It returns "" if f is nil.
// It returns "source" or "target" if f is the source or target file in the test.
// Otherwise, it returns "unknown file".
internal static @string describeFile(this ref rootMultiTest test, ж<Δtesting.T> t, ж<Δos.File> f) {
    if (f == nil) {
        return ""u8;
    }
    var (fi, err) = f.Stat();
    if (err != nil) {
        t.Fatal(err);
    }
    switch (ᐧ) {
    case {} when Δos.SameFile(fi, test.sourceInfo): {
        return sourceˢ;
    }
    case {} when Δos.SameFile(fi, test.targetInfo): {
        return targetˢ;
    }
    default: {
        return unknownFileˢ;
    }}
}

// dirTreeContents returns a description of the contents of directory.
// For example:
//
//	drwxrwxrwx dir/
//	-rw-rw-rw- dir/file "file contents"
//	Lrw-rw-rw- symlink => dir/file
internal static slice<@string> /*contents*/ dirTreeContents(ж<Δtesting.T> t, @string dir) {
    slice<@string> contents = default;
    GoFrame ᒐ = default;
    try {
        var (root, err) = Δos.OpenRoot(dir);
        if (err != nil) {
            t.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        var rootʗ2 = root;
        fs.WalkDir(root.FS(), ".", (@string path, fs.DirEntry d, error errΔ1) => {
            if (path == "."u8) {
                return default;
            }
            (var info, errΔ1) = d.Info();
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            @string ent = info.Mode().String() + " "u8 + path;
            switch (d.Type()) {
            case var ᴛ when ᴛ == fs.ModeDir: {
                ent += "/"u8;
                break;
            }
            case var ᴛ when ᴛ == fs.ModeSymlink || ᴛ == fs.ModeIrregular: {
                var (target, errΔ5) = rootʗ2.Readlink(path);
                if (errΔ5 != nil) {
                    t.Fatal(errΔ5);
                }
                if (filepath.IsAbs(target)) {
                    var (relPath, errΔ6) = filepath.Rel(dir, target);
                    if (errΔ6 == nil && filepath.IsLocal(relPath)) {
                        target = "/.../"u8 + relPath;
                    }
                }
                ent += " => "u8 + target;
                break;
            }
            default: {
                var (f, errΔ7) = rootʗ2.Open(path);
                if (errΔ7 != nil) {
                    ent += " (unreadable)"u8;
                } else {
                    var (content, errΔ8) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    if (errΔ8 != nil) {
                        t.Fatal(errΔ8);
                    }
                    ent += fmt.Sprintf(" %q"u8, content);
                }
                break;
            }}
            contents = append(contents, ent);
            return default;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return contents;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string openQˢ => ref openQˢᵇ;
private static readonly @string openQˢᵇ = "Open(%q)"u8;

// TestRootMultiOpen tests os.Root.Open.
//
// This also serves as a prototypical example of using rootMultiTest
// (see also the doc comment on rootMultiTest above).
public static void TestRootMultiOpen(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        GoFrame ᒐ = default;
        try {
            // This function will be run many times, with different inputs:
            //   - in and out of a Root
            //   - opening a file, directory, symlink, or nothing at all
            //   - opening various names: target, DIR/../target, /abs/path/to/target, etc.
            //
            // The test function should perform the requested operation
            // (for example: open "target" in a Root),
            // verify that the result is consistent with expectations,
            // and then return a description of the result.
            //
            // The returned description is used to validate consistent behavior
            // between operations in and out of a Root.
            Func<@string, (ж<Δos.File>, error)> open = Δos.Open;
            if (test.root != nil) {
                var recvʗ1 = test.root;
                open = (@string p1) => recvʗ1.Open(p1);
            }

            test.setOp(openQˢ, test.targetPath); // test's operation, for errors
            var (f, gotErr) = open(test.targetPath);
            if (gotErr == nil) {
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
            }

            // testRootDescribeFile returns a string identifying a file.
            //
            // This is always "source" or "target" for the source/target files in a test,
            // or "" if f is nil.
            // (Note that most tests use only a target file, no source.)
            @string got = test.describeFile(tΔ1, f);

            switch (ᐧ) {
            case {} when test.target.isError(): {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when test.root != nil && test.target.escapes(): {
                // The operation escapes the root.
                test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
                break;
            }
            case {} when test.target.finalKind() == testFileAbsent: {
                // The file does not exist ("absent").
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when test.target.anySlashSuffix(): {
                // The file name or a symlink target contain a trailing slash.
                // Trailing slashes are handled differently on different platforms,
                // so we won't try to assert an outcome when they are present.
                // runRootMultiTest will verify that root.Open and os.Open
                // produce consistent results.
                break;
            }
            default: {
                // We should have successfully opened the file.
                test.wantError(tΔ1, gotErr, default);
                @string want = targetˢ; if (got != want) {
                    tΔ1.Fatalf("opened file %q, want %q", got, want);
                }
                break;
            }}

            // Return the name of the file opened (possibly "" for nothing) and the error.
            // runRootMultiTest will compare the results for in-a-root and out-of-a-root
            // to validate that they are the same.
            return (got, gotErr);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chmodQOˢ => ref chmodQOˢᵇ;
private static readonly @string chmodQOˢᵇ = "Chmod(%q, %o)"u8;

public static void TestRootMultiChmod(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "wasip1"u8) {
        t.Skip("Chmod not supported on " + Δruntime.GOOS);
    }
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        ref var chmod = ref heap<Func<@string, fs.FileMode, error>>(out var Ꮡchmod);

        chmod = Δos.Chmod;
        Func<@string, (fs.FileInfo, error)> stat = Δos.Stat;
        Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            Ꮡchmod.ValueSlot = (@string p1, fs.FileMode p2) => recvʗ1.Chmod(p1, p2);
            var recvʗ2 = test.root;
            stat = (@string p1) => recvʗ2.Stat(p1);
            var recvʗ3 = test.root;
            lstat = (@string p1) => recvʗ3.Lstat(p1);
        }

        // Using the wrong mode here can cause problems during test cleanup,
        // if we leave a temp dir with a mode that prevents listing or removing
        // its contents.
        //
        // read+execute permissions let us list directory contents,
        // and we restore writability before deleting the temp dir.
        var wantMode = (fs.FileMode)320; // readable, executable
        if (Δruntime.GOOS == "windows"u8) {
            // On Windows, the only modes we support are the default (777/rwx)
            // or read-only (444/r-x). Making a directory read-only doesn't prevent
            // listing its contents, so we can use 444 here.
            wantMode = 292; // readable
        }
        tΔ1.Cleanup(() => {
            Ꮡchmod.ValueSlot(test.targetPath, 448);
        });

        test.setOp(chmodQOˢ, test.targetPath, wantMode);
        var gotErr = Ꮡchmod.ValueSlot(test.targetPath, wantMode);

        var escapes = test.target.escapes();
        testFileKind targetKind = test.target.finalKind();
        if (Δruntime.GOOS == "windows"u8) {
            // On Windows, Chmod("symlink") affects the link, not its target.
            // See issue #71492.
            stat = lstat;
            escapes = test.target.@ref.escapes;
            targetKind = test.target.kind;
        }

        fs.FileMode gotMode = default;
        switch (ᐧ) {
        case {} when test.root != nil && escapes: {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when targetKind == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            // Don't expect anything, just be consistent with the OS.
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);

            var (fi, err) = stat(test.targetPath);
            if (err != nil) {
                tΔ1.Fatalf("could not stat target: %v", err);
            }
            if (Δruntime.GOOS == "windows"u8 && !fi.Mode().IsRegular()) {
                // See issue #71492.
                break;
            }

            gotMode = (fs.FileMode)(fi.Mode() & fs.ModePerm);
            if (gotMode != wantMode) {
                tΔ1.Fatalf("file %q:\ngot mode:  %v\nwant mode: %v", test.targetPath, gotMode, wantMode);
            }
            break;
        }}

        if (Δruntime.GOOS == "windows"u8 && test.root == nil && gotErr != nil) {
            // On Windows, os.Chmod calls GetFileAttributes on the target.
            // This seems to fail in a number of situations where the os.Root
            // chmod path works. For now, just skip the consistency check
            // when os.Chmod fails.
            return ("", errSkipRootConsistencyCheck);
        }

        return (gotMode.String(), gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string createQˢ => ref createQˢᵇ;
private static readonly @string createQˢᵇ = "Create(%q)"u8;

public static void TestRootMultiCreate(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        GoFrame ᒐ = default;
        try {
            Func<@string, (ж<Δos.File>, error)> create = Δos.Create;
            if (test.root != nil) {
                var recvʗ1 = test.root;
                create = (@string p1) => recvʗ1.Create(p1);
            }

            test.setOp(createQˢ, test.targetPath); // test's operation, for errors
            var (f, gotErr) = create(test.targetPath);
            if (gotErr == nil) {
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
            }

            switch (ᐧ) {
            case {} when test.target.isError(): {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when Δruntime.GOOS == "windows"u8 && test.target.isSymlinkToDir(): {
                // The error here is because the link is a Windows directory link,
                // not because the link target is a directory.
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when test.root != nil && test.target.escapes(): {
                // The operation escapes the root.
                test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
                break;
            }
            default: {
                break;
            }}

            return ("", gotErr);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testSeemsToTickleAˢ = (@string)"test seems to tickle a wasmtime bug"u8;
internal static ref readonly @string linkQQˢ => ref linkQQˢᵇ;
private static readonly @string linkQQˢᵇ = "Link(%q, %q)"u8;

public static void TestRootMultiLink(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "wasip1"u8) {
        var exprᴛ1 = Δos.Getenv(gowasiruntimeˢ);
        switch (ᐧ) {
        case {} when exprᴛ1 == ""u8 || exprᴛ1 == "wasmtime"u8: {
            // This test fails when run with wasmtime, because os.RemoveAll fails
            // to remove the test tempdir.
            t.Skip(testSeemsToTickleAˢ);
            break;
        }}
    }
    testenv.MustHaveLink(new os_test_package.testing_TжTB(t));
    runRootMultiTest2(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, @string, error> rename = Δos.Link;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            rename = (@string p1, @string p2) => recvʗ1.Link(p1, p2);
        }

        test.setOp(linkQQˢ, test.sourcePath, test.targetPath);
        var gotErr = rename(test.sourcePath, test.targetPath);

        switch (ᐧ) {
        case {} when test.root != nil && test.source.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.source.lfinalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.source.kind.isLink(): {
            // os.Link(old, new) may or may not deference old when it is a symlink.
            // POSIX says that link(2) should deference the source, but implementations
            // are inconsistent.
            return ("", errSkipRootConsistencyCheck);
        }
        case {} when test.source.slashSuffix() && test.source.lfinalKind() != testFileDir: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lstatQˢ => ref lstatQˢᵇ;
private static readonly @string lstatQˢᵇ = "Lstat(%q)"u8;

public static void TestRootMultiLstat(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            lstat = (@string p1) => recvʗ1.Lstat(p1);
        }

        test.setOp(lstatQˢ, test.targetPath);
        var (gotStat, gotErr) = lstat(test.targetPath);

        @string result = ""u8;
        if (gotStat != nil) {
            result = gotStat.Mode().String();
        }

        var escapes = test.target.lescapes();
        testFileKind finalKind = test.target.lfinalKind();
        if (Δruntime.GOOS == "windows"u8 && test.target.@ref.hasSlashSuffix()) {
            // When the target of lstat has a trailing slash,
            // Windows follows it.
            escapes = test.target.escapes();
            finalKind = test.target.finalKind();
        }

        switch (ᐧ) {
        case {} when test.root != nil && escapes: {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.kind == testFileAbsent: {
            // Target does not exist.
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when finalKind == testFileSymlink: {
            test.wantError(tΔ1, gotErr, default);
            {
                var (got, want) = (gotStat.Mode().Type(), fs.ModeSymlink); if (got != want) {
                    test.errorf(tΔ1, "got mode %v, want %v", got, want);
                }
            }
            break;
        }
        case {} when finalKind == testFileJunction: {
            test.wantError(tΔ1, gotErr, default);
            {
                var (got, want) = (gotStat.Mode().Type(), fs.ModeIrregular); if (got != want) {
                    test.errorf(tΔ1, "got mode %v, want %v", got, want);
                }
            }
            break;
        }
        case {} when gotErr != nil: {
            break;
        }
        default: {
            if (!Δos.SameFile(gotStat, test.targetInfo)) {
                test.errorf(tΔ1, "stat result is not for target file; want it to be");
            }
            break;
        }}

        return (result, gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mkdirQ0o777ˢ => ref mkdirQ0o777ˢᵇ;
private static readonly @string mkdirQ0o777ˢᵇ = "Mkdir(%q, 0o777)"u8;

public static void TestRootMultiMkdir(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, fs.FileMode, error> mkdir = Δos.Mkdir;
        Func<@string, (fs.FileInfo, error)> stat = Δos.Stat;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            mkdir = (@string p1, fs.FileMode p2) => recvʗ1.Mkdir(p1, p2);
            var recvʗ2 = test.root;
            stat = (@string p1) => recvʗ2.Stat(p1);
        }

        test.setOp(mkdirQ0o777ˢ, test.targetPath);
        var gotErr = mkdir(test.targetPath, 511);

        switch (ᐧ) {
        case {} when test.root != nil && test.target.@ref.escapes: {
            // "mkdir ../target", or equivalent escaping path.
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.slashSuffix() && test.target.kind.isLink(): {
            // "mkdir symlink/", inconsistent behavior across platforms
            // as to whether this follows the symlink or not.
            //
            // If the symlink escapes, this needs to be some kind of error though.
            if (test.root != nil && test.target.escapes()) {
                test.wantError(tΔ1, gotErr, errAny);
            }
            if (Δruntime.GOOS == "openbsd"u8) {
                // Known inconsistency: OpenBSD doesn't resolve the final
                // symlink when creating a directory.
                return ("", errSkipRootConsistencyCheck);
            }
            break;
        }
        case {} when test.target.kind != testFileAbsent: {
            // "mkdir target", where target exists.
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            var (fi, err) = stat(test.targetPath);
            if (err != nil) {
                tΔ1.Fatalf("could not stat target: %v", err);
            }
            if (!fi.IsDir()) {
                tΔ1.Fatalf("%q: not a directory, expected it to be", test.targetPath);
            }
            break;
        }}
        return ("", gotErr);
    });
}

public static void TestRootMultiMkdirAllShallow(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => testRootMultiMkdirAll(tΔ1, test, test.targetPath));
}

public static void TestRootMultiMkdirAllDeep(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        @string targetPath = test.targetPath;
        if (len(targetPath) > 0 && Δos.IsPathSeparator(targetPath[len(targetPath) - 1])) {
            targetPath += "a/b/"u8;
        } else {
            targetPath += "/a/b"u8;
        }
        return testRootMultiMkdirAll(tΔ1, test, targetPath);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mkdirAllQ0o777ˢ => ref mkdirAllQ0o777ˢᵇ;
private static readonly @string mkdirAllQ0o777ˢᵇ = "MkdirAll(%q, 0o777)"u8;

internal static (@string, error) testRootMultiMkdirAll(ж<Δtesting.T> t, ж<rootMultiTest> Ꮡtest, @string targetPath) {
    ref var test = ref Ꮡtest.DerefOrNull();

    Func<@string, fs.FileMode, error> mkdirAll = Δos.MkdirAll;
    if (test.root != nil) {
        var recvʗ1 = test.root;
        mkdirAll = (@string p1, fs.FileMode p2) => recvʗ1.MkdirAll(p1, p2);
    }

    test.setOp(mkdirAllQ0o777ˢ, targetPath);
    var gotErr = mkdirAll(targetPath, 511);

    switch (ᐧ) {
    case {} when test.root != nil && test.target.lescapes(): {
        // "mkdir ../target", or equivalent escaping path.
        test.wantError(t, gotErr, os_internal_test_package.ErrPathEscapes);
        break;
    }
    case {} when test.root != nil && test.target.escapes(): {
        // "mkdir ../target", or equivalent escaping path.
        test.wantError(t, gotErr, errAny);
        return ("", errSkipRootConsistencyCheck);
    }
    case {} when test.root != nil && test.target.kind.isLink() && test.target.target.kind == testFileAbsent && targetPath != test.targetPath: {
        // A minor inconsistency between Root.MkdirAll and os.MkdirAll:
        // When an intermediate component of the tree being constructed is a
        // dangling symlink, Root.MkdirAll will follow the symlink and create
        // its target directory, while os.MkdirAll will fail with an error.
        return ("", errSkipRootConsistencyCheck);
    }
    default: {
        break;
    }}
    return ("", gotErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string renameQQˢ => ref renameQQˢᵇ;
private static readonly @string renameQQˢᵇ = "Rename(%q, %q)"u8;

public static void TestRootMultiRename(ж<Δtesting.T> t) {
    if (Δruntime.GOOS == "wasip1"u8) {
        var exprᴛ1 = Δos.Getenv(gowasiruntimeˢ);
        switch (ᐧ) {
        case {} when exprᴛ1 == ""u8 || exprᴛ1 == "wasmtime"u8: {
            // This test fails when run with wasmtime, because os.RemoveAll fails
            // to remove the test tempdir.
            t.Skip(testSeemsToTickleAˢ);
            break;
        }}
    }
    runRootMultiTest2(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, @string, error> rename = Δos.Rename;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            rename = (@string p1, @string p2) => recvʗ1.Rename(p1, p2);
        }

        // TODO: target directory (if any) should be empty

        test.setOp(renameQQˢ, test.sourcePath, test.targetPath);
        var gotErr = rename(test.sourcePath, test.targetPath);

        if (Δruntime.GOOS == "windows"u8 && (test.source.finalKind() != test.target.finalKind() || test.source.kind.isLink() || test.target.kind.isLink())) {
            // os.Rename on Windows is implemented using MoveFileEx,
            // while Root.Rename is implemented using NtSetInformationFileEx
            // with an explicit request for POSIX semantics.
            //
            // This means the two do not behave the same when renaming
            // a file onto a directory or vice-versa.
            //
            // We should make this consistent, but for now just skip
            // the consistency checks in this case.
            return ("", errSkipRootConsistencyCheck);
        }

        switch (ᐧ) {
        case {} when test.root != nil && test.source.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.source.lfinalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.source.slashSuffix() && test.source.lfinalKind() != testFileDir && Δruntime.GOOS != "js"u8: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.root != nil && test.target.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when Δruntime.GOOS == "plan9"u8: {
            // Plan9 rename behaves differently.
            // Just rely on consistency checks.
            break;
        }
        case {} when test.target.lfinalKind() == testFileDir: {
            // POSIX rename() will replace an empty target directory,
            // but os.Rename will not.
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.source.lfinalKind() == testFileDir && test.target.lfinalKind() != testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.source.anySlashSuffix() || test.target.anySlashSuffix(): {
            if (Δruntime.GOOS == "openbsd"u8) {
                // Known inconsistency: OpenBSD doesn't resolve the final
                // symlink when creating a directory.
                return ("", errSkipRootConsistencyCheck);
            }
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            // TODO: check that the file is in its new location
            break;
        }}

        if (Δruntime.GOOS == "linux"u8 && (test.source.slashSuffix() || test.target.slashSuffix())) {
            return ("", errSkipRootConsistencyCheck);
        }

        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readFileQˢ => ref readFileQˢᵇ;
private static readonly @string readFileQˢᵇ = "ReadFile(%q)"u8;

public static void TestRootMultiReadFile(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (slice<byte>, error)> readFile = Δos.ReadFile;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            readFile = (@string p1) => recvʗ1.ReadFile(p1);
        }

        test.setOp(readFileQˢ, test.targetPath);
        var (data, gotErr) = readFile(test.targetPath);
        @string got = default;
        if (gotErr == nil) {
            got = (@string)data;
        }

        switch (ᐧ) {
        case {} when test.target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.root != nil && test.target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.finalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when Δruntime.GOOS == "plan9"u8: {
            // Plan9 lets you read from directories.
            // Just rely on consistency checks.
            break;
        }
        case {} when Δruntime.GOOS == "netbsd"u8: {
            // See https://go.dev/issue/80322:
            // NetBSD builder appears to be succeeding on read-from-dir as well.
            return ("", gotErr);
        }
        case {} when test.target.finalKind() == testFileDir: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            // Trailing slashes are handled differently on different platforms,
            // so we won't try to assert an outcome when they are present.
            // runRootMultiTest will verify that root.ReadFile and os.ReadFile
            // produce consistent results.
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            @string want = targetˢ; if (got != want) {
                tΔ1.Fatalf("read file content %q, want %q", got, want);
            }
            break;
        }}

        return (got, gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string statQˢ => ref statQˢᵇ;
private static readonly @string statQˢᵇ = "Stat(%q)"u8;

public static void TestRootMultiStat(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (fs.FileInfo, error)> stat = Δos.Stat;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            stat = (@string p1) => recvʗ1.Stat(p1);
        }

        test.setOp(statQˢ, test.targetPath);
        var (gotStat, gotErr) = stat(test.targetPath);

        switch (ᐧ) {
        case {} when test.target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.root != nil && test.target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.finalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            if (!Δos.SameFile(gotStat, test.targetInfo)) {
                test.errorf(tΔ1, "stat result is not for target file; want it to be");
            }
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string removeQˢ => ref removeQˢᵇ;
private static readonly @string removeQˢᵇ = "Remove(%q)"u8;

public static void TestRootMultiRemove(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, error> remove = Δos.Remove;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            remove = (@string p1) => recvʗ1.Remove(p1);
        }

        test.setOp(removeQˢ, test.targetPath);
        var gotErr = remove(test.targetPath);

        switch (ᐧ) {
        case {} when test.root != nil && test.target.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.kind == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            if (Δruntime.GOOS == "linux"u8) {
                // Linux treats rmdir("symlink/") as an error when
                // "symlink" is a symlink to a directory.
                // Root.Remove prefers the POSIX interpretation
                // of resolving the symlink.
                return ("", errSkipRootConsistencyCheck);
            }
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string removeAllQˢ => ref removeAllQˢᵇ;
private static readonly @string removeAllQˢᵇ = "RemoveAll(%q)"u8;

public static void TestRootMultiRemoveAll(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, error> removeAll = Δos.RemoveAll;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            removeAll = (@string p1) => recvʗ1.RemoveAll(p1);
        }

        test.setOp(removeAllQˢ, test.targetPath);
        var gotErr = removeAll(test.targetPath);

        switch (ᐧ) {
        case {} when test.root != nil && test.target.@ref.escapes: {
            // This is only checking target.ref.escapes,
            // not target.lescapes(), because RemoveAll strips
            // terminal slashes.
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            // We are inconsistent on some platforms on whether
            // RemoveAll("symlink/") removes the link or the link target.
            // Something worth addressing, but for now skip the check.
            return ("", errSkipRootConsistencyCheck);
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chtimesQVVˢ => ref chtimesQVVˢᵇ;
private static readonly @string chtimesQVVˢᵇ = "Chtimes(%q, %v, %v)"u8;

public static void TestRootMultiChtimes(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, time.Time, time.Time, error> chtimes = Δos.Chtimes;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            chtimes = (@string p1, time.Time p2, time.Time p3) => recvʗ1.Chtimes(p1, p2, p3);
        }

        var now = time.Now();
        test.setOp(chtimesQVVˢ, test.targetPath, now, now);
        var gotErr = chtimes(test.targetPath, now, now);

        switch (ᐧ) {
        case {} when test.target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.root != nil && test.target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.finalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readlinkQˢ => ref readlinkQˢᵇ;
private static readonly @string readlinkQˢᵇ = "Readlink(%q)"u8;

public static void TestRootMultiReadlink(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (@string, error)> readlink = Δos.Readlink;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            readlink = (@string p1) => recvʗ1.Readlink(p1);
        }

        test.setOp(readlinkQˢ, test.targetPath);
        var (got, gotErr) = readlink(test.targetPath);
        var (suffix, ok) = strings.CutPrefix(got, test.dir); if (ok) {
            // Replace absolute path prefix with /.../
            got = "/..."u8 + suffix;
        }

        switch (ᐧ) {
        case {} when test.root != nil && test.target.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when !test.target.kind.isLink(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            break;
        }}
        return (got, gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeFileQˢ => ref writeFileQˢᵇ;
private static readonly @string writeFileQˢᵇ = "WriteFile(%q, ...)"u8;

public static void TestRootMultiWriteFile(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, slice<byte>, fs.FileMode, error> writeFile = Δos.WriteFile;
        if (test.root != nil) {
            var recvʗ1 = test.root;
            writeFile = (@string p1, slice<byte> p2, fs.FileMode p3) => recvʗ1.WriteFile(p1, p2, p3);
        }

        test.setOp(writeFileQˢ, test.targetPath);
        var gotErr = writeFile(test.targetPath, slice<byte>("data"u8), 438);

        switch (ᐧ) {
        case {} when test.target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when Δruntime.GOOS == "windows"u8 && test.target.isSymlinkToDir(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.root != nil && test.target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when test.target.finalKind() == testFileDir: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when test.target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string openFileQORdonly0ˢ => ref openFileQORdonly0ˢᵇ;
private static readonly @string openFileQORdonly0ˢᵇ = "OpenFile(%q, O_RDONLY, 0)"u8;

public static void TestRootMultiOpenFile(ж<Δtesting.T> t) {
    runRootMultiTest(t, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        GoFrame ᒐ = default;
        try {
            Func<@string, nint, fs.FileMode, (ж<Δos.File>, error)> openFile = Δos.OpenFile;
            if (test.root != nil) {
                var recvʗ1 = test.root;
                openFile = (@string p1, nint p2, fs.FileMode p3) => recvʗ1.OpenFile(p1, p2, p3);
            }

            test.setOp(openFileQORdonly0ˢ, test.targetPath);
            var (f, gotErr) = openFile(test.targetPath, Δos.O_RDONLY, 0);
            if (gotErr == nil) {
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
            }

            @string got = test.describeFile(tΔ1, f);

            switch (ᐧ) {
            case {} when test.target.isError(): {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when test.root != nil && test.target.escapes(): {
                test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
                break;
            }
            case {} when test.target.finalKind() == testFileAbsent: {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when test.target.anySlashSuffix(): {
                break;
            }
            default: {
                test.wantError(tΔ1, gotErr, default);
                @string want = targetˢ; if (got != want) {
                    tΔ1.Fatalf("opened file %q, want %q", got, want);
                }
                break;
            }}

            return (got, gotErr);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
        finally { ᒐ.Run(); }
    });
}

} // end os_test_package
