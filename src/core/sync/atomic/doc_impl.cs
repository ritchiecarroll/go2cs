// doc_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System.Threading;

namespace go.sync;

using @unsafe = unsafe_package;

partial class atomic_package
{
    public static partial int32 /*old*/ SwapInt32(ж<int32> addr, int32 @new)
    {
        return Interlocked.Exchange(ref addr.Value, @new);
    }

    public static partial int64 /*old*/ SwapInt64(ж<int64> addr, int64 @new)
    {
        return Interlocked.Exchange(ref addr.Value, @new);
    }

    public static partial uint32 /*old*/ SwapUint32(ж<uint32> addr, uint32 @new)
    {
        return Interlocked.Exchange(ref addr.Value, @new);
    }

    public static partial uint64 /*old*/ SwapUint64(ж<uint64> addr, uint64 @new)
    {
        return Interlocked.Exchange(ref addr.Value, @new);
    }

    public static partial uintptr /*old*/ SwapUintptr(ж<uintptr> addr, uintptr @new)
    {
        // uintptr is a golib struct; atomics target its inner nuint storage
        return Interlocked.Exchange(ref addr.Value.Value, @new.Value);
    }

    // ---- the NATIVE-backed *unsafe.Pointer arm, shared by all four Pointer atomics -------------
    //
    // `(*unsafe.Pointer)(unsafe.Pointer(&val))` over a uint64 — the hammer family's reinterpret —
    // reaches these functions as a NATIVE-backed ж<@unsafe.Pointer>: a box aliasing raw pinned
    // storage rather than holding a managed slot. Go's semantics for that memory are that the 8
    // bytes hold the POINTER'S VALUE (possibly a fabricated number — Go's own test comment:
    // "values that aren't real pointers"). Dereferencing such a box as a managed reference slot
    // (`ref addr.Value`) both loses the number and plants an untraced CLR reference in memory the
    // GC never scans — the stored box is collectible the moment the store returns, and the next
    // load resurrects a dangling reference (measured: TestHammerStoreLoad, `Pointer: 0 != N`).
    // So the native arm reads and writes the slot as the pointer-sized WORD it is (golib's
    // pointer-word accessors — the one unsafe seam, kept in golib because this project compiles
    // with AllowUnsafeBlocks=false), and converts number ↔ box here, where the type is known:
    // `(uintptr)p` answers a Pointer's number nil-safely in one existing conversion, and
    // `new @unsafe.Pointer(n)` marks the zero address nil, so a nil pointer round-trips exactly.
    // The managed arm is unchanged.

    public static partial @unsafe.Pointer /*old*/ SwapPointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer @new)
    {
        if (addr.IsNative)
            return new @unsafe.Pointer((uintptr)addr.ExchangePointerWord(((uintptr)@new).Value));

        return Interlocked.Exchange(ref addr.Value, @new);
    }

    public static partial bool /*swapped*/ CompareAndSwapInt32(ж<int32> addr, int32 old, int32 @new)
    {
        return Interlocked.CompareExchange(ref addr.Value, @new, old) == old;
    }

    public static partial bool /*swapped*/ CompareAndSwapInt64(ж<int64> addr, int64 old, int64 @new)
    {
        return Interlocked.CompareExchange(ref addr.Value, @new, old) == old;
    }

    public static partial bool /*swapped*/ CompareAndSwapUint32(ж<uint32> addr, uint32 old, uint32 @new)
    {
        return Interlocked.CompareExchange(ref addr.Value, @new, old) == old;
    }

    public static partial bool /*swapped*/ CompareAndSwapUint64(ж<uint64> addr, uint64 old, uint64 @new)
    {
        return Interlocked.CompareExchange(ref addr.Value, @new, old) == old;
    }

    public static partial bool /*swapped*/ CompareAndSwapUintptr(ж<uintptr> addr, uintptr old, uintptr @new)
    {
        return Interlocked.CompareExchange(ref addr.Value.Value, @new.Value, old.Value) == old.Value;
    }

    public static partial bool /*swapped*/ CompareAndSwapPointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer old, @unsafe.Pointer @new)
    {
        // Native arm: the slot holds the pointer's VALUE, so the CAS compares NUMBERS — which is
        // Go's own comparison for unsafe.Pointer, and the same one Pointer.Equals already answers.
        if (addr.IsNative)
            return addr.CompareExchangePointerWord(((uintptr)old).Value, ((uintptr)@new).Value);

        return Interlocked.CompareExchange(ref addr.Value, @new, old) == old;
    }

    public static partial int32 /*new*/ AddInt32(ж<int32> addr, int32 delta)
    {
        return Interlocked.Add(ref addr.Value, delta);
    }

    public static partial uint32 /*new*/ AddUint32(ж<uint32> addr, uint32 delta)
    {
        return Interlocked.Add(ref addr.Value, delta);
    }

    public static partial int64 /*new*/ AddInt64(ж<int64> addr, int64 delta)
    {
        return Interlocked.Add(ref addr.Value, delta);
    }

    public static partial uint64 /*new*/ AddUint64(ж<uint64> addr, uint64 delta)
    {
        return Interlocked.Add(ref addr.Value, delta);
    }

    public static partial uintptr /*new*/ AddUintptr(ж<uintptr> addr, uintptr delta)
    {
        nuint initialValue, newValue;

        do
        {
            initialValue = Volatile.Read(ref addr.Value.Value);
            newValue = initialValue + delta;
        }
        while (Interlocked.CompareExchange(ref addr.Value.Value, newValue, initialValue) != initialValue);

        return newValue;
    }

    public static partial int32 /*old*/ AndInt32(ж<int32> addr, int32 mask)
    {
        return Interlocked.And(ref addr.Value, mask);
    }

    public static partial uint32 /*old*/ AndUint32(ж<uint32> addr, uint32 mask)
    {
        return Interlocked.And(ref addr.Value, mask);
    }

    public static partial int64 /*old*/ AndInt64(ж<int64> addr, int64 mask)
    {
        return Interlocked.And(ref addr.Value, mask);
    }

    public static partial uint64 /*old*/ AndUint64(ж<uint64> addr, uint64 mask)
    {
        return Interlocked.And(ref addr.Value, mask);
    }

    public static partial uintptr /*old*/ AndUintptr(ж<uintptr> addr, uintptr mask)
    {
        // Returns the OLD value — Go's contract (`func AndUintptr(addr *uintptr, mask uintptr)
        // (old uintptr)`), and what Interlocked.And returns for the fixed-width overloads above.
        // Returned newValue until 2026-08-11 (L11), a silent wrong answer.
        nuint initialValue, newValue;

        do
        {
            initialValue = Volatile.Read(ref addr.Value.Value);
            newValue = initialValue & mask;
        }
        while (Interlocked.CompareExchange(ref addr.Value.Value, newValue, initialValue) != initialValue);

        return initialValue;
    }

    public static partial int32 /*old*/ OrInt32(ж<int32> addr, int32 mask)
    {
        return Interlocked.Or(ref addr.Value, mask);
    }

    public static partial uint32 /*old*/ OrUint32(ж<uint32> addr, uint32 mask)
    {
        return Interlocked.Or(ref addr.Value, mask);
    }

    public static partial int64 /*old*/ OrInt64(ж<int64> addr, int64 mask)
    {
        return Interlocked.Or(ref addr.Value, mask);
    }

    public static partial uint64 /*old*/ OrUint64(ж<uint64> addr, uint64 mask)
    {
        return Interlocked.Or(ref addr.Value, mask);
    }

    public static partial uintptr /*old*/ OrUintptr(ж<uintptr> addr, uintptr mask)
    {
        // Returns the OLD value — Go's contract; see AndUintptr above.
        nuint initialValue, newValue;

        do
        {
            initialValue = Volatile.Read(ref addr.Value.Value);
            newValue = initialValue | mask;
        }
        while (Interlocked.CompareExchange(ref addr.Value.Value, newValue, initialValue) != initialValue);

        return initialValue;
    }

    public static partial int32 /*val*/ LoadInt32(ж<int32> addr)
    {
        return Volatile.Read(ref addr.Value);
    }

    public static partial int64 /*val*/ LoadInt64(ж<int64> addr)
    {
        return Volatile.Read(ref addr.Value);
    }

    public static partial uint32 /*val*/ LoadUint32(ж<uint32> addr)
    {
        return Volatile.Read(ref addr.Value);
    }

    public static partial uint64 /*val*/ LoadUint64(ж<uint64> addr)
    {
        return Volatile.Read(ref addr.Value);
    }

    public static partial uintptr /*val*/ LoadUintptr(ж<uintptr> addr)
    {
        return Volatile.Read(ref addr.Value.Value);
    }

    public static partial @unsafe.Pointer /*val*/ LoadPointer(ж<@unsafe.Pointer> addr)
    {
        // Native arm: see the banner above SwapPointer.
        if (addr.IsNative)
            return new @unsafe.Pointer((uintptr)addr.ReadPointerWord());

        return Volatile.Read(ref addr.Value);
    }

    public static partial void StoreInt32(ж<int32> addr, int32 val)
    {
        Interlocked.Exchange(ref addr.Value, val);
    }

    public static partial void StoreInt64(ж<int64> addr, int64 val)
    {
        Interlocked.Exchange(ref addr.Value, val);
    }

    public static partial void StoreUint32(ж<uint32> addr, uint32 val)
    {
        Interlocked.Exchange(ref addr.Value, val);
    }

    public static partial void StoreUint64(ж<uint64> addr, uint64 val)
    {
        Interlocked.Exchange(ref addr.Value, val);
    }

    public static partial void StoreUintptr(ж<uintptr> addr, uintptr val)
    {
        Interlocked.Exchange(ref addr.Value.Value, val.Value);
    }

    public static partial void StorePointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer val)
    {
        // Native arm: see the banner above SwapPointer.
        if (addr.IsNative)
        {
            addr.ExchangePointerWord(((uintptr)val).Value);
            return;
        }

        Interlocked.Exchange(ref addr.Value, val);
    }

    // Managed-referent atomic pointer ops. Go's `atomic.LoadPointer((*unsafe.Pointer)(
    // unsafe.Pointer(&p.field)))` / `StorePointer(…, unsafe.Pointer(v))` targets a *T field that,
    // in the managed world, holds a `ж<T>` REFERENCE — which cannot round-trip through a `uintptr`
    // address (the `fixed` address is transient and reinterpreting it loses GC identity, so the
    // literal conversion NREs, e.g. x/sys/windows's LazyDLL/LazyProc caches). The converter
    // recognizes that idiom and emits these overloads on the FIELD BOX (`ж<ж<T>>`) directly, so the
    // read/write is an atomic operation on the managed reference itself — Volatile gives the
    // acquire/release ordering Go's LoadPointer/StorePointer require. Additive overloads: the
    // `ж<ж<T>>` argument never matches the existing `ж<@unsafe.Pointer>` (= `ж<Pointer>`) signature,
    // so ordinary `unsafe.Pointer` atomics are unaffected.
    public static ж<T> LoadPointer<T>(ж<ж<T>> addr)
    {
        return Volatile.Read(ref addr.Value);
    }

    public static void StorePointer<T>(ж<ж<T>> addr, ж<T> val)
    {
        Volatile.Write(ref addr.Value, val);
    }
} // end atomic_package
