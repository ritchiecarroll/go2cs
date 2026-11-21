// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using @unsafe = unsafe_package;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using atomic = atomic_package;
using static atomic_package;

partial class atomic_package {

// Int32 is an atomically accessed int32 value.
//
// An Int32 must not be copied.
[StructLayout(LayoutKind.Explicit, Size = 4)] partial struct Int32 {
    [FieldOffset(0)] internal readonly noCopy noCopy;
    [FieldOffset(0)] internal int32 value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static int32 Loadᴍ(ж<Int32> Ꮡi) {
    return Loadint32(Ꮡi.Ꮡvalue);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Int32> Ꮡi, int32 value) {
    Storeint32(Ꮡi.Ꮡvalue, value);
}

// CompareAndSwap atomically compares i's value with old,
// and if they're equal, swaps i's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ(ж<Int32> Ꮡi, int32 old, int32 @new) {
    return Casint32(Ꮡi.Ꮡvalue, old, @new);
}

// Swap replaces i's value with new, returning
// i's value before the replacement.
//
//go:nosplit
[GoMethodBody("Swap")] internal static int32 Swapᴍ(ж<Int32> Ꮡi, int32 @new) {
    return Xchgint32(Ꮡi.Ꮡvalue, @new);
}

// Add adds delta to i atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoMethodBody("Add")] internal static int32 Addᴍ(ж<Int32> Ꮡi, int32 delta) {
    return Xaddint32(Ꮡi.Ꮡvalue, delta);
}

} partial class atomic_package {

// Int64 is an atomically accessed int64 value.
//
// 8-byte aligned on all platforms, unlike a regular int64.
//
// An Int64 must not be copied.
[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Int64 {
    [FieldOffset(0)] internal readonly noCopy noCopy;
    [FieldOffset(0)] internal readonly align64 _;
    [FieldOffset(0)] internal int64 value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static int64 Loadᴍ(ж<Int64> Ꮡi) {
    return Loadint64(Ꮡi.Ꮡvalue);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Int64> Ꮡi, int64 value) {
    Storeint64(Ꮡi.Ꮡvalue, value);
}

// CompareAndSwap atomically compares i's value with old,
// and if they're equal, swaps i's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ(ж<Int64> Ꮡi, int64 old, int64 @new) {
    return Casint64(Ꮡi.Ꮡvalue, old, @new);
}

// Swap replaces i's value with new, returning
// i's value before the replacement.
//
//go:nosplit
[GoMethodBody("Swap")] internal static int64 Swapᴍ(ж<Int64> Ꮡi, int64 @new) {
    return Xchgint64(Ꮡi.Ꮡvalue, @new);
}

// Add adds delta to i atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoMethodBody("Add")] internal static int64 Addᴍ(ж<Int64> Ꮡi, int64 delta) {
    return Xaddint64(Ꮡi.Ꮡvalue, delta);
}

} partial class atomic_package {

// Uint8 is an atomically accessed uint8 value.
//
// A Uint8 must not be copied.
[StructLayout(LayoutKind.Explicit, Size = 1)] partial struct Uint8 {
    [FieldOffset(0)] internal readonly noCopy noCopy;
    [FieldOffset(0)] internal uint8 value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static uint8 Loadᴍ(ж<Uint8> Ꮡu) {
    return Load8(Ꮡu.Ꮡvalue);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Uint8> Ꮡu, uint8 value) {
    Store8(Ꮡu.Ꮡvalue, value);
}

// And takes value and performs a bit-wise
// "and" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoMethodBody("And")] internal static void Andᴍ(ж<Uint8> Ꮡu, uint8 value) {
    And8(Ꮡu.Ꮡvalue, value);
}

// Or takes value and performs a bit-wise
// "or" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoMethodBody("Or")] internal static void Orᴍ(ж<Uint8> Ꮡu, uint8 value) {
    Or8(Ꮡu.Ꮡvalue, value);
}

} partial class atomic_package {

// Bool is an atomically accessed bool value.
//
// A Bool must not be copied.
partial struct Bool {
    // Inherits noCopy from Uint8.
    internal Uint8 u;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static bool Loadᴍ(ж<Bool> Ꮡb) {
    return Ꮡb.Ꮡu.Load() != 0;
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Bool> Ꮡb, bool value) {
    var s = (uint8)0;
    if (value) {
        s = 1;
    }
    Ꮡb.Ꮡu.Store(s);
}

} partial class atomic_package {

// Uint32 is an atomically accessed uint32 value.
//
// A Uint32 must not be copied.
[StructLayout(LayoutKind.Explicit, Size = 4)] partial struct Uint32 {
    [FieldOffset(0)] internal readonly noCopy noCopy;
    [FieldOffset(0)] internal uint32 value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static uint32 Loadᴍ(ж<Uint32> Ꮡu) {
    return atomic.Load(Ꮡu.Ꮡvalue);
}

// LoadAcquire is a partially unsynchronized version
// of Load that relaxes ordering constraints. Other threads
// may observe operations that precede this operation to
// occur after it, but no operation that occurs after it
// on this thread can be observed to occur before it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoMethodBody("LoadAcquire")] internal static uint32 LoadAcquireᴍ(ж<Uint32> Ꮡu) {
    return LoadAcq(Ꮡu.Ꮡvalue);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Uint32> Ꮡu, uint32 value) {
    atomic.Store(Ꮡu.Ꮡvalue, value);
}

// StoreRelease is a partially unsynchronized version
// of Store that relaxes ordering constraints. Other threads
// may observe operations that occur after this operation to
// precede it, but no operation that precedes it
// on this thread can be observed to occur after it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoMethodBody("StoreRelease")] internal static void StoreReleaseᴍ(ж<Uint32> Ꮡu, uint32 value) {
    StoreRel(Ꮡu.Ꮡvalue, value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ(ж<Uint32> Ꮡu, uint32 old, uint32 @new) {
    return Cas(Ꮡu.Ꮡvalue, old, @new);
}

// CompareAndSwapRelease is a partially unsynchronized version
// of Cas that relaxes ordering constraints. Other threads
// may observe operations that occur after this operation to
// precede it, but no operation that precedes it
// on this thread can be observed to occur after it.
// It reports whether the swap ran.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoMethodBody("CompareAndSwapRelease")] internal static bool CompareAndSwapReleaseᴍ(ж<Uint32> Ꮡu, uint32 old, uint32 @new) {
    return CasRel(Ꮡu.Ꮡvalue, old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
[GoMethodBody("Swap")] internal static uint32 Swapᴍ(ж<Uint32> Ꮡu, uint32 value) {
    return Xchg(Ꮡu.Ꮡvalue, value);
}

// And takes value and performs a bit-wise
// "and" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoMethodBody("And")] internal static void Andᴍ(ж<Uint32> Ꮡu, uint32 value) {
    atomic.And(Ꮡu.Ꮡvalue, value);
}

// Or takes value and performs a bit-wise
// "or" operation with the value of u, storing
// the result into u.
//
// The full process is performed atomically.
//
//go:nosplit
[GoMethodBody("Or")] internal static void Orᴍ(ж<Uint32> Ꮡu, uint32 value) {
    atomic.Or(Ꮡu.Ꮡvalue, value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoMethodBody("Add")] internal static uint32 Addᴍ(ж<Uint32> Ꮡu, int32 delta) {
    return Xadd(Ꮡu.Ꮡvalue, delta);
}

} partial class atomic_package {

// Uint64 is an atomically accessed uint64 value.
//
// 8-byte aligned on all platforms, unlike a regular uint64.
//
// A Uint64 must not be copied.
[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Uint64 {
    [FieldOffset(0)] internal readonly noCopy noCopy;
    [FieldOffset(0)] internal readonly align64 _;
    [FieldOffset(0)] internal uint64 value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static uint64 Loadᴍ(ж<Uint64> Ꮡu) {
    return Load64(Ꮡu.Ꮡvalue);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Uint64> Ꮡu, uint64 value) {
    Store64(Ꮡu.Ꮡvalue, value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ(ж<Uint64> Ꮡu, uint64 old, uint64 @new) {
    return Cas64(Ꮡu.Ꮡvalue, old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
[GoMethodBody("Swap")] internal static uint64 Swapᴍ(ж<Uint64> Ꮡu, uint64 value) {
    return Xchg64(Ꮡu.Ꮡvalue, value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoMethodBody("Add")] internal static uint64 Addᴍ(ж<Uint64> Ꮡu, int64 delta) {
    return Xadd64(Ꮡu.Ꮡvalue, delta);
}

} partial class atomic_package {

// Uintptr is an atomically accessed uintptr value.
//
// A Uintptr must not be copied.
[StructLayout(LayoutKind.Explicit, Size = 8)] partial struct Uintptr {
    [FieldOffset(0)] internal readonly noCopy noCopy;
    [FieldOffset(0)] internal uintptr value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static uintptr Loadᴍ(ж<Uintptr> Ꮡu) {
    return Loaduintptr(Ꮡu.Ꮡvalue);
}

// LoadAcquire is a partially unsynchronized version
// of Load that relaxes ordering constraints. Other threads
// may observe operations that precede this operation to
// occur after it, but no operation that occurs after it
// on this thread can be observed to occur before it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoMethodBody("LoadAcquire")] internal static uintptr LoadAcquireᴍ(ж<Uintptr> Ꮡu) {
    return LoadAcquintptr(Ꮡu.Ꮡvalue);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Uintptr> Ꮡu, uintptr value) {
    Storeuintptr(Ꮡu.Ꮡvalue, value);
}

// StoreRelease is a partially unsynchronized version
// of Store that relaxes ordering constraints. Other threads
// may observe operations that occur after this operation to
// precede it, but no operation that precedes it
// on this thread can be observed to occur after it.
//
// WARNING: Use sparingly and with great care.
//
//go:nosplit
[GoMethodBody("StoreRelease")] internal static void StoreReleaseᴍ(ж<Uintptr> Ꮡu, uintptr value) {
    StoreReluintptr(Ꮡu.Ꮡvalue, value);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
//
//go:nosplit
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ(ж<Uintptr> Ꮡu, uintptr old, uintptr @new) {
    return Casuintptr(Ꮡu.Ꮡvalue, old, @new);
}

// Swap replaces u's value with new, returning
// u's value before the replacement.
//
//go:nosplit
[GoMethodBody("Swap")] internal static uintptr Swapᴍ(ж<Uintptr> Ꮡu, uintptr value) {
    return Xchguintptr(Ꮡu.Ꮡvalue, value);
}

// Add adds delta to u atomically, returning
// the new updated value.
//
// This operation wraps around in the usual
// two's-complement way.
//
//go:nosplit
[GoMethodBody("Add")] internal static uintptr Addᴍ(ж<Uintptr> Ꮡu, uintptr delta) {
    return Xadduintptr(Ꮡu.Ꮡvalue, delta);
}

} partial class atomic_package {

// Float64 is an atomically accessed float64 value.
//
// 8-byte aligned on all platforms, unlike a regular float64.
//
// A Float64 must not be copied.
partial struct Float64 {
    // Inherits noCopy and align64 from Uint64.
    internal Uint64 u;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static float64 Loadᴍ(ж<Float64> Ꮡf) {
    var r = Ꮡf.Ꮡu.Load();
    return bitcast<uint64, float64>(r);
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ(ж<Float64> Ꮡf, float64 value) {
    Ꮡf.Ꮡu.Store(bitcast<float64, uint64>(value));
}

} partial class atomic_package {

// UnsafePointer is an atomically accessed unsafe.Pointer value.
//
// Note that because of the atomicity guarantees, stores to values
// of this type never trigger a write barrier, and the relevant
// methods are suffixed with "NoWB" to indicate that explicitly.
// As a result, this type should be used carefully, and sparingly,
// mostly with values that do not live in the Go heap anyway.
//
// An UnsafePointer must not be copied.
partial struct UnsafePointer {
    internal noCopy noCopy;
    internal @unsafe.Pointer value;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static @unsafe.Pointer Loadᴍ(ж<UnsafePointer> Ꮡu) {
    return Loadp(@unsafe.Pointer.FromBox(Ꮡu.Ꮡvalue));
}

// StoreNoWB updates the value atomically.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer Store instead.
//
//go:nosplit
[GoMethodBody("StoreNoWB")] internal static void StoreNoWBᴍ(ж<UnsafePointer> Ꮡu, @unsafe.Pointer value) {
    StorepNoWB(@unsafe.Pointer.FromBox(Ꮡu.Ꮡvalue), value);
}

// Store updates the value atomically.
[GoMethodBody("Store")] internal static void Storeᴍ(ж<UnsafePointer> Ꮡu, @unsafe.Pointer value) {
    storePointer(Ꮡu.Ꮡvalue, value);
}

} partial class atomic_package {

// provided by runtime
//
//go:linkname storePointer
internal static partial void storePointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer @new);

} partial class atomic_package {

// CompareAndSwapNoWB atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer CompareAndSwap instead.
//
//go:nosplit
[GoMethodBody("CompareAndSwapNoWB")] internal static bool CompareAndSwapNoWBᴍ(ж<UnsafePointer> Ꮡu, @unsafe.Pointer old, @unsafe.Pointer @new) {
    return Casp1(Ꮡu.Ꮡvalue, old, @new);
}

// CompareAndSwap atomically compares u's value with old,
// and if they're equal, swaps u's value with new.
// It reports whether the swap ran.
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ(ж<UnsafePointer> Ꮡu, @unsafe.Pointer old, @unsafe.Pointer @new) {
    return casPointer(Ꮡu.Ꮡvalue, old, @new);
}

} partial class atomic_package {

internal static partial bool casPointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new);

// Pointer is an atomic pointer of type *T.
partial struct Pointer<T> {
    internal UnsafePointer u;
}

} partial class atomic_package {

// Load accesses and returns the value atomically.
//
//go:nosplit
[GoMethodBody("Load")] internal static ж<T> Loadᴍ<T>(ж<Pointer<T>> Ꮡp) {
    return @unsafe.Pointer.To<T>(Ꮡp.Ꮡu.Load());
}

// StoreNoWB updates the value atomically.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer Store instead.
//
//go:nosplit
[GoMethodBody("StoreNoWB")] internal static void StoreNoWBᴍ<T>(ж<Pointer<T>> Ꮡp, ж<T> value) {
    Ꮡp.Ꮡu.StoreNoWB(@unsafe.Pointer.FromPinnedBox(value));
}

// Store updates the value atomically.
//
//go:nosplit
[GoMethodBody("Store")] internal static void Storeᴍ<T>(ж<Pointer<T>> Ꮡp, ж<T> value) {
    Ꮡp.Ꮡu.Store(@unsafe.Pointer.FromPinnedBox(value));
}

// CompareAndSwapNoWB atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
//
// WARNING: As the name implies this operation does *not*
// perform a write barrier on value, and so this operation may
// hide pointers from the GC. Use with care and sparingly.
// It is safe to use with values not found in the Go heap.
// Prefer CompareAndSwap instead.
//
//go:nosplit
[GoMethodBody("CompareAndSwapNoWB")] internal static bool CompareAndSwapNoWBᴍ<T>(ж<Pointer<T>> Ꮡp, ж<T> old, ж<T> @new) {
    return Ꮡp.Ꮡu.CompareAndSwapNoWB(@unsafe.Pointer.FromPinnedBox(old), @unsafe.Pointer.FromPinnedBox(@new));
}

// CompareAndSwap atomically (with respect to other methods)
// compares u's value with old, and if they're equal,
// swaps u's value with new.
// It reports whether the swap ran.
[GoMethodBody("CompareAndSwap")] internal static bool CompareAndSwapᴍ<T>(ж<Pointer<T>> Ꮡp, ж<T> old, ж<T> @new) {
    return Ꮡp.Ꮡu.CompareAndSwap(@unsafe.Pointer.FromPinnedBox(old), @unsafe.Pointer.FromPinnedBox(@new));
}

} partial class atomic_package {

// noCopy may be embedded into structs which must not be copied
// after the first use.
//
// See https://golang.org/issues/8005#issuecomment-190753527
// for details.
partial struct noCopy {
}

} partial class atomic_package {

// Lock is a no-op used by -copylocks checker from `go vet`.
[GoMethodBody("Lock")] internal static void Lockᴍ(ref noCopy _) { }
[GoMethodBody("Unlock")] internal static void Unlockᴍ(ref noCopy _) { }

} partial class atomic_package {

// align64 may be added to structs that must be 64-bit aligned.
// This struct is recognized by a special case in the compiler
// and will not work if copied to any other package.
partial struct align64 {
}

} // end atomic_package
