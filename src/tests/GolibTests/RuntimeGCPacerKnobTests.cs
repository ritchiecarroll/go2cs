using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using static go.runtime_package;
using debug = go.runtime.debug_package;
using metrics = go.runtime.metrics_package;

namespace GolibTests;

// The GC pacer's two knobs, GOGC and GOMEMLIMIT, have ONE source of truth: runtime's gcController.
// Go initializes it from the environment in schedinit (gcinit, after goenvs), runtime/debug's
// SetGCPercent and SetMemoryLimit are runtime's own setters (//go:linkname), and the /gc/gogc:percent
// and /gc/gomemlimit:bytes metrics read it. Until this seat runtime/debug kept private copies and
// gcController stayed 0, so runtime's TestReadMetrics read "got 0, want 99" and "got 0, want
// 536870912". Every arm restores what it changes.
[TestClass]
public class RuntimeGCPacerKnobTests
{
    private const long MiB = 1 << 20;

    [TestMethod]
    public void SetGCPercentReachesTheGogcMetric()
    {
        nint previous = debug.SetGCPercent(99);
        ulong metric;

        try
        {
            metric = ReadUint64Metric("/gc/gogc:percent");
        }
        finally
        {
            debug.SetGCPercent(previous);
        }

        Assert.AreEqual(99UL, metric, "/gc/gogc:percent after debug.SetGCPercent(99)");
    }

    [TestMethod]
    public void SetMemoryLimitReachesTheGomemlimitMetric()
    {
        long previous = debug.SetMemoryLimit(512 * MiB);
        ulong metric;

        try
        {
            metric = ReadUint64Metric("/gc/gomemlimit:bytes");
        }
        finally
        {
            debug.SetMemoryLimit(previous);
        }

        Assert.AreEqual((ulong)(512 * MiB), metric, "/gc/gomemlimit:bytes after debug.SetMemoryLimit(512 MiB)");
    }

    [TestMethod]
    public void TheControllerStartsFromTheProcessEnvironment()
    {
        // Every arm restores the knobs, so the controller still holds what the startup step read.
        (int gcPercent, long memoryLimit, int fromGOGC, long fromGOMEMLIMIT) = GoGCPacerKnobsProbe();

        Assert.AreEqual(fromGOGC, gcPercent, "gcController.gcPercent against readGOGC()");
        Assert.AreEqual(fromGOMEMLIMIT, memoryLimit, "gcController.memoryLimit against readGOMEMLIMIT()");
    }

    [TestMethod]
    public void TheSaveAndRestoreIdiomDisablesAndRestoresTheController()
    {
        // `defer debug.SetGCPercent(debug.SetGCPercent(-1))`, the standard test idiom (sync's TestPool).
        int before = GoGCPacerKnobsProbe().gcPercent;
        nint previous = debug.SetGCPercent(-1);
        int disabled = GoGCPacerKnobsProbe().gcPercent;
        debug.SetGCPercent(previous);
        int restored = GoGCPacerKnobsProbe().gcPercent;

        Assert.AreEqual(before, (int)previous, "SetGCPercent answers the controller's value");
        Assert.AreEqual(-1, disabled, "SetGCPercent(-1) disables collection in the controller");
        Assert.AreEqual(before, restored, "the idiom restores the controller");
    }

    [TestMethod]
    public void GOGCAndGOMEMLIMITFromTheEnvironmentReachTheKnobs()
    {
        // The startup step, run over a substituted environment snapshot, then read back through the
        // public knobs. Each read is a round trip, so both knobs end where they started.
        GoGCPacerFromEnvironmentProbe(["GOGC=50", "GOMEMLIMIT=64MiB"], () =>
        {
            Assert.AreEqual((nint)50, RoundTripGCPercent(), "GOGC=50");
            Assert.AreEqual(64 * MiB, debug.SetMemoryLimit(-1), "GOMEMLIMIT=64MiB");
        });

        GoGCPacerFromEnvironmentProbe(["GOGC=off"], () =>
        {
            Assert.AreEqual((nint)(-1), RoundTripGCPercent(), "GOGC=off");
            Assert.AreEqual(long.MaxValue, debug.SetMemoryLimit(-1), "no GOMEMLIMIT");
        });

        GoGCPacerFromEnvironmentProbe([], () =>
        {
            Assert.AreEqual((nint)100, RoundTripGCPercent(), "no GOGC");
        });
    }

    private static nint RoundTripGCPercent()
    {
        nint current = debug.SetGCPercent(100);
        debug.SetGCPercent(current);
        return current;
    }

    private static ulong ReadUint64Metric(string name)
    {
        slice<metrics.Sample> samples = new(1);
        samples[0].Name = name;
        metrics.Read(samples);

        Assert.AreEqual(metrics.KindUint64, metrics.Kind(samples[0].Value), name);
        return metrics.Uint64(samples[0].Value);
    }
}
