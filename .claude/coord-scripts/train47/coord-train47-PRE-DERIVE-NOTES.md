# TRAIN 47 — PRE-DERIVE NOTES

**Status: NEW FILES ONLY. Nothing was run, committed, posted or announced.** No build, no converter,
no `dotnet`, no `git merge`, no `-tests` pipeline. The train-46 assembly worktree was not touched and
the train-46 files were not edited — they are the derivation's INPUT and the self-check's negative
control, and a derive that writes the running train's scripts is the door that opens by itself (bash
reads a script incrementally by byte offset).

The train-46 battery was live in its worktree while this was written; run 7 was at LEG 2b.

---

## 1. What is here

| file | lines | what it is |
|---|---:|---|
| `coord-train47-assemble.sh` | 4,032 | the assembly, derived from `coord-train46-assemble.sh` |
| `coord-train47-rehearse.sh` | 357 | the rehearsal, now with a **per-seat `go vet`** |
| `coord-train47-derive-selfcheck.sh` | 509 | the derive-time self-check, now with **arm 12** |
| `coord-train47-land.sh` | 522 | the landing, driven by the assembly's **OWED VECTOR** |
| `coord-train47-land-dryread.sh` | 256 | the land dry-read, now with **ARM F** |
| `t47-derive.py` | 2,130 | the derivation itself — **130 asserted operations** |
| `coord-train47-PRE-DERIVE-NOTES.md` | this file | what changed and why |

Run the derive with the Windows CPython on this box: `python t47-derive.py`
(`python3` here resolves to the WindowsApps stub and does nothing).

Two runs of the derive produce **byte-identical** outputs (md5 compared).

---

## 2. The three FILL POINTS, as left

Every one of them **refuses** until it is filled — a template that runs with a placeholder in it is a
battery measuring a tree nobody named.

**F1 — the assembly worktree.** `WT="${TRAIN47_WT:-PENDING}"`. Verified: running the assembly as
shipped prints `WORKTREE UNFILLED … ABORT` and exits **2**, having written no log and taken no lock.
It is deliberately NOT inherited from train 46: a train-47 script carrying a neighbour's worktree
would run inside a tree another battery may be holding, and the mid-battery freeze binds the
WORKTREE. `coord-train47-land.sh` has the same fill point as `LAND_WT` (verified: exits 2).

**F2 — the base.**

```
EXPECT_45='44f858717'           # train 45's landing -- the ORDER pin (a landed, known ancestor)
EXPECT_46='PENDING'             # FILL: the SHA train 46 landed; this train's BASE
```

The BASE ASSERTION refuses a PENDING base by name and exits 2; it also still refuses a base that is
not the declared one. The rehearsal and the land script **read `EXPECT_46` out of the assembly script**
rather than carrying a second copy of the SHA.

**F3 — the seat table.** Six placeholder rows:

```
SEAT_TABLE="1|claude/PENDING-seat1|PENDING|converter|tip
2|claude/PENDING-seat2|PENDING|converter-test+docs|tip
3|claude/PENDING-seat3|PENDING|golib|tip
4|claude/PENDING-seat4|PENDING|golib-corpus-handown|tip
5|claude/PENDING-seat5|PENDING|manifest|tip
6|claude/PENDING-seat6|PENDING|doctrine|tip"
```

The CLASS column is a placeholder too — every class below has a `merge_seat` arm, so the coordinator
overwrites ref, SHA and class together. A PENDING row is SKIPPED WITH A STAMP by default and ABORTS
under `TRAIN47_REQUIRE_ALL=1`, which is the shape a LANDING run takes.

**The optional seventh column is new: `allowed=<ERE>`.** See lesson 2.

**The classes `merge_seat` implements (10).** Train 46's five are kept verbatim; five are new:

| class | new? | shape (summary) |
|---|---|---|
| `converter-guard` | kept | converter + slnx + a behavioral guard + the four MSTest classes + a record |
| `golib-gen` | kept | golib + the go2cs-gen Templates tree + converter + a guard |
| `golib-corpus-handown` | kept | golib + corpus hand-owns + a converter registry + GolibTests + a record |
| `golib-converter-docs` | kept | golib + converter + GolibTests + a record (+ a `docs/phase4/probes` tree) |
| `docs` | kept | `docs/**.md` alone |
| `doctrine` | **new** | `CLAUDE.md` alone; G4 runs with teeth |
| `converter-test+docs` | **new** | `src/go2cs/*` (testConversion.go, the `_test.go` guards, `go2cs-src.projitems`) + `src/check-roster-format.ps1` + `docs/phase4/*.md` |
| `manifest` | **new** | one `src/core/<pkg>/go2cs_test_disclosures.json` — the tightest shape in the file |
| `converter` | **new** | converter + slnx + the corpus METADATA a converter change re-mints (`package_info.cs`, `.csproj`) + a record; forbidden list uses `:(exclude)` pathspecs because the seat legitimately lives inside `src/core` |
| `golib` | **new** | `src/core/golib` + `src/tests/GolibTests` + a record |

---

## 3. The twelve mechanised lessons, each with the run-record line that motivated it

Six of train 46's seven launches refused or were killed on an INSTRUMENT defect rather than on a
finding about a tree. Each fix below names the line that paid for it.

### L1 — the seat COUNT is derived, never a literal
> `run1 15:38:47  ^ SEAT TABLE REFUSED: the table holds 6 row(s) where this train's derived seat set is FIVE`

The literal was a fact about the derive, and it refused **after merging all six seats**. Train 46 then
edited the literal to `6`, which is the same defect one train later.

Fixed: the count comes from the table at run time. Because a count compared against itself is a check
that cannot fail, what is **asserted** instead is what a hand-edited table actually breaks and nothing
else would see — seat numbers 1..N contiguous with no duplicate or gap, every ref unique, every filled
SHA unique, every row five fields or more, the pre-loop and post-loop counts equal, and the loop
accounting for every listed row. Stamped as `SEAT TABLE ROWS ARE STRUCTURALLY SOUND ::`.

The "is this the table the coordinator announced" question is the coordinator's at seat fill and the
land script's afterwards; it is not a thing the assembly can know about itself.

### L2 — A7's allowed set is a TABLE FIELD, not code
> `run2 15:42:03  ^ A7 REFUSED: a committed behavioral file other than the four MSTest classes was MODIFIED or DELETED` (four `SwitchPointerSentinelCase` files — a coordinator-RULED re-baseline)

Train 46 hand-wrote the exemption into A7 as a named project and a named seat variable
(`$SEAT5_SHA`), which is a ruling expressed in code that the next derive inherits.

Fixed: a seat that rules a re-baseline declares `allowed=<ERE>` in its OWN ROW. A7 builds the
alternation from the rows that declare one; with none, `A7ALLOW` is a sentinel that cannot match any
path, so the strict form is the default **by construction** rather than by an `if` a later edit could
invert. Each admitted file is compared against the blob of **the seat whose own pattern matches it**
(union blob == that seat's blob, and the line-set too), so the exemption cannot carry another seat's
change; a path matched by two rulings or by none is REFUSED rather than resolved. The
golden-is-the-emission arm is derived from the PATH (`<f>.cs` vs `<f>.cs.target`) instead of a project
name. The allowed set is stamped BEFORE any merge (`A7 ALLOWED SET ::`), and a PENDING row carrying a
ruling is refused at the preflight.

### L3 — G4's expectation is derived from the seat CLASSES
> `run3 15:45:15  G4 REFUSED :: CLAUDE.md is UNTOUCHED in this train's delta -- seat 1 IS the doctrine batch, so an untouched CLAUDE.md means it did not land`

That was train 45's premise carried by the derive into a train with no doctrine seat.

Fixed: `OWED_CLAUDEMD` comes from the OWED VECTOR (below). Owed and untouched refuses; untouched and
not owed is a stated READING; **and touched while not owed also refuses** — a doctrine edit riding in
on another class is an edit nobody ruled on. Both readings are stamped and only the MISMATCH refuses.

### L4 — no `case "${VAR:-0}" in ''` anywhere
> `run3 15:45:18  G10a BOARD structural :: … deletions= (must be 0)` → `^ G10a REFUSED`

`${GADEL:-0}` substitutes *before* the `''` arm can be reached, so the empty case is unreachable and
the value is never normalised. Train 46 fixed its own G10a mid-run and **left the identical shape
standing in `coord-train46-land.sh:306` and `coord-train46-land-dryread.sh:164`**, where nothing on
that train was looking.

Fixed in all five scripts: every such site tests the RAW value. The self-check greps for the shape and
requires zero — and its CONTROL is pointed at the land and dry-read files, because the train-46
assembly no longer carries it (see §5).

### L5 — the rehearsal type-checks after every seat
> `run4 15:45:31  LEG C FULL converter suite exit=1 wall=2s :: pkgsOK=0 FAILlines=2 :: FAIL go2cs [build failed]`
> `UNION FIX (coordinator, 15:58) … git merged the two edits to one file CLEAN by content; the test package then fails to compile at the union and ONLY at the union`

That cost a battery leg forty minutes in.

Fixed: `coord-train47-rehearse.sh` runs `go vet ./...` in `src/go2cs` after **every** seat merge, under
the CONVERTER BUILD PIN (the go1.24.13 root, `GOTOOLCHAIN=local`, derived from `$HOME`), and reports
the **first failing seat by name**. Consequences stated rather than glossed: the rehearsal is no
longer "git only" (it reads the toolchain and writes the build cache — it still writes nothing into
the repository, runs no converter and no dotnet, and still creates its own worktree); a vet leg whose
pin cannot be established is reported **UNMEASURED and fails**, never a pass; `NOVET=1` turns it off
and says so in the summary and in the exit arithmetic.

The UNION FIX mechanism is kept and made **OPTIONAL**: absent patch = no fix, STATED, with the
explicit note that a run with no patch asserts nothing by itself about whether the union compiles —
LEG C is that assertion. A patch that IS present must ship `<patch>.expected` (one
`path<TAB>add<TAB>del` line, exactly as `git diff --numstat` prints), compared line for line, with
`go vet` under the pin as its gate. A patch with no declared footprint is a patch nobody sized.

### L6 — the census TEMPLATE class is carried byte-for-byte
> `run5 16:04:23  G6 coarse census :: detectorHits=21 … residual=11 (must be 0)` — all eleven in the security guard's own `Sprintf` fixtures
> `run6 16:16:48  CONTROL C1/G6T template class :: … templateExcluded on the fixture=1 (want 1)`

`CENSUS_DETECT`/`CENSUS_EXCLUDE`/`CENSUS_PLACEHOLDER`/`CENSUS_HARD`/`CENSUS_TEMPLATE`, the four
`CENSUS_KIND_*` patterns and the C1/G6T six-arm control are **carried verbatim**. They are proven, and
a derive that retyped them would be re-deriving a measured answer through a shell that collapses
doubled backslashes. The self-check asserts `CENSUS_TEMPLATE=` and `CONTROL C1/G6T template class` are
present. Only the G6 STAMP's prose changed, because a template has no derive-time reading to carry.

### L7 — every G11 justification is derived from the seat CLASSES
> `run6 16:22:49  G11(a) JUSTIFICATION FALSE :: golib=6 syscall/windows=0 GolibTests=3 -- at least one is ZERO`

`src/core/syscall/windows` was train 45's premise (it had a syscall seat) expressed as a DIRECTORY
LITERAL. Train 46 corrected it by hand mid-run.

Fixed by one derivation with two readers. After the seat loop the assembly computes an **OWED VECTOR**
from the CLASSES of the rows that MERGED and stamps it as its own line:

```
OWED VECTOR :: golib=… gen=… converter=… behavioral=… corpus=… golibtests=… docs=… claudemd=… manifest=…
              (DERIVED from the CLASSES of the N merged row(s): [ … ])
```

G11(a) runs one `(owed, measured)` arm per directory: owed-and-absent refuses by name, not-owed is a
stated READING that can never refuse, and `syscall/windows` is printed as a READING with a note saying
why. G4, the DOCS-DELTA arm and LEG 3's arms-count arm read the same vector. **The land script PARSES
that stamped line** instead of re-deriving it — two instruments deriving one fact independently is how
they drift — and a record with no such line ABORTS rather than defaulting to "nothing is owed", since
a default there would silently turn every directory requirement into a pass.

This also fixes an inversion worth naming: train 45's land required `src/gen` to be **zero** and train
46's required it to be **non-zero**. Two opposite assertions, each correct about its own train.

### L8 — launch, kill and census mechanics
Stated in the assembly header as LAUNCH SHAPE:

* the launch wrapper must end in **`exit $rc`** — a wrapper whose last statement is a `tail` (or any
  pipe) reports the LAST command's status, so a script that exited 1 is announced as 0. The written
  form captures `rc=$?` as the first statement after the run;
* a running assembly is killed **by the pid the LOCK FILE names**, mapped MSYS→Windows via `ps -l`
  (its WINPID column) before `taskkill /T /F` — never by the harness task's shell, which kills the
  task's shell and leaves the chain an orphan writing into the same log;
* the census after a kill **includes `bash` by command line** (the chain is a bash script, so an
  executable-only census cannot see it) and **excludes the querying shells by AGE** (>30 s: a real long
  run is minutes old, the shell doing the check is seconds old), and **reads the lock file** before
  believing "no survivors".

### L9 — `set -u` only, and ZERO `| grep -q`
Every one of the five headers states it: under `set -o pipefail` a `grep -q` that exits on its first
match SIGPIPEs its producer, the pipeline's status becomes the producer's death, and a TRUE match
reads as a failure (R's measurement). All nine `| grep -q` sites in the assembly and the one in the
land script became `grep -c` (or `grep -ac`) plus an integer test. The rule has no judgement in it —
ZERO, asserted — rather than asking per site whether its producer streams, because a site left alone
is a landmine for the derive that does set pipefail.

### L10 — exact STAMPS, never words
> a waiter keyed on `ABORT` fired on `TRAIN46_REQUIRE_ALL=1 :: a PENDING row will ABORT BY NAME`, which is prose about a refusal

The header lists the five stamps a chain or watcher may key on: `=== <label> ASSEMBLE START`,
`=== LIGHT GATES DONE`, `=== A-ASSERTIONS FAILED`, `=== CHAIN STOPPED`, `=== <label> ASSEMBLE DONE`,
plus each leg's `exit=<code>` and its one-line verdict carrying its row count.

### L11 — every `FAILED=1` names its gate
Train 46 carried **fifteen** setters with no stamp on the same or the previous line. A reader scanning
for the flag could not tell which gate set it, and one of them cost a run its attribution.

Fixed: `fail_gate(){ FAILED=1; stamp "  ^ FAILED=1 set by gate [$1]"; }`, and each of the fifteen is
now `fail_gate <GATE>` — `A5-nested`, `G6-residual`, `LEG-C`, `LEG-2`, `LEG-2b`,
`LEG-D-seeding-control`, `LEG-D-conversion`, `LEG-D-prediction`, `LEG-R-convert`, `LEG-R-build`,
`LEG-3-build`, `LEG-3-skip-delta`, `LEG-4-after-guard`, `LEG-5-failing-set`,
`LEG-5-reconciliation`, `LEG-K-row-exit`. Each is anchored in the derive on a unique nearby line with
a bounded window, because seven of them are the byte-identical line `  FAILED=1` and an ordinal would
retarget silently the moment anything above them moved.

### L12 — the land script carries ZERO acceptance paths
Unchanged from train 46 in substance and asserted by the dry-read's ARM D (the five train-45 variable
and block names must be absent as CODE while the record of WHY stays as prose) and ARM E (the one
surviving exclusion, the wrapper's trailing `assembly exit=N` line, is a sentinel declared once and
assigned in exactly one place, behind three measured conditions).

---

## 4. Two things a template could not carry, and what replaced them

**A1..A4 (the per-seat content assertions).** Train 46's four arms read each NAMED seat's own content
out of the assembled FILES rather than out of the graph — the strongest assertions in that file, and
the most train-specific. A template cannot write them, so what it carries is the **SLOT and the GATE**:
a fenced `# === SEAT-CONTENT ASSERTIONS BEGIN/END ===` block the coordinator fills with one arm per
named seat (each gated on its own `$SEATn_SHA != PENDING`, each ending in `SEATASSERT_N=$(( … + 1 ))`),
a worked example taken from train 46's A3 (a registry entry and the body it displaces, read as TWO
halves), and a refusal when the arm count is below the merged-seat count. A battery over a tree whose
seats nobody asserted measures a tree nobody ruled on.

**The land script's seat-specific `req` anchors.** `A5 TOP-LEVEL … missingFiles` at a count of two,
`A5 NESTED … slnxRegistrations=1` at two, `A3 seat 3 registry AND displacement`, `A2 seat 2
ISlice.IsNil` — all four named train 46's seats. An anchor whose words no longer appear anywhere in
the assembly can never be satisfied, so the landing refuses a HEALTHY battery and the operator reaches
for the switch that makes it stop refusing. They are **gone, not re-pointed**, replaced by anchors a
template can guarantee: `SEAT TABLE ROWS ARE STRUCTURALLY SOUND ::`, `OWED VECTOR ::`,
`SEAT-CONTENT ASSERTIONS :: arms=`, `A7 ALLOWED SET ::`, and an A5 anchor that admits either the
per-project reading or the "this train adds none" statement. The dry-read's ARM A is what found the
one anchor I initially left stale (`BASE ASSERTION :: derived-time base`) — the instrument working.

---

## 5. Controls, run and reported

**Self-check on the derived files** — `bash coord-train47-derive-selfcheck.sh`
→ **exit 0**, 50 PASS/ok lines, 0 FAIL.

**Self-check on the train-46 original** (negative control) —
`bash coord-train47-derive-selfcheck.sh ./coord-train46-assemble.sh` → **exit 1**, 13 FAIL.

**Arm 12, per lesson.** Every predicate is ONE definition run over whichever file it is handed, so the
arm and its control are literally the same code; all read CODE ONLY, because a file that documents the
defect it forbids necessarily spells it.

| lesson | on train-47 | on train-46 | verdict |
|---|---:|---:|---|
| L1 no literal seat count | 0 | 1 | PASS (fires on the original) |
| L3 G4 derived from the classes | 0 | 1 | PASS |
| L4 no case-default (land) | 0 | 1 | PASS |
| L4 no case-default (dry-read) | 0 | 1 | PASS |
| L9 zero `\| grep -q` | 0 | 9 | PASS |
| L11 every `FAILED=1` names its gate | 0 | 15 | PASS |

**⚠ L4's control is pointed at the LAND and DRY-READ files, not the assembly, and that is a
measurement rather than a choice.** Train 46's G10a site was corrected in flight after run 3 refused
on it, so the train-46 assembly no longer carries the shape and a control against it reads a dead
ZERO (`reading L4 (assembly) :: train47=0 train46=0`, reported as such). The identical shape DID
survive in the land script and its dry-read. The assembly's own zero is still asserted, by the same
predicate, beside the two arms that prove it can fire.

**A control that cannot RUN is UNMEASURED and FAILS**: if the train-46 originals are not beside the
self-check (`T46DIR=<dir>` overrides), the arm says so and does not pass. The control filenames are
COMPOSED from `T46NUM=46` rather than spelled, because the derive runs a global
`coord-train46-* → coord-train47-*` rename over that file — and on this arm's first run it did exactly
that, pointing L3 and L11 at their own output and reporting the control as dead.

**`bash -n`** — all five parse. The derive now runs `bash -n` on every file it writes and REFUSES,
because a block whose text ends with a shell `"` directly against the Python `"""` terminator loses
that quote to the terminator; that happened twice while this was written, in two different blocks, and
it is invisible by reading either language alone.

**Land dry-read** — `bash coord-train47-land-dryread.sh` → **exit 0**, 0 MISS, 64 req anchors, 0 dead
refusal patterns, 0 hits on healthy prose.

**Fill-point refusals** — the assembly as shipped exits **2** with `WORKTREE UNFILLED`, having written
no log and taken no lock; the land script exits **2** with `LAND WORKTREE UNFILLED`.

**Reproducibility** — two consecutive derive runs produce byte-identical outputs.

**Security census** (Python; ripgrep is absent and `grep -i -F` aborts on this box) over the five
scripts and the derive: **0 residual hits**, 86 exclusions each enumerated with a reason —
the census DETECTOR's own pattern definitions (15, carried byte-for-byte), Python regex sources
carrying escaped backslashes (28), planted control fixtures (19), redacted placeholder segments (7),
escaped-regex literals in instrument patterns (5), and shell path-form conversions where a backslash
is a SEPARATOR and not a UNC prefix (12). Every one of the five detectors was shown to FIRE on its own
planted line before the zero was believed.

Two real breaches were found and fixed while doing it, both inherited: `pin_go` and `pin_go_pairing`
each spelled the toolchain root a second time as a Windows-form literal carrying a profile path and an
account name. Both now DERIVE the Windows form from the POSIX root with `cygpath -w` (with a hand-rule
fallback). That is one fix for two faults: a profile path must not be in a file any part of which may
be quoted onto a pushed surface, and a second spelling of one root is the thing that drifts when a
toolchain moves.

---

## 6. What I deliberately did NOT change

* **The two-pin pairing and its after-guard.** GOROOT re-exported to the 1.23.12 root with its bin
  FIRST on PATH and GOTOOLCHAIN UNSET for the corpus/oracle legs; the explicit 1.24.13 root with
  `GOTOOLCHAIN=local` for the converter's build and suites; every pin RUNS `go version` AND
  `go env GOROOT` and ABORTS on either mismatch. Only the ROOTS moved, from literals to `$HOME`-derived
  values with an existence assertion (`GO2CS_SDK_ROOT` / `GO2CS_DOTNET_ROOT` override by name).
* **LEG D's corrected seeding control** — three arms, both binaries from `git archive`, six seeds from
  one snapshot, write-evidence per arm per target, the prediction stamped BEFORE the diff and derived
  with the THREE-DOT form per merged seat, the per-target comparison BY MECHANISM.
* **LEG 5's own converter rebuild** immediately before the suite, and the transpile-predicate stamps
  before and after (zero behavioral `.cs` newer than the binary) — route #2's door.
* **LEG K's derived reflect canary set** (controlled both directions before its membership is used),
  plus `sync` and plus `crypto/internal/nistec` as the cost canary whose WALL is recorded and not
  judged.
* **The pre-resolved merge mechanism**, the six preflight controls C1..C6, the run lock, the
  disk-floor preflight, ROUTE B, `< /dev/null` on every loop-resident child, processed == listed on
  every loop, the freeze announcement stamps, G10a/b/c/d, G12, LEG 0, LEG 1, LEG 2, LEG 2b, LEG C,
  LEG Cg, LEG R, LEG 3's compile-set evaluator and declared-count reconciliation, and the eight
  alias-carrying `GOLDEN_PROJECTS`.
* **The `docs` owe is SOFT and says so.** A class may legitimately carry no record, so the DOCS-DELTA
  arm reads the vector as "a record is EXPECTED" and checks a zero against the skip count rather than
  refusing outright.
* **The prose "train 46".** It names the base this train sits on and the run records each lesson came
  from. A derive that scrubbed it would delete the record of why every arm is shaped as it is.

---

## 7. What the coordinator does at seat fill

1. Fill **F1** (`WT=` or `TRAIN47_WT`), **F2** (`EXPECT_46=` — the SHA train 46 landed), **F3** (the
   six table rows: ref, SHA, class, and `allowed=` where a re-baseline is RULED, with the ruling
   written at the row).
2. Write one arm per named seat into the fenced `SEAT-CONTENT ASSERTIONS` block. The assembly refuses
   when the arm count is below the merged-seat count.
3. Re-run `bash coord-train47-derive-selfcheck.sh` — with a filled table, arms 3 and 8 stop reporting
   UNMEASURABLE and check every seat's real diff against its class shape.
4. Run `bash coord-train47-rehearse.sh` and read its per-seat `go vet` line. If it names a failing
   seat, that is a union-only compile failure: write the patch plus its `.expected` footprint as
   `coord-train47-unionfix.patch` / `.expected` beside the assembly.
5. Launch a **per-run copy** (`coord-train47-assemble-run1.sh`) from a wrapper that ends in
   `exit $rc`, with `TRAIN47_REQUIRE_ALL=1` for a landing run.

## 8. Known limits of this template, stated

* **The seat CLASSES in the placeholder rows are guesses** at the likely train-47 set. Every class has
  an arm, so any reassignment is a table edit and not a code change — but if a filled seat needs a
  shape no class admits, the script is **RE-DERIVED**, never loosened: widening a class to admit the
  seat that violates it is how a gate stops being a gate.
* **Arm 12's controls depend on the train-46 originals staying in this directory.** They are inputs to
  a measurement now, not spent artifacts. If they are moved, `T46DIR=` must point at them or the arm
  reports UNMEASURED and fails.
* **The rehearsal's per-seat `go vet` has not been exercised against a real conflict or a real
  union-only failure** — it has only been parsed. Its first real run is its first measurement, and it
  should be read as such.
* **`converter`'s and `golib`'s `:(exclude)` forbidden pathspecs have not been exercised against a
  real seat diff** (there are no filled seats). The self-check's arm 8 will measure them the moment
  the table is filled; the three decoys it runs today cover only the three classes train 46 had.
* **No gate here was run against a tree.** Everything reported above is a parse, a static read, or a
  refusal path — by design: the brief forbade running anything, and a battery worktree was live.
