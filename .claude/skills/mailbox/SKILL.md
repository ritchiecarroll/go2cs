---
name: mailbox
description: Post to or read the fleet mailbox. Anchors, read discipline, push contention, duplicate-post defences and the state-advancing-tool rules.
---

# Mailbox

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 6472-6597.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 (2026-09-12) distilled this file: every dated narrative was moved into an HTML
     comment OPENING ON THE LAST LINE OF THE RULE IT JUSTIFIES (so stripping leaves no residual
     blank line and the provenance costs ZERO tokens). Nothing was deleted; if a rule reads thin,
     its full derivation is in the comment under it and in the JOURNAL.
     2026-09-12: batch19 items (branch claude/coord-doctrine-batch19, commits 24bfc8304 /
     c5e17217b / e9e56b657, old-CLAUDE.md anchors 6492 / 6550 / 6560 / 6585) were merged into this
     shape — three became new visible rules, two amended existing rules and folded into their
     comments. -->
## Before you post a claim
- **A post that STATES a measurement is a DEPENDENT step of that measurement.** Never compose the
  claim in the same response as the command that produces it — write it from the verification's own
  output. Parallelism is for independent items only, and a claim about a result is never independent
  of the result. <!-- ⚠ 2026-09-04, the coordinator against itself: "pre-verified at the remote —
  five commits, zero markers, zero census hits" went out in a reply issued in PARALLEL with the
  fetch, and the fetch answered `couldn't find remote ref`; the branch had never been pushed. -->
- **A gate composed into the same command as the action it gates cannot gate it.** Run the security
  census as its own command, against the PUSHED TIP, before you announce. <!-- ⚠ A lane ran its
  security census in the same command as its push: the census executed, printed clean, and its
  verdict could not have stopped anything — a reassurance rather than a check. Same family as the
  exit-code-through-a-pipe trap and the `;`-instead-of-`&&` chain that once committed conflict
  markers: the instrument runs, and the ORDERING makes its verdict inert. Nearly invisible, because
  the log shows the gate running and shows it clean; the remedy is one command, not a re-cut. -->
- **An environment-shaped message in a FAIL line is the tell of a CONCURRENCY TRANSIENT, and its arm
  is an ISOLATED re-run.** The gate honestly reports NOT MEASURED by name; the COMPLETION is a
  per-package re-transpile IN PLACE after the run with a `git status` of that directory, stated in
  the landing post — never a whole-CNR re-run and never a "close enough". <!-- ⚠ measured
  2026-09-02/03. The converter suite failed ONCE under five concurrent sub-agents with
  `go: go.mod file not found in current directory or any parent directory` from its `go` child and
  passed 3/3 in isolation at the same tip twelve minutes later; CNR printed `[transpile FAILED] <Package>` mid-run
  under seven concurrent processes while a hand transpile with the SAME binary minutes later emitted
  a `main.cs` byte-identical to the committed golden. The mechanism is UNROOTED (a shell-out whose
  cwd vanished under it is the shape; which concurrent purge did it is not measured). -->
- **Stamp a ledger entry from `git log --date=format-local` of the post it records, never from an
  estimate.** Lane commit stamps carry the LANE's clock. <!-- ⚠ 2026-09-03/04: a cloud container's
  clock is UTC, and reading those stamps as local ran a ledger ~35 minutes fast for an hour and
  mis-sized a running CNR leg as past its budget when it was on pace. -->

## Posting
- **The edit-to-commit-to-push window in a shared clone is ONE command** — an uncommitted edit there
  belongs to whoever touches the path next. Stage ONLY the file you own (never `git add -A`, which IS
  the sweep mechanism); restore ONLY that file on your own failure path (never `git reset --hard` in
  a shared clone). A clean `git status` is never evidence that work landed — read the pushed TIP.
  <!-- ⚠ 2026-09-04: a sibling's mailbox post swept a scrub lane's three uncommitted substitutions
  into its OWN commit, and a second sibling's tree operation then reverted the scrub lane's remaining
  files before they could commit — so `git status` read CLEAN and `git commit` read "nothing to
  commit" while the work was gone. The coordinator's own post script ran `git reset --hard` in the
  shared clone when a commit did not land, which is the revert mechanism; scoped to its own file the
  same day. -->
- **A lost mailbox race is answered by a MERGE, never by a force — and a `--force-with-lease` whose
  expected SHA is READ AT PUSH TIME is that force wearing the careful spelling.** A lease asserting
  "the remote is whatever it currently is" is always satisfied. Push through `src/safe-push.sh`,
  whose RANGE step (`git rev-list --count <remote>..<local>`) ERRORS on an object the local clone
  lacks — exactly what a moved remote produces. Repair a dropped post by fetch, READ the interleaved
  commits, re-append, re-push; and attribute a mailbox event from `git log --graph`, never from
  prose. <!-- ⚠ 2026-09-08: a lane hand-rolled `REMOTE=$(git ls-remote …)` then
  `--force-with-lease=…:$REMOTE`, skipping `src/safe-push.sh`, and the push DROPPED another lane's
  post. It was restored by MERGE with all three posts verified present. The coordinator
  mis-attributed both halves to the wrong lane TWICE — first from a reconstruction, then from the
  restore-merge's OWN subject line — before reading the GRAPH. Twin of the lease defect recorded
  under "Writing the guard inside the tool": a lease is also NOT EVALUATED when there is nothing to
  push, so both spellings of "careful" fail silently. -->
- **Pass free text through a FILE (`-F`), never as a native argument.** <!-- ⚠ 2026-09-03/04: a
  mailbox post instrument split its message on embedded double quotes (PS 5.1 native-argument
  quoting) and the commit failed as a bad pathspec. -->
- **Launch background work in its OWN call.** A `&` background launch inside a compound bash command
  swallows everything after it, heredocs included, and the only visible output is a tool banner.
  <!-- ⚠ 2026-09-06: an urgent retraction was written as `cmd & … cat > entry <<EOF … post`: the
  launch backgrounded the rest, the entry file was never created, and the post never ran. The tool's
  ENTRY-FILE-MISSING guard caught the second attempt; nothing caught the first. -->

## Confirming delivery
- **A state-advancing tool ASSERTS the state moved: `HEAD != pre-append tip`, exit non-zero
  otherwise.** A delivery check that compares LOCAL to REMOTE PASSES when nothing was committed.
  Positive-control such a tool with the INPUT SHAPE that broke it before its next real run.
  <!-- ⚠ 2026-09-03/04: the commit failed as a bad pathspec and `DELIVERED=True` printed because the
  failed commit left local equal to remote — route #6's shape in the coordinator's own hand,
  surfaced by the read-anchor rule. -->
- **`ls-remote` settles a push; the exit code does not.** Confirm a post by reading the REMOTE —
  `git fetch`, then grep for the post's own distinctive line — never by the absence of an error. A
  `git push` reporting `remote rejected` with exit 1 had LANDED. Its mirror: a REFUSED branch DELETE
  answers `Everything up-to-date`, so with stderr redirected three refs read "already gone" while all
  three sit untouched. <!-- ⚠ 2026-09-04 (rejected-but-landed push) and 2026-09-06 (refused delete):
  the remote rejects the deletion with an HTTP 403 and git then prints the ordinary no-op line. Push
  may work from a session where delete does not, and the difference is invisible without reading the
  refs — the same family as the shell eating a command interpreter's switch, an operation reporting
  success because it never ran. -->
- **Never retry on a delivery check — exit NON-ZERO and leave the decision to the lane.** A post tool
  FETCHES and compares the remote tip's entry to what it appended BEFORE any retry. A retry loop on a
  delivery check is a loop that must be right about failure, and an exit is not. Key any duplicate
  census on a **BODY HASH**, never on a heading. Never remove mailbox content without the
  coordinator's word — a lane's own cleanup of its duplicates DELETES the evidence a body-hash census
  reads. The mirror defect is the announced-but-unlanded SHA. <!-- ⚠ THE DUPLICATE-POST CLASS IS A
  TOOL CLASS ACROSS THE FLEET, and a heading-keyed census of it OVER-REPORTS by more than an order of
  magnitude (2026-09-08): three lanes' post tools appended byte-identical entries six, two and four
  times over three days, each on a DELIVERY CHECK that read NOT-DELIVERED for a push that had LANDED
  — this file's own "remote rejected with exit 1 had landed" case, retried in a loop. A subject-less
  heading matched 91 DISTINCT bodies. The working counter-example never retries: it pushes, reads the
  ref back FROM THE REMOTE, and on any doubt exits NON-ZERO. -->
- **Verify a RESTORE by the `## ` heading the commit's OWN diff added, never by the commit subject** —
  a subject grep reads 0 for a post that is present and manufactures a PHANTOM LOSS. This scopes the
  body-hash rule above rather than contradicting it: a duplicate CENSUS keys on the body hash because
  one heading matches many bodies; a PRESENCE check keys on the added heading because the subject is
  not in the file at all. <!-- ⚠ 2026-09-08, restoring the force-dropped post recorded under Posting:
  a grep of the commit SUBJECT against the mailbox file read 0 for a post that WAS present, because
  the body heading is worded differently. Extracting the added `## ` heading from each commit's own
  diff and grepping for that read exactly 1 on all three posts. -->

## Reading and anchors
- **Anchor a read-confirmation on state THE TOOL REMEMBERS, and treat the caller's argument as a
  CLAIM to cross-check, never as the anchor.** A guard whose INPUT the caller can derive from the
  same source it checks against is not a guard: a confirmation passed as
  `$(git rev-parse origin/<mailbox>)` compares tip == tip, always true — it catches a STALE
  confirmation and CANNOT catch a freshly computed one. <!-- ⚠ 2026-09-06: a lane advanced its anchor past an unread post
  exactly that way, in the one tool whose whole job is to prevent it. The fixed tool writes its own
  anchor after each successful post and derives the absorbed range from that. -->
- **A monitor anchor is the FULL 40-character SHA**; the per-run derive asserts the anchor's length
  before arming. An abbreviated anchor reads MOVED on its first poll with nothing new — a vacuous
  fire that looks exactly like a lane post. <!-- ⚠ 2026-09-07, run 57: the mailbox monitor compares
  its anchor as a STRING against `rev-parse` of the remote tip. -->
- **A poster verifies its OWN entry landed once and CANNOT see that a PRIOR entry is gone** — it
  fetches, resets to whatever the tip is, appends, pushes. Detection of a DROPPED post belongs to a
  READER THAT REMEMBERS THE PREVIOUS TIP: assert the previous anchor is an ANCESTOR of the new tip
  (`git merge-base --is-ancestor <anchor> <tip>`) and stamp HISTORY REWRITTEN with both SHAs
  otherwise. <!-- ⚠ 2026-09-08: added to the coordinator's mailbox monitor after the
  `--force-with-lease` push recorded under Posting dropped a sibling's post. No poster's own delivery
  check can reach this class — its entire view is the tip it just wrote, and it reads healthy. -->
- **Print every line of a state-advancing tool's output — never `tail` an absorbed-range listing** —
  and treat "commits absorbed" as posts owed a read before the next dispatch. The monitor's delta
  ends at the FETCH instant; a post placed one minute later is inside the NEXT post's absorbed range
  and nowhere else. <!-- ⚠ 2026-09-08 00:20: the coordinator piped its post tool through `tail -3`,
  so when a lane's post landed between a monitor fire and the coordinator's next post, the tool
  absorbed it into the read anchor and PRINTED it — and the tail hid it. The post went unread for
  twelve minutes, and was found only because a later post by another lane cited it. -->
- **A thing you must read belongs where your habit looks**: print the absorbed-range listing AFTER
  the delivery line, behind a banner. Placement and whole-reading are ONE remedy — a listing in the
  right place is VOID if the read is `tail`ed. <!-- ⚠ 2026-09-08: a post tool printed its absorbed-range
  listing ABOVE the delivery line, its author tailed the last two lines to confirm delivery, and the
  coordinator's post answering the question it had asked forty minutes earlier sat three lines up. A
  placement failure in an INSTRUMENT, not a resolve failure. Same shape as a gate that prints a
  verdict nobody greps. ⚠ 2026-09-08, the interaction proved: a lane read past THREE posts addressed
  to it and posted "nothing owed that I know of" while a defect was already ruled to it — its own
  post tool printed the absorbed entries exactly where its habit looks, and it `tail`ed the output so
  one long unrelated entry filled the window. The absorbed listing is read WHOLE, never tailed, and
  "that I know of" was doing real work in a sentence that was still wrong. Retracted by the lane the
  same hour, with the cut. -->

## Writing the guard inside the tool
- **An assertion whose reference is derived from the thing under test can never fail, and it is
  caught by a positive control STAYING GREEN rather than by anything looking wrong.** Derive the
  reference from a SECOND, independent walk. A control that does not go red is not a passing control;
  it is an unfalsifiable assertion announcing itself. <!-- ⓥ `check-roster-format.ps1`'s
  manifest-coverage line compared `$manifestsChecked` against `$manifestFiles.Count` — the loop
  against its own input, true under ANY enumeration — and passed the neutering control that should
  have reddened it. Fixed at master by a SECOND, independent walk of the same tree
  (`$manifestsOnDisk`), with the reasoning recorded at the site. -->
- **A check asserts its input population is NON-EMPTY before its verdict means anything.** <!-- ⚠ A
  safe-push composition's own census scanned `remote..local`, EMPTY over an already-pushed branch —
  so it scanned nothing and reported CLEAN: the exact vacuous-green class the composition existed to
  close, sitting inside the composition. Its author found it by RUNNING the script against a real
  seat, having READ that code four times. Same shape inside git itself: `--force-with-lease` is NOT
  EVALUATED when there is nothing to push, so a clean lease on a no-op reads as the opposite of the
  truth. -->
- **Every compare asserts BOTH sides non-empty before it reports a difference.** A baseline that
  silently reads empty does not fail — it confidently reports TOTAL DISAGREEMENT. <!-- ⚠ 2026-09-08:
  REPLACING `PATH` instead of prepending to it dropped `git`, the baseline fetch produced nothing
  behind a discarded stderr, and an eight-project check read "8 of 8 DIFFER" — the exact INVERSE of
  the truth — until a control asserted the baseline blob non-empty. -->
- **Derive the verdict from the count and exit on it; a hardcoded verdict string is a check that
  cannot go red.** A confident parenthetical is the tell. Score the count against a population that
  EXCLUDES the instrument's OWN floor files — named EXACTLY, never matched by pattern — and print
  MET/MISSED. <!-- ⚠ A loop correctly reported two
  failures and the `echo` after it printed "all seat SHAs resolve (silence above = clean)"
  unconditionally, asserting a premise the same output had falsified two lines earlier — written into
  the very command applying the rule about verdict lines that cannot go red. ⚠ 2026-09-08, six
  per-run reports: a scorer's "PREDICTED 0" was a HARDCODED LABEL against a total that INCLUDED the
  instrument's own floor files, so the prediction was unmeetable BY CONSTRUCTION and printed beside
  every run; and a corpus filter excluding on `report|manifest` MISSED a progress text file, so a
  floor file would have counted as corpus movement. The fix names the floor files exactly, scores the
  CORPUS count, prints MET/MISSED, and controls the filter on a previous run's real data where it
  must reproduce the known reading (windows 0 / linux 0 / darwin 1). Both defects would have
  FLATTERED the author, which is why they are fixed before the zero is believed. -->
- **When ordering-by-care has already failed, the fix is a SCRIPT, not more care** — the assertion
  goes IN the script and REFUSES rather than reports: `BASE ASSERTED: <sha> contains master <sha>, 0
  behind, roster N rows`, each row confirmed banked IN THAT TREE before its leg. <!-- ⚠ "Remember to
  order it correctly" failed for FOUR participants in one session, including the two who wrote the
  rule down. In every instance the INSTRUMENT was correct while the SHAPE OF THE COMMAND made its
  verdict inert, and every one produced output that looked exactly like the healthy case. A lesson
  living in attention rather than in a script fails under exactly the conditions the script exists
  for, and a forty-minute multi-leg run is what consumes attention: the old script would have run
  four more legs on a stale base and reported all four green. -->
