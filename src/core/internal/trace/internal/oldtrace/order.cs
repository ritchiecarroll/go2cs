// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace.@internal;

using errors = errors_package;

partial class oldtrace_package {

[GoType] partial struct orderEvent {
    internal Event ev;
    internal ж<proc> proc;
}

[GoType("num:nint")] partial struct gStatus;

[GoType] partial struct gState {
    internal uint64 seq;
    internal gStatus status;
}

internal static gStatus gDead => /* iota */ 0;
internal static gStatus gRunnable => 1;
internal static gStatus gRunning => 2;
internal static gStatus gWaiting => 3;
internal const uint64 unordered = /* ^uint64(0) */ 18446744073709551615;
internal const uint64 garbage = /* ^uint64(0) - 1 */ 18446744073709551614;
internal const uint64 noseq = /* ^uint64(0) */ 18446744073709551615;
internal const uint64 seqinc = /* ^uint64(0) - 1 */ 18446744073709551614;

// stateTransition returns goroutine state (sequence and status) when the event
// becomes ready for merging (init) and the goroutine state after the event (next).
internal static (uint64 g, gState init, gState next) stateTransition(ref Event ev) {
    uint64 g = default!;
    gState init = default!;
    gState next = default!;

    // Note that we have an explicit return in each case, as that produces slightly better code (tested on Go 1.19).
    var exprᴛ1 = ev.Type;
    if (exprᴛ1 == EvGoCreate) {
        g = ev.Args[0];
        init = new gState(0, gDead);
        next = new gState(1, gRunnable);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoWaiting || exprᴛ1 == EvGoInSyscall) {
        g = ev.G;
        init = new gState(1, gRunnable);
        next = new gState(2, gWaiting);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoStart || exprᴛ1 == EvGoStartLabel) {
        g = ev.G;
        init = new gState(ev.Args[1], gRunnable);
        next = new gState(ev.Args[1] + 1, gRunning);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoStartLocal) {
        g = ev.G;
        init = new gState( // noseq means that this event is ready for merging as soon as
 // frontier reaches it (EvGoStartLocal is emitted on the same P
 // as the corresponding EvGoCreate/EvGoUnblock, and thus the latter
 // is already merged).
 // seqinc is a stub for cases when event increments g sequence,
 // but since we don't know current seq we also don't know next seq.
noseq, gRunnable);
        next = new gState(seqinc, gRunning);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoBlock || exprᴛ1 == EvGoBlockSend || exprᴛ1 == EvGoBlockRecv || exprᴛ1 == EvGoBlockSelect || exprᴛ1 == EvGoBlockSync || exprᴛ1 == EvGoBlockCond || exprᴛ1 == EvGoBlockNet || exprᴛ1 == EvGoSleep || exprᴛ1 == EvGoSysBlock || exprᴛ1 == EvGoBlockGC) {
        g = ev.G;
        init = new gState(noseq, gRunning);
        next = new gState(noseq, gWaiting);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoSched || exprᴛ1 == EvGoPreempt) {
        g = ev.G;
        init = new gState(noseq, gRunning);
        next = new gState(noseq, gRunnable);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoUnblock || exprᴛ1 == EvGoSysExit) {
        g = ev.Args[0];
        init = new gState(ev.Args[1], gWaiting);
        next = new gState(ev.Args[1] + 1, gRunnable);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGoUnblockLocal || exprᴛ1 == EvGoSysExitLocal) {
        g = ev.Args[0];
        init = new gState(noseq, gWaiting);
        next = new gState(seqinc, gRunnable);
        return (g, init, next);
    }
    if (exprᴛ1 == EvGCStart) {
        g = garbage;
        init = new gState(ev.Args[0], gDead);
        next = new gState(ev.Args[0] + 1, gDead);
        return (g, init, next);
    }
    { /* default: */
        g = unordered;
        return (g, init, next);
    }

}

// no ordering requirements
internal static bool transitionReady(uint64 g, gState curr, gState init) {
    return g == unordered || (init.seq == noseq || init.seq == curr.seq) && init.status == curr.status;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string encounteredImpossibleˢ = "encountered impossible goroutine state transition"u8;

internal static error transition(map<uint64, gState> gs, uint64 g, gState init, gState next) {
    if (g == unordered) {
        return default!;
    }
    var curr = gs[g];
    if (!transitionReady(g, curr, init)) {
        // See comment near the call to transition, where we're building the frontier, for details on how this could
        // possibly happen.
        return errors.New(encounteredImpossibleˢ);
    }
    switch (next.seq) {
    case noseq: {
        next.seq = curr.seq;
        break;
    }
    case seqinc: {
        next.seq = curr.seq + 1;
        break;
    }}

    gs[g] = next;
    return default!;
}

[GoType("[]orderEvent")] partial struct orderEventList;

[GoRecv] internal static bool Less(this ref orderEventList l, nint i, nint j) {
    return (l)[i].ev.Ts < (l)[j].ev.Ts;
}

internal static void Push(this ж<orderEventList> Ꮡh, orderEvent x) {
    x = x.ΔClone();

    ref var h = ref Ꮡh.DerefOrNull();
    h = builtin.append(h, x.ΔClone());
    heapUp(Ꮡh, len(h) - 1);
}

internal static orderEvent Pop(this ж<orderEventList> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    nint n = len(h) - 1;
    ((h)[0], (h)[n]) = ((h)[n].ΔClone(), (h)[0].ΔClone());
    heapDown(Ꮡh, 0, n);
    var x = (h)[len(h) - 1].ΔClone();
    h = (h).slice(0, len(h) - 1);
    return x.ΔClone();
}

internal static void heapUp(ж<orderEventList> Ꮡh, nint j) {
    ref var h = ref Ꮡh.DerefOrNull();

    while (ᐧ) {
        nint i = (j - 1) / 2; // parent
        if (i == j || !h.Less(j, i)) {
            break;
        }
        ((h)[i], (h)[j]) = ((h)[j].ΔClone(), (h)[i].ΔClone());
        j = i;
    }
}

internal static bool heapDown(ж<orderEventList> Ꮡh, nint i0, nint n) {
    ref var h = ref Ꮡh.DerefOrNull();

    nint i = i0;
    while (ᐧ) {
        nint j1 = 2 * i + 1;
        if (j1 >= n || j1 < 0) {
            // j1 < 0 after int overflow
            break;
        }
        nint j = j1; // left child
        {
            nint j2 = j1 + 1; if (j2 < n && h.Less(j2, j1)) {
                j = j2; // = 2*i + 2  // right child
            }
        }
        if (!h.Less(j, i)) {
            break;
        }
        ((h)[i], (h)[j]) = ((h)[j].ΔClone(), (h)[i].ΔClone());
        i = j;
    }
    return i > i0;
}

} // end oldtrace_package
