# REHEARSAL — hop A (Go 1.23.1 → 1.23.12), dry run

> **State: EXECUTED (2026-08-24).** Reviewed and acted on — every finding in §6 carries its
> disposition in the sweep appended at the end of this document. Amended, never rewritten: the body
> below stands exactly as the rehearsal lane wrote it, including the parts its own findings later
> closed.
>
> ⚠ **Line-anchor note.** This document cites `PLAN-hop-campaign.md`, `RECON-go12312-diff.md`,
> `LANES.md` and `ValidatedTestPackages.md` **by line number**. Those citations resolve against
> those files **at this document's banking commit** (`27fe4632e`) — `git show 27fe4632e:<path>` —
> not against today's tree: the consolidation that acted on these findings added status banners and
> supersession notes that shift their lines. The *content* each citation names is still there; only
> the numbers moved.

Date: **2026-08-24** · Clone: `C:\Projects\go2cs` · Branch `master` @ `af84003f9` · tree **clean before and after**
Toolchain on this box: `go1.23.1 windows/amd64` (`GOROOT=C:\Program Files\Go`, `GOROOT/VERSION` = `go1.23.1`)
Pin: `<GoStdLibVersion>1.23.1</GoStdLibVersion>` · `<GoBuildNumber>7</GoBuildNumber>` (`src/version.props:23-24`)

**Constraints honored.** No `version.props` edit. No regen. No migration applied. No sweep, no behavioral
suite, no build. One read-only census run, read-only greps, and two read-only GitHub API calls.
The .NET 10 hop running in parallel on `D:\Projects\go2cs` was not touched.

---

## Board

| # | Rehearsal item | Verdict | Blocking? |
|:--|:--|:--|:--|
| 1 | `migrate-gorelease.ps1` census @ 1.23.12 | **PASS** — exit 0, 20 sites / 8 files, **UNCLASSIFIED: none**, tree clean | no |
| 2 | H3 package census = ∅ | **CONFIRMED INDEPENDENTLY** — 0 removed, 21 added, none creates a converted package | no |
| 3 | Four HIGH rows characterized | **DONE** — all four verified, proof pages agree. ⚠ `syscall` and `time` have **no disclosure manifest** and compare strictly | **`time` is a live risk** |
| 4 | Shard map readiness | **PARTLY READY** — computed but **unbanked** (draft + generator live only in a session scratchpad); every non-i9 speed factor is a self-declared placeholder | **yes, for W=3 dispatch** |
| 5 | H-series dry walk | **walked; three doc-staleness findings + one instrument gap (H6)** | H6 gap is a gate risk |

**Four findings want a coordinator ruling before the hop fires** — §6. The sharpest is that
**false-green route #4 is CLOSED in code but still documented as open in three places**, one of which
is the migration instrument's own printed operator guidance.

---

## 1. The instrument, exercised in census mode

### 1.1 Command and result

```
powershell -File src\migrate-gorelease.ps1 -To 1.23.12
```

**Exit code 0.** 152 lines of output (full capture: `census-raw.txt` beside this file).
`git status --short` **empty** before and after — the census wrote nothing, as designed.

Header block it printed:

```
  repository   C:\Projects\go2cs
  pinned       1.23.1   (<GoStdLibVersion>, src/version.props)
  target       1.23.12
  toolchain    1.23.1   (via GOROOT/VERSION)
  mode         CENSUS (read-only)
```

### 1.2 Editable sites — the two classes `-Apply` would touch

**20 occurrences across 8 files**, from **15 named anchors** (13 DOC-STATEMENT + 2 SOURCE-OF-TRUTH,
verified by counting the `$editableSites` table directly):

| File | Class | Sites | Anchor note (verbatim from output) |
|:--|:--|--:|:--|
| `src/version.props` | SOURCE-OF-TRUTH | 1 | THE pin. Every runtime guard and the whole published version derive from this one element |
| `src/go2cs/go.mod` | SOURCE-OF-TRUTH | 1 | the converter module's go directive (H1.2, ruled: it moves each migration) |
| `CLAUDE.md` | DOC-STATEMENT | 1 | architecture row |
| `docs/README.md` | DOC-STATEMENT | 1 | side-by-side sample section lead-in |
| `docs/README.md` | DOC-STATEMENT | 1 | side-by-side sample table header |
| `docs/README.md` | DOC-STATEMENT | **6** | the six blob links the sample table points at |
| `docs/README.md` | DOC-STATEMENT | 1 | present-tense compile claim |
| `docs/README.md` | DOC-STATEMENT | 1 | the "Try it yourself" prerequisite |
| `docs/ValidatedTestPackages.md` | DOC-STATEMENT | 1 | roster preamble |
| `docs/ValidatedTestPackages.md` | DOC-STATEMENT | 1 | the denominator's definition |
| `docs/ValidatedTestPackages.md` | DOC-STATEMENT | 1 | the per-OS column rule names its era |
| `docs/Roadmap.md` | DOC-STATEMENT | 1 | the converter-improvement loop names the toolchain |
| `docs/Background.md` | DOC-STATEMENT | 1 | the published package version family on nuget.org |
| `docs/Background.md` | DOC-STATEMENT | 1 | the completion-goal denominator's definition |
| `docs/ConversionStrategies.md` | DOC-STATEMENT | 1 | the release the strategy summary draws snippets from |

Arithmetic closes: 1+1+1+10+3+1+2+1 = **20** across **8** files. ✔

### 1.3 The non-editable classes

| Class | Files | Occurrences |
|:--|--:|--:|
| DERIVED-BY-REGEN | **471** | 817 (651 README badge lines across 305 files · 162 proof pages · 3 zbootstrap · 1 `core/VERSION`) |
| DERIVED-AT-RUNTIME | 2 | 8 (`push-nuget.ps1` ×7 · the sweep's pin block ×1) |
| MUST-NOT-CHANGE | 758 | 1,102 (dominated by 653 write-once published snapshots) |
| **UNCLASSIFIED** | **0** | **0** — *"every tracked occurrence falls in a named class"* |
| Whole-tree tracked total | — | **1,963** |

`unanchored occurrences … REVIEW` printed **`none — every occurrence in the doc-statement files is
classified`**, and all **9** history anchors were found present (3 in CLAUDE.md, 3 in `docs/README.md`,
3 in `docs/Roadmap.md`).

### 1.4 Does it match "the census in its own header"?

The `.SYNOPSIS`/`.DESCRIPTION` block carries **no numbers** — it defines the five classes only, and
the run's class structure matches it exactly. The numeric census lives in the two **commit messages**,
and both reconcile:

| Claim | Source | This run | Reconciliation |
|:--|:--|:--|:--|
| DERIVED-BY-REGEN "~470 files" | `e9eba2bb2` | **471** | ✔ exact |
| "fifteen count mismatches" (⇒ 15 anchors) | `c9d46b712` | **15** anchors | ✔ exact |
| "**21 edits** across 8 files" | `c9d46b712` | 20 sites / 8 files | ✔ — the 21st edit is `<GoBuildNumber>` 7→0, which contains **no release string** and so is not a "site". Confirmed in code: the reset is applied at `src/migrate-gorelease.ps1:888-895`, outside the site loop, and `$landed` (line 924) counts release-string sites only |
| "0 remaining / **20 landed**" | `c9d46b712` | **20** | ✔ exact |
| DOC-STATEMENT "**14** named anchors" | `e9eba2bb2` | **13** | ✘ **off by one** — cosmetic prose figure in a superseded commit message; the live table has 13. No action beyond noting it |

### 1.5 ⚠ Correction to the task's premise — the instrument **has** already been run against 1.23.12

The task brief assumed *"the instrument has never been run against a real target version."* It has.
Commit `c9d46b712` (**today**, 08:20 CDT) records a **full `-Apply` at 1.23.12** performed
*"in a throwaway worktree, never in the working tree"*, with three defects found and fixed:

1. **False red on re-run** — every anchor's count was asserted unconditionally, so a census over an
   already-migrated tree reported 15 mismatches and exit 1. Sites now resolve pending / migrated /
   mismatch, and only mismatch fails.
2. **Line-ending damage** — the `go.mod` anchor's `\r?$` guard *consumed* the CR and rewrote that line
   to LF. Now a lookahead `(?=\r?$)`.
3. **Review-section noise** — `1.23.1` is a **prefix of** `1.23.12`, so every successfully migrated
   line tripped the unanchored scan (`src/migrate-gorelease.ps1:640-648`).

**So this run is a confirmation on the real tree, not a maiden voyage** — which is the better news, but
the two are different claims and the report should not inherit the weaker one. What remains genuinely
un-rehearsed: `-Apply` **in the working tree**, and the `-Apply` → CNR → regen sequence end to end.

**Assessment: the instrument is hop-ready.** The prefix hazard (2) is the one I would have expected to
bite and it is already fixed and commented in place.

---

## 2. H3 package census — **∅ confirmed independently**

Not taken on the recon's word. Re-derived from the upstream API on this box
(`gh` authenticated as `ritchiecarroll`):

```
gh api repos/golang/go/compare/go1.23.1...go1.23.12
  → {"commits":83, "files":160, "status":"ahead"}
```

**File status breakdown: 139 `modified`, 21 `added`, 0 `removed`.**

**Zero removed files** settles the "package removed" half outright. All 21 added files, in full:

| Bucket | Count | Bearing on the corpus |
|:--|--:|:--|
| `src/cmd/**` testdata (cgo/testcarchive, testsanitizers, compile/importer, go/testdata, link/testdata) | 11 | **none** — `cmd/*` is never converted |
| `test/fixedbugs/**`, `test/codegen/**` | 7 | **none** — outside `src/`, compiler suite |
| `src/runtime/testdata/testprogcgo/callback_pprof.go` | 1 | none — testdata |
| `src/net/sendfile_unix_test.go` | 1 | `_test.go` in an **existing** package |
| `src/os/copy_test.go` | 1 | `_test.go` in an **existing** package |

Some additions create new **directories** (`.../testsanitizers/testdata/asan_global_asm/`,
`.../link/testdata/linkname/textvar/`) — all under `cmd/` testdata, which the conversion queue never
reaches. **No package directory is added or removed anywhere the converter looks.**

> **H3 prediction upheld. The census will come back ∅, and that is now measured twice, by two
> independent derivations, rather than predicted once.**

### 2.1 Recon cross-check — agrees where it matters, +1 where it does not

| Reading | RECON (per-commit enumeration) | This run (compare endpoint) | Δ |
|:--|--:|--:|:--|
| Commits | 83 | **83** | — |
| Unique files | 161 | 160 | **+1** |
| Under `src/` | 150 | 149 | **+1** |
| Outside `src/` | 11 | **11** | — |
| `src/cmd/**` | 49 | 48 | **+1** |
| runtime tree | 42 | **42** | — |
| **Stdlib-visible remainder** | ~59 | **59** | — |

**The discrepancy is resolved exactly.** The recon's raw `files-unique.txt` survived in this session's
shared scratchpad, so I diffed the two sets directly:

```
comm -23 recon.txt mine.txt   →  src/cmd/internal/moddeps/moddeps_test.go
comm -13 recon.txt mine.txt   →  (empty)
```

**My set is a strict subset of the recon's**, and the single missing file is
`src/cmd/internal/moddeps/moddeps_test.go` — a `cmd/` test file, **never converted, zero corpus
bearing**. This is precisely the truncation the recon predicted (*"the compare endpoint caps its files
array, so per-commit enumeration is the ground truth"*).

> **The recon is authoritative and correct. Use its 161 / 150 / 49 for the record**; my 160/149/48 is
> the same data minus one `cmd/` file. Every corpus-relevant number — 59 stdlib-visible, 42
> runtime-tree, 11 outside `src/`, and the 0-removed / 0-package-adding verdict — **agrees exactly
> across two independent derivations.**

### 2.2 GOROOT-side baseline (capturable now, for H0)

- Converted corpus: **306** production `.csproj` under `src/core` (excluding `*.tests.csproj`)
- GOROOT 1.23.1 non-`cmd` package dirs holding non-test `.go`: **391**
- **No 1.23.12 tree present on this machine** — `~/sdk` holds only `go1.18`. H1 provisioning is
  entirely outstanding here.

### 2.3 ⚠ The recon's raw data is **unbanked**, not lost — but it is one `rm` from lost

`RECON-go12312-diff.md:193` closes with *"Raw TSVs beside this file for re-derivation."*
**They are not in `docs/phase4/`.** They *are* alive in this session's shared scratchpad —
`commits.tsv` (83 rows), `files-by-commit.tsv`, `files-unique.txt` (**161**),
`src-files-classified.tsv` (**150**), `roster.txt` — dated 2026-08-23 06:35, and their counts match
the recon's headline exactly, which is how §2.1 resolved.

So the promise is *nearly* kept: "beside this file" is false, and the data lives in a **volatile
session scratchpad** rather than in the repo. That matters more than it sounds, because §4's
golden-drift triage rests on *"every moved golden maps to one of those commits by name, or it is a
defect"* — the per-commit file map is a **working input to the hop**, not an archived artifact. This
is the same retention hazard §3.2 already rules on for per-row wall times (*"unrecoverable
afterward … make it an obligation of that sweep"*).

**Recommend banking `commits.tsv` + `files-by-commit.tsv` into `docs/phase4/` before H5**, or amending
the closing line to say where they actually are. Cost is trivial and `gh` auth is present for
re-derivation either way. This run's data is in `h3-files.tsv` beside this report.

---

## 3. The four HIGH-attention rows

All figures re-verified directly against the tree (roster row, `git grep` line-anchored marker census,
manifest entry count). **All four proof pages agree with their roster rows.** Roster grammar
(`ValidatedTestPackages.md:81-89`): *"The **Tests** and **Disclosed** columns are the Windows record
for the Go 1.23.1 era"*; a validated non-Windows OS records a `linux: N + D` annotation.
**None of the four carries one** — the whole HIGH set is a Windows-only record, consistent with
`RECON:85-88` (the 7 Linux-annotated rows intersect the upstream-touched set nowhere).

| Package | Roster | Matched | Disclosed | Marked hand-owns | `.cs.auto` | Manifest | Proof page |
|:--|:--:|--:|--:|--:|--:|:--|:--|
| `syscall` | `:268` | **62** | — (0) | **13** | 3 | ⚠ **none — compares strictly** | `syscall.md` 62·0 ✔ |
| `os/exec` | `:251` | **74** | **27** | **0** | 0 | 25 entries (+2 aggregate on page) | `os.exec.md` 74·27 ✔ |
| `database/sql` | `:156` | **137** | **2** | **0** | 0 | 2 entries | `database.sql.md` 137·2 ✔ |
| `time` | `:276` | **159** | — (0) | **1** | 1 | ⚠ **none — compares strictly** | `time.md` 159·0 ✔ |

### 3.1 `syscall` — 62 matched, 0 disclosed, **13 hand-owns**, no manifest

**What re-validation must look at: a production behavior change under a hand-own, plus a moved test file.**

- **Hand-own overlap is real and file-level.** 13 marked paths; 10 are `*_impl.cs` companions (no Go counterpart ⇒ **no `.cs.auto`** ⇒ audited against their principal's diff). The 3 whole-file hand-owns that *do* carry a `.cs.auto` review sibling: `syscall/linux/exec_unix.cs:34`, `syscall/windows/dll_windows.cs:32`, `syscall/windows/exec_windows.cs:41`.
- **Two of the recon's named overlaps confirm exactly**: upstream `dll_windows.go` (+1 `//go:noescape`) ↔ `windows/dll_windows.cs:32`; upstream `exec_unix.go` (+4) ↔ `linux/exec_unix.cs:34`. The third, `syscall_windows.go`, is a **converted** file sitting beside the hand-owned `syscall_windows_impl.cs:91` — package adjacency, **not** a file-level overlap. `windows/exec_windows.cs` is a hand-own the recon does *not* list as upstream-changed ⇒ should be inert this hop, and the differential must say so rather than skip it.
- **`darwin/` has zero marked files**, including `darwin/exec_libc2.cs` and `darwin/exec_unix.cs` which the recon flags as upstream-changed. Pure converted ⇒ they regen with no review row.
- **Test that can move the 62**: `syscall_windows_test.go` +45 (`847cb6f9c`) → banked counterpart **`src/core/syscall/syscall_windows_test.cs` exists**.
- ⚠ **The production risk is CVE-2025-0913** (`O_CREATE|O_EXCL` don't-follow-symlinks) landing in `syscall_windows.go` — the **validated platform's** production behavior — and **there is no manifest**. Any divergence is a hard mismatch with no absorption path.

### 3.2 `os/exec` — 74 matched, **27 disclosed**, zero hand-owns

**What re-validation must look at: 25 signature pins whose fragility is in the NAME, not the signature.**

- **Zero marked files, zero `*_impl.cs`, zero `.cs.auto`** — recon's "no marked files in os/exec" confirmed. Everything (`windows/lp_windows.cs`, `linux/lp_unix.cs`, root `exec.cs`) regens freely, so **H6 has no row here**; this row's risk is entirely H10's.
- **Manifest: 25 entries, all `class: "host-limit"`, and every one pins the identical signature `"exit status 0x8000809a"`** — a .NET hostfxr `LibHostAppRootFindFailure` constant. The 2 aggregate parents (`TestCommand`, `TestLookPathWindows`) are **not** in the JSON; they exist only on the proof page as `aggregate`. 25 + 2 = the roster's 27, exactly as the roster cell narrates.
- ⚠ **Inverted fragility, and it is the interesting finding.** The usual hazard is signature drift; here the signature is a .NET constant that upstream cannot touch. **The exposure is subtest-NAME drift**: 14 of the 25 pins hang off `TestCommand/<subtest>` labels **generated from the very LookPath/dot table that go1.23.11's security fix rewrites** (`dot_test.go` +56, `exec.go` `lookPath` `""`/`.`/`..` expansion). A renamed or re-cased label breaks the pin by name while the signature stays valid.
- **Sharpest verdict-movement risk in the pack:** `lp_windows.go` **+8 is a production security fix on the validated platform**, and `lp_windows_test.cs` is **not** in the upstream-changed set — a production change probed by an **unchanged** test suite.
- ⚠ **Recon inconsistency to correct.** `RECON:151` attributes `exec_posix_test.go` +56 to `os/exec`; `RECON:44` puts `exec_posix.go` under **`os`**. Upstream the file is `src/os/exec_posix_test.go`. There is **no banked `exec_posix_test.cs` anywhere under `src/core/os/`**, and `os` is not a roster row. The recon's *conclusion* ("likely linux-gated → Windows count may not see it") is safe; the attribution is wrong.

### 3.3 `database/sql` — 137 matched, 2 disclosed, zero hand-owns

**What re-validation must look at: the most rewording-fragile pin in the pack, in a file upstream just edited.**

- **Zero marked files, no `*_impl.cs`, no per-GOOS subdirectories** — flat package (`sql.cs`, `convert.cs`, `ctxutil.cs`). Production `sql.go` 12/14 and `convert.go` −2 are plain regen targets. **No H6 row.**
- **Manifest: 2 entries, both `class: "alloc-profile"`** —

  | Test | Signature pinned (verbatim) |
  |:--|:--|
  | `TestGrabConnAllocs` | `"Conn.grabConn allocated "` |
  | `TestRawBytesAllocs` | `"allocs = "` |

- ⚠ **`"allocs = "` is nine generic characters of an upstream `t.Errorf` format string** — not a go2cs message — **in `sql_test.go`, which upstream changed +56/8** (`8a924caaf`). Any rewording (`allocs=%v`, `got %v allocs`) silently invalidates the pin, and it is generic enough to collide with any other test in the package that emits the same prefix. **This is the single most rewording-fragile pin across the four**, and the runbook's rule applies at full force: **re-derive and re-sign, never edit the signature to match** — editing converts a real, re-derivable divergence into a rubber stamp.
- **Both upstream-changed test files have banked counterparts** — `sql_test.cs` and `fakedb_test.cs`. The only package of the four where the mapping is 1:1. ⚠ **`fakedb_test.go` was *reworked*, not merely extended**, and it is the driver harness **all 137 verdicts** run against — so it can move counts in tests whose own source did not change.
- Production risk: the real Rows/Scan race fix (avoid closing `Rows` while a `Scan` is in progress).

### 3.4 `time` — 159 matched, **0 disclosed**, 1 hand-own, no manifest — **the sharpest row**

**What re-validation must look at: new upstream tests probing semantics implemented by a hand-own that will not regen, with no manifest to absorb a failure.**

- **Upstream change is test-only** (`sleep_test.go` +115 over 3 commits — `3b2e846e1`, `8d79bf799`, `58babf6e0`; `time_test.go` a TZ-accept fix that is **OpenBSD-only** ⇒ inert for the windows/amd64 record but still re-converts). Both have banked counterparts (`sleep_test.cs`, `time_test.cs`).
- **One marked hand-own: `src/core/time/tick.cs:44`**, with a committed `tick.cs.auto` sibling. It is at package root and **platform-neutral** — no GOOS subdirectory holds a mark.
- ⚠ **`src/core/time/time_impl.cs` is an impl companion the marker census does NOT count** — its module attribute is `[module: go.GoRequiresUnsafe]` (`time_impl.cs:13`), present because `[LibraryImport]` requires `/unsafe` (SYSLIB1062), **not** `GoManualConversion`. Anyone reconciling this package against a census total should not expect it there.
- ⚠ **The coupling that makes this the sharpest row.** The changed `sleep_test.go` probes **Stop/Reset result semantics**. `tick.go`'s own source is **not** upstream-changed — so `tick.cs`, the hand-written managed timer, **will not regen to pick up upstream's fix**; upstream's production fix lives in `runtime/time.go`, which go2cs's timers do not track line-for-line. The recon (`RECON:76`) puts it exactly right: the new tests are *"a genuine behavioral probe of go2cs's managed timers … pass is not guaranteed."*
- ⚠ **And there is no manifest.** `time` compares strictly. If the new assertions find the same bug **shape** upstream fixed (Reset/Stop returning the wrong result when racing a running timer), the only outcomes are **fix the managed timer** or **create a manifest from scratch** — and a timer-correctness failure does not obviously fall into any existing disclosure class. **This row can turn the hop into converter/runtime work.** Recommend H6 check the managed implementation for the bug shape *proactively*, per `RECON:109-113`, rather than discovering it at H10.
- Oldest validation of the four: proof page stamped `Validated 2026-08-04 · converter f96e7de5b`.

### 3.5 What falls out, for the coordinator

1. **Two of the four (`syscall`, `time`) have no manifest and compare strictly.** A count move in either is a hard failure with **no absorption path** — and `time` is precisely the row the recon says may not pass.
2. **`database/sql`'s `"allocs = "` pin** is the most likely to break by rewording, in a file upstream just edited.
3. **`os/exec`'s fragility is inverted** — name-fragile, signature-stable — which means a signature-oriented triage would look at the wrong half.
4. **H6 rows exist for only two of the four** (`syscall` ×13, `time` ×1); `os/exec` and `database/sql` are pure H10 risk.

---

## 4. Shard-map readiness

### 4.1 Headline: **the map is computed but UNBANKED — only its summary reached the repo**

`docs/phase4/SHARDMAP-go1.23.12.md` **is not in the repo** (verified: no `SHARDMAP*` under
`docs/phase4/`). But the work is largely **done**: a 28 KB computed draft, `shard-map-draft.md`
(2026-08-22), plus its generator `shardmap.py`, are alive in this session's shared scratchpad. Its
section headings show a real per-worker deal exists:

```
## 1. The dataset — JOB-007, parsed
## 3. The assignments                ← the per-worker deal
## 4. Analysis  (4.1 dominance · 4.2 reserved-set makespan · 4.4 factor sensitivity)
## 5. Ready-to-paste — proposed insertion for PLAN-hop-campaign.md §4.3
###   4.3.1 The computed map at the JOB-007 reading — DRAFT, pending recon calibration
## Appendix — reproduction
```

**Only §5's paste-ready block reached the repo** — that is verbatim what `PLAN-hop-campaign.md`
§4.3.1 now holds. The **assignments themselves (draft §3) were never banked.** So the readiness gap
is narrower than "no map exists", and differently shaped: the computation is done and reproducible
(the generator and an Appendix are right there), but the only durable copy of the deal is in a
volatile scratchpad — the **same retention hazard as §2.3**, and the second instance of it in this
rehearsal.

**Recommend banking `shard-map-draft.md` and `shardmap.py` into `docs/phase4/` now**, clearly marked
DRAFT-at-placeholder-factors. It costs nothing, and it converts "must recompute at dispatch" into
"must re-run the generator with measured factors."

§4.3.1's own heading is candid about the rest:

> `### 4.3.1 The computed map at the JOB-007 reading — DRAFT, pending recon calibration`
> *(`docs/PLAN-hop-campaign.md:452`)*

What §4.3.1 actually contains is a **makespan-by-fleet-size projection**, not a worker→rows deal:

| `W` | Fleet | Makespan | Binding bin |
|:--:|:--|--:|:--|
| 3 | i9 + R + coordinator | ~4,289 s (71.5 min) | all three balanced |
| 4 | + G | ~3,956 s (65.9 min) | i9's reserved set alone |
| 5 | + one engaged machine | ~3,956 s (65.9 min) | i9's reserved set alone |

The per-row deal *"is emitted to `SHARDMAP-go1.23.12.md` at dispatch."*

### 4.2 What must be re-measured at hop time

| Input | Status | Source |
|:--|:--|:--|
| **`s_w`, every worker but the i9** | ⚠ **PROVISIONAL placeholder**, stated as such: i9 = 1.00 by definition, R 6850U = 0.45, i7-5820K = 0.35, G 6650U = 0.35, "any fifth worker" = 0.35 **assumed** | `PLAN-hop-campaign.md:454-463` |
| **`k`** (load factor) | ⚠ **assumed 1**, *"placeholder — the hop supplies it"*; measure on the recon phase's first ten rows | `:448` |
| **Roster row count** | 162 today, *"re-read, never carried"* | `:433` |
| **The calibration pair** | `run-behavioral.ps1 --filter Atomic` + `run-validated-sweep.ps1 -Filter 'container/heap'`, *"reported with the worker's first shard"* | `:446` → `LANES.md:37-39` |

**The plan's own dispatch gate** (`:481`): *"At W = 3 it is capacity-bound and every factor error
passes through — a W=3 dispatch waits for real calibration; W≥4 tolerates placeholders."*

⚠ **The calibration pair is weak and under-specified.** At its home in `LANES.md:37-39` it is labelled
an *"Optional 10-minute baseline"* with no recording location, no repetition count and no output
format — yet §4.3:446 elevates it to the authoritative source of measured `s_w`. And
**`container/heap` is a 7-second row** on the i9 (23 s under Linux/WSL2): most of that is fixed
convert/build overhead, not throughput, so it will not reliably discriminate a 0.45 box from a 0.35
box. **Recommend picking a mid-weight row** (e.g. `go/parser`, 259 s i9) **and stating the protocol**
before the hop leans on it.

### 4.3 What is already banked — **both legs, 162/162**

`docs/phase4/DATA-sweep-row-walltimes.md` carries **two complete sections**, both at the roster's full
162 rows (roster header: *"162 / 215 testable packages validated — 75.3%"*, `ValidatedTestPackages.md:73`):

| Leg | Header | Rows | Aggregate | Derivation quality |
|:--|:--|--:|--:|:--|
| **windows** | `corpus 18770d083 · i9-13900K (ritchie-desk2) · 2026-08-23 (JOB-007)` | 162 (all PASS, 18,569 verdicts) | 7,697 s | ⚠ **mtime-derived**, not directly measured; self-check 7,701 vs 7,697 s |
| **linux** | `corpus 18770d083 · Ryzen 7 PRO 6850U (R-LAPTOP, WSL2 Ubuntu 22.04) · 2026-08-23 (JOB-007 Linux leg)` | 162 (149 PASS / 10 FAIL / 3 CVAC) | **19,113 s (5.3 h)** | ✔ **directly instrumented per row** — the better data |

### 4.4 ⚠ **The Linux column exists — and the plan does not know it**

`PLAN-hop-campaign.md:501-503` still reads:

> the Windows leg's per-row walls are **banked** … **the Linux leg's ledger joins the same file when it posts.**

**It posted** — commit `861475db0`, 2026-08-23, same file, same corpus SHA `18770d083`, the same day
§4.3.1 was drafted. The projection in §4.3.1 is therefore parameterized on the **Windows leg alone**
while ignoring measured Linux data that its own source file warns *differs*:

> *"`crypto/dsa` is the largest row here too but three times worse — **4,366 s for 4 verdicts** against
> Windows' 1,317 s — and `hash/maphash` 1,994 s for 22 against 898 s. The Linux/Windows wall ratio is
> ~2.5x overall but is NOT uniform … a shard planner should treat [FAIL rows] as full-cost."*
> *(`DATA-sweep-row-walltimes.md:192-198`)*

**Structural note:** Linux is a separate **section**, not a **column**. There is no side-by-side
per-row Windows/Linux table anywhere, so the plan's row-cost model `t_r` is **not OS-aware**. For
`crypto/dsa` — the single row that dominates the makespan — the choice of leg is a **3.3x** difference.
**This needs an explicit ruling before the map is emitted.**

### 4.5 ⚠ The reserved set misses two `$longTimeouts` rows and misquotes two floors

`src/run-validated-sweep.ps1:495`, verbatim:

```powershell
$longTimeouts = @{ 'hash/maphash' = '60m'; 'index/suffixarray' = '120m'; 'crypto/dsa' = '120m'; 'archive/zip' = '60m'; 'go/parser' = '90m'; 'crypto/internal/mlkem768' = '30m'; 'crypto/tls' = '30m' }
```

Against the plan's reserved set (`PLAN-hop-campaign.md:417-428`):

| Package | Script floor | Plan says | Pinned to the i9? |
|:--|:--|:--|:--|
| `index/suffixarray` | 120m | 120 m ✔ | yes |
| `hash/maphash` | 60m | 60 m ✔ | yes |
| `crypto/tls` | 30m | (floor not stated) | yes |
| `crypto/dsa` | **120m** | "60 m floor" ✘ | yes |
| `archive/zip` | **60m** | "30 m floor" ✘ | yes |
| **`go/parser`** | **90m** | **absent** ✘ | **NO — dealt blind to bulk** |
| **`crypto/internal/mlkem768`** | **30m** | **absent** ✘ | **NO — dealt blind to bulk** |

Neither `go/parser` nor `mlkem768` appears anywhere in `PLAN-hop-campaign.md`, so under §4.3 step 4
they LPT-deal to a 0.35 worker. **This is not hypothetical** — the script's own comment block
(lines 488-494) records exactly that failure:

> *"151-row sweep of 2026-08-19 (laptop R, 340 min) zip blew 30m and parser blew 40m — both as
> one-sided-row truncations reading like divergences — while the SAME DAY, SOLO on the same machine,
> zip measured 850 s and parser 836 s."*

And `go/parser` is **259 s** on the i9 but **777 s** under Linux. `git log -L` on line 495 dates the
raises — `go/parser` 2026-08-14 (`ca8a3a911`), `mlkem768` 2026-08-16 (`c38de2579`), `crypto/tls`
2026-08-19 (`154d5b5ce`) — **all predating the plan's 2026-08-23 draft**, so the reserved set was
written against a stale copy of the floor table. That also explains the two misquotes.

### 4.6 Fleet inventory (`LANES.md:339-346`)

| Machine | Role | `s_w` | Status | Linux? |
|:--|:--|:--|:--|:--|
| i9-13900K (ritchie-desk2) | sweeper; holds reserved set | 1.00 | anchor, by definition | no |
| Ryzen 7 PRO 6850U (R-LAPTOP) | Lane R | 0.45 | **PROVISIONAL** | **WSL2 Ubuntu 22.04** — ran the Linux leg. ⚠ probed at 34 GB free, **below the 60 GB preflight** |
| i7-5820K (desktop) | coordinator | 0.35 | **PROVISIONAL** | no |
| Ryzen 5 PRO 6650U (G-LAPTOP) | Lane G | 0.35 | **PROVISIONAL** | no |
| *"any fifth worker"* | — | 0.35 | **ASSUMED — no such machine in the roster** | — |

**No native-Linux worker exists.** Linux capacity is WSL2 on Lane R plus CI overflow
(`.github/workflows/os-matrix.yml`, `LANES.md:354` — *"Never a merge gate"*). The `W = 5` row therefore
assumes a machine the canonical roster does not list; **the fleet is 4**.

Why the placeholders exist at all (`LANES.md:348-352`): *"Historical cross-machine speed comparisons
are SUSPECT … The hop shard map's speed factors come from FRESH same-workload calibration at campaign
recon, never from pre-anchor history."*

### 4.7 Shard-map verdict

**Measured and trustworthy:** both 162-row wall tables; the roster count; the fleet hardware.
**Computed but unbanked:** the per-worker assignments and their generator.
**Assumed or missing:** every non-i9 `s_w`; `k`; the fifth worker; two reserved rows; the
Linux/Windows `t_r` ruling; a usable calibration protocol.

**Dispatch may proceed at W ≥ 4 on placeholders per the plan's own gate; W = 3 may not.**
Four fixes I would land before dispatch, in order:
1. **Bank `shard-map-draft.md` + `shardmap.py`** into `docs/phase4/` (§4.1) — free, and it is currently one scratchpad purge from being recomputed.
2. Add `go/parser` and `crypto/internal/mlkem768` to the reserved set; reconcile all four floor values against `run-validated-sweep.ps1:495`.
3. Rule explicitly whether `t_r` is the Windows or the Linux column (**3.3x** on `crypto/dsa`, the row that binds the makespan).
4. Replace the `container/heap` calibration workload and write down the recording protocol.

---

## 5. Dry-run walk of the H-series

Legend — **‖** = parallelizable across the fleet · **⊘** = single-machine / serialized · **⟲** = re-measured, never carried.

| Step | Instrument | Gate | This repo's traps that apply | ‖? |
|:--|:--|:--|:--|:--|
| **H0** Baseline capture ⟲ | seeded `go2cs -stdlib -comments -go2cspath <staging>/src` on the **outgoing** toolchain + **new** converter | none (it *is* the baseline) | ⚠ **`.cs.auto` must be generated fresh**, never taken from the committed siblings — the overlay excludes `*.cs.auto`, so the tracked ones are frozen on their own schedule and a stale baseline poisons H6's differential. Seeding traps as H5 | ⊘ |
| **H1** Toolchain **GATE** | manual side-by-side install; `go build`; `go test ./...` | converter tests green **and** binary demonstrably built by the new toolchain | **See §6-A — this trap is now CLOSED in code but open in the docs.** Also: x/tools + x/mod bumps are **their own commit with their own CNR** (ruled) | ⊘ per machine, ‖ across fleet |
| **H2** Pin bump **GATE** | **`src/migrate-gorelease.ps1 -To 1.23.12 -Apply`** | single-package `-stdlib` smoke no longer refuses | Refuses on a dirty tree; asserts each anchor's count; re-censuses itself. **Lands as ONE pair with H1** — in the window between them the binary claims the new release while `version.props` names the old, and a NuGet-referencing conversion silently misjudges module pins. **Pin precedes reconvert** — `checkCorpusToolchainPin` refuses `-stdlib`/`-tests` otherwise | ⊘ (one commit) |
| **H3** Package census ⟲ | judgement + upstream diff (`gh api …/compare`) | *"a patch-level migration should produce an empty census; a non-empty one is a finding"* | **Already discharged — §2.** Deliverable is still owed: a census doc under `docs/phase4/` recording ∅ | ⊘ (already done) |
| **H4** Converter work **GATE** | per-fix; `check-no-regression.ps1` | **CNR byte-identical, zero `NOT MEASURED`** | Expected ~empty (zero language delta). Two standing items regardless: the **hand-owned `src/core/testing`** host follows nothing automatically, and the **`go.mod` readers** — recon says no new verbs this hop, so the latter is a no-op. CNR budget **700 s** (top of range) | ⊘ |
| **H4a** Opening regen slot | the queued-leveling bundle + seeded reconvert + **`go generate .` in `src/go2cs`** | per-fix CNR; born-stale rows re-swept **at banked counts** | ⚠ **`go generate` is not optional** — `stdlib-metadata.txt` is generated FROM the corpus and gated by `TestStdLibMetadataInSync` under plain `go test`, so a regen banked without it **leaves master's converter gate red for whoever runs it next**. Scheduled **before H5** so H5's overlay diff is readable | ⊘ |
| **H5** Seeded reconvert **GATE** | `go2cs -stdlib -comments -go2cspath <staging>/src` | overlay completes, marker gate **0 violations**, **every diff classified** (T0–T5, zero T5) | The dense trap cluster. **(1) Seed first** — `src/core` + `src/version.props` + `docs/validation`, mirroring `src/`; an unseeded root gives the marker nothing to detect, emits every whole-file hand-own as plain `.cs`, and breaks per-GOOS L3 routing. **(2) Never convert twice into one staging root**; delete and re-seed per run; confirm no `go2cs.exe` alive (the recorded failure is one corrupted file with `«DYNTYPE:…»` markers reading exactly like a converter regression). **(3) Wrap the converter call so stderr warnings don't abort the wrapper** — `$ErrorActionPreference='Stop'` turns a native stderr line fatal, which is *how* the double-run corruption happened. **(4) Marker gate is path-precise, line-anchored, re-measured** — head-window scan under-counts, unanchored match over-counts, default ripgrep honors `src/core/.gitignore` and under-counts; census with `git grep`. **(5) Classify emitted-vs-seeded by sentinel mtime**, not content. **(6)** `src/core/README.md` empty-numstat phantom + hand-owned-by-consequence packages are **not** drift | ⊘ **strictly** |
| **H6** Hand-own re-audit ⟲ **GATE** | **none — no instrument exists (§6-D)** | every marked path in the re-measured census appears exactly once; every (b) has a written reason, every (c) a work item; **zero "no `.auto` emitted"** rows | ⚠ Diff is `.auto`(old) vs `.auto`(new), **both from the SAME converter binary** — never `.auto` vs the hand-owned `.cs`. Named blind spot: if the new binary can't parse the OLD tree cleanly, run A degrades — **assert run A's package count and marker gate before trusting it**. Two populations: `*_impl.cs` companions have no `.auto` and are audited against their principal's; hand-owned *packages* by manual upstream diff | partly ‖ (per-package classification), ⊘ for the two runs |
| **H7** Compile parity **GATE** | `dotnet build src/go2cs-stdlib.slnx -p:UseSharedCompilation=false` per `$(GoTargetOS)` | **errors 0 AND skipped-dependents 0, at 100 %** | A dependent of a failed project is **skipped, not errored** — count them or the gate passes vacuously. Purge `bin`/`obj`/`Generated` between OS switches. ⚠ `-p:GoTargetOS=linux` **does not yet complete** (open corpus debt — the Linux compile wall). Budget 600 s; on the i7-5820K expect 3–4x. `AccessViolationException` in `TypeGenerator` under concurrent load is a known flake — **re-run before believing it** | ⊘ per flavor, ‖ across flavors |
| **H8** Multi-platform re-emission **GATE** ⟲ | `go2cs -stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage <stage>`; then `-platform-census` | manifest marker gate **0 per target**; default flavor reproduces the single-target build **byte-for-byte** | Per-GOOS package count is a **measurement, not a constant** — a migration moves the platform axis in **both** directions. ~3x the single-target cost (~545 s measured at r50a) | ⊘ |
| **H9** Golden rebank **GATE** | `run-behavioral.ps1` (all 4 phases); `UpdateTestTargets --createTargetFiles` | full suite green, four phases | **Predict the diff size first** — §1.1 channel 1 (release tags) is **inert** this hop (`releaseTagsForVersion` is minor-keyed, `directiveOperations.go:264`), so expect small-or-empty; a diff materially exceeding that is a **finding, not a rebank**. ⚠ **Re-transpile BEFORE updating goldens** — `UpdateTestTargets` copies on-disk `.cs` and does **not** run the converter, so a copy over stale output silently re-baselines it. Runner's **internal** budgets are independent of the caller's; a expired budget reports **`NOT MEASURED`**, which fails the run and must **never** read as a corpus regression. ⚠ Machine-global hazards: `Get-Process <name> \| Stop-Process` and `dotnet build-server shutdown` reach sibling worktrees — **the .NET 10 hop is live on `D:` right now** | ⊘ |
| **H10** Roster re-derivation ⟲ **GATE** | `run-validated-sweep.ps1` (**serial by design**), one row at a time via **exact-match** `-Filter`; per-shard ledger | absolute row count ≥ prior, losses only as recorded exceptions; **both** absolute and % reported; banked test-project count **==** roster row count | **The campaign.** Unit of isolation is a **clone/worktree, not a directory**. Disclosures pinned by **exact failure signature** — a reworded test invalidates the pin; **re-derive and re-sign, never edit** (editing converts a real divergence into a rubber stamp). A **closure** is a good outcome that still needs evidence — the arithmetic must move visibly. ⚠ Long runs **detached** (`Start-Process`) or the turn boundary reaps them; poll **positively**. Outer wrapper must **clear** the instrument's internal budget. Floors are **floors, not overrides** | **‖ — this is the shardable step** (§4) |
| **H11** Publication **GATE** | `push-nuget.ps1` / `release-nuget.ps1` / `sign-nupkgs.ps1` | version monotonicity **verified by scripted comparison**, never believed | Guard follows H1 for free (reads the binary's runtime version) — that coupling **is** the H1↔H2 window. New packages ⇒ new IDs; removed ⇒ **deprecate with a pointer, never unlist** (∅ this hop). Signing is **mandatory** (cert registered with nuget.org, unsigned pushes rejected); one machine, one PIN. A non-monotonic public sequence is **not correctable** | ⊘ |
| **H12** Docs/badges **GATE** | `migrate-gorelease.ps1` (prose half, already applied at H2) + regen (badges) | release-ritual **dry run** | **State the expected badge diff before the overlay.** Of four badges per README: **Docs** and **Source·Go** read the toolchain (follow H1); **Tests** and **Source·C#** read `version.props` (follow H2) — so this hop moves **all four**, corpus-wide (651 badge lines / 305 files). **Hand-owned READMEs do NOT follow** — re-run their derivation as a control. GOROOT-vendored `golang.org/x/*` re-pin from the new GOROOT's `src/vendor/modules.txt`; recon flags x/net as actually moving this hop | ⊘ |

### 5.1 Encoding and line-ending traps that span the whole ladder

- **`.gitattributes` pins CRLF** for `*.cs`, `*.cs.auto`, `*.cs.target`, `*.csproj`, `*.slnx`, `*.props`, `*.targets`, `src/core/**/README.md` — **above** the `-text` blocks (last match wins). Do not "fix" a file to LF; the pin puts it back.
- **PS 5.1 `Get-Content`/`Out-File` is the mojibake trap** — reads BOM-less UTF-8 as ANSI, double-encodes `©`. Once damaged 258 corpus files. Every rewrite uses `[System.IO.File]::ReadAllText/WriteAllText` + `UTF8Encoding($false)`. `migrate-gorelease.ps1` already complies (`.NOTES`, lines 110-114).
- **T0's empty-numstat rule is FALSE for `-text` paths** (`src/core/compress/testdata/*`) — a pure line-ending flip shows a **real** numstat there. Test CR-stripped equality against `HEAD` directly.
- **`go2cs-src.projitems` is UTF-8 *with* BOM** with uniform endings, guarded by `projitemsIntegrity_test.go` — edit in place or via `ReadAllText/WriteAllText`, never PS 5.1 `Get-Content`.
- **Windows case trap** on `git add` — readdir casing is recorded; `check-solution-integrity.ps1` asserts `src/tests/Behavioral/…` case-sensitively.

### 5.2 Parallelism summary

Genuinely fleet-parallel: **H10** (the whole point of §3/§4), H7 across `$(GoTargetOS)` flavors, H6's
per-package *classification* (not its two runs), and H1 provisioning per machine.
Strictly serial: **H2** (one commit), **H4a**, **H5**, **H8**, **H9**, **H11**.
`run-validated-sweep.ps1` is **serial by design** and exposes no shard/jobs/resume parameter — all
concurrency lives **outside** the instrument, as worktrees each running an internally serial sweep.

---

## 6. Findings wanting a coordinator ruling

### A. ⚠ **False-green route #4 is CLOSED in code but documented as OPEN in three places** — highest value

`src/tests/ConverterBuildInputs.cs` (banked **2026-08-24 01:35**) implements
`IsConverterStale(converterSrcDir, converterExePath)`, which compares the binary's **embedded**
`runtime.Version()` (read back via `go version <exe>`) against the **live** `go env GOVERSION`:

```csharp
// src/tests/ConverterBuildInputs.cs:102-106
string? embedded = EmbeddedGoRelease(converterExePath);
string? live = LiveGoRelease();

if (embedded is null || live is null || !string.Equals(embedded, live, StringComparison.Ordinal))
    return true;
```

It **fails stale-wards on purpose** (an unreadable stamp or unanswerable GOVERSION forces a rebuild,
which then fails loudly at `go build` rather than silently passing an unverified binary), it also
closes **route #5** (the `//go:embed` assets, the converter's `internal/` packages, and `go.mod`/`go.sum`),
it derives the embedded set **from the `//go:embed` directives themselves** so tomorrow's directive is
covered without anyone remembering, and the directive **forms** are pinned from the Go side by
`src/go2cs/embeddedAssets_test.go` under the plain `go test ./...`.

It is linked by path into **all three** mtime-predicate harnesses **and actually called by each** —
verified both ways, not inferred from the file's existence:

```
# linked                                        # called
BehavioralRunner.csproj:31   <Compile Include=…  BehavioralRunner/Program.cs:340
BehavioralTests.csproj:31    <Compile Include=…  BehavioralTests/BehavioralTestBase.cs:147
PerformanceRunner.csproj:30  <Compile Include=…  PerformanceRunner/Program.cs:282
```

The fourth gate, CNR (`src/tests/Behavioral/check-no-regression.ps1:69-73`), rebuilds
**unconditionally** (`go build -o $go2csExe`, no predicate at all) and was never exposed to either
route — the documented asymmetry, intact.

**Three surfaces still say the hole is open:**

| Surface | Stale text |
|:--|:--|
| `docs/GoCorpusMigration.md` §1.2 | *"**Until it lands**, a toolchain change owes an explicit `go build`, and no gate may run before it"*; H1.5 *"Close the stale-binary hole"* |
| `CLAUDE.md` | *"FALSE-GREEN route #4 … **(open; remedy planned)**"* |
| `src/migrate-gorelease.ps1` operator block | *"a toolchain hop invalidates go2cs.exe in **NO** harness predicate, so the build is owed explicitly"* — **printed on every census run**, including mine |

The third is the one that matters: it is the hop's own instrument telling the operator something now
false, at exactly the moment the operator is doing H1. **Recommend levelling all three.** The explicit
`go build` remains harmless and I would keep recommending it — but *"no harness predicate covers this"*
is no longer true, and a stale warning trains operators to discount the instrument's other warnings.

### B. Shard map — see §4.7. Two reserved rows (`go/parser` 90m, `crypto/internal/mlkem768` 30m) absent from the plan entirely, two floors misquoted, Linux/Windows `t_r` unruled, and the computed assignments unbanked.

### C. `PLAN-hop-campaign.md:501-503` says the Linux ledger *"joins the same file when it posts."* **It posted** (`861475db0`, 2026-08-23). One-line correction; the consequence (§4.4) is not one line.

### D. **H6 has no instrument.** Every other H-step names a script; the `.cs.auto` differential — which the runbook itself calls *"the step that distinguishes a corpus upgrade from a corpus regeneration, and the one a migration is most likely to skip because everything compiles without it"* — is entirely manual, over a **73-file** census (see §6-G), with a completeness gate stated in mechanical terms (*"assert every marked path appears exactly once… exit non-zero on any violation"*) that reads like a spec for a script nobody has written. **Recommend writing it before the hop**, or explicitly accepting a hand-run audit of 73 paths as a gate. It is the cheapest kind of preflight — by-path, and impossible to pass vacuously.

### E. ⚠ **`time` can turn this hop into runtime work — and it has no manifest**

The one row where a *test-only* upstream change is the dangerous kind (§3.4). `sleep_test.go` +115
probes Stop/Reset result semantics; the implementation under test is the **hand-owned `tick.cs`**,
whose Go source is *not* upstream-changed, so **it will not regen to pick up upstream's fix** — that
fix lives in `runtime/time.go`, which go2cs's from-scratch managed timers do not track line-for-line.
And `time` has **no `go2cs_test_disclosures.json`**, so it compares strictly: 159 matched, zero
tolerance.

**Recommend H6 check the managed timer for the bug SHAPE proactively** (Reset/Stop returning the
wrong result when racing a running timer, `RECON:109-113`) rather than discovering it at H10, where it
would surface as a hard mismatch in a row nobody was working on. This is exactly the silent-operational
class H6 exists to catch, and it is the best-signposted instance in the hop.

### F. Two recon corrections worth banking

1. **`exec_posix_test.go` is misattributed.** `RECON:151` puts it under `os/exec`; `RECON:44` puts `exec_posix.go` under `os`. Upstream it is `src/os/exec_posix_test.go`; there is **no banked `exec_posix_test.cs`** anywhere under `src/core/os/`, and `os` is **not** a roster row. The recon's conclusion is unaffected; the attribution should be fixed so a later reader does not hunt for a counterpart that cannot exist.
2. **`src/core/time/time_impl.cs` is not in the marker census** — it carries `[module: go.GoRequiresUnsafe]` (`:13`), not `GoManualConversion`. Any reconciliation of `time` against a census total should expect **1** marked file, not 2.

### G. ⚠ **Hand-own census re-measures to 73, not the recon's 70** — one day later

Line-anchored `git grep` over `src/core` on this checkout **today**:

```
git grep -l -E "^\s*\[module:\s*(go\.)?GoManualConversion\]" -- 'src/core/*'   →  73 files
git ls-files 'src/core/**/*_impl.cs'                                          →  59 files
```

`RECON-go12312-diff.md:8-9` measured **70** on 2026-08-23 and said so explicitly: *"up again from the
plan's '66–67 across the 2026-08-22 lanes'; it moves weekly, re-measured here, not carried."*
**66-67 → 70 → 73 in three days.** This is the doctrine working exactly as written, and it is also the
reason §6-D matters: **the population H6 must audit is growing faster than the hop is being planned.**
(Caveat: the recon's 70 may have been taken on a different clone/branch tip than this `C:` master —
the direction is what counts, not the exact delta.)

### H. ⚠ A pattern, not two incidents: **hop inputs are living in session scratchpads**

Two of this rehearsal's findings are the same defect wearing different clothes:

| Artifact | Doc says | Reality |
|:--|:--|:--|
| Recon raw TSVs (`commits.tsv`, `files-by-commit.tsv`, …) | *"beside this file"* (`RECON:193`) | session scratchpad |
| Shard-map assignments + generator (`shard-map-draft.md`, `shardmap.py`) | *"emitted to `SHARDMAP-go1.23.12.md` at dispatch"* | session scratchpad, only the summary pasted |

Both are **working inputs to the hop**, not archived by-products: §4's triage needs the per-commit
file map by name, and H10's dispatch needs the deal. Both survived only because this rehearsal ran in
the **same session directory** — which is luck, and which the *"purge bin/obj/temp roots when tasks
complete"* tidiness rule actively works against.

The repository already ruled this exact question once, for per-row wall times (§3.2: *"per-row log
retention … is unrecoverable afterward. Make it an obligation of that sweep, not of this step"*) — and
that ruling produced `DATA-sweep-row-walltimes.md`, which is banked and is the reason §4 could be
assessed at all. **Recommend extending the same obligation to recon and shard-map outputs.** Cheap,
and the counterfactual is re-deriving both under hop-day time pressure.

---

## 7. What could **not** be rehearsed, and why

Naming these is the point of a rehearsal, not an apology for it.

1. **Everything downstream of H1.** No Go 1.23.12 toolchain exists on this machine (`~/sdk` holds only `go1.18`). Every step from H4a on is *reasoned*, not exercised. **Nothing here proves the new toolchain builds the converter, and that is H1's whole gate.**
2. **The `-Apply` → CNR → regen sequence.** `-Apply` has been proven in a throwaway worktree (§1.5) and re-proven idempotent, but never in a working tree followed by a real gate. The specific unknown: whether the 20 doc edits + build-number reset leave CNR byte-identical (they should — none is converter input — but "should" is what a rehearsal exists to retire).
3. **Every H10 verdict count.** The four HIGH rows' counts (§3) **cannot be predicted at all** — they are re-derived from the *new release's own test sources* and there is no carry-forward path. The recon names *which* rows will move; only the sweep can say *to what*.
4. **Whether any disclosure's signature survives.** A pin breaks on a **reworded test**, and rewording is invisible in a file-level diff — it needs the new sources in hand. The recon flags `os/exec`'s 27 host-limit disclosures as the largest exposure; the count of survivors is unknowable until H10 runs.
5. **`k`, the load factor,** and every non-i9 `s_w`. Both are hop-time measurements by design (§4.2). No amount of reading substitutes.
6. **Whether `-p:GoTargetOS=linux` completes.** It does not today (standing corpus debt). Whether the hop moves it in either direction is unknown and should not be assumed neutral.
7. **The T5 (unattributed) class.** By construction, T5 is *"none of the above"* — the migration's real signal. **A rehearsal cannot enumerate it.** The most a dry run can do is confirm T0–T4 are well-understood, which they are.
8. **Interaction with the concurrent .NET 10 hop.** `migrate-tfm.ps1` is that hop's counterpart instrument and is live on `D:\Projects\go2cs`. The runbooks insist on **one variable at a time**; the two hops are separate by design, but the *machine-global* hazards (§5.1, `Stop-Process` by name, `dotnet build-server shutdown`, `%GOPATH%\src\go2cs` as `deploy-core`'s default target) **do not respect worktree isolation**. Untested, and worth a coordinator word before both hops run heavy gates at once.

---

---

## 8. Findings disposition (appended 2026-08-24 — every §6 finding, with what happened to it)

The rehearsal's whole point was to surface these before the hop fired. Each is closed here against a
commit or a document, or named as still open with its owner. **Nothing is marked closed by argument.**

| # | Finding | Disposition |
|:--|:--|:--|
| **A** | Route #4 is CLOSED in code but documented as OPEN in three places | **CLOSED — `27fe4632e`.** All three surfaces levelled: `GoCorpusMigration.md` §1.2, `CLAUDE.md`'s route catalogue, and — the one that mattered — `migrate-gorelease.ps1`'s operator block, which was printing a falsehood at exactly H1 time. **One residual, since fixed:** the runbook's H1 step 5 still read *"close the stale-binary hole"*, a pending action for a closed hole; it now says **verify the guard held** |
| **B** | Shard map: two reserved rows absent, two floors misquoted, assignments unbanked | **CLOSED, by removing the class rather than the instances.** The draft and generator banked (`e0d8930e1`), then `shardmap.py` was changed to **derive** the reserved set from `$longTimeouts` at generation time (`549b4e556`) — so the missing rows and misquoted floors cannot recur. `PLAN-hop-campaign.md` §4.3's static table is marked SUPERSEDED BY GENERATOR, and the derive-never-copy rule is generalized into `GoCorpusMigration.md` §3.2 |
| **C** | The Linux-vs-Windows `t_r` ruling is unmade | **RESOLVED BY STANDING RULE — no new ruling owed.** `LANES.md`'s roster already rules that *"the hop shard map's speed factors come from FRESH same-workload calibration at campaign recon, never from pre-anchor history"*, and the shard-map draft's own banner repeats it. The leg a row is costed from is not a separate question: the recon that measures `k` and every `s_w` measures the row costs **with** them, on the leg the shard will actually run. `PLAN-hop-campaign.md` §4.3 and `GoCorpusMigration.md` §3.2 now both say so, with the measured non-uniformity (~2.5× overall, 3.3× on `crypto/dsa`) attached so the size of the effect is not lost |
| **D** | H6 has no instrument | **CLOSED — `d215b9bf8`, `src/handown-census.ps1`.** The differential census half; it turned *"review all hand-owns"* into a six-row review list on its first run. The judgement stays human, and the runbook says so |
| **E** | `time` can turn the hop into runtime work, and it has no manifest | **SUPERSEDED by measurement — [`hopA-time-prestage.md`](hopA-time-prestage.md) (`4f6906f40`).** The fear was falsified and something else was found: the banked 159 are safe, the fixed Stop/Reset semantics **already hold** on the shipping modes, and the real blocker is an **AV on `asynctimerchan=2`**. It is now a named pre-H10 obligation in `PLAN-hop-campaign.md` §4.1, and the rule it produced — *a disclosure cannot absorb a crash* — is in `GoCorpusMigration.md` §4 |
| **F** | Two recon corrections worth banking | **APPLIED — `RECON-go12312-diff.md`'s dated ERRATA block** carries both verbatim |
| **G** | Hand-own census re-measures to 73, not 70, one day later | **CARRIED, as evidence rather than as a number.** The runbook keeps the *shape* (a census of dozens reducing to a single-digit review list) and the census's own re-measure-never-carry rule; the 70 → 73 movement is recorded in `RECON-go12312-diff.md`'s amendment as what that rule looks like in practice |
| **H** | A pattern: hop inputs are living in session scratchpads | **CLOSED, and generalized.** The recon's five raws banked by independent re-derivation (`5d4410b71`); the shard map and generator banked (`e0d8930e1`); and **this rehearsal's own three raws are banked verbatim** in [`hopA-inputs/`](hopA-inputs/) — `census-raw.txt` (152 lines, the census run behind §1), `h3-files.tsv` (160 rows, §2's independent derivation), `h3-compare-head.json` (the compare endpoint's headline, preserved as the evidence for the truncation the recon predicted). The rule the pattern earned is now `GoCorpusMigration.md` §3.4: *bank a migration's inputs in the commit that claims them; the report is not the artifact* |

**One item from §7 has also been overtaken**, and is corrected here rather than in place: item 6
asked whether `-p:GoTargetOS=linux` completes and said *"it does not today"*. The Linux compile wall
**fell on 2026-08-14** — 307/307, zero errors
([`CENSUS-linux-compile-wall.md`](CENSUS-linux-compile-wall.md) §10). What remains beyond Windows is
operational, not a compile wall. Everything else in §7 stands: it names what a dry run structurally
cannot reach, and that list does not expire.

*— Rehearsal lane, 2026-08-24. Raw census output: `census-raw.txt`. Upstream file data: `h3-files.tsv`, `h3-compare-head.json` — **all three banked 2026-08-24 in [`hopA-inputs/`](hopA-inputs/)**, verbatim. Nothing in the repository was modified by the rehearsal itself.*
