# PLAN — context diet: CLAUDE.md from 200K tokens to an index

> **Status**: **COMPLETE 2026-09-12** on `claude/context-diet` — Phase 1 (relocate, `56ff452a5`),
> Phase 2 (distil, `c8492fe31`), batch19 re-homed (`86037ef2e`), tier fix (`5b8c780d6`). See §11 for
> the Phase 1 record and §12 for Phase 2, the batch19 routing and the measured subagent readings.
> Launch floor 191,095 → 3,146 tokens; a converter session 191,095 → 9,348.
>
> **The one-sentence problem**: `CLAUDE.md` is **756,545 bytes / 7,836 lines ≈ 200K tokens**, it is
> re-injected into the context window at the start of every session, after every `/compact`, and into
> every subagent — and the documented target for the file is **under 200 lines**.

---

## 0. What the owner must decide before anything moves

1. **Go / no-go on the two-phase shape** (§6): Phase 1 relocates only and is lossless by construction;
   Phase 2 compresses, incrementally, and can run alongside the 1.24.13 migration.
2. **Ratify the T1 safety list** (§5). This is the part with real risk: a rule that belongs in T1 and
   is filed in T2/T3 instead arrives *after* the damaging command. The list is 12 items; it is the
   only page of this plan that must be read adversarially.
3. **When.** The migration is mid-flight at train 47. This is either "before the next coordinator
   session" (it pays for itself immediately, and the next session is where the current burn happens)
   or "at the next train boundary". It touches no converter, corpus or gate source — only
   instruction files — so it cannot redden a build gate.
4. **Fleet rollout.** `.claude/rules/` and `.claude/skills/` are committed and therefore shared. Lanes
   pick them up on their next fetch, with no per-machine setup.

### Preflight (three facts to establish first)

- **Version — ANSWERED 2026-09-12, the preflight is CLEAR.** The desktop app carries its own version
  line (`1.52386.3`), which is *not* the CLI version the documentation thresholds refer to. The app
  **bundles** the CLI at `%APPDATA%\Claude\claude-code\`, and that bundle is **2.1.266** (with 2.1.260
  alongside) — past every threshold the plan touches: v2.1.206 (`/doctor` trim), v2.1.211 (project
  rules under `--setting-sources`), v2.1.214 (memory `modified` field), v2.1.217 (brace-expansion
  stall), v2.1.239 (`claudeMdExcludes` symlinks).

  Confirmed independently by string census of the bundled executable (219 MB), with both controls
  behaving — positive control `CLAUDE.md` 282 hits, `AGENTS.md` 10; negative control 0:

  ```
    .claude/rules        11        InstructionsLoaded   21
    claudeMdExcludes      5        autoMemoryDirectory   5
  ```

  Every mechanism the plan depends on is present in the shipping build.

  ⚠ **A mid-session probe of path scoping reads NEGATIVE and that reading is VOID.** A
  `.claude/rules/` file created during a session did not fire on a matching Read. Rules are discovered
  **at launch**, so an instrument planted mid-session never ran — the instrument-that-never-compiled-in
  shape. `.claude/rules/_probe-pathscope.md` is left armed and self-announcing; the reading is taken at
  the next session start (gate G5), and the file is deleted once taken.
- **`.gitignore` hazard**: `.claude/worktrees/` is ignored here only through `.git/info/exclude`,
  which is **local and uncommitted**. Committing `.claude/rules/` without first adding
  `**/.claude/worktrees/` to the tracked `.gitignore` risks a lane sweeping hundreds of thousands of
  worktree files into a commit. Verified: `.claude/rules/` and `.claude/skills/` are themselves not
  ignored, so they can be tracked.
- **`InstructionsLoaded` hook**: wire it *before* the migration, so §7's before/after is measured
  rather than asserted.

---

## 1. The measurement

| Artifact | Size | Loads |
|---|---|---|
| `CLAUDE.md` | 756 KB / 7,836 lines ≈ **200K tokens** | every session, every compaction, every subagent |
| `MEMORY.md` index | 13 KB | every session (capped at 200 lines / 25 KB — healthy) |
| memory topic files (48) | 246 KB | on demand only — healthy |
| `docs/phase4/MAILBOX.md` | 15.5 MB / 6,733 entries | only when read (secondary cost) |
| `ConversionStrategies-Reference.md` | 2.4 MB | only when read |

**Growth**: 48 KB on 2026-08-01 → 745 KB on 2026-09-08. **15x in five weeks**, effectively all of it
dated incident entries: 808 `⚠` markers, 452 date references in the two largest sections alone.

**Repetition** (the compressibility signal — the same motifs re-stated across dozens of dated entries):

```
  census 330    stale 119    measured-2026 104    byte-identical 58    disclosure 55
  route #N 46   vacuous 38   positive control 36  the tell 30          one-axis 22
```

**Runtime cost, from the session transcripts** (35 sessions in `~/.claude/projects/`):

```
  cache-read tokens, all sessions          24,666,206,320
  busiest single session                   21,531 turns, avg 579K ctx/turn, max 998K
  that session's cache-read                12,384,800,449 tokens
```

At ~200K fixed, `CLAUDE.md` is **35–40% of an average turn's context**, re-paid on every turn of a
21,531-turn session.

**The proof that it is now actively breaking things**: a `claude-code-guide` subagent spawned on
2026-09-12 died before running — *"the request is ~228,297 tokens (limit 200,000) and this
conversation's own content is most of it."* A subagent inherits the project instructions. **A 200K
CLAUDE.md means no 200K-context subagent can be spawned in this repo at all.**

---

## 2. The mechanism (verified against the Claude Code documentation, 2026-09-12)

Four loading tiers exist. The plan uses all four.

| Tier | Mechanism | When it loads | Notes |
|---|---|---|---|
| **T1** | `./CLAUDE.md` | every session, re-injected after `/compact` | **target < 200 lines** |
| **T2** | `.claude/rules/*.md` with `paths:` frontmatter | **only when Claude reads a file matching the glob** | rules *without* `paths:` load at launch |
| **T3** | `.claude/skills/<name>/SKILL.md` | only when invoked, or when judged relevant to the prompt | the documented home for multi-step procedures |
| **T4** | `docs/**` | only when explicitly Read | the archive |

Three findings that shape the design:

1. **`@path` imports are NOT a lever.** The docs are explicit: *"Splitting into `@path` imports helps
   organization but doesn't reduce context, since imported files load at launch."* An index of
   `@imports` would cost exactly what we pay today. This is the trap to avoid.

2. **HTML comments are stripped before injection.** *"Block-level HTML comments in CLAUDE.md files are
   stripped before the content is injected into Claude's context... When you open a CLAUDE.md file
   directly with the Read tool, comments remain visible."* **This is the answer to "we cannot lose the
   evidence."** Every dated derivation, every measurement, every SHA can stay inline inside
   `<!-- ... -->`, visible to a human reader and to any agent that Reads the file, at **zero token
   cost**. The distilled rule is the visible text; the provenance sits beside it, free.
   *(Verify this holds for `.claude/rules/*.md` as well as `CLAUDE.md` — see gate G4.)*

3. **Path-scoped rules load on file READ, which can be after Claude has already acted.** Therefore a
   rule whose violation is expensive *before* any file is touched — "never overlap two conversions",
   "the source freeze" — **must stay in T1**. T2/T3 take reference detail, never the safety floor.

---

## 3. The target shape

```
CLAUDE.md                        ~180 lines   ALWAYS   orientation + invariants + index
.claude/rules/
  converter.md                   paths: src/go2cs/**            
  harness-gates.md               paths: src/tests/**            
  corpus.md                      paths: src/core/**             
  golib-gen.md                   paths: src/core/golib/**, src/gen/**
  docs-records.md                paths: docs/**                 
.claude/skills/
  gate-forensics/SKILL.md        reading a gate result; false greens; instrument traps
  measurement-discipline/        controls, A/B arms, census predicates, predictions
  train-assembly/                seats, rehearsal, union gates, landing
  merge-hazards/                 silent duplication/subtraction, stale base, ordinals
  mailbox/                       post/read protocol, anchors, push contention
  corpus-reconvert/              the seed → convert → overlay → bucket ritual
  validation-bank/               banked rows, disclosures, roster arithmetic
docs/doctrine/
  JOURNAL-2026-09-12.md          VERBATIM copy of today's CLAUDE.md — the safety net
```

---

## 4. The extraction map

Line ranges are against `CLAUDE.md` at `1800b04f8` (7,836 lines). Every range has exactly one
destination.

### Stays in T1 — `CLAUDE.md` (~180 lines)

| Source | KB | Treatment |
|---|---|---|
| 1–23 header / document-authority ladder | 1 | keep, condensed to 8 lines |
| 24–50 What this is | 2 | keep, condensed to 12 lines |
| 51–189 Architecture map | 13 | **table only**; prose → `docs/Architecture.md` |
| 190–224 One tree | 2 | condense to 10 lines, detail → `rules/corpus.md` |
| 4290–4296 Current-state intro | 0 | condense to 4 lines |
| 5456–5462 Known staleness | 0 | keep |
| 7814–7836 Git anchors | 3 | condense to 6 lines; full table → `docs/Roadmap.md` |
| *(new)* Safety invariants | — | ~15 lines, listed in §5 |
| *(new)* Index of rules/skills/docs | — | ~40 lines |

**261 source lines in, ~180 out.**

### Moves to T2 — `.claude/rules/`

| Destination | `paths:` | Source ranges | KB |
|---|---|---|---|
| `converter.md` | `src/go2cs/**` | 225–771 (flags, 50 KB), 772–831, 832–997 (projitems), 998–1081 (routes 3–5) | 857 lines |
| `harness-gates.md` | `src/tests/**` | 1477–1601 (standalone builds, toolchain resolution), 1602–1770, 2536–2687, 2688–2898 (budget table), 3353–3609 | 914 lines |
| `corpus.md` | `src/core/**` | 3610–3773, 4104–4289, 5389–5455 | 417 lines |
| `golib-gen.md` | `src/core/golib/**`, `src/gen/**` | 1129–1367 (route #7 + twins), 4297–4578 (native-boundary classes) | 521 lines |
| `docs-records.md` | `docs/**` | 5463–5684 (Conventions, incl. the security order) | 222 lines |

### Moves to T3 — `.claude/skills/`

| Destination | Source ranges | KB |
|---|---|---|
| `gate-forensics` | 1082–1128 (route #6), 1368–1443 (route #8), **1771–2535 (71 KB)**, 3115–3151 | 925 lines |
| `measurement-discipline` | 3152–3352, **6598–7270 (63 KB)**, 7271–7573 (28 KB) | 1,177 lines |
| `validation-bank` | 2899–3114, 4579–5388 (75 KB — Phase-3/4 state + disclosure classes) | 1,026 lines |
| `merge-hazards` | 5685–6115, 7574–7813 | 671 lines |
| `train-assembly` | 1444–1476 (source freeze), 6116–6471 | 389 lines |
| `corpus-reconvert` | 3774–4103 (30 KB, two-seeded diff / hunk rule) | 330 lines |
| `mailbox` | 6472–6597 + protocol from `MAILBOX.md` header | 126 lines |

### Moves to T4 — `docs/`

| Destination | Source | Note |
|---|---|---|
| `docs/doctrine/JOURNAL-2026-09-12.md` | **all 7,836 lines, verbatim** | the loss-proof |
| `docs/Roadmap.md` (append) | 4579–5388 historical narrative | Phase 2 only, after distillation |

### 4.1 Coverage proof — gate G1, run against this map

The map was checked before proposal, not after execution:

```
  plan covers 7836 of 7836 lines
  GAPS:      0 lines, 0 runs
  OVERLAPS:  0

  T3 measurement          1177     T2 golib-gen.md          521
  T3 validation-bank      1026     T2 corpus.md             417
  T3 gate-forensics        925     T3 train-assembly        389
  T2 harness-gates.md      914     T3 corpus-reconvert      330
  T2 converter.md          857     T1 CLAUDE.md             261
  T3 merge-hazards         671     T2 docs-records.md       222
                                   T3 mailbox               126
  SUM                                                      7836
```

The first run of this check found **264 lines in five gaps** — route #8, the source freeze, the
standalone-build trap, the toolchain-resolution family, and four section joins. They are assigned
above. A map that had not been arithmetically checked would have silently dropped them.

---

## 5. The T1 safety invariants (the only rules that must arrive before any action)

Draft list — these go in `CLAUDE.md` as one-liners with `<!-- provenance -->` beside each:

1. Never run two conversions into one output root; one converter per box at a time.
2. Seed the temp root from `src/core` before any `-stdlib` reconvert.
3. While any gate battery runs, converter/gen/golib source in that worktree is frozen.
4. Never `Get-Process <name> | Stop-Process` — it kills sibling worktrees' runs.
5. Never add an up-to-date skip to `check-no-regression.ps1`.
6. Pass `GOROOT` exactly as `go env GOROOT` spells it.
7. Announce, then push; never force-push a SHA already posted.
8. Never `git add -A` on a tree that has had a sweep or `-tests` run.
9. A gate that has never been made to fail proves nothing.
10. Read the results-file tail before any mass-empty analysis.
11. One worktree per cut.
12. Measure at the tree; a claim about the code is read at the tree.

---

## 6. Execution — two phases, and Phase 1 is lossless by construction

**Phase 1 — RELOCATE ONLY. No editorial judgment, no compression.**

This is the phase that produces ~95% of the token win, and it cannot lose anything because the
journal is a byte-identical copy of the original.

```
  0. add **/.claude/worktrees/ to the tracked .gitignore   # preflight, §0
  1. git checkout -b claude/context-diet
  2. cp CLAUDE.md docs/doctrine/JOURNAL-2026-09-12.md      # byte-identical, asserted (G2)
  3. split by the §4 line ranges into the T2/T3/T4 files — SCRIPTED from the ranges,
     never by hand; the script re-runs §4.1's coverage check and refuses on any gap
  4. write the new CLAUDE.md skeleton (§3, §5)
  5. run the gates in §7
```

The split is scripted for the reason §4.1 demonstrates: a hand split silently drops lines, and the
first check of this very map found 264 of them.

**Phase 2 — COMPRESS, one file at a time, at leisure.** — **DONE 2026-09-12, see §12.**

For each T2/T3 file: collapse its dated entries into durable rules, moving each entry's dated
derivation into an inline `<!-- -->` block beside its rule. Expect 5–10x per file, based on the
repetition census in §1. This phase is incremental and can be interleaved with the 1.24.13 migration
— no file needs to be done before the migration resumes.

---

## 7. Gates (each one falsifiable, each with a positive control)

| # | Gate | Positive control |
|---|---|---|
| **G1** | **Line accounting**: every one of the 7,836 lines lands in exactly one destination. | Delete one line from a destination; the count must go to 7,835 and the gate must go RED. |
| **G2** | **Journal identity**: `sha256(docs/doctrine/JOURNAL-2026-09-12.md) == sha256(CLAUDE.md@1800b04f8)`. | Append one byte; the hashes must differ. |
| **G3** | **Launch load set**: `/context` → **Memory files** lists only `CLAUDE.md` + unscoped rules. Target **< 10K tokens** at launch, from ~200K. | Read a `src/go2cs/*.go` file; `converter.md` must then appear. |
| **G4** | **Comment stripping**: confirm `<!-- -->` blocks are stripped from `.claude/rules/*.md`, not just `CLAUDE.md`. | Put a distinctive sentence in a comment; ask for it verbatim in a fresh session — it must be absent from context but present via Read. |
| **G5** | **Rules actually fire**: `.claude/rules/_probe-pathscope.md` is **armed** (sentinel `PATHSCOPE-SENTINEL-QX4417`, scoped to `src/go2cs/**/*.go`). Read a converter `.go` file in a **fresh** session. | The sentinel must be absent before that read and present after. A probe planted mid-session cannot fire and its negative is VOID. |
| **G6** | **Subagent viability**: spawn a `claude-code-guide` subagent. | It must run. Today it dies on the context limit; that is the standing RED this plan clears. |
| **G7** | **No doctrine lost to the reader**: `git grep` a sample of 20 distinctive rule phrases. | Each must resolve in exactly one of the new homes. |

**`InstructionsLoaded` hook** is the instrument for G3/G5 — it logs which instruction files load, when,
and why. Wire it before the migration so the before/after is measured rather than asserted.

---

## 8. Expected result

| | Today | After Phase 1 | After Phase 2 |
|---|---|---|---|
| Launch context floor | ~200K tokens | ~3K | ~3K |
| Coordinator session (mailbox/trains) | ~200K | ~3K + skill on demand | ~3K |
| Converter session | ~200K | ~3K + ~20K rules | ~3K + ~5K |
| Worst-case loaded set | ~200K | ~60K | ~20K |
| Subagents spawnable | **no** | yes | yes |

On the busiest observed session profile (21,531 turns), removing a 200K fixed floor is on the order of
**4 billion cache-read tokens saved in that one session**.

---

## 9. Out of scope here, queued separately

- **Mailbox rotation**: `MAILBOX.md` is 15.5 MB / 6,733 entries / 8,550 commits. Archive to
  `MAILBOX-archive-YYYY-MM.md` per train, keep the live file to the current train. Secondary to this
  plan — it is git churn, not per-turn context.
- **Session length**: 21,531-turn sessions multiply every context cost. Bound coordinator/lane
  sessions; prefer the harness's background-task notifications over hand-rolled Bash poll loops
  (6,967 Bash calls in the busiest session).
- **Memory hygiene**: `reflect-lane-state.md` is 64 KB — a lane worklog in the memory directory. Move
  it to a lane doc and run the consolidate-memory pass over the 49 files.

---

## 11. Execution record — Phase 1, 2026-09-12, branch `claude/context-diet`

**Result**: `CLAUDE.md` 770,694 bytes / 7,836 lines / ~200,000 tokens → 14,272 bytes / 196 lines /
**~3,328 effective tokens**. A 60x reduction in the always-loaded floor.

**Gates, all green:**

| Gate | Reading |
|---|---|
| G1 coverage | 7,836 / 7,836 lines, **0 gaps, 0 overlaps** |
| G2 journal identity | **blob-identical**: `docs/doctrine/JOURNAL-2026-09-12.md` and `CLAUDE.md@1800b04f8` are the same git object `4730713b8ae419aa4282379cad291e9f97ea8380`. (Worktree sha256 is `01937a451c97cc7b…`; that reading is CRLF-layer and differs from the LF blob, so the **blob** is the durable claim — name the layer.) |
| G7 round-trip | all 12 destinations byte-for-byte equal to their source ranges; 7,722 relocated verbatim + 114 hand-written = 7,836 |
| guard | `TestContextBudget*` 5/5 pass under go1.24.13 |
| projitems | `TestProjitems*` 3/3 pass, BOM and line endings preserved |
| `.gitignore` | `git add -n .claude/` stages **13 files, 0 worktree paths** |

**Positive control, both directions** (the guard was made to fail before it was believed): appending
25 plain lines to `CLAUDE.md` turns `TestContextBudgetCLAUDEmd` **RED**; the same 25 lines wrapped in
an HTML comment leave it **GREEN**; restore is byte-identical (`sha256 f245dea48a702a43`). That second
arm is not decoration — it is the measurement proving the "provenance in comments is free" claim this
plan rests on.

**Two corrections the gates forced, recorded because both were silent failures:**

1. **The first coverage run found 264 lines in five gaps** — route #8, the mid-battery source freeze,
   the standalone-build trap, the toolchain-resolution family, and four section joins. A hand split
   would have dropped them with nothing to notice.
2. **G7 found the T1 range over-assigned.** Lines 51–189 were filed whole as "Architecture map" and
   condensed into a table; 110 of them are golib byte-cost and alloc-instrument doctrine, and 9 are
   the converter-internals list. Those now go to `golib-gen.md` and `converter.md`. The tell was two
   distinctive phrases (`netpollGenericInit`, `birthday expectation`) resolving to **zero**
   destinations. T1 accordingly drew on 114 source lines, not 261.

**Not done in Phase 1**: no file was compressed. Every destination still carries its source verbatim,
so `harness-gates.md` is 914 lines and `measurement-discipline` is 1,177. Phase 2 distills them; until
it runs, a converter-editing session loads more than it eventually will, and still a fraction of what
it loaded before.

**Self-maintenance landed with it.** `CLAUDE.md` now carries a *This file's own budget* section with
the routing table, and `src/go2cs/internal/repoguard/contextBudget_test.go` enforces it under the plain `go test ./...`
every lane already runs. The cap is measured on **effective** lines, so provenance kept in HTML
comments is free by construction rather than by anyone's restraint.

**Branch exposure measured before execution**: of 364 remote branches, 49 are unmerged and exactly
**3** touch `CLAUDE.md` — `coord-doctrine-batch19` (2026-09-08, +1,355, live: its doctrine must be
re-homed rather than merged), `coord-train30-head` (369 behind) and `netversion-derivation` (1,971
behind). The other 46 merge cleanly and inherit the new structure. A lane on an older branch keeps
loading that branch's `CLAUDE.md` until it rebases, which costs nothing but the old floor.

---

## 10. The risk, stated

The real risk is not token loss — G2 makes loss impossible. It is **a safety rule arriving late**: a
T2 rule loads when a matching file is read, which can be after the damaging command has run. §5 exists
to hold that line, and §5's list is the part of this plan most worth the owner's review. If a rule
belongs in §5 and is filed in §4 instead, the migration trades tokens for an incident.

---

## 12. Execution record — Phase 2, batch19 re-homing and the tier fix, 2026-09-12

Three commits on `claude/context-diet`, all signed: `c8492fe31` (distil), `86037ef2e` (batch19),
`5b8c780d6` (tier relocation). Twelve subagents per phase — which is itself the Phase 1 result,
since before the split no subagent could spawn in this repo at all.

**Phase 2 — distillation.** Dated narratives became tell-first imperative rules; every date, SHA,
file:line, measurement and named incident moved into an HTML comment beside the rule it justifies.

| | before | after |
|---|---|---|
| effective lines, the twelve files | 7,843 | 2,628 |
| effective characters | 746,765 | 325,256 |

Nobody reached the 5–10x target and the reason is uniform: once the narratives are in comments,
what remains is not chronicle but reference — converter.md is a ~110-item flag and route reference,
docs-records.md a normative security order. Two lanes explicitly REFUSED to hit the line target by
reflowing to longer lines, which lowers the line count without lowering token cost. **Report
effective CHARACTERS beside effective lines; the line metric is gameable by wrap width.**

**batch19 — re-homed, not merged.** `claude/coord-doctrine-batch19` carried 1,355 lines as pure
insertions into a `CLAUDE.md` that no longer exists. Each of its 92 hunks was routed to the
destination §4 assigns to its anchor: batch19 is based at `44f858717`, the map is written against
`1800b04f8`, and the only difference is the licensing commit's 12-line insertion at line 735, so
every anchor maps by `L <= 735 ? L : L + 12`. The router asserts the map tiles 1..7836 with no gap
or overlap, refuses any hunk carrying a deletion or any anchor it cannot route, and refuses unless
exactly 1,355 lines route. Nothing landed in `CLAUDE.md`; batch19 touched no T1 range.

1,355 incoming lines produced **284 visible ones (~4.8:1)**, because most entries were new
INSTANCES of traps the destinations already ruled on and belonged in those rules' comments. Twelve
contradictions were found; none was resolved by dropping a side — each amends the visible rule the
newer way and keeps both narratives with an explicit supersession note. Three corrections are worth
reading before quoting anything they touch: the **roster arithmetic** (four of six E1 exclusions
already fall outside axis C, so subtracting all six double-counts), **assembly counts** (the
2026-09-07 `find -newermt` remedy counts FILES — 46,802 against a real 878), and three new normative
**security-census** requirements including a `grep -E` negative lookahead that fails OPEN silently.

**The tier fix.** The remaining gap was allocation, not compression. `.claude/rules/` is lazy per
SUBTREE — `converter.md` loads in full on any read under `src/go2cs/**`. `.claude/skills/` is lazy
per TASK: 421 tokens of name+description for all seven, bodies on invocation only. Gate-reading
material moved to `gate-forensics`, the reconvert loop to `corpus-reconvert`; routes #1–#5 stay in
`converter.md` by design. converter.md 588 → 325, corpus.md 236 → 162.

**What a session pays, in tokens of project instructions:**

| | pre-split | after Phase 2 | after the tier fix | §8 target |
|---|---|---|---|---|
| launch floor | 191,095 | 3,146 | **3,146** | ~3K ✓ |
| converter session | 191,095 | 13,931 | **9,348** | ~5K ✗ |
| worst case, all 5 rules | 191,095 | 44,731 | **42,062** | ~20K ✗ |

The worst case moves least because `harness-gates.md` and `golib-gen.md` are heavily unwrapped —
146 and 160 effective LINES against 63,069 and 41,058 effective CHARACTERS. Re-wrapping them, or
moving more of them to skills, is where the remaining gap lives.

**Subagent laziness, measured rather than assumed** (two arms, one axis — which file was read):

| | read `src/go2cs/convChanType.go` | read `LICENSE` |
|---|---|---|
| path-scope sentinel | **present** | **absent** |
| `converter.md` | loaded, **after** the Read | not loaded |
| other four rules | not loaded | not loaded |

Path-scoped rules DO fire inside subagents, they load ON the triggering read rather than at launch,
and only the matching rule loads. A subagent's floor is `CLAUDE.md` alone.

**Gates.** Every phase: `TestContextBudget|TestProjitems` under go1.24.13 (exit 0, guard count read
from `-v` output, since a `-run` green alone does not prove execution); frontmatter derived
independently of the Go guard and made to fail both ways; journal blob still
`4730713b8ae419aa4282379cad291e9f97ea8380`; evidence-preservation checks against the pre-change
blobs with negative controls. **One gate not in the original list earned its place: HTML comment
balance and nesting.** An unclosed `<!--` silently swallows a file's remainder from context, no
other gate here catches it, and it fired for real when a splice anchor collided with its own
provenance comment and excised nearly twice the intended span.

Two hazards recorded in passing: under `core.autocrlf=true` a `git checkout HEAD -- <file>` restore
rewrites the working copy to CRLF and **`git status` cannot see it**; and a mid-write census counts
the write, not the file — several early readings here were wrong until every agent had exited.
