// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go.os;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using os = os_package;
using Δexec = go.os.exec_package;
using strconv = strconv_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using @internal;
using go.os;
using static go.os.exec_internal_test_package;
using ꓸꓸꓸstring = Span<@string>;

partial class exec_test_package {

internal static osꓸSignal quitSignal = default;
internal static osꓸSignal pipeSignal = new exec_test_package.syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)syscall.SIGPIPE));

[GoInit] internal static void initΔ1() {
    registerHelperCommand("pipehandle", cmdPipeHandle);
}

internal static void cmdPipeHandle(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var (handle, _) = strconv.ParseUint(args[0], 16, 64);
    var pipe = os.NewFile((uintptr)handle, ""u8);
    var (_, err) = fmt.Fprint(new os.FileжWriter(pipe), args[1]);
    if (err != nil) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "writing to pipe failed: %v\n"u8, err);
        os.Exit(1);
    }
    pipe.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pipehandleˢ => ref pipehandleˢᵇ;
private static readonly @string pipehandleˢᵇ = "pipehandle"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string markerᶜ => ref markerᶜᵇ;
private static readonly @string markerᶜᵇ = "arrakis, dune, desert planet"u8;

public static void TestPipePassing(ж<testing.T> t) {
    t.Parallel();

    var (r, w, err) = os.Pipe();
    if (err != nil) {
        t.Error(err);
    }
    @string marker = markerᶜ;
    var childProc = helperCommand(t, pipehandleˢ, strconv.FormatUint((uint64)w.Fd(), 16), marker);
    childProc.SysProcAttr = Ꮡ(new syscall.SysProcAttr(AdditionalInheritedHandles: slice<syscallꓸHandle>([(syscallꓸHandle)w.Fd()])));
    err = childProc.Start();
    if (err != nil) {
        t.Error(err);
    }
    w.Close();
    (var response, err) = io.ReadAll(new exec_test_package.os_FileжReader(r));
    if (err != nil) {
        t.Error(err);
    }
    r.Close();
    if (((sstring)response) != marker) {
        t.Errorf("got %q; want %q", (@string)response, marker);
    }
    err = childProc.Wait();
    if (err != nil) {
        t.Error(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cmdˢ => ref cmdˢᵇ;
private static readonly @string cmdˢᵇ = "cmd"u8;
internal static ref readonly @string cExit88ˢ => ref cExit88ˢᵇ;
private static readonly @string cExit88ˢᵇ = "/c exit 88"u8;

public static void TestNoInheritHandles(ж<testing.T> t) {
    t.Parallel();

    var cmd = testenv.Command(new exec_test_package.testing_TжTB(t), cmdˢ, cExit88ˢ);
    cmd.SysProcAttr = Ꮡ(new syscall.SysProcAttr(NoInheritHandles: true));
    var err = cmd.Run();
    var (exitError, ok) = err._<ж<Δexec.ExitError>>(ᐧ);
    if (!ok) {
        t.Fatalf("got error %v; want ExitError", err);
    }
    if (exitError.Value.ProcessState.ExitCode() != 88) {
        t.Fatalf("got exit code %d; want 88", exitError.Value.ProcessState.ExitCode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string systemrootˢ => ref systemrootˢᵇ;
private static readonly @string systemrootˢᵇ = "SYSTEMROOT"u8;
internal static readonly object noSystemrootFoundˢ = (@string)"no SYSTEMROOT found"u8;

// start a child process without the user code explicitly starting
// with a copy of the parent's SYSTEMROOT.
// (See issue 25210.)
public static void TestChildCriticalEnv(ж<testing.T> t) {
    var envᴷ = new stackAppend<@string>(moves: true);

    t.Parallel();
    var cmd = helperCommand(t, echoenvˢ, systemrootˢ);

    // Explicitly remove SYSTEMROOT from the command's environment.
    slice<@string> env = default;
    foreach (var (_, kv) in range(cmd.Environ())) {
        var (k, _, ok) = strings.Cut(kv, "=");
        if (!ok || !strings.EqualFold(k, systemrootˢ)) {
            env = append(envᴷ, env, kv);
        }
    }
    env = move2heap(envᴷ, env);
    cmd.Env = env;

    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatal(err);
    }
    if (strings.TrimSpace((@string)@out) == ""u8) {
        t.Error(noSystemrootFoundˢ);
    }
}

} // end exec_test_package
