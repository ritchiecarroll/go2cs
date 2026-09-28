// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run $GOROOT/src/sort/gen_sort_variants.go -generic
namespace go;

using cmp = cmp_package;
using bits = math.bits_package;
using math;

partial class slices_package {

// Sort sorts a slice of any ordered type in ascending order.
// When sorting floating-point numbers, NaNs are ordered before other values.
public static void Sort<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    nint n = len(x);
    pdqsortOrdered(new slice<E>(x), 0, n, bits.Len((nuint)n));
}

// SortFunc sorts the slice x in ascending order as determined by the cmp
// function. This sort is not guaranteed to be stable.
// cmp(a, b) should return a negative number when a < b, a positive number when
// a > b and zero when a == b or a and b are incomparable in the sense of
// a strict weak ordering.
//
// SortFunc requires that cmp is a strict weak ordering.
// See https://en.wikipedia.org/wiki/Weak_ordering#Strict_weak_orderings.
// The function should return 0 for incomparable items.
public static void SortFunc<S, E>(S x, Func<E, E, nint> cmp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    nint n = len(x);
    pdqsortCmpFunc(new slice<E>(x), 0, n, bits.Len((nuint)n), cmp);
}

// SortStableFunc sorts the slice x while keeping the original order of equal
// elements, using cmp to compare elements in the same way as [SortFunc].
public static void SortStableFunc<S, E>(S x, Func<E, E, nint> cmp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    stableCmpFunc(new slice<E>(x), len(x), cmp);
}

// IsSorted reports whether x is sorted in ascending order.
public static bool IsSorted<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    for (nint i = len(x) - 1; i > 0; i--) {
        if (cmp.Less(x[i], x[i - 1])) {
            return false;
        }
    }
    return true;
}

// IsSortedFunc reports whether x is sorted in ascending order, with cmp as the
// comparison function as defined by [SortFunc].
public static bool IsSortedFunc<S, E>(S x, Func<E, E, nint> cmp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    for (nint i = len(x) - 1; i > 0; i--) {
        if (cmp(x[i], x[i - 1]) < 0) {
            return false;
        }
    }
    return true;
}

// Min returns the minimal value in x. It panics if x is empty.
// For floating-point numbers, Min propagates NaNs (any NaN value in x
// forces the output to be NaN).
public static E Min<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    if (len(x) < 1) {
        throw panic("slices.Min: empty list");
    }
    var m = x[0];
    for (nint i = 1; i < len(x); i++) {
        m = min(m, x[i]);
    }
    return m;
}

// MinFunc returns the minimal value in x, using cmp to compare elements.
// It panics if x is empty. If there is more than one minimal element
// according to the cmp function, MinFunc returns the first one.
public static E MinFunc<S, E>(S x, Func<E, E, nint> cmp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (len(x) < 1) {
        throw panic("slices.MinFunc: empty list");
    }
    var m = x[0];
    for (nint i = 1; i < len(x); i++) {
        if (cmp(x[i], m) < 0) {
            m = x[i];
        }
    }
    return m;
}

// Max returns the maximal value in x. It panics if x is empty.
// For floating-point E, Max propagates NaNs (any NaN value in x
// forces the output to be NaN).
public static E Max<S, E>(S x)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    if (len(x) < 1) {
        throw panic("slices.Max: empty list");
    }
    var m = x[0];
    for (nint i = 1; i < len(x); i++) {
        m = max(m, x[i]);
    }
    return m;
}

// MaxFunc returns the maximal value in x, using cmp to compare elements.
// It panics if x is empty. If there is more than one maximal element
// according to the cmp function, MaxFunc returns the first one.
public static E MaxFunc<S, E>(S x, Func<E, E, nint> cmp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (len(x) < 1) {
        throw panic("slices.MaxFunc: empty list");
    }
    var m = x[0];
    for (nint i = 1; i < len(x); i++) {
        if (cmp(x[i], m) > 0) {
            m = x[i];
        }
    }
    return m;
}

// BinarySearch searches for target in a sorted slice and returns the earliest
// position where target is found, or the position where target would appear
// in the sort order; it also returns a bool saying whether the target is
// really found in the slice. The slice must be sorted in increasing order.
public static (nint, bool) BinarySearch<S, E>(S x, E target)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    // Inlining is faster than calling BinarySearchFunc with a lambda.
    nint n = len(x);
    // Define x[-1] < target and x[n] >= target.
    // Invariant: x[i-1] < target, x[j] >= target.
    nint i = 0;
    nint j = n;
    while (i < j) {
        nint h = (nint)(((nuint)(i + j) >> (int)(1))); // avoid overflow when computing h
        // i ≤ h < j
        if (cmp.Less(x[h], target)){
            i = h + 1; // preserves x[i-1] < target
        } else {
            j = h; // preserves x[j] >= target
        }
    }
    // i == j, x[i-1] < target, and x[j] (= x[i]) >= target  =>  answer is i.
    return (i, i < n && (AreEqual(x[i], target) || (isNaN(x[i]) && isNaN(target))));
}

// BinarySearchFunc works like [BinarySearch], but uses a custom comparison
// function. The slice must be sorted in increasing order, where "increasing"
// is defined by cmp. cmp should return 0 if the slice element matches
// the target, a negative number if the slice element precedes the target,
// or a positive number if the slice element follows the target.
// cmp must implement the same ordering as the slice, such that if
// cmp(a, t) < 0 and cmp(b, t) >= 0, then a must precede b in the slice.
public static (nint, bool) BinarySearchFunc<S, E, T>(S x, T target, Func<E, T, nint> cmp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    nint n = len(x);
    // Define cmp(x[-1], target) < 0 and cmp(x[n], target) >= 0 .
    // Invariant: cmp(x[i - 1], target) < 0, cmp(x[j], target) >= 0.
    nint i = 0;
    nint j = n;
    while (i < j) {
        nint h = (nint)(((nuint)(i + j) >> (int)(1))); // avoid overflow when computing h
        // i ≤ h < j
        if (cmp(x[h], target) < 0){
            i = h + 1; // preserves cmp(x[i - 1], target) < 0
        } else {
            j = h; // preserves cmp(x[j], target) >= 0
        }
    }
    // i == j, cmp(x[i-1], target) < 0, and cmp(x[j], target) (= cmp(x[i], target)) >= 0  =>  answer is i.
    return (i, i < n && cmp(x[i], target) == 0);
}

[GoType("num:nint")] partial struct sortedHint;

internal static sortedHint unknownHint => /* iota */ 0;
internal static sortedHint increasingHint => 1;
internal static sortedHint decreasingHint => 2;

[GoType("num:uint64")] partial struct xorshift;

[GoRecv] internal static uint64 Next(this ref xorshift r) {
    r ^= (xorshift)((r << (int)(13)));
    r ^= (xorshift)((r >> (int)(7)));
    r ^= (xorshift)((r << (int)(17)));
    return (uint64)(r);
}

internal static nuint nextPowerOfTwo(nint length) {
    return ((nuint)1).Lsh((int64)(bits.Len((nuint)length)));
}

// isNaN reports whether x is a NaN without requiring the math package.
// This will always return false if T is not floating-point.
internal static bool isNaN<T>(T x)
    where T : /* cmp.Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return !AreEqual(x, x);
}

} // end slices_package
