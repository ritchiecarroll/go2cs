// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Use an external test to avoid os/exec -> internal/testenv -> os/exec
// circular dependency.
namespace go.os;

using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using Δexec = go.os.exec_package;
using filepath = path.filepath_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.io;
using go.os;
using path;
using static go.os.exec_internal_test_package;
using ꓸꓸꓸstring = Span<@string>;

partial class exec_test_package {

[GoInit] internal static void initΔ2() {
    registerHelperCommand("printpath", cmdPrintPath);
}

internal static void cmdPrintPath(params ꓸꓸꓸstring _ʗp) {
    fmt.Println(testenv.Executable(default));
}

// makePATH returns a PATH variable referring to the
// given directories relative to a root directory.
//
// The empty string results in an empty entry.
// Paths beginning with . are kept as relative entries.
internal static @string makePATH(@string root, slice<@string> dirs) {
    var pathsᴷ = new stackAppend<@string>();

    var paths = new slice<@string>(0, len(dirs));
    foreach (var (_, d) in range(dirs)) {
        switch (ᐧ) {
        case {} when d == ""u8: {
            paths = append(pathsᴷ, paths, ""u8);
            break;
        }
        case {} when d == "."u8 || (len(d) >= 2 && d[0] == (byte)'.' && os.IsPathSeparator(d[1])): {
            paths = append(paths, filepath.Clean(d));
            break;
        }
        default: {
            paths = append(paths, filepath.Join(root, d));
            break;
        }}
    }
    return strings.Join(paths, (@string)(rune)os.PathListSeparator);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string batˢ => ref batˢᵇ;
private static readonly @string batˢᵇ = ".bat"u8;

// installProgs creates executable files (or symlinks to executable files) at
// multiple destination paths. It uses root as prefix for all destination files.
internal static void installProgs(ж<testing.T> t, @string root, slice<@string> files) {
    foreach (var (_, f) in range(files)) {
        @string dstPath = filepath.Join(root, f);

        @string dir = filepath.Dir(dstPath);
        var err = os.MkdirAll(dir, 493); if (err != nil) {
            t.Fatal(err);
        }

        if (os.IsPathSeparator(f[len(f) - 1])) {
            continue; // directory and PATH entry only.
        }
        if (strings.EqualFold(filepath.Ext(f), batˢ)) {
            installBat(t, dstPath);
        } else {
            installExe(t, dstPath);
        }
    }
}

// installExe installs a copy of the test executable
// at the given location, creating directories as needed.
//
// (We use a copy instead of just a symlink to ensure that os.Executable
// always reports an unambiguous path, regardless of how it is implemented.)
internal static void installExe(ж<testing.T> t, @string dstPath) {
    GoFrame ᒐ = default;
    try {
        var (src, err) = os.Open(testenv.Executable(new exec_test_package.testing_TжTB(t)));
        if (err != nil) {
            t.Fatal(err);
        }
        var srcʗ1 = src;
        defer(() => srcʗ1.Close(), ref ᒐ);

        (var dst, err) = os.OpenFile(dstPath, (nint)((nint)(nint)(os.O_CREATE | os.O_EXCL) | os.O_WRONLY), 511);
        if (err != nil) {
            t.Fatal(err);
        }
        var dstʗ1 = dst;
        defer(() => {
            var errΔ1 = dstʗ1.Close(); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }, ref ᒐ);

        (_, err) = io.Copy(new os.FileжWriter(dst), new exec_test_package.os_FileжReader(src));
        if (err != nil) {
            t.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// installBat creates a batch file at dst that prints its own
// path when run.
internal static void installBat(ж<testing.T> t, @string dstPath) {
    GoFrame ᒐ = default;
    try {
        var (dst, err) = os.OpenFile(dstPath, (nint)((nint)(nint)(os.O_CREATE | os.O_EXCL) | os.O_WRONLY), 511);
        if (err != nil) {
            t.Fatal(err);
        }
        var dstʗ1 = dst;
        defer(() => {
            var errΔ1 = dstʗ1.Close(); if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }, ref ᒐ);

        var (_, errΔ2) = fmt.Fprintf(new os.FileжWriter(dst), "@echo %s\r\n"u8, dstPath); if (errΔ2 != nil) {
            t.Fatal(errΔ2);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct lookPathTest {
    internal @string name;
    public @string PATHEXT; // empty to use default
    internal slice<@string> files;
    public slice<@string> PATH; // if nil, use all parent directories from files
    internal @string searchFor;
    internal @string want;
    internal error wantErr;
    internal bool skipCmdExeCheck; // if true, do not check want against the behavior of cmd.exe
}

internal static slice<lookPathTest> lookPathTests = [
    new(
        name: "first match",
        files: [@"p1\a.exe", @"p2\a.exe", @"p2\a"],
        searchFor: @"a",
        want: @"p1\a.exe"
    ),
    new(
        name: "dirs with extensions",
        files: [@"p1.dir\a", @"p2.dir\a.exe"],
        searchFor: @"a",
        want: @"p2.dir\a.exe"
    ),
    new(
        name: "first with extension",
        files: [@"p1\a.exe", @"p2\a.exe"],
        searchFor: @"a.exe",
        want: @"p1\a.exe"
    ),
    new(
        name: "specific name",
        files: [@"p1\a.exe", @"p2\b.exe"],
        searchFor: @"b",
        want: @"p2\b.exe"
    ),
    new(
        name: "no extension",
        files: [@"p1\b", @"p2\a"],
        searchFor: @"a",
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "directory, no extension",
        files: [@"p1\a.exe", @"p2\a.exe"],
        searchFor: @"p2\a",
        want: @"p2\a.exe"
    ),
    new(
        name: "no match",
        files: [@"p1\a.exe", @"p2\a.exe"],
        searchFor: @"b",
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "no match with dir",
        files: [@"p1\b.exe", @"p2\a.exe"],
        searchFor: @"p2\b",
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "extensionless file in CWD ignored",
        files: [@"a", @"p1\a.exe", @"p2\a.exe"],
        searchFor: @"a",
        want: @"p1\a.exe"
    ),
    new(
        name: "extensionless file in PATH ignored",
        files: [@"p1\a", @"p2\a.exe"],
        searchFor: @"a",
        want: @"p2\a.exe"
    ),
    new(
        name: "specific extension",
        files: [@"p1\a.exe", @"p2\a.bat"],
        searchFor: @"a.bat",
        want: @"p2\a.bat"
    ),
    new(
        name: "mismatched extension",
        files: [@"p1\a.exe", @"p2\a.exe"],
        searchFor: @"a.com",
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "doubled extension",
        files: [@"p1\a.exe.exe"],
        searchFor: @"a.exe",
        want: @"p1\a.exe.exe"
    ),
    new(
        name: "extension not in PATHEXT",
        PATHEXT: @".COM;.BAT",
        files: [@"p1\a.exe", @"p2\a.exe"],
        searchFor: @"a.exe",
        want: @"p1\a.exe"
    ),
    new(
        name: "first allowed by PATHEXT",
        PATHEXT: @".COM;.EXE",
        files: [@"p1\a.bat", @"p2\a.exe"],
        searchFor: @"a",
        want: @"p2\a.exe"
    ),
    new(
        name: "first directory containing a PATHEXT match",
        PATHEXT: @".COM;.EXE;.BAT",
        files: [@"p1\a.bat", @"p2\a.exe"],
        searchFor: @"a",
        want: @"p1\a.bat"
    ),
    new(
        name: "first PATHEXT entry",
        PATHEXT: @".COM;.EXE;.BAT",
        files: [@"p1\a.bat", @"p1\a.exe", @"p2\a.bat", @"p2\a.exe"],
        searchFor: @"a",
        want: @"p1\a.exe"
    ),
    new(
        name: "ignore dir with PATHEXT extension",
        files: [@"a.exe\"],
        searchFor: @"a",
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "ignore empty PATH entry",
        files: [@"a.bat", @"p\a.bat"],
        PATH: [@"p"],
        searchFor: @"a",
        want: @"p\a.bat",
        // If cmd.exe is too old it might not respect NoDefaultCurrentDirectoryInExePath,
        // so skip that check.
        skipCmdExeCheck: true
    ),
    new(
        name: "return ErrDot if found by a different absolute path",
        files: [@"p1\a.bat", @"p2\a.bat"],
        PATH: [@".\p1", @"p2"],
        searchFor: @"a",
        want: @"p1\a.bat",
        wantErr: Δexec.ErrDot
    ),
    new(
        name: "suppress ErrDot if also found in absolute path",
        files: [@"p1\a.bat", @"p2\a.bat"],
        PATH: [@".\p1", @"p1", @"p2"],
        searchFor: @"a",
        want: @"p1\a.bat"
    )
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string printpathˢ => ref printpathˢᵇ;
private static readonly @string printpathˢᵇ = "printpath"u8;
internal static ref readonly @string pathextˢ => ref pathextˢᵇ;
private static readonly @string pathextˢᵇ = "PATHEXT"u8;

public static void TestLookPathWindows(ж<testing.T> t) {
    // Not parallel: uses Chdir and Setenv.

    // We are using the "printpath" command mode to test exec.Command here,
    // so we won't be calling helperCommand to resolve it.
    // That may cause it to appear to be unused.
    maySkipHelperCommand(printpathˢ);

    // Before we begin, find the absolute path to cmd.exe.
    // In non-short mode, we will use it to check the ground truth
    // of the test's "want" field.
    var (cmdExe, err) = Δexec.LookPath(cmdˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, vᴛ1) in range(lookPathTests)) {
        ref var tt = ref heap(new lookPathTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var pathsᴷ = new stackAppend<@string>();
            if (ttʗ1.want == ""u8 && ttʗ1.wantErr == nil) {
                tΔ1.Fatalf("test must specify either want or wantErr");
            }

            @string root = tΔ1.TempDir();
            installProgs(tΔ1, root, ttʗ1.files);

            if (ttʗ1.PATHEXT != ""u8) {
                tΔ1.Setenv(pathextˢ, ttʗ1.PATHEXT);
                tΔ1.Logf("set PATHEXT=%s", ttʗ1.PATHEXT);
            }

            @string pathVar = default;
            if (ttʗ1.PATH == nil) {
                var paths = new slice<@string>(0, len(ttʗ1.files));
                foreach (var (_, f) in range(ttʗ1.files)) {
                    @string dir = filepath.Join(root, filepath.Dir(f));
                    if (!slices.Contains(paths, dir)) {
                        paths = append(pathsᴷ, paths, dir);
                    }
                }
                pathVar = strings.Join(paths, (@string)(rune)os.PathListSeparator);
            } else {
                pathVar = makePATH(root, ttʗ1.PATH);
            }
            tΔ1.Setenv(pathˢ, pathVar);
            tΔ1.Logf("set PATH=%s", pathVar);

            tΔ1.Chdir(root);

            if (!testing.Short() && !(ttʗ1.skipCmdExeCheck || errors.Is(ttʗ1.wantErr, Δexec.ErrDot))) {
                // Check that cmd.exe, which is our source of ground truth,
                // agrees that our test case is correct.
                var cmd = testenv.Command(new exec_test_package.testing_TжTB(tΔ1), cmdExe, "/c", ttʗ1.searchFor, printpathˢ);
                var (@out, errΔ1) = cmd.Output();
                if (errΔ1 == nil) {
                    @string gotAbs = strings.TrimSpace((@string)@out);
                    @string wantAbs = ""u8;
                    if (ttʗ1.want != ""u8) {
                        wantAbs = filepath.Join(root, ttʗ1.want);
                    }
                    if (gotAbs != wantAbs) {
                        // cmd.exe disagrees. Probably the test case is wrong?
                        tΔ1.Fatalf("%v\n\tresolved to %s\n\twant %s", cmd.OrTypedNil(), gotAbs, wantAbs);
                    }
                } else if (ttʗ1.wantErr == nil) {
                    var (ee, ok) = errΔ1._<ж<Δexec.ExitError>>(ᐧ); if (ok && len(ee.Stderr) > 0) {
                        tΔ1.Fatalf("%v: %v\n%s", cmd.OrTypedNil(), errΔ1, ee.Stderr);
                    }
                    tΔ1.Fatalf("%v: %v", cmd.OrTypedNil(), errΔ1);
                }
            }

            var (got, errΔ2) = Δexec.LookPath(ttʗ1.searchFor);
            if (filepath.IsAbs(got)) {
                (got, errΔ2) = filepath.Rel(root, got);
                if (errΔ2 != nil) {
                    tΔ1.Fatal(errΔ2);
                }
            }
            if (got != ttʗ1.want) {
                tΔ1.Errorf("LookPath(%#q) = %#q; want %#q", ttʗ1.searchFor, got, ttʗ1.want);
            }
            if (!errors.Is(errΔ2, ttʗ1.wantErr)) {
                tΔ1.Errorf("LookPath(%#q): %v; want %v", ttʗ1.searchFor, errΔ2, ttʗ1.wantErr);
            }
        });
    }
}

partial struct commandTest {
    internal @string name;
    public slice<@string> PATH;
    internal slice<@string> files;
    internal @string dir;
    internal @string arg0;
    internal @string want;
    internal @string wantPath; // the resolved c.Path, if different from want
    internal bool wantErrDot;
    internal error wantRunErr;
}

internal static slice<commandTest> commandTests = [
    // testing commands with no slash, like `a.exe`
    new(
        name: "current directory",
        files: [@"a.exe"],
        PATH: ["."],
        arg0: @"a.exe",
        want: @"a.exe",
        wantErrDot: true
    ),
    new(
        name: "with extra PATH",
        files: [@"a.exe", @"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2", "p"],
        arg0: @"a.exe",
        want: @"a.exe",
        wantErrDot: true
    ),
    new(
        name: "with extra PATH and no extension",
        files: [@"a.exe", @"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2", "p"],
        arg0: @"a",
        want: @"a.exe",
        wantErrDot: true
    ),
    // testing commands with slash, like `.\a.exe`
    new(
        name: "with dir",
        files: [@"p\a.exe"],
        PATH: ["."],
        arg0: @"p\a.exe",
        want: @"p\a.exe"
    ),
    new(
        name: "with explicit dot",
        files: [@"p\a.exe"],
        PATH: ["."],
        arg0: @".\p\a.exe",
        want: @"p\a.exe"
    ),
    new(
        name: "with irrelevant PATH",
        files: [@"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2"],
        arg0: @"p\a.exe",
        want: @"p\a.exe"
    ),
    new(
        name: "with slash and no extension",
        files: [@"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2"],
        arg0: @"p\a",
        want: @"p\a.exe"
    ),
    // tests commands, like `a.exe`, with c.Dir set
    new(
        // should not find a.exe in p, because LookPath(`a.exe`) will fail when
        // called by Command (before Dir is set), and that error is sticky.
        name: "not found before Dir",
        files: [@"p\a.exe"],
        PATH: ["."],
        dir: @"p",
        arg0: @"a.exe",
        want: @"p\a.exe",
        wantRunErr: Δexec.ErrNotFound
    ),
    new(
        // LookPath(`a.exe`) will resolve to `.\a.exe`, but prefixing that with
        // dir `p\a.exe` will refer to a non-existent file
        name: "resolved before Dir",
        files: [@"a.exe", @"p\not_important_file"],
        PATH: ["."],
        dir: @"p",
        arg0: @"a.exe",
        want: @"a.exe",
        wantErrDot: true,
        wantRunErr: fs.ErrNotExist
    ),
    new(
        // like above, but making test succeed by installing file
        // in referred destination (so LookPath(`a.exe`) will still
        // find `.\a.exe`, but we successfully execute `p\a.exe`)
        name: "relative to Dir",
        files: [@"a.exe", @"p\a.exe"],
        PATH: ["."],
        dir: @"p",
        arg0: @"a.exe",
        want: @"p\a.exe",
        wantErrDot: true
    ),
    new(
        // like above, but add PATH in attempt to break the test
        name: "relative to Dir with extra PATH",
        files: [@"a.exe", @"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2", "p"],
        dir: @"p",
        arg0: @"a.exe",
        want: @"p\a.exe",
        wantErrDot: true
    ),
    new(
        // like above, but use "a" instead of "a.exe" for command
        name: "relative to Dir with extra PATH and no extension",
        files: [@"a.exe", @"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2", "p"],
        dir: @"p",
        arg0: @"a",
        want: @"p\a.exe",
        wantErrDot: true
    ),
    new(
        // finds `a.exe` in the PATH regardless of Dir because Command resolves the
        // full path (using LookPath) before Dir is set.
        name: "from PATH with no match in Dir",
        files: [@"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2", "p"],
        dir: @"p",
        arg0: @"a.exe",
        want: @"p2\a.exe"
    ),
    // tests commands, like `.\a.exe`, with c.Dir set
    new(
        // should use dir when command is path, like ".\a.exe"
        name: "relative to Dir with explicit dot",
        files: [@"p\a.exe"],
        PATH: ["."],
        dir: @"p",
        arg0: @".\a.exe",
        want: @"p\a.exe"
    ),
    new(
        // like above, but with PATH added in attempt to break it
        name: "relative to Dir with dot and extra PATH",
        files: [@"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2"],
        dir: @"p",
        arg0: @".\a.exe",
        want: @"p\a.exe"
    ),
    new(
        // LookPath(".\a") will fail before Dir is set, and that error is sticky.
        name: "relative to Dir with dot and extra PATH and no extension",
        files: [@"p\a.exe", @"p2\a.exe"],
        PATH: [".", "p2"],
        dir: @"p",
        arg0: @".\a",
        want: @"p\a.exe"
    ),
    new(
        // LookPath(".\a") will fail before Dir is set, and that error is sticky.
        name: "relative to Dir with different extension",
        files: [@"a.exe", @"p\a.bat"],
        PATH: ["."],
        dir: @"p",
        arg0: @".\a",
        want: @"p\a.bat"
    )
];

public static void TestCommand(ж<testing.T> t) {
    // Not parallel: uses Chdir and Setenv.

    // We are using the "printpath" command mode to test exec.Command here,
    // so we won't be calling helperCommand to resolve it.
    // That may cause it to appear to be unused.
    maySkipHelperCommand(printpathˢ);

    foreach (var (_, vᴛ1) in range(commandTests)) {
        ref var tt = ref heap(new commandTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            if (ttʗ1.PATH == nil) {
                tΔ1.Fatalf("test must specify PATH");
            }

            @string root = tΔ1.TempDir();
            installProgs(tΔ1, root, ttʗ1.files);

            @string pathVar = makePATH(root, ttʗ1.PATH);
            tΔ1.Setenv(pathˢ, pathVar);
            tΔ1.Logf("set PATH=%s", pathVar);

            tΔ1.Chdir(root);

            var cmd = Δexec.Command(ttʗ1.arg0, printpathˢ);
            cmd.Dir = filepath.Join(root, ttʗ1.dir);
            if (ttʗ1.wantErrDot) {
                if (errors.Is(cmd.Err, Δexec.ErrDot)) {
                    cmd.Err = default;
                } else {
                    tΔ1.Fatalf("cmd.Err = %v; want ErrDot", cmd.Err);
                }
            }

            var (@out, err) = cmd.Output();
            if (err != nil) {
                var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (ok && len(ee.Stderr) > 0) {
                    tΔ1.Logf("%v: %v\n%s", cmd.OrTypedNil(), err, ee.Stderr);
                } else {
                    tΔ1.Logf("%v: %v", cmd.OrTypedNil(), err);
                }
                if (!errors.Is(err, ttʗ1.wantRunErr)) {
                    tΔ1.Errorf("want %v", ttʗ1.wantRunErr);
                }
                return;
            }

            @string got = strings.TrimSpace((@string)@out);
            if (filepath.IsAbs(got)) {
                (got, err) = filepath.Rel(root, got);
                if (err != nil) {
                    tΔ1.Fatal(err);
                }
            }
            if (got != ttʗ1.want) {
                tΔ1.Errorf("\nran  %#q\nwant %#q", got, ttʗ1.want);
            }

            @string gotPath = cmd.Path;
            @string wantPath = ttʗ1.wantPath;
            if (wantPath == ""u8) {
                if (strings.Contains(ttʗ1.arg0, @"\")) {
                    wantPath = ttʗ1.arg0;
                } else if (ttʗ1.wantErrDot) {
                    wantPath = strings.TrimPrefix(ttʗ1.want, ttʗ1.dir + @"\"u8);
                } else {
                    wantPath = filepath.Join(root, ttʗ1.want);
                }
            }
            if (gotPath != wantPath) {
                tΔ1.Errorf("\ncmd.Path = %#q\nwant       %#q", gotPath, wantPath);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleComˢ => ref exampleComˢᵇ;
private static readonly @string exampleComˢᵇ = "example.com"u8;

public static void TestAbsCommandWithDoubledExtension(ж<testing.T> t) {
    t.Parallel();

    // We expect that ".com" is always included in PATHEXT, but it may also be
    // found in the import path of a Go package. If it is at the root of the
    // import path, the resulting executable may be named like "example.com.exe".
    //
    // Since "example.com" looks like a proper executable name, it is probably ok
    // for exec.Command to try to run it directly without re-resolving it.
    // However, exec.LookPath should try a little harder to figure it out.

    @string comPath = filepath.Join(t.TempDir(), exampleComˢ);
    @string batPath = comPath + ".bat"u8;
    installBat(t, batPath);

    var cmd = Δexec.Command(comPath);
    var (@out, err) = cmd.CombinedOutput();
    t.Logf("%v: %v\n%s", cmd.OrTypedNil(), err, @out);
    if (!errors.Is(err, fs.ErrNotExist)) {
        t.Errorf("Command(%#q).Run: %v\nwant fs.ErrNotExist", comPath, err);
    }

    (var resolved, err) = Δexec.LookPath(comPath);
    if (err != nil || resolved != batPath) {
        t.Fatalf("LookPath(%#q) = %v, %v; want %#q, <nil>", comPath, resolved, err, batPath);
    }
}

} // end exec_test_package
