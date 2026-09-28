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

internal static osꓸSignal quitSignal = default!;
internal static osꓸSignal pipeSignal = new exec_test_package.syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)syscall.SIGPIPE));

[GoInit] internal static void initΔ1() {
    registerHelperCommand("pipehandle"u8, cmdPipeHandle);
}

internal static void cmdPipeHandle(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    var (handle, _) = strconv.ParseUint(args[0], 16, 64);
    var pipe = os.NewFile((uintptr)handle, ""u8);
    var (_, err) = fmt.Fprint(new os.FileжWriter(pipe), args[1]);
    if (err != default!) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "writing to pipe failed: %v\n"u8, err);
        os.Exit(1);
    }
    pipe.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pipehandleˢ = "pipehandle"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string markerᶜ = "arrakis, dune, desert planet"u8;

public static void TestPipePassing(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (r, w, err) = os.Pipe();
    if (err != default!) {
        Ꮡt.Error(err);
    }
    @string marker = markerᶜ;
    var childProc = helperCommand(Ꮡt, pipehandleˢ, strconv.FormatUint((uint64)w.Fd(), 16), marker);
    childProc.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(AdditionalInheritedHandles: new syscallꓸHandle[]{((syscallꓸHandle)w.Fd())}.slice()));
    err = childProc.Start();
    if (err != default!) {
        Ꮡt.Error(err);
    }
    w.Close();
    (var response, err) = io.ReadAll(new exec_test_package.os_FileжReader(r));
    if (err != default!) {
        Ꮡt.Error(err);
    }
    r.Close();
    if (((sstring)response) != marker) {
        Ꮡt.Errorf("got %q; want %q"u8, ((@string)response), marker);
    }
    err = childProc.Wait();
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmdˢ = "cmd"u8;
internal static readonly @string cExit88ˢ = "/c exit 88"u8;

public static void TestNoInheritHandles(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = testenv.Command(new exec_test_package.testing_TжTB(Ꮡt), cmdˢ, cExit88ˢ);
    cmd.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(NoInheritHandles: true));
    var err = cmd.Run();
    var (exitError, ok) = err._<ж<Δexec.ExitError>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("got error %v; want ExitError"u8, err);
    }
    if (exitError.Value.ProcessState.ExitCode() != 88) {
        Ꮡt.Fatalf("got exit code %d; want 88"u8, exitError.Value.ProcessState.ExitCode());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string systemrootˢ = "SYSTEMROOT"u8;
internal static readonly object noSystemrootFoundˢ = (@string)"no SYSTEMROOT found"u8;

// start a child process without the user code explicitly starting
// with a copy of the parent's SYSTEMROOT.
// (See issue 25210.)
public static void TestChildCriticalEnv(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var cmd = helperCommand(Ꮡt, echoenvˢ, systemrootˢ);
    // Explicitly remove SYSTEMROOT from the command's environment.
    slice<@string> env = default!;
    foreach (var (_, kv) in cmd.Environ()) {
        var (k, _, ok) = strings.Cut(kv, "="u8);
        if (!ok || !strings.EqualFold(k, systemrootˢ)) {
            env = append(env, kv);
        }
    }
    cmd.Value.Env = env;
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (strings.TrimSpace(((@string)@out)) == ""u8) {
        Ꮡt.Error(noSystemrootFoundˢ);
    }
}

} // end exec_test_package
