using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using go.testing_runtime;
using static go.builtin;
using synctest = go.@internal.synctest_package;
using time = go.time_package;

namespace GolibTests;

/// <summary>
/// internal/synctest's five linkname pulls over the golib bubble (S4 of
/// DESIGN-gopark-goready-synctest.md), driven through the converted package's own surface --
/// <c>Run</c>, <c>Wait</c>, <c>Acquire</c>, <c>Bubble.Release</c>, <c>Bubble.Run</c> -- plus the
/// asynctimerchan refusal through time's live GODEBUG setting, and the test host's waiver of its
/// owner rule for <c>SkipNow</c>/<c>FailNow</c> inside a bubble the test started.
/// </summary>
[TestClass]
public class SyncTestPullsTests
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

    [TestMethod]
    public void RunAndWaitReachTheBubble()
    {
        bool waited = false;
        int got = 0;

        AssertNoFailure(OnGoroutine(() =>
            synctest.Run(() =>
            {
                channel<int> ch = new(0);
                Goroutine.Start(() => got = ch.Receive());

                // The receiver is durably blocked on a bubbled channel, so Wait returns.
                synctest.Wait();
                waited = true;
                ch.Send(7);
            })));

        Assert.IsTrue(waited, "synctest.Wait returned once the receiver was durably blocked");
        Assert.AreEqual(7, got);
    }

    [TestMethod]
    public void OutsideABubbleAcquireIsNilAndANilBubbleRunsInPlace()
    {
        bool ran = false;
        SyncTestBubble? during = null;

        AssertNoFailure(OnGoroutine(() =>
        {
            ж<synctest.Bubble> b = synctest.Acquire();
            Assert.IsTrue(b == nil, "Acquire outside a bubble returns nil");

            synctest.Release(b);
            synctest.Run(b, () => { ran = true; during = SyncTestBubble.Current; });
        }));

        Assert.IsTrue(ran, "a nil Bubble's Run calls f directly");
        Assert.IsNull(during, "and f is not bubbled");
    }

    [TestMethod]
    public void AnAcquiredBubbleCannotIdleUntilReleased()
    {
        // The root's f returns leaving one member blocked forever: a deadlock. While the bubble is
        // acquired it must not idle, so the deadlock is reported only after Release.
        ж<synctest.Bubble>? held = null;
        using ManualResetEventSlim acquired = new(false);
        Stopwatch clock = Stopwatch.StartNew();
        long releasedAt = -1, reportedAt = -1;
        Exception? releaseEscaped = null;

        Goroutine.Start(() =>
        {
            try
            {
                acquired.Wait();
                Thread.Sleep(300);
                releasedAt = clock.ElapsedTicks;
                synctest.Release(held!);
            }
            catch (Exception ex)
            {
                releaseEscaped = ex;
            }
        });

        Exception? escaped = OnGoroutine(() =>
        {
            try
            {
                synctest.Run(() =>
                {
                    held = synctest.Acquire();
                    channel<int> never = new(0);
                    Goroutine.Start(() => never.Receive());
                    acquired.Set();
                });
            }
            finally
            {
                reportedAt = clock.ElapsedTicks;
            }
        });

        StringAssert.Contains(escaped?.Message, "deadlock: all goroutines in bubble are blocked");
        Assert.IsNull(releaseEscaped, $"the release escaped: {releaseEscaped}");
        Assert.IsTrue(releasedAt >= 0, "the release ran");
        Assert.IsTrue(reportedAt > releasedAt, "the acquired bubble reported its deadlock only after Release");
    }

    [TestMethod]
    public void BubbleRunMakesAnOutsideGoroutineAMemberForItsDuration()
    {
        ж<synctest.Bubble>? held = null;
        SyncTestBubble? bubble = null, inside = null, after = null;
        channel<int> made = default;
        string? nested = null;
        using ManualResetEventSlim acquired = new(false);
        using ManualResetEventSlim joined = new(false);
        Exception? outsideEscaped = null;

        Goroutine.Start(() =>
        {
            try
            {
                acquired.Wait();

                synctest.Run(held!, () =>
                {
                    inside = SyncTestBubble.Current;
                    made = new channel<int>(1);

                    try
                    {
                        synctest.Run(held!, () => { });
                    }
                    catch (PanicException ex)
                    {
                        nested = ex.Message;
                    }
                });

                after = SyncTestBubble.Current;
                synctest.Release(held!);
            }
            catch (Exception ex)
            {
                outsideEscaped = ex;
            }
            finally
            {
                joined.Set();
            }
        });

        AssertNoFailure(OnGoroutine(() =>
            synctest.Run(() =>
            {
                bubble = SyncTestBubble.Current;
                held = synctest.Acquire();
                acquired.Set();
            })));

        Assert.IsTrue(joined.Wait(TimeoutMs), "the outside goroutine finished");
        Assert.IsNull(outsideEscaped, $"the outside goroutine escaped: {outsideEscaped}");
        Assert.IsNotNull(bubble);
        Assert.AreSame(bubble, inside, "Bubble.Run's f runs in the acquired bubble");
        Assert.IsNull(after, "and the goroutine leaves it when f returns");
        Assert.AreEqual("goroutine is already bubbled", nested);

        // A channel made inside Bubble.Run belongs to the bubble: sending on it from outside panics.
        Exception? send = OnGoroutine(() => made.Send(1));
        StringAssert.Contains(send?.Message, "send on synctest channel from outside bubble");
    }

    [TestMethod]
    public void RunRefusesUnderAsyncTimerChan()
    {
        _ = time.Now(); // time's module initializer installs the setting's reader

        string? previous = Environment.GetEnvironmentVariable("GODEBUG");
        bool ran = false;
        Exception? escaped;

        try
        {
            Environment.SetEnvironmentVariable("GODEBUG", "asynctimerchan=1");
            escaped = OnGoroutine(() => synctest.Run(() => ran = true));
        }
        finally
        {
            Environment.SetEnvironmentVariable("GODEBUG", previous);
        }

        StringAssert.Contains(escaped?.Message, "synctest.Run not supported with asynctimerchan!=0");
        Assert.IsFalse(ran, "f never ran");

        // And with the setting restored, Run runs.
        AssertNoFailure(OnGoroutine(() => synctest.Run(() => ran = true)));
        Assert.IsTrue(ran);
    }

    // ---- the test host: SkipNow / FailNow inside a bubble the test started ----

    private static TestExecution NewExecution(string name)
    {
        TestReporter reporter = new("guard", json: false, verbose: false);
        TestRunner runner = new(new TestRegistry("guard", []), new TestOptions(), reporter, ".", ".");

        return new TestExecution(runner, name, null, "guard.go", 1);
    }

    [TestMethod]
    public void SkipNowInTheTestsBubbleEndsThatGoroutineAndSkipsTheTest()
    {
        TestExecution parent = NewExecution("TestSkipInBubble");
        TestExecution? child = null;
        bool afterSkip = false, runReturned = false;

        parent.Run("child", t =>
        {
            child = t.Value.Execution;
            synctest.Run(() =>
            {
                child.SkipNow();
                afterSkip = true;
            });
            runReturned = true;
        });

        Assert.IsNotNull(child);
        Assert.IsFalse(child.InfrastructureFailed, "a SkipNow inside the test's own bubble is not refused");
        Assert.IsTrue(child.Skipped, "the test is skipped");
        Assert.IsFalse(child.Failed);
        Assert.IsFalse(afterSkip, "SkipNow ended the bubble's goroutine (Go's runtime.Goexit)");
        Assert.IsTrue(runReturned, "synctest.Run returned on the test's goroutine, which went on");
    }

    [TestMethod]
    public void FailNowInTheTestsBubbleEndsThatGoroutineAndFailsTheTest()
    {
        TestExecution parent = NewExecution("TestFailInBubble");
        TestExecution? child = null;
        bool afterFail = false, runReturned = false;

        parent.Run("child", t =>
        {
            child = t.Value.Execution;
            synctest.Run(() =>
            {
                child.FailNow();
                afterFail = true;
            });
            runReturned = true;
        });

        Assert.IsNotNull(child);
        Assert.IsFalse(child.InfrastructureFailed, "a FailNow inside the test's own bubble is not refused");
        Assert.IsTrue(child.Failed, "the test failed");
        Assert.IsFalse(afterFail, "FailNow ended the bubble's goroutine (Go's runtime.Goexit)");
        Assert.IsTrue(runReturned, "synctest.Run returned on the test's goroutine, which went on");
    }

    [TestMethod]
    public void SkipNowFromAPlainGoroutineIsStillRefused()
    {
        TestExecution parent = NewExecution("TestSkipOffThread");
        TestExecution? child = null;
        bool returned = false, finished = false;

        parent.Run("child", t =>
        {
            child = t.Value.Execution;
            using ManualResetEventSlim done = new(false);

            Goroutine.Start(() =>
            {
                try
                {
                    child.SkipNow();
                    returned = true;
                }
                finally
                {
                    done.Set();
                }
            });

            finished = done.Wait(TimeoutMs);
        });

        // Asserted out here: an assertion failing inside the test body would itself read as the
        // infrastructure failure this arm looks for.
        Assert.IsNotNull(child);
        Assert.IsTrue(finished, "the plain goroutine finished");
        Assert.IsTrue(returned, "a refused SkipNow is a no-op, so its goroutine went on");
        Assert.IsTrue(child.InfrastructureFailed, "a SkipNow from a goroutine outside any bubble is refused as before");
        Assert.IsFalse(child.Skipped);
    }

    [TestMethod]
    public void SkipNowFromABubbleTheTestDidNotStartIsStillRefused()
    {
        TestExecution parent = NewExecution("TestSkipForeignBubble");
        TestExecution? child = null;
        bool afterSkip = false, finished = false;
        Exception? escaped = null;

        parent.Run("child", t =>
        {
            child = t.Value.Execution;
            using ManualResetEventSlim done = new(false);

            // The bubble's root is a goroutine the test started, not the test's own goroutine.
            Goroutine.Start(() =>
            {
                try
                {
                    synctest.Run(() =>
                    {
                        child.SkipNow();
                        afterSkip = true;
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

            finished = done.Wait(TimeoutMs);
        });

        Assert.IsNotNull(child);
        Assert.IsTrue(finished, "the foreign bubble finished");
        Assert.IsNull(escaped, $"escaped: {escaped}");
        Assert.IsTrue(child.InfrastructureFailed, "the waiver covers only a bubble rooted at the test's own goroutine");
        Assert.IsFalse(child.Skipped);
        Assert.IsTrue(afterSkip, "a refused SkipNow is a no-op, so its goroutine went on");
    }
}
