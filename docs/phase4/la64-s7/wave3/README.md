# S7 wave 3 dispatch instrument

Lane C1's copy of P5's wave 2 instrument ([`../wave2/README.md`](../wave2/README.md)), itself a copy of G's wave 1
instrument, for wave 3 of the linux/arm64 bank: the last 40 rows. The plan of record is
[`DATA-la64-s7-bank-plan.md`](../../DATA-la64-s7-bank-plan.md); this directory adds no ruling to it.

## What differs from wave 2

| File | Difference from its wave 2 twin |
|---|---|
| `wave3.sh` | `SHARDS`, the row total (40) and the header lines. Modes, gates, refs and dispatch inputs are unchanged. |
| `filters.py` | The shard table of section 2.2. Wave 3 names no row. The rules are the same: the shard's own rows, in list order, with no line end. |
| `A-4.filter`, `A-1.filter` | Wave 3's two row filters, 40 rows. |

- **Dispatch order** is longest ESTIMATE first: A-4 (20 rows, 80 min), then A-1 (20 rows, 72 min).
- **The filters are the roster's pending rows.** Together they hold exactly the 40 rows the roster reads
  `linux-arm64: pending` at master `7c7dc687f8`, and they share none with waves 1 and 2 (116 + 80 + 40 = 236).

## Use

```bash
python -I docs/phase4/la64-s7/wave3/filters.py docs/phase4/rebank-1.26.10.3/darwin docs/phase4/la64-s7/wave3
TIP_PREFIX=claude/c1-tip bash docs/phase4/la64-s7/wave3/wave3.sh plan /path/outside/the/repo/wave3-state
```

Wave 3 is dispatched only after wave 2's rows are READ. They were read and banked in batch 294 (`7c7dc687f8`).
