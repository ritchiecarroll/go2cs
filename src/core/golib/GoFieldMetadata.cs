// GoFieldMetadata.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// The instance fields of a Go struct surrogate, for the golib facts that are DERIVED from them and
/// have no safe answer without them.
/// </summary>
/// <remarks>
/// <para>
/// Reflection over a type whose field metadata was removed by trimming does not fail: it answers "no
/// fields". Three facts turn that answer into silent misbehaviour — <see cref="GoZeroSizeFacts"/>
/// classifies every struct as zero-size, <see cref="GoLibcCall.DispatchArgsStruct"/> calls libc with
/// no arguments, and the slice-header boxes decline a reinterpretation they exist to serve, which
/// sends a header down the address route.
/// </para>
/// <para>
/// A struct that really declares no instance field occupies exactly one byte. A struct that occupies
/// more and reports none has lost its metadata, and that is refused here by name rather than
/// answered.
/// </para>
/// <para>
/// This is a tripwire, not the protection. The trimmer never removes an instance field from a value
/// type it keeps (measured 2026-10-02 across golib, reflect, runtime and fmt published in full trim
/// mode), so today's linker does not produce this case; what keeps a published program correct is the
/// project template's <c>TrimMode=partial</c>, which keeps golib and every converted assembly whole.
/// </para>
/// </remarks>
internal static class GoFieldMetadata
{
    private const BindingFlags InstanceFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// TEST SEAM: a type whose fields are reported as absent, the way a trimmed type's are. Null in
    /// every program; GolibTests sets it for the length of one arm (the assembly runs serially).
    /// </summary>
    internal static Type? WithheldForTest;

    /// <summary>
    /// TEST SEAM: a type whose fields are reported in REVERSE, the way a runtime that reordered GetFields would
    /// report them. Null in every program; GolibTests sets it for the length of one arm.
    /// </summary>
    internal static Type? ReorderedForTest;

    /// <summary>
    /// The instance fields of <paramref name="type"/>, public and non-public, in DECLARATION order: GetFields' order.
    /// </summary>
    /// <remarks>
    /// The sites that need declaration order (the slice-header boxes, GoLibcCall's arguments and result block) sorted
    /// by FieldInfo.MetadataToken until Native AOT, which gives a member no token. GetFields' order is not documented,
    /// so it is measured instead: declaration order under the JIT and under Native AOT on every struct shape read
    /// (2026-10-06), and equal to token order for every value type of every converted assembly GolibTests loads
    /// (FieldOrderGuardTests, under the JIT). A runtime that reorders GetFields fails that guard, not a program.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="type"/> is a value type that occupies storage and reports no instance field.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2070", Justification = GoTypeRegistry.RegisteredTypeJustification)]
    internal static FieldInfo[] InstanceFields(Type type)
    {
        FieldInfo[] fields = type == WithheldForTest ? [] : type.GetFields(InstanceFieldFlags);

        if (type == ReorderedForTest)
            Array.Reverse(fields);

        if (fields.Length == 0 && type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.ContainsGenericParameters)
        {
            int size = RuntimeHelpers.SizeOf(type.TypeHandle);

            if (size > 1)
            {
                throw new InvalidOperationException(
                    $"go2cs: {type.FullName} occupies {size} bytes and reports no instance fields — its field metadata was removed, " +
                    "most likely by trimming. golib derives a Go struct's size, layout and call arguments from its fields: " +
                    "publish with golib and the converted assemblies kept whole (TrimMode=partial, or a trimmer root for each).");
            }
        }

        return fields;
    }
}
