// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux && (amd64 || arm64 || arm64be || ppc64 || ppc64le || loong64 || s390x)
namespace go;

using cpu = @internal.cpu_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

//go:noescape
internal static partial nint vgetrandom1(ж<byte> buf, uintptr length, uint32 flags, uintptr state, uintptr stateSize);


[GoType("dyn")] partial struct vgetrandomAllocᴛ1 {
    internal slice<uintptr> states;
    internal mutex statesLock;
    internal uintptr stateSize;
    internal int32 mmapProt;
    internal int32 mmapFlags;
}
internal static ж<vgetrandomAllocᴛ1> ᏑvgetrandomAlloc = new StandardBox<vgetrandomAllocᴛ1>(new vgetrandomAllocᴛ1());
internal static ref vgetrandomAllocᴛ1 vgetrandomAlloc => ref ᏑvgetrandomAlloc.Value;

[GoType("dyn")] internal partial struct vgetrandomInit_params {
    public uint32 SizeOfOpaqueState;
    public uint32 MmapProt;
    public uint32 MmapFlags;
    internal array<uint32> reserved = new(13);
}

internal static void vgetrandomInit() {
    if (vdsoGetrandomSym == 0) {
        return;
    }
    ref var @params = ref heap(new vgetrandomInit_params(), out var Ꮡparams);
    if (vgetrandom1(nil, 0, 0, (uintptr)Ꮡparams, ~(uintptr)0) != 0) {
        return;
    }
    vgetrandomAlloc.stateSize = (uintptr)@params.SizeOfOpaqueState;
    vgetrandomAlloc.mmapProt = (int32)@params.MmapProt;
    vgetrandomAlloc.mmapFlags = (int32)@params.MmapFlags;
    lockInit(ᏑvgetrandomAlloc.of(vgetrandomAllocᴛ1.ᏑstatesLock), lockRankLeafRank);
}

internal static uintptr vgetrandomGetState() {
    @lock(ᏑvgetrandomAlloc.of(vgetrandomAllocᴛ1.ᏑstatesLock));
    if (len(vgetrandomAlloc.states) == 0) {
        var num = (uintptr)ncpu; // Just a reasonable size hint to start.
        var stateSizeCacheAligned = (uintptr)((vgetrandomAlloc.stateSize + cpu.CacheLineSize - 1) & ~(cpu.CacheLineSize - 1));
        var allocSize = (uintptr)((num * stateSizeCacheAligned + physPageSize - 1) & ~(physPageSize - 1));
        num = (physPageSize / stateSizeCacheAligned) * (allocSize / physPageSize);
        var (Δp, err) = mmap(nil, allocSize, vgetrandomAlloc.mmapProt, vgetrandomAlloc.mmapFlags, -1, 0);
        if (err != 0) {
            unlock(ᏑvgetrandomAlloc.of(vgetrandomAllocᴛ1.ᏑstatesLock));
            return 0;
        }
        var newBlock = (uintptr)Δp;
        if (vgetrandomAlloc.states == default!) {
            vgetrandomAlloc.states = new slice<uintptr>(0, (nint)(num));
        }
        for (var i = (uintptr)0; i < num; i++) {
            if (((uintptr)(newBlock & (physPageSize - 1))) + vgetrandomAlloc.stateSize > physPageSize) {
                newBlock = (uintptr)((newBlock + physPageSize - 1) & ~(physPageSize - 1));
            }
            vgetrandomAlloc.states = append(vgetrandomAlloc.states, newBlock);
            newBlock += stateSizeCacheAligned;
        }
    }
    var state = vgetrandomAlloc.states[len(vgetrandomAlloc.states) - 1];
    vgetrandomAlloc.states = vgetrandomAlloc.states.slice(0, len(vgetrandomAlloc.states) - 1);
    unlock(ᏑvgetrandomAlloc.of(vgetrandomAllocᴛ1.ᏑstatesLock));
    return state;
}

// Free vgetrandom state from the M (if any) prior to destroying the M.
//
// This may allocate, so it must have a P.
internal static void vgetrandomDestroy(ref m mp) {
    if (mp.vgetrandomState == 0) {
        return;
    }
    @lock(ᏑvgetrandomAlloc.of(vgetrandomAllocᴛ1.ᏑstatesLock));
    vgetrandomAlloc.states = append(vgetrandomAlloc.states, mp.vgetrandomState);
    unlock(ᏑvgetrandomAlloc.of(vgetrandomAllocᴛ1.ᏑstatesLock));
}

// This is exported for use in internal/syscall/unix as well as x/sys/unix.
//
//go:linkname vgetrandom
public static (nint ret, bool supported) vgetrandom(slice<byte> Δp, uint32 flags) {
    if (vgetrandomAlloc.stateSize == 0) {
        return (-1, false);
    }
    // We use getg().m instead of acquirem() here, because always taking
    // the lock is slightly more expensive than not always taking the lock.
    // However, we *do* require that m doesn't migrate elsewhere during the
    // execution of the vDSO. So, we exploit two details:
    //   1) Asynchronous preemption is aborted when PC is in the runtime.
    //   2) Most of the time, this function only calls vgetrandom1(), which
    //      does not have a preamble that synchronously preempts.
    // We do need to take the lock when getting a new state for m, but this
    // is very much the slow path, in the sense that it only ever happens
    // once over the entire lifetime of an m. So, a simple getg().m suffices.
    var mp = getg().Value.m;
    if ((~mp).vgetrandomState == 0) {
        mp.Value.locks++;
        var state = vgetrandomGetState();
        mp.Value.locks--;
        if (state == 0) {
            return (-1, false);
        }
        mp.Value.vgetrandomState = state;
    }
    return (vgetrandom1(@unsafe.SliceData(Δp), (uintptr)len(Δp), flags, (~mp).vgetrandomState, vgetrandomAlloc.stateSize), true);
}

} // end runtime_package
