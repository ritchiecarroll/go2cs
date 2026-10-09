// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using goexperiment = @internal.goexperiment_package;
using testenv = @internal.testenv_package;
using Δmath = math_package;
using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = global::go.os.exec_package;
using global::go.os;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object lengthWrongˢ = (@string)"length wrong"u8;
internal static readonly object wrongSignˢ = (@string)"wrong sign"u8;

// negative zero is a good test because:
//  1. 0 and -0 are equal, yet have distinct representations.
//  2. 0 is represented as all zeros, -0 isn't.
//
// I'm not sure the language spec actually requires this behavior,
// but it's what the current map implementation does.
public static void TestNegativeZero(ж<testing.T> Ꮡt) {
    var m = new map<float64, bool>(0);
    m[+0.0D] = true;
    m[Δmath.Copysign(0.0D, -1.0D)] = true; // should overwrite +0 entry
    if (len(m) != 1) {
        Ꮡt.Error(lengthWrongˢ);
    }
    foreach (var (k, _) in m) {
        if (Δmath.Copysign(1.0D, k) > 0D) {
            Ꮡt.Error(wrongSignˢ);
        }
    }
    m = new map<float64, bool>(0);
    m[Δmath.Copysign(0.0D, -1.0D)] = true;
    m[+0.0D] = true; // should overwrite -0.0 entry
    if (len(m) != 1) {
        Ꮡt.Error(lengthWrongˢ);
    }
    foreach (var (k, _) in m) {
        if (Δmath.Copysign(1.0D, k) < 0D) {
            Ꮡt.Error(wrongSignˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nanDisappearedˢ = (@string)"nan disappeared"u8;
internal static readonly object valueWrongˢ = (@string)"value wrong"u8;
internal static readonly object valuesWrongˢ = (@string)"values wrong"u8;

internal static void testMapNan(ж<testing.T> Ꮡt, map<float64, nint> m) {
    if (len(m) != 3) {
        Ꮡt.Error(lengthWrongˢ);
    }
    nint s = 0;
    foreach (var (k, v) in m) {
        if (k == k) {
            Ꮡt.Error(nanDisappearedˢ);
        }
        if (((nint)(v & (v - 1))) != 0) {
            Ꮡt.Error(valueWrongˢ);
        }
        s |= (nint)(v);
    }
    if (s != 7) {
        Ꮡt.Error(valuesWrongˢ);
    }
}

// nan is a good test because nan != nan, and nan has
// a randomized hash value.
public static void TestMapAssignmentNan(ж<testing.T> Ꮡt) {
    var m = new map<float64, nint>(0);
    var nan = Δmath.NaN();
    // Test assignment.
    m[nan] = 1;
    m[nan] = 2;
    m[nan] = 4;
    testMapNan(Ꮡt, m);
}

// nan is a good test because nan != nan, and nan has
// a randomized hash value.
public static void TestMapOperatorAssignmentNan(ж<testing.T> Ꮡt) {
    var m = new map<float64, nint>(0);
    var nan = Δmath.NaN();
    // Test assignment operations.
    m[nan] += 1;
    m[nan] += 2;
    m[nan] += 4;
    testMapNan(Ꮡt, m);
}

public static void TestMapOperatorAssignment(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var m = new map<nint, nint>(0);
    // "m[k] op= x" is rewritten into "m[k] = m[k] op x"
    // differently when op is / or % than when it isn't.
    // Simple test to make sure they all work as expected.
    m[0] = 12345;
    m[0] += 67890;
    m[0] /= 123;
    m[0] %= 456;
    const nint want = /* (12345 + 67890) / 123 % 456 */ 196;
    {
        nint got = m[0]; if (got != want) {
            Ꮡt.Errorf("got %d, want %d"u8, got, (nint)(want));
        }
    }
}

internal static bool sinkAppend;

public static void TestMapAppendAssignment(ж<testing.T> Ꮡt) {
    var m = new map<nint, slice<nint>>(0);
    m[0] = default!;
    m[0] = append(m[0], (nint)(12345));
    m[0] = append(m[0], (nint)(67890));
    (sinkAppend, m[0]) = (!sinkAppend, append(m[0], (nint)(123), (nint)(456)));
    var a = new nint[]{7, 8, 9, 0}.slice();
    m[0] = appendꓸꓸꓸ(m[0], a);
    var want = new nint[]{12345, 67890, 123, 456, 7, 8, 9, 0}.slice();
    {
        var got = m[0]; if (!slices.Equal<slice<nint>, nint>(got, want)) {
            Ꮡt.Errorf("got %v, want %v"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object aliasDidnTWorkˢ = (@string)"alias didn't work"u8;

// Maps aren't actually copied on assignment.
public static void TestAlias(ж<testing.T> Ꮡt) {
    var m = new map<nint, nint>(0);
    m[0] = 5;
    var n = m;
    n[0] = 6;
    if (m[0] != 6) {
        Ꮡt.Error(aliasDidnTWorkˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object naNKeysLostDuringGrowˢ = (@string)"NaN keys lost during grow"u8;
internal static readonly object naNValuesLostDuringGrowˢ = (@string)"NaN values lost during grow"u8;

public static void TestGrowWithNaN(ж<testing.T> Ꮡt) {
    var m = new map<float64, nint>(4);
    var nan = Δmath.NaN();
    // Use both assignment and assignment operations as they may
    // behave differently.
    m[nan] = 1;
    m[nan] = 2;
    m[nan] += 4;
    nint cnt = 0;
    nint s = 0;
    var growflag = true;
    foreach (var (k, v) in m) {
        if (growflag) {
            // force a hashtable resize
            for (nint i = 0; i < 50; i++) {
                m[(float64)i] = i;
            }
            for (nint i = 50; i < 100; i++) {
                m[(float64)i] += i;
            }
            growflag = false;
        }
        if (k != k) {
            cnt++;
            s |= (nint)(v);
        }
    }
    if (cnt != 3) {
        Ꮡt.Error(naNKeysLostDuringGrowˢ);
    }
    if (s != 7) {
        Ꮡt.Error(naNValuesLostDuringGrowˢ);
    }
}

partial struct FloatInt {
    internal float64 x;
    internal nint y;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object keyValueNotUpdatedˢ = (@string)"key/value not updated together 1"u8;
internal static readonly object keyValueNotUpdatedˢ2 = (@string)"key/value not updated together 2"u8;
internal static readonly object entryMissingˢ = (@string)"entry missing"u8;
internal static readonly object wrongNumberOfEntriesˢ = (@string)"wrong number of entries returned by iterator"u8;
internal static readonly object updateToNegzeroMissedByˢ = (@string)"update to negzero missed by iteration"u8;

public static void TestGrowWithNegativeZero(ж<testing.T> Ꮡt) {
    var negzero = Δmath.Copysign(0.0D, -1.0D);
    var m = new map<FloatInt, nint>(4);
    m[new FloatInt(0.0D, 0)] = 1;
    m[new FloatInt(0.0D, 1)] += 2;
    m[new FloatInt(0.0D, 2)] += 4;
    m[new FloatInt(0.0D, 3)] = 8;
    var growflag = true;
    nint s = 0;
    nint cnt = 0;
    nint negcnt = 0;
    // The first iteration should return the +0 key.
    // The subsequent iterations should return the -0 key.
    // I'm not really sure this is required by the spec,
    // but it makes sense.
    // TODO: are we allowed to get the first entry returned again???
    foreach (var (k, v) in m) {
        if (v == 0) {
            continue;
        } // ignore entries added to grow table
        cnt++;
        if (Δmath.Copysign(1.0D, k.x) < 0D){
            if ((nint)(v & 16) == 0) {
                Ꮡt.Error(keyValueNotUpdatedˢ);
            }
            negcnt++;
            s |= (nint)((nint)(v & 15));
        } else {
            if ((nint)(v & 16) == 16) {
                Ꮡt.Error(keyValueNotUpdatedˢ2, k, v);
            }
            s |= (nint)(v);
        }
        if (growflag) {
            // force a hashtable resize
            for (nint i = 0; i < 100; i++) {
                m[new FloatInt(3.0D, i)] = 0;
            }
            // then change all the entries
            // to negative zero
            m[new FloatInt(negzero, 0)] = (nint)(1 | 16);
            m[new FloatInt(negzero, 1)] = (nint)(2 | 16);
            m[new FloatInt(negzero, 2)] = (nint)(4 | 16);
            m[new FloatInt(negzero, 3)] = (nint)(8 | 16);
            growflag = false;
        }
    }
    if (s != 15) {
        Ꮡt.Error(entryMissingˢ, s);
    }
    if (cnt != 4) {
        Ꮡt.Error(wrongNumberOfEntriesˢ, cnt);
    }
    if (negcnt != 3) {
        Ꮡt.Error(updateToNegzeroMissedByˢ, negcnt);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object oddValueReturnedˢ = (@string)"odd value returned"u8;

public static void TestIterGrowAndDelete(ж<testing.T> Ꮡt) {
    var m = new map<nint, nint>(4);
    for (nint i = 0; i < 100; i++) {
        m[i] = i;
    }
    var growflag = true;
    foreach (var (k, _) in m) {
        if (growflag){
            // grow the table
            for (nint i = 100; i < 1000; i++) {
                m[i] = i;
            }
            // delete all odd keys
            for (nint i = 1; i < 1000; i += 2) {
                delete(m, i);
            }
            growflag = false;
        } else {
            if ((nint)(k & 1) == 1) {
                Ꮡt.Error(oddValueReturnedˢ);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object missingKeyˢ = (@string)"missing key"u8;

// make sure old bucket arrays don't get GCd while
// an iterator is still using them.
public static void TestIterGrowWithGC(ж<testing.T> Ꮡt) {
    var m = new map<nint, nint>(4);
    for (nint i = 0; i < 8; i++) {
        m[i] = i;
    }
    for (nint i = 8; i < 16; i++) {
        m[i] += i;
    }
    var growflag = true;
    nint bitmask = 0;
    foreach (var (k, _) in m) {
        if (k < 16) {
            bitmask |= (nint)(((nint)1).Lsh((nuint)k));
        }
        if (growflag) {
            // grow the table
            for (nint i = 100; i < 1000; i++) {
                m[i] = i;
            }
            // trigger a gc
            Δruntime.GC();
            growflag = false;
        }
    }
    if (bitmask != (1 << (int)(16)) - 1) {
        Ꮡt.Error(missingKeyˢ, bitmask);
    }
}

internal static partial void testConcurrentReadsAfterGrowth(ж<testing.T> Ꮡt, bool useReflect) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        if (Δruntime.GOMAXPROCS(-1) == 1) {
            defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(16), ref ᒐ);
        }
        nint numLoop = 10;
        nint numGrowStep = 250;
        nint numReader = 16;
        if (testing.Short()) {
            (numLoop, numGrowStep) = (2, 100);
        }
        for (nint i = 0; i < numLoop; i++) {
            var m = new map<nint, nint>(0);
            for (nint gsᴛ1 = 0; gsᴛ1 < numGrowStep; gsᴛ1++) {
                var gs = gsᴛ1;
                m[gs] = gs;
                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                Ꮡwg.Add(numReader * 2);
                for (nint nr = 0; nr < numReader; nr++) {
                    var mʗ1 = m;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            foreach ((_, _) in mʗ1) {
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                    var mʗ2 = m;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            for (nint key = 0; key < gs; key++) {
                                _ = mʗ2[key];
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                    if (useReflect) {
                        Ꮡwg.Add(1);
                        var mʗ3 = m;
                        goǃ(() => {
                            GoFrame ᒐ = default;
                            try {
                                defer(Ꮡwg.Done, ref ᒐ);
                                ref var mv = ref heap<reflectꓸValue>(out var Ꮡmv);
                                mv = reflect.ValueOf(mʗ3);
                                var keys = mv.MapKeys();
                                foreach (var (_, vᴛ1) in keys) {
                                    ref var k = ref heap(new reflectꓸValue(), out var Ꮡk);
                                    k = vᴛ1;

                                    mv.MapIndex(k);
                                }
                            }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ.Run(); }
                        });
                    }
                }
                Ꮡwg.Wait();
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestConcurrentReadsAfterGrowth(ж<testing.T> Ꮡt) {
    testConcurrentReadsAfterGrowth(Ꮡt, false);
}

public static void TestConcurrentReadsAfterGrowthReflect(ж<testing.T> Ꮡt) {
    testConcurrentReadsAfterGrowth(Ꮡt, true);
}

public static void TestBigItems(ж<testing.T> Ꮡt) {
    array<@string> key = new(256);
    for (nint iΔ1 = 0; iΔ1 < 256; iΔ1++) {
        key[iΔ1] = fooˢ2;
    }
    var m = new map<array<@string>, array<@string>>(4);
    for (nint iΔ2 = 0; iΔ2 < 100; iΔ2++) {
        key[37] = fmt.Sprintf("string%02d"u8, iΔ2);
        m[key] = key.Clone();
    }
    array<@string> keys = new(100);
    array<@string> values = new(100);
    nint i = 0;
    foreach (var (kᴛ1, vᴛ1) in m) {
        var k = kᴛ1.Clone();
        var v = vᴛ1.Clone();

        keys[i] = k[37];
        values[i] = v[37];
        i++;
    }
    slices.Sort<slice<@string>, @string>(keys[..]);
    slices.Sort<slice<@string>, @string>(values[..]);
    for (nint iΔ3 = 0; iΔ3 < 100; iΔ3++) {
        if (keys[iΔ3] != fmt.Sprintf("string%02d"u8, iΔ3)) {
            Ꮡt.Errorf("#%d: missing key: %v"u8, iΔ3, keys[iΔ3]);
        }
        if (values[iΔ3] != fmt.Sprintf("string%02d"u8, iΔ3)) {
            Ꮡt.Errorf("#%d: missing value: %v"u8, iΔ3, values[iΔ3]);
        }
    }
}

internal partial struct TestMapHugeZero_T /*[4000]byte*/;

public static void TestMapHugeZero(ж<testing.T> Ꮡt) {
    var m = new map<nint, TestMapHugeZero_T>{};
    var x = m[0].Clone();
    if (x != (new TestMapHugeZero_T(new byte[4000].array()))) {
        Ꮡt.Errorf("map value not zero"u8);
    }
    var (y, ok) = m[0, ꟷ];
    if (ok) {
        Ꮡt.Errorf("map value should be missing"u8);
    }
    if (y != (new TestMapHugeZero_T(new byte[4000].array()))) {
        Ꮡt.Errorf("map value not zero"u8);
    }
}

partial struct empty {
}

public static void TestEmptyKeyAndValue(ж<testing.T> Ꮡt) {
    var a = new map<nint, empty>(4);
    var b = new map<empty, nint>(4);
    var c = new map<empty, empty>(4);
    a[0] = new empty(nil);
    b[new empty(nil)] = 0;
    b[new empty(nil)] = 1;
    c[new empty(nil)] = new empty(nil);
    if (len(a) != 1) {
        Ꮡt.Errorf("empty value insert problem"u8);
    }
    if (len(b) != 1) {
        Ꮡt.Errorf("empty key insert problem"u8);
    }
    if (len(c) != 1) {
        Ꮡt.Errorf("empty key+value insert problem"u8);
    }
    if (b[new empty(nil)] != 1) {
        Ꮡt.Errorf("empty key returned wrong value"u8);
    }
}

// Tests a map with a single bucket, with same-lengthed short keys
// ("quick keys") as well as long keys.
public static void TestSingleBucketMapStringKeys_DupLen(ж<testing.T> Ꮡt) {
    testMapLookups(Ꮡt, new map<@string, @string>{
        ["x"u8] = "x1val"u8,
        ["xx"u8] = "x2val"u8,
        ["foo"u8] = "fooval"u8,
        ["bar"u8] = "barval"u8, // same key length as "foo"

        ["xxxx"u8] = "x4val"u8,
        [strings.Repeat("x"u8, 128)] = "longval1"u8,
        [strings.Repeat("y"u8, 128)] = "longval2"u8
    });
}

// Tests a map with a single bucket, with all keys having different lengths.
public static void TestSingleBucketMapStringKeys_NoDupLen(ж<testing.T> Ꮡt) {
    testMapLookups(Ꮡt, new map<@string, @string>{
        ["x"u8] = "x1val"u8,
        ["xx"u8] = "x2val"u8,
        ["foo"u8] = "fooval"u8,
        ["xxxx"u8] = "x4val"u8,
        ["xxxxx"u8] = "x5val"u8,
        ["xxxxxx"u8] = "x6val"u8,
        [strings.Repeat("x"u8, 128)] = "longval"u8
    });
}

internal static void testMapLookups(ж<testing.T> Ꮡt, map<@string, @string> m) {
    foreach (var (k, v) in m) {
        if (m[k] != v) {
            Ꮡt.Fatalf("m[%q] = %q; want %q"u8, k, m[k], v);
        }
    }
}

// Tests whether the iterator returns the right elements when
// started in the middle of a grow, when the keys are NaNs.
public static void TestMapNanGrowIterator(ж<testing.T> Ꮡt) {
    var m = new map<float64, nint>();
    var nan = Δmath.NaN();
    UntypedInt nBuckets = 16;
    // To fill nBuckets buckets takes LOAD * nBuckets keys.
    nint nKeys = (nint)(/* nBuckets * runtime.HashLoad */ 14F);
    // Get map to full point with nan keys.
    for (nint i = 0; i < nKeys; i++) {
        m[nan] = i;
    }
    // Trigger grow
    m[1.0D] = 1;
    delete(m, 1.0D);
    // Run iterator
    var found = new map<nint, EmptyStruct>();
    foreach (var (_, v) in m) {
        if (v != -1) {
            {
                var (_, repeat) = found[v, ꟷ]; if (repeat) {
                    Ꮡt.Fatalf("repeat of value %d"u8, v);
                }
            }
            found[v] = new EmptyStruct();
        }
        if (len(found) == nKeys / 2) {
            // Halfway through iteration, finish grow.
            for (nint i = 0; i < nBuckets; i++) {
                delete(m, 1.0D);
            }
        }
    }
    if (len(found) != nKeys) {
        Ꮡt.Fatalf("missing value"u8);
    }
}

// Issue 8410
public static void TestMapSparseIterOrder(ж<testing.T> Ꮡt) {
    // Run several rounds to increase the probability
    // of failure. One is not enough.
NextRound:
    for (nint round = 0; round < 10; round++) {
        var m = new map<nint, bool>();
        // Add 1000 items, remove 980.
        for (nint i = 0; i < 1000; i++) {
            m[i] = true;
        }
        for (nint i = 20; i < 1000; i++) {
            delete(m, i);
        }
        slice<nint> first = default!;
        foreach (var (i, _) in m) {
            first = append(first, i);
        }
        // 800 chances to get a different iteration order.
        // See bug 8736 for why we need so many tries.
        for (nint n = 0; n < 800; n++) {
            nint idx = 0;
            foreach (var (i, _) in m) {
                if (i != first[idx]) {
                    // iteration order changed.
                    goto continue_NextRound;
                }
                idx++;
            }
        }
        Ꮡt.Fatalf("constant iteration order on round %d: %v"u8, round, first);
continue_NextRound:;
    }
break_NextRound:;
}

// Map iteration must not return duplicate entries.
public static void TestMapIterDuplicate(ж<testing.T> Ꮡt) {
    // Run several rounds to increase the probability
    // of failure. One is not enough.
    foreach (var _ᴛ1 in range(1000)) {
        var m = new map<nint, bool>();
        // Add 1000 items, remove 980.
        for (nint i = 0; i < 1000; i++) {
            m[i] = true;
        }
        for (nint i = 20; i < 1000; i++) {
            delete(m, i);
        }
        slice<nint> want = default!;
        for (nint i = 0; i < 20; i++) {
            want = append(want, i);
        }
        slice<nint> got = default!;
        foreach (var (i, _) in m) {
            got = append(got, i);
        }
        slices.Sort<slice<nint>, nint>(got);
        if (!reflect.DeepEqual(got, want)) {
            Ꮡt.Errorf("iteration got %v want %v\n"u8, got, want);
        }
    }
}

public static void TestMapStringBytesLookup(ж<testing.T> Ꮡt) {
    // Use large string keys to avoid small-allocation coalescing,
    // which can cause AllocsPerRun to report lower counts than it should.
    var m = new map<@string, nint>{
        ["1000000000000000000000000000000000000000000000000"u8] = 1,
        ["2000000000000000000000000000000000000000000000000"u8] = 2
    };
    var buf = slice<byte>("1000000000000000000000000000000000000000000000000"u8);
    {
        nint xΔ1 = m[tmpstring(buf)]; if (xΔ1 != 1) {
            Ꮡt.Errorf(@"m[string([]byte(""1""))] = %d, want 1"u8, xΔ1);
        }
    }
    buf[0] = (rune)'2';
    {
        nint xΔ2 = m[tmpstring(buf)]; if (xΔ2 != 2) {
            Ꮡt.Errorf(@"m[string([]byte(""2""))] = %d, want 2"u8, xΔ2);
        }
    }
    nint x = default!;
    var bufʗ1 = buf;
    var mʗ1 = m;
    var n = testing.AllocsPerRun(100, () => {
        x += mʗ1[tmpstring(bufʗ1)];
    });
    if (n != 0D) {
        Ꮡt.Errorf("AllocsPerRun for m[string(buf)] = %v, want 0"u8, n);
    }
    x = 0;
    var bufʗ2 = buf;
    var mʗ2 = m;
    n = testing.AllocsPerRun(100, () => {
        var (y, ok) = mʗ2[tmpstring(bufʗ2), ꟷ];
        if (!ok) {
            throw panic("!ok");
        }
        x += y;
    });
    if (n != 0D) {
        Ꮡt.Errorf("AllocsPerRun for x,ok = m[string(buf)] = %v, want 0"u8, n);
    }
}

/* [N]nint */
internal partial struct TestMapLargeKeyNoPointer_T /*[64]nint*/;

public static void TestMapLargeKeyNoPointer(ж<testing.T> Ꮡt) {
    const nint I = 1000;
    UntypedInt N = 64;
    var m = new map<TestMapLargeKeyNoPointer_T, nint>();
    for (nint i = 0; i < I; i++) {
        TestMapLargeKeyNoPointer_T v = default!;
        for (nint j = 0; j < N; j++) {
            v[j] = i + j;
        }
        m[v] = i;
    }
    Δruntime.GC();
    for (nint i = 0; i < I; i++) {
        TestMapLargeKeyNoPointer_T v = default!;
        for (nint j = 0; j < N; j++) {
            v[j] = i + j;
        }
        if (m[v] != i) {
            Ꮡt.Fatalf("corrupted map: want %+v, got %+v"u8, i, m[v]);
        }
    }
}

/* [N]nint */
internal partial struct TestMapLargeValNoPointer_T /*[64]nint*/;

public static void TestMapLargeValNoPointer(ж<testing.T> Ꮡt) {
    const nint I = 1000;
    UntypedInt N = 64;
    var m = new map<nint, TestMapLargeValNoPointer_T>();
    for (nint i = 0; i < I; i++) {
        TestMapLargeValNoPointer_T v = default!;
        for (nint j = 0; j < N; j++) {
            v[j] = i + j;
        }
        m[i] = v.Clone();
    }
    Δruntime.GC();
    for (nint i = 0; i < I; i++) {
        TestMapLargeValNoPointer_T v = default!;
        for (nint j = 0; j < N; j++) {
            v[j] = i + j;
        }
        var v1 = m[i].Clone();
        for (nint j = 0; j < N; j++) {
            if (v1[j] != v[j]) {
                Ꮡt.Fatalf("corrupted map: want %+v, got %+v"u8, v, v1);
            }
        }
    }
}

// Test that making a map with a large or invalid hint
// doesn't panic. (Issue 19926).
public static void TestIgnoreBogusMapHint(ж<testing.T> Ꮡt) {
    foreach (var (_, hint) in new int64[]{-1, 4611686018427387904L}.slice()) {
        _ = new map<nint, nint>((nint)(hint));
    }
}

internal static nint testNonEscapingMapVariable = 8;

public static void TestNonEscapingMap(ж<testing.T> Ꮡt) {
    var n = testing.AllocsPerRun(1000, () => {
        var m = new map<nint, nint>{};
        m[0] = 0;
    });
    if (n != 0D) {
        Ꮡt.Errorf("mapliteral: want 0 allocs, got %v"u8, n);
    }
    n = testing.AllocsPerRun(1000, () => {
        var m = new map<nint, nint>();
        m[0] = 0;
    });
    if (n != 0D) {
        Ꮡt.Errorf("no hint: want 0 allocs, got %v"u8, n);
    }
    n = testing.AllocsPerRun(1000, () => {
        var m = new map<nint, nint>(8);
        m[0] = 0;
    });
    if (n != 0D) {
        Ꮡt.Errorf("with small hint: want 0 allocs, got %v"u8, n);
    }
    n = testing.AllocsPerRun(1000, () => {
        var m = new map<nint, nint>(testNonEscapingMapVariable);
        m[0] = 0;
    });
    if (n != 0D) {
        Ꮡt.Errorf("with variable hint: want 0 allocs, got %v"u8, n);
    }
}

public static void TestDeferDeleteSlow(ж<testing.T> Ꮡt) {
    var ks = new complex128[]{0D, 1D, 2D, 3D}.slice();
    var m = new map<any, nint>();
    foreach (var (i, k) in ks) {
        m[k] = i;
    }
    if (len(m) != len(ks)) {
        Ꮡt.Errorf("want %d elements, got %d"u8, len(ks), len(m));
    }
    var ksʗ1 = ks;
    var mʗ1 = m;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            foreach (var (_, k) in ksʗ1) {
                defer((ᴛ1, ᴛ2) => delete(ᴛ1, ᴛ2), mʗ1, k, ref ᒐ);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    if (len(m) != 0) {
        Ꮡt.Errorf("want 0 elements, got %d"u8, len(m));
    }
}

// TestIncrementAfterDeleteValueInt and other test Issue 25936.
// Value types int, int32, int64 are affected. Value type string
// works as expected.
public static void TestIncrementAfterDeleteValueInt(ж<testing.T> Ꮡt) {
    const nint key1 = 12;
    const nint key2 = 13;
    var m = new map<nint, nint>();
    m[key1] = 99;
    delete(m, key1);
    m[key2]++;
    {
        nint n2 = m[key2]; if (n2 != 1) {
            Ꮡt.Errorf("incremented 0 to %d"u8, n2);
        }
    }
}

public static void TestIncrementAfterDeleteValueInt32(ж<testing.T> Ꮡt) {
    const nint key1 = 12;
    const nint key2 = 13;
    var m = new map<nint, int32>();
    m[key1] = 99;
    delete(m, key1);
    m[key2]++;
    {
        var n2 = m[key2]; if (n2 != 1) {
            Ꮡt.Errorf("incremented 0 to %d"u8, n2);
        }
    }
}

public static void TestIncrementAfterDeleteValueInt64(ж<testing.T> Ꮡt) {
    const nint key1 = 12;
    const nint key2 = 13;
    var m = new map<nint, int64>();
    m[key1] = 99;
    delete(m, key1);
    m[key2]++;
    {
        var n2 = m[key2]; if (n2 != 1) {
            Ꮡt.Errorf("incremented 0 to %d"u8, n2);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string key2ᶜ = "x"u8;

public static void TestIncrementAfterDeleteKeyStringValueInt(ж<testing.T> Ꮡt) {
    @string key1 = ""u8;
    @string key2 = key2ᶜ;
    var m = new map<@string, nint>();
    m[key1] = 99;
    delete(m, key1);
    m[key2] += 1;
    {
        nint n2 = m[key2]; if (n2 != 1) {
            Ꮡt.Errorf("incremented 0 to %d"u8, n2);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string key2ᶜ1 = "x"u8;

public static void TestIncrementAfterDeleteKeyValueString(ж<testing.T> Ꮡt) {
    @string key1 = ""u8;
    @string key2 = key2ᶜ1;
    var m = new map<@string, @string>();
    m[key1] = "99"u8;
    delete(m, key1);
    m[key2] += "1"u8;
    {
        @string n2 = m[key2]; if (n2 != "1"u8) {
            Ꮡt.Errorf("appended '1' to empty (nil) string, got %s"u8, n2);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string key2ᶜ2 = "x"u8;

// TestIncrementAfterBulkClearKeyStringValueInt tests that map bulk
// deletion (mapclear) still works as expected. Note that it was not
// affected by Issue 25936.
public static void TestIncrementAfterBulkClearKeyStringValueInt(ж<testing.T> Ꮡt) {
    @string key1 = ""u8;
    @string key2 = key2ᶜ2;
    var m = new map<@string, nint>();
    m[key1] = 99;
    foreach (var (k, _) in m) {
        delete(m, k);
    }
    m[key2]++;
    {
        nint n2 = m[key2]; if (n2 != 1) {
            Ꮡt.Errorf("incremented 0 to %d"u8, n2);
        }
    }
}

public static void TestMapTombstones(ж<testing.T> Ꮡt) {
    var m = new map<nint, nint>{};
    UntypedInt N = 10000;
    // Fill a map.
    for (nint i = 0; i < N; i++) {
        m[i] = i;
    }
    runtime_internal_test_package.MapTombstoneCheck(m);
    // Delete half of the entries.
    for (nint i = 0; i < N; i += 2) {
        delete(m, i);
    }
    runtime_internal_test_package.MapTombstoneCheck(m);
    // Add new entries to fill in holes.
    for (nint i = N; i < (nint)(3 * N / 2); i++) {
        m[i] = i;
    }
    runtime_internal_test_package.MapTombstoneCheck(m);
    // Delete everything.
    for (nint i = 0; i < (nint)(3 * N / 2); i++) {
        delete(m, i);
    }
    runtime_internal_test_package.MapTombstoneCheck(m);
}

partial struct canString /*num:nint*/;

internal static @string String(this canString c) {
    return fmt.Sprintf("%d"u8, (nint)c);
}

internal partial interface TestMapInterfaceKey_GrabBag_i1 /*dyn*/ {
    @string String();
}

// Test all the special cases in runtime.typehash.
internal partial struct TestMapInterfaceKey_GrabBag /*dyn*/ {
    internal float32 f32;
    internal float64 f64;
    internal complex64 c64;
    internal complex128 c128;
    internal @string s;
    internal any i0;
    internal TestMapInterfaceKey_GrabBag_i1 i1;
    internal array<@string> a = new(4);
}

public static void TestMapInterfaceKey(ж<testing.T> Ꮡt) {
    var m = new map<any, bool>{};
    // Put a bunch of data in m, so that a bad hash is likely to
    // lead to a bad bucket, which will lead to a missed lookup.
    for (nint i = 0; i < 1000; i++) {
        m[i] = true;
    }
    m[new TestMapInterfaceKey_GrabBag(f32: 1.0F)] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(f32: 1.0F)]) {
        throw panic("f32 not found");
    }
    m[new TestMapInterfaceKey_GrabBag(f64: 1.0D)] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(f64: 1.0D)]) {
        throw panic("f64 not found");
    }
    m[new TestMapInterfaceKey_GrabBag(c64: 1.0F.i())] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(c64: 1.0F.i())]) {
        throw panic("c64 not found");
    }
    m[new TestMapInterfaceKey_GrabBag(c128: 1.0D.i())] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(c128: 1.0D.i())]) {
        throw panic("c128 not found");
    }
    m[new TestMapInterfaceKey_GrabBag(s: "foo"u8)] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(s: "foo"u8)]) {
        throw panic("string not found");
    }
    m[new TestMapInterfaceKey_GrabBag(i0: (@string)"foo"u8)] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(i0: (@string)"foo"u8)]) {
        throw panic("interface{} not found");
    }
    m[new TestMapInterfaceKey_GrabBag(i1: ((canString)5))] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(i1: ((canString)5))]) {
        throw panic("interface{String() string} not found");
    }
    m[new TestMapInterfaceKey_GrabBag(a: new @string[]{"foo"u8, "bar"u8, "baz"u8, "bop"u8}.array())] = true;
    if (!m[new TestMapInterfaceKey_GrabBag(a: new @string[]{"foo"u8, "bar"u8, "baz"u8, "bop"u8}.array())]) {
        throw panic("array not found");
    }
}

partial struct panicStructKey {
    internal slice<nint> sli;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string panicˢ = "panic"u8;

internal static @string String(this panicStructKey p) {
    return panicˢ;
}

partial struct structKey {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string structKeyˢ = "structKey"u8;

internal static @string String(this structKey _) {
    return structKeyˢ;
}

public static void TestEmptyMapWithInterfaceKey(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    bool b = default!;
    nint i = default!;
    int8 i8 = default!;
    int16 i16 = default!;
    int32 i32 = default!;
    int64 i64 = default!;
    nuint ui = default!;
    uint8 ui8 = default!;
    uint16 ui16 = default!;
    uint32 ui32 = default!;
    uint64 ui64 = default!;
    uintptr uipt = default!;
    float32 f32 = default!;
    float64 f64 = default!;
    complex64 c64 = default!;
    complex128 c128 = default!;
    ref var a = ref heap(new array<@string>(4), out var Ꮡa);
    @string s = default!;
    ж<nint> p = default!;
    @unsafe.Pointer up = default!;
    channel<nint> ch = default!;
    any i0 = default!;
    TestMapInterfaceKey_GrabBag_i1 i1 = default!;
    ref var structKey = ref heap(new structKey(), out var ᏑstructKey);
    any i0Panic = new nint[]{}.slice();
    TestMapInterfaceKey_GrabBag_i1 i1Panic = new panicStructKey(nil);
    ref var panicStructKey = ref heap(new panicStructKey(), out var ᏑpanicStructKey);

    panicStructKey = new panicStructKey(nil);
    slice<nint> sli = default!;
    map<any, EmptyStruct> me = new map<any, EmptyStruct>{};
    map<TestMapInterfaceKey_GrabBag_i1, EmptyStruct> mi = new map<TestMapInterfaceKey_GrabBag_i1, EmptyStruct>{};
    void mustNotPanic(Action f) {
        f();
    }
    void mustPanic(Action f) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var r = recover();
                if (r == default!) {
                    Ꮡt.Errorf("didn't panic"u8);
                }
            }, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var meʗ1 = me;
    mustNotPanic(() => {
        _ = meʗ1[b];
    });
    var meʗ2 = me;
    mustNotPanic(() => {
        _ = meʗ2[i];
    });
    var meʗ3 = me;
    mustNotPanic(() => {
        _ = meʗ3[i8];
    });
    var meʗ4 = me;
    mustNotPanic(() => {
        _ = meʗ4[i16];
    });
    var meʗ5 = me;
    mustNotPanic(() => {
        _ = meʗ5[i32];
    });
    var meʗ6 = me;
    mustNotPanic(() => {
        _ = meʗ6[i64];
    });
    var meʗ7 = me;
    mustNotPanic(() => {
        _ = meʗ7[ui];
    });
    var meʗ8 = me;
    mustNotPanic(() => {
        _ = meʗ8[ui8];
    });
    var meʗ9 = me;
    mustNotPanic(() => {
        _ = meʗ9[ui16];
    });
    var meʗ10 = me;
    mustNotPanic(() => {
        _ = meʗ10[ui32];
    });
    var meʗ11 = me;
    mustNotPanic(() => {
        _ = meʗ11[ui64];
    });
    var meʗ12 = me;
    mustNotPanic(() => {
        _ = meʗ12[uipt];
    });
    var meʗ13 = me;
    mustNotPanic(() => {
        _ = meʗ13[f32];
    });
    var meʗ14 = me;
    mustNotPanic(() => {
        _ = meʗ14[f64];
    });
    var meʗ15 = me;
    mustNotPanic(() => {
        _ = meʗ15[c64];
    });
    var meʗ16 = me;
    mustNotPanic(() => {
        _ = meʗ16[c128];
    });
    var aʗ1 = a;
    var meʗ17 = me;
    mustNotPanic(() => {
        _ = meʗ17[aʗ1];
    });
    var meʗ18 = me;
    mustNotPanic(() => {
        _ = meʗ18[s];
    });
    var meʗ19 = me;
    var pʗ1 = p;
    mustNotPanic(() => {
        _ = meʗ19[pʗ1.OrTypedNil()];
    });
    var meʗ20 = me;
    mustNotPanic(() => {
        _ = meʗ20[@unsafe.Pointer.OrTypedNil(up)];
    });
    var chʗ1 = ch;
    var meʗ21 = me;
    mustNotPanic(() => {
        _ = meʗ21[chʗ1];
    });
    var i0ʗ1 = i0;
    var meʗ22 = me;
    mustNotPanic(() => {
        _ = meʗ22[i0ʗ1];
    });
    var i1ʗ1 = i1;
    var meʗ23 = me;
    mustNotPanic(() => {
        _ = meʗ23[i1ʗ1];
    });
    var meʗ24 = me;
    var structKeyʗ1 = structKey;
    mustNotPanic(() => {
        _ = meʗ24[structKeyʗ1];
    });
    var i0Panicʗ1 = i0Panic;
    var meʗ25 = me;
    mustPanic(() => {
        _ = meʗ25[i0Panicʗ1];
    });
    var i1Panicʗ1 = i1Panic;
    var meʗ26 = me;
    mustPanic(() => {
        _ = meʗ26[i1Panicʗ1];
    });
    var meʗ27 = me;
    var panicStructKeyʗ1 = panicStructKey;
    mustPanic(() => {
        _ = meʗ27[panicStructKeyʗ1];
    });
    var meʗ28 = me;
    var sliʗ1 = sli;
    mustPanic(() => {
        _ = meʗ28[sliʗ1];
    });
    var meʗ29 = me;
    mustPanic(() => {
        _ = meʗ29[meʗ29];
    });
    var miʗ1 = mi;
    var structKeyʗ2 = structKey;
    mustNotPanic(() => {
        _ = miʗ1[structKeyʗ2];
    });
    var miʗ2 = mi;
    var panicStructKeyʗ2 = panicStructKey;
    mustPanic(() => {
        _ = miʗ2[panicStructKeyʗ2];
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mapkeysNotImplementedForˢ = (@string)"mapkeys not implemented for swissmaps"u8;

internal partial struct TestMapKeys_key /*dyn*/ {
    internal @string s;
    internal array<byte> pad = new(128); // sizeof(key) > abi.MapMaxKeyBytes
}

public static void TestMapKeys(ж<testing.T> Ꮡt) {
    if (goexperiment.SwissMap) {
        Ꮡt.Skip(mapkeysNotImplementedForˢ);
    }
    var m = new map<TestMapKeys_key, nint>{[new(s: "a"u8)] = 1, [new(s: "b"u8)] = 2};
    ref var keys = ref heap<slice<TestMapKeys_key>>(out var Ꮡkeys);
    keys = new slice<TestMapKeys_key>(0, () => new(), len(m));
    runtime_internal_test_package.MapKeys(m, @unsafe.Pointer.FromPinnedBox(Ꮡkeys));
    foreach (var (_, vᴛ1) in keys) {
        var k = vᴛ1.ΔClone();

        if (len(k.s) != 1) {
            Ꮡt.Errorf("len(k.s) == %d, want 1"u8, len(k.s));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mapvaluesNotImplementedˢ = (@string)"mapvalues not implemented for swissmaps"u8;

internal partial struct TestMapValues_val /*dyn*/ {
    internal @string s;
    internal array<byte> pad = new(128); // sizeof(val) > abi.MapMaxElemBytes
}

public static void TestMapValues(ж<testing.T> Ꮡt) {
    if (goexperiment.SwissMap) {
        Ꮡt.Skip(mapvaluesNotImplementedˢ);
    }
    var m = new map<nint, TestMapValues_val>{[1] = new(s: "a"u8), [2] = new(s: "b"u8)};
    ref var vals = ref heap<slice<TestMapValues_val>>(out var Ꮡvals);
    vals = new slice<TestMapValues_val>(0, () => new(), len(m));
    runtime_internal_test_package.MapValues(m, @unsafe.Pointer.FromPinnedBox(Ꮡvals));
    foreach (var (_, vᴛ1) in vals) {
        var v = vᴛ1.ΔClone();

        if (len(v.s) != 1) {
            Ꮡt.Errorf("len(v.s) == %d, want 1"u8, len(v.s));
        }
    }
}

internal static uintptr computeHash() {
    ref var v = ref heap(new EmptyStruct(), out var Ꮡv);
    return runtime_internal_test_package.MemHash(@unsafe.Pointer.FromPinnedBox(Ꮡv), 0, /* unsafe.Sizeof(v) */ (uintptr)0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunˢ6 = "-test.run=^TestMemHashGlobalSeed$"u8;

internal static uintptr subprocessHash(ж<testing.T> Ꮡt, @string env) {
    Ꮡt.Helper();
    var cmd = testenv.CleanCmdEnv(testenv.Command(new runtime_test_package.testing_TжTB(Ꮡt), Δos.Args[0], testRunˢ6));
    cmd.Value.Env = append((~cmd).Env, "GO_TEST_SUBPROCESS_HASH=1"u8);
    if (env != ""u8) {
        cmd.Value.Env = append((~cmd).Env, env);
    }
    var (@out, err) = cmd.Output();
    if (err != default!) {
        Ꮡt.Fatalf("cmd.Output got err %v want nil"u8, err);
    }
    @string s = strings.TrimSpace(((@string)@out));
    (var h, err) = strconv.ParseUint(s, 10, 64);
    if (err != default!) {
        Ꮡt.Fatalf("Parse output %q got err %v want nil"u8, s, err);
    }
    return (uintptr)h;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goTestSubprocessHashˢ = "GO_TEST_SUBPROCESS_HASH"u8;
internal static readonly @string aesˢ = "aes"u8;
internal static readonly object noAesˢ = (@string)"No AES"u8;
internal static readonly @string noaesˢ = "noaes"u8;
internal static readonly @string godebugCpuAesOffˢ = "GODEBUG=cpu.aes=off"u8;

// memhash has unique per-process seeds, so hashes should differ across
// processes.
//
// Regression test for https://go.dev/issue/66885.
public static void TestMemHashGlobalSeed(ж<testing.T> Ꮡt) {
    if (Δos.Getenv(goTestSubprocessHashˢ) != ""u8) {
        fmt.Println(computeHash());
        Δos.Exit(0);
        return;
    }
    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    // aeshash and memhashFallback use separate per-process seeds, so test
    // both.
    Ꮡt.Run(aesˢ, (ж<testing.T> tΔ1) => {
        if (!runtime_internal_test_package.UseAeshash.Value) {
            tΔ1.Skip(noAesˢ);
        }
        var h1 = subprocessHash(tΔ1, ""u8);
        tΔ1.Logf("%d"u8, h1);
        var h2 = subprocessHash(tΔ1, ""u8);
        tΔ1.Logf("%d"u8, h2);
        var h3 = subprocessHash(tΔ1, ""u8);
        tΔ1.Logf("%d"u8, h3);
        if (h1 == h2 && h2 == h3) {
            tΔ1.Errorf("got duplicate hash %d want unique"u8, h1);
        }
    });
    Ꮡt.Run(noaesˢ, (ж<testing.T> tΔ2) => {
        @string env = ""u8;
        if (runtime_internal_test_package.UseAeshash.Value) {
            env = godebugCpuAesOffˢ;
        }
        var h1 = subprocessHash(tΔ2, env);
        tΔ2.Logf("%d"u8, h1);
        var h2 = subprocessHash(tΔ2, env);
        tΔ2.Logf("%d"u8, h2);
        var h3 = subprocessHash(tΔ2, env);
        tΔ2.Logf("%d"u8, h3);
        if (h1 == h2 && h2 == h3) {
            tΔ2.Errorf("got duplicate hash %d want unique"u8, h1);
        }
    });
}

public static void TestMapIterDeleteReplace(ж<testing.T> Ꮡt) {
    nint inc = 1;
    if (testing.Short()) {
        inc = 100;
    }
    for (nint iᴛ1 = 0; iᴛ1 < 10000; iᴛ1 += inc) {
        var i = iᴛ1;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var m = new map<nint, bool>();
            foreach (var j in range(i)) {
                m[j] = false;
            }
            // Delete and replace all entries.
            foreach (var (k, _) in m) {
                delete(m, k);
                m[k] = true;
            }
            foreach (var (k, v) in m) {
                if (!v) {
                    tΔ1.Errorf("m[%d] got false want true"u8, k);
                }
            }
        });
    }
}

} // end runtime_test_package
