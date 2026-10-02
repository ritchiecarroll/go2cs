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
public sealed class NativeBox<T> : ж<T>, INativeRooted
{
    // The native address this box aliases — never managed storage it owns.
    private readonly nuint m_nativeAddr;

    // §4's source-retention slot — see the class remarks.
    private readonly object? m_retainedSource;

    // Q44 §10.3 ARM 2a: this box's "address" is a live box's ORDER TOKEN and not an address at all.
    // Set ONLY by the uintptr operator, which is the one place that has the resolved box in hand.
    // See OrderTokenRefusal below for why it is a field rather than a lookup.
    private readonly bool m_aliasesAnOrderToken;

    // Create a pointer that ALIASES a native address. A zero address is the nil pointer. An
    // address INTO managed storage carries the pin that holds that storage still; a genuinely
    // native one carries none, there being nothing the collector could move.
    internal NativeBox(nuint nativeAddress, PinnedBuffer? pin = null, object? retainedSource = null,
                       bool aliasesAnOrderToken = false)
        : base(isNull: nativeAddress == 0)
    {
        m_nativeAddr = nativeAddress;
        m_pin = pin;
        m_retainedSource = retainedSource;
        m_aliasesAnOrderToken = aliasesAnOrderToken;

        // The box only. The memory it aliases is native — never charged, because the CLR heap
        // never received it — and the pin, when there is one, is charged by whoever constructed
        // it. Leaf-ctor counting per the B1 split — same charge as before it.
        AllocationCounter.Count();
    }

    /// <summary>
    /// THE ARM 2a REFUSAL, at the DEREFERENCE — where this tree's charter already put the fault.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ⚠ THE CONVERSION IS NOT THE FAULT AND MUST NOT BE, and that is measured rather than
    /// preferred. A refusal at the <c>uintptr</c> operator took SEVEN GolibTests red against an
    /// empty base (2026-09-20), because a native box over a token is a deliberate CARRIER: its
    /// address IS the token, which is how <c>PointerExtensions.Reinterpret</c>'s unpinnable class
    /// and the boundary wrappers recover the source box. RuntimeHashFamilyTests.cs:182 states the
    /// ruling the tree already held — "a dereference is the row-level fault the design chose,
    /// never a number". This is that fault, made CATCHABLE.
    /// </para>
    /// <para>
    /// A FIELD AND NOT A LOOKUP, deliberately. The alternative is asking the token registry on every
    /// native dereference in the corpus, which is a resolve on a path measured at 264,167 calls in a
    /// single roster row — the exact shape this tree has twice removed as an instrument that
    /// perturbs what it measures. The verdict is computed ONCE, by the operator that already
    /// resolved the number, and rides in the object. An ordinary native pointer pays one branch on a
    /// readonly field.
    /// </para>
    /// <para>
    /// BOTH accessors, because both are lethal and for different reasons: the WRITE
    /// (<c>*(*V)(p) = value</c>, reflect's <c>setField</c>) lands on an unmapped page, and the READ
    /// materializes a <typeparamref name="T"/> out of whatever the token's bytes are — which for a
    /// reference-bearing <typeparamref name="T"/> fabricates a managed reference, the same
    /// type-safety hole the native-array-view floor refuses one container over.
    /// </para>
    /// </remarks>
    private PanicException OrderTokenRefusal() =>
        RuntimeErrorPanic.UnsafePointerOrderTokenDereferenced(typeof(T), m_nativeAddr);

    // The address every dereference below reads or writes, refused first where no memory answers:
    // arm 2a's named refusal for a flagged order token, then the minted class as a whole
    // (ManagedPointerTokens.NamesNoUserMemory), which answers the nil-dereference panic linux's own
    // fault already gives instead of letting Windows take the host down.
    private unsafe void* DereferenceableAddress()
    {
        if (m_aliasesAnOrderToken)
            throw OrderTokenRefusal();

        if (ManagedPointerTokens.NamesNoUserMemory(m_nativeAddr))
            throw RuntimeErrorPanic.NilPointerDereference();

        return (void*)m_nativeAddr;
    }

    /// <inheritdoc/>
    public override unsafe ref T Value => ref Unsafe.AsRef<T>(DereferenceableAddress());

    /// <inheritdoc/>
    public override unsafe ref T ValueSlot => ref Unsafe.AsRef<T>(DereferenceableAddress());

    /// <summary>
    /// Whether this box aliases an ORDER TOKEN rather than an address — Q44 §10.3 arm 2a. Its
    /// numeric value and identity are unchanged; only a DEREFERENCE is refused.
    /// </summary>
    public bool AliasesAnOrderToken => m_aliasesAnOrderToken;

    /// <inheritdoc/>
    public override nuint NativeAddress => m_nativeAddr;

    /// <inheritdoc/>
    // The root of every native chain: a field reference over this box asks here (INativeRooted).
    bool INativeRooted.IsNativeRooted => true;

    nuint INativeRooted.NativeRootAddress => NativeAddress;

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

        // A field reference rooted in NATIVE memory names a real machine address, and Go's contract for
        // such a pointer is address identity — `&n.LFNode` over a persistentalloc'd node and the pointer
        // lfstackUnpack rebuilds from its bits are ONE pointer. See FieldRefBox.NativeSlotAddress.
        if (other is FieldRefBox<T> fr && fr.NativeSlotAddress is var address and not 0)
            return m_nativeAddr == address;

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
        return (nuint)Volatile.Read(ref *(ulong*)DereferenceableAddress());
    }

    /// <inheritdoc/>
    public override unsafe nuint ExchangePointerWord(nuint value)
    {
        return (nuint)Interlocked.Exchange(ref *(ulong*)DereferenceableAddress(), value);
    }

    /// <inheritdoc/>
    public override unsafe bool CompareExchangePointerWord(nuint old, nuint @new)
    {
        return Interlocked.CompareExchange(ref *(ulong*)DereferenceableAddress(), @new, old) == old;
    }
}
