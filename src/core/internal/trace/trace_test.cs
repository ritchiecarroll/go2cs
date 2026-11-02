// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using Δtrace = go.@internal.trace_package;
using testtrace = go.@internal.trace.testtrace_package;
using version = go.@internal.trace.version_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using context = context_package;
using exec = go.os.exec_package;
using go.@internal;
using go.@internal.trace;
using go.os;
using iter = iter_package;
using path;
using static go.@internal.trace_internal_test_package;

partial class trace_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string annotationsGoˢ => ref annotationsGoˢᵇ;
private static readonly @string annotationsGoˢᵇ = "annotations.go"u8;

internal partial struct TestTraceAnnotations_evDesc /*dyn*/ {
    internal Δtrace.EventKind kind;
    internal Δtrace.TaskID task;
    internal slice<@string> args;
}

public static void TestTraceAnnotations(ж<testing.T> t) {
    testTraceProg(t, annotationsGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _Δp2, @string _Δp3) => {
        var want = slice<TestTraceAnnotations_evDesc>([
            new(Δtrace.EventTaskBegin, (Δtrace.TaskID)1, slice<@string>(["task0"])),
            new(Δtrace.EventRegionBegin, (Δtrace.TaskID)1, slice<@string>(["region0"])),
            new(Δtrace.EventRegionBegin, (Δtrace.TaskID)1, slice<@string>(["region1"])),
            new(Δtrace.EventLog, (Δtrace.TaskID)1, slice<@string>(["key0", "0123456789abcdef"])),
            new(Δtrace.EventRegionEnd, (Δtrace.TaskID)1, slice<@string>(["region1"])),
            new(Δtrace.EventRegionEnd, (Δtrace.TaskID)1, slice<@string>(["region0"])),
            new(Δtrace.EventTaskEnd, (Δtrace.TaskID)1, slice<@string>(["task0"])),
            //  Currently, pre-existing region is not recorded to avoid allocations.
            new(Δtrace.EventRegionBegin, Δtrace.BackgroundTask, slice<@string>(["post-existing region"]))
        ]);
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)));
        if (err != nil) {
            tΔ1.Error(err);
        }
        for (;;) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            foreach (var (i, wantEv) in want) {
                if (wantEv.kind != ev.Kind()) {
                    continue;
                }
                var match = false;
                switch (ev.Kind()) {
                case var ᴛ when ᴛ == Δtrace.EventTaskBegin || ᴛ == Δtrace.EventTaskEnd: {
                    var task = ev.Task();
                    match = task.ID == wantEv.task && task.Type == wantEv.args[0];
                    break;
                }
                case var ᴛ when ᴛ == Δtrace.EventRegionBegin || ᴛ == Δtrace.EventRegionEnd: {
                    var reg = ev.Region();
                    match = reg.Task == wantEv.task && reg.Type == wantEv.args[0];
                    break;
                }
                case var ᴛ when ᴛ == Δtrace.EventLog: {
                    var log = ev.Log();
                    match = log.Task == wantEv.task && log.Category == wantEv.args[0] && log.Message == wantEv.args[1];
                    break;
                }}
                if (match) {
                    want[i] = want[len(want) - 1];
                    want = want.slice(0, len(want) - 1);
                    break;
                }
            }
        }
        if (len(want) != 0) {
            foreach (var (_, ev) in want) {
                tΔ1.Errorf("no match for %s TaskID=%d Args=%#v", ev.kind, ev.task, ev.args);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string annotationsStressGoˢ => ref annotationsStressGoˢᵇ;
private static readonly @string annotationsStressGoˢᵇ = "annotations-stress.go"u8;

public static void TestTraceAnnotationsStress(ж<testing.T> t) {
    testTraceProg(t, annotationsStressGoˢ, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cgoCallbackGoˢ => ref cgoCallbackGoˢᵇ;
private static readonly @string cgoCallbackGoˢᵇ = "cgo-callback.go"u8;

public static void TestTraceCgoCallback(ж<testing.T> t) {
    testenv.MustHaveCGO(new trace_test_package.testing_TжTB(t));

    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8: {
        t.Skipf("cgo callback test requires pthreads and is not supported on %s", Δruntime.GOOS);
        break;
    }}
    testTraceProg(t, cgoCallbackGoˢ, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cpuProfileGoˢ => ref cpuProfileGoˢᵇ;
private static readonly @string cpuProfileGoˢᵇ = "cpu-profile.go"u8;
internal static readonly object cpuProfileDidNotIncludeˢ = (@string)"CPU profile did not include any samples while tracing was active"u8;

public static void TestTraceCPUProfile(ж<testing.T> t) {
    testTraceProg(t, cpuProfileGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> stderr, @string _Δp3) => {
        // Parse stderr which has a CPU profile summary, if everything went well.
        // (If it didn't, we shouldn't even make it here.)
        var scanner = bufio.NewScanner(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(stderr)));
        nint pprofSamples = 0;
        var pprofStacks = new map<@string, nint>();
        while (scanner.Scan()) {
            ref var stack = ref heap(new @string(), out var Ꮡstack);
            ref var samples = ref heap(new nint(), out var Ꮡsamples);
            var (_, errΔ1) = fmt.Sscanf(scanner.Text(), "%s\t%d"u8, Ꮡstack, Ꮡsamples);
            if (errΔ1 != nil) {
                tΔ1.Fatalf("failed to parse CPU profile summary in stderr: %s\n\tfull:\n%s", scanner.Text(), stderr);
            }
            pprofStacks[stack] = samples;
            pprofSamples += samples;
        }
        {
            var errΔ2 = scanner.Err(); if (errΔ2 != nil) {
                tΔ1.Fatalf("failed to parse CPU profile summary in stderr: %v", errΔ2);
            }
        }
        if (pprofSamples == 0) {
            tΔ1.Skip(cpuProfileDidNotIncludeˢ);
        }

        // Examine the execution tracer's view of the CPU profile samples. Filter it
        // to only include samples from the single test goroutine. Use the goroutine
        // ID that was recorded in the events: that should reflect getg().m.curg,
        // same as the profiler's labels (even when the M is using its g0 stack).
        nint totalTraceSamples = 0;
        nint traceSamples = 0;
        var traceStacks = new map<@string, nint>();
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)));
        if (err != nil) {
            tΔ1.Error(err);
        }
        ж<traceꓸEvent> hogRegion = default;
        bool hogRegionClosed = default;
        for (;;) {
            ref var ev = ref heap<traceꓸEvent>(out var Ꮡev);
            (ev, var errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            if (ev.Kind() == Δtrace.EventRegionBegin && ev.Region().Type == "cpuHogger"u8) {
                hogRegion = Ꮡev;
            }
            if (ev.Kind() == Δtrace.EventStackSample) {
                totalTraceSamples++;
                if (hogRegion != nil && ev.Goroutine() == (~hogRegion).Goroutine()) {
                    traceSamples++;
                    slice<@string> fns = default;
                    foreach (var frame in range<Δtrace.StackFrame>(ev.Stack().Frames().Invoke)) {
                        if (frame.Func != "runtime.goexit"u8) {
                            fns = append(fns, fmt.Sprintf("%s:%d"u8, frame.Func, frame.Line));
                        }
                    }
                    @string stack = strings.Join(fns, "|");
                    traceStacks[stack]++;
                }
            }
            if (ev.Kind() == Δtrace.EventRegionEnd && ev.Region().Type == "cpuHogger"u8) {
                hogRegionClosed = true;
            }
        }
        if (hogRegion == nil) {
            tΔ1.Fatalf("execution trace did not identify cpuHogger goroutine");
        } else if (!hogRegionClosed) {
            tΔ1.Fatalf("execution trace did not close cpuHogger region");
        }

        // The execution trace may drop CPU profile samples if the profiling buffer
        // overflows. Based on the size of profBufWordCount, that takes a bit over
        // 1900 CPU samples or 19 thread-seconds at a 100 Hz sample rate. If we've
        // hit that case, then we definitely have at least one full buffer's worth
        // of CPU samples, so we'll call that success.
        var overflowed = totalTraceSamples >= 1900;
        if (traceSamples < pprofSamples) {
            tΔ1.Logf("execution trace did not include all CPU profile samples; %d in profile, %d in trace", pprofSamples, traceSamples);
            if (!overflowed) {
                tΔ1.Fail();
            }
        }

        foreach (var (stack, traceSamplesΔ1) in traceStacks) {
            nint pprofSamplesΔ1 = pprofStacks[stack];
            delete(pprofStacks, stack);
            if (traceSamplesΔ1 < pprofSamplesΔ1) {
                tΔ1.Logf("execution trace did not include all CPU profile samples for stack %q; %d in profile, %d in trace",
                    stack, pprofSamplesΔ1, traceSamplesΔ1);
                if (!overflowed) {
                    tΔ1.Fail();
                }
            }
        }
        foreach (var (stack, pprofSamplesΔ2) in pprofStacks) {
            tΔ1.Logf("CPU profile included %d samples at stack %q not present in execution trace", pprofSamplesΔ2, stack);
            if (!overflowed) {
                tΔ1.Fail();
            }
        }

        if (tΔ1.Failed()) {
            tΔ1.Logf("execution trace CPU samples:");
            foreach (var (stack, samples) in traceStacks) {
                tΔ1.Logf("%d: %q", samples, stack);
            }
            tΔ1.Logf("CPU profile:\n%s", stderr);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string futileWakeupGoˢ => ref futileWakeupGoˢᵇ;
private static readonly @string futileWakeupGoˢᵇ = "futile-wakeup.go"u8;
internal static readonly object didNotSeeAGoroutineInAˢ = (@string)"did not see a goroutine in a the region 'special'"u8;

public static void TestTraceFutileWakeup(ж<testing.T> t) {
    testTraceProg(t, futileWakeupGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _Δp2, @string _Δp3) => {
        // Check to make sure that no goroutine in the "special" trace region
        // ends up blocking, unblocking, then immediately blocking again.
        //
        // The goroutines are careful to call runtime.Gosched in between blocking,
        // so there should never be a clean block/unblock on the goroutine unless
        // the runtime was generating extraneous events.
        const nint entered = iota;
        const nint blocked = 1;
        const nint runnable = 2;
        const nint running = 3;
        var gs = new map<Δtrace.GoID, nint>();
        var seenSpecialGoroutines = false;
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)));
        if (err != nil) {
            tΔ1.Error(err);
        }
        for (;;) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            // Only track goroutines in the special region we control, so runtime
            // goroutines don't interfere (it's totally valid in traces for a
            // goroutine to block, run, and block again; that's not what we care about).
            if (ev.Kind() == Δtrace.EventRegionBegin && ev.Region().Type == "special"u8) {
                seenSpecialGoroutines = true;
                gs[ev.Goroutine()] = entered;
            }
            if (ev.Kind() == Δtrace.EventRegionEnd && ev.Region().Type == "special"u8) {
                delete(gs, ev.Goroutine());
            }
            // Track state transitions for goroutines we care about.
            //
            // The goroutines we care about will advance through the state machine
            // of entered -> blocked -> runnable -> running. If in the running state
            // we block, then we have a futile wakeup. Because of the runtime.Gosched
            // on these specially marked goroutines, we should end up back in runnable
            // first. If at any point we go to a different state, switch back to entered
            // and wait for the next time the goroutine blocks.
            if (ev.Kind() != Δtrace.EventStateTransition) {
                continue;
            }
            var st = ev.StateTransition();
            if (st.Resource.Kind != Δtrace.ResourceGoroutine) {
                continue;
            }
            var id = st.Resource.Goroutine();
            var (state, ok) = gs[id, ꟷ];
            if (!ok) {
                continue;
            }
            var (_, @new) = st.Goroutine();
            switch (state) {
            case var ᴛ when ᴛ == entered: {
                if (@new == Δtrace.GoWaiting) {
                    state = blocked;
                } else {
                    state = entered;
                }
                break;
            }
            case var ᴛ when ᴛ == blocked: {
                if (@new == Δtrace.GoRunnable) {
                    state = runnable;
                } else {
                    state = entered;
                }
                break;
            }
            case var ᴛ when ᴛ == runnable: {
                if (@new == Δtrace.GoRunning) {
                    state = running;
                } else {
                    state = entered;
                }
                break;
            }
            case var ᴛ when ᴛ == running: {
                if (@new == Δtrace.GoWaiting) {
                    tΔ1.Fatalf("found futile wakeup on goroutine %d", id);
                } else {
                    state = entered;
                }
                break;
            }}
            gs[id] = state;
        }
        if (!seenSpecialGoroutines) {
            tΔ1.Fatal(didNotSeeAGoroutineInAˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gcStressGoˢ => ref gcStressGoˢᵇ;
private static readonly @string gcStressGoˢᵇ = "gc-stress.go"u8;

public static void TestTraceGCStress(ж<testing.T> t) {
    testTraceProg(t, gcStressGoˢ, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gomaxprocsGoˢ => ref gomaxprocsGoˢᵇ;
private static readonly @string gomaxprocsGoˢᵇ = "gomaxprocs.go"u8;

public static void TestTraceGOMAXPROCS(ж<testing.T> t) {
    testTraceProg(t, gomaxprocsGoˢ, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stacksGoˢ => ref stacksGoˢᵇ;
private static readonly @string stacksGoˢᵇ = "stacks.go"u8;
internal static ref readonly @string asyncpreemptoff1ˢ => ref asyncpreemptoff1ˢᵇ;
private static readonly @string asyncpreemptoff1ˢᵇ = "asyncpreemptoff=1"u8;
internal static ref readonly @string traceadvanceperiod0ˢ => ref traceadvanceperiod0ˢᵇ;
private static readonly @string traceadvanceperiod0ˢᵇ = "traceadvanceperiod=0"u8;

internal partial struct TestTraceStacks_frame /*dyn*/ {
    internal @string fn;
    internal nint line;
}

internal partial struct TestTraceStacks_evDesc /*dyn*/ {
    internal Δtrace.EventKind kind;
    internal @string match;
    internal slice<TestTraceStacks_frame> frames;
}

public static void TestTraceStacks(ж<testing.T> t) {
    testTraceProg(t, stacksGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _, @string godebug) => {
        // mainLine is the line number of `func main()` in testprog/stacks.go.
        UntypedInt mainLine = 21;
        var want = slice<TestTraceStacks_evDesc>([
            new(Δtrace.EventStateTransition, "Goroutine Running->Runnable", slice<TestTraceStacks_frame>([
                new("runtime.Gosched", 0),
                new("main.main", mainLine + 87)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine NotExist->Runnable", slice<TestTraceStacks_frame>([
                new("main.main", mainLine + 11)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("runtime.block", 0),
                new("main.main.func1", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("runtime.chansend1", 0),
                new("main.main.func2", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("runtime.chanrecv1", 0),
                new("main.main.func3", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("runtime.chanrecv1", 0),
                new("main.main.func4", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable", slice<TestTraceStacks_frame>([
                new("runtime.chansend1", 0),
                new("main.main", mainLine + 89)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("runtime.chansend1", 0),
                new("main.main.func5", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable", slice<TestTraceStacks_frame>([
                new("runtime.chanrecv1", 0),
                new("main.main", mainLine + 90)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("runtime.selectgo", 0),
                new("main.main.func6", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable", slice<TestTraceStacks_frame>([
                new("runtime.selectgo", 0),
                new("main.main", mainLine + 91)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("sync.(*Mutex).Lock", 0),
                new("main.main.func7", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable", slice<TestTraceStacks_frame>([
                new("sync.(*Mutex).Unlock", 0),
                new("main.main", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("sync.(*WaitGroup).Wait", 0),
                new("main.main.func8", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable", slice<TestTraceStacks_frame>([
                new("sync.(*WaitGroup).Add", 0),
                new("sync.(*WaitGroup).Done", 0),
                new("main.main", mainLine + 96)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("sync.(*Cond).Wait", 0),
                new("main.main.func9", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable", slice<TestTraceStacks_frame>([
                new("sync.(*Cond).Signal", 0),
                new("main.main", 0)
            ])),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                new("time.Sleep", 0),
                new("main.main", 0)
            ])),
            new(Δtrace.EventMetric, "/sched/gomaxprocs:threads", slice<TestTraceStacks_frame>([
                new("runtime.startTheWorld", 0), // this is when the current gomaxprocs is logged.
                new("runtime.startTheWorldGC", 0),
                new("runtime.GOMAXPROCS", 0),
                new("main.main", 0)
            ]))
        ]);
        var asyncPreemptOff = strings.Contains(godebug, asyncpreemptoff1ˢ);
        if (asyncPreemptOff) {
            // Only check for syncPreemptPoint if asynchronous preemption is disabled.
            // Otherwise, the syncPreemptPoint goroutine might be exclusively async
            // preempted, causing this test to flake.
            var syncPreemptEv = new TestTraceStacks_evDesc(Δtrace.EventStateTransition, "Goroutine Running->Runnable", slice<TestTraceStacks_frame>([
                new("main.syncPreemptPoint", 0),
                new("main.main.func12", 0)
            ])
            );
            want = append(want, syncPreemptEv);
        }
        var stress = strings.Contains(godebug, traceadvanceperiod0ˢ);
        if (!stress) {
            // Only check for this stack if !stress because traceAdvance alone could
            // allocate enough memory to trigger a GC if called frequently enough.
            // This might cause the runtime.GC call we're trying to match against to
            // coalesce with an active GC triggered this by traceAdvance. In that case
            // we won't have an EventRangeBegin event that matches the stace trace we're
            // looking for, since runtime.GC will not have triggered the GC.
            var gcEv = new TestTraceStacks_evDesc(Δtrace.EventRangeBegin, "GC concurrent mark phase", slice<TestTraceStacks_frame>([
                new("runtime.GC", 0),
                new("main.main", 0)
            ])
            );
            want = append(want, gcEv);
        }
        if (Δruntime.GOOS != "windows"u8 && Δruntime.GOOS != "plan9"u8) {
            want = appendꓸꓸꓸ(want, slice<TestTraceStacks_evDesc>([
                new(Δtrace.EventStateTransition, "Goroutine Running->Waiting", slice<TestTraceStacks_frame>([
                    new("internal/poll.(*FD).Accept", 0),
                    new("net.(*netFD).accept", 0),
                    new("net.(*TCPListener).accept", 0),
                    new("net.(*TCPListener).Accept", 0),
                    new("main.main.func10", 0)
                ])),
                new(Δtrace.EventStateTransition, "Goroutine Running->Syscall", slice<TestTraceStacks_frame>([
                    new("syscall.read", 0),
                    new("syscall.Read", 0),
                    new("internal/poll.ignoringEINTRIO", 0),
                    new("internal/poll.(*FD).Read", 0),
                    new("os.(*File).read", 0),
                    new("os.(*File).Read", 0),
                    new("main.main.func11", 0)
                ]))
            ]));
            if (Δruntime.GOOS == "darwin"u8) {
                want[len(want) - 1].frames = appendꓸꓸꓸ(slice<TestTraceStacks_frame>([new("syscall.syscall", 0)]), want[len(want) - 1].frames);
            }
        }
        bool stackMatches(traceꓸStack stk, slice<TestTraceStacks_frame> frames) {
            foreach (var (i, f) in slices.Collect(stk.Frames())) {
                if (f.Func != frames[i].fn) {
                    return false;
                }
                {
                    var line = (uint64)frames[i].line; if (line != 0 && line != f.Line) {
                        return false;
                    }
                }
            }
            return true;
        }
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)));
        if (err != nil) {
            tΔ1.Error(err);
        }
        for (;;) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                tΔ1.Fatal(errΔ1);
            }
            foreach (var (i, wantEv) in want) {
                if (wantEv.kind != ev.Kind()) {
                    continue;
                }
                var match = false;
                switch (ev.Kind()) {
                case var ᴛ when ᴛ == Δtrace.EventStateTransition: {
                    var st = ev.StateTransition();
                    @string str = ""u8;
                    switch (st.Resource.Kind) {
                    case var ᴛ2 when ᴛ2 == Δtrace.ResourceGoroutine: {
                        var (old, @new) = st.Goroutine();
                        str = fmt.Sprintf("%s %s->%s"u8, st.Resource.Kind, old, @new);
                        break;
                    }}
                    match = str == wantEv.match;
                    break;
                }
                case var ᴛ when ᴛ == Δtrace.EventRangeBegin: {
                    var rng = ev.Range();
                    match = rng.Name == wantEv.match;
                    break;
                }
                case var ᴛ when ᴛ == Δtrace.EventMetric: {
                    var metric = ev.Metric();
                    match = metric.Name == wantEv.match;
                    break;
                }}
                match = match && stackMatches(ev.Stack(), wantEv.frames);
                if (match) {
                    want[i] = want[len(want) - 1];
                    want = want.slice(0, len(want) - 1);
                    break;
                }
            }
        }
        if (len(want) != 0) {
            foreach (var (_, ev) in want) {
                tΔ1.Errorf("no match for %s Match=%s Stack=%#v", ev.kind, ev.match, ev.frames);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stressGoˢ => ref stressGoˢᵇ;
private static readonly @string stressGoˢᵇ = "stress.go"u8;

public static void TestTraceStress(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skip("no os.Pipe on " + Δruntime.GOOS);
        break;
    }}
    testTraceProg(t, stressGoˢ, checkReaderDeterminism);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stressStartStopGoˢ => ref stressStartStopGoˢᵇ;
private static readonly @string stressStartStopGoˢᵇ = "stress-start-stop.go"u8;

public static void TestTraceStressStartStop(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8: {
        t.Skip("no os.Pipe on " + Δruntime.GOOS);
        break;
    }}
    testTraceProg(t, stressStartStopGoˢ, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string manyStartStopGoˢ => ref manyStartStopGoˢᵇ;
private static readonly @string manyStartStopGoˢᵇ = "many-start-stop.go"u8;

public static void TestTraceManyStartStop(ж<testing.T> t) {
    testTraceProg(t, manyStartStopGoˢ, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string waitOnPipeGoˢ => ref waitOnPipeGoˢᵇ;
private static readonly @string waitOnPipeGoˢᵇ = "wait-on-pipe.go"u8;

public static void TestTraceWaitOnPipe(ж<testing.T> t) {
    var exprᴛ1 = Δruntime.GOOS;
    switch (ᐧ) {
    case {} when exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8: {
        testTraceProg(t, waitOnPipeGoˢ, default);
        return;
    }}
    t.Skip("no applicable syscall.Pipe on " + Δruntime.GOOS);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string iterPullGoˢ => ref iterPullGoˢᵇ;
private static readonly @string iterPullGoˢᵇ = "iter-pull.go"u8;

public static void TestTraceIterPull(ж<testing.T> t) {
    testTraceProg(t, iterPullGoˢ, default);
}

internal static void checkReaderDeterminism(ж<testing.T> t, slice<byte> tb, slice<byte> _Δp2, @string _Δp3) {
    var tbʗ1 = tb;
    slice<traceꓸEvent> events() {
        slice<traceꓸEvent> evs = default;

        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tbʗ1)));
        if (err != nil) {
            t.Error(err);
        }
        for (;;) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            evs = append(evs, ev.ΔClone());
        }

        return evs;
    }

    var evs1 = events();
    var evs2 = events();

    {
        nint l1 = len(evs1);
        nint l2 = len(evs2); if (l1 != l2) {
            t.Fatalf("re-reading trace gives different event count (%d != %d)", l1, l2);
        }
    }
    foreach (var (i, vᴛ1) in evs1) {
        var ev1 = vᴛ1.ΔClone();

        var ev2 = evs2[i].ΔClone();
        {
            @string s1 = ev1.String();
            @string s2 = ev2.String(); if (s1 != s2) {
                t.Errorf("re-reading trace gives different event %d:\n%s\n%s\n", i, s1, s2);
                break;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gotipˢ => ref gotipˢᵇ;
private static readonly @string gotipˢᵇ = "gotip"u8;
internal static ref readonly @string go1ˢ => ref go1ˢᵇ;
private static readonly @string go1ˢᵇ = "go1"u8;
internal static readonly object testFailsOnFreebsdAmd64ˢ = (@string)"test fails on freebsd-amd64-race in LUCI; see go.dev/issue/71556"u8;
internal static ref readonly @string testdataTestprogˢ => ref testdataTestprogˢᵇ;
private static readonly @string testdataTestprogˢᵇ = "./testdata/testprog"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string tracecheckstackownershipˢ => ref tracecheckstackownershipˢᵇ;
private static readonly @string tracecheckstackownershipˢᵇ = "tracecheckstackownership=1"u8;
internal static ref readonly @string gotracebackˢ => ref gotracebackˢᵇ;
private static readonly @string gotracebackˢᵇ = "GOTRACEBACK"u8;
internal static ref readonly @string stressˢ => ref stressˢᵇ;
private static readonly @string stressˢᵇ = "stress"u8;
internal static ref readonly @string defaultˢ => ref defaultˢᵇ;
private static readonly @string defaultˢᵇ = "default"u8;
internal static ref readonly @string defaultˢ2 => ref defaultˢ2ᵇ;
private static readonly @string defaultˢ2ᵇ = "Default"u8;
internal static ref readonly @string asyncPreemptOffˢ => ref asyncPreemptOffˢᵇ;
private static readonly @string asyncPreemptOffˢᵇ = "AsyncPreemptOff"u8;
internal static readonly object skippingTraceAsyncˢ = (@string)"skipping trace async preempt off tests in short mode"u8;
internal static ref readonly @string stressˢ2 => ref stressˢ2ᵇ;
private static readonly @string stressˢ2ᵇ = "Stress"u8;
internal static readonly object skippingTraceStressTestsˢ = (@string)"skipping trace stress tests in short mode"u8;
internal static ref readonly @string allocFreeˢ => ref allocFreeˢᵇ;
private static readonly @string allocFreeˢᵇ = "AllocFree"u8;
internal static readonly object skippingTraceAllocFreeˢ = (@string)"skipping trace alloc/free tests by default; too flaky (see go.dev/issue/70838)"u8;
internal static readonly object skippingTraceAllocFreeˢ2 = (@string)"skipping trace alloc/free tests in short mode"u8;
internal static ref readonly @string traceallocfree1ˢ => ref traceallocfree1ˢᵇ;
private static readonly @string traceallocfree1ˢᵇ = "traceallocfree=1"u8;

internal static void testTraceProg(ж<testing.T> t, @string progName, Action<ж<testing.T>, slice<byte>, slice<byte>, @string> extra) {
    testenv.MustHaveGoRun(new trace_test_package.testing_TжTB(t));

    // Check if we're on a builder.
    var onBuilder = testenv.Builder() != ""u8;
    var onOldBuilder = !strings.Contains(testenv.Builder(), gotipˢ) && !strings.Contains(testenv.Builder(), go1ˢ);

    if (progName == "cgo-callback.go"u8 && onBuilder && !onOldBuilder && Δruntime.GOOS == "freebsd"u8 && Δruntime.GOARCH == "amd64"u8 && race.Enabled) {
        t.Skip(testFailsOnFreebsdAmd64ˢ);
    }

    @string testPath = filepath.Join(testdataTestprogˢ, progName);
    @string testName = progName;
    void runTest(ж<testing.T> tΔ1, bool stress, @string extraGODEBUG) {
        // Build the program.
        var (binFile, err) = os.CreateTemp(""u8, progName);
        if (err != nil) {
            tΔ1.Fatalf("failed to create temporary output file: %v", err);
        }
        @string bin = binFile.Name();
        binFile.Close();
        tΔ1.Cleanup(() => {
            os.Remove(bin);
        });
        var buildCmd = testenv.CommandContext(new trace_test_package.testing_TжTB(tΔ1), tΔ1.Context(), testenv.GoToolPath(new trace_test_package.testing_TжTB(tΔ1)), buildˢ, "-o", bin);
        if (race.Enabled) {
            buildCmd.Args = append(buildCmd.Args, (@string)"-race");
        }
        buildCmd.Args = append(buildCmd.Args, testPath);
        (var buildOutput, err) = buildCmd.CombinedOutput();
        if (err != nil) {
            tΔ1.Fatalf("failed to build %s: %v: output:\n%s", testPath, err, buildOutput);
        }

        // Run the program and capture the trace, which is always written to stdout.
        var cmd = testenv.CommandContext(new trace_test_package.testing_TжTB(tΔ1), tΔ1.Context(), bin);

        // Add a stack ownership check. This is cheap enough for testing.
        @string godebug = tracecheckstackownershipˢ;
        if (stress) {
            // Advance a generation constantly to stress the tracer.
            godebug += ",traceadvanceperiod=0"u8;
        }
        if (extraGODEBUG != ""u8) {
            // Add extra GODEBUG flags.
            godebug += ","u8 + extraGODEBUG;
        }
        cmd.Env = append(cmd.Env, "GODEBUG="u8 + godebug);
        {
            var (_, ok) = os.LookupEnv(gotracebackˢ); if (!ok) {
                // Unless overriden, set GOTRACEBACK=crash.
                cmd.Env = append(cmd.Env, (@string)"GOTRACEBACK=crash");
            }
        }

        // Capture stdout and stderr.
        //
        // The protocol for these programs is that stdout contains the trace data
        // and stderr is an expectation in string format.
        ref var traceBuf = ref heap(new bytes.Buffer(), out var ᏑtraceBuf);
        ref var errBuf = ref heap(new bytes.Buffer(), out var ᏑerrBuf);
        cmd.Stdout = new trace_test_package.bytes_BufferжWriter(ᏑtraceBuf);
        cmd.Stderr = new trace_test_package.bytes_BufferжWriter(ᏑerrBuf);
        // Run the program.
        {
            var errΔ1 = cmd.Run(); if (errΔ1 != nil) {
                if (errBuf.Len() != 0) {
                    tΔ1.Logf("stderr: %s", (@string)errBuf.Bytes());
                }
                tΔ1.Fatal(errΔ1);
            }
        }
        var tb = traceBuf.Bytes();

        // Test the trace and the parser.
        var v = testtrace.NewValidator();
        v.GoVersion = version.Current;
        if (Δruntime.GOOS == "windows"u8 && stress) {
            // Under stress mode we're constantly advancing trace generations.
            // Windows' clock granularity is too coarse to guarantee monotonic
            // timestamps for monotonic and wall clock time in this case, so
            // skip the checks.
            v.SkipClockSnapshotChecks();
        }
        testReader(tΔ1, new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)), v, testtrace.ExpectSuccess());

        // Run some extra validation.
        if (!tΔ1.Failed() && extra != nil) {
            extra(tΔ1, tb, errBuf.Bytes(), godebug);
        }

        // Dump some more information on failure.
        if (tΔ1.Failed() || dumpTraces.Value) {
            @string suffix(bool stressΔ1) {
                if (stressΔ1) {
                    return stressˢ;
                }
                return defaultˢ;
            }
            testtrace.Dump(tΔ1, fmt.Sprintf("%s.%s"u8, testName, suffix(stress)), tb, dumpTraces.Value);
        }
    }
    var runTestʗ1 = runTest;
    t.Run(defaultˢ2, (ж<testing.T> tΔ2) => {
        runTestʗ1(tΔ2, false, ""u8);
    });
    var runTestʗ2 = runTest;
    t.Run(asyncPreemptOffˢ, (ж<testing.T> tΔ3) => {
        if (testing.Short() && Δruntime.NumCPU() < 2) {
            tΔ3.Skip(skippingTraceAsyncˢ);
        }
        runTestʗ2(tΔ3, false, asyncpreemptoff1ˢ);
    });
    var runTestʗ3 = runTest;
    t.Run(stressˢ2, (ж<testing.T> tΔ4) => {
        if (testing.Short()) {
            tΔ4.Skip(skippingTraceStressTestsˢ);
        }
        runTestʗ3(tΔ4, true, ""u8);
    });
    var runTestʗ4 = runTest;
    t.Run(allocFreeˢ, (ж<testing.T> tΔ5) => {
        if (!allocFree.Value) {
            tΔ5.Skip(skippingTraceAllocFreeˢ);
        }
        if (testing.Short()) {
            tΔ5.Skip(skippingTraceAllocFreeˢ2);
        }
        runTestʗ4(tΔ5, false, traceallocfree1ˢ);
    });
}

} // end trace_test_package
