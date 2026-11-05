# h-init-scope-census

The instruments behind [`PLAN-init-statement-scope.md`](../../../PLAN-init-statement-scope.md) (H, 2026-11-04,
measured at master `33589333b2`). Nothing here is built by any solution or by `go test`. Each is run by hand.

| file | what it does |
|---|---|
| `gocensus/` | Go side: lists every `if`, `switch` and type switch carrying an init, and every bare Go block, in source order, with statement kind, init kind, names and name uses. Reads `key<TAB>path` lines on stdin. |
| `cscensus/` | C# side (Roslyn 4.14): lists every bare block and every else-if init block, with the names it declares, the enclosing declaration space and every other occurrence of those names in it. Same input. |
| `census.py` | Builds the per-target file sets from the position maps, runs both sides, joins them file by file, and writes `joined.jsonl` and `reconcile.json`. `--plant` is the red-first control. |
| `analyze.py` | The tables and the candidate-shape populations, from `joined.jsonl`. |
| `scoperules/` | Compiles (and, where it can, runs) each C# scope rule the shapes depend on, then prints the IL of today's shape and of each candidate. |
| `transform.py` | Applies shape (a), strict, to a corpus copy: it blanks each admitted block's `{` and `}` lines and keeps every line number. |
| `ilcompare/` | Compares every method body of two build-output trees. A body reads identical, equivalent (once locals, branch targets and tokens are canonicalized) or different, and each different body is named. A third argument prints the divergence of the methods it matches. |

Run from this folder, with `go` (1.26.10, `GOTOOLCHAIN=local`) and the .NET 10 SDK on `PATH`:

```bash
go build -C gocensus -o ../gocensus.exe .
dotnet build cscensus -c Release
python -I -X utf8 census.py <repo root> <out dir>
python -I -X utf8 census.py <repo root> <out dir>-plant --plant
python -I -X utf8 analyze.py <out dir>
dotnet run --project scoperules -c Release
```

The IL comparison needs two copies of the tree:

1. Build `src/go2cs-stdlib.slnx` in one copy and collect each project's own assembly.
2. Run `transform.py <out dir>/joined.jsonl <censused root> <copy root>` on the other, then build and collect it.
3. Run `dotnet run --project ilcompare -c Release -- <base dir> <new dir>`.

Pass the same `-c` to both builds. The census output and the build outputs belong outside the repository.
