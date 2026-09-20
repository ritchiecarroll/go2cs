# DRAFT — the H10 DRIVER's brief (the re-bank of the roster at go1.24.13)

**DRAFT for the coordinator. Nothing here is ruled.** Tags: `[RB]` = settled procedure in
`docs/GoCorpusMigration.md`, cited by sentence text (that file's own rule — a line citation goes stale
when either file moves); `[RULED <sha>]` = a mailbox ruling; `[OQn]` = needs a ruling, argued once in
§G. Nicknames only; the plan's `-Worker` values are **copied verbatim out of the plan file**, never
typed from here — the driver refuses with *"the name must match the plan EXACTLY … quote it"*.

## A. What the driver is

`[RB]` H10 re-derives every row from the release's own test sources: *"every roster row re-validates
from scratch: numerator, denominator and disclosure set alike. There is no carry-forward path."* The
order is fixed — *"RECON LEG → ROSTER SEAT → PLAN → DRIVER"* — and the first three landed (master
`711a5925a1`, `f6b011037`). The driver is the fourth. `[RULED 3f70a096e §2]` Population = **226 on the
corpus axis** = 203 banked + 23 candidates; 212 costed and in the plan, 14 `#unscheduled`.

`[RB]` **A row's ACT produces five artifacts and four are `-tests` output**: the proof page
`docs/validation/current/<pkg-with-dots>.md`; its row in the shared index `docs/validation/index.md`;
the green badge in `src/core/<pkg>/README.md` (composed during CONVERSION); the test project
`src/core/<pkg>/<pkg>.tests.csproj` with host and package info; and the disclosure manifest
`src/core/<pkg>/go2cs_test_disclosures.json`, **re-signed, never edited**. The roster row in
`docs/ValidatedTestPackages.md` is the fifth and the only hand act.

**How they become BANKED** `[OQ1]`. The runbook rules the shape and leaves the mechanism to the
migration: §3.5 gives the branch shape (*"each shard branches from [the version branch], never from
master"*), the hotspot rule (*"A shard edits only its own rows. The coordinator recomputes the
header."*) and incremental merging. Proposed: (1) each worker cuts **one lane ref per shard** off the
version tip — `claude/<lane>-h10-rebank-s<N>` — carrying only that shard's artifacts; push-then-announce
on a new ref, census-gated via `.claude/coord-scripts/coord-identifier-census.sh`, never `git add -A`
(floor 8). (2) The coordinator merges them as **trains, one leg at a time**, re-asserting the checksum
after each. (3) Roster figures are **derived, never hand-set**: `src/check-roster-format.ps1` already
recomputes the header from the table (validated count == row count; verdicts == Tests column sum;
disclosed == Disclosed column sum; the percentage follows; the honest-set arithmetic). Worker edits
rows; the coordinator takes the header from the guard.

⚠ `[OQ2]` **`docs/validation/index.md` is one shared 242-line file every row rewrites** — measured on
the wrapper's one-row dry run: *"a SINGLE row re-banked bufio's proof page … and **removed 25 lines from
the shared index**."*

⚠ `[OQ3]` **`src/run-h10-dispatch.ps1` as landed cannot perform this act.** It dispatches
`run-validated-sweep.ps1 -Filter <pkg> -Exact`. H10 forbids that: *"**never the sweep wrapper** … the
steady-state gate, enforcing the exact banked count and a drift-clean corpus — both of which this step
invalidates **by design**"*; and the sweep *"selects among BANKED rows … so it cannot reach the twelve
successors or the unbanked rows at all"* (recon wrapper header). The runbook already says so of this
script: *"the sweep's per-row dispatcher for the campaign's **steady-state** passes, **after rows
re-bank**."* The re-bank needs a pipeline mode.

## B. Preconditions per worker

The leg's list `[RULED — the LAUNCH post]`, updated for a tree whose artifacts are **kept**.

1. `[RB]` **Pins by OUTPUT, three derivations**: `go version` from the `go` **on PATH** (what the
   converter spawns) = go1.24.13; the SDK at `-GoRoot` by absolute path; `<goroot>/VERSION`; and PATH's
   `go` must resolve **under** the pinned GOROOT. `GOTOOLCHAIN=local`, `CGO_ENABLED=0` (the corpus's
   emission state). ⚠ Asserting by absolute path does **not** assert the pin — measured: every row died
   on the ambient toolchain while the preflight read green. Floor 6: spell GOROOT as `go env GOROOT` prints it.
2. `[RB]` **The .NET pair** (`DOTNET_ROOT` + PATH) where the machine SDK lags the TFM — missing it is a
   measured NETSDK1045 wall. Set **inside** the child shell where `pwsh` is a dotnet tool.
3. `[RULED 3f70a096e §5]` **The tip asserted by `ls-remote` or `FETCH_HEAD`, never `origin/<branch>`**,
   and the tree's `rev-parse HEAD` equal to it.
4. `[OQ4]` **A fresh linked worktree at the tip — NOT throwaway, NOT detached.** The recon wrapper
   refuses a tree on a branch (*"nothing can be committed from it by habit"*) because its output is
   readings; the driver's is banked. The linked-worktree guard (`--git-dir != --git-common-dir`) **stays**,
   and floor 11 holds — one worktree per shard, one dispatch per worktree.
5. `[RB]` **Residue census before row 1, with a control**: `git status --ignored=matching` — never
   `--porcelain` alone (it read 0 over 516 ignored entries) — with `[Console]::OutputEncoding` set to
   UTF-8 around **every** `git` call in Windows PowerShell, or the census silently measures a smaller
   tree. Record the number; a non-zero BEFORE owes the two-row arm at the end. ⚠ Census the **whole
   tree**, not `src/core`: `docs/validation/current/*.md` is the known second location.
6. `[RB]` **Disk ≥ 25 GB** (floor 12), purged between runs.
7. `[RB]` **Converter built in that tree** after the toolchain move; whole-solution build run once so
   per-package builds go incremental. ⚠ **Any preflight build runs in a DIFFERENT tree** — a
   `dotnet publish` builds a dependency *closure*, so the at-risk set is not the rows that ran.
8. `[RULED]` **Wrapper and plan taken by blob** (`git show <ref>:<path>` into scratch), **sha256 stated
   in the ACK**; scratch outside any work tree. Floor 4: converter source frozen for the battery —
   launch from a per-run copy of the script.
9. `[RB]` **Converters alive = 0**, counted by **executable path**, positive arm first. ⚠ Never
   `Get-Process <name> | Stop-Process` (floor 5); keep the liveness reading and any kill in separate
   commands. ⚠ A converter census reading 0 does **not** mean the leg is dead — sample the wrapper's own
   PID twice a stated wall apart and read the CPU delta.
10. `[RB]` **A two-sided box preflights BOTH arms** — measured this hop: a box whose native arm passed
    had no pinned GOROOT at all on its linux arm.

## C. The shard assignment

THE PLAN: `docs/phase4/hopA-inputs/h10-dispatch-plan.tsv` at master. Header verbatim: `#version 1` ·
`#basis recon-basis.tsv (sha256 c3715b2332b5f1ef…)` · `#slice_cap_seconds 2400` · `#cooldown_seconds 600`
· `#rows 424` · `#unscheduled 14` · `#projection LOWER_BOUND` ·
`#digest 005aeab497fd35e8610a968117e1836dc68c753ba822e16eb1cee5845c77d39d`. Columns:
`W · worker · slice · seq · package · t_r_i9_seconds · reserved`.
⚠ **There is no makespan line in the plan** — 88.4 min at W=3 and 74.1 min at W=4, both **lower bounds**,
are the generator's stdout, recorded at `f6b011037`.

424 = **two complete deals**, W=3 and W=4, 212 rows each; 212 costed + 14 unscheduled = 226. `[RB]`
`-FleetSize` is mandatory because *"the SAME worker appears in EVERY `W` section with a DIFFERENT row
set"* — a driver grepping its own name dispatches rows it was not assigned.

| worker (nickname) | W=4 rows / slices | W=3 rows / slices |
|:--|:--|:--|
| i9 | 104 (93 + **11 reserved**) / 2 | 123 (112 + **11 reserved**) / 3 |
| R-LAPTOP | 42 / 2 | 50 / 3 |
| i7 | 33 / 2 | 39 / 3 |
| G-LAPTOP (Windows side) | 33 / 2 | **absent** |

`[RB]` **The reserved set is DERIVED from the sweep's `$longTimeouts` and pinned to the fastest worker**
— 11 rows, 624 i9-s, all on the i9 at both sizes: `go/parser`, `go/types`, `index/suffixarray`,
`sync/atomic`, `crypto/dsa`, `archive/zip`, `go/doc/comment`, `hash/maphash`,
`crypto/internal/fips140/mlkem`, `crypto/mlkem`, `time`. Four further declared-reserved rows are
unpinnable for want of a cost (`f6b011037`).
**W=3 vs W=4** `[OQ5]`: W=3 drops G-LAPTOP, moves +19 rows to the i9 / +8 to R-LAPTOP / +6 to the i7,
adds a third slice (and a second 10-minute gap) per worker, and costs +14.3 min of makespan.

`[RB]` **Linux/darwin rows are out of scope** (windows first, per platform parity). The host rule: *"a
row re-banked on windows carries `windows:` at the new release … Hop completion is same-platform by
construction."* A windows reading diverging from a row's previous platform bank is re-read on a linux
host at the same tip before classification. Two rows are windows-only; `n/a` is the marker where a
package has no Go files under a platform's constraints. G-LAPTOP's WSL arm is the **discriminator for
diverged rows, later** (LAUNCH post), not a worker in this leg.
⚠ `[RB]` **Do not let the coordinator absorb the reserved set silently** — say so on the channel.

## D. The per-row act

`[RB]` The invocation, reusing the recon wrapper's per-row mechanics verbatim:

```
go2cs -tests -test-action all -test-config Release -test-timeout <floor> \
      -go2cspath <tree>/src  <goroot>/src/<row>  <tree>/src/core/<row>
```

- **The output directory is the SECOND positional** (floor 3) and it is `<tree>/src/core/<row>` — the
  worktree whose `src/core` **is the seed**. Measured in `-tests` form: a hand-own-carrying row converts
  rc 1 bare, rc 0 seeded.
- **`-test-timeout` from the floor DERIVED out of `run-validated-sweep.ps1`'s `$longTimeouts`** — never
  copied (a copied list drifted twice), with the relocated rows' floors **fanned out to their
  successors**; unfloored rows take the default `30m`. ⚠ Floors are floors: raise for a slower box, never
  lower; a derivation reading fewer than 5 entries must refuse.
- **No `-tags`** — the corpus axis (`purego,math_big_pure_go`) arrives by doing nothing, and a no-tags
  population *"describes a build the hop will never perform."* **No `-test-filter`** — a filtered run
  publishes no artifacts and is diagnostic only. **No `-test-allow-handown`** — it short-circuits the
  host check and is what destroyed the `testing` row (14 tracked files modified, 19 new auto files,
  CS0111, contaminating every later row). `[RULED 3f70a096e §2]` **`testing` is excluded from every
  list** until the converter reorder seat lands; it sits in the plan under the hand-own exclusion as its
  cause and is **never scheduled**.
- **Exit code captured on the very next line**, before any pipe or `$(...)` (floor 7). The converter's
  stderr is a terminating error under `Stop` and **failing is the measurement** — lower
  `$ErrorActionPreference` around that call only, keep `2>&1` (the classifier reads stderr to separate
  CONVERT from BUILD), restore in a `finally`, and **reset `$rc` per row**.

**The words** — the fixed vocabulary, *"filled, never placeholdered"*: `PASS` (0 diverged) → **bank**
the artifacts, the roster row and the format gate; `DIVERGED` (≥1 undisclosed name) → mint/re-sign
disclosures, then bank or route; `CONVERT` (rc ≠ 0 at convert) and `BUILD` (converted, did not compile)
→ **no bank**, a sizing post; `TIMEOUT` (the **results-file tail** states a deadline kill) →
re-dispatch at a **raised** budget; `NOVERDICT` (summary absent · comparison unreadable · **stale
record** · thrown row) → by cause.

⚠ `[RULED]` **A long wall is not a TIMEOUT** — the floor applies to the converter's run; only the results
tail says a deadline fired (floor 14). ⚠ **NOVERDICT banks no count** — `NOMATCH`, never 0
(`a2edbc22d`/`d8d3da990`: a stale comparison record reads NOVERDICT / NOMATCH / UNMEASURED / `n/a`, cause
written to `noverdict-cause.txt`). ⚠ **An unreadable artifact is not a PASS.**

**A DIVERGED row** `[RB]`: disclosures are pinned by **exact failure signature**, so a renamed or
reworded test invalidates its pin and the manifest is **re-signed, never edited**. A disclosure is a
**measured reading** carrying `name`, `class`, `signature`, `reason` — never a tolerance, never a skip.
**Since 2026-09-05 the two allocation labels replace `alloc-profile`**: each allocation-count assert
resolves to `deferred` (carrying `want`, `reading` and `plan` — *the loader and the roster guard both
refuse it without them*) or `structural` (a proof it cannot be met). A hop re-signs every manifest, so
this is where a row's legacy label retires. ⚠ §3.4 false green 2: editing a signature to match is the
**rebased-disclosure launder**. ⚠ False green 3: a silently closed disclosure *"is a good outcome and
still owes evidence: the arithmetic must move, visibly."*

**The relocation rows** `[RULED 3f70a096e §1/§2]`: **nine principal targets bank**, each inheriting its
source's 1.23.12 anchor **exactly once**, on the target taking the majority of the source's banked
**verdicts** (never declarations). **Two secondaries stay CANDIDATES** —
`crypto/internal/fips140/nistec`, `crypto/mlkem` — and bank on a terminal pass like any other candidate.
**No proof file is moved, renamed or created**; each target links its SOURCE's existing
`docs/validation/current` record. That leaves 204 proof files for 203 rows: *"if
`check-roster-format.ps1` refuses a row on that basis, the refusal is the gate being right and the driver
retires it"* — the driver never fabricates a record to turn a gate green.
**The five pins mint at the `crypto/internal/fips140test` re-bank**, in a merged
`fips140test/go2cs_test_disclosures.json` written **on the version branch by the driver, from the
measured reading, never carried forward blind**: `TestEdwards25519Allocations` (1) and
`TestNISTECAllocations/P{224,256,384,521}` (4). `class` and `signature` are **expected** to survive
verbatim (`alloc-profile`, `expected zero allocations, got `) — expected, not assumed; the reading
decides, and under the 2026-09-05 rule those five resolve to `deferred` or `structural`. `fips140test` is
principal for **two** sources, so its row carries 1 + 2,195 validated / 0 + 5 disclosed.

## E. The rehearsal

`[RULED 691bb58a7]` *"its first run is a rehearsal **on one shard**, on a **Windows box**, against the
**version tip after APPLY BATCH 2**."* `[RB — DESIGN §7]` The script **has never executed** — statically
checked only, and *"the coordinator's i7 parse gate in both editions is the next arm, and i9's one-slice
`-DryRun` after it."* So a `-DryRun` of the same shard precedes the rehearsal.

`[OQ6]` **The shard: the i7's `W=4` slice 1 — 7 rows, 829 i9-s, one slice, zero cooldown gaps.** It is
the smallest shard containing a relocated principal target, and that target is its **seq 1** row, so the
riskiest artifact lands first and a red is early:

seq 1 `crypto/internal/fips140/edwards25519` PASS 54 — **the relocated principal target** (54 of 55
verdicts) · seq 2 `internal/godebugs` PASS 1 · seq 3 `database/sql` PASS 140 — **2 committed pins**, the
re-sign path · seq 4 `internal/types/errors` PASS 155 · seq 5 `compress/flate` PASS 64 · seq 6 `crypto`
PASS 6 · seq 7 `html/template` PASS 243. (Words from the banked `recon-basis.tsv`.)

⚠ **No shard of any size carries all three required classes.** The banked basis holds 195 PASS / 12 BUILD
/ **4 DIVERGED** / 1 NOVERDICT / 1 CONVERT, and the two DIVERGED rows carrying pins sit on other workers
(`log/slog` 18 pins, R-LAPTOP; `reflect` 62 pins, i9). So **graft one row by name, off-plan and recorded
as such**: `crypto/internal/fips140test` (73 i9-s; BUILD at recon, expected **DIVERGED ~52** after apply
batch 2, per the seat's own 2,215 matched / 52 diverged). It is the **five-pin mint site** and principal
for two sources — the campaign's single highest-risk act. Total: **8 rows, ~902 i9-s**, one slice.

**Acceptance predicate** `[OQ7]`, all five decidable: (1) the shard's TSV — 8 rows, every `word` filled,
no `NOVERDICT`, `sweep_s` integer on every row; (2) artifacts exist per row — proof page, badge,
`.tests.csproj`, manifest — each written **by this row**, asserted by `LastWriteTime` against the row's
own start (⚠ **never `CreationTime`**: NTFS tunnels it back through delete-and-recreate and calls a
freshly rewritten record STALE); (3) `src/check-roster-format.ps1` exits 0, and the two relocation
orphans it currently reds on at the tip (`crypto/internal/nistec`, `crypto/internal/edwards25519`) are
cleared or **named as hop debt** — the refusal is the gate being right; (4) the roster header
**re-derives** from the table (count, verdict sum, disclosed sum, percentage); (5) `fips140test`'s
manifest carries **five pins**, each with a class and signature, every `deferred` entry carrying `want`,
`reading` and `plan`. Plus one control `[RB, floor 13]` — *a gate never made to fail proves nothing*:
delete one pin from the rehearsal's manifest, confirm the format gate names that row, restore, verify
byte-identical.

**A red rehearsal returns to**: a driver defect → the driver seat, re-cut and re-rehearsed **on the same
shard** (the shard is the control, never a second one); a converter/generator/golib defect → that seat's
lane, and the campaign does not launch; a plan defect (a digest that will not reproduce, a row the plan
cannot reach) → `shardmap.py`, re-emit, rehearsal from row 1. **Nothing from a red rehearsal banks.**

## F. Results and banking

`[RB]` **The TSV.** The leg's eleven columns are the floor —
`row · word · verdicts · sweep_s · first_in_list · rc · diverged · platform · tree · wall_s · post_s` —
read **by name, never by position**, **LF only** (any CR refuses), `sweep_s` an integer or the row is
UNSCHEDULED. ⚠ `sweep_s` is the **converter's** wall, closed before any artifact is read; `post_s` is the
wrapper's own seconds; the wrapper's cost lands on **neither** of the first two. `[OQ8]` The driver adds
`W`, `worker`, `slice`, `seq` (the landed dispatcher already emits these) plus `banked` ∈ {yes, no, debt}
and `manifest_pins`.

**Evidence**, keyed by row under a scratch **outside any work tree**: `<scratch>/<row-with-__>/` holding
`go2cs_test_comparison.json`, a **bounded** `results-tail.txt` with its banner (*"a truncation reading as
a fact … is this fleet's most repeated failure"*), `summary.txt` or `output-no-summary.txt`, and
`noverdict-cause.txt` where one applies. ⚠ The tree is **not** discarded here, but the tracked-count
assert across any cleanup still runs: `git ls-files | wc -l` equal before and after, and
`git status --porcelain | grep '^ D'` empty (floor 8).
**Refs** `[OQ1]`: `claude/<lane>-h10-rebank-s<N>` under each lane's usual prefix, one per shard,
artifacts only — **no `docs/validation/index.md`** (OQ2), **no roster header**.

**Train assembly, per leg** `[RB §3.5]`: merge **incrementally, never in one batch** — *"merged one at a
time, the red row names its shard."* After each leg: the identifier census; `repoguard`;
`src/check-roster-format.ps1`; the checksum re-assert (every row exactly once, header recomputed); and **a
post-merge filtered re-sweep of a sample of that leg's own rows at the merge result** — *"a lane's proof
binds its own tree, never the merge result."* ⚠ The **reflection-consumer canary set is derived at gate
time, never carried** — the largest banked reflection-consuming rows by verdict count, recomputed from
the roster at the moment of the gate.

⚠ `[RB]` **A row in the plan and in no shard's ledger is the campaign's one unrecoverable failure mode —
make it a gate, not a hope.** Closing arithmetic: 212 dispatched + 14 unscheduled == 226, and banked +
candidates + named debt == 226. ⚠ **The free cross-check at the end**: banked `.tests.csproj` count ==
roster row count — *"if a migration ends with the two unequal the badge census miscounts."* And two units
meet here: a benchmark-only package emits a **complete test project with zero converted test source**, so
*"declarations recorded"* and *"C# test source produced"* are different quantities and a 0-denominator
ruling travels with the row. **H10's gate**: absolute row count ≥ the prior migration's, rows lost to an
upstream-deleted package admitted as **recorded exceptions**, and **both** absolute and percentage
reported — *"they can move in opposite directions."*

**What H11 receives** `[RB]`: 203 banked rows each with a green badge and a banked test project at the new
base — the two conditions H11's release pre-flight censuses, and why *"the ladder's existing order is not
a convention here but a dependency."* Concretely: `check-roster-format.ps1` and
`release-nuget.ps1 -VerifyOnly` green on a Windows box (both red at the tip today on the ten relocation
rows and the two orphaned manifests — **hop debt of this rung**, cleared by the driver); the re-checked
per-package deadline floors; the re-signed manifests; the roster figures derived, not typed. H12 then
moves every badge, re-pins the vendored `golang.org/x/*` family, and rehearses the five-element release
ritual — whose **write-once proof snapshot** is a copy of the `current/` directory this driver produced.

## G. Open questions for the coordinator

1. **Banking mechanism** (§A). Per-shard lane refs off the version tip, artifacts only, merged as
   incremental trains, figures derived by `check-roster-format.ps1`? *Yes* — §3.5's branch shape and
   hotspot rule applied literally; it keeps a red row naming its own shard.
2. **`docs/validation/index.md`.** Exclude from every lane ref, regenerate once centrally after the last
   leg, row count asserted against the roster? *Yes* — four divergent copies of one shared file is a
   silent-subtraction merge, and the wrapper already measured the mechanism (25 lines, one row).
3. **The driver script.** It dispatches the sweep, which H10 forbids for a re-bank and which cannot reach
   the 23 candidates or the nine successors at all. *Cut a driver seat before the rehearsal*: add
   `-Mode rebank` to the landed script, keeping its plan reader, digest gate,
   `-FleetSize`/`-Worker`/`-OnlySlice` refusals, slice packing and cooldown, and swap the per-row body
   for the recon wrapper's proven pipeline block (floor-derived timeout, stderr handling, `$rc` reset,
   artifact read, classifier, evidence capture); `-Mode sweep` keeps today's behaviour for the
   steady-state pass. **Do not fork a second script** — the two would drift, and the wrapper's per-row
   block is the only one that has run 228 rows.
4. **The worktree.** Linked but **on a branch**, not detached, since artifacts are committed — inverting
   one recon-wrapper guard while keeping the linked-worktree guard, the census, the tracked-count assert
   and floor 11. *Yes.*
5. **W.** *W=4*, G-LAPTOP on its Windows side only: −14.3 min makespan, one fewer slice and one fewer gap
   per worker, i9 at 104 rows not 123. W=3 is the fallback if G-LAPTOP's Windows arm does not preflight
   green — the plan already holds that deal.
6. **Rehearsal shard.** *The i7's W=4 slice 1 (7 rows, 829 i9-s, one slice, zero gaps) plus
   `crypto/internal/fips140test` grafted by name (73 i9-s)* — the relocated principal is seq 1,
   `database/sql` exercises the re-sign, and the graft supplies the only DIVERGED + five-pin-mint act.
7. **Acceptance predicate.** The five in §E plus the deliberate-regression control. *Adopt as written*,
   making item 3's orphan outcome explicit in the post — a named hop debt is a pass, a fabricated proof
   record is not.
8. **TSV schema.** Add `W`, `worker`, `slice`, `seq`, `banked`, `manifest_pins`? *Yes* — the first four
   are already written, and the last two are what the closing arithmetic needs without re-reading trees.
9. **`-SkipBuild` after row 1.** The dispatcher passes `-SkipBuild:($rowsRun -gt 0)`; the runbook wants
   the shared build cost visible as `first_in_list`. *Keep both, but a worker RESUMING mid-shard rebuilds
   on its first row* — a resumed shard skipping the build is §3.4 false green 5 wearing a resume.
10. **The ledger.** §3.4 requires an **idempotent, append-only resume ledger** committed to the shard's
    branch carrying corpus commit, converter commit **and the converter binary's mtime**; the dispatcher
    writes a timings TSV, not a ledger, and is **not** resumable. *Fold the ledger into the driver seat
    (OQ3)* — a multi-hour shard without it costs hours on a reboot, and the corpus-commit field is the
    defense against §3.4's **vacuous shard**.
