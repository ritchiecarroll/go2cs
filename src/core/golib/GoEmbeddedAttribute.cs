// GoEmbeddedAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a struct field the converter emitted as a PLAIN field for a Go EMBEDDED field: one of a
/// predeclared type (<c>struct{ int }</c>, <c>struct{ *byte }</c>) or of an INTERFACE type
/// (<c>struct{ io.Reader }</c>). An embedded struct is emitted in the promoted <c>partial ref</c> shape
/// the reflection projection already keys on; these two have no such shape, so without the stamp the
/// projection could not tell <c>struct{ int }</c> from a field NAMED <c>int</c> of type <c>int</c> --
/// both legal Go -- and <c>StructField.Anonymous</c> read false (reflect's TestFieldPkgPath, issue
/// 21702). Increment E2b of the reflect tail.
/// </summary>
/// <remarks>
/// On an interface embed the stamp has a second reader, go2cs-gen: the interface's methods are
/// PROVIDERS of their names in the struct's embed tree, and a method promoted through another
/// package's embed is minted only when its name is unique there. The generator takes a stamped field
/// as a provider only when its type is an interface; a predeclared embed carries the same attribute and
/// provides nothing.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class GoEmbeddedAttribute : Attribute
{
}
