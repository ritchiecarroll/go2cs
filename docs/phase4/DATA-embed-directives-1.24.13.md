# DATA — `//go:embed` at go1.24.13: census, embedtest's expectations, and a design

> **A point-in-time RECORD** (docs/docs-records.md doc-type: DATA). Amended with dated blocks, never
> rewritten, never executed from. Produced by C2 on COORD's standby order (ledger 2026-09-22,
> `769fc17fb1` item (2)). **No converter cut rides this ref** — it is the sizing the ruling asked for.
>
> Read at: GOROOT `go1.24.13` (`GOTOOLCHAIN=go1.24.13`, `go version` = go1.24.13 linux/amd64) ·
> corpus `claude/version-go1.24.13` **0adf2e4318** · roster `docs/ValidatedTestPackages.md` at master
> **3b48e0c8e0**. Every count below is derived by the tool named beside it, not quoted.
>
> ⚠ Every emission statement here is a **READ**. This box has no .NET and no PowerShell; nothing in
> this record was compiled.

## 0. The finding in one paragraph

The converter has **no `//go:embed` support at all**. It preserves the directive as a comment and
emits the variable **uninitialized**; no `EmbeddedResource` item exists anywhere in `src/core`, and
no embedded payload file has ever been copied into the corpus. Two consequences, and the second was
not previously on the board: `crypto/internal/fips140test`'s `TestACVP` reads a zero-byte config
(C1's classing, `769fc17fb1`), **and** `internal/trace/traceviewer` — a **production** package — ships
`internal static embed.FS staticContent;` with nothing behind it, so `http.FS(staticContent)` serves
an empty tree in a shipped assembly. `embed/internal/embedtest` compiling at batch-3 ref 6 says
nothing about either: compiling is not correctness.

## 1. CENSUS — every `//go:embed` under `GOROOT/src` at go1.24.13

**Method.** `go/ast`, not a line scan: a directive may be indented, may repeat over one variable, and
its patterns may be quoted or back-quoted (`go/build`'s own `parseGoEmbed` shape). The tool walks
every `.go` file with `parser.ParseComments`, reads the directive off the `GenDecl`'s or the
`ValueSpec`'s own doc group, and takes the declared type from the AST. Pattern kind is decided by
`os.Stat` against the package directory, so `dir` versus `file` is measured, not inferred from the
spelling.

**Reconciliation, stated because the two obvious greps disagree.** A line-start grep
(`^//go:embed`) finds **34**; an indent-tolerant one (`^[[:space:]]*//go:embed`) finds **43**; the
census finds **41 declared variables** over **44 patterns**. All three are reconciled per file:
one of the 43 is not a directive at all — `go/build/read_test.go:302`, inside a raw-string literal in
a test table, which the AST correctly never sees — and one variable in
`embed/internal/embedtest/embed_test.go` carries **two** `//go:embed` lines. So: **42 real directives
over 41 variables in 27 files**, and the line-start grep misses 8 of them, `fips140test`'s among them.

**Split.** 22 of the 41 are under `cmd/vendor/...` (the toolchain's vendored `x/tools` analysers and
`pprof`), which the converted stdlib corpus does not contain. They are uniform and dull: all
production, 21 `string` over a single file, 1 `embed.FS` over a directory.

**The 19 that are corpus-eligible:**

| package | site | half | declared type | var | patterns (kind) |
|---|---|---|---|---|---|
| `crypto/internal/fips140test` | acvp_test.go:85 | internal-test | `[]byte` | `capabilitiesJson` | `acvp_capabilities.json` (file) |
| `embed` | example_test.go:14 | external-test | `embed.FS` | `content` | `internal/embedtest/testdata/*.txt` (glob) |
| `embed/internal/embedtest` | embed_test.go:18 | internal-test | `embed.FS` | `global` | `testdata/h*.txt` (glob) `c*.txt` (glob) `testdata/g*.txt` (glob) |
| `embed/internal/embedtest` | embed_test.go:21 | internal-test | `string` | `concurrency` | `c*txt` (glob) |
| `embed/internal/embedtest` | embed_test.go:24 | internal-test | `[]byte` | `glass` | `testdata/g*.txt` (glob) |
| `embed/internal/embedtest` | embed_test.go:84 | internal-test | `embed.FS` | `testDirAll` | `testdata` (dir) |
| `embed/internal/embedtest` | embed_test.go:101 | internal-test | `embed.FS` | `testHiddenDir` | `testdata` (dir) |
| `embed/internal/embedtest` | embed_test.go:104 | internal-test | `embed.FS` | `testHiddenStar` | `testdata/*` (glob) |
| `embed/internal/embedtest` | embed_test.go:144 | internal-test | `[]T` | `helloT` | `testdata/hello.txt` (file) |
| `embed/internal/embedtest` | embed_test.go:146 | internal-test | `[]uint8` | `helloUint8` | `testdata/hello.txt` (file) |
| `embed/internal/embedtest` | embed_test.go:148 | internal-test | `[]EmbedUint8` | `helloEUint8` | `testdata/hello.txt` (file) |
| `embed/internal/embedtest` | embed_test.go:150 | internal-test | `EmbedBytes` | `helloBytes` | `testdata/hello.txt` (file) |
| `embed/internal/embedtest` | embed_test.go:152 | internal-test | `EmbedString` | `helloString` | `testdata/hello.txt` (file) |
| `embed/internal/embedtest` | embedx_test.go:22 | external-test | `embed.FS` | `global` | `testdata/*.txt` (glob) |
| `embed/internal/embedtest` | embedx_test.go:25 | external-test | `string` | `concurrency` | `c*txt` (glob) |
| `embed/internal/embedtest` | embedx_test.go:28 | external-test | `[]byte` | `glass` | `testdata/g*.txt` (glob) |
| `embed/internal/embedtest` | embedx_test.go:31 | external-test | `string` | `sbig` | `testdata/ascii.txt` (file) |
| `embed/internal/embedtest` | embedx_test.go:34 | external-test | `[]byte` | `bbig` | `testdata/ascii.txt` (file) |
| `internal/trace/traceviewer` | http.go:418 | production | `embed.FS` | `staticContent` | `static/trace_viewer_full.html` (file) `static/webcomponents.min.js` (file) |
**Distribution over those 19.** Halves: 1 production, 12 internal-test, 6 external-test. Declared
types: `embed.FS` ×5, `string` ×3, `[]byte` ×4, and **7 named or aliased element types** —
`[]T`, `[]uint8`, `[]EmbedUint8`, `EmbedBytes`, `EmbedString` (`embedtest`'s `TestAliases`), which the
spec permits and which any initializer must satisfy. Pattern kinds: 9 file, 8 glob, 2 dir.

**No `all:` prefix exists anywhere in `GOROOT/src` at 1.24.13** (grep over the whole tree: zero hits).
It is part of the spec and of `embed`'s own doc comment, but **no corpus row can reach it**, so it is
a design obligation with no gate behind it — say so rather than claim coverage.

**Roster position** (master `3b48e0c8e0`):

- `crypto/internal/fips140test` — a **BANKED row** (2,196 tests, 5 disclosures) and the H10 re-bank's
  principal for the retiring `crypto/internal/alias` and `crypto/internal/nistec` anchors. Its
  `acvp_test.go:84` embed is the live blocker on a row that must re-bank at the hop.
- `internal/trace` — a **BANKED row** (92). The embed is **not** in it: it is in
  `internal/trace/traceviewer`, which has **no row of its own**. So no roster row is red from it
  today — this is a **latent production defect**, not a row blocker, and it should not be counted as
  one.
- `embed` and `embed/internal/embedtest` — **no rows**; `embedtest` is a candidate that batch-3 ref 6
  makes COMPILE.

**Payload sizing.** `internal/trace/traceviewer/static/` is **absent from `src/core`** — the two
embedded files are `trace_viewer_full.html` (2,618,942 B) and `webcomponents.min.js` (118,419 B).
Supporting the production case therefore adds ~2.7 MB of binary payload to the corpus, which is a
decision for COORD and not a detail: it is larger than most converted packages.

## 2. READ — what `embedtest` asserts, and what `src/core/embed` already is

**`src/core/embed/embed.cs` at 0adf2e4318 is a COMPLETE auto-conversion, not a stub** (483 lines).
`FS` carries the real `files ж<slice<file>>` layout; `split`, `lookup`, `readDir`, `Open`, `ReadDir`,
`ReadFile`, `openFile` (with `Read`, `Seek`, `ReadAt`, `Close`) and `openDir` are all present, and
`FS` is recorded against `fs.ReadDirFS` and `fs.ReadFileFS`. **Nothing about the runtime is missing.**
What is missing is *construction*: no code path ever populates `files`, and no string/`[]byte`
variable is ever given bytes. `file.hash` (16-byte truncated SHA-256, written by the Go linker) is
**declared and never read** anywhere in the converted package — an initializer may leave it zero, and
that is a fact about this code rather than an assumption.

**The seven test functions** (`embed_test.go`: `TestGlobal`, `TestDir`, `TestHidden`,
`TestUninitialized`, `TestAliases`, `TestOffset`; `embedx_test.go`: `TestXGlobal`) assert, in the
order they bite:

1. **Content**, via `ReadFile`/`Open`+`Read` against exact strings, for `embed.FS`, `string` and
   `[]byte` vars alike.
2. **Directory shape**, via `ReadDir`: `testDir(all, ".", "testdata/")` — the tree's own top entry is
   an FS entry with a trailing slash — then `testdata/i` → `i18n.txt`, `j/`, and so on down.
3. **THE HIDDEN RULES, which are the whole difficulty.** `//go:embed testdata` (a directory pattern)
   yields `-not-hidden/ ascii.txt glass.txt hello.txt i/ ken.txt` — `.hidden` and `_hidden` excluded.
   `//go:embed testdata/*` (a glob) yields those **plus `.hidden/` and `_hidden/`**, because the
   glob's own level matches them — but *within* `testdata/.hidden` the walk is hidden-excluding again:
   `fortune.txt`, `more/`, and **not** `.more` or `_more`. One pattern kind, two walk rules, one level
   apart. Empty directories are dropped (`embed`'s doc comment).
4. **The zero value is a valid empty FS** (`TestUninitialized`): `.` must be a directory and
   `ReadDir(".")` must be empty. The converted code already does this through `dotFile`.
5. **Named element types** (`TestAliases`): `[]T`, `[]uint8`, `[]EmbedUint8`, `EmbedBytes`,
   `EmbedString` must all carry `testdata/hello.txt`'s bytes.
6. **Offsets** (`TestOffset`): `Read`, then `Seek`, then `ReadAt` on one open file — already
   implemented.
7. **Both halves embed** (`TestGlobal` internal, `TestXGlobal` external) over *overlapping* patterns.
   Under the recompile and white-box models both halves compile into ONE assembly, so the two `global`
   variables must not share a resource identity.

## 3. DESIGN — how the converter should emit an embedded variable

1. **Bytes: `<EmbeddedResource Include="<pattern-resolved path>" LogicalName="go.embed/<import-path>/<slash-relative-path>" />`.**
   Deterministic, cwd-independent, and it survives single-file publish and Native AOT — which is the
   contract `//go:embed` actually makes.
2. **Rejected alternative (B): the existing fixture mechanism**, `<None … CopyToOutputDirectory="PreserveNewest" ExcludeFromSingleFile="true" />`
   (`testConversion.go:4335`). It is already built and would be the cheap answer, and it is wrong for
   this: it depends on the working directory, `ExcludeFromSingleFile` is the exact negation of embed's
   contract, and a NuGet-consumed `go.internal.trace.traceviewer` would not carry the files at all.
3. **Rejected alternative (C): emit the bytes as C# literals** (UTF-8 literal or base64). Correct and
   dependency-free for a small file, but `trace_viewer_full.html` alone would put a 2.6 MB `.cs` in the
   corpus — CNR diffs, compile time, and review all pay. Not worth a second mechanism to maintain.
4. **Initializer: a static field initializer at the declaration**, not a module initializer — it keeps
   the emission beside the variable (so the C# still reads like the Go), and it is what Go's "already
   initialized before any code runs" means in a package class's `.cctor`.
5. **THE PATTERN WALK HAPPENS AT CONVERSION TIME, NOT AT RUNTIME.** The converter resolves globs and
   directory patterns against the real tree it is already reading, applies the hidden rules of §2.3 and
   the `all:` variant, drops empty directories, synthesizes the directory entries, and emits the file
   list **already in Go's `(dir, elem)` order** — the order `embed.cs`'s own struct comment specifies.
6. This is the load-bearing reason to split it this way: the hard part is Go's walk and ordering, and
   putting it in the converter makes it **armable in Go alone**, with no .NET on the critical path. The
   C# side is then a thin constructor with nothing subtle in it.
7. **The C# helper is a hand-owned companion inside the converted `embed` package**
   (`src/core/embed/embed_impl.cs`), because `FS.files` is unexported and only a file in that package
   class can set it. That is an existing hand-own kind (CLAUDE.md, *One tree*), not a new mechanism.
8. Emitted shape, production case:
   `internal static embed.FS staticContent = embed.ΔEmbedFS(typeof(traceviewer_package).Assembly, "go.embed/internal/trace/traceviewer/", [...names in order...]);`
   and for the scalar cases `embed.ΔEmbedString(...)` / `embed.ΔEmbedBytes(...)`, with the converter
   emitting its usual conversion for a named element type (`EmbedString`, `[]T`, …).
9. **Production vs test half.** Production items go in the package's own `.csproj` under the package's
   import path. Test-half embeds go in the `.tests.csproj`, and because both halves compile into one
   assembly the logical-name prefix must carry the variant (`…/<import-path>_test/`). `embedtest`
   embeds overlapping patterns in both halves, so the same bytes are embedded twice under two names —
   which is faithful: in Go they are two packages with two embeddings.
10. **The seam: go2cs-gen is not involved at any point.** No attribute, no record, no generated
    partial. The whole seat is converter emission + csproj items + one hand-owned companion, so it is
    gate-able by the Go suite and an emission read, with only the companion needing a build.
11. **One hook that must not be missed:** the embedded payload files have to join `testInputDigest` /
    the staleness inputs, or changing an embedded file will not invalidate a conversion.
    `converterStaleness.go` tracks only the converter's own assets today.

**Preferred: (1) + (4) + (5)/(6) + (7).** The reason is (6): it puts every rule that can be gotten
subtly wrong — the hidden-file asymmetry, `all:`, empty directories, the `(dir, elem)` order — on the
side of the seam that this lane can arm red-first and gate with the suite, and leaves the side that
needs hardware as a constructor with no decisions in it.

## 4. What this record does NOT establish

- Nothing here was compiled. The `embed.cs` reading is a source read at `0adf2e4318`.
- `all:` has no corpus instance, so any implementation of it ships ungated by a row.
- Whether the ~2.7 MB `traceviewer` payload should enter the corpus at all is COORD's call, not sized
  away here.
- The census covers `GOROOT/src` only. Third-party modules reached through `-recurse` are not in scope
  and were not walked.
