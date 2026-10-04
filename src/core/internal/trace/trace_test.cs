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
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using exec = go.os.exec_package;
using go.@internal;
using go.@internal.trace;
using go.os;
using iter = iter_package;
using path;
using static go.@internal.trace_internal_test_package;

partial class trace_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string annotationsGoˢ = "annotations.go"u8;

[GoType("dyn")] internal partial struct TestTraceAnnotations_evDesc {
    internal Δtrace.EventKind kind;
    internal Δtrace.TaskID task;
    internal slice<@string> args;
}

public static void TestTraceAnnotations(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, annotationsGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _Δp2, bool _Δp3) => {
        var want = new TestTraceAnnotations_evDesc[]{
            new(Δtrace.EventTaskBegin, ((Δtrace.TaskID)1), new @string[]{"task0"u8}.slice()),
            new(Δtrace.EventRegionBegin, ((Δtrace.TaskID)1), new @string[]{"region0"u8}.slice()),
            new(Δtrace.EventRegionBegin, ((Δtrace.TaskID)1), new @string[]{"region1"u8}.slice()),
            new(Δtrace.EventLog, ((Δtrace.TaskID)1), new @string[]{"key0"u8, "0123456789abcdef"u8}.slice()),
            new(Δtrace.EventRegionEnd, ((Δtrace.TaskID)1), new @string[]{"region1"u8}.slice()),
            new(Δtrace.EventRegionEnd, ((Δtrace.TaskID)1), new @string[]{"region0"u8}.slice()),
            new(Δtrace.EventTaskEnd, ((Δtrace.TaskID)1), new @string[]{"task0"u8}.slice()), //  Currently, pre-existing region is not recorded to avoid allocations.

            new(Δtrace.EventRegionBegin, Δtrace.BackgroundTask, new @string[]{"post-existing region"u8}.slice())
        }.slice();
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)));
        if (err != default!) {
            tΔ1.Error(err);
        }
        while (ᐧ) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
            foreach (var (i, wantEv) in want) {
                if (wantEv.kind != ev.Kind()) {
                    continue;
                }
                var match = false;
                var exprᴛ1 = ev.Kind();
                if (exprᴛ1 == Δtrace.EventTaskBegin || exprᴛ1 == Δtrace.EventTaskEnd) {
                    var task = ev.Task();
                    match = task.ID == wantEv.task && task.Type == wantEv.args[0];
                }
                else if (exprᴛ1 == Δtrace.EventRegionBegin || exprᴛ1 == Δtrace.EventRegionEnd) {
                    var reg = ev.Region();
                    match = reg.Task == wantEv.task && reg.Type == wantEv.args[0];
                }
                else if (exprᴛ1 == Δtrace.EventLog) {
                    var log = ev.Log();
                    match = log.Task == wantEv.task && log.Category == wantEv.args[0] && log.Message == wantEv.args[1];
                }

                if (match) {
                    want[i] = want[len(want) - 1];
                    want = want.slice(0, len(want) - 1);
                    break;
                }
            }
        }
        if (len(want) != 0) {
            foreach (var (_, ev) in want) {
                tΔ1.Errorf("no match for %s TaskID=%d Args=%#v"u8, ev.kind, ev.task, ev.args);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string annotationsStressGoˢ = "annotations-stress.go"u8;

public static void TestTraceAnnotationsStress(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, annotationsStressGoˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgoCallbackGoˢ = "cgo-callback.go"u8;

public static void TestTraceCgoCallback(ж<testing.T> Ꮡt) {
    testenv.MustHaveCGO(new trace_test_package.testing_TжTB(Ꮡt));
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "plan9"u8 || exprᴛ1 == "windows"u8) {
        Ꮡt.Skipf("cgo callback test requires pthreads and is not supported on %s"u8, Δruntime.GOOS);
    }

    testTraceProg(Ꮡt, cgoCallbackGoˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cpuProfileGoˢ = "cpu-profile.go"u8;
internal static readonly object cpuProfileDidNotIncludeˢ = (@string)"CPU profile did not include any samples while tracing was active"u8;

public static void TestTraceCPUProfile(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testTraceProg(Ꮡt, cpuProfileGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> stderr, bool _Δp3) => {
        // Parse stderr which has a CPU profile summary, if everything went well.
        // (If it didn't, we shouldn't even make it here.)
        var scanner = bufio.NewScanner(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(stderr)));
        nint pprofSamples = 0;
        var pprofStacks = new map<@string, nint>();
        while (scanner.Scan()) {
            ref var stack = ref heap(new @string(), out var Ꮡstack);
            ref var samples = ref heap(new nint(), out var Ꮡsamples);
            var (_, errΔ1) = fmt.Sscanf(scanner.Text(), "%s\t%d"u8, Ꮡstack, Ꮡsamples);
            if (errΔ1 != default!) {
                tΔ1.Fatalf("failed to parse CPU profile summary in stderr: %s\n\tfull:\n%s"u8, scanner.Text(), stderr);
            }
            pprofStacks[stack] = samples;
            pprofSamples += samples;
        }
        {
            var errΔ2 = scanner.Err(); if (errΔ2 != default!) {
                tΔ1.Fatalf("failed to parse CPU profile summary in stderr: %v"u8, errΔ2);
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
        if (err != default!) {
            tΔ1.Error(err);
        }
        ж<traceꓸEvent> hogRegion = default!;
        bool hogRegionClosed = default!;
        while (ᐧ) {
            ref var ev = ref heap<traceꓸEvent>(out var Ꮡev);
            (ev, var errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
            if (ev.Kind() == Δtrace.EventRegionBegin && ev.Region().Type == "cpuHogger"u8) {
                hogRegion = Ꮡev;
            }
            if (ev.Kind() == Δtrace.EventStackSample) {
                totalTraceSamples++;
                if (hogRegion != nil && ev.Goroutine() == (~hogRegion).Goroutine()) {
                    traceSamples++;
                    slice<@string> fns = default!;
                    foreach (var frame in range<Δtrace.StackFrame>(ev.Stack().Frames().Invoke)) {
                        if (frame.Func != "runtime.goexit"u8) {
                            fns = append(fns, fmt.Sprintf("%s:%d"u8, frame.Func, frame.Line));
                        }
                    }
                    @string stack = strings.Join(fns, "|"u8);
                    traceStacks[stack]++;
                }
            }
            if (ev.Kind() == Δtrace.EventRegionEnd && ev.Region().Type == "cpuHogger"u8) {
                hogRegionClosed = true;
            }
        }
        if (hogRegion == nil){
            tΔ1.Fatalf("execution trace did not identify cpuHogger goroutine"u8);
        } else 
        if (!hogRegionClosed) {
            tΔ1.Fatalf("execution trace did not close cpuHogger region"u8);
        }
        // The execution trace may drop CPU profile samples if the profiling buffer
        // overflows. Based on the size of profBufWordCount, that takes a bit over
        // 1900 CPU samples or 19 thread-seconds at a 100 Hz sample rate. If we've
        // hit that case, then we definitely have at least one full buffer's worth
        // of CPU samples, so we'll call that success.
        var overflowed = totalTraceSamples >= 1900;
        if (traceSamples < pprofSamples) {
            tΔ1.Logf("execution trace did not include all CPU profile samples; %d in profile, %d in trace"u8, pprofSamples, traceSamples);
            if (!overflowed) {
                tΔ1.Fail();
            }
        }
        foreach (var (stack, traceSamplesΔ1) in traceStacks) {
            nint pprofSamplesΔ1 = pprofStacks[stack];
            delete(pprofStacks, stack);
            if (traceSamplesΔ1 < pprofSamplesΔ1) {
                tΔ1.Logf("execution trace did not include all CPU profile samples for stack %q; %d in profile, %d in trace"u8,
                    stack, pprofSamplesΔ1, traceSamplesΔ1);
                if (!overflowed) {
                    tΔ1.Fail();
                }
            }
        }
        foreach (var (stack, pprofSamplesΔ2) in pprofStacks) {
            tΔ1.Logf("CPU profile included %d samples at stack %q not present in execution trace"u8, pprofSamplesΔ2, stack);
            if (!overflowed) {
                tΔ1.Fail();
            }
        }
        if (tΔ1.Failed()) {
            tΔ1.Logf("execution trace CPU samples:"u8);
            foreach (var (stack, samples) in traceStacks) {
                tΔ1.Logf("%d: %q"u8, samples, stack);
            }
            tΔ1.Logf("CPU profile:\n%s"u8, stderr);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string futileWakeupGoˢ = "futile-wakeup.go"u8;
internal static readonly object didNotSeeAGoroutineInAˢ = (@string)"did not see a goroutine in a the region 'special'"u8;

public static void TestTraceFutileWakeup(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testTraceProg(Ꮡt, futileWakeupGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _Δp2, bool _Δp3) => {
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
        if (err != default!) {
            tΔ1.Error(err);
        }
        while (ᐧ) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
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
            var exprᴛ1 = state;
            if (exprᴛ1 == entered) {
                if (@new == Δtrace.GoWaiting){
                    state = blocked;
                } else {
                    state = entered;
                }
            }
            else if (exprᴛ1 == blocked) {
                if (@new == Δtrace.GoRunnable){
                    state = runnable;
                } else {
                    state = entered;
                }
            }
            else if (exprᴛ1 == runnable) {
                if (@new == Δtrace.GoRunning){
                    state = running;
                } else {
                    state = entered;
                }
            }
            else if (exprᴛ1 == running) {
                if (@new == Δtrace.GoWaiting){
                    tΔ1.Fatalf("found futile wakeup on goroutine %d"u8, id);
                } else {
                    state = entered;
                }
            }

            gs[id] = state;
        }
        if (!seenSpecialGoroutines) {
            tΔ1.Fatal(didNotSeeAGoroutineInAˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcStressGoˢ = "gc-stress.go"u8;

public static void TestTraceGCStress(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, gcStressGoˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gomaxprocsGoˢ = "gomaxprocs.go"u8;

public static void TestTraceGOMAXPROCS(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, gomaxprocsGoˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stacksGoˢ = "stacks.go"u8;

[GoType("dyn")] internal partial struct TestTraceStacks_frame {
    internal @string fn;
    internal nint line;
}

[GoType("dyn")] internal partial struct TestTraceStacks_evDesc {
    internal Δtrace.EventKind kind;
    internal @string match;
    internal slice<TestTraceStacks_frame> frames;
}

public static void TestTraceStacks(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testTraceProg(Ꮡt, stacksGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _, bool stress) => {
        // mainLine is the line number of `func main()` in testprog/stacks.go.
        UntypedInt mainLine = 21;
        var want = new TestTraceStacks_evDesc[]{
            new(Δtrace.EventStateTransition, "Goroutine Running->Runnable"u8, new TestTraceStacks_frame[]{
                new("main.main"u8, mainLine + 82)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine NotExist->Runnable"u8, new TestTraceStacks_frame[]{
                new("main.main"u8, mainLine + 11)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.block"u8, 0),
                new("main.main.func1"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.chansend1"u8, 0),
                new("main.main.func2"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.chanrecv1"u8, 0),
                new("main.main.func3"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.chanrecv1"u8, 0),
                new("main.main.func4"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("runtime.chansend1"u8, 0),
                new("main.main"u8, mainLine + 84)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.chansend1"u8, 0),
                new("main.main.func5"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("runtime.chanrecv1"u8, 0),
                new("main.main"u8, mainLine + 85)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.selectgo"u8, 0),
                new("main.main.func6"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("runtime.selectgo"u8, 0),
                new("main.main"u8, mainLine + 86)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("sync.(*Mutex).Lock"u8, 0),
                new("main.main.func7"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("sync.(*Mutex).Unlock"u8, 0),
                new("main.main"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("sync.(*WaitGroup).Wait"u8, 0),
                new("main.main.func8"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("sync.(*WaitGroup).Add"u8, 0),
                new("sync.(*WaitGroup).Done"u8, 0),
                new("main.main"u8, mainLine + 91)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("sync.(*Cond).Wait"u8, 0),
                new("main.main.func9"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("sync.(*Cond).Signal"u8, 0),
                new("main.main"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("time.Sleep"u8, 0),
                new("main.main"u8, 0)
            }.slice()),
            new(Δtrace.EventMetric, "/sched/gomaxprocs:threads"u8, new TestTraceStacks_frame[]{
                new("runtime.startTheWorld"u8, 0), // this is when the current gomaxprocs is logged.

                new("runtime.startTheWorldGC"u8, 0),
                new("runtime.GOMAXPROCS"u8, 0),
                new("main.main"u8, 0)
            }.slice())
        }.slice();
        if (!stress) {
            // Only check for this stack if !stress because traceAdvance alone could
            // allocate enough memory to trigger a GC if called frequently enough.
            // This might cause the runtime.GC call we're trying to match against to
            // coalesce with an active GC triggered this by traceAdvance. In that case
            // we won't have an EventRangeBegin event that matches the stace trace we're
            // looking for, since runtime.GC will not have triggered the GC.
            var gcEv = new TestTraceStacks_evDesc(Δtrace.EventRangeBegin, "GC concurrent mark phase"u8, new TestTraceStacks_frame[]{
                new("runtime.GC"u8, 0),
                new("main.main"u8, 0)
            }.slice()
            );
            want = append(want, gcEv);
        }
        if (Δruntime.GOOS != "windows"u8 && Δruntime.GOOS != "plan9"u8) {
            want = appendꓸꓸꓸ(want, new TestTraceStacks_evDesc[]{
                new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                    new("internal/poll.(*FD).Accept"u8, 0),
                    new("net.(*netFD).accept"u8, 0),
                    new("net.(*TCPListener).accept"u8, 0),
                    new("net.(*TCPListener).Accept"u8, 0),
                    new("main.main.func10"u8, 0)
                }.slice()),
                new(Δtrace.EventStateTransition, "Goroutine Running->Syscall"u8, new TestTraceStacks_frame[]{
                    new("syscall.read"u8, 0),
                    new("syscall.Read"u8, 0),
                    new("internal/poll.ignoringEINTRIO"u8, 0),
                    new("internal/poll.(*FD).Read"u8, 0),
                    new("os.(*File).read"u8, 0),
                    new("os.(*File).Read"u8, 0),
                    new("main.main.func11"u8, 0)
                }.slice())
            }.slice());
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
        if (err != default!) {
            tΔ1.Error(err);
        }
        while (ᐧ) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
                tΔ1.Fatal(errΔ1);
            }
            foreach (var (i, wantEv) in want) {
                if (wantEv.kind != ev.Kind()) {
                    continue;
                }
                var match = false;
                var exprᴛ1 = ev.Kind();
                if (exprᴛ1 == Δtrace.EventStateTransition) {
                    var st = ev.StateTransition();
                    @string str = ""u8;
                    var exprᴛ2 = st.Resource.Kind;
                    if (exprᴛ2 == Δtrace.ResourceGoroutine) {
                        var (old, @new) = st.Goroutine();
                        str = fmt.Sprintf("%s %s->%s"u8, st.Resource.Kind, old, @new);
                    }

                    match = str == wantEv.match;
                }
                else if (exprᴛ1 == Δtrace.EventRangeBegin) {
                    var rng = ev.Range();
                    match = rng.Name == wantEv.match;
                }
                else if (exprᴛ1 == Δtrace.EventMetric) {
                    var metric = ev.Metric();
                    match = metric.Name == wantEv.match;
                }

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
                tΔ1.Errorf("no match for %s Match=%s Stack=%#v"u8, ev.kind, ev.match, ev.frames);
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stressGoˢ = "stress.go"u8;

public static void TestTraceStress(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skip("no os.Pipe on " + Δruntime.GOOS);
    }

    testTraceProg(Ꮡt, stressGoˢ, checkReaderDeterminism);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stressStartStopGoˢ = "stress-start-stop.go"u8;

public static void TestTraceStressStartStop(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "js"u8 || exprᴛ1 == "wasip1"u8) {
        Ꮡt.Skip("no os.Pipe on " + Δruntime.GOOS);
    }

    testTraceProg(Ꮡt, stressStartStopGoˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string manyStartStopGoˢ = "many-start-stop.go"u8;

public static void TestTraceManyStartStop(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, manyStartStopGoˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string waitOnPipeGoˢ = "wait-on-pipe.go"u8;

public static void TestTraceWaitOnPipe(ж<testing.T> Ꮡt) {
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "dragonfly"u8 || exprᴛ1 == "freebsd"u8 || exprᴛ1 == "linux"u8 || exprᴛ1 == "netbsd"u8 || exprᴛ1 == "openbsd"u8 || exprᴛ1 == "solaris"u8) {
        testTraceProg(Ꮡt, waitOnPipeGoˢ, default!);
        return;
    }

    Ꮡt.Skip("no applicable syscall.Pipe on " + Δruntime.GOOS);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string iterPullGoˢ = "iter-pull.go"u8;

public static void TestTraceIterPull(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, iterPullGoˢ, default!);
}

internal static void checkReaderDeterminism(ж<testing.T> Ꮡt, slice<byte> tb, slice<byte> _Δp2, bool _Δp3) {
    var tbʗ1 = tb;
    slice<traceꓸEvent> events() {
        slice<traceꓸEvent> evs = default!;
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tbʗ1)));
        if (err != default!) {
            Ꮡt.Error(err);
        }
        while (ᐧ) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
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
            Ꮡt.Fatalf("re-reading trace gives different event count (%d != %d)"u8, l1, l2);
        }
    }
    foreach (var (i, vᴛ1) in evs1) {
        var ev1 = vᴛ1.ΔClone();

        var ev2 = evs2[i].ΔClone();
        {
            @string s1 = ev1.String();
            @string s2 = ev2.String(); if (s1 != s2) {
                Ꮡt.Errorf("re-reading trace gives different event %d:\n%s\n%s\n"u8, i, s1, s2);
                break;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gotipˢ = "gotip"u8;
internal static readonly @string go1ˢ = "go1"u8;
internal static readonly @string testdataTestprogˢ = "./testdata/testprog"u8;
internal static readonly @string runˢ = "run"u8;
internal static readonly @string tracecheckstackownershipˢ = "tracecheckstackownership=1"u8;
internal static readonly object foundBadTraceDumpingToˢ = (@string)"found bad trace; dumping to test log..."u8;
internal static readonly @string defaultˢ2 = "Default"u8;
internal static readonly @string stressˢ2 = "Stress"u8;
internal static readonly object skippingTraceStressTestsˢ = (@string)"skipping trace stress tests in short mode"u8;
internal static readonly @string allocFreeˢ = "AllocFree"u8;
internal static readonly object skippingTraceAllocFreeˢ = (@string)"skipping trace alloc/free tests in short mode"u8;
internal static readonly @string traceallocfree1ˢ = "traceallocfree=1"u8;

internal static void testTraceProg(ж<testing.T> Ꮡt, @string progName, Action<ж<testing.T>, slice<byte>, slice<byte>, bool> extra) {
    testenv.MustHaveGoRun(new trace_test_package.testing_TжTB(Ꮡt));
    // Check if we're on a builder.
    var onBuilder = testenv.Builder() != ""u8;
    var onOldBuilder = !strings.Contains(testenv.Builder(), gotipˢ) && !strings.Contains(testenv.Builder(), go1ˢ);
    @string testPath = filepath.Join(testdataTestprogˢ, progName);
    @string testName = progName;
    void runTest(ж<testing.T> tΔ1, bool stress, @string extraGODEBUG) {
        // Run the program and capture the trace, which is always written to stdout.
        var cmd = testenv.Command(new trace_test_package.testing_TжTB(tΔ1), testenv.GoToolPath(new trace_test_package.testing_TжTB(tΔ1)), runˢ);
        if (race.Enabled) {
            cmd.Value.Args = append((~cmd).Args, "-race"u8);
        }
        cmd.Value.Args = append((~cmd).Args, testPath);
        cmd.Value.Env = append(os.Environ(), "GOEXPERIMENT=rangefunc"u8);
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
        cmd.Value.Env = append((~cmd).Env, "GODEBUG="u8 + godebug);
        // Capture stdout and stderr.
        //
        // The protocol for these programs is that stdout contains the trace data
        // and stderr is an expectation in string format.
        ref var traceBuf = ref heap(new bytes.Buffer(), out var ᏑtraceBuf);
        ref var errBuf = ref heap(new bytes.Buffer(), out var ᏑerrBuf);
        cmd.Value.Stdout = new trace_test_package.bytes_BufferжWriter(ᏑtraceBuf);
        cmd.Value.Stderr = new trace_test_package.bytes_BufferжWriter(ᏑerrBuf);
        // Run the program.
        {
            var err = cmd.Run(); if (err != default!) {
                if (errBuf.Len() != 0) {
                    tΔ1.Logf("stderr: %s"u8, ((@string)errBuf.Bytes()));
                }
                tΔ1.Fatal(err);
            }
        }
        var tb = traceBuf.Bytes();
        // Test the trace and the parser.
        testReader(tΔ1, new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)), testtrace.ExpectSuccess());
        // Run some extra validation.
        if (!tΔ1.Failed() && extra != default!) {
            extra(tΔ1, tb, errBuf.Bytes(), stress);
        }
        // Dump some more information on failure.
        if (tΔ1.Failed() && onBuilder){
            // Dump directly to the test log on the builder, since this
            // data is critical for debugging and this is the only way
            // we can currently make sure it's retained.
            tΔ1.Log(foundBadTraceDumpingToˢ);
            @string s = dumpTraceToText(tΔ1, tb);
            if (onOldBuilder && len(s) > (1 << (int)(20)) + (512 << (int)(10))){
                // The old build infrastructure truncates logs at ~2 MiB.
                // Let's assume we're the only failure and give ourselves
                // up to 1.5 MiB to dump the trace.
                //
                // TODO(mknyszek): Remove this when we've migrated off of
                // the old infrastructure.
                tΔ1.Logf("text trace too large to dump (%d bytes)"u8, len(s));
            } else {
                tΔ1.Log(s);
            }
        } else 
        if (tΔ1.Failed() || dumpTraces.Value) {
            // We asked to dump the trace or failed. Write the trace to a file.
            tΔ1.Logf("wrote trace to file: %s"u8, dumpTraceToFile(tΔ1, testName, stress, tb));
        }
    }
    var runTestʗ1 = runTest;
    Ꮡt.Run(defaultˢ2, (ж<testing.T> tΔ2) => {
        runTestʗ1(tΔ2, false, ""u8);
    });
    var runTestʗ2 = runTest;
    Ꮡt.Run(stressˢ2, (ж<testing.T> tΔ3) => {
        if (testing.Short()) {
            tΔ3.Skip(skippingTraceStressTestsˢ);
        }
        runTestʗ2(tΔ3, true, ""u8);
    });
    var runTestʗ3 = runTest;
    Ꮡt.Run(allocFreeˢ, (ж<testing.T> tΔ4) => {
        if (testing.Short()) {
            tΔ4.Skip(skippingTraceAllocFreeˢ);
        }
        runTestʗ3(tΔ4, false, traceallocfree1ˢ);
    });
}

} // end trace_test_package
