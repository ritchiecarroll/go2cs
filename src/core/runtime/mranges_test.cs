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

internal static void validateAddrRanges(ж<testing.T> t, ж<global::go.runtime_internal_test_package.AddrRanges> Ꮡa, params Span<global::go.runtime_internal_test_package.AddrRange> wantʗp) {
    var want = wantʗp.slice();
    ref var a = ref Ꮡa.DerefOrNull();

    var ranges = a.Ranges();
    if (len(ranges) != len(want)) {
        t.Errorf("want %v, got %v", want, ranges);
        t.Fatal(differentLengthsˢ);
    }
    var gotTotalBytes = (uintptr)0;
    var wantTotalBytes = (uintptr)0;
    foreach (var i in range(len(ranges))) {
        gotTotalBytes += ranges[i].Size();
        wantTotalBytes += want[i].Size();
        if (ranges[i].Base() >= ranges[i].Limit()) {
            t.Error(emptyRangeFoundˢ);
        }
        // Ensure this is equivalent to what we want.
        if (!ranges[i].ΔEquals(want[i])) {
            t.Errorf("range %d: got [0x%x, 0x%x), want [0x%x, 0x%x)", i,
                ranges[i].Base(), ranges[i].Limit(),
                want[i].Base(), want[i].Limit());
        }
        if (i != 0) {
            // Ensure the ranges are sorted.
            if (ranges[i - 1].Base() >= ranges[i].Base()) {
                t.Errorf("ranges %d and %d are out of sorted order", i - 1, i);
            }
            // Check for a failure to coalesce.
            if (ranges[i - 1].Limit() == ranges[i].Base()) {
                t.Errorf("ranges %d and %d should have coalesced", i - 1, i);
            }
            // Check if any ranges overlap. Because the ranges are sorted
            // by base, it's sufficient to just check neighbors.
            if (ranges[i - 1].Limit() > ranges[i].Base()) {
                t.Errorf("ranges %d and %d overlap", i - 1, i);
            }
        }
    }
    if (wantTotalBytes != gotTotalBytes) {
        t.Errorf("expected %d total bytes, got %d", wantTotalBytes, gotTotalBytes);
    }
    var b = a.TotalBytes(); if (b != gotTotalBytes) {
        t.Errorf("inconsistent total bytes: want %d, got %d", gotTotalBytes, b);
    }
    if (t.Failed()) {
        t.Errorf("addrRanges: %v", ranges);
        t.Fatal(detectedBadAddrRangesˢ);
    }
}

public static void TestAddrRangesAdd(ж<testing.T> t) {
    ref var a = ref heap<global::go.runtime_internal_test_package.AddrRanges>(runtime_internal_test_package.NewAddrRanges(), out var Ꮡa);

    // First range.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(512, 1024));
    validateAddrRanges(t, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 1024));

    // Coalesce up.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(1024, 2048));
    validateAddrRanges(t, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 2048));

    // Add new independent range.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(4096, 8192));
    validateAddrRanges(t, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 2048),
        runtime_internal_test_package.MakeAddrRange(4096, 8192));

    // Coalesce down.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(3776, 4096));
    validateAddrRanges(t, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 2048),
        runtime_internal_test_package.MakeAddrRange(3776, 8192));

    // Coalesce up and down.
    Ꮡa.Add(runtime_internal_test_package.MakeAddrRange(2048, 3776));
    validateAddrRanges(t, Ꮡa,
        runtime_internal_test_package.MakeAddrRange(512, 8192));

    // Push a bunch of independent ranges to the end to try and force growth.
    var expectedRanges = slice<global::go.runtime_internal_test_package.AddrRange>([runtime_internal_test_package.MakeAddrRange(512, 8192)]);
    for (var i = (uintptr)0; i < 64; i++) {
        var dRange = runtime_internal_test_package.MakeAddrRange(8192 + (i + 1) * 2048, 8192 + (i + 1) * 2048 + 10);
        Ꮡa.Add(dRange);
        expectedRanges = append(expectedRanges, dRange);
        validateAddrRanges(t, Ꮡa, expectedRanges.ꓸꓸꓸ);
    }

    // Push a bunch of independent ranges to the beginning to try and force growth.
    slice<global::go.runtime_internal_test_package.AddrRange> bottomRanges = default;
    for (var i = (uintptr)0; i < 63; i++) {
        var dRange = runtime_internal_test_package.MakeAddrRange(8 + i * 8, 8 + i * 8 + 4);
        Ꮡa.Add(dRange);
        bottomRanges = append(bottomRanges, dRange);
        validateAddrRanges(t, Ꮡa, appendꓸꓸꓸ(bottomRanges, expectedRanges).ꓸꓸꓸ);
    }
}

internal partial struct TestAddrRangesFindSucc_testt /*dyn*/ {
    internal @string name;
    internal uintptr @base;
    internal nint expect;
    internal slice<global::go.runtime_internal_test_package.AddrRange> ranges;
}

public static void TestAddrRangesFindSucc(ж<testing.T> t) {
    slice<global::go.runtime_internal_test_package.AddrRange> large = default;
    for (nint i = 0; i < 100; i++) {
        large = append(large, runtime_internal_test_package.MakeAddrRange(5 + (uintptr)i * 5, 5 + (uintptr)i * 5 + 3));
    }
    var tests = slice<TestAddrRangesFindSucc_testt>([
        new(
            name: "Empty",
            @base: 12,
            expect: 0,
            ranges: []
        ),
        new(
            name: "OneBefore",
            @base: 12,
            expect: 0,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(14, 16)
            ])
        ),
        new(
            name: "OneWithin",
            @base: 14,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(14, 16)
            ])
        ),
        new(
            name: "OneAfterLimit",
            @base: 16,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(14, 16)
            ])
        ),
        new(
            name: "OneAfter",
            @base: 17,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(14, 16)
            ])
        ),
        new(
            name: "ThreeBefore",
            @base: 3,
            expect: 0,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            ])
        ),
        new(
            name: "ThreeAfter",
            @base: 24,
            expect: 3,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            ])
        ),
        new(
            name: "ThreeBetween",
            @base: 11,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            ])
        ),
        new(
            name: "ThreeWithin",
            @base: 9,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(6, 10),
                runtime_internal_test_package.MakeAddrRange(12, 16),
                runtime_internal_test_package.MakeAddrRange(19, 22)
            ])
        ),
        new(
            name: "Zero",
            @base: 0,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(0, 10)
            ])
        ),
        new(
            name: "Max",
            @base: ~(uintptr)0,
            expect: 1,
            ranges: slice<global::go.runtime_internal_test_package.AddrRange>([
                runtime_internal_test_package.MakeAddrRange(~(uintptr)0 - 5, ~(uintptr)0)
            ])
        ),
        new(
            name: "LargeBefore",
            @base: 2,
            expect: 0,
            ranges: large
        ),
        new(
            name: "LargeAfter",
            @base: 5 + (uintptr)len(large) * 5 + 30,
            expect: len(large),
            ranges: large
        ),
        new(
            name: "LargeBetweenLow",
            @base: 14,
            expect: 2,
            ranges: large
        ),
        new(
            name: "LargeBetweenHigh",
            @base: 249,
            expect: 49,
            ranges: large
        ),
        new(
            name: "LargeWithinLow",
            @base: 25,
            expect: 5,
            ranges: large
        ),
        new(
            name: "LargeWithinHigh",
            @base: 396,
            expect: 79,
            ranges: large
        ),
        new(
            name: "LargeWithinMiddle",
            @base: 250,
            expect: 50,
            ranges: large
        )
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestAddrRangesFindSucc_testt(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var a = runtime_internal_test_package.MakeAddrRanges(testʗ1.ranges.ꓸꓸꓸ);
            nint i = a.FindSucc(testʗ1.@base);
            if (i != testʗ1.expect) {
                tΔ1.Fatalf("expected %d, got %d", testʗ1.expect, i);
            }
        });
    }
}

} // end runtime_test_package
