using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// runtime.Stack's FIRST rendered frame is its caller -- after the JIT has had every chance to inline
// Stack into that caller.
//
// The property is TIER-dependent, and a one-call guard cannot see it: a method called once lives at
// tier-0, which inlines nothing, so `skipFrames: 1` skips Stack's own frame and the caller renders
// first, old code and new alike (measured on windows/amd64 at both tiers by SUB-Q24, 2026-09-04, and
// on Linux by this lane's TestQuery arm). On the net/http row the calling function is HOT --
// afterTest calls interestingGoroutines 562 times in one run -- and the headered dumps showed the
// frame PRESENT for the first 174 calls and ABSENT from the 174th on: tier-1 promotion with dynamic
// PGO raised the inline budget at the hot call site, Stack was inlined into its caller, and the
// count-based skip removed the caller instead. Go's own leak filter then counted the main goroutine
// as a leak. runtime.Stack is now [MethodImpl(NoInlining)] and locates its own frame by IDENTITY
// (the frame whose method IS Stack), rendering from one above it, so the walk cannot start too deep
// under any tier.
//
// The guard takes the ROW'S OWN SHAPE and WARMS it. A three-line wrapper around Stack never reproduced
// the loss: it reached Tier1 with Dynamic PGO at 142 bytes of code while Stack compiled separately
// (DOTNET_JitDisasmSummary, this process), i.e. the JIT declined the inline at that site, and its
// control stayed green -- a vacuous arm, deleted. net/http's interestingGoroutines transcribed and
// called 100 x 8 rounds with 500 ms between them IS promoted here (Tier1 with Synthesized PGO,
// 5,037 bytes of code for 328 bytes of IL -- Stack inlined, as on the row, where the same method
// reached 8,389 bytes), and its control -- the count-based skip restored -- is RED: the calling
// goroutine survives its own filter, the row's exact failure. Under DOTNET_TieredCompilation=0
// nothing is promoted and both old and new code render the caller first, which is why this class
// surfaces only where tiering is on, and the configuration of record (tiering off) never shows it.
// SUB-Q24's one-call measurement stands beside this one: Stack is not inlined at call 1, at either
// tier, on windows/amd64 -- refuted at one call, confirmed at thirty.
// (2026-10-02: until this date the sentence read "which is why the row's `release-tiered` annotation
//  is where this class surfaces"; net/http dropped that annotation 2026-10-01 and no row carries one.)
[TestClass]
public class StackFirstFrameWarmTests
{
    // ROW-SHAPED ARM. The wrapper is net/http's interestingGoroutines transcribed: runtime.Stack(all),
    // split on blank lines, each block's header cut, a substring filter over the rest. Deliberately
    // NOT NoInlining, and deliberately this size: it is the call site the JIT inlined Stack into on
    // the net/http row once the method was hot (R1's transition at dump 174), where a three-line
    // wrapper stayed un-inlined in this process (measured by DOTNET_JitDisasmSummary: the wrapper
    // reached Tier1 with Dynamic PGO at 142 bytes of code while Stack compiled separately at 2,881).
    private static string[] interestingGoroutinesShaped()
    {
        byte[] storage = new byte[256 * 1024];
        slice<byte> buf = new(storage);
        nint written = runtime_package.Stack(buf, true);
        string dump = Encoding.UTF8.GetString(storage, 0, (int)written);
        List<string> gs = [];

        foreach (string g in dump.Split("\n\n"))
        {
            int newline = g.IndexOf('\n');
            string stack = (newline < 0 ? "" : g[(newline + 1)..]).Trim();

            if (stack.Length == 0 ||
                stack.Contains("testing.(*M).before.func1", StringComparison.Ordinal) ||
                stack.Contains("os/signal.signal_recv", StringComparison.Ordinal) ||
                stack.Contains("created by net.startServer", StringComparison.Ordinal) ||
                stack.Contains("created by testing.RunTests", StringComparison.Ordinal) ||
                stack.Contains("closeWriteAndWait", StringComparison.Ordinal) ||
                stack.Contains("testing.Main(", StringComparison.Ordinal) ||
                stack.Contains("runtime.goexit", StringComparison.Ordinal) ||
                stack.Contains("created by runtime.gc", StringComparison.Ordinal) ||
                stack.Contains("interestingGoroutinesShaped", StringComparison.Ordinal) ||
                stack.Contains("runtime.MHeap_Scavenger", StringComparison.Ordinal))
                continue;

            gs.Add(stack);
        }

        return [.. gs];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string[] WarmShaped(int calls, int rounds, int delayMs)
    {
        string[] last = [];

        for (int round = 0; round < rounds; round++)
        {
            for (int i = 0; i < calls; i++)
                last = interestingGoroutinesShaped();

            Thread.Sleep(delayMs);
        }

        return interestingGoroutinesShaped();
    }

    // The main goroutine's own block must be DROPPED by the filter after warm-up -- its first frame
    // must still read interestingGoroutinesShaped. If Stack was inlined into it and the count-based
    // skip removed the caller, the block begins at WarmShaped, nothing matches, and the calling
    // goroutine survives its own leak check: the net/http row's exact failure.
    [TestMethod]
    public void TheCallingGoroutineIsDroppedByTheRowsFilterAfterWarmUp()
    {
        string[] survivors = WarmShaped(calls: 100, rounds: 8, delayMs: 500);
        string[] mine = survivors.Where(s => s.Contains(nameof(WarmShaped), StringComparison.Ordinal)).ToArray();

        Assert.AreEqual(0, mine.Length,
            "the calling goroutine survived its own leak filter after warm-up -- its interestingGoroutinesShaped frame is missing (Stack was inlined into it and a count-based skip removed the caller):\n" + string.Join("\n---\n", mine));
    }
}
