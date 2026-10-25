// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using Δsync = sync_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.unique_package;

partial class unique_internal_test_package {

public static void TestCanonMap(ж<testing.T> Ꮡt) {
    testCanonMap(Ꮡt, () => newCanonMap<@string>());
}

public static void TestCanonMapBadHash(ж<testing.T> Ꮡt) {
    testCanonMap(Ꮡt, () => newBadCanonMap<@string>());
}

public static void TestCanonMapTruncHash(ж<testing.T> Ꮡt) {
    testCanonMap(Ꮡt, () => {
        // Stub out the good hash function with a different terrible one
        // (truncated hash). Everything should still work as expected.
        // This is useful to test independently to catch issues with
        // near collisions, where only the last few bits of the hash differ.
        return newTruncCanonMap<@string>();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string loadEmptyˢ = "LoadEmpty"u8;
internal static readonly @string loadOrStoreˢ = "LoadOrStore"u8;
internal static readonly @string sequentialˢ = "Sequential"u8;
internal static readonly @string concurrentUnsharedKeysˢ = "ConcurrentUnsharedKeys"u8;

internal static void testCanonMap(ж<testing.T> Ꮡt, Func<ж<global::go.unique_package.canonMap<@string>>> newMap) {
    Ꮡt.Run(loadEmptyˢ, (ж<testing.T> tΔ1) => {
        var m = newMap();

        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
            expectMissing(tΔ1, s)(m.Load(s));
        }
    });
    Ꮡt.Run(loadOrStoreˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Run(sequentialˢ, (ж<testing.T> tΔ3) => {
            var refsᴷ = new stackAppend<ж<@string>>();
            var m = newMap();

            slice<ж<@string>> refs = default!;
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                expectMissing(tΔ3, s)(m.Load(s));
                refs = append(refsᴷ, refs, expectPresent(tΔ3, s)(m.LoadOrStore(s)));
                expectPresent(tΔ3, s)(m.Load(s));
                expectPresent(tΔ3, s)(m.LoadOrStore(s));
            }
            drainCleanupQueue(tΔ3);
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                expectPresent(tΔ3, s)(m.Load(s));
                expectPresent(tΔ3, s)(m.LoadOrStore(s));
            }
            Δruntime.KeepAlive(refs);
            refs = default!;
            drainCleanupQueue(tΔ3);
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                expectMissing(tΔ3, s)(m.Load(s));
                expectPresent(tΔ3, s)(m.LoadOrStore(s));
            }
        });
        tΔ2.Run(concurrentUnsharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ4) => {
            @string makeKey(@string s, nint id) => s + "-"u8 + strconv.Itoa(id);

            // Expand and shrink the map multiple times to try to get
            // insertions and cleanups to overlap.
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            foreach (var @try in range(3)) {
                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                foreach (var i in range(gmp)) {
                    Ꮡwg.Add(1);
                    var mʗ1 = m;
                    var makeKeyʗ1 = makeKey;
                    goǃ((nint id) => {
                        GoFrame ᒐ = default;
                        try {
                            var refsᴷ = new stackAppend<ж<@string>>();
                            defer(Ꮡwg.Done, ref ᒐ);

                            slice<ж<@string>> refs = default!;
                            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                                @string key = makeKeyʗ1(s, id);
                                if (@try == 0) {
                                    expectMissing(tΔ4, key)(mʗ1.Load(key));
                                }
                                refs = append(refsᴷ, refs, expectPresent(tΔ4, key)(mʗ1.LoadOrStore(key)));
                                expectPresent(tΔ4, key)(mʗ1.Load(key));
                                expectPresent(tΔ4, key)(mʗ1.LoadOrStore(key));
                            }
                            foreach (var (iΔ1, s) in testData.ΔRangeSnapshot()) {
                                @string key = makeKeyʗ1(s, id);
                                expectPresent(tΔ4, key)(mʗ1.Load(key));
                                {
                                    var (got, want) = (expectPresent(tΔ4, key)(mʗ1.LoadOrStore(key)), refs[iΔ1]); if (got != want) {
                                        tΔ4.Errorf("canonical entry %p did not match ref %p"u8, got.OrTypedNil(), want.OrTypedNil());
                                    }
                                }
                            }
                            // N.B. We avoid trying to test entry cleanup here
                            // because it's going to be very flaky, especially
                            // in the bad hash cases.
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    }, i);
                }
                Ꮡwg.Wait();
            }

            // Run an extra GC cycle to de-flake. Sometimes the cleanups
            // fail to run in time, despite drainCleanupQueue.
            //
            // TODO(mknyszek): Figure out why the extra GC is necessary,
            // and what is transiently keeping the cleanups live.
            // * I have confirmed that they are not completely stuck, and
            //   they always eventually run.
            // * I have also confirmed it's not asynchronous preemption
            //   keeping them around (though that is a possibility).
            // * I have confirmed that they are not simply sitting on
            //   the queue, and that drainCleanupQueue is just failing
            //   to actually empty the queue.
            // * I have confirmed that it's not a write barrier that's
            //   keeping it alive, nor is it a weak pointer dereference
            //   (which shades the object during the GC).
            // The corresponding objects do seem to be transiently truly
            // reachable, but I have no idea by what path.
            Δruntime.GC();

            // Drain cleanups so everything is deleted.
            drainCleanupQueue(tΔ4);

            // Double-check that it's all gone.
            foreach (var id in range(gmp)) {
                @string makeKeyΔ1(@string s) => s + "-"u8 + strconv.Itoa(id);
                foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                    @string key = makeKeyΔ1(s);
                    expectMissing(tΔ4, key)(m.Load(key));
                }
            }
        });
    });
}

internal static Action<ж<T>> expectMissing<T>(ж<testing.T> Ꮡt, T key) {
    Ꮡt.Helper();
    return (ж<T> got) => {
        Ꮡt.Helper();

        if (got != nil) {
            Ꮡt.Errorf("expected key %v to be missing from map, got %p"u8, (key).OrTypedNilOf(), got.OrTypedNil());
        }
    };
}

internal static Func<ж<T>, ж<T>> expectPresent<T>(ж<testing.T> Ꮡt, T key) {
    Ꮡt.Helper();
    return (ж<T> got) => {
        Ꮡt.Helper();

        if (got == nil) {
            Ꮡt.Errorf("expected key %v to be present in map, got %p"u8, (key).OrTypedNilOf(), got.OrTypedNil());
        }
        if (got != nil && !AreEqual(got.ValueSlot, key)) {
            Ꮡt.Errorf("key %v is present in map, but canonical version has the wrong value: got %v, want %v"u8, (key).OrTypedNilOf(), (got.ValueSlot).OrTypedNilOf(), (key).OrTypedNilOf());
        }
        return got;
    };
}

// newBadCanonMap creates a new canonMap for the provided key type
// but with an intentionally bad hash function.
internal static ж<global::go.unique_package.canonMap<T>> newBadCanonMap<T>() {
    // Stub out the good hash function with a terrible one.
    // Everything should still work as expected.
    var m = newCanonMap<T>();
    m.Value.hash = uintptr (@unsafe.Pointer _Δp0, uintptr _Δp1) => (uintptr)(0);
    return m;
}

// newTruncCanonMap creates a new canonMap for the provided key type
// but with an intentionally bad hash function.
internal static ж<global::go.unique_package.canonMap<T>> newTruncCanonMap<T>() {
    // Stub out the good hash function with a terrible one.
    // Everything should still work as expected.
    var m = newCanonMap<T>();
    map<@string, nint> mx = default!;
    var mapType = abi.TypeOf(mx).MapType();
    var hasher = mapType.Value.Hasher;
    var hasherʗ1 = hasher;
    m.Value.hash = (@unsafe.Pointer p, uintptr n) => (uintptr)(hasherʗ1(p, n) & (uintptr)(((uintptr)1 << 4) - 1));
    return m;
}

} // end unique_internal_test_package
