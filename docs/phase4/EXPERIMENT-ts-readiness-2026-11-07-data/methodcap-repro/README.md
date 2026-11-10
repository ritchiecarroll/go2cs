# The CLR per-type method cap: a generated repro and the TS closure readings (i7, 2026-11-10)

**The finding.** In a diagnostic build of the TS parse closure (local merge 8836958ee6, which carries three DIAGNOSTIC
hand-patches for the open rows C22e, C24 and C28), the converted `parsedemo` builds with rc 0 but cannot start. CoreCLR
refuses to load `ast_package`:

    System.TypeLoadException: Type 'go.github.com.microsoft.TypeScript.tsc.internal.ast_package' from assembly
    'github.com.microsoft.TypeScript.tsc.internal.ast, ...' contains more methods than the current implementation allows.

`ast_package` has 116,102 method definitions. The repro below puts the measured threshold for a package class between
**65,505 method definitions (loads) and 65,523 (does not)**. That is lower than 65,524, the figure read from
dotnet/runtime v10.0.0's `MAX_SLOT_INDEX` (65535 - 10). A likely reason, NOT yet verified: the loader counts slots, not
definitions, and a class also carries `System.Object`'s 4 virtual slots, which would put the last loadable count at
65,520. 107,787 of ast_package's methods (93%) are `[GoPromoted]` forwarders
that go2cs-gen's TypeGenerator emits into the one package class: a `this ref T` / `this ж<T>` pair per promoted
method per embedding type. TypeScript's `internal/ast` embeds `Node` (through `NodeDefault`) in about 214 node types.

The C# compiler has no such cap, so the build is green; only loading fails.

## The repro

`gen_methodcap.py.txt` (the generator, a Python script stored as .txt) writes a Go module with one struct, `Base`,
carrying N pointer-receiver methods, embedded in M struct types `T0..T{M-1}`. `main` calls a promoted method on the
first and last types and prints a fixed line. `repro-methodcap.sh.txt` runs the steps below and records the package
class's method count from the built assembly's metadata (`methodcount.ps1.txt`; nothing is loaded):

1. generate;
2. Go oracle (`go run`);
3. `go2cs -recurse`;
4. `dotnet build -c Release`;
5. run;
6. compare.

At go2cs master **66a2e023ca**, the generated package class `main_package` has exactly **2NM + 2N + 5** methods:
2NM forwarders, Base's N methods in two forms, and 5 others. Every run below measured that count exactly.

| N | M | `main_package` methods (measured) | Result |
|---|---|---|---|
| 3 | 2 | 23 | loads; output matches Go |
| 250 | 130 | 65,505 | loads; output matches Go byte for byte (the under-cap control) |
| 697 | 46 | 65,523 | `TypeLoadException` on `go.repro.main_package` before `main` |
| 250 | 131 | 66,005 | `TypeLoadException` on `go.repro.main_package` before `main`, the same text as ast |

The runs were meant to bracket 65,524 with 65,523 and 65,525; the count is always odd, so 65,524 itself cannot be
produced. 65,523 already fails, so the 65,525 run was stopped as redundant. The last loadable count lies between
65,505 and 65,521. A red-first test should use 65,505 (N=250, M=130) as the loading control and 65,523 or above as the
failing arm. `repro-results.txt` holds the four result blocks.

To rerun: `python gen_methodcap.py <dir> <N> <M>`, then convert and build `<dir>` as any `-recurse` module. Builds
at N×M ≈ 32k take about 16 minutes on the i7.

## The TS closure readings

Taken from the same diagnostic build, metadata only unless marked:

- `measured-ast.txt`: the ast assembly. 59,209 types and 256,275 methods; `ast_package` has 116,102 methods but
  only 2,244 distinct names. 244 names have 428 overloads each.
- `measured-top10-types.txt`: the ten largest types across the demo closure (27 of 27 packages built) and the
  type-check closure (36 of 54 built). `ast_package` is the only type over the limit; the runner-up is `x/sys/windows`
  at 13,563.
- `measured-ast-per-struct.txt`: forwarder declarations per embedding type in ast. The maximum is **511**, on 15
  `*Base` types (252 `this ref` + 259 `this ж<>`); the most common count is 508.
- `measured-promoted-method-values.txt`: uncalled method values and method expressions whose method is promoted.
  There are **none** in either closure. `mcexpr_main.go.txt` type-checks each package from source;
  `mcexpr-control_main.go.txt` is its positive control.
- `predict-typecheck-closure.txt` and `predict-emit-closure.txt` are a **prediction**, not metadata.
  `mcpredict_main.go.txt` counts promoted methods from Go method sets in the export data. It predicts 113,392
  forwarders for ast against 107,787 measured, about 5% high. For the 18 unbuilt type-check packages it puts checker
  at about 2,362 forwarders + 2,494 declared methods. Nothing else in the type-check or emit closure is near the cap.

Tools: `methodcount.ps1.txt`, `methodnames.ps1.txt`, `toptypes.ps1.txt`, `mcpredict_main.go.txt`,
`mcexpr_main.go.txt`. Each reads metadata or Go export data only.
