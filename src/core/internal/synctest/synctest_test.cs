// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using context = context_package;
using fmt = fmt_package;
using synctest = go.@internal.synctest_package;
using testenv = go.@internal.testenv_package;
using iter = iter_package;
using os = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strconv = go.strconv_package;
using strings = strings_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using weak = weak_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = go.os.exec_package;
using go;
using go.@internal;
using go.os;
using go.sync;

partial class synctest_test_package {

public static void TestNow(ж<testing.T> Ꮡt) {
    ref var start = ref heap<time.Time>(out var Ꮡstart);
    start = time.Date(2000, 1, 1, 0, 0, 0, 0, time.ΔUTC).In(time.ΔLocal);
    var startʗ1 = start;
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        // Time starts at 2000-1-1 00:00:00.
        {
            var (got, want) = (time.Now(), startʗ1); if (!got.Equal(want)) {
                Ꮡt.Errorf("at start: time.Now = %v, want %v"u8, got, want);
            }
        }
        var startʗ2 = startʗ1;
        goǃ(() => {
            // New goroutines see the same fake clock.
            {
                ref var got = ref heap<time.Time>(out var Ꮡgot);
                got = time.Now();
                ref var want = ref heap<time.Time>(out var Ꮡwant);
                want = startʗ2; if (!got.Equal(want)) {
                    Ꮡt.Errorf("time.Now = %v, want %v"u8, got, want);
                }
            }
        });
        // Time advances after a sleep.
        time.Sleep(1 * time.ΔSecond);
        {
            var (got, want) = (time.Now(), startʗ1.Add(1 * time.ΔSecond)); if (!got.Equal(want)) {
                Ꮡt.Errorf("after sleep: time.Now = %v, want %v"u8, got, want);
            }
        }
    });
}

// TestMonotonicClock exercises comparing times from within a bubble
// with ones from outside the bubble.
public static void TestMonotonicClock(ж<testing.T> Ꮡt) {
    ref var start = ref heap<time.Time>(out var Ꮡstart);
    start = time.Now();
    var startʗ1 = start;
    synctest.Run(() => {
        time.Sleep(time.Until(startʗ1.Round(0)));
        {
            var (got, want) = (time.Now().In(time.ΔUTC), startʗ1.In(time.ΔUTC)); if (!got.Equal(want)) {
                Ꮡt.Fatalf("time.Now() = %v, want %v"u8, got, want);
            }
        }

        var wait = 1 * time.ΔSecond;
        time.Sleep(wait);
        {
            var got = time.Since(startʗ1); if (got != wait) {
                Ꮡt.Fatalf("time.Since(start) = %v, want %v"u8, got, wait);
            }
        }
        {
            var got = time.Now().Sub(startʗ1); if (got != wait) {
                Ꮡt.Fatalf("time.Now().Sub(start) = %v, want %v"u8, got, wait);
            }
        }
    });
}

public static void TestRunEmpty(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
    });
}

public static void TestSimpleWait(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        synctest.Wait();
    });
}

public static void TestGoroutineWait(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        goǃ(() => {
        });
        synctest.Wait();
    });
}

// TestWait starts a collection of goroutines.
// It checks that synctest.Wait waits for all goroutines to exit before returning.
public static void TestWait(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var done = false;
        var ch = new channel<nint>(0);
        ref var f = ref heap<Action>(out var Ꮡf);
        var chʗ1 = ch;
        Ꮡf.ValueSlot = [MethodImpl(MethodImplOptions.NoInlining)] () => {
            nint count = ᐸꟷ(chʗ1);
            if (count == 0){
                done = true;
            } else {
                goǃ(Ꮡf.ValueSlot);
                chʗ1.ᐸꟷ(count - 1);
            }
        };
        goǃ(Ꮡf.ValueSlot);
        ch.ᐸꟷ(100);
        synctest.Wait();
        if (!done) {
            Ꮡt.Fatalf("done = false, want true"u8);
        }
    });
}

public static void TestMallocs(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 100; i++) {
        synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var done = false;
            var ch = new channel<slice<byte>>(0);
            ref var f = ref heap<Action>(out var Ꮡf);
            var chʗ1 = ch;
            Ꮡf.ValueSlot = [MethodImpl(MethodImplOptions.NoInlining)] () => {
                var b = ᐸꟷ(chʗ1);
                if (len(b) == 0){
                    done = true;
                } else {
                    goǃ(Ꮡf.ValueSlot);
                    chʗ1.ᐸꟷ(new slice<byte>(len(b) - 1));
                }
            };
            goǃ(Ꮡf.ValueSlot);
            ch.ᐸꟷ(new slice<byte>(100));
            synctest.Wait();
            if (!done) {
                Ꮡt.Fatalf("done = false, want true"u8);
            }
        });
    }
}

public static void TestTimerReadBeforeDeadline(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        var start = time.Now();
        var tm = time.NewTimer((time.Duration)(5000000000L));
        ᐸꟷ((~tm).C);
        {
            var (got, want) = (time.Since(start), (time.Duration)(5000000000L)); if (got != want) {
                Ꮡt.Errorf("after sleep: time.Since(start) = %v, want %v"u8, got, want);
            }
        }
    });
}

public static void TestTimerReadAfterDeadline(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        var delay = 1 * time.ΔSecond;
        var want = time.Now().Add(delay);
        var tm = time.NewTimer(delay);
        time.Sleep(2 * delay);
        var got = ᐸꟷ((~tm).C);
        if (got != want) {
            Ꮡt.Errorf("<-tm.C = %v, want %v"u8, got, want);
        }
    });
}

public static void TestTimerReset(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        var start = time.Now();
        var tm = time.NewTimer(1 * time.ΔSecond);
        {
            var (got, want) = (ᐸꟷ((~tm).C), start.Add(1 * time.ΔSecond)); if (got != want) {
                Ꮡt.Errorf("first sleep: <-tm.C = %v, want %v"u8, got, want);
            }
        }

        tm.Reset(2 * time.ΔSecond);
        {
            var (got, want) = (ᐸꟷ((~tm).C), start.Add((time.Duration)(3000000000L))); if (got != want) {
                Ꮡt.Errorf("second sleep: <-tm.C = %v, want %v"u8, got, want);
            }
        }

        tm.Reset((time.Duration)(3000000000L));
        time.Sleep(1 * time.ΔSecond);
        tm.Reset((time.Duration)(3000000000L));
        {
            var (got, want) = (ᐸꟷ((~tm).C), start.Add((time.Duration)(7000000000L))); if (got != want) {
                Ꮡt.Errorf("third sleep: <-tm.C = %v, want %v"u8, got, want);
            }
        }
    });
}

public static void TestTimeAfter(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        nint i = 0;
        time.AfterFunc(1 * time.ΔSecond, [MethodImpl(MethodImplOptions.NoInlining)] () => {
            // Ensure synctest group membership propagates through the AfterFunc.
            i++; // 1
            goǃ(() => {
                time.Sleep(1 * time.ΔSecond);
                i++; // 2
            });
        });
        time.Sleep((time.Duration)(3000000000L));
        synctest.Wait();
        {
            nint got = i;
            nint want = 2; if (got != want) {
                Ꮡt.Errorf("after sleep and wait: i = %v, want %v"u8, got, want);
            }
        }
    });
}

public static void TestTimerAfterBubbleExit(ж<testing.T> Ꮡt) {
    var run = false;
    synctest.Run(() => {
        time.AfterFunc(1 * time.ΔSecond, () => {
            run = true;
        });
    });
    if (run) {
        Ꮡt.Errorf("timer ran before bubble exit"u8);
    }
}

public static void TestTimerFromOutsideBubble(ж<testing.T> Ꮡt) {
    var tm = time.NewTimer(10 * time.Millisecond);
    var tmʗ1 = tm;
    synctest.Run(() => {
        ᐸꟷ((~tmʗ1).C);
    });
    if (tm.Stop()) {
        Ꮡt.Errorf("synctest.Run unexpectedly returned before timer fired"u8);
    }
}

// TestTimerNondeterminism verifies that timers firing at the same instant
// don't always fire in exactly the same order.
public static void TestTimerNondeterminism(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        const nint iterations = 1000;
        bool seen1 = default!;
        bool seen2 = default!;
        foreach (var _ᴛ1 in range(iterations)) {
            var tm1 = time.NewTimer(1);
            var tm2 = time.NewTimer(1);
            var selᴛ1 = (~tm1).C;
            var selᴛ2 = (~tm2).C;
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                seen1 = true;
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
                seen2 = true;
                break;
            }}
            if (seen1 && seen2) {
                return;
            }
            synctest.Wait();
        }
        Ꮡt.Errorf("after %v iterations, seen timer1:%v, timer2:%v; want both"u8, (nint)(iterations), seen1, seen2);
    });
}

// TestSleepNondeterminism verifies that goroutines sleeping to the same instant
// don't always schedule in exactly the same order.
public static void TestSleepNondeterminism(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        const nint iterations = 1000;
        bool seen1 = default!;
        bool seen2 = default!;
        foreach (var _ᴛ1 in range(iterations)) {
            ref var first = ref heap(new atomic.Int32(), out var Ꮡfirst);
            goǃ(() => {
                time.Sleep(1);
                Ꮡfirst.CompareAndSwap(0, 1);
            });
            goǃ(() => {
                time.Sleep(1);
                Ꮡfirst.CompareAndSwap(0, 2);
            });
            time.Sleep(1);
            synctest.Wait();
            {
                var v = Ꮡfirst.Load();
                switch (v) {
                case 1: {
                    seen1 = true;
                    break;
                }
                case 2: {
                    seen2 = true;
                    break;
                }
                default: {
                    Ꮡt.Fatalf("first = %v, want 1 or 2"u8, v);
                    break;
                }}
            }
            if (seen1 && seen2) {
                return;
            }
            synctest.Wait();
        }
        Ꮡt.Errorf("after %v iterations, seen goroutine 1:%v, 2:%v; want both"u8, (nint)(iterations), seen1, seen2);
    });
}

// TestTimerRunsImmediately verifies that a 0-duration timer sends on its channel
// without waiting for the bubble to block.
public static void TestTimerRunsImmediately(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        var start = time.Now();
        var tm = time.NewTimer(0);
        var selᴛ3 = (~tm).C;
        switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out var got): {
            if (!got.Equal(start)) {
                Ꮡt.Errorf("<-tm.C = %v, want %v"u8, got, start);
            }
            break;
        }
        default: {
            Ꮡt.Errorf("0-duration timer channel is not readable; want it to be"u8);
            break;
        }}
    });
}

// TestTimerRunsLater verifies that reading from a timer's channel receives the
// timer fired, even when that time is in reading from a timer's channel receives the
// time the timer fired, even when that time is in the past.
public static void TestTimerRanInPast(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        var delay = 1 * time.ΔSecond;
        var want = time.Now().Add(delay);
        var tm = time.NewTimer(delay);
        time.Sleep(2 * delay);
        var selᴛ4 = (~tm).C;
        switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ4.ꟷᐳ(out var got): {
            if (!got.Equal(want)) {
                Ꮡt.Errorf("<-tm.C = %v, want %v"u8, got, want);
            }
            break;
        }
        default: {
            Ꮡt.Errorf("0-duration timer channel is not readable; want it to be"u8);
            break;
        }}
    });
}

// TestAfterFuncRunsImmediately verifies that a 0-duration AfterFunc is scheduled
// without waiting for the bubble to block.
public static void TestAfterFuncRunsImmediately(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        ref var b = ref heap(new atomic.Bool(), out var Ꮡb);
        time.AfterFunc(0, () => {
            Ꮡb.Store(true);
        });
        while (!Ꮡb.Load()) {
            Δruntime.Gosched();
        }
    });
}

// TestTimerResetZeroDoNotHang verifies that using timer.Reset(0) does not
// cause the test to hang indefinitely. See https://go.dev/issue/76052.
public static void TestTimerResetZeroDoNotHang(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var timer = time.NewTimer(0);
        var (ctx, cancel) = context.WithCancel(context.Background());

        var ctxʗ1 = ctx;
        var timerʗ1 = timer;
        goǃ(() => {
            while (ᐧ) {
                var selᴛ5 = ctxʗ1.Done();
                var selᴛ6 = (~timerʗ1).C;
                switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
                case 0 when selᴛ5.ꟷᐳ(out _): {
                    return;
                }
                case 1 when selᴛ6.ꟷᐳ(out _): {
                    break;
                }}
            }
        });

        synctest.Wait();
        timer.Reset(0);
        synctest.Wait();
        cancel();
        synctest.Wait();
    });
}

internal partial struct TestChannelFromOutsideBubble_type /*dyn*/ {
    internal @string desc;
    internal Action<channel<nint>> outside;
    internal Action<channel<nint>> inside;
}

public static void TestChannelFromOutsideBubble(ж<testing.T> Ꮡt) {
    var choutside = new channel<EmptyStruct>(0);
        var choutsideʗ1 = choutside;


    foreach (var (_, vᴛ1) in new TestChannelFromOutsideBubble_type[]{new(
        desc: "read closed"u8,
        outside: (channel<nint> ch) => {
            close(ch);
        },
        inside: (channel<nint> ch) => {
            ᐸꟷ(ch);
        }
    ), new(
        desc: "read value"u8,
        outside: (channel<nint> ch) => {
            ch.ᐸꟷ(0);
        },
        inside: (channel<nint> ch) => {
            ᐸꟷ(ch);
        }
    ), new(
        desc: "write value"u8,
        outside: (channel<nint> ch) => {
            ᐸꟷ(ch);
        },
        inside: (channel<nint> ch) => {
            ch.ᐸꟷ(0);
        }
    ), new(
        desc: "select outside only"u8,
        outside: (channel<nint> ch) => {
            close(ch);
        },
        inside: (channel<nint> ch) => {
            var selᴛ7 = ch;
            var selᴛ8 = choutsideʗ1;
            switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ8.ꟷᐳ(out _): {
                break;
            }}
        }
    ), new(
        desc: "select mixed"u8,
        outside: (channel<nint> ch) => {
            close(ch);
        },
        inside: (channel<nint> ch) => {
            var ch2 = new channel<EmptyStruct>(0);
            var selᴛ9 = ch;
            var selᴛ10 = ch2;
            switch (select(ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
            case 0 when selᴛ9.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ10.ꟷᐳ(out _): {
                break;
            }}
        }
    )
    }.slice()) {
        ref var test = ref heap(new TestChannelFromOutsideBubble_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.desc, (ж<testing.T> tΔ1) => {
            var ch = new channel<nint>(0);
            var chʗ1 = ch;
            var testʗ2 = testʗ1;
            time.AfterFunc(1 * time.Millisecond, () => {
                testʗ2.outside(chʗ1);
            });
            var chʗ2 = ch;
            var testʗ3 = testʗ1;
            synctest.Run(() => {
                testʗ3.inside(chʗ2);
            });
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string outsideBubbleˢ = "outside_bubble"u8;
private static readonly @string differentBubbleˢ = "different_bubble"u8;

internal partial struct TestChannelMovedOutOfBubble_type /*dyn*/ {
    internal @string desc;
    internal Action<channel<EmptyStruct>> f;
    internal @string wantFatal;
}

public static void TestChannelMovedOutOfBubble(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestChannelMovedOutOfBubble_type[]{new(
        desc: "receive"u8,
        f: (channel<EmptyStruct> ch) => {
            ᐸꟷ(ch);
        },
        wantFatal: "receive on synctest channel from outside bubble"u8
    ), new(
        desc: "send"u8,
        f: (channel<EmptyStruct> ch) => {
            ch.ᐸꟷ(new EmptyStruct());
        },
        wantFatal: "send on synctest channel from outside bubble"u8
    ), new(
        desc: "close"u8,
        f: (channel<EmptyStruct> ch) => {
            close(ch);
        },
        wantFatal: "close of synctest channel from outside bubble"u8
    )
    }.slice()) {
        ref var test = ref heap(new TestChannelMovedOutOfBubble_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.desc, (ж<testing.T> tΔ1) => {
            // Bubbled channel accessed from outside any bubble.
            var testʗ2 = testʗ1;
            tΔ1.Run(outsideBubbleˢ, (ж<testing.T> tΔ2) => {
                var testʗ3 = testʗ2;
                wantFatal(tΔ2, testʗ2.wantFatal, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                    var donec = new channel<EmptyStruct>(0);
                    var ch = new channel<channel<EmptyStruct>>(0);
                    var chʗ1 = ch;
                    var donecʗ1 = donec;
                    var testʗ4 = testʗ3;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(ᴛ1 => close(ᴛ1), donecʗ1, ref ᒐ);
                            testʗ4.f(ᐸꟷ(chʗ1));
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                    var chʗ2 = ch;
                    synctest.Run(() => {
                        chʗ2.ᐸꟷ(new channel<EmptyStruct>(0));
                    });
                    ᐸꟷ(donec);
                });
            });
            // Bubbled channel accessed from a different bubble.
            var testʗ5 = testʗ1;
            tΔ1.Run(differentBubbleˢ, (ж<testing.T> tΔ3) => {
                var testʗ6 = testʗ5;
                wantFatal(tΔ3, testʗ5.wantFatal, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                    var donec = new channel<EmptyStruct>(0);
                    var ch = new channel<channel<EmptyStruct>>(0);
                    var chʗ3 = ch;
                    var donecʗ2 = donec;
                    var testʗ7 = testʗ6;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(ᴛ1 => close(ᴛ1), donecʗ2, ref ᒐ);
                            var c = ᐸꟷ(chʗ3);
                            var cʗ1 = c;
                            var testʗ8 = testʗ7;
                            synctest.Run(() => {
                                testʗ8.f(cʗ1);
                            });
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                    var chʗ4 = ch;
                    synctest.Run(() => {
                        chʗ4.ᐸꟷ(new channel<EmptyStruct>(0));
                    });
                    ᐸꟷ(donec);
                });
            });
        });
    }
}

internal partial struct TestTimerFromInsideBubble_type /*dyn*/ {
    internal @string desc;
    internal Action<ж<time.Timer>> f;
    internal @string wantFatal;
}

public static void TestTimerFromInsideBubble(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestTimerFromInsideBubble_type[]{new(
        desc: "read channel"u8,
        f: (ж<time.Timer> tm) => {
            ᐸꟷ((~tm).C);
        },
        wantFatal: "receive on synctest channel from outside bubble"u8
    ), new(
        desc: "Reset"u8,
        f: (ж<time.Timer> tm) => {
            tm.Reset(1 * time.ΔSecond);
        },
        wantFatal: "reset of synctest timer from outside bubble"u8
    ), new(
        desc: "Stop"u8,
        f: (ж<time.Timer> tm) => {
            tm.Stop();
        },
        wantFatal: "stop of synctest timer from outside bubble"u8
    )
    }.slice()) {
        ref var test = ref heap(new TestTimerFromInsideBubble_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.desc, (ж<testing.T> tΔ1) => {
            var testʗ2 = testʗ1;
            wantFatal(tΔ1, testʗ1.wantFatal, [MethodImpl(MethodImplOptions.NoInlining)] () => {
                var donec = new channel<EmptyStruct>(0);
                var ch = new channel<ж<time.Timer>>(0);
                var chʗ1 = ch;
                var donecʗ1 = donec;
                var testʗ3 = testʗ2;
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(ᴛ1 => close(ᴛ1), donecʗ1, ref ᒐ);
                        testʗ3.f(ᐸꟷ(chʗ1));
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                var chʗ2 = ch;
                synctest.Run(() => {
                    var tm = time.NewTimer(1 * time.ΔSecond);
                    chʗ2.ᐸꟷ(tm);
                });
                ᐸꟷ(donec);
            });
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deadlockAllGoroutinesInˢ = "deadlock: all goroutines in bubble are blocked"u8;

public static void TestDeadlockRoot(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanic, Ꮡt, deadlockAllGoroutinesInˢ, ref ᒐ, recovers: true);
        synctest.Run(() => {
            select();
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deadlockMainBubbleˢ = "deadlock: main bubble goroutine has exited but blocked goroutines remain"u8;

public static void TestDeadlockChild(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanic, Ꮡt, deadlockMainBubbleˢ, ref ᒐ, recovers: true);
        synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
            goǃ(() => {
                select();
            });
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDeadlockTicker(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanic, Ꮡt, deadlockMainBubbleˢ, ref ᒐ, recovers: true);
        synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
            goǃ(() => {
                foreach (var _ᴛ1 in time.Tick(1 * time.ΔSecond)) {
                    Ꮡt.Errorf("ticker unexpectedly ran"u8);
                    return;
                }
            });
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCond(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        ref var mu = ref heap(new sync.Mutex(), out var Ꮡmu);
        var cond = sync.NewCond(new sync.MutexжLocker(Ꮡmu));
        var start = time.Now();
        time.Duration waitTime = /* 1 * time.Millisecond */ 1000000;

        var condʗ1 = cond;
        goǃ(() => {
            // Signal the cond.
            time.Sleep(waitTime);
            Ꮡmu.Lock();
            condʗ1.Signal();
            Ꮡmu.Unlock();

            // Broadcast to the cond.
            time.Sleep(waitTime);
            Ꮡmu.Lock();
            condʗ1.Broadcast();
            Ꮡmu.Unlock();
        });

        // Wait for cond.Signal.
        Ꮡmu.Lock();
        cond.Wait();
        Ꮡmu.Unlock();
        {
            var (got, want) = (time.Since(start), waitTime); if (got != want) {
                Ꮡt.Errorf("after cond.Signal: time elapsed = %v, want %v"u8, got, want);
            }
        }

        // Wait for cond.Broadcast in two goroutines.
        var waiterDone = false;
        var condʗ2 = cond;
        goǃ(() => {
            Ꮡmu.Lock();
            condʗ2.Wait();
            Ꮡmu.Unlock();
            waiterDone = true;
        });
        Ꮡmu.Lock();
        cond.Wait();
        Ꮡmu.Unlock();
        synctest.Wait();
        if (!waiterDone) {
            Ꮡt.Errorf("after cond.Broadcast: waiter not done"u8);
        }
        {
            var (got, want) = (time.Since(start), 2 * waitTime); if (got != want) {
                Ꮡt.Errorf("after cond.Broadcast: time elapsed = %v, want %v"u8, got, want);
            }
        }
    });
}

public static void TestIteratorPush(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var seq = (Func<time.Time, bool> yield) => {
            while (yield(time.Now())) {
                time.Sleep(1 * time.ΔSecond);
            }
        };
        ref var got = ref heap<slice<time.Time>>(out var Ꮡgot);
        var seqʗ1 = seq;
        goǃ(() => {
            foreach (var iᴛ1 in range(seqʗ1)) {
                ref var now = ref heap(new time.Time(), out var Ꮡnow);
                now = iᴛ1;

                Ꮡgot.ValueSlot = append(Ꮡgot.ValueSlot, now);
                if (len(Ꮡgot.ValueSlot) >= 3) {
                    break;
                }
            }
        });
        var want = new time.Time[]{
            time.Now(),
            time.Now().Add(1 * time.ΔSecond),
            time.Now().Add(2 * time.ΔSecond)
        }.slice();
        time.Sleep((time.Duration)(5000000000L));
        synctest.Wait();
        if (!slices.Equal<slice<time.Time>, time.Time>(Ꮡgot.ValueSlot, want)) {
            Ꮡt.Errorf("got: %v; want: %v"u8, Ꮡgot.ValueSlot, want);
        }
    });
}

public static void TestIteratorPull(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        var seq = (Func<time.Time, bool> yield) => {
            while (yield(time.Now())) {
                time.Sleep(1 * time.ΔSecond);
            }
        };
        ref var got = ref heap<slice<time.Time>>(out var Ꮡgot);
        var seqʗ1 = seq;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var (next, stop) = iter.Pull(new iter.Seq<time.Time>(seqʗ1));
                var stopʗ1 = stop;
                defer(stopʗ1, ref ᒐ);
                while (len(Ꮡgot.ValueSlot) < 3) {
                    ref var now = ref heap<time.Time>(out var Ꮡnow);
                    (now, _) = next();
                    Ꮡgot.ValueSlot = append(Ꮡgot.ValueSlot, now);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var want = new time.Time[]{
            time.Now(),
            time.Now().Add(1 * time.ΔSecond),
            time.Now().Add(2 * time.ΔSecond)
        }.slice();
        time.Sleep((time.Duration)(5000000000L));
        synctest.Wait();
        if (!slices.Equal<slice<time.Time>, time.Time>(Ꮡgot.ValueSlot, want)) {
            Ꮡt.Errorf("got: %v; want: %v"u8, Ꮡgot.ValueSlot, want);
        }
    });
}

public static partial void TestReflectFuncOf(ж<testing.T> Ꮡt) {
    void mkfunc(@string name, nint i) {
        reflect.FuncOf(new reflectꓸType[]{reflect.StructOf(new reflect.StructField[]{new(
            Name: name + strconv.Itoa(i),
            Type: reflect.TypeOf((nint)0)
        )
        }.slice())
        }.slice(), default!, false);
    }
    var mkfuncʗ1 = mkfunc;
    goǃ(() => {
        for (nint i = 0; i < 100000; i++) {
            mkfuncʗ1("A"u8, i);
        }
    });
    var mkfuncʗ2 = mkfunc;
    synctest.Run(() => {
        for (nint i = 0; i < 100000; i++) {
            mkfuncʗ2("A"u8, i);
        }
    });
}

public static void TestWaitGroupInBubble(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        time.Duration delay = /* 1 * time.Second */ 1000000000;
        goǃ(() => {
            time.Sleep(delay);
            Ꮡwg.Done();
        });
        var start = time.Now();
        Ꮡwg.Wait();
        {
            var got = time.Since(start); if (got != delay) {
                Ꮡt.Fatalf("WaitGroup.Wait() took %v, want %v"u8, got, delay);
            }
        }
    });
}

// https://go.dev/issue/74386
public static void TestWaitGroupRacingAdds(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        foreach (var _ᴛ1 in range(100)) {
            Ꮡwg.Go(() => {
            });
        }
        Ꮡwg.Wait();
    });
}

public static partial void TestWaitGroupOutOfBubble(ж<testing.T> Ꮡt) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(1);
    var donec = new channel<EmptyStruct>(0);
    var donecʗ1 = donec;
    goǃ(synctest.Run, () => {
        // Since wg.Add was called outside the bubble, Wait is not durably blocking
        // and this waits until wg.Done is called below.
        Ꮡwg.Wait();
        close(donecʗ1);
    });
    var selᴛ11 = donec;
    var selᴛ12 = time.After(1 * time.Millisecond);
    switch (select(ᐸꟷ(selᴛ11, ꓸꓸꓸ), ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
    case 0 when selᴛ11.ꟷᐳ(out _): {
        Ꮡt.Fatalf("synctest.Run finished before WaitGroup.Done called"u8);
        break;
    }
    case 1 when selᴛ12.ꟷᐳ(out _): {
        break;
    }}
    Ꮡwg.Done();
    ᐸꟷ(donec);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fatalErrorSyncWaitGroupˢ = "fatal error: sync: WaitGroup.Add called from inside and outside synctest bubble"u8;

public static void TestWaitGroupMovedIntoBubble(ж<testing.T> Ꮡt) {
    wantFatal(Ꮡt, fatalErrorSyncWaitGroupˢ, () => {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        synctest.Run(() => {
            Ꮡwg.Add(1);
        });
    });
}

public static void TestWaitGroupMovedOutOfBubble(ж<testing.T> Ꮡt) {
    wantFatal(Ꮡt, fatalErrorSyncWaitGroupˢ, () => {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        synctest.Run(() => {
            Ꮡwg.Add(1);
        });
        Ꮡwg.Add(1);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fatalErrorSyncWaitGroupˢ2 = "fatal error: sync: WaitGroup.Add called from multiple synctest bubbles"u8;

public static void TestWaitGroupMovedBetweenBubblesWithNonZeroCount(ж<testing.T> Ꮡt) {
    wantFatal(Ꮡt, fatalErrorSyncWaitGroupˢ2, () => {
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        synctest.Run(() => {
            Ꮡwg.Add(1);
        });
        synctest.Run(() => {
            Ꮡwg.Add(1);
        });
    });
}

public static void TestWaitGroupDisassociateInWait(ж<testing.T> Ꮡt) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    synctest.Run(() => {
        Ꮡwg.Add(1);
        Ꮡwg.Done();
        // Count and waiters are 0, so Wait disassociates the WaitGroup.
        Ꮡwg.Wait();
    });
    synctest.Run(() => {
        // Reusing the WaitGroup is safe, because it is no longer bubbled.
        Ꮡwg.Add(1);
        Ꮡwg.Done();
    });
}

public static void TestWaitGroupDisassociateInAdd(ж<testing.T> Ꮡt) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        Ꮡwg.Add(1);
        goǃ(Ꮡwg.Wait);
        synctest.Wait(); // wait for Wait to block
        // Count is 0 and waiters != 0, so Done wakes the waiters and
        // disassociates the WaitGroup.
        Ꮡwg.Done();
    });
    synctest.Run(() => {
        // Reusing the WaitGroup is safe, because it is no longer bubbled.
        Ꮡwg.Add(1);
        Ꮡwg.Done();
    });
}

internal static readonly ж<sync.WaitGroup> ᏑtestWaitGroupLinkerAllocatedWG = new StaticBox<sync.WaitGroup>(default(sync.WaitGroup));
internal static ref sync.WaitGroup testWaitGroupLinkerAllocatedWG => ref ((StaticBox<sync.WaitGroup>)ᏑtestWaitGroupLinkerAllocatedWG).ValueSlot;

public static void TestWaitGroupLinkerAllocated(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        // This WaitGroup is probably linker-allocated and has no span,
        // so we won't be able to add a special to it associating it with
        // this bubble.
        //
        // Operations on it may not be durably blocking,
        // but they shouldn't fail.
        ᏑtestWaitGroupLinkerAllocatedWG.Go(() => {
        });
        ᏑtestWaitGroupLinkerAllocatedWG.Wait();
    });
}

internal static ж<sync.WaitGroup> testWaitGroupHeapAllocatedWG = @new<sync.WaitGroup>();

public static void TestWaitGroupHeapAllocated(ж<testing.T> Ꮡt) {
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        // This package-scoped WaitGroup var should have been heap-allocated,
        // so we can associate it with a bubble.
        testWaitGroupHeapAllocatedWG.Add(1);
        goǃ(testWaitGroupHeapAllocatedWG.Wait);
        synctest.Wait();
        testWaitGroupHeapAllocatedWG.Done();
    });
}

// Issue #75134: Many racing bubble associations.
public static void TestWaitGroupManyBubbles(ж<testing.T> Ꮡt) {
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    foreach (var _ᴛ1 in range(100)) {
        Ꮡwg.Go(() => {
            synctest.Run(() => {
                var cancelc = new channel<EmptyStruct>(0);
                ref var wg2 = ref heap(new sync.WaitGroup(), out var Ꮡwg2);
                foreach (var _ᴛ2 in range(100)) {
                    var cancelcʗ1 = cancelc;
                    Ꮡwg2.Go(() => {
                        ᐸꟷ(cancelcʗ1);
                    });
                }
                synctest.Wait();
                close(cancelc);
                Ꮡwg2.Wait();
            });
        });
    }
    Ꮡwg.Wait();
}

public static void TestHappensBefore(ж<testing.T> Ꮡt) {
    // Use two parallel goroutines accessing different vars to ensure that
    // we correctly account for multiple goroutines in the bubble.
    nint v1 = default!;
    nint v2 = default!;
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        v1++; // 1
        v2++; // 1

        // Wait returns after these goroutines exit.
        goǃ(() => {
            v1++; // 2
        });
        goǃ(() => {
            v2++; // 2
        });
        synctest.Wait();

        v1++; // 3
        v2++; // 3

        // Wait returns after these goroutines block.
        var ch1 = new channel<EmptyStruct>(0);
        var ch1ʗ1 = ch1;
        goǃ(() => {
            v1++; // 4
            ᐸꟷ(ch1ʗ1);
        });
        var ch1ʗ2 = ch1;
        goǃ(() => {
            v2++; // 4
            ᐸꟷ(ch1ʗ2);
        });
        synctest.Wait();

        v1++; // 5
        v2++; // 5
        close(ch1);

        // Wait returns after these timers run.
        time.AfterFunc(0, () => {
            v1++; // 6
        });
        time.AfterFunc(0, () => {
            v2++; // 6
        });
        synctest.Wait();

        v1++; // 7
        v2++; // 7

        // Wait returns after these timer goroutines block.
        var ch2 = new channel<EmptyStruct>(0);
        var ch2ʗ1 = ch2;
        time.AfterFunc(0, () => {
            v1++; // 8
            ᐸꟷ(ch2ʗ1);
        });
        var ch2ʗ2 = ch2;
        time.AfterFunc(0, () => {
            v2++; // 8
            ᐸꟷ(ch2ʗ2);
        });
        synctest.Wait();

        v1++; // 9
        v2++; // 9
        close(ch2);
    });
    // This Run happens after the previous Run returns.
    synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
            goǃ(() => {
                v1++; // 10
            });
        });
        goǃ([MethodImpl(MethodImplOptions.NoInlining)] () => {
            goǃ(() => {
                v2++; // 10
            });
        });
    });
    // These tests happen after Run returns.
    {
        nint got = v1;
        nint want = 10; if (got != want) {
            Ꮡt.Errorf("v1 = %v, want %v"u8, got, want);
        }
    }
    {
        nint got = v2;
        nint want = 10; if (got != want) {
            Ꮡt.Errorf("v2 = %v, want %v"u8, got, want);
        }
    }
}

// https://go.dev/issue/73817
public static void TestWeak(ж<testing.T> Ꮡt) {
    synctest.Run(() => {
        foreach (var _ᴛ1 in range(5)) {
            Δruntime.GC();
            ref var b = ref heap<slice<byte>>(out var Ꮡb);
            b = new slice<byte>(1024);
            weak.Make<slice<byte>>(Ꮡb);
        }
    });
}

internal static void wantPanic(ж<testing.T> Ꮡt, @string want) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        {
            var e = recover(calledByDefer); if (e != default!){
                {
                    @string got = fmt.Sprint(e); if (got != want) {
                        Ꮡt.Errorf("got panic message %q, want %q"u8, got, want);
                    }
                }
            } else {
                Ꮡt.Errorf("got no panic, want one"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;

internal static void wantFatal(ж<testing.T> Ꮡt, @string want, Action f) {
    Ꮡt.Helper();

    if (os.Getenv(goWantHelperProcessˢ) == "1"u8) {
        f();
        return;
    }

    var cmd = testenv.Command(new testing_TжTB(Ꮡt), testenv.Executable(new testing_TжTB(Ꮡt)), "-test.run=^"u8 + Ꮡt.Name() + "$"u8);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Value.Env = append((~cmd).Env, "GO_WANT_HELPER_PROCESS=1"u8);
    var (@out, err) = cmd.CombinedOutput();
    if (err == default!) {
        Ꮡt.Errorf("expected test function to panic, but test returned successfully"u8);
    }
    if (!strings.Contains((@string)@out, want)) {
        Ꮡt.Errorf("wanted test output contaiing %q; got %q"u8, want, (@string)@out);
    }
}

} // end synctest_test_package
