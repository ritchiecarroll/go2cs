// GoFuncAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks the lambda or local function a Go function literal converts to with the name Go gives that literal,
/// without its package: <c>outer.func1</c>, <c>outer.func1.2</c>, <c>(*ring).init.func1</c>, <c>gauge.Name.func1</c>,
/// <c>init.0.func1</c> for a literal in the first <c>func init</c>, <c>init.func1</c> for one in a package-level
/// variable's initializer.
/// </summary>
/// <remarks>
/// <para>
/// The C# compiler emits a lambda or local function as a generated method (<c>&lt;outer&gt;b__X_Y</c>,
/// <c>&lt;outer&gt;g__f|X_Y</c>), and an attribute on the lambda attaches to that method, which is what a stack
/// frame's method is. The runtime's frame naming reads <see cref="Name"/> from it and prefixes the package's
/// import path, so <c>runtime.FuncForPC(pc).Name()</c> answers Go's name for a literal from metadata alone: no
/// PDB, and the same under Native AOT.
/// </para>
/// <para>
/// The name is the declared one, Go's name with inlining off. Go renames a literal by its inlining
/// (<c>main.main.(*T).M.func1</c> for a literal in a method inlined into <c>main</c>), which no conversion can
/// predict.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoFuncAttribute(string name) : Attribute
{
    /// <summary>The Go name of the literal, without its package.</summary>
    public string Name { get; } = name;
}
