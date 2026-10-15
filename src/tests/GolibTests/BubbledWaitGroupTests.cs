using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using sync = go.sync_package;

namespace GolibTests;

/// <summary>
/// Go 1.26's WaitGroup inside a synctest bubble (sync/waitgroup.go, hand-owned as sync/waitgroup.cs): an
/// Add from a bubble associates the WaitGroup with it, so a Wait from that bubble parks durably with
/// Go's <c>sync.WaitGroup.Wait (durable)</c> and counts idle, while a WaitGroup added outside the bubble
/// does not; the two fatal texts of a WaitGroup used across bubbles; the zero-count Wait that
/// disassociates; and Go 1.25's <c>WaitGroup.Go</c>.
/// </summary>
[TestClass]
public class BubbledWaitGroupTests
{
    private const int TimeoutMs = 30000;

    private const string MultipleBubbles = "sync: WaitGroup.Add called from multiple synctest bubbles";
    private const string InsideAndOutside = "sync: WaitGroup.Add called from inside and outside synctest bubble";

    private static ж<sync.WaitGroup> NewWaitGroup()
    {
        heap(new sync.WaitGroup(), out ж<sync.WaitGroup> box);
        return box;
    }

    // Runs body on a goroutine and waits for it; what escaped comes out, the asserting stays here.
    private static Exception? OnGoroutine(Action body)
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

        Assert.IsTrue(done.Wait(TimeoutMs), "the goroutine never finished");
        return escaped;
    }

    private static void AssertNoFailure(Exception? escaped) =>
        Assert.IsNull(escaped, $"escaped: {escaped}");

    [TestMethod]
    public void AWaitOnAWaitGroupAddedInTheBubbleIsDurable()
    {
        WaitReason parkedWith = default;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                ж<sync.WaitGroup> wg = NewWaitGroup();
                Goroutine? waiter = null;
                wg.Add(1);

                Goroutine.Start(() =>
                {
                    Volatile.Write(ref waiter, Goroutine.Current);
                    wg.Wait();
                });

                // Returns only once every member is durably blocked: the waiter's Wait must count.
                SyncTestBubble.Wait();
                parkedWith = Volatile.Read(ref waiter)!.Reason;

                wg.Done();
            });
        }));

        Assert.AreEqual(WaitReason.SynctestWaitGroupWait, parkedWith);
        Assert.AreEqual("sync.WaitGroup.Wait (durable)", WaitReasons.Text(parkedWith));
    }

    [TestMethod]
    public void AWaitOnAWaitGroupAddedOutsideIsNotDurable()
    {
        ж<sync.WaitGroup> wg = NewWaitGroup();
        wg.Add(1);   // from no bubble: never associated

        Goroutine? waiter = null;
        int waitReturned = 0;
        Exception? escaped = null;
        using ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            try
            {
                SyncTestBubble.Run(() =>
                {
                    Goroutine.Start(() =>
                    {
                        Volatile.Write(ref waiter, Goroutine.Current);
                        wg.Wait();
                    });

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

        Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref waiter)?.State == GoroutineState.Parked, TimeoutMs),
            "the waiter never parked");

        Thread.Sleep(500);
        Assert.AreEqual(0, Volatile.Read(ref waitReturned), "synctest.Wait returned while a member waited on a WaitGroup added outside the bubble");
        Assert.AreEqual(WaitReason.SyncWaitGroupWait, Volatile.Read(ref waiter)!.Reason);

        wg.Done();   // from no bubble, on a WaitGroup no bubble added to: no fatal
        Assert.IsTrue(done.Wait(TimeoutMs), "the bubble never finished");
        AssertNoFailure(escaped);
    }

    // Go's two fatals end the process (FatalReport.Fatal exits), so they are read at the decision.
    private delegate string? AddBubble(SyncTestBubble? caller, ref SyncTestBubble? bubble, ref bool flag, bool busy, out bool bubbled);

    // Resolved where it is used, so a missing decision fails its own test and not the class.
    private static AddBubble AddBubbleDecision() => typeof(sync)
        .GetMethod("wgAddBubble", BindingFlags.Static | BindingFlags.NonPublic)!
        .CreateDelegate<AddBubble>();

    private static SyncTestBubble BubbleOf()
    {
        SyncTestBubble? captured = null;
        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() => captured = SyncTestBubble.Current)));
        return captured!;
    }

    [TestMethod]
    public void TheAddDecisionSaysGosFatalWords()
    {
        AddBubble addBubble = AddBubbleDecision();
        SyncTestBubble a = BubbleOf();
        SyncTestBubble b = BubbleOf();
        Assert.AreNotSame(a, b);

        // Outside any bubble, a WaitGroup no bubble touched: nothing to say, nothing recorded.
        SyncTestBubble? bubble = null;
        bool flag = false;
        Assert.IsNull(addBubble(null, ref bubble, ref flag, busy: true, out bool bubbled));
        Assert.IsFalse(bubbled);
        Assert.IsNull(bubble);
        Assert.IsFalse(flag);

        // The first Add from a bubble associates the WaitGroup and sets the flag.
        Assert.IsNull(addBubble(a, ref bubble, ref flag, busy: false, out bubbled));
        Assert.IsTrue(bubbled);
        Assert.AreSame(a, bubble);
        Assert.IsTrue(flag);

        // Again from the same bubble, now busy: still fine, the flag was already set.
        Assert.IsNull(addBubble(a, ref bubble, ref flag, busy: true, out bubbled));

        // From another bubble.
        Assert.AreEqual(MultipleBubbles, addBubble(b, ref bubble, ref flag, busy: true, out _));

        // From outside, on a WaitGroup a bubble added to.
        Assert.AreEqual(InsideAndOutside, addBubble(null, ref bubble, ref flag, busy: true, out _));

        // From a bubble, on a WaitGroup already counting from outside (unflagged and busy).
        SyncTestBubble? fresh = null;
        bool unflagged = false;
        Assert.AreEqual(InsideAndOutside, addBubble(a, ref fresh, ref unflagged, busy: true, out _));
    }

    private static object? StateOf(ж<sync.WaitGroup> wg) =>
        typeof(sync.WaitGroup).GetField("st", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(wg.Value);

    private static SyncTestBubble? BubbleRecordOf(ж<sync.WaitGroup> wg)
    {
        object state = StateOf(wg)!;
        return (SyncTestBubble?)state.GetType().GetField("Bubble", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(state);
    }

    [TestMethod]
    public void AZeroCountWaitInTheBubbleDisassociates()
    {
        ж<sync.WaitGroup> kept = NewWaitGroup();
        ж<sync.WaitGroup> released = NewWaitGroup();
        SyncTestBubble? keptBy = null, releasedBy = null;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                kept.Add(1);
                kept.Done();
                keptBy = BubbleRecordOf(kept);

                released.Add(1);
                released.Done();
                released.Wait();   // zero count, no waiter: Go 1.26 disassociates here
                releasedBy = BubbleRecordOf(released);
            });
        }));

        // The control: without the Wait the association outlives the count, as Go's special does.
        Assert.IsNotNull(keptBy, "an Add from a bubble recorded no association");
        Assert.IsNull(releasedBy, "a zero-count Wait left the WaitGroup associated");

        // And it can be reused from another bubble without Go's fatal.
        AssertNoFailure(OnGoroutine(() => SyncTestBubble.Run(() => { released.Add(1); released.Done(); })));
    }

    // Exactly a converted package-level variable's shape: `internal static ж<T> ᏑX = new StandardBox<T>(...)`.
    private static readonly ж<sync.WaitGroup> ᏑpackageLevelWaitGroup = new StandardBox<sync.WaitGroup>(default(sync.WaitGroup));

    // EXPECTED DIFFERENCE (G-W1 ruling (a)). Go answers Unbubbled for a package-level WaitGroup, because
    // it is outside a heap span (runtime/synctest.go:383-386): Go records no association, its Wait is
    // NOT durable, and synctest.Wait would not return while a member waits on it. A converted global is
    // boxed as a heap value is, so here it IS associated: the Wait is durable and synctest.Wait returns.
    // The other observable effect, Go's two fatals firing for a global, ends the process and is not
    // run here. Retired by the post-hop package-level box marker (BOARD row, G's).
    [TestMethod]
    public void GoLeavesAPackageLevelWaitGroupUnbubbled_HereItIsAssociated()
    {
        ж<sync.WaitGroup> wg = ᏑpackageLevelWaitGroup;
        WaitReason parkedWith = default;
        SyncTestBubble? recorded = null;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                Goroutine? waiter = null;
                wg.Add(1);
                recorded = BubbleRecordOf(wg);

                Goroutine.Start(() =>
                {
                    Volatile.Write(ref waiter, Goroutine.Current);
                    wg.Wait();
                });

                // Go: this would not return (the waiter's Wait is not durable for a global).
                SyncTestBubble.Wait();
                parkedWith = Volatile.Read(ref waiter)!.Reason;

                wg.Done();   // releases the waiter and disassociates, leaving the global reusable
            });
        }));

        Assert.IsNotNull(recorded, "the difference this test states is gone: a global was left unbubbled, as Go does");
        Assert.AreEqual(WaitReason.SynctestWaitGroupWait, parkedWith);
        Assert.IsNull(BubbleRecordOf(wg), "the Add that released a waiter left the global associated");
    }

    [TestMethod]
    public void GoRunsFAndRemovesTheTask()
    {
        ж<sync.WaitGroup> wg = NewWaitGroup();
        int ran = 0;

        AssertNoFailure(OnGoroutine(() =>
        {
            for (int i = 0; i < 4; i++)
                wg.Go(() => Interlocked.Increment(ref ran));

            wg.Wait();
        }));

        Assert.AreEqual(4, Volatile.Read(ref ran));
    }

    [TestMethod]
    public void GoCallsDoneWhenFEndsInGoexit()
    {
        ж<sync.WaitGroup> wg = NewWaitGroup();

        // Go's deferred Done runs on Goexit; a lost Done would leave this Wait parked forever.
        AssertNoFailure(OnGoroutine(() =>
        {
            wg.Go(() => throw new GoexitException());
            wg.Wait();
        }));
    }

    [TestMethod]
    public void GoInsideTheBubbleWaitsDurably()
    {
        int ran = 0;

        AssertNoFailure(OnGoroutine(() =>
        {
            SyncTestBubble.Run(() =>
            {
                ж<sync.WaitGroup> wg = NewWaitGroup();
                channel<int> gate = new(0);

                wg.Go(() => { gate.Receive(); Interlocked.Increment(ref ran); });

                // Both members durably blocked: the task on a bubbled channel, nothing else running.
                SyncTestBubble.Wait();
                gate.Send(1);
                wg.Wait();
            });
        }));

        Assert.AreEqual(1, Volatile.Read(ref ran));
    }
}
