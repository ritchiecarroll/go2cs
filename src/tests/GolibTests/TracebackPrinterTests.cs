using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards the frames a PRINTED traceback carries (runtime/managed_impl.cs, appendGoFrames): the ones
/// runtime.Callers counts (captureCallers' rule), a GoStackRoot host method as the Go frame it stands in
/// for, and Go's elision of the middle of a deep stack. Go's traceback parsers (runtime's
/// parseTraceback) require every function line to carry a tab-indented source line beneath it, which a
/// host frame such as <c>System.Threading.ExecutionContext.RunInternal()</c> does not.
/// Red while the printer renders every CLR frame the stack trace holds.
/// </summary>
[TestClass]
public class TracebackPrinterTests
{
    [TestMethod]
    public void AForeignThreadPrintsOnlyGoFrames()
    {
        List<(string function, string position)> frames = Parse(Task.Run(tracebackprobe_package.StackText).GetAwaiter().GetResult());

        Assert.AreEqual(1, frames.Count, Describe(frames));
        Assert.AreEqual("tracebackprobe.StackText()", frames[0].function);
    }

    [TestMethod]
    public void AStackRootPrintsItsGoFrameAndNothingBelowIt()
    {
        List<(string function, string position)> frames = Parse(RunUnderRoot(() => tracebackprobe_package.Deep(1)));

        CollectionAssert.AreEqual(
            new[] { "tracebackprobe.StackText()", "tracebackprobe.Deep()", "tracebackprobe.Deep()", "testing.tRunner()" },
            frames.ConvertAll(frame => frame.function), Describe(frames));
        Assert.AreEqual("\ttesting/testing.go:1792", frames[^1].position);
    }

    // Go prints tracebackInnerFrames + tracebackOuterFrames frames whole ...
    [TestMethod]
    public void AStackOfExactlyTheLimitIsNotElided()
    {
        List<(string function, string position)> frames = Parse(Task.Run(() => tracebackprobe_package.Deep(98)).GetAwaiter().GetResult());

        Assert.AreEqual(100, frames.Count, Describe(frames));
        Assert.IsFalse(frames.Any(frame => frame.function.EndsWith("frames elided...")), Describe(frames));
    }

    // ... and past that, the first 50, the count it drops, and the last 50.
    [TestMethod]
    public void ADeeperStackElidesItsMiddleAsGoDoes()
    {
        List<(string function, string position)> frames = Parse(Task.Run(() => tracebackprobe_package.Deep(108)).GetAwaiter().GetResult());

        Assert.AreEqual(101, frames.Count, Describe(frames));
        Assert.AreEqual("tracebackprobe.StackText()", frames[0].function);
        Assert.AreEqual("...10 frames elided...", frames[50].function);
        Assert.AreEqual("tracebackprobe.Deep()", frames[^1].function);
    }

    private static string RunUnderRoot(System.Func<string> body)
    {
        string result = null!;

        Task.Factory.StartNew(() =>
        {
            using Goroutine.Scope goroutine = Goroutine.Enter();
            result = TestRoot(body);
        }, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();

        return result;
    }

    [GoStackRoot("testing.tRunner", "testing/testing.go", 1792)]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static string TestRoot(System.Func<string> body) => body();

    // The frames of the ONE goroutine block, read the way runtime's parseTraceback reads them: a
    // function line, then its tab-indented source line (an elision line stands alone).
    private static List<(string function, string position)> Parse(string traceback)
    {
        string[] lines = traceback.TrimEnd('\n').Split('\n');

        Assert.IsTrue(lines[0].StartsWith("goroutine "), traceback);

        List<(string function, string position)> frames = [];

        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("...") && lines[i].EndsWith("frames elided..."))
            {
                frames.Add((lines[i], ""));
                continue;
            }

            Assert.IsTrue(i + 1 < lines.Length && lines[i + 1].StartsWith('\t'), $"missing source line under {lines[i]}:\n{traceback}");
            frames.Add((lines[i], lines[i + 1]));
            i++;
        }

        return frames;
    }

    private static string Describe(List<(string function, string position)> frames) =>
        string.Join(" | ", frames.ConvertAll(frame => frame.function));
}
