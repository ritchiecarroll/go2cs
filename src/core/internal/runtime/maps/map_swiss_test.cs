// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests of map internals that need to use the builtin map type, and thus must
// be built with GOEXPERIMENT=swissmap.
//go:build goexperiment.swissmap
namespace go.@internal.runtime;

using fmt = fmt_package;
using abi = go.@internal.abi_package;
using maps = go.@internal.runtime.maps_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;
using static go.@internal.runtime.maps_internal_test_package;

partial class maps_test_package {

internal static bool alwaysFalse;

internal static any escapeSink;

internal static T escape<T>(T x) {
    if (alwaysFalse) {
        escapeSink = x;
    }
    return x;
}

internal static UntypedInt belowMax => /* abi.SwissMapGroupSlots * 3 / 2 */ 12;                              // 1.5 * group max = 2 groups @ 75%
internal static UntypedInt atMax => /* (2 * abi.SwissMapGroupSlots * maps.MaxAvgGroupLoad) / abi.SwissMapGroupSlots */ 14; // 2 groups at 7/8 full.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapliteralˢ = "mapliteral"u8;
internal static readonly @string escapeˢ = "escape"u8;
internal static readonly @string nohintˢ = "nohint"u8;
internal static readonly @string makemapˢ = "makemap"u8;
internal static readonly @string makemap64ˢ = "makemap64"u8;

internal partial struct TestTableGroupCount_mapCount /*dyn*/ {
    internal nint tables;
    internal uint64 groups;
}

internal partial struct TestTableGroupCount_mapCase /*dyn*/ {
    internal TestTableGroupCount_mapCount initialLit;
    internal TestTableGroupCount_mapCount initialHint;
    internal TestTableGroupCount_mapCount after;
}

internal partial struct TestTableGroupCount_type /*dyn*/ {
    internal nint n;    // n is the number of map elements
    internal TestTableGroupCount_mapCase escape; // expected values for escaping map
}

public static void TestTableGroupCount(ж<testing.T> Ꮡt) {
    // Test that maps of different sizes have the right number of
    // tables/groups.
// 1.5 group max = 2 groups @ 75%
// 2 groups at max
// 2 groups at max + 1 -> grow to 4 groups
// 3 * group max = 4 groups @75%
// 4 groups at max + 1 -> grow to 8 groups
    slice<TestTableGroupCount_type> testCases = new TestTableGroupCount_type[]{
        new(
            n: -((1 << (int)(30))),
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 0)
            )
        ),
        new(
            n: -1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 0)
            )
        ),
        new(
            n: 0,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 0)
            )
        ),
        new(
            n: 1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 1)
            )
        ),
        new(
            n: abi.SwissMapGroupSlots,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(0, 0),
                after: new TestTableGroupCount_mapCount(0, 1)
            )
        ),
        new(
            n: abi.SwissMapGroupSlots + 1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 2),
                after: new TestTableGroupCount_mapCount(1, 2)
            )
        ),
        new(
            n: belowMax,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 2),
                after: new TestTableGroupCount_mapCount(1, 2)
            )
        ),
        new(
            n: atMax,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 2),
                after: new TestTableGroupCount_mapCount(1, 2)
            )
        ),
        new(
            n: atMax + 1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 4),
                after: new TestTableGroupCount_mapCount(1, 4)
            )
        ),
        new(
            n: 2 * belowMax,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 4),
                after: new TestTableGroupCount_mapCount(1, 4)
            )
        ),
        new(
            n: 2 * atMax + 1,
            escape: new TestTableGroupCount_mapCase(
                initialLit: new TestTableGroupCount_mapCount(0, 0),
                initialHint: new TestTableGroupCount_mapCount(1, 8),
                after: new TestTableGroupCount_mapCount(1, 8)
            )
        )
    }.slice();
    void testMap(ж<testing.T> tΔ1, map<nint, nint> mʗp, nint n, TestTableGroupCount_mapCount initial, TestTableGroupCount_mapCount after) {
        ref var m = ref heap(mʗp, out var Ꮡm);
        var mm = ~Ꮡm.Reinterpret<map<nint, nint>, ж<mapsꓸMap>>();
        nint gotTab = mm.TableCount();
        if (gotTab != initial.tables) {
            tΔ1.Errorf("initial TableCount got %d want %d"u8, gotTab, initial.tables);
        }
        var gotGroup = mm.GroupCount();
        if (gotGroup != initial.groups) {
            tΔ1.Errorf("initial GroupCount got %d want %d"u8, gotGroup, initial.groups);
        }
        for (nint i = 0; i < n; i++) {
            m[i] = i;
        }
        gotTab = mm.TableCount();
        if (gotTab != after.tables) {
            tΔ1.Errorf("after TableCount got %d want %d"u8, gotTab, after.tables);
        }
        gotGroup = mm.GroupCount();
        if (gotGroup != after.groups) {
            tΔ1.Errorf("after GroupCount got %d want %d"u8, gotGroup, after.groups);
        }
    }
    var testCasesʗ1 = testCases;
    var testMapʗ1 = testMap;
    Ꮡt.Run(mapliteralˢ, (ж<testing.T> tΔ2) => {
        foreach (var (_, vᴛ1) in testCasesʗ1) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            var testMapʗ2 = testMapʗ1;
            tΔ2.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ3) => {
                var tcʗ2 = tcʗ1;
                var testMapʗ3 = testMapʗ2;
                tΔ3.Run(escapeˢ, (ж<testing.T> tΔ4) => {
                    var m = escape(new map<nint, nint>{});
                    testMapʗ3(tΔ4, m, tcʗ2.n, tcʗ2.escape.initialLit, tcʗ2.escape.after);
                });
            });
        }
    });
    var testCasesʗ2 = testCases;
    var testMapʗ4 = testMap;
    Ꮡt.Run(nohintˢ, (ж<testing.T> tΔ5) => {
        foreach (var (_, vᴛ2) in testCasesʗ2) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ2;

            var tcʗ3 = tc;
            var testMapʗ5 = testMapʗ4;
            tΔ5.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ6) => {
                var tcʗ4 = tcʗ3;
                var testMapʗ6 = testMapʗ5;
                tΔ6.Run(escapeˢ, (ж<testing.T> tΔ7) => {
                    var m = escape(new map<nint, nint>());
                    testMapʗ6(tΔ7, m, tcʗ4.n, tcʗ4.escape.initialLit, tcʗ4.escape.after);
                });
            });
        }
    });
    var testCasesʗ3 = testCases;
    var testMapʗ7 = testMap;
    Ꮡt.Run(makemapˢ, (ж<testing.T> tΔ8) => {
        foreach (var (_, vᴛ3) in testCasesʗ3) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ3;

            var tcʗ5 = tc;
            var testMapʗ8 = testMapʗ7;
            tΔ8.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ9) => {
                var tcʗ6 = tcʗ5;
                var testMapʗ9 = testMapʗ8;
                tΔ9.Run(escapeˢ, (ж<testing.T> tΔ10) => {
                    var m = escape(new map<nint, nint>(tcʗ6.n));
                    testMapʗ9(tΔ10, m, tcʗ6.n, tcʗ6.escape.initialHint, tcʗ6.escape.after);
                });
            });
        }
    });
    var testCasesʗ4 = testCases;
    var testMapʗ10 = testMap;
    Ꮡt.Run(makemap64ˢ, (ж<testing.T> tΔ11) => {
        foreach (var (_, vᴛ4) in testCasesʗ4) {
            ref var tc = ref heap(new TestTableGroupCount_type(), out var Ꮡtc);
            tc = vᴛ4;

            var tcʗ7 = tc;
            var testMapʗ11 = testMapʗ10;
            tΔ11.Run(fmt.Sprintf("n=%d"u8, tc.n), (ж<testing.T> tΔ12) => {
                var tcʗ8 = tcʗ7;
                var testMapʗ12 = testMapʗ11;
                tΔ12.Run(escapeˢ, (ж<testing.T> tΔ13) => {
                    var m = escape(new map<nint, nint>((nint)((int64)tcʗ8.n)));
                    testMapʗ12(tΔ13, m, tcʗ8.n, tcʗ8.escape.initialHint, tcʗ8.escape.after);
                });
            });
        }
    });
}

} // end maps_test_package
