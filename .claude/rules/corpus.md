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


1. **Converted packages** (`src/core/<pkg>`) — `go2cs -stdlib` output, regenerable wholesale. Nothing is
   hand-edited here long-term; fixes belong in the converter, in `golib`, or in a declared hand-own.
2. **Hand-owned packages** — `src/core/unsafe` and `src/core/testing`. Both are skip-listed in the
   conversion queue (`isNonConvertedStdLibPackage`, `stdLibConverter.go`) and both are recovered into the
   generated solution from their dependents' references, so they publish to NuGet like any other package.
   `unsafe` is a compiler intrinsic; `testing` is the Phase-4 test host, and hand-owning it is what makes
   F15b's "ONE testing package, period" structural instead of a remap. `testing`'s **subpackages**
   (`fstest`, `iotest`, `quick`, `slogtest`, `internal/testdeps`) are ordinary converted packages.
3. **Hand-owned FILES inside converted packages** — the `[module: GoManualConversion]` whole-file
   replacements and the `*_impl.cs` companions. A reconvert leaves the
   marked `.cs` alone and drops a `<name>.cs.auto` review sibling beside it.
4. **`golib`** (`src/core/golib/`) — the hand-written runtime, shared by everything, never auto-generated.
   `src/core/go2cs` (the `Symbols.cs` shared project) sits beside it.

**History (why this section used to say something else).** The hand-finished baseline lived at
`src/gocore/` (2020–2025), was renamed to `src/core/` on 2025-03-08 (`ba6fef6c9`), then **overwritten in
place** by the first full-stdlib conversion on 2025-05-05 (`6ca1c45b7`, +508k lines) — which stalled the
loop, because "conversion succeeded" there meant the transpiler didn't crash, not that the C# compiled.
The 2026-06-25 repair relocated that conversion to `src/go-src-converted/` and restored the stub into
`src/core`, giving a green baseline immediately and a **two-tree** doctrine: never reference both, because
both emit `namespace go` with `<pkg>_package` partial classes and would collide. That doctrine held for
six weeks and cost a rewrite pass on every csproj, two exact-path exceptions in the overlay, an inverse
rewrite in `deploy-core`, and a `-tests` remap. Its premise expired at Phase 3 — the corpus compiles under
a standing gate and 69 packages validate — so on **2026-08-01** the conversion moved home to `src/core`,
the stub retired, and all of that machinery was deleted rather than re-pointed. There is still only ever
ONE stdlib in a build; there is now only one on disk.

### Corpus mechanics — measuring/iterating the converted stdlib (`src/core`)
- **⚠ 37 packages are in LAYOUT L3 and the ritual below is UNCHANGED because of it, not despite it.**
  Since 2026-08-08 a package whose emitted C# varies by `GOOS` keeps the varying files in per-GOOS
  subfolders (`<pkg>/{windows,linux,darwin}/`) and its `.csproj` carries a `$(GoTargetOS)` block that
  compiles exactly one of them, defaulting to **`windows`** — so a plain `dotnet build` and a plain
  `-stdlib` reconvert both still mean "the Windows corpus", byte for byte. That default is what keeps
  the seeded-reconvert control honest: a SINGLE-target run **honors** an L3 tree (it writes
  `<name>.cs` back to the `<goos>/` folder the tree already holds it in) rather than laying a flat
  duplicate beside it, so a seeded reconvert of an L3 corpus is still 0 new / 0 absent / 0 content
  differences. Nothing about seeding, the marker gate, the overlay rule or the phantom classification
  changes. What DOES change: an **unseeded** root now breaks layout adoption as well as the marker
  gate (there is no `<goos>/` folder to route into, so every varying file lands flat and the next
  build compiles two copies) — one more reason the seeding is non-negotiable. Hand-owned files are
  routed too, by their principal's platform set; the invariant is guarded by `platformHandOwn_test.go`
  under the plain `go test ./...`. Design: `docs/phase4/DESIGN-multiplatform-corpus.md`.
  ⚠ **What that routing does and does NOT cover, read at the code 2026-09-05.** The three-target merge
  places a hand-own per LOGICAL name (the GOOS folder stripped) **only when its basename maps to an
  EMITTED principal** — `<stem>.cs` for a `<stem>_impl.cs`, or the `.cs.auto` of a marked file — and a
  placed hand-own's candidates are every flavour folder plus flat, whose copies must be BYTE-IDENTICAL
  or the merge REFUSES (`trace_impl.cs`). A companion with NO emitted principal is left in place
  UNCOMPARED, which is why two same-basename pairs that DIFFER across `linux/` and `darwin/` sit on
  master unrefused — an accident of naming, not a ruling. So: **a per-flavour companion that differs
  from its sibling carries a flavour-distinct basename; a byte-identical one keeps one basename and is
  routed as one hand-own.**
  **Two L3 gate lessons (measured 2026-08-15, the three-target leveling lane):** (1) a change whose
  files live in linux/darwin per-GOOS folders is NOT compiled by the default windows build — the
  windows `go2cs-stdlib.slnx` gate alone would have skipped 15 of that regen's 27 files, so L3 work
  owes a `-p:GoTargetOS=linux` build too. ⚠ **The darwin half of this note was STALE for ten days
  and is corrected here (2026-09-02): darwin COMPILES CLEAN** — census run 32649840220 at
  `c003d32af`, **zero errors on osx-x64 AND osx-arm64**, the wall history **19 → 10 → 9 → 0**
  closed by lane G within ~24 hours of the first darwin build ever attempted, re-confirmed green
  at master by the 2026-08-25 census (run 32852475367, both legs). The retired text said "darwin
  does not currently build — `os/dir.cs` cannot resolve `File.readdir`, 19 pre-existing errors";
  that is the state of 2026-08-22, and it survived here long enough to be copied into a lane
  prompt and to send a lane looking for a wall that no longer exists. **The darwin census is a
  REGRESSION GUARD now, cheap and dispatchable at any branch tip** (`.github/workflows/os-matrix.yml`,
  `goos=darwin stage=census`), not a wall to census. What darwin still lacks is a RUN layer, which
  is a separate and open question: `docs/phase4/FINDING-darwin-run-layer.md`).
  (2) **A `GoTargetOS` switch poisons `obj/`**: the `<Compile>` item set changes while timestamps
  don't, so an incremental build after a target switch silently validates the OTHER target's
  assemblies — purge `bin`/`obj`/`Generated` between target switches before trusting any build or
  suite that follows one.
  ⚠ **A CORPUS REFERENCE CLOSURE IS GOOS-CONDITIONED, LIKE THE COMPILE'S ITEM SET** (2026-09-08): a
  project reference under a `$(GoTargetOS)` condition belongs to THAT flavour's closure only, so a
  fold that walks every reference REGARDLESS of its condition reaches windows-only packages while
  deciding a darwin emission — one alias cut's single darwin footprint file was minted through a
  windows-conditioned edge, while the Go dependency list at darwin holds none of those packages at all
  and the darwin census compiles clean at master. **The scoping is the compiler's own rule, measured
  at the project file and at the loader, which is what makes narrowing the fold legitimate rather than
  fitting the answer.**
- **⚠ The corpus's emission cgo state is `CGO_ENABLED=0`, and a conversion against the committed
  tree must MATCH it** (measured 2026-08-29, net's Linux first contact — a corpus-level fact, not a
  lane detail: `net/linux/cgo_stub.cs` is on disk and `cgo_stub.go` is selected ONLY when cgo is
  off). This is the coherent convention — the converter cannot process cgo C halves regardless (it
  skips toolchain intermediates loudly since the Syntax-pairing fix), and Go's own cgo-off file
  selections are fully functional pure-Go paths. The trap is a MIXED-state tree: converting under
  cgo-ON against a cgo-OFF corpus changes the build-tag file selection, so declarations MIGRATE
  between files while the stale other-selection file remains — measured as a CS0111 duplicate init
  forcer in `net` (the forcer moved from cgo_stub.cs to dial.cs with both on disk). Reads exactly
  like a converter defect; it is an environment mismatch. On any Linux host with gcc (where cgo-on
  is the default), set `CGO_ENABLED=0` before converting against or regenerating the corpus.
  ⚠ **THE MIXED-STATE CLASS HAS A CONVERTER-INTERNAL MEMBER** (2026-09-08): the import-alias rename
  pass reads the Go loader's transitive closure AT THE LOADER'S RELEASE to decide whether a package
  alias collides with a child namespace, while the collision is decided AT COMPILE TIME by the C#
  namespace set of the CORPUS's transitive reference closure at the CORPUS's release — **the two
  agree iff the releases agree**, and a converter-pin move made them disagree: one package's internal
  children are re-homed between the two releases, so the ANCESTOR namespace the newer loader sees is
  brought into existence by a child the older corpus still references. Ruled fix: UNION the Go closure
  with the referenced corpus's transitive project closure — exact, never directory existence, which
  would over-approximate corpus-wide — falling back to the Go closure where no corpus project exists;
  a DECISION-level guard over synthetic closures plus a fixture corpus tree; and CNR under the newer
  pin reading 0 CHANGED as the acceptance.
  ⚠ **It bites from the TEST side too, and there the state is PER-PACKAGE** (measured 2026-09-02 on
  a Linux lane as a one-variable A/B on `os/user`, whose Go file selection is cgo-conditional): a
  sweep converts under the session's `CGO_ENABLED`, so a cgo-ON run selects `_test.go` files the
  cgo-OFF corpus never carried, leaves untracked `cgo_*_test.cs` artifacts behind, and dies in the
  closure build in ~12 s with **zero verdicts** — a build failure that reads like a conversion
  defect. Both comparison sides must share ONE cgo state, and the converted side can only be the
  corpus's. `run-validated-sweep.ps1` pins it per package (`$cgoOffPackages`, beside
  `$longTimeouts`); a row whose file selection is cgo-conditional joins that table rather than
  depending on the session it happened to run in.
  ⚠ **And a row can AGREE by a COINCIDENCE OF ERRNO** (measured 2026-09-02, one variable proven both
  ways): Go's `AllThreadsSyscall` tests skip on `ENOTSUP` because the ORACLE is cgo-LINKED, and the
  converted side skips on the same word because an unimplemented stub answers `ENOTSUP` — pass/pass at
  cgo-OFF, skip/skip at cgo-ON. So the `$cgoOffPackages` predicate is not only "does cgo change which
  files convert": the ORACLE's own behaviour (its cgo-gated branches) is a second axis in the
  predicate, and every row number carries the cgo state it was taken under.
  ⚠ **RULED 2026-09-03: cgo OFF is the state of record on EVERY platform, the sweep pins it for the
  WHOLE run, and the roster preamble states it once.** The Linux annotations had been banked without
  naming the axis: a cgo-OFF re-sweep read three rows LOW by exactly the ORACLE's `testenv.HasCGO`-
  gated tests (buildinfo +7, gcimporter +1, pe +3 skip/skip) with **zero verdicts moving**, because
  that bank host's default was cgo ON while the corpus is emitted cgo OFF. The evidence that decided
  it came from the OTHER platform — the Windows bank host has no C compiler, and its counts for the
  same rows ARE the cgo-OFF readings — so ruling the other way would have left the two platform
  bubbles on different unstated axes. A re-sweep already in flight at the ruled state IS the record;
  no second sweep is owed. ⚠ **The DRIVER's CONTEXT is a third axis a row number carries, beside cgo
  and host** (2026-09-04): two `syscall` tests skip under a detached no-TTY driver on BOTH sides and
  DIVERGE where a controlling terminal exists, so every fleet sweep matched them skip/skip and no
  sweep can see the divergence — **a bank states the terminal context it measured in.**
- **The on-disk corpus can be stale** relative to converter changes made since the last regen; building
  the committed tree measures *that* output, not today's. To measure the current converter you reconvert.
- **⚠ For ADDRESS-OF/ALIASING (`Ꮡ`-machinery) converter changes, a seeded corpus reconvert-and-BUILD
  joins the gate list — CNR alone is not sufficient** (proven 2026-08-15, the element-field-address
  fix: of the three defects in that arc, CNR caught ONE; the other two — a pointer-receiver named-array
  blind spot and its over-broad first fix — appeared in NO behavioral test's shape and were found only
  because the whole corpus was reconverted and compiled. The same census surfaced a real shipped lost
  write, `encoding/xml`'s attribute-namespace translation writing into a copy). The behavioral corpus
  is a SAMPLE of Go's shapes; the stdlib is the population — aliasing changes get measured against the
  population before banking.
- **⚠ Blast-radius measurement for a converter change: TWO seeded reconverts diffed against each
  other, never the committed-tree diff** (2026-08-29, the position-table splitter fix). A naive
  reconvert-vs-committed diff reported 147+ files — almost all PRE-EXISTING unbanked drift from
  arcs that landed without their regens (the standing position-map staleness two census lanes
  rooted independently the same day). Seeding one root at the PRE-change converter and one at the
  CHANGED converter and diffing the two emissions isolates exactly the change's own footprint
  (26 metadata files, zero production code, in the measured case). The committed tree is a moving
  baseline; two emissions of the same sources differ only by the change.
  ⚠ Two mechanical tells the ritual owes on EVERY run (paid 2026-09-01 — a lane's "old" binary
  never existed and the diff silently compared committed-tree-vs-fixed, reporting 724 phantom
  files): (1) `go build -o <path> <dir>` with a bare directory as the last positional can land
  the binary at `<dir>/go2cs.exe` and leave the named `-o` path NONEXISTENT while exiting 0 —
  verify the built binary exists at the exact path you will invoke; (2) before trusting any
  two-root diff, assert BOTH sides' emitted files carry THIS RUN's mtimes — a diff between a real
  reconvert and an untouched seed returns a normal-looking result with nothing marking it invalid
  (the emitted-before-seeded family, build-step edition).
  ⚠ **Tell (1) is TOO WEAK when a STALE file already sits at the invoked path, and was strengthened
  2026-09-05**: `go build -o` fed an MSYS `/c/…` path wrote into a stray `C:\c\` tree while the
  existence-and-size check read the STALE binary sitting at the right path and passed. **Assert the
  binary's MTIME MOVED and that it is newer than its sources** — which is the converter's own
  staleness predicate, and note that checking out a branch whose converter sources are newer than the
  binary is exactly what that guard REFUSES, correctly.
  ⚠ **And it runs over the WHOLE corpus, never scoped to the package the cut names** (2026-09-04),
  because a scoped census reproduces its own scope: the whole-corpus run found both a publication
  defect and fourteen sites in `sync/map.cs` that the name-keyed census had not predicted.
  ⚠ **A THIRD failure mode neither documented tell covers (2026-09-04): a script that SEEDS EACH ARM
  from the LIVE worktree at the moment that arm starts, while the lane edits that worktree between
  arms, seeds `base` from one tree and `new` from another** — and the diff then reports files as
  "differing" that NEITHER converter wrote, reading exactly like a footprint. Both standing tells
  passed (the binary at the exact path; this run's mtimes on both sides). **Take EVERY seed before ANY
  arm converts, or seed from a frozen snapshot (`git archive`)** — and make the write-evidence check a
  CONTENT test, never a timestamp: hash a `[module: GoManualConversion]` file across every seed and
  require equality, since a file neither converter writes must be byte-identical everywhere. That
  marker file must sit IN A PACKAGE THE ARC TOUCHES (a hand-own in an untouched package discriminates
  nothing); where the affected packages hold none, fall back to whole-tree content identity outside
  the emitted set. Mtimes are a HINT only — `find -newermt` reported 179 `src/core` files inside one
  run's window that were a branch switch completing before the first seed, and trusting them would
  have discarded a sound A/B — **but a written count of ZERO is decisive where a nonzero one is not**
  (see the fourth mode below). Two mechanics from the same run: derive hunk anchors from the BASE
  EMISSION (difflib opcodes) so no converter glyph passes through a shell (a heredoc mangled one,
  caught at zero matches — the LF-anchor family through its glyph door); and **`git show HEAD:<file>`
  returns the LF blob against a CRLF checkout** (1,408 "differing" lines that CR-strip to 18), so name
  the LAYER before quoting a number.
  ⚠ **A FOURTH mode, 2026-09-05, and it is the emitted-vs-seeded trap's PUREST instance: both arms
  piped through `Select-Object -First N` were KILLED at exit −1 with ZERO `.cs` written, and the
  comparison of two untouched seeds read 0/0/0 — indistinguishable from a clean gate.** (The
  `-First N` kill is the documented pipeline trap; what is new is that it can take out an A/B's arms
  silently, since the trap's usual tell — a visibly dead run — is what the diff then reads as a
  green.) Three tells, one decisive: an impossible WALL (minutes where a three-target conversion
  floors at ~9), the exit code, and a per-arm per-target count of files WRITTEN this run. **The diff
  script ASSERTS the written count and ABORTS on an arm that emitted nothing**, and **a lane that
  finds its own vacuous reading RETRACTS it publicly with the mechanism before anything seats on it.**
  ⚠ Beside it: **a two-seeded diff NAMES the toolchain that built EACH arm** (`go version <binary>`) —
  a `go build` in a shell without the GOROOT pin stamped one arm go1.24.7 against the other's
  go1.23.12, caught only by byte-comparing a pinned rebuild against the running binary.
- **Reconvert → overlay → build → bucket (the measurement loop):**
  1. **⚠ SEED FIRST — non-negotiable (learned 2026-07-25, cost a false operational-break alarm):**
     `cp -r src/core <tmp>/core` BEFORE reconverting. ⚠ The SEED ITSELF can fail halfway and
     carry on (fleet-confirmed twice in one day, 2026-09-01): `Copy-Item -Recurse` dies on
     go2cs-gen's long `obj\...\Generated\` paths, and under the ritual's own required
     `$ErrorActionPreference='Continue'` the copy continues past the death — a PARTIAL seed,
     which is the unseeded-root hazard through a door this rule never named. Exclude build output
     (`bin`/`obj`/`Generated`) from the seed copy and verify the seeded `.cs` COUNT before
     converting; afterward, an emitted-files control (untouched seeded files reproducing HEAD
     byte-for-byte) is what makes a suspect seed's readings trustworthy. The converter emits a hand-owned
     file as `<file>.cs.auto` ONLY when the `[module: GoManualConversion]`-marked file already
     exists at the output path; an EMPTY temp root gives the marker nothing to detect, so every
     hand-owned whole-file rewrite is emitted as plain `.cs` and the standard overlay rule
     ("copy `*.cs`, exclude `*.cs.auto`") protects NOTHING — 14 hand-owned files get clobbered
     with auto conversions that COMPILE but are operationally broken (godebug's auto `init()`
     throws in a module initializer and takes down every dependent). **Hard gate before
     overlaying — PATH-PRECISE, not a count:** for every `[module: GoManualConversion]`-marked
     committed file, the temp root must NOT contain a freshly-EMITTED plain `.cs` at that path
     (either a `.cs.auto` sits beside it, or nothing was emitted there). Counts intentionally
     differ — **41 marked files (re-measured r44a, 2026-08-07) but only 15 produce `.cs.auto`**; the
     other 26 are `*_impl.cs` companions and hand-owned packages the converter never re-emits at
     that path, so they need no protection. A same-count assertion is wrong in both directions.
     ⚠ The number is NOT stable: it was 40 at r40, fell to **39** when `math/unsafe.cs` shed its
     marker, returned to 40 when `internal/weak/pointer.cs` joined at r43e, and is **41** since
     `internal/cpu/cpu_x86_impl.cs` joined at r44a. This is exactly why the census is re-measured,
     never carried forward.
     **The marker scan must read WHOLE FILES** — a head-window scan (e.g. first 40 lines) reported
     35 marked files against the real 60 (measured 2026-08-17), which would have made the clobber
     gate vacuous for 25 hand-owns; some markers sit below long license/using blocks.
     **The marker scan must be LINE-ANCHORED (`^\s*\[module:\s*(go\.)?GoManualConversion\]`)** —
     `reflect/value.cs` and `internal/reflectlite/value.cs` *mention* the marker inside
     bodyless-partial placeholder comments; an unanchored `grep GoManualConversion` reports **63**
     against the real 40 and turns the gate into a false clobber alarm. (The census moves in BOTH
     directions — 32 at r14, 39 before `internal/concurrent`'s `hashtriemap.cs` joined in r39d, 40
     at r40, DOWN to 39 when the r41 train's regen retired `math/unsafe.cs`'s hand-own without
     saying so — the BitConverter bit casts went back to the auto
     `Ꮡf.Reinterpret<float32, uint32>()`, correct now that `Reinterpret` genuinely aliases managed
     storage, and `math`'s banked **76/76** re-proves it every sweep — and back to 40 when
     `internal/weak/pointer.cs` joined at r43e. Benign in that instance, but a hand-own disappeared
     under an overlay while the commit reported its marker gate "40/0": so re-measure the census,
     never assert last session's number, and treat a SHRINK as something to explain rather than to
     copy forward. ⚠ Since r50a the census counts **42**, and for a NEW reason: layout L3 routes a
     hand-owned file into its principal's per-GOOS folders, and `runtime/lock_sema_impl.cs`'s
     principal is selected on Windows *and* macOS — so one hand-own now exists as TWO files. The
     count of marked FILES is no longer the count of distinct hand-owns; both numbers are fine and
     the gate is still per-PATH. Since **r51b it is 44**: `runtime/lock_managed_impl.cs` (the flat,
     platform-neutral managed core of the mutex/note protocol) and `runtime/linux/lock_futex_impl.cs`
     (the futex flavor's 2-arg `notetsleep_internal`) both carry the marker. Multiple `[module:
     GoManualConversion]` attributes in ONE assembly are legal and already normal — `runtime` alone
     carries eight — so a new marked file never needs to displace an existing one. ⚠ At the r59
     regen bank (2026-08-11) the census is **49 marked files / 41 `*_impl.cs` companions / 59
     distinct hand-owns** — r52–r59 growth over r51b's 44; re-measure, never carry, as always.
     At the Linux regen wave (2026-08-14) it is **53 marked files / 42 `*_impl.cs` companions**,
     0 violations across 3 targets × 2 merge passes. At the post-merge rebank (2026-08-24) it is
     **73 marked files / 49 `*_impl.cs` companions / 24 whole-file rewrites**, 0 violations on the
     windows and the linux target alike.
     The regen ritual also gained a check the seed makes necessary: because seeding puts every
     repo file in the temp root, an overlay can never reveal a file the converter has STOPPED
     emitting — classify emitted-vs-seeded by the sentinel mtime and report would-be deletions,
     which is what surfaced the hand-owned-by-consequence class below.) ⚠ The `.cs.auto` siblings are **tracked in git but are NOT refreshed by the
     overlay**: the same exclusion that protects the hand-owned `.cs` beside them also freezes
     them, so they go stale on their own schedule and are RE-MEASURED at every rebank head rather
     than assumed (CleanupBacklog item 18). The measurement moves: 11 of 16 were stale at r40,
     and **0 of 23 at the 2026-08-24 post-merge rebank** — a seeded reconvert per target re-emits
     each sibling, and CR-stripped equality against the committed file is the test (a raw byte
     compare reports the whole set as differing, because a fresh emission carries the in-literal
     LF the working tree holds as CRLF).
  1a. **⚠ SEED `version.props` AND `docs/validation` TOO, and MIRROR THE `src/` LAYOUT** (added
     2026-08-02 with the README validation badges). Each package README's badge LINE carries two
     badges, and the **Tests** one is composed from two REPOSITORY files, not from the conversion:
     `src/version.props` (the published version that pins the proof URL) and
     `docs/validation/current/<dot-id>.md` (the matched/disclosed counts).
     The converter finds both by the same upward walk it uses for `$(go2csPath)` — version.props at the
     root holding `core/golib`, `docs/` as that root's SIBLING — and emits **no Tests badge at all**
     when either is missing, which is a silent, corpus-wide README diff on overlay. So seed
     `<tmp>/src/core`, `<tmp>/src/version.props` and `<tmp>/docs/validation`, and convert with
     `-go2cspath <tmp>/src` so the temp root mirrors the repository. (Seeding a versioned
     `docs/validation/<version>/` is NOT needed — the badge reads `current/`; the versioned directory
     is only the link target and the `Exists`-guarded pack input.)
     ⚠ The badge line holds FOUR badges and they split two-and-two on this exact question. **Docs**
     (2026-08-08) and **Source·Go** (2026-08-08) read the TOOLCHAIN, not the repository — `go env
     GOVERSION` and, for the 19 GOROOT-vendored `golang.org/x/*` packages, GOROOT's own
     `src/vendor/modules.txt` — so they need no seeding and survive an unseeded root. **Tests** and
     **Source·C#** read the repository's `version.props`, so both vanish without it. That is why an
     unseeded reconvert no longer produces a README with NO badge line: it produces one carrying the
     two toolchain badges alone, which is a subtler diff to spot. Seed anyway; the rule is unchanged,
     only the symptom is.
  1b. `go2cs.exe -stdlib -comments -go2cspath <tmp>/src` → output lands in **`<tmp>/src/core/<pkg>`**
     (the `core` subdir is hardcoded; `-go2cspath` is the *output* root, unrelated to the MSBuild
     `$(go2csPath)`). Full stdlib ≈ 3–4 min (per-file work is sub-second; the cost is `go/packages`
     loading the whole type graph, so **batch** — don't invoke per package).
  1c. **⚠ NEVER convert twice into the same temp root, and never let two conversions overlap in one
     (found r41, 2026-08-05).** A `-stdlib` run whose PowerShell wrapper aborted on the converter's
     stderr WARNINGs — `$ErrorActionPreference = 'Stop'` turns a native-stderr line into a terminating
     NativeCommandError, so wrap the converter call in `'Continue'` or do not pipe its stderr at all —
     left a `go2cs.exe` alive; a re-run into the same root raced it, and the result was ONE corrupted
     file: `runtime/arena.cs` with nine unresolved `«DYNTYPE:…:DYNTYPE»` anonymous-struct lift markers,
     which fails the corpus build with CS1056/CS1003 and reads exactly like a converter regression. It
     is not one — a clean-room reconvert (fresh root, seeded, single run) emits zero DYNTYPE markers
     anywhere in the corpus, and so does a single-package run. The rule is therefore mechanical rather
     than diagnostic: **delete the temp root and re-seed for every reconvert**, and confirm no
     `go2cs.exe` is alive before starting one.
  2. Overlay the fresh `.cs`, **`.csproj` and `README.md`** onto `src/core/<pkg>`. Since the trees
     unified (2026-08-01) the reconvert's paths ARE the repository's paths — a straight copy, no
     rewriting, no exceptions. A seeded reconvert of the whole stdlib is byte-identical to the
     committed tree (2518 `.cs`/`.csproj` verified on the consolidation commit; 300 `README.md` joined
     the byte-identical set on 2026-08-02), so any diff after an overlay is a real converter change.
     Two knowns that are NOT: the SIX root attribution files the converter re-copies (`src/core/README.md`
     and its five siblings — measured 2026-08-17; this note previously named only the one — all show
     modified with an EMPTY `git diff --numstat`, pure CRLF phantoms; restore them), and
     the hand-owned-by-consequence **class of FOUR** — `crypto/internal/boring/bcache`,
     `internal/concurrent`, `internal/godebug` and `internal/weak` (censused 2026-09-01 at
     `3e31de03a` over all 306 production packages; the note previously said three, and before that
     godebug alone — bcache was the member nobody had counted, evidenced by the hand-edited
     position-map hash in its `package_info.cs` at `f1df6cbd9`, which a re-emitting converter would
     never need a human to fix) — each a package whose every non-test Go file is hand-owned, so
     `unmarkedFileCount == 0` makes the driver `continue` before `writeProjectFile` and its
     `.csproj`, `package_info.cs` **and `README.md`** are hand-owned by consequence, never
     re-emitted. (`unsafe` is also fully hand-owned but by the OTHER mechanism — skip-listed.)
     Consequence counted the same day: the hand-own FENCE leaves **8 forced-init hooks missing**
     inside this frozen class (godebug 4, concurrent 3, weak 1) that only Stage B's frozen-README
     option (a) can fix — the relocation cannot, since these `package_info.cs` are never re-emitted.
  3. Build single packages with **`dotnet build <pkg>.csproj -c Debug`** — `src/core/Directory.Build.props`
     pins `$(go2csPath)` to the src root, so `core\golib` + the `go2cs-gen` analyzer resolve to live source
     with **no `-p:go2csPath` flag**; or build the whole `go2cs-stdlib.slnx` (~92–150 s warm, 305 assemblies — the 306th, `crypto/x509/internal/macos`, is darwin-exclusive and compiles nothing under the default `$(GoTargetOS)`).
     (If you ever do pass the flag explicitly, use forward slashes —
     `-p:go2csPath=H:/Projects/go2cs/src/` — a trailing `\` escapes the closing quote and mangles the path
     into phantom golib-not-found errors.)
  4. Bucket: `dotnet build … -clp:ErrorsOnly` then group by `error CS####`. Errors shown are *own-errors*
     of leaf-most failures — dependents of a failed project are skipped, not errored.
- **⚠ csproj I/O (2026-07-25):** any script that rewrites a csproj must read AND write with explicit
  UTF-8/no-BOM (`[System.IO.File]::ReadAllText/WriteAllText` + `UTF8Encoding($false)`) — PS 5.1
  `Get-Content` reads the converter's BOM-less UTF-8 as ANSI and `Out-File utf8` re-encodes the damage,
  double-encoding the `©` in `<Copyright>` on every pass (this is what created, then tripled, the
  258-file corpus mojibake; root-caused and leveled in the r11 bank). Python has the same trap in
  the OTHER direction: `utf-8-sig` STRIPS a BOM on read but always ADDS one on write, so a
  read-sig/write-sig round trip silently BOMs a BOM-less file (caught 2026-08-31 by the
  hand-application byte-identity bar during a probe restore). Three encodings, three silent
  corruptions — PS 5.1 ANSI, UTF-16 redirects, utf-8-sig — one rule: byte-compare any
  restore/round-trip against the original before trusting it.
  ⚠ **The BOM round trip breaks in BOTH directions, and the second was measured 2026-09-03:** reading
  with `utf-8-sig` and writing with plain `utf-8` **STRIPS** a BOM the file had — two `go2cs-gen`
  sources lost theirs, caught only by reading the FIRST line of the diff (`-<BOM>// …` against
  `+// …`). **After any scripted edit, diff the file's FIRST and LAST lines specifically**: encoding
  damage (BOM, trailing newline, CRLF) lands there, and review attention is weakest there.
- **Metric:** measure **packages-compiling**, not raw error count. Fixing file-inclusion bugs (e.g. the
  filename build-constraint fix) *raises* the error count because newly-included files surface their own
  latent defects — that's progress, not regression. The claim "my fix caused N new errors" is
  therefore never banked without the five-minute control (named 2026-09-01, after a
  substantially-correct fix was discarded on the misread): REVERT the fix, build PAST the original
  blocker, and see whether the "new" errors are still there. Unmasked errors appear precisely where
  compilation could not previously reach — i.e. in files OTHER than the ones you touched — so "the
  errors are in different files from my change" is evidence of unmasking, never evidence of
  causation.
  ⚠ **The rule read from the other direction: a CLEAN second half is believable only after the first
  half is FIXED** (2026-09-05, the field-walk dedupe): fixing the walk UNMASKED a CS1929 on the METHOD
  half of the same row that the unfixed generator had reported clean — and the second half needed a
  DIFFERENT fix (depth-aware promotion inside an embed), not the first fix applied twice. Errors behind
  a blocker are invisible until the blocker moves, so a two-half row is re-measured after each half.
- **A corpus regen that moves `package_info.cs` records owes `go generate .` in `src/go2cs`** —
  `stdlib-metadata.txt` is generated FROM the corpus and gated by `TestStdLibMetadataInSync` under the
  plain converter `go test`, so banking a regen without the regenerate leaves the converter gate red at
  master for whoever runs it next (happened 2026-08-15: the second leveling regen moved 6 records and
  the drift surfaced in an unrelated lane's gate run). Regenerate, verify the test, commit together.
  ⚠ **A DELETING INSTRUMENT DERIVES ITS CANDIDATE POPULATION FROM WHAT THE CONVERTER EMITS, AND RUNS
  EVERY REFUSAL BEFORE ITS DELETION LOOP** (2026-09-07, `reconvert-deletions.ps1`, found by a lane on
  the first real dry run and NOT applied). (a) The candidates are what the converter EMITS — in
  `go list std` at the SOURCE release and not skip-listed — never a decider that answers a meaningless
  question for a hand-written directory: `go list` said "package not in std at target" for golib
  (116 files) and for the Symbols shared project, TRUE and IRRELEVANT, while the marker-based PROTECTED
  arm cannot see a directory that correctly carries no marker — **the one directory that needs no
  marker is the one the guard does not protect.** (b) Every refusal runs BEFORE the deletion loop, so a
  non-zero exit MEANS nothing was removed; the landed script exited 2 AFTER deleting, which is a report
  wearing a refusal. Its prediction lesson: an instrument answering the DELETION-PASS question (whole
  removed packages plus non-Go directories = 205) and a census answering the LIVE-PACKAGE question (25)
  are two questions — predict against the one the instrument asks. ⚠ Beside it, the idiom that minted a
  phantom row: **PowerShell `$x[0..($x.Count - 2)]` on a SINGLE-element array yields TWO elements**
  (`0..-1` counts DOWN to `@(0,-1)`), which turned `core/GlobalUsings.cs` into the import path
  `GlobalUsings.cs/GlobalUsings.cs` and a DELETE-ABSENT row (`ed191e9a8`) — guard the part count before
  slicing, and grep the other `src/*.ps1` instruments for the idiom. Ruling from the same read: **a
  deleting instrument REFUSES while UNRESOLVED rows stand** (dry run → a human disposes of them →
  `-Apply`), never "delete the DELETE rows and still exit 2".
- **Don't commit corpus regens casually.** `src/core/<pkg>` is regenerable; the unit of work is the
  **converter fix**. Keep the tree restorable (overlay into a branch or restore with `git checkout HEAD --`
  + remove untracked) so a converter-fix commit isn't buried under thousands of generated-file changes.

### Deploying the core to the GOPATH root
`src/deploy-core.ps1` (cmd launcher `deploy-core.bat`) stages the runtime + standard library at
`%GOPATH%\src\go2cs` so converted projects — and, later, recursively converted end-user apps that target
that same root — resolve their `$(go2csPath)core\<pkg>` / `gen\go2cs-gen` references relatively. It has ONE
mode since the trees unified (the old `stub`/`stdlib` argument is gone): a straight copy of `src/core`,
because the repository layout and the deployed layout are now the same layout and no reference needs
rewriting. It also deploys the `go2cs-gen` analyzer, writes a root `Directory.Build.props` that pins
`$(go2csPath)` to the deploy root (so no `-p:go2csPath` is needed), generates `go2cs-core.slnx`, and builds
to verify. The other src PowerShell utilities `clean-bin.ps1` (remove bin/obj/Generated) and
`set-version.ps1` each also have a `.bat` launcher.
⚠ Purge with that instrument, or with an explicitly depth-UNLIMITED walk: an ad-hoc
`find … -maxdepth 3` purge missed 274 of 388 output directories and drove a lane's disk into the
harness's own free-space floor (2026-09-02). **Non-interactively — a background task, a harness
tool call, any `-NonInteractive` host — pass `-Force`**, and invoke it as `clean-bin.bat -Force` or
`powershell -NoProfile -ExecutionPolicy Bypass -File .\clean-bin.ps1 -Force`: the script is
unsigned, so on any host whose execution policy requires signing a bare `powershell -NoProfile
-File` dies "is not digitally signed" (observed 2026-09-03; it does NOT reproduce on a box whose
LocalMachine policy is already `Bypass`, which is exactly why the bypass belongs in the invocation
rather than in an assumption about the host — the `.bat` already carries it and forwards arguments). `-Confirm:$false` is equivalent in-process but
does NOT bind through `powershell -File` on 5.1, which literalizes the argument and rejects it
before the script runs — `-Force` is the contract. **Its exit code is load-bearing before any build
that follows a purge** (0 = everything found is gone; 1 = declined; 2 = the host could not prompt
and `-Force` was absent; 3 = `-WhatIf`; 4 = something survived): a found-but-not-deleted run never
exits 0, and a wrapper that captured a non-zero clean and carried on ran a target-switch build
without the purge it reported attempting (2026-09-03) — the same day the other half of that hole
was met, where `Read-Host` on EOF printed "Found 2866 folders to delete. Operation canceled." and
exited **0** having deleted nothing. Read the code through `-File`/the `.bat`, which propagate it
exactly; `-Command "& …"` collapses every non-zero code to 1. And the purge is only the BELT: after
a `$(GoTargetOS)` switch the braces are the per-target compile item-set read
(`dotnet msbuild -getItem:Compile` — e.g. 39 windows / 0 linux under one `GoTargetOS` and 0 / 75
under the other), and only that second reading answers the question the purge exists for.
⚠ **A BUILD-OUTPUT PURGE ACROSS KEPT WORKTREES IS GATED BY A PROCESS CENSUS AS A PRECONDITION THAT
ABORTS** (2026-09-08) — purging `obj` under a live build corrupts a run that surfaces later as
somebody else's false red — **enforces "never `Generated`" by MEASUREMENT** (counts recorded before
and after, identical in every worktree, since that directory is not gitignored), and **proves "no
source touched" by the load-bearing guard**: every worktree's dirty count re-read afterwards and
unchanged. The reclaimed space EXCEEDED the estimate, and the estimate is the lesson — it came from a
depth-limited `find` and was a FLOOR mislabelled as a measurement, the same class as a `-prune` that
answers a narrower question than the one asked.
⚠ **And its `-Root` default resolves to an EMPTY STRING under `powershell -File`** (2026-09-05):
`Resolve-Path` refuses the empty path, the script then enumerates the CWD anyway and reports "Found
2640 folders", and the run exits 2 having deleted NOTHING. **Pass `-Root` explicitly as well as
`-Force` for any non-interactive purge**, and read the EXIT CODE plus a dirs-left count — never the
log's "Found N", which is what the enumeration found and not what it removed.
⚠ **PURGE AFTER EVERY PROBE, and read an I/O error HISTOGRAM before believing an error count**
(2026-09-06, one disk exhaustion in two costumes). A coordinator's own two scratch worktrees held
**5,096** build-output directories and filled the disk to zero: the fleet's mailbox push failed on
`Out of diskspace`, and a solution leg that had built clean an hour earlier came back exit 127 with
**78 errors** whose histogram was MSB3491/MSB3027/MSB3021/CS0016/CS0041/CS8104 and every message
read *not enough space on the disk*. **N errors whose codes are all I/O are ONE environmental
failure wearing N codes, not N defects** — purging recovered 43 GB. Two readings ride on it: a
measurement taken on a tree that CHANGED under it — here a `bin`/`obj` purge while MSBuild was
writing there — is not a measurement at all, whichever way it reads; and what made the exhaustion
VISIBLE rather than silent was the posting tool REFUSING to claim delivery and saying the commit
did not land, so **a state-advancing tool that reports its own failure honestly is worth more than
one that succeeds quietly.**
**⚠ Its default target is MACHINE-GLOBAL** — `%GOPATH%\src\go2cs`, shared with every sibling worktree —
so never run it bare as a gate. It supports **`-WhatIf`** (a real dry run: the three non-cmdlet writes
are explicitly `ShouldProcess`-gated, and the solution enumeration reads the SOURCE so the projected
project count is truthful) and **`-Target <dir>`** for a scratch deploy. The copy is pure PowerShell
(`Copy-SourceTree`) rather than robocopy since 2026-08-08, and is byte-identical to what robocopy
produced (3,979 files A/B-verified); that also removed the repository's last external Windows tool
dependency. Harness path/platform primitives (`$IsWindowsHost`, `$ExeSuffix`, `$SepPattern`, the roots,
`Get-PathDepth`) live in **`src/_paths.ps1`**, dot-sourced by every instrument — never re-derive them,
especially `$IsWindowsHost` (`$IsWindows` does not exist on PowerShell 5.1, so a bare `-not $IsWindows`
reads backwards on the one platform 5.1 runs on).

