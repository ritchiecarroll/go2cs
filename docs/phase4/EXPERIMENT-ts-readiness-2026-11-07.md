# EXPERIMENT: go2cs readiness against the Go implementation of TypeScript (2026-11-07, i7)

> **Record**, point-in-time. Measurement only: no go2cs source was edited for this experiment.
> Low profile: internal record, not for visitor-facing pages.
> Owner authorization: Ritchie, 2026-11-07, scoped to `H:\go2cs-ts-exp`.
> Brief: COORD, same day. The goal is a **gauge**, not a pass: a defect catalogue to plan .4, or the jump to Go 1.27.

## Source

| Item | Value |
|---|---|
| Where the Go code lives | `microsoft/TypeScript`, directory `tsc/`, module `github.com/microsoft/TypeScript/tsc`. The root `go.work` uses `./tools ./tsc`; every run here uses `GOWORK=off` on `tsc` alone. |
| Former staging repo | `microsoft/typescript-go` is CLOSED. Its last commit is `89d5d5b2849a0db0957065889ca58536fa6d2e4a` (2026-08-20), "Add closure notice and link to original repo (#4919)". The module was renamed (`28c68f49f8`) and moved into the TypeScript 7 layout (`5f647a841a`, 2026-08-19). |
| HEAD on the day | `f7973164a5f079a2d8b88d0fdb39347222549124` (2026-11-06), `go 1.27` |
| Go 1.27 bump | `5c2f7abf1733c9148100c5c6c7dd284745120d6b` (2026-09-16), "Update to Go 1.27 (#63902)". It is NOT only a directive change. It rewrites code to **generic methods** and swaps `github.com/go-json-experiment/json` for std `encoding/json/v2` + `encoding/json/jsontext`. |
| **Commit measured** | **`ba29a562991ceb942481e2a00810b4e5e4062e52`** (2026-09-16), "Update dependencies (#64283)": the bump's parent, `go 1.26.0`, **unmodified**. |
| Pin / patch | **None.** Pinning HEAD to 1.26 would mean reverting source, because go/types 1.26 rejects generic methods. That is a rewrite, not a patch. So the measured commit is the newest one that builds on go1.26.10 unmodified. |
| Toolchain | go1.26.10 windows/amd64, `GOROOT=C:\Users\ritchie\sdk\go1.26.10` (backslashes), `GOTOOLCHAIN=local`; dotnet SDK 10.0.400 |
| Dependencies | Fetched through `proxy.golang.org` only, with `GOSUMDB=sum.golang.org`. `go mod verify`: all modules verified. 23 modules in `go list -m all`; versions are exactly those `tsc/go.mod`/`go.sum` pin. Isolated `GOMODCACHE`/`GOCACHE`/`NUGET_PACKAGES` under `H:\go2cs-ts-exp`. |
| go2cs tip | `2e2b0bae6e` (batch 242; gojq seats A, B, D). Later tips: see the stage table. |

### HEAD sizing (why tracking TypeScript past 2026-09-16 needs Go 1.27)

- Generic-method declarations at HEAD: **31 in 15 `tsc` files**, by the regex `^func \([^)]*\) [A-Za-z_][A-Za-z0-9_]*\[`. This counts single-line declarations only, so it is a lower bound. The count at `ba29a56299` is 0.
- Std `encoding/json/v2` / `jsontext` import sites at HEAD: **1 file**, because TS routes all json through its own `internal/json` wrapper.
- 254 commits touch `tsc/` between the measured commit and HEAD.

## Dependency census (measured commit, production closure)

- 139 non-std packages across 13 modules, 104 of them tsc's own. No cgo.
- Assembly in 2 dependencies: `klauspost/cpuid/v2` (1 `.s`) and `zeebo/xxh3` (3 `.s`, with Go fallbacks).
- `go:linkname`: only in a darwin-only tsc file (`internal/fswatch/fsevents_darwin_ffi.go`) and in non-windows `x/sys` files.
- Packages importing each hazard, across the test closure: reflect 85, sync/atomic 27, iter 19, runtime 19, unsafe 13, syscall 8, os/exec 8, net 7.

### Prediction made before stage 2 (scored after it)

Before any conversion I expected the first walls in four places:
- `go-json-experiment/json`: a reflect-driven, generics-heavy v2 encoder;
- `go-cmp`: unsafe reflect value access;
- `x/sys/windows` + `go-winio`: a syscall surface converted as a third-party module, not as std;
- `tsc/internal/core` + `collections`: generics + iter + reflect at the root of the graph, so their failures cascade into every package.

## Go baseline (native, measured commit)

| Step | rc | Wall | Result |
|---|---|---|---|
| `go mod download` + `go mod verify` | 0 | 5 s | 102 proxy fetches, all modules verified |
| `go build ./...` | 0 | 187 s | 104 packages |
| `go vet ./...` | 0 | 110 s | clean |
| `go test -json -count=1 -timeout 60m ./...` | 1 | 2,324 s | packages 63 pass / 1 fail / 40 no tests. Top-level tests 5,196 pass / 1 fail / 420 skip. Subtests 155,975 pass / 3 fail / 1,732 skip. |

- **The one native failure is environmental.** `internal/astnav` (`TestGetTokenAtPosition`, `TestGetTouchingPropertyName`, `TestFindPrecedingToken`) shells out to `node` and needs `<repo>\node_modules\typescript\lib\typescript.js`. That needs an npm install, which this experiment's authorization does not cover. It is disclosed, not chased.
- `internal/fourslash/tests`: 4,118 pass, 417 skip (TS's own known-failing list). `internal/lsp` alone takes 876 s.
- A network watch over the test processes (any non-loopback TCP endpoint) saw nothing.

All Go steps ran pinned to logical CPUs 6-11 (mask 0xFC0), sharing the box with COORD's gate batteries, so the wall times are not perf numbers.

## Stage table

| Stage | go2cs tip | rc | Wall | Result |
|---|---|---|---|---|
| 1. converted stdlib build (`src\go2cs-stdlib.slnx`, Debug, local, no NuGet go.*) | 2e2b0bae6e | 0 | 1,404 s (cold restore, 6 cores, contended) | 363/363 projects, 0 errors, 1 warning (CS9113 `testing\TestReporter.cs:98`) |
| 2. `go2cs -recurse -go2cspath <clone>\src <tsc> <out>\recurse` (local refs) | a0fba9b08a (`claude/coord-b245-gate`: 243 + 244 + 245 on master d07c7600cb) | **1** | 2,429 s | 139/139 packages converted (104 app + 35 third-party; 149 stdlib referenced; `fourslash/tests` skipped, test files only) → 138 csproj. 0 panics, 0 refusals during conversion. **rc=1 is the end-of-run guard**: 8 unresolved dynamic types emitted as raw Go (rows C1, C2). Because the run fails at that guard, the per-project `.slnx` step never runs; stage 3 builds a hand-written `_all.slnx` over the 138 csproj. |

### Stage 2 warning tally (82 WARNING lines)

| Count | Warning | Catalogue |
|---|---|---|
| 51 | move2heap not emitted: appends at more than one site without observing capacity | informational (see C-notes) |
| 13 | move2heap not emitted: targets or an inlined copy disagree on its appends | informational |
| 7 + 1 | Unresolved dynamic struct type (`interface{KindString() string}` ×7, anonymous struct ×1) | C1, C2 |
| 2 | embedded-interface promoted method `watchCompilerHost.Trace` / `cachedCompilerHost.Trace` is variadic: pointer-form forwarder not emitted | C3 (stage 3 decides) |
| 1 | `@getGenericDefinition`: union pointer constraint `*ast.Node \| *ast.Symbol \| *checker.Type` (checker `relater.go`) not erased; "emission may not compile" | C4 (stage 3 decides) |
| 4 | const via `unsafe.Sizeof` may not match the run-time value (x/sys/windows `security_windows.go` ×2, `types_windows.go`; go-winio `hvsock.go`) | watch item |
| 2 | slow-package notices (checker, ast > 2 m) | perf only |
| 1 | package license unspecified (tsc module root LICENSE not referenced) | packaging only |

Box-sharing datum: CPU affinity (0xFC0 vs COORD's 0-5) protects CPU but **not disk or memory bandwidth**. While this experiment's go test baseline and the stdlib cold build ran on the upper half, COORD's repoguard step on the lower half took 2,880 s against its usual 200-340 s.

## Defect catalogue

One row per ROOT CAUSE. Repro modules live in `H:\go2cs-ts-exp\repros\<name>` on the i7. Each one is quoted below in full, so the record stands alone.

### C1: anonymous interface parameter type, satisfied across a package boundary (converter, S-M)

- **Mechanism:** an implicit conversion of a concrete pointer to ANOTHER package's anonymous-interface parameter type. The call site names the adapter after the raw Go type string (`ast_Nodeжinterface{KindString() string}`) because the anonymous interface's lifted name is not resolvable from the calling package. The same shape **within one package converts clean** (repro `anoniface-local`, rc=0).
- **TS site:** `internal/debug/debug.go:17` `func FailBadSyntaxKind(node interface{ KindString() string }, message ...any)`, called with `*ast.Node` from ast, ls (×2), pseudochecker, scanner, transformers/estransforms and transformers/tstransforms: **7 sites**.
- **Repro** (`anoniface`, rc=1 "Unresolved dynamic struct type: interface{K() string}"):
  ```go
  // dbg/dbg.go
  package dbg
  func Fail(n interface{ K() string }) string { return "fail:" + n.K() }
  // main.go
  package main
  import "repro/anoniface/dbg"
  type N struct{ k string }
  func (n *N) K() string { return n.k }
  func main() { println(dbg.Fail(&N{k: "x"})) } // Go: fail:x
  ```
- **Blast radius:** 7 files in 7 packages. ast and scanner sit at the root of the graph, so this one shape gates nearly every package's compile.

### C2: anonymous struct as a type argument in a `var` declaration (converter, S-M)

- **Mechanism:** `var seen Set[struct{...}]` emits `Set<struct{name string; kind int32}> seen = default;` as raw Go. The identical anonymous struct in the composite literal beside it IS lifted (`ProvideDocumentSymbols_key`), but the type-argument position of the var's declared type is not matched to that lift. Neither a cross-package generic nor a cross-package field type is needed: the repro `anonstructarg-local` is one package with builtin fields only.
- **TS site:** `internal/ls/symbols.go:29` (`var seen collections.Set[struct{ name string; kind lsproto.SymbolKind; rng lsproto.Range }]`): 1 site.
- **Repro** (`anonstructarg-local`, rc=1):
  ```go
  package main
  type Set[T comparable] struct{ m map[T]struct{} }
  func (s *Set[T]) AddIfAbsent(v T) bool { if _, ok := s.m[v]; ok { return false }; if s.m == nil { s.m = map[T]struct{}{} }; s.m[v] = struct{}{}; return true }
  func main() {
  	var seen Set[struct{ name string; kind int32 }]
  	for _, n := range []string{"a", "b", "a"} {
  		key := struct{ name string; kind int32 }{n, 1}
  		println(seen.AddIfAbsent(key)) // Go: true true false
  	}
  }
  ```
- **Blast radius:** 1 file (ls), so only ls and its dependents.

## Closing planning read

<!-- headline: tracking TypeScript past 2026-09-16 requires Go 1.27 generic methods plus encoding/json/v2 -->
