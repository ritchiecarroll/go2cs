// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs HAND-OWNED (native replacement; the auto conversion is kept beside it as mcleanup.cs.auto).
// RULED at COORD c58b4c01d after C1 f9f41e8d8 s3.
//
// WHY THIS FILE IS NOT A FREE ADD. mcleanup.go arrives with Go 1.24 and its AddCleanup calls
// createfing(), which in the converted corpus started the CONVERTED runfinq -- the body mfinal.cs's
// own header declares dead. Taken as a plain auto, runtime.AddCleanup would COMPILE, return a
// Cleanup, and never run it: no throw, no diagnostic, a silent no-op in a public runtime API. The
// rest of the auto body is raw metal that has no managed meaning either -- findObject/spanOfHeap
// walk the heap's span tables, addCleanup threads a _KindSpecialCleanup record onto an mspan's
// specials list, and Stop unlinks it under the span lock. None of that exists here.
//
// WHAT REPLACES IT. A cleanup is keyed on the object's REACHABILITY exactly as a finalizer is
// (mfinal.cs), so it uses the same mechanism: a ConditionalWeakTable keyed on the referent holds a
// set of sentinels, each sentinel's .NET finalizer hands one cleanup body to GoFinalizerQueue, and
// the queue's runner -- the fing analogue -- executes it. The two differ in exactly one way that
// matters to the mechanism: A CLEANUP IS NEVER GIVEN THE OBJECT. Go closes arg into a func() at
// registration and so do we, which is also why a cleanup sentinel must NOT reference the object:
// holding it is the one thing that would stop the cleanup ever running.
//
// DIVERGENCES, STATED HERE RATHER THAN DISCOVERED LATER:
//
//  1. CONCURRENCY. Go: "Cleanups may also run concurrently with one another (unlike finalizers)."
//     Ours run SEQUENTIALLY, on the single fing thread, interleaved with finalizers. Go's own doc
//     already tells a cleanup that must run for a long time to start a goroutine, so no correct Go
//     program depends on the concurrency; what a blocking cleanup costs HERE is the finalizer
//     queue as well, bounded by GoFinalizerQueue.DrainBudgetMs exactly as a parked finalizer is.
//  2. ORDER AGAINST A FINALIZER -- HONOURED SINCE A11, with one residual difference. Go: "If ptr
//     has both a cleanup and a finalizer, the cleanup will only run once it has been finalized and
//     becomes unreachable without an associated finalizer." .NET finalizes the two sentinels in no
//     specified order, so the order is imposed at DISPATCH by a gate the two registrations share
//     (GoFinalizerGate, mfinal.cs): a cleanup whose object dies while its finalizer is pending is
//     HELD, and re-attached to the resurrected object once the finalizer has run, so it runs at the
//     object's next death. THE RESIDUAL: runtime.GC() collects again after the finalizers drain
//     (managed_impl.cs), so when nothing keeps the finalized object alive its cleanup can come due
//     inside the SAME runtime.GC() call, where Go needs a second cycle. It still runs after the
//     finalizer, never before; runtime's TestCleanupAfterFinalizer reads the order, not the cycle.
//  3. THE ptr == arg GUARD IS WIDER THAN GO'S. Go compares the two pointer VALUES. We compare
//     referents, so we also panic when arg is a pointer to a DIFFERENT field of the same
//     allocation. That direction is safe: such an arg keeps the allocation alive, so the cleanup
//     could never run, which is precisely what the guard exists to say. No case is refused whose
//     cleanup would otherwise have run.
//  4. Arena and span checks are GONE, not ported: inUserArenaChunk, findObject and
//     isGoPointerWithoutSpan ask about a heap layout that does not exist here. Their panics
//     ("ptr is arena-allocated", "ptr not in allocated block") are therefore unreachable and the
//     lines are not kept as dead code. debug.sbrk IS kept -- it is a converted variable with a
//     managed meaning and Go's noop-Cleanup answer is still the right one.
[module: go.GoManualConversion]

namespace go;

using atomic = @internal.runtime.atomic_package;
using cpu = @internal.cpu_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;

partial class runtime_package {

// AddCleanup attaches a cleanup function to ptr. Some time after ptr is no longer
// reachable, the runtime will call cleanup(arg) in a separate goroutine.
//
// A typical use is that ptr is an object wrapping an underlying resource (e.g.,
// a File object wrapping an OS file descriptor), arg is the underlying resource
// (e.g., the OS file descriptor), and the cleanup function releases the underlying
// resource (e.g., by calling the close system call).
//
// There are few constraints on ptr. In particular, multiple cleanups may be
// attached to the same pointer, or to different pointers within the same
// allocation.
//
// If ptr is reachable from cleanup or arg, ptr will never be collected
// and the cleanup will never run. As a protection against simple cases of this,
// AddCleanup panics if arg is equal to ptr.
//
// There is no specified order in which cleanups will run.
// In particular, if several objects point to each other and all become
// unreachable at the same time, their cleanups all become eligible to run
// and can run in any order. This is true even if the objects form a cycle.
//
// Cleanups run concurrently with any user-created goroutines.
// Cleanups may also run concurrently with one another (unlike finalizers).
// If a cleanup function must run for a long time, it should create a new goroutine
// to avoid blocking the execution of other cleanups.
//
// If ptr has both a cleanup and a finalizer, the cleanup will only run once
// it has been finalized and becomes unreachable without an associated finalizer.
//
// The cleanup(arg) call is not always guaranteed to run; in particular it is not
// guaranteed to run before program exit.
//
// Cleanups are not guaranteed to run if the size of T is zero bytes, because
// it may share same address with other zero-size objects in memory. See
// https://go.dev/ref/spec#Size_and_alignment_guarantees.
//
// It is not guaranteed that a cleanup will run for objects allocated
// in initializers for package-level variables. Such objects may be
// linker-allocated, not heap-allocated.
//
// Note that because cleanups may execute arbitrarily far into the future
// after an object is no longer referenced, the runtime is allowed to perform
// a space-saving optimization that batches objects together in a single
// allocation slot. The cleanup for an unreferenced object in such an
// allocation may never run if it always exists in the same batch as a
// referenced object. Typically, this batching only happens for tiny
// (on the order of 16 bytes or less) and pointer-free objects.
//
// A cleanup may run as soon as an object becomes unreachable.
// In order to use cleanups correctly, the program must ensure that
// the object is reachable until it is safe to run its cleanup.
// Objects stored in global variables, or that can be found by tracing
// pointers from a global variable, are reachable. A function argument or
// receiver may become unreachable at the last point where the function
// mentions it. To ensure a cleanup does not get called prematurely,
// pass the object to the [KeepAlive] function after the last point
// where the object must remain reachable.
public static Cleanup AddCleanup<T, S>(ж<T> Ꮡptr, Action<S> cleanup, S argʗp) {
    // The pointer to the object must be valid. `abi.Escape` above this line in Go forces ptr to the
    // heap; a go2cs pointer box IS the heap, so there is nothing to force.
    if (Ꮡptr == nil) {
        throw panic("runtime.AddCleanup: ptr is nil");
    }
    // `is null` alone, NOT mfinal.cs's `is null or NilType`: that file's operand is `any`, where a
    // Go nil can arrive still wearing its NilType box. Here the parameter is a TYPED delegate, so
    // the conversion has already happened and null is the only nil there is -- and a NilType pattern
    // against Action<S> does not even compile, which is the kind of copied line that reads correct.
    if (cleanup is null) {
        // Go reaches the same outcome by a different route: calling a nil func value panics when
        // the cleanup is finally invoked, on the finalizer goroutine, with no way to attribute it
        // to the registration. Refusing at the registration is the same rule applied where the
        // caller can still see which AddCleanup did it.
        throw panic("runtime.AddCleanup: cleanup function is nil");
    }
    // Key on the REFERENT for exactly the reason SetFinalizer does (see mfinal.cs): a pointer box is
    // frequently a per-expression temporary, so keying on the box would tie the cleanup to a
    // lifetime nothing in the program shares.
    object referent = ReferentOf(Ꮡptr);
    // Check that arg is not equal to ptr. Go tests this only for a pointer-kinded arg, which is
    // `arg is INilPointer` here -- @unsafe.Pointer is a ж<uintptr> and so answers it too, covering
    // Go's abi.Pointer and abi.UnsafePointer arms in one predicate. See divergence 3 in the header:
    // ours compares referents, so it is the wider guard and safely so.
    if (argʗp is INilPointer && ReferenceEquals(ReferentOf(argʗp!), referent)) {
        throw panic("runtime.AddCleanup: ptr is equal to arg, cleanup will never run");
    }
    if (debug.sbrk != 0) {
        // debug.sbrk never frees memory, so no cleanup will ever run
        // (and we don't have the data structures to record them).
        // Return a noop cleanup.
        return new Cleanup(nil);
    }
    // Go's findObject finds no span for the ZEROBASE, and isGoPointerWithoutSpan makes the cleanup a
    // no-op (`return Cleanup{}`). Every zero-size allocation is that one pointer (golib's GoZeroBase),
    // so registering against it would tie the cleanup to a referent that never dies.
    if (GoZeroBase.Is(referent)) {
        return new Cleanup(nil);
    }
    // Go: `fn := func() { cleanup(arg) }`, closed over arg at registration. The closure is the whole
    // reason a cleanup needs no argument binding at dispatch, unlike a finalizer.
    S arg = argʗp;
    global::System.Action fn = () => cleanup(arg);
    // Ensure we have a finalizer processing goroutine running. createfing() is now the live door
    // (mfinal.cs, rewired under this same ruling); calling it rather than EnsureRunner keeps this
    // line the one Go writes.
    createfing();
    GoCleanupSentinel sentinel = new(fn);
    // GetValue with a factory, NOT GetOrCreateValue: the latter goes through Activator and throws
    // MissingMethodException unless the value type has a PUBLIC parameterless constructor. It would
    // work here today by accident -- a class with no declared constructor gets a public implicit one
    // -- and would start throwing the day someone gives GoCleanupSet a constructor, from a call site
    // that names neither the type nor the reason.
    //
    // Under the lock SetFinalizer links from the other side: a set on an object that already has a
    // Go finalizer waits on that finalizer's gate (A11, divergence 2).
    lock (s_cleanupFinalizerLink) {
        GoCleanupSet set = s_cleanupRegistry.GetValue(referent, static _ => new GoCleanupSet());

        set.Gate = s_finalizerRegistry.TryGetValue(referent, out GoFinalizerSentinel? finalizer) && finalizer.Gate.IsPending
            ? finalizer.Gate
            : null;

        set.Add(sentinel);
    }
    uint64 id = GoCleanupSentinel.Register(sentinel);
    return new Cleanup(
        id: id,
        // Go stores the object's address so Stop can find its span. Ours finds the sentinel by id,
        // so this field is carried for shape and for a debugger only -- it is an order token, not a
        // dereferenceable address, and nothing in this file reads it.
        ptr: ((INilPointer)Ꮡptr).PointerOrderToken
    );
}

// Cleanup is a handle to a cleanup call for a specific object.
[GoType] partial struct Cleanup {
    // id is the unique identifier for the cleanup within the arena.
    internal uint64 id;
    // ptr contains the pointer to the object.
    internal uintptr ptr;
}

// Stop cancels the cleanup call. Stop will have no effect if the cleanup call
// has already been queued for execution (because ptr became unreachable).
// To guarantee that Stop removes the cleanup function, the caller must ensure
// that the pointer that was passed to AddCleanup is reachable across the call to Stop.
public static void Stop(this Cleanup c) {
    if (c.id == 0) {
        // id is set to zero when the cleanup is a noop.
        return;
    }
    // "No effect if the cleanup call has already been queued" falls out of the mechanism rather
    // than needing a check: once the object died, the sentinel was collected and its ~ enqueued the
    // body, so the weak entry below is already dead and there is nothing to cancel. Go says the
    // same thing about its own race, in the same words, for the same reason.
    GoCleanupSentinel.Cancel(c.id);
}

// unique_runtime_blockUntilEmptyCleanupQueue blocks until the cleanup queue is empty or the
// timeout is reached, and reports whether it emptied. Go 1.26 pushes it into unique's in-package
// tests (`//go:linkname unique_runtime_blockUntilEmptyCleanupQueue unique.runtime_blockUntilEmptyCleanupQueue`),
// whose drainCleanupQueue calls it after runtime.GC(). Go's body waits on gcCleanups, the queue
// its cleanup goroutines read, which does not exist here: a cleanup body runs on GoFinalizerQueue
// (see the header), so this is blockUntilEmptyFinalizerQueue's wait. PUBLIC for the forwarder the
// converter emits in the consumer's assembly (linknamePushTargets in go2cs/linknameOperations.go).
// Added at the Go 1.26 hop, ahead of this file's H6 rewrite. Go's timeout is nanoseconds.
public static bool unique_runtime_blockUntilEmptyCleanupQueue(int64 timeout) {
    return blockUntilEmptyFinalizerQueue(timeout);
}

// sync_test_runtime_blockUntilEmptyCleanupQueue is the same wait, pushed into sync's EXTERNAL tests
// (`//go:linkname sync_test_runtime_blockUntilEmptyCleanupQueue sync_test.runtime_blockUntilEmptyCleanupQueue`;
// the consumer is oncefunc_test.go's one-arg handle). Go gives both pushes the identical body.
public static bool sync_test_runtime_blockUntilEmptyCleanupQueue(int64 timeout) {
    return blockUntilEmptyFinalizerQueue(timeout);
}

// Every cleanup registered against one object. ConditionalWeakTable holds ONE value per key, and a
// cleanup is explicitly many-per-object ("multiple cleanups may be attached to the same pointer"),
// so the value is the set rather than the sentinel. The set strong-references its sentinels and the
// dependent handle keeps the set alive only while the key is otherwise reachable, so key, set and
// every sentinel in it become collectible together -- which is exactly when the cleanups are due.
private static readonly global::System.Runtime.CompilerServices.ConditionalWeakTable<object, GoCleanupSet> s_cleanupRegistry = new();

// Orders the two sides of the finalizer/cleanup link (SetFinalizer and AddCleanup, and a re-attach),
// so a set and a finalizer registered concurrently on one object cannot miss each other.
private static readonly object s_cleanupFinalizerLink = new();

// A11: puts cleanups HELD across their object's finalizer back on the object the finalizer
// resurrected. The registry entry survives the resurrection (see GoFinalizerGate.IsPending), so
// this usually finds the same set, now waiting on no finalizer unless the finalizer body registered
// a new one; the sentinels' `~` are re-armed so they run at the object's next death. Each keeps its
// sentinel and so its id: a Stop before the object's next death still cancels it, as Go's does,
// because in Go the cleanup is not queued until then either. If the finalizer registered a new
// finalizer on the object, the new set waits on that one too.
private static void ReattachCleanups(object target, global::System.Collections.Generic.IEnumerable<GoCleanupSentinel> held)
{
    object referent = ReferentOf(target);

    lock (s_cleanupFinalizerLink) {
        GoCleanupSet set = s_cleanupRegistry.GetValue(referent, static _ => new GoCleanupSet());

        set.Gate = s_finalizerRegistry.TryGetValue(referent, out GoFinalizerSentinel? finalizer) && finalizer.Gate.IsPending
            ? finalizer.Gate
            : null;

        foreach (GoCleanupSentinel sentinel in held) {
            sentinel.Reattach(set);
        }
    }
}

private sealed class GoCleanupSet
{
    // ⚠ A LIFETIME ANCHOR, WRITTEN AND NEVER READ, AND THAT IS THE ENTIRE JOB. This list is the only
    // strong reference to a registered sentinel, and it lives under a dependent handle keyed on the
    // object -- so the sentinels die exactly when the object does, which is when their cleanups
    // become due. It looks like dead code to every reader and to most analyzers; deleting it makes
    // every cleanup run at the next GC after registration instead of at the object's death, with no
    // compile error and no test that obviously names it. Do not remove it.
    private readonly global::System.Collections.Generic.List<GoCleanupSentinel> m_sentinels = new();

    // The finalizer this object's cleanups wait on, when the object has one (A11); null otherwise.
    internal volatile GoFinalizerGate? Gate;

    internal void Add(GoCleanupSentinel sentinel)
    {
        lock (m_sentinels)
            m_sentinels.Add(sentinel);

        sentinel.Set = this;
    }
}

// One registered cleanup. It holds the BODY and nothing else -- in particular it does NOT hold the
// object, which is both what Go's API says (the cleanup never receives it) and what makes the
// cleanup able to run at all.
private sealed class GoCleanupSentinel
{
    // NOT readonly, and dropped by Cancel rather than only flagged. The body closes over `arg`, so a
    // cancelled cleanup that merely set a flag would keep arg alive until the OBJECT died -- Go's
    // Stop frees its record there and then, and a Stop that silently retains is the wrong half of
    // that contract. The sentinel shell left behind is two words and dies with the object.
    private global::System.Action? m_body;
    private volatile bool m_cancelled;
    private ulong m_id;

    internal GoCleanupSentinel(global::System.Action body) => m_body = body;

    // The set this sentinel is registered in, read for its finalizer gate when the object dies. A
    // back reference inside one dying group, so it keeps nothing alive that was not already.
    internal volatile GoCleanupSet? Set;

    ~GoCleanupSentinel()
    {
        // Read ONCE into a local. Cancel runs on the caller's thread and this runs on the CLR's, so
        // the flag can turn true between the test and the use; taking the reference first means the
        // two outcomes are "ran" and "did not run", never a null dereference on the finalizer thread.
        global::System.Action? body = m_body;

        if (m_cancelled || body is null)
            return;

        // A11: the object also has a Go finalizer that has not released it -- the gate holds this
        // cleanup for the object's next death instead of queueing it now. The sentinel is reachable
        // again through the gate, so its id entry is renewed at once and a Stop in between still
        // finds it (the old weak entry was cleared when the object died).
        if (Set?.Gate is { } gate && gate.Hold(this))
        {
            s_byID[m_id] = new global::System.WeakReference<GoCleanupSentinel>(this, trackResurrection: false);
            return;
        }

        // HAND OFF, never invoke here -- running a Go body on the CLR finalizer thread is the
        // deadlock GoFinalizerQueue exists to avoid, and it is no less a deadlock for a cleanup.
        // readQueueStats' two counts (the cleanup-queue surface below): queued when the cleanup is handed
        // to the runner, executed once its body has run.
        global::System.Threading.Interlocked.Increment(ref s_cleanupsQueued);
        GoFinalizerQueue.EnqueueCleanup(() => {
            try {
                body();
            }
            finally {
                global::System.Threading.Interlocked.Increment(ref s_cleanupsExecuted);
            }
        });
    }

    // ---- the id table, which exists ONLY so Cleanup.Stop can find a sentinel it must not hold ----
    //
    // Cleanup is a struct a program may keep for as long as it likes in order to call Stop. If it
    // carried the sentinel, keeping the handle would keep the sentinel alive and THE CLEANUP WOULD
    // NEVER RUN -- the exact failure the whole file is about, reintroduced by the cancel path. So
    // the handle carries an id, as Go's does, and the id resolves through a WEAK entry.

    private static ulong s_nextID;
    private static readonly global::System.Collections.Concurrent.ConcurrentDictionary<ulong, global::System.WeakReference<GoCleanupSentinel>> s_byID = new();

    // Swept every s_pruneInterval registrations. Without it the table would keep one dead entry per
    // cleanup ever registered -- small, but unbounded in a long-running program, and unbounded is
    // the property that matters.
    private const int PruneInterval = 64;
    private static int s_sincePrune;

    internal static uint64 Register(GoCleanupSentinel sentinel)
    {
        // Never 0: Go reserves id 0 for the noop cleanup and Stop returns early on it.
        ulong id = global::System.Threading.Interlocked.Increment(ref s_nextID);
        sentinel.m_id = id;

        s_byID[id] = new global::System.WeakReference<GoCleanupSentinel>(sentinel, trackResurrection: false);

        if (global::System.Threading.Interlocked.Increment(ref s_sincePrune) >= PruneInterval)
        {
            global::System.Threading.Interlocked.Exchange(ref s_sincePrune, 0);
            Prune();
        }

        return id;
    }

    // A11: registers a held sentinel in the set its resurrected object now has, and makes its `~`
    // run again at that object's next death. False when a Stop cancelled it while it was held.
    internal bool Reattach(GoCleanupSet set)
    {
        if (m_cancelled || m_body is null)
            return false;

        // The surviving set still lists this sentinel; a set the finalizer body replaced does not.
        if (!ReferenceEquals(Set, set))
            set.Add(this);

        global::System.GC.ReRegisterForFinalize(this); // qualified: bare `GC` binds Go's runtime.GC()
        return true;
    }

    internal static void Cancel(uint64 id)
    {
        if (!s_byID.TryRemove(id, out global::System.WeakReference<GoCleanupSentinel>? entry))
            return;

        if (entry.TryGetTarget(out GoCleanupSentinel? sentinel))
        {
            sentinel.m_cancelled = true;
            // Release arg NOW, as Go's Stop releases its record now -- see the field's comment.
            //
            // The race with ~ resolves to Go's own rule either way, which is why neither side needs
            // a lock. ~ reads the body into a local BEFORE testing the flag, so: if it got there
            // first the cleanup is already queued and Stop "has no effect if the cleanup call has
            // already been queued for execution" -- Go's words, and this is that case; if Cancel got
            // there first, ~ sees a null body, or the flag, or both, and does nothing.
            sentinel.m_body = null;
            // Qualified: a bare `GC` binds Go's runtime.GC() in this namespace.
            global::System.GC.SuppressFinalize(sentinel);
        }
    }

    private static void Prune()
    {
        foreach (global::System.Collections.Generic.KeyValuePair<ulong, global::System.WeakReference<GoCleanupSentinel>> entry in s_byID)
        {
            if (!entry.Value.TryGetTarget(out GoCleanupSentinel? _))
                s_byID.TryRemove(entry.Key, out global::System.WeakReference<GoCleanupSentinel>? _);
        }
    }
}

// =====================================================================================================
// THE GO 1.25 CLEANUP-QUEUE SURFACE (option A, COORD ruling 2026-10-14; PREREAD-h6-runtime-go126 §6.1)
// =====================================================================================================
//
// Go 1.25 moved cleanups onto their own queue: gcCleanups, a cleanupQueue of cleanupBlock lists that
// up to max(GOMAXPROCS/4, 1) cleanup goroutines drain, fed at sweep from the mspan specials. That
// queue is not private to mcleanup.go. Eight converted runtime files reach into it at go1.26.9, so
// the runtime does not compile until it is declared. This file keeps A11's sentinel mechanism (above)
// and divergence 1 (cleanups run sequentially on the fing analogue). It declares Go's types as the
// converter emits them from go1.26.9's mcleanup.go, and answers each member the converted code calls
// with a managed shim:
//
//   member                 site (go1.26.9)                         answer here
//   ---------------------  --------------------------------------  ----------------------------------------
//   lock                   mgc.go gcinit: lockInit(&gcCleanups.     a real mutex field, initialised and never
//                          lock, lockRankCleanupQueue)              contended (nothing below takes it)
//   all                    mgcmark.go markroot, fixedRootCleanups:  stays nil (no cleanupBlock exists), so
//                          walks gcCleanups.all as cleanupBlock     the root loop runs zero times
//   cleanupBlock,          mgcmark.go (the same loop: alllink, n,   declared as emitted; nothing allocates
//   cleanupBlockPtrMask    cleanups[0], cleanupBlockPtrMask)        one
//   cleanupFn,             mheap.go specialCleanup.cleanup and      declared as emitted; specialCleanup
//   cleanupFnPtrMask       addCleanup; mgcmark.go markrootSpans     records are never made here
//   enqueue(cleanupFn)     mheap.go freeSpecial, _KindSpecial-      UNREACHABLE: no mspan cleanup special
//                          Cleanup arm                              exists here, so reaching it throws by name
//   flush()                mgcsweep.go: the last sweeper            a no-op: no P holds a local block
//   needsWake(), wake()    proc.go findRunnable                     false, and a no-op
//   running                mprof.go goroutineProfileWithLabels      stays 0: a running cleanup is on the fing
//                          (counts cleanup goroutines)              runner, not a cleanup goroutine
//   queued                 proc.go destroy: queued += pp.           the field, as Go's; p.cleanupsQueued is
//                          cleanupsQueued (runtime2.cs)             never written here, so it adds 0
//   readQueueStats()       metrics.go /gc/cleanups/*; mgc.go        REAL counts: the managed path counts
//                          gcMarkTermination                        queued and executed (s_cleanups* below)
//   blockUntilEmpty(t)     export_test.go BlockUntilEmptyCleanup-   the finalizer-queue wait, the one the
//                          Queue (mcleanup_test, metrics_test)      two pushed bodies above already use
//
// Out of scope here, and why: cleanupBlock's own methods (enqueue, full, empty, take), cleanupQueue's
// addWork, tryTakeWork, dequeue, needG, createGs, beginRunningCleanups and endRunningCleanups,
// runCleanups, and p.cleanups are all reached only from inside go1.26.9's mcleanup.go, which this file
// replaces whole. Option B, porting the queue for Go's concurrency, is a post-hop design item.

internal static UntypedInt cleanupBlockSize => 512;

[GoType] internal partial struct cleanupBlock {
    /*embed*/ internal cleanupBlockHeader cleanupBlockHeader;
    internal array<cleanupFn> cleanups = new((uintptr)((uintptr)cleanupBlockSize - /* unsafe.Sizeof(cleanupBlockHeader{}) */ (uintptr)32) / /* unsafe.Sizeof(cleanupFn{}) */ (uintptr)24);
}

internal static ж<array<uint8>> ᏑcleanupFnPtrMask = new StandardBox<array<uint8>>(new uint8[]{0b111}.array());
internal static ref array<uint8> cleanupFnPtrMask => ref ᏑcleanupFnPtrMask.Value;

[GoType] internal partial struct cleanupFn {
    internal Action<ж<funcval>, @unsafe.Pointer> call;
    internal ж<funcval> fn;
    internal @unsafe.Pointer arg;
}

internal static ж<array<byte>> ᏑcleanupBlockPtrMask = new StandardBox<array<byte>>(new array<byte>(8));
internal static ref array<byte> cleanupBlockPtrMask => ref ᏑcleanupBlockPtrMask.Value;

[GoType] internal partial struct cleanupBlockHeader {
    internal sys.NotInHeap _;
    /*embed*/ internal lfnode lfnode;
    internal ж<cleanupBlock> alllink;
    internal uint32 n;
}

[GoType] internal partial struct cleanupQueue {
    internal lfstack full;
    internal atomic.Uint64 workUnits;
    internal array<byte> _ = new((uintptr)((uintptr)cpu.CacheLinePadSize - /* unsafe.Sizeof(lfstack(0)) */ (uintptr)8) - /* unsafe.Sizeof(atomic.Uint64{}) */ (uintptr)8);
    internal lfstack free;
    internal atomic.Bool flushed;
    internal array<byte> __ = new((uintptr)((uintptr)cpu.CacheLinePadSize - /* unsafe.Sizeof(lfstack(0)) */ (uintptr)8) - 1);
    internal atomic.UnsafePointer all;
    internal array<byte> ___ = new((uintptr)cpu.CacheLinePadSize - /* unsafe.Sizeof(atomic.UnsafePointer{}) */ (uintptr)8);
    internal mutex @lock;
    internal gList sleeping;
    internal atomic.Uint32 asleep;
    internal atomic.Uint32 running;
    internal atomic.Uint32 ng;
    internal atomic.Uint32 needg;
    internal uint64 queued;
    internal atomic.Uint64 executed;
}

internal static ж<cleanupQueue> ᏑgcCleanups = new StandardBox<cleanupQueue>(new cleanupQueue());
internal static ref cleanupQueue gcCleanups => ref ᏑgcCleanups.Value;

// The managed path's two counts, read by readQueueStats. Incremented by the sentinel's finalizer when it
// hands a cleanup to GoFinalizerQueue, and after the cleanup body returns.
private static long s_cleanupsQueued;
private static long s_cleanupsExecuted;

internal static void enqueue(this ж<cleanupQueue> Ꮡq, cleanupFn c) {
    @throw("runtime: gcCleanups.enqueue reached: the managed runtime makes no mspan cleanup specials (mcleanup.cs runs cleanups through GoFinalizerQueue)"u8);
}

internal static void flush(this ж<cleanupQueue> Ꮡq) {
}

internal static bool needsWake(this ж<cleanupQueue> Ꮡq) {
    return false;
}

internal static void wake(this ж<cleanupQueue> Ꮡq) {
}

internal static (uint64 queued, uint64 executed) readQueueStats(this ж<cleanupQueue> Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();
    uint64 queued = q.queued + (uint64)global::System.Threading.Interlocked.Read(ref s_cleanupsQueued);
    uint64 executed = (uint64)global::System.Threading.Interlocked.Read(ref s_cleanupsExecuted);
    return (queued, executed);
}

internal static bool blockUntilEmpty(this ж<cleanupQueue> Ꮡq, int64 timeout) {
    return blockUntilEmptyFinalizerQueue(timeout);
}

} // end runtime_package
