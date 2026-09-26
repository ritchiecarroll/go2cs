// AllocationCounter.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;
using System.Text;

namespace go;

/// <summary>
/// Counts the managed heap objects the go2cs runtime allocates on behalf of Go semantics — go2cs's
/// own analogue of Go's <c>runtime.MemStats.Mallocs</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists.</b> Go's <c>testing.AllocsPerRun</c> reports a malloc COUNT, and the CLR
/// publishes no in-process count — measured, not assumed (see the survey on
/// <c>testing.AllocsPerRun</c>): the whole public GC surface exposes byte totals only, the one event
/// carrying an object count raises nothing to an in-process listener, and runtime events arrive
/// asynchronously besides. A byte figure is therefore what the platform can supply, and a byte figure
/// is not the number the assert is about.
/// </para>
/// <para>
/// The count that IS obtainable is go2cs's own, for the same reason Go's is Go's own: Mallocs is not
/// a platform facility there either — it is a counter the RUNTIME keeps at its own allocation sites.
/// golib is that runtime here, so counting at golib's allocation sites is the exact structural
/// mirror, not an approximation of one.
/// </para>
/// <para>
/// <b>What is counted — CLR heap objects, not "Go allocations".</b> The unit is the managed object,
/// because that is the quantity that can be AUDITED against
/// <c>GC.GetAllocatedBytesForCurrentThread</c>; a "logical Go allocation" would be a model, and a
/// model is exactly the kind of quiet fiction this counter exists to replace. Where one Go malloc
/// becomes two CLR objects the count says two. The canonical instance is <c>ж&lt;T&gt;</c> over an
/// unmanaged <typeparamref name="T"/>: Go's <c>&amp;x</c> is one malloc, while the box additionally
/// allocates the one-element pinnable slot its address stability requires, so it counts 2. That
/// difference is real allocation behavior and belongs in the number.
/// </para>
/// <para>
/// <b>Coverage — and what is deliberately NOT counted.</b> The counted set is every allocation site in
/// golib, enumerated file by file (the census is recorded in
/// <c>docs/phase4/DESIGN-allocation-counting.md</c>). It does NOT include, and cannot:
/// </para>
/// <list type="bullet">
///   <item><description>
///     allocations the C# compiler emits in CONVERTED code rather than in golib — closure display
///     classes, delegate objects for func values, <c>params object[]</c> arrays at variadic call
///     sites, and boxing at interface-conversion sites;
///   </description></item>
///   <item><description>
///     allocations INTERNAL to a BCL method golib calls (<c>Encoding.GetString</c>'s working buffers,
///     a <c>List&lt;T&gt;</c> growth, a <c>Task</c> the thread pool mints) — golib charges the object
///     it asked for, not the ones that object allocates for itself;
///   </description></item>
///   <item><description>
///     allocations in hand-owned <c>*_impl.cs</c> package code that does not route through golib.
///   </description></item>
/// </list>
/// <para>
/// That residual is the reason a count may never be reported as if it were total. The consumer —
/// <c>testing.AllocsPerRun</c> — therefore keeps measuring BYTES alongside the count and cross-checks
/// them: bytes with a zero count means allocations occurred that this counter did not see, and the
/// zero is suppressed rather than reported, because an uncounted allocation reported as zero is a
/// FALSE PASS, which is worse than the byte figure it replaced.
/// </para>
/// <para>
/// <b>Thread scoping.</b> The count is per-thread, exactly like the byte measurement it accompanies:
/// <c>GC.GetAllocatedBytesForCurrentThread</c> is inherently thread-scoped, and that scoping is what
/// stands in for the <c>runtime.GOMAXPROCS(1)</c> pinning Go's AllocsPerRun uses to keep other
/// goroutines out of its measurement. A process-global counter would reintroduce exactly the
/// pollution the byte instrument is already free of, and would cost an interlocked round trip per
/// allocation to do it. A thread-static increment needs no synchronization at all, since one thread
/// is the only writer of its own slot.
/// </para>
/// <para>
/// <b>Cost when off.</b> Counting is off unless a host turns it on (<see cref="Enable"/>), which the
/// go2cs test host does at startup and nothing else does. Disabled, every site pays one read of a
/// static <see cref="bool"/> and one never-taken, perfectly-predicted branch — the thread-static slot
/// is not touched at all.
/// </para>
/// </remarks>
public static class AllocationCounter
{
    // The process-wide gate. Deliberately a plain static rather than a `static readonly` the JIT
    // could fold to a constant: folding would require the value to be known before the first golib
    // type is initialized, and golib types are constructed while the host is still parsing its own
    // arguments and building its registry. A gate that is merely CHEAP and always correct beats one
    // that is free and racing type initialization -- and the measured cost of the read plus its
    // never-taken branch is below the noise floor of the allocation it guards (see the overhead
    // measurement in docs/phase4/DESIGN-allocation-counting.md).
    private static bool s_enabled;

    // Per-thread, for the reason given in the remarks: it mirrors the thread scoping of the byte
    // measurement this count accompanies, and needs no synchronization because a thread is the sole
    // writer of its own slot.
    [ThreadStatic]
    private static long t_count;

    /// <summary>Zeroes this thread's tally before a pooled thread runs its next goroutine.</summary>
    internal static void ResetThread() => t_count = 0;

    /// <summary>
    /// Gets whether allocation counting is currently enabled for this process.
    /// </summary>
    public static bool Enabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_enabled;
    }

    /// <summary>
    /// Enables allocation counting for the whole process. Called by the go2cs test host at startup;
    /// there is no disable, because a run that begins counting counts for its lifetime.
    /// </summary>
    public static void Enable() => s_enabled = true;

    /// <summary>
    /// Gets the number of golib-allocated managed objects charged to the CALLING thread since the
    /// process started counting. Deltas of this value are the measurement; the absolute value has no
    /// meaning of its own.
    /// </summary>
    public static long CurrentThreadCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => t_count;
    }

    /// <summary>
    /// Charges one allocated managed object to the calling thread.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Count()
    {
        if (s_enabled)
            t_count++;
    }

    /// <summary>
    /// Charges <paramref name="objects"/> allocated managed objects to the calling thread, for a site
    /// that allocates a fixed group in one step (a box and the pinnable slot it owns, a slice header's
    /// backing array plus the array wrapper around it).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Count(int objects)
    {
        if (s_enabled)
            t_count += objects;
    }

    /// <summary>
    /// Allocates a backing array of <paramref name="length"/> elements and charges it.
    /// </summary>
    /// <remarks>
    /// This exists so that counting a backing store is a SUBSTITUTION rather than an insertion: a
    /// golib site cannot allocate through it and forget to charge, and the census stays mechanically
    /// checkable — <c>NoUncountedBackingAllocations</c> greps golib for raw <c>new T[…]</c> backing
    /// creations and fails on any that did not come through here. A zero length still allocates a
    /// real (uncached) array object on the CLR, so it is charged like any other; only
    /// <c>Array.Empty&lt;T&gt;()</c>, written <c>[]</c>, allocates nothing and is charged nothing.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] NewArray<T>(int length)
    {
        if (s_enabled)
            t_count++;

        return new T[length];
    }

    /// <inheritdoc cref="NewArray{T}(int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] NewArray<T>(nint length)
    {
        if (s_enabled)
            t_count++;

        return new T[length];
    }

    /// <inheritdoc cref="NewArray{T}(int)"/>
    /// <remarks>
    /// Takes <see cref="ulong"/> unchanged rather than narrowing to <see cref="nint"/>, so that an
    /// absurd length raises exactly the exception <c>new T[length]</c> always raised here — a cast
    /// would wrap it negative and change which one.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] NewArray<T>(ulong length)
    {
        if (s_enabled)
            t_count++;

        return new T[length];
    }

    /// <summary>
    /// Copies <paramref name="source"/> into a freshly allocated array and charges it.
    /// </summary>
    /// <remarks>
    /// <see cref="ReadOnlySpan{T}.ToArray"/> returns <c>Array.Empty&lt;T&gt;()</c> for an empty span
    /// — no object reaches the heap — so an empty copy is deliberately charged nothing. Anything
    /// else would report an allocation that did not happen, which is the same class of untruth as
    /// missing one that did.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] CopyOf<T>(scoped ReadOnlySpan<T> source)
    {
        if (s_enabled && source.Length > 0)
            t_count++;

        return source.ToArray();
    }

    /// <summary>
    /// Materializes a <see cref="string"/> from UTF-16 characters and charges it.
    /// </summary>
    /// <remarks>
    /// The empty case is charged nothing: <c>new string(char[0])</c> returns the interned
    /// <see cref="string.Empty"/>, so no object reaches the heap — the same rule
    /// <see cref="CopyOf{T}"/> applies to <c>Array.Empty</c>. A null array is the empty case too,
    /// which the pattern below handles rather than dereferencing: <c>new string((char[])null)</c>
    /// yields <see cref="string.Empty"/>, and counting must never turn a legal call into a throw.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NewString(char[] value)
    {
        if (s_enabled && value is { Length: > 0 })
            t_count++;

        return new string(value);
    }

    /// <summary>
    /// Decodes UTF-8 <paramref name="utf8"/> bytes into a <see cref="string"/> and charges it.
    /// </summary>
    /// <inheritdoc cref="NewString(char[])" path="/remarks"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Utf8ToString(scoped ReadOnlySpan<byte> utf8)
    {
        if (s_enabled && utf8.Length > 0)
            t_count++;

        return Encoding.UTF8.GetString(utf8);
    }

    /// <summary>
    /// Encodes <paramref name="value"/> to a freshly allocated UTF-8 byte array and charges it.
    /// </summary>
    /// <remarks>
    /// This is the site every C# string literal reaching a <c>@string</c> passes through, so it is
    /// the single busiest member here. The empty case is charged nothing for the reason
    /// <see cref="CopyOf{T}"/> gives.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Utf8ToBytes(string? value)
    {
        if (s_enabled && value is { Length: > 0 })
            t_count++;

        return Encoding.UTF8.GetBytes(value ?? "");
    }
}
