// GoCpu.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.Intrinsics.X86;

namespace go.golib;

// ---------------------------------------------------------------------------------------------
// GoCpu -- managed bodies for golang.org/x/sys/cpu's two x86 assembly functions (sqlite-cs S21,
// docs/phase4/DESIGN-sqlite-cs-funcptr.md sections 2.3 and 3.7).
//
// x/sys/cpu's init decodes its feature flags from `cpuid` and `xgetbv`, which it implements in
// assembly. A converted program that imports it (modernc.org/libc does on windows and darwin) would
// otherwise die in that init. The converter forwards the two bodyless functions here, by name and
// checked signature (asmForwardBodies, src/go2cs/asmForwardBodies.go), so x/sys/cpu keeps Go's own
// decoding of the bits:
//   - Cpuid reads the processor through X86Base.CpuId: the same leaves and bits Go's assembly reads.
//   - Xgetbv is SYNTHESISED. .NET has no XGETBV, so XCR0 is built from what .NET reports the OS
//     enables: x87 always, SSE state from Sse.IsSupported, AVX state from Avx.IsSupported, the three
//     AVX-512 states from Avx512F.IsSupported. It is a register image from .NET's view of the CPU,
//     not a hardware read.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Managed bodies for golang.org/x/sys/cpu's x86 assembly: <c>cpuid</c> through
/// <see cref="X86Base.CpuId"/>, and a SYNTHESISED <c>xgetbv</c>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static class GoCpu
{
    /// <summary>
    /// x/sys/cpu's <c>cpuid(eaxArg, ecxArg uint32) (eax, ebx, ecx, edx uint32)</c>: the processor's
    /// CPUID leaf <paramref name="eaxArg"/>, subleaf <paramref name="ecxArg"/>. All zero where the
    /// process does not run on x86, as Go's flags are all false there.
    /// </summary>
    public static (uint eax, uint ebx, uint ecx, uint edx) Cpuid(uint eaxArg, uint ecxArg)
    {
        if (!X86Base.IsSupported)
            return (0, 0, 0, 0);

        (int eax, int ebx, int ecx, int edx) = X86Base.CpuId(unchecked((int)eaxArg), unchecked((int)ecxArg));

        return (unchecked((uint)eax), unchecked((uint)ebx), unchecked((uint)ecx), unchecked((uint)edx));
    }

    /// <summary>
    /// x/sys/cpu's <c>xgetbv() (eax, edx uint32)</c>, XCR0 with ecx = 0, SYNTHESISED from .NET's view
    /// of the CPU: bit 0 (x87) on x86, bit 1 (SSE) when <see cref="Sse.IsSupported"/>, bit 2 (AVX) when
    /// <see cref="Avx.IsSupported"/>, bits 5 to 7 (opmask, ZMM_Hi256, Hi16_ZMM) when
    /// <see cref="Avx512F.IsSupported"/> as well. The high word is zero. .NET reports a feature only when
    /// the OS also enables its state, which is what x/sys/cpu reads XCR0 to learn.
    /// </summary>
    public static (uint eax, uint edx) Xgetbv() =>
        (SynthesizedXcr0(X86Base.IsSupported, Sse.IsSupported, Avx.IsSupported, Avx512F.IsSupported), 0);

    /// <summary>The low word of the synthesised XCR0, from the four answers .NET gives.</summary>
    internal static uint SynthesizedXcr0(bool x86, bool sse, bool avx, bool avx512)
    {
        if (!x86)
            return 0;

        uint eax = 1;

        if (sse)
            eax |= 1u << 1;

        if (avx)
            eax |= 1u << 2;

        if (avx && avx512)
            eax |= (1u << 5) | (1u << 6) | (1u << 7);

        return eax;
    }
}
