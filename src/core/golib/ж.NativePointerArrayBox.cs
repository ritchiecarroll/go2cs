// ж.NativePointerArrayBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

/// <summary>
/// A Go <c>*[N]*E</c> over FOREIGN memory -- native memory the Go runtime did not allocate -- whose elements are
/// the machine words in that memory, each read as the foreign address it is.
/// </summary>
/// <remarks>
/// <para>
/// WHY (G's ruling, 2026-10-19). A pointer element carries a managed reference, so
/// <see cref="NativeArrayBox{T}"/> cannot alias it, and before this kind every such array took
/// <see cref="ShadowArrayBox{T}"/>: a zeroed managed store that never reads the block. That is right for the
/// runtime's own OS memory, whose pointer words are Go pointers the runtime stored (<see cref="GoOsMemory"/>
/// keeps those in a shadow), and wrong for memory a system call filled: Windows' DNS_TXT_DATAW
/// <c>pStringArray</c>, which Go reads as <c>(*[1 &lt;&lt; 10]*uint16)(unsafe.Pointer(&amp;d.StringArray[0]))[:n:n]</c>,
/// read as n nils. Go's cgo rules forbid a Go pointer in memory the runtime did not allocate, so every pointer
/// word there is a foreign address, and element i is minted as a <see cref="NativeBox{E}"/> over the word at
/// <c>address + i * ptrSize</c> (nil for zero).
/// </para>
/// <para>
/// THE ROUTES, and what each reads. A word is read only when its element is reached, and only inside the
/// view's length, so a window Go declares far larger than the memory it names (<c>1 &lt;&lt; 10</c> for a record
/// holding two strings) reads only what Go's own code reads:
/// </para>
/// <list type="bullet">
/// <item>the element door -- <c>p.at(i)</c>, <c>ElementRef(i)</c> -- reads word i, through
/// <see cref="NativePointerWords{E}"/>, whose element pointers answer the word's own address, so
/// <c>(*[N]*E)(unsafe.Pointer(&amp;p[0]))</c> builds this kind again;</item>
/// <item>the pointer-form slice -- <c>p.slice(low, high, max)</c> -- reads words [low, max) into a managed
/// slice. A 2-index or 1-index form reads [low, high) and has capacity <c>high - low</c>, not Go's
/// <c>N - low</c>: N here is usually a bound no memory backs, and the slice's elements are a managed copy.</item>
/// <item><see cref="Value"/> REFUSES by name: an <c>array&lt;T&gt;</c> is a whole managed <c>T[]</c>, so serving it
/// would read all N words. Every runtime arena spelling is <c>p.Value[i]</c>, so an arena route that missed
/// its registration in <see cref="GoOsMemory"/> fails here, loudly, rather than reading its own Go pointers
/// as foreign words.</item>
/// </list>
/// <para>
/// STORES are refused by name (G's ruling, 2026-10-19). The memory is its producer's and immutable until the
/// producer releases it, and Go forbids a Go pointer in it. A C# <c>ref</c> cannot observe a store, so every value
/// minted -- into an element slot, or into the managed copy a slice reads -- is kept in <see cref="GoForeignWords"/>
/// and compared again at every later access through a foreign array over the same word and, at the latest, when
/// the producer releases the memory (<see cref="GoForeignMemory.Release"/>). A changed value is refused then; it
/// never reaches the word.
/// </para>
/// </remarks>
internal sealed class NativePointerArrayBox<E> : ж<array<ж<E>>>, INativeWordArray<ж<E>>, IGoReflectBox
{
    private readonly nuint m_nativeAddr;
    private readonly NativePointerWords<E> m_words;

    private NativePointerArrayBox(nuint nativeAddress, nint length)
        : base(isNull: false)
    {
        m_nativeAddr = nativeAddress;
        m_words = new NativePointerWords<E>(nativeAddress, length);

        AllocationCounter.Count();
    }

    /// <summary>
    /// The creation door: a pointer to <paramref name="length"/> foreign pointer words at
    /// <paramref name="nativeAddress"/>. A zero address is the nil pointer; a negative length is refused by name.
    /// </summary>
    internal static ж<array<ж<E>>> Over(nuint nativeAddress, nint length)
    {
        if (length < 0)
            throw new PanicException($"go2cs: native pointer array: negative length {length}");

        return nativeAddress == 0 ? NilBox : new NativePointerArrayBox<E>(nativeAddress, length);
    }

    /// <inheritdoc/>
    internal override IArray? TryGetNativeArrayView(Type elementType) =>
        elementType == typeof(ж<E>) ? m_words : null;

    /// <inheritdoc/>
    public override ref array<ж<E>> Value => throw noMaterializableArray();

    /// <inheritdoc/>
    public override ref array<ж<E>> ValueSlot => throw noMaterializableArray();

    // reflect's read of the pointee, `*p` (Value.Elem, then Index, Len or Interface on the array): Go's by-value copy
    // of the array, which reads every word (m_words.Clone). The slot itself still refuses: no array<T> aliases these
    // words, and a store goes through an element door. Every other reflect face is ж<T>'s: an interface
    // re-implementation maps the rest to the base's members.
    object? IGoReflectBox.ReadBoxSlot() => m_words.Clone();

    // reflect's store of the pointee, `*p = arr` (Value.Elem().Set): Go's array assignment, N element stores. Each goes
    // through p[i]'s element door, so the whole store behaves exactly as `p[i] = arr[i]` for every i, refusals included:
    // a value that differs from the word is refused by name at the next access, and never reaches the word.
    void IGoReflectBox.WriteBoxSlot(object? value)
    {
        array<ж<E>> source = (array<ж<E>>)value!;

        for (nint i = 0; i < source.Length; i++)
            m_words[i] = source[i];
    }

    private PanicException noMaterializableArray() =>
        new($"go2cs: native pointer array at 0x{m_nativeAddr:x}: a *[{m_words.Length}]*{typeof(E).Name} over memory the Go runtime " +
            "did not allocate has no array<T> to dereference (serving one would read every word); read its elements " +
            "through p[i]'s element door or a slice of it, and if this memory is the runtime's own, its allocation " +
            "missed registration in GoOsMemory");

    /// <inheritdoc/>
    public override nuint NativeAddress => m_nativeAddr;

    /// <inheritdoc/>
    // Native memory is not managed storage, answered first by the conversion operators (NativeAddress), as for
    // NativeArrayBox.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <inheritdoc/>
    public override nuint PointerOrderToken => m_nativeAddr;

    nint INativeWordArray<ж<E>>.Length => m_words.Length;

    ref ж<E> INativeWordArray<ж<E>>.ElementRef(nint index) => ref m_words[index];

    slice<ж<E>> INativeWordArray<ж<E>>.Window(nint low, nint high, nint max) => m_words.Window(low, high, max);

    /// <inheritdoc/>
    public override bool Equals(ж<array<ж<E>>>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other is NativePointerArrayBox<E> words && m_nativeAddr == words.m_nativeAddr;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => m_nativeAddr.GetHashCode();
}

/// <summary>
/// A pointer to array whose elements are words in native memory, read one at a time: the element door and
/// the pointer-form slice consult it before dereferencing the array (see <see cref="NativePointerArrayBox{E}"/>).
/// </summary>
internal interface INativeWordArray<T>
{
    /// <summary>The array's N.</summary>
    nint Length { get; }

    /// <summary>The element at <paramref name="index"/>, read from its word (Go's <c>p[i]</c>).</summary>
    ref T ElementRef(nint index);

    /// <summary>Go's <c>p[low:high:max]</c>: words [low, max) read into a managed slice.</summary>
    slice<T> Window(nint low, nint high, nint max);
}

/// <summary>An element view whose element i lives at a native address (an element pointer's identity).</summary>
internal interface INativeWordElements
{
    /// <summary>The native address of element <paramref name="index"/>.</summary>
    nuint ElementAddress(nint index);
}

/// <summary>
/// The element view of a <see cref="NativePointerArrayBox{E}"/>: element i is the foreign pointer in the word at
/// <c>address + i * ptrSize</c>, read when the element is reached (see that class's remarks for the routes and
/// for stores). A by-value copy (<c>*p</c>) and a range over it read the words afresh: what they hand out is a Go
/// copy, which no store through it reaches the memory from, in Go as here.
/// </summary>
internal sealed class NativePointerWords<E> : IArray<ж<E>>, INativeWordArray<ж<E>>, INativeWordElements
{
    private readonly nuint m_address;
    private readonly nint m_length;

    // One slot per element reached: the ref the element door hands out, and what it minted from which word.
    private readonly Dictionary<nint, Slot> m_slots = [];

    private sealed class Slot : GoForeignWords.ISlot
    {
        public ж<E> Value = null!;
        public ж<E> Minted = null!;
        public nuint Word;
        public bool Read;

        public bool Changed => Read && !ReferenceEquals(Value, Minted);
    }

    internal NativePointerWords(nuint address, nint length)
    {
        m_address = address;
        m_length = length;
    }

    /// <inheritdoc/>
    public nint Length => m_length;

    /// <inheritdoc/>
    public nuint ElementAddress(nint index) => m_address + (nuint)index * (nuint)Unsafe.SizeOf<nuint>();

    /// <inheritdoc/>
    public ref ж<E> this[nint index]
    {
        get
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            nuint address = ElementAddress(index);

            lock (m_slots)
            {
                // A store through any value minted from this word -- this slot's included -- is refused before the
                // word is read again.
                GoForeignWords.Verify(address, address + (nuint)Unsafe.SizeOf<nuint>());

                if (!m_slots.TryGetValue(index, out Slot? slot))
                {
                    m_slots.Add(index, slot = new Slot());
                    GoForeignWords.Minted(address, slot);
                }

                nuint word = readWord(index);

                if (!slot.Read || word != slot.Word)
                {
                    slot.Minted = mint(word);
                    slot.Value = slot.Minted;
                    slot.Word = word;
                    slot.Read = true;
                }

                return ref slot.Value;
            }
        }
    }

    object? IArray.this[nint index]
    {
        get => this[index];
        set
        {
            if (index < 0 || index >= m_length)
                throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

            throw GoForeignWords.StoreRefused(ElementAddress(index), "the array's element setter");
        }
    }

    /// <inheritdoc/>
    public ref ж<E> ElementRef(nint index) => ref this[index];

    /// <summary>Go's <c>p[low:high:max]</c> over the words: [low, max) read into a fresh managed slice.</summary>
    public slice<ж<E>> Window(nint low, nint high, nint max)
    {
        if (low < 0 || high < low || max < high || max > m_length)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(low, high, max, m_length);

        GoForeignWords.Verify(ElementAddress(low), ElementAddress(max));

        ж<E>[] elements = new ж<E>[max - low];

        for (nint i = low; i < max; i++)
        {
            elements[i - low] = mint(readWord(i));
            GoForeignWords.Minted(ElementAddress(i), elements, (int)(i - low));
        }

        return new slice<ж<E>>(elements, 0, high - low, max - low);
    }

    private unsafe nuint readWord(nint index) => *(nuint*)ElementAddress(index);

    private static ж<E> mint(nuint word) => word == 0 ? null! : new NativeBox<E>(word);

    // No managed T[] backs these elements: the members that would hand one out refuse by name.
    private InvalidOperationException noManagedBacking() =>
        new($"native pointer array at 0x{m_address:x}: its elements are words in native memory, with no managed backing");

    /// <inheritdoc/>
    public ж<E>[] Source => throw noManagedBacking();

    Array? IArray.Source => throw noManagedBacking();

    /// <inheritdoc/>
    public Span<ж<E>> ꓸꓸꓸ => throw noManagedBacking();

    /// <inheritdoc/>
    public Span<ж<E>> ToSpan() => throw noManagedBacking();

    /// <inheritdoc/>
    // Go's by-value copy of the array, `*p`, reads every element.
    public object Clone()
    {
        GoForeignWords.Verify(ElementAddress(0), ElementAddress(m_length));

        ж<E>[] elements = new ж<E>[m_length];

        for (nint i = 0; i < m_length; i++)
            elements[i] = mint(readWord(i));

        return new array<ж<E>>(elements);
    }

    /// <inheritdoc/>
    // Go's `for i, v := range *p` reads every element, in order.
    public IEnumerator<(nint, ж<E>)> GetEnumerator()
    {
        GoForeignWords.Verify(ElementAddress(0), ElementAddress(m_length));

        for (nint i = 0; i < m_length; i++)
            yield return (i, mint(readWord(i)));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
