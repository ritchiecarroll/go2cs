// GoParamDimsAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Carries the Go array dims of one PARAMETER of one method of the decorated type, which converted code
/// states in a comment before the parameter's type (<c>/*[32]*/ array&lt;byte&gt; hash</c>) rather than as
/// <see cref="GoArrayDimsAttribute"/> on the parameter (docs/PLAN-marker-comment-parity.md, 5.4).
/// </summary>
/// <remarks>
/// <para>
/// Never written by the converter. go2cs-gen's <c>MemberRecordGenerator</c> reads the comment and emits this
/// on a GENERATED partial of the method's declaring type (the package class for a func or a method), since a
/// generated file cannot add an attribute to another part's parameter. The method is identified as
/// <see cref="GoSigChanDirAttribute"/> identifies it: by <see cref="Method"/> and <see cref="ParameterTypes"/>
/// (the C# parameter types, receiver included; a <c>ref</c> parameter by its element type), which the
/// generator writes as <c>typeof</c> expressions the compiler resolves, so no type is ever matched by name.
/// </para>
/// <para>
/// <c>GoReflect.ParamDimsRecords</c> resolves each record when its type is first read and refuses BY NAME
/// one that matches no declared method, matches more than one, names a position the method does not have,
/// or stamps a position whose type is not an array or a pointer to one. A lambda's or a local function's
/// parameter keeps the attribute: neither has a name a record can key.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class GoParamDimsAttribute(string method, Type[] parameterTypes, int position, params long[] dims) : Attribute
{
    /// <summary>The C# method name.</summary>
    public string Method { get; } = method;

    /// <summary>The C# parameter types, receiver included; a <c>ref</c> parameter by its element type.</summary>
    public Type[] ParameterTypes { get; } = parameterTypes;

    /// <summary>The C# parameter position the dims belong to.</summary>
    public int Position { get; } = position;

    /// <summary>The Go array dims, outermost first (the pointee's for a pointer to an array).</summary>
    public long[] Dims { get; } = dims;
}
