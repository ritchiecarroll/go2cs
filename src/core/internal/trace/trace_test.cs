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
internal static readonly @string annotationsGoˢ = "annotations.go"u8;

internal partial struct TestTraceAnnotations_evDesc /*dyn*/ {
    internal Δtrace.EventKind kind;
    internal Δtrace.TaskID task;
    internal slice<@string> args;
}

public static void TestTraceAnnotations(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, annotationsGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _Δp2, @string _Δp3) => {
        var want = new TestTraceAnnotations_evDesc[]{
            new(Δtrace.EventTaskBegin, ((Δtrace.TaskID)1), new @string[]{"task0"u8}.slice()),
            new(Δtrace.EventRegionBegin, ((Δtrace.TaskID)1), new @string[]{"region0"u8}.slice()),
            new(Δtrace.EventRegionBegin, ((Δtrace.TaskID)1), new @string[]{"region1"u8}.slice()),
            new(Δtrace.EventLog, ((Δtrace.TaskID)1), new @string[]{"key0"u8, "0123456789abcdef"u8}.slice()),
            new(Δtrace.EventRegionEnd, ((Δtrace.TaskID)1), new @string[]{"region1"u8}.slice()),
            new(Δtrace.EventRegionEnd, ((Δtrace.TaskID)1), new @string[]{"region0"u8}.slice()),
            new(Δtrace.EventTaskEnd, ((Δtrace.TaskID)1), new @string[]{"task0"u8}.slice()),
            //  Currently, pre-existing region is not recorded to avoid allocations.
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

    testTraceProg(Ꮡt, cpuProfileGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> stderr, @string _Δp3) => {
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
    testTraceProg(Ꮡt, futileWakeupGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _Δp2, @string _Δp3) => {
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
internal static readonly @string asyncpreemptoff1ˢ = "asyncpreemptoff=1"u8;
internal static readonly @string traceadvanceperiod0ˢ = "traceadvanceperiod=0"u8;

internal partial struct TestTraceStacks_frame /*dyn*/ {
    internal @string fn;
    internal nint line;
}

internal partial struct TestTraceStacks_evDesc /*dyn*/ {
    internal Δtrace.EventKind kind;
    internal @string match;
    internal slice<TestTraceStacks_frame> frames;
}

public static void TestTraceStacks(ж<testing.T> Ꮡt) {
    testTraceProg(Ꮡt, stacksGoˢ, (ж<testing.T> tΔ1, slice<byte> tb, slice<byte> _, @string godebug) => {
        // mainLine is the line number of `func main()` in testprog/stacks.go.
        UntypedInt mainLine = 21;
        var want = new TestTraceStacks_evDesc[]{
            new(Δtrace.EventStateTransition, "Goroutine Running->Runnable"u8, new TestTraceStacks_frame[]{
                new("runtime.Gosched"u8, 0),
                new("main.main"u8, mainLine + 87)
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
                new("main.main"u8, mainLine + 89)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.chansend1"u8, 0),
                new("main.main.func5"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("runtime.chanrecv1"u8, 0),
                new("main.main"u8, mainLine + 90)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Running->Waiting"u8, new TestTraceStacks_frame[]{
                new("runtime.selectgo"u8, 0),
                new("main.main.func6"u8, 0)
            }.slice()),
            new(Δtrace.EventStateTransition, "Goroutine Waiting->Runnable"u8, new TestTraceStacks_frame[]{
                new("runtime.selectgo"u8, 0),
                new("main.main"u8, mainLine + 91)
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
                new("main.main"u8, mainLine + 96)
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
        var asyncPreemptOff = strings.Contains(godebug, asyncpreemptoff1ˢ);
        if (asyncPreemptOff) {
            // Only check for syncPreemptPoint if asynchronous preemption is disabled.
            // Otherwise, the syncPreemptPoint goroutine might be exclusively async
            // preempted, causing this test to flake.
            var syncPreemptEv = new TestTraceStacks_evDesc(Δtrace.EventStateTransition, "Goroutine Running->Runnable"u8, new TestTraceStacks_frame[]{
                new("main.syncPreemptPoint"u8, 0),
                new("main.main.func12"u8, 0)
            }.slice()
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
            if (Δruntime.GOOS == "darwin"u8) {
                want[len(want) - 1].frames = appendꓸꓸꓸ(new TestTraceStacks_frame[]{new("syscall.syscall"u8, 0)}.slice(), want[len(want) - 1].frames);
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

internal static void checkReaderDeterminism(ж<testing.T> Ꮡt, slice<byte> tb, slice<byte> _Δp2, @string _Δp3) {
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
internal static readonly object testFailsOnFreebsdAmd64ˢ = (@string)"test fails on freebsd-amd64-race in LUCI; see go.dev/issue/71556"u8;
internal static readonly @string testdataTestprogˢ = "./testdata/testprog"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string tracecheckstackownershipˢ = "tracecheckstackownership=1"u8;
internal static readonly @string gotracebackˢ = "GOTRACEBACK"u8;
internal static readonly @string stressˢ = "stress"u8;
internal static readonly @string defaultˢ = "default"u8;
internal static readonly @string defaultˢ2 = "Default"u8;
internal static readonly @string asyncPreemptOffˢ = "AsyncPreemptOff"u8;
internal static readonly object skippingTraceAsyncˢ = (@string)"skipping trace async preempt off tests in short mode"u8;
internal static readonly @string stressˢ2 = "Stress"u8;
internal static readonly object skippingTraceStressTestsˢ = (@string)"skipping trace stress tests in short mode"u8;
internal static readonly @string allocFreeˢ = "AllocFree"u8;
internal static readonly object skippingTraceAllocFreeˢ = (@string)"skipping trace alloc/free tests by default; too flaky (see go.dev/issue/70838)"u8;
internal static readonly object skippingTraceAllocFreeˢ2 = (@string)"skipping trace alloc/free tests in short mode"u8;
internal static readonly @string traceallocfree1ˢ = "traceallocfree=1"u8;

internal static void testTraceProg(ж<testing.T> Ꮡt, @string progName, Action<ж<testing.T>, slice<byte>, slice<byte>, @string> extra) {
    testenv.MustHaveGoRun(new trace_test_package.testing_TжTB(Ꮡt));

    // Check if we're on a builder.
    var onBuilder = testenv.Builder() != ""u8;
    var onOldBuilder = !strings.Contains(testenv.Builder(), gotipˢ) && !strings.Contains(testenv.Builder(), go1ˢ);

    if (progName == "cgo-callback.go"u8 && onBuilder && !onOldBuilder && Δruntime.GOOS == "freebsd"u8 && Δruntime.GOARCH == "amd64"u8 && race.Enabled) {
        Ꮡt.Skip(testFailsOnFreebsdAmd64ˢ);
    }

    @string testPath = filepath.Join(testdataTestprogˢ, progName);
    @string testName = progName;
    void runTest(ж<testing.T> tΔ1, bool stress, @string extraGODEBUG) {
        // Build the program.
        var (binFile, err) = os.CreateTemp(""u8, progName);
        if (err != default!) {
            tΔ1.Fatalf("failed to create temporary output file: %v"u8, err);
        }
        @string bin = binFile.Name();
        binFile.Close();
        tΔ1.Cleanup(() => {
            os.Remove(bin);
        });
        var buildCmd = testenv.CommandContext(new trace_test_package.testing_TжTB(tΔ1), tΔ1.Context(), testenv.GoToolPath(new trace_test_package.testing_TжTB(tΔ1)), buildˢ, "-o", bin);
        if (race.Enabled) {
            buildCmd.Value.Args = append((~buildCmd).Args, "-race"u8);
        }
        buildCmd.Value.Args = append((~buildCmd).Args, testPath);
        (var buildOutput, err) = buildCmd.CombinedOutput();
        if (err != default!) {
            tΔ1.Fatalf("failed to build %s: %v: output:\n%s"u8, testPath, err, buildOutput);
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
        cmd.Value.Env = append((~cmd).Env, "GODEBUG="u8 + godebug);
        {
            var (_, ok) = os.LookupEnv(gotracebackˢ); if (!ok) {
                // Unless overriden, set GOTRACEBACK=crash.
                cmd.Value.Env = append((~cmd).Env, "GOTRACEBACK=crash"u8);
            }
        }

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
            var errΔ1 = cmd.Run(); if (errΔ1 != default!) {
                if (errBuf.Len() != 0) {
                    tΔ1.Logf("stderr: %s"u8, ((@string)errBuf.Bytes()));
                }
                tΔ1.Fatal(errΔ1);
            }
        }
        var tb = traceBuf.Bytes();

        // Test the trace and the parser.
        var v = testtrace.NewValidator();
        v.Value.GoVersion = version.Current;
        if (Δruntime.GOOS == "windows"u8 && stress) {
            // Under stress mode we're constantly advancing trace generations.
            // Windows' clock granularity is too coarse to guarantee monotonic
            // timestamps for monotonic and wall clock time in this case, so
            // skip the checks.
            v.SkipClockSnapshotChecks();
        }
        testReader(tΔ1, new trace_test_package.bytes_ReaderжReader(bytes.NewReader(tb)), v, testtrace.ExpectSuccess());

        // Run some extra validation.
        if (!tΔ1.Failed() && extra != default!) {
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
    Ꮡt.Run(defaultˢ2, (ж<testing.T> tΔ2) => {
        runTestʗ1(tΔ2, false, ""u8);
    });
    var runTestʗ2 = runTest;
    Ꮡt.Run(asyncPreemptOffˢ, (ж<testing.T> tΔ3) => {
        if (testing.Short() && Δruntime.NumCPU() < 2) {
            tΔ3.Skip(skippingTraceAsyncˢ);
        }
        runTestʗ2(tΔ3, false, asyncpreemptoff1ˢ);
    });
    var runTestʗ3 = runTest;
    Ꮡt.Run(stressˢ2, (ж<testing.T> tΔ4) => {
        if (testing.Short()) {
            tΔ4.Skip(skippingTraceStressTestsˢ);
        }
        runTestʗ3(tΔ4, true, ""u8);
    });
    var runTestʗ4 = runTest;
    Ꮡt.Run(allocFreeˢ, (ж<testing.T> tΔ5) => {
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
