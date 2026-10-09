# Publishing a converted Go module to nuget.org

> **STATUS: DRAFT (C2, 2026-10-09). Its four open questions were RULED by COORD on 2026-10-09 and are folded in
> below; the document as a whole is not yet ruled, and nothing is executed from it until it is.**
> Document type: a [runbook](Glossary.md#runbook). It is the procedure for any one module, on any go2cs release, and
> names no release or module. The first wave's values (hashset, uuid, jwt/v5) and its open questions are the instance
> plan [`PLAN-nugetgo-first-wave.md`](PLAN-nugetgo-first-wave.md). The rulings it follows (B1-B8: publisher, IDs,
> versions, ranges, metadata, rehearsal, first wave) are in [`PLAN-nugetgo.md`](PLAN-nugetgo.md), section 8.

The procedure takes one validated Go module to a signed `nugetgo.*` package on nuget.org, a row in the nugetgo
registry, and a sample program that restores the package instead of converting the module. Steps run in order, and a
failed gate stops the run.

**Names used below.**

| Name | Meaning |
|:--|:--|
| `R` | the go.* release the package is built against (its `ClosureVersion`) |
| `T` | the tools tree: the go2cs commit whose converter and nugetgo scripts run every step (section 0.3) |
| `P` | the target platform, `os/arch`, passed as `-platforms` to every conversion of the module and recorded |
| `M`, `V` | the Go module path and its version, for example `example.com/mod` and `v1.2.3` |
| `ID`, `PV` | the package ID and package version, from `Get-NugetgoPackageId` and `Get-NugetgoVersion` (B2, B3) |
| `SRC` | the module's source directory: `go mod download -json M@V`, its `Dir` field |
| `G` | one go2cs binary built from `T`, used for every conversion of this module |

**Who runs what.** A *lane* runs the conversions, the packs and the consumer runs, on a lane box. The *owner* runs
every step that needs an account or a key: creating the conversion repository, signing, both pushes, and the registry
pull request. *COORD* reads each gate's evidence and rules before the owner's irreversible step.

## 0. Preconditions

Stop at the first one that fails.

1. **`R` is published.** nuget.org's flat container lists `R` for `go.lib`, `go.gen` and every `go.*` package the
   module's closure references: `https://api.nuget.org/v3-flatcontainer/go.lib/index.json` and so on. A version
   appears there only after nuget.org's validation, some time after the push.
2. **`R`'s record commit exists.** `release-nuget.ps1` mints the tag `nuget-R` in its Phase 1 and regenerates
   `src/go2cs/corpus-release.txt` in Phase 4, so at the tag that file still names the previous release. The record
   commit (`release: go2cs converted stdlib R`) is the first commit at which it reads `R`.
3. **`T` is `R`'s record commit with every nugetgo pack-guard branch that `R` does not already contain merged onto
   it, never rebased** (COORD ruling, 2026-10-09: master is not `T` when the train after `R` changes golib). When `R`
   contains every guard, `T` is the record commit itself. `src/tools/nugetgo/New-NugetgoToolsTree.ps1` cuts `T` and
   refuses, by name, each of the following:
   - a `corpus-release.txt` at the record commit that does not read `R`;
   - a guard branch that does not merge cleanly;
   - any change to `src/core/golib` or `src/gen` between the record commit and `T`, because `T` must emit `R`'s dialect;
   - a nugetgo suite that fails or does not run (`TestNugetgoSuitesPass` printing SKIP).

   The script never pushes. The pack's own guards prove the rest at pack time: the published closure against `R`'s
   go.lib and go.gen, the host-path scan of the packed bytes, and the proof's input digest.
4. **The host reaches the services.** `proxy.golang.org`, `api.nuget.org`, `apiint.nugettest.org`, `nugetgo.net` and
   `github.com`. The toolchain is the corpus Go (`go version`), with `GOTOOLCHAIN=local`, the .NET SDK and pwsh 7.
   Free disk is preflighted as for any battery.
5. **`ID` is free on nuget.org.** This reads 404, or an index listing only this project's earlier versions (`id` is
   `ID` in lower case):
   ```
   curl -s -o /dev/null -w '%{http_code}' https://api.nuget.org/v3-flatcontainer/<id>/index.json
   ```
   If another account holds `ID`, stop: which ID a displaced conversion takes is not ruled (B5's named gap).
6. **The conversion repository exists and is public (owner).** It is the package's `RepositoryUrl` (B6). For a
   `canonical` row it sits under the same org on the same host as `M` (PLAN section 2); otherwise the row is
   `community`.

## 1. Convert and validate (lane, at `T`)

1. Build `G` once: `go build -o <bin>/go2cs .` in `T`'s `src/go2cs`. Every conversion of this module uses this one
   binary, because the input digest records the converter (trap 3).
2. Convert with the module's tests, into a fresh output root `VR` (the output directory is the SECOND positional):
   `G -recurse -tests -test-action all -platforms P -go2cspath <T>/src <SRC> <VR>`.
3. **Gate: the proof.** `VR/validation/M/MODULE.md` exists, every package row carries an `Input digest`, and its
   disclosed and excluded declarations are read. COORD reads MODULE.md: it is what the package ships as
   VALIDATION.md.
4. Convert the pack's input into another fresh root `PR`: `G -recurse=nuget -platforms P -go2cspath <T>/src <SRC> <PR>`.
5. **Gate: the binding.** Each library project under `PR/src/M` records a `GoInputDigest` equal to its MODULE.md row.
   The pack checks this again; reading it here catches a wrong binary before a pack runs.

## 2. Pack (lane, at `T`)

Two packs from the same two roots: the rehearsal for int.nugettest.org, and the release candidate. Neither is signed
and neither is pushed by this step.

```
pwsh -NoProfile -File <T>/src/tools/nugetgo/nugetgo-pack.ps1 -ModulePath M -GoVersion V -Revision 0 `
  -RecurseRoot PR -ValidationDir VR/validation/M -ClosureVersion R -Feed https://api.nuget.org/v3/index.json `
  -OutDir <out-rehearsal> -Scratch <scratch-rehearsal> -RepositoryUrl <conversion repo> -Upstream <holder> `
  [-UpstreamPublishes] [-ExcludePackage <import path>] -RehearsalSuffix int.1
```

The release candidate is the same command with `-Release` in place of `-RehearsalSuffix int.1`, and its own `-OutDir`
and `-Scratch`. `-UpstreamPublishes` is given only when the module's author publishes (B6); the pack refuses the switch
for any other org. `-ExcludePackage` names, by import path, a library package of the module that holds the module's
own test fixtures rather than API a consumer imports; the instance plan names any such package. The pack refuses an
exclusion that names no library of the module, and one that a packed package references. The rehearsal label
`int.1` is ruled (COORD, 2026-10-09).

**Gate: both packs exit 0.** The pack itself refuses, by name: a host path in any packed file, a restored `go.*`
closure that is not the published `R`, a proof whose input digest is not the packed project's, and any read-back
mismatch of identity, description, copyright, ranges or self-description.

## 3. Rehearse on int.nugettest.org (B7)

1. **Owner:** push the REHEARSAL package, unsigned, to the test gallery with that gallery's own API key. The
   release candidate never goes there.
   ```
   dotnet nuget push <out-rehearsal>/<ID>.<PV>-int.1.nupkg --source https://apiint.nugettest.org/v3/index.json
   ```
2. **Lane:** wait until int's flat container lists `PV-int.1`.
3. **Lane:** convert the module's sample program (named in the instance plan). The registry has no row yet, so a
   local mappings file carries the row, and the feed flag points selection at the test gallery:
   ```
   G -recurse=nuget -nuget-map <mappings.txt> -nuget-map-only \
     -nuget-map-feed https://apiint.nugettest.org/v3/index.json -go2cspath <T>/src <app> <AR>
   ```
4. **Gate: the consumer half.** The provenance report reads `1 referenced as packages`, with
   `PackageReference <ID> [PV-int.1]`. `go2cs.nuget.lock` pins `PV-int.1` and records the mapping source relative to
   `AR`. No directory for `M` exists under `AR/src` or `AR/pkg`: the module was not converted locally.
5. **Gate: the program.** Restore and run with a private `NUGET_PACKAGES` and a nuget.config that maps `nugetgo.*`
   to the test gallery and everything else to nuget.org (`packageSourceMapping`). `dotnet run` prints exactly what
   `go run` prints, byte for byte.
6. Record the readings (date, host nickname, `T`, both digests, the two outputs) in the instance's record.

## 4. Publish to nuget.org (owner; irreversible)

COORD rules sections 1-3 green before this step.

1. **Sign** the release candidate with the certificate registered to B1's account, as `release-nuget.ps1` Phase 2
   does: `dotnet nuget sign` with the card. nuget.org rejects an unsigned package from that account.
2. **Verify:** `dotnet nuget verify --all <ID>.<PV>.nupkg`.
3. **Push:** `dotnet nuget push <ID>.<PV>.nupkg --source https://api.nuget.org/v3/index.json`. A published version
   can be unlisted but never deleted.
4. **Lane:** wait until the flat container and the registration list `PV`. The registry's checks read both.
5. **Owner:** push the conversion source to the conversion repository: the contents of `PR/src/M`, including the
   `Directory.Build.targets` the pack wrote there, so that a rebuild from the repository produces the same assembly
   attributes.

## 5. The registry row (owner, through the nugetgo repository's CI)

1. If `R` is not listed in the nugetgo repository's `cmd/sitegen/go2cs-releases.txt`, a maintainer adds it in its
   own pull request first. The row's pull request must change only `v1/mappings.txt` to be eligible for the
   `auto-merge-eligible` label, and check 4(b) refuses a package built against an unlisted release.
2. Open the row's pull request: one line in `v1/mappings.txt`, in sorted order, six TAB-separated fields (module path,
   `ID`, the claimed status, the conversion repository's https URL, the date, a contact), with the pull request
   template's checklist filled in. The pull request states `P`, which check 4(c) converts for.
3. **Gate: the CI.** The `ci` workflow runs lint, then checks 2 to 4(b) on the row (existence, provenance, the
   self-description's module and release), then the label job. A `canonical` row that passes every check gets
   `auto-merge-eligible`; a `community` row never does, and waits for review.
4. **Check 4(c), by a maintainer** (the procedure ACCEPTED by COORD on 2026-10-09, and stated in the nugetgo
   repository's CONTRIBUTING). The self-description's sections are the packed packages' surface records
   (`GoTypeAlias`, `GoImplement`, `GoRefPrimary`, `GoSStringTwin`), so a fresh conversion must reproduce them exactly:
   1. Extract `go2cs/source-metadata.txt` from the package version checks 2 to 4 read, and read its `module`,
      `module-version`, `go2cs-release`, `require` and `package` lines.
   2. Build go2cs at the tag `nuget-<go2cs-release>`. The pack guards do not change these records, so the tag
      reproduces what `T` wrote.
   3. Convert `<module>@<module-version>` into an empty root `FR` with the `P` the pull request states:
      `go2cs -recurse=nuget -platforms P -go2cspath <go2cs>/src <SRC> <FR>`.
   4. In `src/go2cs`, regenerate the self-description: `go run ./internal/gensourcemeta -src <FR>/src -module <module>
      -module-version <module-version> -go2cs-release <go2cs-release> -out fresh.txt`, with one
      `-package <import path>=<assembly>` per `package` line and one `-require <module>@<version>=<nuget-id>` per
      `require` line.
   5. The check passes when `fresh.txt` equals the package's `go2cs/source-metadata.txt` byte for byte. A difference
      fails the row; attach the diff to the pull request.
5. A maintainer merges. The Pages workflow republishes the site; wait until `https://nugetgo.net/v1/mappings.txt`
   serves the row.

## 6. The sample-program demo (lane)

PLAN section 6, step 5: the launch demo and the standing integration test.

1. From a clean clone at `T`, with a fresh go2cs cache (`XDG_CACHE_HOME`, or `%LOCALAPPDATA%` on Windows), a private
   `NUGET_PACKAGES` and NO `-nuget-map` flag, so the default registry answers:
   `G -recurse=nuget -go2cspath <T>/src <app> <DR>`.
2. **Gate.** The provenance report lists `M` with its status and the registry URL as its source, and
   `PackageReference <ID> [PV]`. `go2cs.nuget.lock` records the registry URL and the content hash of the package
   downloaded from nuget.org. Nothing for `M` is converted locally.
3. **Gate.** `dotnet run` prints exactly what `go run` prints.
4. Record the run as for section 3.

## Traps

1. **The release tag is not the release record.** A converter built at `nuget-R` has the previous release in
   `corpus-release.txt` and refuses the package's self-description as another corpus release. Build `T` at or after
   the record commit.
2. **A tree whose golib moved past `R` cannot pack against `R`.** Its emission calls golib API the published
   `go.lib` lacks, so the compile against the published `go.gen` fails (the 2026-10-09 hashset rehearsal, arm 2A,
   recorded on `claude/c2-nugetgo-rehearsal`). The published-closure guard refuses an unpublished `go.lib` or `go.gen`
   by name before compiling; the diff gate in 0.3 catches the move before any conversion.
3. **One binary per module.** `GoInputDigest` includes the converter revision, so a rebuild of `G` between 1.2 and
   1.4 makes the pack refuse the proof. Rebuild, then redo both conversions.
4. **Comments do not enter the digest** (COORD ruling, 2026-10-09). `-tests` always converts with comments and the
   pack's root need not, so the two emissions differ in line layout while their digests agree.
5. **A rehearsal version is never published to nuget.org.** go2cs admits a `-RehearsalSuffix` version only from a
   configured `-nuget-map-feed`. The suffix also keeps test-gallery bytes out of every NuGet cache under the release
   version.
6. **Signing on the test gallery is not measured.** Whether int.nugettest.org accepts a package signed with a
   certificate registered only on nuget.org has not been tried; section 3 pushes the rehearsal unsigned.
7. **Frames of a packed module print the `-trimpath` form** (`module@version/file.go`; owner approval, 2026-10-09).
   Until a go.* release carries the runtime half of the host-path change, a process with `GO2CS_DEFAULT_GOROOT` set
   prints that form under the GOROOT's `src` directory instead.
8. **A cloud host may not reach the test gallery or nugetgo.net.** On 2026-10-09 the cloud container's proxy refused
   both, so sections 3 and 6 run on a lane box.
