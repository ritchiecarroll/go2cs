// GoTypeAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;

namespace go;

/// <summary>
/// Marks a struct or interface for automatic code generation.
/// </summary>
/// <param name="definition">Type definition string, if applicable.</param>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate backend C# type code needed to
/// emulate Go behaviour for a type definition.
/// </para>
/// <para>
/// See the <c>TypeGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
/// <param name="definition">Type definition string.</param>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Class)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoTypeAttribute(string definition = "") : Attribute
{
    /// <summary>
    /// Gets the type definition string.
    /// </summary>
    public string Definition => definition;
}
