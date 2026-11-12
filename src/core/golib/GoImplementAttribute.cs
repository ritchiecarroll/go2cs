// GoImplementAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Records that a GENERIC type, at every instantiation, implements an interface: the open-generic form of
/// <see cref="GoImplementAttribute{TStruct, TInterface}"/>. The converter writes it for a conversion made at the
/// type's own type parameters (<c>return &amp;sliceIter[T](values)</c> inside <c>NewIter[T]</c>), which an
/// attribute's type argument cannot name, as <c>[assembly: GoImplement(typeof(sliceIter&lt;&gt;), typeof(Iter),
/// Pointer = true)]</c>. go2cs-gen emits the same generic adapter (or value partial) it emits for closed records of
/// the pair, and one adapter serves both.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoImplementAttribute : Attribute
{
    /// <summary>
    /// Creates a record that the generic type definition <paramref name="structDefinition"/> implements
    /// <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="structDefinition">The unbound generic type definition, such as <c>typeof(sliceIter&lt;&gt;)</c>.</param>
    /// <param name="interfaceType">The implemented interface, which has no type parameters.</param>
    public GoImplementAttribute(Type structDefinition, Type interfaceType)
    {
        StructDefinition = structDefinition;
        InterfaceType = interfaceType;
    }

    /// <summary>
    /// Gets the unbound generic type definition the record names.
    /// </summary>
    public Type StructDefinition { get; }

    /// <summary>
    /// Gets the implemented interface.
    /// </summary>
    public Type InterfaceType { get; }

    /// <summary>
    /// Gets or sets flag indicating the Go interface cast source was a POINTER to the type, as on
    /// <see cref="GoImplementAttribute{TStruct, TInterface}.Pointer"/>.
    /// </summary>
    public bool Pointer { get; set; }
}

/// <summary>
/// Marks an assembly with a structure interface implementation mapping.
/// </summary>
/// <typeparam name="TStruct">Struct type that implements interface.</typeparam>
/// <typeparam name="TInterface">Interface type to implement.</typeparam>
/// <remarks>
/// <para>
/// This attribute is used to auto-generate backend C# code needed to implement
/// an interface for a structure using matching receiver methods.
/// </para>
/// <para>
/// See the <c>ImplementGenerator</c> in the go2cs code generators for details.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoImplementAttribute<TStruct, TInterface> : Attribute
{
    /// <summary>
    /// Gets or sets flag that determines if interface is a promoted
    /// field in the structure.
    /// </summary>
    public bool Promoted { get; set; }

    /// <summary>
    /// Gets or sets flag indicating the Go interface cast source was a POINTER to the struct
    /// (<c>var s Iface = &amp;t</c>). The generator then emits an <see cref="IжAdapter"/> wrapper
    /// class holding the receiver box <c>ж&lt;TStruct&gt;</c> — the interface value aliases the
    /// original box exactly as Go's interface holds the <c>*T</c> — instead of the value-boxing
    /// partial-struct implementation (which copies, and cannot bind direct-ж receiver methods).
    /// </summary>
    public bool Pointer { get; set; }

    /// <summary>
    /// Gets or sets flag indicating this record was emitted for the PRODUCTION half of a
    /// recompile-model test assembly — the pass separation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Under the recompile test model the production <c>.cs</c> files are compile items of the TEST
    /// assembly, so the generator reads the UNION of both halves' records. But the production text
    /// was rendered in the production pass, against the production record set alone, and cannot be
    /// re-rendered — so a test-half record that lands in a production record's adapter-name
    /// collision group must not be allowed to rename the production member. <c>crypto/sha3</c> is
    /// the corpus instance: production records <c>&lt;SHA3, hash.Hash&gt;</c>, the external test
    /// half adds <c>&lt;SHA3, fips140.Hash&gt;</c>, both compose <c>SHA3жHash</c>, and the ordinary
    /// collision rule prefixes BOTH — leaving <c>sha3.cs</c>'s four already-written
    /// <c>SHA3жHash</c> sites naming a class that is never emitted.
    /// </para>
    /// <para>
    /// The converter stamps this facet onto the production records as it seeds them into the test
    /// metadata file; records the test pass merges in carry no facet. Collisions are still computed
    /// over the union — the facet decides NAMING, never grouping. In a colliding group with exactly
    /// ONE faceted member, that member keeps the unprefixed name and only the unfaceted members take
    /// the interface prefix. Two or more faceted members are a collision the production pass already
    /// resolved, so the ordinary rule applies to the whole group and reproduces exactly the names the
    /// production text spells. No faceted member is the ordinary rule unchanged — which is every
    /// production compilation and both reference test models.
    /// </para>
    /// <para>
    /// Keep in sync with the converter's <c>adapterNameCollisions.go</c>, which resolves the matching
    /// cast-site references from these same records.
    /// </para>
    /// </remarks>
    public bool Production { get; set; }

    /// <summary>
    /// Gets or sets flag indicating this records a SELF-REFERENTIAL constraint proxy: the Go
    /// element type <c>TStruct</c> (a pointer type <c>*P</c>) satisfies a generic method-set
    /// constraint interface <c>TInterface</c> (<c>nistPoint[Point]</c>) only structurally, so it
    /// is used as a generic type's type argument (<c>nistCurve[*P224Point]</c>). Because the golib
    /// box <c>ж&lt;TStruct&gt;</c> cannot nominally implement a package interface, the generator
    /// emits a proxy class <c>TStructжTInterface : TInterface&lt;itself&gt;</c> that wraps the box
    /// and carries implicit <c>ж&lt;TStruct&gt;</c>↔proxy conversions, so every self-typed
    /// (<c>T</c>) parameter and result marshals across the boundary automatically. The proxy is
    /// what gets substituted for the type argument at the constrained instantiation. The
    /// <c>TInterface</c> type argument's own type argument is a placeholder — the generator uses
    /// its open definition and closes it over the proxy itself.
    /// </summary>
    public bool ConstraintProxy { get; set; }
}

/// <summary>
/// Marks a generated wrapper whose Go dynamic value is something OTHER than the wrapper itself.
/// </summary>
/// <remarks>
/// The two adapter kinds below answer different unwrap questions, but every consumer asks the same
/// one first — "is this object standing in for another value?" — and for ordinary Go values the
/// answer is no. This base lets that be settled with ONE type test instead of one per kind, which
/// matters because a FAILING interface type test costs ~3 ns (the runtime walks the type's interface
/// map) where a failing class test is free. A type switch and a type assert each pay it per
/// evaluation: probing both kinds separately was ~11.6 ns per iteration of the PerfIface benchmark.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IGoAdapter;

/// <summary>
/// Implemented by generated interface-implementation adapters that wrap the receiver box
/// <c>ж&lt;T&gt;</c> of a POINTER-sourced Go interface value.
/// </summary>
/// <remarks>
/// A type assert back to the Go pointer type (<c>s.(*T)</c>) targets the box itself, so the
/// assert machinery unwraps the adapter through <see cref="Box"/>. Adapter equality also
/// delegates to box identity, matching Go pointer-interface equality.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IжAdapter : IGoAdapter
{
    /// <summary>
    /// Gets the wrapped receiver box (a <c>ж&lt;T&gt;</c>).
    /// </summary>
    object? Box { get; }
}

/// <summary>
/// Implemented by generated interface-to-interface adapters that wrap another Go interface value.
/// </summary>
/// <remarks>
/// Go interface-to-interface assignment preserves the original dynamic value. The runtime unwraps
/// these adapters through <see cref="Value"/> for type assertions and interface equality.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IInterfaceAdapter : IGoAdapter
{
    /// <summary>
    /// Gets the wrapped source interface value.
    /// </summary>
    object? Value { get; }
}

/// <summary>
/// Implemented by generated VALUE-sourced interface-implementation adapters that wrap a COPY of a
/// struct (or a named-func delegate) which this assembly cannot make <c>partial</c> — because the
/// struct is declared in ANOTHER assembly, or is a delegate.
/// </summary>
/// <remarks>
/// The interface value's Go DYNAMIC TYPE is the wrapped struct, never the adapter class, so Go
/// <c>==</c>, a type assert back to the struct type, a type switch and <c>%T</c> all unwrap through
/// <see cref="Value"/> — exactly as the other two kinds do through their own accessor.
/// (image's <c>func (p *NRGBA) At(x, y int) color.Color</c> hands back one of these: the
/// conversion sits in <c>image</c>, where both <c>color.NRGBA</c> and <c>color.Color</c> are
/// foreign.) Without the marker every one of those questions answered against the wrapper class and
/// silently diverged; the value-boxing partial-struct implementation used for a LOCAL struct has no
/// wrapper and never had the problem, which is why only the cross-assembly shape reproduces it.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IValueAdapter : IGoAdapter
{
    /// <summary>
    /// Gets the wrapped value copy — the Go dynamic value this adapter stands in for.
    /// </summary>
    object? Value { get; }
}
