# HANDOVER — go2cs fleet coordinator

> **Purpose.** A pushed, append-only record of the fleet's state so that a coordinator session can be
> resumed after a credit reset (or on another machine) from GitHub alone. Branch `claude/coord-handover`,
> this file, dated blocks appended at the END; never rewritten. Nicknames only (`i7`/`coordinator`,
> `R-LAPTOP`, `G-LAPTOP`, `i9`, lanes `R`, `G`, `C1`, `C2`) — no hostnames, account names, profile
> paths or share names on this surface, ever. The coordinator's LOCAL memory directory on the i7 holds
> more detail (it is auto-loaded by a session opened on the i7 in the go2cs repository); this file is
> the portable subset.

## 0. How the OWNER resumes a coordinator session

1. Open Claude Code (the desktop app's Code tab) in the go2cs repository on the i7 — the main checkout
   at `C:\Projects\go2cs`. Any worktree is fine; the session's own worktree is disposable. Model class
   for the coordinator: Fable 5.1 with ultracode on (owner directive 2026-09-07); execution work is
   delegated to Opus sub-agents.
2. Paste this as the first message (edit the two SHAs to the newest ones named in the last dated block
   of this file):

   > You are the go2cs fleet COORDINATOR resuming after a credit reset. First: `git fetch origin
   > claude/coord-handover` and read `docs/phase4/HANDOVER-coordinator.md` from that branch, every
   > dated block, newest last. Second: read your local memory index (auto-loaded) — in particular
   > `coordinator-handoff-state.md`, `train47-board.md`, `train14-seat-ledger.md`,
   > `owner-asks-pending.md`, `doctrine-batch3-accumulator.md`. Third: read the mailbox delta —
   > `git -C C:\Projects\go2cs-mailbox fetch origin claude/mailbox` and read EVERY entry after the
   > anchor named in the handover's last block (never a small-N skim). Then, before touching any
   > worktree: census live processes by EXECUTABLE PATH (converter, `dotnet`, `BehavioralRunner`,
   > `powershell`) so you never launch into a running battery; read the assembly lock file and the
   > run stdout named in the handover; re-arm the mailbox monitor (template in the repo-local
   > coordinator scripts directory); then post one COORD status to the mailbox naming the handover
   > SHA you resumed from. Standing rules: announce-then-push (a fix to a posted SHA is a commit ON
   > TOP, never a rewrite); a seated branch takes no commits; nicknames only on every pushed surface;
   > no chips — lanes send suggestions to you; Fable reserved for rulings, merges and gates; the
   > mid-battery source freeze binds the assembly worktree; the two-pin pairing for every corpus
   > battery (GOROOT = the go1.23.12 sdk spelled as `go env GOROOT` prints it, its bin first on PATH,
   > GOTOOLCHAIN unset; converter builds under the go1.24.13 sdk with GOTOOLCHAIN=local). Keep this
   > handover file current: append a dated block and push after every landing or ruling that changes
   > fleet state.

3. Owner's standing goal (quote it in every check-in): «keep G, R, C1, C2, i9 and your own local Opus
   class sub-agents busy at all times — this is your fleet — ensure all lanes are pressing forward
   against primary project objectives. Note that i9 is Sonnet class and can only handle one serial item
   at a time due to CPU thermal issues — however, i9 is fastest in fleet and will be best for targeted
   long build operations that can complete faster on his hardware, for example, use i9 for long AOT
   builds for G to run when executing performance suite. Current objective: get to 100% implementable
   test validations for Go v1.23.12. Stretch objective: corpus migration to last build of Go v1.24.
   Project philosophies: honesty: first and always, no shortcuts, do the hard thing first, build a tool
   you can trust.»

   Course correction ruled 2026-09-07: the go1.24.13 hop IS the objective; the 1.23.12 record froze at
   204/209 and shipped as NuGet 1.23.12.3; five rows re-bank at 1.24 (H10, denominator 227). Converter
   pins go1.24.13 since train 43; the corpus stays 1.23.12 until H5 (seeded full reconvert at the target
   GOROOT — the repository goes public on 1.24 at H5, the NuGet feed at H11).

## 1. Where things live on the i7 (paths without identifiers)

| Thing | Location |
|---|---|
| Main checkout | `C:\Projects\go2cs` (master; never assemble here) |
| Assembly worktree (train 46 battery) | `C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c`, branch `coord-train46-head` |
| Coordinator session worktree | `C:\Projects\go2cs\.claude\worktrees\go2cs-fleet-coordinator-bec1b9` (disposable) |
| Handover worktree (this file) | `C:\Projects\go2cs\.claude\worktrees\coord-handover`, branch `claude/coord-handover` |
| Sub-agent worktrees | `…\worktrees\sub-doctrine-batch19` (branch `claude/coord-doctrine-batch19`), `…\worktrees\sub-orphan-check` (branch `claude/coord-orphan-disclosure-check`) |
| Mailbox READ clone | `C:\Projects\go2cs-mailbox` (refspec carries `claude/mailbox` ONLY — never read repo content from it) |
| Mailbox POST tool's clone | `C:\Projects\go2cs-mailbox-coord` (written only by the post tool) |
| Repo-local coordinator scripts (untracked, local) | `C:\Projects\go2cs\.claude\coord-scripts\` — `coord-mailbox-post.ps1` (the post tool: identifier census exit-gated, branch guard, read-anchor), `coord-mbmon-template.sh` (the mailbox monitor; substitute the anchor SHA, assert the substitution, run in the background), `train46\` (the train-46 assembly, rehearsal, self-check, land and dry-read scripts, the derive, the union-fix patch, the run-7 copy and a stdout snapshot), `train47\` (the pre-derived train-47 template, when it lands) |
| Assembly lock | `/tmp/t46-assemble.lock` in the Git Bash namespace (`pid=<msys pid> started=… script=…`); kill by that pid mapped through `ps -l` to its Windows pid, `taskkill /T /F`; stale after a hard kill |
| Toolchains | .NET 10 under the user's `dotnet10` directory (`DOTNET_ROOT` must point at it in every build shell); go1.23.12 and go1.24.13 sdks side by side under the user's `sdk` directory; ambient defaults on this box are WRONG (.NET 9.0.317, go1.23.1) |
| Post tool usage | from the scripts dir: `MSYS_NO_PATHCONV=1 powershell -NoProfile -ExecutionPolicy Bypass -File ./coord-mailbox-post.ps1 -EntryFile <windows path to the entry .md> -Message '<subject>' -LastRead <sha>`; read its output UNFILTERED (the absorbed-range lines are the read discipline); a rejected push means fetch, read, re-post |

## 2. Fleet roster and model classes

- **COORD** — i7-5820K, Fable 5.1, signs at merge; the i7 fails Go's OWN `net` suite (oracle, not conversion) so it is not a `net` bank host.
- **R** — R-LAPTOP, Fable 5.1. Owns the H5 ladder (the 1.24 re-base), `reflect`. Its git signer is HUNG (rc 124 under a 60 s wall) and R's operating instructions forbid an unsigned CODE commit unless the OWNER asks, so R's cuts route through the fleet share + SHA-256 to a coordinator sub-agent until the owner acts (see §5).
- **G** — G-LAPTOP, Opus. Converter surface; WSL side is the fleet's Linux .NET 10 host (SDK 10.0.401, GolibTests linux 784/0/1 = 785 admissible); Windows side permitted by the owner 2026-09-08 as a second conforming-DNS `net` host, qualification pending.
- **C1** — cloud container, Fable at reset; no .NET on its host. Owns the `runtime` row and its hand-owns.
- **C2** — cloud container, Fable; disk-constrained (cannot run solution/behavioral legs — COORD takes them). Owns the Q44 census instrument, the token door, the sync/disclosure manifests.
- **i9** — Sonnet, one serial item at a time; OFFLINE at the time of writing (owner works on it when home 2026-09-08). Queue when it returns: `reflect` census re-run against C2's `7951333dfe`/`6b398dd635`; the `runtime` results-file tail on C1's minted branch; hold the `TestRegisterClass` prediction.

## 3. Standing rulings a resumer must not re-derive

- Lane branch commits are UNSIGNED by doctrine (COORD ruling 2026-08-30); the coordinator's merge commit on master is the signature of record; tags signed; mailbox commits unsigned (owner ruling 2026-08-22, transport only). Do not relay the older "lanes signed" clause (it cost a false owner ask on 2026-09-08).
- One Go primitive, one behaviour: `internal/sync`'s `throw`/`fatal` pair takes `FatalReport.Fatal(s, userFault)` exactly as `sync`'s; lands at the ladder re-base after train 46.
- Frozen-metadata class (4 packages hand-owned by consequence): (B) un-freeze as a converter seat (G) — HELD until the linkname-PULL derivation fix lands as a preceding commit on the same branch (see 2026-09-08 block); (C) relocation on the H5 hand-own branch with orphaned directories removed explicitly (`internal/concurrent` → `internal/sync`, `internal/weak` → `weak`, the leftover `runtime/internal/sys` directory).
- Token door: STAYS (refuse by name when a reference-bearing pointee's token reaches a native argument); the cookie case becomes an (API, argument) contract table (today: `EnumTimeFormatsEx` arg 3); inbound recovery keyed on the SOURCE (`Reinterpret<uintptr,T>` from a tagged token → `Resolve`); C1 sizes the three as ONE `runtime` seat after train 46 lands, i9 the Windows run arm.
- `LockOSThread` fix = Go's whole body (counter AND `dolockOSThread` linkage), C1's seat after train 46 lands; regression falsifier `TestCallbackPanic`.
- The validation configuration of record is Release with tiering off; cgo OFF on every platform; the roster's classes are the owner's (no coordinator-minted exclusion class).
- Deferred-alloc disclosure class (owner-delegated 2026-09-05) stands; orphan-disclosure check ruled on a terminal PASS.

## 4. Train state (2026-09-08, 17:40)

- **Master `44f858717`** (train 45 landed 14:24).
- **Train 46, run 7** in the assembly worktree, base 44f858717, six seats + one UNION FIX assembly commit: 1 `claude/g-alias-namespace-shadow` 05b50de63; 2 `claude/g-slices-typeparam-nil` 9893b70e1; 3 `claude/c1-fatal-path-guard` 8adf8875a; 4 `claude/c2-q44-registry-census` e7201a405; 5 `claude/g-defer-reflowered-box` 18cb44b19; 6 `claude/c1-q53-sizing` 238dfefea. Light gates green; LEG 1 integrity 0 cycles ×3; LEG 2 stdlib slnx 605 s 0/0; LEG 2b go2cs.slnx 954 s (loaded) 0/0; LEG D (three-target two-seeded diff) in progress at 17:40 — base arms and cut/windows done at prediction, cut/linux and cut/darwin remain, verdict near 17:55; then LEG R, LEG 3 (GolibTests both configurations), LEG 4 (CNR at the pairing, predicted CHANGED 0), LEG 5 (full suite, converter rebuilt first), LEG K (canaries + sync + nistec); then `coord-train46-land.sh` (verify-only first, then push master, prune the six seats, landing post with the landed tree hash). The run's stdout is `coord-train46-assemble-run7.stdout` in the session scratchpad; a snapshot sits in the repo-local scripts dir. If a leg reds: stop, attribute per doctrine, never land.
- Runs 1–6 each refused on an INSTRUMENT defect that is fixed in the derive (a literal seat count; A7's landed-guard premise; G4's doctrine premise and a `${VAR:-0}` empty-case bug; a union-only `go vet` failure fixed by the UNION FIX commit; G6's fixture residual → `CENSUS_TEMPLATE`; G11(a)'s syscall premise). One cosmetic blemish remains in run 7: LEG D arm-2's stamp is cut off inside a `$(…)` substitution (a stray `command not found`; numbers intact).
- **Train 47 board** (base = the landed train-46 master): `claude/coord-doctrine-batch19` at **e9e56b657** (batches 19+20+21, 1,355/0 on CLAUDE.md, ONE doctrine seat; batch 22 starts at accumulator item 1322); `claude/coord-orphan-disclosure-check` at **36cbef240** (two increments; OWES a utf8 `-tests` pipeline arm after the battery); `claude/c2-sync-disclosure-retire` 4221789e7f (manifest seat); C1's counter-accounting `runtime` seat (not cut until train 46 lands — same file as its seat 3); C1's erratum block; G's (B) pair (HELD, see §3); R's three cuts via the share (stamp guard `valueCloneStampMembers_test.go` 255/0 + projitems 1/0; `WaitReason.cs` golib seat with a release-aware doc; ARM C behind its `note.key` control). Train-47 seat classes expected: doctrine, converter-test, manifest, docs, converter, golib, golib-corpus-handown. A pre-derived train-47 template (seat table left PENDING) is being produced by a sub-agent into the session scratchpad; copy it to the repo-local scripts dir `train47\` when it lands.

## 5. Owner items open

- **i9 offline** — owner works on it when home (2026-09-08).
- **R-LAPTOP signing** — either answer its GnuPG pinentry in person (never on an RD session), or tell R directly that unsigned lane code commits are authorized; until then R's cuts route through the share.
- **`net` host** — G-LAPTOP Windows side permitted; qualification result pending (Go's own `net` suite at the 1.23.12 pin, ledger criterion). If it needs the WSL-style resolver change, the commands come back to the owner.

## 6. Doctrine accumulator items not yet in CLAUDE.md (1322–1337, 2026-09-08)

1322 an arm that agrees with the arm it was built to differ from is the tell that the instrumentation never compiled in (C2). 1323 a staleness gate and its checker are two instruments; the checker positive-controls itself (`strings -el` on a UTF-16 literal) (C2). 1324 "no file" from a work-triggered census means four things; the discriminator is a START block at module init, not a block count (C2). 1325 a boundary number can be the right position and width and the wrong KIND — a token where Go hands a funcval pointer (C2). 1326 the registry can recover the box and the emitted inbound edge never asks; the remedy key must be measured before sizing (C2). 1327 a mis-labelled control arm can still discriminate twice; score its expectation MISSED and keep what it discriminated (C2). 1328 a ruling that lives only in the coordinator's private memory is invisible to the fleet — rulings land on a pushed surface the day they are made (R). 1329 lane commits UNSIGNED by doctrine (COORD 2026-08-30), coordinator signs at merge; mailbox unsigned (owner 2026-08-22); never `--pinentry-mode error`. 1330 a ruling recalled from memory is superseded by the ruling on the record, one command away (COORD against itself). 1331 compare DATES before conceding — "outranks" is not "supersedes" (G). 1332 a converter-minted csproj's reference set and forced-init hooks are import-derived and blind to a linkname PULL (godebug dropped its `runtime` reference); correct by derivation (imports ∪ pull producers); the CLAUDE.md "8 hooks" sentence is stale — it is 11 (bcache 1, concurrent 4, godebug 4, weak 2) (G). 1333 Git Bash `grep -icF <pattern>` aborts rc 134 on a plain pattern too; read the rows before retracting on a count (R). 1334 a funnel census's moving unit is "what counts as a funnel" (16 → 43 → 23); extract balanced argument lists, keep UNKNOWN rather than absent (C2). 1335 a path re-read from the environment at call time is not the path written at module init; record the path written (C2). 1336 the token door does not retire: the pass-through case is the minority in its own row; a `uintptr`-source key touches one site (C2). 1337 two tests failing on different halves of one omission take Go's whole body; a counter-only fix trips Go's own consistency check (C1).

---

## 2026-09-08 18:00 — train 46 run 7 STOPPED at LEG D on an instrument false red; run 8 launched DETACHED

- **Run 7 (head 3a4f83aa7)**: every leg through LEG 2b green; LEG D's MEASUREMENT was exactly the prediction on every file a target can write — `runtime/panic.cs` +2/−53 (the `throw`/`fatal` displacement) and that target's own `runtime/<goos>/package_info.cs` +1/−1, MET by mechanism on windows, linux and darwin, nothing outside the prediction — but the verdict read MISSED ×3 because the expected set was the UNION across targets, so each target counted the other two flavours' `package_info.cs` as "predicted but did not differ" (layout L3: a single-target conversion writes only its own flavour's folder). The chain stopped by design; LEG R, 3, 4, 5, K UNMEASURED. The land script accepts only a clean chain (no acceptance path by design), so the fix went into the instrument and the battery restarted.
- **The fix**: LEG D's set comparison now filters the prediction PER TARGET (a file under `/windows/`, `/linux/` or `/darwin/` is expected only on its own target; a flat file on all three), stamping the filtered and union counts side by side. Controls before relaunch: run 7's own artifacts re-read through the filter give MET ×3 with extra 0 / missing 0; the unfiltered union reproduces run 7's missing 2; the self-check reads 37 PASS. The train-47 template needs the same fix (it was pre-derived from the pre-fix assembly).
- **Run 8**: launched 17:57:52 DETACHED (`Start-Process -WindowStyle Hidden` from bash, a launcher that pins the environment and appends `assembly exit=N` as its last line), lock `/tmp/t46-assemble.lock` pid 48072, stdout `coord-train46-assemble-run8.stdout` in the session scratchpad with copies of the scripts under the repo-local `.claude/coord-scripts/train46/`. Expected wall: light gates ~10 min, LEG 2 ~10 min, LEG 2b ~16 min, LEG D ~52 min, LEG R ~5 min, LEG 3 ~10 min, LEG 4 ~20 min, LEG 5 ~35–110 min, LEG K ~10 min: `ASSEMBLE DONE` between ~20:40 and ~22:00. **A resumer**: read the run-8 stdout's LAST lines first; if `ASSEMBLE DONE … overallFailed=0`, run `coord-train46-land.sh` with `LAND_ASM` pointing at the run-8 stdout, `LAND_VERIFY_ONLY=1` first, then the landing (push master, prune the six seats, landing post with the landed tree hash); if a leg reads red, stop and attribute — never land. If the process is gone with no DONE stamp, it was killed: census by executable path, read the lock, and relaunch from the derive (never into a worktree with a live converter).
- **Since the first block**: doctrine seat pushed at e9e56b657; C2's tips 06a556ec74 verified; G's (B) HELD for the linkname-pull derivation fix; token door STAYS with the contract-table + source-keyed recovery ruled; R blocked on signing (who-may-authorise; two owner options); the train-47 template pre-derived into `.claude/coord-scripts/train47/` (seat table PENDING; needs the LEG D per-target filter carried over). Mailbox anchor at the time of this block: 5bce50ef.
