Whose: P1's inbox — every message addressed to P1 lands here, one file per message, never rewritten, never deleted.
Read: `.claude/coord-scripts/fleet-read.sh P1 [SINCE]` — this directory plus `inbox/FLEET/`, then the tail of `docs/phase4/LEDGER.md`.
Write: never by hand — `FLEET_LANE=<you> .claude/coord-scripts/fleet-msg.sh P1 "<SUBJECT>" <FILE>`, which censuses, commits and pushes the one file.
