# CENSUS: preservation of in-progress work, 2026-09-12 (R-LAPTOP)

> **Record type.** Point-in-time CENSUS record: amended only by dated blocks under *Amendments*, never
> rewritten, never executed from (rescue steps: the KICKOFF's rescue procedure).
>
> **Provenance.** "Re-verified" = read again by command while writing (2026-09-12 about 21:20 -0500,
> origin/master 9355669f8); the rest reports that day's census run. Classified at f34047501; 9355669f8
> only adds `src/go2cs/repoRoot_test.go` and a projitems line (re-verified), changing no verdict.
> G-LAPTOP facts: a read-only share scan from R-LAPTOP later that day (nothing changed there).
>
> **No pre-scrub text appears in this file.** Identifiers are described, never quoted.

## 1. Purpose and scope

The owner ruled on 2026-09-12 that no in-progress work may be lost and all sessions start on master; a
checkout move can strand one-disk work, so this census finds it first. It is the **durable home** for
evidence otherwise in one session's scratchpad (its census directory holds pre-scrub text: never
committed, posted or attached).

**Measured from R-LAPTOP** (re-verified): main clone `C:/Projects/go2cs` with all 44 worktrees, main
included; its local branches and stash list; `C:/go2cs-tmp` (70 loose files, 64 directories, 32 with
`.git`); all 97 origin heads (29 not in master, 67 in master, plus master). Also `origin/claude/mailbox`
since 2026-09-01. **G-LAPTOP, partly measured** by the share scan: branches, worktrees, stashes, Temp.

**Not visible from R-LAPTOP:** the i7/coordinator and i9 disks; C1, C2 and other cloud containers;
G-LAPTOP's WSL; R-LAPTOP's WSL clones. Their work appears only if a lane announced its SHA (section 4)
or the 2026-09-08 log relayed it (section 5).

## 2. Method

Git queries that refuse empty populations; agents judged.

**P1. Commits on no origin ref:** `git rev-list --count <ref> --not --remotes=origin` > 0 over local
branches and worktree HEADs; the fetch used `--prune` (conservative); blind to tag-only, reflog-only and
`refs/preserve` commits. Result: 12 local branches (re-verified: same names and tips).

**P2. Real edits, EOL-insensitive:** a modified file counts only if `git diff --ignore-cr-at-eol
--ignore-space-at-eol --quiet` still differs; unstaged only (staged laneR-mailbox: heading comparison,
3c). **P3. Untracked, non-regenerable:** `git ls-files --others --exclude-standard`, grouped by path,
matched against known regenerable sets (reflect `-tests` set, crash dumps), the rest classified by
reading; blind to gitignored files (the refuter's item 9). **P4. Stashes:** `git stash list` read 0
(re-verified; one stash ref serves all worktrees); this empty reading was never made to fail.

**P5. Mailbox announce-before-push cross-check**, the only detector reaching other machines: a posted SHA
resolving to nothing points at an unseen disk. Lines added to `docs/phase4/MAILBOX.md` since 2026-09-01
gave 2,480 tokens (7-12 hex characters, a digit and a letter); dropping any resolving locally left 134,
each classified by reading its post and following the SHA chain. Limits: R-LAPTOP's whole object store
counts (P1 covers local-only commits); all-digit and all-letter prefixes and earlier posts are out of
scope; an unannounced SHA is invisible.

**P6. Scrubbed-identifier sweep.** Pass 1: identifiers the 2026-09-01 scrub replaced, derived (never
printed) from pre- vs post-scrub spellings of the same mailbox headings, matched over what each of the 29
non-master origin branches adds; controls 2 of 2 planted lines caught, a clean line 0. Pass 2,
hash-only: the 3 words scrub commits a7595da67 (master) and d72878d6d (mailbox) removed 3+ times and
never added, lowercased with `tr`, matched with `grep -F` (Git Bash `grep -i -c -F` printed nothing, a
fail-open zero); controls g-b1-box-design's added lines 2, laneR-h5-lastrung 0, a planted line 1. It
also read master's tip (excluded from pass 1): exposed (section 6).

**Classification and refuter.** Each item got one of seven verdicts (key in section 3), an owner, an
action and a confidence. A refuter tried to show what each discard-class verdict (SUP, REGEN, THROW)
would lose and **overturned 4** (`*`): items 9 and 21 THROW to DEC (gitignored raw evidence only in its
worktree; `src/lane-r-packrace.ps1` on no origin ref); items 24 and 38 SUP to SAFE (cited at
DESIGN-token-value-tag-refusal.md:232; `parenthesizeForAccessor` exists only there).

## 3. R-LAPTOP results

Key: SUP = SUPERSEDED_IN_MASTER, SAFE = PRESERVE_ALREADY_SAFE_ON_ORIGIN, H5 = PRESERVE_HOLD_FOR_H5,
REGEN = DISCARD_REGENERABLE, THROW = DISCARD_THROWAWAY, SEC = DO_NOT_PUSH_SECURITY, DEC =
NEEDS_OWNER_OR_COORD_DECISION; `*` = overturned. **16 SUP + 9 SAFE + 1 H5 + 4 REGEN + 3 THROW + 2 SEC +
3 DEC = 38.** Rows 1-11: local branches; 12-21: worktrees; 22-38: origin branches. No row authorizes a
deletion; each removal is its owner's, R's or COORD's call.

| # | Item | Verdict | Action |
|---|---|---|---|
| 1 | claude/f1-flavor-fix beebe4862 | SUP | Never push or merge; code = 3a0191ecf (in master). |
| 2 | claude/hopa-sweep-r ba4f2e187 | SEC | Never push (pre-scrub names in a blob); content in master: 97978a7d0, ef632d1a1. |
| 3 | claude/laneR-promotion-pathscope 23dc6e931 | SUP | Pre-stamp SHA of 48a020155 (via 0747d4acd). Never push. |
| 4 | claude/laneR-typearg-cache fd9a4976e | SUP | Landed as bc1a71642 via 9e9bcbc4f; worktree r-mapcache holds only the reflect set. |
| 5 | claude/laneR-win-signal-exec-arc 5fb3454ed | SUP | Reverts 83ea02659 (in master): a merge silently drops the M.Run flag parse. Never push. |
| 6 | claude/reflect-cargo-r1-measure 0dfc95e21 | SUP | In master (952622591, 409d7e990). Tag before any delete: DESIGN-descriptor-cargo.md:742 cites it. |
| 7 | claude/stage2-tfm-prep 1397bf5fb | SUP | In master via 925e48067. |
| 8 | laneR-probe-getoradd-closure 595aae1e9 | SUP | Production form in master; marked ungated. Never push. |
| 9 | r-pprof-measure-throwaway 873e87a98 (worktree posix-spawn-forkexec-5172e7) | DEC* | Merge of two landed commits; never commit its 2 tracked edits. DECISION: gitignored evidence (7 `r-*.log`; `go2cs_test_*` for runtime/pprof, os/signal, syscall, time). |
| 10 | r-union 3ae9c3798 | THROW | Scratch. Side finding: merge 3552e1cf0 dropped SUB-Q60's GoArrayDimsAttribute.cs paragraph (16d1943ac). |
| 11 | rescue/joint-measure-45 95bf02ad5 | SEC | Auto-derived root@<HOST> identity. Never push. |
| 12 | r-mvfix (dd87fabea): 13 reflectlite edits + reflect set | REGEN | `-tests` on internal/reflectlite and reflect. |
| 13 | laneR-gcount: 7 edits | SUP | Stub proc.cs never banked (fix a34c990c0); httputil regenerable. |
| 14 | r-h5b (44f858717): 4 edits | SUP | = 05353494b and 4dfe1509f. Keep while the ladder uses it. |
| 15 | r-golibwr: 2 edits | SAFE | On origin at eafcacdb7 (held for H5). Do not commit the draft. |
| 16 | r-uniq0: 1 edit + 7 untracked | REGEN | unique `-tests` output and proof page. |
| 17 | handoff-review-f7193e: 12 netip files | REGEN | Stale; master holds the newer c713b94bf. |
| 18 | lane-r-runway: lane-r-shard.txt | THROW | Re-run shardmap.py at e2182a59e if needed. |
| 19 | laneR-net-residual: 76 untracked | SUP | 71 = master ignoring CR, 4 superseded by 5f8005e2a, 1 regenerable `.cs.auto`. |
| 20 | posix-spawn-forkexec-5172e7: uncommitted | REGEN | pprof `-tests` output; the README badge edit is a regression. |
| 21 | preflight-trio-de1c72: 3 untracked | DEC* | DECISION on src/lane-r-packrace.ps1 only (named in 28f00abaa; fix 98937dee0); two std.unicode.utf8 csproj files are debris. |
| 22 | claude/c1-h6-rewrites c5fb9e0ed | H5 | Leave; at H5, C1 re-takes it on a new branch from master. |
| 23 | claude/c1-q12-wip 6774198ca | SUP | Remedy in master as 0ad41a927. |
| 24 | claude/c2-elemindex-probe 9483bc624 | SAFE* | Cited evidence. Never merge. |
| 25 | claude/c2-getaddrinfo-probe 83385dad6 | SAFE | Evidence; increment-13 acceptance vehicle. Never merge. |
| 26 | claude/c2-getaddrinfo-probe-before 9ecce1839 | THROW | BEFORE arm, same four blobs; retire only on COORD's or the owner's word. |
| 27 | claude/c2-pprof-blocker-wording 759453104 | SUP | Premise retired by 16dd3da2c..d19cdd518. |
| 28 | claude/coord-train30-head 9c33b95c0 | DEC | Never rebase or merge. COORD decides BOARD block 6e0130e88. |
| 29 | claude/g-b1-box-design 6815eba00 | SAFE | probes/b1-box-dispatch exists only here; master cites the tip. Section 6. |
| 30 | claude/g-hop-b-provisioning d7bf606f0 | SAFE | Unlanded hop-B record. |
| 31 | claude/g-l3-testalias 1d49a34b6 | SAFE | Records work withdrawn 2026-09-02; ledger line owed. |
| 32 | claude/g-pprof-baseline 150b0264e | SAFE | Unlanded gate-2 baseline; 183-vs-157 unreconciled. |
| 33 | claude/laneR-prepin-baselines 87606f3a5 | SAFE | Unlanded gate-2 baseline. |
| 34 | claude/reflect-cargo-inc1 082a251e9 | SUP | Landed via 6969e672c, b1d8757bc. Not prunable as-is: master cites commits only it holds. |
| 35 | claude/reflect-cargo-r1 08e51b77a | SUP | Landed via 84b28aa18. Not prunable as-is, same reason. |
| 36 | claude/sub-array-range-enumerator 3067aeff5 | SUP | f692235a2 via 65d5975fa. |
| 37 | claude/sub-goroutine-park-reason 9e60eb07b | SUP | a9428e779 via 676ea0fc1. |
| 38 | g-nilfunc-boxing 4b9513773 | SAFE* | Cited at TRACKER-100-percent.md:30. |

posix-spawn-forkexec-5172e7 counts twice (items 9 and 20). 239f61940 is outside the 38 (3c).

### 3b. The regenerable reflect `-tests` set

Nine worktrees carry the same 14 untracked files under src/core/reflect: r-e4cut, r-mapcache, r-master,
r-mvfix, rcargo-d (C:/go2cs-tmp); laneR-next, laneR-predicate, laneR-recordcargo,
linux-seam-ledger-measure-74ade5 (.claude/worktrees): 13 `-tests` host files (`*_test.cs`,
go2cs_test_host.cs, package_test_info.cs) and reflect.tests.csproj. Re-verified in each
(toplevel asserted): 14 files, none elsewhere, one hash of the sorted **name** list (not contents); also `grep.exe.stackdump` crash dumps. Regenerate: `-tests` on reflect (`-go2cspath
<tree>/src`, go1.23.12 GOROOT); after discarding, assert `git status --porcelain | grep '^ D'` is empty.

### 3c. The pre-scrub local mailbox commit

claude/mailbox 239f61940 (worktree laneR-mailbox): 1 commit on no origin ref (re-verified), 2026-09-10
per the enumeration. Against origin: 62 local-only headings, all posted entries in pre-scrub spelling; 0
stranded from the staged state; **0 unsent posts**. Keep worktree and branch. **Never push it.**

## 4. Mailbox cross-check

Of 2,480 candidate tokens, **134** resolved to nothing on R-LAPTOP: **58** SUPERSEDED_BY_LATER_COMMIT
(rebased, amended or re-cut into master or origin); **41** LANDED_UNDER_OTHER_SHA (probe merge,
pre-rebase build or scratch union); **34** NOT_A_COMMIT; **1** STRANDED_ON_OTHER_MACHINE.

**cfd71b0ba**, the stranded one, is claude/sub-q73-pprof-vacuous-audit (docs-only
docs/phase4/AUDIT-runtime-pprof-vacuous-passes.md), cut on the i7 by the SUB-Q73 coordinator sub-agent
("docs-only, not pushed", 2026-09-07); ruling 7c946ab62 names it a home of the gate-2 readings.
Re-verified: unresolvable on R-LAPTOP; the census found no origin head and the file absent on master.
**Owed:** COORD pushes it through the rescue procedure, or rules it dropped.

P5 missed G's generic-alias fix: G posted the site (8bf015332), repro and census (aab3473f6), never a
commit SHA (section 5).

## 5. Off-GitHub at-risk items, per machine

Relayed (2026-09-12 enumeration, 2026-09-08 log) unless measured; owner in brackets.

**G-LAPTOP (measured by a read-only share scan from R-LAPTOP, except where relayed).** Its view is stale:
last fetch 2026-09-03, so a G session runs `git fetch --no-prune origin` before judging anything unpushed.

- **[G] 11 branches exist only on G-LAPTOP** (of 193 local; 182 tips on GitHub). None is deleted until G
  says whether it is wanted and COORD rules. **†** = committed before the 2026-09-01 scrub: never pushed
  without a census of its OWN range (rescue legs 1a, 1b, 2).
  claude/g-generic-alias-qualifier ffaafeb19 (the fix); g-seat-preorder-backup a580978fd (pre-reorder
  backup of seat 7078dbada; contains 5f0b75f86); claude/g-seg3-spike 7050417a5 (golib spike, "not for
  banking"); g-tmp-mergecheck b2b34ed2e (throwaway equivalence merge);
  claude/g-mathbits-intrinsics 8d28c52c8 (withdrawn cut, measured-null record);
  claude/g-structof-embedded-methods e57fe22c7 (reflect.StructOf row 2, ruled off train 8);
  **†** g-regress fcd218e27 (integration merge); **†** g-ivt-ab b46aae8ea and **†** g-ivt-probe
  40a2af690 (probe merges); **†** claude/exec-wall-impl 2147c9daf (os.Args[0] under an apphost);
  **†** claude/gifted-einstein-a338d2 c4ae4e3c6 (golib builtin.cs dead-zone removal).
- **[G] The generic-alias fix is COMPLETE, COMMITTED, never announced:** base 8a1b7e71c, 2026-09-08
  22:39 -0500: 4772d4907 (converter, `typeNameResolution.go`), ffaafeb19 (CollidingPackageNames guard and
  golden); 8 files, +91/-3; worktree clean. Train 47's seat 8 (converter; the 2026-09-08 log's "ninth", counted before batch19 retired); never re-write it from
  memory. G, after preservation: fetch; re-cut onto master by the rescue procedure's CUT step (a rebase only if COORD rules);
  verify both typeNameResolution.go changes by name; CNR its own goldens; re-score the recorded predictions (guard RED pre-fix by CS0426; 1.23.12 footprint zero on all three
  targets; unique/handle.cs:91-92 reads isync at 1.24; a27342d03's nine pairings hold); census its own
  range; ANNOUNCE the new SHA; push through src/safe-push.sh.
- **[G] Worktrees:** 15 (main plus 14 linked), all present; 0 stashes. Clean: main clone (tip
  aa846dc5a, on GitHub), gifted-einstein, row-harvest-2, wsl-su-auth-failure, repro. go2cs-g1 (claude/g-a2-compile-order 289b53a16): 0 tracked changes, 98
  untracked, the first 40 regenerable (runtime and reflect `-tests` output, one .cs.auto); **58 not
  listed**: G reads them before any removal. Detached HEADs off GitHub: 5f0b75f86, ffaafeb19. **Not
  measured:** status in the 8 two-seeded diff-arm worktrees.
- **[G] Temp, archive before any cleanup** (G-LAPTOP's user Temp directory): g-repro-221225/repro/ (the
  hand-written synthetic repro module, emit dirs, logs, patched converter build; not on the branch);
  g-gqdiff-20260908-225250 (`conv-*.log`, snap/: the fix's three-target prediction reading); g-parse/
  (G's Go tool, in no git history); census outputs g-cens/, g-lncensus/, g-lncensus2/, g-lncensus-a/,
  g-i1-probe/. No preservation needed: diff runs g-runA, g-runB, g-bfoot; seeded roots g-d3, g-d3stage,
  g-dctrl, g-dmarker, g-e1, g-e1stage, g-pre-conv, g-master-conv, g-gqdiff's `root-*`; empty g-nilconv,
  g-nilconv2, g-nilconv3, g-backup.
- **[G] WSL: NOT MEASURED, not backed up.** The Linux .NET 10 host's virtual disk (with the linux runtime
  records G preserved 2026-09-08, behind 3a8f3eca3) is unreadable to a share scan. G runs this census
  inside WSL, on every clone, before anything else, and reports to COORD.
- **[G, relayed]** g-h6census.sh, g-nstogo.sh, g-movedto.sh (cited at
  CENSUS-h6-handown-package-aliases.md:65, on no origin ref): archive or commit. Lane memory and Windows
  net qualification records: keep.

**i7 / coordinator** [COORD]: untracked `.claude/coord-scripts/` (post tool, monitor template, train46
land script, train47 template with defects (a)-(d) plus the LEG D arm-2 stamp), censused then committed
or archived; local memory (accumulator 1340-1349, found nowhere else; handoff state; train-47 board; seat
ledger; owner asks); the scratchpad (run-8 stdout, the train-46 LAND_ASM record; run-7; f2/e2; ARM B
files); cfd71b0ba (section 4); worktrees (musing-moser-d4552c, session, coord-handover,
sub-doctrine-batch19, sub-orphan-check, which may hold seat 1's owed utf8 arm, SUB-*), both mailbox
clones, stale `/tmp/t46-assemble.lock`: `git status` and `git log --oneline HEAD --not --remotes=origin`
in each before any removal.

**R-LAPTOP (measured)** [R]

- 70 loose files at the C:/go2cs-tmp root (uncommitted `h6*.py` instruments and outputs, `c1-*.cs`
  copies, hand patches, gate logs, roster-new.md), uncited: archive with hashes; never commit a log
  carrying a profile path. 32 non-git directories (h5b, the DEGRADED ladder tree; rung emissions;
  `_retired-worktree-patches`; four hidden `.dq-*`): hash or archive before any cleanup.
- r-h5b-convert.sh, quoted in 93820a2c5, not found: home it before the ladder retires.
- Items 9 and 21, 0dfc95e21 (tag first), the 12 local-only branches: keep every ref.
- **`refs/preserve/g-laptop/<branch>`, copies of all 11 G-only branches** (re-verified: tips as above,
  none on origin): committed work only (5f0b75f86, ffaafeb19 included), not untracked files, Temp or WSL;
  not a publication or a substitute for G announcing and pushing. **Never push a copy**, above all the
  five † (pre-scrub). Delete one only after its branch is on GitHub or COORD retires it.
- [R, the owner] This session's drafts and census directory: copy off-git with SHA-256 and line counts
  before the session ends; never commit, post or attach the census directory.
- WSL clones go2cs and go2cs-probe (unmeasured): set `user.name`, `user.email` before any commit
  (95bf02ad5 took its identity from an unset clone).

**i9** [i9, after restore]: job-i9-* worktrees with 21 unpushed commits (per 63ec48417), eight with
tracked edits; job-i9-root2's 4 `*Tests.cs` edits very likely superseded by 31668f43e (byte-compare
first). **Cloud** [the owner]: C1 (c0e259709), C2 (7f9e9f71f) 0 unpushed at stand-down; no mailbox post
between a27342d03 and 0ff4b03e1: check the session list for the reported new sessions.

## 6. Security exposure

No identifier value is reproduced; paths are under docs/phase4. The owner ruled on 2026-09-12 that the
first two are **COORD's FIRST item**: a commit **on top** of each tip substituting the identifier alone;
no history rewrite or force.

- **origin/claude/g-b1-box-design 6815eba00**, 2 added lines (DESIGN-zh-box-b1.md:43,
  probes/b1-box-dispatch/README.md:12): commit on top, census the range, announce, push. Never delete the
  branch: master cites the tip.
- **origin/claude/mailbox 0ff4b03e1**, 1 line (MAILBOX.md:79168, heading of a 2026-09-02 G post, added by
  1649b77db before the heads-up, which censused clean): the coordinator's remedy, ONE mailbox commit at
  the tip; lanes never scrub the transport piecemeal.
- **origin/master 9355669f8, NEW, not yet ruled**, 2 lines (probes/TlsHandshakeCost/README.md:7,
  probes/WriteDeadlineBudget/README.md:18), added by f54087b3b, merged in 6eed8dd5c (2026-09-01 20:44)
  after scrub a7595da67; `TestNoFleetIdentifiersInTrackedFiles` passes (denylist lacks the token);
  056b2b06c carries root@<HOST> in history. The owner and COORD rule; proposed: a signed COORD commit plus
  a red-first denylist row, no history rewrite.

The other 27 non-master origin branches read clean. Never pushed: R-LAPTOP's 239f61940, ba4f2e187,
95bf02ad5 (on origin 2026-08-30 to 09-02; GitHub may still serve it, owner item) and its
`refs/preserve/g-laptop` copies. G-LAPTOP's five pre-scrub branches (fcd218e27, b46aae8ea, 40a2af690,
2147c9daf, c4ae4e3c6) are never pushed without an own-range census. G pushes nothing to g-b1-box-design.

## 7. The train-47 compile break

**Cause.** e2f9b118f moved repoRootFromPackageDir (at 44f858717,
src/go2cs/fleetIdentifierCensus_test.go:402) from package main into internal/repoguard. Three seats cut
earlier each add one package-main call (re-verified): coord-orphan-disclosure-check 36cbef240
(platformScopedDisclosures_test.go), coord-stamp-guard ec1fe2745 (valueCloneStampMembers_test.go),
laneR-armc-guard bbd0afe43 (duplicatePartialMembers_test.go). Tests compile per package, so a clean
rebase stops every converter test; no per-branch gate sees it.

**Compile proof** (9355669f8 body; throwaway worktrees at master): master plus coord-stamp-guard's file,
go vet exit 1, undefined: repoRootFromPackageDir at valueCloneStampMembers_test.go:162:10. **ARM A:**
plus the helper and all 9 package-main .go files the seats change, exit 0. **ARM B (control):** without
the helper, exit 1 at duplicatePartialMembers_test.go:456:10. It proves the break, not the textual rebase.

**Fix.** 9355669f8 restores the helper as src/go2cs/repoRoot_test.go (deliberate keep-in-lockstep
duplicate, in go2cs-src.projitems). Gates: vet 0, gofmt clean, repoguard 10/10, TestProjitems 3/3,
TestLicensing 10/10. Re-verified: `%G?` G; defined at repoRoot_test.go:38 and
internal/repoguard/fleetIdentifierCensus_test.go:509. Seats need no change for it; owners name textual
conflicts at re-base. Fixing it now was the owner's decision.

## 8. Limits, and what each other machine must run itself

**Limits.** Section 2's limits stand; P6 skipped master's history and every clone's local refs; the
reflect set was compared by name. Verdicts are judgments (four overturned): only the owner discards,
after re-reading.

**Every machine runs the same predicates, fixed:**

1. `git fetch --no-prune origin` first (overrides `fetch.prune`, `remote.origin.prune`; both unset on
   R-LAPTOP's main clone, re-verified).
2. `cd` into each worktree, assert `git rev-parse --show-toplevel` (`git -C` walks up without `.git`).
3. P1 also over local tags, `git for-each-ref refs/preserve`, a reflog leg, with a known local-only
   branch as positive control; P2 over `--cached`; P3 `--ignored`; mailbox clones for commits ahead.
4. Case-insensitive matching by `tr` plus `grep -F`, never `grep -i`.
5. Rescue only by the rescue procedure from a master worktree (`safe-push.sh` censuses the tracked tree,
   not the pushed range or metadata).
6. `git worktree prune`, or acting on "prunable", only on the owning machine: read remotely, all 14 of
   G-LAPTOP's live linked worktrees show "prunable" (absolute gitdir paths). Remote reads use explicit
   `--git-dir`, `--work-tree`, `--no-optional-locks`.

**Owed per machine:** its section 5 items (G-LAPTOP: WSL first; i7: mailbox clones, section 6; R-LAPTOP
WSL: identity first); cloud: the owner checks the session list.

## Amendments

*(Dated blocks append below; the body is never edited.)*
