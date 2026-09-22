# CENSUS — `reflect`'s five divergences at go1.24.13, classed

**Point-in-time record, 2026-09-22, lane C1 (cloud/linux).** A READING. No cut, no gate, no row, no
.NET leg. Evidence at `claude/i9-h10-s1-evidence` **`6dbe347c07`**:
`docs/phase4/h10-evidence/i9-s1/reflect/diverged.tsv` (blob `425a7823c9`), `results-tail.txt`
(`4e9b3f8787`), `timing-s1.tsv` (`342a7caaab`). Tree `c6fdbe73c3`.

## Two corrections to the framing, before the table

**1. `reflect` is NOT a banked row.** The roster carries **203** rows and `reflect` is not one of them
(`docs/ValidatedTestPackages.md`: zero `core/reflect` links; only `internal/reflectlite`, whose own
manifest line at :32 says it is gone). There is no `docs/validation/current/reflect.md`. The 62-pin
manifest `src/core/reflect/go2cs_test_disclosures.json` is real and tracked (42 `alloc-profile`, 19
`runtime-capability`, 1 `codegen-liveness`), but it belongs to a CANDIDATE, not to a banked row. So
"an assertion the banked 1.23.12 row never had" has no banked verdict set to test against, and was
instead tested against the 1.23.12 **source**, below.

**2. Four of the five are new at 1.24.** Materialised `go1.23.12` and looked for each declaration:

| test | at 1.23.12 |
|:--|:--|
| `TestGroupSizeZero` | **ABSENT** (`reflect/map_swiss_test.go:20` at 1.24, under `//go:build goexperiment.swissmap`) |
| `TestMapOfKeyPanic` | **ABSENT** |
| `TestMapOfKeyUpdate` | **ABSENT** |
| `TestTypeFieldReadOnly` | **ABSENT** |
| `TestIsZero` | PRESENT — and its body is **BYTE-IDENTICAL** at both releases (142 lines, empty diff) |

None of the five is pinned in the existing manifest (checked by name and by `name/` subtest prefix).

**The output evidence is a WINDOW, and four of five are not in it.** `results-tail.txt` is 32,200
bytes and carries a run/fail record for `TestTypeFieldReadOnly` only. The other four have **no output
line in the supplied tail**. Their classes below are derived from the Go source and the emission; each
one says what it still needs.

## The sibling question: NOT G's `*SwissMapType` root

`internal/runtime/maps`' routed class — a dereference of a managed pointer with no address, reached
when `abi.TypeOf`'s synthetic descriptor meets the auto `MapType()` — **is not on any of these three
paths.** Measured: `src/core/reflect/*.cs` mentions `MapType`/`SwissMapType` exactly twice, a
`global using mapType = …abi_package.SwissMapType` (`map_swiss.cs:5`) and `mapIterStart`
(`map_swiss.cs:70`); `reflect` never calls the `abi.Type.MapType()` extension. `TestGroupSizeZero`
goes through `reflect.StructOf`/`ArrayOf`, and the `TestMapOfKey*` pair through reflect's
hand-owned managed map in `value_impl.cs`. **G's seat will not move any of the three.**

## The table

| test | class | pin |
|:--|:--|:--|
| `TestTypeFieldReadOnly` | **1.24-new + STRUCTURAL** — proof below | **1 pin, authored** |
| `TestGroupSizeZero` | **1.24-new**, and on the source a **converter/golib defect** at `reflect.StructOf`'s size rule — confirmation owed | NONE yet |
| `TestMapOfKeyPanic` | **1.24-new + converter/golib defect**, site named — confirmation owed | NONE yet |
| `TestMapOfKeyUpdate` | **1.24-new + converter/golib defect**, site named — confirmation owed | NONE yet |
| `TestIsZero` | **NOT 1.24-new; arm unnamed** — a reading owed, no label | NONE |

### `TestTypeFieldReadOnly` — structural, and Go says so itself

`all_test.go:6910`. `typ.Field(0)` then, under `debug.SetPanicOnFault(true)`,
`shouldPanic("", func(){ f.Index[0] = 1 })`. The C# side, from the tail: **`panic: did not panic`** —
the write to `f.Index[0]` SUCCEEDED where Go faults.

Go's own comment states the mechanism: *"Right now StructField.Index is read-only; that saves
allocations"* — `Index` aliases the type's read-only data, so the store traps, and
`SetPanicOnFault` converts the trap into a recoverable panic. In the managed model `Index` is a
slice over an ordinary writable array; the CLR offers no page protection at that granularity and no
`SetPanicOnFault` counterpart that would turn a managed store into a fault.

The strongest part of the proof is Go's own skip: the test begins by skipping `js` and `wasip1`
**"because we don't use the optimization for js or wasip1"**. Go already treats the assertion as
vacuous wherever `Index` is not read-only backing. The corpus is in exactly that position, so this is
a roster disposition and not an implementation task.

**Pin to author:** `TestTypeFieldReadOnly`, class **`runtime-capability`** (the manifest's existing
class for 19 of its 62 pins — page protection and fault-to-panic are a host capability, not an
allocation meter), signature from the observed failure text (`did not panic`).

### `TestGroupSizeZero` — 1.24-new; the size rule, not the map descriptor

`reflect/map_swiss_test.go:20`, a file that exists only at 1.24 and only under
`goexperiment.swissmap`. `MapGroupOf` is an INTERNAL test export (`export_swiss_test.go:9`) wrapping
`groupAndSlotOf`, and the assertion is `grp.Size() > 8` for a `struct{}` key and elem — Go reserves a
trailing word so a pointer to the zero-size slot at the end of a group stays inside the allocation
(the test's own comment says exactly that).

`groupAndSlotOf` **converts faithfully**: `src/core/reflect/map_swiss.cs:24-60` mirrors Go line for
line, `group = StructOf({Ctrl uint64, Slots [SwissMapGroupSlots]slot})`. So the number under test
comes from `reflect.StructOf`'s own size computation, and the reading is that the managed `StructOf`
does not apply Go's trailing-zero-size padding rule, leaving `Size()` at 8. That is implementable —
a size the descriptor controls — so **defect, not structural**, and the site is `StructOf`'s layout
computation rather than anything in the swiss-map descriptor.

**Owed:** the output line. The prediction it should confirm, stated before reading it, is
`Group size got 8 want >8`. Any other text refutes this reading and the class is re-derived.

### `TestMapOfKeyPanic` — 1.24-new; the unhashable-key panic is missing

`all_test.go:8671`. `MakeMap(MapOf(TypeFor[any](), TypeFor[bool]()))` then `m.MapIndex(ValueOf(slice))`
with a nil `[]int`, inside a `recover()` that errors with `didn't panic` if nothing panicked. Go's own
comment: the point is that the flag is set *"even if the map is empty"*.

reflect's `Value.MapIndex` is a `[module: GoManualConversion]` hand-own with **managed** semantics
(`src/core/reflect/value_impl.cs`; the emitted `map_swiss.cs` carries only the placeholder). A managed
dictionary lookup of an unhashable Go value does not raise Go's runtime error — it simply misses and
returns the zero `Value`. **Site:** the hand-owned lookup's missing unhashable-key panic.

### `TestMapOfKeyUpdate` — 1.24-new; last-write-key-wins

`all_test.go:8644`. Sets `m[+0.0]` then `m[-0.0]`, asserts `Len() == 1` **and** that the surviving key
is NEGATIVE (`math.Copysign(1.0, k) > 0` is the error). So the assertion is that an overwrite with an
equal-but-distinct key REPLACES the stored key.

`Len() == 1` should hold in the managed model (.NET `Double` equality and hash agree for ±0.0), but a
managed dictionary's indexed set keeps the **first** key on overwrite. **Site:** the hand-owned
`SetMapIndex` (`value_impl.cs:1727`) / the golib map write path. **Owed:** the output line, to confirm
the failure is the sign assertion (`map key 0.000000 has positive sign`) and not the length.

### `TestIsZero` — the one that is not new, and the one I will not label

Present at 1.23.12 with a byte-identical 142-line body, so neither new surface nor a changed
assertion. Its table drives `setField(…)`, which writes through `unsafe.Sizeof`-derived byte offsets
into structs whose other fields are BLANK (`struct{ _, a, _ uintptr }`, `…func()`, `[256]S`), plus
bare `unsafe.Pointer` cases — a layout-and-offset shape, which is where a managed struct differs most.

**No label.** The failing arm is not named in the supplied window, and the label ladder's fourth
outcome applies: none of the three classes is established, so a reading is owed and no label at all
is taken. It is also NOT callable a regression: `reflect` never banked a row, so no recorded 1.23.12
verdict exists for it, and "the C# side used to pass this" is unevidenced. **Owed:** the output line,
which names the arm index and the kind.

## What this record does not claim

One pin authored, three defects sited but unconfirmed, one test unlabelled. No compile, no BUILD, no
row, no gate — every claim is a read of the two GOROOTs, of the emission at `c6fdbe73c3`, of the
tracked manifest, or of the three evidence blobs. The Windows build path in the tail's stack trace is
deliberately not reproduced here.
