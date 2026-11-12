// GoMapKeyDimsAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Carries the Go DIMENSION of a map field's KEY type, outermost first
/// (<c>map[[2]string]V</c> ⇒ <c>[GoMapKeyDims(2)]</c>).
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="GoArrayDimsAttribute"/> twin for the one accessor that has no other slot. A
/// descriptor's carried dims describe what <c>Elem()</c> hands down — an array's own tail, a
/// pointer's pointee, a map's element — and <c>Key()</c> is the second accessor a map type has, so
/// a map whose KEY is a fixed-size array (<c>map[[2]string][2]*float64</c>, encoding/gob's
/// <c>T1.Marr</c>) needs a second carrier or the key's length is lost while the element's survives.
/// </para>
/// <para>
/// Like the array dims on a field, this is stamped only where the datum has no live source: a map
/// KEY is a type-only position, reached through <c>reflect.Type.Key()</c> from a descriptor that a
/// nil map answers for just as well as a populated one. It rides through a POINTER's <c>Elem()</c>
/// unshifted, the same hop the array dims and the channel direction take.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoMapKeyDimsAttribute(params int[] dims) : Attribute
{
    /// <summary>The Go array dimensions of the map's key type, outermost first.</summary>
    public int[] Dims { get; } = dims;
}
