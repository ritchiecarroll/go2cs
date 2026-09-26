using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards that a converted //go:linkname forwarder leaves no frame in runtime.Callers. Go binds a
/// pull to the target's symbol, so the puller has no frame; the converter emits a forwarder method
/// and marks it [StackTraceHidden], and the managed callers must skip it. Red while it counts the
/// forwarder: a block profile's first frame then read runtime/pprof.blockevent where Go records
/// the test's own function (TestBlockProfileBias under DOTNET_JitNoInline=1, linux, 2026-09-26).
/// </summary>
[TestClass]
public class RuntimeCallerForwarderTests
{
    [TestMethod]
    public void AHiddenForwarderLeavesNoFrame()
    {
        slice<uintptr> pc = GoForwardedCallersProbe();

        var frames = CallersFrames(pc);
        var (first, _) = frames.Next();
        var (second, _) = frames.Next();

        Assert.AreEqual("runtime.goCallersRecordingProbe", (string)first.Function, "the first frame must be the recording function");
        Assert.AreEqual("runtime.GoForwardedCallersProbe", (string)second.Function, "the frame above the forwarder must be its caller, never the forwarder");
    }

    // The printed Go traceback (a crash report, runtime.Stack, debug.Stack) renders through the same
    // frame walk, so it must agree with Callers: a panic raised through a forwarder names no forwarder.
    [TestMethod]
    public void AHiddenForwarderIsNotInAPanicTraceback()
    {
        string trace = GoForwardedPanicTraceProbe();

        StringAssert.Contains(trace, "runtime.goPanicRaisingProbe()", "the traceback must name the raising function");
        StringAssert.Contains(trace, "runtime.GoForwardedPanicTraceProbe()", "the traceback must name the forwarder's caller");
        Assert.IsFalse(trace.Contains("goHiddenPanicForwarderProbe"), $"the traceback must not name the forwarder:\n{trace}");
    }
}
