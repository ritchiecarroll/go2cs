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
