// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using slices = slices_package;
using testing = testing_package;
using static go.@internal.zstd_package;

partial class zstd_internal_test_package {

// literalPredefinedDistribution is the predefined distribution table
// for literal lengths. RFC 3.1.1.3.2.2.1.
internal static slice<int16> literalPredefinedDistribution = [
    4, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1,
    2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 1, 1, 1, 1, 1,
    (int16)(-1), (int16)(-1), (int16)(-1), (int16)(-1)
];

// offsetPredefinedDistribution is the predefined distribution table
// for offsets. RFC 3.1.1.3.2.2.3.
internal static slice<int16> offsetPredefinedDistribution = [
    1, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1,
    1, 1, 1, 1, 1, 1, 1, 1, (int16)(-1), (int16)(-1), (int16)(-1), (int16)(-1), (int16)(-1)
];

// matchPredefinedDistribution is the predefined distribution table
// for match lengths. RFC 3.1.1.3.2.2.2.
internal static slice<int16> matchPredefinedDistribution = [
    1, 4, 3, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1,
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, (int16)(-1), (int16)(-1),
    (int16)(-1), (int16)(-1), (int16)(-1), (int16)(-1), (int16)(-1)
];

internal partial struct TestPredefinedTables_tests /*dyn*/ {
    internal @string name;
    internal slice<int16> distribution;
    internal nint tableBits;
    internal Func<ж<global::go.@internal.zstd_package.Reader>, nint, slice<global::go.@internal.zstd_package.fseEntry>, slice<global::go.@internal.zstd_package.fseBaselineEntry>, error> toBaseline;
    internal slice<global::go.@internal.zstd_package.fseBaselineEntry> predef;
}

// TestPredefinedTables verifies that we can generate the predefined
// literal/offset/match tables from the input data in RFC 8878.
// This serves as a test of the predefined tables, and also of buildFSE
// and the functions that make baseline FSE tables.
public static void TestPredefinedTables(ж<testing.T> t) {
    var tests = slice<TestPredefinedTables_tests>([
        new(
            name: "literal",
            distribution: literalPredefinedDistribution,
            tableBits: 6,
            toBaseline: ((Func<ж<global::go.@internal.zstd_package.Reader>, nint, slice<global::go.@internal.zstd_package.fseEntry>, slice<global::go.@internal.zstd_package.fseBaselineEntry>, error>)(global::go.@internal.zstd_package.makeLiteralBaselineFSE)),
            predef: predefinedLiteralTable[..]
        ),
        new(
            name: "offset",
            distribution: offsetPredefinedDistribution,
            tableBits: 5,
            toBaseline: ((Func<ж<global::go.@internal.zstd_package.Reader>, nint, slice<global::go.@internal.zstd_package.fseEntry>, slice<global::go.@internal.zstd_package.fseBaselineEntry>, error>)(global::go.@internal.zstd_package.makeOffsetBaselineFSE)),
            predef: predefinedOffsetTable[..]
        ),
        new(
            name: "match",
            distribution: matchPredefinedDistribution,
            tableBits: 6,
            toBaseline: ((Func<ж<global::go.@internal.zstd_package.Reader>, nint, slice<global::go.@internal.zstd_package.fseEntry>, slice<global::go.@internal.zstd_package.fseBaselineEntry>, error>)(global::go.@internal.zstd_package.makeMatchBaselineFSE)),
            predef: predefinedMatchTable[..]
        )
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var test = ref heap(new TestPredefinedTables_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            ref var r = ref heap(new global::go.@internal.zstd_package.Reader(), out var Ꮡr);
            var table = new slice<global::go.@internal.zstd_package.fseEntry>(((nint)1).Lsh((int64)(testʗ1.tableBits)));
            {
                var err = r.buildFSE(0, testʗ1.distribution, table, testʗ1.tableBits); if (err != nil) {
                    tΔ1.Fatal(err);
                }
            }

            var baselineTable = new slice<global::go.@internal.zstd_package.fseBaselineEntry>(builtin.len(table));
            {
                var err = testʗ1.toBaseline(Ꮡr, 0, table, baselineTable); if (err != nil) {
                    tΔ1.Fatal(err);
                }
            }

            if (!slices.Equal<slice<global::go.@internal.zstd_package.fseBaselineEntry>, global::go.@internal.zstd_package.fseBaselineEntry>(baselineTable, testʗ1.predef)) {
                tΔ1.Errorf("got %v, want %v", baselineTable, testʗ1.predef);
            }
        });
    }
}

} // end zstd_internal_test_package
