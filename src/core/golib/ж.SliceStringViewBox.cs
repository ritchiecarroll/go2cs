//******************************************************************************************************
//  ж.SliceStringViewBox.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/24/2026 - P5 (go2cs fleet)
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using go.golib;

namespace go;

/// <summary>
/// A SLICE read through a two-word string header: <c>*(*str)(unsafe.Pointer(&amp;b))</c> over a <c>[]T</c>, emitted as
/// <c>Ꮡb.Reinterpret&lt;slice&lt;T&gt;, str&gt;()</c>. Go's slice header begins with the string header's two words (Data,
/// Len), so the view reads the slice's data pointer and its length and ignores its capacity. github.com/zeebo/xxh3 reads
/// a <c>[]byte</c> this way (hash64.go, <c>*(*str)(ptr(&amp;b))</c>).
/// </summary>
/// <remarks>
/// <para>
/// READ-ONLY. The header types are <see cref="StringHeaderBox{TDst}"/>'s (see its SCOPE), and the words are built the
/// same way: Data is the slice's element-0 box (the zerobase element for a zero-capacity slice, nil for a nil slice),
/// wrapped by <c>unsafe.Pointer.FromBoxObject</c>, or that box's pinned number for the uintptr shape; Len is the
/// slice's length. A store through the header would move a slice's data and length with its capacity left behind,
/// which no corpus or module site does; it is refused by name through the generated setter, and a store that bypasses
/// the setter is refused on the next access (StringHeaderBox's limit L1).
/// </para>
/// </remarks>
internal sealed class SliceStringHeaderBox<T, TDst> : ж<TDst>, IHeaderDataCarrier
{
    /// <summary>Whether the pair (<typeparamref name="T"/>, <typeparamref name="TDst"/>) is a slice read through a string header.</summary>
    internal static readonly bool Applies =
        typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(slice<>) && StringHeaderBox<TDst>.Applies;

    internal static ж<TDst> Mint(ж<T> source)
    {
        return new SliceStringHeaderBox<T, TDst>(source);
    }

    private readonly ж<T> m_source;
    private TDst m_value = default!;
    private TDst m_handedOut = default!;
    private bool m_materialized;
    private object? m_element;           // the element-0 box the Data word was minted from
    private object? m_pointer;           // the pointer object over m_element (the unsafe.Pointer shape)
    private nuint m_number;              // the number shape's Data word for m_element
    private object? m_elementBacking;    // the slice words m_element was minted for
    private nint m_elementLow = -1;
    private bool m_elementZeroCapacity;

    private SliceStringHeaderBox(ж<T> source)
    {
        m_source = source;
        AllocationCounter.Count();
    }

    public override ref TDst Value
    {
        get
        {
            Materialize();
            return ref m_value;
        }
    }

    public override ref TDst ValueSlot => ref Value;

    public override bool IsNull => false;

    public override object? PinnableStorage => null;

    /// <inheritdoc/>
    // Never an address: the value is materialized on demand, as the other header boxes' are.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <summary>The header is the slice variable seen through another type, so it orders and compares as that variable's box.</summary>
    public override nuint PointerOrderToken => m_source.PointerOrderToken;

    internal override ulong AllocationId => m_source.AllocationId;

    public override bool Equals(ж<TDst>? other)
    {
        return other is SliceStringHeaderBox<T, TDst> header && ReferenceEquals(header.m_source, m_source);
    }

    public override int GetHashCode()
    {
        return m_source.GetHashCode();
    }

    /// <summary>Refuses the store by name: a slice read through a string header is read-only (see the remarks).</summary>
    public override void OnHeaderStore()
    {
        m_materialized = false;

        throw new PanicException($"slice header not committed: a {typeof(TDst).Name} built over a {GoReflect.GoTypeName(typeof(T))} reads the slice's data pointer and length and is read-only here; a store through it would move the slice's words with its capacity left behind (see SliceStringHeaderBox)");
    }

    /// <inheritdoc/>
    public nuint HeaderDataWord(out object? carried)
    {
        object? data = StringHeaderBox<TDst>.Fields![0].GetValue(Value);

        if (!StringHeaderBox<TDst>.DataIsNumber)
        {
            carried = (data as IUnsafePointer)?.RetainedSource;
            return data is ж<uintptr> { IsNull: false } word ? word.Value.Value : 0;
        }

        nuint number = ((uintptr)data!).Value;
        carried = number != 0 && number == m_number ? m_element : null;
        return number;
    }

    private void Materialize()
    {
        if (m_materialized && !EqualityComparer<TDst>.Default.Equals(m_value, m_handedOut))
            throw new PanicException($"slice header written through a reinterpretation: a {typeof(TDst).Name} built over a {GoReflect.GoTypeName(typeof(T))} was assigned through the header without its generated setter, and the header is read-only (see SliceStringHeaderBox)");

        (object? backing, nint low, nint len, nint cap) = ((IGoReflectSlice)(object)m_source.Value!).ReflectDescribe();
        FieldInfo[] fields = StringHeaderBox<TDst>.Fields!;
        object boxed = default(TDst)!;

        if (backing is null)
        {
            // A nil slice: Data is nil (or 0) and Len 0.
            m_element = null;
            m_pointer = null;
            m_number = 0;
            m_elementBacking = null;
            m_elementLow = -1;
        }
        else if (m_element is null || !ReferenceEquals(backing, m_elementBacking) || low != m_elementLow || (cap == 0) != m_elementZeroCapacity)
        {
            m_element = ((IGoReflectSlice)(object)m_source.Value!).ReflectElementZero();
            m_elementBacking = backing;
            m_elementLow = low;
            m_elementZeroCapacity = cap == 0;

            if (StringHeaderBox<TDst>.DataIsNumber)
            {
                m_pointer = null;
                m_number = ((INilPointer)m_element).StableAddress();
            }
            else
            {
                m_pointer = StringHeaderBox<TDst>.FromBoxMethod!.Invoke(null, [m_element]);
                m_number = 0;
            }
        }

        if (StringHeaderBox<TDst>.DataIsNumber)
            fields[0].SetValue(boxed, (uintptr)m_number);
        else
            fields[0].SetValue(boxed, backing is null ? StringHeaderBox<TDst>.FromBoxMethod!.Invoke(null, [null]) : m_pointer);

        fields[1].SetValue(boxed, StringHeaderBox<TDst>.LenIsUnsigned ? (object)(nuint)len : len);

        m_value = (TDst)boxed;
        m_handedOut = m_value;
        m_materialized = true;
    }
}

/// <summary>
/// A <c>[]byte</c> read AS a string: <c>*(*string)(unsafe.Pointer(&amp;b))</c>, emitted as
/// <c>Ꮡb.Reinterpret&lt;slice&lt;byte&gt;, @string&gt;()</c>, the zero-copy byte-to-string idiom (xxh3's hasher.go, and
/// strings.Builder's own before unsafe.String). Go's string header is the slice header's first two words, so the string
/// is the slice's bytes up to its length, SHARING them: a later write to the slice is seen through the string, as in Go.
/// </summary>
/// <remarks>
/// READ-ONLY. The string is rebuilt from the live slice on every access; a store through the view (a <c>@string</c> has
/// no setter to intercept) is refused by name on the next access, as the header boxes' limit L1 is. Only a byte slice is
/// served: a slice of wider elements read as a string reads Len ELEMENTS' worth of the first bytes in Go, which no site
/// does, and keeps the address route's refusal.
/// </remarks>
internal sealed class SliceAsStringBox<T> : ж<@string>
{
    /// <summary>Whether <typeparamref name="T"/> is <c>[]byte</c>, the one slice this view serves.</summary>
    internal static readonly bool Applies = typeof(T) == typeof(slice<byte>);

    internal static ж<@string> Mint(ж<T> source)
    {
        return new SliceAsStringBox<T>((ж<slice<byte>>)(object)source);
    }

    private readonly ж<slice<byte>> m_source;
    private @string m_value;
    private @string m_handedOut;
    private bool m_materialized;

    private SliceAsStringBox(ж<slice<byte>> source)
    {
        m_source = source;
        AllocationCounter.Count();
    }

    public override ref @string Value
    {
        get
        {
            if (m_materialized && !m_value.SameWindowAs(m_handedOut))
                throw new PanicException("string written through a reinterpretation: a string read over a []byte was assigned, and the view is read-only (see SliceAsStringBox)");

            m_value = @string.AliasOf(m_source.Value);
            m_handedOut = m_value;
            m_materialized = true;
            return ref m_value;
        }
    }

    public override ref @string ValueSlot => ref Value;

    public override bool IsNull => false;

    public override object? PinnableStorage => null;

    /// <inheritdoc/>
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <summary>The string is the slice variable seen through another type, so it orders and compares as that variable's box.</summary>
    public override nuint PointerOrderToken => m_source.PointerOrderToken;

    internal override ulong AllocationId => m_source.AllocationId;

    public override bool Equals(ж<@string>? other)
    {
        return other is SliceAsStringBox<T> view && ReferenceEquals(view.m_source, m_source);
    }

    public override int GetHashCode()
    {
        return m_source.GetHashCode();
    }
}
