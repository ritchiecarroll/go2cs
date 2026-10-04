# go2cs Architecture

A map of the converter pipeline, the `visit*`/`conv*` file taxonomy, the analysis passes, the Roslyn
source generators, and the runtime type map. For how to use the converter, see the [README](README.md#usage).

## Pipeline overview

```
Go source ──► go/parser + go/types ──► typed AST ──► analysis passes ──► visit*/conv* emit ──► C# files + .csproj
              (official Go toolchain)                 (escape, shadow,                          (per package)
                                                       collisions, generics)
```

The converter (`src/go2cs/`, Go) leans on Go's own front-end (`go/ast`, `go/types`, `go/token`,
`go/constant`, `golang.org/x/tools/go/packages`) for parsing and full semantic type information, then
walks the typed AST emitting C#. Because types are fully resolved, conversion decisions (overload
selection, implicit conversions, constant folding, unsigned detection) are semantic, not syntactic.

- **Entry point:** `main.go` — flag parsing, GOROOT/GOPATH resolution, and dispatch: a single file or
  package (`conversionDriver.go`), the standard library (`-stdlib`), a module (`-recurse`), or a
  package's tests (`-tests`).
- **Symbol constants:** `symbols.go` — the cross-language naming/marker constants (`RootNamespace`,
  `PackageSuffix`, `PointerPrefix` `ж`, `AddressPrefix` `Ꮡ`, …). Generated — together with its C# twin
  `src/core/go2cs/Symbols.cs` (class `go2cs.Symbols`, shared into golib and the `go2cs-gen` analyzer via
  `go2cs.projitems`) — from the **canonical symbol table `src/core/go2cs/symbols.json`** by
  `internal/gensymbols`. Edit the JSON, never the generated files; regenerate with `go generate .` from
  `src/go2cs`, or run `src/check-symbol-sync.ps1` (regenerates + exits 1 on drift).
- **Stdlib driver:** `stdLibConverter.go` — scans stdlib packages, builds the dependency graph, produces a
  topologically sorted queue (`sortedQueue`), and converts in dependency order (optionally filtered to a
  package list). Conversion is sequential — it relies on package-level converter state, so a converted
  importer must observe its dependency's finished `package_info.cs`. The topo order is reusable for bottom-up builds.
- **Module driver:** `moduleConverter.go` — `-recurse`: converts a downloaded module plus every
  third-party package in its import closure, in dependency order, while referencing the pre-converted
  standard library. `nuget*.go` maps third-party modules to published NuGet packages under
  `-recurse=nuget` (`nugetMap.go`, `nugetSubstitution.go`, `nugetLock.go`).
- **Test pipeline:** `testConversion.go` — `-tests`: converts a package's `_test.go` suite, emits a
  runnable test host, and compares its verdicts with `go test -json`. `moduleTestsDriver.go` runs it
  for every package of a module (`-tests -recurse`), and `validationProofPages.go` writes the proof
  pages.
- **Output:** `writeOperations.go` emits `.cs` files and generates each package's `.csproj` from a template,
  substituting markers (project references derived from detected imports, `unsafe` usage flag, etc.).

## `visit*.go` — AST node → C# declaration/statement

Each `visit*` file handles one Go AST node category, emitting the corresponding C# construct.

| File | Handles |
|---|---|
| `visitFile.go` | Top-level file: namespace, usings, package partial class. |
| `visitDecl.go` / `visitGenDecl.go` / `visitFuncDecl.go` | Declarations: general (var/const/type/import), functions & methods (receivers → `[GoRecv]`). |
| `visitTypeSpec.go` / `visitValueSpec.go` / `visitImportSpec.go` | Type/value/import specs. |
| `visitStructType.go` / `visitInterfaceType.go` | Struct & interface type defs (embedding, method sets → generators). |
| `visitArrayType.go` / `visitMapType.go` / `visitChanType.go` / `visitFuncType.go` | Composite type forms. |
| `visitStmt.go` / `visitBlockStmt.go` / `visitDeclStmt.go` / `visitExprStmt.go` | Statement dispatch & blocks. |
| `visitAssignStmt.go` / `visitIncDecStmt.go` / `visitSendStmt.go` | Assignment, `++/--`, channel send. |
| `visitIfStmt.go` / `visitForStmt.go` / `visitRangeStmt.go` | Conditionals & loops (range handles ptr deref). |
| `visitSwitchStmt.go` / `visitTypeSwitchStmt.go` / `visitSelectStmt.go` / `visitCommClause.go` | switch / type-switch / select + comm clauses. |
| `visitReturnStmt.go` / `visitBranchStmt.go` / `visitLabeledStmt.go` | return, break/continue/goto, labels. |
| `visitDeferStmt.go` / `visitGoStmt.go` | `defer` (registers into the enclosing `GoFrame`, drained LIFO), `go` (goroutine → thread pool/Task). |
| `visitIdent.go` | Identifier emission / name sanitization. |

## `conv*.go` — expression & type converters

| File | Converts |
|---|---|
| `convExpr.go` / `convExprList.go` | Expression dispatch / lists. |
| `convBasicLit.go` / `convCompositeLit.go` / `convFuncLit.go` / `convKeyValueExpr.go` | Literals & composites. |
| `convIdent.go` / `convSelectorExpr.go` / `convParenExpr.go` | Identifiers, selectors, parentheses. |
| `convCallExpr.go` / `convIndexExpr.go` / `convIndexListExpr.go` / `convSliceExpr.go` | Calls, indexing, generic index lists, slicing. |
| `convStarExpr.go` / `convUnaryExpr.go` / `convBinaryExpr.go` | Pointer deref / unary / binary ops. |
| `convTypeAssertExpr.go` | Type assertions (single-value panic vs `(value, ok)` overloads). |
| `convArrayType.go` / `convMapType.go` / `convChanType.go` / `convFuncType.go` | Type expressions. |
| `convStructType.go` / `convInterfaceType.go` | Struct/interface type expressions. |

Supporting: `Stack.go`, `HashSet.go` (generic data structures), `directiveOperations.go` (build tags /
`+build ignore` / cgo directives).

## Analysis passes

- `escapeAnalysisOperations.go` — stack-vs-heap escape detection, informs allocation strategy (`ж<T>` boxing).
- `refLoweringAnalysisOperations.go` / `refLoweringEmissionOperations.go` — decide which pointer
  parameters can be C# `ref` parameters instead of `ж<T>` heap boxes, and emit them.
- `variableAnalysisOperations.go` — lexical scope + shadowing; generates save/restore (`i__prev1` …) so
  Go's shadowing semantics survive in C#.
- `nameCollisionAnalysisOperations.go` — resolves C#-keyword / cross-symbol name collisions.
- `constraintOperations.go` — generic type constraint conversion (Go 1.18+).
- `importOperations.go` — import resolution & dependency tracking; drives generated `.csproj` references.

## Source generators (`src/gen/go2cs-gen/`, Roslyn)

Compile-time emission so converted C# stays visually close to Go. Referenced as an analyzer
(`OutputItemType="Analyzer"`) by every converted project.

| Generator | Trigger | Produces |
|---|---|---|
| `ImplementGenerator` | `[assembly: GoImplement<TStruct, TInterface>]` | Explicit interface implementations / promoted methods (Go duck-typing). |
| `RecvGenerator` | `[GoRecv]` on methods | Value/pointer receiver overloads handling `ptr<T>`/`ж<T>` deref. |
| `ImplicitConvGenerator` | `[GoImplicitConv]` | Implicit conversion operators between Go type aliases. |
| `TypeGenerator` | `[GoType]` | Wrapper types + field promotion for struct embedding. |
| `PartialStubGenerator` | a bodyless `partial` method with no implementing part | A throwing stub, so a Go function with no Go body (assembly or cgo) compiles until a hand-owned companion supplies it. |
| `StrGenerator` | `[GoStr]` on a method | The `@string` forwarder (and, for a package-level function, its value delegate) for an `sstring` twin. |

## Runtime type map (`src/core/golib/`)

| Go | C# (golib) |
|---|---|
| slice | `slice<T>` (array + low/high/cap) — `slice.cs` |
| array | `array<T>` — `array.cs` |
| map | `map<K,V>` (Dictionary + lock) — `map.cs` |
| channel | `channel<T>` (queue + wait handles) — `channel.cs` |
| string | `@string` (UTF-8 backed) — `string.cs`; `sstring`, a zero-copy stack view for a non-escaping `string([]byte)` — `sstring.cs` |
| `interface{}` / `any` | `object` |
| builtins (`append`,`len`,`cap`,`make`,`copy`,`panic`,`recover`,`close`,`delete`,`...` spread) | `builtin.cs` (the large one) |
| `nil` | `NilType` + `null` for heap refs |
| pointer-to-value boxing | `ж<T>` heap box |
| `int`/`uint` (platform-sized) | `nint`/`nuint` |
| `uint8`/`rune`/`uintptr`/`complex128`/… | aliases (`byte`/`int32`/`UIntPtr`/`Complex`) via global usings |
| panic / runtime error | `PanicException` / `RuntimeErrorPanic` |
| per-call `defer` list | `GoFrame` (`ref struct`, LIFO drain) — `GoFrame.cs` |

See [`ConversionStrategies.md`](ConversionStrategies.md) for a high-level, example-driven tour of the
per-construct mapping, and [`ConversionStrategies-Reference/`](ConversionStrategies-Reference/README.md) for the
exhaustive rationale and edge cases behind each one.
