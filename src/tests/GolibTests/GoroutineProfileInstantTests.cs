using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// A goroutine profile is ONE INSTANT across the goroutine SET and every goroutine's LABELS.
//
// Go takes its goroutine profile with the world stopped, and every goroutine that runs afterwards
// records itself before it does (tryRecordGoroutineProfile), so the set and the labels describe one
// moment. runtime/pprof's TestGoroutineProfileConcurrency/goroutine_launches asserts exactly that: a
// launcher that sets label i and then starts child i must never appear at label i while an earlier
// child is missing.
//
// The managed profile copied the registry, then read each goroutine's label mirror. Nothing stops the
// other goroutines in between, and each goroutine is a thread, so a profiling thread PREEMPTED between
// the two steps paired an old set with new labels. Captured in runtime/pprof rows as "counts
// map[0:1 25:1]": the launcher at label 25, children 1..24 registered (ids below the counter the
// snapshot read) and absent from the copy.
//
// The first arm is deterministic: Goroutine.SnapshotSeamForGuard runs on the profiling thread exactly
// between the copy and the label reads, and lets the launcher advance there. The second is the
// runtime/pprof subtest in miniature, a regression net rather than a red (the stall it needs is rare).
[TestClass]
public class GoroutineProfileInstantTests
{
    // NoInlining for the reason GoroutineProfileTests gives: this frame IS the identity asserted on.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ParkChild(channel<int> c) => c.Receive();

    private static MethodBase MethodOf(string name) =>
        typeof(GoroutineProfileInstantTests).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static bool StartedAt(GoroutineProfileEntry entry, MethodBase function) =>
        entry.Function is not null && entry.Function.MethodHandle == function.MethodHandle;

    // A label object that knows its launch index. Compared by REFERENCE against the arm's own
    // instances, so a sibling arm's leftovers can never be counted.
    private sealed class Step(int index)
    {
        public int Index { get; } = index;
    }

    [TestMethod]
    public void TheSnapshotIsOneInstantAcrossTheSetAndTheLabels()
    {
        channel<int> park = new(0);
        Step[] labels = [new(0), new(1), new(2)];
        MethodBase child = MethodOf(nameof(ParkChild));
        using var launched = new ManualResetEventSlim();
        using var copied = new ManualResetEventSlim();
        using var advanced = new ManualResetEventSlim();

        try
        {
            using (Goroutine.Enter())
            {
                // The launcher: label 0, child 0; then, once the snapshot has copied the set, label 1,
                // child 1, label 2 -- the loop of the runtime/pprof subtest, two iterations of it.
                builtin.goǃ(() =>
                {
                    Goroutine.SetProfileLabels(labels[0]);
                    builtin.goǃ(ParkChild, park);
                    launched.Set();
                    copied.Wait();
                    Goroutine.SetProfileLabels(labels[1]);
                    builtin.goǃ(ParkChild, park);
                    Goroutine.SetProfileLabels(labels[2]);
                    advanced.Set();
                    park.Receive();
                });

                Assert.IsTrue(launched.Wait(TimeSpan.FromSeconds(10)), "the launcher never started child 0");

                // The seam lets the launcher run where a preempted profiler would; with the snapshot
                // one instant, the launcher cannot get there, and the seam gives up after the window.
                GoroutineProfileEntry[] profile;
                Goroutine.SnapshotSeamForGuard = () =>
                {
                    copied.Set();
                    advanced.Wait(TimeSpan.FromMilliseconds(500));
                };

                try
                {
                    profile = Goroutine.ProfileSnapshot();
                }
                finally
                {
                    Goroutine.SnapshotSeamForGuard = null;
                }

                int launcher = profile
                    .Where(e => !StartedAt(e, child) && Array.IndexOf(labels, e.Labels) >= 0)
                    .Select(e => ((Step)e.Labels!).Index)
                    .Single();

                int[] children = [.. profile
                    .Where(e => StartedAt(e, child) && Array.IndexOf(labels, e.Labels) >= 0)
                    .Select(e => ((Step)e.Labels!).Index)
                    .Order()];

                for (int k = 0; k < launcher; k++)
                {
                    Assert.IsTrue(children.Contains(k),
                        $"the launcher reads label {launcher} but child {k}, started before label {launcher} was set, is absent " +
                        $"(children [{string.Join(", ", children)}]): the set and the labels are two instants");
                }
            }
        }
        finally
        {
            copied.Set();
            advanced.Wait(TimeSpan.FromSeconds(10));
            park.Close();
        }
    }

    // The gate's SHARED side is shared: a go statement and a label set proceed while another thread
    // holds it shared, so the gate serializes profiles against launches, never launches against each
    // other. The holder is this thread and the launches run on another, because the gate refuses
    // recursion (NoRecursion) by design.
    [TestMethod]
    public void SharedHoldersOfTheProfileGateNeverBlockEachOther()
    {
        var gate = (ReaderWriterLockSlim)typeof(Goroutine)
            .GetField("s_profileGate", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

        channel<int> park = new(0);
        using var finished = new ManualResetEventSlim();
        Exception? failure = null;

        gate.EnterReadLock();

        try
        {
            var other = new Thread(() =>
            {
                try
                {
                    using (Goroutine.Enter())
                    {
                        Goroutine.SetProfileLabels(new object());
                        builtin.goǃ(ParkChild, park);
                    }
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
                finally
                {
                    finished.Set();
                }
            });

            other.Start();

            Assert.IsTrue(finished.Wait(TimeSpan.FromSeconds(10)),
                "a label set and a go statement blocked while another thread held the profile gate SHARED");
            Assert.IsNull(failure, $"the other thread failed: {failure}");
        }
        finally
        {
            gate.ExitReadLock();
            park.Close();
        }
    }

    // TestGoroutineProfileConcurrency/goroutine_launches, in miniature: a launcher that labels itself
    // i and starts child i, spinning longer each time; a churn chain that labels itself and hands off;
    // and snapshots taken meanwhile, each checked with the subtest's own rule -- the distinct launch
    // labels are 0..max, each held by exactly one goroutine, or two (launcher and child) at max.
    [TestMethod]
    public void NoLaunchLabelIsMissingUnderConcurrentLaunchesAndChurn()
    {
        const int Launches = 120;
        const int Snapshots = 400;

        channel<int> park = new(0);
        Step[] loop = [.. Enumerable.Range(0, Launches).Select(i => new Step(i))];
        object churnLabel = new();
        MethodBase child = MethodOf(nameof(ParkChild));
        int stop = 0;
        using var launcherDone = new ManualResetEventSlim();
        using var churnDone = new ManualResetEventSlim();
        using var ready = new CountdownEvent(2);

        void Churn(int i)
        {
            Goroutine.SetProfileLabels(churnLabel);

            if (i == 0)
                ready.Signal();

            if (Volatile.Read(ref stop) == 0)
                builtin.goǃ(() => Churn(i + 1));
            else
                churnDone.Set();
        }

        try
        {
            using (Goroutine.Enter())
            {
                builtin.goǃ(() =>
                {
                    for (int i = 0; i < Launches && Volatile.Read(ref stop) == 0; i++)
                    {
                        Goroutine.SetProfileLabels(loop[i]);
                        builtin.goǃ(ParkChild, park);

                        for (int j = 0; j < i; j++)
                            Thread.Yield();

                        if (i == 0)
                            ready.Signal();
                    }

                    launcherDone.Set();
                    park.Receive();
                });

                builtin.goǃ(() => Churn(0));

                Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(10)), "the launcher or the churn never started");

                for (int s = 0; s < Snapshots && !launcherDone.IsSet; s++)
                {
                    GoroutineProfileEntry[] profile = Goroutine.ProfileSnapshot();
                    Dictionary<int, int> counts = [];

                    foreach (GoroutineProfileEntry entry in profile)
                    {
                        if (Array.IndexOf(loop, entry.Labels) < 0)
                            continue;

                        int index = ((Step)entry.Labels!).Index;
                        counts[index] = counts.GetValueOrDefault(index) + 1;
                    }

                    int max = counts.Count - 1;

                    for (int j = 0; j <= max; j++)
                    {
                        int n = counts.GetValueOrDefault(j);

                        if (n == 1 || (n == 2 && j == max))
                            continue;

                        Assert.Fail($"snapshot {s}: launch label {j} is held by {n} goroutines (max {max}), counts " +
                            $"{string.Join(" ", counts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"))}");
                    }
                }
            }
        }
        finally
        {
            Volatile.Write(ref stop, 1);
            launcherDone.Wait(TimeSpan.FromSeconds(30));
            churnDone.Wait(TimeSpan.FromSeconds(30));
            park.Close();
        }
    }
}
