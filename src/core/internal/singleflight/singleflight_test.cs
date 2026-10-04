// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go;
using go.sync;
using static go.@internal.singleflight_package;

partial class singleflight_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string barˢ = "bar"u8;
internal static readonly @string barStringˢ = "bar (string)"u8;

public static void TestDo(ж<testing.T> Ꮡt) {
    ref var g = ref heap(new global::go.@internal.singleflight_package.Group(), out var Ꮡg);
    var (v, err, _) = Ꮡg.Do(keyˢ, () => (barˢ, default!));
    {
        @string got = fmt.Sprintf("%v (%T)"u8, v, v);
        @string want = barStringˢ; if (got != want) {
            Ꮡt.Errorf("Do = %v; want %v"u8, got, want);
        }
    }
    if (err != default!) {
        Ꮡt.Errorf("Do error = %v"u8, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string someErrorˢ = "some error"u8;

public static void TestDoErr(ж<testing.T> Ꮡt) {
    ref var g = ref heap(new global::go.@internal.singleflight_package.Group(), out var Ꮡg);
    var someErr = errors.New(someErrorˢ);
    var someErrʗ1 = someErr;
    var (v, err, _) = Ꮡg.Do(keyˢ, () => (default!, someErrʗ1));
    if (!AreEqual(err, someErr)) {
        Ꮡt.Errorf("Do error = %v; want someErr %v"u8, err, someErr);
    }
    if (v != default!) {
        Ꮡt.Errorf("unexpected non-nil value %#v"u8, v);
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDoDupSuppress(ж<testing.T> Ꮡt) {
    ref var g = ref heap(new global::go.@internal.singleflight_package.Group(), out var Ꮡg);
    ref var wg1 = ref heap(new sync.WaitGroup(), out var Ꮡwg1);
    ref var wg2 = ref heap(new sync.WaitGroup(), out var Ꮡwg2);
    var c = new channel<@string>(1);
    ref var calls = ref heap(new atomic.Int32(), out var Ꮡcalls);
    var cʗ1 = c;
    var fn = (any, error) () => {
        if (Ꮡcalls.Add(1) == 1) {
            // First invocation.
            Ꮡwg1.Done();
        }
        @string v = ᐸꟷ(cʗ1);
        cʗ1.ᐸꟷ(v); // pump; make available for any future calls
        time.Sleep(10 * time.Millisecond); // let more goroutines enter Do
        return (v, default!);
    };
    UntypedInt n = 10;
    Ꮡwg1.Add(1);
    for (nint i = 0; i < n; i++) {
        Ꮡwg1.Add(1);
        Ꮡwg2.Add(1);
        var fnʗ1 = fn;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg2.Done, ref ᒐ);
                Ꮡwg1.Done();
                var (v, err, _) = Ꮡg.Do(keyˢ, fnʗ1);
                if (err != default!) {
                    Ꮡt.Errorf("Do error: %v"u8, err);
                    return;
                }
                {
                    var (s, _) = v._<@string>(ᐧ); if (s != "bar"u8) {
                        Ꮡt.Errorf("Do = %T %v; want %q"u8, v, v, barˢ);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg1.Wait();
    // At least one goroutine is in fn now and all of them have at
    // least reached the line before the Do.
    c.ᐸꟷ(barˢ);
    Ꮡwg2.Wait();
    {
        var got = Ꮡcalls.Load(); if (got <= 0 || got >= n) {
            Ꮡt.Errorf("number of calls = %d; want over 0 and less than %d"u8, got, (nint)(n));
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestForgetUnshared(ж<testing.T> Ꮡt) {
    ref var g = ref heap(new global::go.@internal.singleflight_package.Group(), out var Ꮡg);
    ref var firstStarted = ref heap(new sync.WaitGroup(), out var ᏑfirstStarted);
    ref var firstFinished = ref heap(new sync.WaitGroup(), out var ᏑfirstFinished);
    ᏑfirstStarted.Add(1);
    ᏑfirstFinished.Add(1);
    @string key = keyˢ;
    var firstCh = new channel<EmptyStruct>(0);
    var firstChʗ1 = firstCh;
    goǃ(() => {
        var firstChʗ2 = firstChʗ1;
        Ꮡg.Do(key, () => {
            any i = default!;
            error e = default!;
            ᏑfirstStarted.Done();
            ᐸꟷ(firstChʗ2);
            return (i, e);
        });
        ᏑfirstFinished.Done();
    });
    ᏑfirstStarted.Wait();
    Ꮡg.ForgetUnshared(key); // from this point no two function using same key should be executed concurrently
    var secondCh = new channel<EmptyStruct>(0);
    var secondChʗ1 = secondCh;
    goǃ(() => {
        var secondChʗ2 = secondChʗ1;
        Ꮡg.Do(key, () => {
            // Notify that we started
            secondChʗ2.ᐸꟷ(new EmptyStruct());
            ᐸꟷ(secondChʗ2);
            return ((nint)(2), default!);
        });
    });
    ᐸꟷ(secondCh);
    var resultCh = Ꮡg.DoChan(key, () => {
        throw panic("third must not be started");
    });
    if (Ꮡg.ForgetUnshared(key)) {
        Ꮡt.Errorf("Before first goroutine finished, key %q is shared, should return false"u8, key);
    }
    close(firstCh);
    ᏑfirstFinished.Wait();
    if (Ꮡg.ForgetUnshared(key)) {
        Ꮡt.Errorf("After first goroutine finished, key %q is still shared, should return false"u8, key);
    }
    secondCh.ᐸꟷ(new EmptyStruct());
    {
        var result = ᐸꟷ(resultCh); if (!AreEqual(result.Val, (nint)(2))) {
            Ꮡt.Errorf("We should receive result produced by second call, expected: 2, got %d"u8, result.Val);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestDoAndForgetUnsharedRace(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    ref var g = ref heap(new global::go.@internal.singleflight_package.Group(), out var Ꮡg);
    @string key = keyˢ;
    var d = time.Millisecond;
    while (ᐧ) {
        ref var calls = ref heap(new atomic.Int64(), out var Ꮡcalls);
        ref var shared = ref heap(new atomic.Int64(), out var Ꮡshared);
        const nint n = 1000;
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(n);
        for (nint i = 0; i < n; i++) {
            goǃ(() => {
                Ꮡg.Do(key, () => {
                    time.Sleep(d);
                    return (Ꮡcalls.Add(1), default!);
                });
                if (!Ꮡg.ForgetUnshared(key)) {
                    Ꮡshared.Add(1);
                }
                Ꮡwg.Done();
            });
        }
        Ꮡwg.Wait();
        if (Ꮡcalls.Load() != 1) {
            // The goroutines didn't park in g.Do in time,
            // so the key was re-added and may have been shared after the call.
            // Try again with more time to park.
            d *= 2;
            continue;
        }
        // All of the Do calls ended up sharing the first
        // invocation, so the key should have been unused
        // (and therefore unshared) when they returned.
        if (Ꮡshared.Load() > 0) {
            Ꮡt.Errorf("after a single shared Do, ForgetUnshared returned false %d times"u8, Ꮡshared.Load());
        }
        break;
    }
}

} // end singleflight_internal_test_package
