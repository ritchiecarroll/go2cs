// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using static runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

internal static ж<global::go.runtime_internal_test_package.TimeHistogram> ᏑdummyTimeHistogram = new StandardBox<global::go.runtime_internal_test_package.TimeHistogram>(new global::go.runtime_internal_test_package.TimeHistogram());
internal static ref global::go.runtime_internal_test_package.TimeHistogram dummyTimeHistogram => ref ᏑdummyTimeHistogram.Value;

public static void TestTimeHistogram(ж<testing.T> Ꮡt) {
    // We need to use a global dummy because this
    // could get stack-allocated with a non-8-byte alignment.
    // The result of this bad alignment is a segfault on
    // 32-bit platforms when calling Record.
    var h = ᏑdummyTimeHistogram;
    // Record exactly one sample in each bucket.
    for (nint j = 0; j < runtime_internal_test_package.TimeHistNumSubBuckets; j++) {
        var v = ((int64)j << (int)((runtime_internal_test_package.TimeHistMinBucketBits - 1 - runtime_internal_test_package.TimeHistSubBucketBits)));
        for (nint k = 0; k < j; k++) {
            // Record a number of times equal to the bucket index.
            h.Record(v);
        }
    }
    for (nint i = runtime_internal_test_package.TimeHistMinBucketBits; i < runtime_internal_test_package.TimeHistMaxBucketBits; i++) {
        var @base = ((int64)1).Lsh((int64)((i - 1)));
        for (nint j = 0; j < runtime_internal_test_package.TimeHistNumSubBuckets; j++) {
            var v = ((int64)j).Lsh((int64)((i - 1 - (nint)runtime_internal_test_package.TimeHistSubBucketBits)));
            for (nint k = 0; k < (i + 1 - (nint)runtime_internal_test_package.TimeHistMinBucketBits) * (nint)runtime_internal_test_package.TimeHistNumSubBuckets + j; k++) {
                // Record a number of times equal to the bucket index.
                h.Record(@base + v);
            }
        }
    }
    // Hit the underflow and overflow buckets.
    h.Record((int64)(-1));
    h.Record(Δmath.MaxInt64);
    h.Record(Δmath.MaxInt64);
    // Check to make sure there's exactly one count in each
    // bucket.
    for (nint i = 0; i < runtime_internal_test_package.TimeHistNumBuckets; i++) {
        for (nint j = 0; j < runtime_internal_test_package.TimeHistNumSubBuckets; j++) {
            var (cΔ1, okΔ1) = h.Count(i, j);
            if (!okΔ1){
                Ꮡt.Errorf("unexpected invalid bucket: (%d, %d)"u8, i, j);
            } else 
            {
                var idx = (uint64)(i * (nint)runtime_internal_test_package.TimeHistNumSubBuckets + j); if (cΔ1 != idx) {
                    Ꮡt.Errorf("bucket (%d, %d) has count that is not %d: %d"u8, i, j, idx, cΔ1);
                }
            }
        }
    }
    var (c, ok) = h.Count(-1, 0);
    if (ok) {
        Ꮡt.Errorf("expected to hit underflow bucket: (%d, %d)"u8, (nint)(-1), (nint)(0));
    }
    if (c != 1) {
        Ꮡt.Errorf("overflow bucket has count that is not 1: %d"u8, c);
    }
    (c, ok) = h.Count(runtime_internal_test_package.TimeHistNumBuckets + 1, 0);
    if (ok) {
        Ꮡt.Errorf("expected to hit overflow bucket: (%d, %d)"u8, (nint)(runtime_internal_test_package.TimeHistNumBuckets + 1), (nint)(0));
    }
    if (c != 2) {
        Ꮡt.Errorf("overflow bucket has count that is not 2: %d"u8, c);
    }
    dummyTimeHistogram = new runtime_internal_test_package.TimeHistogram(nil);
}

public static void TestTimeHistogramMetricsBuckets(ж<testing.T> Ꮡt) {
    var buckets = runtime_internal_test_package.TimeHistogramMetricsBuckets();
    nint nonInfBucketsLen = runtime_internal_test_package.TimeHistNumSubBuckets * runtime_internal_test_package.TimeHistNumBuckets;
    nint expBucketsLen = nonInfBucketsLen + 3; // Count -Inf, the edge for the overflow bucket, and +Inf.
    if (len(buckets) != expBucketsLen) {
        Ꮡt.Fatalf("unexpected length of buckets: got %d, want %d"u8, len(buckets), expBucketsLen);
    }
    // Check some values.
    var idxToBucket = new map<nint, float64>{
        [0] = Δmath.Inf(-1),
        [1] = 0.0D,
        [2] = (float64)64D / 1e9D,
        [3] = (float64)128D / 1e9D,
        [4] = (float64)192D / 1e9D,
        [5] = (float64)256D / 1e9D,
        [6] = (float64)320D / 1e9D,
        [7] = (float64)384D / 1e9D,
        [8] = (float64)448D / 1e9D,
        [9] = (float64)512D / 1e9D,
        [10] = (float64)640D / 1e9D,
        [11] = (float64)768D / 1e9D,
        [12] = (float64)896D / 1e9D,
        [13] = (float64)1024D / 1e9D,
        [15] = (float64)1536D / 1e9D,
        [81] = (float64)134217728D / 1e9D,
        [82] = (float64)167772160D / 1e9D,
        [108] = (float64)15032385536D / 1e9D,
        [expBucketsLen - 2] = (float64)(140737488355328D) / 1e9D,
        [expBucketsLen - 1] = Δmath.Inf(1)
    };
    foreach (var (idx, bucket) in idxToBucket) {
        {
            var (got, want) = (buckets[idx], bucket); if (got != want) {
                Ꮡt.Errorf("expected bucket %d to have value %e, got %e"u8, idx, want, got);
            }
        }
    }
}

} // end runtime_test_package
