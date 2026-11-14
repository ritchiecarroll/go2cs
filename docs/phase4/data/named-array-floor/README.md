# Evidence for the named-array floor

Point-in-time record, lane P5, 2026-11-14. Branch `claude/p5-named-array-floor` off master `c2919b8b16`. Go 1.26.11,
one cloud linux/amd64 VM (4 vCPU, 15 GB), .NET SDK 10.0, Release, `GoTargetOS=linux`. Nothing here ran on windows,
darwin or arm64 on this box; the hosted runs are named in `predictions.txt` when they report.

The change is in golib only (`ж.cs`, `ж.NativeBox.cs`) and is described in
[`DESIGN-native-array-view.md`](../../DESIGN-native-array-view.md), the amendment of 2026-11-14. The converter and the
generator are untouched, so every `converted.cs.txt` here is master's emission and only the runtime under it differs
between the `-master` and `-floor` outputs.

## Files

| Path | What it is |
|:--|:--|
| `typed/main.go`, `go.mod` | a named array reached through a typed pointer over C memory: three arms, one per process |
| `gomem/main.go`, `go.mod` | a named array viewed over Go memory: seven arms (`ga` to `gh`), one per process |
| `*/go.out` | Go's answer per arm (`go build`, then the binary once per arm) |
| `*/converted.cs.txt` | the emission of `main.go` at master (the `main.cs` the converter wrote) |
| `*/converted-master.out` | that emission built against master's runtime, one arm per process |
| `*/converted-floor.out` | the same emission built against the seat's tip |
| `gomem/plain-conversion.go.txt`, `.cs.txt` | the probe with one more arm, `(*smallMem)(&h.buf)` with no `unsafe`; its emission does not compile (CS0030), so the arm is kept out of `main.go`. A BOARD row |
| `shapes/converted-floor.out`, `shapes/prototype-floor.out` | the 24-arm program and its 11 prototype arms from the array-pointer design's evidence (`data/array-pointer-doors/NativeViewShapes/`, on `claude/p5-native-view-design`), the emission of master, built against the seat's tip |
| `shapes/converted-master.out`, `shapes/prototype-master.out` | the same two, built against master's runtime, for the comparison |
| `predictions.txt` | the predictions as posted before the cut existed, the stop lines, and each scoring in order |

`.cs.txt` is a converted `.cs` under a name no project picks up. In the `.out` files `<tree>` stands for the
worktree and `<kit>` for the scratch directory the program was converted in. Addresses printed by the `-master`
outputs differ from run to run.

## Reading the outputs

The floor's panic text is `cannot view native memory as <type>: the address has no managed element storage behind
it`. An arm that ends in it at the floor and printed a number at master is an arm the floor refuses. Go's answer for
the same arm is in `go.out`: where Go answers and the floor refuses, the arm is waiting for the array-pointer doors.

## Re-running

```
go build -o probe . && for a in ga gb gd ge gf gg gh; do ./probe $a; done          # gomem, Go's side
go2cs <dir> <outdir>                                                               # the emission
dotnet build <outdir> -c Release -p:GoTargetOS=linux -p:go2csPath=<tree>/src/      # against a tree's runtime
```

The guards that hold the behaviour are in GolibTests: `NamedArrayViewFloorTests` and
`NativeArrayViewFloorTests.ANilPointerAtAnArrayPointeeIsANilPointer`.
