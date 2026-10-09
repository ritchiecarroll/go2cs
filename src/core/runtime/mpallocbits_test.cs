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
internal static bool checkPallocBits(ж<testing.T> Ꮡt, ж<global::go.runtime_internal_test_package.ΔPallocBits> Ꮡgot, ж<global::go.runtime_internal_test_package.ΔPallocBits> Ꮡwant) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var got = ref Ꮡgot.DerefOrNull();
    ref var want = ref Ꮡwant.DerefOrNull();

    var d = runtime_internal_test_package.DiffPallocBits(Ꮡgot, Ꮡwant);
    if (len(d) != 0) {
        Ꮡt.Errorf("%d range(s) different"u8, len(d));
        foreach (var (_, bits) in d) {
            Ꮡt.Logf("\t@ bit index %d"u8, bits.I);
            Ꮡt.Logf("\t|  got: %s"u8, runtime_internal_test_package.StringifyPallocBits(Ꮡgot, bits));
            Ꮡt.Logf("\t| want: %s"u8, runtime_internal_test_package.StringifyPallocBits(Ꮡwant, bits));
        }
        return false;
    }
    return true;
}

// makePallocBits produces an initialized PallocBits by setting
// the ranges in s to 1 and the rest to zero.
internal static ж<global::go.runtime_internal_test_package.ΔPallocBits> makePallocBits(slice<global::go.runtime_internal_test_package.BitRange> s) {
    var b = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
    foreach (var (_, v) in s) {
        b.AllocRange(v.I, v.N);
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oneLowˢ = "OneLow"u8;
internal static readonly @string oneHighˢ = "OneHigh"u8;
internal static readonly @string innerˢ = "Inner"u8;
internal static readonly @string alignedˢ = "Aligned"u8;
internal static readonly @string beginˢ = "Begin"u8;
internal static readonly @string endˢ = "End"u8;
internal static readonly @string allˢ = "All"u8;

// Ensures that PallocBits.AllocRange works, which is a fundamental
// method used for testing and initialization since it's used by
// makePallocBits.
public static void TestPallocBitsAllocRange(ж<testing.T> Ꮡt) {
    void test(ж<testing.T> tΔ1, nuint i, nuint n, ж<global::go.runtime_internal_test_package.ΔPallocBits> want) {
        checkPallocBits(tΔ1, makePallocBits(new global::go.runtime_internal_test_package.BitRange[]{new(i, n)}.slice()), want);
    }
    var testʗ1 = test;
    Ꮡt.Run(oneLowˢ, (ж<testing.T> tΔ2) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[0] = 0x1;
        testʗ1(tΔ2, 0, 1, want);
    });
    var testʗ2 = test;
    Ꮡt.Run(oneHighˢ, (ж<testing.T> tΔ3) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[runtime_internal_test_package.PallocChunkPages / 64 - 1] = ((uint64)1 << (int)(63));
        testʗ2(tΔ3, runtime_internal_test_package.PallocChunkPages - 1, 1, want);
    });
    var testʗ3 = test;
    Ꮡt.Run(innerˢ, (ж<testing.T> tΔ4) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[2] = 0x3e;
        testʗ3(tΔ4, 129, 5, want);
    });
    var testʗ4 = test;
    Ꮡt.Run(alignedˢ, (ж<testing.T> tΔ5) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[2] = ~(uint64)0;
        want.Value[3] = ~(uint64)0;
        testʗ4(tΔ5, 128, 128, want);
    });
    var testʗ5 = test;
    Ꮡt.Run(beginˢ, (ж<testing.T> tΔ6) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[0] = ~(uint64)0;
        want.Value[1] = ~(uint64)0;
        want.Value[2] = ~(uint64)0;
        want.Value[3] = ~(uint64)0;
        want.Value[4] = ~(uint64)0;
        want.Value[5] = 0x1;
        testʗ5(tΔ6, 0, 321, want);
    });
    var testʗ6 = test;
    Ꮡt.Run(endˢ, (ж<testing.T> tΔ7) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        want.Value[runtime_internal_test_package.PallocChunkPages / 64 - 1] = ~(uint64)0;
        want.Value[runtime_internal_test_package.PallocChunkPages / 64 - 2] = ~(uint64)0;
        want.Value[runtime_internal_test_package.PallocChunkPages / 64 - 3] = ~(uint64)0;
        want.Value[runtime_internal_test_package.PallocChunkPages / 64 - 4] = ((uint64)1 << (int)(63));
        testʗ6(tΔ7, runtime_internal_test_package.PallocChunkPages - (64 * 3 + 1), 64 * 3 + 1, want);
    });
    var testʗ7 = test;
    Ꮡt.Run(allˢ, (ж<testing.T> tΔ8) => {
        var want = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        foreach (var (i, _) in want.Value) {
            want.Value[i] = ~(uint64)0;
        }
        testʗ7(tΔ8, 0, runtime_internal_test_package.PallocChunkPages, want);
    });
}

// Inverts every bit in the PallocBits.
internal static void invertPallocBits(ж<global::go.runtime_internal_test_package.ΔPallocBits> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var (i, _) in b) {
        b[i] = ~b[i];
    }
}

// Ensures two packed summaries are identical, and reports a detailed description
// of the difference if they're not.
internal static void checkPallocSum(testing.TB t, global::go.runtime_internal_test_package.PallocSum got, global::go.runtime_internal_test_package.PallocSum want) {
    if (got.Start() != want.Start()) {
        t.Errorf("inconsistent start: got %d, want %d"u8, got.Start(), want.Start());
    }
    if (got.Max() != want.Max()) {
        t.Errorf("inconsistent max: got %d, want %d"u8, got.Max(), want.Max());
    }
    if (got.End() != want.End()) {
        t.Errorf("inconsistent end: got %d, want %d"u8, got.End(), want.End());
    }
}

internal partial struct TestMallocBitsPopcntRange_test /*dyn*/ {
    internal nuint i, n; // bit range to popcnt over.
    internal nuint want; // expected popcnt result on that range.
}

internal partial struct TestMallocBitsPopcntRange_tests /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> init; // bit ranges to set to 1 in the bitmap.
    internal slice<TestMallocBitsPopcntRange_test> tests; // a set of popcnt tests to run over the bitmap.
}

public static void TestMallocBitsPopcntRange(ж<testing.T> Ꮡt) {
    var tests = new map<@string, TestMallocBitsPopcntRange_tests>{
        ["None"u8] = new(
            tests: new TestMallocBitsPopcntRange_test[]{
                new(0, 1, 0),
                new(5, 3, 0),
                new(2, 11, 0),
                new(runtime_internal_test_package.PallocChunkPages / 4 + 1, runtime_internal_test_package.PallocChunkPages / 2, 0),
                new(0, runtime_internal_test_package.PallocChunkPages, 0)
            }.slice()
        ),
        ["All"u8] = new(
            init: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            tests: new TestMallocBitsPopcntRange_test[]{
                new(0, 1, 1),
                new(5, 3, 3),
                new(2, 11, 11),
                new(runtime_internal_test_package.PallocChunkPages / 4 + 1, runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2),
                new(0, runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages)
            }.slice()
        ),
        ["Half"u8] = new(
            init: new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages / 2, runtime_internal_test_package.PallocChunkPages / 2)}.slice(),
            tests: new TestMallocBitsPopcntRange_test[]{
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
            }.slice()
        ),
        ["OddBound"u8] = new(
            init: new global::go.runtime_internal_test_package.BitRange[]{new(0, 111)}.slice(),
            tests: new TestMallocBitsPopcntRange_test[]{
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
            }.slice()
        ),
        ["Scattered"u8] = new(
            init: new global::go.runtime_internal_test_package.BitRange[]{
                new(1, 3), new(5, 1), new(7, 1), new(10, 2), new(13, 1), new(15, 4),
                new(21, 1), new(23, 1), new(26, 2), new(30, 5), new(36, 2), new(40, 3),
                new(44, 6), new(51, 1), new(53, 2), new(58, 3), new(63, 1), new(67, 2),
                new(71, 10), new(84, 1), new(89, 7), new(99, 2), new(103, 1), new(107, 2),
                new(111, 1), new(113, 1), new(115, 1), new(118, 1), new(120, 2), new(125, 5)
            }.slice(),
            tests: new TestMallocBitsPopcntRange_test[]{
                new(0, 11, 6),
                new(0, 64, 39),
                new(13, 64, 40),
                new(64, 64, 34),
                new(0, 128, 73),
                new(1, 128, 74),
                new(0, runtime_internal_test_package.PallocChunkPages, 75)
            }.slice()
        )
    };
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestMallocBitsPopcntRange_tests>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.init);
            foreach (var (_, h) in vʗ1.tests) {
                {
                    nuint got = b.PopcntRange(h.i, h.n); if (got != h.want) {
                        tΔ1.Errorf("bad popcnt (i=%d, n=%d): got %d, want %d"u8, h.i, h.n, got, h.want);
                    }
                }
            }
        });
    }
}

// Ensures computing bit summaries works as expected by generating random
// bitmaps and checking against a reference implementation.
public static void TestPallocBitsSummarizeRandom(ж<testing.T> Ꮡt) {
    var b = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
    for (nint i = 0; i < 1000; i++) {
        // Randomize bitmap.
        foreach (var (iΔ1, _) in b.Value) {
            b.Value[iΔ1] = rand.Uint64();
        }
        // Check summary against reference implementation.
        checkPallocSum(new runtime_test_package.testing_TжTB(Ꮡt), b.Summarize(), runtime_internal_test_package.SummarizeSlow(b));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noneFreeˢ = "NoneFree"u8;
internal static readonly @string onlyStartˢ = "OnlyStart"u8;
internal static readonly @string onlyEndˢ = "OnlyEnd"u8;
internal static readonly @string startAndEndˢ = "StartAndEnd"u8;
internal static readonly @string startMaxEndˢ = "StartMaxEnd"u8;
internal static readonly @string onlyMaxˢ = "OnlyMax"u8;
internal static readonly @string multiMaxˢ = "MultiMax"u8;
internal static readonly @string oneˢ = "One"u8;
internal static readonly @string allFreeˢ = "AllFree"u8;

internal partial struct TestPallocBitsSummarize_test /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> free; // Ranges of free (zero) bits.
    internal slice<global::go.runtime_internal_test_package.PallocSum> hits;
}

// Ensures computing bit summaries works as expected.
public static void TestPallocBitsSummarize(ж<testing.T> Ꮡt) {
    global::go.runtime_internal_test_package.PallocSum emptySum = runtime_internal_test_package.PackPallocSum(runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages, runtime_internal_test_package.PallocChunkPages);
    var tests = new map<@string, TestPallocBitsSummarize_test>();
    tests[noneFreeˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(0, 0, 0)
        }.slice()
    );
    tests[onlyStartˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(0, 10)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(10, 10, 0)
        }.slice()
    );
    tests[onlyEndˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(runtime_internal_test_package.PallocChunkPages - 40, 40)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(0, 40, 40)
        }.slice()
    );
    tests[startAndEndˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(0, 11), new(runtime_internal_test_package.PallocChunkPages - 23, 23)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(11, 23, 23)
        }.slice()
    );
    tests[startMaxEndˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(0, 4), new(50, 100), new(runtime_internal_test_package.PallocChunkPages - 4, 4)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(4, 100, 4)
        }.slice()
    );
    tests[onlyMaxˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(1, 20), new(35, 241), new(runtime_internal_test_package.PallocChunkPages - 50, 30)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(0, 241, 0)
        }.slice()
    );
    tests[multiMaxˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(35, 2), new(40, 5), new(100, 5)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(0, 5, 0)
        }.slice()
    );
    tests[oneˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(2, 1)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            runtime_internal_test_package.PackPallocSum(0, 1, 0)
        }.slice()
    );
    tests[allFreeˢ] = new TestPallocBitsSummarize_test(
        free: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
        hits: new global::go.runtime_internal_test_package.PallocSum[]{
            emptySum
        }.slice()
    );
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPallocBitsSummarize_test>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.free);
            // In the PallocBits we create 1's represent free spots, but in our actual
            // PallocBits 1 means not free, so invert.
            invertPallocBits(b);
            foreach (var (_, h) in vʗ1.hits) {
                checkPallocSum(new runtime_test_package.testing_TжTB(tΔ1), b.Summarize(), h);
            }
        });
    }
}

// Benchmarks how quickly we can summarize a PallocBits.
public static void BenchmarkPallocBitsSummarize(ж<testing.B> Ꮡb) {
    var patterns = new uint64[]{
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
    }.slice();
    foreach (var (_, p) in patterns) {
        var buf = @new<global::go.runtime_internal_test_package.ΔPallocBits>();
        for (nint i = 0; i < 8; i++) {
            buf.Value[i] = p;
        }
        var bufʗ1 = buf;
        Ꮡb.Run(fmt.Sprintf("Unpacked%02X"u8, p), (ж<testing.B> bΔ1) => {
            checkPallocSum(new runtime_test_package.testing_BжTB(bΔ1), bufʗ1.Summarize(), runtime_internal_test_package.SummarizeSlow(bufʗ1));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bufʗ1.Summarize();
            }
        });
    }
}

internal partial struct TestPallocBitsAlloc_tests /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> before;
    internal slice<global::go.runtime_internal_test_package.BitRange> after;
    internal uintptr npages;
    internal slice<nuint> hits;
}

// Ensures page allocation works.
public static void TestPallocBitsAlloc(ж<testing.T> Ꮡt) {
    var tests = new map<@string, TestPallocBitsAlloc_tests>{
        ["AllFree1"u8] = new(
            npages: 1,
            hits: new nuint[]{0, 1, 2, 3, 4, 5}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 6)}.slice()
        ),
        ["AllFree2"u8] = new(
            npages: 2,
            hits: new nuint[]{0, 2, 4, 6, 8, 10}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 12)}.slice()
        ),
        ["AllFree5"u8] = new(
            npages: 5,
            hits: new nuint[]{0, 5, 10, 15, 20}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 25)}.slice()
        ),
        ["AllFree64"u8] = new(
            npages: 64,
            hits: new nuint[]{0, 64, 128}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 192)}.slice()
        ),
        ["AllFree65"u8] = new(
            npages: 65,
            hits: new nuint[]{0, 65, 130}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 195)}.slice()
        ),
        ["SomeFree64"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, 32), new(64, 32), new(100, runtime_internal_test_package.PallocChunkPages - 100)}.slice(),
            npages: 64,
            hits: new nuint[]{~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 32), new(64, 32), new(100, runtime_internal_test_package.PallocChunkPages - 100)}.slice()
        ),
        ["NoneFree1"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            npages: 1,
            hits: new nuint[]{~(nuint)0, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["NoneFree2"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            npages: 2,
            hits: new nuint[]{~(nuint)0, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["NoneFree5"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            npages: 5,
            hits: new nuint[]{~(nuint)0, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["NoneFree65"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice(),
            npages: 65,
            hits: new nuint[]{~(nuint)0, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["ExactFit1"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2 - 3), new(runtime_internal_test_package.PallocChunkPages / 2 - 2, runtime_internal_test_package.PallocChunkPages / 2 + 2)}.slice(),
            npages: 1,
            hits: new nuint[]{runtime_internal_test_package.PallocChunkPages / 2 - 3, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["ExactFit2"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2 - 3), new(runtime_internal_test_package.PallocChunkPages / 2 - 1, runtime_internal_test_package.PallocChunkPages / 2 + 1)}.slice(),
            npages: 2,
            hits: new nuint[]{runtime_internal_test_package.PallocChunkPages / 2 - 3, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["ExactFit5"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2 - 3), new(runtime_internal_test_package.PallocChunkPages / 2 + 2, runtime_internal_test_package.PallocChunkPages / 2 - 2)}.slice(),
            npages: 5,
            hits: new nuint[]{runtime_internal_test_package.PallocChunkPages / 2 - 3, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["ExactFit65"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages / 2 - 31), new(runtime_internal_test_package.PallocChunkPages / 2 + 34, runtime_internal_test_package.PallocChunkPages / 2 - 34)}.slice(),
            npages: 65,
            hits: new nuint[]{runtime_internal_test_package.PallocChunkPages / 2 - 31, ~(nuint)0}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, runtime_internal_test_package.PallocChunkPages)}.slice()
        ),
        ["SomeFree161"u8] = new(
            before: new global::go.runtime_internal_test_package.BitRange[]{new(0, 185), new(331, 1)}.slice(),
            npages: 161,
            hits: new nuint[]{332}.slice(),
            after: new global::go.runtime_internal_test_package.BitRange[]{new(0, 185), new(331, 162)}.slice()
        )
    };
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPallocBitsAlloc_tests>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.before);
            foreach (var (iter, i) in vʗ1.hits) {
                var (a, _) = b.Find(vʗ1.npages, 0);
                if (i != a) {
                    tΔ1.Fatalf("find #%d picked wrong index: want %d, got %d"u8, iter + 1, i, a);
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

internal partial struct TestPallocBitsFree_tests /*dyn*/ {
    internal slice<global::go.runtime_internal_test_package.BitRange> beforeInv;
    internal slice<global::go.runtime_internal_test_package.BitRange> afterInv;
    internal slice<nuint> frees;
    internal uintptr npages;
}

// Ensures page freeing works.
public static void TestPallocBitsFree(ж<testing.T> Ꮡt) {
    var tests = new map<@string, TestPallocBitsFree_tests>{
        ["SomeFree"u8] = new(
            npages: 1,
            beforeInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 32), new(64, 32), new(100, 1)}.slice(),
            frees: new nuint[]{32}.slice(),
            afterInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 33), new(64, 32), new(100, 1)}.slice()
        ),
        ["NoneFree1"u8] = new(
            npages: 1,
            frees: new nuint[]{0, 1, 2, 3, 4, 5}.slice(),
            afterInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 6)}.slice()
        ),
        ["NoneFree2"u8] = new(
            npages: 2,
            frees: new nuint[]{0, 2, 4, 6, 8, 10}.slice(),
            afterInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 12)}.slice()
        ),
        ["NoneFree5"u8] = new(
            npages: 5,
            frees: new nuint[]{0, 5, 10, 15, 20}.slice(),
            afterInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 25)}.slice()
        ),
        ["NoneFree64"u8] = new(
            npages: 64,
            frees: new nuint[]{0, 64, 128}.slice(),
            afterInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 192)}.slice()
        ),
        ["NoneFree65"u8] = new(
            npages: 65,
            frees: new nuint[]{0, 65, 130}.slice(),
            afterInv: new global::go.runtime_internal_test_package.BitRange[]{new(0, 195)}.slice()
        )
    };
    foreach (var (name, v) in tests) {
        ref var vΔ1 = ref heap<TestPallocBitsFree_tests>(out var ᏑvΔ1);
        vΔ1 = v;
        var vʗ1 = vΔ1;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            var b = makePallocBits(vʗ1.beforeInv);
            invertPallocBits(b);
            foreach (var (_, i) in vʗ1.frees) {
                b.Free(i, (nuint)vʗ1.npages);
            }
            var want = makePallocBits(vʗ1.afterInv);
            invertPallocBits(want);
            checkPallocBits(tΔ1, b, want);
        });
    }
}

public static void TestFindBitRange64(ж<testing.T> Ꮡt) {
    void check(uint64 x, nuint n, nuint result) {
        nuint i = runtime_internal_test_package.FindBitRange64(x, n);
        if (result == ~(nuint)0 && i < 64){
            Ꮡt.Errorf("case (%016x, %d): got %d, want failure"u8, x, n, i);
        } else 
        if (result != ~(nuint)0 && i != result) {
            Ꮡt.Errorf("case (%016x, %d): got %d, want %d"u8, x, n, i, result);
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

public static void BenchmarkFindBitRange64(ж<testing.B> Ꮡb) {
    var patterns = new uint64[]{
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
    }.slice();
    var sizes = new nuint[]{
        2, 8, 32
    }.slice();
    foreach (var (_, pattern) in patterns) {
        foreach (var (_, size) in sizes) {
            Ꮡb.Run(fmt.Sprintf("Pattern%02XSize%d"u8, pattern, size), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    runtime_internal_test_package.FindBitRange64(pattern, size);
                }
            });
        }
    }
}

} // end runtime_test_package
