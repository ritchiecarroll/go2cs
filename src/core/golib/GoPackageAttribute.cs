// GoPackageAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a class as a Go package.
/// </summary>
/// <param name="packageName">Defines the package name.</param>
[AttributeUsage(AttributeTargets.Class)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoPackageAttribute(string packageName) : Attribute
{
    /// <summary>
    /// Gets the package name.
    /// </summary>
    public string PackageName => packageName;

    /// <summary>
    /// Gets or sets the package's VERBATIM Go import path, stamped by the converter only where the
    /// emission cannot reproduce it: the C# namespace flattens the path with '.', so a '.' inside a
    /// segment (<c>example.com/x</c>, <c>vendor/golang.org/x/...</c>) is indistinguishable from a
    /// separator, and a major-version directory (<c>/v2</c>) or a package name that differs from its
    /// directory is not in the namespace at all. Every package-path reader (reflect's PkgPath, frame
    /// and FuncForPC names, synthesized struct containers) reads this first; null means the
    /// namespace-plus-name derivation is exact.
    /// </summary>
    public string? ImportPath { get; set; }
}
