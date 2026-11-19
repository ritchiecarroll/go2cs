# .4 (1.26.11.1) FINAL hosted burst at 280a01952c: read record

P5, 2026-11-19. The hosted half of the .4 final re-bank (FLEET 20261119T030713Z, the freeze tip `280a01952c`), dealt by
`docs/PLAN-rebank-1-26-11-1.md` (mode R): 33 os-matrix `sweep-shard` dispatches, one tip ref per shard
(`claude/p5-tip-280a01952c-<shard>`), plus four declared or routed re-reads. The first pass at `6e870821d6`
(`hosted-6e870821d6/`) is superseded and banks nothing.

## Files

- `shards.tsv`: the dealt shards (shard, os, projected seconds, rows), identical to the first pass.
- `runs.tsv`: shard, run id, status, conclusion, head. The last four lines are the re-reads.
- `ev/<shard>.tsv`: evidence per shard. Darwin via `docs/phase4/la64-s7/collect_darwin.py` as fixed in batch 314,
  linux via a one-leg collector, `GH-S` from its sweep log line.
- `ev/XDW-7.tsv`: DW-7's 18 osx-x64 rows read from their sweep log `PASS` lines (see DW-7 below).
- `score.tsv`: every read, raw, against the roster dumped at the tip (byte-identical to the deal's).
- `final.tsv`: one line per (row, RID), the read of record and its verdict, with the overrides below applied.

## Result

711 (row, RID) cells: 236 rows on each of linux-arm64, osx-x64 and osx-arm64; `net` on win-x64 (GH-S); `os` and `net`
on linux-x64 (HX-1, HX-NET). **710 AT, 0 ABOVE, 1 BELOW** (`runtime` osx-x64). All seven rows void at `6e870821d6`
(`runtime`, `net/http`, `encoding/json`, `encoding/gob`, `reflect`, `internal/reflectlite`, `text/tabwriter`) read AT on
every RID they were dealt to, except `runtime` osx-x64.

## Re-reads and overrides (each posted to COORD when it happened)

- **runtime, darwin.** DW-1 (44449541821): osx-x64 matched 11061, the Go oracle failing `TestPingPongHog` (the .3
  SIG-1 shape); osx-arm64 11063 + 57 AT. Declared re-read DW-1R (44464066634): osx-arm64 11063 + 57 AT; osx-x64
  matched 11060 with two C#-side lines, `TestChanSendBarrier` and `TestChanSendSelectBarrier` hitting the 60 m
  per-test deadline (a standing osx-x64 class on the BOARD, 2026-10-28). Posted as a finding; banking is COORD's ruling.
- **net, darwin.** DW-5 (44449565182): osx-x64 553 + 3 AT; osx-arm64 Go oracle unstable on
  `TestLookupNoSuchHost/LookupTXT_NXDOMAIN` against the runner's resolver. The banked segment was read with the
  sweep-shard `dns` option, so the declared re-read DW-5R (44474028417) carried it: 553 + 3 AT on both RIDs, the read
  of record. Lesson in the plan's section 9.
- **DW-7.** The osx-x64 job (44449572794) was cancelled by the runner at 05:02:56Z mid-shard. Its 18 finished rows have
  log lines and no comparison records: read from the `PASS` lines (`ev/XDW-7.tsv`), all AT. The two unread cells,
  `internal/trace` and `net/http`, were re-read in DW-7R (44468879667): AT.
- **internal/trace, darwin.** Both reads of record print 186 + 1: `TestTraceCPUProfile/AsyncPreemptOff` (DW-7,
  osx-arm64) and `TestTraceCPUProfile/Default` (DW-7R, osx-x64) diverged on the native profiler's skip. These are the
  row's named host-conditional disclosures, absorbed by name as 187 - k + k (`Test-HostConditionalDisclosureDelta`,
  `src/_roster.ps1:2053`), so AT. `score.tsv` shows them BELOW on raw matched; `final.tsv` carries the override.
- **net, linux-x64.** HX-NET (44459492597), routed by COORD after a lane's local read lost its network mid-row:
  597 + 3, `TestDialCancel` pass on both sides, AT. The read of record for that cell.

`ev/AR-5.tsv` (`net/netip`): the two `TestParsePrefixAllocs` subtests, whose names are IP prefixes, are written
`TestParsePrefixAllocs/*`, as the roster and the BOARD write them.
