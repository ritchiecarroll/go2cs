# Refusing the element-wise `[]N` widening when the callee may write the slice

> Design note for review before any code is cut (COORD, 2026-11-11, seat moved from R). Seat branch
> `claude/c2-widening-write-design`, base master `50dd9a065c`. The defect is R's BOARD row (`claude/r-board-widening`
> `b52d585d31`). Every number below names the census that measured it. The census tool is a standalone go/packages
> program that applies the converter's trigger and the predicate proposed here; it is a prototype, not converter code.

## 1. The defect

A generic parameter declared `[]N`, where `N`'s constraint is a method-set interface and `N` is reached only inside
the slice, takes the element-wise widening when it is instantiated with a pointer type that implements the
interface. The call site copies the caller's slice through the adapter (`convCallExpr.go`):

```csharp
reverse(widen<ж<item>, Named>(all, elemᴛ0 => new itemжNamed(elemᴛ0)));
```

The copy aliases each element through its box, so element method calls reach the original objects. It does not
alias the slice: a callee that writes an element, appends into the backing array, or hands the slice to something
that does, writes the copy. Go's `reverse(all)` prints `cba`; the converted program prints `abc`, with no
diagnostic. The emitting code says so itself: "a callee that reassigns `list[i]` itself would not write back".

The refusal makes this loud. The struct-proxy view (post-.4) is the path to making such writes work.

## 2. Census: every site that takes the route

The trigger is the converter's own: a generic call argument whose declared parameter type is `[]TP`, `TP`'s
constraint a method-set interface with methods, and whose instantiated element is a pointer implementing it. A
variadic parameter's slot is excluded: its arguments are elements, not the slice. Sites are counted per call
expression.

| Corpus | How measured | Sites | Read-only | Writes |
|---|---|---|---|---|
| std, tests included, linux/amd64, windows/amd64, darwin/amd64, linux/arm64 | census tool over `std` with `Tests: true`, per target | 4 per target, the same 4 | 4 | 0 |
| behavioral corpus (1035 projects) | text census of every committed golden for the widening shape, then the census tool on the one project found | 3 | 3 | 0 |
| TS closure (`microsoft/TypeScript` `tsc/` at `ba29a56299`, tests included, linux/amd64 and windows/amd64) | census tool over `./...` | 13 | 12 | 1 |

The sites:

| Site | Callee | Verdict and why |
|---|---|---|
| go/ast `walk.go` 47, 53, 65, 291 | `walkList(v, list []N)` | read-only: `list` is only ranged over |
| behavioral `GenericInterfaceConstraint` x2 | `totalArea(shapes []S)` | read-only: ranged over |
| behavioral `GenericInterfaceConstraint` | `walkAll(shapes []S)` | read-only: ranged over |
| tsc `internal/ls/lsconv/converters.go` 136, 222 | `lspRangeToVirtual`, `lspPositionToVirtual` | read-only |
| tsc `tsbaseline/contentmapper_baseline.go` 80 | `diagnosticwriter.ToDiagnostics` | read-only |
| tsc `GetErrorBaseline` x5 (fourslash, transpile_runner, error_baseline, js_emit_baseline x2) | passes `diagnostics` to `ToDiagnostics` and to `iterateErrorBaseline`, which sorts `slices.Clone(...)` of it | read-only: `slices.Clone` is `append(s[:0:0], s...)`, a zero-capacity view and a spread source |
| **tsc `internal/ls/crossproject.go` 360** | **`combineLocationArray(combined []T, ...)`** | **writes: `combined = append(combined, loc)`, and the result is returned and assigned back by the caller** |

The text census also finds 20 widening sites on the FUNC-RESULT route (crypto/internal/fips140's `sha256.New` family,
and `makeShape` in the behavioral project). A delegate is not a slice, so that route is out of scope.

## 3. The predicate: when the callee is proven read-only for the parameter

The predicate answers, for a callee function and one parameter, whether every use of that parameter in the
callee's body, closures included, is a READ of the caller's slice. Anything not proven is treated as a write.

Proven reads:
- `len(p)`, `cap(p)`, `p == nil`, `p != nil`;
- `p` as a range operand (`for i, x := range p`: assigning `x` writes nothing back);
- `p[i]` as an rvalue, including a method call or field read on the element (the element is a shared box);
- `copy(dst, p)` (the source position);
- `append(dst, p...)` (the spread source);
- a reslice `p[a:b]` used only as a range operand, a `len`/`cap` argument, a spread source, or to rebind `p`
  itself (`p = p[1:]`);
- the zero-capacity view `p[:0:0]` (both bounds the literal 0), through which nothing can reach `p`'s array;
- rebinding `p` (`p = ...`): the local changes, the caller's slice does not. Later writes through `p` are caught
  by their own rule;
- `p` passed to a STATICALLY called declared function whose receiving parameter is itself proven read-only by
  this predicate (see below).

Everything else is a write, including:
- element assignment `p[i] = x`, `p[i] op= x`, `p[i]++`, and swaps;
- `&p[i]`;
- `append(p, ...)`, `copy(p, src)`, `clear(p)`;
- `p` stored anywhere (assigned to another variable, a field, a map, a channel send), returned, or captured into
  an expression that keeps it;
- `p` passed to anything not proven: a function value, an interface method, a method value, reflection, a
  variadic `...any` position (so `fmt.Println(p)` is refused), or a declared function whose body is not
  available.

**A callee that passes the slice on.** "Proven read-only" is transitive and static. The receiving parameter of the
next callee is analysed with the same predicate, by its origin declaration, whatever its declared type is (`[]N`,
`S ~[]E`, `any`), so `slices.Reverse(p)` is refused on `Reverse`'s own element swap, and `slices.Clone(p)` is
accepted on its own body. Results are memoised per (function, parameter index). A recursive cycle adds no write
of its own, so a parameter is read-only when nothing on the cycle writes it.

**Bodies.** The callee's syntax and type info come from the package being converted or, for a foreign callee,
from `importedPackages` (the driver loads with `packages.LoadAllSyntax`, so dependencies carry syntax). A callee with no body (assembly, a linkname stub) is not proven.

**Scope of the predicate.** It is intraprocedural over each body plus the transitive static call graph above. It
does not track aliases of `p` through locals, beyond rebinding `p` itself: `y := p; y[0] = x` is refused at
`y := p`. That is conservative by design, and the census shows no site that needs more.

The prototype gives the expected answer on a fixture of 17 shapes. Read-only: walk, `first` (len, nil compare,
index read), a read-only helper, `copy(dst, p)`, `p[1:]` ranged, recursion. Refused: R's `reverse` swap, a writing
helper, `append`, `copy(p, ...)`, `slices.Reverse`, `sort.Slice`, a closure write, an alias write, a return, `&p[i]`.

## 4. Where it runs

A pre-emission gate, beside `refuseNonSelfConstraintProxies` in `conversionDriver.go`, so a refused package
writes no file, as GO2CS2002's packages do. It walks every call expression in the package, applies the trigger,
and runs the predicate on each triggered site.

The trigger is ONE function shared with `convCallExpr.go`'s emission (`elementWiseSliceWidening(callExpr, i)`),
so the gate and the emitter cannot disagree on which arguments widen. A converter test pins that every site the
emitter widens is one the gate examined.

## 5. The refusal: GO2CS2006

Agreed with R (who owns GO2CS2005 and `-warn`, on `claude/r-warn-option`): a NEW code, so the silent-copy refusal
is off `-warn`'s explicit allow-list by construction.

- `diagnosticOutput.go`: `diagnosticSliceWideningRefused = "GO2CS2006"`.
- `docs/DiagnosticCodes.md`, a row after GO2CS2005. Converter, Error.
  - Meaning: "Refused: a generic call passes a `[]*T` where the callee declares `[]N` (N constrained by a method-set
    interface). go2cs passes that argument as an element-wise copy through the adapter, and the callee may write
    the slice (the first write is named), so its writes would land on the copy, not on the caller's slice. Each
    site is named, and the run exits non-zero."
  - What to do: "Make the callee leave the slice unwritten (no element assignment, append, sort, or hand-off to
    something that writes), declare the parameter `[]*T` or over a value element type, or copy the slice
    explicitly where the write is not meant to be seen."
- Per-site text: `<callee>(...) receives <arg> as []<N>; <callee> may write it: <reason> at <file:line>`.
- `warnOption.go`: a `warnRefusedCodes` entry, with R's wording: "its downgraded sites are silent: the callee's
  writes go to an element-wise copy and the program runs on with a wrong result". `TestWarnFlagAllowsOnlyLoudCodes`
  gains its row.
- The gate ends `refusal.report(); return refusePackage(options, diagnosticSliceWideningRefused, refusal)`: fatal
  for a single package, skipped under `-recurse` with the run ending non-zero.
- The cut bases on R's `claude/r-warn-option` once R pushes it, so both hooks exist. If it must cut first, it ends
  the gate with `log.Fatalf` as master's gates do, and the two hooks are named as owed to the union.

## 6. Predictions

| Gate | Prediction | Falsifier |
|---|---|---|
| CNR | byte-identical across the behavioral corpus; no project refused (its 3 sites are read-only) | any changed file, or a GO2CS2006 line |
| Four-target corpus footprint (seeded `-stdlib` reconverts, before and after) | 0 files on linux/amd64, windows/amd64, darwin/amd64 and linux/arm64; no std package refused (go/ast's 4 sites are read-only) | any file, or go/ast refused on any target |
| std `-tests` | no std test package refused (std's 4 sites include tests) | any refusal |
| TS closure | exactly one site refused: `internal/ls` at `crossproject.go:360` (`combineImplementations` -> `combineLocationArray`). That package is refused with GO2CS2006 and skipped under `-recurse`; the other 12 sites convert as today | any other refused site, or line 360 not refused |

The TS refusal is the one visible change. What that site does today is not measured here. The widened argument's
result, a `slice<HasLocation>`, is assigned back to `combined`, a `slice<ж<LocationLink>>`, so I expect it to fail
the C# build. If so, the refusal turns a C# build error into a named converter refusal. COORD should weigh this
against the TS closure plan; the struct-proxy view is what would let it convert.

## 7. Red first, when cut

- A converter test with R's `reverse` shape, plus the fixture's write shapes: each refused with GO2CS2006, naming
  the write; `walkList`'s shape and the fixture's read-only shapes still convert.
- A predicate unit test per rule in section 3, positive and negative.
- The trigger-agreement test (section 4).
- R's repro (`reverse(all)`) converted at master shows `abc`; after the cut, conversion refuses it by name.
