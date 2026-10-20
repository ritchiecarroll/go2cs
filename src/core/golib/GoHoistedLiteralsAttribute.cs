// GoHoistedLiteralsAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks an assembly whose converted string literals are hoisted to <c>static readonly</c> fields: the standard
/// library's form, which <c>-hoist-literals</c> gives any other package.
/// </summary>
/// <remarks>
/// <para>
/// The converter writes <c>[assembly: GoHoistedLiterals]</c> into the package info file of every package whose
/// literals it hoists (package_info.cs, and package_test_info.cs for its test assembly). Such a package passes no
/// bare C# literal to an <c>any</c> slot, so go2cs-gen's LiteralInterceptorGenerator reads this stamp and runs no
/// semantic transform for the assembly, which is what keeps design time free for it.
/// </para>
/// <para>
/// The stamp only skips work: it never changes what a call binds. If a stamped assembly did pass a bare literal to
/// an <c>object</c> slot, StringLiteralBindingAnalyzer would still report it as GO2CS0004, an error. The attribute
/// is single-use, so a test assembly that compiled two stamped info files would fail the build (CS0579) instead
/// of carrying a duplicate.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GoHoistedLiteralsAttribute : Attribute;
