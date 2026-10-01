# crosspkg-census

The read-only go/types censuses behind `docs/phase4/DESIGN-crosspkg-promoted-methods.md` (TRAIN M). Each tool loads
packages with `golang.org/x/tools/go/packages` (`Tests: true`: production and test variants), prints one row per
finding, and ends with a one-line summary. They never write anywhere but stdout.

Build from this directory with the pinned toolchain (`GOROOT` = the go1.24.13 root, `GOTOOLCHAIN=local`,
`CGO_ENABLED=0`):

```
go build -o census.exe     ./cmd/census
go build -o sim.exe        ./cmd/sim
go build -o r5.exe         ./cmd/r5
go build -o ifaceembed.exe ./cmd/ifaceembed
```

Run each over the standard library once per target, from any directory:

```
GOOS=windows census.exe -root "$GOROOT/src" std
GOOS=windows sim.exe -builtin testdata/builtin-members.txt -root "$GOROOT/src" std
```

Then repeat with `GOOS=linux` and `GOOS=darwin`. Over a behavioral module, pass `-root <module dir> ./...`, and retry
under `GOOS=linux` when the summary shows load errors (a linux-only module).

| tool | answers |
|---|---|
| `census` | Go's method sets of T and *T, the concrete methods promoted through a hop that CROSSES packages; the well-known interfaces each type then satisfies; the drop rows (names Go removes by field shadow or same-depth ambiguity); per row: `unexported`, `shim`, `clash`, `sigunexp` (C3), `transit`/`internalfwd`/`multicross` (C4), `genencl`, `collide` (G's uniqueness rule (i') at the Go level) |
| `sim` | a model of go2cs-gen's forwarder emission TODAY vs the CUT vs Go: removed / added / over-claims / under-claims, and name clashes against the enclosing package scope, `go.builtin` (`-builtin`), dot-imports, and the external-test bridge |
| `r5` | pointer-receiver promotions: through VALUE-only hops (the value-set over-claim R5 fixes cross-package; `same` is the residual), and through a POINTER hop (`same-ptrhop` / `cross-ptrhop`: B1's by-ref forwarders) |
| `ifaceembed` | every embedded INTERFACE field (the converter footprint of the `[GoEmbedded]` marker), and how many are COMPOSITE |

- **Controls.** `testdata/plant` and `testdata/simplant` are planted modules. Every arm has a shape there that must
  fire: crossing, field shadow, same-depth ambiguity, function / var / builtin / dot-import / bridge clash, C3
  `Leak() hidden`, C4 `Deep`, the generic enclosing struct `Gen`, and `Col` (collide).
- **Builtin list.** `testdata/builtin-members.txt` is the public static member list of golib's `go.builtin` (the
  global `using static`). It was read by reflection from golib.dll, excluding accessors and operators, and written
  UTF-8.
