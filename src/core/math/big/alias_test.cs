// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using cryptorand = crypto.rand_package;
using big = go.math.big_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using go.math;
using go.testing;
using io = io_package;
using static go.math.big_internal_test_package;

partial class big_test_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸcryptoꓸrand() {
    builtin.initPackage(typeof(crypto.rand_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸmathꓸbig() {
    builtin.initPackage(typeof(go.math.big_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸmathꓸrand() {
    builtin.initPackage(typeof(go.math.rand_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸreflect() {
    builtin.initPackage(typeof(reflect_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸtesting() {
    builtin.initPackage(typeof(testing_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸtestingꓸquick() {
    builtin.initPackage(typeof(go.testing.quick_package));
}

internal static bool equal(ж<bigꓸInt> Ꮡz, ж<bigꓸInt> Ꮡx) {
    return Ꮡz.Cmp(Ꮡx) == 0;
}

[GoType] partial struct bigInt {
    public partial ref ж<math.big_package.ΔInt> Int { get; }
}

internal static ж<bigꓸInt> generatePositiveInt(ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var n = big.NewInt(1);
    n.Lsh(n, (nuint)randΔ1.Intn(size * 8));
    n.Rand(Ꮡrand, n);
    return n;
}

internal static reflectꓸValue Generate(this bigInt _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var n = generatePositiveInt(Ꮡrand, size);
    if (randΔ1.Intn(4) == 0) {
        n.Neg(n);
    }
    return reflect.ValueOf(new bigInt(n));
}

[GoType] partial struct notZeroInt {
    public partial ref ж<math.big_package.ΔInt> Int { get; }
}

internal static reflectꓸValue Generate(this notZeroInt _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    var n = generatePositiveInt(Ꮡrand, size);
    if (randΔ1.Intn(4) == 0) {
        n.Neg(n);
    }
    if (n.Sign() == 0) {
        n.SetInt64(1);
    }
    return reflect.ValueOf(new notZeroInt(n));
}

[GoType] partial struct positiveInt {
    public partial ref ж<math.big_package.ΔInt> Int { get; }
}

internal static reflectꓸValue Generate(this positiveInt _, ж<rand.Rand> Ꮡrand, nint size) {
    var n = generatePositiveInt(Ꮡrand, size);
    return reflect.ValueOf(new positiveInt(n));
}

[GoType] partial struct prime {
    public partial ref ж<math.big_package.ΔInt> Int { get; }
}

internal static reflectꓸValue Generate(this prime _, ж<rand.Rand> Ꮡr, nint size) {
    ref var r = ref Ꮡr.DerefOrNull();

    var (n, err) = cryptorand.Prime(new big_test_package.rand_RandжReader(Ꮡr), r.Intn(size * 8 - 2) + 2);
    if (err != default!) {
        throw panic(err);
    }
    return reflect.ValueOf(new prime(n));
}

[GoType] partial struct zeroOrOne {
    [GoEmbedded] internal nuint @uint;
}

internal static reflectꓸValue Generate(this zeroOrOne _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    return reflect.ValueOf(new zeroOrOne((nuint)randΔ1.Intn(2)));
}

[GoType] partial struct smallUint {
    [GoEmbedded] internal nuint @uint;
}

internal static reflectꓸValue Generate(this smallUint _, ж<rand.Rand> Ꮡrand, nint size) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    return reflect.ValueOf(new smallUint((nuint)randΔ1.Intn(1024)));
}

// checkAliasingOneArg checks if f returns a correct result when v and x alias.
//
// f is a function that takes x as an argument, doesn't modify it, sets v to the
// result, and returns v. It is the function signature of unbound methods like
//
//	func (v *big.Int) m(x *big.Int) *big.Int
//
// v and x are two random Int values. v is randomized even if it will be
// overwritten to test for improper buffer reuse.
internal static bool checkAliasingOneArg(ж<testing.T> Ꮡt, Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>> f, ж<bigꓸInt> Ꮡv, ж<bigꓸInt> Ꮡx) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    var (x1, v1) = (@new<bigꓸInt>().Set(Ꮡx), @new<bigꓸInt>().Set(Ꮡx));
    // Calculate a reference f(x) without aliasing.
    {
        var @out = f(Ꮡv, Ꮡx); if (@out != Ꮡv) {
            return false;
        }
    }
    // Test aliasing the argument and the receiver.
    {
        var @out = f(v1, v1); if (@out != v1 || !equal(v1, Ꮡv)) {
            Ꮡt.Logf("f(v, x) != f(x, x)"u8);
            return false;
        }
    }
    // Ensure the arguments was not modified.
    return equal(Ꮡx, x1);
}

// checkAliasingTwoArgs checks if f returns a correct result when any
// combination of v, x and y alias.
//
// f is a function that takes x and y as arguments, doesn't modify them, sets v
// to the result, and returns v. It is the function signature of unbound methods
// like
//
//	func (v *big.Int) m(x, y *big.Int) *big.Int
//
// v, x and y are random Int values. v is randomized even if it will be
// overwritten to test for improper buffer reuse.
internal static bool checkAliasingTwoArgs(ж<testing.T> Ꮡt, Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>> f, ж<bigꓸInt> Ꮡv, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    var (x1, y1, v1) = (@new<bigꓸInt>().Set(Ꮡx), @new<bigꓸInt>().Set(Ꮡy), @new<bigꓸInt>().Set(Ꮡv));
    // Calculate a reference f(x, y) without aliasing.
    {
        var @out = f(Ꮡv, Ꮡx, Ꮡy); if (@out == nil){
            // Certain functions like ModInverse return nil for certain inputs.
            // Check that receiver and arguments were unchanged and move on.
            return equal(Ꮡx, x1) && equal(Ꮡy, y1) && equal(Ꮡv, v1);
        } else 
        if (@out != Ꮡv) {
            return false;
        }
    }
    // Test aliasing the first argument and the receiver.
    v1.Set(Ꮡx);
    {
        var @out = f(v1, v1, Ꮡy); if (@out != v1 || !equal(v1, Ꮡv)) {
            Ꮡt.Logf("f(v, x, y) != f(x, x, y)"u8);
            return false;
        }
    }
    // Test aliasing the second argument and the receiver.
    v1.Set(Ꮡy);
    {
        var @out = f(v1, Ꮡx, v1); if (@out != v1 || !equal(v1, Ꮡv)) {
            Ꮡt.Logf("f(v, x, y) != f(y, x, y)"u8);
            return false;
        }
    }
    // Calculate a reference f(y, y) without aliasing.
    // We use y because it's the one that commonly has restrictions
    // like being prime or non-zero.
    v1.Set(Ꮡv);
    var y2 = @new<bigꓸInt>().Set(Ꮡy);
    {
        var @out = f(Ꮡv, Ꮡy, y2); if (@out == nil){
            return equal(Ꮡy, y1) && equal(y2, y1) && equal(Ꮡv, v1);
        } else 
        if (@out != Ꮡv) {
            return false;
        }
    }
    // Test aliasing the two arguments.
    {
        var @out = f(v1, Ꮡy, Ꮡy); if (@out != v1 || !equal(v1, Ꮡv)) {
            Ꮡt.Logf("f(v, y1, y2) != f(v, y, y)"u8);
            return false;
        }
    }
    // Test aliasing the two arguments and the receiver.
    v1.Set(Ꮡy);
    {
        var @out = f(v1, v1, v1); if (@out != v1 || !equal(v1, Ꮡv)) {
            Ꮡt.Logf("f(v, y1, y2) != f(y, y, y)"u8);
            return false;
        }
    }
    // Ensure the arguments were not modified.
    return equal(Ꮡx, x1) && equal(Ꮡy, y1);
}

public static void TestAliasing(ж<testing.T> Ꮡt) {





















    foreach (var (name, f) in new map<@string, any>{
        ["Abs"u8] = bool (bigInt v, bigInt x) => checkAliasingOneArg(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Abs), v.Int, x.Int),
        ["Add"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Add), v.Int, x.Int, y.Int),
        ["And"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.And), v.Int, x.Int, y.Int),
        ["AndNot"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.AndNot), v.Int, x.Int, y.Int),
        ["Div"u8] = bool (bigInt v, bigInt x, notZeroInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Div), v.Int, x.Int, y.Int),
        ["Exp-XY"u8] = bool (bigInt v, bigInt x, bigInt y, notZeroInt z) => {
            var zʗ1 = z;
            return checkAliasingTwoArgs(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1, ж<bigꓸInt> yΔ1) => vΔ1.Exp(xΔ1, yΔ1, zʗ1.Int), v.Int, x.Int, y.Int);
        },
        ["Exp-XZ"u8] = bool (bigInt v, bigInt x, bigInt y, notZeroInt z) => {
            var yʗ1 = y;
            return checkAliasingTwoArgs(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1, ж<bigꓸInt> zΔ1) => vΔ1.Exp(xΔ1, yʗ1.Int, zΔ1), v.Int, x.Int, z.Int);
        },
        ["Exp-YZ"u8] = bool (bigInt v, bigInt x, bigInt y, notZeroInt z) => {
            var xʗ1 = x;
            return checkAliasingTwoArgs(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> yΔ1, ж<bigꓸInt> zΔ1) => vΔ1.Exp(xʗ1.Int, yΔ1, zΔ1), v.Int, y.Int, z.Int);
        },
        ["GCD"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1, ж<bigꓸInt> yΔ1) => vΔ1.GCD(nil, nil, xΔ1, yΔ1), v.Int, x.Int, y.Int),
        ["GCD-X"u8] = bool (bigInt v, bigInt x, bigInt y) => {
            var (a, b) = (@new<bigꓸInt>(), @new<bigꓸInt>());
            var aʗ1 = a;
            var bʗ1 = b;
            return checkAliasingTwoArgs(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1, ж<bigꓸInt> yΔ1) => {
                aʗ1.GCD(vΔ1, bʗ1, xΔ1, yΔ1);
                return vΔ1;
            }, v.Int, x.Int, y.Int);
        },
        ["GCD-Y"u8] = bool (bigInt v, bigInt x, bigInt y) => {
            var (a, b) = (@new<bigꓸInt>(), @new<bigꓸInt>());
            var aʗ2 = a;
            var bʗ2 = b;
            return checkAliasingTwoArgs(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1, ж<bigꓸInt> yΔ1) => {
                aʗ2.GCD(bʗ2, vΔ1, xΔ1, yΔ1);
                return vΔ1;
            }, v.Int, x.Int, y.Int);
        },
        ["Lsh"u8] = bool (bigInt v, bigInt x, smallUint n) => {
            var nʗ1 = n;
            return checkAliasingOneArg(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1) => vΔ1.Lsh(xΔ1, nʗ1.@uint), v.Int, x.Int);
        },
        ["Mod"u8] = bool (bigInt v, bigInt x, notZeroInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Mod), v.Int, x.Int, y.Int),
        ["ModInverse"u8] = bool (bigInt v, bigInt x, notZeroInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.ModInverse), v.Int, x.Int, y.Int),
        ["ModSqrt"u8] = bool (bigInt v, bigInt x, prime p) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.ModSqrt), v.Int, x.Int, p.Int),
        ["Mul"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Mul), v.Int, x.Int, y.Int),
        ["Neg"u8] = bool (bigInt v, bigInt x) => checkAliasingOneArg(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Neg), v.Int, x.Int),
        ["Not"u8] = bool (bigInt v, bigInt x) => checkAliasingOneArg(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Not), v.Int, x.Int),
        ["Or"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Or), v.Int, x.Int, y.Int),
        ["Quo"u8] = bool (bigInt v, bigInt x, notZeroInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Quo), v.Int, x.Int, y.Int),
        ["Rand"u8] = bool (bigInt v, bigInt x, int64 seed) => checkAliasingOneArg(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1) => {
                var rnd = rand.New(rand.NewSource(seed));
                return vΔ1.Rand(rnd, xΔ1);
            }, v.Int, x.Int),
        ["Rem"u8] = bool (bigInt v, bigInt x, notZeroInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Rem), v.Int, x.Int, y.Int),
        ["Rsh"u8] = bool (bigInt v, bigInt x, smallUint n) => {
            var nʗ2 = n;
            return checkAliasingOneArg(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1) => vΔ1.Rsh(xΔ1, nʗ2.@uint), v.Int, x.Int);
        },
        ["Set"u8] = bool (bigInt v, bigInt x) => checkAliasingOneArg(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Set), v.Int, x.Int),
        ["SetBit"u8] = bool (bigInt v, bigInt x, smallUint i, zeroOrOne b) => {
            var bʗ3 = b;
            var iʗ1 = i;
            return checkAliasingOneArg(Ꮡt, (ж<bigꓸInt> vΔ1, ж<bigꓸInt> xΔ1) => vΔ1.SetBit(xΔ1, (nint)iʗ1.@uint, bʗ3.@uint), v.Int, x.Int);
        },
        ["Sqrt"u8] = bool (bigInt v, positiveInt x) => checkAliasingOneArg(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Sqrt), v.Int, x.Int),
        ["Sub"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Sub), v.Int, x.Int, y.Int),
        ["Xor"u8] = bool (bigInt v, bigInt x, bigInt y) => checkAliasingTwoArgs(Ꮡt, (Func<ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>, ж<bigꓸInt>>)(big.Xor), v.Int, x.Int, y.Int)
    }) {
        var fʗ1 = f;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            ref var scale = ref heap<float64>(out var Ꮡscale);
            scale = 1.0D;
            var exprᴛ1 = name;
            if (exprᴛ1 == "ModInverse"u8 || exprᴛ1 == "GCD-Y"u8 || exprᴛ1 == "GCD-X"u8) {
                scale /= 5D;
            }
            else if (exprᴛ1 == "Rand"u8) {
                scale /= 10D;
            }
            else if (exprᴛ1 == "Exp-XZ"u8 || exprᴛ1 == "Exp-XY"u8 || exprᴛ1 == "Exp-YZ"u8) {
                scale /= 50D;
            }
            else if (exprᴛ1 == "ModSqrt"u8) {
                scale /= 500D;
            }

            {
                var err = quick.Check(fʗ1, Ꮡ(new quick.Config(
                    MaxCountScale: scale
                ))); if (err != default!) {
                    tΔ1.Error(err);
                }
            }
        });
    }
}

} // end big_test_package
