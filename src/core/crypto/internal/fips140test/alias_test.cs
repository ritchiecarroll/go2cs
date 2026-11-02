// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using alias = go.crypto.@internal.fips140.alias_package;
using testing = testing_package;
using go.crypto.@internal.fips140;

partial class fipstest_internal_test_package {

internal static array<byte> a = new(100);
internal static array<byte> b = new(100);

partial struct aliasingTestsᴛ1 /*dyn*/ {
    internal slice<byte> x, y;
    internal bool anyOverlap, inexactOverlap;
}
internal static slice<aliasingTestsᴛ1> aliasingTests = slice<aliasingTestsᴛ1>([
    new(a[..], b[..], false, false),
    new(a[..], b[..0], false, false),
    new(a[..], b[..50], false, false),
    new(a[40..50], a[50..60], false, false),
    new(a[40..50], a[60..70], false, false),
    new(a[..51], a[50..], true, true),
    new(a[..], a[..], true, false),
    new(a[..50], a[..60], true, false),
    new(a[..], default, false, false),
    new(default, default, false, false),
    new(a[..], a[..0], false, false),
    new(a[..10], a.slice(0, 10, 20), true, false),
    new(a[..10], a.slice(5, 10, 20), true, true)
]);

internal static void testAliasing(ж<testing.T> t, nint i, slice<byte> x, slice<byte> y, bool anyOverlap, bool inexactOverlap) {
    var any = alias.AnyOverlap(x, y);
    if (any != anyOverlap) {
        t.Errorf("%d: wrong AnyOverlap result, expected %v, got %v", i, anyOverlap, any);
    }
    var inexact = alias.InexactOverlap(x, y);
    if (inexact != inexactOverlap) {
        t.Errorf("%d: wrong InexactOverlap result, expected %v, got %v", i, inexactOverlap, any);
    }
}

public static void TestAliasing(ж<testing.T> t) {
    foreach (var (i, tt) in aliasingTests) {
        testAliasing(t, i, tt.x, tt.y, tt.anyOverlap, tt.inexactOverlap);
        testAliasing(t, i, tt.y, tt.x, tt.anyOverlap, tt.inexactOverlap);
    }
}

} // end fipstest_internal_test_package
