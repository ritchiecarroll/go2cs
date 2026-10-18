using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Bubbled channels (S2 of DESIGN-gopark-goready-synctest.md): a channel made inside a synctest bubble
/// is tagged (Go's hchan.bubble); a send / receive / select / close on it from a goroutine not in that
/// bubble -- no bubble or another one -- is fatal with Go's text (Go 1.26; 1.24 panicked, and only for
/// no bubble); and a member blocked on bubbled channels -- including a select whose other cases are
/// nil -- counts idle, while one blocked on a channel made outside does not.
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

    // Go 1.26's outside-bubble faults end the process (FatalReport.Fatal exits), so they are read at
    // the decision every call site consults (ChanCore.OutsideBubbleFault); the -tests rows
    // TestChannelMovedOutOfBubble and TestTimerFromInsideBubble cover the fatal itself.
    internal static string? ChannelFault(channel<int> ch, string operation) =>
        ((ChanCore)typeof(channel<int>).GetField("m_core", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(ch)!).OutsideBubbleFault(operation);

    private static readonly string[] s_operations = ["send", "receive", "select", "close"];

    // Go's own words, spelled out rather than built from the operation (closechan says "of").
    private static readonly string[] s_goFaults =
    [
        "send on synctest channel from outside bubble",
        "receive on synctest channel from outside bubble",
        "select on synctest channel from outside bubble",
        "close of synctest channel from outside bubble",
    ];

    [TestMethod]
    public void TheOutsideFatalsSayGosWords()
    {
        channel<int> ch = MadeInABubble(1);
        string?[] faults = new string?[s_operations.Length];

        AssertNoFailure(OnGoroutine(() =>
        {
            for (int i = 0; i < s_operations.Length; i++)
                faults[i] = ChannelFault(ch, s_operations[i]);
        }));

        // close is new at Go 1.26 (closechan had no bubble check at 1.24).
        for (int i = 0; i < s_operations.Length; i++)
            Assert.AreEqual(s_goFaults[i], faults[i]);
    }

    [TestMethod]
    public void AnotherBubblesMemberIsOutsideTheChannelsBubble()
    {
        // Go 1.26 compares the caller's bubble with the CHANNEL's: a member of another bubble faults
        // like a goroutine in none (1.24 let it through, and caught only a handoff across bubbles).
        channel<int> ch = MadeInABubble(0);
        string?[] faults = new string?[s_operations.Length];

        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() =>
        {
            for (int i = 0; i < s_operations.Length; i++)
                faults[i] = ChannelFault(ch, s_operations[i]);
        })));

        for (int i = 0; i < s_operations.Length; i++)
            Assert.AreEqual(s_goFaults[i], faults[i]);
    }

    [TestMethod]
    public void InItsOwnBubbleNothingIsRefused()
    {
        string?[] faults = ["unset", "unset", "unset", "unset"];

        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() =>
        {
            channel<int> ch = new(1);
            Goroutine.Start(() => faults[0] = ChannelFault(ch, "send"));   // a member, not only main
            SyncTestBubble.Wait();

            for (int i = 1; i < s_operations.Length; i++)
                faults[i] = ChannelFault(ch, s_operations[i]);

            ch.Send(1);
            ch.Receive();
            close(ch);
        })));

        foreach (string? fault in faults)
            Assert.IsNull(fault);
    }

    [TestMethod]
    public void AChannelMadeOutsideABubbleIsUntagged()
    {
        channel<int> ch = new(1);
        AssertNoFailure(OnGoroutine(() => { ch.Send(1); ch.Receive(); }));

        string?[] faults = new string?[s_operations.Length];
        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() =>
        {
            for (int i = 0; i < s_operations.Length; i++)
                faults[i] = ChannelFault(ch, s_operations[i]);
        })));

        foreach (string? fault in faults)
            Assert.IsNull(fault, "an untagged channel is open to every bubble");
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
}
