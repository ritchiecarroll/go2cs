// BoringCaches.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;

namespace go.golib;

/// <summary>
/// The registry behind <c>crypto/internal/boring/bcache</c>'s GC-cleared caches — the managed
/// form of Go's <c>runtime.registerCache</c> / <c>clearpools</c> boringcrypto arm.
/// </summary>
/// <remarks>
/// <para>
/// <b>What Go does.</b> <c>bcache.Cache.Register</c> hands the runtime the ADDRESS of the cache's
/// <c>ptable</c> word (<c>registerCache(unsafe.Pointer(&amp;c.ptable))</c>), and
/// <c>clearpools</c> — which <c>gcStart</c> runs at the head of every cycle — stores nil straight
/// into each registered address with <c>atomicstorep</c>. No Go code runs; the collector does raw
/// memory work on a word it was given the address of.
/// </para>
/// <para>
/// <b>Why the address cannot be the currency here.</b> The registered word is an
/// <c>atomic.Pointer[cacheTable[K,V]]</c>, whose managed slot holds a <c>ж&lt;T&gt;</c> REFERENCE
/// rather than a machine word (see <c>sync/atomic</c>'s <c>Pointer&lt;T&gt;</c>). Storage
/// containing references is not pinnable, so the <c>ж&lt;T&gt; → uintptr</c> conversion's
/// <c>EnsureStableAddress</c> gets no pin back from <see cref="PinnedBuffer.PinOnly"/>, the
/// provenance record it writes can never satisfy <c>IsPinnedAt</c>, and
/// <c>ManagedPointerTokens.Resolve</c> therefore answers MISS for the number by design
/// (validate-on-read). The number that reaches <c>registerCache</c> names nothing recoverable —
/// which is what <c>runtime</c>'s <c>managed_impl.cs</c> meant by pointer stores that "have no
/// managed meaning", and it is a structural fact about the slot's type, not a gap to be widened
/// around. Pinning it to force the issue would also be self-defeating: this is the package whose
/// entire purpose is to let the collector reclaim what it caches.
/// </para>
/// <para>
/// <b>What this does instead.</b> A registration is a CLEAR DELEGATE — which is the same currency
/// the runtime's two other <c>clearpools</c> arms already use (<c>poolcleanup</c> is an
/// <c>Action</c>, unique's map cleanup is a channel), and the delegate the hand-owned
/// <c>Register</c> hands over is the package's OWN <c>Clear</c> method. Go documents that method
/// as exactly this mechanism — "the runtime does this automatically at each garbage collection;
/// this method is exposed only for testing" — so the managed model performs the operation Go
/// names, by the route Go's own comment describes, rather than simulating the address store it
/// happens to be implemented with.
/// </para>
/// <para>
/// <b>Cadence — a resurrecting finalizable sentinel</b>, the same mechanism (and for the same
/// reasons) as <see cref="GcPauseRecorder"/>'s: nothing strongly references
/// <see cref="Sentinel"/>, so it is finalized by every collection that condemns its generation and
/// re-registers itself for the next one. A cycle is filtered to a CLR gen2 collection, per the
/// ratified identity <see cref="GcPauseRecorder"/> is built on (<c>DESIGN-readmemstats-surface.md</c>
/// §2: a Go GC cycle IS a gen2 collection) — so the cache is cleared on the same cadence Go clears
/// it on, rather than on every ephemeral collection, which would be within the lossy contract but
/// would leave a cache that is almost always empty.
/// </para>
/// <para>
/// <b>Two honest differences from Go, neither observable through the package's contract.</b>
/// (1) Go clears at the START of a cycle; a finalizer runs at the END of one. The contract is that
/// entries may vanish at any collection and clients must cope — "the cache is lossy, and the loss
/// happens at the start of each GC" — so a clear that lands a moment later still keeps every
/// promise the cache makes, and keeps the one that matters: a key the cache saw does not stay
/// reachable through it. (2) The finalizer is asynchronous, so a program cannot observe the clear
/// at a precise instant — which is why <c>runtime.GC()</c> calls <see cref="ClearAll"/> DIRECTLY
/// before returning, exactly as it invokes <c>poolcleanup</c> directly rather than waiting for a
/// cycle to get around to it. Go's <c>GC()</c> is documented to complete a full cycle, and
/// bcache's own test reads the cache immediately after one.
/// </para>
/// <para>
/// <b>Cost.</b> Nothing at all until a cache registers, which only BoringCrypto's shadow-key
/// caches (and this package's test) ever do: the sentinel is armed by the first registration, not
/// by a module initializer. After that it is one ~24 B object cycling through the finalization
/// queue and one <c>CollectionCount</c> read per collection.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public static class BoringCaches
{
    // Copy-on-write: registration is a package-init-time event (Go requires Register to be called
    // during package initialization) and clearing is the hot, finalizer-thread path, so the clear
    // side takes no lock and allocates nothing — which matters because it runs inside a finalizer.
    private static Action[] s_caches = [];

    // Serializes registrations against each other and against arming the sentinel.
    private static readonly object s_lock = new();

    private static bool s_armed;

    // The gen2 count this registry has already cleared for — GcPauseRecorder.Observe's step 1,
    // and for the same reason: the sentinel's first callbacks fire on ephemeral collections,
    // before it has been promoted, and a Go GC cycle is a gen2 collection.
    private static long s_clearedThrough;

    /// <summary>
    /// Registers a cache's clear operation, to be run once per GC cycle from then on.
    /// </summary>
    /// <param name="clear">
    /// The cache's own <c>Clear</c> — it captures the cache, and so keeps it alive exactly as Go's
    /// <c>boringCaches</c> slice keeps every registered <c>&amp;c.ptable</c> alive. That retention
    /// is the CACHE's, never its entries': clearing is what releases the keys and values, which is
    /// the whole of the package's GC-friendliness.
    /// </param>
    public static void Register(Action clear)
    {
        ArgumentNullException.ThrowIfNull(clear);

        lock (s_lock)
        {
            Action[] updated = new Action[s_caches.Length + 1];
            s_caches.CopyTo(updated, 0);
            updated[^1] = clear;

            Volatile.Write(ref s_caches, updated);

            if (s_armed)
                return;

            s_armed = true;
            s_clearedThrough = GC.CollectionCount(GC.MaxGeneration);

            // Deliberately unrooted — a strong reference would mean it is never collected and its
            // finalizer never runs. GcPauseRecorder.Arm's sentinel carries the same note, and
            // GolibTests' negative control proves that half of the mechanism.
            _ = new Sentinel();
        }
    }

    /// <summary>
    /// Clears every registered cache immediately — <c>clearpools</c>' boringcrypto arm, run
    /// directly. <c>runtime.GC()</c> calls this so a completed forced cycle leaves the state a
    /// completed Go cycle leaves behind.
    /// </summary>
    public static void ClearAll()
    {
        foreach (Action clear in Volatile.Read(ref s_caches))
            clear();
    }

    // The gen2 filter: clear once per Go GC cycle, never per ephemeral collection.
    private static void ClearForCollection()
    {
        long trueCount = GC.CollectionCount(GC.MaxGeneration);

        if (trueCount <= Interlocked.Read(ref s_clearedThrough))
            return;

        Interlocked.Exchange(ref s_clearedThrough, trueCount);

        ClearAll();
    }

    private sealed class Sentinel
    {
        ~Sentinel()
        {
            try
            {
                ClearForCollection();
            }
            catch
            {
                // An exception escaping a finalizer takes the process down. A cache that could do
                // that would be a far worse defect than a missed clear — and a missed clear is
                // inside the contract, since the cache is lossy by construction.
            }

            if (!Environment.HasShutdownStarted)
                GC.ReRegisterForFinalize(this);
        }
    }
}
