// GoAlignmentCarrierAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks the field that stands for a Go zero-length array field (<c>[0]T</c>) in a struct whose arrays are held inline: a
/// readonly primitive of the element's ALIGNMENT at the field's Go offset (docs/phase4/DESIGN-array-value-storage.md,
/// 7.6.2).
/// </summary>
/// <remarks>
/// <para>
/// Go gives a <c>[0]T</c> field no bytes and <c>T</c>'s alignment, so it can raise its struct's alignment and add
/// trailing padding. The CLR has no zero-size field; the carrier gives the struct the alignment and leaves its size to
/// the explicit layout. It SHARES bytes with the field Go places at the same offset, so it is readonly, and equality,
/// hashing and reflection skip it.
/// </para>
/// <para>
/// Reflection projects the field as the Go field it stands for: the type <see cref="GoType"/> (<c>array&lt;T&gt;</c>) of
/// length 0, whose Go size is 0 and whose Go alignment is <c>T</c>'s.
/// </para>
/// </remarks>
/// <param name="goType">The managed type of the Go field, <c>array&lt;T&gt;</c>: what go2cs declares for <c>[0]T</c>
/// outside an inline-array struct.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoAlignmentCarrierAttribute(Type goType) : Attribute
{
    /// <summary>
    /// Gets the managed type of the Go zero-length array field the carrier stands for.
    /// </summary>
    public Type GoType => goType;
}
