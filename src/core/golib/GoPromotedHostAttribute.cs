// GoPromotedHostAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks the class go2cs-gen generates to hold one struct's promoted-method forwarders, and names that struct.
/// </summary>
/// <remarks>
/// <para>
/// A method a struct promotes from an embedded field of its own package is an extension method on the struct,
/// like a method the struct declares. A declared method is in the package class. The forwarders are in a class
/// of their own, one per struct, beside the package class in the package's namespace, so that the package class
/// holds only what the Go package declares: CoreCLR loads no static class with more than 65,521 methods, and a
/// package of many structs that embed one base type promotes far more than that.
/// </para>
/// <para>
/// A reader that wants a struct's methods from metadata finds the class by this attribute, never by deriving
/// its name: <c>[GoPromotedHost(typeof(T))]</c> for a struct <c>T</c>, and <c>typeof(T&lt;&gt;)</c> for a generic one.
/// </para>
/// </remarks>
/// <param name="struct">The struct whose promoted methods the class holds.</param>
[AttributeUsage(AttributeTargets.Class)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoPromotedHostAttribute(Type @struct) : Attribute
{
    /// <summary>
    /// Gets the struct whose promoted methods the class holds.
    /// </summary>
    public Type Struct { get; } = @struct;
}
