# MAILBOX.md — ARCHIVED

**ARCHIVED 2026-09-20 under PROTOCOL v4 (addressed comms). Do not append here.** This file is a
stub; a tool that still writes to this path has found the stub instead of the channel, and stops.

Where to look:

- **Your inbox** — `docs/phase4/inbox/<LANE>/`, one file per message, never rewritten. Read it with
  `.claude/coord-scripts/fleet-read.sh <LANE> [SINCE]`.
- **Rulings to everyone** — `docs/phase4/inbox/FLEET/`, read for every lane by the same script.
- **The record** — `docs/phase4/LEDGER.md`: coordinator-only, append-only, one line per ruling,
  landing or stamp. A tick reads its tail since the last tick and nothing else of the channel.
- **To post** — `FLEET_LANE=<you> .claude/coord-scripts/fleet-msg.sh <TO> "<SUBJECT>" <FILE>`.

The full v4 text is the **last entry** of `docs/phase4/archive/MAILBOX-through-2026-09-20.md`.
