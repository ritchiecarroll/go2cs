using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

[TestClass]
public class GoroutineExecutorTests
{
    // The executor's whole claim is that a PARKED goroutine costs no other goroutine's capacity.
    // The behavioral guard (GoroutineParkStorm) proves it end to end through converted Go; these are
    // the unit-level facts underneath it — the ones with no other witness, because runtime.NumGoroutine
    // does not yet read the registry and no emitted code can observe a goroutine's identity at all.
    //
    // No assertion here is an absolute count: the registry is process-global and other tests in this
    // assembly spawn goroutines of their own, so an exact count would be a flake waiting for a
    // scheduling accident. Nor is it a DELTA against a count taken before the test's own goroutines
    // start: an earlier test's goroutine can still be retiring then (its body signalled, its thread
    // has not yet unregistered), and it retires while the test counts, one short (see
    // AssertRegistryTracks). Assertions name the test's own goroutines, or count relative to a peak
    // they are all part of.

    // Comfortably more than any thread pool would have on hand, so a pool-backed executor could not
    // pass CapacityEqualsDemand by luck; small enough to stay well under a second.
    private const int ParkedGoroutines = 500;

    private const int TimeoutMs = 30000;

    [TestMethod]
    public void MainGoroutineIsRegistered()
    {
        // golib's module initializer registers the thread that first touched golib. Whatever else is
        // live, the main goroutine is always one of them — which is what makes a future
        // runtime.NumGoroutine agree with Go's on a program that started no goroutines at all.
        Assert.IsTrue(Goroutine.Count >= 1, $"expected the main goroutine in the registry, got {Goroutine.Count}");
    }

    [TestMethod]
    public void MainGoroutineIsNotOnAGoroutine()
    {
        // The one contract that distinguishes the main goroutine from every other: runtime.Goexit
        // means something different there, so the gate must read false. A thread with no identity at
        // all — this MSTest thread — is not a goroutine either.
        Assert.IsFalse(Goroutine.OnGoroutine);
    }

    [TestMethod]
    public void GoroutineRunsOnItsOwnDedicatedBackgroundThread()
    {
        using ManualResetEventSlim finished = new(false);

        bool onGoroutine = false;
        bool isPoolThread = true;
        bool isBackground = false;
        int goroutineThread = 0;

        Goroutine.Start(() =>
        {
            onGoroutine = Goroutine.OnGoroutine;
            isPoolThread = Thread.CurrentThread.IsThreadPoolThread;
            isBackground = Thread.CurrentThread.IsBackground;
            goroutineThread = Environment.CurrentManagedThreadId;
            finished.Set();
        });

        Assert.IsTrue(finished.Wait(TimeoutMs), "goroutine did not run");

        Assert.IsTrue(onGoroutine, "the body did not run marked as a goroutine");
        Assert.IsFalse(isPoolThread, "the body ran on a ThreadPool thread — the executor is the point");

        // IsBackground is Go's exit semantics: main returning must end the process no matter how many
        // goroutines are still live or parked.
        Assert.IsTrue(isBackground, "a goroutine thread must be a background thread");

        Assert.AreNotEqual(Environment.CurrentManagedThreadId, goroutineThread);
    }

    [TestMethod]
    public void ExecutionContextFlowsIntoTheGoroutine()
    {
        // The converted-test host attributes a failure inside a goroutine to the test that spawned it
        // through an AsyncLocal, which rides the ExecutionContext the launch captures. Thread.Start
        // captures it exactly as ThreadPool.QueueUserWorkItem did — this is that invariant, asserted
        // rather than assumed, because it is the one property the executor swap could silently drop.
        AsyncLocal<string?> ambient = new();
        ambient.Value = "flowed";

        using ManualResetEventSlim finished = new(false);
        string? observed = null;

        Goroutine.Start(() =>
        {
            observed = ambient.Value;
            finished.Set();
        });

        Assert.IsTrue(finished.Wait(TimeoutMs), "goroutine did not run");
        Assert.AreEqual("flowed", observed);
    }

    [TestMethod]
    public void RegistryTracksLiveGoroutinesAndRetiresThem() => AssertRegistryTracks(ParkedGoroutines);

    // The race this assembly's earlier tests hand the registry check: a goroutine whose body has
    // SIGNALLED its test and returned, but whose thread has not yet run its Unregister (the scope's
    // dispose, after the body), is still live when the next test reads the registry, and retires while
    // that test counts. ExecutionContextFlowsIntoTheGoroutine and GoroutineRunsOnItsOwnDedicatedBackground-
    // Thread end exactly so, one test before this one. Staged deliberately before every iteration here.
    private const int StressIterations = 100;
    private const int StressParked = 50;

    [TestMethod]
    public void RegistryTracksItsGoroutinesWhileAnEarlierOneIsStillRetiring()
    {
        for (int i = 0; i < StressIterations; i++)
        {
            using ManualResetEventSlim signalled = new(false);

            Goroutine.Start(() => signalled.Set());
            Assert.IsTrue(signalled.Wait(TimeoutMs), "the retiring goroutine did not run");

            AssertRegistryTracks(StressParked);
        }
    }

    // Asserted on the test's OWN goroutines, by identity, and on the count RELATIVE TO ITS OWN PEAK --
    // never against a count taken before they started, which is what an earlier goroutine still
    // retiring made one short. A goroutine that is not the test's can only RETIRE while the test runs
    // (nothing else starts one), so every count assertion below is phrased so a retirement cannot
    // falsify it; a missing registration or a missing retirement still does.
    private static void AssertRegistryTracks(int parked)
    {
        using ManualResetEventSlim release = new(false);
        using CountdownEvent arrived = new(parked);
        using CountdownEvent finished = new(parked);
        long[] ids = new long[parked];

        for (int i = 0; i < parked; i++)
        {
            int slot = i;

            Goroutine.Start(() =>
            {
                ids[slot] = Goroutine.Current!.Id;
                arrived.Signal();
                release.Wait();
                finished.Signal();
            });
        }

        Assert.IsTrue(arrived.Wait(TimeoutMs), "not every goroutine started while the others were parked");

        // All of them are live and parked at once, so the registry must hold every one of them, and its
        // count must cover them all.
        int unregistered = 0;

        foreach (long id in ids)
        {
            if (Goroutine.FromId(id) is null)
                unregistered++;
        }

        Assert.AreEqual(0, unregistered, $"{unregistered} of {parked} live, parked goroutines are missing from the registry");

        int peak = Goroutine.Count;
        Assert.IsTrue(peak >= parked, $"expected at least {parked} live goroutines, got {peak}");

        release.Set();
        Assert.IsTrue(finished.Wait(TimeoutMs), "goroutines did not finish");

        // A goroutine that finished must stop being counted — the registry's half of "a thread that
        // finished its goroutine stops looking like one". The threads retire asynchronously, so this
        // waits for the drain rather than reading immediately after the last body returned: every one
        // of them leaves the registry, and the count falls by at least as many from the peak.
        Assert.IsTrue(SpinWait.SpinUntil(() =>
        {
            foreach (long id in ids)
            {
                if (Goroutine.FromId(id) is not null)
                    return false;
            }

            return Goroutine.Count <= peak - parked;
        }, TimeoutMs), $"registry did not retire finished goroutines: still {Goroutine.Count} live against a peak of {peak}");
    }

    [TestMethod]
    public void CapacityEqualsDemand()
    {
        // The ladder, in miniature: every goroutine must ARRIVE before any is released, so none can
        // finish to free capacity for the next. On a shared pool this can only proceed as fast as the
        // pool injects threads (~1/s), and 500 of them would take minutes; here it is milliseconds.
        // The assertion is the barrier itself — if capacity did not equal demand, arrived.Wait times
        // out and this fails by timeout, exactly as the behavioral guard does.
        using ManualResetEventSlim release = new(false);
        using CountdownEvent arrived = new(ParkedGoroutines);
        using CountdownEvent finished = new(ParkedGoroutines);

        List<int> observed = new(ParkedGoroutines);

        for (int i = 0; i < ParkedGoroutines; i++)
        {
            int id = i;

            Goroutine.Start(() =>
            {
                arrived.Signal();
                release.Wait();

                lock (observed)
                    observed.Add(id);

                finished.Signal();
            });
        }

        Assert.IsTrue(arrived.Wait(TimeoutMs),
            $"only {ParkedGoroutines - arrived.CurrentCount} of {ParkedGoroutines} goroutines started — parked goroutines are still consuming shared capacity");

        release.Set();

        Assert.IsTrue(finished.Wait(TimeoutMs), "goroutines did not finish");
        Assert.AreEqual(ParkedGoroutines, observed.Count);
    }

    [TestMethod]
    public void NestedEnterKeepsOneIdentity() => AssertNestedEnterKeepsOneIdentity(afterBaseline: null);

    // An earlier test's goroutine retiring inside this one's window (StragglerStaging), staged.
    [TestMethod]
    public void NestedEnterKeepsOneIdentityWhileAnEarlierGoroutineRetires()
    {
        for (int i = 0; i < StragglerStaging.Iterations; i++)
            AssertNestedEnterKeepsOneIdentity(StragglerStaging.Stage());
    }

    private static void AssertNestedEnterKeepsOneIdentity(Action? afterBaseline)
    {
        // The test host calls Enter() on a thread it created itself. Entering a thread that is ALREADY
        // a goroutine must not mint a second one for it, and the inner scope must not retire the outer
        // identity when it disposes — either would corrupt the live count under a host.
        using ManualResetEventSlim finished = new(false);

        int beforeInner = 0;
        int duringInner = 0;
        int afterInner = 0;
        bool stillOnGoroutine = false;

        // Nothing is asserted INSIDE the body: no containment policy is installed here, so an
        // AssertFailedException escaping a goroutine would take the whole test host down through
        // golib's backstop (that fidelity is the executor's own contract). Observations come out; the
        // assertions happen on the test's thread.
        Goroutine.Start(() =>
        {
            beforeInner = Goroutine.Count;
            afterBaseline?.Invoke();

            using (Goroutine.Enter())
                duringInner = Goroutine.Count;

            afterInner = Goroutine.Count;
            stillOnGoroutine = Goroutine.OnGoroutine;
            finished.Set();
        });

        Assert.IsTrue(finished.Wait(TimeoutMs), "goroutine did not run");
        Assert.AreEqual(beforeInner, duringInner, "a nested Enter minted a second identity for one thread");
        Assert.AreEqual(duringInner, afterInner, "the inner scope retired the outer identity");
        Assert.IsTrue(stillOnGoroutine, "the thread stopped looking like a goroutine while it still was one");
    }

    [TestMethod]
    public void StackReserveDefaultsToTheGoScaleReservation()
    {
        // A reservation is address space, not memory. The default matters because a goroutine body is
        // arbitrary Go code and Go stacks grow, while a .NET stack overflow is uncatchable. The
        // reserve is resolved once from the environment, so a host that exercised the documented
        // override is testing a different constant — say so rather than failing on it.
        string? overridden = Environment.GetEnvironmentVariable(Goroutine.StackReserveVariable);

        if (!string.IsNullOrWhiteSpace(overridden))
        {
            Assert.Inconclusive($"{Goroutine.StackReserveVariable} is set to \"{overridden}\" in this environment");
            return;
        }

        Assert.AreEqual(256 * 1024 * 1024, Goroutine.StackReserve);
    }

    [TestMethod]
    public void StackReserveOverrideParsesGoStyleByteSizes()
    {
        // The override is resolved ONCE per process from the environment, so the only way to cover
        // its parser without spawning a child is to test the parser itself. Worth covering rather
        // than eyeballing: every case here is reached only when someone actually sets the variable,
        // which is exactly the input no gate would otherwise exercise.
        AssertParses("268435456", 268435456);
        AssertParses("512B", 512);
        AssertParses("64KiB", 64L * 1024);
        AssertParses("256MiB", 256L * 1024 * 1024);
        AssertParses("1GiB", 1024L * 1024 * 1024);
        AssertParses("  32MiB  ", 32L * 1024 * 1024);

        // 0 is meaningful: it asks Thread for the framework default stack.
        AssertParses("0", 0);

        AssertRejects("");
        AssertRejects("lots");
        AssertRejects("-1");        // NumberStyles.None admits no sign
        AssertRejects("1.5MiB");    // no fractions, exactly as Go's own parser
        AssertRejects("256MB");     // Go spells the power-of-two units, so this is a typo, not 256e6
        AssertRejects("9223372036854775807GiB"); // overflows rather than wrapping to something plausible

        static void AssertParses(string setting, long expected)
        {
            Assert.IsTrue(Goroutine.TryParseByteSize(setting, out long bytes), $"failed to parse \"{setting}\"");
            Assert.AreEqual(expected, bytes, $"wrong value for \"{setting}\"");
        }

        static void AssertRejects(string setting) =>
            Assert.IsFalse(Goroutine.TryParseByteSize(setting, out _), $"accepted \"{setting}\"");
    }
}
