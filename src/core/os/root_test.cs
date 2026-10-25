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
    @string root = filepath.Join(Ꮡt.TempDir(), rootˢ);
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
    name: "slash after symlink to file"u8,
    fs: new @string[]{
        "link => ../ROOT/target"u8
    }.slice(),
    open: "link/"u8,
    target: "target"u8,
    wantError: true
), new(
    name: "slash after symlink to dir"u8,
    fs: new @string[]{
        "link => ../ROOT/target"u8,
        "target/"u8
    }.slice(),
    open: "link/"u8,
    wantError: true
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
    fs: new @string[]{
        "a/"u8
    }.slice(),
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
            // On Windows, the path is cleaned before symlink resolution.
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
                    tΔ1.Errorf(@"Dir.Open(%q): read content %q, %v; want %q"u8, testʗ1.open, (@string)got, err, (@string)wantʗ1);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object httpsGoDevIssue71492ˢ = (@string)"https://go.dev/issue/71492"u8;
internal static readonly @string rootChmodQˢ = "root.Chmod(%q)"u8;

public static void TestRootChmod(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skip("Chmod not supported on " + Δruntime.GOOS);
    }
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (target != ""u8) {
                // Create a file with no read/write permissions,
                // to ensure we can use Chmod on an inaccessible file.
                {
                    var errΔ1 = Δos.WriteFile(target, default!, 0); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            }
            if (Δruntime.GOOS == "windows"u8) {
                // On Windows, Chmod("symlink") affects the link, not its target.
                // See issue 71492.
                var (fi, errΔ2) = root.Lstat(testʗ1.open);
                if (errΔ2 == default! && !fi.Mode().IsRegular()) {
                    tΔ1.Skip(httpsGoDevIssue71492ˢ);
                }
            }
            var want = (fs.FileMode)438;
            var err = root.Chmod(testʗ1.open, want);
            if (errEndsTest(tΔ1, err, testʗ1.wantError, rootChmodQˢ, testʗ1.open)) {
                return;
            }
            (var st, err) = Δos.Stat(target);
            if (err != default!) {
                tΔ1.Fatalf("os.Stat(%q) = %v"u8, target, err);
            }
            {
                var got = st.Mode(); if (got != want) {
                    tΔ1.Errorf("after root.Chmod(%q, %v): file mode = %v, want %v"u8, testʗ1.open, want, got, want);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootChtimesQˢ = "root.Chtimes(%q)"u8;

internal partial struct TestRootChtimes_type /*dyn*/ {
    internal time.Time atime, mtime;
}

public static void TestRootChtimes(ж<Δtesting.T> Ꮡt) {
    // Don't check atimes if the fs is mounted noatime,
    // or on Plan 9 which does not permit changing atimes to arbitrary values.
    var checkAtimes = !hasNoatime() && Δruntime.GOOS != "plan9"u8;
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (target != ""u8) {
                {
                    var err = Δos.WriteFile(target, default!, 438); if (err != default!) {
                        tΔ1.Fatal(err);
                    }
                }
            }
            foreach (var (_, vᴛ2) in new TestRootChtimes_type[]{new(
                atime: time.Now().Add((time.Duration)(-60000000000L)),
                mtime: time.Now().Add((time.Duration)(-60000000000L))
            ), new(
                atime: time.Now().Add((time.Duration)(60000000000L)),
                mtime: time.Now().Add((time.Duration)(60000000000L))
            ), new(
                atime: new time.Time(nil),
                mtime: time.Now()
            ), new(
                atime: time.Now(),
                mtime: new time.Time(nil)
            )
            }.slice()) {
                var times = vᴛ2;

                var exprᴛ1 = Δruntime.GOOS;
                if (exprᴛ1 == "js"u8 || exprᴛ1 == "plan9"u8) {
                    times.atime = times.atime.Truncate(1 * time.ΔSecond);
                    times.mtime = times.mtime.Truncate(1 * time.ΔSecond);
                }
                else if (exprᴛ1 == "illumos"u8) {
                    times.atime = times.atime.Truncate(1 * time.Microsecond);
                    times.mtime = times.mtime.Truncate(1 * time.Microsecond);
                }

                var err = root.Chtimes(testʗ1.open, times.atime, times.mtime);
                if (errEndsTest(tΔ1, err, testʗ1.wantError, rootChtimesQˢ, testʗ1.open)) {
                    return;
                }
                (var st, err) = Δos.Stat(target);
                if (err != default!) {
                    tΔ1.Fatalf("os.Stat(%q) = %v"u8, target, err);
                }
                {
                    var got = st.ModTime(); if (!times.mtime.IsZero() && !got.Equal(times.mtime)) {
                        tΔ1.Errorf("after root.Chtimes(%q, %v, %v): got mtime=%v, want %v"u8, testʗ1.open, times.atime, times.mtime, got, times.mtime);
                    }
                }
                if (checkAtimes) {
                    {
                        var got = os_internal_test_package.Atime(st); if (!times.atime.IsZero() && !got.Equal(times.atime)) {
                            tΔ1.Errorf("after root.Chtimes(%q, %v, %v): got atime=%v, want %v"u8, testʗ1.open, times.atime, times.mtime, got, times.atime);
                        }
                    }
                }
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
internal static readonly @string rootMkdirAllQˢ = "root.MkdirAll(%q)"u8;

public static void TestRootMkdirAll(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        if (test.name == "directory does not exist"u8) {
            // Test expects error, mkdirall creates the missing directory.
            // TestRootMultiMkdirAll covers this case better anyway, just skip.
            continue;
        }
        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
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
            if (err != default!) {
                tΔ1.Fatalf(@"stat file created with Root.MkdirAll(%q): %v"u8, testʗ1.open, err);
            }
            if (!fi.IsDir()) {
                tΔ1.Fatalf(@"stat file created with Root.MkdirAll(%q): not a directory"u8, testʗ1.open);
            }
            {
                var mode = fi.Mode(); if ((fs.FileMode)(mode & 511) == 0) {
                    // Issue #73559: We're not going to worry about the exact
                    // mode bits (which will have been modified by umask),
                    // but there should be mode bits.
                    tΔ1.Fatalf(@"stat file created with Root.MkdirAll(%q): mode=%v, want non-zero"u8, testʗ1.open, mode);
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
internal static readonly @string rootRemoveAllQˢ = "root.RemoveAll(%q)"u8;

public static void TestRootRemoveAll(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (strings.HasSuffix(testʗ1.open, "/"u8)) {
                // The test is removing a file with a trailing /.
                // RemoveAll ignores trailing /s
                // If the file is a symlink, it will remove the symlink.
                @string fullname = filepath.Join(root.Name(), testʗ1.open);
                {
                    var (st, errΔ1) = Δos.Lstat(fullname); if (errΔ1 == default! && st.Mode().Type() == fs.ModeSymlink) {
                        testʗ1.ltarget = testʗ1.open;
                    }
                }
            }
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8){
                // Remove doesn't follow symlinks in the final path component,
                // so it will successfully remove ltarget.
                wantError = false;
                target = filepath.Join(root.Name(), testʗ1.ltarget);
            } else 
            if (target != ""u8) {
                {
                    var errΔ2 = Δos.Mkdir(target, 511); if (errΔ2 != default!) {
                        tΔ1.Fatal(errΔ2);
                    }
                }
                {
                    var errΔ3 = Δos.WriteFile(filepath.Join(target, fileˢ2), default!, 438); if (errΔ3 != default!) {
                        tΔ1.Fatal(errΔ3);
                    }
                }
            }
            var targetExists = true;
            {
                var (_, errΔ4) = root.Lstat(testʗ1.open); if (errors.Is(errΔ4, Δos.ErrNotExist)) {
                    // If the target doesn't exist, RemoveAll succeeds rather
                    // than returning ErrNotExist.
                    targetExists = false;
                    wantError = false;
                }
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
        var (r, err) = Δos.OpenRoot(target);
        if (err == default!) {
            r.Close();
            Ꮡt.Fatal(osOpenRootFileSucceededˢ);
        }
        (r, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        (var rr, err) = r.OpenRoot(targetˢ);
        if (err == default!) {
            rr.Close();
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootReadlinkQˢ = "root.Readlink(%q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ2 = "content"u8;

public static void TestRootReadlink(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string content = contentᶜ2;
            var wantError = testʗ1.wantError;
            if (testʗ1.ltarget != ""u8){
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
            if (err != default!) {
                tΔ1.Fatalf("os.Readlink(%q) = %v, want success"u8, testʗ1.ltarget, err);
            }
            if (got != want) {
                tΔ1.Errorf("root.Readlink(%q) = %q, want %q"u8, testʗ1.open, got, want);
            }
        });
    }
}

// TestRootRenameFrom tests renaming the test case target to a known-good path.
public static void TestRootRenameFrom(ж<Δtesting.T> Ꮡt) {
    testRootMoveFrom(Ꮡt, true);
}

// TestRootRenameFrom tests linking the test case target to a known-good path.
public static void TestRootLinkFrom(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(Ꮡt));
    testRootMoveFrom(Ꮡt, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string renameˢ2 = "Rename"u8;
internal static readonly @string linkˢ2 = "Link"u8;
internal static readonly @string rootVQQˢ = "root.%v(%q, %q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dstPathᶜ = "destination"u8;

internal static void testRootMoveFrom(ж<Δtesting.T> Ꮡt, bool rename) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            if (target != ""u8) {
                {
                    var errΔ1 = Δos.WriteFile(target, wantʗ1, 438); if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                }
            }
            var wantError = testʗ1.wantError;
            @string linkTarget = default!;
            if (testʗ1.ltarget != ""u8) {
                // Rename will rename the link, not the file linked to.
                wantError = false;
                error errΔ2 = default!;
                (linkTarget, errΔ2) = root.Readlink(testʗ1.ltarget);
                if (errΔ2 != default!) {
                    tΔ1.Fatalf("root.Readlink(%q) = %v, want success"u8, testʗ1.ltarget, errΔ2);
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
                    if (errΔ3 == default! && st.IsDir()) {
                        wantError = true;
                    }
                }
            }

            @string dstPath = dstPathᶜ;

            // Plan 9 doesn't allow cross-directory renames.
            if (Δruntime.GOOS == "plan9"u8 && strings.Contains(testʗ1.open, "/"u8)) {
                wantError = true;
            }

            @string op = default!;
            error err = default!;
            if (rename){
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
            if (rename){
                if (!errors.Is(err, Δos.ErrNotExist)) {
                    tΔ1.Errorf("after renaming file, Lstat(%q) = %v, want ErrNotExist"u8, origPath, err);
                }
            } else {
                if (err != default!) {
                    tΔ1.Errorf("after linking file, error accessing original: %v"u8, err);
                }
            }

            @string dstFullPath = filepath.Join(root.Name(), dstPath);
            if (testʗ1.ltarget != ""u8){
                var (got, errΔ1) = Δos.Readlink(dstFullPath);
                if (errΔ1 != default! || got != linkTarget) {
                    tΔ1.Errorf("os.Readlink(%q) = %q, %v, want %q"u8, dstFullPath, got, errΔ1, linkTarget);
                }
            } else {
                var (got, errΔ2) = Δos.ReadFile(dstFullPath);
                if (errΔ2 != default! || !bytes.Equal(got, wantʗ1)) {
                    tΔ1.Errorf(@"os.ReadFile(%q): read content %q, %v; want %q"u8, dstFullPath, (@string)got, errΔ2, (@string)wantʗ1);
                }
                (var st, errΔ2) = Δos.Lstat(dstFullPath);
                if (errΔ2 != default! || (fs.FileMode)(st.Mode() & fs.ModeSymlink) != 0) {
                    tΔ1.Errorf(@"os.Lstat(%q) = %v, %v; want non-symlink"u8, dstFullPath, st.Mode(), errΔ2);
                }
            }
        });
    }
}

// TestRootRenameTo tests renaming a known-good path to the test case target.
public static void TestRootRenameTo(ж<Δtesting.T> Ꮡt) {
    testRootMoveTo(Ꮡt, true);
}

// TestRootLinkTo tests renaming a known-good path to the test case target.
public static void TestRootLinkTo(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(Ꮡt));
    testRootMoveTo(Ꮡt, true);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcPathᶜ = "source"u8;

internal static void testRootMoveTo(ж<Δtesting.T> Ꮡt, bool rename) {
    var want = slice<byte>("target"u8);
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        var wantʗ1 = want;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
            @string srcPath = srcPathᶜ;
            {
                var errΔ1 = Δos.WriteFile(filepath.Join(root.Name(), srcPath), wantʗ1, 438); if (errΔ1 != default!) {
                    tΔ1.Fatal(errΔ1);
                }
            }

            if (Δruntime.GOOS == "windows"u8 && strings.HasSuffix(testʗ1.open, "/"u8)) {
                // Windows will ignore trailing slashes in the rename/link target.
                @string p = strings.TrimSuffix(testʗ1.open, "/"u8);
                var (st, errΔ2) = root.Lstat(p);
                if (errΔ2 == default! && st.Mode().Type() == fs.ModeSymlink) {
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
            if (Δruntime.GOOS == "plan9"u8 && strings.Contains(testʗ1.open, "/"u8)) {
                wantError = true;
            }

            error err = default!;
            @string op = default!;
            if (rename){
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
            if (rename){
                if (!errors.Is(err, Δos.ErrNotExist)) {
                    tΔ1.Errorf("after renaming file, Lstat(%q) = %v, want ErrNotExist"u8, srcPath, err);
                }
            } else {
                if (err != default!) {
                    tΔ1.Errorf("after linking file, error accessing original: %v"u8, err);
                }
            }

            (var got, err) = Δos.ReadFile(filepath.Join(root.Name(), target));
            if (err != default! || !bytes.Equal(got, wantʗ1)) {
                tΔ1.Errorf(@"os.ReadFile(%q): read content %q, %v; want %q"u8, target, (@string)got, err, (@string)wantʗ1);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootSymlinkQˢ = "root.Symlink(%q)"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantTargetᶜ = "linktarget"u8;

public static void TestRootSymlink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    foreach (var (_, vᴛ1) in rootTestCases) {
        ref var test = ref heap(new rootTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡtest.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string target, ж<Δos.Root> root) => {
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
            if (err != default! || got != wantTarget) {
                tΔ1.Fatalf("ReadLink(%q) = %q, %v; want %q, nil"u8, target, got, err, wantTarget);
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
    detailedErrorMismatch: (ж<Δtesting.T> t) => {
        // FreeBSD returns EPERM in the non-Root case.
        return Δruntime.GOOS == "freebsd"u8 && strings.HasPrefix(t.Name(), "TestRootConsistencyRemove"u8);
    }
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
    detailedErrorMismatch: (ж<Δtesting.T> t) => {
        // os.Create returns ENOTDIR or EISDIR depending on the platform.
        return Δruntime.GOOS == "js"u8;
    }
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
    open: "link/"u8,
    check: (ж<Δtesting.T> t) => {
        if (Δruntime.GOOS == "linux"u8 && strings.HasPrefix(t.Name(), "TestRootConsistencyRename/"u8)) {
            // Linux does not resolve "symlink" in rename("symlink/", "target").
            t.Skip((@string)"known inconsistency on linux"u8);
        }
        if (strings.HasPrefix(t.Name(), "TestRootConsistencyRemoveAll/"u8)) {
            // Root.RemoveAll and os.RemoveAll are not always consistent here.
            t.Skip((@string)"known inconsistency in RemoveAll"u8);
        }
    }
), new(
    name: "symlink slash dot"u8,
    fs: new @string[]{
        "target/file"u8,
        "link => target"u8
    }.slice(),
    open: "link/."u8
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
    name: "symlink to dir ends in slash"u8,
    fs: new @string[]{
        "dir/"u8,
        "link => dir/"u8
    }.slice(),
    open: "link"u8
), new(
    name: "symlink to file ends in slash"u8,
    fs: new @string[]{
        "file"u8,
        "link => file/"u8
    }.slice(),
    open: "link"u8
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
    detailedErrorMismatch: (ж<Δtesting.T> t) => {
        // On Windows, os.Root.Open returns "The directory name is invalid."
        // and os.Open returns "The file cannot be accessed by the system.".
        return Δruntime.GOOS == "windows"u8;
    },
    check: (ж<Δtesting.T> t) => {
        if (strings.HasPrefix(t.Name(), "TestRootConsistencyRemoveAll/"u8)) {
            var exprᴛ1 = Δruntime.GOOS;
            if (exprᴛ1 == "windows"u8) {
                // Root.RemoveAll notices that a/ is not a directory,
                // and returns success.
                // os.RemoveAll tries to open a/ and fails because
                // it is not a regular file.
                // The inconsistency here isn't worth fixing, so just skip this test.
                t.Skip((@string)"known inconsistency on windows"u8);
            }
            else if (exprᴛ1 == "js"u8) {
                // GOOS=js behavior varies with what the underlying OS is.
                t.Skip((@string)"known inconsistency with GOOS=js"u8);
            }
        }
    }
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
internal static readonly @string withRootˢ = "with root"u8;
internal static readonly @string withoutRootˢ = "without root"u8;

internal static void run(this rootConsistencyTest test, ж<Δtesting.T> Ꮡt, Func<ж<Δtesting.T>, @string, ж<Δos.Root>, (@string, error)> f) {
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
            if (testʗ1.check != default!) {
                testʗ1.check(tΔ1);
            }

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
                var err1ʗ1 = err1;
                error underlyingError(@string how, error errΔ1) {
                    switch (err1ʗ1.type()) {
                    case ж<osꓸPathError> e: {
                        return (~e).Err;
                    }
                    case ж<Δos.LinkError> e: {
                        return (~e).Err;
                    }
                    default: {
                        var e = err1ʗ1;
                        tΔ1.Fatalf("%v, expected PathError or LinkError; got: %v"u8, how, errΔ1);
                        break;
                    }}
                    return default!;
                }
                var e1 = underlyingError(withRootˢ, err1);
                var e2 = underlyingError(withoutRootˢ, err1);
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
                if (!detailedErrorMismatch && !AreEqual(e1, e2)) {
                    tΔ1.Errorf("with root:    err=%v"u8, e1);
                    tΔ1.Errorf("without root: err=%v"u8, e2);
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
                    return ((@string)b, errΔ1);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chmod0o555ˢ = "chmod 0o555"u8;
internal static readonly @string chmod0o777ˢ = "chmod 0o777"u8;

public static void TestRootConsistencyChmod(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skip("Chmod not supported on " + Δruntime.GOOS);
    }
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            Func<@string, fs.FileMode, error> chmod = Δos.Chmod;
            Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
            if (r != nil) {
                chmod = (@string p1, fs.FileMode p2) => r.Chmod(p1, p2);
                lstat = (@string p1) => r.Lstat(p1);
            }

            fs.FileMode m1 = default!;
            fs.FileMode m2 = default!;
            {
                var errΔ1 = chmod(path, 365); if (errΔ1 != default!) {
                    return (chmod0o555ˢ, errΔ1);
                }
            }
            var (fi, err) = lstat(path);
            if (err == default!) {
                m1 = fi.Mode();
            }
            {
                err = chmod(path, 511); if (err != default!) {
                    return (chmod0o777ˢ, err);
                }
            }
            (fi, err) = lstat(path);
            if (err == default!) {
                m2 = fi.Mode();
            }
            return (fmt.Sprintf("%v %v"u8, m1, m2), err);
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

public static void TestRootConsistencyMkdirAll(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default!;
            if (r == nil){
                err = Δos.MkdirAll(path, 511);
            } else {
                err = r.MkdirAll(path, 511);
            }
            return ("", err);
        });
    }
}

public static void TestRootConsistencyRemove(ж<Δtesting.T> Ꮡt) {
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

public static void TestRootConsistencyRemoveAll(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        if (test.open == "."u8 || test.open == "./"u8) {
            continue; // can't remove the root itself
        }
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            error err = default!;
            if (r == nil){
                err = Δos.RemoveAll(path);
            } else {
                err = r.RemoveAll(path);
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

public static void TestRootConsistencyReadlink(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            if (r == nil){
                return Δos.Readlink(path);
            } else {
                return r.Readlink(path);
            }
        });
    }
}

public static void TestRootConsistencyRename(ж<Δtesting.T> Ꮡt) {
    testRootConsistencyMove(Ꮡt, true);
}

public static void TestRootConsistencyLink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveLink(new os_test_package.testing_TжTB(Ꮡt));
    testRootConsistencyMove(Ꮡt, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object plan9DoesNotSupportCrossˢ = (@string)"Plan 9 does not support cross-directory renames"u8;
internal static readonly @string otherˢ = "other"u8;
internal static readonly @string statErrorˢ = "stat error"u8;

internal static void testRootConsistencyMove(ж<Δtesting.T> Ꮡt, bool rename) {
    if (Δruntime.GOOS == "plan9"u8) {
        // This test depends on moving files between directories.
        Ꮡt.Skip(plan9DoesNotSupportCrossˢ);
    }
    // Run this test in two directions:
    // Renaming the test path to a known-good path (from),
    // and renaming a known-good path to the test path (to).
    foreach (var (_, name) in new @string[]{"from"u8, "to"u8}.slice()) {
        Ꮡt.Run(name, (ж<Δtesting.T> tΔ1) => {
            foreach (var (_, test) in rootConsistencyTestCases) {
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
                    Func<@string, @string, error> move = default!;
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

                    @string srcPath = default!;
                    @string dstPath = default!;
                    if (name == "from"u8){
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
                        if (errΔ1 == default! && (fs.FileMode)(fiΔ1.Mode() & Δos.ModeSymlink) != 0) {
                            return ("", default!);
                        }
                    }

                    {
                        var errΔ2 = move(srcPath, dstPath); if (errΔ2 != default!) {
                            return ("", errΔ2);
                        }
                    }
                    var (fi, err) = lstat(dstPath);
                    if (err != default!) {
                        tΔ2.Errorf("stat(%q) after successful copy: %v"u8, dstPath, err);
                        return (statErrorˢ, err);
                    }
                    return (fmt.Sprintf("name:%q size:%v mode:%v isdir:%v"u8, fi.Name(), fi.Size(), fi.Mode(), fi.IsDir()), default!);
                });
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string targetᶜ1 = "linktarget"u8;

public static void TestRootConsistencySymlink(ж<Δtesting.T> Ꮡt) {
    testenv.MustHaveSymlink(new os_test_package.testing_TжTB(Ꮡt));
    foreach (var (_, test) in rootConsistencyTestCases) {
        test.run(Ꮡt, (ж<Δtesting.T> tΔ1, @string path, ж<Δos.Root> r) => {
            @string target = targetᶜ1;
            error err = default!;
            @string got = default!;
            if (r == nil){
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
            @string got = (@string)b;
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rootRemoveAllRemovedTheˢ = (@string)@"root.Remove(All)?(""."") removed the root"u8;

public static void TestRootRemoveDot(ж<Δtesting.T> Ꮡt) {
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
            var errΔ1 = root.Remove("."u8); if (errΔ1 == default!) {
                Ꮡt.Errorf(@"root.Remove(""."") = %v, want error"u8, errΔ1);
            }
        }
        {
            var errΔ2 = root.RemoveAll("."u8); if (errΔ2 == default!) {
                Ꮡt.Errorf(@"root.RemoveAll(""."") = %v, want error"u8, errΔ2);
            }
        }
        {
            var (_, errΔ3) = Δos.Stat(dir); if (errΔ3 != default!) {
                Ꮡt.Error(rootRemoveAllRemovedTheˢ);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string filenameˢ = "filename"u8;

public static void TestRootWriteReadFile(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string dir = Ꮡt.TempDir();
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        @string name = filenameˢ;
        var want = slice<byte>("file contents"u8);
        {
            var errΔ1 = root.WriteFile(name, want, 438); if (errΔ1 != default!) {
                Ꮡt.Fatalf("root.WriteFile(%q, %q, 0o666) = %v; want nil"u8, name, want, errΔ1);
            }
        }

        (var got, err) = root.ReadFile(name);
        if (err != default!) {
            Ꮡt.Fatalf("root.ReadFile(%q) = %q, %v; want %q, nil"u8, name, got, err, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wasmLacksFstatatˢ = (@string)"wasm lacks fstatat"u8;
internal static readonly @string readDirEntryˢ = "ReadDirEntry"u8;
internal static readonly @string readFileInfoˢ = "ReadFileInfo"u8;
internal static readonly @string readdirnamesˢ = "Readdirnames"u8;

// TestRootNoLstat verifies that we do not use lstat (possibly escaping the root)
// when reading directories in a Root.
public static void TestRootNoLstat(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(wasmLacksFstatatˢ);
        }

        @string dir = makefs(Ꮡt, new @string[]{
            "subdir/"u8
        }.slice());
        UntypedInt size = 42;
        @string contents = strings.Repeat("x"u8, size);
        {
            var errΔ1 = Δos.WriteFile(dir + "/subdir/file"u8, slice<byte>(contents), 438); if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
        }
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);

        var rootʗ2 = root;
        void test(@string name, Action<ж<Δtesting.T>, ж<Δos.File>> fn) {
            var rootʗ3 = rootʗ2;
            Ꮡt.Run(name, (ж<Δtesting.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    os_internal_test_package.SetStatHook(new os_test_package.testing_Tжcleanuper(tΔ1), (ж<Δos.File> fΔ1, @string nameΔ1) => {
                        if (fΔ1 == nil) {
                            tΔ1.Errorf("unexpected Lstat(%q)"u8, nameΔ1);
                        }
                        return (default!, default!);
                    });
                    var (f, errΔ2) = rootʗ3.Open(subdirˢ);
                    if (errΔ2 != default!) {
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
                    tΔ2.Errorf("FileInfo.Name() = %q, want %q"u8, got, want);
                }
            }
            {
                var (got, want) = (fi.Size(), (int64)size); if (got != want) {
                    tΔ2.Errorf("FileInfo.Size() = %v, want %v"u8, got, want);
                }
            }
        }
        var checkFileInfoʗ1 = checkFileInfo;
        [MethodImpl(MethodImplOptions.NoInlining)] void checkDirEntry(ж<Δtesting.T> tΔ3, fs.DirEntry d) {
            tΔ3.Helper();
            {
                @string got = d.Name();
                @string want = fileˢ2; if (got != want) {
                    tΔ3.Errorf("DirEntry.Name() = %q, want %q"u8, got, want);
                }
            }
            {
                var (got, want) = (d.IsDir(), false); if (got != want) {
                    tΔ3.Errorf("DirEntry.IsDir() = %v, want %v"u8, got, want);
                }
            }
            var (fi, errΔ3) = d.Info();
            if (errΔ3 != default!) {
                tΔ3.Fatalf("DirEntry.Info() = _, %v"u8, errΔ3);
            }
            checkFileInfoʗ1(tΔ3, fi);
        }

        test(statˢ, (ж<Δtesting.T> tΔ4, ж<Δos.File> subdir) => {
            var (fi, errΔ4) = subdir.Stat();
            if (errΔ4 != default!) {
                tΔ4.Fatal(errΔ4);
            }
            if (!fi.IsDir()) {
                tΔ4.Fatalf(@"Open(""subdir"").Stat().IsDir() = false, want true"u8);
            }
        });
        // File.ReadDir, returning []DirEntry
        var checkDirEntryʗ1 = checkDirEntry;
        test(readDirEntryˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ5, ж<Δos.File> subdir) => {
            var (dirents, errΔ5) = subdir.ReadDir(-1);
            if (errΔ5 != default!) {
                tΔ5.Fatal(errΔ5);
            }
            if (len(dirents) != 1) {
                tΔ5.Fatalf(@"Open(""subdir"").ReadDir(-1) = {%v}, want {file}"u8, dirents);
            }
            checkDirEntryʗ1(tΔ5, dirents[0]);
        });
        // File.Readdir, returning []FileInfo
        var checkFileInfoʗ2 = checkFileInfo;
        test(readFileInfoˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ6, ж<Δos.File> subdir) => {
            var (fileinfos, errΔ6) = subdir.Readdir(-1);
            if (errΔ6 != default!) {
                tΔ6.Fatal(errΔ6);
            }
            if (len(fileinfos) != 1) {
                tΔ6.Fatalf(@"Open(""subdir"").Readdir(-1) = {%v}, want {file}"u8, fileinfos);
            }
            checkFileInfoʗ2(tΔ6, fileinfos[0]);
        });
        // File.Readdirnames, returning []string
        test(readdirnamesˢ, (ж<Δtesting.T> tΔ7, ж<Δos.File> subdir) => {
            var (names, errΔ7) = subdir.Readdirnames(-1);
            if (errΔ7 != default!) {
                tΔ7.Fatal(errΔ7);
            }
            {
                var (got, want) = (names, new @string[]{"file"u8}.slice()); if (!slices.Equal<slice<@string>, @string>(got, want)) {
                    tΔ7.Fatalf(@"Open(""subdir"").Readdirnames(-1) = %q, want %q"u8, got, want);
                }
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

internal static ж<bool> testVerbose = flag.Bool("verbose"u8, false, "verbose"u8);

// A rootMultiTest function may return this error to disable
// the check that in-root and out-of-root functions have the same outcome.
internal static error errSkipRootConsistencyCheck = errors.New("skip root consistency check"u8);

// runRootMultiTest runs f in a variety of configurations.
// See above.
internal static void runRootMultiTest(ж<Δtesting.T> Ꮡt, Func<ж<Δtesting.T>, ж<rootMultiTest>, (@string, error)> f) {
    foreach (var iᴛ1 in range<testFileDesc>(allTestFileDescs().Invoke)) {
        ref var target = ref heap(new testFileDesc(), out var Ꮡtarget);
        target = iᴛ1;

        var targetʗ1 = target;
        Ꮡt.Run(target.String(), (ж<Δtesting.T> tΔ1) => {
            testFileDesc source = default!;                    // unused
            runRootMultiTestDescs(tΔ1, source, targetʗ1, f);
        });
    }
}

// runRootMultiTest2 runs f in a variety of configurations,
// with both source and target files.
// See above.
internal static void runRootMultiTest2(ж<Δtesting.T> Ꮡt, Func<ж<Δtesting.T>, ж<rootMultiTest>, (@string, error)> f) {
    // A "simple" desc is one which contains only direct references.
    // When not running the comprehensive (but slow) set of test variations,
    // we only test variations where at least one of source and target is simple.
    bool isSimple(testFileDesc desc) {
        if (desc.@ref.template != "BASE"u8) {
            return false;
        }
        if (desc.kind.isLink() && (~desc.target).@ref.template != "BASE"u8) {
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
            Ꮡt.Run(name, (ж<Δtesting.T> tΔ1) => {
                runRootMultiTestDescs(tΔ1, sourceʗ1, targetʗ1, f);
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootˢ2 = "root."u8;

// setOp sets the operation performed by the test (logged in errors).
//
// This currently assumes the operation will be a method of os.Root and a function in os
// (e.g., root.Open/os.Open).
internal static void setOp(this ref rootMultiTest test, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    if (test.root != nil){
        test.op = rootˢ2;
    } else {
        test.op = "os."u8;
    }
    test.op += fmt.Sprintf(format, a.ꓸꓸꓸ);
}

internal static error errAny = errors.New("any error"u8);

internal static partial void errorf(this ref rootMultiTest test, ж<Δtesting.T> Ꮡt, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Errorf("%v:"u8, test.op);
    Ꮡt.Fatalf("  "u8 + format, args.ꓸꓸꓸ);
}

// wantError tests whether got matches want.
// If want is errAny, got may be any non-nil error.
internal static void wantError(this ref rootMultiTest test, ж<Δtesting.T> Ꮡt, error got, error want) {
    Ꮡt.Helper();
    if (errors.Is(got, want) || (got != default! && AreEqual(want, errAny))) {
        return;
    }
    Ꮡt.Fatalf("%v:\ngot error:  %v\nwant error: %v"u8, test.op, got, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object initialDirectoryContentsˢ = (@string)"Initial directory contents:"u8;

internal static void runRootMultiTestDescs(ж<Δtesting.T> Ꮡt, testFileDesc source, testFileDesc target, Func<ж<Δtesting.T>, ж<rootMultiTest>, (@string, error)> f) {
    var rootTest = newRootTest(Ꮡt, source, target, true);
    var osTest = newRootTest(Ꮡt, source, target, false);

    var initialContent = dirTreeContents(Ꮡt, (~rootTest).dir);
    var initialContentʗ1 = initialContent;
    Ꮡt.Cleanup(() => {
        if (Ꮡt.Failed()) {
            Ꮡt.Log(initialDirectoryContentsˢ);
            foreach (var (_, line) in initialContentʗ1) {
                Ꮡt.Logf("  %v"u8, line);
            }
        }
    });

    var (rootResult, rootErr) = f(Ꮡt, rootTest);

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
        if ((~rootTest).source.anySlashSuffix() || (~rootTest).target.anySlashSuffix()) {
            return;
        }
    }

    if (Δruntime.GOOS == "wasip1"u8 || Δruntime.GOOS == "js"u8) {
        // WASI runtimes don't have any consistent behavior for handling paths with
        // a trailing /, so skip consistency tests for these paths.
        if ((~rootTest).source.anySlashSuffix() || (~rootTest).target.anySlashSuffix()) {
            return;
        }
    }

    var (osResult, osErr) = f(Ꮡt, osTest);

    var initialContentʗ2 = initialContent;
    var osErrʗ1 = osErr;
    var osTestʗ1 = osTest;
    var rootErrʗ1 = rootErr;
    var rootTestʗ1 = rootTest;
    Ꮡt.Cleanup(() => {
        if (Ꮡt.Failed() || !testVerbose.Value) {
            return;
        }
        var rootContentΔ1 = dirTreeContents(Ꮡt, (~rootTestʗ1).dir);
        var osContentΔ1 = dirTreeContents(Ꮡt, (~osTestʗ1).dir);
        Ꮡt.Log(initialDirectoryContentsˢ);
        foreach (var (_, line) in initialContentʗ2) {
            Ꮡt.Logf("  %v"u8, line);
        }
        Ꮡt.Logf("%v:"u8, (~rootTestʗ1).op);
        Ꮡt.Logf("  result: %v"u8, rootResult);
        Ꮡt.Logf("  error: %v"u8, rootErrʗ1);
        foreach (var (_, line) in rootContentΔ1) {
            Ꮡt.Logf("  %v"u8, line);
        }
        Ꮡt.Logf("%v:"u8, (~osTestʗ1).op);
        Ꮡt.Logf("  result: %v"u8, osResult);
        Ꮡt.Logf("  error: %v"u8, osErrʗ1);
        foreach (var (_, line) in osContentΔ1) {
            Ꮡt.Logf("  %v"u8, line);
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
        Ꮡt.Errorf("inconsistent results in/out of root"u8);
        Ꮡt.Errorf("%v:"u8, (~rootTest).op);
        Ꮡt.Errorf("  result: %v"u8, rootResult);
        Ꮡt.Errorf("%v:"u8, (~osTest).op);
        Ꮡt.Errorf("  result: %v"u8, osResult);
    }
    if ((rootErr == default!) != (osErr == default!)) {
        Ꮡt.Errorf("inconsistent errors in/out of root"u8);
        Ꮡt.Errorf("%v:"u8, (~rootTest).op);
        Ꮡt.Errorf("  error: %v"u8, rootErr);
        Ꮡt.Errorf("%v:"u8, (~osTest).op);
        Ꮡt.Errorf("  error: %v"u8, osErr);
    }

    // Filesystem consistency check: Same files in the same places.
    var rootContent = dirTreeContents(Ꮡt, (~rootTest).dir);
    var osContent = dirTreeContents(Ꮡt, (~osTest).dir);
    if (!slices.Equal<slice<@string>, @string>(rootContent, osContent)) {
        Ꮡt.Errorf("inconsistent filesystem after running in/out of root"u8);
        Ꮡt.Errorf("%v:"u8, (~rootTest).op);
        foreach (var (_, line) in rootContent) {
            Ꮡt.Errorf("  %v"u8, line);
        }
        Ꮡt.Errorf("%v:"u8, (~osTest).op);
        foreach (var (_, line) in osContent) {
            Ꮡt.Errorf("  %v"u8, line);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sourceˢ = "source"u8;

internal static ж<rootMultiTest> newRootTest(ж<Δtesting.T> Ꮡt, testFileDesc source, testFileDesc target, bool inRoot) {
    ref var dir = ref heap<@string>(out var Ꮡdir);
    dir = makefs(Ꮡt, new @string[]{
        "DIR/"u8
    }.slice());
    ж<Δos.Root> root = default!;
    if (inRoot) {
        error err = default!;
        (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        Ꮡt.Cleanup(() => {
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
        @string path = default!;
        fs.FileInfo fi = default!;
        if (desc.kind == testFileUnused) {
            return ("", default!);
        }
        fi = desc.create(Ꮡt, dirʗ1, name, name);
        path = desc.@ref.path(dirʗ1, name);
        if (!inRoot && !filepath.IsAbs(path)) {
            path = dirʗ1 + "/"u8 + path;
        }
        return (path, fi);
    }
    (test.Value.sourcePath, test.Value.sourceInfo) = createFile(sourceˢ, source);
    (test.Value.targetPath, test.Value.targetInfo) = createFile(targetˢ, target);
    return test;
}

// testFileKind is a kind of file.
partial struct testFileKind /*num:nint*/;

internal static testFileKind testFileUnused => /* testFileKind(iota) */ 0;
internal static testFileKind testFileAbsent => 1; // file does not exist
internal static testFileKind testFileFile => 2; // regular file
internal static testFileKind testFileDir => 3; // directory
internal static testFileKind testFileSymlink => 4; // symlink
internal static testFileKind testFileJunction => 5; // Windows directory junction
internal static testFileKind testFileMax => 6;

// testFileError represents a path which fails during resolution,
// such as "a/b" where "a" does not exist.
internal static testFileKind testFileError => 7;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unusedˢ = "unused"u8;
internal static readonly @string absentˢ = "absent"u8;
internal static readonly @string errorˢ2 = "error"u8;

internal static @string String(this testFileKind kind) {
    var exprᴛ1 = kind;
    if (exprᴛ1 == testFileUnused) {
        return unusedˢ;
    }
    if (exprᴛ1 == testFileAbsent) {
        return absentˢ;
    }
    if (exprᴛ1 == testFileFile) {
        return fileˢ2;
    }
    if (exprᴛ1 == testFileDir) {
        return dirˢ;
    }
    if (exprᴛ1 == testFileSymlink) {
        return symlinkˢ;
    }
    if (exprᴛ1 == testFileJunction) {
        return junctionˢ;
    }
    if (exprᴛ1 == testFileError) {
        return errorˢ2;
    }
    { /* default: */
        return fmt.Sprintf("testFileKind(%d)"u8, kind);
    }
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

internal static slice<testFileRef> testFileRefs = new testFileRef[]{
    new(escapes: false, name: "b"u8, template: "BASE"u8),
    new(escapes: false, name: "bs"u8, template: "BASE/"u8),
    new(escapes: false, name: "dpb"u8, template: "DIR/../BASE"u8),
    new(escapes: false, name: "dpbs"u8, template: "DIR/../BASE/"u8),
    new(escapes: true, name: "prb"u8, template: "../ROOT/BASE"u8),
    new(escapes: true, name: "prbs"u8, template: "../ROOT/BASE/"u8),
    new(escapes: true, name: "srb"u8, template: "/ROOT/BASE"u8),
    new(escapes: true, name: "srbs"u8, template: "/ROOT/BASE/"u8)
}.slice();

// testFileLimitedRefs is a smaller set of references which do not exercise path escapes
// (see allTestFileDescs).
internal static slice<testFileRef> testFileLimitedRefs = testFileRefs[0..2];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string baseˢ = "BASE"u8;
internal static readonly @string rootˢ3 = "/ROOT"u8;

// path creates a path using the template.
//
// dir is the absolute path to the root directory (which must be named "ROOT").
// base is the name of the target file within the root directory.
internal static @string path(this testFileRef @ref, @string dir, @string @base) {
    @string p = @ref.template;
    p = strings.ReplaceAll(p, baseˢ, @base);
    {
        var (trim, ok) = strings.CutPrefix(p, rootˢ3); if (ok) {
            p = dir + trim;
        }
    }
    return p;
}

// hasSlashSuffix reports whether the file reference ends in a /.
internal static bool hasSlashSuffix(this testFileRef @ref) {
    return strings.HasSuffix(@ref.template, "/"u8);
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

internal static ж<bool> rootComprehensive = flag.Bool("root_comprehensive"u8, false,
    "run many more os.Root test variations (slow, uncertain value)"u8);

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
        foreach (var (_, @ref) in testFileRefs) {
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
        foreach (var (_, kind) in new testFileKind[]{testFileSymlink, testFileJunction}.slice()) {
            if (kind == testFileJunction && Δruntime.GOOS != "windows"u8) {
                continue;
            }
            foreach (var (_, @ref) in refs) {
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
                    foreach (var (_, vᴛ1) in linkRefs) {
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
        s += (~desc.target).String();
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
        return (~desc.target).escapes();
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
        return (~desc.target).escapes();
    }
    return false;
}

// finalKind reports the kind of the file after following all symlinks.
internal static testFileKind finalKind(this testFileDesc desc) {
    if (desc.kind.isLink()) {
        return (~desc.target).finalKind();
    }
    return desc.kind;
}

internal static testFileKind lfinalKind(this testFileDesc desc) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        if (desc.@ref.hasSlashSuffix() && desc.kind.isLink() && (~desc.target).kind != testFileDir) {
            return testFileError;
        }
    }
    else { /* default: */
        if (desc.@ref.hasSlashSuffix() && desc.kind.isLink()) {
            return (~desc.target).finalKind();
        }
    }
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
        var exprᴛ1 = descΔ1.kind;
        if (exprᴛ1 == testFileDir) {
            return false;
        }
        if (exprᴛ1 == testFileSymlink) {
            if (Δruntime.GOOS == "windows"u8 && hasSuffix && (~descΔ1.target).kind != testFileDir) {
                return true;
            }
            return ᏑisError.ValueSlot(descΔ1.target.Value, hasSuffix);
        }
        if (exprᴛ1 == testFileJunction) {
            if ((~descΔ1.target).kind == testFileFile || (hasSuffix && (~descΔ1.target).kind != testFileDir)) {
                return true;
            }
            return ᏑisError.ValueSlot(descΔ1.target.Value, hasSuffix);
        }
        { /* default: */
            return hasSuffix;
        }
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
        return (~desc.target).anySlashSuffix();
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
internal static fs.FileInfo /*fi*/ create(this testFileDesc desc, ж<Δtesting.T> Ꮡt, @string dir, @string @base, @string token) {
    fs.FileInfo fi = default!;
    ref var t = ref Ꮡt.DerefOrNull();

    @string path = filepath.Join(dir, @base);
    var exprᴛ1 = desc.kind;
    if (exprᴛ1 == testFileAbsent) {
        // File does not exist.
    }
    else if (exprᴛ1 == testFileFile) {
        // Regular file. We use the token as the file contents.
        {
            var err = Δos.WriteFile(path, slice<byte>(token), 438); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
    }
    else if (exprᴛ1 == testFileDir) {
        // Directory. We create a subdir within the directory named "c_"+token.
        // (The "c_" prefix is to distinguish this subdir from any files that may
        // have the same name as the token.)
        {
            var err = Δos.Mkdir(path, 511); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
    }
    else if (exprᴛ1 == testFileSymlink) {
        // Symlink. We create a symlink target named "s_"+base.
        if (Δruntime.GOOS == "plan9"u8) {
            Ꮡt.Skip("symlinks not supported on " + Δruntime.GOOS);
        }
        @string linktarget = (~desc.target).@ref.path(dir, "s_"u8 + @base);
        if (Δruntime.GOOS == "wasip1"u8 && filepath.IsAbs(linktarget)) {
            Ꮡt.Skip("absolute link targets not supported on " + Δruntime.GOOS);
        }
        fi = (~desc.target).create(Ꮡt, dir, "s_"u8 + @base, token);
        {
            var err = Δos.Symlink(linktarget, path); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
    }
    else if (exprᴛ1 == testFileJunction) {
        // Directory junction. We create a target named "s_"+base.
        if (Δruntime.GOOS != "windows"u8) {
            Ꮡt.Skip("junctions not supported on " + Δruntime.GOOS);
        }
        @string linktarget = (~desc.target).@ref.path(dir, "s_"u8 + @base);
        fi = (~desc.target).create(Ꮡt, dir, "s_"u8 + @base, token);
        createJunction(Ꮡt, path, linktarget);
    }
    else { /* default: */
        Ꮡt.Fatalf("can't create file of kind: %v"u8, desc.kind);
    }
    if (desc.kind == testFileFile || desc.kind == testFileDir) {
        error err = default!;
        (fi, err) = Δos.Lstat(path);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    return fi;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownFileˢ = "unknown file"u8;

// testRootDescribeFile returns a string identifying a file.
//
// It returns "" if f is nil.
// It returns "source" or "target" if f is the source or target file in the test.
// Otherwise, it returns "unknown file".
internal static @string describeFile(this ref rootMultiTest test, ж<Δtesting.T> Ꮡt, ж<Δos.File> Ꮡf) {
    if (Ꮡf == nil) {
        return ""u8;
    }
    var (fi, err) = Ꮡf.Stat();
    if (err != default!) {
        Ꮡt.Fatal(err);
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
internal static slice<@string> /*contents*/ dirTreeContents(ж<Δtesting.T> Ꮡt, @string dir) {
    slice<@string> contents = default!;
    GoFrame ᒐ = default;
    try {
        var (root, err) = Δos.OpenRoot(dir);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var rootʗ1 = root;
        defer(() => rootʗ1.Close(), ref ᒐ);
        var rootʗ2 = root;
        fs.WalkDir(root.FS(), "."u8, (@string path, fs.DirEntry d, error errΔ1) => {
            if (path == "."u8) {
                return default!;
            }
            (var info, errΔ1) = d.Info();
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            @string ent = info.Mode().String() + " "u8 + path;
            var exprᴛ1 = d.Type();
            if (exprᴛ1 == fs.ModeDir) {
                ent += "/"u8;
            }
            else if (exprᴛ1 == fs.ModeSymlink || exprᴛ1 == fs.ModeIrregular) {
                var (target, errΔ5) = rootʗ2.Readlink(path);
                if (errΔ5 != default!) {
                    Ꮡt.Fatal(errΔ5);
                }
                if (filepath.IsAbs(target)) {
                    var (relPath, errΔ6) = filepath.Rel(dir, target);
                    if (errΔ6 == default! && filepath.IsLocal(relPath)) {
                        target = "/.../"u8 + relPath;
                    }
                }
                ent += " => "u8 + target;
            }
            else { /* default: */
                var (f, errΔ7) = rootʗ2.Open(path);
                if (errΔ7 != default!){
                    ent += " (unreadable)"u8;
                } else {
                    var (content, errΔ8) = Δio.ReadAll(new os_test_package.os_FileжReader(f));
                    if (errΔ8 != default!) {
                        Ꮡt.Fatal(errΔ8);
                    }
                    ent += fmt.Sprintf(" %q"u8, content);
                }
            }
            contents = append(contents, ent);
            return default!;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return contents;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openQˢ = "Open(%q)"u8;

// TestRootMultiOpen tests os.Root.Open.
//
// This also serves as a prototypical example of using rootMultiTest
// (see also the doc comment on rootMultiTest above).
public static void TestRootMultiOpen(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
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
            if ((~test).root != nil) {
                var recvʗ1 = (~test).root;
                open = (@string p1) => recvʗ1.Open(p1);
            }

            test.setOp(openQˢ, (~test).targetPath); // test's operation, for errors
            var (f, gotErr) = open((~test).targetPath);
            if (gotErr == default!) {
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
            case {} when (~test).target.isError(): {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when (~test).root != nil && (~test).target.escapes(): {
                // The operation escapes the root.
                test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
                break;
            }
            case {} when (~test).target.finalKind() == testFileAbsent: {
                // The file does not exist ("absent").
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when (~test).target.anySlashSuffix(): {
                // The file name or a symlink target contain a trailing slash.
                // Trailing slashes are handled differently on different platforms,
                // so we won't try to assert an outcome when they are present.
                // runRootMultiTest will verify that root.Open and os.Open
                // produce consistent results.
                break;
            }
            default: {
                // We should have successfully opened the file.
                test.wantError(tΔ1, gotErr, default!);
                {
                    @string want = targetˢ; if (got != want) {
                        tΔ1.Fatalf("opened file %q, want %q"u8, got, want);
                    }
                }
                break;
            }}

            // Return the name of the file opened (possibly "" for nothing) and the error.
            // runRootMultiTest will compare the results for in-a-root and out-of-a-root
            // to validate that they are the same.
            return (got, gotErr);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chmodQOˢ = "Chmod(%q, %o)"u8;

public static void TestRootMultiChmod(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "wasip1"u8) {
        Ꮡt.Skip("Chmod not supported on " + Δruntime.GOOS);
    }
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        ref var chmod = ref heap<Func<@string, fs.FileMode, error>>(out var Ꮡchmod);

        chmod = Δos.Chmod;
        Func<@string, (fs.FileInfo, error)> stat = Δos.Stat;
        Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            Ꮡchmod.ValueSlot = (@string p1, fs.FileMode p2) => recvʗ1.Chmod(p1, p2);
            var recvʗ2 = (~test).root;
            stat = (@string p1) => recvʗ2.Stat(p1);
            var recvʗ3 = (~test).root;
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
            Ꮡchmod.ValueSlot((~test).targetPath, 448);
        });

        test.setOp(chmodQOˢ, (~test).targetPath, wantMode);
        var gotErr = Ꮡchmod.ValueSlot((~test).targetPath, wantMode);

        var escapes = (~test).target.escapes();
        testFileKind targetKind = (~test).target.finalKind();
        if (Δruntime.GOOS == "windows"u8) {
            // On Windows, Chmod("symlink") affects the link, not its target.
            // See issue #71492.
            stat = lstat;
            escapes = test.Value.target.@ref.escapes;
            targetKind = test.Value.target.kind;
        }

        fs.FileMode gotMode = default!;
        switch (ᐧ) {
        case {} when (~test).root != nil && escapes: {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when targetKind == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            // Don't expect anything, just be consistent with the OS.
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);

            var (fi, err) = stat((~test).targetPath);
            if (err != default!) {
                tΔ1.Fatalf("could not stat target: %v"u8, err);
            }
            if (Δruntime.GOOS == "windows"u8 && !fi.Mode().IsRegular()) {
                // See issue #71492.
                break;
            }

            gotMode = (fs.FileMode)(fi.Mode() & fs.ModePerm);
            if (gotMode != wantMode) {
                tΔ1.Fatalf("file %q:\ngot mode:  %v\nwant mode: %v"u8, (~test).targetPath, gotMode, wantMode);
            }
            break;
        }}

        if (Δruntime.GOOS == "windows"u8 && (~test).root == nil && gotErr != default!) {
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
internal static readonly @string createQˢ = "Create(%q)"u8;

public static void TestRootMultiCreate(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        GoFrame ᒐ = default;
        try {
            Func<@string, (ж<Δos.File>, error)> create = Δos.Create;
            if ((~test).root != nil) {
                var recvʗ1 = (~test).root;
                create = (@string p1) => recvʗ1.Create(p1);
            }

            test.setOp(createQˢ, (~test).targetPath); // test's operation, for errors
            var (f, gotErr) = create((~test).targetPath);
            if (gotErr == default!) {
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
            }

            switch (ᐧ) {
            case {} when (~test).target.isError(): {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when Δruntime.GOOS == "windows"u8 && (~test).target.isSymlinkToDir(): {
                // The error here is because the link is a Windows directory link,
                // not because the link target is a directory.
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when (~test).root != nil && (~test).target.escapes(): {
                // The operation escapes the root.
                test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
                break;
            }
            default: {
                break;
            }}

            return ("", gotErr);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testSeemsToTickleAˢ = (@string)"test seems to tickle a wasmtime bug"u8;
internal static readonly @string linkQQˢ = "Link(%q, %q)"u8;

public static void TestRootMultiLink(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "wasip1"u8) {
        var exprᴛ1 = Δos.Getenv(gowasiruntimeˢ);
        if (exprᴛ1 == ""u8 || exprᴛ1 == "wasmtime"u8) {
            // This test fails when run with wasmtime, because os.RemoveAll fails
            // to remove the test tempdir.
            Ꮡt.Skip(testSeemsToTickleAˢ);
        }
    }
    testenv.MustHaveLink(new os_test_package.testing_TжTB(Ꮡt));
    runRootMultiTest2(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, @string, error> rename = Δos.Link;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            rename = (@string p1, @string p2) => recvʗ1.Link(p1, p2);
        }

        test.setOp(linkQQˢ, (~test).sourcePath, (~test).targetPath);
        var gotErr = rename((~test).sourcePath, (~test).targetPath);

        switch (ᐧ) {
        case {} when (~test).root != nil && (~test).source.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).source.lfinalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).source.kind.isLink(): {
            // os.Link(old, new) may or may not deference old when it is a symlink.
            // POSIX says that link(2) should deference the source, but implementations
            // are inconsistent.
            return ("", errSkipRootConsistencyCheck);
        }
        case {} when (~test).source.slashSuffix() && (~test).source.lfinalKind() != testFileDir: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lstatQˢ = "Lstat(%q)"u8;

public static void TestRootMultiLstat(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (fs.FileInfo, error)> lstat = Δos.Lstat;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            lstat = (@string p1) => recvʗ1.Lstat(p1);
        }

        test.setOp(lstatQˢ, (~test).targetPath);
        var (gotStat, gotErr) = lstat((~test).targetPath);

        @string result = ""u8;
        if (gotStat != default!) {
            result = gotStat.Mode().String();
        }

        var escapes = (~test).target.lescapes();
        testFileKind finalKind = (~test).target.lfinalKind();
        if (Δruntime.GOOS == "windows"u8 && (~test).target.@ref.hasSlashSuffix()) {
            // When the target of lstat has a trailing slash,
            // Windows follows it.
            escapes = (~test).target.escapes();
            finalKind = (~test).target.finalKind();
        }

        switch (ᐧ) {
        case {} when (~test).root != nil && escapes: {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.kind == testFileAbsent: {
            // Target does not exist.
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when finalKind == testFileSymlink: {
            test.wantError(tΔ1, gotErr, default!);
            {
                var (got, want) = (gotStat.Mode().Type(), fs.ModeSymlink); if (got != want) {
                    test.errorf(tΔ1, "got mode %v, want %v"u8, got, want);
                }
            }
            break;
        }
        case {} when finalKind == testFileJunction: {
            test.wantError(tΔ1, gotErr, default!);
            {
                var (got, want) = (gotStat.Mode().Type(), fs.ModeIrregular); if (got != want) {
                    test.errorf(tΔ1, "got mode %v, want %v"u8, got, want);
                }
            }
            break;
        }
        case {} when gotErr != default!: {
            break;
        }
        default: {
            if (!Δos.SameFile(gotStat, (~test).targetInfo)) {
                test.errorf(tΔ1, "stat result is not for target file; want it to be"u8);
            }
            break;
        }}

        return (result, gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mkdirQ0o777ˢ = "Mkdir(%q, 0o777)"u8;

public static void TestRootMultiMkdir(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, fs.FileMode, error> mkdir = Δos.Mkdir;
        Func<@string, (fs.FileInfo, error)> stat = Δos.Stat;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            mkdir = (@string p1, fs.FileMode p2) => recvʗ1.Mkdir(p1, p2);
            var recvʗ2 = (~test).root;
            stat = (@string p1) => recvʗ2.Stat(p1);
        }

        test.setOp(mkdirQ0o777ˢ, (~test).targetPath);
        var gotErr = mkdir((~test).targetPath, 511);

        switch (ᐧ) {
        case {} when (~test).root != nil && (~test).target.@ref.escapes: {
            // "mkdir ../target", or equivalent escaping path.
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.slashSuffix() && (~test).target.kind.isLink(): {
            // "mkdir symlink/", inconsistent behavior across platforms
            // as to whether this follows the symlink or not.
            //
            // If the symlink escapes, this needs to be some kind of error though.
            if ((~test).root != nil && (~test).target.escapes()) {
                test.wantError(tΔ1, gotErr, errAny);
            }
            if (Δruntime.GOOS == "openbsd"u8) {
                // Known inconsistency: OpenBSD doesn't resolve the final
                // symlink when creating a directory.
                return ("", errSkipRootConsistencyCheck);
            }
            break;
        }
        case {} when (~test).target.kind != testFileAbsent: {
            // "mkdir target", where target exists.
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            var (fi, err) = stat((~test).targetPath);
            if (err != default!) {
                tΔ1.Fatalf("could not stat target: %v"u8, err);
            }
            if (!fi.IsDir()) {
                tΔ1.Fatalf("%q: not a directory, expected it to be"u8, (~test).targetPath);
            }
            break;
        }}
        return ("", gotErr);
    });
}

public static void TestRootMultiMkdirAllShallow(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => testRootMultiMkdirAll(tΔ1, test, (~test).targetPath));
}

public static void TestRootMultiMkdirAllDeep(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        @string targetPath = test.Value.targetPath;
        if (len(targetPath) > 0 && Δos.IsPathSeparator(targetPath[len(targetPath) - 1])){
            targetPath += "a/b/"u8;
        } else {
            targetPath += "/a/b"u8;
        }
        return testRootMultiMkdirAll(tΔ1, test, targetPath);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mkdirAllQ0o777ˢ = "MkdirAll(%q, 0o777)"u8;

internal static (@string, error) testRootMultiMkdirAll(ж<Δtesting.T> Ꮡt, ж<rootMultiTest> Ꮡtest, @string targetPath) {
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
        test.wantError(Ꮡt, gotErr, os_internal_test_package.ErrPathEscapes);
        break;
    }
    case {} when test.root != nil && test.target.escapes(): {
        // "mkdir ../target", or equivalent escaping path.
        test.wantError(Ꮡt, gotErr, errAny);
        return ("", errSkipRootConsistencyCheck);
    }
    case {} when test.root != nil && test.target.kind.isLink() && (~test.target.target).kind == testFileAbsent && targetPath != test.targetPath: {
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
internal static readonly @string renameQQˢ = "Rename(%q, %q)"u8;

public static void TestRootMultiRename(ж<Δtesting.T> Ꮡt) {
    if (Δruntime.GOOS == "wasip1"u8) {
        var exprᴛ1 = Δos.Getenv(gowasiruntimeˢ);
        if (exprᴛ1 == ""u8 || exprᴛ1 == "wasmtime"u8) {
            // This test fails when run with wasmtime, because os.RemoveAll fails
            // to remove the test tempdir.
            Ꮡt.Skip(testSeemsToTickleAˢ);
        }
    }
    runRootMultiTest2(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, @string, error> rename = Δos.Rename;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            rename = (@string p1, @string p2) => recvʗ1.Rename(p1, p2);
        }

        // TODO: target directory (if any) should be empty

        test.setOp(renameQQˢ, (~test).sourcePath, (~test).targetPath);
        var gotErr = rename((~test).sourcePath, (~test).targetPath);

        if (Δruntime.GOOS == "windows"u8 && ((~test).source.finalKind() != (~test).target.finalKind() || (~test).source.kind.isLink() || (~test).target.kind.isLink())) {
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
        case {} when (~test).root != nil && (~test).source.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).source.lfinalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).source.slashSuffix() && (~test).source.lfinalKind() != testFileDir && Δruntime.GOOS != "js"u8: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).root != nil && (~test).target.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when Δruntime.GOOS == "plan9"u8: {
            // Plan9 rename behaves differently.
            // Just rely on consistency checks.
            break;
        }
        case {} when (~test).target.lfinalKind() == testFileDir: {
            // POSIX rename() will replace an empty target directory,
            // but os.Rename will not.
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).source.lfinalKind() == testFileDir && (~test).target.lfinalKind() != testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).source.anySlashSuffix() || (~test).target.anySlashSuffix(): {
            if (Δruntime.GOOS == "openbsd"u8) {
                // Known inconsistency: OpenBSD doesn't resolve the final
                // symlink when creating a directory.
                return ("", errSkipRootConsistencyCheck);
            }
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            // TODO: check that the file is in its new location
            break;
        }}

        if (Δruntime.GOOS == "linux"u8 && ((~test).source.slashSuffix() || (~test).target.slashSuffix())) {
            return ("", errSkipRootConsistencyCheck);
        }

        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readFileQˢ = "ReadFile(%q)"u8;

public static void TestRootMultiReadFile(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (slice<byte>, error)> readFile = Δos.ReadFile;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            readFile = (@string p1) => recvʗ1.ReadFile(p1);
        }

        test.setOp(readFileQˢ, (~test).targetPath);
        var (data, gotErr) = readFile((~test).targetPath);
        @string got = default!;
        if (gotErr == default!) {
            got = (@string)data;
        }

        switch (ᐧ) {
        case {} when (~test).target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).root != nil && (~test).target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.finalKind() == testFileAbsent: {
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
        case {} when (~test).target.finalKind() == testFileDir: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            // Trailing slashes are handled differently on different platforms,
            // so we won't try to assert an outcome when they are present.
            // runRootMultiTest will verify that root.ReadFile and os.ReadFile
            // produce consistent results.
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            {
                @string want = targetˢ; if (got != want) {
                    tΔ1.Fatalf("read file content %q, want %q"u8, got, want);
                }
            }
            break;
        }}

        return (got, gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string statQˢ = "Stat(%q)"u8;

public static void TestRootMultiStat(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (fs.FileInfo, error)> stat = Δos.Stat;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            stat = (@string p1) => recvʗ1.Stat(p1);
        }

        test.setOp(statQˢ, (~test).targetPath);
        var (gotStat, gotErr) = stat((~test).targetPath);

        switch (ᐧ) {
        case {} when (~test).target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).root != nil && (~test).target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.finalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            if (!Δos.SameFile(gotStat, (~test).targetInfo)) {
                test.errorf(tΔ1, "stat result is not for target file; want it to be"u8);
            }
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string removeQˢ = "Remove(%q)"u8;

public static void TestRootMultiRemove(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, error> remove = Δos.Remove;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            remove = (@string p1) => recvʗ1.Remove(p1);
        }

        test.setOp(removeQˢ, (~test).targetPath);
        var gotErr = remove((~test).targetPath);

        switch (ᐧ) {
        case {} when (~test).root != nil && (~test).target.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.kind == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
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
            test.wantError(tΔ1, gotErr, default!);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string removeAllQˢ = "RemoveAll(%q)"u8;

public static void TestRootMultiRemoveAll(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, error> removeAll = Δos.RemoveAll;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            removeAll = (@string p1) => recvʗ1.RemoveAll(p1);
        }

        test.setOp(removeAllQˢ, (~test).targetPath);
        var gotErr = removeAll((~test).targetPath);

        switch (ᐧ) {
        case {} when (~test).root != nil && (~test).target.@ref.escapes: {
            // This is only checking target.ref.escapes,
            // not target.lescapes(), because RemoveAll strips
            // terminal slashes.
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            // We are inconsistent on some platforms on whether
            // RemoveAll("symlink/") removes the link or the link target.
            // Something worth addressing, but for now skip the check.
            return ("", errSkipRootConsistencyCheck);
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chtimesQVVˢ = "Chtimes(%q, %v, %v)"u8;

public static void TestRootMultiChtimes(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, time.Time, time.Time, error> chtimes = Δos.Chtimes;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            chtimes = (@string p1, time.Time p2, time.Time p3) => recvʗ1.Chtimes(p1, p2, p3);
        }

        var now = time.Now();
        test.setOp(chtimesQVVˢ, (~test).targetPath, now, now);
        var gotErr = chtimes((~test).targetPath, now, now);

        switch (ᐧ) {
        case {} when (~test).target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).root != nil && (~test).target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.finalKind() == testFileAbsent: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readlinkQˢ = "Readlink(%q)"u8;

public static void TestRootMultiReadlink(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, (@string, error)> readlink = Δos.Readlink;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            readlink = (@string p1) => recvʗ1.Readlink(p1);
        }

        test.setOp(readlinkQˢ, (~test).targetPath);
        var (got, gotErr) = readlink((~test).targetPath);
        {
            var (suffix, ok) = strings.CutPrefix(got, (~test).dir); if (ok) {
                // Replace absolute path prefix with /.../
                got = "/..."u8 + suffix;
            }
        }

        switch (ᐧ) {
        case {} when (~test).root != nil && (~test).target.lescapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when !(~test).target.kind.isLink(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            break;
        }}
        return (got, gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeFileQˢ = "WriteFile(%q, ...)"u8;

public static void TestRootMultiWriteFile(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        Func<@string, slice<byte>, fs.FileMode, error> writeFile = Δos.WriteFile;
        if ((~test).root != nil) {
            var recvʗ1 = (~test).root;
            writeFile = (@string p1, slice<byte> p2, fs.FileMode p3) => recvʗ1.WriteFile(p1, p2, p3);
        }

        test.setOp(writeFileQˢ, (~test).targetPath);
        var gotErr = writeFile((~test).targetPath, slice<byte>("data"u8), 438);

        switch (ᐧ) {
        case {} when (~test).target.isError(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when Δruntime.GOOS == "windows"u8 && (~test).target.isSymlinkToDir(): {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).root != nil && (~test).target.escapes(): {
            test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
            break;
        }
        case {} when (~test).target.finalKind() == testFileDir: {
            test.wantError(tΔ1, gotErr, errAny);
            break;
        }
        case {} when (~test).target.anySlashSuffix(): {
            break;
        }
        default: {
            test.wantError(tΔ1, gotErr, default!);
            break;
        }}
        return ("", gotErr);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string openFileQORdonly0ˢ = "OpenFile(%q, O_RDONLY, 0)"u8;

public static void TestRootMultiOpenFile(ж<Δtesting.T> Ꮡt) {
    runRootMultiTest(Ꮡt, (ж<Δtesting.T> tΔ1, ж<rootMultiTest> test) => {
        GoFrame ᒐ = default;
        try {
            Func<@string, nint, fs.FileMode, (ж<Δos.File>, error)> openFile = Δos.OpenFile;
            if ((~test).root != nil) {
                var recvʗ1 = (~test).root;
                openFile = (@string p1, nint p2, fs.FileMode p3) => recvʗ1.OpenFile(p1, p2, p3);
            }

            test.setOp(openFileQORdonly0ˢ, (~test).targetPath);
            var (f, gotErr) = openFile((~test).targetPath, Δos.O_RDONLY, 0);
            if (gotErr == default!) {
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
            }

            @string got = test.describeFile(tΔ1, f);

            switch (ᐧ) {
            case {} when (~test).target.isError(): {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when (~test).root != nil && (~test).target.escapes(): {
                test.wantError(tΔ1, gotErr, os_internal_test_package.ErrPathEscapes);
                break;
            }
            case {} when (~test).target.finalKind() == testFileAbsent: {
                test.wantError(tΔ1, gotErr, errAny);
                break;
            }
            case {} when (~test).target.anySlashSuffix(): {
                break;
            }
            default: {
                test.wantError(tΔ1, gotErr, default!);
                {
                    @string want = targetˢ; if (got != want) {
                        tΔ1.Fatalf("opened file %q, want %q"u8, got, want);
                    }
                }
                break;
            }}

            return (got, gotErr);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    });
}

} // end os_test_package
