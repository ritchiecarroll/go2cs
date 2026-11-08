// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δtrace = go.@internal.trace_package;
using testtrace = go.@internal.trace.testtrace_package;
using io = io_package;
using testing = testing_package;
using go.@internal;
using go.@internal.trace;
using static go.@internal.trace_internal_test_package;
using time = time_package;
using version = go.@internal.trace.version_package;

partial class trace_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string runtimeGcBgMarkWorkerˢ => ref runtimeGcBgMarkWorkerˢᵇ;
private static readonly @string runtimeGcBgMarkWorkerˢᵇ = "runtime.gcBgMarkWorker"u8;
internal static ref readonly @string mainMainFunc1ˢ => ref mainMainFunc1ˢᵇ;
private static readonly @string mainMainFunc1ˢᵇ = "main.main.func1"u8;
internal static ref readonly @string syncˢ => ref syncˢᵇ;
private static readonly @string syncˢᵇ = "sync"u8;
internal static ref readonly @string gcMarkAssistˢ => ref gcMarkAssistˢᵇ;
private static readonly @string gcMarkAssistˢᵇ = "GC mark assist"u8;
internal static readonly object missingSchedWaitTimeˢ = (@string)"missing sched wait time"u8;
internal static readonly object missingSyncBlockTimeˢ = (@string)"missing sync block time"u8;
internal static readonly object missingGcMarkAssistTimeˢ = (@string)"missing GC mark assist time"u8;
internal static readonly object hasTimeThatIsUnaccountedˢ = (@string)"has time that is unaccounted for"u8;

public static void TestSummarizeGoroutinesTrace(ж<testing.T> t) {
    var summaries = summarizeTraceTest(t, testdataTestsGo122Gcˢ).Goroutines;
    bool hasSchedWaitTime = default;
    bool hasSyncBlockTime = default;
    bool hasGCMarkAssistTime = default;
    bool hasUnknownTime = default;

    assertContainsGoroutine(t, summaries, runtimeGcBgMarkWorkerˢ);
    assertContainsGoroutine(t, summaries, mainMainFunc1ˢ);

    foreach (var (_, summary) in summaries) {
        basicGoroutineSummaryChecks(t, summary);
        hasSchedWaitTime = hasSchedWaitTime || summary.SchedWaitTime > 0;
        {
            var (dt, ok) = summary.BlockTimeByReason[syncˢ, ꟷ]; if (ok && dt > 0) {
                hasSyncBlockTime = true;
            }
        }
        {
            var (dt, ok) = summary.RangeTime[gcMarkAssistˢ, ꟷ]; if (ok && dt > 0) {
                hasGCMarkAssistTime = true;
            }
        }
        hasUnknownTime = hasUnknownTime || summary.UnknownTime() > 0;
    }
    if (!hasSchedWaitTime) {
        t.Error(missingSchedWaitTimeˢ);
    }
    if (!hasSyncBlockTime) {
        t.Error(missingSyncBlockTimeˢ);
    }
    if (!hasGCMarkAssistTime) {
        t.Error(missingGcMarkAssistTimeˢ);
    }
    if (hasUnknownTime) {
        t.Error(hasTimeThatIsUnaccountedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTestsGo122ˢ => ref testdataTestsGo122ˢᵇ;
private static readonly @string testdataTestsGo122ˢᵇ = "testdata/tests/go122-annotations.test"u8;

internal partial struct TestSummarizeGoroutinesRegionsTrace_region /*dyn*/ {
    internal Δtrace.EventKind startKind;
    internal Δtrace.EventKind endKind;
}

public static void TestSummarizeGoroutinesRegionsTrace(ж<testing.T> t) {
    var summaries = summarizeTraceTest(t, testdataTestsGo122ˢ).Goroutines;
    var wantRegions = new map<@string, TestSummarizeGoroutinesRegionsTrace_region>{
        // N.B. "pre-existing region" never even makes it into the trace.
        //
        // TODO(mknyszek): Add test case for end-without-a-start, which can happen at
        // a generation split only.
        [""] = new(Δtrace.EventStateTransition, Δtrace.EventStateTransition), // Task inheritance marker.
        ["task0 region"] = new(Δtrace.EventRegionBegin, Δtrace.EventBad),
        ["region0"] = new(Δtrace.EventRegionBegin, Δtrace.EventRegionEnd),
        ["region1"] = new(Δtrace.EventRegionBegin, Δtrace.EventRegionEnd),
        ["unended region"] = new(Δtrace.EventRegionBegin, Δtrace.EventStateTransition),
        ["post-existing region"] = new(Δtrace.EventRegionBegin, Δtrace.EventBad)
    };
    foreach (var (_, summary) in summaries) {
        basicGoroutineSummaryChecks(t, summary);
        foreach (var (_, region) in range(summary.Regions)) {
            var (want, ok) = wantRegions[region.Name, ꟷ];
            if (!ok) {
                continue;
            }
            checkRegionEvents(t, want.startKind, want.endKind, summary.ID, region);
            delete(wantRegions, region.Name);
        }
    }
    if (len(wantRegions) != 0) {
        t.Errorf("failed to find regions: %#v", wantRegions);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTestsGo122ˢ2 => ref testdataTestsGo122ˢ2ᵇ;
private static readonly @string testdataTestsGo122ˢ2ᵇ = "testdata/tests/go122-annotations-stress.test"u8;

internal partial struct TestSummarizeTasksTrace_task /*dyn*/ {
    internal @string name;
    internal ж<Δtrace.TaskID> parent;
    internal slice<Δtrace.TaskID> children;
    internal slice<traceꓸLog> logs;
    internal slice<Δtrace.GoID> goroutines;
}

public static void TestSummarizeTasksTrace(ж<testing.T> t) {
    var summaries = summarizeTraceTest(t, testdataTestsGo122ˢ2).Tasks;
    ж<Δtrace.TaskID> parent(Δtrace.TaskID id) {
        var p = @new<Δtrace.TaskID>();
        p.Value = id;
        return p;
    }
    var wantTasks = new map<Δtrace.TaskID, TestSummarizeTasksTrace_task>{
        [Δtrace.BackgroundTask] = new(
            // The background task (0) is never any task's parent.
            logs: [
                new(Task: Δtrace.BackgroundTask, Category: "log", Message: "before do"),
                new(Task: Δtrace.BackgroundTask, Category: "log", Message: "before do")
            ],
            goroutines: [1]
        ),
        [1] = new(
            // This started before tracing started and has no parents.
            // Task 2 is technically a child, but we lost that information.
            children: [3, 7, 16],
            logs: [
                new(Task: 1, Category: "log", Message: "before do"),
                new(Task: 1, Category: "log", Message: "before do")
            ],
            goroutines: [1]
        ),
        [2] = new(
            // This started before tracing started and its parent is technically (1), but that information was lost.
            children: [8, 17],
            logs: [
                new(Task: 2, Category: "log", Message: "before do"),
                new(Task: 2, Category: "log", Message: "before do")
            ],
            goroutines: [1]
        ),
        [3] = new(
            parent: parent(1),
            children: [10, 19],
            logs: [
                new(Task: 3, Category: "log", Message: "before do"),
                new(Task: 3, Category: "log", Message: "before do")
            ],
            goroutines: [1]
        ),
        [4] = new(
            // Explicitly, no parent.
            children: [12, 21],
            logs: [
                new(Task: 4, Category: "log", Message: "before do"),
                new(Task: 4, Category: "log", Message: "before do")
            ],
            goroutines: [1]
        ),
        [12] = new(
            parent: parent(4),
            children: [13],
            logs: [
                // TODO(mknyszek): This is computed asynchronously in the trace,
                // which makes regenerating this test very annoying, since it will
                // likely break this test. Resolve this by making the order not matter.
                new(Task: 12, Category: "log2", Message: "do"),
                new(Task: 12, Category: "log", Message: "fanout region4"),
                new(Task: 12, Category: "log", Message: "fanout region0"),
                new(Task: 12, Category: "log", Message: "fanout region1"),
                new(Task: 12, Category: "log", Message: "fanout region2"),
                new(Task: 12, Category: "log", Message: "before do"),
                new(Task: 12, Category: "log", Message: "fanout region3")
            ],
            goroutines: [1, 5, 6, 7, 8, 9]
        ),
        [13] = new(
            // Explicitly, no children.
            parent: parent(12),
            logs: [
                new(Task: 13, Category: "log2", Message: "do")
            ],
            goroutines: [7]
        )
    };
    foreach (var (id, summary) in summaries) {
        var (want, ok) = wantTasks[id, ꟷ];
        if (!ok) {
            continue;
        }
        if (id != summary.ID) {
            t.Errorf("ambiguous task %d (or %d?): field likely set incorrectly", id, summary.ID);
        }

        // Check parent.
        if (want.parent != nil) {
            if (summary.Parent == nil) {
                t.Errorf("expected parent %d for task %d without a parent", want.parent.Value, id);
            } else if (summary.Parent.ID != want.parent.Value) {
                t.Errorf("bad parent for task %d: want %d, got %d", id, want.parent.Value, summary.Parent.ID);
            }
        } else if (summary.Parent != nil) {
            t.Errorf("unexpected parent %d for task %d", summary.Parent.ID, id);
        }

        // Check children.
        var gotChildren = new map<Δtrace.TaskID, EmptyStruct>();
        foreach (var (_, child) in range(summary.Children)) {
            gotChildren[child.ID] = new EmptyStruct();
        }
        foreach (var (_, wantChild) in range(want.children)) {
            var (_, okΔ1) = gotChildren[wantChild, ꟷ]; if (okΔ1) {
                delete(gotChildren, wantChild);
            } else {
                t.Errorf("expected child task %d for task %d not found", wantChild, id);
            }
        }
        if (len(gotChildren) != 0) {
            foreach (var (child, _) in gotChildren) {
                t.Errorf("unexpected child task %d for task %d", child, id);
            }
        }

        // Check logs.
        if (len(want.logs) != len(summary.Logs)) {
            t.Errorf("wanted %d logs for task %d, got %d logs instead", len(want.logs), id, len(summary.Logs));
        } else {
            foreach (var i in range(len(want.logs))) {
                if (want.logs[i] != summary.Logs[i].Log()) {
                    t.Errorf("log mismatch: want %#v, got %#v", want.logs[i], summary.Logs[i].Log());
                }
            }
        }

        // Check goroutines.
        if (len(want.goroutines) != len(summary.Goroutines)) {
            t.Errorf("wanted %d goroutines for task %d, got %d goroutines instead", len(want.goroutines), id, len(summary.Goroutines));
        } else {
            foreach (var (_, goid) in range(want.goroutines)) {
                var (g, okΔ2) = summary.Goroutines[goid, ꟷ];
                if (!okΔ2) {
                    t.Errorf("want goroutine %d for task %d, not found", goid, id);
                    continue;
                }
                if (g.ID != goid) {
                    t.Errorf("goroutine summary for %d does not match task %d listing of %d", g.ID, id, goid);
                }
            }
        }

        // Marked as seen.
        delete(wantTasks, id);
    }
    if (len(wantTasks) != 0) {
        t.Errorf("failed to find tasks: %#v", wantTasks);
    }
}

internal static void assertContainsGoroutine(ж<testing.T> t, map<Δtrace.GoID, ж<Δtrace.GoroutineSummary>> summaries, @string name) {
    foreach (var (_, summary) in summaries) {
        if (summary.Name == name) {
            return;
        }
    }
    t.Errorf("missing goroutine %s", name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object summaryFoundForNoˢ = (@string)"summary found for no goroutine"u8;

internal static void basicGoroutineSummaryChecks(ж<testing.T> t, ж<Δtrace.GoroutineSummary> Ꮡsummary) {
    ref var summary = ref Ꮡsummary.DerefOrNull();

    if (summary.ID == Δtrace.NoGoroutine) {
        t.Error(summaryFoundForNoˢ);
        return;
    }
    if ((summary.StartTime != 0 && summary.CreationTime > summary.StartTime) || (summary.StartTime != 0 && summary.EndTime != 0 && summary.StartTime > summary.EndTime)) {
        t.Errorf("bad summary creation/start/end times for G %d: creation=%d start=%d end=%d", summary.ID, summary.CreationTime, summary.StartTime, summary.EndTime);
    }
    if ((summary.PC != 0 && summary.Name == ""u8) || (summary.PC == 0 && summary.Name != ""u8)) {
        t.Errorf("bad name and/or PC for G %d: pc=0x%x name=%q", summary.ID, summary.PC, summary.Name);
    }
    basicGoroutineExecStatsChecks(t, Ꮡsummary.ᏑGoroutineExecStats);
    foreach (var (_, region) in range(summary.Regions)) {
        basicGoroutineExecStatsChecks(t, region.ᏑGoroutineExecStats);
    }
}

internal static ж<Δtrace.Summary> summarizeTraceTest(ж<testing.T> t, @string testPath) {
    var (trc, _, _, err) = testtrace.ParseFile(testPath);
    if (err != nil) {
        t.Fatalf("malformed test %s: bad trace file: %v", testPath, err);
    }
    // Create the analysis state.
    var s = Δtrace.NewSummarizer();

    // Create a reader.
    (var r, err) = Δtrace.NewReader(trc);
    if (err != nil) {
        t.Fatalf("failed to create trace reader for %s: %v", testPath, err);
    }
    // Process the trace.
    for (;;) {
        ref var ev = ref heap<traceꓸEvent>(out var Ꮡev);
        (ev, var errΔ1) = r.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != nil) {
            t.Fatalf("failed to process trace %s: %v", testPath, errΔ1);
        }
        s.Event(Ꮡev);
    }
    return s.ΔFinalize();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedNonNilRegionˢ = (@string)"expected non-nil region start event, got nil"u8;
internal static readonly object expectedNonNilRegionEndˢ = (@string)"expected non-nil region end event, got nil"u8;

internal static void checkRegionEvents(ж<testing.T> t, Δtrace.EventKind wantStart, Δtrace.EventKind wantEnd, Δtrace.GoID goid, ж<Δtrace.UserRegionSummary> Ꮡregion) {
    ref var region = ref Ꮡregion.DerefOrNull();

    switch (wantStart) {
    case var ᴛ when ᴛ == Δtrace.EventBad: {
        if (region.Start != nil) {
            t.Errorf("expected nil region start event, got\n%s", region.Start.String());
        }
        break;
    }
    case var ᴛ when ᴛ == Δtrace.EventStateTransition || ᴛ == Δtrace.EventRegionBegin: {
        if (region.Start == nil) {
            t.Error(expectedNonNilRegionˢ);
        }
        var kind = region.Start.Kind();
        if (kind != wantStart) {
            t.Errorf("wanted region start event %s, got %s", wantStart, kind);
        }
        if (kind == Δtrace.EventRegionBegin) {
            if (region.Start.Region().Type != region.Name) {
                t.Errorf("region name mismatch: event has %s, summary has %s", region.Start.Region().Type, region.Name);
            }
        } else {
            var st = region.Start.StateTransition();
            if (st.Resource.Kind != Δtrace.ResourceGoroutine) {
                t.Errorf("found region start event for the wrong resource: %s", st.Resource);
            }
            if (st.Resource.Goroutine() != goid) {
                t.Errorf("found region start event for the wrong resource: wanted goroutine %d, got %s", goid, st.Resource);
            }
            var (old, _) = st.Goroutine(); if (old != Δtrace.GoNotExist && old != Δtrace.GoUndetermined) {
                t.Errorf("expected transition from GoNotExist or GoUndetermined, got transition from %s instead", old);
            }
        }
        break;
    }
    default: {
        t.Errorf("unexpected want start event type: %s", wantStart);
        break;
    }}

    switch (wantEnd) {
    case var ᴛ when ᴛ == Δtrace.EventBad: {
        if (region.End != nil) {
            t.Errorf("expected nil region end event, got\n%s", region.End.String());
        }
        break;
    }
    case var ᴛ when ᴛ == Δtrace.EventStateTransition || ᴛ == Δtrace.EventRegionEnd: {
        if (region.End == nil) {
            t.Error(expectedNonNilRegionEndˢ);
        }
        var kind = region.End.Kind();
        if (kind != wantEnd) {
            t.Errorf("wanted region end event %s, got %s", wantEnd, kind);
        }
        if (kind == Δtrace.EventRegionEnd) {
            if (region.End.Region().Type != region.Name) {
                t.Errorf("region name mismatch: event has %s, summary has %s", region.End.Region().Type, region.Name);
            }
        } else {
            var st = region.End.StateTransition();
            if (st.Resource.Kind != Δtrace.ResourceGoroutine) {
                t.Errorf("found region end event for the wrong resource: %s", st.Resource);
            }
            if (st.Resource.Goroutine() != goid) {
                t.Errorf("found region end event for the wrong resource: wanted goroutine %d, got %s", goid, st.Resource);
            }
            var (_, @new) = st.Goroutine(); if (@new != Δtrace.GoNotExist) {
                t.Errorf("expected transition to GoNotExist, got transition to %s instead", @new);
            }
        }
        break;
    }
    default: {
        t.Errorf("unexpected want end event type: %s", wantEnd);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object foundNegativeExecTimeˢ = (@string)"found negative ExecTime"u8;
internal static readonly object foundNegativeˢ = (@string)"found negative SchedWaitTime"u8;
internal static readonly object foundNegativeSyscallTimeˢ = (@string)"found negative SyscallTime"u8;
internal static readonly object foundNegativeˢ2 = (@string)"found negative SyscallBlockTime"u8;
internal static readonly object foundNegativeTotalTimeˢ = (@string)"found negative TotalTime"u8;

internal static void basicGoroutineExecStatsChecks(ж<testing.T> t, ж<Δtrace.GoroutineExecStats> Ꮡstats) {
    ref var stats = ref Ꮡstats.DerefOrNull();

    if (stats.ExecTime < 0) {
        t.Error(foundNegativeExecTimeˢ);
    }
    if (stats.SchedWaitTime < 0) {
        t.Error(foundNegativeˢ);
    }
    if (stats.SyscallTime < 0) {
        t.Error(foundNegativeSyscallTimeˢ);
    }
    if (stats.SyscallBlockTime < 0) {
        t.Error(foundNegativeˢ2);
    }
    if (stats.TotalTime < 0) {
        t.Error(foundNegativeTotalTimeˢ);
    }
    foreach (var (reason, dt) in stats.BlockTimeByReason) {
        if (dt < 0) {
            t.Errorf("found negative BlockTimeByReason for %s", reason);
        }
    }
    foreach (var (name, dt) in stats.RangeTime) {
        if (dt < 0) {
            t.Errorf("found negative RangeTime for range %s", name);
        }
    }
}

public static void TestRelatedGoroutinesV2Trace(ж<testing.T> t) {
    @string testPath = testdataTestsGo122Gcˢ;
    var (trc, _, _, err) = testtrace.ParseFile(testPath);
    if (err != nil) {
        t.Fatalf("malformed test %s: bad trace file: %v", testPath, err);
    }

    // Create a reader.
    (var r, err) = Δtrace.NewReader(trc);
    if (err != nil) {
        t.Fatalf("failed to create trace reader for %s: %v", testPath, err);
    }

    // Collect all the events.
    slice<traceꓸEvent> events = default;
    for (;;) {
        var (ev, errΔ1) = r.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != nil) {
            t.Fatalf("failed to process trace %s: %v", testPath, errΔ1);
        }
        events = append(events, ev.ΔClone());
    }

    // Test the function.
    var targetg = (Δtrace.GoID)86;
    var got = Δtrace.RelatedGoroutinesV2(events, targetg);
    var want = new map<Δtrace.GoID, EmptyStruct>{
        [((Δtrace.GoID)86)] = new(), // N.B. Result includes target.
        [((Δtrace.GoID)71)] = new(),
        [((Δtrace.GoID)25)] = new(),
        [((Δtrace.GoID)122)] = new()
    };
    foreach (var (goid, _) in got) {
        var (_, ok) = want[goid, ꟷ]; if (ok) {
            delete(want, goid);
        } else {
            t.Errorf("unexpected goroutine %d found in related goroutines for %d in test %s", goid, targetg, testPath);
        }
    }
    if (len(want) != 0) {
        foreach (var (goid, _) in want) {
            t.Errorf("failed to find related goroutine %d for goroutine %d in test %s", goid, targetg, testPath);
        }
    }
}

} // end trace_test_package
