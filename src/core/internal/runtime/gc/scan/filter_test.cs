// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using scan = go.@internal.runtime.gc.scan_package;
using testing = testing_package;
using go.@internal.runtime.gc;

partial class scan_test_package {

public static void TestFilterNil(ж<testing.T> t) {
    runTestFilterNil(t, scan.FilterNil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string emptyˢ => ref emptyˢᵇ;
private static readonly @string emptyˢᵇ = "empty"u8;
private static ref readonly @string oneˢ => ref oneˢᵇ;
private static readonly @string oneˢᵇ = "one"u8;
private static ref readonly @string elimOneˢ => ref elimOneˢᵇ;
private static readonly @string elimOneˢᵇ = "elimOne"u8;
private static ref readonly @string oneElimBeginˢ => ref oneElimBeginˢᵇ;
private static readonly @string oneElimBeginˢᵇ = "oneElimBegin"u8;
private static ref readonly @string oneElimEndˢ => ref oneElimEndˢᵇ;
private static readonly @string oneElimEndˢᵇ = "oneElimEnd"u8;
private static ref readonly @string oneElimMultiBeginˢ => ref oneElimMultiBeginˢᵇ;
private static readonly @string oneElimMultiBeginˢᵇ = "oneElimMultiBegin"u8;
private static ref readonly @string oneElimMultiEndˢ => ref oneElimMultiEndˢᵇ;
private static readonly @string oneElimMultiEndˢᵇ = "oneElimMultiEnd"u8;
private static ref readonly @string oneElimMultiˢ => ref oneElimMultiˢᵇ;
private static readonly @string oneElimMultiˢᵇ = "oneElimMulti"u8;
private static ref readonly @string twoˢ => ref twoˢᵇ;
private static readonly @string twoˢᵇ = "two"u8;
private static ref readonly @string twoElimBeginˢ => ref twoElimBeginˢᵇ;
private static readonly @string twoElimBeginˢᵇ = "twoElimBegin"u8;
private static ref readonly @string twoElimMidˢ => ref twoElimMidˢᵇ;
private static readonly @string twoElimMidˢᵇ = "twoElimMid"u8;
private static ref readonly @string twoElimEndˢ => ref twoElimEndˢᵇ;
private static readonly @string twoElimEndˢᵇ = "twoElimEnd"u8;
private static ref readonly @string twoElimMultiˢ => ref twoElimMultiˢᵇ;
private static readonly @string twoElimMultiˢᵇ = "twoElimMulti"u8;
private static ref readonly @string multiˢ => ref multiˢᵇ;
private static readonly @string multiˢᵇ = "Multi"u8;

internal static void runTestFilterNil(ж<testing.T> t, Func<ж<uintptr>, int32, int32> filterNil) {
    t.Run(emptyˢ, (ж<testing.T> tΔ1) => {
        testFilterNil(tΔ1, slice<uintptr>([]), slice<uintptr>([]), filterNil);
    });
    t.Run(oneˢ, (ж<testing.T> tΔ2) => {
        testFilterNil(tΔ2, slice<uintptr>([4]), slice<uintptr>([4]), filterNil);
    });
    t.Run(elimOneˢ, (ж<testing.T> tΔ3) => {
        testFilterNil(tΔ3, slice<uintptr>([0]), slice<uintptr>([]), filterNil);
    });
    t.Run(oneElimBeginˢ, (ж<testing.T> tΔ4) => {
        testFilterNil(tΔ4, slice<uintptr>([0, 4]), slice<uintptr>([4]), filterNil);
    });
    t.Run(oneElimEndˢ, (ж<testing.T> tΔ5) => {
        testFilterNil(tΔ5, slice<uintptr>([4, 0]), slice<uintptr>([4]), filterNil);
    });
    t.Run(oneElimMultiBeginˢ, (ж<testing.T> tΔ6) => {
        testFilterNil(tΔ6, slice<uintptr>([0, 0, 0, 4]), slice<uintptr>([4]), filterNil);
    });
    t.Run(oneElimMultiEndˢ, (ж<testing.T> tΔ7) => {
        testFilterNil(tΔ7, slice<uintptr>([4, 0, 0, 0]), slice<uintptr>([4]), filterNil);
    });
    t.Run(oneElimMultiˢ, (ж<testing.T> tΔ8) => {
        testFilterNil(tΔ8, slice<uintptr>([0, 0, 0, 4, 0]), slice<uintptr>([4]), filterNil);
    });
    t.Run(twoˢ, (ж<testing.T> tΔ9) => {
        testFilterNil(tΔ9, slice<uintptr>([5, 12]), slice<uintptr>([5, 12]), filterNil);
    });
    t.Run(twoElimBeginˢ, (ж<testing.T> tΔ10) => {
        testFilterNil(tΔ10, slice<uintptr>([0, 5, 12]), slice<uintptr>([5, 12]), filterNil);
    });
    t.Run(twoElimMidˢ, (ж<testing.T> tΔ11) => {
        testFilterNil(tΔ11, slice<uintptr>([5, 0, 12]), slice<uintptr>([5, 12]), filterNil);
    });
    t.Run(twoElimEndˢ, (ж<testing.T> tΔ12) => {
        testFilterNil(tΔ12, slice<uintptr>([5, 12, 0]), slice<uintptr>([5, 12]), filterNil);
    });
    t.Run(twoElimMultiˢ, (ж<testing.T> tΔ13) => {
        testFilterNil(tΔ13, slice<uintptr>([0, 5, 0, 12, 0]), slice<uintptr>([5, 12]), filterNil);
    });
    t.Run(multiˢ, (ж<testing.T> tΔ14) => {
        testFilterNil(tΔ14, slice<uintptr>([1, 5, 5, 0, 0, 0, 12, 0, 121, 5, 0]), slice<uintptr>([1, 5, 5, 12, 121, 5]), filterNil);
    });
}

internal static void testFilterNil(ж<testing.T> t, slice<uintptr> buf, slice<uintptr> want, Func<ж<uintptr>, int32, int32> filterNil) {
    ж<uintptr> bufp = default;
    if (len(buf) != 0) {
        bufp = Ꮡ(buf, 0);
    }
    var n = filterNil(bufp, (int32)len(buf));
    if (n > (int32)len(buf)) {
        t.Errorf("bogus new length returned: %d > %d", n, len(buf));
        return;
    }
    buf = buf.slice(0, n);
    if (len(buf) != len(want)) {
        t.Errorf("lengths differ: got %d, want %d", len(buf), len(want));
    }

    var wantMap = new map<uintptr, nint>();
    var gotMap = new map<uintptr, nint>();
    foreach (var (_, p) in want) {
        wantMap[p]++;
    }
    foreach (var (_, p) in buf) {
        gotMap[p]++;
    }
    foreach (var (p, nWant) in wantMap) {
        {
            var (nGot, ok) = gotMap[p, ꟷ]; if (!ok) {
                t.Errorf("want %d, but missing from output", p);
            } else if (nGot != nWant) {
                t.Errorf("want %d copies of %d, but got %d", nWant, p, nGot);
            }
        }
    }
    foreach (var (p, _) in gotMap) {
        {
            var (_, ok) = wantMap[p, ꟷ]; if (!ok) {
                t.Errorf("got %d, but didn't want it", p);
            }
        }
    }
    t.Logf("got:  %v", buf);
    t.Logf("want: %v", want);
}

} // end scan_test_package
