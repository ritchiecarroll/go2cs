// ж.NativeBox.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using go.golib;

namespace go;

/// <summary>
/// The NATIVE-address kind — a pointer that ALIASES an address rather than owning managed storage
/// (a kernel-returned pointer, a <c>uintptr</c> round-trip, the reinterpret seam). One of the
/// four kinds of <see cref="ж{T}"/> under the B1 per-kind split.
/// </summary>
/// <remarks>
/// <para>
/// The aliasing doctrine is unchanged from the pre-split box, moved here verbatim: such an
/// address must be ALIASED, never copied — copying the pointed-at value into a managed box loses
/// the address, so a native walk scans the GC heap instead, and returning the box's address to
/// the OS asks it to free GC memory (STATUS_HEAP_CORRUPTION). A zero address IS the nil pointer
/// (<c>(*T)(unsafe.Pointer(uintptr(0))) == nil</c>) — marked at construction, which is what keeps
/// <c>DerefOrNil</c> on the safe shared slot (the amendment-7 contract, preserved by ctor).
/// </para>
/// <para>
/// <see cref="m_retainedSource"/> is the B1 §4 source-retention slot (the NetShareAdd remedy):
/// a native box minted FROM MANAGED STORAGE by either reinterpret-fallback arm retains the box it
/// was derived from, so a hand-owned wrapper can recover the typed struct and perform the
/// established field-for-field boundary copy — <b>the wrapper copies from the retained source and
/// never uses the address</b> (there is no pin on the non-aliasing fallback path; the raw address
/// in such a box remains wrong-but-contained exactly as the address route documents).
/// Kernel-returned native boxes carry <c>null</c> here. Retention pins nothing and roots only
/// what the caller's own frame already rooted.
/// </para>
/// </remarks>
public sealed class NativeBox<T> : ж<T>
{
    // The native address this box aliases — never managed storage it owns.
    private readonly nuint m_nativeAddr;

    // §4's source-retention slot — see the class remarks.
    private readonly object? m_retainedSource;

    // Create a pointer that ALIASES a native address. A zero address is the nil pointer. An
    // address INTO managed storage carries the pin that holds that storage still; a genuinely
    // native one carries none, there being nothing the collector could move.
    internal NativeBox(nuint nativeAddress, PinnedBuffer? pin = null, object? retainedSource = null)
        : base(isNull: nativeAddress == 0)
    {
        m_nativeAddr = nativeAddress;
        m_pin = pin;
        m_retainedSource = retainedSource;

        // The box only. The memory it aliases is native — never charged, because the CLR heap
        // never received it — and the pin, when there is one, is charged by whoever constructed
        // it. Leaf-ctor counting per the B1 split — same charge as before it.
        AllocationCounter.Count();
    }

    /// <inheritdoc/>
    public override unsafe ref T Value => ref Unsafe.AsRef<T>((void*)m_nativeAddr);

    /// <inheritdoc/>
    public override unsafe ref T ValueSlot => ref Unsafe.AsRef<T>((void*)m_nativeAddr);

    /// <inheritdoc/>
    public override nuint NativeAddress => m_nativeAddr;

    /// <inheritdoc/>
    // A native alias is not managed storage at all: its address is m_nativeAddr and both
    // operators return it long before they consult this, so no reachable path reads the answer.
    // It is stated rather than inherited because the abstract member exists precisely so that a
    // kind cannot stay silent — and None is the honest word for "no MANAGED storage to name".
    public override PointerStorage StorageKind => PointerStorage.None;

    /// <summary>
    /// The managed box this native box was derived from by a reinterpret fallback, when there is
    /// one — the B1 §4 recovery surface for hand-owned wrappers (see the class remarks). Null for
    /// kernel-returned native boxes.
    /// </summary>
    public object? RetainedSource => m_retainedSource;

    /// <inheritdoc/>
    // Two boxes ALIASING the same native address are the same Go pointer — that address is the
    // whole of their identity (`(*T)(unsafe.Pointer(p)) == (*T)(unsafe.Pointer(p))` after a
    // uintptr round-trip, which produces a fresh box each time).
    public override nuint PointerOrderToken => IsNilPointer ? 0 : m_nativeAddr;

    /// <inheritdoc/>
    public override bool Equals(ж<T>? other)
    {
        if (other is null)
            return m_isNull;

        if (ReferenceEquals(this, other))
            return true;

        if (other is NativeBox<T> nb)
            return m_nativeAddr == nb.m_nativeAddr;

        return m_isNull && other.IsNilPointer;
    }

    /// <inheritdoc/>
    // A native alias hashes by the address it aliases, so two boxes over one address (which
    // Equals reports as the same pointer) land in the same bucket.
    public override int GetHashCode() => IsNilPointer ? 0 : m_nativeAddr.GetHashCode();

    // ---- the atomic pointer-word boundary (unchanged bodies, relocated with their kind) ----
    //
    // A native-backed ж<T> whose T is a MANAGED type is the reinterpret the hammer family
    // performs. The slot is read and written as the pointer-sized WORD it is, atomically (Go's
    // LoadPointer/StorePointer contract); the number ↔ box conversion happens in the caller.
    // Callers branch on IsNative, and these live in golib because converted packages compile with
    // AllowUnsafeBlocks=false. The word is 64 bits by the corpus's own sizes authority
    // (`types.SizesFor("gc", "amd64")`).

    /// <inheritdoc/>
    public override unsafe nuint ReadPointerWord()
    {
        return (nuint)Volatile.Read(ref *(ulong*)m_nativeAddr);
    }

    /// <inheritdoc/>
    public override unsafe nuint ExchangePointerWord(nuint value)
    {
        return (nuint)Interlocked.Exchange(ref *(ulong*)m_nativeAddr, value);
    }

    /// <inheritdoc/>
    public override unsafe bool CompareExchangePointerWord(nuint old, nuint @new)
    {
        return Interlocked.CompareExchange(ref *(ulong*)m_nativeAddr, @new, old) == old;
    }
}
