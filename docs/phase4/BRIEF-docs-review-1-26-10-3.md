# BRIEF — the owner's user-facing docs review after the 1.26.10.3 publish

> **State: DRAFT for COORD (G, 2026-11-07), on `claude/g-dot3-prep`.** This is the brief a reviewer works from. The
> review gates the wide announcement of 1.26.10.3, and it runs after the publish. Nothing here dispatches it. COORD
> names the tip and the reviewers when it does.

## 1. What the review is for

A visitor arriving from the announcement reads these pages cold. The review reads them the same way: a fresh-eyes
reader with no repository context, at the published tip. Every claim on these pages has to be true of what that
visitor can install and run today.

**Read at:**
- the 1.26.10.3 record commit, the commit at which `src/go2cs/corpus-release.txt` reads `1.26.10.3`, or the tip COORD
  names;
- for the site, `https://go2cs.net` after its Pages build from that commit;
- for the packages, nuget.org as it lists `1.26.10.3`.

**Not in scope:** the process records (`PLAN-*`, `DESIGN-*`, `docs/phase4/`, the runbooks, the BOARD), which the
site already excludes. Converter behaviour is in scope only where a page claims it.

## 2. The pages

| # | page | where a visitor meets it | what to look at hardest |
|--:|:--|:--|:--|
| 1 | `docs/README.md` | the site's home page | The first screen: what go2cs is, what it is not, the version line, the install steps. Each badge and count against its source |
| 2 | The site's other visitor pages: `Features`, `FAQ`, `CommandLine`, `DotnetTool`, `BuildingGo2cs`, `ValidatedModules`, `ValidatedTestPackages`, `Performance`, `DiagnosticCodes` | the site | Commands that run as written. Flags that exist at the tip. The roster pages' counts against their own source, never against a summary |
| 3 | `ConversionStrategies.md` and the 35 pages of `ConversionStrategies-Reference/` | the site | Every C# sample is the emission at the tip. 1.26.10.3 changed `foreach` over a slice (B3), so every loop sample is read against it |
| 4 | `ConsumingGoFromCSharp.md` | the site | The B3 loop forms (`foreach` gives values; `range(s)` gives pairs; `range(len(s))` gives indexes) and the package-consumer setup, read as someone writing C# against `go.*` for the first time |
| 5 | `ConvertingAModule.md` | the site | The module flow at the tip: `-recurse` modes, `-tests -recurse` with plain `-recurse` only, the lock files, the output root, the library-first scope |
| 6 | `NugetgoPublish.md` | GitHub only (the site excludes it) | The steps as a module author would follow them. The owner-only steps are marked as such. The library-first scope line |
| 7 | `Tuning.md` | the site | Every setting it names exists, and every number names its platform and host |
| 8 | `KnownIssues.md` and `Limitations.md` | the site | Nothing fixed by 1.26.10.3 is still listed. Each item that remains is true at the tip. Each has a workaround, or says it has none |
| 9 | `Roadmap.md` | the site | "Now / Next / Unscheduled" against what 1.26.10.3 shipped. gojq and timefmt-go move from "next" to shipped if the wave published them. Every date is a quarter the owner set |
| 10 | `NEWS.md`, the 1.26.10.3 entry, and the GitHub Release body | the site; GitHub Releases | No `PENDING` marker or HTML-comment instruction is left in the published text. Each item's claim against its batch. *Upgrading* comes first and is complete |
| 11 | nugetgo.net: the registry repository's `README.md` and `CONTRIBUTING.md` (`ritchiecarroll/nugetgo`) and the site page | nugetgo.net and GitHub | The registry's rows against nuget.org. The contribution steps against `NugetgoPublish.md` |
| 12 | The conversion repositories' READMEs (`*-cs` under the owner's account: hashset, uuid, jwt, sync, pflag, mousetrap, cobra, and timefmt-go and gojq if they shipped) | GitHub | Each README's tag links resolve to `nuget-<version>` at the 1.26.10.3 revision. The "not affiliated" wording is present. The build steps work |
| 13 | Package pages on nuget.org: `go.lib`, `go.gen`, one neutral go.* package, one RID-specific go.* package (for example `go.os`), and each nugetgo package | nuget.org | The README as nuget.org renders it, the badges, VALIDATION.md, the dependency ranges, and the deprecation notice on each 1.26.10.2 nugetgo revision |

## 3. What a fresh-eyes reader checks, on every page

| check | the question | a finding looks like |
|:--|:--|:--|
| **Truth at the tip** | Does the tip (or nuget.org, or the site) do what the sentence says? | a flag, a file, a package version or a behaviour the tip does not have |
| **Present tense** | Does the page say what is, not what was or will be? | "will", "now supports", "was changed", "new in", outside NEWS |
| **Every claim names its platform** | Does a measured number, a speed, a size or a behaviour say on which OS, architecture and host it was read? | "about 36% faster" with no platform |
| **No stale counts** | Is each count read from its source of record at the tip, or removed? | a roster count, a test count or a package count that differs from its source, or that the page has no source for |
| **Working links** | Does each link resolve, and is a versioned link pinned to a tag? | a 404; a link to `HEAD` where a tag is meant; a link into an excluded record |
| **One reading** | Can a newcomer misread it? | jargon with no definition; the fleet's own names (lanes, batches, seats) on a visitor page |

The repository's own docs guards (`src/go2cs/internal/repoguard/docs*_test.go`) already check internal links and
fragments, links into excluded records, Jekyll and kramdown rendering, and the provenance comments. The review
therefore spends its time on truth, tense, platform and counts, and on external links, which no guard fetches.

## 4. How findings come back

- **One findings table per reviewer**, posted to COORD, in this form:

  | page | anchor or line | the claim, quoted short | check (from section 3) | what is true at the tip, and where it was read | severity |
  |:--|:--|:--|:--|:--|:--|

- **Severity:**
  - **BLOCKS** the announcement: a false claim, a broken install step, or a broken link on the home page or a
    package page;
  - **BEFORE**: fixed before the announcement, but it does not stop it;
  - **NIT**: tone or wording.
- **COORD triages.** Fixes land as one docs batch per page group, each through the docs guards. The reviewer
  re-reads only the pages a fix touched, at the new tip.
- **A finding states what the tip does, with its source:** the file and commit, the command and its output, or the
  nuget.org URL. "This looks wrong" is a question, not a finding.
- **The announcement waits on zero BLOCKS** and COORD's call on the BEFOREs.
