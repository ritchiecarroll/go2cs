// GoInlineLayoutAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks a struct the converter admitted to inline arrays (<c>-inline-arrays</c>; docs/phase4/DESIGN-array-value-storage.md,
/// 7.7.4): its arrays are members of the <c>go.arrays</c> family, its <c>[0]T</c> fields are alignment carriers, and its
/// fields sit at Go's offsets.
/// </summary>
/// <remarks>
/// go2cs-gen implements <see cref="IGoManagedLayout"/> for every struct carrying it, so golib's inline-layout gate
/// (<c>GoReflect.InlineLayoutMismatch</c>) can compare the struct's managed layout with its Go layout at every level.
/// The converter stamps Go's <see cref="Size"/> and <see cref="Align"/> from go/types, so the gate holds the managed
/// layout to a source independent of the declaration it checks: a carrier missing from the declaration takes Go's
/// alignment with it, and only the stamp still says what Go's alignment is.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineLayoutAttribute : Attribute
{
    /// <summary>
    /// Gets or sets Go's size of the struct, in bytes, as go/types computes it; 0 when not stamped.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets Go's alignment of the struct, in bytes, as go/types computes it; 0 when not stamped.
    /// </summary>
    public int Align { get; set; }
}
