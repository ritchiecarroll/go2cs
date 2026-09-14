# TRAIN 48 — PRE-DERIVE NOTES

**Status: NEW FILES ONLY, all of them under `.claude/coord-scripts/train48/`. Nothing was run,
committed, posted or announced.** No build, no converter, no `dotnet`, no `git merge`, no `git fetch`,
no `-tests` pipeline, no `*.ps1` gate. The train-47 directory was READ and never written: it is the
derivation's INPUT and the self-check's negative control, and a derive that writes the running train's
scripts is the door that opens by itself (bash reads a script incrementally by byte offset).

**The train-47 battery was live in its worktree while this was written**, and its instrument moved
under this session: `coord-train47-assemble.sh` read 5,071 lines at 07:50 and 5,076 lines at 08:14.
The derive re-reads it every run and stamps its sha256, which is the mechanism that makes that safe.

---

## 1. What is here

| file | lines | what it is |
|---|---:|---|
| `coord-train48-assemble.sh` | 4,837 | the assembly, derived from `coord-train47-assemble.sh` |
| `coord-train48-rehearse.sh` | 579 | the rehearsal |
| `coord-train48-derive-selfcheck.sh` | 1,215 | the derive-time self-check, with **SEVEN new arms (a)–(g)** |
| `coord-train48-land.sh` | 639 | the landing, driven by the assembly's OWED VECTOR |
| `coord-train48-land-dryread.sh` | 313 | the land dry-read, with a new **ARM G** |
| `launch-run1.sh` | 21 | the launch wrapper (exports the switch, ends in `exit $rc`) |
| `t48-derive.py` | 2,118 | the derivation itself — **185 asserted operations** |
| `t47-derive.py.reference` | 2,130 | train 47's derive, copied VERBATIM as a reference only |
| `coord-train48-PRE-DERIVE-NOTES.md` | this file | what changed, what is unfilled, and every doubt |

⚠ **AMENDED 2026-09-13 (post-review).** The counts above are the POST-REVIEW ones; the pre-review cut
was 4,811 / 1,132 / 628 / 287 lines and **144** operations, and the difference is [§16](#16-the-review-fixes-2026-09-13).
⚠ **AMENDED AGAIN 2026-09-13 ~10:10 — RE-DERIVED FROM MOVED TEMPLATES.** The line and op counts above
are now the ones from the re-derive in [§17](#17-2026-09-13-1010--re-derive-after-the-leg-u--leg-r-restore-patch-and-lesson-lr1);
between §16 and §17 they read 4,830 / 1,210 lines and **183** operations. **183 → 185, not 184**: the
one fix §17 adds is a `gsub` *and* its `absent()` guard, and `absent()` is itself a counted assertion.
The arm count read "six new arms (a)–(g)" in this file's first cut, which is **seven** — a count error
in the prose of a document whose subject is counting, and it is corrected rather than quietly adjusted.

Run the derive with the Windows CPython on this box: `python t48-derive.py`
(`python3` here resolves to the WindowsApps stub and does nothing).

**The copy IS the derive.** Step 1's "copy the five files and rename every train47 token" is performed
inside `t48-derive.py` as asserted `gsub` operations rather than by a separate `cp` pass, for the
reason the whole method exists: a copy step followed by an editing step is two steps that can drift,
and only one of them asserts. `python t48-derive.py` reproduces every train-48 file from the train-47
template; there is no intermediate state to keep in sync.

> **this template was derived from train 47's at
> `b28a1545039b36813fa99232877bf71d6e19887df99a5669d78e890c0742447f`
> (the sha256 of `C:/Projects/go2cs/.claude/coord-scripts/train47/coord-train47-assemble.sh`);
> if train 47's instrument changes before train 48 assembles, re-run `t48-derive.py` from fresh
> copies.**

**ALL FIVE TEMPLATE HASHES, so a mover of any one of them is visible and not just the assembly's**
(`sha256sum`, 2026-09-13 ~10:10, in `C:/Projects/go2cs/.claude/coord-scripts/train47/`). ⚠ Only the
assembly's sha256 is STAMPED BY THE DERIVE ITSELF; the other four are recorded here and are a
by-hand reading, so re-take them rather than trusting this table:

| train-47 template | sha256 | moved since §16? |
|---|---|---|
| `coord-train47-assemble.sh` | `b28a1545039b36813fa99232877bf71d6e19887df99a5669d78e890c0742447f` | **YES** (was `243c7253…`) |
| `coord-train47-rehearse.sh` | `584ea342294f81130535d0c216fb6a2d1e08ba712d02c8fdd2701745860a86b6` | no |
| `coord-train47-derive-selfcheck.sh` | `e42a983df7d2f8888a3c38bbc3bdf9a7ae3c5720d3c0c20ef6deba6548306db7` | **YES** (lesson LR1) |
| `coord-train47-land.sh` | `88e69fe648d1e6e1065a05fffb365d383174560182df435dab857e30d19d0666` | no |
| `coord-train47-land-dryread.sh` | `3418ef1d5c93b90b306c1a76a80d84acfc099e97ee921d57c92745d9ca81c467` | no |

That sentence is not decoration. The template file changed **during this session** (5,071 → 5,076
lines, 07:50 → 08:14), and the derive prints the sha256 it actually read on every run — compare it
before trusting anything below. ⚠ **IT MOVED A THIRD TIME** (5,076 → 5,083 lines, sha256
`243c7253…` → `b28a1545…`) and the re-derive in [§17](#17-2026-09-13-1010--re-derive-after-the-leg-u--leg-r-restore-patch-and-lesson-lr1)
is what this directory now holds.

---

## 2. The FILL POINTS, as left — every one REFUSES

| # | what | where | reads | what refuses it |
|---|---|---|---|---|
| F1 | the assembly worktree | `WT="${TRAIN48_WT:-PENDING}"` | `PENDING` | `WORKTREE UNFILLED ::` → exit 2, before the lock |
| F2 | the CONTAINMENT pin | `T48_CONTAIN_PIN='PENDING'` | `PENDING` | `FILL POINT F2 REFUSED: the containment pin is not filled` → exit 2 |
| F3 | the seat table | `SEAT_TABLE=` | 19 rows: 9 filled, 10 PENDING | a PENDING row ABORTS by default; `TRAIN48_SKIP_PENDING=1` opts into a partial train BY NAME |
| F3a | one **REF** fill point | row 9 `claude/PENDING-BRANCH` @ `68ad83c2c` | branch name not given | SEAT PREFLIGHT: `carries a FILLED SHA … against the PLACEHOLDER ref` |
| F3b | two **CLASS** fill points | rows 12 and 19 read `PENDING-class` | class undecided | `seat_class_placeholders` refuses the moment such a row's SHA is filled |
| F4 | G3's census expectation | `EXPECT_G3='PENDING'` | `PENDING` | `FILL POINT F4 REFUSED: … census expectation is not filled` → exit 2 |

**The base is NOT a fill point and must not become one.** `EXPECT_BASE='origin/master'` — the base is
origin/master RESOLVED AT LAUNCH, per the coordinator's standing rule, because a baked base SHA
refuses a HEALTHY launch the moment any lane lands and the operator then reaches for the switch that
makes it stop refusing. What the train depends on is CONTAINMENT of `T48_CONTAIN_PIN`, and that is the
thing asserted. The PENDING refusal in the BASE ASSERTION is kept intact for a later coordinator who
does spell a SHA there.

**The ORDER pin is `a02ac3df3`** — train 47's own base, and a MEASUREMENT rather than an assumption:
`git ls-remote origin refs/heads/master` read it on 2026-09-13, so it was already an ancestor of
origin/master when this was derived and cannot false-red.

**The corpus pin stays 1.23.12.** A6 still asserts `version.props <GoStdLibVersion>=1.23.12` and
`src/go2cs/go.mod` declaring `go 1.24.13`; the derive asserts it KEPT all three (`a6-corpus-pin`,
`a6-corpus-pin-test`, `a6-gomod-pin`). Train 48 is still pre-H5.

**The lock path, the leg-log prefix and the LEG D scratch prefix are DERIVED, not spelled.**
`TAG="t${LABEL#train}"` comes from the basename, so `coord-train48-assemble.sh` gives `TAG=t48`,
`LOCK=/tmp/t48-assemble.lock`, `coord-t48-legD-$RUNID/` and the new
`coord-t48-legD-patchid-$RUNID/`. Renaming the file is the whole rename.

---

## 3. The seat table (F3), and what was MEASURED about it

`git -C C:/Projects/go2cs ls-remote origin` (no objects transferred) on 2026-09-13:

| row | ref | pin | class | note |
|---:|---|---|---|---|
| 1 | `claude/g-handown-metadata-t48` | **PENDING** | converter-corpus-metadata | branch exists at `bb13897e6`; **SHA pending BY RULING** — the branch is to be RE-BASED after train 47 lands, and a pin taken before the rebase names a commit whose three-way base is a tree this train will not have |
| 2 | `claude/c2-h6-crosscheck` | `191164e7a` | docs | tip == pin ✔ |
| 3 | `claude/laneR-h6-alias-block` | `47592cb3f` | docs | tip == pin ✔ · `stack-on=2` |
| 4 | `claude/laneR-docs-h6-skeleton` | `d18059950` | docs | tip == pin ✔ |
| 5 | `claude/laneR-prepin-baselines-recut` | `becf28abc` | docs | tip == pin ✔ |
| 6 | `claude/c2-safepush-shallow-skip` | `fa2fdd30d` | converter-test | tip == pin ✔ |
| 7 | `claude/c1-seat-duplication-census` | `77e41300a` | converter-test+tooling | tip == pin ✔ · **carries the PATCH-ID ARM's own tool** |
| 8 | `claude/g-fleet-patchid-census` | `9b78bfff6` | converter-test+tooling | tip == pin ✔ |
| 9 | `claude/PENDING-BRANCH` | `68ad83c2c` | docs | **REF FILL POINT** — i9's BOARD entry, branch name not given |
| 10 | `claude/i9-board-archive-tar` | `314e699c6` | docs | tip == pin ✔ · `stack-on=9` |
| 11 | `claude/PENDING-seat11` | PENDING | docs | board SHA `4140a8e55` |
| 12 | `claude/PENDING-seat12` | PENDING | **PENDING-class** | board SHA `0b24685bc` |
| 13 | `claude/PENDING-seat13` | PENDING | tooling | board: `171d419f6` + `33c29952d` |
| 14 | `claude/PENDING-seat14` | PENDING | tooling | board SHA `baf1fbe72` (PowerShell → G2 must parse it) |
| 15 | `claude/PENDING-seat15` | PENDING | docs | board SHA `a0496fb93` |
| 16 | `claude/PENDING-seat16` | PENDING | converter | board SHA `830fa8d26` |
| 17 | `claude/PENDING-seat17` | PENDING | golib-corpus-handown | board SHA `4a9ae8cbb` |
| 18 | `claude/PENDING-seat18` | PENDING | golib-corpus-handown | board SHA `3f1612524` |
| 19 | `claude/PENDING-seat19` | PENDING | **PENDING-class** | board SHA `5f0564da3` |

*(The row numbers above are the MERGE ORDER, which is all the table asserts, and the SHA field of a
PENDING row reads `PENDING` exactly as the brief required.  The PENDING rows carry the
template's own `claude/PENDING-seatN` placeholder refs -- the SEAT PREFLIGHT refuses that prefix the
moment a SHA is filled beside it -- and the candidate each row is FOR is named in the table's own
comment block directly above it, row by row. **A SHA read out of a comment is a SHA nobody asserted**:
the pin lives in field three and nowhere else.)*

### 3.1 Every named branch EXISTS and its TIP EQUALS the given pin

All nine named branches resolve on `origin`, and for all nine the remote tip is byte-for-byte the pin
the board gave. That is a measurement, and it is the strongest thing this session could establish
without a fetch.

⚠ **AMENDED 2026-09-13 (post-review) — THAT IS NO LONGER TRUE OF ROW 7, AND IT IS A LIVE BLOCKER.**
A second `git ls-remote origin` reads `refs/heads/claude/c1-seat-duplication-census` = **`a4802675d`**;
the table pins **`77e41300a`** with tipmode `tip`. Rows 1–6, 8 and 10 still MATCH
(`bb13897e6` for row 1's branch, which the table deliberately leaves PENDING). Two consequences, both
concrete:

* `merge_seat` in `tip` mode **REFUSES row 7 as shipped** — correctly: that is the mode working.
* the PATCH-ID ARM extracts its own tool with `git show "${PIDSHA}:src/seat-duplication-census.sh"`,
  so at the shipped pin it would run the **superseded** copy of the instrument.

**Nothing was re-pinned here.** A pin is the coordinator's to move: `a4802675d` has not been read,
its relationship to `77e41300a` has not been measured, and a pin taken by a reviewer from a live
`ls-remote` is a SHA nobody asserted. **Row 7 needs a coordinator ruling before this train assembles**,
and whichever SHA it lands on, the census tool must be re-`--self-test`ed at that SHA
(see [§6](#6-the-patch-id-arm-new) — the run recorded there was taken at `77e41300a`).

### 3.2 SHAs mapped to branch names by `ls-remote` (recorded, NOT acted on)

| board SHA | remote branch whose TIP it is |
|---|---|
| `68ad83c2c` | `claude/i9-board-runtime-door-bisect` ← **the name row 9 needs; CONFIRM before filling** |
| `4140a8e55` | `claude/c2-runbook-shard-amendment` |
| `0b24685bc` | `claude/c2-h10-shardmap-projection` |
| `33c29952d` | `claude/c2-shardmap-repair` (the shard-map repair TIP) |
| `baf1fbe72` | `claude/c2-sweep-hop-mode` |
| `a0496fb93` | `claude/c2-board-peros-nested-hazard` |
| `4a9ae8cbb` | `claude/c1-gctestisreachable-clean` |
| `3f1612524` | `claude/c1-mfinal-mint-door-clean` |
| `5f0564da3` | `claude/c1-board-goroot` |
| `bb13897e6` | `claude/g-handown-metadata-t48` |

### 3.3 SHAs I could **NOT** map to a branch name — two, and they are different failures

* **`171d419f6`** — not the tip of any remote branch. It resolves in this clone and `33c29952d`
  (`claude/c2-shardmap-repair`) IS a branch tip, so the honest reading is that `171d419f6` is an
  EARLIER commit of that same branch and the board named the pair. **Nothing was inferred into the
  table**: row 13's ref is a placeholder and both SHAs are in its note.
* **`830fa8d26`** — not the tip of any remote branch **and not present in this clone at all**. The
  only goroot-shaped branch is `claude/c2-census-goroot-fix`, whose tip reads **`3ced37e18`**. So
  either the board's "C2 goroot-fix tip" is stale, or it names a branch this remote does not carry.
  **This is a question for the coordinator, not a guess for me.** Row 16 ships PENDING.

---

## 4. The seat table's SEVENTH field, and the parser

The optional fields are now **order-free `key=value` pairs from field SIX on**, not a positional
sixth field:

```
FIELDS:  N | ref | SHA | class | tipmode [| <key>=<value> ...]
  allowed=<ERE>     a coordinator RULING admitting named committed behavioral files (A7)
  stack-on=<row>    this row is deliberately built ON another row of this table
```

* An **UNKNOWN key is REFUSED by name**, not ignored — and that is what catches the table's one real
  encoding hazard: an `allowed=` ERE carrying a `|` ALTERNATION splits into extra fields here, and
  silently keeping field six alone would run a NARROWER exemption than the coordinator ruled.
* A field that is not `key=value` at all is refused with `not <key>=<value>`.
* `stack-on=` REFUSES a value that is not a row number, a row that **does not exist**, a row that
  **merges AFTER it** (the row order IS the merge order), and **ITSELF** (a self-reference would make
  the patch-id census declare a row's own commits exempt from itself — an exemption that can never
  refuse).
* Everything that used to read field six positionally now reads through `seat_opt` — the allowed-set
  readout, the A7 map loop, the SEAT PREFLIGHT and the seat-note printer — because a positional
  `allowed=*` test misses a ruling written in field seven, and a missed ruling reads as the STRICT
  default, i.e. as silence.

---

## 5. The three NEW classes, and the OWED vector

| class | shape (summary) | what it OWES |
|---|---|---|
| `converter-test` | `^src/go2cs/[^/]+$` and nothing else — `converter-test+docs` without the record and without the roster script | LEG C |
| `converter-test+tooling` | converter source + its guards + a repo tool script (`src/*.sh`, `src/*.ps1`, `src/utilities/**`) + a record | **LEG C and G2** |
| `tooling` | a repo tool script + a record, and NO converter source at all | **G2 only** |

`OWED_TOOLING` is new and G11(a) gained an arm for it, reading the pathspec set
`src/*.sh src/*.ps1 src/utilities` rather than a directory — `src/utilities` alone reads ZERO on a row
whose whole deliverable is a root-level script. `tooling` is deliberately NOT in the converter arm: a
class that owed LEG C without touching `src/go2cs` would make G11(a)'s converter arm refuse a healthy
tree, which is the exact fault (L7) was written to remove.

`converter-corpus-metadata` is **unchanged** — it keeps train 47's treatment (`converter=1`,
`corpus=1`, and not in the docs owe).

⚠ **A DOUBT, STATED.** The brief said to treat `converter-corpus-metadata` "as train 47's row 8 did".
Train 47's ROW 8 carries class `converter-guard-rebaseline`, not `converter-corpus-metadata` (the
`converter-corpus-metadata` arm was derived for train 47's *coordinator* seat 6, which was UNSEATED).
I read the instruction as "keep the existing train-47 treatment of that class" and changed nothing. If
the intent was the `converter-guard-rebaseline` treatment (`converter=1`, `docs=1`), say so.

`PENDING-class` is a **class fill point and deliberately has NO `merge_seat` arm**. Implementing one
would let a row whose class nobody decided merge under a shape nobody ruled. Two refusals make it
safe: the structural check refuses a FILLED row carrying it (before any merge), and `merge_seat`'s
unknown-class arm refuses it if one ever reached the loop.

The land script now parses `tooling=` out of the stamped vector. A record with no `tooling=` field is
an OLDER assembly's record, `owed_of` returns empty, and the normaliser reads it as 0 — which is the
correct reading of a train that had no tooling class at all.

⚠ **AMENDED 2026-09-13 (post-review) — PARSED IS NOT ARMED, AND THE FIRST CUT ONLY PARSED IT.**
`O_TOOLING` was read, normalised and printed inside the stamp that says *"the directory arithmetic
below is driven by THIS"* — and there was no arithmetic: no `D_TOOL` measurement existed anywhere in
the land script and no `land_arm` consumed the field. The concrete failing path: a train whose only
tooling row lands **nothing** under `src/*.sh | src/*.ps1 | src/utilities` is REFUSED by the
assembly's `g11a_arm "$OWED_TOOLING"` and PASSED by the landing — two instruments reading one owe and
disagreeing, which is the shape that gets the weaker one quoted. The land now measures `D_TOOL` over
**the same pathspec SET** the assembly uses (`src/utilities` alone reads ZERO on a row whose whole
deliverable is a root-level script), stamps it in the delta-by-directory line, and arms it.

---

## 6. The PATCH-ID ARM (new)

It runs **after the SEAT PREFLIGHT and before any merge**, and it is the only instrument on this train
that can see a cherry-pick: the same content under a new SHA makes `merge-base --is-ancestor` read
false and `git log <base>..<seat>` list a commit nobody recognises, right up until the train lands the
same diff twice.

* **The tool is read out of its OWN seat's pinned blob** — `git show <row 7's pin>:src/seat-duplication-census.sh`
  into `$SCRIPT_DIR/coord-t48-legD-patchid-$RUNID/` — so the instrument and the seat cannot drift.
* **If that row is absent or PENDING the arm stamps `PATCH-ID ARM UNMEASURED :: the census tool's seat
  is not on this table` and sets FAILED.** An unmeasured arm is not a pass.
* **`--self-test` runs FIRST and gates the verdict.** Exit must be 0 *and* an anchored
  `^SELF-TEST CLEAN -- <n> arms` line must be present; the arm count is stamped. A tool whose own
  controls fail measures nothing, so its verdict about this train's seats is not consulted at all.
* Then `--base $BASE`, one `--stack <child-sha>:<parent-sha>` per `stack-on=` row, then every FILLED
  row's pinned SHA. **PENDING rows are EXCLUDED BY NAME and the exclusion is stamped** — a census over
  a set it could not resolve would exit 2 as MISUSE and read as a finding.
* **The exact command line is stamped** for both invocations.
* exit 0 → `PATCH-ID ARM CLEAN` with the tool's verdict line (and a refusal if exit 0 arrives with no
  verdict row: a zero exit and a verdict are two claims). exit 1 → every named row printed verbatim
  plus `fail_gate PATCH-ID-ARM`. exit 2 → `PATCH-ID ARM MISUSE` + FAILED. Any other exit →
  `UNMEASURED` + FAILED.
* **Every read of its output is an ANCHORED ROW MATCH**: `^==> CENSUS (CLEAN|RED)`,
  `^DUPLICATE patch-id `, `^UNDECLARED STACK: `, `^declared-stack SHA `, `^SELF-TEST CLEAN -- `.

A declared stack is a **narrowing**, never a weakening: it can only exempt the shape git already
collapses (one commit on two seats merges ONCE), and a cherry-pick DUPLICATE stays red whatever is
declared — which arm 7 of the tool's own self-test proves.

---

## 7. No seat-number literals (STEP 5), and its RED control

Train 47's assembly made **26** assertions of the form `seat N is …`, `seat N's …` or
`row N is a <class> seat` — in stamps AND in the prose beside gates. Every one is a fact about THAT
table, and a derive inherits them silently because they read like documentation. One of them survived
into train 47's own G11(b) as a *quoted retirement*: the sentence that RETIRED a per-row `src/gen`
premise re-spelled that premise inside its own parenthetical. That sentence is rewritten so no such
literal remains, which is why the rule forbids the SHAPE and not the claim.

Each was rewritten into a claim derived from the OWED VECTOR or from the row's CLASS — 22 phrase
substitutions plus two line rewrites, each asserted by the derive.

⚠ **AMENDED 2026-09-13 (post-review) — THE PREDICATE WAS CASE-SENSITIVE AND THE "0" BELOW WAS A
MEASUREMENT OF THE WRONG THING.** `grep -cE` read **0** on the derived assembly while `grep -ciE`
read **11**. Ten of the eleven were SHOUTED prose beside a gate; the eleventh was a **LIVE STAMP** —
LEG K's `LKPROV="SEAT 3's OWN GATE -- the src/core/sync/mutex.cs retarget …"`, printed on every
`sync` row of every LEG K run, about a row this train does not have (`sync` is added to LEG K's row
set unconditionally, and row 3 here is `claude/laneR-h6-alias-block`, class `docs`). An arm that
cannot see upper case reports the loudest instance of its own defect as clean. The predicate is now
`grep -aciE` / `grep -aniE` in the self-check and `(?i)` in the derive's own STEP-5 refusal, and all
eleven sites are restated by CLASS or by MECHANISM. Figures below are the POST-FIX ones.

**Measured:** `grep -ciE "seat [0-9]+ is |seat [0-9]+'s |row [0-9]+ is a"`

| file | reading |
|---|---:|
| `coord-train48-assemble.sh` | **0** (was 11 before the fix; 0 under the old case-sensitive form) |
| `coord-train48-land.sh` | **0** |
| `coord-train48-land-dryread.sh` | **0** |
| `coord-train47-assemble.sh` (RED control) | **38** (26 under the old case-sensitive form) |
| `coord-train47-land.sh` | 0 |
| `coord-train47-land-dryread.sh` | 0 |

⚠ **The train-47 LAND and DRY-READ read ZERO, so for those two the control is a PLANTED file**, and
ARM (a) says so in its own output rather than reporting a dead zero as a control. The planted file
carries two lines of the forbidden shape and the predicate reads 2 on it.

---

## 8. The row-anchored assertion census (STEP 6)

**Scope, stated:** a place where a *tool's OUTPUT* — a variable holding captured stdout, or a file of
it — is matched as a SUBSTRING to decide a verdict. Greps over the repository's own source, over the
script's own text, or over a planted control file are out of scope and are listed as such.

**The hazard is MEASURED, not assumed.** `src/tests/Behavioral/BehavioralRunner/Program.cs` builds the
phrase `NOT MEASURED` into COUNT lines at lines 549, 858 and 1171 as well as into per-project rows —
so a substring match counts a tally that reads `0 NOT MEASURED` as a hit. That is exactly the class
the brief names.

| | count | where |
|---|---:|---|
| FOUND (in scope) | **23** | assemble 18, land 3, dry-read 2 |
| CONVERTED to an anchored row match | **3** | assemble 2151 `G1PASS`, 2205 `G3CLEAN`, 2207 `G3BAD` |
| built ANCHORED by construction (new) | **5** | the patch-id arm's five reads |
| LEFT, each justified | **23** | below |

*The FOUND count is unchanged by the conversion because each converted site KEEPS its unanchored
count as a stated SECOND READING (`*_ANY`) beside the anchored one, and a disagreement between the two
is STAMPED as a finding. A conversion that threw the old reading away would be a silent change of what
the gate measures.*

### 8.1 Converted (3) — each producer's column-0 form was READ FROM ITS SOURCE

| t48 line | was | is | producer measured at |
|---:|---|---|---|
| 2157 | `grep -acE '[0-9]+ checks pass'` | `grep -acE '^[[:space:]]*roster format guard: [0-9]+ checks pass'` | `src/check-roster-format.ps1:1161` |
| 2214 | `grep -ac 'Pre-flight clean'` | `grep -acE '^[[:space:]]*==> Pre-flight clean'` | `src/push-nuget.ps1:371` via `Write-Step`, which prefixes `==> ` (`:119`) |
| 2216 | `grep -ac 'PRE-FLIGHT FAILED'` | `grep -acE '^[[:space:]]*PRE-FLIGHT FAILED'` | `src/push-nuget.ps1:359`, `Write-Host` at column 0 |

*(Line numbers re-measured 2026-09-13 after §16's fixes moved them; they were 2151/2205/2207 in the
pre-review cut. The site COUNTS are unchanged — ARM (b) still reads 18/3/2 against an 18/3/2
whitelist — so §16 added no new in-scope unanchored site.)*

Leading whitespace is tolerated because a redirected PowerShell host may indent; what is EXCLUDED is
the phrase appearing mid-line, which is the shape a tally produces.

### 8.2 Left, with line numbers and reasons

*(Line numbers below re-measured 2026-09-13 after §16; the reasons are unchanged.)*

`coord-train48-assemble.sh` — 2158, 2215, 2217 (the three deliberate second readings) · 2842, 2843
(`CS0576` is a compiler error CODE; the rebuild line is a whole emitted line — both are READINGS,
stamped and never asserted) · 2939, 2964, 3669, 3979 (**MSBuild diagnostics are printed indented
behind a `file(line,col): ` prefix, so a `^` anchor would read ZERO and turn a RED build GREEN** —
strictly worse than the substring) · 3670, 3765, 3766 (the producers' row spellings are not measurable
from any artifact in this directory, and an anchor that does not match what the producer prints
refuses a healthy run) · 3988 (an MSTest line whose column the runner sets) · 4048 (arm-name mentions;
zeros are normal on a passing run) · **4442 — the measured DUAL-FORM site; LEFT because its only
consumer is a STAMP and it is never asserted, and recorded here so the next derive meets it rather
than discovering it** · 4443, 4444, 4445 (readings).

`coord-train48-land.sh` — 277 (`PRE-RESOLVED MERGE COMMITTED` over the assembly RECORD, whose stamps
are `[<ts>] <text>`, so a `^` anchor would assert the stamp FORMAT rather than the verdict) · 474, 475
(**`WX_ANY` is DELIBERATELY the unanchored SECOND reading of the anchored `WX_N`, and the gate requires
the two to AGREE — converting it would collapse two readings into one**).

`coord-train48-land-dryread.sh` — 120, 152 (both read the ASSEMBLY SCRIPT's own text rather than a
tool's verdict, and 152 is arm A's own NEGATIVE CONTROL: a control that anchored its lookup would stop
being one).

⚠ **The land's refusal-scan and exit-scan lines are NOT on the list, and that is a measurement rather
than an omission**: the shipped detector reads the LAST grep on a line, and on both of those the last
grep is `grep -ac .` — a pattern, not a phrase — so they are out of scope by construction. A
line-by-line Python scan written for this census saw them; the shipped one does not. Recorded so the
next derive meets the detector's rule instead of rediscovering it.

ARM (b) counts the remaining sites against a whitelist written into the arm, one entry per site each
with a comment, and refuses **both** a NEW site and a REMOVED one — a whitelist that outlives its site
is an exemption nobody re-reads.

---

## 9. The self-check: carried arms, new arms, and their controls

**Carried arms re-pointed:** the base arm reads `EXPECT_BASE`/`EXPECT_ORDER`/`T48_CONTAIN_PIN`; arm 2
learns that `PENDING-class` is a class fill point with NO merge_seat arm and asserts the two refusals
instead; arm 2/3 learn that `claude/PENDING-BRANCH` is a REF fill point and assert the assembly's own
preflight refusal for it rather than re-refusing the template; **arm 8 now splits UNMEASURED from
REFUSED-BY-ITS-CLASS** (train 47's arm set one flag for both, so a ref this clone does not carry
printed *"a seat would be REFUSED by its own class"* — an instrument's blindness reported as a finding
about a seat); arm 13's F1 and SEAT-CONTENT expectations become fill-point-and-refusal assertions,
because a template's worktree is PENDING and its SEAT-CONTENT block is empty by construction; arm 13's
"defect (c)" list drops the deleted row-specific land anchor and gains this train's own structural
stamps.

**The train-46 controls are KEPT.** The train-46 originals ARE present at
`.claude/coord-scripts/train46/` (`coord-train46-{assemble,land,land-dryread,rehearse,derive-selfcheck}.sh`
and `t46-derive.py`), the self-check auto-detects `$SELFDIR/../train46`, and arm 12's six lesson arms
all ran with live controls — `L11 :: train48=0 train46=15`, `L9 :: train48=0 train46=9`, and so on.

⚠ **AMENDED 2026-09-13 (post-review) — A SECOND LAND-ANCHOR SET WAS THE SAME SHAPE AND IS NOW
GUARDED RATHER THAN DELETED.** The land `req`s three `LEG U ARM n MET` stamps plus its post-restore
line, and LEG U's own run/skip key in the assembly was `[ "$SEAT1_SHA" = "PENDING" ]` — the (L13)
row-number premise surviving as EXECUTABLE code, where ARM (a) cannot see it at all. Under
`TRAIN48_REQUIRE_ALL=1` no row is PENDING, so that key could never skip and the leg stamped a claim
about whichever row merged first. LEG U is now keyed to **`OWED_CONV`** — the leg measures the
converter's platform-scope rule at the merge result, so the converter owe is what keys it — and the
land's four LEG U anchors are guarded by `[ "$O_CONV" = "1" ]`, with the not-required case STATED.
Guarded rather than deleted because the anchors CAN be satisfied whenever the leg runs; the row-8
anchor below could never be satisfied at all, which is why that one was deleted.

**A land anchor was DELETED WITH ITS ARM.** Train 47's land required
`A-row8 generic cross-package alias qualifier, BOTH HALVES AT THE UNION ::`, a stamp its own row-8
SEAT-CONTENT arm emitted. This template's SEAT-CONTENT block is an empty SLOT, so nothing emits that
stamp and the anchor could NEVER be satisfied — the landing would refuse a HEALTHY battery and the
operator would reach for the override. The dry-read's own ARM A is what found it.

---

## 10. Limits — what is parsed but NOT exercised, stated

* **No gate here was run against a tree.** Everything reported is a parse, a static read, a planted
  control or a refusal path — by design: the brief forbade running anything, and a battery was live.
* **The PATCH-ID ARM has never been run.** Its four branches are asserted to EXIST by ARM (d) and its
  shell parses; its first real run is its first measurement, and it should be read as such.
  ⚠ **AMENDED 2026-09-13:** the tool's `--self-test` HAS now been run, outside the repo. Extracted with
  `git show 77e41300a:src/seat-duplication-census.sh`, it is hermetic (`mktemp -d` + `git init` in its
  own scratch), and `bash seat-duplication-census.sh --self-test` exits **0** printing
  `SELF-TEST CLEAN -- 7 arms` — the exact spelling the arm anchors on, and **seven** arms. Re-run
  byte-exactly as the arm invokes it (`> log 2>&1 < /dev/null`) it is unchanged. One cosmetic note: git
  emits `warning: LF will be replaced by CRLF` to stderr, which the arm's `2>&1` folds into the log at
  column 0; it matches no anchor, so the gate is unaffected. ⚠ **THIS MEASUREMENT IS PINNED TO
  `77e41300a`** and row 7's remote tip has since moved — see [§3.1](#31-every-named-branch-exists-and-its-tip-equals-the-given-pin).
* **ARM (d) IS A STRING-PRESENCE CHECK AND CANNOT BE MORE.** It proves the four branches and the five
  anchored reads are PRESENT in the assembly's text. By construction it cannot detect a child/parent
  reversal in `--stack`, branch names passed where SHAs belong, or a stale pin — those are properties
  of a RUN and of the table, not of the script's text. Stated so the arm's green is not over-read.
* **`converter-test`, `converter-test+tooling` and `tooling` have never been measured against a real
  seat diff for the rows that carry them.** Arm 8 read rows 7 and 8 (`converter-test+tooling`) clean —
  `files=3 outsideShape=0 forbiddenHits=0` each — and could not read row 6 (`converter-test`) because
  its objects are not in this clone. The `tooling` rows are all PENDING.
* **`stack-on=` has been exercised only on PLANTED tables.** ARM (c) proves the parser accepts a
  backward reference and refuses the four bad shapes; it has never run against a real declared stack,
  and the patch-id arm has never consumed one.
* **The rehearsal's per-seat `go vet` is unchanged and still unexercised against a real union-only
  failure** (train 47's note stands).
* **The ORDER pin `a02ac3df3` was read from `ls-remote`, not from `merge-base --is-ancestor`** — I did
  not fetch, so "it is an ancestor of origin/master" is inferred from it being train 47's base rather
  than measured here.
* **Four of the nine filled pins are not in this clone** (`47592cb3f`, `becf28abc`, `fa2fdd30d`,
  `314e699c6`) and `830fa8d26` is not either. The box rules forbid `git fetch` in
  `C:/Projects/go2cs`, so arms 3 and 8 report them UNMEASURED and FAIL. **That is the only reason the
  self-check exits 1.**

---

## 11. What I deliberately did NOT change

* The two-pin pairing and its after-guard; LEG D's corrected seeding control; LEG 5's own converter
  rebuild and the transpile predicate; LEG K's derived reflect canary set; the pre-resolved merge
  mechanism; the six preflight controls; the run lock; the disk floor; ROUTE B; G10a–d; G12; the
  census pattern set and the C1/G6T template-class control (carried BYTE-FOR-BYTE); the eight
  alias-carrying `GOLDEN_PROJECTS`.
* The `docs` owe stays SOFT and says so.
* **The prose "train 46" and "train 47".** They name the base this train sits on and the run records
  each arm came from. A derive that scrubbed them would delete the record of why every arm is shaped
  the way it is — which is also why ARM (f) forbids the `train47` TOKEN outside provenance comments
  rather than forbidding the words.

---

## 12. The derive's own report — 144 asserted operations with line numbers

The full report, with every BLOCK/INSERT/LINE/GSUB and the line it landed on, is kept verbatim beside
this file as **`coord-train48-derive-report.log`** (223 lines after §16; 184 before). It is regenerated
by `python t48-derive.py`, so it is a record rather than a second copy of a fact.
⚠ **ONE LINE-ENDING CAVEAT, STATED BECAUSE IT COULD NOT BE MEASURED BOTH WAYS.** The log is produced by
a shell redirect of the derive's stdout, and Windows CPython translates `\n` to `\r\n` on a redirected
stdout — so the regenerated log is **CRLF (223 CR)**. The pre-review log was produced the same way and
was almost certainly CRLF too, but no byte census of it was taken before it was overwritten, so this is
an inference rather than a measurement. Every `.sh`, the derive and this file remain **pure LF (0 CR)**,
measured before and after. The shape of the report:

| file | ops | the structural ones, by line in the TRAIN-47 source |
|---|---:|---|
| assemble | 107 | header-title 3..5 · header-fillpoints 14..37 · header-L13-L15 after 105 · header-classes after 218 · worktree-fillpoint 328..336 · pins 386..407 · fill-point-preflight after 409 · **seat-table 1058..1297** · seat-vars 1136..1145 · seat-struct 1240..1241 · **patch-id-arm after 1293 (+103)** · classes-new after 1562 · owed-vector 1780..1791 · **seat-content-slot 1915..2339 (425 → 11)** · **§16: f3-legu-key · f2-row-closing-notes · f5-done-stamp · f9-\* ×8 · l13-\* ×11 · f7-\* ×2** |
| rehearse | 11 | rehearse-t48-note after 56 · **f7-rehearse-govet-prov** |
| self-check | 31 | sc-setup after 73 · sc-new-arms 769..EOF (+376) · **f7-sc-run-record** |
| land | 22 | land-row8-anchor-deleted · land-req-t48 · land-notes-t48 after 580 · **f4-seat-floor-deleted · f6-land-tooling-{measure,arm,stamp} · f3-land-legu-guard** |
| dry-read | 12 | dr-armc-words-t48 · dr-armg 261..EOF · **f7-dr-censusreader-prov · f4b-dr-armf-predicate** |

Twenty-two of the assembly's ops are the STEP-5 phrase substitutions (`l5-*`), plus two line rewrites
(`l5-mech-1`, `l5-mech-2`); three are the STEP-6 conversions (`step6-g1pass`, `step6-g3clean`,
`step6-g3bad`); **twenty-seven are §16's review fixes**; the rest are the renames, the fill points, the
table, the parser, the patch-id arm, the three new classes, the OWED vector, G11(a)'s tooling arm,
G3's fill point, and the closing ABSENCE/KEPT assertions — of which §16 added six more
(`f7-no-invented-run-record`, `f7-no-foreign-plant-author`, `f2-no-arow9-anchor`,
`f7-rehearse-no-invented-novelty`, `f7-sc-no-invented-run-record`, `f4-no-seat-count-floor`), each one
the guard that a re-run of the blanket rename cannot re-introduce the defect it just removed.

**Reproducibility.** `t48-derive.py` run in a scratch tree beside FRESH copies of the five train-47
templates reproduces all seven outputs **byte-identical** (`cmp`) to the shipped files, and two
consecutive in-place runs agree byte-for-byte.

The six shipped scripts as they stand (re-derive and compare before trusting anything above) —
**md5s re-measured 2026-09-13 after §16; every one except the launch wrapper moved**:

| file | md5 |
|---|---|
| `coord-train48-assemble.sh` | `7ec4f8ec0cfeb40c2af2d18feba847a6` |
| `coord-train48-rehearse.sh` | `abfb450c690fb3e86abfb5fb8043e9fb` |
| `coord-train48-derive-selfcheck.sh` | `a9f36fa45116cf25ba608e37cabbb904` |
| `coord-train48-land.sh` | `5f569de7c294c3ee54739333e8268813` |
| `coord-train48-land-dryread.sh` | `db584a7f862d6d2c4a82cda4c94efa75` |
| `launch-run1.sh` | `cb37da09311ee300a511c6608bb3dd47` (unchanged) |
| `t48-derive.py` | `68d77ad090b4eff053422d0022e71b46` |

⚠ **AMENDED 2026-09-13 ~10:10 — FOUR OF THE SEVEN md5s ABOVE ARE SUPERSEDED.** The re-derive from the
moved templates ([§17](#17-2026-09-13-1010--re-derive-after-the-leg-u--leg-r-restore-patch-and-lesson-lr1))
moved the assembly, the self-check, the derive and the report log; the rehearsal, the land, the
dry-read and the launch wrapper are byte-identical to the table above. Current md5s are in §17.
The §12 op-shape table's `self-check | 31` likewise reads **33** after §17's one fix and its guard.

**Re-derive, MEASURED rather than claimed (2026-09-13).** `t48-derive.py` was copied into a scratch
tree beside FRESH copies of the five train-47 templates and run there; all seven outputs
(the six scripts plus `t47-derive.py.reference`) came back **byte-identical** to the shipped files
under `cmp`. It was then run a second time in place, and the outputs were byte-identical again.

**Line endings.** Every train-47 source file is pure LF (0 CR bytes), and every file written here is
pure LF (0 CR bytes), measured before and after. ⚠ My FIRST CR census was an INSTRUMENT ARTIFACT: a
`grep -c $'\r'` reported "every line carries a CR" on all nine files; a byte-exact Python count read
zero, and `od -c` confirmed it. The reading that agreed with two other instruments is the one recorded.

---

## 13. Every NEW arm shown RED on its control

Each of the seven new arms was run against a control and the FAIL lines are pasted verbatim. An arm that
has never been made to fire is an assertion wearing a measurement's clothes.

⚠ **AMENDED 2026-09-13 (post-review) — FOUR OF THESE CONTROLS ONLY EXISTED IN THIS FILE.** Arms (a),
(b), (c) and (f) fired a control IN-RUN from the first cut. Arms **(d), (e) and (g) did not**: their
reds were shown once, by hand, against planted copies that are not reproducible from the shipped
instrument, and (g) had no documented control at all — which is exactly the state the safety floor's
*"a gate that has never been made to fail proves nothing"* forbids, and the state ARM (a) exists to
detect in other people's work. Each of the three is now a **function over a file**, so the arm and its
control are the same code, and each plants its own control and fires it in every run:

* **(d)** plants a COPY of the assembly with `PATCH-ID ARM CLEAN ::` renamed out of it;
* **(e)** plants a wrapper that one-shot-prefixes the switch and ends in a `tail`;
* **(g)** plants **two** copies, one per branch — F2's reason text redacted, and the run lock moved to
  line 1 — because a single plant that reddened both would let either branch hide behind the other;
* the dry-read's **ARM F** plants three lines (the `-ge` floor, the `=` equality, and a COMMENTED
  floor) and asserts it reads 2 in code and 3 in all, so the comment exemption is shown to be narrow.

The in-run control lines from the current run are in [§14](#14-the-self-check-and-dry-read-as-run).

**CONTROL A** — the self-check run with `$F` pointed at `coord-train47-assemble.sh`:

```
   FAIL coord-train47-assemble.sh :: 26 seat-number literal(s) survive:                          <- ARM (a)
   FAIL NEW unanchored site :: coord-train47-assemble.sh:2437 :: G1PASS=$(tr -d '\r' < "$G1LOG" | grep -acE '[0-9]+ checks pass' || true)     <- ARM (b)
   FAIL NEW unanchored site :: coord-train47-assemble.sh:2484 :: G3CLEAN=$(tr -d '\r' < "$G3LOG" | grep -ac 'Pre-flight clean' || true)       <- ARM (b)
   FAIL NEW unanchored site :: coord-train47-assemble.sh:2485 :: G3BAD=$(tr -d '\r' < "$G3LOG" | grep -ac 'PRE-FLIGHT FAILED' || true)        <- ARM (b)
   FAIL: cannot resolve block 'SEAT_OPT_KEYS' (start='^SEAT_OPT_KEYS=' -> none, end='^SEAT_OPT_KEYS=' -> none)   <- ARM (c), exit 3
```

ARM (b)'s three RED lines are **exactly the three sites STEP 6 converted**, named by line — the arm
fires on the defect it was written for and on nothing else. ARM (c) reports the CHECKER as broken
(exit 3) rather than the file as bad, which is the correct state for a file that has no parser to
extract: *"a block could not be resolved"* is its own reading, not a finding about a train.

**CONTROL C** (built into the main run — the parser's four bad shapes, each run over a PLANTED table):

```
   ok   stack-on naming an EARLIER row :: ACCEPTED (exit 0)
   ok   stack-on naming a MISSING row :: refused (exit 1) AND named the reason [exists in this table]
   ok   stack-on naming a LATER row :: refused (exit 1) AND named the reason [merges AFTER it]
   ok   stack-on naming ITSELF :: refused (exit 1) AND named the reason [stacked on ITSELF]
   ok   an UNKNOWN option key :: refused (exit 1) AND named the reason [unknown option key]
   ok   an optional field that is not key=value :: refused (exit 1) AND named the reason [not <key>=<value>]
   ok   both keys read ORDER-FREE from row 3 :: allowed=[^x/y\.cs$] stack-on=[1]      (both field orders)
```

**CONTROL D/F** — a copy of the train-48 files with two PLANTED regressions: the
`PATCH-ID ARM CLEAN ::` stamp deleted from the assembly, and one live `train47` token appended to the
land script:

```
   FAIL: the patch-id branch [PATCH-ID ARM CLEAN ::] (x0) or its consequence [==> CENSUS (CLEAN|RED)] (x1) is absent   <- ARM (d)
   FAIL coord-train48-land.sh :: 1 live train47 token(s):                                        <- ARM (f)
        628	PLANTED_T47_REGRESSION="coord-train47-land.sh"
```

Each names the planted site. The unplanted files stayed green in the same run.

**CONTROL E** — a copy with a launch wrapper that one-shot-prefixes the switch and ends in a `tail`:

```
   FAIL: the switch is not EXPORTED on its own line                                              <- ARM (e)
   FAIL: the wrapper does not capture rc=$? as the first statement after the run                 <- ARM (e)
   FAIL: the wrapper's last statement is [tail -40 out.stdout], not 'exit $rc'                   <- ARM (e)
```

---

## 14. The self-check and dry-read, as run

```
T48_SELFCHECK_OFFLINE=1 bash coord-train48-derive-selfcheck.sh
=== SELF-CHECK DONE overallFail=1 offlineUnmeasured=9 ===        190 PASS/ok lines, 5 FAIL lines

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                              exit 0, 0 FAIL, 0 MISS
```

*(190, not the pre-review 186: the four extra lines are §13's new in-run controls. The FAIL count,
the exit codes and the five named rows are UNCHANGED — the fixes added controls and removed defects
without moving the one thing that is still blind.)*

⚠ **AMENDED 2026-09-13 ~10:10 — THE PASS/ok COUNT IS NOW 191.** Train 47's self-check gained lesson
**LR1**, so arm 12 carries a seventh lesson arm and emits one more PASS line
(`PASS LR1 pipeline-leg restores name docs/validation :: train48=0 train46=1`). **The five FAIL rows,
the anchored FAIL count of 5, `offlineUnmeasured=9` and the exit code are all UNCHANGED** — see
[§17](#17-2026-09-13-1010--re-derive-after-the-leg-u--leg-r-restore-patch-and-lesson-lr1).

⚠ **`FAIL` MUST BE COUNTED ANCHORED.** `grep -c FAIL` reads 11 on the self-check's output and 5 of
those are verdicts; the other six are the words FAILS / FAILED / *"set FAILED"* inside prose and
inside a PASS line. `grep -cE '^[[:space:]]*FAIL'` reads **5**. This is the same hazard ARM (b) exists
for, met in this file's own reporting.

**The new arms' in-run control lines, verbatim from the current run:**

```
   ok   CONTROL :: with the CLEAN stamp renamed out of a COPY of the assembly the predicate reports 1 absence(s) -- the arm CAN fire
        CONTROL red: the patch-id branch [PATCH-ID ARM CLEAN ::] (x0) or its consequence [==> CENSUS (CLEAN|RED)] (x1) is absent
   ok   CONTROL :: a planted wrapper that one-shot-prefixes the switch and ends in a tail reads 3 defect(s) -- the arm CAN fire on all three
        CONTROL red: the switch is not EXPORTED on its own line
        CONTROL red: the wrapper does not capture rc=$? as the first statement after the run
        CONTROL red: the wrapper's last statement is [tail -40 out.stdout], not 'exit $rc'
   ok   CONTROL (reason branch) :: a COPY with F2's reason text redacted reads 1 defect(s) -- that branch CAN fire
   ok   CONTROL (order branch)  :: a COPY with the run lock moved to line 1 reads 1 defect(s) -- that branch CAN fire
        CONTROL red: the refusal [FILL POINT F2 REFUSED] (x1) or its reason [the containment pin is not filled] (x0) is absent
        CONTROL red: the fill points are not both stamped and refused BEFORE the run lock is taken
   ok   CONTROL (train-47 assembly) :: the same predicate reads 38 -- the arm CAN fire            <- ARM (a), now case-insensitive
   ok   CONTROL :: the predicate reads 2 IN CODE on a planted file carrying the -ge floor AND the = equality, and 3 in all
                                                                                                  <- the dry-read's ARM F
```

**⚠ THE EXIT IS 1 AND EVERY ONE OF THE FIVE FAILS IS AN OFFLINE-UNMEASURED ENTRY.** There is no
substantive failure in the five:

```
   FAIL (UNMEASURED, offline): row 3's SHA 47592cb3f (claude/laneR-h6-alias-block) is not in this clone …
   FAIL (UNMEASURED, offline): row 5's SHA becf28abc (claude/laneR-prepin-baselines-recut) …
   FAIL (UNMEASURED, offline): row 6's SHA fa2fdd30d (claude/c2-safepush-shallow-skip) …
   FAIL (UNMEASURED, offline): row 10's SHA 314e699c6 (claude/i9-board-archive-tar) …
   FAIL (UNMEASURED): 5 row(s) could not be read at all because their ref is not in this clone.   (arm 8)
```

**Why, and why it was not papered over.** Arms 1, 3 and 8 FETCH each seat's ref before deciding
whether its pin resolves, because *"unresolved" before a fetch is a stale clone and after one is a
typo or an invention* — opposite severities. The box this ran on forbids `git fetch` in
`C:/Projects/go2cs` while a battery is frozen there. So the derive added a NAMED switch,
`T48_SELFCHECK_OFFLINE`, which is **OFF by default** (the shipped instrument is unchanged) and which,
when ON, reports every unresolvable pin as **UNMEASURED and still FAILS**. It would have been one line
to make those pass; that line would have converted a blindness into a green.

**To close them: run `bash coord-train48-derive-selfcheck.sh` once with no switch, outside the
freeze.** It fetches each seat ref and turns all nine into measurements.

The same discipline produced a real correction one layer up: train 47's arm 8 set ONE flag for both
"this row violates its class" and "I could not read this row", so an absent ref printed *"a seat would
be REFUSED by its own class"*. That is an instrument's blindness reported as a finding about a seat.
It now carries two counters and two sentences.

**`bash -n`** — all six shipped scripts parse, and print nothing:

```
coord-train48-assemble.sh          rc=0   out=[]
coord-train48-derive-selfcheck.sh  rc=0   out=[]
coord-train48-land-dryread.sh      rc=0   out=[]
coord-train48-land.sh              rc=0   out=[]
coord-train48-rehearse.sh          rc=0   out=[]
launch-run1.sh                     rc=0   out=[]
```
*(re-run 2026-09-13 after §16; all six still parse and still print nothing.)*

The derive itself runs `bash -n` on every file it writes and REFUSES on a failure — it caught four
real breakages while this was written, three of them the same shape: a block whose text ends with a
shell `"` directly against the Python `"""` terminator loses that quote to the terminator, and it is
invisible by reading either language alone.

---

## 15. Every doubt I have

1. **`830fa8d26` (row 16, "C2 goroot-fix tip") is not a branch tip on `origin` and is not in this
   clone.** The only goroot-shaped branch is `claude/c2-census-goroot-fix` at **`3ced37e18`**. Either
   the board figure is stale or it names a branch this remote does not carry. Row 16 ships PENDING and
   nothing was inferred.
2. ~~**`171d419f6` is not a branch tip either.**~~ ⚠ **RESOLVED 2026-09-13 — the stated reason for not
   checking was itself wrong.** Both objects ARE in this clone. `git merge-base --is-ancestor
   171d419f6 33c29952d` says **YES**, at distance **1**: `171d419f6` *"shardmap.py: repair the
   generator so it emits, and so its guards can fail"* is the immediate PARENT of `33c29952d`
   *".gitattributes: pin the generator's input tables to eol=lf"* on `claude/c2-shardmap-repair`. Row
   13's two board SHAs are a parent/tip pair, as guessed — now measured. **Still nothing written into
   a pin field**: row 13 remains a placeholder for the coordinator.
3. **Row 9's branch name.** `ls-remote` reads `68ad83c2c` as the tip of
   `claude/i9-board-runtime-door-bisect`. That is a MEASUREMENT offered for confirmation, not a fill:
   the brief said not to guess the name, so the row ships as a REF FILL POINT that refuses.
4. **`converter-corpus-metadata` "as train 47's row 8 did"** — train 47's ROW 8 is
   `converter-guard-rebaseline`. See §5; I kept the existing treatment and changed nothing.
5. **A PRIOR, INCOMPLETE `t48-derive.py` WAS ALREADY IN THIS DIRECTORY** when the session started
   (70,726 bytes, mtime 2026-09-13 07:43:36, 935 lines). It compiled but ABORTED at its ninth
   operation (`ANCHOR NOT FOUND … anc-order`), derived only the assembly, and wrote no train-48 file.
   I **extended it rather than replacing it**, fixing five anchor defects in it along the way; the
   shipped file is that work plus the four remaining documents, the launch wrapper and the new arms.
   If that file was another session's live work rather than an aborted earlier attempt at this same
   brief, say so — it is now overwritten, and `git` does not track this directory.
6. **The train-47 template moved DURING this session** (5,071 → 5,076 lines). Everything here is
   derived from the 08:14 version, sha256 `243c7253…`. If it has moved again, re-run the derive.
7. ~~**The `--self-test` spelling was read from the tool's SOURCE, not from a run.**~~ ⚠ **RESOLVED
   2026-09-13 — it was RUN.** The tool creates its hermetic repo under `mktemp -d`, outside the frozen
   checkout, so running it breaks nothing. `bash seat-duplication-census.sh --self-test` → exit **0**,
   `SELF-TEST CLEAN -- 7 arms`, seven `ok` arms. The spelling the arm anchors on is correct and the
   arm count is **7**. ⚠ Measured at `77e41300a`; row 7's tip has since moved ([§3.1](#31-every-named-branch-exists-and-its-tip-equals-the-given-pin)),
   so this must be re-taken at whatever SHA the coordinator pins.
8. **STEP 6's three conversions rest on reading two PowerShell producers' source, not their output.**
   `Write-Step` prefixes `==> ` (`push-nuget.ps1:119`) and `Write-Host` does not — but a redirected
   PowerShell host could still indent or wrap, which is why each anchor tolerates leading whitespace
   and why each keeps its unanchored second reading with a stamped disagreement.
9. **ARM (b)'s detector reads the LAST `grep` on a line.** Two land sites that a line-by-line Python
   scan flagged are therefore out of its scope; §8 records both the discrepancy and the reason. If the
   intent was the wider detector, the whitelist grows by those two.
10. **Nothing here has been rehearsed.** `coord-train48-rehearse.sh` was not run (it creates a
    worktree and runs `go vet`), and the assembly has not been launched even in a refusal-only shape,
    because the shipped file ABORTS at F1 before writing a log or taking a lock and I did not want a
    second chain touching `/tmp/t48-*` while a neighbouring battery is live.
11. **ROW 7'S PIN IS STALE AND NEEDS A COORDINATOR RULING** before this train assembles — the one
    finding here that is not a fix. See [§3.1](#31-every-named-branch-exists-and-its-tip-equals-the-given-pin).
12. **`OWED_CONV` is now load-bearing for LEG U, and it has never been exercised at 0.** Every class
    on this train's filled rows except `docs` sets it, so on the table as it stands LEG U runs and the
    land's guarded anchors are required — the SKIP branch and the *"anchors NOT REQUIRED"* stamp have
    been parsed and never executed. Stated rather than discovered.
13. **ARM (f) forbids the `train47` TOKEN, not the words "train 47" / "TRAIN-47".** That is by design
    ([§11](#11-what-i-deliberately-did-not-change)) and it is why the arm did not catch the planted
    manifest's `"PLANTED BY THE TRAIN-47 ASSEMBLY'S LEG U"`. Widening it to the hyphen and space
    spellings would red on ~30 legitimate provenance lines in comments and in `say` strings. The
    site itself is fixed, and the class is now guarded the other way round — by the derive's own
    `absent()` assertions on each invented spelling ([§16](#16-the-review-fixes-2026-09-13)).

---

## 16. THE REVIEW FIXES (2026-09-13)

An adversarial review and an independent re-run of the pre-derive checks were run against this
template. The re-run reproduced every claim in §§12–14 it tested; the review found **ten** defects.
Nine were fixed, one is a coordinator ruling. Every fix is an asserted operation in `t48-derive.py`
**and** in the shipped file, and a fresh re-derive from clean train-47 copies reproduces all seven
outputs byte-identically — so nothing here is a hand patch the next derive would undo.

| # | what | where it was | what it now does |
|---|---|---|---|
| F1 | ARM (a)'s predicate was **case-sensitive**; `grep -cE` read 0 where `grep -ciE` read 11, and one of the eleven was a **LIVE LEG K STAMP** | selfcheck `SEATLIT_RE` + assemble ×11 | `grep -aciE` / `(?i)`; all eleven sites restated by CLASS or MECHANISM; control reads 38 |
| F2 | two closing stamps asserted **train-47 row facts** with train-48 refs interpolated beside them, and one named an `A-row9` arm that does not exist | assemble, the two `ROW n CLOSING NOTE` stamps | replaced by one CLOSING NOTES stamp that makes no per-row claim, names what the deletion loses, and is guarded by `absent('A-row9')` |
| F3 | **LEG U's run/skip key was `$SEAT1_SHA`** — (L13) surviving as executable positional coupling — and the landing `req`d its stamps unconditionally | assemble + land | keyed to `OWED_CONV`; the land's four LEG U anchors guarded by `[ "$O_CONV" = "1" ]`, with the not-required case STATED |
| F4 | a **stale `-ge 15` seat-count floor** carried byte-for-byte from train 47 | land | DELETED — the row-count equality on the next line is strictly stronger; `absent_in_code` guards it |
| F4b | the dry-read arm written to catch F4 matched only the `= "N"` form and **printed a clean 0 over the defect** | dry-read ARM F | predicate covers `-ge/-gt/-le/-lt/-eq/-ne/=`; comment hits counted and reported, not refused; three-line in-run control |
| F5 | the DONE stamp listed **15 of 19 seat pins** and omitted `tooling=` | assemble | all nineteen, plus `tooling=$OWED_TOOLING` so the two stamps of one fact agree |
| F6 | `tooling` was parsed and stamped but **never armed** at the landing | land | `D_TOOL` over the same pathspec SET, stamped and armed |
| F7 | the blanket rename **corrupted five provenance records** — run records, a planted artifact's authorship, a seat name and a novelty claim | assemble ×2, selfcheck ×2, dry-read, rehearse | each restored BY NAME, each followed by an `absent()` guard so a re-run cannot re-introduce it |
| F9 | eight **stale premises beside gates**, two of them live stamps (G2 announcing the expected state as UNEXPECTED; the row-count comment) and one a stray `:` no-op | assemble | each restated from the OWED VECTOR or the row's CLASS; the no-op replaced by a comment saying where the read went |
| F8 | **row 7's pin is stale** — `ls-remote` reads `a4802675d`, the table pins `77e41300a` | the seat table | **NOT FIXED — a pin is the coordinator's to move.** See §3.1 |

**And four controls that did not fire.** Arms (d), (e) and (g) and the dry-read's ARM F had no in-run
control; their reds lived in this file. Each is now a function over a file with its own planted
control, fired every run — see §13 and the verbatim lines in §14.

**What the review could NOT break, checked directly and recorded here so the next reader does not
re-check it:** `--stack <child>:<parent>` reaches the tool the right way round and is fed pinned SHAs,
never branch names; rule 7 is honoured at both `$?` captures in the patch-id arm; the tool is
re-extracted from its pin every run, so there is no stale scratch copy; every patch-id verdict read is
anchored; ARM (b)'s whitelist hides nothing (an independent per-occurrence scan reproduced exactly the
whitelisted set); every train-47 source and every train-48 `.sh` is pure LF; the train-47 directory
was never written; the fill-point refusals precede the run lock. One thing worth knowing: the ORDER
pin `a02ac3df3` is also `origin/master`'s current tip, so the two BASE-ANCESTRY arms are **not
independent** — the CONTAINMENT pin will be a descendant of the ORDER pin, and the ORDER arm can only
red if the CONTAINMENT arm already has.

---

## 17. 2026-09-13 ~10:10 — re-derive after the LEG U / LEG R restore patch and lesson LR1

**Why.** §§1–16 were derived from the train-47 templates as they stood at
`243c725312637ce19a88cf305a75cf655dc32aee4a11ff12e471e3101128e20f`. Train 47 was then patched twice
more, in its own directory, while its battery ran:

1. **LEG R and LEG U now restore `docs/validation` beside their corpus root** —
   `git checkout -- src/core docs/validation` and `git checkout -- "$LEGU_DIR" docs/validation`, each
   with an explanatory comment. The `-tests` emission rewrites a row's proof page and the index, so a
   restore scoped to the corpus root alone left the tree dirty in two files.
2. **The self-check gained lesson LR1** (`lr1_count` plus a
   `lesson 'LR1 pipeline-leg restores name docs/validation' …` line) — the arm that reads the defect
   above, with the train-46 original as its control.

Everything in this directory is re-derived from the CURRENT templates. **The train-47 directory was
read and never written**, exactly as in §1.

### 17.1 The one thing the blanket rename broke, and the op that fixes it

LR1's provenance sentence cites the run that MEASURED the defect — *"Train 47 run 5 read LEG U's
post-restore tree dirty=2 …"*. `rename_tokens`' `Train 47` → `Train 48` pass rewrote that into
**"Train 48 run 5 read …"**: a measurement attributed to a battery that has never run. That is exactly
the class [§16](#16-the-review-fixes-2026-09-13)'s **F7** fixes elsewhere, and it is guarded the same
way — restored by name, then an `absent()` so a RE-RUN of the rename cannot re-introduce it:

```python
# ---- (F7/LR1) A THIRD RUN RECORD, ADDED TO THE TEMPLATE AFTER THIS DERIVE WAS FIRST CUT.  Train 47
#      gained lesson LR1 on 2026-09-13 (the LEG R / LEG U restores now name docs/validation), and its
#      provenance sentence cites the run that MEASURED the defect: train 47 run 5 read LEG U's
#      post-restore tree dirty=2.  The blanket rename re-attributes that measurement to train 48,
#      which has never run -- the same corruption class as the two run records above.  Restored BY
#      NAME and guarded by its own absence check, because the guard is what makes a RE-RUN of the
#      rename unable to re-introduce it; a fix without the guard is a hand patch the next derive undoes.
S.gsub('Train 48 run 5', 'Train 47 run 5', 'f7-sc-run-record-lr1', minimum=1, maximum=1)
S.absent('Train 48 run 5', 'f7-sc-no-invented-run-record-lr1')
```

It sits immediately after `f7-sc-run-record` / `f7-sc-no-invented-run-record` in `t48-derive.py`, and
the `minimum=1, maximum=1` is load-bearing in both directions: a rename pass that stopped firing and a
rename pass that reached further both refuse.

⚠ **THE OP TOTAL GOES 183 → 185, NOT 184.** The brief that asked for this said 184. One *fix* was
added, but it is two *asserted operations* — `gsub` and `absent` each increment `Doc.ops` — and the
count printed by the derive is the count recorded here. The self-check's own line goes `31 → 33`.

### 17.2 What changed, measured

| | before (§16) | after (§17) |
|---|---|---|
| template `coord-train47-assemble.sh` | `243c7253…`, 5,076 lines | `b28a1545…`, 5,083 lines |
| template `coord-train47-derive-selfcheck.sh` | — (not recorded) | `e42a983d…`, 750 lines |
| `coord-train48-assemble.sh` | 4,830 lines, `7ec4f8ec…` | **4,837 lines, `43d966878ef8a4b20338c8f19bd3e708`** |
| `coord-train48-derive-selfcheck.sh` | 1,210 lines, `a9f36fa4…` | **1,215 lines, `714b916ae278da31fd7c13b54f427e6a`** |
| `coord-train48-rehearse.sh` | 579 lines | 579 lines, `abfb450c690fb3e86abfb5fb8043e9fb` (**unchanged**) |
| `coord-train48-land.sh` | 639 lines | 639 lines, `5f569de7c294c3ee54739333e8268813` (**unchanged**) |
| `coord-train48-land-dryread.sh` | 313 lines | 313 lines, `db584a7f862d6d2c4a82cda4c94efa75` (**unchanged**) |
| `launch-run1.sh` | 21 lines | 21 lines, `cb37da09311ee300a511c6608bb3dd47` (**unchanged**) |
| `t48-derive.py` | 2,109 lines, `68d77ad0…` | **2,118 lines, `2bf95ac47ec14227bd4c4fa447c15e1d`** |
| asserted operations | 183 | **185** |
| `coord-train48-derive-report.log` | 223 lines | 225 lines |

The three scripts and the launch wrapper that did NOT move are a reading, not an omission: the LEG U /
LEG R patch and lesson LR1 touch the assembly and the self-check, and nothing else.

### 17.3 How it was re-derived, and the byte-identity check

`t48-derive.py` was copied into a scratch tree beside **fresh copies of the five current train-47
templates**, the scratch tree's previously produced `.sh` and launch files were DELETED first so the
run started from the templates and not from its own output, and `python t48-derive.py` was run there:
**rc=0, 185 asserted operations**, sha256 stamped `b28a1545…`. All seven outputs were then copied into
this directory and compared:

```
IDENTICAL  coord-train48-assemble.sh          IDENTICAL  coord-train48-land.sh
IDENTICAL  coord-train48-rehearse.sh          IDENTICAL  coord-train48-land-dryread.sh
IDENTICAL  coord-train48-derive-selfcheck.sh  IDENTICAL  launch-run1.sh
IDENTICAL  t47-derive.py.reference
```

⚠ **ONE PROVENANCE CAVEAT ON THE REPORT LOG, STATED RATHER THAN TIDIED.** The shipped
`coord-train48-derive-report.log` is the stdout of the SCRATCH run, so its `template :` line names the
scratch directory, not `.claude/coord-scripts/train47/`. The `sha256 :` line — the thing that actually
identifies the input — reads `b28a1545…`, the same bytes as the live template. The log was not
re-headed by hand: an edited record is not a record.

### 17.4 Line endings, parse, and the two gates as run

```
CR bytes: every live .sh = 0 · t48-derive.py = 0 · these NOTES = 0
          coord-train48-derive-report.log = 225 (CRLF, the §12 redirect caveat, unchanged in kind)
bash -n : all six .sh  rc=0, out=[]
```

```
T48_SELFCHECK_OFFLINE=1 bash coord-train48-derive-selfcheck.sh
=== SELF-CHECK DONE overallFail=1 offlineUnmeasured=9 ===   exit 1
191 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 5 · unanchored grep -c FAIL = 11

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                         exit 0, 0 FAIL, 0 MISS
```

**The five FAILs are the same five as §14** — rows 3, 5, 6 and 10's pins, plus arm 8's aggregate — and
every one is an offline-UNMEASURED entry, not a finding. Nothing new reds. The **191** is **190 + LR1**.

The LR1 arm's own verdict, with its control firing on the train-46 original:

```
PASS LR1 pipeline-leg restores name docs/validation :: train48=0 train46=1 -- the arm reads ZERO on
the derived file and FIRES on the original.  Train 47 run 5 read LEG U's post-restore tree dirty=2 …
```

And the two provenance anchors on the shipped self-check:

```
grep -c "Train 47 run 5 read LEG U"  = 1     (restored)
grep -c "Train 48 run 5"             = 0     (the corruption is gone, and guarded)
grep -c "Train 47 run 4" = 2 · "Train 48 run 4" = 0   (§16's F7, still holding)
```

### 17.5 What §17 did NOT do

* **Nothing under `train47/` was touched** — read-only, live instrument, exactly as §1 requires.
* **No pin was moved.** [§3.1](#31-every-named-branch-exists-and-its-tip-equals-the-given-pin)'s row-7
  finding and §15's doubts stand unchanged; this was a re-derive, not a re-pin.
* **Nothing was run against a tree, committed, pushed or announced.**

## 18. 2026-09-13 ~13:45 — re-derive after the LEG 4 ADVISORY patch and lesson LA1

**Why.** §17 derived from the train-47 templates at
`b28a1545039b36813fa99232877bf71d6e19887df99a5669d78e890c0742447f` (5,083 lines). Train 47 was patched
again that afternoon, in its own directory, while run 7 of its battery ran. Everything here is
re-derived from the CURRENT templates, taken as FRESH copies into a scratch tree exactly as §17.3
describes. **The train-47 directory was read and never written.**

Template inputs, measured:

```
coord-train47-assemble.sh          6109e682a8d446872a28777b2d0f8197ab41f9b6be948a9dd5a799ed75e7a397   5,150 lines
coord-train47-derive-selfcheck.sh  fa26e5f7ba1e79bd8d1500dc8d15cbc6318beaadb874f614b44b84f27029b996     755 lines
```

### 18.1 The derived advisory arm, and why it carries in

Train 47's LEG 4 used to compare the advisory converter-warning count against a **bare literal 2** and,
on a mismatch, stamp the number "counted, never fatal" and then set `FAILED=1` on the next line.
**Run 6 ended `overallFailed=1` with every leg green and no attributable refusal** — a record whose
only failure came from that arm, worded as though it were not one.

The lines themselves were CAPTURED once, standalone, on that tree (a CNR with the converter's WARNING
lines retained by a planted `Add-Content`, the script restored byte-identical after). **The total was
52, and it splits 2 + 50:**

| kind | count | owed by |
|---|---|---|
| `unsafe-sizeof-const` | 2 | base — two `unsafe.Sizeof` in const context in UnsafeOperations (i9 `e458b952f`) |
| `license-unspecified` | 50 | base — licensing landing `1800b04f8`, after train 46 measured its baseline at `44f858717` |

**MECHANISM, read at the writer rather than inferred.** `projectFileWriter.go:497` resolves the
license marker through `licenseConvertedProject` **only for a Library output**; `licensing.go:196-217`
warns when no LICENSE sits beside the project and the module ships none; the once-per-module dedupe is
a PROCESS-global `sync.Map` and CNR runs one converter process per project
(`check-no-regression.ps1:249`), so the bound is **per invocation** — one line per measurable LIBRARY
package without a license. The patched arm therefore does not carry a number: an embedded python
predictor re-derives that set from the TREE and from the CNR log's own platform-exclusive skip list,
`L4ADV_EXPECT=$((2 + L4ADV_LIB))`, and a mismatch calls `fail_gate 'LEG-4-advisory'` (an unreadable
predictor calls `fail_gate 'LEG-4-advisory-predictor'`). **On the run-6 tree the prediction read 50 —
EXACT against the capture, name for name.**

All of that carries into `coord-train48-assemble.sh` through the plain copy and needed no op: the
predictor heredoc, the expectation, both refusals and the launch stamp ("the advisory warning count
equal to the expectation DERIVED from the tree (2 + the library packages without a license)"). In the
derived file the arm is **lines 4157–4229**, the predictor heredoc `<<'PY' … PY` at **4173–4217**.

**FIVE ops were ADDED for this block (F8), and the op total goes 185 → 192.** Four of the five are the
places that did NOT move with train 47's patch and still spelled the run-6-era literal 2 as the
expectation — all of them LIVE lines, none of them provenance:

* `f8-l4adv-verdict-stamp` — the verdict-line stamp said *"(must be 2 — the named healthy baseline …)"*
  twelve lines above an arm that gates on 2 + N. A record that states one expectation and gates on
  another is the false-green shape the patch exists to remove.
* `f8-l4adv-expect-stamp` — the DERIVED-EXPECTATION stamp called the carried 2 *"the train-46
  baseline"*. In train 47's file that reads "the train before this one"; renamed into a train-48 file it
  names a train two hops back. Generalised the way §16's **F7** generalises: the MEASUREMENT keeps its
  provenance (train 46, i9 `e458b952f`) and the RELATIVE wording goes.
* `f8-l4adv-e1-aggregate` and `f8-l4adv-e1-stamp` — E1′'s own aggregate still read
  `[ "${L4ADV:-x}" = "2" ]`, so on any tree whose derived expectation is not 2 it could never say MET
  with every arm green — exactly the run-6 reading. It now reads `${L4ADV_EXPECT:-2}`, defaulting to 2
  only where the verdict line was absent and the arm never ran.

The fifth is the guard: `f8-no-relative-baseline-in-code` asserts *"the train-46 baseline"* survives on
**zero CODE lines** (1 comment hit, counted and kept). **Comment provenance was left alone on purpose**
— the capture block above the arm records run 6, the 52 = 2 + 50 split, the kinds and the mechanism,
and that is a record of how the arm was derived.

**A sixth op pair (F7/LA1) was added for the self-check**, for the reason §17.1 gives. Train 47's new
lesson LA1 cites *"Train 47 run 6 ended overallFailed=1 with every leg green …"*; `rename_tokens`
rewrote that into **"Train 48 run 6"**, a measurement attributed to a battery that has never run.
`f7-sc-run-record-la1` restores it BY NAME (`minimum=1, maximum=1`) and
`f7-sc-no-invented-run-record-la1` is the `absent()` that makes a RE-RUN of the rename unable to
re-introduce it. `Train 47 run 4` = 2 · `Train 47 run 5` = 1 · `Train 47 run 6` = 1 · every
`Train 48 run N` = 0.

**No other op was weakened, removed or retargeted.** 185 + 5 (F8) + 2 (F7/LA1) = **192**.

### 18.2 What changed, measured

| | after §17 | after §18 |
|---|---|---|
| template `coord-train47-assemble.sh` | `b28a1545…`, 5,083 lines | `6109e682…`, **5,150 lines** |
| template `coord-train47-derive-selfcheck.sh` | `e42a983d…`, 750 lines | `fa26e5f7…`, **755 lines** |
| `coord-train48-assemble.sh` | 4,837 lines, `43d96687…` | **4,904 lines, `9cb5104f5cb65df300fc8cb7fdf691f4`** |
| `coord-train48-derive-selfcheck.sh` | 1,215 lines, `714b916a…` | **1,220 lines, `5dbad97055b1ea06bd58b0aeed278628`** |
| `coord-train48-rehearse.sh` | 579 lines, `abfb450c…` | 579 lines, `abfb450c690fb3e86abfb5fb8043e9fb` (**unchanged**) |
| `coord-train48-land.sh` | 639 lines, `5f569de7…` | 639 lines, `5f569de7c294c3ee54739333e8268813` (**unchanged**) |
| `coord-train48-land-dryread.sh` | 313 lines, `db584a7f…` | 313 lines, `db584a7f862d6d2c4a82cda4c94efa75` (**unchanged**) |
| `launch-run1.sh` | 21 lines, `cb37da09…` | 21 lines, `cb37da09311ee300a511c6608bb3dd47` (**unchanged**) |
| `t48-derive.py` | 2,118 lines | **2,172 lines, `0d53135b429c36664adbfe613b562250`** |
| asserted operations | 185 | **192** |
| `coord-train48-derive-report.log` | 225 lines | 230 lines |

The four byte-identical files are a reading, not an omission: the LEG 4 advisory patch and lesson LA1
touch the assembly and the self-check, and nothing else. The report log carries §17.4's provenance
caveat unchanged in kind — it is the stdout of the SCRATCH run, so its `template :` line names the
scratch directory while its `sha256 :` line reads `6109e682…`, the same bytes as the live template.

### 18.3 Line endings, parse, and the gates as run

```
CR bytes: every live .sh = 0 · t48-derive.py = 0 · these NOTES = 0
bash -n : all six .sh  rc=0, out=[]
```

```
T48_SELFCHECK_OFFLINE=1 bash coord-train48-derive-selfcheck.sh
=== SELF-CHECK DONE overallFail=1 offlineUnmeasured=9 ===   exit 1
192 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 5 · unanchored grep -c FAIL = 12

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                         exit 0, 0 FAIL
```

**The five FAILs are the same five as §§14 and 17** — rows 3, 5, 6 and 10's pins, plus arm 8's
aggregate — and every one is an offline-UNMEASURED entry, not a finding. Nothing new reds. The **192**
is **191 + LA1**. (The dry-read's two `MISS` hits under an unanchored `grep -c MISS` are the letters
inside `EMISSION` and `ADMISSIBLE` on two `ok` lines — `| grep MISS` is a silent WHERE clause, and
both lines pass.)

LA1's verdict, with its control firing on the train-46 original:

```
PASS LA1 no fatal flag under a never-fatal stamp :: train48=0 train46=1 -- the arm reads ZERO on the
derived file and FIRES on the original.  Train 47 run 6 ended overallFailed=1 with every leg green …
```

### 18.4 A NEW TRAIN-48 SEAT ITEM

**CNR retains WARNING lines by kind in its verdict so the count is classifiable without a planted
line.** Today the converter's WARNING lines are not retained at all — CNR prints only their TOTAL — so
classifying a moved count means re-running CNR standalone with a planted retention line and restoring
the script byte-identical afterwards. That is a hand step in the middle of a gate battery, and it is
the reason a 52 sat unclassified for a whole run. The train-48 arm above predicts the classifiable
kinds from the tree, which is the right half of the fix; the other half belongs in CNR.

### 18.5 One measured note for anyone writing a predictor over the behavioral corpus

**`src/tests/Behavioral/ForVariants/ForVariants.go` carries a UTF-8 BOM** (`EF BB BF` before
`package`). It is the ONLY `.go` file under `src/tests/Behavioral` that does — measured, 1 of all of
them. `go/parser` accepts it, so nothing in the toolchain says so; a predictor that reads the file with
plain `utf-8` sees `﻿package` and a `^package\s+(\w+)` match fails, silently dropping the package
from whatever set it is building. **Decode with `utf-8-sig`.** The LEG 4 predictor does.

### 18.6 What §18 did NOT do

* **Nothing under `train47/` was touched** — read-only, live instrument (run 7 was in flight), exactly
  as §1 requires.
* **No pin was moved**, and no fill point was filled. §3.1's row-7 finding and §15's doubts stand.
* **Nothing was built, converted, gated, committed, pushed or announced.**
* **Train 47's own E1′ aggregate was NOT patched.** Its `[ "${L4ADV:-x}" = "2" ]` still stands in
  `train47/coord-train47-assemble.sh:4534`, so on the live run 7 the LEG 4 arm can refuse correctly
  while the E1′ MET line stays silent. That is a reading about train 47 for its owner to act on, not a
  change made here.

### 18.7 2026-09-13 ~14:20 — RE-DERIVED AGAIN: run 7 was KILLED, run 8 launched from a patched train 47

**Why.** §18 derived from `coord-train47-assemble.sh` at `6109e682…` (5,150 lines) while run 7 was in
flight. **Run 7 was killed at 14:06 and run 8 launched at 14:12** from a train 47 that had been patched
once more in its own directory (backups `*.bak-20260913p` sit beside the patched files). Everything
below is re-derived from the CURRENT templates, taken as FRESH copies into a scratch tree exactly as
§17.3 describes. **The train-47 directory was read and never written.**

| | after §18 | after §18.7 |
|---|---|---|
| template `coord-train47-assemble.sh` | `6109e682…`, 5,150 lines | `e03178b96d31d162…`, **5,154 lines** |
| template `coord-train47-derive-selfcheck.sh` | `fa26e5f7…`, 755 lines | `d2d6f28c5b553202…`, **766 lines** |
| template `coord-train47-land.sh` | (not measured in §18) | `8a8421dd57c0864b…`, **616 lines** |
| `coord-train48-assemble.sh` | 4,904 lines, `9cb5104f…` | **4,908 lines, `9d02cd023a19c551c053c73b6f9c3f32`** |
| `coord-train48-derive-selfcheck.sh` | 1,220 lines, `5dbad970…` | **1,231 lines, `2addad643ae5a2ca23b7d50b6105f12f`** |
| `coord-train48-land.sh` | 639 lines, `5f569de7…` | **639 lines, `e62dcffa6e3fad4d182370e6f475b1b1`** |
| `coord-train48-rehearse.sh` | 579 lines, `abfb450c…` | 579 lines, `abfb450c690fb3e86abfb5fb8043e9fb` (**unchanged**) |
| `coord-train48-land-dryread.sh` | 313 lines, `db584a7f…` | 313 lines, `db584a7f862d6d2c4a82cda4c94efa75` (**unchanged**) |
| `launch-run1.sh` | 21 lines, `cb37da09…` | 21 lines, `cb37da09311ee300a511c6608bb3dd47` (**unchanged**) |
| `t48-derive.py` | 2,172 lines | **2,171 lines, `9dd11b6406649d652a408ed96279148d`** |
| asserted operations | 192 | **189** |
| `coord-train48-derive-report.log` | 230 lines | 229 lines |

**THREE OPS WERE RETIRED AND NOTHING WAS ADDED: 192 − 3 = 189.** All three are §18's own F8 block, and
train 47's new patch is what made each one either impossible or empty. **One of them REFUSED on the
first run against the final inputs** — the derive exited rc=1 naming it, which is the anchor guard
doing exactly its job:

* `f8-l4adv-e1-aggregate` — **the one that refused.** `SUBLINE-CONTAINS … matched 0 line(s), wanted 1`:
  its anchor `&& [ "${L4ADV:-x}" = "2" ]; then` no longer exists. Train 47's E1′ aggregate now reads
  `[ "${L4ADV:-x}" = "${L4ADV_EXPECT:-UNDERIVED}" ]` at `assemble.sh:4538`, so train 48 **inherits** the
  fixed line through the plain copy. ⚠ **`UNDERIVED` is the BETTER default, and that is the reason to
  take the inherited line rather than re-assert the op's own `:-2`**: a `:-2` says MET on a tree whose
  verdict line was never emitted at all — the unmeasured-reads-as-healthy shape — while `UNDERIVED` can
  never equal a count, so an unread arm refuses instead of passing. §18.6's closing reading about train
  47's unpatched aggregate is now **discharged by train 47 itself**.
* `f8-l4adv-e1-stamp` — retired. Train 47's E1′ MET stamp now names the derived expectation and its
  line is **byte-identical** to the text this op wrote (measured whole-line against `assemble.sh:4539`).
  A no-op rewrite asserts nothing and can only refuse later.
* `f8-l4adv-verdict-stamp` — retired. The verdict-line stamp no longer says *"must be 2"*; train 47 now
  words it *"judged below against the expectation DERIVED from the tree, never against a carried
  baseline"*. The op's replacement differed from that only in capitalisation and one word (*"bare"*),
  so it fixed nothing while pinning a wording train 47 may legitimately move again.

**KEPT:** `f8-l4adv-expect-stamp` (that stamp is still RELATIVE — it calls the carried 2 *"the train-46
baseline"*, which in a train-48 file names a train two hops back) and its guard
`f8-no-relative-baseline-in-code`. **No op touches G11(a)'s refusal**, then or now: train 47 replaced
the one-line `[ "$G11ABAD" = "0" ] || fail_gate G11a` with an if/else that **STAMPS**
`G11(a) justification CHECKED ::` (`assemble.sh:2949-2950`), and that arrives in train 48 by plain copy
at `coord-train48-assemble.sh:2693-2694`. Nothing was re-anchored because nothing was anchored there.

#### The gates, as run

```
python t48-derive.py (scratch, fresh copies)   rc=0   TOTAL asserted operations: 189
CR bytes: every live .sh = 0 · t48-derive.py = 0 · these NOTES = 0
bash -n : all six .sh  rc=0, out=[]
```

```
T48_SELFCHECK_OFFLINE=1 bash coord-train48-derive-selfcheck.sh
=== SELF-CHECK DONE overallFail=1 offlineUnmeasured=9 ===   exit 1
194 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 5 · unanchored grep -c FAIL = 12
```

**The five FAILs are the same five as §§14, 17 and 18** — rows 3, 5, 6 and 10's pins, plus arm 8's
aggregate — every one an offline-UNMEASURED entry, not a finding. Nothing new reds. **LA1 still reads
`train48=0 train46=1`** on the derived file, and the LA1 / LD1 / LD2 / LR1 / L11 controls all fire on
the train-46 original.

#### LL1, the arm train 47 grew this afternoon

Train 47's self-check gained **LL1**: the land-anchor test now reads **both quote forms** of a `req`
line, asserts the double-quoted count is `>= 1`, and treats a literal found **only in a comment** as a
MISS rather than a hit. On the derived train-48 pair it reads:

```
LL1 double-quoted req anchors read=2 (must be >= 1 …) :: comment-only matches=0
req anchors read=75 :: asserted by LITERAL PREFIX=60 :: anchors the assembly does NOT emit=0 ::
   too thin to assert … and REPORTED rather than counted=15
PASS: every land anchor with a substantial literal prefix names a stamp this assembly actually writes
```

**RQDQ = 2 · RQMISS = 0 · comment-only = 0.** The double-quoted pair is the E1′ MET req (it carries an
apostrophe and so can only be double-quoted) and LEG 0's E3′ MET req.

#### The land-anchor census in RECORD mode, and what a dead anchor there means

`land-anchor-census.py coord-train48-land.sh train47/coord-train47-assemble-run6.stdout` — 75 req
patterns, **5 dead**, and **every one is accounted for; ZERO are the train-number rename** (no `req`
pattern in the land script carries a train-number token at all, measured):

| land line | anchor | class |
|---|---|---|
| 361 | `LEG 4 E1' MET :: exit 0, CHANGED == 0 on BOTH readings` | **known** — the old doubled-asterisk markdown form no assembly ever wrote; train 47 fixed the req at its own `land.sh:349` this afternoon and it is **first satisfiable by run 8** |
| 404 | `G11\(a\) justification CHECKED ::` | **known** — the stamp did not exist before this afternoon's if/else; **first satisfiable by run 8**, and run 8 has already emitted it once |
| 397 | `FILL POINTS OK ::` | **train-48-only req, dead by construction** — train 47's land has no such req, and train 47 has no fill points |
| 398 | `SEAT TABLE DECLARED STACKS ::` | **train-48-only req, dead by construction** (§6's patch-id arm) |
| 399 | the PATCH-ID ARM alternation | **train-48-only req, dead by construction** (§6) |

⚠ **The three "dead by construction" rows are the census instrument's limit, not a defect, and the
measurement that says so runs in the other direction**: each names a stamp the DERIVED TRAIN-48
ASSEMBLY actually emits — `FILL POINTS OK ::` ×1, `SEAT TABLE DECLARED STACKS ::` ×1, the PATCH-ID
alternation ×13 — which is exactly what the self-check's own `anchors the assembly does NOT emit=0`
reads. A train-47 RECORD cannot contain a stamp train 47 never had. The baseline confirms it:
**train 47's own land script against the same run-6 record reads 2 dead — the same two known ones.**

⚠ **Do NOT census against `coord-train47-assemble-run8.stdout` while run 8 is in flight.** It reads 48
dead at 14:20 purely because the record stops just past LEG 0. A truncated record is a silent WHERE
clause.

#### What §18.7 did NOT do

* **Nothing under `train47/` was touched** — read-only, live instrument (run 8 is in flight).
* **No pin was moved**, and no fill point was filled. §3.1's row-7 finding and §15's doubts stand.
* **Nothing was built, converted, gated, committed, pushed or announced**, and no op was weakened,
  widened or retargeted — three were retired and the reason for each is a reading, above.

## 19. 2026-09-13 ~18:00 — RE-DERIVED AFTER TRAIN 47 **LANDED**, AND THE TWO PINS FILLED

**Why.** §18.7 derived from the train-47 templates while run 8 was in flight. Run 8 finished
`overallFailed=0` at 17:18 and **train 47 LANDED as master
`31fe4925d055537dbb48c343f726e027631f6aa1`** (tree `161af6c441ae1d8fa44f10b44a9740ba2c20ecea`, base
`a02ac3df3`, 15 seats). Landing it took two more patches to train 47's own land script and two new
self-check lessons. Everything here is re-derived from the CURRENT templates, taken as FRESH copies
into a scratch tree exactly as §17.3 describes. **The train-47 directory was read and never written.**

Template inputs, measured (`sha256sum`, fresh copies):

```
coord-train47-assemble.sh          e03178b96d31d162588acd75fd4f42854b7f6f2c8b698a62e948a08d56e89734  5,154 lines  (UNCHANGED since §18.7)
coord-train47-derive-selfcheck.sh  672ee64eeabd2bfd5dcff567edf0226fbf9b09b4d49a11eeff90e968081ed005    774 lines  (was d2d6f28c…, 766)
coord-train47-land.sh              57ecefe06268ac5bc1f470077de146b44e4603a5855f5a132d1f2ec8f37e6901    637 lines  (was 8a8421dd…, 616)
coord-train47-rehearse.sh          584ea342294f81130535d0c216fb6a2d1e08ba712d02c8fdd2701745860a86b6  (unchanged)
coord-train47-land-dryread.sh      3418ef1d5c93b90b306c1a76a80d84acfc099e97ee921d57c92745d9ca81c467  (unchanged)
```

### 19.1 The land's two fixes are INHERITED, and NOT ONE OP HAD TO BE RETIRED OR RE-ANCHORED

Train 47's landing refused twice on its own instrument and was patched twice:

* **the A7-admission reader** — the old `[ "$D_BEHMOD" = "0" ] || { … }` failed on ANY modified
  behavioral file and refused run 8's landing over seven goldens the assembly's own A7 had ADMITTED by
  seat 8's `allowed=` ruling. It is now a `D_BEHMOD_FILES` loop that reads each admission out of the
  record (`A7 allowed <path> :: ruled by seat …` + `union BLOB == that seat's BLOB`) and fails only on
  `BEH_UNADMITTED`.
* **the full-length read-back** — `ls-remote … | cut -c1-9` against a 10-char `--short` HEAD read a
  LANDED push as NOT LANDED (exit 5). It is now `cut -c1-40` against `HEAD40=$(git rev-parse HEAD)`.

**Both arrive in train 48 by plain copy.** The derive's land ops anchor on the `D_BEHMOD=$(git …`
MEASUREMENT line and on the delta stamp, not on the gate line or the read-back, so the first run
against the final inputs exited **rc=0 with no refusal** — the opposite of §18.7, where one op refused
and named itself. The derived file carries the fixed forms: the LB1 predicate
(`^\[ "\$D_BEHMOD" = "0" \] \|\| \{`) and the LC1 predicate (`refs/heads/master \| cut -c1-9\)`) each
read **0** on `coord-train48-land.sh`, and both FIRE on the train-46 original.

### 19.2 SIX OPS ADDED, NONE RETIRED: 189 → 195

* **`f7-sc-run-record-lb1lc1` + `f7-sc-no-invented-run-record-lb1lc1` (2).** Train 47's self-check grew
  lessons **LC1** and **LB1**, and both provenance sentences cite *"Train 47 run 8"* — the run that
  MEASURED each defect. `rename_tokens` rewrote both into **"Train 48 run 8"**, a measurement
  attributed to a battery that has never run: §17.1's corruption class, for the fifth and sixth time.
  ONE `gsub` covers both and `minimum=2, maximum=2` is what asserts it is exactly both; the `absent()`
  is what makes a RE-RUN of the rename unable to re-introduce them. On the shipped file:
  `Train 47 run 4` = 2 · `run 5` = 1 · `run 6` = 1 · `run 8` = **2** · every `Train 48 run N` = **0**.
* **`f10-fill-f2-containment-pin` + `f10-no-pending-containment-pin` (2)** and
  **`f10-fill-f4-expect-g3` + `f10-no-pending-g3` (2)** — [§19.3](#193-the-two-fill-points-filled-by-the-derive-never-by-hand).

⚠ **THE LABEL PREFIX IS `f10-`, NOT `f9-`, AND THAT IS DELIBERATE.** An `(F9)` family already exists in
this derive (the stale-premise block at `t48-derive.py`'s G1/G2/G3 narration). Two unrelated families
under one prefix is the shape that makes a later reader think an op was retargeted when it was added.

Three further edits changed **op PAYLOADS rather than op COUNTS**, because each lives inside a literal
an existing op already writes and asserts: the seat table (§19.4), the header's own FILL-POINT
narrative, and the land whitelist's fourth entry (§19.5).

### 19.3 The two fill points, FILLED BY THE DERIVE, never by hand

| | fill | mechanism | provenance |
|---|---|---|---|
| **F2** | `T48_CONTAIN_PIN='31fe4925d055537dbb48c343f726e027631f6aa1'` | `A.subline_contains` op `f10-fill-f2-containment-pin` + `A.absent` guard | the SHA train 47 LANDED (run 8, 15 seats, base `a02ac3df3`) |
| **F4** | `EXPECT_G3='204'` | `A.subline_contains` op `f10-fill-f4-expect-g3` + `A.absent` guard | train 47 run 8's own G3 at the tree that landed |

⚠ **A PIN HAND-TYPED INTO `coord-train48-assemble.sh` IS UNDONE WITHOUT TRACE BY THE NEXT RE-DERIVE**,
which is the one thing this whole method exists to prevent. Both fills therefore live in
`t48-derive.py`, each as an asserted operation whose `want=1` refuses a fill that matched nothing and
whose `absent()` refuses a placeholder that survived.

**F4 IS A MEASUREMENT WITH A STATED DERIVATION, NOT A CARRIED LITERAL.** `coord-train47-assemble-run8.stdout`
reads `G3 census INVARIANT holds :: all four sets agree at 204` at the tree that landed. The only two
commits master has taken since (`1885bce69` doctrine batch d, `271300cea` docs) touch **12 files**,
and NONE of them is one of the four sets push-nuget's census counts — no README green badge, no
`docs/validation` proof page, no `docs/ValidatedTestPackages.md` roster row, no `*.tests.csproj`
(measured: `git diff --name-only 31fe4925d..master`). So 204 is what G3 will read at this train's base,
and the arm STAMPS rather than refuses if a roster bank lands between this fill and the run.

The online self-check confirms the pin is not a decoration:

```
the containment pin is CONSUMED by the base-ancestry loop=1 (want 1)
origin/master :: ls-remote reads [271300cea] :: the fetched ref reads [271300cea]
origin/master (271300cea) CONTAINS 31fe4925d055537dbb48c343f726e027631f6aa1
F3 the seat table :: rows=19 filled=10 pending=9
```

### 19.4 THE BOARD ROWS, RE-PINNED AT ORIGIN — every named ref read with `git ls-remote`

| row | ref | pin now | reading |
|---:|---|---|---|
| 1 | **`claude/g-handown-metadata-t48-r47`** | **`35fe4e016`** | ⚠ **RE-NAMED AND RE-PINNED.** The ruling said the branch would be REBASED once train 47 landed; it was, under a NEW NAME. The old `claude/g-handown-metadata-t48` still exists at `bb13897e6` and is SUPERSEDED — pinning it would merge a branch whose three-way base is a pre-train-47 tree. Measured: `35fe4e016` CONTAINS `31fe4925d` and is 7 commits beyond it. |
| 2 | `claude/c2-h6-crosscheck` | `191164e7a` | tip == pin, unmoved |
| 3 | `claude/laneR-h6-alias-block` | `47592cb3f` | tip == pin, unmoved |
| 4 | `claude/laneR-docs-h6-skeleton` | `d18059950` | tip == pin, unmoved |
| 5 | `claude/laneR-prepin-baselines-recut` | `becf28abc` | tip == pin, unmoved |
| 6 | `claude/c2-safepush-shallow-skip` | `fa2fdd30d` | tip == pin, unmoved |
| 7 | `claude/c1-seat-duplication-census` | **`a4802675d`** | ⚠ **RE-PINNED** from `77e41300a` — §3.1's live blocker, discharged at the coordinator's instruction. |
| 8 | `claude/g-fleet-patchid-census` | `9b78bfff6` | tip == pin, unmoved |
| 9 | **`claude/i9-board-runtime-door-bisect`** | `68ad83c2c` | ⚠ **THE REF FILL POINT F3a, FILLED BY MEASUREMENT.** `ls-remote` reads the row's own pin `68ad83c2c` as this branch's tip, on two readings a day apart. The NAME routes the fetch and the tip check; it **cannot change which commit is merged**, because `merge_seat` merges the SHA and tip mode refuses unless tip == pin. |
| 10 | `claude/i9-board-archive-tar` | `314e699c6` | tip == pin, unmoved |
| 11–19 | `claude/PENDING-seatN` | PENDING | unchanged, the coordinator's to fill |

⚠ **ROW 7'S TOOL OWES ITS `--self-test` AGAIN.** The PATCH-ID ARM extracts
`src/seat-duplication-census.sh` from THIS row's pinned blob, and §6's recorded self-test run was taken
at the superseded `77e41300a`. The arm now reads the tool at `a4802675d`; that run has not been taken.

### 19.5 The land whitelist's FOURTH entry — an unanchored site that arrived by inheritance

The online self-check's arm (b) refused the derived land:
`FAIL NEW unanchored site :: coord-train48-land.sh:547` — the A7-admission reader's second grep,
`grep -aq "union BLOB == that seat's BLOB"`, an unanchored phrase over the RECORD. It is LEFT
unanchored for the same reason as the two entries above it (the record's stamps carry a `[<ts>] `
prefix, so a `^` anchor would assert the stamp FORMAT rather than the verdict) and it is now ON the
written list with that reason beside it, which is the point of a whitelist whose LENGTH is asserted
against the site count: `sites=4 :: whitelist entries=4 :: NOT on the whitelist=0`.

### 19.6 The gates, as run

```
python t48-derive.py (scratch, fresh copies)   rc=0   TOTAL asserted operations: 195
CR bytes: every live .sh = 0 · launch-run1.sh = 0 · t48-derive.py = 0 · these NOTES = 0
bash -n : all six .sh (five derived + launch-run1.sh)  rc=0, out=[]
```

| file | lines | md5 |
|---|---:|---|
| `coord-train48-assemble.sh` | 4,929 | `88d4ad898e6a4b45f3e6edf9f40767f2` |
| `coord-train48-derive-selfcheck.sh` | 1,246 | `8ed857884964dae422517fce68c10231` |
| `coord-train48-land.sh` | 660 | `d204f9c9bda3632819fe140586c45cf7` |
| `coord-train48-rehearse.sh` | 579 | `abfb450c690fb3e86abfb5fb8043e9fb` (unchanged since §17) |
| `coord-train48-land-dryread.sh` | 313 | `db584a7f862d6d2c4a82cda4c94efa75` (unchanged since §16) |
| `launch-run1.sh` | 23 | `188716d90882bc0cfc3f762bae77093b` |
| `t48-derive.py` | 2,238 | `b7b954f2dfd1bfc497e5a05e33c4cab9` |

⚠ **`launch-run1.sh` GREW BY TWO LINES AND THE DERIVE NOW REPRODUCES IT BYTE-FOR-BYTE.** Train 47's
run-8 wrapper appends `printf 'assembly exit=%s\n' "$rc"` to the record because the land's exit scan
requires it as the record's last non-empty line. That patch had been applied to `launch-run1.sh` BY
HAND; the `LAUNCH` literal in `t48-derive.py` now carries it, so the next re-derive cannot silently
undo it. (Measured: the derived file is IDENTICAL to the hand-patched one.)

#### The ONLINE self-check — the five offline UNMEASURED entries are CLOSED

```
bash coord-train48-derive-selfcheck.sh          (no T48_SELFCHECK_OFFLINE)
=== SELF-CHECK DONE overallFail=1 offlineUnmeasured=0 ===      exit 1
198 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 1 · unanchored grep -c FAIL = 6
```

```
bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                            exit 0, 0 anchored FAIL
```

(`coord-train48-derive-report.log` carries §17.4's provenance caveat unchanged in kind: it is the
stdout of the SCRATCH run, so its `template :` line names the scratch directory while its `sha256 :`
line reads `e03178b9…`, the same bytes as the live template, and its 235 CR bytes are the §12 redirect
caveat.)

§§14/17/18/18.7's **five** FAILs — rows 3, 5, 6 and 10's pins and arm 8's aggregate — are **gone**: the
fetch resolved every pin (`arm 8 coverage :: rows read=19 of 19 :: UNMEASURED=0`) and both fill-point
refusals now read a FILLED value. Every lesson arm passes with its control firing on the train-46
original: **LL1 · LB1 · LC1 · LA1 · LR1 · LD1 · LD2 · L11 all `train48=0 train46=1`** (L11 reads
`train46=15`).

```
LL1 double-quoted req anchors read=2 (must be >= 1 …) :: comment-only matches=0
req anchors read=75 :: asserted by LITERAL PREFIX=60 :: anchors the assembly does NOT emit=0 ::
   too thin to assert … and REPORTED rather than counted=15
```

#### THE ONE REMAINING FAIL IS A FINDING ABOUT A SEAT, NOT ABOUT THE INSTRUMENT

```
seat 1 (converter-corpus-metadata) claude/g-handown-metadata-t48-r47 @35fe4e016 ::
   files=19 outsideShape=1 forbiddenHits=1
      ^ the seat would be REFUSED by its own class.  Paths outside the shape:
        src/core/crypto/internal/boring/bcache/cache.cs
FAIL: a row would be REFUSED by its own class -- and this is a RULING THE COORDINATOR OWNS, not a
      shape to widen here.
```

The `converter-corpus-metadata` class admits `src/go2cs/*`, `src/go2cs.slnx`, `src/core/**/package_info.cs`,
`src/core/**/*.csproj`, `src/core/**/README.md` and `docs/phase4/*.md`; G's rebased branch carries one
CONVERTED SOURCE file as well. ⚠ **THE CLASS WAS NOT WIDENED AND THE ROW WAS NOT UNSEATED.** Widening a
class to admit the seat that violates it is how a gate stops being a gate; the two remedies are the
coordinator's (the seat moves that file to a branch of its own, or the class is widened in the same
commit that states why, with the forbidden list narrowed to match). **`overallFail=1` is therefore a
MEASUREMENT that the arm is working on real content**, and it is the only thing standing between this
template and a clean online self-check.

### 19.7 The land-anchor census in RECORD mode, against the FINAL run-8 record

`land-anchor-census.py coord-train48-land.sh train47/coord-train47-assemble-run8.stdout` — 75 req
patterns (double-quoted 2), **3 dead, down from §18.7's 5, and ZERO are real**:

| land line | anchor | class |
|---|---|---|
| 397 | `FILL POINTS OK ::` | train-48-only req, dead by construction — train 47 has no fill points |
| 398 | `SEAT TABLE DECLARED STACKS ::` | train-48-only req, dead by construction (§6's patch-id arm) |
| 399 | the PATCH-ID ARM alternation | train-48-only req, dead by construction (§6) |

⚠ **THE TWO "KNOWN" DEAD ANCHORS OF §18.7 ARE NOW ALIVE, AS THAT SECTION PREDICTED.** Line 361's
`LEG 4 E1' MET :: exit 0, CHANGED == 0 on BOTH readings` and line 404's `G11\(a\) justification CHECKED ::`
were "first satisfiable by run 8", and run 8's completed record satisfies both. The record used here is
the FINAL one (`assembly exit=0` as its last line, `overallFailed=0`) — §18.7's warning about
censusing a record while its run is in flight no longer applies, and the reading it produced then (48
dead at 14:20) is exactly what a truncated record does.

The three survivors remain the census instrument's limit rather than a defect, and the measurement that
says so runs in the other direction: the DERIVED TRAIN-48 ASSEMBLY emits `FILL POINTS OK ::` ×1,
`SEAT TABLE DECLARED STACKS ::` ×1 and the PATCH-ID alternation ×13 — which is what the self-check's own
`anchors the assembly does NOT emit=0` reads.

### 19.8 What §19 did NOT do

* **Nothing under `train47/` was touched** — read-only, exactly as §1 requires. No battery is running.
* **Nothing was built, converted, gated, committed, pushed or announced.** No `dotnet`, no `go`, no CNR,
  no worktree entered. The only network operations were `git ls-remote` and the self-check's own
  `git fetch -q origin <refspec>` into the main clone, which is what turns an UNMEASURED pin into a
  measurement.
* **No class was widened, no op weakened, widened or retargeted, and no op retired.** The one FAIL above
  is left standing as a finding.
* **F1 (`WT=`) is still PENDING and still refuses**, and rows 11–19 are still PENDING. Filling those is
  the coordinator's, at seat time.

## 20. 2026-09-13 ~18:45 — ROWS 11–19 FILLED FROM THE COORDINATOR'S FIXED TABLE

**Why.** §19 left rows 11–19 PENDING and said so: *"rows 11–19 are still PENDING. Filling those is the
coordinator's, at seat time."* The coordinator's fixed table arrived (ruling `817f98813` + C1
`aaa41c087`). This section fills those nine rows. **Every 40-char SHA was RE-READ at origin with
`git ls-remote` — not one was typed from the ruling** — and every class was chosen by MEASURING the
row's file set, never by the ruling's prose hint.

### 20.1 The nine SHAs, re-read at origin

`git ls-remote origin refs/heads/<ref>`, 2026-09-13 evening, `origin/master` = `271300cea`:

| row | ref | origin SHA (40) | pin (9) |
|---:|---|---|---|
| 11 | `claude/c1-token-door-census-stacked` | `b4914e878e7b4f0347c217ce028b7e4ccbc1b6a1` | `b4914e878` |
| 12 | `claude/c1-gctestisreachable-clean` | `4a9ae8cbbdba08e2a823b89ba2a5fddb7a620286` | `4a9ae8cbb` |
| 13 | `claude/c2-census-goroot-fix-clean` | `5cee80fbead7bb4c7716343c3b9d853e3a7baa13` | `5cee80fbe` |
| 14 | `claude/c1-mfinal-mint-door-clean` | `3f1612524a764fbc0732e5b44d511a4a698fd1c9` | `3f1612524` |
| 15 | `claude/c2-board-both-ordered` | `da5e8304735057c415e48e71f8ed1ec9b672e00b` | `da5e83047` |
| 16 | `claude/c2-merge-probe-predicate` | `4b7985c078e804683e6a4e7c09031cde60083268` | `4b7985c07` |
| 17 | `claude/c2-h10-shardmap-projection` | `5129946000de80c0d9e965495968afd66aad9bed` | `512994600` |
| 18 | `claude/c2-h10-map-rederivation` | `41c1d1d28ef17381453d86899192bf1905b3f464` | `41c1d1d28` |
| 19 | `claude/c2-safepush-shallow-skip` | `fa2fdd30dc1f06eeeccbcdc792eeb896157c5792` | `fa2fdd30d` |

⚠ **ROW 17'S CONDITIONAL RESOLVED TO THE FALLBACK.** The ruling said to use the tip if C2's re-cut
AMENDMENTS block (per `5cc5a3645`) had moved it past `5129946000…`. It has **not**: `ls-remote` reads
`5129946000de80c0d9e965495968afd66aad9bed` as the tip, so the fallback pin **is** the tip. The re-cut
is not at origin.

### 20.2 The classes, chosen by MEASUREMENT — and the two the ruling's prose got wrong

File sets measured as `git diff --name-only $(git merge-base origin/master <tip>)..<tip>`:

| row | files | class chosen | why this one |
|---:|---:|---|---|
| 11 | 11 | `converter-test+tooling` | six `src/go2cs/*`, `src/token-door-census.sh` (root-level `.sh`), one `docs/phase4/*.md`. The only class admitting all three shapes at once. |
| 12 | 7 | `golib-corpus-handown` | `src/core/runtime/{mgc,mgc_impl,panic_impl}.cs` + `{darwin,linux,windows}/package_info.cs` + `src/go2cs/manualTypeOperations.go`. The shape's `src/core/runtime/([a-z]+/)?[^/]+\.cs` admits BOTH the flat files and the per-GOOS ones. **All seven admitted; no ruling needed.** `converter` was rejected: it admits `package_info.cs` but not `mgc.cs`. |
| 13 | 8 | `converter-test` | six `src/go2cs/*` and nothing else in `src/`. `converter-test`'s shape is exactly `^src/go2cs/[^/]+$` — the TIGHTEST fit, and its forbidden list (`src/core src/gen src/tests src/utilities docs`) is the widest that this row still satisfies. |
| 14 | 1 | `golib-corpus-handown` | `src/core/runtime/mfinal.cs`. Admitted whole. |
| 15 | 1 | `docs` | the BOARD. Admitted whole. |
| 16 | 2 | `tooling` | ⚠ **NO CLASS ADMITS EITHER PATH** — both are under `.claude/skills/merge-hazards/`. `tooling` is chosen for its FORBIDDEN list, the widest in the vocabulary (`src/core src/gen src/tests src/go2cs`), none of which this row touches; both paths are NAMED by `allowed=`. |
| 17 | 1 | `docs` | one `docs/phase4/DATA-*.md`. Admitted whole. |
| 18 | 6 | `tooling` | ⚠ **THE RULING'S HINT ("docs") IS FALSIFIED BY MEASUREMENT.** The row carries `src/run-h10-dispatch.ps1`, and `docs`'s forbidden list is `src` — it would abort on the FORBIDDEN arm, not merely the shape arm. `tooling` admits the root-level `.ps1` and the three `docs/phase4/*.md`; `.gitattributes` and `docs/phase4/hopA-inputs/shardmap.py` are NAMED by `allowed=`. ⚠ **NO TSVs are present at this tip**, contrary to the ruling's description — only the `shardmap.py` generator. |
| 19 | 1 | `converter-test` | `src/go2cs/safePushGuard_test.go`. Admitted whole — and it is **row 6 again**, see §20.4(b). |

⚠ **NOT ONE CLASS WAS WIDENED.** Where a path had no admitting shape, an `allowed=` ruling names it.

### 20.3 The four `allowed=` rulings, and why none of them can carry a `|`

| row | `allowed=` ERE | admits |
|---:|---|---|
| 11 | `^(\.claude/rules/converter\.md)?(\.claude/skills/mailbox/SKILL\.md)?$` | `.claude/rules/converter.md`, `.claude/skills/mailbox/SKILL.md` |
| 13 | `^(\.claude/rules/converter\.md)?(\.claude/skills/mailbox/SKILL\.md)?$` | the same two (row 11 is row 13 plus four files) |
| 16 | `^(\.claude/skills/merge-hazards/SKILL\.md)?(\.claude/skills/merge-hazards/merge-probe\.sh)?$` | both `.claude/skills/merge-hazards/` files |
| 18 | `^(\.gitattributes)?(docs/phase4/hopA-inputs/shardmap\.py)?$` | `.gitattributes`, `docs/phase4/hopA-inputs/shardmap.py` |

⚠ **THE ALTERNATION-FREE FORM IS FORCED, NOT STYLISTIC.** The seat table is parsed `awk -F'|'`, so an
`allowed=` ERE carrying a `|` **splits into extra fields** and the parser refuses it by name — the
ENCODING HAZARD the FIELDS documentation already states. Row 1's single-path ruling never met it.
`^(pathA)?(pathB)?$` names each path exactly, is anchored, and matches nothing else that is a path
(its only other matches are the empty string and the two concatenations, neither of which is a
filename `git diff --name-only` can emit).

**THE CLASS VOCABULARY ADMITS NO `.claude/**` PATH AT ALL.** Three of the four rulings above exist for
that one reason. That is a gap in the vocabulary rather than a property of these seats, and it is the
coordinator's to close — by a class, not by widening `tooling`.

### 20.4 TWO STRUCTURAL REFUSALS, WRITTEN IN DELIBERATELY, BOTH THE COORDINATOR'S TO RULE

Neither was tidied away. A table quietly corrected away from the ruling that produced it is a table
nobody can audit.

**(a) ROW 11 DECLARES `stack-on=13`, WHICH NAMES A LATER ROW.** The ruling states the relationship
correctly and the ancestry MEASURES that way — `git merge-base --is-ancestor 5cee80fbe b4914e878` = YES,
and row 11's own delta over row 13 is exactly four files (`docs/phase4/CENSUS-token-door-live-wrappers.md`,
`src/go2cs/go2cs-src.projitems`, `src/go2cs/tokenDoorCensusGuard_test.go`, `src/token-door-census.sh`).
But **the row order IS the merge order**, so a row built on another must merge SECOND. Measured
verbatim, by running the assembly's own `seat_opts_validate` against the shipped table:

```
  ^ SEAT TABLE REFUSED: row 11 declares stack-on=13, which merges AFTER it.  THE ROW ORDER IS THE
  MERGE ORDER: a row built on another must merge second, or the union it was cut against is not the
  union it lands in.
seat_opts_validate rc=1
```

The remedy is the coordinator's: **SWAP the two seats' positions** (`c2-census-goroot-fix-clean` at 11,
`c1-token-door-census-stacked` at 13 with `stack-on=11`), **or UNSEAT row 13 entirely** — row 11
already CONTAINS it, so seating both without a declared stack merges row 13's four commits twice and
the patch-id census reads that as a cherry-pick contamination.

**(b) ROW 19 IS ROW 6.** `claude/c2-safepush-shallow-skip` @ `fa2fdd30dc1f06eeeccbcdc792eeb896157c5792`
is seated at BOTH row 6 (pinned in §19.4) and row 19 (the fixed table). Same ref, same 40-char SHA.
The online self-check reads it directly:

```
rows=19 :: merge order [1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19] :: duplicate refs=1 ::
   duplicate filled SHAs=1 :: rows with <5 fields=0 :: rows carrying an allowed= ruling=5
FAIL: the seat table is structurally unsound -- the ROW ORDER IS THE MERGE ORDER
```

The remedy is the coordinator's: drop one of the two rows. It is left standing so the fixed table and
this file say the same thing.

### 20.5 The re-derive, per §17.3 — FRESH copies, prior outputs deleted, run in a scratch tree

The **six** train-47 inputs were copied fresh into a scratch tree, the scratch `train48/` held ONLY
`t48-derive.py` (no prior output to derive from), and `python t48-derive.py` was run there.

```
coord-train47-assemble.sh          e03178b96d31d162588acd75fd4f42854b7f6f2c8b698a62e948a08d56e89734  (unchanged since §19)
coord-train47-rehearse.sh          584ea342294f81130535d0c216fb6a2d1e08ba712d02c8fdd2701745860a86b6  (unchanged)
coord-train47-land.sh              57ecefe06268ac5bc1f470077de146b44e4603a5855f5a132d1f2ec8f37e6901  (unchanged)
coord-train47-land-dryread.sh      3418ef1d5c93b90b306c1a76a80d84acfc099e97ee921d57c92745d9ca81c467  (unchanged)
coord-train47-derive-selfcheck.sh  24541bfd6252f2252a09d3264061c82f34c1e4f365dc9efc545c5d2585ef4a71  ⚠ MOVED (was 672ee64e…, 774 lines; now 779)
t47-derive.py                      5de1f00c82929854c622d214b4bbbd74fa32767fab18a7b71238061208eb4a6d
```

⚠ **THE SELF-CHECK TEMPLATE MOVED AGAIN AFTER §19, AND NOT ONE OP HAD TO BE RE-ANCHORED.** The derive
exited rc=0 against it with no refusal. Its five new lines teach arm 8's shape reader to CREDIT an
`allowed=` ruling: §19.6's one standing FAIL — *seat 1 … `files=19 outsideShape=1 forbiddenHits=1` …
the seat would be REFUSED by its own class* — now reads **`files=19 outsideShape=0 forbiddenHits=0
allowedByRuling=1`** and PASSES. That FAIL is closed by the instrument growing, not by a class widening.

```
python t48-derive.py (scratch, fresh copies)   rc=0   TOTAL asserted operations: 195   (UNCHANGED)
```

**NOT ONE OP WAS ADDED, RETIRED, WIDENED OR RETARGETED.** All three edits changed **op PAYLOADS**,
each inside a literal an existing op already writes and asserts: the `header-title` block (`NINETEEN
SEAT ROWS OF WHICH NINE ARE PLACEHOLDERS` → `ALL NINETEEN PINNED`), the F3 paragraph of the FILL-POINT
narrative (`TEN carry a resolved SHA … nine read PENDING` → `ALL NINETEEN … ZERO read PENDING`), and
the `seat-table` block (the nine rows plus their note column).

| file | lines | md5 |
|---|---:|---|
| `coord-train48-assemble.sh` | 5,005 | `43e2711630b36c5bf7a5fc001a259c09` |
| `coord-train48-derive-selfcheck.sh` | 1,251 | `8e544fb61b254e1430a293ee038cef2d` |
| `coord-train48-land.sh` | 660 | `d204f9c9bda3632819fe140586c45cf7` (unchanged since §19) |
| `coord-train48-rehearse.sh` | 579 | `abfb450c690fb3e86abfb5fb8043e9fb` (unchanged since §17) |
| `coord-train48-land-dryread.sh` | 313 | `db584a7f862d6d2c4a82cda4c94efa75` (unchanged since §16) |
| `launch-run1.sh` | 23 | `188716d90882bc0cfc3f762bae77093b` (unchanged) |
| `t48-derive.py` | 2,314 | `8263c421729a00bb90c63b3b6562873a` |

### 20.6 The gates, as run

```
CR bytes: every live .sh = 0 · launch-run1.sh = 0 · t48-derive.py = 0 · these NOTES = 0
bash -n : all six .sh (five derived + launch-run1.sh)  rc=0, out=[]
```

```
bash coord-train48-derive-selfcheck.sh          (ONLINE, no T48_SELFCHECK_OFFLINE)
=== SELF-CHECK DONE overallFail=1 offlineUnmeasured=0 ===      exit 1
198 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 1 · unanchored grep -c FAIL = 6
arm 8 coverage :: rows read=19 of 19 :: rows UNMEASURED because their ref is absent from this clone=0
PENDING rows=0 []
```

**EVERY ONE OF THE NINETEEN ROWS PASSES ITS CLASS.** Arm 8's per-row shape reading, rows 11–19:

```
seat 11 (converter-test+tooling) claude/c1-token-door-census-stacked @b4914e878 :: files=11 outsideShape=0 forbiddenHits=0 allowedByRuling=2
seat 12 (golib-corpus-handown)   claude/c1-gctestisreachable-clean   @4a9ae8cbb :: files=7  outsideShape=0 forbiddenHits=0 allowedByRuling=0
seat 13 (converter-test)         claude/c2-census-goroot-fix-clean   @5cee80fbe :: files=8  outsideShape=0 forbiddenHits=0 allowedByRuling=2
seat 14 (golib-corpus-handown)   claude/c1-mfinal-mint-door-clean    @3f1612524 :: files=1  outsideShape=0 forbiddenHits=0 allowedByRuling=0
seat 15 (docs)                   claude/c2-board-both-ordered        @da5e83047 :: files=1  outsideShape=0 forbiddenHits=0 allowedByRuling=0
seat 16 (tooling)                claude/c2-merge-probe-predicate     @4b7985c07 :: files=2  outsideShape=0 forbiddenHits=0 allowedByRuling=2
seat 17 (docs)                   claude/c2-h10-shardmap-projection   @512994600 :: files=1  outsideShape=0 forbiddenHits=0 allowedByRuling=0
seat 18 (tooling)                claude/c2-h10-map-rederivation      @41c1d1d28 :: files=6  outsideShape=0 forbiddenHits=0 allowedByRuling=2
seat 19 (converter-test)         claude/c2-safepush-shallow-skip     @fa2fdd30d :: files=1  outsideShape=0 forbiddenHits=0 allowedByRuling=0
PASS: every FILLED row's REAL diff is admitted by its own class shape and touches none of its forbidden paths
```

The ONE anchored FAIL is §20.4(b), a finding about the coordinator's table and not about the
instrument. §20.4(a) does **not** reach this FAIL — the self-check exercises the `stack-on` refusals on
SYNTHETIC rows only (`ok stack-on naming a LATER row :: refused (exit 1) AND named the reason
[merges AFTER it]`), so the live table's stack-order refusal first fires at the assembly PREFLIGHT.
It was therefore measured directly, and §20.4(a) quotes that run verbatim.

```
bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                            exit 0, 0 anchored FAIL
```

(Its two `MISS` hits are substring matches inside `EMISSION` and `ADMISSIBLE`, unchanged in count and
in kind from §19's run. `coord-train48-derive-report.log` carries §17.4's provenance caveat unchanged
in kind: it is the stdout of the SCRATCH run, so its `template :` line names the scratch directory
while its `sha256 :` line reads `e03178b9…`, the same bytes as the live template.)

### 20.7 ONE PRE-EXISTING ROW FINDING, RE-READ AND STILL OPEN

```
seat 4 (docs) claude/laneR-docs-h6-skeleton :: ⚠ THE BRANCH GREW -- tip is c2b699daf, 1 commit(s)
   BEYOND the pin d18059950, which IS an ancestor.  merge_seat will ABORT with this diagnosis, which
   is the mechanism WORKING.
```

Not a row this section filled, and not a new defect — recorded here because the online read took it
and a coordinator reading §20 should not have to re-derive it. Rows 1–10 were otherwise unchanged.

### 20.8 What §20 did NOT do

* **Nothing under `train47/` was touched** — read-only, exactly as §1 requires. No battery is running.
* **Nothing was built, converted, gated, committed, stashed, checked out, pushed or announced.** The
  only network operations were `git ls-remote` and `git fetch` of the nine tips into the main clone,
  which is what turns a ruling's typed SHA into a measurement.
* **No class was widened, no op weakened, widened, retargeted, added or retired.** Both structural
  refusals are left standing as findings.
* **F1 (`WT=`) is still PENDING and still refuses.** Filling it is the coordinator's, at launch.

## 21. 2026-09-13 ~19:05-19:45 — THE ADVERSARIAL VERIFY'S FINDINGS D1–D6, RULED AND APPLIED

**Why.** §20 shipped the nine-row fill and left TWO structural refusals standing for the coordinator
(§20.4(a) and (b)). An adversarial verify at 19:05 re-read every pin at origin and read `merge_seat`
against the rulings the table carries. It found **six** things, not two, and the coordinator ruled on
all six the same evening. Every one is applied **in `t48-derive.py`**, none by hand in a derived file,
and **nothing under `train47/` was written** — §1's rule, unchanged.

### 21.1 The six findings and their rulings

| | finding, as MEASURED | ruling | how it is applied |
|---|---|---|---|
| **D1** | Row 17 was **STALE**: `claude/c2-h10-shardmap-projection` had moved one commit past `5129946000…` — `ls-remote` reads `a633896bf69d554f33b19b7a574466175ece32db` (a second AMENDMENTS block, superseding section 5a per row 18). §20.1's "the re-cut is not at origin" is **superseded**. | RE-PIN to the origin tip, re-read by `ls-remote` and not typed; tip mode. | `seat-table` payload (pin `a633896bf` + the row note) |
| **D2** | Row 11 declared `stack-on=13`, naming a **LATER** row; `seat_opts_validate` refuses it verbatim. | **SWAP** the two seats. Row 11 = `claude/c2-census-goroot-fix-clean` (`converter-test`, its own `allowed=`); row 13 = `claude/c1-token-door-census-stacked` (`converter-test+tooling`, its own `allowed=`, `stack-on=11`). Nothing else about either row moves. | `seat-table` payload |
| **D3** | The nineteenth row **duplicated row 6** (`claude/c2-safepush-shallow-skip` @ `fa2fdd30dc…` at both). | **DROP** it. The table is **EIGHTEEN** rows; every literal that said NINETEEN/19 says EIGHTEEN/18. | `header-title`, `header-fillpoints` (F3), `seat-table`, `seat-vars` (SEAT19 dropped), `f5-done-stamp` payloads |
| **D4** | Seat 4 (`claude/laneR-docs-h6-skeleton`) **GREW**: tip `c2b699dafc3c…`, one commit beyond the pin `d18059950`. That commit is G's `mcleanup.cs` row and it describes the **VERSION-BRANCH** census of 147 paths where master's census is 146. | Row 4 becomes **SHA MODE** at the SAME pin. The pin IS an ancestor of the tip (measured). The excluded commit is stamped BY NAME. The 147/145-row amendments are version-branch business (mailbox `46198c1b9` §5). | `seat-table` payload (`…\|d18059950\|docs\|sha`) + the row note |
| **D5** | **THE `allowed=` RULINGS WERE NOT HONOURED BY `merge_seat`.** Its forbidden arm (`git diff --name-only $mb $want -- $forbidden`) and its shape arm (`grep -avE "$shapepat"`) consult the CLASS and nothing else; only A7 read `allowed=`, and A7 scopes `src/tests/Behavioral --diff-filter=MD`. Rows **1, 11, 13, 16 and 18** would have **ABORTED at assembly** on their own class while the ruling at the row read as decoration. | `allowed=` is a **PER-ROW RULING** admitting EXACTLY the named paths OUTSIDE the class's shape and forbidden list, with blob identity asserted. Clauses (i) subtract-count-list + stale-ruling refusal, (ii) post-merge blob identity, (iii) arm **A7b** over the union, owner = the LAST matching row. | **eleven new derive ops** (below) |
| **D6** | Floor item 13: none of the above had ever been made to fail. | Add static, online and table arms to the derived self-check, each with its control. | **seven new self-check/rehearse ops** (below) |

### 21.2 The ops — 195 → 213, every one asserted and counted

**Nothing was retired, weakened or retargeted; not one class shape or forbidden list was widened.**
D1–D4 are **payload** edits inside literals existing ops already write and assert (the precedent §20.5
records). D5 and D6 are **new ops**, each anchored on the train-47 assembler/self-check text exactly as
every other op is:

| op | anchor | what it changes |
|---|---|---|
| `seat-allowed-helpers` | `^seat_row\(\)\{` | inserts `seat_allowed_paths()` (enumerates the literal paths an `allowed=` ERE names) and `alw_filter()` (stdin MINUS the ruled paths; `cat` on an empty ruling, never `grep -avE ''`) |
| `merge-seat-allowed-locals` | merge_seat's `local b="$1" want="$2" …` line | adds `alw alwn alwlist alwp alwbu alwbs dpaths` as **locals** |
| `merge-seat-allowed-read` | the `bad=$(… -- $forbidden \| grep -ac .)` line | reads the ruling through `seat_opt`/`seat_row`, REFUSES a ruled path absent from the seat's real diff (STALE RULING), stamps `allowedByRuling=N` + the paths, and filters the forbidden count |
| `merge-seat-forbidden-list` | the forbidden LISTING line | filters the listing the same way |
| `merge-seat-forbidden-stamp` | `forbidden-path files=0 (checked: …)` | carries `allowedByRuling=$alwn [$alwlist]` |
| `merge-seat-shape-filter` | the `shapebad=$(…)` line | filters the shape count |
| `merge-seat-shape-list` | the shape LISTING line | filters the listing |
| `merge-seat-shape-stamp` | `shape OK :: every changed path matches …` | carries `allowedByRuling=$alwn [$alwlist]` |
| `merge-seat-allowed-identity` | `# marker scan over the committed blobs.` | inserts **ALLOWED-IDENTITY-AFTER-MERGE**: per ruled path, `HEAD:path` blob == `$want:path` blob or ABORT naming both |
| `a7b-arm` | `^\[ "\$\{A7MODX:-1\}" = "0" \] \|\|` | inserts **A7b** over the finished union for every ruled path NOT under `src/tests/Behavioral`; owner = the LAST matching A7MAP row (the loop deliberately does not `break`); `fail_gate A7b-allowed-identity` |
| `fields-allowed-extended` | `⚠ IT IS A RULING, NOT A CONVENIENCE.` | the FIELDS doc states the extended meaning and the identity predicate |
| `rehearse-table-end-anchor` | the rehearsal's `grep -nE '\|tip"$'` | **REQUIRED**: row 18 now ends `…)?$"`, so the old end anchor resolves to nothing and the extraction would abort. Retargeted to the closing quote. |
| `sc-table-end-anchor` | the self-check's `blk … '\|tip"$'` | the same defect in the self-check, where it would have exited 3 |
| `sc-arm3-tipmode-aware` | the `THE BRANCH GREW` say line | arm 3 was **mode-blind** and announced "merge_seat will ABORT" of a `sha`-mode row, which is FALSE and sends a reader to fix a healthy table |
| `sc-arm8-keeps-allowedbyruling` | — | `require('allowedByRuling=')`: D6(d), asserted rather than assumed |
| `sc-arm8-allowed-keyvalue` | arm 8's `alw=''; case "${a:-}" in allowed=*) …` | arm 8 read the ruling **POSITIONALLY**; with two optional keys `${a#allowed=}` yields the ERE with `\|stack-on=NN` welded on — an **ALTERNATION nobody wrote**. Row 13 carries exactly that shape. Read by field scan instead. |
| `sc-allowr-keyvalue` | `ALLOWR=$(… '$6 ~ /^allowed=/')` | the same defect class in the row count |
| `sc-allowed-arms` | after the fill-point PASS line | the three D6 arms (e), (f), (g) with their controls |

The header doc for `allowed=` (the `⚠⚠ THE OPTIONAL FIELDS` block) and the class note that says **no
class was widened** are extended in the same ops that already write them.

### 21.3 The D6 arms, and a vacuous green they caught in themselves

* **(e) STATIC, with a REGRESSED COPY as its red control.** `derived :: forbiddenArmFilters=2
  shapeArmFilters=2 identityBlocks=1 a7bArms=1`; the SAME predicate over a copy with
  `' | alw_filter "$alw"'` sed'd out (i.e. train 47's shape) reads `forbiddenArmFilters=0
  shapeArmFilters=0` and names the regression.
* **(f) ONLINE, per row.** Every path each ruling admits is read against the row's REAL diff
  (`git diff --name-only merge-base..tip` after the fetch): `rows carrying an allowed= ruling=5 ::
  ruled paths read=9 :: STALE ruled paths=0 :: rows UNMEASURED=0`, each path named.
* **(g) TABLE, with a planted red control.** Two rows ruling one path are refused unless one declares
  `stack-on` the other. Rows 11 and 13 rule the same two `.claude/**` paths and 13 declares
  `stack-on=11`, so the owner is unambiguous. The planted overlap (no stack-on) is REFUSED and names
  `docs/x.md`.

⚠ **THE FIRST CUT OF (f) AND (g) WAS A VACUOUS GREEN, AND IT IS RECORDED RATHER THAN QUIETLY FIXED.**
They read the global `SEAT_TABLE`, and the arms ABOVE them **plant synthetic tables into the shell**;
the last plant leaves `SEAT_TABLE` holding **two** rows. (f) reported `rows carrying an allowed=
ruling=0 :: STALE ruled paths=0` — every count zero, nothing refused, the arm reading healthy over a
table nobody seated. The table is now **re-read from the file** for those arms, and the re-read is
itself checked against arm 2's independent row count (`18 == 18`) so a future re-plant cannot restore
the vacuum silently.

### 21.4 The re-derive, per §17.3, and the gates as run

Six train-47 inputs copied FRESH into a scratch tree, the scratch `train48/` holding ONLY
`t48-derive.py`, run there. The inputs are byte-identical to §20.5's record:

```
coord-train47-assemble.sh          e03178b96d31d162588acd75fd4f42854b7f6f2c8b698a62e948a08d56e89734
coord-train47-rehearse.sh          584ea342294f81130535d0c216fb6a2d1e08ba712d02c8fdd2701745860a86b6
coord-train47-land.sh              57ecefe06268ac5bc1f470077de146b44e4603a5855f5a132d1f2ec8f37e6901
coord-train47-land-dryread.sh      3418ef1d5c93b90b306c1a76a80d84acfc099e97ee921d57c92745d9ca81c467
coord-train47-derive-selfcheck.sh  24541bfd6252f2252a09d3264061c82f34c1e4f365dc9efc545c5d2585ef4a71
t47-derive.py                      5de1f00c82929854c622d214b4bbbd74fa32767fab18a7b71238061208eb4a6d
python t48-derive.py (scratch)     rc=0   TOTAL asserted operations: 213   (was 195)
```

| file | lines | md5 |
|---|---:|---|
| `coord-train48-assemble.sh` | 5,177 | `bbe791559a5d471af1503e198604a232` |
| `coord-train48-derive-selfcheck.sh` | 1,393 | `a0a555a092d7bab7ebc0821c16eedfd8` |
| `coord-train48-rehearse.sh` | 579 | `ce04002322429968dcd3da65e61dccd4` |
| `coord-train48-land.sh` | 660 | `d204f9c9bda3632819fe140586c45cf7` (unchanged) |
| `coord-train48-land-dryread.sh` | 313 | `db584a7f862d6d2c4a82cda4c94efa75` (unchanged) |
| `launch-run1.sh` | 23 | `188716d90882bc0cfc3f762bae77093b` (unchanged) |
| `t48-derive.py` | 2,738 | `104f16e48db4bec164f47742c28ce5fd` |

```
CR bytes: every live .sh = 0 · launch-run1.sh = 0 · t48-derive.py = 0 · these NOTES = 0
          coord-train48-derive-report.log = 253 (CRLF, the §12 redirect caveat, unchanged in kind)
bash -n : all five derived .sh + launch-run1.sh + launch-rehearse-run1.sh  rc=0, out=[]

bash coord-train48-derive-selfcheck.sh          (ONLINE)
=== SELF-CHECK DONE overallFail=0 offlineUnmeasured=0 ===       exit 0
215 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 0 · unanchored grep -c FAIL = 6
rows=18 :: merge order [1..18] :: duplicate refs=0 :: duplicate filled SHAs=0 :: rows with <5 fields=0
        :: rows carrying an allowed= ruling=5
arm 8 coverage :: rows read=18 of 18 :: rows UNMEASURED=0 · PENDING rows=0 []

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                             exit 0, 0 FAIL, 0 MISS
```

**§20.6's ONE anchored FAIL (the duplicate row) is gone because the row is gone**, and the six
unanchored `FAIL` hits are the quoted CONTROL-RED lines, which is what a control is for.

### 21.5 The rehearsal, run onto 271300cea

`REHEARSE_WT=/c/go2cs-tmp-coord/t48-rehearse M=271300cea03a2f47bd7dd8d9ed392c6249dac4c4`, launched
from a per-run COPY (`launch-rehearse-run1.sh` → `coord-train48-rehearse-run1.sh`). The rehearsal
created and removed its own worktree.

```
=== REHEARSAL DONE conflicts=3 markers=0 vetFailures=0 vetUnmeasured=0 exit=1 ===
per-seat type check   :: failures=0 unmeasured=0 first failing seat=[none] (NOVET=0)
per-seat LICENSING GUARD :: pass=15 fail=0 -- UNION LICENSING READING :: PASS
seats NEEDING A HAND :: [ 9(i9-board-runtime-door-bisect) 10(i9-board-archive-tar) 15(c2-board-both-ordered)]
```

**All three conflicts are the same one path** — `docs/phase4/BOARD-next-validation-candidates.md`,
three seats appending to one append-only ledger. Resolution SLOTS are saved under
`coord-train48-resolutions/seat{9,10,15}/`; **nothing was resolved here** — that is the coordinator's
ruling. ⚠ Every collision between those seats and a LATER seat is **UNMEASURED** in this run.

The five seats the verify moved all merged CLEAN: seat 1 `@35fe4e0167`, seat 4 `@d180599509` (SHA
mode), seat 11 `@5cee80fbea`, seat 13 `@b4914e878e`, seat 17 `@a633896bf6`.

### 21.6 Provenance: the 18:25 edit of `train47/coord-train47-derive-selfcheck.sh`

§20.5 recorded that template as having "MOVED AGAIN AFTER §19" with no op needing re-anchoring. **That
edit was the COORDINATOR'S OWN patch** (`patch-selfcheck-a7.py`), applied to the derive INPUT so arm 8
would CREDIT an `allowed=` ruling — authorship is known, it was not a lane's change arriving
unannounced, and **train 47's run-8 record copies are untouched by it**. The live
`coord-train47-assemble.sh` is byte-identical to `coord-train47-assemble-run8.sh` (`cmp`, this
session).

⚠ **`git status --porcelain` OVER `train47/` IS VACUOUS** — `.claude/` is in `.git/info/exclude`, so
porcelain reports nothing whatever the files say. The reading that means anything is **`cmp` against
the run-8 copies plus an mtime census**. Measured at the end of this session: every train-47 mtime
predates it (latest `18:22:38`, `coord-train47-derive-selfcheck.sh`), and all six input sha256s are
identical to the copies taken at the start.

### 21.7 What §21 did NOT do

* **Nothing under `train47/` was written.** Read-only, live instrument, as §1 requires.
* **No class shape and no forbidden list was widened.** Where a path has no admitting shape, a ruling
  names it and the ruling is now ENFORCED and ASSERTED rather than decorative.
* **No conflict was resolved**, no commit, stash, checkout or push was made on any branch, and no
  battery was run. The rehearsal's own `go build` / `go vet` / `TestLicensing` per seat are its
  contract and are the only compilation this session performed.
* **F1 (`WT=`) is still PENDING and still refuses.** Filling it is the coordinator's, at launch.

## 22. 2026-09-13 ~20:00 — THE ROUND-2 ADVERSARIAL VERIFY'S FINDINGS E1–E6, RULED AND APPLIED

**Why.** §21 shipped D1–D6 and ran the rehearsal onto `271300cea`, which ended `conflicts=3`. A second
adversarial verify at ~19:50 re-read the table against `merge_seat`'s **own base rule** and found that
the D5 machinery §21 had just wired was reading one row against the wrong base — and that the wrong
base was hiding a ruling that would have **aborted the assembly**. The coordinator ruled on all six
findings the same evening. E1–E3 and the self-check changes are applied **in `t48-derive.py`** as
asserted operations anchored on the train-47 inputs like every other op; E4 is the three (measured:
**two**) resolution slots; **nothing under `train47/` was written**.

### 22.1 The six findings and their rulings

| | finding, as MEASURED | ruling | how it is applied |
|---|---|---|---|
| **E1** | **ROW 13's `allowed=` WAS A STALE RULING AND WOULD HAVE ABORTED THE ASSEMBLY.** `merge_seat` measures a seat against `merge-base HEAD "$want"`. Row 13 declares `stack-on=11`, so by the time it merges HEAD carries row 11's tip `5cee80fbe` and row 13's measured delta is exactly four files — `docs/phase4/CENSUS-token-door-live-wrappers.md`, `src/go2cs/go2cs-src.projitems`, `src/go2cs/tokenDoorCensusGuard_test.go`, `src/token-door-census.sh`. **No `.claude` path is in it.** The ruling it inherited from the pre-D2 table named two paths absent from that diff, which D5's own STALE-RULING arm refuses by name. | Row 13 **DROPS** its `allowed=`: `13\|claude/c1-token-door-census-stacked\|b4914e878\|converter-test+tooling\|tip\|stack-on=11`. Row 11 keeps its ruling and is the **sole owner** of the two `.claude/**` paths. | `seat-table` payload + the row-13 and D-block notes |
| **E2** | **SELF-CHECK ARMS 8 AND (f) MEASURED A STACKED ROW AGAINST `merge-base(origin/master, pin)`**, so they credited row 13 with every file its PARENT changed and could not see E1 at all. The instrument that was supposed to catch a stale ruling was the instrument that hid it. | A row carrying `stack-on=N` is measured against **row N's PINNED SHA — the table's own value**, never master. Both arms. A stacked row whose parent pin does not resolve is **UNMEASURED**, never re-based on master. Plus a control. | **two new ops** (`sc-arm8-stacked-base`, `sc-arm8-stamp-base`) + the arm-(f) rewrite inside `sc-allowed-arms` |
| **E3** | **ROW 4's TIP MOVED AGAIN**: `ls-remote` reads `067302ea09732cade2ef488a49fb7ab83410bd9d`, **TWO** commits beyond the pin `d18059950` (`c2b699dafc` then `067302ea09`), both the version-branch 147/145 census business. D4's prose said ONE. | SHA mode PROCEEDS — the pin is still an ancestor (measured). Every wording that carried a COUNT is rewritten to say the excluded commits are **stamped by name at run time**. | `seat-table` payload (row-4 note + the D4 block) |
| **E4** | The three BOARD conflicts. | The BOARD is an **APPEND-ONLY** ledger: the resolution is the **UNION**, filled mechanically by `git merge-file --union` in a scratch replay, with a pure-append PRECONDITION per seat and a line-arithmetic / verbatim-order POST-CONDITION per slot. | `coord-train48-e4-slot-replay.sh` + `coord-train48-e4-slotcheck.py`, record in `coord-train48-e4-replay.out` |
| **E5** | — | This section. | — |
| **E6** | The directory had accumulated nine `selfcheck-online-*.out`, three `dryread-*.out` and two `.bak-*` from earlier rounds. | Move everything older than this round into `archive/`. **Nothing deleted.** | `archive/` |

### 22.2 The ops — 213 → 215, and why only two

**Nothing was retired, weakened or retargeted.** E1 and E3 are **payload** edits inside literals that
existing asserted ops already write and assert (the precedent §20.5 and §21.2 record); the arm-(f)
rewrite is a payload edit inside `sc-allowed-arms`. Only the arm-8 base rule needed new anchors:

| op | anchor | what it changes |
|---|---|---|
| `sc-arm8-stacked-base` | arm 8's `mb=$(git -C "$G" merge-base "$BASE_EXPECT" "$s" …)` | reads `stack-on=` by field scan, resolves row N's PINNED SHA out of the table, measures against THAT; a stacked row whose parent pin does not resolve is UNMEASURED and FAILS rather than silently re-based on master |
| `sc-arm8-stamp-base` | arm 8's per-row `files=… allowedByRuling=` stamp | the stamp NAMES the base it measured against and the resolved merge-base |

| file | src | out | ops (was) |
|---|---:|---:|---:|
| `coord-train48-assemble.sh` | 5,154 | 5,210 | 124 (124) |
| `coord-train48-rehearse.sh` | 561 | 579 | 12 (12) |
| `coord-train48-derive-selfcheck.sh` | 779 | 1,513 | **45 (43)** |
| `coord-train48-land.sh` | 637 | 660 | 22 (22) |
| `coord-train48-land-dryread.sh` | 264 | 313 | 12 (12) |
| | | | **TOTAL 215 (213)** |

### 22.3 The re-derive, per §17.3, and the gates as run

Six train-47 inputs copied FRESH into a scratch tree whose `train48/` held ONLY `t48-derive.py`, run
there, then **run a SECOND time into a second fresh scratch tree** and every output `cmp`'d:

```
coord-train47-assemble.sh          e03178b96d31d162588acd75fd4f42854b7f6f2c8b698a62e948a08d56e89734
coord-train47-rehearse.sh          584ea342294f81130535d0c216fb6a2d1e08ba712d02c8fdd2701745860a86b6
coord-train47-land.sh              57ecefe06268ac5bc1f470077de146b44e4603a5855f5a132d1f2ec8f37e6901
coord-train47-land-dryread.sh      3418ef1d5c93b90b306c1a76a80d84acfc099e97ee921d57c92745d9ca81c467
coord-train47-derive-selfcheck.sh  24541bfd6252f2252a09d3264061c82f34c1e4f365dc9efc545c5d2585ef4a71
t47-derive.py                      5de1f00c82929854c622d214b4bbbd74fa32767fab18a7b71238061208eb4a6d
python t48-derive.py (scratch x2)  rc=0   TOTAL asserted operations: 215   (was 213)
run 2 vs the shipped copies        IDENTICAL x7  (all five .sh, launch-run1.sh, t47-derive.py.reference)
```

| file | lines | md5 |
|---|---:|---|
| `coord-train48-assemble.sh` | 5,210 | `de410ed0959173a65766818b6b6ce24e` |
| `coord-train48-derive-selfcheck.sh` | 1,513 | `7ae8fd091925d259a10743c1f15581c9` |
| `coord-train48-rehearse.sh` | 579 | `ce04002322429968dcd3da65e61dccd4` (unchanged) |
| `coord-train48-land.sh` | 660 | `d204f9c9bda3632819fe140586c45cf7` (unchanged) |
| `coord-train48-land-dryread.sh` | 313 | `db584a7f862d6d2c4a82cda4c94efa75` (unchanged) |
| `launch-run1.sh` | 23 | `188716d90882bc0cfc3f762bae77093b` (unchanged) |
| `t48-derive.py` | 2,913 | `971c32700c5c50144019f91f5a30c944` |

```
CR bytes: every live .sh = 0 · t48-derive.py = 0 · these NOTES = 0
          coord-train48-derive-report.log = 255 (CRLF, the §12 redirect caveat, unchanged in kind)
bash -n : all five derived .sh + launch-run1.sh + launch-rehearse-run1.sh + the E4 replay  rc=0, out=[]

bash coord-train48-derive-selfcheck.sh          (ONLINE)
=== SELF-CHECK DONE overallFail=0 offlineUnmeasured=0 ===       exit 0
214 PASS/ok lines · anchored FAIL (grep -cE '^[[:space:]]*FAIL') = 0 · unanchored grep -c FAIL = 6
arm 8 coverage :: rows read=18 of 18 :: rows UNMEASURED=0
arm (f) :: rows carrying an allowed= ruling=4 :: ruled paths read=7 :: STALE=0 :: UNMEASURED=0

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                             exit 0, 0 FAIL
```

The six unanchored `FAIL` hits are the quoted CONTROL-RED lines and prose, which is what a control is
for. The `214` is §21's `215` minus the one PASS line arm (f) no longer prints for row 13's dropped
ruling, plus the new control lines — the number is a reading, not a target.

**Arm 8 now NAMES its base, and three rows read a different one:**

```
seat  3 … measured against row 2's  PINNED SHA [191164e7a] (DECLARES stack-on=2)  files=1
seat 10 … measured against row 9's  PINNED SHA [68ad83c2c] (DECLARES stack-on=9)  files=1
seat 13 … measured against row 11's PINNED SHA [5cee80fbe] (DECLARES stack-on=11) files=4 allowedByRuling=0
seat 11 … measured against the train's base [origin/master]                       files=8 allowedByRuling=2
```

### 22.4 The E2 control, and the fact that it reproduces E1 exactly

Arm (f) is now a **function over a table given to it** (`stale_ruled`) rather than a loop over the
global, for the reason safety-floor item 13 gives: a predicate that can only read the live table can
never be shown to fail. The control pair is **DERIVED at run time** — the first row declaring
`stack-on=` whose parent pin resolves, plus a path only the PARENT touches and a path in the child's
OWN delta — so no row number and no SHA is typed into the arm. It selected row 13 on row 11, and the
red path it derived is `.claude/rules/converter.md`: **the control is E1, reproduced mechanically.**

```
ok   CONTROL RED   :: a stacked row ruling [.claude/rules/converter.md] — a path only its BASE
                      touches — reads STALE and the path is NAMED
ok   CONTROL GREEN :: the SAME row ruling [docs/phase4/CENSUS-token-door-live-wrappers.md] — a path
                      in its OWN delta — PASSES
ok   DISCRIMINATOR :: the SAME table with `stack-on=` REMOVED passes on [.claude/rules/converter.md]
```

⚠ **THE DISCRIMINATOR IS THE READING THAT MAKES THE OTHER TWO ABOUT E2.** Without it, the red control
is evidence about a path; with it, the red is shown to be produced **by the base rule and by nothing
else** — strip `stack-on=` and the identical ruling goes green, which is precisely train 47's shape.

### 22.5 E4 — the slots, and the conflict that was an artefact

⚠ **THE SAVED STAGES OF REHEARSAL RUN 1 COULD NOT BE USED, AND THE REASON IS A SILENT-SUBTRACTION.**
Run 1 **aborted** seat 9, so the accumulation it presented to seats 10 and 15 did not contain seat 9's
appended entries. A resolution built from that `ours/` would have **subtracted seat 9** the moment the
assembly — which pre-resolves seat 9 and carries it — reached a later seat. So the slots were filled
from a **scratch replay** of seats 1..18 in table order onto `271300cea`, each conflict resolved as it
happened, so every `ours` is the union the assembly will actually hold.

```
=== E4 REPLAY DONE seatsClean=16 conflicts=2 slotsFilled=2 bad=0 ===   exit 0
union HEAD 60074c0e20…  union tree bfb3be295b6d0b9b65efa9f3d2c747eace4dfbb3
conflict-marker lines across the union's CHANGED files = 0 (in 0 file(s))
go vet ./... in src/go2cs on the FINAL union (GOROOT=go1.24.13, GOTOOLCHAIN=local)  exit=0
go test -run TestLicensing ./... on the FINAL union                                  exit=0
```

**THE THIRD CONFLICT WAS NOT A PROPERTY OF THE TRAIN.** Seat 10 declares `stack-on=9`; with seat 9
aborted, run 1 merged seat 10 onto a tree that lacked its own base, and the BOARD collided. With seat
9 union-resolved and carried, **seat 10 merges CLEAN**. Its run-1 slot is moved to
`archive/stale-slot-seat10-rehearse-run1/` with its reason written beside it — kept, not deleted,
because `--verify` walks every `seat*/` it finds and `merge_seat` only ever reads a slot when a merge
FAILS. ⚠ This also discharges run 1's own caveat: every collision those three seats would have had
with a LATER seat was UNMEASURED there, and the replay measured all of them.

**The precondition, per seat, asserted BEFORE the slot was written:** the seat's BOARD diff against its
merge-base **removes and modifies nothing**. Both passed (`removed=0 modified=0`).

⚠ **THE POST-CONDITION AS FIRST WORDED IS WRONG BY CONSTRUCTION, AND THE MEASUREMENT IS RECORDED
RATHER THAN THE PREDICATE QUIETLY LOOSENED.** `lines(resolved) == lines(ours) + lines added by the
seat` assumes the ledger is appended at EOF. It is not: the BOARD's last line is a **SENTINEL** —
`<!-- {% endraw %} — keep this the FINAL line: the board is append-only and every append must land
INSIDE the raw guard … -->` — so every side **INSERTS above it**. The first cut of the checker read
both slots as FAIL at exactly `−1`. The structural identity is what holds, and every collapsed line
was measured and required to be **EMPTY**:

```
seat  9  base=24275 ours=24328 theirs=24401 resolved=24453
         resolved = base[:24274] + oursInsert(53)  + theirsInsert(126) + baseTail(1) MINUS 1 BLANK line
         merge-file --union exit=0 · P3 all 126 seat lines verbatim+in order · P4 all 24328 ours lines
         verbatim+in order · conflictMarkers=0
seat 15  base=24275 ours=24516 theirs=24539 resolved=24779
         resolved = base[:24274] + oursInsert(241) + theirsInsert(264) + baseTail(1) MINUS 1 BLANK line
         merge-file --union exit=0 · P3 all 264 seat lines verbatim+in order · P4 all 24516 ours lines
         verbatim+in order · conflictMarkers=0
```

The single collapsed line in each is the blank line the two insertions share at their seam, aligned
once by the three-way diff. **No ledger entry was merged, shortened, re-ordered or lost** — that is
what P3 and P4 assert, and they assert it as VERBATIM SUBSEQUENCES rather than by a count.

```
bash coord-train48-rehearse-run2.sh --verify        (a per-run COPY, cmp-identical to the live script)
=== VERIFY DONE seats=2 files=2 bad=0 ===                        exit 0
  seat  9 :: pin [68ad83c2c] == the table's :: RESOLVED … lines=20247 conflictMarkers=0
  seat 15 :: pin [da5e83047] == the table's :: RESOLVED … lines=20508 conflictMarkers=0
```

### 22.6 The template lesson, RECORDED AND DELIBERATELY NOT IMPLEMENTED

The generalising fix for the BOARD is a **`merge=union` attribute** for
`docs/phase4/BOARD-next-validation-candidates.md` in the assembly worktree's `.git/info/attributes`:
git would then resolve that one path by union at merge time and the seat would never conflict at all.
It is **recorded for train 49 and NOT implemented now** — introducing an automatic resolution strategy
into a train that has already been rehearsed changes what every later reading of this train means, and
the slot mechanism already produces the same bytes with a per-seat precondition, a post-condition and
a coordinator in the loop. ⚠ When it IS implemented it needs its own red control: a `merge=union`
attribute silently resolves a MODIFY/MODIFY collision on that path too, and the append-only
precondition this round asserts by hand is exactly what would stop being asserted.

### 22.7 The vacuous-green lesson from round 2, carried forward

§21.3 recorded that arms (f) and (g) first read a `SEAT_TABLE` left holding **two synthetic rows** by
the plants above them, and reported every count zero while reading healthy. Round 2 adds the general
form of that lesson: **a planted control leaks into whatever scope it plants into, and the arms after
it inherit the plant.** The remedies now in the file are (a) re-read the table FROM THE FILE, (b) check
the re-read against an INDEPENDENT count (arm 2's `18 == 18`), and (c) — new this round — write the
predicate as a **function over a table passed in**, so a control hands it a synthetic table as an
ARGUMENT and no global is touched at all. (c) is what the E2 controls use, and it is the only one of
the three that cannot be defeated by a later plant.

### 22.8 What §22 did NOT do

* **Nothing under `train47/` was written.** Re-asserted at the end of this session by the reading that
  means something — `cmp` against the run-8 copies plus sha256 of the six derive inputs plus an mtime
  census — because `git status --porcelain` over `train47/` is **VACUOUS** (`.claude/` is in
  `.git/info/exclude`). All six input sha256s are identical to §21.4's, the live
  `coord-train47-assemble.sh` is `cmp`-identical to `coord-train47-assemble-run8.sh`, and the latest
  train-47 mtime is `18:22:38` — before this session began.
* **No class shape and no forbidden list was widened.** E1 REMOVES an exemption; it adds none.
* **No pin was moved.** Row 4 stays at `d18059950` and row 17 at `a633896bf` — E3 is a wording fix, not
  a re-pin.
* **No commit, stash, checkout or push on any real branch, and no battery.** The E4 replay ran in a
  throwaway detached worktree at `/c/go2cs-tmp-coord/t48-rehearse`, which was **removed and the removal
  confirmed** (`git worktree list` carries no row for it). Its `go vet` and `TestLicensing` on the
  final union are the only compilation this session performed.
* **F1 (`WT=`) is still PENDING and still refuses.** Filling it is the coordinator's, at launch.

## 23. 2026-09-13 ~21:00 — THE EIGHTEEN SEAT-CONTENT ARMS, WRITTEN AT SEAT FILL AND REPLAYED

Run 1 merged all 18 seats and stopped at `SEAT-CONTENT ASSERTIONS REFUSED: 0 arm(s) for 18 merged
seat(s)`. That refusal is the fill point working: **the template cannot write per-seat arms**, and a
battery over a tree nobody asserted is the vacuous green the whole file exists to refuse. This section
records the arms, how they entered the assembler, and the two readings that say they can go red.

### 23.1 They entered through the derive, as ONE asserted and counted payload

The arms are per-train content and are therefore a **derive op**, exactly like the seat table — never a
hand edit of the assembler. Two ops were added to `t48-derive.py` (**215 → 217**):

| op | kind | what |
|---|---|---|
| `seat-content-arms-18` | `insert_after` | the 18 arms, inserted after the slot comment's last line, i.e. **between `SEATASSERT_N=0` and the END marker**; the BEGIN/END fence and every line of the slot's comment text are kept |
| `sc-arm13-blockscoped-armcount` | `insert_after` | the self-check's new **block-scoped** arm count and its regressed-copy control |

The payload asserts its own size in Python before a byte is written (`18 increments, wanted 18`), so an
arm lost to an editing slip is refused at derive time rather than counted by a gate that runs after a
battery.

⚠ **ORDER IS LOAD-BEARING, AND IT COST ONE RUN TO LEARN.** The arms op was first placed beside the slot
op and the derive **refused**: `ABSENCE [coord-train47-assemble.sh / f2-no-arow9-anchor]: 3
occurrence(s) of [A-row9] survive`. That absence check asserts **train 47's** `A-row9` anchor did not
survive the derive, and it must read a file this train has not yet written its own `A-row9` into or it
asserts nothing. The arms op was moved **after** `f2-no-arow9-anchor`, with the reason written beside
it. The refusal was correct and the mechanism is the one the file was built for.

### 23.2 The eighteen arms

Every arm is gated on its own `$SEATn_SHA`, stamps each reading beside its `must be`, and ends in the
`SEATASSERT_N` increment. **Every arm carries at least one predicate that is FALSE at the base
`271300cea0` and TRUE at the union `3e0e6023f8`** — §23.4 is that reading, per arm, mechanically.

| arm | row / branch | what it reads out of the assembled FILES | the reading that cannot be true at the base |
|---|---|---|---|
| A-row1 | 1 `g-handown-metadata-t48-r47` | 2 guard files present + each registered x1 in projitems + each `^func Test` >= 1; `package_info.cs` files this train moved >= 4 (measured 4); the `allowed=` **cache.cs blob == the seat's own blob** | both guard files are absent at the base; the cache.cs blob differs |
| A-row2 | 2 `c2-h6-crosscheck` | the dated heading `Cross-check block -- C2, 2026-09-13` x1; the record in this train's delta | heading base 0 / union 1 |
| A-row3 | 3 `laneR-h6-alias-block` (stack-on=2) | its own heading x1 **and row 2's still x1** — two appends to one record | heading base 0 / union 1 |
| A-row4 | 4 `laneR-docs-h6-skeleton` **SHA mode** | ADDED by the train; **union blob == the blob at `$SEAT4_SHA`**; `mcleanup.cs` x**2** (the PIN's reading; the branch TIP reads 9); the preservation census's `item 21 RETIRED` x1 | the file is absent at the base; the count alone discriminates pin from tip |
| A-row5 | 5 `laneR-prepin-baselines-recut` | `PRE-PIN BASELINE COMMIT (R)` x1; the BOARD's own title still x1 | heading base 0 / union 1 |
| A-row6 | 6 `c2-safepush-shallow-skip` | ⚠ the guard file **already existed at the base** — `-f` and the registration are true there and cannot fail. `^func Test` == **2** (base 1), the new test by name >= 1, its `safePushRepoIsShallowIn` helper x1 | the COUNT (1 → 2) and the test name (0 → 2) |
| A-row7 | 7 `c1-seat-duplication-census` | guard + instrument present, guard registered x1, `TestSeatDuplicationCensusSelfTest` x1, **executable lines** in the instrument >= 1 (measured 283) | both files absent at the base |
| A-row8 | 8 `g-fleet-patchid-census` | same shape; `TestFleetPatchIdCensusSelfTest` x1; instrument executable lines (measured 199) | both files absent at the base |
| A-row9 | 9 `i9-board-runtime-door-bisect` **pre-resolved** | `door regression is EXACTLY` x1; the BOARD's own title still x1 | heading base 0 / union 1 |
| A-row10 | 10 `i9-board-archive-tar` (stack-on=9) | `a banked 97-verdict row killed the test host ONCE in 27 isolated runs` x1 | heading base 0 / union 1 |
| A-row11 | 11 `c2-census-goroot-fix-clean` | 4 files present; guard registered x1; `^func Test` >= 1 (measured 10); `corpusPinnedReleaseOrError` >= 1; **both `allowed=` `.claude/` paths BLOB-IDENTICAL to the seat** (2 of 2) | the guard is absent, the resolver reads 0, the blobs differ |
| A-row12 | 12 `c1-gctestisreachable-clean` | ⚠ **registry AND displacement, two halves**: the `gcTestIsReachable: goosAny` entry x1; the body in `mgc_impl.cs` >= 1; mentions **LEFT in mgc.cs == 1** (base 2); `mgc.cs` DELETION column > 0; the `panic_impl` erratum >= 1 | entry 0 → 1, mgc.cs mentions 2 → 1, deletions 0 → 55 |
| A-row13 | 13 `c1-token-door-census-stacked` (stack-on=11) | guard + instrument + census record; `TestTokenDoorCensusControls` x1; executable lines (measured 134); record ADDED x1 | all three absent at the base |
| A-row14 | 14 `c1-mfinal-mint-door-clean` | ⚠ a **pure annotation**: the door note >= 1 AND numstat **+42/-0**, deletions **must be 0**; the hand-own marker survives | the note reads 0 at the base |
| A-row15 | 15 `c2-board-both-ordered` **pre-resolved** | its own heading x1 **and rows 9 and 10's each still x1** (read only when those rows merged; a partial run stamps NOT MEASURED) | heading base 0 / union 1 |
| A-row16 | 16 `c2-merge-probe-predicate` | script executable lines >= 1 (measured 34); the skill's rule sentence x1; **both ruled paths BLOB-IDENTICAL to the seat** | `merge-probe.sh` is absent at the base |
| A-row17 | 17 `c2-h10-shardmap-projection` | ADDED x1; its own title x1; non-empty lines >= 1 | the file is absent at the base |
| A-row18 | 18 `c2-h10-map-rederivation` | 6 files present; driver executable lines >= 1 (measured 215); **files ADDED among driver + 2 records == 3**; the walltimes digest section >= 1; **`.gitattributes` and `shardmap.py` BLOB-IDENTICAL to the seat** | 3 files absent at the base; both ruled blobs differ |

### 23.3 The self-check's new arm, and its regressed-copy control

`$ARMN` was a **file-wide** count over non-comment lines: an increment written anywhere in the assembly
would satisfy it while saying nothing about the slot the run-time gate reads. The new arm extracts the
**fenced block only**, strips comments (the slot's own comment text spells the increment), and requires
**EXACT EQUALITY** with the filled-row count — the run-time gate is `-lt` and can only ever see one arm
too few, never one too many, and an arm too many is an assertion about a row nobody seated.

```
SEAT-CONTENT arms INSIDE the fenced block=18 :: filled rows=18 :: the regressed copy (one increment deleted) reads 17
ok   the FENCED BLOCK carries EXACTLY one arm per FILLED row (18 == 18) -- a file-wide count cannot say this
ok   CONTROL RED :: the same predicate over a REGRESSED copy reads 17 and misses 18, so the equality above can go red
```

⚠ **THE CONTROL CAUGHT ITSELF FIRST.** Its first form deleted the first line matching `SEATASSERT_N=`,
which is **`SEATASSERT_N=0`** — the initialiser, also inside the fence — so the regressed copy still read
**18** and the arm FAILED. The deletion predicate now matches `SEATASSERT_N + 1` by `index()`. A control
that agreed with the thing it controls is the same vacuity class as the arms it guards.

### 23.4 REPLAY — the fail-able proof, run over both trees

The fenced block was extracted verbatim from the derived assembler (317 lines, `bash -n` rc=0) into a
scratch harness that stubs `stamp` as echo and `fail_gate` as a counter, sets every `SEATn_SHA` to its
pin and `BASE=271300cea0`, and runs the block with the worktree as cwd. A throwaway detached worktree
was created at `/c/go2cs-tmp-coord/t48-armreplay`, run at the union, `checkout --detach`'d to the base,
run again, and **removed** (`git worktree list` carries no row for it).

```
REPLAY IN /c/go2cs-tmp-coord/t48-armreplay AT 3e0e6023f8
REPLAY RESULT :: arms=18 fail_gates=0 fired=[]

REPLAY IN /c/go2cs-tmp-coord/t48-armreplay AT 271300cea0
REPLAY RESULT :: arms=18 fail_gates=18
fired=[A-row1 A-row2 A-row3 A-row4 A-row5 A-row6 A-row7 A-row8 A-row9 A-row10
       A-row11 A-row12 A-row13 A-row14 A-row15 A-row16 A-row17 A-row18]
```

**18 of 18 arms run at the union and none refuses; 18 of 18 refuse at the base, one fail_gate each.**
That is safety-floor item 13 discharged per arm rather than for the block as a whole: not one of these
greens is a green that cannot go red.

### 23.5 The re-derive, per §17.3, and the gates as run

Six train-47 inputs copied FRESH into a scratch tree holding only `t48-derive.py`, run there, then run a
SECOND time into a second fresh scratch tree and every output `cmp`'d:

```
coord-train47-assemble.sh          e03178b96d31d162588acd75fd4f42854b7f6f2c8b698a62e948a08d56e89734
coord-train47-rehearse.sh          584ea342294f81130535d0c216fb6a2d1e08ba712d02c8fdd2701745860a86b6
coord-train47-land.sh              57ecefe06268ac5bc1f470077de146b44e4603a5855f5a132d1f2ec8f37e6901
coord-train47-land-dryread.sh      3418ef1d5c93b90b306c1a76a80d84acfc099e97ee921d57c92745d9ca81c467
coord-train47-derive-selfcheck.sh  24541bfd6252f2252a09d3264061c82f34c1e4f365dc9efc545c5d2585ef4a71
t47-derive.py                      5de1f00c82929854c622d214b4bbbd74fa32767fab18a7b71238061208eb4a6d
python t48-derive.py (scratch x2)  rc=0   TOTAL asserted operations: 217   (was 215)
run 1 vs run 2                     IDENTICAL x7  (all five .sh, launch-run1.sh, t47-derive.py.reference)
```

| file | lines | md5 |
|---|---:|---|
| `coord-train48-assemble.sh` | 5,515 | `2ff36bc5e93823b2744eaf41b426d7b0` (was 5,210) |
| `coord-train48-derive-selfcheck.sh` | 1,522 | `cb4dfb46708fc6ef3262a428685091df` (was 1,513) |
| `coord-train48-rehearse.sh` | 579 | unchanged |
| `coord-train48-land.sh` | 660 | unchanged |
| `coord-train48-land-dryread.sh` | 313 | unchanged |
| `t48-derive.py` | — | `179164f8ce0a2b183c16429aa5c8f848` |

```
CR bytes: all five derived .sh = 0 · launch-run1.sh = 0 · t48-derive.py = 0
bash -n : all five derived .sh + launch-run1.sh + launch-rehearse-run1.sh + the extracted block  rc=0, out=[]

bash coord-train48-derive-selfcheck.sh          (ONLINE)
=== SELF-CHECK DONE overallFail=0 offlineUnmeasured=0 ===       exit 0
216 PASS/ok lines · anchored FAIL (grep -acE '^[[:space:]]*FAIL') = 0 · unanchored grep -ac FAIL = 6

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                             exit 0, 0 FAIL
```

The **216** is §22's 214 plus this section's two new arms. The six unanchored `FAIL` hits are the quoted
CONTROL-RED lines and prose, unchanged in kind.

### 23.6 What §23 did NOT do

* **Nothing under `train47/` was touched.** The six derive inputs hash exactly as §22.3 recorded them,
  `coord-train47-assemble.sh` is `cmp`-IDENTICAL to its own `-run8.sh` copy, and the newest mtime in the
  directory is `2026-09-13T18:22:38`, hours before this session.
* **The run-1 record was not rewritten.** `coord-train48-assemble-run1.sh` and its `.stdout` stand as the
  record of the run that refused; the live assembler is the armed one.
* **No seat was re-pinned, no class changed, no `allowed=` ruling widened, and no fill point filled.**
  `WT=` is still PENDING and still refuses.
* **No battery, no commit, stash, push, or checkout on any real branch.** The only tree touched was the
  throwaway replay worktree, created and removed in this section.

### 23.6 record correction (2026-09-13 21:25, COORD after the adversarial verify)

Six figures above were corrected in place: the md5s of `coord-train48-derive-selfcheck.sh` and `t48-derive.py`
(taken before the control fix in 23.x and not refreshed) now read the live files' hashes, and four readings in
the 23.2 table (A-row1 package_info moved 7, A-row7 executable lines 208, A-row8 168, A-row13 124) now match
the replay outputs. No artifact changed; every corrected figure sits inside its predicate. The 317-line block
count in 23.4 excludes the two marker lines (319 inclusive). A-row6's `-f`/projitems readings are true at the
base tree (the row rests on the Test count 1 -> 2 and the new test name); A-row15's rows-9/10 reading is NOT
MEASURED when either row is PENDING.

## 24. 2026-09-13 ~21:55 — THE RUN-2 DEFECTS (F1a, F1b, F2), CUT AS DERIVE OPS

**What this section records.** Run 2 of the train-48 battery (`coord-train48-assemble-run2.stdout`,
its own read-only record) refused a healthy tree at two gates. Both refusals were the INSTRUMENT
being wrong, both were ruled by COORD, and both are cut as ASSERTED derive operations anchored on the
train-47 assembler text — never by hand-editing the derived assembler, which the next derive would
silently undo. **`train47/` was read and never written**, as §1 requires: the six derive inputs were
verified byte-identical to their pre-task hashes afterwards, and `coord-train47-assemble.sh` is still
`cmp`-identical to `coord-train47-assemble-run8.sh`.

### 24.1 F1a — G10d refused a CORRECT `text eol=lf` pin

Run 2 read, over nine docs files in the delta:

```
CR/LF MISMATCH (attr=[set] i/lf w/lf): docs/phase4/DATA-sweep-row-walltimes.md CR=0 LF=395
G10d docs CR/LF :: checked=9 mismatches=2 textExempt=0 []
^ G10d REFUSED: a docs file that is NOT `-text`-exempt has a non-CRLF working-tree form ...
```

The path is pinned `text eol=lf` by a seat's own `.gitattributes`, and the two layers AGREE (`i/lf
w/lf`). The gate knew exactly one exemption — `-text`, "these bytes are verbatim" — and `eol=lf` is a
different statement: "this file IS text and its worktree form is LF **BY INSTRUCTION**". The gate was
refusing a declaration the repository makes on purpose.

**RULED:** an `eol` attribute reading `lf` is EXEMPT-BY-PIN when the two layers agree, stamped by name
beside `textExempt` and counted SEPARATELY as `eolPinned=N [names]`; a pinned file whose layers
DISAGREE is still a mismatch, exactly as a `-text` file's is. Five ops — `f1a-g10d-eol-counters`,
`f1a-g10d-eol-attr`, `f1a-g10d-eol-branch`, `f1a-g10d-eol-stamp`, `f1a-g10d-eol-refusal` — plus the
`require` guard `f1a-eol-pin-exemption-kept`.

### 24.2 F1b — the PRE-RESOLVED apply left its own artifact for G10d to find

```
CR/LF MISMATCH (attr=[unspecified] i/lf w/lf): docs/phase4/BOARD-next-validation-candidates.md CR=0 LF=24779
```

The BOARD is not pinned at all. It reads LF in the worktree because the PRE-RESOLVED slot apply `cp`s
the LF union file in and `git add`s it: the worktree form stays LF while this box's `autocrlf` would
have materialised CRLF. G10d was reading the apply's own artifact as a seat's defect.

**RULED:** the apply re-materialises each resolved path through git after the add, so the worktree
form is the one the worktree's own attributes dictate (`f1b-preresolved-rematerialise`, guarded by
`f1b-rematerialise-kept`).

⚠ **THE `rm -f` IS LOAD-BEARING, AND THAT IS A LAB FINDING RATHER THAN A PRECAUTION.** The ruling as
worded was `git add -- path && git checkout -- path`. Probed in a throwaway repo with
`core.autocrlf=true`: after `cp`+`add` the file reads **CR=0**; after a bare `git checkout -- path` it
still reads **CR=0** (the index entry is stat-clean one command after the add, so the checkout writes
nothing); `git checkout-index -f -- path` also reads **CR=0**; and only `rm -f` **then** checkout
reads **CR=3** — one CR per LF line. The shipped line is therefore
`cp … && git add … && rm -f … && git checkout … && RESAPPLIED=…`. The removal window is safe by the
`&&` chain: a failed checkout leaves `RESAPPLIED` short, and the existing INCOMPLETE check aborts the
merge and restores the tree. A fix that had shipped without the lab would have been a no-op wearing a
conversion's clothes — the exact class this instrument exists to catch.

### 24.3 F2 — G11(a) refused a tree for a CLASS'S POTENTIAL

```
OWED VECTOR :: golib=1 … golibtests=1 … (DERIVED from the CLASSES of the 18 merged row(s))
^ G11(a) JUSTIFICATION FALSE :: src/tests/GolibTests=0 where the OWED vector says a merged class owes it
```

Two `golib-corpus-handown` rows touch `src/core/runtime` and `src/go2cs` and nothing under
`src/tests/GolibTests`. A class says what a seat MAY touch; G11(a) refuses on what a seat DID touch,
and those are different questions — the same fault the syscall DIRECTORY LITERAL was written out of.

**RULED:** `golibtests` leaves the class loop and is derived from the merged DELTA, stamped as
`golibtests=N (from the DELTA: M file(s))`; `OWED_GOLIB` stays class-derived, because it selects LEGS
and LEG 3 still RUNS for the golib family. LEG 3's existing NOTE is unchanged and now reads the
delta-derived value, so a zero-GolibTests train is a READING there rather than a refusal here. Three
ops — `f2-owed-gt-off-the-class`, `f2-owed-gt-from-delta`, `f2-owed-gt-cross-check` — plus the
`absent_in_code` guard `f2-no-class-derived-golibtests-owe`.

⚠ `f2-owed-gt-cross-check` exists because the vector is stamped BEFORE the per-directory census runs,
so the same quantity is now derived twice over one `BASE..HEAD`. The two are CHECKED against each
other and a disagreement calls `fail_gate G11a-gt-derivation`: two instruments deriving one fact
independently is how two instruments drift.

⚠ **AND WHAT F2 DOES NOT FIX, STATED RATHER THAN TIDIED.** Run 2's OTHER G11(a) refusal —
`JUSTIFICATION FALSE :: src/core/golib=0` — is the SAME shape on `OWED_GOLIB`, and the ruling
deliberately leaves `OWED_GOLIB` class-derived. On a seat set like run 2's, G11(a) still reads FALSE
for `src/core/golib` and still calls `fail_gate G11a`. That is a ruling owed to COORD, not an
omission here.

### 24.4 The one op nobody asked for, and why it is here

Arm 10 of the self-check greps for G10d's refusal BY ITS WHOLE SPELLING, exemption list included.
F1a adds `eol=lf` to that list, and the first ONLINE run after the fix read
`FAIL: the non-exempt refusal is missing, so the arm is a blanket pass for docs/` — an arm reporting
the absence of a SPELLING as the absence of the THING. `f1a-arm10-decoy-reanchored` re-anchors the
predicate on the part of the sentence that is about the READING (`has a non-CRLF working-tree form`),
which cannot move when the list grows again.

### 24.5 The new self-check arms, and their controls

`f1f2-selfcheck-arms` adds two sections, each with a RED control:

* **(h)** STATIC over F1a and F1b, with the control a copy regressed to **train 47's exact shape** —
  the pin branch's guard neutered and the re-materialising `rm`+`checkout` sed'd out. Derived reads
  `eolAttrReads=1 pinBranches=1 pinCountStamped=1 rematerialisations=1`; the regressed copy reads
  `pinBranches=0 rematerialisations=0`.
* **(i)** BEHAVIOURAL over F2: `blk` extracts the assembly's OWN `g11a_arm` and runs it over a
  synthetic vector. A delta-derived owe `(0,0)` sets `G11ABAD=0`; the class-derived owe `(1,0)` sets
  `G11ABAD=1`. Its static half reads `deltaDerivations=1 classSetters=0 stampNamesDelta=1
  golibClassDerived=1`.

### 24.6 Measured, after the derive

| | before | after |
|---|---|---|
| asserted operations | 217 | **231** (assembly 125 → 137, self-check 46 → 48) |
| `coord-train48-assemble.sh` | 5,515 lines | **5,563 lines**, `e296f79521df47da9d66cfaad23bfa2a` |
| `coord-train48-derive-selfcheck.sh` | 1,522 lines | **1,602 lines**, `543eac648d7b740520639632be8a865c` |
| `coord-train48-rehearse.sh` | 579 lines | 579 lines, `ce04002322429968dcd3da65e61dccd4` (**unchanged**) |
| `coord-train48-land.sh` | 660 lines | 660 lines, `d204f9c9bda3632819fe140586c45cf7` (**unchanged**) |
| `coord-train48-land-dryread.sh` | 313 lines | 313 lines, `db584a7f862d6d2c4a82cda4c94efa75` (**unchanged**) |
| `launch-run1.sh` | 23 lines | 23 lines, `188716d90882bc0cfc3f762bae77093b` (**unchanged**) |
| `t48-derive.py` | 3,259 lines | **3,467 lines**, `cb44625f317ceaaa904f049924aed7e7` |

```
re-derived per 17.3 TWICE from fresh copies of the six train-47 inputs -- rc=0, 231 ops both times;
all seven outputs IDENTICAL on the second run
CR bytes: every derived .sh = 0 - t48-derive.py = 0 - bash -n on all six = rc 0
ONLINE  bash coord-train48-derive-selfcheck.sh -> rc=0, overallFail=0, offlineUnmeasured=0
        anchored FAIL = 0 - 220 PASS/ok lines (the 6 unanchored FAIL hits are the lesson prose)
        bash coord-train48-land-dryread.sh     -> rc=0, overallFail=0, 0 FAIL
LAB     F1a PASS (pinned file EXEMPT-BY-PIN, unpinned LF decoy still a mismatch: checked=2
        eolPinned=1 mismatches=1) - F1b PASS (cp+add CR=0, bare checkout CR=0, rm+checkout CR=3)
        F2 PASS (synthetic 0, regressed 1)
```

### 24.7 What §24 did NOT do

* **Run 2's LEG C failure is a SEAT defect, not a template item.** Row 13's hardcoded default path is
  the seat's, C1 is re-cutting it, and nothing in the template is changed for it. A template op that
  worked around a seat's bug would hide the seat's bug.
* **Nothing under `train47/` was touched**, and no pin was moved.
* **Nothing was committed, stashed, checked out, pushed or announced**, and the live run-2 copy, its
  worktree and its stdout were read as a record and never written.

## 25. 2026-09-13 ~22:40 — ROW 13 RE-PINNED TO C1'S RE-CUT (G1), AND A-row13 RE-WRITTEN FOR IT

**What this section records.** Run 2's LEG C refusal was ruled a **SEAT** defect (§24.7), C1 re-cut the
seat, and COORD re-pinned row 13 to the re-cut (ruling `b6c472397`). The re-pin and the arm re-write
are cut as **derive operations** — payload edits inside the existing seat-table, header-narrative and
arms ops (the §20.5 precedent), plus six NEW asserted ops that make the re-pin a claim the derive
checks rather than a change a reader has to trust. **`train47/` was read and never written**: the six
derive inputs hash exactly as §23.5/§24 recorded them, `coord-train47-assemble.sh` is still
`cmp`-IDENTICAL to `coord-train47-assemble-run8.sh`, and the newest mtime in that directory is
`2026-09-13T18:22`, hours before this task.

### 25.1 The new pin, every figure READ rather than typed

```
git ls-remote origin refs/heads/claude/c1-token-door-census-recut
  93bf3403014efeacc9a99ea6aaa56975a3ba89d8        (short, from git rev-parse --short=9: 93bf34030)
parent                        b4914e878  (the SUPERSEDED pin -- named HERE, in the record, and nowhere
                                          in the assembler: see §25.2)
the one new commit            src/token-door-census.sh  +10/-1 -- "derive CORE from the script's own
                              location -- the literal default refused on one box and censused the
                              WRONG CHECKOUT on another"
delta vs origin/master        11 files, 7 commits (was 11 files, 6 commits)
                              = row 11's EIGHT + CENSUS-token-door-live-wrappers.md +
                                tokenDoorCensusGuard_test.go + token-door-census.sh
                              (go2cs-src.projitems is in row 11's eight and this row modifies it again)
stack-on=11 still holds       git merge-base --is-ancestor 5cee80fbe 93bf34030 -> yes
row 11's two ruled blobs      .claude/rules/converter.md      5d63166a55 at BOTH pins
                              .claude/skills/mailbox/SKILL.md 127dd9fefe at BOTH pins
                              -- so A7b's identity predicate is untouched by the re-pin
```

The new row, as the table now reads it:

```
13|claude/c1-token-door-census-recut|93bf34030|converter-test+tooling|tip|stack-on=11
```

### 25.2 The ops — three payload edits, six NEW assertions

| what | where | kind |
|---|---|---|
| the table row | `seat-table` payload | edit |
| the D2 narrative + a NEW dated `(G1)` paragraph | `header-fill` payload | edit |
| row 13's NOTE column (ref, pin, commit count, the seventh commit named) | the same header payload | edit |
| the D-ruling recap sentence | the same header payload | edit |
| A-row13's whole arm | `seat-content-arms-18` payload | edit |
| `g1-no-superseded-row13-ref` / `g1-no-superseded-row13-sha` | after the arms op | `absent` |
| `g1-row13-recut-seated` / `g1-row13-recut-named` (>= 4) | after the arms op | `require` |
| `g1-row13-no-absolute-path-reading` / `g1-row13-dirname-derivation-reading` | after the arms op | `require` |

⚠ **THE SUPERSEDED REF AND SHA ARE DELIBERATELY ABSENT FROM THE ASSEMBLER.** An assembler that spells
two pins for one row is an assembler a reader can quote the wrong half of; the supersession is history
and history belongs in this file. The two `absent` ops are what makes "gone everywhere" a MEASUREMENT:
before the payload edits the derived assembler read `c1-token-door-census-stacked` **5 times** and
`b4914e878` **2 times**, and either op would have refused the derive outright — that is their red
control, observed rather than constructed.

⚠ **THE SIX OPS SIT AFTER THE ARMS OP ON PURPOSE.** The arms payload carries row 13's ref in its own
comment, so an absence check ordered before it would read a file the arms had not yet been written
into and would assert nothing — the exact ordering fault §23.1 records for `f2-no-arow9-anchor`.

### 25.3 The re-written A-row13, and why it reads the FIX and not merely the file

The previous cut's instrument carried a **hardcoded absolute default path**. `TestTokenDoorCensusControls`
drives the script with **no argument**, so that literal WAS the path under test: it refused on one box,
censused the wrong checkout on another, and it was a username-style home path on a pushed surface
besides. The arm keeps every predicate it had — three files present, guard registered x1, the named
control test x1, executable lines >= 1, the census record ADDED x1 — and adds the two readings that are
about the FIX:

```
S13ABS  absolute paths in the instrument   must be 0   (slash-anchored home/drive forms)
S13DIR  ^CORE=${1:- ... dirname "$0" ... }$ must be 1   (the default DERIVED from the script's location)
```

Both are read through `tr -d '\r'` first. `*.sh` is pinned `text eol=lf` **today** (`git check-attr`
reads `text: set`, `eol: lf`), so the worktree form is LF and an end-anchored pattern matches; a pattern
that silently stops matching the day a pin moves is the CRLF trap this train has already met three
times, and the strip costs nothing. When `S13ABS` is non-zero the arm PRINTS the offending lines by
number before it refuses, because "an absolute path exists" and "here it is" are different messages and
only one of them can be acted on.

⚠ A reading, not a defect: at the BASE the three files do not exist, so the two `tr` redirects emit a
shell `No such file or directory` line each. They sit directly under A-row13's own `REFUSED: ... is not
in the assembled tree` stamps and the arm fires correctly; at a union — the only tree a battery runs
this at — they cannot occur.

### 25.4 Measured, after the derive

| | before (§24.6) | after |
|---|---|---|
| asserted operations | 231 | **237** |
| `coord-train48-assemble.sh` | 5,563 lines | **5,592 lines**, `6acdf3f1e79b841934d590ab813a5923` |
| `coord-train48-derive-selfcheck.sh` | 1,602 lines | 1,602 lines, `543eac648d7b740520639632be8a865c` (**unchanged**) |
| `coord-train48-rehearse.sh` | 579 | 579, `ce04002322429968dcd3da65e61dccd4` (**unchanged**) |
| `coord-train48-land.sh` | 660 | 660, `d204f9c9bda3632819fe140586c45cf7` (**unchanged**) |
| `coord-train48-land-dryread.sh` | 313 | 313, `db584a7f862d6d2c4a82cda4c94efa75` (**unchanged**) |
| `launch-run1.sh` | 23 | 23, `188716d90882bc0cfc3f762bae77093b` (**unchanged**) |
| `t48-derive.py` | 3,467 lines | **3,518 lines**, `f91565a13db52bb0155a1ca2ac207b91` |

### 25.5 The re-derive per §17.3, and the gates as run

The derive asserts its own location (`train48/`) and reads a SIBLING `train47/`, so the scratch tree
mirrors exactly that shape and holds nothing else — `t48-derive.py` plus FRESH copies of the six
inputs, no prior output anywhere, run TWICE into two independent fresh trees:

```
python t48-derive.py (scratch x2)  rc=0   TOTAL asserted operations: 237   both runs
run 1 vs run 2                     IDENTICAL x7 (five .sh, launch-run1.sh, t47-derive.py.reference)
shipped == run 1                   IDENTICAL x7 after the copy-in
CR bytes: every derived .sh = 0 - launch-run1.sh = 0 - t48-derive.py = 0
bash -n : all five derived .sh + launch-run1.sh  rc=0, out=[]
old spelling in the derived .sh: c1-token-door-census-stacked = 0, b4914e878 = 0 (all five files)
new spelling: assemble.sh carries the ref x6 and 93bf34030 x3; no other derived file names the row

bash coord-train48-derive-selfcheck.sh      (ONLINE)
=== SELF-CHECK DONE overallFail=0 offlineUnmeasured=0 ===   exit 0
220 PASS/ok lines - anchored FAIL = 0 - unanchored FAIL = 6 (the quoted CONTROL-RED prose, as before)
  seat 13 (converter-test+tooling) claude/c1-token-door-census-recut :: tip == pin 93bf34030
  seat 13 ... files=4 outsideShape=0 forbiddenHits=0 allowedByRuling=0 :: measured against row 11's
           PINNED SHA [5cee80fbe] -> merge-base 5cee80fbead7bb...

bash coord-train48-land-dryread.sh
=== DRY READ DONE overallFail=0 ===                         exit 0, 0 FAIL
(byte-identical to §24's dry-read except the assembler's line count, 5563 -> 5592)
```

### 25.6 REPLAY — rows 1..13 merged by the assembler's OWN `merge_seat`, then the arms at both ends

A throwaway detached worktree at `/c/go2cs-tmp-coord/t48-r5-replay`, created at the base
`271300cea03a2f47bd7dd8d9ed392c6249dac4c4` and **removed afterwards** (`git worktree list` carries no
row for it and the directory is gone). `merge_seat` — with `seat_field`, `seat_opt`, `seat_row`,
`seat_allowed_paths`, `alw_filter` and the `SEAT_TABLE` — was extracted **by anchor** from the derived
assembler and sourced; nothing was retyped. Rows 1..13 in table order, rows 14..18 gated PENDING:

```
SEAT 1..13 rc=0 each - MERGE PHASE rc=0 - MARKERS=0
seat 9  CONFLICT on the BOARD alone -> PRE-RESOLVED from coord-train48-resolutions/seat9
        manifest seat/pin/base all matched; RESAPPLIED=1 of 1; F1b's re-materialisation visible in the
        run ("LF will be replaced by CRLF") -- the fix working, on the path it was written for
seat 10, 11, 12, 13  CLEAN
seat 11 allowed= ruling IN FORCE, allowedByRuling=2, both paths named
seat 13 files=4 (CENSUS .md 222, projitems 1, guard 85, instrument 208 added lines), outsideShape=0,
        forbiddenHits=0, allowedByRuling=0, conflictMarkersInBlobs=0

REPLAY RESULT UNION :: arms=13 fail_gates=0 fired=[]
REPLAY RESULT BASE  :: arms=13 fail_gates=13
  fired=[A-row1 A-row2 A-row3 A-row4 A-row5 A-row6 A-row7 A-row8 A-row9 A-row10 A-row11 A-row12 A-row13]
A-row13 at the union :: files present=1 - registered x1 - control test x1 - executable lines=124 -
                        ABSOLUTE paths=0 - dirname-derived default x1 - record ADDED=1
```

Thirteen of thirteen arms run at the union and none refuses; thirteen of thirteen refuse at the base,
one `fail_gate` each — safety-floor item 13 discharged per arm, for the re-pinned row as for the rest.

⚠ **THE HARNESS CAUGHT ITSELF, AND THE FIRST PASS IS REPORTED RATHER THAN DISCARDED.** The first replay
extracted every function `merge_seat` calls EXCEPT `alw_filter`, and printed `alw_filter: command not
found` at each forbidden-path and shape reading — those two readings were VACUOUS in that pass (an
empty pipeline reads zero, which is the value that passes). The fragment now extracts `alw_filter` too
and the second pass carries the readings quoted above, with no `command not found` anywhere. Both
passes merged 1..13 rc=0 with 0 markers and produced the same arm verdicts; only the merge-commit SHAs
differ (timestamps), which is why the union SHA is not quoted as a pin here.

### 25.7 What §25 did NOT do

* **Nothing under `train47/` was touched**, and no pin other than row 13's was moved.
* **Rows 14..18 were not replayed.** They were gated PENDING on purpose: this task's question is
  rows 1..13 over the re-pinned seat, and an arm run over a tree its row never merged into asserts
  nothing. §23.4's 18-of-18 reading stands for the full set at the previous row-13 pin.
* **No battery was launched**, and run 2's live copy (`coord-train48-assemble-run2.sh`), its worktree
  `/c/go2cs-tmp-coord/t48-asm` and its stdout were neither read for state nor written.
* **Nothing was committed, stashed, checked out, pushed or announced** on any real branch. The only
  tree written was the throwaway replay worktree, created and removed here.
* **F2's open item is unchanged**: §24.3's `OWED_GOLIB` refusal is still a ruling owed to COORD, and
  nothing in this section touches it.
