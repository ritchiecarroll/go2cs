using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δsync = go.sync_package;

namespace GolibTests;

/// <summary>
/// The synctest bubble core (golib SyncTestBubble, S1c of DESIGN-gopark-goready-synctest.md): Go's
/// synctestGroup accounting on golib's park seam, with the READY half (S1a) accounting a wake on the
/// waker's side and a child counted by its creator at the go statement.
/// </summary>
/// <remarks>
/// Every arm runs Run inside a golib goroutine: golib registers the MAIN goroutine on the thread that
/// first loads it, so a test's own thread can be a goroutine, and Run must never be entered by chance.
/// </remarks>
[TestClass]
public class SyncTestBubbleTests
{
    private const int TimeoutMs = 30000;

    // Runs `body` on a fresh golib goroutine and returns what escaped it (null when nothing did).
    private static Exception? OnGoroutine(Action body, int timeoutMs = TimeoutMs)
    {
        Exception? escaped = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                escaped = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Assert.IsTrue(done.Wait(timeoutMs), "the goroutine never finished (a bubble that never idled or never woke)");
        return escaped;
    }

    // The whole exception, stack included: an escape from a bubble names WHERE, not only what.
    private static void AssertNoFailure(Exception? escaped) =>
        Assert.IsNull(escaped, $"escaped: {escaped}");

    [TestMethod]
    public void RunReturnsOnceEveryMemberHasExited()
    {
        int ran = 0;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                Goroutine.Start(() => Interlocked.Increment(ref ran));
                Goroutine.Start(() => Interlocked.Increment(ref ran));
            });

            Assert.IsNull(SyncTestBubble.Current, "the root is no longer a member once Run returns");
        }));

        Assert.AreEqual(2, ran, "Run returned before its members finished");
    }

    [TestMethod]
    public void MembershipIsInheritedFromTheCreatorOnly()
    {
        SyncTestBubble? inF = null, inChild = null, outside = null;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                inF = SyncTestBubble.Current;
                using ManualResetEventSlim childDone = new(false);
                Goroutine.Start(() => { inChild = SyncTestBubble.Current; childDone.Set(); });
                childDone.Wait();
            });

            using ManualResetEventSlim outDone = new(false);
            Goroutine.Start(() => { outside = SyncTestBubble.Current; outDone.Set(); });
            outDone.Wait();
        }));

        Assert.IsNotNull(inF, "Run's function runs in the bubble");
        Assert.AreSame(inF, inChild, "a goroutine started by a member joins the same bubble");
        Assert.IsNull(outside, "a goroutine started outside a bubble is in none");
    }

    [TestMethod]
    public void RunDoesNotReturnBeforeAChildSpawnedAsFsLastActHasRun()
    {
        // The BEHAVIOUR creator-side counting buys: f's last act is a go statement, and f then exits.
        // Counted by its creator, the child keeps the bubble busy from that statement on; counted on its
        // own thread, f's exit can land first, the bubble reads total == 1, and Run returns while the
        // child has not yet run. This arm, not the count arm below, is the one the child-side red variant
        // breaks (measured: .NET's Thread.Start returns only once the new thread is running, so the
        // count arm's two reads rarely straddle the child's own count).
        int early = 0;

        for (int round = 0; round < 50; round++)
        {
            int ran = 0;

            AssertNoFailure(OnGoroutine(() =>
                SyncTestBubble.Run(() => Goroutine.Start(() => { Thread.Sleep(2); Volatile.Write(ref ran, 1); }))));

            if (Volatile.Read(ref ran) == 0)
                early++;
        }

        Assert.AreEqual(0, early, $"Run returned before a child spawned by f had run, in {early} of 50 rounds");
    }

    [TestMethod]
    public void AChildIsCountedAtTheGoStatement()
    {
        // Go's newproc counts the child before it runs: the count, read right after the go statement.
        // (A weak discriminator on its own -- see the arm above -- kept as the direct statement of it.)
        int lagged = 0;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                SyncTestBubble bubble = SyncTestBubble.Current!;

                // Every child stays alive until the loop ends, so no child EXITS between the two reads
                // of one iteration: the first cut released each child at once, and a previous child's
                // exit (total--) landing between the reads read as a lag in 199 of 200 rounds -- the
                // test's race, not the count. Not disposed: a child may not have reached its Wait, and
                // an ObjectDisposedException on a goroutine ends the process.
                ManualResetEventSlim release = new(false);

                for (int i = 0; i < 200; i++)
                {
                    int before = bubble.Total;
                    Goroutine.Start(() => release.Wait());

                    if (bubble.Total != before + 1)
                        lagged++;
                }

                release.Set();
            });
        }));

        Assert.AreEqual(0, lagged, "a child was not yet counted when its go statement returned");
    }

    [TestMethod]
    public void WaitReturnsOverDurablyBlockedMembers()
    {
        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                ж<Δsync.Mutex> mu = @new<Δsync.Mutex>();
                ж<Δsync.Cond> cond = Δsync.NewCond(new Δsync.MutexжLocker(mu));
                ж<Δsync.WaitGroup> wg = @new<Δsync.WaitGroup>();
                bool go = false;
                wg.Add(1);

                Goroutine.Start(() => { mu.Lock(); while (!go) cond.Wait(); mu.Unlock(); });
                Goroutine.Start(() => wg.Wait());

                SyncTestBubble.Wait();   // both members durably blocked (Cond, WaitGroup)

                mu.Lock();
                go = true;
                cond.Broadcast();
                mu.Unlock();
                wg.Done();
            });
        }));
    }

    [TestMethod]
    public void WaitDoesNotReturnOverAMutexWait()
    {
        // sync.Mutex.Lock is not durable in Go (isIdleInSynctest): a member contending for a mutex held
        // outside the bubble keeps it busy, so Wait must not return until the mutex is released.
        ж<Δsync.Mutex> mu = @new<Δsync.Mutex>();
        mu.Lock();

        int waitReturned = 0;
        Exception? escaped = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    Goroutine.Start(() => { mu.Lock(); mu.Unlock(); });
                    SyncTestBubble.Wait();
                    Interlocked.Exchange(ref waitReturned, 1);
                });
            }
            catch (Exception ex)
            {
                escaped = ex;
            }
            finally
            {
                done.Set();
            }
        });

        Thread.Sleep(500);
        Assert.AreEqual(0, Volatile.Read(ref waitReturned), "Wait returned while a member was blocked on a mutex");

        mu.Unlock();
        Assert.IsTrue(done.Wait(TimeoutMs), "the bubble never finished after the mutex was released");
        AssertNoFailure(escaped);
        Assert.AreEqual(1, waitReturned);
    }

    [TestMethod]
    public void AWaitAfterASignalNeverReturnsBeforeTheSignalledMemberRuns()
    {
        // THE WINDOW (DESIGN §2): the waker readies the Cond waiter before it signals, so the member is
        // RUNNING from the Signal on, and Wait cannot return until it has done its work and idled again.
        // A wake accounted by the wakee's resumption would let Wait slip into the gap between the
        // Signal and the member's thread running -- repeated so a timing-dependent gap is found.
        const int rounds = 500;
        int observedBehind = 0;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                ж<Δsync.Mutex> mu = @new<Δsync.Mutex>();
                ж<Δsync.Cond> cond = Δsync.NewCond(new Δsync.MutexжLocker(mu));
                bool pending = false, stop = false;
                int handled = 0;

                Goroutine.Start(() =>
                {
                    mu.Lock();

                    while (true)
                    {
                        while (!pending && !stop)
                            cond.Wait();

                        if (stop)
                            break;

                        pending = false;
                        handled++;
                    }

                    mu.Unlock();
                });

                SyncTestBubble.Wait();

                for (int i = 1; i <= rounds; i++)
                {
                    mu.Lock();
                    pending = true;
                    cond.Signal();
                    mu.Unlock();

                    SyncTestBubble.Wait();

                    mu.Lock();

                    if (handled != i)
                        observedBehind++;

                    mu.Unlock();
                }

                mu.Lock();
                stop = true;
                cond.Signal();
                mu.Unlock();
            });
        }, TimeoutMs * 4));

        Assert.AreEqual(0, observedBehind, $"Wait returned before the signalled member ran, in {observedBehind} of {rounds} rounds");
    }

    [TestMethod]
    public void ASelectForeverInAMemberIsTheBubblesDeadlock()
    {
        Exception? escaped = OnGoroutine(() =>
            SyncTestBubble.Run(() => Goroutine.Start(() => select())));

        Assert.IsInstanceOfType(escaped, typeof(PanicException));
        StringAssert.Contains(escaped!.Message, "deadlock: all goroutines in bubble are blocked");
    }

    [TestMethod]
    public void AParkedCoroutineIsDurable()
    {
        bool ran = false;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                Coro coro = Coro.Start(() => ran = true);

                SyncTestBubble.Wait();   // the coro goroutine is parked "coroutine": idle
                coro.Switch();           // run its body to the end
            });
        }));

        Assert.IsTrue(ran, "the coroutine's body never ran");
    }

    [TestMethod]
    public void TheRefusalsSayGosWords()
    {
        Exception? outside = OnGoroutine(SyncTestBubble.Wait);
        StringAssert.Contains(outside?.Message, "goroutine is not in a bubble");

        // Caught INSIDE Run's function, as Go's own test recovers it there: that function runs on the
        // bubble's first member goroutine, and a panic escaping a goroutine ends the process (this arm's
        // first cut let it escape and crashed the test host).
        Exception? nested = null;
        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() =>
        {
            try
            {
                SyncTestBubble.Run(() => { });
            }
            catch (PanicException ex)
            {
                nested = ex;
            }
        })));
        StringAssert.Contains(nested?.Message, "synctest.Run called from within a synctest bubble");

        // A second Wait while one is in progress. Member B blocks in a plain BCL wait -- invisible to
        // golib's park seam, so B stays RUNNING -- which holds member A's Wait open; the root's own Wait
        // then must be refused by Go's text, deterministically.
        Exception? refused = null;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                ManualResetEventSlim releaseB = new(false);

                Goroutine.Start(() => releaseB.Wait());
                Goroutine.Start(SyncTestBubble.Wait);
                Thread.Sleep(300);   // A is inside Wait, and cannot leave it while B runs

                try
                {
                    SyncTestBubble.Wait();
                }
                catch (PanicException ex)
                {
                    refused = ex;
                }

                releaseB.Set();
            });
        }));

        StringAssert.Contains(refused?.Message, "wait already in progress");
    }
}
