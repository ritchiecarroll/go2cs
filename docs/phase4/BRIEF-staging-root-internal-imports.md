# BRIEF — the staging-root / internal-import refusal
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

**Point-in-time brief for coordinator ruling**, per the document ladder in `CLAUDE.md`: this is a
record, not a runbook and not doctrine. It is amended with dated blocks, never rewritten, and nothing
is executed from it. Written 2026-08-28 by lane `claude/sibling-testdata-staging` on the i7-5820K,
against Go **1.23.12** and master `dddabef4b`. Every claim below carries the measurement that
produced it; the probe scripts are named where they matter so the numbers can be re-taken.

---

## 1. The refusal, exactly

A converted test host runs its suite in an isolated sandbox. When one of that suite's tests asks the
**real Go toolchain** to compile a staged Go source — `go run ./testdata/testprog/x.go`,
`go build` on a staged harness — the child `go` refuses any `internal/…` import the same test is
permitted under `go test`:

```
package command-line-arguments
        …\testdata\testprog\cpu-profile.go:15:2: use of internal package internal/profile not allowed
```

The rule is `cmd/go/internal/load.disallowInternal` (`pkg.go:1425`). For a standard-library package
(`p.Module == nil`) the whole decision is one directory comparison:

```go
parent := p.Dir[:i+len(p.Dir)-len(p.ImportPath)]          // == $GOROOT/src
if str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent)) { return nil }
srcDir = expandPath(srcDir)                                // filepath.EvalSymlinks
parent = expandPath(parent)
if str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent)) { return nil }
```

`srcDir` is the directory holding the file being compiled. Under `go test` that directory is
`$GOROOT/src/<pkg>/testdata/…` and the prefix test passes; under the converted host it is the
sandbox, and it does not. **This is an environment-fidelity gap, not a conversion defect** — the same
category as the working-directory class that `DESIGN-package-ancestry-view.md` closed on 2026-08-13.

**There is no escape flag, and this was verified rather than assumed.** Reading `disallowInternal`
whole, the only allowances are: a `gccgo` compiler, an importer path prefixed `bootstrap/`,
`testing/internal` imported by `testmain`, an EMPTY importer path (the internal package itself named
on the command line), and the directory / module-prefix tests above. No flag, no environment
variable, no `GOFLAGS` spelling reaches it. `GONOSUMDB`/`GOPRIVATE`/`GOFLAGS` are unrelated
machinery.

---

## 2. Membership, measured today — and two corrections to the commissioning list

The class was commissioned as four members: `internal/godebugs`, `io/ioutil`, `go/build`, and
`internal/trace`'s `TestTraceCPUProfile`. Re-measured against the current roster and board:

| package | state today | in this class? |
|---|---|---|
| `internal/godebugs` | **banked, 1 of 1** (roster line 275) | **no — recovered** by the ancestry view, 2026-08-13 |
| `io/ioutil` | **banked, 28 of 28** (roster line 292) | **no — recovered** by the ancestry view, 2026-08-13 |
| `go/build` | 57 of 58, not banked | **partly** — see below; a GOROOT-IDENTITY member, not an internal-import one |
| `internal/coverage/cfile` | 4 of 16, not banked | **yes** — `use of internal package internal/coverage/slicewriter not allowed` |
| `internal/trace` | 85 of 92 as of this lane's run | **yes** — `TestTraceCPUProfile` + 3 subtests = **4 verdicts** |

Two corrections matter for a ruling. First, `internal/godebugs` and `io/ioutil` are **already
recovered**; carrying them forward inflates the class and its apparent payoff. Second, `go/build`'s
residual failure is a **different root inside the same family**: `TestLocalDirectory` calls
`ImportDir(cwd)` and expects the import path `go/build` back, which requires the *asking* directory
to sit under the GOROOT the toolchain reports — an identity question, not an import-permission one.
A remedy for the import-permission root does not automatically answer it, and the brief does not
claim it does.

So the honest live membership is **two packages** — plus `go/build`'s adjacent identity root — and
the verdicts at stake are:

- `internal/trace`: **4** — `TestTraceCPUProfile` and its Default / Stress / AllocFree subtests.
- `internal/coverage/cfile`: **up to 11** — `TestCoverageApis` and `TestApisOnNocoverBinary` both die
  in `buildHarness`, which runs `go build … testdata/harness.go` (`emitdata_test.go:123`) on a file
  importing `internal/coverage/slicewriter`; and because `TestCoverageApis` `t.Fatal`s in setup
  before any of its **nine** `t.Run` subtests register, those nine compare `Go="pass"` against an
  ABSENT C# verdict. Clearing the refusal lets all eleven be *produced*; whether they then MATCH is
  unmeasured, and the brief does not assume it. (cfile's other root, module resolution, was closed
  by the ancestry view — `src/go.mod` now sits above the package tree.)

So: **15 verdicts at stake, 4 of them certain.**

**Future membership is bounded and countable.** A package can only join by handing the toolchain a
staged Go source that imports `internal/…`. Census over `$GOROOT/src` (2026-08-28, Go 1.23.12):
**33 `testdata` `.go` files import an `internal/…` path**, in `internal/coverage/cfile` (1),
`internal/trace` (20), `runtime` (6), `runtime/race` (1) and `cmd/*` (5, never converted). `runtime`
is therefore the one unvalidated package that will meet this wall when its suite is attempted.

---

## 3. What the harness already does, and why it does not close this

`PackageAncestry` (landed 2026-08-13) stages GOROOT's content from its top level down to the package
under the run root — sibling directories as links, files as hard links, the package's own directory
as real copies — and leaves GOROOT itself pointing at the real installation. That closed the
working-directory class. It cannot close this one, and the reason is measured, not argued:
**the decision is made on the DIRECTORY PATH, and hard-linked files do not move it.**

---

## 4. Options, with what each was measured to do

Every row below is one `go build` of Go's own `internal/trace/testdata/testprog/cpu-profile.go` — the
file that imports `internal/profile` — or of a two-line equivalent, on Go 1.23.12 / Windows. Each row
is reproducible from its own description in a scratch directory; the shapes differ only in how the
directory holding the file was staged, and the CONTROL row is the shape the harness stages today.

| # | staged shape | result |
|---|---|---|
| **control** | plain COPY of the directory — *what the harness stages today* | **REFUSED** |
| A | the staged directory is a **junction** → the real GOROOT directory | **allowed**, exe produced |
| A′ | same, named by a **relative** path from the sandbox cwd — the real usage shape | **allowed**, exe produced |
| B | junction at the **`src` level**, the real package path materialized below it | **allowed** |
| C | the staged directory is a **directory symlink** → the same GOROOT directory | **allowed** |
| D | a real directory holding **hard links** to the GOROOT files | **REFUSED** |
| E | **GOROOT repointed** to a junction mirror; staged file under `<mirror>/src/<import path>` | **allowed** |
| E-ctl | same mirror GOROOT, file OUTSIDE its `src` | **REFUSED** |
| F | junction whose target is **not** inside `$GOROOT/src` | **REFUSED** |
| G | **`-overlay`**: a file absent from disk, declared at a virtual path INSIDE `$GOROOT/src` | **allowed**, ran, nothing written to GOROOT |
| G-ctl | same overlay content at a virtual path OUTSIDE `$GOROOT/src` | **REFUSED** |

Four further measurements the options turn on:

- **`cmd/go` DOES honor the `GOROOT` environment variable on 1.23.12.** `go env GOROOT` returns the
  mirror, and `go list -f {{.Dir}} internal/profile` returns `<mirror>\src\internal\profile`. This
  **contradicts** the board's 2026-08-13 note ("the child `go` resolves its own GOROOT from its
  executable location — measured: `go list` returns real-GOROOT paths with `GOROOT` set to a
  mirror"). Both cannot be true of the same toolchain; the 2026-08-28 measurement is the one with a
  matched control (E vs E-ctl) and is what a ruling should use. Executable location works *as well*:
  a **hard-linked `go.exe`** inside a synthetic root reports that root as GOROOT.
- **`GOFLAGS=-overlay=<file>` carries into a child `go run` the harness never sees the command line
  of** — measured. That is the only lever the harness has over a command the *test* constructs.
- **An overlay is a content map, not a location map.** Its KEY is the logical path, and the import
  check reads the key: G passes only because the key is a GOROOT path. A test that names
  `./testdata/testprog/x.go` from the sandbox pins the key to the sandbox, so overlay+GOFLAGS
  **cannot on its own** rescue `TestTraceCPUProfile`.
- **`filepath.EvalSymlinks` does NOT resolve a Windows junction** (`Lstat` → `ModeIrregular`;
  `EvalSymlinks` returns the path unchanged — measured twice, absolute and relative). So rows A/A′/B
  are measured facts whose *mechanism inside cmd/go is unattributed* — `expandPath` alone does not
  explain them. Row **C** (symlink) is fully explained by `expandPath`. This asymmetry is the single
  most important caveat in this brief and is carried into §7.

### Options rejected, with the reason

- **Vendor / copy the internal packages into the staged module.** A vendor directory cannot shadow a
  standard-library import path — `internal/profile` resolves in GOROOT before any module lookup — and
  rewriting the import would change the program under test, which is the one thing a differential
  harness may never do. *(Reasoned from the loader's resolution order; not probed.)*
- **An escape flag.** Verified absent (§1).
- **Full synthetic GOROOT for the whole run (option E as policy).** It works (E), but it repoints
  GOROOT for the **test process**, and the board's 2026-08-13 rejection still stands on its own
  measurement: a link mirror is not walk-equivalent to the real tree (a `*.gz` walk finds **0** where
  the real tree has **4**; `src/unicode` reports **1** entry against **19**), and two ALREADY-BANKED
  packages walk GOROOT that way (`compress/gzip`'s issue14937, `path/filepath`). Blast radius: all
  **162** banked rows, to recover fifteen verdicts.
- **Per-package skip / honest named-refusal class.** Always available, costs nothing, recovers
  nothing. It is the right answer only if the recommendation below fails its guard.

---

## 5. Recommendation — LINK-STAGE the runnable-program fixture trees

**Stage a `testdata` subdirectory that exists to be COMPILED as a link into the real GOROOT
directory, instead of as file copies. Copy everything else exactly as today.**

Concretely, the predicate proposed for the implementing lane — measured over the whole stdlib, not
invented: *a `testdata` subdirectory holding at least one `.go` file, in which **every** `.go` file
declares `package main`.* That selects exactly the runnable-program fixture trees and nothing else:

| directory | files / `.go` / `package main` | selected |
|---|---|---|
| `internal/trace/testdata/testprog` | 13 / 13 / 13 | **yes** |
| `internal/trace/testdata/generators` | 17 / 17 / 17 | **yes** |
| `internal/coverage/cfile/testdata` | 1 / 1 / 1 | **yes** |
| `runtime/testdata/testprog` | 33 / 31 / 31 | **yes** (future member) |
| `go/doc/testdata` | 81 / 23 / 0 | no — parse fixtures, correctly copied |
| `internal/types/testdata/check` | 67 / 67 / 3 | no — type-check fixtures, correctly copied |

**Why this one.** It is the smallest change that the measurements actually support: it moves ONE
property of ONE kind of staged directory, leaves GOROOT real (so nothing about the 2026-08-13
walk-equivalence finding is disturbed), leaves the run sandbox writable everywhere a test writes, and
needs no new concept — `PackageAncestry` already stages sibling directories as links and already owns
the containment rules for them. Option E buys the same five verdicts by changing the execution
contract of 162 banked rows; this buys them by changing the staging of six directories.

**Blast radius.** The fixture set is emitted into the test `.csproj`, the manifest, and the input
digest, so a link-staged tree changes three surfaces at once and must be handled deliberately:
(a) a linked directory has no per-file `<None>` items, so the csproj shrinks and the host copies
nothing for that subtree — the LINK must be created by the host at sandbox construction, not by
MSBuild; (b) `testInputDigest` must keep hashing the tree's CONTENT (F7) or a fixture edit stops
invalidating a prior comparison; (c) `copyTestFixtures` and `TestHost.CopyFixtures` both need the
link case. None of these is deep, but all three are load-bearing.

**Two hard constraints, both measured.**

1. **A write through the link would reach the real Go installation.** The host's existing guard
   turns that into content LOSS rather than corruption — `PackageAncestry.EnsureWritable` deletes a
   link component and creates an EMPTY real directory in its place, it does **not** copy the
   contents — which is safe but silently wrong. The predicate must therefore stay conservative, and
   the implementing lane owes a guard that a link-staged tree is never written to.
2. **The junction mechanism is unattributed** (§4). If it turns out to be incidental rather than
   intended, the same staging with a **directory symlink** (row C) is explained by `expandPath` and
   behaves identically — at the cost of `SeCreateSymbolicLinkPrivilege`, which on this box was
   **not** required (a directory symlink was created unelevated, presumably Developer Mode) but
   cannot be assumed on a fleet machine. Recommended shape: symlink when it can be created, junction
   otherwise, and a startup assertion that the chosen link is accepted by the toolchain.

**What it recovers.** `internal/trace`'s **4** verdicts (`TestTraceCPUProfile` + its Default / Stress
/ AllocFree subtests), taking that package from 85 of 92 to **89 of 92**; and
`internal/coverage/cfile`'s internal-import root, which un-blocks **up to 11** more (§2) — produced,
not guaranteed matching. It recovers **neither** `go/build`'s identity root nor anything for
`internal/godebugs` / `io/ioutil`, which are already banked.

**What it does not fix, and should not pretend to.** `internal/trace` still cannot bank after this
change: three more verdicts fail on a genuine converted-code divergence in the old-trace parser
(`TestOldtrace/stress_1_21_good` — "p 3 is running before start"; `TestOldtrace/
stress_start_stop_1_11_good` — "previous sweeping is not ended before a new one"; and their parent).
That root was **masked** by the staging gap this lane closed and is unrelated to this brief.

---

## 6. Cost of doing nothing

Up to fifteen verdicts today, in two unbanked packages; `runtime`'s testprog family (6 more files)
whenever that suite is attempted. The refusal is loud, attributable and stable, so a named-refusal class is a
defensible interim: it would read as *"a test that asks the real toolchain to compile a staged source
outside `$GOROOT/src` is refused its `internal/…` imports; go2cs stages outside GOROOT by design"* —
honest, and unlike a disclosure it names a harness limitation rather than laundering one as an
unsatisfiable assert.

---

## 7. Open questions a ruling should settle

1. **Attribute the junction result** (§4) before building on it, or adopt the symlink form, which is
   attributable. A gate that has never been made to fail proves nothing: the implementing lane owes a
   deliberate regression (link → copy) that reports exactly the four `TestTraceCPUProfile` verdicts.
2. **Reconcile the GOROOT-env contradiction** with the 2026-08-13 board note. It does not change this
   recommendation (which leaves GOROOT alone) but it changes what option E costs, and a stale
   "GOROOT env is ignored" belief will mis-price the next design that reaches for it.
3. **Does `go/build`'s identity root want its own arc**, or does it stay an honest 57 of 58? It is the
   only member a link-staging remedy does not touch.
4. **Keep `-overlay` in the toolbox.** It is a real location remap (G) and it carries through
   `GOFLAGS` into commands the harness cannot otherwise reach — the right instrument the day a test
   needs a *modified* source compiled at a GOROOT path, which is precisely what an
   `internal/coverage/cfile` harness rewrite would need.
<!-- {% endraw %} -->
