# CENSUS — the `-tests` production alias rewrite: the axis is the COMPILATION UNIT, not the host

**Point-in-time record, 2026-09-22, lane C1 (cloud/linux).** A SIZING. No cut. Every claim is a
measurement on this box or a read of source at the named tree. No .NET leg, no BUILD, no row, no gate —
this box has no .NET SDK and no PowerShell.

## Verdict up front

The `Δio` emission is **REAL and REPRODUCED**. The stated mechanism is **REFUTED**: it is not the
host's GOOS, not the host's build-tag set, and not the target. A production conversion on this **linux**
host reproduces the committed **windows** host-of-record **byte for byte**. What moves the bytes is
`-tests` — one import-alias rename map is computed per RUN and applied to two compilation units whose
reference closures differ.

## Setup

Tree `c6fdbe73c3` (the campaign tip, `claude/version-go1.24.13`), converter built from
`c6fdbe73c3:src/go2cs` with `GOTOOLCHAIN=go1.24.13`; `go version` = `go1.24.13 linux/amd64`;
`GOROOT` exported exactly as `go env GOROOT` prints it
(`…/toolchain@v0.0.1-go1.24.13.linux-amd64`). Master read at `9e12c3e7d0` (`git ls-remote`).

Four separately seeded roots, **all seeds taken before any arm converted**, each `src/core` copied with
`bin`/`obj`/`Generated` pruned plus `src/version.props` and `docs/validation`, **3,907 `.cs` per seed on
all four**; one conversion per root, run strictly serially, `pgrep` asserted no converter alive before
each.

| arm | invocation | target |
|:--|:--|:--|
| A | `-stdlib -comments -platforms linux/amd64` | linux (this host's default) |
| B | `-stdlib -comments -platforms windows/amd64` | windows, from the linux host |
| C | `-tests -comments -test-action convert -platforms linux/amd64` | linux |
| D | `-tests -comments -test-action convert -platforms windows/amd64` | windows, from the linux host |

All four `rc=0`. Write evidence: 5 `.cs` written in `regexp/` per production arm, and `regexp.cs` is
among them (mtime +21 s on the seed) — so the production readings below are an EMISSION, not a seeded
survivor.

## (1) Reproduction

| comparison | result |
|:--|:--|
| A (linux target) `regexp.cs` vs **committed** | **byte-identical** |
| B (windows target) `regexp.cs` vs **committed** | **byte-identical** |
| A vs B | **byte-identical** |
| C (`-tests`, linux) `regexp.cs` vs committed | **DIFFERS**, `+6/−6` |
| D (`-tests`, windows) `regexp.cs` vs committed | **DIFFERS** |
| **C vs D** | **byte-identical** |

C and D being byte-identical is the discriminator: with the host held fixed and the target flipped, the
emission does not move. The committed bytes came from a windows host and this linux host reproduces them
on the production path. **The host axis is empty for this file; the target axis is empty for this file.**

The confirmed difference, `regexp.cs:67` and five use sites (6 lines, not the 4 reported — stated as
measured):

```
-using io = io_package;            +using Δio = io_package;
   …and Δio.RuneReader at :496, :539, :558, :888, :1096
```

Production files the `-tests` run rewrote in `regexp/`: `regexp.cs` (+6/−6), `exec.cs` (+6/−6),
`backtrack.cs` (+1/−1). The rest of that run's churn is TEST artifacts it is supposed to write, which
are nonetheless tracked and so count toward the 41: `all_test.cs` (+19/−3),
`package_info_internal_test.cs` (+33/−3), `package_test_info.cs` (+37/−0), `regexp.tests.csproj`
(+45/−17), `go2cs_test_host.cs` (+2/−2), `onepass_test.cs` (+2/−0). **Splitting the 41 into the
production defect and the tracked-test-emission churn is worth doing before anything is cut.**

## (2) The mechanism

**Not the host.** The loader's platform comes from the TARGET at every load site —
`stdLibConverter.go:272-279` (`GOOS=`/`GOARCH=` from `options.targetPlatform`), and the same shape at
`conversionDriver.go:135`, `linknameOperations.go:158-160`, `moduleConverter.go:170`,
`refLoweringCensus.go:270-271`. `computeImportAliasRenames` receives
`goosOfTarget(options.targetPlatform)` at both call sites (`conversionDriver.go:445`,
`autoSiblingOperations.go:68`). The only `build.Default` fields read anywhere are `GOROOT` and `GOPATH`
(`importAliasOperations.go:138`, `importOperations.go:1169`, `main.go:232`), both pinned at
`main.go:291-292`; **`build.Default.GOOS` is never read**.

**The real mechanism**, in one chain:

1. `computeImportAliasRenames` (`importAliasOperations.go:81`) folds `siblingClosureImportPaths` into
   the collision closure at `:103-106` — "the `_test.go` half under `-tests`, so the namespace maps
   describe the ASSEMBLY the emitted C# compiles into".
2. That variable is populated by `collectSiblingTestClosure` (`testConversion.go:1591` reset, `:1642`
   set) with the test half's transitive closure.
3. `collides(name)` is true iff `packageChildNamespaces[packageNS + "." + name]` — i.e. some closure
   path carries `name` as a **non-final** leading segment; then
   `packageImportAliasRenames[name] = Δ + name` (`:169-171`).
4. `regexp`'s production closure contains **zero** `io/*` paths (measured: `go list -deps regexp | grep
   '^io/'` = 0). Its test closure reaches `os`, whose deps contain `io/fs` (measured). So `go.io`
   becomes a child namespace only under `-tests`, and the direct import `io` renames.
5. `packageImportAliasRenames` is a package-level **global**, computed once as a whole-package pre-pass
   before any file emits, so **both** units get the test unit's answer.

**Both answers are individually correct, and the committed tree proves it.** `regexp.csproj` references
golib, bytes, io, regexp/syntax, slices, strconv, strings, sync, unicode, unicode/utf8 — and
`core/io/io.csproj` itself references only errors, golib, sync, so **`io/fs` is not in the production
assembly's closure** and bare `using io` is correct there. `regexp.tests.csproj` **does** reference
`core/io/fs/io.fs.csproj`, so `Δio` is REQUIRED there. And the test assembly compiles only `*_test.cs`
(its `<Compile>` items) while **referencing** `regexp.csproj` for the production half — so a production
`.cs` lands in exactly one assembly, the one where the bare spelling is right. Committed corroboration
one package over: `strings/reader.cs:7` carries bare `using io`, `strings/reader_test.cs:8` already
carries `using Δio`.

**Severity, stated plainly.** The rename is applied consistently to the alias and every use, so the
emission **still compiles**. This is a determinism and visual-fidelity defect plus tracked-corpus churn
— the committed bytes depend on which driver last ran — and not a compile break. The fidelity cost is
real for a project whose premise is that a Go developer can read the C#: Go reads `io.RuneReader`, the
`-tests` emission reads `Δio.RuneReader` with no collision to justify it.

## (3) Corpus reach

Census over the **345** std packages at the pin, porting the converter's own predicate (a direct
import's name colliding with a child namespace of the closure; production closure vs
production ∪ test closure). **21 packages flip**:

`bufio` · `bytes` · `crypto` · `flag` (`os`, `runtime`) · `hash` · `image` · `iter` · `log` ·
`math/big` (`rand`) · `net` (`os`) · `os` · `reflect` · `regexp` · `runtime` (`math`) · `strings` ·
`sync` · `syscall` · `testing` · `time` · `unique` · `weak` — all `io`→`Δio` except where named.

Both controls hold:

- **Positive** — the census predicts `strings io→Δio`; measured, committed `strings/reader.cs:7` is
  bare and the `-tests` emission renames it. Predicted before it was run.
- **Negative** — the census predicts no flip for `sort`; measured, **zero** production `.cs` differ
  after a `-tests` conversion of `sort`.

**Two stated bounds, both one-directional.** The census uses Go's `Deps` as the production-closure
proxy, while the converter also folds `corpusReferenceClosure` read from the csprojs on disk; for the
`io` case the two agree, but a package whose csproj closure is WIDER than Go's deps could flip without
the census seeing it — so **21 is a lower bound**. And the package count is not the file count: each
flipping package rewrites every production file naming the colliding import (`regexp`: 3 of 5).

## (4) The fix's shape

1. The rename set must be per **COMPILATION UNIT**, not per run. Target-driven is already true; the
   missing axis is the unit.
2. Compute it twice — once with `siblingClosureImportPaths` empty (the production unit), once with it
   folded (the test units) — and select by the unit each file belongs to.
3. Minimal edit: `computeImportAliasRenames` already takes `files`; give it an
   `includeSiblingClosure bool` (or have it return both maps) and point the emitter at the right map
   around each unit's file loop instead of reading the two globals.
4. Alternative worth COORD's ruling, which deletes the class instead of making it correct: a `-tests`
   run does not re-emit production `.cs` at all, the production emission of record being `-stdlib`'s.
   Bigger behavioural change; the `-test-action build` path needs checking, though a seeded root
   already carries the production files.
5. Guard, red-first: convert one flipping package with `-tests` and assert its production `.cs` are
   byte-identical to a `-stdlib` emission of the same package at the same tree. Regress by folding the
   sibling closure into the production unit and confirm the guard names `regexp.cs`.

**Is it C1's seat?** The converter change is Go-side, suite-gateable, and its proof is an emission read —
so yes on the Go side. The .NET legs (the rows past BUILD, stdlib 344, CNR) belong to a hardware lane.
The choice between (3) and (4) is COORD's before anything is cut.
