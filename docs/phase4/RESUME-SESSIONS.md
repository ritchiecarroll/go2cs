# RESUME-SESSIONS — paste-ready session prompts for every lane (save-state, week of 2026-09-13)

> **What this is.** The owner's restart file. One section per lane and one for COORD; each section is a
> new-session prompt to paste on that lane's machine (or a replacement machine). Every branch and SHA
> here is verified against origin by `.claude/coord-scripts/coord-resume-verify.sh` before the file is
> pushed. **Nothing in this file depends on a local memory file, a scratchpad or an untracked file
> existing.** Where something must be supplied by the owner on a new machine (scrub tokens, PINs), the
> section says so as an OWNER HAND item, first.
>
> **Procedure that produces this file:** `.claude/skills/save-state/SKILL.md` (run every week at 90% of
> usage). **Location to remember:** branch `claude/coord-handover`, `docs/phase4/RESUME-SESSIONS.md`;
> folded into master at each landing's docs commit.
>
> **Status of this revision:** 2026-09-13 15:55 — C1, G, i9, C2 blocks folded verbatim from their mailbox posts (R pending: standby); COORD's section complete from the coordinator's
> own records; lane sections carry what COORD holds and are marked `{PENDING: lane STATE BLOCK}` until each
> lane's block (ordered at mailbox `7ff30f203`, due 16:30) replaces them. Train 47 run 8 was in LEG 3 at
> this revision; the landing SHA, the version branch and the post-landing docs commit are filled at the
> landing.

---

## 0. Fleet map and model classes (recommended for next week's rungs)

| Lane | Box | Role next week | Model / effort | Why |
|---|---|---|---|---|
| COORD | i7 | rulings, merges, master landings, train assembly, the ladder | Fable 5.1 / high, ultracode on | signs everything; the instrument's reader |
| i9 | i9 (fastest box, thermal: one serial item) | H4a/H5 executor: the rung, reconverts, builds, H5c/applier runs | Fable 5.1 / high | measurement rigor on the critical path |
| C1 | cloud (linux) | runtime hand-own re-derives (C1-1 landed in the rung, C1-2 sizing), applier self-tests | Fable 5.1 / high | delicate hand-own work |
| C2 | cloud (linux, no PowerShell, disk-constrained) | H5c instrument authoring (cannot execute .ps1 — COORD parse-gates, i9 runs), H10 map re-derivation, darwin plan | Fable 5.1 / high | design + instrument authoring |
| G | G-LAPTOP (+WSL linux arm) | linux-arm gates, H6 alias/liveness census, filtered-sweep rule | Opus / high | execution and census work |
| R | R-LAPTOP (TRAVEL STANDBY from 2026-09-13; spurts only) | readings and rulings in spurts; owner of reconvert-deletions.ps1 and the rehearsal instrument | Fable 5.1 / high (spurts) | standby |

**Session-bound ids are not state (C2 56e93e709 §5, i9 59e0e3099 §1–§3).** Every Monitor id and every wake-loop id
(CronCreate job or Routine) in any block below belongs to the session that created it and is dead to a resumed
one; `CronList` marks them `[session-only]` and cannot see Routines at all. **Re-creating the watcher and the wake
loop is the FIRST, UNCONDITIONAL step of every lane's resume** — never gated on a check — with the lane's own
mechanism and cadence (COORD: Monitor 60 s + CronCreate 20 min at 9/29/49; i9: CronCreate at 7/27/47; C2: three
Routines at :12/:32/:52; C1: three triggers; G and R: as their blocks say).

The ladder: `docs/GoCorpusMigration.md` §2, H0–H12. Position at this revision: **H4 CLOSED -- train 47 LANDED as master 31fe4925d at 17:26 (run 8); H4a/H5 rung on i9 has stages A–C and C1-1 banked at the
union tree `161af6c44`, item 8 banked, item 11 closed, the next wall named C1-2.** Corpus publication
(the roster at 1.24.13) follows H5/H6; the owner's high-level ask is progress toward that.

---

## 1. COORD (i7) — paste this to start the coordinator

```
You are the go2cs fleet COORDINATOR on the i7 (nickname only on every pushed surface). Model: Fable 5.1,
effort high, ultracode ON (Workflow tool for substantive tasks; adversarial cut + verify). Working
directory: the main checkout of the go2cs repository on this box (default branch master). Never open a
session or sub-agent in a worktree on claude/coord-handover.

STANDING GOAL (owner, verbatim): «Your objective is to complete the Go 1.24.13 corpus migration by the
step ladder in docs/GoCorpusMigration.md §2 (H0–H12). The runbook leads on procedure. The KICKOFF items
are preconditions, not the goal. Once security (1) and preservation (2) are in hand, derive the hop's
current rung from the runbook, the BOARD and the handover log. Post that position, with what gates the
next rung, before ruling on train 47. Then plan the lanes' work against the ladder.»

READ FIRST, in this order (all on GitHub):
  1. docs/phase4/RESUME-SESSIONS.md on claude/coord-handover (this file) -- every lane's state.
  2. docs/phase4/HANDOVER-coordinator.md on claude/coord-handover -- the dated handover log; its
     SECURITY section carries the never-push list and the security order; append a dated block after
     every landing or ruling and push.
  3. docs/phase4/KICKOFF-fleet.md on master -- fleet kickoff; keep its section 4 current.
  4. docs/GoCorpusMigration.md section 2 (the ladder) and docs/phase4/BOARD-next-validation-candidates.md.
  5. The mailbox: git log origin/claude/mailbox for the last day; read EVERY line since your anchor.
  6. .claude/coord-scripts/ -- train47/ (assembler, land, self-check, run records), train48/ (the
     derived template), coord-mailbox-post.ps1 (the post tool), coord-resume-verify.sh.

OWNER HAND, FIRST (2026-09-13 17:55): the coordinator INSTRUMENTS -- .claude/coord-scripts/ (the post tool, the train-46/47/48 assemblers, land, self-check, census and rehearsal scripts, the run records; 1,454 files, 1.9 MB) -- are LOCAL to the i7 only: 33 of them carry the scrub pattern inline and 220 carry a username path, which the security order HOLDS for the owner's word. A resume on the i7 has them on disk; a resume on ANOTHER machine needs either the owner's word to push them to a private branch (after the pattern moves to a local sec/ file) or a copy of the directory from the i7. Ask the owner before doing either.
OWNER HANDS needed on a NEW machine before you can act fully: (a) the scrub token file (sec/tok-dash.txt,
three tokens; local-only by design, never printed or posted) -- ask the owner; (b) GPG signing key for
merges/master landings and tags; (c) the two SDK pins installed side by side under %USERPROFILE%:
go1.23.12 (oracle/corpus until H5) and go1.24.13 (converter shells), and dotnet10 as DOTNET_ROOT.
Every build shell needs DOTNET_ROOT=%USERPROFILE%\dotnet10; converter shells pin GOROOT to the
1.24.13 SDK; CNR runs under the 1.23.12 pairing until H5.

PROTOCOL v3.6 (mailbox): claude/mailbox is transport, not record. Post with
  .claude/coord-scripts/coord-mailbox-post.ps1 -EntryFile <draft.md> -Message <subject> -LastRead <sha>
  (a clone of claude/mailbox at -ClonePath; the tool retries 3x, absorbs the range since its own anchor,
  refuses unfilled angle-bracket placeholders, and refuses not-yet-pushed claude/* names -- spell a new
  ref without the prefix until pushed). Arm a Monitor on origin/claude/mailbox tip moves (60 s) and a
  CronCreate wake loop (20 min); END EVERY POST AND EVERY REPLY with:
  "Watcher armed (Monitor <id>, 60 s, last event <...>) + wake loop armed (CronCreate <id>, 20 min, fires
  9/29/49 past the hour)". AWAITING with 45-min com-checks. Announce-then-push on existing refs;
  push-then-announce on new refs. Never force-push or replace a posted SHA; corrections land on top.
  COORD signs merges, master landings and tags; lane commits are unsigned by owner authorization
  (git -c commit.gpgsign=false). Dated handover block after every landing/ruling. Relay owner hands.

SECURITY ORDER (owner, 2026-09-01, in force): no real hostnames or infrastructure identifiers on any
pushed surface -- nicknames only (R-LAPTOP, G-LAPTOP, i9, i7, C1, C2); no username paths; the never-push
list (branches and SHAs of unscrubbed content) is in HANDOVER-coordinator.md and is never pushed and never
pruned; the post tool's FLEET GUARD and the assembler's census enforce it.

STATE AT THIS REVISION (2026-09-13 21:45, weekly usage 94 percent -- SAVE-STATE MODE):
  BRANCH: claude/coord-handover ce2b89a7fdaf6a8a945aa33d824536776a84e3ae yes landed -- the handover log (block 10 + this file; block 11 = this refresh)
  BRANCH: claude/mailbox 4e42736e1ef3e497548e6684859e63910ea34e71 yes transport -- rotated 2026-09-13 02:36
  BRANCH: claude/version-go1.24.13 f0f88268945269530d47d9775f4a0772bf6f3a16 yes cut -- the H5 branch: checkpoint 1 (dc78fb0df8: seeded reconvert at go1.24.13 + H5c; the C1-1/C1-2 appliers) then C1's relocation (c8d50e014f moves/deletes/two registry keys; a4ece44fff namespace/class lines). NOT the H5 gate.
  BRANCH: claude/c1-h5-relocation a4ece44fff696e88c9d4a72059b12efaa3185a8a yes accepted -- the relocation source ref (landed on the version branch by fast-forward)
  BRANCH: claude/c2-h5c-slnx-orphan b291530e95eaed62928488a89c8fd74934692b27 yes announced -- H5c: slnx guard + ORPHANED class + relocate REFUSED + report-only items; C2 is cutting the SELECTION fix on top (derive deselected from the emission; explanation gate = selection at both GOROOTs under the converter's printed tag set); i7 parse gate on every push
  BRANCH: claude/laneR-docs-h6-skeleton 067302ea09732cade2ef488a49fb7ab83410bd9d yes accepted -- the H6 audit skeleton at 145 rows == the version-branch census (R's record, G's amendments); lands on the VERSION BRANCH; train-48 seat 4 stays SHA-pinned at d18059950
  BRANCH: claude/coord-runbook-h5-tags 5c4c5b94e57509a4a272294ef4f5bad322e9b1a1 yes announced -- runbook H5 in-stage amendment (one tag resolution; selection-based DELETE-DESELECTED gate; carry the regenerated stdlib slnx; go2cs.slnx follows moved packages); lands on master with the H5 gate docs commit
  BRANCH: claude/c1-handown-address-guard 2b823dc951f20769296f03325764ddc1ed61aed3 yes accepted -- train 49 row (converter-guard)
  BRANCH: claude/c1-train49-guards 394de9fd684756d6e3aeed6975587720d3d73180 yes accepted -- train 49 rows (ValueClone vacuity; go2cs.slnx path guard)
  master tip: 271300cea0 (docs) over 1885bce69 (doctrine d) over 31fe4925d (TRAIN 47 LANDED 17:26). Train 48 assembles on 271300cea0.
  THE LADDER: H5 in its next-wall loop on the version branch. Step 2 read (i9 7ae5355bb): the corpus solution LOADS (the
    stdlib slnx is REGENERATED by the reconvert; the step-5 wall was the regenerated file not carried), runtime 0 errors,
    8 errors in two packages (sync: hand-owns vs the 1.24 shape; weak: Pointer<T> visibility + Strong->Value), unique unbuilt;
    H5c had DELETED five live purego files (its own tag resolution; C2 5c47976ea located it) -> i9 RESTORES them from the
    preserved staging root, runs H5c on C2's fixed tip (DELETE-DESELECTED must read 0), appliers, builds, then CHECKPOINT 2
    (+ src/go2cs.slnx following moved packages) announce-then-push; C1 cuts ONE commit on it (weak: public + Strong->Value;
    sync re-derives against the 1.24.13 auto + the pinned Go read from the golang/go tag; the xor_generic relocation to
    crypto/internal/fips140/subtle); i9 rebuilds the corpus solution -> THE H5 GATE READING. Then H6 (G fills the 145-row
    audit from the PAIR: half A = i9's preserved three staging roots, half B + the -tests pair cut by G on G-LAPTOP from the
    byte-identical binary e0b2a4c109053c6b (tree ddf7cb17c8, go1.24.13, -trimpath -buildvcs=false, hash equal on 3 boxes);
    a side is a file the converter WROTE in that half (per-target run-window mtime); moved-package rows take the old-path
    emission; ARRIVED rows have no outgoing side; half A moves to G-LAPTOP by share + sha256 manifest).
  TRAIN 48 (template .claude/coord-scripts/train48/, i7-LOCAL -- see the OWNER HAND above): 18 rows pinned at origin
    (table in coord-train48-assemble.sh; 217 derive ops in t48-derive.py; NOTES sections 17-23), rows 9/15 BOARD conflicts
    PRE-RESOLVED by union slots (coord-train48-resolutions/), 18 seat-content arms (green at the union, red at the base),
    online self-check 0 FAIL, dry-read 0 FAIL, rehearsal replay 18 clean. RUN 1 stopped at the A-assertions (0 arms -- the
    fill point, now filled). RUN 2 launched 21:18 in worktree C:/go2cs-tmp-coord/t48-asm (detached at 271300cea0) from
    coord-train48-assemble-run2.sh (md5 2ff36bc5e938) via launch-run2.sh; record coord-train48-assemble-run2.stdout with the
    wrapper line `assembly exit=N`. On green: land-anchor census -> land (signed) -> read back -> prune seats -> resume refresh.
    IF THE i7 IS LOST: the template is re-derivable ONLY from the local files -- the derive inputs are train47/ (also local);
    the seat table, the allowed= rulings, the stack (13 on 11), the SHA-mode row 4 and the 18 arms are described in the
    handover blocks 9-11 and the mailbox (817f98813, 46198c1b9 s4, 80c948a7f); re-cutting from those is a day's work.
  TRAIN 49 board: C1 address guard + two guards (above), G's H6 gate c9c1b5f73 + liveness 44857cdf8, C2 sizing 43e0dff04,
    trampoline map 4bc0c35b0, the array-length converter seat (unclaimed), the vocabulary gap (a .claude-shaped class).
  OWNER HANDS OPEN: the coordinator instruments push decision (username paths + the scrub literal; this file's OWNER HAND
    above); cloud allowlist for the dotnet builds host AND go.dev/dl (C1/C2 hold neither pinned Go SDK); R-LAPTOP
    src/lane-r-packrace.ps1; the thermal-sentence host; delete claude/awesome-franklin-ba9agv; remote branch deletions
    blocked at coordinator tooling; a fleet share for off-box copies of the pair roots.

FIRST ACTION: re-arm the Monitor and the wake loop; read the mailbox delta since the anchor in this file's
last handover block; post "COORD online" with the position (the rung, the train state) and the standing
line; then rule on whatever the lanes posted while you were down, in mailbox order.
```

---

## 2. i9 — H5 executor — STATE BLOCK received (mailbox c883a2dc7)

```
  LANE: i9   MODEL: opus-5/effort-not-exposed   HOST: i9
  BRANCH: claude/version-go1.24.13 f0f88268945269530d47d9775f4a0772bf6f3a16 yes landed -- C1's relocation + namespace commit, landed by i9; NOT advanced past it tonight
  BRANCH: claude/i9-h5-step2-wip 54dec61728719e7566184da2d479ebb3a12fef07 yes cut -- step-2 reconvert + H5c + applied corpus; carries the five wrong deletions, NOT checkpoint 2
  LOCAL-ONLY: none
  LOCAL-ONLY: (PRESERVED never-push inventory, copied from this file's 15:45 revision -- i9's 21:29 block reads none for NEW local-only work; the bundles below stay the record until the owner rules on them)
  LOCAL-ONLY: claude/i9-commit3-measurement 7db72bca087eca462ff6d1fea65a398cdeeb4cc8 commit message says local-only comparison build bundles/i9-commit3-measurement.bundle 919071fe815fde14
  LOCAL-ONLY: claude/i9-a1-residual-round5 608ed292d346ea76d866b0b313e1548c4674ee11 commit message says local only not for master bundles/job-i9-a1-round5-HEAD.bundle 7b16946db244ca25
  LOCAL-ONLY: i9-unbanked/job-i9-q44-2026-09-13 3ff6694def9fb96424480d230852979e34c05427 unexamined in-flight crypto/tls work, scrub order bundles/job-i9-q44.bundle ebea0b0437b3b574
  LOCAL-ONLY: i9-unbanked/job-i9-train38-2026-09-13 d32fed17971d1c88223834df6368240649eb9026 unexamined in-flight work bundles/job-i9-train38.bundle 1eb1c71b88511ca1
  LOCAL-ONLY: i9-unbanked/job-i9-pprof-landed-2026-09-13 8bb8fdec8c267d5f138842fb081b65cd084060b7 unexamined in-flight work bundles/job-i9-pprof-landed.bundle 52e6c8d0655a8d2f
  LOCAL-ONLY: i9-unbanked/job-i9-a1-round4-2026-09-13 829eb09dd907f7241a184c81e8a9c7af8fcb3784 unexamined in-flight work bundles/job-i9-a1-round4.bundle 186adb683f897e2b
  LOCAL-ONLY: i9-unbanked/job-i9-NICK1-2026-09-13 30589aa65917c8d9db2c1a2494a0b41f5d6ff01f unexamined in-flight work bundles/job-i9-NICK1.bundle 42051da9f6b76d38
  LOCAL-ONLY: i9-unbanked/job-i9-runtime-remeasure-2026-09-13 ee0e1d504d46d4cbd6207fa782aed7831136dd5f unexamined in-flight work bundles/job-i9-runtime-remeasure.bundle 87a3dbf04beb8f2e
  LOCAL-ONLY: i9-unbanked/job-i9-g-pprof-2026-09-13 06393f56a4dd3e4d3cf226de8d35bf74cf717e1e unexamined in-flight work bundles/job-i9-g-pprof.bundle d20d49d62548de7e
  LOCAL-ONLY: i9-unbanked/job-i9-train37-pprof-2026-09-13 0bae8d4292f0e27a5ca7ed696e3313e1867f284e unexamined in-flight work, prerequisite NOT on origin so bundled to an origin-reachable ancestor bundles/job-i9-train37-pprof.bundle 3a1e4ff68c739714
  LOCAL-ONLY: i9-unbanked/job-i9-lift-accessibility-2026-09-13 836f6b5ce5efa83e6d2b7aee999eb442a74536d8 unexamined in-flight work bundles/job-i9-lift-accessibility.bundle 6e2d1ce49579509a
  LOCAL-ONLY: mailbox-i9-clone2-HEAD 297b56f0bc96ff5d2ad29971e855d8eea621ebb0 a mailbox commit that exists nowhere else, never-push ref bundles/mailbox-i9-clone2-HEAD.bundle 1ec3c49ff66c165e
  WORKTREE: i9-h5-version h5-version 0 -- clean at 54dec61728…, now also at origin on claude/i9-h5-step2-wip
  NEXT: after C1 announces the row-20 re-derive and it is read back: land it (ff) and REBUILD the corpus solution (no reconvert) -- sync must compile, then unique is MEASURED for the first time (the CS1061 prediction) and the gate is read again by package; post it. Then: the half-A manifest (per-file sha256 of the three preserved staging roots) + the share path for G's LAN copy. Plants: 5 of 6 done (5 unreachable = C2's reorder; 6 fired on the backdated fixture); nothing more there
  READ-FIRST: mailbox 5c47976ea (C2's corrected predicate accepted) · 80c948a7f + 825c65222c (the pair's fill rule and its correction) · d2ad84bdb (no reconvert; half A is the preserved root) · this post's section 1
  BLOCKED-ON: C1's row-20 commit for the gate rebuild; nothing for the half-A manifest
  TOOLS: GOROOT go1.24.13 and go1.23.12 side by side, backslash form, GOTOOLCHAIN=local, CGO_ENABLED=0 · DOTNET_ROOT the dotnet10 root (SDK 10.0.401) · python 3.12 · PATH must carry the POSIX dirs AND the gh dir or one of the two vanishes
```
WAKE (i9, verbatim from 59e0e3099 s3): re-create on resume -- i9's wake leg is a CronCreate job and CronList marks it [session-only]. The id cdf12613 is THIS session's and is dead to any other. Same for the Monitor id (bvgzqvs2y), per-session by construction. Neither is inheritable state; both are STEPS. i9 cadence: 7,27,47 past the hour, PROTOCOL v3.6 leg b.

```
You are lane i9 of the go2cs fleet (nickname i9 on every pushed surface). Model: Fable 5.1, effort high.
One serial item at a time (thermal). You execute the H4a/H5 rung and every long build. COORD rules;
you measure and post; suggestions go to COORD, never chips.
STATE: your STATE BLOCK is the fenced block at the TOP of this section (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE /
  NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it.  That block is the ONLY record: COORD
  re-reads every BRANCH pin from origin and re-derives NEXT, BLOCKED-ON and READ-FIRST from the LATEST ruling
  at every refresh.  Nothing else in this section carries state; a NEXT found anywhere else is stale by
  construction (i9 6520a98801, G f2f6240a1).  Read it top to bottom before the first command.
PROTOCOL: as COORD's section (post tool, watcher line, announce-then-push, nicknames only).
```

## 3. C1 — runtime hand-owns (cloud) — STATE BLOCK received 21:29 delta (mailbox 5e55c4b92 + 438b6f762)

```
You are lane C1 of the go2cs fleet (cloud session, linux container, NO .NET SDK; nickname C1). Model:
Opus 5 / high (C1's own recommendation; Fable 5.1 for the delicate re-derive cuts if the owner prefers).
You own the runtime hand-own re-derives and their appliers. Every C# reading you bank comes from the
os-matrix census workflow; GolibTests and go2cs.slnx are built by no workflow you can run.
STATE BLOCK (C1, 2026-09-13 15:39; push sweep: 13 of 13 branches already on origin at identical SHAs):
  LANE: C1   MODEL: Opus 5/high   HOST: C1 (linux container, no .NET SDK)
  BRANCH: claude/c1-h5-rederive-patch ff54907996fb2c7833b56e3608b878bdb467dc33 yes accepted -- C1-1 + C1-2 (amended 54ce45d9b) + C1-2b; VERSION-BRANCH ONLY (red converter guard at master by design); tip per cf06dafee (was 2c8841571 at 7d3734a84)
  BRANCH: claude/c1-mcleanup-handown 23d07f74260f96e88186bd3e14bc48812ad357b2 yes superseded -- mcleanup.cs hand-own + createfing rewire; train 48; census 306/306, both corpus flavours compile -- SUPERSEDED for the H5 set by the -clean re-cut below (docs conflict resolved; C1 fc64c7d0c)
  BRANCH: claude/c1-mcleanup-handown-clean d4e40e28bf6da7e676a887611bef2319c77f3d39 yes accepted -- the mcleanup seat re-cut on master 271300cea (DESIGN-managed-getg.md pure-append concatenation; code untouched); H5-set member (C1 fc64c7d0c; SHA verified at origin)
  BRANCH: claude/c1-token-door-census ad19af72b4cd964b5ee32db1f9bfbcc3e60625ba yes accepted -- token-door census (7 wrappers, 1 reached 6 latent) + the TestGetStartupInfo stale-bank record
  BRANCH: claude/c1-seat-duplication-census a4802675d4cc6e7a1843c309af30362cc9cd7dbb yes accepted -- patch-id seat census, SHA-first split, 8 arms
  BRANCH: claude/c1-lockosthread-body dc34e4b4a649a5a8365b769ede07e0567765e4ab yes landed -- LockOSThread carries Go's whole body (train-47 seat 15)
  BRANCH: claude/c1-crashwhiletracing-marking d781b0251999293b1430926575d2042e3d554c60 yes landed -- the 1-6/7-8 marking on TestCrashWhileTracing (train-47 seat 13)
  BRANCH: claude/c1-getcallerpc-erratum 3ca63093d55cce2c1bc413dffa2e0338df8149c9 yes landed -- the erratum block DESIGN-getcallerpc.md owed since train 46 (train-47 seat 14)
  BRANCH: claude/c1-board-goroot 5f0564da38e05beb59383ac214660fa5259e6e52 yes landed -- BOARD: -goroot is not read by the loader in any mode
  BRANCH: claude/c1-gctestisreachable-clean 4a9ae8cbbdba08e2a823b89ba2a5fddb7a620286 yes accepted -- managed gcTestIsReachable, the clean re-cut
  BRANCH: claude/c1-gctestisreachable 21222f2e86468e2a640236218073427522e99e66 yes superseded -- pre-re-cut; superseded by -clean
  BRANCH: claude/awesome-franklin-ba9agv 21222f2e86468e2a640236218073427522e99e66 yes superseded -- session-designated branch, same tip as the superseded gctestisreachable; no unique work; owner deletes
  BRANCH: claude/c1-mfinal-mint-door-clean 3f1612524a764fbc0732e5b44d511a4a698fd1c9 yes accepted -- mfinal pointer-mint door named per site, the clean re-cut
  BRANCH: claude/c1-mfinal-mint-door 0dab478581d78eb39a23c08a8ec232672009215a yes superseded -- pre-re-cut; superseded by -clean
  BRANCH: claude/c1-token-door-census-stacked b4914e878e7b4f0347c217ce028b7e4ccbc1b6a1 yes accepted -- train-48 row 11, the token-door census RE-CUT on top of row 13 (c2-census-goroot-fix-clean); stack-on=13
  BRANCH: claude/c1-h5-relocation a4ece44fff696e88c9d4a72059b12efaa3185a8a yes accepted -- the H5 relocation: five moves (autos -> .cs.auto), two deletes, two registry keys (crypto/internal/alias -> crypto/internal/fips140/alias; getgcmask -> pointerMask) + the namespace/class commit on top (ruling 43ce0c8e6); i9 fast-forwards claude/version-go1.24.13 onto its tip, then step 2 (f0837eea1)
  LOCAL-ONLY: c1-stranded-2 42044336c (in /c1-mailbox-clone) -- a post-tool commit stranded on a stale
        base 2026-09-13 23:06. CONTENT VERIFIED DELIVERED: the re-post carries the same heading
        ("C2's ad388158b PREDICATE APPLIED TO MY OWN 046d4f950, SIX MINUTES OLD") under a reworded
        subject, so no bundle is needed. ⚠ I checked this rather than trusting my own note that it had
        been re-posted -- the first phrase I searched for read 0 and only a second, differently-worded
        phrase found it. A stranded branch is exactly where "I remember re-posting that" goes wrong.
  (delta applied from mailbox 5e55c4b92)
  WORKTREE: C1 home /go2cs claude/c1-h5-rederive-patch 0 clean at the pushed tip
  WORKTREE: C1 home /c1-armA detached 2e6cf71e4 0 reachable from origin/claude/c1-board-goroot
  WORKTREE: C1 home /c1-armB detached 449ecce7a 0 reachable from origin/claude/g-generic-alias-recut
  NEXT: H6 ROW 20 on the H5 critical path (ruling post-gate-read 22:30): re-derive internal/sync/hashtriemap.cs against the pinned 1.24.13 internal/sync/hashtriemap.go (golang/go tag; G 553ac199b declarations) and the real 1.24.13 auto beside it (internal/sync/hashtriemap.cs.auto, an emission since checkpoint 2): keep the hand-own's managed-hashing design (the abi hasher contract is why it is hand-owned), give it 1.24's eleven public methods (All Clear CompareAndDelete CompareAndSwap Delete Load LoadAndDelete LoadOrStore Range Store Swap), NewHashTrieMap gone in favour of init/initSlow as the Go has it; ONE commit on claude/version-go1.24.13 f0f8826894 (your three H6 rows landed there), announce-then-push (or announce on claude/c1-h6-rows and i9 fast-forwards); the address guard on the tree first. The wrapper that fails is the auto sync/hashtriemap.cs (7 x CS1929: Store Range Clear Delete LoadAndDelete Swap CompareAndSwap)
  READ-FIRST: mailbox 46198c1b9 (the six dispositions measured; the git-move mechanism; the two registry keys crypto/internal/alias -> crypto/internal/fips140/alias and getgcmask -> pointerMask) · C1's own ce3add7af (pointerMask found by signature) · 3f54a3253 (row 76 RE-POINT) · 8be44bbc0a (C1-2b ruling)
  BLOCKED-ON: nothing. f0f8826894 is at origin with your three rows; row 20 is GO
  TOOLS: python3 3.11 on PATH (the applier's H5_PYTHON override exists for lanes without the name); GOROOT go1.24.7 (also 1.25.1 present), neither pin -- 1.23.12 and 1.24.13 are both pins are LOCAL and cheap: /golang/go carries go1.24.13 and go1.23.12 via `git fetch --filter=blob:none --depth 1 origin refs/tags/<tag>:refs/tags/<tag>` for BOTH tags = .git 820 KB, blobs pulled lazily per `git show <tag>:<path>`; no SDK, no working tree, no shallow clone. Assert provenance (origin URL + VERSION at each tag) before reading.; GOTOOLCHAIN unset; DOTNET_ROOT none
OPEN ACCEPTANCE (C1, gate-family decision, COORD's): CleanupDispatchTests' five arms on claude/c1-mcleanup-handown are written and unrunnable by any standing gate (GolibTests and go2cs.slnx are built by no workflow).
HELD RIDERS: seven small measured results in a C1 scratch file, ordered posted as ONE mailbox entry (COORD, save-state) so they survive the container.
NEXT (ruled f633ad759): C1-2 -- size the runtime2.cs 1.24.13 member bill (read the bodies at 1.24.13 first), then cut it as a hop-conditional applier after C1-1 (self-test: pre-C1-1 tree refuses; unpatched tree fails --verify naming members; idempotent); announce-then-push; i9 runs it.
PROTOCOL: as COORD's section.
```
WAKE (C1, verbatim from cf06dafee):
  WAKE: three claude-code-remote ROUTINES (create_trigger), NOT CronCreate jobs --
        trig_01HwSpTYDdZqjtJLpMBGCRKU `5 * * * *` / trig_01KfDoqdbnUk8A7MmviVogwn `25 * * * *` /
        trig_01Qd573JaByefkopyckGzhX1 `45 * * * *`, all enabled, all last run SUCCEEDED, all
        persistent_session_id-bound to THIS session = 20-minute cadence at 5/25/45. Read off
        list_triggers on this box, confirming C2's 77f2d31f8 reading rather than taking it.
        PLUS one CronCreate job 86a41926 at */17 added 21:25Z, so CronList on this lane is NOT empty
        any more -- C2's forecast that it would read "No scheduled jobs" here was true until then and
        the underlying point stands: CronList never enumerates Routines, so on this lane it answers a
        different population either way.
        IDS ARE AUDIT-ONLY. RECIPE on resume, UNCONDITIONAL and never gated on a check (i9's
        refinement): create three Routines at 5/25/45 bound to the NEW session with the C1 WAKE TICK
        prompt, plus one mailbox-tip Monitor.
⚠ and re-arm after a CONTAINER RESTART, which is a second death mode: it kills the Monitor with NO timeout notice, so it presents as silence rather than as an event. Measured this session ~02:19Z — worktrees, scratchpad tools and /root/c1-anchor all survived and every commit was already at origin, so the restart cost nothing except the watcher.

## 4. C2 — instruments and designs (cloud) — STATE BLOCK received 20:49 (mailbox d198239b4)

```
  LANE: C2            MODEL: Opus 5/high   HOST: C2-CONTAINER (linux cloud, ephemeral; NO dotnet, NO pwsh)
  BRANCH: claude/c2-h5c-apply-amendment 088f8778f6ce605f66ca6f2388068d7505b88d16 yes accepted -- H5c deletion-pass amendment; item 11 CLOSED per COORD 171972f5f; residue term added (local ref stale at 01caa02a03, origin is the record)
  BRANCH: claude/c2-h10-map-rederivation 41c1d1d28ef17381453d86899192bf1905b3f464 yes accepted -- H10 shard-map re-derivation + DATA record; timings read by column name, the plan-through-driver gap struck after i9 measured it
  BRANCH: claude/c2-darwin-trampoline-map 4bc0c35b01b0aff944c84f8433e105f81d6683c4 yes accepted -- darwin trampoline guard scores the TRANSFORM not containment, + the dated DESIGN §1.2 amendment; steps 2-3 wait on the hop per COORD
  BRANCH: claude/c2-h10-dispatch-driver 02b87b501d4bd5cf88d64d0c830b671d6098b642 yes accepted -- the H10 per-row dispatch driver; map gains a machine-readable plan
  BRANCH: claude/c2-h10-shardmap-projection a633896bf69d554f33b19b7a574466175ece32db yes cut -- the go1.24.13 H10 shard-map projection; BLOCKED awaiting train 48's base for its AMENDMENTS block
  BRANCH: claude/c2-darwin-option2-sizing 43e0dff04ccb19bc4dc7f753e1719b41598ff441 yes cut -- sizing of the darwin run layer's option 2, at a02ac3df3
  BRANCH: claude/c2-h6-crosscheck 191164e7a55d95755fd5d87c984ec7680ba1c298 yes cut -- C2's dated cross-check block on the H6 hand-own package-alias census
  BRANCH: claude/c2-board-peros-nested-hazard a0496fb937f337843e0e9f77b9969bd238a1d80d yes cut -- BOARD: DESIGN-peros-roster.md 7's nested table silently loses 5 of 11 floors
  BRANCH: claude/c2-census-goroot-fix 3ced37e1848ee0d86fda507033847f365da2fba5 yes cut -- mailbox skill: a clone with a negative refspec must not hold that ref at all
  BRANCH: claude/c2-runbook-shard-amendment 4140a8e55d993ed30ad1d17939497e8a185c8502 yes cut -- runbook 3.1's "exposes no jobs/throttle/shard/resume parameter" was FALSE at master
  BRANCH: claude/c2-sweep-hop-mode baf1fbe7273d0f44e246cfddd40d020e09c2c69c yes cut -- run-validated-sweep: $hop -> $hopCount, the counter was the switch parameter
  BRANCH: claude/c2-shardmap-repair 33c29952df89f75c009bebab649aafbfca5691a0 yes cut -- .gitattributes: pin the generator's input tables to eol=lf
  BRANCH: claude/c2-safepush-shallow-skip fa2fdd30dc1f06eeeccbcdc792eeb896157c5792 yes cut -- TestSafePushSelfTest skips with a named reason in a shallow clone
  BRANCH: claude/jolly-lovelace-j0sk1t bd1d26faffe1dd063fda91399ec9a2b35910fd8c no landed -- session-designated branch; the fleet kickoff commit, already an ancestor of origin/master, carries no unique work
  BRANCH: claude/c2-board-sparsearray-truncation ed9e58abb83f0e03465a0c036d435202af1b7aaa yes accepted -- the SparseArray truncation BOARD finding plus the dated amendment carrying i9's CLR measurement (9457d56c0); train-48 board carries this TIP, not the parent 258169d80 (COORD 873492c2f took that correction)  (delta applied from mailbox 0d8088e2b; tip verified at origin)
  BRANCH: claude/c2-board-both-ordered da5e8304735057c415e48e71f8ed1ec9b672e00b yes cut -- C2's re-cut per 3a28f2f87 (census-goroot-fix onto master 271300cea after the advance broke it; the BOARD pair ordered as one branch); FF-by-construction from 271300cea; SHA read from origin
  BRANCH: claude/c2-board-darwin-resolver 3ebe6cbfe858c5836c5bb52d6e5e848abaf5bccd yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-bucket3-darwin fbbc8cbb3651ad0fabcf5d2bafc33e2e03afe396 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-census-goroot-fix-clean 5cee80fbead7bb4c7716343c3b9d853e3a7baa13 yes cut -- C2's re-cut per 3a28f2f87 (census-goroot-fix onto master 271300cea after the advance broke it; the BOARD pair ordered as one branch); FF-by-construction from 271300cea; SHA read from origin
  BRANCH: claude/c2-darwin-board-t23 f065afd82bbbcc078a9d11e3f0139efb87663184 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-darwin-getaddrinfo c7d767ea4b5c4eb5d291966f4760d9f3fcc4cda3 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-darwin-inc10 5d53a5ad9b2fcc25cd2c5fc743d9293d41b962e1 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-darwin-inc9 d185e28b8d60518a134a2acdbc504855c2cee654 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-darwin-ptrout 409dc90f824db66a84d81c8f0ea14bd89bd1704e yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-elemindex-probe 9483bc624b3a9809597331096a26a77b3cba4186 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-getaddrinfo-probe 83385dad6c0d988dff718ad68bc3f098df7817db yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-getaddrinfo-probe-before 9ecce1839cbade0676ab3e2e60f7574aeb853167 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-pprof-blocker-wording 759453104c856f74e83a293813a39ba07a02609f yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-q44-cut eed11b55014bece81686370ba8a259a3e7ec4549 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-q44-record-amend 968ad27a4d26db9a86155d943e47096b36425e3a yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-q56-lift 0ac8a607cf6db2832f1dbcbb74cb1d0a0b327ae4 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-recon-go124 90a020f906f40d7abb41a81d0fc3e4c8a7affbc0 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-runlayer-pin e058910632b364c9fab644557c95f73e121aecb3 yes unclassified -- ON ORIGIN but NOT in C2's state block (an older ref; state unknown to COORD; C2 classifies or prunes it; SHA read from origin 18:05)
  BRANCH: claude/c2-h5c-slnx-orphan b291530e95eaed62928488a89c8fd74934692b27 yes accepted -- H5c: DELETE-ABSENT packages' slnx Project entries removed in -Apply (keyed on no surviving .csproj, accepted with the divergence), ORPHANED-HAND-OWN class refuses until every orphan has a disposition; one file on the H5-set tip; parse-gated on the i7 0 errors (46198c1b9 section 4)
  LOCAL-ONLY: c2-h5c-work 088f8778f6ce605f66ca6f2388068d7505b88d16 local alias, SHA identical to origin/claude/c2-h5c-apply-amendment so nothing is unpushed; no bundle cut because no bundle is needed none
  LOCAL-ONLY: refs/preserve/c2-container/** (28 refs, 251 commits) 036894085a1f778c not pushed -- historical container preserve namespace 08-28..09-06, security gate clean, all 13300 paths origin-reachable, zero unique content; a bundle in an ephemeral container preserves nothing so AWAITING YOUR RULING: push under one namespace or accept loss 036894085a1f778c
  WORKTREE: <C2-HOME>/go2cs claude/jolly-lovelace-j0sk1t 0 clean-at-landed-tip
  WORKTREE: <C2-HOME>/sswt claude/c2-darwin-trampoline-map 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/mrwt claude/c2-h10-map-rederivation 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/h5wt c2-h5c-work 0 clean-at-pushed-tip (alias of c2-h5c-apply-amendment)
  WORKTREE: <C2-HOME>/h6wt claude/c2-h6-crosscheck 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/cgwt claude/c2-census-goroot-fix 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/ddwt claude/c2-h10-dispatch-driver 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/dswt claude/c2-darwin-option2-sizing 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/rbwt claude/c2-shardmap-repair 0 clean-at-pushed-tip
  WORKTREE: <C2-HOME>/mbx-clone claude/mailbox 0 separate single-branch clone, mailbox transport only
  NEXT: nothing. The selection fix is cut and announced; i9's rerun is the next event and it is not mine. at 5c4c5b94e5) · 19d80c04e (the restore plan my tip is step 2 of) · c883a2dc7 (7 not 5) (C2 241eb474f, folded from the fenced delta)
  READ-FIRST: mailbox 46198c1b9 (H5c cut ACCEPTED with the no-surviving-csproj predicate; parse-gated on the i7) + 5b5e91b74 (the +283/-1 figure) · C2's own 77c6680fc (the cut) and a35f2c040 (the frozen tracking ref; i9 4ff24a03f widens it three-way -- row 13's rule wording is under-general when next touched) · master 271300cea · docs/phase4/BOARD-next-validation-candidates.md tail
  BLOCKED-ON: nothing of mine. Parse gate on the i7 is COORD's, not a block on C2 (ruling fefc7d4be). (C2 241eb474f, folded from the fenced delta)
  TOOLS: python3 3.11.15 · GOROOT=/usr/local/go1.24.7 (also /usr/local/go1.25.1) · module-cache toolchains go1.23.12 go1.24.13 go1.24.7 read directly for both-pin work · DOTNET_ROOT unset and dotnet ABSENT · pwsh ABSENT -- every .ps1 cut of mine is parse-gated on i7, never run here
```
COORD NOTE (16afb8a3f, amended per C2 56e93e709 s4): refs/preserve/c2-container/** (28 refs, 251 commits reachable from no origin ref, a prior container's ritual 08-28..09-06; never scrub-censused, so unpushable) -- zero unique PATHS; blobs COMPARED: 686 unique file versions over 369 paths, all intermediate states of 28 superseded lines whose current content is on origin (src 600, docs 81, CLAUDE.md 5). LOSS ACCEPTED on the record, nothing pushed; ref-list digest 036894085a1f778c.
WAKE (C2, verbatim from 0d8088e2b):
  WAKE: re-create on resume, UNCONDITIONALLY -- C2's wake leg is THREE claude-code-remote ROUTINES (create_trigger), NOT CronCreate jobs, which is why CronList reads "No scheduled jobs" on this lane and why that empty is not a defect. Each carries persistent_session_id = THIS session and is dead to any other. Recipe: create three Routines at cron 12/32/52 * * * * bound to the NEW session with the C2 WAKE TICK prompt. Audit-only ids: trig_01PehGf5ad4P1vN9XQcmrTs6 / trig_01DxLxSRnqCwtc4a5KEEb5gP / trig_012aMXji4bMictAY14m2SfHL. Monitor: per-session, re-arm as a STEP, and stamp `date -u` into its ARMED line and every event -- see §4.

```
You are lane C2 of the go2cs fleet (cloud session, linux, no PowerShell, disk-constrained; nickname C2).
Model: Fable 5.1, effort high. You author H5c/H10 instruments and designs; COORD parse-gates your .ps1
on the i7 and i9 executes them.
STATE: your STATE BLOCK is the fenced block at the TOP of this section (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE /
  NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it.  That block is the ONLY record: COORD
  re-reads every BRANCH pin from origin and re-derives NEXT, BLOCKED-ON and READ-FIRST from the LATEST ruling
  at every refresh.  Nothing else in this section carries state; a NEXT found anywhere else is stale by
  construction (i9 6520a98801, G f2f6240a1).  Read it top to bottom before the first command.
PROTOCOL: as COORD's section.
```

## 5. G — linux arm and census (G-LAPTOP) — STATE BLOCK received (mailbox a5534b5de)

```
  LANE: G   MODEL: Opus/high   HOST: G-LAPTOP
  BRANCH: claude/laneR-docs-h6-skeleton 067302ea09732cade2ef488a49fb7ab83410bd9d yes accepted -- 145 rows, still set-identical to the census at the NEW tip c2345d7731
  (delta applied from mailbox 1a806029b)
  BRANCH: claude/g-handown-metadata-t48-r47 35fe4e0167e044539245f7e4721a198fb35a98d0 yes accepted -- train-48 seat 6, the metadata un-freeze re-based onto the train-47 landing
  BRANCH: claude/g-h6-completeness-gate c9c1b5f737c8808f2234e01671768d0cb7848685 yes accepted -- train 49, the H6 gate + go-test guard + BOM tolerance
  BRANCH: claude/g-fleet-patchid-census 9b78bfff61000f5ca4984f163503c182b5c1819e yes accepted -- train 48, the fleet-wide patch-id census
  BRANCH: claude/g-repoguard-liveness-set 44857cdf898ef5d0b04e4b14351ec33c18290a38 yes accepted -- train 49, network-path-split liveness + finding-SET assertion
  LOCAL-ONLY: 18 refs (pre-session, never scrub-censused so never pushed) -- preserved in g2-state/g-local-only-2026-09-13.bundle on G-LAPTOP, 20 heads, 2,090,187 bytes, sha256 718de5f7d40da349, `git bundle verify` OK, all 18 at-risk tips present (0 missing, checked this hour)
  WORKTREE: G-LAPTOP go2cs/.claude/worktrees/row-harvest-2-1f7b91 claude/g-handown-metadata-t48-r47 0 uncommitted -- tree clean
  NEXT: row 20 internal/sync/hashtriemap.cs reads RE-DERIVE IN PROGRESS (C1) in the audit, not landed-clean: the gate on f0f8826894 is red by exactly that row (7 x CS1929 in the auto sync/hashtriemap.cs against the hand-own's 1.23 surface). Otherwise as before: verify i9's half-A manifest hash-by-hash on arrival, then fill from 067302ea0, PRINCIPAL CHANGED rows first, under the pair rule (80c948a7f; PRESENT/ABSENT by the principal's existence; moved-package rows take the old-path emission)
  READ-FIRST: 80c948a7f (per-row pair rule) · fc4ccab4b (the pair is an emission product) · 825c65222 (the 27/5 split and the five no-left-side rows) · d496727c8 (⚠ the mtime test has a FALSE-NEGATIVE hole per C2 241eb474f needToWriteFile; principal-existence at the release is the proposed test) · a5534b5de §2 (the ARTIFACT RECIPE: binary e0b2a4c1…, flags, tag line, seed, the version.props note, three tree hashes -- half B and the -tests pair are ON G-LAPTOP under h6-pair/, re-cuttable in ~16 min) · docs/phase4/AUDIT-h6-handown-go124.md at 067302ea0
  (delta applied from mailbox e5f16ea4a)
  BLOCKED-ON: lane -- i9's half-A manifest and share; unchanged by this landing
  (delta applied from mailbox 1a806029b)
  TOOLS: GOROOT = the go1.23.12 and go1.24.13 SDKs side by side, native backslash spelling, GOTOOLCHAIN=local, CGO_ENABLED=0; DOTNET_ROOT = the .NET 10 SDK (10.0.400); the machine defaults are NOT the pins
```
WAKE (G, verbatim from 26e7c0955 s1): re-create on resume, UNCONDITIONALLY -- G's wake leg is TWO mechanisms, both session-bound: (1) a MONITOR polling the mailbox tip (git ls-remote on refs/heads/claude/mailbox every 67 s from the mailbox clone, emitting MAILBOX-CHANGED old -> new, anchor asserted 40 chars at arm time, re-armed immediately after every firing); (2) a CRON wake tick every 20 min (PROTOCOL v3.6 leg b) running the same read-and-report pass. The ids in earlier posts (Monitor b0y8mzb29, CronCreate 07e74363) are this session's, audit only -- never checked on resume, only re-created. Owner instruction on this lane: watch claude/mailbox at ALL times and re-arm after every firing, standby included.

## 6. R — standby (R-LAPTOP, travel) — SAVE-STATE STEWARD prompt (owner order 2026-09-13 22:15); STATE BLOCK still pending from R

R's role while credits last: fold every lane's STATE BLOCK delta into this file by script, verify, commit (unsigned by owner authorization), announce-then-push, report the tip. The scripts are on claude/coord-instruments under .claude/coord-scripts/save-state/.

```
You are lane R of the go2cs fleet (nickname R / R-LAPTOP on every pushed surface). Model: Opus 5, effort high.
ROLE FOR THIS SESSION: SAVE-STATE STEWARD for the coordinator (COORD on the i7), by owner order at 94% of the
weekly usage limit. You keep docs/phase4/RESUME-SESSIONS.md on claude/coord-handover CURRENT so every lane can
resume on any machine; COORD keeps ruling. Work in a dedicated worktree of the repository checked out at
origin/claude/coord-handover (never a session or sub-agent in a worktree on that branch for anything else);
a second clone holds claude/mailbox (transport).

FIRST (once): post your own STATE BLOCK for the R section (keys LANE / BRANCH with 40-char SHAs read from
origin / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS, one line each, `none` rather than
omission, no angle brackets), and read the COORD section of the resume file for the security order and the
mailbox protocol. Then arm a Monitor on origin/claude/mailbox tip moves (60 s) and a wake loop (20 min); end
every post with the watcher line.

EVERY 20 MINUTES, AND AFTER EVERY LANDING OR RULING COORD POSTS:
  1. Pull the mailbox; read EVERY entry since your anchor (never a head or tail filter).
  2. Fold what the lanes posted, by SCRIPT, never retyped. The scripts live in the repo at
     .claude/coord-scripts/save-state/ on claude/coord-instruments (COORD pushed them 2026-09-13):
       replace-block.py LANE SHA        a complete fenced STATE BLOCK (starts with LANE:) replaces the lane's block
       apply-block-delta.py LANE SHA KEY,KEY   KEY: lines posted as a delta replace those keys
       apply-wake.py LANE SHA          a posted WAKE paragraph replaces the lane's WAKE paragraph
       refresh-resume.py spec.json     re-reads EVERY BRANCH pin from origin (ls-remote) and applies a
                                       spec: {"add": [[LANE, ref, description]], "keys": {LANE: {KEY: text}},
                                       "log": "...", "handover": "path-to-a-dated-block.md"}
     The scripts carry their paths as constants (repo root, the mailbox clone, the resume file) -- set them for
     your box once, in the copy you run, never in the pushed file.
  3. Verify: bash .claude/coord-scripts/coord-resume-verify.sh docs/phase4/RESUME-SESSIONS.md must read
     missing=0. Then the guard before the push: the diff's added lines carry no username path, no hostname, no
     IPv4 -- if a hit is a false positive (a version like 10.0.400) say so in the commit message.
  4. Commit on claude/coord-handover (lane commits are unsigned by owner authorization:
     git -c commit.gpgsign=false commit), announce-then-push on the existing ref, read the tip back with
     ls-remote, and post one line to the mailbox: the tip SHA, what was folded, the verifier line.
  5. If a lane's post carries state in a shape the scripts refuse (keys without colons, text without its key),
     copy it into a spec by a small script of your own and say so -- never retype a SHA.

WHAT YOU DO NOT DO: rule on anything; change any lane's NEXT from your own reading (only from a posted delta or
a COORD ruling that names it); push any branch other than claude/coord-handover; print or post the scrub
token file (sec/); use real hostnames or username paths anywhere.

READ FIRST: docs/phase4/RESUME-SESSIONS.md (COORD section, then your own), .claude/skills/save-state/SKILL.md,
docs/phase4/HANDOVER-coordinator.md (the SECURITY section and blocks 9-12), and the mailbox since
6481627c0 (the 94% order). If COORD is silent for more than 45 minutes, post a com-check and keep folding.
```

---

## 7. Revision log

- 2026-09-13 15:45 — skeleton: COORD section complete; lane sections from COORD's records, blocks pending
  (mailbox order 7ff30f203, due 16:30). Verifier: `.claude/coord-scripts/coord-resume-verify.sh`.
- 2026-09-13 15:55 — C1 (7d3734a84), G (f96225ea), i9 (29419cf30), C2 (d198239b4) blocks folded verbatim by fold-block.py; verifier gained landed-and-pruned and declared-local classes.
- 2026-09-13 16:18 — session-bound ids rule (§0) + WAKE lines for i9 and C2; accepted-loss wording amended (686 unique blobs, all intermediate states).
- 2026-09-13 17:15 — i9 block delta (NEXT / READ-FIRST / BLOCKED-ON) applied from 839a8d926; the rung is measured and idle.
- 2026-09-13 17:18 — C1 block delta (NEXT / READ-FIRST / BLOCKED-ON / WAKE verbatim; c1-h5-rederive-patch tip) applied from cf06dafee.
- 2026-09-13 17:48 — LANDED: master 31fe4925d (train 47) + 1885bce69 (doctrine d) + 271300cea (docs: this file, the skill and the verifier now on master too); claude/version-go1.24.13 created; H4 closed.
- 2026-09-13 18:30 — train 48 pinned and self-checked online (0 FAIL); rows 11-19 proposed; H5 proper in step 2 on the version branch.
- 2026-09-13 19:00 -- BRANCH pins re-read at origin by refresh-resume.py; i9's version-branch checkpoint, C2's H5c cut, C1's three train-48 refs and G's skeleton tip added; NEXT per lane per mailbox 46198c1b9 (the six orphan dispositions); handover block 9
- 2026-09-13 19:15 -- BLOCKED-ON re-measured for every lane (all four were stale: G f2f6240a1 caught its own -- a false BLOCKED-ON costs the session); G's NEXT corrected to RE-POINT x1 for mbitmap_impl.cs per 3f54a3253; READ-FIRST re-pointed at the latest rulings per lane
- 2026-09-13 19:25 -- G's WORKTREE key names its real branch (G 83eccc3e31); note: WAKE is a paragraph outside the fence by design, not a missing key
- 2026-09-13 19:32 -- C1's relocation ref added (tip re-read at origin), C1 NEXT/BLOCKED-ON after the namespace commit a4ece44fff; i9 lands it then step 2
- 2026-09-13 19:42 -- step 1 landed on the version branch (i9 9b7848700); i9 NEXT re-derived for step 2 with C2's narrowed tip and the superseded figure corrected (i9 caught its own stale +283); handover block 10
- 2026-09-13 20:30 -- NEXT re-derived for i9 (corrected reconvert + checkpoint 2 + the H6 pair), C1 (the weak/sync H6 rows on checkpoint 2; two train-49 guards), G (hold for the pair; OQ-2/OQ-3 ruled) per d2ad84bdb and fc4ccab4b
- 2026-09-13 21:15 -- G NEXT: half B cut, the fill rule (80c948a7f), pull half A by share; i9 NEXT: the restore, H5c on C2's fix, checkpoint 2, half A manifest + share
- 2026-09-13 21:45 -- SAVE-STATE at 94 percent (owner order): COORD state block rewritten to the current position (pins re-read from origin); fleet STATE BLOCK order posted 6481627c0; handover block 11
- 2026-09-13 21:55 -- C1 delta (5e55c4b92): two train-49 refs added, NEXT replaced verbatim, LOCAL-ONLY stranded post-tool commit noted; i9's never-push inventory restored under a dated marker (its 21:29 block reads none for new work)
- 2026-09-13 22:00 -- C2's delta (241eb474f section 4) folded: NEXT + BLOCKED-ON copied from the fence; its BRANCH pin re-read from origin
- 2026-09-13 22:12 -- CHECKPOINT 2 read back (version branch c2345d7731); i9/C1 NEXT for the gate rebuild; handover block 12
- 2026-09-13 22:22 -- R's section carries the SAVE-STATE STEWARD paste prompt (owner order 22:15): fold by script, verify, unsigned commit, announce-then-push, report the tip; the fold scripts are pushed on claude/coord-instruments under .claude/coord-scripts/save-state/
- 2026-09-13 22:32 -- H5 GATE READ on f0f8826894: red by row 20 only; C1 re-derives internal/sync/hashtriemap.cs; i9/G NEXT accordingly; version-branch pin re-read
