// GoInlineArraySeedAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Names a type this package admitted because an <c>-inline-arrays=&lt;import path&gt;#&lt;TypeName&gt;</c> seed selected it,
/// as if the package converted it whole through <c>unsafe.Pointer</c> (docs/phase4/DESIGN-array-value-storage.md, 7.7,
/// ruling C).
/// </summary>
/// <remarks>
/// A converter record, read from <c>package_info.cs</c>. A <c>-tests</c> conversion seeds the same names, so it admits
/// what production admitted. The seed exists for a type an importer converts whole and passes back by value, such as
/// modernc.org/libc's <c>largeInteger</c> under modernc.org/sqlite on windows. Nothing reads the attribute by
/// reflection.
/// </remarks>
/// <param name="name">The Go name of the seeded type or alias.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineArraySeedAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the Go name of the seeded type or alias.
    /// </summary>
    public string Name => name;
}
