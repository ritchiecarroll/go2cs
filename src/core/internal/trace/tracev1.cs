// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements conversion from v1 (Go 1.11–Go 1.21) traces to the v2
// format (Go 1.22+).
//
// Most events have direct equivalents in v2, at worst requiring arguments to
// be reordered. Some events, such as GoWaiting need to look ahead for follow-up
// events to determine the correct translation. GoSyscall, which is an
// instantaneous event, gets turned into a 1 ns long pair of
// GoSyscallStart+GoSyscallEnd, unless we observe a GoSysBlock, in which case we
// emit a GoSyscallStart+GoSyscallEndBlocked pair with the correct duration
// (i.e. starting at the original GoSyscall).
//
// The resulting trace treats the trace v1 as a single, large generation,
// sharing a single evTable for all events.
//
// We use a new (compared to what was used for 'go tool trace' in earlier
// versions of Go) parser for v1 traces that is optimized for speed, low memory
// usage, and minimal GC pressure. It allocates events in batches so that even
// though we have to load the entire trace into memory, the conversion process
// shouldn't result in a doubling of memory usage, even if all converted events
// are kept alive, as we free batches once we're done with them.
//
// The conversion process is lossless.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using tracev1 = go.@internal.trace.@internal.tracev1_package;
using tracev2 = go.@internal.trace.tracev2_package;
using io = io_package;
using go.@internal.trace;
using go.@internal.trace.@internal;

partial class trace_package {

partial struct traceV1Converter {
    internal tracev1.Trace trace;
    internal ж<evTable> evt;
    internal bool preInit;
    internal map<GoID, EmptyStruct> createdPreInit;
    internal tracev1.Events events;
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

// Block reasons
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

// STW kinds
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

internal static error init(this ж<traceV1Converter> Ꮡit, tracev1.Trace pr) {
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

    // Convert from trace v1's Strings map to our dataTable.
    uint64 max = default!;
    foreach (var (id, s) in pr.Strings) {
        evt.of(evTable.Ꮡstrings).insert(((stringID)id), s);
        if (id > max) {
            max = id;
        }
    }
    pr.Strings = default!;

    // Add all strings used for UserLog. In the trace v1 format, these were
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
        it.inlineToStringID = builtin.append(it.inlineToStringID, nid);
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
    // tracev1 we wouldn't have to copy the map.
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
internal static (ΔEvent, error) next(this ж<traceV1Converter> Ꮡit) {
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

// convertEvent converts an event from the trace v1 format to zero or more
// events in the new format. Most events translate 1 to 1. Some events don't
// result in an event right away, in which case convertEvent returns errSkip.
// Some events result in more than one new event; in this case, convertEvent
// returns the first event and stores additional events in it.extra. When
// encountering events that tracev1 shouldn't be able to emit, ocnvertEvent
// returns a descriptive error.
internal static (ΔEvent OUT, error ERR) convertEvent(this ж<traceV1Converter> Ꮡit, ж<tracev1.Event> Ꮡev) {
    ref var it = ref Ꮡit.DerefOrNull();
    ref var ev = ref Ꮡev.DerefOrNull();

    tracev2.EventType mappedType = default!;
    timedEventArgs mappedArgs = default!;
    copy(mappedArgs[..], ev.Args[..]);

    var exprᴛ1 = ev.Type;
    if (exprᴛ1 == tracev1.EvGomaxprocs) {
        mappedType = tracev2.EvProcsChange;
        if (it.preInit) {
            // The first EvGomaxprocs signals the end of trace initialization. At this point we've seen
            // all goroutines that already existed at trace begin.
            it.preInit = false;
            foreach (var (gid, _) in it.createdPreInit) {
                // These are goroutines that already existed when tracing started but for which we
                // received neither GoWaiting, GoInSyscall, or GoStart. These are goroutines that are in
                // the states _Gidle or _Grunnable.
                it.extra = builtin.append(it.extra, new ΔEvent(
                    ctx: new schedCtx(
                        // G: GoID(gid),
                        G: NoGoroutine,
                        P: NoProc,
                        M: NoThread
                    ),
                    table: it.evt,
                    @base: new baseEvent(
                        typ: tracev2.EvGoStatus,
                        time: ((ΔTime)(int64)ev.Ts),
                        args: new timedEventArgs(new uint64[]{(uint64)(int64)gid, ~(uint64)0, (uint64)(uint8)tracev2.GoRunnable}.array(4))
                    )
                ));
            }
            it.createdPreInit = default!;
            return (new ΔEvent(nil), errSkip);
        }
    }
    else if (exprᴛ1 == tracev1.EvProcStart) {
        it.procMs[((ProcID)(int64)ev.P)] = ((ThreadID)(int64)ev.Args[0]);
        {
            var (_, ok) = it.seenProcs[((ProcID)(int64)ev.P), ꟷ]; if (ok){
                mappedType = tracev2.EvProcStart;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P}.array(4));
            } else {
                it.seenProcs[((ProcID)(int64)ev.P)] = new EmptyStruct();
                mappedType = tracev2.EvProcStatus;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P, (uint64)(uint8)tracev2.ProcRunning}.array(4));
            }
        }
    }
    else if (exprᴛ1 == tracev1.EvProcStop) {
        {
            var (_, ok) = it.seenProcs[((ProcID)(int64)ev.P), ꟷ]; if (ok){
                mappedType = tracev2.EvProcStop;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P}.array(4));
            } else {
                it.seenProcs[((ProcID)(int64)ev.P)] = new EmptyStruct();
                mappedType = tracev2.EvProcStatus;
                mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.P, (uint64)(uint8)tracev2.ProcIdle}.array(4));
            }
        }
    }
    else if (exprᴛ1 == tracev1.EvGCStart) {
        mappedType = tracev2.EvGCBegin;
    }
    else if (exprᴛ1 == tracev1.EvGCDone) {
        mappedType = tracev2.EvGCEnd;
    }
    else if (exprᴛ1 == tracev1.EvSTWStart) {
        var sid = it.builtinToStringID[(nint)sSTWUnknown + it.trace.STWReason(ev.Args[0])];
        it.lastStwReason = sid;
        mappedType = tracev2.EvSTWBegin;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)sid}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvSTWDone) {
        mappedType = tracev2.EvSTWEnd;
        mappedArgs = new timedEventArgs(new uint64[]{it.lastStwReason}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGCSweepStart) {
        mappedType = tracev2.EvGCSweepBegin;
    }
    else if (exprᴛ1 == tracev1.EvGCSweepDone) {
        mappedType = tracev2.EvGCSweepEnd;
    }
    else if (exprᴛ1 == tracev1.EvGoCreate) {
        if (it.preInit) {
            it.createdPreInit[((GoID)(int64)ev.Args[0])] = new EmptyStruct();
            return (new ΔEvent(nil), errSkip);
        }
        mappedType = tracev2.EvGoCreate;
    }
    else if (exprᴛ1 == tracev1.EvGoStart) {
        if (it.preInit){
            mappedType = tracev2.EvGoStatus;
            mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ~(uint64)0, (uint64)(uint8)tracev2.GoRunning}.array(4));
            delete(it.createdPreInit, ((GoID)(int64)ev.Args[0]));
        } else {
            mappedType = tracev2.EvGoStart;
        }
    }
    else if (exprᴛ1 == tracev1.EvGoStartLabel) {
        it.extra = new ΔEvent[]{new(
            ctx: new schedCtx(
                G: ((GoID)(int64)ev.G),
                P: ((ProcID)(int64)ev.P),
                M: it.procMs[((ProcID)(int64)ev.P)]
            ),
            table: it.evt,
            @base: new baseEvent(
                typ: tracev2.EvGoLabel,
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
                typ: tracev2.EvGoStart,
                time: ((ΔTime)(int64)ev.Ts),
                args: mappedArgs.Clone()
            )
        ), default!);
    }
    else if (exprᴛ1 == tracev1.EvGoEnd) {
        mappedType = tracev2.EvGoDestroy;
    }
    else if (exprᴛ1 == tracev1.EvGoStop) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sForever], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoSched) {
        mappedType = tracev2.EvGoStop;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sGosched], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoPreempt) {
        mappedType = tracev2.EvGoStop;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sPreempted], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoSleep) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSleep], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlock) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sEmpty], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoUnblock) {
        mappedType = tracev2.EvGoUnblock;
    }
    else if (exprᴛ1 == tracev1.EvGoBlockSend) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sChanSend], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlockRecv) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sChanRecv], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlockSelect) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSelect], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlockSync) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSync], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlockCond) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sSyncCond], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlockNet) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sNetwork], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoBlockGC) {
        mappedType = tracev2.EvGoBlock;
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)it.builtinToStringID[sMarkAssistWait], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvGoSysCall) {
        // Look for the next event for the same G to determine if the syscall
        // blocked.
        var blocked = false;
        Ꮡit.of(traceV1Converter.Ꮡevents).All()((ж<tracev1.Event> nev) => {
            if ((~nev).G != Ꮡev.Value.G) {
                return true;
            }
            // After an EvGoSysCall, the next event on the same G will either be
            // EvGoSysBlock to denote a blocking syscall, or some other event
            // (or the end of the trace) if the syscall didn't block.
            if ((~nev).Type == tracev1.EvGoSysBlock) {
                blocked = true;
            }
            return false;
        });
        if (blocked){
            mappedType = tracev2.EvGoSyscallBegin;
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
                    typ: tracev2.EvGoSyscallBegin,
                    time: ((ΔTime)(int64)ev.Ts),
                    args: new timedEventArgs(new array<uint64>(4){[1] = (uint64)ev.StkID})
                )
            );

            var out2 = new ΔEvent(
                ctx: out1.ctx,
                table: it.evt,
                @base: new baseEvent(
                    typ: tracev2.EvGoSyscallEnd,
                    time: ((ΔTime)(int64)(ev.Ts + 1)),
                    args: new timedEventArgs(new uint64[4].array())
                )
            );

            it.extra = builtin.append(it.extra, out2.ΔClone());
            return (out1.ΔClone(), default!);
        }
    }
    else if (exprᴛ1 == tracev1.EvGoSysExit) {
        mappedType = tracev2.EvGoSyscallEndBlocked;
    }
    else if (exprᴛ1 == tracev1.EvGoSysBlock) {
        return (new ΔEvent(nil), errSkip);
    }
    else if (exprᴛ1 == tracev1.EvGoWaiting) {
        mappedType = tracev2.EvGoStatus;
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ~(uint64)0, (uint64)(uint8)tracev2.GoWaiting}.array(4));
        delete(it.createdPreInit, ((GoID)(int64)ev.Args[0]));
    }
    else if (exprᴛ1 == tracev1.EvGoInSyscall) {
        mappedType = tracev2.EvGoStatus;
        // In the new tracer, GoStatus with GoSyscall knows what thread the
        // syscall is on. In trace v1, EvGoInSyscall doesn't contain that
        // information and all we can do here is specify NoThread.
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ~(uint64)0, (uint64)(uint8)tracev2.GoSyscall}.array(4));
        delete(it.createdPreInit, ((GoID)(int64)ev.Args[0]));
    }
    else if (exprᴛ1 == tracev1.EvHeapAlloc) {
        mappedType = tracev2.EvHeapAlloc;
    }
    else if (exprᴛ1 == tracev1.EvHeapGoal) {
        mappedType = tracev2.EvHeapGoal;
    }
    else if (exprᴛ1 == tracev1.EvGCMarkAssistStart) {
        mappedType = tracev2.EvGCMarkAssistBegin;
    }
    else if (exprᴛ1 == tracev1.EvGCMarkAssistDone) {
        mappedType = tracev2.EvGCMarkAssistEnd;
    }
    else if (exprᴛ1 == tracev1.EvUserTaskCreate) {
        mappedType = tracev2.EvUserTaskBegin;
        var parent = ev.Args[1];
        if (parent == 0) {
            parent = (uint64)NoTask;
        }
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], parent, ev.Args[2], (uint64)ev.StkID}.array());
        var (name, _) = it.evt.of(evTable.Ꮡstrings).get(((stringID)ev.Args[2]));
        it.tasks[((TaskID)ev.Args[0])] = new taskState(name: name, parentID: ((TaskID)ev.Args[1]));
    }
    else if (exprᴛ1 == tracev1.EvUserTaskEnd) {
        mappedType = tracev2.EvUserTaskEnd;
        // Event.Task expects the parent and name to be smuggled in extra args
        // and as extra strings.
        var (ts, ok) = it.tasks[((TaskID)ev.Args[0]), ꟷ];
        if (ok){
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
    else if (exprᴛ1 == tracev1.EvUserRegion) {
        switch (ev.Args[1]) {
        case 0: { // start
            mappedType = tracev2.EvUserRegionBegin;
            break;
        }
        case 1: { // end
            mappedType = tracev2.EvUserRegionEnd;
            break;
        }}
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ev.Args[2], (uint64)ev.StkID}.array(4));
    }
    else if (exprᴛ1 == tracev1.EvUserLog) {
        mappedType = tracev2.EvUserLog;
        mappedArgs = new timedEventArgs(new uint64[]{ev.Args[0], ev.Args[1], it.inlineToStringID[ev.Args[3]], (uint64)ev.StkID}.array());
    }
    else if (exprᴛ1 == tracev1.EvCPUSample) {
        mappedType = tracev2.EvCPUSample;
        // When emitted by the Go 1.22 tracer, CPU samples have 5 arguments:
        // timestamp, M, P, G, stack. However, after they get turned into Event,
        // they have the arguments stack, M, P, G.
        //
        // In Go 1.21, CPU samples did not have Ms.
        mappedArgs = new timedEventArgs(new uint64[]{(uint64)ev.StkID, ~(uint64)0, (uint64)ev.P, ev.G}.array());
    }
    else { /* default: */
        return (new ΔEvent(nil), fmt.Errorf("unexpected event type %v"u8, ev.Type));
    }

    if (tracev1.EventDescriptions[ev.Type].Stack) {
        {
            var stackIDs = tracev2.Specs()[mappedType].StackIDs; if (len(stackIDs) > 0) {
                mappedArgs[stackIDs[0] - 1] = (uint64)ev.StkID;
            }
        }
    }

    var m = NoThread;
    if (ev.P != -1 && ev.Type != tracev1.EvCPUSample) {
        {
            var (t, ok) = it.procMs[((ProcID)(int64)ev.P), ꟷ]; if (ok) {
                m = t;
            }
        }
    }
    if (ev.Type == tracev1.EvProcStop) {
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

// convertV1Trace takes a fully loaded trace in the v1 trace format and
// returns an iterator over events in the new format.
internal static ж<traceV1Converter> convertV1Trace(tracev1.Trace pr) {
    var it = Ꮡ(new traceV1Converter(nil));
    it.init(pr);
    return it;
}

} // end trace_package
