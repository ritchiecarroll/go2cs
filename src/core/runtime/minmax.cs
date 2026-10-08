// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static @string strmin(@string x, @string y) {
    if (y < x) {
        return y;
    }
    return x;
}

internal static @string strmax(@string x, @string y) {
    if (y > x) {
        return y;
    }
    return x;
}

internal static float32 fmin32(float32 x, float32 y) {
    return fmin(x, y);
}

internal static float64 fmin64(float64 x, float64 y) {
    return fmin(x, y);
}

internal static float32 fmax32(float32 x, float32 y) {
    return fmax(x, y);
}

internal static float64 fmax64(float64 x, float64 y) {
    return fmax(x, y);
}

partial interface floaty<ΔT> /*operators = Sum, Arithmetic, Comparable, Ordered*/ {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

internal static F fmin<F>(F x, F y)
    where F : /* floaty */ IAdditionOperators<F, F, F>, ISubtractionOperators<F, F, F>, IMultiplyOperators<F, F, F>, IDivisionOperators<F, F, F>, IIncrementOperators<F>, IDecrementOperators<F>, IUnaryNegationOperators<F, F>, IEqualityOperators<F, F, bool>, IComparisonOperators<F, F, bool>, new()
{
    if (!AreEqual(y, y) || y < x) {
        return y;
    }
    if (!AreEqual(x, x) || x < y || !AreEqual(x, 0)) {
        return x;
    }
    // x and y are both ±0
    // if either is -0, return -0; else return +0
    return forbits(x, y);
}

internal static F fmax<F>(F x, F y)
    where F : /* floaty */ IAdditionOperators<F, F, F>, ISubtractionOperators<F, F, F>, IMultiplyOperators<F, F, F>, IDivisionOperators<F, F, F>, IIncrementOperators<F>, IDecrementOperators<F>, IUnaryNegationOperators<F, F>, IEqualityOperators<F, F, bool>, IComparisonOperators<F, F, bool>, new()
{
    if (!AreEqual(y, y) || y > x) {
        return y;
    }
    if (!AreEqual(x, x) || x > y || !AreEqual(x, 0)) {
        return x;
    }
    // x and y are both ±0
    // if both are -0, return -0; else return +0
    return fandbits(x, y);
}

internal static F forbits<F>(F xʗp, F yʗp)
    where F : /* floaty */ IAdditionOperators<F, F, F>, ISubtractionOperators<F, F, F>, IMultiplyOperators<F, F, F>, IDivisionOperators<F, F, F>, IIncrementOperators<F>, IDecrementOperators<F>, IUnaryNegationOperators<F, F>, IEqualityOperators<F, F, bool>, IComparisonOperators<F, F, bool>, new()
{
    ref var x = ref heap(xʗp, out var Ꮡx);
    ref var y = ref heap(yʗp, out var Ꮡy);

    var exprᴛ1 = @unsafe.Sizeof(x);
    if (exprᴛ1 == 4) {
        (Ꮡx.Reinterpret<F, uint32>()).Value |= (Ꮡy.Reinterpret<F, uint32>()).Value;
    }
    else if (exprᴛ1 == 8) {
        (Ꮡx.Reinterpret<F, uint64>()).Value |= (Ꮡy.Reinterpret<F, uint64>()).Value;
    }

    return x;
}

internal static F fandbits<F>(F xʗp, F yʗp)
    where F : /* floaty */ IAdditionOperators<F, F, F>, ISubtractionOperators<F, F, F>, IMultiplyOperators<F, F, F>, IDivisionOperators<F, F, F>, IIncrementOperators<F>, IDecrementOperators<F>, IUnaryNegationOperators<F, F>, IEqualityOperators<F, F, bool>, IComparisonOperators<F, F, bool>, new()
{
    ref var x = ref heap(xʗp, out var Ꮡx);
    ref var y = ref heap(yʗp, out var Ꮡy);

    var exprᴛ1 = @unsafe.Sizeof(x);
    if (exprᴛ1 == 4) {
        (Ꮡx.Reinterpret<F, uint32>()).Value &= (Ꮡy.Reinterpret<F, uint32>()).Value;
    }
    else if (exprᴛ1 == 8) {
        (Ꮡx.Reinterpret<F, uint64>()).Value &= (Ꮡy.Reinterpret<F, uint64>()).Value;
    }

    return x;
}

} // end runtime_package
