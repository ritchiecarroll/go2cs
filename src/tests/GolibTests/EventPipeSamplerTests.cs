using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.CpuProfiler;

namespace GolibTests;

/// <summary>
/// Guards the opt-in CPU sampler's session lifetime (go2cs.CpuProfiler/EventPipeSampler.cs, section 11
/// of DESIGN-managed-profiling.md): Start opens a SampleProfiler session on this process and Stop
/// drains its trace; a session that cannot open (DOTNET_EnableDiagnostics=0 is that class) leaves no
/// samples and throws nothing. The sampler is driven directly here, not through runtime's seam, so
/// RuntimeCpuSamplerSeamTests' proxy stays the registered sampler. Writing samples is the next piece
/// (frames); in this piece Stop writes none.
/// </summary>
[TestClass]
public class EventPipeSamplerTests
{
    private static int s_writes;

    private static void CountingWriter(int64 nanotime, slice<uintptr> stack, unsafe_package.Pointer tag) => s_writes++;

    [TestMethod]
    public void ASessionOpensOnThisProcessAndDrainsATrace()
    {
        var sampler = new EventPipeSampler(EventPipeSampler.OpenInProcessSession);

        sampler.Start(100);
        Assert.IsTrue(sampler.LastSessionOpened, "a SampleProfiler session must open on this process");

        // Burn a little CPU so the session has samples to carry.
        var clock = Stopwatch.StartNew();
        ulong spin = 1;

        while (clock.ElapsedMilliseconds < 200)
            spin = spin * 6364136223846793005UL + 1442695040888963407UL;

        GC.KeepAlive(spin);
        sampler.Stop(CountingWriter);

        Assert.IsTrue(sampler.LastTraceBytes > 0, "stopping the session must drain a non-empty trace");
    }

    [TestMethod]
    public void ASessionThatCannotOpenLeavesNoSamplesAndNoThrow()
    {
        var sampler = new EventPipeSampler(() => throw new ServerNotAvailableProbeException());
        s_writes = 0;

        sampler.Start(100);
        sampler.Stop(CountingWriter);

        Assert.IsFalse(sampler.LastSessionOpened);
        Assert.AreEqual(0L, sampler.LastTraceBytes);
        Assert.AreEqual(0, s_writes, "a sampler with no session writes no sample");
    }

    private sealed class ServerNotAvailableProbeException() : Exception("diagnostic port not available (probe)");
}
