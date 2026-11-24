using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// M:N row 4: a carrier reserves main's 1 GiB (MainThread.StackReserve, or the goroutine reserve when that is larger),
/// because any carrier may run a step of main's twin; a platform that refuses that size gets the largest it grants,
/// halving a local size down to 1 MB; only a start that succeeds publishes the smaller size and says it on stderr by name,
/// and a refusal at every size is a counted start failure that leaves the shared size alone. No platform here refuses
/// 1 GiB, so these rows force the refusal through the test-only GoScheduler.RefuseCarrierStackAboveForTest and make the
/// fallback fail and recover (safety floor 13; COORD and P3, 2026-11-24).
/// </summary>
[TestClass]
[DoNotParallelize]
public class CarrierStackFallbackTests
{
    private const int MB = 1024 * 1024;

    // A carrier loop that ends at once: the rows read how the thread STARTED, not what it runs.
    private static void Idle() { }

    private static readonly int s_defaultSize = GoScheduler.DefaultCarrierStackSize;

    // Starts count carriers (all at once when concurrent) from a shared size of startSize under the refusal threshold,
    // and returns whether each started, what stderr said, and how many start failures were counted. The threshold, the
    // shared size and the process-wide carrier counters are put back after: SchedulerOffTests reads the counters and must
    // see only the scheduler's own (none with it off).
    private static (bool[] Started, string Stderr, long FailuresCounted, int SizeAfter) StartCarriers(int startSize, int refuseAbove, int count, bool concurrent = false)
    {
        TextWriter saved = Console.Error;
        StringWriter captured = new();
        TextWriter synchronized = TextWriter.Synchronized(captured);
        int savedRefuse = GoScheduler.RefuseCarrierStackAboveForTest;
        long startedBefore = GoScheduler.CarriersStarted, failuresBefore = GoScheduler.CarrierStartFailures;

        try
        {
            Console.SetError(synchronized);
            GoScheduler.ResetCarrierStackForTest(startSize);
            GoScheduler.RefuseCarrierStackAboveForTest = refuseAbove;

            bool[] started;

            if (concurrent)
            {
                using Barrier barrier = new(count);
                Task<bool>[] starts = Enumerable.Range(0, count)
                    .Select(_ => Task.Factory.StartNew(() => { barrier.SignalAndWait(); return GoScheduler.StartCarrierThread(Idle); }, TaskCreationOptions.LongRunning))
                    .ToArray();
                started = starts.Select(start => start.Result).ToArray();
            }
            else
            {
                started = Enumerable.Range(0, count).Select(_ => GoScheduler.StartCarrierThread(Idle)).ToArray();
            }

            synchronized.Flush();
            return (started, captured.ToString(), GoScheduler.CarrierStartFailures - failuresBefore, GoScheduler.CarrierStackSize);
        }
        finally
        {
            GoScheduler.RefuseCarrierStackAboveForTest = savedRefuse;
            GoScheduler.RestoreCarrierCountersForTest(startedBefore, failuresBefore);
            GoScheduler.ResetCarrierStackForTest(s_defaultSize);
            Console.SetError(saved);
        }
    }

    private static string[] Lines(string stderr) => stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    [TestMethod]
    public void OneRefusalHalvesOnceAndSaysTheSizeThatWorkedOnce()
    {
        (bool[] started, string stderr, long failures, int size) = StartCarriers(256 * MB, refuseAbove: 128 * MB, count: 2);

        Assert.IsTrue(started.All(ok => ok), "both carriers start, at the size the platform grants");
        Assert.AreEqual(0, failures);
        Assert.AreEqual(128 * MB, size, "the size that worked is published and stays");

        string[] lines = Lines(stderr);

        Assert.AreEqual(1, lines.Length, $"one line, from the start that published; the later carrier says nothing:\n{stderr}");
        StringAssert.Contains(lines[0], "refused a 256 MB carrier stack");
        StringAssert.Contains(lines[0], "carriers reserve 128 MB");
        StringAssert.Contains(lines[0], "main included", "the line says what the size bounds");
    }

    [TestMethod]
    public void TwoCarriersRefusedAtOnceHalveOnce()
    {
        (bool[] started, string stderr, long failures, int size) = StartCarriers(256 * MB, refuseAbove: 128 * MB, count: 2, concurrent: true);

        Assert.IsTrue(started.All(ok => ok));
        Assert.AreEqual(0, failures);
        Assert.AreEqual(128 * MB, size, "two concurrent fallbacks publish ONE halving, not two");
        Assert.AreEqual(1, Lines(stderr).Length, $"one of the two publishes and says it:\n{stderr}");
    }

    [TestMethod]
    public void TwoHalvingsAreSaidOnce()
    {
        // 256 and 128 MB are both refused: the first carrier halves twice and starts at 64 MB, with one line.
        (bool[] started, string stderr, _, int size) = StartCarriers(256 * MB, refuseAbove: 64 * MB, count: 2);

        Assert.IsTrue(started.All(ok => ok));
        Assert.AreEqual(64 * MB, size);
        Assert.AreEqual(1, Lines(stderr).Length, $"two halvings, one line:\n{stderr}");
        StringAssert.Contains(stderr, "carriers reserve 64 MB");
    }

    [TestMethod]
    public void ARefusalAt1MBIsCountedAndTheSharedSizeIsUnchanged()
    {
        (bool[] started, string stderr, long failures, int size) = StartCarriers(GoScheduler.MinCarrierStackSize, refuseAbove: 0, count: 1);

        Assert.IsFalse(started[0], "the carrier does not start");
        Assert.AreEqual(1, failures, "the refusal at 1 MB is counted as a start failure");
        Assert.AreEqual(GoScheduler.MinCarrierStackSize, size, "the shared size is unchanged");
        Assert.AreEqual(0, Lines(stderr).Length, $"nothing worked, so nothing is said:\n{stderr}");
    }

    [TestMethod]
    public void ARefusalAtEverySizeLeavesTheSharedSizeAlone()
    {
        // A refusal that is not about the size (a thread or process limit) refuses every size the halving reaches.
        (bool[] started, string stderr, long failures, int size) = StartCarriers(256 * MB, refuseAbove: 0, count: 1);

        Assert.IsFalse(started[0]);
        Assert.AreEqual(1, failures, "counted as one start failure");
        Assert.AreEqual(256 * MB, size, "the shared size is NOT walked down: the next carrier still asks for 256 MB");
        Assert.AreEqual(0, Lines(stderr).Length, $"nothing worked, so nothing is said:\n{stderr}");
    }

    [TestMethod]
    public void ACarrierReservesTheMainGoroutinesStack()
    {
        // Main's twin parks and resumes on whichever carrier is free, so every carrier reserves what main's own thread
        // does without the scheduler: Go's main may recurse that deep (behavioral MainDeepRecursion, under --mn).
        Assert.IsTrue(GoScheduler.DefaultCarrierStackSize >= MainThread.StackReserve,
            $"a carrier reserves {GoScheduler.DefaultCarrierStackSize / MB} MB, short of main's {MainThread.StackReserve / MB} MB");
        Assert.IsTrue(GoScheduler.DefaultCarrierStackSize >= Goroutine.StackReserve, "and never less than a goroutine's thread");
    }

    [TestMethod]
    public void AfterANonSizeRefusalTheNextCarrierStartsAt256MB()
    {
        // A thread or process limit refuses every size; then threads free up (no refusal) and the next carrier, starting
        // from the shared size the refusal left, gets the full 256 MB with nothing said.
        (bool[] refused, _, long failures, int left) = StartCarriers(256 * MB, refuseAbove: 0, count: 1);

        Assert.IsFalse(refused[0]);
        Assert.AreEqual(1, failures);

        (bool[] recovered, string stderr, long failuresAfter, int size) = StartCarriers(left, refuseAbove: int.MaxValue, count: 1);

        Assert.IsTrue(recovered[0], "the next carrier starts once threads free up");
        Assert.AreEqual(0, failuresAfter);
        Assert.AreEqual(256 * MB, size, "and at 256 MB: the refusal did not cost the deep-recursion stack");
        Assert.AreEqual(0, Lines(stderr).Length, $"a full-size start says nothing:\n{stderr}");
    }
}
