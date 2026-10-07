// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using Δruntime = runtime_package;
using static sync_package;
using Δtesting = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goroutineNotAsleepˢ = (@string)"goroutine not asleep"u8;
internal static readonly object tooManyGoroutinesAwakeˢ = (@string)"too many goroutines awake"u8;

public static partial void TestCondSignal(ж<Δtesting.T> Ꮡt) {
    ref var m = ref heap(new Δsync.Mutex(), out var Ꮡm);
    var c = NewCond(new Δsync.MutexжLocker(Ꮡm));
    nint n = 2;
    var running = new channel<bool>(n);
    var awake = new channel<bool>(n);
    for (nint i = 0; i < n; i++) {
        var awakeʗ1 = awake;
        var cʗ1 = c;
        var runningʗ1 = running;
        goǃ(() => {
            Ꮡm.Lock();
            runningʗ1.ᐸꟷ(true);
            cʗ1.Wait();
            awakeʗ1.ᐸꟷ(true);
            Ꮡm.Unlock();
        });
    }
    for (nint i = 0; i < n; i++) {
        ᐸꟷ(running); // Wait for everyone to run.
    }
    while (n > 0) {
        var selᴛ1 = awake;
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out _): {
            Ꮡt.Fatal(goroutineNotAsleepˢ);
            break;
        }
        default: {
            break;
        }}
        Ꮡm.Lock();
        c.Signal();
        Ꮡm.Unlock();
        ᐸꟷ(awake); // Will deadlock if no goroutine wakes up
        var selᴛ2 = awake;
        switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            Ꮡt.Fatal(tooManyGoroutinesAwakeˢ);
            break;
        }
        default: {
            break;
        }}
        n--;
    }
    c.Signal();
}

public static partial void TestCondSignalGenerations(ж<Δtesting.T> Ꮡt) {
    ref var m = ref heap(new Δsync.Mutex(), out var Ꮡm);
    var c = NewCond(new Δsync.MutexжLocker(Ꮡm));
    nint n = 100;
    var running = new channel<bool>(n);
    var awake = new channel<nint>(n);
    for (nint i = 0; i < n; i++) {
        var awakeʗ1 = awake;
        var cʗ1 = c;
        var runningʗ1 = running;
        goǃ((nint iΔ1) => {
            Ꮡm.Lock();
            runningʗ1.ᐸꟷ(true);
            cʗ1.Wait();
            awakeʗ1.ᐸꟷ(iΔ1);
            Ꮡm.Unlock();
        }, i);
        if (i > 0) {
            nint a = ᐸꟷ(awake);
            if (a != i - 1) {
                Ꮡt.Fatalf("wrong goroutine woke up: want %d, got %d"u8, i - 1, a);
            }
        }
        ᐸꟷ(running);
        Ꮡm.Lock();
        c.Signal();
        Ꮡm.Unlock();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goroutineWokeUpTwiceˢ = (@string)"goroutine woke up twice"u8;
internal static readonly object goroutineDidNotExitˢ = (@string)"goroutine did not exit"u8;

public static partial void TestCondBroadcast(ж<Δtesting.T> Ꮡt) {
    ref var m = ref heap(new Δsync.Mutex(), out var Ꮡm);
    var c = NewCond(new Δsync.MutexжLocker(Ꮡm));
    nint n = 200;
    var running = new channel<nint>(n);
    var awake = new channel<nint>(n);
    var exit = false;
    for (nint i = 0; i < n; i++) {
        var awakeʗ1 = awake;
        var cʗ1 = c;
        var runningʗ1 = running;
        goǃ((nint g) => {
            Ꮡm.Lock();
            while (!exit) {
                runningʗ1.ᐸꟷ(g);
                cʗ1.Wait();
                awakeʗ1.ᐸꟷ(g);
            }
            Ꮡm.Unlock();
        }, i);
    }
    for (nint i = 0; i < n; i++) {
        for (nint iΔ1 = 0; iΔ1 < n; iΔ1++) {
            ᐸꟷ(running); // Will deadlock unless n are running.
        }
        if (i == n - 1) {
            Ꮡm.Lock();
            exit = true;
            Ꮡm.Unlock();
        }
        var selᴛ3 = awake;
        switch (trySelect(ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out _): {
            Ꮡt.Fatal(goroutineNotAsleepˢ);
            break;
        }
        default: {
            break;
        }}
        Ꮡm.Lock();
        c.Broadcast();
        Ꮡm.Unlock();
        var seen = new slice<bool>(n);
        for (nint iΔ2 = 0; iΔ2 < n; iΔ2++) {
            nint g = ᐸꟷ(awake);
            if (seen[g]) {
                Ꮡt.Fatal(goroutineWokeUpTwiceˢ);
            }
            seen[g] = true;
        }
    }
    var selᴛ4 = running;
    switch (trySelect(ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ4.ꟷᐳ(out _): {
        Ꮡt.Fatal(goroutineDidNotExitˢ);
        break;
    }
    default: {
        break;
    }}
    c.Broadcast();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object want2ˢ = (@string)"want 2"u8;
internal static readonly object want3ˢ = (@string)"want 3"u8;

public static partial void TestRace(ж<Δtesting.T> Ꮡt) {
    nint x = 0;
    var c = NewCond(new Δsync.MutexжLocker(Ꮡ(new Mutex(nil))));
    var done = new channel<bool>(0);
    var cʗ1 = c;
    var doneʗ1 = done;
    goǃ(() => {
        (~cʗ1).L.Lock();
        x = 1;
        cʗ1.Wait();
        if (x != 2) {
            Ꮡt.Error(want2ˢ);
        }
        x = 3;
        cʗ1.Signal();
        (~cʗ1).L.Unlock();
        doneʗ1.ᐸꟷ(true);
    });
    var cʗ2 = c;
    var doneʗ2 = done;
    goǃ(() => {
        (~cʗ2).L.Lock();
        while (ᐧ) {
            if (x == 1) {
                x = 2;
                cʗ2.Signal();
                break;
            }
            (~cʗ2).L.Unlock();
            Δruntime.Gosched();
            (~cʗ2).L.Lock();
        }
        (~cʗ2).L.Unlock();
        doneʗ2.ᐸꟷ(true);
    });
    var cʗ3 = c;
    var doneʗ3 = done;
    goǃ(() => {
        (~cʗ3).L.Lock();
        while (ᐧ) {
            if (x == 2) {
                cʗ3.Wait();
                if (x != 3) {
                    Ꮡt.Error(want3ˢ);
                }
                break;
            }
            if (x == 3) {
                break;
            }
            (~cʗ3).L.Unlock();
            Δruntime.Gosched();
            (~cʗ3).L.Lock();
        }
        (~cʗ3).L.Unlock();
        doneʗ3.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    ᐸꟷ(done);
    ᐸꟷ(done);
}

public static partial void TestCondSignalStealing(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint iters = 0; iters < 1000; iters++) {
        ref var m = ref heap(new Δsync.Mutex(), out var Ꮡm);
        var cond = NewCond(new Δsync.MutexжLocker(Ꮡm));
        // Start a waiter.
        var ch = new channel<EmptyStruct>(0);
        var chʗ1 = ch;
        var condʗ1 = cond;
        goǃ(() => {
            Ꮡm.Lock();
            chʗ1.ᐸꟷ(new EmptyStruct());
            condʗ1.Wait();
            Ꮡm.Unlock();
            chʗ1.ᐸꟷ(new EmptyStruct());
        });
        ᐸꟷ(ch);
        Ꮡm.Lock();
        Ꮡm.Unlock();
        // We know that the waiter is in the cond.Wait() call because we
        // synchronized with it, then acquired/released the mutex it was
        // holding when we synchronized.
        //
        // Start two goroutines that will race: one will broadcast on
        // the cond var, the other will wait on it.
        //
        // The new waiter may or may not get notified, but the first one
        // has to be notified.
        var done = false;
        var condʗ2 = cond;
        goǃ(() => {
            condʗ2.Broadcast();
        });
        var condʗ3 = cond;
        goǃ(() => {
            Ꮡm.Lock();
            while (!done) {
                condʗ3.Wait();
            }
            Ꮡm.Unlock();
        });
        // Check that the first waiter does get signaled.
        ᐸꟷ(ch);
        // Release the second waiter in case it didn't get the
        // broadcast.
        Ꮡm.Lock();
        done = true;
        Ꮡm.Unlock();
        cond.Broadcast();
    }
}

public static void TestCondCopy(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var err = recover();
            if (err == default! || err._<@string>() != "sync.Cond is copied"u8) {
                Ꮡt.Fatalf("got %v, expect sync.Cond is copied"u8, err);
            }
        }, ref ᒐ);
        ref var c = ref heap<Δsync.Cond>(out var Ꮡc);
        c = new Cond(L: new Δsync.MutexжLocker(Ꮡ(new Mutex(nil))));
        Ꮡc.Signal();
        ref var c2 = ref heap(new Δsync.Cond(), out var Ꮡc2);
        reflect.ValueOf(Ꮡc2).Elem().Set(reflect.ValueOf(Ꮡc).Elem()); // c2 := c, hidden from vet
        Ꮡc2.Signal();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkCond1(ж<Δtesting.B> Ꮡb) {
    benchmarkCond(Ꮡb, 1);
}

public static void BenchmarkCond2(ж<Δtesting.B> Ꮡb) {
    benchmarkCond(Ꮡb, 2);
}

public static void BenchmarkCond4(ж<Δtesting.B> Ꮡb) {
    benchmarkCond(Ꮡb, 4);
}

public static void BenchmarkCond8(ж<Δtesting.B> Ꮡb) {
    benchmarkCond(Ꮡb, 8);
}

public static void BenchmarkCond16(ж<Δtesting.B> Ꮡb) {
    benchmarkCond(Ꮡb, 16);
}

public static void BenchmarkCond32(ж<Δtesting.B> Ꮡb) {
    benchmarkCond(Ꮡb, 32);
}

internal static partial void benchmarkCond(ж<Δtesting.B> Ꮡb, nint waiters) {
    var c = NewCond(new Δsync.MutexжLocker(Ꮡ(new Mutex(nil))));
    var done = new channel<bool>(0);
    nint id = 0;
    for (nint routine = 0; routine < waiters + 1; routine++) {
        var cʗ1 = c;
        var doneʗ1 = done;
        goǃ(() => {
            for (nint i = 0; i < Ꮡb.Value.N; i++) {
                (~cʗ1).L.Lock();
                if (id == -1) {
                    (~cʗ1).L.Unlock();
                    break;
                }
                id++;
                if (id == waiters + 1){
                    id = 0;
                    cʗ1.Broadcast();
                } else {
                    cʗ1.Wait();
                }
                (~cʗ1).L.Unlock();
            }
            (~cʗ1).L.Lock();
            id = -1;
            cʗ1.Broadcast();
            (~cʗ1).L.Unlock();
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint routine = 0; routine < waiters + 1; routine++) {
        ᐸꟷ(done);
    }
}

} // end sync_test_package
