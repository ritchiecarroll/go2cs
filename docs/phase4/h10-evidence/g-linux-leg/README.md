# G's Linux leg at go1.24.13 — evidence (a record)

Point-in-time record (2026-09-23). The bank it supports is `claude/g-linux-leg` (annotations only).

**Host and pins.** G-LAPTOP's WSL arm; `GoTargetOS=linux`, `CGO_ENABLED=0`, go1.24.13 (PATH, `go env`
and `VERSION` agree; the ambient 1.23.12 go is the dissenting control), .NET 10, Release with tiering
off. `run-validated-sweep.ps1` blob `da3913368c` and `run-h10-recon.ps1` blob `f17cc5b437` (the
`d095fe8108` instruments; the 8c tree carries the same two blobs).

**Runs.**
- The full sweep, six shards, over the 217 applicable rows at `bb54ff0920`. Shard walls are 1,451 /
  1,646 / 1,601 / 1,571 / 1,327 / 7,042 s. Shard 6 carries sync/atomic's 5,340 s deadline.
- The four rows whose Windows page moved in batch 8c, re-read on the 8c tree `1719e3b87f`:
  internal/godebug, internal/trace, os/user and testing.
- runtime, through the H10 wrapper at `1719e3b87f`, as classification input.
- Every FAIL row re-read once at `bb54ff0920` with its results file kept, for attribution. Every
  one reproduced.
- The two static-init crashes (crypto/x509, hash/maphash) were read on their published hosts.

**Net qualification.** Go's own `go test -count=1 -timeout 40m net` at go1.24.13, cgo off: rc 1 in
36 s. 577 pass, 25 skip, and 1 fail: `TestLookupCNAME`, the tolerated drift. The WSL DNS probe read
the 09-02 pin intact, verdict DNS-CONFORMING.

**`rows.tsv`.** One line per roster row.
- `word` is the sweep's classification (PASS / COUNT / DISC / FAIL / ORACLE / N/A).
- `linux_new` is the reading: the matched count, plus the converter's disclosed count when nonzero.
- `banked` = yes only where the row moved AND its Windows page is at 1.24.13.
- `note` names the attributed cause of every non-reading.

**The dominant cause.** 25 of the 30 FAIL rows are the one missing body:
`internal/syscall/unix.vgetrandom`. It is a `//go:linkname vgetrandom runtime.vgetrandom` partial in
`src/core/internal/syscall/unix/linux/getrandom.cs`. It throws
`NotImplementedException: vgetrandom: no implementation reached this compilation` because the
linux runtime's `vgetrandom` (runtime/linux/vgetrandom_linux.cs) never pushes into it. This is new
at Go 1.24, and Windows never reaches it.
- 23 rows carry the message, in their results file or on the published host.
- 2 rows, net/http and net/http/httptest, die on a goroutine inside `crypto/tls`'s handshake. They
  are attributed by the stack alone; the message was not captured.

**Item 4 of the relabel seat.** These readings are from `bb54ff0920`.
- encoding/binary `TestSizeAllocs/{complex64, complex128, binary.Struct}` read pass/pass.
- math/big `TestNewIntAllocs` reads fail/fail, a matched both-fail.
- bytes `TestGrow` reads pass/fail and is disclosed (alloc-profile).

## Amended 2026-09-23 — the three unprivileged readings after the leg

Same WSL arm and pins, run as the go1.24.13 install's unprivileged owner. The 1.23.12 Linux
annotations were ROOT readings, so a test that skips itself under root can move. syscall's
TestUnshareUidGidMapping is the measured case. `records/<row>.verdicts.json` is each row's comparison
record reduced to its verdict fields: package, status, go, csharp, matched, skipped, disclosed,
excluded and environment. `errors` and `stderr` are dropped because they carry host paths and the
host's own run output. The go and csharp maps reproduce the counts below.

| Row | Tree | Reading | Wall |
|:--|:--|:--|--:|
| `syscall` | `5b9ebc5e50` (the descriptor-limit restore + two disclosures, on `971d919113`) | VALIDATED 45 + 11, 56 of 56 C# rows (banked `ce065f8aa9`) | 132 s |
| `sync/atomic` | `b293973e9f` (the vgetrandom seat), `-TestTimeout 150m` | VALIDATED 108, confirming `linux: 108`; the 90m floor was the leg's FAIL (floor seat `48e1e4d245`) | 5,868 s |
| `net` | `fc6269b0bf` (batch 8e) | FAILING on one undisclosed verdict: 581 matched + 2 disclosed of 584; `TestIPAppendTextNoAllocs` pass / fail at 3 allocations per run (pinned in batch 8f) | 357 s |

net's host qualification at that tip: Go's own `go test -count=1 -timeout 40m net` read rc 1 in 41 s,
with 578 pass, 24 skip and 1 fail, `TestLookupCNAME` (the tolerated drift). In the leg's reading one
test that now passes was a skip.

## Amended 2026-09-23 — net/http, the host rule's read before the close

The runbook's host rule reads net/http at the close tip before it is classified or demoted. This is
evidence only: net/http carries no Linux annotation and banks nothing.

- **Tree and configuration.** `faaa8fe999` (batch 8g), the tree's own sweep (blob `710655e61f`),
  Release, with the row's `release-tiered` pin applied (the record reads `tiered: true`). WSL arm,
  unprivileged.
- **Reading.** FAILING: 1,370 matched of 1,387, 0 disclosed, 17 undisclosed divergences. Wall 309 s.
- **The divergences.** They are exactly the synctest class: the 11 leaves (10 pass / infrastructure-error,
  plus `TestTransportIdleConnRacesRequest/h2unencrypted` as skip / infrastructure-error) and their 6
  parents, all pass / fail.
  - `TestNewClientServerTest/synctest/{h1,h2,https1}`
  - `TestServerShutdownStateNew/{h1,h2}`
  - `TestTransportIdleConnRacesRequest/{h1,h2unencrypted}`
  - `TestTransportRemovesConnsAfterBroken/{h1,h2}`
  - `TestTransportRemovesConnsAfterIdle/{h1,h2}`
- **TestRegisterErr.** It and every one of its subtests read pass / pass on Linux.
- **Record.** `records/net.http.verdicts.json`, reduced as above, with `errors`, `stderr` and `gated`
  dropped. It was reduced by Python rather than PowerShell: the record carries keys that differ only in
  case (`…/h1/GZIP` and `…/h1/gzip`), which ConvertFrom-Json refuses.
