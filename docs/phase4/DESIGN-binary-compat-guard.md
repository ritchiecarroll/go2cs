# DESIGN — a binary-compatibility guard for the go.* packages

> **State: PROPOSED sizing (G, 2026-11-07), design only; nothing is cut.** COORD's order (4), 2026-11-07: size the SDK's
> package validation and ApiCompat against a hand-rolled check; say how B3's intended break is suppressed so the guard
> is green at 1.26.10.3 and red on the next unannounced break; say which packages it covers; and state the policy on
> B4's dependency range that the guard enforces. Measured at master `c6cddb6903` against the published 1.26.10.1 and
> 1.26.10.2 packages, with the .NET SDK 10.0.400 and Microsoft.DotNet.ApiCompat.Tool 10.0.403. Every number below was
> read in scratch; the census tables are committed under [`data/binary-compat/`](data/binary-compat/).

## 1. Why a guard

- A NuGet range only constrains restore; it does not check what a compiled reference binds. The go.* assemblies are
  not strong-named, so the loader takes whatever version restore picked, and a missing member surfaces at run time as
  `MissingMethodException` or `TypeLoadException`.
- B4 ([`PLAN-nugetgo.md`](../PLAN-nugetgo.md), *OWNER RULINGS 2026-09-30*) gives every converted module's stdlib
  dependency the range `[build, next Go minor)`. That range is honest only while each revision is binary-compatible
  with the ones before it inside the Go minor.
- B3 (batch 231) is the first break that is known to matter. A `foreach` over a slice compiled against 1.26.10.2 binds
  `slice<T>.Enumerator.get_Current` returning `(nint, T)`; at 1.26.10.3 that member returns `T`. Nothing in the
  release flow saw it: it was found by reading the change (`PLAN-rebank-1-26-10-3.md`
  §8, on `claude/g-dot3-prep`).
- No compatibility check exists in the repo today: no `EnablePackageValidation`, no ApiCompat, no
  `CompatibilitySuppressions.xml`.

## 2. What was measured

| # | measurement | result |
|:--|:--|:--|
| M1 | SDK package validation, golib at master packed against published `go.lib` 1.26.10.2 | **12 diagnostics, all B3**: 1 CP0001 (`sslice<T>.Enumerator` removed), 9 CP0002 (both enumerators' `(nint, T)` `Current`, `IArray<T>.GetEnumerator`, `sslice<T>.GetEnumerator`, five index-yielding `builtin.range` overloads), 2 CP0008 (`array<T>` and `slice<T>` enumerators no longer implement `IEnumerator<(nint, T)>`). About 2 s on top of the pack |
| M2 | M1 with a generated suppression file of those 12 | green (exit 0, 0 diagnostics) |
| M3 | M2 after one deliberate regression: `GoTypeOps<T>.Instance` made `internal` | **red, naming exactly that member** (one CP0002); the 12 suppressed stay quiet. Restored; the file's blob is identical, and the next run is green again |
| M4 | the published 1.26.10.1 → 1.26.10.2 pair, over the 46 go.* packages the seven published modules depend on | **one baseline diagnostic**: `go.lib` CP0006, `IGoTypeOps.NewNativeBox` added to an interface. 36 s for all 46 |
| M5 | the same 46 packages, published 1.26.10.2 → master, Windows asset, assembly mode | **golib's 12 again (the B3 set) and 0 in the other 45**. 40 s for all 46 |
| M6 | `go.gen` 1.26.10.1 → 1.26.10.2 | "no breaking changes", vacuously: the package holds only `analyzers/dotnet/cs/go2cs-gen.dll`, and ApiCompat compares `lib/` and `runtimes/` assets |

### 2.1 Two traps in the SDK path, both measured

- **The SDK's validation is incremental.** `RunPackageValidation` writes a semaphore in `obj/` and is skipped when its
  inputs are unchanged. A second pack with the suppression file **removed** exited 0 with "Skipping target
  RunPackageValidation because all output files are up-to-date". That is a false green, and a release script that packs
  twice reads it.
- **The suppression file is an item, not a property.** `-p:ApiCompatSuppressionFile=<path>` is silently ignored
  (`@(ApiCompatSuppressionFile)` stays empty) and every suppressed diagnostic comes back. The property that sets it is
  `CompatibilitySuppressionFilePath`; the default is `CompatibilitySuppressions.xml` beside the project.

### 2.2 Per-OS surface: what ApiCompat reports inside one package

Package mode also compares each package's compile asset (`lib/net10.0`, the Windows build) with each runtime asset
(`runtimes/<rid>/lib/net10.0`). On the 46 packages that reads **8,523 diagnostics in three packages** at both 1.26.10.1
and 1.26.10.2: `go.syscall` 8,452, `go.runtime` 57, `go.os` 14. They are Go's own per-GOOS API (`syscall.AddrinfoW`
exists only on Windows), not a change between revisions. Package mode exits non-zero on them, so a guard in package
mode has to suppress them permanently or it is red at every release.

Assembly mode, run asset by asset (`apicompat -l <baseline>/<asset> -r <current>/<asset>`), compares only the baseline
with the current build of the same asset, so this class never appears: `go.syscall` 1.26.10.1 → 1.26.10.2 reads 0 on
`lib/`, `runtimes/linux-x64` and `runtimes/osx-arm64`, about 3 s each.

### 2.3 1.26.10.2 → master over the 46 packages (M5)

- **The build:** the 46 packages and their closure (196 assemblies), built at master in Release for the default
  `GoTargetOS` (Windows) in 482 s.
- **The comparison:** each package's published 1.26.10.2 `lib/net10.0` asset against the master build of the same
  assembly, in assembly mode.
- **Result:** golib reads 12: 1 CP0001, 9 CP0002 and 2 CP0008, the same composition as M1 read through the SDK path
  in package mode, so the two paths agree. The other 45 read 0.
- **So, at these 46 packages, 1.26.10.3 carries one break, and it is B3.** Nothing the converter changed between the
  two revisions moved a public member of the converted stdlib.
- **Not covered:** the Linux and macOS runtime assets. Master was built for Windows only, and the guard's first real
  run covers them.
- The tables are [`census-1.26.10.1-to-1.26.10.2.tsv`](data/binary-compat/census-1.26.10.1-to-1.26.10.2.tsv) and
  [`census-1.26.10.2-to-master-c6cddb6903.tsv`](data/binary-compat/census-1.26.10.2-to-master-c6cddb6903.tsv). The
  generated suppression file is
  [`golib-B3-CompatibilitySuppressions.xml`](data/binary-compat/golib-B3-CompatibilitySuppressions.xml).

### 2.4 M4's one diagnostic does not reach a consumer

`IGoTypeOps` is `[EditorBrowsable(Never)]`, and golib's sealed `GoTypeOps<T>` (private constructor) is its only
implementer. Generated code implements `IGoTypeOpsSource` and names `GoTypeOps<S>.Instance`; it never implements
`IGoTypeOps`. Adding a member to it therefore breaks no compiled reference. ApiCompat cannot know that, so the guard
needs a suppression line for this class too. M4 found one such line in a whole revision.

## 3. The SDK's engine, driven after the merge, not a hand-rolled check

**Recommendation: use ApiCompat (the engine `EnablePackageValidation` runs), called by `push-nuget.ps1` on the final
merged packages, asset by asset in assembly mode, against the last published revision. Do not set
`EnablePackageValidation` in the projects.**

- **Why not in the projects.** `push-nuget.ps1` builds and packs once per `GoTargetOS`, then merges the flavors: the
  published `lib/net10.0` is the Windows build and `runtimes/<rid>/lib/net10.0` carries each OS. Validation inside
  `dotnet pack` sees an unmerged flavor whose `lib/` is that OS's build and compares it with the published `lib/`, the
  Windows build. Every platform package (`syscall`, `runtime`, `os`, …) would be red on every non-Windows flavor. The
  incremental skip (§2.1) is a second reason.
- **Why not hand-rolled.** A reflection diff would have to re-derive ApiCompat's rules: interface additions (M4's
  CP0006), removed interface implementations (B3's CP0008), return-type changes reported as removals (B3's CP0002),
  generic constraints, sealing, virtual changes. M1 shows the engine already reads B3 exactly, and M3 shows it names a
  single regression with no noise.
- **Why assembly mode, asset by asset.** It is a baseline-only comparison by construction (§2.2), so the per-OS
  surface is never a diagnostic and needs no suppression. The driver lists the baseline package's assets and requires
  each one to exist in the current package (a missing asset is a break the tool cannot see).
- **The tool** is pinned in a repo tool manifest (`.config/dotnet-tools.json`, `microsoft.dotnet.apicompat.tool`) so
  every box reads the same rules.
- **Where it runs.** After the merge, before the first push, in the same pre-flight that already checks the feed. The
  baseline is each package's last published revision of the same Go minor, fetched from the flat container that
  `_nugetpush.ps1` already queries. `-VerifyOnly` runs it too. A pack-only run may skip it, but a run that pushes may
  not.
- **Cost.** M4 read 46 packages in 36 s in package mode, and the largest asset (`syscall.dll`) takes about 3 s in
  assembly mode. The whole stdlib is about 600 packages, most with one asset and the platform packages with five, so
  the guard is about 10 to 20 minutes on one box. The baseline download is one fetch per package, which the push
  already makes for its feed check.

## 4. B3's suppression: green at 1.26.10.3, red on the next unannounced break

- A suppression file per package, checked in beside its project as `CompatibilitySuppressions.xml` (the SDK's default
  name, so a developer running the SDK path by hand reads the same file). Only packages with an announced break carry
  one. At 1.26.10.3 that is golib alone, with the 12 entries of M1, every one `IsBaselineSuppression=true`. Each entry
  carries an XML comment naming its ruling: B3, batch 231, and the release notes' *Upgrading* section.
- Any diagnostic that is not in the file is red, so the next unannounced break fails the push and names its member (M3).
- **The file empties itself.** When 1.26.10.3 is published it becomes the baseline, and the B3 entries no longer match
  anything. The guard runs without `--permit-unnecessary-suppressions`, so an entry that matches nothing is itself red
  and has to be deleted. That way a suppression cannot outlive its release.
- The M4 class (§2.4) is suppressed the same way, one line per member, each citing the census that shows golib is the
  only implementer. There is no blanket rule for it.
- Should the guard ride 1.26.10.3? It can, because the file is already generated (M2). That is a release-scope
  decision for COORD; this document only sizes it.

## 5. Coverage

| package set | covered? | why |
|:--|:--|:--|
| `go.lib` | **yes** | every known break is here (B3; M4's interface member) |
| `go.gen` | **no ApiCompat surface**; covered through `go.lib` | it is an analyzer (M6). The code it generates is compiled into the consumer and binds golib members such as `GoTypeOps<S>.Instance`, the member M3 regressed. golib's guard is what protects it |
| the converted stdlib, all packages | **yes, every asset** | the 46 a published module depends on are the minimum (M4, M5). C# consumers that reference go.* directly ([`ConsumingGoFromCSharp.md`](../ConsumingGoFromCSharp.md)) reach the rest, and the cost is minutes (§3) |
| `nugetgo.*` modules | **yes, in `nugetgo-pack.ps1`**, against the module's previous published revision | B4's edge between third-party packages takes a bare minimum version: published cobra names published pflag by a bare version, which NuGet reads as "this revision or later" (the nuspec read 2026-11-07). A consumer who adds pflag's next revision gets it under a cobra built against the earlier one. Seven modules, seconds each |

## 6. The range policy the guard enforces (B4)

**Within one Go minor, every published revision of a go.* or nugetgo.* package is binary-compatible with the
revision before it, except for the breaks listed in that release's suppression files. A release that lists any break:**
1. announces it in the release notes' *Upgrading* section;
2. ships the nugetgo rebuild wave with it, built on that release
   (`PLAN-rebank-1-26-10-3.md` §8, on `claude/g-dot3-prep` until the re-bank);
3. deprecates the earlier revision of every published module on nuget.org, with the new revision as the alternate.
   That step is an owner hand.

- With that rule, B4's `[build, next Go minor)` stays as ruled. It is true for every revision the guard passed with no
  suppression. For an announced break it is untrue for the packages already published, and nuget.org will not let a
  published range be narrowed, so the deprecation in step 3 is what reaches those consumers.
- **Alternatives, both rejected.**
  - Narrowing B4 to the exact build would make every module rebuild at every revision, and two modules built a revision
    apart could no longer be restored together.
  - Signalling a break in the version itself is not possible. The first three numbers are Go's, and the fourth counts
    revisions.
- **Across a Go minor** (for example 1.26 → 1.27) the range already ends, so there is no baseline. The guard reports
  but does not gate, and the first release of the new minor becomes the baseline for its line.

## 7. Size of the cut (not cut)

| piece | where | size |
|:--|:--|:--|
| the per-asset driver: list the baseline's assets, require each one in the current package, run `apicompat` on each pair with the package's suppression file | a new `src/tools/` module, called from `push-nuget.ps1`'s pre-flight and from `-VerifyOnly` | about 150 lines of PowerShell |
| the tool pin | `.config/dotnet-tools.json` | new file |
| golib's B3 suppressions | `src/core/golib/CompatibilitySuppressions.xml` | 12 entries (M2) |
| the module check | `nugetgo-pack.ps1`, against the module's previous published revision | about 30 lines plus the shared driver |
| the negative control | a test that regresses one golib member in a fixture copy and requires the driver to name it (M3's shape), plus a missing-asset case | one test |

---

> **RULED AND CUT, 2026-11-07 (COORD ruling on `761cff788b`): the guard rides 1.26.10.3.** Sections 3-6 were accepted
> as recommended, and §6's policy went into `PLAN-rebank-1-26-10-3.md` §8. The ruling set four conditions:
> (a) the driver must not be fooled by an incremental skip, and must assert that it ran, by a compared-asset count and
> not by exit 0 alone; (b) the negative control is part of the cut and runs under the plain `go test`; (c) the tool is
> pinned in a local tool manifest, at an exact version, and restored by the script; (d) census and repoguard.
> The text above stays as the sizing record. This block says what the cut does and where it differs from the sizing.
>
> **What landed:**
> - `src/tools/pushnuget/GoApiCompat.psm1` is the driver.
>   - Each asset gets its own `dotnet tool run apicompat` process: no MSBuild, so nothing can be skipped.
>   - Each process must print exactly one verdict line, and an asset counts as compared only then.
>   - Each caller requires the compared count to equal the baselines' asset count.
> - `.config/dotnet-tools.json` pins `microsoft.dotnet.apicompat.tool` 10.0.403. `Initialize-GoApiCompatTool` restores
>   it and refuses any other version.
> - `push-nuget.ps1`:
>   - pre-flight (with `-VerifyOnly` too): restores the tool and reads every committed suppression file;
>   - after the merge and the path guard, on **every** pack, not only `-Push`: the guard over every merged package.
>     `release-nuget.ps1` Phase 1 packs without `-Push`, and that pack is the one that ships.
> - `nugetgo-pack.ps1` compares a module with its previous published revision of the same module version, and refuses
>   and removes the package on a break. `-ApiCompatSuppressions` names an announced break.
> - `src/core/golib/CompatibilitySuppressions.xml`: B3's 12 entries, each with a comment citing B3 and batch 231.
> - `src/tools/pushnuget/Test-GoApiCompat.ps1` is the negative control, run by repoguard's `TestNugetgoSuitesPass`
>   under the plain `go test`. Fixtures are in `testdata/apicompat`, built by `build-apicompat-fixtures.ps1`.
>
> **Where the cut differs from the sizing, each measured:**
> 1. **The tool exits 0 on an unnecessary suppression.** It prints "Unnecessary suppressions found" and lists the
>    entries, but does not fail, even without `--permit-unnecessary-suppressions`. So §4's "the file empties itself"
>    holds only because the driver reads that line and counts each listed entry as red.
> 2. **The suppression file is in assembly-mode form, not the SDK's.** The SDK's package-mode entries
>    (`IsBaselineSuppression=true`) do not match in assembly mode: all 12 came back with "Unnecessary suppressions".
>    The cut's entries name the package-relative asset on both sides, through a transformation pattern applied to the
>    scratch paths. The reader refuses a package-mode entry by name, and one that names two assets.
> 3. **Each asset is judged only by its own entries.** One file serves all of a package's assets, so the driver hands
>    each process just the entries for that asset. An entry for an asset the baseline lacks is red.
> 4. **`-VerifyOnly` packs nothing,** so there it proves the instrument (restore plus every suppression file read), not
>    a comparison.
> 5. A module whose version is a Go prerelease or pseudo-version (`L.0.N`, B3) has no baseline under the
>    same-three-numbers rule. It is reported, not gated.
>
> **The gate's readings** (tree as cut; dotnet 10.0.400, apicompat 10.0.403):
> - **The suite reads 28 of 28.** Among its arms: a member made internal reads red, naming
>   `Fixture.Widget.Instance`, CP0002 and its asset, with both assets still compared; restored, it reads green.
> - **The suite reads red when the guard is broken.** Three regressions of the driver, each restored with an identical
>   sha256:
>   - dropping the tool's diagnostics reds 3 arms, the negative control first;
>   - ignoring unnecessary suppressions reds 2;
>   - skipping the verdict count (trap (a)) reds 2: a comparison the tool could not make, and its count.
> - **Real packages**, through the module as `push-nuget.ps1` calls it, against nuget.org:
>   - master's golib against go.lib 1.26.10.2: CLEAN, 1 of 1 asset, with golib's file; RED, the 12 B3 members named,
>     without it. The same at `c6cddb6903` and at the cut's base `6c2a77a4bd`. Batch 236 changed golib's extension-method
>     registry between the two and added no break.
>   - the 38 platform-neutral packages among M4's 46, repacked from the `c6cddb6903` Release build: 38 of 38 compared,
>     0 failures, about 1 s each. Batches 235-237 changed none of the 38 (only archive/tar, crypto/x509 and vendored
>     cryptobyte in the converted stdlib).
