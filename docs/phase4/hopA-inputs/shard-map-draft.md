# SHARD MAP DRAFT — the hop-era fleet shard map, computed from JOB-007

> **DRAFT — pending coordinator review and hop-recon factor calibration.** Computed 2026-08-22
> from the JOB-007 per-row wall times (`docs/phase4/DATA-sweep-row-walltimes.md`, windows ·
> corpus `18770d083` · i9-13900K, 2026-08-23), the H10 construction in
> `docs/PLAN-hop-campaign.md` §4.3, and the canonical fleet roster in `docs/phase4/LANES.md`.
> **Every speed factor below is a PROVISIONAL PLACEHOLDER** — LANES.md marks historical
> cross-machine ratios SUSPECT ("the hop shard map's speed factors come from FRESH same-workload
> calibration at campaign recon, never from pre-anchor history"). The assignments are the
> method's output at the placeholder factors; the map is re-emitted at recon once `s_w` and `k`
> are measured. Nothing in this file is a repo change; the paste-ready section at the end is the
> proposed insertion, gated on coordinator review.

---

## 1. The dataset — JOB-007, parsed

**162 rows · 18,569 verdicts · 7,701 i9-seconds total (128.3 min single-machine serial)** — the
parsed deltas sum to 7,701 s against the sweep's own 7,697 s aggregate, matching the DATA file's
recorded self-check (the 4 s spread is the derivation's stated mtime-delta noise).

| Statistic | Value |
|:--|--:|
| rows | 162 |
| total wall | 7,701 i9-s (128.3 min) |
| median row | 10 s |
| mean row | 47.5 s |
| p75 / p90 / p95 | 16 s / 71 s / 226 s |

**Distribution shape — extreme right skew.** The bulk of the roster is per-row overhead; the wall
lives in a handful of rows:

| Bucket | Rows | Wall | Share of total |
|:--|--:|--:|--:|
| ≤ 10 s | 95 | 831 s | 10.8 % |
| 11–30 s | 41 | 633 s | 8.2 % |
| 31–60 s | 8 | 357 s | 4.6 % |
| 61–120 s | 6 | 549 s | 7.1 % |
| 121–300 s | 6 | 1,224 s | 15.9 % |
| **> 300 s** | **6** | **4,107 s** | **53.3 %** |

**Top-10 heaviest rows — 5,017 s = 65.1 % of the total wall:**

| Row | Verdicts | Wall |
|:--|--:|--:|
| `crypto/dsa` | 4 | 1,317 s |
| `hash/maphash` | 22 | 898 s |
| `crypto/tls` | 400 | 659 s |
| `index/suffixarray` | 12 | 573 s |
| `archive/zip` | 100 | 354 s |
| `go/internal/gcimporter` | 583 | 306 s |
| `go/parser` | 173 | 259 s |
| `crypto/internal/mlkem768` | 12 | 228 s |
| `regexp` | 45 | 226 s |
| `time` | 159 | 197 s |

The DATA file's own header names the two Go-vs-C# inversions this table confirms: `crypto/dsa`
1,317 s for 4 verdicts and `hash/maphash` 898 s for 22 — verdict counts are a bad proxy for wall
time in both directions (OQ-H5's exact argument: `go/doc/comment` carries 10,059 verdicts at 18 s
in this reading, `archive/zip` 100 verdicts at 354 s).

---

## 2. Method and inputs

**Construction:** `PLAN-hop-campaign.md` §4.3, followed literally —

1. rows := the 162 JOB-007 rows (re-read at the hop branch tip before dispatch, never carried);
2. R := the reserved set ∩ rows — all 7 present (`index/suffixarray`, `hash/maphash`,
   `crypto/dsa`, `archive/zip`, `crypto/tls`, `go/doc/comment`, `go/types`), **3,956 i9-s
   (65.9 min), pinned to the i9**;
3. B := the remaining **155 rows, 3,745 i9-s**, sorted DESC by `t_r` (deterministic name
   tie-break), assigned **LPT-greedy**: each row to the bin with the smallest current projected
   local time (`load / s_w`), the plan's literal rule;
4. any bin whose `load / s_w` exceeds C = **5,400 s (~90 min)** splits into sequential shards —
   at these factors and k = 1, **no bin splits at any W**;
5. checksum: |rows| = |R| + |B| = 7 + 155 = 162 — holds for every W below.

**Interpretation note for coordinator review:** §4.3's step 3 (recon: P dealt round-robin ASC)
and step 4 (bulk: B LPT-greedy DESC) both range over `rows \ R`. This draft reads the recon deal
as an **execution-ordering directive, not a second allocation**: all 155 non-reserved rows are
*allocated* by LPT, and each worker *runs* its bin cheapest-first, so the campaign's opening rows
are the cheap ones and `k` is measured on the first ten per the plan's own placeholder note. If
the coordinator intended recon as a separate dealt tranche, the bins move by single-digit
seconds — the cheap rows are 6–10 s each — and the makespans below are unchanged to the minute.

**Fleet and provisional speed factors** (`s_w`, i9 = 1.00 — **PLACEHOLDERS, all four**, to be
replaced by the LANES.md calibration pair at hop recon; the 0.35/0.45 values are rough
readings-of-record — CLAUDE.md's "roughly 3–4× the i9" for the i7-5820K class — not measurements
of these boxes on this workload):

| W | Worker | Silicon | `s_w` (provisional) |
|:--:|:--|:--|--:|
| 3+ | **i9 (sweeper)** | i9-13900K, 16C/24T | **1.00** (definition) |
| 3+ | **R** (R-LAPTOP) | Ryzen 7 PRO 6850U, 8C/16T | **0.45** |
| 3+ | **coordinator** | i7-5820K, 6C/12T | **0.35** |
| 4+ | **G** (G-LAPTOP) | Ryzen 5 PRO 6650U, 6C/12T | **0.35** |
| 5 | **X** (fifth engaged machine) | unknown — placeholder silicon | **0.35** (assumed laptop-class) |

`t_r` is the JOB-007 wall time × `k`; **k = 1 is assumed throughout this draft** (see §4's caveat
— the DATA header describes the measured wall as convert + build + both hosts + compare, but the
plan carries `k` as a recon-measured placeholder and this draft does not preempt that).

---

## 3. The assignments

Reserved rows are marked †; the number after each row is its JOB-007 i9-seconds. Load is in
i9-seconds; "local wall" divides by the provisional `s_w`.

#### W = 3 -- makespan 4,289 s local (~71 min)

| Worker | s_w | Rows | Load (i9-s) | Local wall | Shards @ ~90 min |
|:--|--:|--:|--:|--:|--:|
| **i9-13900K (sweeper)** | 1.00 | 45 | 4,272 | 4,272 s (~71 min) | 1 |
| **R -- R-LAPTOP 6850U** | 0.45 | 66 | 1,928 | 4,284 s (~71 min) | 1 |
| **i7-5820K (coordinator)** | 0.35 | 51 | 1,501 | 4,289 s (~71 min) | 1 |

**i9-13900K (sweeper)** (45 rows):
> `index/suffixarray`† 573 · `hash/maphash`† 898 · `crypto/dsa`† 1317 · `archive/zip`† 354 · `crypto/tls`† 659 · `go/doc/comment`† 18 · `go/types`† 137 · `crypto/aes` 9 · `crypto/hmac` 9 · `crypto/sha256` 9 · `database/sql/driver` 9 · `debug/plan9obj` 9 · `encoding/hex` 9 · `go/constant` 9 · `hash/crc32` 9 · `internal/coverage/cformat` 9 · `internal/coverage/slicereader` 9 · `internal/cpu` 9 · `internal/diff` 9 · `internal/testenv` 9 · `internal/xcoff` 9 · `io/ioutil` 9 · `log/slog/internal/benchmarks` 9 · `testing/iotest` 9 · `text/scanner` 9 · `container/list` 8 · `crypto/des` 8 · `crypto/md5` 8 · `encoding/base32` 8 · `encoding/base64` 8 · `hash/fnv` 8 · `image/color` 8 · `internal/coverage/slicewriter` 8 · `internal/itoa` 8 · `maps` 8 · `math/cmplx` 8 · `path` 8 · `plugin` 8 · `runtime/internal/math` 8 · `cmp` 7 · `container/heap` 7 · `crypto/internal/alias` 7 · `hash/adler32` 7 · `internal/saferio` 7 · `math/bits` 7

**R -- R-LAPTOP 6850U** (66 rows):
> `go/internal/gcimporter` 306 · `crypto/internal/mlkem768` 228 · `time` 197 · `crypto/rsa` 119 · `compress/flate` 106 · `sync/atomic` 82 · `crypto/internal/edwards25519/field` 66 · `strings` 56 · `database/sql` 47 · `os/exec` 43 · `crypto/ecdsa` 32 · `encoding/json` 28 · `text/template` 25 · `compress/gzip` 21 · `regexp/syntax` 20 · `bytes` 18 · `debug/buildinfo` 18 · `sync` 18 · `crypto/subtle` 16 · `strconv` 16 · `crypto/ecdh` 15 · `crypto/ed25519` 15 · `image/png` 14 · `net/http/fcgi` 14 · `syscall` 14 · `crypto/rand` 13 · `expvar` 13 · `crypto/internal/bigmod` 12 · `flag` 12 · `os/signal` 12 · `debug/gosym` 11 · `go/ast` 11 · `go/printer` 11 · `runtime/debug` 11 · `debug/macho` 10 · `encoding/asn1` 10 · `errors` 10 · `go/format` 10 · `hash` 10 · `internal/abi` 10 · `internal/fuzz` 10 · `internal/reflectlite` 10 · `io/fs` 10 · `mime/quotedprintable` 10 · `net/mail` 10 · `net/url` 10 · `testing/slogtest` 10 · `bufio` 9 · `compress/bzip2` 9 · `crypto/rc4` 9 · `encoding/binary` 9 · `go/token` 9 · `internal/coverage/pods` 9 · `internal/dag` 9 · `io` 9 · `math` 9 · `unicode/utf16` 9 · `crypto/internal/boring` 8 · `go/version` 8 · `internal/buildcfg` 8 · `internal/sysinfo` 8 · `os/exec/internal/fdtest` 8 · `text/tabwriter` 8 · `container/ring` 7 · `internal/gover` 7 · `unicode/utf8` 6

**i7-5820K (coordinator)** (51 rows):
> `go/parser` 259 · `regexp` 226 · `internal/godebugs` 177 · `encoding/pem` 105 · `mime/multipart` 71 · `archive/tar` 60 · `encoding/xml` 54 · `math/rand/v2` 34 · `math/rand` 31 · `html/template` 28 · `go/importer` 20 · `go/internal/srcimporter` 19 · `compress/zlib` 18 · `crypto/elliptic` 16 · `sort` 16 · `crypto` 15 · `image/gif` 14 · `internal/types/errors` 14 · `net/rpc/jsonrpc` 14 · `context` 13 · `path/filepath` 13 · `debug/elf` 12 · `image` 12 · `debug/dwarf` 11 · `go/internal/gccgoimporter` 11 · `image/jpeg` 11 · `crypto/internal/hpke` 10 · `encoding/csv` 10 · `fmt` 10 · `go/scanner` 10 · `image/draw` 10 · `internal/profile` 10 · `internal/zstd` 10 · `mime` 10 · `net/textproto` 10 · `runtime/metrics` 10 · `text/template/parse` 10 · `compress/lzw` 9 · `crypto/sha512` 9 · `go/build/constraint` 9 · `internal/coverage/cmerge` 9 · `internal/singleflight` 9 · `log` 9 · `testing/quick` 9 · `crypto/sha1` 8 · `hash/crc64` 8 · `internal/fmtsort` 8 · `net/http/internal/ascii` 8 · `unicode` 8 · `encoding/ascii85` 7 · `runtime/internal/sys` 7

Checksum: **162 rows assigned = 7 reserved + 155 bulk** (every roster row named exactly once).

#### W = 4 -- makespan 3,956 s local (~66 min)

| Worker | s_w | Rows | Load (i9-s) | Local wall | Shards @ ~90 min |
|:--|--:|--:|--:|--:|--:|
| **i9-13900K (sweeper)** | 1.00 | 7 | 3,956 | 3,956 s (~66 min) | 1 |
| **R -- R-LAPTOP 6850U** | 0.45 | 61 | 1,468 | 3,262 s (~54 min) | 1 |
| **i7-5820K (coordinator)** | 0.35 | 47 | 1,138 | 3,251 s (~54 min) | 1 |
| **G -- G-LAPTOP 6650U** | 0.35 | 47 | 1,139 | 3,254 s (~54 min) | 1 |

**i9-13900K (sweeper)** (7 rows):
> `index/suffixarray`† 573 · `hash/maphash`† 898 · `crypto/dsa`† 1317 · `archive/zip`† 354 · `crypto/tls`† 659 · `go/doc/comment`† 18 · `go/types`† 137

**R -- R-LAPTOP 6850U** (61 rows):
> `go/parser` 259 · `regexp` 226 · `crypto/rsa` 119 · `encoding/pem` 105 · `crypto/internal/edwards25519/field` 66 · `encoding/xml` 54 · `os/exec` 43 · `math/rand` 31 · `html/template` 28 · `go/importer` 20 · `go/internal/srcimporter` 19 · `debug/buildinfo` 18 · `crypto/subtle` 16 · `strconv` 16 · `crypto/ed25519` 15 · `image/png` 14 · `net/rpc/jsonrpc` 14 · `crypto/rand` 13 · `path/filepath` 13 · `debug/elf` 12 · `os/signal` 12 · `go/ast` 11 · `image/jpeg` 11 · `runtime/debug` 11 · `encoding/asn1` 10 · `fmt` 10 · `hash` 10 · `internal/abi` 10 · `internal/profile` 10 · `io/fs` 10 · `net/mail` 10 · `runtime/metrics` 10 · `text/template/parse` 10 · `compress/bzip2` 9 · `crypto/hmac` 9 · `crypto/sha512` 9 · `encoding/binary` 9 · `encoding/hex` 9 · `go/token` 9 · `internal/coverage/cmerge` 9 · `internal/cpu` 9 · `internal/diff` 9 · `internal/testenv` 9 · `io/ioutil` 9 · `math` 9 · `text/scanner` 9 · `unicode/utf16` 9 · `crypto/internal/boring` 8 · `encoding/base32` 8 · `hash/crc64` 8 · `internal/buildcfg` 8 · `internal/coverage/slicewriter` 8 · `internal/sysinfo` 8 · `net/http/internal/ascii` 8 · `plugin` 8 · `text/tabwriter` 8 · `cmp` 7 · `crypto/internal/alias` 7 · `internal/gover` 7 · `math/bits` 7 · `unicode/utf8` 6

**i7-5820K (coordinator)** (47 rows):
> `crypto/internal/mlkem768` 228 · `time` 197 · `compress/flate` 106 · `mime/multipart` 71 · `strings` 56 · `math/rand/v2` 34 · `encoding/json` 28 · `compress/gzip` 21 · `bytes` 18 · `sync` 18 · `sort` 16 · `crypto/ecdh` 15 · `internal/types/errors` 14 · `syscall` 14 · `expvar` 13 · `flag` 12 · `debug/dwarf` 11 · `go/internal/gccgoimporter` 11 · `crypto/internal/hpke` 10 · `encoding/csv` 10 · `go/format` 10 · `image/draw` 10 · `internal/reflectlite` 10 · `mime` 10 · `net/textproto` 10 · `testing/slogtest` 10 · `compress/lzw` 9 · `crypto/rc4` 9 · `database/sql/driver` 9 · `go/build/constraint` 9 · `hash/crc32` 9 · `internal/coverage/pods` 9 · `internal/dag` 9 · `internal/xcoff` 9 · `log` 9 · `testing/iotest` 9 · `container/list` 8 · `crypto/md5` 8 · `encoding/base64` 8 · `hash/fnv` 8 · `internal/fmtsort` 8 · `maps` 8 · `os/exec/internal/fdtest` 8 · `runtime/internal/math` 8 · `container/heap` 7 · `encoding/ascii85` 7 · `internal/saferio` 7

**G -- G-LAPTOP 6650U** (47 rows):
> `go/internal/gcimporter` 306 · `internal/godebugs` 177 · `sync/atomic` 82 · `archive/tar` 60 · `database/sql` 47 · `crypto/ecdsa` 32 · `text/template` 25 · `regexp/syntax` 20 · `compress/zlib` 18 · `crypto/elliptic` 16 · `crypto` 15 · `image/gif` 14 · `net/http/fcgi` 14 · `context` 13 · `crypto/internal/bigmod` 12 · `image` 12 · `debug/gosym` 11 · `go/printer` 11 · `debug/macho` 10 · `errors` 10 · `go/scanner` 10 · `internal/fuzz` 10 · `internal/zstd` 10 · `mime/quotedprintable` 10 · `net/url` 10 · `bufio` 9 · `crypto/aes` 9 · `crypto/sha256` 9 · `debug/plan9obj` 9 · `go/constant` 9 · `internal/coverage/cformat` 9 · `internal/coverage/slicereader` 9 · `internal/singleflight` 9 · `io` 9 · `log/slog/internal/benchmarks` 9 · `testing/quick` 9 · `crypto/des` 8 · `crypto/sha1` 8 · `go/version` 8 · `image/color` 8 · `internal/itoa` 8 · `math/cmplx` 8 · `path` 8 · `unicode` 8 · `container/ring` 7 · `hash/adler32` 7 · `runtime/internal/sys` 7

Checksum: **162 rows assigned = 7 reserved + 155 bulk** (every roster row named exactly once).

#### W = 5 -- makespan 3,956 s local (~66 min)

| Worker | s_w | Rows | Load (i9-s) | Local wall | Shards @ ~90 min |
|:--|--:|--:|--:|--:|--:|
| **i9-13900K (sweeper)** | 1.00 | 7 | 3,956 | 3,956 s (~66 min) | 1 |
| **R -- R-LAPTOP 6850U** | 0.45 | 47 | 1,124 | 2,498 s (~42 min) | 1 |
| **i7-5820K (coordinator)** | 0.35 | 36 | 873 | 2,494 s (~42 min) | 1 |
| **G -- G-LAPTOP 6650U** | 0.35 | 36 | 874 | 2,497 s (~42 min) | 1 |
| **X -- fifth engaged machine** | 0.35 | 36 | 874 | 2,497 s (~42 min) | 1 |

**i9-13900K (sweeper)** (7 rows):
> `index/suffixarray`† 573 · `hash/maphash`† 898 · `crypto/dsa`† 1317 · `archive/zip`† 354 · `crypto/tls`† 659 · `go/doc/comment`† 18 · `go/types`† 137

**R -- R-LAPTOP 6850U** (47 rows):
> `go/parser` 259 · `time` 197 · `sync/atomic` 82 · `archive/tar` 60 · `encoding/xml` 54 · `math/rand/v2` 34 · `html/template` 28 · `regexp/syntax` 20 · `compress/zlib` 18 · `crypto/elliptic` 16 · `strconv` 16 · `image/gif` 14 · `net/http/fcgi` 14 · `context` 13 · `path/filepath` 13 · `image` 12 · `go/ast` 11 · `image/jpeg` 11 · `crypto/internal/hpke` 10 · `errors` 10 · `hash` 10 · `internal/fuzz` 10 · `internal/reflectlite` 10 · `mime/quotedprintable` 10 · `runtime/metrics` 10 · `compress/bzip2` 9 · `compress/lzw` 9 · `crypto/sha256` 9 · `encoding/binary` 9 · `go/token` 9 · `internal/coverage/cmerge` 9 · `internal/coverage/slicereader` 9 · `internal/singleflight` 9 · `io/ioutil` 9 · `testing/iotest` 9 · `testing/quick` 9 · `crypto/des` 8 · `encoding/base32` 8 · `hash/fnv` 8 · `internal/fmtsort` 8 · `internal/itoa` 8 · `net/http/internal/ascii` 8 · `runtime/internal/math` 8 · `container/heap` 7 · `container/ring` 7 · `internal/gover` 7 · `unicode/utf8` 6

**i7-5820K (coordinator)** (36 rows):
> `regexp` 226 · `internal/godebugs` 177 · `mime/multipart` 71 · `database/sql` 47 · `encoding/json` 28 · `compress/gzip` 21 · `bytes` 18 · `crypto/subtle` 16 · `crypto/ecdh` 15 · `internal/types/errors` 14 · `crypto/rand` 13 · `debug/elf` 12 · `debug/dwarf` 11 · `go/printer` 11 · `encoding/asn1` 10 · `go/format` 10 · `internal/abi` 10 · `io/fs` 10 · `net/textproto` 10 · `text/template/parse` 10 · `crypto/hmac` 9 · `database/sql/driver` 9 · `go/build/constraint` 9 · `internal/coverage/cformat` 9 · `internal/dag` 9 · `internal/xcoff` 9 · `log/slog/internal/benchmarks` 9 · `unicode/utf16` 9 · `crypto/sha1` 8 · `hash/crc64` 8 · `internal/coverage/slicewriter` 8 · `math/cmplx` 8 · `plugin` 8 · `cmp` 7 · `crypto/internal/alias` 7 · `internal/saferio` 7

**G -- G-LAPTOP 6650U** (36 rows):
> `go/internal/gcimporter` 306 · `compress/flate` 106 · `crypto/internal/edwards25519/field` 66 · `os/exec` 43 · `math/rand` 31 · `go/importer` 20 · `debug/buildinfo` 18 · `sort` 16 · `crypto/ed25519` 15 · `net/rpc/jsonrpc` 14 · `expvar` 13 · `flag` 12 · `debug/gosym` 11 · `runtime/debug` 11 · `encoding/csv` 10 · `go/scanner` 10 · `internal/profile` 10 · `mime` 10 · `net/url` 10 · `bufio` 9 · `crypto/rc4` 9 · `debug/plan9obj` 9 · `go/constant` 9 · `internal/coverage/pods` 9 · `internal/diff` 9 · `io` 9 · `math` 9 · `container/list` 8 · `crypto/internal/boring` 8 · `encoding/base64` 8 · `image/color` 8 · `internal/sysinfo` 8 · `os/exec/internal/fdtest` 8 · `text/tabwriter` 8 · `encoding/ascii85` 7 · `math/bits` 7

**X -- fifth engaged machine** (36 rows):
> `crypto/internal/mlkem768` 228 · `crypto/rsa` 119 · `encoding/pem` 105 · `strings` 56 · `crypto/ecdsa` 32 · `text/template` 25 · `go/internal/srcimporter` 19 · `sync` 18 · `crypto` 15 · `image/png` 14 · `syscall` 14 · `crypto/internal/bigmod` 12 · `os/signal` 12 · `go/internal/gccgoimporter` 11 · `debug/macho` 10 · `fmt` 10 · `image/draw` 10 · `internal/zstd` 10 · `net/mail` 10 · `testing/slogtest` 10 · `crypto/aes` 9 · `crypto/sha512` 9 · `encoding/hex` 9 · `hash/crc32` 9 · `internal/cpu` 9 · `internal/testenv` 9 · `log` 9 · `text/scanner` 9 · `crypto/md5` 8 · `go/version` 8 · `internal/buildcfg` 8 · `maps` 8 · `path` 8 · `unicode` 8 · `hash/adler32` 7 · `runtime/internal/sys` 7

Checksum: **162 rows assigned = 7 reserved + 155 bulk** (every roster row named exactly once).
---

## 4. Analysis

### 4.1 What dominates

Six rows over 300 s carry **53.3 %** of the whole roster's wall; the top ten carry **65.1 %**.
Five of those six are already in the plan's reserved set (`crypto/dsa` 1,317, `hash/maphash` 898,
`crypto/tls` 659, `index/suffixarray` 573, `archive/zip` 354); the sixth,
`go/internal/gcimporter` (306 s), is the largest *bulk* row and lands first in every LPT deal.
The reserved pin is quantitatively right: **`crypto/dsa` alone on a 0.35-factor box is 3,763 s
(62.7 min) — nearly the entire W=4 makespan in one row.** No slow worker can carry any of the
top four rows without becoming the campaign's critical path.

Two reserved rows earn their pin for reasons the wall-time column does NOT show, and the map must
not "optimize" them out at recon: `go/doc/comment` reads 18 s here but carries 10,059 verdicts and
spawns `go build` throughout `TestStd` — its cost under `-test-action all` (the `k` regime) will
diverge hardest from this table; and `go/types` read 137 s here against 364 s in the plan's own
reserved table — the row moves between readings, which is itself the argument for pinning it to
the fastest, best-characterized box.

### 4.2 The structural finding: at W ≥ 4 the makespan IS the reserved set

| W | Makespan | Binding bin | Bulk bins finish at |
|:--:|--:|:--|--:|
| 3 | **4,289 s (~71.5 min)** | all three balanced (4,272–4,289 s) | — |
| 4 | **3,956 s (~65.9 min)** | **the i9's reserved set, alone** | ~3,250 s (~54 min) |
| 5 | **3,956 s (~65.9 min)** | **the i9's reserved set, alone** | ~2,497 s (~42 min) |

At W=3 the fleet is capacity-bound: makespan ≈ total / Σs_w = 7,701 / 1.80 = 4,278 s, and LPT
lands within 11 s of that bound (the i9 takes 38 cheap bulk rows on top of its reserves). From
W=4 the picture inverts: the three non-i9 workers absorb the entire 3,745 s bulk and still
finish ~18 % below the i9's floor, LPT gives the i9 **zero** bulk rows, and the campaign's
floor is the reserved set's
3,956 s run serially on the i9. **Adding the fifth worker buys no makespan at all** — it only
drops the bulk bins from ~54 to ~42 min. W=5's value is margin and re-deal resilience (a reboot
costs a smaller shard), not speed.

Two levers if ~66 min needs to shrink, both coordinator decisions, neither assumed here:
(1) the i9's declared capacity is 3 concurrent worktrees — an LPT packing of the 7 reserved rows
into 3 parallel worktrees reaches ~1,369 s (≈ 23 min) on the reserved leg, at the cost of running
reserved rows under concurrent load, which is exactly the condition their `$longTimeouts` floors
exist to survive (crypto/dsa measured 2,444 s under concurrent gates); (2) peel `go/types` +
`go/doc/comment` (the two cheap reserves) to a bulk bin, saving 155 s of floor — barely worth the
exception to "never sharded blind".

### 4.3 Is the ~90-minute shard target achievable?

**Yes at every W — with k = 1 and these factors, no bin splits.** The margins, as k-thresholds
(the multiplier at which a bin first exceeds C = 5,400 s and must split):

| W | Largest local bin | Headroom vs 90 min | Splits when k ≥ |
|:--:|--:|--:|--:|
| 3 | 4,289 s | 1,111 s | **1.26** |
| 4 | 3,956 s (i9 reserved) | 1,444 s | **1.37** |
| 5 | 3,956 s (i9 reserved) | 1,444 s | **1.37** |

So the target survives a convert-and-build multiplier of up to ~1.26 at W=3 and ~1.37 at W≥4
before any bin becomes two sequential shards. If recon measures k materially above that, the
construction's step 5 splits the affected bins mechanically — the map does not need redesigning,
only re-emitting. (Whether k ≈ 1 is genuinely open: the DATA header says the measured wall
already covers convert + build + both test hosts + compare, which argues k ≈ 1, but the plan
prices k as a recon measurement on the first ten rows and this draft defers to that.)

### 4.4 Sensitivity to the suspect speed factors

Perturbing the provisional factors at W=4 (LPT re-run per scenario):

| Scenario | Makespan | Δ vs base |
|:--|--:|--:|
| base (i9 = 1.00, R = 0.45, i7 = 0.35, G = 0.35) | 3,956 s (65.9 min) | — |
| slow laptops (R = 0.35, G = 0.25) | 3,956 s | **+0 %** |
| slow coordinator (i7 = 0.25) | 3,956 s | **+0 %** |
| fast laptops (R = 0.55, G = 0.45) | 3,956 s | **+0 %** |
| everything slow (R = 0.35, i7 = 0.25, G = 0.25) | 4,176 s | +6 % |
| **i9 degraded 20 % (i9 = 0.80)** | 4,945 s (82.4 min) | **+25 %** |

(The table's scenarios re-run LPT at the true factors — i.e. a map *built* knowing them. The
"slow laptops" row lands at +0 % by a hair: the three bulk workers' combined capacity at the
i9's 3,956 s floor is 0.95 × 3,956 = 3,758 i9-s against the 3,745 i9-s bulk load — 99.7 %
utilization.)

The headline: **at W ≥ 4 the makespan is only mildly sensitive to the suspect factors** — the
bulk bins finish ~18 % below the i9's reserved floor, so a mis-calibrated laptop factor
mis-balances bins that were not the critical path anyway. The costlier case is *executing* a
map built on a wrong factor: a worker sized at 0.45 that truly runs at 0.35 finishes its bin
~29 % later than projected (54 → ~70 min — briefly the critical path, lifting the makespan
~6 % over the floor, still well under C). Mis-calibration therefore costs projection accuracy,
idle time and single-digit-percent finish slip — never the 90-min target.
**The one factor that matters is the i9's own throughput** (reboot recovery, concurrent-job load,
thermal state): 20 % degradation moves the makespan 25 %, because the i9 is the floor. At W=3 the
insensitivity does NOT hold — the makespan there is capacity-bound (≈ total / Σs_w), so every
factor error passes straight through; a W=3 dispatch should wait for real calibration numbers,
a W≥4 dispatch tolerates the placeholders.

Corollary for OQ-H6's fallback: if the i9 drops out entirely, the reserved set cannot be
re-dealt at these budgets — `crypto/dsa` + `hash/maphash` + `crypto/tls` + `index/suffixarray`
on a 0.45 box is ~7,600 s local (~2.1 h) before the floors are raised. The re-deal rule in §6
lens 3 (raise `-TestTimeout`, never re-deal at the same budget) is the operative defense; the
shard map itself should name R as the reserved-set fallback since it carries the largest
non-i9 factor.

---

## 5. Ready-to-paste — proposed insertion for `PLAN-hop-campaign.md` §4.3

> Insert after the map-construction code block and its symbol table, before "Illustrative widths
> over 162 rows" (which it supersedes — the illustrative table's even-split arithmetic is now
> replaced by a computed deal; the coordinator may retire or keep that table). Marked DRAFT
> per the header; the full per-row lists land in `docs/phase4/SHARDMAP-go1.23.12.md` at
> dispatch time per the construction's step 6.

```markdown
### 4.3.1 The computed map at the JOB-007 reading — DRAFT, pending recon calibration

> **DRAFT (2026-08-22) — pending coordinator review and hop-recon factor calibration.** The
> construction above, executed against the anchor's per-row wall times
> ([`phase4/DATA-sweep-row-walltimes.md`](phase4/DATA-sweep-row-walltimes.md), windows · corpus
> `18770d083` · i9-13900K, JOB-007: 162 rows, 18,569 verdicts, **7,701 i9-s total**). ⚠ **Every
> `s_w` below is a provisional placeholder** — LANES.md's roster marks historical cross-machine
> ratios SUSPECT, so the factors here are class-level readings (i9 = 1.00 by definition,
> R 6850U = 0.45, i7-5820K = 0.35, G 6650U = 0.35, any fifth worker = 0.35 assumed), to be
> replaced by the calibration pair's fresh numbers at recon and the map re-emitted. `k` is
> assumed 1 pending its recon measurement. The full per-row deal (every row named exactly once,
> checksum 162 = 7 reserved + 155 bulk) is emitted to `SHARDMAP-go1.23.12.md` at dispatch.

**The reserved set is the makespan from W = 4 up.** R's seven rows sum to **3,956 s (65.9 min)**
serial on the i9 — and at W ≥ 4 the remaining 155 rows (3,745 i9-s) fit on the other workers
~18 % under that floor, so LPT hands the i9 zero bulk rows and the campaign finishes when the
reserved set does:

| `W` | Fleet | Makespan (local wall) | Binding bin | Bulk bins finish |
|:--:|:--|--:|:--|--:|
| 3 | i9 + R + coordinator | **~4,289 s (71.5 min)** | all three balanced | — |
| 4 | + G | **~3,956 s (65.9 min)** | i9's reserved set alone | ~54 min |
| 5 | + one engaged machine | **~3,956 s (65.9 min)** | i9's reserved set alone | ~42 min |

Findings the numbers force, stated before dispatch:

1. **C = 90 min holds at every W at k = 1 — no bin splits.** The split thresholds: a bin first
   exceeds C at k ≥ 1.26 (W=3) / k ≥ 1.37 (W≥4). If recon's k lands above that, step 5 splits
   mechanically; the map re-emits, it does not redesign.
2. **At W ≥ 4 the makespan is only mildly sensitive to the suspect factors** (±0.10 on every
   non-i9 factor moves it 0–6 %) because the bulk bins are not the critical path;
   mis-calibration costs projection accuracy and single-digit finish slip, never the 90-min
   target. **At W = 3 it is capacity-bound and every factor
   error passes through** — a W=3 dispatch waits for real calibration; W≥4 tolerates
   placeholders. The one live sensitivity is the i9 itself: 20 % degradation → +25 % makespan.
3. **W = 5 buys margin, not makespan** (bulk bins 54 → 42 min). Engage a fifth machine for
   re-deal resilience against the i9's ~daily reboots, not for speed.
4. **Six rows carry 53 % of the wall**, five of them already reserved; `crypto/dsa` alone on a
   0.35 box would be 62.7 min. The pin is arithmetic, not caution. Two reserves are pinned for
   reasons this table cannot show: `go/doc/comment` (18 s here, but 10,059 verdicts and spawns
   `go build` throughout `TestStd` — the row most exposed to k) and `go/types` (137 s here vs
   364 s at its prior reading — it moves between readings).
5. **Reserved-set fallback (OQ-H6): R**, the largest non-i9 factor — at raised budgets only
   (~2.1 h local for the top four rows; never re-dealt at the i9's budgets, per §6 lens 3).
6. If ~66 min must shrink: the i9's 3-worktree capacity packs the reserved set to ~1,369 s
   (~23 min) in 3 parallel worktrees — at the cost of running exactly the `$longTimeouts` rows
   under concurrent load (crypto/dsa measured 2,444 s under concurrent gates). A coordinator
   trade, not a default.
```

---

## Appendix — reproduction

> **⚠ AMENDMENT (2026-08-24) — this file and `shardmap.py` are BANKED, and the generator has moved
> on.** "This scratchpad directory" is now `docs/phase4/hopA-inputs/` (`e0d8930e1`, banked as-found).
> **`emit_md.py` was never banked** — it existed only in the session that wrote this file, and it is
> gone; `shardmap.py` alone is the reproduction path, and the tables below are its output rather
> than `emit_md.py`'s. `shardmap.py` has since changed in one load-bearing way (`549b4e556`): it
> **derives** its reserved set from `run-validated-sweep.ps1`'s `$longTimeouts` at generation time
> instead of carrying the copied list this draft was computed with — so re-running it today yields a
> **larger** reserved set than the 7 rows below, before any factor calibration. Read the deal here as
> the method's output at the placeholder factors *and* the drifted reserved set of its day.

Computed by `shardmap.py` / `emit_md.py` in this scratchpad directory (Python 3; parses the
fenced JOB-007 table from `docs/phase4/DATA-sweep-row-walltimes.md`, asserts 162 rows and the
7 + 155 checksum, runs the §4.3 construction literally — reserved pin, DESC LPT to smallest
`load / s_w`, deterministic name tie-breaks — then the sensitivity scenarios of §4.4). The LPT
deal is deterministic: two coordinators running it against the same DATA table and factor set
emit byte-identical bins.
