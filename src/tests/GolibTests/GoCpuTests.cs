using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// golib's managed bodies for golang.org/x/sys/cpu's x86 assembly (sqlite-cs S21): <c>cpuid</c> reads the
/// processor through <see cref="X86Base.CpuId"/>, and <c>xgetbv</c> SYNTHESISES XCR0 from what .NET reports
/// the OS enables, since .NET has no XGETBV.
/// </summary>
[TestClass]
public class GoCpuTests
{
    [TestMethod]
    public void CpuidReadsTheProcessorThroughX86Base()
    {
        if (!X86Base.IsSupported)
        {
            Assert.AreEqual((0u, 0u, 0u, 0u), GoCpu.Cpuid(0, 0), "off x86, every leaf reads zero, as Go's flags are all false");
            return;
        }

        // The two reads may run on different logical processors (on Windows with more than one, they are pinned to
        // two on purpose, so the comparison crosses cores every time rather than by chance).
        bool crossCores = OperatingSystem.IsWindows() && Environment.ProcessorCount > 1;

        foreach ((uint leaf, uint subleaf) in new[] { (0u, 0u), (1u, 0u), (7u, 0u), (0x80000000u, 0u) })
        {
            (int eax, int ebx, int ecx, int edx) = crossCores ? OnProcessor(0, () => X86Base.CpuId((int)leaf, (int)subleaf)) : X86Base.CpuId((int)leaf, (int)subleaf);
            (uint, uint, uint, uint) actual = crossCores ? OnProcessor(1, () => GoCpu.Cpuid(leaf, subleaf)) : GoCpu.Cpuid(leaf, subleaf);
            Assert.AreEqual(PerCoreMasked(leaf, ((uint)eax, (uint)ebx, (uint)ecx, (uint)edx)), PerCoreMasked(leaf, actual), $"leaf {leaf:x}, subleaf {subleaf}");
        }
    }

    // Leaf 1 EBX bits 31:24 are the INITIAL APIC ID of the logical processor that executed CPUID, so two reads on
    // two cores differ there and nowhere else; x/sys/cpu never reads them. No other leaf the test reads carries a
    // per-core field (leaves 4, 0xB and 0x1F, which do, are not read).
    private static (uint, uint, uint, uint) PerCoreMasked(uint leaf, (uint eax, uint ebx, uint ecx, uint edx) r) =>
        leaf == 1 ? (r.eax, r.ebx & 0x00FF_FFFFu, r.ecx, r.edx) : r;

    // Runs read with the calling thread pinned to one logical processor, then restores the thread's mask.
    private static T OnProcessor<T>(int processor, Func<T> read)
    {
        nint thread = GetCurrentThread();
        nuint previous = SetThreadAffinityMask(thread, (nuint)1 << processor);
        Assert.AreNotEqual((nuint)0, previous, $"SetThreadAffinityMask to processor {processor}");

        try
        {
            Thread.Yield();
            return read();
        }
        finally
        {
            SetThreadAffinityMask(thread, previous);
        }
    }

    [DllImport("kernel32.dll")]
    private static extern nint GetCurrentThread();

    [DllImport("kernel32.dll")]
    private static extern nuint SetThreadAffinityMask(nint thread, nuint mask);

    [TestMethod]
    public void TheSynthesisedXcr0SetsEachStateFromItsAnswer()
    {
        Assert.AreEqual(0u, GoCpu.SynthesizedXcr0(false, true, true, true), "off x86 there is no XCR0");
        Assert.AreEqual(0b1u, GoCpu.SynthesizedXcr0(true, false, false, false), "x87 state is always enabled");
        Assert.AreEqual(0b11u, GoCpu.SynthesizedXcr0(true, true, false, false), "SSE state");
        Assert.AreEqual(0b111u, GoCpu.SynthesizedXcr0(true, true, true, false), "SSE and AVX state");
        Assert.AreEqual(0b1110_0111u, GoCpu.SynthesizedXcr0(true, true, true, true), "AVX-512's three states, opmask, ZMM_Hi256, Hi16_ZMM");
        Assert.AreEqual(0b11u, GoCpu.SynthesizedXcr0(true, true, false, true), "AVX-512 state is never set without AVX's");
    }

    [TestMethod]
    public void XgetbvAgreesWithWhatDotNetSaysTheOsEnables()
    {
        (uint eax, uint edx) = GoCpu.Xgetbv();

        Assert.AreEqual(0u, edx);
        Assert.AreEqual(Avx.IsSupported, (eax & 0b110u) == 0b110u, "x/sys/cpu's osSupportsAVX reads bits 1 and 2");
        Assert.AreEqual(Avx512F.IsSupported, (eax & 0b1110_0000u) == 0b1110_0000u, "x/sys/cpu's osSupportsAVX512 reads bits 5 to 7");
    }
}
