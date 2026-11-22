// GoTwinOfAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;

namespace go.golib;

/// <summary>
/// Stamps an M:N twin, or an async literal inside one, with the function it was generated from and the line of each of
/// its awaits, so the runtime names and places a parked twin's frame with no lookup by name and no scan of IL.
/// </summary>
/// <remarks>
/// <para>
/// go2cs-gen's TwinGenerator writes it; it is never written by hand. The runtime finds a twin's kickoff method from its
/// state machine by identity (the method whose AsyncStateMachineAttribute names that machine), reads this stamp, and
/// resolves the origin as the one method of <see cref="Host"/> named <see cref="Name"/> whose parameters are
/// <see cref="Parameters"/>: each by-ref type stripped (an attribute cannot spell one), a generic type as its open
/// definition, and null where the parameter is a type parameter of the method. A parked frame's line is
/// <see cref="AwaitSites"/>[state], the converted file's line of the call the machine awaits at that state: the compiler
/// numbers a method's awaits in source order, which GenTests' TwinStampTests holds every stamp to. Where that call
/// sits on a later line of a multi-line statement, <see cref="CallOrdinals"/>[state] names it among the statement's
/// calls of the same name, as the position map's per-call table records Go's call lines, so the frame reports the call's
/// own line as Go does.
/// </para>
/// <para>
/// FROZEN SURFACE (docs/phase4/DATA-mn-origin-table.md; PublicSurface.golib.txt [mn-frozen]): its constructor and usage
/// are named by generated code in every converted assembly, so they hold across a break floor.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoTwinOfAttribute(Type host, string name, Type?[] parameters, int[] awaitSites, int[] callOrdinals) : Attribute
{
    /// <summary>Gets the type that declares the function the twin was generated from.</summary>
    public Type Host { get; } = host;

    /// <summary>Gets the name of the function the twin was generated from.</summary>
    public string Name { get; } = name;

    /// <summary>Gets that function's parameter types: by-ref stripped, generic types open, null for a method type parameter.</summary>
    public Type?[] Parameters { get; } = parameters;

    /// <summary>Gets the converted file's line of each awaited call, in source order (the compiler's state numbering).</summary>
    public int[] AwaitSites { get; } = awaitSites;

    /// <summary>
    /// Gets, for each await, the awaited call's place among the calls of its name in its statement, in evaluation order:
    /// <c>(ordinal &lt;&lt; 16) | total</c>, the ordinal from 1. The runtime reads the call's own Go line from the position
    /// map's per-call table with it.
    /// </summary>
    public int[] CallOrdinals { get; } = callOrdinals;
}
