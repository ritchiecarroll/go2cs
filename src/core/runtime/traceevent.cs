// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace event writing API for trace2runtime.go.
namespace go;

using abi = @internal.abi_package;
using sys = @internal.runtime.sys_package;
using tracev2 = @internal.trace.tracev2_package;
using @internal;
using @internal.runtime;
using @internal.trace;
using ꓸꓸꓸtraceArg = Span<runtime_package.traceArg>;

partial class runtime_package {

// traceArg is a simple wrapper type to help ensure that arguments passed
// to traces are well-formed.
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
// In this case, the default status should be tracev2.GoBad or tracev2.ProcBad to help identify bugs sooner.
internal static traceEventWriter eventWriter(this traceLocker tl, tracev2.GoStatus goStatus, tracev2.ProcStatus procStatus) {
    {
        var pp = (~tl.mp).p.ptr(); if (pp != nil && !pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen) && pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).acquireStatus(tl.gen)) {
            tl.writer().writeProcStatus((uint64)(~pp).id, procStatus, (~pp).trace.inSweep).end();
        }
    }
    {
        var gp = tl.mp.Value.curg; if (gp != nil && !gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen) && gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).acquireStatus(tl.gen)) {
            tl.writer().writeGoStatus((~gp).goid, (int64)(~tl.mp).procid, goStatus, (~gp).inMarkAssist, 0 /* no stack */).end();
        }
    }
    return tl.rawEventWriter();
}

// rawEventWriter creates a new traceEventWriter without emitting any status events.
//
// It is the caller's responsibility to emit any status events, if necessary.
internal static traceEventWriter rawEventWriter(this traceLocker tl) {
    return new traceEventWriter(tl);
}

// event writes out a trace event.
internal static void @event(this traceEventWriter e, tracev2.EventType ev, params ꓸꓸꓸtraceArg argsʗp) {
    var args = argsʗp.sslice();

    e.tl.writer().@event(ev, args.ꓸꓸꓸ).end();
}

// stack takes a stack trace skipping the provided number of frames.
// It then returns a traceArg representing that stack which may be
// passed to write.
internal static traceArg stack(this traceLocker tl, nint skip) {
    return (traceArg)traceStack(skip, nil, ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstackTab, (ulong)(tl.gen % 2)));
}

// startPC takes a start PC for a goroutine and produces a unique
// stack ID for it.
//
// It then returns a traceArg representing that stack which may be
// passed to write.
internal static traceArg startPC(this traceLocker tl, uintptr pc) {
    // +PCQuantum because makeTraceFrame expects return PCs and subtracts PCQuantum.
    return (traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstackTab, (ulong)(tl.gen % 2)).put(slice<uintptr>([
        logicalStackSentinel,
        startPCForTrace(pc) + (uintptr)sys.PCQuantum
    ]));
}

// string returns a traceArg representing s which may be passed to write.
// The string is assumed to be relatively short and popular, so it may be
// stored for a while in the string dictionary.
internal static traceArg @string(this traceLocker tl, @string s) {
    return (traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(tl.gen % 2)).put(tl.gen, s);
}

// uniqueString returns a traceArg representing s which may be passed to write.
// The string is assumed to be unique or long, so it will be written out to
// the trace eagerly.
internal static traceArg uniqueString(this traceLocker tl, @string s) {
    return (traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(tl.gen % 2)).emit(tl.gen, s);
}

// rtype returns a traceArg representing typ which may be passed to write.
internal static traceArg rtype(this traceLocker tl, ж<abi.Type> typ) {
    return (traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑtypeTab, (ulong)(tl.gen % 2)).put(typ);
}

} // end runtime_package
