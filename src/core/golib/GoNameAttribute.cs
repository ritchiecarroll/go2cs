// GoNameAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;

namespace go;

/// <summary>
/// Carries the Go name of a declaration whose Go name starts with <c>Δ</c>, the converter's
/// collision-rename marker. Golib projects an emitted name to its Go name by removing one leading
/// <c>Δ</c>, which is right for a name the converter renamed (<c>ΔMain</c> is Go's <c>Main</c>) and
/// wrong for a name the Go source itself spells with a leading <c>Δ</c> (<c>Δt</c> is Go's
/// <c>Δt</c>). The two emit the same way, so the converter stamps this attribute where the Go name
/// starts with <c>Δ</c>: on fields, methods, interface methods, named types and package functions.
/// Every projection reads the stamp first and removes the marker only from an unstamped name, so an
/// assembly converted before the stamp existed keeps the projection it had.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Struct |
                AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoNameAttribute(string name) : Attribute
{
    /// <summary>The Go name of the declaration.</summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the Go name of a converted member from its emitted name: the <see cref="GoNameAttribute"/>
    /// stamp when the member carries one; otherwise the emitted name with one leading <c>Δ</c> removed.
    /// </summary>
    /// <param name="member">Converted field, method or type.</param>
    /// <param name="emittedName">The member's emitted name; defaults to <see cref="MemberInfo.Name"/>.</param>
    /// <returns>The Go name.</returns>
    public static string GoNameOf(MemberInfo member, string? emittedName = null)
    {
        string name = emittedName ?? member.Name;

        // A stamp is only ever emitted on a Δ-led name, so an unmarked name skips the attribute read.
        if (!name.StartsWith('Δ'))
            return name;

        if (member.GetCustomAttribute<GoNameAttribute>(inherit: false) is { } stamp)
            return stamp.Name;

        return name.Length > 1 ? name[1..] : name;
    }
}
