using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using Δruntime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// The park-accounting contract of <c>docs/phase4/DESIGN-cooperative-scheduler.md</c> §5.3, at the
/// level no converted Go program can reach.
/// </summary>
/// <remarks>
/// <para>
/// The behavioral guard <c>GoroutineWaitState</c> proves the end-to-end fact — a goroutine blocked on
/// a mutex, a channel, a select or a WaitGroup reports Go's own word for that wait, and stops
/// reporting it once released — by comparing a normalized reading against <c>go run</c>. What it
/// cannot see is anything below the traceback text: the enum's own strings, the scope's behavior on a
/// thread that is not a goroutine, and whether the registry the dump enumerates is the same set
/// <c>runtime.NumGoroutine</c> counts. Those are here.
/// </para>
/// <para>
/// Nothing here spawns a park through a real primitive: driving <c>Goroutine.Park</c> directly is the
/// point, because it isolates the accounting from every wait protocol wrapped around it.
/// </para>
/// </remarks>
[TestClass]
public class GoroutineParkAccountingTests
{
    private const int TimeoutMs = 30000;

    // Wide enough that a whole-process dump is never truncated. A truncated dump would fail the
    // header-count test as a disagreement rather than as the buffer size it actually was.
    private const int DumpSize = 1 << 20;

    [TestMethod]
    public void EveryWaitReasonCarriesGosOwnString()
    {
        // The STRINGS are the contract, not the enum names: Go's own tests grep the bracketed word of
        // a traceback header (runtime/pprof's awaitBlockedGoroutine builds a regex around
        // `\[sync\.Mutex\.Lock\]`), so a typo here is a silent divergence no compile can catch. Every
        // expectation below is copied from $GOROOT/src/runtime/runtime2.go's waitReasonStrings.
        Dictionary<WaitReason, string> go = new()
        {
            [WaitReason.Zero] = "",
            [WaitReason.IOWait] = "IO wait",
            [WaitReason.ChanReceiveNilChan] = "chan receive (nil chan)",
            [WaitReason.ChanSendNilChan] = "chan send (nil chan)",
            [WaitReason.Select] = "select",
            [WaitReason.SelectNoCases] = "select (no cases)",
            [WaitReason.ChanReceive] = "chan receive",
            [WaitReason.ChanSend] = "chan send",
            [WaitReason.Semacquire] = "semacquire",
            [WaitReason.Sleep] = "sleep",
            [WaitReason.SyncCondWait] = "sync.Cond.Wait",
            [WaitReason.SyncMutexLock] = "sync.Mutex.Lock",
            [WaitReason.SyncRWMutexRLock] = "sync.RWMutex.RLock",
            [WaitReason.SyncRWMutexLock] = "sync.RWMutex.Lock"
        };

        // Both directions. The forward one catches a wrong string; this one catches a member added
        // without its Go text, which would otherwise reach a traceback as an unchecked word.
        foreach (WaitReason reason in Enum.GetValues<WaitReason>())
        {
            Assert.IsTrue(go.ContainsKey(reason),
                $"{reason} has no expectation here — add its verbatim waitReasonStrings entry");

            Assert.AreEqual(go[reason], WaitReasons.Text(reason), $"wrong Go string for {reason}");
        }

        Assert.AreEqual(go.Count, Enum.GetValues<WaitReason>().Length);

        // Out of range answers Go's own waitReason.String fallback rather than a .NET enum name: a
        // traceback is observable output, so even the impossible case renders in Go's vocabulary.
        Assert.AreEqual("unknown wait reason", WaitReasons.Text((WaitReason)9999));

        // Zero is the not-parked encoding, so it is never a header word.
        Assert.AreEqual(go.Count - 1, WaitReasons.Parked().Length);

        foreach (WaitReason reason in WaitReasons.Parked())
            Assert.AreNotEqual(WaitReason.Zero, reason);
    }

    [TestMethod]
    public void ParkPublishesTheReasonAndUnparkClearsIt()
    {
        // The negative control the whole contract rests on: a goroutine that is not inside a park
        // scope must read Running, or every "is it parked" answer downstream is vacuous.
        Observed before = ObserveOnAGoroutine(static _ => { });

        Assert.AreEqual(GoroutineState.Running, before.State);
        Assert.AreEqual(WaitReason.Zero, before.Reason);

        Observed inside = ObserveOnAGoroutine(static observe =>
        {
            using (Goroutine.Park(WaitReason.ChanReceive))
                observe();
        });

        Assert.AreEqual(GoroutineState.Parked, inside.State);
        Assert.AreEqual(WaitReason.ChanReceive, inside.Reason);

        // And the unpark half — a reason that were set but never cleared would leave every finished
        // goroutine claiming a wait it is no longer in.
        Observed after = ObserveOnAGoroutine(static observe =>
        {
            using (Goroutine.Park(WaitReason.ChanReceive)) { }

            observe();
        });

        Assert.AreEqual(GoroutineState.Running, after.State);
        Assert.AreEqual(WaitReason.Zero, after.Reason);
    }

    [TestMethod]
    public void NestedParkRestoresTheEnclosingReason()
    {
        // An inner park must not report the OUTER one as having woken. Scopes restore the reason they
        // replaced rather than clearing to Zero, which is why this reads SyncMutexLock and not
        // Running after the inner scope closes.
        Observed observed = ObserveOnAGoroutine(static observe =>
        {
            using (Goroutine.Park(WaitReason.SyncMutexLock))
            {
                using (Goroutine.Park(WaitReason.Semacquire)) { }

                observe();
            }
        });

        Assert.AreEqual(GoroutineState.Parked, observed.State);
        Assert.AreEqual(WaitReason.SyncMutexLock, observed.Reason);
    }

    [TestMethod]
    public void ParkOnAThreadWithNoGoroutineIdentityIsInert()
    {
        // A RAW thread, not this one: golib's module initializer registers whichever thread first
        // touched golib as the MAIN goroutine, and in an MSTest host that is the test thread — it has
        // an identity, it is simply not "on a goroutine" (GoroutineExecutorTests holds that half).
        // The threads this test is about are the ones that never ran Go code at all: a BCL callback,
        // an IO completion, the finalizer, time's timer service thread. Every park site in the corpus
        // is reachable from one, so the scope has to be a no-op there rather than minting an identity,
        // touching the live count, or throwing.
        int before = Goroutine.Count;

        bool hadIdentity = true;
        bool mintedIdentity = true;
        Exception? failure = null;

        Thread foreign = new(() =>
        {
            try
            {
                hadIdentity = Goroutine.Current is not null;

                using (Goroutine.Park(WaitReason.Sleep))
                    mintedIdentity = Goroutine.Current is not null;
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        })
        {
            IsBackground = true
        };

        foreign.Start();

        Assert.IsTrue(foreign.Join(TimeoutMs), "the foreign thread did not finish");
        Assert.IsNull(failure, $"parking on a non-goroutine thread threw: {failure}");
        Assert.IsFalse(hadIdentity, "a raw thread unexpectedly carried a goroutine identity");
        Assert.IsFalse(mintedIdentity, "an inert park minted an identity");

        Assert.AreEqual(before, Goroutine.Count, "an inert park changed the live count");
    }

    [TestMethod]
    public void StackEnumeratesExactlyTheGoroutinesNumGoroutineCounts()
    {
        // runtime.NumGoroutine answers from the registry's COUNT while Stack(all: true) enumerates the
        // registry's MEMBERS, and Go's own TestNumGoroutine holds the two against each other: it
        // counts the headers in a whole-process dump and requires the total to equal NumGoroutine().
        // The two are separate reads of one registry — a slot is added before the count is bumped and
        // removed before it is dropped — so they agree AT REST and can differ for as long as a
        // goroutine is registering or retiring. Other tests in this assembly leave goroutines draining
        // behind them, so this waits for the quiet rather than asserting into the churn.
        using ManualResetEventSlim release = new(false);
        using CountdownEvent arrived = new(Parked);
        using CountdownEvent left = new(Parked);

        for (int i = 0; i < Parked; i++)
        {
            Goroutine.Start(() =>
            {
                arrived.Signal();

                using (Goroutine.Park(WaitReason.ChanReceive))
                    release.Wait();

                left.Signal();
            });
        }

        Assert.IsTrue(arrived.Wait(TimeoutMs), "not every goroutine started");

        try
        {
            int headers = 0;
            int counted = 0;

            bool agreed = SpinWait.SpinUntil(() =>
            {
                // Counted FIRST and from the same instant as the dump it is compared against: reading
                // the count after the dump would let a goroutine registered in between look like a
                // disagreement.
                counted = (int)Δruntime.NumGoroutine();
                headers = CountGoroutineHeaders(WholeProcessDump());

                return headers == counted;
            }, TimeoutMs);

            Assert.IsTrue(agreed,
                $"Stack(all: true) printed {headers} goroutine headers against NumGoroutine()'s {counted}");

            // And the enumeration must actually contain the parked ones — an agreement between two
            // empty answers would satisfy the equality above and mean nothing.
            Assert.IsTrue(counted > Parked,
                $"expected more than the {Parked} parked goroutines plus main, got {counted}");

            string dump = WholeProcessDump();
            int parkedHeaders = CountOccurrences(dump, "[" + WaitReasons.Text(WaitReason.ChanReceive) + "]:");

            Assert.IsTrue(parkedHeaders >= Parked,
                $"expected at least {Parked} [chan receive] headers, found {parkedHeaders}");
        }
        finally
        {
            release.Set();

            // JOIN before the using scope disposes `release`. Set() only WAKES the workers; a
            // goroutine still inside ManualResetEventSlim.Wait when the event is disposed throws
            // ObjectDisposedException on its own thread, where nothing catches it -- so the test
            // HOST dies and the run reports "Test Run Aborted", which is an UNMEASURED suite, not
            // a failure (an abort silently costs a lane its whole GolibTests verdict). Measured:
            // this fired once under contention -- 10/10 clean solo, both full suites clean -- so it
            // is rare, which is exactly what makes it worth closing rather than chasing later.
            Assert.IsTrue(left.Wait(TimeoutMs), "a parked goroutine never left release.Wait()");
        }
    }

    [TestMethod]
    public void StackNamesTheCurrentGoroutineRunningAndNeverFabricatesForeignFrames()
    {
        string dump = WholeProcessDump();

        // The calling goroutine is running by definition, and its block is first — Go dumps the
        // current goroutine ahead of the rest.
        Assert.IsTrue(dump.StartsWith("goroutine ", StringComparison.Ordinal),
            $"a dump must open with a goroutine header; got: {First(dump)}");

        Assert.IsTrue(First(dump).EndsWith("[running]:", StringComparison.Ordinal),
            $"the calling goroutine must be running; got: {First(dump)}");

        // Every other block carries the placeholder, not frames. This is the assertion that keeps the
        // Stage-A honesty property mechanical: the moment someone synthesizes plausible-looking frames
        // for a stack that was never walked, this fails.
        int others = CountGoroutineHeaders(dump) - 1;
        int placeholders = CountOccurrences(dump, "[stack unavailable:");

        Assert.AreEqual(others, placeholders,
            $"{others} foreign goroutine block(s) but {placeholders} placeholder(s) — a block gained frames from somewhere");

        // all: false is unchanged by any of this: one block, the caller's own.
        Assert.AreEqual(1, CountGoroutineHeaders(Dump(all: false)));
    }

    // Comfortably more than one, few enough to stay fast; the point is a plural enumeration, not scale
    // (GoroutineExecutorTests owns the capacity claim).
    private const int Parked = 8;

    private readonly record struct Observed(GoroutineState State, WaitReason Reason);

    // Runs `body` on a real goroutine and captures that goroutine's accounting at the instant `body`
    // calls the action handed to it. Nothing is asserted inside the goroutine: no containment policy
    // is installed here, so an AssertFailedException escaping a goroutine root would take the whole
    // test host down (that fidelity is the executor's own contract). Observations come out; the
    // assertions happen on the test's thread.
    private static Observed ObserveOnAGoroutine(Action<Action> body)
    {
        using ManualResetEventSlim finished = new(false);

        GoroutineState state = default;
        WaitReason reason = default;

        Goroutine.Start(() =>
        {
            Goroutine self = Goroutine.Current!;

            body(() =>
            {
                state = self.State;
                reason = self.Reason;
            });

            finished.Set();
        });

        Assert.IsTrue(finished.Wait(TimeoutMs), "goroutine did not run");

        return new Observed(state, reason);
    }

    private static string WholeProcessDump() => Dump(all: true);

    private static string Dump(bool all)
    {
        slice<byte> buf = new(new byte[DumpSize]);
        nint written = Δruntime.Stack(buf, all);

        Assert.IsTrue(written > 0, "runtime.Stack wrote nothing");
        Assert.IsTrue(written < DumpSize, "the dump filled the buffer — it may be truncated");

        byte[] bytes = new byte[(int)written];

        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = buf[i];

        return Encoding.UTF8.GetString(bytes);
    }

    // Counts HEADER LINES rather than occurrences of the word: a frame name or a placeholder could
    // contain "goroutine" without being a header, and Go's own TestNumGoroutine is counting blocks.
    private static int CountGoroutineHeaders(string dump)
    {
        int headers = 0;

        foreach (string line in dump.Split('\n'))
        {
            if (line.StartsWith("goroutine ", StringComparison.Ordinal) && line.EndsWith("]:", StringComparison.Ordinal))
                headers++;
        }

        return headers;
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        int found = 0;

        for (int at = haystack.IndexOf(needle, StringComparison.Ordinal); at >= 0;
             at = haystack.IndexOf(needle, at + needle.Length, StringComparison.Ordinal))
        {
            found++;
        }

        return found;
    }

    private static string First(string dump)
    {
        int end = dump.IndexOf('\n');

        return end < 0 ? dump : dump[..end];
    }
}
