// GoManagedLayout.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace go.golib;

/// <summary>
/// A struct's MANAGED layout, measured by the struct itself: each field's CLR byte offset, its CLR size and its CLR
/// alignment. golib's inline-layout gate compares it with the Go layout (docs/phase4/DESIGN-array-value-storage.md,
/// 7.6.3).
/// </summary>
/// <remarks>
/// The offsets are read with <see cref="Unsafe.ByteOffset{T}(ref readonly T, ref readonly T)"/> over field references,
/// never with <c>Marshal.OffsetOf</c>, which answers the MARSHALED layout and differs from the managed one whenever a
/// bool or a char is present. go2cs-gen writes the table for every struct the converter admits to inline arrays, so it
/// needs no reflection and holds under Native AOT.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoManagedLayout
{
    private readonly string[] m_fieldNames;
    private readonly nint[] m_fieldOffsets;

    /// <summary>
    /// Creates a managed layout.
    /// </summary>
    /// <param name="fieldNames">The C# names of the struct's instance fields.</param>
    /// <param name="fieldOffsets">Each field's CLR byte offset, in the order of <paramref name="fieldNames"/>.</param>
    /// <param name="size">The struct's CLR size, <c>Unsafe.SizeOf</c>.</param>
    /// <param name="alignment">The struct's CLR alignment, <see cref="AlignmentOf{T}"/>.</param>
    public GoManagedLayout(string[] fieldNames, nint[] fieldOffsets, nint size, nint alignment)
    {
        if (fieldNames.Length != fieldOffsets.Length)
            throw new ArgumentException("one offset per field name", nameof(fieldOffsets));

        m_fieldNames = fieldNames;
        m_fieldOffsets = fieldOffsets;
        Size = size;
        Alignment = alignment;
    }

    /// <summary>
    /// Gets the struct's CLR size.
    /// </summary>
    public nint Size { get; }

    /// <summary>
    /// Gets the struct's CLR alignment.
    /// </summary>
    public nint Alignment { get; }

    /// <summary>
    /// Gets the CLR byte offset of the field named <paramref name="fieldName"/>.
    /// </summary>
    public bool TryGetOffset(string fieldName, out nint offset)
    {
        int index = Array.IndexOf(m_fieldNames, fieldName);

        offset = index < 0 ? 0 : m_fieldOffsets[index];
        return index >= 0;
    }

    /// <summary>
    /// The CLR alignment of <typeparamref name="T"/>: its offset after a byte in a sequential struct. The CLR honors a
    /// sequential layout for an unmanaged <typeparamref name="T"/>, which every inline-array struct is.
    /// </summary>
    public static nint AlignmentOf<T>() where T : unmanaged
    {
        AfterByte<T> probe = default;
        return Unsafe.ByteOffset(ref Unsafe.As<AfterByte<T>, byte>(ref probe), ref Unsafe.As<T, byte>(ref probe.Value));
    }

    /// <summary>
    /// The CLR byte offset of <paramref name="field"/> inside <paramref name="owner"/>.
    /// </summary>
    public static nint OffsetOf<TOwner, TField>(ref readonly TOwner owner, ref readonly TField field) =>
        Unsafe.ByteOffset(ref Unsafe.As<TOwner, byte>(ref Unsafe.AsRef(in owner)), ref Unsafe.As<TField, byte>(ref Unsafe.AsRef(in field)));

    // Only the offset of Value is read; Head is the byte it follows.
    private struct AfterByte<T> where T : unmanaged
    {
#pragma warning disable CS0649
        public byte Head;
#pragma warning restore CS0649
        public T Value;
    }
}

/// <summary>
/// A struct that measures its own managed layout. go2cs-gen implements it for every struct the converter admits to
/// inline arrays; golib's inline-layout gate reads it.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public interface IGoManagedLayout
{
    /// <summary>
    /// Gets the struct's managed layout.
    /// </summary>
    GoManagedLayout ManagedLayout { get; }
}
