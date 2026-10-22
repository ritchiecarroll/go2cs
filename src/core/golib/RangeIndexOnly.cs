// RangeIndexOnly.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

namespace go;

/// <summary>
/// Go's index-only range over a slice or an array, <c>for i := range x</c> (and <c>for i, _ := range x</c>, and
/// <c>for range x</c>), as the converter emits it: <c>foreach (var i in range(x))</c>.
/// </summary>
/// <remarks>
/// <para>
/// Each overload reads the operand's LENGTH once, when the loop starts, and returns the allocation-free index
/// enumerator of range-over-int (<see cref="RangeOverInt{T}"/>), so the loop yields 0..len(x)-1 and never reads an
/// element. Go's index-only range loads no element either. Ranging through the (index, value) enumerator instead,
/// with the value discarded, still read every element into a stack temp: an element the body waits to see collected
/// stayed reachable from the loop's own frame (crypto/internal/fips140cache's TestCache never finished), and an
/// element of a native-backed slice over memory that refuses a read panicked where Go touches nothing.
/// </para>
/// <para>
/// A slice and an array bind their own overloads by identity. A named slice or array type, or a type parameter whose
/// core type is one, binds the constrained <c>range&lt;TSeq&gt;(in TSeq)</c>. None of these is ambiguous with the
/// integer <see cref="range{T}(T)"/>: its constraint rejects them.
/// </para>
/// </remarks>
public static partial class builtin
{
    /// <summary>
    /// Enumerates the indices of a slice, 0..len(s)-1, reading no element.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="s">Slice to range over.</param>
    /// <returns>The indices, as Go's <c>int</c>.</returns>
    public static RangeOverInt<nint> range<T>(slice<T> s) => range(s.Length);

    /// <summary>
    /// Enumerates the indices of a variadic parameter's slice, 0..len(s)-1, reading no element.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="s">The stack slice a variadic parameter is emitted as.</param>
    /// <returns>The indices, as Go's <c>int</c>.</returns>
    /// <remarks>
    /// <see cref="sslice{T}"/> is a ref struct, so no other overload can take it: without this one C# reaches the
    /// integer <see cref="range{T}(T)"/> and refuses the ref struct as its type argument (CS9244).
    /// </remarks>
    public static RangeOverInt<nint> range<T>(sslice<T> s) => range(s.Length);

    /// <summary>
    /// Enumerates the indices of an array, 0..len(a)-1, reading no element.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="a">Array to range over.</param>
    /// <returns>The indices, as Go's <c>int</c>.</returns>
    public static RangeOverInt<nint> range<T>(array<T> a) => range(a.Length);

    /// <summary>
    /// Enumerates the indices of the array a pointer addresses, 0..len(*p)-1, reading no element.
    /// </summary>
    /// <typeparam name="TArray">The array type, an <see cref="array{T}"/> or a named array.</typeparam>
    /// <param name="p">Pointer to the array.</param>
    /// <returns>The indices, as Go's <c>int</c>.</returns>
    /// <remarks>
    /// The length is read through the pointer. Go's length of a <c>*[N]T</c> is the constant N, so Go ranges a nil
    /// one N times; golib's array carries its length in the value, not the type, so a nil pointer panics here, as
    /// the <c>p.Value</c> the converter emitted before did.
    /// </remarks>
    public static RangeOverInt<nint> range<TArray>(ж<TArray> p) where TArray : IArray => range(p.Value.Length);

    /// <summary>
    /// Enumerates the indices of a named slice or array type, or of a type parameter whose core type is one,
    /// 0..len(x)-1, reading no element.
    /// </summary>
    /// <typeparam name="TSeq">The sequence type.</typeparam>
    /// <param name="x">Sequence to range over.</param>
    /// <returns>The indices, as Go's <c>int</c>.</returns>
    /// <remarks>
    /// Generic and constrained, so a struct sequence is read without boxing (a type parameter of <c>slices</c>, for one,
    /// is a struct). It takes its operand <c>in</c> because the integer <see cref="range{T}(T)"/> already holds the
    /// by-value signature: C# overloads by-value against <c>in</c>, and a sequence fails that overload's constraint, so
    /// this one binds. <c>in</c> also spares the copy of a large named array.
    /// </remarks>
    public static RangeOverInt<nint> range<TSeq>(in TSeq x) where TSeq : IArray => range(x.Length);
}
