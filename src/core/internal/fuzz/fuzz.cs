// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fuzz provides common fuzzing functionality for tests built with
// "go test" and for programs that use fuzzing functionality in the testing
// package.
global using CorpusEntry = go.@internal.fuzz_package.CorpusEntryᴛ1;

namespace go.@internal;

using bytes = bytes_package;
using context = context_package;
using sha256 = crypto.sha256_package;
using errors = errors_package;
using fmt = fmt_package;
using godebug = global::go.@internal.godebug_package;
using io = io_package;
using bits = global::go.math.bits_package;
using os = os_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;
using crypto;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.io;
using global::go.math;
using path;
using ꓸꓸꓸCorpusEntry = Span<CorpusEntry>;
using ꓸꓸꓸany = Span<any>;

partial class fuzz_package {

// CoordinateFuzzingOpts is a set of arguments for CoordinateFuzzing.
// The zero value is valid for each field unless specified otherwise.
[GoType] partial struct CoordinateFuzzingOpts {
    // Log is a writer for logging progress messages and warnings.
    // If nil, io.Discard will be used instead.
    public io.Writer Log;
    // Timeout is the amount of wall clock time to spend fuzzing after the corpus
    // has loaded. If zero, there will be no time limit.
    public time.Duration Timeout;
    // Limit is the number of random values to generate and test. If zero,
    // there will be no limit on the number of generated values.
    public int64 Limit;
    // MinimizeTimeout is the amount of wall clock time to spend minimizing
    // after discovering a crasher. If zero, there will be no time limit. If
    // MinimizeTimeout and MinimizeLimit are both zero, then minimization will
    // be disabled.
    public time.Duration MinimizeTimeout;
    // MinimizeLimit is the maximum number of calls to the fuzz function to be
    // made while minimizing after finding a crash. If zero, there will be no
    // limit. Calls to the fuzz function made when minimizing also count toward
    // Limit. If MinimizeTimeout and MinimizeLimit are both zero, then
    // minimization will be disabled.
    public int64 MinimizeLimit;
    // parallel is the number of worker processes to run in parallel. If zero,
    // CoordinateFuzzing will run GOMAXPROCS workers.
    public nint Parallel;
    // Seed is a list of seed values added by the fuzz target with testing.F.Add
    // and in testdata.
    public slice<CorpusEntry> Seed;
    // Types is the list of types which make up a corpus entry.
    // Types must be set and must match values in Seed.
    public slice<reflectꓸType> Types;
    // CorpusDir is a directory where files containing values that crash the
    // code being tested may be written. CorpusDir must be set.
    public @string CorpusDir;
    // CacheDir is a directory containing additional "interesting" values.
    // The fuzzer may derive new values from these, and may write new values here.
    public @string CacheDir;
}

// CoordinateFuzzing creates several worker processes and communicates with
// them to test random inputs that could trigger crashes and expose bugs.
// The worker processes run the same binary in the same directory with the
// same environment variables as the coordinator process. Workers also run
// with the same arguments as the coordinator, except with the -test.fuzzworker
// flag prepended to the argument list.
//
// If a crash occurs, the function will return an error containing information
// about the crash, which can be reported to the user.
[MethodImpl(MethodImplOptions.NoInlining)] public static error /*err*/ CoordinateFuzzing(context.Context ctx, CoordinateFuzzingOpts opts) {
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var err = ref Ꮡerr.ValueSlot;

        {
            var errΔ1 = ctx.Err(); if (errΔ1 != default!) {
                err = errΔ1; goto ᒐdone;
            }
        }
        if (opts.Log == default!) {
            opts.Log = io.Discard;
        }
        if (opts.Parallel == 0) {
            opts.Parallel = Δruntime.GOMAXPROCS(0);
        }
        if (opts.Limit > 0 && (int64)opts.Parallel > opts.Limit) {
            // Don't start more workers than we need.
            opts.Parallel = (nint)opts.Limit;
        }
        (var c, err) = newCoordinator(opts);
        if (err != default!) {
            goto ᒐdone;
        }
        if (opts.Timeout > 0) {
            Action cancel = default!;
            (ctx, cancel) = context.WithTimeout(ctx, opts.Timeout);
            var cancelʗ1 = cancel;
            defer(cancelʗ1, ref ᒐ);
        }
        // fuzzCtx is used to stop workers, for example, after finding a crasher.
        var (fuzzCtx, cancelWorkers) = context.WithCancel(ctx);
        var cancelWorkersʗ1 = cancelWorkers;
        defer(() => cancelWorkersʗ1(), ref ᒐ);
        ref var doneC = ref heap</*<-*/channel<EmptyStruct>>(out var ᏑdoneC);
        doneC = ctx.Done();
        // stop is called when a worker encounters a fatal error.
        ref var fuzzErr = ref heap<error>(out var ᏑfuzzErr);
        var stopping = false;
        var cʗ1 = c;
        var cancelWorkersʗ2 = cancelWorkers;
        var fuzzCtxʗ1 = fuzzCtx;
        [MethodImpl(MethodImplOptions.NoInlining)] void stop(error errΔ2) {
            if (shouldPrintDebugInfo()) {
                var (_, @file, line, ok) = Δruntime.Caller(1);
                if (ok){
                    cʗ1.debugLogf("stop called at %s:%d. stopping: %t"u8, @file, line, stopping);
                } else {
                    cʗ1.debugLogf("stop called at unknown. stopping: %t"u8, stopping);
                }
            }
            if (AreEqual(errΔ2, fuzzCtxʗ1.Err()) || isInterruptError(errΔ2)) {
                // Suppress cancellation errors and terminations due to SIGINT.
                // The messages are not helpful since either the user triggered the error
                // (with ^C) or another more helpful message will be printed (a crasher).
                errΔ2 = default!;
            }
            if (errΔ2 != default! && (ᏑfuzzErr.ValueSlot == default! || AreEqual(ᏑfuzzErr.ValueSlot, ctx.Err()))) {
                ᏑfuzzErr.ValueSlot = errΔ2;
            }
            if (stopping) {
                return;
            }
            stopping = true;
            cancelWorkersʗ2();
            ᏑdoneC.ValueSlot = default!;
        }
        // Ensure that any crash we find is written to the corpus, even if an error
        // or interruption occurs while minimizing it.
        var crashWritten = false;
        var cʗ2 = c;
        var optsʗ1 = opts;
        defer(() => {
            if ((~cʗ2).crashMinimizing == nil || crashWritten) {
                return;
            }
            var werr = writeToCorpus((~cʗ2).crashMinimizing.of(fuzzResult.Ꮡentry), optsʗ1.CorpusDir);
            if (werr != default!) {
                Ꮡerr.ValueSlot = fmt.Errorf("%w\n%v"u8, Ꮡerr.ValueSlot, werr);
                return;
            }
            if (Ꮡerr.ValueSlot == default!) {
                Ꮡerr.ValueSlot = new crashErrorжerror(Ꮡ(new crashError(
                    path: (~(~cʗ2).crashMinimizing).entry.Path,
                    err: errors.New((~(~cʗ2).crashMinimizing).crasherMsg)
                )));
            }
        }, ref ᒐ);
        // Start workers.
        // TODO(jayconrod): do we want to support fuzzing different binaries?
        @string dir = ""u8; // same as self
        @string binPath = os.Args[0];
        var args = appendꓸꓸꓸ(new @string[]{"-test.fuzzworker"u8}.slice(), os.Args[1..]);
        var env = os.Environ(); // same as self
        var errC = new channel<error>(0);
        var workers = new slice<ж<worker>>(opts.Parallel);
        foreach (var (i, _) in workers) {
            error errΔ3 = default!;
            (workers[i], errΔ3) = newWorker(c, dir, binPath, args, env);
            if (errΔ3 != default!) {
                err = errΔ3; goto ᒐdone;
            }
        }
        foreach (var (i, _) in workers) {
            var w = workers[i];
            var errCʗ1 = errC;
            var fuzzCtxʗ2 = fuzzCtx;
            var wʗ1 = w;
            goǃ(() => {
                var errΔ4 = wʗ1.coordinate(fuzzCtxʗ2);
                if (fuzzCtxʗ2.Err() != default! || isInterruptError(errΔ4)) {
                    errΔ4 = default!;
                }
                var cleanErr = wʗ1.cleanup();
                if (errΔ4 == default!) {
                    errΔ4 = cleanErr;
                }
                errCʗ1.ᐸꟷ(errΔ4);
            });
        }
        // Main event loop.
        // Do not return until all workers have terminated. We avoid a deadlock by
        // receiving messages from workers even after ctx is canceled.
        nint activeWorkers = len(workers);
        var statTicker = time.NewTicker((time.Duration)(3000000000L));
        var statTickerʗ1 = statTicker;
        defer(statTickerʗ1.Stop, ref ᒐ);
        var cʗ3 = c;
        defer(cʗ3.logStats, ref ᒐ);
        c.logStats();
        while (ᐧ) {
            // If there is an execution limit, and we've reached it, stop.
            if ((~c).opts.Limit > 0 && (~c).count >= (~c).opts.Limit) {
                stop(default!);
            }
            channel<fuzzInput> inputC = default!;
            var (input, ok) = c.peekInput();
            if (ok && (~c).crashMinimizing == nil && !stopping) {
                inputC = c.Value.inputC;
            }
            channel<fuzzMinimizeInput> minimizeC = default!;
            (var minimizeInput, ok) = c.peekMinimizeInput();
            if (ok && !stopping) {
                minimizeC = c.Value.minimizeC;
            }
            var selᴛ1 = doneC;
            var selᴛ2 = errC;
            var selᴛ3 = (~c).resultC;
            var selᴛ4 = inputC.ᐸꟷ(input, ꓸꓸꓸ);
            var selᴛ5 = minimizeC.ᐸꟷ(minimizeInput, ꓸꓸꓸ);
            var selᴛ6 = (~statTicker).C;
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ), selᴛ4, selᴛ5, ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                stop(ctx.Err());
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out var errΔ5): {
                stop(errΔ5);
                activeWorkers--;
                if (activeWorkers == 0) {
                    // Interrupted, canceled, or timed out.
                    // stop sets doneC to nil, so we don't busy wait here.
                    // A worker terminated, possibly after encountering a fatal error.
                    err = fuzzErr; goto ᒐdone;
                }
                break;
            }
            case 2 when selᴛ3.ꟷᐳ(out var resultᴛ1): {
                ref var result = ref heap(resultᴛ1, out var Ꮡresult);
                if (stopping) {
                    // Received response from worker.
                    break;
                }
                c.updateStats(result);
                if (result.crasherMsg != ""u8){
                    if (c.warmupRun() && result.entry.IsSeed) {
                        @string target = filepath.Base((~c).opts.CorpusDir);
                        fmt.Fprintf((~c).opts.Log, "failure while testing seed corpus entry: %s/%s\n"u8, target, testName(result.entry.Parent));
                        stop(errors.New(result.crasherMsg));
                        break;
                    }
                    if (c.canMinimize() && result.canMinimize){
                        if ((~c).crashMinimizing != nil) {
                            // This crash is not minimized, and another crash is being minimized.
                            // Ignore this one and wait for the other one to finish.
                            if (shouldPrintDebugInfo()) {
                                c.debugLogf("found unminimized crasher, skipping in favor of minimizable crasher"u8);
                            }
                            break;
                        }
                        // Found a crasher but haven't yet attempted to minimize it.
                        // Send it back to a worker for minimization. Disable inputC so
                        // other workers don't continue fuzzing.
                        c.Value.crashMinimizing = Ꮡresult;
                        fmt.Fprintf((~c).opts.Log, "fuzz: minimizing %d-byte failing input file\n"u8, len(result.entry.Data));
                        c.queueForMinimization(result, default!);
                    } else 
                    if (!crashWritten) {
                        // Found a crasher that's either minimized or not minimizable.
                        // Write to corpus and stop.
                        var errΔ6 = writeToCorpus(Ꮡresult.of(fuzzResult.Ꮡentry), opts.CorpusDir);
                        if (errΔ6 == default!) {
                            crashWritten = true;
                            errΔ6 = new crashErrorжerror(Ꮡ(new crashError(
                                path: result.entry.Path,
                                err: errors.New(result.crasherMsg)
                            )));
                        }
                        if (shouldPrintDebugInfo()) {
                            c.debugLogf(
                                "found crasher, id: %s, parent: %s, gen: %d, size: %d, exec time: %s"u8,
                                result.entry.Path,
                                result.entry.Parent,
                                result.entry.Generation,
                                len(result.entry.Data),
                                result.entryDuration);
                        }
                        stop(errΔ6);
                    }
                } else 
                if (result.coverageData != default!){
                    if (c.warmupRun()){
                        if (shouldPrintDebugInfo()) {
                            c.debugLogf(
                                "processed an initial input, id: %s, new bits: %d, size: %d, exec time: %s"u8,
                                result.entry.Parent,
                                countBits(diffCoverage((~c).coverageMask, result.coverageData)),
                                len(result.entry.Data),
                                result.entryDuration);
                        }
                        c.updateCoverage(result.coverageData);
                        c.Value.warmupInputLeft--;
                        if ((~c).warmupInputLeft == 0) {
                            fmt.Fprintf((~c).opts.Log, "fuzz: elapsed: %s, gathering baseline coverage: %d/%d completed, now fuzzing with %d workers\n"u8, c.elapsed(), (~c).warmupInputCount, (~c).warmupInputCount, (~c).opts.Parallel);
                            if (shouldPrintDebugInfo()) {
                                c.debugLogf(
                                    "finished processing input corpus, entries: %d, initial coverage bits: %d"u8,
                                    len((~c).corpus.entries),
                                    countBits((~c).coverageMask));
                            }
                        }
                    } else 
                    {
                        var keepCoverage = diffCoverage((~c).coverageMask, result.coverageData); if (keepCoverage != default!){
                            // Found a value that expanded coverage.
                            // It's not a crasher, but we may want to add it to the on-disk
                            // corpus and prioritize it for future fuzzing.
                            // TODO(jayconrod, katiehockman): Prioritize fuzzing these
                            // values which expanded coverage, perhaps based on the
                            // number of new edges that this result expanded.
                            // TODO(jayconrod, katiehockman): Don't write a value that's already
                            // in the corpus.
                            if (c.canMinimize() && result.canMinimize && (~c).crashMinimizing == nil){
                                // Send back to workers to find a smaller value that preserves
                                // at least one new coverage bit.
                                c.queueForMinimization(result, keepCoverage);
                            } else {
                                // Update the coordinator's coverage mask and save the value.
                                nint inputSize = len(result.entry.Data);
                                var (entryNew, errΔ7) = c.addCorpusEntries(true, result.entry);
                                if (errΔ7 != default!) {
                                    stop(errΔ7);
                                    break;
                                }
                                if (!entryNew) {
                                    if (shouldPrintDebugInfo()) {
                                        c.debugLogf(
                                            "ignoring duplicate input which increased coverage, id: %s"u8,
                                            result.entry.Path);
                                    }
                                    break;
                                }
                                c.updateCoverage(keepCoverage);
                                c.of(coordinator.ᏑinputQueue).enqueue(result.entry);
                                c.Value.interestingCount++;
                                if (shouldPrintDebugInfo()) {
                                    c.debugLogf(
                                        "new interesting input, id: %s, parent: %s, gen: %d, new bits: %d, total bits: %d, size: %d, exec time: %s"u8,
                                        result.entry.Path,
                                        result.entry.Parent,
                                        result.entry.Generation,
                                        countBits(keepCoverage),
                                        countBits((~c).coverageMask),
                                        inputSize,
                                        result.entryDuration);
                                }
                            }
                        } else {
                            if (shouldPrintDebugInfo()) {
                                c.debugLogf(
                                    "worker reported interesting input that doesn't expand coverage, id: %s, parent: %s, canMinimize: %t"u8,
                                    result.entry.Path,
                                    result.entry.Parent,
                                    result.canMinimize);
                            }
                        }
                    }
                } else 
                if (c.warmupRun()) {
                    // No error or coverage data was reported for this input during
                    // warmup, so continue processing results.
                    c.Value.warmupInputLeft--;
                    if ((~c).warmupInputLeft == 0) {
                        fmt.Fprintf((~c).opts.Log, "fuzz: elapsed: %s, testing seed corpus: %d/%d completed, now fuzzing with %d workers\n"u8, c.elapsed(), (~c).warmupInputCount, (~c).warmupInputCount, (~c).opts.Parallel);
                        if (shouldPrintDebugInfo()) {
                            c.debugLogf(
                                "finished testing-only phase, entries: %d"u8,
                                len((~c).corpus.entries));
                        }
                    }
                }
                break;
            }
            case 3: {
                c.sentInput(input);
                break;
            }
            case 4: {
                c.sentMinimizeInput(minimizeInput);
                break;
            }
            case 5 when selᴛ6.ꟷᐳ(out _): {
                c.logStats();
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡerr.ValueSlot;
}

// Sent the next input to a worker.
// Sent the next input for minimization to a worker.
// TODO(jayconrod,katiehockman): if a crasher can't be written to the corpus,
// write to the cache instead.

// crashError wraps a crasher written to the seed corpus. It saves the name
// of the file where the input causing the crasher was saved. The testing
// framework uses this to report a command to re-run that specific input.
[GoType] partial struct crashError {
    internal @string path;
    internal error err;
}

[GoRecv] internal static @string Error(this ref crashError e) {
    return e.err.Error();
}

[GoRecv] internal static error Unwrap(this ref crashError e) {
    return e.err;
}

[GoRecv] internal static @string CrashPath(this ref crashError e) {
    return e.path;
}

[GoType] partial struct corpus {
    internal slice<CorpusEntry> entries;
    [GoMapKeyDims(32)]
    internal map<array<byte>, bool> hashes;
}

// addCorpusEntries adds entries to the corpus, and optionally writes the entries
// to the cache directory. If an entry is already in the corpus it is skipped. If
// all of the entries are unique, addCorpusEntries returns true and a nil error,
// if at least one of the entries was a duplicate, it returns false and a nil error.
[GoRecv] internal static (bool, error) addCorpusEntries(this ref coordinator c, bool addToCache, params ꓸꓸꓸCorpusEntry entriesʗp) {
    var entries = entriesʗp.sslice();

    var noDupes = true;
    foreach (var (_, vᴛ1) in entries) {
        ref var e = ref heap(new CorpusEntry(), out var Ꮡe);
        e = vᴛ1;

        var (data, err) = corpusEntryData(e);
        if (err != default!) {
            return (false, err);
        }
        var h = sha256.Sum256(data);
        if (c.corpus.hashes[h]) {
            noDupes = false;
            continue;
        }
        if (addToCache) {
            {
                var errΔ1 = writeToCorpus(Ꮡe, c.opts.CacheDir); if (errΔ1 != default!) {
                    return (false, errΔ1);
                }
            }
            // For entries written to disk, we don't hold onto the bytes,
            // since the corpus would consume a significant amount of
            // memory.
            e.Data = default!;
        }
        c.corpus.hashes[h] = true;
        c.corpus.entries = append(c.corpus.entries, e);
    }
    return (noDupes, default!);
}

// CorpusEntry represents an individual input for fuzzing.
//
// We must use an equivalent type in the testing and testing/internal/testdeps
// packages, but testing can't import this package directly, and we don't want
// to export this type from testing. Instead, we use the same struct type and
// use a type alias (not a defined type) for convenience.
[GoType("dyn")] public partial struct CorpusEntryᴛ1 {
    public @string Parent;
    // Path is the path of the corpus file, if the entry was loaded from disk.
    // For other entries, including seed values provided by f.Add, Path is the
    // name of the test, e.g. seed#0 or its hash.
    public @string Path;
    // Data is the raw input data. Data should only be populated for seed
    // values. For on-disk corpus files, Data will be nil, as it will be loaded
    // from disk using Path.
    public slice<byte> Data;
    // Values is the unmarshaled values from a corpus file.
    public slice<any> Values;
    public nint Generation;
    // IsSeed indicates whether this entry is part of the seed corpus.
    public bool IsSeed;
}

// corpusEntryData returns the raw input bytes, either from the data struct
// field, or from disk.
internal static (slice<byte>, error) corpusEntryData(CorpusEntry ce) {
    if (ce.Data != default!) {
        return (ce.Data, default!);
    }
    return os.ReadFile(ce.Path);
}

[GoType] partial struct fuzzInput {
    // entry is the value to test initially. The worker will randomly mutate
    // values from this starting point.
    internal CorpusEntry entry;
    // timeout is the time to spend fuzzing variations of this input,
    // not including starting or cleaning up.
    internal time.Duration timeout;
    // limit is the maximum number of calls to the fuzz function the worker may
    // make. The worker may make fewer calls, for example, if it finds an
    // error early. If limit is zero, there is no limit on calls to the
    // fuzz function.
    internal int64 limit;
    // warmup indicates whether this is a warmup input before fuzzing begins. If
    // true, the input should not be fuzzed.
    internal bool warmup;
    // coverageData reflects the coordinator's current coverageMask.
    internal slice<byte> coverageData;
}

[GoType] partial struct fuzzResult {
    // entry is an interesting value or a crasher.
    internal CorpusEntry entry;
    // crasherMsg is an error message from a crash. It's "" if no crash was found.
    internal @string crasherMsg;
    // canMinimize is true if the worker should attempt to minimize this result.
    // It may be false because an attempt has already been made.
    internal bool canMinimize;
    // coverageData is set if the worker found new coverage.
    internal slice<byte> coverageData;
    // limit is the number of values the coordinator asked the worker
    // to test. 0 if there was no limit.
    internal int64 limit;
    // count is the number of values the worker actually tested.
    internal int64 count;
    // totalDuration is the time the worker spent testing inputs.
    internal time.Duration totalDuration;
    // entryDuration is the time the worker spent execution an interesting result
    internal time.Duration entryDuration;
}

[GoType] partial struct fuzzMinimizeInput {
    // entry is an interesting value or crasher to minimize.
    internal CorpusEntry entry;
    // crasherMsg is an error message from a crash. It's "" if no crash was found.
    // If set, the worker will attempt to find a smaller input that also produces
    // an error, though not necessarily the same error.
    internal @string crasherMsg;
    // limit is the maximum number of calls to the fuzz function the worker may
    // make. The worker may make fewer calls, for example, if it can't reproduce
    // an error. If limit is zero, there is no limit on calls to the fuzz function.
    internal int64 limit;
    // timeout is the time to spend minimizing this input.
    // A zero timeout means no limit.
    internal time.Duration timeout;
    // keepCoverage is a set of coverage bits that entry found that were not in
    // the coordinator's combined set. When minimizing, the worker should find an
    // input that preserves at least one of these bits. keepCoverage is nil for
    // crashing inputs.
    internal slice<byte> keepCoverage;
}

// coordinator holds channels that workers can use to communicate with
// the coordinator.
[GoType] partial struct coordinator {
    internal CoordinateFuzzingOpts opts;
    // startTime is the time we started the workers after loading the corpus.
    // Used for logging.
    internal time.Time startTime;
    // inputC is sent values to fuzz by the coordinator. Any worker may receive
    // values from this channel. Workers send results to resultC.
    internal channel<fuzzInput> inputC;
    // minimizeC is sent values to minimize by the coordinator. Any worker may
    // receive values from this channel. Workers send results to resultC.
    internal channel<fuzzMinimizeInput> minimizeC;
    // resultC is sent results of fuzzing by workers. The coordinator
    // receives these. Multiple types of messages are allowed.
    internal channel<fuzzResult> resultC;
    // count is the number of values fuzzed so far.
    internal int64 count;
    // countLastLog is the number of values fuzzed when the output was last
    // logged.
    internal int64 countLastLog;
    // timeLastLog is the time at which the output was last logged.
    internal time.Time timeLastLog;
    // interestingCount is the number of unique interesting values which have
    // been found this execution.
    internal nint interestingCount;
    // warmupInputCount is the count of all entries in the corpus which will
    // need to be received from workers to run once during warmup, but not fuzz.
    // This could be for coverage data, or only for the purposes of verifying
    // that the seed corpus doesn't have any crashers. See warmupRun.
    internal nint warmupInputCount;
    // warmupInputLeft is the number of entries in the corpus which still need
    // to be received from workers to run once during warmup, but not fuzz.
    // See warmupInputLeft.
    internal nint warmupInputLeft;
    // duration is the time spent fuzzing inside workers, not counting time
    // starting up or tearing down.
    internal time.Duration duration;
    // countWaiting is the number of fuzzing executions the coordinator is
    // waiting on workers to complete.
    internal int64 countWaiting;
    // corpus is a set of interesting values, including the seed corpus and
    // generated values that workers reported as interesting.
    internal corpus corpus;
    // minimizationAllowed is true if one or more of the types of fuzz
    // function's parameters can be minimized.
    internal bool minimizationAllowed;
    // inputQueue is a queue of inputs that workers should try fuzzing. This is
    // initially populated from the seed corpus and cached inputs. More inputs
    // may be added as new coverage is discovered.
    internal queue inputQueue;
    // minimizeQueue is a queue of inputs that caused errors or exposed new
    // coverage. Workers should attempt to find smaller inputs that do the
    // same thing.
    internal queue minimizeQueue;
    // crashMinimizing is the crash that is currently being minimized.
    internal ж<fuzzResult> crashMinimizing;
    // coverageMask aggregates coverage that was found for all inputs in the
    // corpus. Each byte represents a single basic execution block. Each set bit
    // within the byte indicates that an input has triggered that block at least
    // 1 << n times, where n is the position of the bit in the byte. For example, a
    // value of 12 indicates that separate inputs have triggered this block
    // between 4-7 times and 8-15 times.
    internal slice<byte> coverageMask;
}

internal static (ж<coordinator>, error) newCoordinator(CoordinateFuzzingOpts opts) {
    // Make sure all the seed corpus has marshaled data.
    foreach (var (i, _) in opts.Seed) {
        if (opts.Seed[i].Data == default! && opts.Seed[i].Values != default!) {
            opts.Seed[i].Data = marshalCorpusFile(opts.Seed[i].Values.ꓸꓸꓸ);
        }
    }
    var c = Ꮡ(new coordinator(
        opts: opts,
        startTime: time.Now(),
        inputC: new channel<fuzzInput>(0),
        minimizeC: new channel<fuzzMinimizeInput>(0),
        resultC: new channel<fuzzResult>(0),
        timeLastLog: time.Now(),
        corpus: new corpus(hashes: new map<array<byte>, bool>())
    ));
    {
        var err = c.readCache(); if (err != default!) {
            return (default!, err);
        }
    }
    if (opts.MinimizeLimit > 0 || opts.MinimizeTimeout > 0) {
        foreach (var (_, t) in opts.Types) {
            if (isMinimizable(t)) {
                c.Value.minimizationAllowed = true;
                break;
            }
        }
    }
    nint covSize = len(coverage());
    if (covSize == 0){
        fmt.Fprintf((~c).opts.Log, "warning: the test binary was not built with coverage instrumentation, so fuzzing will run without coverage guidance and may be inefficient\n"u8);
        // Even though a coverage-only run won't occur, we should still run all
        // of the seed corpus to make sure there are no existing failures before
        // we start fuzzing.
        c.Value.warmupInputCount = len((~c).opts.Seed);
        foreach (var (_, e) in (~c).opts.Seed) {
            c.of(coordinator.ᏑinputQueue).enqueue(e);
        }
    } else {
        c.Value.warmupInputCount = len((~c).corpus.entries);
        foreach (var (_, e) in (~c).corpus.entries) {
            c.of(coordinator.ᏑinputQueue).enqueue(e);
        }
        // Set c.coverageMask to a clean []byte full of zeros.
        c.Value.coverageMask = new slice<byte>(covSize);
    }
    c.Value.warmupInputLeft = c.Value.warmupInputCount;
    if (len((~c).corpus.entries) == 0) {
        fmt.Fprintf((~c).opts.Log, "warning: starting with empty corpus\n"u8);
        slice<any> vals = default!;
        foreach (var (_, t) in opts.Types) {
            vals = append(vals, zeroValue(t));
        }
        var data = marshalCorpusFile(vals.ꓸꓸꓸ);
        var h = sha256.Sum256(data);
        @string name = fmt.Sprintf("%x"u8, h[..4]);
        c.addCorpusEntries(false, new CorpusEntry(Path: name, Data: data));
    }
    return (c, default!);
}

[GoRecv] internal static void updateStats(this ref coordinator c, fuzzResult result) {
    c.count += result.count;
    c.countWaiting -= result.limit;
    c.duration += result.totalDuration;
}

[GoRecv] internal static void logStats(this ref coordinator c) {
    var now = time.Now();
    if (c.warmupRun()){
        nint runSoFar = c.warmupInputCount - c.warmupInputLeft;
        if (coverageEnabled){
            fmt.Fprintf(c.opts.Log, "fuzz: elapsed: %s, gathering baseline coverage: %d/%d completed\n"u8, c.elapsed(), runSoFar, c.warmupInputCount);
        } else {
            fmt.Fprintf(c.opts.Log, "fuzz: elapsed: %s, testing seed corpus: %d/%d completed\n"u8, c.elapsed(), runSoFar, c.warmupInputCount);
        }
    } else 
    if (c.crashMinimizing != nil){
        fmt.Fprintf(c.opts.Log, "fuzz: elapsed: %s, minimizing\n"u8, c.elapsed());
    } else {
        var rate = (float64)(c.count - c.countLastLog) / now.Sub(c.timeLastLog).Seconds();
        if (coverageEnabled){
            nint total = c.warmupInputCount + c.interestingCount;
            fmt.Fprintf(c.opts.Log, "fuzz: elapsed: %s, execs: %d (%.0f/sec), new interesting: %d (total: %d)\n"u8, c.elapsed(), c.count, rate, c.interestingCount, total);
        } else {
            fmt.Fprintf(c.opts.Log, "fuzz: elapsed: %s, execs: %d (%.0f/sec)\n"u8, c.elapsed(), c.count, rate);
        }
    }
    c.countLastLog = c.count;
    c.timeLastLog = now;
}

// peekInput returns the next value that should be sent to workers.
// If the number of executions is limited, the returned value includes
// a limit for one worker. If there are no executions left, peekInput returns
// a zero value and false.
//
// peekInput doesn't actually remove the input from the queue. The caller
// must call sentInput after sending the input.
//
// If the input queue is empty and the coverage/testing-only run has completed,
// queue refills it from the corpus.
[GoRecv] internal static (fuzzInput, bool) peekInput(this ref coordinator c) {
    if (c.opts.Limit > 0 && c.count + c.countWaiting >= c.opts.Limit) {
        // Already making the maximum number of calls to the fuzz function.
        // Don't send more inputs right now.
        return (new fuzzInput(nil), false);
    }
    if (c.inputQueue.len == 0) {
        if (c.warmupRun()) {
            // Wait for coverage/testing-only run to finish before sending more
            // inputs.
            return (new fuzzInput(nil), false);
        }
        c.refillInputQueue();
    }
    var (entry, ok) = c.inputQueue.peek();
    if (!ok) {
        throw panic("input queue empty after refill");
    }
    var input = new fuzzInput(
        entry: entry._<CorpusEntry>(),
        timeout: workerFuzzDuration,
        warmup: c.warmupRun()
    );
    if (c.coverageMask != default!) {
        input.coverageData = bytes.Clone(c.coverageMask);
    }
    if (input.warmup) {
        // No fuzzing will occur, but it should count toward the limit set by
        // -fuzztime.
        input.limit = 1;
        return (input, true);
    }
    if (c.opts.Limit > 0) {
        input.limit = quo(c.opts.Limit, (int64)c.opts.Parallel);
        if (rem(c.opts.Limit, (int64)c.opts.Parallel) > 0) {
            input.limit++;
        }
        var remaining = c.opts.Limit - c.count - c.countWaiting;
        if (input.limit > remaining) {
            input.limit = remaining;
        }
    }
    return (input, true);
}

// sentInput updates internal counters after an input is sent to c.inputC.
[GoRecv] internal static void sentInput(this ref coordinator c, fuzzInput input) {
    c.inputQueue.dequeue();
    c.countWaiting += input.limit;
}

// refillInputQueue refills the input queue from the corpus after it becomes
// empty.
[GoRecv] internal static void refillInputQueue(this ref coordinator c) {
    foreach (var (_, e) in c.corpus.entries) {
        c.inputQueue.enqueue(e);
    }
}

// queueForMinimization creates a fuzzMinimizeInput from result and adds it
// to the minimization queue to be sent to workers.
[GoRecv] internal static void queueForMinimization(this ref coordinator c, fuzzResult result, slice<byte> keepCoverage) {
    if (shouldPrintDebugInfo()) {
        c.debugLogf(
            "queueing input for minimization, id: %s, parent: %s, keepCoverage: %t, crasher: %t"u8,
            result.entry.Path,
            result.entry.Parent,
            keepCoverage != default!,
            result.crasherMsg != ""u8);
    }
    if (result.crasherMsg != ""u8) {
        c.minimizeQueue.clear();
    }
    var input = new fuzzMinimizeInput(
        entry: result.entry,
        crasherMsg: result.crasherMsg,
        keepCoverage: keepCoverage
    );
    c.minimizeQueue.enqueue(input);
}

// peekMinimizeInput returns the next input that should be sent to workers for
// minimization.
[GoRecv] internal static (fuzzMinimizeInput, bool) peekMinimizeInput(this ref coordinator c) {
    if (!c.canMinimize()) {
        // Already making the maximum number of calls to the fuzz function.
        // Don't send more inputs right now.
        return (new fuzzMinimizeInput(nil), false);
    }
    var (v, ok) = c.minimizeQueue.peek();
    if (!ok) {
        return (new fuzzMinimizeInput(nil), false);
    }
    var input = v._<fuzzMinimizeInput>();
    if (c.opts.MinimizeTimeout > 0) {
        input.timeout = c.opts.MinimizeTimeout;
    }
    if (c.opts.MinimizeLimit > 0){
        input.limit = c.opts.MinimizeLimit;
    } else 
    if (c.opts.Limit > 0) {
        if (input.crasherMsg != ""u8){
            input.limit = c.opts.Limit;
        } else {
            input.limit = quo(c.opts.Limit, (int64)c.opts.Parallel);
            if (rem(c.opts.Limit, (int64)c.opts.Parallel) > 0) {
                input.limit++;
            }
        }
    }
    if (c.opts.Limit > 0) {
        var remaining = c.opts.Limit - c.count - c.countWaiting;
        if (input.limit > remaining) {
            input.limit = remaining;
        }
    }
    return (input, true);
}

// sentMinimizeInput removes an input from the minimization queue after it's
// sent to minimizeC.
[GoRecv] internal static void sentMinimizeInput(this ref coordinator c, fuzzMinimizeInput input) {
    c.minimizeQueue.dequeue();
    c.countWaiting += input.limit;
}

// warmupRun returns true while the coordinator is running inputs without
// mutating them as a warmup before fuzzing. This could be to gather baseline
// coverage data for entries in the corpus, or to test all of the seed corpus
// for errors before fuzzing begins.
//
// The coordinator doesn't store coverage data in the cache with each input
// because that data would be invalid when counter offsets in the test binary
// change.
//
// When gathering coverage, the coordinator sends each entry to a worker to
// gather coverage for that entry only, without fuzzing or minimizing. This
// phase ends when all workers have finished, and the coordinator has a combined
// coverage map.
[GoRecv] internal static bool warmupRun(this ref coordinator c) {
    return c.warmupInputLeft > 0;
}

// updateCoverage sets bits in c.coverageMask that are set in newCoverage.
// updateCoverage returns the number of newly set bits. See the comment on
// coverageMask for the format.
[GoRecv] internal static nint updateCoverage(this ref coordinator c, slice<byte> newCoverage) {
    if (len(newCoverage) != len(c.coverageMask)) {
        throw panic(fmt.Sprintf("number of coverage counters changed at runtime: %d, expected %d"u8, len(newCoverage), len(c.coverageMask)));
    }
    nint newBitCount = 0;
    foreach (var (i, _) in newCoverage) {
        var diff = (byte)(newCoverage[i] & ~c.coverageMask[i]);
        newBitCount += bits.OnesCount8(diff);
        c.coverageMask[i] |= (byte)(newCoverage[i]);
    }
    return newBitCount;
}

// canMinimize returns whether the coordinator should attempt to find smaller
// inputs that reproduce a crash or new coverage.
[GoRecv] internal static bool canMinimize(this ref coordinator c) {
    return c.minimizationAllowed && (c.opts.Limit == 0 || c.count + c.countWaiting < c.opts.Limit);
}

[GoRecv] internal static time.Duration elapsed(this ref coordinator c) {
    return time.Since(c.startTime).Round(1 * time.ΔSecond);
}

// readCache creates a combined corpus from seed values and values in the cache
// (in GOCACHE/fuzz).
//
// TODO(fuzzing): need a mechanism that can remove values that
// aren't useful anymore, for example, because they have the wrong type.
[GoRecv] internal static error readCache(this ref coordinator c) {
    {
        var (_, errΔ1) = c.addCorpusEntries(false, c.opts.Seed.ꓸꓸꓸ); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    var (entries, err) = ReadCorpus(c.opts.CacheDir, c.opts.Types);
    if (err != default!) {
        {
            var (_, ok) = err._<ж<MalformedCorpusError>>(ᐧ); if (!ok) {
                // It's okay if some files in the cache directory are malformed and
                // are not included in the corpus, but fail if it's an I/O error.
                return err;
            }
        }
    }
    // TODO(jayconrod,katiehockman): consider printing some kind of warning
    // indicating the number of files which were skipped because they are
    // malformed.
    {
        var (_, errΔ2) = c.addCorpusEntries(false, entries.ꓸꓸꓸ); if (errΔ2 != default!) {
            return errΔ2;
        }
    }
    return default!;
}

// MalformedCorpusError is an error found while reading the corpus from the
// filesystem. All of the errors are stored in the errs list. The testing
// framework uses this to report malformed files in testdata.
[GoType] partial struct MalformedCorpusError {
    internal slice<error> errs;
}

[GoRecv] public static @string Error(this ref MalformedCorpusError e) {
    slice<@string> msgs = default!;
    foreach (var (_, s) in e.errs) {
        msgs = append(msgs, s.Error());
    }
    return strings.Join(msgs, "\n"u8);
}

// ReadCorpus reads the corpus from the provided dir. The returned corpus
// entries are guaranteed to match the given types. Any malformed files will
// be saved in a MalformedCorpusError and returned, along with the most recent
// error.
public static (slice<CorpusEntry>, error) ReadCorpus(@string dir, slice<reflectꓸType> types) {
    var (files, err) = os.ReadDir(dir);
    if (os.IsNotExist(err)){
        return (default!, default!); // No corpus to read
    } else 
    if (err != default!) {
        return (default!, fmt.Errorf("reading seed corpus from testdata: %v"u8, err));
    }
    slice<CorpusEntry> corpus = default!;
    slice<error> errs = default!;
    foreach (var (_, @file) in files) {
        // TODO(jayconrod,katiehockman): determine when a file is a fuzzing input
        // based on its name. We should only read files created by writeToCorpus.
        // If we read ALL files, we won't be able to change the file format by
        // changing the extension. We also won't be able to add files like
        // README.txt explaining why the directory exists.
        if (@file.IsDir()) {
            continue;
        }
        @string filename = filepath.Join(dir, @file.Name());
        var (data, errΔ1) = os.ReadFile(filename);
        if (errΔ1 != default!) {
            return (default!, fmt.Errorf("failed to read corpus file: %v"u8, errΔ1));
        }
        slice<any> vals = default!;
        (vals, errΔ1) = readCorpusData(data, types);
        if (errΔ1 != default!) {
            errs = append(errs, fmt.Errorf("%q: %v"u8, filename, errΔ1));
            continue;
        }
        corpus = append(corpus, new CorpusEntry(Path: filename, Values: vals));
    }
    if (len(errs) > 0) {
        return (corpus, new MalformedCorpusErrorжerror(Ꮡ(new MalformedCorpusError(errs: errs))));
    }
    return (corpus, default!);
}

internal static (slice<any>, error) readCorpusData(slice<byte> data, slice<reflectꓸType> types) {
    var (vals, err) = unmarshalCorpusFile(data);
    if (err != default!) {
        return (default!, fmt.Errorf("unmarshal: %v"u8, err));
    }
    {
        err = CheckCorpus(vals, types); if (err != default!) {
            return (default!, err);
        }
    }
    return (vals, default!);
}

// CheckCorpus verifies that the types in vals match the expected types
// provided.
public static error CheckCorpus(slice<any> vals, slice<reflectꓸType> types) {
    if (len(vals) != len(types)) {
        return fmt.Errorf("wrong number of values in corpus entry: %d, want %d"u8, len(vals), len(types));
    }
    var valsT = new slice<reflectꓸType>(len(vals));
    foreach (var (valsI, v) in vals) {
        valsT[valsI] = reflect.TypeOf(v);
    }
    foreach (var (i, _) in types) {
        if (!AreEqual(valsT[i], types[i])) {
            return fmt.Errorf("mismatched types in corpus entry: %v, want %v"u8, valsT, types);
        }
    }
    return default!;
}

// writeToCorpus atomically writes the given bytes to a new file in testdata. If
// the directory does not exist, it will create one. If the file already exists,
// writeToCorpus will not rewrite it. writeToCorpus sets entry.Path to the new
// file that was just written or an error if it failed.
internal static error /*err*/ writeToCorpus(ж<CorpusEntry> Ꮡentry, @string dir) {
    ref var entry = ref Ꮡentry.DerefOrNull();

    @string sum = fmt.Sprintf("%x"u8, sha256.Sum256(entry.Data))[..16];
    entry.Path = filepath.Join(dir, sum);
    {
        var errΔ1 = os.MkdirAll(dir, 511); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    {
        var errΔ2 = os.WriteFile(entry.Path, entry.Data, 438); if (errΔ2 != default!) {
            os.Remove(entry.Path); // remove partially written file
            return errΔ2;
        }
    }
    return default!;
}

internal static @string testName(@string path) {
    return filepath.Base(path);
}

internal static any zeroValue(reflectꓸType t) {
    foreach (var (_, v) in zeroVals) {
        if (AreEqual(reflect.TypeOf(v), t)) {
            return v;
        }
    }
    throw panic(fmt.Sprintf("unsupported type: %v"u8, t));
}

internal static slice<any> zeroVals = new any[]{
    slice<byte>(""u8),
    ((@string)""u8),
    false,
    (byte)0,
    (rune)0,
    (float32)0F,
    (float64)0D,
    (nint)0,
    (int8)0,
    (int16)0,
    (int32)0,
    (int64)0,
    (nuint)0,
    (uint8)0,
    (uint16)0,
    (uint32)0,
    (uint64)0
}.slice();

internal static bool debugInfo = godebug.New("#fuzzdebug"u8).Value() == "1"u8;

internal static bool shouldPrintDebugInfo() {
    return debugInfo;
}

[GoRecv] internal static void debugLogf(this ref coordinator c, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    @string t = time.Now().Format("2006-01-02 15:04:05.999999999"u8);
    fmt.Fprintf(c.opts.Log, t + " DEBUG "u8 + format + "\n"u8, args.ꓸꓸꓸ);
}

} // end fuzz_package
