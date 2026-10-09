// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using rand = global::go.math.rand_package;
using static runtime_package;
using testing = testing_package;
using time = time_package;
using global::go.math;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;
using ꓸꓸꓸfloat64Stream = Span<runtime_test_package.float64Stream>;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcUtilizationˢ = "GC utilization"u8;
internal static readonly @string goalRatioˢ = "goal ratio"u8;
internal static readonly @string triggerRatioˢ = "trigger ratio"u8;
internal static readonly @string runwayˢ = "runway"u8;

public static void TestGcPacer(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    const int64 initialHeapBytes = /* 256 << 10 */ 262144;
    foreach (var (_, e) in new ж<gcExecTest>[]{
        Ꮡ(new gcExecTest(
            name: "Steady"u8, // The most basic test case: a steady-state heap.
 // Growth to an O(MiB) heap, then constant heap size, alloc/scan rates.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ1, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n >= 25) {
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ1, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    assertInEpsilon(tΔ1, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ1, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "SteadyBigStacks"u8, // Same as the steady-state case, but lots of stacks to scan relative to the heap size.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(132.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(2048D).sum(ramp((128 << (int)(20)), 8)),
            length: 50,
            checker: (ж<testing.T> tΔ2, slice<gcCycleResult> c) => {
                // Check the same conditions as the steady-state case, except the old pacer can't
                // really handle this well, so don't check the goal ratio for it.
                nint n = len(c);
                if (n >= 25) {
                    // For the pacer redesign, assert something even stronger: at this alloc/scan rate,
                    // it should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ2, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    assertInRange(tΔ2, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    assertInEpsilon(tΔ2, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "SteadyBigGlobals"u8, // Same as the steady-state case, but lots of globals to scan relative to the heap size.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)128 << (int)(20)),
            nCores: 8,
            allocRate: constant(132.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ3, slice<gcCycleResult> c) => {
                // Check the same conditions as the steady-state case, except the old pacer can't
                // really handle this well, so don't check the goal ratio for it.
                nint n = len(c);
                if (n >= 25) {
                    // For the pacer redesign, assert something even stronger: at this alloc/scan rate,
                    // it should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ3, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    assertInRange(tΔ3, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    assertInEpsilon(tΔ3, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "StepAlloc"u8, // This tests the GC pacer's response to a small change in allocation rate.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D).sum(ramp(66.0D, 1).delay(50)),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 100,
            checker: (ж<testing.T> tΔ4, slice<gcCycleResult> c) => {
                nint n = len(c);
                if ((n >= 25 && n < 50) || n >= 75) {
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles
                    // and then is able to settle again after a significant jump in allocation rate.
                    assertInEpsilon(tΔ4, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ4, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "HeavyStepAlloc"u8, // This tests the GC pacer's response to a large change in allocation rate.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33D).sum(ramp(330D, 1).delay(50)),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 100,
            checker: (ж<testing.T> tΔ5, slice<gcCycleResult> c) => {
                nint n = len(c);
                if ((n >= 25 && n < 50) || n >= 75) {
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles
                    // and then is able to settle again after a significant jump in allocation rate.
                    assertInEpsilon(tΔ5, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ5, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "StepScannableFrac"u8, // This tests the GC pacer's response to a change in the fraction of the scannable heap.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(128.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(0.2D).sum(unit(0.5D).delay(50)),
            stackBytes: constant(8192D),
            length: 100,
            checker: (ж<testing.T> tΔ6, slice<gcCycleResult> c) => {
                nint n = len(c);
                if ((n >= 25 && n < 50) || n >= 75) {
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles
                    // and then is able to settle again after a significant jump in allocation rate.
                    assertInEpsilon(tΔ6, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ6, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "HighGOGC"u8, // Tests the pacer for a high GOGC value with a large heap growth happening
 // in the middle. The purpose of the large heap growth is to check if GC
 // utilization ends up sensitive

            gcPercent: 1500,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: random(7D, 0x53).offset(165D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12), random(0.01D, 0x1), unit(14D).delay(25)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ7, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 12) {
                    if (n == 26){
                        // In the 26th cycle there's a heap growth. Overshoot is expected to maintain
                        // a stable utilization, but we should *never* overshoot more than GOGC of
                        // the next cycle.
                        assertInRange(tΔ7, goalRatioˢ, c[n - 1].goalRatio(), 0.90D, 15D);
                    } else {
                        // Give a wider goal range here. With such a high GOGC value we're going to be
                        // forced to undershoot.
                        //
                        // TODO(mknyszek): Instead of placing a 0.95 limit on the trigger, make the limit
                        // based on absolute bytes, that's based somewhat in how the minimum heap size
                        // is determined.
                        assertInRange(tΔ7, goalRatioˢ, c[n - 1].goalRatio(), 0.90D, 1.05D);
                    }
                    // Ensure utilization remains stable despite a growth in live heap size
                    // at GC #25. This test fails prior to the GC pacer redesign.
                    //
                    // Because GOGC is so large, we should also be really close to the goal utilization.
                    assertInEpsilon(tΔ7, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, runtime_internal_test_package.GCGoalUtilization + 0.03D);
                    assertInEpsilon(tΔ7, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.03D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "OscAlloc"u8, // This test makes sure that in the face of a varying (in this case, oscillating) allocation
 // rate, the pacer does a reasonably good job of staying abreast of the changes.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: oscillate(13D, 0D, 8).offset(67D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ8, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 12) {
                    // After the 12th GC, the heap will stop growing. Now, just make sure that:
                    // 1. Utilization isn't varying _too_ much, and
                    // 2. The pacer is mostly keeping up with the goal.
                    assertInRange(tΔ8, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                    assertInRange(tΔ8, gcUtilizationˢ, c[n - 1].gcUtilization, 0.25D, 0.3D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "JitterAlloc"u8, // This test is the same as OscAlloc, but instead of oscillating, the allocation rate is jittery.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: random(13D, 0xf).offset(132D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12), random(0.01D, 0xe)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ9, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 12) {
                    // After the 12th GC, the heap will stop growing. Now, just make sure that:
                    // 1. Utilization isn't varying _too_ much, and
                    // 2. The pacer is mostly keeping up with the goal.
                    assertInRange(tΔ9, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.025D);
                    assertInRange(tΔ9, gcUtilizationˢ, c[n - 1].gcUtilization, 0.25D, 0.275D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "HeavyJitterAlloc"u8, // This test is the same as JitterAlloc, but with a much higher allocation rate.
 // The jitter is proportionally the same.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: random(33.0D, 0x0).offset(330D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12), random(0.01D, 0x152)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ10, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 13) {
                    // After the 12th GC, the heap will stop growing. Now, just make sure that:
                    // 1. Utilization isn't varying _too_ much, and
                    // 2. The pacer is mostly keeping up with the goal.
                    // We start at the 13th here because we want to use the 12th as a reference.
                    assertInRange(tΔ10, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                    // Unlike the other tests, GC utilization here will vary more and tend higher.
                    // Just make sure it's not going too crazy.
                    assertInEpsilon(tΔ10, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.05D);
                    assertInEpsilon(tΔ10, gcUtilizationˢ, c[n - 1].gcUtilization, c[11].gcUtilization, 0.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "SmallHeapSlowAlloc"u8, // This test sets a slow allocation rate and a small heap (close to the minimum heap size)
 // to try to minimize the difference between the trigger and the goal.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(1.0D),
            scanRate: constant(2048.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 3)),
            scannableFrac: constant(0.01D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ11, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 4) {
                    // After the 4th GC, the heap will stop growing.
                    // First, let's make sure we're finishing near the goal, with some extra
                    // room because we're probably going to be triggering early.
                    assertInRange(tΔ11, goalRatioˢ, c[n - 1].goalRatio(), 0.925D, 1.025D);
                    // Next, let's make sure there's some minimum distance between the goal
                    // and the trigger. It should be proportional to the runway (hence the
                    // trigger ratio check, instead of a check against the runway).
                    assertInRange(tΔ11, triggerRatioˢ, c[n - 1].triggerRatio(), 0.925D, 0.975D);
                }
                if (n > 25) {
                    // Double-check that GC utilization looks OK.
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ11, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure GC utilization has mostly levelled off.
                    assertInEpsilon(tΔ11, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.05D);
                    assertInEpsilon(tΔ11, gcUtilizationˢ, c[n - 1].gcUtilization, c[11].gcUtilization, 0.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "MediumHeapSlowAlloc"u8, // This test sets a slow allocation rate and a medium heap (around 10x the min heap size)
 // to try to minimize the difference between the trigger and the goal.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(1.0D),
            scanRate: constant(2048.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 8)),
            scannableFrac: constant(0.01D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ12, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 9) {
                    // After the 4th GC, the heap will stop growing.
                    // First, let's make sure we're finishing near the goal, with some extra
                    // room because we're probably going to be triggering early.
                    assertInRange(tΔ12, goalRatioˢ, c[n - 1].goalRatio(), 0.925D, 1.025D);
                    // Next, let's make sure there's some minimum distance between the goal
                    // and the trigger. It should be proportional to the runway (hence the
                    // trigger ratio check, instead of a check against the runway).
                    assertInRange(tΔ12, triggerRatioˢ, c[n - 1].triggerRatio(), 0.925D, 0.975D);
                }
                if (n > 25) {
                    // Double-check that GC utilization looks OK.
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ12, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure GC utilization has mostly levelled off.
                    assertInEpsilon(tΔ12, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.05D);
                    assertInEpsilon(tΔ12, gcUtilizationˢ, c[n - 1].gcUtilization, c[11].gcUtilization, 0.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "LargeHeapSlowAlloc"u8, // This test sets a slow allocation rate and a large heap to try to minimize the
 // difference between the trigger and the goal.

            gcPercent: 100,
            memoryLimit: Δmath.MaxInt64,
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(1.0D),
            scanRate: constant(2048.0D),
            growthRate: constant(4.0D).sum(ramp(-3.0D, 12)),
            scannableFrac: constant(0.01D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ13, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 13) {
                    // After the 4th GC, the heap will stop growing.
                    // First, let's make sure we're finishing near the goal.
                    assertInRange(tΔ13, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                    // Next, let's make sure there's some minimum distance between the goal
                    // and the trigger. It should be around the default minimum heap size.
                    assertInRange(tΔ13, runwayˢ, c[n - 1].runway(), /* DefaultHeapMinimum - 64<<10 */ 4.128768e+06D, /* DefaultHeapMinimum + 64<<10 */ 4.25984e+06D);
                }
                if (n > 25) {
                    // Double-check that GC utilization looks OK.
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ13, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure GC utilization has mostly levelled off.
                    assertInEpsilon(tΔ13, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.05D);
                    assertInEpsilon(tΔ13, gcUtilizationˢ, c[n - 1].gcUtilization, c[11].gcUtilization, 0.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "SteadyMemoryLimit"u8, // The most basic test case with a memory limit: a steady-state heap.
 // Growth to an O(MiB) heap, then constant heap size, alloc/scan rates.
 // Provide a lot of room for the limit. Essentially, this should behave just like
 // the "Steady" test. Note that we don't simulate non-heap overheads, so the
 // memory limit and the heap limit are identical.

            gcPercent: 100,
            memoryLimit: ((int64)512 << (int)(20)),
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ14, slice<gcCycleResult> c) => {
                nint n = len(c);
                {
                    var peak = c[n - 1].heapPeak; if (peak >= applyMemoryLimitHeapGoalHeadroom(((uint64)512 << (int)(20)))) {
                        tΔ14.Errorf("peak heap size reaches heap limit: %d"u8, peak);
                    }
                }
                if (n >= 25) {
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ14, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    assertInEpsilon(tΔ14, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ14, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "SteadyMemoryLimitNoGCPercent"u8, // This is the same as the previous test, but gcPercent = -1, so the heap *should* grow
 // all the way to the peak.

            gcPercent: -1,
            memoryLimit: ((int64)512 << (int)(20)),
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(2.0D).sum(ramp(-1.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ15, slice<gcCycleResult> c) => {
                nint n = len(c);
                {
                    var goal = c[n - 1].heapGoal; if (goal != applyMemoryLimitHeapGoalHeadroom(((uint64)512 << (int)(20)))) {
                        tΔ15.Errorf("heap goal is not the heap limit: %d"u8, goal);
                    }
                }
                if (n >= 25) {
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization.
                    assertInEpsilon(tΔ15, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    assertInEpsilon(tΔ15, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ15, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "ExceedMemoryLimit"u8, // This test ensures that the pacer doesn't fall over even when the live heap exceeds
 // the memory limit. It also makes sure GC utilization actually rises to push back.

            gcPercent: 100,
            memoryLimit: ((int64)512 << (int)(20)),
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(3.5D).sum(ramp(-2.5D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ16, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 12) {
                    // We're way over the memory limit, so we want to make sure our goal is set
                    // as low as it possibly can be.
                    {
                        var (goal, live) = (c[n - 1].heapGoal, c[n - 1].heapLive); if (goal != live) {
                            tΔ16.Errorf("heap goal is not equal to live heap: %d != %d"u8, goal, live);
                        }
                    }
                }
                if (n >= 25) {
                    // Due to memory pressure, we should scale to 100% GC CPU utilization.
                    // Note that in practice this won't actually happen because of the CPU limiter,
                    // but it's not the pacer's job to limit CPU usage.
                    assertInEpsilon(tΔ16, gcUtilizationˢ, c[n - 1].gcUtilization, 1.0D, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    // In this case, that just means it's not wavering around a whole bunch.
                    assertInEpsilon(tΔ16, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "ExceedMemoryLimitNoGCPercent"u8, // Same as the previous test, but with gcPercent = -1.

            gcPercent: -1,
            memoryLimit: ((int64)512 << (int)(20)),
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(3.5D).sum(ramp(-2.5D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ17, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n < 10) {
                    {
                        var goal = c[n - 1].heapGoal; if (goal != applyMemoryLimitHeapGoalHeadroom(((uint64)512 << (int)(20)))) {
                            tΔ17.Errorf("heap goal is not the heap limit: %d"u8, goal);
                        }
                    }
                }
                if (n > 12) {
                    // We're way over the memory limit, so we want to make sure our goal is set
                    // as low as it possibly can be.
                    {
                        var (goal, live) = (c[n - 1].heapGoal, c[n - 1].heapLive); if (goal != live) {
                            tΔ17.Errorf("heap goal is not equal to live heap: %d != %d"u8, goal, live);
                        }
                    }
                }
                if (n >= 25) {
                    // Due to memory pressure, we should scale to 100% GC CPU utilization.
                    // Note that in practice this won't actually happen because of the CPU limiter,
                    // but it's not the pacer's job to limit CPU usage.
                    assertInEpsilon(tΔ17, gcUtilizationˢ, c[n - 1].gcUtilization, 1.0D, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles.
                    // In this case, that just means it's not wavering around a whole bunch.
                    assertInEpsilon(tΔ17, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "MaintainMemoryLimit"u8, // This test ensures that the pacer maintains the memory limit as the heap grows.

            gcPercent: 100,
            memoryLimit: ((int64)512 << (int)(20)),
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(3.0D).sum(ramp(-2.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ18, slice<gcCycleResult> c) => {
                nint n = len(c);
                if (n > 12) {
                    // We're trying to saturate the memory limit.
                    {
                        var goal = c[n - 1].heapGoal; if (goal != applyMemoryLimitHeapGoalHeadroom(((uint64)512 << (int)(20)))) {
                            tΔ18.Errorf("heap goal is not the heap limit: %d"u8, goal);
                        }
                    }
                }
                if (n >= 25) {
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization,
                    // even with the additional memory pressure.
                    assertInEpsilon(tΔ18, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles and
                    // that it's meeting its goal.
                    assertInEpsilon(tΔ18, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ18, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            })),
        Ꮡ(new gcExecTest(
            name: "MaintainMemoryLimitNoGCPercent"u8, // Same as the previous test, but with gcPercent = -1.

            gcPercent: -1,
            memoryLimit: ((int64)512 << (int)(20)),
            globalsBytes: ((uint64)32 << (int)(10)),
            nCores: 8,
            allocRate: constant(33.0D),
            scanRate: constant(1024.0D),
            growthRate: constant(3.0D).sum(ramp(-2.0D, 12)),
            scannableFrac: constant(1.0D),
            stackBytes: constant(8192D),
            length: 50,
            checker: (ж<testing.T> tΔ19, slice<gcCycleResult> c) => {
                nint n = len(c);
                {
                    var goal = c[n - 1].heapGoal; if (goal != applyMemoryLimitHeapGoalHeadroom(((uint64)512 << (int)(20)))) {
                        tΔ19.Errorf("heap goal is not the heap limit: %d"u8, goal);
                    }
                }
                if (n >= 25) {
                    // At this alloc/scan rate, the pacer should be extremely close to the goal utilization,
                    // even with the additional memory pressure.
                    assertInEpsilon(tΔ19, gcUtilizationˢ, c[n - 1].gcUtilization, runtime_internal_test_package.GCGoalUtilization, 0.005D);
                    // Make sure the pacer settles into a non-degenerate state in at least 25 GC cycles and
                    // that it's meeting its goal.
                    assertInEpsilon(tΔ19, gcUtilizationˢ, c[n - 1].gcUtilization, c[n - 2].gcUtilization, 0.005D);
                    assertInRange(tΔ19, goalRatioˢ, c[n - 1].goalRatio(), 0.95D, 1.05D);
                }
            }))
    }.slice()) {
        // TODO(mknyszek): Write a test that exercises the pacer's hard goal.
        // This is difficult in the idealized model this testing framework places
        // the pacer in, because the calculated overshoot is directly proportional
        // to the runway for the case of the expected work.
        // However, it is still possible to trigger this case if something exceptional
        // happens between calls to revise; the framework just doesn't support this yet.
        var eΔ1 = e;
        var eʗ1 = eΔ1;
        Ꮡt.Run((~eΔ1).name, (ж<testing.T> tΔ20) => {
            tΔ20.Parallel();
            var c = runtime_internal_test_package.NewGCController((~eʗ1).gcPercent, (~eʗ1).memoryLimit);
            int64 bytesAllocatedBlackLast = default!;
            var results = new slice<gcCycleResult>(0, (~eʗ1).length);
            for (nint i = 0; i < (~eʗ1).length; i++) {
                var cycle = eʗ1.next();
                c.StartCycle(cycle.stackBytes, (~eʗ1).globalsBytes, cycle.scannableFrac, (~eʗ1).nCores);
                // Update pacer incrementally as we complete scan work.
                time.Duration revisePeriod = /* 500 * time.Microsecond */ 500000;
                
                const float64 rateConv = /* 1024 * float64(revisePeriod) / float64(time.Millisecond) */ 512;
                int64 nextHeapMarked = default!;
                if (i == 0){
                    nextHeapMarked = initialHeapBytes;
                } else {
                    nextHeapMarked = (int64)((float64)((int64)c.HeapMarked() - bytesAllocatedBlackLast) * cycle.growthRate);
                }
                ref var globalsScanWorkLeft = ref heap<int64>(out var ᏑglobalsScanWorkLeft);
                ᏑglobalsScanWorkLeft.Value = (int64)(~eʗ1).globalsBytes;
                ref var stackScanWorkLeft = ref heap<int64>(out var ᏑstackScanWorkLeft);
                ᏑstackScanWorkLeft.Value = (int64)cycle.stackBytes;
                ref var heapScanWorkLeft = ref heap<int64>(out var ᏑheapScanWorkLeft);
                ᏑheapScanWorkLeft.Value = (int64)((float64)nextHeapMarked * cycle.scannableFrac);
                (int64, int64, int64) doWork(int64 work) {
                    array<int64> deltas = new(3);
                    // Do globals work first, then stacks, then heap.
                    foreach (var (iΔ1, workLeft) in new ж<int64>[]{ᏑglobalsScanWorkLeft, ᏑstackScanWorkLeft, ᏑheapScanWorkLeft}.slice()) {
                        if (workLeft.Value == 0) {
                            continue;
                        }
                        if (workLeft.Value > work){
                            deltas[iΔ1] += work;
                            workLeft.Value -= work;
                            work = 0;
                            break;
                        } else {
                            deltas[iΔ1] += workLeft.Value;
                            work -= workLeft.Value;
                            workLeft.Value = 0;
                        }
                    }
                    return (deltas[0], deltas[1], deltas[2]);
                }
                int64 gcDuration = default!;
                int64 assistTime = default!;
                int64 bytesAllocatedBlack = default!;
                while (ᏑheapScanWorkLeft.Value + ᏑstackScanWorkLeft.Value + ᏑglobalsScanWorkLeft.Value > 0) {
                    // Simulate GC assist pacing.
                    //
                    // Note that this is an idealized view of the GC assist pacing
                    // mechanism.
                    // From the assist ratio and the alloc and scan rates, we can idealize what
                    // the GC CPU utilization looks like.
                    //
                    // We start with assistRatio = (bytes of scan work) / (bytes of runway) (by definition).
                    //
                    // Over revisePeriod, we can also calculate how many bytes are scanned and
                    // allocated, given some GC CPU utilization u:
                    //
                    //     bytesScanned   = scanRate  * rateConv * nCores * u
                    //     bytesAllocated = allocRate * rateConv * nCores * (1 - u)
                    //
                    // During revisePeriod, assistRatio is kept constant, and GC assists kick in to
                    // maintain it. Specifically, they act to prevent too many bytes being allocated
                    // compared to how many bytes are scanned. It directly defines the ratio of
                    // bytesScanned to bytesAllocated over this period, hence:
                    //
                    //     assistRatio = bytesScanned / bytesAllocated
                    //
                    // From this, we can solve for utilization, because everything else has already
                    // been determined:
                    //
                    //     assistRatio = (scanRate * rateConv * nCores * u) / (allocRate * rateConv * nCores * (1 - u))
                    //     assistRatio = (scanRate * u) / (allocRate * (1 - u))
                    //     assistRatio * allocRate * (1-u) = scanRate * u
                    //     assistRatio * allocRate - assistRatio * allocRate * u = scanRate * u
                    //     assistRatio * allocRate = assistRatio * allocRate * u + scanRate * u
                    //     assistRatio * allocRate = (assistRatio * allocRate + scanRate) * u
                    //     u = (assistRatio * allocRate) / (assistRatio * allocRate + scanRate)
                    //
                    // Note that this may give a utilization that is _less_ than GCBackgroundUtilization,
                    // which isn't possible in practice because of dedicated workers. Thus, this case
                    // must be interpreted as GC assists not kicking in at all, and just round up. All
                    // downstream values will then have this accounted for.
                    var assistRatio = c.AssistWorkPerByte();
                    var utilization = assistRatio * cycle.allocRate / (assistRatio * cycle.allocRate + cycle.scanRate);
                    if (utilization < runtime_internal_test_package.GCBackgroundUtilization) {
                        utilization = runtime_internal_test_package.GCBackgroundUtilization;
                    }
                    // Knowing the utilization, calculate bytesScanned and bytesAllocated.
                    var bytesScanned = (int64)(cycle.scanRate * rateConv * (float64)(~eʗ1).nCores * utilization);
                    var bytesAllocated = (int64)(cycle.allocRate * rateConv * (float64)(~eʗ1).nCores * (1D - utilization));
                    // Subtract work from our model.
                    var (globalsScanned, stackScanned, heapScanned) = doWork(bytesScanned);
                    // doWork may not use all of bytesScanned.
                    // In this case, the GC actually ends sometime in this period.
                    // Let's figure out when, exactly, and adjust bytesAllocated too.
                    var actualElapsed = revisePeriod;
                    var actualAllocated = bytesAllocated;
                    {
                        var actualScanned = globalsScanned + stackScanned + heapScanned; if (actualScanned < bytesScanned) {
                            // actualScanned = scanRate * rateConv * (t / revisePeriod) * nCores * u
                            // => t = actualScanned * revisePeriod / (scanRate * rateConv * nCores * u)
                            actualElapsed = ((time.Duration)(int64)((float64)actualScanned * (float64)(int64)revisePeriod / (cycle.scanRate * rateConv * (float64)(~eʗ1).nCores * utilization)));
                            actualAllocated = (int64)(cycle.allocRate * rateConv * (float64)(int64)actualElapsed / (float64)(int64)revisePeriod * (float64)(~eʗ1).nCores * (1D - utilization));
                        }
                    }
                    // Ask the pacer to revise.
                    c.Revise(new runtime_internal_test_package.GCControllerReviseDelta(
                        HeapLive: actualAllocated,
                        HeapScan: (int64)((float64)actualAllocated * cycle.scannableFrac),
                        HeapScanWork: heapScanned,
                        StackScanWork: stackScanned,
                        GlobalsScanWork: globalsScanned
                    ));
                    // Accumulate variables.
                    assistTime += (int64)((float64)(int64)actualElapsed * (float64)(~eʗ1).nCores * (utilization - (float64)runtime_internal_test_package.GCBackgroundUtilization));
                    gcDuration += (int64)actualElapsed;
                    bytesAllocatedBlack += actualAllocated;
                }
                // Put together the results, log them, and concatenate them.
                var result = new gcCycleResult(
                    cycle: i + 1,
                    heapLive: c.HeapMarked(),
                    heapScannable: (int64)((float64)((int64)c.HeapMarked() - bytesAllocatedBlackLast) * cycle.scannableFrac),
                    heapTrigger: c.Triggered(),
                    heapPeak: c.HeapLive(),
                    heapGoal: c.HeapGoal(),
                    gcUtilization: (float64)assistTime / ((float64)gcDuration * (float64)(~eʗ1).nCores) + (float64)runtime_internal_test_package.GCBackgroundUtilization
                );
                tΔ20.Log((@string)"GC"u8, result.String());
                results = append(results, result);
                // Run the checker for this test.
                eʗ1.check(tΔ20, results);
                c.EndCycle((uint64)(nextHeapMarked + bytesAllocatedBlack), assistTime, gcDuration, (~eʗ1).nCores);
                bytesAllocatedBlackLast = bytesAllocatedBlack;
            }
        });
    }
}

partial struct gcExecTest {
    internal @string name;
    internal nint gcPercent;
    internal int64 memoryLimit;
    internal uint64 globalsBytes;
    internal nint nCores;
    internal float64Stream allocRate; // > 0, KiB / cpu-ms
    internal float64Stream scanRate; // > 0, KiB / cpu-ms
    internal float64Stream growthRate; // > 0
    internal float64Stream scannableFrac; // Clamped to [0, 1]
    internal float64Stream stackBytes; // Multiple of 2048.
    internal nint length;
    internal Action<ж<testing.T>, slice<gcCycleResult>> checker;
}

// minRate is an arbitrary minimum for allocRate, scanRate, and growthRate.
// These values just cannot be zero.
internal static UntypedFloat minRate => 0.0001;

internal static gcCycle next(this ref gcExecTest e) {
    return new gcCycle(
        allocRate: e.allocRate.min(minRate)(),
        scanRate: e.scanRate.min(minRate)(),
        growthRate: e.growthRate.min(minRate)(),
        scannableFrac: e.scannableFrac.limit(0D, 1D)(),
        stackBytes: (uint64)e.stackBytes.quantize(2048D).min(0D)()
    );
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noResultsPassedToCheckˢ = (@string)"no results passed to check"u8;
internal static readonly object firstCycleHasIncorrectˢ = (@string)"first cycle has incorrect number"u8;
internal static readonly object cycleNumbersOutOfOrderˢ = (@string)"cycle numbers out of order"u8;
internal static readonly object gcUtilizationNotWithinˢ = (@string)"GC utilization not within acceptable bounds"u8;
internal static readonly object heapScannableIsNegativeˢ = (@string)"heapScannable is negative"u8;
internal static readonly object testSpecificCheckerIsˢ = (@string)"test-specific checker is missing"u8;

internal static void check(this ref gcExecTest e, ж<testing.T> Ꮡt, slice<gcCycleResult> results) {
    Ꮡt.Helper();
    // Do some basic general checks first.
    nint n = len(results);
    switch (n) {
    case 0: {
        Ꮡt.Fatal(noResultsPassedToCheckˢ);
        return;
    }
    case 1: {
        if (results[0].cycle != 1) {
            Ꮡt.Error(firstCycleHasIncorrectˢ);
        }
        break;
    }
    default: {
        if (results[n - 1].cycle != results[n - 2].cycle + 1) {
            Ꮡt.Error(cycleNumbersOutOfOrderˢ);
        }
        break;
    }}

    {
        var u = results[n - 1].gcUtilization; if (u < 0D || u > 1D) {
            Ꮡt.Fatal(gcUtilizationNotWithinˢ);
        }
    }
    {
        var s = results[n - 1].heapScannable; if (s < 0) {
            Ꮡt.Fatal(heapScannableIsNegativeˢ);
        }
    }
    if (e.checker == default!) {
        Ꮡt.Fatal(testSpecificCheckerIsˢ);
    }
    // Run the test-specific checker.
    e.checker(Ꮡt, results);
}

partial struct gcCycle {
    internal float64 allocRate;
    internal float64 scanRate;
    internal float64 growthRate;
    internal float64 scannableFrac;
    internal uint64 stackBytes;
}

partial struct gcCycleResult {
    internal nint cycle;
    // These come directly from the pacer, so uint64.
    internal uint64 heapLive;
    internal uint64 heapTrigger;
    internal uint64 heapGoal;
    internal uint64 heapPeak;
    // These are produced by the simulation, so int64 and
    // float64 are more appropriate, so that we can check for
    // bad states in the simulation.
    internal int64 heapScannable;
    internal float64 gcUtilization;
}

internal static float64 goalRatio(this ref gcCycleResult r) {
    return (float64)r.heapPeak / (float64)r.heapGoal;
}

internal static float64 runway(this ref gcCycleResult r) {
    return (float64)(r.heapGoal - r.heapTrigger);
}

internal static float64 triggerRatio(this ref gcCycleResult r) {
    return (float64)(r.heapTrigger - r.heapLive) / (float64)(r.heapGoal - r.heapLive);
}

internal static @string String(this ref gcCycleResult r) {
    return fmt.Sprintf("%d %2.1f%% %d->%d->%d (goal: %d)"u8, r.cycle, r.gcUtilization * 100D, r.heapLive, r.heapTrigger, r.heapPeak, r.heapGoal);
}

internal static void assertInEpsilon(ж<testing.T> Ꮡt, @string name, float64 a, float64 b, float64 epsilon) {
    Ꮡt.Helper();
    assertInRange(Ꮡt, name, a, b - epsilon, b + epsilon);
}

internal static void assertInRange(ж<testing.T> Ꮡt, @string name, float64 a, float64 min, float64 max) {
    Ꮡt.Helper();
    if (a < min || a > max) {
        Ꮡt.Errorf("%s not in range (%f, %f): %f"u8, name, min, max, a);
    }
}

internal delegate float64 float64Stream();

// constant returns a stream that generates the value c.
internal static float64Stream constant(float64 c) {
    return () => c;
}

// unit returns a stream that generates a single peak with
// amplitude amp, followed by zeroes.
//
// In another manner of speaking, this is the Kronecker delta.
internal static float64Stream unit(float64 amp) {
    var dropped = false;
    return () => {
        if (dropped) {
            return 0D;
        }
        dropped = true;
        return amp;
    };
}

// oscillate returns a stream that oscillates sinusoidally
// with the given amplitude, phase, and period.
internal static float64Stream oscillate(float64 amp, float64 phase, nint period) {
    nint cycle = default!;
    return () => {
        var p = (float64)cycle / (float64)period * 2D * (float64)Δmath.Pi + phase;
        cycle++;
        if (cycle == period) {
            cycle = 0;
        }
        return Δmath.Sin(p) * amp;
    };
}

// ramp returns a stream that moves from zero to height
// over the course of length steps.
internal static float64Stream ramp(float64 height, nint length) {
    nint cycle = default!;
    return () => {
        var h = height * (float64)cycle / (float64)length;
        if (cycle < length) {
            cycle++;
        }
        return h;
    };
}

// random returns a stream that generates random numbers
// between -amp and amp.
internal static float64Stream random(float64 amp, int64 seed) {
    var r = rand.New(rand.NewSource(seed));
    var rʗ1 = r;
    return () => ((rʗ1.Float64() - 0.5D) * 2D) * amp;
}

// delay returns a new stream which is a buffered version
// of f: it returns zero for cycles steps, followed by f.
internal static float64Stream delay(this float64Stream f, nint cycles) {
    nint zeroes = 0;
    return () => {
        if (zeroes < cycles) {
            zeroes++;
            return 0D;
        }
        return f();
    };
}

// scale returns a new stream that is f, but attenuated by a
// constant factor.
internal static float64Stream scale(this float64Stream f, float64 amt) {
    return () => f() * amt;
}

// offset returns a new stream that is f but offset by amt
// at each step.
internal static float64Stream offset(this float64Stream f, float64 amt) {
    return () => {
        var old = f();
        return old + amt;
    };
}

// sum returns a new stream that is the sum of all input streams
// at each step.
internal static float64Stream sum(this float64Stream f, params ꓸꓸꓸfloat64Stream fsʗp) {
    var fs = fsʗp.slice();

    var fsʗ1 = fs;
    return () => {
        var sum = f();
        foreach (var (_, s) in fsʗ1) {
            sum += s();
        }
        return sum;
    };
}

// quantize returns a new stream that rounds f to a multiple
// of mult at each step.
internal static float64Stream quantize(this float64Stream f, float64 mult) {
    return () => {
        var r = f() / mult;
        if (r < 0D) {
            return Δmath.Ceil(r) * mult;
        }
        return Δmath.Floor(r) * mult;
    };
}

// min returns a new stream that replaces all values produced
// by f lower than min with min.
internal static float64Stream min(this float64Stream f, float64 min) {
    return () => Δmath.Max(min, f());
}

// max returns a new stream that replaces all values produced
// by f higher than max with max.
internal static float64Stream max(this float64Stream f, float64 max) {
    return () => Δmath.Min(max, f());
}

// limit returns a new stream that replaces all values produced
// by f lower than min with min and higher than max with max.
internal static float64Stream limit(this float64Stream f, float64 min, float64 max) {
    return () => {
        var v = f();
        if (v < min){
            v = min;
        } else 
        if (v > max) {
            v = max;
        }
        return v;
    };
}

internal static uint64 applyMemoryLimitHeapGoalHeadroom(uint64 goal) {
    var headroom = goal / 100 * (uint64)runtime_internal_test_package.MemoryLimitHeapGoalHeadroomPercent;
    if (headroom < runtime_internal_test_package.MemoryLimitMinHeapGoalHeadroom) {
        headroom = runtime_internal_test_package.MemoryLimitMinHeapGoalHeadroom;
    }
    if (goal < headroom || goal - headroom < headroom){
        goal = headroom;
    } else {
        goal -= headroom;
    }
    return goal;
}

public static void TestIdleMarkWorkerCount(ж<testing.T> Ꮡt) {
    UntypedInt workers = 10;
    var c = runtime_internal_test_package.NewGCController(100, Δmath.MaxInt64);
    c.SetMaxIdleMarkWorkers(workers);
    for (nint i = 0; i < workers; i++) {
        if (!c.NeedIdleMarkWorker()) {
            Ꮡt.Fatalf("expected to need idle mark workers: i=%d"u8, i);
        }
        if (!c.AddIdleMarkWorker()) {
            Ꮡt.Fatalf("expected to be able to add an idle mark worker: i=%d"u8, i);
        }
    }
    if (c.NeedIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to not need idle mark workers"u8);
    }
    if (c.AddIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to not be able to add an idle mark worker"u8);
    }
    for (nint i = 0; i < workers; i++) {
        c.RemoveIdleMarkWorker();
        if (!c.NeedIdleMarkWorker()) {
            Ꮡt.Fatalf("expected to need idle mark workers after removal: i=%d"u8, i);
        }
    }
    for (nint i = 0; i < (nint)(workers - 1); i++) {
        if (!c.AddIdleMarkWorker()) {
            Ꮡt.Fatalf("expected to be able to add idle mark workers after adding again: i=%d"u8, i);
        }
    }
    for (nint i = 0; i < 10; i++) {
        if (!c.AddIdleMarkWorker()) {
            Ꮡt.Fatalf("expected to be able to add idle mark workers interleaved: i=%d"u8, i);
        }
        if (c.AddIdleMarkWorker()) {
            Ꮡt.Fatalf("expected to not be able to add idle mark workers interleaved: i=%d"u8, i);
        }
        c.RemoveIdleMarkWorker();
    }
    // Support the max being below the count.
    c.SetMaxIdleMarkWorkers(0);
    if (c.NeedIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to not need idle mark workers after capacity set to 0"u8);
    }
    if (c.AddIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to not be able to add idle mark workers after capacity set to 0"u8);
    }
    for (nint i = 0; i < (nint)(workers - 1); i++) {
        c.RemoveIdleMarkWorker();
    }
    if (c.NeedIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to not need idle mark workers after capacity set to 0"u8);
    }
    if (c.AddIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to not be able to add idle mark workers after capacity set to 0"u8);
    }
    c.SetMaxIdleMarkWorkers(1);
    if (!c.NeedIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to need idle mark workers after capacity set to 1"u8);
    }
    if (!c.AddIdleMarkWorker()) {
        Ꮡt.Fatalf("expected to be able to add idle mark workers after capacity set to 1"u8);
    }
}

} // end runtime_test_package
