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
