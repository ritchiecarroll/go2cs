// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using System.Threading;

[module: go.GoManualConversion]

namespace go.sync;

using static atomic_package;

partial class atomic_package {

// A Value provides an atomic load and store of a consistently typed value.
// The zero value for a Value returns nil from [Value.Load].
//
// A Value must not be copied after first use.
[GoType] public partial struct Value {
    internal any v;
}

} partial class atomicᴛValue {

// NATIVE reimplementation (hand-owned — this file carries [module: GoManualConversion], so a
// -stdlib reconvert will not regenerate the Go version over it). Go's atomic.Value manipulates an
// interface's internal (type, data) words through unsafe.Pointer/efaceWords to load and store an
// `any` atomically. That two-word layout is a Go runtime detail with NO managed equivalent: here an
// `any` is a single System.Object reference, so the whole value is one word. Reinterpreting a
// managed reference as a raw address and poking type/data words simply NREs (the S1
// managed-referent-through-unsafe.Pointer case — internal/testlog's package-level
// `var logger atomic.Value`, loaded during os.Getenv, was the first operational hit). We store the
// value directly in `v` and use Volatile/Interlocked for the acquire/release ordering and CAS the
// literal conversion cannot provide. The nil-store and inconsistent-type panics match Go's spec.

// Load returns the value set by the most recent Store.
// It returns nil if there has been no call to Store for this Value.
[GoRecv] public static any /*val*/ Load(this ref Value v) {
    return Volatile.Read(ref v.v);
}

// Store sets the value of the [Value] v to val.
// All calls to Store for a given Value must use values of the same concrete type.
// Store of an inconsistent type panics, as does Store(nil).
[GoRecv] public static void Store(this ref Value v, any val) {
    if (val == default!) {
        throw panic("sync/atomic: store of nil value into Value");
    }
    while (ᐧ) {
        any cur = Volatile.Read(ref v.v);
        if (cur != default! && GoReflect.GoDynamicTypeOf(cur) != GoReflect.GoDynamicTypeOf(val)) {
            throw panic("sync/atomic: store of inconsistently typed value into Value");
        }
        if (ReferenceEquals(Interlocked.CompareExchange(ref v.v, val, cur), cur)) {
            return;
        }
    }
}

// Swap stores new into Value and returns the previous value. It returns nil if
// the Value is empty.
//
// All calls to Swap for a given Value must use values of the same concrete
// type. Swap of an inconsistent type panics, as does Swap(nil).
[GoRecv] public static any /*old*/ Swap(this ref Value v, any @new) {
    if (@new == default!) {
        throw panic("sync/atomic: swap of nil value into Value");
    }
    while (ᐧ) {
        any cur = Volatile.Read(ref v.v);
        if (cur != default! && GoReflect.GoDynamicTypeOf(cur) != GoReflect.GoDynamicTypeOf(@new)) {
            throw panic("sync/atomic: swap of inconsistently typed value into Value");
        }
        if (ReferenceEquals(Interlocked.CompareExchange(ref v.v, @new, cur), cur)) {
            return cur;
        }
    }
}

// CompareAndSwap executes the compare-and-swap operation for the [Value].
//
// All calls to CompareAndSwap for a given Value must use values of the same
// concrete type. CompareAndSwap of an inconsistent type panics, as does
// CompareAndSwap(old, nil).
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Value v, any old, any @new) {
    if (@new == default!) {
        throw panic("sync/atomic: compare and swap of nil value into Value");
    }
    if (old != default! && GoReflect.GoDynamicTypeOf(old) != GoReflect.GoDynamicTypeOf(@new)) {
        throw panic("sync/atomic: compare and swap of inconsistently typed values");
    }
    while (ᐧ) {
        any cur = Volatile.Read(ref v.v);
        if (cur == default!) {
            // Value is empty: succeeds only against a nil `old`.
            if (old != default!) {
                return false;
            }
            if (ReferenceEquals(Interlocked.CompareExchange(ref v.v, @new, null), null)) {
                return true;
            }
            continue;
        }
        if (GoReflect.GoDynamicTypeOf(cur) != GoReflect.GoDynamicTypeOf(@new)) {
            throw panic("sync/atomic: compare and swap of inconsistently typed value into Value");
        }
        // Compare current to old via runtime equality (value types compare by value), matching Go's
        // `i != old`. A mismatch fails without swapping.
        if (!AreEqual(cur, old)) {
            return false;
        }
        if (ReferenceEquals(Interlocked.CompareExchange(ref v.v, @new, cur), cur)) {
            return true;
        }
    }
}

} partial class atomic_package {

} // end atomic_package
