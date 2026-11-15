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

## Re-run at the landed master (2026-11-14 19:35Z)

Batch 305 landed as master `c2919b8b16`; the design branch merged it at `8a378c0ff1`. The merge base is the cut tip
`837a413fdc`. At the merged tree, same box, same commands:

- The converter built from the merged tree emits `main.cs` and `package_info.cs` byte-identical to the cut's.
- The 24 arms give the output and exit codes of `converted-cut.out`. One line differs on every run, at any tree:
  the eight bytes arm M0 prints are the managed reference stored into the C memory, an address.
- The 11 prototype arms give `prototype.out` byte for byte.
- Generator output for the program: the two array wrappers (`RawMem`, `smallMem`) are byte-identical; the struct
  `hdr` gains `System.IEquatable<hdr>` (P4's S4, in the same batch), which no arm depends on.
- Corpus counts: 13 span emissions in the same 6 files, 4 in the `.cs.auto` sibling, `To<array<` on 84 lines in 28
  files, as at the cut. `src/core` differs from the cut in `fmt/errors.cs`, `unicode/utf8/utf8.cs` and
  `golib/runtime/HashCode.cs`.

The runner was first checked on the cut's own build: it reproduces `converted-cut.out` except the M0 line. No new
output file is committed, because none would differ from the ones here.

## Amendment 2026-11-14 22:40Z: the census by arm and two probes (`amend-1114/`)

For section 10 of the design (COORD's rulings 3 to 6).

| Path | What it is |
|:--|:--|
| `amend-1114/by-arm.txt` | door A's slice sites split by arm, the callers of libc's parameter functions in sqlite's generated code (windows/amd64), the probe's table, and the 11 std lines read at the source |
| `amend-1114/prng/main.go`, `go.mod`, `go.out` | libc's windows `Xmemcpy` body with six callers that pass the number of a Go variable; Go's answers |
| `amend-1114/prng/converted.out`, `prototype.out` | the conversion at master's emission and with the one-line hand edit, built against golib at `4c8ee1cbd2` on linux/amd64 |
| `amend-1114/prng/prototype-edit.txt` | the two spellings of the edited line, and the struct's emitted layout |
| `amend-1114/w/main.go`, `go.mod`, `go.out` | arm W, the twin of w5b2 that runs as Go: three statements over a local `[16]byte` |

The `prng` probe was built with no prediction written first; `predictions.txt` says so in its fifth set. Its
sixth set holds the predictions for the doors, written before any door exists.

```
cd amend-1114/prng && go build -o prng . && for a in pa pb pc pd pe pf; do ./prng $a; done
go2cs -go2cspath <tree>/src <kit>/prng <kit>/prng
dotnet build prngprobe.csproj -c Release -p:GoTargetOS=linux -p:go2csPath=<tree>/src/
```

## Amendment 2026-11-15: door A as cut (`scope-probe/`, `door-a-1115/`)

For section 11 of the design. Converters: base = the batch-305 converter; cut = the converter built at
`a0c43a357b`. One cloud linux/amd64 VM, Go 1.26.11, .NET SDK 10.0, Release, `GoTargetOS=linux`.

| Path | What it is |
|:--|:--|
| `scope-probe/main.go`, `go.mod`, `go.out` | 25 sliced pointer-to-array conversions, one per route the converter has for the conversion |
| `scope-probe/base-305.cs.txt`, `cut-before-fix.cs.txt`, `cut.cs.txt` | the program's emission by the base converter, by the cut before `a0c43a357b` (22 shapes changed, 14 of them not the copying span) and by the cut at it (8 changed, each the copying span) |
| `door-a-1115/footprint/two-seeded-<target>.files.txt` | `diff -rq` of the base root against the cut root, each seeded from `src/core`, per target |
| `door-a-1115/footprint/two-seeded-<target>.diff.txt` | the same roots' unified diff under `src/core`, CR-stripped; the two run artifacts that differ are named and their bodies omitted |
| `door-a-1115/footprint/arm-961d63313b-<target>.files.txt` | `diff -rq` of a seed taken at `961d63313b` against a `-stdlib runtime` reconvert of it by the cut converter: `runtime.csproj` is not in it on any target |
| `door-a-1115/kits/NativeViewShapes.main.cs.txt`, `.converted.out` | the 24-arm program as the cut converter emits it, and its run against the tree at `a37a986860`, one arm per process |
| `door-a-1115/kits/prng.*`, `w.*` | the same for the six memcpy arms of `amend-1114/prng/` and for arm W |
| `door-a-1115/lazy/main.go`, `go.mod`, `go.out` | seven arms that write at the address of a named array: five before the array's first use, two controls |
| `door-a-1115/lazy/main.cs.txt`, `converted.out` | its emission and its run: the five print 0 where Go prints the value written |
| `door-a-1115/lazy/census/main.go`, `go.mod`, `go.sum` | a census tool: the defined named array types of a package closure, and each conversion to `unsafe.Pointer` of a pointer to one |
| `door-a-1115/v2/NativeViewShapes.converted.out`, `prng.converted.out` | the same two kits run against the tree at V2's commit (`a7490fcee7`): N, pa, pb, pc and pe print Go's answers; no other line differs from `door-a-1115/kits/` |
| `door-a-1115/lazy/census/sq-<target>.tsv`, `.summary.txt` | its rows and totals for `modernc.org/sqlite/...` v1.62.0 on linux/amd64 and windows/amd64 (run inside a module that requires it, `GOFLAGS=-mod=mod`) |
| `door-a-1115/reassign/main.go`, `go.mod`, `go.out`, `main.cs.txt`, `converted.out` | design 13.1: ten arms that take an array's address, assign the array whole, then write or read through the address; its emission, and its run against the tree at the review fixes (`d133673c65`): eight lines differ from Go |
| `door-a-1115/reassign-plain/` (the same five files) | the same with no `unsafe`: a slice of the array, or a pointer to an element, taken before the whole assignment; seven of nine lines differ from Go |
| `door-a-1115/lazy/converted-option1.out` | design 14: the same program run against the tree with the pin paths publishing first (`ddba5cc1e7`); seven lines equal to `go.out`. The program is now the behavioral project `NamedArrayAddressWrite` |
| `door-a-1115/chain/ZzChainProbeTests.cs.txt`, `probe.out` | design 14.4: a scratch GolibTests class (never committed as a test) that advances one pointer N times with `unsafe.Add` over a Go value and over C memory, and its readings: view capacity, bytes kept, the cost of the comparing loop, and `==` after N steps. The last line is the 1,000,000-step comparison it did not return from |
| `door-a-1115/chain/probe-first.out` | the first form of the probe, with the view taken 4 bytes inside the value: bounded through 61 steps, Go's capacity from 63 |
| `door-a-1115/chain/reach.txt` | the lines that assign `unsafe.Add`'s result back to its first argument, in the sqlite closure's Go sources and in the standard library at 1.26.11 |
| `door-a-1115/chain/red-first.out` | design 15: `UnsafeAddChainTests` at the parent commit `70abf5dd5a`: 5 rows green, 9 red, and the two rows that end the test host with a stack overflow, run alone |
| `door-a-1115/chain/probe-after.out` | design 15: the probe of `probe.out` run again with the change (`fcc2a35455`): bounded at every depth, no bytes kept, `==` returns at 1,000,000 steps |

In the footprint files `base/` and `cut/` stand for the two conversion roots, and `seed/` and `reconvert/` for
the arm's. `<kit>` in an `.out` file is the scratch directory the program was converted in. The `lazy` program
is a finding on master that the doors do not touch (design 11.6); it is here because this seat's kit runs
found it.

## Amendment 2026-11-15: door B as cut (`door-b-1115/`)

For section 16 of the design. Converters: base = the converter built at `04660ffaa6` (the chain seat's tip); cut =
the converter built at `5244cfbe2b`. One cloud linux/amd64 VM, Go 1.26.11, .NET SDK 10.0, Release,
`GoTargetOS=linux`.

| Path | What it is |
|:--|:--|
| `door-b-1115/scope-probe/main.go`, `go.mod`, `go.out` | 55 sibling shapes of an index, an element's address, a dereference and a whole store through `(*A)(p)`: the forms the doors take and the forms they decline |
| `door-b-1115/scope-probe/base.cs.txt`, `cut.cs.txt`, `base-against-cut.diff.txt` | its emission by each converter and their diff: 40 lines change, one for one |
| `door-b-1115/lenprobe/main.go`, `go.mod`, `go.out` | the shapes of 16.3: `len`, `cap` and `range` over the dereference and over the conversion, with and without a call in the operand |
| `door-b-1115/lenprobe/base.cs.txt`, `cut.cs.txt`, `converted-base.out`, `converted.out` | its emission by each converter and each one's run: the base's five printed lines differ from Go's and it then panics on nil; the cut's equals `go.out` |
| `door-b-1115/rprobe/main.go`, `go.mod`, `go.out`, `cut.cs.txt`, `converted.out` | 16.6: the byte-view line R's kit reported, in three operand forms, with a named whole store and an alias whole store; the run equals `go.out` |
| `door-b-1115/iprobe/main.go`, `go.mod`, `go.out`, `base.cs.txt`, `cut.cs.txt` | sqlite's function-number helper, `(*iface)(unsafe.Pointer(&f))[1]` with `f` an interface: both converters emit the same file (the function-cookie route, which the doors do not reach) and it prints Go's two lines |
| `door-b-1115/kits/NativeViewShapes.main.cs.txt`, `.converted.out` | the 24-arm program as the cut converter emits it, and its run against the tree at the regen commit, one arm per process: 21 arms equal `NativeViewShapes/go.out`; F, F0 and Q do not |
| `door-b-1115/footprint/two-seeded-<target>.files.txt` | `diff -rq` of the base root against the cut root, each seeded from `src/core` at `04660ffaa6`, per target |
| `door-b-1115/footprint/two-seeded-<target>.diff.txt` | the same roots' unified diff under `src`, CR-stripped; the two run artifacts that differ are named and their bodies omitted |

In the footprint files `base/` and `cut/` stand for the two conversion roots. The panic traces in an `.out` file
name the program by its module path.
