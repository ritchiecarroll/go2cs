# PLAN — the Go 1.26 hop (hop C′): go1.24.13 → go1.26.9, one fold, with an attribution checkpoint at 1.25.14

> **STATUS: RULED GO (COORD, 2026-10-14), H0 OPEN at master `b594cd90af`.** Proposed by the i9, the hop's
> lead, 2026-10-11; O1 and O2 ruled by the owner 2026-10-12; the stage order and lanes ruled 2026-10-14
> (Amendments). The remaining owner hands are in §2. **H0's trigger, the 1.24.13.7 release record, landed
> 2026-10-14** (`b0ef977fda`, index row `64ffbdac2a`). It is a campaign plan, so it supplies *this* hop's target, sequencing, census, lane
> proposal and estimates. It supplies **no procedure**. The procedure is
> [`GoCorpusMigration.md`](GoCorpusMigration.md) (the runbook), referenced by stage and never restated.
> Where this plan and the runbook disagree, the runbook wins, and the disagreement becomes an in-stage
> runbook edit (§9).
>
> **The ruling it executes** (owner, 2026-10-11, relayed by COORD):
> - the hop target is **Go 1.26**, straight from 1.24.13, with **no 1.25 release**;
> - OQ-18 is re-ruled for this hop: the fold is admitted, with the **attribution checkpoint** (§4) as its
>   safeguard;
> - 1.27 follows as its own hop, with its generic-methods DESIGN drafted during this hop's H10.
>
> **Its evidence** is [`phase4/RECON-go125-hop.md`](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-go125-recon/docs/phase4/RECON-go125-hop.md)
> on `claude/i9-go125-recon` (`b19228d610`): the 1.25 measurements and the EXTENSION's 1.26 measurements.
> It also builds on COORD's pre-hop notes, [`phase4/NOTES-next-hop.md`](phase4/NOTES-next-hop.md), whose
> 1.25-target items carry over wherever 1.26 does not supersede them.

---

## 0. The hop in one screen

- **Target: go1.26.9**, the newest 1.26 at the ruling. OQ-1 freezes the target at hop start. If a 1.26.10
  ships mid-hop, adopting it is an explicit restart decision and never a drift. **Upstream support at
  landing:** 1.26 is supported until 1.28 ships (about 2027-02).
- **The fold.** One hop crosses two releases. Every red is attributed to 1.25 or 1.26 **before** it is
  triaged, by the checkpoint in §4. This is the one structural novelty against hop B. Everything else is
  the runbook as written.
- **The bill** (measured at go1.26.4 against go1.24.13, RECON EXTENSION X.2):
  - **Language:** `new(expr)` and generic self-reference. The standard library uses `new(expr)` at 1 site
    (a test in an experiment package).
  - **Converter:** builds and vets clean under 1.26.4.
  - **Runtime:** +23.4k / −5.1k lines; Green Tea GC and the randomized heap base are on by default.
  - **H6:** **79 substantive hand-owns** (runtime 27, syscall 9, sync 5).
  - **Roster:** 38 rows untouched upstream, 75 rows with a moved test-function set (+420 / −45), and 4
    rows relocated.
  - **New packages:** every row reaches 10 new packages through `runtime`.
- **The finish line** is the go1.26.9 population of record, derived at H3. At go1.25.14 it was 237
  testable packages; 1.26 adds and removes more. The full roster is about **N_impl/N_impl at about
  235**, not 225/225 (§3.4).
- **The shape is C, the hop B shape.** Run every row, then publish at the H10 close. A short list of rows,
  named in advance from §3.3's reserved pool and capped (§2 O7), may be demoted, each with a costed
  recon-basis row.
- **Effort** is an estimate: **100–165 lane-days, 4–5.5 calendar weeks at 7 lanes** (§6; amended
  2026-10-12, see the foot). The H6 review dominates it: 79 files, 9,600 upstream lines changed,
  measured at 1.26.9. The H10 re-derivation is next.
- **The critical path:** H1/H2 → H4 (the testing host, then the language cuts on the H1-built converter)
  → H4a → **the §4 checkpoint** → H5 → H6 ∥ H7 → H7a → H8 → H9 → H10 → H11/H12.

---

## 1. Preconditions: Readiness and the fourth ordering

**The outgoing final release must ship before H1.** The runbook's fourth ordering requires it, because
`migrate-gorelease.ps1` resets `<GoBuildNumber>` at the pin stage. 1.24.13.6 was published on
2026-10-12.

**RULED (owner, 2026-10-12; O1): 1.24.13.7 is the FINAL 1.24 release.** It is the next train: comment
parity, the macOS x509 fix, G's trim fixes and the go-cmp speedup. H1 starts from it.

**H0's trigger: the 1.24.13.7 release record lands.** That covers the publish, the write-once proof
snapshot and the post-publish smoke. H0's comparands are then captured at that tree, which already
carries the comment-parity re-baseline below.

**The comment and spacing parity work lands BEFORE the hop** (owner, 2026-10-11 evening; decisions
ruled 2026-10-12). Its seats re-baseline the behavioral goldens, whitespace only. So **H0's comparands
(the `.cs.auto` baseline, the behavioral failing set, the manifests) are captured AFTER the last
comment-parity train lands**, never before. Otherwise H5's differential mixes the hop with the
re-baseline.

**Instruments**, Readiness "Instruments". Each lands on master and is red-proved before the stage that
needs it (lessons 2, 43):

| # | Instrument | State at master `b594cd90af` (re-read 2026-10-14; was `20279db5f6` / `7092f7126e`) | Needed by |
|:--|:--|:--|:--|
| A3 | `-Hop` applies roster execution pins | **LANDED** in TRAIN T4 (P2, `8637b815ee`) | H10 |
| A7 | `src/run-h10-recon.ps1` reads the pin from `version.props` | **LANDED** in TRAIN T4 (P2, `db025b1f8b`; a release matches only whole) | H10 recon leg |
| A8 | `src/check-h6-completeness.ps1` derives the audit file from the release | **LANDED** in TRAIN T4 (P2, `1666c83e9d`; default `AUDIT-h6-handown-go<GoStdLibVersion>.md`) | H6 |
| A9 | `docs/phase4/hopA-inputs/shardmap.py` takes the population by release | **LANDED** in TRAIN T4 (P2, `841fc39322`) | H10 |
| A12 | x/tools and x/mod at H1 step 3 | the converter pins x/tools v0.42.0 and x/mod v0.33.0 (both `go 1.24.0`); the newest cached here are v0.48.0 and v0.38.0, both `go 1.25.0`. **Choosing a 1.26-era version needs a module fetch (§2 O6)** | H1 |
| A13 | `src/check-roster-format.ps1` derives the population path | **DONE** (`:906`) | — |
| A14 | the allocation-unit extractor (lesson 31's reading run) | **LANDED** in TRAIN T4 (P2, `0af043ecac`, tooling only, footprint 0; runbook H10 step 3(a) names it) | H10 |
| — | the repoguard retired-marker guard, "owed before the next hop's H6" (runbook H6) | **LANDED** in TRAIN T4 (P2, `11579e35ea`) | H6 |

**Hosts**, Readiness "Host":
- each lane's box record: DNS, WSL `::1`, symlinks, `LongPathsEnabled`, pwsh 7, and the pinned .NET SDK;
- the linux arms run Microsoft's .NET with the libcoreclr hash recorded;
- GOROOT on NTFS;
- the BoGo host is qualified by measurement (§2 O3);
- seat census: every accepted seat since the last anchor is an ancestor of master, or carries a
  disposition.

---

## 2. What the owner's hands are needed for

**For H0 and H1:**
- **O1 — Name the final 1.24.13.N** (§1). H1 waits for it to ship. **RULED 2026-10-12: 1.24.13.7.**
- **O2 — Toolchains, side by side under `~/sdk`, verified by `go version` output, never auto-fetched as an
  install.**
  - **RULED 2026-10-12 for go1.26.9:** "OK to install 1.26.9 on you and all lanes" (owner to COORD).
  - **State:** the i7 is DONE (SHA-256 matched against go.dev's release JSON; go1.24.13 untouched). Each
    other lane installs on its own box, confirms by owner, and records its `go version` output.
  - **go1.25.14 for the checkpoint is NOT in the ruling's words.** It is still an ask (below).

  The boxes:
  - **go1.26.9 on the i7** and on every box that converts or builds: the i9; G-LAPTOP and **its linux
    arm**; R-LAPTOP and its WSL arm; each cloud lane's container, where the container recipe fetches it.
    G-LAPTOP's linux arm had no 1.24 root until it was provisioned earlier this year (G), and it carries
    H7's linux flavour, so **ask for it now**. The i9 already holds a checksum-verified go1.26.9 in
    scratch (RECON X.7). It is a reading copy and not the O2 install;
  - **go1.25.14 on the box that runs the §4 checkpoint** (the i9 is proposed). This box has only go1.25.12
    cached, and the ruling names .14.
- **O3 — The BoGo host.**
  - A many-core Windows host for `crypto/tls`'s standard BoGo wall, plus any cloud quota it needs. File
    the quota now; its lead time is outside the fleet's control.
  - The i9 is banned from TLS rows by owner order. The i7 or a temporary kit host carries `crypto/tls`.
  - Any raised wall is an owner ruling.
- **O4 — DNS or router changes** a host record finds (lesson 42: net's row box is re-qualified twice).

**Later in the hop:**
- **O5 — NuGet prefix reservations at H3** for the new public IDs. On the 1.26.4 census that is
  `go.testing.synctest`, `go.testing.cryptotest` and `go.crypto.hpke`, plus any internal IDs H3 rules
  packable.
- **O6 — The x/tools and x/mod fetch at H1 step 3.** Take the highest versions whose `go` directive is at
  most 1.26.x. They are not in the module cache, and a fetch is a download.
- **O7 — The shape C demotion cap,** and the P2/OQ-10 exception scoped to it. A number (≤5 is proposed),
  and the pool is §3.3's reserved list.
- **O8 — Q15:** does H11 refuse when origin is unreadable, or degrade to local tags?
- **O9 — Deprecation of the removed IDs after the publish** (runbook H11):
  - NOTES G1's list: `go.internal.trace.event`, `…event.go122`, `…internal.oldtrace`,
    `…testgen.go122`, `go.log.slog.internal.slogtest`, `go.runtime.internal.startlinetest`, and darwin's
    `go.vendor.golang.org.x.net.route`;
  - plus, at 1.26, `go.crypto.internal.hpke` and `go.internal.itoa`;
  - first confirming which IDs earlier 1.24.13.N releases actually published.
- **O10 — Release day:** the PIN and the publish at the release machine.

---

## 3. The census (from the RECON; re-measured at go1.26.9 by each ⟲ stage)

### 3.1 Packages (H3)

`go list std` at the RECON axes (windows/amd64, `CGO_ENABLED=0`, no tags, default GOEXPERIMENT):

| OS | go1.24.13 | go1.26.4 |
|:--|--:|--:|
| windows | 346 | 362 (+26 / −10) |
| linux | 344 | 360 |
| darwin | 345 | 360 |

- **New public packages:** `testing/synctest`, `testing/cryptotest`, `crypto/hpke` and
  `crypto/mlkem/mlkemtest`.
- **Relocations by name:**
  - `crypto/internal/hpke` → `crypto/hpke`;
  - `internal/itoa` → `internal/strconv`;
  - `internal/trace/internal/oldtrace` → `…/tracev1`;
  - `go/ast/internal/tests` → `go/ast`;
  - `runtime/internal/{startlinetest,wasitest}` → `internal/runtime/…`.

  H3's relocation file derives them by test body, as C2 did at 1.25.14.
- **GOEXPERIMENT baseline:** `Dwarf5`, `RandomizedHeapBase64` and `GreenTeaGC` are on.
  `CoverageRedesign`, `SpinbitMutex`, `SwissMap`, `AliasTypeParams` and `SyncHashTrieMap` are no longer
  baseline flags. Expect the H5c CS0102 class on the `internal/goexperiment` pairs.

### 3.2 Converter (H4)

- **Language:**
  - **`new(expr)`**. The standard library has 1 use, outside the default set. It is still owed for
    third-party modules declaring `go 1.26` (the nugetgo axis).
  - **Generic self-reference** in a type-parameter list.
- **Front end:** `go/ast`'s `BasicLit.ValueEnd` corrects `End` for multi-line raw strings in
  CRLF-terminated files. **Check the position maps.** `go/types` and `go/token` changes are additive.
- **The standing H4 items** (runbook H4):
  - **the hand-owned `testing` host** (§5's first item);
  - the `go.mod` readers (1.25 added an `ignore` directive);
  - throwing linkname bodies per non-Windows GOOS, from NOTES §3.9's linkname delta re-taken at 1.26,
    linux first (lessons 30, 64).
- **The declared-set guards** (`TestLinknamePushRegistryMatchesGoSource`,
  `TestDeclaredNotImplementedCensus`'s `mapKeyError` control) red **by design** (lessons 19, 60). They
  are fixed in the commit that changes the converter.

### 3.3 Rows (H10's pre-staging input)

The census over each toolchain's own file selection at the corpus tags:

| Class | Rows | Banked verdicts |
|:--|--:|--:|
| R — package gone | 4 | 28 |
| U — untouched | 38 | 372 |
| T — tests only | 14 | 10,425 |
| P — production only | 44 | 1,046 |
| B — both, same test set | 50 | 3,434 |
| **F — test set moved** | **75** | **54,472** |

**The reserved pool**, shape C's only demotion candidates and H10's pre-stage first tier:
- the **26 rows with a substantive hand-own in their own package** (13,917 verdicts at 1.26.4);
- the `testing/synctest` consumers (`net/http`, `encoding/json`);
- the trace and runtime family (`runtime`, `runtime/debug`, `runtime/pprof`, `runtime/trace`,
  `internal/trace`);
- `crypto/tls`, which waits on its host (O3);
- `unicode`, a strict table row.

The pool is re-derived at go1.26.9 before O7 is ruled.

### 3.4 The denominator

- The population of record at go1.25.14 is 237 (`docs/phase4/data/population-go1.25.14.txt`: 10 added,
  3 removed).
- **H3 derives `population-go1.26.9.txt` by two independent routes** (lesson 52): genpopulation, and an
  enumeration such as the RECON's row census.
- The five exclusions are re-ruled one by one, never carried. Any new exclusion is ruled with its reason.
- The header's denominator moves at H10, and both ratios are reported (OQ-10, R7).

---

## 4. The attribution checkpoint — the fold's safeguard

**Purpose.** A folded hop's differential shows the union of two releases' upstream changes (the OQ-18
rationale). The checkpoint splits it **before triage**. It produces no release, no commit of corpus
output, and no golden.

**Where it sits:** after H4a, and before H5's real (1.26.9) reconvert. **One box runs both conversions:**
the i9, serially, with the hardware order's caps. Each root gets one conversion (safety floors 1 and
11).

**Steps.** Each is a runbook stage run twice: once at go1.25.14 into scratch root **S25**, then at
go1.26.9 into scratch root **S26**.

1. **Build the converter once, after H4 closes.** It already carries the 1.26 features, which are inert
   at 1.25. Record its sha256. Both roots use that same binary (runbook H5's seeded-root amendment).
2. **H5 seeded three-target reconvert into S25** with a **scratch-only** 1.25.14 pin. The repo's
   `version.props` is never touched; the seed carries the pin. Run H5c dry and capture its class
   counts, then **apply H5c in S25 only, never in the repository**. The RECON measured that without it
   the ladder stops at rung 1.
3. **H7's compile ladder on S25, per flavour, serially.** It **stops early, and the record states where**:
   no H6 work is done at 1.25.14, so the ladder stops at the rung where the hand-owned files, still
   written for 1.24.13, block the build. That stop is not a red to triage. Each rung's red set up to the
   stop is **RED25**: attribution evidence up to the stop and no further.
4. **The same at go1.26.9 into S26.** This is H5 proper, with its own gate. It gives **RED26**.
5. **Attribute the reds:**
   - each RED26 site that is also in RED25 is **1.25-born**;
   - each RED26 site that is not is **1.26-born**;
   - each RED25 site absent from RED26 was **closed by 1.26 upstream**. That is R11's other direction,
     classified with evidence;
   - a RED26 site on a rung beyond the S25 stop has no 1.25.14 reading to compare with. It is read
     against **both** releases' source, by name.
6. **Attribute the H6 deltas the same way.** Run `handown-census.ps1` twice, 1.24.13 → 1.25.14 and
   1.25.14 → 1.26.9. Each of the 79 substantive files then carries the release or releases that moved
   it. The H6 audit records that column.
7. **The record:** `docs/phase4/CHECKPOINT-go125-attribution.md`. It lists the per-class counts at both
   pins (lesson 57), the rung where the S25 ladder stopped, every red by site with its attribution,
   and the binary hash.
   **A red is never triaged without its attribution.**

**Cost:** 4–7 lane-days, the i9 serial, plus disk at the H5 floor for two roots.

**Gate:** "every RED26 site and every H6 substantive file carries an attribution, and the record states
the S25 stop". It blocks H6 and H7, not H5. It is not a parity gate. *(Wording aligned 2026-10-14 with
the runbook's release-fold text, §2 AMENDED 2026-10-13, edits E1 and E2.)*

**What it does not do.** It does not run behavioral goldens or roster rows at 1.25. Those would be a
second hop's cost. Attribution at H9 and H10 rests on the compile- and H6-level attribution, plus the
RECON's row classes. A row red at H10 whose cause is not already attributed is read against both
GOROOTs' source by name.

---

## 5. Stage plan (runbook stage by stage; ⟲ = re-measured at the real target)

| Stage | What this hop does beyond the runbook text | Lanes (proposal, §7) | Lane-days (ESTIMATE) |
|:--|:--|:--|:--|
| **Readiness** | A3, A7, A8, A9, A14 and the retired-marker guard **LANDED in TRAIN T4** (§1). Still open at H0: P2's §9 derivation notes; A12/O6 (the module fetch, an owner ask); each lane's host record, posted at its first leg; the BoGo host, qualified by measurement at H10 | P2 (instruments, §9 notes); each lane its host | 8–14 (mostly spent) |
| **H0** ⟲ | Seeded old-release `.cs.auto` baseline; package census; roster snapshot; the outgoing three-target manifest; the behavioral failing set by name; each box's bare `go version`. Commit every unbanked reading (lesson 48) | COORD/i7 (behavioral, manifests), i9 (census, `.auto`) | 3–5 |
| **H1 + H2** (one pair) | go1.26.9 installed (O2); the `go` directive; x/tools and x/mod in their own commit (O6, A12), **gated by CNR byte-identical after the bump**, a full seeded corpus reconvert with hours of wall; the converter `go test` suite (about 40 min a run on G-LAPTOP); `go vet` under the new directive (clean today at 1.26.9, RECON X.7); GODEBUG defaults read against the test host (1.26 adds the heap-randomization, Green Tea and crypto-randomness classes; 1.25's `containermaxprocs`, `updatemaxprocs`, `decoratemappings`, `tlssha1` and `x509sha256skid`) | G (the converter half, 1.5–3), COORD (the pin pair) | 3–5 |
| **H3** ⟲ | `population-go1.26.9.txt` by two routes; relocations by body; new IDs → O5; `//go:embed`, linker-synthesized linknames and new syntax, each with a probe (lesson 54) | C2 (genpopulation), i9 (independent route) | 2–3 |
| **H4** | **The testing host FIRST** (hand-owned C#, so it can be written against the outgoing corpus, lesson 13): `Attr`, `Output`, `ArtifactDir`; `AllocsPerRun`'s parallel panic with Go's exact `parallelStart`/`parallelStop` accounting; the `testingSynctestTest` hook; `synctest.Run` removed. **The language cuts start only AFTER H1**, because `new(expr)` and self-reference typecheck only in a converter built with go1.26's `go/types` (G). They are: **`new(expr)`**, 1.5–2.5: the one emission site `convCallExpr.go:2846` (today `@new<T>()`) boxes the evaluated value, with untyped-constant default typing and the array and struct clone rule. Two sites would break SILENTLY: `convCallExpr.go:1244` (the named-array wrapper over a zero value) and `visitValueSpec.go:579` (the struct-type lift that assumes a type argument). Three are safe: `callerInliningAnalysis.go:453`, `refLoweringAnalysisOperations.go:881`, `typedNilInterfaceBoxing.go:256`. **Generic self-reference**, 1–3: C# accepts the F-bounded constraint; the risk is the converter's own walkers recursing (20 `.Constraint()` reads in 5 files, `constraintOperations.go` first) and go2cs-gen's constraint emission; probe first. **`BasicLit.ValueEnd`**, 0.25–0.5: only CRLF sources move; GOROOT is LF, so CNR should read identical; one probe for CRLF module checkouts. **The `go.mod` `ignore` directive**, 0.25, rides the x/mod bump. Then the linkname bodies. CNR byte-identical | R (testing host), G (language, 3–6.5), P2 (linkname bodies) | 8–14 |
| **H4a** | The outgoing-record-frozen shape: one seeded three-target old-release regen into staging, nothing committed | i9 | 2–3 |
| **§4 checkpoint** | S25 and S26; RED25 against RED26; the two-leg H6 census | i9 | 4–7 |
| **H5** | The 1.26.9 three-target reconvert (S26 is its run); H5c with `-ExpectSourceGo go1.24.13`; overlay; marker gate | i9 (emission), COORD (H5c review) | 5–8 |
| **H6** ⟲ | **79 files, attributed (§4 step 6).** Every one gets a classified delta record. The completeness gate runs at H6 AND before the release. Split, exhaustive at the 1.26.4 census: **C1 + G, 32:** `runtime` 27, `runtime/pprof`, `internal/runtime/{syscall,maps}`, `internal/abi`, `internal/cpu`. **P2, 24:** `syscall` 9, `internal/syscall/windows` 3 + `registry` 2, `internal/poll` 2, `os` 3, `net` 3, darwin `vendor/…/route` 2. **R, 13:** `sync` 5, `internal/sync`, `unique`, `time`, `iter`, `internal/synctest`, `testing`, `internal/testenv`, `internal/godebug`. **G, 10:** `reflect` 2, `math/bits`, `hash/crc32`, `crypto/rand`, four `crypto/internal/fips140/*`, `internal/chacha8rand`. Re-split at the go1.26.9 census. **Sizing (amended 2026-10-12, measured at go1.26.9):** the 79 hand-owns map to 76 upstream files with 9,600 changed lines. **20 files are large** (≥150 changed lines; 10 of them `runtime`): `internal/poll/fd_windows.go` +658/−438, `runtime/malloc.go` +654/−244, `runtime/mcleanup.go` +660/−15, `testing/testing.go` +483/−165, `runtime/mgc.go` +464/−116, `runtime/runtime2.go` +336/−128, and so on. 56 are small. At G's rates (about 1–1.5 lane-days per large file, 0.3 per small one), H6 is **30–45**; `runtime` (25 files, 5,060 lines) is 14–20 of that | C1, G, R, P2 | **30–45** |
| **H7** | 100% compile on the windows, linux and darwin flavours, serially per flavour; RED26 drives it | i9 (windows), G (linux), COORD or C1 (darwin) | 8–14 |
| **H7a** | One master fold, never a rebase, after the last compile-side seat; plan the second fold at the campaign tip (lesson 25) | COORD | 1–2 |
| **H8** ⟲ | Platform manifest; marker gate zero per target; the default-flavour build reproduces the single-target build | P2 if its legs fit a cloud session's 30 GB; else R or G. **Disk need: measured at H0's first step** (the seeded three-target root plus one flavour's build, read from the volume's free-space delta) and named in the H0 reading | 2–4 |
| **H9** | Golden rebank on **windows** (the banking platform); predict first; Output's failing set equal by name to H0's, with APPEARED empty | COORD/i7 | 3–5 |
| **H10** ⟲ | Census and pre-stage (the reserved pool first), then the recon leg in a throwaway worktree, then seat, plan and driver. The linux READING runs alongside; the linux BANK is its own leg. Alloc relabel (lesson 31). Every relocated row's `Receives` provenance. Address-shaped output under heap randomization is read through `addressPairs` | i9 and G (windows shards, i9 non-TLS only), **R on R-LAPTOP's WSL (linux reading and bank; G the second linux host)**, COORD (dispatch and the `crypto/tls` host), P1 (disclosure re-signs) | 18–30 |
| **H11 + H12 + release** | Existence-plus-monotonicity before the first publish; badges and docs (R); the ritual rehearsed; release-smoke on four RIDs; the PIN (O10); deprecations after the publish (O9) | COORD, R, the owner | 3–5 |
| **Total** | | | **100–165** |

**What can overlap:**
- Readiness instruments and host records can run alongside O1's wait.
- H3's derivation can run as soon as go1.26.9 is installed. It does not need H2.
- The testing host (H4's hand-owned half) is written against the outgoing corpus first, per lesson 13.
  The language cuts cannot be: they wait on the H1-built converter.
- H6 review runs alongside the H7 ladder once H5 lands.
- H10's pre-staging of the reserved pool can begin at H7a.

The rest is serial, as the runbook states (H1/H2 a pair; H4 closed before H4a; H5c before the ladder;
H7a before any run rung; H9 before H10; H10 before H11).

---

## 6. Calendar

- **Hop B, for reference** (NOTES §4):
  - tag `nuget-1.23.12.3` on 2026-09-07; H1.2 the same day;
  - H5 checkpoint 09-13; H7a 09-16; H9 closed 09-20; H10 close 09-23; publish `nuget-1.24.13.1`
    09-24 (16.4 days, six lanes);
  - 225/225 about 2026-10-01.
- **This hop:**
  - the H6 bill is +72% on hop B's (79 against 46);
  - the language delta is small (hop B carried generic type aliases);
  - one checkpoint is added;
  - one more lane.
- **ESTIMATE: 4–5.5 calendar weeks from H1** to the publish at the H10 close. The serial spine is about:
  - H1 → H5: 1.5–2 weeks;
  - H6/H7 → H9: 1.5–2 weeks (the H6 sizing: four lanes in parallel);
  - H10: 1–1.5 weeks.
- The demoted rows (if any, O7) bank after it, as in hop B.
- **The 1.27 generic-methods DESIGN** is drafted during H10, off this hop's critical path (the ruling).

---

## 7. Lane assignment proposal (7 lanes + COORD)

Box facts are from `RESUME-SESSIONS.md` 1e and 1h on `claude/coord-handover`. Every assignment is
COORD's to rule.

| Lane | Box | Proposed hop role | Constraints |
|:--|:--|:--|:--|
| **i9** (lead) | windows | This plan; H0 census and `.auto`; H3's second route; H4a; **the §4 checkpoint**; H5 emission; the H7 windows flavour; H10 windows shards (non-TLS) | Owner hardware order: one heavy task at a time, `-m:4` / `-p 4`, WHEA counted; **no `crypto/tls` or TLS-using net rows** |
| **G** | windows + linux arm | H1 converter; H4 language and front end; the H6 runtime share; the H7 linux flavour; H10 windows shards; the second linux host | The ILC generic-cycle root cause runs alongside, at lower priority |
| **R** | windows + WSL | **The H4 testing host**; the H6 sync and synctest share; **the H10 linux reading and bank on R-LAPTOP's WSL**; H12 docs and badges; the overflow battery host | Also writes the launch plan |
| **C1** | cloud linux | The H6 runtime share (its runtime hand-own history); darwin | Cloud ceiling (about 4 vCPU, 16 GB RAM, 30 GB disk, fixed): code, docs, sizing and CI dispatch only |
| **C2** | cloud linux | H3 genpopulation and relocations | Cloud ceiling as C1; **also carries the owner's nugetgo modules (priority 1)**, so its hop load is kept small |
| **P1** | cloud linux (Sonnet, pilot) | H10 disclosure re-signs and alloc relabels: bounded and mechanically checked | — |
| **P2** | cloud linux | H4 linkname bodies; the H6 syscall, poll, os, net and reflect share; H8 if its legs fit 30 GB | Cloud ceiling as C1: package builds only, purged between packages |
| **COORD** | the i7 | Rulings; the H1/H2 pin pair; H5c review; H7a folds; H9 goldens; H10 dispatch and the `crypto/tls` host liaison; H11/H12 with the owner | — |

---

## 8. Risks, ranked

1. **H6 at 79 files**, a third of them `runtime`. Mitigations: attribution (§4.6) and a split by package
   (§5). Hop B's single largest converter-side item was 46 files.
2. **`runtime` compile at 1.26** (+23.4k lines; `internal/runtime/gc/scan`; Green Tea). Every row waits
   on it. Mitigation: the checkpoint names which reds are 1.26-born before triage.
3. **The `crypto/tls` host (O3).** In hop B the BoGo host held the close for about 15 h. Mitigation: ask
   now; pre-stage the row on the qualified host; the row is in the reserved pool.
4. **Heap randomization:** printed pointers vary between runs. Mitigation: the `addressPairs` reading
   (`oneToOne` and `nToN`) is already in the record. Rows whose output names addresses are pre-staged
   first.
5. **Lane contention with nugetgo**, owner priority 1, mostly C2. Mitigation: C2 gets one hop stage (H3).
6. **The target moves.** A 1.26.10 mid-hop is a restart decision (OQ-1), never a drift.

---

## 9. Runbook edits owed in-stage (applied when their trigger lands, each with a one-line dated note)

- **When H1 starts:** the runbook's text has no fold. Add a dated note under §2 that a fold is admitted
  only by an explicit ruling (this hop's: owner, 2026-10-11) and that it carries an attribution
  checkpoint (this plan §4).
- **The version-specific spellings** the stage digest found (window literals, the `pin_pair` block, H4a
  and H5 commands, the `REHEARSAL-*-go124` names, the H6 audit name, the H10 population path) become
  derivation rules as A7, A8 and A9 land (NOTES §6).
- **At the close:** a dated "Lessons from the 1.26 hop" delta under §7 (NOTES Q11), including the
  checkpoint's cost and yield.

<!-- Sources: the owner's ruling (ledger 2026-10-11 12:12, relayed by COORD); RECON-go125-hop.md at claude/i9-go125-recon
     b19228d610 (sections 1-5 for 1.25.12; EXTENSION X.1-X.6 for 1.26.4); NOTES-next-hop.md (A1-A14, B0-B5, C-G, Q1-Q15,
     §4 hop B milestones); GoCorpusMigration.md (Readiness 156, H0 201, H1 221, H2 292, H3 407, H4 433, H4a 456, H5 620,
     H5c 702, H6 1291, H7 1511, H7a 1615, H8 1737, H9 2069, H10 2361, H11 3414, H12 3450, release day 3522, §3 3564,
     §5-§6 3927-3964, §7 lessons 2, 13, 19, 25, 30, 31, 42, 43, 48, 52, 54, 57, 60, 64); PLAN-corpus-upgrade.md §6, §8
     (OQ-1, OQ-10, OQ-18); RESUME-SESSIONS.md 1e and 1h on claude/coord-handover 1af8d80c09 (lanes and boxes).
     Instrument states read at master 20279db5f6 on 2026-10-11: src/_roster.ps1:475 and :587, run-h10-recon.ps1:841-843,
     check-h6-completeness.ps1:76, shardmap.py:94, check-roster-format.ps1:906; src/go2cs/go.mod; module-cache x/tools and
     x/mod .mod files. -->

---

## Amendments

*(Dated. Each block states what changed and why.)*

**2026-10-12 — G's corrections for its stages, checked and applied** (G's message via COORD; the i9's
measurements; RECON X.7 on `claude/i9-go125-recon` `dc0e159ccf`):

- **H4.** The language cuts start after H1: `new(expr)` and self-reference typecheck only under
  go1.26's `go/types`. G's site list and estimates went into the H4 row (3–6.5 for G's share).
  - Every cited site was spot-checked at master: `convCallExpr.go:2846` and `:1244`,
    `visitValueSpec.go:579`, `callerInliningAnalysis.go:453`, `refLoweringAnalysisOperations.go:881`,
    `typedNilInterfaceBoxing.go:256`.
  - The 20 `.Constraint()` reads are 11 + 3 + 4 + 1 + 1 over 5 files.
  - The H4 total stays 8–14.
- **H1.** The gate is named: CNR byte-identical after the x/tools and x/mod bump. The converter suite
  takes about 40 min a run.
- **H6.** 15–25 → **30–45**. G proposed 25–38 from the 1.24.13 → 1.25.12 runtime half (18 of 71 runtime
  hand-own files changed, +1,312 / −654). The i9 measured the full 1.24.13 → 1.26.9 range: 76 upstream
  files, 9,600 changed lines, 20 of them large. At G's per-file rates that gives 30–45.
- **Totals.** **100–165 lane-days; 4–5.5 calendar weeks.**
- **O2.** G-LAPTOP's linux arm is named, with an ask-now note.
- **§1.** The publication date of 1.24.13.6 is corrected. The comment-parity precondition (H0 after its
  last train) is added. The instrument states were re-read at master `7092f7126e`, unchanged.
- **§4 note.** The two-leg H6 census (step 6) will show `runtime`'s 1.25 leg as already heavy, as G
  measured, before Green Tea's 1.26 leg.

**2026-10-12 (later) — O1 and O2 RULED** (owner, relayed by COORD, ledger 11:59):
- **O1:** 1.24.13.7 is the final 1.24 release, and H1 starts from it.
- **O2:** go1.26.9 may be installed on every lane's box; the i7 is done.
- **The H0 trigger is named:** the 1.24.13.7 release record lands.
- **go1.25.14** (the §4 checkpoint) is outside the ruling's words and stays an ask.
- **Readiness instruments A3, A7, A8 and A9 moved to P2** (dispatched 2026-10-12).
- **Status:** PROPOSED, with O1 and O2 RULED.

**2026-10-12 (evening) — A14 BUILT; go1.25.14 installed on the i9:**
- **A14**, the allocation-unit extractor, is BUILT by P2 on `claude/p2-hop-a14-alloc-unit` `0af043ecac`.
  It is tooling only, with footprint 0, and runbook H10 step 3(a) names it. It is no longer C1's.
- **go1.25.14 for the §4 checkpoint** is installed side by side on the i9 with the user's OK in chat,
  SHA-256-matched against go.dev, so the checkpoint box's half of O2 is closed. go1.26.9 is installed
  there too.

**2026-10-14 — the plan RULED GO; H0 open** (COORD, after 1.24.13.7 published from master `57ca7c1616`;
record `b0ef977fda`, index row `64ffbdac2a`):
- **Ruled:** the stage order exactly as written, with the runbook's release-fold text (§2, AMENDED
  2026-10-13, landed at `10cdccc564`). The checkpoint's gate blocks H6 and H7, not H5. **§4 now uses
  that text's wording:** H5c is applied in S25 only; the S25 ladder's stop is stated, and a RED26 site
  beyond it is read against both releases' source.
- **H0 base:** master `b594cd90af` (the record plus TRAIN T5, C1's AOT bound, the fold text and docs
  fixes); version.props `1.24.13` / build `7`.
- **Readiness re-read at the base:** A3, A7, A8, A9, A14 and the retired-marker guard are LANDED (TRAIN
  T4, P2's seats; §1 table). Open: P2's §9 notes; A12/O6, asked of the owner; host records at each lane's
  first leg; the BoGo host at H10. The post-hop seats (G runtime-settings, C1 cgi-symlink, the golib
  select-fairness fix) stay held and land as the first train after the hop's record, rebased onto the
  1.26 base.
- **Lanes, for a new constraint:** Anthropic cloud sessions (C1, C2, P2) have a fixed ceiling of about
  4 vCPU, 16 GB RAM and 30 GB disk. Heavy legs go to the Remote Control boxes, and the cloud lanes take
  code, docs, sizing and CI dispatch:
  - **H10 linux moves from C1 to R** on R-LAPTOP's WSL, with G as the second linux host. C1 keeps the
    H6 runtime history and darwin.
  - P2 keeps H4 linkname bodies and its H6 share, building package by package and purging between them.
    It keeps H8 only if H8's legs fit 30 GB; that disk need is measured at H0's first step.
  - R is the overflow battery host.
  - G runs the ILC generic-cycle root cause alongside, at lower priority.
- **The RECON roots are rebuilt, not reused.** They were seeded at `226afb810a`, before comment parity
  moved emission, so their counts (RECON-go1.26 §4-§6) are predictions only.
- **H0's first step and its predictions** (stamped by COORD):
  - Build the converter at the base and record its sha256, then run bare `go version`.
  - Run ONE seeded three-target go1.24.13 regen into a fresh H0 root.
  - (a) It equals the committed production corpus on all three targets (union-attributable 0).
  - (b) The fresh `.cs.auto` differs from the committed siblings on at least 24 of 32. Those 24 still
    carry the receiver and plain type attribute lines that face lifts A and B removed; they were last
    written 2026-09-13.
