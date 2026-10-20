// GoInterfaceShellAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics.CodeAnalysis;

namespace go;

/// <summary>
/// Stamped by go2cs-gen on every converted NAMED interface that has runtime duck-typing shells,
/// naming the two shell classes emitted alongside it and the interface's method set in the order
/// the object shell forwards them.
/// </summary>
/// <remarks>
/// <para>
/// Go resolves an interface assertion structurally at run time; C# can only convert nominally. For
/// the cases the compile-time recorders cannot see — a dynamic type living in an assembly converted
/// AFTER the interface's own (io/fs is converted before os, so <c>fs/package_info.cs</c> can never
/// record <c>os.dirFS</c>) — the runtime has to BUILD an implementation. Since no dynamic code
/// generation is available, a per-interface compile-time artifact is the irreducible minimum, and it
/// must live in the interface's OWN assembly: that is the only placement guaranteed to be loaded at
/// every asserting site, and the only one that yields a single cross-assembly identity.
/// </para>
/// <para>
/// Discovery is by ATTRIBUTE rather than by static members on the interface itself. Static members
/// on a converted interface are inherited by every interface that embeds it, which produced both a
/// large CS0108 hiding-warning class and a method-set corruption (a Go type can never implement a
/// static helper, so an embedding interface's structural probe demanded one). Carrying the shells
/// here also makes their names non-contractual — the generator may disambiguate them freely.
/// </para>
/// <para>
/// See <see cref="AdapterBinder"/> for the binding rules and the tier selection.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class GoInterfaceShellAttribute : Attribute
{
    // What AdapterBinder reads of each shell by reflection, kept by both trimmers for every interface this attribute is
    // kept on: a typeof argument to an attribute parameter carrying [DynamicallyAccessedMembers] keeps those members. A
    // full-trim Native AOT publish otherwise dropped the object shell's constructor, and every duck-typed assertion over a
    // value (fmt's Stringer against reflect.Kind, 2026-10-12) missed. The generic shell's bound-flags are static fields.
    private const DynamicallyAccessedMemberTypes ObjectShellMembers = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors;

    private const DynamicallyAccessedMemberTypes GenericShellMembers = ObjectShellMembers | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields;

    /// <summary>
    /// Creates a new <see cref="GoInterfaceShellAttribute"/>.
    /// </summary>
    /// <param name="genericShell">
    /// Open generic delegate-bound shell, <c>Δ&lt;Iface&gt;&lt;&gt;</c>, closed over the Go ELEMENT type
    /// (the pointee of a receiver box, or the value's own type). Never <c>null</c>.
    /// </param>
    /// <param name="objectShell">
    /// Non-generic reflective shell, or <c>null</c> when the interface has a member the reflective
    /// invoker cannot forward (a by-ref or ref-struct parameter — a Go variadic tail lowers to
    /// <c>Span&lt;T&gt;</c>, which cannot be boxed — or a generic method).
    /// </param>
    /// <param name="methodNames">
    /// The interface's transitive Go method set, in the order the object shell's forwarders index it.
    /// Bare (unescaped) names, matching <see cref="System.Reflection.MemberInfo.Name"/>.
    /// </param>
    public GoInterfaceShellAttribute([DynamicallyAccessedMembers(GenericShellMembers)] Type genericShell, [DynamicallyAccessedMembers(ObjectShellMembers)] Type? objectShell, params string[] methodNames)
    {
        GenericShell = genericShell;
        ObjectShell = objectShell;
        MethodNames = methodNames;
    }

    /// <summary>
    /// Gets the open generic delegate-bound shell type.
    /// </summary>
    [DynamicallyAccessedMembers(GenericShellMembers)]
    public Type GenericShell { get; }

    /// <summary>
    /// Gets the non-generic reflective shell type, if one was emitted.
    /// </summary>
    [DynamicallyAccessedMembers(ObjectShellMembers)]
    public Type? ObjectShell { get; }

    /// <summary>
    /// Gets the interface's transitive method names, in object-shell forwarding order.
    /// </summary>
    public string[] MethodNames { get; }
}
