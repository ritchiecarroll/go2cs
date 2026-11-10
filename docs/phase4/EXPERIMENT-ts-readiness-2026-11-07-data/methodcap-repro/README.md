# The CLR per-type method cap: a generated repro and the TS closure readings (i7, 2026-11-10)

**The finding.** In a diagnostic build of the TS parse closure (local merge 8836958ee6, which carries three DIAGNOSTIC
hand-patches for the open rows C22e, C24 and C28), the converted `parsedemo` builds with rc 0 but cannot start. CoreCLR
refuses to load `ast_package`:

    System.TypeLoadException: Type 'go.github.com.microsoft.TypeScript.tsc.internal.ast_package' from assembly
    'github.com.microsoft.TypeScript.tsc.internal.ast, ...' contains more methods than the current implementation allows.

`ast_package` has 116,102 method definitions. The repro below pins the threshold for a compiled package class
exactly. **65,521 method definitions load; 65,522 do not.** That is not 65,524, the figure read from
dotnet/runtime v10.0.0's `MAX_SLOT_INDEX` (65535 - 10). It matches the law P1 measured separately across nine type
shapes: inherited virtual slots + (definitions - overrides) <= 65,525. A static class inherits `System.Object`'s 4
virtual slots, so 4 + 65,521 = 65,525. A guard threshold must come from this measurement, not from the source
constant. 107,787 of ast_package's methods (93%) are `[GoPromoted]` forwarders
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

**Pinning the edge: the K axis.** The optional fourth argument K adds K plain package funcs. Each adds exactly one
method (measured: count = 2NM + 2N + 5 + K). Every K run keeps the loading control's shape, N=250 and M=130, so K is
the only thing that changes:

| K | `main_package` methods (measured) | Predicted beforehand | Result |
|---|---|---|---|
| 14 | 65,519 | loads (i7 and P1) | loads; output matches Go |
| 16 | 65,521 | fails (i7's Object-slot hypothesis, edge 65,520); loads (P1's law) | **loads**; output matches Go. i7's hypothesis is FALSIFIED |
| 17 | 65,522 | fails (P1's law) | `TypeLoadException` before `main` |
| 18 | 65,523 | fails (count-only reading; P1's law) | `TypeLoadException` before `main` |

K=18 fails at the same count as the N=697, M=46 run, at a different shape. From metadata, the two failing assemblies
and the loading K=16 one show the same method shape in the package class:
- every method is static;
- every method has generic arity 0;
- there are no MethodImpls and no bodiless methods.
So the count alone decides (`measured-shape-compare.txt`, from `methodflags.ps1.txt`).

An earlier plan bracketed 65,524 with 65,523 and 65,525. The 65,525 run was stopped once 65,523 had failed; the K
axis replaces it.

For a red-first test: 65,521 (N=250, M=130, K=16) is the highest count that loads, and 65,522 (K=17) is the lowest
that fails. `repro-results.txt` holds every result block.

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
