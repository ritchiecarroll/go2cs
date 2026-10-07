// GoStrAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks the member of an sstring TWIN that carries the Go body (docs/phase4/DESIGN-sstring-twin-pilot.md):
/// each twinned parameter is typed <c>sstring</c>. go2cs-gen's StrGenerator emits its companions:
/// the <c>@string</c> member that forwards to it under <c>[OverloadResolutionPriority(-1)]</c>, and for a
/// package-level function the canonical value delegate <c>&lt;Name&gt;ᶠ</c>.
/// </summary>
/// <remarks>
/// <para>
/// Converted code does not write it (docs/PLAN-marker-comment-parity.md, 5.7): the converter types each
/// twinned parameter <c>sstring</c> and nothing else in converted code takes one, so StrGenerator selects
/// the member by that parameter. A hand-written file may keep the attribute, and in a hand-owned package
/// (<see cref="GoHandOwnedPackageAttribute"/>) a member is a twin by this attribute alone.
/// </para>
/// <para>
/// Every call with an <c>@string</c>, a C# string or a u8 literal binds the member, and every func value
/// names the delegate. The package's <c>package_info.cs</c> separately PUBLISHES each exported
/// package-level twin to other packages as <see cref="GoSStringTwinAttribute"/>, for the converter.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GoStrAttribute : Attribute;
