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

    // The spin tier's own hazard (design (A)): releases that see a spinner free their tokens for the spinners, and the
    // spinners take FEWER than were left. Only the last spinner's hand-off on leaving (LeaveSpin) gives the rest to a
    // queued sleeper; without it the sleeper sleeps beside a free token until some later release. Deterministic through
    // the InSpin seam: two releases while one spinner is counted; the spinner takes one and HOLDS it until the sleeper is
    // served, so only the hand-off can serve it. (The first version of this row had the spinner give up instead; its own
    // post-queue retry then won the token, and the row read green with the hand-off removed: vacuous.)
    [TestMethod]
    public void TokensLeftForASpinnerThatTakesFewerReachTheSleeper()
    {
        PTokens tokens = new(2);
        tokens.Wait();
        tokens.Wait();                                   // this thread holds both tokens

        using ManualResetEventSlim inSpin = new(false), released = new(false), sleeperServed = new(false);
        Thread? spinnerThread = null;
        bool spun = false;

        PTokens.InSpinForTest = () =>
        {
            if (Thread.CurrentThread == spinnerThread && !inSpin.IsSet)
            {
                inSpin.Set();
                released.Wait();
            }
        };

        try
        {
            Thread sleeper = new(() => { tokens.Wait(); sleeperServed.Set(); tokens.Release(); }) { IsBackground = true };
            sleeper.Start();
            SpinWait.SpinUntil(() => tokens.WaitingForTest == 1, TimeSpan.FromSeconds(30));
            Assert.AreEqual(1, tokens.WaitingForTest, "the sleeper is queued");

            spinnerThread = new Thread(() =>
            {
                tokens.Wait();                           // takes one token in its spin
                spun = true;
                sleeperServed.Wait(TimeSpan.FromSeconds(10));   // HOLDS it until the sleeper is served (or gives up)
                tokens.Release();
            }) { IsBackground = true };
            spinnerThread.Start();
            Assert.IsTrue(inSpin.Wait(TimeSpan.FromSeconds(30)), "the spinner is counted in its spin");

            tokens.Release();                            // a spinner is counted: both tokens go to the free count
            tokens.Release();
            released.Set();                              // the spinner takes ONE and leaves the spin tier

            Assert.IsTrue(sleeperServed.Wait(TimeSpan.FromSeconds(10)), "the last spinner's hand-off served the sleeper");
            Assert.IsTrue(sleeper.Join(TimeSpan.FromSeconds(10)) && spinnerThread.Join(TimeSpan.FromSeconds(20)));
            Assert.IsTrue(spun);
            Assert.AreEqual(2, tokens.CurrentCount, "conservation: both tokens are free again");
            Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");
        }
        finally
        {
            PTokens.InSpinForTest = null;
        }
    }
}
