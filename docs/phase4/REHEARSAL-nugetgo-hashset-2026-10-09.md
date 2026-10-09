# REHEARSAL — nugetgo PoC steps 3-5 with hashset, LOCAL only (2026-10-09)

**Status: record.** A dress rehearsal of `docs/PLAN-nugetgo.md` section 6 steps 3-5, dispatched by COORD (nugetgo prep while
lanes are idle). Nothing was published: no nuget.org push, no push to any owner repository.
[`rehearsal-nugetgo-hashset/rehearse.sh`](rehearsal-nugetgo-hashset/rehearse.sh) reproduces every arm, with the sample
app's source beside it.

- **Tree:** TRAIN FL's landing head, `claude/coord-trainFL-union` @ `56f0f1f254` (`corpus-release.txt` = `1.24.13.4`,
  `version.props` GoBuildNumber 4). Converter built from that tree. One host (C2, linux-x64), Go 1.24.13, .NET SDK 10.0.112.
- **Module:** `github.com/ritchiecarroll/hashset@v1.0.0` (go.sum `h1:PImqeJq5qbarFfta6ZMXRXozT8qhfW4h4WTZ2VY0WuY=`).
- **Isolation:** every restore used a private `NUGET_PACKAGES`; the user's global packages folder holds 0 `go.*` and 0
  `nugetgo.*` entries after the run. The converter's cache was isolated by `XDG_CACHE_HOME`.

## Results

| Step | Arm | Result |
|:--|:--|:--|
| 1 convert + validate | `go2cs -recurse -tests -test-action all` | **Validated 37** against `go test`, 0 disclosed, 0 skipped. 5 `Example*` declarations (example_test.go) excluded: example execution is deferred to Phase 4D. MODULE.md: 37 matched / 0 disclosed. |
| 2 pack | A: `-Feed` nuget.org (go.lib/go.gen 1.24.13.4) | **RED**, `dotnet pack` exit 1: 58 distinct compile errors, all in `hashset.cs` (CS8130 18, CS0315 17, CS8183 9, CS1579 8, CS0411 3, CS0021 2, CS1729 1). The published go.gen 1.24.13.4 does not read the face-lift marker `partial struct HashSet<T> /*map[T, EmptyStruct]*/;`, so `HashSet<T>` is not a map. |
| 2 pack | B: `-Feed` a local folder holding go.lib + go.gen packed from this tree as 1.24.13.900 | **GREEN**: `nugetgo.github.com.ritchiecarroll.hashset 1.0.0-local.1`, 1 assembly, 1 dependency `go.lib [1.24.13.4, 1.25.0)`, self-description 412 bytes (1 package, 0 requires, `go2cs-release 1.24.13.4`), the author's PROOF description, VALIDATION.md + index.md. Every read-back check passed. NU1603 (approximate match) warned and did not fail. |
| 2 scripts | `Test-NugetgoIdentity.ps1`, `Test-NugetgoSelfDescription.ps1` | 57/0 and 16/0. Neither takes a nupkg: they test the modules on literal tables and a fabricated root. The packed nupkg is validated only by the pack's own read-back. |
| 3 convert app | `-recurse=nuget -nuget-map <local mappings.txt> -nuget-map-only`, fresh root | Mapped, then **converted locally**: the converter asked `api.nuget.org` for the package's versions and got 404. |
| 3 convert app | same, with a hand-seeded `go2cs.nuget.lock` (1.0.0-local.1 + its SHA-512) and the nupkg in the converter's cache | Substituted: ONE `PackageReference nugetgo.github.com.ritchiecarroll.hashset [1.0.0-local.1]`, no `pkg/` tree, provenance table printed, lock rewritten. |
| 3 run | A: go.* restored from nuget.org (all 1.24.13.4) | Built; **run FAILED**: `FileNotFoundException: golib, Version=1.24.13.900`. The packed dll binds the golib it was compiled against; the nuspec's lower bound (1.24.13.4) let restore pick an older one. |
| 3 run | B: go.lib/go.gen 1.24.13.900 from the local feed, the rest of the closure (go.fmt, go.sort, ...) 1.24.13.4 from nuget.org | **GREEN**: output byte-identical to `go run` (6 lines). |

## Gap list — ranked by what blocks the real first-wave publish (hashset, then uuid/jwt per B8)

1. **The publish must come from a PUBLISHED release tree, and nothing enforces it.** Packing at a tree whose golib/gen
   are past the last published release produces a package that cannot compile (arm 2A) or cannot run (arm 3A) against
   that release. Seams: `nugetgo-pack.ps1` derives B4's lower bound from `-ClosureVersion` (lines 95-97), not from the
   go.lib the build restored, and accepts NU1603; `nugetPackage.go:qualifyNuGetPackage` matches only
   `go2cs-release == corpusRelease()`, and `corpus-release.txt` stays `1.24.13.4` while this tree's emission dialect is
   newer. Order that works: FL lands, the next go.* release is cut and PUSHED, then hashset is packed from that tag
   with `-ClosureVersion` = that release. A guard: the pack refuses unless the restored go.lib/go.gen equal
   `-ClosureVersion` exactly (NU1603 as an error), and refuses a tree whose `version.props` is not that release.
2. **The consumer can select a mapped package only from nuget.org.** `nugetPackage.go`: `nugetFlatContainerURL` is a
   fixed `https://api.nuget.org/v3-flatcontainer` (a variable for tests only); `fetchNuGetVersions` and
   `readNuGetPackage` read nowhere else; `nugetPackageCandidates` also refuses a rehearsal-suffixed version. So B7's
   int.nugettest.org rehearsal cannot exercise the consumer half, and a local feed works only through a hand-written
   lock (this record's arm). Needs a source flag (a service index or flat-container URL, or a folder) for selection.
3. **The packed assembly carries host paths.** `positionMapOperations.go:(*Visitor).goSourceIdentity` writes a
   `-recurse` module's source as an ABSOLUTE path (`<GOMODCACHE>/github.com/ritchiecarroll/hashset@v1.0.0/hashset.go`,
   by design: what Go bakes without `-trimpath`); on a Windows publisher that path holds the user profile. The dll's
   debug directory also names the build's `.pdb` path: `nugetgo-pack.ps1` builds without `ContinuousIntegrationBuild`
   or a `PathMap`. A published nupkg needs a `-trimpath` equivalent (`<module>@<version>/<file>`) and a mapped PDB path.
4. **The proof is not bound to the packed bytes.** VALIDATION.md comes from a `-recurse -tests` root; the assembly from a
   separate `-recurse=nuget` root. The two `hashset.cs` differ (Go comments, and so the position map); semantics agree.
   `nugetgo-pack.ps1 -ValidationDir` accepts any MODULE.md: it does not compare the proof's manifest (`inputDigest`,
   `converterRevision`) with the packed tree's.
5. **A stale comment misstates S3.** `commandLineOptions.go:102-103` says the map is "resolved, locked and reported, not
   yet applied (stage S3b)". S3b's substitution is in the tree (`nugetSubstitution.go`, 23a6809206) and substituted in
   arm 3. The comment, not the code, is partial.
6. **The lock records the mapping source's absolute path** (`go2cs.nuget.lock` field 5, the `layer`). A conversion
   repository that commits its lock commits that path. `nugetLock.go:writeNuGetLock`.
7. **MODULE.md omits the excluded declarations.** It reads "37 matched · 0 disclosed"; the 5 Example exclusions appear
   only in index.md. `validationProofPages.go:writeThirdPartyModuleSummary`.
8. **The registry is read with nothing to map.** A `-recurse=nuget` run of a module with 0 third-party modules still
   fetched `https://nugetgo.net/v1/mappings.txt` and warned. `nugetMap.go:runNuGetMapResolution`.
9. **The license warning names a remedy the nugetgo route does not need** (routed here from the i9's windows reading,
   ledger 2026-10-09 01:13). Every conversion of hashset printed "Package license is unspecified ... the module root's
   LICENSE is not packed ... unless it is placed beside the project, or pass -license", yet `nugetgo-pack.ps1` found the
   upstream LICENSE in the module cache (`Find-NugetgoModuleLicense`) and packed it as the package's license file. The
   warning is right for a plain `dotnet pack` of the converted csproj and wrong for the ruled pack.
   `licensing.go:warnUnspecifiedLicense`.

Not measured here: whether nugetgo.net serves the file (this host's proxy refuses the domain); uuid and jwt (B8) were not
packed; the third-party (not-the-author) description form; Windows. Observation, ruled and unchanged: the dll's
copyright keeps the csproj template's years (2018-2026) where the upstream LICENSE says 2021-2026 (the 2026-10-04 skip
rule for The go2cs Authors).
