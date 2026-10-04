// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using Δtesting = testing_package;
using static time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.time_internal_test_package;
using Δtime = time_package;

partial class time_test_package {

[GoType("dyn")] internal partial struct TestTicker_type {
    internal nint count;
    internal Δtime.Duration delta;
}

public static void TestTicker(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    // We want to test that a ticker takes as much time as expected.
    // Since we don't want the test to run for too long, we don't
    // want to use lengthy times. This makes the test inherently flaky.
    // Start with a short time, but try again with a long one if the
    // first test fails.
    nint baseCount = 10;
    var baseDelta = 20 * Millisecond;
    // On Darwin ARM64 the tick frequency seems limited. Issue 35692.
    if ((Δruntime.GOOS == "darwin"u8 || Δruntime.GOOS == "ios"u8) && Δruntime.GOARCH == "arm64"u8) {
        // The following test will run ticker count/2 times then reset
        // the ticker to double the duration for the rest of count/2.
        // Since tick frequency is limited on Darwin ARM64, use even
        // number to give the ticks more time to let the test pass.
        // See CL 220638.
        baseCount = 6;
        baseDelta = 100 * Millisecond;
    }
    ref var errs = ref heap<slice<@string>>(out var Ꮡerrs);
    void logErrs() {
        foreach (var (_, e) in Ꮡerrs.ValueSlot) {
            Ꮡt.Log(e);
        }
    }
    foreach (var (_, test) in new TestTicker_type[]{new(
        count: baseCount,
        delta: baseDelta
    ), new(
        count: 8,
        delta: 1 * ΔSecond
    )
    }.slice()) {
        nint count = test.count;
        var delta = test.delta;
        var ticker = NewTicker(delta);
        var t0 = Now();
        foreach (var _ᴛ1 in range(count / 2)) {
            ᐸꟷ((~ticker).C);
        }
        ticker.Reset(delta * 2);
        foreach (var _ᴛ2 in range(count - count / 2)) {
            ᐸꟷ((~ticker).C);
        }
        ticker.Stop();
        var t1 = Now();
        var dt = t1.Sub(t0);
        var target = 3 * delta * ((Δtime.Duration)(int64)(count / 2));
        var slop = target * 3 / 10;
        if (dt < target - slop || dt > target + slop) {
            errs = append(errs, fmt.Sprintf("%d %s ticks then %d %s ticks took %s, expected [%s,%s]"u8, count / 2, delta, count / 2, delta * 2, dt, target - slop, target + slop));
            if (dt > target + slop) {
                // System may be overloaded; sleep a bit
                // in the hopes it will recover.
                Sleep(ΔSecond / 2);
            }
            continue;
        }
        // Now test that the ticker stopped.
        Sleep(2 * delta);
        var selᴛ20 = (~ticker).C;
        switch (trySelect(ᐸꟷ(selᴛ20, ꓸꓸꓸ))) {
        case 0 when selᴛ20.ꟷᐳ(out _): {
            errs = append(errs, "Ticker did not shut down"u8);
            continue;
            break;
        }
        default: {
            break;
        }}
        // ok
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

// Issue 21874
public static void TestTickerStopWithDirectInitialization(ж<Δtesting.T> Ꮡt) {
    var c = new channel<Δtime.Time>(0);
    var tk = Ꮡ(new Ticker(C: c));
    tk.Stop();
}

// Test that a bug tearing down a ticker has been fixed. This routine should not deadlock.
public static void TestTeardown(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    var Delta = 100 * Millisecond;
    if (Δtesting.Short()) {
        Delta = 20 * Millisecond;
    }
    foreach (var _ᴛ1 in range(3)) {
        var ticker = NewTicker(Delta);
        ᐸꟷ((~ticker).C);
        ticker.Stop();
    }
}

// Test the Tick convenience wrapper.
public static void TestTick(ж<Δtesting.T> Ꮡt) {
    // Test that giving a negative duration returns nil.
    {
        var got = Tick(-1); if (got != default!) {
            Ꮡt.Errorf("Tick(-1) = %v; want nil"u8, got);
        }
    }
}

// Test that NewTicker panics when given a duration less than zero.
public static void TestNewTickerLtZeroDuration(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var err = recover(); if (err == default!) {
                    Ꮡt.Errorf("NewTicker(-1) should have panicked"u8);
                }
            }
        }, ref ᒐ);
        NewTicker(-1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that Ticker.Reset panics when given a duration less than zero.
public static void TestTickerResetLtZeroDuration(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var err = recover(); if (err == default!) {
                    Ꮡt.Errorf("Ticker.Reset(0) should have panicked"u8);
                }
            }
        }, ref ᒐ);
        var tk = NewTicker(ΔSecond);
        tk.Reset(0);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object outputChannelIsClosedˢ = (@string)"output channel is closed"u8;
internal static readonly object timerExpiredˢ = (@string)"timer expired"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestLongAdjustTimers(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "android"u8 || Δruntime.GOOS == "ios"u8) {
            Ꮡt.Skipf("skipping on %s - too slow"u8, Δruntime.GOOS);
        }
        Ꮡt.Parallel();
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        // Build up the timer heap.
        const nint count = 5000;
        Ꮡwg.Add(count);
        foreach (var _ᴛ1 in range(count)) {
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    Sleep(10 * Microsecond);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        foreach (var _ᴛ2 in range(count)) {
            Sleep(1 * Microsecond);
        }
        // Give ourselves 60 seconds to complete.
        // This used to reliably fail on a Mac M3 laptop,
        // which needed 77 seconds.
        // Trybots are slower, so it will fail even more reliably there.
        // With the fix, the code runs in under a second.
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        AfterFunc((Δtime.Duration)(60000000000L), () => {
            close(doneʗ1);
        });
        // Set up a queuing goroutine to ping pong through the scheduler.
        var inQ = new channel<Action>(0);
        var outQ = new channel<Action>(0);
        defer(ᴛ1 => close(ᴛ1), inQ, ref ᒐ);
        Ꮡwg.Add(1);
        var doneʗ2 = done;
        var inQʗ1 = inQ;
        var outQʗ1 = outQ;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                defer(ᴛ1 => close(ᴛ1), outQʗ1, ref ᒐ);
                slice<Action> q = default!;
                while (ᐧ) {
                    channel<Action> sendTo = default!;
                    Action send = default!;
                    if (len(q) > 0) {
                        sendTo = outQʗ1;
                        send = q[0];
                    }
                    var selᴛ21 = sendTo.ᐸꟷ(send, ꓸꓸꓸ);
                    var selᴛ22 = inQʗ1;
                    var selᴛ23 = doneʗ2;
                    switch (select(selᴛ21, ᐸꟷ(selᴛ22, ꓸꓸꓸ), ᐸꟷ(selᴛ23, ꓸꓸꓸ))) {
                    case 0: {
                        q = q[1..];
                        break;
                    }
                    case 1 when selᴛ22.ꟷᐳ(out var f, out var ok): {
                        if (!ok) {
                            return;
                        }
                        q = append(q, f);
                        break;
                    }
                    case 2 when selᴛ23.ꟷᐳ(out _): {
                        return;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        foreach (var i in range(50000)) {
            const nint @try = 20;
            foreach (var _ᴛ3 in range(@try)) {
                inQ.ᐸꟷ(() => {
                });
            }
            foreach (var _ᴛ4 in range(@try)) {
                var selᴛ24 = outQ;
                var selᴛ25 = After((Δtime.Duration)(5000000000L));
                var selᴛ26 = done;
                switch (select(ᐸꟷ(selᴛ24, ꓸꓸꓸ), ᐸꟷ(selᴛ25, ꓸꓸꓸ), ᐸꟷ(selᴛ26, ꓸꓸꓸ))) {
                case 0 when selᴛ24.ꟷᐳ(out var _, out var ok): {
                    if (!ok) {
                        Ꮡt.Fatal(outputChannelIsClosedˢ);
                    }
                    break;
                }
                case 1 when selᴛ25.ꟷᐳ(out _): {
                    Ꮡt.Fatalf("failed to read work, iteration %d"u8, i);
                    break;
                }
                case 2 when selᴛ26.ꟷᐳ(out _): {
                    Ꮡt.Fatal(timerExpiredˢ);
                    break;
                }}
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkTicker(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        var ticker = NewTicker(ΔNanosecond);
        while (pb.Next()) {
            ᐸꟷ((~ticker).C);
        }
        ticker.Stop();
    });
}

public static void BenchmarkTickerReset(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        var ticker = NewTicker(ΔNanosecond);
        while (pb.Next()) {
            ticker.Reset(ΔNanosecond * 2);
        }
        ticker.Stop();
    });
}

public static void BenchmarkTickerResetNaive(ж<Δtesting.B> Ꮡb) {
    benchmark(Ꮡb, (ж<Δtesting.PB> pb) => {
        var ticker = NewTicker(ΔNanosecond);
        while (pb.Next()) {
            ticker.Stop();
            ticker = NewTicker(ΔNanosecond * 2);
        }
        ticker.Stop();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string afterˢ = "After"u8;
internal static readonly @string tickˢ = "Tick"u8;
internal static readonly @string newTimerˢ = "NewTimer"u8;
internal static readonly @string newTickerˢ = "NewTicker"u8;
internal static readonly @string newTimerStopˢ = "NewTimerStop"u8;
internal static readonly @string newTickerStopˢ = "NewTickerStop"u8;

public static void TestTimerGC(ж<Δtesting.T> Ꮡt) {
    void run(ж<Δtesting.T> tΔ1, @string what, Action f) {
        tΔ1.Helper();
        tΔ1.Run(what, (ж<Δtesting.T> tΔ2) => {
            tΔ2.Helper();
            UntypedFloat N = 1e4;
            ref var stats = ref heap(new Δruntime.MemStats(), out var Ꮡstats);
            Δruntime.GC();
            Δruntime.GC();
            Δruntime.GC();
            Δruntime.ReadMemStats(Ꮡstats);
            var before = (int64)(stats.Mallocs - stats.Frees);
            for (nint j = 0; j < N; j++) {
                f();
            }
            Δruntime.GC();
            Δruntime.GC();
            Δruntime.GC();
            Δruntime.ReadMemStats(Ꮡstats);
            var after = (int64)(stats.Mallocs - stats.Frees);
            // Allow some slack, but inuse >= N means at least 1 allocation per iteration.
            var inuse = after - before;
            if (inuse >= N) {
                tΔ2.Errorf("%s did not get GC'ed: %d allocations"u8, what, inuse);
                Sleep(1 * ΔSecond);
                Δruntime.ReadMemStats(Ꮡstats);
                var afterΔ1 = (int64)(stats.Mallocs - stats.Frees);
                inuse = afterΔ1 - before;
                tΔ2.Errorf("after a sleep: %d allocations"u8, inuse);
            }
        });
    }
    run(Ꮡt, afterˢ, () => {
        After(ΔHour);
    });
    run(Ꮡt, tickˢ, () => {
        Tick(ΔHour);
    });
    run(Ꮡt, newTimerˢ, () => {
        NewTimer(ΔHour);
    });
    run(Ꮡt, newTickerˢ, () => {
        NewTicker(ΔHour);
    });
    run(Ꮡt, newTimerStopˢ, () => {
        NewTimer(ΔHour).Stop();
    });
    run(Ꮡt, newTickerStopˢ, () => {
        NewTicker(ΔHour).Stop();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timerˢ = "Timer"u8;
internal static readonly @string tickerˢ = "Ticker"u8;

public static void TestChan(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, name) in new @string[]{"0"u8, "1"u8, "2"u8}.slice()) {
        Ꮡt.Run("asynctimerchan="u8 + name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Setenv(godebugˢ, "asynctimerchan="u8 + name);
            tΔ1.Run(timerˢ, (ж<Δtesting.T> tΔ2) => {
                var tim = NewTimer((Δtime.Duration)(10000000000000L));
                testTimerChan(tΔ2, new time_test_package.time_Timerжtimer(tim), (~tim).C, name == "0"u8);
            });
            tΔ1.Run(tickerˢ, (ж<Δtesting.T> tΔ3) => {
                var tim = Ꮡ(new tickerTimer(Ticker: NewTicker((Δtime.Duration)(10000000000000L))));
                testTimerChan(tΔ3, new time_test_package.tickerTimerжtimer(tim), (~tim).C, name == "0"u8);
            });
        });
    }
}

[GoType] partial interface timer {
    bool Stop();
    bool Reset(Δtime.Duration _);
}

// tickerTimer is a Timer with Reset and Stop methods that return bools,
// to have the same signatures as Timer.
[GoType] partial struct tickerTimer {
    public partial ref ж<time_package.Ticker> Ticker { get; }
    internal bool stopped;
}

[GoRecv] internal static bool Stop(this ref tickerTimer t) {
    var pending = !t.stopped;
    t.stopped = true;
    t.Ticker.Stop();
    return pending;
}

[GoRecv] internal static bool Reset(this ref tickerTimer t, Δtime.Duration d) {
    var pending = !t.stopped;
    t.stopped = false;
    t.Ticker.Reset(d);
    return pending;
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void testTimerChan(ж<Δtesting.T> Ꮡt, timer tim, /*<-*/channel<Δtime.Time> C, bool synctimerchan) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (_, isTimer) = tim._<ж<Δtime.Timer>>(ᐧ);
    var isTicker = !isTimer;
    // Retry parameters. Enough to deflake even on slow machines.
    // Windows in particular has very coarse timers so we have to
    // wait 10ms just to make a timer go off.
    Δtime.Duration sched = /* 10 * Millisecond */ 10000000;
    
    const nint tries = 100;
    
    const nint drainTries = 5;
    // drain1 removes one potential stale time value
    // from the timer/ticker channel after Reset.
    // When using Go 1.23 sync timers/tickers, draining is never needed
    // (that's the whole point of the sync timer/ticker change).
    var Cʗ1 = C;
    void drain1() {
        foreach (var _ᴛ1 in range(drainTries)) {
            var selᴛ27 = Cʗ1;
            switch (trySelect(ᐸꟷ(selᴛ27, ꓸꓸꓸ))) {
            case 0 when selᴛ27.ꟷᐳ(out _): {
                return;
            }
            default: {
                break;
            }}
            Sleep(sched);
        }
    }
    // drainAsync removes potential stale time values after Stop/Reset.
    // When using Go 1 async timers, draining one or two values
    // may be needed after Reset or Stop (see comments in body for details).
    var drain1ʗ1 = drain1;
    void drainAsync() {
        if (synctimerchan) {
            // sync timers must have the right semantics without draining:
            // there are no stale values.
            return;
        }
        // async timers can send one stale value (then the timer is disabled).
        drain1ʗ1();
        if (isTicker) {
            // async tickers can send two stale values: there may be one
            // sitting in the channel buffer, and there may also be one
            // send racing with the Reset/Stop+drain that arrives after
            // the first drain1 has pulled the value out.
            // This is rare, but it does happen on overloaded builder machines.
            // It can also be reproduced on an M3 MacBook Pro using:
            //
            //	go test -c strings
            //	stress ./strings.test &   # chew up CPU
            //	go test -c -race time
            //	stress -p 48 ./time.test -test.count=10 -test.run=TestChan/asynctimerchan=1/Ticker
            drain1ʗ1();
        }
    }
    var Cʗ2 = C;
    void noTick() {
        Ꮡt.Helper();
        var selᴛ28 = Cʗ2;
        switch (trySelect(ᐸꟷ(selᴛ28, ꓸꓸꓸ))) {
        case 0 when selᴛ28.ꟷᐳ(out _): {
            Ꮡt.Errorf("extra tick"u8);
            break;
        }
        default: {
            break;
        }}
    }
    var Cʗ3 = C;
    void assertTick() {
        Ꮡt.Helper();
        var selᴛ29 = Cʗ3;
        switch (trySelect(ᐸꟷ(selᴛ29, ꓸꓸꓸ))) {
        case 0 when selᴛ29.ꟷᐳ(out _): {
            return;
        }
        default: {
            break;
        }}
        foreach (var _ᴛ2 in range(tries)) {
            Sleep(sched);
            var selᴛ30 = Cʗ3;
            switch (trySelect(ᐸꟷ(selᴛ30, ꓸꓸꓸ))) {
            case 0 when selᴛ30.ꟷᐳ(out _): {
                return;
            }
            default: {
                break;
            }}
        }
        Ꮡt.Errorf("missing tick"u8);
    }
    var Cʗ4 = C;
    void assertLen() {
        Ꮡt.Helper();
        if (synctimerchan) {
            {
                nint nΔ1 = len(Cʗ4); if (nΔ1 != 0) {
                    Ꮡt.Errorf("synctimer has len(C) = %d, want 0 (always)"u8, nΔ1);
                }
            }
            return;
        }
        nint n = default!;
        {
            n = len(Cʗ4); if (n == 1) {
                return;
            }
        }
        foreach (var _ᴛ3 in range(tries)) {
            Sleep(sched);
            {
                n = len(Cʗ4); if (n == 1) {
                    return;
                }
            }
        }
        Ꮡt.Errorf("len(C) = %d, want 1"u8, n);
    }
    // Test simple stop; timer never in heap.
    tim.Stop();
    noTick();
    // Test modify of timer not in heap.
    tim.Reset((Δtime.Duration)(10000000000000L));
    noTick();
    if (synctimerchan){
        // Test modify of timer in heap.
        tim.Reset(1);
        Sleep(sched);
        {
            nint l = len(C);
            nint c = cap(C); if (l != 0 || c != 0) {
            }
        }
        // t.Fatalf("len(C), cap(C) = %d, %d, want 0, 0", l, c)
        assertTick();
    } else {
        // Test modify of timer in heap.
        tim.Reset(1);
        assertTick();
        Sleep(sched);
        tim.Reset((Δtime.Duration)(10000000000000L));
        drainAsync();
        noTick();
        // Test that len sees an immediate tick arrive
        // for Reset of timer in heap.
        tim.Reset(1);
        assertLen();
        assertTick();
        // Test that len sees an immediate tick arrive
        // for Reset of timer NOT in heap.
        tim.Stop();
        drainAsync();
        tim.Reset(1);
        assertLen();
        assertTick();
    }
    // Sleep long enough that a second tick must happen if this is a ticker.
    // Test that Reset does not lose the tick that should have happened.
    Sleep(sched);
    tim.Reset((Δtime.Duration)(10000000000000L));
    drainAsync();
    noTick();
    void notDone(channel<bool> doneΔ1) {
        Ꮡt.Helper();
        var selᴛ31 = doneΔ1;
        switch (trySelect(ᐸꟷ(selᴛ31, ꓸꓸꓸ))) {
        case 0 when selᴛ31.ꟷᐳ(out _): {
            Ꮡt.Fatalf("early done"u8);
            break;
        }
        default: {
            break;
        }}
    }
    void waitDone(channel<bool> doneΔ2) {
        Ꮡt.Helper();
        foreach (var _ᴛ4 in range(tries)) {
            Sleep(sched);
            var selᴛ32 = doneΔ2;
            switch (trySelect(ᐸꟷ(selᴛ32, ꓸꓸꓸ))) {
            case 0 when selᴛ32.ꟷᐳ(out _): {
                return;
            }
            default: {
                break;
            }}
        }
        Ꮡt.Fatalf("never got done"u8);
    }
    // Reset timer in heap (already reset above, but just in case).
    tim.Reset((Δtime.Duration)(10000000000000L));
    drainAsync();
    // Test stop while timer in heap (because goroutine is blocked on <-C).
    ref var done = ref heap<channel<bool>>(out var Ꮡdone);
    done = new channel<bool>(0);
    notDone(done);
    var Cʗ5 = C;
    goǃ(() => {
        ᐸꟷ(Cʗ5);
        close(Ꮡdone.ValueSlot);
    });
    Sleep(sched);
    notDone(done);
    // Test reset far away while timer in heap.
    tim.Reset((Δtime.Duration)(20000000000000L));
    Sleep(sched);
    notDone(done);
    // Test imminent reset while in heap.
    tim.Reset(1);
    waitDone(done);
    // If this is a ticker, another tick should have come in already
    // (they are 1ns apart). If a timer, it should have stopped.
    if (isTicker){
        assertTick();
    } else {
        noTick();
    }
    tim.Stop();
    drainAsync();
    noTick();
    // Again using select and with two goroutines waiting.
    tim.Reset((Δtime.Duration)(10000000000000L));
    drainAsync();
    done = new channel<bool>(2);
    var done1 = new channel<bool>(0);
    var done2 = new channel<bool>(0);
    ref var stop = ref heap<channel<bool>>(out var Ꮡstop);
    stop = new channel<bool>(0);
    var Cʗ6 = C;
    var done1ʗ1 = done1;
    goǃ(() => {
        var selᴛ33 = Cʗ6;
        var selᴛ34 = Ꮡstop.ValueSlot;
        switch (select(ᐸꟷ(selᴛ33, ꓸꓸꓸ), ᐸꟷ(selᴛ34, ꓸꓸꓸ))) {
        case 0 when selᴛ33.ꟷᐳ(out _): {
            Ꮡdone.ValueSlot.ᐸꟷ(true);
            break;
        }
        case 1 when selᴛ34.ꟷᐳ(out _): {
            break;
        }}
        close(done1ʗ1);
    });
    var Cʗ7 = C;
    var done2ʗ1 = done2;
    goǃ(() => {
        var selᴛ35 = Cʗ7;
        var selᴛ36 = Ꮡstop.ValueSlot;
        switch (select(ᐸꟷ(selᴛ35, ꓸꓸꓸ), ᐸꟷ(selᴛ36, ꓸꓸꓸ))) {
        case 0 when selᴛ35.ꟷᐳ(out _): {
            Ꮡdone.ValueSlot.ᐸꟷ(true);
            break;
        }
        case 1 when selᴛ36.ꟷᐳ(out _): {
            break;
        }}
        close(done2ʗ1);
    });
    Sleep(sched);
    notDone(done);
    tim.Reset(sched / 2);
    Sleep(sched);
    waitDone(done);
    tim.Stop();
    close(stop);
    waitDone(done1);
    waitDone(done2);
    if (isTicker) {
        // extra send might have sent done again
        // (handled by buffering done above).
        var selᴛ37 = done;
        switch (trySelect(ᐸꟷ(selᴛ37, ꓸꓸꓸ))) {
        case 0 when selᴛ37.ꟷᐳ(out _): {
            break;
        }
        default: {
            break;
        }}
        // extra send after that might have filled C.
        var selᴛ38 = C;
        switch (trySelect(ᐸꟷ(selᴛ38, ꓸꓸꓸ))) {
        case 0 when selᴛ38.ꟷᐳ(out _): {
            break;
        }
        default: {
            break;
        }}
    }
    notDone(done);
    // Test enqueueTimerChan when timer is stopped.
    stop = new channel<bool>(0);
    done = new channel<bool>(2);
    foreach (var _ᴛ5 in range(2)) {
        var Cʗ8 = C;
        goǃ(() => {
            var selᴛ39 = Cʗ8;
            var selᴛ40 = Ꮡstop.ValueSlot;
            switch (select(ᐸꟷ(selᴛ39, ꓸꓸꓸ), ᐸꟷ(selᴛ40, ꓸꓸꓸ))) {
            case 0 when selᴛ39.ꟷᐳ(out _): {
                throw panic("unexpected data");
                break;
            }
            case 1 when selᴛ40.ꟷᐳ(out _): {
                break;
            }}
            Ꮡdone.ValueSlot.ᐸꟷ(true);
        });
    }
    Sleep(sched);
    close(stop);
    waitDone(done);
    waitDone(done);
    // Test that Stop and Reset block old values from being received.
    // (Proposal go.dev/issue/37196.)
    if (synctimerchan) {
        tim.Reset(1);
        Sleep(10 * Millisecond);
        {
            var pending = tim.Stop(); if (pending != true) {
                Ꮡt.Errorf("tim.Stop() = %v, want true"u8, pending);
            }
        }
        noTick();
        tim.Reset(ΔHour);
        noTick();
        {
            var pending = tim.Reset(1); if (pending != true) {
                Ꮡt.Errorf("tim.Stop() = %v, want true"u8, pending);
            }
        }
        assertTick();
        Sleep(10 * Millisecond);
        if (isTicker){
            assertTick();
            Sleep(10 * Millisecond);
        } else {
            noTick();
        }
        {
            var (pending, want) = (tim.Reset(ΔHour), isTicker); if (pending != want) {
                Ꮡt.Errorf("tim.Stop() = %v, want %v"u8, pending, want);
            }
        }
        noTick();
    }
}

public static void TestManualTicker(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Code should not do this, but some old code dating to Go 1.9 does.
    // Make sure this doesn't crash.
    // See go.dev/issue/21874.
    var c = new channel<Δtime.Time>(0);
    var tick = Ꮡ(new Ticker(C: c));
    tick.Stop();
}

public static void TestAfterTimes(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    // Using After(10ms) but waiting for 500ms to read the channel
    // should produce a time from start+10ms, not start+500ms.
    // Make sure it does.
    // To avoid flakes due to very long scheduling delays,
    // require 10 failures in a row before deciding something is wrong.
    foreach (var _ᴛ1 in range(10)) {
        var start = Now();
        var c = After(10 * Millisecond);
        Sleep(500 * Millisecond);
        var dt = (ᐸꟷ(c)).Sub(start);
        if (dt < 400 * Millisecond) {
            return;
        }
        Ꮡt.Logf("After(10ms) time is +%v, want <400ms"u8, dt);
    }
    Ꮡt.Errorf("not working"u8);
}

public static void TestTickTimes(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Parallel();
    // See comment in TestAfterTimes
    foreach (var _ᴛ1 in range(10)) {
        var start = Now();
        var c = Tick(10 * Millisecond);
        Sleep(500 * Millisecond);
        var dt = (ᐸꟷ(c)).Sub(start);
        if (dt < 400 * Millisecond) {
            return;
        }
        Ꮡt.Logf("Tick(10ms) time is +%v, want <400ms"u8, dt);
    }
    Ꮡt.Errorf("not working"u8);
}

} // end time_test_package
