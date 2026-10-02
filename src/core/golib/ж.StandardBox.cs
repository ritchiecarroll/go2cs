// ж.StandardBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go.golib;

namespace go;

/// <summary>
/// The STANDARD pointer kind — a heap box that IS the storage it names (Go's <c>&amp;x</c> /
/// <c>new(T)</c> allocation). One of the four kinds of <see cref="ж{T}"/> under the B1 per-kind
/// split (<c>docs/phase4/DESIGN-zh-box-b1.md</c> §3); the other three are
/// <see cref="FieldRefBox{T}"/>, <see cref="ElemRefBox{T}"/> and <see cref="NativeBox{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// UNSEALED by P-F5's resolution: <c>@unsafe.Pointer</c> derives from
/// <c>StandardBox&lt;uintptr&gt;</c> — its value IS an address, so it is standard-kind storage
/// with address-identity overrides. Every OTHER kind seals.
/// </para>
/// <para>
/// The storage doctrine is unchanged from the pre-split box, moved here verbatim: a managed
/// <typeparamref name="T"/> lives inline in <see cref="m_val"/> (one object); an unmanaged
/// <typeparamref name="T"/> lives in the eager one-element pinnable <see cref="m_slot"/> (two
/// objects — the box plus the slot, and the counter charges exactly that), because Go's
/// <c>uintptr(unsafe.Pointer(&amp;x))</c> names an address that must stay valid while native code
/// uses it, and only a reference-free array can be pinned. The slot is eager and never migrated:
/// <c>heap()</c> hands out a <c>ref</c> alias to it before any address is taken.
/// </para>
/// </remarks>
public class StandardBox<T> : ж<T>
{
    // A managed T lives here (reference-containing layouts cannot be pinned, and no syscall can
    // meaningfully take their address, so no slot exists for them).
    private T m_val = default!;

    // PINNABLE storage for an unmanaged T — the ONE authority for the value when it exists (never
    // a mirror of m_val). See the storage doctrine in the class remarks.
    private readonly T[]? m_slot;

    /// <summary>
    /// Creates a new pointer to a heap allocated instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Source value for the heap allocated reference.</param>
    public StandardBox(in T value)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // One object: this box. A managed T lives in m_val, a field of the box itself.
            m_val = value;
            AllocationCounter.Count();
        }
        else
        {
            // TWO objects for Go's ONE malloc — the box, plus the one-element pinnable slot whose
            // eager allocation cannot be deferred (heap() aliases it before any address-take).
            // Leaf-ctor counting per the B1 split: the abstract base charges nothing, so every box
            // is counted exactly once, with the same per-kind charges as before the split.
            m_slot = [value];
            AllocationCounter.Count(2);
        }
    }

    /// <summary>
    /// Creates a new nil pointer box.
    /// </summary>
    public StandardBox(NilType _) : base(isNull: true)
    {
        m_val = default!;

        // Counted, even though Go's nil pointer is a word and allocates nothing: this constructor
        // really does hand the CLR heap an object, and the counter's contract is what the heap
        // received.
        AllocationCounter.Count();
    }

    // Creates a box that HOLDS value but is nonetheless the nil pointer. Needed by unsafe.Pointer,
    // whose value IS its address: `unsafe.Pointer(uintptr(0))` must compare equal to nil yet still
    // round-trip back out as 0. Protected — an ordinary box is nil or holds a value, never both.
    protected StandardBox(in T value, bool isNull) : base(isNull)
    {
        // Same two shapes, and the same charge, as the public value constructor above.
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            m_val = value;
            AllocationCounter.Count();
        }
        else
        {
            m_slot = [value];
            AllocationCounter.Count(2);
        }
    }

    /// <inheritdoc/>
    public override ref T Value
    {
        get
        {
            if (IsNull)
                throw RuntimeErrorPanic.NilPointerDereference();

            // The pinnable slot IS the storage when it exists — never a mirror of m_val, so there
            // is only ever one authority to read or write.
            return ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);
        }
    }

    /// <inheritdoc/>
    public override ref T ValueSlot =>
        ref m_slot is null ? ref m_val! : ref MemoryMarshal.GetArrayDataReference(m_slot);

    // The value-peeking IsNull, standard-kind only (see the base's doc): a real heap box whose
    // reference-typed value is null answers nil for DEREFERENCE guards. s_valueCanBeNull folds at
    // JIT time per instantiation, exactly as before the split.
    public override bool IsNull => m_isNull || (s_valueCanBeNull && HeldValueIsNull);

    // The held value, read from whichever slot actually holds it (a ж<Nullable<T>> keeps its value
    // in the pinnable slot, so peeking m_val alone answered for the wrong storage).
    private bool HeldValueIsNull => (m_slot is null ? m_val : m_slot[0]) is null;

    private static readonly bool s_valueCanBeNull =
        !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) is not null;

    /// <inheritdoc/>
    // A standard heap box IS the storage it addresses — its own identity, never the held value's
    // (a token derived from the pointee would change when the pointee is assigned, which is not
    // something an address does). Offset 0 within itself, which is also why `&s` and
    // `&s.firstField` token alike — as Go's addresses do.
    public override nuint PointerOrderToken =>
        IsNilPointer ? 0 : NamesZeroBase ? GoZeroBase.Token :
        AllocationBase(AllocationId);

    internal override ulong AllocationId
    {
        get
        {
            int id = System.Threading.Volatile.Read(ref m_id);

            if (id != 0)
                return (ulong)(uint)id;

            int fresh = (int)ManagedPointerTokens.NextIdentity();
            int prior = System.Threading.Interlocked.CompareExchange(ref m_id, fresh, 0);
            return (ulong)(uint)(prior != 0 ? prior : fresh);
        }
    }

    /// <inheritdoc/>
    // A ZERO-SIZE heap allocation is Go's zerobase: mallocgc(0) answers &zerobase for every one of
    // them, so two news of struct{} are one pointer (GoZeroBase). Nil is never the zerobase.
    internal override bool NamesZeroBase => GoZeroSizeFacts<T>.IsZeroSize && !m_isNull;

    /// <inheritdoc/>
    // The zerobase's referent is the zerobase, which runtime treats as outside every heap span.
    public override object ReferentObject => NamesZeroBase ? GoZeroBase.Box : this;

    /// <inheritdoc/>
    public override bool Equals(ж<T>? other)
    {
        if (other is null)
            return m_isNull;

        if (ReferenceEquals(this, other))
            return true;

        // A nil pointer compares equal only to another nil pointer — STRUCTURAL nil only: a real
        // heap box whose held reference-typed value is null is a NON-nil pointer holding a nil
        // value, and two such distinct boxes are distinct addresses in Go.
        if (m_isNull || other.IsNilPointer)
            return m_isNull && other.IsNilPointer;

        // Every zerobase pointer is one pointer, whichever kind names it.
        if (NamesZeroBase || other.NamesZeroBase)
            return NamesZeroBase && other.NamesZeroBase;

        // Go pointer comparison is by identity — the same storage location — never by the
        // pointed-to value (which would be wrong, and unsound: self-referential structs recurse).
        // Reference equality already answered false above; a standard box equals no other kind.
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => IsNilPointer ? 0 : NamesZeroBase ? GoZeroBase.HashCode : RuntimeHelpers.GetHashCode(this);

    /// <inheritdoc/>
    // The pinnable value slot, when T admits one — what EnsureStableAddress pins on address-take.
    public override object? PinnableStorage => m_slot;

    /// <inheritdoc/>
    // The slot is both the address and the pin: with one, the value lives in a pinnable
    // buffer; without one, it lives in m_val, a field of this box object, and its address is
    // a movable heap interior that SUB-Q42's witness measured going stale. That absence is a
    // genuine None — this is the kind the token arm was written for and it is unchanged.
    public override PointerStorage StorageKind =>
        m_slot is null ? PointerStorage.None : PointerStorage.Pinnable;

    // FieldInfo access for the contracts IL builder (ж.Contracts.cs) — the fields the split moved
    // here from the old single-class box; the builder targets THIS type now.
}

/// <summary>
/// A heap box that is a HANDLE rather than an allocation: its identity is its own even when
/// <typeparamref name="T"/> is zero-size, so it never names <see cref="GoZeroBase"/>.
/// </summary>
/// <remarks>
/// runtime's <c>*Func</c> is the case: <c>Func</c> is zero-size in Go, but a <c>*Func</c> points into
/// the pclntab at the function it describes, never at zerobase, so two functions' <c>*Func</c> are two
/// pointers. A plain <see cref="StandardBox{T}"/> of <c>Func</c> would be the zerobase, and every
/// <c>FuncForPC</c> answer would compare equal to every other.
/// </remarks>
/// <typeparam name="T">Pointee type.</typeparam>
internal sealed class HandleBox<T> : StandardBox<T>
{
    internal HandleBox(in T value) : base(value)
    {
    }

    internal override bool NamesZeroBase => false;
}
