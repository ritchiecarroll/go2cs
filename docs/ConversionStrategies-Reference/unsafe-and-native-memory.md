# Unsafe and Native Memory

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#unsafepointer-and-uintptr)

This page covers `unsafe.Pointer` identity: how an `unsafe.Pointer` compares, and how the nil pointer survives a round trip through `uintptr`.

## Pointer comparison

### An `unsafe.Pointer` is compared BY ADDRESS, and a container's pointer names its STORAGE

Three separate identity rules meet in Go's own cycle detectors, and all three were wrong in the same
direction — too *fine*, so nothing ever compared equal to itself:

1. **`unsafe.Pointer`.** It is a `ж<uintptr>` whose VALUE is the address, and `ж<T>` compares and
   hashes a heap box by REFERENCE — right for every other pointer (the box IS the storage it names)
   and wrong for this one, which CARRIES an address while the converter mints a fresh box on every
   `uintptr → unsafe.Pointer` conversion. `ж<T>.Equals` is `virtual` for this single override, which
   makes `==`, `Equals` and a map-key lookup answer through one rule; an
   `operator ==(Pointer, Pointer)` would instead have made every existing
   `uintptr == unsafe.Pointer` comparison ambiguous (CS0034, measured in `runtime`'s `map.cs` and
   `mfinal.cs`).
2. **A MAP or SLICE reached through `reflect.Value.UnsafePointer`.** Go answers the STORAGE address —
   the hmap for a map, `&s[0]` for a slice — while the managed value is a HEADER STRUCT, freshly boxed
   on every read out of a slot, so two reads of one Go map tokened differently. The token now comes
   from the backing store (plus the window offset for a slice), which is the same root
   `deepValueEqual` keys its own cycle detection on, so the two walks cannot disagree about what "the
   same map" means.
3. **A struct field of INTERFACE type.** `go2cs-gen`'s memberwise `Equals` compared every member with
   C# `==`, which for an interface or `object` operand is reference identity where Go compares
   interface values by dynamic type and value. Since a struct's `Equals` is also what a map LOOKUP
   calls, such a struct could never be found under a key it had itself been stored under. Those
   members now route through `builtin.AreEqual`, the same relation the converter emits for a bare Go
   `==` between interface operands; `ж<T>`, named-pointer wrappers and delegates keep `==`, because
   pointer identity IS Go's pointer relation and a struct holding a func is not comparable in Go at
   all.

`encoding/json`'s encoder needs all three: it keys `e.ptrSeen` on `v.Interface()` for a pointer, on
`v.UnsafePointer()` for a map, and on `struct{ ptr any; len int }` for a slice. With any of them
answering by identity the lookup never matched an entry it had itself stored, no cycle was detected,
and `Marshal` of a self-referential value recursed until the process died — `0xc00000fd`, which is
uncatchable and takes every verdict the run had not yet produced with it. Go returns
`UnsupportedValueError: encountered a cycle`.

### `unsafe.Pointer(uintptr(0))` must compare equal to `nil`

**`unsafe.Pointer(uintptr(0))` must compare equal to `nil`.** The converter bridges every
`unsafe.Pointer`-valued call through `uintptr`, because `unsafe` lives in its own assembly and can
carry no implicit conversion on the core pointer class. golib's round-trip therefore has to preserve
nil in *both* directions, and it did not: `Pointer(uintptr)` produced a non-nil box wrapping 0, and
`(uintptr)ptr` dereferenced a nil-constructed box instead of yielding 0. The symptom was silent and
far away — sync's `poolDequeue` reads each ring slot's `typ` word to decide whether it is free, every
empty slot came back "occupied", `pushHead` returned false forever and `TestPoolDequeue`/
`TestPoolChain` spun. `unsafe.cs` now marks the zero address nil (a protected `ж<T>(value, isNull)`
constructor holds the address *and* the nil flag) and tolerates a nil box on the way out.

---

[Index](README.md)
