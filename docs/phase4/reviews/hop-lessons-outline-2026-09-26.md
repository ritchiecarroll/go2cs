# OUTLINE: "Lessons from the 1.24.13 hop" (C1, 2026-09-26), for COORD's ruling before any prose

**The seat.** COORD, 2026-09-26: one dated section in `docs/GoCorpusMigration.md`, plus in-place procedure edits
wherever a lesson changes a step. It lands on the ref `claude/c1-hop-lessons` from master `db1bd885a2`, docs only,
and rides TRAIN C. This file is the outline only; no runbook prose is written yet.

**How it was derived.**
- **Sources.** Seven read-only agents read the whole population:
  - the ledger, 2026-09-20 → 2026-09-25;
  - the mailbox archives, 2026-09-07 → 09-20, in four slices;
  - the BOARD entries dated 09-07 → 09-25, at `db1bd885a2`;
  - the runbook's 31 dated blocks at `db1bd885a2`, which also produced a coverage map of what the runbook already
    says;
  - the RESUME-SESSIONS STATE DELTAs and the 41 briefs and reviews on `claude/coord-handover`.
- **Candidates.** 449 in all: 389 RUNBOOK and 60 BOARD. Each carries a verbatim quote and its line.
- **Check so far.** A script found every quote in its source, allowing for markup:
  - 9 quotes sit a few lines off the cited line;
  - one miss was a parser artifact;
  - 16 more cite two files on one line, and the script only searches the first; they get a hand check.
  Line numbers are corrected at the write.
- **Check still owed.** Before the prose: an adversarial pass on each RULED lesson, asking whether the cited line
  supports the claim and whether it makes the next hop quicker or smoother. The outline cites one or two primary
  sources per lesson; the full candidate files stay in C1's scratch.
- **Excluded.** Per the owner, the i9's hardware; the host lesson below is generic.

**Test applied.** A lesson reaches the runbook only if it changes what the next hop DOES, sooner or with less
rework. The general measurement discipline found along the way is not runbook material. The hop-specific one-offs
stay on the BOARD. (Both lists are at the end.)

## A. In-place fixes: step text that a dated block now contradicts (13, from the coverage map)

| step text at `db1bd885a2` | contradicted by | the edit |
|---|---|---|
| §2 L107 "Three orderings" | the AMENDED fourth ordering | "Four" |
| H0 L155-157 | no outgoing platform manifest (H8's comparand was produced mid-hop) | add to H0's capture list (see B2) |
| H4a L393-406 "owes, in one commit series" | the 2026-09-13 staging-baseline amendment | state both shapes |
| H5 gate L571, with no deletion step | the H5c amendments of 09-07 / 09-13 | H5c named inside the gate |
| H6 L1292-1302, a mechanical check | "no instrument" (the script is now `check-h6-completeness.ps1`) | name the instrument (see B8) |
| H7a L1565-1567 "one merge at one boundary" | the campaign-tip merge | two folds (see B10) |
| H9 L1925 and §5 P3 L3633 "all four phases green" | scored against the named base | "green against H0's named failing set" |
| H10 L2257-2259 "must equal the roster's row count" | the NAMED IDENTITIES | the identities |
| H10 L2324-2325 "the corpus axis is 226" | the §3.3 two-route population | the definition, with no number |
| H10 L2399-2407 "`testing` excluded from every list" | its own amendment (a wholly hand-owned package) | restate the rule |
| H12 L3207-3209 badges "follow H2" | badges follow the PUBLISHED stamp | fix |
| H12 L3230 "top-level docs" | the docs census-and-read (B15) | fix |
| §6 L3652 "seeded full reconvert: once per phase" | the close ran a third, and the post-close regen a fourth | "once per phase, plus one per late converter batch" |

## B. Lessons, by the step each edit touches (* = a new step or checklist item, not only a note)

**Pre-hop (a new "Readiness" checklist ahead of H0).**
- **B1\*** Qualify every host BEFORE H0, as a per-box record:
  - DNS on public IPv4 resolvers, with IPv6 unbound where there is no v6 path, qualified by Go's own
    `go test -count=1 -timeout 40m net`;
  - WSL `localhost` including ::1;
  - Developer Mode (unelevated symlinks) and LongPathsEnabled;
  - pwsh 7 and the .NET SDK on every worker;
  - a many-core host (local or cloud) for crypto/tls's standard BoGo wall; only GOFLAGS reaches BoGo's nested
    `go test`, so raising the wall is otherwise an owner ruling;
  - cloud quota filed early.
  - Cites: `2026-09-22 19:34 · FINDING · 43d149b87d`; `2026-09-24 00:33 · FINDING · 836004dd20`;
    `2026-09-23 22:36 · FINDING · 020365abcd`; `2026-09-22 00:05 · RULING · 0adf2e431`;
    `2026-09-22 21:55 · FINDING · 7462befde0`; `2026-09-23 14:52 · OWNER ORDER · c9e15c73a0`;
    briefs/az1-tls-brief.md:44.
- **B2\*** Land and red-prove the hop's instruments BEFORE the stage that needs them:
  - the recon wrapper, as one blob on master, rehearsed with a made-to-fail control; its two pin defects were fixed
    mid-hop at `d095fe8108`;
  - the sweep's `-Hop` mode;
  - the H6 completeness gate;
  - the H11 existence-plus-monotonicity check over local AND origin tags.
  - Cites: `2026-09-21 23:52 · RULING · 0adf2e431`; `2026-09-22 14:52 · RULING · 158ce37f6c`;
    `2026-09-24 02:10 · STAMP · bccf8d977b`; mailbox-archive.md:113793.
- **B3\*** Fleet operations from day one:
  - addressed comms (the ledger, per-lane inboxes, 40 lines or fewer) and one message per event;
  - post tools with a dry-run admission test and a delta census;
  - one OWNER-HAND line per owner action;
  - save-state points planned against the weekly limit, with CHECKPOINT commits;
  - a cloud lane's state is only what is at origin plus its resume prompt.
  - Cites: mailbox-archive.md:115567; `2026-09-22 00:09 · RULING · 93c9f43a6`; mailbox-archive.md:38414, :41717,
    :42569.
- **B4** The outgoing record, before H2:
  - commit every unbanked reading;
  - the freeze snapshot carries the roster and links to the tag;
  - derive the "must land first" list from master, not memory.
  - Cites: mailbox-0907-0913.md:8224-8226; `2026-09-23 23:41 · RULING · fa18863b94`. (Extends the fourth
    ordering.)
- **B5** Keep a next-hop notes record, and census known converter constructs against the NEXT release's tree.
  Cite: mailbox-archive.md:89386.

**H0.**
- **B6\*** Add to the capture list:
  - the outgoing `-platform-census` manifest (H8's comparand);
  - the behavioral suite's failing set BY NAME (H9's base);
  - the committed corpus's drift from the pre-hop converter, by class;
  - each box's bare `go` read from a directory with no go.mod.
  - Cites: mailbox-archive.md:76106, :79907, :42579, :54051.

**H1.**
- **B7\*** Every go-invoking instrument pins GOROOT with GOTOOLCHAIN=local and prints its toolchain.
  - Assert the pin three ways from a directory with no module. `-goroot` alone never steered the loader (the
    converter now refuses a mismatch). `GO111MODULE=off` cancels a GOTOOLCHAIN redirect.
  - Run `go vet` under the new directive early, and land the fixes it forces on master before the H1/H2 pair.
  - Read the release notes' GODEBUG default changes against the test host (1.24's winsymlink).
  - Cites: mailbox-archive.md:8633, :76147, :88151, :104090; mailbox-0907-0913.md:11008.

**H2.**
- **B8** Run migrate-gorelease's bare census as soon as any doc restructure lands, not at H2.
  - Guard H2's build-number reset with a repoguard arm, because it was skipped and the error reached 335 badges.
  - A registry re-key lands with the stage whose witness it reads: the GOROOT-witness key with H2, the
    corpus-witness key with H5.
  - Cites: mailbox-archive.md:1292, :79230; mailbox-0907-0913.md:12877, :15262.

**H3.**
- **B9\*** Every count states its axes:
  - the `-tests`/`-stdlib` tags (`purego,math_big_pure_go`);
  - `CGO_ENABLED=0`;
  - ReleaseTags pinned per side;
  - the GOEXPERIMENT baseline read from `internal/buildcfg` at the target.
  - File the NuGet prefix reservation for new package IDs as soon as H3 names them.
  - Cites: mailbox-0907-0913.md:7965; mailbox-archive.md:4337, :76149, :78200, :78653; resume.md:1078.

**H4 / H4a.**
- **B10** Pay the hand-owned testing host's bill for new TB/B members FIRST, sized by blocking call sites. Class
  each H4 item as "lands now" or "rides the hop". Cites: mailbox-0907-0913.md:8218, :8648, :29091.
- **B11** Re-key registry entries against the target by SIGNATURE, not by a similar name, and land target-only
  entries on the version branch with the H5 set. Cites: mailbox-archive.md:30274, :23933.
- **B12** Seat rules:
  - a converter seat re-baselines the goldens it moves, and its reach census covers `src/tests/Behavioral`;
  - predict a footprint from an unfiltered probe;
  - rule adjacent-insert union classes, and ONE resolver per converter chain, in advance;
  - C# from a lane without an SDK is built on a scratch merge before the version branch.
  - Cites: mailbox-archive.md:77806, :50525, :53580; `2026-09-22 15:09 · FINDING · f9f3c1039`;
    `2026-09-22 00:05 · RULING · 1271662ec`; `2026-09-20 15:35 · RULING · 6d814e2d3`;
    `2026-09-22 06:13 · RULING · 34d8a5be0`.

**H5 / H5c.**
- **B13\*** Rehearse H5 at the union tree with written predictions, reporting packages compiling beside the error
  count.
  - Commit progress as CHECKPOINTs even while the gate is red.
  - The build seed includes `src/gen`, `src/Directory.Build.props` and the whole of `docs/validation`.
  - The overlay carries the root attribution files.
  - `src/go2cs.slnx` follows the corpus in the checkpoint commit.
  - Cites: mailbox-0907-0913.md:12424; mailbox-archive.md:21015, :34119, :20916, :74972, :33699;
    `2026-09-23 18:11 · RULING · 922994cec3`.
- **B14\*** H5c's deletion selection is the CONVERTER's selection:
  - never a second tag resolution, and never mtime or staging-root presence;
  - it adds a hop-stale census (files no base arm wrote);
  - every class count is printed even at zero, and every post-condition asserts a non-empty population;
  - orphaned hand-owns relocate with their principal: `git mv` PLUS the namespace and class lines.
  - Cites: mailbox-archive.md:34588, :19838, :52550, :33716, :32383.
- **B15** Converter fixes landing after H5 owe a seeded reconvert. Measure each batch's corpus footprint AT the
  batch: the close regen inherited 154 unmeasured converter commits, and the release packs `src/core` as
  committed. Cites: `2026-09-22 08:18 · STAMP · ae2f25198`; briefs/h10-close-obligations.md:56;
  briefs/packaging-train-seats-2-3-review-2026-09-24.md:195.
- **B16** Expect this hop's declared-set guards (q82, RED 8 (d), q84) to fail at the next hop by design, naming the
  new members; budget for them. Cite: mailbox-archive.md:48061.

**H6.**
- **B17\*** Run the completeness gate (`src/check-h6-completeness.ps1`) at H6 AND at the close, and row each
  hop-time hand-own AS IT LANDS: 21 were unaudited at the release. Cites: `2026-09-24 02:10 · STAMP · bccf8d977b`;
  `2026-09-24 02:50 · STAMP · 33b6623eab`.
- **B18\*** New arms:
  - STRANDED hand-owns: a hop-new bodyless declaration in a package with a companion becomes a throwing stub. Run the
    q82 census.
  - A hand-owned host that quotes Go text (the testing host's panic strings) is re-diffed at the new pin.
  - Observers are re-derived at the pin.
  - A retired hand-own's class is re-censused in its replacement package, from merge-base.
  - Cites: mailbox-archive.md:48204, :41916, :64588, :65210; board.md:25361.
- **B19** A relocated, PRINCIPAL-CHANGED hand-own keeps the old release's shape, so its H6 re-derive precedes the H5
  gate. Read the gate by project, because each fix unmasks the next package. Cite: mailbox-archive.md:38214.
- **B20** The pair recipe, as the step text:
  - one `-trimpath -buildvcs=false` binary;
  - the bare `-stdlib` default tags;
  - the old `version.props` for half B;
  - a stage that starts empty of `.cs.auto`;
  - CR-stripped hashes.
  - Cites: mailbox-archive.md:35207, :35589, :34954, :41111.

**H7 / H7a.**
- **B21** The first post-H5 build STARTS H7:
  - walk each red's ProjectReference closure;
  - size a red by a whole-corpus census and cure it as ONE converter seat, never by hand patch;
  - a new release's test files are unmeasured until their first compile;
  - run the q82 census before claiming parity.
  - Cites: mailbox-archive.md:43032, :46095, :43029, :46521; board.md:25174.
- **B22\*** H7a:
  - fold master in TWICE, at H7→H8 and again after H10's tooling lands on master (the campaign tip), or land hop
    tooling on the version branch;
  - re-mint a conflicted EMITTED file, never take a side;
  - gate the fold before the push.
  - Cites: `2026-09-21 23:52 · RULING · 0adf2e431`; resume.md:741; mailbox-archive.md:70225, :70405.

**H8 / H9.**
- **B23**
  - Build `classify`'s input with the manifest builder.
  - Derive class counts per file.
  - Run the byte-identity views on linux or WSL, because each Windows view takes about 15 minutes.
  - Take platform-exclusive packages from their native emission, and diff every L3 csproj after the three-target
    regen.
  - Cites: mailbox-archive.md:78222, :79653, :79605; `2026-09-24 01:46 · STAMP · 61724860b4`;
    `2026-09-23 18:39 · RULING · 1a328f3ee8`.
- **B24** Gate H9 against H0's named failing set, with the prediction re-derived at the tip of record and on the
  banking platform. Cites: mailbox-archive.md:79907, :77496, :78845; resume.md:360.

**H10.**
- **B25\*** Order:
  - census successors and pre-stage conversion only;
  - then the recon leg (a throwaway detached linked worktree, `-tests -test-action all` per row, the wrapper by
    blob, derived floors, execution pins applied);
  - then the roster seat, the plan and the driver.
  - Cites: mailbox-archive.md:78194, :78562, :85580, :87189, :88148.
- **B26\*** Launch preconditions, stated in H10:
  - Go overrides per worker, and the .NET pins set INSIDE pwsh;
  - a DRIVER_EXIT marker, not `$LASTEXITCODE`;
  - the sibling-wait predicate excludes the harness's own pwsh hosts;
  - the host rule.
  - Cites: `2026-09-20 15:32 · RULING · 0adf2e431`; `2026-09-22 03:33 · STAMP · c6fdbe73c`;
    mailbox-archive.md:85605.
- **B27\*** Start the LINUX leg alongside the recon, not before the close, with GoTargetOS=linux and its privilege
  stated. Late, it found vgetrandom (25 of 30 rows) and a GolibTests linux build broken for eight days. Cites:
  `2026-09-23 06:18 · RULING · 5da426433b`; `2026-09-22 03:45 · RULING · c6fdbe73c`;
  `2026-09-23 10:13 · ACCEPT · ce065f8aa9`.
- **B28** Allocation labels run in a fixed order: a reading run, then the relabel, then the rulings, then page
  banking. The host records only the first nonzero AllocsPerRun unit. Cite: `2026-09-22 16:39 · RULING ·
  c8e6ca9034`.
- **B29** Roster arithmetic:
  - Tests equals the page's matched count, and Disclosed equals the page's disclosed count;
  - the header is derived from the table at every leg;
  - a page census runs before the close;
  - hand-owned READMEs go in the bank checklist.
  - Cites: `2026-09-22 15:42 · CORRECTION · 3469154a95`; `2026-09-22 04:14 · RULING · 854b94107`;
    `2026-09-22 14:45 · RULING · d5414aa151`; `2026-09-23 04:03 · STAMP · 1719e3b87f`.
- **B30**
  - Every leg or train battery runs the FULL converter suite.
  - Measurement commits are pushed before a page cites them.
  - A non-default environment (GOFLAGS) is recorded on the page.
  - Cites: `2026-09-22 08:18 · STAMP · ae2f25198`; `2026-09-22 22:11 · CORRECTION · 8179b65f16`;
    briefs/h10-close-obligations.md:93-94.
- **B31** Rule hold-or-demote as soon as a row's repair is sized beyond a batch (net/http: "two days" became 4 days
  to 3 weeks). Cite: `2026-09-22 17:27 · FINDING · 43d149b87d`.
- **B32** Derive the close brief from the runbook, and run a three-lens adversarial audit before the close. The
  existing close amendment covers this; it gets one line only. Cites: `2026-09-22 16:31 · FINDING · 3469154a95`;
  `2026-09-23 13:36 · RULING · 3ec2c9ff39`.

**§3 / §4 / §6 (the campaign and the sweep).**
- **B33** Keep the recon leg's per-row TSVs as the next hop's shard-map basis (the cost is a floor of intrinsic
  work, not the verdict count). Derive the population by two routes. Cites: mailbox-archive.md:13074, :106663;
  `2026-09-22 11:27 · RULING · f545b18d4d`.
- **B34** §4 gains two dispositions: a HOST-TIMING reading (a deferred allocation split between hosts), and a
  throwing-stub verdict read as infrastructure, which is cured by a refuse-by-name body. Cites: resume.md:182, :876.
- **B35** §6 sweep:
  - a detached survival canary before row 1;
  - `clean-bin -Root` passed explicitly, and `src/gen` purged separately;
  - `index.md` restored after every `-tests` run;
  - stale committed test artifacts named as a drift class;
  - disk preflight also for lane gates and between legs.
  - Cites: briefs/full-roster-sweep-brief.md:107, :118; `2026-09-24 04:01 · STAMP · 9c9b1a81af`;
    `2026-09-24 04:58 · STAMP · 5098289482`; `2026-09-22 12:24 · RULING · 71d03e812`.

**H11 / H12 / release.**
- **B36\*** H11 is declared AFTER H10 when packages relocate:
  - census the new and removed IDs;
  - removed-ID deprecations are an owner step AFTER the publish;
  - the release census holds named identities, and release-nuget shows the child's names.
  - Cites: mailbox-archive.md:84061; `2026-09-22 19:06 · FINDING · 43d149b87d`; resume.md:1180;
    `2026-09-24 02:10 · STAMP · bccf8d977b`.
- **B37\*** H12:
  - user-facing docs get a scan AND a full read, in five classes (migrate-gorelease anchors only a quarter of them);
  - re-run every transcript cold;
  - rehearse the tag mint in an isolated clone with no remote, and run the dry run in a throwaway tree;
  - list the badges expected to dangle until the publish.
  - Cites: `2026-09-24 00:25 · OWNER ORDER · fa18863b94`; briefs/docs-go-version-migration-plan.md:680;
    `2026-09-24 04:01 · STAMP · 9c9b1a81af`; briefs/postclose-h11-h12-brief.md:440;
    briefs/postclose-h11-h12-repair-log.md:44; briefs/h10-close-obligations.md:109.
- **B38\*** A "release day" block under H12:
  - Order:
    - master moves first, after the §6 sweep, and the release is cut from master;
    - nothing that moves golib or runtime rides between the sweep and the release;
    - release branches: `release/go1.N` advances, and the new line is minted at the next cutover.
  - The publish:
    - it is the owner's act at the physical console, from a tree with empty porcelain;
    - no GPG signing on that machine during Phase 2;
    - a failed Phase 2 resumes by the signer, never by re-running release-nuget.
  - Gates:
    - the walkthrough runs on linux AND windows from packages repacked at the shipping tree;
    - GolibTests is gated at Debug AND Release;
    - after the publish, "indexed" means the whole restore closure is listed.
  - Cites: `2026-09-24 00:04 · OWNER RULING · fa18863b94`; `2026-09-24 00:45 · ACCEPT · 0b6bf15ab0`;
    `2026-09-24 07:24 · ANNOUNCE · b6746ab185`; `2026-09-24 05:48 · RULING · 6ab65f3409`;
    `2026-09-24 15:57 · STAMP · e06494c6f8`; `2026-09-24 11:27 · STAMP · 53f52233fc`;
    `2026-09-24 14:10 · ANNOUNCE · 4c53b02a0a`; `2026-09-24 16:17 · STAMP · 9a5f63041b`.
- **B39** Packaging: multi-target packages need a per-RID COMPILE surface. It was fixed for 1.24.13.2 (seat 1
  `501b7e4c27`), and the linux walkthrough is now in the checklist (B38). Cites: `2026-09-24 06:44 · FINDING ·
  38529c765a`; `2026-09-24 14:10 · ANNOUNCE · 4c53b02a0a`.

## C. Not runbook: routed or dropped (asks for COORD's ruling)

- **ROUTE TO SKILLS, a separate docs seat, not this ref.** About 30 candidates are general measurement and
  instrument discipline:
  - a skip inside a guard is Fatal;
  - plant a member before trusting a zero;
  - `| head` on a state-writing tool;
  - `-run` against the wrong package;
  - NUL-delimited tree walks;
  - grep -Fi / -e aborts;
  - PS 5.1 `*>` UTF-16LE and ConvertFrom-Json case collisions;
  - `set -e` and `git -C`;
  - a counter needs a planted difference;
  - a census scope needs a widened control.
  They belong in `.claude/skills/gate-forensics` and `measurement-discipline` (CLAUDE.md's placement table), not
  in the runbook.
- **BOARD ONLY.** The one-off code findings, such as the RED seat details, alias collisions and single-row
  mechanisms: 60 candidates, plus the RUNBOOK-classed candidates that fail the quicker-or-smoother test.
- **ALREADY IN THE RUNBOOK.** H10 close lessons 1-7 (R-B13..R-B16 among them) are cross-referenced by one line,
  not restated.

**Asks.**
1. Rule the list: which of B1-B39 go in, and whether the starred items become step text as proposed.
2. Rule the skills routing (C1) as a separate seat or as part of this one.
3. Confirm the section's placement: a new dated section after §6, before "Sources", with in-place step edits
   (A and the stars).

## D. APPENDIX (2026-09-26, per COORD's ruling (2)): the SKILLS-ROUTING list, kept whole for its own seat

These candidates are general instrument and measurement discipline. They were not written into the runbook; they
are routed to `.claude/skills/gate-forensics` and `.claude/skills/measurement-discipline` in a SEPARATE seat,
after the lessons seat lands. The IDs are those of the candidate files in C1's derivation (`derive-<slice>.md`,
entry number). Each entry's full quote and provenance is carried into that seat.

| id | candidate | primary cite |
|:--|:--|:--|
| B#5 | A run directory does not measure what ran; the output's own control line does | board.md:25655 |
| B#10 | Scope a census, then run it once widened as a control | board.md:25369 |
| B#11 | Test a C#-signature classifier against the shapes it will misread before its first reading | board.md:25391 |
| B#12 | A sizing must read the guarding path as well as the emitting path | board.md:25378 |
| B#13 | Name the axes a precedent shares, then ask about the one it does not | board.md:25484 |
| B#14 | Footprint predictions owe the side effects: import aliases, map lines and counter-renumber tails | board.md:25418 |
| B#37 | A rate needs its runs enumerated and its power stated; a row that crashed once is not a calibration standard | board.md:24510 |
| B#38 | A drop in verdict count is tested for truncation by set comparison, and names the commit it sits beside | board.md:24354 |
| B#39 | Bisect along `--first-parent`, assert ancestry, and never invent a full SHA | board.md:24447 |
| B#40 | Count commits in an attribution window on a full clone | board.md:26021 |
| B#46 | A skip inside a guard is a Fatal; check that a control ran, not its exit status | board.md:25387 |
| B#47 | A zero from a predicate that has never fired is no evidence | board.md:25712 |
| B#48 | A negative result needs a control on the side of the answer, placed in the run | board.md:25833 |
| B#49 | A comparison against a missing reference answers yes | board.md:25541 |
| B#50 | Before spending a gate, show that the tree can exhibit the condition | board.md:25810 |
| B#51 | Read an edit back before running the arm it regresses | board.md:25398 |
| B#52 | Walk the tree with `ls-files -z` | board.md:25701 |
| L#57 | The real-root run is the gate, not a fixture spelled like the pattern | 2026-09-22 07:15 · LANDING · facb69304 |
| L#58 | Census false green: zero-byte tool files read as clean | 2026-09-20 15:29 · STAMP · 0adf2e431 |
| L#59 | CNR exited 0 without git | 2026-09-22 09:46 · STAMP · 598d1978a |
| L#60 | Silent false-clean text tools on large lists | 2026-09-22 07:05 · STAMP · 07240495e |
| M2#12 | The registry guards skip, and `go test` prints `ok`, when `src/core` is not beside the converter | mailbox-archive.md:29023 |
| M2#22 | A zero in an instrument must be a printed line, and a post-condition needs a population assertion | mailbox-archive.md:33716 |
| M2#23 | Merging the H5 set onto the version branch: a fast-forward or disjoint-path green is not a merge test | mailbox-archive.md:27554 |
| M3#5 | Seat census rules: port the converter's predicate, keep the gate's exclusions, scan `.cs.auto`, control both ways | mailbox-archive.md:45734 |
| M3#24 | H7 script: drive-letter paths under MSYS_NO_PATHCONV, and arms that gate on the command's rc | mailbox-archive.md:74104 |
| M3#32 | Merge rehearsals: a merge-tree "clean" can be an instrument artifact | mailbox-archive.md:57292 |
| M4#24 | Fire the gate's self-test on the scoring box first | mailbox-archive.md:76174 |
| M4#25 | Look for the surviving census output before re-running | mailbox-archive.md:77535 |
| M4#32 | A difference counter is not trusted unless something else can contradict it | mailbox-archive.md:82778 |
| M4#52 | Wrapper post-processing cost dwarfed the converter on big rows | mailbox-archive.md:110448 |
| M4#53 | Check a recon wrapper is alive by its PID's CPU delta, not a process-name census | mailbox-archive.md:95914 |
| M4#55 | A stale comparison record produced PASS | mailbox-archive.md:105615 |
| M4#56 | PowerShell 5.1 `ConvertFrom-Json` throws on subtest names that differ only by case | mailbox-archive.md:105873 |
| M4#69 | MSYS `grep -F -i` aborts, and a crashing guard fails open | mailbox-archive.md:75882 |
| M4#70 | A replica of a text-scanning predicate under-counts silently | mailbox-archive.md:77070 |
| M4#71 | Floor-1 converter censuses are blind to custom-named binaries | mailbox-archive.md:89750 |
| M4#72 | `/ head -N` on a state-writing tool can kill it mid-run | mailbox-archive.md:90755 |
| M4#73 | `-run` against the root package is a false green | mailbox-archive.md:86935 |
| M4#74 | A failed `cd` retargets the next git command at the wrong repository | mailbox-archive.md:89413 |
| R#10 | Launcher traps: a net8-apphost pwsh under a dotnet10 PATH, and an unset `$LASTEXITCODE` reading rc 0 | resume.md:803 |
| R#11 | GNU grep 3.0 with many `-e` patterns silently reads 0 matches when piped | resume.md:914 |
| R#12 | From PowerShell, bare `bash` is the WSL launcher; call Git Bash by its full path | briefs/dns-probe-NOTES.md:254 |
| R#13 | Windows PowerShell 5.1 traps: `*>` writes UTF-16LE, and one guard arm is red only under 5.1 (seed 10) | briefs/full-roster-sweep-repair-log.md:49 |
| R#14 | Every battery script records its own PID and takes a worktree lock at start | resume.md:1280 |
| R#23 | A reporting sub-agent's death is not the driver's: read the artifacts before re-running | resume.md:720 |
| R#33 | A lane driver's per-row budget must clear the instrument's worst case: 4 × pkgTimeout + 30 min | briefs/full-roster-sweep-repair-log.md:29 |

47 candidates.
