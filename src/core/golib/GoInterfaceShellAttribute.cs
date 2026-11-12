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
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoInterfaceShellAttribute : Attribute
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
    /// invoker cannot forward and no bound delegate can carry either (a by-ref parameter, a generic
    /// method, or a variadic tail no delegate family of golib's declares). A Go variadic tail lowers to
    /// <c>Span&lt;T&gt;</c>, which cannot be boxed; the object shell forwards such a member through a
    /// bound delegate instead (see <see cref="VariadicMethods"/>).
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

    /// <summary>
    /// Gets or sets the names, among <see cref="MethodNames"/>, of the members the object shell forwards through a
    /// BOUND DELEGATE rather than a reflective invoker: those with a Go variadic tail (<c>params Span&lt;T&gt;</c>),
    /// which no invoker can box. <c>null</c> when the interface has none.
    /// </summary>
    /// <remarks>
    /// Without them an interface with any variadic method had no object shell, and a value-typed dynamic value could
    /// bind only through the generic shell, closed over the value's type by <c>MakeGenericType</c>: under Native AOT
    /// that has no native code for a struct, so every such assertion read false where Go and the JIT read true
    /// (DATA-aot-variadic-shell.md). The delegate comes from the type's generated method set (<c>IGoMethodSet</c>),
    /// compiled code, so no run-time instantiation is needed.
    /// </remarks>
    public string[]? VariadicMethods { get; set; }

    /// <summary>
    /// Gets or sets the delegate type the object shell casts each of <see cref="VariadicMethods"/> to, in the same
    /// order: the type golib's reflect builds for the member's signature without its receiver, which is the type a
    /// generated method-set arm's bound delegate has. The binder takes a delegate only when its type is exactly this.
    /// </summary>
    public Type[]? VariadicDelegates { get; set; }
}
