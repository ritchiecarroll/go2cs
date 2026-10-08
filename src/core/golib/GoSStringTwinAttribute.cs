// GoSStringTwinAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Records that an exported package-level function of this assembly is an sstring TWIN
/// (docs/phase4/DESIGN-sstring-twin-pilot.md): an <c>sstring</c> member that carries the Go body (see
/// <see cref="GoStrAttribute"/>), plus the <c>@string</c> member go2cs-gen generates for it. The function
/// therefore has no single method group, and a func value of it is the canonical delegate
/// <c>&lt;Name&gt;ᶠ</c>, which a converted package in ANOTHER assembly must name instead of the method
/// group (a method-group conversion is CS0123).
/// </summary>
/// <param name="functionName">The Go name of the function, as declared.</param>
/// <remarks>
/// A consumer reads the record from this package's <c>package_info.cs</c>, or from the converter's
/// embedded standard-library metadata for a published NuGet package, as it reads
/// <see cref="GoRefPrimaryAttribute"/> records. A call site needs no record: overload resolution binds
/// the twin by its priority. Nothing reads the attribute by reflection.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GoSStringTwinAttribute(string functionName) : Attribute
{
    /// <summary>
    /// Gets the Go name of the function.
    /// </summary>
    public string FunctionName => functionName;
}
