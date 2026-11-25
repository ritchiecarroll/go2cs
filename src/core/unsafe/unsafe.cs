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
// Registers unsafe.Pointer's canonical nil with golib's type-parameter typed-nil boundary
// (TypedNilOfExtensions.OrTypedNilOf), which cannot name this package's type.
internal static class PointerTypedNilRegistration {
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Register() => TypedNilOfExtensions.UnsafePointerNil = Pointer.Nil;
}

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

    /// <summary>
    /// The canonical TYPED nil unsafe.Pointer -- what <c>any(unsafe.Pointer(nil))</c> holds.
    /// </summary>
    /// <remarks>
    /// Go's nil unsafe.Pointer inside an interface is a VALUE with a dynamic type: the interface is
    /// non-nil, <c>%T</c> prints <c>unsafe.Pointer</c> and <c>reflect.ValueOf</c> answers a valid
    /// UnsafePointer-kind Value. This class renders its nil as a C# <c>null</c> reference, which boxes
    /// as nothing, so the converter substitutes this instance at the empty-interface boundary through
    /// <see cref="OrTypedNil"/> (the typed-nil boundary's third arm, beside the pointer and func arms).
    /// ONE instance, so two typed nils compare reference-equal wherever the comparison is an untyped
    /// object compare -- the same doctrine as <c>ж&lt;T&gt;.NilBox</c>.
    /// </remarks>
    public static readonly Pointer Nil = new(nil);

    /// <summary>
    /// The pointer itself, or <see cref="Nil"/> for a null reference -- the converter's
    /// <c>TypedNilUnsafePointerAccessor</c>. A STATIC method rather than an extension: golib's
    /// <c>OrTypedNil</c> extension binds <c>ж&lt;uintptr&gt;</c> and would answer that type's nil box,
    /// a different dynamic type.
    /// </summary>
    public static Pointer OrTypedNil(Pointer? pointer) => pointer ?? Nil;

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
    //
    // ⚠ EXCEPT A TOKEN (COORD ruling 2026-09-22). When the number minted is an ORDER TOKEN -- a pointee
    // with no address at all, which since that ruling includes an element of a reference-bearing
    // array ([]*int, []string) -- the number cannot recover its box except through the WEAK token
    // registry, so a bare mint whose box the minting frame drops leaves a token that resolves to
    // nothing. Go's unsafe.Pointer keeps its referent alive; for a token the only faithful way is to
    // carry the box. A real address (pinned or native) still retains nothing, so the positive
    // control above (a pinnable StandardBox<long>) is unmoved: its number is an address, not a token.
    public Pointer(INilPointer box) : this(box, box is null || box.IsNilPointer ? 0 : box.StableAddress())
    {
    }

    private Pointer(INilPointer? box, nuint number) : this((uintptr)number, ManagedPointerTokens.IsTaggedToken(number) ? box : null)
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
            return IdentityOf(mine) == pointer.IdentityOf(theirs);
        return PointerOrderToken == pointer.PointerOrderToken;
    }

    // ONE IDENTITY FOR A POINTER AND WHAT IS DERIVED FROM IT (2026-11-14). A pointer whose referent resolved is
    // identified by ONE number, which Equals compares and GetHashCode hashes, so two equal pointers cannot hash
    // apart (a Go map keyed on unsafe.Pointer would miss a key it holds). Two rules give that number.
    //
    //   A DERIVED POINTER. A pointer moved by unsafe.Add, or converted to a typed pointer and back, carries a native
    //   box AT the new address (it keeps what the pointer carried), while a pointer made over a Go variable carries
    //   the variable's own box. Identified by what they carry, the two were always different:
    //   `unsafe.Add(p, 0) == p` answered false, and so did
    //   `unsafe.Add(unsafe.Pointer(&a[0]), 8) == unsafe.Pointer(&a[1])`, where Go answers true. A native box derived
    //   from a managed one now answers its place in that box's token space (NativeDerivation.Token: the source's
    //   token plus the distance moved), which is the token a managed pointer to the same place answers. No address
    //   is asked of a pointer that was never moved, so equality and hashing pin nothing they did not pin before.
    //
    //   TOKEN ARITHMETIC. A struct that holds a reference has no address; its pointer's number is its order token,
    //   and unsafe.Add over it answers the token plus the offset, carrying the root. Such a pointer's identity is its
    //   own NUMBER. It was the root's before, so `unsafe.Add(p, 8) == p` answered true over such a struct. Every
    //   other mint of a token-numbered pointer stores the token of what it carries, so its number and its
    //   referent's token are one value.
    //
    // The asymmetric pair of the hash contract above (exactly one side resolves a referent) is as it was.
    private nuint IdentityOf(object referent)
    {
        nuint number = Value.Value;

        return ManagedPointerTokens.IsTaggedToken(number) ? number : ReferentToken(referent);
    }

    public override int GetHashCode()
    {
        return Referent is { } referent ? IdentityOf(referent).GetHashCode() : PointerOrderToken.GetHashCode();
    }

    // A referent's identity is its ORDER TOKEN, never the box OBJECT: a field or element reference is
    // minted afresh at every `&l.p` / `&a[i]`, so two boxes over ONE field are two objects with one
    // token (allocation base + Go field offset -- "equal pointers always produce equal tokens", the
    // contract PointerOrderToken documents), while a heap box's token is its own allocation base.
    // Comparing the objects said `unsafe.Pointer(&l.p) != unsafe.Pointer(&l.p)` where Go and the
    // numeric rule both say equal (the ManagedAtomicPointer behavioral guard, full-suite run of the
    // referent cut). Hashing by the same token keeps Equals and GetHashCode one fact.
    //
    // A NON-BOX referent (a channel, or an object reflect registered) keys on the SAME token the
    // registry projects for it -- ManagedPointerTokens.CurrentToken's shape -- so that equality and
    // uintptr agree, as Go requires: p1 == p2 exactly when uintptr(p1) == uintptr(p2). It keyed on the
    // identity HASH before, which collides between live objects (G's review of the hash-token cut).
    private static nuint ReferentToken(object referent)
    {
        return referent switch
        {
            INativeRooted { NativeRootAddress: not 0 } derived => NativeDerivation.Token(derived),
            INilPointer box => box.PointerOrderToken,
            IChannel channel => channel.PointerOrderToken,
            _ => ManagedPointerTokens.IdentityToken(referent)
        };
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

        // A zerobase pointer's number is the zerobase's one address (GoZeroBase), the number every
        // other conversion of it answers; the box still rides along as the referent.
        if (box.NamesZeroBase)
            return new Pointer(GoZeroBase.Address, box);

        // A native alias's number IS its meaning — carry it exactly; the retained box still rides
        // along so a store through it reaches the aliased memory via the box's own slot access.
        if (box.NativeAddress != 0)
            return new Pointer((uintptr)box.NativeAddress, box);

        // A view with no storage of its own (golib's view of a field's bytes over a Go value that has no address)
        // has no slot to take the address of. Its number is the field's, through the box's untyped face, and the
        // view rides along as the referent: its identity is the field's, so `unsafe.Pointer(p)` and
        // `unsafe.Pointer(&x.f)` are one pointer, as in Go.
        if (box is IPointerNumberForwarder)
            return new Pointer((uintptr)((IGoReflectBox)box).TransientSlotAddress(), box);

        fixed (T* ptr = &box.ValueSlot)
            return new Pointer((uintptr)ptr, box);
    }

    // FromBox for a box known only as an object (trim stage 3c-2a): golib's slice-header box mints the pointer of a header
    // over a slice whose element type it knows only at run time, and closing FromBox<T> over it needed MakeGenericMethod,
    // which Native AOT cannot. The same four answers through the box's untyped face; the typed FromBox above keeps its own
    // body because converted code calls it on hot paths. GolibTests' SliceHeaderReinterpretTests holds the two equal for a
    // nil, a zerobase, a native and a managed box.
    public static Pointer FromBoxObject(object? box)
    {
        if (box is null)
            return new Pointer(nil);

        IGoReflectBox face = box as IGoReflectBox ?? throw new global::System.ArgumentException($"unsafe.Pointer.FromBoxObject: {box.GetType()} is not a pointer box", nameof(box));

        if (face.IsNilPointer)
            return new Pointer(nil);

        if (face.NamesZeroBase)
            return new Pointer(GoZeroBase.Address, box);

        if (face.NativeAddress != 0)
            return new Pointer((uintptr)face.NativeAddress, box);

        return new Pointer((uintptr)face.TransientSlotAddress(), box);
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

    // The converter's emission for Go's unsafe rule 6 on a reflect header's Data field: `unsafe.Pointer(h.Data)` in ONE
    // expression, with h a *reflect.StringHeader or *reflect.SliceHeader (docs/phase4/DESIGN-sqlite-ccgo-ts.md, QW-8
    // and section 10.3). Go's pointer keeps what the header describes alive; the number the field holds keeps nothing,
    // and a pointer re-formed from it after its element box died read moved memory (measured: 199 of 200 wrong, 10.1).
    //
    // So the converter hands over the header's BOX, and the answer is decided at RUN TIME, because the converter sees
    // only the static type: a HEADER box (golib's string and slice header boxes) answers its Data word together with
    // the element box it minted the word from, and the pointer CARRIES that box, so the pin and the bytes live while
    // the pointer does; any other box (a header built field by field, `&reflect.StringHeader{Data: n}`) answers
    // exactly today's number pointer, from the Data word Go's layout puts first.
    public static Pointer FromHeaderData<T>(ж<T> header)
    {
        // A nil header is Go's nil dereference at `h.Data`, never a .NET NullReferenceException (P3's code read, nit 5).
        if (header is null || header.IsNilPointer)
            throw RuntimeErrorPanic.ErrorString("invalid memory address or nil pointer dereference");

        if (header is IHeaderDataCarrier source)
        {
            nuint word = source.HeaderDataWord(out object? carried);
            return new Pointer((uintptr)word, carried);
        }

        return new Pointer(Unsafe.As<T, uintptr>(ref header.Value));
    }

    // Go's `(*T)(p)` for an unsafe.Pointer VALUE p (docs/phase4/DESIGN-interior-pointer-root.md 4.1 and 10.1). The
    // converter's earlier spelling was `(ж<T>)(uintptr)(p)`: the cast to a number dropped what p carried before
    // golib saw it, so an interior address of Go memory came back as a native box that kept nothing and pinned
    // nothing, and the storage moved or was collected under a pointer still in use.
    //
    // The answer is the number conversion's own (ж<T>.FromNumber: one body, one box minted), with two differences.
    //
    //   THE BASE. A pointer straight from a mint names its source at offset 0, and its stored number may be an
    //   address read before anything held the storage still (FromBox reads it under `fixed` for one statement). So
    //   the number converted is the source's OWN, asked now (BaseOf), never the stored one.
    //
    //   THE ROOT. Where the conversion would mint a native box that keeps nothing, it keeps what p carried.
    //
    // A pointer whose source is itself a pointer to T is that pointer: it names its source at offset 0, which is
    // what the registry answered for a pinned source and what nothing answered for one that was never pinned.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static ж<T> To<T>(Pointer? p)
    {
        if (p is null || p.IsNull)
            return ж<T>.FromNumber(default, carried: null);

        if (p.m_retainedSource is ж<T> itself && NamesAManagedSourceByAddress(p, out _))
            return itself;

        return ж<T>.FromNumber(BaseOf(p), p.m_retainedSource);
    }

    // The same conversion for an operand that is already a number: the number conversion, unchanged.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static ж<T> To<T>(uintptr number)
    {
        return ж<T>.FromNumber(number, carried: null);
    }

    // ---- Go's `(*A)(p)[lo:hi]`, A an array type and p an unsafe.Pointer: the VIEW doors (door A) ----
    //
    // docs/phase4/DESIGN-array-pointer-doors.md, 4.1 to 4.4 and section 10. The converter emitted these forms as a
    // span over the pointer cast to a number, which COPIES: a write through the slice never reached the memory, and
    // a named array (`type RawMem [1<<50 - 1]byte`, modernc.org/libc) was read as a managed wrapper out of C
    // memory. A door takes the pointer itself and the array's length N as a constant. Nothing of length N is
    // allocated or spanned: N is a bound.
    //
    //     (*A)(p)[lo:hi]              View<E>(p, lo, hi, N)
    //     (*A)(p)[lo:hi:max]          View<E>(p, lo, hi, max, N)
    //     (*A)(p)[lo:]                View<E>(p, lo, N)
    //     copy((*A)(p)[lo:], src)     CopyInto<E>(p, lo, N, src)
    //     copy(dst, (*A)(p)[lo:])     CopyFrom<E>(dst, p, lo, N)
    //
    // Go's order: the nil check, then the bounds against N with Go's slice panics for an array, then the view.
    // WHAT THE VIEW IS depends on what p points to (Window):
    //
    //   V1  an element of a Go array or slice of E, or a Go array of E named by its own address: a managed window
    //       on that array. Writes reach it and nothing needs to be held still. Its capacity, and what a copy door
    //       may copy, end where the Go array ends; a view that would pass that end is refused by name, because the
    //       managed array is all the storage there is.
    //   V3  an address the pointer registry does not know (C memory, or an interior address of Go memory): a native
    //       window over the address, with Go's capacity. It keeps what the pointer carried, which is nothing for C
    //       memory alone.
    //   V2  any other Go value that its box holds still (a scalar, a struct of scalars, an array of another element
    //       type, a named array by its own address): a native window over the value's own bytes, ANCHORED on the
    //       box. The box owns the pin, so the storage stays where it is for as long as any slice cut from the view
    //       lives, and writes reach the value. The window ends where the pinned storage ends; a view that would
    //       pass that end is refused by name.
    //   V4  a Go value nothing can hold still (it holds references): a COPY of the elements read at the pointer, as
    //       before the doors. A read is exact and a write does not reach the value, so a copy door refuses such a
    //       destination by name.
    //
    // The length is taken at full width and refused by name past what a span can hold, as unsafe.Slice refuses it:
    // the cast to int it replaces wrapped (2^32 + 5 would have read five elements).
    private enum ViewUse
    {
        Slice,
        CopyDestination,
        CopySource,
        Element,
        ArrayLoad,
        ArrayStore,
        FieldView
    }

    // `(*A)(p)[lo:hi]`: length hi-lo, capacity N-lo.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static slice<E> View<E>(Pointer? p, long lo, long hi, long length)
    {
        RequireArrayPointer(p);

        if ((ulong)hi > (ulong)length || (ulong)lo > (ulong)hi)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(lo, hi, length);

        return Window<E>(p!, lo, hi - lo, length - lo, length, ViewUse.Slice);
    }

    // `(*A)(p)[lo:hi:max]`: length hi-lo, capacity max-lo.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static slice<E> View<E>(Pointer? p, long lo, long hi, long max, long length)
    {
        RequireArrayPointer(p);

        if ((ulong)max > (ulong)length || (ulong)hi > (ulong)max || (ulong)lo > (ulong)hi)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(lo, hi, max, length);

        return Window<E>(p!, lo, hi - lo, max - lo, length, ViewUse.Slice);
    }

    // `(*A)(p)[lo:]` anywhere but directly under copy: the whole array from lo. For an array longer than a span
    // can hold (RawMem) this is refused by name: a slice of that length has no representation here, and a
    // shortened one would answer a wrong len.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static slice<E> View<E>(Pointer? p, long lo, long length)
    {
        return View<E>(p, lo, length, length);
    }

    // `copy((*A)(p)[lo:], src)`. Go copies min(N-lo, len(src)) elements, so the view cut is exactly that long,
    // whatever N is. p is evaluated before src, as Go evaluates copy's arguments.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static nint CopyInto<E>(Pointer? p, long lo, long length, in slice<E> src)
    {
        long count = OpenFormCount(p, lo, length, src.Length);

        if (count == 0)
            return 0;

        // p is held until the elements are copied: the window is over memory p keeps.
        nint copied = copy(Window<E>(p!, lo, count, count, length, ViewUse.CopyDestination), src);

        GC.KeepAlive(p);

        return copied;
    }

    // The same with a string source, which Go allows into a byte destination.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static nint CopyInto(Pointer? p, long lo, long length, in @string src)
    {
        long count = OpenFormCount(p, lo, length, src.Length);

        if (count == 0)
            return 0;

        nint copied = copy(Window<byte>(p!, lo, count, count, length, ViewUse.CopyDestination), src);

        GC.KeepAlive(p);

        return copied;
    }

    // `copy(dst, (*A)(p)[lo:])`: min(len(dst), N-lo) elements.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static nint CopyFrom<E>(in slice<E> dst, Pointer? p, long lo, long length)
    {
        long count = OpenFormCount(p, lo, length, dst.Length);

        if (count == 0)
            return 0;

        nint copied = copy(dst, Window<E>(p!, lo, count, count, length, ViewUse.CopySource));

        GC.KeepAlive(p);

        return copied;
    }

    // ---- Go's `(*A)(p)[i]`, `&(*A)(p)[i]`, `*(*A)(p)` and `*(*A)(p) = v`: the ELEMENT and ARRAY doors (door B) ----
    //
    // docs/phase4/DESIGN-array-pointer-doors.md, 4.5 and 10.5. These forms went through the pointer conversion to
    // the array type, which has no answer over memory that is no Go array of that type: C memory was refused by
    // name (`cannot view native memory as array<...>`), and a named array was read as a managed wrapper out of it.
    // No box of the whole array is made here. N is a bound, as it is for the view doors.
    //
    //     (*A)(p)[i]          ElementRef<E>(p, i, N)        read, store, compound assignment, ++ and --
    //     &(*A)(p)[i]         Element<E>(p, i, N)
    //     v := *(*A)(p)       LoadArray<E>(p, N)            a Go array value: a copy
    //     *(*A)(p) = v        StoreArray<E>(p, N, v)
    //
    // Go's order: the nil check, then the index against N with Go's index panic, then the element. WHAT THE ELEMENT
    // IS follows the view doors' four answers (Aliased), for a window of one element at i:
    //
    //   V1  the Go array's own element box. An index past the end of the Go array is refused by name.
    //   V3  a native box at the element's address, which keeps what the pointer carried.
    //   V2  a native box at the element's address inside the Go value, anchored on the box that holds the value
    //       still. An index past the end of the value is refused by name.
    //   V4  Go's own definition of the form, `(*E)(unsafe.Add(p, i*size))`, which the pointer conversion answers.

    // `&(*A)(p)[i]`: the pointer to element i.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static ж<E> Element<E>(Pointer? p, long i, long length)
    {
        RequireArrayPointer(p);

        if ((ulong)i >= (ulong)length)
            throw RuntimeErrorPanic.IndexOutOfRange(i, length);

        slice<E> window = Aliased<E>(p!, i, 1, 1, length, ViewUse.Element, out ж<E>? unaliased);

        if (unaliased is not null)
            return i == 0 ? unaliased : To<E>(Moved(p, (nuint)i * (nuint)Unsafe.SizeOf<E>()));

        return ElementAddressUnchecked(window, 0);
    }

    // `(*A)(p)[i]` as a location: read, stored, compounded or stepped in the one expression that names it.
    //
    // THE V3 FAST PATH (10.5). The index form is in hot generated code (sqlite's chacha block, its b-tree balance),
    // nearly always over C memory. A pointer that keeps nothing, whose number the registry does not know and that
    // names user memory, is C memory: the element is at p + i*size and nothing has a lifetime, so no box is made.
    // Every other pointer takes the element door, whose box is what keeps the root until the reference is used: a
    // reference into Go storage is one the collector follows by itself.
    //
    // A `ref` result cannot live across an await (CS8178): a site that cannot keep it in one expression takes
    // Element(...).Value.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static ref E ElementRef<E>(Pointer? p, long i, long length)
    {
        RequireArrayPointer(p);

        if ((ulong)i >= (ulong)length)
            throw RuntimeErrorPanic.IndexOutOfRange(i, length);

        if (!RuntimeHelpers.IsReferenceOrContainsReferences<E>() && NativeDerivation.Kept(p!.m_retainedSource) is null)
        {
            nuint number = (nuint)p.Value.Value;

            if (!ManagedPointerTokens.NamesNoUserMemory(number) && ManagedPointerTokens.Resolve(number) is null)
                return ref Unsafe.AsRef<E>((void*)(number + (nuint)i * (nuint)Unsafe.SizeOf<E>()));
        }

        return ref Element<E>(p, i, length).Value;
    }

    // `v := *(*A)(p)`: the array's N elements, copied, as a Go array value is. Over a Go value nothing can alias
    // (V4) the read is still exact.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static array<E> LoadArray<E>(Pointer? p, long length)
    {
        RequireArrayPointer(p);

        slice<E> view = Window<E>(p!, 0, length, length, length, ViewUse.ArrayLoad);
        array<E> value = new array<E>((int)length);

        copy(value[..], view);
        GC.KeepAlive(p);

        return value;
    }

    // `*(*A)(p) = v`: N elements copied to where p points. A Go value nothing can alias (V4) is refused by name: the
    // write would land in a copy. A value shorter than N (the zero value of an array that was never given storage)
    // is zeros from where it ends, as Go's is.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static void StoreArray<E>(Pointer? p, long length, in array<E> value)
    {
        RequireArrayPointer(p);

        if (length == 0)
            return;

        slice<E> view = Window<E>(p!, 0, length, length, length, ViewUse.ArrayStore);

        for (nint i = copy(view, value[..]); i < view.Length; i++)
            view[i] = default!;

        GC.KeepAlive(p);
    }

    // ---- Go's `(*[N]E)(p)` kept, passed or returned, E an unnamed array's element: door C, the unnamed form ----
    //
    // docs/phase4/DESIGN-array-pointer-doors.md, section 20 (decision D5, taken for the unnamed form only). The
    // conversion bound to a local, passed or returned was the pointer conversion `To<array<E>>(p)`, which has no N:
    // array<E> does not carry it, so over an address no Go value owns the conversion can build no window and refuses
    // by name (ж<T>.FromNumber; DESIGN-native-array-view.md 1.1 is why it must). The converter hands N here from the
    // type, so that case has an answer.
    //
    // This is To<array<E>>(p) with a length, and nothing else decides: the same preamble (nil, and a pointer whose
    // source is itself the array pointer), then the number conversion's own arm order (ж<T>.FromNumber's length
    // variant). Its arms answer as they do for To: the same-type alias, the order token through the interior-alias
    // step, the reinterpret, and token arithmetic, which still refuses by name. Only what stood at arm 5 changes: a
    // number no Go value owns (C memory), and a pinned real address of another pointee, open a native array pointer
    // of N elements (builtin.NativeArrayPointer). Its Value refuses by name as before; its elements are read and
    // written in place through ElementRef, which the converter emits for an index through a local bound by this door,
    // and `*p = v` stores through it (array<T>.Assign).
    //
    // A window over Go storage is cut by the view doors' own rule (V2, ArrayView above): only over bytes a box holds
    // still, anchored on that box, and only when all N elements lie inside them. Where nothing holds the storage still
    // (an address derived from a field of a struct that holds a reference, which cannot be pinned) or the window would
    // pass its end, the opener declines and the conversion answers what To answers: arm 5's refusal by name, or the
    // fall-through box. A window over storage the collector may move would read and write whatever lies at the old
    // address once it moves (measured 2026-11-20: a write through such a window after a collection was lost).
    //
    // An E that holds a reference passes no opener, so it answers exactly what To<array<E>>(p) answers; the converter
    // keeps such an E, a named array and a FromRef operand off this door.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static ж<array<E>> ToArray<E>(Pointer? p, long length)
    {
        if (p is null || p.IsNull)
            return ж<array<E>>.FromNumber(default, carried: null);

        if (p.m_retainedSource is ж<array<E>> itself && NamesAManagedSourceByAddress(p, out _))
            return itself;

        return ж<array<E>>.FromNumber(BaseOf(p), p.m_retainedSource, (nint)length, KeptArrayOpener<E>.Open);
    }

    // The window door for E, or null for an E that holds a reference (whose window is not this door's to open).
    private static class KeptArrayOpener<E>
    {
        internal static readonly global::System.Func<nuint, nint, object?, object?, ж<array<E>>?>? Open =
            RuntimeHelpers.IsReferenceOrContainsReferences<E>() ? null : static (address, length, resolved, carried) => OpenWindow(address, length, resolved, carried);

        // `resolved` is the live Go box the number names, or null; `carried` is what the pointer carried.
        private static ж<array<E>>? OpenWindow(nuint address, nint length, object? resolved, object? carried)
        {
            nuint bytes = (nuint)length * (nuint)Unsafe.SizeOf<E>();

            if (resolved is not null)
            {
                // Arm 2b. A resolved box that names native memory (an element or field over a native window) holds
                // nothing here: the address is native memory, and the window keeps what the pointer carried.
                if (NativeDerivation.KeptResolved(resolved, aliasesAnOrderToken: false) is not { } holder)
                    return builtin.NativeArrayPointer<E>(address, length, NativeDerivation.Kept(carried));

                // A Go value of another type: a window only over the bytes it holds still from this address.
                return holder is IHeldStorage storage && storage.HeldBytesFrom(address) is var room and not 0 && room >= bytes
                    ? builtin.NativeArrayPointer<E>(address, length, holder)
                    : null;
            }

            object? kept = NativeDerivation.Kept(carried);

            // C memory, or a number that carries nothing: nothing here knows where it ends, as in Go.
            if (NativeDerivation.HeldSource(kept) is not { } origin)
                return builtin.NativeArrayPointer<E>(address, length, kept);

            // An address derived from a Go value (an offset into it, a field of it) that the registry does not know:
            // a window only when the value holds its storage still and all N elements lie inside it.
            return origin.TryHeldExtent(out nuint start, out nuint end) && address >= start && address <= end && end - address >= bytes
                ? builtin.NativeArrayPointer<E>(address, length, kept)
                : null;
        }
    }

    // ---- Go's `(*[N]E)(unsafe.Pointer(&p.f))`, f not an E, over a root that is not native: the FIELD VIEW ----
    //
    // docs/phase4/DESIGN-array-pointer-doors.md, section 18. ArrayPointer<E>.Of answers this where golib's native
    // field door declines. p is the pinned pointer to the field. The route was the pointer conversion to the array
    // type, which answered a native box of array<E> over the field: its Value read the field's bytes as the three
    // fields of array<E> itself, a managed reference made out of content.
    //
    // The answer is the view doors' own (Aliased), for a window of N elements at the field:
    //
    //   V2  a native array pointer over the bytes the field's root holds still. It is anchored on the box that
    //       holds them, so the view keeps the root, and it ends where the root's storage ends: a view longer than
    //       its field reads the fields after it, as Go does, and one that would pass the end of the root is
    //       refused by name.
    //   V3  the same over a field whose own box is native.
    //   V1  a field that is an element of managed E storage: a window on that array.
    //   V4  a root with no address (a struct that holds references): nothing holds the field's bytes still, so no
    //       native array pointer can be made over them, and an array pointer over a copy would lose every write.
    //       The answer is golib's view of the FIELD's own bytes through the field's pointer, read again at every
    //       access (ReinterpretedArrayBox, section 19). It is made when the field holds no reference and the view
    //       fits inside it; a longer view, or a field whose converted form holds a reference, is refused by name.
    internal static ж<array<E>> FieldView<E>(Pointer p, nint length, object? field)
    {
        if (p.IsNull)
            return To<array<E>>(p);

        slice<E> window = Aliased<E>(p, 0, length, length, length, ViewUse.FieldView, out ж<E>? unaliased);

        if (unaliased is not null)
            return ReinterpretedArrayBox<E>.TryOver(field, length, out string? refusal) ?? throw new PanicException((@string)$"go2cs: {ViewForm<E>(0, length, length, ViewUse.FieldView)}: {refusal}");

        if (window.IsNativeBacked)
            return builtin.NativeArrayPointer<E>(window.NativeElementAddressUnchecked(0), length, NativeSliceAnchors<E>.Of(window));

        return array<E>.AliasPointer(ElementAddressUnchecked(window, 0), length);
    }

    // Slicing through a nil array pointer is Go's nil dereference, before any bound is read.
    private static void RequireArrayPointer(Pointer? p)
    {
        if (p is null || p.IsNull)
            throw RuntimeErrorPanic.NilPointerDereference();
    }

    // The open form's checks and the count copy uses of it.
    private static long OpenFormCount(Pointer? p, long lo, long length, nint other)
    {
        RequireArrayPointer(p);

        if ((ulong)lo > (ulong)length)
            throw RuntimeErrorPanic.LengthSliceBoundsOutOfRange(lo, length, length);

        return Math.Min(length - lo, other);
    }

    // The Go form a refusal names: N and E are the array's, the bounds the site's.
    private static string ViewForm<E>(long lo, long count, long length, ViewUse use) =>
        use switch
        {
            ViewUse.Slice => $"(*[{length}]{GoReflect.GoTypeName(typeof(E))})(p)[{lo}:{lo + count}]",
            ViewUse.Element => $"(*[{length}]{GoReflect.GoTypeName(typeof(E))})(p)[{lo}]",
            ViewUse.ArrayLoad => $"*(*[{length}]{GoReflect.GoTypeName(typeof(E))})(p)",
            ViewUse.ArrayStore => $"*(*[{length}]{GoReflect.GoTypeName(typeof(E))})(p) = v",
            ViewUse.FieldView => $"(*[{length}]{GoReflect.GoTypeName(typeof(E))})(unsafe.Pointer(&p.f))",
            _ => $"copy through (*[{length}]{GoReflect.GoTypeName(typeof(E))})(p)[{lo}:]"
        };

    // The view of `count` elements starting `lo` elements after p, with room for `capacity`. The bounds are checked
    // by the caller. p is resolved where it points and the view starts lo elements on, so a number the registry
    // knows keeps its answer for every lo.
    private static slice<E> Window<E>(Pointer p, long lo, long count, long capacity, long length, ViewUse use)
    {
        slice<E> window = Aliased<E>(p, lo, count, capacity, length, use, out ж<E>? unaliased);

        if (unaliased is null)
            return window;

        // V4. A write would land in the copy, so a destination is refused instead of written and lost.
        if (use is ViewUse.CopyDestination or ViewUse.ArrayStore)
            throw new PanicException((@string)$"go2cs: {ViewForm<E>(lo, count, length, use)} cannot write the Go value p points to: it is not an array of {GoReflect.GoTypeName(typeof(E))}, and a view of it is a copy");

        if (count == 0)
            return new slice<E>(Array.Empty<E>());

        fixed (E* source = &unaliased.Value)
            return new slice<E>(new ReadOnlySpan<E>(source + lo, (int)count));
    }

    // The window that ALIASES what p points to (V1, V2, V3), or `unaliased`: the pointer to the first element where
    // nothing can be aliased (V4), for the caller to answer as its form requires.
    private static slice<E> Aliased<E>(Pointer p, long lo, long count, long capacity, long length, ViewUse use, out ж<E>? unaliased)
    {
        unaliased = null;

        // The converter refuses such an element at the site; the doors are public, so they refuse it too.
        if (RuntimeHelpers.IsReferenceOrContainsReferences<E>())
            throw new PanicException((@string)$"go2cs: {ViewForm<E>(lo, count, length, use)}: an element that holds references cannot be viewed at an address");

        // A zero-size element has no storage to view, so a length past any span is still a valid Go slice
        // (unsafe.Slice's exemption). Its capacity is its length.
        if (GoSliceLimit.IsZeroSize<E>() && GoSliceLimit.LongerThanAnySpan(count))
            return new slice<E>((nint)count);

        if (GoSliceLimit.LongerThanAnySpan(count))
            throw RuntimeErrorPanic.RuntimeRaised($"go2cs: {ViewForm<E>(lo, count, length, use)}: length {count} exceeds the maximum slice length of this runtime");

        ж<E> ptr = To<E>(p);

        // V1, an element of managed E storage: the pointer's own box, or the box a number resolved to.
        if (ptr.TryGetElementStorage(out E[]? elements, out nint first))
            return ManagedWindow(elements, first, elements.Length - first, lo, count, capacity, length, use);

        // What holds the storage still, for V2: the box a number resolved to, or the pointer's own box.
        object? holder = null;
        nuint held = 0;

        // An order token is a number that is no address: it is never viewed as memory (the copy below reads through
        // the box, which refuses a token by name).
        if (ptr.IsNative && !ManagedPointerTokens.IsTaggedToken(ptr.NativeAddress))
        {
            object? resolved = ManagedPointerTokens.Resolve(ptr.NativeAddress);

            if (resolved is null)
            {
                object? kept = NativeDerivation.Kept(ptr);

                // An address the registry does not know that was DERIVED from a Go value (unsafe.Add over a pointer
                // to it, an element of a view of it) is inside that value's storage, or was meant to be. Go's
                // capacity N-lo is no bound on it: `(*[1<<30]byte)(unsafe.Add(unsafe.Pointer(&v), 4))[:n]` would
                // read and write the heap past v. So the Go value answers for where its storage ends (V2 at an
                // interior address), and an address outside it is refused by name.
                if (NativeDerivation.HeldSource(kept) is not { } origin)
                {
                    // V3: C memory, or a number that carries nothing. Kept answers null for C memory alone, so no
                    // anchor is allocated for it. Nothing here knows where such memory ends, as in Go.
                    nuint address = ptr.NativeAddress + (nuint)lo * (nuint)Unsafe.SizeOf<E>();

                    return slice<E>.OverNativeMemory(address, (nint)count, (nint)capacity, NativeSliceAnchors<E>.For(kept));
                }

                if (origin.TryHeldExtent(out nuint start, out nuint end))
                {
                    if (ptr.NativeAddress < start || ptr.NativeAddress > end)
                        throw new PanicException((@string)$"go2cs: {ViewForm<E>(lo, count, length, use)}: p is outside the Go value it was derived from");

                    return HeldWindow<E>(kept!, ptr.NativeAddress, end - ptr.NativeAddress, lo, count, capacity, length, use);
                }

                // The Go value holds nothing still at this address (it cannot be pinned, or it has other storage
                // now): no window is cut over it, and the copy arm below answers.
            }
            else if (resolved is ж<array<E>> { IsNilPointer: false } arrayBox && arrayBox.ValueSlot is { Source: { } source } whole)
            {
                // V1, a Go array of E named by its own address (`unsafe.Pointer(&a)`, or that number): the address
                // is the array's element 0, so the view is a window on the array.
                return ManagedWindow(source, whole.Low, whole.Length, lo, count, capacity, length, use);
            }
            else
            {
                holder = resolved;
                held = ptr.NativeAddress;
            }
        }
        else if (!ptr.IsNative && !ptr.IsNilPointer && ptr.StorageKind is PointerStorage.Pinnable)
        {
            // A pointer to one E that is no element of an array (`unsafe.Pointer(&x)`, x an E): its number is the
            // pin moment, and the box is its own holder.
            holder = ptr;
            held = (nuint)((uintptr)ptr).Value;
        }

        // V2. The holder answers for the bytes it holds still from the address; nothing else is viewed.
        if (holder is IHeldStorage storage && storage.HeldBytesFrom(held) is var room and not 0)
            return HeldWindow<E>(holder, held, room, lo, count, capacity, length, use);

        unaliased = ptr;

        return default;
    }

    // A window on the bytes of a Go value its box holds still (V2): `room` bytes exist from `address` to the end of
    // the storage the holder pinned. The window is anchored on the holder, whose pin keeps the bytes where they are.
    private static slice<E> HeldWindow<E>(object holder, nuint address, nuint room, long lo, long count, long capacity, long length, ViewUse use)
    {
        nuint size = (nuint)Unsafe.SizeOf<E>();
        long remain = (long)(room / size) - lo;

        if (count > remain)
            throw new PanicException((@string)$"go2cs: {ViewForm<E>(lo, count, length, use)} would pass the end of the Go value p points to ({Math.Max(remain, 0)} elements of it remain)");

        return slice<E>.OverNativeMemory(address + (nuint)lo * size, (nint)count, (nint)Math.Min(capacity, remain), NativeSliceAnchors<E>.For(holder));
    }

    // A window on a Go array of E: `available` elements exist from the pointer's element to the array's end.
    private static slice<E> ManagedWindow<E>(E[] elements, nint first, nint available, long lo, long count, long capacity, long length, ViewUse use)
    {
        long remain = available - lo;

        if (count > remain)
            throw new PanicException((@string)$"go2cs: {ViewForm<E>(lo, count, length, use)} would pass the end of the Go array p points into ({Math.Max(remain, 0)} elements remain)");

        nint low = first + (nint)lo;

        return new slice<E>(elements, low, low + (nint)count, low + (nint)Math.Min(capacity, remain));
    }

    // Whether p came straight from a mint over a MANAGED box and holds an address for it: FromBox, FromBoxObject or
    // FromPinnedBox over storage that has one. Such a pointer names its source at offset 0 by construction, because
    // every derivation replaces the source (Add carries the native box NativeDerivation.Moved made). A zerobase
    // pointer, a native alias and a token (tagged; its arithmetic keeps the source, see Add) are not this case.
    private static bool NamesAManagedSourceByAddress(Pointer p, [NotNullWhen(true)] out IGoReflectBox? source)
    {
        source = p.m_retainedSource as IGoReflectBox;

        return source is { NativeAddress: 0, IsNilPointer: false, NamesZeroBase: false } && !ManagedPointerTokens.IsTaggedToken((nuint)p.Value.Value);
    }

    // The number a derivation from p starts at (design 10.1): for a pointer that names a managed box by address, the
    // box's OWN number, asked now. The box's number conversion is the pin moment: it holds pinnable storage still
    // for the box's lifetime, records it, and answers the address that now holds (or the order token, for a pointee
    // that holds a reference and so has no address). The stored number is used as it is when the box is already
    // pinned there, which is every FromPinnedBox mint, and for every other pointer.
    private static uintptr BaseOf(Pointer p)
    {
        if (!NamesAManagedSourceByAddress(p, out IGoReflectBox? source))
            return p.Value;

        if (source is INilPointer pinned && pinned.IsPinnedAt((nuint)p.Value.Value))
            return p.Value;

        return (uintptr)source.NativeArgumentAddress();
    }

    // unsafe.Add's body. Three rules beyond the sum:
    //
    //   over NIL it answers the offset as a number, as Go does (`unsafe.Add(nil, n)` is the address n; it answered
    //   nil here until 2026-11-14). Offset 0 over nil is still nil: the zero address is the nil pointer;
    //
    //   the BASE is BaseOf(ptr), so a pointer minted by FromBox is derived from where its storage is held, not from
    //   a number read before anything held it;
    //
    //   the SOURCE is kept. For an address, the moved pointer carries a native box at the new value that retains
    //   what ptr carried (NativeDerivation.Moved). For a TOKEN (a source that holds a reference has no address) the
    //   sum is token arithmetic, which the conversion answers by the alias walk over the token's own box; that box
    //   is found through a weak record, so the moved pointer keeps it, or the struct could be collected while the
    //   pointer that Go says keeps it alive is still in use.
    internal static Pointer Moved(Pointer? ptr, nuint offset)
    {
        if (ptr is null || ptr.IsNull)
            return new Pointer((uintptr)offset);

        nuint value = (nuint)BaseOf(ptr).Value + offset;

        if (NativeDerivation.Moved(ptr.m_retainedSource, value) is { } moved)
            return FromBox(moved);

        if (ManagedPointerTokens.IsTaggedToken(value) && NativeDerivation.Kept(ptr.m_retainedSource) is { } kept)
            return new Pointer((uintptr)value, kept);

        return new Pointer((uintptr)value);
    }

    // The word a Go FUNC value answers when it is read AS a word -- `*(*unsafe.Pointer)(unsafe.Pointer(&f))`,
    // emitted for a func-typed f. Go's func value is one pointer (to a funcval); a managed one is a
    // delegate with no address, so the word is its GoFuncCookie: carried by native code as an lParam and
    // resolved back to the same delegate by the `*(*func())(unsafe.Pointer(&word))` read (golib's
    // Reinterpret). A nil func is Go's zero word. It carries no retained source on purpose: a cookie is a
    // NUMBER in Go's sense, and like a uintptr it does not keep the func alive.
    public static Pointer OfFunc(Delegate? fn)
    {
        return fn is null ? Nil : new Pointer(GoFuncCookie.Of(fn));
    }

    // Go's `*(*unsafe.Pointer)(unsafe.Pointer(&f)) = p` for a func variable f of type F (funcWordOperations.go, w4;
    // TC-13, 2026-11-20): f's word becomes p. A managed func value is a delegate, and the word it answers is its
    // GoFuncCookie (OfFunc above), so a p that IS a func word -- one func value's word copied into another -- is that
    // func, and 0 is the nil func. Any other word is Go's funcval pointer, whose fn names code by a PC, or C memory.
    // go2cs has no func a PC names before cgo callbacks (Phase 5), so that store is refused by name. It is never stored
    // as a nil func: the emission this replaces wrote p into a fresh box and left f nil, silently.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static F FuncOfWord<F>(Pointer? p) where F : Delegate
    {
        nuint word = p is null ? 0 : (nuint)(uintptr)p;

        if (word == 0)
            return null!;

        if (GoFuncCookie.IsCookie(word))
            return GoFuncCookie.Func<F>(word);

        throw RuntimeErrorPanic.RuntimeRaised(FuncOfWordText);
    }

    private const string FuncOfWordText = "func word store: the pointer stored as a func value's word is not a func value's word (a funcval whose fn is a code PC, or C memory), and go2cs binds no func to such a word before cgo callbacks, so the store is refused";

    // Go's `*(*F)(e) = fn` for a func type F and an unsafe.Pointer VALUE e (docs/phase4/DESIGN-interior-pointer-root.md
    // 4.8 B1 and 10.4). The store's own type says the VALUE is a func. It does not say what e names, and a managed
    // func is a reference while Go's is one word, so what is stored is decided here, at run time, by what e names:
    //
    //   a WORD: C memory, or Go memory that holds no reference (a `uintptr` variable, a field of a struct of
    //   numbers, an element of a word array). The func's word is stored (GoFuncCookie), through the same conversion
    //   `*(*uintptr)(e) = word` takes. This is ccgo's function-pointer table (modernc.org/sqlite, vfs/patches64.go);
    //
    //   a FUNC: a func variable, or a func field or element inside a value that holds references. The func itself is
    //   stored, as the slot's own delegate type (a named func type's variable takes a func stored as the unnamed
    //   type, as it does in Go);
    //
    //   a `uintptr` field or element inside a value that holds references: the word, through its alias.
    //
    // Anything else is refused by name, and no arm writes 8 bytes over a reference. The emission this replaces,
    // `To<F>(e).Value = fn`, was a native box over a delegate: a managed reference written into raw memory.
    //
    // LIFETIME. A word does not keep its func alive, as a uintptr does not in Go. A static function's word is good
    // for the life of the process. A closure's word is good while something else holds the closure; after that it
    // names no live func and a call through it is refused by name (GoFuncCookie.ResolveAs).
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static void StoreFunc(Pointer? e, Delegate? fn)
    {
        if (e is null || e.IsNull)
            throw go.golib.RuntimeErrorPanic.NilPointerDereference();

        nuint number = (nuint)BaseOf(e).Value;

        // An address: memory with no reference in it. The word store.
        if (!ManagedPointerTokens.IsTaggedToken(number))
        {
            To<uintptr>(e).Value = (uintptr)GoFuncCookie.Of(fn);
            return;
        }

        // A token: a value that holds a reference has no address, and the number names its box (and, moved, an
        // offset into it). The slot is the one Go's layout puts there.
        nuint offset = 0;
        object? root = ManagedPointerTokens.Resolve(number);

        if (root is null && ManagedPointerTokens.IsTokenArithmetic(number))
            root = ManagedPointerTokens.ResolveArithmeticBase(number, out offset);

        if (root is null)
            throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer func store: the pointer names no live Go value");

        object? slot = GoReflect.ResolveInteriorFuncSlot(root, offset, out Type? slotType, out string shape);

        if (slot is ж<uintptr> word)
        {
            word.Value = (uintptr)GoFuncCookie.Of(fn);
            return;
        }

        if (slot is IUntypedSlotAccess func && slotType is not null)
        {
            // The slot's own delegate type: a func of another type with the same signature is rebound to it, and one
            // that cannot be is refused by name there.
            Delegate? value = fn is null || fn.GetType() == slotType ? fn : GoFuncCookie.ResolveAs(GoFuncCookie.Of(fn), slotType);

            if (func.TryStoreThrough(value))
                return;
        }

        throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer func store: the pointer names neither a func nor a word (" + root.GetType().Name + " +" + offset + ":" + shape + ")");
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

    // ---- W1 / W2 / W3: a *unsafe.Pointer load and store over a WORD (docs/phase4/DESIGN-cgo-phase3-marshaling.md,
    //      18.3 to 18.7, cure S-B1) ----
    //
    // The converter emits `*p` as LoadWord(p) and `*p = v` as StoreWord(p, v) when p's static type is *unsafe.Pointer, and
    // the hand-owned atomic doors take SwapWord and CompareAndSwapWord over a native slot. Over a box that holds a CLR
    // Pointer slot (a variable, a field or an element typed unsafe.Pointer) the slot is the value, as before. Over a native
    // box, the slot is a WORD:
    //
    //   GO MEMORY (a word of a Go value a pin holds still: the reinterpret `(*unsafe.Pointer)(unsafe.Pointer(&w))`): the
    //   store writes the referent's number, taken through the PINNING projection (BaseOf: a pinned address, or an order
    //   token), never a FromBox mint's transient number; golib's word table records the referent for the slot, which
    //   keeps it alive and pinned while the word is published (GoPointerWords). The load answers the referent whose
    //   entry matches the word, and a number where none does.
    //
    //   C MEMORY alone: the word is a number. A Go pointer stored there is refused by name (C memory must not hold a Go
    //   pointer), and a word read there that names Go memory is refused by name (its storage may have moved).
    //
    // Before this, `p.Value` went through the reference floor's exemption and stored a CLR REFERENCE in the word, which
    // the collector does not trace (found 2026-11-16); the exemption is deleted with this door (W4).

    private const string StoreUnheldText = "unsafe.Pointer store: the stored pointer's storage can be neither pinned nor tokened";
    private const string StoreIntoCText = "unsafe.Pointer store: storing a Go pointer into C memory is refused (C memory must not hold a Go pointer)";
    private const string LoadFromCText = "unsafe.Pointer load: a Go pointer read out of C memory is refused (C memory must not hold a Go pointer)";
    private const string UnheldSlotText = "unsafe.Pointer word: the slot is a native box over Go storage that nothing holds still";

    // The projection: the number a store writes for v, and the referent the word's entry keeps. A pointer with no referent
    // is a number and keeps nothing. With `check`, a referent whose number names nothing held still is refused by name.
    private static nuint ProjectWord(Pointer? v, out object? referent, bool check)
    {
        referent = null;

        if (v is null || v.IsNull)
            return 0;

        nuint word = (nuint)BaseOf(v).Value;
        referent = v.ResolveReferent();

        if (referent is null || word == 0)
        {
            referent = null;
            return word;
        }

        if (!GoPointerWords.NamesHeldStorage(referent, word))
        {
            if (check)
                throw RuntimeErrorPanic.RuntimeRaised(StoreUnheldText);

            referent = null;
        }

        return word;
    }

    // Whether v names Go memory, for a store into C memory: a referent other than C memory alone, or a number the
    // registry, a live pin or the tag calls Go memory.
    private static bool NamesGoMemory(object? referent, nuint word) =>
        (referent is not null && !GoPointerWords.IsCMemoryAlone(referent)) || GoPointerWords.NamesGoMemory(word);

    // The LOADED word (interiorPointerMintCensus's FromWord row): the number is what StoreWord stored in the slot, the
    // BaseOf projection of the stored pointer: a pinned address (pinned by the slot's bag entry for as long as the word is
    // stored) or an order token (the referent is the entry's), never FromBox's transient number. A number with no referent
    // is read from a slot of C memory, and from a word of Go memory with NO entry: a raw uintptr store, or entries cleared
    // by a copy from untabled storage.
    private static Pointer FromWord(nuint word, object? referent) =>
        word == 0 ? Nil : referent is null ? new Pointer((uintptr)word) : new Pointer((uintptr)word, referent);

    private static void RequireSlot(ж<Pointer>? p)
    {
        if (p is null || p.IsNilPointer)
            throw RuntimeErrorPanic.NilPointerDereference();
    }

    /// <summary>W1: Go's <c>*p</c> for <c>p</c> a <c>*unsafe.Pointer</c>.</summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static Pointer LoadWord(ж<Pointer>? p)
    {
        RequireSlot(p);

        switch (GoPointerWords.Classify(p!))
        {
            case GoPointerWords.SlotKind.GoMemory:
            {
                nuint word = GoPointerWords.Load(p!, out object? referent);
                return FromWord(word, referent);
            }

            case GoPointerWords.SlotKind.CMemory:
            {
                nuint word = p!.ReadPointerWord();

                if (GoPointerWords.NamesGoMemory(word))
                    throw RuntimeErrorPanic.RuntimeRaised(LoadFromCText);

                return FromWord(word, null);
            }

            case GoPointerWords.SlotKind.Unheld:
                throw RuntimeErrorPanic.RuntimeRaised(UnheldSlotText);

            default:
                // A managed slot: its slot itself, never Value, which throws on a box whose held unsafe.Pointer is nil (a nil
                // unsafe.Pointer is that held null). RequireSlot has already refused a nil POINTER, structurally.
                return p!.ValueSlot;
        }
    }

    /// <summary>W2: Go's <c>*p = v</c> for <c>p</c> a <c>*unsafe.Pointer</c>.</summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static void StoreWord(ж<Pointer>? p, Pointer? v)
    {
        RequireSlot(p);

        switch (GoPointerWords.Classify(p!))
        {
            case GoPointerWords.SlotKind.GoMemory:
            {
                nuint word = ProjectWord(v, out object? referent, check: true);
                GoPointerWords.Store(p!, word, referent);
                return;
            }

            case GoPointerWords.SlotKind.CMemory:
            {
                nuint word = ProjectWord(v, out object? referent, check: false);

                if (NamesGoMemory(referent, word))
                    throw RuntimeErrorPanic.RuntimeRaised(StoreIntoCText);

                p!.ExchangePointerWord(word);
                return;
            }

            case GoPointerWords.SlotKind.Unheld:
                throw RuntimeErrorPanic.RuntimeRaised(UnheldSlotText);

            default:
                // A managed slot: written through its slot, as LoadWord reads it (Value would read the held nil first).
                p!.ValueSlot = v!;
                return;
        }
    }

    /// <summary>W3's swap over a NATIVE slot: the store, answering the word it displaced as a pointer.</summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static Pointer SwapWord(ж<Pointer> p, Pointer? v)
    {
        RequireSlot(p);

        switch (GoPointerWords.Classify(p))
        {
            case GoPointerWords.SlotKind.GoMemory:
            {
                nuint word = ProjectWord(v, out object? referent, check: true);
                nuint displaced = GoPointerWords.Swap(p, word, referent, out object? previous);
                return FromWord(displaced, previous);
            }

            case GoPointerWords.SlotKind.CMemory:
            {
                nuint word = ProjectWord(v, out object? referent, check: false);

                if (NamesGoMemory(referent, word))
                    throw RuntimeErrorPanic.RuntimeRaised(StoreIntoCText);

                return FromWord(p.ExchangePointerWord(word), null);
            }

            case GoPointerWords.SlotKind.Unheld:
                throw RuntimeErrorPanic.RuntimeRaised(UnheldSlotText);

            default:
                // A native kind the table does not classify: the word is a number, as the atomics always read it.
                return FromWord(p.ExchangePointerWord(ProjectWord(v, out _, check: false)), null);
        }
    }

    /// <summary>
    /// W3's compare-and-swap over a NATIVE slot. <paramref name="old"/> is taken through the same projection as a store,
    /// so two FromBox mints whose transient numbers coincide never match each other's word.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static bool CompareAndSwapWord(ж<Pointer> p, Pointer? old, Pointer? @new)
    {
        RequireSlot(p);

        switch (GoPointerWords.Classify(p))
        {
            case GoPointerWords.SlotKind.GoMemory:
            {
                nuint oldWord = ProjectWord(old, out object? oldReferent, check: false);
                nuint newWord = ProjectWord(@new, out object? newReferent, check: true);
                return GoPointerWords.CompareAndSwap(p, oldWord, oldReferent, newWord, newReferent);
            }

            case GoPointerWords.SlotKind.CMemory:
            {
                nuint newWord = ProjectWord(@new, out object? newReferent, check: false);

                if (NamesGoMemory(newReferent, newWord))
                    throw RuntimeErrorPanic.RuntimeRaised(StoreIntoCText);

                return p.CompareExchangePointerWord(ProjectWord(old, out _, check: false), newWord);
            }

            case GoPointerWords.SlotKind.Unheld:
                throw RuntimeErrorPanic.RuntimeRaised(UnheldSlotText);

            default:
                return p.CompareExchangePointerWord(ProjectWord(old, out _, check: false), ProjectWord(@new, out _, check: false));
        }
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
            throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer store-through: the pointer carries no recoverable managed referent (raw-address stores are not part of the managed model)");

        // A WORD of Go memory (a slot that holds a uintptr): the W2 door, so the word is the projected number and its
        // entry keeps the referent (cure S-B1). Before, the raw number arm below wrote a FromBox mint's transient number.
        if (WordSlotForThrough(slot) is { } word)
        {
            StoreWord(word, val);
            return;
        }

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

        throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer store-through: the named slot cannot hold the stored pointer value");
    }

    // The store-through and load-through doors' WORD case: the slot this pointer names holds a uintptr, and the
    // reinterpret over it is a word of Go memory a pin holds still. Any other slot keeps its own arms.
    private ж<Pointer>? WordSlotForThrough(IUntypedSlotAccess slot)
    {
        if (!slot.TryLoadThrough(out object? current) || current is not uintptr)
            return null;

        ж<Pointer> word = To<Pointer>(this);
        return GoPointerWords.Classify(word) == GoPointerWords.SlotKind.GoMemory ? word : null;
    }

    /// <summary>
    /// Performs Go's <c>*(*unsafe.Pointer)(ptr)</c> — the load the bare-Pointer atomic primitives
    /// (<c>Loadp</c>) are defined as. Same recovery, same loud residual, as
    /// <see cref="StoreThrough"/>.
    /// </summary>
    public Pointer LoadThrough()
    {
        if (ResolveReferent() is not IUntypedSlotAccess slot)
            throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer load-through: the pointer carries no recoverable managed referent (raw-address loads are not part of the managed model)");

        // A WORD of Go memory: the W1 door (cure S-B1).
        if (WordSlotForThrough(slot) is { } word)
            return LoadWord(word);

        if (!slot.TryLoadThrough(out object? value))
            throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer load-through: nil pointer dereference");

        return value switch
        {
            null => new Pointer(nil),
            Pointer p => p,
            uintptr u => new Pointer(u),
            INilPointer referent => FromReferent(referent),
            _ => throw go.golib.RuntimeErrorPanic.RuntimeRaised("unsafe.Pointer load-through: the named slot does not hold a pointer-shaped value"),
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
// INSTANCE. A shape TryGoSizeOf declines (a type past the address space, an array length nothing
// carries, the depth bound) is REFUSED BY NAME. Marshal.SizeOf<T>() was the fallback here, and it
// answered a MARSHALLING size, which is not Go's: dnsmessage's SVCParam marshals as 48 where Go says
// 32, and a struct holding a [1<<62]int64 field (2^65 bytes in Go) marshals as 16. And because the
// call was generic, ILC compiled a Marshal struct thunk for every T that reaches this method: for a T
// holding a slice, string or array value field (SVCParam, string, debug/macho.Symbol,
// debug/pe.COFFSymbol) that thunk is a "will always throw" stub in every Native AOT image reaching
// it, though no T in the corpus ever took the branch (each answers through TryGoSizeOf).
public static uintptr Sizeof<T>(T x) {
    // TryGoSizeOf answers derivability separately from the size, which matters here: the old
    // signed form reported a type of 2^63 bytes and up as -1, indistinguishable from "unknown".
    nint[]? dims = GoReflect.ArrayDimsOfValue(x);

    return GoReflect.TryGoSizeOf(typeof(T), dims, out nuint size)
        ? (uintptr)size
        : throw new PanicException($"runtime error: unsafe.Sizeof: the Go size of {GoReflect.GoTypeName(typeof(T), dims)} cannot be derived (GoReflect.TryGoSizeOf declined it)");
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
    return Pointer.Moved(ptr, (nuint)nint.CreateTruncating(len));
}

// A typed element box addresses its target through an Int32 element index, so an offset outside
// Int32 cannot be represented. Truncating it would step a wrong number of elements silently (a
// 4 GiB + 8 offset landing 8 on), so it is refused by name, like Slice and String's length.
// Negative offsets stay legal: Go's Add accepts them.
private static int PointerOffset<TLen>(TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    if (len > TLen.CreateSaturating(int.MaxValue) || (TLen.IsNegative(len) && len < TLen.CreateSaturating(int.MinValue)))
        throw RuntimeErrorPanic.RuntimeRaised($"unsafe.Add: offset {len} exceeds what this runtime can address through a typed element box");

    return int.CreateTruncating(len);
}

public static ж<T> Add<T, TLen>(ж<T> ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    // Go's len is of any integer type (the `IntegerType` constraint); reduce it to the pointer
    // offset type. A signed/unsigned/native-width argument (e.g. a `uintptr`) thus all bind here.
    int n = PointerOffset(len);

    if (ptr == nil)
        return new StandardBox<T>(nil);

    // A pointer that ALIASES a native address offsets that address directly. Go's unsafe.Add is
    // byte arithmetic (its argument is an unsafe.Pointer), which is what a native alias reproduces;
    // the managed array-element path below is an element step against a managed backing store.
    if (ptr.IsNative)
        return new NativeBox<T>((nuint)((nint)ptr.NativeAddress + n), retainedSource: NativeDerivation.Kept(ptr));

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

// SpanLength reduces Go's length argument to the Int32 length the managed model can represent. slice<T> and
// @string here are built over CLR arrays and spans, whose length is Int32, so a longer length has no
// representation: it used to WRAP (int.CreateTruncating), which made 3 GiB negative and 4 GiB + 5 a
// five-element slice. It is refused by name instead (coordinator ruling 2026-09-29 11:33).
//
// The order matters. NEGATIVE first, on the full-width value: any negative length returns -1 for the
// caller's own "unsafe.Slice/String: len out of range" panic (runtime/unsafe.go's errorString), because narrowing a length below Int32.MinValue
// wrapped it POSITIVE (-(2^32) + 5 came back as five elements; TRAIN J review, 2026-09-29). Then the
// Int32 representation limit, which binds every element type, zero-size included. Then golib's ONE
// maximum (GoSliceLimit), the same bound the native-backed slice window refuses at, with the same
// zero-size exemption. The refusal text is unchanged (runtime's TestMemmoveOverflow disclosure pins it).
private static int SpanLength<T, TLen>(TLen len, string function) where TLen : System.Numerics.IBinaryInteger<TLen> {
    if (TLen.IsNegative(len))
        return -1;

    if (len > TLen.CreateSaturating(int.MaxValue) || GoSliceLimit.ExceedsManagedSpan<T>(long.CreateSaturating(len)))
        throw go.golib.RuntimeErrorPanic.RuntimeRaised($"{function}: length {len} exceeds the maximum slice length of this runtime");

    return int.CreateTruncating(len);
}

public static slice<T> Slice<T, TLen>(ж<T> ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> {
    // Go's len is of any integer type (the `IntegerType` constraint); reduce it to int.
    int n = SpanLength<T, TLen>(len, "unsafe.Slice");

    if (n < 0)
        throw go.golib.RuntimeErrorPanic.ErrorString("unsafe.Slice: len out of range");

    if (ptr == nil)
    {
        // Go's special case: a nil ptr with a zero len is the NIL slice. `[]` built an empty slice over a real
        // (empty) array, which `== nil` reads as non-nil.
        if (n == 0)
            return default;

        throw go.golib.RuntimeErrorPanic.ErrorString("unsafe.Slice: ptr is nil and len is not zero");
    }

    // A zero-size element has no storage to span, so a length beyond any managed span is still a
    // valid Go slice (GoSliceLimit's exemption): it is the storage-free make, never a copy of n elements.
    if (GoSliceLimit.IsZeroSize<T>() && GoSliceLimit.LongerThanAnySpan(n))
        return new slice<T>((nint)n);

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
    {
        // A POINTER element cannot alias native memory: its words are read through the pointer-array kinds, which
        // read memory the Go runtime did not allocate as the foreign addresses its words are (G's ruling,
        // 2026-10-19: crypto/x509's windows chain reads, unsafe.Slice(chainCtx.Chains, n)) and keep the runtime's
        // own words in a shadow. The slice reads the n words (see builtin.NativeArrayPointer).
        if (NativePointerElement<T>.Is)
        {
            slice<T> words = builtin.NativeArrayPointer<T>(ptr.NativeAddress, n).slice(0, n, n);
            NativeSliceAnchors<T>.Keep(words, NativeDerivation.Kept(ptr));
            return words;
        }

        return slice<T>.OverNativeMemory(ptr.NativeAddress, n, n, NativeSliceAnchors<T>.For(NativeDerivation.Kept(ptr)));
    }

    // A pointer INTO managed array/slice storage (`unsafe.Slice(&s[i], n)`) yields a window that
    // ALIASES that storage, exactly as Go's unsafe.Slice does: writes through the rebuilt slice must
    // reach the original backing. The snapshot below silently swallowed them — crypto/subtle's
    // xorBytes rebuilds dst/x/y from `&dst[0]` and writes every XOR result through dst, so XORBytes
    // wrote NOTHING (its whole test matrix compared dst against its untouched 0xdd fill).
    if (ElementWindow.TryGet(ptr, n, out slice<T> window))
        return window;

    // No aliasable managed storage — a heap box, a struct field, or a REINTERPRETING pointer over a
    // differently-typed array (a T[] view over another element type does not exist in the managed
    // model). Reading the n elements through the pinned referent is still exact; writes through the
    // result do not reach the source.
    fixed (T* pointer = &ptr.Value)
        return new slice<T>(new ReadOnlySpan<T>(pointer, n));
}

// Whether T is a Go pointer, ж<E>: a pure type query, decided once per T.
private static class NativePointerElement<T> {
    internal static readonly bool Is = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ж<>);
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
    // ElementWindow.TryGet arm rebuilds a window over the original backing, so a write through the
    // rebuilt slice reaches the source — Go's semantics. Guarded by the UnsafeStringEmpty and
    // UnsafeSliceDataAliasing behavioral tests.
    //
    // UNCHECKED, because Go's SliceData is `&s[:1][0]`: it names the underlying array's first element
    // whenever cap(s) > 0, even at len(s) == 0, where Go's `&s[0]` (the checked Ꮡ) would panic.
    return ElementAddressUnchecked(slice, 0);
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
    int n = SpanLength<byte, TLen>(len, "unsafe.String");

    if (n < 0)
        throw go.golib.RuntimeErrorPanic.ErrorString("unsafe.String: len out of range");

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
        throw go.golib.RuntimeErrorPanic.ErrorString("unsafe.String: ptr is nil and len is not zero");

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
    // This is the exact mirror of Slice's element-window arm above (same ElementWindow.TryGet, same
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
    if (ElementWindow.TryGet(ptr, n, out slice<byte> window))
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
    //
    // A STATIC backing (a string literal's, or a one-byte string's) is the exception: Go keeps those bytes in
    // read-only memory and faults on a write, where here they are shared by the whole process. Such a window gets an
    // element box over one private copy of its backing instead, which keeps pointer identity across calls and reports a
    // write at its next access (golib's ReadOnlyStringBacking). A heap string keeps the alias above, as Go's does.
    slice<byte> window = str.UnsafeBackingWindow();
    return ReadOnlyStringBacking.TryDataBox(window) ?? Ꮡ(window, 0);
}

// Go's `(*[N]T)(unsafe.Pointer(&p.f))` where f's type is not T: the converter emits it as
// `@unsafe.ArrayPointer<T>.Of(Ꮡp.of(S.Ꮡf), N)`, naming the field ONCE, as the Go line does
// (docs/phase4/DESIGN-native-array-view.md, the LookupServicePort door).
//
// It carries both routes. A field one hop off a NATIVE root takes golib's native field-view door, which
// views the field at its GO offset (builtin.NativeFieldArrayPointer). Every other root takes the field view over
// Go storage (Pointer.FieldView): a native array pointer over the bytes the field's root holds still, which keeps
// the root and ends where its storage ends, and for a root with no address (a struct that holds references)
// golib's view of the field's own bytes, read again at every access (section 19). The route lives here rather
// than in golib because golib sits below this package and cannot name Pointer.FromPinnedBox, the retaining pin it
// needs.
//
// So the answer is a native array pointer for every root that has an address, and the forms the converter emits
// over it are the same: ElementRef for an index, at and slice for an address and a slice. Over a root with no
// address the index and the whole-array load and store below are served; a slice and an element's address need
// storage that can be named, and are refused by name.
//
// The other roots took the pointer conversion to the array type until section 18 of the doors' design: first
// spelled `(ж<array<T>>)(uintptr)Pointer.FromPinnedBox(field)`, a cast through a number that dropped the box, then
// the typed door Pointer.To, which kept it (10.6). Either way the box answered was a native box of array<T> over
// the field, whose Value is no array.
//
// T rides on the CLASS and TField on the method so the call site names only T: C# infers TField from
// the field argument, and an extension member cannot take T alone (CS1061 under C# 14).
public static class ArrayPointer<T> {
    public static ж<array<T>> Of<TField>(ж<TField> field, nint length) =>
        builtin.NativeFieldArrayPointer<T>(field, length) ?? Pointer.FieldView<T>(Pointer.FromPinnedBox(field), length, field);

    // THE WHOLE ARRAY THROUGH THE VIEW (section 18, part C). The view has no array<T> of its own, so its Value
    // refuses by name, and the forms that need all N elements go through the elements instead. Both take the view's
    // window, golib's slice of a pointer to an array, which answers for a native array pointer and for an ordinary
    // box alike, with Go's nil dereference for a nil one. A view of a field's bytes over a root with no address has
    // no window, and copies element by element.
    //
    //     v := *p        Load(p)        a Go array value: a copy of the N elements
    //     *p = v         Store(p, v)    N element stores, in place
    //
    // A range that reads the values ranges the same window, `p.slice()`, where the elements are.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static array<T> Load(ж<array<T>> view)
    {
        // A view of a field's bytes over a Go value that has no address has no window: element by element.
        if (view is ReinterpretedArrayBox<T> field)
            return field.Load();

        slice<T> window = view.slice();
        array<T> value = new array<T>((int)window.Length);

        copy(value[..], window);

        return value;
    }

    // A value shorter than N (the zero value of an array that was never given storage) is zeros from where it
    // ends, as Go's is.
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    public static void Store(ж<array<T>> view, in array<T> value)
    {
        if (view is ReinterpretedArrayBox<T> field)
        {
            field.Store(value);
            return;
        }

        slice<T> window = view.slice();

        for (nint i = copy(window, value[..]); i < window.Length; i++)
            window[i] = default!;
    }
}

// Go's `(*[unsafe.Sizeof(s)]byte)(unsafe.Pointer(&s))[lo:hi]` as a direct argument of copy, where s's struct is
// REFERENCE-BEARING (a fixed array inside it is a managed array<T>), so it has no address to view: the converter
// emits `copy(@unsafe.GoImage(Ꮡs).slice(lo, hi), data)`, keeping copy at the site, and golib's GoImage<S> copies
// through the struct's Go-layout image (docs/phase4/DESIGN-reinterpret-seam.md). Any other use of such a view keeps
// the order-token refusal by name.
public static global::go.GoImage<S> GoImage<S>(ж<S> p) => global::go.GoImage<S>.Of(p);

} // end unsafe_package
