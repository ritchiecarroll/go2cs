// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using fmt = fmt_package;
using cpu = go.@internal.cpu_package;
using goarch = go.@internal.goarch_package;
using gc = go.@internal.runtime.gc_package;
using scan = go.@internal.runtime.gc.scan_package;
using bits = math.bits_package;
using rand = math.rand.rand_package;
using slices = slices_package;
using sync = go.sync_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go;
using go.@internal;
using go.@internal.runtime;
using go.@internal.runtime.gc;
using math;
using math.rand;

partial class scan_test_package {

// type scanFunc is a methodless func type — rendered inline as its base delegate

internal static void testScanSpanPacked(ж<testing.T> Ꮡt, Func<@unsafe.Pointer, ж<uintptr>, ж<gc.ObjMask>, uintptr, ж<gc.PtrMask>, int32> scanF) {
    GoFrame ᒐ = default;
    try {
        var scanR = scan.ScanSpanPackedReference;

        // Construct a fake memory
        var (mem, free) = makeMem(new testing_TжTB(Ꮡt), 1);
        var freeʗ1 = free;
        defer(freeʗ1, ref ᒐ);
        foreach (var i in range(mem)) {
            // Use values > heap.PageSize because a scan function can discard
            // pointers smaller than this.
            mem[i] = (uintptr)((nint)gc.PageSize + i + 1);
        }

        // Construct a random pointer mask
        var rnd = rand.New(new rand.PCGжSource(rand.NewPCG(42, 42)));
        ref var ptrs = ref heap(new gc.PtrMask(), out var Ꮡptrs);
        foreach (var i in range(ptrs)) {
            ptrs[i] = (uintptr)rnd.Uint64();
        }

        var bufF = new slice<uintptr>(gc.PageWords);
        var bufR = new slice<uintptr>(gc.PageWords);
        var bufFʗ1 = bufF;
        var bufRʗ1 = bufR;
        var memʗ1 = mem;
        var scanRʗ1 = scanR;
        testObjs(Ꮡt, (ж<testing.T> tΔ1, nint sizeClass, ж<gc.ObjMask> objs) => {
            var nF = scanF(@unsafe.Pointer.FromBox(Ꮡ(memʗ1, 0)), Ꮡ(bufFʗ1, 0), objs, (uintptr)sizeClass, Ꮡptrs);
            var nR = scanRʗ1(@unsafe.Pointer.FromBox(Ꮡ(memʗ1, 0)), Ꮡ(bufRʗ1, 0), objs, (uintptr)sizeClass, Ꮡptrs);

            if (nR != nF){
                tΔ1.Errorf("want %d count, got %d"u8, nR, nF);
            } else 
            if (!slices.Equal<slice<uintptr>, uintptr>(bufFʗ1.slice(0, nF), bufRʗ1.slice(0, nR))) {
                tΔ1.Errorf("want scanned pointers %d, got %d"u8, bufRʗ1.slice(0, nR), bufFʗ1.slice(0, nF));
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testObjs(ж<testing.T> Ꮡt, Action<ж<testing.T>, nint, ж<gc.ObjMask>> f) {
    foreach (var sizeClass in range(gc.NumSizeClasses)) {
        if (sizeClass == 0) {
            continue;
        }
        var size = (uintptr)gc.SizeClassToSize[sizeClass];
        if (size > gc.MinSizeForMallocHeader) {
            break; // Pointer/scalar metadata is not packed for larger sizes.
        }
        Ꮡt.Run(fmt.Sprintf("size=%d"u8, size), (ж<testing.T> tΔ1) => {
            // Scan a few objects near i to test boundary conditions.
            UntypedInt objMask = 0x101;
            var nObj = (uintptr)gc.SizeClassToNPages[sizeClass] * (uintptr)gc.PageSize / size;
            foreach (var i in range<uintptr>(nObj - (uintptr)(bits.Len(objMask) - 1))) {
                tΔ1.Run(fmt.Sprintf("objs=0x%x<<%d"u8, (nint)(objMask), i), (ж<testing.T> tΔ2) => {
                    ref var objs = ref heap(new gc.ObjMask(), out var Ꮡobjs);
                    objs[i / (uintptr)goarch.PtrBits] = (uintptr)objMask << (int)(i % (uintptr)goarch.PtrBits);
                    f(tΔ2, sizeClass, Ꮡobjs);
                });
            }
        });
    }
}

internal static Func<slice<uintptr>> dataCacheSizes = sync.OnceValue(slice<uintptr> () => {
    var cs = cpu.DataCacheSizes();
    foreach (var (i, c) in cs) {
        fmt.Printf("# L%d cache: %d (%d Go pages)\n"u8, i + 1, c, c / (uintptr)gc.PageSize);
    }
    return cs;
});

public static void BenchmarkScanSpanPacked(ж<testing.B> Ꮡb) {
    benchmarkCacheSizes(Ꮡb, benchmarkScanSpanPackedAllSizeClasses);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cacheTinyPages1ˢ = "cache=tiny/pages=1"u8;

internal static void benchmarkCacheSizes(ж<testing.B> Ꮡb, Action<ж<testing.B>, nint> fn) {
    var cacheSizes = dataCacheSizes();
    Ꮡb.Run(cacheTinyPages1ˢ, (ж<testing.B> bΔ1) => {
        fn(bΔ1, 1);
    });
    foreach (var (i, cacheBytes) in cacheSizes) {
        nint pages = (nint)(cacheBytes * 3 / 4) / (nint)gc.PageSize;
        Ꮡb.Run(fmt.Sprintf("cache=L%d/pages=%d"u8, i + 1, pages), (ж<testing.B> bΔ2) => {
            fn(bΔ2, pages);
        });
    }
    if (len(cacheSizes) == 0) {
        return;
    }
    nint ramPages = (nint)(cacheSizes[len(cacheSizes) - 1] * 3 / 2) / (nint)gc.PageSize;
    Ꮡb.Run(fmt.Sprintf("cache=ram/pages=%d"u8, ramPages), (ж<testing.B> bΔ3) => {
        fn(bΔ3, ramPages);
    });
}

internal static void benchmarkScanSpanPackedAllSizeClasses(ж<testing.B> Ꮡb, nint nPages) {
    foreach (var sc in range(gc.NumSizeClasses)) {
        if (sc == 0) {
            continue;
        }
        var size = gc.SizeClassToSize[sc];
        if (size >= gc.MinSizeForMallocHeader) {
            break;
        }
        Ꮡb.Run(fmt.Sprintf("sizeclass=%d"u8, sc), (ж<testing.B> bΔ1) => {
            benchmarkScanSpanPacked(bΔ1, nPages, sc);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string implReferenceˢ = "impl=Reference"u8;
private static readonly @string implGoˢ = "impl=Go"u8;
private static readonly @string implPlatformˢ = "impl=Platform"u8;

internal static void benchmarkScanSpanPacked(ж<testing.B> Ꮡb, nint nPages, nint sizeClass) {
    GoFrame ᒐ = default;
    try {
        var rnd = rand.New(new rand.PCGжSource(rand.NewPCG(42, 42)));

        // Construct a fake memory
        var (mem, free) = makeMem(new testing_BжTB(Ꮡb), nPages);
        var freeʗ1 = free;
        defer(freeʗ1, ref ᒐ);
        foreach (var i in range(mem)) {
            // Use values > heap.PageSize because a scan function can discard
            // pointers smaller than this.
            mem[i] = (uintptr)((nint)gc.PageSize + i + 1);
        }

        // Construct a random pointer mask
        var ptrs = GoReflect.WithElemDims(new slice<gc.PtrMask>(nPages), 16);
        foreach (var i in range(ptrs)) {
            foreach (var j in range(ptrs[i])) {
                ptrs[i][j] = (uintptr)rnd.Uint64();
            }
        }

        // Visit the pages in a random order
        var pageOrder = rnd.Perm(nPages);

        // Create the scan buffer.
        var buf = new slice<uintptr>(gc.PageWords);

        // Sweep from 0 marks to all marks. We'll use the same marks for each page
        // because I don't think that predictability matters.
        var objBytes = (uintptr)gc.SizeClassToSize[sizeClass];
        var nObj = (uintptr)gc.PageSize / objBytes;
        var markOrder = rnd.Perm((nint)nObj);
        UntypedInt steps = 11;
        for (nint i = 0; i < steps; i++) {
            var frac = (float64)i / (float64)(steps - 1D);
            // Set frac marks.
            nint nMarks = (nint)((float64)len(markOrder) * frac + 0.5D);
            ref var objMarks = ref heap(new gc.ObjMask(), out var ᏑobjMarks);
            foreach (var (_, mark) in markOrder.slice(0, nMarks)) {
                objMarks[mark / (nint)goarch.PtrBits] |= (uintptr)(((uintptr)1).Lsh((int64)(mark % (nint)goarch.PtrBits)));
            }
            nint greyClusters = 0;
            foreach (var page in range(ptrs)) {
                greyClusters += countGreyClusters(sizeClass, ᏑobjMarks, Ꮡ(ptrs, page));
            }

            // Report MB/s of how much memory they're actually hitting. This assumes
            // 64 byte cache lines (TODO: Should it assume 128 byte cache lines?)
            // and expands each access to the whole cache line. This is useful for
            // comparing against memory bandwidth.
            //
            // TODO: Add a benchmark that just measures single core memory bandwidth
            // for comparison. (See runtime memcpy benchmarks.)
            //
            // TODO: Should there be a separate measure where we don't expand to
            // cache lines?
            var avgBytes = quo((int64)greyClusters * (int64)cpu.CacheLineSize, (int64)len(ptrs));

            var bufʗ1 = buf;
            var memʗ1 = mem;
            var pageOrderʗ1 = pageOrder;
            var ptrsʗ1 = ptrs;
            Ꮡb.Run(fmt.Sprintf("pct=%d"u8, (nint)(100D * frac)), (ж<testing.B> bΔ1) => {
                var bufʗ2 = bufʗ1;
                var memʗ2 = memʗ1;
                var pageOrderʗ2 = pageOrderʗ1;
                var ptrsʗ2 = ptrsʗ1;
                bΔ1.Run(implReferenceˢ, (ж<testing.B> bΔ2) => {
                    bΔ2.SetBytes(avgBytes);
                    foreach (var iΔ1 in range((~bΔ2).N)) {
                        nint page = pageOrderʗ2[iΔ1 % len(pageOrderʗ2)];
                        scan.ScanSpanPackedReference(@unsafe.Pointer.FromBox(Ꮡ(memʗ2, (nint)gc.PageWords * page)), Ꮡ(bufʗ2, 0), ᏑobjMarks, (uintptr)sizeClass, Ꮡ(ptrsʗ2, page));
                    }
                });
                var bufʗ3 = bufʗ1;
                var memʗ3 = memʗ1;
                var pageOrderʗ3 = pageOrderʗ1;
                var ptrsʗ3 = ptrsʗ1;
                bΔ1.Run(implGoˢ, (ж<testing.B> bΔ3) => {
                    bΔ3.SetBytes(avgBytes);
                    foreach (var iΔ2 in range((~bΔ3).N)) {
                        nint page = pageOrderʗ3[iΔ2 % len(pageOrderʗ3)];
                        scan.ScanSpanPackedGo(@unsafe.Pointer.FromBox(Ꮡ(memʗ3, (nint)gc.PageWords * page)), Ꮡ(bufʗ3, 0), ᏑobjMarks, (uintptr)sizeClass, Ꮡ(ptrsʗ3, page));
                    }
                });
                if (scan.HasFastScanSpanPacked()) {
                    var bufʗ4 = bufʗ1;
                    var memʗ4 = memʗ1;
                    var pageOrderʗ4 = pageOrderʗ1;
                    var ptrsʗ4 = ptrsʗ1;
                    bΔ1.Run(implPlatformˢ, (ж<testing.B> bΔ4) => {
                        bΔ4.SetBytes(avgBytes);
                        foreach (var iΔ3 in range((~bΔ4).N)) {
                            nint page = pageOrderʗ4[iΔ3 % len(pageOrderʗ4)];
                            scan.ScanSpanPacked(@unsafe.Pointer.FromBox(Ꮡ(memʗ4, (nint)gc.PageWords * page)), Ꮡ(bufʗ4, 0), ᏑobjMarks, (uintptr)sizeClass, Ꮡ(ptrsʗ4, page));
                        }
                    });
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint countGreyClusters(nint sizeClass, ж<gc.ObjMask> ᏑobjMarks, ж<gc.PtrMask> ᏑptrMask) {
    ref var objMarks = ref ᏑobjMarks.DerefOrNull();
    ref var ptrMask = ref ᏑptrMask.DerefOrNull();

    nint clusters = 0;
    nint lastCluster = -1;

    var expandBy = (uintptr)gc.SizeClassToSize[sizeClass] / (uintptr)goarch.PtrSize;
    foreach (var word in range(gc.PageWords)) {
        var objI = (uintptr)word / expandBy;
        if ((uintptr)(objMarks[objI / (uintptr)goarch.PtrBits] & ((uintptr)1 << (int)(objI % (uintptr)goarch.PtrBits))) == 0) {
            continue;
        }
        if ((uintptr)(ptrMask[word / (nint)goarch.PtrBits] & (((uintptr)1).Lsh((int64)(word % (nint)goarch.PtrBits)))) == 0) {
            continue;
        }
        nint c = word * 8 / (nint)goarch.PtrBits;
        if (c != lastCluster) {
            lastCluster = c;
            clusters++;
        }
    }
    return clusters;
}

public static void BenchmarkScanMaxBandwidth(ж<testing.B> Ꮡb) {
    // Measure the theoretical "maximum" bandwidth of scanning by reproducing
    // the memory access pattern of a full page scan, but using memcpy as the
    // kernel instead of scanning.
    benchmarkCacheSizes(Ꮡb, (ж<testing.B> bΔ1, nint heapPages) => {
        GoFrame ᒐ = default;
        try {
            var (mem, free) = makeMem(new testing_BжTB(bΔ1), heapPages);
            var freeʗ1 = free;
            defer(freeʗ1, ref ᒐ);
            foreach (var i in range(mem)) {
                mem[i] = (uintptr)((nint)gc.PageSize + i + 1);
            }
            var buf = new slice<uintptr>(gc.PageWords);

            // Visit the pages in a random order
            var rnd = rand.New(new rand.PCGжSource(rand.NewPCG(42, 42)));
            var pageOrder = rnd.Perm(heapPages);

            bΔ1.SetBytes((int64)gc.PageSize);

            bΔ1.ResetTimer();
            foreach (var i in range((~bΔ1).N)) {
                nint page = pageOrder[i % len(pageOrder)];
                copy(buf, mem.slice((nint)gc.PageWords * page));
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

} // end scan_test_package
