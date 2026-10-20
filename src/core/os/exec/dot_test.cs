// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using errors = errors_package;
using testenv = @internal.testenv_package;
using os = os_package;
using static go.os.exec_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using fs = go.io.fs_package;
using go.os;
using path;
using static go.os.exec_internal_test_package;
using Δexec = go.os.exec_package;

partial class exec_test_package {

internal static @string pathVar = ((Func<@string>)(() => {
    if (runtime.GOOS == "plan9"u8) {
        return "path"u8;
    }
    return "PATH"u8;
}))();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdirˢ = "testdir"u8;
internal static readonly @string execabsTestˢ = "execabs-test"u8;
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string pathˢ = "PATH"u8;
internal static readonly @string emptyˢ = "empty"u8;
internal static readonly @string dotˢ = "dot"u8;
internal static readonly @string dotdot1ˢ = "dotdot1"u8;
internal static readonly @string abcˢ = "abc/.."u8;
internal static readonly @string dotdot2ˢ = "dotdot2"u8;

public static void TestLookPath(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new exec_test_package.testing_TжTB(Ꮡt));
    // Not parallel: uses Chdir and Setenv.

    @string tmpDir = filepath.Join(Ꮡt.TempDir(), testdirˢ);
    {
        var err = os.Mkdir(tmpDir, 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }

    @string executable = execabsTestˢ;
    if (runtime.GOOS == "windows"u8) {
        executable += ".exe"u8;
    }
    {
        var err = os.WriteFile(filepath.Join(tmpDir, executable), new byte[]{1, 2, 3}.slice(), 511); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    Ꮡt.Chdir(tmpDir);
    Ꮡt.Logf(". is %#q"u8, tmpDir);

    @string origPath = os.Getenv(pathVar);

    // Add "." to PATH so that exec.LookPath looks in the current directory on all systems.
    // And try to trick it with "../testdir" too.
    foreach (var (_, errdot) in new @string[]{"1"u8, "0"u8}.slice()) {
        Ꮡt.Run("GODEBUG=execerrdot="u8 + errdot, (ж<testing.T> tΔ1) => {
            tΔ1.Setenv(godebugˢ, "execerrdot="u8 + errdot + ",execwait=2"u8);
            foreach (var (_, dir) in new @string[]{"."u8, "../testdir"u8}.slice()) {
                tΔ1.Run(pathVar + "="u8 + dir, (ж<testing.T> tΔ2) => {
                    tΔ2.Setenv(pathVar, dir + ((@string)(rune)filepath.ListSeparator) + origPath);
                    @string good = dir + "/execabs-test"u8;
                    {
                        var (found, errΔ1) = LookPath(good); if (errΔ1 != default! || !strings.HasPrefix(found, good)) {
                            tΔ2.Fatalf(@"LookPath(%#q) = %#q, %v, want ""%s..."", nil"u8, good, found, errΔ1, good);
                        }
                    }
                    if (runtime.GOOS == "windows"u8) {
                        good = dir + @"\execabs-test"u8;
                        {
                            var (found, errΔ2) = LookPath(good); if (errΔ2 != default! || !strings.HasPrefix(found, good)) {
                                tΔ2.Fatalf(@"LookPath(%#q) = %#q, %v, want ""%s..."", nil"u8, good, found, errΔ2, good);
                            }
                        }
                    }

                    var (_, err) = LookPath(execabsTestˢ);
                    if (errdot == "1"u8){
                        if (err == default!){
                            tΔ2.Fatalf("LookPath didn't fail when finding a non-relative path"u8);
                        } else 
                        if (!errors.Is(err, ErrDot)) {
                            tΔ2.Fatalf("LookPath returned unexpected error: want Is ErrDot, got %q"u8, err);
                        }
                    } else {
                        if (err != default!) {
                            tΔ2.Fatalf("LookPath failed unexpectedly: %v"u8, err);
                        }
                    }

                    var cmd = Command(execabsTestˢ);
                    if (errdot == "1"u8){
                        if ((~cmd).Err == default!){
                            tΔ2.Fatalf("Command didn't fail when finding a non-relative path"u8);
                        } else 
                        if (!errors.Is((~cmd).Err, ErrDot)) {
                            tΔ2.Fatalf("Command returned unexpected error: want Is ErrDot, got %q"u8, (~cmd).Err);
                        }
                        cmd.Value.Err = default!;
                    } else {
                        if ((~cmd).Err != default!) {
                            tΔ2.Fatalf("Command failed unexpectedly: %v"u8, err);
                        }
                    }

                    // Clearing cmd.Err should let the execution proceed,
                    // and it should fail because it's not a valid binary.
                    {
                        var errΔ1 = cmd.Run(); if (errΔ1 == default!){
                            tΔ2.Fatalf("Run did not fail: expected exec error"u8);
                        } else 
                        if (errors.Is(errΔ1, ErrDot)) {
                            tΔ2.Fatalf("Run returned unexpected error ErrDot: want error like ENOEXEC: %q"u8, errΔ1);
                        }
                    }
                });
            }
        });
    }

    // Test the behavior when the first entry in PATH is an absolute name for the
    // current directory.
    //
    // On Windows, "." may or may not be implicitly included before the explicit
    // %PATH%, depending on the process environment;
    // see https://go.dev/issue/4394.
    //
    // If the relative entry from "." resolves to the same executable as what
    // would be resolved from an absolute entry in %PATH% alone, LookPath should
    // return the absolute version of the path instead of ErrDot.
    // (See https://go.dev/issue/53536.)
    //
    // If PATH does not implicitly include "." (such as on Unix platforms, or on
    // Windows configured with NoDefaultCurrentDirectoryInExePath), then this
    // lookup should succeed regardless of the behavior for ".", so it may be
    // useful to run as a control case even on those platforms.
    Ꮡt.Run(pathVar + "=$PWD"u8, (ж<testing.T> tΔ3) => {
        tΔ3.Setenv(pathVar, tmpDir + ((@string)(rune)filepath.ListSeparator) + origPath);
        @string good = filepath.Join(tmpDir, execabsTestˢ);
        {
            var (found, err) = LookPath(good); if (err != default! || !strings.HasPrefix(found, good)) {
                tΔ3.Fatalf(@"LookPath(%#q) = %#q, %v, want \""%s...\"", nil"u8, good, found, err, good);
            }
        }

        {
            var (found, err) = LookPath(execabsTestˢ); if (err != default! || !strings.HasPrefix(found, good)) {
                tΔ3.Fatalf(@"LookPath(%#q) = %#q, %v, want \""%s...\"", nil"u8, execabsTestˢ, found, err, good);
            }
        }

        var cmd = Command(execabsTestˢ);
        if ((~cmd).Err != default!) {
            tΔ3.Fatalf("Command(%#q).Err = %v; want nil"u8, execabsTestˢ, (~cmd).Err);
        }
    });

    Ꮡt.Run(pathVar + "=$OTHER"u8, (ж<testing.T> tΔ4) => {
        // Control case: if the lookup returns ErrDot when PATH is empty, then we
        // know that PATH implicitly includes ".". If it does not, then we don't
        // expect to see ErrDot at all in this test (because the path will be
        // unambiguously absolute).
        var wantErrDot = false;
        tΔ4.Setenv(pathVar, ""u8);
        {
            var (foundΔ1, errΔ1) = LookPath(execabsTestˢ); if (errors.Is(errΔ1, ErrDot)){
                wantErrDot = true;
            } else 
            if (errΔ1 == default!) {
                tΔ4.Fatalf(@"with PATH='', LookPath(%#q) = %#q; want non-nil error"u8, execabsTestˢ, foundΔ1);
            }
        }

        // Set PATH to include an explicit directory that contains a completely
        // independent executable that happens to have the same name as an
        // executable in ".". If "." is included implicitly, looking up the
        // (unqualified) executable name will return ErrDot; otherwise, the
        // executable in "." should have no effect and the lookup should
        // unambiguously resolve to the directory in PATH.

        @string dir = tΔ4.TempDir();
        @string executableΔ1 = execabsTestˢ;
        if (runtime.GOOS == "windows"u8) {
            executableΔ1 += ".exe"u8;
        }
        {
            var errΔ2 = os.WriteFile(filepath.Join(dir, executableΔ1), new byte[]{1, 2, 3}.slice(), 511); if (errΔ2 != default!) {
                tΔ4.Fatal(errΔ2);
            }
        }
        tΔ4.Setenv(pathVar, dir + ((@string)(rune)filepath.ListSeparator) + origPath);

        var (found, err) = LookPath(execabsTestˢ);
        if (wantErrDot){
            @string wantFound = filepath.Join("."u8, executableΔ1);
            if (found != wantFound || !errors.Is(err, ErrDot)) {
                tΔ4.Fatalf(@"LookPath(%#q) = %#q, %v, want %#q, Is ErrDot"u8, execabsTestˢ, found, err, wantFound);
            }
        } else {
            @string wantFound = filepath.Join(dir, executableΔ1);
            if (found != wantFound || err != default!) {
                tΔ4.Fatalf(@"LookPath(%#q) = %#q, %v, want %#q, nil"u8, execabsTestˢ, found, err, wantFound);
            }
        }
    });

    Action<ж<testing.T>> checker(@string test) => (ж<testing.T> tΔ5) => {
            tΔ5.Helper();
            tΔ5.Logf("PATH=%s"u8, os.Getenv(pathˢ));
            var (p, err) = LookPath(test);
            if (err == default!) {
                tΔ5.Errorf("%q: error expected, got nil"u8, test);
            }
            if (p != ""u8) {
                tΔ5.Errorf("%q: path returned should be \"\". Got %q"u8, test, p);
            }
        };

    // Reference behavior for the next test
    var checkerʗ1 = checker;
    Ꮡt.Run(pathVar + "=$OTHER2"u8, (ж<testing.T> tΔ6) => {
        tΔ6.Run(emptyˢ, checkerʗ1(""u8));
        tΔ6.Run(dotˢ, checkerʗ1("."u8));
        tΔ6.Run(dotdot1ˢ, checkerʗ1(abcˢ));
        tΔ6.Run(dotdot2ˢ, checkerʗ1(".."u8));
    });

    // Test the behavior when PATH contains an executable file which is not a directory
    var checkerʗ2 = checker;
    Ꮡt.Run(pathVar + "=exe"u8, (ж<testing.T> tΔ7) => {
        // Inject an executable file (not a directory) in PATH.
        // Use our own binary os.Args[0].
        tΔ7.Setenv(pathVar, testenv.Executable(new exec_test_package.testing_TжTB(tΔ7)));
        tΔ7.Run(emptyˢ, checkerʗ2(""u8));
        tΔ7.Run(dotˢ, checkerʗ2("."u8));
        tΔ7.Run(dotdot1ˢ, checkerʗ2(abcˢ));
        tΔ7.Run(dotdot2ˢ, checkerʗ2(".."u8));
    });

    // Test the behavior when PATH contains an executable file which is not a directory
    var checkerʗ3 = checker;
    Ꮡt.Run(pathVar + "=exe/xx"u8, (ж<testing.T> tΔ8) => {
        // Inject an executable file (not a directory) in PATH.
        // Use our own binary os.Args[0].
        tΔ8.Setenv(pathVar, filepath.Join(testenv.Executable(new exec_test_package.testing_TжTB(tΔ8)), "xx"));
        tΔ8.Run(emptyˢ, checkerʗ3(""u8));
        tΔ8.Run(dotˢ, checkerʗ3("."u8));
        tΔ8.Run(dotdot1ˢ, checkerʗ3(abcˢ));
        tΔ8.Run(dotdot2ˢ, checkerʗ3(".."u8));
    });
}

} // end exec_test_package
