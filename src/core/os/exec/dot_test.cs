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

internal static @string pathVar = func(@string () => {
    if (runtime.GOOS == "plan9"u8) {
        return "path";
    }
    return "PATH";
})();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdirˢ => ref testdirˢᵇ;
private static readonly @string testdirˢᵇ = "testdir"u8;
internal static ref readonly @string execabsTestˢ => ref execabsTestˢᵇ;
private static readonly @string execabsTestˢᵇ = "execabs-test"u8;
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string pathˢ => ref pathˢᵇ;
private static readonly @string pathˢᵇ = "PATH"u8;
internal static ref readonly @string emptyˢ => ref emptyˢᵇ;
private static readonly @string emptyˢᵇ = "empty"u8;
internal static ref readonly @string dotˢ => ref dotˢᵇ;
private static readonly @string dotˢᵇ = "dot"u8;
internal static ref readonly @string dotdot1ˢ => ref dotdot1ˢᵇ;
private static readonly @string dotdot1ˢᵇ = "dotdot1"u8;
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc/.."u8;
internal static ref readonly @string dotdot2ˢ => ref dotdot2ˢᵇ;
private static readonly @string dotdot2ˢᵇ = "dotdot2"u8;

public static void TestLookPath(ж<testing.T> t) {
    testenv.MustHaveExec(new exec_test_package.testing_TжTB(t));
    // Not parallel: uses Chdir and Setenv.

    @string tmpDir = filepath.Join(t.TempDir(), testdirˢ);
    {
        var err = os.Mkdir(tmpDir, 511); if (err != nil) {
            t.Fatal(err);
        }
    }

    @string executable = execabsTestˢ;
    if (runtime.GOOS == "windows"u8) {
        executable += ".exe"u8;
    }
    {
        var err = os.WriteFile(filepath.Join(tmpDir, executable), slice<byte>([1, 2, 3]), 511); if (err != nil) {
            t.Fatal(err);
        }
    }
    t.Chdir(tmpDir);
    t.Logf(". is %#q", tmpDir);

    @string origPath = os.Getenv(pathVar);

    // Add "." to PATH so that exec.LookPath looks in the current directory on all systems.
    // And try to trick it with "../testdir" too.
    foreach (var (_, errdot) in slice<@string>(["1", "0"])) {
        t.Run("GODEBUG=execerrdot="u8 + errdot, (ж<testing.T> tΔ1) => {
            tΔ1.Setenv(godebugˢ, "execerrdot="u8 + errdot + ",execwait=2"u8);
            foreach (var (_, dir) in slice<@string>([".", "../testdir"])) {
                tΔ1.Run(pathVar + "="u8 + dir, (ж<testing.T> tΔ2) => {
                    tΔ2.Setenv(pathVar, dir + ((@string)(rune)filepath.ListSeparator) + origPath);
                    @string good = dir + "/execabs-test"u8;
                    {
                        var (found, errΔ1) = LookPath(good); if (errΔ1 != nil || !strings.HasPrefix(found, good)) {
                            tΔ2.Fatalf(@"LookPath(%#q) = %#q, %v, want ""%s..."", nil", good, found, errΔ1, good);
                        }
                    }
                    if (runtime.GOOS == "windows"u8) {
                        good = dir + @"\execabs-test"u8;
                        {
                            var (found, errΔ2) = LookPath(good); if (errΔ2 != nil || !strings.HasPrefix(found, good)) {
                                tΔ2.Fatalf(@"LookPath(%#q) = %#q, %v, want ""%s..."", nil", good, found, errΔ2, good);
                            }
                        }
                    }

                    var (_, err) = LookPath(execabsTestˢ);
                    if (errdot == "1"u8) {
                        if (err == nil) {
                            tΔ2.Fatalf("LookPath didn't fail when finding a non-relative path");
                        } else if (!errors.Is(err, ErrDot)) {
                            tΔ2.Fatalf("LookPath returned unexpected error: want Is ErrDot, got %q", err);
                        }
                    } else {
                        if (err != nil) {
                            tΔ2.Fatalf("LookPath failed unexpectedly: %v", err);
                        }
                    }

                    var cmd = Command(execabsTestˢ);
                    if (errdot == "1"u8) {
                        if (cmd.Err == nil) {
                            tΔ2.Fatalf("Command didn't fail when finding a non-relative path");
                        } else if (!errors.Is(cmd.Err, ErrDot)) {
                            tΔ2.Fatalf("Command returned unexpected error: want Is ErrDot, got %q", cmd.Err);
                        }
                        cmd.Err = default;
                    } else {
                        if (cmd.Err != nil) {
                            tΔ2.Fatalf("Command failed unexpectedly: %v", err);
                        }
                    }

                    // Clearing cmd.Err should let the execution proceed,
                    // and it should fail because it's not a valid binary.
                    {
                        var errΔ1 = cmd.Run(); if (errΔ1 == nil) {
                            tΔ2.Fatalf("Run did not fail: expected exec error");
                        } else if (errors.Is(errΔ1, ErrDot)) {
                            tΔ2.Fatalf("Run returned unexpected error ErrDot: want error like ENOEXEC: %q", errΔ1);
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
    t.Run(pathVar + "=$PWD"u8, (ж<testing.T> tΔ3) => {
        tΔ3.Setenv(pathVar, tmpDir + ((@string)(rune)filepath.ListSeparator) + origPath);
        @string good = filepath.Join(tmpDir, execabsTestˢ);
        {
            var (found, err) = LookPath(good); if (err != nil || !strings.HasPrefix(found, good)) {
                tΔ3.Fatalf(@"LookPath(%#q) = %#q, %v, want \""%s...\"", nil", good, found, err, good);
            }
        }

        {
            var (found, err) = LookPath(execabsTestˢ); if (err != nil || !strings.HasPrefix(found, good)) {
                tΔ3.Fatalf(@"LookPath(%#q) = %#q, %v, want \""%s...\"", nil", execabsTestˢ, found, err, good);
            }
        }

        var cmd = Command(execabsTestˢ);
        if (cmd.Err != nil) {
            tΔ3.Fatalf("Command(%#q).Err = %v; want nil", execabsTestˢ, cmd.Err);
        }
    });

    t.Run(pathVar + "=$OTHER"u8, (ж<testing.T> tΔ4) => {
        // Control case: if the lookup returns ErrDot when PATH is empty, then we
        // know that PATH implicitly includes ".". If it does not, then we don't
        // expect to see ErrDot at all in this test (because the path will be
        // unambiguously absolute).
        var wantErrDot = false;
        tΔ4.Setenv(pathVar, ""u8);
        {
            var (foundΔ1, errΔ1) = LookPath(execabsTestˢ); if (errors.Is(errΔ1, ErrDot)) {
                wantErrDot = true;
            } else if (errΔ1 == nil) {
                tΔ4.Fatalf(@"with PATH='', LookPath(%#q) = %#q; want non-nil error", execabsTestˢ, foundΔ1);
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
            var errΔ2 = os.WriteFile(filepath.Join(dir, executableΔ1), slice<byte>([1, 2, 3]), 511); if (errΔ2 != nil) {
                tΔ4.Fatal(errΔ2);
            }
        }
        tΔ4.Setenv(pathVar, dir + ((@string)(rune)filepath.ListSeparator) + origPath);

        var (found, err) = LookPath(execabsTestˢ);
        if (wantErrDot) {
            @string wantFound = filepath.Join(".", executableΔ1);
            if (found != wantFound || !errors.Is(err, ErrDot)) {
                tΔ4.Fatalf(@"LookPath(%#q) = %#q, %v, want %#q, Is ErrDot", execabsTestˢ, found, err, wantFound);
            }
        } else {
            @string wantFound = filepath.Join(dir, executableΔ1);
            if (found != wantFound || err != nil) {
                tΔ4.Fatalf(@"LookPath(%#q) = %#q, %v, want %#q, nil", execabsTestˢ, found, err, wantFound);
            }
        }
    });

    Action<ж<testing.T>> checker(@string test) => (ж<testing.T> tΔ5) => {
            tΔ5.Helper();
            tΔ5.Logf("PATH=%s", os.Getenv(pathˢ));
            var (p, err) = LookPath(test);
            if (err == nil) {
                tΔ5.Errorf("%q: error expected, got nil", test);
            }
            if (p != ""u8) {
                tΔ5.Errorf("%q: path returned should be \"\". Got %q", test, p);
            }
        };

    // Reference behavior for the next test
    var checkerʗ1 = checker;
    t.Run(pathVar + "=$OTHER2"u8, (ж<testing.T> tΔ6) => {
        tΔ6.Run(emptyˢ, checkerʗ1(""u8));
        tΔ6.Run(dotˢ, checkerʗ1("."));
        tΔ6.Run(dotdot1ˢ, checkerʗ1(abcˢ));
        tΔ6.Run(dotdot2ˢ, checkerʗ1(".."));
    });

    // Test the behavior when PATH contains an executable file which is not a directory
    var checkerʗ2 = checker;
    t.Run(pathVar + "=exe"u8, (ж<testing.T> tΔ7) => {
        // Inject an executable file (not a directory) in PATH.
        // Use our own binary os.Args[0].
        tΔ7.Setenv(pathVar, testenv.Executable(new exec_test_package.testing_TжTB(tΔ7)));
        tΔ7.Run(emptyˢ, checkerʗ2(""u8));
        tΔ7.Run(dotˢ, checkerʗ2("."));
        tΔ7.Run(dotdot1ˢ, checkerʗ2(abcˢ));
        tΔ7.Run(dotdot2ˢ, checkerʗ2(".."));
    });

    // Test the behavior when PATH contains an executable file which is not a directory
    var checkerʗ3 = checker;
    t.Run(pathVar + "=exe/xx"u8, (ж<testing.T> tΔ8) => {
        // Inject an executable file (not a directory) in PATH.
        // Use our own binary os.Args[0].
        tΔ8.Setenv(pathVar, filepath.Join(testenv.Executable(new exec_test_package.testing_TжTB(tΔ8)), "xx"));
        tΔ8.Run(emptyˢ, checkerʗ3(""u8));
        tΔ8.Run(dotˢ, checkerʗ3("."));
        tΔ8.Run(dotdot1ˢ, checkerʗ3(abcˢ));
        tΔ8.Run(dotdot2ˢ, checkerʗ3(".."));
    });
}

} // end exec_test_package
