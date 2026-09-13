// atomic_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of internal/runtime/atomic's assembly primitives.
//
// Every declaration below is a Go function whose body is a `.s` file — go2cs emits those as
// bodyless `partial` methods and, with no companion, the PartialStubGenerator fills them with a
// throwing stub. That is the correct default for raw-metal assembly, but this package is the
// NATIVE-TYPE half of the S1 fork: these are plain memory atomics over native scalars, and the
// managed runtime has an exact, faithful equivalent in System.Threading.Interlocked / Volatile.
// So they get a real conversion, not a stub — the same ruling that made core/sync/atomic's
// scalar types converter output.
//
// Consequences of leaving them stubbed (what this file fixes): the whole package is the atomic
// substrate the converted runtime is built on, so a single throwing primitive takes down any
// path that touches it — runtime.SetMutexProfileFraction (sync's TestMutex) died on Store64,
// which is otherwise a perfectly faithful conversion.
//
// Semantics are Go's, verified against internal/runtime/atomic's own docs:
//   - Xadd*  return the NEW value      (Interlocked.Add returns the new value)
//   - Xchg*  return the OLD value      (Interlocked.Exchange returns the original)
//   - And32/Or32/And64/Or64/And-Oruintptr return the OLD value (Interlocked.And/Or do too)
//   - And/Or/And8/Or8 return nothing
//   - Cas*   report whether the swap ran
//   - Store* are release stores; StoreRel*/StoreReluintptr are explicitly release-ordered, and
//     .NET's Volatile.Write IS a release store, so both map to the same call.
//
// TWO widths have no managed intrinsic and are served by a lock instead:
//   - 8-bit (And8/Or8): the CLR has no byte-width Interlocked. A read-modify-write under a
//     shared latch is atomic with respect to every other 8-bit atomic here, which is the only
//     guarantee Go's callers rely on (bitmap bytes are never touched non-atomically).
//   - the unsafe.Pointer family (Casp1/StorepNoWB/Loadp/storePointer/casPointer): golib models
//     unsafe.Pointer as a CLASS wrapping a uintptr, so the "pointer word" is the wrapped NUMBER,
//     not the object reference — a CAS must compare the numbers (two distinct Pointer objects
//     addressing the same location are the same pointer to Go). Interlocked cannot express
//     "compare the wrapped field, swap the reference", so these take the same latch. The BARE
//     unsafe.Pointer members (StorepNoWB, Loadp) additionally reach their location through the
//     mint's RETAINED referent — see the family header below (the I5 ruling).
// The latch is a plain object monitor, uncontended in practice; correctness, not throughput, is
// the requirement here.
//
// Hand-owned: there is no atomic_impl.go, so a -stdlib reconvert never regenerates this file;
// the marker is carried for consistency with the other hand-owned companions and so the overlay
// treats it like one.

using System.Threading;

[module: go.GoManualConversion]

namespace go.@internal.runtime;

using @unsafe = go.unsafe_package;

partial class atomic_package
{
    // Serializes the widths the CLR has no intrinsic for (see the header).
    private static readonly object s_wideLatch = new();

    // ---- 32-bit unsigned --------------------------------------------------------------------

    public static partial uint32 Xadd(ж<uint32> ptr, int32 delta) =>
        Interlocked.Add(ref ptr.Value, unchecked((uint32)delta));

    public static partial uint32 Xchg(ж<uint32> ptr, uint32 @new) =>
        Interlocked.Exchange(ref ptr.Value, @new);

    public static partial bool Cas(ж<uint32> ptr, uint32 old, uint32 @new) =>
        Interlocked.CompareExchange(ref ptr.Value, @new, old) == old;

    public static partial bool CasRel(ж<uint32> ptr, uint32 old, uint32 @new) =>
        Interlocked.CompareExchange(ref ptr.Value, @new, old) == old;

    public static partial void Store(ж<uint32> ptr, uint32 val) =>
        Volatile.Write(ref ptr.Value, val);

    public static partial void StoreRel(ж<uint32> ptr, uint32 val) =>
        Volatile.Write(ref ptr.Value, val);

    public static partial void And(ж<uint32> ptr, uint32 val) =>
        Interlocked.And(ref ptr.Value, val);

    public static partial void Or(ж<uint32> ptr, uint32 val) =>
        Interlocked.Or(ref ptr.Value, val);

    public static partial uint32 And32(ж<uint32> ptr, uint32 val) =>
        Interlocked.And(ref ptr.Value, val);

    public static partial uint32 Or32(ж<uint32> ptr, uint32 val) =>
        Interlocked.Or(ref ptr.Value, val);

    // ---- 64-bit unsigned --------------------------------------------------------------------

    public static partial uint64 Xadd64(ж<uint64> ptr, int64 delta) =>
        Interlocked.Add(ref ptr.Value, unchecked((uint64)delta));

    public static partial uint64 Xchg64(ж<uint64> ptr, uint64 @new) =>
        Interlocked.Exchange(ref ptr.Value, @new);

    public static partial bool Cas64(ж<uint64> ptr, uint64 old, uint64 @new) =>
        Interlocked.CompareExchange(ref ptr.Value, @new, old) == old;

    public static partial void Store64(ж<uint64> ptr, uint64 val) =>
        Volatile.Write(ref ptr.Value, val);

    public static partial void StoreRel64(ж<uint64> ptr, uint64 val) =>
        Volatile.Write(ref ptr.Value, val);

    public static partial uint64 And64(ж<uint64> ptr, uint64 val) =>
        Interlocked.And(ref ptr.Value, val);

    public static partial uint64 Or64(ж<uint64> ptr, uint64 val) =>
        Interlocked.Or(ref ptr.Value, val);

    // ---- signed 32/64 -----------------------------------------------------------------------

    public static partial int32 Loadint32(ж<int32> ptr) =>
        Volatile.Read(ref ptr.Value);

    public static partial int64 Loadint64(ж<int64> ptr) =>
        Interlocked.Read(ref ptr.Value);

    public static partial void Storeint32(ж<int32> ptr, int32 @new) =>
        Volatile.Write(ref ptr.Value, @new);

    public static partial void Storeint64(ж<int64> ptr, int64 @new) =>
        Interlocked.Exchange(ref ptr.Value, @new);

    public static partial int32 Xaddint32(ж<int32> ptr, int32 delta) =>
        Interlocked.Add(ref ptr.Value, delta);

    public static partial int64 Xaddint64(ж<int64> ptr, int64 delta) =>
        Interlocked.Add(ref ptr.Value, delta);

    public static partial int32 Xchgint32(ж<int32> ptr, int32 @new) =>
        Interlocked.Exchange(ref ptr.Value, @new);

    public static partial int64 Xchgint64(ж<int64> ptr, int64 @new) =>
        Interlocked.Exchange(ref ptr.Value, @new);

    public static partial bool Casint32(ж<int32> ptr, int32 old, int32 @new) =>
        Interlocked.CompareExchange(ref ptr.Value, @new, old) == old;

    public static partial bool Casint64(ж<int64> ptr, int64 old, int64 @new) =>
        Interlocked.CompareExchange(ref ptr.Value, @new, old) == old;

    // ---- uintptr / uint ---------------------------------------------------------------------
    //
    // golib's `uintptr` is a STRUCT wrapping a public `nuint Value` field precisely so the atomic
    // intrinsics have a native slot to target (see golib/uintptr.cs) — hence `ref ptr.Value.Value`.

    public static partial uintptr Loaduintptr(ж<uintptr> ptr) =>
        Volatile.Read(ref ptr.Value.Value);

    public static partial nuint Loaduint(ж<nuint> ptr) =>
        Volatile.Read(ref ptr.Value);

    public static partial void Storeuintptr(ж<uintptr> ptr, uintptr @new) =>
        Volatile.Write(ref ptr.Value.Value, @new.Value);

    public static partial void StoreReluintptr(ж<uintptr> ptr, uintptr val) =>
        Volatile.Write(ref ptr.Value.Value, val.Value);

    public static partial uintptr Xadduintptr(ж<uintptr> ptr, uintptr delta) =>
        casLoop(ref ptr.Value.Value, delta.Value, static (old, d) => unchecked(old + d), returnOld: false);

    public static partial uintptr Xchguintptr(ж<uintptr> ptr, uintptr @new) =>
        Interlocked.Exchange(ref ptr.Value.Value, @new.Value);

    public static partial bool Casuintptr(ж<uintptr> ptr, uintptr old, uintptr @new) =>
        Interlocked.CompareExchange(ref ptr.Value.Value, @new.Value, old.Value) == old.Value;

    public static partial uintptr Anduintptr(ж<uintptr> ptr, uintptr val) =>
        casLoop(ref ptr.Value.Value, val.Value, static (old, v) => old & v, returnOld: true);

    public static partial uintptr Oruintptr(ж<uintptr> ptr, uintptr val) =>
        casLoop(ref ptr.Value.Value, val.Value, static (old, v) => old | v, returnOld: true);

    // Interlocked exposes Exchange/CompareExchange for nuint but NOT Add/And/Or, so the
    // pointer-width read-modify-writes ride the CAS the intrinsic does provide — the same shape
    // Go itself uses for the platforms without the instruction (atomic_andor_generic.go).
    private static nuint casLoop(ref nuint slot, nuint operand, Func<nuint, nuint, nuint> combine, bool returnOld)
    {
        while (true)
        {
            nuint old = Volatile.Read(ref slot);
            nuint updated = combine(old, operand);

            if (Interlocked.CompareExchange(ref slot, updated, old) == old)
                return returnOld ? old : updated;
        }
    }

    // ---- 8-bit (latched — no byte-width intrinsic) ------------------------------------------

    public static partial void Store8(ж<uint8> ptr, uint8 val) =>
        Volatile.Write(ref ptr.Value, val);

    public static partial void And8(ж<uint8> ptr, uint8 val)
    {
        lock (s_wideLatch)
            ptr.Value &= val;
    }

    public static partial void Or8(ж<uint8> ptr, uint8 val)
    {
        lock (s_wideLatch)
            ptr.Value |= val;
    }

    // ---- unsafe.Pointer family (latched — compares the WRAPPED NUMBER) ----------------------
    //
    // The four Pointer primitives split exactly on signature (the I5 ruling, 2026-08-26). The
    // `*unsafe.Pointer` members below carry an aliasing ж<unsafe.Pointer> and write it directly.
    // The BARE `unsafe.Pointer` members (StorepNoWB, Loadp) receive a pointer whose alias the mint
    // flattened to a number — writing ptr's own uintptr slot was a silent LOST WRITE into a fresh
    // box (TestStorepNoWB's 14/15). Both now go through the referent the mint RETAINS
    // (@unsafe.Pointer.FromBox carries the source box; StoreThrough/LoadThrough recover it and
    // reach the very slot the pointer names), and a pointer with no recoverable referent panics
    // by name instead of losing the write.

    public static partial void StorepNoWB(@unsafe.Pointer ptr, @unsafe.Pointer val)
    {
        // Go's StorepNoWB is *(*unsafe.Pointer)(ptr) = val — the store lands in the location ptr
        // NAMES, reached through the mint's retained referent. Latched for coherence with the
        // sibling CAS family against the same location.
        lock (s_wideLatch)
            ptr.StoreThrough(val);
    }

    // Loadp is *(*unsafe.Pointer)(ptr) — Go's body is real (atomic_amd64.go), but its converted
    // form round-trips through the numeric address ((ж<Pointer>)(uintptr)(ptr)), which for a
    // managed referent is a transient number that keeps accidentally aliasing the slot only until
    // the collector moves it. Registered in manualConversionFuncs so the emission is a placeholder
    // and the load goes through the same retained-referent recovery as the store above.
    public static @unsafe.Pointer Loadp(@unsafe.Pointer ptr)
    {
        lock (s_wideLatch)
            return ptr.LoadThrough();
    }

    public static partial bool Casp1(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new) =>
        casPointerLatched(ptr, old, @new);

    internal static partial void storePointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer @new)
    {
        lock (s_wideLatch)
            ptr.Value = @new;
    }

    internal static partial bool casPointer(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new) =>
        casPointerLatched(ptr, old, @new);

    private static bool casPointerLatched(ж<@unsafe.Pointer> ptr, @unsafe.Pointer old, @unsafe.Pointer @new)
    {
        lock (s_wideLatch)
        {
            @unsafe.Pointer current = ptr.Value;

            // Two distinct Pointer objects addressing the same location ARE the same pointer to
            // Go, so the comparison is on the wrapped number (nil ⇔ null ⇔ 0).
            uintptr currentValue = current is null ? default : current.Value;
            uintptr oldValue = old is null ? default : old.Value;

            if (currentValue != oldValue)
                return false;

            ptr.Value = @new;
            return true;
        }
    }
}
