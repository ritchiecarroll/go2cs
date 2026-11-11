// PointerNilIdentity.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable StaticMemberInGenericType

using System.Collections;
using System.Collections.Generic;

namespace go;

/// <summary>
/// One nil per Go pointer type, for golib code that sees the pointer only as a type argument.
/// </summary>
/// <remarks>
/// <para>
/// A Go nil <c>*T</c> has two C# spellings. A zero, which is a C# default (a field or local never assigned,
/// <c>*new(T)</c>, a declared zero), is <c>null</c>. The <c>nil</c> literal converts to <see cref="ж{T}.NilBox"/>, the
/// canonical typed-nil box, because that conversion is also how a nil pointer enters an interface, where its dynamic
/// type must survive (<c>any((*T)(nil)) != nil</c> in Go). Both are the same Go value, and ж's own operators say so:
/// <c>==</c> treats a null and a nil-constructed box alike. Code that compares through a TYPE ARGUMENT does not reach
/// those operators: generic <c>==</c> (<see cref="builtin.AreEqual{T}(T, T)"/>) and a map key both see two different
/// objects, so typescript-go's <c>core.OrElse(nil, x)</c> (<c>value != *new(T)</c>) returned the nil, generic
/// <c>==</c> answered false, a <c>map[*T]V</c> built in generic code held two nil keys, and an array holding
/// one compared unequal to an array of zeros (TS C49).
/// </para>
/// <para>
/// The rule here is the one ж's operator states: a value whose STATIC type is a Go pointer is nil when it is null or
/// <see cref="INilPointer.IsNilPointer"/>. The test is on the static type, never the value: inside an interface the
/// two nils stay distinct, as Go's interface comparison requires, so a type argument of interface type (<c>any</c>,
/// <c>error</c>) never takes it.
/// </para>
/// <para>
/// The alternative, making the <c>nil</c> conversion produce <c>null</c> and the zero agree with it, would move the
/// typed-nil burden onto every pointer-to-interface conversion in converted and hand-written code alike; this
/// normalizes where golib compares through a type argument (generic <c>==</c>, a map key, an array
/// element) and changes no emitted code.
/// </para>
/// </remarks>
internal static class PointerNilIdentity<T>
{
    /// <summary>
    /// Whether <typeparamref name="T"/> is a Go pointer type: a golib box (<see cref="ж{T}"/>, any kind) or a generated
    /// named-pointer wrapper. Both are classes implementing <see cref="INilPointer"/>; an interface type is not.
    /// Decided once per <typeparamref name="T"/>, from the type alone.
    /// </summary>
    internal static readonly bool IsPointerType = !typeof(T).IsValueType && !typeof(T).IsInterface && typeof(INilPointer).IsAssignableFrom(typeof(T));

    /// <summary>
    /// Whether <paramref name="value"/>, of pointer type <typeparamref name="T"/>, is Go's nil pointer: null, or the
    /// nil-constructed box. Call only when <see cref="IsPointerType"/> holds.
    /// </summary>
    internal static bool IsNil(T? value) => value is null || ((INilPointer)value).IsNilPointer;

    /// <summary>
    /// The element comparer for <see cref="array{T}"/>'s structural equality and hash: for a pointer element type,
    /// one that holds the two nils equal and hashes them alike; otherwise <see cref="EqualityComparer{T}.Default"/>,
    /// exactly as before.
    /// </summary>
    internal static readonly IEqualityComparer ElementComparer = IsPointerType ? new NilNormalizingComparer() : EqualityComparer<T>.Default;

    // IStructuralEquatable calls the non-generic surface, once per element, each side as object.
    private sealed class NilNormalizingComparer : IEqualityComparer
    {
        public new bool Equals(object? x, object? y)
        {
            bool xNil = IsNil((T?)x), yNil = IsNil((T?)y);

            if (xNil || yNil)
                return xNil && yNil;

            return EqualityComparer<T>.Default.Equals((T)x!, (T)y!);
        }

        public int GetHashCode(object obj) => IsNil((T?)obj) ? 0 : EqualityComparer<T>.Default.GetHashCode((T)obj);
    }
}
