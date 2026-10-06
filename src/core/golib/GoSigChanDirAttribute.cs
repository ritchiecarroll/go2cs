// GoSigChanDirAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Carries the channel DIRECTION of each parameter and result of one method of the decorated type, which
/// the method's C# signature cannot hold: <c>func(c &lt;-chan int)</c> and <c>func(c chan int)</c> both
/// emit a <c>channel&lt;nint&gt;</c> parameter, told apart only by the <c>/*&lt;-*/</c> marker comment
/// the converter writes beside the type.
/// </summary>
/// <remarks>
/// <para>
/// Never written by the converter. go2cs-gen's <c>ChanDirSigGenerator</c> reads the marker trivia and
/// emits these on a GENERATED partial of the method's declaring type (the package class for a func or
/// a method, the interface for an interface member), so converted code reads exactly as it did. The
/// method is identified by <see cref="Method"/> and <see cref="ParameterTypes"/> (the C# parameter
/// types, receiver included; a <c>ref</c> parameter is listed by its element type), since Go methods of
/// different receivers share one name in the package class.
/// </para>
/// <para>
/// <see cref="ParameterDirs"/> has one entry per C# parameter and <see cref="ResultDirs"/> one per Go
/// result (a multi-value result's tuple elements, in order); <see cref="GoChanDir.Unstamped"/> marks a
/// position that is not a directional channel. Only the position's OWN channel is described: a
/// direction nested inside it (<c>[]&lt;-chan T</c>, <c>chan&lt;- &lt;-chan T</c>) is not carried.
/// <c>GoReflect.MethodSigChanDirs</c> reads it back and refuses, by name, an entry whose stamped
/// position is not a channel or whose arity disagrees with the method.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class GoSigChanDirAttribute(string method, Type[] parameterTypes, GoChanDir[] parameterDirs, GoChanDir[] resultDirs) : Attribute
{
    /// <summary>The C# method name.</summary>
    public string Method { get; } = method;

    /// <summary>The C# parameter types, receiver included; a <c>ref</c> parameter by its element type.</summary>
    public Type[] ParameterTypes { get; } = parameterTypes;

    /// <summary>One direction per C# parameter; <see cref="GoChanDir.Unstamped"/> where none.</summary>
    public GoChanDir[] ParameterDirs { get; } = parameterDirs;

    /// <summary>One direction per Go result; <see cref="GoChanDir.Unstamped"/> where none.</summary>
    public GoChanDir[] ResultDirs { get; } = resultDirs;
}
