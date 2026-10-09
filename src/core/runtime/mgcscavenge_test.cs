// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using Δmath = math_package;
using rand = global::go.math.rand_package;
using static runtime_package;
using testing = testing_package;
using time = time_package;
using @internal;
using @internal.runtime;
using global::go.math;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// makePallocData produces an initialized PallocData by setting
// the ranges of described in alloc and scavenge.
internal static ж<global::go.runtime_internal_test_package.ΔPallocData> makePallocData(slice<global::go.runtime_internal_test_package.BitRange> alloc, slice<global::go.runtime_internal_test_package.BitRange> scavenged) {
    var b = @new<global::go.runtime_internal_test_package.ΔPallocData>();
    foreach (var (_, v) in alloc) {
        if (v.N == 0) {
            // Skip N==0. It's harmless and allocRange doesn't
            // handle this case.
            continue;
        }
        b.AllocRange(v.I, v.N);
    }
    foreach (var (_, v) in scavenged) {
        if (v.N == 0) {
            // See the previous loop.
            continue;
        }
        b.ScavengedSetRange(v.I, v.N);
    }
    return b;
}

public static void TestFillAligned(ж<testing.T> Ꮡt) {
    uint64 fillAlignedSlow(uint64 x, nuint m) {
        if (m == 1) {
            return x;
        }
        var @out = (uint64)0;
        for (nuint i = (nuint)0; i < 64; i += m) {
            for (nuint j = (nuint)0; j < m; j++) {
                if ((uint64)(x & (((uint64)1).Lsh((i + j)))) != 0) {
                    @out |= (uint64)(((((uint64)1).Lsh(m)) - 1).Lsh(i));
                    break;
                }
            }
        }
        return @out;
    }
    var fillAlignedSlowʗ1 = fillAlignedSlow;
    void check(uint64 x, nuint m) {
        var want = fillAlignedSlowʗ1(x, m);
        {
            var got = runtime_internal_test_package.FillAligned(x, m); if (got != want) {
                Ꮡt.Logf("got:  %064b"u8, got);
                Ꮡt.Logf("want: %064b"u8, want);
                Ꮡt.Errorf("bad fillAligned(%016x, %d)"u8, x, m);
            }
        }
    }
    for (nuint m = (nuint)1; m <= 64; m *= 2) {
        var tests = new uint64[]{
            0x0000000000000000,
            0x00000000ffffffffU,
            0xffffffff00000000UL,
            0x8000000000000001UL,
            0xf00000000000000fUL,
            0xf00000010050000fUL,
            0xffffffffffffffffUL,
            0x0000000000000001,
            0x0000000000000002,
            0x0000000000000008,
            ((uint64)1).Lsh((m - 1)),
            ((uint64)1).Lsh(m), // Try a few fixed arbitrary examples.

            0xb02b9effcf137016UL,
            0x3975a076a9fbff18UL,
            0x0f8c88ec3b81506eUL,
            0x60f14d80ef2fa0e6UL
        }.slice();
        foreach (var (_, test) in tests) {
            check(test, m);
        }
        for (nint i = 0; i < 1000; i++) {
            // Try a pseudo-random numbers.
            check(rand.Uint64(), m);
            if (m > 1) {
                // For m != 1, let's construct a slightly more interesting
                // random test. Generate a bitmap which is either 0 or
                // randomly set bits for each m-aligned group of m bits.
                var val = (uint64)0;
                for (nuint n = (nuint)0; n < 64; n += m) {
                    // For each group of m bits, flip a coin:
                    // * Leave them as zero.
                    // * Set them randomly.
                    if (rand.Uint64() % 2 == 0) {
                        val |= (uint64)(((uint64)(rand.Uint64() & ((((uint64)1).Lsh(m)) - 1))).Lsh(n));
                    }
                }
                check(val, m);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oneFreeˢ = "OneFree"u8;
internal static readonly @string oneScavengedˢ = "OneScavenged"u8;
internal static readonly @string preserveHugePageBottomˢ = "PreserveHugePageBottom"u8;
internal static readonly @string preserveHugePageMiddleˢ = "PreserveHugePageMiddle"u8;
internal static readonly @string preserveHugePageTopˢ = "PreserveHugePageTop"u8;
internal static readonly @string preserveHugePageAllˢ = "PreserveHugePageAll"u8;
internal static readonly @string preserveHugePageNoneˢ = "PreserveHugePageNone"u8;

internal partial struct TestPallocDataFindScavengeCandidate_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> alloc, scavenged;
    internal uintptr min, max;
    internal global::go.runtime_internal_test_package.BitRange want;
}

public static void TestPallocDataFindScavengeCandidate(ж<testing.T> Ꮡt) {
    var tests = new map<@string, TestPallocDataFindScavengeCandidate_test>{
        ["MixedMin1"u8] = new(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 40), new(42, runtime_internal_test_package.PallocChunkPages - 42)}.slice(),
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(0, 41), new(42, runtime_internal_test_package.PallocChunkPages - 42)}.slice(),
            min: 1,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange(41, 1)
        ),
        ["MultiMin1"u8] = new(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 63), new(65, 20), new(87, runtime_internal_test_package.PallocChunkPages - 87)}.slice(),
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(86, 1)}.slice(),
            min: 1,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange(85, 1)
        )
    };
    // Try out different page minimums.
    for (var m = (uintptr)1; m <= 64; m *= 2) {
        @string suffix = fmt.Sprintf("Min%d"u8, m);
        tests["AllFree"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            min: m,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange(0, runtime_internal_test_package.PallocChunkPages)
        );
        tests["AllScavenged"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            min: m,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange(0, 0)
        );
        tests["NoneFree"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
            min: m,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange(0, 0)
        );
        tests["StartFree"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new((nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m)}.slice(),
            min: m,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange(0, (nuint)m)
        );
        tests["EndFree"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m)}.slice(),
            min: m,
            max: runtime_internal_test_package.PallocChunkPages,
            want: new runtime_internal_test_package.BitRange((nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m, (nuint)m)
        );
        tests["Straddle64"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 64 - (nuint)m), new(64 + (nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (64 + (nuint)m))}.slice(),
            min: m,
            max: 2 * m,
            want: new runtime_internal_test_package.BitRange(64 - (nuint)m, 2 * (nuint)m)
        );
        tests["BottomEdge64WithFull"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(64, 64), new(128 + 3 * (nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (128 + 3 * (nuint)m))}.slice(),
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(1, 10)}.slice(),
            min: m,
            max: 3 * m,
            want: new runtime_internal_test_package.BitRange(128, 3 * (nuint)m)
        );
        tests["BottomEdge64WithPocket"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            alloc: new global::go.runtime_internal_test_package.BitRange[]{new(64, 62), new(127, 1), new(128 + 3 * (nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (128 + 3 * (nuint)m))}.slice(),
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(1, 10)}.slice(),
            min: m,
            max: 3 * m,
            want: new runtime_internal_test_package.BitRange(128, 3 * (nuint)m)
        );
        tests["Max0"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
            scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m)}.slice(),
            min: m,
            max: 0,
            want: new runtime_internal_test_package.BitRange((nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m, (nuint)m)
        );
        if (m <= 8) {
            tests[oneFreeˢ] = new TestPallocDataFindScavengeCandidate_test(
                alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 40), new(40 + (nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (40 + (nuint)m))}.slice(),
                min: m,
                max: runtime_internal_test_package.PallocChunkPages,
                want: new runtime_internal_test_package.BitRange(40, (nuint)m)
            );
            tests[oneScavengedˢ] = new TestPallocDataFindScavengeCandidate_test(
                alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 40), new(40 + (nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (40 + (nuint)m))}.slice(),
                scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(40, 1)}.slice(),
                min: m,
                max: runtime_internal_test_package.PallocChunkPages,
                want: new runtime_internal_test_package.BitRange(0, 0)
            );
        }
        if (m > 1) {
            tests["MaxUnaligned"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
                scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(m * 2 - 1))}.slice(),
                min: m,
                max: m - 2,
                want: new runtime_internal_test_package.BitRange((nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m, (nuint)m)
            );
            tests["SkipSmall"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
                alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 64 - (nuint)m), new(64, 5), new(70, 11), new(82, runtime_internal_test_package.PallocChunkPages - 82)}.slice(),
                min: m,
                max: m,
                want: new runtime_internal_test_package.BitRange(64 - (nuint)m, (nuint)m)
            );
            tests["SkipMisaligned"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
                alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, 64 - (nuint)m), new(64, 63), new(127 + (nuint)m, (nuint)runtime_internal_test_package.PallocChunkPages - (127 + (nuint)m))}.slice(),
                min: m,
                max: m,
                want: new runtime_internal_test_package.BitRange(64 - (nuint)m, (nuint)m)
            );
            tests["MaxLessThan"u8 + suffix] = new TestPallocDataFindScavengeCandidate_test(
                scavenged: new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m)}.slice(),
                min: m,
                max: 1,
                want: new runtime_internal_test_package.BitRange((nuint)runtime_internal_test_package.PallocChunkPages - (nuint)m, (nuint)m)
            );
        }
    }
    if (runtime_internal_test_package.PhysHugePageSize > (uintptr)runtime_internal_test_package.PageSize) {
        // Check hugepage preserving behavior.
        nuint bits = (nuint)(runtime_internal_test_package.PhysHugePageSize / (uintptr)runtime_internal_test_package.PageSize);
        if (bits < runtime_internal_test_package.PallocChunkPages){
            tests[preserveHugePageBottomˢ] = new TestPallocDataFindScavengeCandidate_test(
                alloc: new global::go.runtime_internal_test_package.BitRange[]{new(bits + 2, (nuint)runtime_internal_test_package.PallocChunkPages - (bits + 2))}.slice(),
                min: 1,
                max: 3, // Make it so that max would have us try to break the huge page.

                want: new runtime_internal_test_package.BitRange(0, bits + 2)
            );
            if (3 * bits < runtime_internal_test_package.PallocChunkPages) {
                // We need at least 3 huge pages in a chunk for this test to make sense.
                tests[preserveHugePageMiddleˢ] = new TestPallocDataFindScavengeCandidate_test(
                    alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, bits - 10), new(2 * bits + 10, (nuint)runtime_internal_test_package.PallocChunkPages - (2 * bits + 10))}.slice(),
                    min: 1,
                    max: 12, // Make it so that max would have us try to break the huge page.

                    want: new runtime_internal_test_package.BitRange(bits, bits + 10)
                );
            }
            tests[preserveHugePageTopˢ] = new TestPallocDataFindScavengeCandidate_test(
                alloc: new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)runtime_internal_test_package.PallocChunkPages - bits)}.slice(),
                min: 1,
                max: 1, // Even one page would break a huge page in this case.

                want: new runtime_internal_test_package.BitRange((nuint)runtime_internal_test_package.PallocChunkPages - bits, bits)
            );
        } else 
        if (bits == runtime_internal_test_package.PallocChunkPages){
            tests[preserveHugePageAllˢ] = new TestPallocDataFindScavengeCandidate_test(
                min: 1,
                max: 1, // Even one page would break a huge page in this case.

                want: new runtime_internal_test_package.BitRange(0, runtime_internal_test_package.PallocChunkPages)
            );
        } else {
            // The huge page size is greater than pallocChunkPages, so it should
            // be effectively disabled. There's no way we can possible scavenge
            // a huge page out of this bitmap chunk.
            tests[preserveHugePageNoneˢ] = new TestPallocDataFindScavengeCandidate_test(
                min: 1,
                max: 1,
                want: new runtime_internal_test_package.BitRange(runtime_internal_test_package.PallocChunkPages - 1, 1)
            );
        }
    }
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPallocDataFindScavengeCandidate_test>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocData(vʗ1.alloc, vʗ1.scavenged);
            var (start, size) = b.FindScavengeCandidate(runtime_internal_test_package.PallocChunkPages - 1, vʗ1.min, vʗ1.max);
            var got = new runtime_internal_test_package.BitRange(start, size);
            if (!(got.N == 0 && vʗ1.want.N == 0) && got != vʗ1.want) {
                tΔ1.Fatalf("candidate mismatch: got %v, want %v"u8, got, vʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingBecauseVirtualˢ = (@string)"skipping because virtual memory is limited; see #36210"u8;
internal static readonly @string scavAllVeryDiscontiguousˢ = "ScavAllVeryDiscontiguous"u8;

internal partial struct TestPageAllocScavenge_test /*dyn*/ {
    internal uintptr request, expect;
}

internal partial struct TestPageAllocScavenge_setup /*dyn*/ {
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> beforeAlloc;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> beforeScav;
    internal slice<TestPageAllocScavenge_test> expect;
    internal map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>> afterScav;
}

// Tests end-to-end scavenging on a pageAlloc.
public static void TestPageAllocScavenge(ж<testing.T> Ꮡt) {
    if (GOOS == "openbsd"u8 && testing.Short()) {
        Ꮡt.Skip(skippingBecauseVirtualˢ);
    }
    var minPages = runtime_internal_test_package.PhysPageSize / (uintptr)runtime_internal_test_package.PageSize;
    if (minPages < 1) {
        minPages = 1;
    }
    var tests = new map<@string, TestPageAllocScavenge_setup>{
        ["AllFreeUnscavExhaust"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(~(uintptr)0, 3 * runtime_internal_test_package.PallocChunkPages * runtime_internal_test_package.PageSize)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["NoneFreeUnscavExhaust"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(~(uintptr)0, 0)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 2] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            }
        ),
        ["ScavHighestPageFirst"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new((nuint)minPages, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(2 * minPages))}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(1, minPages * (uintptr)runtime_internal_test_package.PageSize)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new((nuint)minPages, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)minPages)}.slice()
            }
        ),
        ["ScavMultiple"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new((nuint)minPages, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(2 * minPages))}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(minPages * (uintptr)runtime_internal_test_package.PageSize, minPages * (uintptr)runtime_internal_test_package.PageSize),
                new(minPages * (uintptr)runtime_internal_test_package.PageSize, minPages * (uintptr)runtime_internal_test_package.PageSize)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["ScavMultiple2"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new((nuint)minPages, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(2 * minPages))}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(2 * minPages))}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(2 * minPages * (uintptr)runtime_internal_test_package.PageSize, 2 * minPages * (uintptr)runtime_internal_test_package.PageSize),
                new(minPages * (uintptr)runtime_internal_test_package.PageSize, minPages * (uintptr)runtime_internal_test_package.PageSize),
                new(minPages * (uintptr)runtime_internal_test_package.PageSize, minPages * (uintptr)runtime_internal_test_package.PageSize)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 1] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        ),
        ["ScavDiscontiguous"u8] = new(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xe] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new((nuint)minPages, (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(2 * minPages))}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xe] = new global::go.runtime_internal_test_package.BitRange[]{new((nuint)(2 * minPages), (nuint)runtime_internal_test_package.PallocChunkPages - (nuint)(2 * minPages))}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(2 * minPages * (uintptr)runtime_internal_test_package.PageSize, 2 * minPages * (uintptr)runtime_internal_test_package.PageSize),
                new(~(uintptr)0, 2 * minPages * (uintptr)runtime_internal_test_package.PageSize),
                new(~(uintptr)0, 0)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0xe] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        )
    };
    // Disable these tests on iOS since we have a small address space.
    // See #46860.
    if (runtime_internal_test_package.PageAlloc64Bit != 0 && goos.IsIos == 0) {
        tests[scavAllVeryDiscontiguousˢ] = new TestPageAllocScavenge_setup(
            beforeAlloc: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x1000] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            beforeScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x1000] = new global::go.runtime_internal_test_package.BitRange[]{}.slice()
            },
            expect: new TestPageAllocScavenge_test[]{
                new(~(uintptr)0, 2 * runtime_internal_test_package.PallocChunkPages * runtime_internal_test_package.PageSize),
                new(~(uintptr)0, 0)
            }.slice(),
            afterScav: new map<global::go.runtime_internal_test_package.ChunkIdx, slice<global::go.runtime_internal_test_package.BitRange>>{
                [runtime_internal_test_package.BaseChunkIdx] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
                [runtime_internal_test_package.BaseChunkIdx + 0x1000] = new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
            }
        );
    }
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPageAllocScavenge_setup>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var b = runtime_internal_test_package.NewPageAlloc(vʗ1.beforeAlloc, vʗ1.beforeScav);
                defer(runtime_internal_test_package.FreePageAlloc, b, ref ᒐ);
                foreach (var (iter, h) in vʗ1.expect) {
                    {
                        var got = b.Scavenge(h.request); if (got != h.expect) {
                            tΔ1.Fatalf("bad scavenge #%d: want %d, got %d"u8, iter + 1, h.expect, got);
                        }
                    }
                }
                var want = runtime_internal_test_package.NewPageAlloc(vʗ1.beforeAlloc, vʗ1.afterScav);
                defer(runtime_internal_test_package.FreePageAlloc, want, ref ᒐ);
                checkPageAlloc(tΔ1, want, b);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timedOutWaitingForˢ = (@string)"timed out waiting for scavenger to run to completion"u8;

public static void TestScavenger(ж<testing.T> Ꮡt) {
    // workedTime is a standard conversion of bytes of scavenge
    // work to time elapsed.
    int64 workedTime(uintptr bytes) => (int64)((bytes + 4095) / 4096) * (int64)(10 * time.Microsecond);
    // Set up a bunch of state that we're going to track and verify
    // throughout the test.
    var totalWork = (uint64)(((uintptr)64 << (int)(20)) - 3 * runtime_internal_test_package.PhysPageSize);
    ref var totalSlept = ref heap(new atomic.Int64(), out var ᏑtotalSlept);
    ref var totalWorked = ref heap(new atomic.Int64(), out var ᏑtotalWorked);
    ref var availableWork = ref heap(new atomic.Uint64(), out var ᏑavailableWork);
    ref var stopAt = ref heap(new atomic.Uint64(), out var ᏑstopAt);                      // How much available work to stop at.
    // Set up the scavenger.
    ref var s = ref heap(new global::go.runtime_internal_test_package.Scavenger(), out var Ꮡs);
    s.Sleep = (int64 ns) => {
        ᏑtotalSlept.Add(ns);
        return ns;
    };
    var workedTimeʗ1 = workedTime;
    s.Scavenge = (uintptr bytes) => {
        var avail = ᏑavailableWork.Load();
        if ((uint64)bytes > avail) {
            bytes = (uintptr)avail;
        }
        var tΔ1 = workedTimeʗ1(bytes);
        if (bytes != 0) {
            ᏑavailableWork.Add(-(int64)bytes);
            ᏑtotalWorked.Add(tΔ1);
        }
        return (bytes, tΔ1);
    };
    s.ShouldStop = () => {
        if (ᏑavailableWork.Load() <= ᏑstopAt.Load()) {
            return true;
        }
        return false;
    };
    s.GoMaxProcs = () => 1;
    // Define a helper for verifying that various properties hold.
    void verifyScavengerState(ж<testing.T> tΔ2, uint64 expWork) {
        tΔ2.Helper();
        // Check to make sure it did the amount of work we expected.
        {
            var workDone = (uint64)Ꮡs.Released(); if (workDone != expWork) {
                tΔ2.Errorf("want %d bytes of work done, got %d"u8, expWork, workDone);
            }
        }
        // Check to make sure the scavenger is meeting its CPU target.
        var idealFraction = (float64)runtime_internal_test_package.ScavengePercent / 100.0D;
        var cpuFraction = (float64)ᏑtotalWorked.Load() / (float64)(ᏑtotalWorked.Load() + ᏑtotalSlept.Load());
        if (cpuFraction < idealFraction - 0.005D || cpuFraction > idealFraction + 0.005D) {
            tΔ2.Errorf("want %f CPU fraction, got %f"u8, idealFraction, cpuFraction);
        }
    }
    // Start the scavenger.
    Ꮡs.Start();
    // Set up some work and let the scavenger run to completion.
    ᏑavailableWork.Store(totalWork);
    Ꮡs.Wake();
    if (!Ꮡs.BlockUntilParked(2000000000)) {
        /* 2 seconds */
        Ꮡt.Fatal(timedOutWaitingForˢ);
    }
    // Run a check.
    verifyScavengerState(Ꮡt, totalWork);
    // Now let's do it again and see what happens when we have no work to do.
    // It should've gone right back to sleep.
    Ꮡs.Wake();
    if (!Ꮡs.BlockUntilParked(2000000000)) {
        /* 2 seconds */
        Ꮡt.Fatal(timedOutWaitingForˢ);
    }
    // Run another check.
    verifyScavengerState(Ꮡt, totalWork);
    // One more time, this time doing the same amount of work as the first time.
    // Let's see if we can get the scavenger to continue.
    ᏑavailableWork.Store(totalWork);
    Ꮡs.Wake();
    if (!Ꮡs.BlockUntilParked(2000000000)) {
        /* 2 seconds */
        Ꮡt.Fatal(timedOutWaitingForˢ);
    }
    // Run another check.
    verifyScavengerState(Ꮡt, 2 * totalWork);
    // This time, let's stop after a certain amount of work.
    //
    // Pick a stopping point such that when subtracted from totalWork
    // we get a multiple of a relatively large power of 2. verifyScavengerState
    // always makes an exact check, but the scavenger might go a little over,
    // which is OK. If this breaks often or gets annoying to maintain, modify
    // verifyScavengerState.
    ᏑavailableWork.Store(totalWork);
    var stoppingPoint = (uint64)(((uintptr)1 << (int)(20)) - 3 * runtime_internal_test_package.PhysPageSize);
    ᏑstopAt.Store(stoppingPoint);
    Ꮡs.Wake();
    if (!Ꮡs.BlockUntilParked(2000000000)) {
        /* 2 seconds */
        Ꮡt.Fatal(timedOutWaitingForˢ);
    }
    // Run another check.
    verifyScavengerState(Ꮡt, 2 * totalWork + (totalWork - stoppingPoint));
    // Clean up.
    Ꮡs.Stop();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bgMarkInterleavedˢ = "Bg/MarkInterleaved"u8;
internal static readonly @string forceMarkInterleavedˢ = "Force/MarkInterleaved"u8;

// Each of these test cases calls mark and then find once.
internal partial struct TestScavengeIndex_testCase /*dyn*/ {
    internal @string name;
    internal Action<Action<uintptr, uintptr>> mark;
    internal Action<Action<global::go.runtime_internal_test_package.ChunkIdx, nuint>> find;
}

public static void TestScavengeIndex(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This test suite tests the scavengeIndex data structure.    // type markFunc is a methodless func type — rendered inline as its base delegate
    // type findFunc is a methodless func type — rendered inline as its base delegate

    // The structure of the tests below is as follows:
    //
    // setup creates a fake scavengeIndex that can be mutated and queried by
    // the functions it returns. Those functions capture the testing.T that
    // setup is called with, so they're bound to the subtest they're created in.
    //
    // Tests are then organized into test cases which mark some pages as
    // scavenge-able then try to find them. Tests expect that the initial
    // state of the scavengeIndex has all of the chunks as dense in the last
    // generation and empty to the scavenger.
    //
    // There are a few additional tests that interleave mark and find operations,
    // so they're defined separately, but use the same infrastructure.
    (Action<uintptr, uintptr> mark, Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find, Action nextGen) setup(ж<testing.T> tΔ1, bool force) {
        Action<uintptr, uintptr> mark = default!;
        Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find = default!;
        Action nextGen = default!;
        tΔ1.Helper();
        // Pick some reasonable bounds. We don't need a huge range just to test.
        var si = runtime_internal_test_package.NewScavengeIndex(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.BaseChunkIdx + 64);
        // Initialize all the chunks as dense and empty.
        //
        // Also, reset search addresses so that we can get page offsets.
        si.AllocRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 64, 0));
        si.NextGen();
        si.FreeRange(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 64, 0));
        for (global::go.runtime_internal_test_package.ChunkIdx ci = runtime_internal_test_package.BaseChunkIdx; ci < runtime_internal_test_package.BaseChunkIdx + 64; ci++) {
            si.SetEmpty(ci);
        }
        si.ResetSearchAddrs();
        // Create and return test functions.
        var siʗ1 = si;
        mark = (uintptr @base, uintptr limit) => {
            tΔ1.Helper();
            siʗ1.AllocRange(@base, limit);
            siʗ1.FreeRange(@base, limit);
        };
        var siʗ2 = si;
        find = (global::go.runtime_internal_test_package.ChunkIdx want, nuint wantOffset) => {
            tΔ1.Helper();
            var (got, gotOffset) = siʗ2.Find(force);
            if (want != got) {
                tΔ1.Errorf("find: wanted chunk index %d, got %d"u8, want, got);
            }
            if (wantOffset != gotOffset) {
                tΔ1.Errorf("find: wanted page offset %d, got %d"u8, wantOffset, gotOffset);
            }
            if (tΔ1.Failed()) {
                tΔ1.FailNow();
            }
            siʗ2.SetEmpty(got);
        };
        var siʗ3 = si;
        nextGen = () => {
            tΔ1.Helper();
            siʗ3.NextGen();
        };
        return (mark, find, nextGen);
    }





























    foreach (var (_, test) in new TestScavengeIndex_testCase[]{
        new(
            name: "Uninitialized"u8,
            mark: (Action<uintptr, uintptr> _Δp0) => {
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> _Δp0) => {
            }
        ),
        new(
            name: "OnePage"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 3), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 4));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx, 3);
            }
        ),
        new(
            name: "FirstPage"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 1));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx, 0);
            }
        ),
        new(
            name: "SeveralPages"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 9), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 14));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx, 13);
            }
        ),
        new(
            name: "WholeChunk"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1);
            }
        ),
        new(
            name: "LastPage"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1);
            }
        ),
        new(
            name: "TwoChunks"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 128), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 128));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx + 1, 127);
                find(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1);
            }
        ),
        new(
            name: "TwoChunksOffset"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 7, 128), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 8, 129));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx + 8, 128);
                find(runtime_internal_test_package.BaseChunkIdx + 7, runtime_internal_test_package.PallocChunkPages - 1);
            }
        ),
        new(
            name: "SevenChunksOffset"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 6, 11), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 13, 15));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx + 13, 14);
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx + 12; i >= runtime_internal_test_package.BaseChunkIdx + 6; i--) {
                    find(i, runtime_internal_test_package.PallocChunkPages - 1);
                }
            }
        ),
        new(
            name: "ThirtyTwoChunks"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 32, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx + 31; i >= runtime_internal_test_package.BaseChunkIdx; i--) {
                    find(i, runtime_internal_test_package.PallocChunkPages - 1);
                }
            }
        ),
        new(
            name: "ThirtyTwoChunksOffset"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 3, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 35, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx + 34; i >= runtime_internal_test_package.BaseChunkIdx + 3; i--) {
                    find(i, runtime_internal_test_package.PallocChunkPages - 1);
                }
            }
        ),
        new(
            name: "Mark"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx; i < runtime_internal_test_package.BaseChunkIdx + 32; i++) {
                    mark(runtime_internal_test_package.PageBase(i, 0), runtime_internal_test_package.PageBase(i + 1, 0));
                }
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx + 31; i >= runtime_internal_test_package.BaseChunkIdx; i--) {
                    find(i, runtime_internal_test_package.PallocChunkPages - 1);
                }
            }
        ),
        new(
            name: "MarkIdempotentOneChunk"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0));
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 1, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                find(runtime_internal_test_package.BaseChunkIdx, runtime_internal_test_package.PallocChunkPages - 1);
            }
        ),
        new(
            name: "MarkIdempotentThirtyTwoChunks"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 32, 0));
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 32, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx + 31; i >= runtime_internal_test_package.BaseChunkIdx; i--) {
                    find(i, runtime_internal_test_package.PallocChunkPages - 1);
                }
            }
        ),
        new(
            name: "MarkIdempotentThirtyTwoChunksOffset"u8,
            mark: (Action<uintptr, uintptr> mark) => {
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 4, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 31, 0));
                mark(runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 5, 0), runtime_internal_test_package.PageBase(runtime_internal_test_package.BaseChunkIdx + 36, 0));
            },
            find: (Action<global::go.runtime_internal_test_package.ChunkIdx, nuint> find) => {
                for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx + 35; i >= runtime_internal_test_package.BaseChunkIdx + 4; i--) {
                    find(i, runtime_internal_test_package.PallocChunkPages - 1);
                }
            }
        )
    }.slice()) {
        ref var testΔ1 = ref heap<TestScavengeIndex_testCase>(out var ᏑtestΔ1);
        testΔ1 = test;
        var setupʗ1 = setup;
        var testʗ1 = testΔ1;
        Ꮡt.Run("Bg/"u8 + testΔ1.name, (ж<testing.T> tΔ2) => {
            var (mark, find, nextGen) = setupʗ1(tΔ2, false);
            testʗ1.mark(mark);
            find(0, 0); // Make sure we find nothing at this point.
            nextGen(); // Move to the next generation.
            testʗ1.find(find); // Now we should be able to find things.
            find(0, 0); // The test should always fully exhaust the index.
        });
        var setupʗ2 = setup;
        var testʗ2 = testΔ1;
        Ꮡt.Run("Force/"u8 + testΔ1.name, (ж<testing.T> tΔ3) => {
            var (mark, find, _) = setupʗ2(tΔ3, true);
            testʗ2.mark(mark);
            testʗ2.find(find); // Finding should always work when forced.
            find(0, 0); // The test should always fully exhaust the index.
        });
    }
    var setupʗ3 = setup;
    Ꮡt.Run(bgMarkInterleavedˢ, (ж<testing.T> tΔ4) => {
        var (mark, find, nextGen) = setupʗ3(tΔ4, false);
        for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx; i < runtime_internal_test_package.BaseChunkIdx + 32; i++) {
            mark(runtime_internal_test_package.PageBase(i, 0), runtime_internal_test_package.PageBase(i + 1, 0));
            nextGen();
            find(i, runtime_internal_test_package.PallocChunkPages - 1);
        }
        find(0, 0);
    });
    var setupʗ4 = setup;
    Ꮡt.Run(forceMarkInterleavedˢ, (ж<testing.T> tΔ5) => {
        var (mark, find, _) = setupʗ4(tΔ5, true);
        for (global::go.runtime_internal_test_package.ChunkIdx i = runtime_internal_test_package.BaseChunkIdx; i < runtime_internal_test_package.BaseChunkIdx + 32; i++) {
            mark(runtime_internal_test_package.PageBase(i, 0), runtime_internal_test_package.PageBase(i + 1, 0));
            find(i, runtime_internal_test_package.PallocChunkPages - 1);
        }
        find(0, 0);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedPackUnpackCheckForˢ = (@string)"failed pack/unpack check for scavChunkData 1"u8;
internal static readonly object failedPackUnpackCheckForˢ2 = (@string)"failed pack/unpack check for scavChunkData 2"u8;

public static void TestScavChunkDataPack(ж<testing.T> Ꮡt) {
    if (!runtime_internal_test_package.CheckPackScavChunkData(1918237402, 512, 512, 0b11)) {
        Ꮡt.Error(failedPackUnpackCheckForˢ);
    }
    if (!runtime_internal_test_package.CheckPackScavChunkData(~(uint32)0, 12, 0, 0b00)) {
        Ꮡt.Error(failedPackUnpackCheckForˢ2);
    }
}

public static void FuzzPIController(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    bool isNormal(float64 x) => !Δmath.IsInf(x, 0) && !Δmath.IsNaN(x);
    var isNormalʗ1 = isNormal;
    bool isPositive(float64 x) => isNormalʗ1(x) && x > 0D;
    // Seed with constants from controllers in the runtime.
    // It's not critical that we keep these in sync, they're just
    // reasonable seed inputs.
    f.Add(0.3375D, 3.2e6D, 1e9D, 0.001D, 1000.0D, 0.01D);
    f.Add(0.9D, 4.0D, 1000.0D, -1000.0D, 1000.0D, 0.84D);
    var isNormalʗ2 = isNormal;
    var isPositiveʗ1 = isPositive;
    Ꮡf.Fuzz((ж<testing.T> t, float64 kp, float64 ti, float64 tt, float64 min, float64 max, float64 setPoint) => {
        // Ignore uninteresting invalid parameters. These parameters
        // are constant, so in practice surprising values will be documented
        // or will be other otherwise immediately visible.
        //
        // We just want to make sure that given a non-Inf, non-NaN input,
        // we always get a non-Inf, non-NaN output.
        if (!isPositiveʗ1(kp) || !isPositiveʗ1(ti) || !isPositiveʗ1(tt)) {
            return;
        }
        if (!isNormalʗ2(min) || !isNormalʗ2(max) || min > max) {
            return;
        }
        // Use a random source, but make it deterministic.
        var rs = rand.New(rand.NewSource(800));
        var rsʗ1 = rs;
        float64 randFloat64() => Δmath.Float64frombits(rsʗ1.Uint64());
        var p = runtime_internal_test_package.NewPIController(kp, ti, tt, min, max);
        var state = (float64)0D;
        for (nint i = 0; i < 100; i++) {
            var input = randFloat64();
            // Ignore the "ok" parameter. We're just trying to break it.
            // state is intentionally completely uncorrelated with the input.
            bool ok = default!;
            (state, ok) = p.Next(input, setPoint, 1.0D);
            if (!isNormalʗ2(state)) {
                t.Fatalf("got NaN or Inf result from controller: %f %v"u8, state, ok);
            }
        }
    });
}

} // end runtime_test_package
