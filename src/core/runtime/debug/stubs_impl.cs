// stubs_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written bodies for runtime/debug's "Implemented in package runtime" declarations.
//
// These are bodyless Go funcs (runtime/debug/stubs.go, garbage.go, mod.go, stack.go) that the Go
// LINKER binds to a runtime function carrying the matching `//go:linkname <impl> runtime/debug.<name>`
// PUSH directive. go2cs has no cross-assembly linker, so it emits them as bodyless `partial`
// methods and the PartialStubGenerator fills each with a throwing stub — debug.SetGCPercent threw
// on its first call, which is the very first line of sync's TestPool / TestPoolNew.
//
// Forwarding to the converted runtime implementations would NOT help: every one of them drives
// Go's own GC pacer / scheduler (runtime.setGCPercent takes the mheap lock on the system stack and
// waits on a mark cycle), machinery that does not exist under the CLR. This is the same fork the
// sync runtime layer took — reimplement the SEMANTIC on managed primitives at the API boundary,
// never emulate the mechanism.
//
// What each knob means here, and where it honestly diverges:
//   - setGCPercent / setMemoryLimit / setMaxStack / setMaxThreads are TUNING knobs. The CLR's GC
//     and thread pool expose no equivalent runtime-settable analog, so each keeps Go's documented
//     GET/SET CONTRACT (remember the value, return the previous one, negative input = query only
//     where Go says so) and has no effect on collection. Programs that only save-and-restore them
//     — `defer debug.SetGCPercent(debug.SetGCPercent(-1))`, the standard test idiom — are exactly
//     right; a program that *asserts collection behaviour changed* is capability-divergent.
//   - freeOSMemory is a REAL operation: a blocking full collect plus a compacting LOH pass is the
//     managed equivalent of "collect and return memory to the OS".
//   - setPanicOnFault is per-GOROUTINE in Go, so it is [ThreadStatic] here (a goroutine is a
//     managed thread in go2cs). It has no effect: the CLR turns a bad access into an
//     AccessViolationException the process cannot resume from.
//   - readGCStats reports a REAL per-pause history, read from the same golib GcPauseRecorder
//     snapshot runtime.ReadMemStats reads (docs/phase4/DESIGN-readmemstats-surface.md, ratified
//     2026-08-21). It used to report an empty one — "the CLR keeps no comparable pause/end-time log,
//     and fabricating one would be worse than reporting none" — and that refusal was right for as
//     long as there was nothing to report; the recorder supplies the missing half rather than
//     laundering the assert, which is why NumGC is still the real gen2 count and not zeroed to make
//     two lengths agree. The packed layout ReadGCStats expects (n pauses, n ends, lastGC, numGC,
//     totalPause) is honored exactly, most-recent-first, at length 2n+3.
//   - modinfo / WriteHeapDump / SetTraceback have no managed form; they are inert, matching what a
//     binary built without module info or heap-dump support reports.
//   - runtime_setCrashFD is a REAL operation as of 2026-08-21, and it no longer owns the slot: the
//     descriptor lives in golib (go.golib.CrashReport), which is where Go keeps it —
//     runtime.crashFD, the very symbol this func's //go:linkname names — and where the printer
//     that writes to it lives. An unhandled panic now writes Go's crash report to this descriptor
//     as well as to stderr, which is what runtime/debug's own TestSetCrashOutput measures
//     (docs/phase4/DESIGN-crash-report.md).
//
// Hand-owned: there is no stubs_impl.go, so a -stdlib reconvert never regenerates this file.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;

[module: go.GoManualConversion]

namespace go.runtime;

using time = go.time_package;

partial class debug_package
{
    // Either assembly can be the first one a program touches — ReadGCStats reaches readGCStats
    // without going through runtime — so runtime/debug arms the recorder from its own initializer
    // too. GcPauseRecorder.Arm is idempotent; whichever runs first wins and the other returns.
    [ModuleInitializer]
    internal static void ᴛArmGcPauseRecorder()
    {
        GcPauseRecorder.Arm();
    }

    // Go's own defaults, so a first GET returns what Go would report.
    private static int32 s_gcPercent = 100;                 // GOGC=100
    private static int64 s_memoryLimit = int64.MaxValue;    // math.MaxInt64 — "no limit"
    private static nint s_maxStack = 1_000_000_000;         // runtime.maxstacksize on 64-bit
    private static nint s_maxThreads = 10_000;              // runtime sched.maxmcount

    // paniconfault is a per-goroutine flag in Go; a goroutine is a managed thread here.
    [ThreadStatic]
    private static bool t_panicOnFault;

    internal static partial int32 setGCPercent(int32 @in)
    {
        int32 old = Interlocked.Exchange(ref s_gcPercent, @in);

        // Go's setGCPercent waits out any in-flight mark when GC is being disabled, so the caller
        // returns with no collection running. Draining pending finalizers is the closest managed
        // equivalent and keeps the "quiet heap on return" property the test idiom relies on.
        if (@in < 0)
            GC.WaitForPendingFinalizers();

        return old;
    }

    internal static partial int64 setMemoryLimit(int64 @in)
    {
        // Documented: a negative input does not adjust the limit, it only reads it back.
        return @in < 0 ? Interlocked.Read(ref s_memoryLimit) : Interlocked.Exchange(ref s_memoryLimit, @in);
    }

    internal static partial nint setMaxStack(nint @in)
    {
        return Interlocked.Exchange(ref s_maxStack, @in);
    }

    internal static partial nint setMaxThreads(nint @in)
    {
        return Interlocked.Exchange(ref s_maxThreads, @in);
    }

    internal static partial bool setPanicOnFault(bool @new)
    {
        bool old = t_panicOnFault;
        t_panicOnFault = @new;
        return old;
    }

    internal static partial void freeOSMemory()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

        // Same boundary runtime.GC() closes, for the same reason: the pause recorder is woken by the
        // finalizer thread, and FreeOSMemory is documented to have completed a cycle when it returns.
        // Draining here is also what makes HeapReleased fresh at the one point Go's TestFreeOSMemory
        // reads it — the drain re-samples TotalCommittedBytes AFTER the memory has gone back.
        GcPauseRecorder.Drain();
        GcPauseRecorder.NoteForcedGC();
    }

    // Scratch for readGCStats' most-recent-first transfer. Fixed storage allocated once, for the
    // same reason the recorder's ring is: a fresh 2 x 256 array per call would be a KB-scale
    // allocation on a measurement surface (DESIGN-readmemstats-surface.md §8.2). It doubles as the
    // lock for its own reuse — readGCStats is a rare call and its cost is not on any measured path.
    private static readonly ulong[] s_gcStatsScratch = new ulong[2 * GcPauseRecorder.RingLength];

    internal static partial void readGCStats(ж<slice<time.Duration>> Ꮡp)
    {
        // ReadGCStats' packed layout: [n pauses][n pause end times][lastGC unix-ns][numGC][total
        // pause], with the pauses and end times MOST RECENT FIRST — which is what its own
        // `n := len(stats.Pause) - 3; n /= 2` arithmetic expects, and what its backwards walk over
        // MemStats.PauseNs then lines up against. Both halves come from one GcPauseRecorder snapshot
        // under one lock, so the two surfaces read the same NumGC, the same total, the same ring.
        slice<time.Duration> buffer = Ꮡp.Value;

        lock (s_gcStatsScratch)
        {
            GcPauseSnapshot gc = GcPauseRecorder.ReadMostRecentFirst(s_gcStatsScratch, out int n);
            nint entries = 2 * n + 3;

            // Reslicing to `entries` may GROW the caller's slice back out toward its capacity: after
            // a previous ReadGCStats call, stats.Pause has len n but still carries the full
            // 2*256+3 capacity, exactly as Go's `p = p[:cap(p)]` accounts for.
            if (cap(buffer) < entries)
                buffer = new slice<time.Duration>(entries);

            buffer = buffer[..(int)entries];

            for (int i = 0; i < 2 * n; i++)
                buffer[i] = (time.Duration)(int64)s_gcStatsScratch[i];

            buffer[2 * n] = (time.Duration)(int64)gc.LastGcEndUnixNs;       // lastGC, unix ns
            buffer[2 * n + 1] = (time.Duration)(int64)gc.NumGC;             // numGC
            buffer[2 * n + 2] = (time.Duration)(int64)gc.PauseTotalNs;      // total pause, ns
        }

        Ꮡp.Value = buffer;
    }

    internal static partial @string modinfo()
    {
        // Go returns the module info the linker embedded; a binary built without it returns "",
        // and ReadBuildInfo then reports ok == false. That is the honest answer here.
        return ""u8;
    }

    public static partial void WriteHeapDump(uintptr fd)
    {
        throw panic((@string)"runtime/debug: WriteHeapDump is not supported by the managed runtime"u8);
    }

    public static partial void SetTraceback(@string level)
    {
        // Traceback verbosity governs the Go runtime's own crash printer, which the managed
        // runtime does not use; the call is accepted and inert, as it is in a Go binary whose
        // GOTRACEBACK is already at or above the requested level.
    }

    internal static partial uintptr runtime_setCrashFD(uintptr fd)
    {
        // Go redirects the runtime's fatal-error output to fd and returns the previous one
        // (^uintptr(0) when unset, which tells SetCrashOutput not to close anything). Both halves
        // of that contract are golib's now — the slot AND the writer — so this is the forwarding
        // linkname it is in Go, and nothing about SetCrashOutput's own close-the-previous-fd logic
        // changes.
        return CrashReport.SetCrashOutputFd(fd);
    }
}
