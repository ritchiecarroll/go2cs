# CLASS A census 1: which assemblies a -tests project must reference

C1, 2026-11-17, against master b2eaf5290a, Go 1.26.11. COORD ruling 20261117T142623Z (GO, census first).

`main.go` loads each committed `src/core/**/*.tests.csproj` (242, the windows flavour) with `go/packages`
(tests on, tags `purego,math_big_pure_go`). It compares the converted packages each project references today with
what each model says it needs:

| column | model |
|:--|:--|
| `opt1` | the whole import closure of what the test assembly compiles |
| `opt2` | the API closure: packages named by the exported surface of every referenced package, to a fixpoint |
| `opt2f` | `opt2` with methods, funcs and fields kept only when the test sources select that name |
| `use` | the use-site model: the packages named by the types of the compiled test files' own expressions, plus, for each method the tests call on a concrete receiver, the signature packages of every same-named method of an exported type in a visible package, to a fixpoint; `-refthis` limits those candidates to the methods go2cs emits as `this ref T` extensions, `-ptronly` to pointer-receiver methods |

The `use` model reads only the `_test.go` files whose conversions the project compiles (its `<Compile>` items).
A reference-model project references the production project; it does not recompile it.

Files:
- `std-windows-allmethods.tsv`: `use` over every method.
- `std-windows-refthis.tsv`: `-refthis`, with `refthis-windows.tsv`.
- `std-{windows,linux,darwin}-ptrrecv.tsv`: `-ptronly`.
- `modules-refthis.tsv`: the eight banked ValidatedModules modules that have Go tests, each converted with
  `-tests -test-action convert -recurse` into a scratch root.
- `refthis-{windows,linux}.tsv`: every `this ref T` extension in `src/core` at b2eaf5290a, per flavour.

Positive control: COORD's t2b repro (claude/coord-t2b-repro 4bdd28cb9e). It adds exactly `database/sql/driver` under
`-refthis` and `-ptronly`, and under `use` over every method too.
