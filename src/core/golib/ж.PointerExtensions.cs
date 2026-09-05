// ж.PointerExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

// ---------------------------------------------------------------------------------------------
// POINTER EXTENSIONS — the operations ON a Go pointer that are not part of the box's own state.
//
// WHAT LIVES HERE
//   Extension methods over ж<T>: reinterpreting a pointer as another pointee type (Go's
//   `(*U)(unsafe.Pointer(p))`), the nil-safe re-alias a pointer walk needs on its last step, and
//   the address-token printer behind `ж<T>.ToString()`. ж.cs itself holds the box — its storage,
//   its identity, its dereference operators.
//
//   PrintPointer is NOT the `%p` implementation, despite producing the same shape of token. Go's
//   own verbs are served by the converted `fmt`, which asks the reflection bridge for a pointer's
//   address (`reflect.Value.UnsafePointer` → INilPointer.PointerOrderToken). This one backs
//   ToString, so it is what a debugger watch window, an interpolated `$"{p}"` and any diagnostic
//   that formats a box without going through Go's fmt will show.
//
// WHY EXTENSION METHODS AND NOT INSTANCE MEMBERS
//   Because a Go nil pointer has TWO managed representations and only one of them can receive an
//   instance call. A zero-valued pointer FIELD is a plain `null` reference, never the canonical
//   nil box, so `p.Reinterpret<T, U>()` written as an instance method throws
//   NullReferenceException on exactly the value Go says is a perfectly ordinary nil pointer. As
//   extension methods these take `ж<T>? box` and answer for both representations — which is why
//   every one of them opens by testing `box is null || box.IsNilPointer` rather than either alone.
//   That is the rule for adding to this file: an operation that must tolerate a null receiver, or
//   that is about a pointer rather than about the box's internals, belongs here; anything that
//   needs ж<T>'s private fields stays on the box.
//
// THE ONE THING TO KNOW BEFORE EDITING
//   The address route and the aliasing route are NOT interchangeable, and the difference is a
//   lifetime. A `uintptr`/`void*` taken from a managed box is pinned only for the duration of the
//   statement that took it, so an address that outlives that statement names memory the collector
//   may already have moved or reused. An ALIASING pointer holds the referent, so it stays valid
//   exactly as Go's does. `Reinterpret` prefers the alias and falls back to the address only where
//   the managed model cannot represent the alias — see its remarks, and
//   docs/phase4/FINDING-managed-box-uintptr-lifetime.md for the failure the alias route fixed
//   (reflect cached an address for process lifetime, and `TypeOf(x).Kind()` began reporting
//   Invalid once that address was recycled).
//
//   That fallback is not exempt from the lifetime rule, it just cannot satisfy it by aliasing: an
//   address of managed storage is a pointer only while the storage is held still, so the derived box
//   PINS it and owns the pin (ж<T>.TryPinnedReinterpret). Where no pin can be taken the route is the
//   bare address it always was — never assume a fallback pointer is safe to retain because the
//   common case now is.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Extension methods for <see cref="ж{T}"/> pointer references.
/// </summary>
public static class PointerExtensions
{
    /// <summary>
    /// Yields this pointer in the form that carries its Go TYPE into interface space — itself when
    /// it is a real box, and the canonical typed nil instance (<see cref="ж{T}.NilBox"/>) when it is
    /// a bare <c>null</c> reference.
    /// </summary>
    /// <typeparam name="T">Pointee type of the pointer.</typeparam>
    /// <param name="box">The pointer, which may be <c>null</c>.</param>
    /// <remarks>
    /// <para>
    /// Go's nil pointer inside an interface is a VALUE with a dynamic type: <c>any((*T)(nil))</c> is
    /// a non-nil interface, <c>%T</c> prints <c>*T</c>, and <c>x.(*T)</c> asserts successfully with
    /// a nil result. The managed model has two representations of a nil <c>*T</c> — a <c>null</c>
    /// reference, which carries nothing once boxed into <c>object</c>, and the canonical typed nil
    /// box, which carries everything — and Go treats them as the same pointer, so both must be
    /// accepted everywhere a pointer is accepted. The choice therefore cannot be made where nil is
    /// PRODUCED (a declared variable, a struct field, a slice element, a map miss); it is made here,
    /// at the one boundary where the difference becomes observable.
    /// </para>
    /// <para>
    /// The converter emits this at every pointer-into-<c>any</c> site (the <c>TypedNilBoxAccessor</c>
    /// symbol). A NON-empty interface target needs it not — the generated adapter null-coalesces its
    /// receiver box to the same canonical instance.
    /// </para>
    /// </remarks>
    public static ж<T> OrTypedNil<T>(this ж<T>? box)
    {
        // The canonical instance, never a fresh box — two typed nils of one type must compare
        // reference-equal wherever the comparison is an untyped object reference compare.
        return box ?? ж<T>.NilBox;
    }

    /// <summary>
    /// Reinterprets a pointer as a pointer to <typeparamref name="TDst"/> — Go's
    /// <c>(*TDst)(unsafe.Pointer(p))</c>.
    /// </summary>
    /// <typeparam name="T">Pointee type of the source pointer.</typeparam>
    /// <typeparam name="TDst">Pointee type of the derived pointer.</typeparam>
    /// <param name="box">Source pointer, which may be <c>null</c>.</param>
    /// <remarks>
    /// <para>
    /// Where the reinterpret is <em>representable</em> in the managed model (see
    /// <see cref="ReinterpretAliasesStorage{T,TDst}"/>) the result ALIASES the source's storage and
    /// therefore keeps its pointee ALIVE and stays valid across garbage collection, exactly as Go's
    /// does — Go's collector tracks a pointer obtained through <c>unsafe.Pointer</c> as a real
    /// reference. That is the property the raw-address route cannot provide: the <c>uintptr</c>/
    /// <c>void*</c> operators pin only for the duration of their own statement, so an address that
    /// escapes them belongs to a box the collector is free to move or reclaim, and a derived pointer
    /// holding one reads whatever later occupies that memory — silently, and only after enough
    /// allocation churn to recycle it. <c>reflect</c> caches exactly such a pointer for process
    /// lifetime (<c>toRType</c> → <c>canonType</c>), and once the address was recycled
    /// <c>TypeOf(x).Kind()</c> began reporting <c>Invalid</c> mid-process. See
    /// <c>docs/phase4/FINDING-managed-box-uintptr-lifetime.md</c>.
    /// </para>
    /// <para>
    /// A <c>null</c> reference and a nil box are the same Go nil pointer, and both reinterpret to the
    /// derived type's nil — Go's <c>(*TDst)(unsafe.Pointer((*T)(nil))) == nil</c>. (An extension
    /// method, rather than an instance method, is what lets the <c>null</c> representation be
    /// tolerated at all — a zero-valued pointer field is a plain <c>null</c>, and an instance call on
    /// it throws where Go yields nil.)
    /// </para>
    /// <para>
    /// A box aliasing NATIVE memory keeps the address model: there the address IS the meaning, and
    /// that is the only thing correct for those call sites. A reinterpret the managed model cannot
    /// represent falls back to the same address route — i.e. to the behavior that predates this
    /// method, never to something newly wrong — except that where the source's storage CAN be held
    /// still, the derived box now pins it for its own lifetime rather than inheriting an address
    /// that was pinned only for the statement that took it (see
    /// <see cref="ж{T}.TryPinnedReinterpret{TDst}"/>).
    /// </para>
    /// </remarks>
    public static ж<TDst> Reinterpret<T, TDst>(this ж<T>? box)
    {
        // Both nil representations — a null reference and a nil box — yield the derived type's nil.
        if (box is null || box.IsNilPointer)
            return ж<TDst>.NilBox;

        // A native-backed box IS its address; reinterpreting it keeps aliasing that address.
        if (box.IsNative)
            return new NativeBox<TDst>(box.NativeAddress);

        if (ReinterpretAliasesStorage<T, TDst>.Value)
        {
            // Alias the SAME managed storage. Routing through ValueSlot (rather than a cached ref)
            // is what makes this GC-safe: the ref is recomputed on each access from a live object
            // reference, so a collection that moves the box is invisible here. ValueSlot also
            // composes through the field-ref and array-element reference kinds, so a reinterpret of
            // `&s.f` or `&a[i]` aliases the real storage rather than a copy.
            return new FieldRefBox<TDst>(box, ж<T>.ReinterpretRef<TDst>);
        }

        // A Go slice HEADER read over a golib slice<X> — `(*slice)(unsafe.Pointer(&b))`, emitted as
        // Reinterpret<slice<X>, Δsliceᴛ>(). Not an aliasing pair (five fields and a T[] against a pointer
        // and two integers) and not pinnable, so the address route below minted a NativeBox over the
        // PINNED managed struct whose first field read back the backing array's reference AS a pointer
        // (measured 2026-09-04: a type-confused System.Byte[], a native SIGSEGV on the first dereference).
        // The header box materializes Go's (array, len, cap) from the live slice instead; see its remarks
        // for the write limit and the string half it deliberately leaves to Q44.
        if (SliceHeaderBox<T, TDst>.Applies)
            return SliceHeaderBox<T, TDst>.Mint(box);

        // The MIRROR: a Go slice built FROM a header — `*(*[]T)(unsafe.Pointer(&sl))` over a
        // notInHeapSlice the runtime has just filled with a NATIVE base and (len, cap), emitted as
        // Reinterpret<notInHeapSlice, slice<X>>(). The address route below would read a golib slice<X>
        // struct out of a three-word header (the same type confusion the other way round); the box
        // minted here re-bases a native-backed slice<X> over the header's address instead. Its sites
        // are the page allocator's summary levels and chunk index and a span's heap-bits window
        // (increment 7 of the runtime row, 2026-09-05); see its remarks for the two shapes it refuses.
        if (HeaderSliceBox<T, TDst>.Applies)
            return HeaderSliceBox<T, TDst>.Mint(box);

        // Not representable as an alias, so the derived pointer has to name the source's storage by
        // ADDRESS — which is only a pointer at all while that storage is held still. Pin it for the
        // derived box's lifetime where it can be pinned (see TryPinnedReinterpret, which also says why
        // that is the array/slice-element reference and nothing else). Where it cannot, keep the
        // pre-existing raw-address behavior — never something newly wrong.
        if (box.TryPinnedReinterpret<TDst>() is { } pinned)
            return pinned;

        ж<TDst> derived = (ж<TDst>)(uintptr)box;

        // THE UNPINNABLE CLASS — remember the source, because nothing else can recover it.
        //
        // The address route's own recovery seam is the provenance record: the uintptr operator
        // calls ManagedPointerTokens.RegisterPinned, and a boundary wrapper resolves the scalar
        // back to the box (the certchain hand-own does exactly that for its opaque extra-policy
        // pointer). That seam VALIDATES ON READ — "alive AND still pinned there" — so it answers
        // MISS for a source whose storage cannot be pinned at all, which is precisely the
        // reference-bearing pointee: no m_slot, so PinnableStorage is null, so no pin, so
        // IsPinnedAt is false and Resolve returns null. Measured, with a reference-free control
        // resolving on the same run: docs/phase4/BOARD-next-validation-candidates.md, the
        // SHARE_INFO_2 entry.
        //
        // For that class the scalar is not merely unrecoverable, it is not an address a moment
        // later either — so a hand-owned wrapper handed this pointer has, today, nothing at all to
        // read. Remembering the source against the DERIVED BOX closes that with no change any
        // existing consumer can observe: the numeric value is byte for byte what it was, the box
        // kind is what it was, and only a caller that asks by name (ReinterpretSource) sees
        // anything new. It also gives the derived pointer the referent-liveness Go's own collector
        // gives a pointer obtained through unsafe.Pointer.
        //
        // NARROW BY DESTINATION, deliberately. A reference-BEARING destination is Go's
        // prefix-downcast idiom — reflect's (*structType)(unsafe.Pointer(t)) over an abi.Type,
        // which is unpinnable too and is HOT — and it neither needs nor wants this: nothing hands
        // that pointer to native code. A reference-FREE destination is the boundary idiom,
        // `(*byte)(unsafe.Pointer(&record))`, whose whole purpose is to cross into a syscall. The
        // corpus census at the time of writing is ~30 such sites, all in syscall/os/net boundary
        // code and all cold.
        if (RemembersReinterpretSource<T, TDst>.Value && box.PinnableStorage is null)
            ManagedPointerTokens.RememberReinterpretSource(derived, box);

        return derived;
    }

    /// <summary>
    /// Whether a non-representable reinterpret from <typeparamref name="T"/> to
    /// <typeparamref name="TDst"/> should REMEMBER its source box against the derived pointer.
    /// Cached per type pair.
    /// </summary>
    /// <remarks>
    /// A reference-bearing source is the class whose address the provenance record cannot serve; a
    /// reference-free destination is the class that is about to be handed to native code. Both
    /// together are the boundary idiom this exists for, and they exclude the prefix-downcast
    /// idiom — see the call site.
    /// </remarks>
    internal static class RemembersReinterpretSource<T, TDst>
    {
        internal static readonly bool Value =
            RuntimeHelpers.IsReferenceOrContainsReferences<T>() &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<TDst>();
    }

    /// <summary>
    /// Whether reinterpreting a <c>ж&lt;<typeparamref name="T"/>&gt;</c> as a
    /// <c>ж&lt;<typeparamref name="TDst"/>&gt;</c> may ALIAS the source's managed storage, rather
    /// than falling back to the raw-address route. Cached per type pair.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Go's rule for <c>(*TDst)(unsafe.Pointer(p))</c> is that <c>TDst</c> is no larger than
    /// <c>T</c> and the two share an equivalent memory layout. That rule cannot simply be inherited,
    /// because a go2cs surrogate's C# layout is NOT its Go layout: a Go <c>[2]byte</c> is 2 bytes but
    /// <c>array&lt;byte&gt;</c> is a single reference to a backing store, a Go <c>string</c> is 16
    /// bytes but <c>@string</c> is 8, a Go <c>[]byte</c> is 24 but <c>slice&lt;byte&gt;</c> is 32. So
    /// a reinterpret that is perfectly VALID in Go can be an oversized, reference-fabricating
    /// <c>Unsafe.As</c> here — and a fabricated managed reference is a CLR type-safety break (an
    /// access violation, or silent heap corruption on a write), which is strictly worse than the
    /// wrong-but-contained read the address route produces.
    /// </para>
    /// <para>
    /// The alias is therefore taken only where it is demonstrably safe:
    /// </para>
    /// <list type="number">
    /// <item>both pointees are VALUE types — a reference-typed pointee's slot holds an object
    /// reference, and punning one against data is exactly the fabrication case above;</item>
    /// <item>the destination FITS in the source, so the read stays inside the source's storage and
    /// never runs off the value slot into the box's own private fields; and</item>
    /// <item>either neither type contains managed references — so no reference can be fabricated
    /// whatever the field correspondence turns out to be — or the two are layout-compatible in the
    /// senses go2cs actually generates: the same type, a single-field wrapper over the other (Go's
    /// struct-embedding idiom, <c>type rtype struct { t abi.Type }</c>, and the generated named-type
    /// wrappers), or an identical recursive field-type sequence.</item>
    /// </list>
    /// <para>
    /// What this deliberately EXCLUDES is Go's prefix-downcast idiom — allocating a larger struct,
    /// handing out a pointer to its embedded header, and casting back
    /// (<c>(*structType)(unsafe.Pointer(t))</c> where <c>t</c> is a <c>*abi.Type</c>). In Go the
    /// larger allocation is really there; in the managed model a <c>ж&lt;abi.Type&gt;</c> holds only
    /// an <c>abi.Type</c>, so there is nothing behind it to downcast to. Those sites keep the address
    /// route and remain the raw-metal class recorded in
    /// <c>docs/phase4/FINDING-managed-box-uintptr-lifetime.md</c>.
    /// </para>
    /// </remarks>
    internal static class ReinterpretAliasesStorage<T, TDst>
    {
        internal static readonly bool Value =
            typeof(T).IsValueType && typeof(TDst).IsValueType &&
            Unsafe.SizeOf<TDst>() <= Unsafe.SizeOf<T>() &&
            ((!RuntimeHelpers.IsReferenceOrContainsReferences<T>() &&
              !RuntimeHelpers.IsReferenceOrContainsReferences<TDst>()) ||
             LayoutCompatible(typeof(T), typeof(TDst)));

        // Layout compatibility, limited to the correspondences the converter actually generates —
        // it is never a general claim about two arbitrary C# structs.
        private static bool LayoutCompatible(Type src, Type dst)
        {
            if (src == dst)
                return true;

            // Go's struct-embedding idiom and the generated named-type wrappers: one side is a
            // (possibly nested) single-field wrapper over the other, so the wrapped field starts at
            // offset 0 and carries the whole layout.
            if (UnwrapSingleField(src) == dst || UnwrapSingleField(dst) == src)
                return true;

            // Otherwise require the same fields in the same order, all the way down — two spellings
            // of one shape (the `(*view)(unsafe.Pointer(h))` struct-pun).
            Type[] srcFields = FieldTypes(src), dstFields = FieldTypes(dst);

            if (srcFields.Length != dstFields.Length)
                return false;

            for (int i = 0; i < srcFields.Length; i++)
            {
                if (srcFields[i] != dstFields[i] && !LayoutCompatible(srcFields[i], dstFields[i]))
                    return false;
            }

            return srcFields.Length > 0;
        }

        private static Type UnwrapSingleField(Type type)
        {
            // Bounded: each hop strictly descends into a field's type, and a struct cannot contain
            // itself, so the chain terminates.
            for (Type[] fields = FieldTypes(type); fields.Length == 1; fields = FieldTypes(type))
                type = fields[0];

            return type;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2070",
            Justification = "Reinterpreted Go struct surrogates are referenced by the converted code that reinterprets them.")]
        private static Type[] FieldTypes([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicFields)] Type type)
        {
            if (!type.IsValueType || type.IsPrimitive || type.IsEnum)
                return [];

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type[] fieldTypes = new Type[fields.Length];

            for (int i = 0; i < fields.Length; i++)
                fieldTypes[i] = fields[i].FieldType;

            return fieldTypes;
        }
    }

    /// <summary>
    /// Nil-safe re-alias accessor: returns a reference to the pointed-to value, or to a shared
    /// default(<typeparamref name="T"/>) slot when <paramref name="box"/> is a nil pointer
    /// (a <c>null</c> reference or a nil standard pointer).
    /// </summary>
    /// <typeparam name="T">Pointed-to type.</typeparam>
    /// <param name="box">Pointer reference, which may be <c>null</c>.</param>
    /// <returns>
    /// A <c>ref</c> to the value of <paramref name="box"/>, or to a shared default slot when it is nil.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Converted code re-aliases a deref'd pointer parameter after repointing its box, e.g.
    /// <c>Ꮡp = p.next; p = ref Ꮡp.DerefOrNil();</c>, when the parameter is walked to a nil terminator
    /// (<c>for p != nil { …; p = p.next }</c>). On the final step the box becomes nil, and the plain
    /// <see cref="ж{T}.Value"/> getter would throw a nil-pointer dereference before the loop guard is
    /// re-checked. This accessor instead yields a <c>ref</c> to a throwaway default slot — never read
    /// while the box is nil (the <c>Ꮡp != nil</c> guard excludes it), so the value is harmless.
    /// </para>
    /// <para>
    /// This is <em>not</em> a substitute for a genuine dereference: reading or writing <c>*p</c> on a
    /// nil pointer (emitted as <c>~Ꮡp</c> / <c>Ꮡp.Value</c>) still panics, preserving Go semantics. Only
    /// the re-alias — which captures a reference without reading it — uses the nil-safe form. As an
    /// extension method it tolerates a <c>null</c> receiver (a nil pointer field is a <c>null</c>
    /// reference, not a nil box).
    /// </para>
    /// </remarks>
    public static ref T DerefOrNil<T>(this ж<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref NilSlot<T>.Slot;

        // ValueSlot, not Value: the nil pointer is already handled above, and a real box whose
        // reference-typed value is null has a REAL slot — the re-alias must alias that slot so a
        // subsequent write through it persists (Value's value-peeking nil check would instead panic
        // on exactly the captured-pointer-local shape this accessor exists for).
        return ref box.ValueSlot;
    }

    // Per-T shared default slot returned by ref for a nil pointer. Never read while the pointer is
    // nil (the converted loop guard excludes it), so the shared mutable storage is benign.
    private static class NilSlot<T>
    {
        public static T Slot = default!;
    }

    /// <summary>
    /// Nil-DEFERRING deref accessor: returns a reference to the pointed-to value, or a <em>null</em>
    /// reference when <paramref name="box"/> is a nil pointer — so establishing the reference never
    /// throws, and the nil-pointer panic is raised by the first read or write THROUGH it.
    /// </summary>
    /// <typeparam name="T">Pointed-to type.</typeparam>
    /// <param name="box">Pointer reference, which may be <c>null</c>.</param>
    /// <returns>
    /// A <c>ref</c> to the value of <paramref name="box"/>, or <see cref="Unsafe.NullRef{T}"/> when
    /// it is nil.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is what makes a POINTER RECEIVER behave as Go's does. Go permits calling a method through
    /// a nil <c>*T</c>: the method runs, and the panic happens only where the body actually
    /// dereferences the pointee — which is why <c>os</c>'s fifteen nil-tolerant <c>*File</c> methods
    /// return <c>ErrInvalid</c> rather than panicking (each delegates its guard to
    /// <c>f.checkValid</c>, reached through the BOX, before it ever touches a field). A method's entry
    /// alias <c>ref var f = ref Ꮡf.Value;</c> instead dereferences at ENTRY, so a nil receiver panicked
    /// before the body could run at all.
    /// </para>
    /// <para>
    /// The distinction from <see cref="DerefOrNil{T}"/> is the whole point and the two are NOT
    /// interchangeable: <c>DerefOrNil</c> hands back a shared throwaway slot, so a deref that Go says
    /// must panic instead reads <c>default(T)</c> — silently wrong, tolerable only where the converted
    /// guard provably excludes the read (the nil-terminated pointer walk it exists for). This one binds
    /// a null reference, which is legal to HOLD and faults on USE: the first field read, field write,
    /// or whole-struct copy raises <see cref="NullReferenceException"/>, which
    /// <see cref="RuntimeErrorPanic.TryAsPanic"/> maps to Go's
    /// <c>runtime error: invalid memory address or nil pointer dereference</c> — recoverable, and
    /// printed verbatim. So the panic is neither lost nor moved earlier: it lands exactly where Go's
    /// does, AFTER any side effect the body performs first.
    /// </para>
    /// <para>
    /// A non-nil box takes <see cref="ж{T}.ValueSlot"/>, not <see cref="ж{T}.Value"/>, for the reason
    /// <c>DerefOrNil</c> does: a real box whose reference-typed value is null (a heap-boxed pointer,
    /// slice, map, interface or func — <c>*(&amp;p)</c> in Go, which yields nil without dereferencing)
    /// has a REAL slot, and the alias must be that slot so a write through it persists. The nil
    /// POINTER is already handled above, so nothing that should panic reaches here.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DerefOrNull<T>(this ж<T>? box)
    {
        if (box is null || box.IsNilStandardPointer)
            return ref Unsafe.NullRef<T>();

        return ref box.ValueSlot;
    }

    /// <summary>
    /// Renders a pointer as a hexadecimal address token — the shape Go prints for <c>%p</c> — or
    /// <c>"&lt;nil&gt;"</c> for the nil pointer. This is what <see cref="ж{T}.ToString"/> returns;
    /// Go's own <c>%p</c>/<c>%v</c> are served by the converted <c>fmt</c> through the reflection
    /// bridge, not by this.
    /// </summary>
    /// <typeparam name="T">Pointee type.</typeparam>
    /// <param name="ptr">Pointer to render; a <c>null</c> reference reads as nil.</param>
    /// <returns>Address token in the form <c>0x…</c>, or <c>"&lt;nil&gt;"</c>.</returns>
    /// <remarks>
    /// The token is a DISPLAY value, not an address anything may act on — see the
    /// <see cref="PrintPointer(object)"/> overload it delegates to. The nil test goes through
    /// <c>ж&lt;T&gt;</c>'s <c>==</c> operator rather than a member access, which is what lets a
    /// <c>null</c> receiver (the other managed spelling of a Go nil pointer) answer
    /// <c>"&lt;nil&gt;"</c> instead of throwing.
    /// </remarks>
    public static string PrintPointer<T>(this ж<T> ptr)
    {
        if (ptr == nil)
            return "<nil>";

        // An array/slice-element reference can sit outside its backing store's valid range —
        // e.g. a pointer at the zero index of an EMPTY backing (unsafe.StringData semantics) —
        // where reading Value would throw. Display only needs an address-like token, so print
        // the backing store's identity instead of dereferencing the element.
        if (ptr.ArrayRef is var (array, index) && !array.IndexIsValid(index))
            return array.PrintPointer();

        return ptr.Value.PrintPointer();
    }

    /// <summary>
    /// Renders any managed object's current heap address as a hexadecimal token, for DISPLAY only,
    /// or <c>"&lt;nil&gt;"</c> for a null reference.
    /// </summary>
    /// <param name="instance">Object whose address to render; may be <c>null</c>.</param>
    /// <returns>Address token in the form <c>0x…</c>, or <c>"&lt;nil&gt;"</c>.</returns>
    /// <remarks>
    /// <para>
    /// This is the bottom of the pointer-printing path: the <c>ж&lt;T&gt;</c> overload above reduces a
    /// pointer to whichever object IS its storage and asks this for the token. Because the caller is
    /// a diagnostic, printing SOMETHING address-shaped is the whole requirement; nothing may treat
    /// the result as an address.
    /// </para>
    /// <para>
    /// It genuinely is not one for longer than this call: <c>__makeref</c> hands out the object
    /// reference the GC is free to move at the very next collection, so two prints of one object can
    /// legitimately differ and a printed token can name memory something else now occupies. A
    /// pointer that must stay valid is aliased (see <see cref="Reinterpret{T,TDst}"/>) or pinned,
    /// never printed and re-read.
    /// </para>
    /// <para>
    /// The <c>catch</c> is deliberately total. A display helper must not be able to fail a program:
    /// the reference-peeking trick is not defined for every object shape, and a diagnostic that
    /// throws while formatting a panic message would replace the diagnosis with its own crash.
    /// </para>
    /// </remarks>
    public static unsafe string PrintPointer(this object? instance)
    {
        if (instance is null)
            return "<nil>";

        try
        {
            // Do not attempt to use this pointer, its address is unfixed
            // and being accessed for display purposes only. The GC will
            // move this pointer location at will.
            TypedReference reference = __makeref(instance);
            nint ptr = **(nint**)&reference;
            return $"0x{ptr:x}";
        }
        catch
        {
            return "<nil>";
        }
    }
}
