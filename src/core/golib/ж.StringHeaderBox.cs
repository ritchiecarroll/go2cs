//******************************************************************************************************
//  ж.StringHeaderBox.cs - Gbtc
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
//  10/21/2026 - C1 (go2cs fleet)
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using go.golib;

namespace go;

/// <summary>
/// The STRING-HEADER reinterpretation: <c>(*unsafeheader.String)(unsafe.Pointer(&amp;s))</c>, Go's (Data, Len) view of
/// a string, emitted as <c>Ꮡs.Reinterpret&lt;@string, unsafeheader.String&gt;()</c>. The string half of
/// <see cref="SliceHeaderBox{T, TDst}"/>, and built the same way.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="@string"/> is a (backing, offset, length) window, not Go's two words, so the address route refused the pair
/// ("its CLR layout is not its Go layout"). This box MATERIALIZES the header from the live string on every access:
/// <c>Data</c> is the element-0 box of the string's own byte window, wrapped by <c>unsafe.Pointer.FromBoxObject</c>,
/// exactly what <c>unsafe.StringData</c> names; <c>Len</c> is the string's length. An empty string's Data is nil, as
/// <c>unsafe.StringData</c> answers for it.
/// </para>
/// <para>
/// WRITES go through the generated header setter's <see cref="OnHeaderStore"/>, as the slice header's do, with the same
/// three limits (see <see cref="SliceHeaderBox{T, TDst}"/>): a store that bypasses the setter is refused on the next
/// access (L1); an inconsistent header is held until it is consistent (L2); a Data that names neither a byte of a
/// managed backing nor native memory is refused by name (L3). A committed string SHARES the backing its Data names, as
/// Go's alias does. Go strings are immutable, so the sharing is Go's own semantics. A native Data is the one copy: an
/// <see cref="@string"/> has no native-backed form, the same snapshot <c>unsafe.String</c> takes over native memory.
/// </para>
/// <para>
/// SCOPE. <typeparamref name="TDst"/> is a value type whose two instance fields are, in declaration order, the
/// <c>unsafe.Pointer</c> class and an <c>nint</c>: unsafeheader.String, runtime.stringStruct, internal/runtime/maps'
/// stringStruct.
/// </para>
/// </remarks>
internal sealed class StringHeaderBox<TDst> : ж<TDst>
{
    private static readonly FieldInfo[]? s_fields;      // [0] pointer, [1] len — declaration order
    private static readonly MethodInfo? s_fromBox;      // <pointer type>.FromBoxObject(object box) → pointer object

    /// <summary>Whether <typeparamref name="TDst"/> is a string header this box serves.</summary>
    internal static readonly bool Applies;

    // The pointer type is unsafe.Pointer, found by name as SliceHeaderBox finds it; see there.
    [System.Diagnostics.CodeAnalysis.DynamicDependency("FromBoxObject", "go.unsafe_package+Pointer", "unsafe")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "pointerType is unsafe.Pointer, whose FromBoxObject the DynamicDependency on this constructor keeps.")]
    static StringHeaderBox()
    {
        Type header = typeof(TDst);

        if (!header.IsValueType || header.IsPrimitive || header.IsEnum)
            return;

        FieldInfo[] fields = GoFieldMetadata.InstanceFields(header);

        if (fields.Length != 2)
            return;

        Type pointerType = fields[0].FieldType;

        if (!pointerType.IsClass || !typeof(IUnsafePointer).IsAssignableFrom(pointerType) || fields[1].FieldType != typeof(nint))
            return;

        MethodInfo? fromBox = pointerType.GetMethod("FromBoxObject", BindingFlags.Public | BindingFlags.Static, [typeof(object)]);

        if (fromBox is null || !pointerType.IsAssignableFrom(fromBox.ReturnType))
            return;

        s_fields = fields;
        s_fromBox = fromBox;
        Applies = true;
    }

    internal static ж<TDst> Mint(ж<@string> source)
    {
        return new StringHeaderBox<TDst>(source);
    }

    private readonly ж<@string> m_source;
    private TDst m_value = default!;
    private TDst m_handedOut = default!;
    private bool m_materialized;
    private @string m_pointerFor;        // the string the cached pointer was minted for
    private object? m_pointer;
    private bool m_held;                 // L2, as SliceHeaderBox's
    private @string m_heldOver;

    private StringHeaderBox(ж<@string> source)
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
    // Never an address: the value is materialized on demand, as SliceHeaderBox's is.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <summary>The header is the string variable seen through another type, so it orders and compares as that variable's box.</summary>
    public override nuint PointerOrderToken => m_source.PointerOrderToken;

    internal override ulong AllocationId => m_source.AllocationId;

    public override bool Equals(ж<TDst>? other)
    {
        return other is StringHeaderBox<TDst> header && ReferenceEquals(header.m_source, m_source);
    }

    public override int GetHashCode()
    {
        return m_source.GetHashCode();
    }

    /// <summary>
    /// Commits the header's words to the string variable when they name a string Go could build, and holds them (L2)
    /// when they do not yet: a nil Data with Len 0 is the empty string; Data at byte i of a managed backing with
    /// <c>0 &lt;= Len &lt;= len(backing) - i</c> is the string over those bytes, sharing them; a native Data is a copy
    /// of Len bytes there. Any other Data is refused by name (L3).
    /// </summary>
    public override void OnHeaderStore()
    {
        object boxed = m_value!;
        object? data = s_fields![0].GetValue(boxed);
        nint len = (nint)s_fields[1].GetValue(boxed)!;

        HeaderData.Kind kind = HeaderData.Resolve(data, out Array? backing, out nint index, out nuint address);
        @string? rebuilt = null;

        switch (kind)
        {
            case HeaderData.Kind.Nil:
            case HeaderData.Kind.ZeroBase:
                if (len == 0)
                    rebuilt = default(@string);
                break;

            case HeaderData.Kind.Run:
                if (backing is not byte[] bytes)
                    throw Refusal(data!, $"its Data names an element of a {backing!.GetType().Name}");

                if (0 <= len && len <= bytes.Length - index)
                    rebuilt = @string.AliasOf(new slice<byte>(bytes, index, index + len));
                break;

            case HeaderData.Kind.Native:
                if (len >= 0)
                {
                    unsafe
                    {
                        rebuilt = new @string(new ReadOnlySpan<byte>((void*)address, (int)len));
                    }
                }
                break;

            default:
                throw Refusal(data!, "its Data names neither a byte of a managed backing nor native memory");
        }

        if (rebuilt is not { } value)
        {
            m_held = true;
            m_heldOver = m_source.Value;
            m_handedOut = m_value;
            return;
        }

        m_source.Value = value;
        m_held = false;
        m_pointer = data;
        m_pointerFor = value;
        m_handedOut = m_value;
    }

    // L3: the store is dropped, as SliceHeaderBox's is.
    private PanicException Refusal(object data, string reason)
    {
        m_materialized = false;
        m_held = false;

        return new PanicException($"string header not committed: a {typeof(TDst).Name} stored over a string names no string the managed runtime can build, because {reason} ({data.GetType().Name})");
    }

    private void Materialize()
    {
        if (m_materialized && !EqualityComparer<TDst>.Default.Equals(m_value, m_handedOut))
            throw new PanicException($"string header written through a reinterpretation: a {typeof(TDst).Name} built over a string was assigned through the header without its generated setter, so the write landed on a detached copy and was never committed to the string (a store through &hdr.field or a ref to the field; see StringHeaderBox, limit L1)");

        @string source = m_source.Value;

        if (m_held)
        {
            if (SameWindow(source, m_heldOver))
                return;

            m_held = false;
        }

        if (m_pointer is null || !SameWindow(source, m_pointerFor))
        {
            object? elementZero = source.Length == 0 ? null : builtin.Ꮡ(source.UnsafeBackingWindow(), 0);
            m_pointer = s_fromBox!.Invoke(null, [elementZero]);
            m_pointerFor = source;
        }

        object boxed = default(TDst)!;
        s_fields![0].SetValue(boxed, m_pointer);
        s_fields[1].SetValue(boxed, (nint)source.Length);

        m_value = (TDst)boxed;
        m_handedOut = m_value;
        m_materialized = true;
    }

    // The same window over the same backing: Go's two words, compared as words rather than as bytes.
    private static bool SameWindow(@string left, @string right)
    {
        return left.SameWindowAs(right);
    }
}
