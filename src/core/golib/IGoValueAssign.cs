// IGoValueAssign.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go.golib;

/// <summary>
/// Go's whole assignment INTO an existing location, for a value that owns array storage: an
/// <see cref="array{T}"/>, a generated named-array wrapper, or a generated
/// <see cref="GoValueCloneAttribute"/> struct.
/// </summary>
/// <remarks>
/// <para>
/// In Go an array variable IS its storage, so <c>a = b</c> copies <c>b</c>'s elements into it, and a
/// slice, an element pointer or an address taken from <c>a</c> beforehand still names <c>a</c>
/// afterwards (docs/phase4/DESIGN-array-reassign-in-place.md). The emitted array is a struct over a
/// shared backing, so the in-place copy writes that backing and leaves the struct itself in place.
/// </para>
/// <para>
/// This member is for a caller that holds the value only as <typeparamref name="TSelf"/>: the
/// per-element walk in <see cref="ArrayExtensions.Assign{T}(ref array{T}, in array{T})"/>, where a
/// generic element cannot take a <c>ref this</c> call, and reflect's store through a pointer box.
/// The cast to this interface boxes a COPY of the receiver; the copy shares the receiver's
/// backings, so an in-place write lands, and the returned value is what the caller writes back into
/// the location. The write-back is what carries the parts that are not shared: a backing installed
/// where there was none, and a struct's by-value fields (P5's C3).
/// </para>
/// <para>
/// A new interface rather than a member of <see cref="IGoValueClone"/>: an abstract member added to
/// a published interface breaks every implementer outside this assembly (ApiCompat CP0006).
/// </para>
/// </remarks>
/// <typeparam name="TSelf">The implementing value type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGoValueAssign<TSelf>
{
    /// <summary>
    /// Copies <paramref name="source"/> into this value's own storage and returns this value, for
    /// the caller to store back into the location it came from.
    /// </summary>
    TSelf AssignedFrom(in TSelf source);
}

/// <summary>
/// The in-place assignment of one location, for generated code that knows the location's type only
/// as a type argument (a <see cref="GoValueCloneAttribute"/> struct's array and struct fields).
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GoValueAssign
{
    /// <summary>
    /// Go's <c>dest = source</c> into <paramref name="dest"/>'s own storage. The interface constraint
    /// makes the call a constrained call on the location itself, so nothing is boxed.
    /// </summary>
    public static void Into<T>(ref T dest, in T source) where T : IGoValueAssign<T> => dest = dest.AssignedFrom(source);
}
