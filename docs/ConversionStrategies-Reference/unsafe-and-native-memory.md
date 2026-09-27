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

## Size, alignment and offset

### `unsafe.Alignof` / `unsafe.Offsetof` name a TYPE, resolved through `go/types`
Go defines `Sizeof`, `Alignof` and `Offsetof` against the **static type** of their operand — and never
evaluates that operand (all three are compile-time constants for any type of non-variable size). golib
matches that shape: `Alignof(Type, string? fieldName = null)` and `Offsetof(Type structType, string
fieldName)` take a `System.Type`, so the converter has to turn the Go operand into a type argument.

It now does that from `go/types`: **`unsafe.Alignof(x)` emits `@unsafe.Alignof(typeof(T))`** for `T` the
C# rendering of the operand's static type — one rule for *every* operand shape, because Go's
`Alignof(s.f)` is the required alignment of the **field's own type**, which is exactly what golib's
two-argument overload resolves to anyway. **`unsafe.Offsetof(s.f)` emits `@unsafe.Offsetof(typeof(S),
"f")`**, where `S` comes from `types.Selection.Recv()` with any implicit pointer dereference stripped,
and a *promoted* field is walked down its embedding chain so the offset is measured against the struct
that declares it (Go's rule: relative to the immediately enclosing struct). The field name is the
emitted identifier with any keyword escape removed, since reflection sees `@out` as `out`.

The shape was previously derived by splitting the **converted C# text** on `.` and reading the pieces as
if they were a Go field selector — one part meant `x`, two meant `s.f`, anything else warned and fell
through to an emission that cannot compile. That mistakes any dotted *rendering* for a selector and
corrupts every operand that is not literally an identifier or a one-level selection: a conversion
operand renders with a leading cast, so `unsafe.Alignof(uint32(0))` became `(uint32)0.GetType()`, which
C# parses as `(uint32)(0.GetType())` — `CS0030: Cannot convert type 'System.Type' to 'uint'`, and the
sole build blocker on `crypto/md5` (its `benchmarkSize` alignment probe); a `ж` dereference `Ꮡx.Value`
read as struct `Ꮡx` with field `Value`; and a two-level `cpu.X86.HasAVX` was rejected outright.
`.GetType()` was also the wrong instrument on its own terms — it reports the **dynamic** type of a boxed
or interface-typed operand where Go uses the static one, and it evaluates the operand, which Go does
not. (`unsafe.Sizeof` was unaffected: it emitted the generic `@unsafe.Sizeof(x)`, whose type argument C#
infers.) Guarded by the `UnsafeOperations` behavioral
test, extended with a conversion operand, an index operand, a selector through a pointer, a two-level
selector, and a field whose name is a C# keyword; all output-compared vs `go run`.

**This shape is now the FALLBACK, not the normal path** — expression sites fold to the constant (next
section), and only a variable-size operand still reaches the `typeof(T)` emission. The promoted-field
rule stated above is also *wrong about Go*, and the fold supersedes it: Go measures a promoted field
against the **operand** struct, not against the struct that declares it.

### `unsafe.Sizeof` / `Alignof` / `Offsetof` FOLD to a constant at expression sites
Go computes all three at compile time from the operand's **static type**, never evaluates the operand,
and yields a **typed `uintptr` constant** for any operand type of non-variable size. *Declaration*
sites have always emitted that constant — `internal static uintptr offsetX86HasAVX => /*
unsafe.Offsetof(cpu.X86.HasAVX) */ 66;` in `runtime/cpuflags.cs`. **Expression sites now emit the same
form**, so one Go construct has one behavior:

```go
var hdr Header32
data := make([]byte, unsafe.Sizeof(hdr))
// …
f.Type = Type(bo.Uint16(data[unsafe.Offsetof(hdr.Type):]))
```
```csharp
var data = new slice<byte>((nint)(/* unsafe.Sizeof(hdr) */ (uintptr)52));
// …
f.Value.Type = ((Type)bo.Uint16(data[(int)(/* unsafe.Offsetof(hdr.Type) */ (uintptr)16)..]));
```

The value comes from `go/types`, which folds against the `types.Sizes` for the **loaded target
`GOARCH`** — the Go compiler's own layout rules — so the emitted number is what the *Go* program
computes, not a measurement of the emitted C#. The literal keeps its `uintptr` type because Go's
constant is typed: a bare number would let `uadd := unsafe.Sizeof(*t)` infer C# `int`, and an `int`
*variable* has no implicit conversion back to `nuint` (`internal/abi`'s `FuncType.InSlice` hands it to
a `uintptr` parameter — CS1503). The cast is inert everywhere else: C#'s constant-expression conversion
would have bound a bare literal anyway, and a cast binds tighter than every binary operator, so no site
needs extra parentheses.

Three things this fixes, beyond removing a reflection call from a construct Go settles at compile time:

- **A latent throw.** golib's `Sizeof` rides `Marshal.SizeOf<T>`, which **throws** for a non-blittable
  `T` — and a converted Go struct is non-blittable as soon as it holds a `slice<T>`, `@string`,
  interface, or `ж<T>` field. `debug/elf`'s `Header32` holds an `array<byte>` (a `byte[]` inside), so
  its `unsafe.Sizeof(hdr)` answered **56** where Go says 52 (a 4-byte-too-long header read), and the
  reader's own `Section`/`Prog` — embedded struct + `io.ReaderAt` + `ж<SectionReader>` —
  **throw** `ArgumentException: … cannot be marshaled as an unmanaged structure`. `runtime.mapiterinit`'s
  `unsafe.Sizeof(hiter{})/goarch.PtrSize != 12` guard sits in front of every map range, and `hiter`
  holds four `ж<…>` fields.
- **Semantic fidelity.** A reflection answer measures the *CLR-marshalled* layout, which differs from
  Go's whenever golib's field representation differs — `string` is 16 bytes in Go/amd64, `@string` is a
  managed struct. `debug/elf` uses these values as **on-disk format offsets**; they must be Go's.
- **Promoted fields.** Go measures `unsafe.Offsetof(e.count)` against the **operand** struct: with
  `count` at 8 inside an embedded `Padded` that itself sits at 8, the answer is **16**. The reflection
  form could see only one hop of the embedding chain; `go/types` folds the whole path.

A **variable-size** operand still emits the run-time form (with a converter warning naming the site),
because Go itself does not fold it: since Go 1.18 the operand may be **type-parameter-typed**, and the
call is then not a constant. Four such sites exist in the stdlib — `slices.Compact`
(`unsafe.Sizeof(a[0])` on `S ~[]E`), `internal/saferio` (`unsafe.Sizeof(v)` on `E`), and
`runtime/minmax` (×2) — and they are why golib's `@unsafe` run-time forms are retained rather than
deleted.

Measured over the full stdlib (seeded A/B reconvert, Go 1.23.1, `windows/amd64`): **262 expression
sites folded across 61 files in 16 packages** — `runtime` 129, `debug/elf` 70, `syscall` 17,
`internal/poll` 13, then a long tail; declaration sites byte-identical. Design record:
[`docs/phase4/DESIGN-unsafe-constant-folding.md`](../phase4/DESIGN-unsafe-constant-folding.md). Guarded by
`UnsafeOperations`, extended with all three builtins in call-argument, arithmetic, comparison,
assignment and compound-assignment, and `make`-size positions, over structs whose Go layout is
padding-, embedding- and array-sensitive (and non-blittable once converted); output-compared vs
`go run`.

### The run-time `unsafe.Sizeof` answers through Go's layout rule, not the CLR's marshaller

The folding arc above removed the reflection call from every site Go itself settles at compile time,
and named what was left: the handful of operands whose type is a **type parameter**, which Go's own
spec calls variable-size and does not fold either. Those kept riding `Marshal.SizeOf<T>` — and there
the latent throw the folding arc had just designed around was not latent at all, because a type
parameter binds at run time to exactly the shapes `Marshal.SizeOf` refuses: a generic type
(`ж<Section>`, `slice<T>`) raises *"The specified Type must not be a generic type"*, and a struct
holding a managed reference raises *"cannot be marshaled as an unmanaged structure"*.

Three packages died on it at once, all through the same one line — `internal/saferio.SliceCap[E]`,
which asks `unsafe.Sizeof(*new(E))` only to decide how large a chunk it may pre-allocate:
`debug/macho` (E = the `Load` **interface**), `internal/xcoff` (E = `ж<Section>`), and
`go/internal/gccgoimporter` through `debug/elf` (E = `ΔSection`, a struct over an embedded header, an
`io.ReaderAt` and a `ж<SectionReader>`).

So the run-time form now answers through **`GoReflect.GoSizeOf`** — the same Go-layout walk the
reflection bridge already stamps into a descriptor's `Size_`, and the same one `reflect.Type.Size()`
reads:

```csharp
public static uintptr Sizeof<T>(T x) {
    nint size = GoReflect.GoSizeOf(typeof(T), GoReflect.ArrayDimsOfValue(x));
    return size >= 0 ? (uintptr)size : (uintptr)Marshal.SizeOf<T>();
}
```

`typeof(T)` is Go's rule verbatim — `Sizeof` is defined against the operand's **static** type, and the
converter's inferred type argument is that type. Dims come from the live value because `array<T>`
carries its Go length in the instance, not the type. `Marshal.SizeOf` stays as the fallback for the
shapes `GoSizeOf` declines (`-1`: an array whose length nothing can reveal, a struct holding such a
field), so no operand that resolved before stops resolving.

This makes the answer *correct* as well as non-throwing, which matters beyond the three packages:
`Marshal.SizeOf` reports a **bool** as 4 bytes where Go says 1, so any struct containing one was
already measured wrong — silently, at the sites the folding arc could not reach. A Go size now has
one definition in the runtime rather than two (the unification `golib/GoReflect.TypeLayout.cs` had
recorded as deferred pending a named consumer). Corpus reach is small by construction: **7 run-time
call sites**, against 283 folded ones.

## Converting pointers to and from `unsafe.Pointer`

### Converting a Go pointer to `unsafe.Pointer`
`unsafe.Pointer` is the golib class `unsafe_package.Pointer : ж<uintptr>` (a numeric address wrapper). A `uintptr`/`unsafe.Pointer` argument converts through the implicit `uintptr ↔ Pointer` operators, but a **Go pointer** argument (`*T`, emitted as the managed box `ж<T>`) has no such conversion — a plain cast `(@unsafe.Pointer)(ж<T>)` is `CS0030` (when `T` is unrelated to `uintptr`) or a runtime `InvalidCastException` (the base→derived downcast `(@unsafe.Pointer)(ж<uintptr>)` compiles but the object is a plain `ж<uintptr>`, not a `Pointer`). So `unsafe.Pointer(ptr)` for a pointer `ptr` is emitted through the referent-**retaining** mint (2026-08-27, the I5 ruling — it was `@unsafe.Pointer.FromRef(ref (box).Value)` before that, which flattened the pointer to a transient number in a fresh box):
```go
func (u *UnsafePointer) Load() unsafe.Pointer { return Loadp(unsafe.Pointer(&u.value)) }
```
```csharp
public static @unsafe.Pointer Load(this ж<UnsafePointer> Ꮡu) {
    ref var u = ref Ꮡu.Value;
    return (uintptr)Loadp(@unsafe.Pointer.FromBox(Ꮡu.of(UnsafePointer.Ꮡvalue)));
}
```
> `FromBox` carries the same transient numeric address the `FromRef` form produced (still **not GC-stable** — the caveat of every `unsafe.Pointer`-as-`uintptr` use) **and the source box itself** (`Pointer.RetainedSource`, the `NativeBox` §4 retention pattern). The retained referent is what makes the bare-`unsafe.Pointer` atomic primitives real: `StorepNoWB`/`Loadp` were silently non-functional — `*(*unsafe.Pointer)(ptr) = val` wrote the argument box's own `uintptr` slot, never the memory it names (`internal/runtime/atomic`'s `TestStorepNoWB`, the 14/15) — and now recover the referent (`StoreThrough`/`LoadThrough`, dispatching through golib's internal `IUntypedSlotAccess` seam on `ж<T>`) and reach the very slot the pointer names. A pointer with no recoverable referent panics **by name** instead of losing the write. A nil box mints the 0 address rather than `FromRef`'s nil-deref panic — Go's `unsafe.Pointer(nil)`, matching the pointer-parameter emission's existing behavior. The `*unsafe.Pointer` siblings (`Casp1`/`storePointer`/`casPointer`) were always correct by signature — they carry an aliasing `ж<unsafe.Pointer>`. A pointer **receiver** keeps `FromRef` (a `this ref T` receiver has no box to retain) — the one non-retaining mint, a recorded residual, loud at the primitives if ever reached. (The reinterpret pattern `*(*U)(unsafe.Pointer(&x))` is handled separately and is not affected. Guarded by `GolibTests`' `UnsafePointerRetentionTests`, failing-first against the `FromRef` emission, plus the banked `internal/runtime/atomic` suite itself.)

**A NIL pointer converts to address 0, not a throw.** golib's `ж<T> → uintptr` (and `ж<T> → void*`) operator takes the pointed-to storage's address via a `fixed` block — but a **nil** box has no storage to pin, so `&value.Value` dereferences it and throws. Go's `uintptr(unsafe.Pointer(nil))` is simply **0**, and the syscall wrappers pass nil pointers exactly this way: `syscall.Write` hands `writeFile` a nil `*Overlapped` for a synchronous write, then passes `uintptr(unsafe.Pointer(overlapped))` (= 0) to the `SyscallN` trampoline. The operators now return `0`/`null` for a nil box before pinning — so any converted `os.Stdout.Write` (hence `fmt.Println`) whose stdout is a pipe reaches the OS `WriteFile` and prints, instead of crashing on the nil-`overlapped` argument. (Guarded by the `NilPointerUintptr` behavioral **output** test — `uintptr(unsafe.Pointer(nilPtr)) == 0` and a non-nil control, vs Go.)

**A PACKAGE-SCOPE `uintptr(unsafe.Pointer(...))` must not crash the converter.** The `unsafe.Pointer` conversion path has a special case that rewrites `unsafe.Pointer(arg)` into the ref-based extension call `(uintptr)@unsafe.Pointer.FromRef(ref arg)` when the *enclosing function is a pointer-receiver method* whose single argument aliases the receiver (pointer-receiver methods are emitted as ref-based extension functions, so the pointer must be reconstructed from a `ref`). That test read `v.currentFuncSignature.Recv()` **unconditionally** — but a **package-level** `var` initializer is converted with no enclosing function, so `currentFuncSignature` is `nil` and the receiver probe nil-panicked *during conversion* (`go/types.(*Signature).Recv`). The special case can never apply at package scope — there is no receiver — so the fix guards it with `v.currentFuncSignature != nil` (the same idiom `convUnaryExpr` and `captureModeOperations` already use), which falls through to the ordinary emission: `var gPtr uintptr = uintptr(unsafe.Pointer(&global))` → `(uintptr)Ꮡglobal` (it read `(uintptr)new @unsafe.Pointer(Ꮡglobal)` until the dead-wrapper peephole below), identical to the in-function non-receiver form the corpus already produces. This is what blocked `cmp`'s Phase-4 validation: `cmp_test.go` declares `var nonnilptr uintptr = uintptr(unsafe.Pointer(&negzero))` and `var nilptr uintptr = uintptr(unsafe.Pointer(nil))` at package scope, and the converter crashed before emitting a line.

**A null `Pointer` *reference* (from `unsafe.Pointer(nil)`) also converts to 0.** Distinct from the nil-*box* case above: the untyped `nil` literal in `unsafe.Pointer(nil)` renders as `(@unsafe.Pointer)default!`, and `default` of the reference type `Pointer` is a C# **null**, not a `ж<T>` box. golib's `Pointer → uintptr` operator then dereferenced `value.Value` and threw `NullReferenceException` — even though `Pointer`'s own `==`/`!=` operators already treat a null reference as nil (`value?.IsNull ?? true`). The `uintptr` and `void*` conversion operators now honor that same null-tolerance (`value is null ? 0/null : value.Value`), so `uintptr(unsafe.Pointer(nil))` yields 0 whether the nil arrives as a nil box or a null `Pointer` reference. (Both the package-scope crash and this null-reference conversion are guarded by the extended `NilPointerUintptr` behavioral **output** test — package-level `var gPtr = uintptr(unsafe.Pointer(&global))` (non-zero) and `var gNil = uintptr(unsafe.Pointer(nil))` (0), vs Go; the pre-fix converter panics on the package-scope declaration and, once past that, the pre-fix golib NREs on `gNil`.)

**A pointer to a Go fixed array resolves to the array's DATA, pinned across the FFI call.** The other half of that `fixed`-block operator is wrong for a `ж<array<T>>` (`unsafe.Pointer(&arr)` where `arr` is a Go `[N]T`): `&value.Value` is the address of the golib `array<T>` **struct wrapper** — which holds the backing `T[]` as a reference field, an offset, and a length — *not* the address of the array data, and the `fixed` releases it before the operator even returns. A native syscall handed that address writes over the wrapper's fields (clobbering the `T[]` reference), so a later `buf[i]` reads through a corrupted array and faults. This is exactly the go-isatty MSYS/cygwin-pipe probe: `IsCygwinTerminal` fills a `[262]uint16` with a `FILE_NAME_INFO` via `GetFileInformationByHandleEx(…, uintptr(unsafe.Pointer(&buf)), …)`, then reads `l := *(*uint32)(unsafe.Pointer(&buf))` (the `FileNameLength`) and slices `buf[2 : 2+l/2]` — the garbage `l` drove `array<uint16>.get_Item(Range)` off the end (`AccessViolationException`). The converted **fatih/color** sample went **empty on a pipe** because of it: fatih/color's `NoColor` probe evaluates `!isatty.IsTerminal(fd) && !isatty.IsCygwinTerminal(fd)`, so only a *pipe* (where `IsTerminal` is false, unlike a console, and `GetFileType` is `FILE_TYPE_PIPE`, unlike a file) reaches the faulting FFI call — file-redirect and real-console output were fine, matching the observed matrix. The operators now special-case a value that is a Go fixed array — an `IArray` that is **not** an `ISlice` (a `slice<T>`'s `&s` is its header, exactly as in Go, so slices stay on the value-slot path) — and return the pinned address of element 0 of the backing `T[]`, via a `PinnedBuffer` (a `GCHandle.Alloc(…, Pinned)`) cached on the box. The pin lives for the box's lifetime — so the syscall write lands in the real backing array and every managed read afterward (the `l` reinterpret and the `buf[2:]` slice) observes it — and is released when the box is collected (the `PinnedBuffer` finalizer frees the handle). This is a golib-only change (no emitted-code difference); the array-buffer-to-syscall pattern that previously faulted now runs, while non-array pointers keep the existing transient `fixed`-address behavior byte-for-byte. (Guarded by the `FixedArrayBufferPointer` behavioral **output** test — the `*(*uint32)(unsafe.Pointer(&buf))` read-back idiom, the array still readable through its own indexer afterward, and address-stability across repeated conversions, vs Go; end-to-end, the converted fatih/color sample now prints byte-identically to `go run` through a pipe.)

**EVERY managed address handed to native code is pinned for the pointer's lifetime — a `fixed` block cannot outlive its own statement (2026-08-03, r38-os-fin).** The entry above pinned the ONE case that had been proven to corrupt memory; the general case was left with the transient `fixed` address, under a soundness note in `syscall/dll_windows.cs` that called the window between capture and the trampoline's `calli` "short and allocation-free". It is neither, for a BLOCKING syscall: the window stays open for as long as the kernel takes. `os`'s `TestPipeEOF` parks in `ReadFile` on a pipe for 10 ms per read while the rest of a parallel suite allocates around it, and a gen0 collection in that window moves both the `*uint32` byte-count box `syscall.Read` passes and the caller's read buffer — measured directly, a `heap(new uint32(), out var Ꮡdone)` box and a `Ꮡ(buf, 0)` element pointer BOTH report a different address after one forced collection. The kernel then writes to neither: `done` stays 0, `syscall.Read` returns `(0, nil)`, and `internal/poll`'s `FD.eofError` turns that into a premature `io.EOF`. That is the whole of the row characterized as *"`bufio.Reader.ReadBytes` over a converted `os.Pipe` returns a premature `io.EOF`, and only under parallel load"*, and it explains its measured shape exactly — monotone in the parallelism level (0 of 4 at `-parallel 1`, 1 of 3 at 4, 5 of 5 at 8, 100% at the default), because more threads means more allocation means more collections inside the same 10 ms window; independent of finalizers, which a control had already ruled out. The buffer's half of the same defect writes 4 KB into freed heap, which is the moving-site `ExecutionEngineException` recorded beside it. **golib now pins before it reads an address**: `ж<T>`'s `uintptr`/`void*` operators call `EnsureStableAddress`, which takes a lifetime `GCHandle` on the ROOT storage the pointer names — a standard heap box pins its own value slot, an element reference pins the canonical backing array, a field reference recurses to the allocation that contains the field — on exactly the terms `pinnedArrayData` already used for the fixed-array case, and released when the box is collected. The enabling change is that a standard heap box's value STORAGE is now a one-element array for a `T` that contains no references (`ж<T>.m_slot`): a box is a class with reference fields and `GCHandle` refuses to pin anything that contains pointers, so the value had nowhere pinnable to live. It is allocated EAGERLY and never migrated — `heap<T>(out ж<T>)` hands the caller a `ref` alias before any address is taken (`ref var done = ref heap(new uint32(), out var Ꮡdone)`), so moving the storage on first address-take would leave that alias on the abandoned copy, which is this very bug one level down. A `T` that DOES carry references gets no slot and keeps the transient address: its C# layout is not a native layout either, so no syscall can meaningfully be handed its address — the change is additive, and `RuntimeHelpers.IsReferenceOrContainsReferences<T>()` is a JIT constant, so neither the branch nor the allocation costs such a box anything. This also makes Go's unsafe.Pointer RULE 3 (pointer arithmetic through `uintptr`) sound, which it silently was not. golib-only — no emitted-code difference. (Guarded by `src/tests/GolibTests/NativeAddressStabilityTests.cs`, a neutered-fix control across all four box kinds plus the reference-bearing negative case: with `EnsureStableAddress` removed every address assertion fails on the first forced collection. Operationally, `os`'s residual went from 13 rows to 3 in one change — `TestPipeEOF` and the whole child-stdout family, whose empty child output was the same premature EOF read through `exec`'s pipe.)

**Atomic pointer ops on a MANAGED pointer field read/write the reference, not a `uintptr`.** The lock-free-cache idiom `atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))` / `atomic.StorePointer(…, unsafe.Pointer(v))` — where `x.field` has type `*T` and so holds a `ж<T>` reference — cannot go through the literal conversion: `new @unsafe.Pointer(v)` round-trips the managed reference through its (transient) address, and `(ж<@unsafe.Pointer>)(uintptr)(FromRef(ref …field))` dereferences raw memory, losing GC identity (it NRE'd on the very first read — x/sys/windows's `LazyDLL`/`LazyProc` proc caches at package-init). `convCallExpr.managedAtomicPointerIdiom` recognizes the idiom (the callee is `sync/atomic.LoadPointer`/`StorePointer` and the argument is `(*unsafe.Pointer)(unsafe.Pointer(&Z))` with `Z` of pointer type) and emits golib's managed-referent overloads on the **field box** directly: `atomic.LoadPointer(Ꮡx.of(T.Ꮡfield))` → `ж<ж<T>>` → `Volatile.Read` returning `ж<T>`, and `atomic.StorePointer(Ꮡx.of(T.Ꮡfield), v)` → `Volatile.Write` of the plain `ж<T>` (the stored value unwrapped from its `unsafe.Pointer(…)` conversion). The overloads are additive — a `ж<ж<T>>` argument never matches the existing `ж<@unsafe.Pointer>` (`= ж<Pointer>`) signature, so ordinary `unsafe.Pointer` atomics are untouched. The load stays `unsafe.Pointer`-typed to Go, so a caller's `== nil` still renders `(uintptr)… == nil`; the `ж<T> → uintptr` operator (above) yields 0 for a nil box, so the nil test is correct with no change to the surrounding emission. Blast radius is only the packages using the idiom (x/sys/windows and a handful of stdlib sites), each a pure re-shaping to the managed overload; CNR byte-identical across the behavioral corpus. (Guarded by the `ManagedAtomicPointer` behavioral **output** test — a `*proc`-field lock-free cache initialized once and re-read, vs Go; it NRE'd before the fix.)

The `ref` the helper takes depends on how the pointer argument **renders**. A genuine box — an address-of expression, a local pointer variable, a pointer field, a call result — is the `ж<T>` object, so the ref goes through its boxed value: `FromRef(ref (box).Value)`. But a **deref-aliased** pointer — a pointer *parameter* or pointer *receiver*, which the body renders as the pointed-to value alias (`ref var p = ref Ꮡp.Value`) — is not a box; `.Value` on it is `CS1061` (`nuint` has no `Value` — runtime `select.go` `unsafe.Pointer(pc0)` and `heapdump.go` `unsafe.Pointer(pstk)`, both `*uintptr` parameters). The alias is itself a ref-local into the boxed storage, so the converter takes its ref directly: `FromRef(ref p)`. Detection reuses `exprIsDerefAliasedPointer` (the same discriminator the pointer-reinterpret block uses). This also let the `guintptr`/`muintptr` receiver family (`runtime2.go` `(*uintptr)(unsafe.Pointer(gp))` inside `guintptr.cas`) compile — previously `ref (gp).Value` bound the `[GoType]` wrapper's `Value` *property* (CS0206); the CAS it feeds (`atomic.Casuintptr`) is a `partial` asm stub, so the copy-box semantics match the established reinterpret precedent (compile-milestone bar; the faithful managed-referent `ж<T>` model for those types remains a separate effort). (The bare `unsafe.Pointer(p)` pin stays exercised across the stdlib — runtime `select.go`/`heapdump.go`, and `runtime2.go`'s genuine `*guintptr`→`*uintptr` reinterpret here, whose differing element types keep it off the identity path. The `UnsafePointerParamPin` behavioral **output** test now guards the same-type **identity collapse** of the `(*uintptr)(unsafe.Pointer(p))` shape it originally used — see *A SAME-TYPE reinterpret … collapses to the pointer itself* above — where the whole conversion elides to the box; a `(*byte)(unsafe.Pointer(&value))`-style DIFFERENT-type reinterpret still pins through `FromRef`.)

**Returning an `unsafe.Pointer` parameter whole is a plain value return.** The return path boxes a *pointer parameter* returned whole (`return p` → `return Ꮡp` — the value alias cannot bind the pointer result), and the pointer-result check counts the `UnsafePointer` basic as a pointer. But an `unsafe.Pointer` parameter renders as a plain **value** param (`@unsafe.Pointer zero`) with *no* box, so the prefix referenced a nonexistent `Ꮡzero`/`Ꮡv`/`Ꮡfd` (CS0103 — runtime `map.go` `mapaccess1_fat`/`mapaccess2_fat`'s `return zero`, `mem_windows.go`, and `panic.go` `readvarintUnsafe`'s tuple return). The box form now applies only when the returned parameter's own type is a **genuine `*T`** (deref-aliased, so `Ꮡp` exists); an `unsafe.Pointer` param returns as-is. (Guarded by the `UnsafePointerParamPin` extension — the whole-return, tuple-return, and genuine-`*T`-control shapes, values vs Go; cleared 4 runtime CS0103, 63 → 59.)

The **reverse** direction — reinterpreting a raw address *as* a pointer, `(*T)(p)` where `p` is an `unsafe.Pointer` (or `uintptr`) — is the reinterpret pattern referenced above. Its result is the pointer type `ж<T>`. A plain `(ж<T>)p` cast is `CS0030`: because `unsafe.Pointer` is `Pointer : ж<uintptr>`, reaching `ж<T>` needs the two chained user-defined conversions `Pointer → uintptr → ж<T>`, and C# performs at most one user-defined conversion in a cast. The converter routes explicitly through `uintptr` — `(ж<T>)(uintptr)(p)` — which reads the `T` at `p`'s address via golib's `explicit operator ж<T>(uintptr value) => new ж<T>(*(T*)value)` (with `uintptr(Pointer) => Value`, the address the pointer holds). The deref `*((*unsafe.Pointer)(k))` then adds `.Value`: `((ж<@unsafe.Pointer>)(uintptr)(k)).Value` — Go's read of the `unsafe.Pointer` stored at `k`. This is the identical routing the *dereference* path (`(*int)(p)` inside `*(...)`) already used via its `isPointerCast` flag; the fix extends it to the two shapes that did **not** set that flag: a bare call **argument** `atomicwb((*unsafe.Pointer)(ptr), new)` (runtime `atomic_pointer.go`) and an **extra-paren** deref `*((*unsafe.Pointer)(k))` (runtime `map.go`'s indirect key — `convStarExpr`'s dereference branch sees a `ParenExpr`, not the `CallExpr`, so it never marks the cast). Gated to a **pointer-result** conversion whose **argument** is a raw address (`unsafe.Pointer`/`uintptr` basic); the pointer-to-*named*-type value conversion `(*Base)(defPtr)` (below) has a `*T` argument, is handled earlier, and is not affected. Like every reinterpret through the `uintptr` round-trip, the golib operator reads/boxes a **copy** from a `fixed` address, so this is memory-layout-dependent code whose runtime values are **not the contract** — golib's own `map<K,V>` is what actually runs; the converted `runtime/map.go` only needs to compile. (Guarded by the `UnsafePointerReinterpret` behavioral **Compile + Target** test — both the extra-paren deref and the bare-argument shapes; cleared all 21 `unsafe.Pointer → ж<unsafe.Pointer>` CS0030 in `runtime`, 137 → 114.)

**A SAME-TYPE reinterpret `(*T)(unsafe.Pointer(p))` where `p` is already `*T` collapses to the pointer itself.** Converting a `*T` to `unsafe.Pointer` and back to the *same* `*T` is a no-op identity in Go — the language spec makes `(*Builder)(abi.NoEscape(unsafe.Pointer(b)))` exactly `b.addr = b` (strings.Builder's copy-by-value guard; the type's own TODO says to revert it to that once escape analysis improves). The `uintptr` round-trip above is **wrong** for this shape: golib's `ж<T>(uintptr)` DEREFERENCES-and-COPIES, so `b.addr` became a fresh box over a *copy* of the receiver, never reference-equal to it — and the guard's own `b.addr != b` self-check FALSE-PANICKED on the second call to any `strings.Builder` method (a `Grow` then a `WriteString`, or `strings.Join`'s repeated `WriteString`), surfacing as `panic: strings: illegal use of non-zero Builder copied by value` in the converted **fatih/color** `-recurse` sample the moment color was enabled. `convCallExpr.pointerReinterpretIdentitySource` intercepts this exact shape at the top of the conversion path — a `(*T)(…)` whose source, after peeling an optional escape-analysis identity wrapper (`abi.NoEscape` or a package-local `noescape`, matched by name **and** `unsafe.Pointer→unsafe.Pointer` signature), is `unsafe.Pointer(p)` with `p` of the *identical* pointer type `*T` — and emits `p`'s **box** directly (in the `isPointer` context, so a deref-aliased receiver/param renders `Ꮡb`, not its value alias `b`):
```csharp
internal static void copyCheck(this ж<Builder> Ꮡb) {
    ref var b = ref Ꮡb.Value;
    if (b.addr == nil) {
        b.addr = Ꮡb;                       // was (ж<Builder>)(uintptr)(abi.NoEscape((uintptr)@unsafe.Pointer.FromRef(ref b)))
    } else if (b.addr != Ꮡb) {
        throw panic("strings: illegal use of non-zero Builder copied by value");
    }
}
```
This preserves pointer identity AND shared storage (a write through the reinterpreted pointer now flows back, unlike the copy). A **different** element type is a genuine reinterpret and keeps the `uintptr` round-trip; the interception is `(*T)`-target- and same-element-type-gated (`types.Identical(srcElem, targetElem)`), so it fires ONLY for the identity. Across the 302-package stdlib it rewrites exactly **8** latently-miscompiled sites (`strings.Builder.copyCheck`, `internal/reflectlite`, `internal/syscall/windows/registry`, `os`, `syscall`, and three `runtime` sites) to the cleaner, correct box form — CNR byte-identical everywhere else; the bare `unsafe.Pointer(p)` pin (61 files) and the genuine-reinterpret round-trip (130 files) both remain and stay compile-guarded by the full build. (Guarded by the `PointerReinterpretIdentity` behavioral **output** test — a Builder-style `copyCheck` self-reference called repeatedly must NOT panic, and a genuine copy-by-value MUST still be caught, vs Go; it panicked before the fix — plus the identity-collapse arms of `UnsafePointerParamPin` (param/receiver/field), `PointerSelectorDeref`, and `PointerCastSliceRange`.)

**The identity also collapses when the source pointer is reached DIRECTLY — `*(*T)(p)` / `(*T)(p)` with `p` already `*T`.** This is the same no-op, minus the `unsafe.Pointer` hop: Go's way of re-reading a pointer at a fixed type. It was **not** recognised, and the deref path made it worse than the round-trip above. `convStarExpr`'s casted-pointer-deref branch sets `isPointerCast`, and the conversion renderer took that flag *alone* as licence to emit the raw-address bridge `(ж<T>)(uintptr)(p)`. But `isPointerCast` means only "this conversion is the operand of a deref" — it says nothing about the source being an **address**, and the bridge is only ever correct for one that is. A typed Go pointer is a managed **box**, and a deref-aliased pointer parameter renders as that box's *value alias*, so the `(uintptr)` leg had no conversion at all:

```csharp
// Go:  func derefStruct(p *Pt) Pt { return *(*Pt)(p) }
internal static Pt derefStruct(ж<Pt> Ꮡp) {
    ref var p = ref Ꮡp.Value;
    return ~(ж<Pt>)(uintptr)(p);           // CS0030: cannot convert 'Pt' to 'uintptr'
}
```
```csharp
internal static Pt derefStruct(ж<Pt> Ꮡp) {
    return ~Ꮡp;                            // the box, dereferenced in place
}
```

`pointerReinterpretIdentitySource` now accepts either source form — the direct pointer, or one unwrapped from `unsafe.Pointer(p)` — so the identity is intercepted before the bridge is ever considered. Emitting the box is correct for all three uses at once: a **value read** copies (`~Ꮡp`, plus the array `.Clone()` where the element is an array), an **lvalue write** lands on the real storage (`(Ꮡp).Value = …`) rather than on the round-trip's copy, and **pointer identity** is preserved. Note the recognition must stay pinned to a genuine `(*T)(…)` **conversion** — its `Fun` must denote a type. Matching on argument type alone collapses any one-argument *call* that takes and returns the same pointer type, silently deleting it (`advance(a)` → `a`, `Ꮡp.Swap(Ꮡa)` → `Ꮡa`); CNR caught exactly that across 13 behavioral projects. The corpus-wide A/B footprint is **two lines in one file** — `time.NewTimer`/`AfterFunc`'s `(*Timer)(newTimer(…))`, where `newTimer` already returns `*Timer`, shed a redundant identity cast — because the CS0030 shape needs a pointer *parameter*, which the stdlib's own reinterprets never use; the defect bites converted end-user code and behavioral guards. (Guarded by the `TypedPointerCastDeref` behavioral **output** test — struct, named-numeric, via-`unsafe`, non-deref, lvalue, and local-pointer shapes, plus the one-argument-call over-match control — and by the strengthened `ArrayCastDerefClone`; both verified to FAIL with the fix neutered, with that exact CS0030.)

**Still routed through the bridge (a known gap):** a typed-pointer source whose element type *differs* but shares an underlying — Go permits `(*T)(p)` there — is only partly covered by the named↔named / named↔basic / named↔array re-box routes below. A tag-differing struct pair (`types.Identical` counts tags, Go's conversion rule does not), an unnamed-array ↔ named-array pair, and a named ↔ unnamed struct pair all fall through to the raw-address bridge and mis-render. None occurs in the stdlib corpus, and narrowing the bridge gate without a correct box→box route for them merely trades one broken form (`(ж<Row>)(uintptr)(p)`) for another (`(ж<Row>)p`), so the gate is left as-is and the shapes are recorded here.

A deref whose **starred inner is a func type** (or any non-identifier type) — `*(*func())(add(…))`, runtime `panic.go`'s deferred-slot read `return *(*func())(add(p.slotsPtr, i*…)), true` — misses the identifier-gated cast-deref branch and falls to the default deref path, which must **wrap the cast before `.Value`**: C# postfix binds tighter than a cast, so a naked `.Value` re-binds onto the cast's *inner* operand (`(ж<Action>)(uintptr)(add(…)).Value` reads the inner `@unsafe.Pointer`'s `uintptr` — CS0029 `ж<Action>`→`Action` in the tuple return). The default deref now wraps any type-conversion operand: `(((ж<Action>)(uintptr)(add(…))).Value, true)`. This is the fourth instance of the cast-precedence/extra-paren family, and **indexing** a reinterpret result directly is the fifth: `(*[2]uint64)(x)[0] = 0` (runtime `malloc.go`) appended the pointer-to-array auto-deref `.Value` and the index to the cast render — `(ж<array<uint64>>)(uintptr)(x).Value[0]` read the inner `@unsafe.Pointer`'s `uintptr` and indexed a `nuint` (CS0021); the index emission now wraps a type-conversion base the same way: `((ж<array<uint64>>)(uintptr)(x)).Value[0]`. (Guarded by the `UnsafePointerReinterpret` extensions — the func-type deref in a tuple return and the indexed reinterpret write/read.)

The unsafe builtins `unsafe.Add`, `unsafe.Slice`, and `unsafe.String` accept a length/offset of **any integer type** (Go's `IntegerType` constraint, which includes `uintptr`/`uint`). golib's implementations therefore take a generic `IBinaryInteger` length, truncated to the `int` offset — so `unsafe.Slice(p, uintptrLen)` binds without an explicit cast (a plain `nint` parameter rejected a `uintptr`/`uint` argument with CS1503). (Guarded by `UnsafeBuiltinIntegerLen`.)

Passing an `unsafe.Pointer` **argument to an `unsafe.Pointer` parameter** keeps the `@unsafe.Pointer` struct value — `add(p, x)`, not `add(p.Value, x)`. The struct is an exact match for the parameter. (Guarded by `UnsafePointerArgPassing`.)

**Array-backed defined types reinterpret through storage-sharing `Value` refs, not value copies.** The fiat field-arithmetic shape (crypto/internal/edwards25519 `scalar.go`) reinterprets `&s.s` (a `fiatScalarMontgomeryDomainFieldElement`, written directly over `[4]uint64`) as `(*[4]uint64)` — and as its *sibling* `(*fiatScalarNonMontgomeryDomainFieldElement)` — then **writes element-wise through the reinterpreted pointer** (`fiatScalarFromBytes` parses INTO `&s.s` on a virgin receiver). Neither the copy-boxing named↔named route (each `[GoType("[N]elem")]` wrapper converts only to `array<E>`; a sibling cast needs two chained user conversions — CS0030) nor a plain `ж<>` cast (distinct instantiations) works, and any copy-based route would materialize the wrapper's **lazy** backing on a temp and orphan every write. The emission derefs through the ref-returning `ж<T>.Value` and invokes the wrapper's `Value` property in place — `Ꮡ((Ꮡs.of(Scalar.Ꮡs)).Value.Value)` (underlying-array form) / `Ꮡ((nonMont)((…).Value.Value))` (sibling form, one implicit conversion from `array<E>`) — materializing the backing on the ORIGINAL storage and boxing an `array<E>` struct that shares its `T[]`: element reads and writes flow through. Gating consults the type's **written RHS** (a new per-package pre-pass records each `TypeSpec`'s declared right-hand side, which `Named.Underlying()`'s full resolution loses): only types written *directly* over an unnamed array take this route, so chain-defined view wrappers (`type pallocBits pageBits`) keep the existing copy-box route byte-identically; the same written-RHS gate lets `isTypeConversion` claim the pointer-to-type-literal target `(*[4]uint64)(…)` (no `types.Object` exists for a composite type) without disturbing the pointer-cast slice form (`(*[1<<20]Method)(p)[:n:n]`, internal/abi). Caveat (documented, no stdlib site): a *whole-value* write through the reinterpreted box (`*p = q`) rebinds only the boxed struct. (Guarded by the `NamedArrayWrapper` extensions — a virgin-field write through the underlying reinterpret, a sibling reinterpret aliasing the same storage read-during-write, and a heap-boxed local, all output-compared vs Go.)

**The `uintptr → ж<T>` raw-address reinterpret operator is `explicit` by design.** It boxes a **copy** of the value read at an arbitrary address (the runtime-unsafe reinterpret seam) — never something to happen silently, and every converter-emitted reinterpret already uses explicit cast syntax (`(ж<T>)(uintptr)(p)`). As an *implicit* conversion it also poisoned overload resolution: a `uintptr` argument converted to **both** an `@unsafe.Pointer` parameter (via the numeric `uintptr ↔ Pointer` operators, which stay implicit) and any `ж<T>` parameter, so a **free function and a same-named pointer-receiver method** — runtime's `func add(p unsafe.Pointer, x uintptr)` (stubs.go) vs `func (p *notInHeap) add(bytes uintptr)` (malloc.go), both emitted as static `add` overloads in the package class — were ambiguous (CS0121) at every free-call site whose argument is a **pin of a boxless receiver**: inside a `[GoRecv] ref` method, `unsafe.Pointer(b)` emits the `uintptr`-typed `(uintptr)@unsafe.Pointer.FromRef(ref b)` (runtime `map.go` `b.keys()`/`b.overflow()`/`b.setoverflow()`, `mprof.go`'s stack-record walkers — 6 sites). With the operator explicit, the `uintptr` argument binds only the `@unsafe.Pointer` overload. The reverse `ж<T> → uintptr` (box → address) operator remains implicit — producing a number is not a silent deref. (Guarded by the `FuncVsMethodOverload` behavioral **output** test — the free `add` + direct-ж method `add` overload pair with the boxless-receiver pin call shape, plus both method-call forms, values vs Go; cleared all 6 runtime CS0121, 59 → 53.)

**`uintptr(unsafe.Pointer(x))` builds no `Pointer` object — the operand converts straight to `uintptr` (2026-08-03, r39c).** Go's most common syscall idiom converted to `(uintptr)new @unsafe.Pointer(x)`, and that object was provably dead. golib's `Pointer` is a `ж<uintptr>` whose only value-taking constructor takes a `uintptr`, so the operand is ALREADY converted — by `implicit operator uintptr(ж<T>)`, the very operator the enclosing cast would use — before the wrapper exists; the wrapper stores that finished number in its own one-element slot, and the cast reads it straight back out. The round-trip is the identity, exactly: `uintptr(Pointer)` returns `value.IsNull ? 0 : value.Value`, and the constructor marks the box nil precisely when the address is 0 (`base(value, value == 0)`, with `ж<uintptr>`'s value-peeking `IsNull` arm unable to fire for a value-typed pointee). So the wrapper is no longer emitted:

```go
r1, _, e1 := Syscall(procConvertSidToStringSidW.Addr(), 2, uintptr(unsafe.Pointer(sid)), uintptr(unsafe.Pointer(&stringSid)), 0)
```
```csharp
var (r1, _, e1) = Syscall(procConvertSidToStringSidW.Addr(), 2, (uintptr)Ꮡsid, (uintptr)ᏑstringSid, 0);
```

**The pin is what makes this safe to elide, and it is not the wrapper's.** Since r38 every managed address handed to native code is pinned for the POINTER's lifetime (*EVERY managed address handed to native code is pinned…*, above) — but `EnsureStableAddress` / `pinnedArrayData` set `m_pin` on the **operand** box, released when the operand is collected. The `Pointer` wrapper owns no pin, holds no reference to the operand, and tracks no lifetime; its `ж<uintptr>` slot holds a number. Eliding it therefore cannot shorten any pin, move any address, or change any value — a strictly-dead allocation, which is why this is a peephole and not a semantic change.

The rule is keyed at the **wrapper's own emission site** (`unsafePointerBoxEmission`, marked per-CallExpr by `markDeadUnsafePointerBox` from the enclosing conversion), so it is self-limiting: the `unsafe.Pointer` arms that render a raw address by other means — `@unsafe.Pointer.FromRef(ref x)` for a deref-aliased pointer receiver, `((@unsafe.Pointer)(uintptr)v)` for a named uintptr/pointer operand, and `(@unsafe.Pointer)default!` for the literal `nil` — never consult the mark and are byte-identical. The enclosing conversion must be one that renders a `(uintptr)` cast around the operand, which is the basic `uintptr(…)` target and the named-over-uintptr target that hops through its underlying (`Handle(…)`, `syscall/security_windows.cs`'s `LocalFree` defers). An operand that binds looser than a cast is parenthesized (`unsafe.Pointer(uintptr(p) + off)`); an address-of operand is not, because `&x` renders as the primary box form `Ꮡx` / `Ꮡ(…)` / `Ꮡs.at<T>(i)`.

Corpus footprint: **494 sites across 45 files** in a seeded two-temp-root A/B whole-stdlib reconvert — 249 in `syscall/zsyscall_windows.cs` alone, 65 in `internal/syscall/windows/zsyscall_windows.cs`, 25 each in `runtime/heapdump.cs` and `runtime/os_windows.cs` — plus 10 across four behavioral goldens; every changed line in the A/B is reproduced exactly by the transformation and none is anything else. Measured with a `GC.GetAllocatedBytesForCurrentThread` probe over 2,000 calls: `syscall.Write` **1,072 → 544 B/op** (−49.3 %) and `os.File.WriteString` **9,208 → 8,680 B/op**, i.e. 528 bytes — three wrapper objects — off every zsyscall wrapper call chain. (The hand-owned `crypto/subtle/xor_generic.cs` keeps its three sites, as it must: the converter regenerates it into the `xor_generic.cs.auto` review sibling instead of over it.) (Guarded by the `UintptrUnsafePointerIdiom` behavioral **output** test — address identity across two takes of one global, a pointer parameter, the nil pointer's 0, an array-element stride, an existing `unsafe.Pointer` value round-trip, a dereferenced pointer-to-pointer operand, and a write read back through the same pointer, all vs `go run` — plus the re-baselined `NilPointerUintptr`, `NilPointerParamUnsafePointer`, `FixedArrayBufferPointer` and `UnsafeOperations` goldens, which diverge the moment the peephole is neutered.)

### An OPAQUE `*struct{}` conversion mints a recoverable token — `syscall.Pointer(unsafe.Pointer(p))` keeps its referent

Windows type definitions use `type Pointer *struct{}` for a "pointer to one of many types" field — `CERT_CHAIN_POLICY_PARA.pvExtraPolicyPara`, `WSAMsg.Name` — and Go populates it with `T(unsafe.Pointer(p))` over whatever record the API expects. The emission used to be the numeric chain `(T)(ж<EmptyStruct>)(uintptr)(new @unsafe.Pointer(p))`, which projects p's box to a scalar at the `@unsafe.Pointer` constructor (the class holds only a number — `Pointer : ж<uintptr>`). For a pointee **carrying managed references** that scalar is a transient GC-heap address with no recoverable box behind it: golib's `uintptr` operator pins only reference-free storage, so the number that reaches the boundary is neither stable nor resolvable, and the referent's liveness ends at the JIT's discretion.

The measured victim was crypto/x509's `checkChainSSLServerPolicy` — the mint-site problem the CryptoAPI chain arc left open by name. Its `SSLExtraCertChainPolicyPara` holds `ServerName *uint16`, a managed reference, so the SSL policy parameter crossed into `CertVerifyCertificateChainPolicy` as a wrong-layout transient address: `Fatal error 0xC0000005` inside `Syscall6`, the last crypt32 member standing between `crypto/tls` and a roster row.

The conversion now emits golib's referent-preserving mint whenever the target's underlying type is `*struct{}` and the source is `unsafe.Pointer(p)` over a Go pointer whose box the emitter has in hand:

```go
sslPara := &syscall.SSLExtraCertChainPolicyPara{ AuthType: syscall.AUTHTYPE_SERVER, ServerName: servernamep }
para := &syscall.CertChainPolicyPara{ ExtraPolicyPara: (syscall.Pointer)(unsafe.Pointer(sslPara)) }
```

```csharp
var sslPara = Ꮡ(new syscall.SSLExtraCertChainPolicyPara(AuthType: syscall.AUTHTYPE_SERVER, ServerName: servernamep));
var para = Ꮡ(new syscall.CertChainPolicyPara(ExtraPolicyPara: ((syscall.Pointer)ManagedPointerTokens.MintOpaque(sslPara))));
```

`ManagedPointerTokens.MintOpaque` (`golib/ж.PointerTokens.cs`) keeps the numeric route **byte for byte** for every pointee that route already answered exactly — nil is 0, a native-backed box is its real address, a reference-free pointee pins and reports stable storage — and diverges only for the reference-bearing class, where three things happen at the mint: the scalar becomes the box's own `PointerOrderToken`; the token is `Register`ed so a boundary wrapper recovers the box with `Resolve` (the same round trip the reflect projection above and the ADDRINFOW hand-own's sockaddr fields take — this is the table's third minter); and the minted `ж<EmptyStruct>` holds the referent reachable for its own lifetime through a `ConditionalWeakTable`, so the opaque pointer keeps its pointee alive exactly as the Go pointer it stands for would — the referent is otherwise reachable only through a local the JIT is free to retire before the syscall that consumes the token.

The consuming side is `syscall`'s hand-owned `CertVerifyCertificateChainPolicy` (`zsyscall_windows_certchain_impl.cs`): a scalar the table recognizes as an `SSLExtraCertChainPolicyPara` box is transcribed — server name and all — into native mirrors for exactly the duration of the call; a scalar it does not recognize passes through unchanged, which is both the nil pointer and any genuinely native (or pinned reference-free) address, for which pass-through is the correct native call.

Scope is deliberate on three edges. The target must be `*struct{}`-underlying: such a type names an opaque pointer **by construction** (there is nothing to dereference), so no reader can depend on the scalar being a dereferenceable address; wider named-pointer targets keep their existing routes. The source must be the `unsafe.Pointer(p)` call form — a stored `unsafe.Pointer` variable or a `uintptr` has already lost its referent and keeps the numeric chain. And the emission-level fix, rather than a hand-own of the one x509 function, is what covers every author of the Go shape — `internal/poll`'s six `WSAMsg.Name` mints (whose `RawSockaddrAny` referents now stay alive for the message's lifetime, ready for the WSA-msg wrappers when that arc lands), converted test suites, and user code. (Guarded by the `SystemCertVerify` behavioral **output** test's policy rows, which drive the same mint from test code against a trusted-by-waiver chain: with `CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG` set, a MATCHING server name answers `0` and a MISMATCHED one answers `CERT_E_CN_NO_MATCH` — an answer crypt32 can only give if the name crossed the boundary intact — and with nothing waived the same chain answers `CERT_E_UNTRUSTEDROOT`. Pre-fix, the first policy call dies with the access violation above.)

### Pointer DISPLAY never dereferences out-of-range; `unsafe.StringData("")` is nil

Printing a pointer (`ж<T>.ToString()` → `PrintPointer`, the stub-fmt fallback for `%v`/`%p` of a
pointer) only needs an address-like `0x…` token, but the printer read `ptr.Value` to derive one —
and an array/slice-ELEMENT reference can legally sit outside its backing store's valid range (the
zero index of an EMPTY pinned buffer, or one-past-the-end pointer arithmetic), where that read
throws `IndexOutOfRangeException` and kills the host (strings' `TestClone`, Phase-4 row R9).
`PrintPointer` now checks an element reference's index against its backing store first and prints
the BACKING STORE's identity when the element is unreadable — stable per pointer box, never a
throw. Relatedly, `unsafe.StringData` of an EMPTY string now returns **nil**: Go documents the
empty-string result as unspecified-may-be-nil, its runtime returns nil (probed — so distinct empty
strings' data pointers compare EQUAL, which `TestClone` asserts), and golib's
pin-a-fresh-buffer-per-call implementation could never satisfy that identity. Addresses differ run
to run, so behavioral coverage checks printed SHAPE and nil-identity (`UnsafePointerPrint`); the
out-of-range print itself has no Go-parity spelling from converted code today (the
`unsafe.Add`-through-`unsafe.Pointer` seam loses the element box), so that property is guarded at
the golib level by `GolibTests.PointerPrintTests` — a golib UNIT-test project (beside
`ChannelTests` under `/tests/library/`) for runtime properties no Go↔C# output comparison can
reach.

**`unsafe.String(ptr, 0)` reads nothing, so it must not pin `ptr` either.** That same empty-buffer
element reference reaches the string builtin, by a route the standard library takes constantly on
Windows: `syscall.UTF16ToString` truncates its argument at the first NUL and returns
`unsafe.String(unsafe.SliceData(buf), len(buf))`, so an **all-NUL** `[N]uint16` — every unset
`WCHAR` field of a Win32 record (`WIN32_FIND_DATAW.cAlternateFileName` on a volume with 8.3 name
generation disabled, `MIB_IFROW.wszName`, `PROCESSENTRY32.szExeFile`, `STARTUPINFO.lpDesktop`) —
arrives as a zero-length `buf`. `SliceData` is documented to return a **non-nil** pointer to an
unspecified address for a non-nil slice of capacity 0 (only a *nil* slice yields nil), which this
model materializes as an index-0 box into a zero-length backing array; `unsafe.String` then pinned
that referent (`fixed (byte* p = &ptr.Value)`) to build the string and threw
`IndexOutOfRangeException` where Go returns `""`. The zero-length case now returns the empty string
**before** the pointer is touched, which is precisely Go's rule: a run-time panic occurs only when
`ptr` is nil *and* `len` is not zero, so a length of zero dereferences nothing whatever the pointer
is. `SliceData` is deliberately left alone — Go specifies the non-nil-pointer-to-unspecified-address
result for `cap == 0`, so returning nil there to dodge the deref would trade a throw for a wrong
answer. `unsafe.Slice(ptr, 0)` needs no matching guard: its element-window route
(`TryGetElementWindow`) already yields an empty aliasing window over a zero-length backing without a
deref. (Guarded by the `UnsafeStringEmpty` behavioral **output** test — the all-NUL `[14]`/`[260]`
buffers, terminated/unterminated/leading-NUL controls that keep truncation honest, and the
`SliceData`/`StringData` zero-length matrix with non-zero-length positive controls, compared vs
`go run`; verified to throw with the guard removed.)

**`unsafe.SliceData` is an INTERIOR POINTER, not a pin.** Go defines it as `&slice[:1][0]`, so the
faithful model is the array-element reference `Ꮡ(s, 0)` — the exact box the converter emits for
`&s[0]`. It was instead a pinned-buffer box over `slice.buffer`, and that was wrong three ways at
once:

| The pin | The consequence |
|:--|:--|
| `GCHandle.Alloc(…, Pinned)` refuses storage whose element type carries a managed reference | `SliceData` over **any** such slice threw `ArgumentException: Object contains references`. `log/slog`'s `GroupValue` is the corpus witness — `groupptr(unsafe.SliceData(as))` over `[]Attr`, rebuilt by `unsafe.Slice` in `Value.group()` — and it infrastructure-errored every grouping path in the package (5 `testing/slogtest` rows). |
| The pin covered the whole backing array from index 0, ignoring the slice's LOW bound | `SliceData(s[2:])` addressed element 0 rather than element 2, and did not compare equal to `&s[2]` as Go's pointer identity requires. |
| `PinnedBuffer` implements `IArray<byte>` alone | `ж<T>.Value`'s `array is IArray<T>` test failed for every element type but `byte`, so the derived pointer was undereferenceable — `InvalidOperationException` instead of the element. |

Pinning was never what `SliceData` *means*. An address is needed only when the pointer is converted
to `uintptr`/`void*`, and `ж` already pins on demand at exactly those conversions
(`EnsureStableAddress`), declining gracefully for storage that cannot be held still. The element
reference additionally makes the round trip **alias** rather than snapshot — `unsafe.Slice`'s
`TryGetElementWindow` arm rebuilds a window over the original backing, so a write through the
rebuilt slice reaches the source, which is Go's semantics. `StringData` keeps its pin: a `@string`
is a `byte[]`, always pinnable, and its emptiness identity is the property `TestClone` asserts.
(Guarded by the `UnsafeSliceDataAliasing` behavioral **output** test — a `[]struct{string;int}`
round-tripped through `SliceData`/`Slice` and written through, `SliceData(s) == &s[0]` identity, a
re-sliced source's low bound, an `[]int64` and a `[]*T` deref, and the plain `[]byte` control —
plus `UnsafeStringEmpty`, which pins the zero-length behavior above unchanged.)

## Value puns and pointer-word reads

### A numeric value pun READ is a bitcast — `*(*uint64)(unsafe.Pointer(&f))` boxes nothing

Go's `math.Float64bits` is one line, `return *(*uint64)(unsafe.Pointer(&f))`, and Go compiles it to a
register move. The general pointer-reinterpret emission (this section's neighbours) had to treat it like
any other address-of:
- `f` was heap-boxed because its address is taken;
- it was read back through `~Ꮡf.Reinterpret<float64, uint64>()`.

That is **three counted objects per call** where Go has none: the box, its pinnable slot, and the
reinterpreting field reference. GolibTests' `ValuePunBitcastTests` measures the old body at exactly 3.
Every caller paid it, including encoding/binary, strconv's float formatting, log/slog's float
`Value`s and gob.

**A pun that is only READ, between two same-size predeclared sized numerics** (int8…int64, uint8…uint64,
float32, float64), now renders as golib's value bitcast (`Unsafe.BitCast`). Its `&x` no longer counts as
address-taken, so `x` stays an ordinary parameter or local when that was its only address use:

```go
func Float64bits(f float64) uint64 { return *(*uint64)(unsafe.Pointer(&f)) }
```
```csharp
public static uint64 Float64bits(float64 f) {
    return bitcast<float64, uint64>(f);
}
```

**The shapes that keep the aliasing reinterpret, deliberately:**
- a pun that is WRITTEN, e.g. runtime/minmax.go's `*(*uint32)(unsafe.Pointer(&x)) |= …`, whose write
  must land in `x`;
- a pun whose result is addressed;
- a named type;
- `int`/`uint`/`uintptr`, whose width is the target's;
- `bool`, complex and every composite.

A pun read whose `x` is ALSO addressed elsewhere still bitcasts, and its box stays for that other use.
The recognition is a per-package pre-pass (`valuePunOperations.go`), and escape analysis skips the `&x`
it consumes. The `//go:cgo_unsafe_args` lift is the precedent for both.

**Stdlib footprint at `fa18863b94`:** all 12 production pun reads, identical on windows, linux and
darwin:
- math ×4;
- runtime's `float64bits`/`float64frombits` and its two histogram infinities;
- reflect's two float32 register moves;
- internal/runtime/atomic's `Float64.Load`/`Store`.

That is 5 files plus their position maps (9 per target), each target −49/+29, and the census is in
`docs/phase4/probes/c2-value-pun-census/`.

**Guarded by:**
- the `ValuePunBits` behavioral test (an accepted read, an address kept elsewhere, and a write), and
  `UnsafeOperations`, whose golden moves on its own three pun reads;
- the converter's `TestValuePunReadRendersAsABitcastWithoutABox`, controlled both ways;
- GolibTests' `ValuePunBitcastTests`.

### The pointer-word read — `*(*unsafe.Pointer)(unsafe.Pointer(&x))` emits a CARRYING pointer, never a byte pun

Go's idiom for reading the pointer word stored at some location — `time.syncTimer`'s
`*(*unsafe.Pointer)(unsafe.Pointer(&c))` reading a channel's header word, `runtime/stack.go`'s
`*(*unsafe.Pointer)(&pp)` re-typing a `uintptr` — has ONE destination the managed storage
reinterpret can never serve: `unsafe.Pointer` itself is a CLASS in the surrogate model, so golib's
alias gate refuses it, and the pre-2026-08-25 fallback deref-copied whatever bits sat in the
source's first reference-sized slot INTO a `Pointer` reference. A reference materialized from bytes
is a CLR type-safety break: junk dispatch on a quiet heap, an `AccessViolationException` when the
punned bits land unmapped — measured live on every `NewTimer` (the `asynctimerchan=2` witness,
2026-08-24: two pipeline runs died in `Pointer.op_Implicit`; three structurally-faithful quiet-heap
repros passed, the byte-view census's latent-with-live-trigger shape exactly).

`reinterpretManagedEmission` (convCallExpr.go) therefore special-cases a target pointee of the
`unsafe.Pointer` basic and emits the word the two ways the managed model can carry one:

```go
u := uintptr(0xC0FFEE)
q := *(*unsafe.Pointer)(unsafe.Pointer(&u))   // the word IS the number

c := make(chan int, 1)
p := *(*unsafe.Pointer)(unsafe.Pointer(&c))   // the word is a REFERENCE
```

```csharp
@unsafe.Pointer q = ~Ꮡ(new @unsafe.Pointer(~Ꮡu));           // exact fidelity: Pointer(value)
@unsafe.Pointer p = ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡc));   // the source BOX's pin token
```

The uintptr arm is exact Go semantics. The managed arm carries the source box's
`ManagedPointerTokens` pin token — non-nil for a live value, stable for the box's lifetime, and
provenance-resolvable back to the storage it names. One corner is knowingly inexact and stated at
the emission: a nil channel's word is 0 in Go while a box token is non-zero; the one stdlib
consumer (`newTimer`) reads only the nil-bit and recomputes it from the GODEBUG setting, and no
stdlib site passes a nil channel through this shape.

**Deliberately NO golib-side guard backs this up**, and the history is the load-bearing part: a
runtime refusal (return a zero-holding box for any reference-typed destination) was tried first and
the full behavioral suite failed `PointerCastSliceRange` — the `ж<T> → ж<U>` DOUBLE-pointer pun
(reflect `MapOf`'s `**(**mapType)(unsafe.Pointer(&imap))`) reads one ж instantiation's reference
slot as another and works precisely because the generic layouts coincide, a distinction golib
cannot draw generically and the converter draws exactly. Guarded by
`tests/Behavioral/UnsafePointerWordRead` (both arms vs `go run`, token stability across re-reads);
the corpus regen moved all 11 members of the shape (`sleep.cs` + ten runtime sites) and nothing
else. The 10 runtime sites are dormant raw-metal; a site that WRITES through the derived pointer
stores into a detached box rather than corrupting punned memory, and real write-through semantics
belong to the provenance arc.

## Keeping managed storage alive

### Pointer-derived funnel arguments and bridged `unsafe.Pointer` arguments keep their box alive across the call — the statement-scoped `GC.KeepAlive` drain

Go's `//go:uintptrkeepalive` contract (cmd/compile's `escape.rewriteArgument`, `cmd/compile/internal/escape/call.go`): an argument that is a `uintptr(<operand of type unsafe.Pointer>)` conversion to a syscall funnel keeps the pointed-to object alive for the duration of the call — the inline `uintptr(unsafe.Pointer(&x))`, the two-step `_p0 = unsafe.Pointer(&p[0]) … uintptr(_p0)` that mksyscall emits for every `[]byte` argument, and a pointer-typed variable passed as `uintptr(unsafe.Pointer(p))` alike. The funnel set is package-qualified through go/types (`syscallFunnelCall`, `syscallKeepAliveAnalysis.go`): `syscall.Syscall/Syscall6/9/12/15/18/SyscallN/RawSyscall/RawSyscall6`, `internal/runtime/syscall.Syscall6`, darwin's lowercase libc trampolines `syscall`, `syscall6`, `syscall6X`, `syscallX`, `syscallPtr`, `rawSyscall`, `rawSyscall6` (runtime linknames declared by package `syscall`), and `crypto/x509/internal/macos`'s own `syscall` (corefoundation.go). In C# the argument is a NUMBER over a pinned box, and nothing references the box once the argument is evaluated, so a collection during the call retires the pin the kernel is reading through. Emitted form: each pointer-derived argument is captured into a `ᴋN` temp declared before the statement, the call takes `(uintptr)ᴋN`, and `System.GC.KeepAlive(ᴋN);` follows the statement (`convSyscallFunnelCall`, drained by `visitStmt`):

```go
func recvfrom(fd int, p []byte, flags int, from *RawSockaddrAny, fromlen *_Socklen) (n int, err error) {
	var _p0 unsafe.Pointer
	if len(p) > 0 {
		_p0 = unsafe.Pointer(&p[0])
	} else {
		_p0 = unsafe.Pointer(&_zero)
	}
	r0, _, e1 := syscall6(abi.FuncPCABI0(libc_recvfrom_trampoline), uintptr(fd), uintptr(_p0), uintptr(len(p)), uintptr(flags), uintptr(unsafe.Pointer(from)), uintptr(unsafe.Pointer(fromlen)))
	n = int(r0)
	if e1 != 0 {
		err = errnoErr(e1)
	}
	return
}
```

```csharp
internal static (nint n, error err) recvfrom(nint fd, slice<byte> p, nint flags, ж<RawSockaddrAny> Ꮡfrom, ж<_Socklen> Ꮡfromlen) {
    nint n = default!;
    error err = default!;

    @unsafe.Pointer _p0 = default!;
    if (len(p) > 0){
        _p0 = @unsafe.Pointer.FromPinnedBox(Ꮡ(p, 0));
    } else {
        _p0 = @unsafe.Pointer.FromBox(Ꮡ_zero);
    }
    var ᴋ16 = _p0;
    var ᴋ17 = Ꮡfrom;
    var ᴋ18 = Ꮡfromlen;
        var (r0, _, e1) = syscall6(abi.FuncPCABI0(libc_recvfrom_trampoline), (uintptr)fd, (uintptr)ᴋ16, (uintptr)len(p), (uintptr)flags, (uintptr)ᴋ17, (uintptr)ᴋ18);
    System.GC.KeepAlive(ᴋ16);
    System.GC.KeepAlive(ᴋ17);
    System.GC.KeepAlive(ᴋ18);
    n = (nint)r0;
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return (n, err);
}
```

The MANAGED-callee member of the same class (measured 2026-09-05 on runtime's `TestSmhasherWindowed`): the converter bridges every value-consumed call whose RESULT is `unsafe.Pointer` with `(uintptr)` (`convCallExpr`), so `memhash32(noescape(unsafe.Pointer(&i)), seed)` hands the callee a number with the Pointer's retained source stripped, and the frame-minted box `Ꮡi` has no reference left after `FromPinnedBox` returns. A bridged `unsafe.Pointer`-returning CALL argument (not a conversion, not a builtin) over `unsafe.Pointer(&x)` of a frame-minted local — a parameter re-minted as a box included — names `Ꮡx` for a `System.GC.KeepAlive(Ꮡx)` after the statement (`bridgedWrapperKeepAliveBoxes`); a `return f(…)` hoists the call into a `ᴛN` temp so the KeepAlive sits between the call and the return:

```go
func int32Hash(i uint32, seed uintptr) uintptr {
	return memhash32(noescape(unsafe.Pointer(&i)), seed)
}
```

```csharp
internal static uintptr int32Hash(uint32 iʗp, uintptr seed) {
    ref var i = ref heap(iʗp, out var Ꮡi);

    var ᴛ2 = memhash32((uintptr)noescape(@unsafe.Pointer.FromPinnedBox(Ꮡi)), seed);
    System.GC.KeepAlive(Ꮡi);
    return ᴛ2;
}
```

Boundaries, each guarded in `syscallFunnelSet_test.go`: a BARE `f(unsafe.Pointer(&x))` argument is not bridged (the Pointer retains its box through the call) and records nothing; a package-level variable's box is a static field and records nothing; an outer CONVERSION (`n := uintptr(noescape(unsafe.Pointer(&i)))`, runtime's `stdcallN` / `mstart0` shape) records nothing, because a stored number is outside the class — a KeepAlive after the store would hold the box exactly as far as the store. The drain is per STATEMENT: a box named inside an `if` condition is kept alive by the first statement after the call (inside the body or after the `if` — a use on any path after the call keeps the local live at it); a `for` INIT/POST clause, emitted inside the C# header where no statement can follow, is refused by name (`rejectForClauseKeepAlive`), as a deferred or spawned call is (the contract is statement-scoped and a `defer` runs at unwind); a function literal converts its body against an EMPTY pending list and restores the enclosing statement's afterwards, so a box the enclosing call named before its literal argument was converted is never drained inside the lambda. `src/syscall-keepalive-census.ps1` walks the converted emission: every captured temp has exactly one KeepAlive (arm 1), hand-own pointer arguments are held across their call (arm 2), and no raw pointer-derived funnel argument remains (arm 3 — RED 104 on the pre-cut darwin emission, which had never been through the funnel path).

### An address of MANAGED storage that outlives its statement must carry a PIN

The native-address box above is exactly right when the address IS native memory — there is nothing
behind it the collector could move. It is a **dangling pointer** when the address points into managed
storage, and the reinterpret fallback produces precisely that: where `(*U)(unsafe.Pointer(p))` cannot
alias `p`'s storage in the managed model, golib names it by address instead
(`PointerExtensions.Reinterpret` → `(ж<U>)(uintptr)box`), and the `uintptr` operator's

```csharp
fixed (void* ptr = &value.Value)
    return (uintptr)ptr;
```

pins for **that statement** and no longer. The derived pointer outlives it. Once a collection moves
the storage, reads through the pointer return whatever now occupies the old address — and **writes
land in whatever now owns it**. The second is the one that matters: it is not a wrong value, it is
silent heap corruption, and the crash surfaces later, somewhere unrelated. The corpus's clearest
instance of the shape is `os_windows_test.go`'s `createMountPoint`, whose four `uint16` field stores
go through a `[]byte` scratch buffer addressed as a reparse record:

```go
byteblob := make([]byte, buflen)
buf = (*windows.MountPointReparseBuffer)(unsafe.Pointer(&byteblob[0]))
buf.SubstituteNameOffset = target.substituteName.offset   // … and three more
```

```csharp
var byteblob = new slice<byte>(buflen);
buf = Ꮡ(byteblob, 0).Reinterpret<byte, windows.MountPointReparseBuffer>();
buf.Value.SubstituteNameOffset = target.substituteName.offset;
```

**The rule: the pin's lifetime is the DERIVED POINTER's, not the address-taking statement's.** The
fallback now asks the source box for a pinned address (`ж<T>.TryPinnedReinterpret`), and the derived
box OWNS that pin — a `PinnedBuffer.PinOnly` handle held in the box's `m_pin` field and released by
its finalizer when the box is collected. This is the same field and the same idiom as the fixed-array
syscall-buffer pin (`pinnedArrayData`, above); the two uses are disjoint, since a native box never
takes the lazy one.

Only an **array/slice-element** reference can be pinned, and that is not a shortcut — it is the only
reference kind whose storage is an object the runtime can be asked to hold still. A native alias has
nothing managed behind it; a nil box has no storage; and the storage of a standard heap box and of a
struct-field reference alike is a field of a `ж<T>`, which holds delegates and a nullable tuple and so
is never blittable — `GCHandle` refuses to pin it. Those kinds keep the pre-existing address route, so
the change is strictly additive: where a pin cannot be taken, behavior is what it was, never something
newly wrong. That is also what keeps `reflect`'s prefix-downcast idiom
(`(*structType)(unsafe.Pointer(t))` over a `*abi.Type`, structurally unrepresentable and deliberately
on the address route) working unchanged — a blanket "fail loudly" was never available.

The pin is **cross-checked before it is trusted**. It is taken on the backing store
`CanonicalElement` names, which proves something only if the referent really lives inside that object,
so the address reached through the box's own value slot must be the same byte as the address of that
backing's element; a view whose `Source` is a detached copy fails the test and gets no pin.

Guarded by `tests/Behavioral/ReinterpretPinLifetime`, which is deterministic in both directions — it
writes through the derived pointer before and after enough allocation churn to move and recycle the
buffer, and reads back through both the derived pointer and the original slice. Pre-fix C# printed
`read: false true true` / `write: false false false false false false` on 5 of 5 runs where Go prints
all `true`; post-fix it matches Go on 8 of 8. Its sibling `ReinterpretPointerLifetime` guards the
other half of the same contract — the ALIASING route, for reinterprets the managed model *can*
represent.

**What this is NOT evidence of.** `os`'s test host was separately observed dying with an
`ExecutionEngineException` whose crash site moved between runs, and `createMountPoint` was the
standing suspect. A pre-fix control run of the whole `os` suite at `af5df9e16` (golib stashed back to
base, the rebuilt `golib.dll` verified to lack the fix) reproduced no such crash, and two complete
post-fix runs bracket its agreeing count from both sides, so that attribution is retracted — see the
`os` section of [`docs/phase4/BOARD-next-validation-candidates.md`](../phase4/BOARD-next-validation-candidates.md).
The pin defect is real and deterministic on its own evidence; it simply was not shown to be that
host-killer.

**Not fixed by this, and a different class:** a destination struct holding a managed reference where
Go has an inline array (`PathBuffer [1]uint16` → `array<uint16>`) still fabricates an object reference
out of whatever bytes sit at that offset when the field is read. Pinning makes those bytes the *real*
buffer's rather than recycled memory, but a fabricated reference is a CLR type-safety break either
way. That is the raw-metal-on-non-native-types fork (`os.readReparseLink`'s remedy is a hand-owned
decode; `os_windows_test.go`'s `createMountPoint` is test code that cannot be hand-owned).

## Native memory

### A reinterpreted raw address ALIASES native memory instead of boxing a copy
`(ж<T>)(uintptr)` is the reinterpret seam: it turns a raw address back into a pointer. It used to
box a **copy** of the pointed-at value —

```csharp
public static unsafe explicit operator ж<T>(uintptr value) => new ж<T>(*(T*)value.Value);
```

— which silently discarded the address. That is fine only for an immediate single read. It makes
three things impossible: pointer arithmetic (there is no address left to advance), observing writes
that native code makes afterward, and — worst — handing the pointer **back** to the OS, which then
operates on the address of a *managed box field* instead of the native block.

`syscall.Environ` does all three. It walks the `GetEnvironmentStringsW` block and frees it:

```go
envp, e := GetEnvironmentStrings()
defer FreeEnvironmentStrings(envp)
for *envp != 0 { … end = unsafe.Add(end, size) … }
```

Converted, the walk scanned the GC heap and the deferred `FreeEnvironmentStringsW` asked Windows to
free GC memory — an outright `STATUS_HEAP_CORRUPTION` (0xC0000374) process kill. `os.Environ()`
alone reproduced it, so nothing that reads the environment could run.

`ж<T>` now has a fourth reference kind alongside the standard value, struct-field and array-element
refs: a box that **aliases a native address**. `Value`/`ValueSlot` read that memory through
`Unsafe.AsRef`, `IsNull` is address-based (address 0 is the nil pointer, matching Go's
`(*T)(unsafe.Pointer(uintptr(0))) == nil`), and the `uintptr`/`void*` operators round-trip the
address **exactly** — which is what Go's `uintptr(unsafe.Pointer(p))` guarantees. `unsafe.Add`,
`unsafe.Slice` and `unsafe.String` honor the kind.

`unsafe.Add` also gained an `unsafe.Pointer` overload. Go's `unsafe.Add` is **byte** arithmetic and
its argument is always an `unsafe.Pointer`, but golib models `unsafe.Pointer` as `ж<uintptr>` whose
*value* is the address — so the generic `ж<T>` overload, which resolves a managed array-element
reference, found none and returned a **nil** pointer. Stepping through a native block dereferenced
address 0 on the very first step.

**Known limit:** `unsafe.Slice`/`unsafe.String` over a native address *snapshot* the memory into a
managed buffer rather than aliasing it the way Go does, so writes through the result do not reach the
native memory. That is sufficient for reading a block a syscall returned (the `Environ` shape) and is
where the seam still differs from Go.

### `unsafe.Slice` over MANAGED element storage ALIASES it

The snapshot above is the right answer for a native address and the wrong one for the far commoner
shape: `unsafe.Slice(&s[i], n)`, where the pointer addresses an element of a managed slice or array.
Go's result shares that storage, so writes through the rebuilt slice must land in the original
backing — and the snapshot silently swallowed every one of them. crypto/subtle is the case that
exposed it: `XORBytes` hands `xorBytes` bare pointers, which rebuilds its three slices and writes the
whole result through `dst`, so **`XORBytes` wrote nothing at all** (its test matrix compared `dst`
against its untouched `0xdd` fill).

`ж<T>` answers with the window when it has real managed element storage
(`TryGetElementWindow`): the referent is reduced through the same `CanonicalElement` mapping pointer
equality uses — so a pointer taken through a re-sliced view addresses the same absolute element Go's
would — and the result is a `slice<T>` over that backing with `len == cap == n`, exactly Go's shape.
A heap box, a struct-field ref, or a REINTERPRETING pointer (a `(*U)(unsafe.Pointer(&b[0]))` over a
differently-typed array) has no such storage and keeps the snapshot; a `T[]` view over another
element type does not exist in the managed model.

That last exclusion is why `crypto/subtle/xor_generic.cs` is **hand-owned**
(`[module: GoManualConversion]`). Its word-at-a-time loop reinterprets the byte slices as
`[]uintptr` —

```go
func words(x []byte) []uintptr {
	return unsafe.Slice((*uintptr)(unsafe.Pointer(&x[0])), uintptr(len(x))/wordSize)
}
```

— which the converted form can only snapshot, so for every length that is a multiple of 8 the XOR
went to a detached buffer and `dst` stayed untouched, while other lengths landed only their trailing
`n % 8` bytes. The hand-owned file does the same reinterpret the managed way,
`MemoryMarshal.Cast<byte, ulong>` over the slices' own spans — a genuine aliasing view, so the word
writes land in place. It keeps Go's word-at-a-time behavior (and with it the performance contract
crypto/cipher's CTR and GCM modes depend on) and drops only Go's `supportsUnaligned`/`aligned` gate,
which exists for architectures whose unaligned word loads fault. (Validated by crypto/subtle's own
suite: 7/7, no disclosures, over the full 1..1024 × 8 × 8 × 8 alignment matrix.)

---

[Index](README.md)
