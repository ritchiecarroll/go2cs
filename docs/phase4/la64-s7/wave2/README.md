# S7 wave 2 dispatch instrument

Lane P5's copy of G's wave 1 instrument ([`../wave1/README.md`](../wave1/README.md)) for wave 2 of the
linux/arm64 bank. The plan of record is [`DATA-la64-s7-bank-plan.md`](../../DATA-la64-s7-bank-plan.md); this
directory adds no ruling to it.

## What differs from wave 1

| File | Difference from its wave 1 twin |
|---|---|
| `wave2.sh` | `SHARDS`, the row total (80) and the header lines. Modes, gates, refs and dispatch inputs are unchanged. |
| `filters.py` | The shard table and the named rows of section 2.2. Same rules: the shard's own rows, in list order, no line end. |
| `A-3.filter` ... `A-15.filter` | Wave 2's six row filters, 80 rows. |

Dispatch order is longest ESTIMATE first: A-3 (20 rows), A-10 (19), A-5 (20), A-11 (19), A-14 (1), A-15 (1).
The floor-set rows of wave 2 are net (A-15), os (A-11) and sync/atomic (A-14).

## Use, and the one gate `check` cannot read

```bash
python docs/phase4/la64-s7/wave2/filters.py docs/phase4/rebank-1.26.10.3/darwin /path/to/empty/dir
TIP_PREFIX=claude/p5-tip bash docs/phase4/la64-s7/wave2/wave2.sh plan /path/outside/the/repo/wave2-state
```

Wave 2 is dispatched only after wave 1's rows are READ (section 2.2). `check` reads only the gate commits, so
the dispatching lane confirms on the mailbox that wave 1 is read before it runs `refs`.

## What was exercised (2026-11-11, master `742a3dbb24`)

| Path | Result |
|---|---|
| `filters.py` | six files, 80 rows, every named row present; no row shared with wave 1's 116 |
| `check` with the stub bodies seat not yet on master | NOT READY, rc 1; 6 shards, 80 rows |
| `plan` with `TIP_PREFIX=claude/p5-tip` | six would-push lines in the order above, rc 0 |
| `refs` at NOT READY | refused |
