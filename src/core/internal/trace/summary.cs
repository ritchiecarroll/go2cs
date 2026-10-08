// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using cmp = cmp_package;
using slices = slices_package;
using strings = strings_package;
using time = time_package;
using iter = iter_package;

partial class trace_package {

// Summary is the analysis result produced by the summarizer.
partial struct Summary {
    public map<GoID, ж<GoroutineSummary>> Goroutines;
    public map<TaskID, ж<UserTaskSummary>> Tasks;
}

// GoroutineSummary contains statistics and execution details of a single goroutine.
// (For v2 traces.)
partial struct GoroutineSummary {
    public GoID ID;
    public @string Name; // A non-unique human-friendly identifier for the goroutine.
    public uint64 PC; // The first PC we saw for the entry function of the goroutine
    public ΔTime CreationTime;   // Timestamp of the first appearance in the trace.
    public ΔTime StartTime;   // Timestamp of the first time it started running. 0 if the goroutine never ran.
    public ΔTime EndTime;   // Timestamp of when the goroutine exited. 0 if the goroutine never exited.
    // List of regions in the goroutine, sorted based on the start time.
    public slice<ж<UserRegionSummary>> Regions;
    // Statistics of execution time during the goroutine execution.
    public partial ref GoroutineExecStats GoroutineExecStats { get; }
    // goroutineSummary is state used just for computing this structure.
    // It's dropped before being returned to the caller.
    //
    // More specifically, if it's nil, it indicates that this summary has
    // already been finalized.
    internal partial ref ж<goroutineSummary> goroutineSummary { get; }
}

// UserTaskSummary represents a task in the trace.
partial struct UserTaskSummary {
    public TaskID ID;
    public @string Name;
    public ж<UserTaskSummary> Parent; // nil if the parent is unknown.
    public slice<ж<UserTaskSummary>> Children;
    // Task begin event. An EventTaskBegin event or nil.
    public ж<ΔEvent> Start;
    // End end event. Normally EventTaskEnd event or nil.
    public ж<ΔEvent> End;
    // Logs is a list of EventLog events associated with the task.
    public slice<ж<ΔEvent>> Logs;
    // List of regions in the task, sorted based on the start time.
    public slice<ж<UserRegionSummary>> Regions;
    // Goroutines is the set of goroutines associated with this task.
    public map<GoID, ж<GoroutineSummary>> Goroutines;
}

// Complete returns true if we have complete information about the task
// from the trace: both a start and an end.
public static bool Complete(this ref UserTaskSummary s) {
    return s.Start != nil && s.End != nil;
}

// Descendents returns a slice consisting of itself (always the first task returned),
// and the transitive closure of all of its children.
public static slice<ж<UserTaskSummary>> Descendents(this ж<UserTaskSummary> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var descendents = new ж<UserTaskSummary>[]{Ꮡs}.slice();
    foreach (var (_, child) in s.Children) {
        descendents = appendꓸꓸꓸ(descendents, child.Descendents());
    }
    return descendents;
}

// UserRegionSummary represents a region and goroutine execution stats
// while the region was active. (For v2 traces.)
partial struct UserRegionSummary {
    public TaskID TaskID;
    public @string Name;
    // Region start event. Normally EventRegionBegin event or nil,
    // but can be a state transition event from NotExist or Undetermined
    // if the region is a synthetic region representing task inheritance
    // from the parent goroutine.
    public ж<ΔEvent> Start;
    // Region end event. Normally EventRegionEnd event or nil,
    // but can be a state transition event to NotExist if the goroutine
    // terminated without explicitly ending the region.
    public ж<ΔEvent> End;
    public partial ref GoroutineExecStats GoroutineExecStats { get; }
}

// GoroutineExecStats contains statistics about a goroutine's execution
// during a period of time.
partial struct GoroutineExecStats {
    // These stats are all non-overlapping.
    public time.Duration ExecTime;
    public time.Duration SchedWaitTime;
    public map<@string, time.Duration> BlockTimeByReason;
    public time.Duration SyscallTime;
    public time.Duration SyscallBlockTime;
    // TotalTime is the duration of the goroutine's presence in the trace.
    // Necessarily overlaps with other stats.
    public time.Duration TotalTime;
    // Total time the goroutine spent in certain ranges; may overlap
    // with other stats.
    public map<@string, time.Duration> RangeTime;
}

public static map<@string, time.Duration> NonOverlappingStats(this GoroutineExecStats s) {
    var stats = new map<@string, time.Duration>{
        ["Execution time"u8] = s.ExecTime,
        ["Sched wait time"u8] = s.SchedWaitTime,
        ["Syscall execution time"u8] = s.SyscallTime,
        ["Block time (syscall)"u8] = s.SyscallBlockTime,
        ["Unknown time"u8] = s.UnknownTime()
    };
    foreach (var (reason, dt) in s.BlockTimeByReason) {
        stats["Block time ("u8 + reason + ")"u8] += dt;
    }
    // N.B. Don't include RangeTime or TotalTime; they overlap with these other
    // stats.
    return stats;
}

// UnknownTime returns whatever isn't accounted for in TotalTime.
public static time.Duration UnknownTime(this GoroutineExecStats s) {
    var sum = s.ExecTime + s.SchedWaitTime + s.SyscallTime + s.SyscallBlockTime;
    foreach (var (_, dt) in s.BlockTimeByReason) {
        sum += dt;
    }
    // N.B. Don't include range time. Ranges overlap with
    // other stats, whereas these stats are non-overlapping.
    if (sum < s.TotalTime) {
        return s.TotalTime - sum;
    }
    return 0;
}

// sub returns the stats v-s.
internal static GoroutineExecStats /*r*/ sub(this GoroutineExecStats s, GoroutineExecStats v) {
    GoroutineExecStats r = default!;

    r = s.clone();
    r.ExecTime -= v.ExecTime;
    r.SchedWaitTime -= v.SchedWaitTime;
    foreach (var (reason, _) in s.BlockTimeByReason) {
        r.BlockTimeByReason[reason] -= v.BlockTimeByReason[reason];
    }
    r.SyscallTime -= v.SyscallTime;
    r.SyscallBlockTime -= v.SyscallBlockTime;
    r.TotalTime -= v.TotalTime;
    foreach (var (name, _) in s.RangeTime) {
        r.RangeTime[name] -= v.RangeTime[name];
    }
    return r;
}

internal static GoroutineExecStats /*r*/ clone(this GoroutineExecStats s) {
    GoroutineExecStats r = default!;

    r = s;
    r.BlockTimeByReason = new map<@string, time.Duration>();
    foreach (var (reason, dt) in s.BlockTimeByReason) {
        r.BlockTimeByReason[reason] = dt;
    }
    r.RangeTime = new map<@string, time.Duration>();
    foreach (var (name, dt) in s.RangeTime) {
        r.RangeTime[name] = dt;
    }
    return r;
}

// snapshotStat returns the snapshot of the goroutine execution statistics.
// This is called as we process the ordered trace event stream. lastTs is used
// to process pending statistics if this is called before any goroutine end event.
internal static GoroutineExecStats /*ret*/ snapshotStat(this ref GoroutineSummary g, ΔTime lastTs) {
    GoroutineExecStats ret = default!;

    ret = g.GoroutineExecStats.clone();
    if (g.goroutineSummary == nil) {
        return ret; // Already finalized; no pending state.
    }
    // Set the total time if necessary.
    if (g.TotalTime == 0) {
        ret.TotalTime = lastTs.Sub(g.CreationTime);
    }
    // Add in time since lastTs.
    if (g.lastStartTime != 0) {
        ret.ExecTime += lastTs.Sub(g.lastStartTime);
    }
    if (g.lastRunnableTime != 0) {
        ret.SchedWaitTime += lastTs.Sub(g.lastRunnableTime);
    }
    if (g.lastBlockTime != 0) {
        ret.BlockTimeByReason[g.lastBlockReason] += lastTs.Sub(g.lastBlockTime);
    }
    if (g.lastSyscallTime != 0) {
        ret.SyscallTime += lastTs.Sub(g.lastSyscallTime);
    }
    if (g.lastSyscallBlockTime != 0) {
        ret.SchedWaitTime += lastTs.Sub(g.lastSyscallBlockTime);
    }
    foreach (var (name, ts) in g.lastRangeTime) {
        ret.RangeTime[name] += lastTs.Sub(ts);
    }
    return ret;
}

// finalize is called when processing a goroutine end event or at
// the end of trace processing. This finalizes the execution stat
// and any active regions in the goroutine, in which case trigger is nil.
internal static void finalize(this ref GoroutineSummary g, ΔTime lastTs, ж<ΔEvent> Ꮡtrigger) {
    ref var trigger = ref Ꮡtrigger.DerefOrNull();

    if (Ꮡtrigger != nil) {
        g.EndTime = trigger.Time();
    }
    var finalStat = g.snapshotStat(lastTs);
    g.GoroutineExecStats = finalStat;
    // System goroutines are never part of regions, even though they
    // "inherit" a task due to creation (EvGoCreate) from within a region.
    // This may happen e.g. if the first GC is triggered within a region,
    // starting the GC worker goroutines.
    if (!IsSystemGoroutine(g.Name)) {
        foreach (var (_, s) in g.activeRegions) {
            s.Value.End = Ꮡtrigger;
            s.Value.GoroutineExecStats = finalStat.sub((~s).GoroutineExecStats);
            g.Regions = append(g.Regions, s);
        }
    }
    (g.goroutineSummary).Value = new goroutineSummary(nil);
}

// goroutineSummary is a private part of GoroutineSummary that is required only during analysis.
partial struct goroutineSummary {
    internal ΔTime lastStartTime;
    internal ΔTime lastRunnableTime;
    internal ΔTime lastBlockTime;
    internal @string lastBlockReason;
    internal ΔTime lastSyscallTime;
    internal ΔTime lastSyscallBlockTime;
    internal map<@string, ΔTime> lastRangeTime;
    internal slice<ж<UserRegionSummary>> activeRegions; // stack of active regions
}

// Summarizer constructs per-goroutine time statistics for v2 traces.
partial struct Summarizer {
    // gs contains the map of goroutine summaries we're building up to return to the caller.
    internal map<GoID, ж<GoroutineSummary>> gs;
    // tasks contains the map of task summaries we're building up to return to the caller.
    internal map<TaskID, ж<UserTaskSummary>> tasks;
    // syscallingP and syscallingG represent a binding between a P and G in a syscall.
    // Used to correctly identify and clean up after syscalls (blocking or otherwise).
    internal map<ProcID, GoID> syscallingP;
    internal map<GoID, ProcID> syscallingG;
    // rangesP is used for optimistic tracking of P-based ranges for goroutines.
    //
    // It's a best-effort mapping of an active range on a P to the goroutine we think
    // is associated with it.
    internal map<rangeP, GoID> rangesP;
    internal ΔTime lastTs; // timestamp of the last event processed.
    internal ΔTime syncTs; // timestamp of the last sync event processed (or the first timestamp in the trace).
}

// NewSummarizer creates a new struct to build goroutine stats from a trace.
public static ж<Summarizer> NewSummarizer() {
    return Ꮡ(new Summarizer(
        gs: new map<GoID, ж<GoroutineSummary>>(),
        tasks: new map<TaskID, ж<UserTaskSummary>>(),
        syscallingP: new map<ProcID, GoID>(),
        syscallingG: new map<GoID, ProcID>(),
        rangesP: new map<rangeP, GoID>()
    ));
}

partial struct rangeP {
    internal ProcID id;
    internal @string name;
}

// Event feeds a single event into the stats summarizer.
public static void Event(this ref Summarizer s, ж<ΔEvent> Ꮡev) {
    ref var ev = ref Ꮡev.DerefOrNull();

    if (s.syncTs == 0) {
        s.syncTs = ev.Time();
    }
    s.lastTs = ev.Time();
    var exprᴛ1 = ev.Kind();
    if (exprᴛ1 == EventSync) {
        s.syncTs = ev.Time();
    }
    else if (exprᴛ1 == EventStateTransition) {
        var st = ev.StateTransition();
        var exprᴛ2 = st.Resource.Kind;
        if (exprᴛ2 == ResourceGoroutine) {
            do {
                ref var id = ref heap<GoID>(out var Ꮡid);
                id = st.Resource.Goroutine();
                var (old, @new) = st.Goroutine();
                if (old == @new) {
                    // Record sync time for the RangeActive events.
                    // Handle state transitions.
                    // Handle goroutine transitions, which are the meat of this computation.
                    // Skip these events; they're not telling us anything new.
                    break;
                }
                var g = s.gs[id];
                var exprᴛ3 = old;
                if (exprᴛ3 == GoUndetermined || exprᴛ3 == GoNotExist) {
                    g = Ꮡ(new GoroutineSummary( // Handle transition out.
ID: id, goroutineSummary: Ꮡ(new goroutineSummary(nil))));
                    if (old == GoUndetermined){
                        // If we're coming out of GoUndetermined, then the creation time is the
                        // time of the last sync.
                        g.Value.CreationTime = s.syncTs;
                    } else {
                        g.Value.CreationTime = ev.Time();
                    }
                    g.Value.lastRangeTime = new map<@string, ΔTime>();
                    g.Value.BlockTimeByReason = new map<@string, time.Duration>();
                    g.Value.RangeTime = new map<@string, time.Duration>();
                    {
                        var creatorG = s.gs[ev.Goroutine()]; if (creatorG != nil && len((~creatorG).activeRegions) > 0) {
                            // The goroutine is being created, or it's being named for the first time.
                            // When a goroutine is newly created, inherit the task
                            // of the active region. For ease handling of this
                            // case, we create a fake region description with the
                            // task id. This isn't strictly necessary as this
                            // goroutine may not be associated with the task, but
                            // it can be convenient to see all children created
                            // during a region.
                            //
                            // N.B. ev.Goroutine() will always be NoGoroutine for the
                            // Undetermined case, so this is will simply not fire.
                            var regions = creatorG.Value.activeRegions;
                            var sΔ8 = regions[len(regions) - 1];
                            g.Value.activeRegions = new ж<UserRegionSummary>[]{Ꮡ(new UserRegionSummary(TaskID: (~sΔ8).TaskID, Start: Ꮡev))}.slice();
                        }
                    }
                    s.gs[(~g).ID] = g;
                }
                else if (exprᴛ3 == GoRunning) {
                    g.Value.ExecTime += ev.Time().Sub((~g).lastStartTime);
                    g.Value.lastStartTime = 0;
                }
                else if (exprᴛ3 == GoWaiting) {
                    if ((~g).lastBlockTime != 0) {
                        // Record execution time as we transition out of running
                        // Record block time as we transition out of waiting.
                        g.Value.BlockTimeByReason[(~g).lastBlockReason] += ev.Time().Sub((~g).lastBlockTime);
                        g.Value.lastBlockTime = 0;
                    }
                }
                else if (exprᴛ3 == GoRunnable) {
                    if ((~g).lastRunnableTime != 0) {
                        // Record sched latency time as we transition out of runnable.
                        g.Value.SchedWaitTime += ev.Time().Sub((~g).lastRunnableTime);
                        g.Value.lastRunnableTime = 0;
                    }
                }
                else if (exprᴛ3 == GoSyscall) {
                    if ((~g).lastSyscallTime != 0) {
                        // Record syscall execution time and syscall block time as we transition out of syscall.
                        if ((~g).lastSyscallBlockTime != 0){
                            g.Value.SyscallBlockTime += ev.Time().Sub((~g).lastSyscallBlockTime);
                            g.Value.SyscallTime += (~g).lastSyscallBlockTime.Sub((~g).lastSyscallTime);
                        } else {
                            g.Value.SyscallTime += ev.Time().Sub((~g).lastSyscallTime);
                        }
                        g.Value.lastSyscallTime = 0;
                        g.Value.lastSyscallBlockTime = 0;
                        // Clear the syscall map.
                        delete(s.syscallingP, s.syscallingG[id]);
                        delete(s.syscallingG, id);
                    }
                }

                if ((~g).Name == ""u8) {
                    // The goroutine hasn't been identified yet. Take the transition stack
                    // and identify the goroutine by the root frame of that stack.
                    // This root frame will be identical for all transitions on this
                    // goroutine, because it represents its immutable start point.
                    foreach (var frame in range<StackFrame>(st.Stack.Frames().Invoke)) {
                        // NB: this PC won't actually be consistent for
                        // goroutines which existed at the start of the
                        // trace. The UI doesn't use it directly; this
                        // mainly serves as an indication that we
                        // actually saw a call stack for the goroutine
                        g.Value.PC = frame.PC;
                        g.Value.Name = frame.Func;
                    }
                }
                var exprᴛ4 = @new;
                var matchᴛ1 = false;
                if (exprᴛ4 == GoRunning) { matchᴛ1 = true;
                    g.Value.lastStartTime = ev.Time();
                    if ((~g).StartTime == 0) {
                        // Handle transition in.
                        // We started running. Record it.
                        g.Value.StartTime = ev.Time();
                    }
                }
                else if (exprᴛ4 == GoRunnable) { matchᴛ1 = true;
                    g.Value.lastRunnableTime = ev.Time();
                }
                else if (exprᴛ4 == GoWaiting) { matchᴛ1 = true;
                    do {
                        if (st.Reason != "forever"u8) {
                            g.Value.lastBlockTime = ev.Time();
                            g.Value.lastBlockReason = st.Reason;
                            break;
                        }
                        fallthrough = true;
                    } while (false);
                }
                if (fallthrough || !matchᴛ1 && exprᴛ4 == GoNotExist) {
                    g.finalize(ev.Time(), // "Forever" is like goroutine death.
 Ꮡev);
                }
                else if (exprᴛ4 == GoSyscall) { matchᴛ1 = true;
                    s.syscallingP[ev.Proc()] = id;
                    s.syscallingG[id] = ev.Proc();
                    g.Value.lastSyscallTime = ev.Time();
                }

            } while (false);
        }
        else if (exprᴛ2 == ResourceProc) {
            var id = st.Resource.Proc();
            var (old, @new) = st.Proc();
            if (old != @new && @new == ProcIdle) {
                // Handle procs to detect syscall blocking, which si identifiable as a
                // proc going idle while the goroutine it was attached to is in a syscall.
                {
                    var (goid, ok) = s.syscallingP[id, ꟷ]; if (ok) {
                        var g = s.gs[goid];
                        g.Value.lastSyscallBlockTime = ev.Time();
                        delete(s.syscallingP, id);
                    }
                }
            }
        }

    }
    else if (exprᴛ1 == EventRangeBegin || exprᴛ1 == EventRangeActive) {
        do {
            var r = ev.Range();
// Handle ranges of all kinds.
            ж<GoroutineSummary> g = default!;
            var exprᴛ5 = r.Scope.Kind;
            if (exprᴛ5 == ResourceGoroutine) {
                g = s.gs[r.Scope.Goroutine()];
            }
            else if (exprᴛ5 == ResourceProc) {
                g = s.gs[ev.Goroutine()];
                if (g != nil) {
                    // Simple goroutine range. We attribute the entire range regardless of
                    // goroutine stats. Lots of situations are still identifiable, e.g. a
                    // goroutine blocked often in mark assist will have both high mark assist
                    // and high block times. Those interested in a deeper view can look at the
                    // trace viewer.
                    // N.B. These ranges are not actually bound to the goroutine, they're
                    // bound to the P. But if we happen to be on the P the whole time, let's
                    // try to attribute it to the goroutine. (e.g. GC sweeps are here.)
                    s.rangesP[new rangeP(id: r.Scope.Proc(), name: r.Name)] = ev.Goroutine();
                }
            }

            if (g == nil) {
                break;
            }
            if (ev.Kind() == EventRangeActive){
                {
                    var ts = (~g).lastRangeTime[r.Name]; if (ts != 0) {
                        g.Value.RangeTime[r.Name] += s.syncTs.Sub(ts);
                    }
                }
                g.Value.lastRangeTime[r.Name] = s.syncTs;
            } else {
                g.Value.lastRangeTime[r.Name] = ev.Time();
            }
        } while (false);
    }
    else if (exprᴛ1 == EventRangeEnd) {
        do {
            var r = ev.Range();
            ж<GoroutineSummary> g = default!;
            var exprᴛ6 = r.Scope.Kind;
            if (exprᴛ6 == ResourceGoroutine) {
                g = s.gs[r.Scope.Goroutine()];
            }
            else if (exprᴛ6 == ResourceProc) {
                var rp = new rangeP(id: r.Scope.Proc(), name: r.Name);
                {
                    var (goid, ok) = s.rangesP[rp, ꟷ]; if (ok) {
                        if (goid == ev.Goroutine()) {
                            // As the comment in the RangeBegin case states, this is only OK
                            // if we finish on the same goroutine we started on.
                            g = s.gs[goid];
                        }
                        delete(s.rangesP, rp);
                    }
                }
            }

            if (g == nil) {
                break;
            }
            var ts = (~g).lastRangeTime[r.Name];
            if (ts == 0) {
                break;
            }
            g.Value.RangeTime[r.Name] += ev.Time().Sub(ts);
            delete((~g).lastRangeTime, r.Name);
        } while (false);
    }
    else if (exprᴛ1 == EventRegionBegin) {
        var g = s.gs[ev.Goroutine()];
        var r = ev.Region();
        var region = Ꮡ(new UserRegionSummary( // Handle user-defined regions.

            Name: r.Type,
            TaskID: r.Task,
            Start: Ꮡev,
            GoroutineExecStats: g.snapshotStat(ev.Time())
        ));
        g.Value.activeRegions = append((~g).activeRegions, region);
        var task = s.getOrAddTask(r.Task);
        task.Value.Regions = append((~task).Regions, // Associate the region and current goroutine to the task.
 region);
        task.Value.Goroutines[(~g).ID] = g;
    }
    else if (exprᴛ1 == EventRegionEnd) {
        var g = s.gs[ev.Goroutine()];
        var r = ev.Region();
        ж<UserRegionSummary> sd = default!;
        {
            var regionStk = g.Value.activeRegions; if (len(regionStk) > 0){
                // Pop the top region from the stack since that's what must have ended.
                nint n = len(regionStk);
                sd = regionStk[n - 1];
                regionStk = regionStk.slice(0, n - 1);
                g.Value.activeRegions = regionStk;
            } else {
                // N.B. No need to add the region to a task; the EventRegionBegin already handled it.
                // This is an "end" without a start. Just fabricate the region now.
                sd = Ꮡ(new UserRegionSummary(Name: r.Type, TaskID: r.Task));
                // Associate the region and current goroutine to the task.
                var task = s.getOrAddTask(r.Task);
                task.Value.Goroutines[(~g).ID] = g;
                task.Value.Regions = append((~task).Regions, sd);
            }
        }
        sd.Value.GoroutineExecStats = g.snapshotStat(ev.Time()).sub((~sd).GoroutineExecStats);
        sd.Value.End = Ꮡev;
        g.Value.Regions = append((~g).Regions, sd);
    }
    else if (exprᴛ1 == EventTaskBegin || exprᴛ1 == EventTaskEnd) {
        var t = ev.Task();
        var task = s.getOrAddTask(t.ID);
        task.Value.Name = t.Type;
        task.Value.Goroutines[ev.Goroutine()] = s.gs[ev.Goroutine()];
        if (ev.Kind() == EventTaskBegin){
            // Handle tasks and logs.
            // Initialize the task.
            task.Value.Start = Ꮡev;
        } else {
            task.Value.End = Ꮡev;
        }
        if (t.Parent != NoTask && (~task).Parent == nil) {
            // Initialize the parent, if one exists and it hasn't been done yet.
            // We need to avoid doing it twice, otherwise we could appear twice
            // in the parent's Children list.
            var parent = s.getOrAddTask(t.Parent);
            task.Value.Parent = parent;
            parent.Value.Children = append((~parent).Children, task);
        }
    }
    else if (exprᴛ1 == EventLog) {
        var log = ev.Log();
        var task = s.getOrAddTask(log.Task);
        task.Value.Goroutines[ev.Goroutine()] = s.gs[ev.Goroutine()];
        task.Value.Logs = append((~task).Logs, // Just add the log to the task. We'll create the task if it
 // doesn't exist (it's just been mentioned now).
 Ꮡev);
    }

}

internal static ж<UserTaskSummary> getOrAddTask(this ref Summarizer s, TaskID id) {
    var task = s.tasks[id];
    if (task == nil) {
        task = Ꮡ(new UserTaskSummary(ID: id, Goroutines: new map<GoID, ж<GoroutineSummary>>()));
        s.tasks[id] = task;
    }
    return task;
}

// Finalize indicates to the summarizer that we're done processing the trace.
// It cleans up any remaining state and returns the full summary.
public static ж<Summary> ΔFinalize(this ref Summarizer s) {
    foreach (var (_, g) in s.gs) {
        g.finalize(s.lastTs, nil);
        // Sort based on region start time.
        slices.SortFunc((~g).Regions, (ж<UserRegionSummary> a, ж<UserRegionSummary> b) => {
            var x = a.Value.Start;
            var y = b.Value.Start;
            if (x == nil) {
                if (y == nil) {
                    return 0;
                }
                return -1;
            }
            if (y == nil) {
                return +1;
            }
            return cmp.Compare((~x).Time(), (~y).Time());
        });
        g.Value.goroutineSummary = default!;
    }
    return Ꮡ(new Summary(
        Goroutines: s.gs,
        Tasks: s.tasks
    ));
}

// Process all the events, looking for transitions of goroutines
// out of GoWaiting. If there was an active goroutine when this
// happened, then we know that active goroutine unblocked another.
// Scribble all these down so we can process them.
internal partial struct RelatedGoroutinesV2_unblockEdge /*dyn*/ {
    internal GoID @operator;
    internal GoID operand;
}

// RelatedGoroutinesV2 finds a set of goroutines related to goroutine goid for v2 traces.
// The association is based on whether they have synchronized with each other in the Go
// scheduler (one has unblocked another).
public static map<GoID, EmptyStruct> RelatedGoroutinesV2(slice<ΔEvent> events, GoID goid) {
    slice<RelatedGoroutinesV2_unblockEdge> unblockEdges = default!;
    foreach (var (_, vᴛ1) in events) {
        var ev = vᴛ1.ΔClone();

        if (ev.Goroutine() == NoGoroutine) {
            continue;
        }
        if (ev.Kind() != EventStateTransition) {
            continue;
        }
        var st = ev.StateTransition();
        if (st.Resource.Kind != ResourceGoroutine) {
            continue;
        }
        var id = st.Resource.Goroutine();
        var (old, @new) = st.Goroutine();
        if (old == @new || old != GoWaiting) {
            continue;
        }
        unblockEdges = append(unblockEdges, new RelatedGoroutinesV2_unblockEdge(
            @operator: ev.Goroutine(),
            operand: id
        ));
    }
    // Compute the transitive closure of depth 2 of goroutines that have unblocked each other
    // (starting from goid).
    var gmap = new map<GoID, EmptyStruct>();
    gmap[goid] = new EmptyStruct();
    for (nint i = 0; i < 2; i++) {
        // Copy the map.
        var gmap1 = new map<GoID, EmptyStruct>();
        foreach (var (g, _) in gmap) {
            gmap1[g] = new EmptyStruct();
        }
        foreach (var (_, edge) in unblockEdges) {
            {
                var (_, ok) = gmap[edge.operand, ꟷ]; if (ok) {
                    gmap1[edge.@operator] = new EmptyStruct();
                }
            }
        }
        gmap = gmap1;
    }
    return gmap;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeˢ = "runtime."u8;

public static bool IsSystemGoroutine(@string entryFn) {
    // This mimics runtime.isSystemGoroutine as closely as
    // possible.
    // Also, locked g in extra M (with empty entryFn) is system goroutine.
    return entryFn == ""u8 || entryFn != "runtime.main"u8 && strings.HasPrefix(entryFn, runtimeˢ);
}

} // end trace_package
