// IGoZeroConstructed.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// Marks a converted Go struct whose Go ZERO VALUE must be built: its constructor runs what <c>default(T)</c> skips (a
/// fixed-size array field's <c>= new(N)</c>, a promoted embed's box, a value field whose own type needs building, a
/// directional channel's stamp), and <see cref="GoZeroNew"/> runs it.
/// </summary>
/// <remarks>
/// <para>
/// go2cs-gen writes it on the generated part of exactly those structs (StructTypeTemplate), generic or not. golib reads
/// it where it needs a zero value it cannot take from <c>default</c>: <see cref="builtin.GoZero{T}(T)"/>, a container
/// filling a zeroed window, <c>clear</c>, and <see cref="builtin.GoZero{T}()"/> for a type no factory registers.
/// </para>
/// <para>
/// Trim stage 3b (docs/PLAN-golib-full-trim.md, section 9.6): golib asked the type for its parameterless constructor
/// and ran it with <c>Activator</c>, which a trimmer cannot follow from a type parameter, and which Native AOT can run
/// only for a constructor whose metadata it kept. The member is compiled for every struct the program has. It also
/// closes the residual the GoZero ruling stated: a GENERIC struct has no closed type a module initializer could register
/// a factory for, but its generated part names its own closed instantiation here. GolibTests' GoZeroConstructionGuardTests
/// checks, over every converted assembly the host loads, that every struct whose constructor builds something carries it.
/// </para>
/// </remarks>
public interface IGoZeroConstructed
{
    /// <summary>A new Go zero value of this struct, built by its parameterless constructor.</summary>
    object GoZeroNew();
}
