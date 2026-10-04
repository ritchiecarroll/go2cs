// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using context = context_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using race = global::go.@internal.race_package;
using io = io_package;
using os = os_package;
using signal = global::go.os.signal_package;
using reflect = reflect_package;
using strconv = strconv_package;
using testing = testing_package;
using time = time_package;
using global::go.@internal;
using global::go.os;
using static global::go.@internal.fuzz_package;

partial class fuzz_internal_test_package {

internal static ж<bool> benchmarkWorkerFlag = flag.Bool("benchmarkworker"u8, false, ""u8);

public static void TestMain(ж<testing.M> Ꮡm) {
    flag.Parse();
    if (benchmarkWorkerFlag.Value) {
        runBenchmarkWorker();
        return;
    }
    os.Exit(Ꮡm.Run());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object todo48504FixAndReEnableˢ = (@string)"TODO(48504): fix and re-enable"u8;

public static void BenchmarkWorkerFuzzOverhead(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        if (race.Enabled) {
            Ꮡb.Skip(todo48504FixAndReEnableˢ);
        }
        @string origEnv = os.Getenv(godebugˢ);
        defer(() => {
            os.Setenv(godebugˢ, origEnv);
        }, ref ᒐ);
        os.Setenv(godebugˢ, fmt.Sprintf("%s,fuzzseed=123"u8, origEnv));
        var ws = Ꮡ(new workerServer(
            fuzzFn: (CorpusEntry _) => (time.ΔSecond, default!),
            workerComm: new workerComm(memMu: new channel<ж<global::go.@internal.fuzz_package.sharedMem>>(1))
        ));
        var (mem, err) = sharedMemTempFile(workerSharedMemSize);
        if (err != default!) {
            Ꮡb.Fatalf("failed to create temporary shared memory file: %s"u8, err);
        }
        var memʗ1 = mem;
        defer(() => {
            {
                var errΔ1 = memʗ1.Close(); if (errΔ1 != default!) {
                    Ꮡb.Error(errΔ1);
                }
            }
        }, ref ᒐ);
        var initialVal = new any[]{new slice<byte>(32)}.slice();
        var encodedVals = marshalCorpusFile(initialVal.ꓸꓸꓸ);
        mem.setValue(encodedVals);
        (~ws).memMu.ᐸꟷ(mem);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            ws.Value.m = newMutator();
            mem.setValue(encodedVals);
            mem.header().Value.count = 0;
            ws.fuzz(context.Background(), new fuzzArgs(Limit: 1));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// BenchmarkWorkerPing acts as the coordinator and measures the time it takes
// a worker to respond to N pings. This is a rough measure of our RPC latency.
public static void BenchmarkWorkerPing(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (race.Enabled) {
        Ꮡb.Skip(todo48504FixAndReEnableˢ);
    }
    b.SetParallelism(1);
    var w = newWorkerForTest(new fuzz_internal_test_package.testing_BжTB(Ꮡb));
    for (nint i = 0; i < b.N; i++) {
        {
            var err = (~w).client.ping(context.Background()); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object workerDidNotMakeProgressˢ = (@string)"worker did not make progress"u8;

// BenchmarkWorkerFuzz acts as the coordinator and measures the time it takes
// a worker to mutate a given input and call a trivial fuzz function N times.
public static void BenchmarkWorkerFuzz(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (race.Enabled) {
        Ꮡb.Skip(todo48504FixAndReEnableˢ);
    }
    b.SetParallelism(1);
    var w = newWorkerForTest(new fuzz_internal_test_package.testing_BжTB(Ꮡb));
    var entry = new CorpusEntry(Values: new any[]{slice<byte>(default!)}.slice());
    entry.Data = marshalCorpusFile(entry.Values.ꓸꓸꓸ);
    for (var i = (int64)0; i < (int64)b.N; ) {
        var args = new fuzzArgs(
            Limit: (int64)b.N - i,
            Timeout: workerFuzzDuration
        );
        var (_, resp, _, err) = (~w).client.fuzz(context.Background(), entry, args);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        if (resp.Err != ""u8) {
            Ꮡb.Fatal(resp.Err);
        }
        if (resp.Count == 0) {
            Ꮡb.Fatal(workerDidNotMakeProgressˢ);
        }
        i += resp.Count;
    }
}

// newWorkerForTest creates and starts a worker process for testing or
// benchmarking. The worker process calls RunFuzzWorker, which responds to
// RPC messages until it's stopped. The process is stopped and cleaned up
// automatically when the test is done.
internal static ж<global::go.@internal.fuzz_package.worker> newWorkerForTest(testing.TB tb) {
    tb.Helper();
    var (c, err) = newCoordinator(new CoordinateFuzzingOpts(
        Types: new reflectꓸType[]{reflect.TypeOf(slice<byte>(default!))}.slice(),
        Log: io.Discard
    ));
    if (err != default!) {
        tb.Fatal(err);
    }
    @string dir = ""u8; // same as self
    @string binPath = os.Args[0]; // same as self
    var args = append(os.Args[1..], "-benchmarkworker"u8);
    var env = os.Environ(); // same as self
    (var w, err) = newWorker(c, dir, binPath, args, env);
    if (err != default!) {
        tb.Fatal(err);
    }
    var wʗ1 = w;
    tb.Cleanup(() => {
        {
            var errΔ1 = wʗ1.cleanup(); if (errΔ1 != default!) {
                tb.Error(errΔ1);
            }
        }
    });
    {
        var errΔ2 = w.startAndPing(context.Background()); if (errΔ2 != default!) {
            tb.Fatal(errΔ2);
        }
    }
    var wʗ2 = w;
    tb.Cleanup(() => {
        {
            var errΔ3 = wʗ2.stop(); if (errΔ3 != default!) {
                tb.Error(errΔ3);
            }
        }
    });
    return w;
}

internal static void runBenchmarkWorker() {
    GoFrame ᒐ = default;
    try {
        var (ctx, cancel) = signal.NotifyContext(context.Background(), os.Interrupt);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var fn = error (CorpusEntry _) => default!;
        {
            var err = RunFuzzWorker(ctx, fn); if (err != default! && !AreEqual(err, ctx.Err())) {
                throw panic(err);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string initialFailureForDeflakeˢ = "initial failure for deflake"u8;

public static void BenchmarkWorkerMinimize(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        if (race.Enabled) {
            Ꮡb.Skip(todo48504FixAndReEnableˢ);
        }
        var ws = Ꮡ(new workerServer(
            workerComm: new workerComm(memMu: new channel<ж<global::go.@internal.fuzz_package.sharedMem>>(1))
        ));
        ref var mem = ref heap<ж<global::go.@internal.fuzz_package.sharedMem>>(out var Ꮡmem);
        (mem, var err) = sharedMemTempFile(workerSharedMemSize);
        if (err != default!) {
            Ꮡb.Fatalf("failed to create temporary shared memory file: %s"u8, err);
        }
        defer(() => {
            {
                var errΔ1 = Ꮡmem.ValueSlot.Close(); if (errΔ1 != default!) {
                    Ꮡb.Error(errΔ1);
                }
            }
        }, ref ᒐ);
        (~ws).memMu.ᐸꟷ(mem);
        var bytes = new slice<byte>(1024);
        var ctx = context.Background();
        for (nint sz = 1; sz <= len(bytes); sz <<= (int)(1)) {
            nint szΔ1 = sz;
            var input = new any[]{bytes.slice(0, szΔ1)}.slice();
            var encodedVals = marshalCorpusFile(input.ꓸꓸꓸ);
            mem = ᐸꟷ((~ws).memMu);
            mem.setValue(encodedVals);
            (~ws).memMu.ᐸꟷ(mem);
            var ctxʗ1 = ctx;
            var wsʗ1 = ws;
            Ꮡb.Run(strconv.Itoa(szΔ1), (ж<testing.B> bΔ1) => {
                nint i = 0;
                wsʗ1.Value.fuzzFn = (CorpusEntry _) => {
                    if (i == 0) {
                        i++;
                        return (time.ΔSecond, errors.New(initialFailureForDeflakeˢ));
                    }
                    return (time.ΔSecond, default!);
                };
                for (nint iΔ1 = 0; iΔ1 < (~bΔ1).N; iΔ1++) {
                    bΔ1.SetBytes((int64)szΔ1);
                    wsʗ1.minimize(ctxʗ1, new minimizeArgs(nil));
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fuzz_internal_test_package
