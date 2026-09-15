---
name: save-state
description: Weekly save-state / GitHub-persist procedure for the fleet — run when weekly usage passes 90% (or the owner asks), after every landing or ruling, and at every wake tick. Produces docs/phase4/RESUME-SESSIONS.md — one paste-ready resume prompt per lane, verified against origin — and the order the fleet comes back up in.
---

# save-state — the same procedure every week at 90%

**Trigger:** the owner says usage is near or past 90% of the weekly limit, or asks for "save state" /
"resume script". The limit can arrive fast, so the order goes out at the first mention and the file is
pushed in a skeleton form within the hour, then refined. **The figure itself is the OWNER's to report at
check-ins; the CADENCE, not the trigger, keeps the record current** — the refresh runs after every landing
or ruling and at every wake tick regardless. <!-- 2026-09-14, checked against the documentation: no CLI command,
statusline field, hook or MCP tool exposes the weekly-usage percentage to a session. A procedure keyed to a number no
instrument in the session can read runs late; keyed to a cadence it cannot. -->

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

## 2. `docs/phase4/RESUME-SESSIONS.md` — one shape per lane section

One section per lane plus COORD, each carrying: role, model class + effort, the standing goal verbatim, the
lane's STATE BLOCK, the mailbox protocol (post tool, anchors, the watcher + wake-loop line,
announce-then-push), the security order (nicknames only; the never-push list is in HANDOVER-coordinator.md),
what to read first, the first action. COORD's also names the handover log, the KICKOFF, the train scripts
directory, the live instruments and every open owner hand. Nothing in the file may require a scratchpad, a
memory file or an untracked file to exist.

**A lane section has ONE shape** — this supersedes the earlier "each section is a paste-ready prompt"
wording, which left the layout to the author:

1. `## N. LANE — role — provenance`, a NUMBERED heading; a section ends at the next one.
2. The **STATE BLOCK** fence, the section's FIRST: `KEY: text` lines and indented continuations ONLY.
3. The **WAKE paragraph**, OUTSIDE any fence, starting `WAKE (LANE`.
4. The marker line `PASTE PROMPT (revision DATE ...)`, then the **prompt fence**; then, last, any
   verbatim record fences (posts quoted whole).

- **No prose under a key name: a `BRANCH:` line carries a 40-char SHA or it is not a `BRANCH:` line —
  write `NOTE:`.**
- **The fold scripts operate on the section's FIRST fence; the verifier reads every `BRANCH:` line in
  the file, fence or not.** A key-shaped line anywhere is read by something.
<!-- ⚠ 2026-09-14, three ways in one refresh. (a) C2's final block wrote two PROSE lines under `BRANCH:`; the verifier,
deliberately fence-blind, read `missing=2` at the pushed tip — cured by re-keying them to `NOTE:`, never by deleting a
line. (b) C1's section had its STATE BLOCK INSIDE the prompt fence plus a stale second `NEXT (ruled ...)` line: two
answers to one question, and the fold scripts reached neither. (c) R's draft prompt carried column-0 `BRANCH:` and
`WAKE:` lines inside its prompt fence, which the fold scripts would have mis-applied (apply-wake.py scans the whole
section span, fence-blind; dedupe-paste-prompts.py drops everything after a key-shaped line). Three readers parse this
file and only one sees fences. -->

## 3. The paste prompt

`preamble + YOUR FIRST ITEM + WAKE + NOT YOURS`, in that order, and nothing else:

- **The shared preamble is ONE file**, four braces filled — LANE / MODEL / EFFORT / BOX — nothing else
  altered; byte-identical across lanes is a verification lens (§4).
- **`YOUR FIRST ITEM (ruling post SHA, ruling id)`**, derived from the ruling of record; the prompt
  promises nothing that lands later.
- **A one-line WAKE** restating the lane's own mechanism and cadence with NO ids (every Monitor, Cron and
  Routine id in the record is dead to a resumed session), then **`NOT YOURS`** — what this lane does not
  pick up, naming the lane-plan sentence it amends.
- **The STATE BLOCK keys NEXT / READ-FIRST / BLOCKED-ON are re-derived in the SAME commit that lands the
  prompt.** A prompt beside a section still showing the previous rung's NEXT gives two answers.
- Model class and effort sit in the header; the OWNER sets model, effort and permission mode (auto) at bring-up.

**A paste prompt NEVER pins the handover branch's tip.** It cites branch + path, tells the lane to read
the tip with `git ls-remote`, and to NAME the tip it fetched in its ACK.
<!-- ⚠ 2026-09-14: the coordinator pushed two refreshes while a verification round was still running; every draft that
pinned the tip was refuted on that alone and three lanes' prompts were re-cut by hand. The tip moves with every refresh
BY CONSTRUCTION, so a pin in the file that republishes itself is guaranteed stale — and one command reads the true one. -->

## 4. Draft from the record, verify on three lenses

- **Drafts are written by Opus-class sub-agents FROM THE RECORD, never from memory**: every SHA read by
  `ls-remote`, every mailbox SHA resolved in the read clone. Fable is reserved for COORD's own rulings.
- **Three adversarial lenses before the refresh, each its own pass:** (1) **refs at origin** — every SHA
  resolvable, every ref present; (2) **security / format** — no `<PLACEHOLDER>` tokens, no hostnames, account
  names or profile paths, nicknames only, the preamble byte-identical; (3) **actionability / fidelity** — a
  fresh session with no local file could act on it; nothing beyond the ruling; nothing the ruling gave dropped.
- **The coordinator then applies its own edits and STATES them in the refresh's log line.**
<!-- 2026-09-14, the rounds that produced G's, C1's and i9's fences. Drafts clean on all three lenses still took COORD
edits — a pinned handover tip, a re-run ruled to the wrong owner, half-A hashes verified against a re-post instead of the
recipe post, three branches a lane had called census-dirty that were at origin at those SHAs. A passing lens is not a
release; the log line is where a later reader learns which sentences the verification did not write. -->

## 5. Before every push — two gates and a signature

```
bash .claude/coord-scripts/coord-resume-verify.sh docs/phase4/RESUME-SESSIONS.md
```

- **`missing=0`** — every BRANCH SHA reachable from its ref on origin; LOCAL-ONLY lines listed. **A
  pre-existing miss is CURED by re-keying prose to `NOTE:`, never by deleting a line.**
- **An identifier census of the diff's ADDED lines reads 0** — username path, hostname, IPv4. A false
  positive (a version like `10.0.400`) is named in the commit message, not silenced.
- **Sign when the agent is primed.** The owner primes each Windows box's gpg-agent at the keyboard
  (`default-cache-ttl` / `max-cache-ttl` 604800 in `gpg-agent.conf`). Probe with
  `--batch --pinentry-mode error` on a clearsign of `test` — NEVER on a commit. A plain `gpg --clearsign`
  from the session POPS the pinentry on the owner's screen: that is the prime itself when the owner is at
  the keyboard (measured 2026-09-14 on the i7), and a hang when nobody is.

Commit on `claude/coord-handover` (dated message), push, read back the tip; fold into master at the next
landing's docs commit. CRLF/BOM: LF, no BOM (docs/ is outside the eol pin).

## 6. The resume order — a checklist, one lane at a time

1. **COORD first.** Re-arm the Monitor and the wake loop UNCONDITIONALLY; read the mailbox from the POST
   TOOL'S OWN anchor, every entry, whole; post **ONLINE** carrying the ruling per lane and the protocol;
   refresh the COORD section + a dated handover block; push; read the tip back.
2. **Then the lanes, ONE AT A TIME, in critical-path order,** each from its own PASTE PROMPT fence.
3. **Each lane ACKs** `watcher armed + wake loop armed`, the tip it resumed from, the item it is starting,
   and a STATE BLOCK delta for anything stale.

The **fleet protocol paragraph** the ONLINE post carries: the owner is NOT at the lane keyboards, so a lane
request needing a person is an OWNER-HAND post to COORD, relayed in the coordinator session; no chips
anywhere; the owner primes GPG at each Windows box; model and effort per the prompt header.

## 7. Report the location — twice

Tell the owner, in the reply text, **branch + path** (`claude/coord-handover`,
`docs/phase4/RESUME-SESSIONS.md`) and the tip SHA. Repeat it in the next status reply so it is findable
in the session history without scrolling.

## 8. What is deliberately NOT persisted

Scrub tokens (`sec/`), credentials, real hostnames, PINs. The file says where each is supplied by the
owner on a new machine. A resume prompt that needs one of them says so as an OWNER HAND item, first.

## 9. Tooling — copy the set, set three constants

The runnable scripts are on `claude/coord-instruments` under `.claude/coord-scripts/save-state/`, with
`coord-resume-verify.sh` one directory up. A session COPIES the set into its scratchpad and sets the three
path constants once — repo root, the handover worktree's resume file, the post tool's clone — in the copy
it runs, never in the pushed file. The set: `fold-block.py`, `replace-block.py`, `apply-block-delta.py`,
`apply-wake.py`, `append-verbatim.py`, `dedupe-paste-prompts.py`, `refresh-resume.py`,
`refresh-coord-section.py`, `c2-delta-to-spec.py`, and the 2026-09-14 additions `resume-tools.py`
(`set-key LANE KEY FILE`; `set-prompt LANE FILE`, which NORMALIZES the section to §2's shape; `show LANE`),
`rewrite-coord-section-v2.py`, `rekey-c2.py`. Run with `PYTHONUTF8=1` and Windows-style paths on a Windows
box. **A section ends at a NUMBERED heading** — verbatim posts inside fences carry `## 2026-...` headings of
their own, so a bare `^## ` split walks into them.
<!-- 2026-09-14: the additions were written mid-resume, because hand-editing a 700-line file while a verification round
runs against it is exactly the error surface §2's shape rules describe. `set-prompt` exists so a prompt fence lands in
the one layout all three readers of this file agree on. -->

## Derivation

Owner order 2026-09-13 (weekly credits near 80%; "safe recovery mode" at 90%; lanes may resume on
different machines; local memory is temporal; RESUME FAST; make it a skill so it is deterministic every
week). First run: train 47 run 8 in flight, the H4a/H5 rung on i9, five lanes + COORD.
Amended 2026-09-14 from the FIRST resume run made from the file (COORD online, then the lanes one at a time):
§2 the lane-section shape, §3 the paste prompt and the no-tip-pin rule, §4 the drafting and three-lens
verification, §5 the census and the signing probe, §6 the resume order, §9 the tooling.
