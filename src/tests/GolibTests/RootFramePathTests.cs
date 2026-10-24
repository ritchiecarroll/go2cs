using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards the FILE a modelled root frame reports (runtime/managed_impl.cs: the GoStackRoot frame and the
/// runtime.goexit tail). A recorded Go frame's file is rooted at the link-time root, defaultGOROOT, when the
/// program has one (GoPositionMapRecord.ResolveGoFile), and a modelled root frame must be rooted the same
/// way, in Callers' Frame.File and in the printed traceback alike: runtime/debug's TestStack, run by the
/// pipeline with GOROOT set, expects testing.tRunner's line as GOROOT/src/testing/testing.go. With no
/// link-time root the recorded form stands, as Go's -trimpath form does. Red while the root frames report
/// the recorded form whatever the root.
/// </summary>
[TestClass]
public class RootFramePathTests
{
    private const string Root = "/go/root";

    private static T WithDefaultGoroot<T>(string root, Func<T> body)
    {
        string previous = runtime_package.GoSetDefaultGoroot(root);

        try
        {
            return body();
        }
        finally
        {
            runtime_package.GoSetDefaultGoroot(previous);
        }
    }

    private static T RunUnderRoot<T>(Func<T> body)
    {
        T result = default!;

        Task.Factory.StartNew(() =>
        {
            using Goroutine.Scope goroutine = Goroutine.Enter();
            result = TestRoot(body);
        }, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();

        return result;
    }

    [GoStackRoot("testing.tRunner", "testing/testing.go", 2036)]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static T TestRoot<T>(Func<T> body) => body();

    [TestMethod]
    public void CallersRootsTheModelledFramesAtTheLinkTimeRoot()
    {
        List<(string function, string file, long line)> frames = WithDefaultGoroot(Root, () => RunUnderRoot(() => stackrootprobe_package.CallersHere(32, 0)));

        Assert.AreEqual("runtime.goexit", frames[^1].function);
        Assert.AreEqual(Root + "/src/runtime/asm_amd64.s", frames[^1].file, "goexit's file, rooted as Go's own frame is");
        Assert.AreEqual("testing.tRunner", frames[^2].function);
        Assert.AreEqual(Root + "/src/testing/testing.go", frames[^2].file, "tRunner's file, rooted as runtime/debug's TestStack expects");
    }

    [TestMethod]
    public void TheTracebackRootsTheModelledFrameAtTheLinkTimeRoot()
    {
        string traceback = WithDefaultGoroot(Root, () => RunUnderRoot(tracebackprobe_package.StackText));

        StringAssert.Contains(traceback, "testing.tRunner()\n\t" + Root + "/src/testing/testing.go:2036\n");
    }

    // R's spliced panicking frames (runtime.gopanic and the fault frames) are interned root frames too:
    // one Callers result must not mix the rooted recorded frames with relative spliced ones.
    [TestMethod]
    public void TheSplicedPanicFramesAreRootedToo()
    {
        List<(string function, string file)> rooted = WithDefaultGoroot(Root, panicframesprobe_package.plainPanicFiles);
        List<(string function, string file)> recorded = WithDefaultGoroot("", panicframesprobe_package.plainPanicFiles);

        int at = rooted.FindIndex(frame => frame.function == "runtime.gopanic");
        Assert.IsTrue(at >= 0, $"no runtime.gopanic: {string.Join(" | ", rooted)}");
        Assert.AreEqual(Root + "/src/runtime/panic.go", rooted[at].file, "the spliced gopanic's file under a link-time root");
        Assert.AreEqual("runtime/panic.go", recorded[recorded.FindIndex(frame => frame.function == "runtime.gopanic")].file, "and its recorded form with none");
    }

    [TestMethod]
    public void WithNoLinkTimeRootTheRecordedFormStands()
    {
        List<(string function, string file, long line)> frames = WithDefaultGoroot("", () => RunUnderRoot(() => stackrootprobe_package.CallersHere(32, 0)));
        string traceback = WithDefaultGoroot("", () => RunUnderRoot(tracebackprobe_package.StackText));

        Assert.AreEqual("testing/testing.go", frames[^2].file);
        Assert.AreEqual("runtime/asm_amd64.s", frames[^1].file);
        StringAssert.Contains(traceback, "testing.tRunner()\n\ttesting/testing.go:2036\n");
    }
}
