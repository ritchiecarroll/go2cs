# go2cs converter

This directory holds the Go sources of the **go2cs converter** — the program that translates Go
source code into C#. It is written in Go and built on the official `go/ast` + `go/types`
toolchain (`golang.org/x/tools/go/packages`), so it type-checks the code it converts with the
same front end the Go compiler uses.

> The file `readme.go` here is a *component of the converter* — it emits the per-package
> `README.md` (description, license attribution, validation badges) for each converted
> standard-library package. It is not this directory's readme; you are reading that.

## Build

```bash
go build
```

The behavioral and validation harnesses rebuild the converter automatically when any `*.go`
source here is newer than the binary; `go test ./...` runs the converter's own test suite,
including the shared-project registration and hand-own routing integrity gates.

## Usage

```bash
go2cs [options] <input_dir> [output_dir]
```

See [`main.go`](main.go) for the authoritative flag set (`-stdlib`, `-recurse`, `-tests`,
`-platforms`, `-go2cspath`, …) and the repository's
[`CLAUDE.md`](../../CLAUDE.md) / [`docs/Architecture.md`](../../docs/Architecture.md) for how
each mode is used in practice.

## Layout

- `main.go` — entry point and flag handling; `stdLibConverter.go` — the standard-library
  conversion driver (dependency graph, conversion queue, multi-platform emission).
- `visit*.go` — AST statement/declaration visitors (functions, range, defer, select, …).
- `conv*.go` — expression and type conversion (calls, slices, pointers, composite literals, …).
- Analysis passes — escape analysis, variable shadowing, name collisions, generic constraints,
  imports (`*Operations.go`).
- `testConversion*.go` — the Phase-4 `-tests` pipeline: converts a package's `_test.go` suite,
  builds the runnable test host, and differentially compares results against `go test -json`.

The conversion strategy — how each Go construct maps to C# and why — is documented in
[`docs/ConversionStrategies.md`](../../docs/ConversionStrategies.md) (summary) and
[`docs/ConversionStrategies-Reference.md`](../../docs/ConversionStrategies-Reference.md)
(exhaustive reference).

## Licensing and source provenance

The converter is AGPL-3.0-only with the go2cs Converter Output Exception
([LICENSE-EXCEPTION](LICENSE-EXCEPTION)), an additional permission under AGPL
section 7 that every converter source header refers to. Alternative commercial
licensing is available from the copyright holder listed in [AUTHORS](../../AUTHORS),
at the contact address given there. See [LICENSING.md](../../LICENSING.md). Generated
output does not inherit AGPL merely by being generated, and the exception grants
MIT for the templates and scaffolding the converter reproduces in output.
Converted standard-library packages carry the upstream BSD license, in their README
and in the packed license file alike.

`-provenance` adds a deterministic `// Converted from Go source: "..."` comment.
It defaults to **off** and never includes a conversion timestamp. Paths are relative
to GOROOT or the main module when known, otherwise only the filename is emitted.
Recognized leading copyright/license notices are retained even with `-comments` off.

`-license "MIT"` (or another NuGet-supported SPDX expression) overrides package
license metadata without changing the input source's licensing. Without it, a local
`LICENSE` is used if present; standard-library packages otherwise reference the
shared upstream license. Unknown licenses produce a conversion warning, never a
guessed license. NuGet validates the expression when packing.

Local `LICENSE`, `NOTICE` and `AUTHORS` files are optionally packed when present.
Conversion never authors license text. Standard-library packages pack their root
BSD license by relative path, including for project-owned handwritten additions;
the third-party dependency modules of a `-recurse` conversion get their own
module's license file copied verbatim to the converted module root, once per
module, and pack it the same way. The application's own module, and a dependency
that ships no license file, are reported once per module on stderr and left
unspecified. Application projects resolve the same metadata without a warning.
