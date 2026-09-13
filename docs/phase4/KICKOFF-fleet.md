# KICKOFF -- go2cs fleet

## 0. Header and maintenance

**As of 2026-09-13, master `ddd509c1e`.** Current and rewritable; never evidence. Detail:
`docs/phase4/CENSUS-preservation-2026-09-12.md`. Until both land, the owner pastes this file plus one block.
- Log: `claude/coord-handover` `9e20af0ad`, by `git show` only. Its "GOTOOLCHAIN=local" clause (lines 36, 142)
  is wrong for battery shells.
- R-LAPTOP holds the drafting session's drafts and census directory off-git at `C:/go2cs-tmp/handover-2026-09-12/`
  (copied after 1e's counts; `MANIFEST.sha256` gives SHA-256 and line counts); the census directory holds pre-scrub
  text: never committed, posted or attached. Nicknames only.
- PROPOSED: land beside a "kickoff" amendment to `docs/Glossary.md:307` (records are only the RECON-,
  REHEARSAL-, CENSUS-, DATA-, STAGE0- files); rewrite section 4 after verified landings; merge master into
  `claude/coord-handover` before any log append.

## 1. Preservation first

**Every lane's first action: run this section on its own machine and report to COORD.** Delete nothing.

### 1a. Census, per clone (main, WSL, mailbox, probe)
Unfiltered: `git fetch --no-prune origin`; assert `git rev-parse --show-toplevel` is the clone; `git stash
list`; `git log --branches --tags --not --remotes=origin --oneline` (prints COMMITS; R-LAPTOP control: 24 lines, 19 on
branches and 5 tag-only, both tags on origin, so check tags with `git ls-remote origin refs/tags/<tag>`); per branch
`for b in $(git for-each-ref refs/heads --format='%(refname:short)'); do [ $(git rev-list --count $b --not
--remotes=origin) -gt 0 ] && echo $b; done` (R-LAPTOP control: 12 branches); `git fsck
--no-reflogs --unreachable`. Per worktree: assert toplevel; `git log --oneline HEAD --not --remotes=origin`;
`git status --porcelain --ignored=matching`. List non-git roots. WSL: set `user.email` first. R-LAPTOP: `git
for-each-ref refs/preserve`. Post counts. `git worktree prune` only on the owning machine: remotely, live
worktrees read "prunable" (G-LAPTOP 14 of 14); remote reads use `--git-dir`/`--work-tree`/`--no-optional-locks`.

### 1b. Rescue a local-only commit (SHARED text; step 6 amended against silent subtraction)
RESCUE A LOCAL-ONLY COMMIT. Nothing runs from the old base, so this works on every base, including bases older than src/safe-push.sh (added 8f849c952, 2026-09-06) or older than src/go2cs/internal/repoguard (e2f9b118f, 2026-09-12). Owner authorization 2026-09-12: lane sessions may commit UNSIGNED on lane branches with `git -c commit.gpgsign=false commit`. Run each step as its own command; capture its exit code before any pipe; its exit gates the next step; never chain a census to a push.

0. SETUP, in the clone that holds the ref. Run `git fetch --no-prune origin`; --no-prune overrides fetch.prune and remote.origin.prune. Check `git rev-parse --verify <ref>^{commit}`. Create a throwaway worktree in a SIBLING directory outside every clone and worktree: `git worktree add --detach <dir> origin/master`. cd into it; assert `git rev-parse --show-toplevel` equals it and `git merge-base --is-ancestor 9355669f8e6d461306837135688cea9fad68f8aa HEAD` succeeds. Steps 1-7 run HERE, where master's safe-push.sh and repoguard always exist.
1. POPULATION. MB=$(git merge-base origin/master <ref>); N=$(git rev-list --count $MB..<ref>). Stop unless N > 0. Print `git log --oneline $MB..<ref>`.
2. DUMP OF THE BRANCH'S OWN RANGE. Write an untracked file at the worktree root, e.g. rescue-dump.txt, containing: the ref name; `git log --no-color --format='commit %H%n%B' -p $MB..<ref>` (every message and every per-commit patch, so a token added and later removed inside the range is still seen); and `git diff --no-color $MB <ref>`. Assert its line count is nonzero.
3. LEG 1a, REPOSITORY GUARD. Run `git add -f rescue-dump.txt` (index only, never committed). Use a shell whose GOROOT is the go1.24.13 sdk as `go env GOROOT` prints it, with its bin first on PATH and GOTOOLCHAIN=local for this call; the converter module refuses older toolchains (measured: go.mod requires go >= 1.24.13). A session that cannot produce a go1.24.13 toolchain (probe `GOTOOLCHAIN=go1.24.13 go version`, never PATH alone) pushes no lane branch or tag, still runs legs 1b and 2, posts its question and tip SHA to COORD on the mailbox, and makes no further commits until COORD rules (its container kept up until then). Run `cd src/go2cs && go test -count=1 -v -run 'TestNoFleetIdentifiersInTrackedFiles|TestFleetIdentifierClearancesAreLive' ./internal/repoguard > <log> 2>&1; rc=$?`. PASS only if rc=0 AND the log contains '--- PASS: TestNoFleetIdentifiersInTrackedFiles' AND contains neither '(cached)' nor '[no tests to run]'. CONTROL: record the dump's sha256; append ONE line holding a profile path whose ACCOUNT SEGMENT IS FOREIGN -- never the environment's own and never a member of the guard's placeholder set -- after first asserting that the bare foreign token alone reads clean (measured 2026-09-13 on G-LAPTOP: an environment whose account segment is a placeholder-set member passes the old control vacuously); re-run, and the guard must FAIL naming rescue-dump.txt; delete that line, re-run, and it must PASS with the sha256 back to the recorded value.
4. LEG 1b, DERIVED-TOKEN PASS. This leg is REQUIRED: the repoguard denylist does not carry every identifier the 2026-09-01 scrub removed (measured 2026-09-12: the guard PASSES at 9355669f8 while two master lines carry one). Tokens are the words that scrub commits a7595da67 (master) and d72878d6d (mailbox) removed at least 3 times and never added (3 tokens, measured). Hold them in a shell variable or a temp file outside every clone; never put them in a worktree, commit, post or document. Lowercase both the tokens and the dump with `tr 'A-Z' 'a-z'` and match with `grep -F -f`. NEVER use `grep -i` in Git Bash: measured 2026-09-12, `grep -i -c -F` printed nothing, a fail-open zero. Print only the hit count and dump line numbers. PASS only on 0. CONTROLS, measured values: the added lines of `git diff <merge-base> 6815eba00` -- the PRE-FIX tip of claude/g-b1-box-design, PINNED: its current tip f632a942b (2026-09-13) removed those very lines, so a control read at the moving tip reads 0 and cannot fire -- read 2; origin/claude/laneR-h5-lastrung reads 0; one planted token line reads 1. Any hit refuses until COORD reads it in a terminal, never on a pushed surface; the shortest token also occurs in ordinary master content.
5. LEG 2, IDENTITY. ALLOW=$(git log --format='%an <%ae>%n%cn <%ce>' origin/master | sort -u | grep -v -e '^root ' -e '<root@' -e 'localdomain>'). Every line of `git log --format='%an <%ae>%n%cn <%ce>' $MB..<ref> | sort -u` must match a whole ALLOW line (`grep -qxF`). Never require equality with the master tip's identity: lane commits legitimately carry 'Claude <noreply@anthropic.com>' (C1), 'i9 <i9@local>' and the C2 lane identities. The filter is needed because master's own history carries one auto-derived root@<HOST> identity (056b2b06c). CONTROLS, measured 2026-09-12: 95bf02ad5 REFUSED, 056b2b06c REFUSED; c5fb9e0ed, 44ab61dad and 31668f43e ADMITTED. A refused identity is never pushed or re-authored in place; its files are re-cut in step 6.
6. CUT (the default). Unstage and delete the dump (`git rm --cached -q rescue-dump.txt`, then remove the file). Run `git switch -c <new lane branch>`; HEAD is origin/master. Bring over ONLY the non-regenerable files, by explicit path: `git checkout <ref> -- <path>` only where `git diff --quiet $MB origin/master -- <path>` succeeds; otherwise write `git diff --no-color --no-ext-diff --binary --src-prefix=a/ --dst-prefix=b/ $MB <ref> -- <path>` to a patch file OUTSIDE the worktree, assert exit 0 and a non-empty file, then `git apply -3 <patch>` (or `git -c commit.gpgsign=false cherry-pick -x $MB..<ref>`, which commits and names the original SHA itself: skip the commit below and take <paths> from `git diff --name-only $MB <ref>`), since a whole-file checkout silently reverts master (1800b04f8 changed typeNameResolution.go after 8a1b7e71c). Before the commit, assert `git diff --cached origin/master -- <paths>` adds and removes the same lines as `git diff $MB <ref> -- <paths>`; it reads the index, which holds a checked-out or `apply -3` import and equals HEAD after a cherry-pick (`git diff origin/master HEAD` compares commits only and stays empty until the commit). A committed lane branch is rebased instead where COORD rules; stage by explicit path, never `git add -A`. Commit with `git -c commit.gpgsign=false commit -F <msgfile>`, naming the original SHA in the message. Repeat steps 2-5 over origin/master..HEAD. Announce the new 40-character SHA on the mailbox. Then, in the same toolchain shell, run `src/safe-push.sh --branch <new> --new --announced <sha>`; its tree gate now covers the rescued content. ONLY ON COORD's RULING (an already-posted SHA that must stay resolvable) push the original ref from this worktree instead, with `src/safe-push.sh --branch <name> --ref <ref> --new --announced <40-char sha>`; its tree gate then scans master, and legs 1a, 1b and 2 are what gated the range.
7. AFTER. `git ls-remote origin refs/heads/<branch>` must equal the pushed SHA. `git status --porcelain | grep '^ D'` must be empty. Remove the throwaway worktree only after both checks. Never force-push; never rewrite a posted SHA. Never push 239f61940, claude/hopa-sweep-r or rescue/joint-measure-45.

### 1c. Do not push
R-LAPTOP security: `claude/mailbox` `239f61940`, `claude/hopa-sweep-r`, `rescue/joint-measure-45`. R-LAPTOP
stale: `claude/laneR-win-signal-exec-arc` (reverts landed `83ea02659`), `claude/f1-flavor-fix`,
`claude/laneR-promotion-pathscope` `23dc6e931`, `claude/laneR-typearg-cache`, `claude/stage2-tfm-prep`,
`laneR-probe-getoradd-closure`, `r-pprof-measure-throwaway`, `r-union`, `claude/reflect-cargo-r1-measure`
(tag `0dfc95e21` first). i7 `3a4f83aa7`. G-LAPTOP `5f0b75f86` (inside `g-seat-preorder-backup`).
**G-LAPTOP pre-scrub, never pushed without an OWN-range census (1b legs 1a, 1b, 2):** `g-regress` `fcd218e27`,
`g-ivt-ab` `b46aae8ea`, `g-ivt-probe` `40a2af690`, `claude/exec-wall-impl` `2147c9daf`,
`claude/gifted-einstein-a338d2` `c4ae4e3c6`. **R-LAPTOP `refs/preserve/g-laptop/*`** (11 verified copies of the
G-only branches; committed work only; not a publication): never pushed (five pre-scrub); delete a copy only after
its branch (seat 8: its re-cut) is on GitHub or COORD retires it.

### 1d. Never prune (origin)
`master`, `claude/mailbox`, `claude/coord-handover`, `release/go1.23`, the train-47 seats,
`claude/laneR-waitreason-47`, `claude/c1-h6-rewrites`, `claude/g-weak-rekey`, `claude/g-l3-testalias`,
`claude/laneR-prepin-baselines`, `claude/g-pprof-baseline`, `claude/g-hop-b-provisioning`,
`claude/g-b1-box-design`, `g-nilfunc-boxing`, `claude/c2-elemindex-probe`, `claude/c2-getaddrinfo-probe`,
`claude/coord-train30-head`, `claude/reflect-cargo-inc1`, `claude/reflect-cargo-r1`. Landed heads (e.g.
`claude/context-diet`) prune on COORD's word. **Local, G-LAPTOP:** no G-only branch (1e) is deleted until G
says whether it is wanted and COORD rules.

### 1e. Off GitHub, at risk (CENSUS record, section 5)
- G-LAPTOP (read-only share scan from R-LAPTOP): 11 G-only branches: `claude/g-generic-alias-qualifier`
  `ffaafeb19` (seat 8), `g-seat-preorder-backup` `a580978fd`, `claude/g-seg3-spike` `7050417a5`, `g-tmp-mergecheck`
  `b2b34ed2e`, `claude/g-mathbits-intrinsics` `8d28c52c8`, `claude/g-structof-embedded-methods` `e57fe22c7`, and
  1c's five. User Temp, archive before any cleanup: `g-repro-221225/repro/`, `g-gqdiff-20260908-225250`
  (conv-*.log, snap/), `g-parse/`, `g-cens/`, `g-lncensus/`, `g-lncensus2/`, `g-lncensus-a/`, `g-i1-probe/`
  (regenerable: `g-runA`, `g-runB`, `g-bfoot`, seeded `g-d*`/`g-e*`/`g-*-conv` roots). `go2cs-g1`: 58 untracked
  files unlisted; 8 diff-arm worktrees unmeasured. H6 scripts `g-h6census.sh`, `g-nstogo.sh`, `g-movedto.sh`.
  **WSL NOT MEASURED, not backed up** (09-08 linux runtime records).
- i7: `.claude/coord-scripts/`; coordinator memory (accumulator 1340-1349); run-8 stdout; `cfd71b0ba`;
  worktrees incl. `sub-orphan-check`; both mailbox clones.
- R-LAPTOP: `C:/go2cs-tmp` (70 loose files, 32 non-git directories); `r-h5b-convert.sh`; two DECISION
  worktrees; WSL clones.
- i9: 21 unpushed commits per i9 `63ec48417`. C1/C2: 0 unpushed at stand-down.

## 2. Roster (owner-approved 2026-09-12; medium only for mechanical legs)

|Session|Host|Model, effort|Status|
|---|---|---|---|
|COORD|i7|Fable 5.1, ultracode (only Fable)|offline since 09-08|
|R|R-LAPTOP, WSL|Opus 5, high|last lane post `a27342d03`|
|G|G-LAPTOP, WSL|Opus 5, high|cut complete at `ffaafeb19` 09-08 22:39, G-only, unannounced|
|C1|cloud; cannot build .NET (no dotnet), but both Go pins resolve: the converter builds, converts and its suites run there (measured 09d16d1d0)|Opus 5, xhigh|UP (owner 2026-09-13); ARMED 66e22a44f|
|C2|cloud; can convert, cannot compile (no dotnet, no PowerShell; measured f3555892d); disk-constrained, ephemeral|Opus 5, high|UP (owner 2026-09-13); ARMED f3555892d|
|i9|i9|Opus 5, high; ONE serial item at a time|offline since 09-08|

## 3. Division of labour

**Design goes to COORD**, with rulings, merges, gates and trains. Lanes execute their section-5 items, post
design questions and lessons to COORD, and move on.

## 4. Current state

**Master `ddd509c1e`** (the security landing on `bd1d26faf`). Since train 46 (`8a1b7e71c`): licensing
`1800b04f8`; the context diet `56ff452a5`..`f34047501`; the kickoff and CENSUS-preservation landing
`bd1d26faf`; `ddd509c1e`. CLAUDE.md is a 196-line index (`TestContextBudget` cap: 200 effective lines);
doctrine in `.claude/rules` and `.claude/skills`; batch19 RETIRED (`86037ef2e`, tag `doctrine-batch19-preserved`).
**Security CLOSED on all three surfaces**, each one commit on top, identifier alone, nothing rewritten:
`claude/mailbox` `c64c289cd`, `claude/g-b1-box-design` `f632a942b` (37 lines; its base predates the 09-01
scrub, so 35 were inherited and a naive merge would reintroduce them), master `ddd509c1e` (two probe
READMEs). The repoguard denylist now carries the hashed Len-15 row, so the guard catches this class; the
whole `internal/repoguard` package reads green at the landing tree, the RED-with-row control on record.
**Train 47, base `ddd509c1e`** -- H4's closing train plus H5's inputs; it precedes the H5 series by
construction. Merge order `1 2 3 4 9 10 7 6 8 5 11 12 13`. Seats board as ruled with the REHEARSAL as the
judge (named conflicts, the silent-subtraction assertion, `go vet` at every merge step, the named guards);
a rebase rewrites a posted SHA and is never done by fiat. Template: the pre-derived train-47 set with the
four carried defects fixed before first use, thirteen rows, G4 RE-INVERTED (CLAUDE.md not in the delta).

|#|Branch @ tip|Class|Ruling|
|---|---|---|---|
|1|`claude/coord-orphan-disclosure-check` `36cbef240`|converter-test|AS-IS, 2/48; owes the utf8 `-tests` arm, which COORD runs as a battery LEG at the assembled head, never as a hand run|
|2|`claude/coord-stamp-guard` `ec1fe2745`|converter-test|AS-IS, 1/48; ARM B stamp guard, R credited in the body|
|3|`claude/laneR-armc-guard` `bbd0afe43`|converter-test|AS-IS, 1/48; ARM C guard|
|4|`claude/c2-sync-disclosure-retire` `4221789e7`|manifest|AS-IS, 1/48; the Windows reading is the battery's own sync leg|
|5|`claude/c2-census-reader` `44ab61dad`|golib|AT ITS TIP (code ends `fb82482ba`; a seat is a branch tip). 12/82, read THREE-DOT at fill; boards last of the code seats|
|6|`claude/g-unfreeze-handown-metadata` `7078dbada`|converter|PENDING RE-CUT: the rehearsal read two adjacent-insert CONFLICTS with licensing 1800b04f8 (internal.godebug.csproj, projectFileWriter.go); G re-cuts onto the base after seat 8, both-kept, acceptance = the godebug csproj re-mints byte-identical + Run A/B/C; boards at the announced SHA|
|7|`claude/g-h6-alias-census` `898cbfefe`|docs|AS-IS, 3/48; the R and C2 blocks it owes ride later trains|
|8|`g-generic-alias-qualifier` `ffaafeb19` (G-LAPTOP only)|converter|PENDING: boards ONLY as G's announced re-cut (one hunk at `typeNameResolution.go:423-425`; `:449-464` is the donor); else the HashTrieMap sites wait for train 48|
|9|`claude/laneR-h5-lastrung` `826045a74`|docs|AS-IS, 1/130 (age, not conflicts); `h5-removals.txt`, the 14-package set|
|10|`claude/coord-pprof-vacuous-audit` `5994c12b2`|docs|NEW; the rescued runtime/pprof vacuous-passes audit|
|11|R's section-15 ladder block|docs|PENDING: boards at the announced SHA, else train 48|
|12|`claude/coord-glossary-kickoff` `ff9d0fb47`|docs|NEW; the `Kickoff` document-type entry, the amendment section 0 proposed|
|13|`claude/g-census-2026-09-13` `748beefbb`|docs|NEW; G-LAPTOP's preservation census record|

**The position on the runbook's section-2 ladder** (derived and posted at mailbox `db6d9462f`): the work in
hand is **H4**, inside the H2->H5 window. H1 and H3 are done; **H2 has NEVER run** -- `src/version.props`
still reads 1.23.12 and the "H2 landed in train 43" shorthand is wrong -- and it is the next UNPASSED gate,
landing as the FIRST commit on the hop's version branch. H4a is RULED a staging BASELINE regen by the same
binary that runs H5 (H0's fresh `.cs.auto`, H6's old side, H5's overlay comparand), never a 1.23.12 landing.
The ladder reads 12/12/12 at `8a1b7e71c` in four owned classes (4 leftover-seed -> H5c; 5 godebug cascades ->
seat 6; 2 HashTrieMap -> seat 8; 1 `Ꮡr` scored against `ce1ee957b`). Four things gate the H5 SERIES: H4
closing for the known sites; R's FIFTH rehearsal, run after train 47 lands; the H4a baseline; then the series
itself (H2's pin, the three-target reconvert, H5c, the overlay, `go generate`, H9, the hand-own branch).
**Open.** The H5 hand-own branch has no owner (R by default; `claude/c1-h6-rewrites` `c5fb9e0ed` never seats).
G's six extra local-only branches await per-row dispositions; R's and i9's ACKs are owed in the measured form;
seat 8's re-cut is owed. Pre-pin gate-2 readings (`7c946ab62`: reflect 326/59/3 of 388 and unique on
`87606f3a5`; runtime/pprof on `150b0264e`; net/http/pprof 11 of 15 and blind runtime 84 of 883, mailbox and
master prose only) still owe their provenance banked. Template defects (a)-(d) carry into the derive.
**Rulings made tonight** (mailbox `3e5951a83`, `db6d9462f`, `90f2dc3ed`, `47f283826`): R-LAPTOP's mailbox
worktree is FROZEN and `r-post.sh` moves to fetch plus `merge --ff-only` plus refuse-on-dirt, never a reset in
a shared clone; H4a is a staging baseline, not a landing; the fetchable guard STANDS unweakened (a census
names unfetchable refs in a pushed record and the post cites the record); leg 1a's control now plants a
FOREIGN account segment (1b step 3); `cfd71b0ba` is rescued and pushed as `claude/coord-pprof-vacuous-audit`.
**Records owed.** The H1.1 amendment and the H4a worked-instance block into `docs/GoCorpusMigration.md`; R's
section-15 dated block on `REHEARSAL-h5-go124.md`; the H6 audit-file SKELETON, one row per marked path in the
census instrument's own predicate (146, not a literal grep's 105); `d7bf606f0`; the Glossary entry with seat 12.
**Owner items.** An off-box copy of the i7 archive; the H5 hand-own branch's owner; C1/C2 restarts; a
conforming-DNS Windows host for `net`.

## 5. Kickoff prompts

**Owner, per session** (worktree OUTSIDE every clone; ancestor CLAUDE.md files load):

```bash
( CLONE='<clone root>'; NEW='<sibling directory>'
  cd "$CLONE" && [ "$(cd "$(git rev-parse --show-toplevel)" && pwd)" = "$(pwd)" ] && git fetch --no-prune origin \
  && git worktree add --detach "$NEW" origin/master && cd "$NEW" \
  && git merge-base --is-ancestor 9355669f8e6d461306837135688cea9fad68f8aa HEAD && echo BASE-OK
  d="$NEW"; while [ "$d" != "$(dirname "$d")" ]; do [ -f "$d/CLAUDE.md" ] && wc -l "$d/CLAUDE.md"; d="$(dirname "$d")"; done )
```
Proceed only on BASE-OK with every listed CLAUDE.md under 1,000 lines.

> **Shared preamble.** You are **<SESSION>** of the go2cs fleet.
> **The owner's words, 2026-09-12:** "I explicitly authorize you, a lane session, to make UNSIGNED commits on
> lane branches with `git -c commit.gpgsign=false commit`. COORD signs merges and landing commits to master.
> Tags are signed. Mailbox commits stay unsigned." Valid only when the owner pastes it.
> **Step 0:** assert `git merge-base --is-ancestor 9355669f8e6d461306837135688cea9fad68f8aa HEAD`, else STOP.
> **First:** run section 1 on your machine; report to COORD. Mailbox fallback anchors (check by ancestry):
> COORD, R, G `827c8d7b00fe3f5934189f7cffd1b5764406736a`; C1 `30eb0316e5974563bf8ba38970390a0325eca585`; C2
> `7e0c20d1c8916e1cc67cfdabd878654d8d446cf8`; i9 `7c18aca21bbdb18cf1d00a37ee68199b643c4526`. Announce before
> pushing; never rewrite a posted SHA; seated branches take no commits; push via 1b; mailbox posts never force
> (safe-push refuses in the mailbox tree: tell COORD).
> **Toolchain (verbatim):** TWO-PIN PAIRING, as master states it (.claude/rules/converter.md:550-556; .claude/rules/harness-gates.md:46, :513, :518-519; docs/GoCorpusMigration.md:313, the fifth arm). Corpus batteries, CNR and the behavioral suite run in ONE shell. GOROOT is the go1.23.12 sdk, spelled exactly as `go env GOROOT` prints it; its bin comes FIRST on PATH; GOTOOLCHAIN stays UNSET (auto). The converter module declares go 1.24.13 and every corpus module declares go 1.23, so Go switches ONLY the converter's own build up to 1.24.13, through the module cache, and loads every corpus package at 1.23.12. converter.md: "`GOTOOLCHAIN` stays UNSET (auto) on the 1.23.12 pin so that switch can happen: at `GOTOOLCHAIN=local` CNR's OWN `go build` of the converter fails `go.mod requires go >= 1.24.13` and throws, which a wrapper then reports as "CHANGED 0"." Assert the pin from a directory with NO go.mod: inside src/go2cs, `go version` reports the switched toolchain. Abort unless the bare `go version` line reports go1.23.12. Verify the built binary with `go version <exe>`. Confirm `go env GOTOOLCHAIN` reads auto; an unset environment variable is not enough. Measured 2026-09-12 on R-LAPTOP: a user-level `go env -w` value (GOTOOLCHAIN=go1.23.1) makes bare `go version` report go1.23.1 even with the go1.23.12 sdk first on PATH. A hand-invoked two-arm instrument may use a SPLIT pin instead. It names one GOROOT on each `go build` (the go1.24.13 sdk) and another on each conversion (the go1.23.12 sdk). It needs no switch and works under either GOTOOLCHAIN setting. The conversion half still reaches a child `go`, so the split pin is sound only while every corpus module's directive stays below the convert pin. The runbook spells the two roots GOROOT_BUILD and GOROOT_CONVERT as prose names; no script, test or tool reads variables of those names. In the converter-pin window, -tests rows build the converter under 1.24.13 and run the pipeline under 1.23.12, and -SkipBuild is MANDATORY for the sweep. The old log's "converter builds under the go1.24.13 sdk with GOTOOLCHAIN=local" (log lines 36 and 142) is wrong as a battery-shell instruction and right only as the build half of a split pin. On a pushed surface, prove a pin by the bare `go version` line alone.

> **COORD (i7): Fable 5.1, ultracode.** (1) SECURITY at the two live tips; rule master's exposure with the
> owner. (2) PRESERVE the i7: process census by executable path; 1a in every worktree and mailbox clone;
> census, then commit or archive `.claude/coord-scripts/`; copy run-8 stdout; export coordinator memory;
> `cfd71b0ba` via 1b or ruled dropped; clear `/tmp/t46-assemble.lock` last. (3) Then: land this file and the
> CENSUS record; section 4's rulings and records owed; re-bases; seat 1's utf8 arm (check `sub-orphan-check`); seats 5, 6, 8; template
> defects with controls; C1's build arm; gate 2; the log block; accumulator 1322 onward.

> **R (R-LAPTOP): Opus 5, high.** First: section 1 on Windows and WSL; keep `refs/preserve/g-laptop/*` (1c); tag `0dfc95e21` locally (push and signing as COORD rules); archive
> `C:/go2cs-tmp` with hashes; locate `r-h5b-convert.sh`. Then: (1) seats `bbd0afe43`, `826045a74`,
> `ec1fe2745`: re-base as COORD rules. (2) After train 47: seeded ladder re-convert, predictions scored as worded.
> (3) H6 block on `898cbfefe`'s file. (4) Offer `87606f3a5`. (5) `eafcacdb7` HELD for H5. (6) At (2)'s re-convert, score whether `Ꮡr` at
> `os/root_openat.cs:123` (the 09-08 22:05 log block's defect) clears under `ce1ee957b`, landed in train 46; only a
> surviving site becomes a cut: minimal repro, announced first.

> **G (G-LAPTOP): Opus 5, high.** First: section 1 INSIDE WSL on every clone, then Windows; `git fetch --no-prune
> origin` before judging anything unpushed (last fetch 2026-09-03); delete no G-only branch until COORD rules;
> archive 1e's Temp items and H6 scripts with hashes. Then: (1) seat 8 is COMMITTED, never announced by SHA:
> `claude/g-generic-alias-qualifier` `ffaafeb19` (`4772d4907` converter, `ffaafeb19` guard and golden; base
> `8a1b7e71c`; 8 files +91/-3; clean). Re-cut it: 1b steps 0-5 on `ffaafeb19`, then step 6's cut
> through the commit only, on a NEW branch name (the local branch stays), by `git -c commit.gpgsign=false cherry-pick -x
> 8a1b7e71c..ffaafeb19` (a rebase in place only if COORD rules; sites now `typeNameResolution.go:423-425`,
> `:449-464`). Then, in the preamble's go1.23.12 battery shell: verify `1800b04f8`'s change and yours in that file BY
> NAME; CNR the branch's own goldens; re-score: guard RED pre-fix (CS0426); footprint ZERO x3; `unique/handle.cs:91-92`
> reads isync; `a27342d03`'s pairings hold. Then finish step 6 in the leg-1a shell: steps 2-5 over the range,
> ANNOUNCE the new SHA, push via `src/safe-push.sh`. (2) Seat 6 as COORD rules. (3) Offer `150b0264e`, `d7bf606f0`.
> (4) WSL is C1's linux arm. Push nothing to `claude/g-b1-box-design`.

> **C1 (cloud): Opus 5, xhigh; if restarted.** You cannot build: COORD's build arm runs golib, runtime and
> GolibTests first. Announced branches: (1) `LockOSThread` as Go's whole body. (2) Token door re-armed with the
> (API, argument) contract table. (3) Erratum block. (4) At H5, re-take `c5fb9e0ed` on a NEW branch.

> **C2 (cloud): Opus 5, high; if restarted.** `grep -c` form: `91e393d62`; retry-loop gap: `7f9e9f71f` (no fix).
> (1) H6 cross-check block on `898cbfefe`'s file. (2) Seats 4, 5 as COORD asks. (3) Increment 13 keeps AFTER
> `83385dad6` and BEFORE `9ecce1839`, blobs equal by hash.

> **i9: Opus 5, high; ONE serial item at a time; after restore.** First: section 1. `job-i9-root2`'s 4 `*Tests.cs` edits
> are very likely superseded by `31668f43e`: byte-compare before discarding. Then, one at a time as COORD confirms: (1) reflect
> census re-run on `claude/c2-census-reader`, ON beside OFF, read by C2's ARMED-ZERO rule. (The `runtime`
> results-file tail is ANSWERED: `0dd133719` on `44f858717`, 185 of 880 verdicts, next door `TestLockOSThreadNesting`,
> root-read by C1 at `18a34299f`; re-run only if COORD asks.) (2) When
> C1's `LockOSThread` seat exists, run it solo through the `runtime` pipeline and report by failure kind. Standing
> prediction, scored by whichever run first executes it: `TestRegisterClass` is unexecuted today; when it runs, the door
> must refuse its argument 0 (`Ꮡwc`) with the identical text, and any other outcome is a hole. Post each reading
> with its tree SHA, configuration and load.
