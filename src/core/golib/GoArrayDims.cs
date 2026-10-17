// GoArrayDims.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.CompilerServices;

namespace go;

/// <summary>
/// A phantom type that carries a Go array type's DIMENSIONS as a C# type argument.
/// </summary>
/// <remarks>
/// <para>
/// C# spells every Go <c>[N]E</c> as the one <see cref="array{T}"/>, whose length lives in the INSTANCE. A generic
/// whose type argument is an array therefore has no length to build <c>var x T</c> or <c>new(T)</c> from: both read
/// as a length-0 array (runtime's <c>testFreegc[[16]byte]</c> had <c>unsafe.Sizeof(*p) == 0</c>). The converter gives
/// such a generic an ARRAY-DIMS COMPANION type parameter, constrained to this interface, and every call site supplies
/// a phantom type naming the dims: <c>ᴀ16</c> for <c>[16]E</c>, <c>ᴀ2_3</c> for <c>[2][3]E</c>, or <see cref="ᴀnone"/> when the
/// type argument is not an array. See src/go2cs/arrayDimsCompanion.go.
/// </para>
/// <para>
/// The dims are a static abstract member, so a body reads them from its type parameter with no instance, no
/// reflection and the same answer under native AOT.
/// </para>
/// </remarks>
public interface IGoArrayDims
{
    /// <summary>The array dimensions, outermost first; empty when the type argument is not an array.</summary>
    static abstract nint[] Dims { get; }
}

/// <summary>The array-dims companion for a type argument that is not an array.</summary>
public readonly struct ᴀnone : IGoArrayDims
{
    /// <inheritdoc/>
    public static nint[] Dims => [];
}

/// <summary>
/// Go zero values of a type parameter whose array dimensions travel in an <see cref="IGoArrayDims"/> companion.
/// </summary>
public static class GoArrayZero
{
    /// <summary>The zero value of <typeparamref name="T"/>: <c>var x T</c>.</summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <typeparam name="TDims">Its array-dims companion.</typeparam>
    /// <returns>A sized zero array when <typeparamref name="TDims"/> names dims; otherwise <see cref="builtin.GoZero{T}()"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Of<T, TDims>() where TDims : IGoArrayDims =>
        TDims.Dims.Length == 0 ? builtin.GoZero<T>() : (T)GoReflect.MakeSizedArray(typeof(T), TDims.Dims, 0);

    /// <summary>A pointer to a new zero value of <typeparamref name="T"/>: <c>new(T)</c>.</summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <typeparam name="TDims">Its array-dims companion.</typeparam>
    /// <returns>The box <see cref="builtin.@new{T}()"/> returns, holding a sized zero array when <typeparamref name="TDims"/> names dims.</returns>
    /// <remarks><see cref="builtin.Ꮡ{T}(in T)"/> charges the allocation to GoMemProfile, as <see cref="builtin.@new{T}()"/> does.</remarks>
    public static ж<T> New<T, TDims>() where TDims : IGoArrayDims =>
        TDims.Dims.Length == 0 ? builtin.@new<T>() : builtin.Ꮡ(Of<T, TDims>());

    /// <summary>
    /// A call the converter REFUSED: an exported generic called with an array type argument at a parameter whose
    /// zero value it builds, which cannot carry an array-dims companion. Panics with <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The converter's refusal.</param>
    public static void Refuse(string message) => throw builtin.panic(message);

    /// <summary>The value-returning form of <see cref="Refuse(string)"/>, typed as the refused call's result.</summary>
    /// <typeparam name="T">The refused call's result type.</typeparam>
    /// <param name="message">The converter's refusal.</param>
    /// <returns>Never returns.</returns>
    public static T Refuse<T>(string message) => throw builtin.panic(message);
}
