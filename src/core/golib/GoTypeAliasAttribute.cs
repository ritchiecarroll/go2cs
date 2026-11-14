// GoTypeAliasAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly with an exported alias for a type.
/// </summary>
/// <param name="alias">Alias name.</param>
/// <param name="typeName">Type name of the alias.</param>
/// <remarks>
/// This attribute is used to define an exported alias for a specific type.
/// When packages are referenced, any defined exported aliases will be imported
/// into the local project as global usings with a package prefix.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoTypeAliasAttribute(string alias, string typeName) : Attribute
{
    /// <summary>
    /// Gets the alias name.
    /// </summary>
    public string Alias => alias;

    /// <summary>
    /// Gets the type name of the alias.
    /// </summary>
    public string TypeName => typeName;
}
