# Migrating the go2cs corpus to a new Go version

> The standing runbook for moving the converted standard library — and everything derived from it:
> the goldens, the validation roster, the proof pages, the disclosure manifests, the published
> packages — from one Go release to the next. It is **version-agnostic by design**: it names
> instruments, gates and traps, never a particular release.
>
> **This runbook leads.** It is the living procedure for a corpus migration: the canonical
> H0–H12 (+H4a) step inventory is the one maintained **here**, amended in-stage as lessons are
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

**This section is the canonical H0–H12 (+H4a) inventory and its procedure.** It was generalized from
[`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) §2, which now points here; the ⟨OQ-n⟩ rulings
behind each "(ruled)" remain recorded in that plan's §8. **Steps marked GATE are pass/fail and block
the next; steps marked ⟲ are re-measured at every migration and never carried forward.**

**Three orderings are not negotiable**, and each has a mechanical reason rather than a preference:

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

### H0 — Baseline capture ⟲

Capture, on the **outgoing** toolchain and the **new** converter build, everything the migration will
diff against: the hand-own `.cs.auto` baseline, the package census, the roster snapshot, the
disclosure manifests.

⚠ **Generate the `.cs.auto` baseline fresh, from a seeded old-release regen — never from the committed
siblings.** The overlay rule excludes `*.cs.auto` in order to protect the hand-owned `.cs` beside it,
so the tracked siblings are **frozen on their own schedule** and a materially stale baseline poisons
the differential. This is a ruled decision, not a preference.

### H1 — Toolchain provisioning **GATE**

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

### H3 — Package census ⟲

Diff the conversion queue's package set against the outgoing corpus: **added**, **removed**,
**renamed or promoted**, and **experiment-gated and therefore deliberately absent**. The last category
matters as much as the others: experiment-gated packages stay out until they graduate to the default
package set (ruled), and naming them explicitly is what stops a later reader re-diagnosing a
"missing package".

Deliverable: a census document under `docs/phase4/`, in the shape of the existing census docs. **A
patch-level migration should produce an empty census; a non-empty one is a finding.**

### H4 — Converter feature work **GATE**

Whatever the release's language delta requires, plus whatever the census surfaced. Each item follows
the standing repository discipline: root-cause against emitted `.cs`, land a behavioral regression
test, update the conversion-strategy reference (and the summary only if the headline mapping moved),
and prove `check-no-regression` clean **on the outgoing corpus** where the change is meant to be
neutral.

**Two recurring work items belong here by ruling rather than being discovered as audit findings:**

- **The hand-owned test host.** `src/core/testing` is skip-listed and never converted, so it follows
  **nothing** automatically while upstream keeps adding to `testing`'s API. It is a named work item of
  every migration that adds one.
- **The `go.mod` readers.** New `go.mod` verbs are silently dropped by lax parsing, so any new
  directive in the target release owes a re-check of the converter's `go.mod` handling rather than an
  assumption of safety.

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

**A corpus migration is that regen.** Schedule the bundle **before H5**, for one reason: H5's overlay
diff is the migration's primary signal, and every un-levelled artifact is noise inside it. Levelling
first is what makes the upstream delta readable.

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
- **Never convert twice into one staging root**, and never let two conversions overlap in one. Delete
  and re-seed per run, and confirm no converter process is alive before starting. The recorded failure
  is a single corrupted file with unresolved lift markers that reads exactly like a converter
  regression and is not one.
- **Wrap the converter call so its stderr warnings do not abort the wrapper** — a terminating
  error-action policy turns a native stderr line into a fatal, which is how the overlapping-run
  corruption happened in the first place.
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

**Gate:** overlay completes with the marker gate at zero violations and **every diff classified**
(§4).

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

Mechanically: re-measure the line-anchored census over `src/core`; assert every marked path appears
exactly once in the audit file; assert every row's class is one of `unchanged`/`a`/`b`/`c`; assert
every `b` carries a non-empty reason and every `c` a work-item reference; assert **zero** rows in the
"no `.auto` emitted" state. Exit non-zero on any violation — the same shape as the repository's other
preflights: cheap, by-path, and impossible to pass vacuously.

Deliverable: one audit file per migration under `docs/phase4/` (ruled), rather than per-package notes,
because the completeness gate must be checkable in one place.

### H7 — Compile parity **GATE**

Full `go2cs-stdlib.slnx` build with shared compilation disabled, zero errors, **skipped-dependents
enumerated and zero** (a dependent of a failed project is skipped, not errored — count them). Run
**every** buildable `$(GoTargetOS)` flavor, purging `bin`/`obj`/`Generated` between switches.

**Gate: 100 % of the migration's package set compiles.** Not "as many as before" — 100 %, per the
frame.

### H8 — Multi-platform re-emission **GATE** ⟲

Re-run the multi-target emission and the platform census, and diff the manifest against the outgoing
one. A migration changes the platform axis in **two** directions at once: new packages may be
platform-varying, and existing ones may stop being so. The per-GOOS package count is a measurement,
not a constant.

**Gate:** the platform manifest's marker gate is zero per target, and the default-flavor build
reproduces the single-target build byte-for-byte.

### H9 — Behavioral golden rebank **GATE**

Behavioral goldens are conversions of go2cs's *own* Go programs, so a corpus migration reaches them
through §1.1's three channels — and which are live is knowable in advance. **Predict the diff's size
before running the rebank**; a diff that materially exceeds the prediction is a finding, not a rebank.

Procedure: re-transpile everything **first** (the golden-update utility copies on-disk `.cs`; it does
**not** re-run the converter, so a copy over stale output silently re-baselines it), then update the
goldens, then **classify every moved golden before banking**. A migration is not a licence to rebank
unexamined diffs.

**Gate:** the full behavioral suite green across all four phases. Note the runner's **own** internal
budgets are independent of the caller's, and a budget that expires reports `NOT MEASURED`, which fails
the run and must **never** be read as a corpus regression.

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
   standing production-flip classes at the end. A wrapper `-Hop` mode is open instrument debt.
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
4. Regenerate the proof page and let the README validation badge recompose from it.
5. **Re-check the per-package deadline floors** in the sweep's long-timeout table — a migration can
   change a suite's cost.

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

**One arithmetic cross-check, free and worth running here:** the count of banked test project files
must equal the roster's row count. It is the committed-evidence half of the green-badge rule, and if
a migration ends with the two unequal the badge census miscounts — loudly, by design.

### H11 — Publication and compatibility guards **GATE**

- The published version is the pinned Go release plus the build counter, already set at H2.
- **Verify version monotonicity with a scripted comparison before the first publish**, never believe
  it. A non-monotonic sequence on a public feed is not correctable.
- The NuGet compatibility guard follows the migration for free, because it reads the converter
  binary's own runtime version — which H1 rebuilt. That coupling is exactly the H1↔H2 window §2 warns
  about.
- **New packages need new package IDs; removed packages need a disposition** — ruled: **deprecate,
  with a pointer to the last release that carried it. Never unlist.**
- The published-release stamp is a **repository-recorded fact** written by the publish ritual; a feed
  query is advisory only, never the gate.

### H12 — Docs, badges, READMEs **GATE**

- **Every validation badge on every package README moves** as a matter of course: two of them read the
  toolchain and follow H1, two read `version.props` and follow H2. **State the expected diff size
  before the overlay** so it is not mistaken for drift.
- **The hand-owned READMEs do not follow.** They are hand-edited, and their edits are *derived and
  proved against the converter's own output*, never typed. **Re-run that derivation as a control** at
  every migration.
- **The GOROOT-vendored `golang.org/x/*` packages re-pin** from the new GOROOT's own vendor manifest —
  on a patch-level migration this is the badge family most likely to actually move.
- The Go version appears in prose in the top-level docs, the roadmap, the roster and CLAUDE.md's
  architecture row.
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
unrecoverable afterward. Make it an obligation of that sweep, not of this step.

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
| **T2 · test-closure re-emission** | one of the named shapes an `-stdlib` and a `-tests` emission differ by — an import alias, a namespace root escape, the using-block reorder an alias causes, or the test-init hook a `-tests` run adds as **real lines** an `-stdlib` run omits | **Restore.** A standing restore, not a cleanup, until the two emissions agree. ⚠ the hook shape survives a numstat filter |
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
| **Behavioral parity** | all four phases green, **zero** `NOT MEASURED`, every moved golden classified T0–T4 with **zero T5** |
| **Hand-own audit** | §H6's completeness gate passes: every marked path in the **re-measured** census appears exactly once; every (b) carries a written reason; every (c) a work item; **zero** "no `.auto` emitted" rows |
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
| `go2cs.slnx` | **yes** after any golib/runtime API change; it is the only gate compiling the non-generated members |
| full behavioral suite (four phases) | **yes**, at H9 and at the parity gate |
| seeded full reconvert | **once** per phase — H4a's bundle and H5. Never twice into one staging root |
| multi-target emission + platform census | **yes**, at H8 |
| full validated-roster sweep | **once**, at the parity gate: coordinator-owned, backgrounded, on the fastest available machine, **never parked by a lane** — a lane's process tree is reaped at its turn boundary, and sweeps have been lost to exactly that. Recovery is `roster − logged`, re-run inline, with the verdict arithmetic checked to close |
| release-ritual dry run | **yes**, at H12 |

Budget every one from CLAUDE.md's measured budget table, **from the top of each range**, and
**re-measure and update the table** when a healthy run exceeds a row. A stale baseline is what makes a
healthy run look hung — and, in the other direction, what lets a hung one look healthy.

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
