// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using goos = @internal.goos_package;
using static runtime_package;
using testing = testing_package;
using @internal;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

internal static void checkPageAlloc(ж<testing.T> Ꮡt, ж<global::go.runtime_internal_test_package.PageAlloc> Ꮡwant, ж<global::go.runtime_internal_test_package.PageAlloc> Ꮡgot) {
    ref var want = ref Ꮡwant.DerefOrNull();
    ref var got = ref Ꮡgot.DerefOrNull();

    // Ensure start and end are correct.
    var (wantStart, wantEnd) = Ꮡwant.Bounds();
    var (gotStart, gotEnd) = Ꮡgot.Bounds();
    if (gotStart != wantStart) {
        Ꮡt.Fatalf("start values not equal: got %d, want %d"u8, gotStart, wantStart);
    }
    if (gotEnd != wantEnd) {
        Ꮡt.Fatalf("end values not equal: got %d, want %d"u8, gotEnd, wantEnd);
    }
    for (global::go.runtime_internal_test_package.ChunkIdx i = gotStart; i < gotEnd; i++) {
        // Check the bitmaps. Note that we may have nil data.
        var (gb, wb) = (Ꮡgot.PallocData(i), Ꮡwant.PallocData(i));
        if (gb == nil && wb == nil) {
            continue;
        }
        if ((gb == nil && wb != nil) || (gb != nil && wb == nil)) {
            Ꮡt.Errorf("chunk %d nilness mismatch"u8, i);
        }
        if (!checkPallocBits(Ꮡt, gb.PallocBits(), wb.PallocBits())) {
            Ꮡt.Logf("in chunk %d (mallocBits)"u8, i);
        }
        if (!checkPallocBits(Ꮡt, gb.Scavenged(), wb.Scavenged())) {
            Ꮡt.Logf("in chunk %d (scavenged)"u8, i);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string extremelyDiscontiguousˢ = "ExtremelyDiscontiguous"u8;

internal partial struct TestPageAllocGrow_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.ChunkIdx> chunks;
    internal slice<global::go.runtime_internal_test_package.AddrRange> inUse;
}

// TODO(mknyszek): Verify summaries too?
public static void TestPageAllocGrow(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocGrow_test>{
        ["One"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0))
            }.slice()
        ),
        ["Contiguous2"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0))
            }.slice()
        ),
        ["Contiguous5"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1,
                runtime_internal_test_package.BaseChunkIdx + 2,
                runtime_internal_test_package.BaseChunkIdx + 3,
                runtime_internal_test_package.BaseChunkIdx + 4
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0))
            }.slice()
        ),
        ["Discontiguous"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 2,
                runtime_internal_test_package.BaseChunkIdx + 4
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 3, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 4, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0))
            }.slice()
        ),
        ["Mixed"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1,
                runtime_internal_test_package.BaseChunkIdx + 2,
                runtime_internal_test_package.BaseChunkIdx + 4
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 3, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 4, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0))
            }.slice()
        ),
        ["WildlyDiscontiguous"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1,
                runtime_internal_test_package.BaseChunkIdx + 0x10,
                runtime_internal_test_package.BaseChunkIdx + 0x21
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x10, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x11, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x21, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x22, 0))
            }.slice()
        ),
        ["ManyDiscontiguous"u8] = new(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{ // The initial cap is 16. Test 33 ranges, to exercise the growth path (twice).

                runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.BaseChunkIdx + 4, runtime_internal_test_package.BaseChunkIdx + 6,
                runtime_internal_test_package.BaseChunkIdx + 8, runtime_internal_test_package.BaseChunkIdx + 10, runtime_internal_test_package.BaseChunkIdx + 12, runtime_internal_test_package.BaseChunkIdx + 14,
                runtime_internal_test_package.BaseChunkIdx + 16, runtime_internal_test_package.BaseChunkIdx + 18, runtime_internal_test_package.BaseChunkIdx + 20, runtime_internal_test_package.BaseChunkIdx + 22,
                runtime_internal_test_package.BaseChunkIdx + 24, runtime_internal_test_package.BaseChunkIdx + 26, runtime_internal_test_package.BaseChunkIdx + 28, runtime_internal_test_package.BaseChunkIdx + 30,
                runtime_internal_test_package.BaseChunkIdx + 32, runtime_internal_test_package.BaseChunkIdx + 34, runtime_internal_test_package.BaseChunkIdx + 36, runtime_internal_test_package.BaseChunkIdx + 38,
                runtime_internal_test_package.BaseChunkIdx + 40, runtime_internal_test_package.BaseChunkIdx + 42, runtime_internal_test_package.BaseChunkIdx + 44, runtime_internal_test_package.BaseChunkIdx + 46,
                runtime_internal_test_package.BaseChunkIdx + 48, runtime_internal_test_package.BaseChunkIdx + 50, runtime_internal_test_package.BaseChunkIdx + 52, runtime_internal_test_package.BaseChunkIdx + 54,
                runtime_internal_test_package.BaseChunkIdx + 56, runtime_internal_test_package.BaseChunkIdx + 58, runtime_internal_test_package.BaseChunkIdx + 60, runtime_internal_test_package.BaseChunkIdx + 62,
                runtime_internal_test_package.BaseChunkIdx + 64
            }.slice(),
            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 3, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 4, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 6, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 7, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 8, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 9, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 10, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 11, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 12, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 13, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 14, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 15, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 16, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 17, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 18, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 19, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 20, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 21, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 22, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 23, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 24, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 25, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 26, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 27, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 28, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 29, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 30, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 31, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 32, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 33, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 34, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 35, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 36, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 37, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 38, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 39, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 40, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 41, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 42, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 43, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 44, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 45, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 46, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 47, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 48, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 49, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 50, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 51, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 52, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 53, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 54, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 55, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 56, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 57, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 58, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 59, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 60, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 61, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 62, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 63, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 64, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 65, 0))
            }.slice()
        )
    };
    // Disable these tests on iOS since we have a small address space.
    // See #46860.
    if (runtime_internal_test_package.PageAlloc64Bit != 0 && goos.IsIos == 0) {
        tests[extremelyDiscontiguousˢ] = new TestPageAllocGrow_test(
            chunks: new global::go.runtime_internal_test_package.ChunkIdx[]{
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 0x100000
            }.slice(), // constant translates to O(TiB)

            inUse: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x100000, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x100001, 0))
            }.slice()
        );
    }
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPageAllocGrow_test>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                // By creating a new pageAlloc, we will
                // grow it for each chunk defined in x.
                var x = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>();
                foreach (var (_, c) in vʗ1.chunks) {
                    x[c] = new global::go.runtime_internal_test_package.BitRange[]{}.slice();
                }
                var b = runtime_internal_test_package.NewPageAlloc(x, default!);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                var got = b.InUse();
                var want = vʗ1.inUse;
                // Check for mismatches.
                if (len(got) != len(want)){
                    tΔ1.Fail();
                } else {
                    foreach (var (i, _) in want) {
                        if (!want[i].ΔEquals(got[i])) {
                            tΔ1.Fail();
                            break;
                        }
                    }
                }
                if (tΔ1.Failed()) {
                    tΔ1.Logf("found inUse mismatch"u8);
                    tΔ1.Logf("got:"u8);
                    foreach (var (i, r) in got) {
                        tΔ1.Logf("\t#%d [0x%x, 0x%x)"u8, i, r.Base(), r.Limit());
                    }
                    tΔ1.Logf("want:"u8);
                    foreach (var (i, r) in want) {
                        tΔ1.Logf("\t#%d [0x%x, 0x%x)"u8, i, r.Base(), r.Limit());
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestPageAllocAlloc_hit /*dyn*/ {
    internal uintptr npages, @base, scav;
}

internal partial struct TestPageAllocAlloc_test /*dyn*/ {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> scav;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> before;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> after;
    internal slice<TestPageAllocAlloc_hit> hits;
}

public static void TestPageAllocAlloc(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocAlloc_test>{
        ["AllFree1"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 1), new(2, 2)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 1), 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2), runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 3), runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4), 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 5)}.slice()
            }
        ),
        ["ManyArena1"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages - 1)}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages - 1), runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["NotContiguous1"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 0)}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0xff, 0), runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 1)}.slice()
            }
        ),
        ["AllFree2"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 3), new(7, 1)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 2 * runtime_internal_test_package.PageSize),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2), runtime_internal_test_package.PageSize),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4), 0),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 6), runtime_internal_test_package.PageSize),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 8), 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 10)}.slice()
            }
        ),
        ["Straddle2"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages - 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(1, runtime_internal_test_package.PallocChunkPages - 1)}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages - 1, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1), runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["AllFree5"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 8), new(9, 1), new(17, 5)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 5 * runtime_internal_test_package.PageSize),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 5), 4 * runtime_internal_test_package.PageSize),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 10), 0),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 15), 3 * runtime_internal_test_package.PageSize),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 20), 2 * runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 25)}.slice()
            }
        ),
        ["AllFree64"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(21, 1), new(63, 65)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(64, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 2 * runtime_internal_test_package.PageSize),
                new(64, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 64), 64 * runtime_internal_test_package.PageSize),
                new(64, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 128), 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 192)}.slice()
            }
        ),
        ["AllFree65"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(129, 1)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(65, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 0),
                new(65, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 65), runtime_internal_test_package.PageSize),
                new(65, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 130), 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 195)}.slice()
            }
        ),
        ["ExhaustPallocChunkPages-3"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(10, 1)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages - 3, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages - 3, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 3), 0),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 2), 0),
                new(1, 0, 0),
                new(runtime_internal_test_package.PallocChunkPages - 3, 0, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["AllFreePallocChunkPages"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 1), new(runtime_internal_test_package.PallocChunkPages - 1, 1)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 2 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages, 0, 0),
                new(1, 0, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["StraddlePallocChunkPages"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(3, 100)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2), 100 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages, 0, 0),
                new(1, 0, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["StraddlePallocChunkPages+1"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages + 1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2), (runtime_internal_test_package.PallocChunkPages + 1) * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, runtime_internal_test_package.PallocChunkPages / 2 + 1), runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2 + 2)}.slice()
            }
        ),
        ["AllFreePallocChunkPages*2"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages * 2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 0),
                new(runtime_internal_test_package.PallocChunkPages * 2, 0, 0),
                new(1, 0, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["NotContiguousPallocChunkPages*2"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x40] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x41] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x40] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x41] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages * 2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x40, 0), 0),
                new(21, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 21 * runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 21), runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 22)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x40] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x41] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["StraddlePallocChunkPages*2"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 7)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(3, 5), new(121, 10)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2 + 12, 2)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages * 2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2), 15 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages * 2, 0, 0),
                new(1, 0, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["StraddlePallocChunkPages*5/4"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages * 3 / 4)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages * 3 / 4)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 0)}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 4 + 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 3, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages * 2 / 3, 1)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages * 5 / 4, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages * 3 / 4), runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages * 5 / 4, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, runtime_internal_test_package.PallocChunkPages * 3 / 4), runtime_internal_test_package.PageSize)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages * 3 / 4 + 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["AllFreePallocChunkPages*7+5"u8] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 4] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 5] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 6] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 7] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(50, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(31, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(7, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{new(200, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 4] = new global::go.runtime_internal_test_package.BitRange[]{new(3, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 5] = new global::go.runtime_internal_test_package.BitRange[]{new(51, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 6] = new global::go.runtime_internal_test_package.BitRange[]{new(20, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 7] = new global::go.runtime_internal_test_package.BitRange[]{new(1, 1)}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages * 7 + 5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 8 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages * 7 + 5, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 7, 5), 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 4] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 5] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 6] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 7] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 6)}.slice()
            }
        )
    };
    // Disable these tests on iOS since we have a small address space.
    // See #46860.
    if (runtime_internal_test_package.PageAlloc64Bit != 0 && goos.IsIos == 0) {
        UntypedInt chunkIdxBigJump = 0x100000; // chunk index offset which translates to O(TiB)
        // This test attempts to trigger a bug wherein we look at unmapped summary
        // memory that isn't just in the case where we exhaust the heap.
        //
        // It achieves this by placing a chunk such that its summary will be
        // at the very end of a physical page. It then also places another chunk
        // much further up in the address space, such that any allocations into the
        // first chunk do not exhaust the heap and the second chunk's summary is not in the
        // page immediately adjacent to the first chunk's summary's page.
        // Allocating into this first chunk to exhaustion and then into the second
        // chunk may then trigger a check in the allocator which erroneously looks at
        // unmapped summary memory and crashes.
        // Figure out how many chunks are in a physical page, then align BaseChunkIdx
        // to a physical page in the chunk summary array. Here we only assume that
        // each summary array is aligned to some physical page.
        global::go.runtime_internal_test_package.ChunkIdx sumsPerPhysPage = ((global::go.runtime_internal_test_package.ChunkIdx)(nuint)(runtime_internal_test_package.PhysPageSize / runtime_internal_test_package.PallocSumBytes));
        global::go.runtime_internal_test_package.ChunkIdx baseChunkIdx = (global::go.runtime_internal_test_package.ChunkIdx)(runtime_internal_test_package.BaseChunkIdx & ~(sumsPerPhysPage - 1));
        tests["DiscontiguousMappedSumBoundary"u8] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [baseChunkIdx + (nuint)chunkIdxBigJump] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [baseChunkIdx + (nuint)chunkIdxBigJump] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{
                new(runtime_internal_test_package.PallocChunkPages - 1, runtime_internal_test_package.PageBase(baseChunkIdx + sumsPerPhysPage - 1, 0), 0),
                new(1, runtime_internal_test_package.PageBase(baseChunkIdx + sumsPerPhysPage - 1, runtime_internal_test_package.PallocChunkPages - 1), 0),
                new(1, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)chunkIdxBigJump, 0), 0),
                new(runtime_internal_test_package.PallocChunkPages - 1, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)chunkIdxBigJump, 1), 0),
                new(1, 0, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [baseChunkIdx + (nuint)chunkIdxBigJump] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        );
        // Test to check for issue #40191. Essentially, the candidate searchAddr
        // discovered by find may not point to mapped memory, so we need to handle
        // that explicitly.
        //
        // chunkIdxSmallOffset is an offset intended to be used within chunkIdxBigJump.
        // It is far enough within chunkIdxBigJump that the summaries at the beginning
        // of an address range the size of chunkIdxBigJump will not be mapped in.
        UntypedInt chunkIdxSmallOffset = 0x503;
        tests["DiscontiguousBadSearchAddr"u8] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{ // The mechanism for the bug involves three chunks, A, B, and C, which are
 // far apart in the address space. In particular, B is chunkIdxBigJump +
 // chunkIdxSmalloffset chunks away from B, and C is 2*chunkIdxBigJump chunks
 // away from A. A has 1 page free, B has several (NOT at the end of B), and
 // C is totally free.
 // Note that B's free memory must not be at the end of B because the fast
 // path in the page allocator will check if the searchAddr even gives us
 // enough space to place the allocation in a chunk before accessing the
 // summary.

                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 0)] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages - 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset] = new global::go.runtime_internal_test_package.BitRange[]{
                    new(0, runtime_internal_test_package.PallocChunkPages - 10),
                    new(runtime_internal_test_package.PallocChunkPages - 1, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 2)] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 0)] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 2)] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new TestPageAllocAlloc_hit[]{ // We first allocate into A to set the page allocator's searchAddr to the
 // end of that chunk. That is the only purpose A serves.

                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1), 0), // Then, we make a big allocation that doesn't fit into B, and so must be
 // fulfilled by C.
 //
 // On the way to fulfilling the allocation into C, we estimate searchAddr
 // using the summary structure, but that will give us a searchAddr of
 // B's base address minus chunkIdxSmallOffset chunks. These chunks will
 // not be mapped.

                new(100, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)(chunkIdxBigJump * 2), 0), 0), // Now we try to make a smaller allocation that can be fulfilled by B.
 // In an older implementation of the page allocator, this will segfault,
 // because this last allocation will first try to access the summary
 // for B's base address minus chunkIdxSmallOffset chunks in the fast path,
 // and this will not be mapped.

                new(9, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset, runtime_internal_test_package.PallocChunkPages - 10), 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 0)] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 2)] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 100)}.slice()
            }
        );
    }
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPageAllocAlloc_test>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.before, vʗ1.scav);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                foreach (var (iter, i) in vʗ1.hits) {
                    var (a, s) = b.Alloc(i.npages);
                    if (a != i.@base) {
                        tΔ1.Fatalf("bad alloc #%d: want base 0x%x, got 0x%x"u8, iter + 1, i.@base, a);
                    }
                    if (s != i.scav) {
                        tΔ1.Fatalf("bad alloc #%d: want scav %d, got %d"u8, iter + 1, i.scav, s);
                    }
                }
                var want = runtime_internal_test_package.NewPageAlloc(vʗ1.after, vʗ1.scav);
                defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);
                checkPageAlloc(tΔ1, want, b);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestPageAllocExhaust(ж<testing.T> Ꮡt) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    foreach (var (_, npages) in new uintptr[]{1, 2, 3, 4, 5, 8, 16, 64, 1024, 1025, 2048, 2049}.slice()) {
        var npagesΔ1 = npages;
        Ꮡt.Run(fmt.Sprintf("%d"u8, npagesΔ1), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                // Construct b.
                var bDesc = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>();
                for (global::go.runtime_internal_test_package.ChunkIdx i = ((global::go.runtime_internal_test_package.ChunkIdx)0); i < 4; i++) {
                    bDesc[runtime_internal_test_package.BaseChunkIdx + i] = new global::go.runtime_internal_test_package.BitRange[]{}.slice();
                }
                var b = runtime_internal_test_package.NewPageAlloc(bDesc, default!);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                // Allocate into b with npages until we've exhausted the heap.
                nint nAlloc = (nint)(runtime_internal_test_package.PallocChunkPages * 4) / (nint)npagesΔ1;
                for (nint i = 0; i < nAlloc; i++) {
                    var addr = runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, (nuint)i * (nuint)npagesΔ1);
                    {
                        var (a, _) = b.Alloc(npagesΔ1); if (a != addr) {
                            tΔ1.Fatalf("bad alloc #%d: want 0x%x, got 0x%x"u8, i + 1, addr, a);
                        }
                    }
                }
                // Check to make sure the next allocation fails.
                {
                    var (a, _) = b.Alloc(npagesΔ1); if (a != 0) {
                        tΔ1.Fatalf("bad alloc #%d: want 0, got 0x%x"u8, nAlloc, a);
                    }
                }
                // Construct what we want the heap to look like now.
                nint allocPages = nAlloc * (nint)npagesΔ1;
                var wantDesc = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>();
                for (global::go.runtime_internal_test_package.ChunkIdx i = ((global::go.runtime_internal_test_package.ChunkIdx)0); i < 4; i++) {
                    if (allocPages >= runtime_internal_test_package.PallocChunkPages){
                        wantDesc[runtime_internal_test_package.BaseChunkIdx + i] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice();
                        allocPages -= runtime_internal_test_package.PallocChunkPages;
                    } else 
                    if (allocPages > 0){
                        wantDesc[runtime_internal_test_package.BaseChunkIdx + i] = new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)allocPages)}.slice();
                        allocPages = 0;
                    } else {
                        wantDesc[runtime_internal_test_package.BaseChunkIdx + i] = new global::go.runtime_internal_test_package.BitRange[]{}.slice();
                    }
                }
                var want = runtime_internal_test_package.NewPageAlloc(wantDesc, default!);
                defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);
                // Check to make sure the heap b matches what we want.
                checkPageAlloc(tΔ1, want, b);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestPageAllocFree_tests /*dyn*/ {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> before;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> after;
    internal uintptr npages;
    internal slice<uintptr> frees;
}

public static void TestPageAllocFree(ж<testing.T> Ꮡt) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocFree_tests>{
        ["Free1"u8] = new(
            npages: 1,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 1),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 3),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(5, runtime_internal_test_package.PallocChunkPages - 5)}.slice()
            }
        ),
        ["ManyArena1"u8] = new(
            npages: 1,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages - 1)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2), new(runtime_internal_test_package.PallocChunkPages / 2 + 1, runtime_internal_test_package.PallocChunkPages / 2 - 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(1, runtime_internal_test_package.PallocChunkPages - 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages - 1)}.slice()
            }
        ),
        ["Free2"u8] = new(
            npages: 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 6),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 8)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(10, runtime_internal_test_package.PallocChunkPages - 10)}.slice()
            }
        ),
        ["Straddle2"u8] = new(
            npages: 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages - 1, 1)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, 1)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            }
        ),
        ["Free5"u8] = new(
            npages: 5,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 5),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 10),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 15),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 20)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(25, runtime_internal_test_package.PallocChunkPages - 25)}.slice()
            }
        ),
        ["Free64"u8] = new(
            npages: 64,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 64),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 128)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(192, runtime_internal_test_package.PallocChunkPages - 192)}.slice()
            }
        ),
        ["Free65"u8] = new(
            npages: 65,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 65),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 130)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(195, runtime_internal_test_package.PallocChunkPages - 195)}.slice()
            }
        ),
        ["FreePallocChunkPages"u8] = new(
            npages: runtime_internal_test_package.PallocChunkPages,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            }
        ),
        ["StraddlePallocChunkPages"u8] = new(
            npages: runtime_internal_test_package.PallocChunkPages,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            }
        ),
        ["StraddlePallocChunkPages+1"u8] = new(
            npages: runtime_internal_test_package.PallocChunkPages + 1,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2 + 1, runtime_internal_test_package.PallocChunkPages / 2 - 1)}.slice()
            }
        ),
        ["FreePallocChunkPages*2"u8] = new(
            npages: runtime_internal_test_package.PallocChunkPages * 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            }
        ),
        ["StraddlePallocChunkPages*2"u8] = new(
            npages: runtime_internal_test_package.PallocChunkPages * 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)}.slice()
            }
        ),
        ["AllFreePallocChunkPages*7+5"u8] = new(
            npages: runtime_internal_test_package.PallocChunkPages * 7 + 5,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 4] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 5] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 6] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 7] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            frees: new uintptr[]{
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)
            }.slice(),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 4] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 5] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 6] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 7] = new global::go.runtime_internal_test_package.BitRange[]{new(5, runtime_internal_test_package.PallocChunkPages - 5)}.slice()
            }
        )
    };
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPageAllocFree_tests>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.before, default!);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                foreach (var (_, addr) in vʗ1.frees) {
                    b.Free(addr, vʗ1.npages);
                }
                var want = runtime_internal_test_package.NewPageAlloc(vʗ1.after, default!);
                defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);
                checkPageAlloc(tΔ1, want, b);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestPageAllocAllocAndFree_hit /*dyn*/ {
    internal bool alloc;
    internal uintptr npages;
    internal uintptr @base;
}

internal partial struct TestPageAllocAllocAndFree_tests /*dyn*/ {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> init;
    internal slice<TestPageAllocAllocAndFree_hit> hits;
}

public static void TestPageAllocAllocAndFree(ж<testing.T> Ꮡt) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocAllocAndFree_tests>{ // TODO(mknyszek): Write more tests here.

        ["Chunks8"u8] = new(
            init: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 3] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 4] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 5] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 6] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 7] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            hits: new TestPageAllocAllocAndFree_hit[]{
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, 1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, 1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0))
            }.slice()
        )
    };
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPageAllocAllocAndFree_tests>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.init, default!);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                foreach (var (iter, i) in vʗ1.hits) {
                    if (i.alloc){
                        {
                            var (a, _) = b.Alloc(i.npages); if (a != i.@base) {
                                tΔ1.Fatalf("bad alloc #%d: want 0x%x, got 0x%x"u8, iter + 1, i.@base, a);
                            }
                        }
                    } else {
                        b.Free(i.@base, i.npages);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

} // end runtime_test_package
