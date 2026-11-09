# RECORD: how go.* packages pin each other, and what that does to nugetgo packages

> **State: MEASURED (2026-11-09), P2. Options for the owner; nothing was published.** COORD's order of 2026-11-09:
> census the go.* dependency pins as generated and find the ruling that chose them; measure today's exact pins, ranged
> sibling pins and any other shape on a local feed; give each one's restore result, NuGet's picks and the run-time
> risk, with the S1 deprecation's mechanism as the worked example; recommend one, with what it means for the
> binary-compatibility guard (GoApiCompat). Read at master `fb885891b3` with the .NET SDK 10.0.403 on linux-x64.
> Every restore ran under a scratch `HOME` and NuGet cache. The ranged and exact-nugetgo arms used a local folder
> feed of the real nupkgs with their nuspec edges rewritten (unsigned, never pushed). Point-in-time: amend with dated
> blocks, never rewrite.
>
> **Findings:**
> - **The exact sibling pins do not give OQ-2's guarantee.** A mixed go.* graph fails at restore only when two
>   paths that both pin exactly meet with no direct reference between them (NU1107). NuGet's own message then says to
>   reference the newer package directly. Doing that restores a mixed graph (50 go.* packages at 1.26.10.2 beside 69
>   at 1.26.10.3) with warnings only, and it throws `MissingMethodException` before its first output line (§4, a2).
> - **A nugetgo package built before an announced break restores under it silently** whenever every go.* edge it
>   has is satisfied by the newer release. Today's published `hashset` 1.0.0 (revision 4) beside go.fmt 1.26.10.3
>   restores with 0 warnings, and it throws at its first loop over a slice (§4, h1). That is the S1 deprecation's
>   mechanism exactly, and the exact pins cannot see it by construction.
> - **The deprecation does not show at restore.** The S1 pin sheet says it does. On SDK 10.0.403, `dotnet restore`
>   prints nothing for a deprecated package, even at `-v n`. Only `dotnet list package --deprecated`, an IDE's
>   package manager and the nuget.org page show it (§5).
> - **COORD's premise holds at restore and not for correctness.** Under exact sibling pins, every go.* release,
>   breaking or not, makes an older nugetgo package fail restore (NU1107) beside a direct reference to the new
>   release, unless every go.* package it names is already in the graph at the new release (go.lib, in h1). The
>   restore result depends on the version constraints alone, so the measured breaking pair (a1, a4) stands for a
>   non-breaking one too. Central pinning lifts every go.* package to the new release with no warnings (§4, d1). At
>   a release GoApiCompat passed with no suppression, that lifted graph keeps every member the older package binds,
>   so a rebuild wave there is a convenience (inferred: only the breaking pair exists to run). Only an announced
>   break makes the wave a correctness matter.
>
> **Recommendation (§8):** keep the exact sibling pins and B4's range, and add a **build-time break floor** to
> go.lib's targets. The consumer's build then names any referenced go2cs package built before the newest announced
> break, before anything runs. The floor needs no change to an already-published package, because the facts it
> reads already ship in them. GoApiCompat's suppression files then decide when the floor moves (§9).

## 1. The question

go.* packages of one release reach each other through project references in the stdlib pack, and each such
dependency is packed as an exact version, `[1.26.10.3]`. A nugetgo package reaches the go.* packages through
package references, with B4's range `[build, next Go minor)`. COORD's question: if go.* siblings pin each other
exactly, does every go.* release force a nugetgo rebuild wave before a consumer can mix the two? And what would a
range cost, given that B3 (batch 231) changed a member a compiled `foreach` binds?

## 2. Census: the pins as generated, and the ruling that chose them

| Edge | Range as packed | Made by | Chosen by |
|:--|:--|:--|:--|
| go.* → go.* (go.lib included) | **exact**, `[v]` | the `GoExactProjectReferenceVersions` target, `src/core/golib/buildTransitive/go.lib.pack.targets:52-74`: after `_GetProjectReferenceVersions`, it rewrites each project reference's version to `[v]`, unless the property is `false` | commit `4e4c98f74c` (2026-10-26), on **COORD's OQ-2 ruling** in [`DESIGN-crosspkg-exported-box.md`](DESIGN-crosspkg-exported-box.md) §9 |
| nugetgo → go.* | `[closure, next Go minor)` | `src/tools/nugetgo/nugetgo-pack.ps1:173-176` (`$stdlibRange`), written on package references, which the target above leaves alone | **B4**, [`PLAN-nugetgo.md`](../PLAN-nugetgo.md), *OWNER RULINGS 2026-09-30* |
| nugetgo → nugetgo | a bare minimum version, meaning "this revision or later" | the nuspec as packed | B4: "an edge between third-party packages takes the first revision built for the same corpus"; read on the published cobra, [`DESIGN-binary-compat-guard.md`](DESIGN-binary-compat-guard.md) §5 |

**Why exact was chosen.** The design offered three remedies for 1.26.10.2's binary break (each exported value
variable became a `ref` property over a box): (i) pack go.* inter-package dependencies as exact ranges, so that
NuGet never mixes go.* releases; (ii) state the break and have the nugetgo wave rebuild against 1.26.10.2; (iii) a
field-backed box that avoids the break. G recommended (i) with (ii). COORD ruled (i), verbatim: *"exact go.*
inter-package version ranges at pack. A mixed 1.26.10.1/1.26.10.2 go.* graph then fails at restore instead of
throwing `MissingFieldException` at run time."* The target's own comment states the same intent.

**The census of the packed edges** (every nuspec in the scratch cache after the arms below restored):
- 233 go.* packages (revisions 2 and 3 of the 1.26.10 line): all 1362 go.*-to-go.* edges are exact.
- uuid's two revisions: all 36 go.* edges are ranges, `[1.26.10.2, 1.27.0)` at 1.6.0 (revision 2) and
  `[1.26.10.3, 1.27.0)` at 1.6.0 (revision 3).
- hashset 1.0.0 (revision 4) has one dependency, go.lib, with the range `[1.26.10.2, 1.27.0)`.

## 3. How NuGet resolves these edges

Three NuGet rules decide every arm below:
1. **Lowest applicable version.** For a transitive range NuGet takes the lowest version in it. A nugetgo package
   built on 1.26.10.2 therefore pulls go.encoding.json 1.26.10.2, even when 1.26.10.3 is on the feed.
2. **Cousin conflict, NU1107 (an error).** Two paths that pin one package to different exact versions, with no
   direct reference to that package, fail the restore. NuGet names both paths and says to reference the newer
   version directly.
3. **Direct dependency wins, NU1608 (a warning).** A direct reference overrides an exact transitive pin, and each
   overridden pin is reported as "outside of dependency constraint". The restore succeeds.

Rule 3 is why the exact pins catch only some mixed graphs: one direct reference turns a refused graph into a
restored one.

## 4. Measurements

**The consumer.** A console app that calls `strings.ToUpper` and `fmt.Println` from go.* and `uuid.New` and
`UUID.String` from nugetgo uuid, as in the docs audit's walkthrough. Arm h1 uses `hashset.NewHashSet` instead. A
run counts as passing when it prints both lines.

**Sources.** a1-a5, d1 and h1 restore from nuget.org (the published packages as they are). b1, b2 and c1 restore
from a local folder feed alone (`<clear/>`), holding the same nupkgs with their nuspec edges rewritten: b1 and b2
with every go.*-to-go.* edge `[v]` → `[v, 1.27.0)`, and c1 with uuid's go.* edges ranged → `[v]`. Counts are
unique diagnostics; a build log repeats each one. A nugetgo module version is written "x.y.z (revision w)",
inside quoted NuGet output too.

| Arm | Pins | Consumer references | Restore | NuGet picked | Run |
|:--|:--|:--|:--|:--|:--|
| a1 | today's | go.fmt and go.strings 1.26.10.3, uuid 1.6.0 (revision 2), built on 1.26.10.2 | **fails, NU1107** on go.unicode.utf16: `uuid 1.6.0 (revision 2) -> go.encoding.json 1.26.10.2 -> go.unicode.utf16 (= 1.26.10.2)` against go.fmt's path to `(= 1.26.10.3)`; 238 NU1608 | (none) | (none) |
| a2 | today's | a1 plus go.unicode.utf16 1.26.10.3, as NuGet's message says | **restores**, 239 NU1608 warnings | **mixed**: 50 go.* at 1.26.10.2 (go.encoding.json among them) and 69 at 1.26.10.3; go.lib 1.26.10.3 | **throws** before any output (exit 134): `MissingMethodException: Method not found: 'IEnumerator<ValueTuple<IntPtr,!0>> go.IArray<T>.GetEnumerator()'`, raised in `Program.<Main>$` |
| a3 | today's | go.* 1.26.10.3, uuid 1.6.0 (revision 3), built on 1.26.10.3 | clean | all go.* 1.26.10.3 | passes |
| a4 | today's | go.fmt and go.strings 1.26.10.2, uuid 1.6.0 (revision 3) | **fails, NU1107** on go.errors: `go.fmt 1.26.10.2 -> go.errors (= 1.26.10.2)` against `uuid 1.6.0 (revision 3) -> go.errors (>= 1.26.10.3 && < 1.27.0)` | (none) | (none) |
| a5 | today's | a2 with `<WarningsAsErrors>NU1608</WarningsAsErrors>` | **fails**, 239 NU1608 errors | (none) | (none) |
| b1 | go.* siblings ranged `[v, 1.27.0)` | as a1 | **restores, 0 warnings** | mixed, as a2: 50 at 1.26.10.2, 69 at 1.26.10.3 | **throws**, the same exception |
| b2 | go.* siblings ranged | as a3 | clean | all 1.26.10.3 | passes |
| c1 | uuid's go.* edges exact `[1.26.10.2]`; go.* unchanged | as a1 | **fails, NU1107** on go.time (`uuid 1.6.0 (revision 2) -> go.time (= 1.26.10.2)`); 240 NU1608 | (none) | (none) |
| d1 | today's, plus the consumer's central package management with transitive pinning (`CentralPackageTransitivePinningEnabled`), one `PackageVersion` at 1.26.10.3 for each of the 119 go.* packages in the graph | as a1 | **restores, 0 warnings** | **uniform**: all 119 go.* at 1.26.10.3; uuid 1.6.0 (revision 2) | **throws**, the same exception |
| h1 | today's published packages | go.fmt 1.26.10.3, hashset 1.0.0 (revision 4), built on 1.26.10.2, with go.lib its only edge | **restores, 0 warnings** | all go.* at 1.26.10.3 | prints `start`, then **throws**: `Method not found: 'ValueTuple<IntPtr,!0> Enumerator.get_Current()'` at `hashset_package.UnionWith[T]`, from `NewHashSet[T]` (exit 2) |

**What the table says.**
- a1 and a4 are the NU1107 shape COORD asked about, in both directions: an older nugetgo package beside a newer
  go.* release, and a newer nugetgo package beside an older go.* release.
- a2, b1, d1 and h1 all run code built on 1.26.10.2 against go.lib 1.26.10.3, and all four throw the B3 exception.
  The pins decide only how loudly the restore goes first: 239 warnings (a2), no warnings (b1, d1, h1), or an error
  (a1, c1, a5).
- The two named members are both in golib's B3 suppression file (DESIGN-binary-compat-guard.md, M1).
- a2's frame is `Program.<Main>$`, which calls uuid's `New` and `UUID.String`. Those two are the only methods built
  on 1.26.10.2 that `Main` reaches directly, so the failing body is one of them, or one inlined into them. That is
  inferred from the frame; the method was not isolated.

## 5. The run-time risk, with S1's deprecation as the worked example

S1's block 6 ([`nugetgo-1-26-10-3/S1-PIN-SHEET.md`](nugetgo-1-26-10-3/S1-PIN-SHEET.md)) deprecated the twelve
nugetgo revisions built on go.* 1.26.10.1 and 1.26.10.2, for this reason: *"Every revision ... has a go.* range,
`[1.26.10.1, 1.27.0)` or `[1.26.10.2, 1.27.0)`, that admits 1.26.10.3. NuGet therefore restores it under 1.26.10.3
without an error, and it throws `MissingMethodException` at its first such loop."*

- **h1 reproduces that sentence on today's published packages.** hashset 1.0.0 (revision 4) is one of the twelve,
  and it restores with no diagnostic and throws at its first loop over a slice.
- **uuid 1.6.0 (revision 2), also one of the twelve, does not restore silently.** It fails with NU1107 (a1),
  because its go.* edges pull go.* 1.26.10.2 packages whose exact pins meet go.fmt 1.26.10.3's. So whether a stale
  package restores depends on which go.* packages it names, not on its range. The error then leads to a2.
- **The deprecation is not seen at restore.** h1's `dotnet restore -v n` printed no deprecation line.
  `dotnet list package --deprecated` lists `nugetgo.github.com.ritchiecarroll.hashset 1.0.0 (revision 4)`, reason
  Other, alternative 1.0.0 (revision 5) or later. So the sheet's *"A deprecation shows a warning at restore and on
  the package page"* is half true on SDK 10.0.403: the page shows it, and restore and build do not.

**Why a range is not free, and why exactness is not a cure.** A range lets NuGet bind code compiled against one
revision to the members of another. That is safe exactly when the newer revision keeps every member the older one
exported, which GoApiCompat checks for every revision it passes with no suppression. B3 was an announced break, so
any graph that runs pre-B3 code over a post-B3 go.lib throws. Exact pins make some of those graphs fail at restore
(a1), but NuGet's suggested fix (a2), central pinning (d1) and a go.lib-only package (h1) each reach the same
graph with no error.

## 6. What each part of today's design does and does not do

| Claim | Holds? | Evidence |
|:--|:--|:--|
| An older nugetgo package beside a newer go.* release fails restore | **yes**, unless every go.* package it names is already in the graph at the newer release | a1, c1; h1 (go.lib only) is the exception |
| ... and the failure keeps a mixed graph from running | **no**: NuGet's message leads to a mixed graph that restores with warnings | a2 |
| A mixed go.* graph "fails at restore and names them" (OQ-2; the 1.26.10.2 NEWS) | **only without a direct reference** to the conflicting package; with one, the result is NU1608 warnings | a2 |
| A nugetgo package built before an announced break is stopped at restore | **no** | h1 (silent), d1 (silent with central pinning) |
| The deprecation warns at restore | **no** (SDK 10.0.403) | h1 |
| GoApiCompat keeps B4's range honest | **yes for unannounced revisions**: it refuses any break without a suppression entry. For an announced break, nothing a consumer runs reads the suppression | DESIGN-binary-compat-guard.md §6; h1 |

## 7. The options

| Option | Restore when an older nugetgo package meets a newer go.* release | Run-time risk at an announced break | Rebuild-wave cost |
|:--|:--|:--|:--|
| **(a) today:** exact siblings, B4 range on nugetgo | NU1107 (a1), except where every go.* package it names is already at the newer release (h1, silent) | **throws** after NuGet's suggested fix (a2), or with no diagnostic (h1) | a wave at every release for co-restore; needed for correctness only at an announced break |
| **(b) ranged siblings** `[v, next minor)` | **clean, 0 warnings**, mixed graph (b1) | **throws** with no signal at all (b1) | none for co-restore at a release GoApiCompat passed. It mixes go.* revisions inside the stdlib, which never happens in Go, where a stdlib is one release, and which GoApiCompat checks only between adjacent revisions |
| **(c) exact nugetgo → go.* edges** | NU1107 (c1), now also for packages like h1's | **throws** after the same direct-reference fix | a wave at **every** release with no consumer workaround but direct references; two nugetgo packages built a revision apart conflict. Already rejected in DESIGN-binary-compat-guard.md §6 |
| **(d) consumer central pinning** (documented, not shipped) | **clean, 0 warnings**, uniform graph at the newer release (d1) | **throws** (d1) | none for co-restore at a release GoApiCompat passed; the consumer lists every go.* package |
| **(e) consumer NU1608 as an error** (documented) | fails (a5), including the mixes that would be safe | refused at restore, except h1 and d1, which raise no NU1608 | as (a) |
| **(f) build-time break floor** in go.lib's targets (new, §8) | unchanged from whichever pin shape it sits beside | **build error naming the stale package**, before anything runs, in a2, b1, d1 and h1 alike | a wave only at an announced break, as already ruled |
| **(g) no announced break inside a Go minor:** GoApiCompat refuses any suppression except at a hop | unchanged | none: B4's range is true by construction | none inside a minor. A change shaped like B3 waits for the next Go minor, because C# cannot overload `GetEnumerator` on its return type, so B3 had no additive form |

## 8. Recommendation

**Keep (a), add (f), and document (d).**

1. **Keep the exact sibling pins.** They match Go's model, in which one standard library release is one unit, and
   they turn the common cousin conflict into a named restore error. Ranged siblings (b) remove that signal and
   leave the crash (b1). Exact nugetgo edges (c) force a wave at every release and add no safety (c1 still yields
   to a direct reference).
2. **Keep B4's range.** GoApiCompat already makes it true for every revision released with no suppression.
3. **Add a build-time break floor (f).** It closes the hole that every pin shape leaves open (a2, b1, d1, h1):
   - go.lib's `buildTransitive` targets carry `GoBinaryBreakFloor`: the newest go.* release in this Go minor that
     announced a consumer-reachable break (1.26.10.3 today, for B3).
   - A target after `ResolveAssemblyReferences` reads each resolved reference that came from a go2cs package. For
     a go.* package, the release it was built on is its own version. For a nugetgo package, it is the
     `go2cs-release` line of `go2cs/source-metadata.txt`, which every published nugetgo package already ships
     (hashset 1.0.0 (revision 4) reads `go2cs-release 1.26.10.2`). No published package needs a change.
   - Any reference built below the floor fails the build by name, for example:
     *"nugetgo.github.com.ritchiecarroll.hashset 1.0.0 (revision 4) was built on go.* 1.26.10.2, before go.*
     1.26.10.3's binary break (see Upgrading); it would throw MissingMethodException. Reference 1.0.0 (revision 5)
     or later."*
   - It fails a build, not a restore, so a CI build stops before anything ships. An escape property
     (`GoBinaryBreakFloor=false`) keeps the owner's override, like `GoExactProjectReferenceVersions`.
   - It does not reach a consumer whose restored go.lib predates the floor target. The deprecations cover those
     graphs, as now.
   - Sizing, judgement: about 60-100 lines of MSBuild in `go.lib.targets`, a push-nuget step that writes the floor
     (§9), and red-first package tests built from h1 and a2; about 2-3 lane-days. It lands with the next go.lib
     release.
4. **Document (d)** in [`ConsumingGoFromCSharp.md`](../ConsumingGoFromCSharp.md): at a release GoApiCompat passed
   with no suppression, central pinning lifts every go.* package to the new release beside older nugetgo packages,
   with no warnings. With (f) in place, the same recipe across an announced break fails the build by name.
   (e) stays a note, because it also refuses the safe mixes.

(g) is the alternative if the owner prefers to rule out announced breaks inside a Go minor. It makes (f)
unnecessary, at the cost of holding changes shaped like B3 until the next Go minor.

## 9. What this means for GoApiCompat

- **Today** the guard decides whether a revision is compatible with the one before it, and a suppression entry
  records a break that is accepted. Nothing downstream reads that record, so an announced break reaches consumers
  only through release notes and deprecations, which restore does not show (§5).
- **With (f)**, the suppression files decide when the floor moves:
  - A release that adds a **consumer-reachable** suppression entry must set `GoBinaryBreakFloor` to its own
    version. push-nuget refuses a release whose new suppressions and floor disagree.
  - A release with no new suppression keeps the floor, so no wave is owed.
  - Reachability is classified per entry, as the guard's 2026-11-07 whole-surface census already did by hand:
    golib's B3 entries are reachable; the vendored cryptobyte entries (batch 235) are not, because only the standard
    library imports a std-vendored package. That census also reasons that crypto/x509's exact pin refuses a mixed
    restore, which inherits §6's exception: a direct reference would bypass it. The conclusion stands on the import
    rule alone.
- **With (g)**, the guard refuses any suppression file except in the first release of a Go minor. The floor never
  moves, and B4's range is true by construction.
- **With (b)**, the guard would have to prove every pair of go.* revisions in a minor compatible, not only
  adjacent ones, and it still could not see behaviour shared between converted packages of different revisions.
- The nugetgo-to-nugetgo edge (a bare minimum version) is outside this record. The guard's per-module check in
  `nugetgo-pack.ps1` covers it, and the floor could later read a module's own revision the same way.

## 10. Records this contradicts (amendments owed; not made in this seat)

- `nugetgo-1-26-10-3/S1-PIN-SHEET.md`, block 6: "A deprecation shows a warning at restore": restore shows none (§5).
- `DESIGN-binary-compat-guard.md`, the 2026-11-07 whole-surface census block: "go.* packages pin each other
  EXACTLY, so a mixed go.* set fails at restore": true only without a direct reference (§6, a2).
- `DESIGN-crosspkg-exported-box.md`, the 1.26.10.2 NEWS text: "a project that mixes 1.26.10.1 and 1.26.10.2
  packages fails at restore and names them": the same exception. `docs/NEWS.md`, `PLAN-release-1-26-10-2.md` and
  `PLAN-nugetgo-wave-1-26-10-2.md` carry the same sentence.

## 11. Reproduction

Each arm is a directory in the P2 kit: a consumer project, its restore or build log and its run log, with
`project.assets.json` kept for the picks. A small script rewrites the nuspec of each cached nupkg for the local
feeds and drops its signature (modes `ranged` and `ngexact`). Each arm restores under its own scratch `HOME` and
NuGet cache with SDK 10.0.403. The kit is deleted when this record lands.
