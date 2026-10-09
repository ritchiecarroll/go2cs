// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.edwards25519;

using testing = testing_package;
using quick = go.testing.quick_package;
using go.testing;
using static go.crypto.@internal.fips140.edwards25519.field_package;

partial class field_internal_test_package {

internal static Func<global::go.crypto.@internal.fips140.edwards25519.field_package.Element, global::go.crypto.@internal.fips140.edwards25519.field_package.Element, bool> checkAliasingOneArg(Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>> f) {
    return (global::go.crypto.@internal.fips140.edwards25519.field_package.Element vʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element xʗp) => {
        ref var v = ref heap(vʗp, out var Ꮡv);
        ref var x = ref heap(xʗp, out var Ꮡx);
        var x1 = x;
        ref var v1 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡv1);
        v1 = x;
        // Calculate a reference f(x) without aliasing.
        {
            var @out = f(Ꮡv, Ꮡx); if (@out != Ꮡv && isInBounds(@out)) {
                return false;
            }
        }
        // Test aliasing the argument and the receiver.
        {
            var @out = f(Ꮡv1, Ꮡv1); if (@out != Ꮡv1 || v1 != v) {
                return false;
            }
        }
        // Ensure the arguments was not modified.
        return x == x1;
    };
}

internal static Func<global::go.crypto.@internal.fips140.edwards25519.field_package.Element, global::go.crypto.@internal.fips140.edwards25519.field_package.Element, global::go.crypto.@internal.fips140.edwards25519.field_package.Element, bool> checkAliasingTwoArgs(Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>> f) {
    return (global::go.crypto.@internal.fips140.edwards25519.field_package.Element vʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element xʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element yʗp) => {
        ref var v = ref heap(vʗp, out var Ꮡv);
        ref var x = ref heap(xʗp, out var Ꮡx);
        ref var y = ref heap(yʗp, out var Ꮡy);
        var x1 = x;
        var y1 = y;
        ref var v1 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡv1);
        v1 = new Element(nil);
        // Calculate a reference f(x, y) without aliasing.
        {
            var @out = f(Ꮡv, Ꮡx, Ꮡy); if (@out != Ꮡv && isInBounds(@out)) {
                return false;
            }
        }
        // Test aliasing the first argument and the receiver.
        v1 = x;
        {
            var @out = f(Ꮡv1, Ꮡv1, Ꮡy); if (@out != Ꮡv1 || v1 != v) {
                return false;
            }
        }
        // Test aliasing the second argument and the receiver.
        v1 = y;
        {
            var @out = f(Ꮡv1, Ꮡx, Ꮡv1); if (@out != Ꮡv1 || v1 != v) {
                return false;
            }
        }
        // Calculate a reference f(x, x) without aliasing.
        {
            var @out = f(Ꮡv, Ꮡx, Ꮡx); if (@out != Ꮡv) {
                return false;
            }
        }
        // Test aliasing the first argument and the receiver.
        v1 = x;
        {
            var @out = f(Ꮡv1, Ꮡv1, Ꮡx); if (@out != Ꮡv1 || v1 != v) {
                return false;
            }
        }
        // Test aliasing the second argument and the receiver.
        v1 = x;
        {
            var @out = f(Ꮡv1, Ꮡx, Ꮡv1); if (@out != Ꮡv1 || v1 != v) {
                return false;
            }
        }
        // Test aliasing both arguments and the receiver.
        v1 = x;
        {
            var @out = f(Ꮡv1, Ꮡv1, Ꮡv1); if (@out != Ꮡv1 || v1 != v) {
                return false;
            }
        }
        // Ensure the arguments were not modified.
        return x == x1 && y == y1;
    };
}

internal partial struct TestAliasing_target /*dyn*/ {
    internal @string name;
    internal Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>> oneArgF;
    internal Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>> twoArgsF;
}

// TestAliasing checks that receivers and arguments can alias each other without
// leading to incorrect results. That is, it ensures that it's safe to write
//
//	v.Invert(v)
//
// or
//
//	v.Add(v, v)
//
// without any of the inputs getting clobbered by the output being written.
public static void TestAliasing(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in new TestAliasing_target[]{
        new(name: "Absolute"u8, oneArgF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Absolute))),
        new(name: "Invert"u8, oneArgF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Invert))),
        new(name: "Negate"u8, oneArgF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Negate))),
        new(name: "Set"u8, oneArgF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Set))),
        new(name: "Square"u8, oneArgF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Square))),
        new(name: "Pow22523"u8, oneArgF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Pow22523))),
        new(
            name: "Mult32"u8,
            oneArgF: (ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> v, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> x) => v.Mult32(x, 0xffffffffU)
        ),
        new(name: "Multiply"u8, twoArgsF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Multiply))),
        new(name: "Add"u8, twoArgsF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Add))),
        new(name: "Subtract"u8, twoArgsF: ((Func<ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>>)(global::go.crypto.@internal.fips140.edwards25519.field_package.Subtract))),
        new(
            name: "SqrtRatio"u8,
            twoArgsF: (ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> v, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> x, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> y) => {
                var (r, _) = v.SqrtRatio(x, y);
                return r;
            }
        ),
        new(
            name: "Select0"u8,
            twoArgsF: (ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> v, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> x, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> y) => v.Select(x, y, 0)
        ),
        new(
            name: "Select1"u8,
            twoArgsF: (ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> v, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> x, ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> y) => v.Select(x, y, 1)
        )
    }.slice()) {
        error err = default!;
        switch (ᐧ) {
        case {} when tt.oneArgF != default!: {
            err = quick.Check((checkAliasingOneArg(tt.oneArgF)).OrTypedNilFunc(), quickCheckConfig(256));
            break;
        }
        case {} when tt.twoArgsF != default!: {
            err = quick.Check((checkAliasingTwoArgs(tt.twoArgsF)).OrTypedNilFunc(), quickCheckConfig(256));
            break;
        }}

        if (err != default!) {
            Ꮡt.Errorf("%v: %v"u8, tt.name, err);
        }
    }
}

} // end field_internal_test_package
