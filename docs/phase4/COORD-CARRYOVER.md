# COORD CARRYOVER: everything a fresh COORD session needs that is not in CLAUDE.md, the rules or the skills

Written 2026-10-17 at the fleet roll, from COORD's local memory, so nothing operative lives only on the i7's disk.
Read it after RESUME-SESSIONS §1l. Owner-private notes (travel, employer, contact details) are deliberately
absent; they are not needed to run the fleet. Ask the owner when anything personal matters.

## Machine and environment (the i7)
- All COORD work is on drive H: (H:/Projects/go2cs, H:/Projects/go2cs-mailbox, H:/go2cs-tmp-coord/*). Never walk
  the user profile recursively: it hydrates OneDrive and fills C:.
- DOTNET_ROOT is the dotnet10 folder in the profile (put it first on PATH). GOROOT is the BACKSLASH spelling that
  `go env GOROOT` prints: go1.26.9 for the hop, go1.24.13 for master. A forward-slash GOROOT misroutes the emission.
- pwsh is a .NET 8 GLOBAL TOOL. Launch it with DOTNET_ROOT UNSET and the ambient PATH, and set the gate env INSIDE
  pwsh (`env -u DOTNET_ROOT pwsh -NoProfile -Command '$env:GOROOT=...; $env:DOTNET_ROOT=...; $env:PATH=...; & ./x.ps1;
  exit $LASTEXITCODE'`). rc 150 means pwsh never started.
- Solutions build in DEBUG for gates: a Release build of go2cs.slnx reads an installed $(USERPROFILE)/go2cs layout
  and cascades thousands of errors.
- The Bash tool silently truncates multi-KB commands, so write long text with the Write tool. On Windows, python
  `-I` ignores PYTHONIOENCODING, so print non-ASCII via ascii(). Use H:/ paths, not /h/ paths, inside python.
- `$(...)` in a double-quoted bash commit message is command substitution: put such text in a file.
- Guard every rm on a derived path (assert non-empty and under the root). Never kill processes by name. Never run
  machine-wide cleanup (`dotnet build-server shutdown`), and forbid it in every sub-agent brief.

## Comms and cadence
- G and the i9: direct session messages (titles "g -- resume sesstion" and "i9 -- Corpus migration to 1.24.13
  coordination"). C1 and C2: direct message out, mailbox back. P1, P2, R: mailbox (docs/phase4/inbox/<LANE>/), sent with
  .claude/coord-scripts/fleet-msg.sh after coord-identifier-census.sh entry on the file.
- Slack: the private fleet channel (find it by the connector's channel search). Mention the owner whenever a lane
  waits on him, for milestones, and when a report he asked for is ready. Nobody polls it.
- Owner hands route through COORD. Lanes post OWNER-HAND to COORD at once, and COORD Slack-pings the owner.
- Re-arm the inbox watcher (tP/watch-battery-inbox.sh <FULL 40-char mailbox tip> inbox, from coord-scratch, as a tracked background task, never nohup) as the last tool call
  of every working turn. End every reply on text, never on a tool call.
- Address the owner by his first name, the form he uses in chat (never a fleet title unless he does). Read the clock (`date`) in the same command as any dated line.
- Status replies carry the branch, path and tip; integration turns end with Completed / Running / Queued.
- Ledger: one line per ruling, via coord-live/ledger-append.sh with a FRESH entry file that starts STAMP_PLACEHOLDER.
- Monitor weekly credits every turn (get_usage). Around 90%, run the save-state skill; 95% is the hard stop.

## How COORD works
- COORD rules, merges, gates and records. Execution goes to lanes or to Opus sub-agents (model: opus on every Agent
  call), in their own worktrees, with no pushes unless told.
- COORD has the final design say. Escalate to the owner only a reads-like-Go or runs-like-Go shape choice, or
  anything outward-facing.
- Timelines are COORD's: never trade a gate for a date. "Do it now, do it right": fold the known remaining piece into
  the current seat rather than defer it. Behavioral parity outranks build time and output size.
- Every converter, gen or golib seat by another lane gets G's review before merge. A "measure first" order precedes
  any cut whose mechanism is unmeasured.
- Merge mechanics on the hop: signed --no-ff merges; full gates; revert the ruled platform-alias drift (the x509 pair,
  FieldAccessReinterpret's syscall aliases); re-cite moved docs notes by hand with a fingerprint check; union BOARD
  append conflicts; REGENERATE converter-output conflicts, never hand-merge them; verify the remote tip, push, prune.
- Lanes clean their own disk ("cleanup:" line at task end); COORD prunes remote branches after every landing.
- Landing gates read rc, never a `| tail`. A rejected tool call may have run anyway: read the target before re-sending.
- Never type a SHA from memory: read it.

## Standing owner rulings (beyond 1k.2)
- Nicknames only on pushed surfaces (R-LAPTOP, G-LAPTOP, i9, i7); no real hostnames, profile paths or account names.
- Breaking changes are fine before the loud announcement. Docs state what IS, in present tense; history goes in
  HTML comments, the BOARD or design docs. A docs claim names the platform it was measured on. Durable docs carry no
  exact corpus figures.
- Never change visible emission for a diagnostic-only test: disclose instead. Drop unneeded golib deps without asking.
- The standing allow for Go and .NET package downloads covers module and NuGet restores, not installers or pin changes.
- NuGet releases: the owner signs with his PIN. COORD prepares and gives him the release-shell env block.
- Version lines: master carries the current Go line; the next minor hops on a branch; cut release/go1.N for security
  fixes after the next lands; hop every release; there is NEVER a Go 1.25 line.
- The i9 has a known CPU defect: crypto/tls never runs there; TLS-touching rows go to P2; one heavy row at a time;
  WHEA per row; a fetch fault means one retry, then `git fsck --full`.
- The TypeScript side mission stays off master's visitor docs (internal mention is fine).

## Queued work and plans (none of it in flight at the roll)
- POST-HOP: the shift-count cast cleanup joins the parity stage (P1 measured it: 3,900 literal sites; fix at
  convBinaryExpr.go:1818 and visitAssignStmt.go:1986-2020, never inside intCastOperand). The WaitGroup bubble sync
  (a StaticBox marker, G; BOARD 2026-10-15). The nugetgo rebuild wave (all modules plus mousetrap and cobra; README
  "About" text plus a C# repo link). The version-line PLAN doc. The owed G BOARD rows.
- PARITY STAGE (approved): docs/phase4/proposals/visual-parity-review.md. Plain literals and interceptors per 1l.0,
  then the adversarial perf review, then the strings design doc.
- C# consumer API: LANDED on the hop (8bb3b8b5be); the doc text for release is in
  docs/phase4/proposals/gtk-doc-updates.md.
- Planned: a reboot of ALL machines around 10-23 at a stage boundary (largely done at this roll).
- RAINY-DAY: opt-in ASCII spellings for glyph names (a plan doc only). The IDE-mode experiment: PLAN-ide-mode.md.
- Platform parity goal: Windows, Linux and Darwin at 100% honest validation. Roster rows show W/L/D bubbles that are
  derived, never hand-set.
