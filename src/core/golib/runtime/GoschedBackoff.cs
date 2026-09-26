//******************************************************************************************************
//  GoschedBackoff.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR REPRESENTATIONS OF ANY KIND, either expressed or implied. Refer
//  to the License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

using System.Diagnostics;
using System.Threading;

namespace go.golib;

/// <summary>
/// The adaptive escalation behind <c>runtime.Gosched</c> (board finding 2026-08-21, ratified):
/// consecutive provably-inert yields escalate to <see cref="Thread.Sleep(int)"/> so a starved
/// peer thread can reach the processor.
/// </summary>
/// <remarks>
/// Converted goroutines are dedicated OS threads, so a bare <see cref="Thread.Yield"/> rotates the
/// KERNEL's run queue rather than Go's own. Linux lowers it to sched_yield(2), which CFS makes
/// near-inert for CPU-bound threads: sync/atomic's TestValueCompareAndSwapConcurrent — a strict
/// token-passing ring whose every handoff needs one specific thread scheduled — exceeded 45 minutes
/// on Linux against 183 s on Windows on the same silicon. Sleeping leaves the run queue entirely,
/// which is what actually lets the one thread that can make progress run.
///
/// Inertness is decided by TIMING, not by <see cref="Thread.Yield"/>'s return value alone, because
/// that value lies on Linux (measured 2026-08-21, both hosts, 12 logical processors: Windows idle
/// yields return false at ~300 ns and contended return true at ~4.1 µs; Linux returns true in both
/// states, ~431 ns idle vs ~6.5 µs contended). A 2 µs threshold sits in that gap with ≥7× margin
/// on each side, on each host: an inert yield is <c>!switched || elapsed &lt; 2 µs</c>.
///
/// The counter is per-thread (one goroutine per thread makes that per-goroutine) and resets on any
/// effective yield or escalation. There is deliberately NO time-spaced reset: a thread whose
/// sporadic Gosched calls are all inert accrues one 1 ms sleep per 64 such calls, a negligible cost
/// for sporadic use, and the clock reads a reset rule would add are the fast path this class must
/// not tax.
/// </remarks>
internal static class GoschedBackoff
{
    // Measured basis above: inert ≤ ~561 ns (p99) and effective ≥ ~4.1 µs (p50) on both hosts.
    private static readonly long s_inertYieldTicks = (long)(2_000e-9 * Stopwatch.Frequency);

    // 64 consecutive inert yields cost ~30 µs before the first escalation — instant against the
    // 1 ms sleep tier, yet far beyond what any effective-yield workload strings together.
    private const int EscalationThreshold = 64;

    [ThreadStatic]
    private static int t_consecutiveInertYields;

    /// <summary>Clears this thread's yield streak before a pooled thread runs its next goroutine.</summary>
    internal static void ResetThread() => t_consecutiveInertYields = 0;

    // Total escalations across all threads — a test-visible observation point, not a control input.
    private static long s_escalations;

    internal static long Escalations => Interlocked.Read(ref s_escalations);

    /// <summary>
    /// Yields the processor, escalating to a 1 ms sleep after <see cref="EscalationThreshold"/>
    /// consecutive inert yields by the calling thread.
    /// </summary>
    internal static void Yield()
    {
        long started = Stopwatch.GetTimestamp();
        bool switched = Thread.Yield();

        if (switched && Stopwatch.GetTimestamp() - started >= s_inertYieldTicks)
        {
            // An effective yield: another thread really ran. The ring is rotating on its own.
            t_consecutiveInertYields = 0;
            return;
        }

        if (++t_consecutiveInertYields < EscalationThreshold)
            return;

        // Yielding is provably not moving the scheduler — leave the run queue so a starved peer
        // can have the processor, then re-probe from a clean count.
        t_consecutiveInertYields = 0;
        Interlocked.Increment(ref s_escalations);
        Thread.Sleep(1);
    }
}
