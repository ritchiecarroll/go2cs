# .4 (1.26.11.1) hosted burst at 6e870821d6: read record

P5, 2026-11-18. The hosted half of the .4 re-bank deal (`docs/PLAN-rebank-1-26-11-1.md`, mode R): 33 os-matrix
`sweep-shard` dispatches at master `6e870821d6`, one tip ref per shard (`claude/p5-tip-6e870821d6-<shard>`):
14 darwin shards (both RIDs), 17 linux-arm64, the windows `net` row on a hosted runner, and the linux x64 `os` row.

**Superseded.** COORD ruled a full .4 re-bank at the final tip after the reflect array-copy fix and P2's class 2 fix
land. This is the record of the 6e870821d6 read; it banks nothing.

## Files

- `shards.tsv`: the dealt shards (shard, goos, estimated seconds, rows).
- `runs.tsv`: shard, run id, status, conclusion.
- `ev/<shard>.tsv`: evidence per shard. Darwin via `docs/phase4/la64-s7/collect_darwin.py`, linux via a one-leg
  collector, `GH-S` from its sweep log line.
- `ev/XDW-<shard>.tsv`: the darwin rows the collector did not emit. It stops at a row with no comparison record,
  so the rows of a failed darwin shard after that row are read from the sweep log's `PASS`/`FAIL` line instead.
- `score.tsv`: one line per (row, RID): the count read against the roster at the tip, verdict AT/ABOVE/BELOW.

## Result

710 (row, RID) reads: 236 rows on each of linux-arm64, osx-x64 and osx-arm64, plus `net` on win-x64 and `os` on
linux-x64. **689 AT, 0 ABOVE, 21 BELOW.**

The 21 BELOW are 7 rows on 3 RIDs: `runtime`, `net/http`, `encoding/json`, `encoding/gob`, `reflect`,
`internal/reflectlite`, `text/tabwriter`. All seven were build-blocked at 6e870821d6 by the -tests holder class
(the six) and the tabwriter CS0579 (P1's). Those reads are void. The holder fix landed as master `8a86140f79`, and
the six were re-read there:

- linux, both RIDs (run 44358697867): five AT, `encoding/gob` BELOW.
- darwin `runtime` (44365184748): 11062 osx-x64 and 11063 osx-arm64, AT.
- darwin five (44365199860): `net/http` 1538, `encoding/json` 550, `reflect` 445, `internal/reflectlite` 30 AT on
  both RIDs; `encoding/gob` BELOW (`TestEndToEnd`).

The `encoding/gob` BELOW is the reflect array lost write (BOARD, 2026-11-18, P5), fixed on
`claude/p5-reflect-array-copy`. `text/tabwriter` is P1's fix.

`ev/AR-5.tsv` (`net/netip`): the two `TestParsePrefixAllocs` subtests, whose names are IP prefixes, are written
`TestParsePrefixAllocs/*`, as the roster and the BOARD write them.
