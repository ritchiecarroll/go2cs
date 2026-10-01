// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goos = @internal.goos_package;
using rand = global::go.math.rand_package;
using static runtime_package;
using testing = testing_package;
using @internal;
using global::go.math;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

internal static void checkPageCache(ж<testing.T> Ꮡt, global::go.runtime_internal_test_package.PageCache gotʗp, global::go.runtime_internal_test_package.PageCache wantʗp) {
    ref var got = ref heap(gotʗp, out var Ꮡgot);
    ref var want = ref heap(wantʗp, out var Ꮡwant);

    if (Ꮡgot.Base() != Ꮡwant.Base()) {
        Ꮡt.Errorf("bad pageCache base: got 0x%x, want 0x%x"u8, Ꮡgot.Base(), Ꮡwant.Base());
    }
    if (Ꮡgot.Cache() != Ꮡwant.Cache()) {
        Ꮡt.Errorf("bad pageCache bits: got %016x, want %016x"u8, Ꮡgot.Base(), Ꮡwant.Base());
    }
    if (Ꮡgot.Scav() != Ꮡwant.Scav()) {
        Ꮡt.Errorf("bad pageCache scav: got %016x, want %016x"u8, Ꮡgot.Scav(), Ꮡwant.Scav());
    }
}

[GoType("dyn")] internal partial struct TestPageCacheAlloc_hit {
    internal uintptr npages;
    internal uintptr @base;
    internal uintptr scav;
}

[GoType("dyn")] internal partial struct TestPageCacheAlloc_tests {
    internal global::go.runtime_internal_test_package.PageCache cache;
    internal slice<TestPageCacheAlloc_hit> hits;
}

public static void TestPageCacheAlloc(ж<testing.T> Ꮡt) {
    var @base = runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0);
    var tests = new map<@string, TestPageCacheAlloc_tests>{
        ["Empty"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, 0, 0),
            hits: new TestPageCacheAlloc_hit[]{
                new(1, 0, 0),
                new(2, 0, 0),
                new(3, 0, 0),
                new(4, 0, 0),
                new(5, 0, 0),
                new(11, 0, 0),
                new(12, 0, 0),
                new(16, 0, 0),
                new(27, 0, 0),
                new(32, 0, 0),
                new(43, 0, 0),
                new(57, 0, 0),
                new(64, 0, 0),
                new(121, 0, 0)
            }.slice()
        ),
        ["Lo1"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, 0x1, 0x1),
            hits: new TestPageCacheAlloc_hit[]{
                new(1, @base, runtime_internal_test_package.PageSize),
                new(1, 0, 0),
                new(10, 0, 0)
            }.slice()
        ),
        ["Hi1"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, ((uint64)0x1 << (int)(63)), 0x1),
            hits: new TestPageCacheAlloc_hit[]{
                new(1, @base + (uintptr)(63 * runtime_internal_test_package.PageSize), 0),
                new(1, 0, 0),
                new(10, 0, 0)
            }.slice()
        ),
        ["Swiss1"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, 0x20005555, 0x5505),
            hits: new TestPageCacheAlloc_hit[]{
                new(2, 0, 0),
                new(1, @base, runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(2 * runtime_internal_test_package.PageSize), runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(4 * runtime_internal_test_package.PageSize), 0),
                new(1, @base + (uintptr)(6 * runtime_internal_test_package.PageSize), 0),
                new(1, @base + (uintptr)(8 * runtime_internal_test_package.PageSize), runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(10 * runtime_internal_test_package.PageSize), runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(12 * runtime_internal_test_package.PageSize), runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(14 * runtime_internal_test_package.PageSize), runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(29 * runtime_internal_test_package.PageSize), 0),
                new(1, 0, 0),
                new(10, 0, 0)
            }.slice()
        ),
        ["Lo2"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, 0x3, ((uint64)0x2 << (int)(62))),
            hits: new TestPageCacheAlloc_hit[]{
                new(2, @base, 0),
                new(2, 0, 0),
                new(1, 0, 0)
            }.slice()
        ),
        ["Hi2"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, ((uint64)0x3 << (int)(62)), ((uint64)0x3 << (int)(62))),
            hits: new TestPageCacheAlloc_hit[]{
                new(2, @base + (uintptr)(62 * runtime_internal_test_package.PageSize), 2 * runtime_internal_test_package.PageSize),
                new(2, 0, 0),
                new(1, 0, 0)
            }.slice()
        ),
        ["Swiss2"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, ((uint64)0x3333 << (int)(31)), ((uint64)0x3030 << (int)(31))),
            hits: new TestPageCacheAlloc_hit[]{
                new(2, @base + (uintptr)(31 * runtime_internal_test_package.PageSize), 0),
                new(2, @base + (uintptr)(35 * runtime_internal_test_package.PageSize), 2 * runtime_internal_test_package.PageSize),
                new(2, @base + (uintptr)(39 * runtime_internal_test_package.PageSize), 0),
                new(2, @base + (uintptr)(43 * runtime_internal_test_package.PageSize), 2 * runtime_internal_test_package.PageSize),
                new(2, 0, 0)
            }.slice()
        ),
        ["Hi53"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, (((((uint64)1 << (int)(53))) - 1) << (int)(10)), (((((uint64)1 << (int)(16))) - 1) << (int)(10))),
            hits: new TestPageCacheAlloc_hit[]{
                new(53, @base + (uintptr)(10 * runtime_internal_test_package.PageSize), 16 * runtime_internal_test_package.PageSize),
                new(53, 0, 0),
                new(1, 0, 0)
            }.slice()
        ),
        ["Full53"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, ~(uint64)0, (((((uint64)1 << (int)(16))) - 1) << (int)(10))),
            hits: new TestPageCacheAlloc_hit[]{
                new(53, @base, 16 * runtime_internal_test_package.PageSize),
                new(53, 0, 0),
                new(1, @base + (uintptr)(53 * runtime_internal_test_package.PageSize), 0)
            }.slice()
        ),
        ["Full64"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, ~(uint64)0, ~(uint64)0),
            hits: new TestPageCacheAlloc_hit[]{
                new(64, @base, 64 * runtime_internal_test_package.PageSize),
                new(64, 0, 0),
                new(1, 0, 0)
            }.slice()
        ),
        ["FullMixed"u8] = new(
            cache: runtime_internal_test_package.NewPageCache(@base, ~(uint64)0, ~(uint64)0),
            hits: new TestPageCacheAlloc_hit[]{
                new(5, @base, 5 * runtime_internal_test_package.PageSize),
                new(7, @base + (uintptr)(5 * runtime_internal_test_package.PageSize), 7 * runtime_internal_test_package.PageSize),
                new(1, @base + (uintptr)(12 * runtime_internal_test_package.PageSize), 1 * runtime_internal_test_package.PageSize),
                new(23, @base + (uintptr)(13 * runtime_internal_test_package.PageSize), 23 * runtime_internal_test_package.PageSize),
                new(63, 0, 0),
                new(3, @base + (uintptr)(36 * runtime_internal_test_package.PageSize), 3 * runtime_internal_test_package.PageSize),
                new(3, @base + (uintptr)(39 * runtime_internal_test_package.PageSize), 3 * runtime_internal_test_package.PageSize),
                new(3, @base + (uintptr)(42 * runtime_internal_test_package.PageSize), 3 * runtime_internal_test_package.PageSize),
                new(12, @base + (uintptr)(45 * runtime_internal_test_package.PageSize), 12 * runtime_internal_test_package.PageSize),
                new(11, 0, 0),
                new(4, @base + (uintptr)(57 * runtime_internal_test_package.PageSize), 4 * runtime_internal_test_package.PageSize),
                new(4, 0, 0),
                new(6, 0, 0),
                new(36, 0, 0),
                new(2, @base + (uintptr)(61 * runtime_internal_test_package.PageSize), 2 * runtime_internal_test_package.PageSize),
                new(3, 0, 0),
                new(1, @base + (uintptr)(63 * runtime_internal_test_package.PageSize), 1 * runtime_internal_test_package.PageSize),
                new(4, 0, 0),
                new(2, 0, 0),
                new(62, 0, 0),
                new(1, 0, 0)
            }.slice()
        )
    };
    foreach (var (name, test) in tests) {
        ref var testΔ1 = ref heap<TestPageCacheAlloc_tests>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            ref var c = ref heap<global::go.runtime_internal_test_package.PageCache>(out var Ꮡc);
            c = testʗ1.cache;
            foreach (var (i, h) in testʗ1.hits) {
                var (b, s) = Ꮡc.Alloc(h.npages);
                if (b != h.@base) {
                    tΔ1.Fatalf("bad alloc base #%d: got 0x%x, want 0x%x"u8, i, b, h.@base);
                }
                if (s != h.scav) {
                    tΔ1.Fatalf("bad alloc scav #%d: got %d, want %d"u8, i, s, h.scav);
                }
            }
        });
    }
}

public static void TestPageCacheFlush(ж<testing.T> Ꮡt) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    slice<global::go.runtime_internal_test_package.BitRange> bits64ToBitRanges(uint64 bits, nuint @base) {
        slice<global::go.runtime_internal_test_package.BitRange> ranges = default!;
        nuint start = (nuint)0;
        nuint size = (nuint)0;
        for (nint i = 0; i < 64; i++) {
            if ((uint64)(bits & (((uint64)1).Lsh((int64)(i)))) != 0){
                if (size == 0) {
                    start = (nuint)i + @base;
                }
                size++;
            } else {
                if (size != 0) {
                    ranges = append(ranges, new runtime_internal_test_package.BitRange(start, size));
                    size = 0;
                }
            }
        }
        if (size != 0) {
            ranges = append(ranges, new runtime_internal_test_package.BitRange(start, size));
        }
        return ranges;
    }
    var bits64ToBitRangesʗ1 = bits64ToBitRanges;
    void runTest(ж<testing.T> tΔ1, nuint @base, uint64 cache, uint64 scav) {
        GoFrame ᒐ = default;
        try {
            // Set up the before state.
            var beforeAlloc = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(@base, 64)}.slice()
            };
            var beforeScav = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            };
            var b = runtime_internal_test_package.NewPageAlloc(beforeAlloc, beforeScav);
            defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
            // Create and flush the cache.
            ref var c = ref heap<global::go.runtime_internal_test_package.PageCache>(out var Ꮡc);
            c = runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, @base), cache, scav);
            Ꮡc.Flush(b);
            if (!Ꮡc.Empty()) {
                tΔ1.Errorf("pageCache flush did not clear cache"u8);
            }
            // Set up the expected after state.
            var afterAlloc = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = bits64ToBitRangesʗ1(~cache, @base)
            };
            var afterScav = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = bits64ToBitRangesʗ1(scav, @base)
            };
            var want = runtime_internal_test_package.NewPageAlloc(afterAlloc, afterScav);
            defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);
            // Check to see if it worked.
            checkPageAlloc(tΔ1, want, b);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    // Empty.
    runTest(Ꮡt, 0, 0, 0);
    // Full.
    runTest(Ꮡt, 0, ~(uint64)0, ~(uint64)0);
    // Random.
    for (nint i = 0; i < 100; i++) {
        // Generate random valid base within a chunk.
        nuint @base = (nuint)rand.Intn(runtime_internal_test_package.PallocChunkPages / 64) * 64;
        // Generate random cache.
        var cache = rand.Uint64();
        var scav = (uint64)(rand.Uint64() & cache);
        // Run the test.
        runTest(Ꮡt, @base, cache, scav);
    }
}

[GoType("dyn")] internal partial struct TestPageAllocAllocToCache_test {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> beforeAlloc;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> beforeScav;
    internal slice<global::go.runtime_internal_test_package.PageCache> hits; // expected base addresses and patterns
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> afterAlloc;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> afterScav;
}

public static void TestPageAllocAllocToCache(ж<testing.T> Ꮡt) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocAllocToCache_test>{
        ["AllFree"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(1, 1), new(64, 64)}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), ~(uint64)0, 0x2),
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 64), ~(uint64)0, ~(uint64)0),
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 128), ~(uint64)0, 0),
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 192), ~(uint64)0, 0)
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 256)}.slice()
            }
        ),
        ["ManyArena"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages - 64)}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages - 64), ~(uint64)0, 0)
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["NotContiguous"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 0)}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(31, 67)}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0xff, 0), ~(uint64)0, (((((uint64)1 << (int)(33))) - 1) << (int)(31)))
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 64)}.slice()
            },
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(64, 34)}.slice()
            }
        ),
        ["First"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 32), new(33, 31), new(96, 32)}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(1, 4), new(31, 5), new(66, 2)}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), ((uint64)1 << (int)(32)), ((uint64)1 << (int)(32))),
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 64), (((uint64)1 << (int)(32))) - 1, ((uint64)0x3 << (int)(2)))
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 128)}.slice()
            }
        ),
        ["Fail"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(0, 0, 0),
                runtime_internal_test_package.NewPageCache(0, 0, 0),
                runtime_internal_test_package.NewPageCache(0, 0, 0)
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["RetainScavBits"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 1), new(10, 2)}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 4), new(11, 1)}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), ~(uint64)((uint64)(0x1 | ((0x3 << (int)(10))))), ((uint64)0x7 << (int)(1)))
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 64)}.slice()
            },
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 1), new(11, 1)}.slice()
            }
        )
    };
    // Disable these tests on iOS since we have a small address space.
    // See #46860.
    if (runtime_internal_test_package.PageAlloc64Bit != 0 && goos.IsIos == 0) {
        UntypedInt chunkIdxBigJump = 0x100000; // chunk index offset which translates to O(TiB)
        // This test is similar to the one with the same name for
        // pageAlloc.alloc and serves the same purpose.
        // See mpagealloc_test.go for details.
        global::go.runtime_internal_test_package.ChunkIdx sumsPerPhysPage = ((global::go.runtime_internal_test_package.ChunkIdx)(nuint)(runtime_internal_test_package.PhysPageSize / runtime_internal_test_package.PallocSumBytes));
        global::go.runtime_internal_test_package.ChunkIdx baseChunkIdx = (global::go.runtime_internal_test_package.ChunkIdx)(runtime_internal_test_package.BaseChunkIdx & ~(sumsPerPhysPage - 1));
        tests["DiscontiguousMappedSumBoundary"u8] = new TestPageAllocAllocToCache_test(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages - 1)}.slice(),
                [baseChunkIdx + (nuint)chunkIdxBigJump] = new global::go.runtime_internal_test_package.BitRange[]{new(1, runtime_internal_test_package.PallocChunkPages - 1)}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [baseChunkIdx + (nuint)chunkIdxBigJump] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new global::go.runtime_internal_test_package.PageCache[]{
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(baseChunkIdx + sumsPerPhysPage - 1, runtime_internal_test_package.PallocChunkPages - 64), ((uint64)1 << (int)(63)), 0),
                runtime_internal_test_package.NewPageCache(runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)chunkIdxBigJump, 0), 1, 0),
                runtime_internal_test_package.NewPageCache(0, 0, 0)
            }.slice(),
            afterAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [baseChunkIdx + (nuint)chunkIdxBigJump] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        );
    }
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPageAllocAllocToCache_test>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.beforeAlloc, vʗ1.beforeScav);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                foreach (var (_, expect) in vʗ1.hits) {
                    checkPageCache(tΔ1, b.AllocToCache(), expect);
                    if (tΔ1.Failed()) {
                        return;
                    }
                }
                var want = runtime_internal_test_package.NewPageAlloc(vʗ1.afterAlloc, vʗ1.afterScav);
                defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);
                checkPageAlloc(tΔ1, want, b);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end runtime_test_package
