// GoInternalRootAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks the assembly of a Go internal or vendored package: Go lets only code at or below <see cref="Root"/> import it.
/// </summary>
/// <remarks>
/// The converter stamps it in the package's <c>package_info.cs</c>. The package stays public to .NET, so go2cs-gen and
/// go.lib's consumer analyzer read this attribute and warn (GO2CS0014) on hand-written C# that names one of the
/// package's types from outside its root. Go internal and vendored packages are not covered by any compatibility
/// promise.
/// </remarks>
/// <param name="root">The import path of the root; <c>""</c> is the standard library's root (GOROOT/src).</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInternalRootAttribute(string root) : Attribute
{
    /// <summary>
    /// Gets the import path of the root; <c>""</c> is the standard library's root.
    /// </summary>
    public string Root => root;

    /// <summary>
    /// Gets or sets whether the root is the standard library's, which Go tests by directory rather than by import path:
    /// only standard-library code may then import the package.
    /// </summary>
    public bool Standard { get; set; }
}
