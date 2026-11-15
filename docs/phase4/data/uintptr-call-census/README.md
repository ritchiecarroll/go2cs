# The typed `(uintptr)<call>` census of hand-written code (R, 2026-11-14)

**Owed by** `docs/phase4/DESIGN-unsafe-pointer-call-result.md` section 8: *every hand-written cast of a call's result
to `uintptr` where the call returns an `unsafe.Pointer`, in the hand-owned files and packages, TYPED and not textual.*
Queued to R by COORD as queue item 4. Measured at master `c2919b8b16` (batch 305), go1.26.11, on R-LAPTOP.

**Result: 0 true sites.** No hand-written file, on any of the four targets, casts the result of an
`unsafe.Pointer`-returning call to `uintptr` and lets the number convert back to an `unsafe.Pointer`. Nothing is
changed in `src/core`.

## The defect class

`unsafe.Pointer` (`go.unsafe_package.Pointer`) converts implicitly to and from `go.uintptr`. A pointer returned from a
call can CARRY the box that keeps its storage pinned. `(uintptr)f()` turns it into a number, and if that number then
converts back to an `unsafe.Pointer` the new pointer carries nothing (batch 301 removed the converter's prefix that did
this at about 600 converted sites; `runtime/mfinal.cs:315` was the one hand-owned line).

## The instrument

`analyzer/` is a census-only Roslyn analyzer (`UCC001`), injected into real package builds with
`-p:CustomAfterMicrosoftCommonTargets=census.targets`, so every site is read with the compiler's own types, per
flavour, including generated code. It reports every cast whose target type is `go.uintptr` and whose operand,
parentheses stripped, is a call whose result type is `unsafe.Pointer`. The site is classed by its consumer:

| Class | Meaning |
|---|---|
| `BACK-TO-POINTER` | the cast's value converts straight back to `unsafe.Pointer` (the defect class, if the callee can return a pointer that carries a box) |
| `POINTER-CAST` | the cast is the operand of another cast, to a typed or native pointer (the pointer-cast shape the design keeps as its own emission) |
| `NUMBER` | the cast's value is used as a `uintptr`: a correct conversion to a number |

`run-census.sh` builds `reflect`, `internal/poll`, `internal/reflectlite`, `runtime.tests` and `reflect.tests` (plus
`crypto/x509/internal/macos` on darwin) for windows/amd64, linux/amd64, darwin/amd64 and linux/arm64, each from a purged
build tree, so their closures compile with the analyzer exactly once. `report.py` writes `readings/<flavour>.tsv` and
the tallies below. A file is hand-written when a LINE begins `[module: GoManualConversion]`, when it is a `*_impl.cs`
companion, or when it is in `unsafe` or `testing`. (A substring test is wrong: the converter's placeholder comments
quote the marker, and the first pass of this census counted `runtime/export_test.cs`, a converted file, as hand-written.)

**Positive control** (`plant-control.sh`): batch 301's shape re-planted at `runtime/mfinal.cs:315` reads
`BACK-TO-POINTER`, callee `runtime.mallocgc(uintptr, ж<abi.Type>, bool)`; the file was restored byte-identical. The
`NUMBER` and `POINTER-CAST` classes are populated in every flavour, so each class has been seen to fire.

## Readings

| Flavour | Sites | Hand-written `BACK-TO-POINTER` | Hand-written `POINTER-CAST` | Hand-written `NUMBER` | Converted `BACK-TO-POINTER` |
|---|---|---|---|---|---|
| windows/amd64 | 138 | **0** | 6 | 2 | 72 |
| linux/amd64 | 69 | **0** | 6 | 2 | 26 |
| darwin/amd64 | 70 | **0** | 6 | 2 | 26 |
| linux/arm64 | 69 | **0** | 6 | 2 | 26 |

**The text census reconciled.** 264 hand-written files hold 52 textual `(uintptr)<call>(` hits (every flavour folder).
12 are typed sites above. The other 40 are casts of calls that return numbers (`len`, `sizeof`, `Marshal.SizeOf`,
identity tokens, word reads, `uvarint`), and each is in a package the census compiled. None is uncompiled.

**The hand-written sites, for the record.**

- `NUMBER`: `runtime/<os>/mem_*_impl.cs`, `sysAllocOS` and `sysReserveOS` read as numbers (two per target).
- `POINTER-CAST`, six per target:
  - `runtime/mem_persistent_impl.cs` 96, 108, 184 and 205: `(ж<notInHeap>)(uintptr)sysAlloc(...)`, native memory that
    carries no box.
  - `runtime/mprof_impl.cs:194`: `(ж<bucket>)(uintptr)(allnext.Load())`.
  - `unsafe/unsafe.cs:1452`: `(ж<array<T>>)(uintptr)Pointer.FromPinnedBox(field)`.

  These are the ruled pointer-cast shape and outside this census's class. The last two take a pointer that can carry a
  box, and they are named for whoever next reviews the pointer-cast route.

**Converted `BACK-TO-POINTER`, read for completeness.** In production files every such site's callee is
`unsafe.Pointer.FromRef`, a conversion whose pointer carries nothing (`new Pointer((uintptr)ptr)`); 13 per flavour.
No production file holds a true call with the prefix on any target, so batch 301's application reached the whole
production corpus. The rest are in committed `-tests` emissions last regenerated on 2026-11-07 (`1d164c6eb4`), before
batch 301:

| File | windows | linux, darwin, arm64 |
|---|---|---|
| `reflect/all_test.cs` | 10 | 10 |
| `reflect/set_test.cs` | 3 | 3 |
| `runtime/export_test.cs` | 4 | not read |
| `runtime/list_manual_test.cs` | 28 | not read |
| `runtime/list_test.cs` | 14 | not read |

The `-tests` pipeline re-emits these on every run, so no sweep verdict reads them. The committed copies level at the
next `-tests` regeneration; that is expected, not measured here.

## Limits

- **Test projects.** `runtime.tests` and `reflect.tests` do not build standalone outside the `-tests` pipeline (MSB3030
  on its copied Go sources; CS0123, CS0246 and CS0426 in converted test code). Their compiles still ran, and windows
  read the runtime test files; linux, darwin and arm64 did not ("not read" above). The hand-written population's 33
  test sources are all under `testing/` and hold no text hit, so the result is unaffected.
- **Scope.** `src/core` hand-written code, golib excluded: golib sits below the `unsafe` package and cannot name
  `unsafe.Pointer` as a type (it names it only inside a `DynamicDependency` string), so it cannot hold this shape.
  Packages outside the four builds' closures hold no hand-written text hit (the reconciliation above), so nothing in
  scope went uncompiled.

## Amendment 2026-11-15 (R): the marker test missed the namespace-qualified spelling

**The defect.** The file-kind test above keys on a line beginning `[module: GoManualConversion]`. 198 of the 226
whole-file hand-owns spell it `[module: go.GoManualConversion]`, so the first reading counted them as converted. It
was found while cutting the next seat, the hand-owns' pointer-cast respelling, when that seat's census missed
`runtime/mfinal.cs`, `runtime/runtime2.cs` and `net/windows/lookup_windows.cs`. `report.py` now accepts both spellings
(`\[module:\s*(go\.)?GoManualConversion\]`), and `readings/` is regenerated from the same `.ucc` files. No build was
re-run, because the analyzer's records do not depend on the file kind.

**The result does not change.** Hand-written `BACK-TO-POINTER` is still 0 on every target, so there are still 0 true
sites. The figures above that do change:

| Figure | First reading | Corrected |
|---|---|---|
| Hand-written files | 264 | 294 |
| Textual `(uintptr)<call>(` hits in them | 52 (12 typed, 40 not) | 55 (14 typed, 41 not) |
| Hand-written `POINTER-CAST` per target | 6 | 8 |
| Hand-written `NUMBER` per target | 2 | 2 |

The two added `POINTER-CAST` sites are both in the namespace-qualified hand-owns, and both take a pointer that can carry
a box: `runtime/mfinal.cs:178`, `(ж<finBlock>)(uintptr)(persistentalloc(…))`, and `runtime/runtime2.cs:364`,
`(ж<Δhchan>)(uintptr)(Δp.maybeTraceablePtr.get())`. Like the two named above, they are the pointer-cast route. They
are in scope for the respelling seat (`claude/r-handown-ptr-root`), which takes every `(ж<…>)(uintptr)(` cast in a
hand-owned file.
