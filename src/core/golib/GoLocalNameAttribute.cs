// GoLocalNameAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Carries the ORIGINAL Go source name of a function-local type the converter lifted to package
/// scope under a function-prefixed C# name (<c>type Person struct</c> inside
/// <c>TestNoFixedSize</c> lifts to <c>TestNoFixedSize_Person</c>). The reflection bridge's
/// Go type naming (<c>GoReflect.GoTypeName</c> → <c>reflect.Type.String()</c>, <c>%T</c>)
/// prefers this name, so a lifted local type prints Go's <c>binary.Person</c>, never the lifted
/// C# identifier. Deliberately a SEPARATE attribute — the <c>[GoType]</c> definition slot is
/// parsed by exact match in the TypeGenerator and must not carry extra tokens (I2.R R-8).
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoLocalNameAttribute(string name) : Attribute
{
    /// <summary>The original Go source-local type name.</summary>
    public string Name { get; } = name;
}
