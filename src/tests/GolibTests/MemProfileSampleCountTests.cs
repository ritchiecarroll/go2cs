// MemProfileSampleCountTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// A memory-profile sample is not a Go allocation. Go's profiler records a sample outside mallocgc (the M's
/// profStack, persistentalloc'd buckets, a static size-class table), so runtime.MemStats.Mallocs, and
/// testing.AllocsPerRun with it, never counts one. golib's recorder allocates as it records: its stack
/// buffer, and on the first sample the size-class tables' lazy holders. Counted on the allocating thread,
/// those objects land in whatever window that thread is measuring. That is the AppendOfMakeTests flake
/// (BOARD, 2026-11-05): at Go's default rate a sample can fall inside that test's control window, which then
/// reads 4 where it reads 2. MemProfileRate = 1 samples every allocation, so here the same windows read
/// wrong on every run unless the recorder's own objects are kept off the count.
/// </summary>
[TestClass]
public class MemProfileSampleCountTests
{
    // Runs allocate at the given MemProfileRate with runtime's own recorder, wrapped only to count its
    // calls, and returns the objects the counter charged to this thread and the samples recorded.
    private static (long Counted, int Samples) Measure(nint rate, Action allocate)
    {
        // Reading runtime's MemProfileRate loads runtime, whose module initializer installs the recorder.
        nint previous = MemProfileRate;
        Action<object, nuint, bool, GoAllocEntry>? recorder = GoMemProfile.Recorder;
        Assert.IsNotNull(recorder, "runtime installs the memory profile's recorder");

        int samples = 0;
        AllocationCounter.Enable();
        GoMemProfile.Recorder = (allocation, size, noscan, entry) =>
        {
            samples++;
            recorder(allocation, size, noscan, entry);
        };

        try
        {
            MemProfileRate = rate;
            long before = AllocationCounter.CurrentThreadCount;
            allocate();
            return (AllocationCounter.CurrentThreadCount - before, samples);
        }
        finally
        {
            MemProfileRate = previous;
            GoMemProfile.Recorder = recorder;
        }
    }

    // Every allocation sampled, against none: the count must not move.
    private static void AssertSamplingCountsNothing(string shape, Action allocate, long expected)
    {
        var (unsampled, none) = Measure(0, allocate);
        var (sampled, samples) = Measure(1, allocate);

        Assert.AreEqual(0, none, $"{shape}: MemProfileRate = 0 samples nothing");
        Assert.IsTrue(samples > 0, $"{shape}: MemProfileRate = 1 samples every allocation, and none was sampled");
        Assert.AreEqual(expected, unsampled, $"{shape}: the control, with no sample taken");
        Assert.AreEqual(unsampled, sampled, $"{shape}: {samples} sample(s) changed the count; the profiler's own objects are not Go allocations");
    }

    [TestMethod]
    public void ASampledMakeCountsItsBackingAlone() =>
        AssertSamplingCountsNothing("make([]int, 3)", static () => _ = new slice<nint>(3), 1);

    // The flake's own windows, read with every allocation sampled.
    [TestMethod]
    public void TheAppendOfMakeWindowsReadTheSameWhenEverythingIsSampled()
    {
        slice<nint> s = new(2, 2);

        AssertSamplingCountsNothing("append(s, make([]int, 3)...) by make then append", () => _ = appendꓸꓸꓸ(s, new slice<nint>(3)), 2);
        AssertSamplingCountsNothing("append(s, make([]int, 3)...) as extendslice", () => _ = appendꓸꓸꓸ(s, makeꓸꓸꓸ<nint>(3)), 1);
    }

    // A box and a map's growth reach the sampler through its two doors, Charge and ChargeBytes.
    [TestMethod]
    public void ASampledBoxOrMapGrowthCountsWhatItCountsUnsampled()
    {
        var (boxCount, _) = Measure(0, static () => _ = heap(42L, out ж<long> _));
        AssertSamplingCountsNothing("an escaping local", static () => _ = heap(42L, out ж<long> _), boxCount);

        static void grow()
        {
            var m = new map<nint, nint>();

            for (nint i = 0; i < 64; i++)
                m[i] = i;
        }

        var (mapCount, _) = Measure(0, grow);
        AssertSamplingCountsNothing("a map grown to 64 entries", grow, mapCount);
    }
}
