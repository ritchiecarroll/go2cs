// GoRefPrimaryAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Records that an exported pointer-receiver method of this assembly carries a <c>ref</c>-receiver
/// PRIMARY (<c>[GoRecv] this ref T</c>) beside its <c>ж&lt;T&gt;</c> twin, so a converted package in
/// ANOTHER assembly can bind the primary at a ref-addressable call site instead of minting a box.
/// </summary>
/// <param name="typeName">The Go name of the receiver's struct type, as declared.</param>
/// <param name="methodName">The Go name of the method, as declared.</param>
/// <remarks>
/// <para>
/// This is the cross-package lowering CONTRACT's record (docs/phase4/DESIGN-zh-box-three-capabilities.md
/// §3.2): the declaring package PUBLISHES its verdict, and a consumer reads it from this package's
/// <c>package_info.cs</c> (or, for a published NuGet package, from the converter's embedded
/// standard-library metadata) exactly as it reads <see cref="GoImplementAttribute{TStruct, TInterface}"/>
/// records today. Both names are the GO spellings so neither side has to agree on an emitted C# alias.
/// </para>
/// <para>
/// A record is written only for a declaration a foreign package can bind — an EXPORTED method on an
/// EXPORTED type — and the section that carries these records is omitted from <c>package_info.cs</c>
/// entirely when it would be empty. Nothing reads the attribute by reflection; it is a compile-time
/// record for the converter, published where the assembly's other cross-package facts already live.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoRefPrimaryAttribute(string typeName, string methodName) : Attribute
{
    /// <summary>
    /// Gets the Go name of the receiver's struct type.
    /// </summary>
    public string TypeName => typeName;

    /// <summary>
    /// Gets the Go name of the method.
    /// </summary>
    public string MethodName => methodName;
}
