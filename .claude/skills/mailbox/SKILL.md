---
name: mailbox
description: Post to or read the fleet mailbox. Anchors, read discipline, push contention, duplicate-post defences and the state-advancing-tool rules.
---

# Mailbox

<!-- EXTRACTED VERBATIM from CLAUDE.md at 1800b04f8, lines 6472-6597.
     Phase 1 of docs/PLAN-context-diet.md: RELOCATION ONLY, no compression, no edits.
     The byte-identical original is docs/doctrine/JOURNAL-2026-09-12.md.
     Phase 2 distills this file. Keep every dated derivation, but move it into an HTML
     comment beside the rule it justifies: comments are stripped before this file enters
     context, so provenance kept this way costs ZERO tokens. -->

- **⚠ A CONCURRENCY TRANSIENT IS READ FROM THE FAIL LINE'S ERROR TEXT, and its arm is an ISOLATED
  re-run (2026-09-02/03).** The converter suite failed ONCE under five concurrent sub-agents with
  `go: go.mod file not found in current directory or any parent directory` from its `go` child and
  passed 3/3 in isolation at the same tip twelve minutes later; CNR printed `[transpile FAILED]
  <Package>` mid-run under seven concurrent processes while a hand transpile with the SAME binary
  minutes later emitted a `main.cs` byte-identical to the committed golden. The mechanism is UNROOTED
  (a shell-out whose cwd vanished under it is the shape; which concurrent purge did it is not
  measured) — **an environment-shaped message is the tell**. The gate honestly reports NOT MEASURED by
  name, and the COMPLETION is a per-package re-transpile IN PLACE after the run with a `git status` of
  that directory, stated in the landing post — never a whole-CNR re-run and never a "close enough".
- **⚠ A STATE-ADVANCING TOOL ASSERTS THE STATE MOVED (2026-09-03/04).** A delivery check comparing
  LOCAL to REMOTE PASSES when nothing was committed: a mailbox post instrument split its message on
  embedded double quotes (PS 5.1 native-argument quoting), the commit failed as a bad pathspec, and
  `DELIVERED=True` printed because the failed commit left local equal to remote. Such a tool asserts
  `HEAD != pre-append tip`, exits non-zero otherwise, passes free text through a FILE (`-F`) rather
  than an argument, and is positive-controlled with the INPUT SHAPE that broke it before its next real
  run — route #6's shape in the coordinator's own hand, surfaced by the read-anchor rule. ⚠ And **a
  ledger entry is stamped from `git log --date=format-local` of the post it records, never from an
  estimate**: lane commit stamps carry the LANE's clock (a cloud container's is UTC), and reading them
  as local ran a ledger ~35 minutes fast for an hour and mis-sized a running CNR leg as past its
  budget when it was on pace.
  ⚠ **THE DUPLICATE-POST CLASS IS A TOOL CLASS ACROSS THE FLEET, and a heading-keyed census of it
  OVER-REPORTS by more than an order of magnitude** (2026-09-08): three lanes' post tools appended
  byte-identical entries six, two and four times over three days, each on a DELIVERY CHECK that read
  NOT-DELIVERED for a push that had LANDED — this file's own "remote rejected with exit 1 had landed"
  case, retried in a loop. The sound predicate is a **BODY HASH** (a subject-less heading matched 91
  DISTINCT bodies); the instruction is that a post tool FETCHES and compares the remote tip's entry to
  what it appended BEFORE any retry; and **a lane's own cleanup of its duplicates DELETES the evidence
  a body-hash census reads**, so mailbox content is never removed without the coordinator's word. The
  working counter-example never retries: it pushes, reads the ref back FROM THE REMOTE, and on any
  doubt exits NON-ZERO leaving the decision to the lane. **A retry loop on a delivery check is a loop
  that must be right about failure, and an exit is not.** Its mirror is the announced-but-unlanded
  SHA: the same defect the other way round.
  ⚠ **IN A SHARED CLONE AN UNCOMMITTED EDIT BELONGS TO WHOEVER TOUCHES THE PATH NEXT** (2026-09-04): a
  sibling's mailbox post swept a scrub lane's three uncommitted substitutions into its OWN commit, and
  a second sibling's tree operation then reverted the scrub lane's remaining files before they could
  commit — so `git status` read CLEAN and `git commit` read "nothing to commit" while the work was
  gone. **The edit-to-commit-to-push window in a shared clone is ONE command**, a clean status there is
  never evidence that work landed (read the pushed TIP), and a post instrument stages ONLY the file it
  owns — never `-A`, which IS the sweep mechanism — and restores ONLY that file on its own failure
  path: the coordinator's own post script ran `git reset --hard` in the shared clone when a commit did
  not land, which is the revert mechanism, scoped to its own file the same day. ⚠ And **a `git push`
  reporting `remote rejected` with exit 1 had LANDED** — `ls-remote` settles a push, so "read the
  pushed tip, not the exit code" cuts both ways.
  ⚠ **Two more, 2026-09-06.** **A guard whose INPUT the caller can derive from the same source it
  checks against is not a guard**: a mailbox read-confirmation passed as `$(git rev-parse
  origin/<mailbox>)` makes the comparison tip == tip, always true, so it catches a STALE confirmation
  and CANNOT catch a freshly computed one — and a lane advanced its anchor past an unread post exactly
  that way, in the one tool whose whole job is to prevent it. **Anchor such a check on state THE TOOL
  REMEMBERS** (it writes its own anchor after each successful post and derives the absorbed range from
  that) **and treat the caller's argument as a CLAIM to cross-check, never as the anchor.** And the
  ls-remote rule's mirror: **a REFUSED branch DELETE answers `Everything up-to-date`** — the remote
  rejects the deletion with an HTTP 403 and git then prints the ordinary no-op line, so with stderr
  redirected the command reports three cheerful "already gone" results while all three refs sit
  untouched. The tell is `ls-remote` AFTERWARDS, never the exit code; push may work from a session
  where delete does not, and the difference is invisible without reading the refs — the same family as
  the shell eating a command interpreter's switch, an operation reporting success because it never ran.
  ⚠ **A VERIFICATION CLAIM IS POSTED ONLY FROM THE VERIFICATION'S OUTPUT, never composed in the same
  response as the command that produces it** (2026-09-04, the coordinator against itself):
  "pre-verified at the remote — five commits, zero markers, zero census hits" went out in a reply
  issued in PARALLEL with the fetch, and the fetch answered `couldn't find remote ref`; the branch had
  never been pushed. **Any post that STATES a measurement is a DEPENDENT step of that measurement** —
  parallelism is for independent items only, and a claim about a result is never independent of the
  result.
  ⚠ **A `&` BACKGROUND LAUNCH IN A COMPOUND BASH COMMAND SWALLOWS EVERYTHING AFTER IT, HEREDOCS
  INCLUDED** (2026-09-06). An urgent retraction was written as `cmd & … cat > entry <<EOF … post`:
  the launch backgrounded the rest, the entry file was never created, the post never ran, and the
  only visible output was a tool banner. **A post is CONFIRMED by reading the REMOTE — `git fetch`
  then grep for the post's own distinctive line — never by the absence of an error.** The tool's
  ENTRY-FILE-MISSING guard caught the second attempt; nothing caught the first. **Launch background
  work in its OWN call.**
  ⚠ **A GATE COMPOSED INTO THE SAME COMMAND AS THE ACTION IT GATES CANNOT GATE IT.** A lane ran its
  security census in the same command as its push: the census executed, printed clean, and its verdict
  could not have stopped anything — a reassurance rather than a check. **Same family as the
  exit-code-through-a-pipe trap and the `;`-instead-of-`&&` chain that once committed conflict markers:
  the instrument runs, and the ORDERING makes its verdict inert.** Nearly invisible, because the log shows
  the gate running and shows it clean; the remedy is one command, not a re-cut — run the gate against the
  PUSHED TIP.
  ⚠ **AN ASSERTION WHOSE REFERENCE IS DERIVED FROM THE THING UNDER TEST CAN NEVER FAIL, and it is caught
  by a positive control STAYING GREEN rather than by anything looking wrong.** ⓥ
  `check-roster-format.ps1`'s manifest-coverage line compared `$manifestsChecked` against
  `$manifestFiles.Count` — the loop against its own input, true under ANY enumeration — and passed the
  neutering control that should have reddened it. **Fixed at master by a SECOND, independent walk of the
  same tree** (`$manifestsOnDisk`), with the reasoning recorded at the site. **A control that does not go
  red is not a passing control; it is an unfalsifiable assertion announcing itself.**
  ⚠ **A CHECK ASSERTS ITS INPUT POPULATION IS NON-EMPTY BEFORE ITS VERDICT MEANS ANYTHING.** A safe-push
  composition's own census scanned `remote..local`, EMPTY over an already-pushed branch — so it scanned
  nothing and reported CLEAN: **the exact vacuous-green class the composition existed to close, sitting
  inside the composition.** Its author found it by RUNNING the script against a real seat, having READ that
  code four times. Same shape inside git itself: **`--force-with-lease` is NOT EVALUATED when there is
  nothing to push**, so a clean lease on a no-op reads as the opposite of the truth.
  ⚠ **A COMPARISON WHOSE BASELINE SILENTLY READS EMPTY DOES NOT FAIL — IT CONFIDENTLY REPORTS TOTAL
  DISAGREEMENT** (2026-09-08): REPLACING `PATH` instead of prepending to it dropped `git`, the
  baseline fetch produced nothing behind a discarded stderr, and an eight-project check read "8 of 8
  DIFFER" — the exact INVERSE of the truth — until a control asserted the baseline blob non-empty.
  **Every compare asserts BOTH sides non-empty before it reports a difference.**
  ⚠ **A VERDICT LINE THAT IS A HARDCODED STRING IS A CHECK THAT CANNOT GO RED — derive the verdict from
  the count and exit on it.** A loop correctly reported two failures and the `echo` after it printed "all
  seat SHAs resolve (silence above = clean)" unconditionally; **the parenthetical was the tell, asserting
  a premise the same output had falsified two lines earlier** — written into the very command applying the
  rule about verdict lines that cannot go red.
  ⚠ **WHEN "REMEMBER TO ORDER IT CORRECTLY" HAS FAILED FOR FOUR PARTICIPANTS IN ONE SESSION — INCLUDING
  THE TWO WHO WROTE THE RULE DOWN — THE FIX IS A SCRIPT, NOT MORE CARE.** In every instance the INSTRUMENT
  was correct while the SHAPE OF THE COMMAND made its verdict inert, and every one produced output that
  looked exactly like the healthy case. A lesson living in attention rather than in a script fails under
  exactly the conditions the script exists for, and a forty-minute multi-leg run is what consumes
  attention: the remedy is the assertion IN the script (`BASE ASSERTED: <sha> contains master <sha>, 0
  behind, roster N rows`, **REFUSING** rather than reporting, each row confirmed banked IN THAT TREE before
  its leg) — the old script would have run four more legs on a stale base and reported all four green.
  ⚠ **A MONITOR ANCHOR IS THE FULL 40-CHARACTER SHA** (2026-09-07, run 57): the mailbox monitor
  compares its anchor as a STRING against `rev-parse` of the remote tip, so an ABBREVIATED anchor reads
  MOVED on its first poll with nothing new — a vacuous fire that looks exactly like a lane post. The
  per-run derive ASSERTS the anchor's length before arming. ⚠ **And a post tool's ABSORBED-RANGE
  listing is read WHOLE, never tailed** (2026-09-08 00:20): the coordinator piped its post tool through
  `tail -3`, so when a lane's post landed between a monitor fire and the coordinator's next post, the
  tool absorbed it into the read anchor and PRINTED it — and the tail hid it. The post went unread for
  twelve minutes, and was found only because a later post by another lane cited it. **The monitor's
  delta ends at the FETCH instant; a post placed one minute later is inside the NEXT post's absorbed
  range and nowhere else.** Print every line of a state-advancing tool's output, and treat "commits
  absorbed" as posts owed a read before the next dispatch.
  ⚠ **A THING YOU MUST READ BELONGS WHERE YOUR HABIT LOOKS** (2026-09-08): a post tool printed its
  absorbed-range listing ABOVE the delivery line, its author tailed the last two lines to confirm
  delivery, and the coordinator's post answering the question it had asked forty minutes earlier sat
  three lines up. **A placement failure in an INSTRUMENT, not a resolve failure** — fixed by printing
  the listing AFTER the delivery line behind a banner. Same shape as a gate that prints a verdict
  nobody greps.
