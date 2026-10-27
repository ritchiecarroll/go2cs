// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using static go.time_package;

partial class time_internal_test_package {

[GoInit] internal static void init() {
    // Force US/Pacific for time zone tests.
    ForceUSPacificForTesting();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string americaLosAngelesˢ = "America/Los_Angeles"u8;

internal static void initTestingZone() {
    // For hermeticity, use only tzinfo source from the test's GOROOT,
    // not the system sources and not whatever GOROOT may happen to be
    // set in the process's environment (if any).
    // This test runs in GOROOT/src/time, so GOROOT is "../..",
    // but it is theoretically possible
    var sources = new @string[]{"../../lib/time/zoneinfo.zip"u8}.slice();
    var (z, err) = loadLocation(americaLosAngelesˢ, sources);
    if (err != default!) {
        throw panic("cannot load America/Los_Angeles for testing: " + err.Error() + "; you may want to use -tags=timetzdata");
    }
    z.Value.name = localˢ;
    localLoc = z.Value;
}

internal static slice<@string> origPlatformZoneSources;
internal static void initᴛorigPlatformZoneSources() { origPlatformZoneSources = platformZoneSources; }

internal static Action /*undo*/ disablePlatformSources() {
    platformZoneSources = default!;
    return () => {
        platformZoneSources = origPlatformZoneSources;
    };
}

public static readonly ж<Action> ᏑInterrupt = new StaticBox<Action>(interrupt);
public static ref Action Interrupt => ref ((StaticBox<Action>)ᏑInterrupt).ValueSlot;

public static readonly ж<Func<global::go.time_package.ΔMonth, nint, nint>> ᏑDaysIn = new StaticBox<Func<global::go.time_package.ΔMonth, nint, nint>>(daysIn);
public static ref Func<global::go.time_package.ΔMonth, nint, nint> DaysIn => ref ((StaticBox<Func<global::go.time_package.ΔMonth, nint, nint>>)ᏑDaysIn).ValueSlot;

internal static void empty(any arg, uintptr seq, int64 delta) {
}

// Test that a runtimeTimer with a period that would overflow when on
// expiration does not throw or cause other timers to hang.
//
// This test has to be in internal_test.go since it fiddles with
// unexported data structures.
public static void CheckRuntimeTimerPeriodOverflow() {
    GoFrame ᒐ = default;
    try {
        // We manually create a runtimeTimer with huge period, but that expires
        // immediately. The public Timer interface would require waiting for
        // the entire period before the first update.
        var t = newTimer(runtimeNano(), 9223372036854775807L, empty, default!, nil);
        var tʗ1 = t;
        defer(() => tʗ1.Stop(), ref ᒐ);

        // If this test fails, we will either throw (when siftdownTimer detects
        // bad when on update), or other timers will hang (if the timer in a
        // heap is in a bad state). There is no reliable way to test this, but
        // we wait on a short timer here as a smoke test (alternatively, timers
        // in later tests may hang).
        ᐸꟷ(After(25 * Millisecond));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static readonly ж<global::go.time_package.Time> ᏑMinMonoTime = new StaticBox<global::go.time_package.Time>(default(global::go.time_package.Time));
public static ref global::go.time_package.Time MinMonoTime => ref ((StaticBox<global::go.time_package.Time>)ᏑMinMonoTime).ValueSlot;
internal static void initᴛMinMonoTime() { MinMonoTime = new Time(wall: ((uint64)1 << 63), ext: -9223372036854775808L, loc: ΔUTC); }
public static readonly ж<global::go.time_package.Time> ᏑMaxMonoTime = new StaticBox<global::go.time_package.Time>(default(global::go.time_package.Time));
public static ref global::go.time_package.Time MaxMonoTime => ref ((StaticBox<global::go.time_package.Time>)ᏑMaxMonoTime).ValueSlot;
internal static void initᴛMaxMonoTime() { MaxMonoTime = new Time(wall: ((uint64)1 << 63), ext: 9223372036854775807L, loc: ΔUTC); }

public static readonly ж<global::go.time_package.Time> ᏑNotMonoNegativeTime = new StaticBox<global::go.time_package.Time>(new Time(wall: 0, ext: -9223372036854775758L));
public static ref global::go.time_package.Time NotMonoNegativeTime => ref ((StaticBox<global::go.time_package.Time>)ᏑNotMonoNegativeTime).ValueSlot;

} // end time_internal_test_package
