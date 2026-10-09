// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using rand = go.math.rand_package;
using Δruntime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
using static time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using go.sync;
using static go.time_internal_test_package;
using Δtime = time_package;

partial class time_test_package {

// newTimerFunc simulates NewTimer using AfterFunc,
// but this version will not hit the special cases for channels
// that are used when calling NewTimer.
// This makes it easy to test both paths.
internal static ж<Δtime.Timer> newTimerFunc(Δtime.Duration d) {
    var c = new channel<Δtime.Time>(1);
    var cʗ1 = c;
    var t = AfterFunc(d, () => {
        cʗ1.ᐸꟷ(Now());
    });
    t.Value.C = c.WithDirection(GoChanDir.Recv);
    return t;
}

// haveHighResSleep is true if the system supports at least ~1ms sleeps.
//
//go:linkname haveHighResSleep runtime.haveHighResSleep
internal static bool haveHighResSleep { get => go.runtime_package.haveHighResSleep; set => go.runtime_package.haveHighResSleep = value; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object adjustingDelayForLowˢ = (@string)"adjusting delay for low resolution sleep"u8;

// adjustDelay returns an adjusted delay based on the system sleep resolution.
// Go runtime uses different Windows timers for time.Now and sleeping.
// These can tick at different frequencies and can arrive out of sync.
// The effect can be seen, for example, as time.Sleep(100ms) is actually
// shorter then 100ms when measured as difference between time.Now before and
// after time.Sleep call. This was observed on Windows XP SP3 (windows/386).
internal static Δtime.Duration adjustDelay(ж<Δtesting.T> Ꮡt, Δtime.Duration delay) {
    if (haveHighResSleep) {
        return delay;
    }
    Ꮡt.Log(adjustingDelayForLowˢ);
    var exprᴛ1 = Δruntime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        return delay - 17 * Millisecond;
    }
    { /* default: */
        Ꮡt.Fatal("adjustDelay unimplemented on " + Δruntime.GOOS);
        return 0;
    }

}

public static partial void TestSleep(ж<Δtesting.T> Ꮡt) {
    Δtime.Duration delay = /* 100 * Millisecond */ 100000000;
    goǃ(() => {
        Sleep(delay / 2);
        time_internal_test_package.Interrupt();
    });
    var start = Now();
    Sleep(delay);
    var delayadj = adjustDelay(Ꮡt, delay);
    var duration = Since(start);
    if (duration < delayadj) {
        Ꮡt.Fatalf("Sleep(%s) slept for only %s"u8, delay, duration);
    }
}

// Test the basic function calling behavior. Correct queuing
// behavior is tested elsewhere, since After and AfterFunc share
// the same code.
public static void TestAfterFunc(ж<Δtesting.T> Ꮡt) {
    nint i = 10;
    var c = new channel<bool>(0);
    ref var f = ref heap<Action>(out var Ꮡf);
    var cʗ1 = c;
    f = () => {
        i--;
        if (i >= 0){
            AfterFunc(0, Ꮡf.ValueSlot);
            Sleep(1 * ΔSecond);
        } else {
            cʗ1.ᐸꟷ(true);
        }
    };
    AfterFunc(0, f);
    ᐸꟷ(c);
}

public static partial void TestTickerStress(ж<Δtesting.T> Ꮡt) {
    ref var stop = ref heap(new atomic.Bool(), out var Ꮡstop);
    goǃ(() => {
        while (!Ꮡstop.Load()) {
            Δruntime.GC();
            // Yield so that the OS can wake up the timer thread,
            // so that it can generate channel sends for the main goroutine,
            // which will eventually set stop = 1 for us.
            Sleep(ΔNanosecond);
        }
    });
    var ticker = NewTicker(1);
    for (nint i = 0; i < 100; i++) {
        ᐸꟷ((~ticker).C);
    }
    ticker.Stop();
    Ꮡstop.Store(true);
}

public static partial void TestTickerConcurrentStress(ж<Δtesting.T> Ꮡt) {
    ref var stop = ref heap(new atomic.Bool(), out var Ꮡstop);
    goǃ(() => {
        while (!Ꮡstop.Load()) {
            Δruntime.GC();
            // Yield so that the OS can wake up the timer thread,
            // so that it can generate channel sends for the main goroutine,
            // which will eventually set stop = 1 for us.
            Sleep(ΔNanosecond);
        }
    });
    var ticker = NewTicker(1);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 10; i++) {
        Ꮡwg.Add(1);
        var tickerʗ1 = ticker;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
                    ᐸꟷ((~tickerʗ1).C);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    ticker.Stop();
    Ꮡstop.Store(true);
}

public static partial void TestAfterFuncStarvation(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Start two goroutines ping-ponging on a channel send.
        // At any given time, at least one of these goroutines is runnable:
        // if the channel buffer is full, the receiver is runnable,
        // and if it is not full, the sender is runnable.
        //
        // In addition, the AfterFunc callback should become runnable after
        // the indicated delay.
        //
        // Even if GOMAXPROCS=1, we expect the runtime to eventually schedule
        // the AfterFunc goroutine instead of the runnable channel goroutine.
        // However, in https://go.dev/issue/65178 this was observed to live-lock
        // on wasip1/wasm and js/wasm after <10000 runs.
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), ref ᒐ);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        ref var stop = ref heap(new atomic.Bool(), out var Ꮡstop);
        channel<bool> c = new channel<bool>(1);
        Ꮡwg.Add(2);
        var cʗ1 = c;
        goǃ(() => {
            while (!Ꮡstop.Load()) {
                cʗ1.ᐸꟷ(true);
            }
            close(cʗ1);
            Ꮡwg.Done();
        });
        var cʗ2 = c;
        goǃ(() => {
            foreach (var _ᴛ1 in cʗ2) {
            }
            Ꮡwg.Done();
        });
        AfterFunc(1 * Microsecond, () => {
            Ꮡstop.Store(true);
        });
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void benchmark(ж<Δtesting.B> Ꮡb, Action<ж<Δtesting.PB>> bench) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Create equal number of garbage timers on each P before starting
    // the benchmark.
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var garbageAll = new slice<slice<ж<Δtime.Timer>>>(Δruntime.GOMAXPROCS(0));
    foreach (var (i, _) in garbageAll) {
        Ꮡwg.Add(1);
        var garbageAllʗ1 = garbageAll;
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var garbage = new slice<ж<Δtime.Timer>>((1 << (int)(15)));
                foreach (var (j, _) in garbage) {
                    garbage[j] = AfterFunc(ΔHour, default!);
                }
                garbageAllʗ1[iΔ1] = garbage;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    Ꮡwg.Wait();
    b.ResetTimer();
    Ꮡb.RunParallel(bench);
    b.StopTimer();
    foreach (var (_, garbage) in garbageAll) {
        foreach (var (_, t) in garbage) {
            t.Stop();
        }
    }
}

public static void BenchmarkAfterFunc1000(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            nint n = 1000;
            var c = new channel<bool>(0);
            ref var f = ref heap<Action>(out var Ꮡf);
            var cʗ1 = c;
            Ꮡf.ValueSlot = () => {
                n--;
                if (n >= 0){
                    AfterFunc(0, Ꮡf.ValueSlot);
                } else {
                    cʗ1.ᐸꟷ(true);
                }
            };
            AfterFunc(0, Ꮡf.ValueSlot);
            ᐸꟷ(c);
        }
    });
}

public static void BenchmarkAfter(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            ᐸꟷ(After(1));
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string implChanˢ = "impl=chan"u8;
internal static readonly @string implFuncˢ = "impl=func"u8;

public static void BenchmarkStop(ж<Δtesting.B> Ꮡb) {
    Ꮡb.Run(implChanˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.B> bΔ1) => {
        benchmark(bΔ1, (ж<Δtesting.PB> pb) => {
            while (pb.Next()) {
                NewTimer(1 * ΔSecond).Stop();
            }
        });
    });
    Ꮡb.Run(implFuncˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.B> bΔ2) => {
        benchmark(bΔ2, (ж<Δtesting.PB> pb) => {
            while (pb.Next()) {
                newTimerFunc(1 * ΔSecond).Stop();
            }
        });
    });
}

public static void BenchmarkSimultaneousAfterFunc1000(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            nint n = 1000;
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(n);
            foreach (var _ᴛ1 in range(n)) {
                AfterFunc(0, Ꮡwg.Done);
            }
            Ꮡwg.Wait();
        }
    });
}

public static void BenchmarkStartStop1000(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            const nint N = 1000;
            var timers = new slice<ж<Δtime.Timer>>(N);
            foreach (var (i, _) in timers) {
                timers[i] = AfterFunc(ΔHour, default!);
            }
            foreach (var (i, _) in timers) {
                timers[i].Stop();
            }
        }
    });
}

public static void BenchmarkReset(ж<Δtesting.B> Ꮡb) {
    Ꮡb.Run(implChanˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.B> bΔ1) => {
        benchmark(bΔ1, (ж<Δtesting.PB> pb) => {
            var t = NewTimer(ΔHour);
            while (pb.Next()) {
                t.Reset(ΔHour);
            }
            t.Stop();
        });
    });
    Ꮡb.Run(implFuncˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.B> bΔ2) => {
        benchmark(bΔ2, (ж<Δtesting.PB> pb) => {
            var t = newTimerFunc(ΔHour);
            while (pb.Next()) {
                t.Reset(ΔHour);
            }
            t.Stop();
        });
    });
}

public static void BenchmarkSleep1000(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            const nint N = 1000;
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(N);
            foreach (var _ᴛ1 in range(N)) {
                goǃ(() => {
                    Sleep(ΔNanosecond);
                    Ꮡwg.Done();
                });
            }
            Ꮡwg.Wait();
        }
    });
}

public static void TestAfter(ж<Δtesting.T> Ꮡt) {
    Δtime.Duration delay = /* 100 * Millisecond */ 100000000;
    var start = Now();
    var end = ᐸꟷ(After(delay));
    var delayadj = adjustDelay(Ꮡt, delay);
    {
        var duration = Since(start); if (duration < delayadj) {
            Ꮡt.Fatalf("After(%s) slept for only %d ns"u8, delay, duration);
        }
    }
    {
        var min = start.Add(delayadj); if (end.Before(min)) {
            Ꮡt.Fatalf("After(%s) expect >= %s, got %s"u8, delay, min, end);
        }
    }
}

public static void TestAfterTick(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    UntypedInt Count = 10;
    var Delta = 100 * Millisecond;
    if (Δtesting.Short()) {
        Delta = 10 * Millisecond;
    }
    var t0 = Now();
    for (nint i = 0; i < Count; i++) {
        ᐸꟷ(After(Delta));
    }
    var t1 = Now();
    var d = t1.Sub(t0);
    var target = Delta * (int64)Count;
    if (d < target * 9 / 10) {
        Ꮡt.Fatalf("%d ticks of %s too fast: took %s, expected %s"u8, (nint)(Count), Delta, d, target);
    }
    if (!Δtesting.Short() && d > target * 30 / 10) {
        Ꮡt.Fatalf("%d ticks of %s too slow: took %s, expected %s"u8, (nint)(Count), Delta, d, target);
    }
}

public static void TestAfterStop(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Run(implChanˢ, (ж<Δtesting.T> tΔ1) => {
        testAfterStop(tΔ1, NewTimer);
    });
    Ꮡt.Run(implFuncˢ, (ж<Δtesting.T> tΔ2) => {
        testAfterStop(tΔ2, newTimerFunc);
    });
}

internal static void testAfterStop(ж<Δtesting.T> Ꮡt, Func<Δtime.Duration, ж<Δtime.Timer>> newTimer) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We want to test that we stop a timer before it runs.
    // We also want to test that it didn't run after a longer timer.
    // Since we don't want the test to run for too long, we don't
    // want to use lengthy times. That makes the test inherently flaky.
    // So only report an error if it fails five times in a row.
    ref var errs = ref heap<slice<@string>>(out var Ꮡerrs);
    void logErrs() {
        foreach (var (_, e) in Ꮡerrs.ValueSlot) {
            Ꮡt.Log(e);
        }
    }
    for (nint i = 0; i < 5; i++) {
        AfterFunc(100 * Millisecond, () => {
        });
        var t0 = newTimer(50 * Millisecond);
        var c1 = new channel<bool>(1);
        var c1ʗ1 = c1;
        var t1 = AfterFunc(150 * Millisecond, () => {
            c1ʗ1.ᐸꟷ(true);
        });
        var c2 = After(200 * Millisecond);
        if (!t0.Stop()) {
            errs = append(errs, "failed to stop event 0"u8);
            continue;
        }
        if (!t1.Stop()) {
            errs = append(errs, "failed to stop event 1"u8);
            continue;
        }
        ᐸꟷ(c2);
        var selᴛ5 = (~t0).C;
        var selᴛ6 = c1;
        switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
        case 0 when selᴛ5.ꟷᐳ(out _): {
            errs = append(errs, "event 0 was not stopped"u8);
            continue;
            break;
        }
        case 1 when selᴛ6.ꟷᐳ(out _): {
            errs = append(errs, "event 1 was not stopped"u8);
            continue;
            break;
        }
        default: {
            break;
        }}
        if (t1.Stop()) {
            errs = append(errs, "Stop returned true twice"u8);
            continue;
        }
        // Test passed, so all done.
        if (len(errs) > 0) {
            Ꮡt.Logf("saw %d errors, ignoring to avoid flakiness"u8, len(errs));
            logErrs();
        }
        return;
    }
    Ꮡt.Errorf("saw %d errors"u8, len(errs));
    logErrs();
}

public static void TestAfterQueuing(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Run(implChanˢ, (ж<Δtesting.T> tΔ1) => {
        testAfterQueuing(tΔ1, After);
    });
    Ꮡt.Run(implFuncˢ, (ж<Δtesting.T> tΔ2) => {
        testAfterQueuing(tΔ2, (Δtime.Duration d) => (~newTimerFunc(d)).C);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "!=nil"u8;

internal static void testAfterQueuing(ж<Δtesting.T> Ꮡt, Func<Δtime.Duration, /*<-*/channel<Δtime.Time>> after) {
    // This test flakes out on some systems,
    // so we'll try it a few times before declaring it a failure.
    const nint attempts = 5;
    var err = errors.New(nilˢ);
    for (nint i = 0; i < attempts && err != default!; i++) {
        var delta = ((Δtime.Duration)(int64)(20 + i * 50)) * Millisecond;
        {
            err = testAfterQueuing1(delta, after); if (err != default!) {
                Ꮡt.Logf("attempt %v failed: %v"u8, i, err);
            }
        }
    }
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

internal static slice<nint> slots = new nint[]{5, 3, 6, 6, 6, 1, 1, 2, 7, 9, 4, 8, 0}.slice();

partial struct afterResult {
    internal nint slot;
    internal Δtime.Time t;
}

internal static void await(nint slot, channel/*<-*/<afterResult> result, /*<-*/channel<Δtime.Time> ac) {
    result.ᐸꟷ(new afterResult(slot, ᐸꟷ(ac)));
}

internal static partial error testAfterQueuing1(Δtime.Duration delta, Func<Δtime.Duration, /*<-*/channel<Δtime.Time>> after) {
    // make the result channel buffered because we don't want
    // to depend on channel queuing semantics that might
    // possibly change in the future.
    var result = new channel<afterResult>(len(slots));
    var t0 = Now();
    foreach (var (_, slot) in slots) {
        goǃ(await, slot, result.WithDirection(GoChanDir.Send), After(((Δtime.Duration)(int64)slot) * delta));
    }
    slice<nint> order = default!;
    slice<Δtime.Time> times = default!;
    foreach ((_, _) in slots) {
        var r = ᐸꟷ(result);
        order = append(order, r.slot);
        times = append(times, r.t);
    }
    foreach (var (i, _) in order) {
        if (i > 0 && order[i] < order[i - 1]) {
            return fmt.Errorf("After calls returned out of order: %v"u8, order);
        }
    }
    foreach (var (i, t) in times) {
        var dt = t.Sub(t0);
        var target = ((Δtime.Duration)(int64)order[i]) * delta;
        if (dt < target - delta / 2 || dt > target + delta * 10) {
            return fmt.Errorf("After(%s) arrived at %s, expected [%s,%s]"u8, target, dt, target - delta / 2, target + delta * 10);
        }
    }
    return default!;
}

public static partial void TestTimerStopStress(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        return;
    }
    Ꮡt.Parallel();
    for (nint i = 0; i < 100; i++) {
        goǃ((nint iΔ1) => {
            var timer = AfterFunc(2 * ΔSecond, () => {
                Ꮡt.Errorf("timer %d was not stopped"u8, iΔ1);
            });
            Sleep(1 * ΔSecond);
            timer.Stop();
        }, i);
    }
    Sleep((Δtime.Duration)(3000000000L));
}

public static partial void TestSleepZeroDeadlock(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Sleep(0) used to hang, the sequence of events was as follows.
        // Sleep(0) sets G's status to Gwaiting, but then immediately returns leaving the status.
        // Then the goroutine calls e.g. new and falls down into the scheduler due to pending GC.
        // After the GC nobody wakes up the goroutine from Gwaiting status.
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(4), ref ᒐ);
        var c = new channel<bool>(0);
        var cʗ1 = c;
        goǃ(() => {
            for (nint i = 0; i < 100; i++) {
                Δruntime.GC();
            }
            cʗ1.ᐸꟷ(true);
        });
        for (nint i = 0; i < 100; i++) {
            Sleep(0);
            var tmp = new channel<bool>(1);
            tmp.ᐸꟷ(true);
            ᐸꟷ(tmp);
        }
        ᐸꟷ(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string resettingUnfiredTimerˢ = "resetting unfired timer returned false"u8;
internal static readonly @string timerFiredEarlyˢ = "timer fired early"u8;
internal static readonly @string resetTimerDidNotFireˢ = "reset timer did not fire"u8;
internal static readonly @string resettingExpiredTimerˢ = "resetting expired timer returned true"u8;

internal static error testReset(Δtime.Duration d) {
    var t0 = NewTimer(2 * d);
    Sleep(d);
    if (!t0.Reset(3 * d)) {
        return errors.New(resettingUnfiredTimerˢ);
    }
    Sleep(2 * d);
    var selᴛ7 = (~t0).C;
    switch (trySelect(ᐸꟷ(selᴛ7, ꓸꓸꓸ))) {
    case 0 when selᴛ7.ꟷᐳ(out _): {
        return errors.New(timerFiredEarlyˢ);
    }
    default: {
        break;
    }}
    Sleep(2 * d);
    var selᴛ8 = (~t0).C;
    switch (trySelect(ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
    case 0 when selᴛ8.ꟷᐳ(out _): {
        break;
    }
    default: {
        return errors.New(resetTimerDidNotFireˢ);
    }}
    if (t0.Reset(50 * Millisecond)) {
        return errors.New(resettingExpiredTimerˢ);
    }
    return default!;
}

public static void TestReset(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We try to run this test with increasingly larger multiples
    // until one works so slow, loaded hardware isn't as flaky,
    // but without slowing down fast machines unnecessarily.
    //
    // (maxDuration is several orders of magnitude longer than we
    // expect this test to actually take on a fast, unloaded machine.)
    var d = 1 * Millisecond;
    Δtime.Duration maxDuration = /* 10 * Second */ 10000000000;
    while (ᐧ) {
        var err = testReset(d);
        if (err == default!) {
            break;
        }
        d *= 2;
        if (d > maxDuration) {
            Ꮡt.Error(err);
        }
        Ꮡt.Logf("%v; trying duration %v"u8, err, d);
    }
}

// Test that sleeping (via Sleep or Timer) for an interval so large it
// overflows does not result in a short sleep duration. Nor does it interfere
// with execution of other timers. If it does, timers in this or subsequent
// tests may not fire.
public static partial void TestOverflowSleep(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Δtime.Duration big = /* Duration(int64(1<<63 - 1)) */ 9223372036854775807;
    goǃ(() => {
        Sleep(big);
        // On failure, this may return after the test has completed, so
        // we need to panic instead.
        throw panic("big sleep returned");
    });
    var selᴛ9 = After(big);
    var selᴛ10 = After(25 * Millisecond);
    switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
    case 0 when selᴛ9.ꟷᐳ(out _): {
        Ꮡt.Fatalf("big timeout fired"u8);
        break;
    }
    case 1 when selᴛ10.ꟷᐳ(out _): {
        break;
    }}
    // OK
    Δtime.Duration neg = /* Duration(-1 << 63) */ -9223372036854775808;
    Sleep(neg); // Returns immediately.
    var selᴛ11 = After(neg);
    var selᴛ12 = After(1 * ΔSecond);
    switch (select(ᐸꟷ(selᴛ11, ꓸꓸꓸ), ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
    case 0 when selᴛ11.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ12.ꟷᐳ(out _): {
        Ꮡt.Fatalf("negative timeout didn't fire"u8);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedPanicButNoneˢ = (@string)"Expected panic, but none happened."u8;
internal static readonly object shouldBeUnreachableˢ = (@string)"Should be unreachable."u8;

// OK

// Test that a panic while deleting a timer does not leave
// the timers mutex held, deadlocking a ticker.Stop in a defer.
public static void TestIssue5745(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ticker = NewTicker(ΔHour);
        var tickerʗ1 = ticker;
        defer(() => {
            // would deadlock here before the fix due to
            // lock taken before the segfault.
            tickerʗ1.Stop();
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Error(expectedPanicButNoneˢ);
                }
            }
        }, ref ᒐ);
        // cause a panic due to a segfault
        ж<Δtime.Timer> timer = default!;
        timer.Stop();
        Ꮡt.Error(shouldBeUnreachableˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestOverflowPeriodRuntimeTimer(ж<Δtesting.T> Ꮡt) {
    // This may hang forever if timers are broken. See comment near
    // the end of CheckRuntimeTimerOverflow in internal_test.go.
    time_internal_test_package.CheckRuntimeTimerPeriodOverflow();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string calledOnUninitializedˢ = "called on uninitialized Timer"u8;

internal static void checkZeroPanicString(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var e = recover();
        var (s, _) = e._<@string>(ᐧ);
        {
            @string want = calledOnUninitializedˢ; if (!strings.Contains(s, want)) {
                Ꮡt.Errorf("panic = %v; want substring %q"u8, e, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestZeroTimerResetPanics(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(checkZeroPanicString, Ꮡt, ref ᒐ);
        ref var tr = ref heap(new Δtime.Timer(), out var Ꮡtr);
        Ꮡtr.Reset(1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestZeroTimerStopPanics(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(checkZeroPanicString, Ꮡt, ref ᒐ);
        ref var tr = ref heap(new Δtime.Timer(), out var Ꮡtr);
        Ꮡtr.Stop();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string implCacheˢ = "impl=cache"u8;

// Test that zero duration timers aren't missed by the scheduler. Regression test for issue 44868.
public static void TestZeroTimer(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Run(implChanˢ, (ж<Δtesting.T> tΔ1) => {
        testZeroTimer(tΔ1, NewTimer);
    });
    Ꮡt.Run(implFuncˢ, (ж<Δtesting.T> tΔ2) => {
        testZeroTimer(tΔ2, newTimerFunc);
    });
    Ꮡt.Run(implCacheˢ, (ж<Δtesting.T> tΔ3) => {
        var timer = newTimerFunc(ΔHour);
        var timerʗ1 = timer;
        testZeroTimer(tΔ3, (Δtime.Duration d) => {
            timerʗ1.Reset(d);
            return timerʗ1;
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shortˢ = (@string)"-short"u8;

internal static void testZeroTimer(ж<Δtesting.T> Ꮡt, Func<Δtime.Duration, ж<Δtime.Timer>> newTimer) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(shortˢ);
    }
    for (nint i = 0; i < 1000000; i++) {
        var s = Now();
        var ti = newTimer(0);
        ᐸꟷ((~ti).C);
        {
            var diff = Since(s); if (diff > 2 * ΔSecond) {
                Ꮡt.Errorf("Expected time to get value from Timer channel in less than 2 sec, took %v"u8, diff);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deadlineExpiredˢ = (@string)"deadline expired"u8;

// Test that rapidly moving a timer earlier doesn't cause it to get dropped.
// Issue 47329.
public static void TestTimerModifiedEarlier(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "plan9"u8 && Δruntime.GOARCH == "arm"u8) {
            testenv.SkipFlaky(new time_test_package.testing_TжTB(Ꮡt), 50470);
        }
        var past = Until(Unix(0, 0));
        nint count = 1000;
        nint fail = 0;
        for (nint i = 0; i < count; i++) {
            var timer = newTimerFunc(ΔHour);
            for (nint j = 0; j < 10; j++) {
                if (!timer.Stop()) {
                    ᐸꟷ((~timer).C);
                }
                timer.Reset(past);
            }
            var deadline = NewTimer((Δtime.Duration)(10000000000L));
            var deadlineʗ1 = deadline;
            defer(() => deadlineʗ1.Stop(), ref ᒐ);
            var now = Now();
            var selᴛ13 = (~timer).C;
            var selᴛ14 = (~deadline).C;
            switch (select(ᐸꟷ(selᴛ13, ꓸꓸꓸ), ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
            case 0 when selᴛ13.ꟷᐳ(out _): {
                {
                    var since = Since(now); if (since > (Δtime.Duration)(8000000000L)) {
                        Ꮡt.Errorf("timer took too long (%v)"u8, since);
                        fail++;
                    }
                }
                break;
            }
            case 1 when selᴛ14.ꟷᐳ(out _): {
                Ꮡt.Error(deadlineExpiredˢ);
                break;
            }}
        }
        if (fail > 0) {
            Ꮡt.Errorf("%d failures"u8, fail);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timerResetReturnedTrueˢ = (@string)"timer.Reset returned true"u8;

// Test that rapidly moving timers earlier and later doesn't cause
// some of the sleep times to be lost.
// Issue 47762
public static void TestAdjustTimers(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<rand.Rand> rnd = rand.New(rand.NewSource(Now().UnixNano()));
    var timers = new slice<ж<Δtime.Timer>>(100);
    var states = new slice<nint>(len(timers));
    var indices = rnd.Perm(len(timers));
    while (len(indices) != 0) {
        nint ii = rnd.Intn(len(indices));
        nint i = indices[ii];
        ж<Δtime.Timer> timer = timers[i];
        nint state = states[i];
        states[i]++;
        switch (state) {
        case 0: {
            timers[i] = newTimerFunc(0);
            break;
        }
        case 1: {
            ᐸꟷ((~timer).C); // Timer is now idle.
            break;
        }
        case 2: {
            if (timer.Reset((Δtime.Duration)(60000000000L))) {
                // Reset to various long durations, which we'll cancel.
                throw panic("shouldn't be active (1)");
            }
            break;
        }
        case 4: {
            if (timer.Reset((Δtime.Duration)(180000000000L))) {
                throw panic("shouldn't be active (3)");
            }
            break;
        }
        case 6: {
            if (timer.Reset((Δtime.Duration)(120000000000L))) {
                throw panic("shouldn't be active (2)");
            }
            break;
        }
        case 3 or 5 or 7: {
            if (!timer.Stop()) {
                // Stop and drain a long-duration timer.
                Ꮡt.Logf("timer %d state %d Stop returned false"u8, i, state);
                ᐸꟷ((~timer).C);
            }
            break;
        }
        case 8: {
            if (timer.Reset(0)) {
                // Start a short-duration timer we expect to select without blocking.
                Ꮡt.Fatal(timerResetReturnedTrueˢ);
            }
            break;
        }
        case 9: {
            var now = Now();
            ᐸꟷ((~timer).C);
            var dur = Since(now);
            if (dur > 750 * Millisecond) {
                Ꮡt.Errorf("timer %d took %v to complete"u8, i, dur);
            }
            break;
        }
        case 10: {
            indices[ii] = indices[len(indices) - 1];
            indices = indices.slice(0, len(indices) - 1);
            break;
        }}

    }
}

// Timer is done. Swap with tail and remove.
public static void TestStopResult(ж<Δtesting.T> Ꮡt) {
    testStopResetResult(Ꮡt, true);
}

public static void TestResetResult(ж<Δtesting.T> Ꮡt) {
    testStopResetResult(Ꮡt, false);
}

// Test that when racing between running a timer and stopping a timer Stop
// consistently indicates whether a value can be read from the channel.
// Issue #69312.
internal static void testStopResetResult(ж<Δtesting.T> Ꮡt, bool testStop) {
    foreach (var (_, name) in new @string[]{"0"u8, "1"u8, "2"u8}.slice()) {
        Ꮡt.Run("asynctimerchan="u8 + name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.T> tΔ1) => {
            testStopResetResultGODEBUG(tΔ1, testStop, name);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;

internal static partial void testStopResetResultGODEBUG(ж<Δtesting.T> Ꮡt, bool testStop, @string godebug) {
    Ꮡt.Setenv(godebugˢ, "asynctimerchan="u8 + godebug);
    bool stopOrReset(ж<Δtime.Timer> timer) {
        if (testStop){
            return timer.Stop();
        } else {
            return timer.Reset((Δtime.Duration)(3600000000000L));
        }
    }
    var start = new channel<EmptyStruct>(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    const nint N = 1000;
    Ꮡwg.Add(N);
    foreach (var _ᴛ1 in range(N)) {
        var startʗ1 = start;
        var stopOrResetʗ1 = stopOrReset;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ᐸꟷ(startʗ1);
                for (nint j = 0; j < 100; j++) {
                    var timer1 = NewTimer(1 * Millisecond);
                    var timer2 = NewTimer(1 * Millisecond);
                    var selᴛ15 = (~timer1).C;
                    var selᴛ16 = (~timer2).C;
                    switch (select(ᐸꟷ(selᴛ15, ꓸꓸꓸ), ᐸꟷ(selᴛ16, ꓸꓸꓸ))) {
                    case 0 when selᴛ15.ꟷᐳ(out _): {
                        if (!stopOrResetʗ1(timer2)) {
                            // The test fails if this
                            // channel read times out.
                            ᐸꟷ((~timer2).C);
                        }
                        break;
                    }
                    case 1 when selᴛ16.ꟷᐳ(out _): {
                        if (!stopOrResetʗ1(timer1)) {
                            // The test fails if this
                            // channel read times out.
                            ᐸꟷ((~timer1).C);
                        }
                        break;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    close(start);
    Ꮡwg.Wait();
}

// Test having a large number of goroutines wake up a ticker simultaneously.
// This used to trigger a crash when run under x/tools/cmd/stress.
public static partial void TestMultiWakeupTicker(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(shortˢ);
    }
    nint goroutines = Δruntime.GOMAXPROCS(0);
    var timer = NewTicker(Microsecond);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(goroutines);
    foreach (var _ᴛ1 in range(goroutines)) {
        var timerʗ1 = timer;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                foreach (var _ᴛ2 in range(100000)) {
                    var selᴛ17 = (~timerʗ1).C;
                    var selᴛ18 = After(Millisecond);
                    switch (select(ᐸꟷ(selᴛ17, ꓸꓸꓸ), ᐸꟷ(selᴛ18, ꓸꓸꓸ))) {
                    case 0 when selᴛ17.ꟷᐳ(out _): {
                        break;
                    }
                    case 1 when selᴛ18.ꟷᐳ(out _): {
                        break;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Test having a large number of goroutines wake up a timer simultaneously.
// This used to trigger a crash when run under x/tools/cmd/stress.
public static partial void TestMultiWakeupTimer(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(shortˢ);
    }
    nint goroutines = Δruntime.GOMAXPROCS(0);
    var timer = NewTimer(ΔNanosecond);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(goroutines);
    foreach (var _ᴛ1 in range(goroutines)) {
        var timerʗ1 = timer;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                foreach (var _ᴛ2 in range(10000)) {
                    var selᴛ19 = (~timerʗ1).C;
                    switch (trySelect(ᐸꟷ(selᴛ19, ꓸꓸꓸ))) {
                    case 0 when selᴛ19.ꟷᐳ(out _): {
                        break;
                    }
                    default: {
                        break;
                    }}
                    timerʗ1.Reset(ΔNanosecond);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWithGomaxprocs2ˢ = (@string)"skipping with GOMAXPROCS < 2 or NumCPU < GOMAXPROCS"u8;
internal static readonly @string nsOpˢ = "ns/op"u8;
internal static readonly @string avgLateNsˢ = "avg-late-ns"u8;
internal static readonly @string maxLateNsˢ = "max-late-ns"u8;

internal partial struct BenchmarkParallelTimerLatency_type /*dyn*/ {
    internal float64 sum;
    internal Δtime.Duration max;
    internal int64 count;
    internal array<int64> _ = new(5); // cache line padding
}

// Benchmark timer latency when the thread that creates the timer is busy with
// other work and the timers must be serviced by other threads.
// https://golang.org/issue/38860
public static void BenchmarkParallelTimerLatency(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint gmp = Δruntime.GOMAXPROCS(0);
    if (gmp < 2 || Δruntime.NumCPU() < gmp) {
        Ꮡb.Skip(skippingWithGomaxprocs2ˢ);
    }
    // allocate memory now to avoid GC interference later.
    nint timerCount = gmp - 1;
    var stats = new slice<BenchmarkParallelTimerLatency_type>(timerCount);
    // Ensure the time to start new threads to service timers will not pollute
    // the results.
    warmupScheduler(gmp);
    // Note that other than the AfterFunc calls this benchmark is measuring it
    // avoids using any other timers. In particular, the main goroutine uses
    // doWork to spin for some durations because up through Go 1.15 if all
    // threads are idle sysmon could leave deep sleep when we wake.
    // Ensure sysmon is in deep sleep.
    doWork(30 * Millisecond);
    b.ResetTimer();
    Δtime.Duration delay = /* Millisecond */ 1000000;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    ref var count = ref heap(new int32(), out var Ꮡcount);
    for (nint i = 0; i < b.N; i++) {
        Ꮡwg.Add(timerCount);
        atomic.StoreInt32(Ꮡcount, 0);
        for (nint j = 0; j < timerCount; j++) {
            nint jΔ1 = j;
            ref var expectedWakeup = ref heap<Δtime.Time>(out var ᏑexpectedWakeup);
            expectedWakeup = Now().Add(delay);
            var expectedWakeupʗ1 = expectedWakeup;
            var statsʗ1 = stats;
            AfterFunc(delay, () => {
                var late = Since(expectedWakeupʗ1);
                if (late < 0) {
                    late = 0;
                }
                statsʗ1[jΔ1].count++;
                statsʗ1[jΔ1].sum += (float64)late.Nanoseconds();
                if (late > statsʗ1[jΔ1].max) {
                    statsʗ1[jΔ1].max = late;
                }
                atomic.AddInt32(Ꮡcount, 1);
                while (atomic.LoadInt32(Ꮡcount) < (int32)timerCount) {
                }
                // spin until all timers fired
                Ꮡwg.Done();
            });
        }
        while (atomic.LoadInt32(Ꮡcount) < (int32)timerCount) {
        }
        // spin until all timers fired
        Ꮡwg.Wait();
        // Spin for a bit to let the other scheduler threads go idle before the
        // next round.
        doWork(Millisecond);
    }
    float64 total = default!;
    float64 samples = default!;
    var max = ((Δtime.Duration)0);
    foreach (var (_, s) in stats) {
        if (s.max > max) {
            max = s.max;
        }
        total += s.sum;
        samples += (float64)s.count;
    }
    b.ReportMetric(0D, nsOpˢ);
    b.ReportMetric(total / samples, avgLateNsˢ);
    b.ReportMetric((float64)max.Nanoseconds(), maxLateNsˢ);
}

// Benchmark timer latency with staggered wakeup times and varying CPU bound
// workloads. https://golang.org/issue/38860
public static void BenchmarkStaggeredTickerLatency(ж<Δtesting.B> Ꮡb) {
    nint gmp = Δruntime.GOMAXPROCS(0);
    if (gmp < 2 || Δruntime.NumCPU() < gmp) {
        Ꮡb.Skip(skippingWithGomaxprocs2ˢ);
    }
    Δtime.Duration delay = /* 3 * Millisecond */ 3000000;
    foreach (var (_, dur) in new Δtime.Duration[]{300 * Microsecond, 2 * Millisecond}.slice()) {
        Ꮡb.Run(fmt.Sprintf("work-dur=%s"u8, dur), (ж<Δtesting.B> bΔ1) => {
            for (nint tickersPerP = 1; tickersPerP < (nint)(int64)(delay / dur) + 1; tickersPerP++) {
                nint tickerCount = gmp * tickersPerP;
                bΔ1.Run(fmt.Sprintf("tickers-per-P=%d"u8, tickersPerP), [MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.B> bΔ2) => {
                    // allocate memory now to avoid GC interference later.
                    var stats = new slice<BenchmarkParallelTimerLatency_type>(tickerCount);
                    // Ensure the time to start new threads to service timers
                    // will not pollute the results.
                    warmupScheduler(gmp);
                    bΔ2.ResetTimer();
                    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                    Ꮡwg.Add(tickerCount);
                    for (nint j = 0; j < tickerCount; j++) {
                        nint jΔ1 = j;
                        doWork(delay / ((Δtime.Duration)(int64)gmp));
                        ref var expectedWakeup = ref heap<Δtime.Time>(out var ᏑexpectedWakeup);
                        ᏑexpectedWakeup.Value = Now().Add(delay);
                        var ticker = NewTicker(delay);
                        var statsʗ1 = stats;
                        goǃ((nint c, ж<Δtime.Ticker> tickerΔ1, Δtime.Time firstWake) => {
                            GoFrame ᒐ = default;
                            try {
                                defer(tickerΔ1.Stop, ref ᒐ);
                                for (; c > 0; c--) {
                                    ᐸꟷ((~tickerΔ1).C);
                                    var late = Since(ᏑexpectedWakeup.Value);
                                    if (late < 0) {
                                        late = 0;
                                    }
                                    statsʗ1[jΔ1].count++;
                                    statsʗ1[jΔ1].sum += (float64)late.Nanoseconds();
                                    if (late > statsʗ1[jΔ1].max) {
                                        statsʗ1[jΔ1].max = late;
                                    }
                                    ᏑexpectedWakeup.Value = ᏑexpectedWakeup.Value.Add(delay);
                                    doWork(dur);
                                }
                                Ꮡwg.Done();
                            }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ.Run(); }
                        }, (~bΔ2).N, ticker, ᏑexpectedWakeup.Value);
                    }
                    Ꮡwg.Wait();
                    float64 total = default!;
                    float64 samples = default!;
                    var max = ((Δtime.Duration)0);
                    foreach (var (_, s) in stats) {
                        if (s.max > max) {
                            max = s.max;
                        }
                        total += s.sum;
                        samples += (float64)s.count;
                    }
                    bΔ2.ReportMetric(0D, nsOpˢ);
                    bΔ2.ReportMetric(total / samples, avgLateNsˢ);
                    bΔ2.ReportMetric((float64)max.Nanoseconds(), maxLateNsˢ);
                });
            }
        });
    }
}

// warmupScheduler ensures the scheduler has at least targetThreadCount threads
// in its thread pool.
internal static partial void warmupScheduler(nint targetThreadCount) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    ref var count = ref heap(new int32(), out var Ꮡcount);
    for (nint i = 0; i < targetThreadCount; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            atomic.AddInt32(Ꮡcount, 1);
            while (atomic.LoadInt32(Ꮡcount) < (int32)targetThreadCount) {
            }
            // spin until all threads started
            // spin a bit more to ensure they are all running on separate CPUs.
            doWork(Millisecond);
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

internal static void doWork(Δtime.Duration dur) {
    var start = Now();
    while (Since(start) < dur) {
    }
}

public static void BenchmarkAdjustTimers10000(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            UntypedInt n = 10000;
            var timers = new slice<ж<Δtime.Timer>>(0, n);
            foreach (var _ᴛ1 in range(n)) {
                var t = AfterFunc(ΔHour, () => {
                });
                timers = append(timers, t);
            }
            timers[n - 1].Reset(ΔNanosecond);
            Sleep(Microsecond);
            foreach (var (_, t) in timers) {
                t.Stop();
            }
        }
    });
}

} // end time_test_package
