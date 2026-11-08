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

internal static void checkPageAlloc(ж<testing.T> t, ж<global::go.runtime_internal_test_package.PageAlloc> want, ж<global::go.runtime_internal_test_package.PageAlloc> got) {
    // Ensure start and end are correct.
    var (wantStart, wantEnd) = want.Bounds();
    var (gotStart, gotEnd) = got.Bounds();
    if (gotStart != wantStart) {
        t.Fatalf("start values not equal: got %d, want %d", gotStart, wantStart);
    }
    if (gotEnd != wantEnd) {
        t.Fatalf("end values not equal: got %d, want %d", gotEnd, wantEnd);
    }

    for (global::go.runtime_internal_test_package.ChunkIdx i = gotStart; i < gotEnd; i++) {
        // Check the bitmaps. Note that we may have nil data.
        var (gb, wb) = (got.PallocData(i), want.PallocData(i));
        if (gb == nil && wb == nil) {
            continue;
        }
        if ((gb == nil && wb != nil) || (gb != nil && wb == nil)) {
            t.Errorf("chunk %d nilness mismatch", i);
        }
        if (!checkPallocBits(t, gb.PallocBits(), wb.PallocBits())) {
            t.Logf("in chunk %d (mallocBits)", i);
        }
        if (!checkPallocBits(t, gb.Scavenged(), wb.Scavenged())) {
            t.Logf("in chunk %d (scavenged)", i);
        }
    }
    // TODO(mknyszek): Verify summaries too?
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string extremelyDiscontiguousˢ => ref extremelyDiscontiguousˢᵇ;
private static readonly @string extremelyDiscontiguousˢᵇ = "ExtremelyDiscontiguous"u8;

internal partial struct TestPageAllocGrow_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.ChunkIdx> chunks;
    internal slice<global::go.runtime_internal_test_package.AddrRange> inUse;
}

public static void TestPageAllocGrow(ж<testing.T> t) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        t.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocGrow_test>{
        ["One"] = new(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0))
            ])
        ),
        ["Contiguous2"] = new(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0))
            ])
        ),
        ["Contiguous5"] = new(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1,
                runtime_internal_test_package.BaseChunkIdx + 2,
                runtime_internal_test_package.BaseChunkIdx + 3,
                runtime_internal_test_package.BaseChunkIdx + 4
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0))
            ])
        ),
        ["Discontiguous"] = new(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 2,
                runtime_internal_test_package.BaseChunkIdx + 4
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 3, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 4, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0))
            ])
        ),
        ["Mixed"] = new(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1,
                runtime_internal_test_package.BaseChunkIdx + 2,
                runtime_internal_test_package.BaseChunkIdx + 4
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 3, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 4, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0))
            ])
        ),
        ["WildlyDiscontiguous"] = new(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 1,
                runtime_internal_test_package.BaseChunkIdx + 0x10,
                runtime_internal_test_package.BaseChunkIdx + 0x21
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x10, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x11, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x21, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x22, 0))
            ])
        ),
        ["ManyDiscontiguous"] = new(
            // The initial cap is 16. Test 33 ranges, to exercise the growth path (twice).
            chunks: [
                runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.BaseChunkIdx + 4, runtime_internal_test_package.BaseChunkIdx + 6,
                runtime_internal_test_package.BaseChunkIdx + 8, runtime_internal_test_package.BaseChunkIdx + 10, runtime_internal_test_package.BaseChunkIdx + 12, runtime_internal_test_package.BaseChunkIdx + 14,
                runtime_internal_test_package.BaseChunkIdx + 16, runtime_internal_test_package.BaseChunkIdx + 18, runtime_internal_test_package.BaseChunkIdx + 20, runtime_internal_test_package.BaseChunkIdx + 22,
                runtime_internal_test_package.BaseChunkIdx + 24, runtime_internal_test_package.BaseChunkIdx + 26, runtime_internal_test_package.BaseChunkIdx + 28, runtime_internal_test_package.BaseChunkIdx + 30,
                runtime_internal_test_package.BaseChunkIdx + 32, runtime_internal_test_package.BaseChunkIdx + 34, runtime_internal_test_package.BaseChunkIdx + 36, runtime_internal_test_package.BaseChunkIdx + 38,
                runtime_internal_test_package.BaseChunkIdx + 40, runtime_internal_test_package.BaseChunkIdx + 42, runtime_internal_test_package.BaseChunkIdx + 44, runtime_internal_test_package.BaseChunkIdx + 46,
                runtime_internal_test_package.BaseChunkIdx + 48, runtime_internal_test_package.BaseChunkIdx + 50, runtime_internal_test_package.BaseChunkIdx + 52, runtime_internal_test_package.BaseChunkIdx + 54,
                runtime_internal_test_package.BaseChunkIdx + 56, runtime_internal_test_package.BaseChunkIdx + 58, runtime_internal_test_package.BaseChunkIdx + 60, runtime_internal_test_package.BaseChunkIdx + 62,
                runtime_internal_test_package.BaseChunkIdx + 64
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
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
            ])
        )
    };
    // Disable these tests on iOS since we have a small address space.
    // See #46860.
    if (runtime_internal_test_package.PageAlloc64Bit != 0 && goos.IsIos == 0) {
        tests[extremelyDiscontiguousˢ] = new TestPageAllocGrow_test(
            chunks: [
                runtime_internal_test_package.BaseChunkIdx,
                runtime_internal_test_package.BaseChunkIdx + 0x100000 // constant translates to O(TiB)
            ],
            inUse: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0)),
                runtime_internal_test_package.MakeAddrRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x100000, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x100001, 0))
            ])
        );
    }
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPageAllocGrow_test(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                // By creating a new pageAlloc, we will
                // grow it for each chunk defined in x.
                var x = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>();
                foreach (var (_, c) in range(vʗ1.chunks)) {
                    x[c] = [];
                }
                var b = runtime_internal_test_package.NewPageAlloc(x, default);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);

                var got = b.InUse();
                var want = vʗ1.inUse;

                // Check for mismatches.
                if (len(got) != len(want)) {
                    tΔ1.Fail();
                } else {
                    foreach (var i in range(len(want))) {
                        if (!want[i].ΔEquals(got[i])) {
                            tΔ1.Fail();
                            break;
                        }
                    }
                }
                if (tΔ1.Failed()) {
                    tΔ1.Logf("found inUse mismatch");
                    tΔ1.Logf("got:");
                    foreach (var (i, r) in range(got)) {
                        tΔ1.Logf("\t#%d [0x%x, 0x%x)", i, r.Base(), r.Limit());
                    }
                    tΔ1.Logf("want:");
                    foreach (var (i, r) in range(want)) {
                        tΔ1.Logf("\t#%d [0x%x, 0x%x)", i, r.Base(), r.Limit());
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string allFree64ˢ => ref allFree64ˢᵇ;
private static readonly @string allFree64ˢᵇ = "AllFree64"u8;
internal static ref readonly @string allFree65ˢ => ref allFree65ˢᵇ;
private static readonly @string allFree65ˢᵇ = "AllFree65"u8;
internal static ref readonly @string straddlePallocChunkPagesˢ => ref straddlePallocChunkPagesˢᵇ;
private static readonly @string straddlePallocChunkPagesˢᵇ = "StraddlePallocChunkPages"u8;
internal static ref readonly @string allFreePallocChunkPages7ˢ => ref allFreePallocChunkPages7ˢᵇ;
private static readonly @string allFreePallocChunkPages7ˢᵇ = "AllFreePallocChunkPages*7+5"u8;
internal static ref readonly @string straddlePallocChunkPagesˢ2 => ref straddlePallocChunkPagesˢ2ᵇ;
private static readonly @string straddlePallocChunkPagesˢ2ᵇ = "StraddlePallocChunkPages*2"u8;

internal partial struct TestPageAllocAlloc_hit /*dyn*/ {
    internal uintptr npages, @base, scav;
}

internal partial struct TestPageAllocAlloc_test /*dyn*/ {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> scav;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> before;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> after;
    internal slice<TestPageAllocAlloc_hit> hits;
}

public static void TestPageAllocAlloc(ж<testing.T> t) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        t.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocAlloc_test>{
        ["AllFree1"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 1), new(2, 2)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 1), 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2), runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 3), runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4), 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 5)]
            }
        ),
        ["ManyArena1"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages - 1)]
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages - 1), runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        ),
        ["NotContiguous1"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = [new(0, 0)]
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0xff, 0), runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 0xff] = [new(0, 1)]
            }
        ),
        ["AllFree2"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 3), new(7, 1)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 2 * runtime_internal_test_package.PageSize),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2), runtime_internal_test_package.PageSize),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4), 0),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 6), runtime_internal_test_package.PageSize),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 8), 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 10)]
            }
        ),
        ["Straddle2"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages - 1)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(1, runtime_internal_test_package.PallocChunkPages - 1)]
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(runtime_internal_test_package.PallocChunkPages - 1, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1), runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        ),
        ["AllFree5"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 8), new(9, 1), new(17, 5)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 5 * runtime_internal_test_package.PageSize),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 5), 4 * runtime_internal_test_package.PageSize),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 10), 0),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 15), 3 * runtime_internal_test_package.PageSize),
                new(5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 20), 2 * runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 25)]
            }
        ),
        ["ExhaustPallocChunkPages-3"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(10, 1)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages - 3, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages - 3, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 3), 0),
                new(2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 2), 0),
                new(1, 0, 0),
                new(runtime_internal_test_package.PallocChunkPages - 3, 0, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        ),
        ["AllFreePallocChunkPages"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 1), new(runtime_internal_test_package.PallocChunkPages - 1, 1)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 2 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages, 0, 0),
                new(1, 0, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        ),
        ["StraddlePallocChunkPages+1"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages / 2)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages + 1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2), (runtime_internal_test_package.PallocChunkPages + 1) * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, runtime_internal_test_package.PallocChunkPages / 2 + 1), runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages / 2 + 2)]
            }
        ),
        ["AllFreePallocChunkPages*2"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages * 2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 0),
                new(runtime_internal_test_package.PallocChunkPages * 2, 0, 0),
                new(1, 0, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        ),
        ["NotContiguousPallocChunkPages*2"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 0x40] = [],
                [runtime_internal_test_package.BaseChunkIdx + 0x41] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 0x40] = [],
                [runtime_internal_test_package.BaseChunkIdx + 0x41] = []
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages * 2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 0x40, 0), 0),
                new(21, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 21 * runtime_internal_test_package.PageSize),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 21), runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 22)],
                [runtime_internal_test_package.BaseChunkIdx + 0x40] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 0x41] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        ),
        ["StraddlePallocChunkPages*5/4"] = new(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages * 3 / 4)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages * 3 / 4)],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [new(0, 0)]
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 4 + 1)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(runtime_internal_test_package.PallocChunkPages / 3, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [new(runtime_internal_test_package.PallocChunkPages * 2 / 3, 1)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages * 5 / 4, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages * 3 / 4), runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages * 5 / 4, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, runtime_internal_test_package.PallocChunkPages * 3 / 4), runtime_internal_test_package.PageSize)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages * 3 / 4 + 1)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        )
    };
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        tests[allFree64ˢ] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(21, 1), new(63, 65)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(64, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 2 * runtime_internal_test_package.PageSize),
                new(64, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 64), 64 * runtime_internal_test_package.PageSize),
                new(64, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 128), 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 192)]
            }
        );
        tests[allFree65ˢ] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(129, 1)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(65, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 0),
                new(65, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 65), runtime_internal_test_package.PageSize),
                new(65, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 130), 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 195)]
            }
        );
        tests[straddlePallocChunkPagesˢ] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages / 2)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)]
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(3, 100)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2), 100 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages, 0, 0),
                new(1, 0, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        );
        tests[allFreePallocChunkPages7ˢ] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [],
                [runtime_internal_test_package.BaseChunkIdx + 4] = [],
                [runtime_internal_test_package.BaseChunkIdx + 5] = [],
                [runtime_internal_test_package.BaseChunkIdx + 6] = [],
                [runtime_internal_test_package.BaseChunkIdx + 7] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(50, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(31, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(7, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [new(200, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 4] = [new(3, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 5] = [new(51, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 6] = [new(20, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 7] = [new(1, 1)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages * 7 + 5, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), 8 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages * 7 + 5, 0, 0),
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 7, 5), 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 4] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 5] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 6] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 7] = [new(0, 6)]
            }
        );
        tests[straddlePallocChunkPagesˢ2] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages / 2)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)]
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, 7)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(3, 5), new(121, 10)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(runtime_internal_test_package.PallocChunkPages / 2 + 12, 2)]
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages * 2, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2), 15 * runtime_internal_test_package.PageSize),
                new(runtime_internal_test_package.PallocChunkPages * 2, 0, 0),
                new(1, 0, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            }
        );
    }
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
        global::go.runtime_internal_test_package.ChunkIdx sumsPerPhysPage = (global::go.runtime_internal_test_package.ChunkIdx)(nuint)(runtime_internal_test_package.PhysPageSize / runtime_internal_test_package.PallocSumBytes);
        global::go.runtime_internal_test_package.ChunkIdx baseChunkIdx = (global::go.runtime_internal_test_package.ChunkIdx)(runtime_internal_test_package.BaseChunkIdx & ~(sumsPerPhysPage - 1));
        tests["DiscontiguousMappedSumBoundary"] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = [],
                [baseChunkIdx + (nuint)chunkIdxBigJump] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = [],
                [baseChunkIdx + (nuint)chunkIdxBigJump] = []
            },
            hits: slice<TestPageAllocAlloc_hit>([
                new(runtime_internal_test_package.PallocChunkPages - 1, runtime_internal_test_package.PageBase(baseChunkIdx + sumsPerPhysPage - 1, 0), 0),
                new(1, runtime_internal_test_package.PageBase(baseChunkIdx + sumsPerPhysPage - 1, runtime_internal_test_package.PallocChunkPages - 1), 0),
                new(1, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)chunkIdxBigJump, 0), 0),
                new(runtime_internal_test_package.PallocChunkPages - 1, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)chunkIdxBigJump, 1), 0),
                new(1, 0, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [baseChunkIdx + sumsPerPhysPage - 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [baseChunkIdx + (nuint)chunkIdxBigJump] = [new(0, runtime_internal_test_package.PallocChunkPages)]
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
        tests["DiscontiguousBadSearchAddr"] = new TestPageAllocAlloc_test(
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                // The mechanism for the bug involves three chunks, A, B, and C, which are
                // far apart in the address space. In particular, B is chunkIdxBigJump +
                // chunkIdxSmalloffset chunks away from B, and C is 2*chunkIdxBigJump chunks
                // away from A. A has 1 page free, B has several (NOT at the end of B), and
                // C is totally free.
                // Note that B's free memory must not be at the end of B because the fast
                // path in the page allocator will check if the searchAddr even gives us
                // enough space to place the allocation in a chunk before accessing the
                // summary.
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 0)] = [new(0, runtime_internal_test_package.PallocChunkPages - 1)],
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset] = [
                    new(0, runtime_internal_test_package.PallocChunkPages - 10),
                    new(runtime_internal_test_package.PallocChunkPages - 1, 1)],
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 2)] = []
            },
            scav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 0)] = [],
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset] = [],
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 2)] = []
            },
            hits: slice<TestPageAllocAlloc_hit>([
                // We first allocate into A to set the page allocator's searchAddr to the
                // end of that chunk. That is the only purpose A serves.
                new(1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1), 0),
                // Then, we make a big allocation that doesn't fit into B, and so must be
                // fulfilled by C.
                //
                // On the way to fulfilling the allocation into C, we estimate searchAddr
                // using the summary structure, but that will give us a searchAddr of
                // B's base address minus chunkIdxSmallOffset chunks. These chunks will
                // not be mapped.
                new(100, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)(chunkIdxBigJump * 2), 0), 0),
                // Now we try to make a smaller allocation that can be fulfilled by B.
                // In an older implementation of the page allocator, this will segfault,
                // because this last allocation will first try to access the summary
                // for B's base address minus chunkIdxSmallOffset chunks in the fast path,
                // and this will not be mapped.
                new(9, runtime_internal_test_package.PageBase(baseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset, runtime_internal_test_package.PallocChunkPages - 10), 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 0)] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 1) + (nuint)chunkIdxSmallOffset] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + (nuint)(chunkIdxBigJump * 2)] = [new(0, 100)]
            }
        );
    }
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPageAllocAlloc_test(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.before, vʗ1.scav);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);

                foreach (var (iter, i) in range(vʗ1.hits)) {
                    var (a, s) = b.Alloc(i.npages);
                    if (a != i.@base) {
                        tΔ1.Fatalf("bad alloc #%d: want base 0x%x, got 0x%x", iter + 1, i.@base, a);
                    }
                    if (s != i.scav) {
                        tΔ1.Fatalf("bad alloc #%d: want scav %d, got %d", iter + 1, i.scav, s);
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

public static void TestPageAllocExhaust(ж<testing.T> t) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        t.Skip(skippingBecauseVirtualˢ);
    }
    foreach (var (_, npages) in range(slice<uintptr>([1, 2, 3, 4, 5, 8, 16, 64, 1024, 1025, 2048, 2049]))) {
        t.Run(fmt.Sprintf("%d"u8, npages), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                // Construct b.
                var bDesc = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>();
                for (global::go.runtime_internal_test_package.ChunkIdx i = (global::go.runtime_internal_test_package.ChunkIdx)0; i < 4; i++) {
                    bDesc[runtime_internal_test_package.BaseChunkIdx + i] = [];
                }
                var b = runtime_internal_test_package.NewPageAlloc(bDesc, default);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);

                // Allocate into b with npages until we've exhausted the heap.
                nint nAlloc = (nint)(runtime_internal_test_package.PallocChunkPages * 4) / (nint)npages;
                for (nint i = 0; i < nAlloc; i++) {
                    var addr = runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, (nuint)i * (nuint)npages);
                    var (a, _) = b.Alloc(npages); if (a != addr) {
                        tΔ1.Fatalf("bad alloc #%d: want 0x%x, got 0x%x", i + 1, addr, a);
                    }
                }

                // Check to make sure the next allocation fails.
                {
                    var (a, _) = b.Alloc(npages); if (a != 0) {
                        tΔ1.Fatalf("bad alloc #%d: want 0, got 0x%x", nAlloc, a);
                    }
                }

                // Construct what we want the heap to look like now.
                nint allocPages = nAlloc * (nint)npages;
                var wantDesc = new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>();
                for (global::go.runtime_internal_test_package.ChunkIdx i = (global::go.runtime_internal_test_package.ChunkIdx)0; i < 4; i++) {
                    if (allocPages >= runtime_internal_test_package.PallocChunkPages) {
                        wantDesc[runtime_internal_test_package.BaseChunkIdx + i] = [new(0, runtime_internal_test_package.PallocChunkPages)];
                        allocPages -= runtime_internal_test_package.PallocChunkPages;
                    } else if (allocPages > 0) {
                        wantDesc[runtime_internal_test_package.BaseChunkIdx + i] = [new(0, (nuint)allocPages)];
                        allocPages = 0;
                    } else {
                        wantDesc[runtime_internal_test_package.BaseChunkIdx + i] = [];
                    }
                }
                var want = runtime_internal_test_package.NewPageAlloc(wantDesc, default);
                defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);

                // Check to make sure the heap b matches what we want.
                checkPageAlloc(tΔ1, want, b);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string free64ˢ => ref free64ˢᵇ;
private static readonly @string free64ˢᵇ = "Free64"u8;
internal static ref readonly @string free65ˢ => ref free65ˢᵇ;
private static readonly @string free65ˢᵇ = "Free65"u8;

internal partial struct TestPageAllocFree_test /*dyn*/ {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> before;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> after;
    internal uintptr npages;
    internal slice<uintptr> frees;
}

public static void TestPageAllocFree(ж<testing.T> t) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        t.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocFree_test>{
        ["Free1"] = new(
            npages: 1,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 1),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 3),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(5, runtime_internal_test_package.PallocChunkPages - 5)]
            }
        ),
        ["ManyArena1"] = new(
            npages: 1,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 2, runtime_internal_test_package.PallocChunkPages - 1)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages / 2), new(runtime_internal_test_package.PallocChunkPages / 2 + 1, runtime_internal_test_package.PallocChunkPages / 2 - 1)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(1, runtime_internal_test_package.PallocChunkPages - 1)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages - 1)]
            }
        ),
        ["Free2"] = new(
            npages: 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 2),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 6),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 8)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(10, runtime_internal_test_package.PallocChunkPages - 10)]
            }
        ),
        ["Straddle2"] = new(
            npages: 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(runtime_internal_test_package.PallocChunkPages - 1, 1)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, 1)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            }
        ),
        ["Free5"] = new(
            npages: 5,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 5),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 10),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 15),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 20)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(25, runtime_internal_test_package.PallocChunkPages - 25)]
            }
        ),
        ["FreePallocChunkPages"] = new(
            npages: runtime_internal_test_package.PallocChunkPages,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = []
            }
        ),
        ["StraddlePallocChunkPages"] = new(
            npages: runtime_internal_test_package.PallocChunkPages,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages / 2)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            }
        ),
        ["StraddlePallocChunkPages+1"] = new(
            npages: runtime_internal_test_package.PallocChunkPages + 1,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages / 2)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(runtime_internal_test_package.PallocChunkPages / 2 + 1, runtime_internal_test_package.PallocChunkPages / 2 - 1)]
            }
        ),
        ["FreePallocChunkPages*2"] = new(
            npages: runtime_internal_test_package.PallocChunkPages * 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = []
            }
        ),
        ["StraddlePallocChunkPages*2"] = new(
            npages: runtime_internal_test_package.PallocChunkPages * 2,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages / 2)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages / 2)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)]
            }
        ),
        ["AllFreePallocChunkPages*7+5"] = new(
            npages: runtime_internal_test_package.PallocChunkPages * 7 + 5,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 4] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 5] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 6] = [new(0, runtime_internal_test_package.PallocChunkPages)],
                [runtime_internal_test_package.BaseChunkIdx + 7] = [new(0, runtime_internal_test_package.PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [],
                [runtime_internal_test_package.BaseChunkIdx + 4] = [],
                [runtime_internal_test_package.BaseChunkIdx + 5] = [],
                [runtime_internal_test_package.BaseChunkIdx + 6] = [],
                [runtime_internal_test_package.BaseChunkIdx + 7] = [new(5, runtime_internal_test_package.PallocChunkPages - 5)]
            }
        )
    };
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        // avoid constant overflow when PallocChunkPages is small
        nuint PallocChunkPages = runtime_internal_test_package.PallocChunkPages;
        tests[free64ˢ] = new TestPageAllocFree_test(
            npages: 64,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 64),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 128)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(192, PallocChunkPages - 192)]
            }
        );
        tests[free65ˢ] = new TestPageAllocFree_test(
            npages: 65,
            before: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(0, PallocChunkPages)]
            },
            frees: slice<uintptr>([
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 65),
                runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 130)
            ]),
            after: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [new(195, PallocChunkPages - 195)]
            }
        );
    }
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPageAllocFree_test(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.before, default);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);

                foreach (var (_, addr) in range(vʗ1.frees)) {
                    b.Free(addr, vʗ1.npages);
                }
                var want = runtime_internal_test_package.NewPageAlloc(vʗ1.after, default);
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

public static void TestPageAllocAllocAndFree(ж<testing.T> t) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        t.Skip(skippingBecauseVirtualˢ);
    }
    var tests = new map<@string, TestPageAllocAllocAndFree_tests>{
        // TODO(mknyszek): Write more tests here.
        ["Chunks8"] = new(
            init: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = [],
                [runtime_internal_test_package.BaseChunkIdx + 1] = [],
                [runtime_internal_test_package.BaseChunkIdx + 2] = [],
                [runtime_internal_test_package.BaseChunkIdx + 3] = [],
                [runtime_internal_test_package.BaseChunkIdx + 4] = [],
                [runtime_internal_test_package.BaseChunkIdx + 5] = [],
                [runtime_internal_test_package.BaseChunkIdx + 6] = [],
                [runtime_internal_test_package.BaseChunkIdx + 7] = []
            },
            hits: slice<TestPageAllocAllocAndFree_hit>([
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, 1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(false, 1, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0)),
                new(true, runtime_internal_test_package.PallocChunkPages * 8, runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0))
            ])
        )
    };
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPageAllocAllocAndFree_tests(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.init, default);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);

                foreach (var (iter, i) in range(vʗ1.hits)) {
                    if (i.alloc) {
                        var (a, _) = b.Alloc(i.npages); if (a != i.@base) {
                            tΔ1.Fatalf("bad alloc #%d: want 0x%x, got 0x%x", iter + 1, i.@base, a);
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
