// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.gc;

using scan = go.@internal.runtime.gc.scan_package;
using testing = testing_package;
using go.@internal.runtime.gc;

partial class scan_test_package {

public static void TestFilterNil(ж<testing.T> Ꮡt) {
    runTestFilterNil(Ꮡt, scan.FilterNil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string emptyˢ = "empty"u8;
private static readonly @string oneˢ = "one"u8;
private static readonly @string elimOneˢ = "elimOne"u8;
private static readonly @string oneElimBeginˢ = "oneElimBegin"u8;
private static readonly @string oneElimEndˢ = "oneElimEnd"u8;
private static readonly @string oneElimMultiBeginˢ = "oneElimMultiBegin"u8;
private static readonly @string oneElimMultiEndˢ = "oneElimMultiEnd"u8;
private static readonly @string oneElimMultiˢ = "oneElimMulti"u8;
private static readonly @string twoˢ = "two"u8;
private static readonly @string twoElimBeginˢ = "twoElimBegin"u8;
private static readonly @string twoElimMidˢ = "twoElimMid"u8;
private static readonly @string twoElimEndˢ = "twoElimEnd"u8;
private static readonly @string twoElimMultiˢ = "twoElimMulti"u8;
private static readonly @string multiˢ = "Multi"u8;

internal static void runTestFilterNil(ж<testing.T> Ꮡt, Func<ж<uintptr>, int32, int32> filterNil) {
    Ꮡt.Run(emptyˢ, (ж<testing.T> tΔ1) => {
        testFilterNil(tΔ1, new uintptr[]{}.slice(), new uintptr[]{}.slice(), filterNil);
    });
    Ꮡt.Run(oneˢ, (ж<testing.T> tΔ2) => {
        testFilterNil(tΔ2, new uintptr[]{4}.slice(), new uintptr[]{4}.slice(), filterNil);
    });
    Ꮡt.Run(elimOneˢ, (ж<testing.T> tΔ3) => {
        testFilterNil(tΔ3, new uintptr[]{0}.slice(), new uintptr[]{}.slice(), filterNil);
    });
    Ꮡt.Run(oneElimBeginˢ, (ж<testing.T> tΔ4) => {
        testFilterNil(tΔ4, new uintptr[]{0, 4}.slice(), new uintptr[]{4}.slice(), filterNil);
    });
    Ꮡt.Run(oneElimEndˢ, (ж<testing.T> tΔ5) => {
        testFilterNil(tΔ5, new uintptr[]{4, 0}.slice(), new uintptr[]{4}.slice(), filterNil);
    });
    Ꮡt.Run(oneElimMultiBeginˢ, (ж<testing.T> tΔ6) => {
        testFilterNil(tΔ6, new uintptr[]{0, 0, 0, 4}.slice(), new uintptr[]{4}.slice(), filterNil);
    });
    Ꮡt.Run(oneElimMultiEndˢ, (ж<testing.T> tΔ7) => {
        testFilterNil(tΔ7, new uintptr[]{4, 0, 0, 0}.slice(), new uintptr[]{4}.slice(), filterNil);
    });
    Ꮡt.Run(oneElimMultiˢ, (ж<testing.T> tΔ8) => {
        testFilterNil(tΔ8, new uintptr[]{0, 0, 0, 4, 0}.slice(), new uintptr[]{4}.slice(), filterNil);
    });
    Ꮡt.Run(twoˢ, (ж<testing.T> tΔ9) => {
        testFilterNil(tΔ9, new uintptr[]{5, 12}.slice(), new uintptr[]{5, 12}.slice(), filterNil);
    });
    Ꮡt.Run(twoElimBeginˢ, (ж<testing.T> tΔ10) => {
        testFilterNil(tΔ10, new uintptr[]{0, 5, 12}.slice(), new uintptr[]{5, 12}.slice(), filterNil);
    });
    Ꮡt.Run(twoElimMidˢ, (ж<testing.T> tΔ11) => {
        testFilterNil(tΔ11, new uintptr[]{5, 0, 12}.slice(), new uintptr[]{5, 12}.slice(), filterNil);
    });
    Ꮡt.Run(twoElimEndˢ, (ж<testing.T> tΔ12) => {
        testFilterNil(tΔ12, new uintptr[]{5, 12, 0}.slice(), new uintptr[]{5, 12}.slice(), filterNil);
    });
    Ꮡt.Run(twoElimMultiˢ, (ж<testing.T> tΔ13) => {
        testFilterNil(tΔ13, new uintptr[]{0, 5, 0, 12, 0}.slice(), new uintptr[]{5, 12}.slice(), filterNil);
    });
    Ꮡt.Run(multiˢ, (ж<testing.T> tΔ14) => {
        testFilterNil(tΔ14, new uintptr[]{1, 5, 5, 0, 0, 0, 12, 0, 121, 5, 0}.slice(), new uintptr[]{1, 5, 5, 12, 121, 5}.slice(), filterNil);
    });
}

internal static void testFilterNil(ж<testing.T> Ꮡt, slice<uintptr> buf, slice<uintptr> want, Func<ж<uintptr>, int32, int32> filterNil) {
    ref var t = ref Ꮡt.DerefOrNull();

    ж<uintptr> bufp = default!;
    if (len(buf) != 0) {
        bufp = Ꮡ(buf, 0);
    }
    var n = filterNil(bufp, (int32)len(buf));
    if (n > (int32)len(buf)) {
        Ꮡt.Errorf("bogus new length returned: %d > %d"u8, n, len(buf));
        return;
    }
    buf = buf.slice(0, n);
    if (len(buf) != len(want)) {
        Ꮡt.Errorf("lengths differ: got %d, want %d"u8, len(buf), len(want));
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
            var (nGot, ok) = gotMap[p, ꟷ]; if (!ok){
                Ꮡt.Errorf("want %d, but missing from output"u8, p);
            } else 
            if (nGot != nWant) {
                Ꮡt.Errorf("want %d copies of %d, but got %d"u8, nWant, p, nGot);
            }
        }
    }
    foreach (var (p, _) in gotMap) {
        {
            var (_, ok) = wantMap[p, ꟷ]; if (!ok) {
                Ꮡt.Errorf("got %d, but didn't want it"u8, p);
            }
        }
    }
    Ꮡt.Logf("got:  %v"u8, buf);
    Ꮡt.Logf("want: %v"u8, want);
}

} // end scan_test_package
