# EXPERIMENT: go2cs readiness against the Go implementation of TypeScript (2026-11-07, i7)

> **Record**, point-in-time. Measurement only: no go2cs source was edited for this experiment.
> Low profile: internal record, not for visitor-facing pages.
> Owner authorization: the owner, 2026-11-07, scoped to `H:\go2cs-ts-exp`.
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
| Toolchain | go1.26.10 windows/amd64, `GOROOT=C:\Users\<user>\sdk\go1.26.10` (backslashes), `GOTOOLCHAIN=local`; dotnet SDK 10.0.400 |
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
| 51 | move2heap not emitted: appends at more than one site without observing capacity | informational; run-time effect unmeasured |
| 13 | move2heap not emitted: targets or an inlined copy disagree on its appends | informational |
| 7 + 1 | Unresolved dynamic struct type (`interface{KindString() string}` ×7, anonymous struct ×1) | C1, C2 |
| 2 | embedded-interface promoted method `watchCompilerHost.Trace` / `cachedCompilerHost.Trace` is variadic: pointer-form forwarder not emitted | C3 (stage 3 decides) |
| 1 | `@getGenericDefinition`: union pointer constraint `*ast.Node \| *ast.Symbol \| *checker.Type` (checker `relater.go`) not erased; "emission may not compile" | C4 (stage 3 decides) |
| 4 | const via `unsafe.Sizeof` may not match the run-time value (x/sys/windows `security_windows.go` ×2, `types_windows.go`; go-winio `hvsock.go`) | watch item |
| 2 | slow-package notices (checker, ast > 2 m) | perf only |
| 1 | package license unspecified (tsc module root LICENSE not referenced) | packaging only |

| 1b. stdlib rebuild at the stage-2 tip | a0fba9b08a | 0 | 1,116 s | 363/363 |
| 3A. `dotnet build _all.slnx` over the 138 converted csproj, emitted tree as-is | a0fba9b08a | 1 | 175 s | **36 built / 7 failed / 95 blocked** downstream. 36 unique errors, all first causes (C5-C11). The 8 raw-Go sites (C1, C2) are never compiled: their packages are all downstream of a failure. |
| 3B. same, C1 + C2 hand-patched (`passB-handpatch.diff`, 30 lines / 14 files, CRLF kept) | a0fba9b08a | 1 | 179 s | identical to 3A (same 36 errors, same 7 projects), as expected: the C1/C2 packages sit downstream of C5-C11. |

| 3C. same, plus C5-C11 hand-patched (`passC-handpatch-C5-C11.diff`, 420 lines / 51 files; 380+ of them the C7 rename `jsontext.Value`→`ΔValue`) | a0fba9b08a | 1 | 699 s | **46 built / 7 failed** (116 unique errors). First causes: C8 again (x/text/language, 14), C11 again (go-winio hvsock, 4), and the new C12, C13, C14, C15, C16, C17. The compile frontier moved from 36 to 46 projects. tsc's own root (tspath → core → ast → ...) is still blocked: jsontext (C12/C16) and vfs/internal (C14) sit under it. |

**Prediction scored as worded.**
- go-json-experiment/json: RIGHT package, WRONG reason. It is a name clash (C7), not reflect or generics.
- go-cmp: WRONG. All 5 packages build.
- x/sys/windows + go-winio: HALF. x/sys/windows builds; go-winio/internal/socket fails (C11).
- core + collections cascading: RIGHT that they are blocked, but by tspath (C6) and jsontext (C7) upstream. Their own constructs are not yet measured.
- Not predicted at all: C5 (vendored-path mapping), C8, C9, C10.

Downstream reach of each first cause, over the 140-package test closure: jsontext 79, tspath 78, x/text/internal/language 64, cpuid 60, go-winio/internal/socket 42, x/text/unicode/norm 17, gotest.tools/internal/format 13. Any one of them blocks 95.

Box-sharing datum: CPU affinity (0xFC0 vs COORD's 0-5) protects CPU but **not disk or memory bandwidth**. While this experiment's go test baseline and the stdlib cold build ran on the upper half, COORD's repoguard step on the lower half took 2,880 s against its usual 200-340 s.

## Defect catalogue

One row per ROOT CAUSE. Repro modules are saved file by file as `EXPERIMENT-ts-readiness-2026-11-07-data/repros/<module>_<path>.txt`; the `.txt` suffix keeps repo tooling off them. Each one is also quoted below. Four repro modules carry the numeral they had before the catalogue was renumbered: `c15recvshadowsimport` is C14, `c16typeswitchbinding` is C15, `c17namedbytesconstraint` is C16, `c18twolevelpromotion` is C17.

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
- **Reach:** modules with the shape (an anonymous struct as a type argument of a declared var). This is general Go set/map idiom.

C1 **reach:** modules with the shape (an anonymous-interface parameter satisfied from another package); a common "duck-typed debug helper" idiom.

### C5: a third-party import of a std-VENDORED path maps to std's vendor namespace (converter, S-M) — reach: ANY -recurse user

- **Mechanism:** in a converted module package, `import "golang.org/x/text/transform"` emits `using transform = vendor.golang.org.x.text.transform_package;` and `using vendor.golang.org.x.text;`, which is std's vendored copy (`GOROOT/src/vendor/golang.org/x/text/transform`), instead of the converted third-party module `go.golang.org.x.text.transform_package`. The project does not reference std's vendor assembly, so it fails with CS0246 'vendor' (27 errors, including 25 in go2cs-gen output that inherits the usings). x/text packages that std does NOT vendor (`internal/language`, `language`) map correctly.
- **TS site:** `golang.org/x/text/unicode/norm` (normalize.go, transform.go), imported by tsc for locale handling.
- **Repro** (`c5vendoredpath`; requires golang.org/x/text v0.42.0): `lib` imports `golang.org/x/text/transform` and returns `transform.Nop`; main calls `transform.String(lib.Nop(), "abc")`. Go prints `abc`; conversion rc=0; build gives CS0246 'vendor'.
- **Blast radius:** 17 TS-closure packages. Ecosystem-wide, it hits every module that imports a path std vendors: x/text transform/unicode/norm/unicode/bidi/secure/bidirule, x/net idna/http/httpguts/http2/hpack/dns/dnsmessage, x/crypto chacha20/chacha20poly1305/cryptobyte/hkdf, x/sys/cpu. These are among the most-imported modules in Go.

### C6: `cmp.Ordered` over a named string type (converter / go2cs-gen, S-M) — reach: ANY -recurse user with the shape

- **Mechanism:** `slices.Sort` on `[]Path` (`type Path string`) emits `slices.Sort<slice<Path>, Path>(...)`. Sort's E constraint is `IAdditionOperators<E,E,E>, IEqualityOperators<E,E,bool>, IComparisonOperators<E,E,bool>`, but the generated `Path` wrapper implements only `IGoEquatable`, so CS0315. Every `cmp.Ordered` consumer (slices.Sort/Max/Min/BinarySearch, cmp.Compare, maps sorted keys) is affected when it is instantiated with a named string type, and likely any named numeric type too (not measured).
- **TS site:** `internal/tspath/path.go:1234`, `slices.Sort(orderedGroups)` (3 errors, one site).
- **Repro** (`c6namedstringsort`): `type P string; s := []P{"b","c","a"}; slices.Sort(s)`. Go prints `a b c`; build gives CS0315.
- **Blast radius:** tspath is at the root of tsc: 78 packages downstream.

### C7: a Go type NAMED `Value` collides with the generated `Value` property (go2cs-gen TypeGenerator, S) — reach: ANY -recurse user with the shape

- **Mechanism:** every named-slice wrapper gets `public slice<T> Value => m_value;`. For `type Value []byte`, C# rejects a member named like its enclosing type (CS0542).
- **TS site:** `github.com/go-json-experiment/json/jsontext.Value` (1 error), which blocks all of go-json-experiment/json and therefore tsc's `internal/json` and everything above it: 79 packages downstream. `Value` is a common type name, e.g. any `type Value []byte` or `type Value map[...]...`.
- **Repro** (`c7typenamedvalue`): `type Value []byte; func (v Value) Len() int`. Go prints `3 abc`; build gives CS0542.

### C8: a value receiver calling a pointer method whose receiver is boxed (converter, M) — reach: modules with the shape

- **Mechanism:** when a pointer method's receiver is emitted boxed (`this ж<T> Ꮡt`), a call on an addressable VALUE receiver/parameter needs a heap box or a `ref` overload, and neither is emitted.
  - It emits `t.M()`, which gives CS1929 (repro `c8d`).
  - After `(t, err) = f(t)` it emits `Ꮡt.M()` with no `Ꮡt` declared, which gives CS0103 (repros `c8b`, `c8c`; the real site).
  - Without the boxed receiver it converts and runs correctly (`c8valuerecvptrcall`).
- **What boxes the receiver here:** a stack array sliced through a method call in the body (`var buf [8]byte; b := buf[:t.gen(buf[:])]`, shaped like x/text `(*Tag).RemakeString`).
- **TS site:** `golang.org/x/text/internal/language/match.go:202` `Tag.minimize` (1 error); 64 packages downstream.

### C9: a `:=`-bound variadic closure passed as a func-typed argument (converter, S) — reach: modules with the shape

- **Mechanism:** `writeFormat := func(format string, args ...interface{}) {...}; formatHeader(writeFormat, conf)`. The local is emitted as `var`, so C# infers an anonymous delegate (it has a `params` parameter) that does not convert to `go.Actionꓸꓸꓸ<@string, object>` (CS1503).
- **TS site:** `gotest.tools/v3/internal/format/diff.go` (2 errors); 13 test-side packages downstream.
- **Repro** (`c9variadicclosurearg`). Go prints `n=2 x`; build gives CS1503.

### C10: a named POINTER type converted to its underlying pointer at a call (converter, S) — reach: modules with the shape

- **Mechanism:** with `type Features *flagSet`, passing `f Features` to a `*flagSet` parameter emits `Ꮡf`, as if the conversion were `&f`. No such local exists, so CS0103.
- **TS site:** `github.com/klauspost/cpuid/v2/cpuid.go:457` `(*CPUInfo).HasAll` (1 error); 60 packages downstream (via xxh3).
- **Repro** (`c10namedpointertype`). Go prints `true false`; build gives CS0103.

### C11: a keyed literal of a named struct defined over a foreign struct (converter / go2cs-gen, S-M; repro not yet cut) — reach: modules with the shape

- **Mechanism:** `type GUID windows.GUID` becomes a value wrapper (`partial struct GUID /*...windows_package.GUID*/`, `[GoValueClone("Value")]`). The keyed literal `guid.GUID{Data1: ..., Data2: ..., Data3: ..., Data4: [8]byte{...}}` emits field-named constructor arguments that the wrapper does not have (CS1739).
- **Repro status:** a 2-package repro with a small local struct compiles AND runs correctly (`c11namedforeignstruct`), so the real ingredient is x/sys/windows.GUID's shape or its converted form. Not yet isolated.
- **TS site:** `github.com/Microsoft/go-winio/internal/socket/socket.go:113` (1 error); 42 packages downstream.

### Pass-C layer (behind C5-C11)

**C8 and C11 again.**
- x/text/language: 14× CS1929. `Tag.tag()` / `Tag.lang()` are boxed-receiver pointer methods called on value receivers: the C8 mechanism at more sites.
- go-winio `hvsock.go`: 4× CS1739, more keyed literals of `guid.GUID`: the C11 mechanism.

#### C12: the generated named-STRING wrapper is incomplete as a string (go2cs-gen TypeGenerator, M) — reach: ANY -recurse user with a named string type

- **Same family as C6, same site** (the named-string wrapper). The wrapper converts only from `@string`, `ReadOnlySpan<byte>` and nil. That leaves three gaps:
  - (a) no operator interfaces, so `cmp.Ordered` fails (C6);
  - (b) no conversion from `slice<byte>`, so Go `Pointer(b)` gives CS0030;
  - (c) no `GetEnumerator`, so `for i, r := range p` gives CS1579.
- **TS site:** `go-json-experiment/json/jsontext` `type Pointer string` (errors.go, state.go, decode.go; 8 errors).
- **Repro** (`c12namedstringbytesrange`): `type Pointer string; p := Pointer([]byte("/a/b")); for i, r := range p {...}`. Go prints `/a/b 2`; build gives CS0030 + CS1579.
- **Planning:** fix C6 and C12 together, as one wrapper completeness change, with a behavioral project covering every string operation over a named string.

#### C13: a package-level alias to `unsafe.Pointer` emits a namespace path (converter, S) — reach: modules with the shape (common in hashing/serialization code)

- **Mechanism:** `type ptr = unsafe.Pointer` emits `global using ptr = go.@unsafe.Pointer;`. That namespace does not exist (`unsafe` is `go.unsafe_package`), so CS0234. The resulting broken alias then cascades through xxh3: 64 errors, including go2cs-gen output (CS0715 "static classes cannot contain user-defined operators", CS0426 `Pointer` not found).
- **TS site:** `github.com/zeebo/xxh3/utils.go` (xxh3 is the asm-bearing dependency; its asm was NOT the blocker).
- **Repro** (`c13aliasunsafeptr`): `type ptr = unsafe.Pointer`. Go prints `42`; build gives CS0234.

#### C14: a receiver named like an import is renamed at its uses but not at its declaration (converter, S) — reach: modules with the shape

- **Mechanism:** `func (vfs *Common) Stat(path string) vfs.FileInfo { ... vfs.RootAndPath(path) ... }`. The body's receiver uses are emitted as `vfsΔ1.RootAndPath(...)`, but the receiver parameter is declared `this ref Common vfs`, so CS0103.
- **TS site:** `internal/vfs/internal/internal.go` (6) and `internal/bundled/embed.go` (2). vfs/internal is under tsc's whole VFS layer.
- **Repro** (`c15recvshadowsimport`). Go prints `/r/x`; build gives CS0103 `vfsΔ1`.

#### C15: a type-switch binding collides with a sibling case's `out var` (converter, S; small form does not reproduce) — reach: modules with the shape

- **Mechanism:** `switch expectedType := expected.(type) { case func(error) bool: ...; case reflect.Type: ... }` emits the second case as `case {} ΔexpectedType when ΔexpectedType._<reflectꓸType>(out var expectedType)`, which collides with the first case's pattern variable `expectedType` (CS0136).
- **Repro status:** the small form (`c16typeswitchbinding`) compiles AND runs correctly (`func:true type:int nil other`), so the real site's extra ingredient (closure context: the switch sits inside a returned `func() Result`) is not yet isolated.
- **TS site:** `gotest.tools/v3/assert/cmp/compare.go:312` (2 errors); test-side only.

#### C16: a named `[]byte` does not satisfy a `~[]byte | ~string` constraint (go2cs-gen / golib IByteSeq, S-M) — reach: modules with the shape (json/encoding libraries)

- **Mechanism:** batch 241 maps a string-or-bytes constraint to `IByteSeq`, but the generated named-slice wrapper (`type Value []byte`) does not implement `IByteSeq`, so CS0315 at `jsonwire.NewInvalidCharacterError[Bytes ~[]byte | ~string]`. The same family covers the go2cs-gen `Clone` receiver (CS0266 `object`→`ΔValue`) and `(ΔValue)src` from a type parameter (CS0030).
- **TS site:** `go-json-experiment/json/jsontext` (encode.go ×4, value.go, the generated Clone).
- **Repro** (`c17namedbytesconstraint`): `type Val []byte; prefix[Bytes ~[]byte | ~string](b Bytes, n int)` called with `v[1:]`. Go prints `bc x`; build gives CS0315.

#### C17: a two-level promotion through an embedded pointer, used for an interface implementation (go2cs-gen ImplementGenerator, M; small form does not reproduce) — reach: modules with the shape

- **Mechanism:** `type win32MessageBytePipe struct { win32Pipe; ... }` with `type win32Pipe struct { *win32File; path string }`. The generated `net.Conn` implementation for `*win32MessageBytePipe` calls `Close`/`SetReadDeadline`/`SetWriteDeadline` on `ж<win32MessageBytePipe>`, but the promoted extension it expects (from `*win32File`, two levels down) does not exist (CS1929, 5 errors).
- **Repro status:** the small form (`c18twolevelpromotion`, satisfying `io.Closer`) compiles AND runs correctly (`1`). The real ingredients are probably the larger interface (`net.Conn`) and the outer type overriding some of the methods (`CloseWrite`, `Read`, `Write`).
- **TS site:** `github.com/Microsoft/go-winio/pipe.go`.

### Stage-2 warnings, status after stage 3

C3 (variadic promoted `Trace` forwarders), C4 (non-erased union pointer constraint in checker `relater.go`), the 4 `unsafe.Sizeof` consts and the 64 move2heap notes: NOT YET CONFIRMED OR CLEARED, except one point: x/sys/windows BUILT, so its two `unsafe.Sizeof` consts at least compile (their run-time values are unmeasured). Everything else (execute, checker, go-winio hvsock) is downstream of the first walls.

## Catalogue summary

| Id | Mechanism | Class | Size | Reach | Repro | First seen | TS packages downstream |
|---|---|---|---|---|---|---|---|
| C1 | anonymous-interface parameter satisfied across packages | converter | S-M | shape | yes | stage 2 | (ast, scanner …; behind C6/C7) |
| C2 | anonymous struct as a type argument in a `var` | converter | S-M | shape | yes | stage 2 | ls |
| C3 | variadic promoted `Trace` forwarder not emitted | go2cs-gen | ? | shape | — | stage 2 warning | not yet compiled |
| C4 | non-erased union pointer constraint (checker `relater.go`) | converter | ? | shape | — | stage 2 warning | not yet compiled |
| **C5** | **third-party import of a std-vendored path → std vendor namespace** | converter | S-M | **ANY -recurse user** | yes | 3A | 17 |
| **C6 + C12** | **named-string wrapper incomplete: Ordered ops, `[]byte` conversion, range** | go2cs-gen | M | **ANY -recurse user** | yes | 3A / 3C | 78 (tspath), + jsontext |
| **C7** | **Go type named `Value` vs the generated `Value` property** | go2cs-gen | S | **ANY -recurse user** | yes | 3A | 79 |
| C8 | value receiver → boxed-receiver pointer method | converter | M | shape | yes | 3A, 3C | 64 |
| C9 | `:=`-bound variadic closure as a func argument | converter | S | shape | yes | 3A | 13 (test side) |
| C10 | named pointer type passed as its underlying pointer | converter | S | shape | yes | 3A | 60 |
| C11 | keyed literal of a named struct over a foreign struct | converter / gen | S-M | shape | not yet | 3A, 3C | 42 |
| C13 | alias to `unsafe.Pointer` emits a namespace path | converter | S | shape | yes | 3C | (xxh3 → 60) |
| C14 | receiver named like an import: uses renamed, declaration not | converter | S | shape | yes | 3C | vfs layer |
| C15 | type-switch binding vs sibling `out var` | converter | S | shape | not yet | 3C | test side |
| C16 | named `[]byte` vs `~[]byte \| ~string` (IByteSeq) | gen / golib | S-M | shape | yes | 3C | jsontext |
| C17 | two-level promotion through an embedded pointer, for an interface | go2cs-gen | M | shape | not yet | 3C | go-winio |

- **No wrong-answer class was found.** Every defect is a compile-time failure, and every repro that compiled also ran with Go's output: C8 small form, C11 small form, C15 small form, C17 small form, plus the two same-package controls. Nothing seen so far touches .3's shipped bits: all 17 rows are emission shapes the stdlib corpus does not exercise, which is why the corpus is green.
- **Not reached:** stage 4 (tests) and stage 5 (the bonus .ts run). No tsc-owned package above tspath compiled, so there is no test host to run. 46 of 138 projects compile with C1-C11 hand-patched.

## Closing planning read

**Headline: tracking TypeScript past 2026-09-16 requires Go 1.27 generic methods plus `encoding/json/v2`.**
- TS HEAD's own bump commit rewrote code to 31 generic-method declarations in 15 files; it cannot be pinned back to 1.26 without a source revert.
- Every TS commit since 2026-09-16 (254 touch `tsc/`) is out of reach of a 1.26-line go2cs.

**What gates the most.**
- Three rows are general defects that any `-recurse` user meets, independent of TypeScript:
  - C5: std-vendored import paths. x/text, x/net, x/crypto and x/sys/cpu are among the most-imported modules in the ecosystem.
  - C6 + C12: the named-string wrapper.
  - C7: a type named `Value`.
- Their downstream reach is 17, 78 and 79 of the 140 closure packages. Together with C8-C11, the seven pass-A first causes block 95. All three general rows are cheap: S to M each, each at a single converter or go2cs-gen site.
- After them, the frontier is C8, C10 and C13 (shape rows, S-M), then the jsontext pair (C12/C16) and the vfs receiver (C14). Those sit directly under tsc's own root.

**What a .4 needs, in order.**
1. C5, C6+C12 and C7: the general rows, each a red-first behavioral project from the repros here.
2. C8, C10, C13, C14 and C16: shape rows with ready repros.
3. Isolate C11, C15 and C17 (the real site compiles where the small form does not), then fix them.
4. Re-run this experiment at the same TS commit to read the NEXT layer. tsc's own packages (core, ast, checker) are not yet measured at all, and they hold most of the reflect/iter/generics weight. Expect at least one more layer of comparable size.

**For or against 1.27 first.**
- For TypeScript specifically, 1.27 is mandatory to follow upstream past 2026-09-16, but it does not unblock anything measured here. Every wall above is on the 1.26 commit and would persist on a 1.27 base.
- Ordering argument: fix the general rows on the current line first (.4). They raise every `-recurse` user, and they are prerequisite to measuring tsc's own packages on ANY Go version.
- Then take the 1.27 hop with generic methods. C# has generic methods natively, so the mapping is plausibly direct; the design is already listed in the Roadmap.
- A 1.27-first plan would re-measure into the same C5/C6/C7 walls.

**Box-sharing note for the fleet.** Pinning to 0xFC0 kept CPU apart but not disk or memory bandwidth. Heavy work on both halves slowed COORD's timing-sensitive step about 10×. Schedule heavy experiment work around timing gates, not beside them.
