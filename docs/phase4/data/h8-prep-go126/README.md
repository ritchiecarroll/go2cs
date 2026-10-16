# H8 prep: which roster suites import what today's H7 classes touched (go1.26.9)

Data record, cut 2026-10-16 by C1 at COORD's request. It is the first H8 reading. **It rules nothing.** The four
vanished roster rows below go to H8's ruling list, and COORD rules them when H8 opens.

## What was measured

H7 touched four packages: `net`, `archive/zip`, `errors`, and `crypto/subtle`, reached through GCM. For every one
of the 225 rows of [`docs/ValidatedTestPackages.md`](../../../ValidatedTestPackages.md) at `8094c24b5a`,
on `linux`, `darwin` and `windows` (amd64, `CGO_ENABLED=0`, go1.26.9), these TSVs record:

| column | value | meaning |
|:--|:--|:--|
| `net`, `archive/zip`, `errors`, `crypto/subtle` | `D` | the row's `Imports`, `TestImports` or `XTestImports` name it |
| | `P` | reached only through the row's **own** closure, `go list -deps <pkg>` without `-test` |
| | `-` | neither |
| `GCM(own closure)` | `gcm` | `crypto/internal/fips140/aes/gcm` is in the own closure |
| `cipher/aes direct (test)` | | the suite's test files import `crypto/cipher` or `crypto/aes` directly |
| `NewGCM in tests` | n | `NewGCM*(` call sites in the row's `*_test.go` files |

## The method finding

**The TEST closure (`go list -test -deps`) cannot select.** It reaches `crypto/subtle` and the GCM package in
219 of 219 listable rows, through the test harness itself, and `errors` in all of them. Only the package's own
closure, or a direct import, says anything about a row. Every `P` above is the own closure, never the test closure.

## Linux, in brief (darwin and windows differ only where the post to COORD lists)

- `net`: 30 `D`, 2 `P`.
- `archive/zip`: 2 `D` (archive/zip, html/template).
- `crypto/subtle`: 8 `D`, 26 `P`.
- GCM in the own closure: 33 rows. It is EXERCISED by `crypto/cipher` (27 `NewGCM*` sites) and `crypto/rsa` (1).
- `errors`: 128 `D`, 52 `P`. That is too wide to select anything.

## Roster rows that no longer exist at go1.26.9 (for H8's ruling list)

| row | at go1.26.9 |
|:--|:--|
| `crypto/internal/hpke` | public as `crypto/hpke` |
| `internal/itoa` | gone; `internal/strconv` exists |
| `go/ast/internal/tests` | gone (`go/ast` has no `internal/`) |
| `internal/trace/internal/oldtrace` | gone; `internal/trace/internal` holds `testgen`, `tracev1` |

`internal/syscall/windows` and `internal/syscall/windows/registry` are `NOT-LISTABLE` on linux and darwin
because of their build constraints. That is expected, and they list on windows.
