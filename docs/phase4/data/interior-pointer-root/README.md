# Evidence for DESIGN-interior-pointer-root.md

Point-in-time record, lane P5, 2026-11-14. Measured at master `314e1d8b60` and at H's
`claude/h-cgo-phase3-helpers` `83fbf28560`. Go 1.26.10, one cloud linux/amd64 VM (4 vCPU, 15 GB), .NET SDK
10.0.400, Release, `GoTargetOS=linux`. Nothing here ran on windows or darwin; the census tables cover them by
type-checking under `GOOS`/`GOARCH`.

## Files

| Path | What it is |
|:--|:--|
| `InteriorPointerShapes/main.go`, `go.mod` | the 12-shape program |
| `InteriorPointerShapes/go.out` | `go run .`, 17 lines |
| `InteriorPointerShapes/converted-master.cs.txt` | master's emission of `main.go` (the `main.cs` the converter wrote) |
| `InteriorPointerShapes/converted-master.out` | that emission built and run at master; identical in 3 runs |
| `InteriorPointerShapes/converted-htip.out` | the SAME emission built and run against H's tip; identical in 3 runs, and byte-equal to the master output |
| `InteriorPointerShapes/prototype.cs.txt`, `prototype.out` | the emission rewritten by hand to the design's spellings, built against master plus `prototype-on-master.diff` |
| `prototype-on-master.diff` | the throwaway golib/unsafe change (2 files, 12 lines). It never lands as written. |
| `CmsgStress/main.go`, `go.mod` | the stress of `syscall.UnixRights` (design 1.3) |
| `ipcensus/` | the census tool (go/packages and go/types; golang.org/x/tools v0.52.0) |
| `census/std-<os>-<arch>.tsv` | one row per site, per target |
| `census/*.summary.txt` | the tool's stderr per run: packages loaded, load errors, the helper functions it followed, site count |
| `std-outside-runtime.txt` | the 32 standard library sites outside `runtime` and `internal/runtime/maps`, union of the five targets |
| `corpus-counts.py`, `corpus-counts.txt` | the counts of the emitted spellings in the committed corpus, and the script that takes them |
| `sqlite-summary.txt` | counts for modernc.org/sqlite v1.62.0 with modernc.org/libc v1.80.1, linux/amd64 and windows/amd64. The per-site tables are about 700 KB each and are not committed; the tool regenerates them. |

`.cs.txt` is a converted `.cs` under a name no project picks up.

## Row format (`census/*.tsv`)

Tab separated, no header: tag, package, file:line, via, origin, use, pointee holds pointers, in a test file, target
type.

- **via**: how the operand was derived. `uintptr+` is `unsafe.Pointer(uintptr(p) + n)`; `unsafe.Add`;
  `helper:<name>` for a function the tool found to return a parameter or receiver moved by one of those; a
  `local<-` prefix where the derived pointer went through a local variable first; `local-uintptr+` where the
  arithmetic was done on a local `uintptr`.
- **origin**: the tool's label for the base. `managed` (an address-of expression, a typed Go pointer, or a method
  on one), `number` (a `uintptr` value), `pointer-var` (an `unsafe.Pointer` variable or field, not followed
  further).
- **use**: `read`, `write`, or `held:<how>` (stored, returned, passed as an argument, sliced, address taken).
- **pointee holds pointers**: Go's answer for the target type. It is not go2cs's answer: `sync.Mutex` reads
  `false` here and its C# form holds a reference (design 2.4).

## Limits of the tool

- **origin is a heuristic.** It reads the expression, not the data flow. A method that takes the address of its
  receiver (`(*Cmsghdr).data`) reads `number`; `unsafe.Add(unsafe.Pointer(&(*T)(unsafe.Pointer(bp)).Field), 0)`
  over ccgo memory reads `managed`. The design's statements about a site come from reading the site.
- **Helpers are found by one rule**, to a fixpoint of at most 6 rounds: a function with one `unsafe.Pointer`
  result that returns a parameter or receiver moved by arithmetic. A helper of another shape (two results, or the
  pointer returned through a field) is not followed, and its callers are not rows.
- **A site is a conversion.** A pointer derived by arithmetic and then passed on as an `unsafe.Pointer` without a
  typed conversion in the same expression is not a row.
- **Test files were not loaded** for these tables (the column is `false` on every row). `IP_TESTS=1` loads them.

## Re-running

Put Go 1.26.10's `bin` first on `PATH`, with `GOTOOLCHAIN=local CGO_ENABLED=0`.

**The program.**

```
cd InteriorPointerShapes && go run . > go.out
go2cs <this directory>/InteriorPointerShapes <an empty output directory>
dotnet build <output directory> -c Release -p:GoTargetOS=linux -p:go2csPath=<worktree>/src/
<the built executable> > cs.out ; diff go.out cs.out
```

The output directory is the converter's second positional. Without it the conversion lands beside `main.go`.
Expected at master and at H's tip: lines 2, 3, 13 and 17 of the output differ (shapes 1, 8 and 9).

**The stress.** The same four steps for `CmsgStress`. It prints one line, the count of messages with a wrong
descriptor. Read at master: 0 in each of 3 runs. Go prints 0.

**The census.**

```
cd ipcensus && go build -o ipcensus .
GOOS=linux GOARCH=amd64 ./ipcensus std-linux-amd64 std > std-linux-amd64.tsv 2> std-linux-amd64.summary.txt
```

Run it from a directory whose module resolves the patterns. `std` resolves anywhere. For sqlite, a scratch module
that requires `modernc.org/sqlite v1.62.0` and `modernc.org/libc v1.80.1`, with the patterns
`modernc.org/sqlite/... modernc.org/libc/... modernc.org/memory/...`.

**The corpus counts** in the design are `corpus-counts.py`, run from the repo root at master `314e1d8b60`; its
output is `corpus-counts.txt`. It matches the cast's type argument by bracket depth and reads the file list with
`core.quotepath=false`. A `grep -o` with a greedy pattern over the default `git ls-files` output undercounts (900
against 919): it reads two casts on one line as one and skips the 17 golib files whose name is not ASCII.
