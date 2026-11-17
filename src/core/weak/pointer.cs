// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
The weak package is a package for managing weak pointers.

Weak pointers are pointers that explicitly do not keep a value live and
must be queried for a regular Go pointer.
The result of such a query may be observed as nil at any point after a
weakly-pointed-to object becomes eligible for reclamation by the garbage
collector.
More specifically, weak pointers become nil as soon as the garbage collector
identifies that the object is unreachable, before it is made reachable
again by a finalizer.
In terms of the C# language, these semantics are roughly equivalent to the
the semantics of "short" weak references.
In terms of the Java language, these semantics are roughly equivalent to the
semantics of the WeakReference type.

Using go:linkname to access this package and the functions it references
is explicitly forbidden by the toolchain because the semantics of this
package have not gone through the proposal process. By exposing this
functionality, we risk locking in the existing semantics due to Hyrum's Law.

If you believe you have a good use-case for weak references not already
covered by the standard library, file a proposal issue at
https://github.com/golang/go/issues instead of relying on this package.
*/

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted pointer.go output). This package's
// whole body is two //go:linkname declarations, and both push bodies live in runtime/mheap.go:
// `registerWeakPointer` → `getOrAddWeakHandle` → `getWeakHandle` → `spanOfHeap`, and
// `makeStrongFromWeak`, which loads a word out of a heap-allocated handle and RE-DERIVES an object
// pointer from that address after sweeping the span it lands in. Both read `mheap_` span metadata the
// managed model does not populate, and the second asks a question — "what object lives at this
// address?" — that the CLR does not answer at all. The converted forms therefore could only fault or,
// far worse, hand back a plausible-looking pointer derived from garbage, so the linkname pair is
// registered UNHONORABLE and announces itself by name (see linknamePushTargets in the converter). This
// file is what those announcements were pointing at.
//
// The remedy is the sync.Mutex / internal/concurrent.HashTrieMap precedent (mutex.cs, hashtriemap.cs):
// runtime-coupled machinery gets a managed-native rewrite that keeps the Go API and its OBSERVABLE
// contract while dropping the mechanism. That is a better fit here than anywhere it has been applied
// so far, because the CLR has first-class weak references of its own and the contract translates
// clause for clause:
//
//   Go's contract                                   | Managed mechanism
//   ------------------------------------------------|-----------------------------------------------
//   Make(ptr) never fails; Strong() yields the       | WeakReference<ж<T>> over the ж<T> box, SHORT
//   ORIGINAL pointer while the referent is reachable | (trackResurrection: false — Go's weak handle
//   and nil once the collector has identified it as  | likewise clears BEFORE a finalizer can make the
//   unreachable, BEFORE a finalizer resurrects it    | object reachable again)
//   A weak pointer does not keep its referent alive  | Nothing on the Pointer<T> → handle → referent
//                                                   | path is a strong reference (see below)
//   Weak handles are UNIQUE AND CANONICAL for each   | A ConditionalWeakTable keyed on the referent
//   byte offset into an object, so two weak pointers | ALLOCATION, whose value is a dictionary keyed
//   made from pointers that compare equal compare    | on the Go POINTER — ж<T>'s own Equals/GetHashCode
//   equal — and pointers to different offsets in one | are exactly Go's pointer identity, including
//   object do NOT                                   | "different fields of one struct are different
//                                                   | addresses". Pointer<T> then compares by handle
//                                                   | REFERENCE, as Go compares the handle address
//   Equality is retained after the referent is       | Pointer<T> holds the handle STRONGLY, so the
//   reclaimed                                        | handle outlives the referent and keeps
//                                                   | answering; it just answers nil forever after
//   A weak pointer created after a resurrection is   | The table entry dies WITH the referent (that is
//   NEWLY UNIQUE, never equal to one made before     | what a ConditionalWeakTable key is), so a later
//                                                   | Make mints a fresh handle
//
// WHY THE CANONICAL TABLE DOES NOT PIN WHAT IT INDEXES — the one subtle claim in this file. The table
// is a ConditionalWeakTable, i.e. an EPHEMERON: its value (and everything the value strongly
// references) is kept alive only while the KEY is independently reachable, and references FROM the
// value TO the key do not count as reachability. So:
//
//   - The key is the referent allocation (ж<T>.ReferentObject — the same lifetime question
//     runtime.SetFinalizer already keys on in mfinal.cs, and answered the same way: an element ref
//     resolves to its backing storage, a field ref to the containing allocation, and a standard heap
//     box to ITSELF, the box being the allocation).
//   - The value holds the ж<T> boxes strongly, as dictionary keys. That is deliberate: for a FIELD or
//     ELEMENT pointer the box is a per-expression view that would otherwise die long before the
//     struct does, and Strong() must keep returning it. The ephemeron makes that safe — the view
//     strong-references the source, but a value→key edge roots nothing.
//   - The handle holds ONLY a WeakReference. Pointer<T> is reachable from wherever the program puts
//     it (unique's intern map, for one), so any strong edge from the handle to the referent would
//     make weak.Pointer keep its value alive — the precise inverse of the package's purpose.
//
// Composing those three, a box is reachable exactly when its referent is, so the plain WeakReference
// tracks the REFERENT's liveness for every pointer shape, not merely the standard-box shape.
//
// ONE SHAPE IS NOT MODELLED, and it is Go's error case too: a box that ALIASES A NATIVE ADDRESS
// (golib's fourth ж kind) names unmanaged storage the collector does not own, so its managed
// reachability is not the Go question. Go answers this by faulting —
// `throw("getWeakHandle on invalid pointer")`, since a non-heap address has no span. Nothing in the
// converted corpus takes a weak pointer to one; if something ever does, it observes an eventual nil
// rather than a fabricated pointer, which is the safe direction.
//
// ⚠ Pointer<T> is written out rather than left to [GoType], and the reason is a defect this hand-own
// also closes. The generated struct equality is field-wise `==` GUARDED on every type parameter
// carrying an IEqualityOperators constraint, and Go's `Pointer[T any]` carries none — so the emitted
// body was literally `Equals(other) => false /* missing equality constraints */`. Two weak pointers to
// the same object NEVER compared equal, which silently defeats unique.Make's
// `m.CompareAndDelete(value, wp)` (it could never match, so a dead entry could never be evicted) and
// contradicts the type's own doc comment above. Equality is the whole reason the runtime canonicalizes
// the handle, so it is hand-written here — and as IEquatable<Pointer<T>>, which the generated form does
// not implement, so EqualityComparer<Pointer<T>>.Default reaches it without boxing on every lookup.
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

// Hand-owned native replacement of the converted pointer.go output — the converter skips regenerating
// a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker). pointer.go is this package's ONLY Go file, so marking it makes the
// whole package hand-owned: the driver `continue`s on unmarkedFileCount == 0 and stops re-emitting
// internal.weak.csproj, package_info.cs and README.md as well (the position internal/godebug and
// internal/concurrent are in). A pointer.cs.auto review sibling IS produced, since 2026-08-19 — the
// fully-hand-owned branch's sibling emitter nil-dereferenced until it was built through the real
// per-file Visitor constructor. The marker's protection is proven in both directions by a seeded
// reconvert.
[module: go.GoManualConversion]

namespace go;

using static weak_package;

partial class weak_package {

// Pointer is a weak pointer to a value of type T.
//
// This value is comparable is guaranteed to compare equal if the pointers
// that they were created from compare equal. This property is retained even
// after the object referenced by the pointer used to create a weak reference
// is reclaimed.
//
// If multiple weak pointers are made to different offsets within same object
// (for example, pointers to different fields of the same struct), those pointers
// will not compare equal.
// If a weak pointer is created from an object that becomes reachable again due
// to a finalizer, that weak pointer will not compare equal with weak pointers
// created before it became unreachable.
public partial struct Pointer<T> : IEquatable<Pointer<T>>
{
    // Go stores an unsafe.Pointer to the runtime's canonical weak HANDLE for the address — never the
    // address itself. Holding the indirection is what makes the three equality clauses in the doc
    // comment above hold, including after the referent is gone. Null is Go's nil `u`: the zero
    // Pointer[T], and the result of Make on a nil pointer.
    internal readonly handle<T>? u;

    internal Pointer(handle<T>? u) => this.u = u;

    public Pointer(NilType _) => u = null;

    // Go compares the two handle ADDRESSES; the managed handle is the object at that address, so
    // reference identity is the same comparison. Canonicalization (see canonicalHandle) is what makes
    // it answer for two independently made weak pointers to one object.
    public bool Equals(Pointer<T> other) => ReferenceEquals(u, other.u);

    public override bool Equals(object? obj) => obj is Pointer<T> other && Equals(other);

    // Identity hash, matching the identity equality above. RuntimeHelpers rather than u.GetHashCode()
    // so it stays an identity hash whatever handle<T> may grow later, and answers 0 for a nil u.
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(u);

    public static bool operator ==(Pointer<T> left, Pointer<T> right) => left.Equals(right);

    public static bool operator !=(Pointer<T> left, Pointer<T> right) => !(left == right);

    // Handle comparisons between 'nil' and struct 'Pointer' — the zero Pointer[T] is the nil one.
    public static bool operator ==(Pointer<T> value, NilType nil) => value.u is null;

    public static bool operator !=(Pointer<T> value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, Pointer<T> value) => value == nil;

    public static bool operator !=(NilType nil, Pointer<T> value) => value != nil;

    public static implicit operator Pointer<T>(NilType nil) => default;

    public override string ToString() => string.Concat("{", u?.ToString() ?? "<nil>", "}");
}

// Make creates a weak pointer from a strong pointer to some value of type T.
public static Pointer<T> Make<T>(ж<T> Ꮡptr) {
    // Go opens with `ptr = abi.Escape(ptr)`, forcing the pointee out of the caller's stack frame so
    // the address the handle records stays valid. There is no managed counterpart to force: a ж<T> IS
    // a heap allocation from construction, whatever its pointee's type (see golib's ж.cs) — the
    // escape has already happened by the time any pointer exists to be weakened.
    if (Ꮡptr == nil) {
        return default;
    }
    handle<T> u = canonicalHandle(Ꮡptr);
    // Go's `runtime.KeepAlive(ptr)` — here the managed primitive it names. Load-bearing rather than
    // decorative: the referent is reachable from this frame ONLY through Ꮡptr, and every use of it
    // above is finished, so without this the collector would be free to take it between the handle's
    // creation and the return, and Make could hand back an already-dead weak pointer. Go's stated
    // invariant is that creating a weak pointer cannot fail.
    GC.KeepAlive(Ꮡptr);
    return new Pointer<T>(u);
}

} partial class weakᴛPointer {

// Strong creates a strong pointer from the weak pointer.
// Returns nil if the original value for the weak pointer was reclaimed by
// the garbage collector.
// If a weak pointer points to an object with a finalizer, then Strong will
// return nil as soon as the object's finalizer is queued for execution.
public static ж<T> Value<T>(this Pointer<T> p) {
    // A nil handle is the zero Pointer[T], which names no referent. Go reaches
    // `(*atomic.Uintptr)(nil).Load()` here and faults; nothing in the converted corpus calls Strong
    // on a zero Pointer, and nil is this method's own documented answer for "no referent", so it is
    // returned rather than a panic invented for an unreachable corner. Same reasoning, and the same
    // shape, as the lazily seeded zero HashTrieMap in hashtriemap.cs.
    return p.u is null ? default! : p.u.Strong();
}

} partial class weak_package {

// The canonical indirection object. Go allocates one `atomic.Uintptr` per weakly-referenced address
// and hangs it off the span as a `specialWeakHandle`; the identity of THAT object is what two weak
// pointers share, and clearing it is how the collector reports the referent gone. Here the managed
// weak reference IS that cleared-by-the-collector word, and this object is the address it lives at.
//
// SHORT weak (trackResurrection: false) is the exact match for Go's "become nil as soon as the
// garbage collector identifies that the object is unreachable, BEFORE it is made reachable again by
// a finalizer" — a long weak reference (true) would keep answering across finalization and resurrect
// values Go promises it will not.
internal sealed class handle<T>
{
    private readonly WeakReference<ж<T>> m_ptr;

    internal handle(ж<T> ptr) => m_ptr = new WeakReference<ж<T>>(ptr, trackResurrection: false);

    // Null — golib's other spelling of a nil Go pointer, the one `== nil` answers true for — once the
    // referent is gone. Go returns an untyped nil unsafe.Pointer at the same moment.
    internal ж<T> Strong() => m_ptr.TryGetTarget(out ж<T>? ptr) ? ptr : default!;

    // Go's `%v` of a Pointer[T] renders the handle's ADDRESS, which is what makes two of them visibly
    // the same or visibly different in a test's failure message. An identity token is the managed
    // stand-in — stable for this object's lifetime and equal exactly when Equals is, the same
    // opaque-token discipline ж<T>'s own printer follows.
    public override string ToString() => "0x" + RuntimeHelpers.GetHashCode(this).ToString("x");
}

// The per-instantiation canonical index. Keyed on the referent ALLOCATION so the entry dies with the
// object it describes, and holding a dictionary keyed on the GO POINTER so distinct offsets within one
// allocation get distinct handles, exactly as Go's per-offset `specialWeakHandle` records do. See the
// file header for why neither level keeps the referent alive.
private static class canon<T>
{
    internal static readonly ConditionalWeakTable<object, ConcurrentDictionary<ж<T>, handle<T>>> perAllocation = new();
}

// Go's getOrAddWeakHandle: retrieve the canonical handle for this address, creating it if this is the
// first weak pointer to it. Both levels are lock-free and idempotent under a race — GetValue and
// GetOrAdd may each build a loser that is discarded, which is precisely what Go does when addspecial
// reports an existing record (it frees its own special and re-reads).
private static handle<T> canonicalHandle<T>(ж<T> Ꮡptr) {
    ConcurrentDictionary<ж<T>, handle<T>> handles =
        canon<T>.perAllocation.GetValue(Ꮡptr.ReferentObject, static _ => new ConcurrentDictionary<ж<T>, handle<T>>());

    return handles.GetOrAdd(Ꮡptr, static ptr => new handle<T>(ptr));
}

} // end weak_package
