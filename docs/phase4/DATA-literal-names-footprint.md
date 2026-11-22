# DATA: the literal-name seat's footprint, predicted before the cut

Seat `claude/c1-literal-names` (C1), cut on train 4's union `62447ae8e2`. The owner ruled option (a) on 2026-11-22 (COORD
171256Z): every function literal is emitted with `GoFunc("<Go name without the package>")`. This record holds the
per-flavour prediction of the seat's corpus and behavioral footprint. It is committed before any converter, golib or
generator change of the seat, and the two-seeded regen is scored against it below the line, appended and never edited.

## What the change emits

1. Every function literal the converter emits gains `GoFunc("…")` at the head of its lambda or local function, in one
   attribute list with `MethodImpl(MethodImplOptions.NoInlining)` where the literal already carries that mark:
   `[GoFunc("outer.func1")] () => …`, `[MethodImpl(MethodImplOptions.NoInlining), GoFunc("callLine.func1")] () => …`.
   The text is added on the line the lambda starts on, so each such line changes and no line is added or removed.
2. The line half: an immediately invoked literal's call (`func() { … }()`) is counted in GoPositionMap's per-call table
   as `Invoke`, where before the whole statement went undescribed. Only a statement whose `}(` sits on a later line than
   the statement itself gains an entry, so only the record of that statement's Go file changes.

Nothing else changes: no line is added or removed, so the position table and the literal-span table of every record
stay byte-identical, and no `.csproj` is touched.

## How the prediction was measured

The census converter is the base converter at `62447ae8e2` with three logging hooks and no other change: at the entry of
`convFuncLit` (every literal the emitter converts), at `visitFile` (the AST literal count of every visited file), and at
the later-line call payload (the payload with and without the `Invoke` rule). The census ran a seeded `-stdlib -comments`
emission once per target, one target at a time, each into its own seeded root (corpus, `version.props`,
`docs/validation`), at `CGO_ENABLED=0` with the default tag set `purego,math_big_pure_go`, under go1.26.11. Every run
exited 0: windows/amd64 279 s, linux/amd64 277 s, darwin/amd64 280 s, linux/arm64 267 s. A literal is counted once by
its Go position.

The behavioral census ran the same converter over a copy of `src/tests/Behavioral`. It converted each of the 1,178
package directories in place, deepest first and with each project's `go2cs.options`, as `check-no-regression.ps1` does.
The 12 windows-exclusive projects took `-platforms windows/amd64`. All 1,178 exited 0, and none read best-effort.

## Prediction: the corpus, per flavour

Each row is that flavour's single-target emission, base converter against seat converter, both seeded from the same
tree. "Literal lines" is every converted `.cs` line that changes, as `-N/+N`: one line per literal. No two literals share
a Go line in any flavour (distinct lines equal literals in all four), so no C# line carries two.

| Flavour | Literal lines in `.cs` | `.cs` files | Packages | of them with NoInlining | `.cs.auto` lines / files | `package_info.cs` | `.csproj` |
|---|---|---|---|---|---|---|---|
| windows/amd64 | 1,662 | 385 | 153 | 17 | 22 / 7 | 4 | 0 |
| linux/amd64 | 1,668 | 399 | 154 | 17 | 18 / 6 | 5 | 0 |
| darwin/amd64 | 1,658 | 387 | 153 | 17 | 17 / 5 | 5 | 0 |
| linux/arm64 | 1,667 | 398 | 153 | 17 | 18 / 6 | 5 | 0 |

- **`package_info.cs`, one record line each**, in the package of the Go file named. Every flavour moves the four in
  `crypto/internal/fips140/hmac` (`hmac.go:173`), `go/types` (`named.go:676`), `internal/buildcfg` (`exp.go:31`) and
  `net/http/cgi` (`host.go:38`). The three non-windows flavours also move `runtime` (`signal_unix.go:1257`), which
  windows does not compile. Each line gains one `Invoke/1/1/<delta>` entry in the record's per-call argument (with an
  empty method-value argument ahead of it where the record had none) and nothing else.
- **`.cs.auto` review siblings** carry the attribute too, since a hand-own's sibling is emitted by the same visit. They
  are `internal/godebug/godebug.go`, `internal/sync/hashtriemap.go`, `runtime/mfinal.go`, `sync/oncefunc.go` and
  `sync/waitgroup.go` on every flavour, plus `net/lookup_windows.go` and `syscall/exec_windows.go` on windows and
  `os/wait_waitid.go` on the two linux flavours. A sibling carries no record, so none of these moves a `package_info.cs`.
- **Literals the change does not reach.** The visited files hold more literals than the emitter converts: 58 more on
  windows, 53 on linux/amd64, 54 on darwin, 53 on linux/arm64. Every one is in a function whose body a hand-own
  displaces: `runtime/proc.go` 16, `reflect/swapper.go` and `internal/reflectlite/swapper.go` 9 each, `reflect/type.go`
  4 (linux/amd64 figures), and so on. These functions emit no body, so they emit no literal and nothing changes there.
- **The merged corpus.** In the four-target merge a shared file is flat and counts once, and a per-key file counts in
  its own folder. The corpus diff therefore lies between the largest flavour and the sum of the four, at most 1,668 +
  1,662 + 1,658 + 1,667 literal lines. The per-flavour rows above are the scored claims, read from each target's own
  staging.

## Prediction: the behavioral corpus

- **1,625 literals on 1,613 Go lines in 328 Go files.** 12 lines hold two literals, so 1,613 `.cs` lines change.
- **Goldens:** 323 `.cs.target` files in 321 projects move, by 1,605 lines. The 5 `.cs` files of nested sub-libraries
  move by 8 lines, and a nested package has no golden. Each moved `.cs` moves exactly as its golden does.
- **`package_info.cs`:** 35 files move from the line half, by one record line each (54 statements in 35 Go files; 53
  statements gain one `Invoke` entry, one gains two, `Invoke/1/2/2,Invoke/2/2/2`):
  AppendOfMake, CaptureHoistThroughConversion, CaptureModeFuncLitParam, ClosureDefer, DeferArgEnclosingCapture,
  DeferFinallyLowering, DeferFrameScopes, DeferLambdaParam, DeferPanicArg, DeferValueFieldPtrReceiver,
  EmbeddedPointerNilAssign, ErrorMethodPanicFormat, ErrorfFormatting, FieldAccessReinterpret, GenericResultLambdaInfer,
  ImmediatelyInvokedFunc, InteriorPointerShapes, LineMarkerDeferEndTC0, LiteralFrameNames, NestedFieldElementAddr,
  NilReceiverNormalization, PackageVarFuncLitTypeLift, PackageVarMethodValue, PanicDeferCalleeFrame, PanicRecover,
  ParenIifeNilFuncConv, PointerFieldSelectors, PointerReinterpretIdentity, PromotedMethodExpr, RecoverInHelper,
  ReflectNamedFuncAssign, SetFinalizerBridge, ShadowedCompoundAssign, TypedErrorAssertThroughAdapter and
  ValueReceiverArgumentOrder. No golden is a `package_info.cs`.
- **`.csproj`:** 0.
- **Base drift that is not this seat's:** on this linux host the BASE converter's emission differs from the committed
  tree in 4 of 3,609 files: `main.cs` and `package_info.cs` of DeltaNameEdgeCases and of NamedSliceMethodShadow. The
  committed files keep a holder class where the base emits the `⁔` suffix form. The seat's rebaseline applies only its
  own lines to those files and leaves the rest as committed.

## Falsifiers

- Any moved line that is not a literal's first line, or a `package_info.cs` record line outside the five named
  packages (corpus) or the 35 named projects (behavioral).
- Any change to a record's position table (its third argument) or its literal-span table (its fourth), which would mean
  a line was added or removed.
- Any `.csproj` move.
- An emitted literal without `GoFunc`, read as a count of `GoFunc(` in the seat emission below the literal count. The one
  shape left unnamed on purpose is a literal in a blank function (`func _`); the census did not count those apart, so
  any such literal is named in the scoring.
- A count above or below a row. The census counts a literal at its conversion, so one converted but whose output is
  discarded would read here and not in the emission.

## Limits stated with the prediction

- **The names are the declared ones**, Go's names under `-gcflags=-l`. Go renames a literal by its inlining
  (`main.main.(*T).M.func1`), which no conversion can predict. The behavioral rows mark their functions `//go:noinline`.
- **Var-initializer literals are numbered in initialization order** (`types.Info.InitOrder`), as Go numbers them. This
  was measured across two files: a var declared first whose initializer reads a later one takes the later number. A
  test binary numbers the variant's whole file set. On linux/amd64, of 125 production var-initializer literals in std
  packages with an internal test variant, 0 are numbered differently in the test binary. That is measured on that
  flavour only.

## Correction, 2026-11-22, before the measurement

The `package_info.cs` rows above are wrong in mechanism, found while updating the converter's emission tests and
before any regen ran. They count only the line half. A second record argument also moves: GoPositionMap's
deferred-closure sites (the eighth argument), which record each literal handed straight to `defer` at the C# column of
its `{` and of its first statement. The `{` sits on the lambda's header line, where `GoFunc("…")` now lands, so its
column moves by the attribute's length. Every record with a non-empty eighth argument therefore changes, by those
columns and nothing else. The corrected prediction counts the records in the committed tree, read per flavour (a
package's per-key `package_info.cs` where it has one, else the flat one):

| Flavour | Records with deferred-closure sites | in `package_info.cs` | Line-half files | Overlap | `package_info.cs` moved |
|---|---|---|---|---|---|
| windows/amd64 | 94 | 50 | 4 | 2 | 52 |
| linux/amd64 | 93 | 51 | 5 | 3 | 53 |
| darwin/amd64 | 93 | 50 | 5 | 3 | 52 |
| linux/arm64 | 93 | 51 | 5 | 3 | 53 |

Behavioral: 127 `package_info.cs` carry deferred-closure sites (127 records). With the line half's 35 (24 of them in
both), 138 `package_info.cs` move.

Added falsifiers: a deferred-closure column that moves by anything other than the length of `[GoFunc("<name>")] ` (or
of `, GoFunc("<name>")` where a NoInlining list was already there); a moved site that is a first-statement site rather
than a `{` site on a header line; and a thunk site (the seventh argument) or a bounded-go site that moves at all. The
thunk and bounded-go sentinels sit ahead of any literal on their lines, so neither should move.

## Scoring, 2026-11-22: the two-seeded regen

Measured per flavour: the base converter (`be4a4eae9c`) against the seat converter (`0b44b1e4f1`), each a
single-target `-stdlib -comments` emission into its own root seeded from this tree, one run at a time, every run exit 0.
The prediction above and its correction stand unedited. A "MISS" is stated against the row as worded.

| Flavour | Literal lines in `.cs` | `.cs` files | Packages | with NoInlining | `.cs.auto` lines / files | `package_info.cs` | `.csproj` |
|---|---|---|---|---|---|---|---|
| windows/amd64 | **1,659** (MISS, −3) | 385 | 153 | 17 | 22 / 7 | 52 | 0 |
| linux/amd64 | **1,663** (MISS, −5) | 399 | 154 | 17 | 18 / 6 | 53 | 0 |
| darwin/amd64 | **1,653** (MISS, −5) | 387 | 153 | 17 | 17 / 5 | 52 | 0 |
| linux/arm64 | **1,662** (MISS, −5) | 398 | 153 | 17 | 18 / 6 | 53 | 0 |

- **Literals: met on every flavour.** `GoFunc(` added in `.cs` equals the predicted literal count exactly (1,662, 1,668,
  1,658, 1,667) and in `.cs.auto` the predicted 22, 18, 17, 18. No moved line lacks the attribute.
- **Literal lines: missed on every flavour**, by the same mechanism. The prediction reasoned that literals on distinct Go
  lines land on distinct C# lines. The converter joins some onto one C# line: `crypto/ecdsa` `privateKeyToFIPS` and
  `crypto/ed25519` `PrivateKey.Sign` and `sign` (two literal arguments of one `privateKeyCache.Get` call, every flavour),
  and on the three non-windows flavours `os` `root_unix.go` `chmodat` and `chownat` (a literal and the literal nested in
  it). Each such line gains two attributes; nothing else moved.
- **`package_info.cs`: met** as corrected before the measurement (52, 53, 52, 53), and by mechanism: the deferred-closure
  argument moved in 94, 93, 93, 93 records and the per-call argument in 4, 5, 5, 5; no position table or literal-span
  table moved.
- **Packages** read 154, 155, 154, 153 by C# folder; mapped back to Go import paths (an `amd64/` key folder is not a
  package, `internal/syscall/windows` is one) the sets equal the census's on every flavour.
- **The merged corpus: MISS, not predicted.** The record said no line is added or removed and no `.csproj` is touched.
  Two files moved instead. Go numbers package-level var-initializer literals on one counter over the PLATFORM'S file set,
  so the same literal has a different name per GOOS. Read from `go tool objdump` of each GOOS's `internal/poll` under
  `-gcflags=-l`: `fd.go:83` is `init.func1` on windows and darwin and `init.func2` on linux; `sendfile.go:7` is
  `init.func7`, `init.func3` and `init.func2`. Both files were flat (shared); they now differ by GOOS, and the
  four-target merge (run filtered to `internal/poll`) moves each into `windows/`, `linux/` and `darwin/`. The project
  already compiles `$(GoTargetOS)/*.cs`, so its `.csproj` did not change. Across the four flavours every other path two or
  more flavours move carries identical line pairs. The limit stated with the prediction measured test-binary renumbering
  and never asked the cross-platform question.
- **Applied:** 560 files by line pairs (2,188 lines) plus the `internal/poll` split; marker gate 0 of 225.

**Behavioral** (1,178 packages, same host, base and seat):

- 1,625 attributes added: met. 323 goldens, 5 nested `.cs` by 8 lines, 138 `package_info.cs`, 0 `.csproj`: met.
- Golden lines **1,616** against 1,605: MISS, +11. Of the 12 Go lines holding two literals, the converter emits 11 as
  two C# lines (the inverse of the corpus miss above). Every rebaselined file equals the seat emission byte for byte with
  CRs stripped.

## Prediction, 2026-11-22: the collection fallback (`361c0cac97`), before its measurement

The hosted battery at `e122bcfbb0` found a defect this record never predicted. A C# collection expression whose first
bracket token opens a literal's `[GoFunc(…)]` list is read by Roslyn as an attributed lambda: `internal/profile`'s
decoder tables failed to compile on every leg (CS1001 ×259), as did seven behavioral projects. `361c0cac97` writes such
a collection in the converter's own `new T[]{…}.slice()` / `.array(N)` form. The rule is the first bracket token, so it
also rewrites collections that compiled only because a `new` stopped Roslyn's guess first.

Predicted from the committed tree with the converter's predicate (each collection moves its opening and closing
lines; one written on a single line moves that line):

- **Corpus, every flavour alike** (all three files are flat): `internal/profile/encode.cs` 8 collections on 16 lines,
  `net/http/h2_bundle.cs` 1 on 2, `testing/slogtest/slogtest.cs` 1 on 2. 3 files, 20 lines. No line added or removed, 0
  `package_info.cs`, 0 `.csproj`. Committed `_test.cs` sources are not `-stdlib` emission and are not counted.
- **Behavioral:** 14 projects' `main.cs` and their goldens, 31 lines (DeepEqual, FuncFieldUnexportedType,
  FuncLitArgCapture, FuncLitEmptyInterfaceTarget, FuncTypeVerbs, MethodValueReceiverSnapshot ×2, NilFuncInterfaceReads,
  NilFuncTypeSwitch, PackageVarFuncLitTypeLift, PkgLevelFuncLitLocals, RangeExprFuncLitCapture ×2, SliceBoundsRecover,
  StatementTableFuncNames, TypeSwitchCollapsedFuncCase), plus the nested `LazyPackageVars/tables/tables.cs` by 2. 0
  `package_info.cs`, 0 `.csproj`.
- **Falsifiers:** any other file, a line-count change, any `package_info.cs`, or a collection left as a collection
  expression whose first bracket opens a literal's attributes.
