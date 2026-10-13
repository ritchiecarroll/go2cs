// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using platform = go.@internal.platform_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using exec = go.os.exec_package;
using fs = io.fs_package;
using go.@internal;
using go.os;
using path;

partial class testenv_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string exeˢ = ".exe"u8;

public static void TestGoToolLocation(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new testing_TжTB(Ꮡt));

    @string exeSuffix = default!;
    if (Δruntime.GOOS == "windows"u8) {
        exeSuffix = exeˢ;
    }

    // Tests are defined to run within their package source directory,
    // and this package's source directory is $GOROOT/src/internal/testenv.
    // The 'go' command is installed at $GOROOT/bin/go, so if the environment
    // is correct then testenv.GoTool() should be identical to ../../../bin/go.

    @string relWant = "../../../bin/go"u8 + exeSuffix;
    var (absWant, err) = filepath.Abs(relWant);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    (var wantInfo, err) = os.Stat(absWant);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡt.Logf("found go tool at %q (%q)"u8, relWant, absWant);

    (var goTool, err) = testenv.GoTool();
    if (err != default!) {
        Ꮡt.Fatalf("testenv.GoTool(): %v"u8, err);
    }
    Ꮡt.Logf("testenv.GoTool() = %q"u8, goTool);

    (var gotInfo, err) = os.Stat(goTool);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!os.SameFile(wantInfo, gotInfo)) {
        Ꮡt.Fatalf("%q is not the same file as %q"u8, absWant, goTool);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nooptˢ = "-noopt"u8;
private static readonly @string mustHaveExecˢ = "MustHaveExec"u8;
private static readonly @string mustHaveExecPathˢ = "MustHaveExecPath"u8;
private static readonly @string mainGoˢ = "main.go"u8;
private static readonly @string buildˢ = "build"u8;

public static void TestHasGoBuild(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (!testenv.HasGoBuild()) {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
            // No exec syscall, so these shouldn't be able to 'go build'.
            Ꮡt.Logf("HasGoBuild is false on %s"u8, Δruntime.GOOS);
            return;
        }

        @string b = testenv.Builder();
        if (b == ""u8) {
            // We shouldn't make assumptions about what kind of sandbox or build
            // environment external Go users may be running in.
            Ꮡt.Skipf("skipping: 'go build' unavailable"u8);
        }

        // Since we control the Go builders, we know which ones ought
        // to be able to run 'go build'. Check that they can.
        //
        // (Note that we don't verify that any builders *can't* run 'go build'.
        // If a builder starts running 'go build' tests when it shouldn't,
        // we will presumably find out about it when those tests fail.)
        var exprᴛ2 = Δruntime.GOOS;
        if (exprᴛ2 == "ios"u8) {
            if (isCorelliumBuilder(b)){
                // The corellium environment is self-hosting, so it should be able
                // to build even though real "ios" devices can't exec.
            } else {
                // The usual iOS sandbox does not allow the app to start another
                // process. If we add builders on stock iOS devices, they presumably
                // will not be able to exec, so we may as well allow that now.
                Ꮡt.Logf("HasGoBuild is false on %s"u8, b);
                return;
            }
        }
        else if (exprᴛ2 == "android"u8) {
            if (isEmulatedBuilder(b) && platform.MustLinkExternal(Δruntime.GOOS, Δruntime.GOARCH, false)) {
                // As of 2023-05-02, the test environment on the emulated builders is
                // missing a C linker.
                Ꮡt.Logf("HasGoBuild is false on %s"u8, b);
                return;
            }
        }

        if (strings.Contains(b, nooptˢ)) {
            // The -noopt builder sets GO_GCFLAGS, which causes tests of 'go build' to
            // be skipped.
            Ꮡt.Logf("HasGoBuild is false on %s"u8, b);
            return;
        }

        Ꮡt.Fatalf("HasGoBuild unexpectedly false on %s"u8, b);
    }

    Ꮡt.Logf("HasGoBuild is true; checking consistency with other functions"u8);

    var hasExec = false;
    var hasExecGo = false;
    Ꮡt.Run(mustHaveExecˢ, (ж<testing.T> tΔ1) => {
        testenv.MustHaveExec(new testing_TжTB(tΔ1));
        hasExec = true;
    });
    Ꮡt.Run(mustHaveExecPathˢ, (ж<testing.T> tΔ2) => {
        testenv.MustHaveExecPath(new testing_TжTB(tΔ2), "go"u8);
        hasExecGo = true;
    });
    if (!hasExec) {
        Ꮡt.Errorf(@"MustHaveExec(t) skipped unexpectedly"u8);
    }
    if (!hasExecGo) {
        Ꮡt.Errorf(@"MustHaveExecPath(t, ""go"") skipped unexpectedly"u8);
    }

    @string dir = Ꮡt.TempDir();
    @string mainGo = filepath.Join(dir, mainGoˢ);
    {
        var errΔ1 = os.WriteFile(mainGo, slice<byte>("package main\nfunc main() {}\n"u8), 420); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), "go"u8, buildˢ, "-o", os.DevNull, mainGo);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%v: %v\n%s"u8, cmd.OrTypedNil(), err, @out);
    }
}

public static void TestMustHaveExec(ж<testing.T> Ꮡt) {
    var hasExec = false;
    Ꮡt.Run(mustHaveExecˢ, (ж<testing.T> tΔ1) => {
        testenv.MustHaveExec(new testing_TжTB(tΔ1));
        tΔ1.Logf("MustHaveExec did not skip"u8);
        hasExec = true;
    });

    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        if (hasExec) {
            // js and wasip1 lack an “exec” syscall.
            Ꮡt.Errorf("expected MustHaveExec to skip on %v"u8, Δruntime.GOOS);
        }
    }
    else if (exprᴛ1 == "ios"u8) {
        {
            @string b = testenv.Builder(); if (isCorelliumBuilder(b) && !hasExec) {
                // Most ios environments can't exec, but the corellium builder can.
                Ꮡt.Errorf("expected MustHaveExec not to skip on %v"u8, b);
            }
        }
    }
    else { /* default: */
        {
            @string b = testenv.Builder(); if (b != ""u8 && !hasExec) {
                Ꮡt.Errorf("expected MustHaveExec not to skip on %v"u8, b);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string helpˢ = "help"u8;
private static readonly @string pwdˢ = "PWD="u8;
private static readonly object pwdNotSetInCmdEnvˢ = (@string)"PWD not set in cmd.Env"u8;

public static void TestCleanCmdEnvPWD(ж<testing.T> Ꮡt) {
    // Test that CleanCmdEnv sets PWD if cmd.Dir is set.
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8) {
        Ꮡt.Skipf("PWD is not used on %s"u8, Δruntime.GOOS);
    }
    @string dir = Ꮡt.TempDir();
    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.GoToolPath(new testing_TжTB(Ꮡt)), helpˢ);
    cmd.Value.Dir = dir;
    cmd = testenv.CleanCmdEnv(cmd);

    foreach (var (_, env) in (~cmd).Env) {
        if (strings.HasPrefix(env, pwdˢ)) {
            @string pwd = strings.TrimPrefix(env, pwdˢ);
            if (pwd != dir) {
                Ꮡt.Errorf("unexpected PWD: want %s, got %s"u8, dir, pwd);
            }
            return;
        }
    }
    Ꮡt.Error(pwdNotSetInCmdEnvˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string corelliumˢ = "-corellium"u8;
private static readonly @string corelliumˢ2 = "_corellium"u8;

internal static bool isCorelliumBuilder(@string builderName) {
    // Support both the old infra's builder names and the LUCI builder names.
    // The former's names are ad-hoc so we could maintain this invariant on
    // the builder side. The latter's names are structured, and "corellium" will
    // appear as a "host" suffix after the GOOS and GOARCH, which always begin
    // with an underscore.
    return strings.HasSuffix(builderName, corelliumˢ) || strings.Contains(builderName, corelliumˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string emuˢ = "-emu"u8;
private static readonly @string emuˢ2 = "_emu"u8;

internal static bool isEmulatedBuilder(@string builderName) {
    // Support both the old infra's builder names and the LUCI builder names.
    // The former's names are ad-hoc so we could maintain this invariant on
    // the builder side. The latter's names are structured, and the signifier
    // of emulation "emu" will appear as a "host" suffix after the GOOS and
    // GOARCH because it modifies the run environment in such a way that it
    // the target GOOS and GOARCH may not match the host. This suffix always
    // begins with an underscore.
    return strings.HasSuffix(builderName, emuˢ) || strings.Contains(builderName, emuˢ2);
}

} // end testenv_test_package
