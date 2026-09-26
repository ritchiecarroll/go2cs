using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using Δruntime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// The runtime's per-thread g against a goroutine identity LENT to more than one thread
/// (<see cref="Goroutine.EnterAsMain"/>, which the -tests host uses for its run thread while the host's
/// own main thread keeps goroutine 1). getg's g is cached per THREAD and was published into the shared
/// Goroutine record only when it was minted, so the record could name ANOTHER holder's g -- running --
/// when the holder that actually parked was readied, and the ready panicked "status is 2" (R's
/// coro-pool battery; i9 census, ledger 655ec260cc). Two separate triggers, one arm each:
/// </summary>
/// <remarks>
/// <para>
/// R1, publish on park: the parker adopted goroutine 1 and minted its g, and a second holder minted
/// AFTER it, so the record names the second holder's running g. Only the parker publishing its own g
/// at park fixes this; a re-mint cannot, since the parker's identity never changed.
/// </para>
/// <para>
/// R2, re-mint on an identity change: the parker minted its g BEFORE adopting goroutine 1 (goid 0, no
/// identity, nothing published), and the cache never noticed the adoption. R1 alone would publish that
/// stale g and hide the panic, so this arm asserts the goid too; and the mirror, a thread whose lent
/// scope ended, must stop answering as goroutine 1 (the -tests host's run thread goes back to the pool).
/// </para>
/// <para>
/// DEDICATED threads stand in for the pool on purpose: a pool thread can be the very thread golib's
/// module initializer registered as goroutine 1, which makes EnterAsMain inert and the arm vacuous.
/// "Minted before adopting" is the property, and a fresh thread gives it deterministically. The
/// coroutine exit's ready runs on the coro's own thread, so the runtime's ready transition is wrapped to
/// RECORD a panic instead of letting it end the host; MSTest runs this assembly serially, which is what
/// makes swapping that global slot for one test safe, and it is restored in <c>finally</c>.
/// </para>
/// </remarks>
[TestClass]
public class GetgLentIdentityTests
{
    private const int TimeoutMs = 30000;

    [TestInitialize]
    public void RuntimeHooksAreInstalled()
    {
        _ = Δruntime.GoStatusWaiting;
        Assert.IsNotNull(Goroutine.ReadyTransition, "the runtime's ready transition is not installed");
    }

    // Wraps the runtime's ready transition and records any panic it throws, by message.
    private sealed class ReadyPanics : IDisposable
    {
        private readonly Action<Goroutine, WaitReason>? m_previous = Goroutine.ReadyTransition;
        internal readonly ConcurrentQueue<string> Seen = new();

        internal ReadyPanics()
        {
            Goroutine.ReadyTransition = (target, reason) =>
            {
                try
                {
                    m_previous?.Invoke(target, reason);
                }
                catch (PanicException panic)
                {
                    Seen.Enqueue(panic.Message);
                }
            };
        }

        public void Dispose() => Goroutine.ReadyTransition = m_previous;
    }

    private static Thread StartThread(Action body, ConcurrentQueue<Exception> failures)
    {
        Thread thread = new(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
        })
        { IsBackground = true };

        thread.Start();
        return thread;
    }

    private static void Await(ManualResetEventSlim signal, string what, ConcurrentQueue<Exception> failures)
    {
        if (!signal.Wait(TimeoutMs))
            Assert.Fail($"{what} never happened; failures: {string.Join(" | ", failures)}");
    }

    // Resumes a fresh coroutine whose body returns at once, so its EXIT readies this thread's goroutine.
    private static void DriveACoroToExit()
    {
        Coro coro = Coro.Start(static () => { });
        coro.Switch();
    }

    private static void AssertNoReadyPanic(ReadyPanics panics, ConcurrentQueue<Exception> failures)
    {
        Assert.IsTrue(failures.IsEmpty, $"a thread failed: {string.Join(" | ", failures)}");
        Assert.IsTrue(panics.Seen.IsEmpty, $"the coroutine's exit ready panicked: {string.Join(" | ", panics.Seen)}");
    }

    [TestMethod]
    public void AParkOnALentMainReadiesTheParkersOwnG()
    {
        ConcurrentQueue<Exception> failures = new();
        using ManualResetEventSlim parkerMinted = new(), otherMinted = new(), parkerDone = new(), release = new();
        using ReadyPanics panics = new();

        // The parker: adopts goroutine 1 and mints its g FIRST (published at mint).
        Thread parker = StartThread(() =>
        {
            using Goroutine.Scope main = Goroutine.EnterAsMain();
            _ = Δruntime.GoGetgSnapshot();
            parkerMinted.Set();
            Await(otherMinted, "the second holder's mint", failures);

            DriveACoroToExit();
            parkerDone.Set();
        }, failures);

        // The second holder: adopts goroutine 1 and mints AFTER the parker, so the record now names ITS
        // g, which stays _Grunning (this thread never parks through the runtime).
        Thread other = StartThread(() =>
        {
            Await(parkerMinted, "the parker's mint", failures);
            using Goroutine.Scope main = Goroutine.EnterAsMain();
            _ = Δruntime.GoGetgSnapshot();
            otherMinted.Set();
            Await(release, "the release", failures);
        }, failures);

        try
        {
            Await(parkerDone, "the parker's coroutine exit", failures);
            AssertNoReadyPanic(panics, failures);
        }
        finally
        {
            release.Set();
            parker.Join(TimeoutMs);
            other.Join(TimeoutMs);
        }
    }

    [TestMethod]
    public void AThreadThatMintedBeforeAdoptingMainParksAsGoroutineOne()
    {
        ConcurrentQueue<Exception> failures = new();
        using ManualResetEventSlim holderMinted = new(), parkerDone = new(), release = new();
        using ReadyPanics panics = new();
        ulong mainGoid = 0, parkerGoid = ulong.MaxValue;

        // A live holder of goroutine 1 whose running g the record names.
        Thread holder = StartThread(() =>
        {
            using Goroutine.Scope main = Goroutine.EnterAsMain();
            mainGoid = Δruntime.GoGetgSnapshot().Goid;
            holderMinted.Set();
            Await(release, "the release", failures);
        }, failures);

        // The parker: mints with NO identity first (goid 0, nothing published), THEN adopts goroutine 1.
        Thread parker = StartThread(() =>
        {
            Await(holderMinted, "the holder's mint", failures);
            _ = Δruntime.GoGetgSnapshot();

            using Goroutine.Scope main = Goroutine.EnterAsMain();
            parkerGoid = Δruntime.GoGetgSnapshot().Goid;

            DriveACoroToExit();
            parkerDone.Set();
        }, failures);

        try
        {
            Await(parkerDone, "the parker's coroutine exit", failures);
            AssertNoReadyPanic(panics, failures);
            Assert.AreNotEqual(0UL, mainGoid, "the holder's g must carry goroutine 1's id");
            Assert.AreEqual(mainGoid, parkerGoid, "a thread that adopted goroutine 1 must answer getg() as goroutine 1, not the goid it minted before adopting");
        }
        finally
        {
            release.Set();
            holder.Join(TimeoutMs);
            parker.Join(TimeoutMs);
        }
    }

    [TestMethod]
    public void ALentScopeThatEndedLeavesNoGoroutineOneOnTheThread()
    {
        ConcurrentQueue<Exception> failures = new();
        ulong lentGoid = 0, afterGoid = ulong.MaxValue;

        Thread thread = StartThread(() =>
        {
            using (Goroutine.EnterAsMain())
                lentGoid = Δruntime.GoGetgSnapshot().Goid;

            // The -tests host's run thread goes back to the pool here: it is no goroutine any more.
            afterGoid = Δruntime.GoGetgSnapshot().Goid;
        }, failures);

        Assert.IsTrue(thread.Join(TimeoutMs), "the thread never finished");
        Assert.IsTrue(failures.IsEmpty, $"the thread failed: {string.Join(" | ", failures)}");
        Assert.AreNotEqual(0UL, lentGoid, "inside the lent scope getg() must answer goroutine 1");
        Assert.AreEqual(0UL, afterGoid, "after the lent scope ends getg() must stop answering goroutine 1");
    }
}
