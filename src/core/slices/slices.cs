// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package slices defines various functions useful with slices of any type.
namespace go;

using cmp = cmp_package;
using bits = math.bits_package;
using @unsafe = unsafe_package;
using math;

partial class slices_package {

// Equal reports whether two slices are equal: the same length and all
// elements equal. If the lengths are different, Equal returns false.
// Otherwise, the elements are compared in increasing index order, and the
// comparison stops at the first unequal pair.
// Empty and nil slices are considered equal.
// Floating point NaNs are not considered equal.
public static bool Equal<S, E>(S s1, S s2)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (len(s1) != len(s2)) {
        return false;
    }
    foreach (var (i, _) in s1) {
        if (!AreEqual(s1[i], s2[i])) {
            return false;
        }
    }
    return true;
}

// EqualFunc reports whether two slices are equal using an equality
// function on each pair of elements. If the lengths are different,
// EqualFunc returns false. Otherwise, the elements are compared in
// increasing index order, and the comparison stops at the first index
// for which eq returns false.
public static bool EqualFunc<S1, S2, E1, E2>(S1 s1, S2 s2, Func<E1, E2, bool> eq)
    where S1 : /* ~[]E1 */ ISlice<E1>, ISupportMake<S1>, ISliceWrap<S1, E1>, new()
    where S2 : /* ~[]E2 */ ISlice<E2>, ISupportMake<S2>, ISliceWrap<S2, E2>, new()
{
    if (len(s1) != len(s2)) {
        return false;
    }
    foreach (var (i, v1) in s1) {
        var v2 = s2[i];
        if (!eq(v1, v2)) {
            return false;
        }
    }
    return true;
}

// Compare compares the elements of s1 and s2, using [cmp.Compare] on each pair
// of elements. The elements are compared sequentially, starting at index 0,
// until one element is not equal to the other.
// The result of comparing the first non-matching elements is returned.
// If both slices are equal until one of them ends, the shorter slice is
// considered less than the longer one.
// The result is 0 if s1 == s2, -1 if s1 < s2, and +1 if s1 > s2.
public static nint Compare<S, E>(S s1, S s2)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    foreach (var (i, v1) in s1) {
        if (i >= len(s2)) {
            return +1;
        }
        var v2 = s2[i];
        {
            nint c = cmp.Compare(v1, v2); if (c != 0) {
                return c;
            }
        }
    }
    if (len(s1) < len(s2)) {
        return -1;
    }
    return 0;
}

// CompareFunc is like [Compare] but uses a custom comparison function on each
// pair of elements.
// The result is the first non-zero result of cmp; if cmp always
// returns 0 the result is 0 if len(s1) == len(s2), -1 if len(s1) < len(s2),
// and +1 if len(s1) > len(s2).
public static nint CompareFunc<S1, S2, E1, E2>(S1 s1, S2 s2, Func<E1, E2, nint> cmp)
    where S1 : /* ~[]E1 */ ISlice<E1>, ISupportMake<S1>, ISliceWrap<S1, E1>, new()
    where S2 : /* ~[]E2 */ ISlice<E2>, ISupportMake<S2>, ISliceWrap<S2, E2>, new()
{
    foreach (var (i, v1) in s1) {
        if (i >= len(s2)) {
            return +1;
        }
        var v2 = s2[i];
        {
            nint c = cmp(v1, v2); if (c != 0) {
                return c;
            }
        }
    }
    if (len(s1) < len(s2)) {
        return -1;
    }
    return 0;
}

// Index returns the index of the first occurrence of v in s,
// or -1 if not present.
public static nint Index<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    foreach (var (i, _) in s) {
        if (AreEqual(v, s[i])) {
            return i;
        }
    }
    return -1;
}

// IndexFunc returns the first index i satisfying f(s[i]),
// or -1 if none do.
public static nint IndexFunc<S, E>(S s, Func<E, bool> f)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    foreach (var (i, _) in s) {
        if (f(s[i])) {
            return i;
        }
    }
    return -1;
}

// Contains reports whether v is present in s.
public static bool Contains<S, E>(S s, E v)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return Index(s, v) >= 0;
}

// ContainsFunc reports whether at least one
// element e of s satisfies f(e).
public static bool ContainsFunc<S, E>(S s, Func<E, bool> f)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return IndexFunc(s, f) >= 0;
}

// Insert inserts the values v... into s at index i,
// returning the modified slice.
// The elements at s[i:] are shifted up to make room.
// In the returned slice r, r[i] == v[0],
// and, if i < len(s), r[i+len(v)] == value originally at r[i].
// Insert panics if i > len(s).
// This function is O(len(s) + len(v)).
public static S Insert<S, E>(S s, nint i, params Span<E> vʗp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    var v = vʗp.slice();

    _ = subslice<S, E>(s, i); // bounds check
    nint m = len(v);
    if (m == 0) {
        return s;
    }
    nint n = len(s);
    if (i == n) {
        return appendꓸꓸꓸ<S, E>(s, v);
    }
    if (n + m > cap(s)) {
        // Use append rather than make so that we bump the size of
        // the slice up to the next storage class.
        // This is what Grow does but we don't call Grow because
        // that might copy the values twice.
        var s2 = appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, i), makeꓸꓸꓸ<E>(n + m - i));
        copy(subslice<S, E>(s2, i), v);
        copy(subslice<S, E>(s2, i + m), subslice<S, E>(s, i));
        return s2;
    }
    s = subslice<S, E>(s, 0, n + m);
    // before:
    // s: aaaaaaaabbbbccccccccdddd
    //            ^   ^       ^   ^
    //            i  i+m      n  n+m
    // after:
    // s: aaaaaaaavvvvbbbbcccccccc
    //            ^   ^       ^   ^
    //            i  i+m      n  n+m
    //
    // a are the values that don't move in s.
    // v are the values copied in from v.
    // b and c are the values from s that are shifted up in index.
    // d are the values that get overwritten, never to be seen again.
    if (!overlaps(v, new slice<E>(subslice<S, E>(s, i + m)))) {
        // Easy case - v does not overlap either the c or d regions.
        // (It might be in some of a or b, or elsewhere entirely.)
        // The data we copy up doesn't write to v at all, so just do it.
        copy(subslice<S, E>(s, i + m), subslice<S, E>(s, i));
        // Now we have
        // s: aaaaaaaabbbbbbbbcccccccc
        //            ^   ^       ^   ^
        //            i  i+m      n  n+m
        // Note the b values are duplicated.
        copy(subslice<S, E>(s, i), v);
        // Now we have
        // s: aaaaaaaavvvvbbbbcccccccc
        //            ^   ^       ^   ^
        //            i  i+m      n  n+m
        // That's the result we want.
        return s;
    }
    // The hard case - v overlaps c or d. We can't just shift up
    // the data because we'd move or clobber the values we're trying
    // to insert.
    // So instead, write v on top of d, then rotate.
    copy(subslice<S, E>(s, n), v);
    // Now we have
    // s: aaaaaaaabbbbccccccccvvvv
    //            ^   ^       ^   ^
    //            i  i+m      n  n+m
    rotateRight(new slice<E>(subslice<S, E>(s, i)), m);
    // Now we have
    // s: aaaaaaaavvvvbbbbcccccccc
    //            ^   ^       ^   ^
    //            i  i+m      n  n+m
    // That's the result we want.
    return s;
}

// Delete removes the elements s[i:j] from s, returning the modified slice.
// Delete panics if j > len(s) or s[i:j] is not a valid slice of s.
// Delete is O(len(s)-i), so if many items must be deleted, it is better to
// make a single call deleting them all together than to delete one at a time.
// Delete zeroes the elements s[len(s)-(j-i):len(s)].
public static S Delete<S, E>(S s, nint i, nint j)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    _ = subslice3<S, E>(s, i, j, len(s)); // bounds check
    if (i == j) {
        return s;
    }
    nint oldlen = len(s);
    s = appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, i), subslice<S, E>(s, j));
    clear(subslice<S, E>(s, len(s), oldlen)); // zero/nil out the obsolete elements, for GC
    return s;
}

// DeleteFunc removes any elements from s for which del returns true,
// returning the modified slice.
// DeleteFunc zeroes the elements between the new length and the original length.
public static S DeleteFunc<S, E>(S s, Func<E, bool> del)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    nint i = IndexFunc(s, del);
    if (i == -1) {
        return s;
    }
    // Don't start copying elements until we find one to delete.
    for (nint j = i + 1; j < len(s); j++) {
        {
            var v = s[j]; if (!del(v)) {
                s[i] = v;
                i++;
            }
        }
    }
    clear(subslice<S, E>(s, i)); // zero/nil out the obsolete elements, for GC
    return subslice<S, E>(s, 0, i);
}

// Replace replaces the elements s[i:j] by the given v, and returns the
// modified slice.
// Replace panics if j > len(s) or s[i:j] is not a valid slice of s.
// When len(v) < (j-i), Replace zeroes the elements between the new length and the original length.
public static S Replace<S, E>(S s, nint i, nint j, params Span<E> vʗp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    var v = vʗp.slice();

    _ = subslice<S, E>(s, i, j); // bounds check
    if (i == j) {
        return Insert(s, i, v.ꓸꓸꓸ);
    }
    if (j == len(s)) {
        var s2 = appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, i), v);
        if (len(s2) < len(s)) {
            clear(subslice<S, E>(s, len(s2))); // zero/nil out the obsolete elements, for GC
        }
        return s2;
    }
    nint tot = len(subslice<S, E>(s, 0, i)) + len(v) + len(subslice<S, E>(s, j));
    if (tot > cap(s)) {
        // Too big to fit, allocate and copy over.
        var s2 = appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, i), makeꓸꓸꓸ<E>(tot - i)); // See Insert
        copy(subslice<S, E>(s2, i), v);
        copy(subslice<S, E>(s2, i + len(v)), subslice<S, E>(s, j));
        return s2;
    }
    var r = subslice<S, E>(s, 0, tot);
    if (i + len(v) <= j) {
        // Easy, as v fits in the deleted portion.
        copy(subslice<S, E>(r, i), v);
        copy(subslice<S, E>(r, i + len(v)), subslice<S, E>(s, j));
        clear(subslice<S, E>(s, tot)); // zero/nil out the obsolete elements, for GC
        return r;
    }
    // We are expanding (v is bigger than j-i).
    // The situation is something like this:
    // (example has i=4,j=8,len(s)=16,len(v)=6)
    // s: aaaaxxxxbbbbbbbbyy
    //        ^   ^       ^ ^
    //        i   j  len(s) tot
    // a: prefix of s
    // x: deleted range
    // b: more of s
    // y: area to expand into
    if (!overlaps(new slice<E>(subslice<S, E>(r, i + len(v))), v)) {
        // Easy, as v is not clobbered by the first copy.
        copy(subslice<S, E>(r, i + len(v)), subslice<S, E>(s, j));
        copy(subslice<S, E>(r, i), v);
        return r;
    }
    // This is a situation where we don't have a single place to which
    // we can copy v. Parts of it need to go to two different places.
    // We want to copy the prefix of v into y and the suffix into x, then
    // rotate |y| spots to the right.
    //
    //        v[2:]      v[:2]
    //         |           |
    // s: aaaavvvvbbbbbbbbvv
    //        ^   ^       ^ ^
    //        i   j  len(s) tot
    //
    // If either of those two destinations don't alias v, then we're good.
    nint y = len(v) - (j - i); // length of y portion
    if (!overlaps(new slice<E>(subslice<S, E>(r, i, j)), v)) {
        copy(subslice<S, E>(r, i, j), v.slice(y));
        copy(subslice<S, E>(r, len(s)), v.slice(0, y));
        rotateRight(new slice<E>(subslice<S, E>(r, i)), y);
        return r;
    }
    if (!overlaps(new slice<E>(subslice<S, E>(r, len(s))), v)) {
        copy(subslice<S, E>(r, len(s)), v.slice(0, y));
        copy(subslice<S, E>(r, i, j), v.slice(y));
        rotateRight(new slice<E>(subslice<S, E>(r, i)), y);
        return r;
    }
    // Now we know that v overlaps both x and y.
    // That means that the entirety of b is *inside* v.
    // So we don't need to preserve b at all; instead we
    // can copy v first, then copy the b part of v out of
    // v to the right destination.
    nint k = startIdx(v, new slice<E>(subslice<S, E>(s, j)));
    copy(subslice<S, E>(r, i), v);
    copy(subslice<S, E>(r, i + len(v)), subslice<S, E>(r, i + k));
    return r;
}

// Clone returns a copy of the slice.
// The elements are copied using assignment, so this is a shallow clone.
// The result may have additional unused capacity.
public static S Clone<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    // Preserve nilness in case it matters.
    if (s.IsNil) {
        return default!;
    }
    // Avoid s[:0:0] as it leads to unwanted liveness when cloning a
    // zero-length slice of a large array; see https://go.dev/issue/68488.
    return appendꓸꓸꓸ<S, E>(new S{}, s);
}

// Compact replaces consecutive runs of equal elements with a single copy.
// This is like the uniq command found on Unix.
// Compact modifies the contents of the slice s and returns the modified slice,
// which may have a smaller length.
// Compact zeroes the elements between the new length and the original length.
public static S Compact<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (len(s) < 2) {
        return s;
    }
    for (nint k = 1; k < len(s); k++) {
        if (AreEqual(s[k], s[k - 1])) {
            var s2 = subslice<S, E>(s, k);
            for (nint k2 = 1; k2 < len(s2); k2++) {
                if (!AreEqual(s2[k2], s2[k2 - 1])) {
                    s[k] = s2[k2];
                    k++;
                }
            }
            clear(subslice<S, E>(s, k)); // zero/nil out the obsolete elements, for GC
            return subslice<S, E>(s, 0, k);
        }
    }
    return s;
}

// CompactFunc is like [Compact] but uses an equality function to compare elements.
// For runs of elements that compare equal, CompactFunc keeps the first one.
// CompactFunc zeroes the elements between the new length and the original length.
public static S CompactFunc<S, E>(S s, Func<E, E, bool> eq)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (len(s) < 2) {
        return s;
    }
    for (nint k = 1; k < len(s); k++) {
        if (eq(s[k], s[k - 1])) {
            var s2 = subslice<S, E>(s, k);
            for (nint k2 = 1; k2 < len(s2); k2++) {
                if (!eq(s2[k2], s2[k2 - 1])) {
                    s[k] = s2[k2];
                    k++;
                }
            }
            clear(subslice<S, E>(s, k)); // zero/nil out the obsolete elements, for GC
            return subslice<S, E>(s, 0, k);
        }
    }
    return s;
}

// Grow increases the slice's capacity, if necessary, to guarantee space for
// another n elements. After Grow(n), at least n elements can be appended
// to the slice without another allocation. If n is negative or too large to
// allocate the memory, Grow panics.
public static S Grow<S, E>(S s, nint n)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (n < 0) {
        throw panic("cannot be negative");
    }
    {
        n -= cap(s) - len(s); if (n > 0) {
            // This expression allocates only once (see test).
            s = subslice<S, E>(appendꓸꓸꓸ<S, E>(subslice<S, E>(s, 0, cap(s)), makeꓸꓸꓸ<E>(n)), 0, len(s));
        }
    }
    return s;
}

// Clip removes unused capacity from the slice, returning s[:len(s):len(s)].
public static S Clip<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    return subslice3<S, E>(s, 0, len(s), len(s));
}

// TODO: There are other rotate algorithms.
// This algorithm has the desirable property that it moves each element at most twice.
// The follow-cycles algorithm can be 1-write but it is not very cache friendly.

// rotateLeft rotates s left by r spaces.
// s_final[i] = s_orig[i+r], wrapping around.
internal static void rotateLeft<E>(slice<E> s, nint r) {
    Reverse<slice<E>, E>(s.slice(0, r));
    Reverse<slice<E>, E>(s.slice(r));
    Reverse<slice<E>, E>(s);
}

internal static void rotateRight<E>(slice<E> s, nint r) {
    rotateLeft(s, len(s) - r);
}

// go2cs generated this placeholder — func overlaps is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// startIdx returns the index in haystack where the needle starts.
// prerequisite: the needle must be aliased entirely inside the haystack.
internal static nint startIdx<E>(slice<E> haystack, slice<E> needle) {
    var p = Ꮡ(needle, 0);
    foreach (var (i, _) in haystack) {
        if (p == Ꮡ(haystack, i)) {
            return i;
        }
    }
    // TODO: what if the overlap is by a non-integral number of Es?
    throw panic("needle not found");
}

// Reverse reverses the elements of the slice in place.
public static void Reverse<S, E>(S s)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    for ((nint i, nint j) = (0, len(s) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (s[i], s[j]) = (s[j], s[i]);
    }
}

// Concat returns a new slice concatenating the passed in slices.
public static S Concat<S, E>(params Span<S> slicesʗp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    var slices = slicesʗp.sslice();

    nint size = 0;
    foreach (var (_, s) in slices) {
        size += len(s);
        if (size < 0) {
            throw panic("len out of range");
        }
    }
    // Use Grow, not make, to round up to the size class:
    // the extra space is otherwise unused and helps
    // callers that append a few elements to the result.
    var newslice = Grow<S, E>((S)(default!), size);
    foreach (var (_, s) in slices) {
        newslice = appendꓸꓸꓸ<S, E>(newslice, s);
    }
    return newslice;
}

// Repeat returns a new slice that repeats the provided slice the given number of times.
// The result has length and capacity (len(x) * count).
// The result is never nil.
// Repeat panics if count is negative or if the result of (len(x) * count)
// overflows.
public static S Repeat<S, E>(S x, nint count)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    if (count < 0) {
        throw panic("cannot be negative");
    }
    nuint maxInt = /* ^uint(0) >> 1 */ unchecked((nuint)9223372036854775807);
    var (hi, lo) = bits.Mul((nuint)len(x), (nuint)count);
    if (hi > 0 || lo > maxInt) {
        throw panic("the result of (len(x) * count) overflows");
    }
    var newslice = make<S>((nint)lo); // lo = len(x) * count
    nint n = copy(newslice, x);
    while (n < len(newslice)) {
        n += copy(subslice<S, E>(newslice, n), subslice<S, E>(newslice, 0, n));
    }
    return newslice;
}

} // end slices_package
