# DESIGN: a build-time break floor in go.lib's targets

> **State: PROPOSED (2026-11-09), P2; design only, nothing is cut.** The owner ruled "Yes for 'break floor' in .4"
> (relayed by COORD, 2026-11-09). This note settles the shape before the seat: the property names, the error text,
> where the floor value lives and who moves it, how the targets find each package's release, and what a project
> with no go2cs package sees. The measurements behind it are in `docs/phase4/RECORD-nuget-sibling-pins.md` (on
> `claude/p2-nu1107-pins`; its §4 arms a2, b1, d1 and h1 are the graphs this floor exists to stop). Read at master
> `0bbe3ab6e5`. Point-in-time: amend with dated blocks, never rewrite.

## 1. The rule

A C# build that references go2cs-built packages fails, naming each package, when one of those packages was built
below the **break floor**: the newest go.* release that changed a member compiled code binds. Today the floor is
go.* 1.26.10.3 (B3, batch 231: `slice<T>.Enumerator.get_Current` and `IArray<T>.GetEnumerator` changed shape).

- The exact go.* sibling pins and B4's nugetgo range stay as they are. The floor adds a check after restore; it
  changes no package version, range or restore result.
- It reads only facts the published packages already ship (§5), so no package is republished.
- It fails the **build**, before anything runs, not the restore: a package's targets are imported only once
  restore has finished, so a check go.lib ships can run at build and no earlier.

## 2. Names

| Name | Kind | Set by | Meaning |
|:--|:--|:--|:--|
| `GoBreakFloor` | property | go.lib's `buildTransitive/go.lib.targets`, unconditionally | the floor, a go.* release version such as `1.26.10.3`. Package targets import after the project body, so a project cannot lower it by setting the property |
| `GoAllowBelowBreakFloor` | property | the consumer | `true` turns each error into a warning with the same code and text, for a consumer who accepts the risk. Anything else, or unset, keeps the error |
| `GO2CS3002` | diagnostic | the target `GoCheckBreakFloor` | a package built below the floor. One diagnostic per package. Registered as `Go2csDiagnosticBreakFloor` in the build-side registry block of go.lib.targets, with its row in `docs/DiagnosticCodes.md` (`TestGo2csDiagnosticCodeRegistry` enforces both) |
| `GoCheckBreakFloor` | target | go.lib.targets | runs `BeforeTargets="CoreCompile"`, like `Go2csCompilerFloor`, and not in a design-time build (`'$(DesignTimeBuild)' != 'true'`), so an IDE's background evaluation shows no error the real build would not |

One code, raised as an error or, under `GoAllowBelowBreakFloor=true`, as a warning, keeps `NoWarn` and
`WarningsAsErrors` working the usual way.

## 3. The error text

For a converted module (any package that ships `go2cs/source-metadata.txt`):

```text
error GO2CS3002: <id> <version> was built on go.* <built-on>, below go.* $(GoBreakFloor), the newest release that
changed members compiled code binds (see "Upgrading" in the go2cs release notes). Run, it would fail with
MissingMethodException or a similar error. Reference a revision of <id> built on go.* $(GoBreakFloor) or later, or set
GoAllowBelowBreakFloor=true to build anyway at your own risk.
```

For a go.* package, whose release is its own version:

```text
error GO2CS3002: go.encoding.json 1.26.10.2 is a go.* 1.26.10.2 package, below go.* $(GoBreakFloor), the newest
release that changed members compiled code binds (see "Upgrading" in the go2cs release notes). Run, it would fail with
MissingMethodException or a similar error. Reference go.* $(GoBreakFloor) or later for it, or for the package that
brings it in, or set GoAllowBelowBreakFloor=true to build anyway at your own risk.
```

`<built-on>` reads `an unknown release` when a converted module's metadata has no `go2cs-release` line, and that
package counts as below the floor (§5).

## 4. Where the floor lives, and who moves it

**The value** is one literal in `src/core/golib/buildTransitive/go.lib.targets`:
`<GoBreakFloor>1.26.10.3</GoBreakFloor>`. A move is a reviewed one-line diff, in the release that makes the break.

**push-nuget moves nothing; it refuses a release whose floor is wrong.** After the binary-compatibility guard passes
(`DESIGN-binary-compat-guard.md`), every suppression entry left in the tree is a break against the previous published
revision, because the guard refuses an entry that matches nothing. So the guard's suppression map already says
whether this release breaks. With R the release version and F the tree's floor:

| This release | push-nuget requires | Why |
|:--|:--|:--|
| any **consumer-reachable** package carries a suppression entry | F = R | the release breaks compiled code a consumer can hold |
| none does, and the baseline go.lib carries a floor | F = the baseline's floor | no break, so no move and no rebuild wave |
| none does, and the baseline go.lib carries no floor (the first release with this target, 1.26.10.4) | F ≤ the baseline's version, and F on R's Go line | the floor names a release already published |
| no baseline on this Go line (the first release of a Go minor) | F = R | everything built on the earlier minor is below it, as B4's ranges already say |

A failure names the case, F and R. It runs under `-VerifyOnly` and dry runs, as the guard does.

**Consumer-reachable** follows Go's import rule. A package is reachable unless its Go import path has an `internal`
element or starts with `vendor/`: only the standard library can import those, so no converted module binds them.
golib is reachable. By this rule the 1.26.10.3 release would have moved the floor for golib's twelve B3 entries, and
the vendored cryptobyte entries (batch 235) alone would not have.

- The residual: a consumer who overrides the exact sibling pins by a direct reference (NU1608 warnings) can hold a
  mixed go.* graph across an unreachable break. That is the NU1608 path the record measured. The floor still names
  every go.* package below it in such a graph, so the residual is a mix across an internal-only break.
- The conservative alternative, held: count every suppression entry. It moves the floor, and with it a nugetgo
  rebuild wave, for breaks no converted module can reach.

## 5. How the target finds each package's release

After `ResolvePackageAssets`, every compile and runtime asset carries `NuGetPackageId`, `NuGetPackageVersion` and
`PathInPackage` (read on arm h1's graph: 69 compile items and 69 runtime items, hashset's and go.lib's among them).
`GoCheckBreakFloor` takes the distinct packages among `@(ResolvedCompileFileDefinitions)` and
`@(RuntimeCopyLocalItems)` and reads, for each:

1. **A converted module**: the package root is the asset's full path minus `PathInPackage`, the same arithmetic
   `GoSelectRidCompileAssets` uses. If `<root>/go2cs/source-metadata.txt` exists, its `go2cs-release <version>` line
   is the release the package was built on (`ReadLinesFromFile`). Every packed module writes the file
   (`internal/gensourcemeta`, format v1, `PLAN-nugetgo.md` §5). Read in the scratch cache: uuid 1.6.0 (revision 2)
   reads `go2cs-release 1.26.10.2`, revision 3 reads `1.26.10.3`, and hashset 1.0.0 (revision 4) reads `1.26.10.2`.
   The seat reads every published nugetgo revision the same way before it relies on the line. The test is the
   file, not the `nugetgo.` prefix, so a module a user packs under another ID is checked too.
2. **A go.* package** (ID starts with `go.`, the same literal and the same caveat as `GoSelectRidCompileAssets`): its
   own `NuGetPackageVersion`.
3. Any other package is not go2cs-built and is skipped.

Versions compare with `$([MSBuild]::VersionLessThan(...))` on the part before any `-`, so a prerelease of release R
counts as built on R. A package built on an earlier Go minor (1.24.13) is below a 1.26 floor, which is right: its
range caps below 1.25.

## 6. A project with no go2cs package

Nothing happens. go.lib.targets reaches a project only through go.lib's `buildTransitive`, so a project that
references no go2cs package never imports it. A project that references golib as a project (the corpus, the
converter's templates) never imports `buildTransitive` either. A project that imports it and resolves no package
below the floor sees nothing, not even a message at normal verbosity.

## 7. What the floor does not cover

- **A consumer whose restored go.lib predates the target** (1.26.10.3 or earlier). The deprecations stay the only
  signal there, and restore does not show them (the record, §5).
- **Breaks between two nugetgo modules** (cobra on pflag): B4's bare-minimum edge. The per-module guard in
  `nugetgo-pack.ps1` stays the check. The same target could later read a module's own revision floor.
- **A mixed go.* graph at one floor**, across releases GoApiCompat passed with no suppression: safe by the guard's
  verdict, and outside this check by design.

## 8. The seat, red-first

1. **Property-level red** (repoguard, `go test`): a probe project imports the working-tree go.lib.targets and runs
   `GoCheckBreakFloor` alone over hand-made items and package roots: a module below the floor (error naming it), one
   at the floor (clean), one with no `go2cs-release` line (error, unknown), a go.* package below (error), a non-go2cs
   package (skipped), the opt-out (warning, exit 0), no items (silent). Red before the target exists; the
   deliberate-regression check of the safety floor's item 13 on the comparison.
2. **The registry**: `Go2csDiagnosticBreakFloor` and the `docs/DiagnosticCodes.md` row
   (`TestGo2csDiagnosticCodeRegistry`).
3. **The package fixture** `src/tests/PackageTests/BreakFloor/` with its script, on one local feed (the published
   nupkgs it needs, plus golib packed from the tree as a prerelease of the next revision): hashset 1.0.0 (revision 4)
   with go.fmt 1.26.10.3 fails with GO2CS3002 naming hashset (the S1 mechanism, arm h1, as the worked red); a module
   built on 1.26.10.3 builds and runs; the opt-out builds with the warning.
4. **push-nuget**: the four cases of §4 as a function in `GoApiCompat.psm1`, called after the guard, with cases in
   `Test-GoApiCompat.ps1`. That script runs under pwsh here; its Windows PowerShell 5.1 arm needs a Windows host.
5. **Docs**: the property and the code in `ConsumingGoFromCSharp.md`; the floor in the 1.26.10.4 release notes'
   *Upgrading* section; a KnownIssues line only if a case of §7 is judged one.
6. **Gates**: `go test` (TestLicensing*, TestDocs*, the registry), repoguard, Test-GoApiCompat, the golib pack, and
   the fixture.

## 9. Size

About 2-3 lane-days, judgement: the target and registry about 80 lines of MSBuild; the probe test about 200 lines of
Go; the push-nuget function and its cases about 120 lines of PowerShell; the fixture and its script; the docs. The
one unknown is §5's first step on every published nugetgo revision; the seat reads them first and amends this note
if any lacks the line.
