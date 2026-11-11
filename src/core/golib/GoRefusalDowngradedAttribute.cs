// GoRefusalDowngradedAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Records one converter refusal that was downgraded to a warning with <c>-warn</c>: the package converted, and
/// the named site fails loud, a panic carrying the refusal text, when it is reached.
/// </summary>
/// <remarks>
/// <para>
/// The converter writes one <c>[assembly: GoRefusalDowngraded(code, site, text)]</c> into package_info.cs per
/// downgraded site. go2cs-gen's RefusalDowngradedAnalyzer reports each as GO2CS0011, a warning, in every C# build
/// of the assembly, so a project that is never converted again still shows that it carries a known-broken path.
/// </para>
/// <para>
/// Only a refusal whose downgraded site cannot run silently wrong may be downgraded (the converter's allow-list,
/// warnOption.go); today that is GO2CS2002, whose site is a constraint-proxy forwarder that panics when called.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GoRefusalDowngradedAttribute(string code, string site, string text) : Attribute
{
    /// <summary>The converter's refusal code, for example <c>GO2CS2002</c>.</summary>
    public string Code { get; } = code;

    /// <summary>The refused site, as the Go source file's base name and line, for example <c>main.go:33</c>.</summary>
    public string Site { get; } = site;

    /// <summary>The refusal's text, as the converter reported it.</summary>
    public string Text { get; } = text;
}
