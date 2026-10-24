# PLAN — version lines: which Go release each branch carries, and how a fix reaches each line

> **STATUS: ACCEPTED AS PROPOSED (COORD, 2026-10-24).** The owner approved the frame in §0 on 2026-10-16.
> This plan states that frame and derives what it requires of the branches, the package numbers and the
> release tooling. Of its three questions (§7), OQ-2 and OQ-3 are ruled and OQ-1 is open. It is a
> **strategy plan**: it holds the rules and their rulings. The procedure for a patch hop and for a
> maintenance release lives in the runbook,
> [`GoCorpusMigration.md`](GoCorpusMigration.md) §1.3 and §1.4, and is referenced here, never restated.
<!-- The 2026-10-16 approval as COORD recorded it (claude/coord-handover, docs/phase4/RESUME-SESSIONS.md, the
     "2026-10-16, version lines (owner OK)" entry): "master carries the current Go line, with patch hops on master;
     the next minor hops on a hop branch; at each landing, cut release/go1.N for security fixes only, until Go drops
     that line; hop every Go release; there will NEVER be a Go 1.25 line." Commissioned by COORD to C2 as a
     post-hop docs task on 2026-10-23; drafted 2026-10-24 at hop tip 9dcf0bafe1. -->

---

## 0. The frame

1. **`master` carries the current Go line.** A new patch release of that minor is a **patch hop** on
   master.
2. **The next minor hops on a hop branch** (for example `claude/hop-go126`) and reaches master at the
   runbook's cutover.
3. **When a minor lands on master, the outgoing line gets a `release/go1.N` maintenance branch.** That
   branch takes security fixes only, and only while Go itself supports that minor.
4. **Every Go minor gets a hop.** No minor is skipped from 1.26 onward. There is no Go 1.25 line.

Go supports each minor until two newer minors have shipped
([Go release policy](https://go.dev/doc/devel/release#policy)). Rule 3 therefore gives a maintenance
line a fixed lifetime. It opens when go2cs lands the next minor, and it closes when Go ships the minor
after that.

## 1. The lines and what each branch receives

| Branch | Carries | Receives | Publishes |
|:--|:--|:--|:--|
| `master` | the current line, at its newest adopted patch | all work: converter, golib, docs, validation, patch hops | the current line's releases |
| a hop branch | the next minor, until cutover | the hop's work, and periodic merges from master | nothing; the hop's first release is cut from master after cutover |
| `release/go1.N` | one outgoing line, starting at its last release from master | security fixes only, while Go supports 1.N | that line's security releases |

**Master never merges a maintenance branch, and a maintenance branch never merges master.** A fix
reaches a maintenance branch as a cherry-pick of the commit that already landed on master. A merge in
either direction would carry one line's corpus into the other.

**A maintenance branch starts at its line's last release record**, which is the commit that writes that
release's `src/version.props` counter, its `docs/validation/<version>/` snapshot and its
`src/go2cs/corpus-release.txt`. The release tag is not that commit. The release ritual tags the last
commit *before* the record. A branch started at the tag therefore lacks that release's record: its
counter still names the previous release, so its next publish would compose the version that is
already published, which the release pre-flight's existence check refuses.
<!-- The tag-before-record order is stated in .github/workflows/release-binaries.yml (the header comment: "The release
     ritual mints the tag before it commits the version: the tag sits on the last commit before `release: go2cs
     converted stdlib <version>`"). Measured 2026-10-24 through the GitHub compare API: nuget-1.24.13.7 peels to
     57ca7c1616, whose src/version.props reads counter 6; its record b0ef977fda ("release: go2cs converted stdlib
     1.24.13.7") is one commit ahead and reads counter 7. nuget-1.23.12.3 peels to b6746ab185 (counter 2); its record
     6c03b2c80f is one commit ahead (counter 3). -->

**When a line closes**, its branch stays as the permanent anchor of that line's last release and takes
no further commits. A line closes when Go stops supporting its minor.

## 2. Minting `release/go1.24` at the 1.26 publish

The 1.24 line is already closed: Go stopped supporting 1.24 in February 2026, and
[`Roadmap.md`](Roadmap.md) names 1.24.13.7 as the last 1.24 release. So `release/go1.24` is an anchor
from the moment it is minted, and no 1.24 release follows it unless the owner reopens the line.

**When.** On the 1.26 release day, after master has moved to 1.26 and the first 1.26 release record
has landed, so the release day order in the runbook is unchanged. Nothing about the 1.24 line depends
on the branch existing earlier, because nothing lands on it.

**From what.** The 1.24.13.7 release record, `b0ef977fda` (`release: go2cs converted stdlib
1.24.13.7`), not the tag `nuget-1.24.13.7`, which names its parent (§1).

**How**, from a clone with the tags fetched:

```bash
git fetch origin tag nuget-1.24.13.7
git rev-parse b0ef977fda^1                      # must equal: git rev-parse nuget-1.24.13.7^{commit}
git show b0ef977fda:src/go2cs/corpus-release.txt # must read 1.24.13.7
git push origin b0ef977fda:refs/heads/release/go1.24
git ls-remote --heads origin release/go1.24     # must read the full SHA of b0ef977fda
```

The push is an announced act, the same as any other push to origin. It creates a branch and moves no
tag.

**`release/go1.23` names the 1.23.12.3 record, `6c03b2c80f`.** It was found missing from origin on
2026-10-24 and re-created there the same day (OQ-3), so both closed lines follow the same convention.
<!-- The 2026-09-24 record: claude/coord-handover, docs/phase4/RESUME-SESSIONS.md ("DONE 2026-09-24 (owner-approved;
     ledger "ANNOUNCE · b6746ab185"): the BRANCH release/go1.23 was FAST-FORWARDED from 10c78227a7 ... to b6746ab185,
     the commit of nuget-1.23.12.3, and read back by ls-remote"). It is also on the never-prune list in
     docs/phase4/KICKOFF-fleet.md §1d. The 2026-10-24 reading: `git ls-remote --heads origin` returned master,
     proj/rt-measure and claude/* rows only. The repository events API covers only the newest 300 events (back to
     2026-10-22 at the reading), so it cannot date a deletion. b6746ab185 is the TAG commit, whose counter reads 2;
     the 1.23.12.3 record is its child 6c03b2c80f. Re-created by COORD on the owner's OK (2026-10-24) at 6c03b2c80f,
     a fast-forward of b6746ab185; `git ls-remote --heads origin release/go1.23` read 6c03b2c80f4941cbedc25d39e78c72d6948b6fda. -->

This supersedes the earlier record that `release/go1.24` would be minted at a 1.25 cutover. There is
no 1.25 cutover.
<!-- docs/phase4/NOTES-next-hop.md, item D6: "release/go1.24 is minted at the 1.25 cutover, not before." -->

## 3. Package version numbers per line

Every `go.*` package of a release carries one version, `<Go release>.<build counter>`, read from
`src/version.props`. NuGet reads it as Major.Minor.Patch.Revision.

- **The counter resets to 0 at each base change.** A base change is any pin bump, so this includes a
  patch hop. `migrate-gorelease.ps1 -Apply` performs the reset.
- **The first publish on a base is `.1`.** `push-nuget.ps1 -Push` increments the counter before it
  packs, and counter 0 is never published.
- **Lines never collide.** The version starts with the line's own Go release, so `1.24.13.8` and
  `1.26.11.1` cannot name the same package version.
- **Within a line, versions only increase.** A patch hop raises the third component, and a
  maintenance release on an unchanged base raises the fourth.

| Event | Before | After |
|:--|:--|:--|
| second release on master, same Go patch | `1.26.10.1` | `1.26.10.2` |
| patch hop on master to 1.26.11 | `1.26.10.2` | `1.26.11.1` |
| go2cs-side security fix on `release/go1.26`, after 1.27 lands | `1.26.11.3` | `1.26.11.4` |
| upstream 1.26.12 adopted on `release/go1.26` | `1.26.11.4` | `1.26.12.1` |

The guards already hold this per line in two places:

- `TestPublishedCounterMatchesTheRecordedReleases` compares the counter only against releases recorded
  on the **current base**, at `src/go2cs/internal/repoguard/publishedCounterReset_test.go:203`
  `onBase := releasestamp.OnBase(snapshots, base)`.
- `src/go2cs/corpus-release.txt` names the last release published from the tree it sits in, so on a
  maintenance branch it names that line's release.

**The release pre-flight does not hold this per line.** It refuses a next version that is not newer
than *every* recorded release, on every line, at `src/push-nuget.ps1:567`
`if ((Compare-GoReleaseStamp $next $version) -le 0) {`. It reads origin's tags as well as the local
ones. Once a newer line's first release is tagged, it refuses every later release on an older line.
This is tooling item T1 in §6.

## 4. What a consumer restores

A `-recurse=nuget` conversion writes a `Directory.Build.props` that defaults `GoStdLibVersion` to the
converting toolchain's Go **minor** with a float, for example `1.26.*`, so every restore takes the newest
published release of that minor. The compatibility guard compares the same minor, at
`src/go2cs/toolchainResolution.go:241`
`// Only the language release is compared: 1.23.1 versus 1.23.5 is the same standard library and the`.
So the float matches exactly the releases the guard accepts.
<!-- Ruled by the owner 2026-10-24 (OQ-2, "fix in-hop"), relayed by COORD; the seat is claude/c2-nuget-minor-float.
     Before it, the default was the converting toolchain's full release followed by .* (a Go 1.26.5 toolchain wrote
     1.26.5.*). -->

**Why the minor and not the patch.** A float that matches nothing published resolves to the oldest
package above it, not the newest. Measured on linux-x64 with .NET SDK 10.0.112, against a local feed
holding one package at `1.26.10.1`, `1.26.10.2` and `1.26.11.1`:

| Requested | Resolved |
|:--|:--|
| `1.26.5.*` | `1.26.10.1` (no NU1603 warning printed at the default verbosity) |
| `1.26.10.*` | `1.26.10.2` |
| `1.26.*` | `1.26.11.1` |
| `[1.26.5,1.27)` | `1.26.10.1`, with NU1603 |
<!-- Probe 2026-10-24, C2, in a scratch directory since deleted: a one-class library packed three times as go.probe at
     the three versions into a local folder feed; a net10.0 consumer with a nuget.config clearing every other source;
     NUGET_PACKAGES pointed at an empty per-probe directory; each arm restored after deleting obj/; the resolved
     version read from obj/project.assets.json. A first probe with only 1.26.10.1 on the feed resolved 1.26.5.* to
     1.26.10.1, restore rc 0. Not measured: a windows or darwin SDK, nuget.org itself (a remote feed), or
     PackageReference transitive pins between go.* packages. Through the converter's own emission, same day and
     platform: a no-import module converted by -recurse=nuget, with fake go.lib and go.gen packed at the three versions,
     restored go.lib/go.gen 1.26.10.2 from the hop tip's props (1.26.10.*) and 1.26.11.1 from the seat's (1.26.*). -->

A patch-level float would leave two kinds of project without security fixes:

- one converted with a Go patch older than every published base would restore that minor's **oldest**
  published release and stay there;
- one converted with a Go patch equal to a published base would receive that base's later builds, but
  not a later patch hop.

The minor-level float reaches both. A consumer who wants one exact version sets `GoStdLibVersion`.

## 5. How a security fix reaches each line

A fix has one of three sources, and each source has one route.

| Source | Route | Lines it reaches |
|:--|:--|:--|
| an upstream Go patch release of the **current** minor | a patch hop on master (runbook §1.3) | the current line |
| an upstream Go patch release of a **previous** minor that Go still supports | a patch hop on `release/go1.N`, taken only when the patch fixes a security issue in a converted package | that maintenance line |
| a go2cs defect with security impact, in the converter, golib or a hand-owned file | landed on master first, then cherry-picked to each open maintenance line (runbook §1.4) | the current line and every open maintenance line |

A closed line receives none of the three.

Which lines are open depends on when go2cs lands each minor:

| Line | Opens as maintenance | Closes |
|:--|:--|:--|
| 1.24 | at the 1.26 publish | already closed: Go stopped supporting 1.24 in February 2026 |
| 1.26 | at the 1.27 publish, planned for Q1 2027 | when Go 1.28 ships, expected around February 2027 |

If the 1.27 hop lands after Go 1.28 ships, the 1.26 line closes on the day it opens. The window
lengthens only when go2cs hops sooner after each upstream release. Rule 4 sets that pace.
<!-- Upstream dates as docs/Roadmap.md states them at 9dcf0bafe1 ("Go 1.24 left upstream support in February 2026, and
     the 1.25 series left it in August 2026"; "The 1.26 series leaves support when Go 1.28 ships, around February
     2027"); the 1.27 hop date is Roadmap's "Q1 2027, by about February". -->

## 6. Tooling owed before the first maintenance release

None of these blocks the 1.26 publish, and none is needed while every maintenance line is closed. The
first maintenance release on `release/go1.26` needs T1 to T3. All four are post-hop work, and each is a
row on the [BOARD](phase4/BOARD-next-validation-candidates.md) (2026-10-24, C2).

| # | What | Where it fails today |
|:--|:--|:--|
| **T1** | The pre-flight compares the next version for monotonicity only against releases of the **same Go minor**, and keeps its existence check (an exact version that is already recorded) across every line. | `src/push-nuget.ps1:567` `if ((Compare-GoReleaseStamp $next $version) -le 0) {` refuses it (§3). |
| **T2** | The binaries workflow looks for the release record on the branch that holds the tag, not only on master. | `.github/workflows/release-binaries.yml:105` `for c in $(git log --reverse --format=%H "$tag_commit..origin/master" -- "$release_file"); do` finds no record for a maintenance tag, waits its two hours and fails. |
| **T3** | A maintenance-line GitHub Release is published with *Set as the latest release* cleared. | Asset names carry no version, so `releases/latest/download/<name>` serves whichever Release is marked latest. A maintenance Release marked latest would serve an older converter. |
| **T4** | `release-nuget.ps1` accepts a `release/go1.N` branch without its "not master" warning. | `src/release-nuget.ps1:155` `if ($branch -ne 'master') { Write-Host '    (not master -- intentional?)' -ForegroundColor Yellow }` only warns, so this one is cosmetic. |

The docs site deploys from master, so it documents the current line only. A maintenance release's
NEWS entry lands on master, where readers look for it.

## 7. Open questions

**OQ-1 — what triggers a patch hop on master?**
- (a) Every upstream patch release of the current minor.
- (b) Only a patch release whose source delta reaches a converted package.

*Recommendation: (b), with the delta measured for each patch at H3.* A delta that reaches no
converted package changes nothing a user restores except the version constant in `internal/buildcfg`.
(b) costs one delta reading per upstream patch release; (a) spends a full release cycle on a patch that
changes nothing converted.

**OQ-2 — should `-recurse=nuget` emit a minor-level float (`1.26.*`)? RULED: yes, in the hop.**

The owner ruled it on 2026-10-24, and it is a converter seat inside the 1.26 hop, so the first 1.26
release already ships it (§4). The reasons:
- It is the only one of the four measured forms that resolves to the line's newest release.
- The compatibility guard already treats every patch of a minor as one standard library, so the
  minor-level float claims nothing the guard does not already accept.
- The cost: a project converted with an older patch restores packages converted from a newer one.
  A patch-level float did the same whenever the requested patch was never published.

**OQ-3 — re-create `release/go1.23`? DONE.**

Re-created on 2026-10-24 at the 1.23.12.3 record `6c03b2c80f` (§2), on the owner's OK. That puts both
closed lines on one convention: each branch names its line's last record. It is a fast-forward of the
commit the branch held on 2026-09-24, so no commit the earlier branch named is lost.

## 8. What this plan changes elsewhere, at its ruling

- **[`Roadmap.md`](Roadmap.md), *Go releases***: replace "How often later hops run is decided ahead of
  the Go 1.27 hop" with rule 4, and add one sentence naming the maintenance branches.
- **[`PLAN-corpus-upgrade.md`](PLAN-corpus-upgrade.md) §6**: its row marking `release/go1.23` DONE gets
  a dated note pointing at §2 of this plan.

The runbook half lands with this plan: §1.3 (a patch hop), §1.4 (a maintenance release) and the
*Release day* step that mints `release/go1.<old>` at the outgoing line's final release record (§1).
