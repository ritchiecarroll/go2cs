# g-crosspkg-box-census -- the cross-package exported-variable box's census (point-in-time record)

The data behind [`DESIGN-crosspkg-exported-box.md`](../../DESIGN-crosspkg-exported-box.md). G, 2026-10-26, read at
master `d269ef477b`, Go 1.26.10, GOARCH amd64, `-tags purego`. Not a gate.

## Tools

`main.go` and `importers.go` are one program with three modes. Each loads `std` for one GOOS. The first two read the
committed emission under `-core`:

```
go build -o xpbox .
./xpbox -core <tree>/src/core -goos <os>             > owners-<os>.tsv        2> summary
./xpbox -core <tree>/src/core -goos <os> -tests      > owners-tests-<os>.tsv  2> summary
./xpbox -goos <os> -importers                        > importers-<os>.tsv     2> summary
```

- **Owners.** Every exported package-level var, by kind (`pointer`, `interface`, `value:<underlying>`), with:
  - the emitted file of its Go file (`<pkg>/<goos>/<stem>.cs` when the L3 folder holds it, else `<pkg>/<stem>.cs`);
  - whether that file is a whole-file hand-own (the `[module: GoManualConversion]` line, in either spelling);
  - whether it already declares the var's box (`Ꮡ<Name> = new `), and for a hand-own, whether its `.cs.auto` twin does;
  - whether its file carries a `//go:linkname` naming it.
- **Owners, `-tests`.** The same, for vars declared in `_test.go` files.
- **Importers.** Every use of another package's exported non-pointer var in GOROOT sources (generated files are
  skipped), classified as `addr`, `ptrcall`, `methodvalue` or `read`.

`emitprobe/` is the importer-arm measurement of the design's section 2:
- `src/`: a two-package program; Go prints `5 2 3 7 4 true`;
- `master/`: its `lib.cs` and `main.cs` as master `62ae00e43e`'s converter emits them (the converter is unchanged
  since `d269ef477b`);
- `widened/`: the same with `owner-arm.patch` applied, the experiment arm "box every exported non-pointer var of a
  non-`main` package".

Each package was converted alone, with an explicit output directory. The `package_info.cs` files are not kept: a
scratch transpile carries an absolute source path in them.

`behavioral/` is a syntax-only scan of `src/tests/Behavioral` for exported vars outside `package main`
(`go run . <tree>/src/tests/Behavioral`).

## Controls

- **Emission found:** `emitted=no` is 0 for every production var on all three targets. The `-tests` leg misses 2 /
  24 / 12, which are test files no committed `_test.cs` holds.
- **Positive, the hand-own match:** a plain substring match also hit converted files whose placeholder comments
  mention the attribute (12 windows hits; the line-anchored match reads 3). This is the trap recorded as "hand-owned by
  attribute, not string".
- **Positive, `syscall.ForkLock`:** boxed in the compiled emission on linux and darwin; on windows the hand-own has no
  box while its twin does (the latent mismatch in the design's section 3).
- **Positive, the importer census:** it reproduces R's census of 2026-10-25 exactly. The sites whose emission moves are
  the same six, in tests, on all three targets.
