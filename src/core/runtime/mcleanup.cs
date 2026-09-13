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
//  2. ORDER AGAINST A FINALIZER. Go: "If ptr has both a cleanup and a finalizer, the cleanup will
//     only run once it has been finalized and becomes unreachable without an associated
//     finalizer." We cannot order two .NET finalizers against each other, so the two sentinels for
//     one object are collected in an unspecified order and their bodies queue in that order. Go
//     specifies no order among cleanups either; this is the one ordering Go DOES specify and we do
//     not honour it.
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
    // Go: `fn := func() { cleanup(arg) }`, closed over arg at registration. The closure is the whole
    // reason a cleanup needs no argument binding at dispatch, unlike a finalizer.
    S arg = argʗp;
    global::System.Action fn = () => cleanup(arg);
    // Ensure we have a finalizer processing goroutine running. createfing() is now the live door
    // (mfinal.cs, rewired under this same ruling); calling it rather than EnsureRunner keeps this
    // line the one Go writes.
    createfing();
    GoCleanupSentinel sentinel = new(fn);
    s_cleanupRegistry.GetOrCreateValue(referent).Add(sentinel);
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

// Every cleanup registered against one object. ConditionalWeakTable holds ONE value per key, and a
// cleanup is explicitly many-per-object ("multiple cleanups may be attached to the same pointer"),
// so the value is the set rather than the sentinel. The set strong-references its sentinels and the
// dependent handle keeps the set alive only while the key is otherwise reachable, so key, set and
// every sentinel in it become collectible together -- which is exactly when the cleanups are due.
private static readonly global::System.Runtime.CompilerServices.ConditionalWeakTable<object, GoCleanupSet> s_cleanupRegistry = new();

private sealed class GoCleanupSet
{
    // ⚠ A LIFETIME ANCHOR, WRITTEN AND NEVER READ, AND THAT IS THE ENTIRE JOB. This list is the only
    // strong reference to a registered sentinel, and it lives under a dependent handle keyed on the
    // object -- so the sentinels die exactly when the object does, which is when their cleanups
    // become due. It looks like dead code to every reader and to most analyzers; deleting it makes
    // every cleanup run at the next GC after registration instead of at the object's death, with no
    // compile error and no test that obviously names it. Do not remove it.
    private readonly global::System.Collections.Generic.List<GoCleanupSentinel> m_sentinels = new();

    internal void Add(GoCleanupSentinel sentinel)
    {
        lock (m_sentinels)
            m_sentinels.Add(sentinel);
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

    internal GoCleanupSentinel(global::System.Action body) => m_body = body;

    ~GoCleanupSentinel()
    {
        // Read ONCE into a local. Cancel runs on the caller's thread and this runs on the CLR's, so
        // the flag can turn true between the test and the use; taking the reference first means the
        // two outcomes are "ran" and "did not run", never a null dereference on the finalizer thread.
        global::System.Action? body = m_body;

        if (m_cancelled || body is null)
            return;

        // HAND OFF, never invoke here -- running a Go body on the CLR finalizer thread is the
        // deadlock GoFinalizerQueue exists to avoid, and it is no less a deadlock for a cleanup.
        GoFinalizerQueue.EnqueueCleanup(body);
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

        s_byID[id] = new global::System.WeakReference<GoCleanupSentinel>(sentinel, trackResurrection: false);

        if (global::System.Threading.Interlocked.Increment(ref s_sincePrune) >= PruneInterval)
        {
            global::System.Threading.Interlocked.Exchange(ref s_sincePrune, 0);
            Prune();
        }

        return id;
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

} // end runtime_package
