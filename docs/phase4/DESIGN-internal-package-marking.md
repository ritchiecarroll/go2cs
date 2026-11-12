# DESIGN: Go `internal` and `vendor` packages are marked and warned on (OQ1)

> **Status: PROPOSED, awaiting COORD's review.** Nothing here is built. Written 2026-11-11 against master
> `742a3dbb24` (worktree branch `claude/r-oq1-design`), from the owner's OQ1 ruling and lane C2's groundwork,
> each claim of which was re-read against the code (section 11 lists what changed). The open questions in section 12
> need answers before the cut.

<!-- Sources. The ruling: docs/PLAN-public-surface-stability.md, OQ1 and the RULING block, on
     claude/coord-public-surface-plan at 530fd3b1bc. The tier page: docs/PublicSurface.md on claude/c1-public-surface
     at d78a1bbb8d, "Tier 3: no promise". The OQ4 note this one must sit beside: docs/phase4/DESIGN-assembly-identity.md
     on claude/r-oq4-assembly-identity at ace36f4775. Every count below was measured read-only at 742a3dbb24 with
     git ls-tree / git grep; the commands are in section 4.4. The published-package evidence in section 5 was read from
     the go.* 1.26.10.2 packages in the local NuGet package cache (their .nuspec files and folder listings), not from a
     build. -->

## 1. The ruling and its scope

The owner ruled OQ1 on 2026-11-11, as recommended: Go `internal` and `vendor` packages **stay public to .NET**, and
they are:

- **marked**: an assembly attribute carrying the internal ROOT path, and `[EditorBrowsable(Never)]` on the package
  class;
- **warned on now**, by a new analyzer diagnostic, **GO2CS0014** (0011 is taken by the `-warn` work, 0012 exists,
  0013 is P1's);
- **documented** as having no compatibility promise, in the words "not covered by any compatibility promise; this
  warning may become an error".

Unlisting the internal packages on nuget.org is to be checked against the publishing workflow and **reported only**
(section 8).

C1's contract page already puts these packages in tier 3 (`docs/PublicSurface.md`, *Tier 3: no promise*). Its
sentence "These can change in any release, and nothing is done to warn about them" becomes untrue when this lands;
the page needs one amendment naming GO2CS0014 (section 12, Q7).

Out of scope: making the packages C# `internal` (option 1, rejected by the ruling), a run-time check (option 3,
rejected), and `cmd/` (the corpus converts no `cmd/` package).

## 2. The rule enforced

Go's own, as `cmd/go/internal/load` states it (`findInternal`, `disallowInternal`, `disallowVendor`):

1. **internal**: find the FINAL `internal` element of the imported package's path. Its ROOT is everything before
   that element (`crypto/internal/fips140/aes` has root `crypto`; `internal/poll` has root "", the standard library's
   root). The import is allowed only from the root or below it.
   - For a package in the standard library, "below" is a DIRECTORY test: below `GOROOT/src`. Root "" therefore admits
     every standard-library package and nothing else.
   - For a package in a module, it is an IMPORT-PATH prefix test (`str.HasPathPrefix`).
2. **vendor**: a package under `vendor/` may be imported only from the parent of `vendor/`. In the standard library
   that parent is `GOROOT/src`, so a std vendored package's root is "" and only the standard library may use it.
   Module-mode vendoring never produces a `vendor/` import path (go/packages reports the original path), so in
   practice this rule applies to the standard library alone.
3. **both**: a package with an `internal` element under `vendor/` takes the tighter root, which is always the
   internal one (`vendor/golang.org/x/crypto/internal/alias` has root `vendor/golang.org/x/crypto`).

The unit Go enforces is the IMPORT. A package that never imports an internal package may still hold and use a value
of one of its types, obtained through a public API, and call its methods. The analyzer mirrors that (section 7.2).

## 3. Who can violate it

**Converted Go code cannot.** go/packages refuses "use of internal package ... not allowed" before go2cs emits
anything, so every converted file of a package uses only what Go allowed it to import.

**Converter output can still NAME a forbidden type without violating Go.** Go lets code use a type it cannot import,
through a type alias or an inferred type; the converter often has to spell such a type out in C#. In the corpus
today no public package exports an alias into an internal one (every `GoTypeAlias` whose target is under `@internal`
is declared in an internal package itself), but nothing guarantees it. So converter output is skipped by the
analyzer, never judged.

**Hand-written C# can**, in three shapes:

| Shape | Example | Who carries the analyzer today |
|---|---|---|
| A pure C# project referencing go.* packages | a console app with `PackageReference go.fmt`, naming `go.@internal.poll_package` | nobody (section 5) |
| A hand-written file in a converted project | `*_impl.cs`, `*_impl_test.cs`, a `[module: GoManualConversion]` file, a user's own partial | go2cs-gen, through the project's own analyzer reference |
| A pure C# project referencing converted PROJECTS | `src/tests/CSharpConsumer` (ProjectReferences to converted modules) | nobody, and this design does not change that (Q4) |

## 4. Marking

### 4.1 The attribute

A new golib attribute, beside `GoPackageAttribute` and following its shape:

```csharp
namespace go;

/// The assembly is a Go internal or vendored package: only Go code at or below Root may import it.
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GoInternalRootAttribute(string root) : Attribute
{
    /// The import path of the root; "" is the standard library's root (GOROOT/src).
    public string Root => root;

    /// The root is the standard library's, tested as Go does: the importer must itself be in the standard library.
    public bool Standard { get; set; }
}
```

`Standard` is needed because root "" (109 of the 185 packages) is a prefix of every path, and because Go tests a
standard-library root by directory, not by path: a module named `crypto/x` is not "below" `crypto`. `{ get; set; }`
follows `GoPackageAttribute.ImportPath`, the existing named-argument precedent.

It is a new public golib type, so C1's golib baseline (`src/core/golib/PublicSurface.golib.txt`, which lists
`go.GoPackageAttribute` and `go.GoHandOwnedPackageAttribute`) gains `go.GoInternalRootAttribute`, in tier 2 with the
other `Go…` attributes.

### 4.2 Where it is emitted

**In the package's `package_info.cs`, by the converter.** That is where every other package-level record lives
(`[GoPackage]`, `GoImplement`, `GoPositionMap`), it survives a user `-csproj` template, and the converter knows the
exact import path (go/packages `PkgPath`). Two lines, for an internal or vendored package only:

```csharp
[assembly: GoInternalRoot("crypto", Standard = true)]
...
namespace go.crypto.@internal.fips140;

[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
[GoPackage("aes")]
public static partial class aes_package
```

- **The attribute line** goes with the other assembly records, regenerated on every conversion (removed and
  rewritten, as the `GoHoistedLiterals` line is), so a package that moves out of `internal/` loses it.
- **The `EditorBrowsable` line goes ABOVE `[GoPackage]`, never between it and the class.** `go.lib.pack.targets`
  reads each package's namespace and class with a regular expression that requires `[GoPackage("…")]` to be followed
  by whitespace and then `public static … partial class`; a line between them makes the pack fail with "declares no
  namespace followed by a [GoPackage] package class" for all 185 packages. Fully qualified, so no Go identifier named
  `ComponentModel` or `EditorBrowsable` can capture it, and no `using` line is added.
- **Test info files are not marked** (`package_info_internal_test.cs`, `package_info_external_test.cs`,
  `package_test_info.cs`). A test assembly is never referenced, and a test project never compiles the production
  `package_info.cs` (its csproj lists compile items explicitly and says why), so no duplicate attribute can arise.
- **The root** is computed by one converter function mirroring section 2 on `PkgPath`, with a table test (internal at
  the start, middle and end of a path; two `internal` elements, the last wins; vendor alone; vendor with internal).

### 4.3 Census at `742a3dbb24`

Of 604 projects under `src/core`, 362 are production projects (242 are `*.tests.csproj`). By the import path of the
project directory:

| Class | Projects |
|---|---|
| an `internal` element only | 168 |
| a `vendor` element only | 15 |
| both | 2 (`vendor/golang.org/x/crypto/internal/{alias,poly1305}`) |
| **marked (internal or vendor)** | **185** (170 internal, 17 vendor) |
| neither | 177 |

The 168 internal-only by top-level directory: `internal/` 97, `crypto/` 51, `net/` 5, `go/` 4, `log/` 4, `reflect/` 2,
and one each under `image/`, `math/`, `os/`, `runtime/`, `testing/`.

Their roots: "" (standard library) 109; `crypto` 48; `net/http` 4; `go` 4; `log/slog` 3; `vendor/golang.org/x/crypto`,
`reflect` and `internal/trace` 2 each; `testing`, `runtime/race`, `os/exec`, `net`, `math/big`, `log`,
`internal/runtime/gc`, `image`, `crypto/x509`, `crypto/tls` and `crypto/internal/sysrand` 1 each.

**Files the marking touches.** No marked package has a hand-owned `package_info.cs`. 155 packages have one root copy;
the other 30 are layout-L3 packages with 69 copies in key folders (windows 20, darwin 18, linux 15, linux-arm64 7,
amd64 6, linux-amd64 3). **224 files in all.**

**Per target** (one copy compiles per package per target; a package with no copy for a target is not built there):

| Target | Root copies | Key-folder copies | Marked packages compiled |
|---|---|---|---|
| windows/amd64 | 155 | 26 (windows 20, amd64 6) | 181 |
| linux/amd64 | 155 | 24 (linux 15, amd64 6, linux-amd64 3) | 179 |
| darwin/amd64 | 155 | 24 (darwin 18, amd64 6) | 179 |
| linux/arm64 | 155 | 22 (linux 15, linux-arm64 7) | 177 |

That assumes every root-copy package is built on all four targets: none of the 155 carries `GoPlatformExclusive`
(read), but the per-target build lists were not run.

The behavioral corpus has one marked package: `RootedUsingCapture/internal/sync` (module `RootedUsingCapture`, root
`RootedUsingCapture`, `Standard` false), whose converted sibling `lib` uses it.

### 4.4 The census commands

Run from the worktree at `742a3dbb24`, Git Bash:

```bash
git ls-tree -r --name-only 742a3dbb24 -- src/core | grep '\.csproj$' > all.txt              # 604
grep -v '\.tests\.csproj$' all.txt > prod.txt                                               # 362
sed 's#^src/core/##; s#/[^/]*\.csproj$##' prod.txt > dirs.txt
awk '{n=split($0,a,"/"); i=0; v=0; for(k=1;k<=n;k++){if(a[k]=="internal")i=1; if(a[k]=="vendor")v=1}
     if(i&&v)b++; else if(i)io++; else if(v)vo++; else p++}
     END{print "internal-only",io,"vendor-only",vo,"both",b,"neither",p}' dirs.txt   # 168 15 2 177
grep -E '(^|/)(internal|vendor)(/|$)' dirs.txt > marked.txt                                 # 185
git ls-tree -r --name-only 742a3dbb24 -- src/core | grep '/package_info\.cs$' | sed 's#^src/core/##' > pi.txt
# per marked dir: the root copy if pi.txt has <dir>/package_info.cs, else each
# <dir>/<key>/package_info.cs for key in windows linux darwin amd64 arm64 windows-amd64 linux-amd64 darwin-amd64 linux-arm64
# (the go.lib.pack.targets rule: key folders are read only when the root has no copy)          -> 155 root + 69 key = 224
```

## 5. Delivery: does a pure C# consumer receive go2cs-gen?

**No.** The evidence, end to end:

1. **The corpus template references go2cs-gen privately.** `src/go2cs/csproj-template.xml` carries
   `<ProjectReference Include="$(go2csPath)gen/go2cs-gen/go2cs-gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" PrivateAssets="All" />`.
   Under `-recurse=nuget`, `src/go2cs/projectFileWriter.go` swaps it for
   `<PackageReference Include="go.gen" Version="$(GoStdLibVersion)" PrivateAssets="all" />`. Both forms are private.
2. **go.gen is not a dependency of any go.* package.** A project reference with `ReferenceOutputAssembly="false"` and
   `PrivateAssets="All"` is not written as a NuGet dependency. In the published 1.26.10.2 packages in the local cache,
   the only `.nuspec` that names `go.gen` is go.gen's own. go.gen itself is `DevelopmentDependency=true` with its DLL
   under `analyzers/dotnet/cs` only (`src/gen/go2cs-gen/go2cs-gen.csproj`).
3. **And even a dependency's analyzers would not flow.** Every dependency a go.* package declares is packed with
   NuGet's default private assets. go.fmt 1.26.10.2's nuspec, for example:
   `<dependency id="go.lib" version="[1.26.10.2]" exclude="Build,Analyzers" />`, and the same shape for each of its 11
   go.* dependencies (the exact `[x]` pin is `GoExactProjectReferenceVersions` in `go.lib.pack.targets`). The
   `exclude` attribute is NuGet's default, not a line in the tree, so it is evidenced from the packed nuspec only.
4. **What does flow is go.lib's `buildTransitive/`.** NuGet's `Build` exclusion covers `build/`, not
   `buildTransitive/`. `src/core/golib/golib.csproj` packs `buildTransitive/go.lib.targets` (with
   `go.lib.symbols.targets` and `go.lib.pack.targets`) and says why: "Every converted package depends on go.lib, so
   buildTransitive reaches every consumer." The published go.lib 1.26.10.2 holds exactly those three files under
   `buildTransitive/`, and nothing under `analyzers/`. The break floor (GO2CS3002) and the consumer usings already
   reach pure C# consumers this way.
5. **A project that references golib as a PROJECT never imports `go.lib.targets`** (its own comment: "A project
   reference to golib never imports buildTransitive, so the corpus and the -tests hosts are untouched"). So the corpus,
   the behavioral packages and `src/tests/CSharpConsumer` see neither carrier.

**So the check is carried twice**, by one analyzer source compiled into two assemblies:

| Consumer | Carrier |
|---|---|
| a converted project (corpus, behavioral, `-recurse` local projects) | go2cs-gen, which every converted project already loads; no csproj line changes |
| a pure C# project with go.* packages | a small analyzer DLL packed in go.lib, added as an `<Analyzer>` item by `go.lib.targets` |
| a `-recurse=nuget` converted module (it gets go.gen AND go.lib's buildTransitive) | go2cs-gen; go.lib's item is withheld (below) |

**Why a separate DLL**, not go2cs-gen itself for pure consumers: go2cs-gen's generators and analyzers would run over
hand-written C#. `StringLiteralBindingAnalyzer` raises GO2CS0004/0005, which are ERRORS, on a C# literal reaching an
`object` slot of a converted call, a thing a C# consumer does routinely. The small DLL holds GO2CS0014 and nothing
else.

**The small DLL.** A new analyzer project under `src/gen/` (netstandard2.0, Microsoft.CodeAnalysis 4.14, the floor
GO2CS3001 already enforces for go.lib consumers), compiling the analyzer file, `GeneratorDiagnostics.cs` (the
diagnostic registry guard requires every go2cs-gen descriptor to live there) and the shared helpers it needs, as
linked files. golib.csproj builds it (`ReferenceOutputAssembly="false"`) and packs it under `buildTransitive/`, NOT
under `analyzers/`: a direct go.lib reference (every `-recurse=nuget` module has one) would load anything under
`analyzers/` automatically, beside go2cs-gen's copy. The ApiCompat guard ignores it (its asset pattern is `lib/` and
`runtimes/*/lib/` only, in `GoApiCompat.psm1`).

**Withholding it where go2cs-gen runs.** A target in `go.lib.targets`, `BeforeTargets="CoreCompile"` (which also runs
in design-time builds, with compilation skipped), adds the item only when no `@(Analyzer)` already has the file name
`go2cs-gen`, whether it came from the go.gen package or from a project reference. `GoInternalPackageWarning=false`
turns the item off, for a consumer who prefers that to a NoWarn.

## 6. The consumer's Go path

To answer "is the consumer at or below root R?", the analyzer needs the consuming compilation's Go import path and
whether it is in the standard library.

### 6.1 The path is already recorded, losslessly

C2's groundwork said assemblies record no import path. **They do.** Every converted package class carries
`[GoPackage("<name>")]` in its namespace, and the converter adds `ImportPath = "<path>"` exactly where the
namespace-plus-name decode would be wrong (`goPackageImportPathStamp` in `src/go2cs/packageInfoWriter.go`: a dot inside
a segment, a major-version directory, a package name that differs from its directory, a sanitized segment). golib's
`reflect` PkgPath, frame names and synthesized containers already decode it this way. 25 std `package_info.cs` files
carry the stamp at `742a3dbb24`, e.g. `[GoPackage("chacha20", ImportPath = "vendor/golang.org/x/crypto/chacha20")]`
and `[GoPackage("rand", ImportPath = "math/rand/v2")]`; `internal/poll` decodes from `namespace go.@internal;` plus
`poll`, and `RootedUsingCapture/internal/sync` from `namespace go.RootedUsingCapture.@internal;` plus `sync`.

### 6.2 The options

| Option | Consumer path from | Corpus footprint | Behavioral (CNR) footprint |
|---|---|---|---|
| (a) C2's: `<GoImportPath>` in every converted csproj, read as `build_property.GoImportPath` | a new csproj line (plus a `CompilerVisibleProperty` item) | 360 converted std library csproj + 242 test csproj | every behavioral `.csproj` moves (1,039 tracked) |
| (b) `[assembly: GoImportPath]` in every `package_info.cs` | a new record | 428 production info files, plus test info files | every behavioral `package_info.cs` moves (1,037 tracked) |
| **(c) decode the compilation's own `[GoPackage]` class** | the existing stamp | **none** | **none** |

**Recommended: (c).** It needs no new emission, and its correctness is the converter's existing invariant (a stamp
wherever the decode is lossy), which golib's run-time decoders already depend on.

- The analyzer reads the `[GoPackage]` classes declared in the compilation's own SOURCE. A trailing `_test` on the
  package name of an external test package is dropped (`testing_test` decodes to `testing`), so a test compilation's
  internal and external packages agree. No `[GoPackage]` class, or classes that disagree, means hand-written C#: no
  path, so every root is forbidden.
- The decode is ten lines and mirrors `goPackageImportPathStamp` (the `@` escape removed, `.` read as `/` below the
  `go` root, then `/` and the name). Its guard is a GenTests table over the converter's own cases: `example.com/x`,
  `vendor/golang.org/x/...`, `math/rand/v2`, `go.@internal`.
- C2's (a) needed a `CompilerVisibleProperty` declared by golib's buildTransitive. That would not have reached the
  corpus or the behavioral packages at all (section 5, point 5); it would have had to go in the csproj template too.
  (c) has no such seam.

### 6.3 Standard-library membership: one hand-owned property

The path alone cannot say "standard library": behavioral modules are named `RootedUsingCapture`, with no dot, so a
dotless path proves nothing. **`src/core/Directory.Build.props` sets `<GoStandardLibrary>true</GoStandardLibrary>`
and declares `<CompilerVisibleProperty Include="GoStandardLibrary" />`**, beside the three `CompilerVisibleProperty`
items it already declares for the escape registry. Every standard-library project imports that file; a `-stdlib`
reconvert into a seeded temp root carries it; no converted file changes. Two lines, hand-owned.

~~A consumer who sets `GoStandardLibrary=true` in its own project has opted out on purpose, exactly as a NoWarn would.~~
(Struck by the 2026-11-12 amendment, S4: go.lib's targets declare no `CompilerVisibleProperty`, so for a pure consumer
the property never reaches the analyzer.)

## 7. The analyzer

### 7.1 What it decides, once per compilation

At compilation start:

1. `consumerStd` = `build_property.GoStandardLibrary` is `true`; `consumerPath` = the decode of section 6.2, or none.
2. For every referenced assembly carrying `GoInternalRoot(R, Standard = s)`, the use is ALLOWED when `consumerPath`
   exists, the standard-library flags agree (`s` requires `consumerStd`; a module root requires its absence), and
   `consumerPath` is `R`, or starts with `R/`, or `R` is "".
3. The FORBIDDEN set is every marked reference not allowed. **When it is empty, nothing more is registered**, which is
   the case for most standard-library compilations, so the stdlib build pays almost nothing.

### 7.2 What it reports

In each tree it checks (7.3), a NAME that binds a type declared in a forbidden assembly: the package class itself
(`poll_package.Foo()`), a Go type (`poll.FD`, `typeof(…)`, `new …`), a `using static` or alias target, and a use of
an alias whose target is such a type (`poll.Foo()` after `using poll = go.@internal.poll_package;`, including an alias
from a generated global-usings file, reported at the use).

It does NOT report a namespace `using` (a namespace such as `go.@internal` spans many assemblies), nor a member reached
through a value (`x.Method()` on a value of an internal type, which binds a method, not a type), nor an inferred type
(`var`). Those are the uses Go allows without an import.

**One diagnostic per (tree, forbidden assembly)**, at the first naming use: the analogue of Go's one error per
import.

| Code | Severity | Raised when | Message |
|---|---|---|---|
| `GO2CS0014` | Warning | Hand-written C# names a type of a Go `internal` or `vendor` package from outside that package's root. | `'{0}' is in Go package '{1}', which only Go code under '{2}' may import. Go internal and vendored packages are not covered by any compatibility promise; this warning may become an error.` (`{2}` reads "the standard library" for root "") |

Its `docs/DiagnosticCodes.md` row, *What to do*: "Use a public package instead. A Go internal or vendored package
changes without notice between Go releases. In a converted project, move the code into a package under the root."
The descriptor goes in `GeneratorDiagnostics.cs`, which `TestGo2csDiagnosticCodeRegistry` holds to one registration,
one raise and one row.

### 7.3 Which trees it checks

**Recommended: go2cs-gen's existing rule** (`Common.IsConvertedSourceFile`), with the metadata files counted as
converter output. Checked: `*_impl.cs`, `*_impl_test.cs`, a file carrying `[module: GoManualConversion]`, and every tree
of a compilation that declares no `[GoPackage]` class. Skipped: every other file of a converted project
(`package_info.cs`, `package_init.cs`, the test info files and host, converted Go files) and generated code
(`GeneratedCodeAnalysisFlags.None`).

The gap: a user's own partial in a converted project, named anything but `*_impl.cs`, counts as converter output and
is not checked. A stricter rule exists: "converter output is a file a `GoPositionMap` record names, or a metadata
file", since the converter writes one record per converted file and none for hand-written ones (read: no record for
`internal/poll`'s `*_impl.cs` files or its `GoManualConversion` file `linux/fd_writev_unix.cs`). It closes the gap at
the price of a second predicate beside the one the generators use (Q3).

## 8. Unlisting on nuget.org: the report

**Feasible, with three preconditions; not recommended now.**

- **The mechanism exists.** On nuget.org a package DELETE unlists (`dotnet nuget delete <id> <version>`, or a DELETE to
  the publish endpoint), one call per ID per VERSION. Unlisted versions still restore by exact version, and every
  dependency among go.* packages is an exact pin (`[x]`), so no consumer restore breaks. The flat container, which
  `_nugetpush.ps1` and the post-publish restore check read, lists unlisted versions too.
- **Nothing in the workflow unlists today.** `src/push-nuget.ps1`, `src/_nugetpush.ps1` and `src/release-nuget.ps1`
  mention unlisting only in their "can be unlisted, never deleted" warnings.
- **Precondition 1, doctrine.** `docs/GoCorpusMigration.md` rules for removed packages: "deprecate, with a pointer to
  the last release that carried it. Never unlist." It was written about removed IDs, but it reads as absolute; adopting
  unlisting needs an owner ruling that scopes it.
- **Precondition 2, the key.** nuget.org grants unlisting as its own API-key scope. Whether the publishing key has it
  was not checked (an owner fact).
- **Precondition 3, the step.** A Phase 3b in `release-nuget.ps1` after the push (and a resume for it), reusing
  `_nugetpush.ps1`'s endpoint discovery and Retry-After handling: the push quota already stopped one release at push
  351 of 362, and this adds 185 calls per release.
- **The ID set is not `go.internal.*`.** Package IDs are `go.` plus the dotted path, so `go.internal.*` covers only
  the 97 top-level `internal/` packages. The other 88 are `go.crypto.internal.*`, `go.net.http.internal.*` and the
  like (71), and `go.vendor.*` (17). The list should come from the marking (`GoInternalRoot` in the packed assembly),
  never from a prefix.
- **Why not now.** It removes search clutter only: nobody adds these packages on purpose, and the analyzer and
  `EditorBrowsable` already cover the consumer who does. It costs an irreversible-feeling step per release and a
  doctrine change. Recommend revisiting once the analyzer has shipped one release (Q6).

Converted modules need nothing: nugetgo mints one package per MODULE, so a module's internal packages are never
separate IDs.

## 9. Interaction with OQ4 (assembly identity)

OQ4 lands first and renames every std library assembly to its package ID (`internal.poll` → `go.internal.poll`).

- **The rename does not make the path recoverable from the assembly name.** `go.internal.bytealg` still cannot tell
  `internal/bytealg` from a directory named `internal.bytealg`, and `go.vendor.golang.org.x.crypto.chacha20` cannot say
  that `golang.org` is one segment; `go/ast` becomes `go.go.ast`. It does not matter here: under (c) the analyzer never
  reads an assembly name, on either side. The referenced side is read from `GoInternalRoot`, the consumer side from
  `[GoPackage]`.
- **No shared csproj line under (c).** OQ4 changes the `AssemblyName` and `PackageId` lines of every library csproj and
  the test template's assembly name; this design changes no csproj. Under C2's (a), the new `<GoImportPath>` line
  would sit beside the renamed `AssemblyName`, an adjacent-insert hunk to rebase across.
- **ApiCompat.** OQ4 gives the guard a rename map. This seat adds an attribute to 185 package classes and an
  assembly attribute; the guard passes no attribute rule (none is named in `GoApiCompat.psm1`), so both should read as
  no change. Not run; stated as a prediction (section 10).
- **Order.** Cut this seat on a base that contains OQ4's merge, so the four-target footprint is measured on the renamed
  tree and the two batches never stack.

## 10. Red first, controls and predictions

### 10.1 Red first (each fails at the base, before the change)

GenTests, the analyzer in memory against metadata references carrying the attribute:

1. **In-root hand file, no warning**: a compilation declaring `[GoPackage("lib")]` in `namespace go.RootedUsingCapture;`
   with a file `lib_impl.cs` naming the package class of a reference marked `GoInternalRoot("RootedUsingCapture")`.
2. **Hand file in module A naming a std internal package: GO2CS0014**, once, at the first name, with `Standard = true`,
   root "".
3. **Pure C# naming an internal package: GO2CS0014**: no `[GoPackage]` class; once through the package class, once
   through an alias declared in another file (reported at the use).
4. **The standard library's own hand files: none**: `GoStandardLibrary=true`, `[GoPackage("os")]` in `namespace go;`,
   an `os_impl.cs` naming `internal/poll`'s class. Then `net/http` naming a `crypto`-root reference: GO2CS0014 (a
   non-empty std root is still a path test). Then a module whose path is `crypto/x`, no `GoStandardLibrary`, naming the
   same reference: GO2CS0014.

Controls, each green at the base and after:

- **Public package**: case 3 against an unmarked reference: none.
- **Converter output**: case 2 with the use in a converted-named file: none (the skip is real).
- **Go-allowed shapes**: case 2 with `var x = pub.F(); x.Method();`, the internal type never named: none.
- **Decode table**: the section 6.2 cases, each decoding to its import path.

Converter (`go test ./...`): the root function's table; `package_info.cs` emission for `internal/poll` (""),
`crypto/internal/fips140/aes` (`crypto`), `vendor/golang.org/x/net/idna` (""),
`vendor/golang.org/x/crypto/internal/alias` (`vendor/golang.org/x/crypto`), a module internal package, and a public
package (no line); the `EditorBrowsable` line above `[GoPackage]`.

Package level, a new `src/tests/PackageTests/InternalPackageWarning` fixture in the pattern of `ConsumerUsings` and
`BreakFloor` (one feed, a fresh package cache): (A) `Program.cs` naming a `go.internal.*` class: one GO2CS0014;
(B) control, a public package: none; (C) `-p:WarningsAsErrors=GO2CS0014`: the build fails (the code is the one
raised); (D) with `go.gen` also referenced: exactly one GO2CS0014, not two; (E) `-p:GoInternalPackageWarning=false`:
none. Red at the base: (A) and (C) read no diagnostic against the published go.lib.

The whole-stdlib gate, made to fail first (safety floor 13): plant one `*_impl.cs` in `net/http` naming a
`crypto/internal` class, read exactly one GO2CS0014 naming that file, remove it, confirm the tree byte-identical, then
read the four-target stdlib builds at zero.

### 10.2 Predictions, stated for the cut

- **Corpus footprint (two-seeded, four targets): only the 224 marked `package_info.cs` files move, each by +2 lines**
  (the attribute and the `EditorBrowsable` line). Per target: windows/amd64 181, linux/amd64 179, darwin/amd64 179,
  linux/arm64 177 (section 4.3), union 224. No `.csproj`, no converted Go file, no `.cs.auto`, no test info file moves.
  Any other moved file is a finding to explain, not to absorb.
- **Hand-owned changes**: golib (the attribute; the pack item and the analyzer project reference in `golib.csproj`;
  the new target in `go.lib.targets`), go2cs-gen (the analyzer, the descriptor), the new small analyzer project (and its
  `go2cs.slnx` registration), `src/core/Directory.Build.props` (+2), C1's golib baseline (+1),
  `docs/DiagnosticCodes.md` (+1 row), the tier-3 sentence of `docs/PublicSurface.md`, the consumer guide.
- **CNR: one package changed**, `RootedUsingCapture`, one file (`internal/sync/package_info.cs`, +2/-0). Falsifier:
  any `.csproj` or any other `package_info.cs` moving. Under C2's (a) the prediction would have been every behavioral
  package.
- **Std warning count: 0 on all four targets.** Read-only census of the 255 hand-owned files (the `*_impl.cs` and
  `*_impl_test.cs` files, the `[module: GoManualConversion]` files, and the files of the hand-owned `testing` and
  `unsafe` projects): every reference they make to a non-std-root internal namespace is from inside that root
  (`crypto/internal/...` files naming `go.crypto.@internal.*`, `crypto/x509/internal/macos/darwin` naming
  `go.crypto.x509.@internal`, `vendor/golang.org/x/crypto/internal/alias` naming its own root). The census reads
  namespace spellings, so it is a prediction; the build is the gate.
- **Not reached, and stated as such**: `src/tests/GolibTests` (40 files mention `@internal`) loads neither go2cs-gen
  as an analyzer nor go.lib's buildTransitive, so it gains no warning; `src/core/go2cs.CpuProfiler` names no internal
  namespace.
- **Pack**: every marked package still writes its `build/<id>.props` (the `EditorBrowsable` line is above
  `[GoPackage]`). Falsifier: the "declares no namespace followed by a [GoPackage] package class" error.
- **ApiCompat**: no new difference from the attributes. go.lib's new `buildTransitive/` DLL is outside the guard's
  asset pattern.

## 11. What changed from C2's groundwork

- **The consumer's path is already recorded.** "Assemblies record no import path today" is not so: `[GoPackage]` plus
  its namespace, with the `ImportPath` stamp exactly where the decode is lossy, is a lossless in-assembly record that
  golib already decodes. That turns the consumer-path decision from "add a csproj line to every converted project" into
  "add nothing" (section 6).
- **The census is 185, not 184**: 170 internal (168 internal-only plus 2 both, as the plan's own count says), 17
  vendor. C2 counted 169 internal.
- **A `CompilerVisibleProperty` declared by golib's buildTransitive would not reach converted projects**: they
  reference golib as a project and never import `go.lib.targets`. Moot under (c); it would have needed the template.
- **The pure-consumer evidence is stronger than "PrivateAssets=all"**: go.gen is not a dependency of any go.* package
  at all (none of the published 1.26.10.2 nuspecs names it), and the `exclude="Build,Analyzers"` C2 cited is NuGet's
  default, seen in the packed nuspec, not in the tree.
- **The `EditorBrowsable` line has a placement constraint** C2 did not state: above `[GoPackage]`, or the pack's
  usings-props regex fails for all 185 packages.
- **Std membership needs its own fact.** With root "" a prefix of every path and behavioral module paths dotless, the
  in-root test needs `Standard` on the attribute and `GoStandardLibrary` on the consumer.
- **The unlist ID set is not `go.internal.*`**; that prefix covers 97 of 185 IDs.
- Confirmed as stated: the private go.gen reference (the swap in `projectFileWriter.go` is its `if emitNuGet` block, at lines 405-412),
  exact pins, only go.lib's buildTransitive reaching a pure consumer, no unlist step, Go's rule, converted code unable
  to violate it, and the separate small analyzer so no generator runs over hand-written C#.

## 12. Open questions for COORD, with recommendations

1. **Consumer path: (c), the `[GoPackage]` decode, over C2's (a) csproj property.** Recommend (c): no emission, no CNR
   churn, no csproj line beside OQ4's.
2. **Standard-library membership: `GoStandardLibrary` in `src/core/Directory.Build.props`.** Recommend it over a
   converter-emitted flag (one hand-owned line against 600 csproj lines).
3. **Which trees are hand-written: go2cs-gen's existing name-and-marker rule, or the `GoPositionMap`-record rule** that
   also catches a user's differently named partial. Recommend the existing rule now, with the gap documented in the
   diagnostic's row; take the record rule if the owner wants the gap closed before the warning becomes an error.
4. **A C# project that references converted PROJECTS** (`src/tests/CSharpConsumer`'s shape) gets no warning.
   Recommend accepting and documenting it: the ruling is about the published packages, and such a consumer can add the
   analyzer by hand.
5. **One diagnostic per (file, internal package), at the first naming use**, rather than per site. Recommend per file,
   the analogue of Go's per-import error.
6. **Unlisting**: feasible with an owner ruling scoping the runbook's "Never unlist", an API key with the unlist scope,
   and a Phase 3b. Recommend not now; revisit after one release with the analyzer.
7. **`docs/PublicSurface.md` tier 3** says "nothing is done to warn about them". Recommend this seat amends it to name
   GO2CS0014 and the ruled wording, in the same commit as the analyzer, with C1 told.
8. **Names**: the attribute `GoInternalRoot`, the switch `GoInternalPackageWarning`, the small DLL (proposed
   `go2cs-consumer-analyzers.dll` under go.lib's `buildTransitive/`). Recommend these unless C1's page has a naming
   rule they break.
9. **Test assemblies stay unmarked.** Recommend: they are never referenced.
10. **The wording.** The ruling's "not covered by any compatibility promise; this warning may become an error" is used
    verbatim in the message and the row. Per C1's page, a severity change happens only at a release whose notes say
    so; recommend the row say that too.

## 13. Order of work (on the rulings)

1. Red first: the GenTests cases, the converter tables, the PackageTests fixture, each read red at the base.
2. golib: the attribute, the baseline line. go2cs-gen: the descriptor and the analyzer. The small project; golib's
   pack item; the `go.lib.targets` target; `src/core/Directory.Build.props`.
3. Converter: the root function and the two `package_info.cs` lines.
4. The two-seeded four-target reconvert on a base that contains OQ4, scored against section 10.2; apply the delta.
5. Docs: the DiagnosticCodes row, the PublicSurface amendment, the consumer guide.
6. Gates: `go test ./...` (the converter suite, `-timeout 60m`), repoguard, GenTests, GolibTests, CNR, the footprint, the
   planted-violation stdlib gate then the clean four-target stdlib builds, the PackageTests fixture, ApiCompat. Its own
   batch; not stacked with OQ2 or OQ4.

## Amendment, 2026-11-12: COORD's review, folded in before the cut (C2)

COORD reviewed this design (inbox/R `20261112T064347Z`, verdict APPROVE-WITH-FIXES) and ruled every recommendation
in section 12 as recommended. The implementing lane is C2, branch `claude/c2-oq1-internal-marking`, cut on master
`6869ec2b54`, which contains OQ4's merge (batch 289). The design above is the record. This block states what the
review changed, and section 13's order runs with it.

**M1. A `main` package takes its path from its module.** A main package's class is `[GoPackage("main")]` with no
`ImportPath` (`src/tests/Behavioral/RootedUsingCapture/package_info.cs`), so the section 6.2 decode yields no path,
and a hand `*_impl.cs` in a module's main naming `M/internal/x` would be flagged, though Go allows it. The consumer
path of a compilation whose package is `main` is the module path, read from the compilation's own
`GoModInfoAttribute` assembly attribute (its `mod` line), which every converted executable carries. No `ImportPath` is
stamped on main packages: that would move every behavioral `package_info.cs`. If GoModInfo is absent, there is no
path. A main's directory inside its module is not recorded, so a main is allowed every root INSIDE its module: the
root is `M`, or begins with `M/`. That gives no false positive on correct Go. The accepted gap, documented in the
diagnostic's row, is the false negative this leaves: a main outside a deeper internal root (`M/a/internal/x` named
from `M/cmd/foo`) is not flagged. A red case covers M1, and a control covers the gap.

**M2. An external test's stamp carries `_test` in the path too.** `src/core/math/rand/v2/package_test_info.cs` stamps
`[GoPackage("rand_test", ImportPath = "math/rand/v2_test")]`. The decode drops a trailing `_test` from the stamped
path as well as from the name (the converter's `packageInfoBelongsHere` does the same), so the internal and external
test classes of one test compilation agree. A decode-table row covers it.

**S1. The two analyzer copies are disjoint in code, not by file name.** go2cs-gen's copy acts only in a compilation
that declares a `[GoPackage]` class; the consumer copy, packed in go.lib, acts only in one that declares none. Two
copies in one compilation therefore never both report, and an older go.gen beside a newer go.lib still leaves one
check. go.lib's `<Analyzer>` item is conditioned on the project being C# and on `GoInternalPackageWarning`, with no
file-name test. PackageTests case (D) exercises this.

**S2. The two `package_info.cs` lines are regenerated in place.** Persisted info files are patched, not rewritten from
the template, so both lines get an idempotent update: the assembly line is removed and rewritten on every write, like
`GoHoistedLiterals`, and spelled through `rootAttributeName`. The `EditorBrowsable` line is inserted above the
package's `[GoPackage]` stamp when it is missing, kept when present, and removed when the package is no longer
internal or vendored. A converter test reconverts twice and reads identical bytes.

**S3. The prediction is the exact block, not "+2 lines".** Every assembly stamp is written with its prose and a blank
line. The footprint prediction quotes the block the probe emits, line for line, and is posted before the run.

**S4.** Section 6.3's sentence about a consumer setting `GoStandardLibrary=true` is struck: for a pure consumer the
property never reaches the analyzer. The opt-outs are `GoInternalPackageWarning=false` and `NoWarn`.

**S5. Deduplication is deterministic.** The analyzer makes one semantic-model pass per tree, in a semantic-model
action, never concurrent node actions racing on a shared set. So "one diagnostic per (tree, forbidden assembly), at the
first naming use" means the first in source order, on every run. The row documents that a `#pragma` at that first
site silences the warning for the whole file.

**For the diagnostic's row (review notes).** The accepted false negatives are a static `<Using>` in a csproj and a
target-typed `new()` into an internal-typed parameter. Q3's gap (a differently named partial in a converted project)
widens if the warning ever becomes an error. `RunAnalyzers=false` silences it.

**Taken as proposed, with no contrary ruling.** (i) A main package's path comes from GoModInfo only, not also from a
csproj property. (ii) The consumer DLL keeps the name `go2cs-consumer-analyzers.dll` (Q8). After S1 the name is no
longer load-bearing.

