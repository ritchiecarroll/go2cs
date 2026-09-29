using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// runtime.sysAllocOS / sysFreeOS on Windows (src/core/runtime/windows/mem_windows_impl.cs). The converted
// bodies reached VirtualAlloc/VirtualFree through stdcall -> asmcgocall and threw; one throw under
// persistentalloc1 held globalAlloc's lock and echoed as 70 abandoned-lock panics on the runtime row.
// Windows-only by construction: the bodies and seams live in runtime's windows/ folder.
[TestClass]
public class RuntimeSysAllocWindowsTests
{
    [TestMethod]
    public void SysAllocOSReturnsZeroedGranularityAlignedWritablePagesAndSysFreeOSReleasesThem()
    {
        const nuint n = 64 * 1024;
        nuint p = runtime_package.GoSysAllocOS(n);

        Assert.AreNotEqual((nuint)0, p, "sysAllocOS returned nil for 64 KiB");

        try
        {
            Assert.AreEqual((nuint)0, p % (64 * 1024), "VirtualAlloc's allocation granularity: a 64 KiB-aligned base");

            byte[] probe = new byte[(int)n];
            Marshal.Copy((nint)p, probe, 0, probe.Length);
            Assert.IsTrue(Array.TrueForAll(probe, b => b == 0), "sysAllocOS pages are zeroed (Go's contract for fresh OS memory)");

            Marshal.WriteByte((nint)p, 0x5a);
            Marshal.WriteByte((nint)p + (nint)(n - 1), 0xa5);
            Assert.AreEqual((byte)0x5a, Marshal.ReadByte((nint)p));
            Assert.AreEqual((byte)0xa5, Marshal.ReadByte((nint)p + (nint)(n - 1)));
        }
        finally
        {
            runtime_package.GoSysFreeOS(p, n);   // throws by Go's own message if VirtualFree refused
        }
    }

    // sysReserveOS / sysUsedOS / sysUnusedOS (W1): the page allocator's reserve, commit and decommit.
    [TestMethod]
    public void ReserveCommitDecommitRoundTripsOverGosKernelCalls()
    {
        const nuint n = 128 * 1024;
        nuint p = runtime_package.GoSysReserveOS(0, n);

        Assert.AreNotEqual((nuint)0, p, "sysReserveOS returned nil for 128 KiB with no hint");

        try
        {
            Assert.AreEqual((nuint)0, p % (64 * 1024), "a reservation starts on the 64 KiB allocation granularity");

            runtime_package.GoSysUsedOS(p, 2 * 4096);
            byte[] probe = new byte[2 * 4096];
            Marshal.Copy((nint)p, probe, 0, probe.Length);
            Assert.IsTrue(Array.TrueForAll(probe, b => b == 0), "freshly committed pages are zeroed");

            Marshal.WriteByte((nint)p + 4096, 0x5a);
            Assert.AreEqual((byte)0x5a, Marshal.ReadByte((nint)p + 4096));

            // Decommit discards the contents; a re-commit reads zero again (Go relies on this for scavenged pages).
            runtime_package.GoSysUnusedOS(p, 2 * 4096);
            runtime_package.GoSysUsedOS(p, 2 * 4096);
            Assert.AreEqual((byte)0, Marshal.ReadByte((nint)p + 4096), "a decommitted then re-committed page is zeroed");
        }
        finally
        {
            runtime_package.GoSysFreeOS(p, n);
        }
    }

    [TestMethod]
    public void ReserveAtATakenHintFallsBackToAKernelChosenAddress()
    {
        const nuint n = 64 * 1024;
        nuint first = runtime_package.GoSysReserveOS(0, n);
        Assert.AreNotEqual((nuint)0, first);

        nuint second = 0;

        try
        {
            // Go: "This will fail if any of [v, v+n) is already reserved. Next let the kernel choose the address."
            second = runtime_package.GoSysReserveOS(first, n);
            Assert.AreNotEqual((nuint)0, second, "the fallback reservation succeeds");
            Assert.AreNotEqual(first, second, "a hint inside an existing reservation is not honored");
        }
        finally
        {
            if (second != 0)
                runtime_package.GoSysFreeOS(second, n);

            runtime_package.GoSysFreeOS(first, n);
        }
    }

    // A range spanning TWO reservations: one VirtualAlloc/VirtualFree call cannot span them, so Go's halving
    // retry commits and decommits the pieces. The two must be ADJACENT: reserve 2n, release it, then reserve
    // each half at its exact hint -- the kernel honours a hint over a range it just freed. (The first cut
    // hinted just past a fresh reservation and the kernel placed it elsewhere, so the arm was INCONCLUSIVE on
    // its first gate run and never exercised the retry.) When the kernel still will not, nothing is asserted.
    [TestMethod]
    public void CommitAndDecommitAcrossTwoAdjacentReservationsTakeGosHalvingRetry()
    {
        const nuint n = 64 * 1024;
        nuint span = runtime_package.GoSysReserveOS(0, 2 * n);
        Assert.AreNotEqual((nuint)0, span);
        runtime_package.GoSysFreeOS(span, 2 * n);

        nuint first = runtime_package.GoSysReserveOS(span, n);
        Assert.AreNotEqual((nuint)0, first);
        nuint second = 0;

        try
        {
            if (first != span)
                Assert.Inconclusive($"the kernel did not honour the first hint ({span:x} then {first:x})");

            second = runtime_package.GoSysReserveOS(first + n, n);

            if (second != first + n)
                Assert.Inconclusive($"the kernel did not place the second reservation adjacently ({first:x} then {second:x})");

            runtime_package.GoSysUsedOS(first, 2 * n);
            Marshal.WriteByte((nint)first, 1);
            Marshal.WriteByte((nint)(first + 2 * n - 1), 2);
            Assert.AreEqual((byte)1, Marshal.ReadByte((nint)first));
            Assert.AreEqual((byte)2, Marshal.ReadByte((nint)(first + 2 * n - 1)), "the second reservation's pages were committed by the retry");

            runtime_package.GoSysUnusedOS(first, 2 * n);   // throws by Go's own message if a piece could not be decommitted
            runtime_package.GoSysUsedOS(first, 2 * n);
            Assert.AreEqual((byte)0, Marshal.ReadByte((nint)(first + 2 * n - 1)), "both reservations were decommitted");
        }
        finally
        {
            if (second != 0)
                runtime_package.GoSysFreeOS(second, n);

            runtime_package.GoSysFreeOS(first, n);
        }
    }
}
