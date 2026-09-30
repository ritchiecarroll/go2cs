# NOTES — the next Go hop (hop C: go1.24.13 → go1.25.14)

> **COORD's working notes. Opened 2026-09-29.** A point-in-time **record**: amended with dated blocks
> at the foot, never rewritten, never executed from. **The runbook leads.**
> [`../GoCorpusMigration.md`](../GoCorpusMigration.md) is the procedure. Where this note and the
> runbook disagree, the runbook wins, and the disagreement becomes an in-stage runbook edit (§6).
>
> **Status: PREPARATION.** The hop has not started. Under the fourth ordering (runbook §2, *AMENDED
> 2026-09-07*), H1 and H2 wait for the outgoing corpus's **final 1.24.13.N NuGet release**. Since the
> owner's direction of 2026-09-29 (ledger 08:27) that release follows **225/225, expected about
> 2026-10-01 to 10-02**, so the preparation window is days, not weeks (§4). This note holds:
> - the pre-hop checklist (§2), with an owner and a state for each item;
> - the known Go 1.25 bill (§3);
> - the timing constraints (§4);
> - the open questions (§5).
>
> **What the readings compare.** The reading box has no go1.25.14 SDK, so every measurement in §3
> compares **go1.24.13 with go1.25.13**. go1.25.14 (2026-08-19) differs from .13 only in net/http
> fixes. Every ⟲ step re-measures at the real target anyway. These readings size the hop; none of them
> is H0 or H3.
<!-- Opened under SIDE SEAT S4 (ledger 2026-09-28 03:42 · STAMP · e4a93b14c0: "COORD opens
     docs/phase4/NOTES-next-hop.md, and the -Hop sweep mode (00bee3f041, branch-only) lands"), created by the owner's
     roadmap rulings (ledger 2026-09-28 03:38 · RULING · 636e491d81; brief claude/coord-handover
     docs/phase4/briefs/roadmap-post100-2026-09-28.md, sections 2-6 and appendices A-B). The timing and parity frame
     is the owner's later direction: ledger 2026-09-29 08:27 · OWNER · e4a93b14c0 ("The 1.24 freeze planning date of
     2026-10-24 moves up to whenever 225/225 lands (about 10-01 to 10-02), then the final 1.24.13.N release (owner
     PIN), then hop C (1.25.14)"; the darwin run layer in parallel).
     Read at origin/master a1f133c3a9, claude/coord-handover 9a11dff385, claude/c2-sweep-hop-mode baf1fbe727,
     claude/r-sweep-hop-landing b46af2049e, and the ledger through 2026-09-29 12:53 (mailbox 8c07a349b6). Release
     notes: https://go.dev/doc/go1.25 and https://go.dev/doc/devel/release, read 2026-09-29.
     Raw readings (std lists per OS and toolchain, linkname O/T/added/removed, GOEXPERIMENT and testing-API dumps,
     the hand-own census output) are in COORD's hopprep scratch. They are not committed; §3 states what was read and
     on which axis. -->

---

## 0. Headline

- **Target: go1.25.14, and it cannot move.** OQ-1 freezes the target at hop start, and the 1.25
  series closed at .14, so a mid-hop restart is impossible. `PLAN-corpus-upgrade.md` §1.1's table
  still reads 1.25.12, which is stale by that table's own rule (§5 Q10).
- **No language changes.** On the converter side this is a library and behaviour hop. The heavy
  parts are these:
  - the hand-owned `testing` host (new `TB` members, `AllocsPerRun`'s parallel panic, the synctest
    hook);
  - `testing/synctest` entering the default package set;
  - **58 substantive hand-own files** for H6, against hop B's 46 (about 26% more: a larger H6,
    not a step change);
  - the `unicode` table regeneration, a strict-compare row.
- **The hop tooling is partly ready.** The `-Hop` sweep mode's population swap is fixed in a landing
  seat that is SEATED for TRAIN K, not yet landed (§2 A1). Still owed: per-row execution pins (A3),
  banking the per-row wall-time TSV (A4), and parse and acceptance evidence under both shells (A5).
  **Four more instruments still name 1.24** (A7, A8, A9, A13), and the go1.25.14 population of
  record does not exist yet (C7). §2 A lands before hop C's H10 recon leg, whose TSV is what the
  shard map reads. It does not gate the final 1.24.13.N release.
- **The 1.24.13 lessons are already on master.** Runbook §7, *Lessons from the 1.24.13 hop*, dated
  2026-09-26, holds 39 lessons, fixes A1–A13 and their in-place step edits. The H10 close amendment
  holds close lessons 1–7. Every seed item in the owner's 2026-09-23 order has a home there, so no
  second lessons section is owed. **COORD proposes (§5 Q11) that hop C's lessons land as a dated
  delta on §7.**
<!-- §7 landed via TRAIN C: ledger 2026-09-26 03:23 · ACCEPT · e7a3287d56 (lane C1). Owner order: ledger 2026-09-23 14:52 ·
     OWNER ORDER · c9e15c73a0. Seed-item coverage, checked 2026-09-29 against master a1f133c3a9:
     host/DNS/many-core/cloud quota = Readiness "Host" + §7.1 lesson 1; linux leg early (vgetrandom) = H10 "Order and
     launch" + lesson 30; alloc labels (reading run, relabel, rulings) = lesson 31; wrapper defects d095fe8108 = lesson 2;
     push measurement commits (4c5b0a3b7 page) = lesson 33; costed recon-basis row, //go:embed payloads + -text pins,
     emitter-moved flat files git rm'd = H10 close amendment "THREE LESSONS FOR THE NEXT HOP"; one message per event =
     lesson 3; go2cs.slnx non-Debug -p:go2csPath = §6 table row + close lesson 7 (.claude/rules/harness-gates.md:24
     already states the in-solution non-Debug case); repoguard payload admit = close lesson 5; staging seeder carries
     docs/validation = close lesson 4; csproj merge renderer keeps post-block groups = close lesson 6.
     The seed item with the thinnest home: "sub-agent batteries with briefs on coord-handover" (lesson 35 is the
     nearest). It is carried as §2 F3 and §5 Q11. COORD's 2026-09-29 §7.7 addendum draft (a dated sub-section inside
     §7) carries the step text some lessons never got; it is not a second section.
     H6 comparator: docs/phase4/AUDIT-h6-handown-go124.md:149-153 (hop B: 146 marked, 46 touched-substantive, 6 trivial,
     43 untouched, 51 no-counterpart). -->

---

## 1. Order of events

1. **The objective closes: 225/225, expected about 2026-10-01 to 10-02.** *(Owner direction,
   2026-09-29.)* This replaces the 2026-10-24 planning date. Runtime work still open at the freeze
   re-banks at 1.25. The exit criterion is Windows and Linux at 100% honest validation. A **Darwin
   run layer goes in parallel** on one or two lanes, on the CI macOS runners, off hop C's critical
   path.
2. **TRAIN K lands before the release.** *(COORD PROPOSAL, not ruled; §5 Q13.)* TRAIN K's draft
   carries the two .NET side seats, the `-Hop` landing seat and seats unrelated to the hop, and it
   lists the runtime row bank among its pending seats, so it may itself carry the freeze.
3. **The final 1.24.13.N release** ships, with H12's ritual in full and the owner's PIN at the
   console. It freezes the outgoing roster, proof pages and READMEs, and it is the last moment the
   outgoing corpus can be published at its own release. *(Ruled.)*
4. **Hop C runs H1 and H2 as one pair**, then H3 onward per runbook §2. *(Ruled.)*
5. **Afterwards:** the .NET 11 measurement stage, never overlapping a Go hop. Hop D (1.26.x) is a
   separate hop, and hop E (1.27) follows by about 2027-02. *(Ruled.)*
<!-- Items 3-5 and the 1.24 re-bank rule: ledger 2026-09-28 03:38 · RULING · 636e491d81 (items 1, 3, 4). Item 1's date
     and the darwin run layer: ledger 2026-09-29 08:27 · OWNER · e4a93b14c0, which amends 03:38's "Darwin COMPILE-ONLY
     until a Mac run layer exists" ("it now runs in parallel, off the critical path"; .github/workflows/os-matrix.yml
     already carries macos-15 and macos-15-intel legs). The owner-approved docs/Roadmap.md at a1f133c3a9 agrees
     ("October 2026: A final Go 1.24 package release, then the Go 1.25 hop"; the macOS run layer "in parallel with the Go
     hops, off their critical path"). Item 2: neither 636e491d81 nor the 08:27 line names TRAIN K; the TRAIN K draft is
     claude/coord-handover 9a11dff385 .claude/coord-scripts/trainK/tK-seats-draft.txt (S1 272afeef93, S2 ac2fefdf88,
     field-pointer equality 1176230dec, the -Hop landing b46af2049e; PENDING includes "the runtime row bank").
     Fourth ordering: runbook §2 AMENDED 2026-09-07 (migrate-gorelease.ps1 resets <GoBuildNumber> at the pin stage).
     release/go1.24 is minted at the 1.25 cutover, not before (RESUME-SESSIONS on claude/coord-handover). -->

---

## 2. The pre-hop checklist

**Owner types:** **COORD** (rulings, seating, records, gates) · **a lane** (cut, measure, gate) ·
**the owner** (hands, accounts, money, the release PIN).
**States:** NOT STARTED · FOUND (named here and not yet ruled) · RULED (seat named, not cut) ·
CUT (branch-only) · SEATED (in a train, not landed) · LANDED · DONE.

### A. Tooling (Readiness "Instruments": each item landed on master and red-proved before the stage that needs it)

| # | Item | Owner | State |
|:--|:--|:--|:--|
| A1 | **`-Hop` landing seat.** The skeleton path comes from GOROOT's `VERSION`, the answer the pin guard trusts. `-Hop` refuses by name when that release's skeleton is missing, never falls back to another release's, and checks this before the pin guard. Four controls fired: the go124 skeleton reads 227 rows, the 10 `Receives` cells and 0 Expected; `-Hop` at go1.24.13 reads it; a faked go1.25.14 refuses and names the go125 skeleton; a non-hop sweep matches master's in every header and verdict line. A discriminating arm, with `version.props` bumped as H2 does, shows the unfixed script silently loading the go124 skeleton and the fixed one refusing. **Interim:** it still reads a version-derived CENSUS file name (`CENSUS-h10-eligibility-go<MM>.md`) until A2 switches it to the DATA artifact. One stated deviation: it writes a git-ignored per-row wall-time file on every run. | a lane; COORD seats | **SEATED** (TRAIN K, `b46af2049e`) |
| A2 | **The skeleton generator**, the seat that completes A1. It runs `go list` axis C over the incoming GOROOT (test files that survive the target's constraints AND declare a `func Test*`, with the corpus tags `purego,math_big_pure_go`). It writes a **DATA** artifact under `docs/phase4/data/`, and `-Hop` reads that file, never a CENSUS record. Positive control: run against go1.24.13, it reproduces the go124 skeleton's 227 rows, and any difference is explained row by row before the tool is trusted. The control needs only the 1.24.13 GOROOT; **the 1.25.14 artifact waits on B0.** | a lane | RULED for TRAIN K; NOT STARTED |
| A3 | **`-Hop` must apply roster execution pins.** Skeleton rows carry `Execution = $null`, and the sweep builds its per-row arguments from `$row.Execution`, so no pin reaches a hop row. At master **three banked rows carry a pin, all `release-tiered`**: `internal/godebug`, `log/slog` and `net/http`. No row carries `release-tc0`. This is the defect `d095fe8108` fixed in the recon wrapper (lesson 2). The fix: carry each row's pin from the banked roster into its skeleton row. A relocated row takes its predecessor's pin, and a re-entering row, such as `net/http`'s candidate cell, carries its pin too. The firing control uses one of the three pinned rows. **Not in the landing seat.** | a lane; COORD rules the seat (Q1) | FOUND |
| A4 | **Bank the per-row wall-time TSV.** The sweep writes it to `scratchpad/sweep-row-walltimes/`, which `.gitignore` excludes (**still so in the landing seat**). Under §3.4 the inputs are banked in the commit that claims them: the TSV becomes a new (OS, SHA, machine) section of `docs/phase4/DATA-sweep-row-walltimes.md`. This is hygiene and §3.2's cost proxy. Hop C's shard map is emitted from **hop C's own recon-leg TSV** (runbook H10's launch amendment). Hop B's recon TSVs in `docs/phase4/hopA-inputs/recon-tsv/` are the ruled basis for balancing hop C's recon lists (§7 lesson 36). | a lane (the sweep's runner); COORD checks the commit | FOUND |
| A5 | **Execute `-Hop` under both shells.** The landing seat's controls executed it (A1). Owed: a parse gate under PowerShell 5.1 and pwsh 7, plus an acceptance run on one banked row, each stated by shell. Readiness requires both shells. | a lane | PARTIAL |
| A6 | **Sequencing.** Land A1–A5 before hop C's H10 recon leg. Having them landed before the final 1.24.13.N release's §6 sweep is desirable, so that the sweep's walls bank as §3.2's proxy, but **it is not release-gating**: under the compressed timeline, instrument seats stay off the release's critical path. | COORD (sequencing) | constraint |
| A7 | **`src/run-h10-recon.ps1`**: the three-way pin assertion hard-codes `go1\.24\.13` (the `Deny` lines at 841–843). It should read the pin from `src/version.props`, with a firing control at a faked release. | a lane | FOUND; NOT STARTED |
| A8 | **`src/check-h6-completeness.ps1`**: `-AuditFile` defaults to `docs/phase4/AUDIT-h6-handown-go124.md` (line 76). Run without the flag at hop C, it silently reads the old audit, the same hazard A1 fixes. **First rule one naming convention (§5 Q14)**, then cut A8 to derive the file from the incoming release and refuse by name when it is missing. | a lane, after Q14 | FOUND; NOT STARTED |
| A9 | **`docs/phase4/hopA-inputs/shardmap.py`**: `POPULATION_OF_RECORD` names `population-go1.24.13.txt` (line 94). Parameterize it by release, and have it refuse when the named population is missing. It reads the same file as A13. | a lane | FOUND; NOT STARTED |
| A10 | **Red-prove A1–A9, A13 and A14**: regress one site, confirm the instrument names it, restore, and verify the result is byte-identical (safety floor 13; lesson 2). | the cutting lane; COORD judges | NOT STARTED |
| A11 | **H11's existence-plus-monotonicity check is on master.** `push-nuget.ps1` reads local AND origin tags (the origin note at :445, `Get-GoOriginReleaseTags` at :558, the union at :740). The last release read 9 origin tags, 0 missing locally. **Gap:** when origin is unreadable it prints a yellow warning and compares the LOCAL tags only (:733–737, :749). It degrades by design instead of refusing (§5 Q15). | COORD | ON MASTER; one ruling owed |
| A12 | **x/tools and x/mod.** The converter pins x/tools v0.42.0 and x/mod v0.33.0, both `go 1.24.0`. **The binding constraint is a ceiling, not a floor.** A version declaring `go 1.26` or later cannot be built by a go1.25.14 converter under `GOTOOLCHAIN=local`, and would switch toolchains silently under `auto`. The newest versions in the module cache, x/tools v0.49.0 and x/mod v0.40.0, declare `go 1.25.0`. The first versions read that declare 1.25 (x/tools v0.47, x/mod v0.37) are not floors: the cache holds no x/tools v0.43–v0.46 or x/mod v0.34–v0.36. **H1 step 3 is gated by H1 step 2** (the converter's `go` directive moves to 1.25.14 first). The bump is its own commit and CNR (OQ-5). | a lane, at H1 | constraint (§5 Q9) |
| A13 | **`src/check-roster-format.ps1`** hard-codes `docs/phase4/hopA-inputs/recon-lists/population-go1.24.13.txt` (line 896). The roster guard's four population assertions (the header's N, banked and excluded membership, implementable) would keep measuring the 1.24 population at hop C. Derive the path from `version.props` as `push-nuget.ps1` already does (:617–622), refuse by name when the file is missing, and add a firing control. | a lane | FOUND; NOT STARTED |
| A14 | **The allocation-unit extractor** that reads each alloc entry's `AllocsPerRun` unit note from the results JSON. At hop B it existed only as a method in an evidence branch's README. Lesson 31's reading run needs it as a landed, red-proved instrument. | a lane | FOUND |
<!-- A1: ledger 2026-09-29 12:51 · SEAT · b46af2049e (claude/r-sweep-hop-landing, a signed union merge of
     claude/c2-sweep-hop-mode baf1fbe727 plus the per-release skeleton commit; 2 files +474/-14; ACCEPTED for TRAIN K).
     The derivation is at run-validated-sweep.ps1:224-245 on b46af2049e ($hopRelease = "go<major><minor>"). Earlier
     read: ledger 2026-09-29 12:39 · RULING · baf1fbe727 (skeleton path hard-coded at run-validated-sweep.ps1:191 to
     docs/phase4/CENSUS-h10-eligibility-go124.md, which exists on master, so -Hop would SILENTLY sweep the 1.24 eligible
     population at hop C; items (1)+(2) the landing seat, (3) the generator). A2: 12:39 item (3); the 12:51 line
     calls it the seat that "completes ruling 8f0a3e4f7e" and records that the 1.25.14 artifact "waits on the OWNER's
     OK for the Go 1.25.14 toolchain download".
     A3: found 2026-09-29 by COORD's read of baf1fbe727 (not in the 12:39 ruling); still present at b46af2049e
     (src/_roster.ps1:466 `Execution = $null` in Get-HopSkeletonRows). Pins at master: docs/ValidatedTestPackages.md
     rows :361 (internal/godebug), :392 (log/slog), :406 (net/http), each "· execution: release-tiered"; the net/http
     candidate cell at :966 carries none. The recon wrapper's fix: src/run-h10-recon.ps1:616-626 at master ("THE PER-ROW
     INVOCATION: ITS DEADLINE AND ITS EXECUTION PIN, BUILT IN ONE PLACE"), commit d095fe8108.
     A4: b46af2049e run-validated-sweep.ps1:1921 ($timingDir = scratchpad/sweep-row-walltimes); master .gitignore
     "scratchpad/" ("never commit"). The DATA record's own header: "Add a section per new (OS, SHA, machine)
     measurement at each hop recon; do not overwrite old sections — supersede them." Runbook H10 launch amendment
     (RECON LEG -> ROSTER SEAT -> PLAN -> DRIVER: the recon leg "banks the per-row TSV the map reads"); §7 lesson 36.
     A5: the branch's author had no PowerShell (12:39: "NO tests or controls of its own"); 12:51 lists the controls
     that fired, with no shell stated.
     A7-A9, A13: read at origin/master a1f133c3a9 (run-h10-recon.ps1:841-843; check-h6-completeness.ps1:76;
     shardmap.py:94; check-roster-format.ps1:896; push-nuget.ps1:617-622 composes population-go$preflightGoPin.txt from
     <GoStdLibVersion>). A11: push-nuget.ps1 at a1f133c3a9; ledger 2026-09-24 04:59 · STAMP · ffa5c1015a ("9 origin
     tags, 0 missing locally"). A12: the converter's go.mod at master; module-cache .mod files under
     golang.org/x/tools and golang.org/x/mod, read 2026-09-29. A14: claude/coord-h10-readings ac9f8251ee
     (docs/phase4/h10-evidence/i7-readings/README.md "How the unit was extracted"; the README and TSV only, no script),
     not an ancestor of master. -->

### B. Hosts (Readiness "Host": a per-box record, taken and read before H0)

| # | Item | Owner | State |
|:--|:--|:--|:--|
| B0 | **OWNER-HAND: the go1.25.14 toolchain download.** Ask now for the owner's OK to install go1.25.14 side by side under each box's `~/sdk`, never auto-fetched. It gates A2's 1.25 artifact, C1, C3 (the Go control side must run on the target toolchain), C7 and B1, and the hop itself on every host. | **the owner**; COORD asks | OPEN |
| B1 | **Install go1.25.14 side by side** under `~/sdk` on every native arm and every WSL arm, once B0 is given. Auto-fetched toolchains are read-only and are not an install. Record each box's `GOTOOLCHAIN` and `GOROOT` pins, spelling `GOROOT` exactly as `go env GOROOT` prints it (safety floor 6). Watch two-sided boxes (§3.3). | each lane for its box; the owner where hands are needed | waits on B0 |
| B2 | **DNS**: public IPv4 resolvers per adapter, IPv6 unbound where there is no v6 path, qualified by Go's own `go test -count=1 -timeout 40m net` (`TestLookupCNAME` is the tolerated drift). Re-qualify every box, including boxes qualified during hop B, because router and adapter state drifts. | each lane; the owner for router or adapter changes | NOT STARTED |
| B3 | **The rest of the Readiness host record**: WSL `localhost` resolves `::1`; unelevated symlinks (Developer Mode) and `LongPathsEnabled=1`; pwsh 7 and the pinned .NET SDK reachable by every launcher (after TRAIN K, the pinned SDK is `global.json`'s). **Record each linux arm's .NET build provenance**: Microsoft or distro build, and the runtime patch. Before triaging an "Internal CLR error" (0x80131506) on a cloud lane, re-run it on Microsoft's .NET build (fleet rule, 2026-09-29). The signature alone never clears golib. | each lane | NOT STARTED |
| B4 | **A many-core Windows host for crypto/tls's standard BoGo wall.** Only `GOFLAGS` reaches BoGo's nested `go test`, so raising the wall is an owner ruling. **Request any cloud quota now**, because quota lead time is outside the fleet's control. | the owner | OPEN |
| B5 | **Disk preflight** per box: the sweep floors at 25 GB. Purge build output between runs (safety floor 12). | each lane | at each battery |
<!-- B0: ledger 2026-09-29 12:51 · SEAT · b46af2049e ("the 1.25.14 artifact waits on the OWNER's OK for the Go 1.25.14
     toolchain download, which the hop itself also needs on every fleet host"); the runbook's pre-staging technique
     puts the Go control side on the target toolchain, "verified by go version OUTPUT, per H1, never by a file".
     B3's rule: ledger 2026-09-29 07:49 · RULING · 7bf4ac26cd (FLEET RULE (1); the OSR crash reproduced only on one
     cloud lane's linux box, on .NET 10.0.12, with that box's build provenance queued for reading).
     Readiness bullet (runbook §2, edited in place 2026-09-26 from §7 lessons 1-3). The owner excluded hardware-specific
     host issues from the lessons as temporal; B-items stay generic on purpose. §3.3 of the runbook covers two-sided
     boxes (a native and a WSL toolchain on one machine). -->

### C. Recon and pre-staging (off the critical path; start as soon as a lane is idle)

| # | Item | Owner | State |
|:--|:--|:--|:--|
| C1 | **`docs/phase4/RECON-go1.25-hop.md`**, shaped like `RECON-go1.24-hop.md`: one seeded `-stdlib` conversion with a **scratch-only** 1.25.14 pin (the repo's `version.props` is never touched); the compile ladder; the hand-own census (§3.10 is a first reading); the `go list std` census (§3.8); and the testing-host bill sized by receiver-typed call sites of the new members (§3.2; lesson 13). Also: a census of the `AllocsPerRun` asserts that changed between 1.24.13 and 1.25.14 and the deferred entries they touch (lesson 31's path; §3.1); the rows that observe each runtime change in §3.4 (Q8); and a re-census of the anonymous-struct-as-generic-type-argument construct (§3.12). Seed the scratch root from `src/core` first (safety floor 2), and pass the output directory as the second positional (floor 3). | a lane | waits on B0 (brief Initiative 4) |
| C2 | **This note is committed** to `docs/phase4/NOTES-next-hop.md`. It is absent on both master and coord-handover today. | COORD | DRAFT (this file) |
| C3 | **Pre-stage the flagged rows, conversion only** (the H10 technique), ranked by runbook §4's fragility rule: strict-compare rows carrying upstream-changed production code come before rows with manifests. **Tier 1, strict, with substantive hand-own changes:** `internal/synctest` (28), `iter` (28; `iter_impl.cs`), `internal/godebug` (5, release-tiered pin; `godebug.cs`, and `fips140` becomes `Immutable`). **Tier 2, strict, with converted-code changes:** `unicode` (28), `crypto/elliptic` (82), `crypto/x509` (518), `net/http` (1387, release-tiered pin; the only package 1.25.14 changes), `internal/trace` (92), `runtime/trace` (2), and `go/ast` (9; it receives relocated tests). **Tier 3, rows with manifests, most with substantive hand-owns:** `unique` (21+1; `clone.cs`, the `registerUniqueMapCleanup` pair removed, eager reclamation, new `runtime_rand` and `blockUntilEmptyCleanupQueue` pulls), `os` (1105+2; `Root` gains 12 methods, `file_windows_impl.cs`, overlapped `NewFile`), `crypto/tls` (4759+1), `sync` (46+6; `waitgroup.cs`, `once.cs`, `oncefunc.cs`, `runtime_impl.cs`), `testing` (53+15; `testing.cs`) and `reflect` (396+22; `value_impl.cs`, `TypeAssert`). **Reserved set:** the runtime family (`runtime`, `runtime/debug`, `runtime/pprof`, whichever bank at the freeze). It carries 25 of the 58 substantive hand-owns and the `[recovered, repanicked]`, `GOMAXPROCS` and `AddCleanup` changes. The full `runtime` row read 10,890 verdicts in 6,545 s on windows at Release TC0. These are hop C's hardest H10 rows. | a lane | waits on B0 |
| C4 | **H4 items that can land at the outgoing corpus.** Testing-host members that compile at 1.24 may land early if they are gated green at the outgoing corpus (lesson 13). Target-only registry entries, meaning the new linknames and synctest pulls, ride the version branch with the H5 set (lesson 14). | a lane; COORD rules each | NOT STARTED |
| C5 | **NuGet ID for `testing/synctest`**, the one new public package. Its ID follows the corpus scheme (`PackageId` = `go.$(AssemblyName)`, so `go.testing.synctest`) under the existing publisher. File the reservation when H3 names the new IDs (lesson 12); the filing is an owner hand. Whether the new internal packages are packable is H3's call. Removed IDs are G1. | COORD drafts; the owner files | NOT STARTED |
| C6 | **Third-party seeds stay off the hop's critical path.** No hop step waits on a seed. | COORD | ruled |
| C7 | **The population of record for go1.25.14**: `docs/phase4/hopA-inputs/recon-lists/population-go1.25.14.txt`, enumerated by **two independent routes** (runbook H10, *AMENDED 2026-09-22*). A2's axis-C skeleton is not one of the two. It is a precondition of the roster seat, of A9 and A13, and of H11/H12: `push-nuget.ps1`'s release preflight refuses without it. | a lane; COORD checks that the routes agree | waits on B0 |
<!-- C1: brief roadmap-post100-2026-09-28 Initiative 4 ("RECON-go1.25-hop"). C3: roster cells at a1f133c3a9 (Tests +
     Disclosed); hand-own names from the hopprep census (THE H6 REVIEW LIST); runbook §4 "A row with NO manifest compares
     strictly ... rank strict-compare rows carrying upstream-changed production code ABOVE rows with large manifests";
     runtime wall: ledger 2026-09-29 12:53 · FINDING · 61cf81290c ("the FULL windows runtime row at Release TC0 in
     6,545 s ... all 10,890 verdicts present"); runtime, runtime/debug and runtime/pprof are not banked at a1f133c3a9,
     and the 08:27 freeze is 225/225. C4: runbook §7 lessons 13 and 14. C5: runbook §7 lesson 12; ID scheme from the
     corpus csproj template (e.g. src/core/internal/trace/event/internal.trace.event.csproj:64). The owner's open item 5
     (ledger 2026-09-28 03:38) is the nugetgo third-party identity, not the stdlib IDs. C6: the brief's critic item 4.
     C7: runbook H10 lines 2443-2449 at a1f133c3a9 ("a population is an ENUMERATION checked by two INDEPENDENT routes");
     push-nuget.ps1:617-622. -->

### D. Outgoing-side captures at the final 1.24.13.N tree

| # | Item | Owner | State |
|:--|:--|:--|:--|
| D1 | **The §6 full sweep** of the final 1.24.13.N tree. If A1 and A4 have landed by then, its per-row walls bank as §3.2's proxy. It does not wait on them (A6). | a lane (sweep host); COORD gates | waits on the freeze |
| D2 | **The final 1.24.13.N release**: H12's ritual in full, the pre-pack signed tag, the write-once proof snapshot, both badge retargets, and the recomputed re-verification. | COORD prepares; **the owner** at the console (PIN) | waits on the freeze and on the TRAIN K seats Q13 rules release-gating |
| D3 | **H0's three comparands**: the outgoing three-target `-platform-census` manifest, the behavioral failing set by name, and each box's bare `go`. | a lane; COORD reads | NOT STARTED |
| D4 | **Freeze every current proof page into the snapshot and commit any unbanked reading** (lesson 4). **Push every measurement commit before a page cites it** (lesson 33). | COORD | at D2 |
| D5 | **The perf re-baseline**, which also serves as hop C's "before" side. | a lane | NOT STARTED |
| D6 | **`release/go1.24`** is minted at the 1.25 cutover, not before. | COORD | ruled |

### E. The .NET side seats and the hop (one variable at a time)

| # | Item | Owner | State |
|:--|:--|:--|:--|
| E1 | **S1, the .NET pin**: `global.json` plus `LangVersion` 14 in the emitted csproj template, the props file and the nine hand-written csproj. It rides TRAIN K together with its regeneration: 1,351 csproj predicted, each −1/+1, with zero `.cs`, `.cs.target` or map lines. **COORD PROPOSAL (Q13): it lands before the final 1.24.13.N release, never mid-hop**, so hop C's H5 inherits it rather than measuring it. | COORD seats | SEATED (TRAIN K) |
| E2 | **`DotNetMigration.md` §4 step 4 is stale against the owner's ruling.** The owner ruled on 2026-09-28 (item 4) that `global.json` plus a pinned `LangVersion` land before 2026-11-10. Step 4 still says to pin the SDK "once the TFM moves (§5), not before". It owes an in-stage amendment recording the newer ruling, a current-major pin ahead of the TFM move, landing with TRAIN K. S1's file carries only the `sdk` key and no `test` key, as §4's warning requires. | COORD | FOUND (amendment owed) |
| E3 | **S2, the C# 15 identifier escape** (`closed`, `union`, `safe`). | COORD seats | SEATED (TRAIN K) |
| E4 | **The .NET 11 measurement stage** comes after hop C and never overlaps a Go hop. Hop D's recon may run beside it; hop D's H1 waits for that stage to close. **The stage must also say how its legs get past S1's pin.** §4 step 4 requires both SDKs to stay selectable by environment during the SDK-only and baseline stages, "which is precisely what a pin fights". S1's pin (10.0.100, `rollForward: latestFeature`) never selects an 11.x SDK inside the repo. The stage either runs its legs outside the repo (§4 step 5's throwaway projects already do), or makes a per-stage `global.json` change that is itself the stage's one variable. | COORD | ruled (ordering); the override is owed |
<!-- E1: ledger 2026-09-29 09:21 · SEAT · 272afeef93 (claude/p1-s1-dotnet-pin; RED b6d540c300; global.json
     {"sdk":{"version":"10.0.100","rollForward":"latestFeature","allowPrerelease":false}}; predicted 568 core, 768
     behavioral, 15 performance csproj; "It cannot ride TRAIN J alone"). E3: ledger 2026-09-29 12:15 · SEAT · ac2fefdf88
     (claude/r-s2-cs15-escape), sized at 11:09 · RULING. E2: ledger 2026-09-28 03:38 · RULING · 636e491d81 item (4);
     DotNetMigration.md §4 step 4 at master a1f133c3a9 lines 175-178, with the "no test key" warning at 180+. E4:
     PLAN-hop-campaign.md §5 ("the one ordering with no defense"); DotNetMigration.md §1 (the one-variable rule), §4 steps
     4-5. -->

### F. Fleet and comms

| # | Item | Owner | State |
|:--|:--|:--|:--|
| F1 | **Addressed comms from day one**: ledger, per-lane inboxes, one message per event, one OWNER-HAND line per owner action, and resume prompts written on the rule that nothing local survives a restart. | COORD | standing (protocol v4) |
| F2 | **Re-estimate hop C for the lanes available.** Hop B took about 16.4 days, tag to tag, with six lanes. Hop C's roster is **seven lanes plus COORD**: five mailbox-only lanes (two of them returned on 2026-09-28) and two direct lanes. Model class and effort are COORD's call. **Credits are the hard ceiling**, so the estimate is in usage weeks. One cloud lane cannot run disk-heavy gates, and some hosts carry exclusions. | COORD | OPEN (§5 Q3) |
| F3 | **Sub-agent batteries run from briefs committed on coord-handover**, so the battery and its record survive a session loss. | COORD | standing; thin home in §7 (§5 Q11) |
| F4 | **One worktree per cut and one conversion per output root**, with no overlapping conversions on one box (safety floors 1 and 11). Assign the H5, H8 and H10 shards with this in mind. | COORD | standing |
<!-- F2: RESUME-SESSIONS on claude/coord-handover 9a11dff385, section 1e.0 (owner order 2026-09-28 18:40: R, C1 and C2
     move to new sessions and P1 and P2 return, all five MAILBOX-ONLY; G, the i9 and COORD unchanged); ledger 2026-09-29
     08:12 · OWNER · 2ff42f7a16 (model class and effort delegated to COORD); the brief's line 48 ("calendar estimates
     are in credit-weeks") and its critic item 2, written when P1 and P2 were out. Hop B's six lanes: the brief's
     appendix A row. -->

### G. H11 and release-day items known now

| # | Item | Owner | State |
|:--|:--|:--|:--|
| G1 | **Removed-ID deprecations**, an owner step after hop C's publish (runbook H11: deprecate with a pointer to the last release that carried the ID, never unlist; Release day step 5; §7 lesson 38). Packable IDs at master whose Go package 1.25 removes: `go.internal.trace.event`, `go.internal.trace.event.go122`, `go.internal.trace.internal.oldtrace`, `go.internal.trace.internal.testgen.go122`, `go.log.slog.internal.slogtest` and `go.runtime.internal.startlinetest`, plus darwin's `go.vendor.golang.org.x.net.route`. The `go/ast/internal/tests` and `runtime/internal/wasitest` test projects are `IsPackable` false. **First confirm which of these IDs 1.24.13.1 and .2 actually published**, and point each at the last release that did. | COORD drafts; **the owner** deprecates | NOT STARTED |
<!-- G1: csproj at a1f133c3a9 (`<PackageId>go.$(AssemblyName)</PackageId>` in each production csproj;
     go.ast.internal.tests.tests.csproj and runtime.internal.wasitest.tests.csproj carry <IsPackable>false</IsPackable>);
     runbook H11 lines 3301-3302 and Release day (line 3397). -->

---

## 3. The known Go 1.25 bill

Axis for §3.8–§3.10: toolchains go1.24.13 and go1.25.13, `CGO_ENABLED=0`, no tags, default
GOEXPERIMENT, run from a directory with no module.

### 3.1 Language and compiler — H4, and H9/H10 classification

- **No language changes.** The spec dropped "core types" in favour of prose.
- **The nil-check fix.** A compiler bug introduced in Go 1.21 "could incorrectly delay nil pointer
  checks". Go now panics where it did not before. The C# side never had the bug, so differential rows
  can flip in either direction. **R11: classify closures as carefully as breaks, and retire a
  disclosure that closes only with evidence** (runbook §4, "classify closures").
- **More slice backing stores go on the stack.** This lowers Go-side allocation counts, and upstream
  alloc asserts may tighten, so the deferred disclosures' `want` and `reading` can move. It follows
  lesson 31's path (reading run, relabel, rulings), and C1's census sizes it. Revert for diagnosis:
  `-gcflags=all=-d=variablemakehash=n`.
- **DWARF5 is the default.** It affects measurement only, never emitted C#. Revert:
  `GOEXPERIMENT=nodwarf5`.
<!-- go.dev/doc/go1.25 (issue 72860). PLAN-corpus-upgrade.md §1.2-§1.3 ("a pure library/behavior hop for the converter")
     and §5 R11. -->

### 3.2 The hand-owned `testing` host — OQ-3 names it an H4 item; R12

- **New members**: `(*common).Attr(key, value string)` and `(*common).Output() io.Writer`. `TB`
  gains both, and `T`, `B` and `F` inherit them. Size the bill by receiver-typed call sites, not by a
  name census (lesson 13).
- **`AllocsPerRun` panics during parallel tests** with "testing: AllocsPerRun called during parallel
  test" when `parallelStart != parallelStop`. Go's own suites therefore never call it while parallel
  tests run. **The parity risk runs the other way:** a host whose parallel accounting differs from
  Go's panics where Go does not. Port the panic with Go's exact `parallelStart`/`parallelStop`
  accounting, each counter incremented where Go increments it, and test it by name.
- **The synctest hook.** `testing.testingSynctestTest` is a push linkname into `testing/synctest`. It
  builds a child `T` with `isSynctest: true`, and `T.Deadline` panics inside a bubble. **Without this
  function the host leaves `testing/synctest.Test` with no body.**
- **`go test -json` emits a new `attr` action** (`=== ATTR`). The converter's verdict switches accept
  only pass, fail, skip, timeout and infrastructure-error, so the new action is ignored. It looks
  inert; confirm it (§5 Q4).
- **`testing/fstest`** (converted, not hand-owned): `MapFS` gains `Lstat` and `ReadLink` and so
  implements `ReadLinkFS`; `TestFS` no longer follows symlinks.
<!-- Measured API delta go1.24.13 vs go1.25.13 (testing-api-O/T readings). testing/allocs.go:21-22 (the panic);
     testing/testing.go:430-431 (the counters), :1710 (parallelStart.Add), :1847 (parallelStop.Add), :2020
     (testingSynctestTest). Verdict switches: src/go2cs/testConversion.go:8273, :9217, :9235 at master. Release notes:
     "The AllocsPerRun function now panics if parallel tests are running." -->

### 3.3 `testing/synctest` enters the corpus — OQ-2

- It graduates into the default package set with `Test(t, f)` and `Wait()`. The old API survives only
  under `GOEXPERIMENT=synctest`, and 1.26 removes it.
- **`internal/synctest` gains four runtime pulls**: `IsInBubble`, `associate`, `disassociate` and
  `isAssociated`. They extend the hand-owned `synctest_impl.cs` that the 1.24 synctest arc built.
- **`sync.WaitGroup` calls `synctest.IsInBubble` and `synctest.Associate`**, and `sync` is
  hand-owned. **`time.runtimeIsBubbled`** is a new pull.
- **`encoding/json/v2` and `encoding/json/jsontext` stay OUT**: both are experiment-gated (OQ-2, R13).
<!-- runtime/synctest.go:327-451 (the linknamed pulls); sync/waitgroup.go:87-90; time/time.go:1328. -->

### 3.4 Runtime — H6 input and the golib panic printer

- **Container-aware `GOMAXPROCS`** with periodic re-reads on every OS, and a new
  `runtime.SetDefaultGOMAXPROCS`.
- **Unhandled-panic output**: a panic that was recovered and repanicked now prints
  `panic: X [recovered, repanicked]` and no longer repeats the value. This touches golib's panic
  printer and the runtime crash tests.
- **`AddCleanup` cleanups run concurrently and in parallel**, so they may finish out of order.
- Also new: VMA decoration on linux, the `checkfinalizers` GODEBUG, the trace flight recorder
  (`runtime/trace.FlightRecorder`, four new linknames), and more eager `unique` reclamation.

### 3.5 Library changes that touch banked rows (C3 ranks them)

| Row | What changes at 1.25 |
|:--|:--|
| `unicode` | Tables regenerated: `CategoryAliases`, `Cn`, `LC`, and `C` now includes `Cn`. **The row has no manifest, so it compares strictly, and §4's fragility rule ranks it above rows with large manifests.** |
| `crypto/elliptic` | The hidden, undocumented `Inverse` and `CombinedMult` methods are removed. |
| `hash/*`, `crypto/*` | Every stdlib `Hash` implements the new `hash.Cloner`; `hash.XOF` is new. |
| `sync` | `WaitGroup.Go`, plus the bubble association (§3.3). |
| `os` | `Root` gains 12 methods; `DirFS` and `Root.FS` implement `ReadLinkFS`; `CopyFS` follows symlinks; `NewFile` supports overlapped I/O on Windows. |
| `net` (Windows) | `FileConn` and friends, through the new `os → net.newWindowsFile` linkname. |
| `reflect` | `TypeAssert`. |
| `unique` | More eager reclamation; the `registerUniqueMapCleanup` pair is removed; new `runtime_rand` and `blockUntilEmptyCleanupQueue` pulls. |
| `internal/godebug` | The `Immutable` field (`fips140`). |
| `crypto/tls` | SHA-1 is disallowed in TLS 1.2 handshakes (GODEBUG `tlssha1=1` re-enables it); stricter compliance; FIPS rules. |
| `crypto/x509` | SubjectKeyId uses truncated SHA-256 (`x509sha256skid`); a negative pathLen is rejected. |
| `go/ast`, `go/parser` | Deprecations (the APIs are still present). `go/ast` also receives relocated tests (§3.8). |
| Also | `regexp/syntax` class names, `log/slog`, `mime/multipart`, `net/http.CrossOriginProtection`, and the `runtime/pprof` mutex profile's end-of-critical-section attribution. |

### 3.6 GODEBUG defaults — H1 step 8

- **New entries in `internal/godebugs/table.go` with a change marker at 25**: `containermaxprocs`
  (old 0), `updatemaxprocs` (old 0), `decoratemappings` (old 0), `tlssha1` (old 1),
  `x509sha256skid` (old 0).
- **New table entries with no change marker**: `fips140ems`, `htmlmetacontenturlescape`, and
  `embedfollowsymlinks` (cmd/go only).
- **Modified**: `fips140` became `Immutable`. **Nothing was removed from the table.** No
  Windows-filesystem default changed this release, unlike 1.24's `winsymlink`.
- **Outside the table**, the runtime's own `GODEBUG` variables (`runtime/runtime1.go`) lose
  `runtimecontentionstacks` and gain `checkfinalizers`.
<!-- Measured in internal/godebugs/table.go and runtime/runtime1.go (dbgvars), go1.24.13 vs go1.25.13. -->

### 3.7 GOEXPERIMENT — H3, H4a and H5c

- **The baseline drops `CoverageRedesign` and `SpinbitMutex`** (both flags are removed) and **adds
  `Dwarf5`** where supported. The new flags `greenteagc` and `jsonv2` are off by default.
- **H5c**: expect the `internal/goexperiment` pairs `exp_coverageredesign_*` and `exp_spinbitmutex_*`
  to be DELETE-ABSENT, and `exp_dwarf5_*`, `exp_greenteagc_*` and `exp_jsonv2_*` to appear. This is
  the CS0102 class hop B met.
- **H4a / H6**: `runtime/lock_spinbit.go` is now `//go:build !wasm`, and the
  `lock_{sema,futex}_tristate.go` files are gone. Re-read `lock_managed_impl.cs` against the
  unconditional spinbit file.
- **H3**: counts state the new baseline experiment set (lesson 11).
<!-- Measured in internal/buildcfg and internal/goexperiment (gx-O/T readings). -->

### 3.8 Package delta — H3 (a first reading, not the census)

- **Added on all three OSes**: `testing/synctest`, `crypto/internal/fips140cache`,
  `internal/runtime/{cgroup,gc,startlinetest,strconv,wasitest}`, `internal/testhash`,
  `internal/trace/internal/{testgen,tracev1}`, `internal/trace/tracev2`, `math/big/internal/asmgen`,
  `net/http/internal/httpcommon`. Linux also adds `internal/cgrouptest`; Darwin also adds
  `internal/routebsd`.
- **Removed**: `go/ast/internal/tests`, `internal/trace/event`, `internal/trace/event/go122`,
  `internal/trace/internal/oldtrace`, `internal/trace/internal/testgen/go122`,
  `log/slog/internal/slogtest`, `runtime/internal/{startlinetest,wasitest}`. Darwin also loses
  `vendor/golang.org/x/net/route`, one of hop B's RN-5 platform-exclusive seed survivors. The
  NuGet IDs these carried are G1.
- **Totals** (raw `go list std`): windows 346 → 351, linux 344 → 350, darwin 345 → 350.
- **Two roster rows lose their package.** Both are relocations, the same shape as hop B's ten, and
  H10's `Receives` carries each predecessor's count as provenance:
  - `go/ast/internal/tests` (3 verdicts) → `go/ast` (`import_test.go`:
    `TestSortImportsUpdatesFileImportsField` and two siblings);
  - `internal/trace/internal/oldtrace` (3) → `internal/trace/internal/tracev1` (`parser_test.go`,
    3 tests).
- **Production `//go:embed`**: unchanged; only `internal/trace/traceviewer`. The new
  `encoding/json/internal/jsontest` embed is `goexperiment.jsonv2`-gated. The hop B close lesson on
  `//go:embed` payloads and `-text` pins still applies to the one that remains.

### 3.9 Linkname delta — H4 and H6 (read the non-Windows legs early)

38 added, 8 removed (1,301 → 1,331, over `//go:linkname` in non-test, non-cmd files).

- **Non-Windows first** (the vgetrandom pattern: these surface on the linux leg before anything reds
  on Windows): `internal/syscall/unix.Utimensat → syscall.utimensat` (`unix && !wasip1`).
- **Windows only**: `os.net_newWindowsFile → net.newWindowsFile`.
- **All platforms**: `internal/runtime/cgroup.throw` (the `runtime/panic.go` push and
  `internal/runtime/cgroup/runtime.go` carry no build constraint, and the package is in the
  windows std list); the four `internal/synctest` pulls and `time.runtimeIsBubbled`;
  `testing/synctest.testingSynctestTest`; the four `runtime/trace` flight-recorder pulls;
  `unique.runtime_rand`; `runtime_blockUntilEmptyCleanupQueue` for `unique` and `sync_test`;
  `internal/runtime/maps.typeString`.
- **Out of scope**: the libc `*at` procs in `internal/syscall/unix/at_libc.go` are
  `aix || solaris` only, outside all three corpus targets. The riscv64 and plan9 additions are
  outside them too.
- **Removed, each expected H6 input and not a converter regression** (lesson 19):
  - the `unique.runtime_registerUniqueMapCleanup` pair (the `runtime/mgc.go` push and the
    `unique/handle.go` pull). The push registry holds it (`linknameOperations.go:369`), so
    `TestLinknamePushRegistryMatchesGoSource` reds at a 1.25 GOROOT, and it already read it as a
    false regression under GOROOT 1.25.1;
  - the `internal/runtime/maps.mapKeyError` pair (the `runtime/map_swiss.go` push and the
    `internal/runtime/maps/runtime_swiss.go` pull). At 1.25 it is an ordinary function in
    `internal/runtime/maps/map.go`. The q82 guard (`TestDeclaredNotImplementedCensus`) declares it
    (`declaredNotImplemented_test.go:385`) and uses it as a positive control (`:95`), so the guard
    reds by design. Replace the control with a member that is still stubbed, in the same commit;
  - `runtime → crypto/internal/boring/fipstls.runtime_arg0`, a push into a package already absent
    at 1.24.13, dropped;
  - darwin's `vendor/golang.org/x/net/route` `sysctl` pull, which goes with the package (Q12);
  - `math/big`'s `addVW`/`subVW`, a relocation from `arith_decl.go` into `arith.go` (still
    linknamed).
<!-- linkname-O/T/added/removed readings; go1.25.13 source (runtime/panic.go:1059-1062; internal/runtime/cgroup/runtime.go
     has no //go:build; at_libc.go "//go:build aix || solaris"; utimes.go "//go:build unix && !wasip1";
     internal/runtime/maps/map.go:824 mapKeyError; runtime/runtime_boring.go keeps only the boring.runtime_arg0 push).
     Guards at master: src/go2cs/linknamePushRegistry_test.go:54, src/go2cs/linknameOperations.go:369,
     src/go2cs/declaredNotImplemented_test.go:95 and :385. The BOARD records the registerUniqueMapCleanup false regression
     under GOROOT 1.25.1 (around line 25674 at master). Runbook H10 "Order and launch" and §7 lesson 30 cover the
     linux-early reading. -->

### 3.10 Hand-own census — H6 (budget for it)

`handown-census.ps1` run over a `git archive` of master's `src/core`, go1.24.13 → go1.25.13. Of 184
marked files: **58 touched-substantive**, 4 touched-trivial, 58 untouched, 64 with no upstream
counterpart.

Substantive files by upstream package: runtime 25 (including `runtime/pprof`'s `pprof_impl.cs`);
internal/syscall 5; syscall 4; sync 4; crypto/internal 3; os, net and internal/poll 2 each; one each
for testing, reflect, unique, iter, internal/synctest, internal/testenv, internal/godebug,
internal/chacha8rand, internal/abi, internal/runtime, and the darwin copy of `vendor/.../route`. None
is under net/http, so the 1.25.14 patch should not move the list. H6 re-measures at the real target
regardless (⟲).

**Against hop B's census** (146 marked, 46 touched-substantive, 6 trivial, 43 untouched, 51 with no
counterpart), hop C's review list is about 26% longer. H6 is hop C's largest converter-side review
item, and the lane plan (F2) budgets for it. H10 stays the hop's largest step (runbook H10).
<!-- handown-census-124-to-12513 reading, taken 2026-09-29; the extraction tree was deleted after the run.
     "Classification only -- the judgment on every substantive row stays human (H6)." Hop B: AUDIT-h6-handown-go124.md
     :149-153. Runbook H10 "The migration's largest step"; PLAN §2 H10; the brief's risk 7. -->

### 3.11 The go command and toolchain — H1 and H4

- **`go.mod` gains an `ignore` directive.** Re-check the converter's `go.mod` readers (a standing H4
  item): the lax parser drops unknown verbs, so a silent drop has to be confirmed as intended.
- **vet** gains the `waitgroup` and `hostport` analyzers (H1 step 7). Also new: `go version -m -json`,
  the `work` package pattern, and `go` no longer adds a toolchain line automatically.
- **Ports**: macOS 12 is the new minimum. 1.25 is the last release with the 32-bit windows/arm port,
  which 1.26 removes; it is not a corpus target. OQ-13's deprecation rule applies at every hop that
  removes a package, hop C included (G1).
- **H1 step 3 is gated by step 2** (§2 A12).

### 3.12 Carried from the 1.24.13 hop (lesson 5)

- **The anonymous struct as a generic type argument** (the converter seat `ce8d0bd654`, on master)
  spreads at 1.25: 2 sites at 1.24 (`reflect/all_test.go`) become 6 at 1.25.1. Four of the six are
  in `encoding/json/v2/errors_test.go`, which is OUT under OQ-2. **Disposition:** no spread into the
  default set is expected; re-census at 1.25.14 (C1); revisit when `encoding/json/v2` graduates.
<!-- Mailbox archive docs/phase4/archive/MAILBOX-through-2026-09-20.md:89265 and :89380-89386 (the 1.25.1 census:
     reflect/all_test.go:3549, :6923 and encoding/json/v2/errors_test.go:50, :53, :86, :89; "Worth a line in the hop-B
     notes when there is one"); runbook §7 lesson 5. ce8d0bd654 is an ancestor of a1f133c3a9. -->

---

## 4. Timing constraints

> ⚠ **Re-derived 2026-09-29 from the owner's direction (ledger 08:27).** The freeze no longer waits
> for a 2026-10-24 planning date; it follows 225/225.

- **The start gate**: 225/225 (about 2026-10-01 to 10-02) → the final 1.24.13.N release (owner
  PIN) → H1/H2. Which TRAIN K seats must land first is Q13. Nothing on the hop's critical path starts
  before the release. Everything in §2 except D2 is preparation, and the preparation window is now
  **days**.
- **The window**: release in early October. At hop B's pace, about 16.4 days tag to tag, hop C runs
  through about the third week of October, well clear of 2026-11-10. `PLAN-corpus-upgrade.md` §6
  sizes it at 3–5 gate cycles and 2 full sweeps. The brief's window (about Oct 26 – Nov 8) is stale
  (Q10).
- **The reference point**: hop B ran from `nuget-1.23.12.3` (2026-09-07 20:19) to `nuget-1.24.13.1`
  (2026-09-24 05:49) with six lanes. Hop C has seven lanes plus COORD (F2), with credits as the hard
  ceiling. Re-estimate from that roster.
- **The dominant risk is compressed preparation.** The release can be ready before TRAIN K, §2 A,
  B0 and C7 are. Mitigations: rule Q13 now; keep the instrument seats off the release's critical
  path (A6); ask for B0 now, because C1, C3 and C7 all wait on it.
- **.NET 11 GA is 2026-11-10.** S1 and S2 are dated before GA. The .NET 11 measurement stage runs
  **after** hop C and never overlaps it. This is the one-variable rule, and `PLAN-hop-campaign.md`
  §5 calls it "the one ordering with no defense". The brief's risk 1, a late freeze pushing hop C
  into GA, largely recedes under the 08:27 timeline. It returns only if 225/225 or hop C slips by
  about three weeks.
- **Separate from 1.26.** OQ-18's fold rule failed because hop B owed class (c) hand-own rewrites, so
  1.25 and 1.26 are separate hops. Hop D's recon may overlap the .NET stage, but hop D's H1 waits for
  that stage to close.
- **Hop E (1.27) by about 2027-02** keeps the corpus on a supported Go (the OQ-19 cadence, a Q4
  item).
<!-- Ledger 2026-09-29 08:27 · OWNER · e4a93b14c0. Hop B milestones (ledger): nuget-1.23.12.3 2026-09-07 20:19; H1.2
     d5398554d8 09-07; H5 checkpoint 92333bbd42 09-13; H7a fc275f1ac3 09-16; H9 closed 09-20; H10 close fa18863b94 09-23
     22:23; nuget-1.24.13.1 09-24 05:49 (16 d 9.5 h; the brief rounds H1-to-publish to "about 17 days").
     OQ-18: AUDIT-h6-handown-go124.md rows 46, 48, 130 (class (c) rewrites). Brief appendix A (the window, written
     before 08:27) and section (e) risk 1. -->

---

## 5. Open questions

| # | Question | Whose call |
|:--|:--|:--|
| Q1 | Do the execution-pin fix (A3) and the TSV banking (A4) join the generator seat (A2), or ride as their own seat right behind it? Either way, both land before hop C's H10 recon leg. | COORD |
| Q2 | Amend `DotNetMigration.md` §4 step 4 in-stage to record the 2026-09-28 ruling (a current-major pin ahead of the TFM move), landing with TRAIN K (E2). Add to it how the .NET 11 stage's legs get past the pin (E4). | COORD |
| Q3 | Hop C's lane plan and duration for the seven-lane roster (F2), in usage weeks, and which host runs crypto/tls's BoGo wall (B4). Is cloud quota needed, and if so, has it been requested? | COORD; the owner for quota and hosts |
| Q4 | Is the new `attr` action in `go test -json` inert through all three verdict switches? A one-row check with a test that calls `t.Attr` at go1.25.14 settles it. | a lane (the §2 C1 recon) |
| Q5 | `unicode` is a strict-compare row with no manifest. Does its new table content convert byte for byte (C3 tier 2)? | a lane; COORD rules the ranking |
| Q6 | Which of the new internal packages are packable (H3)? `testing/synctest` takes `go.testing.synctest` under the existing publisher, and the reservation is filed when H3 names the new IDs (C5). | COORD drafts; the owner files |
| Q7 | How is the 58-file H6 list split? Runtime alone is 25, and the synctest and `sync` files interlock (§3.3). | COORD |
| Q8 | Does golib emulate the 1.25 runtime behaviours the rows can observe: container-aware and periodically re-read `GOMAXPROCS`, `[recovered, repanicked]`, concurrent `AddCleanup`? Each is H6 or H9 input. The C1 recon names the rows that observe each one, starting with the runtime family (C3's reserved set). | a lane (the §2 C1 recon); COORD rules |
| Q9 | H1 step 3 adopts **the highest x/tools and x/mod whose `go` directive is at most 1.25.x**, contemporary with 1.25.14, in its own commit and CNR (OQ-5). Step 3 follows step 2. | a lane at H1; COORD rules |
| Q10 | Records to bring current before H1. The **brief's window** (about Oct 26 – Nov 8) is the stale record against ledger 08:27 and the owner-approved Roadmap ("October 2026: a final Go 1.24 package release, then the Go 1.25 hop"), and it owes a dated amendment. `PLAN-corpus-upgrade.md` §1.1 reads 1.25.12 (→ 1.25.14), and the PLAN does not yet record the 2026-09-28 hop ruling. | COORD |
| Q11 | Where do hop C's lessons land? COORD proposes a dated delta block on runbook §7, including the 1.24.13 seed item with the thinnest home: sub-agent batteries run from briefs committed on coord-handover (F3). | COORD |
| Q12 | Darwin loses `vendor/golang.org/x/net/route` (darwin gains `internal/routebsd`), and the darwin hand-owned route copy is on the substantive list. With a Darwin run layer now going in parallel (ledger 08:27), the hand-own feeds a live run layer, not only a compile-only flavour. Is this an H6 retirement, with any needed content following `internal/routebsd`, or an H8 item? | COORD |
| Q13 | **Which TRAIN K seats must precede the final 1.24.13.N release?** COORD proposes S1 and S2, by the one-variable rule: the pin must not land mid-hop. The `-Hop` landing, the generator, A3 and A4 can land on master before hop C's H10 recon leg without gating the release. The TRAIN K draft also lists the runtime row bank among its pending seats, which would put the freeze itself in TRAIN K. | COORD; the owner informed |
| Q14 | **One naming convention for per-release instrument inputs** before A8 is cut. Today there are four: OQ-15's `docs/phase4/AUDIT-handowns-go1.NN.md`, hop B's `AUDIT-h6-handown-go124.md`, the landing seat's `go<major><minor>` (`CENSUS-h10-eligibility-go125.md`), and the population file's full version (`population-go1.24.13.txt`). The ruling covers the `-Hop` skeleton, the H6 audit and the population file. | COORD |
| Q15 | When origin is unreadable, H11's next-release check compares local tags only and warns (A11). Does Readiness accept that degrade, or must the preflight refuse? | COORD; the owner informed |

---

## 6. Runbook in-stage edits owed (applied when their trigger lands, each with a one-line dated note)

- **When `-Hop` lands**: two sentences stop being true and need rewording to say which legs `-Hop`
  may run and which it may not (the H10 recon leg stays the recon wrapper's unless re-ruled):
  - H10 step 1: "never the sweep wrapper … A wrapper `-Hop` mode is open instrument debt" (around
    lines 2287–2292);
  - the H10 launch amendment: the population gap "is the whole reason the sweep cannot run this leg"
    (around line 2377).
  - The Readiness "Instruments" bullet already names `-Hop` and does not change.
- **When A7–A9 and A13 land**: wherever the runbook names a 1.24-specific file or pin as an
  instrument default, it names the derivation rule instead. That includes H10's 2026-09-22
  amendment, which names `population-go1.24.13.txt` as "the enumeration" (lines 2443–2449).
- **With TRAIN K (Q2)**: `DotNetMigration.md` §4 step 4 records the 2026-09-28 ruling and how the
  .NET 11 stage's legs get past the pin (E2, E4).
- **COORD's §7.7 addendum** (the 1.24.13 step text some lessons never got) lands as its own docs
  seat. It is a dated sub-section inside §7, not a second lessons section.
- **At hop C's close**: a dated "Lessons from the 1.25.14 hop" **delta** under §7, not a parallel
  section, if Q11 rules so.

---

## Sources

- **Procedure**: `docs/GoCorpusMigration.md` §§1–7, including the Readiness gate, the fourth
  ordering, the H10 launch and close amendments and §7; `docs/DotNetMigration.md` §1 and §4.
- **Strategy and rulings**: `docs/PLAN-corpus-upgrade.md` §1.1–§1.3, §5 (R11–R13), §6, §8 (OQ-1, 2, 3,
  5, 13, 15, 18, 19); `docs/PLAN-hop-campaign.md` §5; `docs/Roadmap.md`; `docs/Glossary.md` (*hop*).
- **Brief and handover**: `docs/phase4/briefs/roadmap-post100-2026-09-28.md`,
  `docs/phase4/RESUME-SESSIONS.md` (section 1e) and `.claude/coord-scripts/trainK/tK-seats-draft.txt`,
  all on `claude/coord-handover`.
- **Ledger**: 2026-09-23 14:52 (owner order for the lessons); 2026-09-24 04:59 (the origin-tag
  reading); 2026-09-26 03:23 (§7 accepted); 2026-09-28 03:38 and 03:42 (roadmap rulings, side
  seats); 2026-09-29 07:49 (the CLR-error fleet rule), 08:12 (model delegation), 08:27 (the owner's
  direction: freeze, darwin), 09:21 (S1), 12:15 (S2), 12:39 (the `-Hop` read), 12:51 (the `-Hop`
  landing seat, B0) and 12:53 (the runtime row's wall).
- **Records**: `docs/phase4/RECON-go1.24-hop.md` (the shape for C1); `docs/phase4/DATA-sweep-row-walltimes.md`;
  `docs/phase4/AUDIT-h6-handown-go124.md`; `docs/phase4/CENSUS-h10-eligibility-go124.md`;
  `docs/phase4/archive/MAILBOX-through-2026-09-20.md` on `claude/mailbox` (§3.12).
- **External**: <https://go.dev/doc/go1.25>; <https://go.dev/doc/devel/release>.

---

## Amendments

*(Append-only. Each block is dated and states what changed and why. Earlier text stays as written.)*
