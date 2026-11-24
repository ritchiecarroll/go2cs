using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// M:N row 4: a carrier reserves Goroutine.StackReserve, and a platform that refuses that size gets the largest it
/// grants, halving down to 1 MB, said once on stderr by name; a refusal at 1 MB is a counted start failure. No platform
/// here refuses 256 MB, so these rows force the refusal through the test-only GoScheduler.RefuseCarrierStackAboveForTest
/// and make the fallback fail and recover (safety floor 13; COORD 2026-11-24).
/// </summary>
[TestClass]
[DoNotParallelize]
public class CarrierStackFallbackTests
{
    private const int MB = 1024 * 1024;

    // A carrier loop that ends at once: the rows read how the thread STARTED, not what it runs.
    private static void Idle() { }

    // Starts count carriers under the refusal threshold and returns whether each started, what stderr said, and how many
    // start failures were counted. The process-wide carrier counters are put back after: SchedulerOffTests reads them
    // and must see only the scheduler's own (none with it off).
    private static (bool[] Started, string Stderr, long FailuresCounted) StartCarriers(int refuseAbove, int count)
    {
        TextWriter saved = Console.Error;
        StringWriter captured = new();
        int savedRefuse = GoScheduler.RefuseCarrierStackAboveForTest;
        long startedBefore = GoScheduler.CarriersStarted, failuresBefore = GoScheduler.CarrierStartFailures;

        try
        {
            Console.SetError(captured);
            GoScheduler.ResetCarrierStackForTest(256 * MB);
            GoScheduler.RefuseCarrierStackAboveForTest = refuseAbove;

            bool[] started = Enumerable.Range(0, count).Select(_ => GoScheduler.StartCarrierThread(Idle)).ToArray();

            return (started, captured.ToString(), GoScheduler.CarrierStartFailures - failuresBefore);
        }
        finally
        {
            GoScheduler.RefuseCarrierStackAboveForTest = savedRefuse;
            GoScheduler.RestoreCarrierCountersForTest(startedBefore, failuresBefore);
            Console.SetError(saved);
        }
    }

    [TestMethod]
    public void ARefused256MBCarrierStartsAt128MBAndSaysSoOnce()
    {
        try
        {
            (bool[] started, string stderr, _) = StartCarriers(refuseAbove: 128 * MB, count: 2);

            Assert.IsTrue(started.All(ok => ok), "both carriers start, at the size the platform grants");
            Assert.AreEqual(128 * MB, GoScheduler.CarrierStackSize, "the carrier stack falls back to 128 MB and stays there");

            string[] lines = stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(1, lines.Length, $"the fallback is said exactly once, the later carrier says nothing:\n{stderr}");
            StringAssert.Contains(lines[0], "refused a 256 MB carrier stack");
            StringAssert.Contains(lines[0], "carriers reserve 128 MB");
            StringAssert.Contains(lines[0], Goroutine.StackReserveVariable, "the line names the knob");
        }
        finally
        {
            GoScheduler.ResetCarrierStackForTest(Math.Max(Goroutine.StackReserve, GoScheduler.MinCarrierStackSize));
        }
    }

    [TestMethod]
    public void TwoHalvingsAreSaidOnce()
    {
        try
        {
            // 256 and 128 MB are both refused: the first carrier halves twice and starts at 64 MB, with one line.
            (bool[] started, string stderr, _) = StartCarriers(refuseAbove: 64 * MB, count: 2);

            Assert.IsTrue(started.All(ok => ok));
            Assert.AreEqual(64 * MB, GoScheduler.CarrierStackSize);
            Assert.AreEqual(1, stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length, $"two halvings, one line:\n{stderr}");
        }
        finally
        {
            GoScheduler.ResetCarrierStackForTest(Math.Max(Goroutine.StackReserve, GoScheduler.MinCarrierStackSize));
        }
    }

    [TestMethod]
    public void ARefusalAt1MBIsACountedStartFailure()
    {
        try
        {
            (bool[] started, _, long failures) = StartCarriers(refuseAbove: 0, count: 1);

            Assert.IsFalse(started[0], "no size the halving reaches is granted: the carrier does not start");
            Assert.AreEqual(1, failures, "the refusal at 1 MB is counted as a start failure");
            Assert.AreEqual(GoScheduler.MinCarrierStackSize, GoScheduler.CarrierStackSize, "the halving stops at 1 MB");
        }
        finally
        {
            GoScheduler.ResetCarrierStackForTest(Math.Max(Goroutine.StackReserve, GoScheduler.MinCarrierStackSize));
        }
    }
}
