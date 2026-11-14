// GoInlineArraysAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Records how the converter chose this package's inline-array structs when that was not its default rule: <c>"forced"</c>
/// (<c>-inline-arrays=&lt;pattern&gt;</c>, every struct the package declares or spells that holds an array) or <c>"off"</c>
/// (<c>-inline-arrays=-&lt;pattern&gt;</c>, none). The default rule, a struct the package converts whole through
/// <c>unsafe.Pointer</c> and every struct it holds by value, is recorded by its <see cref="GoInlineArrayTypeAttribute"/>
/// records alone (docs/phase4/DESIGN-array-value-storage.md, 7.7).
/// </summary>
/// <remarks>
/// A converter record, read from <c>package_info.cs</c>: a <c>-tests</c> conversion of the package applies the same rule
/// rather than deciding again. Nothing reads the attribute by reflection.
/// </remarks>
/// <param name="mode">The rule: <c>"forced"</c> or <c>"off"</c>.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInlineArraysAttribute(string mode) : Attribute
{
    /// <summary>
    /// Gets the rule the converter applied: <c>"forced"</c> or <c>"off"</c>.
    /// </summary>
    public string Mode => mode;
}
