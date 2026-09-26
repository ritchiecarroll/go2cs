using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards the CPU SAMPLER SEAM (runtime/cpusampler_impl.cs, section 11 of DESIGN-managed-profiling.md):
/// a registered sampler is started by SetCPUProfileRate(hz), stopped by SetCPUProfileRate(0), and the
/// samples it writes on stop reach the profile log that runtime/pprof reads. A sampler that throws leaves
/// a valid profile with no samples and never throws out of SetCPUProfileRate. Red while the per-target
/// setProcessCPUProfiler bodies do not call the seam. The sampler is registered once per process, so
/// one proxy is registered and each arm sets its behavior; with no behavior set it does nothing, which
/// keeps RuntimeCPUProfilerTests' zero-sample profiles unchanged.
/// </summary>
[TestClass]
public class RuntimeCpuSamplerSeamTests
{
    private sealed class ProxySampler : IGoCpuSampler
    {
        public Action<int>? OnStart;
        public Action<GoCpuSampleWriter>? OnStop;

        public void Start(int hz) => OnStart?.Invoke(hz);

        public void Stop(GoCpuSampleWriter write) => OnStop?.Invoke(write);
    }

    private static readonly ProxySampler s_proxy = new();

    [ClassInitialize]
    public static void RegisterProxy(TestContext _)
    {
        // Another class may not register first: the seam refuses a second sampler.
        Assert.IsTrue(GoRegisterCpuSampler(s_proxy), "the proxy must be the process's sampler");
    }

    [TestCleanup]
    public void ClearProxy()
    {
        s_proxy.OnStart = null;
        s_proxy.OnStop = null;
    }

    private static readonly TimeSpan Bound = TimeSpan.FromSeconds(30);

    // Drains the profile log to end-of-data and returns its records as (hdr, stack), skipping the
    // leading header record (hdr = hz, no stack). Bounded: a read that never reaches EOF is a failure.
    private static List<(ulong hdr, ulong[] stack)> DrainRecords()
    {
        Task<List<(ulong, ulong[])>> task = Task.Run(() =>
        {
            var records = new List<(ulong, ulong[])>();

            while (true)
            {
                var (data, _, eof) = runtime_pprof_readProfile();

                for (int i = 0; i < (int)len(data);)
                {
                    int n = (int)data[i];
                    var stack = new ulong[n - 3];

                    for (int j = 0; j < stack.Length; j++)
                        stack[j] = data[i + 3 + j];

                    records.Add((data[i + 2], stack));
                    i += n;
                }

                if (eof)
                    return records;
            }
        });

        if (!task.Wait(Bound))
            Assert.Fail($"the profile log did not reach end-of-data within {Bound.TotalSeconds} s");

        List<(ulong, ulong[])> all = task.Result;
        Assert.IsTrue(all.Count >= 1 && all[0].Item2.Length == 0, "the first record is the header");
        all.RemoveAt(0);
        return all;
    }

    [TestMethod]
    public void ARegisteredSamplerIsStartedAndStopped()
    {
        int started = 0;
        bool stopped = false;
        s_proxy.OnStart = hz => started = hz;
        s_proxy.OnStop = _ => stopped = true;

        SetCPUProfileRate(100);
        SetCPUProfileRate(0);
        DrainRecords();

        Assert.AreEqual(100, started, "SetCPUProfileRate(100) must start the sampler at 100 Hz");
        Assert.IsTrue(stopped, "SetCPUProfileRate(0) must stop the sampler");
    }

    [TestMethod]
    public void SamplesWrittenOnStopReachTheProfileLog()
    {
        s_proxy.OnStop = write =>
        {
            for (int i = 0; i < 3; i++)
                write(1000 + i, new uintptr[] { 0x1234, 0x5678 }.slice(), default);
        };

        SetCPUProfileRate(100);
        SetCPUProfileRate(0);
        List<(ulong hdr, ulong[] stack)> records = DrainRecords();

        int matching = records.FindAll(r => r.hdr == 1 && r.stack.Length == 2 && r.stack[0] == 0x1234 && r.stack[1] == 0x5678).Count;
        Assert.AreEqual(3, matching, "the three samples written on stop must be in the profile, one count each");
    }

    [TestMethod]
    public void AThrowingSamplerLeavesAnEmptyProfileAndNoThrow()
    {
        s_proxy.OnStart = _ => throw new InvalidOperationException("start");
        s_proxy.OnStop = _ => throw new InvalidOperationException("stop");

        for (int round = 1; round <= 2; round++)
        {
            SetCPUProfileRate(100);
            SetCPUProfileRate(0);
            Assert.AreEqual(0, DrainRecords().Count, $"round {round}: a throwing sampler leaves no samples");
        }
    }
}
