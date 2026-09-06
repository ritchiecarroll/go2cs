// GoEmbeddedAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a struct field the converter emitted as a PLAIN field for a Go EMBEDDED field of a predeclared
/// type (<c>struct{ int }</c>, <c>struct{ *byte }</c>). Every other embed is emitted in the promoted
/// <c>partial ref</c> shape the reflection projection already keys on; a builtin has nothing to promote,
/// so without this stamp the projection could not tell <c>struct{ int }</c> from a field NAMED
/// <c>int</c> of type <c>int</c> -- both legal Go -- and <c>StructField.Anonymous</c> read false
/// (reflect's TestFieldPkgPath, issue 21702). Increment E2b of the reflect tail.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class GoEmbeddedAttribute : Attribute
{
}
