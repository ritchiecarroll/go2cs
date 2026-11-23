// GoCgoStructAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Marks a struct cmd/cgo declared for a C struct that has a pointer or <c>unsafe.Pointer</c> field (cgo Phase 3,
/// increment 3.2, part 1a: docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9).
/// </summary>
/// <remarks>
/// <para>
/// go2cs-gen reads the marker, never the type's name, however it was stamped. For each such field it emits a GET/SET
/// property in place of the <c>ref</c> one: a <c>ж&lt;U&gt;</c> field reads and writes through
/// <see cref="ж{T}.cgoPointerField{U}(FieldRefFunc{T, ж{U}})"/>, and an <c>unsafe.Pointer</c> field through
/// <c>unsafe.Pointer.LoadWord</c> and <c>StoreWord</c> over <see cref="ж{T}.cgoWordField{TF}"/>. In C memory the field
/// is a word at its Go offset, where a <c>ref</c> to the struct's CLR slot would name the wrong bytes, so a <c>ref</c> to
/// such a field does not compile, by design.
/// </para>
/// <para>
/// The converter stamps it only for a package on the -cgo route, so the converted standard library carries none.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Struct)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoCgoStructAttribute : Attribute
{
}
