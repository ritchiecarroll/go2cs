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

---

## AMENDMENT 2026-11-07 — refreshed against master `c6cddb6903`, with gojq as the concrete case (G, COORD order)

> **What this block is:** a refresh, appended under the 2026-09-29 text, which stays as the record. It says:
> - what landed for D1-D7 and M1-M6;
> - what changed since 2026-09-29;
> - a recommended ruling for each of OQ-4a-d;
> - the questions gojq raises now that the flow exists.
>
> **Sources:**
> - read at master `c6cddb6903`; none of the files it cites changed through batch 237 (`6c2a77a4bd`), the base;
> - gojq and timefmt-go read with `go list` from the Go module proxy at v0.12.19 / v0.1.8 and v0.12.20 / v0.1.9, on
>   go1.26.10;
> - published nugetgo versions read from nuget.org's flat container the same day.
>
> P1's closure census (which packages, which tests) is the authority for the conversion itself. The counts below come
> from `go list` and are a prediction for it to confirm.

### A1. What landed

| | status at `c6cddb6903` | where | differs from the design |
|:--|:--|:--|:--|
| **D1** `-tests -recurse` driver (M3) | LANDED | `src/go2cs/moduleTestsDriver.go` (`runModuleTests`); refusals in `main.go` | Plain `-recurse` only: `=module` and `=nuget` are refused ("-tests -recurse supports the plain -recurse form only"). Phase B recompiles production with the test closure. A module with no Go test files is refused. |
| **D2** records by full import path (M1) | LANDED | `testConversion.go` (`comparisonRecordPackage`) | The record keeps its file name in each package's own `src/<importPath>` directory. The import path is in its `package` field. No JSON module record: MODULE.md sums. |
| **D3** module ancestry (M4) | LANDED | `src/core/testing/PackageAncestry.cs` (`TryStageModule`), via `GO2CS_MODULE_ROOT` | A real COPY of the module, not a link. `StageFixtureLinks` still refuses a non-GOROOT package that is not module-staged. |
| **D4** `go` line (M3, M6) | LANDED | `checkModuleGoLine` in the driver; `godebugDefault.go` plus golib's `GoDefaultGodebugAttribute` | The newer-than-corpus refusal is in the `-tests` driver. Plain `-recurse` meets it only through `go mod download`'s own "requires go >=". |
| **D5** lock and preflight (M2) | LANDED | `modulesLock.go` (`go2cs.modules.lock`), `go mod download` with `-mod=readonly`, `goModCacheDir()` in the resolver | Applies to every `-recurse` run. A second lock, `go2cs.nuget.lock`, pins each mapped module's package version and content hash. |
| **D6** packaging (M5) | LANDED, ID changed | `src/tools/nugetgo/nugetgo-pack.ps1`: one nupkg per module, one assembly per package, `go2cs/source-metadata.txt` | The ID is `nugetgo.<dotted module path>` (B2 as amended 2026-10-02), not `go.<…>`. Added since: per-OS flavors, the B4 range, README and icon, `-ThirdPartyPackage` dependencies, `-ExcludePackage`. |
| **D7** proof pages (M1) | LANDED | `validationProofPages.go`: `<outRoot>/validation/<modulePath>/…` plus MODULE.md, packed as VALIDATION.md | Published modules' pages are committed under `docs/validation/modules/<module>@<version>/`. A module with no Go tests gets a behavioral proof (mousetrap). The host-fatal check was not generalized; `-module-disclosures` reads per-module manifests instead. |

### A2. What changed since 2026-09-29

- **Identity, versions, ranges** (PLAN-nugetgo's owner rulings, 2026-09-30, and their amendments):
  - **B2:** every non-stdlib ID is `nugetgo.<dotted module path>`, and `go.` is the stdlib only.
  - **B3:** a release's rebuilds are `X.Y.Z.N`. A prerelease or pseudo-version is `L.0.N`.
  - **B4:** stdlib dependency `[build, next Go minor)`. A third-party edge takes the first revision built for the same
    corpus.
  - **B8:** "gojq's scope is ruled before it is packed".
- **`-recurse` modes:** bare (the module plus its third-party closure, converted), `module` and `nuget` (published
  go.* packages, and with the registry, published nugetgo packages: exact-pinned, one `PackageReference` per module),
  combinable as `-recurse=module,nuget`.
- **Published:** uuid, jwt, x/sync, hashset, pflag, cobra and mousetrap, seven modules. The five on
  `ValidatedModules.md` were read on Go 1.24.13. cobra's counts live in `nugetgo-modules.psd1`, and mousetrap's proof is
  behavioral. **gojq would be the first module read on the Go 1.26.10 corpus.**
- **The precedent for a published dependency is cobra.**
  - It is validated with its dependencies converted from source, by the plain `-tests -recurse` driver.
  - It is packed depending on the published pflag and mousetrap, through `-ThirdPartyPackage`.
  - Hosted sample rows are "additional evidence, never the validation" (COORD ruling 2026-10-12).
- **B3's binary break (batch 231)** and the binary-compatibility guard (COORD ruling 2026-11-07,
  `DESIGN-binary-compat-guard.md`):
  - every module revision built before 1.26.10.3 is rebuilt with it;
  - `nugetgo-pack.ps1` now refuses a revision that drops its previous revision's surface.

### A3. OQ-4a-d: recommended rulings

- **OQ-4a: RATIFY as landed.** One assembly per Go package and one nupkg per module, with the ID per B2 (`nugetgo.`),
  not D6's `go.`. Seven modules have shipped this way.
- **OQ-4b: RATIFY as landed.** The version-free `pkg/<importPath>` with `go2cs.modules.lock`, held for every
  `-recurse` run, not only the driver. Plus `go2cs.nuget.lock` for mapped modules (its content hash is refused on a
  mismatch).
- **OQ-4c: RATIFY as landed.** `<outRoot>/validation/<modulePath>/…` beside the conversion, and a committed copy under
  `docs/validation/modules/<module>@<version>/` for each published module. Nothing is written into the stdlib roster.
- **OQ-4d: RATIFY as landed, with one point open.**
  - The driver requires an explicit output root and refuses one inside the module. Only main-module packages are
    validated, and dependencies' own tests are not run.
  - The Go oracle is `go test -json .` in the input directory. No module has yet been validated with its main module
    read straight from the read-only module cache, so gojq's first run is the measurement.
  - A main module whose tests write into their own directory needs a writable clone. gojq's root tests only read
    (`builtin.jq`).

### A4. gojq: the questions the flow raises (new, for ruling)

> **Owner, 2026-11-07, relayed by COORD:** "Yes, let's shoot for being able to publish gojq with .3". This section
> rests on P1's scout of gojq v0.12.19 (`SCOUT-gojq.md`, measured on linux/amd64 at `c6cddb6903`) and C2's scout of
> timefmt-go v0.1.8 (`SCOUT-timefmt-go.md`). The per-GOOS closure was read with `go list -deps` here.

**The facts.**

| package | reached by | closure on windows | closure on linux and darwin | converted tests, measured with the scout's patches |
|:--|:--|:--|:--|:--|
| `gojq` (library root) | — | timefmt-go | timefmt-go | 162 of 164 (2 fail on defect E) |
| `gojq/cli` | `cmd/gojq` | go-yaml, go-isatty, go-runewidth, uax29/v2 (graphemes, internal/iterators), stringish | the same plus **`golang.org/x/sys/unix`** (through go-isatty) | 898 of 914 (16 fail on defects F and H) |
| `gojq/cmd/gojq` | — | `package main` | `package main` | no tests |

- **The root's build tag** is `gojq_debug`, which is not a GOOS tag. So its flavors should compare equal and the root
  ships neutral. The pack decides.
- **timefmt-go** validates clean: 417 matched, 0 disclosed.
- **Before gojq builds:** three converter defects (A, B, C) block the build. A fourth (D) fails `-tests`. golib's
  defect E costs the root its 2 tests. Defects F and H cost `cli` its 16. Seats are dealt for A to H; G is a 440x
  slowdown in 2 `cli` subtests, which needs a `-test-timeout` of about 50 minutes until it is fixed.
- **The driver validates the whole module.** It has no package filter, so its exit (NugetgoPublish section 1, gate 3)
  is `cli`'s exit too, even when `cli` is not shipped.

**The questions:**

- **OQ-4e: scope (B8).** Recommend **the library root only for 1.26.10.3**, packed with
  `-ExcludePackage github.com/itchyny/gojq/cli`. `cmd/gojq` is a main package and is never packed.
  - **Precedent:** cobra/doc, a public package that was validated and excluded because its imports are not nugetgo
    packages (`nugetgo-modules.psd1`).
  - **What the root is:** what the Roadmap row promises, an in-process jq for .NET. Its whole third-party closure is
    timefmt-go.
  - **Why not `cli` at 1.26.10.3:** shipping it means publishing six more modules first (OQ-4f), x/sys among them.
  - **Cost:** none in converter work beyond the seats already dealt. `cli` must still validate in the same run
    (previous bullet), so F and H stay on the critical path, or `cli`'s verdicts go into a disclosure manifest under
    `-module-disclosures`.
- **OQ-4f: the dependency order.** Recommend **timefmt-go v0.1.8 validated, packed and published first**, as its own
  module, then gojq packed against it.
  - The edge: `-ThirdPartyPackage github.com/itchyny/timefmt-go=nugetgo.github.com.itchyny.timefmt-go@0.1.8`, a
    bare minimum at its first revision for the corpus (B4). This is cobra's route.
  - gojq's validation run converts timefmt-go from source (`=nuget` is refused under `-tests`). The published binding
    is checked by hosted sample rows.
  - **Cost:** timefmt-go must be live on nuget.org, and visible to gojq's `-recurse=nuget` conversion, before gojq's
    pack. That means its registry row (owner, NugetgoPublish section 5), or a local `-nuget-map` source naming it.
- **OQ-4g: the version.** Recommend **gojq v0.12.19 with timefmt-go v0.1.8**, the Roadmap's pins.
  - **Revised from the draft's v0.12.20:** the scout and every seat were measured at v0.12.19. A version change now
    re-opens the conversion, the build and the test readings a week before the release.
  - v0.12.20 (tagged 2026-11-01, needing timefmt-go v0.1.9) becomes a new package version in a later wave, not a
    revision.
  - **Cost:** the first publish is one release behind upstream.
- **OQ-4h: the corpus.** Build on **1.26.10.3**, after B3, in the 1.26.10.3 nugetgo wave.
  - NugetgoPublish section 0 makes `T` the release's record commit, so gojq packs after the go.* push, with C2's
    rebuild wave.
  - Its first revision then becomes the guard's first nugetgo baseline on the 1.26 line.
- **OQ-4i: the rest of the closure (new).** Recommend that it **waits for a later wave**. Of the six modules `cli`
  needs, `golang.org/x/sys` cannot be packed by today's rules:
  - **The pack refuses it.** Its `unix` package has no Windows build, and `nugetgo-pack.ps1` converts every GOOS
    flavor and refuses "the windows flavor has no <package>: a package missing on one GOOS is not packed".
  - **It is large:** 12 packages on Windows alone, 61 test files.
  - **Its tests exercise the kernel**, so its validation is host-bound.
  - **go-isatty differs per flavor:** its Windows flavor imports no x/sys at all, and its unix flavors import
    `x/sys/unix`.
  - **go-yaml would be the first pseudo-version package** (B3's `L.0.N` route).
  - **What it would take:**
    - a pack rule for a package that exists on some GOOS only: runtimes/ assets for those RIDs and no lib/ compile
      asset, a design question for COORD;
    - a validation route for x/sys;
    - five more conversion repositories.

---

> **RULED, 2026-11-07 (COORD, on `4b62f179e6`; OQ-4e by the owner): the recommendation is adopted in full.**
> - **OQ-4a, 4b, 4c: RATIFIED as landed.**
>   - 4a: one assembly per package, one nupkg per module, with B2's ID.
>   - 4b: the version-free `pkg/` with both locks.
>   - 4c: proof copies under `docs/validation/modules/<m>@<v>/`.
> - **OQ-4d: RATIFIED.** gojq's first run measures the oracle against the read-only module cache, and that reading
>   goes in the gojq record, stated as the first.
> - **OQ-4e: RULED BY THE OWNER**, relayed by COORD: "Yes, command line deffered is fine, lib focus for .3".
>   - The library root for 1.26.10.3: `-ExcludePackage github.com/itchyny/gojq/cli`, on cobra/doc's precedent.
>     `cmd/gojq` is never packed.
>   - No driver package filter for 1.26.10.3. The driver still converts and tests `cli`, so seats F and H (P1) and A
>     (batch 241) stay in 1.26.10.3, so that `cli` validates or is disclosed.
> - **OQ-4f, 4g, 4h: RATIFIED.**
>   - timefmt-go v0.1.8 first, then gojq v0.12.19 packed against it with a bare-minimum edge.
>   - Both are in the 1.26.10.3 wave, with `T` the record commit.
>   - v0.12.20 and v0.1.9 come later, as new package versions.
> - **OQ-4i: DEFERRED**, until after 1.26.10.3. x/sys/unix has no windows build, and packing a package that exists on
>   some GOOS only is a design question. So 1.26.10.3 packs TWO closure packages, timefmt-go and the gojq library.
>   The plan and the owner's steps are in `PLAN-rebank-1-26-10-3.md` section 8.
