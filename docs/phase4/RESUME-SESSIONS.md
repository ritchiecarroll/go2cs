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
> **Status of this revision:** SKELETON, 2026-09-13 15:45 — COORD's section complete from the coordinator's
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

## 2. i9 — H5 executor {PENDING: lane STATE BLOCK}

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

## 4. C2 — instruments and designs (cloud) {PENDING: lane STATE BLOCK}

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

## 5. G — linux arm and census (G-LAPTOP) {PENDING: lane STATE BLOCK}

```
You are lane G of the go2cs fleet (G-LAPTOP with a WSL linux arm; nickname G). Model: Opus, effort high.
STATE (from COORD's records; replace with your STATE BLOCK):
  train-47 seats 7, 8, 12 (h6 alias census 898cbfefe, generic alias recut 449ecce7a, census record
  31adad88c) landing with train 47; liveness 44857cdf8 and the H6 gate c9c1b5f73 on the train-49 board
  (the H6 hand-own parity gate counts the 32 never-written .cs.auto rows -- i9's auto-discriminator.tsv);
  seat 6 re-base at the landing; filtered-sweep rule line owed.
BLOCKED-ON: owner hand -- G-LAPTOP .git/index.lock.
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
