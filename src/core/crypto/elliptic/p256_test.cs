// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using big = math.big_package;
using testing = testing_package;
using math;
using static go.crypto.elliptic_package;

partial class elliptic_internal_test_package {

internal partial struct scalarMultTest {
    internal @string k;
    internal @string xIn, yIn;
    internal @string xOut, yOut;
}

internal static slice<scalarMultTest> p256MultTests = new scalarMultTest[]{
    new(
        "2a265f8bcbdcaf94d58519141e578124cb40d64a501fba9c11847b28965bc737"u8,
        "023819813ac969847059028ea88a1f30dfbcde03fc791d3a252c6b41211882ea"u8,
        "f93e4ae433cc12cf2a43fc0ef26400c0e125508224cdb649380f25479148a4ad"u8,
        "4d4de80f1534850d261075997e3049321a0864082d24a917863366c0724f5ae3"u8,
        "a22d2b7f7818a3563e0f7a76c9bf0921ac55e06e2e4d11795b233824b1db8cc0"u8
    ),
    new(
        "313f72ff9fe811bf573176231b286a3bdb6f1b14e05c40146590727a71c3bccd"u8,
        "cc11887b2d66cbae8f4d306627192522932146b42f01d3c6f92bd5c8ba739b06"u8,
        "a2f08a029cd06b46183085bae9248b0ed15b70280c7ef13a457f5af382426031"u8,
        "831c3f6b5f762d2f461901577af41354ac5f228c2591f84f8a6e51e2e3f17991"u8,
        "93f90934cd0ef2c698cc471c60a93524e87ab31ca2412252337f364513e43684"u8
    )
}.slice();

public static void TestP256BaseMult(ж<testing.T> Ꮡt) {
    var scalarsᴷ = new stackAppend<ж<bigꓸInt>>();

    var p256 = P256();
    var p256Generic = genericParamsForCurve(p256);

    var scalars = new slice<ж<bigꓸInt>>(0, len(p224BaseMultTests) + 1);
    foreach (var (_, e) in p224BaseMultTests) {
        var (kΔ1, _) = @new<bigꓸInt>().SetString(e.k, 10);
        scalars = append(scalarsᴷ, scalars, kΔ1);
    }
    var k = @new<bigꓸInt>().SetInt64(1);
    k.Lsh(k, 500);
    scalars = append(scalars, k);

    foreach (var (i, kΔ2) in scalars) {
        var (x, y) = p256.ScalarBaseMult(kΔ2.Bytes());
        var (x2, y2) = p256Generic.ScalarBaseMult(kΔ2.Bytes());
        if (x.Cmp(x2) != 0 || y.Cmp(y2) != 0) {
            Ꮡt.Errorf("#%d: got (%x, %x), want (%x, %x)"u8, i, x.OrTypedNil(), y.OrTypedNil(), x2.OrTypedNil(), y2.OrTypedNil());
        }

        if (testing.Short() && i > 5) {
            break;
        }
    }
}

public static void TestP256Mult(ж<testing.T> Ꮡt) {
    var p256 = P256();
    foreach (var (i, e) in p256MultTests) {
        var (x, _) = @new<bigꓸInt>().SetString(e.xIn, 16);
        var (y, _) = @new<bigꓸInt>().SetString(e.yIn, 16);
        var (k, _) = @new<bigꓸInt>().SetString(e.k, 16);
        var (expectedX, _) = @new<bigꓸInt>().SetString(e.xOut, 16);
        var (expectedY, _) = @new<bigꓸInt>().SetString(e.yOut, 16);

        var (xx, yy) = p256.ScalarMult(x, y, k.Bytes());
        if (xx.Cmp(expectedX) != 0 || yy.Cmp(expectedY) != 0) {
            Ꮡt.Errorf("#%d: got (%x, %x), want (%x, %x)"u8, i, xx.OrTypedNil(), yy.OrTypedNil(), expectedX.OrTypedNil(), expectedY.OrTypedNil());
        }
    }
}

public static void TestIssue52075(ж<testing.T> Ꮡt) {
    var (Gx, Gy) = (P256().Params().Value.Gx, P256().Params().Value.Gy);
    var scalar = new slice<byte>(33);
    scalar[32] = 1;
    var (x, y) = P256().ScalarBaseMult(scalar);
    if (x.Cmp(Gx) != 0 || y.Cmp(Gy) != 0) {
        Ꮡt.Errorf("unexpected output (%v,%v)"u8, x.OrTypedNil(), y.OrTypedNil());
    }
    (x, y) = P256().ScalarMult(Gx, Gy, scalar);
    if (x.Cmp(Gx) != 0 || y.Cmp(Gy) != 0) {
        Ꮡt.Errorf("unexpected output (%v,%v)"u8, x.OrTypedNil(), y.OrTypedNil());
    }
}

} // end elliptic_internal_test_package
