using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards the ROOT frames runtime.Callers reports at the bottom of a goroutine's stack
/// (runtime/managed_impl.cs, captureCallers). Go's unwinder ends every goroutine at
/// <c>runtime.goexit</c>, the return address newproc plants, and a test goroutine at
/// <c>testing.tRunner</c> above it; Go's own tests read that bottom (runtime's testCallersEqual
/// drops the LAST frame, so a stack that ends at the test function loses the test function).
/// Red while the managed walk stops at the last converted frame.
/// </summary>
[TestClass]
public class StackRootTests
{
    [TestMethod]
    public void AGoStatementGoroutineEndsAtGoexit()
    {
        List<(string function, string file, long line)> frames = stackrootprobe_package.CallersOnGoroutine();

        Assert.IsTrue(frames.Count >= 3, $"expected Callers, the probe body and a root; got {Describe(frames)}");
        Assert.AreEqual("runtime.Callers", frames[0].function);
        Assert.AreEqual("runtime.goexit", frames[^1].function, Describe(frames));
        Assert.AreEqual("runtime/asm_amd64.s", frames[^1].file);
        Assert.AreEqual(1700L, frames[^1].line);
    }

    [TestMethod]
    public void AStackRootFrameReportsItsGoFunctionAboveGoexit()
    {
        List<(string function, string file, long line)> frames = RunUnderRoot();

        Assert.IsTrue(frames.Count >= 3, Describe(frames));
        Assert.AreEqual("runtime.goexit", frames[^1].function, Describe(frames));
        Assert.AreEqual("testing.tRunner", frames[^2].function, Describe(frames));
        Assert.AreEqual("testing/testing.go", frames[^2].file);
        Assert.AreEqual(1792L, frames[^2].line);
        Assert.IsTrue(frames[^3].function.StartsWith("stackrootprobe."), Describe(frames));
    }

    // A thread no goroutine owns and no root marks is not a Go stack: nothing is appended.
    [TestMethod]
    public void AForeignThreadGetsNoRoot()
    {
        List<(string function, string file, long line)> frames = Task.Run(stackrootprobe_package.CallersHere).GetAwaiter().GetResult();

        Assert.AreNotEqual("runtime.goexit", frames[^1].function, Describe(frames));
        Assert.IsTrue(frames[^1].function.StartsWith("stackrootprobe."), Describe(frames));
    }

    // A buffer too small for the whole stack is filled from the top, as Go fills it: no root is
    // forced in at the cost of a real frame.
    [TestMethod]
    public void ATruncatedWalkKeepsTheTopFrames()
    {
        List<(string function, string file, long line)> full = RunUnderRoot();
        List<(string function, string file, long line)> truncated = RunUnderRoot(capacity: full.Count - 1);

        Assert.AreEqual(full.Count - 1, truncated.Count, Describe(truncated));
        Assert.AreEqual("testing.tRunner", truncated[^1].function, Describe(truncated));
    }

    // skip counts the roots as frames, as Go's skip counts goexit.
    [TestMethod]
    public void SkipCountsTheRootFrames()
    {
        List<(string function, string file, long line)> full = RunUnderRoot();
        List<(string function, string file, long line)> last = RunUnderRoot(skip: full.Count - 1);

        Assert.AreEqual(1, last.Count, Describe(last));
        Assert.AreEqual("runtime.goexit", last[0].function);
        Assert.AreEqual(0, RunUnderRoot(skip: full.Count).Count, "skipping every frame must report none");
    }

    private static List<(string function, string file, long line)> RunUnderRoot(int capacity = 32, int skip = 0)
    {
        List<(string function, string file, long line)> result = null!;

        Task.Factory.StartNew(() =>
        {
            using Goroutine.Scope goroutine = Goroutine.Enter();
            result = TestRoot(capacity, skip);
        }, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();

        return result;
    }

    [GoStackRoot("testing.tRunner", "testing/testing.go", 1792)]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<(string function, string file, long line)> TestRoot(int capacity, int skip) =>
        stackrootprobe_package.CallersHere(capacity, skip);

    private static string Describe(List<(string function, string file, long line)> frames) =>
        string.Join(" | ", frames.ConvertAll(frame => $"{frame.function} {frame.file}:{frame.line}"));
}
