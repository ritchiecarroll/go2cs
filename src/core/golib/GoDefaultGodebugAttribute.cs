// GoDefaultGodebugAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;

namespace go;

/// <summary>
/// The program's default GODEBUG settings: the converter's reproduction of what <c>cmd/go</c> bakes into a
/// Go binary as <c>runtime.godebugDefault</c> (<c>-X=runtime.godebugDefault=...</c>) from the main module's
/// <c>go</c> line, its go.mod <c>godebug</c> block and the package's <c>//go:debug</c> directives.
/// </summary>
/// <remarks>
/// <para>
/// Applied to the ENTRY assembly -- a converted program or a converted test host -- and only when the
/// default is non-empty, so every program whose module is at the corpus release emits nothing. Read once at
/// start-up: the runtime copies it into <c>godebugDefault</c> before <c>parsedebugvars</c>, and
/// <c>internal/godebug</c> layers it UNDER the GODEBUG environment variable, exactly as Go does.
/// </para>
/// <para>
/// An attribute rather than an environment variable because, like Go's link-time value, it belongs to the
/// binary: a test that starts a child process must not hand that child its own defaults.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GoDefaultGodebugAttribute(string value) : Attribute
{
    /// <summary>The comma-separated <c>name=value</c> settings, sorted by name, as cmd/go writes them.</summary>
    public string Value { get; } = value;

    private static string? s_entryValue;

    /// <summary>
    /// The entry assembly's default GODEBUG, or "" when it declares none (or there is no entry assembly).
    /// </summary>
    public static string EntryValue => s_entryValue ??= Assembly.GetEntryAssembly()?.GetCustomAttribute<GoDefaultGodebugAttribute>()?.Value ?? "";
}
