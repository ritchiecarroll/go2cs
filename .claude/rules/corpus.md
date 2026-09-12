---
paths:
  - "src/core/**"
---

# The one tree: corpus layout, hand-owns, L3 and deployment

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 197-224, 3610-3773, 4104-4289, 5389-5455.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

<!-- PHASE 2 APPLIED 2026-09-12 (branch claude/context-diet). Every visible rule is traceable to Phase-1 text; every date,
     SHA, count, package name and named incident that was visible before is preserved in the comment beside the rule it
     justifies. Nothing deleted. Byte-identical pre-split original: docs/doctrine/JOURNAL-2026-09-12.md. -->

## What `src/core` holds

ONE stdlib, on disk and in any build; no path rewriting anywhere. Five kinds of file, only the first regenerable:

1. **Converted packages** (`src/core/<pkg>`) — `go2cs -stdlib` output, regenerable wholesale. Never hand-edit here long-term;
   fixes belong in the converter, in `golib`, or in a declared hand-own.
2. **Hand-owned packages** — `src/core/unsafe`, `src/core/testing`: skip-listed in the conversion queue
   (`isNonConvertedStdLibPackage`, `stdLibConverter.go`), recovered into the generated solution from their dependents' references,
   so they publish to NuGet normally. `testing`'s subpackages (`fstest`, `iotest`, `quick`, `slogtest`, `internal/testdeps`) are
   ordinary converted packages.
3. **Hand-owned FILES inside converted packages** — `[module: GoManualConversion]` whole-file replacements and `*_impl.cs`
   companions. A reconvert leaves the marked `.cs` alone and drops a `<name>.cs.auto` sibling beside it.
4. **Hand-owned BY CONSEQUENCE** — every non-test Go file hand-owned → `unmarkedFileCount == 0` → the driver `continue`s before
   `writeProjectFile`, so `.csproj`, `package_info.cs` **and `README.md`** are never re-emitted either. Members:
   `crypto/internal/boring/bcache`, `internal/concurrent`, `internal/godebug`, `internal/weak` — **re-census, never quote.**
5. **`golib`** (`src/core/golib/`) — the hand-written runtime, shared by everything, never auto-generated; `src/core/go2cs` (the
   `Symbols.cs` shared project) sits beside it.

<!-- KIND 2 RATIONALE: `unsafe` is a compiler intrinsic; `testing` is the Phase-4 test host, and hand-owning it is what makes
     F15b's "ONE testing package, period" STRUCTURAL instead of a remap.

     KIND 4 censused 2026-09-01 at 3e31de03a over all 306 production packages; the class has GROWN TWICE — the note previously
     said THREE members, and before that godebug alone. bcache was the member nobody had counted, evidenced by the hand-edited
     position-map hash in its package_info.cs at f1df6cbd9: a re-emitting converter would never need a human to fix that.
     `unsafe` is also fully hand-owned but by the OTHER mechanism (skip-listed), which is why it is kind 2, not kind 4.
     Consequence counted the same day: the hand-own FENCE leaves 8 forced-init hooks missing inside this frozen class (godebug
     4, concurrent 3, weak 1) that only Stage B's frozen-README option (a) can fix — the relocation cannot, since these
     package_info.cs are never re-emitted.

     HISTORY (why this section used to say something else). The hand-finished baseline lived at src/gocore/ (2020-2025), was
     renamed to src/core/ on 2025-03-08 (ba6fef6c9), then OVERWRITTEN IN PLACE by the first full-stdlib conversion on 2025-05-05
     (6ca1c45b7, +508k lines) — which stalled the loop, because "conversion succeeded" there meant the transpiler didn't crash,
     not that the C# compiled. The 2026-06-25 repair relocated that conversion to src/go-src-converted/ and restored the stub
     into src/core, giving a green baseline immediately and a TWO-TREE doctrine: never reference both, because both emit
     `namespace go` with <pkg>_package partial classes and would collide. That doctrine held six weeks and cost a rewrite pass
     on every csproj, two exact-path exceptions in the overlay, an inverse rewrite in deploy-core, and a -tests remap. Its
     premise expired at Phase 3 — the corpus compiles under a standing gate and 69 packages validate — so on 2026-08-01 the
     conversion moved home to src/core, the stub retired, and all that machinery was DELETED rather than re-pointed. -->

## Layout L3 — per-GOOS folders

A package whose emitted C# varies by `GOOS` keeps the varying files in `<pkg>/{windows,linux,darwin}/`; its `.csproj` carries a
`$(GoTargetOS)` block compiling exactly one, defaulting to **`windows`**. A single-target run HONORS an L3 tree — writes
`<name>.cs` back into the `<goos>/` folder the tree already holds it in, not a flat duplicate — so a plain build, a plain
`-stdlib` reconvert and the seeded-reconvert control all still mean "the Windows corpus" byte for byte (0 new / 0 absent / 0
content differences). Seeding, the marker gate, the overlay rule and the phantom classification are UNCHANGED by L3. Design:
`docs/phase4/DESIGN-multiplatform-corpus.md`.

- **Unseeded root**: no `<goos>/` folder to route into → every varying file lands flat → the next build compiles two copies.
- **Hand-owns route by their principal's platform set** (`platformHandOwn_test.go`, plain `go test ./...`). The three-target
  merge places one per LOGICAL name only when its basename maps to an EMITTED principal (`<stem>.cs` for `<stem>_impl.cs`, or a
  marked file's `.cs.auto`), then REFUSES unless every candidate — each flavour folder plus flat — is BYTE-IDENTICAL; a
  companion with no emitted principal is left UNCOMPARED. **A per-flavour companion that differs from its sibling carries a
  flavour-distinct basename; a byte-identical one keeps one basename.**
- **L3 work owes a `-p:GoTargetOS=linux` build** — the windows `go2cs-stdlib.slnx` gate compiles no `linux/`/`darwin/` file.
- **darwin COMPILES CLEAN**: its census is a cheap regression guard at any branch tip (`.github/workflows/os-matrix.yml`,
  `goos=darwin stage=census`), not a wall. What darwin lacks is a RUN layer: `docs/phase4/FINDING-darwin-run-layer.md`.
- **A `GoTargetOS` switch poisons `obj/`** — the `<Compile>` item set changes while timestamps don't, so an incremental build
  after a switch validates the OTHER target's assemblies. Purge `bin`/`obj`/`Generated` between switches.
- **A reference closure is GOOS-conditioned like the item set**: a reference under a `$(GoTargetOS)` condition belongs to that
  flavour only, so a fold ignoring conditions reaches windows-only packages while deciding a darwin emission.

<!-- L3 since 2026-08-08; 37 packages were in L3 when this was written, and the ritual is UNCHANGED BECAUSE of the windows
     default, not despite it. ROUTING SCOPE read at the code 2026-09-05: the merge REFUSED on trace_impl.cs; two same-basename
     pairs that DIFFER across linux/ and darwin/ sit on master unrefused because they have no emitted principal and are left
     UNCOMPARED — an accident of naming, not a ruling.

     TWO L3 GATE LESSONS, measured 2026-08-15 on the three-target leveling lane: (1) the windows-only gate would have skipped
     15 of that regen's 27 files; (2) the obj/ poisoning above.

     DARWIN: the darwin half of lesson (1) was STALE FOR TEN DAYS and was corrected 2026-09-02. The retired text said "darwin
     does not currently build — os/dir.cs cannot resolve File.readdir, 19 pre-existing errors"; that was the state of 2026-08-22
     and survived here long enough to be copied into a lane prompt and send a lane looking for a wall that no longer exists.
     Census run 32649840220 at c003d32af: ZERO errors on osx-x64 AND osx-arm64; wall history 19 -> 10 -> 9 -> 0, closed by lane
     G within ~24 h of the first darwin build ever attempted; re-confirmed green at master by the 2026-08-25 census (run
     32852475367, both legs).

     GOOS-CONDITIONED CLOSURE, 2026-09-08: one alias cut's single darwin footprint file was minted through a WINDOWS-conditioned
     edge, while the Go dependency list at darwin holds none of those packages and the darwin census compiles clean at master.
     The scoping is the compiler's own rule, measured at the project file and at the loader, which is what makes narrowing the
     fold legitimate rather than fitting the answer. -->

## cgo: the corpus is emitted at `CGO_ENABLED=0`

**RULED: cgo OFF is the state of record on EVERY platform; the sweep pins it for the WHOLE run and the roster preamble states it
once.** Set `CGO_ENABLED=0` before converting against or regenerating the corpus on any Linux host with gcc.

- **MIXED-state tree**: file selection changes → declarations MIGRATE while the stale other-selection file remains → CS0111
  duplicate init forcer in `net`. Reads exactly like a converter defect; it is an environment mismatch.
- **From the TEST side the state is PER-PACKAGE**: a cgo-ON sweep selects `_test.go` files the corpus never carried, leaves
  untracked `cgo_*_test.cs`, and dies in the closure build in seconds with **zero verdicts**. `run-validated-sweep.ps1` pins it
  per package (`$cgoOffPackages`, beside `$longTimeouts`) — a cgo-conditional row JOINS that table, never the ambient session.
- **Every row number carries the cgo state it was taken under, and a bank states its TERMINAL CONTEXT too**: the ORACLE's own
  cgo-gated branches are a second axis (a row can AGREE by coincidence of errno), and tests that skip under a detached no-TTY
  driver on both sides DIVERGE where a controlling terminal exists.
- **Converter-internal member of the class**: the import-alias rename pass reads the Go loader's closure AT THE LOADER'S RELEASE
  while the collision is settled AT COMPILE TIME by the CORPUS's reference closure at the CORPUS's release — **they agree iff the
  releases agree.** Ruled fix: UNION the Go closure with the corpus's transitive PROJECT closure (exact, never directory
  existence), falling back to the Go closure where no corpus project exists; acceptance is CNR under the newer pin reading 0 CHANGED.

<!-- WHY cgo-off is coherent: the converter cannot process cgo C halves regardless (it skips toolchain intermediates loudly since
     the Syntax-pairing fix), and Go's own cgo-off file selections are fully functional pure-Go paths.

     A CORPUS-LEVEL fact, not a lane detail — measured 2026-08-29 at net's Linux first contact: net/linux/cgo_stub.cs is on disk
     and cgo_stub.go is selected ONLY when cgo is off. The mixed-state tell was measured as the forcer moving from cgo_stub.cs to
     dial.cs with BOTH on disk. PER-PACKAGE TEST SIDE measured 2026-09-02 on a Linux lane as a one-variable A/B on os/user, whose
     Go file selection is cgo-conditional; the closure build died in ~12 s with zero verdicts. Both comparison sides must share
     ONE cgo state, and the converted side can only be the corpus's.

     ERRNO COINCIDENCE measured 2026-09-02, one variable proven both ways: Go's AllThreadsSyscall tests skip on ENOTSUP because
     the ORACLE is cgo-LINKED and the converted side skips on the same word from an unimplemented stub — pass/pass at cgo-OFF,
     skip/skip at cgo-ON.

     THE RULING, 2026-09-03. The Linux annotations had been banked without naming the axis: a cgo-OFF re-sweep read three rows
     LOW by exactly the ORACLE's testenv.HasCGO-gated tests (buildinfo +7, gcimporter +1, pe +3 skip/skip) with ZERO verdicts
     moving, because that bank host's default was cgo ON while the corpus is emitted cgo OFF. The deciding evidence came from the
     OTHER platform — the Windows bank host has no C compiler, so its counts for those rows ARE the cgo-OFF readings — and ruling
     the other way would have left the two platform bubbles on different unstated axes. A re-sweep already in flight at the ruled
     state IS the record; no second sweep is owed. DRIVER CONTEXT as a third axis: 2026-09-04 — two syscall tests, so every fleet
     sweep matched them skip/skip and no sweep can see the divergence.

     CONVERTER-INTERNAL MEMBER, 2026-09-08: a converter-pin move made the releases disagree — one package's internal children are
     re-homed between the two releases, so the ANCESTOR namespace the newer loader sees is brought into existence by a child the
     older corpus still references. The ruled fix also carries a DECISION-level guard over synthetic closures plus a fixture
     corpus tree. -->

## Measuring a converter change

- **The on-disk corpus can be STALE**: the committed tree measures *that* output, not today's converter. Reconvert.
- **ADDRESS-OF/ALIASING (`Ꮡ`) changes owe a seeded reconvert-and-BUILD; CNR alone is NOT sufficient.** The behavioral corpus is a
  SAMPLE of Go's shapes; the stdlib is the population.
- **Blast radius = TWO seeded reconverts (pre-change, changed) diffed against EACH OTHER — never the committed-tree diff — over
  the WHOLE corpus.** The committed tree is a moving baseline carrying unbanked drift; a scoped census reproduces its own scope.
- **Take EVERY seed before ANY arm converts**, or seed from a frozen snapshot (`git archive`): seeding each arm from the live
  worktree as it starts reports files as "differing" that NEITHER converter wrote.
- **Prove the write; never infer it** — four assertions owed every run:
  1. **The binary's MTIME MOVED and is newer than its sources.** Existence-and-size is too weak: `go build -o <path> <dir>` can
     land the binary at `<dir>/go2cs.exe` while exiting 0, and the check then passes on the STALE binary at the invoked path.
  2. **Name the toolchain that built EACH arm** (`go version <binary>`) — a build without the GOROOT pin can stamp the two arms
     at different Go releases.
  3. **Count files WRITTEN this run, per arm per target, and ABORT on an arm that emitted nothing** — two untouched seeds compare
     0/0/0, indistinguishable from a clean gate. Corroborate with the wall and the exit code. **A lane finding its own vacuous
     reading RETRACTS it publicly.**
  4. **Write-evidence is a CONTENT test, never a timestamp**: hash a `[module: GoManualConversion]` file IN A PACKAGE THE ARC
     TOUCHES across every seed and require equality; else fall back to whole-tree identity outside the emitted set. Mtimes are a
     HINT — **a written count of ZERO is decisive, a nonzero one is not.**
- **Derive hunk anchors from the BASE EMISSION** (difflib opcodes) so no converter glyph passes through a shell, and
  **`git show HEAD:<file>` returns the LF blob against a CRLF checkout** — name the LAYER before quoting a count.

<!-- ALIASING GATE proven 2026-08-15, the element-field-address fix: of three defects in that arc CNR caught ONE; the other two —
     a pointer-receiver named-array blind spot and its over-broad first fix — appeared in NO behavioral test's shape and were
     found only because the whole corpus was reconverted and compiled. The same census surfaced a real SHIPPED lost write:
     encoding/xml's attribute-namespace translation writing into a copy.

     TWO-SEEDED DIFF, 2026-08-29, the position-table splitter fix: a naive reconvert-vs-committed diff reported 147+ files,
     almost all PRE-EXISTING unbanked drift from arcs that landed without their regens (the standing position-map staleness two
     census lanes rooted independently the same day); the two-seeded diff isolated 26 metadata files, zero production code.
     WHOLE-CORPUS SCOPE, 2026-09-04: the whole-corpus run found both a publication defect and fourteen sites in sync/map.cs the
     name-keyed census had not predicted.

     TELLS 1 and 2 paid 2026-09-01: a lane's "old" binary never existed and the diff silently compared committed-tree-vs-fixed,
     reporting 724 phantom files (the emitted-before-seeded family, build-step edition). Tell 1 STRENGTHENED 2026-09-05:
     `go build -o` fed an MSYS /c/... path wrote into a stray C:\c\ tree while the existence-and-size check read the STALE binary
     at the right path and passed. The mtime assertion is the converter's own staleness predicate; checking out a branch whose
     converter sources are newer than the binary is exactly what it REFUSES, correctly. The toolchain tell came from a build
     stamping one arm go1.24.7 against the other's go1.23.12, caught only by byte-comparing a pinned rebuild against the running
     binary.

     THIRD MODE (live-worktree seeding), 2026-09-04: both standing tells passed — binary at the exact path, this run's mtimes on
     both sides — yet base and new were seeded from different trees and the diff read exactly like a footprint. `find -newermt`
     reported 179 src/core files inside one run's window that were a branch switch completing before the first seed; trusting
     them would have discarded a sound A/B. A hand-own in an untouched package discriminates nothing. Same run: a heredoc
     mangled a converter glyph, caught at zero matches (the LF-anchor family through its glyph door); and `git show HEAD:<file>`
     produced 1,408 "differing" lines that CR-strip to 18.

     FOURTH MODE, 2026-09-05, the emitted-vs-seeded trap's PUREST instance: both arms piped through `Select-Object -First N` were
     KILLED at exit -1 with ZERO .cs written and the comparison of two untouched seeds read 0/0/0. The -First N kill is the
     documented pipeline trap; what was new is that it can take out an A/B's arms silently, since the trap's usual tell (a
     visibly dead run) is what the diff then reads as a green. The impossible WALL is minutes where a three-target conversion
     floors at ~9. The diff script now ASSERTS the written count and ABORTS on an empty arm, and the lane retracted its own
     reading with the mechanism before anything seated on it. -->

## Reconvert → overlay → build → bucket

1. **SEED FIRST — non-negotiable.** `cp -r src/core <tmp>/core` before reconverting, **excluding `bin`/`obj`/`Generated`**, then
   **verify the seeded `.cs` COUNT**. An EMPTY or PARTIAL root gives the marker nothing to detect — `<file>.cs.auto` is written
   only when the marked file already exists at the output path — so every hand-owned rewrite emits as plain `.cs`, the overlay
   rule ("copy `*.cs`, exclude `*.cs.auto`") protects NOTHING, and hand-owns are clobbered with auto conversions that COMPILE and
   are operationally broken. Control: untouched seeded files reproduce HEAD byte-for-byte. Seeding also hides files the converter
   has STOPPED emitting — classify emitted-vs-seeded by sentinel mtime and report would-be deletions.
2. **Seed `<tmp>/src/version.props` and `<tmp>/docs/validation` too, mirroring the repository** (version.props at the root
   holding `core/golib`, `docs/` its SIBLING); convert with `-go2cspath <tmp>/src`. Of the README's four badges, **Tests** and **Source·C#** read the
   REPOSITORY (`src/version.props`, `docs/validation/current/<dot-id>.md`) and vanish without it; **Docs** and **Source·Go** read
   the TOOLCHAIN and survive — so the tell is a README carrying two badges, not none.
3. **Convert**: `go2cs.exe -stdlib -comments -go2cspath <tmp>/src` → output at **`<tmp>/src/core/<pkg>`** (`core` is hardcoded;
   `-go2cspath` is the *output* root, unrelated to MSBuild's `$(go2csPath)`). ≈ 3–4 min for the full stdlib — **batch, never per package**.
4. **NEVER convert twice into one temp root; never let two conversions overlap on a box.** Delete and re-seed the root for every
   reconvert; confirm no `go2cs.exe` is alive first. A race corrupts ONE file with unresolved `«DYNTYPE:…:DYNTYPE»` lift markers
   → CS1056/CS1003, reading exactly like a converter regression. Wrap the call in `$ErrorActionPreference='Continue'` (or don't
   pipe its stderr): under `'Stop'` a stderr WARNING becomes a terminating `NativeCommandError` that aborts the wrapper and
   leaves the exe alive.
5. **MARKER GATE before overlaying — PATH-PRECISE, not a count.** For every `[module: GoManualConversion]`-marked committed file
   the temp root must NOT hold a freshly-EMITTED plain `.cs` at that path. Marked files far outnumber `.cs.auto` producers, so
   **a same-count assertion is wrong in both directions**.
   - **Re-measure the census every time; never assert last session's number; treat a SHRINK as something to EXPLAIN.** Under L3
     one hand-own can exist as TWO files, so marked FILES is not distinct hand-owns. Multiple markers in ONE assembly are legal
     and normal (`runtime` carries eight).
   - **Scan WHOLE FILES** (markers sit below long license/using blocks) and **LINE-ANCHOR**
     `^\s*\[module:\s*(go\.)?GoManualConversion\]` — `reflect/value.cs` and `internal/reflectlite/value.cs` *mention* the marker
     in placeholder comments, so an unanchored `grep GoManualConversion` raises a false clobber alarm and a head-window scan
     makes the gate VACUOUS.
   - **`.cs.auto` siblings are tracked in git but NOT refreshed by the overlay** → they stale on their own schedule and are
     RE-MEASURED at every rebank head, by **CR-stripped equality** against the committed file.
6. **Overlay** the fresh `.cs`, **`.csproj` and `README.md`** onto `src/core/<pkg>` — a straight copy, no rewriting. A seeded
   reconvert of the whole stdlib is byte-identical to the committed tree, so any post-overlay diff is a real converter change.
   Two knowns that are NOT: the SIX root attribution files (`src/core/README.md` and its five siblings) show modified with an
   EMPTY `git diff --numstat` — CRLF phantoms, restore them; and kind 4, never re-emitted at all.
7. **Build** with **`dotnet build <pkg>.csproj -c Debug`** — `src/core/Directory.Build.props` pins `$(go2csPath)`, so
   `core\golib` + the `go2cs-gen` analyzer resolve to live source with **no `-p:go2csPath` flag** — or build all of
   `go2cs-stdlib.slnx` (~92–150 s warm, 305 assemblies; the 306th, `crypto/x509/internal/macos`, is darwin-exclusive). Passing
   the flag anyway needs forward slashes (`-p:go2csPath=H:/Projects/go2cs/src/`): a trailing `\` escapes the closing quote and
   mangles the path into phantom golib-not-found errors.
8. **Bucket**: `dotnet build … -clp:ErrorsOnly`, group by `error CS####`. Errors shown are *own-errors* of leaf-most failures —
   dependents of a failed project are skipped, not errored.

<!-- SEED FIRST learned 2026-07-25, cost a false operational-break alarm: 14 hand-owned files got clobbered, and godebug's auto
     init() throws in a module initializer and takes down every dependent. PARTIAL SEED fleet-confirmed twice in one day,
     2026-09-01: Copy-Item -Recurse dies on go2cs-gen's long obj\...\Generated\ paths and, under the ritual's own required
     $ErrorActionPreference='Continue', carries on past the death — the unseeded-root hazard through a door the rule never named.
     The emitted-files control is what makes a suspect seed's readings trustworthy.

     MARKER CENSUS HISTORY — exactly why it is re-measured, never carried forward: 32 at r14; 39 before internal/concurrent's
     hashtriemap.cs joined in r39d; 40 at r40; DOWN to 39 when the r41 train's regen retired math/unsafe.cs's hand-own without
     saying so (the BitConverter bit casts went back to the auto `Ꮡf.Reinterpret<float32, uint32>()`, correct now that
     Reinterpret genuinely aliases managed storage, and math's banked 76/76 re-proves it every sweep — benign in that instance,
     but a hand-own disappeared under an overlay while the commit reported its marker gate "40/0"); back to 40 when
     internal/weak/pointer.cs joined at r43e; 41 at r44a (re-measured 2026-08-07) when internal/cpu/cpu_x86_impl.cs joined, of
     which only 15 produce .cs.auto and the other 26 are *_impl.cs companions and hand-owned packages the converter never
     re-emits at that path; 42 since r50a for a NEW reason — L3 routes a hand-own into its principal's per-GOOS folders and
     runtime/lock_sema_impl.cs's principal is selected on Windows AND macOS, so one hand-own exists as TWO files; 44 since r51b,
     when runtime/lock_managed_impl.cs (the flat, platform-neutral managed core of the mutex/note protocol) and
     runtime/linux/lock_futex_impl.cs (the futex flavor's 2-arg notetsleep_internal) both took the marker; 49 marked /
     41 *_impl.cs companions / 59 distinct hand-owns at the r59 regen bank (2026-08-11); 53 marked / 42 companions at the Linux
     regen wave (2026-08-14), 0 violations across 3 targets x 2 merge passes; 73 marked / 49 companions / 24 whole-file rewrites
     at the 2026-08-24 post-merge rebank, 0 violations on windows and linux alike.

     WHOLE-FILE SCAN measured 2026-08-17: a first-40-lines window reported 35 marked files against the real 60, which would have
     made the clobber gate vacuous for 25 hand-owns. LINE-ANCHORING: an unanchored grep reports 63 against the real 40.
     .cs.auto STALENESS is CleanupBacklog item 18 and the measurement moves — 11 of 16 stale at r40, 0 of 23 at the 2026-08-24
     post-merge rebank; a seeded reconvert per target re-emits each sibling, and a RAW byte compare reports the whole set as
     differing because a fresh emission carries the in-literal LF the working tree holds as CRLF.

     BADGES: version.props + docs/validation seeding added 2026-08-02 with the README validation badges. The converter finds both
     by the same upward walk it uses for $(go2csPath) and emits NO Tests badge when either is missing — a silent, corpus-wide
     README diff on overlay. Docs and Source-Go were measured toolchain-sourced 2026-08-08 (go env GOVERSION, and GOROOT's own
     src/vendor/modules.txt for the 19 GOROOT-vendored golang.org/x/* packages), which is why the symptom is now a two-badge line
     rather than no badge line; the rule is unchanged, only the symptom is. A versioned docs/validation/<version>/ need NOT be
     seeded — the badge reads current/, and the versioned directory is only the link target and the Exists-guarded pack input.

     CONVERSION COST: per-file work is sub-second; the ≈3-4 min is go/packages loading the whole type graph, which is why the run
     must be batched rather than invoked per package.

     RACED CONVERSION found r41, 2026-08-05: runtime/arena.cs with nine unresolved DYNTYPE anonymous-struct lift markers. It is
     NOT a converter regression — a clean-room reconvert (fresh root, seeded, single run) emits zero DYNTYPE markers anywhere in
     the corpus, and so does a single-package run; hence a mechanical rule rather than a diagnostic one.

     OVERLAY: trees unified 2026-08-01, so the reconvert's paths ARE the repository's paths — no rewriting, no exceptions; 2518
     .cs/.csproj verified byte-identical on the consolidation commit, 300 README.md joining the byte-identical set 2026-08-02.
     The six root attribution files were measured 2026-08-17 — this note previously named only one. -->

## Encoding, metric, and banking a regen

- **Byte-compare any restore or round trip against the original**, and **after any scripted edit diff the file's FIRST and LAST
  lines** — BOM, trailing-newline and CRLF damage lands there, where review attention is weakest.
- A script rewriting a csproj must read AND write explicit UTF-8/no-BOM (`[System.IO.File]::ReadAllText`/`WriteAllText` +
  `UTF8Encoding($false)`); PS 5.1 `Get-Content` reads BOM-less UTF-8 as ANSI and `Out-File utf8` re-encodes the damage. Python's
  `utf-8-sig` breaks BOTH ways: read-sig/write-sig ADDS a BOM, read-sig/write-plain STRIPS one.
- **Measure packages-compiling, not raw error count**: fixing file-inclusion bugs *raises* the count because newly-included files
  surface their own latent defects — progress, not regression.
- **Never bank "my fix caused N new errors" without the five-minute control**: revert the fix, build PAST the original blocker,
  see whether they remain. Unmasked errors appear precisely where compilation could not previously reach, so "the errors are in
  different files from my change" is evidence of UNMASKING, never of causation.
- **A CLEAN second half is believable only after the first half is FIXED** — re-measure a two-half row after each half, and expect
  the second to need a DIFFERENT fix, not the first applied twice.
- **A regen that moves `package_info.cs` records owes `go generate .` in `src/go2cs`**: `stdlib-metadata.txt` is generated FROM
  the corpus and gated by `TestStdLibMetadataInSync` under the plain converter `go test`. Regenerate, verify, commit together.
- **Don't commit corpus regens casually** — the unit of work is the **converter fix**. Keep the tree restorable (overlay into a
  branch, or `git checkout HEAD --` plus removing untracked files).

<!-- csproj I/O 2026-07-25: the ANSI/Out-File round trip double-encoded the © in <Copyright> on every pass, creating and then
     TRIPLING the 258-file corpus mojibake; root-caused and leveled in the r11 bank. utf-8-sig ADD caught 2026-08-31 by the
     hand-application byte-identity bar during a probe restore; STRIP measured 2026-09-03, when two go2cs-gen sources lost their
     BOMs, caught only by reading the FIRST line of the diff (`-<BOM>// ...` against `+// ...`). Three encodings, three silent
     corruptions — PS 5.1 ANSI, UTF-16 redirects, utf-8-sig.

     The five-minute control was named 2026-09-01, after a substantially-correct fix was DISCARDED on the misread; the example
     inclusion bug is the filename build-constraint fix. The other direction was measured 2026-09-05 on the field-walk dedupe:
     fixing the walk UNMASKED a CS1929 on the METHOD half of the same row the unfixed generator had reported clean, and that half
     needed depth-aware promotion inside an embed. The stdlib-metadata drift happened 2026-08-15: the second leveling regen moved
     6 records and the drift surfaced in an unrelated lane's gate run, leaving the converter gate red at master for whoever ran
     it next. src/core/<pkg> is regenerable, which is why a converter-fix commit must not be buried under thousands of
     generated-file changes. -->

## A DELETING instrument over the corpus

- **Derive the candidate population from what the CONVERTER EMITS** — in `go list std` at the SOURCE release and not skip-listed
  — never from a decider answering a meaningless question for a hand-written directory. `go list` says "package not in std at
  target" for `golib` and the Symbols project, TRUE and IRRELEVANT, while the marker-based PROTECTED arm cannot see a directory
  that correctly carries no marker: **the one directory needing no marker is the one the guard does not protect.**
- **Run every refusal BEFORE the deletion loop** so a non-zero exit MEANS nothing was removed, and **REFUSE while UNRESOLVED rows
  stand** (dry run → a human disposes → `-Apply`). Non-zero AFTER deleting is a report wearing a refusal.
- **Predict against the question the instrument actually asks** — a deletion pass and a live-package census are two questions.
- **PowerShell `$x[0..($x.Count - 2)]` on a SINGLE-element array yields TWO elements** (`0..-1` counts DOWN to `@(0,-1)`). Guard
  the part count before slicing, and grep the other `src/*.ps1` instruments for the idiom.

<!-- All from the 2026-09-07 read of reconvert-deletions.ps1, found by a lane on the first real dry run and NOT applied. go list
     refused golib (116 files) and the Symbols shared project; the landed script exited 2 AFTER deleting. Prediction numbers: the
     deletion-pass question (whole removed packages plus non-Go directories) = 205, the live-package census = 25. The slicing
     idiom turned core/GlobalUsings.cs into the import path GlobalUsings.cs/GlobalUsings.cs and minted a phantom DELETE-ABSENT
     row at ed191e9a8. -->

## Deploying the core, and purging build output

`src/deploy-core.ps1` (launcher `deploy-core.bat`) stages runtime + stdlib at `%GOPATH%\src\go2cs` so converted projects — and
recursively converted end-user apps targeting that root — resolve `$(go2csPath)core\<pkg>` / `gen\go2cs-gen` relatively. ONE mode
since the trees unified (the old `stub`/`stdlib` argument is gone): a straight copy of `src/core`. It also deploys the
`go2cs-gen` analyzer, writes a root `Directory.Build.props` pinning `$(go2csPath)` to the deploy root, generates
`go2cs-core.slnx`, and builds to verify.

- **Its default target is MACHINE-GLOBAL** (`%GOPATH%\src\go2cs`, shared with every sibling worktree) — **never run it bare as a
  gate.** `-WhatIf` is a real dry run (the three non-cmdlet writes are `ShouldProcess`-gated and the enumeration reads the SOURCE,
  so the projected project count is truthful); `-Target <dir>` gives a scratch deploy.
- **Never re-derive harness path/platform primitives** — `$IsWindowsHost`, `$ExeSuffix`, `$SepPattern`, the roots and
  `Get-PathDepth` live in **`src/_paths.ps1`**, dot-sourced by every instrument. `$IsWindows` does not exist on PowerShell 5.1, so
  a bare `-not $IsWindows` reads BACKWARDS on the one platform 5.1 runs on.
- **Purge with `clean-bin.ps1` or an explicitly depth-UNLIMITED walk, after every probe** — an ad-hoc `find … -maxdepth 3` purge
  missed most output directories and drove a lane's disk into the harness's free-space floor.
- **Non-interactively (background task, harness tool call, any `-NonInteractive` host) pass `-Force` AND `-Root` explicitly**, as
  `clean-bin.bat -Force` or `powershell -NoProfile -ExecutionPolicy Bypass -File .\clean-bin.ps1 -Force`. The script is unsigned
  → a bare `powershell -NoProfile -File` dies "is not digitally signed" where policy requires signing; `-Confirm:$false` does NOT
  bind through `powershell -File` on 5.1, so **`-Force` is the contract**. Without an explicit `-Root` the default resolves to an
  EMPTY STRING and the run enumerates the CWD, reports "Found N folders", and deletes NOTHING.
- **Its exit code is load-bearing before any build that follows a purge**: 0 = all found are gone, 1 = declined, 2 = could not
  prompt and `-Force` absent, 3 = `-WhatIf`, 4 = something survived. **Read the exit code plus a dirs-left count, never the log's
  "Found N"** — and read it through `-File` or the `.bat`; `-Command "& …"` collapses every non-zero code to 1.
- **After a `$(GoTargetOS)` switch the purge is only the BELT**: the braces are the per-target compile item-set read
  (`dotnet msbuild -getItem:Compile`), and only that answers the question the purge exists for.
- **A purge across KEPT worktrees is gated by a PROCESS CENSUS that ABORTS** — purging `obj` under a live build corrupts a run
  that surfaces later as somebody else's false red. **Enforce "never `Generated`" by MEASUREMENT** (counts before and after,
  identical in every worktree, since it is not gitignored) and **prove "no source touched"** by re-reading every worktree's dirty
  count afterwards.
- **N errors whose codes are all I/O are ONE environmental failure wearing N codes, not N defects** — read the histogram
  (MSB3491/MSB3027/MSB3021/CS0016/CS0041/CS8104, every message *not enough space on the disk*) before believing a count.
- **A measurement taken on a tree that CHANGED under it is not a measurement** (e.g. a `bin`/`obj` purge while MSBuild was writing
  there), and **a depth-limited `find` estimate is a FLOOR mislabelled as a measurement** — the same class as a `-prune` answering
  a narrower question than the one asked.

<!-- The copy is pure PowerShell (Copy-SourceTree) rather than robocopy since 2026-08-08 and is byte-identical to what robocopy
     produced (3,979 files A/B-verified); that also removed the repository's last external Windows tool dependency. The
     repository layout and the deployed layout are now the same layout, so no reference needs rewriting and no -p:go2csPath is
     needed at the deploy root. The other src PowerShell utilities clean-bin.ps1 (remove bin/obj/Generated) and set-version.ps1
     each also have a .bat launcher.

     The maxdepth-3 purge missed 274 of 388 output directories, 2026-09-02. The unsigned-script death was observed 2026-09-03 and
     does NOT reproduce on a box whose LocalMachine policy is already Bypass, which is exactly why the bypass belongs in the
     invocation rather than in an assumption about the host — the .bat already carries it and forwards arguments. The same day
     met the other half of that hole: a wrapper captured a non-zero clean and carried on, running a target-switch build without
     the purge it reported attempting — and Read-Host on EOF printed "Found 2866 folders to delete. Operation canceled." and
     exited 0 having deleted nothing. A found-but-not-deleted run never exits 0. The empty -Root default was measured 2026-09-05:
     Resolve-Path refuses the empty path, the script enumerates the CWD anyway, reports "Found 2640 folders", and the run exits 2
     having deleted NOTHING. Item-set example: 39 windows / 0 linux under one GoTargetOS and 0 / 75 under
     the other.

     KEPT-WORKTREE PURGE, 2026-09-08: the reclaimed space EXCEEDED the estimate, and the estimate is the lesson — it came from a
     depth-limited find and was a FLOOR mislabelled as a measurement.

     DISK EXHAUSTION IN TWO COSTUMES, 2026-09-06: a coordinator's own two scratch worktrees held 5,096 build-output directories
     and filled the disk to zero. The fleet's mailbox push failed on "Out of diskspace", and a solution leg that had built clean
     an hour earlier came back exit 127 with 78 errors whose histogram was entirely I/O; purging recovered 43 GB. What made the
     exhaustion VISIBLE rather than silent was the posting tool REFUSING to claim delivery and saying the commit did not land — a
     state-advancing tool that reports its own failure honestly is worth more than one that succeeds quietly. -->
