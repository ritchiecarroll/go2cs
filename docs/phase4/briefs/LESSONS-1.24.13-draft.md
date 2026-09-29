# LESSONS-1.24.13: the lessons delta for `docs/GoCorpusMigration.md`

Draft only; the runbook is not edited. I read everything at `origin/master` = `a1f133c3a9` (fetched 2026-09-29), the ledger
through mailbox `8c07a349b6` (2026-09-29 12:53), and `claude/coord-handover` at `9a11dff385`.

**How to apply.**
1. Land `4140a8e55d` first, as its own commit (E13).
2. Then apply the rest **bottom-up** (highest line first).
3. Anchor each edit by its quoted "Current" text. That text is verbatim at `a1f133c3a9`, backticks and indentation
   included. The line numbers are guides only, because E13 shifts every line after §3.1 by 35.

## 0. Placement: an addendum inside §7, not a second section

The owner's order (ledger 2026-09-23 14:52 · `c9e15c73a0`) is already mostly met:

- The runbook has **§7 "Lessons from the 1.24.13 hop — dated 2026-09-26"** (lines 3801-4031): 39 lessons, fixes A1-A13,
  and starred step text. It landed at `e7a3287d56`.
- The H10 close amendment has close lessons 1-7 (lines 3187-3253, `13e9b039ea`).
- A second "Lessons" section would break "ONE dated section". So the insert below is a dated sub-section, **§7.7**,
  inside §7.

**Insert §7.7 in two places:**

1. **The body goes after lesson 39.** Put it after lesson 39's HTML comment, which ends
   `…seed items 13, 14, 16, 17. -->` (line 4030). It sits before the blank line and the `---` above `## Sources`
   (lines 4031-4032).
2. **One sentence goes in §7's intro.** See edit E0.

**How §7.7 was chosen.** It holds only what §7 lacks:

- step text for lessons that recorded a rule without one: 1 (one clause), 13, 18, 31, 33 (a pointer), and 4 with 36
  (one §3.2 predicate);
- seven lessons found after 2026-09-26 or left in a comment (40-46).

Lessons 1-39 are not restated.

**Seed list, verified against the records.** All nine items had landed in some form. Items 8 and 9 owe nothing.
Items 1-6 have the step-text gaps below. Item 7's remainder is fleet discipline, which goes to skills. Nothing was
dropped as unsupported.

| # | Seed item | Verdict | Where it lands |
|:--|:--|:--|:--|
| 1 | Prepare hosts (DNS, many-core TLS host, quota) | KEEP; landed as lesson 1 and the Readiness Host bullet | One clause: per OS side (lesson 1's step text, E2) |
| 2 | Linkname partials surface first on linux (vgetrandom) | KEEP; landed as lesson 30 and "Order and launch" | Gap: the linux bank annotated only rows already annotated. Lesson 43, E9 |
| 3 | Reading run, relabel, rulings for allocation labels | KEEP; lesson 31 only, and H10 step 3 still says "TWO labels" | Lesson 31's step text (E7), with its extractor named as an instrument (E3) |
| 4 | Wrapper defects (floor lowered an asked timeout; pins), fixed at `d095fe8108` | KEEP; landed (the re-bank amendment, lesson 2) | Gaps: an accepted ref taken for a landed one; the `GoTargetOS` refusal ruled at 09-22 03:45 was never cut. Lesson 42, E3/E6/E10 |
| 5 | Push measurement commits (`4c5b0a3b7`) | KEEP; lesson 33 only | A pointer in "How a row banks" (E11). Re-verified: `4c5b0a3b7` is still not an object in the clone |
| 6 | The close derivation's catches (costed basis row, payloads plus `-text`, `git rm` of moved flat files) | KEEP; landed as close steps 5 and 8 and close lessons 1-3 | Moved earlier, to every regen from H4a: lesson 46, E5 |
| 7 | One message per event; sub-agent batteries with briefs | KEEP; landed as lessons 3 and 35 and Readiness "Fleet" | The battery-result and resume-refresh rules are fleet discipline. They go to the `gate-forensics` and `save-state` skills, as §7's intro routes that class |
| 8 | `go2cs.slnx` Release gate pins `-p:go2csPath=<repo>/src/`; fix harness-gates | DONE: §6 row, close step 9, close lesson 7, `.claude/rules/harness-gates.md:24` | Nothing owed |
| 9 | Payload admit; staging seeder carries `docs/validation`; csproj renderer keeps post-block groups | DONE: `de436207c9`/`33f5f11297`, `781c1c3c31`, `ae813db069`, all on master | They become regen checks in lesson 46 |

Excluded, as the owner ordered:

- the i9's hardware; lesson 1's new clause stays generic;
- comms-tooling items (ledger backticks, duplicate appends), which belong in skills and memory.

---

## 1. The insert: §7.7, verbatim

```markdown
### 7.7 Addendum 2026-09-29 — step text owed, and lessons found later

> **What this adds.** Lessons 1-39 stand. This addendum gives step text to lessons that recorded a rule without one
> (1, 13, 18, 31, 33, and 4 with 36). It then adds lessons 40-46, found after 2026-09-26 or left in a comment. Where a
> lesson changes a step, that step was edited in place on this date and carries a one-line note. The next hop's
> release-specific bill does not belong in this runbook, which names no release. It goes to the next-hop notes record
> (lesson 5).
>
> **Where the 1.24.13 hop's calendar went.** The hop took about 16.4 days from the outgoing publish to the incoming
> one. A weekly usage cutoff idled the whole fleet for about three of those days. A second took the coordinator
> offline for about 1.3 days mid-H10, while the lanes kept their queued cuts. H10 took about four days from its recon
> leg to its close. Its passes were cheap: pass 1 took about three hours on four workers. Its fix batches were not:
> thirteen of them, serial, each gated by a battery. Lesson 40 plans around the first cost. The second cannot move
> before H1, because a row reads only against a corpus that compiles (lesson 16).
<!-- Derived 2026-09-29 (read-only, at origin/master a1f133c3a9): from the ledger (claude/mailbox docs/phase4/LEDGER.md
     through 8c07a349b6), MAILBOX-through-2026-09-20.md, the handover record, and the branch state at origin. The seed
     list is the owner order's (ledger 2026-09-23 14:52 · OWNER ORDER · c9e15c73a0). Calendar: tag nuget-1.23.12.3
     2026-09-07 20:19; tag nuget-1.24.13.1 2026-09-24 05:49. Fleet offline from the 2026-09-16 weekly cutoff to 09-19
     (mailbox-archive.md:75476; the mailbox carries no posts on 09-17 or 09-18). Coordinator only, offline 2026-09-20
     ~15:35 to 09-21 23:33 (ledger 2026-09-21 17:36 · LANDING · 54bbb29f5: "COORD OFFLINE ... lanes: keep your queued
     cuts"; 23:33 · RULING · 7c291e5f2). H10: the recon leg at 0dc65a8e8d (2026-09-20 01:08); the campaign tip
     2026-09-22 03:33 · STAMP · c6fdbe73c; the re-bank train 07:05 · STAMP · 07240495e; the close 2026-09-23 22:30 ·
     STAMP · fa18863b94. Fix batches 2 (2026-09-20 14:55 · 0adf2e431), 3 (09-22 02:36 · 146c22828), 4, 5, 6, 7, 8a, 8b,
     8c, 8d, 8e, 8f and 8g (09-23 14:39 · faaa8fe999): thirteen. Lesson 16: the H5 rehearsal predicted 300-325 projects
     built and built 70. -->

**Step text for earlier lessons.**

- **Lesson 1, per OS side.** A WSL arm and the Windows side of one machine are two hosts, and each is qualified.
  **Step text:** Readiness, "Host".
  Why: on release night a §6 sweep host's WSL arm was the qualified linux net host while its Windows side was not
  qualified, so the net rows could join its shard only after a clean re-qualification.
  <!-- ledger 2026-09-24 00:33 · FINDING · 836004dd20 (a §6 sweep host's Windows side "is NOT NET-QUALIFIED", with no
  symlink privilege and long paths disabled, while "Its WSL arm IS the qualified linux net host"); 2026-09-24 02:02 ·
  RULING · f223c19182 (SW-3: net and os join that shard only if the re-qualification is clean). The resolver failure
  shapes (a LAN resolver timing out a nonexistent name; a router relay answering it SERVFAIL) share lesson 1's
  qualifier and cure: 2026-09-22 19:34 · FINDING · 43d149b87d; 22:55 · FINDING · 8b03ac8132. -->
- **Lesson 13, one more step.** After sizing the bill, convert the target release's own `testing` tests once against
  the host. **Step text:** H4, "The hand-owned test host".
  Why: a binding the host already declared still excluded a whole test file, because the tests-only mode's exclusion
  predicate never consulted the host's declarations. The `testing` row lost 30 verdicts until mid-H10.
  <!-- ledger 2026-09-22 16:05 · RULING · 3469154a95 (1.24.13's export_test.go adds `const ParallelConflict =
  parallelConflict`; the tests-only mode excluded the external testing_test.go whole, although
  src/core/testing/ExportTest.cs already declares it, because "the exclusion predicate never consults the host's
  declarations"). The check guards against that ParallelConflict class. Lesson 13's own citations:
  mailbox-0907-0913.md:8612-8648 (8 rows / 2,425 verdicts blocked). PLAN-corpus-upgrade.md R12 and OQ-3. -->
- **Lesson 18, at the close.** B-only attribution starts from the footprints the batches measured at their STAMPs.
  **Step text:** H10 close step 8.
  Why: one batch's STAMP carried a two-seeded footprint, one more named a single owed file, and the rest named
  nothing. The close regen stopped twice, on three classes only a regen shows, and a fourth seeded reconvert was owed
  before the release.
  <!-- Batch 3 measured: ledger 2026-09-22 02:36 · STAMP · 146c22828 ("the two-seeded corpus diff 20 files fully
  attributed … os/exec.cs RECLASSIFIED shared -> variant"), inside its own battery. Batch 4 (2026-09-22 08:18 · STAMP ·
  ae2f25198) named one production file as "ref 2's corpus footprint, landed by the hop's final -stdlib reconvert", with
  no pair. No later batch STAMP names a footprint. The close regen's stops: 2026-09-23 18:11 (922994cec3, R-B13: 337
  READMEs lost both badges); 18:39 (1a328f3ee8, R-B14 the payload guard and R-B15 the csproj LICENSE group). The 21:43
  stop (the slnx pin) is not a regen class. §6 fix A13 records the fourth seeded reconvert. A per-batch §6 gate row and
  a "B-only equals the union of the batches' owed files" predicate are held for a ruling. -->
- **Lesson 31, the order and its instrument.** H10 step 3 names the three live allocation labels and the order:
  reading run, relabel, rulings, then pages. The reading run's allocation-unit extractor is a Readiness instrument.
  **Step text:** H10 step 3; Readiness, "Instruments".
  Why: the relabel surfaced only at the close audit. It needed its own reading run, two adversarial reviews and a
  28-page battery, about fifteen hours from audit to stamp. It moved one row's count, `os` from 1,103 to 1,105,
  because the relabel forced that page to regenerate. The extractor existed only as a method in an evidence branch's
  README.
  <!-- ledger 2026-09-22 16:31 · FINDING · 3469154a95 (audit gap 1: "H10 step 3's legacy alloc labels were NOT
  retired"); 16:39 · RULING · c8e6ca9034 (only the bare alloc-profile retires; "alloc-count-semantics is the LIVE label
  for an unmeasurable unit"); 2026-09-23 02:17 · FINDING · ac9f8251ee (126 entries read; "the host records the unit only
  for a test's FIRST nonzero AllocsPerRun call"; the method in docs/phase4/h10-evidence/i7-readings/README.md "How the
  unit was extracted", README and TSV only, on claude/coord-h10-readings, not an ancestor of master); 03:37 · RULING ·
  718060141d (two adversarial reviews folded; item 5, "NOTHING RE-MEASURES AUTOMATICALLY TODAY -- `reading` is only
  presence-checked"; slices TestConcat stays alloc-count-semantics, O4); 07:12 · STAMP · 74bae27672 (batch 8d: 28 pages
  re-banked with 0 verdict moves, and "os BANKED at 1105 + 2 (Tests 1103 -> 1105) ... because the relabel required its
  page to regenerate"). -->
- **Lesson 33, where it applies.** Its two rules get a pointer in "How a row banks". **Step text:** H10, "How a row
  banks".
- **Lessons 4 and 36, beyond the recon leg.** Before the release, every evidence ref that a committed file or a
  ruling cites is merged to master. **Step text:** §3.2.
  Why: after the release, the §6 sweep's shards, the linux leg's rows, the allocation readings and two H10 passes'
  evidence lived only on evidence branches. A branch prune had already removed one H10 record's branch from origin.
  <!-- Branch-only at a1f133c3a9, each 1-3 commits ahead and cited by a committed file or a ruling:
  claude/g-p2-sweep-evidence 878eccf4af and claude/r-p2-sweep-evidence 45ae823fa0 (the §6 sweep's shards);
  claude/g-linux-leg-evidence ed694cd6f3 (the linux leg's rows.tsv; ledger 2026-09-23 06:18 · RULING · 5da426433b);
  claude/coord-h10-readings ac9f8251ee (the allocation readings the relabel rulings cite); claude/i9-h10-s2-evidence
  b2eff468c7 and claude/g-h10-pass3-evidence 1e7709ac7a (named by master's floor-table comment in
  src/run-validated-sweep.ps1, the Go 1.24.13 re-check block). claude/coord-h10-rehearsal-record (ledger 2026-09-22
  00:05 · LANDING · ac26e6a99, the rehearsal's timing and resume TSVs) is gone from origin; its commit is reachable only
  through claude/c1-fips140test-classing. In the tree: docs/phase4/hopA-inputs/recon-basis.tsv and recon-tsv/. -->

40. **Plan the hop in usage weeks, not machine days**, and do not let H5 or H10, the two coordinator-heavy
    stretches, straddle a usage cutoff. **Step text:** Readiness.
    Why: the usage allowance, not machine time, bounded the 1.24.13 hop's calendar (the paragraph above).
    <!-- mailbox-archive.md:75476 (COORD online 2026-09-19 "after the 2026-09-16 weekly cutoff"); ledger 2026-09-21
    17:36 · LANDING · 54bbb29f5 (the coordinator offline at the weekly limit; lanes kept cutting). The post-100 brief
    plans in credit-weeks (claude/coord-handover docs/phase4/briefs/roadmap-post100-2026-09-28.md, line 48). The
    cutoff at the rehearsal cost nothing (2026-09-21 23:33 · RULING · 7c291e5f2: the driver had already run, rc 0,
    7 rows banked; only the reporting sub-agent died). The rules that a battery's result is its committed TSV and that
    the resume record refreshes at every STAMP are fleet discipline, routed to the gate-forensics and save-state
    skills per §7's intro. -->

41. **Before H1, run a scratch recon at the target release and commit it as a RECON record.** In a scratch root
    seeded from `src/core`, with the pin bumped in that root only, run: one `-stdlib` conversion by a converter built
    with the target toolchain, the compile ladder, the hand-own census, a `go list std` census, and the `testing`
    host's bill sized as lesson 13 says. It reads no roster row. **Step text:** Readiness.
    Why: the 1.24.13 hop's recon sized four bills five days before H1: the front end (every package converted), the
    first two compile walls, the hand-own bill and the ten-row roster bill. No step owed it. A per-row reading is not
    part of it: rows need a compiling corpus, and compile parity took H4 through H7.
    <!-- docs/phase4/RECON-go1.24-hop.md (2026-09-02: 342 of 342 packages converted; two compile rungs; 4 vanished
    principals and 39 changed; 10 rows whose package no longer exists). H1: d5398554d8, 2026-09-07. The next hop's plan
    re-derived the same recon as the post-100 brief's Initiative 4 (RECON-go1.25-hop). Compile parity: lesson 16 (the H5
    rehearsal predicted 300-325 projects built, built 70); lesson 24 ("5 errors were 29 sites"); H7a fc275f1ac3
    (2026-09-16). A whole-population reading earlier than H10's recon leg (the leg at H7 green, in parallel with
    H7a-H9) is held for a ruling, because H9 is a GATE before H10. -->

42. **An instrument counts as landed only when `git merge-base --is-ancestor <sha> origin/master` succeeds**;
    acceptance into a train does not count. The recon wrapper and the dispatch driver refuse a non-windows host
    unless `GoTargetOS` names its OS. **Step text:** Readiness, "Instruments"; H10 step 1; H10's preconditions.
    (Sharpens lesson 2.)
    Why: a train of accepted instrument seats never landed, and only a branch-prune review after the release found
    it. Separately, a linux worker's first slice ran the windows flavour because nothing refused it, and it banked
    nothing. The refusal was ruled and never cut.
    <!-- ledger 2026-09-26 10:07 · FINDING · 3ffd1d8a8d ("TRAIN 49 NEVER LANDED"; the sweep's -Hop mode, the H5c
    empty-population verdict and the §3.1 correction named as "instrument and runbook debt that hop-lessons says must
    land before the next hop"). Still branch-only at a1f133c3a9: claude/c2-sweep-hop-mode baf1fbe727 (3 ahead; its
    landing seat claude/r-sweep-hop-landing b46af2049e ACCEPTED for a train on 2026-09-29, ledger 12:51),
    claude/c2-h5c-slnx-orphan 40f2b85145 (2 ahead), claude/c2-runbook-shard-amendment 4140a8e55d (1 ahead),
    claude/g-hop-b-provisioning d7bf606f07 (1 ahead). The refusal: ledger 2026-09-22 03:45 · RULING · c6fdbe73c,
    finding (a) ("the wrapper/driver on a non-windows host must REFUSE without GoTargetOS matching the host (an
    instrument seat; the H10 precondition table gains the row)"; 7/7 rows DIVERGED with an empty C# side, dead at
    syscall.LoadDLL(kernel32)). Never cut: neither src/run-h10-recon.ps1 (blob f17cc5b437) nor src/run-h10-dispatch.ps1
    (blob 05ec63184b) reads GoTargetOS at a1f133c3a9. -->

43. **The linux BANK annotates every banked row the linux reading covered, never only the rows already annotated.**
    **Step text:** H10 "Order and launch".
    Why: the leg re-banked only rows that already carried an annotation. So 30 banked rows it had read stayed
    unannotated, and closing linux parity after the release took a serial re-read of all 30.
    <!-- ledger 2026-09-26 06:01 · CENSUS · 3ffd1d8a8d ("Of 219 banked rows ... 30 have NO linux reading. ALL 30 were
    read by G's H10 linux leg … but never annotated, because that leg re-banked only already-annotated rows"); 06:34 ·
    FINDING · 3ffd1d8a8d (the serial re-read: 29 of 30 equal to windows; os routed as a finding). The leg: 2026-09-23
    06:18 · RULING · 5da426433b (217 rows in 4h04m, six shards, evidence claude/g-linux-leg-evidence). -->

44. **Re-key and re-check the deadline floors at the recon leg, per platform, before pass 1.** A floor keyed to a
    retired or relocated package moves to its successors. Raise any floor whose largest wall on any platform reaches
    0.75× the floor. **Step text:** H10 step 5.
    Why: step 5 went undone until the close audit found the table still keyed a retired package. One row was
    hand-stopped past its floor. One linux row was killed by its floor at 89 of 108 verdicts, then passed at a raised
    wall.
    <!-- ledger 2026-09-22 16:31 · FINDING · 3469154a95 (audit gap 2: "$longTimeouts still keys the retired
    crypto/internal/mlkem768 … and net's only 1.24.13 wall (3,792 s) exceeds its 40m floor"); 2026-09-23 11:51 ·
    FINDING · b293973e9f (sync/atomic on linux: killed by its 90m floor at 5,340 s, 89/108; PASS 108 at 150m, 5,868 s;
    "the d095 table's own rule (wall >= 0.75 x floor)"); 2026-09-24 04:58 · STAMP · 5098289482 (SW-8: "RAISE when the
    largest wall is at least 0.75 x the floor"). The re-keyed table at a1f133c3a9: src/run-validated-sweep.ps1:923-1014. -->

45. **A shard ref banks by two rules.** It stages test artifacts only, from a named list, and the leg gate refuses a
    ref that carries a production `.cs`. A rowless PASS package's artifacts and proof page ride the ref whether or not
    its roster row exists yet. **Step text:** H10, "How a row banks".
    Why: both were ruled mid-campaign after a ref got them wrong. Production files were restored on two legs, 40 on
    one and 6 on another, and three pages were re-run.
    <!-- ledger 2026-09-22 04:01 · RULING · 7227138c2 ("a re-bank ref stages TEST artifacts only … and NEVER a
    production .cs … The coordinator's leg gate refuses a shard ref that carries a production .cs"); 04:43 · RULING ·
    06f37e478 (40 production files restored on that leg); 06:33 · RULING · 4099c204b (6 restored on another); 06:38 ·
    RULING · 4099c204b ("a PASS row's test artifacts and proof page ride the shard ref WHETHER OR NOT a roster row
    exists yet"; three pages re-run). Close step 8 ("a row ref stages test artifacts only") states the first clause for
    the close regen. The Disclosed-cell rule is already the 2026-09-22 Tests-cell amendment. -->

46. **The close regen's checks run at every regen of the hop, from H4a on:**
    - widen the overlay by each emitted `<EmbeddedResource Include>` payload, and pin each one `-text`;
    - `git rm` an emitter-moved flat file only when all three per-GOOS copies are added;
    - assert that no README lost a badge line and that every L3 csproj round-trips;
    - run repoguard over the overlay.

    **Step text:** H5, the overlay bullet. (The close amendment's lessons 2-6, moved earlier.)
    Why: each check was first found at the close, and each stopped the close seat. The next hop has `//go:embed`
    emission live from its start, so it meets all of them at its first regen.
    <!-- §2 H10 close amendment, lessons 2-6: R-B13 fix 781c1c3c31, R-B14 fix de436207c9 (merged at the close as
    f0ada4471e) with census mirror 33f5f11297, R-B15 fix ae813db069; all on master. //go:embed emission landed mid-H10 at
    3ace3efd6a (2026-09-22), after H5, so no hop stage before the close met a payload. -->
```

---

## 2. In-place edits

Each edit gives the section and line at `a1f133c3a9`, the current text (verbatim; at most 3 lines), and the
replacement text. Every replacement carries the one-line dated note, in the form §7 already uses. Code-block
content keeps the anchor's own indentation.

**E0. §7 intro, after the "Fixes A1-A13" paragraph (line 3816).**
- Current: `> them) were corrected in place. Each carries a one-line dated note naming its fix, so the old text stays readable.`
- Replacement: keep the line, and append:
  ```
  >
  > **Addendum 2026-09-29 (§7.7).** Step text for lessons 1, 13, 18, 31, 33, and 4 with 36, and lessons 40-46, found
  > after 2026-09-26 or left in a comment.
  ```

**E1. Readiness, after the Fleet bullet (lines 172-173). Lessons 40 and 41.**
- Current:
  ```
  - **Fleet.** Addressed comms from day one (ledger, per-lane inboxes, one message per event), one OWNER-HAND line per
    owner action, and resume prompts written on the rule that nothing local survives a restart.
  ```
- Replacement: keep the bullet, and add after it:
  ```

  Hop-wide, before H1:

  - **Calendar.** Plan the hop in usage weeks, not machine days. Do not let H5 or H10 straddle a usage cutoff.
  - **Target recon.** A scratch recon at the target release, committed as a RECON record: a scratch root seeded from
    `src/core` with the pin bumped there only, one `-stdlib` conversion by a converter built with the target
    toolchain, the compile ladder, the hand-own census, a `go list std` census, and the `testing` host's bill (H4).
    It reads no roster row.
  *(Edited in place 2026-09-29, §7 lessons 40 and 41.)*
  ```

**E2. Readiness → Host bullet (line 164). Lesson 1's step text.**
- Current: `- **Host.** DNS on public IPv4 resolvers per adapter (IPv6 unbound where there is no v6 path), qualified by Go's own`
- Replacement: insert one clause after `**Host.**`, rewrap, and keep the rest of the bullet exactly as it is:
  ```
  - **Host.** Taken per OS side: a WSL arm and the Windows side of one machine are two hosts. DNS on public IPv4
    resolvers per adapter (IPv6 unbound where there is no v6 path), qualified by Go's own
  ```
  At the end of the bullet (after `any cloud quota requested now.`, line 168), add an indented line:
  `  *(Edited in place 2026-09-29, §7.7: lesson 1's step text.)*`

**E3. Readiness → Instruments bullet (lines 169-171). Lessons 42 and 31.**
- Current:
  ```
  - **Instruments.** Every instrument a later stage depends on is landed on master and red-proved BEFORE that stage —
    the H10 recon wrapper as one blob, the sweep's `-Hop` mode (under PowerShell 5.1 and pwsh 7), the H6 completeness
    gate, the H11 existence-plus-monotonicity check reading local AND origin tags.
  ```
- Replacement:
  ```
  - **Instruments.** Every instrument or runbook correction a later stage depends on is landed on master — checked by
    `git merge-base --is-ancestor <sha> origin/master`, since an accepted ref is not a landed one — and red-proved
    BEFORE that stage: the H10 recon wrapper as one blob, including its and the dispatch driver's refusal of a
    non-windows host whose `GoTargetOS` does not name its OS; the sweep's `-Hop` mode (under PowerShell 5.1 and
    pwsh 7); the H6 completeness gate; the H11 existence-plus-monotonicity check reading local AND origin tags; and
    the allocation-unit extractor H10 step 3's reading run uses.
    *(Edited in place 2026-09-29, §7 lesson 42 and lesson 31's step text.)*
  ```

**E4. H4 → "The hand-owned test host" bullet (lines 400-402). Lesson 13's step text.**
- Current:
  ```
  - **The hand-owned test host.** `src/core/testing` is skip-listed and never converted, so it follows
    **nothing** automatically while upstream keeps adding to `testing`'s API. It is a named work item of
    every migration that adds one.
  ```
- Replacement: keep the first two sentences, and replace "It is a named work item of every migration that adds
  one." with:
  ```
    It is a named work item of every migration that adds one, and the FIRST H4 item. Size it by
    receiver-typed call sites of the NEW members in the target release's test sources (a name census
    over-matches), convert the target's own `testing` tests once against the host, and land it before H1
    wherever it can be gated green at the outgoing corpus; the rest rides the version branch with H5.
    *(Edited in place 2026-09-29, §7.7: lesson 13's step text.)*
  ```

**E5. H5 → overlay bullet, after "…class must notice.**" (lines 619-620). Lesson 46.**
- Current:
  ```
    `package_info.cs` and `README.md` are never re-emitted. **A migration that adds a package to that
    class must notice.**
  ```
- Replacement: keep the text, and add a sub-bullet:
  ```
    - **The overlay also carries, at this regen and at H4a's:** every payload an emitted csproj names in
      `<EmbeddedResource Include>`, each pinned `-text` in `.gitattributes` and proved by re-checkout against GOROOT's
      sha256. An emitter-moved flat file is removed with `git rm` only when its per-GOOS copies are added for all
      three targets. After the overlay, assert that no README lost a badge line, that every L3 csproj round-trips
      byte-identically, and that repoguard passes. These are H10's close lessons 2-6, run where they first bite.
      *(Edited in place 2026-09-29, §7 lesson 46.)*
  ```

**E6. H10 step 1, last sentence (line 2292). Lesson 42.**
- Current: `   standing production-flip classes at the end. A wrapper `-Hop` mode is open instrument debt.`
- Replacement:
  ```
     standing production-flip classes at the end. The per-package instrument is `src/run-h10-recon.ps1`, taken by
     blob; the re-bank passes run `src/run-h10-dispatch.ps1 -Mode rebank`. Whether the sweep's `-Hop` mode may run
     any H10 leg is ruled when it lands on master. *(Edited in place 2026-09-29, §7 lesson 42: this read "A wrapper
     `-Hop` mode is open instrument debt.")*
  ```

**E7. H10 step 3, the allocation-label paragraph (lines 2297-2303). Lesson 31's step text.**
- Current (first line; the paragraph runs through line 2303, `[`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md).`):
  `   ⚠ **Since 2026-09-05 a re-derived manifest emits the TWO allocation labels, not `alloc-profile`.**`
- Replacement: replace the whole ⚠ paragraph:
  ```
     ⚠ **A re-derived manifest retires only the bare `alloc-profile` label.** Three labels are live:
     - `deferred`: the CLR can meet the assert. The entry carries `want`, `reading` and `plan`, and the loader and the
       roster guard both refuse it without them.
     - `structural`: a proof in the reason that the assert cannot be met, naming the object Go keeps off the heap. No
       plan.
     - `alloc-count-semantics`: the meter cannot see the unit.

     **The order:**
     1. A reading run takes each entry's unit note and per-run figure into a committed evidence TSV, through the
        allocation-unit extractor (Readiness, "Instruments"). The host notes only a test's FIRST nonzero
        `AllocsPerRun` unit, so a failing later call is read directly.
     2. Relabel from that TSV.
     3. Rule the entries the TSV cannot decide.
     4. Only then do pages bank, so each banks once.

     Nothing re-measures `reading` automatically; the loader only checks that it is present. Full definitions are in
     [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md).
     *(Edited in place 2026-09-29, §7.7: lesson 31's step text; this read "the TWO allocation labels", and it gave
     no order.)*
  ```

**E8. H10 step 5 (lines 2305-2306). Lesson 44.**
- Current:
  ```
  5. **Re-check the per-package deadline floors** in the sweep's long-timeout table — a migration can
     change a suite's cost.
  ```
- Replacement:
  ```
  5. **Re-check the per-package deadline floors** in the sweep's long-timeout table at the recon leg, per platform,
     before pass 1. A migration can change a suite's cost. Re-key a floor whose package was retired or relocated to its
     successors. Raise any floor whose largest wall on any platform is at least 0.75× the floor.
     *(Edited in place 2026-09-29, §7 lesson 44.)*
  ```

**E9. H10 "Order and launch" (lines 2313-2315). Lesson 43.**
- Current:
  ```
  READING (`GoTargetOS=linux`, CGO 0, privilege stated) runs alongside the recon leg; the linux BANK stays a separate
  leg. *(Edited in place 2026-09-26, §7 lessons 28-30: seat-before-recon made the generator refuse, launch traps cost a
  ```
- Replacement for the first line and the word `leg.` that opens the second (the existing italic note stays as it is):
  ```
  READING (`GoTargetOS=linux`, CGO 0, privilege stated) runs alongside the recon leg; the linux BANK stays a separate
  leg, and it annotates every banked row the reading covered, never only the rows already annotated.
  ```
  After the existing italic note (it ends `…on 25 of its 30 FAIL rows.)*`, line 2317), add:
  `*(Edited in place 2026-09-29, §7 lesson 43.)*`

**E10. H10 precondition table: a new last row, after line 2669. Lesson 42.**
- Current (the last row starts): `| any **preflight build** — a dry run, a red arm, a rehearsal row — runs in a tree that is **NOT the leg's** |`
- Replacement: keep that row, and add:
  ```
  | a re-bank row runs on the banking platform's arm; a linux run exports `GoTargetOS=linux` and belongs to the linux leg only, and the recon wrapper and dispatch driver refuse a non-windows host without it | 2026-09-22 (the banking platform ruled windows/amd64): a linux worker's first slice ran the windows flavour on a linux host, and all 7 rows read DIVERGED with an EMPTY C# side, dead in `os`'s static initializer at `LoadDLL(kernel32)`; it banked nothing. *(Added 2026-09-29, §7 lesson 42.)* |
  ```

**E11. H10 "HOW A ROW BANKS": a new block after the Tests-cell amendment (after line 2896). Lesson 45, and lesson 33's pointer.**
- Current (the amendment's last two lines):
  ```
  whose first page is older are counted, never gated. A figure set in a brief is a prediction; the page is
  the reading.
  ```
- Replacement: keep the paragraph, and add:
  ```

  ⚠ **AMENDED 2026-09-29 (§7 lesson 45): TWO BANKING RULES, each ruled mid-campaign at the 1.23 → 1.24 hop after a
  ref got it wrong.**

  1. **A shard ref stages TEST artifacts only**: `*_test.cs`, `package_test_info.cs`,
     `package_info_internal_test.cs`, `go2cs_test_host.cs`, `<pkg>.tests.csproj`, the proof page, the README badge,
     and a manifest only when a ruling changed its content. It never stages a production `.cs`. The production
     emission of record is `-stdlib`'s, so a production file a `-tests` run rewrote is restored with `git checkout`
     and named by count in the announcement. The leg gate refuses a ref that carries one.
  2. **A rowless PASS package's artifacts and proof page ride the ref**, whether or not its roster row exists yet.

  Lesson 33's two rules apply to every ref as well: push the measurement commit before a page cites it, and record a
  non-default environment on the page and in the row's execution pin.
  <!-- ledger 2026-09-22 04:01 · RULING · 7227138c2 (1); 06:38 · RULING · 4099c204b (2); close step 8 ("a row ref
       stages test artifacts only") states rule 1's first clause for the close regen; lesson 33 and
       briefs/h10-close-obligations.md:93-94. The Disclosed-cell rule is the Tests-cell amendment above. -->
  ```

**E12. H10 close step 8, the B-only line (line 3119). Lesson 18's step text.**
- Current (five leading spaces): `     - **B-only** is a converter change that never landed. Attribute each to a named first-parent merge.`
- Replacement (keep the five leading spaces; continuation lines take seven):
  ```
       - **B-only** is a converter change that never landed. Attribute each to a named first-parent merge, starting
         from the footprints the batches measured at their STAMPs (§7 lesson 18). *(Edited in place 2026-09-29, §7.7.)*
  ```

**E13. §3.1 (line 3415): land the stranded correction `4140a8e55d`.**
- Cherry-pick `claude/c2-runbook-shard-amendment` `4140a8e55d` (2026-09-13) onto the docs seat, as its own commit,
  in place of a second wording. It corrects "no jobs, throttle, shard or resume parameter" in place and adds a dated
  amendment explaining why §3.1's conclusion survives: the sweep's `-ShardCount`/`-ShardIndex` pair slices the row
  set into contiguous roster-order pieces on one host, so it cannot express §3.2's cost-ordered map.
- Verified 2026-09-29: it applies to `a1f133c3a9` with no conflict (a three-way merge-tree, base `4140a8e55d~1`). Its
  citations still hold at master (`src/run-validated-sweep.ps1:98-108`, `:111-114`, `:276-283`). The -Hop design
  ruling accepted it as P5.
- Its amendment quotes the sweep's own comment on the parameter's single-host thermal purpose. That documents the
  instrument, not a host issue, and it names no host. COORD confirms this against the owner's hardware exclusion at
  seating.

**E14. §6 table.** Withdrawn. The per-batch footprint row is held (H-4). The H10 fix-battery row repeats H-3 and is
dropped. The sweep-row addition is replaced by E15.

**E15. §3.2 (lines 3425-3427). Lessons 4 and 36.**
- Current:
  ```
  honest proxy is the previous full sweep's per-row wall time** — which means **per-row log retention on
  the preceding consolidation sweep is a prerequisite of the next migration's shard map**, and is
  unrecoverable afterward. Make it an obligation of that sweep, not of this step.
  ```
- Replacement: keep the text, and append to the paragraph:
  ```
  The same holds for every per-row reading a hop produces: **before the release, every evidence ref that a
  committed file or a ruling cites is merged to master.** *(Edited in place 2026-09-29, §7.7: lessons 4 and 36.)*
  ```

---

## 3. Held for a ruling; not in the insert

A runbook edit never reopens a ruling. Each item below changes a gate or a recorded rule, so it needs a ruling first.

- **H-1. Read the whole population at the target release earlier than H10.** The only feasible form is the H10
  recon leg run at H7 green, in a throwaway detached tree, in parallel with H7a-H9, reading only.
  - Conflict: the ladder says GATE steps block the next, and H9 is a GATE before H10.
  - Value: the post-H5 emission's per-row reading arrives about three days earlier. At hop B that is 09-16 against
    09-20. Defects the fix batches later found would surface then: the chan-conversion cast, maphash's `TypeFor`
    and synctest's `iter.Seq` binding (ledger 2026-09-20 13:30 · `6d814e2d3`); time's CS0052 (2026-09-22 04:43 ·
    `06f37e478`); embedtest and json publicization seeding (batch 3, `146c22828`).
  - A pre-H1 per-row reading is not feasible. Rows need a compiling corpus, and compile parity took H4 through H7
    (§7 lessons 16 and 24).
- **H-2. §6 sweep row: "never parked by a lane".**
  - At hop B the sweep ran as sharded, detached drivers on lane hosts, each with a survival canary and a resume ledger. That was recorded as deviation SW-2 (ledger 2026-09-24 02:02 · `f223c19182`; 04:58 · `5098289482`). It went green.
  - Proposed wording, if ruled: "coordinator-owned; sharded detached drivers on lane hosts are admitted when each carries a survival canary before row 1 and a resume ledger. Never a driver inside a lane's turn."
- **H-3. PLAN-corpus-upgrade §6, hop C row.** This is plan text, owned by the plan.
  - Re-estimate hop C in usage weeks and fix batteries, not the 3-5 gate-cycles currently written. The plan budgeted
    hop B at 5-8 gate-cycles and two sweeps. H10 alone ran thirteen fix batches, each gated by a battery, and the
    hop ran four seeded reconverts (H4a's, H5's, the close's and the pre-release one; §6 fix A13).
  - Timing: the owner's direction of 2026-09-29 (ledger 08:27) moves the freeze up to 225/225, about 10-01 to 10-02.
    Hop C therefore starts in early October, and the brief's window (about 10-26 to 11-08, `roadmap-post100-2026-09-28.md`
    appendix A) is stale. The squeeze against .NET 11 GA (2026-11-10) largely recedes. The brief owes a dated
    amendment.
- **H-4. A per-batch footprint gate** (from lesson 18).
  - Proposed: a §6 row, "two-seeded corpus footprint: **yes**, per converter or golib batch landed on the version
    branch, attributed at the batch's STAMP with the files owed to the close regen named"; and at close step 8,
    "the B-only set equals the union of those owed-file lists; a file on neither side is a finding".
  - Cost: two seeded conversions per batch, across about thirteen batches at hop B. Evidence that it fits: batch 3
    (`146c22828`) carried its two-seeded diff inside its own battery.
- **H-5. The sweep's `-Hop` mode's role at H10** (which legs, if any, it may run).
  - The 1.24.13 hop re-banked with `run-h10-recon.ps1` and `run-h10-dispatch.ps1 -Mode rebank`. The `-Hop`
    commit (`00bee3f041`) replaces only the count-versus-floor comparison. H10 step 1's second reason for "never the
    sweep wrapper" (re-emitted test sources reading as unclassified drift) is not shown to be handled.
  - The landing seat `b46af2049e` is SEATED for a train (ledger 2026-09-29 12:51). E6 says the role is ruled when
    it lands.

---

## 4. Where each lesson meets Go 1.25: for the next-hop notes record, not the runbook

The runbook names no release. These items belong in `docs/phase4/NOTES-next-hop.md` §3 and PLAN §5; the notes
record is the fuller statement.

Sources:
- go.dev/doc/go1.25, read 2026-09-29;
- this directory's sibling recon files: `testing-api-{O,T}.txt`, `linkname-added.txt`, `linkname-removed.txt` and
  `handown-census-124-to-12513.txt`. They were read at go1.25.13; re-read them at the pinned patch, 1.25.14.

| Lesson | Go 1.25 item |
|:--|:--|
| 13 and 41, the test host | `T/B/F.Attr` and `.Output` (`common.Attr`, `common.Output`, a new `outputWriter`), and a new `=== ATTR` output line. `AllocsPerRun` now panics if parallel tests are running, so Go's own suites never call it then. Port the panic with Go's exact `parallelStart`/`parallelStop` accounting: a host whose accounting differs panics where Go does not. `testing/fstest.MapFS` gains `Lstat`/`ReadLink` (`io/fs.ReadLinkFS`), and `TestFS` no longer follows symlinks. |
| 31, allocation readings | The compiler stack-allocates slice backing stores in more situations. Go-side counts drop, upstream alloc asserts may tighten, and every `deferred` entry's `want` and `reading` is re-read. The recon censuses the `AllocsPerRun` asserts that changed. |
| 41, the recon; H3 census, OQ-2 | `testing/synctest` graduates and joins the population. `encoding/json/v2` and `jsontext` stay out (`GOEXPERIMENT=jsonv2`). |
| 30 and 43, the linux reading | Non-windows first: `internal/syscall/unix.Utimensat` (`unix && !wasip1`). `internal/runtime/cgroup.throw` is all-platform, so it is not a linux-leg item. The libc `*at` procs are `aix || solaris`, outside the corpus targets. Container-aware `GOMAXPROCS` on linux changes runtime readings (`containermaxprocs`, `updatemaxprocs`). |
| 19, the declared-set guards | Removed pairs red the guards by design: `unique.runtime_registerUniqueMapCleanup` (the push registry) and `internal/runtime/maps.mapKeyError` (the q82 census's positive control; replace it in the same commit). |
| H1, lesson 8 (vet) | New `waitgroup` and `hostport` analyzers run over the converter's own code. |
| H1, lesson 9 (GODEBUG) | `internal/godebugs/table.go`: added `containermaxprocs`, `updatemaxprocs`, `decoratemappings`, `tlssha1`, `x509sha256skid` (change marker 25), and `fips140ems`, `htmlmetacontenturlescape`, `embedfollowsymlinks`; `fips140` becomes `Immutable`; nothing removed. The runtime's own variables (`runtime1.go`) lose `runtimecontentionstacks` and gain `checkfinalizers`. |
| H4, the go.mod readers | A new `ignore` directive. |
| 1, hosts | crypto/tls again: SHA-1 is disallowed in TLS 1.2, `GetEncryptedClientHelloKeys` is added, servers prefer the highest version, and EMS is required under FIPS. BoGo stays the long row. |
| Output-reading rows; H9 goldens | Panic text for a recovered-then-repanicked value now reads `panic: X [recovered, repanicked]` on one line. This touches golib's crash printer and any golden or test that reads panic text. |
| H6 hand-owns | `os.Root` gains 12 methods. Every std `Hash` implements `hash.Cloner`, which touches the source-generated hash shells. `sync.WaitGroup.Go` touches the hand-owned `sync`. `crypto/elliptic` drops `Inverse`/`CombinedMult`. `reflect.TypeAssert` is new. The `unicode` tables are regenerated. |
| §4, PLAN R11 | The nil-check fix can flip rows in both directions. Classify closures as carefully as breaks. |
