# CENSUS — the roster's Linux annotations at the go1.24.13 hop (a sizing, read-only)

**Read-only record** (C1, 2026-09-22). Rows and Windows cells as batch 8b reads them (`51a1d30ff9`); pages at the batch-7 stamp `3469154a95`; the 1.23.12 baseline is master `20eb0af70e` (header 203 / 215). Rules nothing — the Linux leg is COORD's to cut. Point-in-time.

## Why every annotation is owed a Linux run

The header's Linux line — **"188 of 217 applicable rows validated at their Linux counts — 20,878 matching verdicts · 168 disclosed"** — is derived by the format guard from the rows' `· linux: N + D` annotations, and every one of those annotations is **1.23.12-era evidence** (the three this hop's re-sign had to read were written 2026-09-08). It sits beside a 1.24.13 Windows header without a date of its own, so as printed it claims a 1.24.13 Linux state nothing has measured. A Linux bank lives **only** in the annotation (validation-bank: the committed tests, pages and badges are the Windows record; a Linux sweep's README badge rewrite is RESTORED, not banked).

**190 annotated rows** = 35 whose Windows cells MOVED across the hop + 140 whose 1.24.13 page left them unchanged + 13 not yet measured at 1.24.13 even on Windows + 2 `linux: n/a`.

## 1. Annotations that WILL move — the Windows test set moved across the hop

The same Go test sources run on both axes, so a row whose Windows verdict count changed between 1.23.12 and 1.24.13 has a changed Linux test set too. The prediction is the DIRECTION (the annotation moves), not the Linux number: Linux counts differ from Windows by GOOS-gated tests and are measured, never derived.

| Row | Windows 1.23.12 | Windows 1.24.13 | Linux annotation (1.23.12) |
|:--|--:|--:|--:|
| `archive/tar` | 97/0 | 98/0 | 97 |
| `bytes` | 82/6 | 83/6 | 86 + 6 |
| `crypto/aes` | 13/0 | 57/0 | 13 |
| `crypto/cipher` | 13/1 | 27272/0 | 13 + 1 |
| `crypto/des` | 18/0 | 55/0 | 18 |
| `crypto/ecdsa` | 82/0 | 77/0 | 82 |
| `crypto/ed25519` | 8/1 | 9/1 | 8 + 1 |
| `crypto/rand` | 298/0 | 314/1 | 302 |
| `crypto/rc4` | 2/0 | 75/0 | 2 |
| `crypto/rsa` | 559/1 | 568/1 | 559 + 1 |
| `crypto/sha256` | 23/1 | 22/1 | 23 + 1 |
| `crypto/sha512` | 36/1 | 35/1 | 36 + 1 |
| `crypto/subtle` | 7/0 | 9/0 | 7 |
| `crypto/x509` | 341/0 | 518/0 | 322 |
| `database/sql` | 138/2 | 140/2 | 138 + 2 |
| `debug/buildinfo` | 197/0 | 211/0 | 197 |
| `encoding/asn1` | 38/0 | 40/0 | 38 |
| `encoding/binary` | 137/9 | 140/6 | 137 + 9 |
| `encoding/json` | 491/0 | 532/0 | 491 |
| `encoding/pem` | 8/0 | 18/0 | 8 |
| `encoding/xml` | 386/0 | 387/0 | 386 |
| `go/doc` | 85/0 | 86/0 | 85 |
| `go/internal/gcimporter` | 583/0 | 621/0 | 581 |
| `go/parser` | 173/0 | 176/0 | 173 |
| `go/types` | 557/0 | 574/0 | 557 |
| `hash/maphash` | 22/0 | 59/0 | 22 |
| `internal/buildcfg` | 3/0 | 4/0 | 3 |
| `log/slog` | 194/19 | 197/19 | 194 + 19 |
| `math/big` | 224/2 | 230/1 | 224 + 2 |
| `net/netip` | 210/57 | 211/57 | 210 + 57 |
| `net/smtp` | 19/0 | 20/0 | 19 |
| `net/url` | 48/0 | 49/0 | 48 |
| `slices` | 119/3 | 120/3 | 119 + 3 |
| `strings` | 68/4 | 69/4 | 68 + 4 |
| `sync` | 47/4 | 46/6 | 47 + 4 |

(35 rows; `internal/syscall/windows` is also a Windows mover but is `linux: n/a` and owes nothing.)

## 2. Pin-driven Linux moves (the orphan census, the re-sign and the two new pins)

- **`math/big`** `224 + 2` → **+1 predicted, measured**: Go's own `TestNewIntAllocs` FAILS on linux/amd64 at 1.24.13 under `-tags math_big_pure_go` (and passes without it), so the pin absorbs nothing on Linux either. Once the Linux run reads it, the pin retires fully (it is `[linux, darwin]` since the re-sign; darwin has no run layer and no claim).
- **`encoding/binary`** `137 + 9` and **`crypto/cipher`** `13 + 1`: the three `TestSizeAllocs` pins and `TestGCMAsm` are TERMINAL PASSES on the Windows record at 1.24.13 and were scoped off Windows only because these Linux claims still name them. Linux builds the same purego file set, so the Linux run is expected to read them passing too (binary `+9` → `+6`, cipher `+1` → `+0`) — **unmeasured**; if it does, the in-run orphan report names them and they retire on every platform.
- **`unicode/utf8`** `14` → expected `N + 1`: the new-in-1.24 `TestRuneCountNonASCIIAllocation` diverges by mechanism (golib allocation sites), not by OS, and its new deferred pin covers it.
- **`fmt`** `63` → `N + 1` once the row re-banks (its Windows row is still DIVERGED on `TestSprintf`, which is not pinned).
- **`syscall`** `38 + 17`, **`os/signal`**, **`internal/poll`**, **`os/exec`**, **`runtime/debug`**: the re-sign scoped their linux-only / unix-only pins to exactly the platforms whose tests they name; their Linux D is predicted UNCHANGED by the re-sign (the counts move only with the test set, §1/§3).

## 3. Annotations predicted UNCHANGED — but still 1.23.12 evidence

140 rows have a 1.24.13 Windows page whose cells did not move. Their Linux annotations most likely hold; a Linux run confirms them rather than moves them, and it is what lets the header's Linux line stand as a 1.24.13 figure.

<details><summary>The rows</summary>

`archive/zip`, `bufio`, `cmp`, `compress/bzip2`, `compress/flate`, `compress/gzip`, `compress/lzw`, `compress/zlib`, `container/heap`, `container/list`, `container/ring`, `context`, `crypto`, `crypto/dsa`, `crypto/ecdh`, `crypto/elliptic`, `crypto/hmac`, `crypto/internal/boring`, `crypto/internal/hpke`, `crypto/md5`, `crypto/sha1`, `database/sql/driver`, `debug/dwarf`, `debug/elf`, `debug/gosym`, `debug/macho`, `debug/pe`, `debug/plan9obj`, `encoding/ascii85`, `encoding/base32`, `encoding/base64`, `encoding/csv`, `encoding/gob`, `encoding/hex`, `errors`, `expvar`, `flag`, `go/ast`, `go/build`, `go/build/constraint`, `go/constant`, `go/doc/comment`, `go/format`, `go/importer`, `go/internal/gccgoimporter`, `go/internal/srcimporter`, `go/printer`, `go/scanner`, `go/token`, `go/version`, `hash`, `hash/adler32`, `hash/crc32`, `hash/crc64`, `hash/fnv`, `html`, `html/template`, `image`, `image/color`, `image/draw`, `image/gif`, `image/jpeg`, `image/png`, `index/suffixarray`, `internal/abi`, `internal/chacha8rand`, `internal/coverage/cformat`, `internal/coverage/cmerge`, `internal/coverage/pods`, `internal/coverage/slicereader`, `internal/coverage/slicewriter`, `internal/cpu`, `internal/dag`, `internal/diff`, `internal/fmtsort`, `internal/fuzz`, `internal/godebugs`, `internal/gover`, `internal/itoa`, `internal/platform`, `internal/poll`, `internal/profile`, `internal/reflectlite`, `internal/saferio`, `internal/singleflight`, `internal/sysinfo`, `internal/testenv`, `internal/trace/internal/oldtrace`, `internal/types/errors`, `internal/xcoff`, `internal/zstd`, `io`, `io/fs`, `io/ioutil`, `iter`, `log`, `log/slog/internal/benchmarks`, `log/slog/internal/buffer`, `maps`, `math`, `math/bits`, `math/cmplx`, `math/rand/v2`, `mime`, `mime/quotedprintable`, `net/http/cgi`, `net/http/cookiejar`, `net/http/fcgi`, `net/http/httptest`, `net/http/httptrace`, `net/http/httputil`, `net/http/internal`, `net/http/internal/ascii`, `net/mail`, `net/rpc`, `net/rpc/jsonrpc`, `net/textproto`, `os/exec`, `os/exec/internal/fdtest`, `os/signal`, `path`, `path/filepath`, `plugin`, `regexp`, `regexp/syntax`, `runtime/debug`, `runtime/metrics`, `sort`, `strconv`, `sync/atomic`, `testing/fstest`, `testing/iotest`, `testing/quick`, `testing/slogtest`, `text/scanner`, `text/tabwriter`, `text/template`, `text/template/parse`, `unicode`, `unicode/utf16`

</details>

## 4. Rows not yet measured at 1.24.13 on Windows

| Row | Windows (1.23.12 anchor) | Linux annotation | State |
|:--|--:|--:|:--|
| `crypto/tls` | 3643/1 | 401 + 1 | Windows re-bank first |
| `fmt` | 63/0 | 63 | Windows re-bank first |
| `internal/coverage/cfile` | 15/1 | 15 + 1 | banked by G's pass 3 in batch 8b — joins §1/§3 once its page lands |
| `internal/godebug` | 5/0 | 5 | Windows re-bank first |
| `internal/runtime/atomic` | 15/0 | 15 | Windows re-bank first |
| `internal/trace` | 92/0 | 95 | Windows re-bank first |
| `math/rand` | 43/0 | 43 | banked by G's pass 3 in batch 8b — joins §1/§3 once its page lands |
| `mime/multipart` | 52/0 | 52 | banked by G's pass 3 in batch 8b — joins §1/§3 once its page lands |
| `net` | 472/2 | 577 + 2 | Windows re-bank first |
| `os/user` | 5/0 | 12 | Windows re-bank first |
| `syscall` | 65/0 | 38 + 17 | Windows re-bank first |
| `time` | 169/0 | 167 | Windows re-bank first |
| `unicode/utf8` | 14/0 | 14 | Windows re-bank first |

A Linux refresh of these waits on the Windows re-bank: the Windows record is the row's evidence of record, and a Linux number banked beside a stale Windows anchor would describe two different Go versions in one row.

## 5. What the Linux leg runs

- **Host:** G-LAPTOP's WSL arm (COORD's routing). Cross the boundary with a heredoc (`wsl -- bash -s <<'EOF'`), never `bash -lc '…'` (harness-gates).
- **Target:** `GoTargetOS=linux`. On a Linux host `_paths.ps1` pins it for the sweep; a hand-invoked `-tests` run needs it IN THE ENVIRONMENT or it links the Windows dependency set and mints phantom CS0426s (harness-gates). The build that proves the L3 graph is `--no-incremental -p:GoTargetOS=linux`.
- **Oracle state:** the sweep pins `CGO_ENABLED=0` for the whole run — the annotations' state of record on every platform (a cgo-ON oracle reads Linux counts HIGH by exactly its cgo-gated tests).
- **What:** `run-validated-sweep.ps1` on the Linux host over §1 + §2 + §3 (and §4's three once 8b lands), sharded with `-ShardCount/-ShardIndex` like the Windows passes. Each row is checked against its own `linux:` annotation (`Get-RosterRowExpectation -Goos linux`); a row whose count moved reports a mismatch against the annotation, which is the reading the refresh banks.
- **Banking:** the lane edits ONLY the `· linux: N + D` annotations (and the batch's guard re-derives the header's Linux line and the README's); the Linux sweep's README badge rewrite is RESTORED. A disclosure REMOVAL the Linux run motivates is a cross-platform edit (doctrine rule 1) and goes through a platform scope, never a delete.
- **Order:** after batch 8b (it carries the re-sign's scopes, the two new pins and G's three banks), so the Linux run reads the manifests it will be judged against.

## 6. Limits

- §1's list is a lower bound on MOVERS: a row whose Windows count held can still move on Linux through a GOOS-gated test the hop added or removed; only the run says.
- Nothing here predicts a Linux NUMBER. Linux and Windows counts differ by construction (e.g. `crypto/rand` 302 vs 298 at 1.23.12, `net` 577 vs 472), so every new annotation is a measurement.

## 7. Aside, as COORD asked — the two synctest counts, from the evidence refs

- **`net/http`: partly resolved.** The i9 recon record (`docs/phase4/hopA-inputs/recon-evidence/i9/net/http/go2cs_test_comparison.json`, carried on `1e7709ac7a`) holds **exactly the 11 named leaves** as `infrastructure-error` (`NewClientServerTest/synctest/{h1,h2,https1}`, `ServerShutdownStateNew/{h1,h2}`, `TransportIdleConnRacesRequest/{h1,h2unencrypted}`, `TransportRemovesConnsAfterBroken/{h1,h2}`, `TransportRemovesConnsAfterIdle/{h1,h2}`), plus the **6** parents failing by aggregation (G's "6 parents"). G's **12** is from pass 3 at `8fc439415f`, whose TSV carries no per-test rows, so whether pass 3 produced a twelfth infrastructure-error row or the count slipped is NOT resolvable from the refs; the battery that next runs `net/http` settles it. One more reading from the same record: `TransportIdleConnRacesRequest/h2unencrypted` is **Go skip / C# infrastructure-error**, so once the bubble exists that leaf becomes a skip/skip match, not a pass — the synctest design's prediction holds with that caveat.
- **`internal/synctest`: not resolvable.** No evidence ref carries its comparison record; the two readings (29 = 26 + 3, DIVERGED 28 = 26 + 2) exist only in the ledger lines.
