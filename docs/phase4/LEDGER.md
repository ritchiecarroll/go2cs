# LEDGER — the durable record under PROTOCOL v4

Coordinator-only. Append-only. ONE LINE per ruling, landing or stamp, oldest first, in the form
`YYYY-MM-DD HH:MM · KIND · SHA · one sentence` where KIND is RULING, LANDING or STAMP.
A lane's tick reads the tail since its last tick — a few lines — and nothing else of the channel;
`.claude/coord-scripts/fleet-read.sh <LANE> [SINCE]` prints exactly those lines after the inbox.
Never rewrite a line: a correction is a NEW line that names the line it corrects.

2026-09-20 13:15 · RULING · 77a03bd82 · PROTOCOL v4 addressed comms in force; MAILBOX.md archived
