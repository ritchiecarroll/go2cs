# PLAN — the nugetgo rebuild wave against go.* 1.26.10.2

> **STATUS: DRAFT (C2, 2026-11-03).** Its scope was ruled by COORD on 2026-11-03: prepare the wave now, and publish
> nothing until 1.26.10.2 ships. Document type: an instance plan. The procedure is the runbook
> [`NugetgoPublish.md`](NugetgoPublish.md), and this file only fixes its values for this wave. Where the two differ,
> the runbook leads.

## 1. Why the wave exists

go.* 1.26.10.2 changes how one package reads another package's variable. A package-level `var` becomes storage that
is reached through an accessor, not a field. An assembly compiled against 1.26.10.1 must be rebuilt against 1.26.10.2
before it runs on it. The release note states this for the standard library, and it holds for every nugetgo package
the project has published against 1.26.10.1.

**The restore does not catch a stale nugetgo package.** go.* 1.26.10.2 packages depend on each other at the exact
release version, so a project that mixes go.* 1.26.10.1 and 1.26.10.2 fails at restore and names them. A nugetgo
package, however, references go.* as a range from its build to the next Go minor (ruling B4, `PLAN-nugetgo.md`
section 8). A package built against 1.26.10.1 reads `[1.26.10.1, 1.27.0)`, and that range admits 1.26.10.2. A consumer
on go.* 1.26.10.2 therefore restores the stale package without an error, and it can fail at run time on a
cross-package variable read.

> **AMENDED 2026-11-09 (P2, measured in `docs/phase4/RECORD-nuget-sibling-pins.md` §4, SDK 10.0.403).** "Fails at restore and names them" holds only
> when two dependency paths pin one go.* package to different exact versions and the project references that package
> directly on neither (NU1107). NuGet's NU1107 message says to reference the newer package directly. That reference
> overrides the exact pins with NU1608 warnings, and the mixed set restores and builds: arm a2 restored 50 go.*
> packages at 1.26.10.2 beside 69 at 1.26.10.3 with 239 NU1608 warnings, then threw `MissingMethodException` at run
> time. The paragraph's point about nugetgo packages stands; only its premise about go.* is narrower than written.

What this wave does about it (COORD ruling 2026-11-03):

- (a) This plan and the 1.26.10.2 release note say so, now.
- (b) Deprecating the stale revisions on nuget.org is prepared as an OPTIONAL owner step in both sheets. The owner
  decides on release day; it is a public act.
- (c) A B4 change, so that a dialect-breaking release's packages cannot be resolved by a later release they would
  break on, is post-hop. It is its own BOARD row, owner C2.

## 2. The values this wave fixes

| Name | Value |
|:--|:--|
| `R` | `1.26.10.2` |
| `T` | `R`'s record commit (`release: go2cs converted stdlib 1.26.10.2`), cut with `New-NugetgoToolsTree.ps1` (runbook 0.3). It does not exist until release day, so no kit can be built before it. |
| `P` | `linux/amd64`, as in the 1.26.10.1 wave |
| go.* range in every package | `[1.26.10.2, 1.27.0)` (B4) |
| Kit refs | `claude/c2-nugetgo-1261002-<name>`, one per module, holding `conversion/`, `nupkg/` and `KIT.md`, as the 1.26.10.1 wave's `claude/c2-nugetgo-wave3-<name>` refs do |
| Sheets | [`phase4/nugetgo-1-26-10-2/S1-PIN-SHEET.md`](phase4/nugetgo-1-26-10-2/S1-PIN-SHEET.md) and [`S2-PIN-SHEET.md`](phase4/nugetgo-1-26-10-2/S2-PIN-SHEET.md), templates until release day |

Every package is a rebuild of a published package version, so each takes one more B3 revision. The published
revisions were read from nuget.org's flat container on 2026-11-03.

| Session | Module | Package ID | Published (built on) | This wave |
|:--|:--|:--|:--|:--|
| S1 | `github.com/ritchiecarroll/hashset@v1.0.0` | `nugetgo.github.com.ritchiecarroll.hashset` | 1.0.0.3 (1.26.10.1) | 1.0.0.4 |
| S1 | `github.com/google/uuid@v1.6.0` | `nugetgo.github.com.google.uuid` | 1.6.0.1 (1.26.10.1) | 1.6.0.2 |
| S1 | `github.com/golang-jwt/jwt/v5@v5.3.1` | `nugetgo.github.com.golang-jwt.jwt.v5` | 5.3.1.1 (1.26.10.1) | 5.3.1.2 |
| S1 | `golang.org/x/sync@v0.19.0` | `nugetgo.golang.org.x.sync` | 0.19.0.1 (1.26.10.1) | 0.19.0.2 |
| S1 | `github.com/spf13/pflag@v1.0.9` | `nugetgo.github.com.spf13.pflag` | 1.0.9.1 (1.26.10.1) | 1.0.9.2 |
| S1 | `github.com/inconshreveable/mousetrap@v1.1.0` | `nugetgo.github.com.inconshreveable.mousetrap` | 1.1.0 (1.26.10.1) | 1.1.0.1 |
| S2 | `github.com/spf13/cobra@v1.10.2` | `nugetgo.github.com.spf13.cobra` | 1.10.2 (1.26.10.1) | 1.10.2.1 |

The counts each module's proof must read are the `Expect` entries of
[`nugetgo-modules.psd1`](../src/tools/nugetgo/nugetgo-modules.psd1), unchanged. A rebuild moves the package version
and the digests, never a count; a count that moves is a finding, posted before any sheet is filled. mousetrap has no
Go tests, and its proof is its four hosted behavior rows.

The older revisions of each ID (hashset 1.0.0 and 1.0.0.2, uuid 1.6.0, jwt/v5 5.3.1, x/sync 0.19.0, pflag 1.0.9) were
built against go.* 1.24.13 and cap their range at `1.25.0`. They cannot resolve under 1.26 at all, so they are not part
of the stale-package problem.

## 3. The order, and the exact-pin rule

**Rule.** No package in this wave may reference a package built against 1.26.10.1, and no package is packed before
everything it references exists at its 1.26.10.2 revision.

- **S1, the six leaves, one PIN session.** None of the six references another nugetgo package, so their order inside
  S1 is free. The sheet pushes them in table order.
- **S2, cobra, after S1.** cobra references pflag and mousetrap, so B4's third-party edge ("the first revision built
  for the same corpus") moves with them: pflag 1.0.9.1 to 1.0.9.2, mousetrap 1.1.0 to 1.1.0.1.
  - cobra is PACKED against S1's two release candidates from a local folder feed, before S1 publishes. That is the
    runbook's local-feed route, as in the 1.26.10.1 wave: `Invoke-NugetgoRepack.ps1 -Feed <folder> -ThirdPartyPackage ...`.
  - cobra is SIGNED and PUSHED only after S1's block 5 has listed both edges on nuget.org. The S2 sheet's block 3
    refuses otherwise.

**The restore check that proves the rule.** For each package, the runbook's consumer restore (section 3, the hosted
rows) lists its whole closure with `dotnet list <consumer> package --include-transitive`. The check passes only when:

- every go.* package in that list resolves to `1.26.10.2`;
- every nugetgo.* package in it resolves to this wave's revision from the table above.

A closure that names any other go.* or nugetgo.* version is a refusal, posted with the list.

## 4. Release day, in order

Each step is the runbook's; this list fixes the values and the order.

1. **Preconditions (runbook 0).**
   - nuget.org's flat container lists `1.26.10.2` for go.lib, go.gen and every go.* package the seven closures
     reference.
   - The record commit exists, and `New-NugetgoToolsTree.ps1` cuts `T` from it with every pack guard 1.26.10.2 lacks.
     The 1.26.10.1 wave's guards are all on master, so `T` is expected to be the record commit itself.
2. **S1's six kits (runbook 1-3).** For each module, one run:
   `Invoke-NugetgoRepack.ps1 -Module <M@V> -Revision <N> -T <T> -Release 1.26.10.2 -Work <new dir>`.
   - The run must exit 0, with every count equal to the module's `Expect`.
   - mousetrap's release candidate carries its four hosted rows.
   - Each kit is committed to its `claude/c2-nugetgo-1261002-<name>` ref.
3. **cobra's kit, from the local feed.** The same driver, with `-Feed` set to a folder that holds S1's pflag and
   mousetrap release candidates beside go.* 1.26.10.2, and
   `-ThirdPartyPackage github.com/spf13/pflag=nugetgo.github.com.spf13.pflag@1.0.9.2,github.com/inconshreveable/mousetrap=nugetgo.github.com.inconshreveable.mousetrap@1.1.0.1`.
   Run it without root's file capabilities (`capsh --drop=cap_dac_override,cap_dac_read_search`) and with a private
   module cache made writable (`chmod -R u+w`): Go itself fails `TestDeadcodeElimination` when it cannot create
   `test_deadcode/` in a read-only module directory (runbook Traps, 12).
4. **The restore check of section 3, for all seven.** Then COORD rules sections 1-3 green.
5. **The conversion repositories (runbook 4.5).** Each of the seven conversion commits and its `nuget-<PV>` tag is
   pushed.
6. **The S1 sheet (owner).** The sheet's placeholders are filled from the kits, it is pasted, and its block 5 lists all
   six packages. Block 6, the deprecation, is optional and the owner's decision.
7. **The S2 sheet (owner).** It is filled and pasted the same way.
8. **The registry (runbook 5).** A registry row maps a module to its package ID, and no ID changes in this wave, so no
   row is expected to change. The CI resolution check is re-read against the new revisions.
9. **The sample-program demo (runbook 6)**, against the new revisions.

## 5. What is ready now, and what waits

| Item | State |
|:--|:--|
| This plan | DRAFT, for COORD's ruling |
| The S1 and S2 sheets | Templates. `Test-SheetPaste.ps1` reads 0 findings on each. The signer commit is a named placeholder, and each sheet's block 1 refuses while any placeholder is left. |
| The deprecation step | In both sheets as block 6, optional, "owner decides". Each block re-checks that nuget.org lists the new revision and prints the page, the version and the message; it changes nothing itself. |
| The release-note line | Sent to COORD for the held 1.26.10.2 docs branch |
| The kits | Wait for `T` |
| `Test-PINGates.ps1` | The 1.26.10.1 wave ran it against the six release candidates. It needs the candidates, so it runs on release day. |
