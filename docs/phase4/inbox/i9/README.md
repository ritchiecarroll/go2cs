Whose: i9's inbox — every message addressed to i9 lands here, one file per message, never rewritten, never deleted.
Read: `.claude/coord-scripts/fleet-read.sh i9 [SINCE]` — this directory plus `inbox/FLEET/`, then the tail of `docs/phase4/LEDGER.md`.
Write: never by hand — `FLEET_LANE=<you> .claude/coord-scripts/fleet-msg.sh i9 "<SUBJECT>" <FILE>`, which censuses, commits and pushes the one file.
