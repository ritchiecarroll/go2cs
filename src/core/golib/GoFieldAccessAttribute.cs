// GoFieldAccessAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Lists the struct types whose fields this package reaches through a pointer it reinterpreted from
/// <c>unsafe.Pointer</c>, so go2cs-gen emits a field property on the pointer for each of their fields.
/// </summary>
/// <param name="types">The struct types; a generic struct is named by its open definition.</param>
/// <remarks>
/// <para>
/// The converter writes the record on the package class of each FILE that has a scoped site, naming the struct types
/// that file's sites touch; go2cs-gen unions the records of a compilation. The record is per file, not per package,
/// because a test assembly recompiles the production files beside its own, so the record travels with its sites. A
/// site is a field access <c>p.f</c> through a local <c>p</c> whose every assignment is
/// <c>(*T)(unsafe.Pointer(e))</c> with <c>T</c> holding a managed reference: such a pointer may alias native memory,
/// where the struct's CLR layout is not Go's. The site reads as Go does, <c>buf.SubstituteNameOffset = x</c>, through
/// a generated C# 14 extension property, <c>ref TF</c> over <see cref="ж{T}.field{TF}"/>, which reaches the field at
/// its Go offset in native memory and at its own slot in managed memory, with no box in between.
/// </para>
/// <para>
/// A field whose name is also a member of <c>ж&lt;T&gt;</c> gets no property (an instance member binds before an
/// extension one, so the property would silently lose to it); the converter emits the explicit
/// <c>p.of(T.Ꮡf).Value</c> form for it. The name list is the converter's <c>fieldAccessCollisions.txt</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GoFieldAccessAttribute(params Type[] types) : Attribute
{
    /// <summary>
    /// Gets the struct types.
    /// </summary>
    public Type[] Types => types;
}
