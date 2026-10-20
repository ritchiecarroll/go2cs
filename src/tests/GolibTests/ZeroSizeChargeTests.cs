using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A zero-size allocation is never profiled. Go's mallocgc returns &amp;zerobase for a size-0 request
/// before it reaches the sampler (go1.26.9 runtime/malloc.go:1074-1077), so make([]T, 0) and a zero-size
/// value leave no heap record even at MemProfileRate = 1. Both of golib's doors, Charge and ChargeBytes,
/// follow it. The control charges one value in the same shape, so the guard cannot pass merely because
/// nothing is recorded at all.
/// </summary>
[TestClass]
public class ZeroSizeChargeTests
{
    // Runs the probe at MemProfileRate = 1 with the recorder replaced, and returns every charge it made.
    private static List<(long size, GoAllocEntry entry)> ChargesOf(Action probe)
    {
        List<(long, GoAllocEntry)> charges = [];
        Action<object, nuint, bool, GoAllocEntry> recorder = GoMemProfile.Recorder;
        nint previous = GoMemProfile.Rate;
        GoMemProfile.Recorder = (_, size, _, entry) => charges.Add(((long)size, entry));
        GoMemProfile.Rate = 1;

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

    [TestMethod]
    public void AZeroCountChargeIsNotSampled()
    {
        // make([]int64, 0): Go's makeslice calls mallocgc(0, ...), which returns zerobase.
        CollectionAssert.AreEqual(new List<(long, GoAllocEntry)>(),
            ChargesOf(() => GoMemProfile.Charge<long>(new object(), 0, GoAllocEntry.MakeSlice)));
    }

    [TestMethod]
    public void AZeroByteChargeIsNotSampled()
    {
        object? allocation = null;
        CollectionAssert.AreEqual(new List<(long, GoAllocEntry)>(),
            ChargesOf(() => GoMemProfile.ChargeBytes(ref allocation, 0, noscan: true, GoAllocEntry.NewArray)));
        Assert.IsNull(allocation, "an unsampled charge allocates nothing");
    }

    [TestMethod]
    public void ControlANonZeroChargeIsSampled()
    {
        object? allocation = null;
        List<(long, GoAllocEntry)> charges = ChargesOf(() =>
        {
            GoMemProfile.Charge<long>(new object(), 1, GoAllocEntry.MakeSlice);
            GoMemProfile.ChargeBytes(ref allocation, 8, noscan: true, GoAllocEntry.NewArray);
        });

        CollectionAssert.AreEqual(new List<(long, GoAllocEntry)> { (8, GoAllocEntry.MakeSlice), (8, GoAllocEntry.NewArray) }, charges);
    }
}
