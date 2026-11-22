#nullable enable

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// The s_p convoy row (COORD's ruling, docs/phase4/inbox/G/20261122T110220Z-COORD.md item 2): the scheduler's P tokens
// are a lock-free hand-back (PTokens), and COORD's gate for it is exact CONSERVATION: at quiescence, after many cycles,
// the free tokens are exactly P and no waiter is left counted, and no Wait is ever left asleep beside a free token.
[TestClass]
public class PTokensTests
{
    [DataTestMethod]
    [DataRow(2)]
    [DataRow(4)]
    public void TokensAreConservedUnderStress(int procs)
    {
        PTokens tokens = new(procs);
        const int threads = 16, cycles = 5_000;
        int inside = 0, maxInside = 0, done = 0;
        Thread[] workers = new Thread[threads];

        for (int t = 0; t < threads; t++)
        {
            workers[t] = new Thread(() =>
            {
                for (int i = 0; i < cycles; i++)
                {
                    tokens.Wait();
                    int now = Interlocked.Increment(ref inside);
                    int seen;
                    while (now > (seen = Volatile.Read(ref maxInside)) && Interlocked.CompareExchange(ref maxInside, now, seen) != seen) { }
                    if ((i & 7) == 0) Thread.Yield();
                    Interlocked.Decrement(ref inside);
                    tokens.Release();
                }
                Interlocked.Increment(ref done);
            }) { IsBackground = true };
        }

        foreach (Thread worker in workers) worker.Start();
        foreach (Thread worker in workers) Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(120)), "every Wait returned: no thread asleep beside a free token");

        Assert.AreEqual(threads, done);
        Assert.IsTrue(maxInside <= procs, $"never more than P holders at once: saw {maxInside} with P={procs}");
        Assert.AreEqual(procs, tokens.CurrentCount, "conservation: every token is free again at quiescence");
        Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");
    }

    // The lost-wakeup arm, made deterministic through the test seam: a release runs in the window between a waiter's
    // first failed take and its counting itself, finds no waiter, and frees its token. Only the waiter's post-queue retry
    // wins that token; without it the waiter sleeps beside a free token forever.
    [TestMethod]
    public void AReleaseInTheWaitersWindowIsNotLost()
    {
        PTokens tokens = new(1);
        tokens.Wait();                                   // this thread holds the only token

        using ManualResetEventSlim inWindow = new(false), released = new(false);
        Thread? waiterThread = null;
        bool waited = false;

        PTokens.BeforeCountForTest = () =>
        {
            if (Thread.CurrentThread == waiterThread)
            {
                inWindow.Set();
                released.Wait();
            }
        };

        try
        {
            waiterThread = new Thread(() => { tokens.Wait(); waited = true; tokens.Release(); }) { IsBackground = true };
            waiterThread.Start();

            Assert.IsTrue(inWindow.Wait(TimeSpan.FromSeconds(30)), "the waiter reached the window");
            tokens.Release();                            // finds no waiter queued: the token goes free
            released.Set();

            Assert.IsTrue(waiterThread.Join(TimeSpan.FromSeconds(10)), "the waiter won the freed token: no lost wakeup");
            Assert.IsTrue(waited);
            Assert.AreEqual(1, tokens.CurrentCount, "conservation: the one token is free again");
            Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");
        }
        finally
        {
            PTokens.BeforeCountForTest = null;
        }
    }
}
