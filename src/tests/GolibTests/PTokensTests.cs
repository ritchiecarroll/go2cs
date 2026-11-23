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

    // Withdraw against grant (review item 7): a release grants the queued node in the window after the sleeper queued and
    // before its retry, and a second release frees a token the retry then wins. The node is granted, so the withdraw
    // fails and the sleeper holds TWO tokens; only the give-back keeps P. Deterministic through the BeforeRetry seam.
    [TestMethod]
    public void AGrantRacingAWithdrawGivesTheSecondTokenBack()
    {
        PTokens tokens = new(2);
        tokens.Wait();
        tokens.Wait();                                   // this thread holds both tokens

        using ManualResetEventSlim inWindow = new(false), released = new(false), holding = new(false), done = new(false);
        Thread? sleeperThread = null;

        PTokens.BeforeRetryForTest = () =>
        {
            if (Thread.CurrentThread == sleeperThread)
            {
                inWindow.Set();
                released.Wait();
            }
        };

        try
        {
            sleeperThread = new Thread(() => { tokens.Wait(); holding.Set(); done.Wait(); tokens.Release(); }) { IsBackground = true };
            sleeperThread.Start();

            Assert.IsTrue(inWindow.Wait(TimeSpan.FromSeconds(30)), "the sleeper queued and reached its retry");
            tokens.Release();                            // grants the queued node
            tokens.Release();                            // no live sleeper: the token goes free
            Assert.AreEqual(1, tokens.CurrentCount, "one token free for the retry to win");
            released.Set();

            Assert.IsTrue(holding.Wait(TimeSpan.FromSeconds(10)), "the sleeper returned");
            Assert.AreEqual(1, tokens.CurrentCount, "the sleeper holds ONE token: the second went back");
            done.Set();
            Assert.IsTrue(sleeperThread.Join(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(2, tokens.CurrentCount, "conservation: both tokens are free again");
            Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");
        }
        finally
        {
            PTokens.BeforeRetryForTest = null;
        }
    }

    // Release(n) (review item 1; SetProcs grows P this way): with sleepers queued and no spinner, n tokens wake exactly
    // min(n, sleepers) of them, never a bare addition to the free count that leaves them asleep.
    [TestMethod]
    public void ReleaseOfNWakesMinOfNAndTheSleepers()
    {
        PTokens tokens = new(4);
        for (int i = 0; i < 4; i++) tokens.Wait();       // this thread holds all four

        const int sleepers = 3;
        int woken = 0;
        using ManualResetEventSlim done = new(false);
        Thread[] threads = new Thread[sleepers];

        for (int i = 0; i < sleepers; i++)
        {
            threads[i] = new Thread(() => { tokens.Wait(); Interlocked.Increment(ref woken); done.Wait(); tokens.Release(); }) { IsBackground = true };
            threads[i].Start();
        }

        Assert.IsTrue(SpinWait.SpinUntil(() => tokens.WaitingForTest == sleepers, TimeSpan.FromSeconds(30)), "the sleepers are counted");
        Thread.Sleep(200);                               // past their retries, asleep

        tokens.Release(2);
        Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref woken) == 2, TimeSpan.FromSeconds(10)), $"Release(2) woke two sleepers: woke {woken}");
        Thread.Sleep(200);
        Assert.AreEqual(2, Volatile.Read(ref woken), "and no more than two");
        Assert.AreEqual(1, tokens.WaitingForTest, "one sleeper is still counted");
        Assert.AreEqual(0, tokens.CurrentCount, "both tokens went to sleepers");

        tokens.Release(2);                               // one sleeper left: it wakes, the other token goes free
        Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref woken) == 3, TimeSpan.FromSeconds(10)), "Release(2) woke the last sleeper");
        Assert.AreEqual(1, tokens.CurrentCount, "the token no sleeper needed is free");

        done.Set();
        foreach (Thread thread in threads) Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.AreEqual(4, tokens.CurrentCount, "conservation: all four tokens are free again");
        Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");
    }

    // Wait(0) (review item 1; SetProcs takes Ps back this way) only tries the free count: no spin, no queue.
    [TestMethod]
    public void AZeroTimeoutWaitNeverSpinsOrQueues()
    {
        PTokens tokens = new(1);
        int spun = 0, counted = 0;
        PTokens.InSpinForTest = () => Interlocked.Increment(ref spun);
        PTokens.BeforeCountForTest = () => Interlocked.Increment(ref counted);

        try
        {
            Assert.IsTrue(tokens.Wait(0), "a free token is taken");
            Assert.IsFalse(tokens.Wait(0), "none free: false at once");
            Assert.AreEqual(0, spun, "Wait(0) never entered the spin tier");
            Assert.AreEqual(0, counted, "Wait(0) never counted or queued a waiter");
            Assert.AreEqual(0, tokens.WaitingForTest);
            tokens.Release();
            Assert.AreEqual(1, tokens.CurrentCount, "conservation: the token is free again");
        }
        finally
        {
            PTokens.InSpinForTest = null;
            PTokens.BeforeCountForTest = null;
        }
    }

    // A timed Wait that times out withdraws its node: it leaves no waiter counted, and a later release is not handed to
    // the dead node.
    [TestMethod]
    public void ATimedOutWaitLeavesNothingBehind()
    {
        PTokens tokens = new(1);
        tokens.Wait();                                   // this thread holds the only token

        bool result = true;
        Thread waiter = new(() => result = tokens.Wait(100)) { IsBackground = true };
        waiter.Start();
        Assert.IsTrue(waiter.Join(TimeSpan.FromSeconds(10)));
        Assert.IsFalse(result, "timed out: no token");
        Assert.AreEqual(0, tokens.WaitingForTest, "the timed-out node was withdrawn and uncounted");

        tokens.Release();
        Assert.AreEqual(1, tokens.CurrentCount, "conservation: the release reached the free count, not the dead node");
    }

    // A grant racing a timeout (review item 1): the timed wait's event times out, and a release grants the node before
    // the withdraw. The withdraw fails, so the node was granted and the Wait returns TRUE holding that token; reporting
    // false would lose it. Deterministic through the BeforeWithdraw seam.
    [TestMethod]
    public void AGrantRacingATimeoutIsReportedAsATakenToken()
    {
        PTokens tokens = new(1);
        tokens.Wait();                                   // this thread holds the only token

        using ManualResetEventSlim inWindow = new(false), released = new(false);
        Thread? waiterThread = null;
        bool result = false;

        PTokens.BeforeWithdrawForTest = () =>
        {
            if (Thread.CurrentThread == waiterThread)
            {
                inWindow.Set();
                released.Wait();
            }
        };

        try
        {
            waiterThread = new Thread(() => result = tokens.Wait(50)) { IsBackground = true };
            waiterThread.Start();

            Assert.IsTrue(inWindow.Wait(TimeSpan.FromSeconds(30)), "the wait timed out and reached its withdraw");
            tokens.Release();                            // the node is still Waiting: it is granted the token
            Assert.AreEqual(0, tokens.CurrentCount, "the token went to the timed-out node");
            released.Set();

            Assert.IsTrue(waiterThread.Join(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(result, "the failed withdraw reports the granted token");
            Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");

            tokens.Release();                            // the waiter's token, given back on its behalf
            Assert.AreEqual(1, tokens.CurrentCount, "conservation: the one token is free again");
        }
        finally
        {
            PTokens.BeforeWithdrawForTest = null;
        }
    }

    // Timed waits under stress: a grant can win the race with a timeout's withdraw, and then the Wait must report the
    // token it holds (a failed withdraw returns true), or the token is lost. Conservation at quiescence is the gate.
    [TestMethod]
    public void TimedWaitsConserveTokensUnderStress()
    {
        const int procs = 2, threads = 16, cycles = 3_000;
        PTokens tokens = new(procs);
        int inside = 0, maxInside = 0;
        Thread[] workers = new Thread[threads];

        for (int t = 0; t < threads; t++)
        {
            int seed = t;
            workers[t] = new Thread(() =>
            {
                for (int i = 0; i < cycles; i++)
                {
                    while (!tokens.Wait((i + seed) % 3)) { }   // 0, 1 or 2 ms, retried until a token is held
                    int now = Interlocked.Increment(ref inside);
                    int seen;
                    while (now > (seen = Volatile.Read(ref maxInside)) && Interlocked.CompareExchange(ref maxInside, now, seen) != seen) { }
                    if ((i & 3) == 0) Thread.Yield();
                    Interlocked.Decrement(ref inside);
                    tokens.Release();
                }
            }) { IsBackground = true };
        }

        foreach (Thread worker in workers) worker.Start();
        foreach (Thread worker in workers) Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(120)), "every Wait returned");

        Assert.IsTrue(maxInside <= procs, $"never more than P holders at once: saw {maxInside} with P={procs}");
        Assert.AreEqual(procs, tokens.CurrentCount, "conservation: every token is free again at quiescence");
        Assert.AreEqual(0, tokens.WaitingForTest, "conservation: no waiter is left counted");
    }

    // An interrupt while asleep (review item 6) leaves nothing behind: the node is withdrawn and uncounted, the
    // exception reaches the caller, and a later release goes to the free count, not to the dead node.
    [TestMethod]
    public void AnInterruptedWaitLeavesNothingBehind()
    {
        PTokens tokens = new(1);
        tokens.Wait();                                   // this thread holds the only token

        bool interrupted = false;
        Thread sleeper = new(() =>
        {
            try { tokens.Wait(); tokens.Release(); }
            catch (ThreadInterruptedException) { interrupted = true; }
        }) { IsBackground = true };
        sleeper.Start();

        Assert.IsTrue(SpinWait.SpinUntil(() => tokens.WaitingForTest == 1 && (sleeper.ThreadState & ThreadState.WaitSleepJoin) != 0, TimeSpan.FromSeconds(30)), "the sleeper is asleep");
        sleeper.Interrupt();
        Assert.IsTrue(sleeper.Join(TimeSpan.FromSeconds(10)));
        Assert.IsTrue(interrupted, "the interrupt reached the caller");
        Assert.AreEqual(0, tokens.WaitingForTest, "the interrupted node was withdrawn and uncounted");

        tokens.Release();
        Assert.AreEqual(1, tokens.CurrentCount, "conservation: the release reached the free count, not the dead node");
    }
}
