// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using heap = container.heap_package;
using math = math_package;
using sort = sort_package;
using strings = strings_package;
using time = time_package;
using container;

partial class trace_package {

// MutatorUtil is a change in mutator utilization at a particular
// time. Mutator utilization functions are represented as a
// time-ordered []MutatorUtil.
partial struct MutatorUtil {
    public int64 Time;
    // Util is the mean mutator utilization starting at Time. This
    // is in the range [0, 1].
    public float64 Util;
}

partial struct UtilFlags /*num:nint*/;

public static UtilFlags UtilSTW => /* 1 << iota */ 1;
public static UtilFlags UtilBackground => 2;
public static UtilFlags UtilAssist => 4;
public static UtilFlags UtilSweep => 8;
public static UtilFlags UtilPerProc => 16;

// Set up a bunch of analysis state.
internal partial struct MutatorUtilizationV2_perP /*dyn*/ {
    // gc > 0 indicates that GC is active on this P.
    internal nint gc;
    // series the logical series number for this P. This
    // is necessary because Ps may be removed and then
    // re-added, and then the new P needs a new series.
    internal nint series;
}

internal partial struct MutatorUtilizationV2_procsCount /*dyn*/ {
    // time at which procs changed.
    internal int64 time;
    // n is the number of procs at that point.
    internal nint n;
}

// MutatorUtilizationV2 returns a set of mutator utilization functions
// for the given v2 trace, passed as an io.Reader. Each function will
// always end with 0 utilization. The bounds of each function are implicit
// in the first and last event; outside of these bounds each function is
// undefined.
//
// If the UtilPerProc flag is not given, this always returns a single
// utilization function. Otherwise, it returns one function per P.
public static slice<slice<MutatorUtil>> MutatorUtilizationV2(slice<ΔEvent> events, UtilFlags flags) {
    var @out = new slice<MutatorUtil>[]{}.slice();
    nint stw = 0;
    var ps = new MutatorUtilizationV2_perP[]{}.slice();
    var inGC = new map<GoID, bool>();
    var states = new map<GoID, GoState>();
    var bgMark = new map<GoID, bool>();
    var procs = new MutatorUtilizationV2_procsCount[]{}.slice();
    var seenSync = false;
    // Helpers.
    bool handleSTW(ΔRange r) => (UtilFlags)(flags & UtilSTW) != 0 && isGCSTW(r);
    bool handleMarkAssist(ΔRange r) => (UtilFlags)(flags & UtilAssist) != 0 && isGCMarkAssist(r);
    bool handleSweep(ΔRange r) => (UtilFlags)(flags & UtilSweep) != 0 && isGCSweep(r);
    // Iterate through the trace, tracking mutator utilization.
    ж<ΔEvent> lastEv = default!;
    foreach (var (i, _) in events) {
        var ev = Ꮡ(events, i);
        lastEv = ev;
        // Process the event.
        var exprᴛ1 = (~ev).Kind();
        if (exprᴛ1 == EventSync) {
            seenSync = true;
        }
        else if (exprᴛ1 == EventMetric) {
            do {
                var m = (~ev).Metric();
                if (m.Name != "/sched/gomaxprocs:threads"u8) {
                    break;
                }
                nint gomaxprocs = (nint)m.Value.Uint64();
                if (len(ps) > gomaxprocs) {
                    if ((UtilFlags)(flags & UtilPerProc) != 0) {
                        // End each P's series.
                        foreach (var (_, p) in ps.slice(gomaxprocs)) {
                            @out[p.series] = addUtil(@out[p.series], new MutatorUtil((int64)(~ev).Time(), 0D));
                        }
                    }
                    ps = ps.slice(0, gomaxprocs);
                }
                while (len(ps) < gomaxprocs) {
                    // Start new P's series.
                    nint series = 0;
                    if ((UtilFlags)(flags & UtilPerProc) != 0 || len(@out) == 0) {
                        series = len(@out);
                        @out = append(@out, new MutatorUtil[]{new((int64)(~ev).Time(), 1D)}.slice());
                    }
                    ps = append(ps, new MutatorUtilizationV2_perP(series: series));
                }
                if (len(procs) == 0 || gomaxprocs != procs[len(procs) - 1].n) {
                    procs = append(procs, new MutatorUtilizationV2_procsCount(time: (int64)(~ev).Time(), n: gomaxprocs));
                }
            } while (false);
        }

        if (len(ps) == 0) {
            // We can't start doing any analysis until we see what GOMAXPROCS is.
            // It will show up very early in the trace, but we need to be robust to
            // something else being emitted beforehand.
            continue;
        }
        var exprᴛ2 = (~ev).Kind();
        var matchᴛ1 = false;
        if (exprᴛ2 == EventRangeActive) { matchᴛ1 = true;
            do {
                if (seenSync) {
                    // If we've seen a sync, then we can be sure we're not finding out about
                    // something late; we have complete information after that point, and these
                    // active events will just be redundant.
                    break;
                }
                var r = (~ev).Range();
                var matchᴛ2 = false;
                if (handleMarkAssist(r)) {
                    do {
                        if (!states[(~ev).Goroutine()].Executing()) {
                            // This range is active back to the start of the trace. We're failing to account
                            // for this since we just found out about it now. Fix up the mutator utilization.
                            //
                            // N.B. A trace can't start during a STW, so we don't handle it here.
                            // If the goroutine isn't executing, then the fact that it was in mark
                            // assist doesn't actually count.
                            break;
                        }
                        fallthrough = true;
                    } while (false);
                }
                if (fallthrough || !matchᴛ2 && (handleSweep(r))) { matchᴛ2 = true;
                    do {
                        if ((UtilFlags)(flags & UtilPerProc) != 0) {
                            // This G has been in a mark assist *and running on its P* since the start
                            // of the trace.
                            // This P has been in sweep (or mark assist, from above) in the start of the trace.
                            //
                            // We don't need to do anything if UtilPerProc is set. If we get an event like
                            // this for a running P, it must show up the first time a P is mentioned. Therefore,
                            // this P won't actually have any MutatorUtils on its list yet.
                            //
                            // However, if UtilPerProc isn't set, then we probably have data from other procs
                            // and from previous events. We need to fix that up.
                            break;
                        }
                        nint mi = 0;
                        nint pi = 0;
                        while (mi < len(@out[0])) {
                            // Subtract out 1/gomaxprocs mutator utilization for all time periods
                            // from the beginning of the trace until now.
                            if (pi < len(procs) - 1 && procs[pi + 1].time < @out[0][mi].Time) {
                                pi++;
                                continue;
                            }
                            @out[0][mi].Util -= (float64)1D / (float64)procs[pi].n;
                            if (@out[0][mi].Util < 0D) {
                                @out[0][mi].Util = 0D;
                            }
                            mi++;
                        }
                    } while (false);
                }

                fallthrough = true;
            } while (false);
        }
        if (fallthrough || !matchᴛ1 && exprᴛ2 == EventRangeBegin) { matchᴛ1 = true;
            var r = (~ev).Range();
            if (handleSTW(r)){
                // After accounting for the portion we missed, this just acts like the
                // beginning of a new range.
                stw++;
            } else 
            if (handleSweep(r)){
                ps[(nint)((~ev).Proc())].gc++;
            } else 
            if (handleMarkAssist(r)) {
                ps[(nint)((~ev).Proc())].gc++;
                {
                    var g = r.Scope.Goroutine(); if (g != NoGoroutine) {
                        inGC[g] = true;
                    }
                }
            }
        }
        else if (exprᴛ2 == EventRangeEnd) { matchᴛ1 = true;
            var r = (~ev).Range();
            if (handleSTW(r)){
                stw--;
            } else 
            if (handleSweep(r)){
                ps[(nint)((~ev).Proc())].gc--;
            } else 
            if (handleMarkAssist(r)) {
                ps[(nint)((~ev).Proc())].gc--;
                {
                    var g = r.Scope.Goroutine(); if (g != NoGoroutine) {
                        delete(inGC, g);
                    }
                }
            }
        }
        else if (exprᴛ2 == EventStateTransition) {
            do {
                var st = (~ev).StateTransition();
                if (st.Resource.Kind != ResourceGoroutine) {
                    break;
                }
                var (old, @new) = st.Goroutine();
                var g = st.Resource.Goroutine();
                if (inGC[g] || bgMark[g]) {
                    if (!old.Executing() && @new.Executing()){
                        // Started running while doing GC things.
                        ps[(nint)((~ev).Proc())].gc++;
                    } else 
                    if (old.Executing() && !@new.Executing()) {
                        // Stopped running while doing GC things.
                        ps[(nint)((~ev).Proc())].gc--;
                    }
                }
                states[g] = @new;
            } while (false);
        }
        else if (exprᴛ2 == EventLabel) { matchᴛ1 = true;
            var l = (~ev).Label();
            if ((UtilFlags)(flags & UtilBackground) != 0 && strings.HasPrefix(l.ΔΔLabel, "GC "u8) && l.ΔΔLabel != "GC (idle)"u8) {
                // Background mark worker.
                //
                // If we're in per-proc mode, we don't
                // count dedicated workers because
                // they kick all of the goroutines off
                // that P, so don't directly
                // contribute to goroutine latency.
                if (!((UtilFlags)(flags & UtilPerProc) != 0 && l.ΔΔLabel == "GC (dedicated)"u8)) {
                    bgMark[(~ev).Goroutine()] = true;
                    ps[(nint)((~ev).Proc())].gc++;
                }
            }
        }

        if ((UtilFlags)(flags & UtilPerProc) == 0){
            // Compute the current average utilization.
            if (len(ps) == 0) {
                continue;
            }
            nint gcPs = 0;
            if (stw > 0){
                gcPs = len(ps);
            } else {
                foreach (var (iΔ1, _) in ps) {
                    if (ps[iΔ1].gc > 0) {
                        gcPs++;
                    }
                }
            }
            var muΔ1 = new MutatorUtil((int64)(~ev).Time(), 1D - (float64)gcPs / (float64)len(ps));
            // Record the utilization change. (Since
            // len(ps) == len(out), we know len(out) > 0.)
            @out[0] = addUtil(@out[0], muΔ1);
        } else {
            // Check for per-P utilization changes.
            foreach (var (iΔ2, _) in ps) {
                var p = Ꮡ(ps, iΔ2);
                var util = 1.0D;
                if (stw > 0 || (~p).gc > 0) {
                    util = 0.0D;
                }
                @out[(~p).series] = addUtil(@out[(~p).series], new MutatorUtil((int64)(~ev).Time(), util));
            }
        }
    }
    // No events in the stream.
    if (lastEv == nil) {
        return default!;
    }
    // Add final 0 utilization event to any remaining series. This
    // is important to mark the end of the trace. The exact value
    // shouldn't matter since no window should extend beyond this,
    // but using 0 is symmetric with the start of the trace.
    var mu = new MutatorUtil((int64)(~lastEv).Time(), 0D);
    foreach (var (i, _) in ps) {
        @out[ps[i].series] = addUtil(@out[ps[i].series], mu);
    }
    return @out;
}

internal static slice<MutatorUtil> addUtil(slice<MutatorUtil> util, MutatorUtil mu) {
    if (len(util) > 0) {
        if (mu.Util == util[len(util) - 1].Util) {
            // No change.
            return util;
        }
        if (mu.Time == util[len(util) - 1].Time) {
            // Take the lowest utilization at a time stamp.
            if (mu.Util < util[len(util) - 1].Util) {
                util[len(util) - 1] = mu;
            }
            return util;
        }
    }
    return append(util, mu);
}

partial struct totalUtil /*num:float64*/;

internal static totalUtil totalUtilOf(float64 meanUtil, int64 dur) {
    return ((totalUtil)(meanUtil * (float64)dur));
}

// mean returns the mean utilization over dur.
internal static float64 mean(this totalUtil u, time.Duration dur) {
    return (float64)u / (float64)(int64)dur;
}

// An MMUCurve is the minimum mutator utilization curve across
// multiple window sizes.
partial struct MMUCurve {
    internal slice<mmuSeries> series;
}

partial struct mmuSeries {
    internal slice<MutatorUtil> util;
    // sums[j] is the cumulative sum of util[:j].
    internal slice<totalUtil> sums;
    // bands summarizes util in non-overlapping bands of duration
    // bandDur.
    internal slice<mmuBand> bands;
    // bandDur is the duration of each band.
    internal int64 bandDur;
}

partial struct mmuBand {
    // minUtil is the minimum instantaneous mutator utilization in
    // this band.
    internal float64 minUtil;
    // cumUtil is the cumulative total mutator utilization between
    // time 0 and the left edge of this band.
    internal totalUtil cumUtil;
    // integrator is the integrator for the left edge of this
    // band.
    internal integrator integrator;
}

// NewMMUCurve returns an MMU curve for the given mutator utilization
// function.
public static ж<MMUCurve> NewMMUCurve(slice<slice<MutatorUtil>> utils) {
    var series = new slice<mmuSeries>(len(utils));
    foreach (var (i, util) in utils) {
        series[i] = newMMUSeries(util);
    }
    return Ꮡ(new MMUCurve(series));
}

// bandsPerSeries is the number of bands to divide each series into.
// This is only changed by tests.
internal static nint bandsPerSeries = 1000;

internal static mmuSeries newMMUSeries(slice<MutatorUtil> util) {
    // Compute cumulative sum.
    var sums = new slice<totalUtil>(len(util));
    MutatorUtil prev = default!;
    totalUtil sum = default!;
    foreach (var (j, u) in util) {
        sum += totalUtilOf(prev.Util, u.Time - prev.Time);
        sums[j] = sum;
        prev = u;
    }
    // Divide the utilization curve up into equal size
    // non-overlapping "bands" and compute a summary for each of
    // these bands.
    //
    // Compute the duration of each band.
    nint numBands = bandsPerSeries;
    if (numBands > len(util)) {
        // There's no point in having lots of bands if there
        // aren't many events.
        numBands = len(util);
    }
    var dur = util[len(util) - 1].Time - util[0].Time;
    var bandDur = quo((dur + (int64)numBands - 1), (int64)numBands);
    if (bandDur < 1) {
        bandDur = 1;
    }
    // Compute the bands. There are numBands+1 bands in order to
    // record the final cumulative sum.
    var bands = new slice<mmuBand>(numBands + 1);
    ref var s = ref heap<mmuSeries>(out var Ꮡs);
    s = new mmuSeries(util, sums, bands, bandDur);
    var leftSum = new integrator(Ꮡs, 0);
    foreach (var (i, _) in bands) {
        var (startTime, endTime) = s.bandTime(i);
        var cumUtil = leftSum.advance(startTime);
        nint predIdx = leftSum.pos;
        var minUtil = 1.0D;
        for (nint iΔ1 = predIdx; iΔ1 < len(util) && util[iΔ1].Time < endTime; iΔ1++) {
            minUtil = math.Min(minUtil, util[iΔ1].Util);
        }
        bands[i] = new mmuBand(minUtil, cumUtil, leftSum);
    }
    return s;
}

internal static (int64 start, int64 end) bandTime(this ref mmuSeries s, nint i) {
    int64 start = default!;
    int64 end = default!;

    start = (int64)i * s.bandDur + s.util[0].Time;
    end = start + s.bandDur;
    return (start, end);
}

partial struct bandUtil {
    // Utilization series index
    internal nint series;
    // Band index
    internal nint i;
    // Lower bound of mutator utilization for all windows
    // with a left edge in this band.
    internal float64 utilBound;
}

partial struct bandUtilHeap /*[]bandUtil*/;

internal static nint Len(this bandUtilHeap h) {
    return len(h);
}

internal static bool Less(this bandUtilHeap h, nint i, nint j) {
    return h[i].utilBound < h[j].utilBound;
}

internal static void Swap(this bandUtilHeap h, nint i, nint j) {
    (h[i], h[j]) = (h[j], h[i]);
}

internal static void Push(this ref bandUtilHeap h, any x) {
    h = append(h, x._<bandUtil>());
}

internal static any Pop(this ref bandUtilHeap h) {
    var x = (h)[len(h) - 1];
    h = (h).slice(0, len(h) - 1);
    return x;
}

// UtilWindow is a specific window at Time.
partial struct UtilWindow {
    public int64 Time;
    // MutatorUtil is the mean mutator utilization in this window.
    public float64 MutatorUtil;
}

partial struct utilHeap /*[]UtilWindow*/;

internal static nint Len(this utilHeap h) {
    return len(h);
}

internal static bool Less(this utilHeap h, nint i, nint j) {
    if (h[i].MutatorUtil != h[j].MutatorUtil) {
        return h[i].MutatorUtil > h[j].MutatorUtil;
    }
    return h[i].Time > h[j].Time;
}

internal static void Swap(this utilHeap h, nint i, nint j) {
    (h[i], h[j]) = (h[j], h[i]);
}

internal static void Push(this ref utilHeap h, any x) {
    h = append(h, x._<UtilWindow>());
}

internal static any Pop(this ref utilHeap h) {
    var x = (h)[len(h) - 1];
    h = (h).slice(0, len(h) - 1);
    return x;
}

// An accumulator takes a windowed mutator utilization function and
// tracks various statistics for that function.
partial struct accumulator {
    internal float64 mmu;
    // bound is the mutator utilization bound where adding any
    // mutator utilization above this bound cannot affect the
    // accumulated statistics.
    internal float64 bound;
    // Worst N window tracking
    internal nint nWorst;
    internal utilHeap wHeap;
    // Mutator utilization distribution tracking
    internal ж<mud> mud;
    // preciseMass is the distribution mass that must be precise
    // before accumulation is stopped.
    internal float64 preciseMass;
    // lastTime and lastMU are the previous point added to the
    // windowed mutator utilization function.
    internal int64 lastTime;
    internal float64 lastMU;
}

// resetTime declares a discontinuity in the windowed mutator
// utilization function by resetting the current time.
internal static void resetTime(this ref accumulator acc) {
    // This only matters for distribution collection, since that's
    // the only thing that depends on the progression of the
    // windowed mutator utilization function.
    acc.lastTime = math.MaxInt64;
}

// addMU adds a point to the windowed mutator utilization function at
// (time, mu). This must be called for monotonically increasing values
// of time.
//
// It returns true if further calls to addMU would be pointless.
internal static bool addMU(this ж<accumulator> Ꮡacc, int64 timeΔ1, float64 mu, time.Duration window) {
    ref var acc = ref Ꮡacc.DerefOrNull();

    if (mu < acc.mmu) {
        acc.mmu = mu;
    }
    acc.bound = acc.mmu;
    if (acc.nWorst == 0) {
        // If the minimum has reached zero, it can't go any
        // lower, so we can stop early.
        return mu == 0D;
    }
    // Consider adding this window to the n worst.
    if (len(acc.wHeap) < acc.nWorst || mu < acc.wHeap[0].MutatorUtil) {
        // This window is lower than the K'th worst window.
        //
        // Check if there's any overlapping window
        // already in the heap and keep whichever is
        // worse.
        foreach (var (i, ui) in acc.wHeap) {
            if (timeΔ1 + (int64)window > ui.Time && ui.Time + (int64)window > timeΔ1) {
                if (ui.MutatorUtil <= mu){
                    // Keep the first window.
                    goto keep;
                } else {
                    // Replace it with this window.
                    heap.Remove(new utilHeapжInterface(Ꮡacc.of(accumulator.ᏑwHeap)), i);
                    break;
                }
            }
        }
        heap.Push(new utilHeapжInterface(Ꮡacc.of(accumulator.ᏑwHeap)), new UtilWindow(timeΔ1, mu));
        if (len(acc.wHeap) > acc.nWorst) {
            heap.Pop(new utilHeapжInterface(Ꮡacc.of(accumulator.ᏑwHeap)));
        }
keep:;
    }
    if (len(acc.wHeap) < acc.nWorst){
        // We don't have N windows yet, so keep accumulating.
        acc.bound = 1.0D;
    } else {
        // Anything above the least worst window has no effect.
        acc.bound = math.Max(acc.bound, acc.wHeap[0].MutatorUtil);
    }
    if (acc.mud != nil) {
        if (acc.lastTime != math.MaxInt64) {
            // Update distribution.
            acc.mud.add(acc.lastMU, mu, (float64)(timeΔ1 - acc.lastTime));
        }
        (acc.lastTime, acc.lastMU) = (timeΔ1, mu);
        {
            var (_, mudBound, ok) = acc.mud.approxInvCumulativeSum(); if (ok){
                acc.bound = math.Max(acc.bound, mudBound);
            } else {
                // We haven't accumulated enough total precise
                // mass yet to even reach our goal, so keep
                // accumulating.
                acc.bound = 1D;
            }
        }
        // It's not worth checking percentiles every time, so
        // just keep accumulating this band.
        return false;
    }
    // If we've found enough 0 utilizations, we can stop immediately.
    return len(acc.wHeap) == acc.nWorst && acc.wHeap[0].MutatorUtil == 0D;
}

// MMU returns the minimum mutator utilization for the given time
// window. This is the minimum utilization for all windows of this
// duration across the execution. The returned value is in the range
// [0, 1].
public static float64 /*mmu*/ MMU(this ref MMUCurve c, time.Duration window) {
    ref var acc = ref heap<accumulator>(out var Ꮡacc);
    acc = new accumulator(mmu: 1.0D, bound: 1.0D);
    c.mmu(window, Ꮡacc);
    return acc.mmu;
}

// Examples returns n specific examples of the lowest mutator
// utilization for the given window size. The returned windows will be
// disjoint (otherwise there would be a huge number of
// mostly-overlapping windows at the single lowest point). There are
// no guarantees on which set of disjoint windows this returns.
public static slice<UtilWindow> /*worst*/ Examples(this ref MMUCurve c, time.Duration window, nint n) {
    ref var acc = ref heap<accumulator>(out var Ꮡacc);
    acc = new accumulator(mmu: 1.0D, bound: 1.0D, nWorst: n);
    c.mmu(window, Ꮡacc);
    sort.Sort(sort.Reverse(acc.wHeap));
    return ((slice<UtilWindow>)acc.wHeap);
}

// MUD returns mutator utilization distribution quantiles for the
// given window size.
//
// The mutator utilization distribution is the distribution of mean
// mutator utilization across all windows of the given window size in
// the trace.
//
// The minimum mutator utilization is the minimum (0th percentile) of
// this distribution. (However, if only the minimum is desired, it's
// more efficient to use the MMU method.)
public static slice<float64> MUD(this ref MMUCurve c, time.Duration window, slice<float64> quantiles) {
    if (len(quantiles) == 0) {
        return new float64[]{}.slice();
    }
    // Each unrefined band contributes a known total mass to the
    // distribution (bandDur except at the end), but in an unknown
    // way. However, we know that all the mass it contributes must
    // be at or above its worst-case mean mutator utilization.
    //
    // Hence, we refine bands until the highest desired
    // distribution quantile is less than the next worst-case mean
    // mutator utilization. At this point, all further
    // contributions to the distribution must be beyond the
    // desired quantile and hence cannot affect it.
    //
    // First, find the highest desired distribution quantile.
    var maxQ = quantiles[0];
    foreach (var (_, q) in quantiles) {
        if (q > maxQ) {
            maxQ = q;
        }
    }
    // The distribution's mass is in units of time (it's not
    // normalized because this would make it more annoying to
    // account for future contributions of unrefined bands). The
    // total final mass will be the duration of the trace itself
    // minus the window size. Using this, we can compute the mass
    // corresponding to quantile maxQ.
    int64 duration = default!;
    foreach (var (_, s) in c.series) {
        var duration1 = s.util[len(s.util) - 1].Time - s.util[0].Time;
        if (duration1 >= (int64)window) {
            duration += duration1 - (int64)window;
        }
    }
    var qMass = (float64)duration * maxQ;
    // Accumulate the MUD until we have precise information for
    // everything to the left of qMass.
    ref var acc = ref heap<accumulator>(out var Ꮡacc);
    acc = new accumulator(mmu: 1.0D, bound: 1.0D, preciseMass: qMass, mud: @new<mud>());
    acc.mud.setTrackMass(qMass);
    c.mmu(window, Ꮡacc);
    // Evaluate the quantiles on the accumulated MUD.
    var @out = new slice<float64>(len(quantiles));
    foreach (var (i, _) in @out) {
        var (mu, _) = acc.mud.invCumulativeSum((float64)duration * quantiles[i]);
        if (math.IsNaN(mu)) {
            // There are a few legitimate ways this can
            // happen:
            //
            // 1. If the window is the full trace
            // duration, then the windowed MU function is
            // only defined at a single point, so the MU
            // distribution is not well-defined.
            //
            // 2. If there are no events, then the MU
            // distribution has no mass.
            //
            // Either way, all of the quantiles will have
            // converged toward the MMU at this point.
            mu = acc.mmu;
        }
        @out[i] = mu;
    }
    return @out;
}

internal static void mmu(this ref MMUCurve c, time.Duration window, ж<accumulator> Ꮡacc) {
    ref var acc = ref Ꮡacc.DerefOrNull();

    if (window <= 0) {
        acc.mmu = 0D;
        return;
    }
    ref var bandU = ref heap<bandUtilHeap>(out var ᏑbandU);
    var windows = new slice<time.Duration>(len(c.series));
    foreach (var (i, vᴛ1) in c.series) {
        var s = vᴛ1;

        windows[i] = window;
        {
            var max = ((time.Duration)(s.util[len(s.util) - 1].Time - s.util[0].Time)); if (window > max) {
                windows[i] = max;
            }
        }
        var bandU1 = ((bandUtilHeap)s.mkBandUtil(i, windows[i]));
        if (bandU == default!){
            bandU = bandU1;
        } else {
            bandU = appendꓸꓸꓸ(bandU, bandU1);
        }
    }
    // Process bands from lowest utilization bound to highest.
    heap.Init(new bandUtilHeapжInterface(ᏑbandU));
    // Refine each band into a precise window and MMU until
    // refining the next lowest band can no longer affect the MMU
    // or windows.
    while (len(bandU) > 0 && bandU[0].utilBound < acc.bound) {
        nint i = bandU[0].series;
        c.series[i].bandMMU(bandU[0].i, windows[i], Ꮡacc);
        heap.Pop(new bandUtilHeapжInterface(ᏑbandU));
    }
}

internal static slice<bandUtil> mkBandUtil(this ref mmuSeries c, nint series, time.Duration window) {
    // For each band, compute the worst-possible total mutator
    // utilization for all windows that start in that band.
    // minBands is the minimum number of bands a window can span
    // and maxBands is the maximum number of bands a window can
    // span in any alignment.
    nint minBands = (nint)(quo(((int64)window + c.bandDur - 1), c.bandDur));
    nint maxBands = (nint)(quo(((int64)window + 2 * (c.bandDur - 1)), c.bandDur));
    if (window > 1 && maxBands < 2) {
        throw panic("maxBands < 2");
    }
    var tailDur = rem((int64)window, c.bandDur);
    nint nUtil = len(c.bands) - maxBands + 1;
    if (nUtil < 0) {
        nUtil = 0;
    }
    var bandU = new slice<bandUtil>(nUtil);
    foreach (var (i, _) in bandU) {
        // To compute the worst-case MU, we assume the minimum
        // for any bands that are only partially overlapped by
        // some window and the mean for any bands that are
        // completely covered by all windows.
        totalUtil util = default!;
        // Find the lowest and second lowest of the partial
        // bands.
        var l = c.bands[i].minUtil;
        var r1 = c.bands[i + minBands - 1].minUtil;
        var r2 = c.bands[i + maxBands - 1].minUtil;
        var minBand = math.Min(l, math.Min(r1, r2));
        // Assume the worst window maximally overlaps the
        // worst minimum and then the rest overlaps the second
        // worst minimum.
        if (minBands == 1){
            util += totalUtilOf(minBand, (int64)window);
        } else {
            util += totalUtilOf(minBand, c.bandDur);
            var midBand = 0.0D;
            switch (ᐧ) {
            case {} when minBand == l: {
                midBand = math.Min(r1, r2);
                break;
            }
            case {} when minBand == r1: {
                midBand = math.Min(l, r2);
                break;
            }
            case {} when minBand == r2: {
                midBand = math.Min(l, r1);
                break;
            }}

            util += totalUtilOf(midBand, tailDur);
        }
        // Add the total mean MU of bands that are completely
        // overlapped by all windows.
        if (minBands > 2) {
            util += c.bands[i + minBands - 1].cumUtil - c.bands[i + 1].cumUtil;
        }
        bandU[i] = new bandUtil(series, i, util.mean(window));
    }
    return bandU;
}

// bandMMU computes the precise minimum mutator utilization for
// windows with a left edge in band bandIdx.
internal static void bandMMU(this ref mmuSeries c, nint bandIdx, time.Duration window, ж<accumulator> Ꮡacc) {
    ref var acc = ref Ꮡacc.DerefOrNull();

    var util = c.util;
    // We think of the mutator utilization over time as the
    // box-filtered utilization function, which we call the
    // "windowed mutator utilization function". The resulting
    // function is continuous and piecewise linear (unless
    // window==0, which we handle elsewhere), where the boundaries
    // between segments occur when either edge of the window
    // encounters a change in the instantaneous mutator
    // utilization function. Hence, the minimum of this function
    // will always occur when one of the edges of the window
    // aligns with a utilization change, so these are the only
    // points we need to consider.
    //
    // We compute the mutator utilization function incrementally
    // by tracking the integral from t=0 to the left edge of the
    // window and to the right edge of the window.
    var left = c.bands[bandIdx].integrator;
    var right = left;
    var (timeΔ1, endTime) = c.bandTime(bandIdx);
    {
        var utilEnd = util[len(util) - 1].Time - (int64)window; if (utilEnd < endTime) {
            endTime = utilEnd;
        }
    }
    acc.resetTime();
    while (ᐧ) {
        // Advance edges to time and time+window.
        var mu = (right.advance(timeΔ1 + (int64)window) - left.advance(timeΔ1)).mean(window);
        if (Ꮡacc.addMU(timeΔ1, mu, window)) {
            break;
        }
        if (timeΔ1 == endTime) {
            break;
        }
        // The maximum slope of the windowed mutator
        // utilization function is 1/window, so we can always
        // advance the time by at least (mu - mmu) * window
        // without dropping below mmu.
        var minTime = timeΔ1 + (int64)((mu - acc.bound) * (float64)(int64)window);
        // Advance the window to the next time where either
        // the left or right edge of the window encounters a
        // change in the utilization curve.
        {
            var (t1, t2) = (left.next(timeΔ1), right.next(timeΔ1 + (int64)window) - (int64)window); if (t1 < t2){
                timeΔ1 = t1;
            } else {
                timeΔ1 = t2;
            }
        }
        if (timeΔ1 < minTime) {
            timeΔ1 = minTime;
        }
        if (timeΔ1 >= endTime) {
            // For MMUs we could stop here, but for MUDs
            // it's important that we span the entire
            // band.
            timeΔ1 = endTime;
        }
    }
}

// An integrator tracks a position in a utilization function and
// integrates it.
partial struct integrator {
    internal ж<mmuSeries> u;
    // pos is the index in u.util of the current time's non-strict
    // predecessor.
    internal nint pos;
}

// advance returns the integral of the utilization function from 0 to
// time. advance must be called on monotonically increasing values of
// times.
internal static totalUtil advance(this ref integrator @in, int64 time) {
    var util = @in.u.Value.util;
    nint pos = @in.pos;
    // Advance pos until pos+1 is time's strict successor (making
    // pos time's non-strict predecessor).
    //
    // Very often, this will be nearby, so we optimize that case,
    // but it may be arbitrarily far away, so we handled that
    // efficiently, too.
    const nint maxSeq = 8;
    if (pos + maxSeq < len(util) && util[pos + maxSeq].Time > time){
        // Nearby. Use a linear scan.
        while (pos + 1 < len(util) && util[pos + 1].Time <= time) {
            pos++;
        }
    } else {
        // Far. Binary search for time's strict successor.
        nint l = pos;
        nint r = len(util);
        while (l < r) {
            nint h = (nint)(((nuint)(l + r) >> (int)(1)));
            if (util[h].Time <= time){
                l = h + 1;
            } else {
                r = h;
            }
        }
        pos = l - 1; // Non-strict predecessor.
    }
    @in.pos = pos;
    totalUtil partial = default!;
    if (time != util[pos].Time) {
        partial = totalUtilOf(util[pos].Util, time - util[pos].Time);
    }
    return (~@in.u).sums[pos] + partial;
}

// next returns the smallest time t' > time of a change in the
// utilization function.
internal static int64 next(this ref integrator @in, int64 time) {
    foreach (var (_, u) in (~@in.u).util.slice(@in.pos)) {
        if (u.Time > time) {
            return u.Time;
        }
    }
    return 9223372036854775807L;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stopTheWorldˢ = "stop-the-world"u8;

internal static bool isGCSTW(ΔRange r) {
    return strings.HasPrefix(r.Name, stopTheWorldˢ) && strings.Contains(r.Name, "GC"u8);
}

internal static bool isGCMarkAssist(ΔRange r) {
    return r.Name == "GC mark assist"u8;
}

internal static bool isGCSweep(ΔRange r) {
    return r.Name == "GC incremental sweep"u8;
}

} // end trace_package
