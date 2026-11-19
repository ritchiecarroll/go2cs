# PLAN — the re-bank deal for go.* 1.26.11.1

> **State: READY FOR RULING (P5, 2026-11-18), on `claude/p5-dot4-rebank-deal` from master `894570ae1c`.** It assigns
> every roster row to a reading lane on each OS for the 1.26.11.1 re-bank and projects when each lane finishes.
> Nothing here dispatches a row. It dispatches once i9's tests regeneration lands and COORD names the tip. The
> procedure is [`GoCorpusMigration.md`](GoCorpusMigration.md) H10 and its 2026-10-24 amendment (item 5, the darwin
> leg). The structure follows [`PLAN-release-1-26-10-2.md`](PLAN-release-1-26-10-2.md), *The re-bank legs*.

The deal is [`data/rebank-1.26.11.1/legs.tsv`](phase4/data/rebank-1.26.11.1/legs.tsv) (`<LEGS>` below), one line per
(os, lane, shard, seq, row) with `projected_s` and the source of that wall. Each list runs smallest-first, and
`testing` runs last in its list. [`deal.py`](phase4/data/rebank-1.26.11.1/deal.py) writes it (`cut`) and re-checks
it against a tree (`check`). Its input is the roster read through `src/_roster.ps1`'s `Get-ValidatedRosterRows`, by
[`roster_dump.ps1`](phase4/data/rebank-1.26.11.1/roster_dump.ps1); the dump at `894570ae1c` is
[`roster.tsv`](phase4/data/rebank-1.26.11.1/roster.tsv).

## 1. The eligible sets

| OS | Eligible | Read from the roster reader | Not eligible |
|:--|--:|:--|:--|
| windows | **238** | every banked row | none |
| linux (amd64) | **236** | a numeric `linux:` segment | `internal/syscall/windows`, `.../registry` (`linux: n/a`) |
| linux-arm64 | **236** | the `linux-arm64` RID note on the `linux:` segment, new at 1.26.11.1 | the same two |
| darwin (both RIDs) | **236** | a numeric `darwin:` segment (3 carry an arch-strict RID note) | the same two |

`$longTimeouts` holds 14 keys, all linux-eligible: the reserved set.

## 2. Where the walls come from

Every wall is the latest measured reading of that row, and `<LEGS>` names its file:

| OS | Source | Lanes measured |
|:--|:--|:--|
| windows | `rebank-1.26.10.3/sweep/`: `i9-S` (182 rows), `H-S` (55), `GH-S` (`net`), `R-W` (`crypto/tls`, full count) | i9, H, hosted, R-LAPTOP |
| linux | `rebank-1.26.10.3/linux/`: `RWSL-1`, `RWSL-2`, `P1-1`, `P1-2`, `P2-1`, `P2-2`, `C1-R` | R-WSL, P1, P2, hosted |
| linux-arm64 | `rebank-1.26.10.4/linux-arm64/evidence/` (waves 1-3, re-reads, the go1.26.11 H10 reads) | hosted, both RIDs |
| darwin | `rebank-1.26.10.3/darwin/` (C1-1 … C1-15, the re-reads, the go1.26.11 H10 read) | hosted, both RIDs |

**Windows.** A row stays on the box that measured it, at its measured wall, except that G has no .3 reading. G's 53
rows are estimates from the .2 box factors (G 0.655, i9 0.751, H 1.072), each marked `est:`.

**Linux: the .2 cost model is replaced.** .2 put a cloud row at its WSL wall plus about 275 s. At .3 that model was
2 to 4 times too high: P1 projected 4.94 h and measured 1.32 h, and P2 projected 5.03 h and measured 3.07 h. The arm64
bank read all 236 rows on the hosted linux-x64 runner as well, so every row now has one wall on one host class. Each
lane's factor is its .3 walls over the hosted walls of the same rows, derived by `deal.py` at run time: **R-WSL 1.74,
P1 1.68, P2 3.44** (P2's kit carries a per-row floor). A row keeps its measured wall on the lane that read it, and is
otherwise the hosted wall times the lane's factor (`est:`). **P4 has no reading; it takes P2's 3.44, the slower.**

**Hosted.** A linux job costs 120 s outside its rows (measured: job start to sweep plus upload, wave 3's A-1). A
darwin job costs 300 s (.2's figure). Seven arm64 jobs ran at once in wave 1, and the account runs 5 mac jobs at once.

## 3. Pins (rows whose reading depends on the host)

| Row | OS | Lane | Why |
|:--|:--|:--|:--|
| `net` | windows | the hosted windows runner (`GH-S`) | the LAN resolver answers SERVFAIL (.3 ruling (1)) |
| `crypto/tls` | windows | R-LAPTOP, solo | the full count 4920 needs 16 cores; H reads 1393 (.3, class (h)) |
| `runtime` | windows | H, 90-minute deadline | the i9's `GOMAXPROCS=4` cap disables `TestGOMAXPROCSUpdate` |
| `os`, `archive/tar` | windows | i9 | host truth (.3 §6: pagefile skip, Developer Mode) |
| off-list (`crypto/*`, `net/http`, `net/http/*`, `net/smtp`, `os/exec`) | windows | H or G, never the i9 | owner's box rule |
| `os` | linux | the hosted linux-x64 runner (`HX-1`) | COORD 2026-11-10: os's linux count is the hosted runner's |
| `internal/runtime/gc/scan` | linux | a WSL lane (no AVX-512) | the P1 container and the hosted runner read 9261, not the banked 3095 (AVX-512) |
| `crypto/tls` | linux | a WSL lane | the full count 4921 was read on a 16-core box |
| the reserved set | linux | R-WSL (mode R) | disk and memory: `archive/zip`'s 4 GiB streams, `runtime`, `index/suffixarray` |

## 4. The shards (mode R, recommended)

| OS | Lane (account) | Shard | Rows | Projected | Holds |
|:--|:--|:--|--:|--:|:--|
| windows | i9 (B) | i9-S | 182 | 1.50 h | every row the i9 may read |
| windows | H (A) | H-S | 1 | 0.96 h | `runtime` |
| windows | G-LAPTOP (A) | G-S | 53 | 0.73 h (est.) | the off-list rows; 1.2 h if G reads at H's .3 speed |
| windows | R-LAPTOP (B) | R-S | 1 | 0.40 h | `crypto/tls` full count |
| windows | hosted (B dispatches) | GH-S | 1 | 0.22 h | `net` |
| linux | R-WSL (B) | RWSL-1 | 15 | 3.17 h, after R-S: **finish 3.57 h** | the reserved set, `gc/scan`, `crypto/tls`; `runtime` 3737 s |
| linux | P1 (B) | P1-1 | 109 | 2.16 h | `testing` last |
| linux | P2 (B) | P2-1 | 52 | 2.15 h | |
| linux | P4 (B) | P4-1 | 59 | 2.15 h | at P2's factor |
| linux | hosted (B dispatches) | HX-1 | 1 | 0.12 h | `os`, `arch=x64` |
| linux-arm64 | hosted (B dispatches) | AR-1 … AR-17 | 236 | **0.83 h** at 7 jobs at once | `internal/poll`, `runtime`, `crypto/tls` alone |
| darwin | hosted (B dispatches) | DW-1 … DW-14 | 236 | **3.64 h** at 5 mac jobs at once | `runtime` (DW-1), `crypto/tls` (DW-2) alone |

Linux lanes include 15 minutes of clone and build each. A row of 1,200 s or more on any RID dispatches alone, so its
re-read re-reads nothing else. The cutter takes the fewest dispatches within 5 minutes of the earliest finish.

**Lane totals:** i9 1.50 h; H 0.96 h; G 0.73 h; R-LAPTOP 3.57 h (R-S then RWSL-1, one battery per box); P1, P2 and P4
2.15 h each; hosted linux-arm64 5.1 runner-hours (4.6 h of rows); hosted darwin 17.9 mac-job-hours (11.4 h on the slower RID).

## 5. The critical path

- **Latest finish: 3.64 h from dispatch, darwin**, with R-LAPTOP level at 3.57 h. Dispatch DW-1 (`runtime`) and DW-2
  (`crypto/tls`) first, then the rest longest-first; GitHub queues what exceeds 5 mac jobs.
- **Darwin `runtime` is the widest wall:** osx-x64 has read it at 1,808 to 5,811 s, and the deal uses the latest,
  4,544 s. At 5,811 s DW-1 runs 1.7 h, still inside the darwin span.
- Hosted concurrency: 7 arm64 + 5 mac + `GH-S` + `HX-1` = 14 jobs, inside the account's 20.
- **Mode C** (R-LAPTOP reads `crypto/tls` on windows only): the cloud lanes take the reserved set, G-WSL takes
  `gc/scan` and linux `crypto/tls` after G-S, and linux finishes at **3.69 h**. It costs 3 minutes of latest finish, but
  it puts `archive/zip`'s 4 GiB streams and `runtime` on containers whose disk stopped P2 at its 15 GB floor at .2,
  and linux `crypto/tls` on a box whose full count is unproven.

## 6. Monitoring cost, per lane

- **i9:** one sweep shard; a launch, a ledger look every 30 minutes, and a DONE read.
- **H and G-LAPTOP:** one sweep shard each; a launch and a result read. Machine time only.
- **R-LAPTOP:** two launches, R-S then RWSL-1, with the disk floor (25 GB) read at each row boundary.
- **P1, P2, P4:** the linux kit, resumable from its ledger; about one look an hour plus a DONE post with its evidence
  ref. The most token-heavy legs, and the only ones a container restart can interrupt.
- **Hosted (one B lane with gh):** 33 dispatches in one burst (17 arm64, 14 darwin, `GH-S`, `HX-1`), then a read
  every 30 to 45 minutes as runs complete. Annotation reads the artifacts with the existing annotate scripts.

## 7. Checks

`deal.py check` re-reads the eligible sets at a tree and asserts: each OS's dealt rows equal its eligible set, each
exactly once; every seq runs 1..n and `testing` ends its list; each hosted dispatch holds at most 20 rows; no off-list
row, `runtime` or `net` is on the i9; every pin in §3 holds; and in mode R the reserved set is on R-WSL. It passes at
`894570ae1c`. It was made to fail first, with five planted defects (a linux row dropped, a darwin row dealt twice, an
off-list row on the i9, `testing` renumbered to the front of its list, linux `os` moved to P1), and each failed naming
its row. **At the dispatch tip, re-dump the roster and run `check`; if any eligible set moved, run `cut` and post the
delta.**

## 8. For ruling

1. Mode R (recommended) or mode C.
2. The pins in §3 as stated, `net` on the hosted windows runner and linux `os` on the hosted linux-x64 runner included.
3. G-LAPTOP reading 53 off-list rows at estimated walls (it has no .3 reading; the windows leg ends inside linux and
   darwin either way).

## 9. Lessons from the run (2026-11-19, P5, final hosted burst at `280a01952c`)

1. **A row banked with a sweep option is dealt with that option.** The darwin `net` segment was banked from a read with
   the sweep-shard `dns` option (public resolvers; `NET-DNS`, run 42617546338). This deal put `net` in DW-5 without
   it, and the Go oracle failed `TestLookupNoSuchHost/LookupTXT_NXDOMAIN` on osx-arm64 against the runner's resolver
   (C# passed). The declared re-read DW-5R carried the option. The next deal reads each row's banked evidence for the
   options it was read with and deals the row with them (COORD 20261119T064301Z).
2. **The darwin collector must not stop at a void row.** In the first pass at `6e870821d6`, `collect_darwin.py` exited
   at the first row with no comparison record, and every later row of that shard lost its evidence line. Fixed in
   batch 314 (`f5e51abe33`): the row is printed as `no-record` and the read goes on.
3. **Deal linux-arm64 in a form the annotator can read.** Since S7 every linux segment carries a linux-arm64 note, so
   `annotate_linux.py`'s single-RID mode refuses every row, and its `--rids` mode needs a linux-x64 and a linux-arm64
   line for the row from ONE dispatch. This deal read linux-x64 on the lanes and linux-arm64 hosted alone, so no
   annotator could derive the bank's linux segments; the bank was matched cell by cell instead
   (`data/rebank-1.26.11.1/bank_check.py`, COORD 20261119T091154Z). The next deal dispatches the hosted arm64 shards
   arch-both (the sweep-shard `arch` input), so the same artifacts carry both RIDs, and keeps the lanes' linux-x64
   reads as the timing record.
4. **A cancelled job leaves rows the darwin annotator cannot read.** DW-7's osx-x64 job was cancelled by the runner, so
   its 18 finished rows have PASS lines and no comparison record (`ev/XDW-7.tsv`). A re-dispatch of the whole shard
   (an extending stem, DW-7R) keeps the evidence in the annotator's form; re-reading only the unread rows does not.
