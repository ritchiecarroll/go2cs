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
     justifies. Nothing deleted. Byte-identical pre-split original: docs/doctrine/JOURNAL-2026-09-12.md.
     BATCH19 MERGED 2026-09-12: the items routed here from branch claude/coord-doctrine-batch19 (commits 24bfc8304 / c5e17217b /
     e9e56b657, pure insertions into the OLD CLAUDE.md shape at anchors 3647 / 3761 / 4250) are folded in below — most as further
     evidence under rules this file already carried, three as new or amended visible rules.

     MOVED OUT 2026-09-12 (same branch, from 86037ef2e): two sections left this file for
     .claude/skills/corpus-reconvert/SKILL.md — the one on measuring a converter change and the one on
     the reconvert/overlay/build/bucket loop — whole, each rule with its attached HTML comment, and
     MERGED into that skill's existing ordered procedure (§2 emit, §3 diff, §4 apply, §5 verify)
     rather than appended as a second copy of the loop. WHY: this file is PATH-SCOPED
     (paths: src/core/**), so it loads IN FULL for anyone — subagents included — who reads ANY file
     under src/core, while a skill's body loads only when the skill is invoked. Those two sections are
     a PROCEDURE you invoke rather than facts you need in hand while editing a corpus file, and
     corpus-reconvert already advertised exactly that remit ("the seeded two-seeded diff, the hunk
     rule, marker gates, per-target emission and the reconvert-overlay-build-bucket loop"). What STAYS
     here is what answers "may I regenerate this file / how is the tree shaped": the five kinds,
     layout L3, the cgo state of record, encoding-metric-banking a regen, the deleting instrument,
     deployment. NOTHING was distilled on the way out — every date, count, package name, error code,
     glyph and named incident from both sections is in the skill's comments, where it costs zero
     tokens exactly as it did here; 22 rule units landed there as 17 visible lines plus one fold (the
     `.cs.auto`-not-refreshed-by-the-overlay rule, which the skill already carried as "named, not
     overlaid"). The two sections as this file last held them are at 86037ef2e:.claude/rules/corpus.md
     (this file was 236 effective lines then, 162 after); the byte-identical pre-split original of both
     is docs/doctrine/JOURNAL-2026-09-12.md. They are replaced below by ONE pointer line, deliberately
     not a section. -->

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
- **A hand-own that rewires ONE CALLER of a runtime door and leaves the DOOR ITSELF leaves a silent no-op
  for the next caller.** Rewire or RETIRE the door, and grep the callers of every door a hand-own retires.
  <!-- ⚠ 2026-09-13, C1 f9f41e8d8 §3, with COORD ruling `runtime/mcleanup.cs` a hand-own OWED rather than a
  free add at H5. `mfinal.cs:496` rewired `SetFinalizer` to `GoFinalizerQueue.EnsureRunner`; `createfing`
  at :182 was NEVER rewired and still starts the CONVERTED runner that :644 calls vestigial and the file
  header declares DEAD. 1.24's `mcleanup.cs` is one of 12 new files and its `AddCleanup` calls
  `createfing()` at :115 — so adding it as the plain auto makes `runtime.AddCleanup` compile, return a
  `Cleanup`, and never run one, with no throw and no diagnostic, invisible at the moment it would be
  introduced. -->

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
- **A HAND-OWN'S DISPOSITION AT A HOP IS READ AT THE PRINCIPAL, NOT AT THE FILE** — principal
  byte-identical under a new package = RELOCATE (one registry key); principal ABSENT = DELETE, never
  relocate; principal moved AND changed = relocate plus a PRINCIPAL CHANGED row. <!-- ⚠ 2026-09-13, COORD
     `46198c1b9`. A whole-file replacement with no principal is a file no reconvert writes an `.auto`
     beside, which is what makes DELETE the only sound answer for `xor.go` and the whitebox's
     `dumpMap`/`dumpNode`. For `hashtriemap` (14 -> 28 funcs) and `weak` (`Strong` -> `Value`) the 1.23
     body is carried ON PURPOSE, because the auto it displaces is the one that COMPILES and lies — safety
     floor 2 in miniature. And a registry key whose declaration was RENAMED is found by SIGNATURE, never
     by a look-alike name: `getgcmask(ep any) []byte` became `pointerMask`, not `getGCMask(t *_type)
     *byte`, and two lanes each measured the wrong target correctly. -->
- **MOVES GO IN GIT BEFORE THE RECONVERT, NEVER IN AN INSTRUMENT — AND A DELETION INSTRUMENT NEVER PERFORMS
  A PARTIAL MOVE.** <!-- ⚠ 2026-09-13, COORD `46198c1b9` s3 and C2 `cb1e4aaf6`. `.cs.auto` siblings are
     TRACKED and the converter writes them by its own rule when the hand-own is at the output path at
     conversion time, so a relocation committed as `git mv` (auto -> `.cs.auto`, hand-own onto the path)
     lets the seeded reconvert produce the pair and the placeholders, and H5c's ORPHANED class then reads
     0 with its firing arm proven by the control. The instrument's `-Orphan path=relocate:pkg` was a bare
     `Move-Item` with ZERO occurrences of "namespace" — it would have landed a file at the right path with
     the WRONG identity — and was narrowed to a REFUSAL with a pointer; `delete` was kept, because nothing
     survives to carry a wrong address. After a proper move the orphan set is empty by construction. -->
- **`git mv` CARRIES THE PATH AND NOT THE C# IDENTITY: THE NAMESPACE AND CLASS LINES *ARE* THE MOVE.**
  <!-- ⚠ 2026-09-13, C1 `8f800233b` / COORD `43ce0c8e6`. All three relocated hand-owns kept their SOURCE
     namespace and class, landed in a lone partial-class part beside the destination's `package_info`
     stub, and presented as CS1061 missing-member on exactly the symbols the H6 wall was predicted to
     name — a defect wearing the costume of a ruled-ignorable failure — while the alias companion was
     silently INERT and the defective auto shipped. The no-build discriminator is `grep -m1 '^namespace'`
     on each moved file against its siblings. Same-layer trap in the same cut: an LF blob from `git show`
     compared against a CRLF checkout reads DIFFERS at line 1. The intermediate CS0111 (both spellings of
     a displaced body until the reconvert runs with the new registry key) is a property of the ORDER —
     loud and self-naming — and not a camouflaged defect; but an upstream dependency failure MASKS every
     downstream reading, so "no CS1061 in `unique`" is UNMEASURED, not refuted (i9 `0cfc5f33c`). -->
- **`go2cs-gen` READS THE PACKAGE ADDRESS OUT OF THE HAND-OWN'S OWN FILE AND NEVER CHECKS IT AGAINST THE
  PACKAGE BEING COMPILED — so a half-moved hand-own is MIRRORED into a consistent generated companion and
  produces ZERO build errors on any platform.** The only instrument that can catch it is a TEXT guard.
  <!-- ⚠ 2026-09-13, C1 `d85d4ea2c`. `ImplementGenerator.cs:155-157` takes the namespace and first class;
     `TemplateBase.cs:87` emits `namespace {{PackageNamespace}}; public static partial class
     {{PackageName}}_package`. That mirroring is what made the CS1106 prediction wrong. The guard: every
     `[module: GoManualConversion]` file's namespace and package class equal its directory's other
     non-`.auto` files, with a planted-mismatch firing arm and a vacuity refusal. -->
- **"THE FILE IS EXCLUDED" AND "THE BODY IS DISPLACED" ARE DIFFERENT MECHANISMS WITH THE SAME VISIBLE
  RESULT** — looking for a file exclusion finds nothing, and "nothing found" reads as "does not transfer".
  <!-- ⚠ 2026-09-13, C1 `76e4026ae`. The runtime lock files ARE converted and compiled;
     `manualConversionFuncs["runtime"]` (seven names at `goosAny`) replaces each registered BODY with a
     placeholder and the flat managed core supplies it, so `m.nextwaitm` reads 0 references because every
     function that would touch it is a comment. The registry is what the converter guards test:
     registration alone at 1.23.12 goes red in two guards naming both halves, which makes the row
     hop-conditional and MEASURED rather than reasoned. -->
- **A STRANDED HAND-OWN IS AN H6 METHOD GAP: A HOP ADDS A BODYLESS DECLARATION TO A PACKAGE WHOSE COMPANION
  COVERS THE OLD DECLARATION SET — THE COMPANION STILL COMPILES, THE NEW DECLARATION STUB-THROWS, AND
  NOTHING SAYS SO.** <!-- ⚠ 2026-09-15. `time.runtimeNow` arrives at 1.24 while `time_impl.cs` hand-owns
     `now()`/`runtimeNano()`; `internal/sync`'s six bridges sit with `sync`'s companions left behind. The
     H6 member-body arm compares REALISED functions and can never see an ARRIVED declaration with no body,
     so the audit gains the arm and the 24 hop-new throwing-stub push targets are its population. A
     registry forward is NOT the remedy when the pushed body itself lands on another stub. -->
- **THE CENSUS PREDICATE FOR A CLOSED CLASS IS THE FORK — THE GO IDIOM — NOT THE WRAPPER AND NOT THE
  STRUCT: a class closed in one package can be OPEN in a sibling package's own copy of the same fork.**
  <!-- ⚠ 2026-09-15, C1 `9a7789127a`. `syscall/windows/security_windows.cs` was hand-owned in August for
     "a kernel byte buffer the caller reinterprets" — pinned buffer, native `ж<SID>` boxes, a
     `ConditionalWeakTable` anchor, SIDs not copied because `GetLengthSid` is a call THROUGH the address
     in question. `internal/syscall/windows` declares its OWN `TOKEN_GROUPS` and `getTokenInfo` loop, and
     nothing reached it until 1.24 moved `os/user` onto the process token. -->
- **A GO TEST THAT OBSERVED A HAND-OWN AT ONE PIN MAY NOT AT THE NEXT: RE-DERIVE EACH ROW'S OBSERVER AT THE
  PIN.** <!-- ⚠ 2026-09-15. `os/user`'s `TestGroupIds` takes the process-token path at 1.24
     (`Current().Uid == user.Uid`), so `listGroupsForUsernameAndDomain` is reached only by a test that
     creates an account — skipped outside Go's own builders. When Go's suite cannot reach a hand-own on
     any fleet box, the row owes a `GolibTests` observer that drives the branch through a seam. -->

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
- **A FLAT hand-own's `.cs.auto` review sibling splits per KEY FOLDER when its emission differs**: one
  `<pkg>/<key>/<name>.cs.auto` per class of identical flavours, at the key the lattice homes the class (a GOOS, a GOARCH or
  a pair folder), back to one flat sibling when they agree (`mergeReviewSiblings`). **The folder need not hold source**: a
  sibling is never compiled, so `crypto/internal/fips140/subtle` keeps `amd64/` and `linux-arm64/` holding
  `xor_generic.cs.auto` alone, and its project file gains no block for them. A single-target run writes its flavour back
  into the key folder that HOLDS the sibling (`heldReviewSiblingPath`); the hand-own move leaves the split alone, and its
  single-copy refusal binds hand-maintained files only. A reader pairing `X.cs` with `X.cs.auto` also looks one folder up
  from ANY key folder of the universe, never from a list of the three GOOS names. The one flavour set that stays flat, and
  NAMED in the merge report, is one whose key folder is a nested package of the same name (`internal/syscall/windows`).
  <!-- 2026-11-10, G claude/g-xor-generic-sibling (the S2 residual, design R1e). Before it the merge split only into
     SOURCE key folders, kept subtle's first flavour flat and named the package, and a single-target linux/arm64
     reconvert wrote its own flavour over the flat file: one changed row on a tree that should read clean. The
     sentence about a package with no per-GOOS source folder, below, describes that earlier rule.
     COORD's S5 ruling (a), 2026-11-05, C1 claude/c1-flatfile-drift. The case:
     runtime/runtime2.cs.auto, whose linux flavour carries `sigmask` in m's [GoValueClone] list and `public partial
     struct g`, and whose darwin flavour differs by one mPadded line (R's end-regen F2); the one windows-flavoured flat
     sibling hid both. A package with no per-GOOS source folder keeps the first flavour flat and is NAMED in the merge
     report. The flat test-side package_info_internal_test.cs files are NOT this class: -tests deletes and re-emits them
     per run for its own target, and the -stdlib merge never writes them. -->
- **L3 work owes a `-p:GoTargetOS=linux` build** — the windows `go2cs-stdlib.slnx` gate compiles no `linux/`/`darwin/` file.
- **darwin COMPILES CLEAN**: its census is a cheap regression guard at any branch tip (`.github/workflows/os-matrix.yml`,
  `goos=darwin stage=census`), not a wall. What darwin lacks is a RUN layer: `docs/phase4/FINDING-darwin-run-layer.md`.
- **A `GoTargetOS` switch poisons `obj/`** — the `<Compile>` item set changes while timestamps don't, so an incremental build
  after a switch validates the OTHER target's assemblies. Purge `bin`/`obj`/`Generated` between switches. **Even WITH a purge
  per flavour, a leftover `bin` tree is the LAST flavour's** — read per-flavour presence from that flavour's own build log
  (`built=N errors=N`), never from what survives on disk.
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
     fold legitimate rather than fitting the answer.

     FLAVOUR BUILD LOG vs LEFTOVER bin, 2026-09-08 (R): a three-flavour ladder purges before each flavour and darwin runs LAST,
     so "internal.syscall.windows.dll PRESENT under bin" was TRUE of darwin — where that package compiles nothing — and FALSE of
     the windows question actually being asked. Caught and corrected before it reached the post. The purge bullet above is what
     MAKES the leftover tree the last flavour's, so the two readings are one mechanism seen from opposite ends. -->
- **L3 EMITS METADATA INTO PER-GOOS FOLDERS BY DESIGN, so flavour metadata rows are KEEP-METADATA, and the
  per-GOOS flavour derivation already exists in `Resolve-Principal`.** <!-- ⚠ 2026-09-13, C2 `ef18baafb` /
     COORD `dd03e6e3f`. The metadata branch simply threw the resolver's answer away — the third instance
     that day of a fix being ordered for code that already handles the case. READ THE SITE BEFORE ORDERING
     A FIX FOR A SYMPTOM. Recorded with it: an arm with no readable input ("present and NOT converted this
     run") is documented AT THE SITE, never coded — the unfalsifiable-guard shape, refused. -->
- **A change that MOVES the corpus layout (where files live, not what they say) runs every reader of the layout before
  it is posted**: the FULL converter suite (not repoguard alone), GolibTests on every flavour, the comment census and
  the behavioral harness. Any reader that enumerates a fixed folder set (`""`, `windows`, `linux`, `darwin`) goes blind
  to a re-homed file, and a blind reader reads GREEN or reads FALLING counts, never red. Grep for such lists first.
  A count that falls under a byte-identical rename is a blind reader until proven otherwise.
  <!-- 2026-11-10, linux/arm64 S2 (G, claude/g-la64-s2 780cb9b34f: 74 linux-arm64/ files, 271/271 renames, 0 bytes
       changed). Before posting, the rehearsal's full suite found 8 converter guards that knew the layout only as
       "own directory, or its GOOS folder", where repoguard alone was green. After posting, i9 read S2 itself as the
       control and found three more readers: GolibTests SkipCountedWalkerFrameTests (DeclaringFiles scans only
       "", windows, linux, darwin; runtime/traceback.cs moved to amd64/, so "no declaration of runtime.gcallers");
       TestCommentParityCensus (21 FELL lines, e.g. body:extra:unexplained 2502 -> 2474); and a syscall linux
       sweep moving 42 + 17 to 50 + 9 under the same renames (mechanism owed by S2). COORD kept the seat out of
       batch 282's first cut until all of them were scored. -->

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

Measuring a converter change's corpus footprint and the reconvert → overlay → build → bucket loop live in the `corpus-reconvert` skill.

## Encoding, metric, and banking a regen

- **Byte-compare any restore or round trip against the original**, and **after any scripted edit diff the file's FIRST and LAST
  lines** — BOM, trailing-newline and CRLF damage lands there, where review attention is weakest.
- A script rewriting a csproj must read AND write explicit UTF-8/no-BOM (`[System.IO.File]::ReadAllText`/`WriteAllText` +
  `UTF8Encoding($false)`); PS 5.1 `Get-Content` reads BOM-less UTF-8 as ANSI and `Out-File utf8` re-encodes the damage. Python's
  `utf-8-sig` breaks BOTH ways: read-sig/write-sig ADDS a BOM, read-sig/write-plain STRIPS one.
- **Measure packages-compiling, not raw error count**: fixing file-inclusion bugs *raises* the count because newly-included files
  surface their own latent defects — progress, not regression. On an UNMASKING LADDER the error total stops measuring progress at
  all: **predict the CLEARED SET and the cut's OWN sites to zero, and report the net total as a READING** of the residue every
  other class contributes. A prediction or hedge pinned to a FILE AND LINE is one layer too low when the blocker cleared is a
  PACKAGE.
- **A compiler names only the implementers it actually COMPILES**, so the sites one build reports are never the population —
  "one site, reported twice" is a reading of that build's reach, not a census.
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
     generated-file changes.

     UNMASKING LADDER — three instances, all 2026-09-08 on the Go 1.24.13 hop, which is why the metric bullet now states the
     prediction discipline and not just the metric. (a) A lane pre-registered "a residue at one named file and line after this
     rung is not a failure of it" and the rung read 6 -> 24 errors, because the blocker cleared was the PACKAGE: ~555 assemblies
     behind it compiled for the first time, surfacing twelve sites in four packages no prior rung could reach. Scored as the lane
     scored it — prediction FAILED, mechanism HELD, hedge one layer too low — while packages-compiling read 194 -> 750 assemblies
     per flavour. A corrected prediction (58/67/73 per flavour, replacing a retired single figure) then hit three for three, where
     the retired figure would have minted a phantom finding. (b) R: a rung predicted 24 -> 20 and measured 40/34/44 per flavour,
     because clearing one package let ~1,100 more assemblies compile and their errors were unreachable before — while the
     sub-prediction about the cut's OWN sites held exactly. Third instance of the count-versus-own-sites split. (c) R: rung 5 read
     24 errors at 750 assemblies and four exact cuts later the ladder read 34 errors at 2,752 — the error total UP 40 percent
     while the compiling corpus more than TRIPLED, every cut's own cleared set exact and every net swamped by what it unblocked.
     A net-total prediction on such a ladder is a bet on how much more corpus compiles. COMPANION to (c), and the origin of the
     implementers bullet: a CS0535 fix reached a SECOND implementer another lane's build never named. -->

## A DELETING instrument over the corpus

- **Derive the candidate population from what the CONVERTER EMITS** — in `go list std` at the SOURCE release and not skip-listed
  — never from a decider answering a meaningless question for a hand-written directory. `go list` says "package not in std at
  target" for `golib` and the Symbols project, TRUE and IRRELEVANT, while the marker-based PROTECTED arm cannot see a directory
  that correctly carries no marker: **the one directory needing no marker is the one the guard does not protect.**
- **Run every refusal BEFORE the deletion loop** so a non-zero exit MEANS nothing was removed, and **REFUSE while UNRESOLVED rows
  stand** (dry run → a human disposes → `-Apply`). Non-zero AFTER deleting is a report wearing a refusal.
- **Predict against the question the instrument actually asks** — a deletion pass and a live-package census are two questions.
- **A missing-member error at a MOVED package's OLD path on a release hop is neither an emission nor a regen defect — and
  regenerating at the old path RE-MINTS a package the new release does not have.** The old-path consumer is a DELETION BY
  SELECTION (the instrument's own rule: a principal `go list` selects at the old release and not at the new). Settle it with a
  table measured against the PINNED SOURCE, never against either route offered.
- **PowerShell `$x[0..($x.Count - 2)]` on a SINGLE-element array yields TWO elements** (`0..-1` counts DOWN to `@(0,-1)`). Guard
  the part count before slicing, and grep the other `src/*.ps1` instruments for the idiom.

<!-- All from the 2026-09-07 read of reconvert-deletions.ps1, found by a lane on the first real dry run and NOT applied. go list
     refused golib (116 files) and the Symbols shared project; the landed script exited 2 AFTER deleting. Prediction numbers: the
     deletion-pass question (whole removed packages plus non-Go directories) = 205, the live-package census = 25. The slicing
     idiom turned core/GlobalUsings.cs into the import path GlobalUsings.cs/GlobalUsings.cs and minted a phantom DELETE-ABSENT
     row at ed191e9a8.

     MOVED PACKAGE, 2026-09-08: six CS0117 on a 1.24 ladder rung were neither a converter emission defect nor a regen defect —
     the package had MOVED under a new parent and the corpus already carried the new release's names, so the consumer left at the
     old path is this instrument's own deletion-by-selection class. BOTH routes the coordinator offered were wrong; a table
     measured against the pinned source refuted them. Beside it, a CONSTRUCT new at the release — a nil comparison on a
     slice-typed type parameter, rendered `== default!`, CS8761 — is a converter cut with a ZERO footprint at the old release and
     the ladder as its acceptance. -->
- **A SINGLE-FLAVOUR INVOCATION CANNOT KNOW A DIRECTORY IS DEAD** — remove the ENUMERATED residue files,
  then the directory only if it is now empty, and otherwise KEEP it and NAME what remains. <!-- ⚠
     2026-09-13, C2 `0773595` s3. The H5c instrument runs one `-Goos` per call, so "the package is absent
     at the target" is known for THAT flavour only; loud, never destructive. A leftover surfaces again at
     the `git rm` step's `cmp` as a divergence. The artifact a later step compares against is emitted on
     DRY runs too, LF-joined and ordinal-sorted so it is `cmp`-comparable across boxes. -->
- **A CLASS CHECK THAT RUNS BEFORE THE ABSENCE CHECK MISFILES A REMOVED PACKAGE'S METADATA AS A HUMAN'S
  PROBLEM.** <!-- ⚠ 2026-09-13, i9 `8f2eafdc8` s3: 15 of the 42 UNRESOLVED rows were `package_info.cs` in
     packages removed between 1.23.12 and 1.24.13, which H5c's own DELETE-ABSENT rule justifies — but the
     metadata branch ran first. Ordering fix ruled. Recorded beside it, the third instance that day of one
     population with three answers: hand-own counts of 226 (`grep -rl`), 23 (a structural guess) and 146
     (the instrument's own regex) — only the instrument's is THE population. -->
- **A POST-CONDITION MUST MODEL EVERY ROUTE A FILE LEAVES BY, AND PRINT THE TERM IT SUBTRACTS.** <!-- ⚠
     2026-09-13, i9 `846cbd849` / COORD `cee96ffad`. H5c's arithmetic expected `total - classified` while
     the ruled residue sweep ALSO removes test and host `.cs` beside DELETE-ABSENT rows
     (102 + 37 = 139 = 4039 - 3900); the instrument re-walked the disk, disagreed with its own bookkeeping
     and exited 3 rather than reporting success. That refusal was of a FALSE EXPECTATION, not of a wrong
     deletion — which is the distinction worth reading before "fixing" it. -->
- **THE RECONVERT RITUAL SEEDS WHAT A CONVERSION NEEDS; A BUILD ADDITIONALLY NEEDS `src/gen` AND
  `src/Directory.Build.props`.** <!-- ⚠ 2026-09-13, i9 `0687402db` s2. Without the generator project every
     generated half is missing and 62 CS8795/CS1739/CS0029 errors landed in `internal/runtime/atomic` and
     `internal/goarch`, UPSTREAM of every site the reproduction had set out to reach. The tell was
     LOCATION, not count: a reproduction that fails somewhere other than the sites it set out to reproduce
     has not reproduced. -->

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
