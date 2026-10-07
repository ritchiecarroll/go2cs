---
name: save-state
description: Weekly save-state / GitHub-persist procedure for the fleet — run when weekly usage passes 90% (or the owner asks). Produces docs/phase4/RESUME-SESSIONS.md, the paste-ready resume prompt per lane, verified against origin.
---

# save-state — the same procedure every week at 90%

**Trigger:** the owner says usage is near or past 90% of the weekly limit, or asks for "save state" /
"resume script". The limit can arrive fast, so the order goes out at the first mention and the file is
pushed in a skeleton form within the hour, then refined.

**Invariant:** *no preserved instance depends on local memory.* Memory files, scratchpads, untracked
prompt files and worktree state are caches. The record of how to resume every lane is one file on
GitHub, and every SHA in it is verified against origin before it is reported.

## 1. Order the fleet (one mailbox post)

Post the STATE BLOCK order with a deadline (~45 min). Keys, verbatim, one line each, `none` rather than
omission; no angle brackets (the post tool refuses them as placeholders):

```
LANE: {nickname}   MODEL: {class}/{effort}   HOST: {nickname}
BRANCH: {name} {sha40} {on-origin yes|no} {cut|announced|accepted|landed|superseded|stale} -- {what it is}
LOCAL-ONLY: {name} {sha40} {why not pushed} {preserved: bundle path-by-nickname} {sha256-16}
WORKTREE: {path-by-nickname} {branch} {uncommitted files N} {preserved how}
NEXT: {first action on resume, one sentence, with the starting SHA}
READ-FIRST: {mailbox SHAs / docs paths a fresh session reads before acting}
BLOCKED-ON: {owner hand | lane | landing | none}
TOOLS: {pins by env-var name and version -- GOROOT, DOTNET_ROOT, python -- never a hostname}
```

With it, the **push sweep**: every scrub-clean local branch pushed (announce-then-push on existing refs,
push-then-announce on new); never-push content bundled with an origin-reachable prerequisite, verified
from an origin-only clone, copied off the volume where one exists, and NAMED with its digest.
Children-before-parents for any worktree cleanup. This is not a stop order — work continues.

## 2. Compile `docs/phase4/RESUME-SESSIONS.md`

One section per lane plus COORD. Each section is a **paste-ready new-session prompt**: role, model
class + effort, the standing goal verbatim, the lane's STATE BLOCK, the mailbox protocol (post tool,
anchors, the watcher + wake-loop line, announce-then-push), the security order in words (nicknames only;
the never-push list lives in HANDOVER-coordinator.md), what to read first, the first action. COORD's
section additionally names: the handover log, the KICKOFF, the train scripts directory, the live
instruments, and every open owner hand. Nothing in it may require a scratchpad, a memory file, or an
untracked file to exist.
**File shape, learned the hard way — three readers parse this file and only one of them sees fences:**

- **A LANE SECTION HAS ONE SHAPE: a STATE BLOCK fence, a WAKE paragraph outside any fence, and a PASTE
  PROMPT fence** — a key-shaped line INSIDE a prompt fence is read as the record. <!-- ⚠ 2026-09-14. The
     verifier, the fold script and a human read the same file with different notions of structure, so any
     line that looks like `BRANCH: …` becomes data wherever it sits. -->
- **SECTIONS END AT NUMBERED HEADINGS** — verbatim posts inside fences carry their own `## 2026-…`
  headings, and a bare `^## ` split walks straight into them. <!-- ⚠ 2026-09-14. -->
- **A `BRANCH:` LINE CARRIES A 40-CHAR SHA OR IT IS A `NOTE:` LINE** — the resume verifier is fence-blind
  by design and read `missing=2` on two prose lines. <!-- ⚠ 2026-09-14. -->
- **A PASTE PROMPT NEVER PINS A TIP ITS OWN REFRESH MOVES: cite BRANCH + PATH, and the lane reads the tip
  by `ls-remote` and names it in the ACK.** <!-- ⚠ 2026-09-14, COORD; three lanes' prompts were re-cut on
     it. The handover branch's tip moves with every refresh of this very file, so a pinned SHA is stale
     before the lane pastes it. -->
- **RE-DERIVE `NEXT` AND `BLOCKED-ON` FROM THE LATEST RULING PER LANE, AND RE-MEASURE EVERY `BLOCKED-ON`
  BEFORE CARRYING IT** — a FALSE blocked-on costs the session, because a lane that believes itself blocked
  does not work. <!-- ⚠ 2026-09-13, G `f2f6240a1`: a resume file's `NEXT` compiled BEFORE a correction
     landed re-issues the error to the resumed lane. The two measured cases were an `index.lock` that was
     long gone and a landing that had already happened. -->

- **EVERY SECTION NAMES ITS LANE'S COMMS ROUTE, MEASURED, AND COORD'S SECTION CARRIES THE SLACK DUTY.** Per lane:
  how COORD reaches it and how it answers (a direct session message, by the FULL session title the session list
  prints; an inbox file on `claude/mailbox`; a lane on another account is mailbox-only), and whether it runs a
  mailbox watcher. COORD's section names the private Slack channel (#fleet; found by the connector's channel
  search, never by an id written here), its three uses (private detail the public mailbox may not hold; COORD's
  @mention pings to the owner for anything that needs him; his replies) and that nobody polls it. A new session
  on another account needs its own Slack connector, and a connector toggled in a running session arrives only at
  a turn the owner starts. <!-- ⚠ 2026-10-07, owner ruling (ledger 01:22, 02:0x): the routes and the owner's
     Slack pings had lived only in a session's memory and one handover subsection; the owner asked that every
     roll, on any account, carry them. -->

## 3. Verify, then push

```
bash .claude/coord-scripts/coord-resume-verify.sh docs/phase4/RESUME-SESSIONS.md
```

must read `missing=0` (every BRANCH SHA reachable from its ref on origin; LOCAL-ONLY lines listed).
Commit on `claude/coord-handover` (dated message), push, read back the tip. Fold into master at the next
landing's docs commit. CRLF/BOM: LF, no BOM (docs/ is outside the eol pin).
- **READ THE FOLD'S EXIT CODE BEFORE ITS COMMIT — NEVER THE VERIFIER ALONE.** <!-- ⚠ 2026-09-15. A python
     fold script that PRINTS a line of the resume file crashes on the Windows console (cp1252) the moment
     a lane's text carries a non-ASCII identifier (`ж` in `New<ж<…>>`): UnicodeEncodeError BEFORE the
     save, so the fold silently did nothing while the verifier still read clean — the file was simply the
     previous one, and it verified. Fixed at the script with
     `sys.stdout.reconfigure(encoding="utf-8", errors="replace")`. -->

## 4. Report the location — twice

Tell the owner, in the reply text, **branch + path** (`claude/coord-handover`,
`docs/phase4/RESUME-SESSIONS.md`) and the tip SHA. Repeat it in the next status reply so it is findable
in the session history without scrolling.

## 5. What is deliberately NOT persisted

Scrub tokens (`sec/`), credentials, real hostnames, PINs. The file says where each is supplied by the
owner on a new machine. A resume prompt that needs one of them says so as an OWNER HAND item, first.

## Derivation

Owner order 2026-09-13 (weekly credits near 80%; "safe recovery mode" at 90%; lanes may resume on
different machines; local memory is temporal; RESUME FAST; make it a skill so it is deterministic every
week). First run: train 47 run 8 in flight, the H4a/H5 rung on i9, five lanes + COORD.
