# cgo phase 2 fixture

One extern-only cgo package and the two Go files `cmd/cgo` generates for it. The converter's cgo
route (Phase 2 increment 5; `docs/phase4/DESIGN-cgo-phase2-pinvoke.md` §11.20) is tested on these
trees where no C compiler is. Nothing here is built or run by `go test`.

Every C function `fixture.go` calls is declared by a header and defined by a library: the system C
library's `abs`, `labs`, `div`, `cos`, `exp`, `frexp` and `mktime`, and four functions of the probe
library (`src/tests/GolibTests/cgo-probe/probe.c`), declared in the preamble as its header would
declare them. The probe functions give `cmd/cgo` each form of its pointer-check closure to write:

- the address form (`&x`) and the variable form (a pointer held in a variable);
- a closure nested in another's argument;
- the two-result closure, with a result and with C's `void`;
- a result-less closure as a statement;
- the deferred and goroutine forms;
- `&x` under conversions.

They also give it each of the five places where Go and Phase 2 behave differently (§11.16).

| File | What it is |
|---|---|
| `fixture.go` | The hand-written cgo source. |
| `fixture.cgo1.go` | `cmd/cgo`'s rewrite of `fixture.go`, with one line rewritten (below). |
| `cgo_gotypes.go` | `cmd/cgo`'s `_cgo_gotypes.go`, renamed, under a license header (below). |

## How the generated files were produced

They were made on a GitHub-hosted linux/amd64 runner by `TestTheCgoPhase2FixtureIsCmdCgosOutputForFixtureGo`,
which repeats the steps below and logs each file it makes, in run 42973745743. The files were
committed from that run's `REGEN` lines. `fixture.go` sat alone in a directory beside a `go.mod` of
two lines:

```
module example.com/cgophase2
go 1.26
```

The command, run in that directory:

```
CGO_ENABLED=1 go list -compiled -json -trimpath .
```

The files are committed with the same three changes as Phase 1's (`../phase1/README.md`):

1. `fixture.cgo1.go`'s `//line` directive is rewritten to `//line fixture.go:1:1`;
2. `_cgo_gotypes.go` is committed as `cgo_gotypes.go`;
3. that file gets the license header's seven comment lines and one blank line in front.

No other byte was changed.

**There is no `cgo_import.go`.** `go list -compiled` handed back no `_cgo_import.go` for this
package, where it does for Phase 1's. The likely cause, not measured, is that `cmd/cgo -dynimport`
reads a binary linked from the preamble, and nothing here links the probe library that defines
the four probe functions. The converter drops `_cgo_import.go` in any case. Phase 1's fixture is
what tests that it is dropped.

## Toolchain

```
go version go1.26.10 linux/amd64
go env CC: zig cc, the pinned zig 0.17.0 (clang version 22.1.8), from .github/cgo-toolchain.ps1
```

The pinned zig is the cgo C toolchain default on linux (PLAN-cgo-interop §5.1). As with Phase 1, a
different Go release, target, C compiler or C library can produce different bytes.
