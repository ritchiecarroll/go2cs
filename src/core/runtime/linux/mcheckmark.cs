// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// GC checkmarks
//
// In a concurrent garbage collector, one worries about failing to mark
// a live object due to mutations without write barriers or bugs in the
// collector implementation. As a sanity check, the GC has a 'checkmark'
// mode that retraverses the object graph with the world stopped, to make
// sure that everything that should be marked is marked.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using abi = @internal.abi_package;

partial class runtime_package {

// A checkmarksMap stores the GC marks in "checkmarks" mode. It is a
// per-arena bitmap with a bit for every word in the arena. The mark
// is stored on the bit corresponding to the first word of the marked
// allocation.
partial struct checkmarksMap {
    internal sys.NotInHeap _;
    internal array<uint8> b = new(heapArenaBytes / goarch.PtrSize / 8);
}

// If useCheckmark is true, marking of an object uses the checkmark
// bits instead of the standard mark bits.
internal static bool useCheckmark = false;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string outOfMemoryAllocatingˢ4 = "out of memory allocating checkmarks bitmap"u8;

// startCheckmarks prepares for the checkmarks phase.
//
// The world must be stopped.
internal static void startCheckmarks() {
    assertWorldStopped();

    // Clear all checkmarks.
    void clearCheckmarks(arenaIdx ai) {
        var arena = mheap_.arenas[ai.l1()].Value[ai.l2()];
        var bitmap = arena.checkmarks;

        if (bitmap == nil) {
            // Allocate bitmap on first use.
            bitmap = (ж<checkmarksMap>)(uintptr)(persistentalloc(/* unsafe.Sizeof(*bitmap) */ (uintptr)1048576, 0, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            if (bitmap == nil) {
                @throw(outOfMemoryAllocatingˢ4);
            }
            arena.checkmarks = bitmap;
        } else {
            // Otherwise clear the existing bitmap.
            builtin.clear(bitmap.b[..]);
        }
    }
    foreach (var (_, ai) in mheap_.heapArenas) {
        clearCheckmarks(ai);
    }
    foreach (var (_, ai) in mheap_.userArenaArenas) {
        clearCheckmarks(ai);
    }

    // Enable checkmarking.
    useCheckmark = true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcWorkNotFlushedˢ = "GC work not flushed"u8;

// endCheckmarks ends the checkmarks phase.
internal static void endCheckmarks() {
    if (!gcIsMarkDone()) {
        @throw(gcWorkNotFlushedˢ);
    }
    useCheckmark = false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string baseˢ = "base"u8;
internal static readonly @string objˢ = "obj"u8;
internal static readonly @string checkmarkFoundUnmarkedˢ = "checkmark found unmarked object"u8;

// setCheckmark throws if marking object is a checkmarks violation,
// and otherwise sets obj's checkmark. It returns true if obj was
// already checkmarked.
internal static bool setCheckmark(uintptr obj, uintptr @base, uintptr off, markBits mbits) {
    if (!mbits.isMarked()) {
        printlock();
        print((@string)"runtime: checkmarks found unexpected unmarked object obj="u8, (Δhex)(uint64)obj, (@string)"\n"u8);
        print((@string)"runtime: found obj at *("u8, (Δhex)(uint64)@base, (@string)"+"u8, (Δhex)(uint64)off, (@string)")\n"u8);

        // Dump the source (base) object
        gcDumpObject(baseˢ, @base, off);

        // Dump the object
        gcDumpObject(objˢ, obj, ~(uintptr)0);

        getg().Value.m.Value.traceback = 2;
        @throw(checkmarkFoundUnmarkedˢ);
    }
    var (bytep, mask) = getCheckmark(obj);
    if (bytep == nil) {
        return false;
    }
    if ((uint8)(atomic.Load8(bytep) & mask) != 0) {
        // Already checkmarked.
        return true;
    }
    atomic.Or8(bytep, mask);
    return false;
}

internal static (ж<byte> bytep, uint8 mask) getCheckmark(uintptr obj) {
    ж<byte> bytep = default;
    uint8 mask = default;

    arenaIdx ai = arenaIndex(obj);
    var arena = mheap_.arenas[ai.l1()].Value[ai.l2()];
    if (arena == nil) {
        // Non-heap pointer.
        return (default, 0);
    }
    var wordIdx = (obj - alignDown(obj, heapArenaBytes)) / (uintptr)goarch.PtrSize;
    var arenaWord = wordIdx / 8;
    mask = (byte)((byte)(1 << (int)(wordIdx % 8)));
    bytep = arena.checkmarks.at(checkmarksMap.Ꮡb, (ulong)(arenaWord));
    return (bytep, mask);
}

// runCheckmark runs a full non-parallel, stop-the-world mark using
// checkmark bits, to check that we didn't forget to mark anything
// during the concurrent mark process.
//
// The world must be stopped to call runCheckmark.
internal static void runCheckmark(Action<ж<gcWork>> prepareRootSet) {
    assertWorldStopped();

    // Turn off gcwaiting because that will force
    // gcDrain to return early if this goroutine
    // happens to have its preemption flag set.
    // This is fine because the world is stopped.
    // Restore it after we're done just to be safe.
    Ꮡsched.of(schedt.Ꮡgcwaiting).Store(false);
    startCheckmarks();
    gcResetMarkState();
    var gcw = (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡgcw);
    prepareRootSet(gcw);
    gcDrain(gcw, 0);
    wbBufFlush1((~(~getg()).m).p.ptr());
    gcw.dispose();
    endCheckmarks();
    Ꮡsched.of(schedt.Ꮡgcwaiting).Store(true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string finalizerˢ = "finalizer"u8;
internal static readonly @string cleanupˢ = "cleanup"u8;
internal static readonly @string detectedPossibleIssuesˢ = "detected possible issues with cleanups and/or finalizers"u8;

// Find the arena and page index into that arena for this shard.
internal partial struct checkFinalizersAndCleanups_report /*dyn*/ {
    internal nint issues;
    internal uintptr ptr;
    internal ж<special> sp;
}

// checkFinalizersAndCleanups uses checkmarks to check for potential issues
// with the program's use of cleanups and finalizers.
internal static void checkFinalizersAndCleanups() {
    assertWorldStopped();

    const nint reportCycle = /* 1 << iota */ 1;
    const nint reportTiny = 2;
    ref var reports = ref heap(new array<checkFinalizersAndCleanups_report>(50), out var Ꮡreports);
    nint nreports = default;
    bool more = default;
    uintptr lastTinyBlock = default;

    forEachSpecial((uintptr Δp, ж<mspan> s, ж<special> sp) => {
        // N.B. The tiny block specials are sorted first in the specials list.
        if (sp.kind == _KindSpecialTinyBlock) {
            lastTinyBlock = s.@base() + sp.offset;
            return true;
        }

        // We only care about finalizers and cleanups.
        if (sp.kind != _KindSpecialFinalizer && sp.kind != _KindSpecialCleanup) {
            return true;
        }

        // Run a checkmark GC using this cleanup and/or finalizer as a root.
        if (debug.checkfinalizers > 1) {
            print((@string)"Scan trace for cleanup/finalizer on "u8, (Δhex)(uint64)Δp, (@string)":\n"u8);
        }
        runCheckmark((ж<gcWork> gcw) => {
            switch (sp.kind) {
            case var ᴛ when ᴛ == _KindSpecialFinalizer: {
                gcScanFinalizer(sp.Reinterpret<special, specialfinalizer>(), s, gcw);
                break;
            }
            case var ᴛ when ᴛ == _KindSpecialCleanup: {
                gcScanCleanup(sp.Reinterpret<special, specialCleanup>(), gcw);
                break;
            }}
        });
        if (debug.checkfinalizers > 1) {
            println();
        }

        // Now check to see if the object the special is attached to was marked.
        // The roots above do not directly mark p, so if it is marked, then p
        // must be reachable from the finalizer and/or cleanup, preventing
        // reclamation.
        var (bytep, mask) = getCheckmark(Δp);
        if (bytep == nil) {
            return true;
        }
        nint issues = default;
        if ((uint8)(atomic.Load8(bytep) & mask) != 0) {
            issues |= (nint)(reportCycle);
        }
        if (Δp >= lastTinyBlock && Δp < lastTinyBlock + (uintptr)maxTinySize) {
            issues |= (nint)(reportTiny);
        }
        if (issues != 0) {
            if (nreports >= len(Ꮡreports.Value)) {
                more = true;
                return false;
            }
            Ꮡreports.Value[nreports] = new checkFinalizersAndCleanups_report(issues, Δp, sp);
            nreports++;
        }
        return true;
    });

    if (nreports > 0) {
        var lastPtr = (uintptr)0;
        println((@string)"WARNING: LIKELY CLEANUP/FINALIZER ISSUES"u8);
        println();
        foreach (var (_, r) in reports.slice(0, nreports)) {
            ж<specialCheckFinalizer> ctx = default;
            @string kind = default;
            if (r.sp.kind == _KindSpecialFinalizer) {
                kind = finalizerˢ;
                ctx = getCleanupContext(r.ptr, 0);
            } else {
                kind = cleanupˢ;
                ctx = getCleanupContext(r.ptr, ((r.sp.Reinterpret<special, specialCleanup>())).Value.id);
            }

            // N.B. reports is sorted 'enough' that cleanups/finalizers on the same pointer will
            // appear consecutively because the specials list is sorted.
            if (lastPtr != r.ptr) {
                if (lastPtr != 0) {
                    println();
                }
                print((@string)"Value of type "u8, toRType(ctx.ptrType).@string(), (@string)" at "u8, (Δhex)(uint64)r.ptr, (@string)"\n"u8);
                if ((nint)(r.issues & reportCycle) != 0) {
                    if (r.sp.kind == _KindSpecialFinalizer) {
                        println((@string)"  is reachable from finalizer"u8);
                    } else {
                        println((@string)"  is reachable from cleanup or cleanup argument"u8);
                    }
                }
                if ((nint)(r.issues & reportTiny) != 0) {
                    println((@string)"  is in a tiny block with other (possibly long-lived) values"u8);
                }
                if ((nint)(r.issues & reportTiny) != 0 && (nint)(r.issues & reportCycle) != 0) {
                    if (r.sp.kind == _KindSpecialFinalizer) {
                        println((@string)"  may be in the same tiny block as finalizer"u8);
                    } else {
                        println((@string)"  may be in the same tiny block as cleanup or cleanup argument"u8);
                    }
                }
            }
            println();

            println((@string)"Has"u8, kind, (@string)"at"u8, (Δhex)(uint64)(uintptr)r.sp);
            var ΔfuncInfo = findfunc(ctx.funcPC);
            if (ΔfuncInfo.valid()) {
                var (@file, line) = funcline(ΔfuncInfo, ctx.funcPC);
                print((@string)"  "u8, funcname(ΔfuncInfo), (@string)"()\n"u8);
                print((@string)"      "u8, @file, (@string)":"u8, line, (@string)" +"u8, (Δhex)(uint64)(ctx.funcPC - ΔfuncInfo.entry()), (@string)"\n"u8);
            } else {
                print((@string)"  <bad pc "u8, (Δhex)(uint64)ctx.funcPC, (@string)">\n"u8);
            }

            println((@string)"created at: "u8);
            var createInfo = findfunc(ctx.createPC);
            if (createInfo.valid()) {
                var (@file, line) = funcline(createInfo, ctx.createPC);
                print((@string)"  "u8, funcname(createInfo), (@string)"()\n"u8);
                print((@string)"      "u8, @file, (@string)":"u8, line, (@string)" +"u8, (Δhex)(uint64)(ctx.createPC - createInfo.entry()), (@string)"\n"u8);
            } else {
                print((@string)"  <bad pc "u8, (Δhex)(uint64)ctx.createPC, (@string)">\n"u8);
            }

            lastPtr = r.ptr;
        }
        println();
        if (more) {
            println((@string)"... too many potential issues ..."u8);
        }
        @throw(detectedPossibleIssuesˢ);
    }
}

// forEachSpecial is an iterator over all specials.
//
// Used by debug.checkfinalizers.
//
// The world must be stopped.
internal static void forEachSpecial(Func<uintptr, ж<mspan>, ж<special>, bool> yield) {
    assertWorldStopped();

    // Find the arena and page index into that arena for this shard.
    foreach (var (_, ai) in mheap_.markArenas) {
        var ha = mheap_.arenas[ai.l1()].Value[ai.l2()];

        // Construct slice of bitmap which we'll iterate over.
        foreach (var i in range(ha.pageSpecials[..])) {
            // Find set bits, which correspond to spans with specials.
            var specials = atomic.Load8(ha.at(heapArena.ᏑpageSpecials, i));
            if (specials == 0) {
                continue;
            }
            for (nuint j = (nuint)0; j < 8; j++) {
                if ((uint8)(specials & (((uint8)1).Lsh(j))) == 0) {
                    continue;
                }
                // Find the span for this bit.
                //
                // This value is guaranteed to be non-nil because having
                // specials implies that the span is in-use, and since we're
                // currently marking we can be sure that we don't have to worry
                // about the span being freed and re-used.
                var s = ha.spans[(nuint)i * 8 + j];

                // Lock the specials to prevent a special from being
                // removed from the list while we're traversing it.
                for (var sp = s.specials; sp != nil; sp = sp.next) {
                    if (!yield(s.@base() + sp.offset, s, sp)) {
                        return;
                    }
                }
            }
        }
    }
}

} // end runtime_package
