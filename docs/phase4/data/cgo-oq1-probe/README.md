# cgo OQ-1 probe (scratch fixture, 2026-11-08)

Answers `docs/PLAN-cgo-interop.md` OQ-1 on a cgo-capable linux host: go1.26.10, gcc 13.3.0, golang.org/x/tools
v0.52.0 (go2cs's pin), master `30ad20e17e`.

- `fixture/`: one cgo file whose preamble declares a function, a mixed struct, `#define` integer, float and string
  constants, an enum and a typedef'd pointer. The Go side uses each of them, plus `C.CString`, `C.free` and
  `C.GoString`.
- `probe/`: loads the fixture with `go/packages` and prints the files, the package scope and what each `_C*`
  identifier the syntax uses resolves to. It is run as `cgoprobe <dir> <CGO_ENABLED> <coord|go2cs>`: `coord`
  is NeedName|NeedFiles|NeedSyntax|NeedTypes|NeedTypesInfo|NeedCompiledGoFiles, and `go2cs` is
  packages.LoadAllSyntax.
- `out-<mode>-<CGO_ENABLED>.txt`: the four readings, with the scratch path replaced by `<scratch>`.

The positions printed for the `uses` lines come from iterating a map, so which use of a name is shown varies
between runs. The go2cs-1 and coord-1 readings differ only in those lines.
