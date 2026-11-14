// GoInlineArrayTypeAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Records a named struct type of this package whose arrays the converter holds inline (members of the <c>go.arrays</c>
/// family; docs/phase4/DESIGN-array-value-storage.md, 7.7).
/// </summary>
/// <remarks>
/// A converter record, read from <c>package_info.cs</c>. A <c>-tests</c> conversion of the package checks the types it
/// admits against these records, and a package that reinterprets one of these types through <c>unsafe.Pointer</c> as a
/// type of its own reads them to see whether the two layouts agree. Nothing reads the attribute by reflection.
/// </remarks>
/// <param name="typeName">The Go name of the struct type, as declared.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineArrayTypeAttribute(string typeName) : Attribute
{
    /// <summary>
    /// Gets the Go name of the struct type.
    /// </summary>
    public string TypeName => typeName;
}
