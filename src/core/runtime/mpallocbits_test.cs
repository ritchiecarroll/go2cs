// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using rand = global::go.math.rand_package;
using static runtime_package;
using testing = testing_package;
using global::go.math;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// Ensures that got and want are the same, and if not, reports
// detailed diff information.
internal static bool checkPallocBits(ж<testing.T> t, ж<global::go.runtime_internal_test_package.ΔPallocBits> got, ж<global::go.runtime_internal_test_package.ΔPallocBits> want) {
    var d = runtime_internal_test_package.DiffPallocBits(got, want);
    if (len(d) != 0) {
        t.Errorf("%d range(s) different", len(d));
        foreach (var (_, bits) in range(d)) {
            t.Logf("\t@ bit index %d", bits.I);
            t.Logf("\t|  got: %s", runtime_internal_test_package.StringifyPallocBits(got, bits));
            t.Logf("\t| want: %s", runtime_internal_test_package.StringifyPallocBits(want, bits));
        }
        return false;
    }
    return true;
}

// makePallocBits produces an initialized PallocBits by setting
// the ranges in s to 1 and the rest to zero.
internal static ж<global::go.runtime_internal_test_package.ΔPallocBits> makePallocBits(slice<global::go.runtime_internal_test_package.BitRange> s) {
    var b = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
    foreach (var (_, v) in range(s)) {
        b.AllocRange(v.I, v.N);
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oneLowˢ => ref oneLowˢᵇ;
private static readonly @string oneLowˢᵇ = "OneLow"u8;
internal static ref readonly @string oneHighˢ => ref oneHighˢᵇ;
private static readonly @string oneHighˢᵇ = "OneHigh"u8;
internal static ref readonly @string innerˢ => ref innerˢᵇ;
private static readonly @string innerˢᵇ = "Inner"u8;
internal static ref readonly @string alignedˢ => ref alignedˢᵇ;
private static readonly @string alignedˢᵇ = "Aligned"u8;
internal static ref readonly @string beginˢ => ref beginˢᵇ;
private static readonly @string beginˢᵇ = "Begin"u8;
internal static ref readonly @string endˢ => ref endˢᵇ;
private static readonly @string endˢᵇ = "End"u8;
internal static ref readonly @string allˢ => ref allˢᵇ;
private static readonly @string allˢᵇ = "All"u8;

// Ensures that PallocBits.AllocRange works, which is a fundamental
// method used for testing and initialization since it's used by
// makePallocBits.
public static void TestPallocBitsAllocRange(ж<testing.T> t) {
    void test(ж<testing.T> tΔ1, nuint i, nuint n, ж<global::go.runtime_internal_test_package.ΔPallocBits> want) {
        checkPallocBits(tΔ1, makePallocBits([new(i, n)]), want);
    }
    var testʗ1 = test;
    t.Run(oneLowˢ, (ж<testing.T> tΔ2) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[0] = 0x1;
        testʗ1(tΔ2, 0, 1, want);
    });
    var testʗ2 = test;
    t.Run(oneHighˢ, (ж<testing.T> tΔ3) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[runtime_internal_test_package.PallocChunkPages / 64 - 1] = (uint64)1 << 63;
        testʗ2(tΔ3, runtime_internal_test_package.PallocChunkPages - 1, 1, want);
    });
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        var testʗ3 = test;
        t.Run(innerˢ, (ж<testing.T> tΔ4) => {
            var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
            want.Value[..][2] = 0x3e;
            testʗ3(tΔ4, 129, 5, want);
        });
        var testʗ4 = test;
        t.Run(alignedˢ, (ж<testing.T> tΔ5) => {
            var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
            want.Value[..][2] = ~(uint64)0;
            want.Value[..][3] = ~(uint64)0;
            testʗ4(tΔ5, 128, 128, want);
        });
        var testʗ5 = test;
        t.Run(beginˢ, (ж<testing.T> tΔ6) => {
            var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
            want.Value[..][0] = ~(uint64)0;
            want.Value[..][1] = ~(uint64)0;
            want.Value[..][2] = ~(uint64)0;
            want.Value[..][3] = ~(uint64)0;
            want.Value[..][4] = ~(uint64)0;
            want.Value[..][5] = 0x1;
            testʗ5(tΔ6, 0, 321, want);
        });
        var testʗ6 = test;
        t.Run(endˢ, (ж<testing.T> tΔ7) => {
            // avoid constant overflow when PallocChunkPages is small
            nuint PallocChunkPages = runtime_internal_test_package.PallocChunkPages;
            var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
            want.Value[PallocChunkPages / 64 - 1] = ~(uint64)0;
            want.Value[PallocChunkPages / 64 - 2] = ~(uint64)0;
            want.Value[PallocChunkPages / 64 - 3] = ~(uint64)0;
            want.Value[PallocChunkPages / 64 - 4] = (uint64)1 << 63;
            testʗ6(tΔ7, PallocChunkPages - (64 * 3 + 1), 64 * 3 + 1, want);
        });
    }
    var testʗ7 = test;
    t.Run(allˢ, (ж<testing.T> tΔ8) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        foreach (var i in range(want, 8)) {
            want.Value[i] = ~(uint64)0;
        }
        testʗ7(tΔ8, 0, runtime_internal_test_package.PallocChunkPages, want);
    });
}

// Inverts every bit in the PallocBits.
internal static void invertPallocBits(ж<global::go.runtime_internal_test_package.ΔPallocBits> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var i in range(8)) {
        b[i] = ~b[i];
    }
}

// Ensures two packed summaries are identical, and reports a detailed description
// of the difference if they're not.
internal static void checkPallocSum(testing.TB t, global::go.runtime_internal_test_package.PallocSum got, global::go.runtime_internal_test_package.PallocSum want) {
    if (got.Start() != want.Start()) {
        t.Errorf("inconsistent start: got %d, want %d", got.Start(), want.Start());
    }
    if (got.Max() != want.Max()) {
        t.Errorf("inconsistent max: got %d, want %d", got.Max(), want.Max());
    }
    if (got.End() != want.End()) {
        t.Errorf("inconsistent end: got %d, want %d", got.End(), want.End());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oddBoundˢ => ref oddBoundˢᵇ;
private static readonly @string oddBoundˢᵇ = "OddBound"u8;
internal static ref readonly @string scatteredˢ => ref scatteredˢᵇ;
private static readonly @string scatteredˢᵇ = "Scattered"u8;

internal partial struct TestMallocBitsPopcntRange_test /*dyn*/ {
    internal nuint i, n; // bit range to popcnt over.
    internal nuint want; // expected popcnt result on that range.
}

internal partial struct TestMallocBitsPopcntRange_testCase /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> init; // bit ranges to set to 1 in the bitmap.
    internal slice<TestMallocBitsPopcntRange_test> tests;                   // a set of popcnt tests to run over the bitmap.
}

public static void TestMallocBitsPopcntRange(ж<testing.T> t) {
    var tests = new map<@string, TestMallocBitsPopcntRange_testCase>{
        ["None"] = new(
            tests: [
                new(0, 1, 0),
                new(5, 3, 0),
                new(2, 11, 0),
                new(runtime_internal_test_package.PallocChunkPages / 4 + 1, runtime_internal_test_package.PallocChunkPages / 2, 0),
                new(0, runtime_internal_test_package.PallocChunkPages, 0)
            ]
        ),
        ["All"] = new(
            init: [new(0, runtime_internal_test_package.PallocChunkPages)],
            tests: [
                new(0, 1, 1),
                new(5, 3, 3),
                new(2, 11, 11),
                new(runtime_internal_test_package.PallocChunkPages / 4 + 1, runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2),
                new(0, runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages)
            ]
        ),
        ["Half"] = new(
            init: [new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)],
            tests: [
                new(0, 1, 0),
                new(5, 3, 0),
                new(2, 11, 0),
                new(runtime_internal_test_package.PallocChunkPages / 2 - 1, 1, 0),
                new(runtime_internal_test_package.PallocChunkPages / 2, 1, 1),
                new(runtime_internal_test_package.PallocChunkPages / 2 + 10, 1, 1),
                new(runtime_internal_test_package.PallocChunkPages / 2 - 1, 2, 1),
                new(runtime_internal_test_package.PallocChunkPages / 4, runtime_internal_test_package.PallocChunkPages / 4, 0),
                new(runtime_internal_test_package.PallocChunkPages / 4, runtime_internal_test_package.PallocChunkPages / 4 + 1, 1),
                new(runtime_internal_test_package.PallocChunkPages / 4 + 1, runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 4 + 1),
                new(0, runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages / 2)
            ]
        )
    };
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        tests[oddBoundˢ] = new TestMallocBitsPopcntRange_testCase(
            init: [new(0, 111)],
            tests: [
                new(0, 1, 1),
                new(5, 3, 3),
                new(2, 11, 11),
                new(110, 2, 1),
                new(99, 50, 12),
                new(110, 1, 1),
                new(111, 1, 0),
                new(99, 1, 1),
                new(120, 1, 0),
                new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2, 0),
                new(0, runtime_internal_test_package.PallocChunkPages, 111)
            ]
        );
        tests[scatteredˢ] = new TestMallocBitsPopcntRange_testCase(
            init: [
                new(1, 3), new(5, 1), new(7, 1), new(10, 2), new(13, 1), new(15, 4),
                new(21, 1), new(23, 1), new(26, 2), new(30, 5), new(36, 2), new(40, 3),
                new(44, 6), new(51, 1), new(53, 2), new(58, 3), new(63, 1), new(67, 2),
                new(71, 10), new(84, 1), new(89, 7), new(99, 2), new(103, 1), new(107, 2),
                new(111, 1), new(113, 1), new(115, 1), new(118, 1), new(120, 2), new(125, 5)
            ],
            tests: [
                new(0, 11, 6),
                new(0, 64, 39),
                new(13, 64, 40),
                new(64, 64, 34),
                new(0, 128, 73),
                new(1, 128, 74),
                new(0, runtime_internal_test_package.PallocChunkPages, 75)
            ]
        );
    }
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestMallocBitsPopcntRange_testCase(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.init);
            foreach (var (_, h) in range(vʗ1.tests)) {
                nuint got = b.PopcntRange(h.i, h.n); if (got != h.want) {
                    tΔ1.Errorf("bad popcnt (i=%d, n=%d): got %d, want %d", h.i, h.n, got, h.want);
                }
            }
        });
    }
}

// Ensures computing bit summaries works as expected by generating random
// bitmaps and checking against a reference implementation.
public static void TestPallocBitsSummarizeRandom(ж<testing.T> t) {
    var b = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
    for (nint i = 0; i < 1000; i++) {
        // Randomize bitmap.
        foreach (var iΔ1 in range(b, 8)) {
            b.Value[iΔ1] = rand.Uint64();
        }
        // Check summary against reference implementation.
        checkPallocSum(new runtime_test_package.testing_TжTB(t), b.Summarize(), runtime_internal_test_package.SummarizeSlow(b));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noneFreeˢ => ref noneFreeˢᵇ;
private static readonly @string noneFreeˢᵇ = "NoneFree"u8;
internal static ref readonly @string onlyStartˢ => ref onlyStartˢᵇ;
private static readonly @string onlyStartˢᵇ = "OnlyStart"u8;
internal static ref readonly @string onlyEndˢ => ref onlyEndˢᵇ;
private static readonly @string onlyEndˢᵇ = "OnlyEnd"u8;
internal static ref readonly @string startAndEndˢ => ref startAndEndˢᵇ;
private static readonly @string startAndEndˢᵇ = "StartAndEnd"u8;
internal static ref readonly @string startMaxEndˢ => ref startMaxEndˢᵇ;
private static readonly @string startMaxEndˢᵇ = "StartMaxEnd"u8;
internal static ref readonly @string onlyMaxˢ => ref onlyMaxˢᵇ;
private static readonly @string onlyMaxˢᵇ = "OnlyMax"u8;
internal static ref readonly @string multiMaxˢ => ref multiMaxˢᵇ;
private static readonly @string multiMaxˢᵇ = "MultiMax"u8;
internal static ref readonly @string oneˢ => ref oneˢᵇ;
private static readonly @string oneˢᵇ = "One"u8;

internal partial struct TestPallocBitsSummarize_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> free; // Ranges of free (zero) bits.
    internal slice<global::go.runtime_internal_test_package.PallocSum> hits;
}

// Ensures computing bit summaries works as expected.
public static void TestPallocBitsSummarize(ж<testing.T> t) {
    global::go.runtime_internal_test_package.PallocSum emptySum = runtime_internal_test_package.PackPallocSum(runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages);
    var tests = new map<@string, TestPallocBitsSummarize_test>();
    tests[noneFreeˢ] = new TestPallocBitsSummarize_test(
        free: [],
        hits: slice<global::go.runtime_internal_test_package.PallocSum>([
            runtime_internal_test_package.PackPallocSum(0, 0, 0)
        ])
    );
    tests[onlyStartˢ] = new TestPallocBitsSummarize_test(
        free: [new(0, 10)],
        hits: slice<global::go.runtime_internal_test_package.PallocSum>([
            runtime_internal_test_package.PackPallocSum(10, 10, 0)
        ])
    );
    tests[onlyEndˢ] = new TestPallocBitsSummarize_test(
        free: [new(runtime_internal_test_package.PallocChunkPages - 40, 40)],
        hits: slice<global::go.runtime_internal_test_package.PallocSum>([
            runtime_internal_test_package.PackPallocSum(0, 40, 40)
        ])
    );
    tests[startAndEndˢ] = new TestPallocBitsSummarize_test(
        free: [new(0, 11), new(runtime_internal_test_package.PallocChunkPages - 23, 23)],
        hits: slice<global::go.runtime_internal_test_package.PallocSum>([
            runtime_internal_test_package.PackPallocSum(11, 23, 23)
        ])
    );
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        tests[startMaxEndˢ] = new TestPallocBitsSummarize_test(
            free: [new(0, 4), new(50, 100), new(runtime_internal_test_package.PallocChunkPages - 4, 4)],
            hits: slice<global::go.runtime_internal_test_package.PallocSum>([
                runtime_internal_test_package.PackPallocSum(4, 100, 4)
            ])
        );
        tests[onlyMaxˢ] = new TestPallocBitsSummarize_test(
            free: [new(1, 20), new(35, 241), new(runtime_internal_test_package.PallocChunkPages - 50, 30)],
            hits: slice<global::go.runtime_internal_test_package.PallocSum>([
                runtime_internal_test_package.PackPallocSum(0, 241, 0)
            ])
        );
        tests[multiMaxˢ] = new TestPallocBitsSummarize_test(
            free: [new(35, 2), new(40, 5), new(100, 5)],
            hits: slice<global::go.runtime_internal_test_package.PallocSum>([
                runtime_internal_test_package.PackPallocSum(0, 5, 0)
            ])
        );
    }
    tests[oneˢ] = new TestPallocBitsSummarize_test(
        free: [new(2, 1)],
        hits: slice<global::go.runtime_internal_test_package.PallocSum>([
            runtime_internal_test_package.PackPallocSum(0, 1, 0)
        ])
    );
    tests[allFreeˢ] = new TestPallocBitsSummarize_test(
        free: [new(0, runtime_internal_test_package.PallocChunkPages)],
        hits: [
            emptySum
        ]
    );
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPallocBitsSummarize_test(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.free);
            // In the PallocBits we create 1's represent free spots, but in our actual
            // PallocBits 1 means not free, so invert.
            invertPallocBits(b);
            foreach (var (_, h) in range(vʗ1.hits)) {
                checkPallocSum(new runtime_test_package.testing_TжTB(tΔ1), b.Summarize(), h);
            }
        });
    }
}

// Benchmarks how quickly we can summarize a PallocBits.
public static void BenchmarkPallocBitsSummarize(ж<testing.B> b) {
    var patterns = slice<uint64>([
        0,
        ~(uint64)0,
        0xaa,
        0xaaaaaaaaaaaaaaaaUL,
        0x80000000aaaaaaaaUL,
        0xaaaaaaaa00000001UL,
        0xbbbbbbbbbbbbbbbbUL,
        0x80000000bbbbbbbbUL,
        0xbbbbbbbb00000001UL,
        0xccccccccccccccccUL,
        0x4444444444444444UL,
        0x4040404040404040UL,
        0x4000400040004000UL,
        0x1000404044ccaaffUL
    ]);
    foreach (var (_, p) in range(patterns)) {
        var buf = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        for (nint i = 0; i < 8; i++) {
            buf.Value[i] = p;
        }
        var bufʗ1 = buf;
        b.Run(fmt.Sprintf("Unpacked%02X"u8, p), (ж<testing.B> bΔ1) => {
            checkPallocSum(new runtime_test_package.testing_BжTB(bΔ1), bufʗ1.Summarize(), runtime_internal_test_package.SummarizeSlow(bufʗ1));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bufʗ1.Summarize();
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someFree64ˢ => ref someFree64ˢᵇ;
private static readonly @string someFree64ˢᵇ = "SomeFree64"u8;
internal static ref readonly @string exactFit65ˢ => ref exactFit65ˢᵇ;
private static readonly @string exactFit65ˢᵇ = "ExactFit65"u8;
internal static ref readonly @string someFree161ˢ => ref someFree161ˢᵇ;
private static readonly @string someFree161ˢᵇ = "SomeFree161"u8;

internal partial struct TestPallocBitsAlloc_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> before;
    internal slice<global::go.runtime_internal_test_package.BitRange> after;
    internal uintptr npages;
    internal slice<nuint> hits;
}

// Ensures page allocation works.
public static void TestPallocBitsAlloc(ж<testing.T> t) {
    var tests = new map<@string, TestPallocBitsAlloc_test>{
        ["AllFree1"] = new(
            npages: 1,
            hits: [0, 1, 2, 3, 4, 5],
            after: [new(0, 6)]
        ),
        ["AllFree2"] = new(
            npages: 2,
            hits: [0, 2, 4, 6, 8, 10],
            after: [new(0, 12)]
        ),
        ["AllFree5"] = new(
            npages: 5,
            hits: [0, 5, 10, 15, 20],
            after: [new(0, 25)]
        ),
        ["NoneFree1"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages)],
            npages: 1,
            hits: [~(nuint)0, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        ),
        ["NoneFree2"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages)],
            npages: 2,
            hits: [~(nuint)0, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        ),
        ["NoneFree5"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages)],
            npages: 5,
            hits: [~(nuint)0, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        ),
        ["NoneFree65"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages)],
            npages: 65,
            hits: [~(nuint)0, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        ),
        ["ExactFit1"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages / 2 - 3), new(runtime_internal_test_package.PallocChunkPages / 2 - 2, runtime_internal_test_package.PallocChunkPages / 2 + 2)],
            npages: 1,
            hits: [runtime_internal_test_package.PallocChunkPages / 2 - 3, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        ),
        ["ExactFit2"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages / 2 - 3), new(runtime_internal_test_package.PallocChunkPages / 2 - 1, runtime_internal_test_package.PallocChunkPages / 2 + 1)],
            npages: 2,
            hits: [runtime_internal_test_package.PallocChunkPages / 2 - 3, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        ),
        ["ExactFit5"] = new(
            before: [new(0, runtime_internal_test_package.PallocChunkPages / 2 - 3), new(runtime_internal_test_package.PallocChunkPages / 2 + 2, runtime_internal_test_package.PallocChunkPages / 2 - 2)],
            npages: 5,
            hits: [runtime_internal_test_package.PallocChunkPages / 2 - 3, ~(nuint)0],
            after: [new(0, runtime_internal_test_package.PallocChunkPages)]
        )
    };
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        // avoid constant overflow when PallocChunkPages is small
        nuint PallocChunkPages = runtime_internal_test_package.PallocChunkPages;
        tests[allFree64ˢ] = new TestPallocBitsAlloc_test(
            npages: 64,
            hits: [0, 64, 128],
            after: [new(0, 192)]
        );
        tests[allFree65ˢ] = new TestPallocBitsAlloc_test(
            npages: 65,
            hits: [0, 65, 130],
            after: [new(0, 195)]
        );
        tests[someFree64ˢ] = new TestPallocBitsAlloc_test(
            before: [new(0, 32), new(64, 32), new(100, PallocChunkPages - 100)],
            npages: 64,
            hits: [~(nuint)0],
            after: [new(0, 32), new(64, 32), new(100, PallocChunkPages - 100)]
        );
        tests[exactFit65ˢ] = new TestPallocBitsAlloc_test(
            before: [new(0, PallocChunkPages / 2 - 31), new(PallocChunkPages / 2 + 34, PallocChunkPages / 2 - 34)],
            npages: 65,
            hits: [PallocChunkPages / 2 - 31, ~(nuint)0],
            after: [new(0, PallocChunkPages)]
        );
        tests[someFree161ˢ] = new TestPallocBitsAlloc_test(
            before: [new(0, 185), new(331, 1)],
            npages: 161,
            hits: [332],
            after: [new(0, 185), new(331, 162)]
        );
    }
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPallocBitsAlloc_test(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.before);
            foreach (var (iter, i) in range(vʗ1.hits)) {
                var (a, _) = b.Find(vʗ1.npages, 0);
                if (i != a) {
                    tΔ1.Fatalf("find #%d picked wrong index: want %d, got %d", iter + 1, i, a);
                }
                if (i != ~(nuint)0) {
                    b.AllocRange(a, (nuint)vʗ1.npages);
                }
            }
            var want = makePallocBits(vʗ1.after);
            checkPallocBits(tΔ1, b, want);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string someFreeˢ => ref someFreeˢᵇ;
private static readonly @string someFreeˢᵇ = "SomeFree"u8;
internal static ref readonly @string noneFree64ˢ => ref noneFree64ˢᵇ;
private static readonly @string noneFree64ˢᵇ = "NoneFree64"u8;
internal static ref readonly @string noneFree65ˢ => ref noneFree65ˢᵇ;
private static readonly @string noneFree65ˢᵇ = "NoneFree65"u8;

internal partial struct TestPallocBitsFree_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> beforeInv;
    internal slice<global::go.runtime_internal_test_package.BitRange> afterInv;
    internal slice<nuint> frees;
    internal uintptr npages;
}

// Ensures page freeing works.
public static void TestPallocBitsFree(ж<testing.T> t) {
    var tests = new map<@string, TestPallocBitsFree_test>{
        ["NoneFree1"] = new(
            npages: 1,
            frees: [0, 1, 2, 3, 4, 5],
            afterInv: [new(0, 6)]
        ),
        ["NoneFree2"] = new(
            npages: 2,
            frees: [0, 2, 4, 6, 8, 10],
            afterInv: [new(0, 12)]
        ),
        ["NoneFree5"] = new(
            npages: 5,
            frees: [0, 5, 10, 15, 20],
            afterInv: [new(0, 25)]
        )
    };
    if (runtime_internal_test_package.PallocChunkPages >= 512) {
        tests[someFreeˢ] = new TestPallocBitsFree_test(
            npages: 1,
            beforeInv: [new(0, 32), new(64, 32), new(100, 1)],
            frees: [32],
            afterInv: [new(0, 33), new(64, 32), new(100, 1)]
        );
        tests[noneFree64ˢ] = new TestPallocBitsFree_test(
            npages: 64,
            frees: [0, 64, 128],
            afterInv: [new(0, 192)]
        );
        tests[noneFree65ˢ] = new TestPallocBitsFree_test(
            npages: 65,
            frees: [0, 65, 130],
            afterInv: [new(0, 195)]
        );
    }
    foreach (var (name, vᴛ1) in tests) {
        ref var v = ref heap(new TestPallocBitsFree_test(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.beforeInv);
            invertPallocBits(b);
            foreach (var (_, i) in range(vʗ1.frees)) {
                b.Free(i, (nuint)vʗ1.npages);
            }
            var want = makePallocBits(vʗ1.afterInv);
            invertPallocBits(want);
            checkPallocBits(tΔ1, b, want);
        });
    }
}

public static void TestFindBitRange64(ж<testing.T> t) {
    void check(uint64 x, nuint n, nuint result) {
        nuint i = runtime_internal_test_package.FindBitRange64(x, n);
        if (result == ~(nuint)0 && i < 64) {
            t.Errorf("case (%016x, %d): got %d, want failure", x, n, i);
        } else if (result != ~(nuint)0 && i != result) {
            t.Errorf("case (%016x, %d): got %d, want %d", x, n, i, result);
        }
    }
    for (nuint i = (nuint)1; i <= 64; i++) {
        check(~(uint64)0, i, 0);
    }
    for (nuint i = (nuint)1; i <= 64; i++) {
        check(0, i, ~(nuint)0);
    }
    check(0x8000000000000000UL, 1, 63);
    check(0xc000010001010000UL, 2, 62);
    check(0xc000010001030000UL, 2, 16);
    check(0xe000030001030000UL, 3, 61);
    check(0xe000030001070000UL, 3, 16);
    check(0xffff03ff01070000UL, 16, 48);
    check(0xffff03ff0107ffffUL, 16, 0);
    check(0x0fff03ff01079fffUL, 16, ~(nuint)0);
}

public static void BenchmarkFindBitRange64(ж<testing.B> b) {
    var patterns = slice<uint64>([
        0,
        ~(uint64)0,
        0xaa,
        0xaaaaaaaaaaaaaaaaUL,
        0x80000000aaaaaaaaUL,
        0xaaaaaaaa00000001UL,
        0xbbbbbbbbbbbbbbbbUL,
        0x80000000bbbbbbbbUL,
        0xbbbbbbbb00000001UL,
        0xccccccccccccccccUL,
        0x4444444444444444UL,
        0x4040404040404040UL,
        0x4000400040004000UL
    ]);
    var sizes = slice<nuint>([
        2, 8, 32
    ]);
    foreach (var (_, pattern) in range(patterns)) {
        foreach (var (_, size) in range(sizes)) {
            b.Run(fmt.Sprintf("Pattern%02XSize%d"u8, pattern, size), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    runtime_internal_test_package.FindBitRange64(pattern, size);
                }
            });
        }
    }
}

} // end runtime_test_package
