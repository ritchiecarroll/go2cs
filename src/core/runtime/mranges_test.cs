// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object differentLengthsˢ = (@string)"different lengths"u8;
internal static readonly object emptyRangeFoundˢ = (@string)"empty range found"u8;
internal static readonly object detectedBadAddrRangesˢ = (@string)"detected bad addrRanges"u8;

internal static void validateAddrRanges(ж<testing.T> Ꮡt, ж<global::go.runtime_internal_test_package.AddrRanges> Ꮡa, params Span<global::go.runtime_internal_test_package.AddrRange> wantʗp) {
    var want = wantʗp.slice();

    ref var a = ref Ꮡa.DerefOrNull();
    var ranges = a.Ranges();
    if (len(ranges) != len(want)) {
        Ꮡt.Errorf("want %v, got %v"u8, want, ranges);
        Ꮡt.Fatal(differentLengthsˢ);
    }
    var gotTotalBytes = (uintptr)0;
    var wantTotalBytes = (uintptr)0;
    foreach (var (i, _) in ranges) {
        gotTotalBytes += ranges[i].Size();
        wantTotalBytes += want[i].Size();
        if (ranges[i].Base() >= ranges[i].Limit()) {
            Ꮡt.Error(emptyRangeFoundˢ);
        }
        // Ensure this is equivalent to what we want.
        if (!ranges[i].ΔEquals(want[i])) {
            Ꮡt.Errorf("range %d: got [0x%x, 0x%x), want [0x%x, 0x%x)"u8, i,
                ranges[i].Base(), ranges[i].Limit(),
                want[i].Base(), want[i].Limit());
        }
        if (i != 0) {
            // Ensure the ranges are sorted.
            if (ranges[i - 1].Base() >= ranges[i].Base()) {
                Ꮡt.Errorf("ranges %d and %d are out of sorted order"u8, i - 1, i);
            }
            // Check for a failure to coalesce.
            if (ranges[i - 1].Limit() == ranges[i].Base()) {
                Ꮡt.Errorf("ranges %d and %d should have coalesced"u8, i - 1, i);
            }
            // Check if any ranges overlap. Because the ranges are sorted
            // by base, it's sufficient to just check neighbors.
            if (ranges[i - 1].Limit() > ranges[i].Base()) {
                Ꮡt.Errorf("ranges %d and %d overlap"u8, i - 1, i);
            }
        }
    }
    if (wantTotalBytes != gotTotalBytes) {
        Ꮡt.Errorf("expected %d total bytes, got %d"u8, wantTotalBytes, gotTotalBytes);
    }
    {
        var b = a.TotalBytes(); if (b != gotTotalBytes) {
            Ꮡt.Errorf("inconsistent total bytes: want %d, got %d"u8, gotTotalBytes, b);
        }
    }
    if (Ꮡt.Failed()) {
        Ꮡt.Errorf("addrRanges: %v"u8, ranges);
        Ꮡt.Fatal(detectedBadAddrRangesˢ);
    }
}

public static void TestAddrRangesAdd(ж<testing.T> Ꮡt) {
    ref var a = ref heap<global::go.runtime_internal_test_package.AddrRanges>(out var Ꮡa);
    a = runtime_internal_test_package.NewAddrRanges();
    // First range.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(512, 1024));
    validateAddrRanges(Ꮡt, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 1024));
    // Coalesce up.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(1024, 2048));
    validateAddrRanges(Ꮡt, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 2048));
    // Add new independent range.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(4096, 8192));
    validateAddrRanges(Ꮡt, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 2048),
        runtime_internal_test_package.MakeAddrRange(4096, 8192));
    // Coalesce down.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(3776, 4096));
    validateAddrRanges(Ꮡt, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 2048),
        runtime_internal_test_package.MakeAddrRange(3776, 8192));
    // Coalesce up and down.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(2048, 3776));
    validateAddrRanges(Ꮡt, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 8192));
    // Push a bunch of independent ranges to the end to try and force growth.
    var expectedRanges = new global::go.runtime_internal_test_package.AddrRange[]{runtime_internal_test_package.MakeAddrRange(512, 8192)}.slice();
    for (var i = (uintptr)0; i < 64; i++) {
        var dRange = runtime_internal_test_package.MakeAddrRange(8192 + (i + 1) * 2048, 8192 + (i + 1) * 2048 + 10);
        Ꮡa.Add(dRange);
        expectedRanges = append(expectedRanges, dRange);
        validateAddrRanges(Ꮡt, Ꮡa, expectedRanges.ꓸꓸꓸ);
    }
    // Push a bunch of independent ranges to the beginning to try and force growth.
    slice<global::go.runtime_internal_test_package.AddrRange> bottomRanges = default!;
    for (var i = (uintptr)0; i < 63; i++) {
        var dRange = runtime_internal_test_package.MakeAddrRange(8 + i * 8, 8 + i * 8 + 4);
        Ꮡa.Add(dRange);
        bottomRanges = append(bottomRanges, dRange);
        validateAddrRanges(Ꮡt, Ꮡa, appendꓸꓸꓸ(bottomRanges, expectedRanges).ꓸꓸꓸ);
    }
}

internal partial struct TestAddrRangesFindSucc_testt /*dyn*/ {
    internal @string name;
    internal uintptr @base;
    internal nint expect;
    internal slice<global::go.runtime_internal_test_package.AddrRange> ranges;
}

public static void TestAddrRangesFindSucc(ж<testing.T> Ꮡt) {
    slice<global::go.runtime_internal_test_package.AddrRange> large = default!;
    for (nint i = 0; i < 100; i++) {
        large = append(large, runtime_internal_test_package.MakeAddrRange(5 + (uintptr)i * 5, 5 + (uintptr)i * 5 + 3));
    }
    var tests = new TestAddrRangesFindSucc_testt[]{
        new(
            name: "Empty"u8,
            @base: 12,
            expect: 0,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{}.slice()
        ),
        new(
            name: "OneBefore"u8,
            @base: 12,
            expect: 0,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(14, 16)
            }.slice()
        ),
        new(
            name: "OneWithin"u8,
            @base: 14,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(14, 16)
            }.slice()
        ),
        new(
            name: "OneAfterLimit"u8,
            @base: 16,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(14, 16)
            }.slice()
        ),
        new(
            name: "OneAfter"u8,
            @base: 17,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(14, 16)
            }.slice()
        ),
        new(
            name: "ThreeBefore"u8,
            @base: 3,
            expect: 0,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            }.slice()
        ),
        new(
            name: "ThreeAfter"u8,
            @base: 24,
            expect: 3,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            }.slice()
        ),
        new(
            name: "ThreeBetween"u8,
            @base: 11,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            }.slice()
        ),
        new(
            name: "ThreeWithin"u8,
            @base: 9,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            }.slice()
        ),
        new(
            name: "Zero"u8,
            @base: 0,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(0, 10)
            }.slice()
        ),
        new(
            name: "Max"u8,
            @base: ~(uintptr)0,
            expect: 1,
            ranges: new global::go.runtime_internal_test_package.AddrRange[]{
                runtime_internal_test_package.MakeAddrRange(~(uintptr)0 - 5, ~(uintptr)0)
            }.slice()
        ),
        new(
            name: "LargeBefore"u8,
            @base: 2,
            expect: 0,
            ranges: large
        ),
        new(
            name: "LargeAfter"u8,
            @base: 5 + (uintptr)len(large) * 5 + 30,
            expect: len(large),
            ranges: large
        ),
        new(
            name: "LargeBetweenLow"u8,
            @base: 14,
            expect: 2,
            ranges: large
        ),
        new(
            name: "LargeBetweenHigh"u8,
            @base: 249,
            expect: 49,
            ranges: large
        ),
        new(
            name: "LargeWithinLow"u8,
            @base: 25,
            expect: 5,
            ranges: large
        ),
        new(
            name: "LargeWithinHigh"u8,
            @base: 396,
            expect: 79,
            ranges: large
        ),
        new(
            name: "LargeWithinMiddle"u8,
            @base: 250,
            expect: 50,
            ranges: large
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestAddrRangesFindSucc_testt(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var a = runtime_internal_test_package.MakeAddrRanges(testʗ1.ranges.ꓸꓸꓸ);
            nint i = a.FindSucc(testʗ1.@base);
            if (i != testʗ1.expect) {
                tΔ1.Fatalf("expected %d, got %d"u8, testʗ1.expect, i);
            }
        });
    }
}

} // end runtime_test_package
