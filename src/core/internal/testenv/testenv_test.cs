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
private static ref readonly @string exeˢ => ref exeˢᵇ;
private static readonly @string exeˢᵇ = ".exe"u8;

public static void TestGoToolLocation(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new testing_TжTB(t));

    @string exeSuffix = default;
    if (Δruntime.GOOS == "windows"u8) {
        exeSuffix = exeˢ;
    }

    // Tests are defined to run within their package source directory,
    // and this package's source directory is $GOROOT/src/internal/testenv.
    // The 'go' command is installed at $GOROOT/bin/go, so if the environment
    // is correct then testenv.GoTool() should be identical to ../../../bin/go.

    @string relWant = "../../../bin/go"u8 + exeSuffix;
    var (absWant, err) = filepath.Abs(relWant);
    if (err != nil) {
        t.Fatal(err);
    }

    (var wantInfo, err) = os.Stat(absWant);
    if (err != nil) {
        t.Fatal(err);
    }
    t.Logf("found go tool at %q (%q)", relWant, absWant);

    (var goTool, err) = testenv.GoTool();
    if (err != nil) {
        t.Fatalf("testenv.GoTool(): %v", err);
    }
    t.Logf("testenv.GoTool() = %q", goTool);

    (var gotInfo, err) = os.Stat(goTool);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!os.SameFile(wantInfo, gotInfo)) {
        t.Fatalf("%q is not the same file as %q", absWant, goTool);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string nooptˢ => ref nooptˢᵇ;
private static readonly @string nooptˢᵇ = "-noopt"u8;
private static ref readonly @string mustHaveExecˢ => ref mustHaveExecˢᵇ;
private static readonly @string mustHaveExecˢᵇ = "MustHaveExec"u8;
private static ref readonly @string mustHaveExecPathˢ => ref mustHaveExecPathˢᵇ;
private static readonly @string mustHaveExecPathˢᵇ = "MustHaveExecPath"u8;
private static ref readonly @string mainGoˢ => ref mainGoˢᵇ;
private static readonly @string mainGoˢᵇ = "main.go"u8;
private static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;

public static void TestHasGoBuild(ж<testing.T> t) {
    if (!testenv.HasGoBuild()) {
        var exprᴛ1 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
            // No exec syscall, so these shouldn't be able to 'go build'.
            t.Logf("HasGoBuild is false on %s", Δruntime.GOOS);
            return;
        }}

        @string b = testenv.Builder();
        if (b == ""u8) {
            // We shouldn't make assumptions about what kind of sandbox or build
            // environment external Go users may be running in.
            t.Skipf("skipping: 'go build' unavailable");
        }

        // Since we control the Go builders, we know which ones ought
        // to be able to run 'go build'. Check that they can.
        //
        // (Note that we don't verify that any builders *can't* run 'go build'.
        // If a builder starts running 'go build' tests when it shouldn't,
        // we will presumably find out about it when those tests fail.)
        var exprᴛ2 = Δruntime.GOOS;
        switch (ᐧ) {
        case {} when exprᴛ2 == "ios"u8: {
            if (isCorelliumBuilder(b)) {
                // The corellium environment is self-hosting, so it should be able
                // to build even though real "ios" devices can't exec.
            } else {
                // The usual iOS sandbox does not allow the app to start another
                // process. If we add builders on stock iOS devices, they presumably
                // will not be able to exec, so we may as well allow that now.
                t.Logf("HasGoBuild is false on %s", b);
                return;
            }
            break;
        }
        case {} when exprᴛ2 == "android"u8: {
            if (isEmulatedBuilder(b) && platform.MustLinkExternal(Δruntime.GOOS, Δruntime.GOARCH, false)) {
                // As of 2023-05-02, the test environment on the emulated builders is
                // missing a C linker.
                t.Logf("HasGoBuild is false on %s", b);
                return;
            }
            break;
        }}

        if (strings.Contains(b, nooptˢ)) {
            // The -noopt builder sets GO_GCFLAGS, which causes tests of 'go build' to
            // be skipped.
            t.Logf("HasGoBuild is false on %s", b);
            return;
        }

        t.Fatalf("HasGoBuild unexpectedly false on %s", b);
    }

    t.Logf("HasGoBuild is true; checking consistency with other functions");

    var hasExec = false;
    var hasExecGo = false;
    t.Run(mustHaveExecˢ, (ж<testing.T> tΔ1) => {
        testenv.MustHaveExec(new testing_TжTB(tΔ1));
        hasExec = true;
    });
    t.Run(mustHaveExecPathˢ, (ж<testing.T> tΔ2) => {
        testenv.MustHaveExecPath(new testing_TжTB(tΔ2), "go");
        hasExecGo = true;
    });
    if (!hasExec) {
        t.Errorf(@"MustHaveExec(t) skipped unexpectedly");
    }
    if (!hasExecGo) {
        t.Errorf(@"MustHaveExecPath(t, ""go"") skipped unexpectedly");
    }

    @string dir = t.TempDir();
    @string mainGo = filepath.Join(dir, mainGoˢ);
    {
        var errΔ1 = os.WriteFile(mainGo, slice<byte>("package main\nfunc main() {}\n"u8), 420); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    var cmd = testenv.Command(new testing_TжTB(t), "go", buildˢ, "-o", os.DevNull, mainGo);
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("%v: %v\n%s", cmd.OrTypedNil(), err, @out);
    }
}

public static void TestMustHaveExec(ж<testing.T> t) {
    var hasExec = false;
    t.Run(mustHaveExecˢ, (ж<testing.T> tΔ1) => {
        testenv.MustHaveExec(new testing_TжTB(tΔ1));
        tΔ1.Logf("MustHaveExec did not skip");
        hasExec = true;
    });

    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        if (hasExec) {
            // js and wasip1 lack an “exec” syscall.
            t.Errorf("expected MustHaveExec to skip on %v", Δruntime.GOOS);
        }
        break;
    }
    case {} when exprᴛ1 == "ios"u8: {
        {
            @string b = testenv.Builder(); if (isCorelliumBuilder(b) && !hasExec) {
                // Most ios environments can't exec, but the corellium builder can.
                t.Errorf("expected MustHaveExec not to skip on %v", b);
            }
        }
        break;
    }
    default: {
        {
            @string b = testenv.Builder(); if (b != ""u8 && !hasExec) {
                t.Errorf("expected MustHaveExec not to skip on %v", b);
            }
        }
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string helpˢ => ref helpˢᵇ;
private static readonly @string helpˢᵇ = "help"u8;
private static ref readonly @string pwdˢ => ref pwdˢᵇ;
private static readonly @string pwdˢᵇ = "PWD="u8;
private static readonly object pwdNotSetInCmdEnvˢ = (@string)"PWD not set in cmd.Env"u8;

public static void TestCleanCmdEnvPWD(ж<testing.T> t) {
    // Test that CleanCmdEnv sets PWD if cmd.Dir is set.
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8: {
        t.Skipf("PWD is not used on %s", Δruntime.GOOS);
        break;
    }}
    @string dir = t.TempDir();
    var cmd = testenv.Command(new testing_TжTB(t), testenv.GoToolPath(new testing_TжTB(t)), helpˢ);
    cmd.Dir = dir;
    cmd = testenv.CleanCmdEnv(cmd);

    foreach (var (_, env) in cmd.Env) {
        if (strings.HasPrefix(env, pwdˢ)) {
            @string pwd = strings.TrimPrefix(env, pwdˢ);
            if (pwd != dir) {
                t.Errorf("unexpected PWD: want %s, got %s", dir, pwd);
            }
            return;
        }
    }
    t.Error(pwdNotSetInCmdEnvˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string corelliumˢ => ref corelliumˢᵇ;
private static readonly @string corelliumˢᵇ = "-corellium"u8;
private static ref readonly @string corelliumˢ2 => ref corelliumˢ2ᵇ;
private static readonly @string corelliumˢ2ᵇ = "_corellium"u8;

internal static bool isCorelliumBuilder(@string builderName) {
    // Support both the old infra's builder names and the LUCI builder names.
    // The former's names are ad-hoc so we could maintain this invariant on
    // the builder side. The latter's names are structured, and "corellium" will
    // appear as a "host" suffix after the GOOS and GOARCH, which always begin
    // with an underscore.
    return strings.HasSuffix(builderName, corelliumˢ) || strings.Contains(builderName, corelliumˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string emuˢ => ref emuˢᵇ;
private static readonly @string emuˢᵇ = "-emu"u8;
private static ref readonly @string emuˢ2 => ref emuˢ2ᵇ;
private static readonly @string emuˢ2ᵇ = "_emu"u8;

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
