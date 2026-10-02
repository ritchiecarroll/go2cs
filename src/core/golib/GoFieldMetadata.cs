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
    /// The instance fields of <paramref name="type"/>, public and non-public, in the runtime's order.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="type"/> is a value type that occupies storage and reports no instance field.
    /// </exception>
    internal static FieldInfo[] InstanceFields(Type type)
    {
        FieldInfo[] fields = type == WithheldForTest ? [] : type.GetFields(InstanceFieldFlags);

        return fields;
    }
}
