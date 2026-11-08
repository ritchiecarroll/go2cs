// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using context = context_package;
using errors = errors_package;
using runtime = runtime_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using static go.crypto.@internal.fips140cache_package;
using ꓸꓸꓸжvalue = Span<ж<fips140cache_internal_test_package.value>>;

partial class fips140cache_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newCalledˢ = (@string)"new called"u8;
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = "error"u8;

public static partial void TestCache(ж<testing.T> t) {
    var c = @new<global::go.crypto.@internal.fips140cache_package.Cache<key, value>>();
    var checkTrue = (ж<value> _Δp0) => true;
    var checkFalse = (ж<value> _Δp0) => false;
    var newNotCalled = (ж<value>, error) () => {
        t.Helper();
        t.Fatal(newCalledˢ);
        return (default, default);
    };

    ref var k1 = ref heap<ж<key>>(newKey(), out var Ꮡk1);
    var v1 = Ꮡ(new value());

    var v1ʗ1 = v1;
    var (v, err) = c.Get(k1, () => (v1ʗ1, default), checkTrue);
    expectValue(t, v, err, v1);

    // Cached value is returned if check is true.
    (v, err) = c.Get(k1, newNotCalled, checkTrue);
    expectValue(t, v, err, v1);

    // New value is returned and cached if check is false.
    var v2 = Ꮡ(new value());
    var v2ʗ1 = v2;
    (v, err) = c.Get(k1, () => (v2ʗ1, default), checkFalse);
    expectValue(t, v, err, v2);
    (v, err) = c.Get(k1, newNotCalled, checkTrue);
    expectValue(t, v, err, v2);
    expectMapSize(t, c, 1);

    // Cache is evicted when key becomes unreachable.
    waitUnreachable(t, Ꮡk1);
    expectMapSize(t, c, 0);

    // Value is not cached if new returns an error.
    ref var k2 = ref heap<ж<key>>(newKey(), out var Ꮡk2);
    var err1 = errors.New(errorˢ);
    var err1ʗ1 = err1;
    (_, err) = c.Get(k2, () => (default, err1ʗ1), checkTrue);
    if (!AreEqual(err, err1)) {
        t.Errorf("got %v, want %v", err, err1);
    }
    expectMapSize(t, c, 0);

    // Value is not replaced if check is false and new returns an error.
    var v1ʗ2 = v1;
    (v, err) = c.Get(k2, () => (v1ʗ2, default), checkTrue);
    expectValue(t, v, err, v1);
    var err1ʗ2 = err1;
    var v2ʗ2 = v2;
    (_, err) = c.Get(k2, () => (v2ʗ2, err1ʗ2), checkFalse);
    if (!AreEqual(err, err1)) {
        t.Errorf("got %v, want %v", err, err1);
    }
    (v, err) = c.Get(k2, newNotCalled, checkTrue);
    expectValue(t, v, err, v1);
    expectMapSize(t, c, 1);

    // Cache is evicted for keys used only once.
    ref var k3 = ref heap<ж<key>>(newKey(), out var Ꮡk3);
    var v1ʗ3 = v1;
    (v, err) = c.Get(k3, () => (v1ʗ3, default), checkTrue);
    expectValue(t, v, err, v1);
    expectMapSize(t, c, 2);
    waitUnreachable(t, Ꮡk2);
    waitUnreachable(t, Ꮡk3);
    expectMapSize(t, c, 0);

    // When two goroutines race, the returned value may be the new or old one,
    // but the map must shrink to 0.
    var keys = new slice<ж<key>>(100);
    foreach (var i in range(len(keys))) {
        keys[i] = newKey();
        var (v1Δ1, v2Δ1) = (Ꮡ(new value()), Ꮡ(new value()));
        var start = new channel<EmptyStruct>(0);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(2);
        var cʗ1 = c;
        var checkTrueʗ1 = checkTrue;
        var keysʗ1 = keys;
        var startʗ1 = start;
        var v1ʗ4 = v1Δ1;
        var v2ʗ3 = v2Δ1;
        goǃ(() => {
            ᐸꟷ(startʗ1);
            var v1ʗ5 = v1ʗ4;
            var (vΔ1, errΔ1) = cʗ1.Get(keysʗ1[i], () => (v1ʗ5, default), checkTrueʗ1);
            expectValue(t, vΔ1, errΔ1, v1ʗ4, v2ʗ3);
            Ꮡwg.Done();
        });
        var cʗ2 = c;
        var checkTrueʗ2 = checkTrue;
        var keysʗ2 = keys;
        var startʗ2 = start;
        var v1ʗ6 = v1Δ1;
        var v2ʗ4 = v2Δ1;
        goǃ(() => {
            ᐸꟷ(startʗ2);
            var v2ʗ5 = v2ʗ4;
            var (vΔ2, errΔ2) = cʗ2.Get(keysʗ2[i], () => (v2ʗ5, default), checkTrueʗ2);
            expectValue(t, vΔ2, errΔ2, v1ʗ6, v2ʗ4);
            Ꮡwg.Done();
        });
        close(start);
        Ꮡwg.Wait();
        var v3 = Ꮡ(new value());
        var v3ʗ1 = v3;
        var (vΔ3, errΔ3) = c.Get(keys[i], () => (v3ʗ1, default), checkTrue);
        expectValue(t, vΔ3, errΔ3, v1Δ1, v2Δ1);
    }
    foreach (var i in range(len(keys))) {
        waitUnreachable(t, Ꮡ(keys, i));
    }
    expectMapSize(t, c, 0);
}

internal partial struct key {
    internal ж<nint> _;
}

internal partial struct value {
    internal ж<nint> _;
}

// newKey allocates a key value on the heap.
//
//go:noinline
internal static partial ж<key> newKey() {
    return Ꮡ(new key());
}

internal static void expectValue(ж<testing.T> t, ж<value> v, error err, params ꓸꓸꓸжvalue wantʗp) {
    var want = wantʗp.slice();

    t.Helper();
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, w) in range(want)) {
        if (v == w) {
            return;
        }
    }
    t.Errorf("got %p, want %p", v.OrTypedNil(), want);
}

internal static void expectMapSize(ж<testing.T> t, ж<global::go.crypto.@internal.fips140cache_package.Cache<key, value>> c, nint want) {
    t.Helper();
    nint size = default;
    // Loop a few times because the AddCleanup might not be done yet.
    foreach (var _ᴛ1 in range(10)) {
        size = 0;
        c.Ꮡm.Range((any _Δp0, any _Δp1) => {
            size++;
            return true;
        });
        if (size == want) {
            return;
        }
        time.Sleep(100 * time.Millisecond);
    }
    t.Errorf("got %d, want %d", size, want);
}

internal static void waitUnreachable(ж<testing.T> t, ж<ж<key>> Ꮡk) {
    GoFrame ᒐ = default;
    try {
        ref var k = ref Ꮡk.DerefOrNull();

        var (ctx, cancel) = context.WithCancel(t.Context());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var cancelʗ2 = cancel;
        runtime.AddCleanup(k, (ж<nint> _) => {
            cancelʗ2();
        }, (ж<nint>)(nil));
        k = default;
        while (ctx.Err() == nil) {
            runtime.GC();
        }
        if (!AreEqual(ctx.Err(), context.Canceled)) {
            t.Fatal(ctx.Err());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fips140cache_internal_test_package
