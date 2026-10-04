// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run gen_sort_variants.go

// Package sort provides primitives for sorting slices and user-defined collections.
namespace go;

using bits = math.bits_package;
using slices = slices_package;
using math;

partial class sort_package {

// An implementation of Interface can be sorted by the routines in this package.
// The methods refer to elements of the underlying collection by integer index.
[GoType] partial interface Interface {
    // Len is the number of elements in the collection.
    nint Len();
    // Less reports whether the element with index i
    // must sort before the element with index j.
    //
    // If both Less(i, j) and Less(j, i) are false,
    // then the elements at index i and j are considered equal.
    // Sort may place equal elements in any order in the final result,
    // while Stable preserves the original input order of equal elements.
    //
    // Less must describe a transitive ordering:
    //  - if both Less(i, j) and Less(j, k) are true, then Less(i, k) must be true as well.
    //  - if both Less(i, j) and Less(j, k) are false, then Less(i, k) must be false as well.
    //
    // Note that floating-point comparison (the < operator on float32 or float64 values)
    // is not a transitive ordering when not-a-number (NaN) values are involved.
    // See Float64Slice.Less for a correct implementation for floating-point values.
    bool Less(nint i, nint j);
    // Swap swaps the elements with indexes i and j.
    void Swap(nint i, nint j);
}

// Sort sorts data in ascending order as determined by the Less method.
// It makes one call to data.Len to determine n and O(n*log(n)) calls to
// data.Less and data.Swap. The sort is not guaranteed to be stable.
//
// Note: in many situations, the newer [slices.SortFunc] function is more
// ergonomic and runs faster.
public static void Sort(Interface data) {
    nint n = data.Len();
    if (n <= 1) {
        return;
    }
    nint limit = bits.Len((nuint)n);
    pdqsort(data, 0, n, limit);
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
    nuint shift = (nuint)bits.Len((nuint)length);
    return (nuint)(((nuint)1).Lsh(shift));
}

// lessSwap is a pair of Less and Swap function for use with the
// auto-generated func-optimized variant of sort.go in
// zfuncversion.go.
[GoType] partial struct lessSwap {
    public Func<nint, nint, bool> Less;
    public Action<nint, nint> Swap;
}

[GoType] partial struct reverse {
    // This embedded Interface permits Reverse to use the methods of
    // another Interface implementation.
    [GoEmbedded] public Interface Interface;
}

// Less returns the opposite of the embedded implementation's Less method.
internal static bool Less(this reverse r, nint i, nint j) {
    return r.Interface.Less(j, i);
}

// Reverse returns the reverse order for data.
public static Interface Reverse(Interface data) {
    return new reverseжInterface(Ꮡ(new reverse(data)));
}

// IsSorted reports whether data is sorted.
//
// Note: in many situations, the newer [slices.IsSortedFunc] function is more
// ergonomic and runs faster.
public static bool IsSorted(Interface data) {
    nint n = data.Len();
    for (nint i = n - 1; i > 0; i--) {
        if (data.Less(i, i - 1)) {
            return false;
        }
    }
    return true;
}

[GoType("[]nint")] partial struct IntSlice;

// Convenience types for common cases
public static nint Len(this IntSlice x) {
    return len(x);
}

public static bool Less(this IntSlice x, nint i, nint j) {
    return x[i] < x[j];
}

public static void Swap(this IntSlice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// Sort is a convenience method: x.Sort() calls Sort(x).
public static void Sort(this IntSlice x) {
    Sort((Interface)(x));
}

[GoType("[]float64")] partial struct Float64Slice;

public static nint Len(this Float64Slice x) {
    return len(x);
}

// Less reports whether x[i] should be ordered before x[j], as required by the sort Interface.
// Note that floating-point comparison by itself is not a transitive relation: it does not
// report a consistent ordering for not-a-number (NaN) values.
// This implementation of Less places NaN values before any others, by using:
//
//	x[i] < x[j] || (math.IsNaN(x[i]) && !math.IsNaN(x[j]))
public static bool Less(this Float64Slice x, nint i, nint j) {
    return x[i] < x[j] || (isNaN(x[i]) && !isNaN(x[j]));
}

public static void Swap(this Float64Slice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// isNaN is a copy of math.IsNaN to avoid a dependency on the math package.
internal static bool isNaN(float64 f) {
    return f != f;
}

// Sort is a convenience method: x.Sort() calls Sort(x).
public static void Sort(this Float64Slice x) {
    Sort((Interface)(x));
}

[GoType("[]@string")] partial struct StringSlice;

public static nint Len(this StringSlice x) {
    return len(x);
}

public static bool Less(this StringSlice x, nint i, nint j) {
    return x[i] < x[j];
}

public static void Swap(this StringSlice x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// Sort is a convenience method: x.Sort() calls Sort(x).
public static void Sort(this StringSlice x) {
    Sort((Interface)(x));
}

// Convenience wrappers for common cases

// Ints sorts a slice of ints in increasing order.
//
// Note: as of Go 1.22, this function simply calls [slices.Sort].
public static void Ints(slice<nint> x) {
    slices.Sort<slice<nint>, nint>(x);
}

// Float64s sorts a slice of float64s in increasing order.
// Not-a-number (NaN) values are ordered before other values.
//
// Note: as of Go 1.22, this function simply calls [slices.Sort].
public static void Float64s(slice<float64> x) {
    slices.Sort<slice<float64>, float64>(x);
}

// Strings sorts a slice of strings in increasing order.
//
// Note: as of Go 1.22, this function simply calls [slices.Sort].
public static void Strings(slice<@string> x) {
    slices.Sort<slice<@string>, @string>(x);
}

// IntsAreSorted reports whether the slice x is sorted in increasing order.
//
// Note: as of Go 1.22, this function simply calls [slices.IsSorted].
public static bool IntsAreSorted(slice<nint> x) {
    return slices.IsSorted<slice<nint>, nint>(x);
}

// Float64sAreSorted reports whether the slice x is sorted in increasing order,
// with not-a-number (NaN) values before any other values.
//
// Note: as of Go 1.22, this function simply calls [slices.IsSorted].
public static bool Float64sAreSorted(slice<float64> x) {
    return slices.IsSorted<slice<float64>, float64>(x);
}

// StringsAreSorted reports whether the slice x is sorted in increasing order.
//
// Note: as of Go 1.22, this function simply calls [slices.IsSorted].
public static bool StringsAreSorted(slice<@string> x) {
    return slices.IsSorted<slice<@string>, @string>(x);
}

// Notes on stable sorting:
// The used algorithms are simple and provable correct on all input and use
// only logarithmic additional stack space. They perform well if compared
// experimentally to other stable in-place sorting algorithms.
//
// Remarks on other algorithms evaluated:
//  - GCC's 4.6.3 stable_sort with merge_without_buffer from libstdc++:
//    Not faster.
//  - GCC's __rotate for block rotations: Not faster.
//  - "Practical in-place mergesort" from  Jyrki Katajainen, Tomi A. Pasanen
//    and Jukka Teuhola; Nordic Journal of Computing 3,1 (1996), 27-40:
//    The given algorithms are in-place, number of Swap and Assignments
//    grow as n log n but the algorithm is not stable.
//  - "Fast Stable In-Place Sorting with O(n) Data Moves" J.I. Munro and
//    V. Raman in Algorithmica (1996) 16, 115-160:
//    This algorithm either needs additional 2n bits or works only if there
//    are enough different elements available to encode some permutations
//    which have to be undone later (so not stable on any input).
//  - All the optimal in-place sorting/merging algorithms I found are either
//    unstable or rely on enough different elements in each step to encode the
//    performed block rearrangements. See also "In-Place Merging Algorithms",
//    Denham Coates-Evely, Department of Computer Science, Kings College,
//    January 2004 and the references in there.
//  - Often "optimal" algorithms are optimal in the number of assignments
//    but Interface has only Swap as operation.

// Stable sorts data in ascending order as determined by the Less method,
// while keeping the original order of equal elements.
//
// It makes one call to data.Len to determine n, O(n*log(n)) calls to
// data.Less and O(n*log(n)*log(n)) calls to data.Swap.
//
// Note: in many situations, the newer slices.SortStableFunc function is more
// ergonomic and runs faster.
public static void Stable(Interface data) {
    stable(data, data.Len());
}

/*
Complexity of Stable Sorting


Complexity of block swapping rotation

Each Swap puts one new element into its correct, final position.
Elements which reach their final position are no longer moved.
Thus block swapping rotation needs |u|+|v| calls to Swaps.
This is best possible as each element might need a move.

Pay attention when comparing to other optimal algorithms which
typically count the number of assignments instead of swaps:
E.g. the optimal algorithm of Dudzinski and Dydek for in-place
rotations uses O(u + v + gcd(u,v)) assignments which is
better than our O(3 * (u+v)) as gcd(u,v) <= u.


Stable sorting by SymMerge and BlockSwap rotations

SymMerg complexity for same size input M = N:
Calls to Less:  O(M*log(N/M+1)) = O(N*log(2)) = O(N)
Calls to Swap:  O((M+N)*log(M)) = O(2*N*log(N)) = O(N*log(N))

(The following argument does not fuzz over a missing -1 or
other stuff which does not impact the final result).

Let n = data.Len(). Assume n = 2^k.

Plain merge sort performs log(n) = k iterations.
On iteration i the algorithm merges 2^(k-i) blocks, each of size 2^i.

Thus iteration i of merge sort performs:
Calls to Less  O(2^(k-i) * 2^i) = O(2^k) = O(2^log(n)) = O(n)
Calls to Swap  O(2^(k-i) * 2^i * log(2^i)) = O(2^k * i) = O(n*i)

In total k = log(n) iterations are performed; so in total:
Calls to Less O(log(n) * n)
Calls to Swap O(n + 2*n + 3*n + ... + (k-1)*n + k*n)
   = O((k/2) * k * n) = O(n * k^2) = O(n * log^2(n))


Above results should generalize to arbitrary n = 2^k + p
and should not be influenced by the initial insertion sort phase:
Insertion sort is O(n^2) on Swap and Less, thus O(bs^2) per block of
size bs at n/bs blocks:  O(bs*n) Swaps and Less during insertion sort.
Merge sort iterations start at i = log(bs). With t = log(bs) constant:
Calls to Less O((log(n)-t) * n + bs*n) = O(log(n)*n + (bs-t)*n)
   = O(n * log(n))
Calls to Swap O(n * log^2(n) - (t^2+t)/2*n) = O(n * log^2(n))

*/

} // end sort_package
