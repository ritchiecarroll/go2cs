// GoMethodBodyAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// SCRATCH PROTOTYPE (the global ᴍ cost test; never for master as it stands): marks the BODY of a Go method, a plain
/// static named <c>&lt;name&gt;ᴍ</c> in the package class whose first parameter is the receiver, and names the Go method.
/// </summary>
/// <remarks>
/// The Go-named extension method in the type's holder forwards to it. Frame naming reads this mark, never the
/// spelling, so a Go identifier that ends in the glyph is never taken for a body, and a body whose name the converter
/// doubled to stay clear of one (<c>&lt;name&gt;ᴍᴍ</c>) still names its method.
/// </remarks>
/// <param name="goName">The Go method's name.</param>
[AttributeUsage(AttributeTargets.Method)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoMethodBodyAttribute(string goName) : Attribute
{
    /// <summary>Gets the Go method's name.</summary>
    public string GoName { get; } = goName;
}
