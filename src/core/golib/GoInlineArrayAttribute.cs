// GoInlineArrayAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks a type of the inline-array family (<c>go.arrays.array&lt;N&gt;&lt;T&gt;</c>, in <c>go.arrays.dll</c>): a Go array
/// value <c>[N]T</c> held inline in its owner, so a struct holding one lays out as Go's does.
/// </summary>
/// <remarks>
/// golib recognises the family by this attribute and never names a family type, so a program that does not opt in to
/// inline arrays never loads <c>go.arrays.dll</c> (docs/phase4/DESIGN-array-value-storage.md, 7.7, P7). Reflection
/// reads <see cref="Length"/> for a field's Go size and dimensions.
/// </remarks>
/// <param name="length">The Go array's length, <c>N</c>.</param>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineArrayAttribute(int length) : Attribute
{
    /// <summary>
    /// Gets the Go array's length.
    /// </summary>
    public int Length => length;
}
