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
/// WRITES (the unsafeheader write-through seat, 2026-10-21). A <c>ref</c> cannot be intercepted, and the header's LAST
/// store (<c>hdr.Len = len(s)</c> in internal/unsafeheader's TestWriteThroughHeader) is followed by no access of the box
/// at all, so a materializing box could never observe the finished header. go2cs-gen's FieldAccessGenerator therefore
/// gives a HEADER-SHAPED struct's fields GET/SET accessors on <c>ж&lt;H&gt;</c> rather than <c>ref</c> ones, and the
/// setter calls <see cref="ж{T}.OnHeaderStore"/> after the store. Here that commits the header's words into the slice
/// variable whenever they name a slice Go could build (<see cref="OnHeaderStore"/> lists the four), so every step of a
/// field-by-field build reaches the variable in Go's order. Three limits, stated rather than hidden:
/// <list type="bullet">
/// <item>(L1) A store that bypasses the setter, through <c>&amp;hdr.Len</c> or a <c>ref</c> to the field, commits
/// nothing. The box compares each fresh materialization against what it last handed out and panics naming the class on
/// its NEXT access, as it did before the seat.</item>
/// <item>(L2) An INCONSISTENT header (a nil Data with a non-zero Cap, Len above Cap, a Cap past the backing) is HELD:
/// the header reads back its own words, and the variable keeps its last consistent slice until a later store makes the
/// header consistent. Go shows the broken words in the variable too; a read of the variable in between differs from
/// Go. The runtime's addrRanges stores Cap before Data, and its writers stay hand-owned at the write
/// (<c>runtime/mranges_impl.cs</c>, re-basing over <see cref="slice{T}.OverNativeMemory(nuint, nint, nint)"/>).</item>
/// <item>(L3) A Data that names neither a run of the slice's own element type nor native memory (a heap box, a field,
/// an array of another element type, an order token) is refused by name at the store, never approximated.</item>
/// </list>
/// The header→slice direction is the mirror box, <see cref="HeaderSliceBox{T, TDst}"/>.
/// </para>
/// <para>
/// SCOPE. <typeparamref name="T"/> is exactly <c>slice&lt;X&gt;</c>; <typeparamref name="TDst"/> is a
/// value type whose three instance fields are, in declaration order, a class implementing
/// <see cref="IUnsafePointer"/> (the <c>unsafe.Pointer</c> class; golib cannot name that assembly, so
/// the factory is resolved by reflection once per pair and pinned by a guard), then two <c>nint</c>s.
/// A header whose first field is a typed <c>ж&lt;…&gt;</c> (<c>notInHeapSlice</c>) is NOT admitted —
/// every corpus site that takes that view of a managed slice is a WRITER (the field-by-field family,
/// hand-owned; the whole-header family, unreached), so there is no reaching READ for it to serve. The STRING header
/// (<c>@string</c> → <c>stringStruct</c>, two fields) is its own box, <see cref="StringHeaderBox{TDst}"/>.
/// </para>
/// </remarks>
public abstract partial class ж<T>
{
    /// <summary>
    /// Called by go2cs-gen's generated field SETTER on a header-shaped struct (an <c>unsafe.Pointer</c> field, then one
    /// or two <c>int</c>s) after it stores the field. A header box commits the header's words to its source here; every
    /// other box has nothing to commit.
    /// </summary>
    public virtual void OnHeaderStore()
    {
    }
}

/// <summary>
/// What a slice or string header's <c>Data</c> word names, for the header boxes' commit: the inverse of the element-0
/// box their read side mints.
/// </summary>
internal static class HeaderData
{
    internal enum Kind
    {
        /// <summary>A nil pointer.</summary>
        Nil,

        /// <summary>An element of a managed array: <c>backing[index]</c>.</summary>
        Run,

        /// <summary>The zerobase, which Go gives a zero-capacity slice.</summary>
        ZeroBase,

        /// <summary>Native memory at <c>address</c>.</summary>
        Native,

        /// <summary>Anything else: a heap box, a field, an element the box cannot place, an order token.</summary>
        Other
    }

    internal static Kind Resolve(object? data, out Array? backing, out nint index, out nuint address)
    {
        backing = null;
        index = 0;
        address = 0;

        if (data is not ж<uintptr> word || word.IsNull)
            return Kind.Nil;

        nuint number = word.Value.Value;

        // The box the pointer was minted from (FromPinnedBox, FromBox); else whatever the token registry recovers for
        // a bare number, as the uintptr conversion does.
        object? referent = ((IUnsafePointer)data).RetainedSource ?? ManagedPointerTokens.Resolve(number);

        if (referent is IElementRun { RunBacking: { } run } element)
        {
            backing = run;
            index = element.RunIndex;
            return Kind.Run;
        }

        if (referent is IGoReflectBox face)
        {
            if (face.NativeAddress != 0)
            {
                address = face.NativeAddress;
                return Kind.Native;
            }

            return face.NamesZeroBase ? Kind.ZeroBase : Kind.Other;
        }

        // A bare number nothing recovers is an address, unless it is one of golib's order tokens, which name no memory.
        if (referent is null && !ManagedPointerTokens.IsTaggedToken(number))
        {
            address = number;
            return Kind.Native;
        }

        return Kind.Other;
    }
}

internal sealed class SliceHeaderBox<T, TDst> : ж<TDst>
{
    // ---- the shape, resolved once per (T, TDst) ----

    private static readonly FieldInfo[]? s_fields;      // [0] pointer, [1] len, [2] cap — declaration order
    private static readonly MethodInfo? s_fromBox;      // <pointer type>.FromBoxObject(object box) → pointer object
    private static readonly Func<IArray, (object? backing, nint low, nint len, nint cap)>? s_describe;
    private static readonly Func<IArray, object>? s_elementZero;
    private static readonly Func<Array, nint, nint, nint, object?>? s_rebase;        // backing[low:high:max], or null for another element
    private static readonly Func<nuint, nint, nint, object>? s_overNative;

    /// <summary>Whether the pair (<typeparamref name="T"/>, <typeparamref name="TDst"/>) is a slice → header reinterpretation this box serves.</summary>
    internal static readonly bool Applies;

    // The header's pointer type is unsafe.Pointer, the one IUnsafePointer (no generated type implements it), in an
    // assembly golib cannot reference; its FromBoxObject is found by name. The dependency keeps that method in a
    // trimmed publish (trim stage 1, docs/PLAN-golib-full-trim.md). The trimmer reads its type name as a
    // documentation ID, so the nested type is unsafe_package.Pointer, not unsafe_package+Pointer.
    [System.Diagnostics.CodeAnalysis.DynamicDependency("FromBoxObject", "go.unsafe_package.Pointer", "unsafe")]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2075",
        Justification = "pointerType is unsafe.Pointer, whose FromBoxObject the DynamicDependency on this constructor keeps.")]
    static SliceHeaderBox()
    {
        Type source = typeof(T);
        Type header = typeof(TDst);

        if (!source.IsGenericType || source.GetGenericTypeDefinition() != typeof(slice<>) || !header.IsValueType || header.IsPrimitive || header.IsEnum)
            return;

        // Declaration order is GoFieldMetadata's order (no metadata token under Native AOT; see there).
        FieldInfo[] fields = GoFieldMetadata.InstanceFields(header);

        if (fields.Length != 3)
            return;

        Type pointerType = fields[0].FieldType;

        if (!pointerType.IsClass || !typeof(IUnsafePointer).IsAssignableFrom(pointerType))
            return;

        if (fields[1].FieldType != typeof(nint) || fields[2].FieldType != typeof(nint))
            return;

        // The pointer type's own retaining factory for a box known only as an object:
        // `public static <Pointer> FromBoxObject(object box)` (unsafe.Pointer's untyped twin of FromBox<X>; trim stage
        // 3c-2a, so nothing here closes a generic method over the element). Resolved by name because the marker interface
        // is empty and golib cannot reference the assembly that defines the class; the contract is pinned by
        // SliceHeaderReinterpretTests.
        MethodInfo? fromBox = pointerType.GetMethod("FromBoxObject", BindingFlags.Public | BindingFlags.Static, [typeof(object)]);

        if (fromBox is null || !pointerType.IsAssignableFrom(fromBox.ReturnType))
            return;

        s_fields = fields;
        s_fromBox = fromBox;

        // The slice's identity words, without allocating: backing array (null for a nil slice), low, len, cap.
        s_describe = static array => ((IGoReflectSlice)array).ReflectDescribe();

        // The element-0 box at the slice's low index — Go's `s.array`. Constructed only when the words moved.
        //
        // A ZERO-CAPACITY slice names no element, and Go never gives it a pointer past the end: the
        // compiler keeps an in-bounds base when a reslice leaves cap 0, and mallocgc(0) answers the
        // runtime's zerobase. Element 0 at such a slice's low index is one past the end of its backing
        // (mem[len:len], or any make([]T, 0)), and minting the pointer read it: IndexOutOfRangeException,
        // which took runtime's TestMemclr down at MemclrBytes(mem[size:size]). So a cap-0 slice's array
        // word is a per-element-type zerobase element: non-nil, as Go's is, and never dereferenced by a
        // correct program, since there is no element to reach through it.
        //
        // Both through the slice's own face (ISlice<T>'s default members, trim stage 3c-2a): T IS the slice type here, so
        // the face is compiled for it; the box no longer closes a generic method over its element.
        s_elementZero = static array => ((IGoReflectSlice)array).ReflectElementZero();

        // The commit's two rebuilds, through a zero value of the slice type (OnHeaderStore).
        s_rebase = static (backing, low, high, max) => ((IGoReflectSlice)(object)default(T)!).ReflectRebase(backing, low, high, max);
        s_overNative = static (address, len, cap) => ((IGoReflectSlice)(object)default(T)!).ReflectOverNativeMemory(address, len, cap);
        Applies = true;
    }

    internal static ж<TDst> Mint(ж<T> source)
    {
        return new SliceHeaderBox<T, TDst>(source);
    }

    // ---- the instance ----

    private readonly ж<T> m_source;
    private TDst m_value = default!;   // the header handed out through Value (a ref into this field)
    private TDst m_handedOut = default!; // what that header held when it was handed out
    private bool m_materialized;
    private object? m_pointer;          // the cached pointer object, minted for (m_pointerBacking, m_pointerLow, cap == 0)
    private object? m_pointerBacking;
    private nint m_pointerLow = -1;
    private bool m_pointerZeroCapacity;  // a cap-0 slice names the zerobase, so cap 0 is part of the key
    private bool m_held;                 // L2: the header holds inconsistent words the slice does not show
    private (object? backing, nint low, nint len, nint cap) m_heldOver; // the slice's words when the header was held

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

    /// <inheritdoc/>
    // NEVER an address, stated here rather than inherited from a pinnability question. The value
    // is materialized on demand, so `fixed` would hand out the address of a temporary that the
    // next Materialize replaces — the address route over a header box has a recorded native
    // crash behind it. This kind must stay tokenised and now says so in its own file.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <summary>The header is the slice variable seen through another type, so it orders and compares as that variable's box.</summary>
    public override nuint PointerOrderToken => m_source.PointerOrderToken;

    internal override ulong AllocationId => m_source.AllocationId;

    public override bool Equals(ж<TDst>? other)
    {
        return other is SliceHeaderBox<T, TDst> header && ReferenceEquals(header.m_source, m_source);
    }

    public override int GetHashCode()
    {
        return m_source.GetHashCode();
    }

    /// <summary>
    /// Commits the header's words to the slice variable when they name a slice Go could build, and holds them (L2) when
    /// they do not yet:
    /// <list type="bullet">
    /// <item>a nil Data with Len and Cap 0 is the nil slice;</item>
    /// <item>Data at element i of a backing <c>X[]</c> of the slice's own element type, with
    /// <c>0 &lt;= Len &lt;= Cap &lt;= len(backing) - i</c>, is that backing's <c>[i:i+Len:i+Cap]</c>, SHARING it as Go's
    /// alias does;</item>
    /// <item>the zerobase with Len and Cap 0 is an empty non-nil slice;</item>
    /// <item>a native Data is the slice over that memory (an unmanaged element type only, as
    /// <see cref="slice{T}.OverNativeMemory(nuint, nint, nint)"/> requires).</item>
    /// </list>
    /// Any other Data is refused by name (L3).
    /// </summary>
    public override void OnHeaderStore()
    {
        object boxed = m_value!;
        object? data = s_fields![0].GetValue(boxed);
        nint len = (nint)s_fields[1].GetValue(boxed)!;
        nint cap = (nint)s_fields[2].GetValue(boxed)!;

        HeaderData.Kind kind = HeaderData.Resolve(data, out Array? backing, out nint index, out nuint address);
        bool shape = 0 <= len && len <= cap;
        object? rebuilt = null;

        switch (kind)
        {
            case HeaderData.Kind.Nil:
                if (len == 0 && cap == 0)
                    rebuilt = default(T)!;
                break;

            case HeaderData.Kind.Run:
                if (shape && cap <= backing!.Length - index)
                    rebuilt = s_rebase!(backing, index, index + len, index + cap) ?? throw Refusal(data!, $"its Data names an element of a {backing.GetType().Name}");
                else if (s_rebase!(backing!, 0, 0, 0) is null)
                    throw Refusal(data!, $"its Data names an element of a {backing!.GetType().Name}");
                break;

            case HeaderData.Kind.ZeroBase:
                if (len == 0 && cap == 0)
                    rebuilt = s_rebase!(((IElementRun)s_elementZero!((IArray)(object)default(T)!)).RunBacking!, 0, 0, 0);
                break;

            case HeaderData.Kind.Native:
                if (shape)
                    rebuilt = s_overNative!(address, len, cap);
                break;

            default:
                throw Refusal(data!, "its Data names neither an element of the slice's own element type nor native memory");
        }

        if (rebuilt is null)
        {
            // L2: hold the words; the variable keeps its last consistent slice.
            m_held = true;
            m_heldOver = s_describe!((IArray)(object)m_source.Value!);
            m_handedOut = m_value;
            return;
        }

        m_source.Value = (T)rebuilt;
        m_held = false;

        // The header already holds these words, and its Data object is the pointer the variable now names, so the next
        // access reuses it rather than minting another.
        (object? newBacking, nint newLow, _, nint newCap) = s_describe!((IArray)rebuilt);
        m_pointer = data;
        m_pointerBacking = newBacking;
        m_pointerLow = newLow;
        m_pointerZeroCapacity = newCap == 0;
        m_handedOut = m_value;
    }

    // L3. The store is dropped rather than held: the next access re-reads the variable, which the store never reached.
    private PanicException Refusal(object data, string reason)
    {
        m_materialized = false;
        m_held = false;

        return new PanicException($"slice header not committed: a {typeof(TDst).Name} stored over a {typeof(T).Name} names no slice the managed runtime can build, because {reason} ({data.GetType().Name})");
    }

    private void Materialize()
    {
        if (m_materialized && !EqualityComparer<TDst>.Default.Equals(m_value, m_handedOut))
            throw new PanicException($"slice header written through a reinterpretation: a {typeof(TDst).Name} built over a {typeof(T).Name} was assigned through the header without its generated setter, so the write landed on a detached copy and was never committed to the managed slice (a store through &hdr.field or a ref to the field; see SliceHeaderBox, limit L1)");

        (object? backing, nint low, nint len, nint cap) = s_describe!((IArray)(object)m_source.Value!);

        // A held header shows its own words until the variable moves under it: then the variable's words win, as a
        // direct assignment to a Go slice overwrites its header.
        if (m_held)
        {
            if (ReferenceEquals(backing, m_heldOver.backing) && (low, len, cap) == (m_heldOver.low, m_heldOver.len, m_heldOver.cap))
                return;

            m_held = false;
        }

        if (m_pointer is null || !ReferenceEquals(backing, m_pointerBacking) || low != m_pointerLow || (cap == 0) != m_pointerZeroCapacity)
        {
            object? elementZero = backing is null ? null : s_elementZero!((IArray)(object)m_source.Value!);
            m_pointer = s_fromBox!.Invoke(null, [elementZero]);
            m_pointerBacking = backing;
            m_pointerLow = low;
            m_pointerZeroCapacity = cap == 0;
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
