# Trim probes (measurement only)

A measurement ref for trim stage 3 (docs/PLAN-golib-full-trim.md, section 9.11). It never lands; COORD deletes it
after the train. Five small programs, published against a go2cs source tree and compared with Go's own output.

| Probe | What it exercises | Go source |
|---|---|---|
| `consumer` | a C# consumer of golib, runtime and sort (`runtime.Caller` under a `sort.Slice` comparator) | none: C# only |
| `genprobe` | fmt and reflect over generic structs: `%v`, `%+v`, `reflect.TypeOf(...).Field`, `DeepEqual` | `go/main.go`, a Go twin written from the converted C# |
| `c32a` | reflect.Zero, nested sized arrays, MakeSlice / MakeMap / MakeChan, SetBytes on named byte slices, a tagged struct conversion read after a GC | `go/main.go` |
| `c32b` | reflect.New and Zero of pointer types: generated operations, the builtins table, the fallback (`**T`, an interface) | `go/main.go` |
| `c32c` | reflect.New and Zero(*C) for containers: an array, slice, map and chan | `go/main.go` |

`expected.txt` in each probe is Go's output (go1.24.13). The converted C# (`main.cs`, `package_info.cs`) is committed as
converted; the projects reference the tree under test through `go2csPath`.

## Run

```bash
bash src/tests/TrimProbes/trimprobes.sh "$PWD/src" default
```

The first argument is the ABSOLUTE path of the tree's `src` directory. Arms: `default` (the template-default
`dotnet publish`: ILLink partial trim, ReadyToRun, no Native AOT; the consumer's `D` arm mirrors the template), `F`
(Native AOT, full trim), `P` (Native AOT, partial trim; only consumer, genprobe and c32a carry it, and it takes hours).
A third argument overrides the runtime identifier. The script prints one row per program and exits 1 if any program
misses its expectation.

## Expected

- `default`: every program publishes and runs; consumer `RUNS-CLEAN`, the other four `GO-EQUAL`. A failure here is
  a release blocker for any trim stage (COORD's gate, 2026-10-09).
- `F` with the full trim stack (3a through 3c-2b(iii)): c32a, c32b and c32c `GO-EQUAL`; genprobe stops by design at
  family E (`GoReflect.buildFieldAccessor`, exit 2) and is reported, not judged.
