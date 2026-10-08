// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements conversion from old (Go 1.11–Go 1.21) traces to the Go
// 1.22 format.
//
// Most events have direct equivalents in 1.22, at worst requiring arguments to
// be reordered. Some events, such as GoWaiting need to look ahead for follow-up
// events to determine the correct translation. GoSyscall, which is an
// instantaneous event, gets turned into a 1 ns long pair of
// GoSyscallStart+GoSyscallEnd, unless we observe a GoSysBlock, in which case we
// emit a GoSyscallStart+GoSyscallEndBlocked pair with the correct duration
// (i.e. starting at the original GoSyscall).
//
// The resulting trace treats the old trace as a single, large generation,
// sharing a single evTable for all events.
//
// We use a new (compared to what was used for 'go tool trace' in earlier
// versions of Go) parser for old traces that is optimized for speed, low memory
// usage, and minimal GC pressure. It allocates events in batches so that even
// though we have to load the entire trace into memory, the conversion process
// shouldn't result in a doubling of memory usage, even if all converted events
// are kept alive, as we free batches once we're done with them.
//
// The conversion process is lossless.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using oldtrace = go.@internal.trace.@internal.oldtrace_package;
using io = io_package;
using go.@internal.trace;
using go.@internal.trace.@event;
using go.@internal.trace.@internal;

partial class trace_package {

partial struct oldTraceConverter {
    internal oldtrace.Trace trace;
    internal ж<evTable> evt;
    internal bool preInit;
    internal map<GoID, EmptyStruct> createdPreInit;
    internal oldtrace.Events events;
    internal slice<ΔEvent> extra;
    internal array<ΔEvent> extraArr = new(3, () => new());
    internal map<TaskID, taskState> tasks;
    internal map<ProcID, EmptyStruct> seenProcs;
    internal ΔTime lastTs;
    internal map<ProcID, ThreadID> procMs;
    internal uint64 lastStwReason;
    internal slice<uint64> inlineToStringID;
    internal slice<uint64> builtinToStringID;
}

internal static UntypedInt sForever => iota;
internal static UntypedInt sPreempted => 1;
internal static UntypedInt sGosched => 2;
internal static UntypedInt sSleep => 3;
internal static UntypedInt sChanSend => 4;
internal static UntypedInt sChanRecv => 5;
internal static UntypedInt sNetwork => 6;
internal static UntypedInt sSync => 7;
internal static UntypedInt sSyncCond => 8;
internal static UntypedInt sSelect => 9;
internal static UntypedInt sEmpty => 10;
internal static UntypedInt sMarkAssistWait => 11;
internal static UntypedInt sSTWUnknown => 12;
internal static UntypedInt sSTWGCMarkTermination => 13;
internal static UntypedInt sSTWGCSweepTermination => 14;
internal static UntypedInt sSTWWriteHeapDump => 15;
internal static UntypedInt sSTWGoroutineProfile => 16;
internal static UntypedInt sSTWGoroutineProfileCleanup => 17;
internal static UntypedInt sSTWAllGoroutinesStackTrace => 18;
internal static UntypedInt sSTWReadMemStats => 19;
internal static UntypedInt sSTWAllThreadsSyscall => 20;
internal static UntypedInt sSTWGOMAXPROCS => 21;
internal static UntypedInt sSTWStartTrace => 22;
internal static UntypedInt sSTWStopTrace => 23;
internal static UntypedInt sSTWCountPagesInUse => 24;
internal static UntypedInt sSTWReadMetricsSlow => 25;
internal static UntypedInt sSTWReadMemStatsSlow => 26;
internal static UntypedInt sSTWPageCachePagesLeaked => 27;
internal static UntypedInt sSTWResetDebugLog => 28;
internal static UntypedInt sLast => 29;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string traceContainsTooManyˢ = "trace contains too many strings"u8;
internal static readonly @string foreverˢ = "forever"u8;
internal static readonly @string preemptedˢ = "preempted"u8;
internal static readonly @string runtimeGoschedˢ = "runtime.Gosched"u8;
internal static readonly @string sleepˢ = "sleep"u8;
internal static readonly @string chanSendˢ = "chan send"u8;
internal static readonly @string chanReceiveˢ = "chan receive"u8;
internal static readonly @string networkˢ = "network"u8;
internal static readonly @string syncˢ = "sync"u8;
internal static readonly @string syncCondWaitˢ = "sync.(*Cond).Wait"u8;
internal static readonly @string selectˢ = "select"u8;
internal static readonly @string gcMarkAssistWaitForWorkˢ = "GC mark assist wait for work"u8;
internal static readonly @string gcMarkTerminationˢ = "GC mark termination"u8;
internal static readonly @string gcSweepTerminationˢ = "GC sweep termination"u8;
internal static readonly @string writeHeapDumpˢ = "write heap dump"u8;
internal static readonly @string goroutineProfileˢ = "goroutine profile"u8;
internal static readonly @string goroutineProfileCleanupˢ = "goroutine profile cleanup"u8;
internal static readonly @string allGoroutineStackTraceˢ = "all goroutine stack trace"u8;
internal static readonly @string readMemStatsˢ = "read mem stats"u8;
internal static readonly @string allThreadsSyscallˢ = "AllThreadsSyscall"u8;
internal static readonly @string gomaxprocsˢ = "GOMAXPROCS"u8;
internal static readonly @string startTraceˢ = "start trace"u8;
internal static readonly @string stopTraceˢ = "stop trace"u8;
internal static readonly @string countPagesInUseTestˢ = "CountPagesInUse (test)"u8;
internal static readonly @string readMetricsSlowTestˢ = "ReadMetricsSlow (test)"u8;
internal static readonly @string readMemStatsSlowTestˢ = "ReadMemStatsSlow (test)"u8;
internal static readonly @string pageCachePagesLeakedTestˢ = "PageCachePagesLeaked (test)"u8;
internal static readonly @string resetDebugLogTestˢ = "ResetDebugLog (test)"u8;

internal static error init(this ж<oldTraceConverter> Ꮡit, oldtrace.Trace pr) {
    ref var it = ref Ꮡit.DerefOrNull();

    it.trace = pr;
    it.preInit = true;
    it.createdPreInit = new map<GoID, EmptyStruct>();
    it.evt = Ꮡ(new evTable(pcs: new map<uint64, frame>()));
    it.events = pr.Events;
    it.extra = it.extraArr[..0];
    it.tasks = new map<TaskID, taskState>();
    it.seenProcs = new map<ProcID, EmptyStruct>();
    it.procMs = new map<ProcID, ThreadID>();
    it.lastTs = -1;
    var evt = it.evt;
    // Convert from oldtracer's Strings map to our dataTable.
    uint64 max = default!;
    foreach (var (id, s) in pr.Strings) {
        evt.of(evTable.Ꮡstrings).insert(((stringID)id), s);
        if (id > max) {
            max = id;
        }
    }
    pr.Strings = default!;
    // Add all strings used for UserLog. In the old trace format, these were
    // stored inline and didn't have IDs. We generate IDs for them.
    if (max + (uint64)len(pr.InlineStrings) < max) {
        return errors.New(traceContainsTooManyˢ);
    }
    ref var addErr = ref heap<error>(out var ᏑaddErr);
    var evtʗ1 = evt;
    void add(stringID id, @string s) {
        {
            var err = evtʗ1.of(evTable.Ꮡstrings).insert(id, s); if (err != default! && ᏑaddErr.ValueSlot == default!) {
                ᏑaddErr.ValueSlot = err;
            }
        }
    }
    foreach (var (id, s) in pr.InlineStrings) {
        var nid = max + 1 + (uint64)id;
        it.inlineToStringID = append(it.inlineToStringID, nid);
        add(((stringID)nid), s);
    }
    max += (uint64)len(pr.InlineStrings);
    pr.InlineStrings = default!;
    // Add strings that the converter emits explicitly.
    if (max + (uint64)sLast < max) {
        return errors.New(traceContainsTooManyˢ);
    }
    it.builtinToStringID = new slice<uint64>(sLast);
    var addʗ1 = add;
    void addBuiltin(nint c, @string s) {
        var nid = max + 1 + (uint64)c;
        Ꮡit.Value.builtinToStringID[c] = nid;
        addʗ1(((stringID)nid), s);
    }
    addBuiltin(sForever, foreverˢ);
    addBuiltin(sPreempted, preemptedˢ);
    addBuiltin(sGosched, runtimeGoschedˢ);
    addBuiltin(sSleep, sleepˢ);
    addBuiltin(sChanSend, chanSendˢ);
    addBuiltin(sChanRecv, chanReceiveˢ);
    addBuiltin(sNetwork, networkˢ);
    addBuiltin(sSync, syncˢ);
    addBuiltin(sSyncCond, syncCondWaitˢ);
    addBuiltin(sSelect, selectˢ);
    addBuiltin(sEmpty, ""u8);
    addBuiltin(sMarkAssistWait, gcMarkAssistWaitForWorkˢ);
    addBuiltin(sSTWUnknown, ""u8);
    addBuiltin(sSTWGCMarkTermination, gcMarkTerminationˢ);
    addBuiltin(sSTWGCSweepTermination, gcSweepTerminationˢ);
    addBuiltin(sSTWWriteHeapDump, writeHeapDumpˢ);
    addBuiltin(sSTWGoroutineProfile, goroutineProfileˢ);
    addBuiltin(sSTWGoroutineProfileCleanup, goroutineProfileCleanupˢ);
    addBuiltin(sSTWAllGoroutinesStackTrace, allGoroutineStackTraceˢ);
    addBuiltin(sSTWReadMemStats, readMemStatsˢ);
    addBuiltin(sSTWAllThreadsSyscall, allThreadsSyscallˢ);
    addBuiltin(sSTWGOMAXPROCS, gomaxprocsˢ);
    addBuiltin(sSTWStartTrace, startTraceˢ);
    addBuiltin(sSTWStopTrace, stopTraceˢ);
    addBuiltin(sSTWCountPagesInUse, countPagesInUseTestˢ);
    addBuiltin(sSTWReadMetricsSlow, readMetricsSlowTestˢ);
    addBuiltin(sSTWReadMemStatsSlow, readMemStatsSlowTestˢ);
    addBuiltin(sSTWPageCachePagesLeaked, pageCachePagesLeakedTestˢ);
    addBuiltin(sSTWResetDebugLog, resetDebugLogTestˢ);
    if (addErr != default!) {
        // This should be impossible but let's be safe.
        return fmt.Errorf("couldn't add strings: %w"u8, addErr);
    }
    it.evt.of(evTable.Ꮡstrings).compactify();
    // Convert stacks.
    foreach (var (id, stk) in pr.Stacks) {
        evt.of(evTable.Ꮡstacks).insert(((stackID)(uint64)id), new stack(pcs: stk));
    }
    // OPT(dh): if we could share the frame type between this package and
    // oldtrace we wouldn't have to copy the map.
    foreach (var (pc, f) in pr.PCs) {
        evt.Value.pcs[pc] = new frame(
            pc: pc,
            funcID: ((stringID)f.Fn),
            fileID: ((stringID)f.File),
            line: (uint64)f.Line
        );
    }
    pr.Stacks = default!;
    pr.PCs = default!;
    evt.of(evTable.Ꮡstacks).compactify();
    return default!;
}

// next returns the next event, io.EOF if there are no more events, or a
// descriptive error for invalid events.
internal static (ΔEvent, error) next(this ж<oldTraceConverter> Ꮡit) {
    ref var it = ref Ꮡit.DerefOrNull();

    if (len(it.extra) > 0) {
        var evΔ1 = it.extra[0].ΔClone();
        it.extra = it.extra[1..];
        if (len(it.extra) == 0) {
            it.extra = it.extraArr[..0];
        }
        // Two events aren't allowed to fall on the same timestamp in the new API,
        // but this may happen when we produce EvGoStatus events
        if (evΔ1.@base.time <= it.lastTs) {
            evΔ1.@base.time = it.lastTs + 1;
        }
        it.lastTs = evΔ1.@base.time;
        return (evΔ1.ΔClone(), default!);
    }
    var (oev, ok) = it.events.Pop();
    if (!ok) {
        return (new ΔEvent(nil), io.EOF);
    }
    var (ev, err) = Ꮡit.convertEvent(oev);
    if (AreEqual(err, errSkip)){
        return Ꮡit.next();
    } else 
    if (err != default!) {
        return (new ΔEvent(nil), err);
    }
    // Two events aren't allowed to fall on the same timestamp in the new API,
    // but this may happen when we produce EvGoStatus events
    if (ev.@base.time <= it.lastTs) {
        ev.@base.time = it.lastTs + 1;
    }
    it.lastTs = ev.@base.time;
    return (ev.ΔClone(), default!);
}

internal static error errSkip = errors.New("skip event"u8);

// convertEvent converts an event from the old trace format to zero or more
// events in the new format. Most events translate 1 to 1. Some events don't
// result in an event right away, in which case convertEvent returns errSkip.
// Some events result in more than one new event; in this case, convertEvent
// returns the first event and stores additional events in it.extra. When
// encountering events that oldtrace shouldn't be able to emit, ocnvertEvent
// returns a descriptive error.
internal static (ΔEvent OUT, error ERR) convertEvent(this ж<oldTraceConverter> Ꮡit, ж<oldtrace.Event> Ꮡev) {
    ref var it = ref Ꮡit.DerefOrNull();
    ref var ev = ref Ꮡev.DerefOrNull();

    @event.Type mappedType = default!;
    timedEventArgs mappedArgs = default!;
    copy(mappedArgs[..], ev.Args[..]);
    var exprᴛ1 = ev.Type;
    if (exprᴛ1 == oldtrace.EvGomaxprocs) {
        mappedType = go122.EvProcsChange;
        if (it.preInit) {
            // The first EvGomaxprocs signals the end of trace initialization. At this point we've seen
            // all goroutines that already existed at trace begin.
            it.preInit = false;
            foreach (var (gid, _) in it.createdPreInit) {
                // These are goroutines that already existed when tracing started but for which we
                // received neither GoWaiting, GoInSyscall, or GoStart. These are goroutines that are in
                // the states _Gidle or _Grunnable.
                it.extra = append(it.extra, new ΔEvent(
                    ctx: new schedCtx( // G: GoID(gid),

                        G: NoGoroutine,
                        P: NoProc,
                        M: NoThread
                    ),
                    table: it.evt,
                    @base: new baseEvent(
                        typ: go122.EvGoStatus,
                        time: ((ΔTime)(int64)ev.Ts),
                        args: new timedEventArgs(new uint64[]{(uint64)(int64)gid, ~(uint64)0, (uint64)(uint8)go122.GoRunnable}.array(4))
                    )
                ));
            }
            it.createdPreInit = default!;
            return (new ΔEvent(nil), errSkip);
        }
    }
    else if (exprᴛ1 == oldtrace.EvProcStart) {
        it.procMs[((ProcID)(int64)ev.P)] = ((ThreadID)(int64)ev.Args[0]);
        {
            var (_, ok) = it.seenProcs[((ProcID)(int64)ev.P), ꟷ]; if (ok){
                mappedType = go122.EvProcStart;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P}.array(4));
            } else {
                it.seenProcs[((ProcID)(int64)ev.P)] = new EmptyStruct();
                mappedType = go122.EvProcStatus;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P, (uint64)(uint8)go122.ProcRunning}.array(4));
            }
        }
    }
    else if (exprᴛ1 == oldtrace.EvProcStop) {
        {
            var (_, ok) = it.seenProcs[((ProcID)(int64)ev.P), ꟷ]; if (ok){
                mappedType = go122.EvProcStop;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P}.array(4));
            } else {
                it.seenProcs[((ProcID)(int64)ev.P)] = new EmptyStruct();
                mappedType = go122.EvProcStatus;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P, (uint64)(uint8)go122.ProcIdle}.array(4));
            }
        }
    }
    else if (exprᴛ1 == oldtrace.EvGCStart) {
        mappedType = go122.EvGCBegin;
    }
    else if (exprᴛ1 == oldtrace.EvGCDone) {
        mappedType = go122.EvGCEnd;
    }
    else if (exprᴛ1 == oldtrace.EvSTWStart) {
        var sid = it.builtinToStringID[(nint)sSTWUnknown + it.trace.STWReason(ev.Args[0])];
        it.lastStwReason = sid;
        mappedType = go122.EvSTWBegin;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)sid}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvSTWDone) {
        mappedType = go122.EvSTWEnd;
        mappedArgs = new timedEventArgs(new uint64[]{it.lastStwReason}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGCSweepStart) {
        mappedType = go122.EvGCSweepBegin;
    }
    else if (exprᴛ1 == oldtrace.EvGCSweepDone) {
        mappedType = go122.EvGCSweepEnd;
    }
    else if (exprᴛ1 == oldtrace.EvGoCreate) {
        if (it.preInit) {
            it.createdPreInit[((GoID)(int64)ev.Args[0])] = new EmptyStruct();
            return (new ΔEvent(nil), errSkip);
        }
        mappedType = go122.EvGoCreate;
    }
    else if (exprᴛ1 == oldtrace.EvGoStart) {
        if (it.preInit){
            mappedType = go122.EvGoStatus;
            mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ~(uint64)0, (uint64)(uint8)go122.GoRunning}.array(4));
            delete(it.createdPreInit, ((GoID)(int64)ev.Args[0]));
        } else {
            mappedType = go122.EvGoStart;
        }
    }
    else if (exprᴛ1 == oldtrace.EvGoStartLabel) {
        it.extra = new ΔEvent[]{new(
            ctx: new schedCtx(
                G: ((GoID)(int64)ev.G),
                P: ((ProcID)(int64)ev.P),
                M: it.procMs[((ProcID)(int64)ev.P)]
            ),
            table: it.evt,
            @base: new baseEvent(
                typ: go122.EvGoLabel,
                time: ((ΔTime)(int64)ev.Ts),
                args: new timedEventArgs(new uint64[]{ev.Args[2]}.array(4))
            )
        )
        }.slice();
        return (new ΔEvent(
            ctx: new schedCtx(
                G: ((GoID)(int64)ev.G),
                P: ((ProcID)(int64)ev.P),
                M: it.procMs[((ProcID)(int64)ev.P)]
            ),
            table: it.evt,
            @base: new baseEvent(
                typ: go122.EvGoStart,
                time: ((ΔTime)(int64)ev.Ts),
                args: mappedArgs.Clone()
            )
        ), default!);
    }
    else if (exprᴛ1 == oldtrace.EvGoEnd) {
        mappedType = go122.EvGoDestroy;
    }
    else if (exprᴛ1 == oldtrace.EvGoStop) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sForever], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoSched) {
        mappedType = go122.EvGoStop;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sGosched], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoPreempt) {
        mappedType = go122.EvGoStop;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sPreempted], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoSleep) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSleep], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlock) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sEmpty], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoUnblock) {
        mappedType = go122.EvGoUnblock;
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockSend) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sChanSend], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockRecv) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sChanRecv], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockSelect) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSelect], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockSync) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSync], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockCond) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSyncCond], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockNet) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sNetwork], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoBlockGC) {
        mappedType = go122.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sMarkAssistWait], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvGoSysCall) {
        var blocked = false;
        Ꮡit.of(oldTraceConverter.Ꮡevents).All()((ж<oldtrace.Event> nev) => {
            // Look for the next event for the same G to determine if the syscall
            // blocked.
            if ((~nev).G != Ꮡev.Value.G) {
                return true;
            }
            // After an EvGoSysCall, the next event on the same G will either be
            // EvGoSysBlock to denote a blocking syscall, or some other event
            // (or the end of the trace) if the syscall didn't block.
            if ((~nev).Type == oldtrace.EvGoSysBlock) {
                blocked = true;
            }
            return false;
        });
        if (blocked){
            mappedType = go122.EvGoSyscallBegin;
            mappedArgs = new timedEventArgs(new array<uint64>(4){[1] = (uint64)ev.StkID});
        } else {
            // Convert the old instantaneous syscall event to a pair of syscall
            // begin and syscall end and give it the shortest possible duration,
            // 1ns.
            var out1 = new ΔEvent(
                ctx: new schedCtx(
                    G: ((GoID)(int64)ev.G),
                    P: ((ProcID)(int64)ev.P),
                    M: it.procMs[((ProcID)(int64)ev.P)]
                ),
                table: it.evt,
                @base: new baseEvent(
                    typ: go122.EvGoSyscallBegin,
                    time: ((ΔTime)(int64)ev.Ts),
                    args: new timedEventArgs(new array<uint64>(4){[1] = (uint64)ev.StkID})
                )
            );
            var out2 = new ΔEvent(
                ctx: out1.ctx,
                table: it.evt,
                @base: new baseEvent(
                    typ: go122.EvGoSyscallEnd,
                    time: ((ΔTime)(int64)(ev.Ts + 1)),
                    args: new timedEventArgs(new uint64[4].array())
                )
            );
            it.extra = append(it.extra, out2.ΔClone());
            return (out1.ΔClone(), default!);
        }
    }
    else if (exprᴛ1 == oldtrace.EvGoSysExit) {
        mappedType = go122.EvGoSyscallEndBlocked;
    }
    else if (exprᴛ1 == oldtrace.EvGoSysBlock) {
        return (new ΔEvent(nil), errSkip);
    }
    else if (exprᴛ1 == oldtrace.EvGoWaiting) {
        mappedType = go122.EvGoStatus;
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ~(uint64)0, (uint64)(uint8)go122.GoWaiting}.array(4));
        delete(it.createdPreInit, ((GoID)(int64)ev.Args[0]));
    }
    else if (exprᴛ1 == oldtrace.EvGoInSyscall) {
        mappedType = go122.EvGoStatus;
        mappedArgs = new timedEventArgs(new uint64[]{ // In the new tracer, GoStatus with GoSyscall knows what thread the
 // syscall is on. In the old tracer, EvGoInSyscall doesn't contain that
 // information and all we can do here is specify NoThread.
ev.Args[0], ~(uint64)0, (uint64)(uint8)go122.GoSyscall}.array(4));
        delete(it.createdPreInit, ((GoID)(int64)ev.Args[0]));
    }
    else if (exprᴛ1 == oldtrace.EvHeapAlloc) {
        mappedType = go122.EvHeapAlloc;
    }
    else if (exprᴛ1 == oldtrace.EvHeapGoal) {
        mappedType = go122.EvHeapGoal;
    }
    else if (exprᴛ1 == oldtrace.EvGCMarkAssistStart) {
        mappedType = go122.EvGCMarkAssistBegin;
    }
    else if (exprᴛ1 == oldtrace.EvGCMarkAssistDone) {
        mappedType = go122.EvGCMarkAssistEnd;
    }
    else if (exprᴛ1 == oldtrace.EvUserTaskCreate) {
        mappedType = go122.EvUserTaskBegin;
        var parent = ev.Args[1];
        if (parent == 0) {
            parent = (uint64)NoTask;
        }
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], parent, ev.Args[2], (uint64)ev.StkID}.array());
        var (name, _) = it.evt.of(evTable.Ꮡstrings).get(((stringID)ev.Args[2]));
        it.tasks[((TaskID)ev.Args[0])] = new taskState(name: name, parentID: ((TaskID)ev.Args[1]));
    }
    else if (exprᴛ1 == oldtrace.EvUserTaskEnd) {
        mappedType = go122.EvUserTaskEnd;
        var (ts, ok) = it.tasks[((TaskID)ev.Args[0]), ꟷ];
        if (ok){
            // Event.Task expects the parent and name to be smuggled in extra args
            // and as extra strings.
            delete(it.tasks, ((TaskID)ev.Args[0]));
            mappedArgs = new timedEventArgs(new uint64[]{
                ev.Args[0],
                ev.Args[1],
                (uint64)ts.parentID,
                (uint64)it.evt.addExtraString(ts.name)
            }.array());
        } else {
            mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ev.Args[1], (uint64)NoTask, (uint64)it.evt.addExtraString(""u8)}.array());
        }
    }
    else if (exprᴛ1 == oldtrace.EvUserRegion) {
        switch (ev.Args[1]) {
        case 0: {
            mappedType = go122.EvUserRegionBegin;
            break;
        }
        case 1: {
            mappedType = go122.EvUserRegionEnd;
            break;
        }}

        mappedArgs = new timedEventArgs(new uint64[]{ // start
 // end
ev.Args[0], ev.Args[2], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == oldtrace.EvUserLog) {
        mappedType = go122.EvUserLog;
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ev.Args[1], it.inlineToStringID[ev.Args[3]], (uint64)ev.StkID}.array());
    }
    else if (exprᴛ1 == oldtrace.EvCPUSample) {
        mappedType = go122.EvCPUSample;
        mappedArgs = new timedEventArgs(new uint64[]{ // When emitted by the Go 1.22 tracer, CPU samples have 5 arguments:
 // timestamp, M, P, G, stack. However, after they get turned into Event,
 // they have the arguments stack, M, P, G.
 //
 // In Go 1.21, CPU samples did not have Ms.
(uint64)ev.StkID, ~(uint64)0, (uint64)ev.P, ev.G}.array());
    }
    else { /* default: */
        return (new ΔEvent(nil), fmt.Errorf("unexpected event type %v"u8, ev.Type));
    }

    if (oldtrace.EventDescriptions[ev.Type].Stack) {
        {
            var stackIDs = go122.Specs()[mappedType].StackIDs; if (len(stackIDs) > 0) {
                mappedArgs[stackIDs[0] - 1] = (uint64)ev.StkID;
            }
        }
    }
    var m = NoThread;
    if (ev.P != -1 && ev.Type != oldtrace.EvCPUSample) {
        {
            var (t, ok) = it.procMs[((ProcID)(int64)ev.P), ꟷ]; if (ok) {
                m = t;
            }
        }
    }
    if (ev.Type == oldtrace.EvProcStop) {
        delete(it.procMs, ((ProcID)(int64)ev.P));
    }
    var g = ((GoID)(int64)ev.G);
    if (g == 0) {
        g = NoGoroutine;
    }
    var @out = new ΔEvent(
        ctx: new schedCtx(
            G: g,
            P: ((ProcID)(int64)ev.P),
            M: m
        ),
        table: it.evt,
        @base: new baseEvent(
            typ: mappedType,
            time: ((ΔTime)(int64)ev.Ts),
            args: mappedArgs.Clone()
        )
    );
    return (@out.ΔClone(), default!);
}

// convertOldFormat takes a fully loaded trace in the old trace format and
// returns an iterator over events in the new format.
internal static ж<oldTraceConverter> convertOldFormat(oldtrace.Trace pr) {
    var it = Ꮡ(new oldTraceConverter(nil));
    it.init(pr);
    return it;
}

} // end trace_package
