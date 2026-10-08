// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using os = os_package;
using exec = go.os.exec_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = go.sync_package;
using testing = testing_package;
using time = time_package;
using go;
using go.os;
using ꓸꓸꓸstring = Span<@string>;

partial class testenv_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skippingTestˢ = (@string)"skipping test:"u8;

// MustHaveExec checks that the current system can start new processes
// using os.StartProcess or (more commonly) exec.Command.
// If not, MustHaveExec calls t.Skip with an explanation.
//
// On some platforms MustHaveExec checks for exec support by re-executing the
// current executable, which must be a binary built by 'go test'.
// We intentionally do not provide a HasExec function because of the risk of
// inappropriate recursion in TestMain functions.
//
// To check for exec support outside of a test, just try to exec the command.
// If exec is not supported, testenv.SyscallIsNotSupported will return true
// for the resulting error.
public static void MustHaveExec(testing.TB t) {
    {
        var err = tryExec(); if (err != default!) {
            @string msg = fmt.Sprintf("cannot exec subprocess on %s/%s: %v"u8, Δruntime.GOOS, Δruntime.GOARCH, err);
            if (t == default!) {
                throw panic(msg);
            }
            t.Helper();
            t.Skip(skippingTestˢ, msg);
        }
    }
}

// Assume that exec always works on non-mobile platforms and Android.
// ios has an exec syscall but on real iOS devices it might return a
// permission error. In an emulated environment (such as a Corellium host)
// it might succeed, so if we need to exec we'll just have to try it and
// find out.
//
// As of 2023-04-19 wasip1 and js don't have exec syscalls at all, but we
// may as well use the same path so that this branch can be tested without
// an ios environment.
// This isn't a standard 'go test' binary, so we don't know how to
// self-exec in a way that should succeed without side effects.
// Just forget it.
// We know that this is a test executable. We should be able to run it with a
// no-op flag to check for overall exec support.
internal static Func<error> tryExec;
internal static void initᴛtryExec() { tryExec = sync.OnceValue(error () => {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "wasip1"u8 || exprᴛ1 == "js"u8 || exprᴛ1 == "ios"u8) {
    }
    else { /* default: */
        return default!;
    }

    if (!testing.Testing()) {
        return errors.New("can't probe for exec support with a non-test executable"u8);
    }
    var (exe, err) = exePath();
    if (err != default!) {
        return fmt.Errorf("can't probe for exec support: %w"u8, err);
    }
    var cmd = exec.Command(exe, "-test.list=^$"u8);
    cmd.Value.Env = origEnv;
    return cmd.Run();
}); }

// Executable is a wrapper around [MustHaveExec] and [os.Executable].
// It returns the path name for the executable that started the current process,
// or skips the test if the current system can't start new processes,
// or fails the test if the path can not be obtained.
public static @string Executable(testing.TB t) {
    MustHaveExec(t);
    var (exe, err) = exePath();
    if (err != default!) {
        @string msg = fmt.Sprintf("os.Executable error: %v"u8, err);
        if (t == default!) {
            throw panic(msg);
        }
        t.Fatal(msg);
    }
    return exe;
}

internal static Func<(@string, error)> exePath = sync.OnceValues((@string, error) () => os.Executable());

internal static ж<sync.Map> ᏑexecPaths = new StandardBox<sync.Map>(default(sync.Map));
internal static ref sync.Map execPaths => ref ᏑexecPaths.Value; // path -> error

// MustHaveExecPath checks that the current system can start the named executable
// using os.StartProcess or (more commonly) exec.Command.
// If not, MustHaveExecPath calls t.Skip with an explanation.
public static void MustHaveExecPath(testing.TB t, @string path) {
    MustHaveExec(t);
    var (err, found) = ᏑexecPaths.Load(path);
    if (!found) {
        (_, err) = exec.LookPath(path);
        (err, _) = ᏑexecPaths.LoadOrStore(path, err);
    }
    if (err != default!) {
        t.Helper();
        t.Skipf("skipping test: %s: %s"u8, path, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string godebugˢ = "GODEBUG="u8;
private static readonly @string gotracebackˢ = "GOTRACEBACK="u8;

// CleanCmdEnv will fill cmd.Env with the environment, excluding certain
// variables that could modify the behavior of the Go tools such as
// GODEBUG and GOTRACEBACK.
//
// If the caller wants to set cmd.Dir, set it before calling this function,
// so PWD will be set correctly in the environment.
public static ж<exec.Cmd> CleanCmdEnv(ж<exec.Cmd> Ꮡcmd) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    if (cmd.Env != default!) {
        throw panic("environment already set");
    }
    foreach (var (_, env) in cmd.Environ()) {
        // Exclude GODEBUG from the environment to prevent its output
        // from breaking tests that are trying to parse other command output.
        if (strings.HasPrefix(env, godebugˢ)) {
            continue;
        }
        // Exclude GOTRACEBACK for the same reason.
        if (strings.HasPrefix(env, gotracebackˢ)) {
            continue;
        }
        cmd.Env = append(cmd.Env, env);
    }
    return Ꮡcmd;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goTestTimeoutScaleˢ = "GO_TEST_TIMEOUT_SCALE"u8;

internal partial interface CommandContext_type /*dyn*/ :
    testing.TB
{
    (time.Time, bool) Deadline();
}

// CommandContext is like exec.CommandContext, but:
//   - skips t if the platform does not support os/exec,
//   - sends SIGQUIT (if supported by the platform) instead of SIGKILL
//     in its Cancel function
//   - if the test has a deadline, adds a Context timeout and WaitDelay
//     for an arbitrary grace period before the test's deadline expires,
//   - fails the test if the command does not complete before the test's deadline, and
//   - sets a Cleanup function that verifies that the test did not leak a subprocess.
public static ж<exec.Cmd> CommandContext(testing.TB t, context.Context ctx, @string name, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    MustHaveExec(t);
    Action cancelCtx = default!;
    time.Duration gracePeriod = default!;                 // unlimited unless the test has a deadline (to allow for interactive debugging)
    {
        var (tΔ1, ok) = t._<CommandContext_type>(ᐧ); if (ok) {
            {
                var (td, okΔ1) = tΔ1.Deadline(); if (okΔ1) {
                    // Start with a minimum grace period, just long enough to consume the
                    // output of a reasonable program after it terminates.
                    gracePeriod = 100 * time.Millisecond;
                    {
                        @string s = os.Getenv(goTestTimeoutScaleˢ); if (s != ""u8) {
                            var (scale, err) = strconv.Atoi(s);
                            if (err != default!) {
                                tΔ1.Fatalf("invalid GO_TEST_TIMEOUT_SCALE: %v"u8, err);
                            }
                            gracePeriod *= ((time.Duration)(int64)scale);
                        }
                    }
                    // If time allows, increase the termination grace period to 5% of the
                    // test's remaining time.
                    var testTimeout = time.Until(td);
                    {
                        var gp = testTimeout / 20; if (gp > gracePeriod) {
                            gracePeriod = gp;
                        }
                    }
                    // When we run commands that execute subprocesses, we want to reserve two
                    // grace periods to clean up: one for the delay between the first
                    // termination signal being sent (via the Cancel callback when the Context
                    // expires) and the process being forcibly terminated (via the WaitDelay
                    // field), and a second one for the delay between the process being
                    // terminated and the test logging its output for debugging.
                    //
                    // (We want to ensure that the test process itself has enough time to
                    // log the output before it is also terminated.)
                    var cmdTimeout = testTimeout - 2 * gracePeriod;
                    {
                        var (cd, okΔ2) = ctx.Deadline(); if (!okΔ2 || time.Until(cd) > cmdTimeout) {
                            // Either ctx doesn't have a deadline, or its deadline would expire
                            // after (or too close before) the test has already timed out.
                            // Add a shorter timeout so that the test will produce useful output.
                            (ctx, cancelCtx) = context.WithTimeout(ctx, cmdTimeout);
                        }
                    }
                }
            }
        }
    }
    var cmd = exec.CommandContext(ctx, name, args.ꓸꓸꓸ);
    var cancelCtxʗ1 = cancelCtx;
    var cmdʗ1 = cmd;
    cmd.Value.Cancel = () => {
        if (cancelCtxʗ1 != default! && AreEqual(ctx.Err(), context.DeadlineExceeded)){
            // The command timed out due to running too close to the test's deadline.
            // There is no way the test did that intentionally — it's too close to the
            // wire! — so mark it as a test failure. That way, if the test expects the
            // command to fail for some other reason, it doesn't have to distinguish
            // between that reason and a timeout.
            t.Errorf("test timed out while running command: %v"u8, cmdʗ1.OrTypedNil());
        } else {
            // The command is being terminated due to ctx being canceled, but
            // apparently not due to an explicit test deadline that we added.
            // Log that information in case it is useful for diagnosing a failure,
            // but don't actually fail the test because of it.
            t.Logf("%v: terminating command: %v"u8, ctx.Err(), cmdʗ1.OrTypedNil());
        }
        return (~cmdʗ1).Process.Signal(Sigquit);
    };
    cmd.Value.WaitDelay = gracePeriod;
    var cancelCtxʗ2 = cancelCtx;
    var cmdʗ2 = cmd;
    t.Cleanup(() => {
        if (cancelCtxʗ2 != default!) {
            cancelCtxʗ2();
        }
        if ((~cmdʗ2).Process != nil && (~cmdʗ2).ProcessState == nil) {
            t.Errorf("command was started, but test did not wait for it to complete: %v"u8, cmdʗ2.OrTypedNil());
        }
    });
    return cmd;
}

// Command is like exec.Command, but applies the same changes as
// testenv.CommandContext (with a default Context).
public static ж<exec.Cmd> Command(testing.TB t, @string name, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    return CommandContext(t, context.Background(), name, args.ꓸꓸꓸ);
}

} // end testenv_package
