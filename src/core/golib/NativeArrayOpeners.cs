// NativeArrayOpeners.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;

namespace go;

/// <summary>
/// The doors a pointer-to-array over native memory is built through when golib knows its element only as a
/// <see cref="Type"/>, registered per element by the compilation that spells it.
/// </summary>
/// <remarks>
/// <para>
/// WHY (G's ruling, 2026-10-19: NARROWER). Two native array kinds are reached from code that names only the ARRAY
/// type: a foreign pointer array <c>*[N]*E</c> (<see cref="NativePointerArrayBox{E}"/>), reached from
/// <see cref="builtin.NativeArrayPointer{T}"/> with T = <c>ж&lt;E&gt;</c>; and the window over a native struct's
/// array field (<c>FieldRefBox</c>'s native window), reached with only <c>array&lt;X&gt;</c>. Building either by
/// MakeGenericMethod has no code under Native AOT for a value type, and building it through every type's compiled
/// operations (<see cref="IGoTypeOps"/>) put the box's code into every such type: +17.4% on the probe binary for a
/// door few programs reach. So each opener exists only for an element some compilation SPELLS: go2cs-gen registers
/// it in the package's module initializer wherever a closed <c>array&lt;ж&lt;E&gt;&gt;</c>, a closed
/// <c>unsafe.Slice&lt;ж&lt;E&gt;&gt;</c> or a struct field <c>array&lt;X&gt;</c> is written.
/// </para>
/// <para>
/// A MISS FAILS CLOSED, under the JIT exactly as under Native AOT: a Go panic that names the element, never a read
/// through some other route. An element no compilation spells is one only reflection can build (an
/// <c>ArrayOf(N, PointerTo(E))</c>), and a generator that missed a spelling then fails in our own tests rather
/// than only in a published executable.
/// </para>
/// </remarks>
public static class NativeArrayOpeners
{
    // E -> NativePointerArrayBox<E>.Over, answering a ж<array<ж<E>>>.
    private static readonly ConcurrentDictionary<Type, Func<nuint, nint, object>> s_pointerArrays = new();

    // X -> builtin.NativeArrayPointer<X>, answering a ж<array<X>>.
    private static readonly ConcurrentDictionary<Type, Func<nuint, nint, object>> s_arrays = new();

    /// <summary>Registers the foreign pointer-array door for <c>*[N]*E</c>. Idempotent.</summary>
    public static void RegisterPointerArray<E>() =>
        s_pointerArrays.TryAdd(typeof(E), static (address, length) => NativePointerArrayBox<E>.Over(address, length));

    /// <summary>Registers the native array door for <c>*[N]X</c>, the window over an <c>[N]X</c> field. Idempotent.</summary>
    public static void RegisterArray<X>() =>
        s_arrays.TryAdd(typeof(X), static (address, length) => builtin.NativeArrayPointer<X>(address, length));

    /// <summary>A <c>*[length]*E</c> over the foreign words at <paramref name="address"/>.</summary>
    internal static object OpenPointerArray(Type element, nuint address, nint length) =>
        s_pointerArrays.TryGetValue(element, out Func<nuint, nint, object>? open)
            ? open(address, length)
            : throw new PanicException($"go2cs: native pointer array: *[{length}]*{GoTypeName(element)} over memory the Go runtime did not allocate: this build never spells *[N]*{GoTypeName(element)}, so no reader for it was compiled");

    /// <summary>A <c>*[length]X</c> over the native memory at <paramref name="address"/>.</summary>
    internal static object OpenArray(Type element, nuint address, nint length) =>
        s_arrays.TryGetValue(element, out Func<nuint, nint, object>? open)
            ? open(address, length)
            : throw new PanicException($"go2cs: native array field: a [{length}]{GoTypeName(element)} field of a struct in native memory: this build declares no [N]{GoTypeName(element)} field, so no window for it was compiled");

    // The element as Go spells it (`main.E`, `*uint16`): reflect's own type string.
    private static string GoTypeName(Type type) => GoReflect.GoTypeName(type);
}
