using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the memory profile's map-growth model (COORD ruling 2026-09-26 19:01, a MODEL of Go's table
/// growth rather than an observation; approved to cut 2026-09-29 01:41). A golib map stores through a BCL
/// Dictionary whose growth nothing charges, where Go's map allocates its groups, tables and directory at
/// fixed entry counts and runtime/pprof's TestHeapRuntimeFrames looks for exactly those samples under the
/// inserting function. Each arm lists the allocations go1.24.13's internal/runtime/maps makes for a
/// map[int]int (a 16-byte slot, a 136-byte group), in Go's order, with Go's sizes before roundupsize.
/// Red against master, where a map charges nothing at all.
/// </summary>
[TestClass]
public class MapGrowthProfileTests
{
    private const long Group = 136, Table = 32;

    // Runs the probe at MemProfileRate = 1 with the recorder replaced, and returns every charge it made.
    private static List<(long size, bool noscan)> ChargesOf(Action probe, nint rate = 1) =>
        EntriesOf(probe, rate).ConvertAll(static c => (c.size, c.noscan));

    // The same, with the runtime entry point each charge names.
    private static List<(long size, bool noscan, GoAllocEntry entry)> EntriesOf(Action probe, nint rate = 1)
    {
        // The model runs only where runtime/pprof is reachable (the startup toggle); this host's closure holds it.
        Assert.IsTrue(GoMemProfile.PprofReachable, "the map-growth model is the toggle-on arm");

        List<(long, bool, GoAllocEntry)> charges = [];
        Action<object, nuint, bool, GoAllocEntry> recorder = GoMemProfile.Recorder;
        nint previous = GoMemProfile.Rate;

        GoMemProfile.Recorder = (_, size, noscan, entry) => charges.Add(((long)size, noscan, entry));
        GoMemProfile.Rate = rate;

        try
        {
            probe();
        }
        finally
        {
            GoMemProfile.Rate = previous;
            GoMemProfile.Recorder = recorder;
        }

        return charges;
    }

    private static string Show(List<(long size, bool noscan)> charges) =>
        string.Join(", ", charges.ConvertAll(static c => $"{c.size}{(c.noscan ? "" : "*")}"));

    // growToSmall at the 1st insert; growToTable (new(table), 2 groups, a 1-entry directory) at the 9th;
    // then table.grow doubles at 7/8 load: the 15th, 29th, 57th, 113th, 225th and 449th inserts.
    [TestMethod]
    public void GrowMapChargesGosGrowthSchedule()
    {
        List<(long, bool)> expected =
        [
            (Group, true),
            (Table, false), (2 * Group, true), (8, false),
            (Table, false), (4 * Group, true),
            (Table, false), (8 * Group, true),
            (Table, false), (16 * Group, true),
            (Table, false), (32 * Group, true),
            (Table, false), (64 * Group, true),
            (Table, false), (128 * Group, true),
        ];

        List<(long, bool)> actual = ChargesOf(mapgrowthprobe_package.growMap);

        CollectionAssert.AreEqual(expected, actual, $"want [{Show(expected)}], got [{Show(actual)}] (* = holds pointers)");
    }

    // make(map[int]int, 512): NewMap sizes one 1024-slot table up front (512 * 8 / 7 = 585, rounded up to
    // a power of two), so the 512 inserts that follow never grow it.
    [TestMethod]
    public void HintedMapChargesItsTableAtMake()
    {
        List<(long, bool)> expected = [(8, false), (Table, false), (128 * Group, true)];
        List<(long, bool)> actual = ChargesOf(mapgrowthprobe_package.growMapHinted);

        CollectionAssert.AreEqual(expected, actual, $"want [{Show(expected)}], got [{Show(actual)}]");
    }

    // Past 1024 slots a table splits: the 897th insert finds the full table's 896 slots used and replaces
    // it with two 1024-slot tables, doubling the directory to two entries.
    [TestMethod]
    public void FullTableSplitsIntoTwo()
    {
        List<(long, bool)> actual = ChargesOf(mapgrowthprobe_package.growMapSplit);
        List<(long, bool)> tail = [(Table, false), (128 * Group, true), (Table, false), (128 * Group, true), (16, false)];

        Assert.AreEqual(21, actual.Count, $"got [{Show(actual)}]");
        CollectionAssert.AreEqual(tail, actual.GetRange(16, 5), $"want the split [{Show(tail)}] last, got [{Show(actual)}]");
    }

    // A delete frees its slot for the next insert, so churn below the growth mark never grows the map.
    [TestMethod]
    public void ChurnBelowTheMarkChargesNothing()
    {
        List<(long, bool)> actual = ChargesOf(mapgrowthprobe_package.churnSmallMap);

        CollectionAssert.AreEqual(new List<(long, bool)> { (Group, true) }, actual, $"got [{Show(actual)}]");
    }

    // Each charge names the runtime function Go 1.26 allocates that part through, the frame its heap-profile
    // record carries under mallocgc: newGroups' newarray for a group array, newTable's new(table), and the
    // directory's make([]*table, n).
    [TestMethod]
    public void GrowMapChargesNameGosEntryPoints()
    {
        List<GoAllocEntry> expected =
        [
            GoAllocEntry.NewArray,
            GoAllocEntry.NewObject, GoAllocEntry.NewArray, GoAllocEntry.MakeSlice,
            GoAllocEntry.NewObject, GoAllocEntry.NewArray,
        ];

        List<GoAllocEntry> actual = EntriesOf(mapgrowthprobe_package.growMap).ConvertAll(static c => c.entry).GetRange(0, expected.Count);

        CollectionAssert.AreEqual(expected, actual, $"want [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}]");
    }

    // MemProfileRate = 0 turns the model off with the rest of the profile.
    [TestMethod]
    public void RateZeroChargesNothing()
    {
        Assert.AreEqual(0, ChargesOf(mapgrowthprobe_package.growMap, rate: 0).Count);
    }
}
