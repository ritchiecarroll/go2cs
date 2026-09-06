//******************************************************************************************************
//  ж.HeaderSliceBox.cs - Gbtc
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
/// The HEADER → SLICE reinterpretation, the mirror of <see cref="SliceHeaderBox{T, TDst}"/>:
/// <c>*(*[]T)(unsafe.Pointer(&amp;sl))</c> over a <c>notInHeapSlice</c> value the runtime has just
/// filled with a NATIVE base and Go's (len, cap) — the shape by which the page allocator's summary
/// levels (<c>mpagealloc_64bit.go</c>), the scavenge index's chunk array and a span's heap-bits
/// window (<c>mbitmap.go</c>) become ordinary slices over reserved memory. Emitted as
/// <c>~Ꮡsl.Reinterpret&lt;notInHeapSlice, slice&lt;X&gt;&gt;()</c>.
/// </summary>
/// <remarks>
/// <para>
/// Before this box the pair took <see cref="PointerExtensions.Reinterpret{T, TDst}"/>'s ADDRESS route:
/// a <see cref="NativeBox{T}"/> over the pinned three-word header, whose <c>Value</c> read a forty-byte
/// golib <see cref="slice{T}"/> struct out of twenty-four bytes of header — the backing-array slot
/// read the <c>ж</c> box's reference bits, <c>m_low</c> read <c>len</c>, and everything past the
/// header's end was whatever followed it on the heap. That is the wall the eight page-allocator rows
/// stand at once <c>physPageSize</c> is set (increment 7 of the runtime row, 2026-09-05).
/// </para>
/// <para>
/// This box MATERIALIZES a slice from the LIVE header on every access: a NATIVE array box re-bases a
/// native-backed <see cref="slice{T}"/> over its address through the model's single creation door,
/// <see cref="slice{T}.OverNativeMemory(nuint, nint, nint)"/>, with the header's <c>len</c> and
/// <c>cap</c> as Go's second and third words — writes reach the memory, element addresses are the real
/// ones, and lifetime is the mapping's own; a nil array with zero words is Go's nil slice. Two shapes are
/// REFUSED by name rather than approximated: a nil array carrying a non-zero length or capacity (not a
/// slice Go can build either), and an array box over MANAGED storage (an element box of a golib slice
/// or array) — re-basing a managed slice from a header has no reaching site in the corpus and is
/// recorded, not built. A reference-bearing element type is refused by the door itself, in its words.
/// </para>
/// <para>
/// WRITES. The slice handed out through <c>Value</c> is a materialized copy; a header re-read shows
/// the same words, and a write to the copy's own words (a reassignment of the slice through the box)
/// cannot reach the header. As with the sibling box the mismatch is detected on the NEXT access and
/// refused by name; every corpus site derefs the box once (<c>~</c>) and never writes it.
/// </para>
/// <para>
/// SCOPE. <typeparamref name="T"/> is a value type whose three instance fields are, in declaration
/// order, a constructed <c>ж&lt;Y&gt;</c> (the <c>notInHeapSlice</c> shape) and two <c>nint</c>s;
/// <typeparamref name="TDst"/> is exactly <c>slice&lt;X&gt;</c>. The <c>unsafe.Pointer</c>-first header
/// (<c>runtime.slice</c> → <c>[]metricSample</c>, <c>readMetricsLocked</c>) is deliberately NOT admitted:
/// its one corpus site carries a MANAGED array through the address route today and is exercised by
/// banked rows, so it is left on the route it is measured on.
/// </para>
/// </remarks>
internal sealed class HeaderSliceBox<T, TDst> : ж<TDst>
{
    // ---- the shape, resolved once per (T, TDst) ----

    private static readonly FieldInfo[]? s_fields;                                   // [0] array box, [1] len, [2] cap — declaration order
    private static readonly Func<object, (bool nil, bool native, nuint address)>? s_words;  // the array box's identity words
    private static readonly Func<nuint, nint, nint, object>? s_rebase;               // slice<X>.OverNativeMemory(address, len, cap), boxed

    /// <summary>Whether the pair (<typeparamref name="T"/>, <typeparamref name="TDst"/>) is a header → slice reinterpretation this box serves.</summary>
    internal static readonly bool Applies;

    static HeaderSliceBox()
    {
        Type header = typeof(T);
        Type target = typeof(TDst);

        if (!target.IsGenericType || target.GetGenericTypeDefinition() != typeof(slice<>) || !header.IsValueType || header.IsPrimitive || header.IsEnum)
            return;

        Type element = target.GetGenericArguments()[0];
        FieldInfo[] fields = header
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(field => field.MetadataToken)
            .ToArray();

        if (fields.Length != 3)
            return;

        Type pointerType = fields[0].FieldType;

        // A constructed ж<Y> — the notInHeapSlice shape. The unsafe.Pointer-first header is excluded on
        // purpose (see the remarks); a class implementing IUnsafePointer never satisfies this test.
        if (!pointerType.IsClass || !pointerType.IsGenericType || pointerType.GetGenericTypeDefinition() != typeof(ж<>))
            return;

        if (fields[1].FieldType != typeof(nint) || fields[2].FieldType != typeof(nint))
            return;

        Type pointee = pointerType.GetGenericArguments()[0];
        MethodInfo words = typeof(HeaderSliceBox<T, TDst>).GetMethod(nameof(Words), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(pointee);
        MethodInfo rebase = typeof(HeaderSliceBox<T, TDst>).GetMethod(nameof(Rebase), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(element);

        s_fields = fields;
        s_words = (Func<object, (bool, bool, nuint)>)Delegate.CreateDelegate(typeof(Func<object, (bool, bool, nuint)>), words);
        s_rebase = (Func<nuint, nint, nint, object>)Delegate.CreateDelegate(typeof(Func<nuint, nint, nint, object>), rebase);
        Applies = true;
    }

    // The array box's identity: nil, native, and the native address (0 for a managed element box).
    private static (bool nil, bool native, nuint address) Words<Y>(object pointer)
    {
        ж<Y> box = (ж<Y>)pointer;
        return (box.IsNilPointer, box.IsNative, box.NativeAddress);
    }

    // The single creation door, closed over the element type.
    private static object Rebase<X>(nuint address, nint len, nint cap)
    {
        return slice<X>.OverNativeMemory(address, len, cap);
    }

    internal static ж<TDst> Mint(ж<T> source)
    {
        return new HeaderSliceBox<T, TDst>(source);
    }

    // ---- the instance ----

    private readonly ж<T> m_source;
    private TDst m_value;               // the slice handed out through Value (a ref into this field)
    private TDst m_handedOut;           // what that slice was when it was handed out
    private bool m_materialized;

    private HeaderSliceBox(ж<T> source)
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
    // NEVER an address, for the same recorded reason as the sibling header kind: Value hands out a
    // ref into m_value, a copy this box MATERIALIZES on demand, so `fixed` would take the address of
    // a temporary that the next Materialize replaces -- and the address route over a header box has
    // a measured native crash behind it. Ordering by m_source's token (above) says the same thing
    // from the other side: the identity here is the SOURCE variable's, never this copy's location.
    //
    // This kind is the reason the answer is ABSTRACT rather than virtual. It was added while the
    // repair was being cut against a base that predates it, and the union's first build stopped at
    // CS0534 instead of silently giving it an answer to a question it was never asked.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <summary>The slice is the header variable seen through another type, so it orders and compares as that variable's box.</summary>
    public override nuint PointerOrderToken => m_source.PointerOrderToken;

    public override bool Equals(ж<TDst>? other)
    {
        return other is HeaderSliceBox<T, TDst> view && ReferenceEquals(view.m_source, m_source);
    }

    public override int GetHashCode()
    {
        return m_source.GetHashCode();
    }

    private void Materialize()
    {
        if (m_materialized && !EqualityComparer<TDst>.Default.Equals(m_value, m_handedOut))
            throw new PanicException($"slice written through a header reinterpretation: a {typeof(TDst).Name} built over a {typeof(T).Name} was reassigned through the view and the header cannot be rebuilt from it — the write landed on a detached copy");

        object boxed = m_source.Value!;
        object? pointer = s_fields![0].GetValue(boxed);
        nint len = (nint)s_fields[1].GetValue(boxed)!;
        nint cap = (nint)s_fields[2].GetValue(boxed)!;

        (bool nil, bool native, nuint address) = pointer is null ? (true, false, (nuint)0) : s_words!(pointer);

        if (nil)
        {
            if (len != 0 || cap != 0)
                throw new PanicException($"slice header names a nil array with len {len} and cap {cap}: a {typeof(TDst).Name} cannot be built over a {typeof(T).Name} whose array is nil unless both words are zero");

            m_value = default!;
        }
        else if (native)
        {
            m_value = (TDst)s_rebase!(address, len, cap);
        }
        else
        {
            throw new PanicException($"slice header names MANAGED storage: a {typeof(TDst).Name} cannot be re-based over a {typeof(T).Name} whose array is an element box of a golib slice or array (no reaching site; recorded, not built)");
        }

        m_handedOut = m_value;
        m_materialized = true;
    }
}
