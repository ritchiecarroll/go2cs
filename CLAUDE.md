# CLAUDE.md — go2cs orientation

> **This file is an INDEX, not a manual.** It holds only what must be true before any file is read.
> Everything else loads on demand: see *Where the rest lives* below.
>
> **Document authority — one ladder.** This file is repo doctrine and gates. The two migration
> **runbooks** — [`docs/GoCorpusMigration.md`](docs/GoCorpusMigration.md) and
> [`docs/DotNetMigration.md`](docs/DotNetMigration.md) — are the **living procedure** for release hops:
> they **lead**, are amended in-stage from lessons learned, and no plan or record overrides them on
> procedure. `docs/PLAN-*.md` hold ruled strategy and instance campaigns; their OQ rulings are settled,
> and only a new ruling reopens one — but a ruling's **SCOPE** is a claim about *who reaches the seam*,
> not a permanent property. `docs/phase4/` RECON-/REHEARSAL-/CENSUS-/DATA-/STAGE0- files are
> point-in-time **records** — amended with dated blocks, never rewritten, never executed from. The
> BOARD is the append-only findings ledger; the mailbox is transport, not record. A lesson lands the day
> it is learned, routed by the table in *This file's own budget*.
> (Doc-type definitions: [`docs/Glossary.md`](docs/Glossary.md), *Document types*.)

## What this is

`go2cs` is a **transpiler that converts Go source into C#** that is both *behaviorally* and *visually*
similar to the original — a Go developer should be able to read the generated C# and follow it. Go's
compiler-provided conveniences (slices, maps, channels, multiple returns, defer/panic/recover,
goroutines, struct embedding, interface duck-typing) are emulated by a hand-written runtime library or
by Roslyn source generators, so the visible converted code stays close to the Go original.

This is **go2cs iteration 2**: the converter is written in **Go**, using `go/ast` + `go/types`. The
earlier C#/ANTLR4 converter is fully retired.

**General working principles** (think before coding, simplicity first, surgical changes) live in the
user-global `~/.claude/CLAUDE.md`. The go2cs-specific discipline: root-cause against the real emitted
`.cs`/`.cs.target` (the golden is the authoritative record); **read the emission BEFORE spending a gate
battery**; keep the A/B footprint minimal; change *only* the goldens a fix must; prove no corpus drift
with `check-no-regression.ps1`. **Compiling is not correctness** — that is the Phase-3 → Phase-4
distinction. And **prefer the durable path over the shortcut**: a converter change over a one-off
hand-patch, a real root cause over a workaround. This does not license speculative machinery; it is
still the *minimal* solution, just the one that generalizes.

## Architecture map

| Component | Location | Language | Role |
|---|---|---|---|
| **Converter** | `src/go2cs/*.go` | Go | Parses Go with `go/ast`/`go/types`, emits C#. |
| **Runtime (`golib`)** | `src/core/golib/` | C# | Go semantics: `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `ж<T>` heap box, `builtin`. Shared by everything; never auto-overwritten. |
| **Source generators** | `src/gen/go2cs-gen/` | C# (Roslyn) | Compile-time Go semantics: interface impl, pointer-receiver overloads, alias conversions, struct embedding. An **analyzer** in every converted project. |
| **Standard library** | `src/core/<pkg>` | C# (converted) | The Go stdlib, auto-converted by `go2cs -stdlib`. Hand-owned: `unsafe`, `testing`. |
| **Behavioral tests** | `src/tests/Behavioral/` | Go + C# | Per-feature Go↔C# equivalence. Counts drift within days — **measure, don't quote**. |
| **Performance tests** | `src/tests/Performance/` | Go + C# | Go vs transpiled C# (JIT and Native AOT). |
| **Examples** | `src/Examples/` | Go + C# | Hand-converted Tour-of-Go / go101 samples. |

**Two solutions, one tree**: `src/go2cs.slnx` is the converter-dev workspace; `src/go2cs-stdlib.slnx`
is every `core/` package. They overlap deliberately. ⚠ **Nothing routinely builds `go2cs.slnx` end to
end**, so a broken solution member rots invisibly — build it once after any golib/runtime API change.

Full taxonomy: [`docs/Architecture.md`](docs/Architecture.md). Converter entry `src/go2cs/main.go`;
`visit*.go` walk AST → C#, `conv*.go` convert expressions/types.

## One tree (read before touching `src/core`)

`src/core` is **the** go2cs standard library. Everything binds `$(go2csPath)core\<pkg>`. There is no
second tree and no path rewriting anywhere. It holds: converted packages (regenerable wholesale),
hand-owned packages (`unsafe`, `testing`), hand-owned FILES inside converted packages (the
`[module: GoManualConversion]` whole-file replacements and `*_impl.cs` companions), and `golib`.
Detail: `.claude/rules/corpus.md`.

## Current state

Phase 3 complete (2026-07-10): the full converted stdlib compiles clean. Phase 4 (operational
validation against Go's own test suites) is live via the `-tests` pipeline, and the corpus is mid-hop
to **Go 1.24.13**. Roster, counts and campaign state are in
[`docs/ValidatedTestPackages.md`](docs/ValidatedTestPackages.md) and
[`docs/phase4/BOARD-next-validation-candidates.md`](docs/phase4/BOARD-next-validation-candidates.md).
**Never quote a count from this file** — it is exactly the kind of fact that goes stale silently.

## The safety floor

These are the only rules that must arrive **before** any file is read. Everything else is one tier down.
Violating one of these costs hours and is usually invisible until much later.

1. **Never run two conversions into one output root**, and never let two overlap on one box. A raced
   conversion corrupts one file with unresolved lift markers and reads exactly like a converter bug.
2. **Seed the temp root from `src/core` before any `-stdlib` reconvert.** An unseeded root silently
   clobbers every hand-owned file with an auto conversion that compiles and is operationally broken.
3. **Pass the output directory as the second positional** for any single-package conversion. It emits
   beside its *input* otherwise; this has written 167 files into a GOROOT and 41 into the repo root.
4. **While any gate battery is running, converter/gen/golib source in that worktree is frozen** — the
   runners rebuild `go2cs.exe` from disk mid-run. Launch long runs from a per-run *copy* of the script.
5. **Never `Get-Process <name> | Stop-Process`.** It matches by name across the whole machine and kills
   a sibling worktree's in-flight suite. Scope kills by executable path, and never by a pattern that can
   match the querying shell.
6. **Spell `GOROOT` exactly as `go env GOROOT` prints it.** A forward-slash path misroutes the entire
   emission into `namespace go.std.*` and **exits reporting success**.
7. **Capture the exit code before any pipe.** `cmd | tail` reports `tail`'s status; a `$(...)` inside the
   line that reads `$?` resets it. Gate every step on the previous one's exit.
8. **Never `git add -A` on a tree that has had a sweep or `-tests` run**, and after any cleanup assert
   `git status --porcelain | grep '^ D'` is empty — a glob has deleted tracked sources three times.
9. **Announce, then push.** Never force-push or replace a SHA that has already been posted; corrections
   land as a commit on top.
10. **Never add an up-to-date skip to `check-no-regression.ps1`.** It re-transpiles unconditionally, and
    that asymmetry is why it is immune to the stale-output false greens.
11. **One worktree per cut**, and one dispatch per worktree. A precondition written in a brief is not a lock.
12. **Preflight free disk before a battery** (the sweep floors at 25 GB) and purge build output between runs.
13. **A gate that has never been made to fail proves nothing.** Regress one site deliberately, confirm it
    names that site, then restore and verify byte-identical.
14. **Read the results-file tail before any mass-empty analysis.** A deadline kill states itself outright.
15. **Measure at the tree.** A claim about the code is read at the ref it claims to describe, after a fetch.
16. **An unfiltered command answers "is it clean"**; a filtered or `head`-limited one answers a different
    question. `| head` is a silent WHERE clause.

<!-- Provenance for the floor, kept here because HTML comments are STRIPPED before this file enters
     context and therefore cost zero tokens. Each item's full derivation, with dates, measurements and
     SHAs, is in docs/doctrine/JOURNAL-2026-09-12.md (the byte-identical pre-split CLAUDE.md) and in
     the topic file that now owns it:
       1  r41 DYNTYPE race, 2026-08-05        -> skills/corpus-reconvert
       2  2026-07-25 false operational alarm  -> skills/corpus-reconvert
       3  2026-08-31 / 2026-09-02, twice      -> rules/converter.md
       4  ruled 2026-08-30; bash byte-offset re-parse 2026-09-04 -> skills/train-assembly
       5  r41 overlap hazard; self-match 2026-09-03 -> skills/gate-forensics
       6  2026-08-24 namespace misroute; env door 2026-08-26 -> rules/converter.md
       7  2026-09-02 three costumes; fourth 2026-09-05 -> skills/gate-forensics
       8  2026-09-03, three instances in one night -> skills/gate-forensics
       9  2026-09-01/02; fast-forward clause 2026-09-02 -> skills/merge-hazards
      10  route #2, fixed 2026-07-20          -> rules/harness-gates.md
      11  2026-09-02 dirt gate; 2026-09-08 shared-worktree purge -> skills/train-assembly
      12  2026-09-03 battery tail aborted at 18 GB free -> rules/harness-gates.md
      13  standing; control-form rules 2026-09-02..06 -> skills/measurement-discipline
      14  2026-08-29, third instance in one week -> skills/gate-forensics
      15  stale-base illusion; three-dot rule 2026-09-04 -> skills/merge-hazards
      16  2026-09-02 repo-root spill; head-as-WHERE 2026-09-07 -> skills/gate-forensics -->

## This file's own budget — read before adding anything to it

CLAUDE.md is **capped at 200 effective lines** and loads in full at the start of every session, after
every `/compact`, and into every subagent. Before 2026-09-12 it was 7,836 lines ≈ 200K tokens per turn,
about 35–40% of an average turn's context, and **no subagent could be spawned in this repo at all**
because the instructions alone exceeded a subagent's context window. Do not let that recur.

**Where a new lesson goes** — decided by where it must ARRIVE, never by how important it feels:

| If the lesson… | It goes to | Loads |
|---|---|---|
| must be known **before any file is read**, and violating it costs hours | the safety floor above | always |
| applies while working in one part of the tree | `.claude/rules/<topic>.md`, with `paths:` frontmatter | when a matching file is read |
| is a **procedure** you invoke | `.claude/skills/<name>/SKILL.md` | on demand |
| is a finding, measurement or campaign ruling | the BOARD or a `docs/` record | never automatically |
| is procedure for a release hop | the runbook, in-stage | never automatically |

**Provenance is free, so keep all of it.** Block HTML comments are stripped before a file enters
context but stay visible to a human and to `Read`. Put the dated derivation, the SHA and the
measurement in a `<!-- -->` block beside the rule it justifies. It costs **zero tokens**. Never delete
evidence to save space — move it into a comment.

**Adding to the safety floor means removing from it.** It is the only always-loaded list and is capped
at 20 items. A 21st means the cap is raised deliberately, here, with the reason recorded.

**`@path` imports do not help.** Imported files are expanded into context at launch, so an index of
imports costs exactly what inlining costs. Path-scoped rules and skills are the only lazy tiers.

`TestContextBudget` in `src/go2cs` enforces these caps under the plain `go test ./...` every lane
already runs. A budget that lives in attention rather than in a guard fails under exactly the
conditions the guard exists for.

## Where the rest lives

**Path-scoped rules** (`.claude/rules/`) load when Claude reads a matching file:

| File | Loads when you touch | Covers |
|---|---|---|
| `converter.md` | `src/go2cs/**` | converter flags, `-stdlib`/`-tests`/`-recurse`, projitems registration, stale-binary routes #1–#5 |
| `harness-gates.md` | `src/tests/**` | behavioral runner, MSTest, goldens and re-baselining, measured budget table, toolchain resolution |
| `corpus.md` | `src/core/**` | the one tree, hand-owns, layout L3, cgo state, deployment |
| `golib-gen.md` | `src/core/golib/**`, `src/gen/**` | route #7 and its twins, the native-boundary classes, pin/lifetime rules |
| `docs-records.md` | `docs/**` | document authority, conventions, coding style, the security order |

**Skills** (`.claude/skills/`) load on demand — invoke by name or let relevance pull them in:

| Skill | Use when |
|---|---|
| `gate-forensics` | a gate, build or test result looks wrong: false-green routes, instrument traps, mass-empty signatures |
| `measurement-discipline` | designing or judging a measurement: controls, one-axis arms, census predicates, predictions |
| `validation-bank` | banking a roster row, minting or judging a disclosure, reading roster arithmetic |
| `merge-hazards` | merging or rebasing lane branches: silent duplication and subtraction, stale bases |
| `train-assembly` | assembling, rehearsing, gating or landing a train |
| `corpus-reconvert` | measuring a converter change's corpus footprint: the two-seeded diff and the hunk rule |
| `mailbox` | posting to or reading the fleet mailbox |

**Records**: [`docs/phase4/BOARD-next-validation-candidates.md`](docs/phase4/BOARD-next-validation-candidates.md)
(findings ledger) · [`docs/ValidatedTestPackages.md`](docs/ValidatedTestPackages.md) (roster of record) ·
[`docs/ConversionStrategies.md`](docs/ConversionStrategies.md) → its
[reference](docs/ConversionStrategies-Reference.md) (how each Go construct maps) ·
[`docs/Glossary.md`](docs/Glossary.md) (CNR, census, chip, guard, golden, banked…) ·
[`docs/Roadmap.md`](docs/Roadmap.md) (phases and git anchors) ·
[`docs/coding-style.md`](docs/coding-style.md).

**The journal**: [`docs/doctrine/JOURNAL-2026-09-12.md`](docs/doctrine/JOURNAL-2026-09-12.md) is the
byte-identical pre-split CLAUDE.md. Nothing was lost in the split; if a rule reads thin in its new
home, its full dated derivation is there.
