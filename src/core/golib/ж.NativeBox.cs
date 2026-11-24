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
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class NativeBox<T> : ж<T>, INativeRooted
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

    // The address of a WHOLE-struct read or write (Value, ValueSlot): DereferenceableAddress, refused first when T is a
    // Go struct whose CLR layout is provably not its Go layout. Such a read would land Go's bytes in the wrong fields,
    // or read a garbage managed reference where Go stores the value inline. Its fields stay reachable at their Go
    // offsets (a field view of a native box is a native box, ж.of), and the native WORD operations (atomics on a
    // pointer slot) read only the word, so they take DereferenceableAddress directly. Decided once per T.
    private unsafe void* ValueAddress()
    {
        if (s_layoutMismatch is { } reason)
            throw RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(T), reason);

        void* address = DereferenceableAddress();

        // THE NAMED-ARRAY FLOOR, at the read of the whole value (2026-11-14; docs/phase4/DESIGN-native-array-view.md,
        // the blocks of that date). T is the wrapper struct of a named array type, whose one field is a managed
        // reference, and Value reads a T at the address with Unsafe.AsRef. No real address holds a wrapper: storage
        // that holds a reference is never pinned, and C memory never held one. So the read took raw bytes for a
        // reference, and where the bytes were zero the wrapper's lazy fill allocated a managed array and stored its
        // reference INTO those bytes, C memory or a Go array's elements, with no error and where the collector cannot
        // see it. Measured before the floor, five Go shapes of seven stored an address into the Go array they viewed.
        //
        // HERE AND NOT IN THE CONSTRUCTOR, where this line first stood. A box of the wrapper is a CARRIER until it
        // is read: its number, its equality, a conversion onward to another pointee and a copy-back that hands it
        // to a caller who never dereferences it read no memory, and all of them worked before the floor
        // (NamedArrayCarrierTests). The same ruling as OrderTokenRefusal above. Every whole-value read of a native
        // box comes through this method, the wrapper's indexer and its lazy fill included, since they take the ref
        // Value returns.
        //
        // AFTER DereferenceableAddress, so an order token keeps its own refusal and needs no exemption here.
        // NOT THE NUMBER 0, which does need one: a nil box passes DereferenceableAddress (that method refuses
        // tokens, and 0 is not one) and must then answer what a nil pointer of every other pointee answers, the
        // fault of the read itself. This line first stood here with no test of the address, on the belief that
        // nil never reached it, and a nil named-array pointer answered the floor's text
        // (NamedArrayViewFloorTests.ANilNamedArrayPointerReadsAsANilPointerAndItsTypeAnswersItsLength, red before
        // the conjunct). It cures nothing: the shapes panic by name until a conversion exists that answers a
        // wrapper box sharing the array.
        if (s_isNamedArrayShaped && address != null)
            throw RuntimeErrorPanic.NamedArrayViewOfNativeMemory(typeof(T));

        // THE MANAGED-DERIVED STRUCT FLOOR (B0', W14, COORD ruling 2026-11-15). T is a Go struct value that holds a managed
        // reference and that the layout refusal above could not prove mismatched (an array field whose length the managed
        // type does not carry leaves its Go offsets unknown), and this box was derived from MANAGED storage: it retains its
        // source, or it pins it. The bytes at such an address are never a T's references. Pinnable storage is reference-free;
        // a pointee that holds a reference has no address at all (its number is an order token); and an UNPINNABLE real
        // address -- a reference-free field or element of a struct that holds references (PointerStorage.Unpinnable) --
        // holds that field's bytes, and a T read there can run past the field (P5, 2026-11-15). So a whole-value read takes
        // data bytes for references:
        // an access violation over filled data (sqlite's crypt_blowfish, *(*TBF_ctx)(unsafe.Pointer(&[1042]uint32{...}))),
        // null references over zeroed data. Neither is Go's answer, so the read is refused by name.
        //
        // Its scope is the census's B2b with source=retained or source=pinned (NativeBoxDereferenceCensus). A box with no
        // retained source and no pin is native-origin (C memory, an OS buffer): that is P5's deferred B2 and its sweep, and
        // this line does not reach it. A CARRIER over the same box is untouched: a field read at its Go offset (FieldRef),
        // the number, and a conversion onward do not come through here.
        if (s_isReferenceBearingGoStruct && address != null && (m_retainedSource is not null || m_pin is not null))
            throw RuntimeErrorPanic.ManagedDerivedStructRead(typeof(T));

        // THE REFERENCE FLOOR (2026-11-15; docs/phase4/DESIGN-cgo-phase3-marshaling.md section 18, the s6next root). T is
        // a CLR REFERENCE type (a Go pointer ж<U>, a func, an interface, unsafe.Pointer), and Unsafe.AsRef reads a T at the
        // address: it took Go's word for a managed reference. C's `entry.next`, a pointer to C memory, became a reference
        // to an object that does not exist, and the next dereference faulted (cgo-e2e s6next on every hosted target). A
        // write stored a managed reference where the collector cannot see it. Measured before the floor with the census
        // (the seat's BOARD row): outside the census's own controls, the one population was unsafe.Pointer over a Go word.
        //
        // NOT a map, a channel, a string, a slice or an array value: golib's forms of those are value types that hold a
        // reference, so this test does not reach them. They stay the census's B2a value kinds below.
        //
        // NO EXEMPTION (W4, cure S-B1, docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.3-18.7). Until the word doors landed,
        // unsafe.Pointer over MANAGED Go storage was exempt, because the converted runtime reaches `(*unsafe.Pointer)(p)`
        // over a Go word (map.go's indirect key, atomic_pointer.go's atomicwb); the exempt read stored a CLR reference in a
        // word the collector does not scan (found 2026-11-16; df230c8d43). The converter now emits `*p` and `*p = v` for a
        // *unsafe.Pointer p as unsafe.Pointer.LoadWord / StoreWord, and the hand-owned atomic doors take the word doors, so
        // a `.Value` read of such a box is reached only by code those doors did not rewrite, and it is refused by name.
        //
        // AFTER the named-array floor and B0' (whose population, a value-type struct, is disjoint from this one) and NOT for
        // the number 0, for the reason given there: a nil pointer to a Go pointer answers what every nil pointer answers.
        // BEFORE the census, so a refused read is never counted.
        if (s_isReference && address != null)
            throw RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(T), ManagedReferenceReason);

        // The dereference census (NativeBoxDereferenceCensus): a T that holds a reference and was not refused above, and
        // a box derived from storage that cannot be pinned. Counted after the refusals, so no refused read is counted.
        // Every count at a non-zero address is a read or a write that happened; a nil box passes the floors above and is
        // counted as an ATTEMPT, and the read then faults (COORD 2026-11-16: the census keeps counting it). Null unless
        // the census is on: one test of a static readonly field.
        if (s_census is { } census)
            NativeBoxDereferenceCensus.Dereference(census, m_retainedSource, m_pin is not null);

        return address;
    }

    private static readonly string? s_layoutMismatch = GoReflect.ClrLayoutMismatch(typeof(T));

    // Is T the wrapper struct go2cs-gen writes for a NAMED array type (`type RawMem [N]byte`)? It is a value type that
    // is an IArray and holds a managed reference (its one field is a lazily filled holder of the array<E>). A slice is
    // an IArray too (ISlice extends it) and is not this shape, and array<E> itself is not: its floor is arm 5 of the
    // number conversion (ж<T>.FromNumber). `default(T) is I` is a type test on a value type: the JIT and ILC answer
    // it from the type, with no box and no reflection. Decided once per T, read by ValueAddress above.
    private static readonly bool s_isNamedArrayShaped =
        !(typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(array<>)) && // ONE TEST, THREE SPELLINGS: ж<T>.s_isArrayShaped and the last line of ж<T>.PublishesArrayBacking (ж.cs) are this one; change the three together
        default(T) is IArray and not ISlice && RuntimeHelpers.IsReferenceOrContainsReferences<T>();

    // Is T a Go struct value that holds a managed reference (the census's B2b)? Read by ValueAddress's managed-derived
    // floor. Decided once per T.
    private static readonly bool s_isReferenceBearingGoStruct =
        typeof(T).IsValueType && RuntimeHelpers.IsReferenceOrContainsReferences<T>() && GoReflect.KindOf(typeof(T)) == GoReflect.Struct;

    // The reference floor's fact about T, read by ValueAddress above. Decided once per T.
    private static readonly bool s_isReference = !typeof(T).IsValueType;

    private const string ManagedReferenceReason = "a managed reference where Go stores words";

    private static readonly NativeBoxDereferenceCensus.Pointee? s_census = NativeBoxDereferenceCensus.Pointee.Of<T>();

    /// <inheritdoc/>
    public override unsafe ref T Value => ref Unsafe.AsRef<T>(ValueAddress());

    /// <inheritdoc/>
    public override unsafe ref T ValueSlot => ref Unsafe.AsRef<T>(ValueAddress());

    // B2's value decode (COORD ruling 2026-11-15; ж<T>.GoValue): a Go struct that holds a managed reference, read as a
    // VALUE, is decoded out of the bytes at Go's own field offsets into a fresh T (NativeStructMarshal.DecodeValue), which
    // is Go's answer for a copy whatever the memory's origin: Go storage of another type (sqlite's crypt_blowfish,
    // *(*TBF_ctx)(unsafe.Pointer(&[1042]uint32{...}))) or C memory (COORD's Q1). Only arrays decode; a struct holding
    // any other reference is refused by name, since memory holds no image of one. Every other T reads as Value does,
    // floors included. The floors above refuse the whole-value REF, which a copy does not need: the layout floor's
    // mismatch is exactly what the decode reads past, and B0' refuses taking bytes for references, which a decode
    // never does.
    private protected override unsafe T ReadGoValue()
    {
        if (!s_isReferenceBearingGoStruct)
            return ValueSlot;

        void* address = DereferenceableAddress();

        if (address == null)
            throw RuntimeErrorPanic.NilPointerDereference();

        return (T)NativeStructMarshal.DecodeValue(typeof(T), (byte*)address);
    }

    /// <summary>
    /// The field <paramref name="fieldRefFunc"/> names, at its GO offset in the native memory this box aliases --
    /// the managed reference behind <see cref="ж{T}.field{TF}"/> for a native root.
    /// </summary>
    /// <remarks>
    /// The struct's CLR layout is not always Go's (an array, pointer or string field is a managed reference, and a
    /// value embed is declared after the converter's fields), so the CLR slot <c>fieldRefFunc(ref Value)</c> is not
    /// where Go, or a native callee, put the field. The offset is the struct's Go layout, memoized per accessor (the
    /// lookup the field view takes, FieldRefBox.GoFieldOffsetOf). A struct whose Go layout cannot be known keeps the
    /// CLR slot, which is the whole-value read and refuses a provably mismatched layout by name. A field that is
    /// itself a managed reference, or a struct whose own layout is not Go's, cannot be read out of native bytes and
    /// is refused by name with the same text.
    /// </remarks>
    internal unsafe ref TF FieldRef<TF>(FieldRefFunc<T, TF> fieldRefFunc)
    {
        nint offset = FieldRefBox<TF>.GoFieldOffsetOf(this, fieldRefFunc);

        if (offset < 0)
            return ref fieldRefFunc(ref Value);

        if (NativeFieldLayout<TF>.Mismatch is { } reason)
            throw reason.Length == 0
                ? RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(T), $"field {FieldRefBox<TF>.FieldNameOf(fieldRefFunc)} is a managed reference where Go stores the value inline")
                : RuntimeErrorPanic.NativeStructLayoutMismatch(typeof(TF), reason);

        return ref Unsafe.AsRef<TF>((byte*)DereferenceableAddress() + offset);
    }

    // Whether a field of type TF can be read in place out of native bytes: null when it can, "" when TF holds a
    // managed reference, else the reason TF's own CLR layout is not its Go layout. Decided once per TF.
    private static class NativeFieldLayout<TF>
    {
        internal static readonly string? Mismatch = RuntimeHelpers.IsReferenceOrContainsReferences<TF>() ? "" : GoReflect.ClrLayoutMismatch(typeof(TF));
    }

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

    /// <summary>
    /// Whether this box's address is INTO managed storage, held still by a pin (see the constructor), rather than a
    /// genuinely native address. A cgo wrapper never hands C such an address (docs/phase4/DESIGN-cgo-phase2-pinvoke.md
    /// §11.1).
    /// </summary>
    internal bool PinsManagedStorage => m_pin is not null;

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

        // Go's contract for a pointer into NATIVE memory is address identity, whichever kind names it: a field
        // reference rooted in native memory (`&n.LFNode` over a persistentalloc'd node and the pointer lfstackUnpack
        // rebuilds from its bits are ONE pointer; see FieldRefBox.NativeSlotAddress) and an element box over a native
        // slice or array view (`&b[4]` and `&p[4]` with p viewing b's pages) included.
        if (TryNativeIdentityEquals(other, out bool equal))
            return equal;

        return m_isNull && other.IsNilPointer;
    }

    /// <inheritdoc/>
    internal override nuint NativeIdentity => IsNilPointer ? 0 : m_nativeAddr;

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

    // C memory alone: an address no reinterpret fallback derived from managed storage, that is no order token, and
    // that no pin holds over managed storage. It is the one pointer whose address a cgo wrapper hands C, asked here
    // without the pointee type because an unsafe.Pointer carries its box as an object (GoCgoArgs.VoidPointer).
    bool INativeRooted.IsGenuinelyNativeRoot => m_retainedSource is null && !m_aliasesAnOrderToken && m_pin is null;

    // What this box was derived from, for the identity of an unsafe.Pointer that carries it (NativeDerivation.Token).
    // A box over an order token has no address to measure a distance from.
    object? INativeRooted.NativeDerivedFrom => m_aliasesAnOrderToken ? null : m_retainedSource;

    // What a box derived from this one retains in its place (NativeDerivation.Kept). A box that owns no pin holds its
    // source and nothing else: the source is what keeps the storage where this box's address says it is, and a box
    // derived from this one needs the source and not this box. A box that owns a pin is kept itself (the pin is
    // released when the box is collected). So is a box over an order token or the number 0, which has no address to
    // measure a distance from (NativeDerivation.Token).
    object? INativeRooted.NativeRetainedInItsPlace =>
        m_pin is null && !m_aliasesAnOrderToken && m_nativeAddr != 0 && !ManagedPointerTokens.IsTaggedToken(m_nativeAddr)
            ? m_retainedSource
            : null;
}
