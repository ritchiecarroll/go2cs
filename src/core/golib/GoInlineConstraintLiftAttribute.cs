// GoInlineConstraintLiftAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly with the lifted C# name its conversion gave an inline constraint that names the
/// parameter it constrains: <c>type Ord[T interface{ Less(T) bool }]</c> lifts to <c>Ord_T&lt;T&gt;</c>.
/// </summary>
/// <param name="owner">The Go name of the generic type or function declaring the type parameter.</param>
/// <param name="index">The type parameter's position in the owner's type parameter list.</param>
/// <param name="typeName">The C# type name the constraint was lifted under.</param>
/// <remarks>
/// Keyed by DECLARATION, never by signature: the lift is generic over the parameters it captures, so
/// its Go text means something else in every declaration. A package instantiating the owner reads the
/// name here, so <c>lib.Ord[age]</c> can record <c>age</c>'s implementation of <c>lib.Ord_T&lt;age&gt;</c>
/// and <c>lib.Ord[*elem.Node]</c> can name the lift's generic constraint proxy.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GoInlineConstraintLiftAttribute(string owner, int index, string typeName) : Attribute
{
    /// <summary>
    /// Gets the Go name of the declaration owning the constrained type parameter.
    /// </summary>
    public string Owner => owner;

    /// <summary>
    /// Gets the constrained type parameter's position in the owner's type parameter list.
    /// </summary>
    public int Index => index;

    /// <summary>
    /// Gets the C# type name the constraint was lifted under.
    /// </summary>
    public string TypeName => typeName;
}
