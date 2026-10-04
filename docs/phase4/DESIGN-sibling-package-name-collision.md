# DESIGN — sibling packages that share one package name

> **Status: design note for a ruling, not a cut.** Drafted 2026-10-03 by lane C2 from the go-cmp Target Atlas
> reading (class G), at the coordinator's request. It changes no converter code. Figures were measured at
> `8f46a9adae` (Go 1.24.13 local, linux, Microsoft .NET 10.0.112).

## The finding

go-cmp v0.7.0's tests import `cmp/internal/teststructs/foo1` and `cmp/internal/teststructs/foo2`. Both directories
declare `package foo`, and both convert to the same C# type:

```csharp
namespace go.github.com.google.go_cmp.cmp.@internal.teststructs;
partial class foo_package { … }
```

One test project references both assemblies, so every use of either is CS0433 ("the type 'foo_package' exists in
both …"). Go has no such problem: a package's identity is its import path, and the two paths differ.

## The naming it meets

[An importer spells the package class from the package NAME](../ConversionStrategies-Reference/package-conversion.md#an-importer-spells-the-package-class-from-the-package-name--the-standard-library-included):
`convertImportPathToNamespace` builds the namespace from the import path with its last segment removed, and the class
from the package NAME (`<name>_package`). A `/vN` tail is handled by its own branch (`math/rand/v2` → `go.math.rand`,
class `rand_package`). Producer and consumer each derive the same spelling from the import path and the package name,
with no registry, which is why the two sides always agree.

The rule loses the last path segment exactly when the package name differs from it. Two such packages under one
parent, sharing a name, then have the same fully qualified type.

## Census

Collision key = (parent import path, package name). Atlas modules present in the module cache (latest version
each) and GOROOT on linux, windows and darwin. Main packages are excluded: each emits `namespace go;` and
`main_package`, is its own executable, and is never referenced by another assembly.

| Population | Non-main packages | Colliding pairs | Co-referenced in one compilation |
|---|---|---|---|
| BurntSushi/toml v1.6.0 | 5 | 0 | — |
| google/go-cmp v0.7.0 | 10 | 1: `teststructs/{foo1,foo2}` → `foo` | YES: cmp's tests (CS0433 measured) |
| google/uuid v1.6.0 | 1 | 0 | — |
| golang.org/x/sync v0.19.0 | 4 | 0 | — |
| golang.org/x/tools v0.42.0 | 163 | 1: `internal/{jsonrpc2,jsonrpc2_v2}` → `jsonrpc2` | no package in the module (tests included) reaches both: latent |
| gopkg.in/yaml.v3 v3.0.1 | 1 | 0 | — |
| GOROOT std, three targets | 344–346 | 0 | — |
| GOROOT cmd | — | 1: `cmd/cgo/internal/{cgotest,test}` → `cgotest` | `cmd/...` is not converted |

The population any fix TOUCHES is wider than the colliding pairs: every package whose name differs from its last
segment. Measured:

| Population | Name differs from tail |
|---|---|
| toml | `internal/toml-test` → `tomltest` |
| go-cmp | `teststructs/foo1`, `teststructs/foo2` → `foo` |
| x/tools | `internal/jsonrpc2_v2` → `jsonrpc2`; `cmd/signature-fuzzer/internal/fuzz-generator` → `generator` |
| yaml.v3 | `gopkg.in/yaml.v3` → `yaml` (a `name.vN` tail) |
| std, all targets | `crypto/internal/fips140deps` → `fipsdeps`, `crypto/internal/fips140test` → `fipstest`, `internal/trace/internal/testgen/go122` → `testkit`, `runtime/internal/wasitest` → `wasi`; `math/rand/v2` → `rand` (`/vN`) |
| std, darwin | `crypto/x509/internal/macos` → `macOS` (case only) |

None of the four std `DIFFERENT` packages is imported by a std production package. The record's "exactly four" (the
package-conversion.md census) predates Go 1.24: `fips140deps` and `fipstest` are new, so that table is stale by two
rows whatever is ruled here.

## Options (each derivable from the import path and package name alone)

1. **Keep the directory in the NAMESPACE when the name differs from the tail.** Namespace = the full import path;
   class = `<name>_package`. `foo1` → `go.…teststructs.foo1.foo_package`, `foo2` → `go.…teststructs.foo2.foo_package`.
   `/vN` and `name.vN` tails keep today's branch, so `yaml.v3` and `math/rand/v2` do not move. *Reader:* the
   producer's `namespace` line and the consumer's `using foo = …foo1.foo_package;` alias name the directory; the
   code that uses the package still reads `foo.X`, as in Go. *Moves:* the mismatched packages' own namespace lines
   and their importers' alias lines — in std, four packages with no std production importer, plus darwin's `macOS`
   and its importer unless a case-only difference is exempted.
2. **Keep the directory in the CLASS name when the name differs from the tail.** Namespace unchanged; class =
   `<name>ꓸ<tail>_package` (e.g. `fooꓸfoo1_package`). *Reader:* the declaration and the alias carry the compound
   name; use sites still read `foo.X`. *Moves:* the same population as option 1, but every reflection or
   diagnostic path that reconstructs a package from its class name (`GoPackage`, `*_package` suffix parsing) must
   learn the compound form, which option 1 avoids.
3. **Disambiguate only on an actual collision.** Smallest footprint, but the consumer cannot know from an import path
   alone that a sibling with the same name exists; it needs a directory scan of the parent, which a partial module
   view (a vendored or trimmed tree) answers differently on each side. Rejected by the no-registry constraint.

## Recommendation

Option 1, with the `/vN` and `name.vN` exemptions and the case-only question ruled explicitly. It keeps the
package's Go identity (its full path) in the C# namespace, changes nothing a Go reader sees at a use site, and its
population is small and fully listed above. A cut would size the importer footprint with the two-seeded emission on
three targets and CNR, and refresh the package-conversion.md table either way.

## RULED 2026-10-04 — option 1, cut in seat `claude/c2-sibling-package-name`

> Amendment, dated. Everything above is the note as written for the ruling and is unchanged.

**Ruling (coordinator, 2026-10-04):** option 1 — when a package's name differs from the last segment of its import path,
the namespace keeps the whole import path and the class stays `<name>_package`. Option 2 declined (a compound class name
would have to be taught to every path that parses `*_package`); option 3 stays rejected (no registry, no directory
scan). Exempt and unmoved: a `/vN` tail, a `name.vN` tail, a case-only difference (Go's module rules forbid two paths
in one module that differ only by case). The cut also leaves `package main` unmoved and decides an external test package
by the package it tests (`foo_test` → `foo`); a hyphenated directory is not exempt (`github.com/mattn/go-isatty` →
`go.github.com.mattn.go_isatty.isatty_package`).

**What the cut found that the note did not say:**

- A persisted `package_info.cs` is copied through verbatim outside its marker sections, so its own `namespace` line and
  the template's `using static <namespace>.<class>;` line would have kept the old namespace. `convergePackageNamespace`
  moves both; a converged file is byte-identical to a fresh one.
- The alias-collision rename (`computeImportAliasRenames`) reads a child-namespace map built from each closure path's
  parent segments. A package that keeps its directory declares one level deeper, so the map records `<parent>.<tail>`
  for it too; otherwise `import foo1 ".../teststructs/foo1"` from a package declared in `<parent>.teststructs` emits an
  unrenamed `using foo1 = …` (CS0576). No std file moved for this.
- The rename follows the namespace in the other direction as well: `crypto/internal/fips140test` sat directly in
  `go.crypto.@internal`, whose child namespace `fips140` forced `using Δfips140 = …`. In
  `go.crypto.@internal.fips140test` it does not collide (the converter emits the alias inside the namespace
  declaration, where it binds before any enclosing namespace's member), so the plain `fips140` returns: 51 lines in 5
  test files. Measured with a two-arm C# probe: alias inside a namespace whose PARENT has child `fips140`, rc 0; inside
  the namespace that HAS child `fips140`, CS0576.

**Footprint, measured** (prediction `3e08661784f4a39e`, posted before either emission; base master `54f7f4439d`):

| Arm | Predicted | Measured |
|---|---|---|
| `-stdlib`, three targets | 6 files, 8/8, flat | 6 files, 8/8, flat — met |
| `-tests` `fips140deps` | 4 files, 8/8 | met |
| `-tests` `wasitest` | 5 files, 6/6 | met (plus `nonblock_test.cs`, linux-only, not committed) |
| `-tests` `fips140test` | 15 files, 16/16 | 16/16 met, plus the 51-line `Δfips140` → `fips140` rename (falsifier fired, scored above) |

| Behavioral (CNR) | `SiblingPackageNames` only | missed: two committed tests also move (below) |

Committed in-seat: 30 corpus files, 89/89. darwin's `crypto/x509/internal/macos` and `math/rand/v2` unmoved. The two moved
production projects build clean on linux, and the `fips140deps` and `fips140test` test assemblies compile (their only
build errors are MSB3030 copies of `.go` sources the repository tree does not carry).

**Behavioral, which the census above did not cover:** a module-ROOT library is a package whose name can differ from its
directory too. `CrossPkgSameNameAlias` (`package atomic`) moves to `go.CrossPkgSameNameAlias`, its guarded shape
intact. `AliasNamespaceShadow/sortlocal` (`package sort`) moves to `go.AliasNamespaceShadow.sortlocal`, so its class no
longer shadows `inner`'s alias for the standard library's `sort`: the test still passes, but it no longer produces the
shape it was written to guard. CNR at the seat tip then equals CNR at the base (one pre-existing mover, byte-identical).

> **Amended 2026-10-04 (later the same day):** `AliasNamespaceShadow` is restored in-seat, as ruled. Its `sortlocal/`
> directory is now `sort/` (name == directory, so it stays in `go.AliasNamespaceShadow`) and the shadowing is produced
> again; with `rootQualifyIfAmbiguous`'s class half removed the test fails Compile with CS0117 (`'sort_package' does not
> contain a definition for 'Ints'`), and it passes with it restored byte-identical.
