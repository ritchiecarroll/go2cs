//******************************************************************************************************
//  ж.SliceHeaderBox.cs - Gbtc
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
//  09/05/2026 - C1 (go2cs fleet)
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using go.golib;

namespace go;

/// <summary>
/// The SLICE-HEADER reinterpretation: <c>(*slice)(unsafe.Pointer(&amp;b))</c>, the shape Go's runtime
/// uses to read a <c>[]T</c>'s (array, len, cap) words — <c>bytesHash</c>, <c>printslice</c>,
/// <c>convTslice</c>'s nil test, the cgo callers walk — emitted as
/// <c>Ꮡb.Reinterpret&lt;slice&lt;T&gt;, Δsliceᴛ&gt;()</c>.
/// </summary>
/// <remarks>
/// <para>
/// golib's <see cref="slice{T}"/> is not layout-compatible with Go's three-word header (five fields
/// including a <c>T[]</c> reference and a native base against a pointer and two integers), so before
/// this box <see cref="PointerExtensions.Reinterpret{T, TDst}"/> took its ADDRESS route for the pair
/// and minted a <see cref="NativeBox{T}"/> over the PINNED managed struct — and the header's first
/// field then read back the <c>m_array</c> reference bits AS a pointer object: a type-confused
/// reference whose real runtime type is <c>System.Byte[]</c>, with <c>len</c>/<c>cap</c> reading
/// golib's low index and length. Measured on 2026-09-04 (Release, tiering off): array →
/// <c>System.Byte[]</c>, len 16, cap 8 where Go says 8 and 24, and the first dereference a native
/// SIGSEGV with empty stderr (the runtime row's <c>TestMapBuckets</c> signature). That was the wall in
/// front of the runtime row's twelve <c>bytesHash</c> rows, one frame BEFORE the hash stub it hid behind.
/// </para>
/// <para>
/// This box MATERIALIZES the header from the LIVE slice on every access: <c>array</c> is the
/// element-0 box of the slice's backing at its low index wrapped by the pointer type's own
/// <c>FromBox</c> — a TRANSIENT address that RETAINS the element box (no pin, no provenance entry,
/// no finalizable holder; the consumer that matters, <c>runtime.memhash</c>, reads the retained box,
/// and a consumer that does arithmetic on the bare number gets one it refuses by name); a nil slice
/// gives a nil pointer and 0/0; an empty NON-nil slice gives a non-nil pointer at its backing, as Go;
/// <c>len</c>/<c>cap</c> are the slice's. The pointer object is re-minted only when the slice's
/// backing or low index moves, so a reassigned slice variable is followed through the box.
/// </para>
/// <para>
/// WRITES. A <c>ref</c> cannot be intercepted, so a header written through this box lands on the
/// materialized copy and never reaches the slice. The box compares each fresh materialization against
/// what it last handed out and PANICS naming the class on the NEXT access if the copy was written —
/// which makes a field-by-field header build (<c>rp.array = …; rp.len = …; rp.cap = …</c>) loud at its
/// second access, and leaves a single whole-header assignment silent on the copy. Both are stated
/// rather than hidden: before this box every such write corrupted the pinned managed slice struct's
/// own bytes, so "silent on a detached copy" is strictly safer and "loud on the next access" is new.
/// Honouring the write direction needs a commit step a <c>ref</c> cannot express: the header's LAST
/// store (<c>ranges.array = persistentalloc(…)</c>, after <c>len</c> and <c>cap</c>) is followed by no
/// access of the box at all, so there is no point at which a materializing box can observe the
/// completed header and re-base the slice — and committing EARLIER is impossible, because between the
/// <c>cap</c> store and the <c>array</c> store the header names a nil array with a non-zero capacity
/// (read on <c>addrRanges.init</c>'s own sequence, increment 7 of the runtime row, 2026-09-05). The
/// corpus's field-by-field writers — <c>addrRanges.init</c>/<c>add</c>/<c>cloneInto</c> — are therefore
/// hand-owned AT the write (<c>runtime/mranges_impl.cs</c>, re-basing over
/// <see cref="slice{T}.OverNativeMemory(nuint, nint, nint)"/>); the whole-header writers
/// (<c>traceMap</c>, <c>mheap.allspans</c>, <c>allArenas</c>) sit behind <c>sysAlloc</c>/<c>mheap.init</c>
/// and stay unreached. The header→slice direction is the mirror box, <see cref="HeaderSliceBox{T, TDst}"/>.
/// </para>
/// <para>
/// SCOPE. <typeparamref name="T"/> is exactly <c>slice&lt;X&gt;</c>; <typeparamref name="TDst"/> is a
/// value type whose three instance fields are, in declaration order, a class implementing
/// <see cref="IUnsafePointer"/> (the <c>unsafe.Pointer</c> class; golib cannot name that assembly, so
/// the factory is resolved by reflection once per pair and pinned by a guard), then two <c>nint</c>s.
/// A header whose first field is a typed <c>ж&lt;…&gt;</c> (<c>notInHeapSlice</c>) is NOT admitted —
/// every corpus site that takes that view of a managed slice is a WRITER (the field-by-field family,
/// hand-owned; the whole-header family, unreached), so there is no reaching READ for it to serve. The STRING header
/// (<c>@string</c> → <c>stringStruct</c>, two fields) is one predicate line away and deliberately
/// not switched on: its live route is the <c>(uintptr)</c> token bridge, which resolves a
/// reference-bearing box only once Q44's token registry lands.
/// </para>
/// </remarks>
internal sealed class SliceHeaderBox<T, TDst> : ж<TDst>
{
    // ---- the shape, resolved once per (T, TDst) ----

    private static readonly FieldInfo[]? s_fields;      // [0] pointer, [1] len, [2] cap — declaration order
    private static readonly MethodInfo? s_fromBox;      // <pointer type>.FromBox<X>(ж<X>) → pointer object
    private static readonly Func<IArray, (object? backing, nint low, nint len, nint cap)>? s_describe;
    private static readonly Func<IArray, object>? s_elementZero;

    /// <summary>Whether the pair (<typeparamref name="T"/>, <typeparamref name="TDst"/>) is a slice → header reinterpretation this box serves.</summary>
    internal static readonly bool Applies;

    static SliceHeaderBox()
    {
        Type source = typeof(T);
        Type header = typeof(TDst);

        if (!source.IsGenericType || source.GetGenericTypeDefinition() != typeof(slice<>) || !header.IsValueType || header.IsPrimitive || header.IsEnum)
            return;

        Type element = source.GetGenericArguments()[0];
        FieldInfo[] fields = header
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(field => field.MetadataToken)
            .ToArray();

        if (fields.Length != 3)
            return;

        Type pointerType = fields[0].FieldType;

        if (!pointerType.IsClass || !typeof(IUnsafePointer).IsAssignableFrom(pointerType))
            return;

        if (fields[1].FieldType != typeof(nint) || fields[2].FieldType != typeof(nint))
            return;

        // The pointer type's own retaining factory: `public static <Pointer> FromBox<X>(ж<X> box)`.
        // Resolved by name because the marker interface is empty and golib cannot reference the
        // assembly that defines the class; the contract is pinned by SliceHeaderReinterpretTests.
        MethodInfo? fromBox = pointerType.GetMethod("FromBox", BindingFlags.Public | BindingFlags.Static);

        if (fromBox is null || !fromBox.IsGenericMethodDefinition || fromBox.GetGenericArguments().Length != 1)
            return;

        MethodInfo closed = fromBox.MakeGenericMethod(element);

        if (!pointerType.IsAssignableFrom(closed.ReturnType))
            return;

        MethodInfo describe = typeof(SliceHeaderBox<T, TDst>).GetMethod(nameof(Describe), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(element);
        MethodInfo elementZero = typeof(SliceHeaderBox<T, TDst>).GetMethod(nameof(ElementZero), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(element);

        s_fields = fields;
        s_fromBox = closed;
        s_describe = (Func<IArray, (object?, nint, nint, nint)>)Delegate.CreateDelegate(typeof(Func<IArray, (object?, nint, nint, nint)>), describe);
        s_elementZero = (Func<IArray, object>)Delegate.CreateDelegate(typeof(Func<IArray, object>), elementZero);
        Applies = true;
    }

    // The slice's identity words, without allocating: backing array (null for a nil slice), low, len, cap.
    private static (object? backing, nint low, nint len, nint cap) Describe<X>(IArray array)
    {
        slice<X> source = (slice<X>)array;
        return (source.m_array, source.Low, source.Length, source.Capacity);
    }

    // The element-0 box at the slice's low index — Go's `s.array`. Constructed only when the words moved.
    private static object ElementZero<X>(IArray array)
    {
        return new ElemRefBox<X>(array, 0);
    }

    internal static ж<TDst> Mint(ж<T> source)
    {
        return new SliceHeaderBox<T, TDst>(source);
    }

    // ---- the instance ----

    private readonly ж<T> m_source;
    private TDst m_value;               // the header handed out through Value (a ref into this field)
    private TDst m_handedOut;           // what that header held when it was handed out
    private bool m_materialized;
    private object? m_pointer;          // the cached pointer object, minted for (m_pointerBacking, m_pointerLow)
    private object? m_pointerBacking;
    private nint m_pointerLow = -1;

    private SliceHeaderBox(ж<T> source)
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

    /// <summary>The header is the slice variable seen through another type, so it orders and compares as that variable's box.</summary>
    public override nuint PointerOrderToken => m_source.PointerOrderToken;

    public override bool Equals(ж<TDst>? other)
    {
        return other is SliceHeaderBox<T, TDst> header && ReferenceEquals(header.m_source, m_source);
    }

    public override int GetHashCode()
    {
        return m_source.GetHashCode();
    }

    private void Materialize()
    {
        if (m_materialized && !EqualityComparer<TDst>.Default.Equals(m_value, m_handedOut))
            throw new PanicException($"slice header written through a reinterpretation: a {typeof(TDst).Name} built over a {typeof(T).Name} was assigned through the header and the managed slice cannot be rebuilt from Go's (array, len, cap) words — the write landed on a detached copy (the runtime's field-by-field writers, addrRanges.init/add/cloneInto, are hand-owned at the write for exactly this reason; its whole-header writers sit behind sysAlloc/mheap.init)");

        (object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value);

        if (m_pointer is null || !ReferenceEquals(backing, m_pointerBacking) || low != m_pointerLow)
        {
            object? elementZero = backing is null ? null : s_elementZero!((IArray)(object)m_source.Value);
            m_pointer = s_fromBox!.Invoke(null, [elementZero]);
            m_pointerBacking = backing;
            m_pointerLow = low;
        }

        object boxed = default(TDst)!;
        s_fields![0].SetValue(boxed, m_pointer);
        s_fields[1].SetValue(boxed, len);
        s_fields[2].SetValue(boxed, cap);

        m_value = (TDst)boxed;
        m_handedOut = m_value;
        m_materialized = true;
    }
}
