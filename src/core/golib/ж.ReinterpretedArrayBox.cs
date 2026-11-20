// ж.ReinterpretedArrayBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

/// <summary>
/// A pointer to a FIXED-SIZE ARRAY whose elements are the bytes of ONE FIELD of a Go value that has no address:
/// Go's <c>(*[N]E)(unsafe.Pointer(&amp;x.f))</c>, f not an E, where x's converted struct holds a reference
/// (docs/phase4/DESIGN-array-pointer-doors.md, section 19).
/// </summary>
/// <remarks>
/// <para>
/// A struct that holds a reference cannot be pinned, so nothing holds the field's bytes still and no native array
/// pointer can be made over them (<see cref="NativeArrayBox{T}"/> needs an address that stays). The field's own
/// pointer already answers "my pointee's storage as bytes", recomputed from the live box on each access
/// (<see cref="IReinterpretSource.RawSlot"/>), which is the read <c>PointerExtensions.Reinterpret</c> takes for one
/// value. This kind is the same read for N elements: element i is byte <c>i x size(E)</c> of the field, read as E.
/// </para>
/// <para>
/// So a read sees the field as it is at that moment and a store writes it: no copy exists to lose a write, the
/// field's own accesses (<c>x.f = v</c>) and the view's see each other at once, and the collector may move the
/// struct between any two accesses. Byte order is the host's, as Go's is.
/// </para>
/// <para>
/// It is made only when the field's type holds no reference, E holds none, and the view fits inside the field
/// (<c>N x size(E) &lt;= size(f)</c>). The fields after f have no address to read, so a longer view is refused by
/// name where it is made; a field whose converted form holds a reference is not N contiguous bytes and is refused
/// the same way.
/// </para>
/// <para>
/// There is no <c>array&lt;E&gt;</c> behind it, so <see cref="Value"/> refuses by name, as the native kinds do.
/// The elements are reached through the element door (<c>PointerExtensions.ElementRef</c>) and the whole array
/// through <see cref="Load"/> and <see cref="Store"/>. A slice of it, a range over its values and the address of
/// one element need storage a slice or a pointer can name, and meet the same refusal.
/// </para>
/// <para>
/// It has no storage and no identity of its own: its order token, allocation, referent, equality, hash and number
/// are the FIELD's pointer's (<see cref="IPointerNumberForwarder"/>), so two views of one field are one Go
/// pointer, and <c>unsafe.Pointer(p) == unsafe.Pointer(&amp;x.f)</c> as in Go. It is never nil.
/// </para>
/// </remarks>
internal sealed class ReinterpretedArrayBox<E> : ж<array<E>>, IGoReflectBox, IPointerNumberForwarder, INativeArrayStore<E>
{
    // The field's own pointer, the `x.Ꮡf` the converter passes: every element read goes through its live slot.
    private readonly IReinterpretSource m_field;

    // The N of Go's *[N]E. N x size(E) is no more than the field's size (TryOver), so every element is inside it.
    private readonly nint m_length;

    private ReinterpretedArrayBox(IReinterpretSource field, nint length)
        : base(isNull: false)
    {
        m_field = field;
        m_length = length;

        // The box only: the bytes are the field's.
        AllocationCounter.Count();
    }

    /// <summary>
    /// The single creation door: the view of <paramref name="length"/> elements over the field
    /// <paramref name="field"/> points to, or null with the reason in <paramref name="refusal"/>, worded to follow
    /// the Go form the caller names. A nil field pointer answers the nil pointer.
    /// </summary>
    internal static ж<array<E>>? TryOver(object? field, nint length, out string? refusal)
    {
        refusal = null;

        if (field is INilPointer { IsNilPointer: true })
            return NilBox;

        if (field is not IReinterpretSource source)
        {
            refusal = "the field's pointer is not a pointer box, so its bytes cannot be viewed as an array";
            return null;
        }

        if (length < 0)
        {
            refusal = $"negative length {length}";
            return null;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<E>())
        {
            refusal = "an element that holds references cannot be viewed over a field's bytes";
            return null;
        }

        if (source.PointeeContainsReferences)
        {
            refusal = $"the field is a {GoReflect.GoTypeName(source.PointeeType)}, whose converted form holds a reference, so it is not contiguous bytes that can be viewed as an array";
            return null;
        }

        long bytes = (long)length * Unsafe.SizeOf<E>();

        if (bytes > source.PointeeSize)
        {
            refusal = $"the view is {bytes} bytes and the field holds {source.PointeeSize}; the field is in a Go value that has no address (a struct that holds references has none), so the fields after it have no address to read";
            return null;
        }

        return new ReinterpretedArrayBox<E>(source, length);
    }

    /// <inheritdoc/>
    public override ref array<E> Value => throw noMaterializableArray();

    /// <inheritdoc/>
    public override ref array<E> ValueSlot => throw noMaterializableArray();

    private PanicException noMaterializableArray() =>
        new($"go2cs: a *[{m_length}]{GoReflect.GoTypeName(typeof(E))} over a field of a Go value that has no address " +
            "(a struct that holds references has none) has no array<T> to dereference: its elements are the field's " +
            "own bytes. Index it (p[i]), load it (*p) or store it (*p = v); a slice of it, a range over its values " +
            "and the address of an element need storage that can be named, which this value does not have");

    /// <summary>The element count this view was made with.</summary>
    internal nint Length => m_length;

    /// <summary>
    /// The element at <paramref name="index"/>, in place in the field: Go's <c>p[i]</c>. Bounded by N and
    /// panicking with Go's message, as <c>array&lt;T&gt;</c>'s own indexer does. The field's slot is read again on
    /// every call, so the reference is to where the field is now.
    /// </summary>
    internal ref E ElementRef(nint index)
    {
        if (index < 0 || index >= m_length)
            throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

        return ref Unsafe.Add(ref Unsafe.As<byte, E>(ref m_field.RawSlot), index);
    }

    /// <inheritdoc cref="ElementRef(nint)"/>
    // An UNSIGNED index is checked before any narrowing, as array<T>'s ulong indexer does.
    internal ref E ElementRef(ulong index)
    {
        if (index >= (ulong)m_length)
            throw RuntimeErrorPanic.IndexOutOfRange(index, m_length);

        return ref ElementRef((nint)index);
    }

    /// <summary>Go's <c>v := *p</c>: the N elements, copied, as a Go array value is.</summary>
    internal array<E> Load()
    {
        array<E> copy = new(m_length);

        for (nint i = 0; i < m_length; i++)
            copy[i] = ElementRef(i);

        return copy;
    }

    /// <summary>
    /// Go's <c>*p = v</c>: N element stores, in place. A value shorter than N (the zero value of an array that was
    /// never given storage) is zeros from where it ends, as Go's is.
    /// </summary>
    internal void Store(in array<E> value)
    {
        nint given = value.Length;

        for (nint i = 0; i < m_length; i++)
            ElementRef(i) = i < given ? value[i] : default!;
    }

    // reflect's read and store of the pointee, `*p` and `*p = arr`: the same two, as for the native kinds.
    object? IGoReflectBox.ReadBoxSlot() => Load();

    object? IGoReflectBox.ReadBoxSlotPublished() => Load();

    void IGoReflectBox.WriteBoxSlot(object? value) => Store((array<E>)value!);

    // Go's `*p = v` reached through ArrayExtensions.Assign, which the converter emits for the tuple and range stores
    // (`*p, n = v, 9`, `for _, *p = range vs`): the same store, since Value has no array<E> to assign into.
    void INativeArrayStore<E>.StoreElements(in array<E> source) => Store(source);

    // The view's number is the field's (see IPointerNumberForwarder): the transient address of the field's slot for
    // unsafe.Pointer's carried mint, and the field's own conversion for a syscall argument word.
    nuint IGoReflectBox.TransientSlotAddress() => ((IGoReflectBox)m_field).TransientSlotAddress();

    nuint IGoReflectBox.NativeArgumentAddress() => ((IGoReflectBox)m_field).NativeArgumentAddress();

    nuint IPointerNumberForwarder.ForwardedNumber() => ((INilPointer)m_field).StableAddress();

    /// <inheritdoc/>
    // No storage of its own: both conversion operators answer the field's number before they read this
    // (IPointerNumberForwarder), so nothing is pinned or registered in this box's name.
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <inheritdoc/>
    // The field's token: allocation base plus the field's Go offset. Go's `unsafe.Pointer(p)` is the field's address.
    public override nuint PointerOrderToken => ((INilPointer)m_field).PointerOrderToken;

    /// <inheritdoc/>
    internal override ulong AllocationId => ((IAllocationIdentity)m_field).AllocationId;

    /// <inheritdoc/>
    public override object ReferentObject => ((INilPointer)m_field).ReferentObject;

    /// <inheritdoc/>
    // Two views are one Go pointer exactly when their fields' pointers are: the field pointer's own rule (two
    // pointers to one field of one allocation are equal, to two fields or two allocations are not).
    public override bool Equals(ж<array<E>>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other is ReinterpretedArrayBox<E> view && m_field.Equals(view.m_field);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => m_field.GetHashCode();
}

/// <summary>
/// A pointer box that has no storage of its own and answers ANOTHER box's number: a view of a field's bytes
/// (<see cref="ReinterpretedArrayBox{E}"/>) answers the field's. The pointer-to-number operators ask this before
/// they read the box's slot, which such a box refuses, and unsafe.Pointer's carried mint asks it for the same
/// reason. Non-generic, so the test is one type check and no generic virtual method exists.
/// </summary>
internal interface IPointerNumberForwarder
{
    /// <summary>The number of the box this one views, minted by that box's own conversion.</summary>
    nuint ForwardedNumber();
}
