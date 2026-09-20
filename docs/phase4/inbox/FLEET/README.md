Whose: every lane — rulings and anything addressed to the whole fleet land here, one file per message, never rewritten, never deleted.
Read: `.claude/coord-scripts/fleet-read.sh <LANE> [SINCE]` reads this directory for EVERY lane, alongside that lane's own inbox.
Write: never by hand — `FLEET_LANE=COORD .claude/coord-scripts/fleet-msg.sh FLEET "<SUBJECT>" <FILE>`; in practice only coordinator rulings go here.
