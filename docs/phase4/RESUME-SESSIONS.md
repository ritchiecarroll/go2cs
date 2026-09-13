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

The ladder: `docs/GoCorpusMigration.md` §2, H0–H12. Position at this revision: **H4 closes with train
47's landing (run 8 = the landing candidate); H4a/H5 rung on i9 has stages A–C and C1-1 banked at the
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

STATE AT THIS REVISION (2026-09-13 15:45):
  BRANCH: claude/coord-handover 7c76f343e5bd335c7bc837d1df5f18243e3697fd yes landed -- the handover log + this file
  BRANCH: claude/coord-train47-union dd021ff5ba1048ff1c0d354fc68dc75d17ced40f yes reproduction -- run-6 union head dd021ff5b, tree 161af6c44; DELETE after the landing
  BRANCH: claude/mailbox 7ff30f203cf9f13c5112658802ada4a6fbd8dee7 yes transport -- rotated 2026-09-13 02:36
  master tip at this revision: a02ac3df3 (train 47's base); the landing SHA is filled at the landing.
  TRAIN 47: fifteen seats (table in .claude/coord-scripts/train47/coord-train47-assemble.sh) on base
    a02ac3df3; union tree 161af6c44; runs 4-7 refused on the instrument (each a real defect, each fixed
    with a self-check lesson: LD1, LD2, LR1, LA1, LL1); RUN 8 launched 14:12 = the landing candidate.
    Landing sequence: land-anchor record census (coord-resume-verify is NOT this; use
    .claude/coord-scripts/train47 land + the census script described in the handover block 8) -> verify-only
    -> announce -> land (signed) -> read back -> create claude/version-go1.24.13 from the landed master ->
    doctrine d (21821509c) -> docs commit (KICKOFF section 4 refresh + CLAUDE.md floor item 12 amendment
    + runbook H5 seed-list line; TestContextBudget must pass; effective count 178 -> 180 of 200) ->
    handover block 8 -> train-48 pin fill.
  TRAIN 48 (template .claude/coord-scripts/train48/, derived by t48-derive.py, 189 ops, verified SOUND):
    board on the train-48 NOTES; pins PENDING until the landing SHA exists (T48_CONTAIN_PIN, EXPECT_G3).
    Seats include C1 mcleanup hand-own 23d07f742, C2 driver 02b87b501, goroot branch tip 3ced37e18,
    C2 H5c amendment stack (be9668d56 + 088f8778f on claude/c2-h5c-apply-amendment), a CNR seat that
    retains WARNING lines by kind, a converter per-file EMISSION MANIFEST seat.
  TRAIN 49 board: C1 patch (superseded by the rung's applier 2c8841571 on claude/c1-h5-rederive-patch),
    C1 token census ad19af72b4, G liveness 44857cdf8, G H6 gate c9c1b5f73, C2 sizing 43e0dff04,
    C2 trampoline map 4bc0c35b0, C2 map re-derivation 41c1d1d28 (claude/c2-h10-map-rederivation),
    i9 DATA record fd3f22f3b, C1-2 (runtime2.cs 1.24.13 member bill, sizing).
  THE RUNG (i9, H4a/H5 at the union): stage B (R's reconvert readings reproduced: 0 failed x3, 147
    WARNINGs, 146 hand-owns), stage C (H5c: 98/4/27/0, 102 deleted, exit 3 = arithmetic term, fixed at
    088f8778f), C1-1 applied (4a5938b7d5 tip; 120 -> 100 errors), next wall = C1-2 (frozen runtime2.cs
    lacks 1.24.13 members: g.syncGroup, g.isIdleInSynctest, m.mWaitList, fipsIndicator, six waitReason
    consts; synctest is 1.24-only). mcleanup carry is NOT in the rung (rc=1 expected on a train-47 tree);
    it discharges at H5 proper on the version branch after train 48 lands.
  OWNER HANDS OPEN: cloud allowlist for the dotnet builds host (C1/C2 cannot fetch the SDK), G-LAPTOP
    .git/index.lock, R-LAPTOP src/lane-r-packrace.ps1, the thermal-sentence host, delete
    claude/awesome-franklin-ba9agv, delete claude/coord-train47-union after the landing, remote branch
    deletions blocked at coordinator tooling.

FIRST ACTION: re-arm the Monitor and the wake loop; read the mailbox delta since the anchor in this file's
last handover block; post "COORD online" with the position (the rung, the train state) and the standing
line; then rule on whatever the lanes posted while you were down, in mailbox order.
```

---

## 2. i9 — H5 executor — STATE BLOCK received 15:45 (mailbox 29419cf30)

```
  LANE: i9            MODEL: Opus/high -- the rung's readings are cross-instrument reconciliations and three of today's corrections came from reading a subject rather than a report; a cheaper tier takes the report   HOST: i9-13900K (sweeper)
  BRANCH: claude/i9-data-recon-2026-09-13 fd3f22f3bb6d4a0fc9cb2635c3c652816c14d903 yes cut -- the recon DATA record, both passes + both TSVs under hopA-inputs, train 49
  BRANCH: claude/i9-board-archive-tar 314e699c649169a26658f648fd935298ea1a5e91 yes accepted -- archive/tar characterisation BOARD entry
  BRANCH: claude/i9-board-runtime-door-bisect 68ad83c2c13a073f3d11cbc28224b8ec66a8c88b yes accepted -- the runtime-row door bisect BOARD entry
  BRANCH: claude/hopa-sweep-i9 5a7454562591db79403658b0c1339d7dae3dd6d2 yes accepted -- hop-A sweep leg
  BRANCH: claude/i9-a1-collision-rename 08fef50674f3455354e8d98f4c4b61d61d752144 yes landed -- A1 collision rename
  BRANCH: claude/i9-a1-residual-defects 58aaf6ddf98a2a6f180fd716d36d72ce3f3fe5cf yes landed -- A1 residual defects
  BRANCH: claude/i9-a1-residual-round2 23bbe8427660cc7002bc3cf6883fa4c61af5aaf7 yes superseded -- by round3/round4
  BRANCH: claude/i9-a1-residual-round3 5442b402ea6b82b37f6194154f456a43beb8a6e9 yes superseded -- by round4
  BRANCH: claude/i9-a1-residual-round4 c229d67c5a1e5f95249d97d7892fe08fce9a7965 yes cut -- A1 residual round 4; its worktree holds 33 uncommitted, preserved as a write-tree branch + bundle
  BRANCH: claude/i9-a1-residual-round5 608ed292d346ea76d866b0b313e1548c4674ee11 no stale -- commit message says local only, not for master; parked behind the STOP-class accessibility-tier fix
  BRANCH: claude/i9-classifier-gap-fix 7cba1e39544b1bc151b56437b0e4380a25831596 yes landed -- comparison-classifier gap
  BRANCH: claude/i9-commit3-footprint 863b08cbbd03297079bc4e55e6310ef6d0733a83 yes accepted -- commit-3 footprint measurement
  BRANCH: claude/i9-commit3-measurement 7db72bca087eca462ff6d1fea65a398cdeeb4cc8 no stale -- commit message says not a real merge, local-only comparison build
  BRANCH: claude/i9-comparison-classifier bc092c9f3a4f43734bc00000df50258142ed1a1d yes landed -- the comparison classifier
  BRANCH: claude/i9-funcinfo-bridge f5ca2621e667ac5b61c419b2adb486fa7a59e728 yes cut -- funcinfo bridge; worktree holds 1 uncommitted, preserved
  BRANCH: claude/i9-gosym-rebank 2ce5fa643cfef31b6211b69e070038ec8fd54a8d yes landed -- debug/gosym rebank
  BRANCH: claude/i9-harness-twopin aa7abc0063be69a36d0637e1b189ee10c003a6cb yes landed -- the two-pin harness
  BRANCH: claude/i9-job023-roster-sweep 99e6649473c3713be7c5a36d5638c7815254f1be yes accepted -- JOB-023 roster sweep
  BRANCH: claude/i9-leveling-rebank e1ab3a72da153bf74f20f9245925865e0283b1af yes landed -- leveling rebank
  BRANCH: claude/i9-lift-accessibility-tier 54fa2b07b5636aae70d34c92b73a778e19f1c9ce yes landed -- the STOP-class accessibility-tier fix
  BRANCH: claude/i9-nonident-receiver-census e96749edebd7ee6b8d672252e6f156faf400c619 yes accepted -- non-ident receiver census
  BRANCH: claude/i9-release-tc0-census cf5cc518353ae101324adee572e0ed5d643a9fed yes landed -- the Release/TC0 census
  BRANCH: claude/i9-roster-guard-testconfig 67e163e3c575bde5afdd5c589e645fb4e2dd3152 yes landed -- roster guard test-config
  BRANCH: claude/i9-run-filter f5d2dd2335ef77f087f4aeece8c095aa52c5d9b6 yes landed -- the -Filter/-Exact run path
  BRANCH: claude/i9-runtime-regen 4df231e5a382d2dcd2e5dab96c106ee1e8921b6a yes accepted -- runtime regen
  BRANCH: claude/i9-runtime-semantic-bill 516d3c8735c5cc098e3acbe6c96d9060a18695b1 yes accepted -- the runtime semantic bill
  BRANCH: claude/i9-stub-message 4884a9cacb0b03a71b1089d52ab8f5454a5248bd yes landed -- stub message
  BRANCH: claude/i9-sweep-testconfig ac385553ef57b46fa698329f8d40607bfb9e7395 yes landed -- sweep test-config
  BRANCH: claude/i9-updatetesttargets-ordinal 47c3b1e85ff1bcea7d649331b02b60d87412dc96 yes landed -- UpdateTestTargets ordinal
  BRANCH: claude/i9-w3-accessibility 440b0603757b3ad2fa9a359ccfa4297bbdce5292 yes landed -- W3 accessibility
  BRANCH: claude/i9-wrapper-family 982bd5ef9c5a49db532d4ca2ff55f8d999e77303 yes landed -- the wrapper family
  BRANCH: claude/stage0-i9-provisioning 9cfbda17dd4ee5d40237490ced54fb0c1ce9f489 yes landed -- STAGE0 provisioning for this box
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
  WORKTREE: i9-board claude/i9-data-recon-2026-09-13 0 clean; the cut is on origin
  WORKTREE: i9-rung-union detached-at-the-union 0 clean; the rung's tree, delete with the reproduction ref
  WORKTREE: rung1-scratch-postrung not-a-worktree 0 RETAINED post-H5c + C1-1 scratch that C1-2 measures on -- do not clean
  WORKTREE: rung1-stage not-a-worktree 0 RETAINED three per-target staging roots behind item 8's .auto reading -- do not clean
  WORKTREE: rung-scratch not-a-worktree 0 FRESH stage-A/B tree for the 088f8778f proof run, conversion complete
  WORKTREE: i9-rung detached 84 scratch from the standalone 1.24.13 -tests emission; disposable, nothing owed
  WORKTREE: job-i9-q44 detached 30 write-tree branch i9-unbanked/job-i9-q44-2026-09-13 + bundle; HEAD unmoved
  WORKTREE: job-i9-train38 detached 39 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-pprof-landed detached 39 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-a1-round4 claude/i9-a1-residual-round4 33 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-NICK1 detached 11 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-runtime-remeasure detached 11 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-g-pprof detached 9 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-train37-pprof detached 9 write-tree branch + bundle; HEAD unmoved
  WORKTREE: job-i9-lift-accessibility claude/i9-funcinfo-bridge 1 write-tree branch + bundle; HEAD unmoved
  WORKTREE: mailbox-i9-clone6 claude/mailbox 1 never-push content, not bundled; the ref is the scrub order's and the content is unread
  NEXT: run stage C behind 088f8778f6ce605f66ca6f2388068d7505b88d16 on the fresh scratch (stage B finished: 147 WARNINGs, 146 hand-owns, identical to the first run) -- predictions exit 0, .cs 3900, residue .cs 37, hand-owns 146
  READ-FIRST: mailbox 0687402db (rung build result: R's 120 reproduced, C1-1 clears all seven, the next wall named), f633ad759 (COORD's C1-2 ruling), 7ff30f203 (this save-state order), 846cbd849 (stage C classification), f4c659e02 (item 8 .auto discriminator); docs/phase4/DATA-recon-pass1-2026-09-13.md on claude/i9-data-recon-2026-09-13
  BLOCKED-ON: lane -- C1-2's sizing before the rung's next build loop; nothing else
  TOOLS: GOROOT corpus/oracle $HOME/sdk/go1.23.12 (go1.23.12) and converter $HOME/sdk/go1.24.13 (go1.24.13), GOTOOLCHAIN=local, CGO_ENABLED=0, GOROOT spelled in BACKSLASH form or the emission misroutes and exits 0; python 3.12.0 -- there is NO python3 on this box AND WindowsApps shadows BOTH python and python3 with a Store redirector that exits 49, so prepend the real interpreter's directory or every candidate resolves to the alias; DOTNET_ROOT with SDK 10.0.401 beside 9.0.318 (net10.0 needs the 10.x); PowerShell 5.1 Desktop by ABSOLUTE path -- the Bash PATH is the Windows semicolon form so /usr/bin never resolves unless prepended on every call
```

```
You are lane i9 of the go2cs fleet (nickname i9 on every pushed surface). Model: Fable 5.1, effort high.
One serial item at a time (thermal). You execute the H4a/H5 rung and every long build. COORD rules;
you measure and post; suggestions go to COORD, never chips.
READ FIRST: docs/phase4/RESUME-SESSIONS.md (this file, COORD section for the ladder position);
  mailbox posts 8f2eafdc8, 846cbd849, 0687402db (your own rung readings) and COORD's rulings
  66e2b64d9, 49d0b9ea1, 990f3ba1b, dd03e6e3f, cee96ffad, 171972f5f, f633ad759.
STATE (from COORD's records; replace with your STATE BLOCK):
  scratch root: post-H5c + C1-1 applied at the union tree 161af6c44 (claude/coord-train47-union),
    src/gen and Directory.Build.props seeded for the build; logs build-prepatch2 (120), build-postpatch
    (100), applier-run (rc=1); pre-patch runtime2.cs/mfinal.cs kept aside.
  never-push content: nine i9-unbanked/* write-tree branches + the two KEEP rows, bundled with
    origin-reachable prerequisites, verified from an origin-only clone, copied to F: (digest 50f11d73...).
  reproduction ref claude/coord-train47-union dd021ff5b (COORD deletes after the landing).
NEXT: when C1 cuts C1-2, apply it after C1-1 on the scratch and rebuild runtime (the next-wall loop until
  it compiles); the proof run behind 088f8778f from a FRESH stage A (predict exit 0, .cs 3900, residue
  .cs 37, 146 hand-owns) when the rung allows.
TOOLS: GOROOT pins go1.23.12 and go1.24.13 side by side; DOTNET_ROOT the dotnet10 root; a real python
  on PATH ahead of the WindowsApps alias (the applier refuses the alias correctly); run instruments from
  a worktree at their SHA, never from an extracted copy.
PROTOCOL: as COORD's section (post tool, watcher line, announce-then-push, nicknames only).
```

## 3. C1 — runtime hand-owns (cloud) — STATE BLOCK received 15:39 (mailbox 7d3734a84)

```
You are lane C1 of the go2cs fleet (cloud session, linux container, NO .NET SDK; nickname C1). Model:
Opus 5 / high (C1's own recommendation; Fable 5.1 for the delicate re-derive cuts if the owner prefers).
You own the runtime hand-own re-derives and their appliers. Every C# reading you bank comes from the
os-matrix census workflow; GolibTests and go2cs.slnx are built by no workflow you can run.
STATE BLOCK (C1, 2026-09-13 15:39; push sweep: 13 of 13 branches already on origin at identical SHAs):
  LANE: C1   MODEL: Opus 5/high   HOST: C1 (linux container, no .NET SDK)
  BRANCH: claude/c1-h5-rederive-patch 2c884157167bb60e276b485acc43e760bd17719a yes accepted -- the C1-1 H5 re-derive applier, 16 arms; ran on i9's real post-H5c root, cleared all seven sites, 120->100
  BRANCH: claude/c1-mcleanup-handown 23d07f74260f96e88186bd3e14bc48812ad357b2 yes accepted -- mcleanup.cs hand-own + createfing rewire; train 48; census 306/306, both corpus flavours compile
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
  LOCAL-ONLY: c1-stranded-2026-09-13 a5aa199100e54a3c4eeddc447357a2c1e845f5e5 a post-tool commit stranded on a stale base; content superseded by the delivered entry ddcfde091 -- no bundle needed
  WORKTREE: C1 home /go2cs claude/c1-h5-rederive-patch 0 clean at the pushed tip
  WORKTREE: C1 home /c1-armA detached 2e6cf71e4 0 reachable from origin/claude/c1-board-goroot
  WORKTREE: C1 home /c1-armB detached 449ecce7a 0 reachable from origin/claude/g-generic-alias-recut
  NEXT: post the C1-2 member-bill sizing (measured, unposted at 15:39) then cut the runtime2.cs re-derive, starting from 2c884157167bb60e276b485acc43e760bd17719a
  READ-FIRST: mailbox 0687402db (i9 rung result), f633ad759 (the C1-2 assignment), a2b892aef + 3ea7c0e38 (C2 on the gate), a50d4f8c1 (i9 on the applier); docs/phase4/PATCH-h5-c1-1-runtime-rederives.md; docs/phase4/CENSUS-token-door-live-wrappers.md
  BLOCKED-ON: none (the dotnet builds host allowlist is an owner hand that would let this lane build; structural otherwise)
  TOOLS: python3 3.11 on PATH (the applier's H5_PYTHON override exists for lanes without the name); GOROOT go1.24.7 (also 1.25.1 present), neither pin -- 1.23.12 and 1.24.13 are fetched from source tags when needed; GOTOOLCHAIN unset; DOTNET_ROOT none
OPEN ACCEPTANCE (C1, gate-family decision, COORD's): CleanupDispatchTests' five arms on claude/c1-mcleanup-handown are written and unrunnable by any standing gate (GolibTests and go2cs.slnx are built by no workflow).
HELD RIDERS: seven small measured results in a C1 scratch file, ordered posted as ONE mailbox entry (COORD, save-state) so they survive the container.
NEXT (ruled f633ad759): C1-2 -- size the runtime2.cs 1.24.13 member bill (read the bodies at 1.24.13 first), then cut it as a hop-conditional applier after C1-1 (self-test: pre-C1-1 tree refuses; unpatched tree fails --verify naming members; idempotent); announce-then-push; i9 runs it.
PROTOCOL: as COORD's section.
```

## 4. C2 — instruments and designs (cloud) — STATE BLOCK received 20:49 (mailbox d198239b4)

```
  LANE: C2            MODEL: Opus 5/high   HOST: C2-CONTAINER (linux cloud, ephemeral; NO dotnet, NO pwsh)
  BRANCH: claude/c2-h5c-apply-amendment 088f8778f6ce605f66ca6f2388068d7505b88d16 yes accepted -- H5c deletion-pass amendment; item 11 CLOSED per COORD 171972f5f; residue term added (local ref stale at 01caa02a03, origin is the record)
  BRANCH: claude/c2-h10-map-rederivation 41c1d1d28ef17381453d86899192bf1905b3f464 yes accepted -- H10 shard-map re-derivation + DATA record; timings read by column name, the plan-through-driver gap struck after i9 measured it
  BRANCH: claude/c2-darwin-trampoline-map 4bc0c35b01b0aff944c84f8433e105f81d6683c4 yes accepted -- darwin trampoline guard scores the TRANSFORM not containment, + the dated DESIGN §1.2 amendment; steps 2-3 wait on the hop per COORD
  BRANCH: claude/c2-h10-dispatch-driver 02b87b501d4bd5cf88d64d0c830b671d6098b642 yes accepted -- the H10 per-row dispatch driver; map gains a machine-readable plan
  BRANCH: claude/c2-h10-shardmap-projection 0b24685bc2aad38a5a025b5145d298c65a56b0ee yes cut -- the go1.24.13 H10 shard-map projection; BLOCKED awaiting train 48's base for its AMENDMENTS block
  BRANCH: claude/c2-darwin-option2-sizing 43e0dff04ccb19bc4dc7f753e1719b41598ff441 yes cut -- sizing of the darwin run layer's option 2, at a02ac3df3
  BRANCH: claude/c2-h6-crosscheck 191164e7a55d95755fd5d87c984ec7680ba1c298 yes cut -- C2's dated cross-check block on the H6 hand-own package-alias census
  BRANCH: claude/c2-board-peros-nested-hazard a0496fb937f337843e0e9f77b9969bd238a1d80d yes cut -- BOARD: DESIGN-peros-roster.md 7's nested table silently loses 5 of 11 floors
  BRANCH: claude/c2-census-goroot-fix 3ced37e1848ee0d86fda507033847f365da2fba5 yes cut -- mailbox skill: a clone with a negative refspec must not hold that ref at all
  BRANCH: claude/c2-runbook-shard-amendment 4140a8e55d993ed30ad1d17939497e8a185c8502 yes cut -- runbook 3.1's "exposes no jobs/throttle/shard/resume parameter" was FALSE at master
  BRANCH: claude/c2-sweep-hop-mode baf1fbe7273d0f44e246cfddd40d020e09c2c69c yes cut -- run-validated-sweep: $hop -> $hopCount, the counter was the switch parameter
  BRANCH: claude/c2-shardmap-repair 33c29952df89f75c009bebab649aafbfca5691a0 yes cut -- .gitattributes: pin the generator's input tables to eol=lf
  BRANCH: claude/c2-safepush-shallow-skip fa2fdd30dc1f06eeeccbcdc792eeb896157c5792 yes cut -- TestSafePushSelfTest skips with a named reason in a shallow clone
  BRANCH: claude/jolly-lovelace-j0sk1t bd1d26faffe1dd063fda91399ec9a2b35910fd8c no landed -- session-designated branch; the fleet kickoff commit, already an ancestor of origin/master, carries no unique work
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
  NEXT: read the mailbox delta from 2a6938f4b96fcb35a7121b31d2408bce1b254433, then check whether C1's C1-2 cut closed the waitReasonStrings row my 2a6938f4b names (COORD ruled CUT IT at f9c551a5c on the shape that omits it); for darwin work start from 4bc0c35b01b0aff944c84f8433e105f81d6683c4
  READ-FIRST: mailbox 7ff30f203 (this order) 2a6938f4b (my waitReasonStrings finding) f9c551a5c (COORD: cut C1-2) 0df3d0991 (C1-2 sizing) f633ad759 (s4/s5 rulings) 0687402db (i9 rung) 171972f5f (item 11 closed) 29419cf30 (i9 save-state, the invented-SHA lesson) f96225ea5 (G save-state, un-censused branches are unpushable); docs/phase4/DATA-h10-map-rederivation-2026-09-13.md; docs/phase4/DESIGN-darwin-run-layer.md 1.2 amendment; src/reconvert-deletions.ps1
  BLOCKED-ON: lane -- train 48's base for the projection's AMENDMENTS block; the hop for darwin steps 2-3; i9's proof run of 088f8778f6. Plus one ruling owed from you: the preserve namespace above
  TOOLS: python3 3.11.15 · GOROOT=/usr/local/go1.24.7 (also /usr/local/go1.25.1) · module-cache toolchains go1.23.12 go1.24.13 go1.24.7 read directly for both-pin work · DOTNET_ROOT unset and dotnet ABSENT · pwsh ABSENT -- every .ps1 cut of mine is parse-gated on i7, never run here
```

```
You are lane C2 of the go2cs fleet (cloud session, linux, no PowerShell, disk-constrained; nickname C2).
Model: Fable 5.1, effort high. You author H5c/H10 instruments and designs; COORD parse-gates your .ps1
on the i7 and i9 executes them.
STATE (from COORD's records; replace with your STATE BLOCK):
  claude/c2-h5c-apply-amendment 088f8778f (stacked: 01caa02a0 -> be9668d56 -> 088f8778f; item 11
  closed); claude/c2-h10-map-rederivation 41c1d1d28; sizing 43e0dff04; trampoline map 4bc0c35b0;
  driver 02b87b501 (train 48); darwin plan steps 2-3 hardware-free, wait on the hop.
NEXT: train 48's base for the projection's AMENDMENTS block once the landing SHA exists; the per-file
  emission manifest seat (converter) if COORD assigns it.
BLOCKED-ON: owner hand -- the dotnet builds host allowlist.
PROTOCOL: as COORD's section.
```

## 5. G — linux arm and census (G-LAPTOP) — STATE BLOCK received 15:44 (mailbox f96225ea)

```
You are lane G of the go2cs fleet (G-LAPTOP with a WSL linux arm; nickname G). Model: Opus, effort high.
STATE BLOCK (G, 2026-09-13 15:44; push sweep: everything G owns is on origin; 20 pre-session local-only
branches are NOT pushed -- never scrub-censused -- and are preserved in a verified bundle):
  LANE: G   MODEL: Opus/high   HOST: G-LAPTOP
  BRANCH: claude/g-census-2026-09-13 31adad88c2ff17440b2b038f2a8aed86873599b6 yes accepted -- train-47 seat 12, the preservation census
  BRANCH: claude/g-h6-completeness-gate c9c1b5f737c8808f2234e01671768d0cb7848685 yes accepted -- train 49, the H6 gate + go-test guard + BOM tolerance (counts the 32 never-written .cs.auto rows, i9's auto-discriminator.tsv)
  BRANCH: claude/g-repoguard-liveness-set 44857cdf898ef5d0b04e4b14351ec33c18290a38 yes accepted -- train 49, network-path-split liveness + finding-SET assertion + joined-pass suppression
  BRANCH: claude/g-fleet-patchid-census 9b78bfff61000f5ca4984f163503c182b5c1819e yes accepted -- train 48, the fleet-wide patch-id census (a census, not a gate)
  BRANCH: claude/g-handown-metadata-t48 bb13897e6c73f3bd8ded1598a4b3b1b668fa65df yes accepted -- train-48 seat 6, the metadata un-freeze; OWES A RE-BASE once train 47 lands
  BRANCH: claude/g-h6-alias-census 898cbfefe9527726198a40654d954a8ff4dead4b yes accepted -- train-47 seat 8, the H6 alias census; base of the C2/R declared stack
  BRANCH: claude/g-generic-alias-recut 449ecce7a98b2a7acc2641ef82b9073d2566e143 yes accepted -- train-47 seat 7, the CollidingPackageNames generic arm
  BRANCH: claude/g-hop-b-provisioning d7bf606f070b2faa3738f53afc3aea2b101fc206 yes cut -- STAGE0 hop-B provisioning record for this box; offered, unruled
  BRANCH: claude/g-pprof-baseline 150b0264e85d52e5fe76f55d8b1dc75c89b546c7 yes cut -- runtime/pprof baseline before the pin moved; offered, unruled
  BRANCH: claude/g-l3-testalias 1d49a34b6578d382fac77a7beca92cc9dc2f7cd7 yes cut -- the -tests MERGE alias contradiction; offered, unruled
  BRANCH: claude/g-weak-rekey e7e976f9d4c5aac8b6e6a16c87d01015d2d1bb3a yes cut -- H4 re-key of crypto/internal/alias; offered, unruled
  BRANCH: claude/g-unfreeze-handown-metadata 7078dbada7377dc195e84d5e3752d133093e2669 yes superseded -- earlier cut of seat 6, superseded by bb13897e6
  BRANCH: claude/g-unfreeze-handown-recut ce2d9d082e5cbaba00674004c30cefb6e521c374 yes superseded -- second cut of seat 6, superseded by bb13897e6
  BRANCH: g-nilfunc-boxing 4b9513773fd4dd9dd05a482024c81af751f90051 yes stale -- older G work, on origin, no current claim
  BRANCH: claude/g-b1-box-design f632a942bb9ca67cbc8412ee9f3f6516fdc281b4 yes stale -- REMOTE tip; G's local 6815eba00 DIVERGES and the KICKOFF says push nothing here
  LOCAL-ONLY: 20 branches (listed in claude/g-census-2026-09-13) not-scrub-censused-so-not-pushed preserved: bundle g2-state/g-local-only-2026-09-13.bundle (2,090,187 bytes) sha256 718de5f7d40da349 -- verify "is okay", 20 refs, all 27 prerequisites in origin/master (restores from an origin-only clone); on the same single volume as the clone
  WORKTREE: G-LAPTOP go2cs/.claude/worktrees/row-harvest-2-1f7b91 claude/g-h6-completeness-gate 0 uncommitted -- tree clean, every branch above on origin
  NEXT: re-base seat 6 claude/g-handown-metadata-t48 from bb13897e6c73f3bd8ded1598a4b3b1b668fa65df onto the train-47 landing SHA once run 8 lands
  READ-FIRST: mailbox 7ff30f203 (the save-state order), dd9ea4a1d + 814e227a1 (the H6/goexperiment rulings), f4c659e02 + b42438150 (i9's .auto and emission readings that score G's work), docs/phase4/CENSUS-g-laptop-2026-09-13.md, src/check-handown-audit.ps1
  BLOCKED-ON: landing (seat 6 re-base); owner hand -- G-LAPTOP .git/index.lock
  TOOLS: GOROOT = the go1.23.12 sdk in native backslash spelling; GOTOOLCHAIN unset (auto); DOTNET_ROOT = the dotnet10 side-by-side root (SDK 10.0.400; the machine default 9.0.316 fails net10.0 with NETSDK1045); python none
Also owed: the filtered-sweep rule line (COORD's board).
PROTOCOL: as COORD's section.
```

## 6. R — standby (R-LAPTOP, travel) {PENDING: lane STATE BLOCK}

```
You are lane R of the go2cs fleet (R-LAPTOP, on TRAVEL STANDBY since 2026-09-13; nickname R). Model:
Fable 5.1, effort high, in spurts when the owner says so. You own reconvert-deletions.ps1 and the
rehearsal instrument; your readings are the rung's reference.
STATE (from COORD's records; replace with your STATE BLOCK):
  train-47 seats 3, 5, 10 (armc guard 49c309f8b, h5 lastrung 826045a74, h5 s15 rungs ff40eee3a);
  rehearsal hand-off 1d0ea0f79 (rehearsal tree == landing tree); your (b) at 4b4134242 superseded in
  wording by item 11's release-membership disposition; reconcile "eight sites" vs seven in a spurt.
BLOCKED-ON: owner hand -- src/lane-r-packrace.ps1 (owner's file).
PROTOCOL: as COORD's section.
```

---

## 7. Revision log

- 2026-09-13 15:45 — skeleton: COORD section complete; lane sections from COORD's records, blocks pending
  (mailbox order 7ff30f203, due 16:30). Verifier: `.claude/coord-scripts/coord-resume-verify.sh`.
- 2026-09-13 15:55 — C1 (7d3734a84), G (f96225ea), i9 (29419cf30), C2 (d198239b4) blocks folded verbatim by fold-block.py; verifier gained landed-and-pruned and declared-local classes.
