// IGoZeroShaped.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// Marks a golib value whose Go ZERO VALUE carries run-time SHAPE that its C# type does not, so
/// <c>default(T)</c> is not usable storage for it.
/// </summary>
/// <remarks>
/// The one such shape is the fixed-size <see cref="array{T}"/>: <c>[4]int32</c> and <c>[8]int32</c>
/// are the SAME C# type (<c>array&lt;int32&gt;</c>) and the Go length exists only in the instance,
/// so a zero value can only be produced from a value that already has the shape. See
/// <see cref="builtin.GoZero{T}"/>, which is where a built-in like <c>clear</c> asks for one.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IGoZeroShaped
{
    /// <summary>
    /// Gets a NEW value of this type shaped exactly like this one, with every element the Go zero
    /// value of the element type (recursively, for a nested shape).
    /// </summary>
    object GoZeroLike();
}
