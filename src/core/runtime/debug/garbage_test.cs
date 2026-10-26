// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using testenv = @internal.testenv_package;
using os = os_package;
using runtime = runtime_package;
using static go.runtime.debug_package;
using testing = testing_package;
using time = time_package;
using @internal;
using debug = go.runtime.debug_package;

partial class debug_test_package {

public static void TestReadGCStats(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(SetGCPercent, SetGCPercent(-1), ref ᒐ);

        ref var stats = ref heap(new debug.GCStats(), out var Ꮡstats);
        ref var mstats = ref heap(new runtime.MemStats(), out var Ꮡmstats);
        time.Duration minimum = default!;
        time.Duration maximum = default!;

        // First ReadGCStats will allocate, second should not,
        // especially if we follow up with an explicit garbage collection.
        stats.PauseQuantiles = new slice<time.Duration>(10);
        ReadGCStats(Ꮡstats);
        runtime.GC();

        // Assume these will return same data: no GC during ReadGCStats.
        ReadGCStats(Ꮡstats);
        runtime.ReadMemStats(Ꮡmstats);

        if (stats.NumGC != (int64)mstats.NumGC) {
            Ꮡt.Errorf("stats.NumGC = %d, but mstats.NumGC = %d"u8, stats.NumGC, mstats.NumGC);
        }
        if (stats.PauseTotal != ((time.Duration)(int64)mstats.PauseTotalNs)) {
            Ꮡt.Errorf("stats.PauseTotal = %d, but mstats.PauseTotalNs = %d"u8, stats.PauseTotal, mstats.PauseTotalNs);
        }
        if (stats.LastGC.UnixNano() != (int64)mstats.LastGC) {
            Ꮡt.Errorf("stats.LastGC.UnixNano = %d, but mstats.LastGC = %d"u8, stats.LastGC.UnixNano(), mstats.LastGC);
        }
        nint n = (nint)mstats.NumGC;
        if (n > len(mstats.PauseNs)) {
            n = len(mstats.PauseNs);
        }
        if (len(stats.Pause) != n){
            Ꮡt.Errorf("len(stats.Pause) = %d, want %d"u8, len(stats.Pause), n);
        } else {
            nint offΔ1 = ((nint)mstats.NumGC + len(mstats.PauseNs) - 1) % len(mstats.PauseNs);
            for (nint i = 0; i < n; i++) {
                var dt = stats.Pause[i];
                if (dt != ((time.Duration)(int64)mstats.PauseNs[offΔ1])) {
                    Ꮡt.Errorf("stats.Pause[%d] = %d, want %d"u8, i, dt, mstats.PauseNs[offΔ1]);
                }
                maximum = max(maximum, dt);
                if (i == 0){
                    minimum = dt;
                } else {
                    minimum = min(minimum, dt);
                }
                offΔ1 = (offΔ1 + len(mstats.PauseNs) - 1) % len(mstats.PauseNs);
            }
        }

        var q = stats.PauseQuantiles;
        nint nq = len(q);
        if (q[0] != minimum || q[nq - 1] != maximum) {
            Ꮡt.Errorf("stats.PauseQuantiles = [%d, ..., %d], want [%d, ..., %d]"u8, q[0], q[nq - 1], minimum, maximum);
        }

        for (nint i = 0; i < nq - 1; i++) {
            if (q[i] > q[i + 1]) {
                Ꮡt.Errorf("stats.PauseQuantiles[%d]=%d > stats.PauseQuantiles[%d]=%d"u8, i, q[i], i + 1, q[i + 1]);
            }
        }

        // compare memory stats with gc stats:
        if (len(stats.PauseEnd) != n) {
            Ꮡt.Fatalf("len(stats.PauseEnd) = %d, want %d"u8, len(stats.PauseEnd), n);
        }
        nint off = ((nint)mstats.NumGC + len(mstats.PauseEnd) - 1) % len(mstats.PauseEnd);
        for (nint i = 0; i < n; i++) {
            var dt = stats.PauseEnd[i];
            if (dt.UnixNano() != (int64)mstats.PauseEnd[off]) {
                Ꮡt.Errorf("stats.PauseEnd[%d] = %d, want %d"u8, i, dt.UnixNano(), mstats.PauseEnd[off]);
            }
            off = (off + len(mstats.PauseEnd) - 1) % len(mstats.PauseEnd);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<byte> big;

public static void TestFreeOSMemory(ж<testing.T> Ꮡt) {
    // Tests FreeOSMemory by making big susceptible to collection
    // and checking that at least that much memory is returned to
    // the OS after.

    UntypedInt bigBytes = /* 32 << 20 */ 33554432;
    big = new slice<byte>(bigBytes);

    // Make sure any in-progress GCs are complete.
    runtime.GC();

    ref var before = ref heap(new runtime.MemStats(), out var Ꮡbefore);
    runtime.ReadMemStats(Ꮡbefore);

    // Clear the last reference to the big allocation, making it
    // susceptible to collection.
    big = default!;

    // FreeOSMemory runs a GC cycle before releasing memory,
    // so it's fine to skip a GC here.
    //
    // It's possible the background scavenger runs concurrently
    // with this function and does most of the work for it.
    // If that happens, it's OK. What we want is a test that fails
    // often if FreeOSMemory does not work correctly, and a test
    // that passes every time if it does.
    FreeOSMemory();

    ref var after = ref heap(new runtime.MemStats(), out var Ꮡafter);
    runtime.ReadMemStats(Ꮡafter);

    // Check to make sure that the big allocation (now freed)
    // had its memory shift into HeapReleased as a result of that
    // FreeOSMemory.
    if (after.HeapReleased <= before.HeapReleased) {
        Ꮡt.Fatalf("no memory released: %d -> %d"u8, before.HeapReleased, after.HeapReleased);
    }

    // Check to make sure bigBytes was released, plus some slack. Pages may get
    // allocated in between the two measurements above for a variety for reasons,
    // most commonly for GC work bufs. Since this can get fairly high, depending
    // on scheduling and what GOMAXPROCS is, give a lot of slack up-front.
    //
    // Add a little more slack too if the page size is bigger than the runtime page size.
    // "big" could end up unaligned on its ends, forcing the scavenger to skip at worst
    // 2x pages.
    var slack = (uint64)(bigBytes / 2);
    var pageSize = (uint64)os.Getpagesize();
    if (pageSize > (uint64)8 << 10) {
        slack += pageSize * 2;
    }
    if (slack > bigBytes) {
        // We basically already checked this.
        return;
    }
    if (after.HeapReleased - before.HeapReleased < (uint64)bigBytes - slack) {
        Ꮡt.Fatalf("less than %d released: %d -> %d"u8, (uint64)bigBytes - slack, before.HeapReleased, after.HeapReleased);
    }
}

internal static any setGCPercentBallast;
internal static any setGCPercentSink;

public static void TestSetGCPercent(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testenv.SkipFlaky(new testing_TжTB(Ꮡt), 20076);

        // Test that the variable is being set and returned correctly.
        nint old = SetGCPercent(123);
        nint @new = SetGCPercent(old);
        if (@new != 123) {
            Ꮡt.Errorf("SetGCPercent(123); SetGCPercent(x) = %d, want 123"u8, @new);
        }

        // Test that the percentage is implemented correctly.
        defer(() => {
            SetGCPercent(old);
            (setGCPercentBallast, setGCPercentSink) = (default!, default!);
        }, ref ᒐ);
        SetGCPercent(100);
        runtime.GC();
        // Create 100 MB of live heap as a baseline.
        UntypedInt baseline = /* 100 << 20 */ 104857600;
        ref var ms = ref heap(new runtime.MemStats(), out var Ꮡms);
        runtime.ReadMemStats(Ꮡms);
        setGCPercentBallast = new slice<byte>((nint)((uint64)baseline - ms.Alloc));
        runtime.GC();
        runtime.ReadMemStats(Ꮡms);
        if (abs64((int64)baseline - (int64)ms.Alloc) > (int64)10 << 20) {
            Ꮡt.Fatalf("failed to set up baseline live heap; got %d MB, want %d MB"u8, ms.Alloc >> 20, (nint)(baseline >> 20));
        }
        // NextGC should be ~200 MB.
        UntypedInt thresh = /* 20 << 20 */ 20971520; // TODO: Figure out why this is so noisy on some builders
        {
            var want = (int64)(2 * baseline); if (abs64(want - (int64)ms.NextGC) > thresh) {
                Ꮡt.Errorf("NextGC = %d MB, want %d±%d MB"u8, ms.NextGC >> 20, want >> 20, (nint)(thresh >> 20));
            }
        }
        // Create some garbage, but not enough to trigger another GC.
        for (nint i = 0; i < (nint)((nint)(/* 1.2 * baseline */ 125829120L)); i += (1 << 10)) {
            setGCPercentSink = new slice<byte>((1 << 10));
        }
        setGCPercentSink = default!;
        // Adjust GOGC to 50. NextGC should be ~150 MB.
        SetGCPercent(50);
        runtime.ReadMemStats(Ꮡms);
        {
            var want = (int64)(/* 1.5 * baseline */ 157286400L); if (abs64(want - (int64)ms.NextGC) > thresh) {
                Ꮡt.Errorf("NextGC = %d MB, want %d±%d MB"u8, ms.NextGC >> 20, want >> 20, (nint)(thresh >> 20));
            }
        }

        // Trigger a GC and get back to 100 MB live with GOGC=100.
        SetGCPercent(100);
        runtime.GC();
        // Raise live to 120 MB.
        setGCPercentSink = new slice<byte>((nint)((nint)(/* 0.2 * baseline */ 20971520L)));
        // Lower GOGC to 10. This must force a GC.
        runtime.ReadMemStats(Ꮡms);
        var ngc1 = ms.NumGC;
        SetGCPercent(10);
        // It may require an allocation to actually force the GC.
        setGCPercentSink = new slice<byte>((1 << 20));
        runtime.ReadMemStats(Ꮡms);
        var ngc2 = ms.NumGC;
        if (ngc1 == ngc2) {
            Ꮡt.Errorf("expected GC to run but it did not"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static int64 abs64(int64 a) {
    if (a < 0) {
        return -a;
    }
    return a;
}

public static void TestSetMaxThreadsOvf(ж<testing.T> Ꮡt) {
    // Verify that a big threads count will not overflow the int32
    // maxmcount variable, causing a panic (see Issue 16076).
    //
    // This can only happen when ints are 64 bits, since on platforms
    // with 32 bit ints SetMaxThreads (which takes an int parameter)
    // cannot be given anything that will overflow an int32.
    //
    // Call SetMaxThreads with 1<<31, but only on 64 bit systems.
    nint nt = SetMaxThreads(unchecked((nint)(2147483648L)));
    SetMaxThreads(nt); // restore previous value
}

} // end debug_test_package
