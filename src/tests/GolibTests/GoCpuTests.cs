using System.Runtime.Intrinsics.X86;
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

        foreach ((uint leaf, uint subleaf) in new[] { (0u, 0u), (1u, 0u), (7u, 0u), (0x80000000u, 0u) })
        {
            (int eax, int ebx, int ecx, int edx) = X86Base.CpuId((int)leaf, (int)subleaf);
            Assert.AreEqual(((uint)eax, (uint)ebx, (uint)ecx, (uint)edx), GoCpu.Cpuid(leaf, subleaf), $"leaf {leaf:x}, subleaf {subleaf}");
        }
    }

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
