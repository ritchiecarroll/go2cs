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
    registerHelperCommand("printpath"u8, cmdPrintPath);
}

internal static void cmdPrintPath(params ꓸꓸꓸstring _ʗp) {
    fmt.Println(testenv.Executable(default!));
}

// makePATH returns a PATH variable referring to the
// given directories relative to a root directory.
//
// The empty string results in an empty entry.
// Paths beginning with . are kept as relative entries.
internal static @string makePATH(@string root, slice<@string> dirs) {
    var paths = new slice<@string>(0, len(dirs));
    foreach (var (_, d) in dirs) {
        switch (ᐧ) {
        case {} when d == ""u8: {
            paths = append(paths, ""u8);
            break;
        }
        case {} when d == "."u8 || (len(d) >= 2 && d[0] == (rune)'.' && os.IsPathSeparator(d[1])): {
            paths = append(paths, filepath.Clean(d));
            break;
        }
        default: {
            paths = append(paths, filepath.Join(root, d));
            break;
        }}

    }
    return strings.Join(paths, ((@string)(rune)os.PathListSeparator));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string batˢ = ".bat"u8;

// installProgs creates executable files (or symlinks to executable files) at
// multiple destination paths. It uses root as prefix for all destination files.
internal static void installProgs(ж<testing.T> Ꮡt, @string root, slice<@string> files) {
    foreach (var (_, f) in files) {
        @string dstPath = filepath.Join(root, f);
        @string dir = filepath.Dir(dstPath);
        {
            var err = os.MkdirAll(dir, 493); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        if (os.IsPathSeparator(f[len(f) - 1])) {
            continue; // directory and PATH entry only.
        }
        if (strings.EqualFold(filepath.Ext(f), batˢ)){
            installBat(Ꮡt, dstPath);
        } else {
            installExe(Ꮡt, dstPath);
        }
    }
}

// installExe installs a copy of the test executable
// at the given location, creating directories as needed.
//
// (We use a copy instead of just a symlink to ensure that os.Executable
// always reports an unambiguous path, regardless of how it is implemented.)
internal static void installExe(ж<testing.T> Ꮡt, @string dstPath) {
    GoFrame ᒐ = default;
    try {
        var (src, err) = os.Open(testenv.Executable(new exec_test_package.testing_TжTB(Ꮡt)));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var srcʗ1 = src;
        defer(() => srcʗ1.Close(), ref ᒐ);
        (var dst, err) = os.OpenFile(dstPath, (nint)((nint)(nint)(os.O_CREATE | os.O_EXCL) | os.O_WRONLY), 511);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dstʗ1 = dst;
        defer(() => {
            {
                var errΔ1 = dstʗ1.Close(); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }, ref ᒐ);
        (_, err) = io.Copy(new os.FileжWriter(dst), new exec_test_package.os_FileжReader(src));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// installBat creates a batch file at dst that prints its own
// path when run.
internal static void installBat(ж<testing.T> Ꮡt, @string dstPath) {
    GoFrame ᒐ = default;
    try {
        var (dst, err) = os.OpenFile(dstPath, (nint)((nint)(nint)(os.O_CREATE | os.O_EXCL) | os.O_WRONLY), 511);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var dstʗ1 = dst;
        defer(() => {
            {
                var errΔ1 = dstʗ1.Close(); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
        }, ref ᒐ);
        {
            var (_, errΔ2) = fmt.Fprintf(new os.FileжWriter(dst), "@echo %s\r\n"u8, dstPath); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
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

// If cmd.exe is too old it might not respect NoDefaultCurrentDirectoryInExePath,
// so skip that check.
internal static slice<lookPathTest> lookPathTests = new lookPathTest[]{
    new(
        name: "first match"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.exe"u8, @"p2\a"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p1\a.exe"u8
    ),
    new(
        name: "dirs with extensions"u8,
        files: new @string[]{@"p1.dir\a"u8, @"p2.dir\a.exe"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p2.dir\a.exe"u8
    ),
    new(
        name: "first with extension"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a.exe"u8,
        want: @"p1\a.exe"u8
    ),
    new(
        name: "specific name"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\b.exe"u8}.slice(),
        searchFor: @"b"u8,
        want: @"p2\b.exe"u8
    ),
    new(
        name: "no extension"u8,
        files: new @string[]{@"p1\b"u8, @"p2\a"u8}.slice(),
        searchFor: @"a"u8,
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "directory, no extension"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"p2\a"u8,
        want: @"p2\a.exe"u8
    ),
    new(
        name: "no match"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"b"u8,
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "no match with dir"u8,
        files: new @string[]{@"p1\b.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"p2\b"u8,
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "extensionless file in CWD ignored"u8,
        files: new @string[]{@"a"u8, @"p1\a.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p1\a.exe"u8
    ),
    new(
        name: "extensionless file in PATH ignored"u8,
        files: new @string[]{@"p1\a"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p2\a.exe"u8
    ),
    new(
        name: "specific extension"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.bat"u8}.slice(),
        searchFor: @"a.bat"u8,
        want: @"p2\a.bat"u8
    ),
    new(
        name: "mismatched extension"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a.com"u8,
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "doubled extension"u8,
        files: new @string[]{@"p1\a.exe.exe"u8}.slice(),
        searchFor: @"a.exe"u8,
        want: @"p1\a.exe.exe"u8
    ),
    new(
        name: "extension not in PATHEXT"u8,
        PATHEXT: @".COM;.BAT"u8,
        files: new @string[]{@"p1\a.exe"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a.exe"u8,
        want: @"p1\a.exe"u8
    ),
    new(
        name: "first allowed by PATHEXT"u8,
        PATHEXT: @".COM;.EXE"u8,
        files: new @string[]{@"p1\a.bat"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p2\a.exe"u8
    ),
    new(
        name: "first directory containing a PATHEXT match"u8,
        PATHEXT: @".COM;.EXE;.BAT"u8,
        files: new @string[]{@"p1\a.bat"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p1\a.bat"u8
    ),
    new(
        name: "first PATHEXT entry"u8,
        PATHEXT: @".COM;.EXE;.BAT"u8,
        files: new @string[]{@"p1\a.bat"u8, @"p1\a.exe"u8, @"p2\a.bat"u8, @"p2\a.exe"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p1\a.exe"u8
    ),
    new(
        name: "ignore dir with PATHEXT extension"u8,
        files: new @string[]{@"a.exe\"u8}.slice(),
        searchFor: @"a"u8,
        wantErr: Δexec.ErrNotFound
    ),
    new(
        name: "ignore empty PATH entry"u8,
        files: new @string[]{@"a.bat"u8, @"p\a.bat"u8}.slice(),
        PATH: new @string[]{@"p"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p\a.bat"u8,
        skipCmdExeCheck: true
    ),
    new(
        name: "return ErrDot if found by a different absolute path"u8,
        files: new @string[]{@"p1\a.bat"u8, @"p2\a.bat"u8}.slice(),
        PATH: new @string[]{@".\p1"u8, @"p2"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p1\a.bat"u8,
        wantErr: Δexec.ErrDot
    ),
    new(
        name: "suppress ErrDot if also found in absolute path"u8,
        files: new @string[]{@"p1\a.bat"u8, @"p2\a.bat"u8}.slice(),
        PATH: new @string[]{@".\p1"u8, @"p1"u8, @"p2"u8}.slice(),
        searchFor: @"a"u8,
        want: @"p1\a.bat"u8
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string printpathˢ = "printpath"u8;
internal static readonly @string pathextˢ = "PATHEXT"u8;

public static void TestLookPathWindows(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Not parallel: uses Chdir and Setenv.
    // We are using the "printpath" command mode to test exec.Command here,
    // so we won't be calling helperCommand to resolve it.
    // That may cause it to appear to be unused.
    maySkipHelperCommand(printpathˢ);
    // Before we begin, find the absolute path to cmd.exe.
    // In non-short mode, we will use it to check the ground truth
    // of the test's "want" field.
    var (cmdExe, err) = Δexec.LookPath(cmdˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, vᴛ1) in lookPathTests) {
        ref var tt = ref heap(new lookPathTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            if (ttʗ1.want == ""u8 && ttʗ1.wantErr == default!) {
                tΔ1.Fatalf("test must specify either want or wantErr"u8);
            }
            @string root = tΔ1.TempDir();
            installProgs(tΔ1, root, ttʗ1.files);
            if (ttʗ1.PATHEXT != ""u8) {
                tΔ1.Setenv(pathextˢ, ttʗ1.PATHEXT);
                tΔ1.Logf("set PATHEXT=%s"u8, ttʗ1.PATHEXT);
            }
            @string pathVar = default!;
            if (ttʗ1.PATH == default!){
                var paths = new slice<@string>(0, len(ttʗ1.files));
                foreach (var (_, f) in ttʗ1.files) {
                    @string dir = filepath.Join(root, filepath.Dir(f));
                    if (!slices.Contains(paths, dir)) {
                        paths = append(paths, dir);
                    }
                }
                pathVar = strings.Join(paths, ((@string)(rune)os.PathListSeparator));
            } else {
                pathVar = makePATH(root, ttʗ1.PATH);
            }
            tΔ1.Setenv(pathˢ, pathVar);
            tΔ1.Logf("set PATH=%s"u8, pathVar);
            tΔ1.Chdir(root);
            if (!testing.Short() && !(ttʗ1.skipCmdExeCheck || errors.Is(ttʗ1.wantErr, Δexec.ErrDot))) {
                // Check that cmd.exe, which is our source of ground truth,
                // agrees that our test case is correct.
                var cmd = testenv.Command(new exec_test_package.testing_TжTB(tΔ1), cmdExe, "/c"u8, ttʗ1.searchFor, printpathˢ);
                var (@out, errΔ1) = cmd.Output();
                if (errΔ1 == default!){
                    @string gotAbs = strings.TrimSpace(((@string)@out));
                    @string wantAbs = ""u8;
                    if (ttʗ1.want != ""u8) {
                        wantAbs = filepath.Join(root, ttʗ1.want);
                    }
                    if (gotAbs != wantAbs) {
                        // cmd.exe disagrees. Probably the test case is wrong?
                        tΔ1.Fatalf("%v\n\tresolved to %s\n\twant %s"u8, cmd.OrTypedNil(), gotAbs, wantAbs);
                    }
                } else 
                if (ttʗ1.wantErr == default!) {
                    {
                        var (ee, ok) = errΔ1._<ж<Δexec.ExitError>>(ᐧ); if (ok && len((~ee).Stderr) > 0) {
                            tΔ1.Fatalf("%v: %v\n%s"u8, cmd.OrTypedNil(), errΔ1, (~ee).Stderr);
                        }
                    }
                    tΔ1.Fatalf("%v: %v"u8, cmd.OrTypedNil(), errΔ1);
                }
            }
            var (got, errΔ2) = Δexec.LookPath(ttʗ1.searchFor);
            if (filepath.IsAbs(got)) {
                (got, errΔ2) = filepath.Rel(root, got);
                if (errΔ2 != default!) {
                    tΔ1.Fatal(errΔ2);
                }
            }
            if (got != ttʗ1.want) {
                tΔ1.Errorf("LookPath(%#q) = %#q; want %#q"u8, ttʗ1.searchFor, got, ttʗ1.want);
            }
            if (!errors.Is(errΔ2, ttʗ1.wantErr)) {
                tΔ1.Errorf("LookPath(%#q): %v; want %v"u8, ttʗ1.searchFor, errΔ2, ttʗ1.wantErr);
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

// testing commands with no slash, like `a.exe`
// testing commands with slash, like `.\a.exe`
// tests commands, like `a.exe`, with c.Dir set
// should not find a.exe in p, because LookPath(`a.exe`) will fail when
// called by Command (before Dir is set), and that error is sticky.
// LookPath(`a.exe`) will resolve to `.\a.exe`, but prefixing that with
// dir `p\a.exe` will refer to a non-existent file
// like above, but making test succeed by installing file
// in referred destination (so LookPath(`a.exe`) will still
// find `.\a.exe`, but we successfully execute `p\a.exe`)
// like above, but add PATH in attempt to break the test
// like above, but use "a" instead of "a.exe" for command
// finds `a.exe` in the PATH regardless of Dir because Command resolves the
// full path (using LookPath) before Dir is set.
// tests commands, like `.\a.exe`, with c.Dir set
// should use dir when command is path, like ".\a.exe"
// like above, but with PATH added in attempt to break it
// LookPath(".\a") will fail before Dir is set, and that error is sticky.
// LookPath(".\a") will fail before Dir is set, and that error is sticky.
internal static slice<commandTest> commandTests = new commandTest[]{
    new(
        name: "current directory"u8,
        files: new @string[]{@"a.exe"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        arg0: @"a.exe"u8,
        want: @"a.exe"u8,
        wantErrDot: true
    ),
    new(
        name: "with extra PATH"u8,
        files: new @string[]{@"a.exe"u8, @"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8, "p"u8}.slice(),
        arg0: @"a.exe"u8,
        want: @"a.exe"u8,
        wantErrDot: true
    ),
    new(
        name: "with extra PATH and no extension"u8,
        files: new @string[]{@"a.exe"u8, @"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8, "p"u8}.slice(),
        arg0: @"a"u8,
        want: @"a.exe"u8,
        wantErrDot: true
    ),
    new(
        name: "with dir"u8,
        files: new @string[]{@"p\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        arg0: @"p\a.exe"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "with explicit dot"u8,
        files: new @string[]{@"p\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        arg0: @".\p\a.exe"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "with irrelevant PATH"u8,
        files: new @string[]{@"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8}.slice(),
        arg0: @"p\a.exe"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "with slash and no extension"u8,
        files: new @string[]{@"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8}.slice(),
        arg0: @"p\a"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "not found before Dir"u8,
        files: new @string[]{@"p\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        dir: @"p"u8,
        arg0: @"a.exe"u8,
        want: @"p\a.exe"u8,
        wantRunErr: Δexec.ErrNotFound
    ),
    new(
        name: "resolved before Dir"u8,
        files: new @string[]{@"a.exe"u8, @"p\not_important_file"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        dir: @"p"u8,
        arg0: @"a.exe"u8,
        want: @"a.exe"u8,
        wantErrDot: true,
        wantRunErr: fs.ErrNotExist
    ),
    new(
        name: "relative to Dir"u8,
        files: new @string[]{@"a.exe"u8, @"p\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        dir: @"p"u8,
        arg0: @"a.exe"u8,
        want: @"p\a.exe"u8,
        wantErrDot: true
    ),
    new(
        name: "relative to Dir with extra PATH"u8,
        files: new @string[]{@"a.exe"u8, @"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8, "p"u8}.slice(),
        dir: @"p"u8,
        arg0: @"a.exe"u8,
        want: @"p\a.exe"u8,
        wantErrDot: true
    ),
    new(
        name: "relative to Dir with extra PATH and no extension"u8,
        files: new @string[]{@"a.exe"u8, @"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8, "p"u8}.slice(),
        dir: @"p"u8,
        arg0: @"a"u8,
        want: @"p\a.exe"u8,
        wantErrDot: true
    ),
    new(
        name: "from PATH with no match in Dir"u8,
        files: new @string[]{@"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8, "p"u8}.slice(),
        dir: @"p"u8,
        arg0: @"a.exe"u8,
        want: @"p2\a.exe"u8
    ),
    new(
        name: "relative to Dir with explicit dot"u8,
        files: new @string[]{@"p\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        dir: @"p"u8,
        arg0: @".\a.exe"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "relative to Dir with dot and extra PATH"u8,
        files: new @string[]{@"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8}.slice(),
        dir: @"p"u8,
        arg0: @".\a.exe"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "relative to Dir with dot and extra PATH and no extension"u8,
        files: new @string[]{@"p\a.exe"u8, @"p2\a.exe"u8}.slice(),
        PATH: new @string[]{"."u8, "p2"u8}.slice(),
        dir: @"p"u8,
        arg0: @".\a"u8,
        want: @"p\a.exe"u8
    ),
    new(
        name: "relative to Dir with different extension"u8,
        files: new @string[]{@"a.exe"u8, @"p\a.bat"u8}.slice(),
        PATH: new @string[]{"."u8}.slice(),
        dir: @"p"u8,
        arg0: @".\a"u8,
        want: @"p\a.bat"u8
    )
}.slice();

public static void TestCommand(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Not parallel: uses Chdir and Setenv.
    // We are using the "printpath" command mode to test exec.Command here,
    // so we won't be calling helperCommand to resolve it.
    // That may cause it to appear to be unused.
    maySkipHelperCommand(printpathˢ);
    foreach (var (_, vᴛ1) in commandTests) {
        ref var tt = ref heap(new commandTest(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            if (ttʗ1.PATH == default!) {
                tΔ1.Fatalf("test must specify PATH"u8);
            }
            @string root = tΔ1.TempDir();
            installProgs(tΔ1, root, ttʗ1.files);
            @string pathVar = makePATH(root, ttʗ1.PATH);
            tΔ1.Setenv(pathˢ, pathVar);
            tΔ1.Logf("set PATH=%s"u8, pathVar);
            tΔ1.Chdir(root);
            var cmd = Δexec.Command(ttʗ1.arg0, printpathˢ);
            cmd.Value.Dir = filepath.Join(root, ttʗ1.dir);
            if (ttʗ1.wantErrDot) {
                if (errors.Is((~cmd).Err, Δexec.ErrDot)){
                    cmd.Value.Err = default!;
                } else {
                    tΔ1.Fatalf("cmd.Err = %v; want ErrDot"u8, (~cmd).Err);
                }
            }
            var (@out, err) = cmd.Output();
            if (err != default!) {
                {
                    var (ee, ok) = err._<ж<Δexec.ExitError>>(ᐧ); if (ok && len((~ee).Stderr) > 0){
                        tΔ1.Logf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, (~ee).Stderr);
                    } else {
                        tΔ1.Logf("%v: %v"u8, cmd.OrTypedNil(), err);
                    }
                }
                if (!errors.Is(err, ttʗ1.wantRunErr)) {
                    tΔ1.Errorf("want %v"u8, ttʗ1.wantRunErr);
                }
                return;
            }
            @string got = strings.TrimSpace(((@string)@out));
            if (filepath.IsAbs(got)) {
                (got, err) = filepath.Rel(root, got);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            if (got != ttʗ1.want) {
                tΔ1.Errorf("\nran  %#q\nwant %#q"u8, got, ttʗ1.want);
            }
            @string gotPath = cmd.Value.Path;
            @string wantPath = ttʗ1.wantPath;
            if (wantPath == ""u8) {
                if (strings.Contains(ttʗ1.arg0, @"\"u8)){
                    wantPath = ttʗ1.arg0;
                } else 
                if (ttʗ1.wantErrDot){
                    wantPath = strings.TrimPrefix(ttʗ1.want, ttʗ1.dir + @"\"u8);
                } else {
                    wantPath = filepath.Join(root, ttʗ1.want);
                }
            }
            if (gotPath != wantPath) {
                tΔ1.Errorf("\ncmd.Path = %#q\nwant       %#q"u8, gotPath, wantPath);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleComˢ = "example.com"u8;

public static void TestAbsCommandWithDoubledExtension(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    // We expect that ".com" is always included in PATHEXT, but it may also be
    // found in the import path of a Go package. If it is at the root of the
    // import path, the resulting executable may be named like "example.com.exe".
    //
    // Since "example.com" looks like a proper executable name, it is probably ok
    // for exec.Command to try to run it directly without re-resolving it.
    // However, exec.LookPath should try a little harder to figure it out.
    @string comPath = filepath.Join(Ꮡt.TempDir(), exampleComˢ);
    @string batPath = comPath + ".bat"u8;
    installBat(Ꮡt, batPath);
    var cmd = Δexec.Command(comPath);
    var (@out, err) = cmd.CombinedOutput();
    Ꮡt.Logf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, @out);
    if (!errors.Is(err, fs.ErrNotExist)) {
        Ꮡt.Errorf("Command(%#q).Run: %v\nwant fs.ErrNotExist"u8, comPath, err);
    }
    (var resolved, err) = Δexec.LookPath(comPath);
    if (err != default! || resolved != batPath) {
        Ꮡt.Fatalf("LookPath(%#q) = %v, %v; want %#q, <nil>"u8, comPath, resolved, err, batPath);
    }
}

} // end exec_test_package
