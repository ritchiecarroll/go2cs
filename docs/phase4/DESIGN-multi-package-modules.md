# DESIGN — Multi-package modules: converting, testing, packaging and proving a module with several packages

**Status:** PROPOSED (OQ-4), for COORD's ruling before any cut. Read-only design, lane R, 2026-09-29. Read at master
`a1f133c3a9`; `file:line` cites are to `src/go2cs/` unless another directory is named.

The roadmap's real-world modules need five things. This record designs all five as one flow:
- one flow that runs a module's tests across all its packages (`-tests` combined with `-recurse`);
- a layout for a dependency taken from the module cache;
- staging test fixtures that live outside `testdata`;
- a packaging decision for a module with several packages;
- third-party proof pages written beside the conversion.

The running examples are the roadmap's own: `golang-jwt/jwt` (the root package, `request`, and a `test` helper package
the other packages' tests import) and `itchyny/gojq` with its dependency `itchyny/timefmt-go`.

---

## 1. What exists today

### 1.1 `-tests` converts, builds, runs and compares ONE package

- **Converting a non-GOROOT package already works.** `getProjectName` walks up to `go.mod` for the module path
  (`importOperations.go:49-219`), the import path everywhere downstream is go/packages' `PkgPath` (`testConversion.go:729,
  945`), and the Go oracle is `go test -json .` run in the INPUT directory (`testConversion.go:8800-8836`).
- **One test project per package**, written into the one output directory:
  - the converted `*_test.cs` files, the host `go2cs_test_host.cs`, the manifest, and `<name>.tests.csproj`;
  - the publish output (`bin/tests/publish`) and the comparison and results records (`testConversion.go:52-75,
    924-972, 6377, 8902`).
- **One single-file host per package** (`OutputType Exe`, SelfContained, PublishSingleFile). It runs its tests in a
  sandbox at `src/<importPath>` (`src/core/testing/TestHost.cs:207-251`).
- **The reference models need the production project COLOCATED** with the test project (`<name>.csproj` in the same
  output directory, `testConversion.go:4423-4425`). The recompile fallback lists production sources instead
  (`:4459-4461`).
- **Another package of the same module** (a sibling, or a helper imported only by tests) resolves through
  `getLocalModulePackageInfo` (`importOperations.go:518-596`). That yields an ABSOLUTE `<Go source dir>/<name>.csproj`,
  never checked for existence and never made relative (`:585-595`, `testConversion.go:4528`). So it works only if that
  package was earlier converted IN PLACE. Otherwise MSBuild warns (MSB9008) and the build fails (CS0246/CS0234), and the
  alias metadata read from that directory comes back empty with no message.

### 1.2 `-recurse` converts a whole module, but not its tests

- **One project per package**, in dependency order (`conversionDriver.go:177-202`, `moduleConverter.go:106-111`):
  - main-module packages go to `<root>/src/<importPath>`, every dependency to `<root>/pkg/<importPath>`
    (`moduleConverter.go:366-374`);
  - references between them are RELATIVE (`projectFileWriter.go:442-452`);
  - the standard library is referenced, never converted (`moduleConverter.go:236-238`);
  - output may be outside any checkout, with `Directory.Build.props` pinning `go2csPath` (`:700-728`).
- **Dependency versions are DROPPED** from paths and names (`moduleConverter.go:360-365`). Two conversions into one root
  at different dependency versions silently overwrite `pkg/<importPath>`.
- **Test-only closure is never loaded:** `_test` variants are skipped and `Tests` is not set (`moduleConverter.go:228,
  151-173`).
- **`-recurse=nuget` has no third-party arm.** Only stdlib imports become `PackageReference`s
  (`projectFileWriter.go:433-439`).
- **`-tests` with `-recurse` is REFUSED** (`main.go:551-557`).

### 1.3 Hardwired to GOROOT inside the test host

- `PackageAncestry.TryStage` mirrors only `GOROOT/src/<importPath>` and returns false otherwise. A module package's
  sandbox is a bare `src/<importPath>`, with no `go.mod` and no sibling packages above it (`src/core/testing/
  PackageAncestry.cs:77-99`).
- `StageFixtureLinks` THROWS for a package not under GOROOT (`:246-252`). The converter selects link-staging for any
  `testdata` directory holding only `package main` files (`testConversion.go:5852`).
- `../testdata` fixtures are copied ABOVE the output directory (`testConversion.go:6139-6150`).
- The comparison record is named by the BARE directory name, so `request` or `test` collides across modules
  (`testConversion.go:6471, 6482, 8959`).
- The host-fatal disclosure check keys proof pages by `core/<importPath>`, and only prints "unchecked" elsewhere
  (`testConversion.go:7098-7116, 7218-7226`).
- The toolchain pin requires GOROOT to equal the corpus release (`main.go:772-783`).

### 1.4 Proof pages are the stdlib roster's

- `emitValidationProofPage` (`validationProofPages.go:780`) finds the go2cs root by walking up to `core/golib`, and
  writes `<root>/../docs/validation/current/<dot-id>.md` plus the index (`:625-641`). It silently does nothing without
  a root (`:787-797`).
- The dot-id's reverse mapping is exact only for stdlib paths, so it breaks on `github.com/...` (`:104-111`).
- The page links `src/core/<importPath>` (`:223, 571`).
- VALIDATION.md is packed for stdlib output only (`projectFileWriter.go:213-228, 393-398`).
- So a module converted inside a checkout would write into that checkout's stdlib roster, and one converted
  elsewhere gets no page at all.

### 1.5 Packaging

- Every converted project is `AssemblyName` = the dotted import path, and `PackageId go.$(AssemblyName)`
  (`importOperations.go:296-298`, `csproj-template.xml:64`). So packing a multi-package module yields N package ids.
- PLAN-nugetgo.md's v1 rule is "one module, one nupkg, one root package ID" (§5), keyed on the MODULE path (§4). It
  explicitly allows the module's packages' assemblies to ship together (§5).

---

## 2. The combined flow: `go2cs -tests -recurse <moduleDir> <outRoot>`

**D1: One driver, two phases, one output root.**
1. **Phase A is today's `-recurse` conversion,** extended to load the TEST closure too (`Tests: true`). That way a
   helper package imported only from `_test.go` files (jwt's `test`), and any test-only dependency, is converted like
   any other package.
2. **Phase B runs `-tests` once per main-module package that has tests,** with its output directory set to THAT
   PACKAGE'S phase-A directory, `<outRoot>/src/<importPath>`. That gives the colocation the reference models already
   require, with no new model.
3. **Test projects reference siblings and dependencies through the SAME resolver `-recurse` uses**
   (`getRecurseDependencyInfo`, relative paths). `getLocalModulePackageInfo` is never reached under the driver.
4. The `-recurse`-only refusal at `main.go:551-557` becomes the entry point.
5. **Output root must be outside the module's source tree.** In-place output is refused under the driver
   (item 12 of the gap list: in-place under GOPATH loads `./...`).

**D2: One host per package, as today.** Isolation, parallel execution and the existing host are kept.
- A module run is N hosts plus one MODULE record summing them (§5).
- Comparison and results records are named by the FULL import path (dotted, or path-keyed as in §5), which fixes the
  `request`/`test` collision.

**D3: Module ancestry in the sandbox.**
- `PackageAncestry` gains a MODULE root: it stages the module's source root at `src/<modulePath>` (a read-only junction
  or symlink, as the link-staging already does), so the package sits at its relative path under a real `go.mod`.
- That fixes, in one mechanism:
  - tests that read `../` files outside `testdata`, including jwt's `test/*.pem`, which lives in a SIBLING package's
    directory;
  - tests that read `go.mod`;
  - `package main` testdata link-staging (`StageFixtureLinks` takes the module root instead of refusing).
- `../testdata` copies land inside the output root, not above it.

**D4: A module's `go` line and the corpus release.**
- The converted stdlib is the corpus release, so the driver REFUSES a module whose `go` directive is NEWER than
  `<GoStdLibVersion>`, naming both. An older `go` line is fine.
- The module's `go` line then selects Go's GODEBUG defaults for it: the roadmap's `DefaultGODEBUG` layer. That is a
  separate seat (golib side), because it is runtime behaviour rather than flow.

---

## 3. Dependencies from the module cache

**D5: One version per module per output root, recorded and enforced.**
- Go's minimal version selection puts exactly ONE version of each module in a build, so within a root the version-free
  layout `pkg/<importPath>` is correct. What is missing is the record.
- The driver writes `<outRoot>/go2cs.modules.lock`: module path, version, go.sum hash, converter revision.
- It REFUSES a conversion into a root whose lock names a different version of a module, naming both, instead of
  silently overwriting.
- Versioned paths (`pkg/<module>@<version>`) are rejected: they would put the version into every AssemblyName and
  namespace, where Go never shows it.
- The fix for the stale resolver: `getLocalModulePackageInfo` stops hardcoding `GOPATH/pkg/mod` and uses
  `goModCacheDir()` (`importOperations.go:567`).
- The driver runs `go mod download` (read-only against the module's own go.sum) before loading, so a missing entry is a
  refusal, not a per-package load warning.

---

## 4. Packaging (the roadmap's "packaging decision for a module with several packages")

**D6: One assembly per Go PACKAGE, one nupkg per MODULE.**
- **Assemblies stay per package.** It is Go's compilation unit and today's layout, and every reference, the per-package
  metadata (`package_info.cs`, GoPackage) and the stdlib precedent assume it. Merging a module's packages into one
  assembly would change every emitted reference and every package's identity for no semantic gain. Go's `internal/`
  rule is a path-visibility rule that the conversion already enforced at compile time.
- **The nupkg is per module**, satisfying nugetgo v1 ("one module, one nupkg, one root package ID"):
  - root id `go.<dotted module path>`;
  - `lib/<tfm>/` carrying each package's assembly;
  - `go2cs/source-metadata.txt` listing module path, module version, go2cs release, and each PACKAGE's assembly and
    surface.
  - The per-project `PackageId go.$(AssemblyName)` stays for the stdlib's per-package packages and is not used when
    packing a module.
- **`-recurse=nuget`, third-party arm:**
  - a package import maps to its module (the longest module-path prefix, from go/packages' `Module`);
  - through the registry to the module's id;
  - one `PackageReference` per MODULE, whatever number of its packages are imported.
- **Version:** the package version is the MODULE version (e.g. `5.3.1`). The go2cs build rides in the self-description,
  and the registry never pins versions (nugetgo §4).

---

## 5. Proof pages outside the stdlib roster (J1)

**D7: Written beside the conversion, keyed by path, one per package plus one per module.**
- **Where:** `<outRoot>/validation/<modulePath>/<package-relative-path>.md` (the module's root package at `index` of its
  directory), plus `<outRoot>/validation/<modulePath>/MODULE.md` summing every package's matched / disclosed /
  excluded.
- **Keying by PATH** avoids the dot-id and its non-invertible mapping (`validationProofPages.go:104-111`). Nothing is
  ever written into a go2cs checkout's `docs/validation/` or roster.
- **What writes them:** the same `emitValidationProofPage`, in a third-party mode chosen when the package is not under
  `core/`:
  - it takes `<outRoot>/validation` instead of walking up to `core/golib`;
  - it links the converted package's own directory, not `src/core`;
  - it skips the stdlib index and roster.
  - The host-fatal disclosure check is generalized to that root (`testConversion.go:7218-7226`).
- **Packing:** the module nupkg carries `MODULE.md` as VALIDATION.md and the per-package pages beside it (extending
  `projectFileWriter.go:393-398`).
- **Deferred, named:** a README badge for third-party modules. There is no go2cs.net page to link, so it waits for the
  registry's publishing identity.

---

## 6. Seat ladder (rough sizes; each seat red-first with a converter integration fixture, the zsstore_test shape)

| Seat | Scope | Rough size | Unblocks |
|:--|:--|:--|:--|
| **M1** | Third-party proof pages (D7) + comparison records named by full import path (D2) + the host-fatal check generalized | converter ~200 lines + fixture; no golib | **uuid** (one package: today's single-package `-tests` plus M1 is the whole pilot) |
| **M2** | Module-cache dependency lock and refusal (D5), `go mod download` preflight, `goModCacheDir()` in the stale resolver, an END-TO-END `ConvertModule` over a real `@version` cache entry (none exists today; `moduleConverter_integration_test.go` uses `replace` fixtures only) | converter ~250 lines + one fixture module in a temp cache | **gojq** (timefmt-go from the cache), with the gojq packaging from M5 |
| **M3** | The `-tests -recurse` driver (D1): test closure in phase A, per-package phase B into the recurse layout, sibling references through the recurse resolver, the `go` line refusal (D4) | the largest seat: converter ~500-700 lines, 3-package fixture (root, sub, test-only helper) | **jwt** (root, request, test) |
| **M4** | Module ancestry in the sandbox (D3): PackageAncestry module root, StageFixtureLinks off GOROOT, `../testdata` inside the root | golib `testing` ~150 lines + converter staging, GolibTests arms | **jwt's `test/*.pem`** (fixtures outside `testdata`) |
| **M5** | Packaging (D6): per-module nupkg of per-package assemblies, the self-description, the `-recurse=nuget` third-party arm | push script + converter ~300 lines | publishing any module (after its validation) |
| **M6** | `DefaultGODEBUG` from the module's `go` line (D4, golib) | golib godebug layer; sized separately | **jwt** (the roadmap names it) |

**Order:**
- M1 first, which also closes the uuid pilot.
- M2 for gojq, with its packaging decision taken here as D6. That is the roadmap's "made by the time gojq is reached".
- M3 + M4 + M6 for jwt.
- M5 whenever the first module is published.

M3 depends on M1 (records and pages) and should land after M2 (the lock is the driver's root invariant).

## 7. Open questions for the ruling

- **OQ-4a:** assemblies per package with a per-module nupkg (D6, recommended), or one merged assembly per module?
- **OQ-4b:** the version-free `pkg/<importPath>` plus a lock (D5, recommended), or versioned paths?
- **OQ-4c:** proof-page location `<outRoot>/validation/<modulePath>/…` (D7), or inside the converted package directories?
- **OQ-4d:** does the driver require an explicit `<outRoot>` (recommended: yes, refusing in-place), and may it run the Go
  oracle against the module cache's read-only tree (it must, for dependencies' own tests, which D1 does NOT run: only
  the main module's packages are validated)?
