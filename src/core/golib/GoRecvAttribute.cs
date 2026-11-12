// GoRecvAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a reference based receiver method for automatic code generation.
/// </summary>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate a pointer receiver method that
/// references a <see cref="ж{T}"/> type which calls this receiver method
/// declared with a <see langword="ref" /> type.
/// </para>
/// <para>
/// See the <c>RecvGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
/// <param name="options">Receiver options string.</param>
[AttributeUsage(AttributeTargets.Method)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoRecvAttribute(string options = "") : Attribute
{
    /// <summary>
    /// Gets the receiver options string.
    /// </summary>
    public string Options => options;
}
