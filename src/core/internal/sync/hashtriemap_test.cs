// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using isync = go.@internal.sync_package;
using math = math_package;
using Δruntime = runtime_package;
using strconv = strconv_package;
using sync = go.sync_package;
using testing = testing_package;
using weak = weak_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go;
using go.@internal;
using static go.@internal.sync_internal_test_package;

partial class sync_test_package {

public static void TestHashTrieMap(ж<testing.T> Ꮡt) {
    testHashTrieMap(Ꮡt, () => {
        ref var m = ref heap(new isync.HashTrieMap<@string, nint>(), out var Ꮡm);
        return Ꮡm;
    });
}

public static void TestHashTrieMapBadHash(ж<testing.T> Ꮡt) {
    testHashTrieMap(Ꮡt, () => sync_internal_test_package.NewBadHashTrieMap<@string, nint>());
}

public static void TestHashTrieMapTruncHash(ж<testing.T> Ꮡt) {
    testHashTrieMap(Ꮡt, () => {
        // Stub out the good hash function with a different terrible one
        // (truncated hash). Everything should still work as expected.
        // This is useful to test independently to catch issues with
        // near collisions, where only the last few bits of the hash differ.
        return sync_internal_test_package.NewTruncHashTrieMap<@string, nint>();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string loadEmptyˢ = "LoadEmpty"u8;
internal static readonly @string loadOrStoreˢ = "LoadOrStore"u8;
internal static readonly @string allˢ = "All"u8;
internal static readonly @string clearˢ = "Clear"u8;
internal static readonly @string simpleˢ = "Simple"u8;
internal static readonly @string concurrentˢ = "Concurrent"u8;
internal static readonly @string compareAndDeleteˢ = "CompareAndDelete"u8;
internal static readonly @string oneˢ = "One"u8;
internal static readonly @string multipleˢ = "Multiple"u8;
internal static readonly @string iterateˢ = "Iterate"u8;
internal static readonly @string concurrentUnsharedKeysˢ = "ConcurrentUnsharedKeys"u8;
internal static readonly @string concurrentSharedKeysˢ = "ConcurrentSharedKeys"u8;
internal static readonly @string compareAndSwapˢ = "CompareAndSwap"u8;
internal static readonly @string swapˢ = "Swap"u8;
internal static readonly @string loadAndDeleteˢ = "LoadAndDelete"u8;

internal static void testHashTrieMap(ж<testing.T> Ꮡt, Func<ж<isync.HashTrieMap<@string, nint>>> newMap) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(loadEmptyˢ, (ж<testing.T> tΔ1) => {
        var m = newMap();
        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
            var (ᴛ1, ᴛ2) = m.Load(s);
            expectMissing(tΔ1, s, (nint)(0))(ᴛ1, ᴛ2);
        }
    });
    Ꮡt.Run(loadOrStoreˢ, (ж<testing.T> tΔ2) => {
        var m = newMap();
        foreach (var (i, s) in testData.ΔRangeSnapshot()) {
            var (ᴛ3, ᴛ4) = m.Load(s);
            expectMissing(tΔ2, s, (nint)(0))(ᴛ3, ᴛ4);
            var (ᴛ5, ᴛ6) = m.LoadOrStore(s, i);
            expectStored(tΔ2, s, i)(ᴛ5, ᴛ6);
            var (ᴛ7, ᴛ8) = m.Load(s);
            expectPresent(tΔ2, s, i)(ᴛ7, ᴛ8);
            var (ᴛ9, ᴛ10) = m.LoadOrStore(s, 0);
            expectLoaded(tΔ2, s, i)(ᴛ9, ᴛ10);
        }
        foreach (var (i, s) in testData.ΔRangeSnapshot()) {
            var (ᴛ11, ᴛ12) = m.Load(s);
            expectPresent(tΔ2, s, i)(ᴛ11, ᴛ12);
            var (ᴛ13, ᴛ14) = m.LoadOrStore(s, 0);
            expectLoaded(tΔ2, s, i)(ᴛ13, ᴛ14);
        }
    });
    Ꮡt.Run(allˢ, (ж<testing.T> tΔ3) => {
        var m = newMap();
        testAll(tΔ3, m, testDataMap(testData[..]), (@string _Δp0, nint _Δp1) => true);
    });
    Ꮡt.Run(clearˢ, (ж<testing.T> tΔ4) => {
        tΔ4.Run(simpleˢ, (ж<testing.T> tΔ5) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ15, ᴛ16) = m.Load(s);
                expectMissing(tΔ5, s, (nint)(0))(ᴛ15, ᴛ16);
                var (ᴛ17, ᴛ18) = m.LoadOrStore(s, i);
                expectStored(tΔ5, s, i)(ᴛ17, ᴛ18);
                var (ᴛ19, ᴛ20) = m.Load(s);
                expectPresent(tΔ5, s, i)(ᴛ19, ᴛ20);
                var (ᴛ21, ᴛ22) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ5, s, i)(ᴛ21, ᴛ22);
            }
            m.Clear();
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ23, ᴛ24) = m.Load(s);
                expectMissing(tΔ5, s, (nint)(0))(ᴛ23, ᴛ24);
            }
        });
        tΔ4.Run(concurrentˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ6) => {
            var m = newMap();
            // Load up the map.
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ25, ᴛ26) = m.Load(s);
                expectMissing(tΔ6, s, (nint)(0))(ᴛ25, ᴛ26);
                var (ᴛ27, ᴛ28) = m.LoadOrStore(s, i);
                expectStored(tΔ6, s, i)(ᴛ27, ᴛ28);
            }
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ1 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            // Try a couple things to interfere with the clear.
                            expectNotDeleted(tΔ6, s, math.MaxInt)(mʗ1.CompareAndDelete(s, math.MaxInt));
                            mʗ1.CompareAndSwap(s, i, i + 1); // May succeed or fail; we don't care.
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            // Concurrently clear the map.
            Δruntime.Gosched();
            m.Clear();
            // Wait for workers to finish.
            Ꮡwg.Wait();
            // It should all be empty now.
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ29, ᴛ30) = m.Load(s);
                expectMissing(tΔ6, s, (nint)(0))(ᴛ29, ᴛ30);
            }
        });
    });
    Ꮡt.Run(compareAndDeleteˢ, (ж<testing.T> tΔ7) => {
        tΔ7.Run(allˢ, (ж<testing.T> tΔ8) => {
            var m = newMap();
            foreach (var _ᴛ1 in range(3)) {
                foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ31, ᴛ32) = m.Load(s);
                    expectMissing(tΔ8, s, (nint)(0))(ᴛ31, ᴛ32);
                    var (ᴛ33, ᴛ34) = m.LoadOrStore(s, i);
                    expectStored(tΔ8, s, i)(ᴛ33, ᴛ34);
                    var (ᴛ35, ᴛ36) = m.Load(s);
                    expectPresent(tΔ8, s, i)(ᴛ35, ᴛ36);
                    var (ᴛ37, ᴛ38) = m.LoadOrStore(s, 0);
                    expectLoaded(tΔ8, s, i)(ᴛ37, ᴛ38);
                }
                foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ39, ᴛ40) = m.Load(s);
                    expectPresent(tΔ8, s, i)(ᴛ39, ᴛ40);
                    expectNotDeleted(tΔ8, s, math.MaxInt)(m.CompareAndDelete(s, math.MaxInt));
                    expectDeleted(tΔ8, s, i)(m.CompareAndDelete(s, i));
                    expectNotDeleted(tΔ8, s, i)(m.CompareAndDelete(s, i));
                    var (ᴛ41, ᴛ42) = m.Load(s);
                    expectMissing(tΔ8, s, (nint)(0))(ᴛ41, ᴛ42);
                }
                foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ43, ᴛ44) = m.Load(s);
                    expectMissing(tΔ8, s, (nint)(0))(ᴛ43, ᴛ44);
                }
            }
        });
        tΔ7.Run(oneˢ, (ж<testing.T> tΔ9) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ45, ᴛ46) = m.Load(s);
                expectMissing(tΔ9, s, (nint)(0))(ᴛ45, ᴛ46);
                var (ᴛ47, ᴛ48) = m.LoadOrStore(s, i);
                expectStored(tΔ9, s, i)(ᴛ47, ᴛ48);
                var (ᴛ49, ᴛ50) = m.Load(s);
                expectPresent(tΔ9, s, i)(ᴛ49, ᴛ50);
                var (ᴛ51, ᴛ52) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ9, s, i)(ᴛ51, ᴛ52);
            }
            expectNotDeleted(tΔ9, testData[15], math.MaxInt)(m.CompareAndDelete(testData[15], math.MaxInt));
            expectDeleted(tΔ9, testData[15], 15)(m.CompareAndDelete(testData[15], 15));
            expectNotDeleted(tΔ9, testData[15], 15)(m.CompareAndDelete(testData[15], 15));
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 15){
                    var (ᴛ53, ᴛ54) = m.Load(s);
                    expectMissing(tΔ9, s, (nint)(0))(ᴛ53, ᴛ54);
                } else {
                    var (ᴛ55, ᴛ56) = m.Load(s);
                    expectPresent(tΔ9, s, i)(ᴛ55, ᴛ56);
                }
            }
        });
        tΔ7.Run(multipleˢ, (ж<testing.T> tΔ10) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ57, ᴛ58) = m.Load(s);
                expectMissing(tΔ10, s, (nint)(0))(ᴛ57, ᴛ58);
                var (ᴛ59, ᴛ60) = m.LoadOrStore(s, i);
                expectStored(tΔ10, s, i)(ᴛ59, ᴛ60);
                var (ᴛ61, ᴛ62) = m.Load(s);
                expectPresent(tΔ10, s, i)(ᴛ61, ᴛ62);
                var (ᴛ63, ᴛ64) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ10, s, i)(ᴛ63, ᴛ64);
            }
            foreach (var (_, i) in new nint[]{1, 105, 6, 85}.slice()) {
                expectNotDeleted(tΔ10, testData[i], math.MaxInt)(m.CompareAndDelete(testData[i], math.MaxInt));
                expectDeleted(tΔ10, testData[i], i)(m.CompareAndDelete(testData[i], i));
                expectNotDeleted(tΔ10, testData[i], i)(m.CompareAndDelete(testData[i], i));
            }
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 1 || i == 105 || i == 6 || i == 85){
                    var (ᴛ65, ᴛ66) = m.Load(s);
                    expectMissing(tΔ10, s, (nint)(0))(ᴛ65, ᴛ66);
                } else {
                    var (ᴛ67, ᴛ68) = m.Load(s);
                    expectPresent(tΔ10, s, i)(ᴛ67, ᴛ68);
                }
            }
        });
        tΔ7.Run(iterateˢ, (ж<testing.T> tΔ11) => {
            var m = newMap();
            var mʗ2 = m;
            testAll(tΔ11, m, testDataMap(testData[..]), (@string s, nint i) => {
                expectDeleted(tΔ11, s, i)(mʗ2.CompareAndDelete(s, i));
                return true;
            });
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ69, ᴛ70) = m.Load(s);
                expectMissing(tΔ11, s, (nint)(0))(ᴛ69, ᴛ70);
            }
        });
        tΔ7.Run(concurrentUnsharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ12) => {
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ3 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        @string makeKey(@string s) => s + "-"u8 + strconv.Itoa(id);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ71, ᴛ72) = mʗ3.Load(key);
                            expectMissing(tΔ12, key, (nint)(0))(ᴛ71, ᴛ72);
                            var (ᴛ73, ᴛ74) = mʗ3.LoadOrStore(key, id);
                            expectStored(tΔ12, key, id)(ᴛ73, ᴛ74);
                            var (ᴛ75, ᴛ76) = mʗ3.Load(key);
                            expectPresent(tΔ12, key, id)(ᴛ75, ᴛ76);
                            var (ᴛ77, ᴛ78) = mʗ3.LoadOrStore(key, 0);
                            expectLoaded(tΔ12, key, id)(ᴛ77, ᴛ78);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ79, ᴛ80) = mʗ3.Load(key);
                            expectPresent(tΔ12, key, id)(ᴛ79, ᴛ80);
                            expectDeleted(tΔ12, key, id)(mʗ3.CompareAndDelete(key, id));
                            var (ᴛ81, ᴛ82) = mʗ3.Load(key);
                            expectMissing(tΔ12, key, (nint)(0))(ᴛ81, ᴛ82);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ83, ᴛ84) = mʗ3.Load(key);
                            expectMissing(tΔ12, key, (nint)(0))(ᴛ83, ᴛ84);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
        tΔ7.Run(concurrentSharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ13) => {
            var m = newMap();
            // Load up the map.
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ85, ᴛ86) = m.Load(s);
                expectMissing(tΔ13, s, (nint)(0))(ᴛ85, ᴛ86);
                var (ᴛ87, ᴛ88) = m.LoadOrStore(s, i);
                expectStored(tΔ13, s, i)(ᴛ87, ᴛ88);
            }
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ4 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        foreach (var (iΔ1, s) in testData.ΔRangeSnapshot()) {
                            expectNotDeleted(tΔ13, s, math.MaxInt)(mʗ4.CompareAndDelete(s, math.MaxInt));
                            mʗ4.CompareAndDelete(s, iΔ1);
                            var (ᴛ89, ᴛ90) = mʗ4.Load(s);
                            expectMissing(tΔ13, s, (nint)(0))(ᴛ89, ᴛ90);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            var (ᴛ91, ᴛ92) = mʗ4.Load(s);
                            expectMissing(tΔ13, s, (nint)(0))(ᴛ91, ᴛ92);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
    });
    Ꮡt.Run(compareAndSwapˢ, (ж<testing.T> tΔ14) => {
        tΔ14.Run(allˢ, (ж<testing.T> tΔ15) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ93, ᴛ94) = m.Load(s);
                expectMissing(tΔ15, s, (nint)(0))(ᴛ93, ᴛ94);
                var (ᴛ95, ᴛ96) = m.LoadOrStore(s, i);
                expectStored(tΔ15, s, i)(ᴛ95, ᴛ96);
                var (ᴛ97, ᴛ98) = m.Load(s);
                expectPresent(tΔ15, s, i)(ᴛ97, ᴛ98);
                var (ᴛ99, ᴛ100) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ15, s, i)(ᴛ99, ᴛ100);
            }
            foreach (var j in range(3)) {
                foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ101, ᴛ102) = m.Load(s);
                    expectPresent(tΔ15, s, i + j)(ᴛ101, ᴛ102);
                    expectNotSwapped<@string, nint>(tΔ15, s, math.MaxInt, i + j + 1)(m.CompareAndSwap(s, math.MaxInt, i + j + 1));
                    expectSwapped(tΔ15, s, i, i + j + 1)(m.CompareAndSwap(s, i + j, i + j + 1));
                    expectNotSwapped(tΔ15, s, i + j, i + j + 1)(m.CompareAndSwap(s, i + j, i + j + 1));
                    var (ᴛ103, ᴛ104) = m.Load(s);
                    expectPresent(tΔ15, s, i + j + 1)(ᴛ103, ᴛ104);
                }
            }
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ105, ᴛ106) = m.Load(s);
                expectPresent(tΔ15, s, i + 3)(ᴛ105, ᴛ106);
            }
        });
        tΔ14.Run(oneˢ, (ж<testing.T> tΔ16) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ107, ᴛ108) = m.Load(s);
                expectMissing(tΔ16, s, (nint)(0))(ᴛ107, ᴛ108);
                var (ᴛ109, ᴛ110) = m.LoadOrStore(s, i);
                expectStored(tΔ16, s, i)(ᴛ109, ᴛ110);
                var (ᴛ111, ᴛ112) = m.Load(s);
                expectPresent(tΔ16, s, i)(ᴛ111, ᴛ112);
                var (ᴛ113, ᴛ114) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ16, s, i)(ᴛ113, ᴛ114);
            }
            expectNotSwapped<@string, nint>(tΔ16, testData[15], math.MaxInt, 16)(m.CompareAndSwap(testData[15], math.MaxInt, 16));
            expectSwapped(tΔ16, testData[15], 15, 16)(m.CompareAndSwap(testData[15], 15, 16));
            expectNotSwapped(tΔ16, testData[15], 15, 16)(m.CompareAndSwap(testData[15], 15, 16));
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 15){
                    var (ᴛ115, ᴛ116) = m.Load(s);
                    expectPresent(tΔ16, s, (nint)(16))(ᴛ115, ᴛ116);
                } else {
                    var (ᴛ117, ᴛ118) = m.Load(s);
                    expectPresent(tΔ16, s, i)(ᴛ117, ᴛ118);
                }
            }
        });
        tΔ14.Run(multipleˢ, (ж<testing.T> tΔ17) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ119, ᴛ120) = m.Load(s);
                expectMissing(tΔ17, s, (nint)(0))(ᴛ119, ᴛ120);
                var (ᴛ121, ᴛ122) = m.LoadOrStore(s, i);
                expectStored(tΔ17, s, i)(ᴛ121, ᴛ122);
                var (ᴛ123, ᴛ124) = m.Load(s);
                expectPresent(tΔ17, s, i)(ᴛ123, ᴛ124);
                var (ᴛ125, ᴛ126) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ17, s, i)(ᴛ125, ᴛ126);
            }
            foreach (var (_, i) in new nint[]{1, 105, 6, 85}.slice()) {
                expectNotSwapped<@string, nint>(tΔ17, testData[i], math.MaxInt, i + 1)(m.CompareAndSwap(testData[i], math.MaxInt, i + 1));
                expectSwapped(tΔ17, testData[i], i, i + 1)(m.CompareAndSwap(testData[i], i, i + 1));
                expectNotSwapped(tΔ17, testData[i], i, i + 1)(m.CompareAndSwap(testData[i], i, i + 1));
            }
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 1 || i == 105 || i == 6 || i == 85){
                    var (ᴛ127, ᴛ128) = m.Load(s);
                    expectPresent(tΔ17, s, i + 1)(ᴛ127, ᴛ128);
                } else {
                    var (ᴛ129, ᴛ130) = m.Load(s);
                    expectPresent(tΔ17, s, i)(ᴛ129, ᴛ130);
                }
            }
        });
        tΔ14.Run(concurrentUnsharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ18) => {
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ5 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        @string makeKey(@string s) => s + "-"u8 + strconv.Itoa(id);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ131, ᴛ132) = mʗ5.Load(key);
                            expectMissing(tΔ18, key, (nint)(0))(ᴛ131, ᴛ132);
                            var (ᴛ133, ᴛ134) = mʗ5.LoadOrStore(key, id);
                            expectStored(tΔ18, key, id)(ᴛ133, ᴛ134);
                            var (ᴛ135, ᴛ136) = mʗ5.Load(key);
                            expectPresent(tΔ18, key, id)(ᴛ135, ᴛ136);
                            var (ᴛ137, ᴛ138) = mʗ5.LoadOrStore(key, 0);
                            expectLoaded(tΔ18, key, id)(ᴛ137, ᴛ138);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ139, ᴛ140) = mʗ5.Load(key);
                            expectPresent(tΔ18, key, id)(ᴛ139, ᴛ140);
                            expectSwapped(tΔ18, key, id, id + 1)(mʗ5.CompareAndSwap(key, id, id + 1));
                            var (ᴛ141, ᴛ142) = mʗ5.Load(key);
                            expectPresent(tΔ18, key, id + 1)(ᴛ141, ᴛ142);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ143, ᴛ144) = mʗ5.Load(key);
                            expectPresent(tΔ18, key, id + 1)(ᴛ143, ᴛ144);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
        tΔ14.Run("ConcurrentUnsharedKeysWithDelete"u8, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ19) => {
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ6 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        @string makeKey(@string s) => s + "-"u8 + strconv.Itoa(id);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ145, ᴛ146) = mʗ6.Load(key);
                            expectMissing(tΔ19, key, (nint)(0))(ᴛ145, ᴛ146);
                            var (ᴛ147, ᴛ148) = mʗ6.LoadOrStore(key, id);
                            expectStored(tΔ19, key, id)(ᴛ147, ᴛ148);
                            var (ᴛ149, ᴛ150) = mʗ6.Load(key);
                            expectPresent(tΔ19, key, id)(ᴛ149, ᴛ150);
                            var (ᴛ151, ᴛ152) = mʗ6.LoadOrStore(key, 0);
                            expectLoaded(tΔ19, key, id)(ᴛ151, ᴛ152);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ153, ᴛ154) = mʗ6.Load(key);
                            expectPresent(tΔ19, key, id)(ᴛ153, ᴛ154);
                            expectSwapped(tΔ19, key, id, id + 1)(mʗ6.CompareAndSwap(key, id, id + 1));
                            var (ᴛ155, ᴛ156) = mʗ6.Load(key);
                            expectPresent(tΔ19, key, id + 1)(ᴛ155, ᴛ156);
                            expectDeleted(tΔ19, key, id + 1)(mʗ6.CompareAndDelete(key, id + 1));
                            expectNotSwapped(tΔ19, key, id + 1, id + 2)(mʗ6.CompareAndSwap(key, id + 1, id + 2));
                            expectNotDeleted(tΔ19, key, id + 1)(mʗ6.CompareAndDelete(key, id + 1));
                            var (ᴛ157, ᴛ158) = mʗ6.Load(key);
                            expectMissing(tΔ19, key, (nint)(0))(ᴛ157, ᴛ158);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ159, ᴛ160) = mʗ6.Load(key);
                            expectMissing(tΔ19, key, (nint)(0))(ᴛ159, ᴛ160);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
        tΔ14.Run(concurrentSharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ20) => {
            var m = newMap();
            // Load up the map.
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ161, ᴛ162) = m.Load(s);
                expectMissing(tΔ20, s, (nint)(0))(ᴛ161, ᴛ162);
                var (ᴛ163, ᴛ164) = m.LoadOrStore(s, i);
                expectStored(tΔ20, s, i)(ᴛ163, ᴛ164);
            }
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ7 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        foreach (var (iΔ1, s) in testData.ΔRangeSnapshot()) {
                            expectNotSwapped<@string, nint>(tΔ20, s, math.MaxInt, iΔ1 + 1)(mʗ7.CompareAndSwap(s, math.MaxInt, iΔ1 + 1));
                            mʗ7.CompareAndSwap(s, iΔ1, iΔ1 + 1);
                            var (ᴛ165, ᴛ166) = mʗ7.Load(s);
                            expectPresent(tΔ20, s, iΔ1 + 1)(ᴛ165, ᴛ166);
                        }
                        foreach (var (iΔ2, s) in testData.ΔRangeSnapshot()) {
                            var (ᴛ167, ᴛ168) = mʗ7.Load(s);
                            expectPresent(tΔ20, s, iΔ2 + 1)(ᴛ167, ᴛ168);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
    });
    Ꮡt.Run(swapˢ, (ж<testing.T> tΔ21) => {
        tΔ21.Run(allˢ, (ж<testing.T> tΔ22) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ169, ᴛ170) = m.Load(s);
                expectMissing(tΔ22, s, (nint)(0))(ᴛ169, ᴛ170);
                var (ᴛ171, ᴛ172) = m.Swap(s, i);
                expectNotLoadedFromSwap(tΔ22, s, i)(ᴛ171, ᴛ172);
                var (ᴛ173, ᴛ174) = m.Load(s);
                expectPresent(tΔ22, s, i)(ᴛ173, ᴛ174);
                var (ᴛ175, ᴛ176) = m.Swap(s, i);
                expectLoadedFromSwap(tΔ22, s, i, i)(ᴛ175, ᴛ176);
            }
            foreach (var j in range(3)) {
                foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ177, ᴛ178) = m.Load(s);
                    expectPresent(tΔ22, s, i + j)(ᴛ177, ᴛ178);
                    var (ᴛ179, ᴛ180) = m.Swap(s, i + j + 1);
                    expectLoadedFromSwap(tΔ22, s, i + j, i + j + 1)(ᴛ179, ᴛ180);
                    var (ᴛ181, ᴛ182) = m.Load(s);
                    expectPresent(tΔ22, s, i + j + 1)(ᴛ181, ᴛ182);
                }
            }
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ183, ᴛ184) = m.Swap(s, i + 3);
                expectLoadedFromSwap(tΔ22, s, i + 3, i + 3)(ᴛ183, ᴛ184);
            }
        });
        tΔ21.Run(oneˢ, (ж<testing.T> tΔ23) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ185, ᴛ186) = m.Load(s);
                expectMissing(tΔ23, s, (nint)(0))(ᴛ185, ᴛ186);
                var (ᴛ187, ᴛ188) = m.Swap(s, i);
                expectNotLoadedFromSwap(tΔ23, s, i)(ᴛ187, ᴛ188);
                var (ᴛ189, ᴛ190) = m.Load(s);
                expectPresent(tΔ23, s, i)(ᴛ189, ᴛ190);
                var (ᴛ191, ᴛ192) = m.Swap(s, i);
                expectLoadedFromSwap(tΔ23, s, i, i)(ᴛ191, ᴛ192);
            }
            var (ᴛ193, ᴛ194) = m.Swap(testData[15], 16);
            expectLoadedFromSwap(tΔ23, testData[15], (nint)(15), (nint)(16))(ᴛ193, ᴛ194);
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 15){
                    var (ᴛ195, ᴛ196) = m.Load(s);
                    expectPresent(tΔ23, s, (nint)(16))(ᴛ195, ᴛ196);
                } else {
                    var (ᴛ197, ᴛ198) = m.Load(s);
                    expectPresent(tΔ23, s, i)(ᴛ197, ᴛ198);
                }
            }
        });
        tΔ21.Run(multipleˢ, (ж<testing.T> tΔ24) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ199, ᴛ200) = m.Load(s);
                expectMissing(tΔ24, s, (nint)(0))(ᴛ199, ᴛ200);
                var (ᴛ201, ᴛ202) = m.Swap(s, i);
                expectNotLoadedFromSwap(tΔ24, s, i)(ᴛ201, ᴛ202);
                var (ᴛ203, ᴛ204) = m.Load(s);
                expectPresent(tΔ24, s, i)(ᴛ203, ᴛ204);
                var (ᴛ205, ᴛ206) = m.Swap(s, i);
                expectLoadedFromSwap(tΔ24, s, i, i)(ᴛ205, ᴛ206);
            }
            foreach (var (_, i) in new nint[]{1, 105, 6, 85}.slice()) {
                var (ᴛ207, ᴛ208) = m.Swap(testData[i], i + 1);
                expectLoadedFromSwap(tΔ24, testData[i], i, i + 1)(ᴛ207, ᴛ208);
            }
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 1 || i == 105 || i == 6 || i == 85){
                    var (ᴛ209, ᴛ210) = m.Load(s);
                    expectPresent(tΔ24, s, i + 1)(ᴛ209, ᴛ210);
                } else {
                    var (ᴛ211, ᴛ212) = m.Load(s);
                    expectPresent(tΔ24, s, i)(ᴛ211, ᴛ212);
                }
            }
        });
        tΔ21.Run(concurrentUnsharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ25) => {
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ8 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        @string makeKey(@string s) => s + "-"u8 + strconv.Itoa(id);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ213, ᴛ214) = mʗ8.Load(key);
                            expectMissing(tΔ25, key, (nint)(0))(ᴛ213, ᴛ214);
                            var (ᴛ215, ᴛ216) = mʗ8.Swap(key, id);
                            expectNotLoadedFromSwap(tΔ25, key, id)(ᴛ215, ᴛ216);
                            var (ᴛ217, ᴛ218) = mʗ8.Load(key);
                            expectPresent(tΔ25, key, id)(ᴛ217, ᴛ218);
                            var (ᴛ219, ᴛ220) = mʗ8.Swap(key, id);
                            expectLoadedFromSwap(tΔ25, key, id, id)(ᴛ219, ᴛ220);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ221, ᴛ222) = mʗ8.Load(key);
                            expectPresent(tΔ25, key, id)(ᴛ221, ᴛ222);
                            var (ᴛ223, ᴛ224) = mʗ8.Swap(key, id + 1);
                            expectLoadedFromSwap(tΔ25, key, id, id + 1)(ᴛ223, ᴛ224);
                            var (ᴛ225, ᴛ226) = mʗ8.Load(key);
                            expectPresent(tΔ25, key, id + 1)(ᴛ225, ᴛ226);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ227, ᴛ228) = mʗ8.Load(key);
                            expectPresent(tΔ25, key, id + 1)(ᴛ227, ᴛ228);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
        tΔ21.Run("ConcurrentUnsharedKeysWithDelete"u8, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ26) => {
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ9 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        @string makeKey(@string s) => s + "-"u8 + strconv.Itoa(id);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ229, ᴛ230) = mʗ9.Load(key);
                            expectMissing(tΔ26, key, (nint)(0))(ᴛ229, ᴛ230);
                            var (ᴛ231, ᴛ232) = mʗ9.Swap(key, id);
                            expectNotLoadedFromSwap(tΔ26, key, id)(ᴛ231, ᴛ232);
                            var (ᴛ233, ᴛ234) = mʗ9.Load(key);
                            expectPresent(tΔ26, key, id)(ᴛ233, ᴛ234);
                            var (ᴛ235, ᴛ236) = mʗ9.Swap(key, id);
                            expectLoadedFromSwap(tΔ26, key, id, id)(ᴛ235, ᴛ236);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ237, ᴛ238) = mʗ9.Load(key);
                            expectPresent(tΔ26, key, id)(ᴛ237, ᴛ238);
                            var (ᴛ239, ᴛ240) = mʗ9.Swap(key, id + 1);
                            expectLoadedFromSwap(tΔ26, key, id, id + 1)(ᴛ239, ᴛ240);
                            var (ᴛ241, ᴛ242) = mʗ9.Load(key);
                            expectPresent(tΔ26, key, id + 1)(ᴛ241, ᴛ242);
                            expectDeleted(tΔ26, key, id + 1)(mʗ9.CompareAndDelete(key, id + 1));
                            var (ᴛ243, ᴛ244) = mʗ9.Swap(key, id + 2);
                            expectNotLoadedFromSwap(tΔ26, key, id + 2)(ᴛ243, ᴛ244);
                            var (ᴛ245, ᴛ246) = mʗ9.Load(key);
                            expectPresent(tΔ26, key, id + 2)(ᴛ245, ᴛ246);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ247, ᴛ248) = mʗ9.Load(key);
                            expectPresent(tΔ26, key, id + 2)(ᴛ247, ᴛ248);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
        tΔ21.Run(concurrentSharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ27) => {
            var m = newMap();
            // Load up the map.
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ249, ᴛ250) = m.Load(s);
                expectMissing(tΔ27, s, (nint)(0))(ᴛ249, ᴛ250);
                var (ᴛ251, ᴛ252) = m.LoadOrStore(s, i);
                expectStored(tΔ27, s, i)(ᴛ251, ᴛ252);
            }
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ10 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        foreach (var (iΔ1, s) in testData.ΔRangeSnapshot()) {
                            mʗ10.Swap(s, iΔ1 + 1);
                            var (ᴛ253, ᴛ254) = mʗ10.Load(s);
                            expectPresent(tΔ27, s, iΔ1 + 1)(ᴛ253, ᴛ254);
                        }
                        foreach (var (iΔ2, s) in testData.ΔRangeSnapshot()) {
                            var (ᴛ255, ᴛ256) = mʗ10.Load(s);
                            expectPresent(tΔ27, s, iΔ2 + 1)(ᴛ255, ᴛ256);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
    });
    Ꮡt.Run(loadAndDeleteˢ, (ж<testing.T> tΔ28) => {
        tΔ28.Run(allˢ, (ж<testing.T> tΔ29) => {
            var m = newMap();
            foreach (var _ᴛ2 in range(3)) {
                foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ257, ᴛ258) = m.Load(s);
                    expectMissing(tΔ29, s, (nint)(0))(ᴛ257, ᴛ258);
                    var (ᴛ259, ᴛ260) = m.LoadOrStore(s, i);
                    expectStored(tΔ29, s, i)(ᴛ259, ᴛ260);
                    var (ᴛ261, ᴛ262) = m.Load(s);
                    expectPresent(tΔ29, s, i)(ᴛ261, ᴛ262);
                    var (ᴛ263, ᴛ264) = m.LoadOrStore(s, 0);
                    expectLoaded(tΔ29, s, i)(ᴛ263, ᴛ264);
                }
                foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ265, ᴛ266) = m.Load(s);
                    expectPresent(tΔ29, s, i)(ᴛ265, ᴛ266);
                    var (ᴛ267, ᴛ268) = m.LoadAndDelete(s);
                    expectLoadedFromDelete(tΔ29, s, i)(ᴛ267, ᴛ268);
                    var (ᴛ269, ᴛ270) = m.Load(s);
                    expectMissing(tΔ29, s, (nint)(0))(ᴛ269, ᴛ270);
                    var (ᴛ271, ᴛ272) = m.LoadAndDelete(s);
                    expectNotLoadedFromDelete(tΔ29, s, (nint)(0))(ᴛ271, ᴛ272);
                }
                foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                    var (ᴛ273, ᴛ274) = m.Load(s);
                    expectMissing(tΔ29, s, (nint)(0))(ᴛ273, ᴛ274);
                }
            }
        });
        tΔ28.Run(oneˢ, (ж<testing.T> tΔ30) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ275, ᴛ276) = m.Load(s);
                expectMissing(tΔ30, s, (nint)(0))(ᴛ275, ᴛ276);
                var (ᴛ277, ᴛ278) = m.LoadOrStore(s, i);
                expectStored(tΔ30, s, i)(ᴛ277, ᴛ278);
                var (ᴛ279, ᴛ280) = m.Load(s);
                expectPresent(tΔ30, s, i)(ᴛ279, ᴛ280);
                var (ᴛ281, ᴛ282) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ30, s, i)(ᴛ281, ᴛ282);
            }
            var (ᴛ283, ᴛ284) = m.Load(testData[15]);
            expectPresent(tΔ30, testData[15], (nint)(15))(ᴛ283, ᴛ284);
            var (ᴛ285, ᴛ286) = m.LoadAndDelete(testData[15]);
            expectLoadedFromDelete(tΔ30, testData[15], (nint)(15))(ᴛ285, ᴛ286);
            var (ᴛ287, ᴛ288) = m.Load(testData[15]);
            expectMissing(tΔ30, testData[15], (nint)(0))(ᴛ287, ᴛ288);
            var (ᴛ289, ᴛ290) = m.LoadAndDelete(testData[15]);
            expectNotLoadedFromDelete(tΔ30, testData[15], (nint)(0))(ᴛ289, ᴛ290);
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 15){
                    var (ᴛ291, ᴛ292) = m.Load(s);
                    expectMissing(tΔ30, s, (nint)(0))(ᴛ291, ᴛ292);
                } else {
                    var (ᴛ293, ᴛ294) = m.Load(s);
                    expectPresent(tΔ30, s, i)(ᴛ293, ᴛ294);
                }
            }
        });
        tΔ28.Run(multipleˢ, (ж<testing.T> tΔ31) => {
            var m = newMap();
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ295, ᴛ296) = m.Load(s);
                expectMissing(tΔ31, s, (nint)(0))(ᴛ295, ᴛ296);
                var (ᴛ297, ᴛ298) = m.LoadOrStore(s, i);
                expectStored(tΔ31, s, i)(ᴛ297, ᴛ298);
                var (ᴛ299, ᴛ300) = m.Load(s);
                expectPresent(tΔ31, s, i)(ᴛ299, ᴛ300);
                var (ᴛ301, ᴛ302) = m.LoadOrStore(s, 0);
                expectLoaded(tΔ31, s, i)(ᴛ301, ᴛ302);
            }
            foreach (var (_, i) in new nint[]{1, 105, 6, 85}.slice()) {
                var (ᴛ303, ᴛ304) = m.Load(testData[i]);
                expectPresent(tΔ31, testData[i], i)(ᴛ303, ᴛ304);
                var (ᴛ305, ᴛ306) = m.LoadAndDelete(testData[i]);
                expectLoadedFromDelete(tΔ31, testData[i], i)(ᴛ305, ᴛ306);
                var (ᴛ307, ᴛ308) = m.Load(testData[i]);
                expectMissing(tΔ31, testData[i], (nint)(0))(ᴛ307, ᴛ308);
                var (ᴛ309, ᴛ310) = m.LoadAndDelete(testData[i]);
                expectNotLoadedFromDelete(tΔ31, testData[i], (nint)(0))(ᴛ309, ᴛ310);
            }
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                if (i == 1 || i == 105 || i == 6 || i == 85){
                    var (ᴛ311, ᴛ312) = m.Load(s);
                    expectMissing(tΔ31, s, (nint)(0))(ᴛ311, ᴛ312);
                } else {
                    var (ᴛ313, ᴛ314) = m.Load(s);
                    expectPresent(tΔ31, s, i)(ᴛ313, ᴛ314);
                }
            }
        });
        tΔ28.Run(iterateˢ, (ж<testing.T> tΔ32) => {
            var m = newMap();
            var mʗ11 = m;
            testAll(tΔ32, m, testDataMap(testData[..]), (@string s, nint i) => {
                var (ᴛ315, ᴛ316) = mʗ11.LoadAndDelete(s);
                expectLoadedFromDelete(tΔ32, s, i)(ᴛ315, ᴛ316);
                return true;
            });
            foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ317, ᴛ318) = m.Load(s);
                expectMissing(tΔ32, s, (nint)(0))(ᴛ317, ᴛ318);
            }
        });
        tΔ28.Run(concurrentUnsharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ33) => {
            var m = newMap();
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ12 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        @string makeKey(@string s) => s + "-"u8 + strconv.Itoa(id);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ319, ᴛ320) = mʗ12.Load(key);
                            expectMissing(tΔ33, key, (nint)(0))(ᴛ319, ᴛ320);
                            var (ᴛ321, ᴛ322) = mʗ12.LoadOrStore(key, id);
                            expectStored(tΔ33, key, id)(ᴛ321, ᴛ322);
                            var (ᴛ323, ᴛ324) = mʗ12.Load(key);
                            expectPresent(tΔ33, key, id)(ᴛ323, ᴛ324);
                            var (ᴛ325, ᴛ326) = mʗ12.LoadOrStore(key, 0);
                            expectLoaded(tΔ33, key, id)(ᴛ325, ᴛ326);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ327, ᴛ328) = mʗ12.Load(key);
                            expectPresent(tΔ33, key, id)(ᴛ327, ᴛ328);
                            var (ᴛ329, ᴛ330) = mʗ12.LoadAndDelete(key);
                            expectLoadedFromDelete(tΔ33, key, id)(ᴛ329, ᴛ330);
                            var (ᴛ331, ᴛ332) = mʗ12.Load(key);
                            expectMissing(tΔ33, key, (nint)(0))(ᴛ331, ᴛ332);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            @string key = makeKey(s);
                            var (ᴛ333, ᴛ334) = mʗ12.Load(key);
                            expectMissing(tΔ33, key, (nint)(0))(ᴛ333, ᴛ334);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
        tΔ28.Run(concurrentSharedKeysˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ34) => {
            var m = newMap();
            // Load up the map.
            foreach (var (i, s) in testData.ΔRangeSnapshot()) {
                var (ᴛ335, ᴛ336) = m.Load(s);
                expectMissing(tΔ34, s, (nint)(0))(ᴛ335, ᴛ336);
                var (ᴛ337, ᴛ338) = m.LoadOrStore(s, i);
                expectStored(tΔ34, s, i)(ᴛ337, ᴛ338);
            }
            nint gmp = Δruntime.GOMAXPROCS(-1);
            ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
            foreach (var i in range(gmp)) {
                Ꮡwg.Add(1);
                var mʗ13 = m;
                goǃ((nint id) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            mʗ13.LoadAndDelete(s);
                            var (ᴛ339, ᴛ340) = mʗ13.Load(s);
                            expectMissing(tΔ34, s, (nint)(0))(ᴛ339, ᴛ340);
                        }
                        foreach (var (_, s) in testData.ΔRangeSnapshot()) {
                            var (ᴛ341, ᴛ342) = mʗ13.Load(s);
                            expectMissing(tΔ34, s, (nint)(0))(ᴛ341, ᴛ342);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, i);
            }
            Ꮡwg.Wait();
        });
    });
}

internal static void testAll<K, V>(ж<testing.T> Ꮡt, ж<isync.HashTrieMap<K, V>> Ꮡm, map<K, V> testData, Func<K, V, bool> yield) {
    foreach (var (k, v) in testData) {
        var (ᴛ343, ᴛ344) = Ꮡm.LoadOrStore(k, v);
        expectStored(Ꮡt, k, v)(ᴛ343, ᴛ344);
    }
    var visited = new map<K, nint>();
    var testDataʗ1 = testData;
    var visitedʗ1 = visited;
    Ꮡm.All()((K key, V got) => {
        var (want, ok) = testDataʗ1[key, ꟷ];
        if (!ok) {
            Ꮡt.Errorf("unexpected key %v in map"u8, key);
            return false;
        }
        if (!AreEqual(got, want)) {
            Ꮡt.Errorf("expected key %v to have value %v, got %v"u8, key, want, got);
            return false;
        }
        visitedʗ1[key]++;
        return yield(key, got);
    });
    foreach (var (key, n) in visited) {
        if (n > 1) {
            Ꮡt.Errorf("visited key %v more than once"u8, key);
        }
    }
}

internal static Action<V, bool> expectPresent<K, V>(ж<testing.T> Ꮡt, K key, V want) {
    Ꮡt.Helper();
    return (V got, bool ok) => {
        Ꮡt.Helper();
        if (!ok) {
            Ꮡt.Errorf("expected key %v to be present in map"u8, key);
        }
        if (ok && !AreEqual(got, want)) {
            Ꮡt.Errorf("expected key %v to have value %v, got %v"u8, key, want, got);
        }
    };
}

internal static Action<V, bool> expectMissing<K, V>(ж<testing.T> Ꮡt, K key, V want) {
    Ꮡt.Helper();
    if (!AreEqual(want, @new<V>().ValueSlot)) {
        // This is awkward, but the want argument is necessary to smooth over type inference.
        // Just make sure the want argument always looks the same.
        throw panic("expectMissing must always have a zero value variable");
    }
    return (V got, bool ok) => {
        Ꮡt.Helper();
        if (ok) {
            Ꮡt.Errorf("expected key %v to be missing from map, got value %v"u8, key, got);
        }
        if (!ok && !AreEqual(got, want)) {
            Ꮡt.Errorf("expected missing key %v to be paired with the zero value; got %v"u8, key, got);
        }
    };
}

internal static Action<V, bool> expectLoaded<K, V>(ж<testing.T> Ꮡt, K key, V want) {
    Ꮡt.Helper();
    return (V got, bool loaded) => {
        Ꮡt.Helper();
        if (!loaded) {
            Ꮡt.Errorf("expected key %v to have been loaded, not stored"u8, key);
        }
        if (!AreEqual(got, want)) {
            Ꮡt.Errorf("expected key %v to have value %v, got %v"u8, key, want, got);
        }
    };
}

internal static Action<V, bool> expectStored<K, V>(ж<testing.T> Ꮡt, K key, V want) {
    Ꮡt.Helper();
    return (V got, bool loaded) => {
        Ꮡt.Helper();
        if (loaded) {
            Ꮡt.Errorf("expected inserted key %v to have been stored, not loaded"u8, key);
        }
        if (!AreEqual(got, want)) {
            Ꮡt.Errorf("expected inserted key %v to have value %v, got %v"u8, key, want, got);
        }
    };
}

internal static Action<bool> expectDeleted<K, V>(ж<testing.T> Ꮡt, K key, V old) {
    Ꮡt.Helper();
    return (bool deleted) => {
        Ꮡt.Helper();
        if (!deleted) {
            Ꮡt.Errorf("expected key %v with value %v to be in map and deleted"u8, key, old);
        }
    };
}

internal static Action<bool> expectNotDeleted<K, V>(ж<testing.T> Ꮡt, K key, V old) {
    Ꮡt.Helper();
    return (bool deleted) => {
        Ꮡt.Helper();
        if (deleted) {
            Ꮡt.Errorf("expected key %v with value %v to not be in map and thus not deleted"u8, key, old);
        }
    };
}

internal static Action<bool> expectSwapped<K, V>(ж<testing.T> Ꮡt, K key, V old, V @new) {
    Ꮡt.Helper();
    return (bool swapped) => {
        Ꮡt.Helper();
        if (!swapped) {
            Ꮡt.Errorf("expected key %v with value %v to be in map and swapped for %v"u8, key, old, @new);
        }
    };
}

internal static Action<bool> expectNotSwapped<K, V>(ж<testing.T> Ꮡt, K key, V old, V @new) {
    Ꮡt.Helper();
    return (bool swapped) => {
        Ꮡt.Helper();
        if (swapped) {
            Ꮡt.Errorf("expected key %v with value %v to not be in map or not swapped for %v"u8, key, old, @new);
        }
    };
}

internal static Action<V, bool> expectLoadedFromSwap<K, V>(ж<testing.T> Ꮡt, K key, V want, V @new) {
    Ꮡt.Helper();
    return (V got, bool loaded) => {
        Ꮡt.Helper();
        if (!loaded){
            Ꮡt.Errorf("expected key %v to be in map and for %v to have been swapped for %v"u8, key, want, @new);
        } else 
        if (!AreEqual(want, got)) {
            Ꮡt.Errorf("key %v had its value %v swapped for %v, but expected it to have value %v"u8, key, got, @new, want);
        }
    };
}

internal static Action<V, bool> expectNotLoadedFromSwap<K, V>(ж<testing.T> Ꮡt, K key, V @new) {
    Ꮡt.Helper();
    return (V old, bool loaded) => {
        Ꮡt.Helper();
        if (loaded) {
            Ꮡt.Errorf("expected key %v to not be in map, but found value %v for it"u8, key, old);
        }
    };
}

internal static Action<V, bool> expectLoadedFromDelete<K, V>(ж<testing.T> Ꮡt, K key, V want) {
    Ꮡt.Helper();
    return (V got, bool loaded) => {
        Ꮡt.Helper();
        if (!loaded){
            Ꮡt.Errorf("expected key %v to be in map to be deleted"u8, key);
        } else 
        if (!AreEqual(want, got)) {
            Ꮡt.Errorf("key %v was deleted with value %v, but expected it to have value %v"u8, key, got, want);
        }
    };
}

internal static Action<V, bool> expectNotLoadedFromDelete<K, V>(ж<testing.T> Ꮡt, K key, V _) {
    Ꮡt.Helper();
    return (V old, bool loaded) => {
        Ꮡt.Helper();
        if (loaded) {
            Ꮡt.Errorf("expected key %v to not be in map, but found value %v for it"u8, key, old);
        }
    };
}

internal static map<@string, nint> testDataMap(slice<@string> data) {
    var m = new map<@string, nint>();
    foreach (var (i, s) in data) {
        m[s] = i;
    }
    return m;
}

internal static array<@string> testDataSmall = new(8);
internal static array<@string> testData = new(128);
internal static array<@string> testDataLarge = new(131072);

[GoInit] internal static void init() {
    foreach (var (i, _) in testDataSmall) {
        testDataSmall[i] = fmt.Sprintf("%b"u8, i);
    }
    foreach (var (i, _) in testData) {
        testData[i] = fmt.Sprintf("%b"u8, i);
    }
    foreach (var (i, _) in testDataLarge) {
        testDataLarge[i] = fmt.Sprintf("%b"u8, i);
    }
}

[GoLocalName("dummy")] [GoType("[32]byte")] internal partial struct TestConcurrentCache_dummy;

[GoType("dyn")] internal partial struct TestConcurrentCache_cleanupArg {
    internal nint key;
    internal weak.Pointer<TestConcurrentCache_dummy> value;
}

// TestConcurrentCache tests HashTrieMap in a scenario where it is used as
// the basis of a memory-efficient concurrent cache. We're specifically
// looking to make sure that CompareAndSwap and CompareAndDelete are
// atomic with respect to one another. When competing for the same
// key-value pair, they must not both succeed.
//
// This test is a regression test for issue #70970.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestConcurrentCache(ж<testing.T> Ꮡt) {
    ref var m = ref heap(new isync.HashTrieMap<nint, weak.Pointer<TestConcurrentCache_dummy>>(), out var Ꮡm);
    var cleanup = (TestConcurrentCache_cleanupArg arg) => {
        Ꮡm.CompareAndDelete(arg.key, arg.value);
    };
    var cleanupʗ1 = cleanup;
    ж<TestConcurrentCache_dummy> get(ж<isync.HashTrieMap<nint, weak.Pointer<TestConcurrentCache_dummy>>> mΔ1, nint key) {
        var nv = @new<TestConcurrentCache_dummy>();
        var nw = weak.Make<TestConcurrentCache_dummy>(nv);
        while (ᐧ) {
            var (w, loaded) = mΔ1.LoadOrStore(key, nw);
            if (!loaded) {
                Δruntime.AddCleanup(nv, cleanupʗ1, new TestConcurrentCache_cleanupArg(key, nw));
                return nv;
            }
            {
                var v = w.Value(); if (v != nil) {
                    return v;
                }
            }
            // Weak pointer was reclaimed, try to replace it with nw.
            if (mΔ1.CompareAndSwap(key, w, nw)) {
                Δruntime.AddCleanup(nv, cleanupʗ1, new TestConcurrentCache_cleanupArg(key, nw));
                return nv;
            }
        }
    }
    const nint N = 100_000;
    const nint P = 5_000;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(N);
    foreach (var i in range(N)) {
        var getʗ1 = get;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var a = getʗ1(Ꮡm, i % P);
                var b = getʗ1(Ꮡm, i % P);
                if (a != b) {
                    Ꮡt.Errorf("consecutive cache reads returned different values: a != b (%p vs %p)\n"u8, a.OrTypedNil(), b.OrTypedNil());
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

} // end sync_test_package
