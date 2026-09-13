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

## 3. Verify, then push

```
bash .claude/coord-scripts/coord-resume-verify.sh docs/phase4/RESUME-SESSIONS.md
```

must read `missing=0` (every BRANCH SHA reachable from its ref on origin; LOCAL-ONLY lines listed).
Commit on `claude/coord-handover` (dated message), push, read back the tip. Fold into master at the next
landing's docs commit. CRLF/BOM: LF, no BOM (docs/ is outside the eol pin).

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
