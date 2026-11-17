# Evidence for DESIGN-array-pointer-doors.md section 19 (the field view over a struct that holds a reference)

Point-in-time record, lane P5, 2026-11-15. Base: part C (`ecbf346695`); `probe-open/` and `footprint/` were made after the
seat was merged with batch 308 (`b2214e00e4`). Go 1.26.11, one cloud linux/amd64 VM,
.NET SDK 10.0, Release, `GoTargetOS=linux`. Nothing here ran on windows or darwin.

| Path | What it is |
|:--|:--|
| `prediction.txt` | the seat's four predictions as written, and the correction to the second, written before any footprint ran |
| `prediction4.out`, `prediction4.err.txt` | the first commit's behavioral program, as part C's converter emits it, built against part C's tree: no line printed, refused by name at its first view |
| `probe/main.go`, `go.mod` | the shape probe: a field view through one selection, two selections, a struct variable, a package variable, a promoted field and a slice element's field, over a struct with no reference and over one that holds a reference; one line per arm, a panic printed and the next arm run |
| `probe/go.out` | Go's 36 lines |
| `probe/base.cs.txt`, `probe/cut.cs.txt`, `probe/emit.diff` | the emission of `main.go` by part C's converter and by the widened one, and their diff (49 lines differ) |
| `probe/base.out`, `probe/cut.out` | each emission built against the first commit's tree: 12 and 32 lines equal to Go's |
| `probe-arr/main.go`, `go.mod`, `go.out` | the array-field probe: `(*[8]byte)(unsafe.Pointer(&d.name))`, name a `[8]int8`, sliced and kept in a local, over a pointer root, a struct variable, a package variable and a two-selection chain. Go's 7 lines. With an argument it runs one arm alone |
| `probe-arr/base.cs.txt`, `probe-arr/cut.cs.txt` | its emission by part C's converter and by the widened one |
| `probe-arr/base.arms.out` | part C's emission built against part C's tree, one arm per process: exit code, the line printed, and the first line of a fatal error |
| `probe-arr/base.arm2.err.txt` | the fatal error of arm 2 (a struct variable, `[:]`), whole |
| `probe-arr/cut.arms.out` | the widened emission built against the seat's tree, one arm per process: Go's 7 lines |
| `probe-unpinnable/` | the element door over a field nothing holds still, as a unit arm run once and not kept in the tree: over 200 rounds with a forced compacting collection the struct moved in 200 and the kept element pointer read stale in 200 (index 1) and 196 (index 0) |
| `probe-open/main.go`, `go.mod`, `go.out` | the open-slice probe, made after the seat was merged with batch 308 (`b2214e00e4`): `(*[8]byte)(unsafe.Pointer(&x.name))[:]`, name a `[8]int8`, over a promoted field, a field of a slice element and a field of a call's result (arms 0 to 2), and over the three forms that reach the same field from a variable or a pointer (arms 3 to 5). Each arm runs under a `recover`. Go's 6 lines. With an argument it runs one arm alone |
| `probe-open/prediction.txt` | its two predictions, written before the probe was converted |
| `probe-open/cut.cs.txt` | its emission by the seat's converter |
| `probe-open/arms.out` | the emission built against the seat's merged tree, one arm per process: exit code, the line printed, and the first line of a fatal error. Arms 0 to 2 end the process (exit code 134, no line printed); arms 3 to 5 print Go's lines |
| `probe-open/arm0.err.txt` | the fatal error of arm 0 (a promoted field), whole |
| `footprint/summary.txt` | the two-seeded footprint of the seat's converter change, run after the merge with batch 308: the base arm is merged part C's converter, the cut arm the seat's, each converting the standard library for one target into a root seeded from the merged tree; per target, the files that differ between the two roots and whether each arm equals the committed file |
| `footprint/two-seeded-<target>.files.txt` | `diff -rq` of the base root against the cut root, per target. `base/` and `cut/` stand for the two roots. `src/reports/conversion_report.html` and `src/stdlib_conversion_progress.txt` are each run's own report, with its times, and differ on every target |
| `footprint/two-seeded-linux-amd64.diff.txt`, `two-seeded-linux-arm64.diff.txt` | the unified diff of the one corpus file that differs, CR-stripped: 3 lines of `syscall/linux/lsf_linux.cs` |
| `census-std/main.go`, `go.mod`, `go.sum` | the census tool of `field-view-1115/census2/` with one change: `CENSUS_STD=1` reads the packages that have no module, the standard library, and only those. Its header line still says "packages with a module" |
| `census-std/std-<target>.out`, `std-sites-<target>.txt` | its run over `std` on four targets: totals by class, and one row per site. `hook=true` is the tool's predicate, which also counts a view to a NAMED array type; the converter's does not |

`.cs.txt` is a converted `.cs` under a name no project picks up. In the two `.err.txt` files the runtime's assembly-qualified type
arguments are shortened to the type's name (`[[System.Byte]]`); nothing else is changed. A panic line carries the address the run saw.
