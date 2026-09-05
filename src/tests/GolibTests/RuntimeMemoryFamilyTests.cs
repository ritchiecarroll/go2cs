using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards increment 6 of the runtime row (the memory family): the Linux primitives sysMmap /
/// sysMunmap / madvise / usleep answer through libc with Go's error convention, and the displaced
/// persistentalloc1 / inPersistentAlloc keep Go's chunk list -- a lock-free push of NATIVE chunks
/// onto a managed pointer slot -- so that two blocks come back distinct, zeroed, and inside the list
/// while a stranger address is outside it. Every arm reaches runtime through Go-prefixed public
/// helpers. Linux only: the primitives are bodied in the linux flavour.
/// </summary>
[TestClass]
public class RuntimeMemoryFamilyTests
{
    private static bool OnLinux => OperatingSystem.IsLinux();

    [TestMethod]
    public void AMappedRegionRoundTripsAndUnmaps()
    {
        if (!OnLinux) Assert.Inconclusive("the primitives are the linux flavour's");
        (nuint address, bool roundTrip, nint firstErr, int madviseRc) = GoSysMmapProbe(1 << 20);
        Assert.AreEqual((nint)0, firstErr, "mmap answered");
        Assert.AreNotEqual((nuint)0, address, "a real address");
        Assert.AreEqual((nuint)0, address & 0xFFF, "page-aligned");
        Assert.IsTrue(roundTrip, "every page written reads back");
        Assert.AreEqual(0, madviseRc, "madvise(DONTNEED) answered 0");
    }

    [TestMethod]
    public void AnImpossibleMapAnswersAnErrnoNotAnException()
    {
        if (!OnLinux) Assert.Inconclusive("the primitives are the linux flavour's");
        nint err = GoSysMmapErrnoProbe();
        Assert.AreEqual((nint)22, err, "EINVAL for a zero-length map, as Go's err");
    }

    [TestMethod]
    public void UsleepSleepsAtLeastWhatItWasAsked()
    {
        if (!OnLinux) Assert.Inconclusive("usleep is bodied in the linux flavour");
        long ms = GoUsleepProbe(20_000);
        Assert.IsTrue(ms >= 19_000, $"20 ms asked, {ms} us slept");
        long us = GoUsleepProbe(200);
        Assert.IsTrue(us >= 190, $"200 us asked (the sub-millisecond spin), {us} us slept");
    }

    [TestMethod]
    public void NThreadsTimesMPushesLandExactlyNTimesMChunks()
    {
        if (!OnLinux) Assert.Inconclusive("persistentalloc reaches sysAlloc, which is the linux flavour's here");
        const int N = 8, M = 16;
        int before = GoPersistentChunkCount();
        int contended = 0;
        System.Threading.Tasks.Parallel.For(0, N, _ =>
        {
            for (int i = 0; i < M; i++)
            {
                if (GoPersistentChunkPushProbe() > 1)
                    System.Threading.Interlocked.Increment(ref contended);
            }
        });
        int after = GoPersistentChunkCount();
        Assert.AreEqual(N * M, after - before, $"no push was lost under contention (before {before}, after {after}, {contended} pushes retried)");
    }

    [TestMethod]
    public void ADistinctBoxOverTheHeadsOwnAddressDoesNotSatisfyTheCas()
    {
        if (!OnLinux) Assert.Inconclusive("persistentalloc reaches sysAlloc, which is the linux flavour's here");
        (nuint headAddress, bool remintedIsDistinct, bool remintedRefused, bool observedAccepted) = GoPersistentChunkCasIdentityProbe();
        Assert.AreNotEqual((nuint)0, headAddress, "a head exists");
        Assert.IsTrue(remintedIsDistinct, "the re-minted box is a different instance over the same address");
        Assert.IsTrue(remintedRefused, "identity, not address: the re-minted comparand does not satisfy the exchange");
        Assert.IsTrue(observedAccepted, "the box observed from the slot does");
    }

    [TestMethod]
    public void PersistentallocHandsOutDistinctZeroedBlocksInsideTheChunkList()
    {
        if (!OnLinux) Assert.Inconclusive("persistentalloc reaches sysAlloc, which is the linux flavour's here");
        (nuint first, nuint second, bool firstInList, bool secondInList, bool strangerInList) = GoPersistentAllocProbe(64);
        Assert.AreNotEqual((nuint)0, first);
        Assert.AreNotEqual(first, second, "two blocks, two addresses");
        Assert.AreEqual((nuint)0, first & 7, "8-byte aligned by default");
        Assert.IsTrue(firstInList && secondInList, "both inside the chunk list persistentChunks walks");
        Assert.IsFalse(strangerInList, "an address that was never carved is outside it");
    }
}
