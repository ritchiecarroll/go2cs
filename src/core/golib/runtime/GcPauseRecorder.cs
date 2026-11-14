// GcPauseRecorder.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;

namespace go.golib;

/// <summary>
/// The per-GC-cycle pause history behind <c>runtime.ReadMemStats</c> and
/// <c>runtime/debug.ReadGCStats</c> — one recorder, one ring, one snapshot, so the two surfaces
/// cannot disagree with each other.
/// </summary>
/// <remarks>
/// <para>
/// Design: <c>docs/phase4/DESIGN-readmemstats-surface.md</c> (RATIFIED 2026-08-21, all six open
/// questions ruled as recommended; §7.1 carries the S0/S1 measurements this implementation is built
/// on). §2 is the one definition everything below follows from: <b>a Go GC cycle is a CLR gen2
/// collection</b>. Go has one heap generation, so every Go cycle is a full cycle; the CLR has three,
/// and <c>NumGC</c> had already chosen gen2 (<c>GC.CollectionCount(GC.MaxGeneration)</c>). Applying
/// that identity uniformly makes the ring a gen2 ring, <c>LastGC</c> a gen2 end time, and
/// <c>PauseTotalNs</c> the ring's gen2 running sum (⟨OQ-5⟩).
/// </para>
/// <para>
/// <b>The mechanism (§3.1, §3.2) — a resurrecting finalizable sentinel.</b> <see cref="Sentinel"/> is
/// an object nothing strongly references; its finalizer calls <see cref="Observe"/> and then
/// <c>GC.ReRegisterForFinalize(this)</c>, so it wakes once per collection that condemns its
/// generation — which, after its first promotion, is gen2. The alternatives were priced and refused:
/// <c>GC.RegisterForFullGCNotification</c> requires background GC to be turned OFF process-wide, an
/// in-process EventPipe listener receives runtime events ~117 ms after the fact (r56d), and polling
/// <c>GetGCMemoryInfo</c> from each read loses every collection between two reads — which puts holes
/// in a ring whose slots are indexed by cycle number.
/// </para>
/// <para>
/// <b>Write ordering is Go's, verbatim.</b> <see cref="Observe"/> writes
/// <c>pauseNs[observed % 256]</c> and then increments <c>observed</c>, exactly as
/// <c>gcMarkTermination</c> writes <c>pause_ns[numgc%256]</c> before incrementing <c>numgc</c>. That
/// is what makes <c>MemStats</c>' documented "the most recent pause is at
/// <c>PauseNs[(NumGC+255)%256]</c>" true by construction, and what makes <c>ReadGCStats</c>' backwards
/// walk line up with it by construction rather than by agreement.
/// </para>
/// <para>
/// <b><c>NumGC</c> is <see cref="Observed"/>, not <c>CollectionCount</c>.</b> Both surfaces read one
/// snapshot taken under one lock, so there is no second source to disagree with. The cost is that
/// <c>NumGC</c> can lag the true gen2 count by at most one collection, for at most the finalizer's
/// scheduling latency — and understating is the safe direction, because a pause the recorder did not
/// observe is never invented. <see cref="Drain"/> closes the lag at the one boundary Go's tests
/// depend on: <c>runtime.GC()</c> and <c>debug.FreeOSMemory()</c> both drain before returning.
/// </para>
/// <para>
/// <b>Measured boundaries (§7.1.5, i7-5820K, .NET 9.0.19).</b> The lag bound holds exactly in both
/// regimes a program actually meets — drained (44 collections, 44 observed, lag 0 after the drain)
/// and naturally paced (54 collections in 4.0 s, all 54 observed). It degrades only under gen2
/// collections forced BACK TO BACK with no drain between them, where the finalizer thread never gets
/// a turn and <see cref="Observe"/> advances by at most one per call (17.8–25.6 % observed). That is a
/// harness pattern, and the one harness pattern that matters — <c>runtime.GC()</c> in a loop — is
/// drained by construction. The negative control holds too: a strongly-referenced sentinel is never
/// finalized, so the live readings are evidence of the resurrection and not of some other wake-up.
/// </para>
/// <para>
/// <b>Cost (§3.3, §7.1.3).</b> Always on, armed from the <c>runtime</c> (and <c>runtime/debug</c>)
/// module initializer — ⟨OQ-1⟩ as ratified. One ~24 B sentinel permanently cycling through the
/// finalization queue, two fixed 2 KiB arrays for the process lifetime, and one finalizer run plus one
/// <c>GetGCMemoryInfo</c> call per <b>gen2 collection</b>. Measured against a disarmed control over
/// 4 × 3 × 60 forced collections, the per-collection overhead is <b>not resolvable</b> against a
/// 1.25–1.64 ms gen2 collection (−0.4 %, +0.6 %, −0.9 %, −3.4 % — the sign changes run to run). The
/// two rejected activation models are rejected on correctness, not cost: arming on first read would
/// make <c>NumGC</c> LESS true than it is today, and arming only under the test host would make a
/// measurement surface answer differently under test than in production, which is the one shape a
/// measurement surface must never have (<c>expvar</c>'s <c>/debug/vars</c> is a production consumer).
/// </para>
/// <para>
/// <b>The escape hatch — <c>GO2CS_GC_PAUSE_HISTORY=0</c>.</b> For anyone who MEASURES a problem, not a
/// configuration knob. With it set, the recorder never arms and both surfaces answer exactly as they
/// did before this arc: <c>NumGC</c> from <c>GC.CollectionCount(GC.MaxGeneration)</c>,
/// <c>PauseTotalNs</c> from <c>GC.GetTotalPauseDuration()</c> (all generations), an empty ring, and
/// <c>LastGC = 0</c>. <c>HeapReleased</c> and <c>NumForcedGC</c> keep working: the high-water mark is
/// still advanced by every <c>ReadMemStats</c> call (§4.1) and forced cycles are counted whether or
/// not the sentinel is armed.
/// </para>
/// <para>
/// <b>Two approximations, named rather than hidden.</b> (1) The CLR publishes no per-GC wall-clock end
/// stamp, so the end time is <see cref="DateTime.UtcNow"/> read inside <see cref="Observe"/> — the
/// recorder's own read, which is the honest approximation. (2) A BACKGROUND gen2 collection reports
/// TWO entries in <c>PauseDurations</c> rather than one stop-the-world pause (measured §7.1.6:
/// <c>index=101, generation=2, concurrent=True, [2,000 ns, 149,000 ns]</c>), so the ring's single
/// number is their SUM.
/// </para>
/// <para>
/// <b>The overload rule (§7.1.6, refinement 4) — the default is wrong.</b> <c>GC.GetGCMemoryInfo()</c>
/// defaults to <c>GCKind.Any</c>, which reports the latest collection of ANY kind. Measured: after a
/// forced gen2 (<c>Any</c>: index 86, generation 2) followed by a single forced gen0,
/// <c>Any</c> reports index 87, generation 0 while <c>FullBlocking</c> still reports index 86,
/// generation 2. A recorder reading <c>Any</c> unconditionally would write an EPHEMERAL collection's
/// pause into a gen2 ring slot. Neither single overload covers both cases either — <c>FullBlocking</c>
/// misses a background gen2, <c>Background</c> misses a blocking one — so
/// <see cref="TryReadGen2Info"/> reads <c>Any</c>, accepts it only when
/// <c>Generation == GC.MaxGeneration</c>, and otherwise takes whichever of
/// <c>FullBlocking</c>/<c>Background</c> carries the higher <c>Index</c>.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public static class GcPauseRecorder
{
    /// <summary>
    /// The ring's fixed length — <c>len(runtime.MemStats.PauseNs)</c>, which is Go's own 256 and is
    /// what <c>ReadGCStats</c>' <c>2n+3</c> buffer arithmetic is sized against.
    /// </summary>
    public const int RingLength = 256;

    // The escape hatch of ⟨OQ-1⟩. Read once, at Arm.
    private const string DisableVariable = "GO2CS_GC_PAUSE_HISTORY";

    // The ring: fixed storage, allocated once, never reallocated — §8.2's landing precondition is
    // that a READ of this surface allocates nothing, and a ring copied into a fresh array per read
    // (2 KiB) is exactly the regression the guard exists to catch.
    private static readonly ulong[] s_pauseNs = new ulong[RingLength];
    private static readonly ulong[] s_pauseEndUnixNs = new ulong[RingLength];

    // Guards the six pieces of state §3.2 names: the two ring arrays, observed, lastGcEndUnixNs,
    // pauseTotalNs — plus the committed high-water mark of §4.1, sampled at the same points.
    private static readonly object s_lock = new();

    private static long s_baseline;             // CollectionCount(MaxGeneration) when Arm ran
    private static long s_observed;             // gen2 collections this recorder has SEEN
    private static ulong s_lastGcEndUnixNs;
    private static ulong s_pauseTotalNs;

    private static long s_committed;            // latest TotalCommittedBytes reading
    private static long s_heapSize;             // latest HeapSizeBytes reading (Go's NextGC)
    private static long s_committedHighWater;   // §4.1's running maximum
    private static bool s_hasCommittedSample;

    private static int s_numForcedGC;           // §4.2 — GC cycles the PROGRAM asked for
    private static bool s_armed;
    private static bool s_enabled;

    /// <summary>
    /// True when the recorder is armed and running. False when <c>GO2CS_GC_PAUSE_HISTORY=0</c> put
    /// both surfaces back on their pre-recorder answers.
    /// </summary>
    public static bool Enabled => Volatile.Read(ref s_enabled);

    /// <summary>
    /// True once a committed-bytes reading exists that <c>ReadMemStats</c> can reuse instead of
    /// fetching its own. False while disabled, so an opted-out program keeps reading fresh figures.
    /// </summary>
    public static bool HasCommittedSample => Enabled && Volatile.Read(ref s_hasCommittedSample);

    /// <summary>
    /// Arms the recorder. Idempotent, and safe to call from more than one module initializer — both
    /// <c>runtime</c> and <c>runtime/debug</c> do, because either assembly can be the first one a
    /// program touches and the surface must not depend on which.
    /// </summary>
    public static void Arm()
    {
        lock (s_lock)
        {
            if (s_armed)
                return;

            s_armed = true;

            if (IsDisabledByEnvironment())
                return;

            // The baseline is what keeps this recorder from ever FABRICATING. Seeding `observed`
            // from CollectionCount instead would claim a ring the recorder cannot fill — it would
            // report real collections it never saw with a zero pause, which is a plausible-looking
            // invented number and precisely what §4.3's rule exists to refuse. Counting from the
            // arm point instead UNDERSTATES NumGC by however many gen2 collections happened before
            // the runtime assembly was first touched (normally none), and understating is the safe
            // direction — the same trade §3.4 takes for the finalizer lag.
            s_baseline = GC.CollectionCount(GC.MaxGeneration);

            Volatile.Write(ref s_enabled, true);

            // Deliberately unrooted: nothing may hold a strong reference to the sentinel, or it is
            // never collected and its finalizer never runs. The negative control in
            // GolibTests.GcMeasurementSurfaceProbes proves that half.
            _ = new Sentinel();
        }
    }

    /// <summary>
    /// §3.2's single write path, idempotent per collection: it records at most ONE collection per
    /// call, and only when the CLR's gen2 count has advanced past what this recorder has seen.
    /// </summary>
    /// <remarks>
    /// The idempotence is what lets <see cref="Drain"/> call this directly right after the finalizer
    /// may already have: neither can double-record what the other took. It is also what filters the
    /// sentinel's first callbacks, which fire on ephemeral collections before it is promoted.
    /// </remarks>
    public static void Observe()
    {
        if (!Enabled)
            return;

        long trueCount = GC.CollectionCount(GC.MaxGeneration) - Interlocked.Read(ref s_baseline);

        // Step 1 — has the gen2 count advanced past what this recorder has seen?
        if (trueCount <= Interlocked.Read(ref s_observed))
            return;

        // Step 2 — read the collection's facts, from the overload rule the measurements corrected
        // §3.2 with (see the type remarks).
        if (!TryReadGen2Info(out GCMemoryInfo info))
            return;

        ulong pause = 0;

        // A background gen2 reports two entries; the ring's single number is their sum. For a
        // blocking collection PauseDurations.Length is still 2 with the second entry zero, so
        // summing is right in both cases and costs nothing.
        foreach (TimeSpan duration in info.PauseDurations)
            pause += (ulong)(duration.Ticks * 100L);

        ulong endUnixNs = (ulong)((DateTime.UtcNow - DateTime.UnixEpoch).Ticks * 100L);

        lock (s_lock)
        {
            long observed = s_observed;

            if (trueCount <= observed)
                return;

            // Step 3 — Go's ordering: write the slot, THEN advance the counter.
            s_pauseNs[observed % RingLength] = pause;
            s_pauseEndUnixNs[observed % RingLength] = endUnixNs;
            s_lastGcEndUnixNs = endUnixNs;
            s_pauseTotalNs += pause;
            Interlocked.Exchange(ref s_observed, observed + 1);

            // Inside the same hold, so a reader that sees this collection's ring entry also sees the
            // committed figure that belongs to it. SampleCommitted takes no lock of its own.
            SampleCommitted(info.TotalCommittedBytes, info.HeapSizeBytes);
        }
    }

    /// <summary>
    /// §3.4's mitigation: wait the finalizer out, then observe directly. <c>runtime.GC()</c> and
    /// <c>debug.freeOSMemory()</c> both end with this, so <c>NumGC</c> is current when they return —
    /// which is precisely the state Go's tests read it in.
    /// </summary>
    public static void Drain()
    {
        if (!Enabled)
            return;

        GC.WaitForPendingFinalizers();
        Observe();
    }

    /// <summary>
    /// Counts one GC cycle the PROGRAM forced — <c>runtime.GC()</c> or <c>debug.FreeOSMemory()</c>.
    /// </summary>
    /// <remarks>
    /// Go documents <c>NumForcedGC</c> as "the number of GC cycles that were forced by the
    /// application calling the GC function", which is a fact about the program rather than about the
    /// collector — so the managed model can count exactly it, and counts it whether or not the pause
    /// recorder is armed.
    /// </remarks>
    public static void NoteForcedGC()
    {
        Interlocked.Increment(ref s_numForcedGC);
    }

    /// <summary>
    /// Records a committed-bytes reading and advances §4.1's high-water mark.
    /// </summary>
    /// <remarks>
    /// Called from <see cref="Observe"/> for every gen2 collection, and from <c>ReadMemStats</c> on
    /// the fallback path where no recorder sample exists yet. Both feed the same high-water mark, so
    /// a program that never calls <c>ReadMemStats</c> still tracks it through the recorder, and a
    /// program that calls <c>ReadMemStats</c> in a tight loop pays one comparison.
    /// </remarks>
    public static void SampleCommitted(long committedBytes, long heapSizeBytes)
    {
        if (committedBytes < 0)
            committedBytes = 0;

        if (heapSizeBytes < 0)
            heapSizeBytes = 0;

        Interlocked.Exchange(ref s_committed, committedBytes);
        Interlocked.Exchange(ref s_heapSize, heapSizeBytes);
        Volatile.Write(ref s_hasCommittedSample, true);

        long highWater = Interlocked.Read(ref s_committedHighWater);

        while (committedBytes > highWater)
        {
            long previous = Interlocked.CompareExchange(ref s_committedHighWater, committedBytes, highWater);

            if (previous == highWater)
                break;

            highWater = previous;
        }
    }

    /// <summary>
    /// Fills the caller's <c>MemStats</c> ring images IN RING ORDER (slot <c>N%256</c> is the pause of
    /// the <c>N%256</c>th most recent cycle, exactly as Go fills them) and returns the matching
    /// scalars — all under ONE lock, so <c>ReadMemStats</c> and <c>ReadGCStats</c> cannot see a torn
    /// or half-updated ring.
    /// </summary>
    /// <remarks>
    /// Allocation-free by construction, and that is a landing precondition rather than a nicety
    /// (§8.2): <c>net/textproto</c>'s banked <c>TestReadMIMEHeaderAllocations</c> brackets each header
    /// read between two <c>ReadMemStats</c> calls and asserts under 32,768 B per iteration, so
    /// anything this path allocates is charged to <c>ReadMIMEHeader</c>. <c>array&lt;T&gt;</c> is a
    /// readonly struct over a backing array the <c>MemStats</c> constructor already allocated and
    /// <c>ToSpan()</c> is a window onto it, so the copy writes into storage the caller already owns.
    /// A destination shorter than the ring (a default-constructed <c>MemStats</c> whose arrays were
    /// never sized) is honored by writing only what fits rather than by panicking.
    /// </remarks>
    public static GcPauseSnapshot ReadInto(array<uint64> pauseNs, array<uint64> pauseEnd)
    {
        lock (s_lock)
        {
            CopyRing(s_pauseNs, pauseNs);
            CopyRing(s_pauseEndUnixNs, pauseEnd);

            return SnapshotLocked();
        }
    }

    /// <summary>
    /// Fills <paramref name="destination"/> the way <c>readGCStats</c>' packed layout wants it — the
    /// <c>n = min(observed, 256)</c> most recent pauses MOST RECENT FIRST in
    /// <c>destination[0..n)</c>, then their end times in <c>destination[n..2n)</c> — and returns the
    /// matching scalars from the same lock hold.
    /// </summary>
    /// <remarks>
    /// The backwards walk is <c>j = (observed - 1 - i) mod 256</c>, which is the exact inverse of the
    /// write ordering above and therefore the same walk Go's <c>readGCStats_m</c> performs. The caller
    /// supplies the destination so this allocates nothing.
    /// </remarks>
    public static GcPauseSnapshot ReadMostRecentFirst(ulong[] destination, out int count)
    {
        ArgumentNullException.ThrowIfNull(destination);

        lock (s_lock)
        {
            long observed = s_observed;
            int n = (int)Math.Min(observed, RingLength);

            if (n * 2 > destination.Length)
                n = destination.Length / 2;

            for (int i = 0; i < n; i++)
            {
                long slot = (observed - 1 - i) % RingLength;

                if (slot < 0)
                    slot += RingLength;

                destination[i] = s_pauseNs[slot];
                destination[n + i] = s_pauseEndUnixNs[slot];
            }

            count = n;

            return SnapshotLocked();
        }
    }

    // ------------------------------------------------------------------------------------------

    private static GcPauseSnapshot SnapshotLocked()
    {
        long committed = Interlocked.Read(ref s_committed);
        long highWater = Interlocked.Read(ref s_committedHighWater);

        // §4.1's formulation — and a deliberate departure from the commissioning ruling's literal
        // "cumulative TotalCommittedBytes decrease", ratified as ⟨OQ-2⟩. Go documents HeapReleased as
        // "bytes of physical memory returned to the OS ... [that] has not yet been reacquired for the
        // heap": a CURRENT quantity that goes DOWN when the heap grows back. A monotone lifetime
        // total is a different quantity wearing the same name — measured (§7.1.8) drifting ~33.6 MB
        // per release/reacquire cycle, and already 134 MB above the truth after four cycles of a test
        // that runs in 37 ms, while this form falls back to 0 exactly as Go's field does.
        //
        // Two honesty notes that belong here and not only in the design:
        //   - GCMemoryInfo.TotalCommittedBytes is a SNAPSHOT AS OF THE LAST GC, not a live figure. So
        //     HeapReleased is fresh exactly when a collection has just run — which is the case at
        //     every point TestFreeOSMemory reads it, since FreeOSMemory collects and then drains this
        //     recorder. Elsewhere it is as stale as the last observed gen2 collection.
        //   - HeapIdle >= HeapReleased, an invariant MemStats' own doc comments assert, can be false
        //     here: HeapIdle is instantaneous (committed - live) while HeapReleased is a difference
        //     against a historical high-water mark, so after a large release the second can exceed
        //     the first. Clamping is REFUSED — it would make a measured number smaller to satisfy a
        //     relation the managed model does not have.
        ulong heapReleased = highWater > committed ? (ulong)(highWater - committed) : 0;

        if (Volatile.Read(ref s_enabled))
        {
            return new GcPauseSnapshot(
                numGC: (ulong)s_observed,
                ringCount: (int)Math.Min(s_observed, RingLength),
                lastGcEndUnixNs: s_lastGcEndUnixNs,
                pauseTotalNs: s_pauseTotalNs,
                committedBytes: (ulong)committed,
                heapSizeBytes: (ulong)Interlocked.Read(ref s_heapSize),
                heapReleased: heapReleased,
                numForcedGC: (uint)Volatile.Read(ref s_numForcedGC));
        }

        // GO2CS_GC_PAUSE_HISTORY=0 — the pre-recorder answers, verbatim. NumGC stays the real gen2
        // count rather than dropping to zero (destroying a fact the CLR genuinely measures is what
        // §1.2's anti-laundering reasoning refuses), PauseTotalNs stays the ALL-GENERATION total the
        // surface reported before ⟨OQ-5⟩, and the ring reports empty — which is exactly today's
        // state, including today's failing length assertions in TestReadGCStats.
        return new GcPauseSnapshot(
            numGC: (ulong)GC.CollectionCount(GC.MaxGeneration),
            ringCount: 0,
            lastGcEndUnixNs: 0,
            pauseTotalNs: (ulong)(GC.GetTotalPauseDuration().Ticks * 100L),
            committedBytes: (ulong)committed,
            heapSizeBytes: (ulong)Interlocked.Read(ref s_heapSize),
            heapReleased: heapReleased,
            numForcedGC: (uint)Volatile.Read(ref s_numForcedGC));
    }

    private static void CopyRing(ulong[] source, array<uint64> destination)
    {
        nint length = destination.Length;

        if (length <= 0)
            return;

        int count = (int)Math.Min(length, RingLength);

        source.AsSpan(0, count).CopyTo(destination.ToSpan());
    }

    // The overload rule of §7.1.6, measured rather than read off the API's documentation: `Any` means
    // "the latest collection of ANY kind", so it walks off the gen2 the moment an ephemeral
    // collection lands, and neither FullBlocking nor Background alone covers both flavors of gen2.
    private static bool TryReadGen2Info(out GCMemoryInfo info)
    {
        info = GC.GetGCMemoryInfo();

        if (info.Generation == GC.MaxGeneration)
            return true;

        GCMemoryInfo blocking = GC.GetGCMemoryInfo(GCKind.FullBlocking);
        GCMemoryInfo background = GC.GetGCMemoryInfo(GCKind.Background);

        info = background.Index > blocking.Index ? background : blocking;

        return info.Generation == GC.MaxGeneration;
    }

    private static bool IsDisabledByEnvironment()
    {
        string? setting = Environment.GetEnvironmentVariable(DisableVariable);

        if (string.IsNullOrEmpty(setting))
            return false;

        return setting.Equals("0", StringComparison.Ordinal) ||
               setting.Equals("false", StringComparison.OrdinalIgnoreCase) ||
               setting.Equals("off", StringComparison.OrdinalIgnoreCase);
    }

    // The mechanism itself. Nothing strongly references an instance of this (see Arm), so every
    // collection that condemns its generation makes it unreachable, runs this finalizer, and the
    // ReRegisterForFinalize puts it back for the next one. After its first promotion that generation
    // is gen2, which is why Observe's step 1 — "has the gen2 count advanced?" — is also the filter
    // for the ephemeral callbacks that fire before promotion.
    private sealed class Sentinel
    {
        ~Sentinel()
        {
            try
            {
                Observe();
            }
            catch
            {
                // An exception escaping a finalizer takes the process down. A recorder that could do
                // that would be a worse defect than the missing measurement it exists to supply.
            }

            if (Volatile.Read(ref s_enabled) && !Environment.HasShutdownStarted)
                GC.ReRegisterForFinalize(this);
        }
    }
}

/// <summary>
/// One consistent reading of <see cref="GcPauseRecorder"/>'s scalars — every field taken under a
/// single lock hold, so the two surfaces that consume it cannot disagree.
/// </summary>
/// <remarks>
/// A <c>readonly struct</c> of scalars, deliberately: <c>ReadMemStats</c> must not allocate (§8.2),
/// and a snapshot CLASS allocated per call is one of the two regressions the named GolibTests guard
/// exists to catch.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public readonly struct GcPauseSnapshot
{
    internal GcPauseSnapshot(ulong numGC, int ringCount, ulong lastGcEndUnixNs, ulong pauseTotalNs, ulong committedBytes, ulong heapSizeBytes, ulong heapReleased, uint numForcedGC)
    {
        NumGC = numGC;
        RingCount = ringCount;
        LastGcEndUnixNs = lastGcEndUnixNs;
        PauseTotalNs = pauseTotalNs;
        CommittedBytes = committedBytes;
        HeapSizeBytes = heapSizeBytes;
        HeapReleased = heapReleased;
        NumForcedGC = numForcedGC;
    }

    /// <summary>Completed GC cycles — gen2 collections, per §2's one definition.</summary>
    public ulong NumGC { get; }

    /// <summary>Valid ring entries: <c>min(NumGC, 256)</c>, and 0 while the recorder is disabled.</summary>
    public int RingCount { get; }

    /// <summary>End of the last observed gen2 collection, nanoseconds since the Unix epoch.</summary>
    public ulong LastGcEndUnixNs { get; }

    /// <summary>The ring's running sum — gen2 pause time (⟨OQ-5⟩).</summary>
    public ulong PauseTotalNs { get; }

    /// <summary><c>GCMemoryInfo.TotalCommittedBytes</c> as of the sample — Go's <c>Sys</c>/<c>HeapSys</c>.</summary>
    public ulong CommittedBytes { get; }

    /// <summary><c>GCMemoryInfo.HeapSizeBytes</c> as of the sample — Go's <c>NextGC</c>.</summary>
    public ulong HeapSizeBytes { get; }

    /// <summary><c>max(0, committedHighWater - currentCommitted)</c> — §4.1.</summary>
    public ulong HeapReleased { get; }

    /// <summary>GC cycles the program forced through <c>runtime.GC()</c>/<c>debug.FreeOSMemory()</c>.</summary>
    public uint NumForcedGC { get; }
}
