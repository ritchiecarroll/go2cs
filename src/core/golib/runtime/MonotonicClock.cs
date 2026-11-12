// MonotonicClock.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System.Diagnostics;

namespace go.golib;

/// <summary>
/// The monotonic nanosecond clock Go's <c>runtime.nanotime</c> reports — the one source every
/// converted consumer of that clock reads.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this exists.</b> <c>runtime.nanotime1</c> is assembly in Go (a VDSO call on Linux,
/// <c>QueryPerformanceCounter</c> on Windows), so the converter emits a bodyless partial and
/// <c>PartialStubGenerator</c> fills it with a throw. That throw is not inert: <c>nanotime</c> is
/// read by <c>cpuprof</c>, <c>metrics</c>, <c>mgc</c>, <c>mgcmark</c>, <c>mgcpacer</c>,
/// <c>mprof</c>, <c>netpoll</c> and <c>debuglog</c>, so the first call into any of them dies —
/// <c>runtime/pprof</c>'s <c>StartCPUProfile</c> reaches it through
/// <c>SetCPUProfileRate</c> and takes the host down with a goroutine panic.
/// </para>
/// <para>
/// <b>Why it is truthful rather than a stand-in.</b> Go's contract for this clock is narrow and
/// entirely satisfiable here: a monotonic, nanosecond-denominated counter whose EPOCH IS
/// ARBITRARY, because only differences are ever observed. <see cref="Stopwatch"/> is the same
/// underlying source Go uses on Windows (<c>QueryPerformanceCounter</c>), so this is the platform's
/// own monotonic clock rather than a model of one.
/// </para>
/// <para>
/// <b>The scaling is the part that has to be right.</b> Ticks are scaled to nanoseconds with a
/// seconds/remainder split rather than the obvious <c>ticks * 1e9 / Frequency</c>: that product
/// overflows <see cref="long"/> for any real uptime (at a 10 MHz timer it passes
/// <c>long.MaxValue</c> in about 15 minutes), which would make the clock jump BACKWARDS — the one
/// property a monotonic clock may never lose. Splitting keeps <c>seconds * 1e9</c> and
/// <c>rem * 1e9 / Frequency</c> (with <c>rem &lt; Frequency</c>) each well inside range while
/// preserving full sub-tick resolution.
/// </para>
/// <para>
/// This is deliberately the same computation <c>time</c>'s hand-owned <c>runtimeNano</c> already
/// performs, because the two must agree: a converted program can compare a <c>time.Since</c>
/// against a runtime-sourced duration, and two independently-derived "monotonic" clocks would make
/// that comparison meaningless. Stated here rather than shared by reference because <c>time</c>
/// sits above <c>runtime</c> and cannot be depended on from below; if the two ever diverge, this
/// remark is the record that they were meant to be one clock.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public static class MonotonicClock
{
    /// <summary>
    /// Gets a monotonic reading in nanoseconds. The epoch is arbitrary; only differences between
    /// two readings are meaningful.
    /// </summary>
    public static long Nanoseconds()
    {
        long ticks = Stopwatch.GetTimestamp();
        long frequency = Stopwatch.Frequency;
        long seconds = ticks / frequency;
        long remainder = ticks % frequency;

        return seconds * 1_000_000_000L + remainder * 1_000_000_000L / frequency;
    }

    /// <summary>
    /// Gets a monotonic reading in <see cref="TicksPerSecond"/>-denominated ticks &#8212; the clock
    /// behind Go's <c>runtime.cputicks</c>. The epoch is <see cref="Nanoseconds"/>'s epoch, and that
    /// sharing is load-bearing; see the remarks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Why the shared epoch is load-bearing.</b> Go derives the tick rate rather than declaring
    /// it: <c>runtime.ticksPerSecond</c> computes
    /// <c>(nowTicks - startTicks) * 1e9 / (nowTime - startTime)</c> from a pair written down by
    /// <c>ticks.init</c>. That initializer is called from <c>schedinit</c>, which this corpus never
    /// reaches, so <c>startTicks</c> and <c>startTime</c> are both zero and the expression collapses
    /// to <c>Ticks() * 1e9 / Nanoseconds()</c>. Because both readings come from the same
    /// <see cref="Stopwatch"/> origin, that quotient is exactly <see cref="TicksPerSecond"/> &#8212;
    /// the clock's true rate. Two independently-originated clocks would make it an arbitrary number,
    /// and every duration Go converts through that rate would be wrong by the ratio of the epochs.
    /// </para>
    /// <para>
    /// It is also right if <c>ticks.init</c> ever DOES run: a real paired sample gives the same
    /// ratio, because the two clocks advance together by construction.
    /// </para>
    /// <para>
    /// Unlike <see cref="Nanoseconds"/> this needs no scaling, so it cannot overflow and is the
    /// cheaper reading; Go's own <c>cputicks</c> is likewise a raw counter (the TSC on x86) whose
    /// rate is discovered rather than assumed. Go's comment on it warns that it "is not guaranteed
    /// to be monotonic" across CPUs; <see cref="Stopwatch"/> gives us a stronger guarantee than the
    /// contract asks for, which is a safe direction to be wrong in.
    /// </para>
    /// </remarks>
    public static long Ticks() => Stopwatch.GetTimestamp();

    /// <summary>
    /// Gets the number of <see cref="Ticks"/> per second &#8212; the value
    /// <c>runtime.ticksPerSecond</c> derives, stated here so a guard can compare the derivation
    /// against the source of truth rather than against itself.
    /// </summary>
    public static long TicksPerSecond => Stopwatch.Frequency;
}
