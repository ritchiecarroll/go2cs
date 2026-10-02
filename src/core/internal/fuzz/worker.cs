// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using context = context_package;
using sha256 = crypto.sha256_package;
using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using exec = global::go.os.exec_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using sync = global::go.sync_package;
using time = time_package;
using System.Runtime.CompilerServices;
using crypto;
using encoding;
using global::go;
using global::go.os;

partial class fuzz_package {

internal static time.Duration workerFuzzDuration => /* 100 * time.Millisecond */ 100000000;
internal static time.Duration workerTimeoutDuration => /* 1 * time.Second */ 1000000000;
internal static UntypedInt workerExitCode => 70;
internal static UntypedInt workerSharedMemSize => /* 100 << 20 */ 104857600; // 100 MB

// worker manages a worker process running a test binary. The worker object
// exists only in the coordinator (the process started by 'go test -fuzz').
// workerClient is used by the coordinator to send RPCs to the worker process,
// which handles them with workerServer.
[GoType] partial struct worker {
    internal @string dir;  // working directory, same as package directory
    internal @string binPath;  // path to test executable
    internal slice<@string> args; // arguments for test executable
    internal slice<@string> env; // environment for test executable
    internal ж<coordinator> coordinator;
    internal channel<ж<sharedMem>> memMu; // mutex guarding shared memory with worker; persists across processes.
    internal ж<exec.Cmd> cmd;  // current worker process
    internal ж<workerClient> client; // used to communicate with worker process
    internal error waitErr;         // last error returned by wait, set before termC is closed.
    internal bool interrupted;          // true after stop interrupts a running worker.
    internal channel<EmptyStruct> termC; // closed by wait when worker process terminates
}

internal static (ж<worker>, error) newWorker(ж<coordinator> Ꮡc, @string dir, @string binPath, slice<@string> args, slice<@string> env) {
    var (mem, err) = sharedMemTempFile(workerSharedMemSize);
    if (err != default!) {
        return (default!, err);
    }
    var memMu = new channel<ж<sharedMem>>(1);
    memMu.ᐸꟷ(mem);
    return (Ꮡ(new worker(
        dir: dir,
        binPath: binPath,
        args: args,
        env: env.slice(0, len(env), len(env)), // copy on append to ensure workers don't overwrite each other.

        coordinator: Ꮡc,
        memMu: memMu
    )), default!);
}

// cleanup releases persistent resources associated with the worker.
[GoRecv] internal static error cleanup(this ref worker w) {
    var mem = ᐸꟷ(w.memMu);
    if (mem == nil) {
        return default!;
    }
    close(w.memMu);
    return mem.Close();
}

// coordinate runs the test binary to perform fuzzing.
//
// coordinate loops until ctx is canceled or a fatal error is encountered.
// If a test process terminates unexpectedly while fuzzing, coordinate will
// attempt to restart and continue unless the termination can be attributed
// to an interruption (from a timer or the user).
//
// While looping, coordinate receives inputs from the coordinator, passes
// those inputs to the worker process, then passes the results back to
// the coordinator.
internal static error coordinate(this ж<worker> Ꮡw, context.Context ctx) {
    ref var w = ref Ꮡw.DerefOrNull();

    // Main event loop.
    while (ᐧ) {
        // Start or restart the worker if it's not running.
        if (!w.isRunning()) {
            {
                var err = Ꮡw.startAndPing(ctx); if (err != default!) {
                    return err;
                }
            }
        }
        var selᴛ7 = ctx.Done();
        var selᴛ8 = w.termC;
        var selᴛ9 = (~w.coordinator).inputC;
        var selᴛ10 = (~w.coordinator).minimizeC;
        switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ), ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
        case 0 when selᴛ7.ꟷᐳ(out _): {
            var err = Ꮡw.stop();
            if (err != default! && !w.interrupted && !isInterruptError(err)) {
                // Worker was told to stop.
                return err;
            }
            return ctx.Err();
        }
        case 1 when selᴛ8.ꟷᐳ(out _): {
            var err = Ꮡw.stop();
            if (w.interrupted) {
                // Worker process terminated unexpectedly while waiting for input.
                throw panic("worker interrupted after unexpected termination");
            }
            if (err == default! || isInterruptError(err)) {
                // Worker stopped, either by exiting with status 0 or after being
                // interrupted with a signal that was not sent by the coordinator.
                //
                // When the user presses ^C, on POSIX platforms, SIGINT is delivered to
                // all processes in the group concurrently, and the worker may see it
                // before the coordinator. The worker should exit 0 gracefully (in
                // theory).
                //
                // This condition is probably intended by the user, so suppress
                // the error.
                return default!;
            }
            {
                var (exitErr, ok) = err._<ж<exec.ExitError>>(ᐧ); if (ok && exitErr.Value.ProcessState.ExitCode() == workerExitCode) {
                    // Worker exited with a code indicating F.Fuzz was not called correctly,
                    // for example, F.Fail was called first.
                    return fmt.Errorf("fuzzing process exited unexpectedly due to an internal failure: %w"u8, err);
                }
            }
            return fmt.Errorf("fuzzing process hung or terminated unexpectedly: %w"u8, // Worker exited non-zero or was terminated by a non-interrupt
 // signal (for example, SIGSEGV) while fuzzing.
 err);
        }
        case 2 when selᴛ9.ꟷᐳ(out var input): {
            var args = new fuzzArgs( // TODO(jayconrod,katiehockman): if -keepfuzzing, restart worker.
 // Received input from coordinator.

                Limit: input.limit,
                Timeout: input.timeout,
                Warmup: input.warmup,
                CoverageData: input.coverageData
            );
            var (entry, resp, isInternalError, err) = w.client.fuzz(ctx, input.entry, args);
            var canMinimize = true;
            if (err != default!) {
                // Error communicating with worker.
                Ꮡw.stop();
                if (ctx.Err() != default!) {
                    // Timeout or interruption.
                    return ctx.Err();
                }
                if (w.interrupted) {
                    // Communication error before we stopped the worker.
                    // Report an error, but don't record a crasher.
                    return fmt.Errorf("communicating with fuzzing process: %v"u8, err);
                }
                {
                    var (sig, ok) = terminationSignal(w.waitErr); if (ok && !isCrashSignal(sig)) {
                        // Worker terminated by a signal that probably wasn't caused by a
                        // specific input to the fuzz function. For example, on Linux,
                        // the kernel (OOM killer) may send SIGKILL to a process using a lot
                        // of memory. Or the shell might send SIGHUP when the terminal
                        // is closed. Don't record a crasher.
                        return fmt.Errorf("fuzzing process terminated by unexpected signal; no crash will be recorded: %v"u8, w.waitErr);
                    }
                }
                if (isInternalError) {
                    // An internal error occurred which shouldn't be considered
                    // a crash.
                    return err;
                }
                // Unexpected termination. Set error message and fall through.
                // We'll restart the worker on the next iteration.
                // Don't attempt to minimize this since it crashed the worker.
                resp.Err = fmt.Sprintf("fuzzing process hung or terminated unexpectedly: %v"u8, w.waitErr);
                canMinimize = false;
            }
            var result = new fuzzResult(
                limit: input.limit,
                count: resp.Count,
                totalDuration: resp.TotalDuration,
                entryDuration: resp.InterestingDuration,
                entry: entry,
                crasherMsg: resp.Err,
                coverageData: resp.CoverageData,
                canMinimize: canMinimize
            );
            (~w.coordinator).resultC.ᐸꟷ(result);
            break;
        }
        case 3 when selᴛ10.ꟷᐳ(out var input): {
            var (result, err) = Ꮡw.minimize(ctx, // Received input to minimize from coordinator.
 input);
            if (err != default!) {
                // Error minimizing. Send back the original input. If it didn't cause
                // an error before, report it as causing an error now.
                // TODO: double-check this is handled correctly when
                // implementing -keepfuzzing.
                result = new fuzzResult(
                    entry: input.entry,
                    crasherMsg: input.crasherMsg,
                    canMinimize: false,
                    limit: input.limit
                );
                if (result.crasherMsg == ""u8) {
                    result.crasherMsg = err.Error();
                }
            }
            if (shouldPrintDebugInfo()) {
                w.coordinator.debugLogf(
                    "input minimized, id: %s, original id: %s, crasher: %t, originally crasher: %t, minimizing took: %s"u8,
                    result.entry.Path,
                    input.entry.Path,
                    result.crasherMsg != ""u8,
                    input.crasherMsg != ""u8,
                    result.totalDuration);
            }
            (~w.coordinator).resultC.ᐸꟷ(result);
            break;
        }}
    }
}

// minimize tells a worker process to attempt to find a smaller value that
// either causes an error (if we started minimizing because we found an input
// that causes an error) or preserves new coverage (if we started minimizing
// because we found an input that expands coverage).
internal static (fuzzResult min, error err) minimize(this ж<worker> Ꮡw, context.Context ctx, fuzzMinimizeInput input) {
    fuzzResult min = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        if ((~w.coordinator).opts.MinimizeTimeout != 0) {
            Action cancel = default!;
            (ctx, cancel) = context.WithTimeout(ctx, (~w.coordinator).opts.MinimizeTimeout);
            var cancelʗ1 = cancel;
            defer(cancelʗ1, ref ᒐ);
        }
        var args = new minimizeArgs(
            Limit: input.limit,
            Timeout: input.timeout,
            KeepCoverage: input.keepCoverage
        );
        (var entry, var resp, err) = w.client.minimize(ctx, input.entry, args);
        if (err != default!) {
            // Error communicating with worker.
            Ꮡw.stop();
            if (ctx.Err() != default! || w.interrupted || isInterruptError(w.waitErr)) {
                // Worker was interrupted, possibly by the user pressing ^C.
                // Normally, workers can handle interrupts and timeouts gracefully and
                // will return without error. An error here indicates the worker
                // may not have been in a good state, but the error won't be meaningful
                // to the user. Just return the original crasher without logging anything.
                (min, err) = (new fuzzResult(
                    entry: input.entry,
                    crasherMsg: input.crasherMsg,
                    coverageData: input.keepCoverage,
                    canMinimize: false,
                    limit: input.limit
                ), default!); goto ᒐdone;
            }
            (min, err) = (new fuzzResult(
                entry: entry,
                crasherMsg: fmt.Sprintf("fuzzing process hung or terminated unexpectedly while minimizing: %v"u8, err),
                canMinimize: false,
                limit: input.limit,
                count: resp.Count,
                totalDuration: resp.Duration
            ), default!); goto ᒐdone;
        }
        if (input.crasherMsg != ""u8 && resp.Err == ""u8) {
            (min, err) = (new fuzzResult(nil), fmt.Errorf("attempted to minimize a crash but could not reproduce"u8)); goto ᒐdone;
        }
        (min, err) = (new fuzzResult(
            entry: entry,
            crasherMsg: resp.Err,
            coverageData: resp.CoverageData,
            canMinimize: false,
            limit: input.limit,
            count: resp.Count,
            totalDuration: resp.Duration
        ), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (min, err);
}

[GoRecv] internal static bool isRunning(this ref worker w) {
    return w.cmd != nil;
}

// startAndPing starts the worker process and sends it a message to make sure it
// can communicate.
//
// startAndPing returns an error if any part of this didn't work, including if
// the context is expired or the worker process was interrupted before it
// responded. Errors that happen after start but before the ping response
// likely indicate that the worker did not call F.Fuzz or called F.Fail first.
// We don't record crashers for these errors.
internal static error startAndPing(this ж<worker> Ꮡw, context.Context ctx) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (ctx.Err() != default!) {
        return ctx.Err();
    }
    {
        var err = Ꮡw.start(); if (err != default!) {
            return err;
        }
    }
    {
        var err = w.client.ping(ctx); if (err != default!) {
            Ꮡw.stop();
            if (ctx.Err() != default!) {
                return ctx.Err();
            }
            if (isInterruptError(err)) {
                // User may have pressed ^C before worker responded.
                return err;
            }
            // TODO: record and return stderr.
            return fmt.Errorf("fuzzing process terminated without fuzzing: %w"u8, err);
        }
    }
    return default!;
}

// start runs a new worker process.
//
// If the process couldn't be started, start returns an error. Start won't
// return later termination errors from the process if they occur.
//
// If the process starts successfully, start returns nil. stop must be called
// once later to clean up, even if the process terminates on its own.
//
// When the process terminates, w.waitErr is set to the error (if any), and
// w.termC is closed.
[MethodImpl(MethodImplOptions.NoInlining)] internal static error /*err*/ start(this ж<worker> Ꮡw) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();

        if (w.isRunning()) {
            throw panic("worker already started");
        }
        w.waitErr = default!;
        w.interrupted = false;
        w.termC = default!;
        var cmd = exec.Command(w.binPath, w.args.ꓸꓸꓸ);
        cmd.Value.Dir = w.dir;
        cmd.Value.Env = w.env.slice(0, len(w.env), len(w.env)); // copy on append to ensure workers don't overwrite each other.
        // Create the "fuzz_in" and "fuzz_out" pipes so we can communicate with
        // the worker. We don't use stdin and stdout, since the test binary may
        // do something else with those.
        //
        // Each pipe has a reader and a writer. The coordinator writes to fuzzInW
        // and reads from fuzzOutR. The worker inherits fuzzInR and fuzzOutW.
        // The coordinator closes fuzzInR and fuzzOutW after starting the worker,
        // since we have no further need of them.
        (var fuzzInR, var fuzzInW, err) = os.Pipe();
        if (err != default!) {
            goto ᒐdone;
        }
        var fuzzInRʗ1 = fuzzInR;
        defer(() => fuzzInRʗ1.Close(), ref ᒐ);
        (var fuzzOutR, var fuzzOutW, err) = os.Pipe();
        if (err != default!) {
            fuzzInW.Close();
            goto ᒐdone;
        }
        var fuzzOutWʗ1 = fuzzOutW;
        defer(() => fuzzOutWʗ1.Close(), ref ᒐ);
        setWorkerComm(ref (cmd).DerefOrNull(), new workerComm(fuzzIn: fuzzInR, fuzzOut: fuzzOutW, memMu: w.memMu));
        // Start the worker process.
        {
            var errΔ1 = cmd.Start(); if (errΔ1 != default!) {
                fuzzInW.Close();
                fuzzOutR.Close();
                err = errΔ1; goto ᒐdone;
            }
        }
        // Worker started successfully.
        // After this, w.client owns fuzzInW and fuzzOutR, so w.client.Close must be
        // called later by stop.
        w.cmd = cmd;
        w.termC = new channel<EmptyStruct>(0);
        var comm = new workerComm(fuzzIn: fuzzInW, fuzzOut: fuzzOutR, memMu: w.memMu);
        var m = newMutator();
        w.client = newWorkerClient(comm, m);
        goǃ(() => {
            Ꮡw.Value.waitErr = Ꮡw.Value.cmd.Wait();
            close(Ꮡw.Value.termC);
        });
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

// stop tells the worker process to exit by closing w.client, then blocks until
// it terminates. If the worker doesn't terminate after a short time, stop
// signals it with os.Interrupt (where supported), then os.Kill.
//
// stop returns the error the process terminated with, if any (same as
// w.waitErr).
//
// stop must be called at least once after start returns successfully, even if
// the worker process terminates unexpectedly.
[MethodImpl(MethodImplOptions.NoInlining)] internal static error stop(this ж<worker> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    if (w.termC == default!) {
        throw panic("worker was not started successfully");
    }
    var selᴛ11 = w.termC;
    switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
    case 0 when selᴛ11.ꟷᐳ(out _): {
        if (w.client == nil) {
            // Worker already terminated.
            // stop already called.
            return w.waitErr;
        }
        w.client.Close();
        w.cmd = default!;
        w.client = default!;
        return w.waitErr;
    }
    default: {
        break;
    }}
    // Possible unexpected termination.
    // Worker still running.
    // Tell the worker to stop by closing fuzz_in. It won't actually stop until it
    // finishes with earlier calls.
    var closeC = new channel<EmptyStruct>(0);
    var closeCʗ1 = closeC;
    goǃ(() => {
        Ꮡw.Value.client.Close();
        close(closeCʗ1);
    });
    var sig = os.Interrupt;
    if (Δruntime.GOOS == "windows"u8) {
        // Per https://golang.org/pkg/os/#Signal, “Interrupt is not implemented on
        // Windows; using it with os.Process.Signal will return an error.”
        // Fall back to Kill instead.
        sig = os.ΔKill;
    }
    var t = time.NewTimer(workerTimeoutDuration);
    while (ᐧ) {
        var selᴛ12 = w.termC;
        var selᴛ13 = (~t).C;
        switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
        case 0 when selᴛ12.ꟷᐳ(out _): {
            t.Stop();
            ᐸꟷ(closeC);
            w.cmd = default!;
            w.client = default!;
            return w.waitErr;
        }
        case 1 when selᴛ13.ꟷᐳ(out _): {
            w.interrupted = true;
            var exprᴛ1 = sig;
            if (AreEqual(exprᴛ1, os.Interrupt)) {
                (~w.cmd).Process.Signal(sig);
                sig = os.ΔKill;
                t.Reset(workerTimeoutDuration);
            }
            else if (AreEqual(exprᴛ1, os.ΔKill)) {
                (~w.cmd).Process.Signal(sig);
                sig = default!;
                t.Reset(workerTimeoutDuration);
            }
            else if (AreEqual(exprᴛ1, default!)) {
                fmt.Fprintf((~w.coordinator).opts.Log, // Worker terminated.
 // Timer fired before worker terminated.
 // Try to stop the worker with SIGINT and wait a little longer.
 // Try to stop the worker with SIGKILL and keep waiting.
 // Still waiting. Print a message to let the user know why.
 "waiting for fuzzing process to terminate...\n"u8);
            }

            break;
        }}
    }
}

// RunFuzzWorker is called in a worker process to communicate with the
// coordinator process in order to fuzz random inputs. RunFuzzWorker loops
// until the coordinator tells it to stop.
//
// fn is a wrapper on the fuzz function. It may return an error to indicate
// a given input "crashed". The coordinator will also record a crasher if
// the function times out or terminates the process.
//
// RunFuzzWorker returns an error if it could not communicate with the
// coordinator process.
public static error RunFuzzWorker(context.Context ctx, Func<CorpusEntry, error> fn) {
    ref var comm = ref heap<workerComm>(out var Ꮡcomm);
    (comm, var err) = getWorkerComm();
    if (err != default!) {
        return err;
    }
    var srv = Ꮡ(new workerServer(
        workerComm: comm,
        fuzzFn: (CorpusEntry e) => {
            GoFrame ᒐ = default;
            try {
                var timer = time.AfterFunc((time.Duration)(10000000000L), () => {
                    throw panic("deadlocked!"); // this error message won't be printed
                });
                var timerʗ1 = timer;
                defer(() => timerʗ1.Stop(), ref ᒐ);
                var start = time.Now();
                var errΔ1 = fn(e);
                return (time.Since(start), errΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        },
        m: newMutator()
    ));
    return srv.serve(ctx);
}

// call is serialized and sent from the coordinator on fuzz_in. It acts as
// a minimalist RPC mechanism. Exactly one of its fields must be set to indicate
// which method to call.
[GoType] partial struct call {
    public ж<pingArgs> Ping;
    public ж<fuzzArgs> Fuzz;
    public ж<minimizeArgs> Minimize;
}

// minimizeArgs contains arguments to workerServer.minimize. The value to
// minimize is already in shared memory.
[GoType] public partial struct minimizeArgs {
    // Timeout is the time to spend minimizing. This may include time to start up,
    // especially if the input causes the worker process to terminated, requiring
    // repeated restarts.
    public time.Duration Timeout;
    // Limit is the maximum number of values to test, without spending more time
    // than Duration. 0 indicates no limit.
    public int64 Limit;
    // KeepCoverage is a set of coverage counters the worker should attempt to
    // keep in minimized values. When provided, the worker will reject inputs that
    // don't cause at least one of these bits to be set.
    public slice<byte> KeepCoverage;
    // Index is the index of the fuzz target parameter to be minimized.
    public nint Index;
}

// minimizeResponse contains results from workerServer.minimize.
[GoType] partial struct minimizeResponse {
    // WroteToMem is true if the worker found a smaller input and wrote it to
    // shared memory. If minimizeArgs.KeepCoverage was set, the minimized input
    // preserved at least one coverage bit and did not cause an error.
    // Otherwise, the minimized input caused some error, recorded in Err.
    public bool WroteToMem;
    // Err is the error string caused by the value in shared memory, if any.
    public @string Err;
    // CoverageData is the set of coverage bits activated by the minimized value
    // in shared memory. When set, it contains at least one bit from KeepCoverage.
    // CoverageData will be nil if Err is set or if minimization failed.
    public slice<byte> CoverageData;
    // Duration is the time spent minimizing, not including starting or cleaning up.
    public time.Duration Duration;
    // Count is the number of values tested.
    public int64 Count;
}

// fuzzArgs contains arguments to workerServer.fuzz. The value to fuzz is
// passed in shared memory.
[GoType] public partial struct fuzzArgs {
    // Timeout is the time to spend fuzzing, not including starting or
    // cleaning up.
    public time.Duration Timeout;
    // Limit is the maximum number of values to test, without spending more time
    // than Duration. 0 indicates no limit.
    public int64 Limit;
    // Warmup indicates whether this is part of a warmup run, meaning that
    // fuzzing should not occur. If coverageEnabled is true, then coverage data
    // should be reported.
    public bool Warmup;
    // CoverageData is the coverage data. If set, the worker should update its
    // local coverage data prior to fuzzing.
    public slice<byte> CoverageData;
}

// fuzzResponse contains results from workerServer.fuzz.
[GoType] partial struct fuzzResponse {
    // Duration is the time spent fuzzing, not including starting or cleaning up.
    public time.Duration TotalDuration;
    public time.Duration InterestingDuration;
    // Count is the number of values tested.
    public int64 Count;
    // CoverageData is set if the value in shared memory expands coverage
    // and therefore may be interesting to the coordinator.
    public slice<byte> CoverageData;
    // Err is the error string caused by the value in shared memory, which is
    // non-empty if the value in shared memory caused a crash.
    public @string Err;
    // InternalErr is the error string caused by an internal error in the
    // worker. This shouldn't be considered a crasher.
    public @string InternalErr;
}

// pingArgs contains arguments to workerServer.ping.
[GoType] public partial struct pingArgs {
}

// pingResponse contains results from workerServer.ping.
[GoType] partial struct pingResponse {
}

// workerComm holds pipes and shared memory used for communication
// between the coordinator process (client) and a worker process (server).
// These values are unique to each worker; they are shared only with the
// coordinator, not with other workers.
//
// Access to shared memory is synchronized implicitly over the RPC protocol
// implemented in workerServer and workerClient. During a call, the client
// (worker) has exclusive access to shared memory; at other times, the server
// (coordinator) has exclusive access.
[GoType] partial struct workerComm {
    internal ж<os.File> fuzzIn, fuzzOut;
    internal channel<ж<sharedMem>> memMu; // mutex guarding shared memory
}

// workerServer is a minimalist RPC server, run by fuzz worker processes.
// It allows the coordinator process (using workerClient) to call methods in a
// worker process. This system allows the coordinator to run multiple worker
// processes in parallel and to collect inputs that caused crashes from shared
// memory after a worker process terminates unexpectedly.
[GoType] partial struct workerServer {
    internal partial ref workerComm workerComm { get; }
    internal ж<mutator> m;
    // coverageMask is the local coverage data for the worker. It is
    // periodically updated to reflect the data in the coordinator when new
    // coverage is found.
    internal slice<byte> coverageMask;
    // fuzzFn runs the worker's fuzz target on the given input and returns an
    // error if it finds a crasher (the process may also exit or crash), and the
    // time it took to run the input. It sets a deadline of 10 seconds, at which
    // point it will panic with the assumption that the process is hanging or
    // deadlocked.
    internal Func<CorpusEntry, (time.Duration, error)> fuzzFn;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noArgumentsProvidedForˢ = "no arguments provided for any call"u8;

// serve reads serialized RPC messages on fuzzIn. When serve receives a message,
// it calls the corresponding method, then sends the serialized result back
// on fuzzOut.
//
// serve handles RPC calls synchronously; it will not attempt to read a message
// until the previous call has finished.
//
// serve returns errors that occurred when communicating over pipes. serve
// does not return errors from method calls; those are passed through serialized
// responses.
internal static error serve(this ж<workerServer> Ꮡws, context.Context ctx) {
    ref var ws = ref Ꮡws.DerefOrNull();

    var enc = json.NewEncoder(new os.FileжWriter(ws.fuzzOut));
    var dec = json.NewDecoder(new contextReaderжReader(Ꮡ(new contextReader(ctx: ctx, r: new os_FileжReader(ws.fuzzIn)))));
    while (ᐧ) {
        ref var c = ref heap(new call(), out var Ꮡc);
        {
            var err = dec.Decode(Ꮡc); if (err != default!) {
                if (AreEqual(err, io.EOF) || AreEqual(err, ctx.Err())){
                    return default!;
                } else {
                    return err;
                }
            }
        }
        any resp = default!;
        switch (ᐧ) {
        case {} when c.Fuzz != nil: {
            resp = Ꮡws.fuzz(ctx, c.Fuzz.Value);
            break;
        }
        case {} when c.Minimize != nil: {
            resp = Ꮡws.minimize(ctx, c.Minimize.Value);
            break;
        }
        case {} when c.Ping != nil: {
            resp = ws.ping(ctx, c.Ping.Value);
            break;
        }
        default: {
            return errors.New(noArgumentsProvidedForˢ);
        }}

        {
            var err = enc.Encode(resp); if (err != default!) {
                return err;
            }
        }
    }
}

// chainedMutations is how many mutations are applied before the worker
// resets the input to its original state.
// NOTE: this number was picked without much thought. It is low enough that
// it seems to create a significant diversity in mutated inputs. We may want
// to consider looking into this more closely once we have a proper performance
// testing framework. Another option is to randomly pick the number of chained
// mutations on each invocation of the workerServer.fuzz method (this appears to
// be what libFuzzer does, although there seems to be no documentation which
// explains why this choice was made.)
internal static UntypedInt chainedMutations => 5;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fuzzFunctionFailedWithNoˢ = "fuzz function failed with no input"u8;

// fuzz runs the test function on random variations of the input value in shared
// memory for a limited duration or number of iterations.
//
// fuzz returns early if it finds an input that crashes the fuzz function (with
// fuzzResponse.Err set) or an input that expands coverage (with
// fuzzResponse.InterestingDuration set).
//
// fuzz does not modify the input in shared memory. Instead, it saves the
// initial PRNG state in shared memory and increments a counter in shared
// memory before each call to the test function. The caller may reconstruct
// the crashing input with this information, since the PRNG is deterministic.
internal static fuzzResponse /*resp*/ fuzz(this ж<workerServer> Ꮡws, context.Context ctx, fuzzArgs args) {
    fuzzResponse resp = default!;
    GoFrame ᒐ = default;
    try {
        ref var ws = ref Ꮡws.DerefOrNull();

        if (args.CoverageData != default!) {
            if (ws.coverageMask != default! && len(args.CoverageData) != len(ws.coverageMask)) {
                resp.InternalErr = fmt.Sprintf("unexpected size for CoverageData: got %d, expected %d"u8, len(args.CoverageData), len(ws.coverageMask));
                goto ᒐdone;
            }
            ws.coverageMask = args.CoverageData;
        }
        ref var start = ref heap<time.Time>(out var Ꮡstart);
        start = time.Now();
        var startʗ1 = start;
        defer(() => {
            resp.TotalDuration = time.Since(startʗ1);
        }, ref ᒐ);
        if (args.Timeout != 0) {
            Action cancel = default!;
            (ctx, cancel) = context.WithTimeout(ctx, args.Timeout);
            var cancelʗ1 = cancel;
            defer(cancelʗ1, ref ᒐ);
        }
        var mem = ᐸꟷ(ws.memMu);
        (~ws.m).r.save(mem.header().of(sharedMemHeader.ᏑrandState), mem.header().of(sharedMemHeader.ᏑrandInc));
        var memʗ1 = mem;
        defer(() => {
            resp.Count = memʗ1.header().Value.count;
            Ꮡws.Value.memMu.ᐸꟷ(memʗ1);
        }, ref ᒐ);
        if (args.Limit > 0 && (~mem.header()).count >= args.Limit) {
            resp.InternalErr = fmt.Sprintf("mem.header().count %d already exceeds args.Limit %d"u8, (~mem.header()).count, args.Limit);
            goto ᒐdone;
        }
        var (originalVals, err) = unmarshalCorpusFile(mem.valueCopy());
        if (err != default!) {
            resp.InternalErr = err.Error();
            goto ᒐdone;
        }
        var vals = new slice<any>(len(originalVals));
        copy(vals, originalVals);
        var argsʗ1 = args;
        var memʗ2 = mem;
        bool shouldStop() => argsʗ1.Limit > 0 && (~memʗ2.header()).count >= argsʗ1.Limit;
        var memʗ3 = mem;
        (time.Duration dur, slice<byte> cov, @string errMsg) fuzzOnce(CorpusEntry entry) {
            time.Duration dur = default!;
            @string errMsg = default!;
            memʗ3.header().Value.count++;
            error errΔ1 = default!;
            (dur, errΔ1) = Ꮡws.Value.fuzzFn(entry);
            if (errΔ1 != default!) {
                errMsg = errΔ1.Error();
                if (errMsg == ""u8) {
                    errMsg = fuzzFunctionFailedWithNoˢ;
                }
                return (dur, default!, errMsg);
            }
            if (Ꮡws.Value.coverageMask != default! && countNewCoverageBits(Ꮡws.Value.coverageMask, coverageSnapshot) > 0) {
                return (dur, coverageSnapshot, "");
            }
            return (dur, default!, "");
        }
        if (args.Warmup) {
            var (dur, _, errMsg) = fuzzOnce(new CorpusEntry(Values: vals));
            if (errMsg != ""u8) {
                resp.Err = errMsg;
                goto ᒐdone;
            }
            resp.InterestingDuration = dur;
            if (coverageEnabled) {
                resp.CoverageData = coverageSnapshot;
            }
            goto ᒐdone;
        }
        while (ᐧ) {
            var selᴛ14 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
            case 0 when selᴛ14.ꟷᐳ(out _): {
                goto ᒐdone;
            }
            default: {
                if ((~mem.header()).count % (int64)chainedMutations == 0) {
                    copy(vals, originalVals);
                    (~ws.m).r.save(mem.header().of(sharedMemHeader.ᏑrandState), mem.header().of(sharedMemHeader.ᏑrandInc));
                }
                ws.m.mutate(vals, builtin.cap(mem.valueRef()));
                var entry = new CorpusEntry(Values: vals);
                var (dur, cov, errMsg) = fuzzOnce(entry);
                if (errMsg != ""u8) {
                    resp.Err = errMsg;
                    goto ᒐdone;
                }
                if (cov != default!) {
                    resp.CoverageData = cov;
                    resp.InterestingDuration = dur;
                    goto ᒐdone;
                }
                if (shouldStop()) {
                    goto ᒐdone;
                }
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return resp;
}

internal static minimizeResponse /*resp*/ minimize(this ж<workerServer> Ꮡws, context.Context ctx, minimizeArgs args) {
    minimizeResponse resp = default!;
    GoFrame ᒐ = default;
    try {
        ref var ws = ref Ꮡws.DerefOrNull();

        ref var start = ref heap<time.Time>(out var Ꮡstart);
        start = time.Now();
        var startʗ1 = start;
        defer(() => {
            resp.Duration = time.Since(startʗ1);
        }, ref ᒐ);
        var mem = ᐸꟷ(ws.memMu);
        var memʗ1 = mem;
        defer(() => {
            Ꮡws.Value.memMu.ᐸꟷ(memʗ1);
        }, ref ᒐ);
        var (vals, err) = unmarshalCorpusFile(mem.valueCopy());
        if (err != default!) {
            throw panic(err);
        }
        var inpHash = sha256.Sum256(mem.valueCopy());
        if (args.Timeout != 0) {
            Action cancel = default!;
            (ctx, cancel) = context.WithTimeout(ctx, args.Timeout);
            var cancelʗ1 = cancel;
            defer(cancelʗ1, ref ᒐ);
        }
        // Minimize the values in vals, then write to shared memory. We only write
        // to shared memory after completing minimization.
        (var success, err) = Ꮡws.minimizeInput(ctx, vals, mem, args);
        if (success) {
            writeToMem(vals, mem);
            var outHash = sha256.Sum256(mem.valueCopy());
            mem.header().Value.rawInMem = false;
            resp.WroteToMem = true;
            if (err != default!){
                resp.Err = err.Error();
            } else {
                // If the values didn't change during minimization then coverageSnapshot is likely
                // a dirty snapshot which represents the very last step of minimization, not the
                // coverage for the initial input. In that case just return the coverage we were
                // given initially, since it more accurately represents the coverage map for the
                // input we are returning.
                if (outHash != inpHash){
                    resp.CoverageData = coverageSnapshot;
                } else {
                    resp.CoverageData = args.KeepCoverage;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return resp;
}

// minimizeInput applies a series of minimizing transformations on the provided
// vals, ensuring that each minimization still causes an error, or keeps
// coverage, in fuzzFn. It uses the context to determine how long to run,
// stopping once closed. It returns a bool indicating whether minimization was
// successful and an error if one was found.
internal static (bool success, error retErr) minimizeInput(this ж<workerServer> Ꮡws, context.Context ctx, slice<any> vals, ж<sharedMem> Ꮡmem, minimizeArgs args) {
    error retErr = default!;

    ref var ws = ref Ꮡws.DerefOrNull();
    ref var mem = ref Ꮡmem.DerefOrNull();
    ref var keepCoverage = ref heap<slice<byte>>(out var ᏑkeepCoverage);
    keepCoverage = args.KeepCoverage;
    ref var memBytes = ref heap<slice<byte>>(out var ᏑmemBytes);
    memBytes = mem.valueRef();
    var bPtr = ᏑmemBytes;
    var count = mem.header().of(sharedMemHeader.Ꮡcount);
    var argsʗ1 = args;
    var countʗ1 = count;
    var shouldStop = () => ctx.Err() != default! || (argsʗ1.Limit > 0 && countʗ1.Value >= argsʗ1.Limit);
    if (shouldStop()) {
        return (false, default!);
    }
    // Check that the original value preserves coverage or causes an error.
    // If not, then whatever caused us to think the value was interesting may
    // have been a flake, and we can't minimize it.
    count.Value++;
    (_, retErr) = ws.fuzzFn(new CorpusEntry(Values: vals));
    if (keepCoverage != default!){
        if (!hasCoverageBit(keepCoverage, coverageSnapshot) || retErr != default!) {
            return (false, default!);
        }
    } else 
    if (retErr == default!) {
        return (false, default!);
    }
    mem.header().Value.rawInMem = true;
    // tryMinimized runs the fuzz function with candidate replacing the value
    // at index valI. tryMinimized returns whether the input with candidate is
    // interesting for the same reason as the original input: it returns
    // an error if one was expected, or it preserves coverage.
    var argsʗ2 = args;
    var bPtrʗ1 = bPtr;
    var countʗ2 = count;
    var valsʗ1 = vals;
    var tryMinimized = (slice<byte> candidate) => {
        var prev = valsʗ1[argsʗ2.Index];
        switch (prev.type()) {
        case slice<byte>: {
            valsʗ1[argsʗ2.Index] = candidate;
            break;
        }
        case @string: {
            valsʗ1[argsʗ2.Index] = ((@string)candidate);
            break;
        }
        default: {
            throw panic("impossible");
            break;
        }}

        copy(bPtrʗ1.ValueSlot, candidate);
        bPtrʗ1.ValueSlot = (bPtrʗ1.ValueSlot).slice(0, len(candidate));
        Ꮡmem.Value.setValueLen(len(candidate));
        countʗ2.Value++;
        var (_, err) = Ꮡws.Value.fuzzFn(new CorpusEntry(Values: valsʗ1));
        if (err != default!) {
            retErr = err;
            if (ᏑkeepCoverage.ValueSlot != default!) {
                // Now that we've found a crash, that's more important than any
                // minimization of interesting inputs that was being done. Clear out
                // keepCoverage to only minimize the crash going forward.
                ᏑkeepCoverage.ValueSlot = default!;
            }
            return true;
        }
        // Minimization should preserve coverage bits.
        if (ᏑkeepCoverage.ValueSlot != default! && isCoverageSubset(ᏑkeepCoverage.ValueSlot, coverageSnapshot)) {
            return true;
        }
        valsʗ1[argsʗ2.Index] = prev;
        return false;
    };
    switch (vals[args.Index].type()) {
    case @string v: {
        minimizeBytes(slice<byte>(v), tryMinimized, shouldStop);
        break;
    }
    case slice<byte> v: {
        minimizeBytes(v, tryMinimized, shouldStop);
        break;
    }
    default: {
        var v = vals[args.Index];
        throw panic("impossible");
        break;
    }}
    return (true, retErr);
}

internal static void writeToMem(slice<any> vals, ж<sharedMem> Ꮡmem) {
    ref var mem = ref Ꮡmem.DerefOrNull();

    var b = marshalCorpusFile(vals.ꓸꓸꓸ);
    mem.setValue(b);
}

// ping does nothing. The coordinator calls this method to ensure the worker
// has called F.Fuzz and can communicate.
[GoRecv] internal static pingResponse ping(this ref workerServer ws, context.Context ctx, pingArgs args) {
    return new pingResponse(nil);
}

// workerClient is a minimalist RPC client. The coordinator process uses a
// workerClient to call methods in each worker process (handled by
// workerServer).
[GoType] partial struct workerClient {
    internal partial ref workerComm workerComm { get; }
    internal ж<mutator> m;
    // mu is the mutex protecting the workerComm.fuzzIn pipe. This must be
    // locked before making calls to the workerServer. It prevents
    // workerClient.Close from closing fuzzIn while workerClient methods are
    // writing to it concurrently, and prevents multiple callers from writing to
    // fuzzIn concurrently.
    internal sync.Mutex mu;
}

internal static ж<workerClient> newWorkerClient(workerComm comm, ж<mutator> Ꮡm) {
    return Ꮡ(new workerClient(workerComm: comm, m: Ꮡm));
}

// Close shuts down the connection to the RPC server (the worker process) by
// closing fuzz_in. Close drains fuzz_out (avoiding a SIGPIPE in the worker),
// and closes it after the worker process closes the other end.
internal static error Close(this ж<workerClient> Ꮡwc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var wc = ref Ꮡwc.DerefOrNull();

        wc.mu.Lock();
        ᒐd1 = true;
        // Close fuzzIn. This signals to the server that there are no more calls,
        // and it should exit.
        {
            var err = wc.fuzzIn.Close(); if (err != default!) {
                wc.fuzzOut.Close();
                return err;
            }
        }
        // Drain fuzzOut and close it. When the server exits, the kernel will close
        // its end of fuzzOut, and we'll get EOF.
        {
            var (_, err) = io.Copy(io.Discard, new os_FileжReader(wc.fuzzOut)); if (err != default!) {
                wc.fuzzOut.Close();
                return err;
            }
        }
        return wc.fuzzOut.Close();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡwc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// errSharedMemClosed is returned by workerClient methods that cannot access
// shared memory because it was closed and unmapped by another goroutine. That
// can happen when worker.cleanup is called in the worker goroutine while a
// workerClient.fuzz call runs concurrently.
//
// This error should not be reported. It indicates the operation was
// interrupted.
internal static error errSharedMemClosed = errors.New("internal error: shared memory was closed and unmapped"u8);

// minimize tells the worker to call the minimize method. See
// workerServer.minimize.
internal static (CorpusEntry entryOut, minimizeResponse resp, error retErr) minimize(this ж<workerClient> Ꮡwc, context.Context ctx, CorpusEntry entryIn, minimizeArgs argsʗp) {
    CorpusEntry entryOut = default!;
    heap<minimizeResponse>(out var Ꮡresp);
    error retErr = default!;
    GoFrame ᒐ = default;
    try {
        ref var wc = ref Ꮡwc.DerefOrNull();

        ref var args = ref heap(argsʗp, out var Ꮡargs);
        ref var resp = ref Ꮡresp.Value;
        wc.mu.Lock();
        defer(Ꮡwc.of(workerClient.Ꮡmu).Unlock, ref ᒐ);
        ref var mem = ref heap<ж<sharedMem>>(out var Ꮡmem);
        (mem, var ok) = ᐸꟷ(wc.memMu, ꟷ);
        if (!ok) {
            (entryOut, resp, retErr) = (new CorpusEntry(), new minimizeResponse(nil), errSharedMemClosed); goto ᒐdone;
        }
        defer(() => {
            Ꮡwc.Value.memMu.ᐸꟷ(Ꮡmem.ValueSlot);
        }, ref ᒐ);
        mem.header().Value.count = 0;
        var (inp, err) = corpusEntryData(entryIn);
        if (err != default!) {
            (entryOut, resp, retErr) = (new CorpusEntry(), new minimizeResponse(nil), err); goto ᒐdone;
        }
        mem.setValue(inp);
        entryOut = entryIn;
        (entryOut.Values, err) = unmarshalCorpusFile(inp);
        if (err != default!) {
            (entryOut, resp, retErr) = (new CorpusEntry(), new minimizeResponse(nil), fmt.Errorf("workerClient.minimize unmarshaling provided value: %v"u8, err)); goto ᒐdone;
        }
        foreach (var (i, v) in entryOut.Values) {
            if (!isMinimizable(reflect.TypeOf(v))) {
                continue;
            }
            wc.memMu.ᐸꟷ(mem);
            args.Index = i;
            var c = new call(Minimize: Ꮡargs);
            var callErr = wc.callLocked(ctx, c, Ꮡresp);
            (mem, ok) = ᐸꟷ(wc.memMu, ꟷ);
            if (!ok) {
                (entryOut, resp, retErr) = (new CorpusEntry(), new minimizeResponse(nil), errSharedMemClosed); goto ᒐdone;
            }
            if (callErr != default!) {
                retErr = callErr;
                if (!(~mem.header()).rawInMem) {
                    // An unrecoverable error occurred before minimization began.
                    (entryOut, resp, retErr) = (entryIn, new minimizeResponse(nil), retErr); goto ᒐdone;
                }
                // An unrecoverable error occurred during minimization. mem now
                // holds the raw, unmarshaled bytes of entryIn.Values[i] that
                // caused the error.
                switch (entryOut.Values[i].type()) {
                case @string: {
                    entryOut.Values[i] = ((@string)mem.valueCopy());
                    break;
                }
                case slice<byte>: {
                    entryOut.Values[i] = mem.valueCopy();
                    break;
                }
                default: {
                    throw panic("impossible");
                    break;
                }}

                entryOut.Data = marshalCorpusFile(entryOut.Values.ꓸꓸꓸ);
                // Stop minimizing; another unrecoverable error is likely to occur.
                break;
            }
            if (resp.WroteToMem) {
                // Minimization succeeded, and mem holds the marshaled data.
                entryOut.Data = mem.valueCopy();
                (entryOut.Values, err) = unmarshalCorpusFile(entryOut.Data);
                if (err != default!) {
                    (entryOut, resp, retErr) = (new CorpusEntry(), new minimizeResponse(nil), fmt.Errorf("workerClient.minimize unmarshaling minimized value: %v"u8, err)); goto ᒐdone;
                }
            }
            // Prepare for next iteration of the loop.
            if (args.Timeout != 0) {
                args.Timeout -= resp.Duration;
                if (args.Timeout <= 0) {
                    break;
                }
            }
            if (args.Limit != 0) {
                args.Limit -= mem.header().Value.count;
                if (args.Limit <= 0) {
                    break;
                }
            }
        }
        resp.Count = mem.header().Value.count;
        var h = sha256.Sum256(entryOut.Data);
        entryOut.Path = fmt.Sprintf("%x"u8, h[..4]);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (entryOut, Ꮡresp.Value, retErr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string workerServerFuzzModifiedˢ = "workerServer.fuzz modified input"u8;

// fuzz tells the worker to call the fuzz method. See workerServer.fuzz.
internal static (CorpusEntry entryOut, fuzzResponse resp, bool isInternalError, error err) fuzz(this ж<workerClient> Ꮡwc, context.Context ctx, CorpusEntry entryIn, fuzzArgs argsʗp) {
    CorpusEntry entryOut = default!;
    heap<fuzzResponse>(out var Ꮡresp);
    bool isInternalError = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var wc = ref Ꮡwc.DerefOrNull();

        ref var args = ref heap(argsʗp, out var Ꮡargs);
        ref var resp = ref Ꮡresp.Value;
        wc.mu.Lock();
        defer(Ꮡwc.of(workerClient.Ꮡmu).Unlock, ref ᒐ);
        var (mem, ok) = ᐸꟷ(wc.memMu, ꟷ);
        if (!ok) {
            (entryOut, resp, isInternalError, err) = (new CorpusEntry(), new fuzzResponse(nil), true, errSharedMemClosed); goto ᒐdone;
        }
        mem.header().Value.count = 0;
        (var inp, err) = corpusEntryData(entryIn);
        if (err != default!) {
            wc.memMu.ᐸꟷ(mem);
            (entryOut, resp, isInternalError, err) = (new CorpusEntry(), new fuzzResponse(nil), true, err); goto ᒐdone;
        }
        mem.setValue(inp);
        wc.memMu.ᐸꟷ(mem);
        var c = new call(Fuzz: Ꮡargs);
        var callErr = wc.callLocked(ctx, c, Ꮡresp);
        if (resp.InternalErr != ""u8) {
            (entryOut, resp, isInternalError, err) = (new CorpusEntry(), new fuzzResponse(nil), true, errors.New(resp.InternalErr)); goto ᒐdone;
        }
        (mem, ok) = ᐸꟷ(wc.memMu, ꟷ);
        if (!ok) {
            (entryOut, resp, isInternalError, err) = (new CorpusEntry(), new fuzzResponse(nil), true, errSharedMemClosed); goto ᒐdone;
        }
        var memʗ1 = mem;
        defer(() => {
            Ꮡwc.Value.memMu.ᐸꟷ(memʗ1);
        }, ref ᒐ);
        resp.Count = mem.header().Value.count;
        if (!bytes.Equal(inp, mem.valueRef())) {
            (entryOut, resp, isInternalError, err) = (new CorpusEntry(), new fuzzResponse(nil), true, errors.New(workerServerFuzzModifiedˢ)); goto ᒐdone;
        }
        var needEntryOut = callErr != default! || resp.Err != ""u8 || (!args.Warmup && resp.CoverageData != default!);
        if (needEntryOut) {
            var (valuesOut, errΔ1) = unmarshalCorpusFile(inp);
            if (errΔ1 != default!) {
                (entryOut, resp, isInternalError, err) = (new CorpusEntry(), new fuzzResponse(nil), true, fmt.Errorf("unmarshaling fuzz input value after call: %v"u8, errΔ1)); goto ᒐdone;
            }
            (~wc.m).r.restore((~mem.header()).randState, (~mem.header()).randInc);
            if (!args.Warmup) {
                // Only mutate the valuesOut if fuzzing actually occurred.
                var numMutations = ((resp.Count - 1) % (int64)chainedMutations) + 1;
                for (var i = (int64)0; i < numMutations; i++) {
                    wc.m.mutate(valuesOut, builtin.cap(mem.valueRef()));
                }
            }
            var dataOut = marshalCorpusFile(valuesOut.ꓸꓸꓸ);
            var h = sha256.Sum256(dataOut);
            @string name = fmt.Sprintf("%x"u8, h[..4]);
            entryOut = new CorpusEntry(
                Parent: entryIn.Path,
                Path: name,
                Data: dataOut,
                Generation: entryIn.Generation + 1
            );
            if (args.Warmup) {
                // The bytes weren't mutated, so if entryIn was a seed corpus value,
                // then entryOut is too.
                entryOut.IsSeed = entryIn.IsSeed;
            }
        }
        (entryOut, resp, isInternalError, err) = (entryOut, resp, false, callErr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (entryOut, Ꮡresp.Value, isInternalError, err);
}

// ping tells the worker to call the ping method. See workerServer.ping.
internal static error ping(this ж<workerClient> Ꮡwc, context.Context ctx) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var wc = ref Ꮡwc.DerefOrNull();

        wc.mu.Lock();
        ᒐd1 = true;
        var c = new call(Ping: Ꮡ(new pingArgs(nil)));
        ref var resp = ref heap(new pingResponse(), out var Ꮡresp);
        return wc.callLocked(ctx, c, Ꮡresp);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡwc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

// callLocked sends an RPC from the coordinator to the worker process and waits
// for the response. The callLocked may be canceled with ctx.
[GoRecv] internal static error /*err*/ callLocked(this ref workerClient wc, context.Context ctx, call c, any resp) {
    var enc = json.NewEncoder(new os.FileжWriter(wc.fuzzIn));
    var dec = json.NewDecoder(new contextReaderжReader(Ꮡ(new contextReader(ctx: ctx, r: new os_FileжReader(wc.fuzzOut)))));
    {
        var errΔ1 = enc.Encode(c); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    return dec.Decode(resp);
}

// contextReader wraps a Reader with a Context. If the context is canceled
// while the underlying reader is blocked, Read returns immediately.
//
// This is useful for reading from a pipe. Closing a pipe file descriptor does
// not unblock pending Reads on that file descriptor. All copies of the pipe's
// other file descriptor (the write end) must be closed in all processes that
// inherit it. This is difficult to do correctly in the situation we care about
// (process group termination).
[GoType] partial struct contextReader {
    internal context.Context ctx;
    internal io.Reader r;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static (nint, error) Read(this ж<contextReader> Ꮡcr, slice<byte> b) {
    ref var cr = ref Ꮡcr.DerefOrNull();

    {
        var ctxErr = cr.ctx.Err(); if (ctxErr != default!) {
            return (0, ctxErr);
        }
    }
    var done = new channel<EmptyStruct>(0);
    // This goroutine may stay blocked after Read returns because the underlying
    // read is blocked.
    nint n = default!;
    ref var err = ref heap<error>(out var Ꮡerr);
    var bʗ1 = b;
    var doneʗ1 = done;
    goǃ(() => {
        (n, Ꮡerr.ValueSlot) = Ꮡcr.Value.r.Read(bʗ1);
        close(doneʗ1);
    });
    var selᴛ15 = cr.ctx.Done();
    var selᴛ16 = done;
    switch (select(ᐸꟷ(selᴛ15, ꓸꓸꓸ), ᐸꟷ(selᴛ16, ꓸꓸꓸ))) {
    case 0 when selᴛ15.ꟷᐳ(out _): {
        return (0, cr.ctx.Err());
    }
    case 1 when selᴛ16.ꟷᐳ(out _): {
        return (n, err);
    }}
    return default!;
}

} // end fuzz_package
