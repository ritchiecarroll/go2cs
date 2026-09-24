using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Bubbled channels (S2 of DESIGN-gopark-goready-synctest.md): a channel made inside a synctest bubble
/// is tagged (Go's hchan.synctest); a send / receive / select on it from a goroutine in no bubble
/// panics with Go's text; a direct handoff across bubbles panics; and a member blocked on bubbled
/// channels -- including a select whose other cases are nil -- counts idle, while one blocked on a
/// channel made outside does not.
/// </summary>
[TestClass]
public class BubbledChannelTests
{
    private const int TimeoutMs = 30000;

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

        Assert.IsTrue(done.Wait(timeoutMs), "the goroutine never finished");
        return escaped;
    }

    private static void AssertNoFailure(Exception? escaped) =>
        Assert.IsNull(escaped, $"escaped: {escaped}");

    // A channel made by a bubble member, handed out after the bubble has finished.
    private static channel<int> MadeInABubble(int size)
    {
        channel<int> made = default;
        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() => made = new channel<int>(size))));
        return made;
    }

    [TestMethod]
    public void TheOutsidePanicsSayGosWords()
    {
        channel<int> ch = MadeInABubble(1);

        StringAssert.Contains(OnGoroutine(() => ch.Send(1))?.Message, "send on synctest channel from outside bubble");
        StringAssert.Contains(OnGoroutine(() => ch.Receive())?.Message, "receive on synctest channel from outside bubble");
        StringAssert.Contains(OnGoroutine(() => select(ch.Receiving))?.Message, "select on synctest channel from outside bubble");

        // Non-blocking forms check too (Go checks before the fast path).
        StringAssert.Contains(OnGoroutine(() => ch.Received(out _))?.Message, "receive on synctest channel from outside bubble");
    }

    [TestMethod]
    public void AChannelMadeOutsideABubbleIsUntagged()
    {
        channel<int> ch = new(1);
        AssertNoFailure(OnGoroutine(() => { ch.Send(1); ch.Receive(); }));
    }

    [TestMethod]
    public void CloseFromOutsideIsNotRefused()
    {
        // Go's closechan carries no bubble check.
        channel<int> ch = MadeInABubble(0);
        AssertNoFailure(OnGoroutine(() => close(ch)));
    }

    [TestMethod]
    public void AMemberBlockedOnABubbledChannelIsDurable()
    {
        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                channel<int> ch = new(0);
                Goroutine.Start(() => ch.Receive());
                Goroutine.Start(() => { channel<int> nilCh = default; select(ch.Receiving, nilCh.Receiving); });

                SyncTestBubble.Wait();   // one plain receive, one select with a nil case: both durable

                ch.Send(1);
                ch.Send(2);
            });
        }));
    }

    [TestMethod]
    public void AMemberBlockedOnAnOutsideChannelIsNotDurable()
    {
        channel<int> outside = new(0);
        int waitReturned = 0;
        Exception? escaped = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    Goroutine.Start(() => outside.Receive());
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
        Assert.AreEqual(0, Volatile.Read(ref waitReturned), "Wait returned while a member was blocked on a channel made outside the bubble");

        outside.Send(1);   // from a non-member: the channel is untagged, so no panic
        Assert.IsTrue(done.Wait(TimeoutMs), "the bubble never finished");
        AssertNoFailure(escaped);
    }

    [TestMethod]
    public void ASelectMixingAnOutsideChannelIsNotDurable()
    {
        channel<int> outside = new(0);
        int waitReturned = 0;
        Exception? escaped = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    channel<int> inside = new(0);
                    Goroutine.Start(() => select(inside.Receiving, outside.Receiving));
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
        Assert.AreEqual(0, Volatile.Read(ref waitReturned), "Wait returned while a member selected on a channel made outside the bubble");

        outside.Send(1);
        Assert.IsTrue(done.Wait(TimeoutMs), "the bubble never finished");
        AssertNoFailure(escaped);
    }

    [TestMethod]
    public void AHandoffAcrossBubblesPanics()
    {
        // Bubble A makes an unbuffered channel; a member of bubble B (which passes the no-bubble check)
        // parks receiving on it; A's send then finds B's goroutine and panics, as Go's send() does. B's
        // receiver is left stranded (dequeued, never woken -- Go's own outcome), so B reports its
        // deadlock.
        channel<int> shared = default;
        using ManualResetEventSlim published = new(false);
        int bParked = 0;
        Exception? sendPanic = null;

        Exception? a = null, b = null;
        using ManualResetEventSlim aDone = new(false);
        using ManualResetEventSlim bDone = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    shared = new channel<int>(0);
                    published.Set();

                    SpinWait.SpinUntil(() => Volatile.Read(ref bParked) == 1, TimeoutMs);
                    Thread.Sleep(200);   // B's member is now parked on the channel

                    try
                    {
                        shared.Send(1);
                    }
                    catch (PanicException ex)
                    {
                        sendPanic = ex;
                    }
                });
            }
            catch (Exception ex)
            {
                a = ex;
            }
            finally
            {
                aDone.Set();
            }
        });

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    published.Wait();
                    Volatile.Write(ref bParked, 1);
                    shared.Receive();
                });
            }
            catch (Exception ex)
            {
                b = ex;
            }
            finally
            {
                bDone.Set();
            }
        });

        Assert.IsTrue(aDone.Wait(TimeoutMs), "bubble A never finished");
        AssertNoFailure(a);
        StringAssert.Contains(sendPanic?.Message, "send on synctest channel from outside bubble");

        Assert.IsTrue(bDone.Wait(TimeoutMs), "bubble B never reported its stranded receiver");
        StringAssert.Contains(b?.Message, "deadlock: all goroutines in bubble are blocked");
    }
}
