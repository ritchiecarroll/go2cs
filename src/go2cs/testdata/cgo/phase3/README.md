# cgo phase 3 fixture (increment 3.1)

One cgo package and the two Go files `cmd/cgo` generates for it. The converter's cgo route
(`docs/phase4/DESIGN-cgo-phase3-marshaling.md` §16) is tested on these trees where no C compiler
is. Nothing here is built or run by `go test`.

`fixture.go` uses what increment 3.1 admits:

- `cmd/cgo`'s own helpers: `C.CString`, `C.CBytes`, `C.GoString`, `C.GoStringN` and `C.GoBytes`;
- `C.malloc` and `C.free`;
- a `void *` as a parameter and as a result;
- the deferred call through `cmd/cgo`'s pointer-check closure, and a deferred C call that needs
  no check.

It hands C no Go memory. Its probe functions are the probe library's
(`src/tests/GolibTests/cgo-probe/probe.c`), declared in the preamble as its header would declare
them.

| File | What it is |
|---|---|
| `fixture.go` | The hand-written cgo source. |
| `fixture.cgo1.go` | `cmd/cgo`'s rewrite of `fixture.go`, with one line rewritten (below). |
| `cgo_gotypes.go` | `cmd/cgo`'s `_cgo_gotypes.go`, renamed, under a license header (below). |

## How the generated files were produced

They were made on a GitHub-hosted linux/amd64 runner by
`TestTheCgoPhase3FixtureIsCmdCgosOutputForFixtureGo`, which repeats the steps of
`../phase2/README.md` and logs each file it makes, in run 43325892733. The files were committed
from that run's `REGEN` lines, with the same three changes as Phase 2's:

1. `fixture.cgo1.go`'s `//line` directive is rewritten to `//line fixture.go:1:1`;
2. `_cgo_gotypes.go` is committed as `cgo_gotypes.go`;
3. that file gets the license header's seven comment lines and one blank line in front.

No other byte was changed. There is no `cgo_import.go`, for Phase 2's reason: nothing links the
probe library when `go list` compiles the package.

## Toolchain

```
go version go1.26.10 linux/amd64
go env CC: gcc, the runner's own (gcc (Ubuntu 15.2.0-16ubuntu1) 15.2.0)
```

The same test compares these files byte for byte on every linux/amd64 leg, under the runner's gcc
and under the pinned zig. A different Go release, target, C compiler or C library can produce
different bytes.
