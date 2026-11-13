# DESIGN: assembly identity — every converted standard-library assembly takes its package ID (OQ4)

<!-- R, 2026-11-11. Owner ruling: docs/PLAN-public-surface-stability.md OQ4 (claude/coord-public-surface-plan 530fd3b1bc,
     RULED), COORD's dispatch inbox/R/20261111T154456Z. Census at master 742a3dbb24: docs/phase4/data/oq4-census/
     (census.sh, census-742a3dbb24.txt, classified.txt). This note is the design and census COORD asked for BEFORE any
     cut; nothing in it is built. -->

## 1. The ruling and what it means here

Package `go.fmt` ships `fmt.dll` today; after this seat it ships `go.fmt.dll`. Simple names like `net.dll`, `log.dll`,
`errors.dll` and `testing.dll` are the ones most likely to clash in a consumer's bin folder, and renaming later breaks
every compiled reference, so the rename is taken now, before outside use, as its own batch.

Today every one of the 604 projects under `src/core` sets `<AssemblyName>` explicitly: 362 library projects (the
dotted import path, e.g. `crypto.tls`) and 242 test projects (`<name>.tests`). The standard library's package ID is
DERIVED from it: `<PackageId>go.$(AssemblyName)</PackageId>`. So the rename is two lines per library project, not one:
the assembly name gains `go.`, and the package ID stops adding it.

## 2. The rule, stated exactly

1. **A standard-library LIBRARY project's AssemblyName is its package ID**: `go.` + the dotted import path
   (`go.fmt`, `go.crypto.tls`, `go.vendor.golang.org.x.text.transform`). Its package ID becomes `$(AssemblyName)`, so
   the two cannot disagree. Package IDs do not change at all.
2. **The `go/*` packages follow the rule literally**: `go/ast` is assembly `go.ast` today, package `go.go.ast`; it
   becomes assembly `go.go.ast`. Odd-looking, and correct: the assembly name equals the package ID, which is the rule.
3. **Test assemblies follow their production assembly**: `go.fmt.tests`. They must: every production project grants
   `InternalsVisibleTo $(AssemblyName).tests` (194 projects), so a test assembly that kept `fmt.tests` would lose
   white-box access. Test assemblies are not shipped, so this costs nothing outside the repository.
4. **Executables are unchanged.** A Go `main` package's assembly is named after the LAST import-path segment, as
   `go build` names its binary (projectFileWriter's Exe rule); no exe ships in a go.* package.
5. **Hand-owned libraries** take the same rule by hand: `unsafe` (`go.unsafe`). `testing` ships today as assembly
   `testing_package` with an explicit package ID `go.testing`; under the rule it becomes `go.testing` (see OQ-B).

## 3. Census: every simple-name dependency

`docs/phase4/data/oq4-census/census.sh` greps the tree by category; `classified.txt` classifies every hit in the large
categories, each into exactly one class (RENAME, GOLIB-GEN, EXE-OR-TEST, GENERIC), with the class counts summing to
the section's hit count. The ones that must change:

| Where | What keys on the simple name | Change |
|---|---|---|
| converter: `csproj-template.xml`, `projectFileWriter.go` | the `<AssemblyName>` line and `<PackageId>go.$(AssemblyName)</PackageId>` | stdlib library: AssemblyName `go.<path>`, PackageId `$(AssemblyName)`; Exe rule unchanged |
| converter: `test-csproj-template.xml` | the test assembly name | `<production AssemblyName>.tests` |
| converter: `testConversion.go` publishBuildOutput | assumes the test assembly is named after the test project FILE | read the declared `<AssemblyName>` (declaredAssemblyName already exists) |
| golib `ж.cs` | `InternalsVisibleTo("unsafe")`, `("runtime")` | `go.unsafe`, `go.runtime` (else golib's internals stop compiling there) |
| golib `GoMemProfile.cs` | `PprofAssemblyName = "runtime.pprof"`, `Type.GetType("go.runtime.pprof_package, runtime.pprof")` | `go.runtime.pprof`. SILENT otherwise: `throwOnError: false` returns null |
| GolibTests `ChattyFlagBridgeTests`, `LazyCallersTests`, `MemProfileReachabilityTests` | `Contains("flag")`, `"sort"`, `sort.pdb`, closure lists of `runtime.dll`/`fmt.dll` | the new names. Two would go VACUOUS (green while proving nothing), not red |
| release tooling: `release-smoke.ps1`, `test-rid-compile-asset.ps1` | `Dll = 'runtime.debug.dll'`, `lib/net10.0/syscall.dll` | the new names (these fail loud) |
| ApiCompat guard: `GoApiCompat.psm1`, `push-nuget.ps1` | compares each baseline asset with the SAME path in the current package | see section 4 |
| comments and diagrams: `go.lib.targets`, `push-nuget.ps1` (layout L3) | `syscall.dll`, `os.dll` | the new names |
| live docs (3) | e.g. "go.fmt ships fmt.dll" | the new names; plans and point-in-time records are never rewritten |

Name-agnostic, no change (read, not assumed): the runtime's PDB lookup and symbol-file census (they use the loaded
assembly's own name); the test host's referenced-assembly walk; ProjectReferences (by path); `-recurse=nuget`'s
package IDs (`go.` + the csproj base name, which does not change); the 436 `go2cs.SynthesizedStructs` grants (golib's
dynamic assembly, a fixed literal).

## 4. Compatibility: the break, and how the guard sees it

An assembly rename is a binary break: anything compiled against `fmt.dll` looks for an assembly named `fmt`. Two
mechanisms already exist for a deliberate .4 break, and the rename rides both:
- **The break floor.** P1's method-cap seat already set `<GoBreakFloor>1.26.10.4</GoBreakFloor>` in go.lib.targets
  for .4. A consumer package built on an older release then fails at BUILD time with GO2CS3002 naming itself, instead
  of a FileNotFoundException at run time. No floor move is needed.
- **The ApiCompat guard.** It compares each baseline asset with the asset at the SAME path, so `lib/net10.0/fmt.dll`
  reads as "missing from the current package", a failure no CompatibilitySuppressions.xml entry can announce.
  PROPOSED: give the guard a rename map derived from the rule (baseline `<path>/<name>.dll` -> current
  `<path>/go.<name>.dll` for a go.* package), so each renamed assembly's API is still compared, and only the identity
  change is the announced break. One suppression-shaped announcement per package, or a single guard-level declaration
  that release N renamed the std assemblies (OQ-D). Its own red-first: a dropped member in a renamed assembly must still
  read red.
- **Release notes**: an "Upgrading" item in the .4 NEWS draft (rebuild against .4; the floor names any stale package).
  The nugetgo packages are rebuilt in the .4 wave anyway, since the floor already forces it.

## 5. Predictions (for the cut, stated now)

- **Corpus footprint (two-seeded, four targets):** every std library project's `.csproj` changes its AssemblyName and
  PackageId lines; every test project's `.csproj` changes its AssemblyName line (and its reference to the production
  assembly, if that names it). No `.cs` file is predicted to move; a `.cs` move is a finding to explain, not to absorb.
  The exact per-target file counts come from the census at the cut's base, before the run.
- **CNR:** 0 changed. Behavioral projects are Exes, or non-std libraries, whose names the rule does not touch. The
  falsifier: any behavioral `.csproj` moving.
- **ApiCompat:** red as a plain run (every std asset renamed); green with the rename map, the renamed assemblies' APIs
  compared, and red again on a planted removed member.
- **release-smoke on 4 RIDs:** green only with the tooling rows above changed; that is the leg most likely to red
  for a non-mechanism reason (a hard-coded `.dll` the census missed).

## 6. Open questions for COORD

- **OQ-A golib and go.gen.** golib ships as `golib.dll` in package `go.lib`; the analyzer as `go2cs-gen.dll` in
  `go.gen`. Neither is a converted assembly, and neither name is clash-prone. Recommend: keep both, and state it on
  the PublicSurface page. Renaming golib to `go.lib.dll` would be consistent, but it touches every friend grant to
  golib and every test of it, for no clash it prevents.
- **OQ-B testing.** `testing_package` becomes `go.testing` under the rule. Recommend yes: it is the clash-prone name
  the ruling is about.
- **OQ-C converted modules (-recurse, nugetgo).** A module's library assemblies already carry their full dotted
  import path (`github.com.fatih.color`), which does not clash. nugetgo mints ONE package per MODULE
  (`nugetgo.<module path>`) carrying several assemblies, so "assembly name = package ID" cannot hold one-to-one there.
  Recommend: the rule is the standard library's; module assemblies keep their full dotted paths.
- **OQ-D the guard's declaration.** A rename map plus one suppression-shaped entry per package, or one guard-level
  declaration for the release. Recommend the map plus a guard-level declaration, so 360 near-identical entries do not
  fill the suppression files.

## 7. Order of work (on the ruling)

1. Red first: a converter test that a std library project's AssemblyName equals its package ID; a GolibTests guard on
   GoMemProfile's pprof lookup by the new name. Both red at the base.
2. Converter: the template and projectFileWriter rule, the test template, publishBuildOutput.
3. Hand-owned: golib's grants and GoMemProfile, unsafe and testing csprojs.
4. The two-seeded four-target reconvert, scored against section 5; apply the corpus delta.
5. Tooling and tests (release-smoke, the RID asset test, the GolibTests guards), docs, the NEWS Upgrading item.
6. The ApiCompat rename map, red-first, after OQ-D.
7. Gates: go test, repoguard, GolibTests D/R, GenTests, CNR, the footprint, ApiCompat, release-smoke on 4 RIDs through
   CI. Its own batch, never stacked with another converter seat.
