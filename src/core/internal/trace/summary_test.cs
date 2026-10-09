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

partial class trace_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeGcBgMarkWorkerˢ = "runtime.gcBgMarkWorker"u8;
internal static readonly @string mainMainFunc1ˢ = "main.main.func1"u8;
internal static readonly @string syncˢ = "sync"u8;
internal static readonly @string gcMarkAssistˢ = "GC mark assist"u8;
internal static readonly object missingSchedWaitTimeˢ = (@string)"missing sched wait time"u8;
internal static readonly object missingSyncBlockTimeˢ = (@string)"missing sync block time"u8;
internal static readonly object missingGcMarkAssistTimeˢ = (@string)"missing GC mark assist time"u8;

public static void TestSummarizeGoroutinesTrace(ж<testing.T> Ꮡt) {
    var summaries = summarizeTraceTest(Ꮡt, testdataTestsGo122Gcˢ).Value.Goroutines;
    bool hasSchedWaitTime = default!;
    bool hasSyncBlockTime = default!;
    bool hasGCMarkAssistTime = default!;
    assertContainsGoroutine(Ꮡt, summaries, runtimeGcBgMarkWorkerˢ);
    assertContainsGoroutine(Ꮡt, summaries, mainMainFunc1ˢ);
    foreach (var (_, summary) in summaries) {
        basicGoroutineSummaryChecks(Ꮡt, summary);
        hasSchedWaitTime = hasSchedWaitTime || (~summary).SchedWaitTime > 0;
        {
            var (dt, ok) = (~summary).BlockTimeByReason[syncˢ, ꟷ]; if (ok && dt > 0) {
                hasSyncBlockTime = true;
            }
        }
        {
            var (dt, ok) = (~summary).RangeTime[gcMarkAssistˢ, ꟷ]; if (ok && dt > 0) {
                hasGCMarkAssistTime = true;
            }
        }
    }
    if (!hasSchedWaitTime) {
        Ꮡt.Error(missingSchedWaitTimeˢ);
    }
    if (!hasSyncBlockTime) {
        Ꮡt.Error(missingSyncBlockTimeˢ);
    }
    if (!hasGCMarkAssistTime) {
        Ꮡt.Error(missingGcMarkAssistTimeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestsGo122ˢ = "testdata/tests/go122-annotations.test"u8;

internal partial struct TestSummarizeGoroutinesRegionsTrace_region /*dyn*/ {
    internal Δtrace.EventKind startKind;
    internal Δtrace.EventKind endKind;
}

public static void TestSummarizeGoroutinesRegionsTrace(ж<testing.T> Ꮡt) {
    var summaries = summarizeTraceTest(Ꮡt, testdataTestsGo122ˢ).Value.Goroutines;
    var wantRegions = new map<@string, TestSummarizeGoroutinesRegionsTrace_region>{ // N.B. "pre-existing region" never even makes it into the trace.
 //
 // TODO(mknyszek): Add test case for end-without-a-start, which can happen at
 // a generation split only.

        [""u8] = new(Δtrace.EventStateTransition, Δtrace.EventStateTransition), // Task inheritance marker.

        ["task0 region"u8] = new(Δtrace.EventRegionBegin, Δtrace.EventBad),
        ["region0"u8] = new(Δtrace.EventRegionBegin, Δtrace.EventRegionEnd),
        ["region1"u8] = new(Δtrace.EventRegionBegin, Δtrace.EventRegionEnd),
        ["unended region"u8] = new(Δtrace.EventRegionBegin, Δtrace.EventStateTransition),
        ["post-existing region"u8] = new(Δtrace.EventRegionBegin, Δtrace.EventBad)
    };
    foreach (var (_, summary) in summaries) {
        basicGoroutineSummaryChecks(Ꮡt, summary);
        foreach (var (_, region) in (~summary).Regions) {
            var (want, ok) = wantRegions[(~region).Name, ꟷ];
            if (!ok) {
                continue;
            }
            checkRegionEvents(Ꮡt, want.startKind, want.endKind, (~summary).ID, region);
            delete(wantRegions, (~region).Name);
        }
    }
    if (len(wantRegions) != 0) {
        Ꮡt.Errorf("failed to find regions: %#v"u8, wantRegions);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestsGo122ˢ2 = "testdata/tests/go122-annotations-stress.test"u8;

internal partial struct TestSummarizeTasksTrace_task /*dyn*/ {
    internal @string name;
    internal ж<Δtrace.TaskID> parent;
    internal slice<Δtrace.TaskID> children;
    internal slice<traceꓸLog> logs;
    internal slice<Δtrace.GoID> goroutines;
}

public static void TestSummarizeTasksTrace(ж<testing.T> Ꮡt) {
    var summaries = summarizeTraceTest(Ꮡt, testdataTestsGo122ˢ2).Value.Tasks;
    ж<Δtrace.TaskID> parent(Δtrace.TaskID id) {
        var p = @new<Δtrace.TaskID>();
        p.Value = id;
        return p;
    }
    var wantTasks = new map<Δtrace.TaskID, TestSummarizeTasksTrace_task>{
        [Δtrace.BackgroundTask] = new(
            logs: new traceꓸLog[]{ // The background task (0) is never any task's parent.

                new(Task: Δtrace.BackgroundTask, Category: "log"u8, Message: "before do"u8),
                new(Task: Δtrace.BackgroundTask, Category: "log"u8, Message: "before do"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{1}.slice()
        ),
        [1] = new(
            children: new Δtrace.TaskID[]{ // This started before tracing started and has no parents.
 // Task 2 is technically a child, but we lost that information.
3, 7, 16}.slice(),
            logs: new traceꓸLog[]{
                new(Task: 1, Category: "log"u8, Message: "before do"u8),
                new(Task: 1, Category: "log"u8, Message: "before do"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{1}.slice()
        ),
        [2] = new(
            children: new Δtrace.TaskID[]{ // This started before tracing started and its parent is technically (1), but that information was lost.
8, 17}.slice(),
            logs: new traceꓸLog[]{
                new(Task: 2, Category: "log"u8, Message: "before do"u8),
                new(Task: 2, Category: "log"u8, Message: "before do"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{1}.slice()
        ),
        [3] = new(
            parent: parent(1),
            children: new Δtrace.TaskID[]{10, 19}.slice(),
            logs: new traceꓸLog[]{
                new(Task: 3, Category: "log"u8, Message: "before do"u8),
                new(Task: 3, Category: "log"u8, Message: "before do"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{1}.slice()
        ),
        [4] = new(
            children: new Δtrace.TaskID[]{ // Explicitly, no parent.
12, 21}.slice(),
            logs: new traceꓸLog[]{
                new(Task: 4, Category: "log"u8, Message: "before do"u8),
                new(Task: 4, Category: "log"u8, Message: "before do"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{1}.slice()
        ),
        [12] = new(
            parent: parent(4),
            children: new Δtrace.TaskID[]{13}.slice(),
            logs: new traceꓸLog[]{ // TODO(mknyszek): This is computed asynchronously in the trace,
 // which makes regenerating this test very annoying, since it will
 // likely break this test. Resolve this by making the order not matter.

                new(Task: 12, Category: "log2"u8, Message: "do"u8),
                new(Task: 12, Category: "log"u8, Message: "fanout region4"u8),
                new(Task: 12, Category: "log"u8, Message: "fanout region0"u8),
                new(Task: 12, Category: "log"u8, Message: "fanout region1"u8),
                new(Task: 12, Category: "log"u8, Message: "fanout region2"u8),
                new(Task: 12, Category: "log"u8, Message: "before do"u8),
                new(Task: 12, Category: "log"u8, Message: "fanout region3"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{1, 5, 6, 7, 8, 9}.slice()
        ),
        [13] = new(
            parent: parent(12), // Explicitly, no children.

            logs: new traceꓸLog[]{
                new(Task: 13, Category: "log2"u8, Message: "do"u8)
            }.slice(),
            goroutines: new Δtrace.GoID[]{7}.slice()
        )
    };
    foreach (var (id, summary) in summaries) {
        var (want, ok) = wantTasks[id, ꟷ];
        if (!ok) {
            continue;
        }
        if (id != (~summary).ID) {
            Ꮡt.Errorf("ambiguous task %d (or %d?): field likely set incorrectly"u8, id, (~summary).ID);
        }
        // Check parent.
        if (want.parent != nil){
            if ((~summary).Parent == nil){
                Ꮡt.Errorf("expected parent %d for task %d without a parent"u8, want.parent.Value, id);
            } else 
            if ((~(~summary).Parent).ID != want.parent.Value) {
                Ꮡt.Errorf("bad parent for task %d: want %d, got %d"u8, id, want.parent.Value, (~(~summary).Parent).ID);
            }
        } else 
        if ((~summary).Parent != nil) {
            Ꮡt.Errorf("unexpected parent %d for task %d"u8, (~(~summary).Parent).ID, id);
        }
        // Check children.
        var gotChildren = new map<Δtrace.TaskID, EmptyStruct>();
        foreach (var (_, child) in (~summary).Children) {
            gotChildren[(~child).ID] = new EmptyStruct();
        }
        foreach (var (_, wantChild) in want.children) {
            {
                var (_, okΔ1) = gotChildren[wantChild, ꟷ]; if (okΔ1){
                    delete(gotChildren, wantChild);
                } else {
                    Ꮡt.Errorf("expected child task %d for task %d not found"u8, wantChild, id);
                }
            }
        }
        if (len(gotChildren) != 0) {
            foreach (var (child, _) in gotChildren) {
                Ꮡt.Errorf("unexpected child task %d for task %d"u8, child, id);
            }
        }
        // Check logs.
        if (len(want.logs) != len((~summary).Logs)){
            Ꮡt.Errorf("wanted %d logs for task %d, got %d logs instead"u8, len(want.logs), id, len((~summary).Logs));
        } else {
            foreach (var (i, _) in want.logs) {
                if (want.logs[i] != (~(~summary).Logs[i]).Log()) {
                    Ꮡt.Errorf("log mismatch: want %#v, got %#v"u8, want.logs[i], (~(~summary).Logs[i]).Log());
                }
            }
        }
        // Check goroutines.
        if (len(want.goroutines) != len((~summary).Goroutines)){
            Ꮡt.Errorf("wanted %d goroutines for task %d, got %d goroutines instead"u8, len(want.goroutines), id, len((~summary).Goroutines));
        } else {
            foreach (var (_, goid) in want.goroutines) {
                var (g, okΔ2) = (~summary).Goroutines[goid, ꟷ];
                if (!okΔ2) {
                    Ꮡt.Errorf("want goroutine %d for task %d, not found"u8, goid, id);
                    continue;
                }
                if ((~g).ID != goid) {
                    Ꮡt.Errorf("goroutine summary for %d does not match task %d listing of %d"u8, (~g).ID, id, goid);
                }
            }
        }
        // Marked as seen.
        delete(wantTasks, id);
    }
    if (len(wantTasks) != 0) {
        Ꮡt.Errorf("failed to find tasks: %#v"u8, wantTasks);
    }
}

internal static void assertContainsGoroutine(ж<testing.T> Ꮡt, map<Δtrace.GoID, ж<Δtrace.GoroutineSummary>> summaries, @string name) {
    foreach (var (_, summary) in summaries) {
        if ((~summary).Name == name) {
            return;
        }
    }
    Ꮡt.Errorf("missing goroutine %s"u8, name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object summaryFoundForNoˢ = (@string)"summary found for no goroutine"u8;

internal static void basicGoroutineSummaryChecks(ж<testing.T> Ꮡt, ж<Δtrace.GoroutineSummary> Ꮡsummary) {
    ref var summary = ref Ꮡsummary.DerefOrNull();

    if (summary.ID == Δtrace.NoGoroutine) {
        Ꮡt.Error(summaryFoundForNoˢ);
        return;
    }
    if ((summary.StartTime != 0 && summary.CreationTime > summary.StartTime) || (summary.StartTime != 0 && summary.EndTime != 0 && summary.StartTime > summary.EndTime)) {
        Ꮡt.Errorf("bad summary creation/start/end times for G %d: creation=%d start=%d end=%d"u8, summary.ID, summary.CreationTime, summary.StartTime, summary.EndTime);
    }
    if ((summary.PC != 0 && summary.Name == ""u8) || (summary.PC == 0 && summary.Name != ""u8)) {
        Ꮡt.Errorf("bad name and/or PC for G %d: pc=0x%x name=%q"u8, summary.ID, summary.PC, summary.Name);
    }
    basicGoroutineExecStatsChecks(Ꮡt, Ꮡsummary.of(Δtrace.GoroutineSummary.ᏑGoroutineExecStats));
    foreach (var (_, region) in summary.Regions) {
        basicGoroutineExecStatsChecks(Ꮡt, region.of(Δtrace.UserRegionSummary.ᏑGoroutineExecStats));
    }
}

internal static ж<Δtrace.Summary> summarizeTraceTest(ж<testing.T> Ꮡt, @string testPath) {
    var (trc, _, err) = testtrace.ParseFile(testPath);
    if (err != default!) {
        Ꮡt.Fatalf("malformed test %s: bad trace file: %v"u8, testPath, err);
    }
    // Create the analysis state.
    var s = Δtrace.NewSummarizer();
    // Create a reader.
    (var r, err) = Δtrace.NewReader(trc);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create trace reader for %s: %v"u8, testPath, err);
    }
    // Process the trace.
    while (ᐧ) {
        ref var ev = ref heap<traceꓸEvent>(out var Ꮡev);
        (ev, var errΔ1) = r.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to process trace %s: %v"u8, testPath, errΔ1);
        }
        s.Event(Ꮡev);
    }
    return s.ΔFinalize();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedNonNilRegionˢ = (@string)"expected non-nil region start event, got nil"u8;
internal static readonly object expectedNonNilRegionEndˢ = (@string)"expected non-nil region end event, got nil"u8;

internal static void checkRegionEvents(ж<testing.T> Ꮡt, Δtrace.EventKind wantStart, Δtrace.EventKind wantEnd, Δtrace.GoID goid, ж<Δtrace.UserRegionSummary> Ꮡregion) {
    ref var region = ref Ꮡregion.DerefOrNull();

    var exprᴛ1 = wantStart;
    if (exprᴛ1 == Δtrace.EventBad) {
        if (region.Start != nil) {
            Ꮡt.Errorf("expected nil region start event, got\n%s"u8, (~region.Start).String());
        }
    }
    else if (exprᴛ1 == Δtrace.EventStateTransition || exprᴛ1 == Δtrace.EventRegionBegin) {
        if (region.Start == nil) {
            Ꮡt.Error(expectedNonNilRegionˢ);
        }
        var kind = (~region.Start).Kind();
        if (kind != wantStart) {
            Ꮡt.Errorf("wanted region start event %s, got %s"u8, wantStart, kind);
        }
        if (kind == Δtrace.EventRegionBegin){
            if ((~region.Start).Region().Type != region.Name) {
                Ꮡt.Errorf("region name mismatch: event has %s, summary has %s"u8, (~region.Start).Region().Type, region.Name);
            }
        } else {
            var st = (~region.Start).StateTransition();
            if (st.Resource.Kind != Δtrace.ResourceGoroutine) {
                Ꮡt.Errorf("found region start event for the wrong resource: %s"u8, st.Resource);
            }
            if (st.Resource.Goroutine() != goid) {
                Ꮡt.Errorf("found region start event for the wrong resource: wanted goroutine %d, got %s"u8, goid, st.Resource);
            }
            {
                var (old, _) = st.Goroutine(); if (old != Δtrace.GoNotExist && old != Δtrace.GoUndetermined) {
                    Ꮡt.Errorf("expected transition from GoNotExist or GoUndetermined, got transition from %s instead"u8, old);
                }
            }
        }
    }
    else { /* default: */
        Ꮡt.Errorf("unexpected want start event type: %s"u8, wantStart);
    }

    var exprᴛ2 = wantEnd;
    if (exprᴛ2 == Δtrace.EventBad) {
        if (region.End != nil) {
            Ꮡt.Errorf("expected nil region end event, got\n%s"u8, (~region.End).String());
        }
    }
    else if (exprᴛ2 == Δtrace.EventStateTransition || exprᴛ2 == Δtrace.EventRegionEnd) {
        if (region.End == nil) {
            Ꮡt.Error(expectedNonNilRegionEndˢ);
        }
        var kind = (~region.End).Kind();
        if (kind != wantEnd) {
            Ꮡt.Errorf("wanted region end event %s, got %s"u8, wantEnd, kind);
        }
        if (kind == Δtrace.EventRegionEnd){
            if ((~region.End).Region().Type != region.Name) {
                Ꮡt.Errorf("region name mismatch: event has %s, summary has %s"u8, (~region.End).Region().Type, region.Name);
            }
        } else {
            var st = (~region.End).StateTransition();
            if (st.Resource.Kind != Δtrace.ResourceGoroutine) {
                Ꮡt.Errorf("found region end event for the wrong resource: %s"u8, st.Resource);
            }
            if (st.Resource.Goroutine() != goid) {
                Ꮡt.Errorf("found region end event for the wrong resource: wanted goroutine %d, got %s"u8, goid, st.Resource);
            }
            {
                var (_, @new) = st.Goroutine(); if (@new != Δtrace.GoNotExist) {
                    Ꮡt.Errorf("expected transition to GoNotExist, got transition to %s instead"u8, @new);
                }
            }
        }
    }
    else { /* default: */
        Ꮡt.Errorf("unexpected want end event type: %s"u8, wantEnd);
    }

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object foundNegativeExecTimeˢ = (@string)"found negative ExecTime"u8;
internal static readonly object foundNegativeˢ = (@string)"found negative SchedWaitTime"u8;
internal static readonly object foundNegativeSyscallTimeˢ = (@string)"found negative SyscallTime"u8;
internal static readonly object foundNegativeˢ2 = (@string)"found negative SyscallBlockTime"u8;
internal static readonly object foundNegativeTotalTimeˢ = (@string)"found negative TotalTime"u8;

internal static void basicGoroutineExecStatsChecks(ж<testing.T> Ꮡt, ж<Δtrace.GoroutineExecStats> Ꮡstats) {
    ref var stats = ref Ꮡstats.DerefOrNull();

    if (stats.ExecTime < 0) {
        Ꮡt.Error(foundNegativeExecTimeˢ);
    }
    if (stats.SchedWaitTime < 0) {
        Ꮡt.Error(foundNegativeˢ);
    }
    if (stats.SyscallTime < 0) {
        Ꮡt.Error(foundNegativeSyscallTimeˢ);
    }
    if (stats.SyscallBlockTime < 0) {
        Ꮡt.Error(foundNegativeˢ2);
    }
    if (stats.TotalTime < 0) {
        Ꮡt.Error(foundNegativeTotalTimeˢ);
    }
    foreach (var (reason, dt) in stats.BlockTimeByReason) {
        if (dt < 0) {
            Ꮡt.Errorf("found negative BlockTimeByReason for %s"u8, reason);
        }
    }
    foreach (var (name, dt) in stats.RangeTime) {
        if (dt < 0) {
            Ꮡt.Errorf("found negative RangeTime for range %s"u8, name);
        }
    }
}

public static void TestRelatedGoroutinesV2Trace(ж<testing.T> Ꮡt) {
    @string testPath = testdataTestsGo122Gcˢ;
    var (trc, _, err) = testtrace.ParseFile(testPath);
    if (err != default!) {
        Ꮡt.Fatalf("malformed test %s: bad trace file: %v"u8, testPath, err);
    }
    // Create a reader.
    (var r, err) = Δtrace.NewReader(trc);
    if (err != default!) {
        Ꮡt.Fatalf("failed to create trace reader for %s: %v"u8, testPath, err);
    }
    // Collect all the events.
    slice<traceꓸEvent> events = default!;
    while (ᐧ) {
        var (ev, errΔ1) = r.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to process trace %s: %v"u8, testPath, errΔ1);
        }
        events = append(events, ev.ΔClone());
    }
    // Test the function.
    var targetg = ((Δtrace.GoID)86);
    var got = Δtrace.RelatedGoroutinesV2(events, targetg);
    var want = new map<Δtrace.GoID, EmptyStruct>{
        [((Δtrace.GoID)86)] = new EmptyStruct(), // N.B. Result includes target.

        [((Δtrace.GoID)71)] = new EmptyStruct(),
        [((Δtrace.GoID)25)] = new EmptyStruct(),
        [((Δtrace.GoID)122)] = new EmptyStruct()
    };
    foreach (var (goid, _) in got) {
        {
            var (_, ok) = want[goid, ꟷ]; if (ok){
                delete(want, goid);
            } else {
                Ꮡt.Errorf("unexpected goroutine %d found in related goroutines for %d in test %s"u8, goid, targetg, testPath);
            }
        }
    }
    if (len(want) != 0) {
        foreach (var (goid, _) in want) {
            Ꮡt.Errorf("failed to find related goroutine %d for goroutine %d in test %s"u8, goid, targetg, testPath);
        }
    }
}

} // end trace_test_package
