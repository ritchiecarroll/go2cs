// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using synctest = go.@internal.synctest_package;
using iter = iter_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using sync = go.sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go;
using go.@internal;

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
            var selᴛ1 = ch;
            var selᴛ2 = choutsideʗ1;
            switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ2.ꟷᐳ(out _): {
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
            var selᴛ3 = ch;
            var selᴛ4 = ch2;
            switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ4.ꟷᐳ(out _): {
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

internal partial struct TestTimerFromInsideBubble_type /*dyn*/ {
    internal @string desc;
    internal Action<ж<time.Timer>> f;
    internal @string wantPanic;
}

public static void TestTimerFromInsideBubble(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestTimerFromInsideBubble_type[]{new(
        desc: "read channel"u8,
        f: (ж<time.Timer> tm) => {
            ᐸꟷ((~tm).C);
        },
        wantPanic: "receive on synctest channel from outside bubble"u8
    ), new(
        desc: "Reset"u8,
        f: (ж<time.Timer> tm) => {
            tm.Reset(1 * time.ΔSecond);
        },
        wantPanic: "reset of synctest timer from outside bubble"u8
    ), new(
        desc: "Stop"u8,
        f: (ж<time.Timer> tm) => {
            tm.Stop();
        },
        wantPanic: "stop of synctest timer from outside bubble"u8
    )
    }.slice()) {
        ref var test = ref heap(new TestTimerFromInsideBubble_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.desc, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            var donec = new channel<EmptyStruct>(0);
            var ch = new channel<ж<time.Timer>>(0);
            var chʗ1 = ch;
            var donecʗ1 = donec;
            var testʗ2 = testʗ1;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => close(ᴛ1), donecʗ1, ref ᒐ);
                    defer(wantPanic, tΔ1, testʗ2.wantPanic, ref ᒐ);
                    testʗ2.f(ᐸꟷ(chʗ1));
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
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string deadlockAllGoroutinesInˢ = "deadlock: all goroutines in bubble are blocked"u8;

public static void TestDeadlockRoot(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanic, Ꮡt, deadlockAllGoroutinesInˢ, ref ᒐ);
        synctest.Run(() => {
            select();
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestDeadlockChild(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanic, Ꮡt, deadlockAllGoroutinesInˢ, ref ᒐ);
        synctest.Run([MethodImpl(MethodImplOptions.NoInlining)] () => {
            goǃ(() => {
                select();
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
            Type: reflect.TypeOf((nint)(0))
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

public static void TestWaitGroup(ж<testing.T> Ꮡt) {
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

internal static void wantPanic(ж<testing.T> Ꮡt, @string want) {
    GoFrame ᒐ = default;
    try {
        {
            var e = recover(); if (e != default!){
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

} // end synctest_test_package
