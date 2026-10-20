# Probe: a raw box's assert to a named interface, under the JIT and Native AOT

P2's first-deadline probe (claude/p2-first-deadline-probe 1f13c82bf0) printed a read error under Native AOT as
`&{read |0 0x8000000300000000}`, where Go and the JIT build print `read |0: i/o timeout`. COORD asked G to place it
(2026-10-20).

The place: golib's nominal adapter tier. A generated pointer adapter registers its factory under
`typeof(ж<T>)` (AdapterImplTemplate's module initializer). Since the box kind split (36b7e9d96b, 2026-08-26) no box
has that runtime type: `ж<T>` is abstract, and a box is a `StandardBox<T>`, `SlottedStandardBox<T>`,
`FieldRefBox<T>`, `ElemRefBox<T>`, `NativeBox<T>` and so on. The lookup keyed the box by its runtime type, so a
raw box never found its adapter, and every assert of one to a named interface fell to the run-time shell tier.
That tier closes the interface's shell over the POINTEE with `MakeGenericType`: the JIT does it, Native AOT
cannot for an instantiation the compiler did not root, and golib's hand-written `error` shell has no object-shell
fallback. fmt's type switch unwraps an interface value to its raw box before asserting `error` and `Stringer`, so
every `%v` of a pointer-sourced error missed under AOT.

The fix (claude/g-box-subject-key): `AdapterRegistry.TryGetAdapterFactory` keys a box by the `ж<T>` in its base
chain, memoized per runtime type. GolibTests' AdapterRegistryBoxKindTests holds it for each box kind.

## The programs

`main.go` asserts and prints a foreign `*fs.PathError` and a same-package `*localErr`, each once inside an `error`
(a generated adapter) and once as a raw box in `any`. Both types have a recorded `error` conversion, so both pairs
have a registered adapter. `bench/main.go` times `v.(error)` on a raw box held in `any`: `*fs.PathError` has a
registered adapter (io/fs records one), and `*localErr` has none there (that program never converts it to
`error`), so it stays on the shell tier and is the control.

Convert with `go2cs -platforms windows/amd64 <dir> <out>`. For the AOT arm, put the publish properties of
`src/tests/Performance/Directory.Build.targets` in a `Directory.Build.targets` beside the converted project under
a gate of your own, add `<IlcFoldIdenticalMethodBodies>false</IlcFoldIdenticalMethodBodies>`, and run
`dotnet publish -c Release`. On windows from Git Bash, the Visual Studio Installer directory (vswhere.exe) must be
on PATH for the native link.

## Readings (windows, i7 laptop, hop tree 874374df34, go1.26.9, .NET 10)

`main.go`, Go:

```
A %v of a foreign *PathError as error: read |0: i/o timeout
B direct Error(): read |0: i/o timeout
C any(error) holding *PathError asserts to error: true
D any holding a raw *PathError asserts to error: true
E %v of the raw one: open x: boom
F %v of a same-package *localErr: local: same assembly
G any holding a raw *localErr asserts to error: true
H *PathError asserts to Stringer (want false): false true
```

- JIT, before and after the fix: Go's output. Before the fix the asserts resolve on the shell tier.
- Native AOT before the fix (TrimMode partial, folding off): A `&{read |0 0x8000000500000000}`, D false,
  E `&{open x 0x8000000700000000}`, F `&{same assembly}`, G false. B, C and H as Go.
- Native AOT after the fix: Go's output, byte for byte.

Two facts the readings turned up:

- At the top of `Main` the registry held both pairs, `(ж<fs.PathError>, error)` and `(ж<localErr>, error)`, among
  60. The probe box's runtime type was `StandardBox<localErr>`, and `AdapterRegistry.TryWrap` answered False.
- Registering `StandardBox<T>` keys by hand in the middle of the AOT run fixed A and F but not D, E and G: under
  AOT those boxes were `SlottedStandardBox<T>` (`BoxShape<T>.Slotted` is volatile and had flipped), and under the
  JIT the same boxes were `StandardBox<T>`. A key per kind cannot work; the lookup maps every kind to its `ж<T>`.

`bench/main.go`, JIT, median of 9 runs of 5,000,000 asserts, three program runs each:

| `v.(error)` on a raw box | Go | before | after |
|---|---|---|---|
| `*fs.PathError`, registered adapter | 1.3 ns | 40.0-42.0 ns | 31.0-32.8 ns |
| `*localErr`, no adapter (control) | 1.2 ns | 35.3-37.9 ns | 35.8-36.5 ns |

Each arm's programs were converted by a converter built from that arm's own tree (before: 874374df34; after: the
seat merged onto 5530cc85f4) and built against that tree's golib; the AOT readings above are the same arms. A first
reading converted both arms with an older scratch converter and read the same within noise (`*fs.PathError`
41.1-41.7 before, 31.2-32.7 after; with `DOTNET_TieredCompilation=0` 60.6-61.4 before, 43.6-44.0 after).
