# DATA: the default-mode canary set (owner ruling D1)

> Point-in-time measurement DATA. P5, 2026-11-22, for COORD's D1 brief (inbox P5 20261122T010251Z) under the
> owner's default-mode ruling D1 (FLEET 20261121T230700Z): every train that touches golib, gen or the converter
> runs a DEFAULT-MODE CANARY SWEEP leg (GO2CS_MN unset, no twins built) whose result must EQUAL master on the
> same box. This record derives the set from a stated census, times every candidate, packs a set for the i7,
> and shows the sweep red by name on a planted golib regression. The battery reads
> [`src/d1-canary-set.txt`](../../src/d1-canary-set.txt), one package path per line.
> Inputs, scripts and raw readings: [`data/d1-canary/`](data/d1-canary/).

## 1. The census predicate

Roster: `docs/ValidatedTestPackages.md` at master `1f8d62ddb3`, 238 banked rows (the roster row shape).
Instrument: `data/d1-canary/census/` (go/ast, no type checking) over each row's OWN GOROOT directory at
go1.26.11: package files and `_test.go` files, subdirectories and `testdata` excluded; a file counts when it
builds for linux OR windows on amd64 with cgo off. Per row it counts eight features, split into package code
and test code (`census.tsv`).

**Predicate.** A row is a candidate when its directory has at least one of:

- (a) anywhere, package or test code: a `go` statement, a `select` statement, a channel send, receive or
  channel type, a timer call (`time.After`, `AfterFunc`, `NewTimer`, `NewTicker`, `Tick`, `Sleep`), `io.Pipe`,
  a `net` `Listen*`/`Dial*`/`Pipe` call, any `net/http/httptest` use, or `context.With*`/`AfterFunc`;
- (b) in test code only: any `sync.*` or `sync/atomic.*` selector.

A package whose only hit is a mutex or a `Once` in its own package code is not a candidate, because its tests
do not drive it concurrently. **91 of 238 rows match**; 116 would match if (b) also counted package code.

**Misses** (rows judged concurrency-relevant that the predicate does not catch): `runtime/metrics` (scheduler
metrics), `runtime/debug` (GC and thread doors), `internal/godebug` (atomics plus the runtime's setting-update
callback), `net/http/httptrace` (hooks carried on a context through `WithValue`), `testing/iotest` (io error
paths). COORD added the first four as **judgement adds, not the predicate's** (inbox P5 20261122T011332Z);
they are timed and ranked like the rest and labelled in `plan.tsv`. `testing/iotest` was optional and is not
timed.

**Over-catches** (match, low exposure): `plugin`, `internal/testenv`, `image/jpeg`, `internal/strconv`,
`hash/crc32`, `encoding/binary`, `internal/zstd`. COORD's rule: cut these first if the budget binds.

## 2. Ranking

Families in COORD's order: sync, runtime-adjacent, time, context, net/http, io, fmt, then net (loopback), then
the rest. Within a family, by exposure score = test-code hits plus the `go`/`select` statements the package
itself starts (`rank.py`). The runtime-adjacent family is a stated list, not measured: `runtime`, `runtime/*`,
`internal/runtime/*`, `internal/poll`, `internal/synctest`, `testing/synctest`, `os/signal`, `unique`, `weak`,
`iter`, `testing`. (The timings ran in an earlier order with net before io; only the rank numbers changed.)

## 3. Timings

Every candidate and judgement add, one `run-validated-sweep.ps1 -Filter <row> -Exact` per row, default mode,
at master `1f8d62ddb3`, linux/amd64, 4 CPUs, run one at a time (`timings.tsv`: package, rc, driver seconds,
family, verdict line). COORD accepted one tree for sizing (inbox P5 20261122T031142Z); the gate's verdict is
"equal to master on the same box" at each train's own run, so the sizing tree does not bind it.
95 rows, 13,170 s in all.

- **Cold reads.** The box's disk forced six purges of the corpus build output (`purges.txt`); the row after a
  purge, and the first row, paid a cold dependency build. Five of them that could enter the set were re-read
  warm (`timings-warm.tsv`): `sync` 272 → 46 s, `weak` 212 → 19 s, `net/http/httptest` 339 → 121 s,
  `testing/synctest` 176 → 153 s, `expvar` 147 → 43 s. The others (`compress/gzip`, `html/template`,
  `image/png`, `os/user`) rank low and are marked cold in `plan.tsv`.
- **Red at master on this box** (host limits, all known): `net` (594 of 597, no IPv6 interface),
  `os` (3831 of 3840), `crypto/x509` (Go's own `TestSetFallbackRoots` fails here), `go/internal/gcimporter`
  (371 of 636), `internal/runtime/gc/scan` (the AVX-512 path).

## 4. Scaling to the i7, and the set

**Scale.** i7 seconds ≈ linux seconds × **1.26**, from two measured ratios:
(1) this box against the i9's windows sweep at go1.26.10 (`i9-go12610.tsv`, from
`h10-evidence/go1.26.10/sweep/i9-S.tsv` and `windows/i9-*.tsv`): median 1.91 over the 31 rows both read warm
(p25 1.41, p75 2.39);
(2) the i7 against the i9: 0.457 (`archive/zip`) and 0.373 (`hash/maphash`), mean 0.415
(`DATA-h10-shardmap-projection-go124.md` §6). 1 / (1.91 × 0.415) = 1.26. The band from those quartiles and
the two pairs is about 0.9× to 1.9×, so the 45 minutes below are an estimate to correct from the first
battery's own timing TSV (`scratchpad/sweep-row-walltimes/` on the i7).
**One override:** `internal/poll` reads 1,844 s here against 17 s on the i9 (a linux-only cost, 108× the
median ratio), so its estimate is the i9 reading / 0.415 = 41 s.

**Selection rule** (`pack.py`), budget 2,700 i7-s:
1. out: any row whose estimate exceeds 10 i7-minutes alone (`runtime` 2,050, `crypto/tls` 1,172,
   `runtime/pprof` 785), and `internal/runtime/gc/scan` (host-dependent red, score 1);
2. pass 1: in each COORD family, rows with score ≥ 20 plus each family's top row, in rank order, a row
   taken if it fits;
3. pass 2: the remaining family rows and judgement adds, in rank order, if they fit; then the rest.

**The set: 20 rows, 2,700 i7-s estimated (45.0 min), 3,954 s on this box.**

| Rank | Row | Family | Score | linux s | i7 est s |
|--:|:--|:--|--:|--:|--:|
| 1 | sync | sync | 235 | 46 | 58 |
| 2 | sync/atomic | sync | 49 | 191 | 241 |
| 3 | internal/sync | sync | 24 | 35 | 44 |
| 6 | internal/synctest | runtime-adj | 178 | 59 | 74 |
| 7 | testing | runtime-adj | 121 | 76 | 96 |
| 8 | os/signal | runtime-adj | 88 | 60 | 76 |
| 9 | internal/runtime/atomic | runtime-adj | 44 | 30 | 38 |
| 10 | internal/poll | runtime-adj | 24 | 1844 | 41 |
| 12 | weak | runtime-adj | 16 | 19 | 24 |
| 17 | time | time | 205 | 376 | 474 |
| 18 | context | context | 198 | 44 | 55 |
| 19 | net/http | net/http | 1310 | 350 | 441 |
| 20 | net/http/httputil | net/http | 124 | 99 | 125 |
| 21 | net/http/httptest | net/http | 114 | 121 | 152 |
| 22 | net/http/pprof | net/http | 22 | 68 | 86 |
| 27 | os | io | 188 | 147 | 185 |
| 28 | os/exec | io | 85 | 182 | 229 |
| 29 | io | io | 61 | 38 | 48 |
| 31 | fmt | fmt | 3 | 42 | 53 |
| 33 | net/rpc | net | 48 | 127 | 160 |

Next in line, if the i7's first reading leaves room: `testing/synctest` 193, `unique` 53, `runtime/trace` 57,
`iter` 20, `bufio` 55, `net` 422 (the loopback row itself; it would replace `net/rpc` plus about 260 s),
`net/smtp` 52, `net/rpc/jsonrpc` 55, `database/sql` 84. `os` is red at master on this box and was, at an older
tree, the i7's standing `os`/`testing` class; under the equality gate a row red at master still compares
(its count must equal master's), which the planted run below shows.

## 5. Made to fail once: a planted golib regression

**Plant** (`plant.diff`, a throwaway local branch on `1f8d62ddb3`, never pushed): `SelectRuntime.TryRun`, the
non-blocking select (the `default:` form), drops from its poll order every receive case whose channel is closed
and drained, so `select { case <-done: ...; default: }` takes `default` after `done` is closed. Blocking select,
plain receive and close are untouched. Predictions were written before the run (`plant-pred.txt`).

**Run:** the 20-row set, default mode, same box, one sweep invocation per row (`plant-results.tsv`).
Compared with the master readings of §3:

| Row | Master | Planted | Named by the sweep |
|:--|:--|:--|:--|
| sync | PASS 48 | **FAIL** | package deadline; running `TestConcurrentRange` |
| os/signal | PASS 30 | **FAIL** | package deadline; running `TestTime` |
| weak | PASS 9 | **FAIL** | package deadline; running `TestIssue69210` |
| time | PASS 176 | **FAIL** | `TestChan` (all nine asynctimerchan subtests) |
| context | PASS 62 | **FAIL** | `TestWithCancel`, `TestCanceledTimeout`, `TestWithCancelCanceledParent`, `TestParentFinishesChild`, `TestChildFinishesFirst`, `TestLayersCancel`, `TestCause` (3 subtests), `TestCustomContextGoroutines`, `TestContextErrDoneRace` |
| net/http | PASS 1538 | **FAIL** | `TestClientConnReserveAndConsume`, `TestServerRequestContextCancel_ServeHTTPDone`, `TestServerShutdownStateNew`, `TestTransportCancelRequestInDo`, `TestTransportCancelRequestWithBodyInDo`, `TestTransportNewClientConnRoundTrip` |
| os | COUNT 3831 of 3840 | COUNT 3831 of 3840 | EQUAL to master (red both sides) |
| the other 13 | PASS | PASS, same counts | EQUAL |

Every planted invocation that changed exited non-zero and named its row; six of 20 rows moved, and the
row red at master stayed equal, so the gate reads the change and not the host.

**Predictions, scored as worded:**
- P1, context reds by name: **MET**. Five of the six named tests failed; `TestLayersTimeout` passed
  (named, missed); five more failed that were not named.
- P2, net/http, httputil, os/exec may red: net/http **red**; httputil and os/exec stayed green.
- P3, sync, sync/atomic, internal/sync and time stay green: **MISSED** for `sync` and `time`. `sync`'s
  `TestConcurrentRange` loops on a select-with-default over a closed `done` (map_test.go:170), a site my
  grep had listed and I did not follow; `time`'s `TestChan` reads closed timer channels the same way.
  sync/atomic and internal/sync held.
- P4, the sweep exits non-zero and names every red row: **MET**, per invocation.

**Cost note:** three of the six reds were package-deadline kills (10 minutes each here), so a planted
hang costs the battery about 10 minutes per hung row beyond its normal time.

**Battery wiring.** `run-validated-sweep.ps1 -Filter` takes one string, so the battery loops the file:
`-Filter <line> -Exact -SkipBuild` per line after one converter build, and compares each row's verdict and
count with master's on the same box. A `-PackageList <file>` parameter would make it one invocation; P5 can
cut that as a small seat if COORD wants it.
