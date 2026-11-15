# Evidence for DESIGN-array-pointer-doors.md section 18 (the field view over a Go struct)

Point-in-time record, lane P5, 2026-11-15. Base `516586c4a9`, Go 1.26.11, one cloud linux/amd64 VM, .NET SDK 10.0,
Release, `GoTargetOS=linux`. Nothing here ran on windows or darwin.

| Path | What it is |
|:--|:--|
| `probe/main.go`, `go.mod` | the scope probe: 15 forms of `(*[N]T)(unsafe.Pointer(&h.f))` over each of four roots and 4 over a fifth; one line per arm, a panic printed and the next arm run |
| `probe/go.out` | Go's 64 lines |
| `probe/prediction.txt` | both predictions as written, before the base was built and before the cut was run |
| `probe/base.cs.txt`, `probe/cut.cs.txt` | the emission of `main.go` by the converter at the base and at the cut (6 lines differ, each a direct index) |
| `probe/base.out` | the base emission built against the base tree |
| `probe/base-emission-cut-package.out` | the base emission built against the cut's unsafe package (part B alone) |
| `probe/cut.out` | the cut's emission built against the cut |
| `probe2/main.go`, `go.mod`, `go.out`, `cut.cs.txt`, `cut.out` | the second probe, 25 lines: the shapes that are not the field view's (a field two selections deep, a field of a struct variable and of a package variable), and the two-selection shape over a struct that holds a reference. The base converter emits the same file byte for byte |
| `census/main.go`, `go.mod`, `go.sum` | the typed census tool (go/packages and go/types): every `(*[N]T)(unsafe.Pointer(&x.f))` in a module's closure, classed by the converter's predicate, by root and by whether the root's struct holds references |
| `census/sqlite.out`, `census/sites.txt` | its run over the sqlite demo's module closure on linux/amd64: the totals, and one row per site |

`.cs.txt` is a converted `.cs` under a name no project picks up. A panic line carries the address the run saw.
