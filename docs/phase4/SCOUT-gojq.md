# SCOUT — gojq v0.12.19: its import closure, conversion, build and tests, measured

> **State: MEASURED (2026-11-07), lane P1, for COORD.** Measurement only: nothing here changes the converter, golib,
> the generator or `src/core`. Box: linux/amd64, 4 cores, go1.26.10, .NET SDK 10.0.403. Converter and standard
> library from master `c6cddb6903`. Plain `-recurse` (local references), the `safe` build tag (the default for a
> module). The owner authorized downloading, building and running gojq and the dependencies its `go.mod` pins,
> on 2026-11-07, in the P1 thread.

## 1. Answer

- **Conversion:** rc 0. All 11 production packages convert, none skipped, in 40.5 s.
- **Build:** 5 of the 11 packages compile. Two fail with 10 errors in total, and 4 are blocked behind them. The 10
  errors come from **3 converter defects**. With those three sites patched by hand in a scratch copy, the whole
  module compiles with 0 errors. The converted `gojq` program then gave the same output as Go on all 7 queries
  tried.
- **Tests:** `-tests` DOES combine with plain `-recurse` at master (`moduleTestsDriver.go`, documented in
  `docs/ConvertingAModule.md`, "Validate a converted module's tests"). That run adds a fourth defect: the converter
  panics on `parser.go`, but only in the test pass. With all four patched in scratch:

  | package | Go verdicts | compared | matched | C# fails |
  |:--|--:|--:|--:|--:|
  | library root | 176 | 164 | **162** | 2 |
  | `cli` | 914 | 914 | **898** | 16 |

  The library's 12 example verdicts are left out of the comparison (benchmarks, examples and fuzz targets wait for
  Phase 4D). The `cli` count needs `-test-timeout 50m`: under the default 2-minute deadline the run was cut off
  after 532 verdicts.
- **Seats:** 4 converter seats, 1 golib seat and 1 performance seat. The library root validates after 4 of them
  (§6).

## 2. Pins and closure

gojq **v0.12.19** is the Roadmap's pin; the latest tag is **v0.12.20**. timefmt-go **v0.1.8** is the pin; the latest
is **v0.1.9**. gojq's `go.mod` says `go 1.24.0`. Every module came through `proxy.golang.org`, and `go mod verify`
reports "all modules verified" against the module's own `go.sum`, unchanged. `go build ./...` passes.

Production packages, with `-tags safe` (Go files and lines, test files excluded):

| package | module | files | lines | reached by |
|:--|:--|--:|--:|:--|
| `github.com/itchyny/gojq` (library root) | gojq | 23 | 9,446 | — |
| `github.com/itchyny/timefmt-go` | v0.1.8 | 3 | 938 | library root |
| `github.com/itchyny/gojq/cli` | gojq | 9 | 1,782 | `cmd/gojq` |
| `github.com/itchyny/go-yaml` | v0.0.0-20251001235044-fca9a0999f15 | 13 | 11,590 | cli |
| `github.com/mattn/go-isatty` | v0.0.20 | 2 | 22 | cli |
| `golang.org/x/sys/unix` | v0.38.0 | 41 | 20,508 | go-isatty |
| `github.com/mattn/go-runewidth` | v0.0.19 | 3 | 892 | cli |
| `github.com/clipperhouse/uax29/v2/graphemes` | v2.3.0 | 4 | 1,639 | go-runewidth |
| `github.com/clipperhouse/uax29/v2/internal/iterators` | v2.3.0 | 1 | 100 | graphemes |
| `github.com/clipperhouse/stringish` | v0.1.1 | 1 | 5 | uax29 |
| `github.com/itchyny/gojq/cmd/gojq` | gojq | 1 | 12 | — |

- **The library root reaches one third-party package, timefmt-go.** Everything else comes in through `cli`.
- **Standard library.** The library root reaches 79 standard packages, internals included. Its direct imports are
  `bytes cmp context encoding/base64 encoding/json errors fmt io iter maps math math/big net/url os path/filepath
  reflect regexp slices sort strconv strings sync time unicode unicode/utf8`. `cli` adds `bufio` and
  `encoding/binary`, 81 in all.
- **Test-only.** `cli`'s tests import `github.com/google/go-cmp/cmp` v0.7.0 and its 4 internal packages. The library
  root's 13 test files are all external (`package gojq_test`) and import nothing outside the standard library. Two
  of them use `unsafe` (§5).
- **Go baseline**, `go test -tags safe -json -count=1`: library root 176 verdicts pass (63 top-level, 0.22 s);
  `cli` 914 pass (4 top-level, 1.19 s).

## 3. Conversion

`go2cs -recurse -go2cspath <master>/src <gojq> <scratch>` returned rc 0: "11/11 packages converted" in 40.5 s.
go-yaml took the longest (19.7 s). The 3 warnings are all expected: the unspecified package license, one
`move2heap` note, and one `unsafe.Sizeof` constant in x/sys/unix.

## 4. Build, classified

`dotnet build <project> -c Debug -p:GoTargetOS=linux`, first for the library root, then for `cmd/gojq`:

| result | packages |
|:--|:--|
| compile | timefmt-go, go-yaml, x/sys/unix, go-isatty, stringish |
| fail | uax29 `internal/iterators` (8 errors), library root (2 errors) |
| blocked | uax29 `graphemes`, go-runewidth, `cli`, `cmd/gojq` |

Error codes: CS0314 ×5, CS1929 ×3, CS0103 ×1, CS0246 ×1. Here they are classified by the file each error is
declared in and by the converter mechanism behind it:

| # | site | errors | mechanism | remedy | size | owner |
|:--|:--|--:|:--|:--|:--|:--|
| A | uax29 `internal/iterators/iterator.cs` (graphemes has the same constraint at 3 more sites) | 8 | A type parameter constrained by `stringish.Interface`, a named interface `~[]byte \| ~string`, gets only `new()`. `len(T)` and `T[i:j]` then have nothing to bind to. `constraintOperations.go` maps a string-or-bytes constraint to `IByteSeq<T, byte>` by its TEXT, which must be exactly `string\|[]byte` or `[]byte\|string`. Tilde terms and a named constraint both miss. | Decide from the type set: the core type of every term is `string` or `[]byte` | small | converter |
| B | `compiler.cs:380` (`compileFuncDef`) | 1 | `defer c.lazy(func() *code { … scope … })()`: the closure capture inside a deferred call's function expression emits `var scopeʗ2 = scopeʗ1;` and never declares `scopeʗ1`. Reproduced in 8 lines: `scope := x; defer lazy(func() int { return scope.id })()`. `f := lazy(…); defer f()` and `defer func(){…}()` emit correctly. | Emit the first copy from the variable itself | small | converter |
| C | `iter.cs:21` (`NewIter[T]`) | 1 | `iter := sliceIter[T](values); return &iter`. This is a pointer adapter for a generic type, instantiated at a TYPE PARAMETER. The call site names `sliceIterжIter<T>`, but no `GoImplement` record is written for it (an open generic cannot appear in an assembly attribute), so no adapter is generated. The behavioral corpus has only closed instantiations (`GenericValueInterfaceImpl`: `G<@string>`, `G<nint>`). | Record an open-generic implement (for example by `typeof(sliceIter<>)`) and generate a generic adapter | medium | converter + generator |

None of these is in x/sys/unix, go-yaml or timefmt-go.

**Scratch probe.** In a copy of the output I patched three things by hand:
- the `IByteSeq<T, byte>` constraint at the uax29 sites;
- the copy line;
- an 8-line `sliceIterжIter<T>` adapter.

With those, `cmd/gojq` and its whole closure build: rc 0, 0 errors. The converted program matched `go run ./cmd/gojq`
on all 7 queries tried, among them `path(..)`, `[limit(3; range(10))]`, `now | type` and `@base64d? // "x"`.

## 5. Tests

`go2cs -tests -recurse -test-action all` reports the same three build defects, plus one more:

| # | site | mechanism | remedy | size | owner |
|:--|:--|:--|:--|:--|:--|
| D | `parser.go` in the test pass | The converter panics with `slice bounds out of range [:-1]` in `trailingCommentRuns` (`trailingCommentAlignment.go:101`). It is recovered as "visit file error", so `parser.go`'s package-info records are lost: the `yyParser` implement, two field-access records, three type stubs and a type lift. That costs 5 further errors. `parser.go` is generated by goyacc and carries 129 `//line parser.go.y:N` directives. `fset.Position` returns the ADJUSTED position, whose column is 0 after a line-only directive, and the guard does not catch column 0. It is not established why the plain `-recurse` pass converts the same file without the panic. | Read the unadjusted position (`PositionFor(pos, false)`), and skip column 0 | small | converter |

After D, the three build sites and `parser.go`'s records were patched in the test tree. The two test hosts were then
built, run and compared with `-tests -test-action compare`.

**Library root: 162 of 164 matched.** Both failures are one test-side construct:

| # | tests | mechanism | remedy | size | owner |
|:--|:--|:--|:--|:--|:--|
| E | `TestCodeCompile_OptimizeTailRec_While`, `…_CallRec` | The test reads an unexported field through `*(*int)(unsafe.Pointer(reflect.Value.UnsafeAddr()))`. The C# side panics with a nil dereference in `NativeBox.DereferenceableAddress` (`ж.op_OnesComplement`). | Make golib's `UnsafeAddr` give a dereferenceable address for a field of a reachable value, or disclose the two tests | small to medium | golib |

**`cli`: 898 of 914 matched.** The 16 failures are 15 subtests plus their parent, and they come from two converter
mechanisms. Two subtests also show a performance gap:

| # | tests | mechanism | remedy | size | owner |
|:--|:--|:--|:--|:--|:--|
| F | 10 subtests: `unary_operators`, `query_with_leading_hyphen`, `range_function_with_iterators_in_arguments`, `array_nested_indexing_by_iterator`, and six more that negate | `funcOpNegate`'s `json.Number` arm: `return v[1:]` and `return "-" + v` keep the named type in Go. The C# `vΔ1[1..]` and `"-" + vΔ1` box a plain string. `cli` decodes numbers as `json.Number`, so `-1` prints as `"-1"`. | Re-wrap a slice or concatenation of a named string type in that type | small | converter |
| H | 5 subtests: `stream_option` and four `--stream` input tests | `append(s.path, 0)` with `path []any` emits `append(s.path, (any)0)`, which boxes a C# `int` rather than Go's `int` (`nint`). The encoder rejects it with `invalid type: int32 (0)`. | Box an untyped integer constant that converts to an interface as `nint` | small | converter |
| G | `assignment_operator_against_large_object` (81.2 s), `update-assignment_operator_against_large_object` (74.5 s) | These pass, but take 0.16 s and 0.17 s in Go: 440 to 510 times slower. Both build a 50,000-key object and update it. The cause is not established. | Profile the object update path | unknown | golib or converter |

**Network.** Traffic was not monitored. No test reported a network error, and the converter and both suites ran
with `GOPROXY=off` once the modules were downloaded.

## 6. Seats

**To a green build** (the whole module):

1. **A**: decide the string-or-bytes constraint from the type set. This unblocks uax29, then go-runewidth, then
   `cli`.
2. **B**: the capture copy inside a deferred call's function expression. It has an 8-line reproduction.
3. **C**: a pointer adapter for a generic type at a type parameter (converter record plus generic adapter).

**To validated, library root first** (the Roadmap puts the `cli` conformance suite at a later stage):

4. **D**: the `//line`-directive panic. It is needed for any `-tests` run of gojq.
5. **E**: golib `UnsafeAddr` for a field read through `unsafe.Pointer`, or disclose the 2 tests.

   After B, C, D and E (A is not needed), the library root should read 164 of 164 on this evidence. That is a
   prediction to score, not a measurement.
6. **F** and **H**: named-string re-wrap and untyped-constant boxing. The `cli` suite should then read 914 of 914.
7. **G**: the large-object update. Until then `cli` needs a deadline above its 2-minute default.

## 7. The prediction, scored

The prediction was posted to COORD before the first build.

| prediction | measured | |
|:--|:--|:--|
| conversion rc 0, all 11 converted, none skipped | rc 0, 11 of 11 | **MET** |
| library root closure 0 to 40 errors | 2 | **MET** |
| whole module 0 to 200 errors | 10 visible, 4 packages blocked | **MET** (with the probe, 0) |
| (a) uax29 `~[]byte \| ~string` generics the likeliest red | 8 of 10 errors | **MET** |
| (b) gojq's own: `goto` and `any` type switches over `math/big` | gojq's 2 errors are a capture copy and a generic pointer adapter; `goto` and the type switches compile | **MISSED** on the mechanism; the package was right |
| (c) x/sys/unix v0.38.0: 0 errors | 0 | **MET** |
| (d) go-yaml compiles; the run-time risk is higher | compiles; the `cli` suite's YAML tests all match | **MET** |
| falsifier: a class outside (a) to (d) above a quarter of the errors | 2 of 10 in (b)'s package with other mechanisms; 20% | did not fire |
| tests: verdicts not predicted | — | — |

One claim in COORD's brief was refuted by the tree: "-tests cannot combine with -recurse". At master it does
(`main.go:636-641`, `:821-851`), for the plain `-recurse` form.

— P1 (2026-11-07)
