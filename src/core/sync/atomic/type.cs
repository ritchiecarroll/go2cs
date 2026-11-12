// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion. Pointer<T> below is rewritten to store its value as a managed ж<T>
// (using Volatile/Interlocked) rather than an unsafe.Pointer: unsafe.Pointer is an alias for nuint
// and the CLR cannot hold a managed reference as a number across a GC move. This module marker is
// detected by containsManualConversionMarker (go2cs/directiveOperations.go); when set, go2cs skips
// re-converting this file so the manual edits are preserved on any future stdlib reconversion.
//
// The scalar types' methods (Bool/Int32/Int64/Uint32/Uint64/Uintptr) are declared in the
// `[GoRecv] (this ref T x)` form — the same shape Value and noCopy already use here — and act on
// `ref x.v` with Volatile/Interlocked directly. The RecvGenerator mints the ж<T> receiver overloads
// (`ref var x = ref Ꮡx.DerefOrNull(); return x.Op(…);`), so both call shapes are ZERO-ALLOCATION.
// The previous hand conversion routed each op through `Ꮡx.of(T.Ꮡv)`, which minted a fresh ж field
// box per call: sync.Once's fast path measured 2 objects/216 B per Do() against Go's 0 (the
// net/textproto TestCommonHeaders want-zero assert, L11). Same primitives, same storage, same
// memory ordering as the doc.cs package-level functions: Load=Volatile.Read,
// Store/Swap=Interlocked.Exchange, CompareAndSwap=Interlocked.CompareExchange, Add/And/Or=Interlocked.
// ZERO-SIZE FIELD LAYOUT (Ruling A, board 2026-08-20). Go's `noCopy` and `align64` are `struct{}` --
// zero bytes -- so `Int32` is FOUR bytes in Go with `v` at offset 0. A C# field always occupies at
// least one byte, so the naive surrogate is EIGHT (the empty struct's byte, then `v` pushed to
// offset 4 by alignment), and `Reinterpret`'s size guard -- which is correct and stays untouched --
// refuses the Go-legal `(*Int32)(unsafe.Pointer(uaddr))` alias the hammer family performs at 8 > 4.
// The atomics then act on a DETACHED COPY and loads answer 0 (measured: `Pointer: 0 != 27809`).
//
// Explicit layout carrying Go's OWN offsets restores the size, and with it the alias. Two details
// are load-bearing:
//
//   - The zero-size fields are `readonly`. C# has no zero-size struct, so a field laid out at the
//     same offset as `v` SHARES bytes with it, and assigning it writes a real zero byte over its
//     neighbour (measured, GolibTests.ZeroSizeFieldLayoutTests: 42 -> 0). Go's write writes nothing.
//     `readonly` makes that one unfaithful operation unexpressible rather than merely unlikely, while
//     the field stays DECLARED so reflect's field walk, NumField() and StructField.Offset still match
//     Go. A whole-struct assignment still writes all Size bytes and stays correct.
//   - It is applied only to the UNMANAGED types here. .NET forbids overlapping a managed reference
//     with anything, so `Pointer<T>` (which holds a ж<T>) and `Value` (which holds an `any`) keep
//     sequential layout; a corpus census puts 59 of the 90 stdlib structs with zero-size fields in
//     that same category.
[module: GoManualConversion]

namespace go.sync;

using @unsafe = unsafe_package;
using System.Threading;
using System.Runtime.InteropServices;
using static atomic_package;

partial class atomic_package {

// A Bool is an atomic boolean value.
// The zero value is false.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 4)] public partial struct Bool {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal uint32 v;
}

} partial class atomicᴛBool {

// Load atomically loads and returns the value stored in x.
[GoRecv] public static bool Load(this ref Bool x) {
    return Volatile.Read(ref x.v) != 0;
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Bool x, bool val) {
    Interlocked.Exchange(ref x.v, b32(val));
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static bool /*old*/ Swap(this ref Bool x, bool @new) {
    return Interlocked.Exchange(ref x.v, b32(@new)) != 0;
}

// CompareAndSwap executes the compare-and-swap operation for the boolean value x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Bool x, bool old, bool @new) {
    uint32 old32 = b32(old);
    return Interlocked.CompareExchange(ref x.v, b32(@new), old32) == old32;
}

} partial class atomic_package {

// b32 returns a uint32 0 or 1 representing b.
internal static uint32 b32(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

// For testing *Pointer[T]'s methods can be inlined.
// Keep in sync with cmd/compile/internal/test/inl_test.go:TestIntendedInlining.
internal static ж<Pointer<nint>> _ᴛ1ʗ = Ꮡ(new Pointer<nint>(nil));

// A Pointer is an atomic pointer of type *T. The zero value is a nil *T.
[GoType] public partial struct Pointer<T>{
    // Mention *T in a field to disallow conversion between Pointer types.
    // See go.dev/issue/56603 for more details.
    // Use *T, not T, to avoid spurious recursive type definition errors.
    internal array<ж<T>> _ = new(0);
    internal noCopy __;
    // go2cs: Go stores the value as an unsafe.Pointer (a raw machine address). In .NET a managed
    // pointer (ж<T>) cannot be held as a number and survive a GC move, so this slot holds the ж<T>
    // directly and the operations below use Volatile/Interlocked for atomicity. A null slot is a
    // nil *T; nilCanon collapses an explicit nil-ж to null so reference-based CompareAndSwap treats
    // every nil pointer as equal (matching Go, where a nil unsafe.Pointer compares equal to nil) —
    // a pointer TO a nil value is NOT such a pointer and stays exactly as stored (see nilCanon).
    internal ж<T> v;
}

// nilCanon canonicalizes THE NIL POINTER to null so the reference comparison in CompareAndSwap
// treats all nil *T values as equal.
//
// The predicate must be the STRUCTURAL one, ж<T>.IsNilPointer ("this box IS the nil pointer"), not
// ж<T>.IsNull. IsNull also reports true for a real box whose HELD reference-typed value happens to be
// null — a pointer TO nil, which in Go is an ordinary non-nil address (`&i` is a real address even
// when `i == nil`, and `new(any)` is a real address holding a nil interface). Asking IsNull collapsed
// every such pointer to nil, and sync.Map is built out of exactly that shape: `e.p.Store(&i)` with a
// nil `any` lost the value outright (Range skipped the entry, CompareAndSwap failed against it), and
// its `expunged = new(any)` sentinel was indistinguishable from nil, so a deleted entry could not be
// told from an expunged one. Same conflation for atomic.Pointer[error], [func()], and a **T slot.
internal static ж<T> nilCanon<T>(ж<T> p){
    return p is null || p.IsNilPointer ? default! : p;
}

} partial class atomicᴛPointer {

// A NATIVE-backed ж<Pointer<T>> is the hammer family's `(*Pointer[byte])(unsafe.Pointer(&val))`
// reinterpret over a uint64: the box aliases raw pinned storage, and Go's semantics for those 8
// bytes are the POINTER'S VALUE (possibly a fabricated number — the test's own comment says
// "values that aren't real pointers"). Dereferencing the box as a Pointer<T> struct would read
// the word as a managed ж<T> REFERENCE — the number is lost and an untraced reference lands in
// memory the GC never scans (measured: TestHammerStoreLoad's Method arm, the same
// `Pointer: 0 != N` shape as the function arm). So each method's native arm goes through golib's
// atomic pointer-word accessors and converts number ↔ box at the boundary: `(uintptr)Ꮡp` answers a
// ж<T>'s number nil-safely (a native box answers its exact address, nil answers 0), and
// `(ж<T>)(uintptr)n` minting is the same conversion the emitted reinterpret itself uses — a zero
// word minting the nil-marked box, so nil round-trips exactly. The managed arms are unchanged.

// Load atomically loads and returns the value stored in x.
public static ж<T> Load<T>(this ж<Pointer<T>> Ꮡx){
    if (Ꮡx.IsNative)
        return (ж<T>)(uintptr)Ꮡx.ReadPointerWord();

    ref var x = ref Ꮡx.Value;

    return Volatile.Read(ref x.v);
}

// Store atomically stores val into x.
public static void Store<T>(this ж<Pointer<T>> Ꮡx, ж<T> Ꮡval){
    if (Ꮡx.IsNative) {
        Ꮡx.ExchangePointerWord(((uintptr)Ꮡval).Value);
        return;
    }

    ref var x = ref Ꮡx.Value;

    Volatile.Write(ref x.v, nilCanon(Ꮡval));
}

// Swap atomically stores new into x and returns the previous value.
public static ж<T> /*old*/ Swap<T>(this ж<Pointer<T>> Ꮡx, ж<T> Ꮡnew){
    if (Ꮡx.IsNative)
        return (ж<T>)(uintptr)Ꮡx.ExchangePointerWord(((uintptr)Ꮡnew).Value);

    ref var x = ref Ꮡx.Value;

    return Interlocked.Exchange(ref x.v, nilCanon(Ꮡnew));
}

// CompareAndSwap executes the compare-and-swap operation for x.
public static bool /*swapped*/ CompareAndSwap<T>(this ж<Pointer<T>> Ꮡx, ж<T> Ꮡold, ж<T> Ꮡnew){
    // Native arm: the slot holds the pointer's VALUE, so the CAS compares NUMBERS — Go's own
    // pointer comparison, and nil (word 0) participates exactly.
    if (Ꮡx.IsNative)
        return Ꮡx.CompareExchangePointerWord(((uintptr)Ꮡold).Value, ((uintptr)Ꮡnew).Value);

    ref var x = ref Ꮡx.Value;

    ж<T> old = nilCanon(Ꮡold);
    return ReferenceEquals(Interlocked.CompareExchange(ref x.v, nilCanon(Ꮡnew), old), old);
}

} partial class atomic_package {

// An Int32 is an atomic int32. The zero value is zero.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 4)] public partial struct Int32 {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal int32 v;
}

} partial class atomicᴛInt32 {

// Load atomically loads and returns the value stored in x.
[GoRecv] public static int32 Load(this ref Int32 x) {
    return Volatile.Read(ref x.v);
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Int32 x, int32 val) {
    Interlocked.Exchange(ref x.v, val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static int32 /*old*/ Swap(this ref Int32 x, int32 @new) {
    return Interlocked.Exchange(ref x.v, @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Int32 x, int32 old, int32 @new) {
    return Interlocked.CompareExchange(ref x.v, @new, old) == old;
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static int32 /*new*/ Add(this ref Int32 x, int32 delta) {
    return Interlocked.Add(ref x.v, delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int32 /*old*/ And(this ref Int32 x, int32 mask) {
    return Interlocked.And(ref x.v, mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int32 /*old*/ Or(this ref Int32 x, int32 mask) {
    return Interlocked.Or(ref x.v, mask);
}

} partial class atomic_package {

// An Int64 is an atomic int64. The zero value is zero.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct Int64 {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal readonly align64 __;
    [FieldOffset(0)] internal int64 v;
}

} partial class atomicᴛInt64 {

// Load atomically loads and returns the value stored in x.
[GoRecv] public static int64 Load(this ref Int64 x) {
    return Volatile.Read(ref x.v);
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Int64 x, int64 val) {
    Interlocked.Exchange(ref x.v, val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static int64 /*old*/ Swap(this ref Int64 x, int64 @new) {
    return Interlocked.Exchange(ref x.v, @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Int64 x, int64 old, int64 @new) {
    return Interlocked.CompareExchange(ref x.v, @new, old) == old;
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static int64 /*new*/ Add(this ref Int64 x, int64 delta) {
    return Interlocked.Add(ref x.v, delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int64 /*old*/ And(this ref Int64 x, int64 mask) {
    return Interlocked.And(ref x.v, mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static int64 /*old*/ Or(this ref Int64 x, int64 mask) {
    return Interlocked.Or(ref x.v, mask);
}

} partial class atomic_package {

// A Uint32 is an atomic uint32. The zero value is zero.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 4)] public partial struct Uint32 {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal uint32 v;
}

} partial class atomicᴛUint32 {

// Load atomically loads and returns the value stored in x.
[GoRecv] public static uint32 Load(this ref Uint32 x) {
    return Volatile.Read(ref x.v);
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Uint32 x, uint32 val) {
    Interlocked.Exchange(ref x.v, val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static uint32 /*old*/ Swap(this ref Uint32 x, uint32 @new) {
    return Interlocked.Exchange(ref x.v, @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Uint32 x, uint32 old, uint32 @new) {
    return Interlocked.CompareExchange(ref x.v, @new, old) == old;
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static uint32 /*new*/ Add(this ref Uint32 x, uint32 delta) {
    return Interlocked.Add(ref x.v, delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint32 /*old*/ And(this ref Uint32 x, uint32 mask) {
    return Interlocked.And(ref x.v, mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint32 /*old*/ Or(this ref Uint32 x, uint32 mask) {
    return Interlocked.Or(ref x.v, mask);
}

} partial class atomic_package {

// A Uint64 is an atomic uint64. The zero value is zero.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct Uint64 {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal readonly align64 __;
    [FieldOffset(0)] internal uint64 v;
}

} partial class atomicᴛUint64 {

// Load atomically loads and returns the value stored in x.
[GoRecv] public static uint64 Load(this ref Uint64 x) {
    return Volatile.Read(ref x.v);
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Uint64 x, uint64 val) {
    Interlocked.Exchange(ref x.v, val);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static uint64 /*old*/ Swap(this ref Uint64 x, uint64 @new) {
    return Interlocked.Exchange(ref x.v, @new);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Uint64 x, uint64 old, uint64 @new) {
    return Interlocked.CompareExchange(ref x.v, @new, old) == old;
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static uint64 /*new*/ Add(this ref Uint64 x, uint64 delta) {
    return Interlocked.Add(ref x.v, delta);
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint64 /*old*/ And(this ref Uint64 x, uint64 mask) {
    return Interlocked.And(ref x.v, mask);
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uint64 /*old*/ Or(this ref Uint64 x, uint64 mask) {
    return Interlocked.Or(ref x.v, mask);
}

} partial class atomic_package {

// A Uintptr is an atomic uintptr. The zero value is zero.
[GoType] [StructLayout(LayoutKind.Explicit, Size = 8)] public partial struct Uintptr {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal uintptr v;
}

} partial class atomicᴛUintptr {

// Load atomically loads and returns the value stored in x.
[GoRecv] public static uintptr Load(this ref Uintptr x) {
    // uintptr is a golib struct; atomics target its inner nuint storage (same as doc_impl.cs).
    return Volatile.Read(ref x.v.Value);
}

// Store atomically stores val into x.
[GoRecv] public static void Store(this ref Uintptr x, uintptr val) {
    Interlocked.Exchange(ref x.v.Value, val.Value);
}

// Swap atomically stores new into x and returns the previous value.
[GoRecv] public static uintptr /*old*/ Swap(this ref Uintptr x, uintptr @new) {
    return Interlocked.Exchange(ref x.v.Value, @new.Value);
}

// CompareAndSwap executes the compare-and-swap operation for x.
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Uintptr x, uintptr old, uintptr @new) {
    return Interlocked.CompareExchange(ref x.v.Value, @new.Value, old.Value) == old.Value;
}

// Add atomically adds delta to x and returns the new value.
[GoRecv] public static uintptr /*new*/ Add(this ref Uintptr x, uintptr delta) {
    // No Interlocked.Add over nuint: CAS loop, mirroring doc_impl.cs AddUintptr.
    nuint initialValue, newValue;

    do
    {
        initialValue = Volatile.Read(ref x.v.Value);
        newValue = initialValue + delta;
    }
    while (Interlocked.CompareExchange(ref x.v.Value, newValue, initialValue) != initialValue);

    return newValue;
}

// And atomically performs a bitwise AND operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uintptr /*old*/ And(this ref Uintptr x, uintptr mask) {
    // No Interlocked.And over nuint: CAS loop. Returns the OLD value — Go's contract
    // (`func (x *Uintptr) And(mask uintptr) (old uintptr)`), matching what Interlocked.And
    // returns for the fixed-width types above.
    nuint initialValue, newValue;

    do
    {
        initialValue = Volatile.Read(ref x.v.Value);
        newValue = initialValue & mask;
    }
    while (Interlocked.CompareExchange(ref x.v.Value, newValue, initialValue) != initialValue);

    return initialValue;
}

// Or atomically performs a bitwise OR operation on x using the bitmask
// provided as mask and returns the old value.
[GoRecv] public static uintptr /*old*/ Or(this ref Uintptr x, uintptr mask) {
    // CAS loop returning the OLD value — Go's contract; see And above.
    nuint initialValue, newValue;

    do
    {
        initialValue = Volatile.Read(ref x.v.Value);
        newValue = initialValue | mask;
    }
    while (Interlocked.CompareExchange(ref x.v.Value, newValue, initialValue) != initialValue);

    return initialValue;
}

} partial class atomic_package {

// noCopy may be added to structs which must not be copied
// after the first use.
//
// See https://golang.org/issues/8005#issuecomment-190753527
// for details.
//
// Note that it must not be embedded, due to the Lock and Unlock methods.
[GoType] internal partial struct noCopy {
}

} partial class atomicᴛnoCopy {

// Lock is a no-op used by -copylocks checker from `go vet`.
[GoRecv] internal static void Lock(this ref noCopy _) {
}

[GoRecv] internal static void Unlock(this ref noCopy _) {
}

} partial class atomic_package {

// align64 may be added to structs that must be 64-bit aligned.
// This struct is recognized by a special case in the compiler
// and will not work if copied to any other package.
[GoType] partial struct align64 {
}

} // end atomic_package
