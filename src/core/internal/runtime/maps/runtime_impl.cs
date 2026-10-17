// runtime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written body for internal/runtime/maps' runtime fatal bridge. runtime.cs:12 carries Go's own
// header for the block, "Functions below pushed from runtime.", and :15 the bodyless partial; the
// push does not arrive in this corpus, so PartialStubGenerator fills it with a throwing
// NotImplementedException.
//
// REACH, measured at this tree: 41 call sites across six files, and every one of them is a
// corruption report — 25 `concurrent map writes`, 10 `concurrent map read and map write`, 5 `small
// map with no empty slot`, 1 `concurrent map iteration and map write`. Go's answer to each is an
// unrecoverable fatal precisely because the map's invariants are already broken and continuing is
// worse than stopping. A throwing stub turns every one of those into an ordinary exception that any
// frame above can catch and carry on from, over a map that is no longer consistent.

// WHY A COMPANION AND NOT A //go:linkname REGISTRY ROW. The converter CAN forward a push into the
// consumer package, and five of RED 7's members are being wired that way (crypto/internal/fips140's
// three, crypto/internal/sysrand's fatal and crypto/internal/fips140hash's sha3Unwrap — G's seat).
// A row is admissible only where the consumer's project ALREADY references the pushing package.
// runtime.csproj REFERENCES internal/runtime/maps, so a row in the other direction would close a
// DIRECT cycle. This member takes the companion, not the row.
// COORD's split at 1fa7940a0, on G's reading of the project files at 5bb307d57e.
//
// WHY golib HOLDS THE PRIMITIVE, and not `runtime`. golib is the only assembly BELOW every converted
// package. FatalReport.cs argues this for itself and names this very set of consumers — it was
// written for the fatal shims already known, and these are the members the 1.24 hop added to the
// same class. One primitive there, a one-line forward here.
//
// WHAT THE FORWARD PRESERVES. Go's fatal is UNRECOVERABLE from its first instruction: no recover(),
// no deferred function, no catch. FatalReport.Fatal writes `fatal error: <text>`, a blank line and a
// Go-spelled traceback, then calls Environment.Exit(2), so the unrecoverability is structural rather
// than a property of the exception type — which is exactly what a throwing stub does NOT give: a
// NotImplementedException is an ordinary managed exception any frame above can catch.
//
// userFault is Go's throwType axis, read from Go's OWN push at the pin rather than inferred:
// runtime/panic.go:1056 pushes maps_fatal, whose body is `fatal(s)` — throwTypeUser, userFault TRUE.

// Aliased rather than imported wholesale: this file needs exactly one golib type, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using FatalReport = go.golib.FatalReport;

// rand, the second member of the same push block (runtime.cs:18; pushed by runtime/rand.go:182,
// `maps_rand` -> `return rand()`), takes the companion for the same cycle reason. Go's runtime.rand is
// the per-m ChaCha8 generator, OS-seeded at startup and non-deterministic run to run; the map consumes
// it for per-map hash seeds and the randomized iteration start (map.go, table.go), both of which Go
// DEFINES as unpredictable. Random.Shared is that contract -- OS-entropy seeded, thread-safe, fast --
// and it is the body math/rand, math/rand/v2, net and os already give their own runtime_rand pulls
// (math/rand/v2/rand_impl.cs). Before this every NewMap reached the throwing stub: 14 of
// internal/runtime/maps' tests stopped at NotImplementedException "rand".
using Random = System.Random;
using BinaryPrimitives = System.Buffers.Binary.BinaryPrimitives;

// The push block's parameter types, spelled as the emitted runtime.cs spells them (:6-7).
using abi = go.@internal.abi_package;
using @unsafe = go.unsafe_package;

// Hand-owned (no *_impl.go exists, so a reconvert never regenerates it); marked so the marker-based
// readers see it as well as the suffix-based ones.
[module: go.GoManualConversion]

namespace go.@internal.runtime;

partial class maps_package
{
    internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);

    internal static partial uint64 rand()
    {
        System.Span<byte> bytes = stackalloc byte[8];
        Random.Shared.NextBytes(bytes);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    // The REST of the push block -- runtime/malloc.go:1729/:1798 (maps_newobject, maps_newarray) and
    // runtime/mbarrier.go:234/:384 (maps_typedmemmove, maps_typedmemclr) -- allocate, copy and clear
    // RAW Go-layout memory: the swiss table's groups, addressed by the descriptor's GroupSize, SlotSize
    // and ElemOff offsets. The managed model has no such memory (golib's map<K,V> is a dictionary, and
    // abi's MapType projection reports those offsets as an honest ZERO), so there is no body that would
    // not fabricate one. Each REFUSES BY NAME -- the FuncPCABI0 precedent (internal/abi/funcpc_impl.cs):
    // a Go panic the test host reports as a failure carrying this text, never a NotImplementedException
    // that reads as a missing implementation. The text is written to serve as the row's disclosure
    // signature (runtime-capability, COORD ruling 2026-09-22).
    private static System.Exception refuseRawGroupMemory(string name) =>
        panic($"runtime-capability: {name} over raw Go-layout group memory has no managed answer (golib map is a dictionary)");

    internal static partial @unsafe.Pointer newarray(ж<abi.Type> typ, nint n) => throw refuseRawGroupMemory("newarray");

    internal static partial @unsafe.Pointer newobject(ж<abi.Type> typ) => throw refuseRawGroupMemory("newobject");

    internal static partial void typedmemmove(ж<abi.Type> typ, @unsafe.Pointer dst, @unsafe.Pointer src) => throw refuseRawGroupMemory("typedmemmove");

    internal static partial void typedmemclr(ж<abi.Type> typ, @unsafe.Pointer ptr) => throw refuseRawGroupMemory("typedmemclr");

    // typeString (Go 1.26, map.go:897-898; pushed by runtime/type.go:16, `maps_typeString` ->
    // `toRType(typ).string()`) names a key type for unhashableTypeError's message, "hash of unhashable
    // type: <T>". The same cycle rule as the rest of the push block puts it here, not in a linkname
    // row. Runtime's own body reads the linker's name blob through nameOff, which a synthesized
    // descriptor does not have; what it DOES carry is its managed type, and GoReflect.GoTypeName is the
    // Go spelling golib already writes for the same panic (GoEqualityComparer's "hash of unhashable
    // type"). Unfilled, the generator stubs this with a throwing body, so the first unhashable-key
    // panic would have reported NotImplementedException instead of Go's message. A descriptor with no
    // managed type names no Go type at all, and cannot be a map key's; it answers so, plainly.
    internal static partial @string typeString(ж<abi.Type> typ) =>
        typ != nil && typ.Value.sysType is { } st ? GoReflect.GoTypeName(st) : "<type with no managed descriptor>";

    // The panic value of mapKeyError (map.go:889-893): golib refuses an unhashable interface key on an
    // empty or nil map with this, because golib's map<K,V> is a dictionary and never reaches the
    // converted mapKeyError. golib sits below this package and cannot name unhashableTypeError, so the
    // runtime package registers this as RuntimeErrorPanic.UnhashableKeyErrorValue (panicvalues_impl.cs).
    // The descriptor is the managed-backed one typeString above reads, so Error() prints Go's spelling.
    public static object unhashableKeyPanicValue(System.Type unhashable) =>
        (error)new unhashableTypeError(abi.synthType(unhashable));
}
