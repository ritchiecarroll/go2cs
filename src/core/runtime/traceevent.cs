// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace event writing API for trace2runtime.go.
namespace go;

using abi = @internal.abi_package;
using sys = @internal.runtime.sys_package;
using @internal;
using @internal.runtime;
using ꓸꓸꓸtraceArg = Span<runtime_package.traceArg>;

partial class runtime_package {

partial struct traceEv /*num:uint8*/;

internal static traceEv traceEvNone => /* iota */ 0; // unused
internal static traceEv traceEvEventBatch => 1; // start of per-M batch of events [generation, M ID, timestamp, batch length]
internal static traceEv traceEvStacks => 2; // start of a section of the stack table [...traceEvStack]
internal static traceEv traceEvStack => 3; // stack table entry [ID, ...{PC, func string ID, file string ID, line #}]
internal static traceEv traceEvStrings => 4; // start of a section of the string dictionary [...traceEvString]
internal static traceEv traceEvString => 5; // string dictionary entry [ID, length, string]
internal static traceEv traceEvCPUSamples => 6; // start of a section of CPU samples [...traceEvCPUSample]
internal static traceEv traceEvCPUSample => 7; // CPU profiling sample [timestamp, M ID, P ID, goroutine ID, stack ID]
internal static traceEv traceEvFrequency => 8; // timestamp units per sec [freq]
internal static traceEv traceEvProcsChange => 9; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack ID]
internal static traceEv traceEvProcStart => 10; // start of P [timestamp, P ID, P seq]
internal static traceEv traceEvProcStop => 11; // stop of P [timestamp]
internal static traceEv traceEvProcSteal => 12; // P was stolen [timestamp, P ID, P seq, M ID]
internal static traceEv traceEvProcStatus => 13; // P status at the start of a generation [timestamp, P ID, status]
internal static traceEv traceEvGoCreate => 14; // goroutine creation [timestamp, new goroutine ID, new stack ID, stack ID]
internal static traceEv traceEvGoCreateSyscall => 15; // goroutine appears in syscall (cgo callback) [timestamp, new goroutine ID]
internal static traceEv traceEvGoStart => 16; // goroutine starts running [timestamp, goroutine ID, goroutine seq]
internal static traceEv traceEvGoDestroy => 17; // goroutine ends [timestamp]
internal static traceEv traceEvGoDestroySyscall => 18; // goroutine ends in syscall (cgo callback) [timestamp]
internal static traceEv traceEvGoStop => 19; // goroutine yields its time, but is runnable [timestamp, reason, stack ID]
internal static traceEv traceEvGoBlock => 20; // goroutine blocks [timestamp, reason, stack ID]
internal static traceEv traceEvGoUnblock => 21; // goroutine is unblocked [timestamp, goroutine ID, goroutine seq, stack ID]
internal static traceEv traceEvGoSyscallBegin => 22; // syscall enter [timestamp, P seq, stack ID]
internal static traceEv traceEvGoSyscallEnd => 23; // syscall exit [timestamp]
internal static traceEv traceEvGoSyscallEndBlocked => 24; // syscall exit and it blocked at some point [timestamp]
internal static traceEv traceEvGoStatus => 25; // goroutine status at the start of a generation [timestamp, goroutine ID, M ID, status]
internal static traceEv traceEvSTWBegin => 26; // STW start [timestamp, kind]
internal static traceEv traceEvSTWEnd => 27; // STW done [timestamp]
internal static traceEv traceEvGCActive => 28; // GC active [timestamp, seq]
internal static traceEv traceEvGCBegin => 29; // GC start [timestamp, seq, stack ID]
internal static traceEv traceEvGCEnd => 30; // GC done [timestamp, seq]
internal static traceEv traceEvGCSweepActive => 31; // GC sweep active [timestamp, P ID]
internal static traceEv traceEvGCSweepBegin => 32; // GC sweep start [timestamp, stack ID]
internal static traceEv traceEvGCSweepEnd => 33; // GC sweep done [timestamp, swept bytes, reclaimed bytes]
internal static traceEv traceEvGCMarkAssistActive => 34; // GC mark assist active [timestamp, goroutine ID]
internal static traceEv traceEvGCMarkAssistBegin => 35; // GC mark assist start [timestamp, stack ID]
internal static traceEv traceEvGCMarkAssistEnd => 36; // GC mark assist done [timestamp]
internal static traceEv traceEvHeapAlloc => 37; // gcController.heapLive change [timestamp, heap alloc in bytes]
internal static traceEv traceEvHeapGoal => 38; // gcController.heapGoal() change [timestamp, heap goal in bytes]
internal static traceEv traceEvGoLabel => 39; // apply string label to current running goroutine [timestamp, label string ID]
internal static traceEv traceEvUserTaskBegin => 40; // trace.NewTask [timestamp, internal task ID, internal parent task ID, name string ID, stack ID]
internal static traceEv traceEvUserTaskEnd => 41; // end of a task [timestamp, internal task ID, stack ID]
internal static traceEv traceEvUserRegionBegin => 42; // trace.{Start,With}Region [timestamp, internal task ID, name string ID, stack ID]
internal static traceEv traceEvUserRegionEnd => 43; // trace.{End,With}Region [timestamp, internal task ID, name string ID, stack ID]
internal static traceEv traceEvUserLog => 44; // trace.Log [timestamp, internal task ID, key string ID, stack, value string ID]
internal static traceEv traceEvGoSwitch => 45; // goroutine switch (coroswitch) [timestamp, goroutine ID, goroutine seq]
internal static traceEv traceEvGoSwitchDestroy => 46; // goroutine switch and destroy [timestamp, goroutine ID, goroutine seq]
internal static traceEv traceEvGoCreateBlocked => 47; // goroutine creation (starts blocked) [timestamp, new goroutine ID, new stack ID, stack ID]
internal static traceEv traceEvGoStatusStack => 48; // goroutine status at the start of a generation, with a stack [timestamp, goroutine ID, M ID, status, stack ID]
internal static traceEv traceEvExperimentalBatch => 49; // start of extra data [experiment ID, generation, M ID, timestamp, batch length, batch data...]

partial struct traceArg /*num:uint64*/;

// traceEventWriter is the high-level API for writing trace events.
//
// See the comment on traceWriter about style for more details as to why
// this type and its methods are structured the way they are.
partial struct traceEventWriter {
    internal traceLocker tl;
}

// eventWriter creates a new traceEventWriter. It is the main entrypoint for writing trace events.
//
// Before creating the event writer, this method will emit a status for the current goroutine
// or proc if it exists, and if it hasn't had its status emitted yet. goStatus and procStatus indicate
// what the status of goroutine or P should be immediately *before* the events that are about to
// be written using the eventWriter (if they exist). No status will be written if there's no active
// goroutine or P.
//
// Callers can elect to pass a constant value here if the status is clear (e.g. a goroutine must have
// been Runnable before a GoStart). Otherwise, callers can query the status of either the goroutine
// or P and pass the appropriate status.
//
// In this case, the default status should be traceGoBad or traceProcBad to help identify bugs sooner.
internal static traceEventWriter eventWriter(this traceLocker tl, traceGoStatus goStatus, traceProcStatus procStatus) {
    {
        var pp = (~tl.mp).p.ptr(); if (pp != nil && !pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen) && pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).acquireStatus(tl.gen)) {
            tl.writer().writeProcStatus((uint64)(~pp).id, procStatus, (~pp).trace.inSweep).end();
        }
    }
    {
        var gp = tl.mp.Value.curg; if (gp != nil && !gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen) && gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).acquireStatus(tl.gen)) {
            tl.writer().writeGoStatus((uint64)(~gp).goid, (int64)(~tl.mp).procid, goStatus, (~gp).inMarkAssist, 0).end();
        }
    }
    /* no stack */
    return new traceEventWriter(tl);
}

// event writes out a trace event.
internal static void @event(this traceEventWriter e, traceEv ev, params ꓸꓸꓸtraceArg argsʗp) {
    var args = argsʗp.sslice();

    e.tl.writer().@event(ev, args.ꓸꓸꓸ).end();
}

// stack takes a stack trace skipping the provided number of frames.
// It then returns a traceArg representing that stack which may be
// passed to write.
internal static traceArg stack(this traceLocker tl, nint skip) {
    return ((traceArg)traceStack(skip, nil, tl.gen));
}

// startPC takes a start PC for a goroutine and produces a unique
// stack ID for it.
//
// It then returns a traceArg representing that stack which may be
// passed to write.
internal static traceArg startPC(this traceLocker tl, uintptr pc) {
    // +PCQuantum because makeTraceFrame expects return PCs and subtracts PCQuantum.
    return ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstackTab, (ulong)(tl.gen % 2)).put(new uintptr[]{
        logicalStackSentinel,
        startPCForTrace(pc) + (uintptr)sys.PCQuantum
    }.slice()));
}

// string returns a traceArg representing s which may be passed to write.
// The string is assumed to be relatively short and popular, so it may be
// stored for a while in the string dictionary.
internal static traceArg @string(this traceLocker tl, @string s) {
    return ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(tl.gen % 2)).put(tl.gen, s));
}

// uniqueString returns a traceArg representing s which may be passed to write.
// The string is assumed to be unique or long, so it will be written out to
// the trace eagerly.
internal static traceArg uniqueString(this traceLocker tl, @string s) {
    return ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(tl.gen % 2)).emit(tl.gen, s));
}

// rtype returns a traceArg representing typ which may be passed to write.
internal static traceArg rtype(this traceLocker tl, ж<abi.Type> Ꮡtyp) {
    return ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑtypeTab, (ulong)(tl.gen % 2)).put(Ꮡtyp));
}

} // end runtime_package
