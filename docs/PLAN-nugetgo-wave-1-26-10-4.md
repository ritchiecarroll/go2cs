# PLAN — the nugetgo rebuild wave against go.* 1.26.10.4

> **STATUS: DRAFT (P2, 2026-11-10); section 7's questions RULED by COORD the same day.** Asked for by COORD on
> 2026-11-10, after the owner ruled the promoted-forwarder holder move a deliberate .4 break (record:
> `claude/coord-forwarder-review-r2` `88e650c17c`). Document type: an instance plan. The procedure is the runbook
> [`NugetgoPublish.md`](NugetgoPublish.md), and the release's gating policy is
> [`PLAN-rebank-1-26-10-3.md`](PLAN-rebank-1-26-10-3.md) section 8. This file only fixes their values for this wave.
> Where they differ, the runbook leads. It follows [`PLAN-nugetgo-wave-1-26-10-2.md`](PLAN-nugetgo-wave-1-26-10-2.md)
> section for section. Nothing here is published, and nothing is built before `T` exists.

## 1. Why the wave exists

go.* 1.26.10.4 moves every promoted-method forwarder out of a package's `<pkg>_package` class into a holder class per
struct (P1's seat, findings 2-6 of the forwarder review binding). A forwarder is the static method a converted module
calls when it uses a method promoted through an embedded field. CoreCLR refuses to load a type with more than 65,524
methods, and the TypeScript-Go `ast` package's `ast_package` exceeds it at 1.26.10.3. After the move, IL compiled
against 1.26.10.3 that binds `go.X_package::M(...)` throws `MissingMethodException` on 1.26.10.4.

**The break floor turns that run-time failure into a build error.** The seat moves `GoBreakFloor` in go.lib's
`buildTransitive/go.lib.targets` to `1.26.10.4`, and push-nuget's reachable-suppression rule refuses the release
otherwise (`docs/phase4/DESIGN-go-break-floor.md` section 4). A consumer on go.lib 1.26.10.4 then gets `GO2CS3002`
for every nugetgo package built on go.* 1.26.10.3 or earlier, one error per package, whether or not that package's IL
reaches a moved forwarder. The floor reads the package's release, not its IL.

So the wave rebuilds **all nine** published modules, not only those whose IL reaches a forwarder. The IL reach count
P1 is measuring (COORD, 2026-11-10: "still wanted for the record, but no longer gating") does not change the wave's
size. It says which 1.26.10.3 revisions would actually throw for a consumer who sets `GoAllowBelowBreakFloor=true`,
and that belongs in the release notes' *Upgrading* paragraph.

The policy in `PLAN-rebank-1-26-10-3.md` section 8 applies as written to a release that lists a break:

1. the release notes' *Upgrading* section announces it (P1's seat adds the paragraph to `docs/NEWS.md`);
2. the nugetgo rebuild wave ships with the release, built on it (this plan). For 1.26.10.4 that means the same release
   cycle, not the same push (COORD, 2026-11-10): go.* publishes first, the wave is built at the record commit, and the
   release is announced once the wave is green. go.* does not wait for the wave;
3. the owner deprecates the earlier revisions of every published module on nuget.org, with the new revision as the
   alternate (block 6 of each sheet). `GO2CS3002`'s text sends the consumer to "a revision built on go.* 1.26.10.4 or
   later", and the deprecation is what names that revision on the package page.

## 2. The values this wave fixes

| Name | Value |
|:--|:--|
| `R` | `1.26.10.4` |
| `T` | `R`'s record commit (`release: go2cs converted stdlib 1.26.10.4`), cut with `New-NugetgoToolsTree.ps1` (runbook 0.3). It does not exist until release day, so no kit can be built before it. |
| `P` | `linux/amd64`, as in the 1.26.10.3 wave |
| go.* range in every package | `[1.26.10.4, 1.27.0)` (B4) |
| Kit refs | `claude/c2-nugetgo-1261004-<name>`, one per module, holding `conversion/`, `nupkg/` and `KIT.md`, as the 1.26.10.3 wave's `claude/c2-nugetgo-1261003-<name>` refs do |
| Sheets | `phase4/nugetgo-1-26-10-4/S1-PIN-SHEET.md` to `S4-PIN-SHEET.md`, and `Test-PostPublishRestore.ps1` beside them. Not cut yet (section 5). |

Every package is a rebuild of a published package version, so each takes one more B3 revision. The published
revisions were read from nuget.org's flat container on 2026-11-10; each module's newest revision is its 1.26.10.3 one.

| Session | Module | Package ID | Published (built on 1.26.10.3) | This wave |
|:--|:--|:--|:--|:--|
| S1 | `github.com/ritchiecarroll/hashset@v1.0.0` | `nugetgo.github.com.ritchiecarroll.hashset` | hashset 1.0.0.5 | hashset 1.0.0.6 |
| S1 | `github.com/google/uuid@v1.6.0` | `nugetgo.github.com.google.uuid` | uuid 1.6.0.3 | uuid 1.6.0.4 |
| S1 | `github.com/golang-jwt/jwt/v5@v5.3.1` | `nugetgo.github.com.golang-jwt.jwt.v5` | jwt/v5 5.3.1.3 | jwt/v5 5.3.1.4 |
| S1 | `golang.org/x/sync@v0.19.0` | `nugetgo.golang.org.x.sync` | x/sync 0.19.0.3 | x/sync 0.19.0.4 |
| S1 | `github.com/spf13/pflag@v1.0.9` | `nugetgo.github.com.spf13.pflag` | pflag 1.0.9.3 | pflag 1.0.9.4 |
| S1 | `github.com/inconshreveable/mousetrap@v1.1.0` | `nugetgo.github.com.inconshreveable.mousetrap` | mousetrap 1.1.0.2 | mousetrap 1.1.0.3 |
| S2 | `github.com/spf13/cobra@v1.10.2` | `nugetgo.github.com.spf13.cobra` | cobra 1.10.2.2 | cobra 1.10.2.3 |
| S3 | `github.com/itchyny/timefmt-go@v0.1.8` | `nugetgo.github.com.itchyny.timefmt-go` | timefmt-go 0.1.8 | timefmt-go 0.1.8.1 |
| S4 | `github.com/itchyny/gojq@v0.12.19`, the library root only | `nugetgo.github.com.itchyny.gojq` | gojq 0.12.19 | gojq 0.12.19.1 |

The counts each module's proof must read are the `Expect` entries of
[`nugetgo-modules.psd1`](../src/tools/nugetgo/nugetgo-modules.psd1), unchanged (gojq's root 164 + 0, timefmt-go
417 + 0). A rebuild moves the package version and the digests, never a count; a count that moves is a finding, posted
before any sheet is filled. mousetrap has no Go tests, and its proof is its four hosted behavior rows. gojq's `cli`
and its six-module closure stay deferred, as ruled for 1.26.10.3 (OQ-4i); `golang.org/x/sys` still cannot be packed.

All nine registry rows are live at `https://nugetgo.net/v1/mappings.txt` (read 2026-11-10), and no package ID changes,
so no row changes.

**Two modules are Apache-2.0: cobra and mousetrap.** Once the provenance seat lands (batch 281), `nugetgo-pack.ps1`
refuses an Apache-2.0 module whose converted files lack go2cs's `-provenance` notice, and converts its flavors with
`-provenance` itself (Apache-2.0 section 4(b)).

- cobra goes through `Invoke-NugetgoRepack.ps1`, whose step 1.4 passes `-provenance` when the module's `LicenseSpdx`
  names Apache-2.0. Nothing to do by hand.
- mousetrap had no row in `nugetgo-modules.psd1` (no Go tests), so its root was converted outside that driver, and
  the pack would have refused it on release day. `DESIGN-provenance-header.md` named only cobra. COORD gave P2 the fix
  as a seat for batch 282 (`claude/p2-mousetrap-provenance`): mousetrap gets a row, and the driver packs a row with no
  Go tests on the runbook's no-tests route, `-provenance` included.
- Both conversion commits change in every converted file: the notice adds two lines, with a blank line on each side,
  after each file's leading comments. The conversion repositories' next commits show it.

## 3. The order, and the exact-pin rule

**Rule.** No package in this wave may reference a package built against 1.26.10.3 or earlier, and no package is
packed before everything it references exists at its 1.26.10.4 revision.

- **S1, the six leaves, one PIN session.** None references another nugetgo package, so their order inside S1 is free.
  The sheet pushes them in table order.
- **S2, cobra, after S1.** cobra references pflag and mousetrap, so B4's third-party edges move with them:
  from pflag 1.0.9.3 to pflag 1.0.9.4, and from mousetrap 1.1.0.2 to mousetrap 1.1.0.3.
  - cobra is PACKED against S1's two release candidates from a local folder feed, before S1 publishes
    (`Invoke-NugetgoRepack.ps1 -Feed <folder> -ThirdPartyPackage ...`).
  - cobra is SIGNED and PUSHED only after S1's block 5 has listed both edges on nuget.org.
- **S3, timefmt-go.** It references no nugetgo package, so it can run beside S1.
- **S4, gojq, after S3.** gojq's edge moves from timefmt-go 0.1.8 to timefmt-go 0.1.8.1. It is packed against S3's
  release candidate from a local folder feed, and its sheet's block 3 refuses to push before nuget.org lists
  timefmt-go 0.1.8.1.

**The checks that prove the rule.**

- **Restore, for each package** (runbook section 3, the hosted rows): `dotnet list <consumer> package
  --include-transitive` passes only when every go.* package resolves to `1.26.10.4` and every nugetgo.* package
  resolves to this wave's revision from the table above. Any other version is a refusal, posted with the list.
- **Build, new in this wave.** A restore never reaches the floor, because `GoCheckBreakFloor` runs before
  `CoreCompile`. So the post-publish smoke also BUILDS each consumer:
  - each of the nine at this wave's revision beside go.fmt 1.26.10.4 builds with no `GO2CS3002`;
  - the red control: uuid 1.6.0.3 beside go.fmt 1.26.10.4 fails with exactly one `GO2CS3002` naming
    the package `nugetgo.github.com.google.uuid` at uuid 1.6.0.3 and `go.* 1.26.10.3`;
  - the same control with `-p:GoAllowBelowBreakFloor=true` builds, with that diagnostic as a warning.

## 4. Release day, in order

Each step is the runbook's, in the order `PLAN-rebank-1-26-10-3.md` section 8 set for 1.26.10.3.

1. **Preconditions (runbook 0).**
   - P1's forwarder seat has landed, with `GoBreakFloor` at `1.26.10.4`, and push-nuget's `-VerifyOnly` has read the
     floor as `.4` and the guard as 0 unsuppressed (P2 is its second reader).
   - The go.* push (release-nuget Phase 3) is done, and nuget.org's flat container lists `1.26.10.4` for go.lib,
     go.gen and every go.* package the nine closures reference.
   - The record commit exists (Phase 4), and `New-NugetgoToolsTree.ps1` cuts `T` from it.
   - `1.26.10.4` is listed in the registry repository's `cmd/sitegen/go2cs-releases.txt` (runbook 5, step 1), so the
     CI resolution check can read packages built on it.
2. **S1's six kits and S3's kit (runbook 1-3).** For each module, one run:
   `Invoke-NugetgoRepack.ps1 -Module <M@V> -Revision <N> -T <T> -Release 1.26.10.4 -Work <new dir>`. mousetrap, with
   no Go tests, runs twice: `-Packs int1`, then, after its hosted rows are written against that rehearsal package,
   `-Packs release -BehaviorProofDir <rows>`.
   - Each run exits 0, with every count equal to the module's `Expect`.
   - mousetrap's release candidate carries its four hosted rows.
   - Each kit is committed to its `claude/c2-nugetgo-1261004-<name>` ref.
3. **cobra's kit, from the local feed.** The same driver, with `-Feed` set to a folder holding S1's pflag and
   mousetrap release candidates beside go.* 1.26.10.4, and the 1.26.10.3 wave's `-ThirdPartyPackage` argument with
   its two versions moved to pflag 1.0.9.4 and mousetrap 1.1.0.3.
   Run it without root's file capabilities and with a writable private module cache (runbook Traps, 12).
4. **gojq's kit, from the local feed.** The whole module validated (`-test-timeout` of about 50 minutes), then the
   root packed with `-ExcludePackage github.com/itchyny/gojq/cli` and
   the 1.26.10.3 wave's `-ThirdPartyPackage` argument with its version moved to timefmt-go 0.1.8.1.
5. **The restore check of section 3, for all nine.** Then COORD rules sections 1-3 green.
6. **The conversion repositories (runbook 4.5).** Each of the nine conversion commits and its `nuget-<PV>` tag is
   pushed.
7. **The sheets (owner), in order:** S1, then S2 once S1's block 5 lists pflag and mousetrap; S3 (any time after its
   kit is ruled green), then S4 once S3's block 5 lists timefmt-go. Block 6 of each, the deprecation, lists every
   1.26.10.x revision below this wave's (section 6).
8. **The post-publish smoke**, restore and build (section 3), with the red control.
9. **The registry (runbook 5).** No row changes. The CI resolution check is re-read against the new revisions.
10. **The sample-program demo (runbook 6)**, against the new revisions.

## 5. What is ready now, and what waits

| Item | State |
|:--|:--|
| This plan | DRAFT, for COORD's ruling |
| The S1-S4 sheets | C2 cuts them (COORD, 2026-11-10), from this plan: the 1.26.10.3 sheets with the release, revisions, kit refs, edges and stale lists moved, a placeholder for every kit and signer SHA, and section 3's build arm folded in. `Test-SheetPaste.ps1` must read 0 findings on each. |
| `Test-PostPublishRestore.ps1` | The 1.26.10.3 script reads the release and the kits from the sheets beside it, so a copy needs no edit for the values. The build arm and the floor control of section 3 are new code. |
| The release-note paragraph | P1's seat (COORD's item 3). The IL reach count, when P1 has it, sizes its "which old revisions would throw" sentence. |
| The kits | Wait for `T`, and so for P1's seat and the go.* push |
| mousetrap's `-provenance` route | P2's seat `claude/p2-mousetrap-provenance`, for batch 282, after the provenance seat (batch 281) |

## 6. The deprecation lists

Each sheet's block 6 deprecates every 1.26.10.x revision of its module, with this wave's revision as the alternate.
Under the 1.26.10.4 floor, all of them fail the consumer's build. The revisions built against go.* 1.24.13
(hashset 1.0.0 and hashset 1.0.0.2, uuid 1.6.0, jwt/v5 5.3.1, x/sync 0.19.0, pflag 1.0.9) cap their range at `1.25.0`
and are left out, as in the earlier waves.

| Module | Built on 1.26.10.1 | 1.26.10.2 | 1.26.10.3 | Alternate |
|:--|:--|:--|:--|:--|
| hashset | hashset 1.0.0.3 | hashset 1.0.0.4 | hashset 1.0.0.5 | hashset 1.0.0.6 |
| uuid | uuid 1.6.0.1 | uuid 1.6.0.2 | uuid 1.6.0.3 | uuid 1.6.0.4 |
| jwt/v5 | jwt/v5 5.3.1.1 | jwt/v5 5.3.1.2 | jwt/v5 5.3.1.3 | jwt/v5 5.3.1.4 |
| x/sync | x/sync 0.19.0.1 | x/sync 0.19.0.2 | x/sync 0.19.0.3 | x/sync 0.19.0.4 |
| pflag | pflag 1.0.9.1 | pflag 1.0.9.2 | pflag 1.0.9.3 | pflag 1.0.9.4 |
| mousetrap | mousetrap 1.1.0 | mousetrap 1.1.0.1 | mousetrap 1.1.0.2 | mousetrap 1.1.0.3 |
| cobra | cobra 1.10.2 | cobra 1.10.2.1 | cobra 1.10.2.2 | cobra 1.10.2.3 |
| timefmt-go | | | timefmt-go 0.1.8 | timefmt-go 0.1.8.1 |
| gojq | | | gojq 0.12.19 | gojq 0.12.19.1 |

The 1.26.10.3 wave deprecated the 1.26.10.1 and 1.26.10.2 revisions with the 1.26.10.3 ones as alternates. Deprecating
them again here moves their alternate to the 1.26.10.4 revision, so no package page points at a revision the floor
refuses.

## 7. Open questions for COORD, and the rulings (COORD, 2026-11-10)

1. Does the 1.26.10.4 release wait for the wave, as section 8's policy item 2 says ("ships with it")? COORD's note of
   2026-11-10 says the wave "follows .4's publish". This plan assumes the 1.26.10.3 order: go.* push, record commit,
   wave, and the release announced when the wave is ready.
   **RULED:** the 1.26.10.3 order. go.* does not wait for the wave; "ships with it" means the same release cycle,
   before the announcement. Section 1 says so.
2. Who cuts the four sheets: C2 by precedent, or P2 now as templates?
   **RULED:** C2, from this plan. The build arm of section 3 is adopted, and C2 folds it into the sheets.
3. gojq's `cli`: still deferred for 1.26.10.4 (this plan's default), since `golang.org/x/sys` cannot be packed?
   **RULED:** deferred.
