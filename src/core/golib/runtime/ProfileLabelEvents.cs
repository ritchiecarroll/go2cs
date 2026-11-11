// ProfileLabelEvents.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace go.golib;

// ---------------------------------------------------------------------------------------------
// PROFILE-LABEL EVENTS — how an opt-in CPU sampler learns which labels a sampled thread carried.
// Section 9.2 step 3 and the labels piece of section 11.6, docs/phase4/DESIGN-managed-profiling.md.
//
// Go's SIGPROF handler reads the running goroutine's labels at the moment it samples
// (`tagPtr = &gp.m.curg.labels`). The managed sampler reads its samples after the profile stops,
// from an EventPipe trace, so reading a goroutine's labels then would answer with the labels it has
// at the end, not the ones it had when sampled (pprof.Do sets and restores them). Instead, every
// label change on a thread is an event in the SAME trace: the event carries its thread id and
// timestamp, and the sampler tags each sample with the latest label event on its thread.
//
// The labels object (runtime/pprof's *labelMap, as the unsafe.Pointer runtime_setProfLabel stores)
// cannot ride in an event, so it travels as an id. The table holding the objects is filled only
// while a session has this provider enabled, and the sampler clears it at each start and stop.
// With no session listening, a label change costs one IsEnabled check and nothing else.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// The EventSource a CPU sampler enables to see profile-label changes, one event per change, on the
/// thread that made it.
/// </summary>
[EventSource(Name = ProviderName)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class ProfileLabelEvents : EventSource
{
    /// <summary>The provider name a sampler enables.</summary>
    public const string ProviderName = "go2cs-ProfileLabels";

    /// <summary>The event id of <see cref="LabelsSet"/>.</summary>
    public const int LabelsSetEventId = 1;

    /// <summary>The single instance.</summary>
    public static readonly ProfileLabelEvents Log = new();

    private static readonly object s_gate = new();
    private static readonly Dictionary<object, long> s_ids = new(ReferenceEqualityComparer.Instance);
    private static readonly List<object> s_labels = [];

    private ProfileLabelEvents() { }

    /// <summary>The calling thread's labels became the object <paramref name="id"/> names (0: none).</summary>
    [Event(LabelsSetEventId, Level = EventLevel.Informational)]
    public void LabelsSet(long id) => WriteEvent(LabelsSetEventId, id);

    /// <summary>Records the calling thread's new labels, when a session is listening.</summary>
    internal static void Record(object? labels)
    {
        if (!Log.IsEnabled())
            return;

        Log.LabelsSet(labels is null ? 0 : IdOf(labels));
    }

    /// <summary>The labels object an event's id names, or <c>null</c> for 0 or an id this table does
    /// not hold.</summary>
    public static object? Resolve(long id)
    {
        lock (s_gate)
        {
            return id > 0 && id <= s_labels.Count ? s_labels[(int)(id - 1)] : null;
        }
    }

    /// <summary>Forgets every labels object recorded so far. A sampler calls it when a profile starts
    /// and after it has read one.</summary>
    public static void Reset()
    {
        lock (s_gate)
        {
            s_ids.Clear();
            s_labels.Clear();
        }
    }

    private static long IdOf(object labels)
    {
        lock (s_gate)
        {
            if (s_ids.TryGetValue(labels, out long id))
                return id;

            s_labels.Add(labels);
            id = s_labels.Count;
            s_ids.Add(labels, id);
            return id;
        }
    }
}
