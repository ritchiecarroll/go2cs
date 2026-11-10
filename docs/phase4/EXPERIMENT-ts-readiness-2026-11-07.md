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

---

## Amendment 2026-11-09 (i7): re-run at the same TypeScript commit, against go2cs `f09e762659`

> **Dated amendment.** The record above is unchanged. This block adds a second measurement and does not rewrite the first.
> Requested by the owner on 2026-11-09. COORD coordinated the box and held its i7 batteries while the heavy stages ran, so this run's wall times are uncontended; the first run's were not.
> Measurement only: no go2cs source was edited. Hand-patches touch only a COPY of the emitted tree, each one names its catalogue id, and none is applied for a row already on master.

### What was measured

| Item | Value |
|---|---|
| TypeScript | **`ba29a562991ceb942481e2a00810b4e5e4062e52`**, the same commit, unmodified, built with `GOWORK=off` on `tsc` |
| go2cs | **`f09e762659`**: batch 277's gate tip, which landed by fast-forward. It is in master's history; master at the time of writing is `0c553534df`, whose batch 278 adds docs only. |
| Rows landed since the first run | C1, C2, C3, C4, C5, C6+C12 (plus addendum B: a named-string index or slice compared with a literal), C7, C8, C9, C10, C11, C13, C14, C15, C17 (including the GO2CS2004 refusal), C18 (named pointer locals and fields) |
| Rows still open | C16 (named `[]byte` vs `~[]byte \| ~string`), and GO2CS2004's other half (a defined type over an instantiated generic struct) |
| Toolchain | Unchanged: go1.26.10 windows/amd64, `GOTOOLCHAIN=local`, dotnet SDK 10.0.400 |
| Dependencies | Unchanged: `proxy.golang.org` with `GOSUMDB=sum.golang.org`. `go mod download` + `go mod verify`: all 23 modules verified. |

### Stage table

| Stage | rc | Wall | Result |
|---|---|---|---|
| 1. converted stdlib build (`src\go2cs-stdlib.slnx`, Debug, local) | 0 | 1,210 s | 363/363 projects, 0 errors, the same 1 warning (CS9113 `testing\TestReporter.cs:98`) |
| 2. `go2cs -recurse` over `tsc` | **0** | 355 s | 139/139 packages → 138 csproj; 0 refusals. **The end-of-run guard now passes** (the first run ended rc=1 on 8 unresolved dynamic types). The emitted file set is identical to the first run's (827 `.cs` files). 69 WARNING lines: 64 move2heap, 4 `unsafe.Sizeof` consts, 1 license. Gone since the first run: C1/C2's 8 unresolved types, C3's 2 `Trace` notices, C4's `relater.go` notice, and the 2 slow-package notices. |
| 3A. `dotnet build` over the 138 csproj, emitted tree **as-is** | 1 | 241 s | **54 built / 3 failed / 81 blocked**, 13 unique errors. For comparison: the first run's 3A was 36 / 7 / 95, and its best hand-patched pass (3C, C1-C11 patched) was 46 / 7 / 85. The three failures are C16 (open), and the new C19, C20 and C21. |
| 3B. + C16, C19, C20, C21 | 1 | 260 s | 59 / 1 / 78. 184 errors, all in `go-json-experiment/json`: the GO2CS2004 shape, reached through two aliases (see *Open rows*). |
| 3C. + GO2CS2004: name the underlying generic in the declaration | 1 | 244 s | 59 / 1 / 78. 4 errors: GO2CS2004's other half (the generated wrapper keeps the type parameter `Coder` in its forwarded field types). |
| 3D. B + GO2CS2004 hand-owned wrapper | 1 | 315 s | **64 / 3 / 71**. 775 errors: C22 (774) and C23 (1). `go-json-experiment/json` builds, and so does every third-party package. An intermediate D build (59 / 1 / 78, 9 errors) showed that the generic struct has no `ΔClone`, and that C16 has 3 more sites in `json`; both are folded into D. |
| 3E. + C22, C23 | 1 | 331 s | **70 / 3 / 65**. 81 errors, all in tsc's own packages: C23 (70 more sites), C24, C25, C26, C27. tsc's `collections` builds. |
| 3F. + the rest of C23, C24, C25, C26, C27 | 1 | 395 s | **78 / 2 / 58**. 5 errors: 3 are fallout of my own C27 hand-own (no converter defect), and 2 are C28. **tsc's `core` builds.** |
| 3G. + the C27 follow-up, C28 | 1 | 1,121 s | **79 / 1 / 58**. 533 errors, all in tsc's `ast`: C29. **The run stops here.** |

**Frontier.**
- 79 of 138 projects compile: all 35 third-party projects, and 44 of tsc's own (`bundled collections core debug diagnostics execute/watchmanager fswatch glob ipc jsnum json jsonrpc locale lsp/lsproto nativepath osutil packagejson pprof project/background project/dirty project/logging repo semver spanmap stringutil tspath vfs`, all ten `vfs/*` and all ten `testutil/*`).
- 58 projects are blocked behind `ast`, among them `scanner`, `binder`, `checker`, `compiler`, `ls` and `execute`. Those hold most of tsc's reflect, iter and generics weight, and they are still unmeasured.
- **Stages 4 and 5 were not reached**: no test host or tool sits above `ast`.

The hand-patch footprint over the emitted tree is 108 files, +1,494 / -1,208 lines; ~1,100 of them are C22's renames. The patch scripts (`rerun-r2*_patch.py.txt`) are the reproducible definition: each one asserts its match counts and names its catalogue id. The per-file footprint is `rerun-handpatch-summary.txt`. The full diff stays on the i7 box (SHA-256 `5365adb8…3c84`), because it is mostly converted TypeScript.

### Scoring the landed rows: did each first cause disappear at its REAL site?

| Row | Real site (first run) | Re-run | Small form (repro) |
|---|---|---|---|
| C1 | 7 `debug.FailBadSyntaxKind` call sites (ast, scanner, ls, …) | **Fixed at conversion.** No raw Go is emitted; the call sites use the declaring package's lift (`new ast.NodeжFailBadSyntaxKind_node(Ꮡnode)`), and `debug` builds. The 7 call sites themselves sit in packages behind C29, so they are not yet compiled. | compiles, prints Go's `fail:x` |
| C2 | `ls/symbols.go:29` | **Fixed at conversion** (`collections.Set<ProvideDocumentSymbols_seen>`). `ls` is behind C29. The same line also needs C22 (`collections.Set` is Δ-renamed). | compiles, matches Go (3 repros) |
| C3 | `execute` `Trace` forwarders | **Fixed at conversion**: the warning is gone and the forwarders are emitted (`execute/watcher.cs:70`). `execute` is behind C29. | — |
| C4 | `checker/relater.go` | **Fixed at conversion**: the warning is gone. `checker` is behind C29. | — |
| C5 | `x/text/unicode/norm` | **Fixed: builds.** | matches (`abc`) |
| C6 + C12 | `tspath/path.go:1234` `slices.Sort`; jsontext `type Pointer string` | **Fixed: `tspath` builds**, from the unpatched `slices.Sort<slice<Path>, Path>`. jsontext's 8 `Pointer` errors are gone. | matches (`a b c`; `/a/b 2`) |
| addendum B | named-string index or slice vs a literal | No error of this shape in any of the 79 built projects. I did not map its TS sites, so this is "not seen failing", not a site score. | — |
| C7 | jsontext `type Value []byte` | **Fixed.** CS0542 is gone; jsontext builds with only the C16 and C21 patches, and no rename is needed. | matches (`3 abc`) |
| C8 | `x/text/internal/language` (1), `x/text/language` (14) | **Fixed: both build.** | matches (4 repros) |
| C9 | `gotest.tools/v3/internal/format` | **Fixed: builds.** | matches (`n=2 x`) |
| C10 | `klauspost/cpuid/v2` | **Fixed: builds.** | matches (`true false`) |
| C11 | go-winio `internal/socket` and `hvsock.go` (5 sites) | **Fixed: both build.** | matches |
| C13 | `zeebo/xxh3/utils.go` (`type ptr = unsafe.Pointer`) | **Fixed.** The CS0234 cascade (64 errors) is gone; xxh3 now fails only on the new C19. | matches (`42`) |
| C14 | `vfs/internal` (6), `bundled` (2) | **Fixed.** `bundled` builds; `vfs/internal`'s CS0103 is gone, and it then fails only on the new C20. | matches (`/r/x`) |
| C15 | `gotest.tools/v3/assert/cmp` | **Fixed: builds.** | matches |
| C17 | go-winio `pipe.go` (`net.Conn` through two embeds) | **Fixed: builds.** | matches (`1`) |
| C18 | named pointer locals and fields | No error of this shape in any of the 79 built projects. | — |

**Headline.**
- Every landed row's first cause is gone at its real site, wherever the build reaches that site.
- Four rows (C1, C2, C3, C4) are clean at conversion, but their sites sit behind the new `ast` wall, so they are not yet compiled.
- No row was found whose small form is fixed while its real site still fails.
- Apart from the two open rows, no catalogue row from the first run produces an error anywhere in the 79 compiled projects.

### Open rows, measured

- **C16** (open): it reproduces at the real site, and in more places than the first run saw: jsontext `encode.go` ×5 and `value.go` ×1, plus 3 more in `json/arshal_default.go` (`AppendUnquote`, `len64` ×2). The `json` sites were hidden behind jsontext in the first run. The patch passes the named slice as its underlying `slice<byte>`, as Go's instantiation `Bytes = Value` means. Downstream: 79.
- **GO2CS2004 — FINDING: the refusal is bypassed through aliases.**
  - The site is `go-json-experiment/json` `options.go:266`, `type marshalersOption Marshalers` (likewise `unmarshalersOption`). Here `Marshalers = typedMarshalers` and `typedMarshalers = typedArshalers[jsontext.Encoder]`.
  - That is a defined type over an instantiated generic struct, reached through two ALIASES. go2cs neither refuses it nor drops the file: it emits the declaration with the raw Go struct text as its underlying type, which gives 184 errors.
  - Once the underlying type is named, the other half shows: the generated wrapper forwards the generic's fields with the type parameter unsubstituted (`slice<typedArshaler<Coder>>`), and the generic struct carries no `ΔClone`.
  - Repro `go2cs2004alias` (conversion rc=0; the build reproduces the TS site's CS0501/CS0102 on the raw struct text), against the direct control `go2cs2004direct` (refused: `error GO2CS2004: a defined type over an instantiated generic type is not supported yet (type D G[int])`).
  - **For P1's fix:** the shape test must see through aliases (`types.Unalias` on the underlying). My own first-run census missed this site for the same reason: its regex read only the syntax.
  - Downstream: 78.

### New catalogue (C19+)

Each row is tagged **GENERAL** (an everyday Go idiom that most modules of any size contain, so any `-recurse` user meets it) or **SHAPE** (only modules with the pattern). The downstream count is the number of projects blocked transitively behind the failing project, at the pass where the row was first seen. Repro modules are saved under `-data/repros/` and quoted in short form below. "Repro" gives this re-run's verdict at `f09e762659`.

#### C19: `&^` with an untyped constant on a `uintptr` ALIAS (converter, S) — SHAPE

- **Mechanism:** `top := ui(l) &^ 15`, with `type ui = uintptr`, emits `(ui)(((ui)l) & ~15)`. The untyped constant stays `int`, and golib's `uintptr` has no `&` with `int` (CS0019). The same expression written with `uintptr` instead of the alias converts and runs correctly (control `c19ctl-uintptrandnot`: `32 16`), so the alias is the ingredient.
- **TS site:** `zeebo/xxh3` `hash64.go:90`, `hash64_seed.go:92`, `hash128.go:188`, `hash128_seed.go:190` (4 errors). This is the C13 package's next wall.
- **Repro** (`c19uintptraliasandnot`): `type ui = uintptr; func top(l int) ui { return ui(l) &^ 15 }`. Go prints `32 16`; the build reproduces CS0019.
- **Downstream:** 59 (xxh3, through tsc's hashing).

#### C20: a `switch` over an ARRAY value with composite-literal cases (converter, S) — SHAPE

- **Mechanism:** `switch bom { case [2]byte{0xFF, 0xFE}: ... }` emits a C# `switch (bom)` with `case new byte[]{0xFF, 0xFE}.array():`. A case label must be a constant, so CS9135. Go arrays are comparable, so the cases need a value comparison (`case var _ when bom == ...`, or an if-chain).
- **TS site:** `internal/vfs/internal/internal.go:161` `decodeBytes` (a byte-order-mark switch; 2 errors). This is the C14 package's next wall.
- **Repro** (`c20arrayswitch`). Go prints `le be none`; the build reproduces CS9135 ×2.
- **Downstream:** 17 (the vfs layer).

#### C21: a Go method named `Clone` on a named slice is shadowed by the wrapper's own `object Clone()` (go2cs-gen TypeGenerator, S) — GENERAL

- **Mechanism:**
  - Every generated named-slice wrapper implements `ICloneable` with an instance `public object Clone()`. A Go method `func (v Value) Clone() Value` is emitted as an EXTENSION method, and C# binds an instance member before an extension.
  - So `v.Clone()` returns `object`, and the generated pointer forwarder (`return v.Clone();` typed `Value`) fails with CS0266. Every package that defines such a method fails to compile.
  - Because it fails in the generated forwarder, no silent wrong answer can come from it. Repro `c21namedsliceclone-any` passes the result as `any` and still fails at that same generated line.
- **Reach:** GENERAL. `Clone()` on a named collection type is an everyday Go idiom. The named-map wrapper probably has the same shape; that was not measured.
- **TS sites:** `go-json-experiment/json/jsontext` `value.go:69` (`func (v Value) Clone() Value`) and its caller `json/errors.go` (2 sites).
- **Repro** (`c21namedsliceclone`): `type V []byte; func (v V) Clone() V {...}`. Go prints `ab!`; the build reproduces CS0266 in the generated forwarder.
- **Downstream:** 79 (it fails in jsontext together with C16).

#### C22: a type Δ-renamed for a method-name clash keeps its Go name in a consumer's PACKAGE-QUALIFIED reference (converter, S-M) — GENERAL

- **Mechanism:**
  - Go lets a package declare a type and a method of the same name (`type Kind` beside `func (t Token) Kind() Kind`). C# cannot hold a nested type and a same-named extension method in one package class, so the package Δ-renames the type and records `[assembly: GoTypeAlias("Kind", "ΔKind")]`.
  - Consumers that use the imported-alias form are right (`astꓸSymbol`, 1,456 uses).
  - A consumer's package-qualified form keeps the Go name, so CS0426. The failing forms are `ж<jsontext.Decoder>` in a signature, a conversion `(jsontext.Kind)(…)`, and above all a generic type, which has no alias form (`collections.Set<T>`).
- **Reach:** GENERAL. A type named after a method of the same package is everyday Go (`Kind`/`Kind()`, `Encoder` beside an `Encoder()` accessor, `Set` beside `OrderedMap.Set`).
- **TS sites:** 1,141 qualified references:

  | Package | Type | References |
  |---|---|---|
  | jsontext | `Decoder` | 516 |
  | jsontext | `Kind` | 203 |
  | jsontext | `Encoder` | 123 |
  | `collections` | `Set` | 280 |
  | `project/dirty` | `Value` | 19 |

  21 packages in the closure record 42 Δ renames between them, so more of these follow behind `ast`.
- **Repro:**
  - `c22b-genericset` reproduces it: a generic `coll.Set[T]` beside `OrderedMap.Set`, used as a field type in another package. Go prints `1`; the build gives CS0426 `Set<>`.
  - The non-generic small form (`c22renamedtypequalified`: `lib.Kind`, `*lib.Encoder` in one module) compiles and runs correctly. The jsontext route's extra ingredient is probably the module boundary (jsontext is a third-party module), not yet isolated.
- **Downstream:** `collections` 71 and `lsproto` 19. `collections` sits under nearly all of tsc.

#### C23: a named `[]byte` from ANOTHER MODULE, converted from a string constant, emits a u8 span with no conversion (converter / go2cs-gen, S; small form not isolated) — reach pending isolation

- **Mechanism:** `result = jsontext.Value("null")` emits `(jsontext.Value)"null"u8`. The named-slice wrapper converts from `slice<byte>`, not from `ReadOnlySpan<byte>`, so CS0030.
- **Repro status:** the same-package form (`c23namedbytesfromconst`) and the same-module cross-package form (`c23b-crosspkgbytes`) both compile and print Go's `null 4`, so the converter takes another route there. `encoding/json.RawMessage("null")` from std: also compiles and prints `null 4`. Its emission, `(json.RawMessage)slice<byte>((@string)"null"u8)`, shows the general path is right, so the TS site's extra ingredient is the third-party module boundary or the jsontext package itself. Not yet isolated (probe `c23c-rawmessage`).
- **TS sites:** `internal/ipc/protocol_jsonrpc.go` (1) and `lsp/lsproto`'s generated code (70, written as verbatim u8 strings: `(jsontext.Value)@"""id"""u8`).
- **Downstream:** `ipc` 40 and `lsproto` 19.

#### C24: a type parameter constrained `~map[K]V` returned as `map[K]V` (converter, S) — SHAPE

- **Mechanism:** `func CopyMapInto[M1 ~map[K]V, M2 ~map[K]V, K comparable, V any](dst M1, src M2) map[K]V { ...; return dst }`. C# has no conversion from the type parameter (an `IMap<K, V>`) to `map<K, V>`, so CS0029 ×2 (`return dst`, and `return maps.Clone(src)`, which is typed `M2`).
- **TS site:** `internal/core/core.go:805`. `core` is tsc's root package.
- **Repro** (`c24mapcoreparamreturn`). Go prints `2 2`; the build reproduces CS0029 ×2.
- **Downstream:** 65.

#### C25: a generic type's pointer converted to a generic interface INSIDE a generic body: the adapter is never generated (converter / go2cs-gen ImplementGenerator, M) — SHAPE

- **Mechanism:**
  - `func (b *Box[T]) Locked(fn func(Value[T])) { fn(b) }` emits `fn(new BoxжΔValue<T>(Ꮡb))`.
  - Implementations are registered as `[assembly: GoImplement<…>]` only for CLOSED instantiations, and an attribute cannot name the open `Box<T>`. With no closed use anywhere, the adapter class is never generated, so CS0246.
- **TS sites:** `internal/project/dirty` `box.go`, `map.go`, `syncmap.go` ×2 (3 adapter types).
- **Repro** (`c25opengenericiface`). Go prints `7`; the build reproduces CS0246 `BoxжΔValue<>`.
- **Downstream:** 12 (the `project` layer).

#### C26: a pointer receiver in its `ref` form escapes as a value into an interface field (converter, S-M) — SHAPE

- **Mechanism:**
  - `func (r *RequestMessage) Message() *Message { ...; return &Message{Kind: kind, msg: r} }`, where `msg` is `any`.
  - The receiver is emitted `this ref RequestMessage r`, because the body only reads through it. The escape `msg: r` then emits `r.OrTypedNil()`, which needs an `ж<T>` that a `ref` receiver does not have (CS0411).
  - Go stores the same pointer. Taking the receiver's address into an interface needs the boxed receiver form.
- **TS site:** `internal/lsp/lsproto/jsonrpc.go:87` and `:125` (`RequestMessage`, `ResponseMessage`).
- **Repro** (`c26refrecvescape`). Go prints `2 true`; the build reproduces CS0411.
- **Downstream:** 19.

#### C27: go2cs-gen constructs a MAP field as if it were a struct, because its type text names a struct that needs construction (go2cs-gen TypeGenerator, S) — SHAPE

- **Mechanism:**
  - In `type Map[K, V] struct { base map[K]V; dirty map[K]*MapEntry[K, V] }`, `MapEntry` embeds a struct, so it "needs construction".
  - The zero-value initializer (`AppendZeroValueInitializers`, `StructTypeNeedsConstruction`) classifies the FIELD `dirty` by that inner type name and emits `this.dirty = new map<K, ж<MapEntry<K, V>>>(nil)`.
  - `map<K, V>` has no `NilType` constructor, so the `(nint size)` overload fails with CS1503 ×3. A map field's zero is nil and needs no construction.
- **TS site:** `internal/project/dirty/map.go:52`.
- **Repro** (`c27mapfieldctor`). Go prints `true 1`; the build reproduces CS1503 in the generated wrapper.
- **Downstream:** 12 (the same project as C25).

#### C28: a pointer PARAMETER held as a deref'd `ref` local and reassigned in a `for` INIT: the init's two statements are run together (converter, S) — SHAPE

- **Mechanism:**
  - `for node = node.Parent; node != nil; node = node.Parent` emits `for (Ꮡnode = node.Parent node = ref Ꮡnode.DerefOrNull();; Ꮡnode != nil; ...)`.
  - That is two statements with no separator, plus a stray `;` (CS1003 ×2). The init must re-derive the ref local after the pointer assignment, as the post statement already does.
- **TS site:** `internal/ast/utilities.go:1865` `GetSuperContainer` (1 site in tsc; the tree-walker idiom `for n = n.Parent; n != nil; n = n.Parent`).
- **Repro** (`c28ptrparamforinit`). Go prints `true true`; the build reproduces CS1003 ×2.
- **Downstream:** 58 (`ast`).

#### C29: a method shadowed across embed depths is not promoted, so the generated forwarders and interface adapters fail (go2cs-gen, M-L; small form does not reproduce) — SHAPE, but it gates all of tsc above `ast`

- **Mechanism:**
  - tsc's node model is `type NodeDefault struct{ Node }`, which REDECLARES ~20 of `Node`'s methods. Then `type NodeBase struct{ NodeDefault }`, and every concrete node embeds `NodeBase` through `ExpressionBase`/`StatementBase`/`TypeNodeBase`/…, often beside sibling embeds (`DeclarationBase`, `FlowNodeBase`, `ModifiersBase`, …) at the same depth.
  - Go selects the SHALLOWEST method (`NodeDefault`'s).
  - The generated code calls `NodeBase.VisitEachChild`, `StatementBase.FlowNodeData`, `ж<ArrowFunction>.AsNode`, … and no such promoted member exists. 533 errors over **429 distinct (type, method) pairs**, all CS1929, in TypeGenerator forwarders and ImplementGenerator adapters.
  - The failing receivers include `TypeNodeBase` (48), `StatementBase` (30), `AccessorDeclarationBase` (16), `NodeBase` (14), and the boxed concrete nodes.
- **Repro status:** the small form (`c29embedshadowdepth`: `Node` → `NodeDefault` redeclaring `Name()` → `NodeBase` → `ExpressionBase` → `Arrow`, through an interface) compiles AND runs correctly (`default default 3`). The real ingredient is not isolated. Candidates:
  - (a) the interface `nodeData` has UNEXPORTED methods (`subtreeFactsWorker`, `setModifiers`, `computeSubtreeFacts`, which together are 137 of the errors);
  - (b) sibling embeds at equal depth that also declare some of the names (`FlowNodeBase`, `DeclarationBase`);
  - (c) the scale of the method set.
- **Not hand-patchable** in this run: 429 pairs in generated code.
- **Downstream:** 58, including `scanner`, `binder`, `checker`, `compiler`, `ls` and `execute`.


### Catalogue summary (re-run)

| Id | Mechanism | Class | Size | Reach | Repro | First seen | Downstream |
|---|---|---|---|---|---|---|---|
| C16 (open) | named `[]byte` vs `~[]byte \| ~string` | gen / golib | S-M | SHAPE | yes (CS0315) | 1st run 3C | 79 |
| GO2CS2004 (open) | defined type over an instantiated generic, **through aliases**; then unsubstituted forwarded field types | converter / gen | M | SHAPE | yes (alias route) | 3B / 3C | 78 |
| **C19** | `&^` constant on a `uintptr` alias | converter | S | SHAPE | yes | 3A | 59 |
| **C20** | `switch` over an array value with composite-literal cases | converter | S | SHAPE | yes | 3A | 17 |
| **C21** | Go `Clone()` on a named slice shadowed by the wrapper's `object Clone()` | go2cs-gen | S | **GENERAL** | yes | 3A | 79 (with C16) |
| **C22** | Δ-renamed type keeps its Go name in a consumer's package-qualified reference | converter | S-M | **GENERAL** | yes (generic route) | 3D | 71 + 19 |
| **C23** | cross-module named `[]byte` from a string constant: u8 span, no conversion | converter / gen | S | SHAPE (pending isolation) | not yet | 3D, 3E | 40 + 19 |
| **C24** | `~map[K]V` type parameter returned as `map[K]V` | converter | S | SHAPE | yes | 3E | 65 |
| **C25** | open-generic pointer → generic interface in a generic body: no adapter | converter / gen | M | SHAPE | yes | 3E | 12 |
| **C26** | `ref`-form pointer receiver escaping into an interface field | converter | S-M | SHAPE | yes | 3E | 19 |
| **C27** | map field constructed as a struct (`new map<…>(nil)`) | go2cs-gen | S | SHAPE | yes | 3E | 12 |
| **C28** | `for` init reassigning a deref'd pointer parameter: statements run together | converter | S | SHAPE | yes | 3F | 58 |
| **C29** | method shadowed across embed depths: not promoted (429 pairs) | go2cs-gen | M-L | SHAPE (gates all of tsc above `ast`) | not yet | 3G | 58 |

**No wrong-answer class was found in the re-run either.**
- Every new row is a compile-time failure.
- Every landed row's small form compiles and prints Go's output: 18 repros.
- The four new repros that compile run correct: C22's non-generic form, C23's same-module forms, and C29's small form.
- C21 looked like a silent-misbinding candidate (`v.Clone()` binding to `ICloneable.Clone`). It cannot reach run time, because the generated pointer forwarder fails to compile first.
- Nothing measured here touches .3's shipped bits. The stdlib corpus does not exercise these shapes: stage 1 is 363/363 at the same tip.


### Closing planning read (re-run)

**Headline: .4's TS rows delivered.**
- Every landed row's first cause is gone at its real site, wherever the build reaches it.
- With no hand-patch at all, the compile frontier moved from 36 to 54 projects. That is more than the first run's best hand-patched pass reached (46).
- With hand-patches for only the two open rows and the new ones, it reaches 79 of 138. That includes tsc's root packages `core`, `collections`, `tspath`, `diagnostics`, `json` and the whole vfs layer, measured here for the first time.
- The first run's closing read predicted "at least one more layer of comparable size". **Scored: RIGHT.** The layer holds 11 new first causes (C19–C29) against the first run's 17 rows, and another layer is certainly behind `ast`.

**What gates now.**
- **C29 is the single wall in front of the rest of tsc** (`scanner`, `binder`, `checker`, `compiler`, `ls`, `execute`: 58 projects). Its small form compiles and runs correctly, so isolating it is the first job. The likeliest ingredient is an interface with UNEXPORTED methods satisfied through the shadowing embed chain, because 137 of its errors are the unexported `subtreeFactsWorker`/`computeSubtreeFacts`/`setModifiers`. It cannot be hand-patched (429 pairs in generated code). tsc's node model is built entirely on this embed-and-override pattern, and so are many Go ASTs and UI trees.
- **C22 is the largest GENERAL row.** It has 1,141 references in this closure, and 42 recorded Δ renames across 21 packages, so it recurs throughout the packages behind `ast`. It is a converter fix at a single seam: qualified references must consult the producing package's `GoTypeAlias` map, as the alias form already does.
- **C21 is a cheap GENERAL row** at one go2cs-gen site: the wrapper's `ICloneable.Clone` must not take the Go name `Clone`, for example by implementing it explicitly. It blocks any module whose named slice has a `Clone` method.
- **GO2CS2004 is not finished.** The refusal must see through aliases (`types.Unalias`), and the real fix (substituted forwarded field types, a `ΔClone` for a generic struct) is what lets `go-json-experiment/json` and every json-v2 user through. With C16, it gates 79.

**What a .5 needs, in order.**
1. The GENERAL rows and the two open rows: C21, C22, C16, GO2CS2004 (alias-aware refusal, then the real fix), and C23 once its ingredient is isolated. Each one is a red-first behavioral project built from the repros here.
2. **Isolate C29, then fix it.** It is the gate to tsc's checker, the package that holds most of tsc's reflect, iter and generics weight.
3. The SHAPE rows with ready repros: C19, C20, C24, C25, C26, C27, C28. Each is S to M, at one converter or go2cs-gen site.
4. **Re-run this experiment a third time at the same TS commit.** The next layer is `scanner`/`binder`/`checker`/`compiler`, and stage 4 (tests) needs `testutil` + `ast` + `checker` at least. Expect one more layer of first causes before any tsc test host runs. With C29 fixed and the GENERAL rows in, a stage-4 attempt on the `ast`-level packages' own tests (`ast`, `scanner`, `tspath`, `core`, `collections`) becomes realistic in that run.

**For or against 1.27 first.** The first run's argument still holds. Every wall in this layer is on the 1.26 commit and would persist on a 1.27 base, and C29/C22 sit in tsc's own packages, which a 1.27 hop would only make larger, through generic methods. Fixing on the current line comes first.

**Box-sharing datum, confirmed.** With COORD holding its batteries, stage 2 took 355 s, against 2,429 s beside COORD's batteries in the first run (about 7×). The stdlib build took 1,210 s against 1,404 s. The bandwidth note in the first record stands: schedule heavy experiment work in a held window.

### Data (re-run)

All under `EXPERIMENT-ts-readiness-2026-11-07-data/`:
- `rerun-stage2-convert-log.txt`: the stage-2 conversion log.
- `rerun-stage3-A-errors.txt`: the as-is build's error histogram.
- `rerun-stage3-G-errors.txt`: the final pass's histogram, with C29's (type, member) tallies.
- `rerun-stage3-passes-status.txt`: built / failed / blocked for every pass, with downstream counts per failing project.
- `rerun-r2{b,c,d,e,f,g}_patch.py.txt` and `rerun-handpatch-summary.txt`: the hand-patches, their order and their footprint.
- `rerun-repro.sh.txt` and `rerun-repro-results.txt`: every repro module run end to end: Go oracle → convert → build → run → compare.
- `rerun-projstatus.py.txt`: the built / failed / blocked classifier. It reproduces the first run's recorded pass-C count, 46 built / 7 failed, as its control.
- New repro modules under `repros/`: `c19*`, `c2*`, `go2cs2004alias`, `go2cs2004direct`. They use their own numbering, which matches the catalogue ids above.

## Amendment 2026-11-09 (i7), later: the parse demo, built as far as the tree allows

The owner OK'd (2026-11-09) adding one Go file to a **scratch copy** of TypeScript's tree, so that a demo can import `tsc/internal/...`. The copy is `tsc/` without `testdata/`, outside the TypeScript clone. The clone itself is unchanged, and nothing from either copy is committed.

**The demo.** `cmd/parsedemo/main.go` is 40 lines (data: `demo-parsedemo-main.go.txt`).
- For each `.ts` file named, it calls `parser.ParseSourceFile`.
- It prints every parse diagnostic (code, start, length, message), then a pre-order dump of node kinds with `[pos,end]`.
- The output is meant to be byte-compared between the native and the converted run, so node kinds and positions have to match exactly.
- The inputs are one valid file and one with syntax errors (`demo-samples.txt`).

**Native (the oracle):** go1.26.10, `GOTOOLCHAIN=local`, `GOWORK=off`.
- `go vet` is clean, and the run exits 0 with 211 lines (`demo-native-output.txt`, SHA-256 `3d40bddc…02ae`).
- The valid file has no diagnostics.
- The bad file has 5: TS1109, TS1005 ×2, TS1109 and TS1128.

**Converted.**
- `cmd/parsedemo` was converted on its own with the pinned `go2cs.exe` (built at `f09e762659`): rc 0 in 5 s, no warnings.
- The emission reads like the Go source (`demo-converted-main.cs.txt`).
- The project was placed in the pass-G tree. Its four internal project references were re-pointed to that tree's layout, because a standalone conversion guesses them. That is wiring, not a converter finding: a module `-recurse` emits the references itself.

**Build:** rc 1 in 1,647 s, run beside COORD's batteries (`demo-build-result.txt`).
- **23 of the parse closure's 26 non-std packages build.** That is exactly the number the demo-distance read predicted from the re-run's logs.
- `ast` fails with 533 CS1929. The distinct (site, code, message) set is **identical** to pass G's `ast` errors, so this is C29 and nothing else.
- `scanner`, `parser` and the demo itself are blocked behind `ast`.

**Read.**
- The demo adds no new first cause. What it adds is a native oracle and a ready harness at the parser's real entry point.
- A converted parse of a real `.ts` file is gated by C29, then by whatever `scanner` and `parser` hold, which nothing has measured yet.
- Once C29 lands, the next step is mechanical:
  1. reconvert;
  2. apply only the hand-patches for rows still open;
  3. build `cmd/parsedemo`;
  4. compare against `demo-native-output.txt`.
- The same binary can then be pointed at the 17,424-unit parser corpus, one digest line per file, for a wider byte-compare.
