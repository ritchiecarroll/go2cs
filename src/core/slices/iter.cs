// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cmp = cmp_package;
using iter = iter_package;

partial class slices_package {

// All returns an iterator over index-value pairs in the slice
// in the usual order.
public static iter.Seq2<nint, E> All<Slice, E>(Slice s)
    where Slice : /* ~[]E */ ISlice<E>, ISupportMake<Slice>, ISliceWrap<Slice, E>, new()
{
    return (Func<nint, E, bool> yield) => {
        foreach (var (i, v) in s) {
            if (!yield(i, v)) {
                return;
            }
        }
    };
}

// Backward returns an iterator over index-value pairs in the slice,
// traversing it backward with descending indices.
public static iter.Seq2<nint, E> Backward<Slice, E>(Slice s)
    where Slice : /* ~[]E */ ISlice<E>, ISupportMake<Slice>, ISliceWrap<Slice, E>, new()
{
    return (Func<nint, E, bool> yield) => {
        for (nint i = len(s) - 1; i >= 0; i--) {
            if (!yield(i, s[i])) {
                return;
            }
        }
    };
}

// Values returns an iterator that yields the slice elements in order.
public static iter.Seq<E> Values<Slice, E>(Slice s)
    where Slice : /* ~[]E */ ISlice<E>, ISupportMake<Slice>, ISliceWrap<Slice, E>, new()
{
    return (Func<E, bool> yield) => {
        foreach (var (_, v) in s) {
            if (!yield(v)) {
                return;
            }
        }
    };
}

// AppendSeq appends the values from seq to the slice and
// returns the extended slice.
// If seq is empty, the result preserves the nilness of s.
public static Slice AppendSeq<Slice, E>(Slice s, iter.Seq<E> seq)
    where Slice : /* ~[]E */ ISlice<E>, ISupportMake<Slice>, ISliceWrap<Slice, E>, new()
{
    foreach (var v in range<E>(seq.Invoke)) {
        s = append(s, v);
    }
    return s;
}

// Collect collects values from seq into a new slice and returns it.
// If seq is empty, the result is nil.
public static slice<E> Collect<E>(iter.Seq<E> seq) {
    return AppendSeq(slice<E>(default!), seq);
}

// Sorted collects values from seq into a new slice, sorts the slice,
// and returns it.
// If seq is empty, the result is nil.
public static slice<E> Sorted<E>(iter.Seq<E> seq)
    where E : /* cmp.Ordered */ IAdditionOperators<E, E, E>, IEqualityOperators<E, E, bool>, IComparisonOperators<E, E, bool>, new()
{
    var s = Collect(seq);
    Sort<slice<E>, E>(s);
    return s;
}

// SortedFunc collects values from seq into a new slice, sorts the slice
// using the comparison function, and returns it.
// If seq is empty, the result is nil.
public static slice<E> SortedFunc<E>(iter.Seq<E> seq, Func<E, E, nint> cmp) {
    var s = Collect(seq);
    SortFunc(s, cmp);
    return s;
}

// SortedStableFunc collects values from seq into a new slice.
// It then sorts the slice while keeping the original order of equal elements,
// using the comparison function to compare elements.
// It returns the new slice.
// If seq is empty, the result is nil.
public static slice<E> SortedStableFunc<E>(iter.Seq<E> seq, Func<E, E, nint> cmp) {
    var s = Collect(seq);
    SortStableFunc(s, cmp);
    return s;
}

// Chunk returns an iterator over consecutive sub-slices of up to n elements of s.
// All but the last sub-slice will have size n.
// All sub-slices are clipped to have no capacity beyond the length.
// If s is empty, the sequence is empty: there is no empty slice in the sequence.
// Chunk panics if n is less than 1.
public static iter.Seq<Slice> Chunk<Slice, E>(Slice s, nint n)
    where Slice : /* ~[]E */ ISlice<E>, ISupportMake<Slice>, ISliceWrap<Slice, E>, new()
{
    if (n < 1) {
        throw panic("cannot be less than 1");
    }

    return (Func<Slice, bool> yield) => {
        for (nint i = 0; i < len(s); i += n) {
            // Clamp the last chunk to the slice bound as necessary.
            nint end = min(n, len(subslice<Slice, E>(s, i)));

            // Set the capacity of each chunk so that appending to a chunk does
            // not modify the original slice.
            if (!yield(subslice3<Slice, E>(s, i, i + end, i + end))) {
                return;
            }
        }
    };
}

} // end slices_package
