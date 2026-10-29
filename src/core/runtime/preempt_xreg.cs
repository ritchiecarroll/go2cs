// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || loong64
// This provides common support for architectures that use extended register
// state in asynchronous preemption.
//
// While asynchronous preemption stores general-purpose (GP) registers on the
// preempted goroutine's own stack, extended register state can be used to save
// non-GP state off the stack. In particular, this is meant for large vector
// register files. Currently, we assume this contains only scalar data, though
// we could change this constraint by conservatively scanning this memory.
//
// For an architecture to support extended register state, it must provide a Go
// definition of an xRegState type for storing the state, and its asyncPreempt
// implementation must write this register state to p.xRegs.scratch.
namespace go;

using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// xRegState is long-lived extended register state. It is allocated off-heap and
// manually managed.
partial struct xRegState {
    internal sys.NotInHeap _; // Allocated from xRegAlloc
    internal xRegs regs;
}

// xRegPerG stores extended register state while a goroutine is asynchronously
// preempted. This is nil otherwise, so we can reuse a (likely small) pool of
// xRegState objects.
partial struct xRegPerG {
    internal ж<xRegState> state;
}

partial struct xRegPerP {
    // scratch temporary per-P space where [asyncPreempt] saves the register
    // state before entering Go. It's quickly copied to per-G state.
    internal xRegs scratch;
    // cache is a 1-element allocation cache of extended register state used by
    // asynchronous preemption. On entry to preemption, this is used as a simple
    // allocation cache. On exit from preemption, the G's xRegState is always
    // stored here where it can be restored, and later either freed or reused
    // for another preemption. On exit, this serves the dual purpose of
    // delay-freeing the allocated xRegState until after we've definitely
    // restored it.
    internal ж<xRegState> cache;
}

// xRegAlloc allocates xRegState objects.
partial struct xRegAllocᴛ1 /*dyn*/ {
    internal mutex @lock;
    internal fixalloc alloc;
}
internal static readonly ж<xRegAllocᴛ1> ᏑxRegAlloc = new StaticBox<xRegAllocᴛ1>(new xRegAllocᴛ1());
internal static ref xRegAllocᴛ1 xRegAlloc => ref ((StaticBox<xRegAllocᴛ1>)ᏑxRegAlloc).ValueSlot;

internal static void xRegInitAlloc() {
    lockInit(ref xRegAlloc.@lock, lockRankXRegAlloc);
    ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡalloc).init(/* unsafe.Sizeof(xRegState{}) */ (uintptr)2112, default, nil, Ꮡmemstats.of(mstats.Ꮡother_sys));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gpXRegStatePNilOnAsyncˢ = "gp.xRegState.p != nil on async preempt"u8;

// xRegSave saves the extended register state on this P to gp.
//
// This must run on the system stack because it assumes the P won't change.
//
//go:systemstack
internal static void xRegSave(ref g gp) {
    if (gp.xRegs.state != nil) {
        // Double preempt?
        @throw(gpXRegStatePNilOnAsyncˢ);
    }

    // Get the place to save the register state.
    ж<xRegState> dest = default;
    var pp = (~gp.m).p.ptr();
    if ((~pp).xRegs.cache != nil) {
        // Use the cached allocation.
        dest = pp.Value.xRegs.cache;
        pp.Value.xRegs.cache = default;
    } else {
        // Allocate a new save block.
        @lock(ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡlock));
        dest = (ж<xRegState>)(uintptr)(ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡalloc).alloc());
        unlock(ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡlock));
    }

    // Copy state saved in the scratchpad to dest.
    //
    // If we ever need to save less state (e.g., avoid saving vector registers
    // that aren't in use), we could have multiple allocation pools for
    // different size states and copy only the registers we need.
    dest.regs = pp.Value.xRegs.scratch.ΔClone();

    // Save on the G.
    gp.xRegs.state = dest;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gpXRegStatePNilOnReturnˢ = "gp.xRegState.p == nil on return from async preempt"u8;

// xRegRestore prepares the extended register state on gp to be restored.
//
// It moves the state to gp.m.p.xRegs.cache where [asyncPreempt] expects to find
// it. This means nothing else may use the cache between this call and the
// return to asyncPreempt. This is not quite symmetric with [xRegSave], which
// uses gp.m.p.xRegs.scratch. By using cache instead, we save a block copy.
//
// This is called with asyncPreempt on the stack and thus must not grow the
// stack.
//
//go:nosplit
internal static void xRegRestore(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (gp.xRegs.state == nil) {
        @throw(gpXRegStatePNilOnReturnˢ);
    }
    // If the P has a block cached on it, free that so we can replace it.
    var pp = (~gp.m).p.ptr();
    if ((~pp).xRegs.cache != nil) {
        // Don't grow the G stack.
        var ppʗ1 = pp;
        systemstack(() => {
            ppʗ1.of(runtime_package.Δp.ᏑxRegs).free();
        });
    }
    pp.Value.xRegs.cache = gp.xRegs.state;
    gp.xRegs.state = default;
}

internal static void free(this ref xRegPerP xRegs) {
    if (xRegs.cache != nil) {
        @lock(ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡlock));
        ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡalloc).free(@unsafe.Pointer.FromPinnedBox(xRegs.cache));
        xRegs.cache = default;
        unlock(ᏑxRegAlloc.of(xRegAllocᴛ1.Ꮡlock));
    }
}

} // end runtime_package
