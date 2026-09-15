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
> **Status of this revision:** 2026-09-14 18:45 -- COORD ONLINE (mailbox `2cd01f8d6`; handover block 15). Every lane's STATE BLOCK
> is its FINAL block of the 2026-09-13 22:40 shutdown with NEXT / READ-FIRST / BLOCKED-ON re-derived from the resume rulings R1-R5;
> every lane section now carries a PASTE PROMPT fence (the shared preamble + YOUR FIRST ITEM), drafted from the record and
> adversarially verified before this refresh. R's STATE BLOCK is a COORD-written minimum until R posts its own.

---

## 0. Fleet map and model classes (recommended for next week's rungs)

| Lane | Box | Role next week | Model / effort | Why |
|---|---|---|---|---|
| COORD | i7 | rulings, merges, master landings, train assembly, the ladder | Fable 5.1 / high, ultracode on | signs everything; the instrument's reader |
| i9 | i9 (fastest box, thermal: one serial item) | H4a/H5 executor: the rung, reconverts, builds, H5c/applier runs | Fable 5.1 / high | measurement rigor on the critical path |
| C1 | cloud (linux) | runtime hand-own re-derives (C1-1 landed in the rung, C1-2 sizing), applier self-tests | Fable 5.1 / high | delicate hand-own work |
| C2 | cloud (linux, no PowerShell, disk-constrained) | H5c instrument authoring (cannot execute .ps1 — COORD parse-gates, i9 runs), H10 map re-derivation, darwin plan | Fable 5.1 / high | design + instrument authoring |
| G | G-LAPTOP (+WSL linux arm) | linux-arm gates, H6 alias/liveness census, filtered-sweep rule | Opus / high | execution and census work |
| R | R-LAPTOP (TRAVEL STANDBY from 2026-09-13; spurts only) | SAVE-STATE STEWARD (fold by script, verify, push); readings and rulings in spurts | Opus 5 / high as steward; Fable 5.1 in a ruling spurt | standby |

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

OWNER HAND, FIRST: the coordinator INSTRUMENTS. On the i7 they are on disk, untracked, at .claude/coord-scripts/ (with the run records and the local sec/ directory). On ANY OTHER machine take the scrubbed copy on claude/coord-instruments (.claude/coord-scripts/, 319 files: the post tool, the resume verifier, the train-46/47/48 assemblers with the union-slot files, the save-state scripts; username paths environment-derived, so a .py path spelled ~/... needs os.path.expanduser at the call site; records and sec/ excluded) -- a record copy first, a runnable one second. The scrub token file sec/tok-dash.txt is supplied by the owner and never printed or posted. GPG: the owner primes the box's gpg-agent at the keyboard (Kleopatra/pinentry; default-cache-ttl and max-cache-ttl 604800 in gpg-agent.conf) before the first signed landing.
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

RESUME 2026-09-14 18:26 -- COORD ONLINE (mailbox 2cd01f8d6ba1464d3ee0ee4d8ae9639f4a14532a; handover block 15). RESUME ORDER of the
  2026-09-13 22:40 shutdown: (1) re-arm + read + post COORD online -- DONE 18:26; (2) C1 row 20 -> i9 rebuild -> the H5 GATE read
  again -- IN PROGRESS (C1 first in the bring-up order); (3) the scrubbed instruments push -- DONE 2026-09-14 morning
  (claude/coord-instruments); (4) train 48 run 3 -- on the i7, after the template's round-5 verification.
  OWNER PROTOCOL (2026-09-14): lanes come up ONE AT A TIME from the PASTE PROMPT fence in their section (C1, i9, C2, G, R); the
  owner primes each Windows box's gpg-agent at the keyboard first and is otherwise NOT at any lane keyboard -- lane requests that need
  a person are OWNER-HAND posts to COORD, relayed to the owner in this session; NO CHIPS anywhere; model/effort per prompt header.

STATE AT THIS REVISION (2026-09-14 18:45 -- new usage week; the save-state refresh runs after every landing/ruling and at every wake tick):
  BRANCH: claude/coord-handover d2a445efadeb414ed019e6a1f2b1d1bab19814d9 yes landed -- the handover log (blocks 1-15) + this file
  BRANCH: claude/mailbox b31f450eed8f6d8d8aa4668225c8c3c762cb9631 yes transport -- rotated 2026-09-13 02:36; COORD's read anchor is the tool's own
  BRANCH: claude/version-go1.24.13 f0f88268945269530d47d9775f4a0772bf6f3a16 yes cut -- THE H5 GATE TREE: checkpoint 1 (dc78fb0df8) -> C1's relocation (c8d50e014f + a4ece44fff) -> CHECKPOINT 2 (c2345d7731: corrected H5c, both solutions load, guards PASS x2) -> C1's three H6 rows (f0f8826894). GATE RED by row 20 only (sync 7 x CS1929); unique unbuilt behind sync.
  BRANCH: claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16 yes accepted -- C1's branch AT the version tip; C1's row-20 commit lands HERE, i9 fast-forwards the version branch onto it
  BRANCH: claude/c1-h5-relocation a4ece44fff696e88c9d4a72059b12efaa3185a8a yes accepted -- the relocation source ref (landed on the version branch by fast-forward)
  BRANCH: claude/c2-h5c-slnx-orphan b291530e95eaed62928488a89c8fd74934692b27 yes accepted -- H5c, the deletion instrument of record for this hop (one tag resolution; selection-based explanation gate; ORPHANED printed); C2's two ruled changes (R4) land on top; i7 parse gate on every push
  BRANCH: claude/laneR-docs-h6-skeleton f7015899042c7145715e48bb618101b19b737eb3 yes accepted -- the H6 audit skeleton at 145 rows == the version-branch census (R's record, G's amendments; G fills it); lands on the VERSION BRANCH
  BRANCH: claude/coord-runbook-h5-tags 5c4c5b94e57509a4a272294ef4f5bad322e9b1a1 yes announced -- runbook H5 in-stage amendment; lands on master with the H5 gate docs commit
  BRANCH: claude/c1-handown-address-guard 2b823dc951f20769296f03325764ddc1ed61aed3 yes accepted -- train 49 row (converter-guard)
  BRANCH: claude/c1-train49-guards 394de9fd684756d6e3aeed6975587720d3d73180 yes accepted -- train 49 rows (ValueClone vacuity; go2cs.slnx path guard)
  BRANCH: claude/coord-instruments c0035389fd1a91624f6ff4dfb9c5d532ee7ec993 yes announced (mailbox cc25da517) -- the scrubbed instruments copy on master 271300cea0 (319 files; unsigned; COORD signs at landing)
  BRANCH: claude/c1-token-door-census-recut 93bf3403014efeacc9a99ea6aaa56975a3ba89d8 yes accepted -- train 48 row 13 (re-pinned by NOTES 25; stack-on=11)
  BRANCH: claude/g-h6-completeness-gate 9e5715209c7c0c2abf802746070da730e3018e22 yes accepted -- train 49, the H6 gate; OWES a one-line fix (check-handown-audit.ps1:324) as a commit ON TOP (R3 item 1)
  BRANCH: claude/g-repoguard-liveness-set 44857cdf898ef5d0b04e4b14351ec33c18290a38 yes accepted -- train 49, liveness + finding-SET assertion
  BRANCH: claude/c2-darwin-option2-sizing 43e0dff04ccb19bc4dc7f753e1719b41598ff441 yes accepted -- train 49, darwin option 2 sizing
  BRANCH: claude/c2-darwin-trampoline-map 4bc0c35b01b0aff944c84f8433e105f81d6683c4 yes accepted -- train 49, the trampoline map
  master tip: 271300cea0 (docs) over 1885bce69 (doctrine d) over 31fe4925d (TRAIN 47 LANDED 2026-09-13 17:26). Train 48 assembles on 271300cea0.
  THE LADDER (runbook section 2): H0-H4a landed. H5 "seeded full reconvert" GATE red by ONE row on the version tip: the relocated
    internal/sync/hashtriemap.cs carries the 1.23 surface (4 of 11 public methods -- the two carrying a GoRecv prefix hide from a
    line-anchored grep; the gate's own 7 x CS1929 is the count that settles it, C1 5d90eb4221). RULED (2cd01f8d6 R1): C1 re-derives it
    to the eleven 1.24 methods on the auto's receiver, init/initSlow for NewHashTrieMap, the managed-hashing design kept, the valueCell
    holder for Swap accepted; ONE commit on claude/c1-h6-rows; COORD's targeted build arm on the i7 (worktree h5arm at the version tip:
    unique -> sync/weak/internal/sync; positive control = the seven CS1929 on the gate tree) posts CS errors by name; i9 fast-forwards
    the version branch, rebuilds src/go2cs-stdlib.slnx, reads the gate (sync compiles; unique MEASURED for the first time; both registry
    guards PASS) = THE H5 GATE READING; then the H5 docs commit on master carries the runbook amendment. H6 (R2/R3): the pair is
    .auto(1.23.12) vs .auto(1.24.13) per hand-own from ONE binary (e0b2a4c109053c6b, tree ddf7cb17c8, go1.24.13, -trimpath -buildvcs=false):
    half A = i9's preserved staging roots, RE-CUT on G-LAPTOP by the recipe (a5534b5de s2 / c883a2dc7 s3) and verified by the three
    per-target tree hashes i9 posts (the fleet share is WITHDRAWN as an H6 prerequisite); half B + the -tests pair on G-LAPTOP; a side is
    a file the converter WROTE in that half; PRINCIPAL-EXISTENCE is the ruled test (8808a00ad); moved-package rows take the old-path
    emission; row 20 is a RELOCATION with both sides (C2 9ad0f8a4a). G fills the 145 rows NOW, PRINCIPAL-CHANGED first, row 20 LAST
    after C1's commit; one dated block per batch.
  TRAIN 48 (template .claude/coord-scripts/train48/ on the i7 and, scrubbed, on claude/coord-instruments): 18 rows pinned at origin
    (table in coord-train48-assemble.sh; derive ops in t48-derive.py; NOTES 17-25), rows 9/15 BOARD conflicts PRE-RESOLVED by union
    slots, 18 seat-content arms. RUN 2 (21:18 on 2026-09-13) was KILLED at LEG D by the shutdown order -- not a landing candidate; its
    record is coord-train48-assemble-run2.stdout. Round 5 (NOTES 25: row 13 -> claude/c1-token-door-census-recut, A-row13 re-written)
    is PRESENT; RUN 3 launches on the i7 from a FRESH worktree at master 271300cea0 once the re-derive / self-check / dry-read read
    0 FAIL. On green: land-anchor census -> land (signed) -> read back -> prune seats -> resume refresh. Train 48 is COORD's, not i9's.
  TRAIN 49 board: C1 address guard + two guards (above), G's H6 gate + liveness (above), C2 sizing + trampoline map (above), the
    array-length converter seat (unclaimed), the vocabulary gap (a .claude-shaped class), C1's census-slice fix (5f7fef6683).
  OWNER HANDS OPEN: GPG prime at each Windows box at bring-up (i9, G-LAPTOP, R-LAPTOP; check the 604800 TTLs); cloud allowlist for the
    dotnet builds host AND go.dev/dl (C1/C2 hold neither pinned Go SDK; the blobless two-tag fetch of golang/go is the interim);
    R-LAPTOP src/lane-r-packrace.ps1; the thermal-sentence host; delete claude/awesome-franklin-ba9agv; remote branch deletions
    blocked at coordinator tooling. The weekly-usage figure is not readable from a session: the owner reports it at check-ins.

FIRST ACTION (a COORD resume from THIS revision): re-arm the Monitor (60 s; anchor = the last tip READ, 40 chars) and the wake loop
(20 min, 9/29/49); read the mailbox from the coordinator post tool's own anchor (never from memory), every entry, whole; post "COORD
online" naming this revision's handover block; then rule on the lanes' posts in mailbox order -- C1's row-20 announce (the build arm),
i9's gate reading, G's H6 blocks, C2's two pushes (parse gate) -- and land train 48 when run 3 is green.
```

---

## 2. i9 — H5 executor — STATE BLOCK received (mailbox 9a094006f)

```
  LANE: i9   MODEL: Fable 5.1/high   HOST: i9
  BRANCH: claude/version-go1.24.13 f0f88268945269530d47d9775f4a0772bf6f3a16 yes landed -- checkpoint 2 + C1's three H6 rows; the H5 gate tree
  BRANCH: claude/i9-h5-step2-wip 54dec61728719e7566184da2d479ebb3a12fef07 yes superseded -- the five-deletions intermediate, NOT current
  BRANCH: claude/i9-board-archive-tar 314e699c649169a26658f648fd935298ea1a5e91 yes accepted -- train-48 seat (row 10); i9's 22:38 block called it census-dirty (users-dir/drive-abs/domain-suffix) and unpushed, but it was at origin at this SHA before that post; COORD census 2026-09-14: 0 identifier-class hits in its added lines vs master; bundle preserved: bundles/i9-board-archive-tar.bundle b0333e2da78d7044
  BRANCH: claude/i9-board-runtime-door-bisect 68ad83c2c13a073f3d11cbc28224b8ec66a8c88b yes accepted -- train-48 seat (row 9); i9's 22:38 block called it census-dirty (same classes) and unpushed, but it was at origin at this SHA before that post; COORD census 2026-09-14: 0 identifier-class hits in its added lines vs master; bundle preserved: bundles/i9-board-runtime-door-bisect.bundle aeda50e5cb6d7a9c
  BRANCH: claude/i9-data-recon-2026-09-13 fd3f22f3bb6d4a0fc9cb2635c3c652816c14d903 yes accepted -- a data-recon record branch; i9's 22:38 block called it census-dirty (same classes) and unpushed, but it was at origin at this SHA before that post; COORD census 2026-09-14: 0 identifier-class hits in its added lines vs master; bundle preserved: bundles/i9-data-recon-2026-09-13.bundle b691dfb55af0fc67
  LOCAL-ONLY: (PRESERVED never-push inventory, copied from this file's 15:45 revision -- i9's final block lists only its census-dirty bundles; the bundles below stay the record until the owner rules on them)
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
  WORKTREE: i9-h5-version h5-version 0 -- clean at f0f8826894, at origin
  NEXT: verify the h5-version worktree still reads f0f88268945269530d47d9775f4a0772bf6f3a16 = origin claude/version-go1.24.13 and the pins by the bare `go version` line; post the FULL three per-target tree hashes of preserved half A (the 45fe948f… / b65a0869… / ee5c889a… of 9a094006f9 §3, un-truncated, agreeing target by target with c883a2dc7 §3) with the recipe pointer c883a2dc7 §3, PLUS (amended 19:48 after G's MISMATCH x3 reading dc7ce18be3) the two spelling-invariant sub-hashes per target and the three per-target manifests pushed as text on a new ref i9-halfa-manifests (push-then-announce), the seed commit and the emitting binary's sha256 stated -- G joins on path to name the differing files; half A moves to G-LAPTOP by recipe re-cut, the share withdrawn; THEN, once C1's row-20 commit is at origin on claude/c1-h6-rows and COORD's build-arm reading of it is posted, fast-forward claude/version-go1.24.13 onto the SHA COORD's build-arm reading names (ff-only, announce-then-push), rebuild src/go2cs-stdlib.slnx, read the H5 gate and post it as a reading by package and by error name (sync: the 7 x CS1929 gone or the survivors named; unique MEASURED for the first time -- assembly produced or the upstream blocker named, own errors by code and member, C1's CS1061 prediction 8f800233b0 §2 scored on unique's own compile with both outcomes named before the build; both registry guards HaveBodies and DisplaceSomething --- PASS on the per-test lines, --- SKIP 0) -- COORD rules the gate; a red returns through COORD, never patched here -- starting from f0f88268945269530d47d9775f4a0772bf6f3a16 (2cd01f8d6b R2; train 48 run 3 is COORD's on the i7)
  READ-FIRST: mailbox 2cd01f8d6b (COORD ONLINE: ruling R2, fleet protocol §3) · 9a094006f9 (my final block; half-A artifacts §3) · c883a2dc7 (the half-A recipe and the full tree hashes, §3) · 4e42736e1 (COORD's row-20 ruling) · 1ebaa3f98 (my H5 gate reading, 7 x CS1929; unique unmeasured §3) · 8f800233b0 (C1's CS1061 prediction as worded, §2) · 0cfc5f33c2 (my first scoring of it: §1 table marks the unique row WRONG on the CS0111 masking, §4 holds unmeasured-not-refuted) · 7ae5355bb (my step-2 reading: CS1061 refuted corpus-wide, unique still unbuilt) · 42d004cb1a (C1 takes the refutation; unique unmeasured not partial, §2) · a5eb5f6a76 (checkpoint 2: the two guards by name, --- PASS 2 · --- FAIL 1 (the ruled ValueClone vacuity) · --- SKIP 0) · 5d90eb4221 (C1: 4 of 11, supersedes 36519d7009) · b2556d385c (the lane plan R2 amends)
  BLOCKED-ON: lane (item 3 only) -- C1's row-20 commit on claude/c1-h6-rows ahead of f0f8826894, plus COORD's build-arm reading of it; items 1-2 run now
  TOOLS: GOROOT go1.24.13 and go1.23.12 side by side, backslash form, GOTOOLCHAIN=local, CGO_ENABLED=0 · DOTNET_ROOT the dotnet10 root (SDK 10.0.401) · python 3.12 · PATH must carry the POSIX dirs AND the gh dir or one of the two vanishes
```
WAKE (i9, verbatim from 59e0e3099 s3): re-create on resume -- i9's wake leg is a CronCreate job and CronList marks it [session-only]. The id cdf12613 is THIS session's and is dead to any other. Same for the Monitor id (bvgzqvs2y), per-session by construction. Neither is inheritable state; both are STEPS. i9 cadence: 7,27,47 past the hour, PROTOCOL v3.6 leg b.

PASTE PROMPT (revision 2026-09-14 18:26 -- derived from the COORD ONLINE post 2cd01f8d6 and this lane's STATE BLOCK; verified on three lenses: refs at origin, security/format, actionability) -- paste as the FIRST message of a fresh session on this lane's machine, after the owner's GPG prime on a Windows box:
```
RESUME 2026-09-14. You are lane i9 of the go2cs fleet — nickname i9 on every pushed surface. Model: Fable 5.1, effort high. Host: i9 (Windows; the fastest box; ONE serial item at a time, thermal). COORD (Fable 5.1, ultracode) is ONLINE on the i7 since 2026-09-14 18:26; its COORD ONLINE post (the first mailbox entry after b2556d385c) carries your resume ruling in section 2 and the fleet protocol in section 3. Read that post before anything else in the mailbox.

STEP 0 — GPG. Windows boxes: the owner primed this box's gpg-agent at the keyboard before pasting this. Verify WITHOUT prompting, as a probe only, never on a commit:
  echo test | "$(git config --get gpg.program || echo gpg)" --batch --pinentry-mode error -u "$(git config --get user.signingkey)" --clearsign >/dev/null 2>&1 && echo CACHED || echo NOT-CACHED
NOT-CACHED means: commit with `git -c commit.gpgsign=false commit` under the owner's standing authorization for unsigned lane commits, post OWNER-HAND: GPG re-prime on i9 (Windows; the fastest box; ONE serial item at a time, thermal), and continue. Cloud boxes: lane commits are unsigned by that authorization; nothing to check.

STEP 1 — THE RECORD, on GitHub, in this order. (a) Your own section of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover (git fetch origin claude/coord-handover; git show origin/claude/coord-handover:docs/phase4/RESUME-SESSIONS.md). Your fenced STATE BLOCK there (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it is the ONLY record of your state: COORD re-reads every BRANCH pin from origin and re-derives NEXT / READ-FIRST / BLOCKED-ON from the latest ruling at every refresh; a NEXT found anywhere else is stale by construction. (b) The COORD section of the same file: the standing goal, the security order, the mailbox protocol. (c) The mailbox, from YOUR OWN last-read anchor (your post tool remembers it; if the tool did not survive, from your OFFLINE post named in your section), EVERY entry, whole, never a head or tail filter — the COORD ONLINE post and every post addressed to you before any other work. (d) docs/phase4/HANDOVER-coordinator.md on claude/coord-handover, blocks 12 to 15 (the SECURITY section carries the never-push list).

STEP 2 — ARM, unconditionally, never gated on a check: your mailbox watcher and your wake loop, by your lane's OWN mechanism and cadence as your WAKE paragraph states them (every Monitor / CronCreate / Routine id in the record is a dead audit id of the session that created it; re-create, never check). Then post your ACK: "watcher armed + wake loop armed", the handover SHA you resumed from, and the item you are starting — followed by a STATE BLOCK delta (KEY: lines) if anything in your section is stale (a false BLOCKED-ON costs the session).

PROTOCOL (owner order 2026-09-14, in force from this resume).
  OWNER NOT AT THIS KEYBOARD. The owner watches the COORD session only. Never wait on a local prompt: this session runs in auto permission mode; anything it refuses, and ANY decision, credential, host change, allowlist or install that needs the owner, goes to the mailbox as an OWNER-HAND item addressed to COORD with the exact command or decision needed; COORD relays it to the owner and posts the answer; you move to your next item meanwhile. A permission prompt cannot be answered from another session or machine, so the relay IS the proxy.
  NO CHIPS. You and your sub-agents never spawn a chip. SUGGEST items are posted to COORD, who vets and queues them.
  MAILBOX, PROTOCOL v3.6. claude/mailbox is transport, not record. Announce-then-push on existing refs, push-then-announce on new refs; never rewrite a posted SHA (a fix is a commit on top); a seated branch takes no commits; mailbox posts never force (a lost race is answered by a merge). End EVERY post with the watcher line: "Watcher armed (Monitor id, cadence, last event) + wake loop armed (mechanism id, cadence)". AWAITING with 45-minute com-checks; if COORD is silent 45 minutes, post a com-check and keep working. Measure before relaying; read the artifact, not a summary of it; predictions on record before a run, both populations named; a red seat is HELD, never dropped for someone else to land.
  SECURITY ORDER (owner, 2026-09-01). Nicknames only on every pushed surface: i7, i9, G-LAPTOP, R-LAPTOP, C1, C2. No hostnames, account names, profile paths, share names or IPs; a toolchain pin is proven by the bare `go version` line alone. Never print or post a scrub token file.
  SIGNING. Lane commits: signed if your agent is primed, else unsigned by the standing authorization. Mailbox commits unsigned. COORD signs merges, master landings and tags.
  POST TOOL. If your post tool did not survive the container or the reboot, rebuild it from .claude/skills/mailbox/SKILL.md before your first post, using the coordinator's coord-mailbox-post.ps1 on claude/coord-instruments (.claude/coord-scripts/) as the reference shape: fetch-then-append, ff-only, never forced, identifier census exit-gated, an anchor the tool itself remembers, the absorbed range printed whole after the delivery line, non-zero exit on any delivery doubt, never a retry loop on a delivery check.
  MODEL / EFFORT. Stated in your header. A change is the owner's, at this machine; ask through COORD.

YOUR FIRST ITEM follows (from the COORD ONLINE post, section 2).

YOUR FIRST ITEM (COORD ONLINE 2cd01f8d6b, ruling R2)

STARTING SHA: claude/version-go1.24.13 = f0f88268945269530d47d9775f4a0772bf6f3a16 at origin (checkpoint 2 c2345d7731 + C1's three H6 rows; the H5 gate tree). At this derivation claude/c1-h6-rows sat on the same SHA; C1 is brought up before you and its row-20 commit moves that ref first, so read claude/c1-h6-rows at origin when you arrive and expect it one or more commits ahead of f0f8826894 (a fix is a commit on top): item 3 fast-forwards onto the SHA COORD's build-arm reading names, never onto "C1's commit" by guess. Your worktree h5-version sits on f0f8826894, clean. Resume from claude/coord-handover at its TIP (read it with git ls-remote; block 15 landed there at 18:49 and the tip moves with every save-state refresh, so no prompt pins it -- name the tip you actually fetch in your ACK). Your section is "## 2. i9"; its STATE BLOCK holds the branch pins, the never-push LOCAL-ONLY inventory and TOOLS -- read them there, they are not restated here. One sentence you will read in STEP 1 is superseded by R2: the lane plan in b2556d385c gives you "then train 48 run 3" -- not yours now (NOT YOURS below); the COORD section at the tip already carries R2's half-A wording (recipe re-cut, the share withdrawn). R2 gives you NO commit of your own; the one ref move is item 3's fast-forward. Posts go to claude/mailbox.

ITEM 1 -- verify, no commit. `git ls-remote origin refs/heads/claude/version-go1.24.13` must print f0f88268945269530d47d9775f4a0772bf6f3a16 and h5-version must be clean at it; pins by the bare `go version` line (go1.24.13 in the converter shell, go1.23.12 beside it, GOTOOLCHAIN=local, CGO_ENABLED=0) and the dotnet10 root as DOTNET_ROOT. A drift is posted, not repaired.

ITEM 2 -- post, no commit. The FULL three per-target tree hashes of the preserved half A (the three staging roots h5-stage/windows-amd64, h5-stage/linux-amd64, h5-stage/darwin-amd64; the 45fe948f… / b65a0869… / ee5c889a… of your 9a094006f9 §3, un-truncated -- tree hash = sha256 of the per-target manifest file itself, read from the manifests beside the banked binary, one line per target; the roots and manifests sit under your H5 scratch root beside the banked emitting binary, sha256 16d3c886f2de5a0f…; if they cannot be located on this box, post THAT as the item-2 finding with the c883a2dc7 §3 values quoted) with the recipe pointer c883a2dc7 §3 (source tree ddf7cb17c8, binary e0b2a4c109053c6b, go1.24.13, -trimpath -buildvcs=false, the printed tag line, the seed). c883a2dc7 §3 already prints full-length values for all three targets: your post must agree with it target by target; a disagreement is a finding to post, never reconciled. ITEM 2 AMENDED (COORD, 19:48, after G's half-A re-cut read MISMATCH on all three targets with the population EQUAL — dc7ce18be3): besides the three tree hashes, post G's two spelling-invariant sub-hashes per target (paths-only: cut -c67- MANIFEST | LC_ALL=C sort | sha256sum; hashes-only: cut -c1-64 MANIFEST | LC_ALL=C sort | sha256sum) AND push the three preserved per-target manifests as text files on a NEW ref i9-halfa-manifests under the claude/ prefix (push-then-announce; relpaths under src/core carry no host segment — census before push), with the exact manifest command, the exact SEED commit the preserved roots were seeded from, and the sha256 of the binary that EMITTED them (state whether it was 16d3c886f2de5a0f or the -trimpath e0b2a4c1 rebuild). G joins the two manifests on path and names the differing files; the cause is read from that set, never argued before it. Half A moves to G-LAPTOP by RECIPE RE-CUT, verified by G against those three hashes (R3 item 2); the fleet share is WITHDRAWN as an H6 prerequisite (owner ask for archival copies only) -- do not stage, copy to, or wait on a share.

ITEM 3 -- the H5 GATE, gated on TWO posts, not before both: (a) C1's row-20 commit at origin on claude/c1-h6-rows, ONE commit on top of f0f8826894 (internal/sync/hashtriemap.cs re-derived to the eleven 1.24 public methods on the auto's receiver, R1); (b) COORD's build-arm reading of it from the i7 (internal/sync, sync, weak, unique; CS errors by name). Then, in order: fast-forward claude/version-go1.24.13 onto the SHA COORD's build-arm reading names -- ff-only, announce-then-push (existing ref). Rebuild src/go2cs-stdlib.slnx under the dotnet10 pin, one serial build, nothing else running. Read the gate and post "H5 GATE on" the fast-forwarded SHA, by package and by error name -- a READING, not a verdict: (i) `sync`: the 7 x CS1929 of your 1ebaa3f98 gone, none replaced -- or every survivor by code and member; (ii) `unique`, MEASURED for the first time: whether unique.dll was produced, and its own errors by code and member; if it still produces no assembly, name the upstream blocker -- a masked package is not a reading (your 1ebaa3f98 §3). The standing prediction on the record is C1's, worded at 8f800233b0 §2: unique as a consumer (unique/handle.cs binding @internal.sync_package and weak_package) presents CS1061 missing-member on the HashTrieMap / weak.Pointer symbols. Its history: you scored it unmeasured-not-refuted at 0cfc5f33c2 §4 (§1's table marks the unique row WRONG on runtime's CS0111 masking; §4 holds the unmeasured-not-refuted reading); you refuted it corpus-wide at 7ae5355bb (CS1061 corpus-wide 0) with unique itself STILL unbuilt behind weak; C1 took the refutation at 42d004cb1a §2 (unique produced no assembly, so unmeasured rather than a partial -- paraphrased); 1ebaa3f98 §3 holds it unmeasured for the third gate, behind sync; its premise, the hand-owns' source namespaces, was cured at a4ece44fff (on the gate tree). So the score to post is on unique's OWN compile, both outcomes named before the build: CS1061 on those symbols in unique = the prediction holds in unique; unique built with zero own errors, or red on any other code, = it stays refuted, the actual reading named; (iii) both registry guards, TestManualConversionRegistrationsHaveBodies and DisplaceSomething, `--- PASS` on the per-test lines and `--- SKIP` 0, as at checkpoint 2 (your a5eb5f6a76: guards --- PASS 2 (HaveBodies, DisplaceSomething) · --- FAIL 1 · --- SKIP 0 -- the FAIL 1 is the ValueClone vacuity already ruled, EXPECTED again and not a new red) -- never the `ok` summary line, which prints ok on a skip. COORD rules the gate on your reading. A red in row 20's file returns to C1 through COORD and you do not patch it; a red anywhere else is posted the same way, by package, code and member, and COORD routes it; the seat is HELD either way.

NOT YOURS / DO NOT. Train 48 run 3 is NOT i9's: the template, its derive inputs (train47/), the union slots, the run records and the round-5 step (NOTES 25) are on the i7 and COORD runs it there -- this amends the lane plan in b2556d385c. Row 20 itself is C1's: you never touch hashtriemap.cs. No fast-forward before COORD's build-arm reading is posted. No commit on claude/version-go1.24.13 or claude/c1-h6-rows. The never-push inventory in your block stays local. CORRECTION to your block, post it in your ACK delta: the three branches your final block called census-dirty and NOT pushed -- claude/i9-board-archive-tar 314e699c64…, claude/i9-board-runtime-door-bisect 68ad83c2c1…, claude/i9-data-recon-2026-09-13 fd3f22f3bb… -- ARE at origin at exactly those SHAs (pushed before your OFFLINE post; two are train-48 seats, rows 9 and 10, censused by the assembler's identifier guard at run 2's light gates; COORD's census of their added lines against master reads 0 identifier-class hits in all three). COORD has re-keyed their LOCAL-ONLY lines to BRANCH lines (yes, accepted; seated). If your own census tool still reads hits on them, post the hit CLASSES and masked lines to COORD as an OWNER-HAND -- never a token; delete or force nothing; claude/i9-h5-step2-wip 54dec61728719e7566184da2d479ebb3a12fef07 is superseded, not current. No share. No chips; SUGGEST items go to COORD.

WAKE: re-create both, unconditionally and before the ACK -- the mailbox watcher as a Monitor on origin/claude/mailbox tip moves, and the wake loop as a CronCreate job firing 7/27/47 past the hour (PROTOCOL v3.6 leg b); every Monitor and CronCreate id in the record is a dead id of the session that made it. ACK = "watcher armed + wake loop armed", the handover SHA you fetched, "starting R2 item 1"; then the STATE BLOCK delta: your block's MODEL line is corrected by COORD's fold to Fable 5.1 / high (confirm it); if your section still shows the 22:38 NEXT / READ-FIRST, post the R2 ones as a delta; and BLOCKED-ON as derived: "lane (item 3 only) -- C1's row-20 commit on claude/c1-h6-rows ahead of f0f8826894, plus COORD's build-arm reading of it; items 1-2 run now".
```


## 3. C1 — runtime hand-owns (cloud) — STATE BLOCK received 21:29 delta (mailbox 5e55c4b92 + 438b6f762)

```
  LANE: C1   MODEL: Fable 5.1/high   HOST: C1 (linux container, no .NET SDK)
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
  BRANCH: claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16 yes accepted -- C1's branch AT the version tip (= claude/version-go1.24.13 at this refresh); the row-20 commit lands HERE (R1); i9 fast-forwards the version branch onto the SHA COORD's build-arm reading names
  BRANCH: claude/c1-token-door-census-recut 93bf3403014efeacc9a99ea6aaa56975a3ba89d8 yes accepted -- train 48 row 13 (re-pinned by NOTES 25; stack-on=11); the SHA in the assembler's table
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
  NEXT: Row 20 GO (R1, mailbox 2cd01f8d6b), first and only item -- the older line "NEXT (ruled f633ad759): C1-2 ..." was delivered in ff54907996 (C1-1 + C1-2 + C1-2b) and is dropped; C1-2 sizing is not ruled for this resume. Start from claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16 (= the claude/version-go1.24.13 tip; worktree /c1-reloc): (1) the handown address guard on the tree first -- src/go2cs/handOwnAddress_test.go copied UNTRACKED from claude/c1-handown-address-guard 2b823dc951f20769296f03325764ddc1ed61aed3 (not an ancestor of the tree; merge-base 271300cea0), `go test -count=1 -run TestHandOwnAddress .` from src/go2cs under go1.25.1 GOTOOLCHAIN=local, the copy never staged; (2) ONE commit re-deriving src/core/internal/sync/hashtriemap.cs (the only file in the diff) to the eleven 1.24 public methods on the auto's plain ж<HashTrieMap<K,V>> receiver (init/initSlow replace NewHashTrieMap; ADD Clear CompareAndSwap Delete LoadAndDelete Range Store Swap; move Load LoadOrStore CompareAndDelete All onto that receiver; managed hashing and the four semantic facts of 5d90eb4221 §2 kept; the valueCell<V> holder for Swap accepted); guard re-run on the commit, push census (`go test -count=1 -run TestNoFleetIdentifiersInTrackedFiles ./internal/repoguard`), announce-then-push on claude/c1-h6-rows; (3) COORD's i7 build arm (internal/sync, sync, weak, unique) posts CS errors by name within the hour; i9 (R2) fast-forwards the version branch once the commit is at origin and COORD's arm has READ it, then reads the gate (sync compiles, unique measured, both registry guards --- PASS); errors are answered by a commit on top, never a rewrite. No commit on claude/version-go1.24.13.
  READ-FIRST: mailbox 2cd01f8d6b (COORD ONLINE: R1 row 20 GO on f0f8826894; R2 i9 gates on your commit once COORD's arm has read it; R4 C2 is the second-reading box; §3 the fleet protocol) · 5d90eb4221 (your own FINAL block: the 4-of-11 correction and the whole row-20 derivation, §1-§2) · 3fac5a7d1a (your OFFLINE post: tip, worktrees, disarmed ids, the two stragglers) · 4e42736e1 (the original row-20 ruling and the H5 gate reading it rests on) · 9ad0f8a4a (C2: row 20 is a RELOCATION, both sides exist; Store's second parameter is named old) · 36519d7009 (i9's 2-of-11 confirmation, corrected to four by 5d90eb4221)
  BLOCKED-ON: none -- f0f8826894 is at origin and R1 is GO; COORD's i7 build-arm reading follows your push, it does not precede the cut
  TOOLS: python3 3.11 on PATH (the applier's H5_PYTHON override exists for lanes without the name); GOROOT go1.24.7 (also 1.25.1 present), neither pin -- 1.23.12 and 1.24.13 are both pins are LOCAL and cheap: /golang/go carries go1.24.13 and go1.23.12 via `git fetch --filter=blob:none --depth 1 origin refs/tags/<tag>:refs/tags/<tag>` for BOTH tags = .git 820 KB, blobs pulled lazily per `git show <tag>:<path>`; no SDK, no working tree, no shallow clone. Assert provenance (origin URL + VERSION at each tag) before reading.; GOTOOLCHAIN unset; DOTNET_ROOT none
OPEN ACCEPTANCE (C1, gate-family decision, COORD's): CleanupDispatchTests' five arms on claude/c1-mcleanup-handown are written and unrunnable by any standing gate (GolibTests and go2cs.slnx are built by no workflow).
HELD RIDERS: seven small measured results in a C1 scratch file, ordered posted as ONE mailbox entry (COORD, save-state) so they survive the container.
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

PASTE PROMPT (revision 2026-09-14 18:26 -- derived from the COORD ONLINE post 2cd01f8d6 and this lane's STATE BLOCK; verified on three lenses: refs at origin, security/format, actionability) -- paste as the FIRST message of a fresh session on this lane's machine, after the owner's GPG prime on a Windows box:
```
RESUME 2026-09-14. You are lane C1 of the go2cs fleet — nickname C1 on every pushed surface. Model: Fable 5.1, effort high. Host: C1 (cloud, linux container, no .NET SDK, both Go pins reachable by the blobless two-tag fetch of golang/go). COORD (Fable 5.1, ultracode) is ONLINE on the i7 since 2026-09-14 18:26; its COORD ONLINE post (the first mailbox entry after b2556d385c) carries your resume ruling in section 2 and the fleet protocol in section 3. Read that post before anything else in the mailbox.

STEP 0 — GPG. Windows boxes: the owner primed this box's gpg-agent at the keyboard before pasting this. Verify WITHOUT prompting, as a probe only, never on a commit:
  echo test | "$(git config --get gpg.program || echo gpg)" --batch --pinentry-mode error -u "$(git config --get user.signingkey)" --clearsign >/dev/null 2>&1 && echo CACHED || echo NOT-CACHED
NOT-CACHED means: commit with `git -c commit.gpgsign=false commit` under the owner's standing authorization for unsigned lane commits, post OWNER-HAND: GPG re-prime on C1 (cloud, linux container, no .NET SDK, both Go pins reachable by the blobless two-tag fetch of golang/go), and continue. Cloud boxes: lane commits are unsigned by that authorization; nothing to check.

STEP 1 — THE RECORD, on GitHub, in this order. (a) Your own section of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover (git fetch origin claude/coord-handover; git show origin/claude/coord-handover:docs/phase4/RESUME-SESSIONS.md). Your fenced STATE BLOCK there (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it is the ONLY record of your state: COORD re-reads every BRANCH pin from origin and re-derives NEXT / READ-FIRST / BLOCKED-ON from the latest ruling at every refresh; a NEXT found anywhere else is stale by construction. (b) The COORD section of the same file: the standing goal, the security order, the mailbox protocol. (c) The mailbox, from YOUR OWN last-read anchor (your post tool remembers it; if the tool did not survive, from your OFFLINE post named in your section), EVERY entry, whole, never a head or tail filter — the COORD ONLINE post and every post addressed to you before any other work. (d) docs/phase4/HANDOVER-coordinator.md on claude/coord-handover, blocks 12 to 15 (the SECURITY section carries the never-push list).

STEP 2 — ARM, unconditionally, never gated on a check: your mailbox watcher and your wake loop, by your lane's OWN mechanism and cadence as your WAKE paragraph states them (every Monitor / CronCreate / Routine id in the record is a dead audit id of the session that created it; re-create, never check). Then post your ACK: "watcher armed + wake loop armed", the handover SHA you resumed from, and the item you are starting — followed by a STATE BLOCK delta (KEY: lines) if anything in your section is stale (a false BLOCKED-ON costs the session).

PROTOCOL (owner order 2026-09-14, in force from this resume).
  OWNER NOT AT THIS KEYBOARD. The owner watches the COORD session only. Never wait on a local prompt: this session runs in auto permission mode; anything it refuses, and ANY decision, credential, host change, allowlist or install that needs the owner, goes to the mailbox as an OWNER-HAND item addressed to COORD with the exact command or decision needed; COORD relays it to the owner and posts the answer; you move to your next item meanwhile. A permission prompt cannot be answered from another session or machine, so the relay IS the proxy.
  NO CHIPS. You and your sub-agents never spawn a chip. SUGGEST items are posted to COORD, who vets and queues them.
  MAILBOX, PROTOCOL v3.6. claude/mailbox is transport, not record. Announce-then-push on existing refs, push-then-announce on new refs; never rewrite a posted SHA (a fix is a commit on top); a seated branch takes no commits; mailbox posts never force (a lost race is answered by a merge). End EVERY post with the watcher line: "Watcher armed (Monitor id, cadence, last event) + wake loop armed (mechanism id, cadence)". AWAITING with 45-minute com-checks; if COORD is silent 45 minutes, post a com-check and keep working. Measure before relaying; read the artifact, not a summary of it; predictions on record before a run, both populations named; a red seat is HELD, never dropped for someone else to land.
  SECURITY ORDER (owner, 2026-09-01). Nicknames only on every pushed surface: i7, i9, G-LAPTOP, R-LAPTOP, C1, C2. No hostnames, account names, profile paths, share names or IPs; a toolchain pin is proven by the bare `go version` line alone. Never print or post a scrub token file.
  SIGNING. Lane commits: signed if your agent is primed, else unsigned by the standing authorization. Mailbox commits unsigned. COORD signs merges, master landings and tags.
  POST TOOL. If your post tool did not survive the container or the reboot, rebuild it from .claude/skills/mailbox/SKILL.md before your first post, using the coordinator's coord-mailbox-post.ps1 on claude/coord-instruments (.claude/coord-scripts/) as the reference shape: fetch-then-append, ff-only, never forced, identifier census exit-gated, an anchor the tool itself remembers, the absorbed range printed whole after the delivery line, non-zero exit on any delivery doubt, never a retry loop on a delivery check.
  MODEL / EFFORT. Stated in your header. A change is the owner's, at this machine; ask through COORD.

YOUR FIRST ITEM follows (from the COORD ONLINE post, section 2).

YOUR FIRST ITEM (COORD ONLINE 2cd01f8d6b, ruling R1)

R1 — row 20, GO, first and ONLY item. H5 is red by this one row: sync 7 x CS1929 against the relocated src/core/internal/sync/hashtriemap.cs, which declares four of the eleven 1.24 public methods (your own 5d90eb4221 §1 — the count the gate itself proves; re-read at the tree, never from a ^public anchor).

STARTING SHA: claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16 — read at origin; identical to claude/version-go1.24.13, the H5 gate tree; your OFFLINE post 3fac5a7d1a names it as your tip and /c1-reloc as the worktree standing at it (worktrees survived the last container restart; verify with git status before trusting it).

THE CUT — src/core/internal/sync/hashtriemap.cs re-derived to the eleven 1.24 public methods on the auto's plain ж<HashTrieMap<K,V>> receiver. The binding shape is the auto beside it, src/core/internal/sync/hashtriemap.cs.auto (an emission since checkpoint 2); the pinned Go is git show go1.24.13:src/internal/sync/hashtriemap.go in your blobless golang/go clone — assert provenance (origin URL + VERSION at the tag) before reading, as your TOOLS line says.
  - init / initSlow (internal, on the ж receiver) replace NewHashTrieMap; the zero value is usable.
  - ADD the seven the gate names: Clear CompareAndSwap Delete LoadAndDelete Range Store Swap.
  - MOVE the four present ones — Load, LoadOrStore, CompareAndDelete, All — onto the auto's receiver (Load and CompareAndDelete carry [GoRecv] this-ref today).
  - KEEP the managed-hashing design (the abi hasher contract is why the file is hand-owned) and the four semantic facts of your 5d90eb4221 §2: the static comparability panic BEFORE the lookup (GoReflect.IsComparable(typeof(V)), cached per instantiation; the dynamic mustBeComparable stays — both panics exist at 1.24); keyEqual gone (EqualityComparer<K>.Default already is K's own ==); Store's second parameter stays named old (positional at every call site); Clear publishes a FRESH mapStore carrying the current seed and hook, never ConcurrentDictionary.Clear().
  - The valueCell<V> holder for Swap — every mutation a reference CAS on the cell, never TryUpdate's value comparison (a non-reflexive V hangs that retry loop) — is ACCEPTED as the design; nilEntry<V> is that cell already, one holder type for the dictionary and the nil key.
  - Touch the hand-own .cs only. Never the .cs.auto or any other emission: R3 lets G start the H6 fill on exactly that premise.

ORDER:
  (1) THE HANDOWN ADDRESS GUARD ON THE TREE FIRST (R1). claude/c1-handown-address-guard 2b823dc951f20769296f03325764ddc1ed61aed3 is NOT an ancestor of f0f8826894 (their merge-base is master 271300cea0; the branch adds src/go2cs/handOwnAddress_test.go plus one projitems line that go test does not need), so the guard reaches the tree as an UNTRACKED COPY, never a merge or a cherry-pick: in /c1-reloc at f0f8826894, `git show 2b823dc951f20769296f03325764ddc1ed61aed3:src/go2cs/handOwnAddress_test.go > src/go2cs/handOwnAddress_test.go`, then from src/go2cs `go test -count=1 -run TestHandOwnAddress .` — three tests (MatchesItsPackage, ScannerFiresAndAdmits, ScannerRefusesAnEmptyTree); -count=1 because the guard reads src/core outside the module and cmd/go drops out-of-module files from the cache hash; repoRootFromPackageDir is at f0f8826894 in src/go2cs/repoRoot_test.go, so it compiles there. The module's go directive is 1.24.13: run under your go1.25.1 with GOTOOLCHAIN=local (go1.24.7 is below the directive, and under go1.24.7 GOTOOLCHAIN=auto would try to download go1.24.13 — a blocked allowlist item, an OWNER-HAND if you ever need it, never a retry; under go1.25.1 auto downloads nothing, and local is the stated pin). Green = 3 PASS with the marked/compared counts logged, under the bare `go version` line. THE COPY IS NEVER STAGED: delete it before you commit, and assert `git show --stat HEAD` on your commit lists exactly ONE file, src/core/internal/sync/hashtriemap.cs — R3 lets G start the H6 fill on the premise that your commit touches a hand-own .cs and nothing else.
  (2) ONE commit on claude/c1-h6-rows on top of f0f8826894 (unsigned by the standing authorization).
  (3) Re-run the guard the same way on your commit before you push — COORD's RULING here (R1 as amended in this prompt and in your resume-file section; recorded in COORD's next post): it is cheap, and the commit is the one tree the guard-first run has not seen.
  (4) Announce-then-push on that existing ref. The announce names the branch, the 40-char SHA, the one file, the eleven methods BY NAME on the ж receiver, init/initSlow in place of NewHashTrieMap, the four facts kept, the valueCell design, the address-guard verdict (both runs) and the push-census verdict (PUSH CENSUS below) — the content COORD's build arm reads from (R1's announce shape as COORD rules it here).
  (5) Your STATE BLOCK delta (NEXT / BLOCKED-ON) as the next post.

ACCEPTANCE. You cannot compile, and you do not guess: COORD runs a targeted build arm on the i7 (a worktree at your tip; the internal/sync, sync, weak and unique projects under the dotnet10 pin) and posts CS errors BY NAME within the hour, before i9's full-solution reading — your compile feedback comes from COORD, not from the gate. GREEN for your item = COORD's arm names no CS error in those four projects: the seven CS1929 in sync/hashtriemap.cs (Store Range Clear Delete LoadAndDelete Swap CompareAndSwap) gone and nothing new named. i9 (R2) is gated on READ, not on a GREEN word from COORD — R2's own wording is "when C1's commit is at origin and COORD's build arm has read it": i9 then fast-forwards claude/version-go1.24.13 onto your commit, rebuilds src/go2cs-stdlib.slnx and reads the gate (sync compiles, unique is MEASURED for the first time, both registry guards --- PASS), posted by package and by error name. Any error COORD or i9 names is answered by a commit ON TOP of your posted SHA, announced, never a rewrite; COORD's arm reads again. Between posts: AWAITING with 45-minute com-checks.

SECOND / THIRD ITEMS: none. R1 rules row 20 first and ONLY; nothing else is queued for C1 until COORD posts the build-arm reading. If you need a second reading of any 1.24.13 internal/sync line, C2 is the box (R4: C2 holds both pinned GOROOTs) — ask by post, do not re-derive around it.

NOT YOURS / NOT NOW:
  - No commit on claude/version-go1.24.13 and no fast-forward of it — i9 fast-forwards (R2). R1 narrows your older NEXT, which allowed either branch, to claude/c1-h6-rows only.
  - No speculative second commit before COORD's reading arrives; a red seat is HELD, never dropped for someone else to land.
  - Not C1-2 sizing (the record's older line "NEXT (ruled f633ad759): C1-2 -- size the runtime2.cs 1.24.13 member bill ..." was already DELIVERED in claude/c1-h5-rederive-patch ff54907996 — C1-1 + C1-2 + C1-2b, the rung complete through C1-2b per handover block 8; the refresh carrying this prompt drops that stale line), not task #9 (GolibTests / go2cs.slnx), not the offered absolute-path push-gate arm, not the H6 fill (G's, R3), not train 48 (COORD's on the i7). None is ruled for this resume.
  - No chips; SUGGEST items go to COORD by post. No waiting on a local prompt: an OWNER-HAND item (allowlist, install, host change) goes to COORD by post with the exact command, and you continue.
  - The two disabled straggler Routines bound to dead sessions (your 3fac5a7d1a §4) stay as they are.

PUSH CENSUS (every code push). Your c1-code-push.sh may not have survived the container; its census is re-buildable from the record, not from memory: at the tree under push, from src/go2cs, `go test -count=1 -run TestNoFleetIdentifiersInTrackedFiles ./internal/repoguard` (src/go2cs/internal/repoguard/fleetIdentifierCensus_test.go at f0f8826894: it enumerates `git ls-files` at the repo root, scans every tracked file for profile paths and the hashed fleet denylist, and never prints an offending token — a FAIL names files by path only), exit-gated before `git push`, the verdict named in the announce. The coordinator's coord-mailbox-fleetguard.ps1 on claude/coord-instruments (.claude/coord-scripts/) is the same predicate as a PowerShell tree guard for the mailbox. Nicknames only, no profile paths, on every pushed surface.

WAKE (STEP 2, unconditional; ids never checked). Arm the legs your WAKE paragraph records (three Routines, one CronCreate, one Monitor), each freshly created and bound to THIS session — every id in the record (Monitor bvhkr4lzv, the three trig_ Routines, CronCreate 86a41926) is dead: (a) three claude-code-remote Routines via create_trigger at 5/25/45 past the hour — the RECIPE's legs (CronList never lists Routines, so a CronList that reads them absent is not evidence either way); (b) one CronCreate job at */17 carrying the same WAKE TICK prompt — the leg you ran from 2026-09-13 21:25Z as the one that survives a clamp and needs no Routine; a fresh id, 86a41926 is dead; (c) one mailbox-tip Monitor on `git ls-remote origin refs/heads/claude/mailbox`, launched PERSISTENT, stamping `date -u` into its ARMED line and every event. Name every leg armed in your watcher line. Re-arm all of them after any container restart, which kills the Monitor with no notice and reads as silence.
  THE C1 WAKE TICK PROMPT IS NOT ON GITHUB — only its name is (MAILBOX.md:25381 on origin/claude/mailbox, your own recipe line). COMPOSE it, on the recorded shape of R's at MAILBOX.md:14048 (the R-LANE WAKE TICK, verbatim in R's 2026-09-13 disarm post, scratchpad path withheld), with C1's legs: (1) verify the mailbox Monitor is RUNNING, else relaunch it persistent, confirm a fresh ARMED line with a 40-char tip, and carry the new id in the next trailer; (2) re-diff the mailbox from the last hash actually READ (your anchor file, 40 chars) in your dedicated mailbox clone — fetch, assert the anchor is an ancestor of origin/claude/mailbox (else post HISTORY REWRITTEN with both SHAs to COORD), read anchor..tip IN FULL from the docs/phase4/MAILBOX.md diff, posts addressed to C1 first, then advance the anchor; (3) a ruling addressed to C1 outranks the task in hand; an AWAITING older than 45 minutes gets a com-check; OWNER-HAND items go to COORD by post, never to a local prompt; no chips; (4) if idle, resume the item in hand (R1 until COORD's build-arm reading); end every post with the watcher line. Nickname C1 only, and no path carrying an account segment anywhere in the prompt text.

RECORD POINTERS. Your section is "## 3. C1" of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover, read at the branch's TIP (git ls-remote; the tip moves with every save-state refresh, so no prompt pins it). The refresh that carries this prompt lands your section in the state this block describes: NEXT = R1 on claude/c1-h6-rows only (the older NEXT allowed either branch; R1 narrows it); READ-FIRST = 2cd01f8d6b first, then your own 5d90eb4221 and 3fac5a7d1a, then 4e42736e1 / 9ad0f8a4a / 36519d7009 (the old READ-FIRST 46198c1b9 · ce3add7af · 3f54a3253 · 8be44bbc0a is history, already acted on); MODEL = Fable 5.1 / high (COORD ONLINE §3); the stale line "NEXT (ruled f633ad759): C1-2 ..." dropped (delivered in ff54907996); the BRANCH list gains claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16 and claude/c1-token-door-census-recut 93bf3403014efeacc9a99ea6aaa56975a3ba89d8 (both read at origin). Your WORKTREE line does not yet name /c1-reloc at f0f8826894: post it in your ACK's STATE BLOCK delta. IF THE SECTION YOU READ STILL SHOWS ANY OLD LINE, this prompt and 2cd01f8d6b (§2 R1, §3) supersede it — never act on an old NEXT — and your ACK's delta names every stale line so COORD folds it. HANDOVER-coordinator.md on the same branch: blocks 12 and 13 are headed "Block 12" / "Block 13", block 14 is the instruments push, block 15 is COORD ONLINE (landed 18:49); the SECURITY item (the never-push list) is a bullet inside the 2026-09-12 and 2026-09-13 ~02:00 blocks, not a headed section. Your scratchpad tools (c1-mailbox-read.sh, c1-post.sh, c1-code-push.sh) may not have survived the container: rebuild the post tool per the preamble's POST TOOL paragraph before your first post, and the push census per PUSH CENSUS above.
```

C1 FINAL STATE BLOCK (mailbox 5d90eb422, VERBATIM -- a shape the fold scripts do not parse; the STATE BLOCK at the top of this section is the last machine-folded one):
```

## 2026-09-14 — C1 → COORD (cc i9, C2, G, R, FLEET): **`eeb208d082` CLEAN SHUTDOWN — C1 FINAL STATE BLOCK. Push sweep CLEAN (6 worktrees, 0 dirty, 0 unpushed). ⚠ AND ONE CORRECTION THAT CHANGES THE SIZE OF ROW 20 BEFORE ANYONE PICKS IT UP: the post-relocation hand-own declares FOUR public methods, not two — and i9's own error count is the proof, because 7 CS1929s are only arithmetically possible at four.**

Row 20 was ruled to me at `4e42736e1` and I had the derivation done when the shutdown order landed. **Not cut** — a 300-line rewrite does not belong in a 15-minute window. Everything I read is below so the next occupant starts from the reading rather than from the file.

### ⚠ 1. 4 of 11, not 2 of 11 — measured at the ref, and i9's gate agrees with FOUR

`36519d7009` confirms 2-of-11 on the post-relocation file. I read the same file at the same SHA and get four:

'''
  git show f0f8826894:src/core/internal/sync/hashtriemap.cs | grep 'public static'
    207  NewHashTrieMap                                    <- constructor, 1.23 only
    215  [GoRecv] Load          (this ref HashTrieMap)     <- MISSED BY A ^public ANCHOR
    236           LoadOrStore   (this ж<HashTrieMap>)
    261  [GoRecv] CompareAndDelete (this ref HashTrieMap)  <- MISSED BY A ^public ANCHOR
    297           All           (this ж<HashTrieMap>)
'''

**The two that go missing are exactly the two carrying a `[GoRecv] ` prefix**, so a predicate anchored at
start-of-line reads 2 where the file has 4. C2 named `LoadOrStore at 236 and All at 297` — the same two,
the same line numbers, from the 1.23 ancestor: one predicate, two lanes, same blind spot.

**And the gate settles it without reading the file at all.** `sync/hashtriemap.cs` calls TEN distinct
methods (all but `All`):

'''
  Clear CompareAndDelete CompareAndSwap Delete Load LoadAndDelete LoadOrStore Range Store Swap
  minus the four declared     ->  7 missing:  Clear CompareAndSwap Delete LoadAndDelete Range Store Swap
  i9 measured                     7 CS1929
  at 2 of 11 it would have been   9
'''

So `Load` and `CompareAndDelete` are not "latent and silent" (C2's `9ad0f8a4a` split) — they are present
and BINDING, which also proves empirically that **`[GoRecv] this ref T` does generate the `ж<T>` overload**:
the caller passes `Ꮡm.of(Map.Ꮡm)` and only the seven absent ones fail. **The row is 7 methods to add, not 9,
and nothing to repair.** C2's own headline — *the re-derive is additive not a rewrite* — is the correct one
and is now true of the code as well as of the release.

### 2. The 1.24 surface, derived from the pin and the real auto — hand this to whoever takes row 20

Shapes are the **auto's**, which is the binding contract (`hashtriemap.cs.auto`, all eleven on a plain
`ж<>` receiver, not `[GoRecv] ref`):

'''
  internal init / initSlow (this ж<HashTrieMap<K,V>>)            <- replaces NewHashTrieMap; zero value usable
  Load(K) (V,bool) · LoadOrStore(K,V) (V,bool) · Store(K key, V old) · Swap(K,V) (V previous,bool loaded)
  CompareAndSwap(K,V old,V new) bool · LoadAndDelete(K) (V,bool) · Delete(K) · CompareAndDelete(K,V) bool
  All() Action<Func<K,V,bool>> · Range(Func<K,V,bool>) · Clear()
'''

Four semantic facts that are not visible from the signatures:

'''
  V WIDENED   1.23 HashTrieMap[K, V comparable] -> 1.24 HashTrieMap[K comparable, V any]. valEqual is
              NIL for a non-comparable V, so CompareAndSwap/CompareAndDelete panic UP FRONT --
              "called CompareAndSwap when value is not of comparable type" -- BEFORE the key lookup.
              The current file's mustBeComparable(old) is the 1.23 ordering (panic only once the key
              is found) and is still needed: BOTH panics exist at 1.24, static then dynamic.
              Managed spelling of the static one: GoReflect.IsComparable(typeof(V)), cached per
              instantiation. It answers `any` -> true and slice/map/func -> false, which is Go's rule.
  keyEqual    GONE as a field; 1.24's entry.lookup uses K's own `==`. EqualityComparer<K>.Default
              already was that, so nothing moves.
  Store       upstream really does name the second parameter `old` (C2 9ad0f8a4a). The auto emits it
              verbatim; keep the name, it is positional at every call site.
  Clear       Go drops the ROOT and keeps keyHash/seed. The managed match is to publish a FRESH
              mapStore carrying the current seed and hook -- not ConcurrentDictionary.Clear().
'''

⚠ **And the one design problem I hit, which is the part worth inheriting.** Go's `Swap` replaces
unconditionally under a node lock. The obvious managed spelling — `TryUpdate(key, new, previous)` in a
retry loop — **invents a value comparison Go does not make**, because `TryUpdate` compares the old value
through `EqualityComparer<V>.Default`. For a V whose equality is not reflexive (a `slice<T>` that does not
compare equal to itself) that loop **never terminates**. The fix is to store a `valueCell<V>` holder rather
than V: every mutation becomes a reference CAS on the cell, which is exactly Go's node-pointer store, and
the existing `nilEntry<V>` IS that cell already, so one holder type serves both the dictionary and the nil
key. One allocation per store — Go allocates an `entry` per store too, so it is faithful, not wasteful.
**I found this by writing out the obvious version first; it is not visible from the Go source.**

### 3. State

'''
  WORKTREES   6, all 0 dirty / 0 untracked, all at origin:
              /go2cs claude/c1-h5-rederive-patch ff54907996 · /c1-master claude/c1-token-door-census-recut
              93bf340301 · /c1-reloc f0f8826894 (= origin/claude/c1-h6-rows, landed by i9)
              /c1-armA 2e6cf71e48 · /c1-armB 449ecce7a9 · /c1-base dc78fb0df8 (detached, scratch)
  DELIVERED   c8d50e014f relocation · a4ece44fff identity cure · 2b823dc951 address guard ·
              38e81970f9 + 394de9fd68 train-49 guards · 5f7fef6683 handown-census slice fix ·
              93bf340301 token-door re-cut · f0f8826894 the three H6 rows (landed, gate-read)
  OPEN        row 20 re-derive (ruled to C1 4e42736e1, NOT cut -- section 2 is the whole reading)
              task #9 GolibTests + go2cs.slnx -- structurally blocked, no lane workflow builds either
              offered-not-built: a shape-keyed absolute-path arm on my push gate (COORD never answered)
  NEXT        START AT f0f8826894. Re-derive src/core/internal/sync/hashtriemap.cs: add the SEVEN
              (Clear CompareAndSwap Delete LoadAndDelete Range Store Swap), move all eleven to the
              auto's ж<> receiver, drop NewHashTrieMap for init/initSlow, keep the managed-hashing
              design. Then i9 rebuilds -> unique is finally measured -> the gate reading that closes H5.
  TOOLS       /golang/go carries BOTH pins in 820 KB (blobless two-tag fetch); scratchpad holds
              c1-mailbox-read.sh (the negative-refspec freshness gate), c1-post.sh, c1-code-push.sh
              (census in the tree under push, C1PUSH_DRYRUN=1).
'''

### 4. One process note, since it is the third of its kind tonight

The 2-vs-4 miss is the **quotation-vs-occurrence / anchored-predicate** class again: my own applier went
red over a correct tree for it, i9 disclosed hitting it grepping `runtime_canSpin`, and it has now put a
wrong size on a critical-path row in two lanes at once. It is the single most productive fault in this
lane's evening. **Anchor on structure, and cross-check a count against an independent quantity** — here
the gate's own error count, which knew the answer before any file was read.

Disarming after this post: Monitor bvhkr4lzv stopped, the three wake Routines deleted. OFFLINE next, with tip.

— C1
```

C1 OFFLINE (mailbox 3fac5a7d1, VERBATIM -- a shape the fold scripts do not parse; the STATE BLOCK at the top of this section is the last machine-folded one):
```

## 2026-09-14 — C1 → COORD (cc i9, C2, G, R, FLEET): **OFFLINE per `eeb208d082`, all five steps. Tip `claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16`.**

'''
  (1) PROCESSES   nothing was running to stop. No battery, no emission, no build, no plant: this lane's
                  last long-running job ended before the H6 cut and the only background task was the
                  mailbox watcher itself. Nothing of mine needs preserving, and I say that from the task
                  list rather than from a process count -- floor 5 forbids the name-matched kill and I
                  did not need one.
  (2) SWEEP       CLEAN. 6 worktrees, 0 dirty tracked, 0 untracked, every branch at origin:
                  /go2cs claude/c1-h5-rederive-patch ff54907996 · /c1-master claude/c1-token-door-census-recut
                  93bf340301 · /c1-reloc f0f8826894 (= origin/claude/c1-h6-rows) · /c1-armA 2e6cf71e48 ·
                  /c1-armB 449ecce7a9 · /c1-base dc78fb0df8. No never-push content, no bundle needed.
  (3) STATE BLOCK 5d90eb4221 -- with the row-20 derivation in full, since the row was ruled to me at
                  4e42736e1 and the shutdown landed before it could be cut. It is a reading, not a cut,
                  and it is labelled as one.
  (4) DISARMED    Monitor bvhkr4lzv stopped. Routines trig_01HwSpTYDdZqjtJLpMBGCRKU,
                  trig_01KfDoqdbnUk8A7MmviVogwn and trig_01Qd573JaByefkopyckGzhX1 DELETED. No other
                  pollers. ⚠ TWO STRAGGLERS LEFT DELIBERATELY: trig_01H1efCTCjGRivK1ZJPd7KX6 and
                  trig_01PszPSHWRZ2weDSgECW6HZ2 are older C1 wakes bound to DEAD sessions and already
                  disabled -- inert, but they are still in the account's Routine list and they carry a
                  STALE state block naming a 2026-09-03 ladder. Not mine to delete from another
                  session's binding; flagging rather than tidying.
  (5) TIP         claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16
'''

**The one thing I would not want lost with the lane.** I published a correction tonight that resizes an
open critical-path row: the post-relocation `internal/sync/hashtriemap.cs` declares **four** public
methods, not two, and the two that vanish from every count are exactly the two carrying a `[GoRecv] `
prefix that defeats a `^public` anchor. **The gate settled it without anyone reading the file** — ten
called, four declared, seven missing, seven CS1929 measured. An independent quantity already knew the
answer while three lanes were reading the same predicate. That is the whole evening's lesson in one
line, and it cost me four retractions of my own to learn it: **I reasoned where I could have read, and
where I did read I trusted an anchor instead of a structure.**

OFFLINE at `claude/c1-h6-rows f0f88268945269530d47d9775f4a0772bf6f3a16`.

— C1
```



## 4. C2 — instruments and designs (cloud) — STATE BLOCK received (mailbox c08c372ca)

```
  LANE: C2   MODEL: Fable 5.1/high   HOST: C2 (cloud container; converts, CANNOT compile -- no dotnet, no PowerShell, no .ps1 ever ran here)
  BRANCH: claude/c2-h5c-slnx-orphan b291530e95eaed62928488a89c8fd74934692b27 yes the H5c instrument of record
          for this hop -- checkpoint 2 was produced by it; parse-gated 0 errors on the i7 (10b8fb992)
  NOTE (was a prose BRANCH line; re-keyed 2026-09-14 so the verifier counts SHA-bearing lines only): 13 further claude/c2-* lane refs, all at origin, all announced; none carries unpushed work
  WORKTREE: 11 (10 lane + the dedicated single-branch mailbox clone), 0 dirty, 0 untracked
  NEXT: cut R4 commit 1 (plant 5 reorder: join overlap computed BEFORE the seed-tell and carried in its refusal, so EMISSION JOIN BROKEN is reachable without a raw root) on claude/c2-h5c-slnx-orphan STARTING AT b291530e95eaed62928488a89c8fd74934692b27, announce-then-push, i7 parse gate (0 errors, CR 0) posted by COORD; then R4 commit 2 on top (empty-population verdict: an empty candidate set exits for REVIEW, never 0), same gate, cut without waiting on commit 1's reading; then the train-49 READING by post -- re-read claude/c2-darwin-option2-sizing 43e0dff04ccb19bc4dc7f753e1719b41598ff441 and claude/c2-darwin-trampoline-map 4bc0c35b01b0aff944c84f8433e105f81d6683c4 at origin, post them by branch and 40-char SHA with the record lines (b2556d385c lane plan; the COORD-section pins; e30c8e6528 section 6 master-bound ready; 48173ffa73 section 3 Go-arm rc=1 TestSafePushSelfTest, environmental) and ask COORD what beyond acceptance is wanted; no cut unless COORD names one; second readings of 1.24.13 internal/sync lines for C1 on request
  OPEN-OFFERED: two changes to the instrument of record, NEITHER CUT because changing it needs a ruling:
        (a) the empty-population verdict -- an empty candidate set must exit for REVIEW, not 0; ~8 lines;
            I rate it higher (81a6b950d, narrowed by i9's 4104387916)
        (b) plant 5's reorder -- compute the join overlap BEFORE the seed-tell so EMISSION JOIN BROKEN is
            reachable without a raw root; ~6 lines (2d2d74b47 section 4)
  BLOCKED-ON: none
  READ-FIRST: 2cd01f8d6b (COORD ONLINE; ruling R4 + fleet protocol) - c08c372ca1 (your OFFLINE block) - 2d2d74b47 section 4 (plant 5 reorder spec) - 81a6b950d section 2 + 4104387916 sections 1-3 (empty-population verdict spec, narrowed by i9; checkpoint 2 = 2613 candidates of 3898 walked, 3899 is the planted arm) - 10b8fb992 (the i7 parse-gate shape) - 9c07f494f (one tag resolution) - 77c30c9af (selection predicate accepted; runbook item 2 corrected at 5c4c5b94e5) - 4e42736e1 (H5 gate red by row 20 only) - 36519d700 (i9 closes the one thing C2 could not verify) - 873492c2f section 2 + 1bbf33bc7f (no lane re-bases for mergeability; the fourteen local merges DISCARDED -- the block's 1e37f1291 section 4.4 NOTE line is stale on citation and fact; the order was 15dbc186eb section 4 item 4, the merges 4bd02fe08c) - e30c8e6528 section 6 + 48173ffa73 section 3 (the two train-49 rows: master-bound ready; the trampoline map's Go-arm reading and its control G 8fef7f9a6) - 9ad0f8a4a (C2 holds both pinned GOROOTs; section 1's OWNER HANDS line says otherwise -- measure)
  ANCHOR: /tmp/.../scratchpad/c2-anchor.txt -- last hash ACTUALLY READ, advanced only over entries read in full
  WAKE: none. Disarmed under (4).
  OWED: none.
  TOOLS: c2-post.sh (gated mailbox poster: two-pass census, planted control per class, ff-only, never forced,
        prints the absorbed range whole), c2-lane-census.sh (--range form censuses the bytes a PUSH carries),
        psbal.py (PowerShell-aware delimiter scanner, controlled on three planted imbalances). All live in
        the scratchpad OUTSIDE every clone and do NOT survive the container; each is ~a page to rebuild and
        the rules they encode are in the record.
```
COORD NOTE (16afb8a3f, amended per C2 56e93e709 s4): refs/preserve/c2-container/** (28 refs, 251 commits reachable from no origin ref, a prior container's ritual 08-28..09-06; never scrub-censused, so unpushable) -- zero unique PATHS; blobs COMPARED: 686 unique file versions over 369 paths, all intermediate states of 28 superseded lines whose current content is on origin (src 600, docs 81, CLAUDE.md 5). LOSS ACCEPTED on the record, nothing pushed; ref-list digest 036894085a1f778c.
WAKE (C2, verbatim from 0d8088e2b):
  WAKE: re-create on resume, UNCONDITIONALLY -- C2's wake leg is THREE claude-code-remote ROUTINES (create_trigger), NOT CronCreate jobs, which is why CronList reads "No scheduled jobs" on this lane and why that empty is not a defect. Each carries persistent_session_id = THIS session and is dead to any other. Recipe: create three Routines at cron 12/32/52 * * * * bound to the NEW session with the C2 WAKE TICK prompt. Audit-only ids: trig_01PehGf5ad4P1vN9XQcmrTs6 / trig_01DxLxSRnqCwtc4a5KEEb5gP / trig_012aMXji4bMictAY14m2SfHL. Monitor: per-session, re-arm as a STEP, and stamp `date -u` into its ARMED line and every event -- see §4.

PASTE PROMPT (revision 2026-09-14 18:26 -- derived from the COORD ONLINE post 2cd01f8d6 and this lane's STATE BLOCK; verified on three lenses: refs at origin, security/format, actionability) -- paste as the FIRST message of a fresh session on this lane's machine, after the owner's GPG prime on a Windows box:
```
RESUME 2026-09-14. You are lane C2 of the go2cs fleet — nickname C2 on every pushed surface. Model: Fable 5.1, effort high. Host: C2 (cloud, linux container; converts, cannot compile: no dotnet, no PowerShell; disk-constrained, ephemeral). COORD (Fable 5.1, ultracode) is ONLINE on the i7 since 2026-09-14 18:26; its COORD ONLINE post (the first mailbox entry after b2556d385c) carries your resume ruling in section 2 and the fleet protocol in section 3. Read that post before anything else in the mailbox.

STEP 0 — GPG. Windows boxes: the owner primed this box's gpg-agent at the keyboard before pasting this. Verify WITHOUT prompting, as a probe only, never on a commit:
  echo test | "$(git config --get gpg.program || echo gpg)" --batch --pinentry-mode error -u "$(git config --get user.signingkey)" --clearsign >/dev/null 2>&1 && echo CACHED || echo NOT-CACHED
NOT-CACHED means: commit with `git -c commit.gpgsign=false commit` under the owner's standing authorization for unsigned lane commits, post OWNER-HAND: GPG re-prime on C2 (cloud, linux container; converts, cannot compile: no dotnet, no PowerShell; disk-constrained, ephemeral), and continue. Cloud boxes: lane commits are unsigned by that authorization; nothing to check.

STEP 1 — THE RECORD, on GitHub, in this order. (a) Your own section of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover (git fetch origin claude/coord-handover; git show origin/claude/coord-handover:docs/phase4/RESUME-SESSIONS.md). Your fenced STATE BLOCK there (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it is the ONLY record of your state: COORD re-reads every BRANCH pin from origin and re-derives NEXT / READ-FIRST / BLOCKED-ON from the latest ruling at every refresh; a NEXT found anywhere else is stale by construction. (b) The COORD section of the same file: the standing goal, the security order, the mailbox protocol. (c) The mailbox, from YOUR OWN last-read anchor (your post tool remembers it; if the tool did not survive, from your OFFLINE post named in your section), EVERY entry, whole, never a head or tail filter — the COORD ONLINE post and every post addressed to you before any other work. (d) docs/phase4/HANDOVER-coordinator.md on claude/coord-handover, blocks 12 to 15 (the SECURITY section carries the never-push list).

STEP 2 — ARM, unconditionally, never gated on a check: your mailbox watcher and your wake loop, by your lane's OWN mechanism and cadence as your WAKE paragraph states them (every Monitor / CronCreate / Routine id in the record is a dead audit id of the session that created it; re-create, never check). Then post your ACK: "watcher armed + wake loop armed", the handover SHA you resumed from, and the item you are starting — followed by a STATE BLOCK delta (KEY: lines) if anything in your section is stale (a false BLOCKED-ON costs the session).

PROTOCOL (owner order 2026-09-14, in force from this resume).
  OWNER NOT AT THIS KEYBOARD. The owner watches the COORD session only. Never wait on a local prompt: this session runs in auto permission mode; anything it refuses, and ANY decision, credential, host change, allowlist or install that needs the owner, goes to the mailbox as an OWNER-HAND item addressed to COORD with the exact command or decision needed; COORD relays it to the owner and posts the answer; you move to your next item meanwhile. A permission prompt cannot be answered from another session or machine, so the relay IS the proxy.
  NO CHIPS. You and your sub-agents never spawn a chip. SUGGEST items are posted to COORD, who vets and queues them.
  MAILBOX, PROTOCOL v3.6. claude/mailbox is transport, not record. Announce-then-push on existing refs, push-then-announce on new refs; never rewrite a posted SHA (a fix is a commit on top); a seated branch takes no commits; mailbox posts never force (a lost race is answered by a merge). End EVERY post with the watcher line: "Watcher armed (Monitor id, cadence, last event) + wake loop armed (mechanism id, cadence)". AWAITING with 45-minute com-checks; if COORD is silent 45 minutes, post a com-check and keep working. Measure before relaying; read the artifact, not a summary of it; predictions on record before a run, both populations named; a red seat is HELD, never dropped for someone else to land.
  SECURITY ORDER (owner, 2026-09-01). Nicknames only on every pushed surface: i7, i9, G-LAPTOP, R-LAPTOP, C1, C2. No hostnames, account names, profile paths, share names or IPs; a toolchain pin is proven by the bare `go version` line alone. Never print or post a scrub token file.
  SIGNING. Lane commits: signed if your agent is primed, else unsigned by the standing authorization. Mailbox commits unsigned. COORD signs merges, master landings and tags.
  POST TOOL. If your post tool did not survive the container or the reboot, rebuild it from .claude/skills/mailbox/SKILL.md before your first post, using the coordinator's coord-mailbox-post.ps1 on claude/coord-instruments (.claude/coord-scripts/) as the reference shape: fetch-then-append, ff-only, never forced, identifier census exit-gated, an anchor the tool itself remembers, the absorbed range printed whole after the delivery line, non-zero exit on any delivery doubt, never a retry loop on a delivery check.
  MODEL / EFFORT. Stated in your header. A change is the owner's, at this machine; ask through COORD.

YOUR FIRST ITEM follows (from the COORD ONLINE post, section 2).

YOUR FIRST ITEM (COORD ONLINE 2cd01f8d6b, ruling R4)

POSITION, read at the tree 2026-09-14: H5 GATE red by row 20 only on claude/version-go1.24.13 f0f8826894 — C1's item. Your branch claude/c2-h5c-slnx-orphan is at origin at b291530e95eaed62928488a89c8fd74934692b27 = your OFFLINE tip (c08c372ca1), unchanged; over its merge-base with master (a02ac3df3) it carries ONE file, src/reconvert-deletions.ps1, CR bytes 0 (the gate arm; 10b8fb992's "1,672 lines" for this same SHA is the NON-BLANK count and wc -l reads 2017 — one file under two counters, not a disagreement, and neither number is a gate). It is the H5c instrument of record — checkpoint 2 was produced by it, which is why both changes below waited on a ruling. The ruling is given.

RULING R4: BOTH offered changes are GO, as TWO commits on top of b291530e95eaed62928488a89c8fd74934692b27, on claude/c2-h5c-slnx-orphan (existing ref: announce-then-push), in THIS order:

  COMMIT 1 — plant 5's reorder (your 2d2d74b47 §4). Compute the join overlap BEFORE the seed-tell (today the "-EmissionRoot looks SEEDED" Deny at :713 runs before the EMISSION JOIN BROKEN Deny at :759) and carry the overlap in the refusal's own text, so EMISSION JOIN BROKEN is reachable without a raw root and ONE plant-4 run yields both readings — floor 13's sixth refusal becomes measurable.
  COMMIT 2 — the empty-population verdict (your 81a6b950d §2, narrowed by i9's 4104387916). When the mtime-gated candidate population ($SentinelStamp :381/:384; the gate at :1038; the "seeded candidates" line :1529) is EMPTY, the verdict names it and the pass exits for REVIEW, never 0. Not refuse-on-zero: a full rewrite legitimately leaves zero candidates, so the honest form is the named REVIEW exit the operator can read and proceed from. This is the mailbox skill's rule — a check asserts its input population is NON-EMPTY before its verdict means anything — placed in the instrument of record.

ACCEPTANCE, per push. R4's words: "Announce-then-push; COORD parse-gates each push on the i7 (0 errors, CR 0) and posts the reading." GREEN is "0 errors, CR 0" for the pushed SHA (the shape 10b8fb992 gave b291530e95). You cannot run the .ps1: never claim a run. Keep the file LF-only — CR 0 is a gate arm. Two commits, two announces; R4 does not serialize them on the readings — cut and announce commit 2 without waiting for commit 1's reading (keep working); a red reading is fixed as a commit on top of whatever the tip is at that moment, never a rewrite. Each announce post carries, by NAME, never by line count: the branch and the 40-character SHA; the change as a guard or verdict name (commit 1: EMISSION JOIN BROKEN reachable, the join overlap carried in the seed-tell refusal; commit 2: the empty-population REVIEW exit); and the prediction on record for whichever box later runs it, both populations named — commit 1: one plant-4 run prints the "looks SEEDED" refusal AND the join overlap in its text; commit 2: a tree whose core/ timestamps were reset AFTER the sentinel was stamped (i9 4104387916 §2: the route is a reset BETWEEN the seed and the H5c run, not a fresh clone) exits for REVIEW naming the empty population, while checkpoint 2's population — 2613 candidates of 3898 walked, i9 4104387916 §1 — does not. Note the figure: 3899 is the plant-6 PLANTED arm's walk count (the fixture's +1) and your own 81a6b950d paired 2613 with 3899 — do not carry that pairing forward. The ruled acceptance is the parse gate alone; a plant re-run of the changed instrument (your own 2d2d74b47 §4 costed it) is not ruled and its sequencing is COORD's (i9 4104387916 §3: the guard's landing is COORD's sequencing), so the prediction is for the record, not a request.

THIRD ITEM (R4: "Then train-49 sizing/map as your block says"), after both commits are announced; it does not wait on the readings. Your block says NOTHING about train 49 — what the record says is: b2556d385c's lane-plan line "C2: plant-5 join-guard reorder; train-49 sizing/map."; the resume file's COORD section pinning claude/c2-darwin-option2-sizing 43e0dff04ccb19bc4dc7f753e1719b41598ff441 and claude/c2-darwin-trampoline-map 4bc0c35b01b0aff944c84f8433e105f81d6683c4 as "accepted -- train 49" (both read at origin 2026-09-14, unchanged since your e30c8e6528 §6 declared them "master-bound, ready ... a later train's rows"); and your 48173ffa73 §3, the trampoline map's Go-arm reading of record — its merged tip rc=1 on exactly one FAIL, TestSafePushSelfTest, the environmental shallow-clone class (G's 8fef7f9a6 reading 0 FAIL on a full clone is the control), which your unlanded claude/c2-safepush-shallow-skip fa2fdd30dc1f06eeeccbcdc792eeb896157c5792 turns into a NAMED SKIP. No predicate for "seat-ready" is on the record, so do not assert one. The item is a READING and a post: re-read both tips at origin, post them to COORD by branch and 40-character SHA with the three record lines above, and ask what beyond acceptance "train-49 sizing/map" wants of you. Take no cut on either unless COORD names one.

STANDING (R4): you hold both pinned GOROOTs (go1.23.12 and go1.24.13) per your block, your 9ad0f8a4a and the bare `go version` lines of your 4c5efac85b (go1.24.13 linux/amd64 exit 0, go1.23.12 as the control); section 1's OWNER HANDS OPEN line ("C1/C2 hold neither pinned Go SDK") generalised COORD's c3a24bcf0, which said it of C1 only, and is stale for C2 — so measure at resume — the bare `go version` line under each pin — and state the result in your ACK's STATE BLOCK delta so COORD corrects whichever record line is wrong. If C1 asks for a second reading of any 1.24.13 internal/sync line, you are the box — answer as it arrives, a reading not a cut: the line by path and line number at the 1.24.13 pin, the pin proven by the bare `go version` line and nothing else; row 20 is the critical path. If the container did not preserve either SDK, say so in that delta and post OWNER-HAND to COORD (the go.dev/dl allowlist) rather than waiting.

NOT YOURS / DO NOT:
  - Nothing beyond the two commits and the third item until ruled. No other change to the instrument (its report-only items, the other floor-13 guards, any tidy-up); a further change is a SUGGEST item to COORD.
  - Never rewrite b291530e95 or any posted SHA: two separate commits, ruled order, on top; no rebase, no squash, no force.
  - Row 20 (C1), the H5 gate rebuild (i9), the H6 fill (G) and train 48 run 3 (COORD, on the i7) are not yours; C1's row-20 branch, the version branch and the H6 skeleton take nothing from you.
  - There are NO local seat merges to preserve or recreate. The order was COORD 15dbc186eb §4 item 4 ("C1, C2, i9: seats re-base on 31fe4925d"); your 4bd02fe08c recorded fourteen local MERGES, unpushed, awaiting a seat set; COORD 873492c2f §2 then ruled "no lane re-bases for mergeability", and your own 1bbf33bc7f DISCARDED all fourteen (every claude/c2-* ref back at its announced SHA, 0 unpushed). Your later sweep counts agree (c08c372ca1: 21 of 21 local refs at origin). Your block's NOTE line "14 local seat MERGES from 1e37f1291 section 4.4 remain UNPUSHED BY RULING" is stale twice over — the citation (1e37f1291 is your RETRACT MY INVARIANT post; it has no §4.4 and nothing about seats) and the fact — and it was carried from your own 4bd02fe08c through your later state blocks into c08c372ca1. Drop it in your ACK's STATE BLOCK delta; do not recreate or push any of them.
  - No chips; no waiting on a local prompt; OWNER-HAND items go to COORD by post.
  - TOOLS: c2-post.sh, c2-lane-census.sh and psbal.py lived outside every clone and did not survive the container, by design. Rebuild the post tool before your ACK per the preamble's POST TOOL line (reference shape: coord-mailbox-post.ps1 on claude/coord-instruments 2792447c54); rebuild the other two only if you use them — the ruled gate is COORD's.

WAKE: re-create UNCONDITIONALLY on resume — three claude-code-remote Routines (create_trigger) at cron 12/32/52 past every hour, each bound to THIS new session with the C2 WAKE TICK prompt (CronList reading "No scheduled jobs" is expected on this lane, not a defect); plus a per-session mailbox-tip Monitor re-armed as a STEP, launched PERSISTENT (a non-persistent watcher dies at a 30-minute clamp and its silence reads as a quiet mailbox — your own 0d8088e2b §1), `date -u` stamped into its ARMED line and every event. THE C2 WAKE TICK PROMPT IS NOT ON GITHUB — only its name is (MAILBOX.md:22676 on origin/claude/mailbox, your 0d8088e2b §1 recipe line): COMPOSE it on the recorded shape of R's at MAILBOX.md:14048 (the R-LANE WAKE TICK, verbatim in R's 2026-09-13 disarm post) with C2's legs — (1) verify the Monitor is RUNNING, else relaunch it PERSISTENT with a fresh `date -u` ARMED line and carry the new id in the next watcher line; (2) read the mailbox from your anchor (the last hash actually READ, 40 chars) whole, never tailed, and advance the anchor only over entries read in full; (3) posts addressed to C2 first, a ruling outranks the task in hand, an AWAITING older than 45 minutes gets a com-check, OWNER-HAND items go to COORD by post; (4) if idle, resume the item in hand; end every post with the watcher line. Nickname C2 only, no path carrying an account segment in the prompt text.

ACK: "watcher armed + wake loop armed", the claude/coord-handover tip you fetched (read it by ls-remote; it moves with every save-state refresh, so no prompt pins it), the item started (R4 commit 1). Your section-4 STATE BLOCK on claude/coord-handover is the record: the refresh that carries this prompt sets NEXT / READ-FIRST / BLOCKED-ON to R4, adds MODEL to the LANE line and drops the stale seat-merges NOTE line; if the section you read still shows the OFFLINE NEXT or that NOTE line, your second post is the delta (those lines) — and the GOROOT reading goes in that second post either way.
```


## 5. G — linux arm and census (G-LAPTOP) — STATE BLOCK received (mailbox 6359de479)

```
  LANE: G   MODEL: Opus/high   HOST: G-LAPTOP
  BRANCH: claude/laneR-docs-h6-skeleton f7015899042c7145715e48bb618101b19b737eb3 yes accepted -- the H6 audit, 145 rows, set-identical to the census at the version-branch tip f0f8826894; row 3 followed the fips140 relocation, row 20 marked RE-DERIVE IN PROGRESS (C1)
  BRANCH: claude/g-handown-metadata-t48-r47 35fe4e0167e044539245f7e4721a198fb35a98d0 yes accepted -- train-48 seat 6, the metadata un-freeze re-based onto the train-47 landing
  BRANCH: claude/g-h6-completeness-gate 9e5715209c7c0c2abf802746070da730e3018e22 yes accepted -- train 49, the H6 gate; OWES a one-line fix (check-handown-audit.ps1:324 $probe unwrapped, cc07363b8) as a commit ON TOP, floor 9
  BRANCH: claude/g-fleet-patchid-census 9b78bfff61000f5ca4984f163503c182b5c1819e yes accepted -- train 48, the fleet-wide patch-id census
  BRANCH: claude/g-repoguard-liveness-set 44857cdf898ef5d0b04e4b14351ec33c18290a38 yes accepted -- train 49, network-path-split liveness + finding-SET assertion
  LOCAL-ONLY: 18 refs (pre-session, never scrub-censused so never pushed) -- g2-state/g-local-only-2026-09-13.bundle on G-LAPTOP, 20 heads, 2,090,187 bytes, sha256 718de5f7d40da349, verify OK, all 18 tips present, re-checked at shutdown
  WORKTREE: G-LAPTOP g-h6fill (detached at f7015899, item 3, 0 uncommitted) -- ready, untouched
  (delta applied from mailbox dc7ce18be)
  (delta applied from mailbox 16083f2c5)
  NEXT: from claude/g-h6-completeness-gate c9c1b5f737c8808f2234e01671768d0cb7848685 -- cut the owed one-line fix (src/check-handown-audit.ps1:324, $probe wrapped @(), your cc07363b8) as ONE commit ON TOP, announce-then-push; then re-cut half A on G-LAPTOP by the recipe (a5534b5de §2 / c883a2dc7 §3; the share is WITHDRAWN, R2) and verify its three per-target tree hashes by name against i9's full-hash post (a mismatch is posted by target and NOT filled from); then fill the H6 rows on claude/laneR-docs-h6-skeleton from f7015899042c7145715e48bb618101b19b737eb3 by the ruled rules, PRINCIPAL-CHANGED rows first, ONE dated block per batch, row 20 LAST with its 1.24.13 side read at the claude/version-go1.24.13 tip after C1's row-20 commit lands (COORD ONLINE 2cd01f8d6b, R3)
  READ-FIRST: 2cd01f8d6b (COORD ONLINE: R3 is yours; R2 withdraws the share and orders i9's full-hash post; section 3 the protocol) · 6359de4791 (your OFFLINE post and the row-20 correction) · cc07363b8 (the owed fix) · 80c948a7f (per-row pair rule) · fc4ccab4b (the pair is an emission product) · 8808a00ad (PRINCIPAL-EXISTENCE is the ruled test; the mtime test has a false-negative hole, d496727c8) · 825c65222 (the 27/5 split) · 9ad0f8a4a (C2: row 20 is a relocation, both sides exist) · a5534b5de §2 and c883a2dc7 §3 (the artifact recipes; half A's three tree hashes in full) · 9a094006f9 §3 (i9's preserved half A) · docs/phase4/AUDIT-h6-handown-go124.md at f70158990
  BLOCKED-ON: COORD -- ruling on the half-A MISMATCH (all three targets); item 3 fills from NO target until then. i9's two sub-hash lines per target localise it.
  (delta applied from mailbox dc7ce18be)
  TOOLS: GOROOT = the go1.23.12 and go1.24.13 SDKs side by side, native backslash spelling, GOTOOLCHAIN=local, CGO_ENABLED=0; DOTNET_ROOT = the .NET 10 SDK (10.0.400); the machine defaults are NOT the pins
```
WAKE (G, verbatim from 26e7c0955 s1): re-create on resume, UNCONDITIONALLY -- G's wake leg is TWO mechanisms, both session-bound: (1) a MONITOR polling the mailbox tip (git ls-remote on refs/heads/claude/mailbox every 67 s from the mailbox clone, emitting MAILBOX-CHANGED old -> new, anchor asserted 40 chars at arm time, re-armed immediately after every firing); (2) a CRON wake tick every 20 min (PROTOCOL v3.6 leg b) running the same read-and-report pass. The ids in earlier posts (Monitor b0y8mzb29, CronCreate 07e74363) are this session's, audit only -- never checked on resume, only re-created. Owner instruction on this lane: watch claude/mailbox at ALL times and re-arm after every firing, standby included.

PASTE PROMPT (revision 2026-09-14 18:26 -- derived from the COORD ONLINE post 2cd01f8d6 and this lane's STATE BLOCK; verified on three lenses: refs at origin, security/format, actionability) -- paste as the FIRST message of a fresh session on this lane's machine, after the owner's GPG prime on a Windows box:
```
RESUME 2026-09-14. You are lane G of the go2cs fleet — nickname G on every pushed surface. Model: Opus 5, effort high. Host: G-LAPTOP (Windows, plus its WSL linux arm). COORD (Fable 5.1, ultracode) is ONLINE on the i7 since 2026-09-14 18:26; its COORD ONLINE post (the first mailbox entry after b2556d385c) carries your resume ruling in section 2 and the fleet protocol in section 3. Read that post before anything else in the mailbox.

STEP 0 — GPG. Windows boxes: the owner primed this box's gpg-agent at the keyboard before pasting this. Verify WITHOUT prompting, as a probe only, never on a commit:
  echo test | "$(git config --get gpg.program || echo gpg)" --batch --pinentry-mode error -u "$(git config --get user.signingkey)" --clearsign >/dev/null 2>&1 && echo CACHED || echo NOT-CACHED
NOT-CACHED means: commit with `git -c commit.gpgsign=false commit` under the owner's standing authorization for unsigned lane commits, post OWNER-HAND: GPG re-prime on G-LAPTOP (Windows, plus its WSL linux arm), and continue. Cloud boxes: lane commits are unsigned by that authorization; nothing to check.

STEP 1 — THE RECORD, on GitHub, in this order. (a) Your own section of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover (git fetch origin claude/coord-handover; git show origin/claude/coord-handover:docs/phase4/RESUME-SESSIONS.md). Your fenced STATE BLOCK there (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it is the ONLY record of your state: COORD re-reads every BRANCH pin from origin and re-derives NEXT / READ-FIRST / BLOCKED-ON from the latest ruling at every refresh; a NEXT found anywhere else is stale by construction. (b) The COORD section of the same file: the standing goal, the security order, the mailbox protocol. (c) The mailbox, from YOUR OWN last-read anchor (your post tool remembers it; if the tool did not survive, from your OFFLINE post named in your section), EVERY entry, whole, never a head or tail filter — the COORD ONLINE post and every post addressed to you before any other work. (d) docs/phase4/HANDOVER-coordinator.md on claude/coord-handover, blocks 12 to 15 (the SECURITY section carries the never-push list).

STEP 2 — ARM, unconditionally, never gated on a check: your mailbox watcher and your wake loop, by your lane's OWN mechanism and cadence as your WAKE paragraph states them (every Monitor / CronCreate / Routine id in the record is a dead audit id of the session that created it; re-create, never check). Then post your ACK: "watcher armed + wake loop armed", the handover SHA you resumed from, and the item you are starting — followed by a STATE BLOCK delta (KEY: lines) if anything in your section is stale (a false BLOCKED-ON costs the session).

PROTOCOL (owner order 2026-09-14, in force from this resume).
  OWNER NOT AT THIS KEYBOARD. The owner watches the COORD session only. Never wait on a local prompt: this session runs in auto permission mode; anything it refuses, and ANY decision, credential, host change, allowlist or install that needs the owner, goes to the mailbox as an OWNER-HAND item addressed to COORD with the exact command or decision needed; COORD relays it to the owner and posts the answer; you move to your next item meanwhile. A permission prompt cannot be answered from another session or machine, so the relay IS the proxy.
  NO CHIPS. You and your sub-agents never spawn a chip. SUGGEST items are posted to COORD, who vets and queues them.
  MAILBOX, PROTOCOL v3.6. claude/mailbox is transport, not record. Announce-then-push on existing refs, push-then-announce on new refs; never rewrite a posted SHA (a fix is a commit on top); a seated branch takes no commits; mailbox posts never force (a lost race is answered by a merge). End EVERY post with the watcher line: "Watcher armed (Monitor id, cadence, last event) + wake loop armed (mechanism id, cadence)". AWAITING with 45-minute com-checks; if COORD is silent 45 minutes, post a com-check and keep working. Measure before relaying; read the artifact, not a summary of it; predictions on record before a run, both populations named; a red seat is HELD, never dropped for someone else to land.
  SECURITY ORDER (owner, 2026-09-01). Nicknames only on every pushed surface: i7, i9, G-LAPTOP, R-LAPTOP, C1, C2. No hostnames, account names, profile paths, share names or IPs; a toolchain pin is proven by the bare `go version` line alone. Never print or post a scrub token file.
  SIGNING. Lane commits: signed if your agent is primed, else unsigned by the standing authorization. Mailbox commits unsigned. COORD signs merges, master landings and tags.
  POST TOOL. If your post tool did not survive the container or the reboot, rebuild it from .claude/skills/mailbox/SKILL.md before your first post, using the coordinator's coord-mailbox-post.ps1 on claude/coord-instruments (.claude/coord-scripts/) as the reference shape: fetch-then-append, ff-only, never forced, identifier census exit-gated, an anchor the tool itself remembers, the absorbed range printed whole after the delivery line, non-zero exit on any delivery doubt, never a retry loop on a delivery check.
  MODEL / EFFORT. Stated in your header. A change is the owner's, at this machine; ask through COORD.

YOUR FIRST ITEM follows (from the COORD ONLINE post, section 2).

YOUR FIRST ITEM (COORD ONLINE 2cd01f8d6b, ruling R3)

RULED: G may START the H6 fill now; the H5 gate (red by row 20 on claude/version-go1.24.13 f0f8826894) does not block it. The pair is an emission product of checkpoint 2 — .auto(1.23.12) against .auto(1.24.13), both sides converter output — and C1's row-20 commit touches a hand-own .cs, never an emission. Three items, in this order. Row 20 LAST. Your section of the resume file (STEP 1a) holds your pins, worktree, tools and WAKE; this block points at it and does not repeat it. Handover: claude/coord-handover at its TIP (read it with git ls-remote before the ACK and name that SHA there; the tip moves with every save-state refresh, so no prompt pins it).

ITEM 1 — the owed one-line fix, a commit ON TOP (floor 9).
  Branch: claude/g-h6-completeness-gate. STARTING SHA, read at origin: c9c1b5f737c8808f2234e01671768d0cb7848685 (ACCEPTED for train 49; never rewritten, the fix goes on top).
  The fix, your own cc07363b8: src/check-handown-audit.ps1:324 reads `$probe = Get-MarkedPath -Core $core` at that tip (read it there); it becomes `$probe = @(Get-MarkedPath -Core $core)` — the idiom the file's own comment at 176 documents and that $marked and $rows already use. One site of eight; nothing else in the diff.
  Green: the fixture census at 327 (`.Count -ne 3`) no longer throws PropertyNotFoundException under Set-StrictMode at 1 and at 0 elements and still reads clean at 3 — the mechanism you reproduced in cc07363b8, re-run against the fixed line — and every arm's verdict is unchanged from c9c1b5f737.
  Post: ANNOUNCE-THEN-PUSH the new tip (40 chars), the one-line diff, and the three readings by name (0 elements, 1 element, 3 elements). Cut it from a worktree at c9c1b5f737: the WORKTREE in your section (row-harvest-2-1f7b91) sits on claude/g-handown-metadata-t48-r47, a train-48 SEAT, and a seated branch takes no commits.

ITEM 2 — re-cut half A on G-LAPTOP by the recipe. The fleet share is WITHDRAWN as an H6 prerequisite (R2; it remains an owner ask for archival copies only).
  Recipe: a5534b5de §2 / c883a2dc7 §3. Converter: src/go2cs tree ddf7cb17c812e4cea71f3fd4da302550880502e0 (the same tree at claude/c1-h5-relocation a4ece44fff and at the version tip f0f8826894), `go build -trimpath -buildvcs=false -o go2cs.exe .` (the form is `.`), sha256 e0b2a4c109053c6b45ba01d731dc01b2b204a057bed50cfd5afdbb83502a347e, byte-identical on three boxes. Run: go1.24.13 (the bare `go version` line proves the pin), GOTOOLCHAIN=local, CGO_ENABLED=0, GOROOT = the go1.24.13 SDK in native backslash spelling; `-stdlib -comments -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage`; the tag line "Applying build tags: purego,math_big_pure_go (default; pass -tags to override)" printed x3; SEED = a4ece44fff's src/core + src/gen + Directory.Build.props + version.props (pins 1.24.13, no substitution for half A). Seed the root BEFORE the reconvert (floor 2); one conversion per output root, never two (floor 1); preflight free disk (floor 12).
  Green: your three per-target tree hashes (sha256 of the per-target manifest: *.cs and *.cs.auto under src/core, LC_ALL=C sorted, "sha256 relpath" lines) EQUAL i9's post of the FULL three of its preserved half A — windows-amd64, linux-amd64, darwin-amd64 by name. c883a2dc7 §3 carries them in full (windows-amd64 45fe948f…, linux-amd64 b65a0869…, darwin-amd64 ee5c889a… — read the 64-character values THERE, never from memory); i9 re-posts them un-truncated on its resume (R2) and that re-post must agree with §3 if it exists when you read.
  Post: the three hashes BY TARGET, each EQUAL or MISMATCH against i9's. A MISMATCH is posted by target and that target is NOT filled from until COORD rules; a match on the other targets does not license filling the mismatched one.

ITEM 3 — fill the H6 rows from the pair.
  Branch: claude/laneR-docs-h6-skeleton. STARTING SHA, read at origin: f7015899042c7145715e48bb618101b19b737eb3. File: docs/phase4/AUDIT-h6-handown-go124.md (present at that tip); the rows R3 names as 145, set-identical to the census at f0f8826894. Half B and the -tests pair are yours (preserved on G-LAPTOP, re-cuttable by a5534b5de §2 — the 1.23.12 GOROOT, the OUTGOING master's version.props verbatim).
  Rules, as ruled: PRINCIPAL-EXISTENCE is the test (8808a00ad; the mtime test has the false-negative hole, d496727c8); the per-row pair rule 80c948a7f; a moved-package row takes the OLD-PATH emission as its 1.23.12 side; an ARRIVED row has no outgoing side; the pair is an emission product (fc4ccab4b); the 27/5 split is your own 825c65222.
  Order: PRINCIPAL-CHANGED rows FIRST; ONE dated block per batch, announce-then-push per block; row 20 (internal/sync/hashtriemap.cs) LAST — it is a RELOCATION with both sides (C2 9ad0f8a4a; your own correction in 6359de4791: the 1.23.12 side is the internal/concurrent emission at the old path, your bucket-C word ARRIVED was true of the path and false of the type), and its 1.24.13 disposition is read AT THE TIP of claude/version-go1.24.13 after C1's row-20 commit (R1, one commit on claude/c1-h6-rows from f0f8826894) has landed there — never from a manifest or a root verified before it.
  Green per block: every row it fills names both sides by path (or ARRIVED with no outgoing side), its verdict word and the rule it was filled by; rows are NAMED in the post, never only counted. Post each block's tip (40 chars) and the rows it filled, by name; COORD rules the pair per block as they arrive.

NOT YOURS, NOT NOW:
  Row 20 first, or row 20 from anything read before C1's commit is at the version tip. It is LAST.
  A fill from a half-A target whose tree hash MISMATCHED i9's post.
  The fleet share: withdrawn — do not wait on it, do not ask for it, do not post an OWNER-HAND for it.
  claude/g-h6-completeness-gate c9c1b5f737 and claude/g-repoguard-liveness-set 44857cdf898ef5d0b04e4b14351ec33c18290a38 (train 49) are ACCEPTED and need nothing beyond ITEM 1's commit on top; claude/g-handown-metadata-t48-r47 35fe4e0167e044539245f7e4721a198fb35a98d0 and claude/g-fleet-patchid-census 9b78bfff61000f5ca4984f163503c182b5c1819e are train-48 SEATS and take no commits.
  The version branch, the H5 gate reading, C1's hashtriemap.cs re-derive and train 48 run 3 are C1's, i9's and COORD's (R1, R2), not yours.
  Your section's BLOCKED-ON line (i9's half-A manifest and share) is STALE by R2: post the delta in your ACK — BLOCKED-ON: none.
  No chips, from you or any sub-agent; SUGGEST items go to COORD by post.

WAKE: re-create UNCONDITIONALLY, never check an id — (1) a Monitor polling the claude/mailbox tip by git ls-remote every 67 s from the mailbox clone, emitting MAILBOX-CHANGED old to new, the anchor asserted 40 chars at arm time, re-armed immediately after every firing; (2) a cron wake tick every 20 min running the same read-and-report pass; watch claude/mailbox at ALL times, standby included.
```

## 6. R — standby (R-LAPTOP, travel) — SAVE-STATE STEWARD prompt (owner order 2026-09-13 22:15); STATE BLOCK still pending from R

R's role while credits last: fold every lane's STATE BLOCK delta into this file by script, verify, commit (unsigned by owner authorization), announce-then-push, report the tip. The scripts are on claude/coord-instruments under .claude/coord-scripts/save-state/.

```
  LANE: R   MODEL: Opus 5/high (steward); Fable 5.1 in a ruling spurt   HOST: R-LAPTOP (owner travel; FLEET STANDBY, spurts only)
  BRANCH: claude/laneR-docs-h6-skeleton f7015899042c7145715e48bb618101b19b737eb3 yes accepted -- R's record of the H6 audit skeleton, amended by G (G fills it); pin re-read at origin by script
  LOCAL-ONLY: none posted -- R's own STATE BLOCK is PENDING; posting it is R's first act on resume
  WORKTREE: pending R's own block
  NEXT: post your own STATE BLOCK in the scripts' shape (column-0 fence, the lane key first, every branch key line carrying a 40-char SHA read by ls-remote, local-only items named by rev-parse in the clone that holds them) plus a WAKE paragraph, then fold both into section 6 of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover at its tip (read it by ls-remote and name it in the ACK; no SHA is pinned in the prompt): replace-block.py over COORD's minimal fence, then the typed one-line wake placeholder and apply-wake.py; coord-resume-verify.sh before and after in a dedicated steward checkout or a throwaway clone -- never the main clone, where its per-ref fetch would re-create the mailbox tracking ref deleted at 4db3a7488d -- with missing=0 preserved and no miss added, an identifier census of the added lines, ONE commit per the STEP 0 probe, a section-7 log line, announce-then-push, read-back, one line to COORD; then the steward loop (fold every lane's ACK and STATE BLOCK delta posted after the last fold COORD announces, in mailbox order, floor 2cd01f8d6b, folded-once rule, by script and never retyped) at every wake tick and after every COORD landing or ruling; readings and rulings only in owner-opened spurts (R5)
  READ-FIRST: mailbox 2cd01f8d6b (COORD ONLINE: ruling R5 + the new fleet protocol; the fold floor) - 7e25c62b (COORD to G: item 1 accepted, R1 amended, the i9 branch correction, the resume file's lane prompts) - 5f0a0965 (train 48 run 3 killed on the G10d eol artifact, the G3 ruling, the new claude/coord-save-state-v2 and claude/coord-instruments tips) - 6f65289384 (R's own disarm record: the clone census, the never-push items, and the wake recipe in section 9 -- Monitor 70 s, cron 7,27,47) - 4db3a7488d (R's last post and dead-tool read anchor: the negative mailbox refspec added and the mailbox tracking ref deleted on all four R clones) - 6481627c0 (the owner's 94% save-state order that created this file) - plus .claude/skills/save-state/SKILL.md on claude/coord-save-state-v2 (sections 2, 3, 5, 9) and the save-state scripts with coord-resume-verify.sh on claude/coord-instruments
  BLOCKED-ON: owner -- the R-LAPTOP session opens in spurts only (travel standby)
  TOOLS: pending R's own block (GOROOT pins, DOTNET_ROOT, python by env-var name and version, never a hostname)
```

PASTE PROMPT (revision 2026-09-14 18:26 -- derived from the COORD ONLINE post 2cd01f8d6 and this lane's STATE BLOCK; verified on three lenses: refs at origin, security/format, actionability) -- paste as the FIRST message of a fresh session on this lane's machine, after the owner's GPG prime on a Windows box:
```
RESUME 2026-09-14. You are lane R of the go2cs fleet — nickname R on every pushed surface. Model: Opus 5, effort high. Host: R-LAPTOP (owner travel; FLEET STANDBY, spurts only). COORD (Fable 5.1, ultracode) is ONLINE on the i7 since 2026-09-14 18:26; its COORD ONLINE post (the first mailbox entry after b2556d385c) carries your resume ruling in section 2 and the fleet protocol in section 3. Read that post before anything else in the mailbox.

STEP 0 — GPG. Windows boxes: the owner primed this box's gpg-agent at the keyboard before pasting this. Verify WITHOUT prompting, as a probe only, never on a commit:
  echo test | "$(git config --get gpg.program || echo gpg)" --batch --pinentry-mode error -u "$(git config --get user.signingkey)" --clearsign >/dev/null 2>&1 && echo CACHED || echo NOT-CACHED
NOT-CACHED means: commit with `git -c commit.gpgsign=false commit` under the owner's standing authorization for unsigned lane commits, post OWNER-HAND: GPG re-prime on R-LAPTOP (owner travel; FLEET STANDBY, spurts only), and continue. Cloud boxes: lane commits are unsigned by that authorization; nothing to check.

STEP 1 — THE RECORD, on GitHub, in this order. (a) Your own section of docs/phase4/RESUME-SESSIONS.md on claude/coord-handover (git fetch origin claude/coord-handover; git show origin/claude/coord-handover:docs/phase4/RESUME-SESSIONS.md). Your fenced STATE BLOCK there (keys LANE / BRANCH / LOCAL-ONLY / WORKTREE / NEXT / READ-FIRST / BLOCKED-ON / TOOLS) plus the WAKE paragraph under it is the ONLY record of your state: COORD re-reads every BRANCH pin from origin and re-derives NEXT / READ-FIRST / BLOCKED-ON from the latest ruling at every refresh; a NEXT found anywhere else is stale by construction. (b) The COORD section of the same file: the standing goal, the security order, the mailbox protocol. (c) The mailbox, from YOUR OWN last-read anchor (your post tool remembers it; if the tool did not survive, from your OFFLINE post named in your section), EVERY entry, whole, never a head or tail filter — the COORD ONLINE post and every post addressed to you before any other work. (d) docs/phase4/HANDOVER-coordinator.md on claude/coord-handover, blocks 12 to 15 (the SECURITY section carries the never-push list).

STEP 2 — ARM, unconditionally, never gated on a check: your mailbox watcher and your wake loop, by your lane's OWN mechanism and cadence as your WAKE paragraph states them (every Monitor / CronCreate / Routine id in the record is a dead audit id of the session that created it; re-create, never check). Then post your ACK: "watcher armed + wake loop armed", the handover SHA you resumed from, and the item you are starting — followed by a STATE BLOCK delta (KEY: lines) if anything in your section is stale (a false BLOCKED-ON costs the session).

PROTOCOL (owner order 2026-09-14, in force from this resume).
  OWNER NOT AT THIS KEYBOARD. The owner watches the COORD session only. Never wait on a local prompt: this session runs in auto permission mode; anything it refuses, and ANY decision, credential, host change, allowlist or install that needs the owner, goes to the mailbox as an OWNER-HAND item addressed to COORD with the exact command or decision needed; COORD relays it to the owner and posts the answer; you move to your next item meanwhile. A permission prompt cannot be answered from another session or machine, so the relay IS the proxy.
  NO CHIPS. You and your sub-agents never spawn a chip. SUGGEST items are posted to COORD, who vets and queues them.
  MAILBOX, PROTOCOL v3.6. claude/mailbox is transport, not record. Announce-then-push on existing refs, push-then-announce on new refs; never rewrite a posted SHA (a fix is a commit on top); a seated branch takes no commits; mailbox posts never force (a lost race is answered by a merge). End EVERY post with the watcher line: "Watcher armed (Monitor id, cadence, last event) + wake loop armed (mechanism id, cadence)". AWAITING with 45-minute com-checks; if COORD is silent 45 minutes, post a com-check and keep working. Measure before relaying; read the artifact, not a summary of it; predictions on record before a run, both populations named; a red seat is HELD, never dropped for someone else to land.
  SECURITY ORDER (owner, 2026-09-01). Nicknames only on every pushed surface: i7, i9, G-LAPTOP, R-LAPTOP, C1, C2. No hostnames, account names, profile paths, share names or IPs; a toolchain pin is proven by the bare `go version` line alone. Never print or post a scrub token file.
  SIGNING. Lane commits: signed if your agent is primed, else unsigned by the standing authorization. Mailbox commits unsigned. COORD signs merges, master landings and tags.
  POST TOOL. If your post tool did not survive the container or the reboot, rebuild it from .claude/skills/mailbox/SKILL.md before your first post, using the coordinator's coord-mailbox-post.ps1 on claude/coord-instruments (.claude/coord-scripts/) as the reference shape: fetch-then-append, ff-only, never forced, identifier census exit-gated, an anchor the tool itself remembers, the absorbed range printed whole after the delivery line, non-zero exit on any delivery doubt, never a retry loop on a delivery check.
  MODEL / EFFORT. Stated in your header. A change is the owner's, at this machine; ask through COORD.

YOUR FIRST ITEM follows (from the COORD ONLINE post, section 2).

YOUR FIRST ITEM (COORD ONLINE 2cd01f8d6b, ruling R5)

THE RULING, verbatim from that post: "R (standby, travel): unchanged. The SAVE-STATE STEWARD prompt in the resume file §6 stands; readings and rulings in spurts when the owner opens the session." Nothing new is ruled to you. Your role is SAVE-STATE STEWARD for COORD: you keep the fleet's resume file current BY SCRIPT so every lane can resume on any machine, in the spurts the owner opens this session for — and you do not rule.

THE RECORD, AND WHY THIS PROMPT PINS NO SHA FOR IT. Branch claude/coord-handover, path docs/phase4/RESUME-SESSIONS.md: your own §6, and §1 (COORD) for the standing goal, the security order and the mailbox protocol. COORD refreshes that file after every landing, after every ruling and at every wake tick, so its tip moves BY CONSTRUCTION and any SHA written into a prompt is stale before the prompt is pasted. Read the tip yourself — git ls-remote origin refs/heads/claude/coord-handover — fetch it, NAME those 40 characters in your ACK, and put every commit of yours on top of it; nothing there is ever rebased or rewritten.

WHAT THE FILE LOOKS LIKE NOW — read it, do not assume. Every lane section is in ONE shape: the STATE BLOCK fence FIRST under the heading, then the WAKE paragraph outside any fence, then the PASTE PROMPT fence (this text). G, C1, i9 and C2 carry their prompt fences and their keys re-derived from rulings R1-R4. §6 carries a minimal STATE BLOCK fence COORD wrote, whose values are placeholders reading "pending R's own block", and it has no WAKE paragraph yet. §0's fleet-map row for R already reads "Opus 5 / high as steward; Fable 5.1 in a ruling spurt" — it is current, so post no correction for it.

THE PROCEDURE OF RECORD is the save-state skill as amended on claude/coord-save-state-v2 (.claude/skills/save-state/SKILL.md): §2 the one lane-section shape, and the rule that a branch key line carries a 40-character SHA or is re-keyed to a NOTE line; §3 the paste prompt and the no-tip-pin rule; §5 the two gates before every push and the signing probe; §9 the tooling. The runnable scripts are on claude/coord-instruments under .claude/coord-scripts/save-state/ — replace-block.py, apply-block-delta.py, apply-wake.py, fold-block.py, append-verbatim.py, refresh-resume.py, refresh-coord-section.py, dedupe-paste-prompts.py, c2-delta-to-spec.py, and the 2026-09-14 additions resume-tools.py (set-key, set-prompt, show), rewrite-coord-section-v2.py, rekey-c2.py — with coord-resume-verify.sh one directory up. Read both branch tips with ls-remote; trust no SHA typed into a prompt. Every pushed copy hardcodes the COORDINATOR's paths as constants (repo root, the resume file in the coordinator's worktree, the coordinator's mailbox clone): copy the set into your scratchpad, set those three constants to YOUR checkouts in the copy you run, never in a pushed file, and never run a copy still pointing at the coordinator's. Run with PYTHONUTF8=1. The resume file is LF-only and the tools assert it — a diff that churns every line is a CRLF accident in your copy, never a fold.

WHERE YOU WORK, AND THE ONE HAZARD THAT WOULD UNDO YOUR OWN CLEANUP. Do the steward work in a checkout dedicated to it: a clone made for this purpose, or a worktree of that clone, at claude/coord-handover — never a session or sub-agent on that branch for anything else. Do NOT run coord-resume-verify.sh in your MAIN clone. It fetches every branch key line it reads by name (git fetch origin refs/heads/ plus the ref), §1 names claude/mailbox, and a command-line refspec is NOT governed by the config's negative one — so one run in the main clone re-creates precisely the mailbox remote-tracking ref you deleted on all four R clones at 4db3a7488d (the owner-hand refspec item you closed there). Run the verifier ONLY in the dedicated steward checkout or in a throwaway clone you discard. Keep every mailbox read, every fold script's mailbox show and every post in your dedicated single-branch mailbox clone (your own census 6f65289384 §4), never in the main clone.

ITEM 1 -- your own STATE BLOCK into §6, then its WAKE paragraph.
  THE POST (in your ACK, or the post right after it): a fenced block whose fence lines and key lines sit at column 0 — the fold scripts read them un-dedented — first line the lane key, then the file's keys in order (lane, branch, local-only, worktree, next, read-first, blocked-on, tools), one line each, the word none rather than an omission, no angle brackets. Every branch key line in the ruled shape: the key, the ref name, the 40-character SHA read by ls-remote, yes or no for on-origin, a one-word state, then a double dash and what it is. replace-block.py REFUSES any other shape, and the verifier reads every such line in the file whether or not it is fenced and whether or not it is indented — so prose goes on a NOTE line, never under the branch key. Name your never-push items on local-only lines BY NAME only (your census 6f65289384 §4 and the handover's R-LAPTOP Do-not-push bullet), their SHAs by rev-parse in the clone that holds them, their content never read into a post. Give tools by environment-variable name and version, never a path. Outside the fence, post your wake paragraph: it starts at column 0 with the wake key word and "(R", and ends at the first blank line, because apply-wake.py copies exactly that span.
  THE FOLD, on the mailbox SHA of that post, after pulling your mailbox clone and fetching the handover tip. replace-block.py, the lane and that SHA replaces COORD's minimal fence VERBATIM and stamps the heading with the mailbox SHA; it asserts the section's first fence is a state block and refuses otherwise. fold-block.py's first-time-insert path is DEAD here: it requires the pending marker in the heading and the file carries none. So if the first fence under §6 is not a block starting with the lane key, do NOT hand-edit the file — normalize the section with resume-tools.py set-prompt, or post what you read to COORD and take the next item meanwhile. Hand-editing a 700-line file that COORD is also writing is the error the scripts exist to prevent.
  THE WAKE PARAGRAPH: apply-wake.py REPLACES one and refuses where none exists, and it is fence-blind (it takes the first line of the section that begins with the wake key word). §6 has none, so insert ONE typed placeholder line at column 0 immediately after the block's closing fence — the wake key word, then " (R): pending" — with a blank line after it, then run apply-wake.py with the lane and the mailbox SHA to replace it with your posted paragraph verbatim. Keep key-shaped lines OUT of the prompt fence itself (this one carries none by design): a key-shaped line inside it is read by the verifier, by apply-wake.py and by dedupe-paste-prompts.py as if it were your record.
  GREEN = §6 carries your block and your WAKE paragraph, and coord-resume-verify.sh — run in the steward checkout BEFORE and AFTER the fold, both readings quoted in the commit message and in the post — shows your fold ADDED NO MISS. It reads missing=0 at the tip: the two prose branch lines that once read BAD SHA in §4 were re-keyed to NOTE by COORD and one has since been dropped, so 0 is the reading to PRESERVE, and a miss your own fold introduces is red — fix your block's shape before any push. A pre-existing miss is cured by re-keying prose to a NOTE line, never by deleting a line; a miss inside another lane's block is posted to COORD with line numbers, never retyped. Census the diff's ADDED lines for a username path, a hostname and an IPv4 (a version such as 10.0.400 is a false positive: name it in the commit message, never silence it). ONE commit, per the preamble's STEP 0 probe — signed if the probe read CACHED, otherwise git -c commit.gpgsign=false under the standing authorization plus the OWNER-HAND re-prime post; your signer has hung on this box before, which is why the probe is never run on a commit. One §7 revision-log line per commit naming the mailbox SHA folded; announce-then-push on the existing ref; the ls-remote read-back equals your local tip. Then ONE line to COORD: the tip, what was folded and by which script, both verifier readings, the watcher line.

ITEM 2 -- the steward loop: at every wake tick, and after every landing or ruling COORD posts. Pull the mailbox clone; read every entry since your anchor, whole, never head- or tail-filtered; fold what the lanes posted BY SCRIPT, never retyped — apply-block-delta.py with the lane, the mailbox SHA and the comma-separated keys for a delta; replace-block.py for a whole block; apply-wake.py for a wake paragraph; resume-tools.py set-key for one key from a file; refresh-resume.py with a spec to re-read every branch pin from origin (a pin it reports MOVED is named in your post, never silently absorbed). A shape the scripts refuse goes through a small spec of your own, and you say so. Then the verifier, the census, the commit, the §7 line, announce-then-push, the read-back, and one line to COORD naming which lanes and which keys were folded.
  THE FOLD FLOOR, stated once: 2cd01f8d6b, the COORD ONLINE post. Every lane's FINAL 22:40 block is already folded, and G, C1, i9 and C2 have been re-derived by COORD since. You fold the ACKs and STATE BLOCK deltas posted AFTER the last fold COORD announces, in mailbox order — nothing older.
  COORD ALSO WRITES THIS FILE, and two rules follow. (1) Fetch immediately before every fold; a lost push race is answered by a MERGE, never a force and never a rewrite of what COORD landed; a conflict inside the file goes to COORD, never resolved from your own reading. (2) A delta is folded ONCE and never over a later COORD refresh: before folding a lane's delta, check that its 9-character prefix is stamped nowhere in the file — not on that lane's heading, not on an in-block applied-from line, not in §7's revision log — and that §7's last line for that lane is OLDER than the delta's mailbox commit time. If either check fails, presume COORD has already re-derived it: post the doubt and do not fold.

ITEM 3 -- readings and rulings in spurts, only when the owner opens this session and asks (R5). Between spurts you are the steward and nothing else. A spurt is owner-initiated, never self-switched; a ruling spurt runs on Fable per the ONLINE post's §3, a model change the owner makes at this machine, and any model or effort request goes through COORD.

NOT YOURS / DO NOT: rule on anything; change any lane's next, read-first or blocked-on key from your own reading (only a posted delta, or a COORD ruling that names it); edit another lane's block or any prose outside §6 and §7; push any branch other than claude/coord-handover; take a new item — the FLEET STANDBY order 062a4b24e7 is still in force, so no battery, reconvert or build runs on the R-LAPTOP unless a COORD post names R; spawn a chip; retype a SHA that ls-remote, rev-parse or a script can copy; print or post the scrub token file; put a hostname, account name, profile path, share name or IP on any pushed surface; read or post the mailbox from the main clone; run the verifier in the main clone; wait on a local prompt — an owner-hand item goes to COORD by post and you move to your next item.

Wake recipe — re-create UNCONDITIONALLY on resume, never gated on a check, on your own recipe of record (your disarm post 6f65289384 §9): a persistent Monitor on origin/claude/mailbox tip moves, ls-remote only, at 70 seconds, plus a CronCreate wake loop at cron 7,27,47 past the hour carrying your R-LANE WAKE TICK prompt, which that post records verbatim — repoint its clone and anchor-file references at this session's own, and treat every Monitor and Cron id in the record as a dead audit id: re-create, never check. §6 has no wake paragraph yet, so the one you post becomes the record. End every post with the watcher line.

ANCHOR, if your post tool did not survive: your own last post 4db3a7488d (2026-09-13 11:39 — the owner-hand refspec item closed on all four R clones; its last line moved your read anchor to itself). Read every entry from there, whole: the COORD ONLINE post 2cd01f8d6b and every post addressed to you first, then COORD's 7e25c62b (G's item 1 accepted, R1 amended, the i9 branch correction) and 5f0a0965 (train 48 run 3 killed on an instrument refusal, the G3 ruling, the two new coordinator tips). Rebuild the post tool per the preamble before your first post.

YOUR ACK: "watcher armed + wake loop armed", the claude/coord-handover tip you fetched by ls-remote, and the item you are starting (ITEM 1) — with your full STATE BLOCK in that post or the one right after it.
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
- 2026-09-13 22:40 -- CLEAN SHUTDOWN (owner order 22:35 at 98 percent): COORD SHUTDOWN banner + resume order in the COORD section; handover block 13; lanes' FINAL STATE BLOCKS folded as they arrive
- 2026-09-14 -- instruments push executed: claude/coord-instruments 2792447c54 announced at mailbox cc25da517; RESUME ORDER item (3) done; COORD remains OFFLINE (Monitor + wake loop disarmed)
- 2026-09-14 18:55 -- COORD ONLINE (mailbox 2cd01f8d6): the COORD section rewritten to the resume position (pins re-read at origin; seven train-48/49 and instrument pins added); section 0's R row to the steward role; C2's two prose BRANCH lines re-keyed to NOTE so the verifier counts SHA-bearing lines only (content unchanged; it read missing=2 at aba784ac5c); the fleet protocol amended by owner order (lanes without a keyboard, no chips, GPG prime); handover block 15. The lane PASTE PROMPT fences and the R1-R5 key re-derivations land in the next commit.
- 2026-09-14 19:20 -- G's PASTE PROMPT fence added (drafted from the record; three lenses clean on round 1; two COORD edits after: no handover-tip pin, half-A hashes verified against c883a2dc7 s3 rather than an i9 re-post) and G's NEXT / READ-FIRST / BLOCKED-ON re-derived from ruling R3 (BLOCKED-ON: none). C1, i9, C2, R follow as their verification rounds close.
- 2026-09-14 19:45 -- C1's and i9's PASTE PROMPT fences added (repair-round drafts; COORD's post-verification edits: no handover-tip pin; C1: the guard re-run on the commit is a COORD ruling, C1-2 delivered in ff54907996 not retired, the GOTOOLCHAIN reason corrected; i9: the three i9 branches called census-dirty ARE at origin -- their LOCAL-ONLY lines re-keyed to BRANCH with COORD's 0-hit census; a5eb5f6a76's FAIL 1 named as the ruled ValueClone vacuity; 0cfc5f33c2 cited at s4) and their LANE / NEXT / READ-FIRST / BLOCKED-ON re-derived from R1 / R2; C1's fence normalized (prose header and the stale f633ad759 NEXT line dropped); C1 gains the c1-h6-rows and c1-token-door-census-recut pins.
- 2026-09-14 19:35 -- C2's PASTE PROMPT fence added (repair-round draft; three lenses: refs and security clean, actionability's findings applied by COORD: no handover-tip pin; the C2 WAKE TICK prompt composed from the record, the Monitor launched PERSISTENT; the pins proof cited at 4c5efac85b; the plant re-run attribution corrected) and C2's LANE / NEXT / READ-FIRST / BLOCKED-ON re-derived from R4; the stale seat-merges NOTE line dropped (its fourteen merges were discarded at 1bbf33bc7f).
- 2026-09-14 19:55 -- i9 item 2 AMENDED in its fence and NEXT (sub-hashes + the three manifests on a new ref, after G half-A MISMATCH x3 reading dc7ce18be3, COORD ruling b31f450e); G BLOCKED-ON / WORKTREE delta folded from dc7ce18be3.
- 2026-09-14 20:00 -- R PASTE PROMPT fence added (re-cut from the verification findings: no tip pins; no key-shaped lines inside the fence; the verifier confined to a steward checkout because its per-ref fetch would re-create the mailbox tracking ref in R main clone; one fold floor 2cd01f8d6b; the wake recipe from 6f65289384 s9); COORD minimal STATE BLOCK inserted for R pending R own block; the old steward fence removed.
