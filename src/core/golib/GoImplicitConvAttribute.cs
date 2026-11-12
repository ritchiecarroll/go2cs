// GoImplicitConvAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly with an implicit conversion mapping.
/// </summary>
/// <typeparam name="TSource">Source type for implicit conversion.</typeparam>
/// <typeparam name="TTarget">Target type for implicit conversion.</typeparam>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate backend C# code needed to implement
/// an implicit conversion from a source type to a target type.
/// </para>
/// <para>
/// See the <c>ImplicitConvGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoImplicitConvAttribute<TSource, TTarget> : Attribute
{
    /// <summary>
    /// Gets or sets flag that determines if operator conversion order should be inverted.
    /// </summary>
    public bool Inverted { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if pointer conversion should be based on value.
    /// </summary>
    /// <remarks>
    /// Implicit C# conversion operators require one of the parameters to be the source instance.
    /// </remarks>
    public bool Indirect { get; set; }

    /// <summary>
    /// Gets or sets the value type of the target type.
    /// </summary>
    /// <remarks>
    /// If value is defined, conversion is from one aliased type to another.
    /// </remarks>
    public string? ValueType { get; set; }
}
