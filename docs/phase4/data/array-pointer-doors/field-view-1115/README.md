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
| `probe3/main.go`, `go.mod` | part C's scope probe (section 18.7): 32 functions, every sliced, dereferenced, loaded, stored and ranged form of a view local and of the conversion where it is written, with five controls. `storeMulti`, the tuple store, is called last so that every other line is reached |
| `probe3/go.out` | Go's 11 lines |
| `probe3/prediction.txt` | part C's seven predictions as written, before the probe ran and before any footprint was measured |
| `probe3/ab.cs.txt`, `probe3/cut.cs.txt` | the emission of `main.go` by the converter at parts A and B (`9dba6dbd8f`) and at part C (22 lines differ) |
| `probe3/cut.out`, `probe3/cut.err` | part C's emission built against part C's tree: 10 lines equal to Go's, then the tuple store refused by name |
| `census2/main.go`, `go.mod`, `go.sum` | the census tool of `census/`, taking its target from `CENSUS_GOOS` and `CENSUS_GOARCH` and printing what the field is and whether the view fits inside it (section 18.8) |
| `census2/nugetgo9.go.mod` | the scratch module that requires the nine nugetgo modules; the census ran from it over each module's `/...` |
| `census2/nugetgo9-<target>.out`, `nugetgo9-sites-<target>.txt` | the nine modules' closure on four targets: totals by class, and one row per site |
| `census2/ts-<target>.out`, `ts-sites-<target>.txt` | the TypeScript closure (the tsc module at `ba29a56299`, `./...`) on four targets: totals by class, and the site rows, every one in `golang.org/x/sys`. The tree is private and none of it is here |

`.cs.txt` is a converted `.cs` under a name no project picks up. A panic line carries the address the run saw.
