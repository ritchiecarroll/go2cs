# cgo phase 1 fixture

One cgo package and the three Go files `cmd/cgo` generates for it. The converter's cgo work reads
these as its input; nothing here is built or run by `go test`.

The five kinds in `fixture.go`'s preamble are a function, a mixed struct, a `#define` (an integer, a
float and a string), an enum and a typedef'd pointer.

| File | What it is |
|---|---|
| `fixture.go` | The hand-written cgo source, with five kinds of C declaration used from Go. |
| `fixture.cgo1.go` | `cmd/cgo`'s rewrite of `fixture.go`, with one line rewritten (below). |
| `cgo_gotypes.go` | `cmd/cgo`'s `_cgo_gotypes.go`, renamed, under a license header (below). |
| `cgo_import.go` | `cmd/cgo`'s `_cgo_import.go`, renamed, under a license header (below). |

## How the generated files were produced

Generated on the P1 lane box, with go1.26.10 on linux/amd64.

`fixture.go` sat alone in a directory beside a `go.mod` of two lines:

```
module example.com/cgofixture
go 1.26
```

The command, run in that directory:

```
CGO_ENABLED=1 go list -compiled -json -trimpath .
```

Its `CompiledGoFiles` lists three files, in this order: `_cgo_gotypes.go`, `fixture.cgo1.go` and
`_cgo_import.go`. They are committed as `go list` named them, with three changes.

1. **One line rewritten.** `go list -trimpath` leaves the fixture directory's absolute path in line 3
   of `fixture.cgo1.go`, the `//line` directive. That line is rewritten to:

   ```
   //line fixture.go:1:1
   ```

   This is what `cmd/cgo` writes when it is given `-trimpath` itself. Run directly in the same
   directory, `go tool cgo -trimpath "$PWD=>" fixture.go` writes a `fixture.cgo1.go` that is
   byte-for-byte equal to the file committed here.

2. **Two files renamed.** `_cgo_gotypes.go` is committed as `cgo_gotypes.go` and `_cgo_import.go` as
   `cgo_import.go`. The Go tools ignore a file whose name begins with `_`, so the original names would
   hide the files from anything that loads this directory as a package.

3. **A license header on the two renamed files.** This repository requires its license header at the
   top of every `.go` file under `src/go2cs`, and a test enforces it. `fixture.go` carries the header
   as written, and `cmd/cgo` copies it into `fixture.cgo1.go`, so that file needs nothing added.
   `cgo_gotypes.go` and `cgo_import.go` get none from `cmd/cgo`, so each has the header's seven
   comment lines and one blank line put in front. From line 9 to the end, each file is byte-for-byte
   what `cmd/cgo` wrote: `tail -n +9` of the committed file equals `go list`'s file.

No other byte of any generated file was changed.

## Toolchain

```
$ go version
go version go1.26.10 linux/amd64

$ CGO_ENABLED=1 go env GOOS GOARCH CGO_ENABLED CC
linux
amd64
1
gcc

$ gcc --version
gcc (Ubuntu 13.3.0-6ubuntu2~24.04.1) 13.3.0
```

The generated files depend on the toolchain and the C library. `cgo_import.go` names glibc symbol
versions (`free#GLIBC_2.2.5`), and `cgo_gotypes.go` carries the `-O2` and `-g` flags of Go's default
`CGO_LDFLAGS`. A different Go release, target, C compiler or C library can produce different bytes.
