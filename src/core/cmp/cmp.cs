// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cmp provides types and functions related to comparing
// ordered values.
namespace go;

partial class cmp_package {

// Ordered is a constraint that permits any ordered type: any type
// that supports the operators < <= >= >.
// If future releases of Go add new ordered types,
// this constraint will be modified to include them.
//
// Note that floating-point types may contain NaN ("not-a-number") values.
// An operator such as == or < will always report false when
// comparing a NaN value with any other value, NaN or not.
// See the [Compare] function for a consistent way to compare NaN values.
[GoType("operators = Sum, Comparable, Ordered")]
partial interface Ordered<ΔT> {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64 | ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr | ~float32 | ~float64 | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

// Less reports whether x is less than y.
// For floating-point types, a NaN is considered less than any non-NaN,
// and -0.0 is not less than (is equal to) 0.0.
public static bool Less<T>(T x, T y)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return (isNaN(x) && !isNaN(y)) || x < y;
}

// Compare returns
//
//	-1 if x is less than y,
//	 0 if x equals y,
//	+1 if x is greater than y.
//
// For floating-point types, a NaN is considered less than any non-NaN,
// a NaN is considered equal to a NaN, and -0.0 is equal to 0.0.
public static nint Compare<T>(T x, T y)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var xNaN = isNaN(x);
    var yNaN = isNaN(y);
    if (xNaN) {
        if (yNaN) {
            return 0;
        }
        return -1;
    }
    if (yNaN) {
        return +1;
    }
    if (x < y) {
        return -1;
    }
    if (x > y) {
        return +1;
    }
    return 0;
}

// isNaN reports whether x is a NaN without requiring the math package.
// This will always return false if T is not floating-point.
internal static bool isNaN<T>(T x)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return !AreEqual(x, x);
}

// Or returns the first of its arguments that is not equal to the zero value.
// If no argument is non-zero, it returns the zero value.
public static T Or<T>(params Span<T> valsʗp) {
    var vals = valsʗp.sslice();

    T zero = GoZero<T>();
    foreach (var (_, val) in vals) {
        if (!AreEqual(val, zero)) {
            return val;
        }
    }
    return zero;
}

} // end cmp_package
