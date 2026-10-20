// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using testing = testing_package;
using quick = go.testing.quick_package;
using go.testing;
using static go.crypto.@internal.fips140.edwards25519_package;

partial class edwards25519_internal_test_package {

public static void TestScalarAliasing(ж<testing.T> Ꮡt) {
    bool checkAliasingOneArg(Func<ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>> f, global::go.crypto.@internal.fips140.edwards25519_package.Scalar vʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp) {
        ref var v = ref heap(vʗp.ΔClone(), out var Ꮡv);
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        var x1 = x.ΔClone();
        ref var v1 = ref heap<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>(out var Ꮡv1);
        v1 = x.ΔClone();

        // Calculate a reference f(x) without aliasing.
        {
            var @out = f(Ꮡv, Ꮡx); if (@out != Ꮡv || !isReduced(@out.Bytes())) {
                return false;
            }
        }

        // Test aliasing the argument and the receiver.
        {
            var @out = f(Ꮡv1, Ꮡv1); if (@out != Ꮡv1 || v1 != v || !isReduced(@out.Bytes())) {
                return false;
            }
        }

        // Ensure the arguments was not modified.
        return x == x1;
    }

    bool checkAliasingTwoArgs(Func<ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>> f, global::go.crypto.@internal.fips140.edwards25519_package.Scalar vʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar yʗp) {
        ref var v = ref heap(vʗp.ΔClone(), out var Ꮡv);
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var y = ref heap(yʗp.ΔClone(), out var Ꮡy);
        var x1 = x.ΔClone();
        var y1 = y.ΔClone();
        ref var v1 = ref heap<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>(out var Ꮡv1);
        v1 = new Scalar(nil);

        // Calculate a reference f(x, y) without aliasing.
        {
            var @out = f(Ꮡv, Ꮡx, Ꮡy); if (@out != Ꮡv || !isReduced(@out.Bytes())) {
                return false;
            }
        }

        // Test aliasing the first argument and the receiver.
        v1 = x.ΔClone();
        {
            var @out = f(Ꮡv1, Ꮡv1, Ꮡy); if (@out != Ꮡv1 || v1 != v || !isReduced(@out.Bytes())) {
                return false;
            }
        }
        // Test aliasing the second argument and the receiver.
        v1 = y.ΔClone();
        {
            var @out = f(Ꮡv1, Ꮡx, Ꮡv1); if (@out != Ꮡv1 || v1 != v || !isReduced(@out.Bytes())) {
                return false;
            }
        }

        // Calculate a reference f(x, x) without aliasing.
        {
            var @out = f(Ꮡv, Ꮡx, Ꮡx); if (@out != Ꮡv || !isReduced(@out.Bytes())) {
                return false;
            }
        }

        // Test aliasing the first argument and the receiver.
        v1 = x.ΔClone();
        {
            var @out = f(Ꮡv1, Ꮡv1, Ꮡx); if (@out != Ꮡv1 || v1 != v || !isReduced(@out.Bytes())) {
                return false;
            }
        }
        // Test aliasing the second argument and the receiver.
        v1 = x.ΔClone();
        {
            var @out = f(Ꮡv1, Ꮡx, Ꮡv1); if (@out != Ꮡv1 || v1 != v || !isReduced(@out.Bytes())) {
                return false;
            }
        }
        // Test aliasing both arguments and the receiver.
        v1 = x.ΔClone();
        {
            var @out = f(Ꮡv1, Ꮡv1, Ꮡv1); if (@out != Ꮡv1 || v1 != v || !isReduced(@out.Bytes())) {
                return false;
            }
        }

        // Ensure the arguments were not modified.
        return x == x1 && y == y1;
    }

        var checkAliasingOneArgʗ1 = checkAliasingOneArg;

        var checkAliasingTwoArgsʗ1 = checkAliasingTwoArgs;

        var checkAliasingTwoArgsʗ2 = checkAliasingTwoArgs;

        var checkAliasingTwoArgsʗ3 = checkAliasingTwoArgs;

        var checkAliasingTwoArgsʗ4 = checkAliasingTwoArgs;

        var checkAliasingTwoArgsʗ5 = checkAliasingTwoArgs;

        var checkAliasingTwoArgsʗ6 = checkAliasingTwoArgs;
    foreach (var (name, f) in new map<@string, any>{
        ["Negate"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x) => {
            v = v.ΔClone();
            x = x.ΔClone();
            return checkAliasingOneArgʗ1(((Func<ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>>)(global::go.crypto.@internal.fips140.edwards25519_package.Negate)), v, x);
        },
        ["Multiply"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x, global::go.crypto.@internal.fips140.edwards25519_package.Scalar y) => {
            v = v.ΔClone();
            x = x.ΔClone();
            y = y.ΔClone();
            return checkAliasingTwoArgsʗ1(((Func<ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>>)(global::go.crypto.@internal.fips140.edwards25519_package.Multiply)), v, x, y);
        },
        ["Add"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x, global::go.crypto.@internal.fips140.edwards25519_package.Scalar y) => {
            v = v.ΔClone();
            x = x.ΔClone();
            y = y.ΔClone();
            return checkAliasingTwoArgsʗ2(((Func<ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>>)(global::go.crypto.@internal.fips140.edwards25519_package.Add)), v, x, y);
        },
        ["Subtract"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x, global::go.crypto.@internal.fips140.edwards25519_package.Scalar y) => {
            v = v.ΔClone();
            x = x.ΔClone();
            y = y.ΔClone();
            return checkAliasingTwoArgsʗ3(((Func<ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>>)(global::go.crypto.@internal.fips140.edwards25519_package.Subtract)), v, x, y);
        },
        ["MultiplyAdd1"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x, global::go.crypto.@internal.fips140.edwards25519_package.Scalar y, global::go.crypto.@internal.fips140.edwards25519_package.Scalar @fixedʗp) => {
            v = v.ΔClone();
            x = x.ΔClone();
            y = y.ΔClone();
            ref var @fixed = ref heap(@fixedʗp.ΔClone(), out var Ꮡfixed);
            return checkAliasingTwoArgsʗ4((ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> vΔ1, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> xΔ1, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> yΔ1) => vΔ1.MultiplyAdd(Ꮡfixed, xΔ1, yΔ1), v, x, y);
        },
        ["MultiplyAdd2"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x, global::go.crypto.@internal.fips140.edwards25519_package.Scalar y, global::go.crypto.@internal.fips140.edwards25519_package.Scalar @fixedʗp) => {
            v = v.ΔClone();
            x = x.ΔClone();
            y = y.ΔClone();
            ref var @fixed = ref heap(@fixedʗp.ΔClone(), out var Ꮡfixed);
            return checkAliasingTwoArgsʗ5((ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> vΔ1, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> xΔ1, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> yΔ1) => vΔ1.MultiplyAdd(xΔ1, Ꮡfixed, yΔ1), v, x, y);
        },
        ["MultiplyAdd3"u8] = bool (global::go.crypto.@internal.fips140.edwards25519_package.Scalar v, global::go.crypto.@internal.fips140.edwards25519_package.Scalar x, global::go.crypto.@internal.fips140.edwards25519_package.Scalar y, global::go.crypto.@internal.fips140.edwards25519_package.Scalar @fixedʗp) => {
            v = v.ΔClone();
            x = x.ΔClone();
            y = y.ΔClone();
            ref var @fixed = ref heap(@fixedʗp.ΔClone(), out var Ꮡfixed);
            return checkAliasingTwoArgsʗ6((ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> vΔ1, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> xΔ1, ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> yΔ1) => vΔ1.MultiplyAdd(xΔ1, yΔ1, Ꮡfixed), v, x, y);
        }
    }) {
        var err = quick.Check(f, quickCheckConfig(32));
        if (err != default!) {
            Ꮡt.Errorf("%v: %v"u8, name, err);
        }
    }
}

} // end edwards25519_internal_test_package
