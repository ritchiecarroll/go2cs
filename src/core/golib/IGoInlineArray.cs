// IGoInlineArray.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.ComponentModel;

namespace go.golib;

/// <summary>
/// The reflection face of a Go array held inline (a <c>go.arrays</c> member, docs/phase4/DESIGN-array-value-storage.md, 7.7):
/// its length and a READ of an element, so <c>reflect</c> (and <c>fmt</c> through it) can walk a boxed one as Go walks an
/// array value.
/// </summary>
/// <remarks>
/// It is deliberately not <see cref="IArray"/>: that interface is <c>IEnumerable</c>, which would let a C# <c>foreach</c>
/// over a member compile, and a member exposes no enumeration (I0's rule that an unanalysed face is a compile error). A
/// member implements it explicitly, so nothing but a boxed member reaches it.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IGoInlineArray
{
    /// <summary>The Go array's length.</summary>
    nint Length { get; }

    /// <summary>A copy of the element at <paramref name="index"/>, boxed.</summary>
    object? this[nint index] { get; }
}
