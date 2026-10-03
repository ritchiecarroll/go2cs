# Migrating the go2cs corpus to a new Go version
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

> The standing runbook for moving the converted standard library — and everything derived from it:
> the goldens, the validation roster, the proof pages, the disclosure manifests, the published
> packages — from one Go release to the next. It is **version-agnostic by design**: it names
> instruments, gates and traps, never a particular release.
>
> **This runbook leads.** It is the living procedure for a corpus migration: the canonical
> H0–H12 (+H4a, H7a) step inventory is the one maintained **here**, amended in-stage as lessons are
> learned — the discipline its first execution already practiced, ratified as the era rule
> (board, 2026-08-24: runbooks are *executed as written, deviations fixing the runbook in the stage
> that finds them*). The **strategy** lives in
> [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) — which releases, in which order, under which
> ruled frame — and every "(ruled)" below points at a ruling recorded there (§8) or in an instance
> plan. **A runbook edit never reopens a ruling**: a change that would contradict one requires a new
> ruling first, recorded where the old one lives. Where this document and any plan disagree about
> *procedure*, the plan is stale — fix this document if it is wrong, and mark the plan superseded in
> the same change.
>
> Companion: [`DotNetMigration.md`](DotNetMigration.md), the same for a new **.NET** release. The two
> are separate documents because they are separate hops — **one variable at a time** is the rule that
> makes either measurable.

**No frozen figures.** Roster rows, verdict totals, package counts, marker-census counts and wall
times are **named by instrument and re-measured at the migration**. Two classes are re-measured *by
standing rule and never carried at all*: the hand-own marker census, and the per-release standard
library delta. Where a budget matters, this document names the row in CLAUDE.md's measured budget
table rather than copying a number that goes stale.

---

## 1. Shape of a corpus migration

A corpus migration moves **the Go release the corpus is converted from**. It moves nothing about the
.NET runtime, and it changes the converter only where the new release's *language* requires it.

Two properties determine almost everything about how expensive a given migration is:

| Property | Cheap end | Expensive end |
|:--|:--|:--|
| **Language delta** | none — a patch-level move within one minor | new syntax or new type-system surface, which is converter work with its own design |
| **Package delta** | none | packages added, removed, promoted, or reorganized wholesale |

A **patch-level migration within one minor** is the cheap end on both axes and is the right rehearsal
for the machinery: the pin guard fires for real, the hand-own differential runs for the first time,
the badges churn, the release ritual is exercised — all without a language change to confound them.

### 1.1 What moves emitted C# even when the Go source did not

Three channels, and knowing which are live for a given migration is the difference between reading a
diff and drowning in one:

1. **Release-tag expansion.** The converter derives the `go1.1 … go1.N` build-tag set from the Go
   version and evaluates build constraints with it, so a **minor** bump flips every `//go:build go1.N`
   guard in the Go tree and changes which files each package includes. **The derivation is
   minor-keyed** (`releaseTagsForVersion`, `src/go2cs/directiveOperations.go`, trims any patch
   suffix), so a **patch-level migration has zero release-tag delta** — verify this against the
   source for the migration at hand rather than assuming either way.
2. **Imported type aliases.** An emitted project reads each imported package's `package_info.cs` to
   mint its `<ImportedTypeAliases>` block, so a moved dependency moves its dependents' emission —
   including the behavioral goldens', whose own Go sources never change.
3. **Upstream source.** The ordinary channel, and the one the migration is *for*.

### 1.2 The toolchain rule, and why it is step one

The converter type-checks from source using the `go/ast` + `go/parser` + `go/types` **compiled into
`go2cs.exe`**. Therefore:

> **To convert Go 1.N sources, `go2cs.exe` must be BUILT with a Go toolchain ≥ 1.N.**

This is why the toolchain move is the first step of every migration and not a housekeeping item.

**And it opens a false-green route the harnesses do not close.** Every rebuild predicate rebuilds the
converter when a converter **`*.go` file** is newer than the binary. Installing a new Go toolchain
touches none of them, so every predicate still says "up to date" and every gate keeps running a
binary whose front end is the **old** release's, against the **new** release's sources. It does not
fail cleanly — the old parser mis-parses or rejects new constructs and the run degrades into the
converter's best-effort *"did not fully type-check"* path, which `check-no-regression.ps1` reports as
`NOT MEASURED` (good) and the runners do not.

**The hole is closed, and the closure is structural rather than remembered.** Every Go binary
already embeds the release that built it, so nothing needed stamping: the three rebuild predicates
delegate to ONE shared helper (`src/tests/ConverterBuildInputs.cs`) that reads the binary's embedded
release back, compares it against the live `go env GOVERSION`, and fails **stale-wards** — an
unreadable stamp or an unanswerable `GOVERSION` forces the rebuild rather than excusing it. **A
toolchain hop invalidates `go2cs.exe` by itself; no explicit `go build` is owed, and no gate runs
against a stale converter.** The same helper derives its input set from the converter's own
`//go:embed` directives, so an embedded-asset edit — a csproj template, the `package_info.cs`
skeleton, a publish profile — invalidates the binary too; that sibling route is why the compare
landed in one place rather than three. Full statement:
[`DotNetMigration.md`](DotNetMigration.md) §5.2. Closure record (⟨OQ-6⟩, landed 2026-08-24):
[`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) §1.4.2.

⚠ **A stamp cannot close the CONFIGURATION form of the same shape** — a toolchain pin that silently
substitutes another release at conversion time, while the stamp truthfully names the toolchain that
built the exe. That one is H1 step 1's, and it is verified by running, never by reading.

---

## 2. The step ladder

**This section is the canonical H0–H12 (+H4a, H7a) inventory and its procedure.** It was generalized from
[`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) §2, which now points here; the ⟨OQ-n⟩ rulings
behind each "(ruled)" remain recorded in that plan's §8. **Steps marked GATE are pass/fail and block
the next; steps marked ⟲ are re-measured at every migration and never carried forward.**

**Four orderings are not negotiable**, and each has a mechanical reason rather than a preference (the fourth is the
AMENDED 2026-09-07 block below: the outgoing corpus's final release ships ahead of the H1↔H2 pair).
*(Edited in place 2026-09-26, §7 fix A1: this line read "Three" after the fourth ordering was added below it.)*

- **The toolchain step and the pin bump land as ONE reviewable pair.** Between them the binary claims
  the new release (its embedded runtime version is what the NuGet compatibility guard reads) while
  `version.props` still names the old one — so a NuGet-referencing conversion in that window refuses
  legitimate old-pin modules and accepts new-pin ones for a corpus that does not exist yet. Silent,
  and it only bites a user.
- **The pin bump precedes the reconvert.** `checkCorpusToolchainPin` refuses `-stdlib` and `-tests`
  otherwise — and the guard's own error text prescribes the remedy verbatim: *"if the corpus is
  deliberately moving to X, bump `<GoStdLibVersion>` to X first."* The ordering is sanctioned by the
  code, not invented.
- **The baseline capture precedes replacing the old Go tree.** With side-by-side installs — which
  every migration should use — this relaxes to "precedes the reconvert".

Everything else may be reordered by the executing lane.

> **AMENDED 2026-09-07 — a FOURTH ordering, and it runs ahead of the H1↔H2 pair: the OUTGOING
> corpus's final NuGet release ships FIRST.** The mechanism is one line of the pin instrument.
> [`src/migrate-gorelease.ps1`](../src/migrate-gorelease.ps1) **resets `<GoBuildNumber>` to 0** at the
> pin stage — line 895, inside the `src/version.props` arm that `-KeepBuildNumber` guards — enacting
> H2's own ruling that the build number resets per release. So the moment H2 lands, the outgoing
> release has no build counter left to advance: the next publish is `<new-release>.1`, a version of a
> corpus the outgoing record never measured, and **the outgoing corpus can never again be published at
> its own release**. Whatever it had shipped when H2 landed is what it shipped, permanently.
>
> **The step:** ahead of H1/H2, publish the outgoing corpus's final release with
> [`src/release-nuget.ps1`](../src/release-nuget.ps1), running H12's release ritual in full — the
> announcement text on the branch, the pre-pack signed tag, the write-once proof snapshot, both badge
> retargets, the recomputed re-verification pass. **That release is what freezes the outgoing roster,
> its proof pages and every package README** at the record they reached; it is the outgoing corpus's
> anchor, and it is the last moment one can be minted. It sits ahead of H1 rather than merely ahead of
> H2 because the first ordering above already binds H1 and H2 into one reviewable pair.
>
> H11 and H12 below are written for the **incoming** corpus and are unchanged; this step is their
> outgoing-side twin, and the two are the same ritual pointed at the two ends of the hop.
> `-KeepBuildNumber` is not a substitute: it holds a counter across the boundary that counter is
> defined to reset at, which is a different and unruled thing.
>
> **Worked instance — the 1.23 → 1.24 hop.** It proceeds with the outgoing record **closed at its
> anchor** rather than driven to 100% (owner ruling, 2026-09-07; the reasoning is recorded in
> [`ValidatedTestPackages.md`](ValidatedTestPackages.md)'s *Excluded packages* block and in
> [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) §1.3's dated amendment). The rows still unbanked
> at the anchor are neither carried forward nor owed anything special: **H10 re-derives every row from
> scratch at the new release regardless**, so they re-bank there on exactly the footing of the rows
> that did bank.

### Readiness — before H0 **GATE**

*(Edited in place 2026-09-26, §7 lessons 1-3: added from the 1.24.13 hop, where each item below surfaced mid-campaign
as a stalled lane, a
re-route or an owner ruling.)*

A per-box record, taken and read before H0:

- **Host.** DNS on public IPv4 resolvers per adapter (IPv6 unbound where there is no v6 path), qualified by Go's own
  `go test -count=1 -timeout 40m net` (`TestLookupCNAME` is the tolerated drift); WSL `localhost` resolving `::1`;
  unelevated symlinks (Developer Mode) and `LongPathsEnabled=1`; pwsh 7 and the pinned .NET SDK reachable by every
  launcher; a many-core Windows host, local or cloud, for crypto/tls's standard BoGo wall, qualified by measurement:
  Go's own `TestBogoSuite` green twice under `-count=1`, then one converted reading inside the standard 600 s wall
  (only `GOFLAGS` reaches BoGo's nested `go test`, so a raised wall is an owner ruling); a temporary host built from a
  kit committed on `claude/coord-handover` and named by its fleet nickname; any cloud quota an OWNER-HAND filed now.
  net's qualification is re-run twice immediately before net's H10 row, on the box that reads it (a router's DNS relay
  turns NXDOMAIN into SERVFAIL whatever its upstream, and nslookup asks only the first server), and what answers only
  over IPv6 is listed before IPv6 is unbound. The record names the filesystem that holds the repository, TEMP and
  GOROOT. GOROOT stays on NTFS, and a box whose worktree is on ReFS does not read os or testing. Linux arms run
  Microsoft's .NET build, with its libcoreclr hash recorded.
  *(Edited in place 2026-10-01, §7 lessons 40, 42 and 44-46: the BoGo host is sized by measurement and a temporary
  host is built from a kit; net's re-qualification, the filesystem record and the .NET build were added. Each was
  first met mid-campaign.)*
- **Instruments.** Every instrument a later stage depends on is landed on master and red-proved BEFORE that stage —
  the H10 recon wrapper as one blob, the sweep's `-Hop` mode (under PowerShell 5.1 and pwsh 7), the H6 completeness
  gate, the H11 existence-plus-monotonicity check reading local AND origin tags. Each derives its per-release input
  from `src/version.props` (or GOROOT's `VERSION`) and refuses by name when that release's file is missing,
  red-proved with `version.props` bumped in a scratch worktree (`docs/phase4/NOTES-next-hop.md`, "NOTES" from here
  on, A7-A10). The sweep's `-Hop` rows carry each row's execution pin (NOTES A3; open at 172d437e66, where
  `src/_roster.ps1:578` sets `Execution = $null`). Plain `go test ./... -count=1` in `src/go2cs`, TestContextBudget
  included, is green at master.
  *(Edited in place 2026-10-01, §7 lessons 43, 49 and 68: hard-coded instruments silently read the old release,
  `-Hop` builds its rows without their execution pins, and an over-budget instruction file kept every sub-agent from
  spawning.)*
- **Fleet.** Addressed comms from day one (ledger, per-lane inboxes, one message per event), one OWNER-HAND line per
  owner action, and resume prompts written on the rule that nothing local survives a restart. Messages take the
  WHAT / EVIDENCE / ASK-or-NEXT shape. Every battery brief and per-run script is committed on
  `claude/coord-handover` before launch. `RESUME-SESSIONS.md` and its verifier exist.
  *(Edited in place 2026-10-01, §7 lessons 78, 82 and 85: briefs derived before launch caught their defects there,
  and a COORD lost to the weekly limit resumed from the branch.)*
- **Seats.** Every seat the ledger accepted since the previous anchor is an ancestor of origin/master, or carries a
  disposition by name (land / re-cut / drop). The census repeats before each train draft and before the close STAMP.
  *(Edited in place 2026-10-01, §7 lesson 41: this bullet was added. TRAIN 49 never landed, and its items were
  missing until they were re-cut.)*

### H0 — Baseline capture ⟲

Capture, on the **outgoing** toolchain and the **new** converter build, everything the migration will
diff against: the hand-own `.cs.auto` baseline, the package census, the roster snapshot, the
disclosure manifests — and three comparands later stages otherwise reconstruct mid-hop: the outgoing
three-target `-platform-census` manifest (H8's comparand, produced under the outgoing pin into a fresh
directory), the behavioral suite's failing set **by name** (H9's base), and each box's bare `go`
(`GOTOOLCHAIN=local go version` from a directory with no `go.mod`).
*(Edited in place 2026-09-26, §7 lesson 6: the last three were added; H8 and H9 had to produce the first two
mid-hop.)*

Before H2's pin bump, each UNBANKED row's reading is committed as matched / disclosed / undisclosed (the denominator is
their sum), naming each undisclosed test and its disposition. A disclosure disputed at that reading is marked CONTESTED.
*(Edited in place 2026-10-01, §7 lesson 48: reflect circulated as 326 of 385 against a record reading 388.)*

⚠ **Generate the `.cs.auto` baseline fresh, from a seeded old-release regen — never from the committed
siblings.** The overlay rule excludes `*.cs.auto` in order to protect the hand-owned `.cs` beside it,
so the tracked siblings are **frozen on their own schedule** and a materially stale baseline poisons
the differential. This is a ruled decision, not a preference.

### H1 — Toolchain provisioning **GATE**

**Default order** (a COORD proposal, NOTES §5 Q13, unruled at 172d437e66; TRAIN K seated it this way): any side seat
that moves a toolchain-adjacent pin (the .NET SDK `global.json`, the emitted `LangVersion`, a C# keyword escape) lands
and regenerates before the outgoing corpus's final release, never inside the hop.
*(Edited in place 2026-10-01, §7 lesson 47: inside a hop, H5 would measure the csproj files such a pin moves instead
of inheriting them. Once Q13 is ruled, cite the ruling here or move the rule into §2's orderings.)*

1. Install the target release **side-by-side**; confirm the target actually **executes** — run
   `<target-root>/bin/go version` and require its OUTPUT to name the exact target. **Reading
   `GOROOT/VERSION` is not a verification** (measured 2026-08-24, hop-A provisioning). Go 1.21+
   toolchain switching obeys a `GOTOOLCHAIN` pin (`go env GOTOOLCHAIN`, persisted in the user's
   `go/env`) **ahead of whichever binary is invoked**, and the redirect is **silent**: the `VERSION`
   file, the target's own `bin/go`, and even the official download shim can disagree, and only the
   ones that *run* tell the truth. A leg that trusted the file would emit the whole corpus with the
   OLD toolchain while believing otherwise — §1.2's false-green shape arriving through
   **configuration**, which no binary stamp can catch. Check `go env GOTOOLCHAIN` explicitly; a pin
   naming another release must be resolved, or overridden per-invocation (`GOTOOLCHAIN=<target>`,
   or `GOTOOLCHAIN=local` with the target's `GOROOT`), before any step below runs. Prefer the
   per-invocation override to editing the pin: the pin is a machine default, outside the standing
   install grant, and while the hop is in flight it is *protective* — it keeps every other process
   on the box on the outgoing release until the migration deliberately moves.
   ⚠ **`GOTOOLCHAIN` is only HALF the override on a box that also pins `GOROOT`** (measured
   2026-08-25, H2's smoke gate, first execution). A user-level `GOROOT` environment variable names
   the TREE, and the two answers diverge silently: under `GOTOOLCHAIN=<target>`, `go env GOROOT`
   reports the *selected toolchain's* root while the process environment still carries the pinned
   one — and `-stdlib` converts the tree the ENVIRONMENT names. The leg would have emitted the OLD
   release's sources into a corpus whose every gate then measures against NEW-release goldens, each
   side internally consistent — except the converter's own pin-vs-tree guard refused, and its
   message named the mechanism. A hop leg's environment therefore sets **both** —
   `GOTOOLCHAIN=<target>` and `GOROOT=<target-root>` — per-invocation, both pins left in place.
   Provisioning records which pins a box carries; the fleet has held every combination.
   ⚠ **Fleet boxes are configured oppositely and neither lane's experience predicts the other's**:
   a *pinned* box switches DOWN, silently ignoring a newly installed release; an *`auto`* box
   switches UP, silently downloading one a `go.mod` asks for. Both make "the SDK is installed"
   insufficient as provisioning evidence, in opposite directions. Worked instance with the resolved
   per-box values: [`phase4/STAGE0-provisioning.md`](phase4/STAGE0-provisioning.md), its hop-A
   section.
   ⚠ **An `auto`-fetched toolchain is READ-ONLY, and the attribute travels** (measured 2026-08-25,
   the i9's reserved shard). `GOTOOLCHAIN=auto` downloads into the per-user module cache, where Go
   marks **every file read-only by design** — a manually provisioned side-by-side SDK
   (`~/sdk/<release>`) is not. Any harness that COPIES fixtures out of that tree carries the
   attribute along (`.NET`'s `File.Copy` propagates `ReadOnly` with the content), and the first
   write onto a copy throws `UnauthorizedAccessException` — which presents as a mass
   `Go="pass" C#=""` file-lock signature, and the stale partial copy it leaves behind then presents
   as an unrelated `CS0234` on retry, convincingly mimicking other catalogued traps. **The fix is
   one attribute strip, in place, once**: clear `IsReadOnly` recursively on the cached toolchain
   directory — no copy, no relocation. A box-configuration trap, not a harness bug: an
   `auto`-configured box meets it identically every hop, a manually-provisioned one never does.
2. Move the converter module's `go` directive to the target (ruled: it moves each migration).
3. Bump the `golang.org/x/tools` and `golang.org/x/mod` requirements to releases contemporary with
   the target. The export-data policy bounds how far they may lag. **This is a separate commit with
   its own CNR** (ruled) — a dependency bump that can move emitted bytes must be visible on its own.
4. `go build` the converter **on the new toolchain**; converter `go test ./...` green.
5. **Verify the stale-binary guard held** (§1.2 — closed 2026-08-24; the harnesses compare the
   binary's embedded release against the live toolchain) before any harness runs. This is a check,
   not a task: the guard fires on its own, and the step exists only to confirm the rebuild it forces
   actually happened.

6. **Every go-invoking instrument asserts its toolchain three ways and prints them** — the PATH-resolved
   `go version` output, the resolved binary, GOROOT's `VERSION` — from a directory with no module, in its own
   run environment. `-goroot` does not steer the package loader (the converter now refuses a mismatch), and
   `GO111MODULE=off` cancels a `GOTOOLCHAIN` redirect.
7. **`go vet` under the new directive is step 2's own bill**: the directive alone enables the new release's vet
   checks, and their fixes block step 4.
8. **Read the release's GODEBUG default changes against the test host** (1.24 dropped `winsymlink=0`, refusing
   junction-staged hosts).
*(Edited in place 2026-09-26, §7 lessons 7-9: steps 6-8 added.)*

**Gate:** converter unit tests green **and** `go2cs.exe` demonstrably built by the new toolchain.

### H2 — The pin bump **GATE**

Bump `<GoStdLibVersion>` in `src/version.props` to the exact target release, and settle the build
number's policy at the same moment (ruled: it **resets** per release). **Nothing else changes in this
commit beyond what the instrument itself edits** — the pin, the build-number reset, H1.2's `go`
directive, and the prose that states the release as present-tense fact. Deliberately a small,
reviewable, revertible move, landing as one pair with H1.

**The instrument is [`src/migrate-gorelease.ps1`](../src/migrate-gorelease.ps1)** (`.bat` launcher
beside it). A bare run is a **census**: it classifies every place in the tree that spells or derives
the Go release into five classes — source-of-truth, doc-statement, derived-by-regen,
derived-at-runtime, must-not-change — and changes nothing. With `-To <release> -Apply` it performs
exactly two of those classes: the pin itself (`<GoStdLibVersion>`, the build-number reset, and H1.2's
`go` directive in `src/go2cs/go.mod`, which `-SkipGoMod` leaves alone) and the prose that states the
release as present-tense fact, each by a **named anchor** whose match count is asserted rather than
substituted blindly. It supports `-WhatIf`, refuses to run when the working tree is dirty in the
files it would touch, and re-reads its own output afterwards to prove zero sites remain — so it is
idempotent, and re-running it is the verification. Its discovery sweep reports anything it cannot
classify as **UNCLASSIFIED** rather than guessing, which is how a newly-introduced site announces
itself at the next migration instead of being missed.

**What it does not do, and will not pretend to:** it does not reconvert (it *prints* the seeded
reconvert and the layout-L3 multi-target emission for H5/H8), it runs no gate, it does not touch the
roster's rows or arithmetic, and it makes none of the migration's judgements — H3's package census,
H6's hand-own differential, §4's golden-drift triage and H10's per-row re-derivation are all
readings a person makes. It also leaves the **converter tool** version alone: that is
`set-version.ps1`'s Windows PE resource and is independent of `version.props`.

**Gate:** a single-package `-stdlib` smoke conversion no longer refuses.

> **AMENDED 2026-09-20 — the build-number reset is now a GUARD ARM, not a step to remember.**
> `TestPublishedCounterMatchesTheRecordedReleases` in `src/go2cs/internal/repoguard` holds it under
> the plain `go test ./...` every lane already runs: **no release recorded on the current base → the
> counter reads 0; releases recorded → the counter names the latest of them, exactly.** It reads
> `src/version.props` and the write-once snapshot directories under `docs/validation/` (H11.5), so it
> needs no git, no tag list and no feed query, and it holds on the shallow clones the lanes work from.
>
> **Why it exists:** the reset was ruled here and enacted by `migrate-gorelease.ps1`, and it was still
> MISSED at `claude/version-go1.24.13` `0f97dcc8db` — the base went 1.23.12 → 1.24.13 with the counter
> carried at 3. Nothing went red: version.props parses, the converter emits, the corpus builds. The
> measured blast radius was **335 package READMEs linking a `nuget-1.24.13.3` tag with no object behind
> it and 191 linking a `docs/validation/1.24.13.3/` directory that was never written** — a defect whose
> only symptom is a reader clicking a badge, which no gate in this repository clicks. A rule enacted by
> an instrument the executing lane may or may not run is a rule that lives in attention.
>
> ⚠ **Resetting the counter is necessary and NOT sufficient.** With the counter at 0 the composed stamp
> is `<new-base>.0`, equally unpublished; see H12's amendment of the same date for what the badges do
> instead.

#### Ruling 2026-09-08 — the H2→H5 window: the converter at go1.24.13, the corpus still at 1.23.12

H2 bumps the **corpus** pin; H1 step 2 has already moved the **converter module's** `go` directive, so the
tree carries two releases until H5's regen closes the window. At master `f4d2b981b` (train 43)
`src/go2cs/go.mod` requires **go1.24.13** while `src/version.props`'s `<GoStdLibVersion>` still reads
**1.23.12** — the converter's build toolchain hopped, the corpus release did not. ⚠ **H1 does not rule this,
and the runbook did not state it before now** — H1 is a five-step list whose steps 2 and 3 rule the pin bump
alone — but its step 1 warning block supplies the mechanism: a hop leg sets **both** `GOTOOLCHAIN` and
`GOROOT` per invocation, and **`-stdlib` converts the tree the ENVIRONMENT names**, which is why the pipeline
under a 1.23.12 `GOROOT` converts 1.23.12 sources. Ruled `014bfe84f`, corrected `84b5913098`, measured
`e96349c54`.

**Both arms, measured at `f4d2b981b`:** building `src/go2cs` under the 1.23.12 pin exits **1** with no binary
(`go: go.mod requires go >= 1.24.13 (running go 1.23.12; GOTOOLCHAIN=local)`); the **same** build under the
1.24.13 pin exits **0**. Arm 2 is what makes the refusal a toolchain fact rather than a broken build.

**The converter suite runs under the 1.24.13 pin** — `GOROOT=<sdk>/go1.24.13`, its `bin` first on PATH,
`GOTOOLCHAIN=local`, bare `go version` **asserted** rather than merely printed. **The behavioral suite and CNR run
under the two-pin PAIRING — the fifth arm below — since 2026-09-08**; the sentence that stood here until then ran
them under the 1.24.13 pin, which reads the mixed-state artifact the fourth arm names.

⚠ **Two staleness guards, and the INVOCATION decides which applies.** `go2cs -tests` invoked DIRECTLY meets
only the converter's own mtime guard (`converterStaleness.go`), so a freshly built 1.24.13 binary passes under
either shell. Anything HARNESS-driven — `BehavioralRunner`, MSTest, `PerformanceRunner`, the sweep's own build
step — meets `ConverterBuildInputs.IsConverterStale`, comparing the binary's embedded release against the live
`GOVERSION`: a 1.24.13 binary reads STALE in a 1.23.12 shell, the harness rebuilds, and that rebuild REFUSES.

**`-tests` rows and `run-validated-sweep.ps1` against the still-1.23.12 corpus, in order:**

```
1  build      src/go2cs under the 1.24.13 pin              exit 0, binary produced
2  run rows   under the 1.23.12 pin, -SkipBuild MANDATORY  guard passes: 1.23.12 == version.props
3  assert     the comparison record's oracleGoVersion reads go1.23.12
```

⚠ **Step 2's "under the 1.23.12 pin" is the ENVIRONMENT, not the flag.** `-goroot` selects the corpus SOURCE
tree and does **not** isolate the converter's package loader: the ambient `GOROOT` leaks into `go/packages`'
resolution of `internal/abi`, so a `-tests` run issued from the shell that BUILT the converter (1.24.13 still
exported) fails the `runtime` row with ~150 errors shaped like `undefined: abi.MapBucketCount` and `use of
internal package internal/abi not allowed` — **a wall that impersonates a corpus break at exactly the moment a
pin moved.** Measured one-variable by i9 (`1cf3af363`): identical command line, `-goroot <sdk>/go1.23.12` in
both runs; ambient `GOROOT` 1.24.13 → rc 1, nothing emitted; ambient 1.23.12 → rc 0, clean. G (`072c283023`)
places it as H1 step 1's ruled mechanism — **`-stdlib` converts the tree the ENVIRONMENT names** — reaching the
`-tests` driver, plus the half H1 does not say: **the flag does not override the environment.** So the converter
build and the corpus run happen in SEPARATE shells, or the run re-exports `GOROOT` and `PATH` to the corpus pin
before invoking `go2cs`; **`-goroot` alone is not the pin.**

⚠ **`-SkipBuild` is mandatory, not stylistic.** The sweep's toolchain guard does not refuse the mixed state, it
**requires** it — throwing when the running release differs from `version.props`, so it passes under 1.23.12
and throws under 1.24.13 — and none of its four switches touches the pin. But its line ~334 is
`if (-not $SkipBuild) { … }`, so a bare sweep builds the converter under the 1.23.12 pin and dies at the
refusal above before a row starts.

⚠ **A pre-hop-pinned instrument is UNBUILDABLE from master in this window**, so a reading taken with one is
tree-locked to the pre-train-43 checkout it was built from — R's 6 VALID / 12 HOOK-ONLY / 10 GENUINELY STALE
base classification (`daa57a1f9`) is one. Re-measuring from master necessarily uses a 1.24.13-built front end:
**a different instrument, named with its pin on both sides, never a refutation.**

**Train batteries take the same split per LEG.** Train 43's assembly pinned each leg
(`coord-train43-assemble.sh`), with a negative control that a module declaring `go 1.24.13` must REFUSE under
the 1.23.12 pin; a cost-canary or sweep leg here takes the two-pin shape above, or is stated **UNMEASURED**.

⚠ **Fourth arm (2026-09-08, i9 `0858372b5`, three arms with the converter binary held byte-identical): under the 1.24.13 run pin the behavioral CNR and the behavioral suite read a MIXED-STATE emission on runtime-importing projects.** The `Δ` on the `runtime` PACKAGE alias is decided from the loaded closure — i.e. from the run `GOROOT` — so the same binary emits `using Δruntime = runtime_package;` under a 1.23.12 environment and the bare `using runtime = runtime_package;` under 1.24.13; the bare form does not compile against the 1.23.12 corpus (CS0576, namespace `go` holds a `runtime` definition). Exactly EIGHT behavioral goldens read CHANGED under the 1.24.13 pin (35 lines, every one the Δ-drop, nothing else), and those eight fail Target AND Compile there — a **named EXPECTED SET** for any battery leg run under that pin, any other member a finding. Under the two-pin pairing (converter built at 1.24.13, environment re-exported to 1.23.12) all eight are byte-identical to the goldens committed at `f4d2b981b`. **Consequences ruled:** goldens are re-baselined at H5 with the hopped corpus in the reference graph, never in the window — H9 as planned for the window is DEFERRED to H5 (a golden re-baselined under a run GOROOT that differs from the corpus's release records the emission for a corpus that does not exist, and Target green + Compile red is its tell); the two-pin pairing is the zero-drift instrument of record for a converter change in the window; and the closure predicate behind the stamp is named (measured on the i7 the same night, five arms, one binary): `computeImportAliasRenames` in `src/go2cs/importAliasOperations.go` records every non-final import-path prefix of the go/packages closure as a child namespace and renames a direct import whose name collides — so it reads the Go closure at the LOADER's release while the collision is decided by the C# namespace set of the corpus's transitive reference closure at the CORPUS's release; the two agree iff the releases agree. The eight collide on the ANCESTOR of `namespace go.runtime.@internal;` (declared by `runtime/internal/{math,sys}`, which the 1.23 `core/runtime.csproj` references and which Go 1.24 moved to `internal/runtime/*`). **Ruled fix (G's cut, rides the train after 44):** the child-namespace set is the UNION of the Go closure and the referenced corpus's transitive csproj closure under `-go2cspath` (exact, never directory existence), falling back to the Go closure where no corpus csproj exists; a decision-level guard over synthetic closures plus a fixture corpus tree; acceptance is CNR under the 1.24.13 pin reading 0 CHANGED on the 1.23.12 corpus. At H5 the eight drop the Δ legitimately (none imports a `runtime/<sub>` package) and re-baseline there.

⚠ **Fifth arm (2026-09-08, measured on two hosts): CNR and the behavioral suite run under the two-pin PAIRING — environment re-exported to 1.23.12 (`GOROOT`, its `bin` first on PATH), `GOTOOLCHAIN` left at auto — with ZERO drift as the expectation, and the eight-member artifact set is retired from batteries.** The module graph enforces the pairing: `src/go2cs/go.mod` says `go 1.24.13` while every corpus module says `go 1.23`, so Go's toolchain rule switches ONLY the converter's build up and every behavioral or corpus package loads at 1.23.12. CNR under that shell read 722 byte-identical / 0 NOT MEASURED with the converter still go1.24.13 afterwards (i9 `ef05467a3`); the behavioral runner read all eight artifact projects green in all four phases, `Δruntime` present in every emission, goldens byte-matched, tree clean (the i7). Five mechanics ride with it. (1) The runner's staleness predicate reads `go env GOVERSION` at the RUNNER's cwd (`src/tests/Behavioral`, no module: 1.23.12) against the binary's embedded 1.24.13, so it REBUILDS the converter on every invocation — ~1.8 s, the content-addressed cache re-links only — which fails safe (never a stale binary, never a Transpile skip); CNR is immune by its unconditional `go build`. (2) The pin is ASSERTED from a directory with NO `go.mod`: inside `src/go2cs` both `go version` and `go env GOROOT` report the SWITCHED toolchain under auto while `command -v go` still resolves under the pinned root (i9 `0eef5b66c`), and the produced binary is verified with `go version <exe>`, which no cwd can switch. (3) `GOTOOLCHAIN=local` BREAKS the single-root pairing outright (the converter cannot be built under the run pin); a SPLIT pin naming `GOROOT_BUILD=<sdk>/go1.24.13` on each build and `GOROOT_CONVERT=<sdk>/go1.23.12` on each conversion needs no switch and works under either setting (G `8f73ed9a6`) — the right spelling for a hand-invoked two-arm instrument. (4) The conversion half still reaches `go` one process down — the converter's package loader shells out to it — so a module declaring ABOVE the convert pin switches SILENTLY under auto (a probe declaring `go 1.24.13` loaded the 1.24.13 stdlib with `GOROOT` naming 1.23.12; under `local` it refused verbatim; i9 `28b5ba6b4`): the corpus is held at 1.23.12 by its own `go 1.23` directives, not by the naming of roots, and an end-user `-recurse` module or a hopped corpus against a stale pin is exactly the case that moves. (5) The switch resolves through the MODULE CACHE (`golang.org/toolchain@v0.0.1-go1.24.13`), so a cold or offline box fetches at that point. The 1.24.13-pinned CNR stays as the alias defect's own instrument — the fix's acceptance is that reading dropping from eight to zero — never as a battery leg.

### H3 — Package census ⟲

Diff the conversion queue's package set against the outgoing corpus: **added**, **removed**,
**renamed or promoted**, and **experiment-gated and therefore deliberately absent**. The last category
matters as much as the others: experiment-gated packages stay out until they graduate to the default
package set (ruled), and naming them explicitly is what stops a later reader re-diagnosing a
"missing package".

Deliverable: a census document under `docs/phase4/`, in the shape of the existing census docs, plus the incoming
release's DATA files, `docs/phase4/data/population-go<rel>.txt`
(`go run ./internal/genpopulation -goroot <incoming GOROOT>`, agreeing by name with an independent enumeration; a
pre-staged pair is regenerated byte-identical) and `relocations-go<rel>.tsv`. **A patch-level migration should produce
an empty census; a non-empty one is a finding.** The census lists every `//go:embed` directive, every
linker-synthesized `//go:linkname` target and every new syntax form, each paired with a behavioral probe. No inherited
population rule is reused until it has been run against the banked set.
*(Edited in place 2026-10-01, §7 lessons 52-54: three instruments refuse without the population file, an inherited
rule would have dropped three banked rows, and each construct was first found at H10 or later.)*

**Every count here and after states its axes**: the population (convertible, or raw `go list std`), the tags
(`purego,math_big_pure_go`), `CGO_ENABLED=0`, `ReleaseTags` pinned per side, the toolchain driven by its own GOROOT,
and the GOEXPERIMENT baseline read from `internal/buildcfg` at the target (H8's amendment (d) is the detail). **New
package IDs** get their NuGet prefix reservation filed now, not at H11.
*(Edited in place 2026-09-26, §7 lessons 11-12: two rulings were withdrawn over counts with unstated axes, and the
reservation was still an
external wait after the release.)*

### H4 — Converter feature work **GATE**

Whatever the release's language delta requires, plus whatever the census surfaced. Each item follows
the standing repository discipline: root-cause against emitted `.cs`, land a behavioral regression
test, update the conversion-strategy reference (and the summary only if the headline mapping moved),
and prove `check-no-regression` clean **on the outgoing corpus** where the change is meant to be
neutral.

**Three recurring work items belong here by ruling rather than being discovered as audit findings:**

- **The hand-owned test host.** `src/core/testing` is skip-listed and never converted, so it follows
  **nothing** automatically while upstream keeps adding to `testing`'s API. It is a named work item of
  every migration that adds one.
- **The `go.mod` readers.** New `go.mod` verbs are silently dropped by lax parsing, so any new
  directive in the target release owes a re-check of the converter's `go.mod` handling rather than an
  assumption of safety.
- **Throwing linkname bodies, per non-Windows GOOS.** Every linkname body that throws `NotImplementedException` is
  answered with Go's own unsupported result, and is re-censused at H6.
  *(Added 2026-10-01, §7 lesson 50: this list came from a ruling, and the third item comes from a measured miss.)*

**Gate:** CNR byte-identical over the full behavioral corpus, zero `NOT MEASURED`. Budget from
CLAUDE.md's `check-no-regression.ps1` row, from the top of its range.

### H4a — The opening deliberate-regen slot

**A standing slot, not a step with fixed contents.** The repository accumulates *queued leveling
items*: converter emission changes that landed without their corpus regen, born-stale banked
artifacts, and cosmetic emission nits explicitly deferred to "the next deliberate regen". Each is
individually too small to justify a full reconvert, and the standing rule is **restore rather than
level** until one regen can carry them all.

**The queue is a LEDGER, not a memory, and it lives in three places a migration reads together**:
[`CleanupBacklog.md`](CleanupBacklog.md) (numbered housekeeping items), the *unbanked intended
drift* inventory under `docs/phase4/` (converter arcs that landed without their regen, each row
carrying the evidence checkable against the committed tree today), and the BOARD's standing
*born-stale, restore rather than level* entries, which name each deferred artifact **at its banked
counts**. A queued item recorded in none of them is one nobody will find at the regen — so
**deferring to "the next deliberate regen" is not complete until the deferral has a ledger row**.

**The hand-own census this slot always runs: for every marked hand-own, did the hop move any of its
principal's declarations into a file the new release newly selects and the reconvert therefore emits?**
A hand-own whose Go principal *shrank* between releases has a twin in the emission: the declaration
the hand-own still carries is now also auto-converted from the file it moved to. The answer is a
per-declaration body displacement (`manualConversionFuncs`), never a field or a file exclusion, and
it is hop-conditional by construction — the registration's guard is red at the old release, so it
lands with the hop on the version branch, never on master ahead of it.
<!-- 2026-09-13: first instance C1-1 (the `note` type left runtime2.go for note_other.go); second C1-2b
     (lock2/unlock2 left lock_sema.go for lock_spinbit.go under goexperiment.spinbitmutex, and m.mWaitList is
     the waiter queue the managed lock core documents as not modelled). C1 76e4026ae, i9 fd4611ae7 + 8c0f26247,
     C2 9a98cfa8; ruled 6cee25f56 and 8be44bbc0. -->

**A corpus migration is that regen.** Schedule the bundle **before H5**, for one reason: H5's overlay
diff is the migration's primary signal, and every un-levelled artifact is noise inside it. Levelling
first is what makes the upstream delta readable.

**Two shapes.** While the outgoing record is live, H4a is the landed bundle below. When the outgoing record is
frozen at its anchor, H4a is ONE seeded three-target old-release regen into staging, committing nothing — H0's
`.cs.auto` baseline, H6's old side and H5's overlay comparand (Amendment 2026-09-13 below).
*(Edited in place 2026-09-26, §7 fix A3: the "one commit series" text stood alone while the 1.23 → 1.24 hop ran the
second shape.)*

**The bundle owes, in one commit series:**

1. every queued converter emission fix, each with its own CNR;
2. a seeded full reconvert;
3. **`go generate .` in `src/go2cs`** — `stdlib-metadata.txt` is generated FROM the corpus and gated
   by `TestStdLibMetadataInSync` under the plain converter `go test`, so a regen banked without it
   leaves the converter gate red at master **for whoever runs it next**, not for the lane that caused
   it;
4. the born-stale rows re-swept **at their banked counts** — the whole point of the class is that the
   staleness is emission drift, not a verdict change, so the sweep is unaffected.

Any small deferred housekeeping that needs a quiet point (unregistered solution members, and the
like) rides here too.

#### Amendment 2026-09-13 — the pin assertion, both halves and the pairing, as copyable blocks

H4a and every step of the H5 series are run by hand, so no harness pins them. Placeholders used from here to H9:

| placeholder | meaning | defined from |
|:--|:--|:--|
| `<landing>` | master after the train-47 landing, its tree hash asserted (`git rev-parse <landing>^{tree}`) before use | H4a |
| `<tree>` | a clean detached worktree: of `<landing>` at H4a; of the version branch at `<H2>` from H2 onward | H4a (`<landing>`), H2 (`<H2>`) |
| `<H2>` | the version branch's H2 commit, named by COORD. **It does not exist at H4a and no H4a step reads it** | H2 |
| `<GOROOT-1.23.12>`, `<GOROOT-1.24.13>` | spelled exactly as `go env GOROOT` prints them, backslashes and all (floor 6) | — |
| `<GOROOT-…-posix>` | the same roots in the shell's spelling (`/c/...`) | — |
| `<stage>` | a drive-letter, forward-slash directory outside every clone, with no module above it; `<stage-posix>` its `/c/...` spelling | H4a |
| `<build root>` | `<stage>/h5` for the ladder; `<tree>` for the gate (the per-flavour build amendment under H7) | H7 |

No path contains a space. Each step is a Git Bash script with `set -uo pipefail` and `export
MSYS_NO_PATHCONV=1` (no `-e`), so **every call below is written `|| exit 3`**: a function's `return 3` stops
nothing by itself (floor 7).

```bash
pin () {  # pin <release> <GOROOT, backslash spelling as `go env GOROOT` prints it> <the same root, POSIX>
  export GOROOT="$2" GOTOOLCHAIN=local PATH="$3/bin:$PATH"
  local d m v r f; d=$(mktemp -d '<stage-posix>/pin.XXXXXX') || return 3
  m=$(cd "$d" && go env GOMOD); v=$(cd "$d" && go version | awk '{print $3}')
  r=$(cd "$d" && go env GOROOT); f=$(head -n1 "$3/VERSION" | tr -d '\r')
  echo "  pin: taken from ${d##*/} under <stage> (GOMOD=$m), in this script's environment: $(cd "$d" && go version); VERSION $f"
  rmdir "$d"
  case "$m" in ''|NUL|/dev/null) ;; *) echo "ABORT: the pin directory is inside a module"; return 3 ;; esac
  [ "$v" = "$1" ] && [ "$f" = "$1" ] && [ "$r" = "$2" ] || { echo "ABORT: pin is not $1"; return 3; }
}
pin_pair () {  # the H2->H5 window pairing at go1.23.12: pin_pair <GOROOT-1.23.12> <GOROOT-1.23.12-posix>
  unset GOTOOLCHAIN; export GOROOT="$1" PATH="$2/bin:$PATH"
  local d m v r f t; d=$(mktemp -d '<stage-posix>/pin.XXXXXX') || return 3
  m=$(cd "$d" && go env GOMOD); v=$(cd "$d" && go version | awk '{print $3}')
  r=$(cd "$d" && go env GOROOT); t=$(cd "$d" && go env GOTOOLCHAIN); f=$(head -n1 "$2/VERSION" | tr -d '\r')
  echo "  pin_pair: taken from ${d##*/} under <stage> (GOMOD=$m, GOTOOLCHAIN=$t): $(cd "$d" && go version); VERSION $f"
  rmdir "$d"
  case "$m" in ''|NUL|/dev/null) ;; *) echo "ABORT: the pin directory is inside a module"; return 3 ;; esac
  [ "$t" = auto ] || { echo "STOP: GOTOOLCHAIN reads $t (a user-level go env -w value): post it; never go env -w on a fleet box"; return 3; }
  [ "$v" = go1.23.12 ] && [ "$f" = go1.23.12 ] && [ "$r" = "$1" ] || { echo "ABORT: pairing is not go1.23.12"; return 3; }
}
```
<!-- COORD 9bdca5025 / 214f2bf7d: a pin assertion is taken from a directory with no module above it AND in the same environment the
     work runs in, asserting the version string and `go env GOROOT` against the named pin and refusing on either (C1 688cea0f5's third
     cell: an ambient install that is neither pin); GOROOT exported in its backslash spelling with its bin first on PATH; the emission's
     toolchain provenance lines read afterwards. COORD dfd85f700 §6 (i9 0b3c12a49 §6): every pin assertion NAMES the directory it is
     taken from — here a fresh directory under <stage>, so the name carries no profile path. VERSION by `head -n1`: i9's control that
     stripping the whole file concatenates its second line into the value. `|| exit 3` at every call: CLAUDE.md floor 7; lane R's
     setup-convert.sh:26-28 exits rather than returning. GOTOOLCHAIN non-auto: KICKOFF-fleet.md:176 at a02ac3df3 (a user-level
     `go env -w GOTOOLCHAIN=go1.23.1` made bare `go version` report go1.23.1). Mechanics: setup-convert.sh:21-28 (2026-09-13); the
     GOMOD arm is added because that script assumes its temp dir has no module above it. -->

- **Build half** — `pin go1.24.13 '<GOROOT-1.24.13>' '<GOROOT-1.24.13-posix>' || exit 3`, then build (the
  seeded-roots amendment under H5; `src/go2cs/go.mod` already reads `go 1.24.13` at `a02ac3df3`); after it,
  `go version <exe>` reads `go1.24.13`.
- **Convert half, H5** — the same pin, `|| exit 3`. **Convert half, H4a** — `pin go1.23.12 '<GOROOT-1.23.12>'
  '<GOROOT-1.23.12-posix>' || exit 3` in a SEPARATE script: the split pin. The converter is never built in a
  convert-half shell.
- **Window batteries** (CNR and the behavioral suite while the corpus is still 1.23.12) take `pin_pair … || exit 3`.
  **`-tests` rows and the sweep take the split instead:** the converter built under `pin go1.24.13`, the pipeline
  run under go1.23.12, and `-SkipBuild` MANDATORY for the sweep (this document's H2 ruling, the two staleness
  guards; KICKOFF-fleet.md:176).
- **After every conversion, read what the converter read.** On a converter carrying `7c1d8832f`
  (`git -C '<tree>' merge-base --is-ancestor 7c1d8832f HEAD`), its log's `toolchain: GOROOT … (VERSION <rel>,
  read in-process)` line names `<rel>`. `7c1d8832f` is in master since train 48 (`271300cea0` and after; the
  provenance line lives at `src/go2cs/toolchainResolution.go`), so a `<landing>` at or after it always has the row; on an
  older tree the line does not exist, the row is NOT AVAILABLE, and the first arm is the pin assertion plus the `mcleanup`
  presence arm — posted as the weaker instrument. Post the bare `go version` line
  and the VERSION token — never a GOROOT value.
<!-- Pairing vs split: this document's H2 ruling (a02ac3df3:docs/GoCorpusMigration.md:249-275); KICKOFF-fleet.md:176 at a02ac3df3.
     After-the-run clause: COORD 214f2bf7d (loader-directory root and its VERSION printed) and 0b5d72d0e §3 (read the emission's own
     path lines; a presence check is a cheap second arm). Provenance line: 7c1d8832f:src/go2cs/toolchainResolution.go:399; ancestry
     measured 2026-09-13 (merge-base --is-ancestor false against a02ac3df3 and 44fbc381a). No-value posting: .claude/rules/docs-records.md. -->

#### Amendment 2026-09-13 — for the 1.23 → 1.24 hop, H4a is a staging BASELINE, not a landing

The outgoing record is frozen at its anchor, so a levelled 1.23.12 corpus has nothing to publish or bank.
**H4a runs as ONE seeded three-target 1.23.12 regen into a staging root, on `<landing>` as its `<tree>`, by the
same converter binary that runs H5, and nothing from it is committed.** That root is at once (a) H0's fresh
`.cs.auto` baseline, (b) H6's old-side `.auto`, and (c) H5's overlay comparand: master → H4a is the queued
levelling noise, H4a → H5 the upstream delta. Of the bundle above: **item 2** (the seeded full reconvert) runs
as this staging baseline and is not committed; **item 3** (`go generate .`) moves to the H5 series (the overlay
amendment under H5); **item 4**'s three ledgers are consumed in H5's triage of master → H4a, not swept at 1.23.12;
**item 1**'s disposition (queued fixes, each with CNR) is not ruled for this hop — owed to COORD, and not run
until it is.
<!-- COORD db6d9462f §3.2 (the ruling; "the owner may object here"; it rules item 4 and says nothing about item 1), §3.3 (the series
     order, go generate after the overlay); KICKOFF-fleet.md:131-132 at a02ac3df3. Bundle items: a02ac3df3:docs/GoCorpusMigration.md:368-376.
     Three-target: ruling 3, COORD 9495495ec, so the comparand matches H5's emission. <landing> as H4a's tree: COORD ruling on the
     handoff (H4a runs on the landing tree, tree asserted; <H2> defined only from H2 onward). -->

1. **Entry gate.** H4 closed: every converter cut this hop needs has landed on `<landing>`. Any later change
   under `src/go2cs` or `src/gen` (read at H5 by `git diff --quiet <landing> <H2> -- src/go2cs src/gen`) means the
   binary is not H5's: re-run H4a from `<H2>`'s converter into a fresh `<stage>`, never reuse this one.
2. Build the binary once and seed BOTH roots from `<landing>` — the seeded-roots amendment under H5. The H4a
   root's `version.props` is `<landing>`'s (1.23.12); the H5 root's is written at H5 from `<H2>`.
3. `pin go1.23.12 '<GOROOT-1.23.12>' '<GOROOT-1.23.12-posix>' || exit 3`, then the reconvert amendment's command
   under H5 with `R=h4a`.
4. That amendment's checks, with two expectations inverted: `runtime/mcleanup.cs` ABSENT, the provenance line
   (where available) naming `go1.23.12`. **H4a is run A of H6's named blind spot:** all three `Failed:` lines
   read 0, its marker gate reads zero violations, and **its package count is asserted against the outgoing
   corpus's** before any H6 or H5 use:
   ```bash
   want=$(git -C '<tree>' -c core.quotePath=false ls-files -- 'src/core/*package_info.cs' | wc -l)
   for g in windows linux darwin; do echo "  $g: package_info.cs in stage $(find '<stage>/h4a-stage/'"$g"'-amd64' -name package_info.cs | wc -l) of $want tracked"; done
   ```
   Want: every difference NAMED (a skip-listed package, a platform-exclusive package of another flavour, a
   per-GOOS folder); an unnamed difference is a STOP. The predicate is NOT MEASURED against a real stage root.
5. Keep `<stage>/h4a` whole, `.cs.auto` included, until H6's audit closes. Never convert into it again (floor 1).
   H4a's counts are recorded in its post and the share manifest (the artifacts amendment under H5); nothing is committed.
<!-- Run A package count: a02ac3df3:docs/GoCorpusMigration.md:576-577 ("Assert run A's package count and marker gate against the
     outgoing corpus's before trusting it"). -->


### H5 — Seeded full reconvert **GATE**

**CLAUDE.md's reconvert ritual, unchanged and unabridged.** A migration is the *most* likely moment to
skip a step of it, so the non-negotiables are restated rather than referenced:

- **Seed first.** Copy `src/core`, `src/version.props` and `docs/validation` into the staging root,
  mirroring the `src/` layout, and convert with `-go2cspath <staging>/src`. An unseeded root gives the
  hand-own marker nothing to detect, so every whole-file hand-own is emitted as a plain `.cs` and the
  overlay rule protects **nothing** — the auto conversions compile and are operationally broken. Since
  the per-GOOS corpus layout landed, an unseeded root also breaks layout adoption: there is no
  per-GOOS folder to route into, so every platform-varying file lands flat and the next build compiles
  two copies.
  - **A BUILD of the staged corpus needs two more things seeded: `src/gen` and
    `src/Directory.Build.props`.** The seed list above is what a *conversion* needs; without the generator
    project every generated half is missing, and the failure reads exactly like a corpus defect (62
    CS8795/CS1739/CS0029 errors in `internal/runtime/atomic` and `internal/goarch`, upstream of every site
    the reading set out to reproduce). The tell is location, not count: *a reproduction that fails
    somewhere other than the sites it set out to reproduce has not reproduced.*
    <!-- 2026-09-13: i9 0687402db s2 on the H4a/H5 rung at the train-47 union; ruled f633ad759 s3. -->
- **Never convert twice into one staging root**, and never let two conversions overlap in one. Delete
  and re-seed per run, and confirm no converter process is alive before starting. The recorded failure
  is a single corrupted file with unresolved lift markers that reads exactly like a converter
  regression and is not one.
- **Wrap the converter call so its stderr warnings do not abort the wrapper** — a terminating
  error-action policy turns a native stderr line into a fatal, which is how the overlapping-run
  corruption happened in the first place.
- **Keep each target's converter stderr and diff its warning counts by kind against the outgoing pin's reconvert.** A
  new kind is sized as a predicted H7 red.
  *(Edited in place 2026-10-01, §7 lesson 56: this bullet was added. One warning fired at all 29 sites of an H7 red
  class while every A/B sent stderr to `/dev/null`.)*
- **The marker gate is PATH-PRECISE, line-anchored, whole-file, and re-measured** ⟲. Per marked path,
  the staging root must not hold a freshly-**emitted** plain `.cs` — either a `.cs.auto` sits beside
  it, or nothing was emitted there. Counts intentionally differ from the census, in both directions,
  so **a same-count assertion is wrong**. Three census traps, each paid for: a head-window scan
  under-counts badly (markers sit below long license blocks); an unanchored match over-counts
  (placeholder comments *mention* the marker); and a default ripgrep honors `src/core/.gitignore` and
  under-counts — census with `git grep` or a raw filesystem walk.
- **Classify emitted-vs-seeded by a sentinel modification time**, not by content: seeding puts every
  repository file in the staging root, so an overlay can never reveal a file the converter has
  *stopped* emitting unless the classification is time-based. **A hop's corpus-side DELETION bill is
  a first-class number, and this classification is the only thing that can see it.** The 1.24 trial
  measured **31 files** — 28 whose principal Go file is gone, 2 build-tag flips (`sync/map.cs` among
  them), 1 other — and an unclassified stale sibling is not a diff but a COMPILE ERROR: the
  `aliastypeparams` baseline flip emits the `_on` file while the seed still holds the `_off` one,
  i.e. CS0102. State the bill with the emission census; do not discover it at the build.
- **Overlay `.cs`, `.csproj` and `README.md`, excluding `*.cs.auto`.** Two knowns that are not drift:
  the root attribution files the converter re-copies (modified with an **empty** numstat — pure
  line-ending phantoms, restore them), and the **hand-owned-by-consequence** packages, whose single Go
  file is entirely hand-owned so the driver never reaches project-file emission and their `.csproj`,
  `package_info.cs` and `README.md` are never re-emitted. **A migration that adds a package to that
  class must notice.**

- **Rehearse first, on a scratch at the union tree, with every count written down**, and report projects built
  beside the error count — one failing leaf skips everything above it. Commit progress as CHECKPOINTs even while
  red, moving `src/go2cs.slnx` in the same commit. A BUILD's seed adds `src/gen` and `src/Directory.Build.props`;
  the overlay carries the root attribution files.
- **H5c's selection is the converter's**: bare `-stdlib`, the printed tag line quoted, never a second tag
  resolution, never an mtime or a seeded root's presence; a hop-stale census; every class count printed at zero;
  an orphaned hand-own relocates with its principal (`git mv` plus its namespace and class lines).
*(Edited in place 2026-09-26, §7 lessons 16-17: the two bullets above.)*

**Gate:** H5c applied (a dry run, then `-Apply` with zero UNRESOLVED and zero UNEXPLAINED-DESELECTION
(`reconvert-deletions.ps1`'s exit 0) and full flavour agreement), then the overlay
completes with the marker gate at zero violations and **every diff classified** (§4).
*(Edited in place 2026-09-26, §7 fix A4: the gate named no deletion step, though the H5c amendments of 2026-09-07 and
09-13 make it one.)*
*(Edited in place 2026-10-01, §7 lesson 58: the instrument's exit 0 needs both zeros.)*

#### Amendment 2026-09-07 — the DELETION PASS is a required step, and it has an instrument

The bullet above states the deletion bill as a *number to report*. The 1.24.13 rehearsal
([`docs/phase4/REHEARSAL-h5-go124.md`](phase4/REHEARSAL-h5-go124.md) §3) showed that reporting it is
not enough: **the stale files have to be removed from the staging root before the overlay, and nothing
in the ritual performed a deletion.** That rehearsal's *first* build died in 116 s having measured
nothing — `internal/goexperiment/exp_aliastypeparams_off.cs` (seeded, 1.23.12) and
`exp_aliastypeparams_on.cs` (emitted, 1.24.13) both declare `AliasTypeParams`, the package csproj globs
`*.cs` so both compile, and the result is **CS0102 ×2 in a leaf essentially the whole corpus depends
on**. A stale sibling is not a diff to classify at leisure; at a hop it is a compile error in front of
every other measurement.

**So H5 gains a step, between the reconvert and the overlay:**

> **H5c — deletion pass.** Run **`src/reconvert-deletions.ps1`** (launcher `reconvert-deletions.bat`)
> against the staging root, dry-run first, then with `-Apply`. Only then overlay.
>
> ```
> .\reconvert-deletions.ps1 -Root <staging>\src -GoRoot <target GOROOT> `
>                           -ExpectGo go<target> -SourceGoRoot <source GOROOT> `
>                           -Sentinel <staging>\run.stamp
> ```
>
> The sentinel is a file created immediately **before** the conversion starts; its modification time is
> the seeded/emitted boundary (`-SentinelTime <datetime>` is the same input without a file).
> `-SourceGoRoot` is the **outgoing** release's `GOROOT` — the one the committed corpus was converted
> from — and it decides what is a deletion candidate at all (see *What makes a file a candidate*
> below). Its expected release is **derived** from `<GoStdLibVersion>` in `src/version.props` unless
> `-ExpectSourceGo` is passed. The instrument **refuses** — exit 3, before printing any table — on a
> missing or ambiguous sentinel, on a `-Root` that resolves to the repository's own `src/core`, or
> when `go version` under **either** `GOROOT` disagrees with its expected release. A deletion pass
> aimed at the wrong release deletes the wrong files, so that is a refusal and not a warning.

**Why the modification time alone cannot decide a deletion, and Go must be asked.** The converter's
write path skips a write whose bytes are identical (`needToWriteFile`), so a file whose emission did not
*change* between the two releases keeps its seed timestamp and reads SEEDED exactly like a file that
stopped being emitted. The rehearsal measured **1,292** seeded-not-rewritten production `.cs` against
**25** real deletions — the seeded set is ~50× the deletion set, and a timestamp-only pass would destroy
the corpus. The timestamp answers only *"is this a candidate"*. **Go answers "should it exist"**, via
`go list -f '{{.GoFiles}} {{.CgoFiles}}' <importpath>` under the target `GOROOT` with `CGO_ENABLED=0`
(the corpus's own emission state), `GOTOOLCHAIN=local`, and the file's own `GOOS`.

**What makes a file a CANDIDATE — the question the pass got wrong on its first real run, corrected
2026-09-07.** "Does Go still select this file's principal at the target?" is only *meaningful* for a
file the converter emits. As first landed the pass asked it of every seeded `.cs`, and lane R's first
dry run against a three-target scratch (mailbox `1f5e8f276`, dry run, **not** applied) returned a
**205-row delete set of which 117 rows were `src/core/golib/*.cs` (116) and `src/core/go2cs/Symbols.cs`
(1)** — the hand-written runtime and the Symbols shared project — classified `DELETE-ABSENT` because
*"package not in std at target"* is **true and irrelevant** for a directory that was never a Go package.
The marker arm could not save them: `golib` correctly carries no `[module: GoManualConversion]` marker,
because nothing converts into it and there is no generated body for a marker to displace. **The one
directory that needs no marker is the one the marker guard does not protect.**

The candidate test is therefore **positive and first**: a seeded file is a candidate only if its
resolved import path is in **`go list std` at the SOURCE release** (`-SourceGoRoot`, per flavour) and
is **not** skip-listed by the converter's own `isNonConvertedStdLibPackage`. Asking the *source*
rather than the target is what keeps a **removed** package a candidate — `internal/weak` is in std at
1.23.12 and gone at 1.24.13, which is exactly the `DELETE-ABSENT` the pass exists to find.

**The classes**, each printed with its count whether or not it is zero:

| class | meaning | deleted? |
|:--|:--|:--|
| `NOT-A-CONVERSION-TARGET` | the converter does not emit into this directory at all: a hand-written repository root (`golib/`, `go2cs/`), a std package the converter skip-lists (`unsafe`, `builtin`, `testing`, `cmd…`), an import path absent from std at the **source** release, or a `.cs` sitting directly in `core/` | **never** — tested first, ahead of the marker scan and any target lookup |
| `PROTECTED` | line-anchored `[module: GoManualConversion]`, or an `*_impl.cs` companion, **inside** a package the converter does emit | **never** — a hand-own is an H6 reconciliation item, not a deletion |
| `KEEP-SELECTED` | Go still selects the principal at the target for this flavour | no — the dominant class, and the pass's own negative control |
| `DELETE-ABSENT` | the principal, or its whole package, is gone at the target (H3 removals) | yes |
| `DELETE-DESELECTED` | the principal still exists on disk but Go does not select it for this flavour — a build-tag or GOEXPERIMENT flip | yes |
| `UNEXPLAINED-DESELECTION` | a `DELETE-DESELECTED` candidate that neither release selects under the converter's tag set while its principal is present at the target | **never** — and the run exits non-zero, with or without `-Apply` |
| `UNRESOLVED` | no Go principal is derivable **inside a package that survives at the target**, and the file is not generated metadata — a stem that maps to no `.go` name | **never** |
| `KEEP-METADATA` | generated metadata in a package that survives at the target | **never** — listed by name, and not blocking |

*(Rows added and the `UNRESOLVED` row narrowed 2026-10-01, §7 lesson 58: they come from the instrument's amendments,
`src/reconvert-deletions.ps1:120-135`. Generated metadata was `UNRESOLVED` until `KEEP-METADATA` took it, so the
`crypto/ecdh/package_init.cs` example below predates that class.)*
<!-- The draft added the two rows only. The UNRESOLVED row was narrowed at the apply (2026-10-02, e2008427b1): it read
     "inside a package the converter emits — generated metadata (package_info.cs, package_init.cs) and anything else
     whose stem maps to no .go name", which the new KEEP-METADATA row contradicted
     (src/reconvert-deletions.ps1:128-135). -->

**`UNRESOLVED` stops the step for a human.** Those rows are always listed, and the class is not
hypothetical and not automatable from a file name: the rehearsal's 25 contains exactly one,
`crypto/ecdh/package_init.cs`, a genuinely stale generated file whose staleness only a reader can
confirm. Deleting it on a guess and dropping it silently are both wrong; the pass does neither and
refuses to report success until somebody has disposed of it.

**Exit 2 means NOTHING WAS DELETED.** As first landed the `UNRESOLVED` check ran *after* the
`Remove-Item` loop, so a run that exited 2 had already deleted — a report wearing a refusal's exit
code, measured at HEAD as **7 files removed on an exit-2 run, `golib/` and `go2cs/` among them**. Every
check that can produce exit 2 now runs first: the `UNRESOLVED` check, the delete-set decomposition
(printed per class before the loop), and a **trespass assertion** — no delete row may sit under a
hand-written root or a skip-listed package directory, re-derived from the **path** rather than from the
`go list` that classified it, and re-asked per row immediately before each irreversible act. With
`-Apply`, `UNRESOLVED` rows are a refusal rather than a footnote: dispose of them, then re-run.

**The skip-list is a MIRROR and it is guarded.** `go list std` names `unsafe` and `testing` like any
other package, so no Go question can exclude them; the converter's `isNonConvertedStdLibPackage`
(`src/go2cs/stdLibConverter.go`) is the only authority and the script carries a copy of it.
`reconvertDeletionsSkipList_test.go`, under the plain `go test ./...` in `src/go2cs`, extracts the
script's literal and compares the two **sets** in both directions — an extra name in the script keeps
stale files that should go, a missing one offers a hand-owned package's files for deletion. Because the
`.ps1` sits outside the converter's module root, cmd/go drops it from the test's input fingerprint:
**any change to the script owes `go test -count=1 ./...`.**

Files the run emitted are not candidates at all, and neither are the `<Compile Remove>`d test-host
artifacts (`package_test_info.cs`, `go2cs_test_host.cs`, `*_test.cs`) — a stale one cannot produce the
CS0102 this pass exists to prevent, and admitting them buries the real rows (the rehearsal subtracted
384 of them from the same arithmetic). They are counted, not listed.

**Two notes on reading the bill.** The `-platforms` multi-target emission is what H5 actually runs
(§H8's requirement, restated in the rehearsal's §8), and a **single-target** run recomputes only its own
flavour — so a deletion pass over a single-target root answers only for that flavour, and an L3 per-GOOS
folder is asked under **its own** `GOOS` regardless of the run's target. And this amendment does not
revise the bullet above: that bullet's **31** is an earlier trial's reading and stands as its own dated
measurement; the rehearsal's **25** is a different run of a different release with a different
instrument. Neither supersedes the other, and both are point-in-time — **re-measure, never carry the
count.**

#### Amendment 2026-09-13 — the seeded roots, exactly: two roots, one seed, one binary

**Preconditions, at H4a.** `git -C '<tree>' status --porcelain` empty and `<tree>` at `<landing>`; ≥ 25 GB free
on `<stage>`'s drive (floor 12); no converter alive on the box —
`powershell -NoProfile -Command "@(Get-Process go2cs -ErrorAction SilentlyContinue).Count"` reads 0, a count
and never a kill (floors 1, 5). **Preconditions, at H5** (once `<H2>` exists): `<tree>` re-pointed at `<H2>`,
clean; `git -C '<tree>' diff --quiet <landing> <H2> -- src/core src/gen src/Directory.Build.props docs/validation`
(H2 moves none of the seed, so the seed taken at H4a serves both releases; non-quiet is a STOP, posted by file
list); and the two commits the H5 steps read are in `<H2>`:
```bash
git -C '<tree>' merge-base --is-ancestor 826045a74 '<H2>' || { echo "STOP: h5-removals.txt (826045a74) not in H2"; exit 3; }
git -C '<tree>' merge-base --is-ancestor 7c1d8832f '<H2>' && echo "  provenance line AVAILABLE" || echo "  provenance line NOT AVAILABLE (7c1d8832f not in H2)"
```

**Build half, at H4a** (`pin go1.24.13 … || exit 3`, the pin-assertion amendment under H4a):
```bash
mkdir -p '<stage>/bin' '<stage>/logs'; [ -e '<stage>/bin/go2cs.exe' ] && { echo "ABORT: a binary already stands"; exit 4; }
( cd '<tree>/src/go2cs' && go build -o '<stage>/bin/go2cs.exe' . ); rc=$?
[ "$rc" = 0 ] && [ -f '<stage>/bin/go2cs.exe' ] || { echo "ABORT: build rc=$rc, or no binary at the invoked path"; exit 4; }
[ "$(go version '<stage>/bin/go2cs.exe' | awk '{print $2}')" = go1.24.13 ] || { echo "ABORT: embedded toolchain"; exit 4; }
( cd '<stage>/bin' && sha256sum go2cs.exe > go2cs.exe.sha256 ) || exit 4
```
Built ONCE. Before each conversion the reconvert amendment copies the checked hash to `<stage>/<R>.exe.sha256`;
before H6 and before the `d-hop` comparand, `cmp '<stage>/h4a.exe.sha256' '<stage>/h5.exe.sha256'` must pass.

**Seed BOTH roots at H4a, before either converts:**
```bash
for R in h4a h5; do
  [ -e "<stage>/$R" ] && { echo "ABORT: <stage>/$R exists"; exit 5; }
  mkdir -p "<stage>/$R/src" "<stage>/$R/docs"
  ( cd '<tree>/src' && tar -cf - --exclude=bin --exclude=obj --exclude=Generated core gen ) | ( cd "<stage>/$R/src" && tar -xf - ) || exit 5
  cp '<tree>/src/Directory.Build.props' "<stage>/$R/src/" || exit 5
  ( cd '<tree>/docs' && tar -cf - validation ) | ( cd "<stage>/$R/docs" && tar -xf - ) || exit 5
done
git -C '<tree>' show '<landing>:src/version.props' > '<stage>/h4a/src/version.props' || exit 5
want=$(git -C '<tree>' -c core.quotePath=false ls-tree -r --name-only HEAD -- src/core | grep -c '[.]cs$')
for R in h4a h5; do
  have=$(find "<stage>/$R/src/core" -name '*.cs' | wc -l)
  echo "  $R: seeded .cs $have of $want tracked"
  [ "$have" = "$want" ] || { echo "ABORT: $R seed is partial"; exit 5; }
done
pv () { grep -oE '<GoStdLibVersion>[^<]+' "$1" | tail -n1 | sed 's/.*>//'; }
[ "$(pv '<stage>/h4a/src/version.props')" = 1.23.12 ] || { echo "ABORT: h4a pin"; exit 5; }
```
**At H5, before the H5 conversion:**
```bash
git -C '<tree>' show '<H2>:src/version.props' > '<stage>/h5/src/version.props' || exit 5
[ "$(pv '<stage>/h5/src/version.props')" = 1.24.13 ] || { echo "ABORT: h5 pin"; exit 5; }
```
The version.props pin is ASSERTED, never only printed: a root whose pin cannot be read runs the corpus pin guard
inert.

| seed member | why it is there |
|:--|:--|
| `src/core` minus `bin`/`obj`/`Generated` | floor 2: the marker detector and layout L3's per-GOOS routing need it |
| `src/version.props` | the corpus pin guard reads it beside `core`; H5's root takes `<H2>`'s file — the pin AND the reset build number the emitted badges read — never a sed of the pin line; H4a's takes `<landing>`'s |
| `docs/validation` | the README Tests and Source·C# badges read it with `version.props`; without either, both badges vanish corpus-wide |
| `src/Directory.Build.props`, `src/gen` minus build dirs | to BUILD the root: `core/Directory.Build.props` imports the file above it (the TFM) and resolves the analyzer at `$(go2csPath)gen/go2cs-gen`, which the generated solution also lists |

<!-- Lane R's seeding, 2026-09-07/08 and 2026-09-13: r-h5b-setup.sh:36-51, setup-convert.sh:38-51 (archived, sha256 in the rehearsal's
     archive manifest). Exclusions and the badge reason: .claude/skills/corpus-reconvert/SKILL.md:84-85; both seeds before either arm:
     same file :88; the binary proven at its invoked path and by its embedded release: same file :91. The tracked count: git quotes
     non-ASCII names by default, so the 13 golib `ж.*.cs` lines end in `.cs"` and a default `ls-tree | grep -c '[.]cs$'` reads 3751
     against 3764 with core.quotePath=false — measured at a02ac3df3 on 2026-09-13 (both verifiers; the trap is documented at
     src/migrate-gorelease.ps1:183-186); the fifth rehearsal's real seed read "seeded .cs 3764". The equality replaces the archived
     scripts' carried floor of 3500; 0 tracked files sit under any bin/obj/Generated at a02ac3df3. Unreadable pin runs the guard inert:
     a02ac3df3:src/go2cs/toolchainResolution.go:348-353. version.props beside core: 7c1d8832f item 4 (commit message, "THE PIN STOPS
     PASSING UNPINNED", corpusPinnedReleaseOrError), approved COORD bc59c619d §1; rules line C2 830fa8d26. The H2 file rather than a
     sed: this document's H2 (build-number reset) and H12 (badges read version.props). Props members read at a02ac3df3:
     src/core/Directory.Build.props:6 and :14, src/go2cs/solutionGenerator.go:53. A rehearsal with no H2 commit may sed the pin line
     alone (r-h5b-setup.sh:48-51) and must say its badge lines are not the series'. -->

#### Amendment 2026-09-13 — the three-target reconvert: command line, sentinel, checks

Convert half (`pin go1.24.13 … || exit 3` for `R=h5`; `pin go1.23.12 … || exit 3` in a separate script for `R=h4a`):
```bash
export CGO_ENABLED=0     # the corpus's emission state
R=h5; EXE='<stage>/bin/go2cs.exe'; SENT="<stage>/$R.run.stamp"; ST="<stage>/$R-stage"
LOG="<stage>/logs/$R-convert-$(date +%Y%m%d-%H%M%S).log"
( cd '<stage>/bin' && sha256sum -c go2cs.exe.sha256 ) || exit 4
cp '<stage>/bin/go2cs.exe.sha256' "<stage>/$R.exe.sha256" || exit 4
[ -e "$ST" ] && { echo "ABORT: $ST exists"; exit 5; }; mkdir -p "$ST"
n=$(powershell -NoProfile -Command "@(Get-Process go2cs -ErrorAction SilentlyContinue).Count" | tr -d '\r')
[ "${n:-0}" = 0 ] || { echo "ABORT: $n converter(s) alive"; exit 2; }
rm -f "$SENT"; touch "$SENT"; sleep 2
START=$(date +%s)
"$EXE" -stdlib -comments -go2cspath "<stage>/$R/src" -platforms windows/amd64,linux/amd64,darwin/amd64 \
  -platform-stage "$ST" -convert-timeout 90m > "$LOG" 2>&1
rc=$?; echo "  exit $rc after $(( $(date +%s) - START ))s"
```
<!-- Lane R: r-h5b-convert.sh:11-28 (2026-09-07/08), setup-convert.sh:53-61 (2026-09-13; the sleep keeps an emitted file from sharing
     the sentinel's second). Three targets: ruling 3, COORD 9495495ec. `-comments` on every stdlib run: .claude/rules/converter.md:115-118.
     CGO_ENABLED=0 is the corpus's emission state (this document's §3.3) and every archived conversion script exported it
     (r-h5b-convert.sh:3). The sentinel lives in <stage>, never a session directory: this document's §3.4. -->

| check | read | want |
|:--|:--|:--|
| exit and wall | `$rc`, the elapsed line | 0; wall posted |
| log encoding | <code>head -c 200 "$LOG" &#124; tr -d -c '&#92;000' &#124; wc -c</code> | 0 — otherwise every grep below lies |
| degraded packages | `grep -ac 'did not fully type-check' "$LOG"` | 0 |
| per-target failures | `grep -a 'Failed:' "$LOG"` | three lines, each `Failed: 0` |
| warnings | `grep -a WARNING "$LOG"` | posted verbatim, uncounted and never gated (147 lines read on lane R's box, 2026-09-13: a reading, not a want) |
| what was read | `grep -a 'toolchain: GOROOT' "$LOG"` | provenance AVAILABLE (Block C): `VERSION go1.24.13` (H4a `go1.23.12`). NOT AVAILABLE: no line, and the row reads NOT AVAILABLE, never pass |
| emission | <code>find "&lt;stage&gt;/$R/src/core" -name '&#42;.cs' -newer "$SENT" &#124; wc -l</code> | non-zero — zero is an arm that emitted nothing: abort |
| per target | <code>find "$ST/&lt;goos&gt;-amd64" -name '&#42;.cs' &#124; wc -l</code>, three goos | reported |
| misrouted emission | <code>grep -rl --include='&#42;.cs' -E '^namespace go&#91;.&#93;std' "&lt;stage&gt;/$R/src/core" &#124; wc -l</code> | 0 (floor 6) |
| release, second arm | <code>find "&lt;stage&gt;/$R/src/core/runtime" -name 'mcleanup.cs&#42;' &#124; wc -l</code> | H5 ≥ 1, H4a 0 (minor-level only) |
| a Go root in emitted metadata | below: the box's own GOROOT basenames, not a guessed name shape | 0 before any overlay (0 read on lane R's box, 2026-09-13: a reading, not a want) |
| marker gate | below | 0 violations, 0 missing |

```bash
n=0; for b in "$(basename '<GOROOT-1.23.12-posix>')" "$(basename '<GOROOT-1.24.13-posix>')"; do
  n=$(( n + $(grep -rlF --include=package_info.cs "$b" "<stage>/$R/src/core" | wc -l) )); done; echo "  metadata naming a Go root basename: $n"
MARK='^[[:space:]]*[[][[:space:]]*module[[:space:]]*:[[:space:]]*(go[.])?[[:space:]]*GoManualConversion(Attribute)?[[:space:]]*[]]'
git -C '<tree>' grep -l -E "$MARK" -- 'src/core/*.cs' > "<stage>/logs/$R-marked.txt"
v=0; while IFS= read -r p; do
  f="<stage>/$R/$p"
  [ -f "$f" ] || { echo "  MISSING $p"; v=$((v+1)); continue; }
  if [ "$f" -nt "$SENT" ] && ! grep -qE "$MARK" "$f"; then echo "  VIOLATION $p"; v=$((v+1)); fi
done < "<stage>/logs/$R-marked.txt"
echo "  marked paths $(wc -l < "<stage>/logs/$R-marked.txt"); violations $v"
```
A GOROOT basename that is a common word (`go`) makes the metadata row meaningless: post the basenames' shape
(never their value) and read the row as NOT AVAILABLE.
<!-- Checks: r-h5b-convert.sh:30-43 and setup-convert.sh:63-72 (lane R), with wants added; three `Failed: 0` lines is the three-target
     shape measured in REHEARSAL-h5-go124.md §10.1. Emission gated rather than printed: corpus-reconvert SKILL.md:91 item (3). Readings
     on lane R's box: setup-convert-run1.log:18 (WARNING 147), :25 (position maps naming another go root: 0). Provenance line:
     7c1d8832f:src/go2cs/toolchainResolution.go:399 (C2's census/goroot seat, COORD bc59c619d, 214f2bf7d). mcleanup is minor-level only:
     sound for 1.23 against 1.24, silent on the patch (C2, accepted by COORD 2026-09-13). Marker predicate: the census instrument's
     (src/reconvert-deletions.ps1:462, handown-census.ps1), spelled without backslashes; population measured 146 against a literal grep's
     105 at bd1d26faf (COORD db6d9462f §1, H6 row), and 146 re-read at a02ac3df3 by both verifiers. -->

#### Amendment 2026-09-13 — H5c as a procedure: both GOROOTs, the fourteen as the EXPECTED set, and the executable delete until the instrument amendment is seated

**A seeded root keeps every file the new pin no longer emits.** Nothing in the reconvert or the overlay removes one;
that is why this step exists. **The H5c filter keys on the two-pin difference or the `.cs.auto` sibling, never on a
file name.** The hazard is a NAME-keyed filter in a hand-built keep or survivor list: `sort/sort_impl_go121.cs` is
converted output (Go's own `sort_impl_go121.go`, retired at 1.24), and a keep list built on `*_impl*` would keep
it. The instrument's own name test (`*_impl.cs`) does not match that file; its dry run classifies it
`DELETE-ABSENT` (`principal removed at target`).
<!-- Seeded-root sentence: COORD c58b4c01d §3 (from C1 f9f41e8d8 §5), measured absent at a02ac3df3 by R 910f2a151 §4. The name trap:
     C1 e8d90a664 §2, ruled into this step by COORD 0b5d72d0e §2; the instrument's name test is src/reconvert-deletions.ps1:758
     (`$name -like '*_impl.cs'`). The row: lane R's fifth-rehearsal windows dry run, `sort/sort_impl_go121.cs <- sort/sort_impl_go121.go
     (principal removed at target)` under DELETE-ABSENT (executability verifier, 2026-09-13). -->

**1. Inputs, before the first run.** Save two programs under `<stage>/logs/`. `extract.awk` turns ONE flavour's
report into rows and fails when its rows disagree with the report's own header counts, or when any of the six
class headers is absent (a refused run prints no table) — a broken extraction, never a reading:
```awk
{ sub(/\r$/, "") }
/^--- [A-Z-]+ \([0-9]+\) -+$/ { c = $2; n = $3; gsub(/[()]/, "", n); want[c] = n + 0; p = ""; next }
/^  [^ ]/ { p = ""; if (c ~ /^(DELETE-ABSENT|DELETE-DESELECTED|UNRESOLVED|PROTECTED)$/) { p = $1; got[c]++ }; next }
/^      / && p != "" { r = $0; sub(/^ +/, "", r); print c "\t" p "\t" r; p = ""; next }
END { bad = 0
  split("DELETE-ABSENT DELETE-DESELECTED UNRESOLVED PROTECTED NOT-A-CONVERSION-TARGET KEEP-SELECTED", need, " ")
  for (i in need) if (!(need[i] in want)) { print "EXTRACTION: class header " need[i] " absent" > "/dev/stderr"; bad = 1 }
  for (k in want) if (k ~ /^(DELETE-ABSENT|DELETE-DESELECTED|UNRESOLVED|PROTECTED)$/ && got[k] + 0 != want[k]) {
    print "EXTRACTION MISMATCH " k ": header " want[k] ", rows " got[k] + 0 > "/dev/stderr"; bad = 1 }
  exit bad }
```
`under.awk` names the listed directory a row belongs to (longest prefix; `-` for none):
```awk
NR == FNR { if ($0 !~ /^#/ && $0 != "") d[$0] = 1; next }
{ n = split($2, s, "/"); hit = ""
  for (i = n - 1; i >= 1 && hit == ""; i--) { pre = s[1]; for (j = 2; j <= i; j++) pre = pre "/" s[j]; if (pre in d) hit = pre }
  print (hit == "" ? "-" : hit) "\t" $0 }
```
And the list, read from the commit rather than a checkout:
```bash
git -C '<tree>' cat-file -e '<H2>:docs/phase4/h5-removals.txt' || exit 3
git -C '<tree>' show '<H2>:docs/phase4/h5-removals.txt' | tr -d '\r' > '<stage>/logs/removals.lst' || exit 3
```
<!-- REHEARSAL-h5-go124.md §10.7 (ff40eee3a): four extraction attempts read zeros until the extraction was asserted against the
     instrument's own header counts. Row shapes: src/reconvert-deletions.ps1:826-860 at a02ac3df3; a refusal prints no table and exits 3
     (:219-231). The six headers read in lane R's fifth-rehearsal windows report: DELETE-ABSENT, DELETE-DESELECTED, UNRESOLVED,
     PROTECTED, NOT-A-CONVERSION-TARGET, KEEP-SELECTED. extract.awk (before the header arm) reproduced 83/4/42/144 on each flavour
     section of the real three-flavour dry log (executability verifier, 2026-09-13); fed the whole three-flavour log at once it exits 1,
     so it takes one flavour's log. `git cat-file -e` before a count taken from `git show`: corpus-reconvert SKILL.md. -->

**2. Dry run, every flavour, gated on the instrument's exit, extracted as it lands.** `-ExpectSourceGo` is ALWAYS
passed: omitted, it is derived from the `version.props` of the tree the SCRIPT lives in, which after H2 names the
target, and the run refuses.
```bash
S='<stage>'
for os in windows linux darwin; do
  L="$S/logs/h5c-dry-$os-$(date +%Y%m%d-%H%M%S).log"
  powershell -NoProfile -ExecutionPolicy Bypass -File '<tree>/src/reconvert-deletions.ps1' \
    -Root "$S/h5/src" -GoRoot '<GOROOT-1.24.13>' -ExpectGo go1.24.13 \
    -SourceGoRoot '<GOROOT-1.23.12>' -ExpectSourceGo go1.23.12 \
    -Sentinel "$S/h5.run.stamp" -Goos $os -Goarch amd64 > "$L" 2>&1
  rc=$?; echo "  $os exit=$rc  log $(basename "$L")"
  case $rc in 0|2) ;; *) echo "  $os REFUSED rc=$rc: STOP, post the log"; exit 3 ;; esac   # 2 = UNRESOLVED rows stand, NOTHING deleted
  awk -f "$S/logs/extract.awk" "$L" > "$S/logs/h5c-$os.tsv" || { echo "  $os STOP: extraction disagrees with the report"; exit 3; }
  awk -F'\t' -f "$S/logs/under.awk" "$S/logs/removals.lst" "$S/logs/h5c-$os.tsv" > "$S/logs/h5c-$os.mapped" || exit 3
done
```
<!-- src/reconvert-deletions.ps1 at a02ac3df3: the derivation :294-311 ($SrcRoot is the script's own directory, _paths.ps1:176), the
     script's own advice to pass it once the pin has moved :139-143, the refusal :452, exit codes :165-173 (2: "NOTHING WAS DELETED";
     3: refused before classifying). Per-flavour loop and both GOROOTs in their `go env` spelling: lane R's r-h5b-deletions.sh:8-43;
     r-h5b-del2.sh:12-16 omitted -ExpectSourceGo and worked only because H2 had not run. -->

**3. `docs/phase4/h5-removals.txt` is the EXPECTED set, never a delete list.**
```bash
S='<stage>'; for os in windows linux darwin; do
  echo "  $os (a) removed-package rows outside the list: $(awk -F'\t' '$1 == "-" && $4 ~ /package not in std at target/' "$S/logs/h5c-$os.mapped" | wc -l)"
  ( cd "$S/h5/src/core" && while IFS= read -r D; do case "$D" in '#'*|'') continue ;; esac
      [ -d "$D" ] && find "$D" -name '*.cs' ! -newer "$S/h5.run.stamp" ! -name '*_test.cs' ! -name package_test_info.cs ! -name go2cs_test_host.cs ! -name '*.g.cs'
    done < "$S/logs/removals.lst" ) | sort -u | sed 's/^/SEEDED\t/' | awk -F'\t' -f "$S/logs/under.awk" "$S/logs/removals.lst" - | cut -f1 | sort | uniq -c > "$S/logs/removed-seeded.cnt"
  awk -F'\t' '$1 != "-" { print $1 }' "$S/logs/h5c-$os.mapped" | sort | uniq -c > "$S/logs/removed-rows-$os.cnt"
  diff "$S/logs/removed-seeded.cnt" "$S/logs/removed-rows-$os.cnt" > /dev/null && echo "  $os (b) every seeded file of a listed package is a row" || echo "  $os (b) STOP: a listed package keeps a file"
done
```

- **(a) ≠ 0** — a package absent at the target that H3 did not list: STOP, post it by name.
- **(b) STOP** — a file of a removed package classified KEEP or not-a-target: post the `diff`.
- **(c) The residue**, posted as a set: every `PROTECTED` and `UNRESOLVED` row under a listed directory, plus
  `find <stage>/h5/src/core/<D> -type f ! -name '*.cs'` for each (`.csproj`, `README.md`, `.cs.auto`, icons).
- **(d) The name trap, by the ruled key.** Over the UNION of the three flavour files (a single flavour answers
  only for per-GOOS folders and packages in std on all three: the conversion-target test runs before the name
  test and uses the flavour's own std set), take the two-pin difference: a path is *produced* in a root when it,
  or its `.cs.auto` sibling, is newer than that root's sentinel; *survivors* are produced-in-h4a minus
  produced-in-h5. A survivor in any `PROTECTED` or KEEP row is a STOP item, named:
  ```bash
  S='<stage>'; cut -f2 "$S"/logs/h5c-windows.tsv "$S"/logs/h5c-linux.tsv "$S"/logs/h5c-darwin.tsv | LC_ALL=C sort -u > "$S/logs/h5c-rows.txt"
  prod () { ( cd "$S/$1/src/core" && find . \( -name '*.cs' -o -name '*.cs.auto' \) -newer "$S/$1.run.stamp" | sed 's#^[.]/##; s#[.]auto$##' | LC_ALL=C sort -u ); }
  LC_ALL=C comm -23 <(prod h4a) <(prod h5) > "$S/logs/survivors.txt"
  for os in windows linux darwin; do awk -F'\t' '$1 == "PROTECTED" { print $2 }' "$S/logs/h5c-$os.tsv"; done | LC_ALL=C sort -u | LC_ALL=C comm -12 - "$S/logs/survivors.txt"
  ```
  NOT MEASURED against real roots. A `*_impl` stem test, if kept at all, is a labelled second arm.
- **(e) Per-file rows outside the list** (`principal removed at target`, `present but not selected for …`) are
  posted by name; they become checkable when `h5-removals.txt` gains its per-file amendment (C1's two-pin
  survivor proposal and the fifth rehearsal's re-derivation, agreeing).
- **(f) Flat rows** (no `windows`/`linux`/`darwin` segment) that are not in the same class in all three
  flavour files are posted by name: an `-Apply` deletes a flat file on one flavour's selection alone.
<!-- The list's own header, 826045a74:docs/phase4/h5-removals.txt:14-19 ("removed BY THE SEEDED RE-CONVERT ... never by hand"), applied
     through this step: C1 fa98268df §2, COORD db6d9462f §3.3, COORD 93c967d4d (boarded as the executable form), COORD 210d49537. Per-file
     amendment: COORD 0b5d72d0e §2 and c58b4c01d §3. Two-pin key: COORD 0b5d72d0e §2; C1 e8d90a664 §2 (produced = it or its .cs.auto
     sibling newer than the seed). Order of tests: src/reconvert-deletions.ps1:746-756 (Resolve-Principal, Test-ConversionTarget with
     Get-SourceStdSet -ForGoos, :679) before the name and marker test :758-771; std membership is flavour-dependent :498-500.
     (f) is a posting rule only: src/reconvert-deletions.ps1:153-156 asks flat files under -Goos. -->

**4. The UNRESOLVED disposition, row by row, posted before any removal.** The rule of record (COORD `5123a14a2`):
an `UNRESOLVED` row is **STALE** only when BOTH hold — *not emitted by any target this run* (no copy newer than
`<stage>/h5.run.stamp` on any per-target STAGE root; the roots are seeded, so the merged root's mtime cannot say)
AND *its package is absent at go1.24.13*. **Never delete** an `UNRESOLVED` row emitted this run, nor the
`package_info.cs` of a package kept alive by a `PROTECTED` hand-own.
```bash
pin go1.24.13 '<GOROOT-1.24.13>' '<GOROOT-1.24.13-posix>' || exit 3
S='<stage>'; ST="$S/h5-stage"
for g in windows linux darwin; do ( cd "$S" && GOOS=$g go list std ); done | LC_ALL=C sort -u > "$S/logs/std-1.24.13.txt" || exit 3
for os in windows linux darwin; do awk -F'\t' '$1 == "UNRESOLVED" { print $2 }' "$S/logs/h5c-$os.tsv"; done | LC_ALL=C sort -u > "$S/logs/h5c-unresolved.txt"
for os in windows linux darwin; do awk -F'\t' '$1 == "PROTECTED" { p = $2; sub(/\/[^\/]*$/, "", p); print p }' "$S/logs/h5c-$os.tsv"; done | LC_ALL=C sort -u > "$S/logs/protected-dirs.txt"
while IFS= read -r p; do
  e=no; for g in windows linux darwin; do [ -n "$(find "$ST/$g-amd64" -path "*/core/$p" -newer "$S/h5.run.stamp" -print -quit)" ] && e=yes; done
  dir=${p%/*}; pkg=$(printf '%s\n' "$dir" | sed -E 's#/(windows|linux|darwin)(/|$)#\2#g')
  live=no; grep -qxF "$pkg" "$S/logs/std-1.24.13.txt" && live=yes
  keep=no; grep -qxF "$dir" "$S/logs/protected-dirs.txt" && keep=yes
  if [ $e = yes ]; then d=KEEP-EMITTED; elif [ $keep = yes ]; then d=KEEP-HANDOWN-CONSEQUENCE; elif [ $live = no ]; then d=STALE; else d=READ; fi
  printf '%s\temitted=%s\tpackage-live=%s\tprotected-in-dir=%s\t%s\n' "$p" $e $live $keep $d
done < "$S/logs/h5c-unresolved.txt" > "$S/logs/h5c-unresolved-disposition.tsv"
awk -F'\t' '$5 == "STALE" { print $1 }' "$S/logs/h5c-unresolved-disposition.tsv" > "$S/logs/h5c-unresolved-stale.txt"
cut -f5 "$S/logs/h5c-unresolved-disposition.tsv" | sort | uniq -c
```
A `READ` row (not emitted, package live) is disposed by a reader and posted by name. This document's own named
case is one: `crypto/ecdh/package_init.cs`, "a genuinely stale generated file" in a LIVE package (the paragraph
above). The rule of record does not admit it; lane R's model delete did, by name. Post it as a named item; append
it to `h5c-unresolved-stale.txt` only on COORD's word. The package predicate (stripping a GOOS segment for layout
L3) and the stage-root `find` are NOT MEASURED on real roots.
<!-- COORD 5123a14a2 (ASK-1 ruled). Readings on lane R's box, 2026-09-13 fifth rehearsal (readings, not wants): the dry classification
     identical on three flavours, DELETE-ABSENT 83, DELETE-DESELECTED 4, UNRESOLVED 42 (24 emitted this run, 4 hand-owned-by-consequence
     package_info, 14 stale), PROTECTED 144; applied 101 (= 83 + 4 + 14); marked hand-owns 146 before and after. The 14 stale in that
     run's delete list include crypto/ecdh/package_init.cs (its package is live at 1.24.13) beside 13 package_info/package_init rows of
     removed packages, so the rule as worded yields 13 there. Earlier measurement on the fixed instrument, R 46145d0a0 (2026-09-07,
     windows): NOT-A-CONVERSION-TARGET 130, PROTECTED 138, UNRESOLVED 42. The runbook sentence: a02ac3df3:docs/GoCorpusMigration.md:494-497.
     REHEARSAL-h5-go124.md §11's 2026-09-07 disposition (15 delete / 28 keep) predates this ruling and does not govern. -->

*(Superseded 2026-10-01, §7 lesson 58: the delete is `reconvert-deletions.ps1 -Apply`, with its eight classes and
`-BuildTags`/`-TagLine`. The interim script below is kept as history. Its backup and its flavour-agreement check
still precede the delete, and each flavour's `-Apply` is gated on rc 0: this step's closing sentence and H5's Gate.)*
<!-- The draft's note ended at "kept as history."; the last sentence was added at the apply (2026-10-02), because the
     note alone dropped three guards that this step's closing sentence, item (f) and the H5 Gate keep. -->

**5. The delete — the executable interim, until the instrument amendment is seated.** `-Apply` refuses while
ANY `UNRESOLVED` row stands, and current metadata is always `UNRESOLVED`, so `-Apply` cannot complete on a
three-target root. COORD `5123a14a2` accepted the instrument amendment "-Apply admits an UNRESOLVED row the run
itself EMITTED" as an item; it is implemented on no tree. **Until it is seated**, the delete is this script, in
`<stage>/h5` only, from a per-run copy:
```bash
#!/usr/bin/bash
# H5c interim delete (COORD 5123a14a2). Deletes DELETE-ABSENT + DELETE-DESELECTED + the STALE UNRESOLVED rows. <stage>/h5 only.
set -uo pipefail; export MSYS_NO_PATHCONV=1
S='<stage-posix>'; C="$S/h5/src/core"; LOGS="$S/logs"
MARK='^[[:space:]]*[[][[:space:]]*module[[:space:]]*:[[:space:]]*(go[.])?[[:space:]]*GoManualConversion(Attribute)?[[:space:]]*[]]'
[ -d "$C" ] && [ -f "$S/h5.run.stamp" ] || { echo "ABORT: not an H5 staging root"; exit 2; }
# 1. the three flavours' DELETE classes must agree, or a flat file would go on one flavour's selection
for os in windows linux darwin; do awk -F'\t' '$1 == "DELETE-ABSENT" || $1 == "DELETE-DESELECTED" { print $2 }' "$LOGS/h5c-$os.tsv" | LC_ALL=C sort -u > "$LOGS/h5c-del-$os.txt"; done
cmp -s "$LOGS/h5c-del-windows.txt" "$LOGS/h5c-del-linux.txt" && cmp -s "$LOGS/h5c-del-windows.txt" "$LOGS/h5c-del-darwin.txt" || { echo "STOP: the flavours' DELETE sets differ; post the diff"; exit 2; }
# 2. backup first; tar reads a drive-letter archive name as a remote host, so the name is POSIX and --force-local is passed
BK="$S/h5-pre-apply.tar"; [ -e "$BK" ] && { echo "ABORT: $BK exists"; exit 2; }
( cd "$S/h5/src" && tar --force-local -cf "$BK" core ) || { echo "ABORT: backup failed"; exit 3; }
[ "$(tar --force-local -tf "$BK" | wc -l)" -gt 0 ] || { echo "ABORT: backup is empty"; exit 3; }
sha256sum "$BK" > "$BK.sha256"
# 3. the delete set
LIST="$LOGS/h5c-delete-set.txt"
cat "$LOGS/h5c-del-windows.txt" "$LOGS/h5c-unresolved-stale.txt" | LC_ALL=C sort -u > "$LIST"
echo "delete set: $(wc -l < "$LIST") paths"
before=$(grep -rlE "$MARK" "$C" --include='*.cs' | wc -l)
# 4. assert EVERY row before ANY removal
bad=0
while IFS= read -r p; do
  case "$p" in golib/*|go2cs/*|unsafe/*|testing/*|builtin/*) echo "  TRESPASS $p"; bad=$((bad+1)); continue ;; esac
  case "$p" in *_impl.cs) echo "  IMPL-COMPANION $p"; bad=$((bad+1)); continue ;; esac
  [ -f "$C/$p" ] || { echo "  MISSING $p"; bad=$((bad+1)); continue; }
  grep -qE "$MARK" "$C/$p" && { echo "  HAND-OWN $p"; bad=$((bad+1)); }
  grep -qxF "$p" "$LOGS/h5c-unresolved.txt" && ! grep -qxF "$p" "$LOGS/h5c-unresolved-stale.txt" && { echo "  UNRESOLVED-NOT-STALE $p"; bad=$((bad+1)); }
done < "$LIST"
[ "$bad" -eq 0 ] || { echo "ABORT: $bad row(s) failed the pre-delete assertions; nothing deleted"; exit 4; }
# 5. remove, then assert the post-condition
n=0; while IFS= read -r p; do rm -f -- "$C/$p" && n=$((n+1)); done < "$LIST"
left=0; while IFS= read -r p; do [ -e "$C/$p" ] && { echo "  STILL PRESENT $p"; left=$((left+1)); }; done < "$LIST"
after=$(grep -rlE "$MARK" "$C" --include='*.cs' | wc -l)
echo "removed $n; still present $left (want 0); marked hand-owns $before before, $after after (want equal)"
[ "$left" = 0 ] && [ "$before" = "$after" ] || { echo "ABORT: post-condition failed; restore from $BK"; exit 5; }
```
**Do not hand-delete outside this script, do not touch a modification time, do not line-filter a `.slnx`.** It
deletes `.cs` rows only: the fourteen's `.csproj`, `README.md`, icons and `.cs.auto` stay (step 3(c)'s residue),
and the generated solution lists every `.csproj` on disk. **Once the instrument amendment is seated, it replaces
this script:** step 2's loop with `-Apply` appended, gated on rc 0 per flavour (rc 2 means nothing was deleted),
one log per flavour carrying its `deleted …` lines and re-walked `after` arithmetic, the backup above first.
<!-- Model: lane R's fifth-rehearsal apply script (2026-09-13), genericized; its trespass, hand-own, _impl and missing-file row assertions,
     backup before removal, and still-present / marked-count post-condition are kept; its marker grep is widened to the census predicate
     above, and the flavour-agreement and UNRESOLVED-not-stale arms are added. A POSIX archive name avoided tar's remote-host reading on
     that box (executability verifier measured `Cannot connect to C: resolve failed`, rc=128, GNU tar 1.34, drive-letter -f).
     Refusal: src/reconvert-deletions.ps1:955-959; metadata UNRESOLVED :776-783; only *.cs walked :711; identical writes skipped at the
     merge, src/go2cs/platformEmit.go:420-422 and :873. A restore that rewrites mtimes reads as emission (R 46145d0a0, method note).
     COORD 210d49537 rule 4: a destructive step on a measurement tree takes a backup first. -->

**6. C1's prepared patch, then `runtime`.** Apply C1's prepared patch —
`src/apply-h5-c1-1-rederives.sh --verify <scratch>` (must name both defects on the unpatched tree), then apply, then `--verify` again (must pass), the
mcleanup hand-own body carried from `claude/c1-mcleanup-handown` or its successor — and only then build `runtime`.
Without it the 1.24 build cannot get past `runtime`. **The carry hazard:** re-deriving `runtime2.cs`/`mfinal.cs`
at H5 takes the hand-own body from `claude/c1-mcleanup-handown` (or its successor), never from the landing tree;
`mfinal.cs`'s `createfing` must read `GoFinalizerQueue.EnsureRunner()` afterwards, and `finalizerDoorGuard_test.go`
asserts it under the plain `go test`

Spelled for this layout (`<scratch>` is the scratch CORE directory, per the applier's usage line; until train 49
lands, the applier is read from its branch):
```bash
S='<stage>'; A="$S/bin/apply-h5-c1-1-rederives.sh"; C="$S/h5/src/core"; P='<patch-ref>'; M='<mcleanup-ref>'
git -C '<tree>' show "$P:src/apply-h5-c1-1-rederives.sh" > "$A" || exit 3
( git -C '<tree>' archive "$M" -- src/core/runtime/mfinal.cs src/core/runtime/mcleanup.cs src/core/runtime/mcleanup.cs.auto | tar -xf - --strip-components=1 -C "$S/h5/src" ) || exit 3
bash "$A" --verify "$C" > "$S/logs/c1-verify-pre.log" 2>&1; echo "  pre-verify rc=$? (want 1, naming BOTH defects)"
bash "$A" "$C"          > "$S/logs/c1-apply.log" 2>&1;      rc=$?; echo "  apply rc=$rc (want 0)"; [ "$rc" = 0 ] || exit 6
bash "$A" --verify "$C" > "$S/logs/c1-verify-post.log" 2>&1; rc=$?; echo "  post-verify rc=$rc (want 0)"; [ "$rc" = 0 ] || exit 6
```
The applier's own arms are the step's controls; this runbook cites them rather than restating them
(`--self-test`, `--verify`, apply; exit 0 / 1 / 2). ⚠ **Its apply mode REFUSES (rc 2, "H5c has not run") while
`runtime/internal/sys` exists as a directory, and step 5 LEAVES that directory.** On the fifth rehearsal's real
post-H5c root it still held two `.csproj`, `README.md`, two icons and three test `.cs` files. That refusal is
therefore the EXPECTED outcome of this step as written, until C1 or COORD rules the precondition or the residue.
STOP and post it to COORD and C1; never remove the residue by hand. `--self-test` uses `python3`. i9's reproduction of lane R's fifth rehearsal
§1–§2 ends here: the applier `--verify`, apply, `--verify`, then the `runtime` build — the first rung of the H5
executor.
<!-- COORD 9da4d9f9a §2 (the step wording, verbatim), COORD 5123a14a2 §2 (the patch ruled; the carry hazard, verbatim from C1). Patch:
     branch claude/c1-h5-rederive-patch at ded03d469 — record docs/phase4/PATCH-h5-c1-1-runtime-rederives.md, applier
     src/apply-h5-c1-1-rederives.sh (usage :15-19; precondition :59-70; --verify :266-269; self-test python3 arms 7-8), train 49, not
     landed. <patch-ref> = ded03d469 or its landed successor. <mcleanup-ref> = claude/c1-mcleanup-handown (23d07f742 on 2026-09-13; its
     src/core/runtime changes are mfinal.cs, mcleanup.cs, mcleanup.cs.auto; mfinal.cs:197 reads GoFinalizerQueue.EnsureRunner()) or
     its successor. `git archive` writes blobs with working-tree conversion, as a checkout would. Readings on lane R's box, fifth
     rehearsal: 120 unique sites, ASM 194-188-188, all in runtime2.cs and mfinal.cs. MEASURED 2026-09-13 on lane R's box, on the fifth
     rehearsal's post-H5c scratch (101 applied): the root still had runtime/internal/sys holding runtime.internal.sys.csproj,
     runtime.internal.sys.tests.csproj, README.md, go2cs.ico, go2cs.png, go2cs_test_host.cs, intrinsics_test.cs, package_test_info.cs.
     `--verify` on that root returned rc 1, with 9 FAIL lines covering both defects and both carry-hazard symptoms. Apply on a copy of
     the real runtime2.cs, mfinal.cs, note_other.cs and both sys directories returned rc 2, REFUSE "runtime/internal/sys is STILL
     PRESENT -- H5c has not run", and left the files byte-unchanged (cmp). -->

#### Amendment 2026-09-13 — the overlay comparand, the overlay, and `go generate`

**1. The comparand** (read-only, both roots from one binary — `cmp <stage>/h4a.exe.sha256 <stage>/h5.exe.sha256`
first). **`d-level` is taken immediately after both conversions. `d-hop` is taken after H5c's step 5 removal and
the C1 patch, and before the overlay** — taken earlier it cannot show a deletion, because the stale seed sits
identically in both roots:
```bash
cd '<stage>' && cmp h4a.exe.sha256 h5.exe.sha256 || exit 4
diff -rq --strip-trailing-cr -x bin -x obj -x Generated -x '*.cs.auto' '<landing-tree>/src/core' h4a/src/core > logs/d-level.txt
diff -rq -x bin -x obj -x Generated -x '*.cs.auto' h4a/src/core h5/src/core > logs/d-hop.txt
```
`<landing-tree>` is a clean worktree of `<landing>`. `d-level` (master → H4a) is the queued levelling noise: each
row matched to a CleanupBacklog item, an unbanked-intended-drift row or a BOARD born-stale entry, else posted
UNCLASSIFIED. `d-hop` (H4a → H5) is the upstream delta: every row through §4, T0 to T5; H5c's removals read as
`Only in h4a` rows. A path in both is classified in `d-hop`, its `d-level` part named.
<!-- COORD db6d9462f §3.2 (c): the queued-levelling noise is what differs between master and the H4a root, the upstream delta what differs
     between that root and H5. Seeded from one tree with identical writes skipped (a02ac3df3:src/go2cs/platformEmit.go:420-422). The
     checkout's line endings are not the converter's, hence the stripped CRs on the master side only. -->

**2. The overlay** — after the H5c amendment's steps 5 and 6, and after the H8 census (the H8 amendment reads a
clean `<tree>`) — `.cs`, `.csproj` and `README.md`, never `*.cs.auto`, a straight copy, plus every emitted
`<EmbeddedResource Include>` payload, pinned `-text` and proven by a re-checkout:
```bash
( cd '<stage>/h5/src' && find core \( -name '*.cs' -o -name '*.csproj' -o -name README.md \) \
    -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Generated/*' ) > '<stage>/logs/overlay-set.txt'
( cd '<stage>/h5/src' && tar -cf - -T '<stage>/logs/overlay-set.txt' ) | ( cd '<tree>/src' && tar -xf - ); echo "rc=$?"
git -C '<tree>' status --porcelain > '<stage>/logs/overlay-status.txt'
grep -c '^ D' '<stage>/logs/overlay-status.txt'   # 0: a copy deletes nothing (floor 8); grep -c exits 1 on a zero count
```
The `find` above does not match the payloads: they are appended to `overlay-set.txt` before the `tar`, derived from
the emission. A refused payload goes through repoguard's csproj-keyed admit only. After the copy, no README has lost a
badge line, and every merged L3 csproj round-trips (close lessons 2, 4-6).
*(Edited in place 2026-10-01, §7 lesson 51: the payload clause and this paragraph were added. The close met these
first, and only by derivation; H5 meets them first.)*
<!-- The first sentence of this paragraph is not in the draft: it was added at the apply (2026-10-02), because the code
     block above copies .cs, .csproj and README.md only, and close lesson 2 (H10's close amendment) widens the overlay
     "by every file an emitted csproj names in <EmbeddedResource Include>, derived from the emission". -->

Then the marker census re-measured in `<tree>`, T0 phantoms restored per §4, and staging by explicit path —
never `git add -A` (floor 8). `<tree>` is a worktree ON the version branch COORD names (checked out at `<H2>`, not
detached, before the overlay); commits are signed; announce, then push (floor 9).

**3. STOP — the overlay cannot carry a deletion.** A file H5c removed from the staging root is still tracked
in `src/core`. Compute and POST the set; commit nothing from it until COORD rules how it lands:
```bash
( cd '<tree>' && git -c core.quotePath=false ls-files -- 'src/core/*.cs' 'src/core/*.csproj' 'src/core/*README.md' ) | sed 's#^src/##' | LC_ALL=C sort > '<stage>/logs/tracked.txt'
LC_ALL=C sort '<stage>/logs/overlay-set.txt' > '<stage>/logs/staged.txt'
LC_ALL=C comm -23 '<stage>/logs/tracked.txt' '<stage>/logs/staged.txt' > '<stage>/logs/absent-in-stage.txt'
```
Its want: exactly the step-5 delete set (prefixed `core/`), plus whatever the ruling removes. **`go2cs-stdlib.slnx`**
is adopted from `<stage>/h5/src` verbatim, and only after that ruling: the generator lists every `.csproj` on disk,
so while the residue stands it lists the residue.

Exception: a flat file the emitter moved to per-GOOS folders is `git rm`'d by name once all three targets carry their
copy (close lesson 3).
*(Edited in place 2026-10-01, §7 lesson 51: an exception to this step's STOP. The close met it first, and only by
derivation.)*
<!-- The overlay rule is this section's bullet above ("Overlay .cs, .csproj and README.md, excluding *.cs.auto"); a straight copy:
     corpus-reconvert SKILL.md:111. The slnx: src/go2cs/solutionGenerator.go:39-44 (adopted verbatim) and :140-175 (every .csproj on disk);
     never line-filtered: 826045a74:docs/phase4/h5-removals.txt:14-17 (28 MSB4025, zero assemblies). Version branch: this document §3.5
     (a long-lived version branch, unnamed at a02ac3df3). -->

**4. `go generate .`** — `stdlib-metadata.txt` is generated from every `package_info.cs` under `src/core` and gated
by `TestStdLibMetadataInSync`. A no-write PREDICTION is available as soon as the H5 root exists, under its pin:
```bash
pin go1.24.13 '<GOROOT-1.24.13>' '<GOROOT-1.24.13-posix>' || exit 3
( cd '<tree>/src/go2cs' && go run ./internal/genstdlibmeta '<stage>/h5/src/core' '<stage>/logs/stdlib-metadata.predicted.txt' ) || exit 6
diff '<tree>/src/go2cs/stdlib-metadata.txt' '<stage>/logs/stdlib-metadata.predicted.txt' > '<stage>/logs/stdlib-metadata.predicted.diff'
```
The real run comes AFTER the last commit of the series that moves a `package_info.cs` — the removal's ruling
applied, the hand-own branch merged, any seat re-minting metadata — because every leftover `package_info.cs`
contributes records:
```bash
pin go1.24.13 '<GOROOT-1.24.13>' '<GOROOT-1.24.13-posix>' || exit 3
( cd '<tree>/src/go2cs' && go generate . ); rc=$?; [ "$rc" = 0 ] || exit 6
git -C '<tree>' status --porcelain -- src/go2cs src/core/go2cs     # want: only src/go2cs/stdlib-metadata.txt
( cd '<tree>/src/go2cs' && go test -count=1 ./... ) > "<stage>/logs/converter-test-$(date +%Y%m%d-%H%M%S).log" 2>&1; echo "rc=$?"
```
Commit the regenerated file with the change that moved it; re-run if a later commit moves a `package_info.cs`.
<!-- Named in this document's H4a bundle (item 3); placed after the overlay for this hop by COORD db6d9462f §3.3; .claude/rules/corpus.md:197-198.
     Directives at a02ac3df3: src/go2cs/stdlibMetadata.go:19 (genstdlibmeta) and src/go2cs/symbols.go:19 (gensymbols, a pure function of
     symbols.json, internal/gensymbols/main.go:20-24, writing ../core/go2cs/Symbols.cs at :62-64 — hence src/core/go2cs in the status
     pathspec; any other file in that status is posted). Root and output arguments: internal/genstdlibmeta/main.go:41-53. -->

#### Amendment 2026-09-13 — where the series' artifacts go, and what each step posts

- **Layout.** `<stage>/bin` (the binary, its `sha256`, the C1 applier copy), `<stage>/h4a`, `<stage>/h5`,
  `<stage>/<R>-stage`, `<stage>/<R>.run.stamp`, `<stage>/<R>.exe.sha256`, `<stage>/logs`. Never a session scratch
  directory, never another lane's root.
- **Logs.** One per step per run, a full date-time in the name, the whole stream (`> log 2>&1`, never `| tail`,
  floor 7). Every leg longer than a few minutes runs from a per-run COPY of its script (floor 4).
- **Manifest.** After each step, over exactly what is shipped:
  `( cd '<stage>' && { find bin logs -type f; find h4a h5 -type f -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Generated/*'; } | LC_ALL=C sort | xargs -d '\n' sha256sum ) > '<stage>/MANIFEST.sha256'`.
  The roots and the logs go to i9's share as mapped from the i7 (COORD `220cffc7b`, "Fleet shares") under a dated
  folder with that manifest; the post names the folder and the manifest's hash, and COORD pulls. Never a committed binary.
- **Each post:** the tree SHA and tree hash; `go version <exe>`'s token and the exe's sha256; each pin half's bare
  `go version` line and the directory name it was taken from; the command line with placeholders; exit code and
  wall; the count lines verbatim; every prediction quoted as worded beside its reading; the box's load; the
  manifest hash. Announce before pushing.
- **The record.** On the version branch, the counts ride in the message of the commit that carries the step; the
  logs on the share. H4a commits nothing: its counts live in its post and the share manifest. Where the real
  series' readings are recorded as a `docs/phase4/` file is not ruled — ask COORD before the first commit.
<!-- Share and manifest: COORD 220cffc7b, the first entry of the new mailbox file after rotation 5e70540f4, "Fleet shares" (the rotation
     commit itself does not carry it). Inputs banked with the claim: this document §3.4. Post contents: KICKOFF-fleet.md i9 prompt at
     a02ac3df3 ("Post each reading with its tree SHA, configuration and load"); no GOROOT value on a pushed surface:
     .claude/rules/docs-records.md. Per-run copies: lane R's r-h5b-chain-213703.sh:2-3. Lane R's own runs wrote logs and sentinels into a
     session directory (r-h5b-convert.sh:8-17, setup-convert.sh:9,55-56), the loss this block prevents. -->


### H6 — The hand-own re-audit ⟲ **GATE**

The step that distinguishes a corpus *upgrade* from a corpus *regeneration*, and the one a migration
is most likely to skip **because everything compiles without it**.

> **Instrument: [`src/handown-census.ps1`](../src/handown-census.ps1)** (runway dispatch,
> 2026-08-24) — the differential CENSUS half of this gate, so the review starts from a list instead
> of from everything. For every `[module: GoManualConversion]`-marked file (re-measured each run,
> line-anchored, whole-file) it maps the upstream Go source the hand-own replaces and classifies it
> across `-FromGoRoot`/`-ToGoRoot`: **untouched** / **touched-trivial** (comment-and-whitespace
> only — Go `//go:` directives count as CODE, not comments) / **touched-substantive** (the review
> list) / **no-upstream-counterpart** (hand-additions; reviewed via their principal). Read-only,
> self-verifying (classes must sum to the marker census), and conservative in one direction only:
> every stripper bailout classifies substantive, because over-reporting sends a human to look.
> **What it does NOT do: the judgment.** Every substantive row still gets the human review below —
> the instrument decides where H6 looks, never what H6 concludes.
>
> **The shape to expect: the review list is a small fraction of the census.** Its first execution
> reduced a census of dozens of marked files to a **single-digit** substantive set, with the rest
> split between untouched and no-upstream-counterpart. That ratio is the instrument's whole value and
> it is also the reason to distrust a run that does not show it — a substantive class near the census
> size means a stripper bailing out, not upstream churn. The figures of any one execution are that
> migration's record, not this document's: the first run's are in
> [`phase4/RECON-go12312-diff.md`](phase4/RECON-go12312-diff.md), where each substantive row was
> independently cross-checked against the upstream package table.

**The failure mode.** A hand-owned file is frozen at the semantics of the release it was written
against. When upstream **adds** code inside that file — a new branch, a new field, a hardening fix —
the hand-own does not receive it. Nothing fails: the file is excluded from the convert set, the corpus
compiles, the suites are green, and the package's own tests may not cover the added path. **The defect
is silent and operational, and it surfaces later as an inexplicable divergence in a package nobody was
working on.** *Newly-added* upstream code is the dangerous class; a *changed* line often shows up as a
behavioral divergence, an added branch shows up as nothing.

> **AMENDED 2026-09-16 — H6's census ENUMERATES SURVIVORS, so a hand-own that was DELETED WITH ITS
> PACKAGE leaves no row, and the class it cured returns uncured in whatever replaced it.** The failure
> mode above is upstream *adding* code inside a hand-own that cannot receive it. This is its sibling and
> it is quieter: the hand-own does not go stale, it **ceases to exist**, and with it the only statement
> of why the automatic conversion of that file was unsafe.
>
> **Measured, not argued** (`src/handown-census.ps1:122`): the marker census is
> `git grep -l -E '^\s*\[module:\s*(go\.)?GoManualConversion\]' -- '*.cs'` **over the working tree** —
> the corpus as it is now. A file already removed is not in `$marked`, so it is not misclassified, it is
> **absent from the census entirely**. The instrument's own `from`/`to` existence test at `:144-150`
> handles the *upstream Go source* appearing or vanishing and correctly calls that `touched-substantive`
> — "always a human look" — but that test presumes the hand-own still exists to be mapped. Nothing in
> the gate looks at the set that is gone.
>
> **The instance that produced this amendment** (go1.23.12 → go1.24.13, `docs/phase4` q97):
> `src/core/vendor/golang.org/x/crypto/sha3/` held a hand-owned `xor.cs` whose comment names its own
> failure mode verbatim — the raw-address reinterpret "fabricated an `array<byte>` backing reference out
> of the keccak state's own DATA" — and cured it with `MemoryMarshal.AsBytes` over the array's span, a
> genuine aliasing view. At the new pin that package is **15 files → 0**: upstream moved the
> implementation to `crypto/internal/fips140/sha3`, the hand-own was dropped with the package it lived
> in, and the replacement re-emits the same length-changing raw-address reinterpret at
> `keccakf.cs:61` — with **no `GoManualConversion` marker anywhere in the new package**. It surfaced as
> four test panics carrying *negative* lengths out of `golib/array.cs`'s bounds check, which is the
> signature of a fabricated `array<T>` struct rather than an out-of-range index. H6 passed. The corpus
> compiled. The cure had simply been deleted.
>
> **The step, and it is mechanical.** At H6, before any substantive-row review, compute the
> **retired-hand-own set** — markers present in the OUTGOING corpus and absent from the incoming one.
>
> ⚠ **`<outgoing-sha>` is the MERGE-BASE of the release branch and master, never master's tip.** A hop
> runs on a long-lived release branch while master keeps moving; a hand-own that landed on master *after*
> the branches diverged was never on the release branch to be retired from it, and a baseline taken at
> master's tip reports it as retired. Corrected in review by a second lane, who re-ran the ancestry test
> on the first run's rows: six of seven survived, and the one that did not had landed on master after the
> divergence. **Assert it per row** —
> `git merge-base --is-ancestor <the-commit-that-added-it> <incoming-sha>` — because that assertion is what catches the class, not the choice of baseline alone.
>
> ⚠ **And a row that fails that assertion is not discarded — it is RE-ROUTED.** It is not a hop
> retirement; it is a **carry-forward gap**: a cure that exists on master and has never reached the
> release branch. That is a different finding and can be a worse one. The instance here: the excluded
> row's cure keeps a runtime test off a fatal path, the release branch carries the seat that makes that
> path an uninterceptable process exit, and the host kill it prevents was measured at 57 verdicts. **The
> false positive was worth more than the row it displaced**, so the step files these rather than dropping
> them, and the hop's own carry-forward census owns them.
>
> ```
> git grep -l -E '^\s*\[module:\s*(go\.)?GoManualConversion\]' <outgoing-sha> -- 'src/core/**/*.cs' | sed "s#^<outgoing-sha>:##" | sort > logs/handowns-outgoing.txt
> git grep -l -E '^\s*\[module:\s*(go\.)?GoManualConversion\]' <incoming-sha> -- 'src/core/**/*.cs' | sed "s#^<incoming-sha>:##" | sort > logs/handowns-incoming.txt
> LC_ALL=C comm -23 logs/handowns-outgoing.txt logs/handowns-incoming.txt > logs/handowns-retired.txt
> ```
>
> where `<outgoing-sha>` is `git merge-base <release-branch> master`.
>
> **One row per retired hand-own, and the row is not closed by the deletion being correct.** For each:
>
> | field | what it must carry |
> |---|---|
> | the hand-own | path at the outgoing pin, and the *class it cured* in its own words — a hand-own that cannot say what it was for is itself the finding |
> | why it went | package dropped · file renamed · upstream absorbed the fix · the hand-own's reason expired |
> | the replacement | the package that now carries those semantics at the new pin, **named**, or `none` with the reason |
> | the class, re-censused | the cured class's predicate run at the NEW pin over the replacement — **cured / moved / gone**, with the count |
> | the marker | whether the replacement carries a hand-own of its own, measured, not assumed |
>
> **The verdict a row needs is `cured` or `gone`.** `moved` — the class present at the new pin in a file
> with no hand-own — is an H6 **GATE FAILURE**, exactly as a substantive row left unreviewed is, and it
> is cured before H6 closes.
>
> ⚠ **Do not read the retired set as a delete list to approve.** Every one of these deletions was
> *correct*: the package really did go away. The question H6 asks is not whether the file should have
> been removed — it is **whether the reason it existed went with it**.
>
> *Named blind spot:* the set is keyed on the marker, so a hand-own retired by having its marker removed
> while the file stays reads as present. That is the whole-file-replacement class the marker exists to
> mark, and losing the marker is already an H6 substantive row by the census's own arithmetic — stated
> here so the two are not confused.
>
> *Durable form, after the hop:* a guard in `src/go2cs/internal/repoguard` carrying the previous pin's
> marker list and asserting that every absent entry has a closed row. Banked, not cut during a
> migration — the list's baseline moves at every hop and a guard whose baseline moves needs the hop to
> be over before it can be written honestly.
> The 1.24.13 hop is over, so the guard is owed before the next hop's H6. It is absent at 172d437e66, and its
> baseline is that hop's `logs/handowns-outgoing.txt`.
> *(Edited in place 2026-10-01, §7 lesson 60: missing the comm step let sha3's fabricated `array<T>` class return
> while H6 passed.)*

**The instrument** is `.cs.auto` — the converter's answer to *"what would the automatic conversion of
this file be, today, from this Go tree?"*

**The diff is `.auto`(old release) vs `.auto`(new release), per hand-own — never `.auto` against the
hand-owned `.cs`.** The latter is dominated by the hand-own's *intended* divergence and is unreadable;
the former isolates the upstream delta. **Both `.auto` files must be produced by the SAME converter
binary**, or converter drift contaminates the release axis and the classification is worthless. Each
staging root carries its **own** `version.props` pinned to its own release, so the pin guard passes on
both sides.

*Named blind spot:* if the new converter build cannot parse the OLD tree cleanly, run A degrades and
the baseline is suspect. **Assert run A's package count and marker gate against the outgoing corpus's
before trusting it.**

**Classification — every delta, explicitly, one of three:**

| Class | Meaning | Required record |
|:--|:--|:--|
| **(a) ABSORBED** | the upstream change is real and has been carried into the hand-own | the commit that carried it; a test or gate that observes it |
| **(b) N/A** | the upstream change does not apply to the managed implementation | **the reason, written out** — never the bare letters |
| **(c) REWRITE OWED** | the hand-own must change and has not yet | a named work item, gating the migration or explicitly deferred with owner and reason |

An **empty** diff still gets a record (`unchanged`, with both hashes). A hand-own the run emitted
**no** `.auto` for gets a record too, and that record is **a defect in the audit, not a pass**: either
the seed did not take at that path, or the marker predicate could not see the marker.

⚠ **Two populations, and conflating them makes the gate either a false alarm or a rubber stamp**
(ruled): the **audit** covers *all* hand-owns; the **`.auto` differential** reaches only the ones the
converter re-emits. A supplemental `*_impl.cs` companion has no Go counterpart and therefore no
`.auto` — it is audited **against its principal's `.auto` diff**. A hand-owned *package* is audited by
**manual upstream diff**. **Every record names its evidence class.**

**The completeness gate:**

> No migration's corpus is adopted until every hand-own in the **re-measured** census has a classified
> delta record in that migration's audit file, and every (c) is either closed or explicitly deferred
> with an owner.

Mechanically — the instrument is `src/check-h6-completeness.ps1` (it dot-sources `handown-census.ps1`'s predicate,
refuses a vacuous run with rc 2, exits 1 on any violation); run it at H6 AND before the release, and row each
hand-own created during the hop AS IT LANDS: re-measure the line-anchored census over `src/core`; assert every marked
path appears
exactly once in the audit file; assert every row's class is one of `unchanged`/`a`/`b`/`c`; assert
every `b` carries a non-empty reason and every `c` a work-item reference; assert **zero** rows in the
"no `.auto` emitted" state. Exit non-zero on any violation — the same shape as the repository's other
preflights: cheap, by-path, and impossible to pass vacuously.

Deliverable: one audit file per migration under `docs/phase4/` (ruled), rather than per-package notes,
because the completeness gate must be checkable in one place.
*(Edited in place 2026-09-26, §7 fix A5 and lesson 20: the instrument is named; at the 1.24.13 pre-release gate 21
hop-time hand-owns had no row.)*

**Four arms beyond the `.auto` diff.** (1) STRANDED: a principal that adds a bodyless declaration to a package
with a companion — the q82 census lists them. (2) A hand-owned host that quotes Go text is re-diffed at the new
pin. (3) Each row's observer is re-derived at the pin. (4) A hand-own deleted with its package owes a re-census,
from the merge-base, of the class it cured. A relocated hand-own whose principal changed is re-derived BEFORE the
H5 gate reads green.
*(Edited in place 2026-09-26, §7 lessons 21-22: the arms H6 missed at the 1.24.13 hop, two of which killed test
hosts.)*

#### Amendment 2026-09-13 — for the 1.23 → 1.24 hop, where the `.auto` pair comes from

- **Old side** `<stage>/h4a/src/core/**/*.cs.auto`; **new side** `<stage>/h5/src/core/**/*.cs.auto` — one binary,
  `cmp <stage>/h4a.exe.sha256 <stage>/h5.exe.sha256` passing, each root with its own `version.props`.
- **Population:** the marked paths by the census instrument's own predicate, `go.`-qualified spelling included —
  never a literal grep, which undercounts.
- **Run A's package count** is asserted against the outgoing corpus's in the H4a baseline amendment (step 4).
- **The differential**, one row per marked path, into `<stage>/logs/h6-auto-pair.txt`:
  ```bash
  S='<stage>'; while IFS= read -r p; do q=${p#src/}
    a="$S/h4a/src/$q.auto"; b="$S/h5/src/$q.auto"
    if [ ! -f "$a" ] || [ ! -f "$b" ]; then s=MISSING-AUTO; elif cmp -s "$a" "$b"; then s=IDENTICAL; else s=CHANGED; fi
    na=old; [ -f "$a" ] && [ "$a" -nt "$S/h4a.run.stamp" ] && na=new; nb=old; [ -f "$b" ] && [ "$b" -nt "$S/h5.run.stamp" ] && nb=new
    printf '%s\t%s\th4a-auto=%s\th5-auto=%s\n' "$p" "$s" "$na" "$nb"
  done < "$S/logs/h5-marked.txt" > "$S/logs/h6-auto-pair.txt"
  ```
  and the census: `powershell -NoProfile -ExecutionPolicy Bypass -File '<tree>/src/handown-census.ps1'
  -FromGoRoot '<GOROOT-1.23.12>' -ToGoRoot '<GOROOT-1.24.13>' > '<stage>/logs/h6-census-<date-time>.log' 2>&1`
  (on the train-47 union the script takes exactly those two parameters plus `-ListUntouched`).
- **Undetermined is not unchanged.** A seeded root cannot tell an `.auto` the run rewrote to identical bytes from
  one it never wrote: identical writes are skipped at the merge. A row `IDENTICAL` with `old` on both sides is
  recorded `no .auto emitted — undetermined`, which the completeness gate counts as a defect, until a
  discriminator is ruled. A candidate discriminator: the sibling writer creates each `.cs.auto` unconditionally in
  the per-target STAGE root, so a copy under `<stage>/<R>-stage/<goos>-amd64` newer than the sentinel may say it
  was emitted — NOT MEASURED, and not a ruling.
- **The completeness gate has no instrument** on `a02ac3df3` or the train-47 union: STOP before scoring it; post
  the differential and the census; COORD rules the gate script and its exit condition.
- **The relocation blind spot.** A frozen hand-own present in both trees passes any set comparison while carrying
  declarations the target release moved (`runtime/internal/sys` → `internal/runtime/sys`). The package-alias
  census (`docs/phase4/CENSUS-h6-handown-package-aliases.md`) is read beside every substantive row.
- **The audit file** is `docs/phase4/AUDIT-h6-handown-go124.md`: one row per marked path. R cut the skeleton
  (`d18059950`, train 48 seat 4) and G filled it (blocks 1–19, landed on master at `4e672aa4a`, 144 of 145 rows
  classified, row 130 recorded REWRITE OWED (c)); it reaches the version branch with master at H12.
<!-- The pair: COORD db6d9462f §3.2 (b). Population: 146 by handown-census.ps1's predicate against 105 by a literal grep at bd1d26faf
     (db6d9462f §1, H6 row). Identical writes skipped: src/go2cs/platformEmit.go:420-422 at a02ac3df3. Unconditional sibling create:
     a02ac3df3:src/go2cs/autoSiblingOperations.go:122-137 (os.Create). Census parameters: 44fbc381a:src/handown-census.ps1:36-39. No gate
     instrument: git grep for AUDIT-h6 / completeness gate over src/*.ps1, src/*.sh, src/go2cs/*.go at 44fbc381a returns nothing
     (accuracy verifier). Relocation class: REHEARSAL-h5-go124.md §6 (runtime2.cs and mfinal.cs missed) and lane R's
     r-ladder-preflight.sh:77-86 (a set arm cannot see it); the alias census is G's, train-47 seat 7 at 898cbfefe. Skeleton: R 910f2a151. -->


### H7 — Compile parity **GATE**

Full `go2cs-stdlib.slnx` build with shared compilation disabled, zero errors, **skipped-dependents
enumerated and zero** (a dependent of a failed project is skipped, not errored — count them). Run
**every** buildable `$(GoTargetOS)` flavor, purging `bin`/`obj`/`Generated` between switches.

**Gate: 100 % of the migration's package set compiles.** Not "as many as before" — 100 %, per the
frame.

#### Amendment 2026-09-13 — the per-flavour build, and the scoring method for the ladder's sites

The gate reads the version-branch tree after the H5 series (`<build root>` = `<tree>`). The same procedure over
`<stage>/h5/src` (`<build root>` = `<stage>/h5`) is the LADDER — the gate's rehearsal, labelled as one. **The
ladder runs only after H5c's step 5 removal and step 6's C1 patch**: built before the removal it compiles the
stale seed siblings (CS0102), and before the patch it cannot get past `runtime`. The ladder is done when it reads
**zero unique sites on all three flavours with own assemblies at the corpus's order: projects minus the
platform-exclusive set, per flavour.** One flavour per invocation, serially, from a per-run copy of the script; a
box whose default .NET SDK lags the corpus TFM exports the .NET pair first (H10 step 1).
```bash
export MSYS_NO_PATHCONV=1 MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
FL=${1:?flavour}; SRC='<build root>/src'; LOG="<stage>/logs/build-$FL-$(date +%Y%m%d-%H%M%S).log"
[ -f "$SRC/core/golib/golib.csproj" ] && [ -f "$SRC/go2cs-stdlib.slnx" ] || { echo "ABORT: $SRC is not a go2cs src root"; exit 3; }
P=$(find "$SRC" -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
find "$SRC" -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
left=$(find "$SRC" -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
echo "  purged $P  remaining $left"; [ "$left" = 0 ] || { echo "ABORT: purge incomplete ($left)"; exit 3; }
START=$(date +%s)
dotnet build "$SRC/go2cs-stdlib.slnx" -c Debug -p:GoTargetOS="$FL" --no-incremental -m -p:UseSharedCompilation=false > "$LOG" 2>&1
rc=$?
echo "  exit $rc after $(( $(date +%s) - START ))s  NULs $(head -c 200 "$LOG" | tr -d -c '\000' | wc -c)"
echo "  CS occurrences $(grep -aoE 'error CS[0-9]+' "$LOG" | wc -l)  MSB/NETSDK occurrences $(grep -aoE 'error (MSB|NETSDK)[0-9]+' "$LOG" | wc -l)"
grep -aoE '[A-Za-z0-9_./\\-]+[.]cs[(][0-9]+,[0-9]+[)]: error CS[0-9]+' "$LOG" | sed 's#\\#/#g; s#.*/core/#core/#' | LC_ALL=C sort -u > "$LOG.sites"
echo "  unique sites $(wc -l < "$LOG.sites")  ROOTS $(grep -vc Generated "$LOG.sites")  CASCADE $(grep -c Generated "$LOG.sites")"
built=0; unbuilt=0; : > "$LOG.unbuilt"
while IFS= read -r p; do
  d=$(dirname "$p"); a=$(grep -aoE '<AssemblyName>[^<]+' "$p" | head -n1 | sed 's/<AssemblyName>//'); [ -n "$a" ] || a=$(basename "$p" .csproj)
  if ls "$d"/bin/Debug/*/"$a".dll > /dev/null 2>&1; then built=$((built+1)); else unbuilt=$((unbuilt+1)); echo "${p#*src/core/}" >> "$LOG.unbuilt"; fi
done < <(find "$SRC/core" -name '*.csproj' ! -name '*.tests.csproj')
echo "  ASM (own assemblies) $built  projects $((built+unbuilt))  none $unbuilt"
```
(`grep -c` exits 1 on a zero count; these lines print, they do not gate.) All counts are read before the next
flavour's purge; on the version-branch tree, `git status --porcelain | grep '^ D'` prints nothing after every purge
(floor 8). **Per flavour the gate is:** exit 0; CS and MSB/NETSDK occurrences 0; every project in `$LOG.unbuilt`
is platform-exclusive to another flavour — its package absent from `GOOS=<flavour> go list std` under `pin
go1.24.13 … || exit 3`, run from a no-module directory.

**Scoring rules.**
- **Units never mix.** Per flavour: CS occurrences (they move with console verbosity and with every referencing
  project), unique sites (`file(line,col): error CSnnnn`, keyed after `core/`), ROOTS (unique sites outside
  `Generated`), CASCADE (unique sites inside `Generated`), and ASM (own assemblies, the gate's arithmetic). A
  comparison names both units and both trees; a step between readings that moved more than one axis is not
  attributed to one of them. A dll-file count with copies included is not ASM.
- **Every unique site is assigned to one owned class** — leftover seed (H5c), frozen metadata, converter emission,
  hand-own, generator cascade — with its owner; a CASCADE site takes the owner of a ROOT in the same project, and
  a site in no class is the finding.
- **Predictions are posted before the build and scored as worded**, the wording quoted beside the reading; the
  predictions of record are those owed in `REHEARSAL-h5-go124.md` §15 §8, each with its premise re-read at the
  tree first (a prediction whose premise is absent is posted VOID with the reading that voids it, never scored).
  A falsifier stated in advance is scored even when the mechanism it tested survives.
<!-- Lane R's per-flavour leg: r-h5b-build.sh:4-25 and the fifth rehearsal's build script (2026-09-13: CS, MSB/NETSDK, ASM, ROOTS,
     CASCADE, UNIQUE keyed after core/), genericized; per-leg capture (the lost darwin-only ASM, REHEARSAL-h5-go124.md §15 §5) and the
     per-run chain r-h5b-chain-213703.sh. No incremental build across GoTargetOS: .github/workflows/os-matrix.yml:340-343 at a02ac3df3.
     Own assemblies with the TFM folder derived, never spelled: os-matrix.yml:389-417 (408-410); platform-exclusive: :436-438. R's ASM
     counter spelled the TFM and counted copies (~3000 against C1's 306 of 306, e8d90a664 §1). Done criterion: COORD db6d9462f §3.1
     ("ZERO unique sites on three flavours with ASM at the corpus's order"). CS0102 on a stale seed: a02ac3df3:docs/GoCorpusMigration.md:429-433.
     The site key read 120 unique sites on lane R's real windows build log (executability verifier, 2026-09-13), the number R's run
     found. Units: REHEARSAL-h5-go124.md §15 §2 and §15 §6 (UNITS); owned classes §15 §7; predictions owed §15 §8. Which tree: COORD
     db6d9462f §3 and §3.1. The purge guard: accuracy verifier (a mis-substituted SRC deletes every build dir under it). -->


#### Amendment 2026-09-16 — the per-flavour gate as executed at the 1.23 → 1.24 hop: arm 4 is inert where every project builds; the build root's spelling; arms gate on their command's rc

- **Arm 4 is inert on this corpus.** Every csproj under `src/core` builds regardless of `-p:GoTargetOS`, so
  `$LOG.unbuilt` is EMPTY on every flavour and "every unbuilt project is platform-exclusive to another flavour"
  is MET over zero items — vacuous, never evidence. Measured at the 1.24.13 hop's compile-parity tree on all
  three flavours: unbuilt 0, ASM 343 of 343 (windows read on two boxes, linux and darwin on one). The arms that
  carry a per-flavour reading are: exit 0; CS occurrences 0; MSB/NETSDK occurrences 0; unique sites 0; and
  ASM = projects. Platform exclusivity is a property of the EMISSION's per-GOOS file sets, read at H8's census,
  not of projects.
- **Minus golib.** `golib` is absent from every `go list std` set and must nonetheless be BUILT; any arm keyed on
  "absent from `go list std`" passes it as legitimately unbuilt. Where arm 4's derivation is still used (e.g. to
  size an expected unbuilt set), subtract golib first — and expect the arithmetic NOT to predict unbuiltness on a
  corpus where every project builds: the linux prediction of 338 missed against a measured 343 for exactly this
  reason.
- **Spell `<build root>` as a drive-letter path (`C:/…`), never `/c/…`.** The script exports `MSYS_NO_PATHCONV=1`
  for dotnet's `-p:` arguments, so a POSIX spelling reaches native `dotnet` and `git` UNCONVERTED: `dotnet build`
  fails on a non-existent project path, and `git -C` dies with "cannot change to … No such file or directory".
  One launch was lost this way, and three added git arms printed PASS from `wc`/`grep` over a failed command.
- **Every arm gates on its command's own rc**, never on the shape of its output; the HEAD arm asserts a sha-shaped
  value, not merely non-empty. An arm that cannot distinguish "clean" from "the command died" is not an arm — and
  a guard's DESCRIPTION is not the guard: state the mechanism actually implemented, and run it on real input
  before quoting it.
- **ASM units.** The script's ASM counts own assemblies over core csproj excluding `*.tests.csproj` (343 at this
  hop = the census's core population). A "distinct produced assemblies" count from the build log reads one higher
  (344: the solution's one non-core member). Name both units; never reconcile a difference of one by feel.
<!-- Executed readings, 1.23 → 1.24 hop, 2026-09-16: all three flavours built with the 2026-09-13 script above from a
     per-run copy, SDK 10.0.400, at the tree 46307b4704. i9's linux reading 4df14c406: arm 4 inert (unbuilt 0, so the
     platform-exclusive arm is MET over zero items), and the three added git arms blind — they scored `wc`/`grep` over a
     command that had already failed. i9's darwin reading f464d9e58: every predicted arm met, and the "every arm gates on
     its command's rc" comment owned as unimplemented at the time it was written, then implemented. i9's linux prediction
     2c6aa2259: the golib arm, 338 predicted against 343 measured. The i7's windows second read at COORD fe23ab855 is the
     second of the two windows boxes. COORD's rung-7 close: a065b1bd9. -->


### H7a — The master fold **GATE**

**The ladder has no rung that folds master into the release branch, and the run rungs need one.** A hop
runs on a long-lived release branch while master keeps taking cuts. Everything from H8 onward *runs* the
corpus — re-emits it, rebanks goldens, re-derives the roster — and a run rung measures the tree it is
given. If master holds cures the branch has never carried, H8 onward measures a corpus that is wrong in
ways the migration did not introduce and cannot see.

**Ruled after an instance, not in advance.** At the go1.23.12 → go1.24.13 hop, master held 105 commits
the release branch had never carried. Among them: a managed `gcTestIsReachable` and the stop that keeps
the fatal path from ending the test host. The release branch carried the seat that makes `runtime.throw`
an uninterceptable process exit, and *not* the cure that keeps the runtime test off it — so the host kill
that had cost the runtime row 57 verdicts was live at the branch tip, silently, with the hand-own and its
converter registration absent **together** rather than as a dangling displacement that would have failed
loudly. It was found by a retired-hand-own census reporting a false positive (see H6), not by anything
looking for it.

**The step: ONE merge, master INTO the release branch, as the first act after the last compile-side seat
lands and before any run rung.** Never a rebase — the branch's SHAs are posted, and a hop's seats are
announced refs.

**Sized before it is taken, and the sizing is a post of its own:**

| | what it must carry |
|---|---|
| the pins | master, the branch tip, and `git merge-base` of the two — **all three re-read at the act**, because both move while the sizing is written |
| the range by path class | the master-only commits classified (docs and instruments / converter guards / converter production / corpus / repo config), merges counted separately because a merge carries its children's paths, and **no residue** — a commit matching no class is named, not dropped |
| what the branch LACKS | every hand-own and every emission-or-CLI change in the range, **named**, not counted. These are the fold's reason; the docs and guards are why the commit count is large and are not why the fold exists |
| the carry-forward set | marked files present on master and absent at the tip, each classified **relocated / gone / gap** — a relocation is verified by its counterpart's PATH, never by basename |
| the conflict set | `merge-tree --write-tree` at those two SHAs **with the ARGUMENT ORDER named**, by path, with the fingerprint stamped — the stamp differs by order while the conflicting paths do not, so the act is scored in the order the prediction stamped in |
| the prediction | what the merged tree must equal **and in which sense** (see *Scored on*), and the falsifiers |

**Scored on:**

- **the dry-run stamp, scored as what it is.** `merge-tree --write-tree` fingerprints the **inputs and
  the merge machinery**, not the commit:
  - a **conflict-free** fold — the merged tree is **byte-identical** to the stamp;
  - a **conflicted** fold — the stamp **cannot be equalled**: it carries markers for every conflicted
    path, and the landed tree differs from it by exactly the resolved paths. Score instead that the
    resolved paths are **exactly** the predicted set and that each resolution matches its ruled class;
    if a stampable figure is wanted, predict the **post-resolution** tree before the commit and score
    against that. ⚠ Scoring a conflicted fold against the dry-run SHA asks for a tree the act cannot
    produce, and a reader who takes it literally reads a **correct** fold as a miss;

<!-- AMENDED 2026-09-16 (COORD dff545e848, from the instance). This bullet read "the merged tree
     byte-identical to the stamped dry-run tree" with no conflict-free/conflicted split, and the sizing
     row above named no argument order. Both were found by lanes EXECUTING the rung, not by reading it.
       (1) THE OBJECT. At the go1.23.12 -> go1.24.13 fold the stamp was 393651af2d and the landed tree
           37dbd311bd -- differing by exactly the nine resolved paths, which is what an UNRESOLVED
           merge-tree fingerprint must do. i9 had to state that in advance (141464d05d) so a correct act
           would not read as a miss, and said so again at the landing (2834187aa): "the tree is NOT the
           stamped 393651af2d, exactly as the prediction said it could not be".
           CORRECTED 2026-09-16 (C2 22e01bf17; COORD): the two SHAs above first cited the HELD first take -- its tree
           dc02500f2e and its entry 550a276a8 ("COMMITTED LOCALLY AND GATED RED") -- an object no pushed ref reaches.
           The landed fold is fc275f1ac3 (entry 2834187aa) with tree 37dbd311bd, and the nine-path property was
           re-measured true of THAT tree (C2 1cfa9ee2a, 22e01bf17). The rule stood; only the SHAs were the superseded take's.
       (2) THE ORDER. C2 measured the stamp order-dependent (81543d8cd): version tip first and master
           second yields 393651af2d; master first yields d7958bb4da -- AT THE SAME NINE PATHS, compared
           by diff and not by eye. Two lanes following this rung literally could stamp different SHAs
           from identical inputs and read it as a disagreement about the fold. That failure mode is a
           FALSE MISS on a correct act, which stops a good fold -- worse than a missing check. The act
           was taken ours-first for this reason, with the orientation measured before the merge.
     Both are the same class as the emitted-file bullet corrected above and as H6's outgoing-pin defect
     before it: an UNDERSPECIFIED COMPARISON -- the step states an equals sign and does not fully
     specify both sides. No SHA was rewritten by either: the fold was never pushed. -->

- the conflict set exactly the predicted paths — **no path resolved that was not predicted**;
- ⚠ **silent subtraction, per symbol and in BOTH directions.** A fold crosses every seat the hop has
  landed. Each landed marker is asserted **by name at its count**; a clean merge rc says "no conflict",
  never "nothing dropped";
- each carry-forward gap present after, **with the whole of its cure** — a hand-own landed without its
  converter registration is a partial fold, and partial is how this class hides;
- relocated packages still at their NEW paths and the old paths **not resurrected** — a modify/delete
  resolved the wrong way re-creates a package the hop retired;
- any source root the range does not touch **byte-identical**; movement there is a MISS, not a bonus.

**Resolution by class, decided before the merge rather than at the conflict:**

- **regenerable metadata** (`package_info.cs`, `.csproj`) — **re-minted**, never hand-merged. These are
  artifacts; a hand merge of an artifact is a hand-written artifact;
- **projitems** — the **union**, and the row count asserted afterwards with no duplicates;
- **a modify/delete where the delete is the hop's own package retirement** — the **delete stands**;
- **a code conflict whose two sides are a displacement and the body it displaces** — **if the file is
  HAND-OWNED**, master's side, and the displacement's registration lands **with** it. **If the file is
  EMITTED, it is RE-MINTED from the merged converter**, exactly as the regenerable metadata above is:
  the merged tree carries the displacement's registration, so the re-mint emits the file *without* the
  displaced body and *with* the hop's own calling convention. ⚠ **An emitted file is never resolved by
  SIDE** — taking master's side reinstates master's **pre-hop emission of the whole file**, including
  every call written against a signature the hop re-signed.

<!-- AMENDED 2026-09-16 (COORD 1bc5eb919c, from the instance). At the go1.23.12 -> go1.24.13 fold this
     bullet read "master's side" with no hand-owned/emitted split, and the fold gated RED on it:
     src/core/runtime/mgc.cs is EMITTED and was in the conflict set, so master's side reinstated
     master's pre-hop emission -- four `lockInit(ref work.…, lockRank…)` calls at mgc.cs(177-180)
     against a signature the hop had re-signed to the box form `ж<mutex>`, giving CS1615 x4 in
     runtime.csproj. The branch carried 113 box-form call sites and ZERO ref-form; master carried 110
     ref-form, of which only these four entered, because only mgc.cs was conflicted (control: chan.cs,
     not taken from master, reads ref-form 0 / box-form 1 -- so the four are a property of the
     RESOLUTION, not of the merge). lockrank_off.cs was NOT conflicted, so the branch's DECLARATION
     auto-merged in correctly, which is why the mismatch surfaced as a call-site error rather than a
     redeclaration. The classes were also NOT DISJOINT: mgc.cs satisfied this bullet AND the
     regenerable-artifact bullet, and the list stated no precedence -- the split above supplies it.
     Measured and held unpushed by i9 (550a276a8); the rule is COORD's own, corrected by COORD at
     1bc5eb919c; R carried it into this text. No SHA was rewritten: the fold was never pushed. -->

**⚠ The closing check is the cheapest proof the fold did what it is for:** re-run H6's retired-hand-own
step. After the fold the merge-base *is* master's tip, so that step reads clean **by construction** — and
if it does not, the fold is incomplete and the rows it still reports are the gaps it failed to carry.

*Named blind spot:* the fold answers "what has master got that the branch has not". It says nothing about
the reverse, and nothing about a cure that exists in **neither** — a class that was broken before the hop
began stays broken and is H6's and H7's to find, not this step's.

*What this step is not:* a licence to take master's tip mid-hop whenever it is convenient. It is one merge per
planned boundary, sized and predicted, because a release branch that keeps re-merging master has stopped being a
release branch. **Plan two:** this fold, and the campaign-tip merge after H10's tooling (roster seat, plan,
shardmap, driver) lands on master. An EMITTED file in a fold's conflict set is re-minted from the merged
converter, never resolved by side; the fold is gated before the push.
*(Edited in place 2026-09-26, §7 fix A6 and lesson 25: this read "one merge at one boundary"; the 1.24.13 hop needed
the second fold to launch H10.)*


### H8 — Multi-platform re-emission **GATE** ⟲

Re-run the multi-target emission and the platform census, and diff the manifest against the outgoing
one. A migration changes the platform axis in **two** directions at once: new packages may be
platform-varying, and existing ones may stop being so. The per-GOOS package count is a measurement,
not a constant.

**Gate:** the platform manifest's marker gate is zero per target, and the default-flavor build
reproduces the single-target build byte-for-byte.

#### Amendment 2026-09-13 — for the 1.23 → 1.24 hop, H8's emission is H5's, and the manifest's comparand is owed

- The three-target emission IS the H5 reconvert; H8 does not re-run it.
- The census, under its own `pin go1.24.13 '<GOROOT-1.24.13>' '<GOROOT-1.24.13-posix>' || exit 3`, with the H5
  binary, **before the overlay** (the only CLEAN tree whose `version.props` reads 1.24.13 is `<tree>` at `<H2>`
  before the overlay dirties it, or a second clean worktree of `<H2>`), into a directory never reused:
  ```bash
  '<stage>/bin/go2cs.exe' -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 \
    -platform-census '<stage>/census-1.24.13' -go2cspath '<tree>/src' > "<stage>/logs/census-1.24.13-$(date +%Y%m%d-%H%M%S).log" 2>&1
  ```
  `-comments` because the census's control target is supposed to reproduce the seed byte for byte, and the seed
  was emitted with comments. Read `<stage>/census-1.24.13/platform-manifest.json`'s class counts
  (shared / variant / partial / exclusive) and the per-target marker gate (must be zero).
- `-goroot` and the loader: the loader follows the environment. A converter carrying `7c1d8832f` refuses a flag
  that disagrees with a set environment, and exports the flag only when the environment is unset. The pin
  assertion, which always exports GOROOT, is the selector.
- **STOP before scoring the gate.** No outgoing manifest is committed, and neither its comparand nor the
  default-flavour byte-identity arm has a procedure at this hop. Post the 1.24.13 manifest's per-target marker gate
  and class counts; COORD rules the comparand.
<!-- Ruling 3, COORD 9495495ec; db6d9462f §1 H8 row ("the three-target emission IS what H5 runs; the manifest gate itself is unmeasured").
     -goroot: C2 a6c126d65 §1 (measured: flag 1.23.12, loader read the ambient root), ruled COORD bc59c619d §1 ("Environment GOROOT unset ->
     export *goRootCmd"), cut 7c1d8832f (main.go:133-145, :443-447); the wording is C2's accepted rules line (830fa8d26). Census semantics
     and -comments: .claude/rules/converter.md:96-102 and :115-118 at a02ac3df3; example line a02ac3df3:src/go2cs/main.go:302. No
     platform-manifest file tracked at a02ac3df3. -->

#### Amendment 2026-09-19 (C2) — the comparand's provenance, the byte-identity arm, and the predicted deltas

The amendment above stops at *"neither its comparand nor the default-flavour byte-identity arm has a
procedure at this hop"*. This closes both. The instrument is [`src/h8-comparand.sh`](../src/h8-comparand.sh)
(`selftest`: 20 arms, every one **made to fail and restored**); it is a reader of manifests and package
sets, converts nothing, and writes into no corpus.

##### (a) The 1.23.12 outgoing manifest is **PRODUCED**, not recovered

Four candidates were measured before one was chosen. Three are refused, and two of them are refused
for reasons that would not have shown up as an error:

| candidate | verdict |
|:--|:--|
| a committed 1.23.12 platform manifest | **does not exist** — no `platform-manifest` file is tracked on any ref, and none ever has been |
| the preserved **half-A** staging roots (`c883a2dc7` §3) | ⚠ **WRONG RELEASE.** Half A's own recipe pins `GOROOT` to the go1.24.13 SDK and notes `version.props` already reads 1.24.13 — half A is the **incoming** side. Scored against G's 1.24.13 manifest it compares the release with itself: **an arm that cannot fail**, reporting a perfect zero delta |
| the preserved **half-B** staging roots (`a5534b5de` §2) | right release (go1.23.12, three targets) but **wrong artifact kind**: half B ran `-platform-stage`, the emission, not `-platform-census`, and its manifests cover **seeded** staging roots — see the seed tell below |
| the **H0** baseline | **does not contain one.** H0 captures the `.cs.auto` baseline, the package census, the roster snapshot and the disclosure manifests; the platform manifest is not among them |

⚠ **A seeded-root manifest is not a census.** A seeded staging root's path set is *(seed ∪ emitted)*
and all three targets share one seed, so such a manifest carries **no emitted-vs-seeded
discriminator** — which is precisely why the converter's own census answers that question with a
sentinel MTIME instead of content. Classify three seeded-root manifests and the `partial` and
`exclusive` counts come from the **seed's** path set rather than from any emission, while looking
exactly like class counts. The two preserved halves show the shape directly: half B's roots hold
3990 / 3995 / 3993 `.cs` against a 3896-file seed, and half A's hold 3898 on all three — the
difference is how far each seed already sits from the release being emitted, not a platform axis.

**The tell, and it is cheap:** in a true per-target emission census a `*_windows.*` artifact **cannot**
be emitted by the linux or darwin target. `h8-comparand.sh classify` refuses a triple in which a
platform-suffixed artifact appears in a foreign target's manifest, rather than returning a number
that reads like a census. `--seeded-content-only` accepts such a triple for the one question it *can*
answer — which shared paths differ in content across targets — and labels its own output as not
emission classes.

**Therefore the outgoing manifest is produced by running the same instrument under the outgoing pin**,
one axis from the 1.24.13 census (`GOROOT` + `version.props`), same binary, same flags, same seed,
into a directory never reused:

```bash
'<stage>/bin/go2cs.exe' -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 \
  -platform-census '<stage>/census-1.23.12' -go2cspath '<tree-1.23.12>/src' \
  > "<stage>/logs/census-1.23.12-$(date +%Y%m%d-%H%M%S).log" 2>&1
```

⚠ `version.props` must be the **outgoing** release's, verbatim: with the incoming 1.24.13 pin the
converter **refuses, exit 1, by design** (measured, `a5534b5de` §2). That refusal is the arm proving
the outgoing leg really ran against the outgoing tree, so it is a feature of this step, not an
obstacle to route around.

Half B is **not** discarded — it is the corroborator. Its three per-target manifests answer the
content axis under `classify --seeded-content-only`, and a variant count from the produced census
that disagrees with half B's content partition over the shared path set is a finding in one of the
two, named before either is believed.

##### (b) The default-flavour byte-identity arm

The gate's wording is *"the default-flavor build reproduces the single-target build byte-for-byte"*.
The two emissions, spelled:

- **E1, the single-target build** — `-stdlib -comments -platforms <host>/amd64` into a root seeded
  identically to E2's stage. Layout L3 is honoured by a single-target run (`platformLayout.go`, rule 1:
  an existing `<goos>/<name>.cs` is where this target's `<name>.cs` belongs), so E1 reproduces the
  layout rather than laying a flat duplicate beside it — which is why **no path normalisation is
  needed** and why introducing one would be the arm's most likely silent failure.
- **E2, the default flavour of the three-target corpus** — the same merged L3 corpus H5 produces,
  restricted to the view a build for `<host>` actually compiles: each package's **flat** files plus
  that package's `<host>/` folder, and nothing from a foreign GOOS folder.

**Compared by:** a per-file `sha256` manifest of each view — `"<sha256>␠␠<relpath>"`, `LC_ALL=C`
sorted — and the **tree hash** is `sha256` of that manifest file. The arm PASSES iff both sides are
non-empty, the path sets are equal, no shared path differs in content, and the two tree hashes are
equal. `h8-comparand.sh view <root> <host>` builds the manifest; `identity <A> <B>` scores the arm.

⚠ **A GOOS-named directory is not automatically a layout folder, and this one is live in the corpus.**
`internal/syscall/windows` is a *package* whose directory is named `windows`; measured at master
`7105c8468` there are 35 directories named `windows`, of which **34 are layout folders and one is
that package**. A filter excluding any path component in {windows, linux, darwin} drops the whole
package from the linux and darwin views — and because it drops it from **both** sides, the arm then
agrees about files it never looked at. The discriminator is structural: a directory is a layout
folder iff its name is a GOOS name, it holds **no** `.csproj` of its own, and its **parent** holds
one. Measured on the real corpus, the linux view keeps that package's 9 files and leaks 0 foreign
layout files.

**The controls that prove the arm can fail** — all five are in `selftest`, and the arm is not scored
until they have been run on the box that will score it:

1. an **empty** side refuses rather than reporting agreement (a baseline that silently reads empty
   otherwise reports total disagreement, or total agreement, with equal confidence);
2. a **one-byte content change** inside the host's own folder goes red, naming the path;
3. a **path-only change** (one file renamed) goes red — this is the control that proves the view is
   not eating differences;
4. a change in a **foreign** GOOS folder leaves the host view unmoved — the view's whole purpose;
5. a **reordered** manifest still passes, so ordering is never read as a difference.

After any planted perturbation the restore is verified **byte-identical by tree hash**, not by
`git status`.

##### ⚠ (a2) THE KEY `classify` TAKES IS THE FLAT ARTIFACT PATH — added 2026-09-19 after G measured the trap

A census staging root is **seeded from an L3 corpus**, so a platform-varying artifact sits under a
per-GOOS layout folder — `os/windows/file.cs` in the windows root, `os/linux/file.cs` in the linux one.
Build the manifest the obvious way (walk the root, `sha256sum`, keep the relative path) and those are
two different names, so **every platform-varying artifact scores `exclusive` and `variant` collapses to
exactly zero**:

```
  raw relative path    identical 1631 · variant  0 · partial  0 · exclusive 718 · union 2349
  flat artifact path   identical 1631 · variant 83 · partial 93 · exclusive 283 · union 2090
  the converter's own  identical 1631 · variant 83 · partial 93 · exclusive 283 · union 2090
```

⚠ **The raw row sums to its own union, passes the partition check and clears the seed tell, and is
wrong.** It is the L3 **tree** partition (`1631 + 718` = the manifest's `l3UnionTreeTotal`, with 718 its
`l3PerGoosFiles`) — a true answer to a different question. The only unaided tell is a reader noticing
`variant 0`. `platformCensus.go` keys an artifact by its **flat package-relative path**: its own
`variantFiles` read `os/file.cs`, never `os/windows/file.cs`.

So the key is not left to the caller. **`h8-comparand.sh manifest <census-target-root>` builds
`classify`'s input**, stripping layout folders with the **structural** discriminator — never the
directory names, because each target holds 100 GOOS-named directories of which 99 are layout folders and
**one is a real package** (`internal/syscall/windows`, which carries its own `.csproj`), and a name
filter deletes that package from two of the three views. It asserts **zero duplicate keys** (stripping
must merge no two artifacts), and it **stamps** what it writes. **`classify` REFUSES an unstamped
manifest** and names this mode in the refusal; `--assume-flat` exists for a manifest produced elsewhere,
and the caller owns that claim. The trap is a self-test arm: the same synthetic tree read flat finds the
variant and read raw reports `variant 0`.

⚠ **AND THE BUILDER NEEDS `--emitted-only` ON A CENSUS TARGET ROOT — the second gap, closed 2026-09-20.**
A census target root is **seeded from an L3 corpus AND emitted into**, so it holds *both*
`archive/tar/package_info.cs` (flat, from the seed) *and* `archive/tar/<goos>/package_info.cs` (this run's
layout copy). Stripping maps the second onto the first and the duplicate-key guard **refuses at rc 5** —
loudly and correctly, but the sanctioned builder then could not consume the artifact the gate is about.
What disambiguates them is the census's own **emitted-set restriction**: `platformCensus.go` stamps every
SEEDED file with the sentinel mtime `2000-01-01T00:00:00Z`, so a file whose mtime has moved was written by
this run. **`manifest --emitted-only` applies that filter**, reproduces the manifest's own `emittedCs`
exactly, and the refusal without it now names the flag as the remedy. Controlled on the real shape: the
seeded-plus-emitted root refuses, the flag admits it and keeps only this run's artifacts, and a root where
nothing was emitted refuses rather than returning an empty manifest.

<!-- G measured this by USING the instrument (mailbox ae545151f §6a): the counts above are the real
     incoming census at 46307b4704. Nothing in the original amendment was wrong -- it specifies the
     manifest FORMAT and never says to build one by walking a census root, and the `view` arm already
     carried the structural discriminator -- but the build step for classify's input was unspecified and
     the natural reading of it failed silently. Closed by making the format self-identifying rather than
     by documenting a convention: a convention a reader must remember is not a gate. -->

##### (c) Predicted class-count deltas, from the 1.24 package census

Derived at run time by `h8-comparand.sh pkgdelta <goroot-1.23.12> <goroot-1.24.13>` (2.7 s), which
**independently reproduces** `docs/phase4/CENSUS-go124-package-delta.md` on a different host and OS:
306 / 304 / 305 → 346 / 344 / 345, net **+40 on every target**, **54 added and 14 removed**, both sets
**identical across all three targets**, and the removed set exactly the 14 rows of
`docs/phase4/h5-removals.txt`.

Because the added and removed **package** sets are identical on all three targets, package membership
contributes **zero** to `partial` and `exclusive`. All class movement from the delta is file-level:

```
  ADDED    54 packages   153 distinct .go artifacts   150 on all three targets
                                                        0 on exactly two   -> partial  +0
                                                        3 on exactly one   -> exclusive +3
  REMOVED  14 packages    53 distinct .go artifacts    53 on all three targets
                                                        0 on exactly two   -> partial  -0
                                                        0 on exactly one   -> exclusive -0
```

The three exclusive artifacts are one per target and all in `crypto/internal/sysrand`:
`rand_windows.go`, `rand_getrandom.go` (linux), `rand_arc4random.go` (darwin).

**The predictions, as worded, to be scored against the produced 1.23.12 → 1.24.13 comparison:**

| # | prediction | falsifier |
|:--|:--|:--|
| P1 | `Δ partial` **= 0** | any non-zero partial delta |
| P2 | `Δ exclusive` **= +3**, and the three are `crypto/internal/sysrand`'s per-target `rand_*` artifacts | a different count, or a different package supplying them |
| P3 | `Δ (identical + variant)` **= +97** source artifacts (+150 − 53), **+40** `package_info` artifacts (+54 − 14 — which is the net package count, and is the internal consistency check), before any test-side artifacts | a source-artifact delta that is not +97 |
| P4 | the `identical` / `variant` split of the 150 added artifacts is **not** predictable from `.go` selection — it is content-dependent, and is a **reading**, not a prediction | — stated so a later number is not read as having been foreseen |
| P5 | the per-target class counts move **symmetrically**: any per-target asymmetry beyond P2's one-artifact-per-target is **not** from package membership and is a finding | an asymmetry the package delta does not explain |

##### ⚠ (c2) P1–P5 MEASURED — and the lesson is about the DERIVATION, not the numbers (2026-09-20)

Scored against the produced outgoing manifest (G `cb1fa651a`, ruled `63b51e754`). **None is a gate item;
all are readings.**

| | as worded | measured | verdict |
|:--|:--|:--|:--|
| P1 | `Δ partial` = 0 | **+6** (7 in, 1 out) | REFUTED |
| P2 | `Δ exclusive` = +3, sysrand's per-target `rand_*` | the three **arrived exactly**; `Δ` = **+6** (15 in, 9 out) | SPLIT: artifacts met, count refuted |
| P3 | +97 source, +40 `package_info` | **+117**, **+37** | REFUTED, both |
| P4 | the identical/variant split is a READING | +155 / +4 | HONOURED |
| P5 | per-target symmetry | +164 / +165 / +166 | **RULED not a defect** — the tolerance was written too tight |

⚠ **THE DERIVATION THEY REST ON WAS EXACTLY RIGHT, AND THAT IS THE POINT.** `pkgdelta`'s net **+40
packages per target** is met on all three, on a different instrument and host, with zero residual. Every
miss is the **extrapolation from package membership to file-level classes**, and the blind spot is now
measured: **+20 source artifacts and +6 partial** come from **Go's own per-file build-tag selection
changing inside packages that exist on all three targets and were never in the added or removed sets**
(1.24's `os.Root` work — `os/root_unix.cs`, `os/root_nonwindows.cs` — landing in packages already
everywhere). A package-delta derivation is structurally blind to it.

**THE RULE: a class-count prediction is derived from the FILE-LEVEL tag selection, or it is stated as a
package-level BOUND and not as a class count.** A clean measurement of the wrong population is the most
persuasive kind of wrong.

⚠ **Two further shapes worth carrying, because each looked like corroboration:**

- **A RELOCATION nets zero.** P2's three artifacts arrived exactly as named *and contribute nothing*,
  because the same three left `crypto/rand` — a package that **survives** the hop and so sits in neither
  the added nor the removed set. A derivation that examines added packages' files and removed packages'
  files cannot see a file moving **out of a surviving package**. Naming the right artifact is not
  predicting its effect.
- **Two numbers agreeing is evidence only if they are the same quantity.** P3's `+40` was cited as an
  "internal consistency check" because it equalled the net package count. There is one `package_info.cs`
  per **EMITTING** package (303 → 340 = **+37**), while +40 is the **QUEUED** delta; three net-new queued
  packages emit no `.cs`. The consistency check *was* the defect.

**The measured statement on REHEARSAL's "a migration moves the platform axis in BOTH directions"**, on the
one-axis comparison: **true at `exclusive`** (15 in, 9 out), **false at `variant`** (4 in, **0** out) and
**false at `partial`** (7 in, 1 out). The H8 text takes that as measured rather than as predicted. The
prior mixed reading (38 → 51, "13 arrived") decomposes cleanly once the axis is isolated: **38 → 47 is
converter drift (+9), 47 → 51 is the release (+4)**, and the release's four are 1.24's `os.Root` and
spinbit-mutex work.

##### (d) Four instrument findings that change how the census is invoked

⚠ **`GO111MODULE=off` silently cancels a `GOTOOLCHAIN` redirect.** Measured 2026-09-19 on a linux box:
`GOTOOLCHAIN=go1.23.12 go version` prints `go1.23.12`, and `GOTOOLCHAIN=go1.23.12 GO111MODULE=off go version`
prints the **ambient** toolchain **at exit 0**. The package-census instrument is specified *with*
`GO111MODULE=off` — precisely the cancelling combination — so a census driven by `GOTOOLCHAIN` alone
measures whichever toolchain the box happens to carry and looks perfect doing it. This is H1's
silent-redirect hazard reached through the *other* half of the pin: drive each release by **its own
`GOROOT` and its own `bin/go`**, and assert the release from `go version` **OUTPUT** before listing
anything. `pkgdelta` does both, and additionally refuses when the two roots run the same release —
a vacuous delta being the failure this guards.

⚠ **THE BUILD TAGS ARE THE THIRD AXIS, measured the hard way twice in one hour (2026-09-20).** The
corpus is defined as Go under `-tags purego,math_big_pure_go` (the `-stdlib` default), so a census run
**without** them answers a different question and looks correct doing it: C1's H10 census ran untagged and
read two rows as DIFF where the census was wrong twice and the converter right twice, and a COORD ruling
(`TestP256PrecomputedTable` off-platform) was **withdrawn** over the same axis because the answer flips
under the corpus's own tags. **State the tag set beside every count.**

⚠ **`CGO_ENABLED` is a real axis on the package count, and it moves exactly one target.** Measured at
both releases: linux reads 305 / 345 at `CGO_ENABLED=1` and 304 / 344 at `CGO_ENABLED=0`, the one
package being `runtime/cgo`; windows and darwin do not move. A census taken on a linux host
targeting linux natively therefore disagrees with one taken on a Windows host by one package on one
target, with neither being wrong. **`CGO_ENABLED=0` is the pin** — it is what reproduces the recorded
census, and it matches the recipe every preserved artifact was cut under.

⚠ **THE TOOLCHAIN'S LOCATION IS THE FOURTH AXIS, and until `19175c31ad` it moved the emitted
corpus with nothing in any log saying so.** `conversionDriver.go` loaded `"./..."` — the input
package AND its whole subtree — for any input under GOPATH. A `GOTOOLCHAIN`-installed toolchain
lives at `$GOPATH/pkg/mod/golang.org/toolchain@<version>`, so on such a box GOROOT is itself under
GOPATH, every stdlib package is also a GOPATH input, and every one of them converted its subtree.
That pulled in `runtime/cgo` — buildable when named, absent from `go list std` at
`CGO_ENABLED=0`, never queued and so never skipped — emitting nine `.cs` under `runtime`'s own
conversion, out of dependency order, with the package named nowhere in the run. A side-by-side SDK
install is not under GOPATH, so the branch never fired there and **the same binary, base, flags and
pin emitted a different corpus on the two boxes**. Measured both ways: C2 with the condition true and
the one-axis effect (9 `.cs` -> 0, 13 skip messages -> 7, the queued-package list byte-identical,
`diff -rq` over `src/core` one line), G with the condition false and the effect absent.

**The seat `19175c31ad` removes the dependency** — a GOROOT input is excluded from the subtree load
outright — so this axis no longer moves a `-stdlib` emission. It is recorded here anyway, because a
census taken with an OLDER converter still carries it, and because the general shape survives the fix:
a GOPATH tree genuinely wanting a subtree load still gets one silently, and nothing in an emission
states which roots it ran under. **State the toolchain's location beside the pin, the tag set and
`CGO_ENABLED`, for any count taken before that seat.**

<!-- C2, 2026-09-20, in-stage. Ruled a converter defect at COORD 3d0c7cd5d on C2's 1257a20bad; G's
     cross-box confirmation at 4f4e3f9f7 (identical queue sha256 across two hosts and two separately
     built binaries, and GOROOT NOT under GOPATH on G-LAPTOP); seat accepted at 887e92d6b. The
     heading said "Two" while carrying three; corrected to four here rather than left to drift.
     ⚠ The author of this section violated its own CGO_ENABLED pin within the hour: the first
     arm-b re-take omitted the export, ran at the box default of 1, and read 343 packages with
     runtime/cgo LEGITIMATELY queued at [332/343] -- a two-axis run reported as one. Caught only
     because the script carried a WRITTEN PREDICTION (0 and 0) that the run falsified; nothing else
     in the run looked wrong. The pin is now an asserted export with a comment in the runner rather
     than a sentence in a document, which is the difference between a rule and a guard. -->

<!-- C2, 2026-09-19, in-stage per the doc-authority ladder (the runbook leads on procedure).
     Refused candidates: half A = c883a2dc7 s3 (GOROOT = the go1.24.13 SDK; "NO substitution needed for half A"),
     half B = a5534b5de s2 (GOROOT = the go1.23.12 SDK; version.props from the outgoing release verbatim; 16m05s,
     windows 3990 / linux 3995 / darwin 3993 .cs against a 3896-file seed). "No platform-manifest tracked" re-verified
     at 7105c8468 by `git ls-files` and by an --all --diff-filter=A search: zero on both.
     Classes and their ORDER are platformManifest.go:166-203 (3 emitters -> identical|variant, 2 -> partial,
     1 -> exclusive); the L3 path shape and the layout-honouring single-target reconvert are platformLayout.go:9-46.
     Package-vs-layout trap measured at 7105c8468: 35 dirs named windows, 34 layout + internal/syscall/windows
     (own .csproj); the linux view keeps its 9 files, leaks 0.
     pkgdelta reproduces CENSUS-go124-package-delta.md s1 on linux/amd64 where the record was cut on windows/amd64;
     the one discrepancy (linux +1 at both releases) is the CGO_ENABLED axis in (d), not a disagreement.
     Script self-test 20/20 at the cut, each arm red-proved then restored. -->




### H9 — Behavioral golden rebank **GATE**

Behavioral goldens are conversions of go2cs's *own* Go programs, so a corpus migration reaches them
through §1.1's three channels — and which are live is knowable in advance. **Predict the diff's size
before running the rebank**; a diff that materially exceeds the prediction is a finding, not a rebank.

Procedure: re-transpile everything **first**, then update the goldens, then **classify every moved
golden before banking**. A migration is not a licence to rebank unexamined diffs.

> **Dated correction, 2026-09-07 (H9 PREP).** This step used to read "re-transpile everything
> **first** (the golden-update utility copies on-disk `.cs`; it does **not** re-run the converter,
> so a copy over stale output silently re-baselines it)". **The parenthetical has been false since
> 2026-09-04:** both re-baseline paths -- the `UpdateTestTargets` utility and
> `run-behavioral.ps1 --update-targets` -- now RE-TRANSPILE each project they are about to
> re-baseline, unconditionally, immediately before the copy, and REFUSE by name when that
> transpile fails, times out, or exits 0 having converted best-effort. **The instruction survives;
> its stated reason does not** -- re-transpiling first is still right because the emission should
> be examined before it becomes a record, but a reader acting on the old reason believes a closed
> hazard, and may believe the utility is safe to point at a stale tree, which the refusal now
> prevents. `--only <Name>[,...]` narrows one invocation, which is what makes the refusal branch
> cheap enough to exercise. Deliberately NO up-to-date predicate on either path: a stale
> COMPARISON is recoverable, a stale RECORD is not.

**Gate:** Transpile, Compile and Target at zero failures, and Output's failing set equal **by name** to the
outgoing base H0 captured, with APPEARED empty; the prediction is re-derived at the tip of record on the banking
platform. *(Edited in place 2026-09-26, §7 fix A7 and lesson 27: this read "green across all four phases", which
master's own base could not meet.)*
Note the runner's **own** internal
budgets are independent of the caller's, and a budget that expires reports `NOT MEASURED`, which fails
the run and must **never** be read as a corpus regression.

#### Amendment 2026-09-13 — the 1.23 → 1.24 rebank: the pin after the window, CNR first, the expected set by name

- **Pin.** The rebank runs on the version branch after H5's overlay, where the tree carries one release: the
  H2→H5 window's pairing no longer applies. Each battery is launched from inside a Git Bash script that first
  runs `pin go1.24.13 '<GOROOT-1.24.13>' '<GOROOT-1.24.13-posix>' || exit 3` — the bash pin reaches a `.ps1` only
  when the `.ps1` is launched from that script; `go version <exe>` reads go1.24.13. Not yet measured on a hopped tree.
- **CNR first:** `powershell -NoProfile -ExecutionPolicy Bypass -File '<tree>/src/tests/Behavioral/check-no-regression.ps1'
  > "<stage>/logs/cnr-$(date +%Y%m%d-%H%M%S).log" 2>&1`, run solo. Want: zero NOT MEASURED, and CHANGED on exactly
  the eight projects below — any other CHANGED member is a finding before any re-baseline, because a whole-corpus
  re-baseline banks every drift silently.
- **The prediction of record:** 8 goldens, 35 changed line-pairs, `added == removed` on every file, one mechanism
  (the `Δruntime` alias drop), zero T5. The eight: `FuncForPCName`, `FuncLiteralCallerNames`, `GoexitDefers`,
  `GoroutineWaitState`, `IterPullRendezvous`, `RuntimeCallerFrames`, `SetFinalizerBridge`, `SyscallKeystonePulls`.
- **Re-baseline** with `UpdateTestTargets --createTargetFiles --only FuncForPCName,FuncLiteralCallerNames,GoexitDefers,GoroutineWaitState,IterPullRendezvous,RuntimeCallerFrames,SetFinalizerBridge,SyscallKeystonePulls`,
  run from its `bin/Debug/<tfm>` inside the same pinned script, whole stream to a dated log. `run-behavioral.ps1`
  has no `--only` (an unknown argument prints usage and returns 2). A ninth moved golden, a non-alias hunk or a
  file with `added != removed` is a finding, never a rebank.
<!-- The window closes at H5: this document's H2 ruling (two releases until H5's regen) and its fourth arm (goldens re-baselined at H5,
     the eight dropping the Delta legitimately). Prediction: a02ac3df3:docs/phase4/REHEARSAL-h9-golden-rebank.md:15-36; CNR first and the
     narrowing flag: same file :141-144. Re-baseline path: a02ac3df3:src/utilities/UpdateTestTargets/Program.cs:131-164 (--only, a
     comma-separated list, parsed inside --createTargetFiles, :142) and .claude/rules/harness-gates.md:451; BehavioralRunner has no --only
     (a02ac3df3:src/tests/Behavioral/BehavioralRunner/Program.cs:183-231; default case prints "Unknown argument" and returns 2). The
     runbook's own line 651 conflates the two paths. The eight by name, each present and none skipped: i9 7ede39d67 §1 (the alias-union
     acceptance, recorded by COORD 204c3ab59). -->

#### Correction 2026-09-19 (C2), PLATFORM-QUALIFIED 2026-09-20 — a LINUX re-derivation, and what it can and cannot settle

> ⚠ **READ THIS BOX FIRST — added 2026-09-20 after i9's CNR measured the banking platform.**
> Everything below §"Mechanism 1" was measured on **linux**. The goldens are banked on **windows**, and
> on that platform **the amendment's original eight goldens / 35 line-pairs is EXACTLY RIGHT** — it was
> not stale at all. The two readings differ by precisely two projects, and both differences are the
> per-GOOS alias mechanism this correction itself names:
>
> | | linux (this correction) | **windows, the BANKING platform** (i9 `dc9eb368c`) |
> |:--|:--|:--|
> | `SyscallKeystonePulls` | alias RETAINED → out of the set | **drops `Δruntime` → IN the set**, 2 pairs |
> | `SetegidBroadcastSeam` | drops `Δruntime` → in the set, 4 pairs | `//go:build linux` — **CNR SKIPS it platform-exclusive**, it cannot be banked here at all |
> | mechanism 1 | 8 goldens / **37** pairs | 8 goldens / **35** pairs |
>
> **THE RULE THIS COST, stated so the next lane does not pay it again: a cross-platform arm can
> discover a MECHANISM, but it cannot enumerate the BANKED SET.** The rebank happens on one platform;
> membership is therefore a platform-qualified claim, and a reading taken elsewhere must say so in its
> own conclusion — not merely list its blind spots and then conclude platform-free. This correction
> listed its seven blind projects and then did exactly that, which is the same defect it flags two
> sections below for `SockaddrRoundTrip`. **The mechanism findings stand on both platforms; the
> membership and the pair count are linux's.**
>
> What survives unqualified: **mechanism 2 is real and was found only by the whole-corpus comparison** —
> i9's windows CNR confirms `GenericTypeInference`, `GenericUntypedIntArg` and `ReceiverCapturedInClosure`
> and adds a fourth file (`ReceiverCapturedInClosure/package_info.cs`, the same RED 11 seat, missed here
> because this sweep compared `main.cs` only — a `GoPositionMap` base64 shifting because the hoist moved
> line positions) and a fourth project, `SystemCertVerify`, under **RED 9** — one of the seven this arm
> declared it could not see. The process gap below is unchanged and is what all four have in common.
>
> ##### THE BANKED SET ON THE BANKING PLATFORM — the TWELVE (i9 `dc9eb368c`, ruled `ab9e7209a`)
>
> ```
>   MECH 1, alias, 8 goldens / 35 pairs
>     RuntimeCallerFrames 15 · SetFinalizerBridge 6 · FuncLiteralCallerNames 3 · GoroutineWaitState 3
>     FuncForPCName 2 · GoexitDefers 2 · IterPullRendezvous 2 · SyscallKeystonePulls 2
>   MECH 2, seats that banked no golden, 4 files over 4 projects
>     GenericTypeInference (RED 12, 4 pairs) · GenericUntypedIntArg (RED 12, 1 pair)
>     ReceiverCapturedInClosure main.cs +4/-1 AND package_info.cs (RED 11, one seat, two files)
>     SystemCertVerify (RED 9)
>   NOT banked here: SetegidBroadcastSeam -- `//go:build linux`, CNR SKIPS it platform-exclusive.
>                    Real on linux, out of scope for a windows rebank, and NOT deleted: a
>                    linux-hosted rebank would need it.
> ```
>
> ⚠ **`SystemCertVerify` is RED 9, and the attribution is a predicate with a FIRING control** — not a
> name match. The hunk is an alias ARRIVAL (`using io` → `using Δio`), which is neither mechanism 1's
> package nor its direction, so i9 attributed it: exactly one landed seat in the window touches
> `importAliasOperations.go` (`f643b67d4`, *a package reached only through a TYPE takes the CS0576
> alias rename*), `SystemCertVerify` has **0** `io.` call sites — RED 9's own predicate, reached only
> through a type — and 20 goldens already carry `Δio` as the established corpus form. **The control
> that makes the predicate discriminate:** `AdapterNameInterfaceCollision` carries `Δio` *and* has 3
> `io.` call sites, i.e. it got its alias the ordinary way, so "carries `Δio`" alone would not have
> separated them.

The amendment above names eight goldens, 35 changed line-pairs and one mechanism. That prediction was
made at `a02ac3df3` for the **H2→H5 window**, and the version branch has since taken every RED seat and
the q9x applies. **Re-derived and MEASURED at version tip `06b1636cae`** — the converter built at that
tip and all **735** behavioral goldens transpiled and compared, not estimated: **716 SAME, 14 CHANGED,
5 NOT MEASURED**. The eight are **stale, not mistaken**.

##### Mechanism 1 — the `Δruntime` alias drop: eight goldens, **37** line-pairs

```
  RuntimeCallerFrames  15    FuncLiteralCallerNames  3    FuncForPCName       2
  SetFinalizerBridge    6    GoroutineWaitState      3    GoexitDefers        2
  SetegidBroadcastSeam  4    IterPullRendezvous      2
```

`added == removed` on every one, and **zero diff lines not containing `runtime`** on all eight —
the single mechanism, measured rather than asserted.

- ⚠ **`SyscallKeystonePulls` is not in the set ON LINUX.** Its golden carries `Δruntime` and so does the
  linux emission at 1.24.13 — it imports `os/user` and `os/exec` alongside `runtime`, and the collision
  that forces the alias survives there, so it contributes **0**. ⚠ **ON WINDOWS IT DOES DROP THE ALIAS
  AND IS IN THE BANKED SET** (i9 `dc9eb368c`, 2 pairs): the collision does not survive there. This was
  worded as "the sharpest falsifier of this whole prediction" and it fired — correctly, and on the
  platform that banks.
- ⚠ **`SetegidBroadcastSeam` is in it ON LINUX**, at 4 pairs, pure alias — but it carries
  `//go:build linux`, so **the windows CNR SKIPS it as platform-exclusive and it can never be banked
  there.** It is a real ninth for the mechanism and NOT a member of the banked set; it entered this
  prediction only because a linux arm could see it. The inverse of this correction's own blind-spot
  list: a project the BANKING platform cannot measure.
- **The 35 reconciles exactly**: the seven surviving originals total 33, and 35 − 33 = 2 is precisely
  what `SyscallKeystonePulls` would have contributed at the two-pair shape (one `using` line, one use
  site) that three of its siblings show. The stale number counted a project that does not move.

##### Mechanism 2 — converter seats that changed emission and banked no golden: three

| golden | shape | seat |
|:--|:--|:--|
| `GenericTypeInference.cs` | `Scale(p, (int32)(2))` vs `Scale(p, 2)` — 4 pairs, 1:1 | **RED 12** `457cba3b60` (`convCallExpr.go`) |
| `GenericUntypedIntArg.cs` | same arm — 1 pair, 1:1 | **RED 12**, same seat |
| `ReceiverCapturedInClosure/main.cs` | the hoist — **+4 / −1** | **RED 11** `410976f049` (`convSelectorExpr.go`) |

##### The `added == removed` rule, restated PER MECHANISM

The rule was written when there was one mechanism. It now reads: a **mechanism-1** golden must satisfy
`added == removed` **and** carry no non-alias line; a **mechanism-2** golden must match the emission
shape its seat is known to produce — RED 12's typed constant is 1:1, **RED 11's hoist adds lines by
construction**. `ReceiverCapturedInClosure` at +4/−1 is accepted as RED 11's shape **on a quote of the
hunk, never on the count**. *"A finding, never a rebank"* is satisfied by the finding being
**classified and named before the copy**; a hunk that cannot be attributed to a landed seat stays a
finding and stops the rebank.

##### ⚠ The blind spot, named as a set — the re-derivation ran on LINUX

```
  NOT MEASURED (emits nothing on linux)  FindFirstFileData · PointerOutParameter · SystemCertVerify
                                         WindowsNewCallback · WsaProtocolInfo
  MEASURED BUT PLATFORM-DIVERGENT        SockaddrRoundTrip · WsaSendtoRoundTrip
```

The last two are the platform showing itself — `syscall.Sockaddr` vs `syscallꓸSockaddr`,
`SockaddrInet4жSockaddr` vs `SockaddrInet4жΔSockaddr` — because the `syscall` package's own content
differs per GOOS, so the collision set the renamer sees differs. **They are not findings.** The
Windows CNR reads all seven; each that comes up CHANGED is classified by mechanism from its hunk
before it joins the `--only` list, and one fitting neither mechanism is a finding.

**A cross-platform arm is admissible only if it is SHOWN to be**, and the control is the rebank's own
history: the **seven goldens re-baselined inside this window** (`CollidingPackageNames`, `CrossPkgUser`,
`DefinedOverNamedComposite`, `DefinedTypeOverForeignStruct`, `DefinedTypeOverPkgType`,
`LiftedLocalTypes`, `NamedArrayWrapper`) reproduce **byte-identically**, 7 of 7 — they were re-baselined
by the very seats in this window, so a platform that moved their emission would show here. It does not.
⚠ That control also caught the run's own defect before it became a result: the first pass passed
`-comments`, and **behavioral goldens were captured without them**, which read as 144 changed lines on a
project that had to be clean. A cross-platform arm without a same-window control is an opinion.

**A hand-owned golden is not a rebank candidate.** A whole-corpus sweep that compares a fresh emission
against `[module: GoManualConversion]` files reports drift the converter cannot produce: the converter
does not write them (it emits the `.cs.auto` sibling). `ManualConversionSiblingState/state.cs` read
+0/−9 for exactly that reason and is predicted **SAME** on CNR.

##### ⚠ The process gap this exposed, which outlives the hop

Mechanism 2 exists **only because two converter seats changed emission and banked no behavioral golden
in the same commit.** Measured over this window: **109** commits touch `src/go2cs` since `a02ac3df3`,
**32** of them touch emission source, and **4** re-baselined a behavioral golden in the same commit
(one more did it in a companion commit). The rest were the stdlib hand-own registry, the `-tests`
pipeline, or plumbing — but RED 11 and RED 12 were neither, and their drift sat unbanked until a
whole-corpus comparison found it.

**The rule, going forward: a converter seat that changes emission re-baselines the goldens it moves in
the same commit, or NAMES in its message why it moves none.** A seat gated on the stdlib compile front
has not been gated on the behavioral corpus, and the gap is invisible until H9 — which is the one step
that re-baselines wholesale, i.e. the step most likely to bank it silently.

<!-- C2, 2026-09-19, in-stage; ruled by COORD at mailbox 39395d257 on C2's measured prediction 56ec9931a.
     Method: go2cs built at 06b1636cae (go1.24.13 linux/amd64, -trimpath -buildvcs=false); every project
     transpiled into its own output root with the output directory as the SECOND POSITIONAL, sequentially,
     never concurrent; compared against EVERY .cs.target the project carries, not just main.cs — 193 of the
     735 are per-source-file targets and a main.cs-only sweep cannot see a ninth hiding in one.
     Both sides asserted non-empty before any verdict, so a project that emits nothing on this platform
     reports NOT MEASURED and never SAME.
     Line-pair counts are diff hunks on the emitted .cs vs its .cs.target at that tip.
     The git derivation agrees with the measurement on WHERE: the mechanism-2 goldens fall in exactly the
     convCallExpr.go / convSelectorExpr.go seats that banked nothing.
     The stale prediction and its provenance: a02ac3df3:docs/phase4/REHEARSAL-h9-golden-rebank.md:15-36. -->


#### Closure 2026-09-20 — H9 CLOSED: the suite reads the base two, APPEARED empty, the 26 opt-out skips reconciled

**H9 is CLOSED.** Closure tree: version-branch commit `c7eb36d845` — R's P-256 table decode applied on
`d91c832543` — reading `go version` `go1.24.13 windows/amd64`, measured through the behavioral runner's
four phases, no MSTest host.

**The criterion, restated for this rung.** The gate line above reads "green across all four phases";
master's behavioral base is not zero, so green is scored against that base and the suite's own rc is
FAIL by construction. Closure required, and got, all four:

- Output's failing set equals master's behavioral base **two BY NAME** — `FuncLiteralCallerNames` and
  `GoroutineWaitState` — with **APPEARED empty**.
- Transpile, Compile and Target each at **zero failures**.
- **Zero timeouts in every phase.** A budget overrun reports `NOT MEASURED` and fails the run; none
  occurred.
- The **twelve** goldens re-baselined at the rebank pass the **byte-compare** — Target passed for
  every project.

**The reading.** 698 projects in the directory = **696 behavioral + 2 tooling** (the runner itself and
the MSTest harness project, excluded from enumeration); 6 platform-exclusive `[linux]` projects are
skipped BY NAME by the runner, so **690 ran**.

```
  Transpile  690 pass / 0 fail        Target  690 pass / 0 fail
  Compile    690 pass / 0 fail        Output  662 pass / 2 fail / 26 skip / 0 timeout
  1,298.8 s.  Suite rc = FAIL, on the base two — which is the expected reading.
```

**The 26 Output skips are a DECLARED OPT-OUT CLASS, named here so the count is derivable**: projects
whose package-info file does not carry `[GoTestMatchingConsoleOutput]`. Output comparison is **opt-in**,
and the runner's only OTHER skip site requires a Compile failure — Compile passed 690/690, so that site
cannot have fired. Reconciled three ways: `28 non-declarers − 2 tooling = 26`;
`696 − 6 platform-exclusive = 690`; `662 + 2 + 26 = 690`. ⚠ **A skip is not a pass.** This run says
nothing about those 26 programs' agreement with Go.

⚠ **`SystemCertVerify` — the RED 9 regression the decode cures — is NOT read from this suite.** This
runner yields no verdict on a host crash: it aborts with no results artifact, so a crashing project
leaves no row to read. The arm of record is therefore the **direct executable** — exit 0, 17 lines
identical to Go, measured on i9 at the same tree — and within this suite it is the **Compile** phase
that covers it.

##### Two runbook lessons from the run

1. **The runner's own disk preflight (25 GB floor) refused at 1.4 GB free, and is never overridden.**
   `-IgnoreDiskPreflight` exists; using it yields a `NOT MEASURED` suite that reads like a failing one.
   Build output was reclaimed FIRST, after proving no `bin/` or `obj/` path is tracked.
2. **Capture the suite's rc on its own line.** A wrapper that ends in `tail` reports `tail`'s exit
   status — which is 0, and means nothing at all beside a summary reading FAIL.

**H10 opens on this closure**, and H11 is declared after H10, as already ruled.

<!-- H9 CLOSURE, 2026-09-20, in-stage. Measured on i9 at version-branch commit c7eb36d845 (R's P-256
     table decode applied on d91c832543); go version go1.24.13 windows/amd64; the behavioral runner's
     four phases, no MSTest host.
     Suite: Transpile 690/0, Compile 690/0, Target 690/0, Output 662 pass / 2 fail / 26 skip / 0
     timeout, 1,298.8 s; rc FAIL on the base two (FuncLiteralCallerNames, GoroutineWaitState), with
     APPEARED empty.
     Enumeration: 698 in the directory = 696 behavioral + 2 tooling (the runner itself and the MSTest
     harness project, both excluded from enumeration); 6 platform-exclusive [linux] projects skipped by
     name by the runner, so 690 ran.
     The 26 Output skips: a package-info file without [GoTestMatchingConsoleOutput]. Comparison is
     opt-in, and the runner's only other skip site requires a Compile failure, which cannot have fired
     at 690/690. Three-way reconciliation as stated in the text; a skip is not a pass and the run
     carries no claim about those 26.
     The twelve re-baselined goldens are the banked set of the correction above (i9 dc9eb368c, ruled
     ab9e7209a); Target passing for every project IS their byte-compare.
     SystemCertVerify: no suite verdict is obtainable on a host crash, since the runner aborts with no
     results artifact; the arm of record is the direct executable, exit 0 and 17 lines identical to Go,
     on i9 at the same tree. Compile is what covers it inside this suite.
     Lesson 1's floor is the runner's own 25 GB disk preflight, which refused at 1.4 GB free; no
     -IgnoreDiskPreflight was passed, and build output was reclaimed only after proving that no bin/ or
     obj/ path is tracked. Lesson 2 is the capture-the-exit-code-before-any-pipe rule of CLAUDE.md's
     safety floor, met here on a suite whose summary read FAIL beside a wrapper rc of 0. -->




### H10 — Roster, proof-page and disclosure re-derivation ⟲ **GATE**

**The migration's largest step, and the one §3 makes a campaign.** Every banked test suite is derived
from the release's own test sources, so **every roster row re-validates from scratch**: numerator,
denominator and disclosure set alike. There is no carry-forward path.

Per banked package:

1. Re-run the converted-test pipeline against the new GOROOT package — **the pipeline itself**
   (`go2cs -tests -test-action all <new-goroot-pkg> <core-pkg>`, **all four overrides** set --
   the Go pair per H1.1, and the .NET pair (`DOTNET_ROOT` + PATH, [`DotNetMigration.md`](DotNetMigration.md)
   trap 6) on any box whose machine-default SDK lags the corpus TFM; missing the .NET pair is a
   NETSDK1045 wall, measured),
   **never the sweep wrapper**: `run-validated-sweep.ps1` is the steady-state gate, enforcing the
   exact *banked* count and a drift-clean corpus — both of which this step invalidates **by
   design** (measured 2026-08-25: the wrapper reds every hop row in seconds — count mismatch, plus
   the re-emitted test sources reading as unclassified drift). The re-emitted sources are the
   **bank-in-waiting** for this step's own re-bank — leave them in the tree, restore only the
   standing production-flip classes at the end. `run-validated-sweep.ps1 -Hop` runs the steady-state pass over
   `docs/phase4/data/population-go<release>.txt` and refuses by name without it. Until NOTES A3 lands it does NOT
   apply execution pins, so a pinned row is read outside `-Hop`. The recon leg stays the recon wrapper's.
   *(Edited in place 2026-10-01, §7 lesson 68: this read "A wrapper `-Hop` mode is open instrument debt". `-Hop`
   landed with TRAIN K, and its rows still carry no execution pin.)*
2. **Re-derive the verdict count.** The denominator moves — tests are added and removed.
3. **Re-derive the disclosure manifest.** Disclosures are pinned by **exact failure signature**, so a
   renamed or reworded test invalidates its pin and the manifest is **re-signed, never edited** (§4,
   class T5's sibling hazard).
   ⚠ **Since 2026-09-05 a re-derived manifest emits the TWO allocation labels, not `alloc-profile`.**
   Every allocation-count assertion resolves into `deferred` (the CLR can meet it; the entry carries
   `want`, `reading` and `plan`, and the loader plus the roster guard both refuse it without them) or
   `structural` (a proof in the reason that it cannot be met, naming the object Go keeps off the heap;
   no plan). A hop re-signs every manifest anyway, so this is the step where a row's legacy label
   retires — full definitions in
   [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md).
   - (a) A reading-run TSV from the landed, red-proved unit extractor (NOTES A14), then the relabel, then rulings,
     then page banking; labels are classed against the plan bar at the first classing.
   - (b) Every structural label is read against its design records.
   - (c) Unit-claiming classes are checked against measured units, and reading-vs-run MOVE rows are listed.
   - (d) Every failure line each signature matches is listed on both OSes; over-absorbing pins are narrowed and
     orphans retired.
     *(Edited in place 2026-10-01, §7 lessons 63 and 69-71: the four sub-steps were added. The relabel took 122
     entries in 24 manifests, six structural pins were reversed, and one pin absorbed five assertions and unbanked
     its row.)*
4. Regenerate the proof page and let the README validation badge recompose from it.
5. **Re-check the per-package deadline floors** in the sweep's long-timeout table — a migration can
   change a suite's cost — re-keyed to the release's relocations and checked against the recon walls (raise when a
   wall passes 0.75 of its floor, never lower), with `src/run-h10-recon.ps1 -SelfTest` passing before pass 1 under
   every launcher's shell (§7 lesson 61 has the command).
   *(Edited in place 2026-10-01, §7 lesson 61: the wrapper's floor and pin defects were found only at pass 3.)*

**Order and launch.** H10 runs: (1) a census of the relocated rows' successors, and a conversion-only `-tests`
pre-stage of them; (2) the recon leg, in a throwaway linked worktree detached at the tip — `-tests -test-action all`
per row, the tree's own wrapper by blob, derived floors (never the 2-minute default), each row's execution pin;
(3) only then the roster seat, the plan and the driver. Every launcher exports the Go overrides per worker and the
.NET pins where its pwsh needs them, asserts pwsh starts under that environment before row 1, greps a `DRIVER_EXIT=`
marker rather than trusting `$LASTEXITCODE`, and excludes the harness's own pwsh hosts from its sibling wait. A linux
READING (`GoTargetOS=linux`, CGO 0, privilege stated) runs alongside the recon leg; the linux BANK stays a separate
leg. It reads and annotates EVERY linux-eligible banked row.
*(Edited in place 2026-09-26, §7 lessons 28-30: seat-before-recon made the generator refuse, launch traps cost a
40-minute false wait and a
parsed-nothing exit 0, and the late linux leg found a linux-only partial on 25 of its 30 FAIL rows.)*
*(Edited in place 2026-10-01, §7 lesson 64: 30 eligible rows were read and never annotated.)*

> **Pre-staging a flagged row — the technique, and it is cheap.** A row the census or the upstream
> survey flags as risky can be answered *before* the campaign reaches it: run the NEW release's test
> suite against the **current** corpus through the real pipeline, with the Go control side on the
> target toolchain (verified by `go version` OUTPUT, per H1, never by a file). Where the package's
> production sources are identical across the two releases — the hand-own census says whether they
> are — only the TESTS differ, so the reading isolates exactly the new assertions and answers three
> questions at once: whether the banked verdicts are safe, whether the new assertions already pass,
> and what the failure *is* if they do not. Two mechanics: the pin guard **refuses** such a run,
> because the corpus is still pinned to the outgoing release — mimic H2 with a worktree-local pin
> bump and restore it afterward — and the answer may be a shape nobody offered. In the recorded
> instance all three dispatched closure options were wrong: the banked verdicts were safe, the
> semantics under suspicion already held, and the real blocker was a **crash** on one debug mode.
> **Pre-staging converts a migration-day surprise into a scheduled piece of work**, which is the
> whole of its value.

**Gate:** the roster's **absolute row count** ≥ the prior migration's (ruled), with rows lost to an
**upstream-deleted package** admitted as **recorded exceptions**. **Both** numbers — absolute and
percentage — are reported every migration, because they can move in opposite directions when a release
adds testable packages faster than validation adds rows.

⚠ **AMENDED 2026-09-22 (the unmeasured-rows rule made concrete, coordinator ruling; ledger 14:45 ·
d5414aa151). "Every roster row re-validates from scratch" has a closing test, and it is taken BY NAME.**
A page census at the batch-7 tree found 25 of 219 rows linking no 1.24.13 page, and **fourteen of them
were BANKED rows never measured at 1.24.13 at all**: the recon leg's 13 NOVERDICT rows at the early tree
0dc65a8e8d (crypto/tls, fmt, internal/coverage/cfile, internal/godebug, internal/runtime/atomic,
internal/trace, math/rand, mime/multipart, net/http, os/user, syscall, testing, unicode/utf8 — several
of them casualties of the `testing` row's contamination of one worker's list), plus net, hand-stopped at
3,792 s. A banked row with no reading at the new release is carry-forward by omission, which this stage
rules out. **H10 closes only when each such row re-banks at the new release or is demoted to a
candidate BY NAME, with the header recomputed by the guard** — never by the row keeping its old count.
The coordinator re-dispatches them with a raised budget; pass 3 (at 8fc439415f) banked three and routed
the rest by class (ledger 16:03 · c660a17d8c).

**One arithmetic cross-check, free and worth running here:** the release census's NAMED IDENTITIES —
tests.csproj, current proof pages, validated badges and index rows, each term named (the close amendment's
"THE RELEASE CENSUS, CORRECTED" states them). A plain "project files equal rows" equality is false by design.
*(Edited in place 2026-09-26, §7 fix A8: this read "must equal the roster's row count".)*

#### Amendment 2026-09-20 (C1) — the LAUNCH, in stage: the order, the dispatch mechanism, the host rule, and the checklist a reader starting here needs

The step above is a per-package procedure and a gate. **It does not say how a campaign of them is
ordered, enumerated, dispatched or hosted**, and at the 1.23 → 1.24 hop every one of those was a
question someone had to answer under time pressure. Written here so the next hop's reader does not.

**THE ORDER, and the two wrong orders are MEASURED refusals rather than cautions.**

```
  RECON LEG  ->  ROSTER SEAT  ->  PLAN  ->  DRIVER
```

The recon leg is the first full pass of **the POPULATION** at the version tip — every row of it, per
package, never the sweep wrapper — and it banks the per-row TSV the map reads. Then the roster seat
lands. Then the plan is emitted from that TSV and the driver runs the campaign's repeated passes.

⚠ **The population is not the roster.** The roster is the BANKED set; a relocated row's successor and
every not-yet-banked row are outside it, and **this leg is the authority on membership**. The two
words are not interchangeable in this block — the sentence below about the map generator says "the
roster file" and is CORRECT, because that is the file the generator enumerates from. The difference
between the two sets is why this leg stays the recon wrapper's: `-Hop` runs the steady-state pass over the
population file, never this leg, unless that is re-ruled.
*(Edited in place 2026-10-01, NOTES-next-hop §6: `-Hop` landed with TRAIN K, b46af2049e.)*

#### The 1.24.13 hop's recon leg, as it actually ran — the readings, cited rather than re-derived

**Complete on three lists, 228 rows, one attempt each.** Recorded here because the next hop's leg is
planned from these numbers and they are otherwise spread across a mailbox.

```
  R   105 rows    85 PASS ·  7 BUILD · 13 NOVERDICT      i9   16 rows   10 PASS · 3 NOVERDICT ·
  G   107 rows   100 PASS ·  4 DIVERGED · 3 BUILD                       2 BUILD · 1 CONVERT
                 ZERO NOVERDICT and ZERO non-integer sweep_s
```

**The sixteen verdict-less rows re-classify from their committed evidence records**, not from the
lane's `diverged` column — the fifth wrapper blob rewrote every real `0` to `n/a`, so that column is
uninformative for the lanes that ran on it. Outcome: **2 PASS recovered** (`math/rand`,
`mime/multipart` — both `matched` with an empty diverged set; the cause was PowerShell 5.1's
case-insensitive JSON reader **throwing** on `Int31n` beside `int31n`), **11 DIVERGED**, **2
NOVERDICT by cause** (the two junction-staged host rows), and **1 for want of evidence** (`testing`,
which carries the hand-own exclusion as its cause and is never scheduled).

⚠ **Two predicate rules the re-classification had to learn, both worth carrying forward:**

```
  a `disclosed` entry is a SENTENCE, not a name -- the name is its LEADING TOKEN, so a membership
     test against the whole string matches nothing and subtracts nothing
  `verdicts` is a SET difference, |go \ names(disclosed)|, never a count difference -- the count
     form assumes disclosed is a SUBSET of go, and the `host-fatal` class names tests that never
     produced a Go verdict at all (runtime/pprof: 6 of 6 absent from its own `go` map)
```

**The banked basis is the three lanes' INTERSECTION, read by column NAME.** G relaunched on a later
wrapper blob and carries an eleventh column the other two lanes do not, so a header-equality check
would refuse the very union the basis exists to be, and a union-and-pad would mint a column that
looks measured and is not. 228 rows in, **213 kept**, 15 excluded as `UNMEASURED` by name, the
hand-stopped row banked at `sweep_s := wall_s` so the generator's drop-must-have-fired assertion can
fire on it. The generator then reads **212 rows parsed, 9,517 s over 212 integer rows**.

**The roster seat that follows it, as landed:** the ten relocation sources retire and **nine**
principal targets bank — nine and not eleven, because one target is the principal for two sources and
two targets are nobody's principal and stay candidates. Each source's 1.23.12 anchor appears exactly
once, on the target taking the majority of its banked **verdicts** (never its declarations: one row
banks 2,195 verdicts from 5 declarations, so a declaration share routes them to a package carrying
none of them). **204 → 203 banked, 32 → 23 candidates.** *(Edited in place 2026-09-26, §7 fix A9: "and the corpus
axis is
226 — a figure independent of how the derivation comes out" struck. The population is the committed enumeration,
checked by two routes; the AMENDED block below shows why.)*

⚠ **AMENDED 2026-09-22 (H10 closing arithmetic, coordinator ruling). "The corpus axis is 226" is
FALSE as a statement about the population, and the way it is false is the lesson: 226 is the
*generator's own axis*, and a generator's axis cannot measure the generator.** `shardmap.py` builds
its axis as *banked-at-seat + costed* — both read from its own inputs — so it closes by construction
and can be wrong by any amount. A package that is neither banked nor carries a measured cost is not
subtracted from that number; it is **invisible to it**, and no reading the map prints can say so.
The enumerated population at `go1.24.13` on the corpus axis (windows/amd64, `-tags
purego,math_big_pure_go`) is **230**, derived twice independently and equal both ways, and the four
members 226 cannot see are `internal/unsafeheader`, `net/http/pprof`, `runtime/pprof` and
`runtime/trace`. Two of those are exclusion-ledger rows and are correctly not dispatched. The other
two — **`net/http/pprof` and `runtime/pprof`** — are ordinary population members, in no shard's plan
and in no unscheduled bucket, and **on the pass-1 map they never run at 1.24.13 at all.**

**The closing identities, as MEASURED at this hop:**

- `212 dispatched + 18 unscheduled = 230`
- `216 banked + 8 candidates + 6 exclusion rows = 230`

**The procedure this amends, for every hop after this one: a population is an ENUMERATION checked by
two INDEPENDENT routes, never a count a generator derives from its own inputs.** The enumeration is
a committed file (`docs/phase4/data/population-go<release>.txt`, an H3 deliverable; `shardmap.py:94` reads the
hopA-inputs copy until NOTES A9 lands); the roster guard
asserts the header's `N` and every banked and excluded row's membership against it, and `shardmap.py`
now refuses when `dispatched + unscheduled` does not close on it, naming every member on each side.
The exclusion ledger takes the same rule the population does: an exclusion is subtractable only from
a set that contains it, which is what struck four E1 rows from the roster's table on the same date.
*(Edited in place 2026-10-01, §7 lesson 52: the path read
`docs/phase4/hopA-inputs/recon-lists/population-go1.24.13.txt`; the roster guard, the sweep's `-Hop` and push-nuget's
preflight read the `docs/phase4/data/` file and refuse without it.)*

⚠ **AMENDED 2026-09-22 (the two live members COSTED, coordinator ruling; ledger 16:31 · 3469154a95;
evidence ledger 11:36 · 553cdcbae3). The closing check above now closes.** The basis
(`docs/phase4/hopA-inputs/recon-basis.tsv`) carries `runtime/pprof` and `net/http/pprof` as COSTED rows
from the i9's measured walls — runtime/pprof 175 s, net/http/pprof 68 s, both DIVERGED at f545b18d4d —
appended from the two result TSVs on `claude/i9-h10-pprof-evidence` 553cdcbae3 with their eleventh column
(`post_s`) dropped to the basis's ten. `shardmap.py --timings recon-basis.tsv` reads **214 dispatched + 14
unscheduled = 228 reached + 2 exclusion-ledger rows (internal/unsafeheader, runtime/trace) = 230** and
exits 0, where it refused before at 212 + 14 = 226 naming the two; they report as candidates (costed,
not banked). **No plan was regenerated**: the committed `h10-dispatch-plan.tsv` still records the pass-1
basis digest (`c3715b2332b5f1ef…`; the basis now reads `6089133e6e3f8144…`), which is correct, since a plan
is the record of the basis it was cut from.

⚠ **A relocated row's disclosure file does not MOVE.** Measured at this hop: there is no tree on which
the move is possible — at master the target directories do not exist, and at the version tip the
source directories are already deleted by the reconvert. **The disclosures retire with the row and are
re-pinned at the successor's re-bank under whatever the declaration is called there**, from a measured
reading, never carried forward blind; `class` and `signature` survive verbatim because the signature
never moved, only the declaration's name did. **No proof file is moved, renamed or created** — a proof
record is the record of a run, and an inherited anchor's record is the SOURCE's.


**The recon leg invokes THE PIPELINE per package** —
`go2cs -tests -test-action all <goroot>/src/<row> <tree>/src/core/<row>` in the worktree at the version tip, whose `src/core` is the
seed — **and not the sweep script**, which is what H10's own line above decides: the sweep is the
steady-state gate and this step invalidates its preconditions by design. One process per worker list
with rows sequential, so the shared build cost falls on the first row: **emit that fact as a column**
so the plan's re-derivation can see which cost carries a build. The driver stays what it is — the
sweep's per-row dispatcher for the campaign's steady-state passes, after rows re-bank, on the costed
plan.

- **Plan before seat** dispatches the relocated rows at their OLD paths, which do not exist at the
  new tip — a per-row failure, late.
- **Seat before recon** makes the map generator **REFUSE**: its population is the roster file, so
  re-pointing rows whose cost is keyed by the old name orphans them, and its own arithmetic
  (`costed + unscheduled == roster rows`) stops closing. Measured at the 1.24 hop:
  `162 costed + 48 unscheduled != 204 roster rows`, naming the six orphans.

**THE DISPATCH MECHANISM.** The campaign is not a loop over the roster. The map generator
(`docs/phase4/hopA-inputs/shardmap.py`) reads the roster for its POPULATION and a banked per-row TSV
for its COSTS, and emits a machine-readable plan; the driver (`src/run-h10-dispatch.ps1`) runs one
worker's rows from that plan, slice by slice, with the ruled cooldown between slices. Both refuse
rather than guess: the driver's `-Plan`, `-Worker` and `-FleetSize` are mandatory with no defaults
(the same worker holds different row sets at different fleet sizes), and a plan whose digest does not
reproduce dispatches nothing. **The recon leg predates the plan and therefore runs from hand-listed
per-worker name lists through the per-package pipeline** — the driver's first use is the costed pass.

⚠ **`testing` is admissible and runs LAST in its list** — the converter seat that refuses its production landed
(AMENDED block below). *(Edited in place 2026-09-26, §7 fix A10: this read "IS NOT A `-tests` ROW AND IS EXCLUDED
FROM EVERY LIST, until the
converter seat that refuses it lands".)* Before that seat it was excluded: it is a wholly hand-owned package, and the
pipeline converts production **in place
with no restore between rows**, so the row converts Go's `testing.go` straight over the hand-owned
host every later row must then compile against. Measured at the 1.24 hop: the row left **14 tracked
files modified and 19 new auto files** beside the 10 marked ones in `src/core/testing`, with CS0111
duplicates — and the **eight** rows from it to the end of that list carry **one contamination event,
not eight readings**. The tell is `unicode`, `unicode/utf8` and `unique` failing to BUILD; rows before
it are unaffected, so the ordering decides how much a list loses. A list that includes it does not
fail loudly — it produces readings, which is why this is a list-construction rule and not a gate.

⚠ **AMENDED 2026-09-22 (the refusing seat has landed, coordinator ruling; ledger 14:45 · d5414aa151).
The precondition above is SATISFIED.** The converter's hand-own guard, `requireConvertibleTestTarget`,
runs a wholly hand-owned host **tests-only** and never emits its production, so the row can no longer
convert `testing.go` over the host. **`testing` is admissible, and it runs LAST in its list**: the
ordering half of the rule stays, since a row that did disturb the tree would still take every later
row with it. Pass 3 dispatched it that way. What remains is a separate converter defect, not this
hazard: the tests-only mode excluded the EXTERNAL `testing_test.go` whole for one bridged reference —
1.24's `export_test.go` adds `const ParallelConflict = parallelConflict` — although the hand-owned host
already declares a public `ParallelConflict` for exactly that binding, so 30 verdicts left the
comparison. Admitting a bridged name the host declares, while still excluding one it does not, is G's
seat (ledger 16:05 · 3469154a95).

**THE TSV the plan is emitted from**, as the generator reads it — `--timings <path>`, no default and
no fallback (an unresolvable path refuses rather than reverting to the other basis); LF only, any CR
refuses; a required header read **by name, never by position**, carrying `row`, `word`, `verdicts`,
`sweep_s`, extras ignored; `sweep_s` must be an integer, because **a row with no measured cost is
UNSCHEDULED and never nominal**; a hand-stopped row is dropped by name **and the drop must fire**, so
the banked TSV must still carry that row; duplicates take the larger and say so; the file's digest is
computed and printed for provenance, not asserted.

**Two of those columns are decided by the leg's own shape and are worth stating here rather than
leaving to the cut.** `sweep_s` is the **CONVERTER's** cost around the **one** pipeline invocation — the
recon leg makes **one attempt per row**, so the re-run inflation that afflicts a re-taking clock cannot
arise by construction, and a row whose oracle is unstable is a READING rather than a retry.
⚠ **`sweep_s` is NOT the wrapper's total wall** — it is the clock taken the moment the converter
call returns, **before any post-processing** — and **the wrapper's own cost lands on NEITHER banked
column**:

```
  sweep_s   the converter's own wall; the clock closes before the comparison document is opened
  wall_s    the SAME number, differing in exactly one circumstance -- it supplies an integer where
            `sweep_s` reads UNMEASURED, which is the hand-stopped row it was added for
  post_s    the wrapper's own seconds, from that clock's close to the row's line being written:
            a TRAILING column ruled 2026-09-20, carried by the wrapper's SEVENTH commit and NOT by
            the three recon TSVs of the 1.24 leg, which the fifth blob emitted
```

**A row whose wrapper time dwarfs its conversion changes how a list is SCHEDULED while changing no
banked cost.** That is why the third column exists instead of being folded into either of the first
two — a cost measured nowhere is this fleet's recurring shape. Where a row banks
`sweep_s := wall_s`, the banked number is the **converter's** wall either way; G's mtime derivation
(row start in the log to the converter's exit, read from the ARTIFACTS) is the **check** on it, and a
completion post states both so a difference becomes a finding about the clock rather than a silent
disagreement.

⚠ **The rule this paragraph's own correction earns, and it applies to every docs seat: a runbook
sentence about a script is read at the SCRIPT'S BLOB before it is written.** A ruling is authority
about what is DECIDED; it is **not** a measurement of what the code DOES. The two join silently when a
ruling's wording is copied into a procedure, which is exactly how the superseded sentence recorded
below got here.

<!-- Dated correction, 2026-09-20 (C1). This paragraph previously read: "The wrapper's own time lands
     on `wall_s`, which is not the banked figure." That was WRONG. It was written the same day, from
     the wording of a ruling (COORD aaf87dd4b) rather than from the script, and it survived one
     landing. i9 read the running blob (c27c0065) and COORD took the correction (787bbf64); C1
     verified the citations independently at the blob before amending here, and the two readings
     agree, so no third was taken.

     Measured in `src/run-h10-recon.ps1` at 8de864a9a9 -- the blob all three recon lists of the
     1.23->1.24 leg actually ran:

         :418   $started = Get-Date
         :422       $output = & $converter -tests -test-action all ...
         :428   $elapsed = [int] ((Get-Date) - $started).TotalSeconds     <- the clock STOPS here
         :460               $jj = Get-Content -LiteralPath $cmpSrc -Raw | ConvertFrom-Json
                                                                          <- post-processing STARTS
         :462/:464          foreach ($p in $jj.go.PSObject.Properties) ...
         :545   $wallS  = $elapsed
         :546   $sweepS = $elapsed
         :551   $sweepS = 'UNMEASURED'
         :557   $sweepS = 'UNMEASURED'

     428 < 460, so $elapsed closes before the comparison document is even opened: BOTH columns are
     that one number and NEITHER carries the post-processing. The occasion was a row whose converter
     exited in ~66 s while its wrapper then held one core for ten minutes over a 4.77 MB comparison
     document (G, add323f4) -- a real cost, which is why post_s was ruled, and which was never in
     either banked column. The wrapper's own header sentence carried the same error and rides the
     seventh commit. -->

`word` is
the leg's **outcome class**, a fixed vocabulary — `PASS` (0 diverged) · `DIVERGED` · `CONVERT` (rc ≠ 0
at convert) · `BUILD` · `TIMEOUT` · `NOVERDICT` (the summary line absent) — and it is **filled, never
placeholdered**: the map generator discards the value, but the column is the basis's only record of
**which verdict a cost was measured under**, and a cost measured under `CONVERT` is not the same
evidence as one measured under `PASS`.

**IS THE LEG ALIVE? A CENSUS BY PROCESS NAME CANNOT ANSWER THAT**, and this is the one question a
watcher asks most often. A converter census (`Get-Process go2cs`, scoped by executable path) answers
*"is a CONVERTER running"* — and that answer is legitimately **0**, for minutes at a time, with no
child process at all, while the wrapper post-processes a completed row. **A wrapper computing for ten
minutes is indistinguishable from a hung one to a name-based census.**

```
  the reading that DOES answer it -- sample the WRAPPER's own PID, twice, a stated wall apart:
      CPU delta over the interval    accumulating  -> computing     flat -> not
      working set                    flat is normal; growth is its own question
      child processes                none is normal AFTER the converter exits
  measured once, 15 s apart:  +15.2 s CPU over 15 s wall, 117 MB flat, no child but a console host
                              -- one core saturated, on a row whose converter had exited 10 min before
```

⚠ **And a row that spends its floor's worth of wall inside the wrapper is NOT a TIMEOUT.** The
per-package deadline floor applies to the converter's test run; what says whether a deadline actually
fired is the **results-file tail**, which states a deadline kill outright (floor 14). Reading a long
wall as a timeout, with no tail read, invents a verdict the run never reported.

⚠ **The liveness census is a READING and must not share a command shape with a kill.** Floor 5
forbids `Get-Process <name> | Stop-Process` because it matches across the whole machine and has taken
a sibling worktree's suite down; a liveness check that is one pipe away from that is an accident
waiting for a tired operator. Scope by executable path for both, and keep the reading and the kill in
separate commands.

**THE POPULATION IS KEYED ON THE CORPUS AXIS — the build tags the pipeline actually converts
under.** An eligibility census taken on a bare platform axis and the corpus's own axis are not
different-but-equal accountings: `resolveBuildTags` applies the stdlib build tags to **every** `-tests`
run unless `-tags` is passed explicitly, because a `-tests` run reconverts the package's PRODUCTION
sources into the test assembly and must select the files the committed corpus was built from. **So a
population keyed on a no-tags axis describes a build the hop will never perform.** At the 1.24 hop this
moved exactly one row — a package whose only surviving test declaration is selected BY the purego tag
and excluded without it — and the general rule it leaves is: **the recon leg is the authority on
membership.** A row that yields zero verdicts under the corpus axis is not a row and is reported by
name; a package outside the enumerated population that the tags select is found the same way.

**THE HOST RULE.** The roster's platform marker records **the platform of the run that banked the
row**, not a requirement on the runner: a row re-banked on windows carries `windows:` at the new
release. A row whose windows reading diverges from its previous platform's bank is re-read on a linux
host at the same tip before it is classified — diverged on both is hop debt, windows-only is a
parity-campaign item disclosed as such with the row's bank being its linux reading, and matched is
banked. **Hop completion is same-platform by construction.** Two row classes are decided by the
package and not by scheduling: a package with no Go files under a platform's build constraints
**cannot be converted there at all** (the converter refuses by name), and its marker reads `n/a`.

⚠ **AMENDED 2026-09-22 (the host rule at the CLOSE, coordinator finding; ledger 16:31 · 3469154a95).
The rule binds at the close, not only mid-campaign.** A row still DIVERGED on windows when H10 closes is
re-read on the linux leg at the same tip **before** it is classified or demoted by name: a demotion
taken without the linux reading decides a platform question on one platform. And a windows pass does
not refresh the roster's linux evidence: **the header's linux line and its 190 `linux:` annotations are
1.23.12 evidence until the linux leg re-reads them at the new tip**, so that leg is a GATING item before
the final figures, not a follow-up. Ruled on C1's linux-annotation sizing (ledger 16:32 · 93cc9e215a):
the leg runs after batch 8b's STAMP, banks only the annotations with the guard re-deriving the header's
linux line, and re-reads on linux every row still diverged on windows.

**THE PRECONDITIONS, which H10 above states only in part.** The step names the four overrides and the
never-the-sweep-wrapper rule; the rest are inherited from earlier stages and are restated here
because a reader starting at H10 gets no pointer to them:

| precondition | where it is ruled |
|:--|:--|
| all four overrides (the Go pair per H1.1; `DOTNET_ROOT` + PATH where the machine SDK lags the TFM) | H10 above |
| the per-package pipeline, **never** `run-validated-sweep.ps1` | H10 above |
| the pin asserted from `go version` **OUTPUT**, never a file, with `GOTOOLCHAIN=local` | H1 — and the target SDK's own `go` answers the MACHINE pin without it |
| the `go` **on PATH** — the one the CONVERTER SPAWNS — itself resolves under the pinned GOROOT, which means **the pinned `bin` is FIRST on PATH**: `GOROOT` does NOT pin the child | i9's defect one: a shell asserting its own `go version` says nothing about what the converter's child process resolves, so the two can disagree silently and the row converts against the wrong toolchain reporting success. ⚠ **A GREEN TOOLCHAIN BANNER IS NOT EVIDENCE ABOUT THE CHILD** — R measured both halves on one box at the 1.24.13 hop: the converter logged `VERSION go1.24.13, read in-process` while the `go` it shelled out to came from PATH and refused with `requires go >= 1.24 (running go 1.23.x; GOTOOLCHAIN=local)`, and separately an ambient `GOROOT` overrode a binary invoked by ABSOLUTE PATH until `GOROOT` was exported. A row's toolchain is pinned in TWO places and the banner reports only the first; the recon wrapper's assertion of the PATH-resolved `go version` is the one that covers the second |
| the output root is the worktree at the tip, whose `src/core` **is** the seed | H5's seeded-root rule; measured in `-tests` form at the 1.24 hop — a hand-own-carrying row converts rc 1 in a bare root and rc 0 seeded |
| ≥ 25 GB free before a battery | H4a |
| one conversion per output root, never two concurrent; one dispatch per worktree | H5 |
| `CGO_ENABLED` pinned to the corpus state, exported rather than assumed | the corpus emission state |
| an entirely hand-owned package converts only under `-test-allow-handown` | the converter's own refusal, by name |
| the leg tree carries **no ignored build residue** before row 1, censused with `git status --ignored=matching` and never `--porcelain` alone, with a control | G measured **516** ignored entries under `src/core` surviving a `git clean -fd`, behind a `status --porcelain` reading **0** — porcelain cannot see an ignored path, so a tree that READS clean is not a tree that IS clean |
| a launcher that WAITS for a sibling battery to clear excludes a `dotnet.exe` whose **command line hosts `pwsh.dll`** | 2026-09-21, the rehearsal: a launcher keyed on `dotnet.exe` alone waited **40 min** on TWO such processes with no battery running at all. A `pwsh` installed as a **dotnet tool** runs as `dotnet.exe … pwsh.dll`, so the harness's own tool shells — including the one the launcher was started from — are indistinguishable from a build by image name, and the predicate waits on itself. Measured again on the i7 the following day: both `dotnet.exe` processes on the box hosted `pwsh.dll` and neither was a battery. Read the COMMAND LINE, which is also floor 5's rule one step on: a census by image name is wrong for the same reason a kill by image name is |
| any **preflight build** — a dry run, a red arm, a rehearsal row — runs in a tree that is **NOT the leg's** | i9 measured `go/types` (443 files) and `net` (672) carrying pre-run build output in a tree where neither had ever been converted: a `dotnet publish` builds a dependency **CLOSURE**, so the at-risk set is everything the arms' closures touched and not the rows that ran |

**THE TREE DISCARD, which is a MEASUREMENT before it is a removal.** The leg's worktree is thrown away
at the end, and until the 1.24 hop the runbook said nothing about what has to be read off it first.
Two lanes found the same class independently on the same night, from opposite ends.

```
  BEFORE row 1   census the tree:  git status --ignored=matching  (never --porcelain alone)
                 with a CONTROL:   the same predicate on a never-built tree must answer 0
                 record the number. A non-zero BEFORE is not a stop -- it is a reading the
                 completion post carries, and it decides whether the arm below is owed.
  AT THE DISCARD re-census, and assert the TRACKED count ACROSS the removal:
                 git ls-files | wc -l   before  ==  after
                 A removal that takes a tracked file is the failure this assert exists for, and
                 it is the same hazard as floor 8's `git status --porcelain | grep '^ D'`, which
                 still runs: one guards what a glob DELETED, this one guards what a clean REMOVED.
```

⚠ **THE INVERSE OF A LEG IS SCOPED TO THE TREE, NOT TO `src/core` — and the example in the rule above
is what made that easy to miss.** `Generated/` under `src/core` is where the bulk of the untracked
output is, so a cleanup written from that example stops at `src/core` and leaves the rest. **The leg
also writes outside it.** Measured at the 1.24.13 hop by G, cleaning for a 107-row list with the
tracked count asserted at **14,485 either side**:

```
  comparison/results records (every row re-ran, so all were residue)   82 across 41 packages
  ignored leg output under src/core                                 1,052
  untracked-not-ignored under src/core                                 65
  LEG FILES OUTSIDE src/core                                            5   docs/validation/current/*.md
```

⚠ **And it is not one lane's quirk**: i9 found the same class independently in **two** of its own
throwaway trees — 4 `docs/validation` paths in one and 2 in another, written by converter runs for
unrelated arms — and noticed only once G had named it. **Two lanes, two machines, the same
out-of-scope writes.** So the removal-by-name list is built from a census of the WHOLE TREE, and
`docs/validation/current/` is named here as the known second location rather than left to be
rediscovered a third time.


⚠ **Why `--porcelain` alone is the wrong instrument here, stated as the measurement and not as
advice.** `git clean -fd` does not remove ignored paths and `status --porcelain` does not report
them, so the two agree on **0** over a tree holding hundreds of files of prior build output. G's
number was **516**. A leg relaunched into such a tree is not a relaunch into a clean tree, and
nothing in the reading says so.

⚠ **AND IN WINDOWS POWERSHELL THE CENSUS NEEDS ITS ENCODING SET, OR IT MEASURES A SMALLER TREE IN
SILENCE.** PS 5.1 decodes a native command's stdout with the **console codepage**, so `git`'s UTF-8
path bytes for the corpus's `Δ`, `ж`, `Ꮡ` and `ˢ` names mangle on the way in and those paths read as
**missing** — a smaller population, censused clean, with nothing in the output saying a byte was lost.
Set `[Console]::OutputEncoding` to UTF-8 around **every** `git` call in Windows PowerShell, not only
this one. The residue answer this was found on stands at **ZERO over 96,792 files with 0
unresolvable**; it is the instrument that was wrong, not the tree, and an unset codepage is the shape
that would have made a dirty tree read clean.

⚠ **And the at-risk set is the dependency CLOSURE, not the rows that ran** — which is the half that
surprises. i9's own census corrected i9's first hypothesis: `go/types` and `net` carried pre-run
`bin`/`obj` although neither had ever been converted in that tree, because a `dotnet publish` for one
row's arms builds much of `src/core`. So "only rows 1–4 ran, so only rows 1–4 are exposed" is false
by construction, and a preflight build belongs in a different tree for exactly that reason.

**When the BEFORE census is non-zero, the leg owes a two-row arm before its TSV is pushed** (ruled at
the 1.24 hop): after the list completes and **before** the tree is discarded, cut a SECOND throwaway
worktree detached at the same tip, build the converter in it, and re-run two rows there — **the
heaviest-residue PASS row and one first-in-tree row** — comparing **word, verdict count and the
diverged set** against the leg's. A match on both retires the exposure and the TSV pushes with the
arm's reading in the completion post; **any difference is a finding and the list re-runs on the clean
tree.**

⚠ **TWO QUESTIONS ABOUT A RECORD'S TIMESTAMP, AND THEY TAKE TWO DIFFERENT PREDICATES.** Ruled at the
1.24 hop after a wrapper gate and the evidence spec were found reading opposite ones:

```
  "did THIS ROW write it"   the GATE          LastWriteTime, against the ROW's own start
  "was this COPIED in"      the evidence (a)  CreationTime, read on the tree's ORIGINALS
```

An **overwrite leaves `CreationTime` at the original** — NTFS tunnels it back through
delete-and-recreate — so `CreationTime` calls a **freshly rewritten record STALE** and is the wrong
predicate for the gate; `LastWriteTime` is correct whether the converter overwrites or recreates, and
a refusal prints both timestamps and the row's. ⚠ **The run-window read belongs on the ORIGINALS, or
on a copy made with timestamps preserved**: per-row evidence copied with `Copy-Item` gets a **new**
`CreationTime`, so a run-window read over the copies reports the moment each lane ran its evidence
commit rather than when the record was written — three lanes, three wrong answers, all internally
consistent.

⚠ **The argument that residue is benign is an ARGUMENT, and the runbook records it as one so a reader
can refuse it.** It runs: the converter binary is byte-identical and was never rebuilt; the tree tip
never moved; and the converter rewrites every `.cs`, so MSBuild sees this run's timestamps and an
incremental build cannot skip on stale inputs. Each of those three is checkable and together they are
persuasive — but they are a chain of reasoning over three facts, not a reading, and the two-row arm
costs one tree and about three minutes of rows. **Spend it rather than let the basis carry an argument
where a measurement was available.**

**Reclaim children-first, with the PARENT test taken AT THE ACT** (floor 12): a tree whose
`--git-common-dir` equals its `--git-dir` and whose `git worktree list` has more than one row is a
PARENT and is never removed — removing it takes every child with it. The test is taken at the moment
of removal and not from a note made earlier, because a sibling lane can have attached a child in
between.

**THE PER-ROW STEPS** are H10's five above, and two of them are where a hop's roster edits actually
happen: the **verdict count re-derives** (the denominator moves), the **manifest is RE-SIGNED, never
edited** — and since 2026-09-05 a re-derived manifest emits `deferred`/`structural`, so a hop retires
every legacy label — the **proof page regenerates and the badge recomposes from it**, and the
**per-package deadline floors are re-checked**. A cleared relocated row needs five artifacts and
**four of them are `-tests` output**: the roster row is the docs edit, while the green badge, the
proof page, the `.tests.csproj` and the disclosure manifest are all produced by the re-bank. That is
why a relocated row cannot be cleared by a docs commit ahead of its run.

⚠ **And two different units meet in this step, so name which one a number is.** A package whose only
test declarations are benchmarks emits a **complete test project** — `.tests.csproj`, host, manifest,
package info — with **zero converted test source in it**; the declarations go to the manifest as
deferred. A gate asking *"does the `.tests.csproj` exist"* scores that row present. So **"declarations
recorded" and "C# test source produced" are different quantities**, a row can be non-zero in the first
and zero in the second, and a 0-denominator ruling has to travel with the row rather than be inferred
from either count.

<!-- Derivation, 2026-09-20 (C1). Order and the two refusals: COORD 1e2d12a64 §1, on C1's brief
     d0c83ed9 (the seat-before-recon refusal measured in-process against shardmap.py with an
     unmodified copy reproducing the in-tree run byte-identically as the control) and C2's pre-flight
     5ceedaf88. Dispatch mechanism: DESIGN-h10-dispatch-driver.md and the rulings e0d5121e2 §7 /
     4327ab7e1 §2; the mandatory-parameter and digest refusals are read from run-h10-dispatch.ps1's
     own header and parameter block. TSV: shardmap.py parse_timings_tsv, read at master 4d25779a1a.
     Host rule: COORD 1e2d12a64 §3. The n/a class: C2 5ceedaf88 measured the converter's refusal on
     the two windows-only rows; C1 re-measured the platform class of all 227 skeleton rows with
     `go list -e` per GOOS at the pinned 1.24.13 GOROOT under the corpus tags -- 225 both, 2
     windows-only, 0 linux-only, with a fabricated package as the control (the first run read 0 of
     227 on linux because one unresolvable package aborts the listing without -e: an all-zero shape,
     caught by the shape and not the rc). The seeded-root arm in -tests form: C2 5ceedaf88 §7(a),
     `sync` rc 1 bare / rc 0 seeded. The five-artifact bill: C1 436b48795 §6. -->

#### Amendment 2026-09-20 (C1) — the RE-BANK, in stage: the driver, the shard, the per-row act, and how a row BANKS

**This is the step's fourth act and the first three have landed.** The order this section already fixes
— RECON LEG → ROSTER SEAT → PLAN → DRIVER — is complete through the plan, so what follows is the
procedure for the act that re-banks a row, written in stage rather than recorded after it.

⚠ **THE LANDED DISPATCH SCRIPT CANNOT PERFORM THIS STEP, and that is a measurement rather than a
caution.** `src/run-h10-dispatch.ps1` dispatches the **sweep** per row. H10 forbids the sweep for a
re-bank in this section's own words — it is the steady-state gate, enforcing the exact banked count and
a drift-clean corpus, **both of which a re-bank invalidates by design** — and the sweep selects among
BANKED rows, so it cannot reach the 23 candidates or the nine relocation successors **at all**. What is
sound in that script is its plan reader, its digest gate, its mandatory-parameter refusals, its slice
packing and its cooldown; what is wrong is only its per-row body.

**THE DRIVER IS A MODE ON THE LANDED SCRIPT, NOT A SECOND SCRIPT.** `-Mode rebank` keeps the reader,
the gate, the refusals, the packing and the cooldown, and replaces the per-row body with the recon
wrapper's pipeline block; `-Mode sweep` keeps today's behaviour for the steady-state passes this
section already assigns it. A second script would fork the plan reader and the digest gate, which are
the two pieces no lane should hold twice.

⚠⚠ **THE DRIVER'S TREE IS A LINKED WORKTREE *ON A BRANCH*, WHICH IS THE OPPOSITE OF THE RECON
LEG'S AND IS EASY TO READ BACKWARDS FROM THIS SECTION.** The recon leg's tree is **detached and
thrown away** — the paragraphs above say so, and its wrapper REFUSES a tree whose HEAD is on a
branch, precisely so nothing can be committed from it by habit. **The re-bank's tree is the
opposite by ruling**: its artifacts are BANKED and committed from it, so it is linked *and on a
branch*. The linked-worktree guard, the census, the tracked-count assert and the one-worktree-per-cut
floor are all kept; it is only the detached requirement that inverts.

⚠ **So the driver must say so explicitly, and the refusal must stay the default.** The wrapper the
driver invokes per row carries an opt-in switch for exactly this case, defaulting to the refusal, and
a run that uses it **reports it in the leg's own output** rather than passing silently — a caller
that turns a guard off leaves a trace a reader of the log can see. **A reader checking that switch
against this section should find both halves here**: the recon leg's detached tree above, and this
paragraph.

**THE PER-ROW ACT** is the recon wrapper's invocation, unchanged:

```
go2cs -tests -test-action all -test-config Release -test-timeout <floor> \
      -go2cspath <tree>/src  <goroot>/src/<row>  <tree>/src/core/<row>
```

- ⚠ **The output directory is the SECOND positional** (floor 3), and it is the worktree whose
  `src/core` **is the seed** — a hand-own-carrying row converts rc 1 bare and rc 0 seeded, measured in
  `-tests` form.
- **`-test-timeout` is DERIVED from the sweep's own long-timeout table, never copied** — a copied list
  has drifted twice — with a relocated row's floor **fanned out to its successors**, and a derivation
  reading fewer than five entries must refuse. Floors are floors: raise for a slower box, never lower.
- **No `-tags`** (the corpus axis arrives by doing nothing, and a no-tags population describes a build
  the hop will never perform); **no `-test-filter`** (a filtered run publishes no artifacts); **no
  `-test-allow-handown`** — it short-circuits the host check and is what destroyed the `testing` row.
- **Capture the exit code on the very next line**, before any pipe or command substitution (floor 7).
  Keep `2>&1` — the classifier reads stderr to separate CONVERT from BUILD — lower the error preference
  around that call alone, restore it in a `finally`, and reset the code per row.

⚠ **AMENDED 2026-09-22 (two wrapper rules measured at pass 3, coordinator rulings; ledger 14:52 ·
158ce37f6c and 16:17 · 3469154a95). Both fixes ride the i7 wrapper instrument seat.**

- **A derived floor never LOWERS an asked `-TestTimeout`: the row runs at `max(asked, floor)`.** "Floors
  are floors" above says raise and never lower, and the sweep's own `$raisesTheFloor` takes the larger;
  `src/run-h10-recon.ps1` (blob 158ce37f6c, line 829) let a row's floor REPLACE an asked raise instead —
  crypto/tls would have run at 30m where 60m was asked, net/http at 60m where 90m was. G's per-run copy
  took the maximum, ruled correct.
- **Every row runs with its roster `execution:` pin, exactly as the sweep does** (`_roster.ps1`'s
  `Get-RosterExecutionArgs`: `release-tiered` → `-test-config Release -test-tiered`, `release-tc0` →
  `-test-config Release`). The same wrapper passes `-test-config $TestConfig` and an empty extra for every
  row, so a `release-tiered` row ran at Release+TC0 — and those rows carry the pin precisely because they
  fail there (`CENSUS-release-tc0-delta.md` §2 and its amendment: internal/godebug, log/slog, net/http).
  Measured at pass 3: internal/godebug's TestCmdBisect read bisect's source lines one low, and net/http's
  TestRegisterErr//a lost its registration site ("registered at unknown location"). The position map
  decodes to Go's lines exactly at both platforms' emission, so neither is a converter defect. **A reading
  taken without the row's pin is not that row's reading**; both rows are re-read with it.

**THE WORDS are the fixed vocabulary, filled and never placeholdered**: `PASS` banks; `DIVERGED` mints
or re-signs disclosures, then banks or routes; `CONVERT` and `BUILD` bank nothing and owe a sizing;
`TIMEOUT` re-dispatches at a raised budget; `NOVERDICT` is recorded by cause. ⚠ **A long wall is not a
TIMEOUT** — only the results-file tail says a deadline fired (floor 14) — and ⚠ **NOVERDICT banks no
count**: it reads `NOMATCH`, never 0, with the cause written beside the row. At the end of pass 1, run the page
census and the population identity (enumerated two ways), and re-dispatch every NOVERDICT or hand-stopped row at a
raised budget.
*(Edited in place 2026-10-01, §7 lesson 62: two rows were in no shard plan and 14 banked rows had never been
measured, which cost a whole pass 3.)*

**HOW A ROW BANKS.** Each worker cuts **one lane ref per shard** off the version tip, carrying that
shard's artifacts **only** — never `docs/validation/index.md`, never the roster header. The coordinator
merges the refs as **incremental trains, one leg at a time**, re-asserting the checksum after each, so
that a red row names its own shard. ⚠ **The roster's figures are DERIVED and never hand-set**:
`src/check-roster-format.ps1` recomputes the header from the table — validated count against row count,
verdicts against the Tests column, disclosed against the Disclosed column, the percentage following
from those — so a worker edits rows and the coordinator takes the header from the guard. **The shard checklist, on
every ref:** test artifacts only, from a named list; rewritten production `.cs` restored; rowless PASS artifacts
carried; staged GOROOT fixtures committed; Tests = MATCHED; any host capability the count depends on recorded and
registered as host-conditional; every cited SHA pushed and read back by `ls-remote`.
*(Edited in place 2026-10-01, §7 lessons 66, 67 and 72: 6 to 61 production files were restored per shard, a page
cited a commit that was never on origin, and os banked at a count only a privileged host reads.)*

⚠ **AMENDED 2026-09-22 (what the Tests cell holds, coordinator correction; ledger 15:42 · 3469154a95).
The Tests cell is the page's MATCHED count, never matched + disclosed.** Disclosed verdicts have their
own column, and the header's "matching" figure is the sum of the Tests cells, so a cell that adds them
counts disclosed verdicts as matching. Batch 7's three new cells did — crypto/internal/fips140test 2267
for the page's 2260 + 7, crypto/sha3 23 for 18 + 5, internal/runtime/maps 111 for 3 + 108 — overstating
the header by 120, and four older rows disagreed with their own 1.24.13 pages on Disclosed. **The guard
enforces it**: `check-roster-format.ps1` section 2f (C1's `claude/c1-own-page-cells` 51a1d30ff9, accepted
for batch 8b; ledger 15:49) requires, for every row whose FIRST `[proof]` page was generated at the pin
read from `src/version.props`, Tests == the page's matched and Disclosed == the page's disclosed. Rows
whose first page is older are counted, never gated. A figure set in a brief is a prediction; the page is
the reading.

⚠ **`docs/validation/index.md` IS ONE SHARED FILE EVERY ROW REWRITES, so no lane ref may carry it.**
Measured on a one-row dry run: re-banking a single row's proof page **removed 25 lines from the shared
index**. It is regenerated **once, centrally, after the last leg**, and its row count is asserted
against the roster.

**THE RESUME LEDGER is part of the driver, not a habit.** Append-only and idempotent, keyed on the
corpus commit, the converter commit and the converter binary's mtime, so a worker resuming mid-shard
re-enters at the first unrecorded row rather than at the first row. ⚠ **A row that is in the plan and
in no shard's ledger is this campaign's one unrecoverable failure mode — it is a gate, not a hope**, and
the closing arithmetic that catches it is `dispatched + unscheduled == the population` and
`banked + candidates + named debt == the population`.

**THE TSV** keeps the leg's eleven columns as its floor, read **by name and never by position**, LF
only, with `sweep_s` an integer or the row is unscheduled. The driver adds `W`, `worker`, `slice` and
`seq` — which the landed dispatcher already emits — plus `banked` ∈ {yes, no, debt} and `manifest_pins`.
⚠ `sweep_s` is the **converter's** wall, closed before any artifact is read, and `post_s` is the
wrapper's own seconds; the wrapper's cost lands on neither.

**THE SHARD SIZE IS W=4 where the Windows side of the second laptop preflights green, and W=3 is the
fallback.** W=3 drops that worker, moves its rows onto the three remaining, adds a third slice and a
second cooldown gap per worker, and costs about a quarter-hour of makespan. ⚠ **The makespan figures
are LOWER BOUNDS and the plan file says so by carrying no makespan line at all** — the numbers live in
the generator's stdout, and a reader who finds them in the plan is reading something else.

⚠ **THE RESERVED SET IS DERIVED, PINNED TO THE FASTEST WORKER, AND NOT SILENTLY ABSORBED.** It comes
from the sweep's own long-timeout table rather than a typed list, and where a declared-reserved row has
no cost it is **unpinnable and stays named** instead of being dropped.

**THE REHEARSAL IS ONE SHARD, ON A WINDOWS BOX, AT THE VERSION TIP AFTER THE APPLY BATCH**, and a
dry run of that same shard precedes it because the script has never executed. The shard is chosen so the
riskiest artifact lands **first**: a relocated principal target at seq 1, and a row with committed pins
to exercise the re-sign path. ⚠ **No shard of any size carries all three required classes**, so the
missing one is **grafted by name** — which is also the five-pin mint site and the single
highest-risk act of the campaign.

⚠⚠ **AND THE GRAFTED ROW IS IN THE PLAN, NOT OFF IT — read the plan before grafting.** The mint-site
row is assigned at **both** sizes: at W=4 to the fastest worker's slice 1 and at W=3 to the
coordinator's slice 1, each under its own sequence number. So grafting it onto a rehearsal shard that
belongs to a different worker does not add an unassigned row — **it hands one worker a row the plan
gives to another**, and the duplicate is real. The rehearsal must either take the row from the worker
the plan assigns it to, or record the graft as a **REASSIGNMENT** against that plan row; what it must
not do is call the row off-plan, because then nothing reconciles it. ⚠ **The train's
every-row-exactly-once checksum is the gate that catches this**, and it can only catch it once two
shard refs exist — which is later and dearer than one look at the plan file now.

⚠⚠ **AND THE RESOLUTION IS THAT THE REHEARSAL DOES NOT BANK THE GRAFTED ROW AT ALL.** It runs there
as **EVIDENCE ONLY** — its TSV line, its minted disclosure file and its artifacts captured to the
rehearsal's scratch and posted as a **prediction** — and is never committed from the rehearsal. The
worker the plan assigns it to banks it in the campaign at its own sequence number, and **the minted
file there must EQUAL the rehearsal's by content**: the mint is deterministic, so a difference is a
finding rather than a discrepancy to reconcile. The row is therefore **reassigned for the rehearsal,
non-banking** — which is the wording to use, because it names both halves.

⚠ **The driver grows NO "banked elsewhere" skip.** The plan stays the single source of who banks
what, and the every-row-exactly-once arithmetic keeps its meaning precisely because nothing in the
driver is allowed to except a row from it. **The rehearsal's own native rows DO bank if it is green**
— they are that worker's shard and its campaign run starts at the next slice — and **a red rehearsal
banks nothing at all.**

⚠ **A HOLDER IS A PROPERTY OF `W`, so every claim about one names its `W`.** The same row sits under
different workers at different fleet sizes, and a sentence that names a holder without naming the size
is not checkable — which is how the off-plan reading survived its first two readers.

**THE ACCEPTANCE PREDICATE IS FIVE DECIDABLE CLAUSES** — the shard's TSV complete with every word
filled; the four artifacts present per row and **written by that row**; the format gate at 0 with its
relocation orphans either cleared or **named as hop debt**; the roster header re-deriving; and the mint
site's manifest carrying its pins, each with a class and a signature, every deferred entry carrying its
want, its reading and its plan. ⚠ **Assert an artifact's freshness by LAST WRITE TIME, never by
creation time** — NTFS tunnels creation time back through a delete-and-recreate and will call a freshly
rewritten record stale. ⚠ **And a gate never made to fail proves nothing** (floor 13): remove one pin
from the rehearsal's manifest, confirm the format gate names that row, restore, verify byte-identical.

**Amended 2026-09-21 (COORD, from the rehearsal):** three of those clauses are read differently than
the sentence above them implies, and each was measured rather than reasoned.

1. **The freshness clause reads FRESH *or* TRACKED-AND-UNMODIFIED, never freshness alone.** The badge
   writer is idempotent by construction — `writeReadmeFile` writes only `if needToWriteFile(...)`, the
   same skip the layout reconciler's comment rests on (`platformEmit.go`: *"emitted" means "the bytes
   changed"*) — so a row whose counts did not move rewrites nothing and its README keeps a
   `LastWriteTime` older than the row's start. On the rehearsal shard **5 of 7 READMEs** were in
   exactly that state. A predicate reading only the timestamp calls them stale and a correct shard
   reds; a predicate reading only trackedness cannot see a record a run failed to rewrite. Both
   halves, disjoined, are what the clause means. (Last-write-time over creation time is unchanged, and
   for the NTFS reason already stated.)
2. **`go2cs_test_disclosures.json` is HAND-OWNED and is never minted by a run**, so "mint site"
   language about it is wrong wherever it appears. `src/core/.gitignore` states the rule at the file
   it protects — the manifest is *"deliberately NOT listed here … authored and committed like source,
   never regenerated"*, unlike the three per-run `go2cs_test_*.json` artifacts beside it that ARE
   ignored. A DIVERGED row's pins are **AUTHORED from that row's divergence evidence**, by a human
   reading the comparison, and the thing that BINDS them to the roster is the format gate's
   **pin-count check** (`check-roster-format.ps1` §2b): a row's largest per-platform `Disclosed` claim
   may not exceed what its manifest can account for. Until 2026-09-22 that check asked only whether
   the FILE existed — deleting one of `database/sql`'s two pins left it silent — so before that date
   the acceptance predicate's manifest clause was weaker than it reads.
3. **A shard's banking path list is the row's OWN directory MINUS its sub-package directories, plus
   that row's proof page — and never `docs/validation/index.md`**, which the paragraph above already
   excludes for its own reason. The subtraction is not tidiness: converting `crypto` mirrors GOROOT
   testdata **recursively** into sibling packages' `testdata/` directories — on the rehearsal,
   **28 files** under `crypto/x509/testdata` (including the nist-pkits set) and **8** under
   `crypto/tls/testdata`, every one byte-identical to its GOROOT source. That is RESIDUE of the
   parent's conversion, not `crypto`'s artifacts and not evidence about `x509` or `tls`, and a shard
   that banks its row's directory wholesale carries another row's testdata into the train.

⚠ **A REFUSAL THAT NAMES A HOP DEBT IS A PASS; A FABRICATED PROOF RECORD IS NOT.** The relocation
targets link their **source's** existing proof record — no proof file is moved, renamed or created — so
the file count exceeds the row count by design. Where the format gate refuses a row on that basis the
refusal is the gate being right, and the driver retires the row rather than inventing a record to turn
the gate green.

**A RED REHEARSAL RETURNS TO ITS CAUSE AND NOTHING FROM IT BANKS**: a driver defect re-cuts the driver
and re-rehearses **on the same shard**, because the shard is the control and a second one measures a
different thing; a converter, generator or runtime defect goes to that seat's lane and the campaign does
not launch; a plan defect re-emits the plan and restarts the rehearsal from its first row.

<!-- Provenance. The draft this amendment is folded from: claude/coord-h10-driver-brief-draft
3d8e522b57, docs/phase4/DRAFT-h10-driver-brief.md, 311 lines, sections A-G, drafted on the coordinator
box by sub-agent from this section's H10, the landed plan and the recon wrapper, every item tagged
RUNBOOK-SOURCED, RULED or PROPOSED. The ten open questions it raised were ruled at mailbox 0cb09c3 and
are folded here in that form: (1) per-shard lane refs off the version tip carrying artifacts only,
merged as incremental trains, roster figures derived by the format tool; (2) the shared index excluded
from every lane ref and regenerated once centrally with its row count asserted; (3) -Mode rebank on the
landed script with no second script, cut by the wrapper's author; (4) the worktree linked and on a
branch, with the linked-worktree guard, census, tracked-count assert and floor 11 kept; (5) W=4 with
W=3 the fallback; (6) the rehearsal shard = the coordinator's W=4 slice 1 plus the mint-site row
grafted by name; (7) the acceptance predicate as written plus the deliberate-regression control, an
orphan refusal naming a hop debt being a PASS; (8) the TSV gains W, worker, slice, seq, banked and
manifest_pins; (9) -SkipBuild after row 1 kept, a worker resuming mid-shard rebuilding on its first
row; (10) the idempotent append-only resume ledger folded into the driver seat. The central finding
that the landed script dispatches the sweep is the draft's, verified here against this section's own
never-the-sweep-wrapper sentence and against the recon wrapper's header. The one-row index measurement
and the seeded-root arm are cited from the readings already recorded in this section rather than
re-derived. A read and a docs act: this lane has no .NET and no PowerShell, so no figure here was
produced by running the driver, the format gate or the sweep. -->

#### Amendment 2026-09-23 (COORD) — the CLOSE, in stage: the checklist as this hop executes it, its gates, and three lessons for the next hop

H10's gate says **when** the step closes: every banked row re-banks at the new release or is demoted
to a candidate BY NAME, the guard derives the header, and both numbers are reported. **It does not
say how a close is carried out**, and until today this section had no close checklist. At the
1.23 → 1.24 hop the close was put together from this section's gates, the ledger's rulings and the
handover record. It runs as two parts with a coordinator checkpoint between them
(`docs/phase4/briefs/batch8g-and-close-brief.md` and `h10-close-obligations.md` on
`claude/coord-handover`; ledger 2026-09-23 13:36 · `3ec2c9ff39`). It is written here in stage, while
the seat runs, so the next hop starts from a checklist rather than a derivation. **The runbook
leads:** where a brief and this block disagree, the brief stops and quotes both.

```
  LAST PRE-CLOSE BATCH -> CHECKPOINT -> refs + controls -> the demoted row's close reading -> demotions
  -> core-refs sweep -> FREEZE -> final seeded REGEN -> post-regen gates -> figures -> index once
  -> closing checks -> CLOSE STAMP (announced before the push)
```

1. **The last pre-close batch** banks every row that can still re-bank. At this hop that was batch 8g,
   stamped at 14:39 · `faaa8fe999`. It banked crypto/tls from a raised-wall run, as the owner ruled,
   with its test artifacts re-emitted convert-only at the merged tree. It also carried net's and
   crypto/tls's linux annotations and two deadline floors. *Gate:* the guard is green, and no row
   carries two Go versions.
2. **The checkpoint (coordinator).** Before the close seat's prompt is cut, three things are true:
   - Every ruling the seat consumes is filled. The seat stops on any that is missing.
   - The host rule is met for each row about to be demoted: either a linux re-read at the same tip,
     or a ruling that a named earlier reading suffices.
   - Each close precondition is either carried as a docs-only ref in the seat's base, or named
     together with the ref that lands it before the close STAMP. The preconditions are: the BOARD
     finding for the hop's open label questions, any attribution owed "before the close", and this
     amendment.
3. **Refs, then controls, before any close edit.**
   - Merge the docs refs (ALLOWED: `docs/**` only), then run the guard as a control.
   - In the leg's tree, record three readings: shardmap closes, the core-references guard's
     `declared · measured · retired` line, and the index tool's self-test plus a dry run.
   - Run the build controls in a CONTROL worktree, never in the leg's (the preflight-build rule
     above): the steady-state sweep of one row whose production the regen will move (the control for
     the overlay's effect), GolibTests, and the full behavioral suite.
   - Then take the ignored-residue census BEFORE the reading, with its control, and clear it to 0.
     Build the converter. The census must now show exactly ONE `!!` row, the converter binary.
4. **The demoted row's close reading.** Read the row once more at the close head:
   - Go through the recon wrapper, with the row's roster `execution:` pin applied and `-AllowBranch`
     reported, because the tree is on a branch. A DIVERGED run publishes no page.
   - Keep the re-emitted TEST artifacts as candidate evidence. That means the ruled set, with Go's
     `testdata/**` byte-equal to GOROOT's, taken from the row's own directory minus its sub-packages.
   - Restore every production re-emission, the README and any `docs/validation/**` write, and count
     each by class.
   - ⚠ **A `-tests` run can REMOVE a generated test artifact it no longer emits.** Admit that removal
     by name with `git rm` (the precedent is ledger 2026-09-22 04:14). Any other deletion stops the
     seat.
5. **Demotion BY NAME: the mechanics.** *Gate:* until the figures step, the guard's failure set is
   EXACTLY the header and NEWS figure arms, and every membership arm is green.
   - **The roster row** is deleted whole. A DATED SUCCESSOR of the closing-identities block goes
     above the old block, which stays verbatim. The successor carries the candidate table in PLAIN
     code spans, never the linked-row shape the parser reads as a row. Each candidate's cell names its
     classes as the close reading worded them, its evidence, the execution pin it carries on re-entry,
     its previous-release record in the write-once snapshot, and its re-entry path.
   - **The current proof page** (`docs/validation/current/<row>.md`) is removed with `git rm`. While
     it exists, the badge writer composes GREEN from it, and the index tool refuses a page that no row
     names. The previous-release page survives in the write-once snapshot. This is not the relocation
     rule's "no proof file is moved, renamed or created", which governs relocated rows.
   - **The README Tests badge** is hand-set to the converter's exact orange form
     (`readmeValidationBadge.go:455-456`). This is a PREDICTION, and the regen scores it byte for byte.
   - **A COSTED recon-basis row** is appended. See lesson 1 below.
   - **A demoted linux ANNOTATION.** Here the windows bank stands but the linux oracle is broken. The
     annotation is removed by name, and one dated sentence names the oracle's state and any state
     change between the banked read and the failing read. At this hop: go/internal/srcimporter, whose
     1.23.12 annotation was derived with cgo ON and whose leg read ran with cgo OFF.
6. **The core-references table is swept to zero BEFORE the freeze**, because the sweep edits converter
   test source (floor 4). The guard first reads `declared N · measured 0 · retired N`. After the sweep
   it reads `declared 0 · measured 0`. A planted stale `ProjectReference` must fail by name.
7. **The freeze.** The tree is clean, and a census shows zero `!!` rows under `src/core`, `src/gen` and
   `docs/validation`, proven by a plant. There is one named exception: `go generate .`, run after both
   conversions have finished.
8. **The hop's final seeded `-stdlib` reconvert: a deliberate same-release REGEN, measured as a
   two-seeded pair.** §6 says a seeded full reconvert happens "once per phase — H4a's bundle and H5".
   This one is a third, ruled on the ledger (2026-09-22 11:10 ruling 2, with 08:18 and 14:49), because
   converter fixes landed after H5 whose corpus footprint no row ref carries: a row ref stages test
   artifacts only.
   - **The two arms.** Arm A is built from the last corpus-wide regen, arm B from the tip. A's base
     must predate every converter change whose footprint never landed. A later base misfiles those
     files as committed drift that neither converter reproduces.
   - **Seeding and running.** Both roots are seeded from ONE `git archive` file, extracted twice before
     either arm converts. The arms run serially, each backgrounded and awaited. If an arm fails, delete
     its root and re-seed it from the same archive. Never convert twice into one root. Then run H5's
     per-arm checks.
   - **Classify by CONTENT** (CR-stripped, `*.cs.auto` included), never by mtime:
     - **B-only** is a converter change that never landed. Attribute each to a named first-parent merge.
     - **A-only** is a change already applied. Count these by merge and never overlay them. ⚠ An EMPTY
       A-only set means the pair measured nothing, so abort.
     - **Files the committed tree disagrees with** go through §4 in order. Test a ruled hand-set line
       before T5 (see below), and plant a T5 before trusting a zero.
   - **H5c** runs on every flavour. `-Apply` only with zero UNRESOLVED and full flavour agreement.
   - **The overlay** copies `.cs`, `.csproj` and `README.md`, never `*.cs.auto`, widened as lesson 2
     says. Then, in order: dispose of the absent-in-stage set (lesson 3); restore T0, T4 and the
     hand-set lines; adopt `go2cs-stdlib.slnx` after the absent set is disposed of; run
     `go generate .`; make ONE commit. After the commit, prove the pins by re-checkout (lesson 2).
   - ⚠ **A HAND-SET LINE UNDER A REGEN.** A regen recomposes every README. A badge the corpus holds
     orange by hand is re-greened when both badge arms agree. At this hop that is
     `crypto/internal/fips140deps`, an exclusion row that keeps a `tests.csproj` AND a current page.
     Restore it after the overlay as a named class. Do not leave it for T5.
9. **Post-regen gates**, each from a per-run copy, serially. Rehearsed at the campaign-tip fold, not first met here.
   *(Edited in place 2026-10-01, §7 lesson 75: the close seat stopped three times, for about 4 h, on R-B13 through
   R-B16.)*
   - the converter suite;
   - CNR, with CHANGED empty, or classified by §4 with zero T5;
   - the stdlib on all three flavours by H7 as amended;
   - GolibTests and the full behavioral suite, each EQUAL by name to step 3's control;
   - `go2cs.slnx`, which is owed after the golib/runtime API changes this hop made, built with
     `-p:go2csPath=<repo>/src/` whenever the configuration is not Debug (R-B16, below);
   - the steady-state sweep of every banked row whose production the regen moved, capped. The rest go
     to §6's full sweep. If the closing box cannot run a row, list it: it is run elsewhere at the close
     head before the STAMP, or deferred to §6 by a named ruling.
10. **Figures by the guard, then the index once.** Write the header and the README NEWS exactly as the
    guard derives them. H10's gate then reports BOTH numbers against the previous anchor, and the
    linux absolute beside them. At this hop the absolute row count is predicted to rise while both
    percentages fall. That opposite-direction case is exactly what the gate exists to report.
    Regenerate the index once, after the last roster edit.
11. **Closing checks.** Each is printed with its count, and each check that can be planted is planted:
    - every row's FIRST proof page is at the new release (a planted old first page is named);
    - every numeric linux annotation equals a new-release linux reading, so the residual set is EMPTY
      (a planted wrong annotation is named);
    - the linux-annotated set equals the eligible set by name;
    - every cited SHA resolves on origin;
    - no accepted seat is unlanded without a named disposition;
    - tracked `testdata` equals the pinned GOROOT (repoguard's `TestTrackedFixturesMatchPinnedGoRoot`, landed with
      TRAIN L at fbaa3d1218; §7 lesson 76 has the by-hand command);
      *(Edited in place 2026-10-01, §7 lessons 41, 64, 66 and 76: the four checks above.)*
      <!-- The fixture clause was re-read at the apply (2026-10-02, e2008427b1). The draft read "TRAIN L's guard once
      fbaa3d1218 lands, until then lesson 76's command"; TRAIN L landed at master aa0a07d5fd. -->
    - shardmap closes;
    - the release census closes as named identities (below);
    - the deadline floors hold against their largest walls;
    - the core-refs table reads 0 / 0;
    - the deletion set is exactly the ruled deletions, by name;
    - the identifier census and repoguard pass, with a plant;
    - the tree is clean;
    - each close precondition is named with the ref that discharges it.

    Then comes the close STAMP.

**THE HEADER IN A LANE REF is a recorded exception, not a new rule.** "How a row banks" (above) says
a worker ref never carries the roster header. At this hop's last batch, COORD accepted a bank ref
that carried guard-derived header and NEWS hunks, and two linux refs that carried the header's linux
line (ledger 2026-09-23 12:36 · `3ec2c9ff39`; recorded at the 8g STAMP 14:39 · `faaa8fe999`). They
merged only as carriers, and the guard's own output overwrote every one. The rule stands: a carried
figure is never trusted, only overwritten.

**THE RELEASE CENSUS, CORRECTED.** The cross-check above reads: "the count of banked test project
files must equal the roster's row count". **That equality is false by design.** Exclusion rows keep
their artifacts, rowless candidates keep theirs, and relocation anchors keep their source pages
(ledger 2026-09-22 16:31 · `3469154a95`: the equality REFUTED, to be restated as a named identity at
the close). **The check is a NAMED IDENTITY, with every term named:**

```
  tracked tests.csproj   =  banked rows + exclusion rows keeping artifacts + rowless candidates keeping artifacts
  current proof pages    =  rows by name + relocation anchors by link + exclusion rows by exclusion
  validated badges       =  banked rows - banked rows with no README (named); an exclusion row that
                            composes GREEN is a hand-set-line restore (step 8), never a badge
  index rows             =  banked rows
```

A migration that ends with the terms unequal has a miscount somewhere, and the identity names the
term. The figures belong to the close STAMP that reads them, not to this block.

**THREE LESSONS FOR THE NEXT HOP.** The close's derivation found each of these before any run
(ledger 13:36), and each is now a step above.

1. **A demoted row needs a COSTED recon-basis row, or shardmap refuses.** The map takes its population
   from the committed enumeration and the roster. A roster row with no cost in the basis is
   UNSCHEDULED (`shardmap.py:416-417`). While the row was banked it was counted as reached. Once it is
   demoted, it is a live population member in NEITHER bucket, and the closing check dies naming it.
   The fix is to append the row's measured cost from its close reading: columns read by name, cut to
   the basis's ten, LF only, and no plan regenerated. This follows the 2026-09-22 pprof precedent
   above. *Red-first:* with the demotion applied and the basis row not yet appended, shardmap must
   refuse and name the row.
2. **Production `//go:embed` payloads must be copied by the reconvert, and they must carry a `-text`
   pin.**
   - *The copy.* The ruled overlay copies `.cs`, `.csproj` and `README.md` only (H5's overlay
     amendment). A payload the regen stages would therefore never reach the tree, while the emitted
     csproj still names it. Widen the overlay by every file an emitted csproj names in
     `<EmbeddedResource Include>`, derived from the emission.
   - *The pin.* Under `src/core`, `.gitattributes` pins only `testdata/**`. On a checkout with
     `core.autocrlf=true`, any other text payload is checked out CRLF where Go embeds LF, so the
     embedded bytes are not Go's. Pin every non-testdata `EmbeddedResource` payload `-text`, derived
     from every tracked and every emitted csproj.
   - *The proof.* PROVE each pin by re-checkout: `rm` the payload, `git checkout HEAD --` it, and
     compare its sha256 with GOROOT's. The control is the same read with the pin line removed from the
     working-tree `.gitattributes`, which must yield CR bytes. A payload with no LF cannot red that
     control; name it.
   - *At go1.24.13*, the only production `//go:embed` in std is `internal/trace/traceviewer`, with two
     payloads. The test-side payloads in `embed/internal/embedtest` and
     `crypto/internal/fips140test` take the same pin.
3. **A flat file the emitter moves to per-GOOS folders is removed with `git rm`.** When a shared
   artifact becomes per-target, the emitter deletes the flat copy in its own staging root
   (`Stale copies removed`, `platformEmit.go:259-295`). The overlay cannot carry a deletion (H5's
   overlay amendment, step 3). Without the removal, the committed flat copy survives beside the new
   per-GOOS copies and declares everything twice. So `git rm` it by name in the overlay commit, and
   only when the overlay adds its per-GOOS copy for ALL THREE targets. Any other absent-in-stage file
   stops the seat. The instance predicted at this hop is `src/core/os/exec.cs`, from batch 3's
   measured footprint that was never committed (ledger 2026-09-22 02:36).

**AMENDED 2026-09-24 (C1, the post-close docs seat, on the close STAMP fa18863b94) -- FOUR LESSONS
FROM THE CLOSE SEAT ITSELF.** The seat stopped three times and each stop was ruled (ledger 2026-09-23
18:11, 18:39, 21:43). Each lesson is a step for the next hop, with the ref that fixes its cause.

4. **The staging seeder carries the whole `docs/validation` tree (R-B13; fix 781c1c3c31).** The
   `-stdlib` and `-platform-census` staging roots are seeded by one seeder (`seedCensusRoot`,
   `platformCensus.go`). The README emitter resolves the published stamp from the snapshot directories
   under `docs/validation`, so a root seeded with `docs/validation/current` alone reads no stamp and
   every package README loses BOTH repository badge lines. On a tree without the fix, restore the
   stripped READMEs from the committed tree as a NAMED class after the overlay; with it, the regen
   writes them byte-identical. Either way, the check after a regen is that no README lost a badge line.
5. **Go's own `//go:embed` payloads outside `testdata` are admitted by repoguard from the csproj that
   embeds them (R-B14; fix de436207c9, merged at the close as f0ada4471e).** A regen can land a Go
   payload outside `testdata` for the first time (at this hop, `internal/trace/traceviewer`'s
   `trace_viewer_full.html`, whose JavaScript regex reads as a network path). repoguard admits a payload
   only when a tracked csproj under `src/core` names it as a converter-minted
   `<EmbeddedResource ... LogicalName="go.embed/...">`, and still runs its denied-token pass on it; the
   bash census mirrors it (33f5f11297). A payload the census refuses is a GUARD gap to route, never a
   hand edit of Go's bytes and never a pathspec exclusion.
6. **The multi-target csproj merge keeps every group that follows the reference block (R-B15; fix
   ae813db069).** When the merge re-renders a staged csproj, the renderer replaces the platform
   reference block by its own extent. Before the fix it cut from the block header to `</Project>` and
   dropped any group after it; at this hop that was log/syslog's shared-LICENSE `<None>` item, restored
   as a named class. On a tree without the fix, diff every merged csproj against the committed one for a
   lost non-reference group; with it, all 22 L3 csprojs round-trip byte-identically.
7. **A non-Debug build of `go2cs.slnx` is pinned to the tree (R-B16).** Every behavioral csproj sets
   `go2csPath` to `$(USERPROFILE)/go2cs/` when the configuration is not Debug, even inside the solution,
   so step 9's `go2cs.slnx` build and §6's row carry `-p:go2csPath=<repo>/src/`. The pinned build is the
   reading of record; an unpinned one measures the deploy root (`.claude/rules/harness-gates.md`, *Build
   context and the `$(go2csPath)` root*).

<!-- Provenance of the four lessons, 2026-09-24 (C1, docs only; no gate named here was run to write
     them). R-B13: ledger 2026-09-23 18:11 (922994cec3) -- the regen's arm B stripped both badges from
     337 package READMEs; restored as class (v); fix 781c1c3c31 accepted 18:23 for the post-close batch
     (red-first TestSeedCensusRootCarriesThePublishedStampTheBadgesRead). R-B14: ledger 18:39
     (1a328f3ee8) -- TestNoFleetIdentifiersInTrackedFiles red on two arms in one payload,
     src/core/internal/trace/traceviewer/static/trace_viewer_full.html:6244; C2's admit de436207c9
     accepted 18:56 and merged by the close as f0ada4471e; the census mirror 33f5f11297 is on master.
     R-B15: ledger 18:39 and 19:01 (ae813db069) -- the renderer, not the merge note; log/syslog restored
     as class (vi); accepted for the post-close batch. R-B16: ledger 21:43 (mailbox 00133ee238) --
     15,240 CS0246/CS0234 across 695 behavioral projects unpinned, rc 0 with 925 assemblies pinned; 698
     of 699 behavioral csprojs carry the Configuration-keyed fallback at fa18863b94. The close STAMP is
     ledger 22:30 (35d9a46970). -->

<!-- Provenance, 2026-09-23 (COORD, written by a docs-only sub-agent on the i7 at the batch-8g stamp
     faaa8fe999). The order and the gates are condensed from the close brief's PART B (B1-B12) and from
     the obligations list's classes (b), (c) and (d), both on claude/coord-handover at eed254e989. Those
     documents were derived by a read-only workflow (three readers, a drafter and two adversarial
     checkers, with 36 defects repaired) from this section's gates and every dated amendment, plus the
     ledger through 12:56 (ledger 13:36 · 3ec2c9ff39). The header exception is recorded at the 8g STAMP
     (ledger 14:39 · faaa8fe999, obligation c17). The release-census correction is ledger 2026-09-22
     16:31 · 3469154a95 (obligations b30 and c5). The three lessons are obligations b6, b11/b12 and b15.
     The hand-set-line and removed-artifact notes are b32 and b33. This is a docs act: no gate named
     here was run to write it. The code citations were read at faaa8fe999: shardmap.py:416-417 (the
     UNSCHEDULED set), platformEmit.go:259-295 (the stale-copy removal), readmeValidationBadge.go:436-446
     (GREEN needs both signals) and :455-456 (the orange form), and .gitattributes (testdata/** is the
     only -text pin under src/core; git check-attr reads the three payloads text: unspecified). The one-production-embed claim was re-read for this block by a grep of
     go1.24.13's std sources, outside cmd/, for //go:embed in non-test files. The close's own figures
     are not written here: step 11 reads them, and the close STAMP records them. -->

### H11 — Publication and compatibility guards **GATE**

- The published version is the pinned Go release plus the build counter, already set at H2.
- **Verify EXISTENCE-PLUS-MONOTONICITY with a scripted comparison before the first publish**, never
  believe it. A non-monotonic sequence on a public feed is not correctable. Whether an unreadable origin may degrade
  to a local-tags comparison is ruled at Readiness and written here (NOTES A11, §5 Q15; `src/push-nuget.ps1` warns and
  compares local tags only). *(Edited in place 2026-10-01, NOTES-next-hop Q15.)*
  > **AMENDED 2026-09-20.** This rung read "verify version monotonicity" until today, and that is
  > TRUE AND INSUFFICIENT: **a counter carried across a base bump is perfectly monotonic.** `0f97dcc8db`
  > is the proof — 1.23.12.3 → 1.24.13.3 increases, and 1.24.13.3 was never published. Monotonicity
  > answers "does the sequence go forwards"; it cannot answer "does this release exist". The second
  > half is the one a hop breaks, and it is the one now held by
  > `TestPublishedCounterMatchesTheRecordedReleases` (`src/go2cs/internal/repoguard`), which asserts the
  > counter against the recorded snapshots rather than against its own previous value. The comparison
  > is NUMERIC per component, never lexical — `1.23.12.10` sorts below `1.23.12.9` as a string, and
  > H2's per-release counter reaches double digits on the tenth publish of a base.
- The NuGet compatibility guard follows the migration for free, because it reads the converter
  binary's own runtime version — which H1 rebuilt. That coupling is exactly the H1↔H2 window §2 warns
  about.
- **New packages need new package IDs; removed packages need a disposition** — ruled: **deprecate,
  with a pointer to the last release that carried it. Never unlist.** The deprecations are an owner step AFTER the
  publish, because each points at an alternate that must already exist on the feed.
- **H11 follows H10 by construction**: its pre-flight cannot pass until every roster row banks at the new base.
  *(Edited in place 2026-09-26, §7 lesson 38: the two lines above.)*
- The published-release stamp is a **repository-recorded fact** written by the publish ritual; a feed
  query is advisory only, never the gate.
  > **ADDED 2026-10-02 (owner-approved).** The converter embeds a second copy of that fact:
  > `src/go2cs/corpus-release.txt`, the **last PUBLISHED** go2cs release (e.g. `1.24.13.3`), which
  > `-recurse=nuget` matches against a published conversion's `go2cs-release`. It is written by
  > `go run ./internal/gencorpusrelease` (from `src/go2cs`) out of `releasestamp.PublishedStamp`, and
  > **regenerated by `release-nuget.ps1` Phase 4**, after the push, joining the record commit. It moves
  > at a PUBLISH, never at the pin: a hop leaves it naming the outgoing release until the new base first
  > publishes, so `migrate-gorelease.ps1` classes it MUST-NOT-CHANGE. A stale constant is refused twice:
  > `TestCorpusReleaseMatchesThePublishedStamp` (`src/go2cs/internal/repoguard`, every `go test ./...`)
  > and `push-nuget.ps1`'s pre-bump gate, which runs that test beside the stdlib-metadata check.

### H12 — Docs, badges, READMEs **GATE**

- **Every validation badge on every package README moves** as a matter of course: two of them read the
  toolchain and follow H1, two follow the PUBLISHED stamp (they move at the publish, not the pin, and are omitted
  when nothing is published). *(Edited in place 2026-09-26, §7 fix A11: this read "read `version.props` and
  follow H2".)* **State the expected diff size
  before the overlay** so it is not mistaken for drift.
  > **AMENDED 2026-09-20 — THE UNPUBLISHED LINE, where the expected diff size is ZERO for half of them.**
  > The two H2-following badges — the **C# Source** badge's tag and the **Tests** badge's proof-page path
  > — do not target `version.props`'s arithmetic. They target the **published stamp** (H11.5: the
  > write-once `docs/validation/<stamp>/` snapshot), resolved by `releasestamp.PublishedStamp`:
  >
  > | at the overlay | the two H2-following badges |
  > |---|---|
  > | the composed `<base>.<counter>` has a snapshot | move to it — the usual case, full diff |
  > | it does not (the hop, before the new base's first publish) | **do not move** — they keep naming the LAST published release |
  > | nothing is published at all | are **omitted** — no honest target exists |
  >
  > So between H2 and the new base's first publish the expected diff is the H1-following badges ALONE,
  > and a lane that stated the full four-badge size will read half of it as a missing overlay. The
  > badges retarget at the publish, not at the pin — which is the only behaviour that leaves a reader
  > clicking a link that resolves.
- **The hand-owned READMEs do not follow.** They are hand-edited, and their edits are *derived and
  proved against the converter's own output*, never typed. **Re-run that derivation as a control** at
  every migration.
- **The GOROOT-vendored `golang.org/x/*` packages re-pin** from the new GOROOT's own vendor manifest —
  on a patch-level migration this is the badge family most likely to actually move.
- The Go version appears in prose under `docs/` (the README, roadmap, roster, background and strategy docs), in the
  hand-owned READMEs and in CLAUDE.md. `migrate-gorelease.ps1` anchors only full-release spellings (about a quarter
  of the edits), so this rung is a scan AND a full read, each reference classed KEEP-HISTORICAL / MIGRATE /
  RERUN-THEN-MIGRATE / GENERATED-CHECK / OWNER-ASK; the Try-it command is timed cold.
  *(Edited in place 2026-09-26, §7 fix A12 and lesson 38: this read "top-level docs".)*
  > **AMENDED 2026-09-20 — "top-level docs" is a NAMING DEFECT in this line, not a file that went
  > missing.** A lane reading it looks for a root `README.md` and finds none; `git ls-tree master` has
  > no root README at all, so nothing was lost in any hop. The prose sites this rung means are the ones
  > under `docs/` plus `CLAUDE.md` — `migrate-gorelease.ps1`'s doc-statement class is the authority on
  > which, and its census names them. **No file is owed here**; the line wants rewording at the next
  > pass, which is recorded rather than done so the amendment stays evidence and not a silent edit.
- **Release-ritual rehearsal**: a dry run exercising the pre-pack signed tag, the write-once proof
  snapshot, both badge retargets, and the recomputed re-verification pass. The frame requires the
  ritual *rehearsed* at the parity gate; whether a given migration also **publishes** is the frame's
  decision, not this document's.

> **The release ritual, DEFINED — so that "rehearsed" names something checkable.** Five elements, in
> this order, and the order is the part that has been paid for:
>
> 1. **The announcement text lands on the branch BEFORE the tag mints.** The `nuget-<version>` tag
>    deliberately mints at the pre-pack point, because the READMEs frozen *inside* the published
>    packages link the tree at that tag — so announcement text applied after the release leaves a
>    visitor browsing AT the tag looking at a NEWS block that predates the announcement, and the tag
>    cannot be moved to fix it: it anchors the exact tree the shipped binaries were built from, and
>    the post-release branch contains merged work those binaries do not. The version the text names
>    is deterministic *before* the bump — it is the version the dry run already computes — and links
>    into the write-once validation snapshot resolve on the live site regardless of which tree a
>    visitor is browsing.
> 2. **The pre-pack signed tag**, minted once and never moved.
> 3. **The write-once proof snapshot** under the release's own validation directory.
> 4. **Both badge retargets** — the proof link half and the source tag-and-message half. Retargeting
>    one is the recorded way to ship a half-migrated badge family.
> 5. **The recomputed re-verification pass**, which is what makes the snapshot evidence rather than a
>    copy.
>
> A rehearsal exercises all five on the migration's own tree. **Signing is mandatory and
> single-machine** — the feed rejects an unsigned push — so a rehearsal proves the ritual, never the
> credential; the credential is proved once, by the machine that holds it.

- **The rehearsal's shape**: the tag mint in an isolated clone with no remote, discarded after; the pack dry run in a
  throwaway worktree (push-nuget writes package READMEs even in a dry run). State the badges that dangle until the
  publish (new or relocated packages have no page in the last published snapshot).
  *(Edited in place 2026-09-26, §7 lesson 38: a pack-only dry run mints no tag, so the ritual's element 2 was
  otherwise unrehearsed.)*

#### Release day

*(Edited in place 2026-09-26, §7 lesson 39: the 1.24.13.1 and 1.24.13.2 releases, both on 2026-09-24.)*

1. **Order.** Master moves first, after §6's full sweep, and the release is cut from master. Nothing that moves
   golib or runtime lands between the sweep and the release. `release/go1.<old>` fast-forwards to the outgoing
   line's final release tag; `release/go1.<new>` is minted only at the next cutover.
2. **The owner's gate** is the README walkthrough on linux AND windows, from packages repacked at the tree that
   ships (it caught a windows-only compile surface); GolibTests is gated at Debug AND Release.
   **A release that ships macOS assets adds the mac half, CI-only** *(added 2026-10-03, the 1.24.13.4 macOS
   seat)*: the darwin `census` green AT the tree that ships (the daily schedule lags master by up to a day,
   so dispatch one when the tip landed after it), and a `release-smoke` dispatch with `goos=darwin` at that
   tree — its windows pack is the only Release-configuration darwin build, and both Mac legs must pass arms
   A–C before the PIN. Arm D (the walkthrough on a Mac) is a measurement: until it reads green, the README
   and the release notes do not say the walkthrough works on macOS.
3. **The publish** is the owner's act at the release machine's own console, from a checkout with empty porcelain
   (an agent's credential checks are refused by the permission classifier). **No GPG signing on the release
   machine during NuGet Phase 2, or `disable-scdaemon` in gpg-agent.conf when no GPG key lives on a card** (the
   owner's call): the likely cause was a woken scdaemon resetting the signing card's session per package.
4. **To resume a failed Phase 2, NEVER re-run release-nuget** — it re-bumps the version. Run the signer's census,
   then the signer with `-Apply -Overwrite` (one PIN), then Phase 3's exact push. Nothing is re-packed.
5. **After the publish**, "indexed" means every id in the walkthrough's restore closure is listed — a partial
   index fails restore loudly (NU1102). Then the post-publish smoke, the removed-ID deprecations, the release record.

---

## 3. The roster re-derivation as a shardable campaign

H10 is embarrassingly parallel and should be run that way whenever more than one machine is available.
This section is the standing procedure; a given migration's plan supplies the fleet and the map.

### 3.1 Why it shards, and what the unit of isolation is

Each row is an independent conversion-build-run-compare against one GOROOT package and one corpus
package. **No row reads another row's output.** What rows share is one corpus tree and one converter
binary — so **the unit of isolation is a clone or worktree, not a directory**. Two lanes on one
machine need separate checkouts, because the gates re-transpile the tree they run in.

⚠ **The validated sweep is SERIAL BY DESIGN** and says so in its own source: concurrent converted-test
runs share freshly-built dependency assemblies and collide on them, *which reads as a package failure
and is not one*. It exposes no jobs, throttle, shard or resume parameter. **Every unit of fleet
concurrency therefore lives outside the instrument** and is a worktree running its own internally
serial sweep. The per-row driver invokes the sweep one row at a time with its **exact-match filter** —
a parameter that exists in the sweep for precisely this purpose, because a substring filter re-sweeps
large rows repeatedly.

### 3.2 Cost proxy and ordering

**Verdict count is a bad cost proxy.** Suites with few verdicts can dominate a campaign (large fixture
streaming, spawned child toolchains), and suites with enormous verdict counts can be quick. **The
honest proxy is the previous full sweep's per-row wall time** — which means **per-row log retention on
the preceding consolidation sweep is a prerequisite of the next migration's shard map**, and is
unrecoverable afterward. Make it an obligation of that sweep, not of this step. The sweep's per-row TSV is committed
as a dated (OS, SHA, machine) section of `docs/phase4/DATA-sweep-row-walltimes.md`; the sweep writes it under a
git-ignored scratchpad (NOTES A4). *(Edited in place 2026-10-01, NOTES-next-hop A4.)*

**Smallest-first is the established ordering, and its reason is banking**: partial results bank as the
campaign goes, and the coordinator merges incrementally rather than waiting for the whole run. Its one
cost is makespan — a long row landing last idles every other worker.

Where a migration has several dominant rows, a **two-phase** ordering resolves the tension without
giving up incremental banking:

- **recon, smallest-first** — every worker takes a stratified slice of the cheapest rows. Deliverable:
  partial banks *and* the migration's **drift families**, named, before any expensive row runs.
- **bulk, largest-first** — greedy longest-processing-time-first assignment onto bins weighted by
  measured worker speed, which is the standard makespan heuristic and exactly right when a few rows
  dominate. Rows still bank as they land; only the order changes.

**Smallest-first throughout is the safe fallback** — it costs makespan, not correctness.

**Reserve the known giants and pin them to the fastest worker.** The sweep's long-timeout table names
the packages that carry per-package deadline **floors**; those plus any row with an unusually large
suite are the reserved set. ⚠ **The floors are floors, not overrides** — a larger timeout raises them
for a slower box; a smaller one still loses. Under-budgeting exactly those rows is the false red the
table exists to prevent.

⚠ **DERIVE the reserved set at generation time; never carry a copy.** The long-timeout table is
*edited* — floors get raised, packages get added — so a reserved set typed into a plan is stale by
its second week. The recorded instance drifted **twice** in one map's short life (three floor rows
missing, two floors misquoted) before the generator started reading the table itself. And a copied
list fails in the one direction that matters: a missing floor deals exactly the row that needs a
raised budget to the slowest worker, which is the false red the floors exist to prevent. *A hoist
still needs an editor; a derivation needs nobody.* Keep the two halves of the set visibly separate —
the **derived** floor rows, and any row pinned for raw wall time, which is an editorial choice and
should not be mistaken for the derived half.

**The map's construction, stated so two coordinators build the same one:**

```
1.  rows  := the roster's rows at the migration branch tip   (re-read, never carried)
2.  R     := reserved set ∩ rows                             (DERIVED, then pinned to the fastest worker)
3.  P     := rows \ R, sorted ASC by t_r                     → phase recon: deal round-robin across W
4.  B     := rows \ R, sorted DESC by t_r                    → phase bulk: LPT-greedy — assign the
             largest unassigned row to the bin with the smallest (load / s_w)
5.  split any bin whose load/s_w exceeds C into ceil(load / (s_w·C)) sequential shards
6.  emit  the migration's shard-map document — one table, W columns, every row named exactly once,
          with a checksum line: |rows| == |R| + |B|
```

| Symbol | Meaning | Where it comes from |
|:--|:--|:--|
| `W` | worker count | the fleet as engaged |
| `s_w` | worker speed factor, fastest worker = 1.00 | **measured at this migration's recon**, from a same-workload calibration pair, reported with the worker's first shard |
| `t_r` | row `r`'s expected wall time | the preceding consolidation sweep's per-row log, × `k` |
| `k` | the convert-and-build multiplier for a full test action against a build-skipping one | **measured** on the recon phase's first rows; never assumed |
| `R` | the reserved set | derived from the long-timeout table, plus the editorially pinned rows |
| `C` | target shard wall time | one session's worth, with margin |

⚠ **Every factor above is re-measured at the migration, and pre-migration cross-machine ratios are
SUSPECT** — including `t_r`, which is **not portable across operating systems**: the recorded
instance measured one leg at roughly 2.5× the other overall and **three times** on the single row
that bound the makespan. So the leg a row is costed from is not a separate question to rule; the
recon that measures `k` and `s_w` measures the row costs with them, on the leg the shard will
actually run. **A map built at placeholder factors is a projection, not a deal** — say which it is,
and gate dispatch on it: at small `W` the campaign is capacity-bound and every factor error passes
through, while at larger `W` the reserved set binds and placeholders cost projection accuracy rather
than the target.

**The calibration workload is part of the protocol, not an afterthought.** A row whose time is
dominated by fixed convert-and-build overhead cannot discriminate a fast worker from a slow one — a
few-second row measures the overhead, not the throughput. Pick a **mid-weight** row, state the
repetition count and where the reading is recorded, and do it before the map leans on the number.

### 3.3 Preconditions a shard must confirm before its first row

- **The worker's Go toolchain is the target release and its clone is at the migration branch tip.**
  The sweep **throws** when `version.props` disagrees with GOROOT's `VERSION` file — so a worker on
  the old toolchain gets a loud refusal rather than a wrong answer, but it should be caught in the
  shard's acknowledgement rather than at row 1.
- **A two-sided worker preflights BOTH arms, and a bounded search that finds nothing is not an
  answer.** A box that runs rows on two sides (a native side and a WSL/linux side) is two workers
  sharing a name: each has its own PATH, its own GOROOT and its own clone, and a green preflight on
  one says nothing about the other. Measured 2026-09-20 at the 1.24.13 hop: a box whose native arm
  passed first run had **no pinned GOROOT at all on its linux arm** — it carried the previous
  corpus pin and the ambient toolchain — so every linux-only row routed there would have run
  against the wrong toolchain or not at all. Provision side-by-side under the arm's own home rather
  than replacing the ambient `go`, which would make the pin assertion vacuous by removing its
  dissenting control.
- **The whole-solution build has been run once**, so the per-package builds go incremental.
- **The converter binary was rebuilt after the toolchain move** (§1.2) — and after any embedded-asset
  edit.
- **The worker's C-toolchain capability is recorded.** On platforms where cgo availability changes
  which tests the **Go side** runs, a worker with a C toolchain and one without are **not measuring
  the same thing**, and the difference presents as a verdict-count discrepancy attributable to nothing
  in the corpus.
- **The worker's cgo STATE is pinned per package, not just recorded.** The bullet above is about the
  Go side and presents as a count discrepancy; this is the other half and it presents as a **build
  failure with zero verdicts**, which reads like a converter regression and is not one. For a package
  whose **production** file selection is cgo-conditional, the cgo state decides *which `.go` files
  exist* — so a conversion run under `CGO_ENABLED=1` against a corpus emitted at `CGO_ENABLED=0`
  compiles a different source set than the committed tree holds: declarations migrate between files
  while the stale other-selection file remains, and the build dies on the duplicates. Both sides of
  the comparison must share **one** cgo state, and the converted side can only be the selection the
  committed tree holds — i.e. the corpus's emission state, which is `CGO_ENABLED=0`.

  Measured 2026-09-02 on Linux as a one-variable A/B on `os/user`: `CGO_ENABLED=1` failed in 12 s with
  zero verdicts; `CGO_ENABLED=0` validated at 12, all agreeing, a strict superset of the 5 banked
  Windows names (the 7 extra are `lookup_unix_test.go`'s, selected only when cgo is off). The remedy
  is the sweep's `$cgoOffPackages` table beside `$longTimeouts` — **per-package, never session-wide**,
  because rows whose annotations were derived cgo-ON (`debug/buildinfo`, `go/internal/gcimporter`,
  `go/internal/srcimporter`) come back short under a global zero.

  **A hop re-derives every row, so census the class first rather than meeting it one package at a
  time.** The census is a grep of the target release's `//go:build` lines for `cgo`, split by whether
  the conditional files are production or test-only:

  - **production-conditional** — the build-failure class; these need the pin. At Go 1.23.12 the
    roster's members are `net` (16 files), `os/user` (7), `plugin` (2) and `crypto/internal/boring`
    (1, and inert unless the `boringcrypto` tag is on, since its constraint is a negated conjunction
    that is already true without it).
  - **test-only-conditional** — no build failure; the cgo state decides which *tests* run, so it
    decides the **count**, and the annotation is only meaningful beside the state it was taken in. At
    1.23.12: `debug/pe`, `os/exec`, `os/signal`. `debug/pe`'s Linux surplus is exactly this — its
    `linux: 13` against a Windows 10 is three tests in `file_cgo_test.go` (`//go:build cgo`) that
    exist in the run only because cgo is on.

  The count moves in both directions, so neither state is "safer": pinning cgo off fixes `net` and
  `os/user` and would *reduce* `debug/pe`. Pin what the corpus's emission state requires and leave the
  rest alone.

### 3.4 The ledger

Per-row log retention plus an **idempotent resume ledger** is what makes a multi-hour shard survivable
on any machine, and what makes a killed or rebooted worker cost minutes rather than hours.

- **One line per row, append-only**, keyed by package path. Fields: package, shard, worker, start and
  end timestamps, timeout used, verdict, matched count, disclosed count, **log path**, corpus commit,
  converter commit **and the converter binary's modification time** (a commit does not say whether the
  binary was rebuilt after it), and the worker's C-toolchain capability.
- **Idempotent**: a row already carrying a terminal verdict at the current corpus commit is skipped on
  restart; a row at a different commit re-runs.
- **`NOT MEASURED` is a first-class verdict, never a failure.** An unmeasured row must never read as a
  pass, and must never read as a corpus regression either — the repository has already paid for both
  mistakes. A shard reports unmeasured rows **by name** and the coordinator re-dispatches them with a
  raised budget.
- **Per-row logs are retained and their paths recorded**, because the sweep collapses build errors
  into bare failure rows *with zero diagnostics* — the by-hand doctrine is what exposes a compile
  error hiding behind batches of silence. **A ledger row without its log is not evidence**, and across
  many workers nobody reads every log unless the report points at one.
- **The ledger is committed to the shard's branch** as the shard's own artifact, so the coordinator
  merges evidence rather than claims.

**Bank a migration's INPUTS where the migration can find them — in the commit that claims them.**
Per-row retention above is one instance of a rule the repository has paid for repeatedly. A *working
input* — the upstream per-commit file map §4's triage resolves against, a shard map's actual deal, a
census's raw output — that lives only in the session directory which produced it re-derives from zero
when that session ends, and the standing tidiness habit actively deletes it. **The report is not the
artifact.** Whatever a record says is "beside this file" must actually be beside it, in the
repository, in the commit that makes the claim; the alternative is not *"we can re-derive it"* but
*"we will re-derive it under migration-day time pressure"*. A record whose inputs genuinely cannot be
banked says where they are instead of promising a location that is false.

**Five ways a re-derived roster reads green and is not.** The repository's standing false-green
catalogue covers gates; a shard *campaign* is a different surface, and these five have each been met:

1. **The vacuous shard.** Every row PASSes because the worker's clone was never moved to the
   migration's corpus commit — a perfect score, measured `W` ways, on the OLD corpus. The ledger's
   corpus-commit field is the defense **and the merge asserts it**.
2. **The rebased-disclosure launder.** A pin breaks when its test is reworded, and the fast fix —
   editing the signature to match — turns a real, re-derivable divergence into a rubber stamp.
   **Re-sign, never edit.** Where a disclosure class pins its rows *as failing*, laundering is
   forbidden by the class's own text.
3. **The disclosure that closed silently.** On a patch-level migration, closure is the *more* likely
   direction. It is a good outcome and still owes evidence: the arithmetic must move, visibly.
4. **The truncated artifact protected by an up-to-date check.** A transpile that times out can leave
   a `.cs` **zero bytes on disk**, and an empty file is still newer than the converter binary, so the
   next run's freshness check skips it. It has been measured failing loudly; the same mechanism could
   hide a real result. Under many workers at budget pressure transpile timeouts are *more* likely,
   not less — **a shard reporting zero timeouts on a slow box deserves a second look, not a
   congratulation**.
5. **A stale converter reaching one worker and not another.** Its worst form *is* a shard campaign:
   the worker whose binary predates a change re-derives against the old emission and reports green
   while a rebuilt worker reports drift, and the disagreement presents as a **machine** difference —
   the hardest kind to chase across a fleet. The converter-commit field alone is not enough, which is
   why the row above also carries the binary's modification time.

**And the structural one, which is not a false green but reads like one**: the sweep collapses build
errors into bare failure rows *with zero diagnostics*, so a compile error hides behind batches of
silence and nobody reads `W × M` logs. **A shard's report must distinguish "failed with named
verdicts" from "failed with none"** — the second is a build failure wearing a verdict's clothes.

### 3.5 Signals and incremental merging

**Branch shape:** the migration lives on a long-lived version branch; each shard branches from *that*,
never from master.

**The merge hotspot, and how to remove it.** Every shard's natural deliverable includes a roster edit,
and the roster's header arithmetic is a **single line every shard would touch**. The rule:

> **A shard edits only its own rows. The coordinator recomputes the header.** The header is already
> recomputed from its own table, the row grammar has its own format guard, and both the sweep and the
> guard read one shared parser. A shard that touches the header has broken protocol, and the format
> guard is the place to notice.

Row edits themselves conflict rarely — one row per line, alphabetical, with shards scattered across
the alphabet. Proof pages, manifests and committed test sources are per-package files and cannot
conflict at all.

**Merge incrementally, not in one batch**, for three reasons each with a precedent:

1. **A lane's proof binds its own tree, never the merge result.** A flagship row has already banked
   green on a lane tip and been red at master the moment its merge landed, because the guilty change
   merged after the lane forked — each side green alone, the union never swept. **A shard merge
   therefore owes a post-merge filtered re-sweep of a sample of its own rows at the merge result.**
2. **The reflection-consumer canary set is derived at gate time, never carried** — the largest banked
   reflection-consuming rows by verdict count, recomputed from the roster at the moment of the gate.
   The known escape happened *precisely because* a merge canary set predated the newest bank.
3. **Incremental merging bounds the blast radius**: many shards merged at once and one red row is a
   many-way bisect; merged one at a time, the red row names its shard.

**Re-assert the checksum after every merge**: every roster row appears exactly once, the header
recomputes, and the roster format guard is green. **A row that is in the shard map and in no shard's
ledger is the campaign's one unrecoverable failure mode** — make it a gate, not a hope.

### 3.6 What a worker owes, and what it must not do

The fleet's established worker contract, generalized: a worker **runs named instruments at stated
budgets and reports raw output** — exit code, the arithmetic lines verbatim, a bounded log tail, and
sweep dirt classified **only** against the documented classes, with anything else posted raw as
**UNCLASSIFIED** for the coordinator to rule on. A worker **makes no rulings and never commits to
master**. A run that exceeds its stated budget is **killed and reported as a timeout with the log
tail** — a worker does **not** extend a budget on its own.

Two operational rules that are not optional:

- **Long runs are launched detached**, or the session's turn boundary reaps them; and the wait is
  written as a **positive** poll on the log or the process, because the naive inverted form reports
  "exited" instantly while the process still runs.
- **A worker's own outer wrapper must clear the instrument's internal budget.** A wrapper tighter than
  the instrument's own budget is a false-red generator, and at the sweep's long-timeout floors the
  mismatch is easy to make.

**When a worker dies mid-campaign, CLASSIFY before diagnosing.** A truncated log with no diagnostic
has four known causes and only one of them is a defect: a sibling's bare-name process kill (which is
machine-global — worktree isolation does not help, and neither does renaming an apphost); harness
background-task **tree** reaping at a turn boundary (which walks parentage, not names, so the rename
defense does nothing there either); a sibling's machine-global build-server shutdown; and an actual
reboot. **Check uptime first** — one command, and on a box that reboots it is the likeliest answer.
Then:

- **Resume, do not restart.** Rows carrying a terminal verdict at the current corpus commit are
  skipped; in-flight rows re-run. A reserved row re-runs *whole*, which is one more reason the
  reserved set is pinned to the fastest worker.
- **Re-dispatch at a RAISED budget, never the same one.** A floor lands differently on a slower box;
  re-dealing at the original budget re-creates exactly the false red the floors exist to prevent.
- **Do not let the coordinator absorb the reserved set silently.** It is the one machine whose
  stalling stalls everyone — say so on the channel instead.
- **Losing the control worker changes the CLASS of the campaign's findings, not just its speed.**
  Where one machine runs the control legs that make another platform's findings attributable, its
  absence degrades them from *measured* to *inferred*. That is not a scheduling problem to solve
  inside a shard map; it is a fact the coordinator states in the record. The cheap insurance is a
  **named fallback** rather than a standing duplicate: it runs the control leg at its own speed with
  the budget raised, and its results carry its machine name — which is all attribution ever required.
- **The one thing that does not survive a reap is the full-roster parity sweep.** Recovery is
  `roster − logged`, re-run inline, with the verdict arithmetic checked to close.

---

## 4. Golden-drift triage

**The instrument is the upstream history.** The authoritative list of what changed between two Go
releases is that project's own log between the two tags, per package directory. **Every moved golden
and every moved verdict count maps to one of those commits by name, or it is a defect** — in the
converter, in the corpus, or in the measurement. This is the migration's central discipline and it is
cheap: a bounded, enumerable set of upstream changes.

Test each diff against the classes **in this order**:

| Class | Test | Disposition |
|:--|:--|:--|
| **T0 · known non-diff** | the file shows modified with an **empty** numstat | line-ending phantom. **Restore.** ⚠ the empty-numstat rule is **false for verbatim-copied paths** marked as binary-ish in `.gitattributes` — git does not normalize them, so a pure line-ending flip shows a *real* numstat. Test line-ending-stripped equality against `HEAD` directly there |
| **T1 · upstream, attributed** | the file maps to an upstream commit touching its Go source | **Bank**, naming the upstream commit in the classification record |
| **T1b · dependency relocation** | the golden's OWN Go source is unchanged and its emission moved because a DEPENDENCY moved -- the alias/namespace change traces to a package relocation in the upstream diff | **Bank**, naming the relocation. **Test this BEFORE T2, which is a trap here:** T2's shape list names "an import alias" and its disposition is RESTORE, but T2 is about two emissions of the SAME sources differing, while this is one emission mode reading DIFFERENT sources. The hunks are indistinguishable and the dispositions are opposite |
| **T2 · test-closure re-emission** | one of the named shapes an `-stdlib` and a `-tests` emission differ by — an import alias, a namespace root escape, the using-block reorder an alias causes, or the test-init hook a `-tests` run adds as **real lines** an `-stdlib` run omits, or a `GoPositionMap` funcLit/range argument the `-tests` emission adds and `-stdlib` omits (rooted independently by two lanes 2026-08-29/30, evidenced by banked `cookiejar`) | **Restore.** A standing restore, not a cleanup, until the two emissions agree. ⚠ the hook shape survives a numstat filter |
| **T3 · born-stale** | the artifact predates an emission that has since landed | **Levelled in H4a's opening bundle.** Anything still in this class afterward is a defect in the bundle |
| **T4 · hand-own consequence** | H6's differential classified the hunk (a)/(b)/(c) | H6 owns it; H10 must not silently absorb it |
| **T5 · UNATTRIBUTED** | none of the above | **Stop.** The migration's real signal, and the only class that blocks. Root-cause before the branch merges |

**A class that is not a diff at all, and belongs in the same triage: the EXPIRED FIXTURE.** Upstream
test data with a wall-clock lifetime — certificates above all — makes a banked suite start failing on
a date nobody changed anything on. Upstream fixes these by pinning the affected test's clock, so the
failure is **already solved in the release the migration is moving to**: a row reading as a
regression on the outgoing corpus is levelled for free by the migration. Check the fixture's clock
before triaging any row that was green, is now red, and has no code change under it — and where the
upstream survey names such a row in advance, put it in the triage record, because the cheapest
attribution is the one written down before the false red arrives.

**The movement class to expect and welcome**, and its trap: a disclosure pinned by exact failure
signature **breaks when its test is reworded**, and the fast fix — editing the signature to match the
new text — converts a real, re-derivable divergence into a rubber stamp. **Re-derive and re-sign;
never edit.** Every re-signed entry names the upstream commit that moved the test.

**Fragility has TWO axes, and a signature-oriented triage looks at only one of them.** A pin breaks
by its SIGNATURE (the failure text is reworded) or by its NAME (the test or subtest label moves), and
the two are independent:

- **Name-fragile, signature-stable.** Where a manifest pins a host or runtime constant upstream
  cannot touch, the signature is effectively immortal and the whole exposure is in the labels.
  Subtest names *generated from a table upstream is rewriting* are the worst case: a renamed or
  re-cased label breaks the pin while its signature stays perfectly valid.
- **Signature-fragile.** Where a pin quotes an upstream `t.Errorf` format string, any rewording
  invalidates it — and **a short, generic prefix is the dangerous shape**: a handful of characters
  that is not a go2cs message at all, which upstream can reword *and* which can collide with another
  test in the same package emitting the same prefix.
- **A row with NO manifest compares strictly, and that is not a safe state.** Zero disclosed means
  **no absorption path**: any verdict movement is a hard mismatch. So a migration's attention list
  should rank strict-compare rows carrying upstream-changed *production* code ABOVE rows with large
  manifests — the opposite of the intuitive ordering, and the ordering the evidence supports.

**A CRASH is not a divergence, and no disclosure can absorb one.** Disclosures absorb verdict
divergence; a host that dies takes every later verdict with it, and the tail that follows — ordered
by test name, uniformly empty — is the crash's fallout, not a hundred findings. Read a mass-empty
tail as **one** defect at its first empty row. And note the ordering consequence: a disclosure scoped
to the crashing case is *unreachable* until the process survives the test, so the fix is the process
first, the disclosure second, never the other way round.

**Two more dispositions.** A HOST-TIMING reading — a deferred allocation disclosure that fires on one host and not
another — is not a regression, and the page of record stands. An infrastructure-error verdict from a throwing stub
is never disclosed; its cure is a refuse-by-name body whose text is the signature.
*(Edited in place 2026-09-26, §7 lesson 37: both surfaced at the 1.24.13 hop.)*

**And classify closures as carefully as breaks.** A row that *matched* because both runtimes were
wrong the same way can newly diverge, and a **disclosed divergence can silently close**. A closure is
a good outcome and must still be **retired with evidence** — the arithmetic must move, visibly, or
nothing was proven.

---

## 5. Parity gates — the arithmetic that lets master cut over

The version branch may carry a red roster gate for a long time; **that is what the branch is for**.
Master merges only when all five hold, each stated so it can be **checked, not felt**:

| Gate | Arithmetic |
|:--|:--|
| **Compile parity** | errors zero **and** skipped-dependents zero, at **100 %** of the migration's package set, under the default target OS |
| **Roster parity** | every roster row appears in exactly one shard ledger (nothing lost) **and** the absolute row count ≥ prior, with upstream-deleted-package losses as **recorded** exceptions. Both absolute and percentage reported. Every row backed by a regenerated proof page and a re-derived, re-signed manifest |
| **Behavioral parity** | Transpile/Compile/Target zero failures and Output's failing set equal to H0's named base with APPEARED empty *(edited in place 2026-09-26, §7 fix A7)*, **zero** `NOT MEASURED`, every moved golden classified T0–T4 with **zero T5** |
| **Hand-own audit** | §H6's completeness gate (`src/check-h6-completeness.ps1`) passes: every marked path in the **re-measured** census appears exactly once; every (b) carries a written reason; every (c) a work item; **zero** "no `.auto` emitted" rows |
| **Release ritual rehearsed** | tag mint, write-once snapshot, every badge retarget, recomputed re-verification — exercised on the migration's own tree |

**Performance is deliberately NOT a parity gate.** A full AOT pass is hours and must run solo; the
frame schedules it **once per ladder** plus coordinator discretion, not once per migration.

---

## 6. Gate accounting — what a corpus migration owes

| Gate | Owed? |
|:--|:--|
| converter `go test ./...` | **yes**, at H1 and after every converter change. Carries the shared-project registration guard, the metadata-sync guard, the capability-gate guard and the platform hand-own guard |
| `check-no-regression.ps1` | **yes**, at H4 and per converter-touching commit. It re-transpiles **unconditionally** and is the authoritative drift instrument — **never add an up-to-date skip to it** |
| `go2cs-stdlib.slnx`, every buildable target-OS flavor | **yes**, at H7 |
| master folded into the release branch | at H7a and again at the campaign tip (§2 H7a) — each sized, predicted, and scored; H6's retired-hand-own step re-run after H7a as the closing check. *(Edited in place 2026-09-26, §7 fix A6.)* |
| `go2cs.slnx` | **yes** after any golib/runtime API change; it is the only gate compiling the non-generated members. In any non-Debug configuration pin `-p:go2csPath=<repo>/src/`, or every behavioral project resolves the machine deploy root instead of the tree (R-B16, H10's close amendment) |
| full behavioral suite (four phases) | **yes**, at H9 and at the parity gate |
| GolibTests build under `GoTargetOS=linux` | **yes**, every battery *(Edited in place 2026-10-01, §7 lesson 55: this row was added; the project had not built under linux for eight days and nothing noticed.)* |
| seeded full reconvert | **once** per phase — H4a's bundle and H5 — plus H10's close regen whenever a converter fix landed after H5, plus any proving regen the release rules; count each. Never twice into one staging root. *(Edited in place 2026-09-26, §7 fix A13: the 1.24.13 hop ran a third at the close and a fourth before the release.)* |
| multi-target emission + platform census | **yes**, at H8 |
| full validated-roster sweep | **once**, at the parity gate: coordinator-owned, backgrounded, on the fastest available machine, **never parked by a lane** — a lane's process tree is reaped at its turn boundary, and sweeps have been lost to exactly that. Recovery is `roster − logged`, re-run inline, with the verdict arithmetic checked to close *(§7 lesson 37: a survival canary before row 1; `clean-bin -Root` explicit and `src/gen` purged separately; disk preflight at the row boundaries.)* |
| `release-nuget.ps1 -VerifyOnly` | **yes**, at every train landing during the hop *(Edited in place 2026-10-01, §7 lesson 77: this row was added; push-nuget's release pre-flight was red at master from TRAIN G until 2026-09-29.)* |
| release-ritual dry run | **yes**, at H12 |

Budget every one from CLAUDE.md's measured budget table, **from the top of each range**, and
**re-measure and update the table** when a healthy run exceeds a row. A stale baseline is what makes a
healthy run look hung — and, in the other direction, what lets a hung one look healthy.

---

## 7. Lessons from the 1.24.13 hop — dated 2026-09-26

> **What this is.** This section records the lessons of the first official corpus hop, go1.23.12 → go1.24.13
> (2026-09-07 to the 1.24.13.1 publish on 2026-09-24, and 1.24.13.2 the same day). The owner ordered it on
> 2026-09-23. It is written for the NEXT hop and keeps only what makes that hop quicker or smoother. Where a lesson
> changes a step, the step itself was edited IN PLACE on the same date (**Step text:** names where), and each such
> edit carries a one-line dated note. The rest of this section is the record. Every lesson cites its source in the
> HTML comment beside it.
>
> One-off code findings stay on the BOARD. The general instrument discipline this hop taught (false greens, census
> controls, shell traps) goes to the `gate-forensics` and `measurement-discipline` skills, in a separate seat. The
> H10 close amendment's lessons 1-7 stand where they are and are not restated here.
>
> **Fixes A1-A13.** Thirteen places where the step text still said what a later dated block had corrected (the
> fourth ordering, H7a's "one merge", H9's "all four phases green", H10's row-count equality and its "226", among
> them) were corrected in place. Each carries a one-line dated note naming its fix, so the old text stays readable.
>
> **Addendum 2026-10-01 (§7.7).** Lessons 40 onward cover 2026-09-26 to the 225/225 close (TRAIN K, 75648a022b) and
> sharpen §7 items whose step text the next hop still needs. Their step edits carry "(Edited in place 2026-10-01,
> §7 lesson N: …)".
<!-- Owner order: ledger 2026-09-23 14:52 · OWNER ORDER · c9e15c73a0 (seed list claude/coord-handover
     docs/phase4/briefs/lessons-learned-seed.md). The owner excluded the i9's hardware issues as temporal. Derived by
     C1 on 2026-09-26: seven read-only agents over the ledger (09-20..09-25), the mailbox archives (09-07..09-20), the
     BOARD at db1bd885a2, this runbook's 31 dated blocks, the RESUME-SESSIONS STATE DELTAs and the 41 briefs and
     reviews produced 449 cited candidates. A script found every quote in its source. The outline
     (claude/c1-hop-lessons-outline ddb47b72b7) was RULED by COORD on 2026-09-26. Three adversarial checkers then
     re-read every cited line: 52 items, 11 KEEP, 41 KEEP-CORRECTED, 0 DROP. Mailbox line numbers refer to
     claude/mailbox docs/phase4/MAILBOX-archive-2026-09-13.md (from line 121655 = 2026-09-07) and
     docs/phase4/archive/MAILBOX-through-2026-09-20.md. -->

### 7.1 Before H0: readiness

1. **Qualify every host before H0**: DNS by Go's own `net` suite, WSL `::1`, symlinks and long paths, pwsh 7 and the
    pinned .NET, a many-core host for crypto/tls's BoGo wall, cloud quota early. Each gap stalled a lane or forced a
    re-route mid-campaign. **Step text:** Readiness.
    <!-- ledger 2026-09-22 19:34 · FINDING · 43d149b87d (the router relay turns NXDOMAIN into SERVFAIL; no box passed
    Go's net suite); 2026-09-23 10:10 · OWNER RULING · fc6269b0bf (a qualified host fails only TestLookupCNAME);
    2026-09-24 00:33 · FINDING · 836004dd20 (public IPv4 resolvers per adapter, IPv6 unbound; Developer Mode off,
    LongPathsEnabled=0); 2026-09-23 22:36 · FINDING · 020365abcd (WSL hosts file maps localhost to 127.0.0.1 only;
    BoGo listens on [::1]); 2026-09-22 00:05 · RULING · 0adf2e431 (pwsh 7 absent on one worker; DOTNET_ROOT not
    exported); 2026-09-22 21:55 · FINDING · 7462befde0 and 2026-09-23 11:59 · OWNER RULING · fad839a224 (the BoGo
    wall; a 48-vCPU host met 600 s at 213 s); briefs/az1-tls-brief.md:44-45 (only GOFLAGS reaches BoGo's nested go
    test); seed items 1 (lessons-learned-seed.md:26-34). -->
2. **Land and red-prove each instrument BEFORE the stage that needs it** — the recon wrapper as one blob, `-Hop`
    mode, the H6 gate, the H11 origin-tag check. All four were built or fixed on the critical path. **Step text:**
    Readiness.
    <!-- ledger 2026-09-22 14:52 · RULING · 158ce37f6c and 2026-09-22 18:02 · SEAT · d095fe8108 (the wrapper's floor
    replaced an asked timeout and ignored the roster execution pin, found at pass 3 and fixed red-first);
    mailbox-archive.md:113793 (fifteen wrapper commits, mostly during launch; the next hop starts from the landed
    blob); mailbox-0907-0913.md:41281 and mailbox-archive.md:5645-5651 (-Hop was open instrument debt, and its first
    cut broke the ordinary gate under PS 5.1); 2026-09-24 02:10 · STAMP · bccf8d977b (RN-15 the H6 gate had no
    instrument; H11 finding (2) the next-release check read local tags only); 2026-09-23 23:41 · RULING · fa18863b94
    (RN-9). -->
3. **Addressed comms from day one** (ledger, per-lane inboxes, one message per event, one OWNER-HAND line); the
    broadcast mailbox measured 59% protocol work. A cloud lane's state is only what is at origin. **Step text:**
    Readiness.
    <!-- mailbox-archive.md:115567-115579 (the owner steer; PROTOCOL v4 rules 2-6); ledger 2026-09-20 13:15 · RULING ·
    77a03bd82; 2026-09-22 00:09 · RULING · 93c9f43a6 (ONE OWNER-HAND line); mailbox-archive.md:41717 (nothing local
    survives a cloud-container restart); mailbox-archive.md:38414 (the weekly-limit shutdown during H5). -->
4. **Before the pin moves**, commit every unbanked reading (a hop destroys scratch), freeze EVERY current proof page
    in the outgoing snapshot, and derive the "must land first" list from master — two of three quoted blockers were
    stale.
    <!-- mailbox-0907-0913.md:8224-8226; ledger 2026-09-23 23:41 · RULING · fa18863b94 (RN-6: 21 links dangled under
    the 1.23.12.3 snapshot, so 1.24.13.1 froze all 232 current pages). -->
5. **Keep a next-hop notes record**: a seat whose census reaches the next release's tree records the spread there
    (this hop, one construct goes from 2 sites to 6 at 1.25.1).
    <!-- mailbox-archive.md:89265, :89380-89386 (C1's read of R's seat ce8d0bd654; "worth a line in the hop-B notes"). -->

### 7.2 H0 – H3

6. **H0 captures the next hop's comparands**: the outgoing `-platform-census` manifest, the behavioral failing set by
    name, each box's bare `go`. H8 and H9 reconstructed the first two mid-hop. **Step text:** H0.
    <!-- mailbox-archive.md:76101-76106 (the H0 baseline "contains no platform manifest"; the comparand was produced
    in about 1,116 s); mailbox-archive.md:79905-79907 (the base two fail identically at pre-hop master; H9 restated BY
    NAME); mailbox-archive.md:54051 (bare go read 1.24.7 and 1.23.1 on different boxes). The committed corpus's drift
    from the pre-hop converter is H4a's `d-level` comparand (mailbox-archive.md:42579), not a separate capture. -->
7. **Assert the toolchain three ways, from a no-module directory, and print them**; `-goroot` does not steer the
    loader and `GO111MODULE=off` cancels a redirect. **Step text:** H1.
    <!-- mailbox-archive.md:8624-8633 (the loader follows the environment; a wrong-release run exited 0);
    mailbox-archive.md:88151 (the three-way fleet preflight); mailbox-archive.md:76147; §2 H2 ruling 2026-09-08 (the
    pairing); the converter refusal 7c1d8832f. -->
8. **`go vet` under the new directive is H1's own bill**: the directive alone failed the converter suite at 10 sites.
    **Step text:** H1.
    <!-- mailbox-0907-0913.md:10977-11008 (G; COORD's ruling at :11101). -->
9. **Read the release's GODEBUG default changes against the test host**: 1.24's `winsymlink` refused junction-staged
    hosts. **Step text:** H1.
    <!-- mailbox-archive.md:104090 (the host seat applies GODEBUG=winsymlink=0 on the junction fallback). -->
10. **Run `migrate-gorelease`'s bare census whenever docs restructure**, not first at H2; a registry re-key lands
    with the stage whose witness it reads (GOROOT → H2, corpus → H5).
    <!-- mailbox-archive.md:1280-1292 (45b58dc86 at ddd509c1e); mailbox-0907-0913.md:12875-12877 (re-key, never
    retire) and :15262 (db071b422 the H2 seat, e7e976f9d the H5 seat, "decided by the guard's WITNESS"). The
    build-number reset missed at 0f97dcc8db reached 335 README badges (mailbox-archive.md:79202-79230) and is held by
    TestPublishedCounterMatchesTheRecordedReleases (H2's AMENDED block). -->
11. **Every count states its axes** — population, tags, `CGO_ENABLED=0`, ReleaseTags per side, toolchain,
    GOEXPERIMENT baseline; two rulings were withdrawn over unstated axes. **Step text:** H3.
    <!-- mailbox-0907-0913.md:7935-7965 (346 raw vs 342 convertible; runtime/cgo under CGO_ENABLED=1);
    mailbox-archive.md:4312-4337 (experimentBaseline: SwissMap, SpinbitMutex, SyncHashTrieMap on at 1.24.13);
    :76147-76149; :78200; :78653 (a disposition withdrawn within the hour); H8 amendment (d). -->
12. **File the NuGet ID-prefix reservation when H3 names new IDs**; it was still an external wait after the release.
    **Step text:** H3.
    <!-- resume.md:1078 and :259. -->

### 7.3 H4 – H5c

13. **The `testing` host's bill for new TB/B/F members goes first**, sized by receiver-typed call sites of the NEW
    members (a name census over-matched 2.6×); an H4 item lands early only if gated green at the OUTGOING corpus.
    <!-- mailbox-0907-0913.md:8210-8218 (COORD owner ruling 2026-09-07); :8612-8648 (8 rows / 2,425 verdicts
    blocked); :29089-29124 and :29239 (a golib member compiled at both releases and was refused by a guard). -->
14. **Re-key registry entries by SIGNATURE, not a similar name** (`getgcmask` became `pointerMask`); a target-only
    entry lands on the version branch with the H5 set.
    <!-- mailbox-archive.md:30251-30290; :23918-23945 (unlock2Wake; TestManualConversionRegistrationsDisplaceSomething
    red at master); c8d50e014f. -->
15. **Seat rules**: re-baseline moved goldens in the same commit; reach censuses cover `src/tests/Behavioral`; read a
    prediction's probe unfiltered; rule union classes and one resolver per shared file before the battery; build
    SDK-less C# on a scratch merge first.
    <!-- mailbox-archive.md:77791-77830 (32 emission-touching commits, 4 banked a golden); ledger 2026-09-22 15:09 ·
    FINDING · f9f3c1039 (CNR red on RangeOverIntegerTypes, missed by a std-only census); mailbox-archive.md:50525;
    2026-09-22 00:05 · RULING · 1271662ec (batch 3 stopped on a projitems adjacent insert); 2026-09-20 15:35 · RULING
    · 6d814e2d3 (one resolver for convCallExpr.go); mailbox-archive.md:53580-53600; 2026-09-22 06:13 · RULING ·
    34d8a5be0 (batch 4 did not stamp on three reds in uncompiled C#). -->
16. **Rehearse H5 at the union tree with written predictions**, reporting projects built (predicted 300-325, built
    70); CHECKPOINT commits; the BUILD seed and the attribution overlay. **Step text:** H5.
    <!-- mailbox-0907-0913.md:12341-12424 (R's rehearsal 917f8bfac); mailbox-archive.md:20883-20918 (i9's union rung;
    62 false errors without src/gen); :21015 (0687402db); :34106-34121 (7ae5355bb CHECKPOINT 2; the slnx in the same
    commit); :33664-33740; :74972-74985 (src/core/VERSION still read go1.23.12 at the 1.24.13 tip). -->
17. **H5c's selection is the converter's** — no second tag resolution, no mtime or seeded-root presence, a hop-stale
    census, zero counts printed, orphans relocated with their namespace. **Step text:** H5.
    <!-- mailbox-archive.md:34580-34597 (bb3a1a747: five live files marked deletable by a second tag resolution);
    :19784-19838 and :20288-20300 (8f2eafdc8, then 79b555fcc withdrew presence-in-a-seeded-root); :52550 (q87, the
    hop-stale census; crypto/ecdh/package_init.cs); :33716, :33840, :34280-34290 (the slnx post-condition passed 0 == 0
    over an empty population); :32383 (8f800233b). -->
18. **Measure each converter batch's corpus footprint AT the batch**: the close inherited 154 converter-touching
    commits unmeasured, and the release packs `src/core` as committed.
    <!-- ledger 2026-09-22 08:18 · STAMP · ae2f25198; briefs/h10-close-obligations.md:49, :56 (54dec61728..d1a0d314d5);
    briefs/packaging-train-seats-2-3-review-2026-09-24.md:195. -->
19. **This hop's declared-set guards (q82, RED 8 (d), q84) fail at the next hop by design**, naming the new members;
    read those reds as H6 input.
    <!-- mailbox-archive.md:48061, :48211, :50085 (q82, d87d2f94a4); :49382-49390 (RED 8 (d)); :50859-50862 (q84);
    ledger 2026-09-22 08:18 · STAMP · ae2f25198 (three corpus guards moved mid-hop). -->

### 7.4 H6 – H9

20. **Run `src/check-h6-completeness.ps1` at H6 and before the release, and row each hop-time hand-own as it lands**;
    21 had no row at the pre-release gate. **Step text:** H6.
    <!-- ledger 2026-09-24 02:10 · STAMP · bccf8d977b (RN-15); 2026-09-24 02:24 · RULING · 4fa5eff2df (the script;
    23 violations); 2026-09-24 02:50 · STAMP · 33b6623eab (rows 146-166; PASS 166/166 on the i7). -->
21. **H6 has four more arms** — stranded bodyless declarations (the q82 census), hand-owned hosts quoting Go text,
    observers re-derived at the pin, retired hand-owns' classes re-censused. **Step text:** H6.
    <!-- mailbox-archive.md:48183-48210 (time.runtimeNow, RED 7); :41916-41935 (the testing host's 1.23.12
    Setenv/Parallel texts); board.md:25361 (39ddead63e); mailbox-archive.md:64588-64600, :65210-65225 (merge-base
    271300cea0). -->
22. **A relocated hand-own whose principal changed is re-derived before the H5 gate reads green**; read the gate by
    project, since each fix unmasks the next package. **Step text:** H6.
    <!-- mailbox-archive.md:38185-38234 (i9's H5 gate reading on f0f8826894); :38912 (COORD: H5 green needs row 20
    re-derived; this reversed :33511's order); 5a03aac159. -->
23. **The H6 pair**: one `-trimpath -buildvcs=false` binary, bare `-stdlib`, the old `version.props` for half B, an
    `.auto`-empty stage, `git archive` seeds, CR-stripped hashes.
    <!-- mailbox-archive.md:34951-34960 (42bac7cf7 / 1bec764b4); :35190-35215; :35589 (G's advice, "for anyone
    repeating this hop"); :39542-39548 (f6c60275e); :41108-41120 (6c820a2115). -->
24. **The first post-H5 build is the H7 baseline**: list each unmasked package's referencing projects, size a red by
    whole-corpus census (5 errors were 29 sites), cure it as ONE seat, and run q82 before claiming parity.
    <!-- mailbox-archive.md:42997-43036; :46095 (RED 4); :46521; board.md:25160-25174. -->
25. **Plan two master folds** — H7a and the campaign tip after H10's tooling lands on master; re-mint conflicted
    emitted files and gate the fold before the push. **Step text:** H7a, §6.
    <!-- mailbox-archive.md:70405 (H7a fc275f1ac3); ledger 2026-09-21 23:52 · RULING · 0adf2e431 (the campaign tip,
    c6fdbe73c3); mailbox-archive.md:70207-70235 (a pre-hop mgc.cs emission reinstated by side-taking, CS1615 ×4). -->
26. **H8**: `classify` takes the `manifest` builder's input, class counts are per file, byte-identity views run on
    linux/WSL, platform-exclusive packages come from their native emission, and every L3 csproj is diffed after the
    regen.
    <!-- mailbox-archive.md:78210-78240 (bdc016826a); :79653; :79605; ledger 2026-09-24 01:46 · STAMP · 61724860b4
    (RN-5); 2026-09-23 18:39 · RULING · 1a328f3ee8 (R-B15). -->
27. **H9's gate is Output's failing set equal to H0's named base, APPEARED empty**, with the prediction re-derived at
    the tip on the banking platform. **Step text:** H9, §5.
    <!-- mailbox-archive.md:79899-79915 (972012f070; FuncLiteralCallerNames and GoroutineWaitState fail at pre-hop
    master too); :77420-77500 (06b1636cae); :78839-78850; §2 H9 Closure 2026-09-20; resume.md:360. -->

### 7.5 H10 – §6

28. **H10 order**: successor census and conversion-only pre-stage, then the recon leg in a throwaway detached
    worktree, then seat, plan and driver; seat-before-recon made the generator refuse. **Step text:** H10.
    <!-- mailbox-archive.md:78194, :78562, :85580 (aa66874ff7), :87189, :88148, :87818; ledger 2026-09-23 12:56 ·
    RULING · b57703b1cd; 2026-09-22 16:17 · RULING · 3469154a95; 2026-09-22 18:02 · SEAT · d095fe8108. -->
29. **Launch preconditions**: Go overrides per worker, .NET pins where the pwsh flavour needs them, a `DRIVER_EXIT=`
    marker, the sibling wait excluding the harness's own hosts. **Step text:** H10.
    <!-- ledger 2026-09-20 15:32 · RULING · 0adf2e431; 2026-09-22 00:05 · RULING · 0adf2e431; 2026-09-22 03:33 ·
    STAMP · c6fdbe73c; 2026-09-21 23:52 · RULING · 0adf2e431 (a 40-minute false-sibling wait); 2026-09-22 03:45 ·
    RULING · c6fdbe73c; mailbox-archive.md:85605. -->
30. **Take a linux READING alongside the recon**; late, it found a linux-only partial on 25 of 30 FAIL rows and a
    GolibTests linux build broken for eight days. **Step text:** H10.
    <!-- ledger 2026-09-23 06:18 · RULING · 5da426433b; 2026-09-22 03:45 · RULING · c6fdbe73c; 2026-09-23 10:13 ·
    ACCEPT · ce065f8aa9; 2026-09-23 08:51 · ACCEPT · 971d919113 (b293973e9f); seed item 2. -->
31. **Allocation labels**: a reading run (committed TSV), then the relabel, then rulings, then page banking; the host
    records only the first nonzero `AllocsPerRun` unit.
    <!-- ledger 2026-09-22 16:39 · RULING · c8e6ca9034; 2026-09-23 02:17 · FINDING · ac9f8251ee; 2026-09-23 07:12 ·
    STAMP · 74bae27672. -->
32. **Re-derive the roster header from the table at every leg** (crypto/cipher went from 13 verdicts to 27,272), and
    put hand-owned READMEs on the bank checklist.
    <!-- ledger 2026-09-22 04:14 · RULING · 854b94107; 2026-09-23 04:03 · STAMP · 1719e3b87f (testing's badge stayed
    37/52 after a 53+15 bank); 2026-09-22 15:42 · CORRECTION · 3469154a95; 2026-09-22 14:45 · RULING · d5414aa151. -->
33. **Every leg brief runs the FULL converter suite; push measurement commits before a page cites them; record a
    non-default environment (GOFLAGS) on the page and in an execution pin.**
    <!-- ledger 2026-09-22 08:18 · STAMP · ae2f25198; briefs/h10-close-obligations.md:93-94 (a page cited converter
    4c5b0a3b7, not an object on origin). -->
34. **Size a repair before ruling a hold**: "about two days" was 4 working days to 3 weeks, and a golib-wide repair
    lands after the hop with the row demoted by name.
    <!-- ledger 2026-09-22 17:18 · OWNER RULING, 17:27 · FINDING and 17:33 · OWNER RULING, all · 43d149b87d. -->
35. **Derive the close brief from this runbook with adversarial checkers, and audit the close through three lenses**;
    the audit found four real misses.
    <!-- ledger 2026-09-22 16:31 · FINDING · 3469154a95 (31 claimed, 6 verified, 4 real); 2026-09-23 13:36 · RULING ·
    3ec2c9ff39 (36 brief defects repaired). -->
36. **Keep the recon leg's per-row TSVs as the next shard basis**: dispatch cost is fixed setup, uncorrelated with
    verdict count (r = 0.08).
    <!-- mailbox-archive.md:13074; :106663 (47c60b1d3, "the next hop's leg is planned from them"). -->
37. **§4 gains HOST-TIMING and throwing-stub dispositions; §6's sweep takes a survival canary, an explicit `clean-bin
    -Root`, a separate `src/gen` purge and row-boundary disk checks.** **Step text:** §4, §6.
    <!-- resume.md:182-183 and ledger 2026-09-24 04:58 · STAMP · 5098289482 (context TestAllocs 57|1 vs 58|0); resume.md:876,
    2026-09-22 05:00 · RULING · 4aa8a36fb, 05:10 · RULING · f401c4ea0; briefs/full-roster-sweep-brief.md:107, :118,
    :209; 2026-09-24 04:25 and 04:37 · FINDING · 61724860b4; 2026-09-22 12:24 · RULING · 71d03e812. -->

### 7.6 H11, H12 and release day

38. **H11 follows H10 by construction; removed-ID deprecations come after the publish; docs get a scan AND a read;
    the tag mint is rehearsed in an isolated clone.** **Step text:** H11, H12.
    <!-- mailbox-archive.md:84061 (a9749f5e3); ledger 2026-09-22 19:06 · FINDING · 43d149b87d; resume.md:1179-1181;
    2026-09-24 02:10 · STAMP · bccf8d977b; 2026-09-24 00:25 · OWNER ORDER · fa18863b94;
    briefs/docs-go-version-migration-plan.md:680; 2026-09-24 04:01 · STAMP · 9c9b1a81af;
    briefs/postclose-h11-h12-brief.md:440 (RN-7); briefs/postclose-h11-h12-repair-log.md:44; briefs/h10-close-obligations.md:109. -->
39. **Release day is procedure**: master first, the owner's console publish with no GPG in Phase 2, a signer resume
    that never re-runs release-nuget, the walkthrough on both OSes at the shipping tree. **Step text:** H12, "Release
    day".
    <!-- ledger 2026-09-24 00:04 · OWNER RULING · fa18863b94; 00:45 · ACCEPT · 0b6bf15ab0; 07:24 · ANNOUNCE ·
    b6746ab185; 05:48 · RULING · 6ab65f3409; 04:59 · STAMP · ffa5c1015a; 15:57 · STAMP · e06494c6f8; 11:27 · STAMP ·
    53f52233fc; 14:10 · ANNOUNCE · 4c53b02a0a; 16:17 · STAMP · 9a5f63041b; 06:44 · FINDING · 38529c765a (CS0426
    'Rlimit' on linux; seats 501b7e4c27, 43c8d5aac7, 26042b3a92); seed items 13, 14, 16, 17. -->

### 7.7 Addendum — 2026-10-01: the close seat to 225/225

<!-- Derived read-only on 2026-10-01 at master 172d437e66 and owner-approved on 2026-10-02. Applied on 2026-10-02 on
     TRAIN M's union e2008427b1, where every cited path, line, flag and name was re-read. The text is the approved
     draft's except where that re-read found the tree different. Each such change is recorded in the comment beside
     its lesson, or, for step text, here:
     - lessons 40, 41, 43, 61 and 68: a flag, the ledger's line form, a pathspec, a command's mandatory parameters and
       the pinned-row list; and the line cites in lessons 61 and 72;
     - the Readiness Host bullet (TestBogoSuite twice under -count=1); H3's deliverable (genpopulation's -goroot);
       H5c's class table (the UNRESOLVED row) and its step-5 note; the H5 overlay amendment's step 2 (the payloads
       are not in the find); H10 step 1 (the population file's name); H10 step 5 (a pointer to lesson 61); close
       step 11 (the fixture guard landed with TRAIN L).
     Placement adaptations that change no meaning are not listed: the first use of "NOTES" spelled out, the handover
     branch named in full, and list and table formatting.
     Lesson 65 is held for a COORD ruling and lesson 86 for the skills seat. Both numbers stay reserved. -->

#### Before H0

40. **Qualify crypto/tls's BoGo host by measurement at Readiness, not at its row.** Use a many-core Windows host, local or
    cloud. Go's own `go test -count=1 -run '^TestBogoSuite$' crypto/tls` is green on it twice, and then one converted
    `-tests` reading of `TestBogoSuite` finishes inside the standard 600 s wall. Any cloud quota is filed as an OWNER-HAND
    the day Readiness opens. The raised-wall bank (`GOFLAGS=-timeout=40m` on both sides, in the row prose; the sweep's
    environment execution pin is an owed instrument seat) is the owner-ruled fallback, not the plan. Why: the host, a
    resize and Go's own loopback flake held H10's close for about 15 h. **Step text:** Readiness.
    <!-- ledger 2026-09-22 21:55 · FINDING · 7462befde0 (C# TestBogoSuite 1,788.4 s against Go's 30.4 s on R-LAPTOP,
    16 logical processors: no standard-wall run there); 2026-09-23 00:20 · RULING · c0f11384e4 (a temporary host, set up
    at 8 vCPU and resized to 48 before the row); 10:10 · OWNER RULING · fc6269b0bf; 11:08 · FINDING · 6cd0bfb554 (a
    48-vCPU host met the 600 s wall in 213.1 s; nine Go=fail / C#=pass leaves came from Go's own shim); 11:59 · OWNER
    RULING · fad839a224 and 12:36 · 3ec2c9ff39 (the raised-wall bank; only GOFLAGS reaches BoGo's nested go test).
    Extends §7 lesson 1. Draft lessons 41 and 43 merged.
    Corrected at the apply, 2026-10-02. The draft's command had no -count=1: in package-list mode a second passing
    run replays the cache (go1.24.13 src/crypto/tls/bogo_shim_test.go:411-415 keeps the test cacheable), and "twice"
    exists because the temporary host's Go oracle flaked differently on each run (11:08 6cd0bfb554). The draft also
    read "in the row prose and an execution pin": the roster has no such pin ($RosterExecutionValues in
    src/_roster.ps1 is release-tc0 and release-tiered), and 11:59 fad839a224 rules a standing environment execution
    pin for the sweep a post-hop instrument seat. -->
41. **Census the accepted-but-unlanded seats before H0, before each train draft and before the close STAMP.** Enumerate
    the ledger's `· ACCEPT ·` and `· SEAT ·` lines since the previous anchor, and its `COORD:` lines that say ACCEPTED or
    seated (the ledger's form since 2026-09-30). After a fetch, run
    `git merge-base --is-ancestor <sha> origin/master` for each one. Give every non-ancestor a disposition by name: land,
    re-cut or drop. Why: TRAIN 49 never landed, and its items were missing until they were re-cut.
    **Step text:** Readiness; H10 close step 11.
    <!-- ledger 2026-09-26 10:07 · FINDING · 3ffd1d8a8d (TRAIN 49 never landed: its converter guard, hand-own census fix,
    sweep -Hop mode, H5c slnx-orphan fix and runbook shard amendment, the last three marked "before the next hop", were
    missing until re-cut); 2026-09-27 15:18 e9c57f833e; 2026-09-29 12:51 · SEAT · b46af2049e (the -Hop landing seat,
    re-cut).
    Corrected at the apply, 2026-10-02. The draft named the kind-field form only. The ledger stopped writing a kind
    field on 2026-09-30: that day and 2026-10-01 carry no ACCEPT or SEAT kind line, and their acceptances (TRAIN K
    onward) read "YYYY-MM-DD HH:MM COORD: ... ACCEPTED ...". The kind-field pattern alone reads every one of them as
    absent, which is a clean census over stranded seats. -->
42. **On the box that will read net, re-qualify DNS twice immediately before net's H10 row. Before unbinding IPv6, list
    what answers only over IPv6.** Why: a targeted pass no longer held two hours later, once IPv6 DNS went back to automatic.
    **Step text:** Readiness.
    <!-- A router's DNS relay turns NXDOMAIN into SERVFAIL whatever its upstream, and nslookup asks only the first server.
    A targeted pass at 00:35 no longer held at 02:17, once IPv6 DNS went to automatic and the router's link-local resolver
    came back into the list. Unbinding IPv6 also disconnected the box's mapped drives. (ledger 2026-09-22 22:55
    8b03ac8132; 2026-09-23 00:35 96a8b8d17c; 02:17 ac9f8251ee; 10:10 and 10:24 fc6269b0bf; 2026-09-24 00:33 836004dd20,
    the per-box wording: public IPv4 resolvers per adapter, IPv6 unbound.) Extends §7 lesson 1. -->
43. **Every instrument derives its per-release input from `src/version.props` (or GOROOT's `VERSION`) and refuses by
    name when that release's file is missing. Red-prove each with `version.props` bumped in a scratch worktree.** At
    Readiness, run
    `git grep -n -E '<outgoing release spellings>' -- 'src/*.ps1' 'src/*.sh' 'docs/phase4/**/*.py' '.claude/coord-scripts'`;
    for a hop out of 1.24.13 the pattern is `'go1\.24\.13|go124'`. Every hit is either a derivation or a named record.
    Why: hard-coded instruments silently read the old release. **Step text:** Readiness.
    <!-- ledger 2026-09-29 14:01 aa4ec663c5 and 16:11 422af93f53 (the roster guard and push-nuget's preflight were found
    hard-coded and fixed). This hop's open instances: NOTES-next-hop.md §2 A7-A10 (src/run-h10-recon.ps1:841-843,
    src/check-h6-completeness.ps1:76, docs/phase4/hopA-inputs/shardmap.py:94) and §5 Q14 (one naming convention).
    Widened at the apply, 2026-10-02. The draft's pathspec was 'src/*.ps1' 'docs/phase4/**/*.py', which leaves out the
    shell instruments and the committed fleet templates. They hard-code the release too (the linux driver template
    refuses by name on any other): .claude/coord-scripts/templates/linux-legs.sh, templates/linux-brief.md and
    src/apply-h5-c1-1-rederives.sh carry 13 of the 36 hits at e2008427b1. -->
44. **Build any temporary host from a kit committed on coord-handover:** a setup script, a brief with the bank gate and
    an evidence-only fallback branch, and a resume prompt. Name the host by its fleet nickname at creation. Keep its own
    identifiers only in the census's local never-push token file. Re-run the kit's validation (identity, push auth, token
    file, census plants) after every resize, and delete the host after its row. Why: the kit's derivation caught 28
    defects, and the VM's computer name tripped the census. **Step text:** Readiness.
    <!-- ledger 2026-09-23 00:20 c0f11384e4 (the kit: setup script, brief, resume prompt; 28 defects repaired); 10:45
    fc6269b0bf (the whole validation re-run after the resize); 11:08 6cd0bfb554. -->
45. **Each sweep box's host record names the filesystem that holds the repository, TEMP and GOROOT. GOROOT stays on
    NTFS. A box whose worktree is on ReFS does not read os or testing; those rows go to an all-NTFS box.** Why: on ReFS,
    os diverges on the converted side (`TestFileReadDir/.`, Go=pass / C#=fail), so does testing (`TestChdir/relative`,
    Go=skip / C#=pass), and a ReFS GOROOT deadlocked runtime's `TestTracebackSystem/panic` child. **Step text:** Readiness.
    <!-- ledger 2026-09-28 10:10 ec23ad9f6b (the cross-volume root cause: ReFS lacks object IDs); 2026-09-30 04:20 (the
    one-filesystem A/B: both divergences retire with GOROOT on the worktree's volume); 04:55 (the ReFS-GOROOT adoption
    reversed: trains keep GOROOT on NTFS; with it there runtime matches its pin);
    coord-handover .claude/coord-scripts/trainK/tK-seats-draft.txt:50 (os and testing moved to an NTFS box for TRAIN K). -->
46. **Linux lanes run Microsoft's .NET build, and the host record carries its libcoreclr hash.** Why: under OSR, a
    distro-packaged .NET 10 faulted with 0x80131506, or exited 0 with truncated output. **Step text:** Readiness.
    <!-- ledger 2026-09-29 07:49 7bf4ac26cd; 2026-09-30 04:38; 08:34. -->
47. **Toolchain-adjacent pins land and regenerate before the outgoing corpus's final release, never inside the hop.**
    These pins are the .NET SDK `global.json`, the emitted `LangVersion` and C# keyword escapes. This is the default order
    until COORD rules NOTES-next-hop.md §5 Q13. Why: inside a hop, H5 would measure about 1,364 moved csproj instead of
    inheriting them. **Step text:** H1.
    <!-- 53eef3cda8; d87e4f883c; TRAIN K 4016a2269c; NOTES-next-hop.md E1, E2. Q13 is a COORD proposal, unruled at
    172d437e66 (NOTES-next-hop.md:520); TRAIN K seated S1 and S2 this way. -->
48. **At H0, before H2's pin bump, commit each UNBANKED row's reading as matched / disclosed / undisclosed.** The
    denominator is their sum. Name each undisclosed test and its disposition, and mark CONTESTED any disclosure disputed
    at that reading. Why: reflect circulated as 326 of 385 against a record reading 388, and unique's "4 of 19" was
    19/1/0 of 20. **Step text:** H0.
    <!-- BOARD:24286-24327. -->
49. **Before H0, plain `go test ./... -count=1` in `src/go2cs` is green at master, TestContextBudget included.** Why:
    before 56ff452a50 (2026-09-12, mid-hop), the instructions alone kept every sub-agent from spawning in the repo.
    **Step text:** Readiness.
    <!-- 56ff452a50; 3076e2074b; 48eaa32d98. -->

#### H3 – H9

50. **At H4, and again at H6, census every linkname body that throws `NotImplementedException`, per non-Windows GOOS.
    Answer each one with Go's own unsupported result.** Why: `internal/syscall/unix.vgetrandom` killed 25 of the linux
    leg's 30 FAIL rows through crypto/rand, and the cure was Go's `(-1, false)`. **Step text:** H4.
    <!-- ledger 2026-09-23 06:18 5da426433b; 08:51 971d919113 (seat bdec3ae4d8); NOTES-next-hop.md §3.9 ("read the
    non-Windows legs early"). Extends §7 lesson 30. -->
51. **Run close lessons 2-6 (H10's close amendment) at H5's overlay and after every seeded regen, not first at the close.
    Run close lesson 1 (a costed recon-basis row) at every demotion.** Why: the close met all six only by derivation, and
    H5 meets them first. **Step text:** H5 overlay amendment, steps 2-3.
    <!-- Close lessons 1-6, in H10's close amendment (172d437e66:docs/GoCorpusMigration.md:3187-3248):
    (1) a costed recon-basis row for a demoted row;
    (2) production //go:embed payloads copied with -text pins;
    (3) emitter-moved flat files git rm'd;
    (4) the staging seeder, which carried only docs/validation/current, stripped both badges from 337 READMEs (R-B13,
    781c1c3c31);
    (5) repoguard's csproj-keyed payload admit (R-B14, de436207c9, merged as f0ada4471e);
    (6) the merge renderer dropped log/syslog's post-block group (R-B15, ae813db069).
    Ledger 2026-09-22 16:36 c3cb3b310; 2026-09-23 13:36 3ec2c9ff39 (the close brief's derivation repaired 36 defects
    and found 1-3 before the close ran); 18:11 922994cec3; 18:39 1a328f3ee8; 22:30 fa18863b94. Draft lessons 58, 59 and
    82 merged; §7 lesson 35 holds the adversarial derivation itself. -->
52. **At H3, commit `docs/phase4/data/population-go<rel>.txt` and `relocations-go<rel>.tsv`.** Generate the population
    with `go run ./internal/genpopulation -goroot <incoming GOROOT>` in `src/go2cs`. It must agree by name with an
    independent enumeration (a GOROOT filesystem walk against `go list`). If a pair was pre-staged before the hop,
    regenerate it with the H1 toolchain and require byte-identity. Why: the roster guard, the sweep's `-Hop` and
    push-nuget's preflight refuse without the population, and shardmap must be re-pointed at it (NOTES A9).
    **Step text:** H3; H10's 2026-09-22 amendment.
    <!-- src/check-roster-format.ps1:896-909; src/run-validated-sweep.ps1:251-264 (-Hop refuses a missing population and
    only states a missing relocations table); docs/phase4/hopA-inputs/shardmap.py:94 (hard-codes the hop-A file);
    ledger 2026-09-30 04:05 5b011e7cbf (the go1.25.14 population, pre-staged); 04:12 4304774785. -->
53. **Before reusing an inherited population rule, run it against the current banked set, and drop any rule that
    subtracts a banked row.** Why: membership is a property of GOROOT under the corpus tags, never of `src/core`. The
    1.23.12 rule "a production .csproj exists" would have dropped three banked test-only rows. **Step text:** H3.
    <!-- ValidatedTestPackages.md:1350-1359, :1376. -->
54. **At H3/H4, census the new release's constructs and pair each one with a behavioral probe.** These are `//go:embed`
    directives (production and test, named and blank `embed` imports), `//go:linkname` targets the Go linker synthesizes,
    and new syntax such as generic type aliases. Why: each was first found at H10 or later, and a blank-import embed
    regression reddened a batch. **Step text:** H3.
    <!-- ledger 2026-09-22 00:43 769fc17fb; 05:57 (battery tip, local only); 2026-09-24 01:50 478e78429b; 2026-09-26
    04:19 db1bd885a2 (go:fipsinfo, generic aliases). -->
55. **Build GolibTests under `GoTargetOS=linux` in every battery.** Why: it had not built under linux since 2026-09-15,
    and nothing noticed for eight days. **Step text:** §6.
    <!-- ledger 2026-09-23 08:51 971d919113 (b293973e9f). Extends §7 lesson 30. -->
56. **Keep each target's converter stderr from H5's seeded reconvert, and diff its warning counts by kind against the
    outgoing pin's. Size a new kind as a predicted H7 red.** Why: a "not erased; emission may not compile" warning fired at
    all 29 sites of one H7 red class on every conversion since the hop, while every A/B sent stderr to /dev/null.
    **Step text:** H5.
    <!-- BOARD:25412-25418 (RED 8); claude/mailbox 3d15626145. -->
57. **Print each class's census, taken at both pins, per class and never as one total. When a hand-own closed a class
    through a Go fork idiom, re-census that class in every package that carries its own copy of the fork.** Why: a total
    read "unchanged" while 3 sites changed class. Three native-boundary members also arrived at 1.24 in
    internal/syscall/windows. Two sat in its own copy of a fork that syscall/windows had already closed, and the third
    (NtCreateFile) ended the test host on row 48.
    <!-- BOARD:25272-25341 (RED 5), :25592-25597; 9a7789127a. -->
58. **Run H5c's delete as `reconvert-deletions.ps1 -Apply`, with its eight classes and `-BuildTags`/`-TagLine`. Its exit
    0 needs zero UNRESOLVED and zero UNEXPLAINED-DESELECTION.** Why: the stage text still prescribed the 2026-09-13
    interim script and a six-class table, both older than the instrument's amendments.
    **Step text:** H5 Gate; H5c step 5 and its class table.
    <!-- src/reconvert-deletions.ps1:120-135 (the two newer classes), :240-246 (exit codes), :1259 (class order), :297
    (-BuildTags); 172d437e66:docs/GoCorpusMigration.md:631, :695-704, :1020; c8ee9bb6a7; b291530e95. -->
59. **From H2 to H5, run repoguard and the registry guards that read Go source with GOROOT at the CORPUS pin. Answer
    "does this package exist" from `git ls-files`/`git ls-tree` at the ref, never from `os.Stat`.** Why: a newer GOROOT
    reported a false linkname regression, and removed directories read as absent, empty or populated depending on the box.
    <!-- BOARD:25674-25692. -->
60. **Cut the retired-hand-own guard (H6's durable form) after each hop closes and before the next hop's H6, baselined
    on that hop's `logs/handowns-outgoing.txt`.** Why: missing the comm step let sha3's fabricated `array<T>` class return
    while H6 passed, and repoguard has no such guard at 172d437e66. **Step text:** H6.
    <!-- 172d437e66:docs/GoCorpusMigration.md:1256-1262, :1288, :1318-1321; NOTES-next-hop.md A15 (owed: the
    entry itself is not in NOTES yet; a separate seat adds it). -->

#### H10

61. **Before pass 1, run
    `src/run-h10-recon.ps1 -SelfTest -Tree <worktree> -NameList x -GoRoot x -Out x -ExpectTip x -Scratch x`
    under every shell a launcher uses. Its row-invocation contract must pass: the deadline is the longer of the derived
    floor and the asked `-TestTimeout`, and every row carries its roster execution pin.** Re-key `$longTimeouts`
    (run-validated-sweep.ps1:1203) to the release's relocations, and check it against the recon walls: raise a floor when
    its wall passes 0.75 of it, never lower one. Why: the floor and pin defects were found only at pass 3, after rows read
    without their pin had been reported as converter defects. **Step text:** H10 step 5.
    <!-- Fixed at c8c77eadff (the wrapper takes max(asked, floor) and the roster pin, with the 1.24.13 floor re-check),
    branch tip d095fe8108. The self-test's contract cases are at run-h10-recon.ps1:616-810. Ledger 2026-09-22 14:52
    (RULING); 16:17 and 16:31 3469154a95; 18:02 d095fe8108 (the floor re-key was H10 step 5's routine re-check). Extends
    §7 lesson 2.
    Corrected at the apply, 2026-10-02. The draft gave the bare `-SelfTest`. The script's six other parameters are
    Mandatory (run-h10-recon.ps1:89-111), so that form fails at parameter binding before any self-test code runs. Under
    -SelfTest only -Tree is read (:238-242, :634-650: a per-run copy takes the sweep's and the roster's rules from the
    tree), so it names the worktree and the other five take any value. Read at e2008427b1 under pwsh 7: rc 0 and
    "SELF-TEST PASSED". $longTimeouts is at run-validated-sweep.ps1:1203 at e2008427b1 (:1189 at 172d437e66). -->
62. **At the END of pass 1, run three checks.** The first is a page census: every row's first proof link is at the new
    release. The second is the population identity, enumerated two ways: dispatched + unscheduled = population. The third
    re-dispatches every NOVERDICT or hand-stopped row at a raised budget. Why: two rows were in no shard plan and 14
    banked rows had never been measured, which cost a whole pass 3. **Step text:** H10, THE WORDS.
    <!-- ledger 2026-09-22 11:27 f545b18d4d; 14:45 d5414aa151; 16:03 c660a17d8c; docs/phase4/hopA-inputs/shardmap.py:974-980. -->
63. **Land and red-prove the allocation-unit extractor before H10 (NOTES A14). Class every alloc label against the plan
    bar (`docs/ConversionStrategies-Reference/manual-conversions.md:3740-3748`) at the FIRST classing.** Why: the hop
    retired the bare `alloc-profile` label, and the host records only the first nonzero `AllocsPerRun` unit. The relabel
    took 122 entries in 24 manifests, 27 rows read and 28 pages re-banked, and six early structural pins had to be
    reversed. **Step text:** H10 step 3.
    <!-- Ledger 2026-09-22 00:43 769fc17fb; 11:24 eeed65f66d; 16:39 c8e6ca9034 (the bare label retired); 2026-09-23 02:17
    ac9f8251ee (the 27-row reading run, extractor proven first); 03:37 718060141d; 07:12 74bae27672; BOARD:25854-25883
    (the six pins reversed). Extends §7 lesson 31. -->
64. **Take the linux leg as a READING in parallel with the windows recon. It reads and annotates every linux-eligible
    banked row, and a closing check asserts, by name, that the annotated set equals the eligible set.** Why: re-banking
    only the rows already annotated left 30 eligible rows read and never annotated. **Step text:** H10, Order and launch;
    close step 11.
    <!-- ledger 2026-09-23 06:18 5da426433b; 2026-09-26 06:01 and 06:34 3ffd1d8a8d. Extends §7 lesson 30. -->

<!-- Lesson 65 of the owner-approved draft is HELD for a COORD ruling and is not in this seat (2026-10-02). Its number
     stays reserved, so 66 onward keep the draft's numbering. The draft read: "Pass GoTargetOS=linux to every linux
     runner, the behavioral one included. Each runner refuses a non-windows host without it, and the uid is recorded
     beside every linux reading." At e2008427b1 no runner refuses: the refusal ruled at ledger 2026-09-22 03:45
     c6fdbe73c (a) was never cut. The converter passes -p:GoTargetOS itself for a -tests row
     (src/go2cs/testConversion.go), src/_paths.ps1 pins the variable from the host for the ps1 instruments, and the
     linux driver template unsets it, aborts if it is exported, and passes it per command to the behavioral runner and
     GolibTests only (.claude/coord-scripts/templates/linux-legs.sh). -->

66. **A measurement commit is on origin before anything cites it.** The leg report shows `git ls-remote origin <ref>`
    returning the cited SHA, and the raw results JSON is kept until COORD has read it. At the close, a planted check
    confirms that every SHA a proof page, roster cell or bank commit cites resolves on origin
    (`git branch -r --contains <sha>` is non-empty after a fetch), and that one planted unpushed SHA is named. Why:
    crypto/tls's page cites a converter commit, 4c5b0a3b7, that was never on origin. **Step text:** H10, HOW A ROW
    BANKS; close step 11.
    <!-- coord-handover docs/phase4/briefs/h10-close-obligations.md:94 (c14); ledger 2026-09-22 22:11 8179b65f16 (the raw
    JSON behind the 1,788 s BoGo reading purged by a git clean); 2026-09-23 14:39 faaa8fe999; 14:58 47e088d3d7;
    172d437e66:docs/GoCorpusMigration.md:3148-3159. The instance is still live at e2008427b1:
    docs/validation/current/crypto.tls.md:9 (and the 1.24.13.1, 1.24.13.2 and 1.24.13.3 snapshots) cite converter
    4c5b0a3b7. Extends §7 lesson 33. Draft lessons 72 and 84 merged. -->
67. **Gate every shard ref on one checklist.** It stages test artifacts only, from a named list. It restores any
    production `.cs` a `-tests` run rewrote. It carries a rowless PASS package's artifacts and page, and it commits the
    GOROOT fixtures the run staged. Tests is set to the page's MATCHED count. Why: 6 to 61 production files were restored
    per shard, three rowless PASS rows were re-run, the header overstated matching by 120, and go/printer's fixtures kept
    their go1.23.12 bytes. **Step text:** H10, HOW A ROW BANKS.
    <!-- ledger 2026-09-22 04:01 7227138c2; 04:05 06ac80d68; 04:43 06f37e478; 06:33 4099c204b; 06:38 4099c204b; 15:42
    3469154a95; 2026-09-30 15:16; 895a2e612b. -->
68. **Until NOTES A3 lands, read every pinned row outside `run-validated-sweep.ps1 -Hop`. Land A3 before the next hop's
    H10, firing-controlled on log/slog, with a relocated row taking its predecessor's pin.** Why:
    `Get-HopPopulationRows` still builds every row with `Execution = $null`, the class c8c77eadff fixed in the recon
    wrapper. **Step text:** Readiness, Instruments; H10 step 1 and its launch amendment.
    <!-- src/_roster.ps1:578 at 172d437e66 (:585 at e2008427b1); 172d437e66:docs/GoCorpusMigration.md:2292, :2377;
    NOTES-next-hop.md A3 and §6.
    Corrected at the apply, 2026-10-02. The draft, derived at 172d437e66, read "firing-controlled on internal/godebug,
    log/slog or net/http". net/http dropped its release-tiered pin on 2026-10-01 and internal/godebug on 2026-10-02, so
    log/slog is the one row that carries a pin at e2008427b1 (src/_roster.ps1:121-124, ValidatedTestPackages.md:527-530),
    and a firing control on either of the other two would be vacuous. -->
69. **Before the relabel accepts a "structural" label, read the signed-off design records for it and for every alloc
    ruling made before the bar. If a design stage removes the cost, the label is "deferred".** Why: COORD reversed six
    structural pins, and the owner-confirmed 2026-08-10 crypto/rsa ratification fell to its own emission.
    **Step text:** H10 step 3.
    <!-- BOARD:25854-25916. -->
70. **In the same reading run, check each unit-claiming disclosure class against every entry's measured unit, and list
    every deferred entry whose run moved past a stated ratio as an unattributed MOVE row. Keep doing this until the
    sweep-side reading-vs-run comparator lands.** Why: 7 of 10 alloc-count-semantics entries read a count, and P256 (8,528 →
    16,149), crypto/rsa and database/sql moved with no gate comparing reading to run. **Step text:** H10 step 3.
    <!-- ValidatedTestPackages.md:508-513; BOARD:25963-26037 (REC-F (v), BOARD:25987). -->
71. **At re-sign, list on both OSes every failure line that each carried disclosure signature matches. Narrow any pin
    that absorbs more than its own rows, retire orphan pins, and keep new pins' evidence lines away from record tails.**
    Why: runtime/debug's `TestStack` pin absorbed five assertions and unbanked the row at TRAIN G. **Step text:** H10 step 3.
    <!-- ValidatedTestPackages.md:1290-1302; ledger 2026-09-28 03:19 7ae74437c2; 06:55 3d56a90fbe; coord-handover
    trainK/tK-seats-draft.txt:53 (a test-order seat truncated runtime/pprof's record past its pinned line). -->
72. **Record any host capability a row's count depends on (symlink privilege, the junction fallback, root), and
    register the row as a host-conditional verdict under ValidatedTestPackages.md's rule (:123) before it banks.** Why: os
    banked at 1103 and then 1105 on a different host, and read COUNT on every unprivileged sweep.
    **Step text:** H10, HOW A ROW BANKS.
    <!-- ledger 2026-09-26 08:45 3ffd1d8a8d; 10:33; 11:05 d4c9dd8305 (internal/trace failed four leaves on junction-
    fallback hosts). The rule is at ValidatedTestPackages.md:123 at e2008427b1 (:116 at 172d437e66). -->
73. **Drop a row's execution pin only on TC0 greens from every box that banks or sweeps it.** Why: log/slog's drop was
    ruled on two boxes, then failed `TestPanics` and `TestSetDefault` at TC0 on the i7 battery box and was reversed.
    <!-- coord-handover trainL/tL-seats-draft.txt 16:17, 17:03; ledger 2026-10-01 16:18; 17:03. -->
74. **Dispatch every E4 and E2 exclusion row as a non-banking READING row, and record its matched count.** Why: shardmap
    treats exclusions as not dispatched, yet runtime/trace left E4 at TRAIN G and moved the denominator from 224 to 225
    mid-campaign.
    <!-- docs/phase4/hopA-inputs/shardmap.py:977-978; ValidatedTestPackages.md:1051-1074, :1111-1114. -->

#### Close – H12

75. **At the campaign-tip fold (§7 lesson 25's second fold), rehearse the close's two-seeded, three-target `-stdlib`
    regen and its post-regen gates from per-run copies:**
    - `go test ./... -count=1`, repoguard included;
    - `dotnet build src/go2cs.slnx -c Release -p:go2csPath=<repo>/src/` (the close's B9(f));
    - the README-badge check and the csproj diff.

    Why: the close seat stopped three times, for about 4 h, on R-B13 through R-B16, and an unpinned Release solution
    build reads the deploy root even in-solution. **Step text:** H10 close step 9.
    <!-- ledger 2026-09-23 18:11 922994cec3; 18:39 1a328f3ee8; 21:43 1a328f3ee8 and claude/mailbox 00133ee238 (R-B16: the
    unpinned build read 15,240 errors); .claude/rules/harness-gates.md:24 as amended at 13e9b039ea; close lesson 7
    (172d437e66:docs/GoCorpusMigration.md:3249-3253) and §6's go2cs.slnx row. -->
76. **Run the fixture-currency check at the close and in every battery: tracked `src/core/**/testdata/**` equals the
    pinned GOROOT's bytes.** Until TRAIN L's guard (fbaa3d1218) lands, run it by hand with `GOROOT` set to the pinned
    release. It prints nothing when every fixture is current:
    `git ls-tree -r HEAD -- src/core | grep -F /testdata/ | while read -r _ _ h p; do [ "$(git hash-object --no-filters "$GOROOT/src/${p#src/core/}" 2>/dev/null)" = "$h" ] || echo "STALE $p"; done`.
    Why: go/printer's `generics.input`/`.golden` kept their go1.23.12 bytes through the whole hop. **Step text:** H10 close
    step 11.
    <!-- 895a2e612b; fbaa3d1218, src/go2cs/internal/repoguard/fixturesCurrent_test.go (red at exactly those 2 of 1,276
    fixtures; testdata trees are -text, so blob hashes compare verbatim); ledger 2026-09-30 15:16; 2026-10-01 15:16;
    NOTES-next-hop.md A16 (owed: the entry itself is not in NOTES yet; a separate seat adds it, and it records a
    LANDED guard). The lesson's text is the draft's, derived at 172d437e66. TRAIN L has landed since (master
    aa0a07d5fd, 2026-10-02): the guard is repoguard's TestTrackedFixturesMatchPinnedGoRoot, in the tree at e2008427b1.
    It reads the committed tree (git ls-tree -r HEAD), so it runs with -count=1 after the bank commit. Close step 11
    names it. -->
77. **During the hop, gate every train landing on `release-nuget.ps1 -VerifyOnly`.** Why: runtime/debug was unbanked at
    TRAIN G but its page was left in place. push-nuget's release pre-flight was then red at master from TRAIN G until the
    i9's closure pack found it on 2026-09-29. **Step text:** §6.
    <!-- ledger 2026-09-24 00:10 677e3715fb; 02:10 bccf8d977b; 2026-09-29 18:28. -->

#### Fleet

78. **Before launch, commit each battery, batch, shard-leg, close and temporary-host brief, with its per-run script copy,
    on `claude/coord-handover`, derived by a read-only workflow with adversarial checkers.** Why: checkers caught 36, 38,
    39, 28 and 25 defects before launches, and a COORD lost to the weekly limit resumed from the branch.
    **Step text:** Readiness, Fleet.
    <!-- ledger 2026-09-21 17:36 54bbb29f5 (the weekly-limit resume); 2026-09-23 00:20 c0f11384e4; 13:36 3ec2c9ff39;
    23:41 fa18863b94 (38); 2026-09-24 02:02 f223c19182; 2026-09-30 13:33. Extends §7 lesson 35. -->
79. **Every seat, docs seats included, runs plain `go test ./... -count=1` in `src/go2cs` at its own tip before
    admission, and the union rehearsal runs it again.** Why: union-only reds recurred at nearly every train, twice in a row
    from a docs seat failing repoguard's kramdown guard.
    <!-- ledger 2026-09-22 08:18 ae2f25198; 2026-09-25 00:08 ff67c57420; 2026-09-29 12:43 d51a62eaad; 2026-09-30 14:02;
    coord-handover trainK/tK-seats-draft.txt:51. Extends §7 lesson 15. -->
80. **At each status, read every lane's last post and unread count.** A lane is declared OFFLINE in one ledger line, with
    one OWNER-HAND, when it has no post for about 6 h while holding unread COORD messages, has a failed delivery, or is
    missing from ListAgents. Its open items are rerouted by name in one ledger RULING, and a returning lane reads its whole
    inbox before its NEXT. Why: four lanes silent for 15-18 h stalled a TRAIN J landing gate, and a reroute met it 12
    minutes later.
    <!-- ledger 2026-09-30 00:26; 00:38; 16:09; 2026-10-01 19:16; reroutes 2026-09-27 17:27 05d7067bea; 2026-09-30 04:03
    f819887fa3; 2026-10-01 06:22. Draft lessons 92 and 95 merged. -->
81. **From H0, only COORD changes the comms tools (the inbox scripts and the ledger append), and each status line
    reports protocol work as a share of output.** Why: on 2026-09-20, 59% of fleet output was measured as protocol work.
    <!-- coord-handover RESUME-SESSIONS.md:789, :59-61 at 91e969cedb; ledger 2026-09-20 13:15 77a03bd82;
    2026-09-28 19:28 bad6fcf862 (the owner order taking the tools out of the lanes' hands). Extends §7 lesson 3. -->
82. **Send one message per deliverable, at most 40 lines, as WHAT (1 line), EVIDENCE (up to 10 lines; anything longer
    is a file on the lane's ref) and ASK or NEXT (1 line). COORD does not echo, and silence after a ruling means
    proceed.** **Step text:** Readiness, Fleet.
    <!-- coord-handover RESUME-SESSIONS.md:50-53, :796-814 at 91e969cedb. -->
83. **Poll origin by `ls-remote`, never by fetching into the clone COORD pulls from. Before re-dispatching a lane that
    ListAgents shows as "idle", read its last message.** Why: a racing fetch handed a lane a corrupt loose object, and G's
    "idle" was its own background battery.
    <!-- ledger 2026-09-22 04:21 6dbe347c0; 14:52 (RULING). -->
84. **Each lane's resume ACK states its wake mode, either a message event or a background inbox watcher, and COORD
    verifies one round trip.** No session carries a watcher or loop id as state. Why: cloud sessions wake only on message
    events, and a cloud lane's evidence sat unread for 25 minutes.
    <!-- coord-handover RESUME-SESSIONS.md:771-776, :822-824 at 91e969cedb; ledger 2026-09-28 19:28 bad6fcf862. -->
85. **`docs/phase4/RESUME-SESSIONS.md` and its verifier exist before H0 and are refreshed at every landing or ruling
    STAMP (the `save-state` skill).** **Step text:** Readiness, Fleet.
    <!-- 271300cea0; ledger 2026-09-24 05:02 a59b194e16; 2026-09-27 14:29 934d5f2409; 2026-09-28 19:28 bad6fcf862. -->

<!-- Lesson 86 of the owner-approved draft is HELD for the skills seat and is not in this seat (2026-10-02). It points
     at the `train-assembly` skill for the hop's train and battery mechanics (conflict maps by real trial merges, union
     template-currency checks, launch-check tables, isolation re-runs, complement-shard bisection and frozen unions),
     and the skill does not hold them at e2008427b1. The draft's placement note lands 86 with or after the seat that
     adds them (draft lessons 97 and 102-110, routed as §7's intro routes instrument discipline). -->

<!-- SEED COVERAGE (owner order 2026-09-23; ledger 2026-09-23 14:52 · OWNER ORDER · c9e15c73a0), each verified against the cited source:
  1 hosts before the hop: DNS (relay NXDOMAIN->SERVFAIL, per-box public IPv4, IPv6 unbound) .......... 42 (+ the Readiness Host bullet)
    many-core Windows host, local or cloud, for crypto/tls's 600 s wall; cloud quota early ............... 40, 44
  2 new linkname partials surface first on non-Windows legs (vgetrandom); read linux early ............. 50, 55, 64 (+ 65, held for a COORD ruling)
  3 reading run -> relabel -> rulings for alloc labels; host records only the first AllocsPerRun unit .. 63, 69, 70
  4 wrapper defects (floor lowered an asked timeout; execution pins not applied), c8c77eadff / d095fe8108 61 (+ 68 the open -Hop twin, 43 release derivation)
  5 push measurement commits (a page cited unpushed 4c5b0a3b7) .......................................... 66
  6 close derivation catches (costed basis row; //go:embed + -text; emitter-moved flat files git rm'd) .. 51 (+ close lessons 1-3), 76
  7 one message per event ................................................................................ 80, 81, 82
  8 sub-agent batteries with briefs on coord-handover .................................................... 78 (+ 86, held for the skills seat)
  9 go2cs.slnx Release gate carries -p:go2csPath (B9(f), R-B16, mailbox 00133ee238) ..................... 75; the rule's opening
    line is owed (the draft's edit B33 to .claude/rules/harness-gates.md:24, not in this seat); its body already carries the non-Debug clause (13e9b039ea)
 10 repoguard payload admit for //go:embed outside testdata (R-B14, de436207c9 / f0ada4471e) .............. 51, 75
 11 -stdlib staging seeder carries docs/validation (R-B13, 781c1c3c31) .................................... 51, 75
 12 csproj merge renderer keeps post-block groups (R-B15, ae813db069) ..................................... 51, 75
 Excluded by the owner: one host's temporal hardware issues. Lesson 40 is host-generic. -->

---

## Sources

- [`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) — the ruling frame, the release research for
  the ladder's remaining rungs, the risk register, and **§8's nineteen ruled open questions**, which
  every "(ruled)" above resolves against (toolchain stamp, fresh baselines, module directive,
  build-number reset, roster gate on absolute count, version monotonicity, removed-package
  disposition, audit home, audit population, experiment-gated packages, test-host ownership). Its
  §2/§3/§4 are pointer shells into this document, which now maintains the inventory, the hand-own
  audit and the parity gates
- `CLAUDE.md` — the reconvert ritual and its marker-gate traps; the false-green route catalogue; the
  post-sweep dirt classification; the measured budget table; the concurrent-session and detachment
  rules; the banked-row merge protection
- [`ValidatedTestPackages.md`](ValidatedTestPackages.md) — the roster grammar its parser and format
  guard enforce, the disclosure classes, and the signature-pinning rule §4 depends on
- [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md) — hand-own detail and the
  disclosure classes' full definitions
- [`DotNetMigration.md`](DotNetMigration.md) — the companion runbook, and §5.2 of it for the
  embedded-asset false-green route §1.2 references
- Source read directly: `src/go2cs/toolchainResolution.go` (the pin guard and its prescriptive error
  text); `src/go2cs/directiveOperations.go` (`releaseTagsForVersion`, minor-keyed);
  `src/go2cs/conversionDriver.go` (source type-checking, the rule in §1.2);
  `src/go2cs/embeddedTemplates.go`; `src/run-validated-sweep.ps1` (serial by design, the exact-match
  filter, the toolchain pin, the disk preflight, the long-timeout floors); `src/_roster.ps1` and
  `src/check-roster-format.ps1`
<!-- {% endraw %} -->
