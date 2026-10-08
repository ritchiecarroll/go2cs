// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

partial class metrics_package {

// Float64Histogram represents a distribution of float64 values.
partial struct ΔFloat64Histogram {
    // Counts contains the weights for each histogram bucket.
    //
    // Given N buckets, Count[n] is the weight of the range
    // [bucket[n], bucket[n+1]), for 0 <= n < N.
    public slice<uint64> Counts;
    // Buckets contains the boundaries of the histogram buckets, in increasing order.
    //
    // Buckets[0] is the inclusive lower bound of the minimum bucket while
    // Buckets[len(Buckets)-1] is the exclusive upper bound of the maximum bucket.
    // Hence, there are len(Buckets)-1 counts. Furthermore, len(Buckets) != 1, always,
    // since at least two boundaries are required to describe one bucket (and 0
    // boundaries are used to describe 0 buckets).
    //
    // Buckets[0] is permitted to have value -Inf and Buckets[len(Buckets)-1] is
    // permitted to have value Inf.
    //
    // For a given metric name, the value of Buckets is guaranteed not to change
    // between calls until program exit.
    //
    // This slice value is permitted to alias with other Float64Histograms' Buckets
    // fields, so the values within should only ever be read. If they need to be
    // modified, the user must make a copy.
    public slice<float64> Buckets;
}

} // end metrics_package
