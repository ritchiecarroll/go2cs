// LinknameVar.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace go;

/// <summary>
/// The late-bound side of a <c>//go:linkname</c> var PULL that cannot be compiled as a direct reference.
/// </summary>
/// <remarks>
/// <para>
/// <b>What Go does.</b> <c>//go:linkname overflowError runtime.overflowError</c> on a package var makes it
/// the SAME variable as the remote one: one storage, so a read anywhere sees runtime's value.
/// </para>
/// <para>
/// <b>Why by name.</b> The converter emits such a pull as a forwarding property to the remote static,
/// which needs a ProjectReference to the remote's package. When the remote's package already imports the
/// puller, that reference closes a project cycle (MSB4006). At go1.26.9 runtime imports math/bits, so
/// math/bits's pulls of runtime.overflowError and runtime.divideError are exactly that shape. The
/// converter then emits a property that reaches the remote field through this class instead: the same
/// storage, no compile-time reference. Reading the static through reflection runs the remote class's
/// static initializer first, as the direct property did, so the value is the remote's own object.
/// </para>
/// <para>
/// <b>Never a silent null.</b> A pull this class cannot resolve (the remote's assembly is not part of the
/// program, or the field is gone) panics naming the target. A null here would be the very bug the
/// late-bound pull replaces: a plain field that is never the remote's value.
/// </para>
/// <para>
/// <b>Trimming and Native AOT.</b> Every emitting site carries one
/// <see cref="DynamicDependencyAttribute"/> naming the remote type's public fields in its assembly, so the
/// trimmer and ILC keep the field and its reflection metadata. The lookups below are therefore known-safe,
/// and their analysis warnings are suppressed with that justification.
/// </para>
/// </remarks>
/// <typeparam name="T">The var's type.</typeparam>
public static class LinknameVar<T>
{
    private static readonly ConcurrentDictionary<(string Type, string Field), FieldInfo> s_fields = new();

    /// <summary>Reads the remote var <paramref name="field"/> of <paramref name="assemblyQualifiedType"/>.</summary>
    public static T Get(string assemblyQualifiedType, string field) =>
        (T)resolve(assemblyQualifiedType, field).GetValue(null)!;

    /// <summary>Writes the remote var <paramref name="field"/> of <paramref name="assemblyQualifiedType"/>.</summary>
    public static void Set(string assemblyQualifiedType, string field, T value) =>
        resolve(assemblyQualifiedType, field).SetValue(null, value);

    // Resolved once per target and cached; every later access is one dictionary hit, never a type search.
    private static FieldInfo resolve(string assemblyQualifiedType, string field) =>
        s_fields.TryGetValue((assemblyQualifiedType, field), out FieldInfo? cached) ? cached :
            s_fields.GetOrAdd((assemblyQualifiedType, field), lookup);

    [UnconditionalSuppressMessage("Trimming", "IL2057",
        Justification = "Every emitting site carries [DynamicDependency(PublicFields, <type>, <assembly>)] naming this type, so it is kept.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "Every emitting site carries [DynamicDependency(PublicFields, <type>, <assembly>)], which keeps the public field read here.")]
    private static FieldInfo lookup((string Type, string Field) target)
    {
        Type? type = Type.GetType(target.Type, throwOnError: false);
        FieldInfo? info = type?.GetField(target.Field, BindingFlags.Public | BindingFlags.Static);

        if (info is null || !typeof(T).IsAssignableFrom(info.FieldType))
        {
            throw builtin.panic(type is null
                ? $"go2cs: //go:linkname pull target {target.Type} is not part of this program, so {target.Field} cannot be read"
                : $"go2cs: //go:linkname pull target {target.Type}.{target.Field} is missing or is not a public static {typeof(T).Name}");
        }

        return info;
    }
}
