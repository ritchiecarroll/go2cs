using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class SelectParkFairnessTests
{
    // Go chooses uniformly at random among a select's ready cases, and that includes the cases of a
    // select that PARKED: when nothing is ready, Go's selectgo enqueues one waiter per case in its
    // lock order, which for several cases on ONE channel is derived from the shuffled poll order, so
    // the counterpart that finally arrives meets those waiters in a random order. runtime's
    // TestPseudoRandomSend reads exactly this (two `c <- v` cases on one unbuffered channel).
    //
    // golib's poll pass was always fair (BuildPollOrder is a Fisher-Yates shuffle), but its PARK
    // path enqueued the waiters in source order, so the counterpart's dequeue took case 0 every time
    // a select parked: 10,000 of 10,000 parked selects went to case 0, z = +100. These guards drive
    // the park path alone. The counterpart waits until THIS iteration's select has started (so the
    // previous select's losing waiter is already unlinked) and its waiters are queued, read through
    // the channel's internal queues by reflection.
    //
    // The bound: N = 10,000 selects and a pass band of |z| <= 4.5, z = (case0 - N/2) / sqrt(N/4),
    // which is |case0 - 5,000| <= 225. On a fair select the exact two-sided binomial false-failure
    // rate is 6.5e-6 per test (about 1.9e-5 for the three tests together); the defect fails it at
    // z = +100.

    private const int Selects = 10_000;
    private const double MaxAbsZ = 4.5;

    // Walls run 0.5-15 s under load. The counterpart spins without sleeping, so it also stops when the
    // test exits (an assert failure included) or when this bound passes, whichever comes first.
    private const int TimeoutMs = 120_000;

    private static readonly FieldInfo s_core = typeof(channel<nint>).GetField("m_core", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly Type s_chanCore = typeof(channel<nint>).Assembly.GetType("go.ChanCore", throwOnError: true)!;
    private static readonly FieldInfo s_sendq = s_chanCore.GetField("Sendq", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo s_recvq = s_chanCore.GetField("Recvq", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly PropertyInfo s_isEmpty = s_sendq.FieldType.GetProperty("IsEmpty", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [TestMethod, Timeout(TimeoutMs)]
    public void ParkedSendCasesOnOneChannelWinEqually()
    {
        RunParked(send: true, c => select(c.Sending(1), c.Sending(0)));
    }

    [TestMethod, Timeout(TimeoutMs)]
    public void ParkedReceiveCasesOnOneChannelWinEqually()
    {
        RunParked(send: false, c =>
        {
            int winner = select(c.Receiving, c.Receiving);

            // Pops the frame the receive win pushed, as the emitted `case N when c.ꟷᐳ(out v):` does.
            Assert.IsTrue(c.Received(out nint _), "a receive win must leave its value pending");
            return winner;
        });
    }

    // reflect.Select reaches the same engine (GoReflect.RunSelect calls SelectRuntime.Run).
    [TestMethod, Timeout(TimeoutMs)]
    public void ParkedReflectSelectCasesOnOneChannelWinEqually()
    {
        RunParked(send: true, c =>
        {
            (nint winner, _, _) = GoReflect.RunSelect([c, c], [true, true], [(nint)1, (nint)0], hasDefault: false);
            return (int)winner;
        });
    }

    // Runs Selects parked selects over two cases on one unbuffered channel, with a counterpart that
    // arrives only after each select has parked, and asserts the split between the two cases.
    private static void RunParked(bool send, Func<channel<nint>, int> selectOnce)
    {
        channel<nint> c = new(0);
        channel<EmptyStruct> done = new(0);
        int started = 0;
        int case0 = 0;
        int case1 = 0;
        int wrongValues = 0;
        int exited = 0;
        Stopwatch elapsed = Stopwatch.StartNew();

        bool stopped() => Volatile.Read(ref exited) != 0 || elapsed.ElapsedMilliseconds > TimeoutMs;

        Goroutine.Start(() =>
        {
            try
            {
                for (int i = 0; i < Selects; i++)
                {
                    int iteration = i + 1;

                    if (!SpinUntil(() => Volatile.Read(ref started) == iteration && HasWaiter(c, send ? s_sendq : s_recvq), stopped))
                    {
                        // The test exited or ran out of time: release a select still parked on c, then stop.
                        c.Close();
                        return;
                    }

                    if (send)
                    {
                        // Case 0 sends 1 and case 1 sends 0, so the value names the case that fired.
                        if (c.Receive() == 1)
                            case0++;
                        else
                            case1++;
                    }
                    else
                    {
                        c.Send(i);
                    }
                }
            }
            finally
            {
                done.Close();
            }
        });

        try
        {
            for (int i = 0; i < Selects; i++)
            {
                Volatile.Write(ref started, i + 1);
                int winner = selectOnce(c);

                if (!send)
                {
                    if (winner == 0)
                        case0++;
                    else if (winner == 1)
                        case1++;
                    else
                        wrongValues++;
                }
                else if (winner is not (0 or 1))
                {
                    wrongValues++;
                }
            }

            done.Receive();
        }
        finally
        {
            Volatile.Write(ref exited, 1);
        }

        Assert.AreEqual(0, wrongValues, "a select returned something other than one of its two cases");
        Assert.AreEqual(Selects, case0 + case1, "every select must fire exactly one case");

        double z = (case0 - Selects / 2.0) / Math.Sqrt(Selects / 4.0);

        Assert.IsTrue(Math.Abs(z) <= MaxAbsZ,
            $"parked select is not uniform: case 0 won {case0} and case 1 won {case1} of {Selects} (z = {z:F1}, band |z| <= {MaxAbsZ})");
    }

    private static bool HasWaiter(channel<nint> c, FieldInfo queue)
    {
        object core = s_core.GetValue(c)!;
        return !(bool)s_isEmpty.GetValue(queue.GetValue(core))!;
    }

    // False when stopped() answered first.
    private static bool SpinUntil(Func<bool> condition, Func<bool> stopped)
    {
        SpinWait spin = new();

        while (!condition())
        {
            if (stopped())
                return false;

            spin.SpinOnce(sleep1Threshold: -1);
        }

        return true;
    }
}
