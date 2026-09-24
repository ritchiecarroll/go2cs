You are an i7 sub-agent for the go2cs fleet coordinator. This brief has THREE PARTS that run IN SEQUENCE, with a COORD CHECKPOINT after each. Each part is dispatched by its own prompt, and you run only the part your prompt names. **PART A**, THE POST-CLOSE BATCH: three accepted refs (C2's two converter fixes and C1's close-lesson docs) merged onto the H10 close STAMP `fa18863b94` in a LINKED worktree ON A BRANCH, the converter gates, then a full three-target seeded `-stdlib` regen with the fixed converter. The regen proves the close's restore classes (v) (337 README badge restores, R-B13) and (vi) (log/syslog's csproj, R-B15) VANISH, with the corpus footprint zero or every hunk named. Then a report, then STOP. **PART B** is H11 (publication and compatibility guards). **PART C** is H12 (docs, badges, READMEs), including the release-ritual REHEARSAL as a DRY RUN ONLY. Part C ends with a READINESS REPORT and STOPS before any signing, tag mint, version bump, snapshot write, push or publish; COORD CHECKPOINT 3 (COORD's work) then pushes the announcement and freezes the release branch before the owner's run. The owner signs and publishes NuGet 1.24.13.1 personally: signing needs the owner's card PIN at the physical console, and neither COORD nor you ever publishes. **DO NOT PUSH** in any part. Read `CLAUDE.md` first (all 16 floors), then `.claude/rules/harness-gates.md`, `.claude/rules/corpus.md`, `.claude/rules/converter.md`, and `.claude/skills/{corpus-reconvert,merge-hazards,gate-forensics}/SKILL.md`. Then read these sections of `docs/GoCorpusMigration.md` AT YOUR MERGED TIP: H5's seeding and three-target amendments (@fa18863b94 :682-812), the overlay amendment (:1051-1114), the H10 close amendment (:2931-3136, which `<c1-docs-ref>` extends), H11 (:3138), H12 (:3160), §5 (:3579) and §6 (:3597). Every `rb :N` in this brief cites `docs/GoCorpusMigration.md@fa18863b94`. C1's ref adds about 45 lines inside the close amendment, so re-find each cite by its heading at your tip. **THE RUNBOOK LEADS ON PROCEDURE:** where this brief and the runbook disagree, STOP and quote both. Never modify the main checkout `<repo>`, `<coord-tmp>/abclose`, `<coord-tmp>/abclose-stage`, or any other worktree under `<coord-tmp>`. COORD's prompt supplies the absolute value of every `<placeholder>` (`<repo>`, `<coord-tmp>`, `<scratchpad>`, `<goroot>`, `<dotnet10>`, `<gpg-exe>`, `<gpg-key>`). `<tree>` is the part's own worktree: `<coord-tmp>/abpost` in Part A, `<coord-tmp>/abrel` in Part B, `<coord-tmp>/abh12` in Part C. Every `go test` runs with cwd `<tree>/src/go2cs` unless a step says otherwise.

## Why
The H10 close STAMPED `claude/version-go1.24.13` at `fa18863b94` (LEDGER 2026-09-23 22:30). Its regen restored two classes by hand because the converter was wrong: (v), because `seedCensusRoot` seeded only `docs/validation/current`, so the staged badges lost the published stamp (R-B13, 18:11); and (vi), because the multi-target csproj renderer cut from the L3 block to `</Project>` and dropped the shared-LICENSE group (R-B15, 18:39). The fixes were accepted for a PRE-RELEASE batch "landing before H12's release, never in the close" (18:11, 18:23 `781c1c3c31`, 19:01 `ae813db069`). The lessons R-B13..R-B16 were accepted as C1's docs (22:40 `13e9b039ea`). The STAMP's NEXT line orders: post-close batch, then H11, then H12, then the owner signs, and "everything post-hop lands AFTER the release" (22:30; RESUME-SESSIONS STATE DELTA 28).

## Release facts, as the records stand (re-read each at your tip; STOP if one moved)
- **Counter.** At `fa18863b94`, `src/version.props:23-24` reads `GoStdLibVersion 1.24.13` and `GoBuildNumber 0`, so it composes 1.24.13.0, which is unpublished. Master `074a12c4ae` still reads 1.23.12 / 3. The recorded snapshots are 1.23.1.2-.7 and 1.23.12.1-.3, none on base 1.24.13, so `releasestamp.PublishedStamp` resolves 1.23.12.3. The next publish is **1.24.13.1**: `release-nuget.ps1:171` runs `push-nuget -BumpBuild`, which bumps 0 -> 1, mints `nuget-1.24.13.1` pre-pack, and writes the write-once `docs/validation/1.24.13.1/`.
- **Badges at H12 (the UNPUBLISHED LINE, rb :3165-3179).** The two H1-following badges (Docs, Source·Go) already read @1.24.13 on 320 converter-emitted READMEs; 5 READMEs lag (C1). The two H2-following badges (the Tests proof link and the Source·C# tag plus message) do NOT move before the publish: their expected H12 diff is **ZERO**. At the publish, push-nuget retargets 342 Source·C# badges and 214 green Tests links from 1.23.12.3 to 1.24.13.1, and the 21 links that dangle today (their pages are absent from the 1.23.12.3 snapshot) resolve.
- **The release branch: UNRULED (RN-12).** Every earlier release was cut on master. The 1.24.13 corpus exists only on the version branch. Releasing from master before the cutover would publish the OLD corpus as 1.23.12.4 (rb :123-143 forbids it). `release-nuget.ps1:134-136` only WARNS off master. go2cs.net serves `master:/docs`, so 1.24.13.1's proof links 404 until the snapshot reaches master.
- **The fold: UNRULED in timing and mechanics (RN-13).** H7a (master INTO the version branch) is done: `fc275f1ac3`, plus the interop fold `30057d0c4a` and the ruled campaign-tip merge `c6fdbe73c3`. The reverse direction, §5's cutover, happens only when all five parity gates hold (rb :3579-3590), and the runbook gives no mechanics for it. rb :1339 expects master to reach the version branch at H12 for the H6 audit file; `docs/phase4/AUDIT-h6-handown-go124.md` is already blob-equal on both tips, so no H12 fold is owed for it. Master's other 13 commits (`git log fa18863b94..074a12c4ae`; c10: the census script, hashes and patterns, `docs-records.md`, `fleetIdentifierCensus_test.go`) ride the cutover (RN-13), or a pre-release fold if COORD rules one; such a fold would also give the tip master's public-handle admit `dd18e5e2ab`, which C5 needs. P1 (compile) and P3 (behavioral; the base two ruled at H9's closure, rb :2113-2126) were discharged at the close STAMP. P2's arithmetic holds at the close (218 >= 204; shardmap 228 + 2 = 230), but its ruled instrument, the full-roster sweep (PLAN-corpus-upgrade.md:555; §6 rb :3609, owed "at the parity gate"), is OWED. P4 (the H6 audit) and P5 (this brief's rehearsal) are owed. P5 requires the tag mint exercised (rb :3590, :3216-3219), and no pack-only dry run mints a tag (MILESTONE-75pct-prep.md:421). Under RN-7's DEFAULT, P5 is therefore NOT MET before the publish, so the cutover FOLLOWS the publish. It may precede the publish only if COORD rules RN-7's isolated-clone alternative, or records a ruling that the partial rehearsal discharges P5.

## Rules for EVERY part
- **Pins, GPG, census and siblings** (as in the close brief; read each back by OUTPUT):
  - GOROOT: unset the machine-scope GOROOT, a DEAD 1.23.1 pin. Put the go1.24.13 SDK's `bin` first on PATH. `go version` must read go1.24.13, and `go env GOROOT` prints the BACKSLASH spelling `<goroot>`, which is the pin (floor 6). Set GOTOOLCHAIN=local, CGO_ENABLED=0 and an empty GOFLAGS.
  - .NET: the dotnet10 root is DOTNET_ROOT and first on PATH for every `dotnet`.
  - **ONE PARENT SHELL: bash**, with the re-pointed `env.sh` sourced. The launch shapes, each followed by `rc=$?` on the NEXT line:
    - a Windows PowerShell 5.1 script (`reconvert-deletions.ps1`, `migrate-gorelease.ps1` [`#Requires -Version 5.1`], `release-nuget.ps1`, `push-nuget.ps1`, `clean-bin.ps1`): `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <script>)" <args> < /dev/null > <log> 2>&1`. Every path argument goes through `"$(cygpath -w …)"`; a backslash path typed unquoted in bash loses its backslashes. 5.1 is .NET Framework, so the dotnet10 pin does not break it.
    - pwsh 7, only where a leg truly needs it (A6, B6(1), C4(5)): `env -u DOTNET_ROOT -u NUGET_API_KEY -u NuGetCertFingerprint PATH="$ORIG_PATH" pwsh -NoProfile -File "$(cygpath -w <script>)" < /dev/null > <log> 2>&1`. Derive `ORIG_PATH` by REMOVING the go1.24.13 `bin` and dotnet10 directories from PATH, never by capturing the launch shell's PATH. On the i7, pwsh is a .NET 8 dotnet global tool: under the pinned DOTNET_ROOT, or with dotnet10 first on PATH, it exits 150 or a large negative code having run NOTHING (gate-forensics SKILL :1504-1511 at fa18863b94). A script that needs the Go/.NET pins inside pwsh 7 runs through a per-run driver (the close's `cnr-driver.ps1` shape) that sets them with `$env:` and ends `exit $LASTEXITCODE`.
    - A bare `pwsh -NoProfile -File …` typed from the pinned bash is a launch defect, never a reading.
  - Every `go test` gets an explicit `-timeout` and `-count=1`. Every exit code is captured on the NEXT line, before any pipe (floor 7): every `git merge`, `go build`, `go test`, `git archive`, extraction, index `--selftest` and dry run, and every script launch.
  - The census is `<repo>/.claude/coord-scripts/coord-identifier-census.sh` from MASTER `074a12c4ae`, whose embed-payload mirror is `33f5f11297`. Assert `git -C <repo> rev-parse HEAD` first, then `git -C <repo> diff --quiet 074a12c4ae -- .claude/coord-scripts/coord-identifier-census.sh .claude/coord-scripts/coord-identifier-patterns.txt .claude/coord-scripts/coord-identifier-hashes.txt`, then `rc=$?`; rc must be 0, otherwise STOP (floor 15: the script is read from disk). Run `entry` on every staged doc, page, README and report.
  - GPG: probe before every `-S` with `echo test | <gpg-exe> --batch --pinentry-mode error --clearsign -u <gpg-key>`. When it is cold, re-probe every 60 s for up to 30 min as a BACKGROUNDED until-loop (foreground `sleep` is blocked), then STOP. Never kill gpg-agent.
- **CREDENTIALS.** `NUGET_API_KEY` and `NuGetCertFingerprint` live at User scope and are inherited by every process. At the start of EVERY shell, remove both from the PROCESS environment only (`Remove-Item Env:NUGET_API_KEY, Env:NuGetCertFingerprint -ErrorAction SilentlyContinue`, or `env -u` in bash). Then assert both absent by BOOLEAN. Never read, print or inject their values.
- **NEVER RUN** (a STOP-level breach, not a judgement call):
  - `release-nuget.ps1` without `-VerifyOnly`, and `release-nuget -WhatIf`, which probes the card;
  - `push-nuget.ps1 -Push` in any tree; `-BumpBuild` in any tree EXCEPT, when RN-7 rules the isolated clone, in `<coord-tmp>/abh12-clone` after asserting that `git -C <coord-tmp>/abh12-clone remote` prints nothing and that its `--git-common-dir` is its own;
  - `sign-nupkgs.ps1` in any mode (its census touches the card);
  - `dotnet nuget push` or `dotnet nuget sign`;
  - `git tag` in any CREATING, MOVING or DELETING form (`git tag <name>`, `-a`, `-s`, `-f`, `-d`) in any worktree of `<repo>/.git`, because tags are SHARED across linked worktrees and push-nuget KEEPS an existing tag (`push-nuget.ps1:528-529`). `git tag --list <pattern>` is read-only and permitted; it is what B2 and C8 use;
  - `set-version.ps1`, `deploy-core.ps1`, or `run-h10-recon.ps1` other than `-SelfTest`;
  - `migrate-gorelease.ps1 -Apply` unless the prompt rules it;
  - `git push`.
- **SERIAL, ALWAYS.** Every leg runs to completion before the next starts, and a backgrounded run is awaited to its rc. Before every converting or building leg, count go2cs.exe BY IMAGE PATH; the count must be 0. A stray process is a STOP, never a kill (floors 1, 5). Converter, gen and golib source is FROZEN while any battery runs (floor 4), and every long run goes from a per-run COPY of its script.
  - The tool's foreground cap is 600 s. A2(2)'s suite, each A4(3) arm, A5's CNR and C7's rehearsal ALWAYS run backgrounded (`run_in_background`). The command itself writes its rc: `…; rc=$?; echo "$rc $(date +%s)" > <stage-or-scratch>/<leg>.result`. Await it by its completion notification, never by a foreground `sleep` poll. A leg killed by a tool timeout is a STOP: count go2cs.exe and dotnet.exe by image path and command line, and never kill.
- **STAGING.** Every commit is signed and staged by explicit path, never `git add -A` or `git add .` (floor 8). After every purge, restore, clean or run, print `git status --porcelain` UNFILTERED (floor 16), then assert `git status --porcelain | grep '^ D'` prints nothing. Across any discard, `git ls-files | wc -l` must be equal before and after.
- **DISK.** Read free disk before every leg; it must be >= 25 GB (floor 12). It read 34 GB at the derivation, and the close's single-pass behavioral control dipped to 22 GB. Purge after every build leg with `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <tree>/src/clean-bin.ps1)" -Force < /dev/null`, then `rc=$?`, and gate on rc 0 (2 = non-interactive without `-Force`, 4 = folders remained).
- **Nicknames only** for machines. No hostnames, usernames, profile paths or IPs in any commit, message or report. Quote every log line with the GOROOT value replaced by `<goroot>` and the dotnet root by `<dotnet10>`, for example `toolchain: GOROOT <goroot> (VERSION go1.24.13, read in-process)`; the converter's toolchain line, env.sh's `pincheck` and every driver's PIN lines print a profile path. Run census `entry` on every report BEFORE sending it.

# PART A -- THE POST-CLOSE BATCH

## Fixed facts (fetch, then assert each by `git ls-remote origin`; STOP on any mismatch)
- BASE: `claude/version-go1.24.13` = `fa18863b94`, the H10 CLOSE STAMP.
- Worktree: `git worktree add -b i7/post-close-<mmddHHMM> <coord-tmp>/abpost fa18863b94`. It is a NEW worktree (floor 11), never abclose. Assert that `--git-dir` != `--git-common-dir`. Take a lock at `<coord-tmp>/abpost.lock` holding the PID. Scratch goes to `<scratchpad>/abpost/`. The stage `<stage>` = `<coord-tmp>/abpost-stage` must NOT exist yet. `abclose` and `abclose-stage` stay IN PLACE and READ-ONLY; they are A4(4)'s determinism comparand. Copy the close's scripts (`<scratchpad>/abclose/{env.sh,g2count.sh,classify.py,b8-bin.sh,b8-seed.sh,b8-convert.sh,b8-checks.sh,h5c.sh,h5c-extract.py,cnr-driver.ps1}`) into `<scratchpad>/abpost/` and RE-POINT every `abclose*` path and image-path prefix at `abpost*`, and set `REGEN=<arm-a>` in the copied `b8-bin.sh`, where `<arm-a>` is R-P2's ruling. Echo `REGEN=$REGEN` into `logs/build-A.log`. A copied script that still reads `54dec61728` without a ruling naming it is a STOP. `b8-overlay.sh` is NOT copied: it runs `tar -xf` into the worktree, so copy and re-point it only after R-P3 rules an overlay; A4(8) spells out the absent set without it.
- `781c1c3c31` is NOT in the local object store. Run `git fetch origin claude/c2-readme-badge-seed claude/c2-csproj-merge-shared-items <the branch of c1-docs-ref>`, then `git cat-file -e <sha>^{commit}` for each ref. A SHA quoted in a message is not a ref (LEDGER 22:36).
- The git on the i7 is 2.35.2, which has NO `merge-tree --write-tree`. Predict a merge with the legacy `git merge-tree <base> <ours> <theirs>`, which is read-only.
- **COORD, at Part A's launch (CP0):** rule RN-6 and DISPATCH the release-census seat NOW, in parallel with Part A (it touches `src/push-nuget.ps1` and the roster instruments, none of Part A's ALLOWED paths). CP1 only names `<census-seat-ref>`.
- THE PLACEHOLDERS AND RULINGS THIS PART CONSUMES. A missing `<c1-docs-ref>` or R-P1 => STOP before A1. A missing R-P2 => STOP before A4(1). R-P3 is consumed at A4(9).
  - **`<c1-docs-ref>`**: C1's docs seat. LEDGER 22:40 ACCEPTED `claude/c1-close-docs` = `13e9b039ea` for it.
  - **R-P1** (COORD RULING NEEDED, RN-1): whether G's crypto/tls LINUX rebank rides this batch, `<g-tls-linux-ref>` or `none`. LEDGER 22:44 says it rides "if G's ref arrives before that batch's Part A assembles", and COORD rules at assembly.
  - **R-P2** (COORD RULING NEEDED, RN-2): ARM A of the regen. NO default: a missing R-P2 => STOP before A4(1), quoting rb :3010-3012 beside COORD's dispatch pin `REGEN_BASE 54dec61728`. Report A's header states which arm ran and its prediction set (A-only 337 vs about 455). The brief's reading, for COORD, not a default: `fa18863b94`'s converter. The runbook says arm A comes from "the last corpus-wide regen" (rb :3010-3012). That is the close's overlay `1a328f3ee8`, emitted by the `922994cec3` converter, and `fa18863b94` differs from it only by a repoguard `_test.go`. The close's arm-B identity sha256 `7014b789e231ee23fdf0002245c71f84c984f9f845d9d3bfb5dce4d51b1c9340` was re-asserted EQUAL at LEDGER 21:43 / 22:30. COORD's pin `REGEN_BASE 54dec61728` was the CLOSE's arm A (its binary's sha256 `e0b2a4c109053c6b45ba01d731dc01b2b204a057bed50cfd5afdbb83502a347e`, `<coord-tmp>/abclose-stage/bin/A`). As arm A here it re-measures the close's 93 A-only and 25 B-only files as noise, predicting A-only of about 455, attributed by merge. Either arm is COORD's to name; the run uses exactly the one named.
  - **R-P3** (COORD RULING NEEDED, RN-3): a NON-zero footprint. DEFAULT: STOP and post the hunks. The alternative is an in-batch overlay by the close recipe, which owes step 9's full gate set.
  - Name the ledger line that makes this regen §6's FOURTH seeded reconvert, after H4a, H5 and the close's ruled third (22:30 NEXT; 18:23; 19:01; rb :3005-3009).

## A1 -- THE REFS, merged `--no-ff -S` IN THIS ORDER
For each ref, before the merge, print `git diff --numstat <ref>^ <ref>` against ALLOWED, and predict the merge with the legacy merge-tree. Each `git merge --no-ff -S` is followed by `rc=$?` on the next line and gates on rc 0. After each merge, `git diff --diff-filter=D fa18863b94 HEAD` must be empty. Any path outside ALLOWED => STOP.
1. `claude/c2-readme-badge-seed` = `781c1c3c31`: one commit on `47e088d3d7`, an ancestor of `fa18863b94`. `seedCensusRoot` seeds the WHOLE `docs/validation` into each per-target staging root, so `publishedPackageVersion` resolves 1.23.12.3 in staging (R-B13). ALLOWED: `src/go2cs/platformCensus.go` (+9/-4, per C2), its test file (name it after the fetch), and `.claude/skills/corpus-reconvert/SKILL.md` INSIDE an HTML comment only (zero visible lines). No version-branch commit touched those paths after `47e088d3d7`, so the merge is predicted CLEAN. Score whether the ref carries the dated amendment to SKILL.md:86's stale "need NOT be seeded" provenance (18:23). If it does not, report it as OWED; `<c1-docs-ref>` does not carry it.
2. `claude/c2-csproj-merge-shared-items` = `ae813db069`: one commit on `47e088d3d7`. `platformReferenceBlockEnd` bounds the L3 block's own extent, so a re-render keeps the shared-LICENSE `ItemGroup` that licensing appends after it (R-B15). ALLOWED: `src/go2cs/platformProject.go` (+63/-6) and `platformProject_test.go` (+122/-0). The legacy merge-tree against `fa18863b94` reads "merged" for both files, so the merge is predicted CLEAN.
3. `<c1-docs-ref>`: one commit on `fa18863b94`. It carries lessons 4-7 (R-B13..R-B16) in the close amendment, the R-B16 `-p:go2csPath=<repo>/src/` pin in step 9 and the §6 row, and one line in harness-gates "Build context". ALLOWED: `.claude/rules/harness-gates.md` (+1/-1) and `docs/GoCorpusMigration.md` (+47/-2). After this merge, RE-READ the close amendment and H11/H12/§5/§6 AT THE TIP, and STOP on any disagreement with this brief.
4. Only if R-P1 names it: `<g-tls-linux-ref>`. ALLOWED: the crypto/tls row's linux annotation and prose in `docs/ValidatedTestPackages.md`, plus the header's linux line as a CARRIER. A carried figure is never trusted; A6 overwrites it with the guard's output (rb :3060-3066). Anything else => STOP.
- Both C2 refs are unsigned lane commits (`%G?` = N). The signed `--no-ff -S` merge is their entry. Quote each merge's `%G?`.

## A2 -- THE CONVERTER GATES at the merged head (BEFORE the freeze)
1. **RED-FIRST REPLAY (floor 13).** In the working tree, run `git checkout fa18863b94 -- src/go2cs/platformCensus.go`. Then, with cwd `<tree>/src/go2cs`, `go test -run 'TestSeedCensusRootCarriesThePublishedStampTheBadgesRead' -count=1 -timeout 10m . > <log> 2>&1`, then `rc=$?`. It must FAIL ON ITS ASSERTION: the gate is rc != 0 AND `--- FAIL: TestSeedCensusRootCarriesThePublishedStampTheBadgesRead` in the log AND no `undefined:` or `[build failed]`. Quote the failing line; the badge-seed arm's red reads both staged badge lines empty (LEDGER 18:23). A compile or build failure is not a red-first and is a STOP. Restore with `git checkout HEAD -- <path>` and confirm `git diff --quiet`. Repeat for `platformProject.go` with `go test -run 'TestRenderPlatformReferencesKeepsWhatFollowsTheBlock|TestPlatformMergeKeepsASharedNonReferenceGroup' -count=1 -timeout 10m . > <log> 2>&1` (the alternation QUOTED), then `rc=$?`, gated the same way on `--- FAIL: <each name>`.
2. **The suite.** Run `cd <tree>/src/go2cs && go test ./... -count=1 -timeout 40m > <log> 2>&1`, then `rc=$?`, from a per-run copy, BACKGROUNDED (about 515 s; Rules, the 600 s cap). Predicted GREEN. That includes C2's three arms; `licensing_test.go:405-421`, which guards log/syslog's committed LICENSE group; TestContextBudget; TestStdLibMetadataInSync; TestPublishedCounterMatchesTheRecordedReleases; TestCommittedCoreReferencesResolve (`declared 0 · measured 0`); and repoguard. The close read go2cs at 415 s and repoguard at 100 s. The UNION of the refs has never been tested, and a same-package test-helper collision would red here.
3. **repoguard PLANT.** Put a denied token in a tracked file. `TestNoFleetIdentifiersInTrackedFiles` must red and name it. Then restore and confirm `git diff --quiet`.
4. **IDENTITY BUILD.** Run `go build -trimpath -buildvcs=false` of the merged `src/go2cs`, then `rc=$?` (gate rc 0). Check `go version <exe>` = go1.24.13. Record its sha256; it is predicted to DIFFER from `7014b789…`.

## A3 -- THE FREEZE
From here to CNR's exit, converter, gen and golib source is FROZEN (floor 4). `git status --porcelain` must be empty (unfiltered). Take the ignored-residue census now, per rb :2999-3001 (step 7 states no condition): `git status --porcelain --ignored=matching -- src/core src/gen docs/validation`, then `rc=$?`, zero `!!`, proven by a plant (one ignored scratch file under `src/core`, named by the census, then removed). Skipping it needs an R-P ruling quoted in Report A.

## A4 -- THE PROVING REGEN (rb :3005-3031; per-arm checks rb :753-812)
(1) **BINARIES.** Build both into paths that do not exist yet, with `go build -trimpath -buildvcs=false` under the pin.
   - Arm A = R-P2's converter, built by the re-pointed `b8-bin.sh` from `git archive <arm-a> src/go2cs` extracted into `<stage>/base-src`. ASSERT its sha256: `7014b789…` for arm A = `fa18863b94`, or `e0b2a4c1…` (the close's `bin/A`) for arm A = `54dec61728`. A different hash is a STOP.
   - Arm B = the worktree's `src/go2cs` at HEAD. It must equal A2(4)'s identity.
   - For each binary: `go version` = go1.24.13; it is newer than its sources; its usage lists `-stdlib -comments -go2cspath -platforms -platform-stage -convert-timeout`.
(2) **ONE SEED, THREE ROOTS.** Seed only after the last merge. Run `git archive -o <stage>/seed.tar HEAD src/core src/gen src/Directory.Build.props src/version.props docs/validation`, then `rc=$?`, and record its sha256. Extract the FILE, never a pipe, into `<stage>/A`, `<stage>/B` and `<stage>/S`, reading rc for each. S is the classifier's reference root. Before either arm converts, assert per root:
   - the seeded `src/core` `.cs` count equals the tracked count under `core.quotePath=false` (4169 at `fa18863b94`; re-derive);
   - version.props reads 1.24.13 / 0;
   - `docs/validation` holds current/, index.md and the nine snapshots, newest 1.23.12.3. EVERY prediction below rests on this.
(3) **CONVERT A, THEN B**, each backgrounded and awaited to rc.
   - Before each arm: go2cs.exe count 0; disk >= 25 GB; `touch <stage>/<R>.run.stamp; sleep 2`.
   - Command: `CGO_ENABLED=0 "<exe>" -stdlib -comments -go2cspath "<stage>/<R>/src" -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage "<stage>/<R>-stage" -convert-timeout 90m > "<stage>/logs/<R>-convert.log" 2>&1`, then `rc=$?`.
   - GOROOT is exported exactly as `<goroot>`. `-go2cspath` is always explicit, because it DEFAULTS to the machine deploy root.
   - Budget about 1,252 s per arm (the close's reading).
   - RETRY: never convert twice into one root. Delete the root, its `-stage` and its sentinel; re-extract the SAME seed.tar; re-assert; re-run.
(4) **PER-ARM CHECKS** (gating for B, reported for A):
   - rc 0, with the wall;
   - 0 NULs;
   - 0 `did not fully type-check`;
   - exactly three `Failed: 0`;
   - the `toolchain: GOROOT … VERSION go1.24.13` line;
   - a non-zero count of `.cs` newer than the sentinel, per target;
   - 0 `^namespace go[.]std`;
   - 0 GOROOT basenames in package_info.cs;
   - the marker gate at 0 violations / 0 missing;
   - the `Stale copies removed` line (predicted 0);
   - du of each root and stage.

   NEW CHECKS:
   - (a) B's `<stage>/B-stage/<goos>-amd64/docs/validation` lists the snapshot dirs on all three targets, while A's lists current/ only. Quote each "seeded N files" line: predicted about 8,600, against the close's 7,377 and C2's +1,220.
   - (b) 0 renderer errors from the new `platformReferenceBlockEnd` paths (`conditioned reference group is not terminated/closed/followed…`). log/syslog's InternalsVisibleTo merge note is EXPECTED to remain: it is routed post-hop (LEDGER 19:01, 19:31).
   - (c) DETERMINISM CONTROL: diff the new A root, CR-stripped, against the close's root built by the same binary, read-only: `<coord-tmp>/abclose-stage/B/src/core` for arm A = `fa18863b94`, `<coord-tmp>/abclose-stage/A/src/core` for arm A = `54dec61728`. It is predicted IDENTICAL. Post any difference by path before trusting (5). PLANT first: diff a scratch copy of one A-root file with one byte changed; the diff must name it.
(5) **CLASSIFY BY CONTENT.** Run the close's `classify.py` VERBATIM. It takes FOUR arguments, the three CORE directories and an output dir (`classify.py <S-core> <A-core> <B-core> <outdir>`), and native Windows python cannot open MSYS paths: `python "$(cygpath -w <scratchpad>/abpost/classify.py)" "$(cygpath -w <stage>/S/src/core)" "$(cygpath -w <stage>/A/src/core)" "$(cygpath -w <stage>/B/src/core)" "$(cygpath -w <scratchpad>/abpost/cls)"`, then `rc=$?`. Passing the ROOTS instead prefixes every class path with `src/core/`, so no by-name disposition matches. It is a pure CR-stripped compare that includes `*.cs.auto`. **Re-derive the disposition step; NEVER reuse `tclass.py`.** That script files every README as "(v) -> RESTORE" and log/syslog as "(vi)" BY PATH, so it would absorb exactly the classes this run must prove EMPTY (the copied-script trap, corpus-reconvert SKILL :208). The dispositions, in order:
   - A-only is the positive control and is never overlaid;
   - T4 BY NAME;
   - (iv) BY NAME;
   - everything else is T5, which is a STOP with the hunk quoted.

   PLANT before trusting a zero T5: `cp -r <stage>/S <stage>/S-plant`, add one non-map line to `<stage>/S-plant/src/core/strings/strings.cs`, and re-run with S-plant's core and a separate outdir `<scratchpad>/abpost/cls-plant`. The disposition must name `strings/strings.cs` as T5 (1). Then remove S-plant and cls-plant.

   PREDICTED with arm A = `fa18863b94` (for `54dec61728`, A-only is predicted about 455, attributed by merge; see R-P2), scored as worded:
   - **A-only 337** = 336 READMEs (the stripped Tests and Source·C# lines, 0 GoPositionMap lines) + `log/syslog/log.syslog.csproj` (loses its 3-line `../../LICENSE` group). An EMPTY A-only set means the pair measured nothing: ABORT.
   - **B-only 0.**
   - **BOTH-EQ 2** = T4 `runtime/runtime2.cs.auto` and `crypto/internal/fips140/subtle/xor_generic.cs.auto`. bcache's `cache.cs.auto` is predicted reproduced by both.
   - **BOTH-NE 1** = (iv) `crypto/internal/fips140deps/README.md`. A strips it; B composes GREEN `1%2F1` linking `validation/1.23.12.3/`; the committed file is hand-set ORANGE (`8f6abad087`; R-B12; rb :3028-3031).
   - T3 0, T5 0.
(6) **THE VANISH READINGS**, printed unfiltered:
   - **(v)**: B vs S differs on EXACTLY 3 paths, the (iv) README and the two T4 files. The identity 337 = 336 vanished + 1 (iv) must close. So C2's "the 337 READMEs coming back byte-identical" and `<c1-docs-ref>`'s lesson-4 sentence are predicted to MISS BY EXACTLY ONE, the ruled hand-set line. Score the miss; never re-scope it. Any other README in B's column is T5 => STOP.
   - B's root holds 7 READMEs without a Tests badge (the committed no-badge set; the close's B root read 344).
   - All 342 badge-carrying READMEs keep `Source-@1.23.12.3-512BD4`.
   - net/http's README equals its committed hand-set orange line: the close's B4(d) prediction, scorable for the first time.
   - **(vi)**: `log/syslog/log.syslog.csproj`, B == committed, with the LICENSE group present. All 568 tracked csprojs, the 22 L3 among them: B == committed.
   - Per-target `.cs.auto` in B's stage vs committed is unchanged from the close: windows 2 differ, linux 4, darwin 2.
   - Never gate on mtime: the fixed converter does not rewrite an unchanged README (`needToWriteFile`).
(7) **THE PLATFORM-EXCLUSIVE SEED SURVIVORS** (a reading, not a gate). `mergeCompanionArtifacts` copies each non-`.cs` companion from the FIRST target that has it (`platformEmit.go:634-655`), and the staging snapshot includes the SEEDED windows copy. For `crypto/x509/internal/macos`, `internal/runtime/syscall` and `vendor/golang.org/x/net/route`, diff each package's README and csproj from its linux or darwin stage emission against the committed file, CR-stripped. Report by path: Docs @1.23.1 or x/net@v0.25.1 vs @1.24.13, and the package_info-first `Compile` order. Their root files "reproduce" only VACUOUSLY. Disposition: RN-5.
(8) **ABSENT SET AND H5c.**
   - The absent set is tracked `src/core` `.cs`/`.csproj`/`README.md` minus B's root. Predicted EMPTY. Spelled out (never via `b8-overlay.sh`, which writes into the worktree):
     - `set -o pipefail; ( cd <tree> && git -c core.quotePath=false ls-files -- 'src/core/*.cs' 'src/core/*.csproj' 'src/core/*README.md' ) | sed 's#^src/##' | LC_ALL=C sort > <stage>/logs/tracked.txt; rc=$?`
     - `( cd <stage>/B/src && find core \( -name '*.cs' -o -name '*.csproj' -o -name README.md \) -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/Generated/*' ) | LC_ALL=C sort > <stage>/logs/b-root.txt; rc=$?`
     - `LC_ALL=C comm -23 <stage>/logs/tracked.txt <stage>/logs/b-root.txt`, printed unfiltered.
     - `core.quotePath=false` is load-bearing: without it the 13 golib `ж` files list quoted and read as absent, a false STOP.
     - PLANT (floor 13): remove one line from a scratch copy of `b-root.txt`; `comm` must name exactly that path.
   - H5c runs DRY on every flavour (rb step 8) through the copied, re-pointed `h5c.sh`, which launches `powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <tree>/src/reconvert-deletions.ps1)" -Root "$(cygpath -w <stage>/B/src)" -GoRoot '<goroot>' -ExpectGo go1.24.13 -SourceGoRoot '<goroot>' -ExpectSourceGo go1.24.13 -Sentinel "$(cygpath -w <stage>/B.run.stamp)" -Goos <os> -Goarch amd64 < /dev/null > <log> 2>&1` with `rc=$?` on the next line, plus `h5c-extract.py` per flavour. The gate is rc 0 or 2.
   - Predicted DELETE-ABSENT 0, DELETE-DESELECTED 0 and UNRESOLVED 0 per flavour. Never `-Apply`.
(9) **OVERLAY DECISION.** If B's column is exactly the 3 predicted paths, the footprint is ZERO with every hunk named: NO overlay and NO regen commit. Otherwise STOP per R-P3 and post each extra path's numstat and line kinds, CR-stripped. If COORD then rules an overlay, it follows rb :3023-3040 and lessons 2 and 3 (:3096-3120):
   - widen the copy by `<EmbeddedResource Include>` payloads;
   - dispose of the absent set;
   - restore T0 (the close's 57 mixed-ending phantoms), T4 and (iv);
   - adopt `go2cs-stdlib.slnx`;
   - run `go generate .`;
   - make ONE commit;
   - prove the pins by re-checkout;
   - then run step 9's gates: H7 on three flavours, GolibTests, behavioral in five purged slices, `go2cs.slnx` Release PINNED `-p:go2csPath=<tree>/src/` (R-B16), and the rows whose production moved.
(10) **PRESERVE** `<stage>` (the three roots, both binaries, seed.tar and every diff) until COORD accepts. Before any reclaim, COPY the three seed survivors' linux and darwin README and csproj emissions (A4(7)) to `<scratchpad>/abpost/survivors/`, each with its sha256; C2 reads them from there.

## A5 -- CNR (after both arms exit)
Copy `check-no-regression.ps1` to `<tree>/src/tests/Behavioral/check-no-regression.runpost.ps1`: it resolves its siblings by `$PSScriptRoot` (`_paths.ps1` at :217, `check-solution-integrity.ps1` at :240), so the copy sits beside the original. Run it through the re-pointed `cnr-driver.ps1` (its `$WT` at `<tree>`, its copy name at `.runpost.ps1`; the pins are set inside; launched by the pwsh 7 shape in Rules), BACKGROUNDED, with git and go on PATH. The gate is rc 0 AND the `==> NO REGRESSION` line (CNR exits 0 on NO REGRESSION, 1 on CHANGED). Then delete the copy, print `git status --porcelain` unfiltered (the copy is not gitignored, so it reads `??` until removed), and assert ` D` empty. It is owed because both C2 refs change converter source (§6, rb :3602). Never add an up-to-date skip (floor 10). The gate is `NO REGRESSION` with CHANGED predicted EMPTY, because the seeder and the L3 merge renderer are off CNR's single-package path. The close read 730 byte-identical in 988 s; the budget is 1,050-1,750 s with a cap of 2,400 s. Its check-solution-integrity preflight must be green.

## A6 -- FIGURES, INDEX, CENSUS
- Run `<tree>/src/check-roster-format.ps1` by the pwsh 7 shape in Rules (`env -u DOTNET_ROOT … PATH="$ORIG_PATH" pwsh -NoProfile -File "$(cygpath -w <tree>/src/check-roster-format.ps1)" < /dev/null > <log> 2>&1`, then `rc=$?`) and quote the 2b2/2b3/2e/2f/2g lines. The gate is rc 0 and `0 of N` (the close read 1,812 checks).
- WITHOUT R-P1's ref, the figures are predicted UNCHANGED: 218/230 = 94.8%, 218/224 = 97.3%, 56,974 / 283; linux 187/216, 49,629 / 163; 2b2 225 = 218 + 4 + 3. Commit nothing.
- WITH it, write the header's linux line and NEWS EXACTLY as the guard derives them, with the carried hunk quoted beside it, as a signed commit.
- `python "$(cygpath -w <tree>/docs/phase4/hopA-inputs/regen-validation-index.py)" --selftest`, then `rc=$?`; then the same with no flag (the dry run), then `rc=$?`: predicted no change (218 rows; 232 pages = 218 + 10 + 4). The script roots itself on its own location (`regen-validation-index.py:45-48`), so only `<tree>`'s copy reads `<tree>`'s roster. Run `--write` only if a roster row changed.
- Census `entry` on C1's two files, every merge message and your report. Fold repoguard's plant from A2(3) into the report.

## A7 -- GATES NOT OWED (state each with its reason; COORD confirms)
With a ZERO footprint, these are not owed:
- GenTests: src/gen is unchanged.
- GolibTests: golib is unchanged.
- H7: no src/core file moves.
- The behavioral suite: CNR covers the converter, and no core C# moves.
- `go2cs.slnx`: no golib/runtime API change.
- H8's census: it walks src/core only.
- Row sweeps: no production moves.
- The H6 audit: T4 is unchanged.
- `go generate .`: no package_info.cs moves, as TestStdLibMetadataInSync confirms.

A7's conditional set is A4(9)'s.

## Commits (Part A; local branch, each signed, each staged by explicit path)
1. The merges, A1.1-A1.3, plus A1.4 if ruled.
2. The guard-derived linux line and NEWS, only with R-P1's ref.
3. The overlay, only by R-P3's ruling.

Close-out: `git status --porcelain` empty (unfiltered) with no ` D`; go2cs.exe count 0; lock released; abpost and `<stage>` IN PLACE; abclose untouched; NOTHING PUSHED.

## Report A (final message), then STOP -- do not start Part B
Include:
- in the HEADER, the arm A that ran (R-P2, quoted) and its prediction set (A-only 337, or about 455 for `54dec61728`);
- the refs as asserted, each merge's SHA with `%G?`, and the SKILL-amendment finding;
- the red-first replays;
- the suite line and repoguard's plant;
- the identity sha256 beside `7014b789…`;
- both binaries' sha256;
- the per-arm table with the new checks and the determinism control;
- the classification tally BESIDE the close's (B-only 25, A-only 93, T0 57, T3 7, T4 3, (v) 337, (vi) 1, T5 0), with the plant's red;
- the vanish readings scored as worded, with the miss-by-one named;
- the seed-survivor diffs;
- the absent set and H5c per flavour;
- CNR's line;
- the not-owed gates;
- the guard and index lines;
- disk before and after;
- `git log --oneline --first-parent fa18863b94..HEAD` with `%G?`;
- everything not verified, stated as such.

# COORD CHECKPOINT 1 (COORD's work)
1. Review Report A. Rule RN-3/RN-4 if the footprint was not zero, and RN-5, the seed survivors.
2. Decide whether lesson 4's "byte-identical" sentence gains a clause for the ruled hand-set line, as a docs follow-up, or stays as written with the scored miss on record.
3. ANNOUNCE, then push the fast-forward to the batch head (floor 9). Write the ledger STAMP line with the time read from `date`. It carries the regen's tally and the new identity sha, because a zero footprint commits nothing (RN-4). Refresh the record. Name `<post-close-stamp>`.
4. After acceptance, reclaim `abclose` and `abclose-stage` children-first (floor 12).
5. Fill Part B: rule RN-9, and name `<census-seat-ref>` (the seat CP0 dispatched under RN-6) or `none`.

# PART B -- H11 (publication and compatibility guards, rb :3138-3158)

## Fixed facts
- BASE: `<post-close-stamp>`, asserted by ls-remote. Worktree: `git worktree add -b i7/h11-<mmddHHMM> <coord-tmp>/abrel <post-close-stamp>` (a NEW cut), then `rc=$?`. Assert `git -C <coord-tmp>/abrel rev-parse --git-dir` != `--git-common-dir` (a child, floor 12). Lock `<coord-tmp>/abrel.lock` holding the PID. Scratch `<scratchpad>/abrel/`. Copy `<scratchpad>/abpost/env.sh` there and re-point its tree path at abrel; source it in every shell (Rules, ONE PARENT SHELL). Build go2cs.exe under the pin and check that `go version` = go1.24.13.
- H11 is a GATE rung. Its reading is RED until B6 is green, and Part C does not start on a red H11 without COORD's word.

## B1 -- THE COUNTER (rb :3140; the H2 amendment rb :249-266)
- Read `src/version.props` by output. Predicted 1.24.13 / 0.
- Run `go test ./internal/repoguard -run TestPublishedCounter -count=1 -v -timeout 10m`, then `rc=$?`. Predicted GREEN, with its t.Logf quoted: `base 1.24.13 · counter 0 · recorded on this base none · newest recorded overall 1.23.12.3 · 9 releases recorded`.
- PLANT: set `<GoBuildNumber>3</GoBuildNumber>` in the working tree. It must fail naming version.props and "NO release is recorded". Restore and confirm `git diff --quiet`.

## B2 -- EXISTENCE PLUS MONOTONICITY, SCRIPTED, before the first publish (rb :3141-3151; PLAN OQ-11 :661)
- Existence is B1. Monotonicity is a script in `<scratchpad>/abrel/`, with its home per RN-9. It collects every recorded stamp (the `docs/validation/<stamp>/` dirs) and every `nuget-*` tag: `git tag --list 'nuget-*'` and `git ls-remote --tags origin 'nuget-*' | grep -v '\^{}$'` (the peeled rows of annotated tags dropped), each under `set -o pipefail` with `rc=$?` on the next line. It keeps only names matching `^nuget-[0-9]+(\.[0-9]+){3}$` and prints every other `nuget-*` name as EXCLUDED, NON-RELEASE, by name (predicted: `nuget-stdlib-2026-07-14`): never dropped silently, never parsed. It then asserts, NUMERICALLY per component, that 1.24.13.1 > the newest. The comparator follows NuGetVersion semantics per OQ-11 (PLAN :661), which for four numeric components is identical to `[System.Version]` and to `releasestamp.Compare`'s rule (`stamp.go:175-204`); the script states that. It prints the newest; predicted 1.23.12.3 and `nuget-1.23.12.3`.
- PLANTS: `1.23.12.10 > 1.23.12.9` and `1.23.12.1 > 1.23.9.1` must hold. A LEXICAL comparator and a reversed pair must each red the script. A third: the excluded tag present must NOT crash the script and must NOT be chosen as newest.
- The feed is ADVISORY only, never the gate.

## B3 -- THE COMPATIBILITY GUARD (rb :3152-3154)
- `go version <exe>` = go1.24.13.
- With cwd `<tree>/src/go2cs`: `go test -run 'TestCheckNuGetStdLibCompatibility|TestCorpusPinIsStricterThanTheNuGetGuard|TestRecurseNuGetReferences' -count=1 -timeout 20m . > <log> 2>&1`, then `rc=$?`. The gate is rc 0 (GREEN).
- Note: `-recurse=nuget` has asked for `go.<pkg> 1.24.13.*` since train 43, and it cannot restore until 1.24.13.1 publishes. The publish closes that gap; no code change is owed.

## B4 -- PACKAGE IDs (rb :3155-3156; OQ-13 :663)
- Derive READ-ONLY, BY NAME, never carrying a count: the packable library projects in `go2cs-stdlib.slnx` at the tip vs the `nuget-1.23.12.3` tree. IDs are `go.<dotted path>`.
- Predicted: 51 NEW packable IDs, 6 of them public (crypto/fips140, crypto/hkdf, crypto/mlkem, crypto/pbkdf2, crypto/sha3, weak).
- Predicted: 14 REMOVED IDs, per the successor table at `docs/phase4/CENSUS-go124-package-delta.md:64-79`: crypto/internal/{alias,bigmod,edwards25519,edwards25519/field,mlkem768,nistec,nistec/fiat}, internal/{concurrent,weak}, runtime/internal/{math,sys}, vendor/golang.org/x/crypto/{hkdf,sha3} and go/internal/typeparams (deleted).
- For the OWNER, write the deprecation instructions for each removed ID: DEPRECATE, NEVER unlist. Shape them to nuget.org's own form (Manage package -> Deprecation; the owner's screenshot, 2026-09-24). The form asks for **Select version(s)**, **Select reason(s)** (at least one of: *This package is legacy and is no longer maintained* / *This package has critical bugs that make it unusable* / *Other*) and, once a reason is checked, an optional **alternate package** (ID, plus a version or any version) and an optional **custom message**. Produce ONE table row per removed ID, with columns: package ID (e.g. `go.crypto.internal.alias`) | versions to select (EVERY published version of that ID, listed from the flat container's index; the ID has no future version) | reason to tick (**legacy**; never *critical bugs*, since 1.23.12.3 still works with the 1.23.12.3 set) | alternate package ID (the SUCCESSOR's `go.<dotted>` ID from the table above; for `go/internal/typeparams`, none) | alternate version (**1.24.13.1**, or "any version" if the form offers it) | custom message (one or two sentences, at most 200 characters, that name the Go 1.24 move and the last release that carried this ID, e.g. "Go 1.24 moved this package to crypto/internal/fips140/alias. Use go.crypto.internal.fips140.alias 1.24.13.1 or later; 1.23.12.3 is the last release of this ID."). For a split successor (`vendor/.../sha3` -> fips140/sha3 + the public crypto/sha3; mlkem768 -> fips140/mlkem + crypto/mlkem), name the PUBLIC successor as the alternate and both in the message. TIMING: the alternate should already exist on nuget.org when the form is saved (confirm it at the first deprecation), so the deprecations run AFTER 1.24.13.1 is published. List them as the owner's POST-PUBLISH hand.
- ADVISORY feed read: one GET per new ID against the nuget.org flat container, reporting 404 or occupied, plus the 8 non-owner `go.*` IDs (LEDGER 09-22 19:24). A push to an occupied ID fails MID-PUBLISH. You never touch nuget.org beyond read GETs.

## B5 -- THE STAMP IS A REPOSITORY FACT (rb :3157-3158)
Census at the tip, expecting exact counts:
- 342 READMEs carry `badge/Source-@1.23.12.3-512BD4` plus a `tree/nuget-1.23.12.3/src/core/` link;
- 214 carry `go2cs.net/validation/1.23.12.3/`;
- 320 carry Docs/Source·Go `@1.24.13`;
- no `docs/validation/1.24.13.*` directory exists;
- no `nuget-1.24.13*` tag exists, locally or on origin.
- PLANTS (floor 13): prove each zero-expectation pattern fires by running it once against a KNOWN hit first: `docs/validation/1.23.12.*` must list 1.23.12.1-.3, and `nuget-1.23.12*` must list 3 tags, locally and on origin. Only then read the 1.24.13 zeros. Each exact count is proven the same way: its pattern run against one README whose line is known.

## B6 -- THE RELEASE PRE-FLIGHT (`release-nuget.ps1 -VerifyOnly`; ruled H11 gates, RESUME-SESSIONS :879-883)
1. `check-roster-format.ps1`, by A6's pwsh 7 shape, gives rc 0.
2. RED-FIRST, at the tip, launched the way the release launches it (Windows PowerShell 5.1), from the pinned bash: `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <tree>/src/release-nuget.ps1)" -VerifyOnly < /dev/null > <log> 2>&1`, then `rc=$?` on the next line. Record which host ran it.
   - PREDICTED RED by code reading (never measured): `Release census: 214 green badge(s) / 232 proof page(s) / 218 roster row(s) / 225 .tests.csproj`, with EXACTLY 21 named problems:
     - 4 README-less rows: crypto.internal.fips140test, embed.internal.embedtest, go.ast.internal.tests, internal.coverage.test;
     - 10 relocation anchors: crypto.internal.{alias,bigmod,edwards25519,edwards25519.field,mlkem768,nistec}, internal.{concurrent,weak}, runtime.internal.{math,sys};
     - 4 exclusion pages: crypto.internal.fips140deps, internal.copyright, net.internal.cgotest, runtime.internal.wasitest;
     - 3 rowless candidates: net.http, reflect, runtime.
   - The green-badge arm (214 agree with their pages) and the Source·C# arm (342) are predicted GREEN.
   - The instrument enforces a set EQUALITY the close amendment REFUTES by design (`push-nuget.ps1:264-368` vs rb :3068-3080). NEVER "fix" the tree to satisfy it: no page deleted, no badge invented.
3. If `<census-seat-ref>` is named: merge it `--no-ff -S`, with ALLOWED paths per RN-6. It must restate the four-number guard AND the fifth-number frozen-snapshot check (`push-nuget.ps1:846-861`, 218 rows vs 232 pages) as the NAMED IDENTITIES (rb :3074-3080; check-roster-format 2b2/2b3 are the model). Then:
   - `-VerifyOnly` must read rc 0 and `Tree is releasable`;
   - PLANT: add one real orphan page, which must be refused by name. Restore.

   Without the seat, H11 reads RED on this arm, NAMED; that is not a STOP of Part B.

## Report B, then STOP
Include:
- B1-B6 readings, with the rc, tree SHA and host for each;
- the plants' reds;
- the new and removed ID lists, with the deprecation text;
- the advisory feed table;
- the H11 verdict per rung;
- everything unverified.

Before STOP: assert abrel's `git status --porcelain` empty (unfiltered), then release `<coord-tmp>/abrel.lock`. abrel stays IN PLACE; Part C never works in it.

# COORD CHECKPOINT 2 (COORD's work)
1. Accept Report B. Push B's commits, if any (announce first). `<part-c-base>` must contain every B commit Part C needs (the B6(3) census-seat merge above all), because Part C cuts its own tree from it.
2. Rule RN-7, RN-8, RN-10, RN-11, RN-12 and RN-13, plus RN-14 and RN-15 if RN-12 = master after the cutover, and RN-5 and RN-17 if not ruled yet. Part C STOPS before C4 if RN-12 or RN-13 is missing.
3. Supply `<announcement-text>` (owner-approved) or `none`, and name `<part-c-base>`.

# PART C -- H12 (docs, badges, READMEs, and the release-ritual REHEARSAL; rb :3160-3219)

## Fixed facts
- A NEW worktree for this dispatch (floor 11: one dispatch per worktree): assert `<part-c-base>` by ls-remote, then `git worktree add -b i7/h12-<mmddHHMM> <coord-tmp>/abh12 <part-c-base>`, then `rc=$?`. Assert `git -C <coord-tmp>/abh12 rev-parse --git-dir` != `--git-common-dir`. Lock `<coord-tmp>/abh12.lock` holding this dispatch's PID. Scratch `<scratchpad>/abh12/`. Copy `<scratchpad>/abpost/env.sh` there and re-point its tree path at abh12; source it in every shell. `abrel` stays IN PLACE and read-only; assert no `<coord-tmp>/abrel.lock` remains (or that its PID is not running, counted by image path, never killed).
- Build go2cs.exe under the pin and check that `go version` = go1.24.13.
- The rehearsal runs ONLY in a THROWAWAY linked worktree created from abh12's HEAD after C6: `git worktree add --detach <coord-tmp>/abh12-dry <HEAD>`. It is removed at close-out, children-first, after its tracked count is asserted. A preflight build never runs in the leg's tree (rb :2568).

## C1 -- STATE THE EXPECTED BADGE DIFF, BEFORE ANY EDIT (the UNPUBLISHED LINE, rb :3165-3179)
Write the statement into the report BEFORE C2 starts. Its contents:
- the H2-following pair: expected diff ZERO before the publish;
- the H1-following pair: expected diff EXACTLY the 5 lagging READMEs, which are:
  - `testing` @1.23.12 and `unsafe` @1.23.1, both hand-owned;
  - `crypto/x509/internal/macos` and `internal/runtime/syscall` @1.23.1;
  - `vendor/golang.org/x/net/route` @x/net v0.25.1-0.20240603202750-6249541f2a6c;
  - the last three are the seed survivors, RN-5;
- AT THE PUBLISH: 342 + 214 retarget to 1.24.13.1;
- the 21 dangling links, by name: crypto.hkdf; crypto.internal.fips140.{aes,bigmod,ecdh,ecdsa,edwards25519,edwards25519.field,mlkem,nistec,rsa}; crypto.internal.sysrand; crypto.mlkem; crypto.pbkdf2; crypto.sha3; internal.pkgbits; internal.runtime.{maps,math,sys}; internal.sync; unique; weak.

Never hand-retarget a Tests link or a Source·C# badge before the publish.

## C2 -- THE HAND-OWNED READMEs, DERIVED (rb :3180-3182; obligation c11)
- For `testing` and `unsafe` (both in `nonConvertedStdLibPackages`, `stdLibConverter.go:235`; the converter emits no README for testing, LEDGER 04:03; the 1.23.12.3 precedent `b6746ab185` hand-derived it), COMPOSE the Docs and Source·Go lines from `readmeValidationBadge.go`'s forms at go1.24.13 (`readmeDocsBadgeLine` :302-334, `readmeGoSourceBadgeLine` :227-256). PROVE them against the converter's own output for a CONVERTED sibling: `src/core/testing/fstest/README.md:5-6` reads @1.24.13, and A4(6) proved B's emission reproduces it byte-identical. With only the import path substituted, the composed lines must equal the sibling's, CR-stripped. Edit only those two lines per README, then RE-COMPOSE as the CONTROL: it must now byte-compare equal. Never convert `<goroot>/src/testing`: it would mint a second `testing_package` (`stdLibConverter.go:212-219`). An emission control needs a COORD RULING (RN-17). If RN-17 rules one, spell it exactly:
  - `git -C <tree> archive -o <scratchpad>/abh12/hand.tar HEAD src/core src/version.props src/Directory.Build.props docs/validation; rc=$?` (tracked-only; corpus-reconvert SKILL :86 seeds version.props and docs/validation too);
  - `mkdir <scratchpad>/abh12/hand && tar -xf <scratchpad>/abh12/hand.tar -C <scratchpad>/abh12/hand --force-local; rc=$?`;
  - `touch <scratchpad>/abh12/hand.stamp; export GOROOT='<goroot>' CGO_ENABLED=0; "<tree>/src/go2cs/go2cs.exe" -go2cspath "$(cygpath -w <scratchpad>/abh12/hand/src)" '<goroot>\src\testing' "$(cygpath -w <scratchpad>/abh12/hand/src/core/testing)" > <scratchpad>/abh12/hand-testing.log 2>&1; rc=$?` (the output dir is the SECOND positional and lies under `-go2cspath`'s `core`, the only single-package case that emits a README: `projectFileWriter.go:241-250`, `:628-630`; floors 2, 3, 6);
  - assert `<scratchpad>/abh12/hand/src/core/testing/README.md` is newer than hand.stamp before comparing; exactly one conversion into this root; the control re-run re-extracts a fresh root.
- The three seed survivors follow RN-5. DEFAULT: take their READMEs from the linux or darwin emission A4(10) copied to `<scratchpad>/abpost/survivors/` (verify each sha256 first), as a named H12 restore class, H1-following lines only.

## C3 -- VENDORED x/* PINS (rb :3183-3184)
Compare every vendored README's Docs and Source badges with `<goroot>/src/vendor/modules.txt`. The expected pins are x/crypto v0.30.0, x/net v0.32.1-0.20250304185419-76f9bf3279ef, x/sys v0.28.0 and x/text v0.21.0. Predicted: 17 of 18 hold, and x/net/route is OWED (C2's RN-5 class). List any mismatch by name.

## C4 -- THE GO VERSION IN PROSE (rb :3185-3192; obligations d4, a10)
1. Run a BARE census by the Windows PowerShell 5.1 shape in Rules (`#Requires -Version 5.1`): `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <tree>/src/migrate-gorelease.ps1)" -From 1.23.12 -To 1.24.13 < /dev/null > <log> 2>&1`, then `rc=$?`. It changes nothing. Post every DOC-STATEMENT site with its state.
   - Predicted: the roster anchor "packages whose Go {OLD} sources define" reads MISMATCH, because roster :664 was reworded. `-Apply` would therefore REFUSE.
2. Edit per RN-10, in present tense for visitors. History stays in NEWS and the records; the BOARD is never edited. The known sites at the tip:
   - `docs/README.md`: :12, :32-37, :111-120 (the go1.23.12 sample links), :461, :481;
   - `docs/Roadmap.md`: :31, :105;
   - `docs/Background.md`;
   - `docs/ConversionStrategies*.md`: :18210, :18229 (3,243), and "the 8 entries" -> 5;
   - the roster: :4, :71, :121-122, :172, :36-42.
3. Census `git grep -nE '3,?24[23]\b'` and class each hit: (a) a present-tense figure to re-derive (roster :71; Reference :18210/:18229; the crypto/tls manifest `reason` and `hostConditional`, `src/core/crypto/tls/go2cs_test_disclosures.json:14-15`); (b) a dated 1.23.12 derivation that stays; (c) a present-tense figure in a code comment (at the tip: `src/_roster.ps1:1142,1168,1240,1242,1400-1401`; `src/check-roster-format.ps1:343,501`; `src/run-validated-sweep.ps1:630`). Close obligations a10/d4 make (a) and (c) H12 work. COORD RULING NEEDED (folded into RN-10): whether (c) is edited here (repoguard owed; CNR not owed for `.ps1` comments), and confirmation that the manifest prose edit leaves every `signature` byte-identical.
4. CLAUDE.md:69 ("mid-hop") changes only per RN-10, inside TestContextBudget's caps.
5. After every edit: the guard (A6's pwsh 7 shape) reads rc 0 and `0 of N` (2e keeps the featured NEWS block equal to the header), and the index dry run (`python "$(cygpath -w <tree>/docs/phase4/hopA-inputs/regen-validation-index.py)"`, then `rc=$?`) shows no change.
6. rb :3186-3192 records that H12's "top-level docs" line is a NAMING DEFECT that "wants rewording at the next pass". Reword it per RN-10, or route it by name to the owner-ordered lessons-learned seat (LEDGER 2026-09-23 14:52); obligation H12-9.

## C5 -- THE ANNOUNCEMENT (ritual element 1; rb :3201-3209)
- With `<announcement-text>` supplied: commit it on abh12 BEFORE any rehearsal or release (precedents: `ca4066a74c`, "NEWS-before-tag per the release ritual", an ancestor of nuget-1.23.12.1; `78d8894e5a` + `88def0eff2`, ancestors of nuget-1.23.12.3's `b6746ab185`), as `docs/NEWS.md` plus the `docs/README.md` NEWS block. It names 1.24.13.1, `validation/1.24.13.1` and `nuget-1.24.13.1`, with every figure as the guard derives it.
- Gates: the guard `0 of N`; census `entry`; repoguard `TestNoFleetIdentifiersInTrackedFiles` with a plant. The TIP's Go guard lacks master's public-handle admit (`dd18e5e2ab`), so a NuGet handle outside a github.com URL may refuse there.
- Without the text: draft it to `<scratchpad>/abh12/announcement-draft.md` for COORD and the owner, and report ritual element 1 as OWED.

## C6 -- THE PRE-FLIGHT, AFTER THE LAST ROSTER- OR BADGE-MOVING COMMIT (`release-nuget.ps1:36-49`)
Re-run B6(2)'s command at abh12's HEAD. The gate is rc 0 and `Tree is releasable`; it is predicted RED without the census seat. On a red, run C7 only to record the first failing line. Predicted: `PRE-FLIGHT FAILED -- 21 problem(s)`, the throw at `push-nuget.ps1:362`, rc != 0, within seconds. Phases (b)-(i) NOT REACHED; elements 2-5 UNEXERCISED; P5 NOT DISCHARGED.

## C7 -- THE RELEASE-RITUAL REHEARSAL (parity gate P5; §6 "release-ritual dry run"; PLAN :558)
1. **Setup.**
   - Create `abh12-dry` (Fixed facts), then `rc=$?`; assert its HEAD == abh12's HEAD, take disk >= 25 GB, and count go2cs.exe = 0.
   - The HOST per RN-8. DEFAULT: `powershell` (Windows PowerShell 5.1), which is the host release-nuget spawns. ONE parent, bash: source the re-pointed `env.sh` (Go pin: bin first, GOTOOLCHAIN=local, CGO_ENABLED=0, GOFLAGS empty; the dotnet10 pair), then `export GOROOT='<goroot>'`, and apply the CREDENTIALS rule with `env -u`. If UTF-8 console output is wanted, use a per-run driver `<scratchpad>/abh12/rehearsal-driver.ps1` (the cnr-driver shape): it sets the pins and `[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false)` inside, runs `& powershell -NoProfile -ExecutionPolicy Bypass -File <push-nuget> -OutDir <dir> *> <log>`, and ends `exit $LASTEXITCODE`, which bash reads with `rc=$?`.
   - Budget 18-47 min on the i7, from the top of the range (MILESTONE-75pct-prep §3.3). BACKGROUNDED (Rules, the 600 s cap).
2. **Run.** `env -u NUGET_API_KEY -u NuGetCertFingerprint powershell -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w <coord-tmp>/abh12-dry/src/push-nuget.ps1)" -OutDir "$(cygpath -w <scratchpad>/abh12/rehearsal/nupkg)" < /dev/null > <scratchpad>/abh12/rehearsal/run.log 2>&1`, then `rc=$?` on the NEXT line, written to `<scratchpad>/abh12/rehearsal.result`. This is the PACK-ONLY dry run: NO `-Push`, NO `-BumpBuild` and NOT `-WhatIf`. Read the log's tail FIRST (floor 14).
3. **PREDICT every phase line in writing BEFORE the run**, then score it. Predictions (b)-(i) hold ONLY with `<census-seat-ref>` teaching BOTH the four-number guard (`push-nuget.ps1:324-373`) and the fifth-number check (`:846-861`). With neither half, the run stops at (a): `push-nuget.ps1:362` throws, (b)-(i) are UNREACHED (never RED), elements 2-5 score UNEXERCISED (blocked at (a)) with the throw line quoted, and the readiness report names P5 NOT MET. With the four-number half only, (b)-(d) run, (e) throws at `:853`, and (f)-(i) are NOT REACHED.
   - (a) the pre-flight: the `Release census` line (red per B6 without the seat);
   - (b) the metadata gate, `TestStdLibMetadataInSync` under the pin;
   - (c) `No build-number bump this run -- not tagging (the run that bumps mints nuget-1.24.13.1)`: ritual element 2 is NOT exercised (RN-7);
   - (d) `Froze N ... would-be 1.24.13.1 (temporary)`: element 3, into TEMP only;
   - (e) `Frozen snapshot self-consistent: R row(s) / P page(s)`: predicted to THROW at 218 vs 232 unless the seat taught it;
   - (f) `Retargeted N README badge link(s) to 1.24.13.0` and `Retargeted N C# Source badge(s) to 1.24.13.0`: element 4, both halves; predicted 214 and 342. At counter 0 `$fullVersion` = 1.24.13.0, a version that exists nowhere (`push-nuget.ps1:440`; the retargets at :883-899 and :952-972 are gated by ShouldProcess, not by `$dryRun`);
   - (g) `Verified N green badge(s) against the would-be 1.24.13.1 proof pages` (214) and `Verified N C# Source badge(s)` (342): element 5;
   - (h) the linux-x64 then win-x64 Release builds (darwin is not shipped), with the flavour comparison P = 0;
   - (i) `Layout L3: N package(s)`, the merge line, `Packed N`, and `Pack-only (default)` with rc 0. The packed nupkg ID set, BY NAME, equals the nuget-1.23.12.3 set minus B4's 14 removed plus B4's 51 new; the predicted count is derived, never carried. Name every extra or missing ID.
   - Known blind spot: the pack includes NO VALIDATION.md, because the Exists() guard reads `docs/validation/1.24.13.0/`.
4. **DISCARD.**
   - Nothing from abh12-dry is ever committed: its READMEs now name 1.24.13.0.
   - Assert that abh12's `git status --porcelain` is still empty and its `git ls-files | wc -l` is unchanged.
   - Assert the TEMP snapshot dir is gone (the script's `finally`, `push-nuget.ps1:922-930`).
   - Assert `git -C <coord-tmp>/abh12-dry rev-parse --git-dir` != `--git-common-dir` (a child, floor 12) and record `git -C <coord-tmp>/abh12-dry ls-files | wc -l`. Purge its bin/obj. Its retargeted READMEs make a plain remove refuse, so run `git -C <coord-tmp>/abh12 worktree remove --force <coord-tmp>/abh12-dry`, then `rc=$?`, and `git -C <coord-tmp>/abh12 worktree prune`. Assert `git worktree list` no longer shows it and abh12's `ls-files | wc -l` is unchanged.
   - Delete `<scratchpad>/abh12/rehearsal/nupkg` after the counts are read.
5. **ELEMENT SCORE.** Score each of the five elements EXERCISED, UNEXERCISED (named) or RED (first failing line quoted).

## C8 -- THE UNTOUCHED STATE (immediately before the report)
Each must read empty or unchanged, after B5's known-hit plants re-run here (floor 13):
- `git tag --list 'nuget-1.24.13*'`: empty;
- `git ls-remote --tags origin 'nuget-1.24.13*'`: empty;
- no `docs/validation/1.24.13.*` directory at abh12's HEAD;
- version.props: 1.24.13 / 0;
- `git status --porcelain`: empty, with no ` D`;
- go2cs.exe count: 0.

Then release `<coord-tmp>/abh12.lock`. Leave abh12 (and abrel) IN PLACE. NOTHING PUSHED.

## READINESS REPORT (final message), then STOP. You never sign, tag, bump, freeze a real snapshot, push or publish.
Include:
- the base and every ref as asserted;
- `git log --oneline --first-parent <post-close-stamp>..HEAD` with `%G?`;
- H11 per rung (B1-B6), and H12 per rung (C1-C7), each with its rc, tree SHA and host;
- the C1 statement and the scored diffs;
- the C2 derivations with their controls;
- the C4 census table;
- the rehearsal's predicted-vs-measured table and the five-element score;
- the release that WOULD run: branch and tip (RN-12), version 1.24.13.1, tag `nuget-1.24.13.1` at the pre-pack HEAD;
- READY requires C6 rc 0 with `Tree is releasable`, and C7 printing (e) `Frozen snapshot self-consistent` and reaching (i) `Pack-only (default)` with rc 0. Otherwise NOT READY, and say why in one line: a real run would bump (`push-nuget.ps1:433`), mint `nuget-1.24.13.1` (`:535`) and write `docs/validation/1.24.13.1` (`:624`) before throwing at `:853`, leaving a tag push-nuget keeps (`:528-529`) and a snapshot the counter guard reads as a recorded release;
- the critical path: if RN-12 = master after the cutover, the release waits on P2's sweep (RN-14) and P4's gate (RN-15); state which;
- the owner's run: `release-nuget.bat` at the i7's PHYSICAL console (never over RDP), in the tree RN-12 names, at the HEAD CP3 pushed. That tree's `git status --porcelain` must be EMPTY, untracked files included (`release-nuget.ps1:126-131`); the main checkout currently carries `?? du.exe.stackdump`. Console block: the go1.24.13 SDK `bin` first on PATH; GOROOT=`<goroot>` (backslash); GOTOOLCHAIN=local; CGO_ENABLED=0; GOFLAGS empty; DOTNET_ROOT=`<dotnet10>`, first on PATH (`push-nuget.ps1:404` runs `go test` on the ambient toolchain, and the machine GOROOT is the dead 1.23.1 pin); the two User-scope credentials present in the process, never printed; the GPG agent warm by the probe (the tag at `:535` is fatal after the bump at `:433`). Interactive points: the GPG pinentry if cold; the card PIN, which may re-prompt (the 1.23.12.3 run prompted twice, and two PIN dialogs dismissed on an RD session cancelled two packages: never touch a PIN dialog on an RD session); the typed word `publish` (`release-nuget.ps1:214-217`);
- the record commit the owner or COORD makes after the push: version.props (counter 1) + `docs/validation/1.24.13.1/` + the retargeted READMEs TOGETHER, staged by explicit path; release-nuget's printed `git add … src/core` is a directory add, so read the porcelain first. Announce, then push the commit and the tag;
- after the push: the guard reads counter 1 green, one nuget.org README is spot-checked, any "repairing with a direct project build" line is recorded, and the index's Frozen snapshots row is added BY HAND (the tool keeps that table verbatim; precedent `d03a4d85fb`);
- the OWNER HANDS: the card PIN (and the GPG pinentry if cold, and the typed `publish`); the 14 deprecations; the go.* prefix answer; approval of the announcement;
- the open rulings that still gate the release (RN-6, RN-7, RN-12..RN-15);
- disk;
- everything not verified.

# COORD CHECKPOINT 3 (COORD's work, before the owner's run)
1. READY only if C6 is rc 0, C7 reached (i) with (e) printed, element 1 is committed, and RN-6, RN-7, RN-12 and RN-13 are ruled. Otherwise NOT READY.
2. ANNOUNCE, then push abh12's commits (C2, C4, C5) as a fast-forward onto the RN-12 release branch (floor 9). Write the ledger line with the time from `date`.
3. LANDING FREEZE on the release branch until the tag and the record commit are on origin. Any commit that lands anyway re-opens C5 (the announcement figures) and C6.
4. In the exact tree the owner will run in: assert HEAD == the pushed tip and `git status --porcelain` EMPTY (untracked included); re-run B1, B2, `release-nuget.ps1 -VerifyOnly` (rc 0, `Tree is releasable`) and C8's two tag checks; probe GPG.
5. Hand the owner the console block from the readiness report.
6. After Phase 3: the record commit and the tag push, a ledger RELEASE stamp, then lift the freeze.

# OBLIGATIONS LEDGER (a checker verifies each row is discharged, owed by name, or routed)
| id | stage | gate (predicted) | source |
|:--|:--|:--|:--|
| PC-1 | A1.1 | merge clean; paths ALLOWED; C2 arm red-first then green | L 18:11, 18:23; STATE DELTA 28 |
| PC-2 | A1.2 | merge clean; two arms red-first then green; 22 L3 csprojs round-trip | L 18:39, 19:01 |
| PC-3 | A1.3 | docs/rules only; TestContextBudget green; runbook re-read | L 21:43, 22:40 |
| PC-4 | A1.4 | rides only by R-P1; guard derives linux line | L 22:36, 22:44 |
| PC-5 | A1.1 | corpus-reconvert SKILL :86 amendment carried, or OWED by name | L 18:23 |
| PC-6 | A2 | suite rc 0; repoguard plant red; identity sha != 7014b789… | rb §6 :3601; floors 7, 13 |
| PC-7 | A3 | freeze; porcelain empty; ignored-residue census zero `!!` with a plant | rb :2999-3001; floor 4 |
| PC-8 | A4(1)-(4) | arm A per R-P2 (no default), sha == 7014b789… or e0b2a4c1…; per-arm checks; determinism control with plant; staged snapshots on B | rb :3005-3016, :753-812 |
| PC-9 | A4(5)-(6) | A-only 337 (nonempty); B-only 0; T4 2; (iv) 1; T5 0 with plant; 337 = 336 + 1 | rb :3017-3031; L 22:30 NEXT |
| PC-10 | A4(7) | seed-survivor diffs posted by path | platformEmit.go:634-655 |
| PC-11 | A4(8) | absent set empty (spelled out, quotePath off, plant); H5c 0/0/0 on three flavours via powershell 5.1 | rb step 8; :1083-1096 |
| PC-12 | A4(9) | zero footprint, or STOP per R-P3 | rb :3023-3040 |
| PC-13 | A5 | CNR rc 0 + NO REGRESSION, CHANGED empty; copy beside the original, then removed | rb §6 :3602; floor 10 |
| PC-14 | A6 | guard 0 of N, figures unchanged or guard-derived; index no change | L 22:30 |
| PC-15 | CP1 | STAMP announced before push; tally in the ledger line | floor 9 |
| H11-1 | B1 | counter guard green + plant; 1.24.13 / 0 | rb :3140, :249-266 |
| H11-2 | B2 | numeric compare script over `^nuget-\d+(\.\d+){3}$` tags, non-release tags listed by name; lexical and reversed plants red; excluded-tag plant; home per RN-9 | rb :3141-3151; PLAN :661 |
| H11-3 | B3 | go1.24.13; guard tests green | rb :3152-3154 |
| H11-4 | B4 | new/removed IDs by name; deprecation text; feed advisory | rb :3155-3156; PLAN :663; L 09-22 19:06, 19:24 |
| H11-5 | B5 | 342 / 214 / 320; no 1.24.13.* snapshot or tag | rb :3157-3158; PLAN :662 |
| H11-6 | B6 | -VerifyOnly red-first naming 21; green only with the seat (RN-6) | RESUME-SESSIONS :879-883; L 09-22 16:31 |
| H12-1 | C1 | expected diff stated first: H2 pair ZERO; H1 pair 5 named; 21 dangling named | rb :3162-3179; d2, d3 |
| H12-2 | C2 | testing/unsafe composed and proved against a converted sibling + control (RN-17); seed survivors per RN-5 from A4(10)'s copy | rb :3180-3182; c11 |
| H12-3 | C3 | 17/18 vendored pins hold; route named | rb :3183-3184 |
| H12-4 | C4 | bare migrate-gorelease census; prose per RN-10; guard green | rb :3185-3192; d4, a10 |
| H12-5 | C5 | announcement committed before any tag, or OWED | rb :3201-3209 |
| H12-6 | C6 | -VerifyOnly after the last moving commit | release-nuget.ps1:36-49 |
| H12-7 | C7 | pack-only dry run in a throwaway tree; five elements scored; tree discarded | rb :3193-3219, §5 :3590, §6 :3610; PLAN :558 |
| H12-8 | C8 | no nuget-1.24.13* tag anywhere; counter 0; no snapshot | push-nuget.ps1:508-542 |
| H12-9 | C4 or POST | rb :3186-3192: reword the "top-level docs" line per RN-10, or route it by name to the owner-ordered lessons-learned seat | rb :3186-3192; L 14:52 |
| REL-0 | CP3 | announcement pushed to the release branch BEFORE the owner's run; freeze; final re-reads at the release HEAD | rb :3201-3209; push-nuget.ps1:535 |
| REL-1 | owner | release-nuget phases 0-4; GPG pinentry if cold, card PIN (may re-prompt), typed `publish` | release-nuget.ps1:113-241 |
| REL-2 | COORD/owner | record commit together; guard counter 1; index row by hand; snapshot to master for Pages | release-nuget.ps1:223-238; d03a4d85fb |
| PAR-1 | cutover | §5 P1-P5 checked (P1 and P3 at the close STAMP; P2 owed = PAR-2's sweep; P4 and P5 owed) | rb :3579-3590; PLAN :555 |
| PAR-2 | cutover | §6 one full-roster sweep, not on the i9 | rb :3609; d6 |
| PAR-3 | cutover | H6 completeness gate + T4 .cs.auto refresh | rb :1292-1339; d7 |
| PAR-4 | cutover | version -> master merge sized; fleetIdentifierCensus_test.go UNION by symbol | rb :1469-1527; c10 |
| PAR-5 | cutover | gofmt pass; suite + CNR | L 09-22 08:37; d15 |
| POST | post-release | synctest train, REC-*, preload, string-literal, IVT union, lessons-learned | STATE DELTA 28; L 14:52 |

# COORD RULINGS NEEDED (each is also named where it bites)
- **RN-1** G's crypto/tls linux rebank: in Part A, or post-release (L 22:44: COORD rules at assembly).
- **RN-2** Arm A of the proving regen. NO default; a missing ruling STOPS Part A before A4(1). Either `fa18863b94`'s converter, identity `7014b789…`, the brief's reading of rb :3010-3012 ("the last corpus-wide regen"), or COORD's dispatch pin `REGEN_BASE 54dec61728`, identity `e0b2a4c1…`, with A-only of about 455. Record it as §6's fourth seeded reconvert.
- **RN-3** A non-zero regen footprint: STOP (DEFAULT) or an in-batch overlay with step 9's full gate set.
- **RN-4** Where a zero-footprint regen's readings live. DEFAULT: Report A plus the STAMP ledger line. The alternative is a committed docs/phase4 record (rb :1136-1138, "ask COORD").
- **RN-5** The three platform-exclusive seed survivors (macos, runtime/syscall, x/net/route: stale badges, and csprojs without the package_info-first order): a pre-release converter seat, an H12 restore class (C2's default), or post-hop.
- **RN-6** The release-census instrument seat. It teaches push-nuget's four-number guard and fifth-number check the named identities, and it must land before H11 goes green and before the rehearsal. The ruling covers who cuts it, on which branch, and whether it rides a batch. It also decides whether the 1.24.13.1 snapshot freezes all 232 pages (anchors and exclusions included), and whether a counter-0 dry run keeps writing `<base>.0` into READMEs. Ruled and DISPATCHED at CP0, in parallel with Part A (the seat touches none of Part A's paths); CP1 only names `<census-seat-ref>`.
- **RN-7** Ritual element 2, the pre-pack signed tag. DEFAULT: UNEXERCISED by the pack-only run, stated, with the proof being the owner's run. The alternatives are an isolated CLONE with no remote (`<coord-tmp>/abh12-clone`, the NEVER RUN list's one `-BumpBuild` carve-out), `-BumpBuild` and a local tag, discarded afterwards; or MILESTONE §3.6.3's redirect copy. DEFAULT consequence: §5 P5 is NOT MET before the publish (no dry run mints a tag, MILESTONE-75pct-prep.md:421), so RN-13's cutover follows the publish unless COORD records a ruling that the partial rehearsal discharges P5.
- **RN-8** The rehearsal host. DEFAULT: Windows PowerShell 5.1, the release's own host; guard obligation d22 is host-dependent. The alternative is pwsh 7.
- **RN-9** The home of the H11 monotonicity script. OQ-11 (PLAN :661) says the `NuGetVersion.Compare` assertion "is added to H11"; COORD rules whether that means a committed instrument. The scratchpad script, with its output in the report, is an INTERIM, not the ruled home. The alternative is a committed guard.
- **RN-10** The H12 prose: re-anchor or retire migrate-gorelease's rotted roster anchor, or hand-edit the DOC-STATEMENT sites. Also decide which roster lines (:4, :172, :664) are present tense and which are the 1.23.12 record; and when CLAUDE.md:69 changes, and by whom, under TestContextBudget, given that it merges at the cutover. Also (C4(3)): whether the present-tense 3,243/3,242 figures in CODE COMMENTS (class c) are edited at H12 (repoguard owed; CNR not owed for `.ps1` comments), and confirmation that the crypto/tls manifest prose edit leaves every `signature` byte-identical. And (C4(6)): reword rb :3186-3192's "top-level docs" line here, or route it to the lessons-learned seat.
- **RN-11** The announcement: who drafts it, what it says about net/http's demotion and the linux figures, and the owner's approval before the tag.
- **RN-12** THE RELEASE BRANCH: the version tip after H12 (the tag names the exact tree, but Pages and index links 404 until master catches up), or master after the §5 cutover (release-nuget's expectation, and the site stays coherent). Ruled at CP2, before Part C's C4: it decides which branch C4's prose and C5's announcement land on, and what the announcement can honestly say about `validation/1.24.13.1`. If (b), the release waits on P2's sweep (RN-14) and P4's gate (RN-15); the readiness report states that critical path.
- **RN-13** THE CUTOVER, version into master: before or after the publish (under RN-7's DEFAULT, after), and its mechanics. Ruled at CP2, before Part C's C4. Is it a `--no-ff -S` merge? Where is it sized, given that the i7's git lacks `--write-tree`? It must carry the UNION of `fleetIdentifierCensus_test.go`: master's public-handle admits plus the tip's embed-payload admit. How promptly does the snapshot reach master?
- **RN-14** §6's one full validated-roster sweep: before the release, or only before the cutover? On which machine (never the i9)? Does it carry crypto/tls's raised-wall environment pin? Is `-VerifyOnly` re-run after it?
- **RN-15** The remaining parity items. H6's completeness gate script (rb :1332-1333: "COORD rules the gate script"), including row 130's (c) and the T4 `.cs.auto` refresh. The §5 P3 line for the ruled base two. The timing of the gofmt pass (d15). Whether close items c6-c9 gate the release or go post-hop.
- **RN-16** Owner-hand timing: the 14 removed-ID deprecations before or after the publish; the `go.*` prefix answer; and whether `release/go1.23` (at `10c78227a7`) advances to `nuget-1.23.12.3`, and whether `release/go1.24` is minted.
- **RN-17** C2's hand-owned README derivation for `testing` and `unsafe`. DEFAULT: COMPOSE the two H1-following lines from `readmeValidationBadge.go`'s forms and prove them against a converted sibling's lines (`testing/fstest`), re-composed as the control. The alternative is an EMISSION control (a single-package conversion of `<goroot>/src/testing` into a seeded scratch root, spelled out in C2), which converts a package `nonConvertedStdLibPackages` deliberately keeps out and mints a second `testing_package` in that root.
