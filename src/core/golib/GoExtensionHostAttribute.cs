// GoExtensionHostAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;

namespace go;

/// <summary>
/// Marks an assembly whose <see cref="GoExtensionHostAttribute"/> list is complete: every top-level class in it that
/// declares an extension method is listed.
/// </summary>
/// <remarks>
/// <para>
/// go2cs-gen writes both attributes at build time, and golib's extension-method scan reads them only where an
/// assembly has no raw metadata to walk (Native AOT), in place of <see cref="System.Reflection.Assembly.GetTypes"/>.
/// An assembly without this marker, such as one built by an older generator, is scanned with <c>GetTypes()</c>.
/// </para>
/// <para>
/// See docs/phase4/DESIGN-extension-host-list.md and docs/phase4/DESIGN-extension-receiver-scope.md.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class GoExtensionHostListAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether every listed class names the other assemblies whose types its extension methods extend
    /// (<see cref="GoExtensionHostAttribute.Homes"/>), so that a listed class naming none extends only this assembly's
    /// types. Without it, as in an assembly from an older generator, the scan reads every listed class for every
    /// question.
    /// </summary>
    public bool HomesListed { get; set; }
}

/// <summary>
/// Lists one class of this assembly that declares extension methods, for golib's extension-method scan.
/// </summary>
/// <remarks>
/// A class may be listed more than once; the scan reads each class once. See
/// <see cref="GoExtensionHostListAttribute"/>.
/// </remarks>
/// <param name="host">The listed class.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GoExtensionHostAttribute([DynamicallyAccessedMembers(GoExtensionHostAttribute.HostMembers)] Type host)
    : Attribute
{
    // What the scan reads from a host: its static methods, public and not. The annotation keeps them through a trim.
    internal const DynamicallyAccessedMemberTypes HostMembers =
        DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods;

    /// <summary>
    /// Gets the listed class.
    /// </summary>
    [DynamicallyAccessedMembers(HostMembers)]
    public Type Host => host;

    /// <summary>
    /// Gets or sets one type from each assembly OTHER than this one whose types the class's extension methods take
    /// as their receiver (a <c>-tests</c> host's methods on its package's types). Empty for a class whose methods all
    /// extend this assembly's types.
    /// </summary>
    public Type[] Homes { get; set; } = [];

    /// <summary>
    /// Gets or sets whether one of the class's extension methods takes a receiver no assembly holds, such as a type
    /// parameter or an array, so that the scan reads the class for every question.
    /// </summary>
    public bool AnyHome { get; set; }
}
