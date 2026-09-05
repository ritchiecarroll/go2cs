// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package unsafe contains operations that step around the type safety of Go programs.

Packages that import unsafe may be non-portable and are not protected by the
Go 1 compatibility guidelines.
*/

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using go.golib;
using go;

[module: GoManualConversion]

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable IL2070
#pragma warning disable IL2072

namespace go;

/// <summary>
/// The unsafe package contains operations that step around the type safety of Go programs.
/// Note that the operations in this package are not type safe and can lead to undefined behavior.
/// In the case of C# operations, the return values will be in context of the C# type system,
/// not Go. Any Go code that has been converted to C# and is dependent on memory layout of Go
/// types will certainly not work as expected and could cause unexpected behavior.
/// </summary>
unsafe partial class unsafe_package  {

// ArbitraryType is here for the purposes of documentation only and is not actually
// part of the unsafe package. It represents the type of an arbitrary Go expression.
[GoType("num:nint")] partial struct ArbitraryType;

// IntegerType is here for the purposes of documentation only and is not actually
// part of the unsafe package. It represents any arbitrary integer type.
[GoType("num:nint")] partial struct IntegerType;

// Pointer represents a pointer to an arbitrary type. There are four special operations
// available for type Pointer that are not available for other types:
//   - A pointer value of any type can be converted to a Pointer.
//   - A Pointer can be converted to a pointer value of any type.
//   - A uintptr can be converted to a Pointer.
//   - A Pointer can be converted to a uintptr.
//
// Pointer therefore allows a program to defeat the type system and read and write
// arbitrary memory. It should be used with extreme care.
//
// The following patterns involving Pointer are valid.
// Code not using these patterns is likely to be invalid today
// or to become invalid in the future.
// Even the valid patterns below come with important caveats.
//
// Running "go vet" can help find uses of Pointer that do not conform to these patterns,
// but silence from "go vet" is not a guarantee that the code is valid.
//
// (1) Conversion of a *T1 to Pointer to *T2.
//
// Provided that T2 is no larger than T1 and that the two share an equivalent
// memory layout, this conversion allows reinterpreting data of one type as
// data of another type. An example is the implementation of
// math.Float64bits:
//
//	func Float64bits(f float64) uint64 {
//		return *(*uint64)(unsafe.Pointer(&f))
//	}
//
// (2) Conversion of a Pointer to a uintptr (but not back to Pointer).
//
// Converting a Pointer to a uintptr produces the memory address of the value
// pointed at, as an integer. The usual use for such a uintptr is to print it.
//
// Conversion of a uintptr back to Pointer is not valid in general.
//
// A uintptr is an integer, not a reference.
// Converting a Pointer to a uintptr creates an integer value
// with no pointer semantics.
// Even if a uintptr holds the address of some object,
// the garbage collector will not update that uintptr's value
// if the object moves, nor will that uintptr keep the object
// from being reclaimed.
//
// The remaining patterns enumerate the only valid conversions
// from uintptr to Pointer.
//
// (3) Conversion of a Pointer to a uintptr and back, with arithmetic.
//
// If p points into an allocated object, it can be advanced through the object
// by conversion to uintptr, addition of an offset, and conversion back to Pointer.
//
//	p = unsafe.Pointer(uintptr(p) + offset)
//
// The most common use of this pattern is to access fields in a struct
// or elements of an array:
//
//	// equivalent to f := unsafe.Pointer(&s.f)
//	f := unsafe.Pointer(uintptr(unsafe.Pointer(&s)) + unsafe.Offsetof(s.f))
//
//	// equivalent to e := unsafe.Pointer(&x[i])
//	e := unsafe.Pointer(uintptr(unsafe.Pointer(&x[0])) + i*unsafe.Sizeof(x[0]))
//
// It is valid both to add and to subtract offsets from a pointer in this way.
// It is also valid to use &^ to round pointers, usually for alignment.
// In all cases, the result must continue to point into the original allocated object.
//
// Unlike in C, it is not valid to advance a pointer just beyond the end of
// its original allocation:
//
//	// INVALID: end points outside allocated space.
//	var s thing
//	end = unsafe.Pointer(uintptr(unsafe.Pointer(&s)) + unsafe.Sizeof(s))
//
//	// INVALID: end points outside allocated space.
//	b := make([]byte, n)
//	end = unsafe.Pointer(uintptr(unsafe.Pointer(&b[0])) + uintptr(n))
//
// Note that both conversions must appear in the same expression, with only
// the intervening arithmetic between them:
//
//	// INVALID: uintptr cannot be stored in variable
//	// before conversion back to Pointer.
//	u := uintptr(p)
//	p = unsafe.Pointer(u + offset)
//
// Note that the pointer must point into an allocated object, so it may not be nil.
//
//	// INVALID: conversion of nil pointer
//	u := unsafe.Pointer(nil)
//	p := unsafe.Pointer(uintptr(u) + offset)
//
// (4) Conversion of a Pointer to a uintptr when calling functions like [syscall.Syscall].
//
// The Syscall functions in package syscall pass their uintptr arguments directly
// to the operating system, which then may, depending on the details of the call,
// reinterpret some of them as pointers.
// That is, the system call implementation is implicitly converting certain arguments
// back from uintptr to pointer.
//
// If a pointer argument must be converted to uintptr for use as an argument,
// that conversion must appear in the call expression itself:
//
//	syscall.Syscall(SYS_READ, uintptr(fd), uintptr(unsafe.Pointer(p)), uintptr(n))
//
// The compiler handles a Pointer converted to a uintptr in the argument list of
// a call to a function implemented in assembly by arranging that the referenced
// allocated object, if any, is retained and not moved until the call completes,
// even though from the types alone it would appear that the object is no longer
// needed during the call.
//
// For the compiler to recognize this pattern,
// the conversion must appear in the argument list:
//
//	// INVALID: uintptr cannot be stored in variable
//	// before implicit conversion back to Pointer during system call.
//	u := uintptr(unsafe.Pointer(p))
//	syscall.Syscall(SYS_READ, uintptr(fd), u, uintptr(n))
//
// (5) Conversion of the result of [reflect.Value.Pointer] or [reflect.Value.UnsafeAddr]
// from uintptr to Pointer.
//
// Package reflect's Value methods named Pointer and UnsafeAddr return type uintptr
// instead of unsafe.Pointer to keep callers from changing the result to an arbitrary
// type without first importing "unsafe". However, this means that the result is
// fragile and must be converted to Pointer immediately after making the call,
// in the same expression:
//
//	p := (*int)(unsafe.Pointer(reflect.ValueOf(new(int)).Pointer()))
//
// As in the cases above, it is invalid to store the result before the conversion:
//
//	// INVALID: uintptr cannot be stored in variable
//	// before conversion back to Pointer.
//	u := reflect.ValueOf(new(int)).Pointer()
//	p := (*int)(unsafe.Pointer(u))
//
// (6) Conversion of a [reflect.SliceHeader] or [reflect.StringHeader] Data field to or from Pointer.
//
// As in the previous case, the reflect data structures SliceHeader and StringHeader
// declare the field Data as a uintptr to keep callers from changing the result to
// an arbitrary type without first importing "unsafe". However, this means that
// SliceHeader and StringHeader are only valid when interpreting the content
// of an actual slice or string value.
//
//	var s string
//	hdr := (*reflect.StringHeader)(unsafe.Pointer(&s)) // case 1
//	hdr.Data = uintptr(unsafe.Pointer(p))              // case 6 (this case)
//	hdr.Len = n
//
// In this usage hdr.Data is really an alternate way to refer to the underlying
// pointer in the string header, not a uintptr variable itself.
//
// In general, [reflect.SliceHeader] and [reflect.StringHeader] should be used
// only as *reflect.SliceHeader and *reflect.StringHeader pointing at actual
// slices or strings, never as plain structs.
// A program should not declare or allocate variables of these struct types.
//
//	// INVALID: a directly-declared header will not hold Data as a reference.
//	var hdr reflect.StringHeader
//	hdr.Data = uintptr(unsafe.Pointer(p))
//	hdr.Len = n
//	s := *(*string)(unsafe.Pointer(&hdr)) // p possibly already lost
public class Pointer : StandardBox<uintptr>, IUnsafePointer {
    // The ZERO address IS the nil pointer: Go's `unsafe.Pointer(uintptr(0)) == nil` holds, and
    // every uintptr round-trip of a nil pointer lands here (the converter bridges an
    // unsafe.Pointer-valued call through uintptr because unsafe lives in its own assembly and can
    // carry no implicit conversion on the core pointer class). Marking the box nil while still
    // holding the address keeps that round-trip EXACT in both directions. Without it a reloaded
    // nil pointer came back non-nil, and sync's poolDequeue read every empty ring slot as
    // occupied — pushHead returned false forever and TestPoolDequeue/TestPoolChain spun.
    // `in`: for a non-`in` argument C# prefers a by-value candidate over an `in` one, which is what
    // lets a BOX argument bind the retaining constructor below while an explicit uintptr still binds
    // here -- a plain by-value pair is ambiguous (CS0121), a reference conversion to the interface
    // and a user-defined conversion to uintptr ranking as equal targets.
    public Pointer(in uintptr value) : base(value, value == 0)
    {
    }

    public Pointer(NilType _) : base(nil)
    {
    }

    // The mint the converter emits for `unsafe.Pointer(p)` over a managed Go pointer (`new
    // @unsafe.Pointer(p)`): the number is the box's stable address exactly as the uintptr overload
    // minted it. Identity (Equals/GetHashCode) holds for every pointee -- including one that carries
    // managed references and so has no pinnable storage, whose registered address can never validate
    // on read (the *bytes.Buffer of reflect's TestImplicitMapConversion) -- through the ORDER TOKEN
    // captured here, which two mints from one box share; it does NOT need the box retained, and the
    // paragraph below says what happened when this constructor briefly kept one. Overload resolution
    // prefers this constructor for any box argument (a reference conversion to the interface beats
    // the user-defined uintptr one); an explicit uintptr or void* argument still takes its own path,
    // numbers unchanged.
    // This constructor takes the ADDRESS from the box and RETAINS NOTHING, and the second half is as
    // load-bearing as the first. It is an exact match for `new @unsafe.Pointer(box)`, so it captures
    // every bare mint that previously bound the implicit `ж<T> -> uintptr` conversion -- and the bare
    // mint's contract is that it retains nothing (PointerMintRetentionTests' positive control; the
    // RETAINING door is `FromPinnedBox`, which the converter emits and which the syscall pin fix
    // depends on). Increment E3 root 4 passed the box here to give referent identity something to
    // compare for a pointee nothing pins; the root-4 AMENDMENT made identity the ORDER TOKEN instead,
    // which two mints from one box share whether or not either retains it, so the reason expired with
    // that commit and the argument is `null`. Measured one-axis, both directions: with the box the
    // control fails and with null it passes, while the ReflectValueSingles identity rows are
    // byte-identical to `go run` either way.
    //
    // What the constructor still earns its place for: a WRAPPER box reaches its address through
    // StableAddress (the plain `(uintptr)` conversion is not defined on a generated named-pointer
    // wrapper), and a structurally nil box mints the zero address rather than pinning anything.
    public Pointer(INilPointer box) : this(box is null || box.IsNilPointer ? (uintptr)0 : (uintptr)box.StableAddress(), null)
    {
    }

    // ---- referent retention (I5: the bare-unsafe.Pointer store/load-through seam) ----
    //
    // The mint for `unsafe.Pointer(&x)` flattens a managed pointer to its numeric address, and for
    // storage the collector may move, that number alone can neither keep the alias nor recover it —
    // which is how StorepNoWB lost its writes (the I5 ruling: the store landed in the argument
    // box's own uintptr slot, never the memory it names). The NativeBox §4 retention pattern
    // applied here closes that: a Pointer minted FROM a managed box CARRIES the box, so the
    // bare-unsafe.Pointer atomic primitives — the only surface that must WRITE or READ through a
    // bare Pointer — recover the referent and reach the very slot the pointer names. Retention
    // pins nothing and roots only what the minting frame already rooted, exactly
    // NativeBox.m_retainedSource's contract.
    private readonly object? m_retainedSource;

    private Pointer(uintptr value, object? retainedSource) : this(value)
    {
        m_retainedSource = retainedSource;
    }

    /// <summary>
    /// The managed box this pointer was minted from, when there is one — the recovery surface for
    /// the bare-<c>unsafe.Pointer</c> primitives (<see cref="StoreThrough"/>/<see cref="LoadThrough"/>).
    /// Null for numeric and native mints.
    /// </summary>
    public object? RetainedSource => m_retainedSource;

    // An unsafe.Pointer's VALUE is the address itself (a real pinned/native address), so its
    // pointer-order token is that address — Go's Value.Pointer() ordering (internal/fmtsort's
    // unsafe.Pointer map-key ordering) reads through unchanged: same-array element addresses
    // ascend by element exactly as in Go.
    //
    // Every IsNull in this class is the STRUCTURAL nil question despite its name: the pointee type
    // is `uintptr`, a value type, so ж<uintptr>'s value-peeking arm can never fire and IsNull is
    // exactly IsNilPointer here (see ж<T>.IsNull). Marking the box nil while it still HOLDS the zero
    // address is what makes the uintptr round-trip of a nil pointer exact — the ctor above.
    public override nuint PointerOrderToken => IsNull ? 0 : Value.Value;

    // ...and because the VALUE is the address, IDENTITY is the address too. That is Go's rule for
    // this one type and it is the reason both members below exist: `ж<T>` compares and hashes a
    // heap box BY REFERENCE (the box IS the storage it names, so two boxes are two addresses),
    // which is right for every other pointer and wrong for exactly this one — an unsafe.Pointer
    // CARRIES an address rather than being one, and the converter mints a fresh box on every
    // `uintptr → unsafe.Pointer` conversion (875 emitted call sites). Two boxes over one address
    // are therefore ONE Go pointer, and reference identity called them different.
    //
    // The measured consumer is any unsafe.Pointer used as a MAP KEY, which is how Go's own
    // cycle detectors are written: encoding/json's encoder stores `e.ptrSeen[v.UnsafePointer()]`
    // and looks it up again on the next level down, so with per-box identity it never found the
    // entry it had just written, no cycle was ever detected, and marshalling a self-referential
    // map or slice recursed until the process died of stack exhaustion instead of returning Go's
    // `UnsupportedValueError: encountered a cycle`. Both members answer through
    // PointerOrderToken, so equality, hashing and ordering are one fact about the address rather
    // than three that can drift apart — and a nil pointer, whichever of its two representations,
    // tokens 0 and so equals and hashes with the other.
    // Overriding the VIRTUAL ж<T>.Equals rather than adding an operator or an object-Equals arm is
    // what makes all four question forms answer once: `p1 == p2` (the base `==` calls it),
    // `p1.Equals(p2)`, `((object)p1).Equals(p2)`, and a Dictionary/golib-map lookup keyed on the
    // boxed pointer. An `operator ==(Pointer, Pointer)` would additionally have made every existing
    // `uintptr == unsafe.Pointer` comparison AMBIGUOUS (both operands convert to both sides —
    // measured as CS0034 in runtime's map.cs and mfinal.cs), so the operator route is not merely
    // redundant, it is unavailable.
    // IDENTITY IS THE REFERENT (increment E3 root 4, 2026-09-05). Two Pointers over ONE box can carry
    // two different NUMBERS: reflect's Value.UnsafePointer() mints the box's stable identity token
    // (what %p prints, what Value.Pointer() orders by, what ManagedPointerTokens resolves back to the
    // box for the (*T)(unsafe.Pointer(v.UnsafePointer())) round trip), while the converter's mint for
    // `unsafe.Pointer(p)` is FromBox -- the transient address of the box's value slot, the box retained.
    // Comparing the numbers made `mv.MapIndex(k).Elem().UnsafePointer() != unsafe.Pointer(b2)` true for
    // the very box the map returned (reflect's TestImplicitMapConversion #5/#7). So when BOTH sides
    // resolve a referent -- the retained box, or the box a registered token or pinned base address
    // names -- equality is ReferenceEquals of the referents; otherwise it is the number, exactly as
    // before. An interior address (unsafe.Add, the indexers) is never registered and never resolves,
    // so it keeps the numeric rule. Two FromBox mints of one box are now equal even across a GC move;
    // two different boxes whose transient addresses ever coincide are now unequal. The probes run
    // only while reflect has registered a token (ManagedPointerTokens' fast path is unchanged for
    // every program that never projected a pointer), and never for the nil operators above.
    //
    // THE HASH CONTRACT, and the hole it accepts (COORD's condition on the cut). A pointer hashes by
    // its OWN referent when it resolves one, by the number otherwise. Both-resolve and neither-resolve
    // pairs therefore hash alike whenever they are equal. The ASYMMETRIC pair -- exactly one side
    // resolves and the two numbers are equal -- is equal by the number while the hashes come from
    // different sources, and is accepted deliberately: it needs a wild number that coincides with a
    // live box's token or pinned address, which is the shape the resolution seam exists to refuse,
    // and it shrinks further once every reference-bearing box registers (Q44's token). A map keyed
    // on such a pair is the population the corpus census posted with this cut measures.
    public override bool Equals(ж<uintptr>? other)
    {
        if (other is not Pointer pointer)
            return base.Equals(other);
        if (Referent is { } mine && pointer.Referent is { } theirs)
            return ReferenceEquals(mine, theirs) || ReferentToken(mine) == ReferentToken(theirs);
        return PointerOrderToken == pointer.PointerOrderToken;
    }

    public override int GetHashCode()
    {
        return Referent is { } referent ? ReferentToken(referent).GetHashCode() : PointerOrderToken.GetHashCode();
    }

    // A referent's identity is its ORDER TOKEN, never the box OBJECT: a field or element reference is
    // minted afresh at every `&l.p` / `&a[i]`, so two boxes over ONE field are two objects with one
    // token (allocation base + Go field offset -- "equal pointers always produce equal tokens", the
    // contract PointerOrderToken documents), while a heap box's token is its own allocation base.
    // Comparing the objects said `unsafe.Pointer(&l.p) != unsafe.Pointer(&l.p)` where Go and the
    // numeric rule both say equal (the ManagedAtomicPointer behavioral guard, full-suite run of the
    // referent cut). Hashing by the same token keeps Equals and GetHashCode one fact.
    private static nuint ReferentToken(object referent)
    {
        return referent is INilPointer box ? box.PointerOrderToken : (nuint)(uint)RuntimeHelpers.GetHashCode(referent);
    }

    /// <summary>
    /// The managed box this pointer names, when one can be recovered: the retained source of a
    /// <see cref="FromBox"/> mint, else the box a registered identity token or pinned base address
    /// resolves to (validate-on-read, so a stale or reused number never answers). Null for a purely
    /// numeric or native pointer and for every interior address.
    /// </summary>
    public object? Referent => IsNull ? null : ResolveReferent();

    public Pointer this[int index] => Value + (uintptr)index;

    public Pointer this[nint index] => Value + (uintptr)index;

    public Pointer this[Range range]
    {
        get
        {
            if (range.End.Value >= 0)
                throw new IndexOutOfRangeException($"End of range not supported for '{nameof(Pointer)}' indexing -- length is not known");

            return Value + (uintptr)range.Start.Value;
        }
    }

    // Enable comparisons between nil and ж<T> instance
    public static bool operator ==(Pointer? value, NilType _)
    {
        return value?.IsNull ?? true;
    }

    public static bool operator !=(Pointer? value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, Pointer? value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, Pointer? value)
    {
        return value != nil;
    }
    
    public static implicit operator Pointer(NilType _)
    {
        return new Pointer(nil);
    }

    public static implicit operator Pointer(uintptr value) {
        return new Pointer(value);
    }

    public static implicit operator uintptr(Pointer value) {
        // A nil unsafe.Pointer can be modeled EITHER as a C# null reference or as a nil-constructed
        // box (the ==/!= operators above treat both as nil); its uintptr value is 0 in both cases.
        // Reading .Value off a nil-constructed box would instead panic with a nil dereference —
        // which is what `atomic.StorePointer(&slot.typ, nil)` followed by a reload used to do.
        return value is null || value.IsNull ? (uintptr)0 : value.Value;
    }

    public static implicit operator Pointer(void* value) {
        return new Pointer((uintptr)value);
    }

    public static implicit operator void*(Pointer value) {
        // Same nil tolerance as the uintptr bridge above: a nil-constructed box has no value to
        // read (.Value panics), and a nil pointer's native form is the null address.
        return value is null || value.IsNull ? null : (void*)value.Value;
    }

    public static Pointer FromRef<T>(ref T type)
    {
        fixed (T* ptr = &type)
            return new Pointer((uintptr)ptr);
    }

    // The converter's mint for `unsafe.Pointer(p)` where p is a managed Go pointer (`ж<T>`): the
    // numeric value is byte-compatible with the FromRef form it replaces (the transient address of
    // the storage the box names — still not GC-stable, the same caveat as every
    // unsafe.Pointer-as-number use), and the BOX rides along so the bare-Pointer primitives can
    // recover the alias the number cannot carry. Nil is the 0 address rather than FromRef's
    // nil-deref panic — Go's `unsafe.Pointer(nil)` — the same fix the pointer-parameter emission
    // took for the syscall wrappers' idiomatic nil out-pointers.
    public static Pointer FromBox<T>(ж<T> box)
    {
        if (box is null || box.IsNilPointer)
            return new Pointer(nil);

        // A native alias's number IS its meaning — carry it exactly; the retained box still rides
        // along so a store through it reaches the aliased memory via the box's own slot access.
        if (box.NativeAddress != 0)
            return new Pointer((uintptr)box.NativeAddress, box);

        fixed (T* ptr = &box.ValueSlot)
            return new Pointer((uintptr)ptr, box);
    }

    // The converter's mint for `unsafe.Pointer(&x)` — an address TAKEN, as opposed to FromBox's
    // pointer VALUE carried across. The difference is the pin, and it is the whole reason this door
    // exists beside FromBox: `fixed` holds storage still for its own statement, while an address
    // handed to a native callee outlives that statement by definition — a blocking `read(2)` is the
    // extreme case, microseconds to seconds wide with the kernel writing the whole time.
    //
    // So the address comes from the box's own uintptr conversion, which is the PIN MOMENT
    // (`ж<T>.EnsureStableAddress` stores a GCHandle in the box's own field, and the conversion
    // registers the provenance record every resolve leans on) — and the box is RETAINED, because a
    // pin whose holder is unreachable is a pin the finalizer releases while the address is still in
    // flight. That was the defect this door was cut for (2026-09-04): `new Pointer(box)` binds the
    // implicit ж→uintptr conversion into `Pointer(uintptr)`, which pins and then retains nothing, so
    // the buffer of every converted `read`/`write` was relocatable under the kernel's own write —
    // SIGSEGV in five seconds under sixteen concurrent TLS connections, gone when the box is held.
    //
    // Nil, native aliases and fixed-array data addresses are all the conversion's own answers rather
    // than a second policy here; that is the point of routing through it.
    public static Pointer FromPinnedBox<T>(ж<T> box)
    {
        if (box is null)
            return new Pointer(nil);

        return new Pointer((uintptr)box, box);
    }

    // ---- the bare-unsafe.Pointer primitives' recovery and through-ops (I5) ----

    // The referent this pointer can store/load through: the retained box first; else whatever the
    // provenance/token registry can recover for the number (a pointer-parameter mint's pinned
    // registration, a reflect projection's token) — validate-on-read protected, so a stale entry
    // never answers.
    private object? ResolveReferent()
    {
        if (m_retainedSource is not null)
            return m_retainedSource;

        return IsNull ? null : ManagedPointerTokens.Resolve(Value.Value);
    }

    /// <summary>
    /// Performs Go's <c>*(*unsafe.Pointer)(ptr) = val</c> — the store the bare-Pointer atomic
    /// primitives (<c>StorepNoWB</c>) are defined as. Panics, loudly and by name, when this
    /// pointer carries no recoverable managed referent or the named slot cannot hold the value:
    /// the silent alternative is a lost write into a fresh box's own slot (the I5 probe), which
    /// is exactly what this member exists to end.
    /// </summary>
    public void StoreThrough(Pointer? val)
    {
        if (ResolveReferent() is not IUntypedSlotAccess slot)
            throw panic("unsafe.Pointer store-through: the pointer carries no recoverable managed referent (raw-address stores are not part of the managed model)");

        if (val is null || val.IsNull)
        {
            // The nil store: a pointer-typed slot takes the null form; a uintptr-shaped slot
            // (reached through reinterpret games) takes the zero address.
            if (slot.TryStoreThrough(null) || slot.TryStoreThrough((uintptr)0))
                return;
        }
        else
        {
            // The slot's own type selects from the candidates: the retained referent serves a
            // `*T` location, the Pointer itself a `*unsafe.Pointer` location, a registry recovery
            // the pointer-parameter mints, and the raw number a `*uintptr` location.
            if (val.RetainedSource is { } referent && slot.TryStoreThrough(referent))
                return;

            if (slot.TryStoreThrough(val))
                return;

            if (ManagedPointerTokens.Resolve(val.Value.Value) is { } resolved && slot.TryStoreThrough(resolved))
                return;

            if (slot.TryStoreThrough(val.Value))
                return;
        }

        throw panic("unsafe.Pointer store-through: the named slot cannot hold the stored pointer value");
    }

    /// <summary>
    /// Performs Go's <c>*(*unsafe.Pointer)(ptr)</c> — the load the bare-Pointer atomic primitives
    /// (<c>Loadp</c>) are defined as. Same recovery, same loud residual, as
    /// <see cref="StoreThrough"/>.
    /// </summary>
    public Pointer LoadThrough()
    {
        if (ResolveReferent() is not IUntypedSlotAccess slot)
            throw panic("unsafe.Pointer load-through: the pointer carries no recoverable managed referent (raw-address loads are not part of the managed model)");

        if (!slot.TryLoadThrough(out object? value))
            throw panic("unsafe.Pointer load-through: nil pointer dereference");

        return value switch
        {
            null => new Pointer(nil),
            Pointer p => p,
            uintptr u => new Pointer(u),
            INilPointer referent => FromReferent(referent),
            _ => throw panic("unsafe.Pointer load-through: the named slot does not hold a pointer-shaped value"),
        };
    }

    // Wraps a loaded managed pointer as an unsafe.Pointer without knowing its T: the scalar is the
    // referent's own order token, REGISTERED so the numeric round-trip a typed cast takes —
    // `(*T)(loaded)` emits `(ж<T>)(uintptr)(…)` — resolves back to the very box (MintOpaque's
    // pattern, the fourth minter); the retention slot carries the recovery for the primitives
    // themselves.
    private static Pointer FromReferent(INilPointer referent)
    {
        if (referent.IsNilPointer)
            return new Pointer(nil);

        nuint token = referent.PointerOrderToken;

        if (token == 0)
            return new Pointer(nil);

        ManagedPointerTokens.Register(token, referent);
        return new Pointer((uintptr)token, referent);
    }
}

// Sizeof takes an expression x of any type and returns the size in bytes
// of a hypothetical variable v as if v was declared via var v = x.
// The size does not include any memory possibly referenced by x.
// For instance, if x is a slice, Sizeof returns the size of the slice
// descriptor, not the size of the memory referenced by the slice;
// if x is an interface, Sizeof returns the size of the interface value itself,
// not the size of the value stored in the interface.
// For a struct, the size includes any padding introduced by field alignment.
// The return value of Sizeof is a Go constant if the type of the argument x
// does not have variable size.
// (A type has variable size if it is a type parameter or if it is an array
// or struct type with elements of variable size).
//
// The converter FOLDS this call to a Go constant wherever go/types can compute one — which is
// every operand Go itself calls fixed-size, 283 corpus sites emitted as `/* unsafe.Sizeof(x) */ 8`.
// What actually reaches this body is the residue Go's own spec calls VARIABLE size: an operand
// whose type is a type parameter. So T here is almost always bound to something managed —
// `ж<Section>`, an interface, a struct holding a reference — and `Marshal.SizeOf<T>` answers
// none of them: it computes an unmanaged MARSHALLING size, which is unrelated to Go's number
// (Go's bool is 1 byte where marshalling says 4), and for a generic type or a type with no
// unmanaged form it does not answer at all, it THROWS. Three packages reached this body through
// internal/saferio.SliceCap[E] and died on that exception (debug/macho, internal/xcoff, and
// go/internal/gccgoimporter through debug/elf).
//
// GoReflect.GoSizeOf is the one Go layout rule the reflection bridge already computes descriptor
// Size_ from, so answering here through it makes unsafe.Sizeof and reflect.Type.Size() the same
// rule rather than two — the unification GoReflect.TypeLayout.cs recorded as deferred pending a
// named consumer. Dims come from the live value because array<T> carries its Go length in the
// INSTANCE. Marshal.SizeOf remains the fallback for the shapes GoSizeOf declines (-1), so no
// operand that resolves today stops resolving.
public static uintptr Sizeof<T>(T x) {
    // TryGoSizeOf answers derivability separately from the size, which matters here: the old
    // signed form reported a type of 2^63 bytes and up as -1, indistinguishable from
    // "unknown", and this line then answered with Marshal.SizeOf<T>() -- a DIFFERENT layout
    // model's number for a type whose Go size was in fact known exactly. The marshalled
    // fallback now applies only where the Go size genuinely is not derivable.
    return GoReflect.TryGoSizeOf(typeof(T), GoReflect.ArrayDimsOfValue(x), out nuint size)
        ? (uintptr)size
        : (uintptr)Marshal.SizeOf<T>();
}

// Offsetof returns the offset within the struct of the field represented by x,
// which must be of the form structValue.field. In other words, it returns the
// number of bytes between the start of the struct and the start of the field.
// The return value of Offsetof is a Go constant if the type of the argument x
// does not have variable size.
// (See the description of [Sizeof] for a definition of variable sized types.)
// go2cs conversion resolves the operand through go/types and converts:
// `unsafe.Offsetof(structValue.field)` to
// `@unsafe.Offsetof(typeof(StructType), "field")`
// A promoted field is measured against the struct that DECLARES it, per Go's
// rule that the offset is relative to the immediately enclosing struct.
public static uintptr Offsetof(Type structType, string fieldName) {
    return (uintptr)Marshal.OffsetOf(structType, fieldName);
}

// Alignof takes an expression x of any type and returns the required alignment
// of a hypothetical variable v as if v was declared via var v = x.
// It is the largest value m such that the address of v is always zero mod m.
// It is the same as the value returned by [reflect.TypeOf](x).Align().
// As a special case, if a variable s is of struct type and f is a field
// within that struct, then Alignof(s.f) will return the required alignment
// of a field of that type within a struct. This case is the same as the
// value returned by [reflect.TypeOf](s.f).FieldAlign().
// The return value of Alignof is a Go constant if the type of the argument
// does not have variable size.
// (See the description of [Sizeof] for a definition of variable sized types.)
// go2cs conversion resolves the operand's STATIC type through go/types and converts
// `unsafe.Alignof(x)` to `@unsafe.Alignof(typeof(T))` for every operand shape —
// including `unsafe.Alignof(s.f)`, whose answer is the alignment of the field's own
// type, which is exactly what the fieldName overload below resolves to. That overload
// is retained for hand-written callers.
public static uintptr Alignof(Type type, string? fieldName = null) {
    // Handle the special case for struct fields
    if (fieldName is not null && type is { IsValueType: true, IsPrimitive: false })
    {
        // Find the specified field
        FieldInfo? field = type.GetField(fieldName, 
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic);

        if (field is not null)
        {
            // Get the field type and determine its alignment
            Type fieldType = field.FieldType;

            // Call Alignof on the field type (without a fieldName parameter)
            // This would require a non-generic version since we can't pass fieldType to a generic method directly
            return AlignofType(fieldType);
        }

        // If field not found, fall back to normal behavior
    }

    // Basic primitive type alignment rules similar to how Go handles them
    if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
        return 1;

    if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
        return 2;

    if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
        return 4;

    if (type == typeof(long) || type == typeof(ulong) || type == typeof(double) || type == typeof(nint) || type == typeof(nuint))
        return 8;

    // For structs, get the largest alignment of any field
    if (type is
    {
        IsValueType: true,
        IsPrimitive: false
    })
    {
        uintptr maxAlignment = 1;

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            uintptr fieldAlignment;

            Type fieldType = field.FieldType;

            // Recursively get alignment for field types
            // This would need to call Alignof for the field type
            // We'll use a simplified approach here
            if (fieldType == typeof(bool) || fieldType == typeof(byte) || fieldType == typeof(sbyte))
                fieldAlignment = 1;
            else if (fieldType == typeof(char) || fieldType == typeof(short) || fieldType == typeof(ushort))
                fieldAlignment = 2;
            else if (fieldType == typeof(int) || fieldType == typeof(uint) || fieldType == typeof(float))
                fieldAlignment = 4;
            else if (fieldType == typeof(long) || fieldType == typeof(ulong) || fieldType == typeof(double) || fieldType == typeof(nint) || fieldType == typeof(nuint))
                fieldAlignment = 8;
            else if (fieldType.IsClass)
                fieldAlignment = (uintptr)nint.Size; // Reference types are pointer-aligned
            else
                fieldAlignment = 8; // Default for complex types

            maxAlignment = Math.Max(maxAlignment, fieldAlignment);
        }

        return maxAlignment;
    }

    // For reference types (classes), return the pointer size
    if (type.IsClass)
        return (uintptr)nint.Size;

    // For arrays, return the alignment of the element type
    if (type.IsArray)
    {
        Type? elementType = type.GetElementType();

        // This would ideally call Alignof for the element type
        // For simplicity, we'll use a fixed alignment based on element size
        if (elementType == typeof(bool) || elementType == typeof(byte) || elementType == typeof(sbyte))
            return 1;
        if (elementType == typeof(char) || elementType == typeof(short) || elementType == typeof(ushort))
            return 2;
        if (elementType == typeof(int) || elementType == typeof(uint) || elementType == typeof(float))
            return 4;
        if (elementType == typeof(long) || elementType == typeof(ulong) || elementType == typeof(double))
            return 8;

        return (uintptr)nint.Size; // Default alignment for complex element types
    }

    // Default alignment for unknown types
    return (uintptr)nint.Size;
}

private static uintptr AlignofType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] Type type)
{
    // Basic primitive type alignment rules similar to how Go handles them
    if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte))
        return 1;

    if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
        return 2;

    if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
        return 4;

    if (type == typeof(long) || type == typeof(ulong) || type == typeof(double) || type == typeof(nint) || type == typeof(nuint))
        return 8;

    // For structs, get the largest alignment of any field
    if (type is
    {
        IsValueType: true,
        IsPrimitive: false
    })
    {
        uintptr maxAlignment = 1;

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            uintptr fieldAlignment = AlignofType(field.FieldType);
            maxAlignment = Math.Max(maxAlignment, fieldAlignment);
        }

        return maxAlignment;
    }

    // For reference types (classes), return the pointer size
    if (type.IsClass)
        return (uintptr)nint.Size;

    // For arrays, return the alignment of the element type
    if (type.IsArray)
    {
        Type? elementType = type.GetElementType();

         if (elementType is null)
             return (uintptr)nint.Size;

         return AlignofType(elementType);
    }

    // Default alignment for unknown types
    return (uintptr)nint.Size;
}

// The function Add adds len to ptr and returns the updated pointer
// [Pointer](uintptr(ptr) + uintptr(len)).
// The len argument must be of integer type or an untyped constant.
// A constant len argument must be representable by a value of type int;
// if it is an untyped constant it is given type int.
// The rules for valid uses of Pointer still apply.
// uintptr forwards through its inner value (the golib struct does not implement the
// IBinaryInteger generic-math surface the TLen constraint requires — CS0315)
public static ж<T> Add<T>(ж<T> ptr, uintptr len) {
    return Add(ptr, len.Value);
}

// Go's unsafe.Add takes an unsafe.Pointer and offsets it by len BYTES. unsafe.Pointer models the
// address as its VALUE (not as managed storage it points at), so the generic ж<T> overload below —
// which resolves a managed array-element reference — cannot serve it: it found no array ref and
// returned a NIL pointer, so walking a native block (syscall.Environ stepping through the
// GetEnvironmentStringsW block) dereferenced address 0 on the first step. This overload is the
// faithful one for the pointer type Go actually requires here, and being more derived it wins
// overload resolution over ж<uintptr>.
public static Pointer Add(Pointer ptr, uintptr len) {
    return Add(ptr, len.Value);
}

public static Pointer Add<TLen>(Pointer ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    if (ptr == nil)
        return new Pointer(nil);

    return new Pointer(ptr.Value + (uintptr)(nuint)nint.CreateTruncating(len));
}

public static ж<T> Add<T, TLen>(ж<T> ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    // Go's len is of any integer type (the `IntegerType` constraint); reduce it to the pointer
    // offset type. A signed/unsigned/native-width argument (e.g. a `uintptr`) thus all bind here.
    int n = int.CreateTruncating(len);

    if (ptr == nil)
        return new StandardBox<T>(nil);

    // A pointer that ALIASES a native address offsets that address directly. Go's unsafe.Add is
    // byte arithmetic (its argument is an unsafe.Pointer), which is what a native alias reproduces;
    // the managed array-element path below is an element step against a managed backing store.
    if (ptr.IsNative)
        return new NativeBox<T>((nuint)((nint)ptr.NativeAddress + n));

    (IArray array, int index)? arrayRef  = ptr.ArrayRef;

    if (arrayRef is null)
        return new StandardBox<T>(nil);

    (IArray array, int index) = arrayRef.Value;

    return new ElemRefBox<T>(array, index + n);
}

// The function Slice returns a slice whose underlying array starts at ptr
// and whose length and capacity are len.
// Slice(ptr, len) is equivalent to
//
//	(*[len]ArbitraryType)(unsafe.Pointer(ptr))[:]
//
// except that, as a special case, if ptr is nil and len is zero,
// Slice returns nil.
//
// The len argument must be of integer type or an untyped constant.
// A constant len argument must be non-negative and representable by a value of type int;
// if it is an untyped constant it is given type int.
// At run time, if len is negative, or if ptr is nil and len is not zero,
// a run-time panic occurs.
// A uintptr length forwards through its inner value: golib's uintptr STRUCT does not (and
// deliberately will not) implement the IBinaryInteger generic-math surface the TLen
// constraint requires (CS0315); the non-generic-length overload is preferred by resolution.
public static slice<T> Slice<T>(ж<T> ptr, uintptr len) {
    return Slice(ptr, len.Value);
}

public static slice<T> Slice<T, TLen>(ж<T> ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    // Go's len is of any integer type (the `IntegerType` constraint); reduce it to int.
    int n = int.CreateTruncating(len);

    if (n < 0)
        throw panic("len is negative");

    if (ptr == nil)
    {
        if (n == 0)
            return [];

        throw panic("ptr is nil and len is not zero");
    }

    // A pointer that ALIASES a GENUINELY NATIVE address yields a NATIVE-BACKED slice over that
    // memory — Go's unsafe.Slice semantics exactly: writes reach the memory, element addresses
    // are the real ones (Mprotect(b[:n]) hands the kernel the mapping), and lifetime is the
    // mapping's own — a claim that is TRUE only because of the provenance consult below. This
    // retires the documented snapshot limitation that made syscall.Mmap return twelve kilobytes
    // that were not the mapping (DESIGN-native-backed-slice.md, the W1b commission); read-only
    // consumers like syscall.Environ ride the same arm and simply never write. Unmanaged T is
    // enforced at the creation door with a named panic.
    //
    // The provenance consult (DESIGN-pointer-provenance §3; AUDIT-unsafe-slice-provenance is the
    // 53-site census behind it): IsNative answers "does this box CARRY an address", not "is that
    // address native" — an EnsureStableAddress round trip stamps a PINNED MANAGED object's
    // address into the same field, and a native-backed slice over managed storage retains
    // nothing that holds the object still, so the pin can release and the GC can move what the
    // slice keeps addressing. Resolve is the discriminator the mechanism ratified: a HIT means a
    // live, still-pinned-there managed registration (validate-on-read), so the pointer falls
    // through to the MANAGED arms below — the element-window arm aliases the pinned box's own
    // storage, which stays correct whether or not the pin ever releases. A MISS means genuinely
    // native (every pin registers; the audit's five uintptr-shaped watch sites never do, so they
    // surface at first read, named, rather than as corruption), and the mapping arm proceeds
    // exactly as before for the audit's 13 native sites, mmap first among them.
    if (ptr.IsNative && ManagedPointerTokens.Resolve(ptr.NativeAddress) is null)
        return slice<T>.OverNativeMemory(ptr.NativeAddress, n);

    // A pointer INTO managed array/slice storage (`unsafe.Slice(&s[i], n)`) yields a window that
    // ALIASES that storage, exactly as Go's unsafe.Slice does: writes through the rebuilt slice must
    // reach the original backing. The snapshot below silently swallowed them — crypto/subtle's
    // xorBytes rebuilds dst/x/y from `&dst[0]` and writes every XOR result through dst, so XORBytes
    // wrote NOTHING (its whole test matrix compared dst against its untouched 0xdd fill).
    if (ptr.TryGetElementWindow(n, out slice<T> window))
        return window;

    // No aliasable managed storage — a heap box, a struct field, or a REINTERPRETING pointer over a
    // differently-typed array (a T[] view over another element type does not exist in the managed
    // model). Reading the n elements through the pinned referent is still exact; writes through the
    // result do not reach the source.
    fixed (T* pointer = &ptr.Value)
        return new slice<T>(new ReadOnlySpan<T>(pointer, n));
}

// SliceData returns a pointer to the underlying array of the argument
// slice.
//   - If cap(slice) > 0, SliceData returns &slice[:1][0].
//   - If slice == nil, SliceData returns nil.
//   - Otherwise, SliceData returns a non-nil pointer to an
//     unspecified memory address.
public static ж<T> SliceData<T>(slice<T> slice) {
    if (slice == nil)
        return new StandardBox<T>(nil);

    // Go DEFINES this as `&slice[:1][0]` — an INTERIOR POINTER into the slice's own backing store —
    // so the faithful model is the array-element reference `Ꮡ(s, 0)`, which is exactly what the
    // converter emits for `&s[0]`. Three things follow that the previous pinned-buffer box got wrong:
    //
    //   1. It PINNED. `slice.buffer` runs GCHandle.Alloc(…, Pinned), which throws `ArgumentException:
    //      Object contains references` for any element type carrying a managed reference. Pinning was
    //      never what SliceData means — an address is only needed when the pointer is CONVERTED to
    //      uintptr/void*, and ж already pins on demand there (EnsureStableAddress), gracefully
    //      declining for storage that cannot be held still. log/slog's GroupValue is the witness:
    //      `groupptr(unsafe.SliceData(as))` over `[]Attr` is pure interior-pointer identity — the
    //      pointer plus len(as) IS the group's two-word representation, rebuilt by `unsafe.Slice`
    //      in Value.group() — and it took down every grouping path in the package.
    //   2. It ignored the slice's LOW bound. The pin was over the whole backing array from index 0,
    //      so `SliceData(s[2:])` addressed element 0 rather than element 2. CanonicalElement resolves
    //      an element reference through the header (backing + Low + index), so the derived pointer now
    //      names the element Go's does — and compares equal to `&s[0]`, as Go's pointer identity requires.
    //   3. It was undereferenceable for any element type but byte. PinnedBuffer implements
    //      IArray<byte> alone, so ж<T>.Value's `array is IArray<T>` test failed for every other T and
    //      threw InvalidOperationException instead of reading the element.
    //
    // The element reference also makes the round trip ALIAS rather than snapshot: unsafe.Slice's
    // TryGetElementWindow arm rebuilds a window over the original backing, so a write through the
    // rebuilt slice reaches the source — Go's semantics. Guarded by the UnsafeStringEmpty and
    // UnsafeSliceDataAliasing behavioral tests.
    return Ꮡ(slice, 0);
}

// String returns a string value whose underlying bytes
// start at ptr and whose length is len.
//
// The len argument must be of integer type or an untyped constant.
// A constant len argument must be non-negative and representable by a value of type int;
// if it is an untyped constant it is given type int.
// At run time, if len is negative, or if ptr is nil and len is not zero,
// a run-time panic occurs.
//
// Since Go strings are immutable, the bytes passed to String
// must not be modified as long as the returned string value exists.
// uintptr forwards through its inner value (see Add overload note)
public static @string String(ж<byte> ptr, uintptr len) {
    return String(ptr, len.Value);
}

public static @string String<TLen>(ж<byte> ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    // Go's len is of any integer type (the `IntegerType` constraint); reduce it to int.
    int n = int.CreateTruncating(len);

    if (n < 0)
        throw panic("len is negative");

    // A zero length reads no bytes, so the pointer is never dereferenced — in Go, and now here
    // either. SliceData over a non-nil slice of capacity 0 is documented to return a NON-nil
    // pointer to an unspecified address, which this model materializes as an index-0 box into a
    // zero-length backing array: pinning its referent below is an IndexOutOfRangeException rather
    // than a read of nothing. syscall.UTF16ToString reaches exactly that — it truncates at the
    // first NUL, so an all-NUL WCHAR buffer decodes through unsafe.String(SliceData(empty), 0),
    // and every unset [N]uint16 field of a Win32 record is one of those. Guarded by the
    // UnsafeStringEmpty behavioral test.
    if (n == 0)
        return [];

    // Only a nil pointer with a NON-zero length is the panic Go specifies.
    if (ptr == nil)
        throw panic("ptr is nil and len is not zero");

    // A pointer that ALIASES a native address reads its n bytes from that address (see Slice above).
    // @string has no native-backed representation — its header is (byte[], offset, length) — so this
    // arm COPIES and is the family's one remaining snapshot. It is reachable only from a NativeBox:
    // ж<T>.NativeAddress is virtual returning 0 and ElemRefBox does not override it, so an element
    // reference is never IsNative and can never be answered here.
    if (ptr.IsNative)
        return new @string(new ReadOnlySpan<byte>((void*)ptr.NativeAddress, n));

    // A pointer INTO managed byte storage (`unsafe.String(&b[i], n)`) yields a string that ALIASES
    // that storage, exactly as Go's unsafe.String does: the string's bytes ARE the pointed-to
    // memory, so a write through the pointer is visible in the string. Go states the property as a
    // prohibition — "the bytes passed to String must not be modified as long as the returned string
    // value exists" — and a prohibition is only meaningful because the aliasing is OBSERVABLE.
    //
    // This is the exact mirror of Slice's element-window arm above (same TryGetElementWindow, same
    // absolute-index bounds), and until it existed unsafe.String was the ONE member of the family
    // that snapshotted: Slice, SliceData and StringData all alias, and SliceData's own note already
    // claimed "the round trip through unsafe.String/unsafe.Slice now ALIASES instead of
    // snapshotting" — true of Slice, false here. What the copy cost, concretely:
    //
    //   * runtime.rawstring is DEFINED to return a string and a byte slice "referring to the same
    //     storage" so the caller can fill the slice and the string see it; over a copy the string
    //     is whatever the storage held before the caller wrote anything.
    //   * runtime.slicebytetostringtmp IS Go's aliasing temporary (golib's own builtin.tmpstring
    //     models the same optimization and always aliased); the converted body did not.
    //   * runtime's TestPinnerCgoCheckString pins &b[0] and then requires the string built from it
    //     to name that same pinned object — a copy names a fresh, unpinned allocation
    //     (DESIGN-runtime-pinner.md §6.2, which named and priced this defect).
    //
    // The window is minted through @string's own aliasing factory, which shares its body with the
    // map-index temporary: @string's instance state stays exactly the Go header (backing, offset,
    // length), so aliasing costs +0 B per string. Guarded by GolibTests' UnsafeStringAliasingTests
    // — four aliasing arms and four invariance arms, the latter pinning that the zero-length
    // early-out above still precedes any dereference and that the snapshot arm below survives.
    if (ptr.TryGetElementWindow(n, out slice<byte> window))
        return @string.AliasOf(window);

    // No aliasable managed storage — a heap box, a struct field, or a REINTERPRETING pointer over a
    // differently-typed array. Reading the n bytes through the pinned referent is still exact;
    // writes through the source do not reach the result. Same documented snapshot arm as Slice's.
    fixed (byte* pointer = &ptr.Value)
        return new @string(new ReadOnlySpan<byte>(pointer, n));
}

// StringData returns a pointer to the underlying bytes of str.
// For an empty string the return value is unspecified, and may be nil.
//
// Since Go strings are immutable, the bytes returned by StringData
// must not be modified.
public static ж<byte> StringData(@string str) {
    // Go returns nil for an empty string (the doc leaves it unspecified, but the runtime's
    // zero string has a nil data pointer and strings' TestClone asserts StringData identity
    // across DISTINCT empty strings) — each call materializes a fresh view, so only nil
    // preserves that identity.
    if (str.Length == 0)
        return new StandardBox<byte>(nil);

    // Go DEFINES this as `&str[0]` — an INTERIOR POINTER into the string's own backing store — so
    // the faithful model is the array-element reference `Ꮡ(window, 0)`, exactly as SliceData above.
    // It used to hand back a box over a PINNED view (`@string.buffer`), and that was wrong for the
    // same two reasons the pinned SliceData box was, plus a third that only a finalizer reaches:
    //
    //   1. It PINNED, and `GCHandle.Alloc(…, Pinned)` is an UNCONDITIONAL STRONG ROOT in the GC's
    //      handle table. Pinning was never what StringData means — an address is only needed when
    //      the pointer is CONVERTED to uintptr/void*, and ж already pins on demand there
    //      (EnsureStableAddress). The witness is unique's TestMakeClonesStrings, whose whole
    //      subject is that an interned handle must not keep the caller's string alive: it sets a
    //      finalizer on StringData(s), drops s, forces a GC and requires the finalizer to run.
    //      runtime.SetFinalizer keys its ConditionalWeakTable on the box's ReferentObject, which
    //      for a pinned-buffer box resolves to the PINNED byte[], while the sentinel holding the
    //      registration strong-references the box → the PinnedBuffer → the pin. A dependent handle
    //      tolerates that value→key cycle; the handle TABLE does not, so the key stayed rooted, the
    //      entry never died, the sentinel never finalized, and the test read "string was improperly
    //      retained" — a retention defect that looked exactly like a cloning defect. (unique's
    //      clone<T> was innocent and always had been: an isolation probe over golib alone reproduces
    //      the retention with no unique, no clone and no intern map in the picture.)
    //   2. It ignored the string's WINDOW. `@string` is an offset/length view over a shared backing
    //      array, and GCHandle pins an object from its START, so a window that did not begin at
    //      index 0 could not be handed out as a pinned pointer at all — it materialized a COPY of
    //      its own bytes first. `unsafe.StringData(s[1:])` therefore named a fresh allocation rather
    //      than `&s[1]`, breaking both the aliasing Go guarantees and pointer identity against the
    //      parent. An element reference names the absolute element Go's pointer does, so the round
    //      trip through unsafe.String/unsafe.Slice now ALIASES instead of snapshotting.
    //
    // Identity is unchanged for every shape that already had it: ElemRefBox compares its canonical
    // (backing, absolute index) pair, so two calls over one string still answer equal and two
    // distinct backings still answer unequal — guarded by the StringDataIdentity behavioral test,
    // whose sub-string arm covers point 2.
    return Ꮡ(str.Slice(0, str.Length), 0);
}

} // end unsafe_package
