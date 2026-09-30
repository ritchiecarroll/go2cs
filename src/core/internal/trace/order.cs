// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using strings = strings_package;
using @event = go.@internal.trace.event_package;
using go122 = go.@internal.trace.@event.go122_package;
using version = go.@internal.trace.version_package;
using go.@internal.trace;
using go.@internal.trace.@event;
using io = io_package;
using ꓸꓸꓸuint64 = Span<uint64>;

partial class trace_package {

// ordering emulates Go scheduler state for both validation and
// for putting events in the right order.
//
// The interface to ordering consists of two methods: Advance
// and Next. Advance is called to try and advance an event and
// add completed events to the ordering. Next is used to pick
// off events in the ordering.
[GoType] partial struct ordering {
    internal map<GoID, ж<gState>> gStates;
    internal map<ProcID, ж<pState>> pStates; // TODO: The keys are dense, so this can be a slice.
    internal map<ThreadID, ж<mState>> mStates;
    internal map<TaskID, taskState> activeTasks;
    internal uint64 gcSeq;
    internal gcState gcState;
    internal uint64 initialGen;
    internal queue<ΔEvent> queue;
}

// Advance checks if it's valid to proceed with ev which came from thread m.
//
// It assumes the gen value passed to it is monotonically increasing across calls.
//
// If any error is returned, then the trace is broken and trace parsing must cease.
// If it's not valid to advance with ev, but no error was encountered, the caller
// should attempt to advance with other candidate events from other threads. If the
// caller runs out of candidates, the trace is invalid.
//
// If this returns true, Next is guaranteed to return a complete event. However,
// multiple events may be added to the ordering, so the caller should (but is not
// required to) continue to call Next until it is exhausted.
internal static (bool, error) Advance(this ж<ordering> Ꮡo, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen) {
    ref var o = ref Ꮡo.DerefOrNull();
    ref var ev = ref Ꮡev.DerefOrNull();

    if (o.initialGen == 0) {
        // Set the initial gen if necessary.
        o.initialGen = gen;
    }
    schedCtx curCtx = default!;
    schedCtx newCtx = default!;
    curCtx.M = m;
    newCtx.M = m;
    ж<mState> ms = default!;
    if (m == NoThread){
        curCtx.P = NoProc;
        curCtx.G = NoGoroutine;
        newCtx = curCtx;
    } else {
        // Pull out or create the mState for this event.
        bool okΔ1 = default!;
        (ms, okΔ1) = o.mStates[m, ꟷ];
        if (!okΔ1) {
            ms = Ꮡ(new mState(
                g: NoGoroutine,
                p: NoProc
            ));
            o.mStates[m] = ms;
        }
        curCtx.P = ms.Value.p;
        curCtx.G = ms.Value.g;
        newCtx = curCtx;
    }
    var f = orderingDispatch[ev.typ];
    if (f == default!) {
        return (false, fmt.Errorf("bad event type found while ordering: %v"u8, ev.typ));
    }
    (newCtx, var ok, var err) = f(Ꮡo, Ꮡev, Ꮡevt, m, gen, curCtx);
    if (err == default! && ok && ms != nil) {
        // Update the mState for this event.
        ms.Value.p = newCtx.P;
        ms.Value.g = newCtx.G;
    }
    return (ok, err);
}

// type orderingHandleFunc is a methodless func type — rendered inline as its base delegate

// Procs.
// Goroutines.
// STW.
// GC events.
// Annotations.
// Coroutines. Added in Go 1.23.
// GoStatus event with a stack. Added in Go 1.23.
// Experimental events.
// Experimental heap span events. Added in Go 1.23.
// Experimental heap object events. Added in Go 1.23.
// Experimental goroutine stack events. Added in Go 1.23.
internal static array<Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>> orderingDispatch;
internal static void initᴛorderingDispatch() { orderingDispatch = new golib.SparseArray<Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>>{
    [go122.EvProcsChange] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAnnotation)),
    [go122.EvProcStart] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceProcStart)),
    [go122.EvProcStop] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceProcStop)),
    [go122.EvProcSteal] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceProcSteal)),
    [go122.EvProcStatus] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceProcStatus)),
    [go122.EvGoCreate] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoCreate)),
    [go122.EvGoCreateSyscall] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoCreateSyscall)),
    [go122.EvGoStart] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoStart)),
    [go122.EvGoDestroy] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoStopExec)),
    [go122.EvGoDestroySyscall] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoDestroySyscall)),
    [go122.EvGoStop] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoStopExec)),
    [go122.EvGoBlock] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoStopExec)),
    [go122.EvGoUnblock] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoUnblock)),
    [go122.EvGoSyscallBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoSyscallBegin)),
    [go122.EvGoSyscallEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoSyscallEnd)),
    [go122.EvGoSyscallEndBlocked] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoSyscallEndBlocked)),
    [go122.EvGoStatus] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoStatus)),
    [go122.EvSTWBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoRangeBegin)),
    [go122.EvSTWEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoRangeEnd)),
    [go122.EvGCActive] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGCActive)),
    [go122.EvGCBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGCBegin)),
    [go122.EvGCEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGCEnd)),
    [go122.EvGCSweepActive] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGCSweepActive)),
    [go122.EvGCSweepBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGCSweepBegin)),
    [go122.EvGCSweepEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGCSweepEnd)),
    [go122.EvGCMarkAssistActive] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoRangeActive)),
    [go122.EvGCMarkAssistBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoRangeBegin)),
    [go122.EvGCMarkAssistEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoRangeEnd)),
    [go122.EvHeapAlloc] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceHeapMetric)),
    [go122.EvHeapGoal] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceHeapMetric)),
    [go122.EvGoLabel] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAnnotation)),
    [go122.EvUserTaskBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceUserTaskBegin)),
    [go122.EvUserTaskEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceUserTaskEnd)),
    [go122.EvUserRegionBegin] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceUserRegionBegin)),
    [go122.EvUserRegionEnd] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceUserRegionEnd)),
    [go122.EvUserLog] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAnnotation)),
    [go122.EvGoSwitch] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoSwitch)),
    [go122.EvGoSwitchDestroy] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoSwitch)),
    [go122.EvGoCreateBlocked] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoCreate)),
    [go122.EvGoStatusStack] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceGoStatus)),
    [go122.EvSpan] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvSpanAlloc] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvSpanFree] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvHeapObject] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvHeapObjectAlloc] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvHeapObjectFree] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvGoroutineStack] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvGoroutineStackAlloc] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree)),
    [go122.EvGoroutineStackFree] = ((Func<ж<ordering>, ж<baseEvent>, ж<evTable>, ThreadID, uint64, schedCtx, (schedCtx, bool, error)>)(advanceAllocFree))
}.array(256); }

[GoRecv] internal static (schedCtx, bool, error) advanceProcStatus(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    ref var pid = ref heap<ProcID>(out var Ꮡpid);
    pid = ((ProcID)(int64)ev.args[0]);
    ref var status = ref heap<go122.ProcStatus>(out var Ꮡstatus);
    status = ((go122.ProcStatus)(uint8)ev.args[1]);
    if ((nint)(uint8)status >= len(go122ProcStatus2ProcState)) {
        return (curCtx, false, fmt.Errorf("invalid status for proc %d: %d"u8, pid, status));
    }
    var oldState = go122ProcStatus2ProcState[status];
    {
        var (s, ok) = o.pStates[pid, ꟷ]; if (ok){
            if (status == go122.ProcSyscallAbandoned && (~s).status == go122.ProcSyscall){
                // ProcSyscallAbandoned is a special case of ProcSyscall. It indicates a
                // potential loss of information, but if we're already in ProcSyscall,
                // we haven't lost the relevant information. Promote the status and advance.
                oldState = ProcRunning;
                ev.args[1] = (uint64)(uint8)go122.ProcSyscall;
            } else 
            if (status == go122.ProcSyscallAbandoned && (~s).status == go122.ProcSyscallAbandoned){
                // If we're passing through ProcSyscallAbandoned, then there's no promotion
                // to do. We've lost the M that this P is associated with. However it got there,
                // it's going to appear as idle in the API, so pass through as idle.
                oldState = ProcIdle;
                ev.args[1] = (uint64)(uint8)go122.ProcSyscallAbandoned;
            } else 
            if ((~s).status != status) {
                return (curCtx, false, fmt.Errorf("inconsistent status for proc %d: old %v vs. new %v"u8, pid, (~s).status, status));
            }
            s.Value.seq = makeSeq(gen, 0); // Reset seq.
        } else {
            o.pStates[pid] = Ꮡ(new pState(id: pid, status: status, seq: makeSeq(gen, 0)));
            if (gen == o.initialGen){
                oldState = ProcUndetermined;
            } else {
                oldState = ProcNotExist;
            }
        }
    }
    ev.extra(version.Go122)[0] = (uint64)(uint8)oldState; // Smuggle in the old state for StateTransition.
    // Bind the proc to the new context, if it's running.
    var newCtx = curCtx;
    if (status == go122.ProcRunning || status == go122.ProcSyscall) {
        newCtx.P = pid;
    }
    // If we're advancing through ProcSyscallAbandoned *but* oldState is running then we've
    // promoted it to ProcSyscall. However, because it's ProcSyscallAbandoned, we know this
    // P is about to get stolen and its status very likely isn't being emitted by the same
    // thread it was bound to. Since this status is Running -> Running and Running is binding,
    // we need to make sure we emit it in the right context: the context to which it is bound.
    // Find it, and set our current context to it.
    if (status == go122.ProcSyscallAbandoned && oldState == ProcRunning) {
        // N.B. This is slow but it should be fairly rare.
        var found = false;
        foreach (var (mid, ms) in o.mStates) {
            if ((~ms).p == pid) {
                curCtx.M = mid;
                curCtx.P = pid;
                curCtx.G = ms.Value.g;
                found = true;
            }
        }
        if (!found) {
            return (curCtx, false, fmt.Errorf("failed to find sched context for proc %d that's about to be stolen"u8, pid));
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceProcStart(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var pid = ((ProcID)(int64)ev.args[0]);
    var seq = makeSeq(gen, ev.args[1]);
    // Try to advance. We might fail here due to sequencing, because the P hasn't
    // had a status emitted, or because we already have a P and we're in a syscall,
    // and we haven't observed that it was stolen from us yet.
    var (state, ok) = o.pStates[pid, ꟷ];
    if (!ok || (~state).status != go122.ProcIdle || !seq.succeeds((~state).seq) || curCtx.P != NoProc) {
        // We can't make an inference as to whether this is bad. We could just be seeing
        // a ProcStart on a different M before the proc's state was emitted, or before we
        // got to the right point in the trace.
        //
        // Note that we also don't advance here if we have a P and we're in a syscall.
        return (curCtx, false, default!);
    }
    // We can advance this P. Check some invariants.
    //
    // We might have a goroutine if a goroutine is exiting a syscall.
    var reqs = new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustNotHave, Goroutine: @event.MayHave);
    {
        var err = validateCtx(curCtx, reqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    state.Value.status = go122.ProcRunning;
    state.Value.seq = seq;
    var newCtx = curCtx;
    newCtx.P = pid;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceProcStop(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // We must be able to advance this P.
    //
    // There are 2 ways a P can stop: ProcStop and ProcSteal. ProcStop is used when the P
    // is stopped by the same M that started it, while ProcSteal is used when another M
    // steals the P by stopping it from a distance.
    //
    // Since a P is bound to an M, and we're stopping on the same M we started, it must
    // always be possible to advance the current M's P from a ProcStop. This is also why
    // ProcStop doesn't need a sequence number.
    var (state, ok) = o.pStates[curCtx.P, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for proc (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.P));
    }
    if ((~state).status != go122.ProcRunning && (~state).status != go122.ProcSyscall) {
        return (curCtx, false, fmt.Errorf("%s event for proc that's not %s or %s"u8, go122.EventString(ev.typ), go122.ProcRunning, go122.ProcSyscall));
    }
    var reqs = new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustHave, Goroutine: @event.MayHave);
    {
        var err = validateCtx(curCtx, reqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    state.Value.status = go122.ProcIdle;
    var newCtx = curCtx;
    newCtx.P = NoProc;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceProcSteal(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var pid = ((ProcID)(int64)ev.args[0]);
    var seq = makeSeq(gen, ev.args[1]);
    var (state, ok) = o.pStates[pid, ꟷ];
    if (!ok || ((~state).status != go122.ProcSyscall && (~state).status != go122.ProcSyscallAbandoned) || !seq.succeeds((~state).seq)) {
        // We can't make an inference as to whether this is bad. We could just be seeing
        // a ProcStart on a different M before the proc's state was emitted, or before we
        // got to the right point in the trace.
        return (curCtx, false, default!);
    }
    // We can advance this P. Check some invariants.
    var reqs = new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MayHave, Goroutine: @event.MayHave);
    {
        var err = validateCtx(curCtx, reqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    // Smuggle in the P state that let us advance so we can surface information to the event.
    // Specifically, we need to make sure that the event is interpreted not as a transition of
    // ProcRunning -> ProcIdle but ProcIdle -> ProcIdle instead.
    //
    // ProcRunning is binding, but we may be running with a P on the current M and we can't
    // bind another P. This P is about to go ProcIdle anyway.
    var oldStatus = state.Value.status;
    ev.extra(version.Go122)[0] = (uint64)(uint8)oldStatus;
    // Update the P's status and sequence number.
    state.Value.status = go122.ProcIdle;
    state.Value.seq = seq;
    // If we've lost information then don't try to do anything with the M.
    // It may have moved on and we can't be sure.
    if (oldStatus == go122.ProcSyscallAbandoned) {
        o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
        return (curCtx, true, default!);
    }
    // Validate that the M we're stealing from is what we expect.
    var mid = ((ThreadID)(int64)ev.args[2]); // The M we're stealing from.
    var newCtx = curCtx;
    if (mid == curCtx.M) {
        // We're stealing from ourselves. This behaves like a ProcStop.
        if (curCtx.P != pid) {
            return (curCtx, false, fmt.Errorf("tried to self-steal proc %d (thread %d), but got proc %d instead"u8, pid, mid, curCtx.P));
        }
        newCtx.P = NoProc;
        o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
        return (newCtx, true, default!);
    }
    // We're stealing from some other M.
    (var mState, ok) = o.mStates[mid, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("stole proc from non-existent thread %d"u8, mid));
    }
    // Make sure we're actually stealing the right P.
    if ((~mState).p != pid) {
        return (curCtx, false, fmt.Errorf("tried to steal proc %d from thread %d, but got proc %d instead"u8, pid, mid, (~mState).p));
    }
    // Tell the M it has no P so it can proceed.
    //
    // This is safe because we know the P was in a syscall and
    // the other M must be trying to get out of the syscall.
    // GoSyscallEndBlocked cannot advance until the corresponding
    // M loses its P.
    mState.Value.p = NoProc;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoStatus(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    ref var gid = ref heap<GoID>(out var Ꮡgid);
    gid = ((GoID)(int64)ev.args[0]);
    var mid = ((ThreadID)(int64)ev.args[1]);
    ref var status = ref heap<go122.GoStatus>(out var Ꮡstatus);
    status = ((go122.GoStatus)(uint8)ev.args[2]);
    if ((nint)(uint8)status >= len(go122GoStatus2GoState)) {
        return (curCtx, false, fmt.Errorf("invalid status for goroutine %d: %d"u8, gid, status));
    }
    var oldState = go122GoStatus2GoState[status];
    {
        var (s, ok) = o.gStates[gid, ꟷ]; if (ok){
            if ((~s).status != status) {
                return (curCtx, false, fmt.Errorf("inconsistent status for goroutine %d: old %v vs. new %v"u8, gid, (~s).status, status));
            }
            s.Value.seq = makeSeq(gen, 0); // Reset seq.
        } else 
        if (gen == o.initialGen){
            // Set the state.
            o.gStates[gid] = Ꮡ(new gState(id: gid, status: status, seq: makeSeq(gen, 0)));
            oldState = GoUndetermined;
        } else {
            return (curCtx, false, fmt.Errorf("found goroutine status for new goroutine after the first generation: id=%v status=%v"u8, gid, status));
        }
    }
    ev.args[2] = (uint64)(((uint64)(uint8)oldState << (int)(32)) | (uint64)(uint8)status); // Smuggle in the old state for StateTransition.
    var newCtx = curCtx;
    var exprᴛ1 = status;
    if (exprᴛ1 == go122.GoRunning) {
        newCtx.G = gid;
    }
    else if (exprᴛ1 == go122.GoSyscall) {
        do {
            if (mid == NoThread) {
                // Bind the goroutine to the new context, since it's running.
                return (curCtx, false, fmt.Errorf("found goroutine %d in syscall without a thread"u8, gid));
            }
            if (mid == curCtx.M) {
                // Is the syscall on this thread? If so, bind it to the context.
                // Otherwise, we're talking about a G sitting in a syscall on an M.
                // Validate the named M.
                if (gen != o.initialGen && curCtx.G != gid) {
                    // If this isn't the first generation, we *must* have seen this
                    // binding occur already. Even if the G was blocked in a syscall
                    // for multiple generations since trace start, we would have seen
                    // a previous GoStatus event that bound the goroutine to an M.
                    return (curCtx, false, fmt.Errorf("inconsistent thread for syscalling goroutine %d: thread has goroutine %d"u8, gid, curCtx.G));
                }
                newCtx.G = gid;
                break;
            }
            var (ms, ok) = o.mStates[mid, ꟷ];
            if (ok){
                // Now we're talking about a thread and goroutine that have been
                // blocked on a syscall for the entire generation. This case must
                // not have a P; the runtime makes sure that all Ps are traced at
                // the beginning of a generation, which involves taking a P back
                // from every thread.
                // This M has been seen. That means we must have seen this
                // goroutine go into a syscall on this thread at some point.
                if ((~ms).g != gid) {
                    // But the G on the M doesn't match. Something's wrong.
                    return (curCtx, false, fmt.Errorf("inconsistent thread for syscalling goroutine %d: thread has goroutine %d"u8, gid, (~ms).g));
                }
                // This case is just a Syscall->Syscall event, which needs to
                // appear as having the G currently bound to this M.
                curCtx.G = ms.Value.g;
            } else 
            if (!ok) {
                // The M hasn't been seen yet. That means this goroutine
                // has just been sitting in a syscall on this M. Create
                // a state for it.
                o.mStates[mid] = Ꮡ(new mState(g: gid, p: NoProc));
            }
            curCtx.M = mid;
        } while (false);
    }

    // Don't set curCtx.G in this case because this event is the
    // binding event (and curCtx represents the "before" state).
    // Update the current context to the M we're talking about.
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoCreate(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Goroutines must be created on a running P, but may or may not be created
    // by a running goroutine.
    var reqs = new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustHave, Goroutine: @event.MayHave);
    {
        var err = validateCtx(curCtx, reqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    // If we have a goroutine, it must be running.
    {
        var (state, ok) = o.gStates[curCtx.G, ꟷ]; if (ok && (~state).status != go122.GoRunning) {
            return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %s"u8, go122.EventString(ev.typ), GoRunning));
        }
    }
    // This goroutine created another. Add a state for it.
    ref var newgid = ref heap<GoID>(out var Ꮡnewgid);
    newgid = ((GoID)(int64)ev.args[0]);
    {
        var (_, ok) = o.gStates[newgid, ꟷ]; if (ok) {
            return (curCtx, false, fmt.Errorf("tried to create goroutine (%v) that already exists"u8, newgid));
        }
    }
    ref var status = ref heap<go122.GoStatus>(out var Ꮡstatus);
    status = go122.GoRunnable;
    if (ev.typ == go122.EvGoCreateBlocked) {
        status = go122.GoWaiting;
    }
    o.gStates[newgid] = Ꮡ(new gState(id: newgid, status: status, seq: makeSeq(gen, 0)));
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoStopExec(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // These are goroutine events that all require an active running
    // goroutine on some thread. They must *always* be advance-able,
    // since running goroutines are bound to their M.
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var (state, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for goroutine (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.G));
    }
    if ((~state).status != go122.GoRunning) {
        return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %s"u8, go122.EventString(ev.typ), GoRunning));
    }
    // Handle each case slightly differently; we just group them together
    // because they have shared preconditions.
    var newCtx = curCtx;
    var exprᴛ1 = ev.typ;
    if (exprᴛ1 == go122.EvGoDestroy) {
        delete(o.gStates, // This goroutine is exiting itself.
 curCtx.G);
        newCtx.G = NoGoroutine;
    }
    else if (exprᴛ1 == go122.EvGoStop) {
        state.Value.status = go122.GoRunnable;
        newCtx.G = NoGoroutine;
    }
    else if (exprᴛ1 == go122.EvGoBlock) {
        state.Value.status = go122.GoWaiting;
        newCtx.G = NoGoroutine;
    }

    // Goroutine stopped (yielded). It's runnable but not running on this M.
    // Goroutine blocked. It's waiting now and not running on this M.
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoStart(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var gid = ((GoID)(int64)ev.args[0]);
    var seq = makeSeq(gen, ev.args[1]);
    var (state, ok) = o.gStates[gid, ꟷ];
    if (!ok || (~state).status != go122.GoRunnable || !seq.succeeds((~state).seq)) {
        // We can't make an inference as to whether this is bad. We could just be seeing
        // a GoStart on a different M before the goroutine was created, before it had its
        // state emitted, or before we got to the right point in the trace yet.
        return (curCtx, false, default!);
    }
    // We can advance this goroutine. Check some invariants.
    var reqs = new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustHave, Goroutine: @event.MustNotHave);
    {
        var err = validateCtx(curCtx, reqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    state.Value.status = go122.GoRunning;
    state.Value.seq = seq;
    var newCtx = curCtx;
    newCtx.G = gid;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoUnblock(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // N.B. These both reference the goroutine to unblock, not the current goroutine.
    var gid = ((GoID)(int64)ev.args[0]);
    var seq = makeSeq(gen, ev.args[1]);
    var (state, ok) = o.gStates[gid, ꟷ];
    if (!ok || (~state).status != go122.GoWaiting || !seq.succeeds((~state).seq)) {
        // We can't make an inference as to whether this is bad. We could just be seeing
        // a GoUnblock on a different M before the goroutine was created and blocked itself,
        // before it had its state emitted, or before we got to the right point in the trace yet.
        return (curCtx, false, default!);
    }
    state.Value.status = go122.GoRunnable;
    state.Value.seq = seq;
    // N.B. No context to validate. Basically anything can unblock
    // a goroutine (e.g. sysmon).
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoSwitch(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // GoSwitch and GoSwitchDestroy represent a trio of events:
    // - Unblock of the goroutine to switch to.
    // - Block or destroy of the current goroutine.
    // - Start executing the next goroutine.
    //
    // Because it acts like a GoStart for the next goroutine, we can
    // only advance it if the sequence numbers line up.
    //
    // The current goroutine on the thread must be actively running.
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var (curGState, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for goroutine (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.G));
    }
    if ((~curGState).status != go122.GoRunning) {
        return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %s"u8, go122.EventString(ev.typ), GoRunning));
    }
    var nextg = ((GoID)(int64)ev.args[0]);
    var seq = makeSeq(gen, ev.args[1]); // seq is for nextg, not curCtx.G.
    (var nextGState, ok) = o.gStates[nextg, ꟷ];
    if (!ok || (~nextGState).status != go122.GoWaiting || !seq.succeeds((~nextGState).seq)) {
        // We can't make an inference as to whether this is bad. We could just be seeing
        // a GoSwitch on a different M before the goroutine was created, before it had its
        // state emitted, or before we got to the right point in the trace yet.
        return (curCtx, false, default!);
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    // Update the state of the executing goroutine and emit an event for it
    // (GoSwitch and GoSwitchDestroy will be interpreted as GoUnblock events
    // for nextg).
    var exprᴛ1 = ev.typ;
    if (exprᴛ1 == go122.EvGoSwitch) {
        curGState.Value.status = go122.GoWaiting;
        o.queue.push(makeEvent(Ꮡevt, // Goroutine blocked. It's waiting now and not running on this M.
 // Emit a GoBlock event.
 // TODO(mknyszek): Emit a reason.
 curCtx, go122.EvGoBlock, ev.time, 0, /* no reason */
 0));
    }
    else if (exprᴛ1 == go122.EvGoSwitchDestroy) {
        delete(o.gStates, /* no stack */
 // This goroutine is exiting itself.
 curCtx.G);
        o.queue.push(makeEvent(Ꮡevt, // Emit a GoDestroy event.
 curCtx, go122.EvGoDestroy, ev.time));
    }

    // Update the state of the next goroutine.
    nextGState.Value.status = go122.GoRunning;
    nextGState.Value.seq = seq;
    var newCtx = curCtx;
    newCtx.G = nextg;
    // Queue an event for the next goroutine starting to run.
    var startCtx = curCtx;
    startCtx.G = NoGoroutine;
    o.queue.push(makeEvent(Ꮡevt, startCtx, go122.EvGoStart, ev.time, (uint64)(int64)nextg, ev.args[1]));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoSyscallBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Entering a syscall requires an active running goroutine with a
    // proc on some thread. It is always advancable.
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var (state, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for goroutine (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.G));
    }
    if ((~state).status != go122.GoRunning) {
        return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %s"u8, go122.EventString(ev.typ), GoRunning));
    }
    // Goroutine entered a syscall. It's still running on this P and M.
    state.Value.status = go122.GoSyscall;
    (var pState, ok) = o.pStates[curCtx.P, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("uninitialized proc %d found during %s"u8, curCtx.P, go122.EventString(ev.typ)));
    }
    pState.Value.status = go122.ProcSyscall;
    // Validate the P sequence number on the event and advance it.
    //
    // We have a P sequence number for what is supposed to be a goroutine event
    // so that we can correctly model P stealing. Without this sequence number here,
    // the syscall from which a ProcSteal event is stealing can be ambiguous in the
    // face of broken timestamps. See the go122-syscall-steal-proc-ambiguous test for
    // more details.
    //
    // Note that because this sequence number only exists as a tool for disambiguation,
    // we can enforce that we have the right sequence number at this point; we don't need
    // to back off and see if any other events will advance. This is a running P.
    var pSeq = makeSeq(gen, ev.args[0]);
    if (!pSeq.succeeds((~pState).seq)) {
        return (curCtx, false, fmt.Errorf("failed to advance %s: can't make sequence: %s -> %s"u8, go122.EventString(ev.typ), (~pState).seq, pSeq));
    }
    pState.Value.seq = pSeq;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoSyscallEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // This event is always advance-able because it happens on the same
    // thread that EvGoSyscallStart happened, and the goroutine can't leave
    // that thread until its done.
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var (state, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for goroutine (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.G));
    }
    if ((~state).status != go122.GoSyscall) {
        return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %s"u8, go122.EventString(ev.typ), GoRunning));
    }
    state.Value.status = go122.GoRunning;
    // Transfer the P back to running from syscall.
    (var pState, ok) = o.pStates[curCtx.P, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("uninitialized proc %d found during %s"u8, curCtx.P, go122.EventString(ev.typ)));
    }
    if ((~pState).status != go122.ProcSyscall) {
        return (curCtx, false, fmt.Errorf("expected proc %d in state %v, but got %v instead"u8, curCtx.P, go122.ProcSyscall, (~pState).status));
    }
    pState.Value.status = go122.ProcRunning;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoSyscallEndBlocked(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // This event becomes advanceable when its P is not in a syscall state
    // (lack of a P altogether is also acceptable for advancing).
    // The transfer out of ProcSyscall can happen either voluntarily via
    // ProcStop or involuntarily via ProcSteal. We may also acquire a new P
    // before we get here (after the transfer out) but that's OK: that new
    // P won't be in the ProcSyscall state anymore.
    //
    // Basically: while we have a preemptible P, don't advance, because we
    // *know* from the event that we're going to lose it at some point during
    // the syscall. We shouldn't advance until that happens.
    if (curCtx.P != NoProc) {
        var (pState, okΔ1) = o.pStates[curCtx.P, ꟷ];
        if (!okΔ1) {
            return (curCtx, false, fmt.Errorf("uninitialized proc %d found during %s"u8, curCtx.P, go122.EventString(ev.typ)));
        }
        if ((~pState).status == go122.ProcSyscall) {
            return (curCtx, false, default!);
        }
    }
    // As mentioned above, we may have a P here if we ProcStart
    // before this event.
    {
        var err = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MayHave, Goroutine: @event.MustHave)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var (state, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for goroutine (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.G));
    }
    if ((~state).status != go122.GoSyscall) {
        return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %s"u8, go122.EventString(ev.typ), GoRunning));
    }
    var newCtx = curCtx;
    newCtx.G = NoGoroutine;
    state.Value.status = go122.GoRunnable;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoCreateSyscall(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // This event indicates that a goroutine is effectively
    // being created out of a cgo callback. Such a goroutine
    // is 'created' in the syscall state.
    {
        var err = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MayHave, Goroutine: @event.MustNotHave)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    // This goroutine is effectively being created. Add a state for it.
    ref var newgid = ref heap<GoID>(out var Ꮡnewgid);
    newgid = ((GoID)(int64)ev.args[0]);
    {
        var (_, ok) = o.gStates[newgid, ꟷ]; if (ok) {
            return (curCtx, false, fmt.Errorf("tried to create goroutine (%v) in syscall that already exists"u8, newgid));
        }
    }
    o.gStates[newgid] = Ꮡ(new gState(id: newgid, status: go122.GoSyscall, seq: makeSeq(gen, 0)));
    // Goroutine is executing. Bind it to the context.
    var newCtx = curCtx;
    newCtx.G = newgid;
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoDestroySyscall(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // This event indicates that a goroutine created for a
    // cgo callback is disappearing, either because the callback
    // ending or the C thread that called it is being destroyed.
    //
    // Also, treat this as if we lost our P too.
    // The thread ID may be reused by the platform and we'll get
    // really confused if we try to steal the P is this is running
    // with later. The new M with the same ID could even try to
    // steal back this P from itself!
    //
    // The runtime is careful to make sure that any GoCreateSyscall
    // event will enter the runtime emitting events for reacquiring a P.
    //
    // Note: we might have a P here. The P might not be released
    // eagerly by the runtime, and it might get stolen back later
    // (or never again, if the program is going to exit).
    {
        var err = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MayHave, Goroutine: @event.MustHave)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    // Check to make sure the goroutine exists in the right state.
    var (state, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("event %s for goroutine (%v) that doesn't exist"u8, go122.EventString(ev.typ), curCtx.G));
    }
    if ((~state).status != go122.GoSyscall) {
        return (curCtx, false, fmt.Errorf("%s event for goroutine that's not %v"u8, go122.EventString(ev.typ), GoSyscall));
    }
    // This goroutine is exiting itself.
    delete(o.gStates, curCtx.G);
    var newCtx = curCtx;
    newCtx.G = NoGoroutine;
    // If we have a proc, then we're dissociating from it now. See the comment at the top of the case.
    if (curCtx.P != NoProc) {
        var (pState, okΔ1) = o.pStates[curCtx.P, ꟷ];
        if (!okΔ1) {
            return (curCtx, false, fmt.Errorf("found invalid proc %d during %s"u8, curCtx.P, go122.EventString(ev.typ)));
        }
        if ((~pState).status != go122.ProcSyscall) {
            return (curCtx, false, fmt.Errorf("proc %d in unexpected state %s during %s"u8, curCtx.P, (~pState).status, go122.EventString(ev.typ)));
        }
        // See the go122-create-syscall-reuse-thread-id test case for more details.
        pState.Value.status = go122.ProcSyscallAbandoned;
        newCtx.P = NoProc;
        // Queue an extra self-ProcSteal event.
        var extra = makeEvent(Ꮡevt, curCtx, go122.EvProcSteal, ev.time, (uint64)(int64)curCtx.P);
        extra.@base.extra(version.Go122)[0] = (uint64)(uint8)go122.ProcSyscall;
        o.queue.push(extra);
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (newCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceUserTaskBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();
    ref var evt = ref Ꮡevt.DerefOrNull();

    // Handle tasks. Tasks are interesting because:
    // - There's no Begin event required to reference a task.
    // - End for a particular task ID can appear multiple times.
    // As a result, there's very little to validate. The only
    // thing we have to be sure of is that a task didn't begin
    // after it had already begun. Task IDs are allowed to be
    // reused, so we don't care about a Begin after an End.
    var id = ((TaskID)ev.args[0]);
    {
        var (_, okΔ1) = o.activeTasks[id, ꟷ]; if (okΔ1) {
            return (curCtx, false, fmt.Errorf("task ID conflict: %d"u8, id));
        }
    }
    // Get the parent ID, but don't validate it. There's no guarantee
    // we actually have information on whether it's active.
    var parentID = ((TaskID)ev.args[1]);
    if (parentID == BackgroundTask) {
        // Note: a value of 0 here actually means no parent, *not* the
        // background task. Automatic background task attachment only
        // applies to regions.
        parentID = NoTask;
        ev.args[1] = (uint64)NoTask;
    }
    // Validate the name and record it. We'll need to pass it through to
    // EvUserTaskEnd.
    var nameID = ((stringID)ev.args[2]);
    var (name, ok) = evt.strings.get(nameID);
    if (!ok) {
        return (curCtx, false, fmt.Errorf("invalid string ID %v for %v event"u8, nameID, ev.typ));
    }
    o.activeTasks[id] = new taskState(name: name, parentID: parentID);
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceUserTaskEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();
    ref var evt = ref Ꮡevt.DerefOrNull();

    var id = ((TaskID)ev.args[0]);
    {
        var (ts, ok) = o.activeTasks[id, ꟷ]; if (ok){
            // Smuggle the task info. This may happen in a different generation,
            // which may not have the name in its string table. Add it to the extra
            // strings table so we can look it up later.
            ev.extra(version.Go122)[0] = (uint64)ts.parentID;
            ev.extra(version.Go122)[1] = (uint64)evt.addExtraString(ts.name);
            delete(o.activeTasks, id);
        } else {
            // Explicitly clear the task info.
            ev.extra(version.Go122)[0] = (uint64)NoTask;
            ev.extra(version.Go122)[1] = (uint64)evt.addExtraString(""u8);
        }
    }
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceUserRegionBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();
    ref var evt = ref Ꮡevt.DerefOrNull();

    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var tid = ((TaskID)ev.args[0]);
    var nameID = ((stringID)ev.args[1]);
    var (name, ok) = evt.strings.get(nameID);
    if (!ok) {
        return (curCtx, false, fmt.Errorf("invalid string ID %v for %v event"u8, nameID, ev.typ));
    }
    (var gState, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("encountered EvUserRegionBegin without known state for current goroutine %d"u8, curCtx.G));
    }
    {
        var err = gState.beginRegion(new userRegion(tid, name)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceUserRegionEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();
    ref var evt = ref Ꮡevt.DerefOrNull();

    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var tid = ((TaskID)ev.args[0]);
    var nameID = ((stringID)ev.args[1]);
    var (name, ok) = evt.strings.get(nameID);
    if (!ok) {
        return (curCtx, false, fmt.Errorf("invalid string ID %v for %v event"u8, nameID, ev.typ));
    }
    (var gState, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("encountered EvUserRegionEnd without known state for current goroutine %d"u8, curCtx.G));
    }
    {
        var err = gState.endRegion(new userRegion(tid, name)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

// Handle the GC mark phase.
//
// We have sequence numbers for both start and end because they
// can happen on completely different threads. We want an explicit
// partial order edge between start and end here, otherwise we're
// relying entirely on timestamps to make sure we don't advance a
// GCEnd for a _different_ GC cycle if timestamps are wildly broken.
[GoRecv] internal static (schedCtx, bool, error) advanceGCActive(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var seq = ev.args[0];
    if (gen == o.initialGen) {
        if (o.gcState != gcUndetermined) {
            return (curCtx, false, fmt.Errorf("GCActive in the first generation isn't first GC event"u8));
        }
        o.gcSeq = seq;
        o.gcState = gcRunning;
        o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
        return (curCtx, true, default!);
    }
    if (seq != o.gcSeq + 1) {
        // This is not the right GC cycle.
        return (curCtx, false, default!);
    }
    if (o.gcState != gcRunning) {
        return (curCtx, false, fmt.Errorf("encountered GCActive while GC was not in progress"u8));
    }
    o.gcSeq = seq;
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGCBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var seq = ev.args[0];
    if (o.gcState == gcUndetermined) {
        o.gcSeq = seq;
        o.gcState = gcRunning;
        o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
        return (curCtx, true, default!);
    }
    if (seq != o.gcSeq + 1) {
        // This is not the right GC cycle.
        return (curCtx, false, default!);
    }
    if (o.gcState == gcRunning) {
        return (curCtx, false, fmt.Errorf("encountered GCBegin while GC was already in progress"u8));
    }
    o.gcSeq = seq;
    o.gcState = gcRunning;
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGCEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var seq = ev.args[0];
    if (seq != o.gcSeq + 1) {
        // This is not the right GC cycle.
        return (curCtx, false, default!);
    }
    if (o.gcState == gcNotRunning) {
        return (curCtx, false, fmt.Errorf("encountered GCEnd when GC was not in progress"u8));
    }
    if (o.gcState == gcUndetermined) {
        return (curCtx, false, fmt.Errorf("encountered GCEnd when GC was in an undetermined state"u8));
    }
    o.gcSeq = seq;
    o.gcState = gcNotRunning;
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceAnnotation(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Handle simple instantaneous events that require a G.
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceHeapMetric(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Handle allocation metrics, which don't require a G.
    {
        var err = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustHave, Goroutine: @event.MayHave)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGCSweepBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Handle sweep, which is bound to a P and doesn't require a G.
    {
        var err = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustHave, Goroutine: @event.MayHave)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    {
        var err = o.pStates[curCtx.P].of(pState.ᏑrangeState).beginRange(makeRangeType(ev.typ, 0)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGCSweepActive(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var pid = ((ProcID)(int64)ev.args[0]);
    // N.B. In practice Ps can't block while they're sweeping, so this can only
    // ever reference curCtx.P. However, be lenient about this like we are with
    // GCMarkAssistActive; there's no reason the runtime couldn't change to block
    // in the middle of a sweep.
    var (pState, ok) = o.pStates[pid, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("encountered GCSweepActive for unknown proc %d"u8, pid));
    }
    {
        var err = pState.of(trace_package.pState.ᏑrangeState).activeRange(makeRangeType(ev.typ, 0), gen == o.initialGen); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGCSweepEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    {
        var errΔ1 = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MustHave, Goroutine: @event.MayHave)); if (errΔ1 != default!) {
            return (curCtx, false, errΔ1);
        }
    }
    var (_, err) = o.pStates[curCtx.P].of(pState.ᏑrangeState).endRange(ev.typ);
    if (err != default!) {
        return (curCtx, false, err);
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoRangeBegin(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Handle special goroutine-bound event ranges.
    {
        var err = validateCtx(curCtx, @event.UserGoReqs); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    var desc = ((stringID)0);
    if (ev.typ == go122.EvSTWBegin) {
        desc = ((stringID)ev.args[0]);
    }
    var (gState, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("encountered event of type %d without known state for current goroutine %d"u8, ev.typ, curCtx.G));
    }
    {
        var err = gState.of(trace_package.gState.ᏑrangeState).beginRange(makeRangeType(ev.typ, desc)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoRangeActive(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    var gid = ((GoID)(int64)ev.args[0]);
    // N.B. Like GoStatus, this can happen at any time, because it can
    // reference a non-running goroutine. Don't check anything about the
    // current scheduler context.
    var (gState, ok) = o.gStates[gid, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("uninitialized goroutine %d found during %s"u8, gid, go122.EventString(ev.typ)));
    }
    {
        var err = gState.of(trace_package.gState.ᏑrangeState).activeRange(makeRangeType(ev.typ, 0), gen == o.initialGen); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceGoRangeEnd(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    {
        var errΔ1 = validateCtx(curCtx, @event.UserGoReqs); if (errΔ1 != default!) {
            return (curCtx, false, errΔ1);
        }
    }
    var (gState, ok) = o.gStates[curCtx.G, ꟷ];
    if (!ok) {
        return (curCtx, false, fmt.Errorf("encountered event of type %d without known state for current goroutine %d"u8, ev.typ, curCtx.G));
    }
    var (desc, err) = gState.of(trace_package.gState.ᏑrangeState).endRange(ev.typ);
    if (err != default!) {
        return (curCtx, false, err);
    }
    if (ev.typ == go122.EvSTWEnd) {
        // Smuggle the kind into the event.
        // Don't use ev.extra here so we have symmetry with STWBegin.
        ev.args[0] = (uint64)desc;
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

[GoRecv] internal static (schedCtx, bool, error) advanceAllocFree(this ref ordering o, ж<baseEvent> Ꮡev, ж<evTable> Ꮡevt, ThreadID m, uint64 gen, schedCtx curCtx) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // Handle simple instantaneous events that may or may not have a P.
    {
        var err = validateCtx(curCtx, new @event.SchedReqs(Thread: @event.MustHave, Proc: @event.MayHave, Goroutine: @event.MayHave)); if (err != default!) {
            return (curCtx, false, err);
        }
    }
    o.queue.push(new ΔEvent(table: Ꮡevt, ctx: curCtx, @base: ev.ΔClone()));
    return (curCtx, true, default!);
}

// Next returns the next event in the ordering.
[GoRecv] internal static (ΔEvent, bool) Next(this ref ordering o) {
    return o.queue.pop();
}

// schedCtx represents the scheduling resources associated with an event.
[GoType] partial struct schedCtx {
    public GoID G;
    public ProcID P;
    public ThreadID M;
}

// validateCtx ensures that ctx conforms to some reqs, returning an error if
// it doesn't.
internal static error validateCtx(schedCtx ctx, @event.SchedReqs reqs) {
    // Check thread requirements.
    if (reqs.Thread == @event.MustHave && ctx.M == NoThread){
        return fmt.Errorf("expected a thread but didn't have one"u8);
    } else 
    if (reqs.Thread == @event.MustNotHave && ctx.M != NoThread) {
        return fmt.Errorf("expected no thread but had one"u8);
    }
    // Check proc requirements.
    if (reqs.Proc == @event.MustHave && ctx.P == NoProc){
        return fmt.Errorf("expected a proc but didn't have one"u8);
    } else 
    if (reqs.Proc == @event.MustNotHave && ctx.P != NoProc) {
        return fmt.Errorf("expected no proc but had one"u8);
    }
    // Check goroutine requirements.
    if (reqs.Goroutine == @event.MustHave && ctx.G == NoGoroutine){
        return fmt.Errorf("expected a goroutine but didn't have one"u8);
    } else 
    if (reqs.Goroutine == @event.MustNotHave && ctx.G != NoGoroutine) {
        return fmt.Errorf("expected no goroutine but had one"u8);
    }
    return default!;
}

[GoType("num:uint8")] partial struct gcState;

internal static gcState gcUndetermined => /* iota */ 0;
internal static gcState gcNotRunning => 1;
internal static gcState gcRunning => 2;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string undeterminedˢ = "Undetermined"u8;
internal static readonly @string notRunningˢ = "NotRunning"u8;
internal static readonly @string runningˢ = "Running"u8;
internal static readonly @string badˢ = "Bad"u8;

// String returns a human-readable string for the GC state.
internal static @string String(this gcState s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == gcUndetermined) {
        return undeterminedˢ;
    }
    if (exprᴛ1 == gcNotRunning) {
        return notRunningˢ;
    }
    if (exprᴛ1 == gcRunning) {
        return runningˢ;
    }

    return badˢ;
}

// userRegion represents a unique user region when attached to some gState.
[GoType] partial struct userRegion {
    // name must be a resolved string because the string ID for the same
    // string may change across generations, but we care about checking
    // the value itself.
    internal TaskID taskID;
    internal @string name;
}

// rangeType is a way to classify special ranges of time.
//
// These typically correspond 1:1 with "Begin" events, but
// they may have an optional subtype that describes the range
// in more detail.
[GoType] partial struct rangeType {
    internal @event.Type typ; // "Begin" event.
    internal stringID desc;   // Optional subtype.
}

// makeRangeType constructs a new rangeType.
internal static rangeType makeRangeType(@event.Type typ, stringID desc) {
    {
        var styp = go122.Specs()[typ].StartEv; if (styp != go122.EvNone) {
            typ = styp;
        }
    }
    return new rangeType(typ, desc);
}

// gState is the state of a goroutine at a point in the trace.
[GoType] partial struct gState {
    internal GoID id;
    internal go122.GoStatus status;
    internal seqCounter seq;
    // regions are the active user regions for this goroutine.
    internal slice<userRegion> regions;
    // rangeState is the state of special time ranges bound to this goroutine.
    internal partial ref rangeState rangeState { get; }
}

// beginRegion starts a user region on the goroutine.
[GoRecv] internal static error beginRegion(this ref gState s, userRegion r) {
    s.regions = append(s.regions, r);
    return default!;
}

// endRegion ends a user region on the goroutine.
[GoRecv] internal static error endRegion(this ref gState s, userRegion r) {
    if (len(s.regions) == 0) {
        // We do not know about regions that began before tracing started.
        return default!;
    }
    {
        var next = s.regions[len(s.regions) - 1]; if (next != r) {
            return fmt.Errorf("misuse of region in goroutine %v: region end %v when the inner-most active region start event is %v"u8, s.id, r, next);
        }
    }
    s.regions = s.regions.slice(0, len(s.regions) - 1);
    return default!;
}

// pState is the state of a proc at a point in the trace.
[GoType] partial struct pState {
    internal ProcID id;
    internal go122.ProcStatus status;
    internal seqCounter seq;
    // rangeState is the state of special time ranges bound to this proc.
    internal partial ref rangeState rangeState { get; }
}

// mState is the state of a thread at a point in the trace.
[GoType] partial struct mState {
    internal GoID g;   // Goroutine bound to this M. (The goroutine's state is Executing.)
    internal ProcID p; // Proc bound to this M. (The proc's state is Executing.)
}

// rangeState represents the state of special time ranges.
[GoType] partial struct rangeState {
    // inFlight contains the rangeTypes of any ranges bound to a resource.
    internal slice<rangeType> inFlight;
}

// beginRange begins a special range in time on the goroutine.
//
// Returns an error if the range is already in progress.
[GoRecv] internal static error beginRange(this ref rangeState s, rangeType typ) {
    if (s.hasRange(typ)) {
        return fmt.Errorf("discovered event already in-flight for when starting event %v"u8, go122.Specs()[typ.typ].Name);
    }
    s.inFlight = append(s.inFlight, typ);
    return default!;
}

// activeRange marks special range in time on the goroutine as active in the
// initial generation, or confirms that it is indeed active in later generations.
[GoRecv] internal static error activeRange(this ref rangeState s, rangeType typ, bool isInitialGen) {
    if (isInitialGen){
        if (s.hasRange(typ)) {
            return fmt.Errorf("found named active range already in first gen: %v"u8, typ);
        }
        s.inFlight = append(s.inFlight, typ);
    } else 
    if (!s.hasRange(typ)) {
        return fmt.Errorf("resource is missing active range: %v %v"u8, go122.Specs()[typ.typ].Name, s.inFlight);
    }
    return default!;
}

// hasRange returns true if a special time range on the goroutine as in progress.
[GoRecv] internal static bool hasRange(this ref rangeState s, rangeType typ) {
    foreach (var (_, ftyp) in s.inFlight) {
        if (ftyp == typ) {
            return true;
        }
    }
    return false;
}

// endRange ends a special range in time on the goroutine.
//
// This must line up with the start event type  of the range the goroutine is currently in.
[GoRecv] internal static (stringID, error) endRange(this ref rangeState s, @event.Type typ) {
    var st = go122.Specs()[typ].StartEv;
    nint idx = -1;
    foreach (var (i, r) in s.inFlight) {
        if (r.typ == st) {
            idx = i;
            break;
        }
    }
    if (idx < 0) {
        return (0, fmt.Errorf("tried to end event %v, but not in-flight"u8, go122.Specs()[st].Name));
    }
    // Swap remove.
    var desc = s.inFlight[idx].desc;
    (s.inFlight[idx], s.inFlight[len(s.inFlight) - 1]) = (s.inFlight[len(s.inFlight) - 1], s.inFlight[idx]);
    s.inFlight = s.inFlight.slice(0, len(s.inFlight) - 1);
    return (desc, default!);
}

// seqCounter represents a global sequence counter for a resource.
[GoType] partial struct seqCounter {
    internal uint64 gen; // The generation for the local sequence counter seq.
    internal uint64 seq; // The sequence number local to the generation.
}

// makeSeq creates a new seqCounter.
internal static seqCounter makeSeq(uint64 gen, uint64 seq) {
    return new seqCounter(gen: gen, seq: seq);
}

// succeeds returns true if a is the immediate successor of b.
internal static bool succeeds(this seqCounter a, seqCounter b) {
    return a.gen == b.gen && a.seq == b.seq + 1;
}

// String returns a debug string representation of the seqCounter.
internal static @string String(this seqCounter c) {
    return fmt.Sprintf("%d (gen=%d)"u8, c.seq, c.gen);
}

internal static @string dumpOrdering(ref ordering order) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    foreach (var (id, state) in order.gStates) {
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "G %d [status=%s seq=%s]\n"u8, id, (~state).status, (~state).seq);
    }
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb));
    foreach (var (id, state) in order.pStates) {
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "P %d [status=%s seq=%s]\n"u8, id, (~state).status, (~state).seq);
    }
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb));
    foreach (var (id, state) in order.mStates) {
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "M %d [g=%d p=%d]\n"u8, id, (~state).g, (~state).p);
    }
    fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb));
    fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "GC %d %s\n"u8, order.gcSeq, order.gcState);
    return sb.String();
}

// taskState represents an active task.
[GoType] partial struct taskState {
    // name is the type of the active task.
    internal @string name;
    // parentID is the parent ID of the active task.
    internal TaskID parentID;
}

// queue implements a growable ring buffer with a queue API.
[GoType] partial struct queue<T> {
    internal nint start, end;
    internal slice<T> buf;
}

// push adds a new event to the back of the queue.
[GoRecv] internal static void push<T>(this ref queue<T> q, T value) {
    if (q.end - q.start == len(q.buf)) {
        q.grow();
    }
    q.buf[q.end % len(q.buf)] = value;
    q.end++;
}

// grow increases the size of the queue.
[GoRecv] internal static void grow<T>(this ref queue<T> q) {
    if (len(q.buf) == 0) {
        q.buf = new slice<T>(2);
        return;
    }
    // Create new buf and copy data over.
    var newBuf = new slice<T>(len(q.buf) * 2);
    nint pivot = q.start % len(q.buf);
    var (first, last) = (q.buf.slice(pivot), q.buf.slice(0, pivot));
    copy(newBuf.slice(0, len(first)), first);
    copy(newBuf.slice(len(first)), last);
    // Update the queue state.
    q.start = 0;
    q.end = len(q.buf);
    q.buf = newBuf;
}

// pop removes an event from the front of the queue. If the
// queue is empty, it returns an EventBad event.
[GoRecv] internal static (T, bool) pop<T>(this ref queue<T> q) {
    if (q.end - q.start == 0) {
        return (@new<T>().ValueSlot, false);
    }
    var elem = Ꮡ(q.buf, q.start % len(q.buf));
    var value = elem.ValueSlot;
    elem.ValueSlot = @new<T>().ValueSlot; // Clear the entry before returning, so we don't hold onto old tables.
    q.start++;
    return (value, true);
}

// makeEvent creates an Event from the provided information.
//
// It's just a convenience function; it's always OK to construct
// an Event manually if this isn't quite the right way to express
// the contents of the event.
internal static ΔEvent makeEvent(ж<evTable> Ꮡtable, schedCtx ctx, @event.Type typ, ΔTime time, params ꓸꓸꓸuint64 argsʗp) {
    var args = argsʗp.sslice();

    var ev = new ΔEvent(
        table: Ꮡtable,
        ctx: ctx,
        @base: new baseEvent(
            typ: typ,
            time: time
        )
    );
    copy(ev.@base.args[..], args);
    return ev.ΔClone();
}

} // end trace_package
