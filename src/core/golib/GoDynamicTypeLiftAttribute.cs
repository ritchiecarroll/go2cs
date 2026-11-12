// GoDynamicTypeLiftAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly with the lifted C# name production's own conversion gave a purely-anonymous
/// (no Go-level <c>type X = ...</c> alias) struct or interface type.
/// </summary>
/// <param name="signature">Structural signature (<c>types.Type.String()</c>) of the anonymous type.</param>
/// <param name="typeName">The C# type name production lifted it under.</param>
/// <remarks>
/// A <c>-tests</c> reference-model conversion does not recompile production's sources, so nothing
/// visits the declaration that lifted this type and nothing records the name a cross-file (here,
/// cross-ASSEMBLY) reference needs. Sibling of <see cref="GoTypeAliasAttribute"/> for the case that
/// attribute does not cover: this is not an alias declaration, only a converter-synthesized name
/// (e.g. <c>ifaceHash_i</c> for an inline <c>interface{F()}</c> parameter type) with no Go source
/// identifier of its own.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoDynamicTypeLiftAttribute(string signature, string typeName) : Attribute
{
    /// <summary>
    /// Gets the anonymous type's structural signature.
    /// </summary>
    public string Signature => signature;

    /// <summary>
    /// Gets the C# type name production lifted it under.
    /// </summary>
    public string TypeName => typeName;
}
