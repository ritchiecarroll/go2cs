// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using metrics = global::go.runtime.metrics_package;
using global::go.runtime;

partial class metrics_test_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string myMetricᶜ => ref myMetricᶜᵇ;
private static readonly @string myMetricᶜᵇ = "/memory/classes/heap/free:bytes"u8;

public static void ExampleRead_readingOneMetric() {
    // Name of the metric we want to read.
    @string myMetric = myMetricᶜ;

    // Create a sample for the metric.
    var sample = new slice<metrics.Sample>(1);
    sample[0].Name = myMetric;

    // Sample the metric.
    metrics.Read(sample);

    // Check if the metric is actually supported.
    // If it's not, the resulting value will always have
    // kind KindBad.
    if (sample[0].Value.Kind() == metrics.KindBad) {
        throw panic(fmt.Sprintf("metric %q no longer supported"u8, myMetric));
    }

    // Handle the result.
    //
    // It's OK to assume a particular Kind for a metric;
    // they're guaranteed not to change.
    var freeBytes = sample[0].Value.Uint64();

    fmt.Printf("free but not released memory: %d\n"u8, freeBytes);
}

public static void ExampleRead_readingAllMetrics() {
    // Get descriptions for all supported metrics.
    var descs = metrics.All();

    // Create a sample for each metric.
    var samples = new slice<metrics.Sample>(len(descs));
    foreach (var i in range(len(samples))) {
        samples[i].Name = descs[i].Name;
    }

    // Sample the metrics. Re-use the samples slice if you can!
    metrics.Read(samples);

    // Iterate over all results.
    foreach (var (_, sample) in range(samples)) {
        // Pull out the name and value.
        @string name = sample.Name;
        var value = sample.Value;

        // Handle each sample.
        switch (value.Kind()) {
        case var ᴛ when ᴛ == metrics.KindUint64: {
            fmt.Printf("%s: %d\n"u8, name, value.Uint64());
            break;
        }
        case var ᴛ when ᴛ == metrics.KindFloat64: {
            fmt.Printf("%s: %f\n"u8, name, value.Float64());
            break;
        }
        case var ᴛ when ᴛ == metrics.KindFloat64Histogram: {
            // The histogram may be quite large, so let's just pull out
            // a crude estimate for the median for the sake of this example.
            fmt.Printf("%s: %f\n"u8, name, medianBucket(value.Float64Histogram()));
            break;
        }
        case var ᴛ when ᴛ == metrics.KindBad: {
            // This should never happen because all metrics are supported
            // by construction.
            throw panic("bug in runtime/metrics package!");
            break;
        }
        default: {
            // This may happen as new metrics get added.
            //
            // The safest thing to do here is to simply log it somewhere
            // as something to look into, but ignore it for now.
            // In the worst case, you might temporarily miss out on a new metric.
            fmt.Printf("%s: unexpected metric Kind: %v\n"u8, name, value.Kind());
            break;
        }}
    }
}

internal static float64 medianBucket(ж<metricsꓸFloat64Histogram> Ꮡh) {
    ref var h = ref Ꮡh.DerefOrNull();

    var total = (uint64)0;
    foreach (var (_, count) in range(h.Counts)) {
        total += count;
    }
    var thresh = total / 2;
    total = 0;
    foreach (var (i, count) in range(h.Counts)) {
        total += count;
        if (total >= thresh) {
            return h.Buckets[i];
        }
    }
    throw panic("should not happen");
}

} // end metrics_test_package
