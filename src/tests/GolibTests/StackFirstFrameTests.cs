// StackFirstFrameTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using go.testing_runtime;

namespace GolibTests;

// runtime.Stack's FIRST rendered frame is its caller -- under every JIT configuration.
//
// Until 2026-09-04 runtime.Stack was the one traceback entry point without [MethodImpl(NoInlining)],
// and it walked with `skipFrames: 1`, a COUNT that assumes Stack owns frame 0. When the JIT inlined
// Stack into its caller (measured on the banked net/http row at its configuration of record --
// Release, tiered on; two preserved records, trains 20 and 22), frame 0 WAS the caller and the skip
// removed it: the main goroutine's block began at `net/http_test.goroutineLeaked()` with the
// `interestingGoroutines()` frame above it missing. That is the one frame Go's own goroutine-leak
// filter reads to drop the main goroutine (main_test.go's interestingGoroutines is keep-unless-
// contains over the block), so the host counted ITSELF as a leaked goroutine, TestMain exited 1, no
// results file was written, and the sweep read FAIL while the comparison record read 1,345/1,345.
//
// The property is tiering-dependent (the class the Q14 note named), so this class is run at Debug,
// at Release + DOTNET_TieredCompilation=0, and at Release + tiering on: one assembly, three
// configurations, the same assertions.
[TestClass]
public class StackFirstFrameTests
{
    private static string CaptureStack(bool all)
    {
        byte[] storage = new byte[256 * 1024];
        slice<byte> buf = new(storage);
        nint written = runtime_package.Stack(buf, all);

        return Encoding.UTF8.GetString(storage, 0, (int)written);
    }

    // The caller Stack must render first. NoInlining on the WRAPPER isolates the axis under test:
    // whatever the JIT does to Stack, this frame is guaranteed to exist, so if it is missing from the
    // rendering the walk started too deep -- Stack's own boundary is wrong, not the caller's.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string CaptureFromThisFrame() => CaptureStack(all: false);

    private static string[] FrameLines(string block)
    {
        // Go's format: a header line, then per frame `<pkg>.<Func>()` followed by a tab-indented
        // position line. The function lines are the ones a reader greps.
        return block.Split('\n')
            .Skip(1)
            .Where(line => line.Length > 0 && line[0] != '\t')
            .ToArray();
    }

    // ARM (i) -- THE GUARD. The first rendered frame is Stack's immediate caller, never its
    // caller's caller.
    [TestMethod]
    public void TheFirstRenderedFrameIsTheCaller()
    {
        string text = CaptureFromThisFrame();
        string callingBlock = text.Split("\n\n")[0];
        string[] frames = FrameLines(callingBlock);

        Console.WriteLine(callingBlock);

        Assert.IsTrue(frames.Length >= 2, $"expected at least two frames, got:\n{callingBlock}");

        // CaptureStack is the frame that called Stack; CaptureFromThisFrame the one above it.
        Assert.IsTrue(frames[0].Contains(nameof(CaptureStack), StringComparison.Ordinal),
            $"the first rendered frame must be Stack's caller ({nameof(CaptureStack)}); the walk started one frame too deep:\n{callingBlock}");
        Assert.IsTrue(frames[1].Contains(nameof(CaptureFromThisFrame), StringComparison.Ordinal),
            $"the second rendered frame must be the caller's caller ({nameof(CaptureFromThisFrame)}):\n{callingBlock}");

        // And Stack's OWN frame is never rendered: Go's runtime.Stack starts at its caller.
        Assert.IsFalse(frames.Any(line => line.EndsWith("runtime.Stack()", StringComparison.Ordinal)),
            $"Stack rendered its own frame:\n{callingBlock}");
    }

    // The exact algorithm of net/http's main_test.go interestingGoroutines, transcribed: split the
    // dump on blank lines, drop each block's header, keep a block unless its stack contains one of
    // the eleven substrings. The main goroutine survives ONLY if its own `interestingGoroutines`
    // frame is missing -- which is exactly the shape the records showed.
    private static readonly string[] GoLeakFilter =
    [
        "testing.(*M).before.func1",
        "os/signal.signal_recv",
        "created by net.startServer",
        "created by testing.RunTests",
        "closeWriteAndWait",
        "testing.Main(",
        "runtime.goexit",
        "created by runtime.gc",
        "interestingGoroutines",
        "runtime.MHeap_Scavenger",
    ];

    // Named EXACTLY as Go's helper, because the filter matches the substring `interestingGoroutines`
    // against the rendered frame name, and this method is that frame here.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string[] interestingGoroutines()
    {
        string dump = CaptureStack(all: true);
        List<string> survivors = [];

        foreach (string block in dump.Split("\n\n"))
        {
            int newline = block.IndexOf('\n');
            string stack = newline < 0 ? "" : block[(newline + 1)..].Trim();

            if (stack.Length == 0 || GoLeakFilter.Any(needle => stack.Contains(needle, StringComparison.Ordinal)))
                continue;

            survivors.Add(stack);
        }

        return [.. survivors];
    }

    // ARM (iii) -- THE ROW'S SHAPE. Driven through the real host: a TestMain that runs Go's leak
    // filter over runtime.Stack(all) and reports its survivors. The main goroutine (the thread
    // running TestMain) must NOT survive: its block carries `interestingGoroutines`, so Go's filter
    // drops it, exactly as it does in Go. Before the fix it survived -- the frame was missing.
    [TestMethod]
    public void TheMainGoroutineUnderTestMainIsDroppedByGoLeakFilter()
    {
        string[]? survivors = null;
        TestRegistry registry = new("guard", []);

        registry.SetTestMain(_ => survivors = interestingGoroutines());

        TestHost.Run(registry, []);

        Assert.IsNotNull(survivors, "TestMain never ran, so nothing was measured");

        string[] mainSurvivors = survivors!
            .Where(stack => stack.Contains("TestMain", StringComparison.Ordinal) ||
                            stack.Contains(nameof(TheMainGoroutineUnderTestMainIsDroppedByGoLeakFilter), StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(0, mainSurvivors.Length,
            "Go's leak filter kept the main goroutine -- its `interestingGoroutines` frame is missing from the rendered block:\n" +
            string.Join("\n---\n", mainSurvivors));
    }

    // ARM (ii) -- A MEASUREMENT, not yet a guard: what a FOREIGN parked goroutine's block carries.
    // Today: a truthful header (the wait reason) and the frameless placeholder. Go's leak filter can
    // match nothing in it, so such a goroutine survives every round -- the second mechanism the
    // train-22 record showed (2 instances of the placeholder). The remedy (a `created by` line from
    // a creator recorded at goroutine start) is sized and waits for its cut; this test states the
    // present truth so the cut has a before-reading to move.
    [TestMethod]
    public void AForeignParkedGoroutineBlockIsHeaderAndPlaceholderToday()
    {
        channel<int> park = new(0);
        Goroutine.Start(() => park.Receive());

        // Let the goroutine reach its park before the dump is taken.
        for (int i = 0; i < 200 && !Goroutine.Snapshot().Any(g => g.State == GoroutineState.Parked); i++)
            System.Threading.Thread.Sleep(5);

        string dump = CaptureStack(all: true);
        string[] blocks = dump.Split("\n\n");
        string? parked = blocks.Skip(1).FirstOrDefault(b => b.StartsWith("goroutine ", StringComparison.Ordinal) && b.Contains("[chan receive]", StringComparison.Ordinal));

        Console.WriteLine(dump);

        Assert.IsNotNull(parked, $"no foreign block with a `chan receive` header was rendered:\n{dump}");
        Assert.IsTrue(parked!.Contains("[stack unavailable", StringComparison.Ordinal),
            "the foreign block's body is expected to be the placeholder until the created-by remedy lands; it is not:\n" + parked);

        park.Close();
    }
}
