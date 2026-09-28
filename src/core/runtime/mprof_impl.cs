// mprof_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The block/mutex/memory profile BUCKET STORE in managed storage, and runtime.saveblockevent over it.
// Increment I2 of docs/phase4/DESIGN-managed-profiling.md, the managed bucket store ruling 8fc439415f (B)
// banked and COORD ordered cut on 2026-09-26. Hand-owned by manualConversionFuncs["runtime"] (goosAny):
// newBucket, bucket.stk, bucket.mp, bucket.bp, stkbucket and saveblockevent. Declared once in mprof.go for
// every target, so these bodies serve windows, linux and darwin.
//
// WHAT GO'S STORE IS, AND WHY IT COULD NOT BE CONVERTED. newBucket persistentallocs ONE block holding a
// 48-byte bucket header whose next / allnext are *bucket (reference-bearing), followed by the stk array
// (nstk words), followed by the blockRecord (or the memRecord). bucket.stk(), bp() and mp() reach the two
// trailing parts by BYTE OFFSET (bucket + 48 + nstk*8), and buckhash is a sysAlloc'd native array of
// *bucket. That is the arm-2a class: a reference-bearing pointee at a Go-layout byte offset into storage
// the CLR lays out itself. The converted saveblockevent refused by name for that reason (COORD ruling (A),
// 2026-09-22).
//
// WHAT THIS STORE IS. The bucket header is an ordinary managed box; its stack and its record live in a side
// record keyed by that box, which the three accessors return. The hash is a managed array of boxes chained
// through bucket.next exactly as Go chains them. The ALL-BUCKETS lists (mbuckets / bbuckets / xbuckets)
// keep Go's shape, an atomic.UnsafePointer holding the newest bucket threaded through allnext, stored with
// unsafe.Pointer.FromPinnedBox (the retaining door) and read back by the converted readers'
// (*bucket)(p) conversion, which resolves the token to the same box. So blockProfileInternal,
// mutexProfileInternal, memProfileInternal and iterate_memprof stay CONVERTED and walk the store unchanged.
// Buckets are never freed, in Go or here.
//
// saveblockevent takes Go's `callers` branch (the frame-pointer branch reads getfp(), which has no managed
// answer): the hand-owned callers (managed_impl.cs) projects the CLR stack onto Go-logical frames with
// synthetic PCs, measured capturing the recording frame by name at 8fc439415f.
//
// debug.profstackdepth. Go sets it (default 128) in parsedebugvars on the schedinit path, which this host
// never runs, so it read 0: the recorders returned before recording and every reader sized its stack
// buffer from it (makeProfStack) and expanded nothing. The module initializer below gives it Go's default,
// the one debug variable the profile paths read. GODEBUG=profstackdepth=N is not parsed on this host, as
// no GODEBUG setting is.
//
// THE MEMORY PROFILE'S ALLOCATION RECORDS (class M, piece M1, COORD ruling 2026-09-27). runtime.MemProfileRate
// is displaced by manualConversionVars["runtime"] and declared below as a ref property over golib's
// GoMemProfile.Rate, and memProfileAlloc records what golib's allocation doors sample (see the section
// below and golib/GoMemProfile.cs). The rate starts at 0 unless runtime.pprof is in the program's static
// assembly closure, Go's disableMemoryProfiling (COORD ruling 2026-09-27, option (a)).
//
// Hand-owned (no mprof_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// Go's dbgvars default for profstackdepth (runtime1.go), applied because parsedebugvars never runs here.
[ModuleInitializer]
internal static void initProfStackDepth() {
    if (debug.profstackdepth == 0) {
        debug.profstackdepth = 128;
    }
}

// The trailing parts of Go's one-block bucket, held beside the header instead of after it.
private sealed class bucketParts {
    internal required slice<uintptr> stk;
    internal ж<memRecord>? mp;
    internal ж<blockRecord>? bp;
}

private static readonly ConditionalWeakTable<ж<bucket>, bucketParts> s_bucketParts = new();

// buckhash as a managed array of chain heads. Written under profInsertLock, read without it, as Go does.
private static ж<bucket>?[]? s_buckhash;

internal static ж<bucket> newBucket(bucketType typ, nint nstk) {
    var parts = new bucketParts { stk = new slice<uintptr>((int)nstk) };

    if (typ == memProfile) {
        parts.mp = Ꮡ(new memRecord());
    }
    else if (typ == blockProfile || typ == mutexProfile) {
        parts.bp = Ꮡ(new blockRecord());
    }
    else {
        @throw("invalid profile bucket type"u8);
    }

    var b = Ꮡ(new bucket());
    b.Value.typ = typ;
    b.Value.nstk = (uintptr)nstk;
    s_bucketParts.Add(b, parts);
    return b;
}

private static bucketParts partsOf(ж<bucket> b) {
    if (!s_bucketParts.TryGetValue(b, out bucketParts? parts)) {
        @throw("runtime: profile bucket not allocated by newBucket"u8);
    }
    return parts!;
}

internal static slice<uintptr> stk(this ж<bucket> Ꮡb) {
    if (Ꮡb.Value.nstk > maxProfStackDepth) {
        @throw("bad profile stack count"u8);
    }
    return partsOf(Ꮡb).stk;
}

internal static ж<memRecord> mp(this ж<bucket> Ꮡb) {
    if (Ꮡb.Value.typ != memProfile) {
        @throw("bad use of bucket.mp"u8);
    }
    return partsOf(Ꮡb).mp!;
}

internal static ж<blockRecord> bp(this ж<bucket> Ꮡb) {
    if (Ꮡb.Value.typ != blockProfile && Ꮡb.Value.typ != mutexProfile) {
        @throw("bad use of bucket.bp"u8);
    }
    return partsOf(Ꮡb).bp!;
}

internal static ж<bucket> stkbucket(bucketType typ, uintptr size, slice<uintptr> stk, bool alloc) {
    var bh = Volatile.Read(ref s_buckhash);
    if (bh is null) {
        @lock(ᏑprofInsertLock);
        bh = s_buckhash;
        if (bh is null) {
            bh = new ж<bucket>?[(int)buckHashSize];
            Volatile.Write(ref s_buckhash, bh);
        }
        unlock(ᏑprofInsertLock);
    }

    // Hash stack.
    uintptr h = default!;
    foreach (var (_, pc) in stk) {
        h += pc;
        h += (h << 10);
        h ^= (uintptr)(h >> 6);
    }
    // hash in size
    h += size;
    h += (h << 10);
    h ^= (uintptr)(h >> 6);
    // finalize
    h += (h << 3);
    h ^= (uintptr)(h >> 11);

    nint i = (nint)(h % (uintptr)buckHashSize);
    // first check optimistically, without the lock
    for (var b = Volatile.Read(ref bh[i]); b is not null && b != nil; b = b.Value.next) {
        if (b.Value.typ == typ && b.Value.hash == h && b.Value.size == size && eqslice(b.stk(), stk)) {
            return b;
        }
    }

    if (!alloc) {
        return default!;
    }

    @lock(ᏑprofInsertLock);
    // check again under the insertion lock
    for (var b = bh[i]; b is not null && b != nil; b = b.Value.next) {
        if (b.Value.typ == typ && b.Value.hash == h && b.Value.size == size && eqslice(b.stk(), stk)) {
            unlock(ᏑprofInsertLock);
            return b;
        }
    }

    // Create new bucket.
    var nb = newBucket(typ, len(stk));
    copy(nb.stk(), stk);
    nb.Value.hash = h;
    nb.Value.size = size;

    ж<atomic.UnsafePointer> allnext = default!;
    if (typ == memProfile) {
        allnext = Ꮡmbuckets;
    }
    else if (typ == mutexProfile) {
        allnext = Ꮡxbuckets;
    }
    else {
        allnext = Ꮡbbuckets;
    }

    nb.Value.next = bh[i] ?? (ж<bucket>)nil;
    nb.Value.allnext = (ж<bucket>)(uintptr)(allnext.Load());

    // Publish the bucket to its hash chain and its all-buckets list, as Go's two StoreNoWB do.
    Volatile.Write(ref bh[i], nb);
    allnext.StoreNoWB(@unsafe.Pointer.FromPinnedBox(nb));

    unlock(ᏑprofInsertLock);
    return nb;
}

internal static void saveblockevent(int64 cycles, int64 rate, nint skip, bucketType which) {
    if (debug.profstackdepth == 0) {
        // profstackdepth is set to 0 by the user, so no stack can be recorded (Go's own early return).
        return;
    }
    if (skip > maxSkip) {
        @throw("invalid skip value"u8);
    }

    // Go's callers branch into its own buffer (Go: mp.profStack, 1 + maxSkip + profstackdepth words).
    var stk = new slice<uintptr>(1 + (int)maxSkip + (int)debug.profstackdepth);
    nint nstk = callers(skip, stk);

    saveBlockEventStack(cycles, rate, stk[..(int)nstk], which);
}

// ---- the memory profile's allocation records (class M, piece M1; COORD ruling 2026-09-27) ----
//
// runtime.MemProfileRate's storage is golib's GoMemProfile.Rate, because Go reads the rate on every
// allocation and allocation happens in golib's constructors here, not in mallocgc. The converted
// declaration is displaced by manualConversionVars["runtime"] and this ref property takes its place, so
// Go code that writes `runtime.MemProfileRate = 1` writes the value golib reads.
public static ref nint MemProfileRate => ref GoMemProfile.Rate;

// golib samples by Go's rule (GoMemProfile.Charge) and hands each sampled allocation here. A method group,
// not a lambda: a lambda would be a Go-source frame of its own between the allocating function and this one.
//
// Go's disableMemoryProfiling, decided the same way Go decides it: from what the program CAN reach, once,
// before any Go code runs. Go's linker sets it when runtime.memProfileInternal is unreachable, which in
// practice means the program does not import runtime/pprof (or testing, which imports it), and the runtime
// then starts with MemProfileRate 0 (proc.go). Here the question is whether runtime.pprof is in the
// program's STATIC assembly closure: the host's TRUSTED_PLATFORM_ASSEMBLIES, which is the app's deps.json
// list and is fixed before the first assembly loads, never the assemblies loaded so far, which load lazily
// and would read "no pprof" at this point in every program. Where the host has no such list (a native AOT
// or single-file publish), runtime.pprof's package type is looked up by its constant name, which native
// AOT's compiler resolves against the assemblies it compiled (see memProfileReachable). Where neither
// answers, Go's default rate stands.
//
// DEVIATIONS. A program that calls runtime.MemProfile directly without runtime.pprof in its closure reads
// an empty profile unless it sets MemProfileRate itself, as a Go program whose linker dropped
// memProfileInternal could not (Go keeps the profile on whenever MemProfile is reachable). And go2cs's
// hand-owned testing does not reference runtime.pprof, as Go's does, so a converted test package that does
// not import runtime/pprof itself starts with the rate at 0 where Go's test binary starts at 512 KiB.
[ModuleInitializer]
internal static void initMemProfileRecorder() {
    GoMemProfile.Recorder = memProfileAlloc;
    if (!memProfileReachable(AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string, findPprofPackage)) {
        GoMemProfile.Rate = 0;
    }
}

private const string pprofAssemblyName = "runtime.pprof";

// The fallback's question is asked by type name, not by walking references: under native AOT the
// compiler resolves a constant Type.GetType name against the assemblies it was given, so the answer is the
// static closure there, and GetReferencedAssemblies throws PlatformNotSupportedException. The name is a
// constant AT THE CALL: the compiler does not follow it through a parameter.
private static Type? findPprofPackage() => Type.GetType("go.runtime.pprof_package, runtime.pprof", throwOnError: false);

private static bool memProfileReachable(string? trustedPlatformAssemblies, Func<Type?> findPprof) {
    if (trustedPlatformAssemblies is not null) {
        foreach (string path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)) {
            if (string.Equals(Path.GetFileNameWithoutExtension(path), pprofAssemblyName, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
    try {
        return findPprof() is not null;
    }
    catch (Exception) {
        // The closure could not be read: keep Go's default rather than silently switch the profile off.
        return true;
    }
}

// mProf_Malloc for an allocation made through a golib constructor. Go's own mProf_Malloc stays converted
// and unreached: profilealloc is called only from mallocgc, which this host does not run. The body is
// Go's, with three differences. The stack buffer is allocated per sample rather than kept on the M. The
// skip is 1, because the only Go-source frame between the allocating function and callers() is this one
// (golib's frames are not Go frames, and Go's own skip of 5 counts mProf_Malloc, profilealloc, mallocgc
// and its entry points). And setprofilebucket, which ties the object to its bucket for the free side,
// is M2's. An allocation no Go frame made is not recorded.
[MethodImpl(MethodImplOptions.NoInlining)]
private static void memProfileAlloc(object allocation, nuint size, bool noscan) {
    uintptr fullSize = roundupsize((uintptr)size, noscan);
    var stk = new slice<uintptr>((int)debug.profstackdepth);
    nint nstk = callers(1, stk);
    if (nstk == 0) {
        // No Go-source frame made this allocation: host code (a test host, golib's own bookkeeping) did,
        // and Go's profile has no such record.
        return;
    }
    var index = (ᏑmProfCycle.read() + 2) % (uint32)len(new memRecord(nil).future);
    var b = stkbucket(memProfile, fullSize, stk[..(int)(nstk)], true);
    var mr = b.mp();
    var mpc = mr.at(memRecord.Ꮡfuture, (nint)(index));
    @lock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    mpc.Value.allocs++;
    mpc.Value.alloc_bytes += fullSize;
    unlock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
}

// ---- the guard's view (RuntimeBlockEventTests): GolibTests is outside runtime's InternalsVisibleTo
//      grant, so this Go-prefixed public helper exposes the one operation ----

/// <summary>Whether a program whose static assembly closure is <paramref name="trustedPlatformAssemblies"/>
/// (a path list, as the host's TRUSTED_PLATFORM_ASSEMBLIES; null where the host has none) starts with the
/// memory profile on, asking <paramref name="findPprof"/> when there is no list.</summary>
public static bool GoMemProfileReachable(string? trustedPlatformAssemblies, Func<Type?> findPprof) =>
    memProfileReachable(trustedPlatformAssemblies, findPprof);

/// <summary>Records <paramref name="count"/> block events of <paramref name="cycles"/> each through
/// <c>runtime.blockevent(cycles, 1)</c> -- the call runtime/pprof's TestBlockProfileBias makes.</summary>
[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
public static void GoBlockEventProbe(int64 cycles, nint count) {
    for (nint i = 0; i < count; i++) {
        blockevent(cycles, 1);
    }
}

} // end runtime_package
