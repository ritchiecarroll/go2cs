# Trim probes (measurement only)

A measurement ref for trim stage 3 (docs/PLAN-golib-full-trim.md, section 9.11). It never lands; COORD deletes it
after the train. Eight small programs, published against a go2cs source tree and compared with Go's own output.

| Probe | What it exercises | Go source |
|---|---|---|
| `consumer` | a C# consumer of golib, runtime and sort (`runtime.Caller` under a `sort.Slice` comparator) | none: C# only |
| `genprobe` | fmt and reflect over generic structs: `%v`, `%+v`, `reflect.TypeOf(...).Field`, `DeepEqual` | `go/main.go`, a Go twin written from the converted C# |
| `c32a` | reflect.Zero, nested sized arrays, MakeSlice / MakeMap / MakeChan, SetBytes on named byte slices, a tagged struct conversion read after a GC | `go/main.go` |
| `c32b` | reflect.New and Zero of pointer types: generated operations, the builtins table, the fallback (`**T`, an interface) | `go/main.go` |
| `c32c` | reflect.New and Zero(*C) for containers: an array, slice, map and chan | `go/main.go` |
| `registry` | expvar's memstats publish, then reflect.New and Addr over `runtime.MemStats.BySize` (a container only a per-OS assembly's exported struct reaches) | `go/main.go` |
| `stringer` | fmt reaching `String()` by duck typing: `reflect.Kind`, `time.Duration`, main's named int, struct and pointer receiver, `%v`/`%s`/`%d`, a direct `x.(fmt.Stringer)` | `go/main.go` |
| `fieldbox` | reflect field access through a pointer: Addr write-through, fmt of a `*struct`, SetInt read back, `&s.A` identity, a promoted embedded field, a defined type over a struct | `go/main.go` |

`expected.txt` in each probe is Go's output (go1.24.13). The converted C# (`main.cs`, `package_info.cs`) is committed as
converted; the projects reference the tree under test through `go2csPath`.

## Run

```bash
bash src/tests/TrimProbes/trimprobes.sh "$PWD/src" default
```

The first argument is the ABSOLUTE path of the tree's `src` directory. Arms: `default` (the template-default
`dotnet publish`: ILLink partial trim, ReadyToRun, no Native AOT; the consumer's `D` arm mirrors the template), `F`
(Native AOT, full trim), `P` (Native AOT, partial trim; consumer, genprobe, c32a and the three newest carry it, and it
takes hours).
A third argument overrides the runtime identifier. The script prints one row per program and exits 1 if any program
misses its expectation.

## Expected

- `default`: every program publishes and runs; consumer `RUNS-CLEAN`, the other seven `GO-EQUAL`. A failure here is
  a release blocker for any trim stage (COORD's gate, 2026-10-09).
- `F` with the full trim stack (3a through 3c-2b(iii)): c32a, c32b and c32c `GO-EQUAL`; genprobe stops by design at
  family E (`GoReflect.buildFieldAccessor`, exit 2) and is reported, not judged.
- `F`, AMENDED 2026-10-12 (G): each newer probe pins the seat that made it Go-equal, so it reads red on an older tree.
  - `registry` needs the per-OS exports registry (`cf5b534a1c`: reflect.New over BySize) and family E (its last line,
    Addr on a field).
  - `stringer` needs the method-set keep (`ec6df12120`).
  - `fieldbox` needs family E's field-box table (`claude/g-trim-fieldbox`). Its line 5 checks the promoted field's value
    through its own pointer, not `x == &o.X`, whose identity is a separate pre-existing defect (sized on its own).
  - genprobe equals Go's output from the method-set keep on; it stays reported, not judged, so older trees read as
    before.
