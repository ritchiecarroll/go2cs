# Evidence for DESIGN-array-pointer-doors.md

Point-in-time record, lane P5, 2026-11-14. Measured at `claude/p5-interior-pointer-cut` `837a413fdc`. Go 1.26.11,
one cloud linux/amd64 VM (4 vCPU, 15 GB), .NET SDK 10.0, Release, `GoTargetOS=linux`. Nothing here ran on windows
or darwin; the census tables cover them by type-checking under `GOOS`/`GOARCH`.

## Files

| Path | What it is |
|:--|:--|
| `NativeViewShapes/main.go`, `go.mod` | the 24-arm program; one arm per process, named by its first argument |
| `NativeViewShapes/go.out` | Go's answer per arm (`go build`, then the binary once per arm) |
| `NativeViewShapes/converted-cut.cs.txt` | the cut's emission of `main.go` (the `main.cs` the converter wrote) |
| `NativeViewShapes/converted-cut.out` | that emission built and run against the cut, one arm per process, with each exit code |
| `NativeViewShapes/prototype.cs.txt`, `prototype.out` | the emission with one hand edit (each span emission becomes `@unsafe.Slice(@unsafe.Pointer.To<byte>(X), LEN)`), built against the same tree with no golib change; 11 arms |
| `predictions.txt` | the predictions as written before each build, and each scoring, in order |
| `avcensus/` | the census tool (go/packages and go/types; golang.org/x/tools v0.52.0) |
| `census/*.tsv`, `*.summary.txt` | one row per site per target, and the tool's stderr per run |
| `census/tables.txt` | the tables the design quotes, generated from the `.tsv` files |
| `census/run.sh` | the seven census runs |

`.cs.txt` is a converted `.cs` under a name no project picks up. In the `.out` files `<tree>` stands for the cut's
worktree and `<kit>` for the scratch directory the program was converted in.

## Row format (`census/*.tsv`)

Tab separated, no header: tag, package, file:line, array type name (`-` for an unnamed array), array length,
`fits` or `past-span` (length over `Array.MaxLength`, 0x7FFFFFC7), element type, element holds pointers, form,
context, operand, in a test file.

- **form**: `slice3`, `slice2` (a high bound, two indexes), `slice-open` (no high bound), `index`, `deref` (the
  array value read or stored whole), `pointer` (the conversion's value itself is used).
- **context**: `copy-dst`, `copy-src` (a direct operand of the builtin), `store`, `read`, `addr`, `bound` (assigned
  to a variable), `arg`, `return`, `range`.
- **operand**: `number` (`unsafe.Pointer(u)`, `u` a `uintptr`), `number-arith`, `addr-of-var`, `addr-of-element`,
  `addr-of-field`, `typed-pointer`, `pointer-var`, `pointer-field`, `unsafe.Add`, `call:<name>`.

## Re-running

```
cd NativeViewShapes && go build -o nvs . && for a in A B C D E F G H I J K L M C0 D0 F0 M0 N O P Q R S T; do ./nvs $a; done
go2cs -go2cspath <tree>/src <kit>/NativeViewShapes <kit>/NativeViewShapes
dotnet build NativeViewShapes.csproj -c Release -p:GoTargetOS=linux -p:go2csPath=<tree>/src/
cd avcensus && go build -o avcensus . && TOOL=$PWD/avcensus OUT=<dir> bash ../census/run.sh <a module that requires sqlite v1.62.0>
```

The converter is given the output directory as its second argument. The module for the sqlite runs needs
modernc.org/sqlite v1.62.0 and modernc.org/libc v1.80.1 in its `go.mod`; the tool is run from inside it.

`census/tables.txt` is the output of a short script over the `.tsv` files: counts per tag and form, then for the two
sqlite tags the split by package and array name, `RawMem` by form, context and operand, the unnamed arrays by form
and by operand, and for two standard library tags the forms and the slice sites one per line.
