// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE — Go 1.26.11. INSPIRED BY src/internal/runtime/gc/scan's own tests:
//   - scan_test.go, testObjs: every packed size class (sizes up to gc.MinSizeForMallocHeader), with the object-mask
//     shape 0x101<<i at every i that keeps both bits inside the span;
//   - scan_test.go, testScanSpanPacked: a span whose words are PageSize+i+1, a seeded random pointer mask, and the
//     kernel compared to ScanSpanPackedReference in ORDER;
//   - expand_test.go, testExpand: ExpandAVX512 compared word by word to ExpandReference;
//   - filter_test.go, runTestFilterNil: the nil-elimination shapes.
// Go's tests skip all three kernels unless scan.CanAVX512() holds. These call the kernels directly, so their managed
// bodies are checked on any host.
//
// The rest of this file is original to go2cs:
//
// ScanAvx512KernelTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
//
// What this adds beyond Go's own tests:
// - ExpandAVX512's output starts as all ones, because the kernel OVERWRITES all 16 words.
// - A second scan arm has zero words in the span, so FilterNilAVX512's removal and its order are both on the path.
// - The RAW scanSpanPackedAVX512 is read alone, over zero words under set mask bits. Its count is the scan mask's
//   popcount, and the zeros sit in place, in order: the assembly has no nil test.
// - FilterNilAVX512's results are compared in ORDER, not as Go's multiset, because ScanSpanPackedAVX512's own test
//   needs that order.
// - CanAVX512() is checked against the hardware, read independently through CPUID.

using System;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.@internal;
using go.@internal.runtime.gc;
using static go.builtin;
using gc = go.@internal.runtime.gc_package;
using goarch = go.@internal.goarch_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

[TestClass]
public class ScanAvx512KernelTests
{
    [TestMethod]
    public void ExpandAVX512OverwritesWithTheReferenceExpansion()
    {
        int shapes = 0;

        ForEachPackedShape((sizeClass, objs) =>
        {
            ref var want = ref heap(new gc.PtrMask(), out var Ꮡwant);
            ref var got = ref heap(new gc.PtrMask(), out var Ꮡgot);

            foreach (var i in range(len(got)))
                got[i] = ~(uintptr)0;

            scan_package.ExpandReference(sizeClass, objs, Ꮡwant);
            scan_package.ExpandAVX512(sizeClass, objs, Ꮡgot);

            foreach (var i in range(len(want)))
                Assert.AreEqual(want[i], got[i], $"size class {sizeClass}: word {i}");

            shapes++;
        });

        Assert.IsTrue(shapes > 3000, $"only {shapes} shapes ran");
    }

    [TestMethod]
    public void ScanSpanPackedAVX512MatchesTheReferenceInOrder()
    {
        ScanArm(zeroEvery: 0);
    }

    [TestMethod]
    public void ScanSpanPackedAVX512DropsZeroWordsAndKeepsOrder()
    {
        ScanArm(zeroEvery: 7);
    }

    [TestMethod]
    public void TheRawScanKernelKeepsZeroWordsInOrder()
    {
        // scan_amd64.s scanSpanPackedAVX512 has no nil test: it compresses every word under the scan mask, zeros
        // included, and ScanSpanPackedAVX512 filters them after. Go's own test fills the span with non-zero words, so
        // only this arm tells the two apart. The kernel is internal, so it is reached by reflection, which touches no
        // generated project file.
        var raw = typeof(scan_package).GetMethod("scanSpanPackedAVX512", BindingFlags.NonPublic | BindingFlags.Static)
                  ?? throw new InvalidOperationException("scan_package.scanSpanPackedAVX512 not found");

        var mem = new slice<uintptr>(gc.PageWords);

        foreach (var i in range(len(mem)))
            mem[i] = i % 3 == 0 ? 0 : (uintptr)((nint)gc.PageSize + i + 1);

        ref var ptrs = ref heap(new gc.PtrMask(), out var Ꮡptrs);

        foreach (var i in range(len(ptrs)))
            ptrs[i] = ~(uintptr)0;

        var buf = new slice<uintptr>(gc.PageWords);
        int shapes = 0, zeroShapes = 0;

        ForEachPackedShape((sizeClass, objs) =>
        {
            ref var grey = ref heap(new gc.PtrMask(), out var Ꮡgrey);
            scan_package.ExpandReference(sizeClass, objs, Ꮡgrey);

            var want = new System.Collections.Generic.List<uintptr>();

            for (nint word = 0; word < gc.PageWords; word++)
            {
                if (((ulong)grey[word / 64] >> (int)(word % 64) & 1) != 0)
                    want.Add(mem[word]);
            }

            var n = (int32)raw.Invoke(null, [@unsafe.Pointer.FromBox(Ꮡ(mem, 0)), Ꮡ(buf, 0), objs, (uintptr)sizeClass, Ꮡptrs])!;

            Assert.AreEqual(want.Count, (int)n, $"size class {sizeClass}: the count is the scan mask's popcount");

            for (int i = 0; i < want.Count; i++)
                Assert.AreEqual(want[i], buf[i], $"size class {sizeClass}: word {i}, zeros in place");

            if (want.Contains(0))
                zeroShapes++;
            shapes++;
        });

        Assert.IsTrue(shapes > 3000, $"only {shapes} shapes ran");
        Assert.IsTrue(zeroShapes > shapes / 2, $"only {zeroShapes} of {shapes} shapes put a zero word under the mask");
    }

    [TestMethod]
    public void FilterNilAVX512KeepsTheOrderOfTheNonZeroWords()
    {
        (ulong[] buf, ulong[] want)[] cases =
        [
            ([], []),
            ([4], [4]),
            ([0], []),
            ([0, 4], [4]),
            ([4, 0], [4]),
            ([0, 0, 0, 4], [4]),
            ([4, 0, 0, 0], [4]),
            ([0, 0, 0, 4, 0], [4]),
            ([5, 12], [5, 12]),
            ([0, 5, 12], [5, 12]),
            ([5, 0, 12], [5, 12]),
            ([5, 12, 0], [5, 12]),
            ([0, 5, 0, 12, 0], [5, 12]),
            ([1, 5, 5, 0, 0, 0, 12, 0, 121, 5, 0], [1, 5, 5, 12, 121, 5]),
            // Longer than one 8-word frame, so the kernel's frame loop and its scalar tail both apply in Go.
            ([0, 1, 2, 0, 3, 4, 0, 5, 6, 0, 0, 7, 8, 9, 0, 10, 11, 0, 12], [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12])
        ];

        foreach (var (input, want) in cases)
        {
            var buf = new slice<uintptr>(input.Length);

            for (int i = 0; i < input.Length; i++)
                buf[i] = (uintptr)input[i];

            ж<uintptr> bufp = input.Length == 0 ? null! : Ꮡ(buf, 0);
            int32 n = scan_package.FilterNilAVX512(bufp, (int32)input.Length);

            Assert.AreEqual(want.Length, (int)n, $"[{string.Join(", ", input)}]: length");

            for (int i = 0; i < want.Length; i++)
                Assert.AreEqual((uintptr)want[i], buf[i], $"[{string.Join(", ", input)}]: word {i}");
        }
    }

    [TestMethod]
    public void CanAVX512ReadsTheHardwareAsGoDoes()
    {
        // Go: avx512ScanPackedReqsMet = VL && BW && GFNI && BITALG && DQ && VBMI (scan_amd64.go), with BITALG read
        // from CPUID leaf 7 ECX bit 12 under HasAVX512F (cpu_x86.go doinit). Read independently here.
        bool bitalg = Avx512F.IsSupported && X86Base.IsSupported && (X86Base.CpuId(7, 0).Ecx & (1 << 12)) != 0;

        Assert.AreEqual(bitalg, cpu_package.X86.HasAVX512BITALG, "HasAVX512BITALG against CPUID 7.ECX[12]");

        bool want = Avx512F.VL.IsSupported && Avx512BW.IsSupported && Gfni.IsSupported && bitalg &&
                    Avx512DQ.IsSupported && Avx512Vbmi.IsSupported;

        Assert.AreEqual(want, scan_package.CanAVX512(), "CanAVX512 against the six features");
    }

    private static void ScanArm(int zeroEvery)
    {
        var mem = new slice<uintptr>(gc.PageWords);

        foreach (var i in range(len(mem)))
            mem[i] = zeroEvery > 0 && i % zeroEvery == 0 ? 0 : (uintptr)((nint)gc.PageSize + i + 1);

        // A seeded pointer mask, as Go's test seeds math/rand/v2's PCG(42, 42); any fixed mask serves the comparison.
        var rnd = new Random(42);
        var word = new byte[8];
        ref var ptrs = ref heap(new gc.PtrMask(), out var Ꮡptrs);

        foreach (var i in range(len(ptrs)))
        {
            rnd.NextBytes(word);
            ptrs[i] = (uintptr)BitConverter.ToUInt64(word);
        }

        var bufF = new slice<uintptr>(gc.PageWords);
        var bufR = new slice<uintptr>(gc.PageWords);
        int shapes = 0;

        ForEachPackedShape((sizeClass, objs) =>
        {
            int32 nF = scan_package.ScanSpanPackedAVX512(@unsafe.Pointer.FromBox(Ꮡ(mem, 0)), Ꮡ(bufF, 0), objs, (uintptr)sizeClass, Ꮡptrs);
            int32 nR = scan_package.ScanSpanPackedReference(@unsafe.Pointer.FromBox(Ꮡ(mem, 0)), Ꮡ(bufR, 0), objs, (uintptr)sizeClass, Ꮡptrs);

            Assert.AreEqual(nR, nF, $"size class {sizeClass}: count");

            for (int i = 0; i < nR; i++)
                Assert.AreEqual(bufR[i], bufF[i], $"size class {sizeClass}: word {i}");

            shapes++;
        });

        Assert.IsTrue(shapes > 3000, $"only {shapes} shapes ran");
    }

    // testObjs (scan_test.go): every packed size class, each with 0x101 shifted to every position that keeps both bits
    // inside the span.
    private static void ForEachPackedShape(Action<nint, ж<gc.ObjMask>> f)
    {
        uintptr objMask = 0x101;

        for (nint sizeClass = 1; sizeClass < gc.NumSizeClasses; sizeClass++)
        {
            var size = (uintptr)gc.SizeClassToSize[sizeClass];

            if (size > (uintptr)gc.MinSizeForMallocHeader)
                break;

            var nObj = (uintptr)gc.SizeClassToNPages[sizeClass] * (uintptr)gc.PageSize / size;

            for (uintptr i = 0; i < nObj - 8; i++)
            {
                ref var objs = ref heap(new gc.ObjMask(), out var Ꮡobjs);
                objs[i / (uintptr)goarch.PtrBits] = objMask << (int)(i % (uintptr)goarch.PtrBits);
                f(sizeClass, Ꮡobjs);
            }
        }
    }
}
