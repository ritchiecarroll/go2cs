// GoTakesCSharpStringAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a golib parameter that takes a C# <see cref="string"/> ON PURPOSE, where converted code may bind a
/// bare C# string literal to it.
/// </summary>
/// <remarks>
/// <para>
/// A converted user package emits Go's <c>"hello"</c> in a <c>string</c> slot as the bare C# literal, which
/// reaches <see cref="@string"/> through its implicit conversion (plain string literals, owner ruling
/// 2026-10-17). A bare C# string also binds an <c>object</c> parameter and a <c>string</c> overload without a
/// compile error, and either one would hand the callee a System.String where Go has a string, so go2cs-gen's
/// StringLiteralBindingAnalyzer reports both as GO2CS0004 in converted code.
/// </para>
/// <para>
/// This attribute is the one exemption, declared at the parameter that is correct to receive a C# string:
/// <see cref="builtin.panic(object?)"/> normalizes a System.String to @string at its boundary, and
/// <see cref="builtin.len(string)"/> counts its UTF-8 bytes as Go's <c>len</c> does. A parameter is marked
/// only when the conversion it receives is that deliberate; the zero-on-corpus build with the analyzer on is
/// the proof that the marked set is complete.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class GoTakesCSharpStringAttribute : Attribute;
