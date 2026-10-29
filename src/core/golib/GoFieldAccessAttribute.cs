// GoFieldAccessAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Lists the struct types whose fields this package reaches through a pointer, so go2cs-gen emits a field property
/// on the pointer for each of their fields and the site reads as Go spells it, <c>p.f</c>.
/// </summary>
/// <param name="types">The struct types; a generic struct is named by its open definition.</param>
/// <remarks>
/// <para>
/// The converter writes the record in the package's <c>package_info.cs</c>, one assembly-level attribute per type
/// (parity seat #31). go2cs-gen unions the records of a compilation: its assembly's own, and the class-level records an
/// earlier converter wrote on the package class of each file. A test assembly that recompiles the production files
/// carries their records too, since its conversion re-converts them; one that references the production assembly
/// sees that assembly's generated properties through <c>InternalsVisibleTo</c>.
/// </para>
/// <para>
/// A site is a field access <c>p.f</c> through a pointer to a named struct. It binds a generated C# 14 extension
/// property, a <c>ref TF</c>: <c>ref p.Value.f</c> on a managed box, and the field at its Go offset on a
/// <see cref="NativeBox{T}"/> (<see cref="ж{T}.field{TF}"/>'s two arms), where a struct holding a managed reference
/// has a CLR layout that is not Go's.
/// </para>
/// <para>
/// A field whose name is also a member of <c>ж&lt;T&gt;</c> gets no property (an instance member binds before an
/// extension one, so the property would silently lose to it); the converter keeps a box form for it. The name list is
/// the converter's <c>fieldAccessCollisions.txt</c>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GoFieldAccessAttribute(params Type[] types) : Attribute
{
    /// <summary>
    /// Gets the struct types.
    /// </summary>
    public Type[] Types => types;
}
