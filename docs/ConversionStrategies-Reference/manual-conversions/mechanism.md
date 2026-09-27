# Manually-Converted Declarations: Mechanism

[Reference index](../README.md) · [Manually-Converted Declarations](../manual-conversions.md) · [Summary of this topic](../../ConversionStrategies.md#manually-converted-declarations)

This page covers how a declaration becomes hand-owned: the registry and its platform scope, the `/unsafe` marker, test-file companions and generated P/Invoke, and the small whole-file hand-owns.

## The hand-own mechanism

### A hand-owned file can declare that it needs `/unsafe`

`<AllowUnsafeBlocks>` is converter-generated from `usesUnsafeCode`, and `usesUnsafeCode` is an **emission** fact: it is set while visiting Go source, so it sees only C# the converter itself wrote. A hand-owned file is by definition code the converter did not write. That left a hole with no honest way through it — a package whose only need for `/unsafe` was hand-written could not express it, because the `.csproj` is regenerated on every transpile and any value set by hand is undone by the next reconvert overlay.

`[module: go.GoRequiresUnsafe]` closes it. The declaration lives in the file that HAS the requirement, and the emission unions it into the property:

```csharp
// core/time/time_impl.cs — `time`'s converted emission contains nothing unsafe at all
[module: go.GoRequiresUnsafe]

namespace go;
```

It is a **union**, and inert for the same reason the cross-platform union in `platformEmit.go` is: the property *grants* a capability rather than using one, so raising it moves no IL for code containing nothing unsafe. Together the two unions mean a `.csproj` says `true` when the converter's own emission needs it on **any** target, or when **any** hand-owned file in the package declares it.

Shape follows the [`GoManualConversion`](../manual-conversions.md#manually-converted-declarations) precedent exactly — a module-scoped attribute class in `golib`, detected by scanning the file's header text — and shares that marker's **scan** rather than adding a second one, so it inherits the same comment lexer: a `/*` that is ordinary prose inside a `//` line comment opens no block, and a marker *mentioned* in a comment is not a declaration. Both marker names live in the canonical symbol table (`src/core/go2cs/symbols.json`), because each is one string spanning a C# attribute declaration and a Go regexp and belongs in neither.

The walk is the package's own files plus its per-GOOS source folders, **never recursive**: a converted package directory can hold nested packages (`internal/runtime` holds `syscall`, `atomic`, …) whose own `.csproj` answers for them, and the discriminator is layout L3's own — a per-GOOS folder holds no project file — so `internal/syscall/windows` is not read as its parent's Windows sources. The declaration is per **package** rather than per platform because a `.csproj` is one file serving every `$(GoTargetOS)`.

Like the hand-own marker itself, this reads the OUTPUT tree, so it inherits the same prerequisite: a reconvert must be **seeded** from the committed corpus, or there is nothing on disk to declare anything. That is already the standing reconvert ritual, so it adds no new rule.

Three packages declare it today, all of them at a kernel boundary: `internal/runtime/syscall` (the Linux `Syscall6` keystone) and `time` flip from `false`, and `syscall` states a requirement its converted emission already happened to satisfy — inheriting a requirement by luck is how it disappears. See [Every P/Invoke is source-generated](#every-pinvoke-is-source-generated) for what the flag is spent on.

### Hand-owns have a platform, and it is not the same question as a folder

Two different mechanisms answer two different questions, and conflating them is what produced the Linux corpus's whole class-(b) failure surface (`docs/phase4/DESIGN-multiplatform-corpus.md` §12, increments 3.5 and 3.5b).

* **Whether a DECLARATION is hand-owned at all** is the registry's question, and until 2026-08-08 it had no platform axis: `manualConversionFuncs` was keyed by name alone. A Go name is not unique across platforms — Go selects one of several files declaring the same function by build constraint — so one entry spoke for every flavor, turning each one's declaration into a placeholder while an implementation existed only where somebody had written one.

Entries now carry a `goosScope`, whose empty value (`goosAny`) means every target and is what nearly all ~120 entries use. Scoping is load-bearing in both directions:

| Entry | Scope | Why |
|:--|:--|:--|
| runtime's `mutexContended`, `lock2`, `unlock2`, `notewakeup`, `notesleep`, `notetsleep_internal` | `goosAny` | Both flavors need hand-owning; the arity difference is the *file's* problem, not the registry's |
| `os.(*File).readdir` | windows, darwin | Those flavors hand OS memory to a Go struct (`FILE_ID_BOTH_DIR_INFO` reinterpreted; libc `readdir_r(&dirent)`). `dir_unix.go`'s is **pure Go** over `internal/poll` and converts faithfully |
| `os.readReparseLink`, `syscall`'s five generated wrappers | windows | Declared only in Go's Windows sources; already inert elsewhere, now stated rather than re-derived |

The `os.(*File).readdir` row is the one that cost something: unscoped, the entry deleted the perfectly convertible unix body too, so every Linux `os` build carried a placeholder with nothing to link against — a hand-own gap **invented by the registry rather than by the Go source**. Scoping it out is why Linux needs no `readdir` hand-own at all, which is strictly better than writing one.

What a scope deliberately does **not** express is a per-platform *signature*. The registry decides whether a declaration is hand-owned; the hand-owned file decides what it looks like. Guarded by `manualConversionScope_test.go`, whose fixture is the `lock_sema`/`lock_futex` pair at their real 4-vs-2 arity, plus a typo guard — a scope naming an unknown GOOS matches nothing, which would silently turn a hand-own off everywhere and is otherwise unreportable, since "not hand-owned" is a legitimate answer for every other declaration.

### The `*_impl_test.cs` convention

**The `*_impl_test.cs` convention** is the piece that made the export_test surface hand-ownable:
`export_test.go` hands the suite raw `Value{typ, ptr, flag}` construction over descriptor
downcasts — unbridgeable literally — so `Field`/`TField`/`Zero` are registry hand-owns whose
implementation lives in `export_impl_test.cs`, the first TEST-file companion. The `_test.cs`
suffix keeps it under the production csproj's EXISTING test-artifact exclusion (no template
change, no corpus churn); `testConversion` globs `*_impl_test.cs` into the tests project's
compile items and the conversion digest. The companion mirrors `reflect`'s hand-owned
`Value.Field`/`Zero` over `GoFields`/`FieldAliasBox`/`ZeroValueOf`, and `TField` hands the
LITERAL `StructFieldType` the SYNTHESIZED `abi.StructType()` — the specialization section below —
so the type-side and value-side walks read one projection.

### Every P/Invoke is source-generated

Every native binding in the converted corpus — all fifteen, across five hand-owned files and two operating systems — is `[LibraryImport]`. There is no `[DllImport]` left, and a new one is a mistake rather than a style choice.

The reason is one property of the two attributes and nothing else. `[DllImport]` answers a signature it cannot marshal by marshalling something *else*: a non-blittable struct becomes a temporary copy, and a kernel that writes through the pointer writes into a temporary the caller never reads. That is a wrong **answer**, produced silently, at run time. The source generator refuses to emit the call at all, so the same mistake is a compile error with a line number.

That distinction is not hypothetical here. It is the exact defect `zsyscall_windows_impl.cs` exists to repair — three times over, at 172, 592 and 568 bytes — where the converted `Timezoneinformation`, `win32finddata1` and `ProcessEntry32` hold their inline `WCHAR[]` buffers as managed `array<uint16>` references and the kernel wrote native records over smaller managed objects. The remedy each time was an explicitly blittable mirror plus a pointer, and until now that discipline was enforced only by a reviewer noticing. It is now enforced by the compiler, which is the whole return on the migration: the residual risk of routing Go's kernel boundary through managed structs is per-struct **layout**, and this makes layout a build-time question.

**What the property costs, and the mechanism that pays it.** `SYSLIB1062` requires `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` unconditionally — even for an all-`nint` signature — because the generated stub is written in terms of pointers. That property is converter-generated from `usesUnsafeCode`, an *emission* fact that observes only C# the converter wrote, so a hand-owned file was structurally unable to ask for it: setting it by hand is undone by the next reconvert overlay. [`[module: go.GoRequiresUnsafe]`](#a-hand-owned-file-can-declare-that-it-needs-unsafe) is that mechanism, and `time` is the package where it is genuinely load-bearing rather than merely honest — its converted emission contains nothing unsafe at all.

**The rejection census.** Twelve of the fifteen declarations the generator accepted **unchanged**, which is itself the finding: `exec_windows.cs`'s five and `zsyscall_windows_impl.cs`'s five were already all-pointer, all-blittable, because both files were written under exactly the discipline the generator checks. Three needed a different signature, and each rejection was a latent hazard rather than a formality:

| Declaration | Rejected for | Became |
|---|---|---|
| `LoadLibraryExW` (`dll_windows.cs`) | `CharSet` has no `[LibraryImport]` equivalent | `StringMarshalling = StringMarshalling.Utf16` — and *not* merely equivalent: UTF-16 marshalling of a `string` is a **pin**, so the stub hands Windows the string's own storage instead of a copy |
| `GetProcAddress` (`dll_windows.cs`) | `CharSet.Ansi` + `BestFitMapping=false` + `ThrowOnUnmappableChar=true`; the latter two are unsupported outright | `byte*`. The mechanical translation (`StringMarshalling.Custom` over `AnsiStringMarshaller`) would have kept the transcode while silently dropping the guard that made it safe — an unmappable rune stops throwing and becomes `'?'`, i.e. a lookup of a *different symbol*. There was never a transcode to preserve: Go passes this entry point a `*byte` with no codepage step anywhere, and the caller already holds that NUL-terminated buffer. The old form decoded it through the ANSI codepage and the marshaller re-encoded it back, lossy in both directions |
| `SetWaitableTimer` (`time_impl.cs`) | `SafeWaitHandle`, and `bool` in both directions | The handle parameter becomes `nint` with `DangerousAddRef`/`DangerousRelease` taken **visibly** in the caller — SafeHandle marshalling is a runtime service, and `[DllImport]` supplied the reference count invisibly, so nothing in the source said where it was taken. `bool` becomes `[MarshalAs(UnmanagedType.Bool)]` on the return and the parameter: its native width is a marshalling decision (Win32 `BOOL` is four bytes, C++ `bool` is one), the runtime marshaller's silent default happened to be right, and the generator refuses to guess. `UnmanagedType.Bool` *is* the four-byte `BOOL`, so the ABI is unchanged and the choice is now written down |

**One converter-side change was required, and it is the interesting one.** `go2cs-gen`'s `PartialStubGenerator` fills every bodyless `partial` method with a throwing stub, which is how the converter emits Go's asm/cgo functions. A `[LibraryImport]` declaration is also a bodyless `partial` method — and **source generators cannot observe each other's output**, so `PartialImplementationPart` is null from there and the declaration looks exactly like an unimplemented asm function. Stubbing it produced two implementing parts and failed the whole package with `CS0757`, for all twelve P/Invokes at once, and only once a hand-own adopted the form. The generator now skips any partial declaration carrying an attribute that obliges a *different* generator to implement it; the test is on the attribute, because the attribute is the obligation. A second such attribute (`JSImport`, `GeneratedComInterface`, …) is added to that set rather than worked around at the call site.

Guarded by the `LibraryImportPartial` behavioral test, which is deliberately a *compile* assertion — both failure modes (`SYSLIB1062` from a missing declaration, `CS0757` from an over-eager stub) are compile failures — carrying a hand-owned `[LibraryImport]` of exactly the corpus's shape, plus a module initializer that calls it, so a program that prints at all is one whose generated marshalling stub reached the kernel.

## Whole-file hand-owns

### `crypto/subtle`'s word-at-a-time XOR

**`crypto/subtle`'s word-at-a-time XOR (`core/crypto/subtle/xor_generic.cs`, whole-file).** `xorBytes` XORs a machine WORD at a time by reinterpreting its three byte slices as `[]uintptr` (`unsafe.Slice((*uintptr)(unsafe.Pointer(&x[0])), len(x)/wordSize)`). A `uintptr[]` view over a `byte[]` does not exist in the managed model — golib's `slice<T>` is a window on a real `T[]` — so the converted `words()` could only SNAPSHOT the bytes into a detached `slice<uintptr>`, and the word loop XORed the snapshot and dropped it: for every length that is a multiple of 8, `XORBytes` wrote **nothing**. The whole file is hand-owned (marked `[module: GoManualConversion]`) and does the same reinterpret the managed way, `MemoryMarshal.Cast<byte, ulong>` over the slices' own spans — a genuine aliasing view, so the word writes land in place — keeping Go's word-at-a-time behavior and the performance contract crypto/cipher's CTR and GCM modes depend on. Only Go's `supportsUnaligned`/`aligned` gate is dropped (it exists for architectures whose unaligned word loads fault). Full detail: *`unsafe.Slice` over MANAGED element storage ALIASES it*. Guarded by crypto/subtle's own suite (7/7, no disclosures, over the full 1..1024 x 8 x 8 x 8 alignment matrix).

### `sync/atomic.Value`

**`sync/atomic.Value` (`core/sync/atomic/value.cs`, whole-file).** Go's `atomic.Value` stores and loads an `any` atomically by reinterpreting the interface's internal two-word `(type, data)` layout: `(*efaceWords)(unsafe.Pointer(&v))`, then `atomic.LoadPointer`/`StorePointer`/`CompareAndSwapPointer` on the `typ` and `data` slots, with a `firstStoreInProgress` sentinel guarding the first store. That layout is a Go runtime detail with **no managed equivalent** — an `any` here is a single `System.Object` reference (one word), and reinterpreting a managed reference as a raw address to poke type/data words simply NREs (the same managed-referent-through-`unsafe.Pointer` wall as the guintptr family). The first *operational* hit was `internal/testlog`'s package-level `var logger atomic.Value`, loaded during `os.Getenv` — so `atomic.Value.Load()` NRE'd on the zero value before any store. The whole file is hand-rewritten (marked `[module: GoManualConversion]`) to store the `any` **directly** in the `Value.v` field and use `Volatile.Read`/`Interlocked.CompareExchange` for the acquire/release ordering and CAS the literal conversion cannot provide; the nil-store and inconsistent-type panics, and `CompareAndSwap`'s by-value comparison (`AreEqual`, matching Go's `i != old`), preserve the spec. Guarded by the `AtomicValue` behavioral test (Load-nil / Store / Swap / CompareAndSwap over typed string values, output-compared vs Go).

### `unique.clone` — a raw-offset string walk the managed model cannot express, hand-owned after the `@string` window made it GC-fatal

Go's `unique.clone[T]` rewrites every string field of a just-interned value in place, addressing each
by raw ABI offset so an interned handle never keeps a large parent string alive:

```go
func clone[T comparable](value T, seq *cloneSeq) T {
	for _, offset := range seq.stringOffsets {
		ps := (*string)(unsafe.Pointer(uintptr(unsafe.Pointer(&value)) + offset))
		*ps = stringslite.Clone(*ps)
	}
	return value
}
```

The converted form — `(ж<@string>)(uintptr)((uintptr)Ꮡvalue + offset)` followed by a `.Value` write —
adds a **Go ABI** offset to the transient interior address of the movable `ж<T>` heap box, whose CLR
field layout is unrelated to Go's ABI (`EnsureStableAddress` cannot pin a box whose `T` contains
references, and for `[2]struct{a string}` the +16 offset is outside the 8-byte `array<T>` reference
that is the entire CLR value). Every such store landed on the box's OWN fields. While `@string` was a
single 8-byte reference the damage was a type-confused slot holding a valid object — silently wrong
values, nothing the collector trips over. When `@string` became an offset/length **window**
(`fc6d8c179`, r57c — 16 bytes: `byte[]` + two `int`s), the same store's integer tail began landing in
an adjacent GC-scanned reference slot, and the next collection — which `unique`'s own `drainMaps`
forces via `runtime.GC()` — walked a garbage pointer and fail-fasted the process with
`COR_E_EXECUTIONENGINE` (0x80131506). Bisected, and reproduced in ~25 lines against golib alone, by
the 2026-08-12 unique-bisect lane (the board's scout-batch-1 `unique` entry holds the full record).

`src/core/unique/clone.cs` therefore carries `[module: go.GoManualConversion]` — the standard S1
managed-referent remedy — with **only `clone<T>` departing from the conversion**. Its contract is
"makes a copy of value, and MAY update string values found in value with a cloned version": the
cloning is a retention optimization, never a semantic requirement, so the hand-own does the
`T == string` case exactly (a right-sized `stringslite.Clone`, no address arithmetic — worth more,
not less, now that `@string` windows share backing) and returns aggregate values unchanged. The one
observable divergence from Go is retention: an interned aggregate's strings keep sharing their
original backing arrays. Equality, identity and intern-map drainage — what `unique.Make` is *for* —
are unaffected. `makeCloneSeq` and the `cloneSeq` builders remain the verbatim conversion (pure
descriptor arithmetic, still validated by `TestMakeCloneSeq`), so a `clone.cs.auto` review sibling is
emitted on every reconvert as usual.

### `internal/cpu.getGOAMD64level` — a BUILD constant, so the honest answer is the baseline

Go declares `getGOAMD64level() int32` bodyless and implements it in `cpu_x86.s`, where it is not code at
all but a compile-time constant selected by the `GOAMD64_vN` define the toolchain sets from
`go env GOAMD64`:

```asm
TEXT ·getGOAMD64level(SB),NOSPLIT,$0-4
#ifdef GOAMD64_v4
	MOVL $4, ret+0(FP)
#else
#ifdef GOAMD64_v3
	MOVL $3, ret+0(FP)
#else
#ifdef GOAMD64_v2
	MOVL $2, ret+0(FP)
#else
	MOVL $1, ret+0(FP)
#endif
```

The question it answers is *which amd64 microarchitecture level was this BINARY built for*, never *which
level does this CPU support* — the two differ constantly, and Go depends on the difference: `doinit`'s
option table gates the `sse3`/`avx`/`avx512` GODEBUG knobs on `level < 2/3/4`, so a v1 build running on a
v3 machine keeps them switchable. go2cs emits portable C# with no GOAMD64 define, no
microarchitecture-gated emission and no instruction-set floor above the amd64 baseline, so the faithful
answer is the same constant Go's own assembly produces for a build without a `GOAMD64_vN` define: **1**.
That is a measured property of the emission rather than a placeholder value, and probing the host through
`System.Runtime.Intrinsics.X86` would answer a *different question* — the inverse-atomic rule's exact
prohibition, since the returned number would look truthful while meaning something else.

`cpu_x86_impl.cs` returns it under `[module: go.GoManualConversion]`, registered as
`manualConversionFuncs["internal/cpu"]["getGOAMD64level"]`, so the converter leaves the standard
placeholder comment where the bodyless partial was. A/B footprint: **one corpus file**. Demonstrated
consumer: `internal/cpu`'s own `TestDisableSSE3`, whose first statement is
`if GetGOAMD64level() > 1 { t.Skip(…) }` — against the unimplemented `PartialStubGenerator` stub that
guard was an infrastructure-error, and it was the package's only divergence (7 of 8). With the constant in
place the test reads 1, walks on into `runDebugOptionsTest`, and skips exactly where Go does:
`internal/cpu` validates **8 of 8**.

### Realizing an asm-backed arch layer with managed hardware intrinsics

Hand-owning an asm-backed declaration does not have to mean stubbing it. Where .NET exposes the *same* instructions the `.s` file issues — via `System.Runtime.Intrinsics` — the architecture layer can be ported for real, and the converted package gains genuine hardware acceleration rather than a fallback. **`hash/crc32` is the first of its kind (2026-07-24) and sets the pattern.**

Go's `crc32_amd64.go` declares three functions with no body — `castagnoliSSE42`, `castagnoliSSE42Triple` (the SSE4.2 `CRC32` instruction) and `ieeeCLMUL` (PCLMULQDQ carry-less multiply folding) — implemented in `crc32_amd64.s`. Converted literally they become bodyless `partial`s that the [`PartialStubGenerator`](../source-generators.md#source-generators) fills with `NotImplementedException`, so `crc32.go` always took the slicing-by-8 fallback and the package's own `TestArchIEEE`/`TestArchCastagnoli` **skipped**. `crc32_amd64.cs` is hand-owned (`[module: go.GoManualConversion]`, whole-file) and the three functions are transcribed against `System.Runtime.Intrinsics.X86` — `Sse42.X64.Crc32` for the CRC32B/W/L/Q chain, `Pclmulqdq.CarrylessMultiply` + `Sse2` for the fold and Barrett reduction, `Sse41.Extract` for the final `PEXTRD`. Every other declaration in the file is the converted output verbatim.

Three rules make this a repeatable recipe rather than a one-off:

* **Probe capabilities LOCALLY; never flip `internal/cpu`'s global flags.** The Go guards read `cpu.X86.HasSSE42` / `HasPCLMULQDQ` / `HasSSE41`. Those flags are shared by **every** converted package's arch path, and the rest of those arch layers are still throwing stubs — setting them centrally would trade each package's working portable fallback for a `NotImplementedException`. The hand-owned file instead defines its own predicates over the `.IsSupported` properties of exactly the instruction sets it uses (`Sse42.X64.IsSupported`; `Pclmulqdq.IsSupported && Sse41.IsSupported`) and `archAvailable*` returns those. The claim then stays precisely true — "these instructions are available to *this* code" — and `internal/cpu` is untouched. When a probe is false, `crc32.go`'s own `archAvailable*` branch falls back to slicing-by-8, which is exactly how Go degrades on an architecture with no arch implementation.
* **Port the `.s` file, label for label, and say so.** The fold constants are the `.s` `DATA` pairs verbatim; a 128-bit load puts the offset-0 quadword in the low half, so `Vector128.Create(offset0, offset8)` reproduces each register image exactly. `PCLMULQDQ`'s `imm8` maps directly onto `CarrylessMultiply`'s `control` (bit 0 selects the *left* operand's quadword, bit 4 the *right*'s), so `PCLMULQDQ $0x11, X0, X1` is `CarrylessMultiply(x1, x0, 0x11)`. Each ported block carries the `.s` label it transcribes (`aligned`, `less_than_8`, `loopback64`, `remain64`, `remain16`, `finish`) so the two can be diffed by eye at upgrade time.
* **Divergences are deliberate, documented in place, and result-neutral.** The `.s` file walks the buffer to an 8-byte boundary before its `CRC32Q` loop; the managed port drops that step. It exists to align the loads, it cannot change the answer (CRC is a pure function of the initial value and the byte *sequence*, and the `CRC(I, ABC)` combining identity holds for any split), and it has no managed counterpart — a slice's backing array can be moved by the GC, so an address observed here is not an address the loads keep. Likewise the `.s` triple loop is a `DECQ`/`JNZ` do-while that would wrap on `rounds == 0`; the managed form is a counted loop, identical for every real input.

Note the csproj constraint this recipe was written under: the converter **regenerates each package's `.csproj` on every transpile**, and it sets `AllowUnsafeBlocks` from `usesUnsafeCode` alone -- which was `false` here, so a hand-owned file could not use `byte*`/`fixed`/`stackalloc` and any setting added by hand was clobbered on the next run. That is no longer the only option (a file may now [declare the requirement](#a-hand-owned-file-can-declare-that-it-needs-unsafe)), but this recipe is still the better answer where it applies: it needs no compiler flag at all. The loads go through `MemoryMarshal.GetReference(p.ToSpan())` plus `Unsafe.ReadUnaligned<T>` / `Unsafe.Add`, which need no compiler flag and read the slice's real backing window (offset included) with no copy.

The payoff is that the package's *own* test suite becomes the correctness oracle, which is what makes this pattern safe to repeat: `TestArchIEEE` and `TestArchCastagnoli` cross-check the intrinsics against the portable slicing-by-8 implementation over randomized buffers at 46 lengths chosen to straddle the `168*3=504` and `1344*3=4032` cutoffs, and enabling the arch path also routes `TestGolden`/`TestGoldenMarshal` through it against known vectors. All 8 `hash/crc32` Test functions match `go test`, with nothing skipped and no disclosed divergences (Phase-4 validated package; see `docs/Roadmap.md`). Confirmed by positive control — corrupting a fold constant and the `CRC32B` tail turns exactly the arch-dependent tests red while the portable-only ones stay green.

---

[← Manually-Converted Declarations](../manual-conversions.md) · [Index](../README.md)
