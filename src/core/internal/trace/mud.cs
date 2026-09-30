// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using cmp = cmp_package;
using math = math_package;
using slices = slices_package;

partial class trace_package {

// mud is an updatable mutator utilization distribution.
//
// This is a continuous distribution of duration over mutator
// utilization. For example, the integral from mutator utilization a
// to b is the total duration during which the mutator utilization was
// in the range [a, b].
//
// This distribution is *not* normalized (it is not a probability
// distribution). This makes it easier to work with as it's being
// updated.
//
// It is represented as the sum of scaled uniform distribution
// functions and Dirac delta functions (which are treated as
// degenerate uniform distributions).
[GoType] partial struct mud {
    internal slice<edge> sorted, unsorted;
    // trackMass is the inverse cumulative sum to track as the
    // distribution is updated.
    internal float64 trackMass;
    // trackBucket is the bucket in which trackMass falls. If the
    // total mass of the distribution is < trackMass, this is
    // len(hist).
    internal nint trackBucket;
    // trackSum is the cumulative sum of hist[:trackBucket]. Once
    // trackSum >= trackMass, trackBucket must be recomputed.
    internal float64 trackSum;
    // hist is a hierarchical histogram of distribution mass.
    internal array<float64> hist = new(mudDegree);
}

internal static UntypedInt mudDegree => 1024;

[GoType] partial struct edge {
    // At x, the function increases by y.
    internal float64 x, delta;
    // Additionally at x is a Dirac delta function with area dirac.
    internal float64 dirac;
}

// add adds a uniform function over [l, r] scaled so the total weight
// of the uniform is area. If l==r, this adds a Dirac delta function.
internal static void add(this ж<mud> Ꮡd, float64 l, float64 r, float64 area) {
    ref var d = ref Ꮡd.DerefOrNull();

    if (area == 0D) {
        return;
    }
    if (r < l) {
        (l, r) = (r, l);
    }
    // Add the edges.
    if (l == r){
        d.unsorted = append(d.unsorted, new edge(l, 0D, area));
    } else {
        var delta = area / (r - l);
        d.unsorted = append(d.unsorted, new edge(l, delta, 0D), new edge(r, -delta, 0D));
    }
    // Update the histogram.
    var h = Ꮡd.of(mud.Ꮡhist);
    var (lbFloat, lf) = math.Modf(l * (float64)mudDegree);
    nint lb = (nint)lbFloat;
    if (lb >= mudDegree) {
        (lb, lf) = (mudDegree - 1, 1D);
    }
    if (l == r){
        h.Value[lb] += area;
    } else {
        var (rbFloat, rf) = math.Modf(r * (float64)mudDegree);
        nint rb = (nint)rbFloat;
        if (rb >= mudDegree) {
            (rb, rf) = (mudDegree - 1, 1D);
        }
        if (lb == rb){
            h.Value[lb] += area;
        } else {
            var perBucket = area / (r - l) / (float64)mudDegree;
            h.Value[lb] += perBucket * (1D - lf);
            h.Value[rb] += perBucket * rf;
            for (nint i = lb + 1; i < rb; i++) {
                h.Value[i] += perBucket;
            }
        }
    }
    // Update mass tracking.
    {
        var thresh = (float64)d.trackBucket / (float64)mudDegree; if (l < thresh) {
            if (r < thresh){
                d.trackSum += area;
            } else {
                d.trackSum += area * (thresh - l) / (r - l);
            }
            if (d.trackSum >= d.trackMass) {
                // The tracked mass now falls in a different
                // bucket. Recompute the inverse cumulative sum.
                d.setTrackMass(d.trackMass);
            }
        }
    }
}

// setTrackMass sets the mass to track the inverse cumulative sum for.
//
// Specifically, mass is a cumulative duration, and the mutator
// utilization bounds for this duration can be queried using
// approxInvCumulativeSum.
[GoRecv] internal static void setTrackMass(this ref mud d, float64 mass) {
    d.trackMass = mass;
    // Find the bucket currently containing trackMass by computing
    // the cumulative sum.
    var sum = 0.0D;
    foreach (var (i, val) in d.hist[..]) {
        var newSum = sum + val;
        if (newSum > mass) {
            // mass falls in bucket i.
            d.trackBucket = i;
            d.trackSum = sum;
            return;
        }
        sum = newSum;
    }
    d.trackBucket = len(d.hist);
    d.trackSum = sum;
}

// approxInvCumulativeSum is like invCumulativeSum, but specifically
// operates on the tracked mass and returns an upper and lower bound
// approximation of the inverse cumulative sum.
//
// The true inverse cumulative sum will be in the range [lower, upper).
[GoRecv] internal static (float64, float64, bool) approxInvCumulativeSum(this ref mud d) {
    if (d.trackBucket == len(d.hist)) {
        return (math.NaN(), math.NaN(), false);
    }
    return ((float64)d.trackBucket / (float64)mudDegree, (float64)(d.trackBucket + 1) / (float64)mudDegree, true);
}

// invCumulativeSum returns x such that the integral of d from -∞ to x
// is y. If the total weight of d is less than y, it returns the
// maximum of the distribution and false.
//
// Specifically, y is a cumulative duration, and invCumulativeSum
// returns the mutator utilization x such that at least y time has
// been spent with mutator utilization <= x.
[GoRecv] internal static (float64, bool) invCumulativeSum(this ref mud d, float64 y) {
    if (len(d.sorted) == 0 && len(d.unsorted) == 0) {
        return (math.NaN(), false);
    }
    // Sort edges.
    var edges = d.unsorted;
    slices.SortFunc(edges, (edge a, edge b) => cmp.Compare(a.x, b.x));
    // Merge with sorted edges.
    d.unsorted = default!;
    if (d.sorted == default!){
        d.sorted = edges;
    } else {
        var oldSorted = d.sorted;
        var newSorted = new slice<edge>(len(oldSorted) + len(edges));
        nint i = 0;
        nint j = 0;
        foreach (var (o, _) in newSorted) {
            if (i >= len(oldSorted)){
                copy(newSorted.slice(o), edges.slice(j));
                break;
            } else 
            if (j >= len(edges)){
                copy(newSorted.slice(o), oldSorted.slice(i));
                break;
            } else 
            if (oldSorted[i].x < edges[j].x){
                newSorted[o] = oldSorted[i];
                i++;
            } else {
                newSorted[o] = edges[j];
                j++;
            }
        }
        d.sorted = newSorted;
    }
    // Traverse edges in order computing a cumulative sum.
    var (csum, rate, prevX) = (0.0D, 0.0D, 0.0D);
    foreach (var (_, e) in d.sorted) {
        var newCsum = csum + (e.x - prevX) * rate;
        if (newCsum >= y) {
            // y was exceeded between the previous edge
            // and this one.
            if (rate == 0D) {
                // Anywhere between prevX and
                // e.x will do. We return e.x
                // because that takes care of
                // the y==0 case naturally.
                return (e.x, true);
            }
            return ((y - csum) / rate + prevX, true);
        }
        newCsum += e.dirac;
        if (newCsum >= y) {
            // y was exceeded by the Dirac delta at e.x.
            return (e.x, true);
        }
        (csum, prevX) = (newCsum, e.x);
        rate += e.delta;
    }
    return (prevX, false);
}

} // end trace_package
