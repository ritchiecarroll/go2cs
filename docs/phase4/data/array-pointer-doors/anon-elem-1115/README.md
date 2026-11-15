# Evidence for DESIGN-array-pointer-doors.md, section 16.9

Point-in-time record, lane P5, 2026-11-15. Measured on `claude/p5-w17-door-anon-elem`, cut from master
`0c1d295ea8` (batch 309). Go 1.26.11, one cloud linux/amd64 VM, .NET SDK 10.0, Release, `GoTargetOS=linux`.
Nothing here ran on windows or darwin; the census covers them by type-checking under `GOOS`/`GOARCH`.

## Files

| Path | What it is |
|:--|:--|
| `red-master.txt` | master's converter over `src/tests/Behavioral/AnonStructArrayPointerDoor` as first written: exit code 1 and its two warnings (`<kit>` stands for the scratch output directory) |
| `unit-red.txt` | the converter test's first failure line with the door's lift taken out: 6 unresolved types, one for each function of its fixture (`<out>` stands for the test's temporary output root) |
| `census/main.go`, `go.mod`, `go.sum` | the census tool (go/packages and go/types; golang.org/x/tools v0.52.0) |
| `census/run.sh` | the five runs: the planted positive, then the standard library on four targets |
| `census/planted.out` | the tool over the behavioral program: 13 sites of the class |
| `census/std-<target>.out` | the tool over the standard library, production and test files: 0 sites on each target |

## The census predicate

A conversion `(*A)(x)` whose type is a pointer to an array, where the array's element type holds a struct type
written in place with at least one field. "Holds" follows arrays, pointers, slices, maps and channels, and stops at
a named type. The class the doors reach is the one whose operand `x` is an `unsafe.Pointer`. The tool prints a
second class for any other operand, and a control line: every conversion of an `unsafe.Pointer` to a pointer to an
array, whatever its element. A line that is absent is a count of 0.

| target | packages | files | control | the class | other operand |
|:--|--:|--:|--:|--:|--:|
| linux/amd64 | 2490 | 3080 | 120 | 0 | 0 |
| linux/arm64 | 2491 | 3064 | 121 | 0 | 0 |
| darwin/amd64 | 2398 | 3047 | 118 | 0 | 0 |
| windows/amd64 | 2473 | 2935 | 128 | 0 | 0 |
| the behavioral program (planted positive) | 62 | 586 | 129 | 13 | 0 |

The planted run loads the program with its imports, so its package, file and control counts include the standard
library packages the program reaches. Its 13 sites are all in the program's `main.go`.

## Re-running

```
bash census/run.sh <GOROOT> <repo>/src/tests/Behavioral/AnonStructArrayPointerDoor
```

The converter test is `TestArrayPointerDoorLiftsAnAnonymousStructElement` in `src/go2cs`.
