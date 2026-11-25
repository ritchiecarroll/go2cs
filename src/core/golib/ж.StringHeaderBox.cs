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
/// stringStruct. Or a <c>uintptr</c> and an <c>nint</c>, <c>reflect.StringHeader</c>: the NUMBER shape, whose Data
/// follows Go's rodata-or-heap split (see <c>s_dataIsNumber</c>; docs/phase4/DESIGN-sqlite-ccgo-ts.md). The length may
/// also be an UNSIGNED <c>uint</c> (<c>nuint</c>): github.com/zeebo/xxh3's <c>str{p unsafe.Pointer; l uint}</c>, the
/// same two words with Len read and written as Go's uint (see <c>s_lenIsUnsigned</c>; C45).
/// </para>
/// </remarks>
internal sealed class StringHeaderBox<TDst> : ж<TDst>, IHeaderDataCarrier
{
    private static readonly FieldInfo[]? s_fields;      // [0] pointer, [1] len — declaration order
    private static readonly MethodInfo? s_fromBox;      // <pointer type>.FromBoxObject(object box) → pointer object

    /// <summary>Whether <typeparamref name="TDst"/> is a string header this box serves.</summary>
    internal static readonly bool Applies;

    // The pointer type is unsafe.Pointer, found by name as SliceHeaderBox finds it; see there.
    [System.Diagnostics.CodeAnalysis.DynamicDependency("FromBoxObject", "go.unsafe_package.Pointer", "go.unsafe")]
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

        // reflect.StringHeader's Data is a NUMBER (uintptr); see s_dataIsNumber.
        bool number = pointerType == typeof(uintptr);

        // Go's int (nint), or an unsigned uint (nuint): the same word, Len's sign aside (C45, zeebo/xxh3's header).
        bool unsignedLen = fields[1].FieldType == typeof(nuint);

        if (fields[1].FieldType != typeof(nint) && !unsignedLen)
            return;

        MethodInfo? fromBox = null;

        if (!number)
        {
            if (!pointerType.IsClass || !typeof(IUnsafePointer).IsAssignableFrom(pointerType))
                return;

            fromBox = pointerType.GetMethod("FromBoxObject", BindingFlags.Public | BindingFlags.Static, [typeof(object)]);

            if (fromBox is null || !pointerType.IsAssignableFrom(fromBox.ReturnType))
                return;
        }

        s_fields = fields;
        s_fromBox = fromBox;
        s_dataIsNumber = number;
        s_lenIsUnsigned = unsignedLen;
        Applies = true;
    }

    // Whether the header's Data is a uintptr (reflect.StringHeader) rather than the unsafe.Pointer class.
    //
    // THE NUMBER SHAPE (docs/phase4/DESIGN-sqlite-ccgo-ts.md, section 10 as amended by 10.1-10.3). Go keeps a string
    // literal's bytes in read-only data, and every other string's bytes in the heap, and the number follows that split:
    //
    //   (L) A header over PACKAGE-LEVEL storage (StaticBox<T>, IPackageLevelStorage) reads the base of an IMMORTAL native
    //       copy of the backing, plus the window's offset (StringLiteralCopies). ccgo's `__ccgo_ts` is this case: taken
    //       once at init and read at `__ccgo_ts + N` for the program's life, which a managed address cannot outlive.
    //   (H) Every other header reads the number of the backing's element at the window's start through that element
    //       box's pin-only hold, as &b[0] does: live bytes, Go memory at the cgo door, StringData == Data as in Go. The
    //       number stays valid while the ELEMENT BOX lives, not while the string does; this box keeps it, and
    //       unsafe.Pointer(h.Data) carries it (IHeaderDataCarrier).
    //   QW-9: an (H) read whose backing already has an (L) copy takes that copy, so one literal has one number.
    //   F4: an empty window reads 0, before any table; under (L) a zero-length window of a non-empty backing reads base
    //       + offset, as Go keeps it; under (H) it has no element to box and reads 0 (disclosed, W5b).
    //   A STORED word keeps the box that answered it (P3's fixes 1 and 2), so after `h2.Data = h1.Data` the array stays
    //   PINNED while h2's header box lives; for a header over package-level storage that is one pinned array for the
    //   process, per header. Bounded by the headers, not by the reads.
    private static readonly bool s_dataIsNumber;

    // Whether the header's Len is a uint (nuint) rather than Go's int (nint). A Len past nint's range is not a length any
    // string has, so it reads as negative and is refused as an inconsistent header would be (L2).
    private static readonly bool s_lenIsUnsigned;

    // The resolved shape, for the slice-origin header (SliceStringHeaderBox), which serves the same header types.
    internal static FieldInfo[]? Fields => s_fields;
    internal static MethodInfo? FromBoxMethod => s_fromBox;
    internal static bool DataIsNumber => s_dataIsNumber;
    internal static bool LenIsUnsigned => s_lenIsUnsigned;

    internal static ж<TDst> Mint(ж<@string> source)
    {
        return new StringHeaderBox<TDst>(source);
    }

    private readonly ж<@string> m_source;
    private TDst m_value = default!;
    private TDst m_handedOut = default!;
    private bool m_materialized;
    private @string m_pointerFor;        // the string the cached pointer was minted for
    private object? m_pointer;           // the pointer object; for the number shape under (H), the element box itself
    private nuint m_number;              // the number shape's Data word now
    private nuint m_mintNumber;          // the number m_pointer (an element box) was minted for, or 0
    private bool m_wordFromStore;        // m_number is a word the program STORED (kept while the window is unchanged)
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
        object lenWord = s_fields[1].GetValue(boxed)!;
        nint len = s_lenIsUnsigned ? unchecked((nint)(nuint)lenWord) : (nint)lenWord;

        // A number Data (D5) resolves through the registry, or through the element box this header minted it from. An (L)
        // copy's number is native memory, so a store of it rebuilds the string as a copy.
        nuint word = s_dataIsNumber ? ((uintptr)data!).Value : 0;
        object? carried = s_dataIsNumber && word != 0 && m_pointer is not null && word == m_mintNumber ? m_pointer : null;
        Array? backing;
        nint index;
        nuint address = 0;

        // What the header keeps once the word commits, and the number that box was minted for.
        object? keep = carried;
        nuint keepMint = carried is null ? 0 : m_mintNumber;

        HeaderData.Kind kind;

        if (s_dataIsNumber && carried is null && HeaderData.TryInterior(m_pointer, m_mintNumber, word, out backing, out index))
        {
            // Fix 1: a word inside the run this header minted from (`Data += k`) is that backing's byte, managed and
            // shared, and the minted box keeps the run pinned.
            kind = HeaderData.Kind.Run;
            keep = m_pointer;
            keepMint = m_mintNumber;
        }
        else if (s_dataIsNumber)
        {
            // Fix 2: the box the registry answered with is KEPT, so the pin behind the word lives with this header.
            kind = HeaderData.ResolveNumber(word, carried, out backing, out index, out address, out object? referent);
            keep = referent;
            keepMint = referent is null ? 0 : word;
        }
        else
        {
            kind = HeaderData.Resolve(data, out backing, out index, out address);
        }

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

        // The header keeps the Data it was given while the window it built is unchanged, as Go's header word does: a
        // later Len-only store commits from it (P3's point 4). For the number shape that is the stored word, with the
        // element box it was minted from when it is this header's own.
        if (s_dataIsNumber)
        {
            m_number = word;
            m_pointer = keep;
            m_mintNumber = keepMint;
            m_wordFromStore = true;
        }
        else
        {
            m_pointer = data;
        }

        m_pointerFor = value;
        m_handedOut = m_value;
    }

    /// <inheritdoc/>
    public nuint HeaderDataWord(out object? carried)
    {
        object data = s_fields![0].GetValue(Value)!;

        if (!s_dataIsNumber)
        {
            // The unsafe.Pointer shape: its pointer object already carries its box.
            carried = (data as IUnsafePointer)?.RetainedSource;
            return data is ж<uintptr> { IsNull: false } word ? word.Value.Value : 0;
        }

        nuint number = ((uintptr)data).Value;

        // The kept box carries an exact word, and a word inside the run it pins (fix 1).
        carried = number != 0 && m_pointer is not null && (number == m_mintNumber || HeaderData.TryInterior(m_pointer, m_mintNumber, number, out _, out _)) ? m_pointer : null;
        return number;
    }

    // The number shape's Data for the string now: (L), QW-9, F4 and (H), in that order. elementBox is set only under (H).
    private nuint NumberFor(@string source, out object? elementBox)
    {
        elementBox = null;

        slice<byte> window = source.UnsafeBackingWindow();
        byte[] backing = window.m_array;
        nuint offset = (nuint)window.Low;

        // F4: an empty backing is the empty string's, and reads 0 before any table.
        if (backing.Length == 0)
            return 0;

        // (L): package-level storage reads its immortal copy, validated on every read (R-a), a zero-length window
        // included.
        if (m_source is IPackageLevelStorage)
            return StringLiteralCopies.BaseOf(backing) + offset;

        // QW-9: a backing that already has an (L) copy, however it is reached, reads that copy, validated too.
        if (StringLiteralCopies.TryExisting(backing, out nuint @base))
            return @base + offset;

        // (H) over a zero-length window: there is no element to box (W5b).
        if (source.Length == 0)
            return 0;

        // (H): the element at the window's start, through its pin-only hold, reusing the box minted for this window.
        if (m_pointer is not null && m_mintNumber != 0 && SameWindow(source, m_pointerFor))
        {
            elementBox = m_pointer;
            return m_mintNumber;
        }

        // A static backing (a literal, a one-byte string) reads unsafe.StringData's private copy, so Data equals
        // StringData(s) as in Go (ReadOnlyStringBacking).
        ж<byte> element = ReadOnlyStringBacking.TryDataBox(window) ?? builtin.Ꮡ(window, 0);
        elementBox = element;
        return ((INilPointer)element).StableAddress();
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

        object boxed = default(TDst)!;

        if (s_dataIsNumber)
        {
            // A word the program stored is kept while the window it built is unchanged. Otherwise the number is derived
            // on every access: (L) and QW-9 validate the copy on each read (R-a), and (H) reuses its element box while
            // the window is unchanged.
            if (!m_wordFromStore || !SameWindow(source, m_pointerFor))
            {
                m_wordFromStore = false;
                m_number = NumberFor(source, out object? elementBox);
                m_pointer = elementBox;
                m_mintNumber = elementBox is null ? 0 : m_number;
                m_pointerFor = source;
            }

            s_fields![0].SetValue(boxed, (uintptr)m_number);
        }
        else
        {
            if (m_pointer is null || !SameWindow(source, m_pointerFor))
            {
                slice<byte> window = source.UnsafeBackingWindow();
                object? elementZero = source.Length == 0 ? null : ReadOnlyStringBacking.TryDataBox(window) ?? builtin.Ꮡ(window, 0);
                m_pointer = s_fromBox!.Invoke(null, [elementZero]);
                m_pointerFor = source;
            }

            s_fields![0].SetValue(boxed, m_pointer);
        }

        s_fields[1].SetValue(boxed, s_lenIsUnsigned ? (object)(nuint)source.Length : (nint)source.Length);

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
