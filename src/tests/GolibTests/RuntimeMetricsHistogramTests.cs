using System;
using go;
using go.runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using metrics = go.runtime.metrics_package;

namespace GolibTests;

/// <summary>
/// runtime/metrics.Read's crossing into the runtime (runtime/metrics/sample.cs and runtime's
/// readMetricsManaged, managed_impl.cs). A histogram metric's Value carries a pointer, and
/// Value.Float64Histogram() casts it to *Float64Histogram. The runtime fills it with its own
/// metricFloat64Histogram, a different type with the same layout, so the cast was a reinterpret the
/// managed pointer model refuses (arm 2a). The runtime row's TestReadMetricsCumulative died there
/// before stopping its background goroutine, which then forced GCs for the rest of the run.
/// </summary>
[TestClass]
public class RuntimeMetricsHistogramTests
{
    [TestMethod]
    public void AHistogramMetricReadsBackThroughFloat64Histogram()
    {
        slice<metrics.Sample> samples = new(1);
        samples[0].Name = "/gc/heap/allocs-by-size:bytes";

        string reading;
        int counts = -1, buckets = -1;
        ulong total = 0;

        try
        {
            metrics.Read(samples);
            ж<metrics.Float64Histogram> hist = samples[0].Value.Float64Histogram();
            counts = (int)len(hist.Value.Counts);
            buckets = (int)len(hist.Value.Buckets);
            for (int i = 0; i < counts; i++)
                total += hist.Value.Counts[i];
            reading = $"counts {counts}, buckets {buckets}, total {total}";
        }
        catch (Exception ex)
        {
            reading = $"{ex.GetType().Name}: {ex.Message}";
        }

        Assert.IsTrue(counts > 0, reading);
        Assert.AreEqual(counts + 1, buckets, $"Go's shape: len(Buckets) == len(Counts)+1 -- {reading}");
    }
}
