# REFRESH-PLAN: the public docs other than Background.md

**Read at:** `8fbc1b0a132c501c305e9c8620750a62c8a75d23` (TRAIN N's assembled union). Every quote below was
taken from `git show <sha>:<path>`; line numbers are at that SHA. Nothing in the tN worktree was read or touched.

**How to apply:** one docs seat on TRAIN N's union once it is pushed. Re-check each quoted line at the seat's base
before editing; if a quote no longer matches, the union moved, so re-read that paragraph instead of forcing the edit.

**Revision 2 (review pass):** amended from three reviews; each finding's disposition is in `REVIEW-NOTES.md` beside
this file. New items: P9 (Performance memory bullet) and RM8 (Roadmap Platforms). R11, R14b and RM3 now carry
preconditions that need a reading before they land.

**Classes:**
- **SAFE**: no checker reads this text. Apply it as written, polishing wording if needed.
- **CHECKED-SITE**: a checker reads this text. Either keep it VERBATIM, or change the named checker in the same seat.
- **NEEDS-OWNER**: a judgment call. A recommendation is given; apply it only on the owner's word.

**The owner's rules, applied throughout:**
- Write in present tense.
- No history unless it is needed for clarity or marks a real transition (ANTLR to Go).
- Use short, plain sentences for a first-time reader.
- Where possible, drop exact figures, versions and dates.
- No host, machine or user names, and no local paths.
- No TypeScript.
- Records stay untouched: `docs/validation/**`, `docs/phase4/**`, `docs/doctrine/**`, `PLAN-*.md`,
  `RoadmapHistory.md` and `ValidatedTestPackages.md`.

**The first-wave template tweak** (drop "If you would rather this conversion not be listed") does not appear
anywhere at this SHA (`git grep` finds no match). It belongs to the first-wave drafts and is not part of this seat.

---

## 0. Seat-wide notes

### 0.1 Two corrections to the inventory

1. **`../` links do not 404 on go2cs.net.** `.github/site-outside-links.py` runs after the Pages build and rewrites
   every href that leaves `docs/` but stays inside the repository to its GitHub URL at the built commit, keeping
   the #fragment. Consequences:
   - The 38 `](../src/...)` links in `docs/ConversionStrategies.md` need **no change**.
   - The `../CLAUDE.md` links in README, Architecture and the Reference README are stale because of what they
     point at, not because they break. Their fixes below are about content.
   - `site-check-baseline.json` lists none of them, which confirms they resolve.
2. **The Performance table has 14 rows, and a 15th benchmark already exists in the tree.**
   `src/tests/Performance/PerfTlsHandshake` is present, but it is not yet in the published results. Roadmap L259
   says the re-baseline "adds a TLS-handshake benchmark". See RM5.

### 0.2 Checker changes this seat owes in the same change (src/migrate-gorelease.ps1)

Retire each site by keeping the row and adding `Retired` plus `RetiredNote`, as the precedents at ps1:355-356 and
:417-418 do. A retiring commit cannot name its own SHA, so:
- **Preferred:** a descriptive `Retired` string, as ps1:488-495 does, for example
  `Retired = 'the docs-refresh seat (README Status rewrite)'`. The census only prints `Retired` (ps1:707-708,
  :758-766), so nothing checks it, and a descriptive string survives a rebase or re-cut of the seat.
- Alternatively, land the docs edits as commit (a) and the ps1 retirements as commit (b), with
  `Retired = '<sha of (a)>'` (the `e43b8f3cda` precedent), filling in the SHA only after the seat's final rebase.

**Why the retirements land in the same commit series as the prose edits:** at the current pin (no `-To`), the census
marks a non-retired site whose count is not its `Expect` as `mismatch` and exits 1 (ps1 about :723 and :812). So
check 0.4 step 2 goes red in the seat itself as soon as the anchored prose is gone. This applies to R13, RM7, CS2 and
the Background rewrite alike.

| Site (ps1 lines) | Anchor | Triggered by | Action |
|:--|:--|:--|:--|
| 4 (380-386) | `packages, Go {OLD}\) compiles cleanly` | R13 | Retire. RetiredNote: "the docs refresh states the compile claim without a count or release; nothing moves at a hop" |
| 7 (428-434) | `` `go build` \(Go {OLD}\) `` | RM7 (recommended) | Retire, or skip RM7 and keep the text VERBATIM |
| 10 (449-455) | `> Go {OLD}\) wherever possible` | CS2 (recommended) | Retire, or skip CS2 and keep the text VERBATIM |
| 8 (435-441), 9 (442-448) | Background.md's two anchors | the **Background rewrite**, not this plan | Retire in the seat that lands Background (see Appendix C). The census exits 1 at the current pin as soon as the anchored prose is gone, so retire them in the same commit series. |
| history anchors (480-483) | `All \*\*302\*\* packages \(Go {OLD}\)...`, `Go {OLD}'s terminal validation marker` | R16 (if the owner takes the milestone trim) | Optional: these are spelled at 1.23.1, so they already match zero times and produce only a warning. Retiring them with a note keeps the census quiet. |

**Also amend the comment at ps1:348-349**, which reads "The present-tense corpus release is stated in
docs/README.md, docs/ValidatedTestPackages.md, docs/Background.md and docs/ConversionStrategies.md, each anchored
below; nothing is unstated." It already omits docs/Roadmap.md, although Roadmap site 7 (ps1:428-434) is active, so
dropping names alone leaves it wrong. Rewrite the list to what the seat leaves:
- docs/README.md and docs/ValidatedTestPackages.md, always;
- plus docs/Roadmap.md if RM7 is skipped;
- plus docs/ConversionStrategies.md if CS2 is skipped.

**Soft reference, no edit owed:** GoCorpusMigration.md H12 (about :3480) says the Go version appears in "the
README, roadmap, roster, background and strategy docs". It stays true enough. Leave the runbook alone unless the
owner wants that list tightened.

### 0.3 What must stay VERBATIM (checked sites and inbound anchors)

**docs/README.md:**
- The NEWS block, from L12 (`## 📰 NEWS — ...`) through L25 (`All announcements can be found`).
  `check-roster-format.ps1` §2e requires `225 of the 230 testable` and checks the other figures in the block. The
  exact figures there are a dated announcement by design.
- L96 `**Go 1.24.13** source`, the L98 table header, and the six `golang/go/blob/go1.24.13/` links at L100-105
  (sites 1-3).
- L157 `**[Go 1.24.13](https://go.dev/dl/)** — the converter is a Go program` (site 6).
- L489 `**[Go 1.24.13](https://go.dev/dl/)** (for the reference` (site 5).

**Headings whose anchors other pages target.** Keep these exactly; site-check fails on a broken #fragment:

| Page | Headings |
|:--|:--|
| README | `### Try it yourself — validate a converted test suite`, `### Converting a real-world module`, `## Status`, `## Milestones`, `### Real standard-library conversions, side by side`, `#### Optional: build against a local standard library`, `## Usage`, `### Performance`, `### Common options` |
| Roadmap | `## Platforms`, `### Declared host limits and their retirement path` (DotNetMigration.md:801 quotes it), `## Performance` |
| src/tests/Performance/README.md | `## History`, `### What the AOT column costs to produce — the honesty footnote` |
| TestingInfrastructureRequirements.md | the `#phase-4d--later-test-kinds` anchor |

**Other constraints:**
- `<!-- PERF-RESULTS:BEGIN/END -->` and everything between them stays untouched (generated).
- KnownIssues.md keeps its name: the comment every emitted csproj carries (csproj-template.xml:58) links it, and
  csprojTemplate_test.go:553 checks that the template's comment contains `docs/KnownIssues.md`. No checker reads the
  page's content, but it keeps its fault / cost / flip-trigger content because that comment points readers here for it.
- CONTRIBUTING.md section 2 stays section 2 (cla.yml).

### 0.4 Verification after the edits (in the seat's worktree)

1. Repo guards:
   `cd src/go2cs && go test -count=1 -timeout 20m -run "TestDocs|TestNoFleetIdentifiers|TestFleet|TestContextBudget" ./internal/repoguard/`.
   Then run the plain `go test ./...` that every lane runs, with an explicit `-timeout`.
2. `pwsh src/migrate-gorelease.ps1 -Quiet` exits 0. Each retired row reports `RETIRED at ...`, and no active
   DOC-STATEMENT row reports a count mismatch. Run it on the committed tree.
3. `pwsh src/check-roster-format.ps1` exits 0. §2e must still find the untouched NEWS block.
4. `git diff --no-index src/tests/Performance/README.md docs/Performance.md` shows only the trailing AUTO-COPIED
   banner, as it does at this SHA.
5. Run the docs-site checks on the seat's branch **before it lands**. `.github/workflows/docs-site.yml` does not
   trigger on lane-branch pushes (its triggers are `push` to master, `pull_request` and `workflow_dispatch`; its
   header says "Lane branches are not triggers"). So run `gh workflow run docs-site.yml --ref <seat-branch>`
   (`workflow_dispatch` builds and checks any ref; the deploy job is skipped off master), or open a PR. Confirm
   site-check reports `0 new` against the baseline. Several edits are checked only here: R4's in-page link, every
   link to `#mapping-modules-to-nuget-packages`, and every rewrapped relative link. Left to the master push, a new
   broken #fragment would fail the build job and hold the Pages deploy.
6. `git grep -n "1\.24\.13" -- docs/README.md docs/Roadmap.md` lists only these expected leftovers:
   - the verbatim sites in 0.3: sites 1-3 (README L96, L98, L100-105), site 5 (L489) and site 6 (L157);
   - the NEWS block, README L14 and L21 (a dated announcement, held by check-roster-format §2e);
   - the milestone-table tags and notes: R16's kept 2026-10-01 row carries `nuget-1.24.13.3`, and if R16 is not
     taken, the rows at L577-581 stay. These are dated history, REVIEW-only in the census, never an exit failure;
   - anything else the owner chose to keep (for example RM7's sentence, if RM7 is skipped).
7. Link text of every relative `.md` link sits on one line (TestDocsLinkTextDoesNotWrap). If a wrap is needed,
   break the line before the `[`.

---

## 1. docs/README.md (HIGH). It is both the GitHub landing page and the go2cs.net home page.

**R1. L242-244, the Target Atlas sentence (stale: "now being planned").** SAFE.
- Current: "Wondering which real-world Go packages make good conversions? The **[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html)** surveys the Go ecosystem's best candidates — the study behind the first operational package conversions now being planned."
- Proposed: "Wondering which real-world Go packages make good conversions? The **[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html)** ranks the most-depended-on Go modules and grades how hard each one is to convert."
- (The wording follows the Atlas page's own meta description.)

**R2. L373, the closing NOTE of the walkthrough (stale: Phase 4 framed as ongoing).** SAFE.
- Current: "> **NOTE:** this `fatih/color` example **compiles clean** — app plus all four dependency projects — **and runs**. Bigger programs are a deeper milestone: the referenced standard library compiles in full, and making it **operational** package by package is the [Phase-4](RoadmapHistory.md#phase-4--convert-and-run-go-package-tests) work that [Validated Test Packages](ValidatedTestPackages.md) tracks."
- Proposed: "> **NOTE:** this `fatih/color` example **compiles clean** — app plus all four dependency projects — **and runs**. The standard library it references is validated package by package against Go's own tests; see [Validated Test Packages](ValidatedTestPackages.md)."

**R3-R8. L203-L213, the options table: one or two lines per option, with the detail moved to a new subsection.** SAFE.

R3. `-recurse` (L203). The current cell starts "Recursively convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library through `$(go2csPath)`. ..." Proposed cell:
> Convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library. An optional second positional sets the output root for the generated `src\` (app) and `pkg\` (dependency) trees. A package that fails to convert is reported and skipped. See [Converting a real-world module](#converting-a-real-world-module).

R4. `-recurse=module` (L204). The current cell starts "Same recursion, narrower **scope**: convert the input module's own packages ...". Proposed cell:
> Same, but convert only the module's own packages: third-party packages are referenced into `pkg\` but not converted, and are listed at the end of the run. See [converting the module only](#optional-convert-the-module-only-and-deal-with-its-dependencies-later).

R5. `-recurse=nuget` (L205). The current cell starts "Same, but the standard library, the `golib` runtime and the analyzer come from NuGet — ..." and ends "... rather than emitting a project whose restore fails on whichever packages that release added or moved." Proposed cell:
> Same, but the standard library, the `golib` runtime and the analyzer come from NuGet ([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), [`go.lib`](https://www.nuget.org/packages/go.lib), [`go.gen`](https://www.nuget.org/packages/go.gen)), so nothing is staged locally. Third-party modules that the [nugetgo.net](https://nugetgo.net) registry maps are referenced as published packages when a qualifying version exists, and converted locally otherwise (see [Mapping modules to NuGet packages](#mapping-modules-to-nuget-packages)). Scope and reference style combine: `-recurse=module,nuget`. The published packages match the Go release go2cs is built with; go2cs checks the module against that release first, and refuses a mismatch, naming both versions.

R6. `-nuget-map <source>` (L206, about 330 words in one cell). Proposed cell:
> With `-recurse=nuget`: add a mapping source, a local file or an `https://` URL. Repeatable; the first source that names a module wins, and nugetgo.net answers the rest. `-nuget-map off` turns mapping off. See [Mapping modules to NuGet packages](#mapping-modules-to-nuget-packages).

Keep the four short `-nuget-map-*` rows (L207-210) as they are.

R6b. **New subsection** after L437 (the end of "Optional: build against a local standard library"), before `## Project layout`. It carries the detail that left the L206 cell:

```markdown
#### Mapping modules to NuGet packages

With `-recurse=nuget`, go2cs asks the [nugetgo.net](https://nugetgo.net) registry whether someone has already
converted and published each third-party module. A mapped module is **referenced, not converted**: one
exact-pinned `PackageReference` per module. A mapping applies only when a published version describes itself (in
`go2cs/source-metadata.txt`) as that module, at exactly the version your build selects, and was built for this
go2cs release; the highest such revision wins. Anything else converts locally, and the end-of-run table says why.
A module supplied by a `replace` directive is never mapped.

The choices are pinned, with each package's SHA-512, in `go2cs.nuget.lock` in the output root. A later run reuses
the pin offline, warns if a source now disagrees, and refuses a package whose bytes no longer match.

- `-nuget-map <source>` adds your own mapping source: a local file or an `https://` URL in the registry's format.
  It is repeatable, and the first source that names a module answers for it. nugetgo.net answers every module your
  sources do not name, so a short override file is enough.
- `-nuget-map off` turns mapping off and makes no request.
- `-nuget-map-only`, `-nuget-map-exclude`, `-nuget-map-refresh` and `-nuget-map-canonical-only` are listed under
  [Common options](#common-options). Every `-nuget-map*` flag needs `-recurse=nuget`.
```

- **Dependency:** R5, R6 and RM4 all link the `#mapping-modules-to-nuget-packages` anchor that R6b creates, so the
  four land together. If R6b is dropped, point those three links at `#common-options` instead.

- **NEEDS-OWNER (R6c):** the old cell ends "Not yet exercised end to end: a mapped module whose package itself
  depends on another third-party package (no such package is published yet)." This is a time-sensitive disclosure.
  - Recommendation: keep one sentence at the end of the subsection's first paragraph until such a package exists:
    "A mapped package that itself depends on another third-party package is not yet tested end to end."

R7. `-tests` (L211). The current cell starts "Also convert the package's eligible `_test.go` suite and emit a runnable C# test-host project (default off). Forces `-comments` on and self-locates `$(go2csPath)` ..." Proposed cell:
> Also convert the package's `_test.go` suite and emit a runnable C# test-host project (default off). Forces `-comments` on and works from a bare clone with no flags or environment setup. With plain `-recurse` it validates a whole module against its own tests: `go2cs -tests -recurse module_dir out_root`, with the output root outside the module's source tree; a module that needs a newer Go than the converted standard library is refused. See [Try it yourself](#try-it-yourself--validate-a-converted-test-suite).

R8. `-test-timeout` (L213). The current cell contains "`hash/maphash` takes ~15 minutes in C# where Go's takes 7.6 seconds, so it is validated with `-test-timeout 30m`." Proposed cell:
> Package deadline for a converted-test action, in Go duration syntax (default `2m`); `run` and `compare` give it to both `go test` and the converted host. The host's `dotnet publish` always gets at least `30m`, because the first publish on a fresh tree builds the whole standard-library closure. A suite that runs long in C# needs a larger value: `hash/maphash` is validated with `-test-timeout 30m`.

**R9. L226, `-cgo` (stale: "staged after the current validation campaign").** SAFE.
- Current: "| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional — but planned, not abandoned: the ratified [cgo interop plan](PLAN-cgo-interop.md) lays out the `import "C"` ladder (P/Invoke-backed, staged after the current validation campaign), and this flag comes alive with it. |"
- Proposed: "| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional, but planned: the [cgo interop plan](PLAN-cgo-interop.md) lays out a P/Invoke-backed bridge for `import "C"`, and this flag comes alive with it. |"

**R10. L274-L295, version wording in the walkthrough prose.** SAFE for the prose. The command pins stay.

L274:
- Current: "Next, pin the app to a **Go 1.24-compatible** dependency set and confirm it builds as Go."
- Proposed: "Next, pin the app's dependencies to releases that go2cs's Go version can read, and confirm it builds as Go."

L276:
- Current: "> **NOTE:** _go2cs is built with **Go 1.24.13**, so its type-checker only reads modules whose `go` directive — and their dependencies' — is **≤ 1.24.13**. `fatih/color` v1.19+ and recent `golang.org/x/sys` releases require a newer Go than 1.24, which would fail step 2 with_ `package requires newer Go version`_; pin as shown._"
- Proposed: "> **NOTE:** _go2cs's type-checker reads only modules whose `go` directive (and their dependencies') is no newer than the Go release go2cs is built with (see [Requirements](#requirements)). Newer `fatih/color` and `golang.org/x/sys` releases need a newer Go, which would fail step 2 with_ `package requires newer Go version`_; pin as shown._"

Keep L278 (the GOTOOLCHAIN paragraph) as it is.

L280-281:
- Current: "First pin the toolchain, so Go uses the Go 1.24.13 you have instead of fetching the newer one a dependency asks for — the one command whose syntax is shell-specific:"
- Proposed: "First pin the toolchain, so Go uses the release you have instead of fetching the newer one a dependency asks for. This is the one command whose syntax is shell-specific:"

L295 comment:
- Current: "`go mod tidy                             # download color + its (Go 1.24-compatible) dependencies`"
- Proposed: "`go mod tidy                             # download color + its dependencies`" (keep the column alignment).

- **NEEDS-OWNER:** L294 `go get github.com/fatih/color@v1.18.0   # a Go 1.24-compatible release (v1.19+ requires Go 1.25)`.
  The pin is load-bearing, so it moves only at a Go hop.
  - Recommendation: keep the line exactly. The pin and its reason are true facts about fatih/color, and the hop
    runbook re-times the walkthrough anyway.

**R11. L335-345, the platforms NOTE (version noise, an implementation aside, and a fixed bug still listed).** SAFE,
with one precondition.
These lines carry no migrate-gorelease anchor. README:338-339 are REVIEW-only, and the history anchor ps1:478 is
spelled at 1.23.1.
- **The Linux colors bug is fixed at this SHA.** Commit `2b31027b77` (golib NativeStructMarshal, merged at
  `65be55315f` in TRAIN C) records `TCGETS ok ... isatty true`, and an integration run of the README walkthrough's
  colordemo under a pty, `ESC[32;1mhello from fatih/color ESC[0;22m`, byte-identical to `go run`. It is an ancestor of
  the release commit `5732917a95`, so the published packages carry it. The current note (`d1ee5a87f8`) was written
  about four hours before the fix landed.
- **Precondition:** re-run the walkthrough once under a Linux pty against the published packages. If the colors show,
  apply the note below. If they do not, end that sentence the old way instead ("... including `fatih/color`'s
  colors in an interactive console on Windows; in an interactive terminal on Linux the colors do not show yet, so
  the output stays plain there."), and drop RM8.
- Current (L337-340 core): "Windows needs go2cs packages **1.24.13.1 or later**, the release matching the converter's Go 1.24.13 toolchain. Linux needs packages **1.24.13.2 or later** AND a converter built from a checkout that includes the 1.24.13.2 changes."
- Current (L341-345): "In an interactive terminal on Linux the colors do not show yet, because the terminal query cannot yet hand its struct to the kernel, so the output stays plain there. Other platforms and architectures are tracked in the Roadmap's [Platforms section](Roadmap.md#platforms), with the operational detail in [PLAN-linux-operation.md](PLAN-linux-operation.md)._"
- Proposed (whole note):

```markdown
> **NOTE — platforms:** _all four steps run on **Windows** (`windows/amd64`) and **Linux** (`linux/amd64`).
> The conversion records the platform it targets, and the go2cs packages compile and run against that
> platform's flavor: `win-x64` for a Windows conversion, `linux-x64` for a Linux one. Use the current go2cs
> packages, with a converter built from a checkout at or after the commit that published them. The output
> matches `go run`, including `fatih/color`'s colors in an interactive terminal, on both platforms. Other
> platforms and architectures are tracked in the Roadmap's [Platforms section](Roadmap.md#platforms)._
```

The PLAN-linux-operation link is dropped here; the Roadmap's Platforms section still links it.

**R12. L452-454, the contributors pointer (CLAUDE.md is now the AI-agent index).** SAFE.
- Current: "Contributors: see [`CLAUDE.md`](../CLAUDE.md) for an architecture overview and [`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md), and [`Roadmap.md`](Roadmap.md) for details. There's lots of low hanging fruit to be had here, jump in if you'd like to help..."
- Proposed: "Contributors: start with [`CONTRIBUTING.md`](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md), then see [`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md) and [`Roadmap.md`](Roadmap.md) for details. There's plenty of low-hanging fruit here; jump in if you'd like to help."

**R13. L458-464, Status paragraph 1 (a dated count, an exact package count and a version).** CHECKED-SITE: retire site 4 (0.2).
- Current: "The converter builds idiomatic C# for the full range of Go language features, gated by nearly 700 Go-vs-C# behavioral regression projects (2026-09-26) — each transpiled, compiled, byte-compared against a committed golden and, where it is a runnable program, executed with its stdout compared against the Go original's. The entire Go standard library (342 packages, Go 1.24.13) compiles cleanly as .NET assemblies."
- Proposed: "The converter builds idiomatic C# for the full range of Go language features, guarded by hundreds of Go-vs-C# behavioral test projects. Each one is transpiled, compiled and compared against a committed golden, and each runnable one is executed with its output compared against Go's. The entire Go standard library compiles cleanly as .NET assemblies."
- The HTML comment at L462-464 may stay, since comments are free provenance. Reword its opening to `"hundreds" (697 at the 2026-09-26 count) ...` so it still matches the prose.

**R14. L471-481, Status paragraph 3 (exact figures outside the guarded block, and a phase label).** SAFE. The module sentence is NEEDS-OWNER.
- Current: "Compiling is not runtime parity, so the library is also validated **operationally** ([Phase 4](RoadmapHistory.md#phase-4--convert-and-run-go-package-tests)). Each package's own `_test.go` suite is converted to C#, built against the converted standard library, and run under a Go-semantics test host. Its results are compared verdict for verdict against a clean `go test -json` baseline, and every difference is disclosed by exact failure signature; a test withdrawn is withdrawn from both sides, by name. At Go 1.24.13 every implementable package validates: 225 of 225, and 225 of the 230 testable. The five outside the implementable set are each listed with the reason they cannot be validated. [Validated Test Packages](ValidatedTestPackages.md) tracks the set, and the results are reproducible via [Try it yourself](#try-it-yourself--validate-a-converted-test-suite)."
- Proposed: "Compiling is not the same as running correctly, so the library is also validated **operationally**. Each package's own `_test.go` suite is converted to C#, built against the converted standard library, and run under a Go-semantics test host. Its results are compared verdict for verdict against a clean `go test -json` run, and every difference is disclosed by exact failure signature; a test withdrawn is withdrawn from both sides, by name. Every implementable package validates this way, on Windows and on Linux where it applies, and the few packages outside that set are each listed with the reason they cannot be validated. [Validated Test Packages](ValidatedTestPackages.md) carries the current counts, and the results are reproducible via [Try it yourself](#try-it-yourself--validate-a-converted-test-suite)."
- **NEEDS-OWNER (R14b):** append "Real third-party Go modules convert and validate against their own test suites the same way, and macOS support is in progress."
  - Recommendation: yes, but name no module until its proof page is public.
  - Evidence at this SHA: docs/phase4/SIZING-j0-uuid.md measured uuid at 51 of 54; the three misses are test order
    (D4). D4 landed in TRAIN K (`5d6a170ba7`), but the record's last amendment only **predicts** the result ("the
    reading to take at the landed tip is 54 of 54"). No 54-of-54 reading and no uuid proof page are recorded in the
    tree. The module proof pages (validationProofPages.go:847) and `-tests -recurse` (moduleTestsDriver.go) are in
    the tree.
  - Before the sentence lands, take the uuid `-tests` reading at a tip that includes D4 and record it (or point at
    another module's recorded reading, such as the hashset module's).
- The HTML comment at L480-481 may stay, reworded to say the prose no longer quotes the counts.
- Move here, as the paragraph's last sentence, the L550 line from R15, reworded: "go2cs moves to newer Go and .NET releases one at a time, and each move re-validates every package against that release's own tests; see the [Roadmap](Roadmap.md)."

**R15. L540-550, the Performance paragraph (a date and a version, and Phase 4 framed as ongoing).** SAFE.
- Current (L543-548): "... run at **parity with Go or faster in both C# variants** (measured against go1.23.1 on 2026-08-25; a rerun on the current Go 1.24.13 pin is queued). Most compute-shaped code — channels included — sits within a small multiple of Go, with runtime structural-interface satisfaction the honest outlier. Save for the [ref struct](...) based stack string and [stack slice](ConversionStrategies.md#slices-and-arrays) work already landed, broad optimization is targeted for _after_ Phase 4 — the parity rows show the ceiling, not the finish line."
- Proposed (L540-548):

```markdown
_Everyone asks:_ how fast is the transpiled C# compared to the original Go — including startup time,
memory, and Native AOT builds? See the [performance comparison](Performance.md) — **`TL;DR`**: _usually
slower than native Go, [but not always](Background.md#how-fast-is-converted-code)_: maps and the optimized
[stack string](ConversionStrategies.md#strings-string-and-sstring) path run at **parity with Go or
faster in both C# variants**. Most compute-shaped code — channels included — sits within a small multiple
of Go, with runtime structural-interface satisfaction the honest outlier. Some optimization is already in,
such as the [ref struct](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct)
based stack strings and [stack slices](ConversionStrategies.md#slices-and-arrays); broader optimization is
planned (see the [Roadmap](Roadmap.md#performance)), and the parity rows show the ceiling, not the finish line.
```

- Delete L550 here; it moves to R14 as above.
- The "but not always" link is **retargeted** from `Background.md#why-convert-go-to-c` to
  `Background.md#how-fast-is-converted-code`: the Background rewrite moves its parity sentence into that section. This
  edit therefore lands in the same seat as the Background rewrite (Appendix C). If Background does not land, keep the
  old target.
- "Planned", not "ongoing": the Roadmap at this SHA puts the allocation work's design kickoff in Q1 2027 and lists the
  interface and stack-string items as unscheduled.

**R16. L552-581, Milestones: 24 dense rows (L558-581).** NEEDS-OWNER.
- Recommendation: trim to the real turning points. NEWS keeps every announcement and RoadmapHistory keeps the
  anchors.
- Keep `## Milestones` (StdLibCompileMilestone.md and Background link it) and the intro line. Dates and tags stay,
  since a timeline is dated by nature. Proposed table:

```markdown
| Date | Milestone | Commit / Tag | Notes |
|:--|:--|:--|:--|
| 2018-05-21 | Project inception | `929d1457f` | A C#/.NET converter built on an ANTLR4 Go grammar. |
| 2022-03-13 | [`v0.1.2` release](NEWS.md#march-13-2022--v012-release) | [`v0.1.2`](https://github.com/ritchiecarroll/go2cs/releases/tag/v0.1.2) | The mature ANTLR4-era converter, tagged. |
| 2025-01-12 | [The converter is rewritten in Go](NEWS.md#january-12-2025--the-converter-is-rewritten-in-go-go2cs-version-2) | `87465f5f5` | Rebuilt on `go/ast` + `go/types`, with the `golib` runtime library and Roslyn source generators supplying Go's semantics. |
| 2025-05-05 | [First full standard-library auto-conversion](NEWS.md#may-5-2025--first-full-standard-library-auto-conversion) | `6ca1c45b7` · [`full-conversion-2025-05`](https://github.com/ritchiecarroll/go2cs/releases/tag/full-conversion-2025-05) (`cc14584c7`) | Every Go file gets a C# file; compiling comes later. |
| 2026-07-10 | [**First clean full-standard-library compile**](NEWS.md#july-10-2026--the-entire-go-standard-library-compiles-in-net) | `51ba5d9cf` · [`stdlib-green-2026-07-10`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-green-2026-07-10) | Every package compiles with zero errors, `runtime`, `reflect` and `net/http` included ([details](StdLibCompileMilestone.md)). |
| 2026-07-14 | [Standard library on NuGet](NEWS.md#july-14-2026--the-converted-go-standard-library-is-on-nuget) | `2363af0e6` · `dd821a556` · [`nuget-stdlib-2026-07-14`](https://github.com/ritchiecarroll/go2cs/releases/tag/nuget-stdlib-2026-07-14) | A converted app references the standard library from nuget.org, with no local go2cs checkout. |
| 2026-07-17 | [**First Go test suite passing in C#**](NEWS.md#july-17-2026--gos-own-tests-now-pass-in-c) | `337a928df` · [`utf8-tests-green-2026-07-17`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf8-tests-green-2026-07-17) | `unicode/utf8` matches `go test`, test for test. |
| 2026-08-08 | [**Go programs run on Linux**](NEWS.md#august-8-2026--go-programs-run-on-linux) | [`linux-first-run-2026-08-08`](https://github.com/ritchiecarroll/go2cs/releases/tag/linux-first-run-2026-08-08) | Converted programs, the `fatih/color` walkthrough included, match `go run` on Linux. |
| 2026-10-01 | [**Every implementable standard-library package validates**](NEWS.md#october-1-2026--every-implementable-standard-library-package-validates) | `133ca704e` · `nuget-1.24.13.3` | Every implementable package passes its own Go tests in C#, `runtime` included, on Windows and Linux. |
```

- If the owner wants the percentage milestones kept (half, 75%, 90%), add them back as one-line rows with no
  verdict counts.
- The removed rows carry two history anchors (0.2); retiring them is optional.

**R17. L613, the License section ("remaining" reads stale).** SAFE.
- Current: "a commercial license, sponsorship of the remaining validation work, or an outright"
- Proposed: "a commercial license, sponsorship of ongoing work, or an outright"

**R18. Purpose bullets (L34-43): point to the module registry.** NEEDS-OWNER (launch timing is the owner's call).
- Recommendation: once nugetgo.net is public, add after L40:
  "* Find converted Go modules as NuGet packages: [nugetgo.net](https://nugetgo.net)"

**Deliberate keeps in README:**
- L119 "Go 1.24 generic aliases" and L142 "Go 1.22 loop variables": these name the Go language version that
  introduced a semantic, which a technical reader needs.
- L392 (sample `-recurse=module` output) and L508 (the expected `Validated 14 tests...` line): these are literal
  tool output.

---

## 2. docs/Roadmap.md (HIGH). Today it contradicts the README and lists finished work as planned.

**RM1. L42-44, "Where things stand".** SAFE, except the module clause, which follows R14b (including its
reading precondition).
- Current: "**Where things stand.** The converted standard library is on Go 1.24 and is validated package by package against Go's own tests on Windows and Linux; [Validated Test Packages](ValidatedTestPackages.md), the validation roster, carries its current state."
- Proposed: "**Where things stand.** Every implementable package of the converted standard library validates against Go's own tests on Windows and Linux, and real third-party modules now convert and validate against their own tests too. [Validated Test Packages](ValidatedTestPackages.md), the validation roster, carries the current state."
- This keeps the top comment's "no forecast" ruling.

**RM2. L77-83, the two "protective changes": both are done at this SHA.** SAFE.
- Evidence:
  - `global.json` pins SDK 10.0.100 with rollForward latestFeature.
  - `LangVersion` 14 is set in src/Directory.Build.props:45, golib.csproj:37, go2cs-gen.csproj:27,
    csproj-template.xml:28 and test-csproj-template.xml:37.
  - `csReservedTypeNames = {closed, union, safe}` is at src/go2cs/identifierNaming.go:93.
- Current (L77-83): "**Two protective changes land before .NET 11 ships in November 2026:** - **An SDK and C# language-version pin**: ... - **Escaping identifiers that C# 15 gives new meaning**, such as `closed`, `union` and `safe` when they name a type: the converter escapes a Go identifier that collides with one."
- Proposed (one standing paragraph in place of the heading and two bullets):
  "**The SDK and the C# language version are pinned.** A repository `global.json` pins the .NET SDK, and every project, including the template the converter emits, fixes `LangVersion` instead of following `latest`, so a machine whose default SDK moves to a newer .NET does not silently compile converted code under a newer C#. The converter also escapes Go identifiers that newer C# gives new meaning, such as `closed`, `union` and `safe` when they name a type."
- Timeline L326: delete the row "| Before .NET 11 ships (November 2026) | The .NET SDK and C# language-version pin, and the C# keyword escape. |".

**RM3. L130-156, "Real-world Go modules": everything reads as future work.** NEEDS-OWNER (which modules to name, and the status of each).

What is in the tree at this SHA:
- `-tests -recurse` is shipped: README L195 and L211, moduleTestsDriver.go (TRAIN L, M3).
- Third-party proof pages are written beside the conversion: validationProofPages.go:847 `validationProofDestination` (M1).
- DefaultGODEBUG from the module's `go` line: godebugDefault.go (M6).
- The module sandbox stages fixtures outside `testdata`: TRAIN L M4, ModuleAncestryTests.
- The module-cache lock: go2cs.modules.lock (M2).
- The multi-package packaging decision is ruled: PLAN-nugetgo.md §8 item 4, amended 2026-09-29, one nupkg per module.
- Unique identity tokens for map and slice backings are in: `src/core/golib/ж.PointerTokens.cs` defines
  `IdentityBand`, the number `reflect.Value.Pointer()` answers for a map, a slice, a func, a channel or any other
  object that is not a `ж<T>`. It states "The id is UNIQUE: minted once per object ... so two live objects never
  share a token", and names gojq's allocator as the case it fixes (ruled from CENSUS-reflect-hash-tokens-go1.24.13.md).
- The uuid pilot has been converted, packed and consumed from a local feed, with 28 of 28 output lines matching
  `go run` (SIZING-j0-uuid.md, on `win-x64`). Its `-tests` reading is 51 of 54; the record only predicts 54 of 54
  with D4 landed (see R14b).

So the "What these need first" list (L142-152) is done.

Proposed section (replacing L130-156):

```markdown
## Real-world Go modules

Third-party modules are the proving ground after the standard library. Each module is converted, validated
against its own Go test suite exactly as a standard-library package is, and given a proof page beside the
conversion rather than on the standard-library roster. `go2cs -tests -recurse` runs a module's tests across all
of its packages, and a validated module packs as one NuGet package.

The [`google/uuid`](https://github.com/google/uuid) pilot is one package with no dependencies: converted, packed
and consumed from a local feed, with its output matching `go run`. It generates Go-identical v1, v3, v5 and v6
UUIDs, where `System.Guid` generates v4 and v7. The next candidates, in order:

| Module | Why it comes here | When |
|:--|:--|:--|
| (gojq row: unchanged from L139) | | Q4 2026 |
| (jwt row: unchanged from L140) | | Q4 2026 |

gojq's full command-line conformance suite is a later second stage, once its command-line package's own
dependencies convert. Further modules follow; the [go2cs Target Atlas](https://go2cs.net/TargetAtlas.html) is one
input to their selection.
```

- The uuid sentence says "converted, packed and consumed", not "validated", until the 54-of-54 reading is recorded
  (R14b). Once it is, add "validated against its tests" back.
- The "Before gojq" identity-token paragraph is gone: that work is in the tree (see above). If the owner wants the
  token-width wrap caveat stated for gojq, it is one optional line; nothing is pending.
- Keep the versions in the gojq and jwt rows: they name the exact upstream releases the plan targets.
- The owner may add modules now in flight. Recommendation: name none until each validates.

**RM4. L158-174, "NuGet packages of converted modules": contradicts the README, and decisions are listed as open.** NEEDS-OWNER (the launch status and date).

What is in the tree at this SHA:
- README L205-210 documents `-nuget-map`, on by default through nugetgo.net, with `go2cs.nuget.lock` (S3 built:
  nugetMap.go, nugetSubstitution.go, nugetLock.go).
- The ID form is ruled: `nugetgo.<dotted module path>`; `go.` is the standard library only (PLAN §8, owner ruling
  2026-10-02).
- The prefix reservation is "IN MOTION" with NuGet.
- The first wave is uuid and jwt (B8).
- Rehearsal goes to int.nugettest.org (B7).

Proposed section:

```markdown
## NuGet packages of converted modules

With `-recurse=nuget`, a converted program references the standard library, the runtime and the source
generators as published NuGet packages, and the same substitution reaches third-party modules: the
[nugetgo.net](https://nugetgo.net) community registry maps Go module paths to published NuGet packages of their
conversions, so a dependency someone has already converted becomes a package restore instead of a local
transpile. The converter side is built and on by default (see the README's
[Mapping modules to NuGet packages](README.md#mapping-modules-to-nuget-packages)). The design is
[`PLAN-nugetgo.md`](PLAN-nugetgo.md).

- **Package IDs:** a converted module publishes as `nugetgo.<dotted module path>`; the `go.` prefix is the
  converted standard library's alone.
- **Proof packages rehearse first**, to a local feed and NuGet's test gallery, before anything reaches nuget.org.
- **Still to come:** the registry's own CI and its public launch, with the first published modules, and NuGet's
  reservation of the package-ID prefixes, which is pending with NuGet.
- **Packages built from an unsupported Go release are labelled as proofs**, and are rebuilt once the converted
  standard library is on a supported release.
```

- The README link works only if R6b lands in the same seat. Otherwise link `README.md#common-options`.
- Owner check: is the registry already "launched"? The site is reachable. If so, change the "Still to come" bullet
  to name only what is left.
- Owner check: which module leads the first wave? PLAN-nugetgo.md B8 rules "uuid and jwt", but §6 and its
  2026-10-02 amendment make hashset the proof of concept and the "first generic module packed". The bullet names no
  module, so it stays right whichever publishes first; name them once the owner says.

**RM5. L259-262, the performance re-baseline (the TLS benchmark already exists).** SAFE.
- Current: "It adds a TLS-handshake benchmark and profiles the crypto paths behind the handshake gap;"
- Proposed: "It brings the TLS-handshake benchmark, already in the suite, into the published table, and profiles the crypto paths behind the handshake gap;"

**RM6. The Timeline table (L323-330).** NEEDS-OWNER for the dates; the rest is SAFE.
- L325, October 2026. Current: "... Starting as capacity frees: cgo's loud failure on `import "C"`, the `google/uuid` pilot to a local feed, and the macOS run layer, in parallel."
  - Proposed: "... Starting as capacity frees: cgo's loud failure on `import "C"` and the macOS run layer, in parallel."
  - The pilot is done. If the owner targets the registry launch for October, add it here.
- L326: delete (RM2).
- L329, Q1 2027. Current: "NuGet registry: converter integration and launch, after the publishing-identity decisions."
  - Proposed: "The nugetgo.net registry launch, with the first published modules." Move it to the row the owner names.

**RM7. L342, the converter loop (an exact version).** CHECKED-SITE: retire site 7 (0.2). Alternatively keep the text VERBATIM.
- Current: "Every converter change runs the same loop: edit `src/go2cs/*.go` → `go build` (Go 1.24.13) → re-transpile → `dotnet build`, and a whole-corpus reconvert shows no unintended drift before the change lands."
- Proposed: "Every converter change runs the same loop: edit `src/go2cs/*.go` → `go build` (with the Go release `src/go2cs/go.mod` names) → re-transpile → `dotnet build`, and a whole-corpus reconvert shows no unintended drift before the change lands."
- If RM7 lands, append a dated line to the L3-12 HTML comment, whose last sentence calls this sentence an editable
  site: "Amended <date>: the converter-loop sentence no longer names a release; its migrate-gorelease site is
  retired."

**RM8. L115-117, Platforms: the fatih/color terminal query is listed as remaining, but it is fixed.** SAFE, with
R11's precondition (drop RM8 if the Linux pty re-run shows no colors).
- Current: "A few operational items remain on Linux, such as the terminal query `fatih/color` makes before it shows colors in an interactive Linux terminal; they are tracked in [`PLAN-linux-operation.md`](PLAN-linux-operation.md) and the multi-target design."
- Proposed: "Remaining Linux operational items are tracked in [`PLAN-linux-operation.md`](PLAN-linux-operation.md) and the multi-target design."
- Evidence: see R11 (`2b31027b77`, an ancestor of the release commit). PLAN-linux-operation.md still names open items
  (F8, F9/N0, F12), so the sentence keeps a pointer rather than claiming nothing remains.

**Roadmap keeps:**
- The month and quarter targets, the Go release table (L64-68) and `net10.0`/`net12.0`: a roadmap is dated by nature.
- Platforms (L109-128, apart from RM8), cgo, Phase 4D, Phase 5, Open converter items.

---

## 3. src/tests/Performance/README.md, then re-mirror docs/Performance.md (MEDIUM)

Edit only the master README. Leave everything between the PERF-RESULTS markers alone, and keep all links absolute
(the file mirrors to docs/). After editing, regenerate the copy exactly as run-performance.ps1:70-75 does:

```powershell
$readme = [IO.File]::ReadAllText("$PWD/src/tests/Performance/README.md")
$banner = "`r`n<!-- AUTO-COPIED from src/tests/Performance/README.md by run-performance.ps1 -- edit that file, not this one. -->`r`n"
[IO.File]::WriteAllText("$PWD/docs/Performance.md", $readme + $banner)
```

**P1. L12-15, the intro range (contradicts the table).** SAFE. The tightest row is Fib at 1.36x on the JIT.
- Current: "while the rest ranges from ~1.5× on tight compute to the structural-interface assert, the honest outlier at the other end."
- Proposed: "while the rest ranges from well under 2× on tight compute to the structural-interface assert, the honest outlier at the other end."

**P2. L17-33, the benchmarks table (missing the RefLower row the results carry).** SAFE. Add after the IfaceShell row:
> | **RefLower** | A pointer-heavy hot loop: pointer parameters passed on to pointer parameters, address-taken locals and field addresses — the shapes the converter's ref-lowering turns from heap boxes into native C# `ref`s. |
- (From PerfRefLower.go's header comment.)
- Do not add TlsHandshake until the results table carries it. NEEDS-OWNER only if the owner wants it listed early.

**P3. L156-157, the Fib reading (contradicts the table: AOT 1.48x trails JIT 1.36x).** SAFE.
- Current: "- **Function calls / integers (Fib):** the closest compute workload — ~1.6× under the JIT and ~1.5× under Native AOT, the one tight loop where AOT leads the JIT rather than trailing it."
- Proposed: "- **Function calls / integers (Fib):** the closest compute workload, well under 2× Go in both C# variants."

**P4. L184, Channel.** SAFE, optional. It matches the table today, but it will drift with each re-run.
- Current: "Currently ~2.3–2.8× on this producer→consumer churn:"
- Proposed: "Currently between 2× and 3× on this producer→consumer churn:"

**P5. L207-212, RefLower ("~2.9×" is stale: the table reads 2.75x).** SAFE.
- Current: "(before that arc this shape ran ~25× Go; the lowering brought the JIT to ~2.9×). AOT currently trails the JIT here by a wide margin (~8×) — ILC's codegen ..."
- Proposed: "(before that arc this shape ran about 25× Go; the lowering brought the JIT within 3×). AOT trails the JIT here by a wide margin — ILC's codegen ..."

**P6. L168, the StringView link (points at the moved-reference stub).** SAFE.
- Current: "(see [ConversionStrategies-Reference](https://github.com/ritchiecarroll/go2cs/blob/master/docs/ConversionStrategies-Reference.md))"
- Proposed: "(see the [strings reference](https://github.com/ritchiecarroll/go2cs/blob/master/docs/ConversionStrategies-Reference/strings.md#a-non-escaping-stringbyte-local-emits-the-stack-string-sstring))"
- (The heading exists at strings.md:220.)

**P7. L254-262, compile provenance (machine names and "fleet" jargon on a public page).** SAFE.
- Current: "so the fourteen-cell AOT ladder was completed by publishing eight of the binaries on a second fleet machine and adopting them here"
  - Proposed: "so the fourteen-cell AOT ladder was completed by publishing eight of the binaries on a second, higher-memory machine and adopting them here"
- Current: "| **farm-adopted** — published on the fleet's i9-13900K, hash-verified, then measured here |"
  - Proposed: "| **farm-adopted** — published on the second machine, hash-verified, then measured here |"

**P8. Jargon.** SAFE, optional.
- L58 "(re-measured per hop)" becomes "(re-measured at each Go or .NET release move)".
- L221 "Measured at the .NET 10 hop" becomes "Measured at the move to .NET 10".
- Leave the generated Environment line and the dated History tables alone (records by design).

**P9. L146-155, "Reading the results": the Memory bullet contradicts the table.** SAFE. This is the largest
contradiction on the page.
- The Memory bullet (L151-155) says the AOT column's working-set floor is *higher* than the JIT's, "so AOT currently
  trades memory for its startup and per-benchmark wins". The current table shows AOT lower than the JIT on all
  fourteen rows (Startup: AOT 12.5 MB, JIT 47.4 MB), and the page's own History section (L316) says the penalty
  "inverted, and did so universally".
- Current Memory bullet: "- **Memory:** the working-set columns carry the cost of the full converted standard library. The JIT column's floor is the .NET runtime plus loaded assemblies; the AOT column's is *higher* — the self-contained binary maps the whole compiled closure into the process — so AOT currently trades memory for its startup and per-benchmark wins. Reducing both floors is optimization surface (trimming eligibility, lazy package init), not a semantic cost."
- Proposed: "- **Memory:** both C# columns carry the cost of the full converted standard library, so they sit well above Go on every row but Map. The JIT column's floor is the .NET runtime plus loaded assemblies; under .NET 10 the AOT column's floor is lower than the JIT's on every row. Reducing both floors is optimization surface (trimming eligibility, lazy package init), not a semantic cost."
- Startup bullet (L146-150): drop the closing "— see the memory note below for its trade", so it ends "For CLI-shaped programs AOT is the deployment story on time."
- Edit `src/tests/Performance/README.md` and re-mirror, as this section prescribes.

---

## 4. docs/Architecture.md (MEDIUM)

**A1. L3, the companion pointer.** SAFE.
- Current: "Companion to [`/CLAUDE.md`](../CLAUDE.md). Detailed map of the converter pipeline, the `visit*`/`conv*` file taxonomy, the analysis passes, the Roslyn source generators, and the runtime type map."
- Proposed: "A map of the converter pipeline, the `visit*`/`conv*` file taxonomy, the analysis passes, the Roslyn source generators, and the runtime type map. For how to use the converter, see the [README](README.md#usage)."

**A2. L19, the entry point, plus two new bullets after the Stdlib driver bullet (L26-29).** SAFE.
- Current L19: "- **Entry point:** `main.go` — flag parsing, GOROOT/GOPATH resolution, single-file/dir vs `-stdlib` mode."
- Proposed L19: "- **Entry point:** `main.go` — flag parsing, GOROOT/GOPATH resolution, and dispatch: a single file or package (`conversionDriver.go`), the standard library (`-stdlib`), a module (`-recurse`), or a package's tests (`-tests`)."
- Add after L29:
  - "- **Module driver:** `moduleConverter.go` — `-recurse`: converts a downloaded module plus every third-party package in its import closure, in dependency order, while referencing the pre-converted standard library. `nuget*.go` maps third-party modules to published NuGet packages under `-recurse=nuget` (`nugetMap.go`, `nugetSubstitution.go`, `nugetLock.go`)."
  - "- **Test pipeline:** `testConversion.go` — `-tests`: converts a package's `_test.go` suite, emits a runnable test host, and compares its verdicts with `go test -json`. `moduleTestsDriver.go` runs it for every package of a module (`-tests -recurse`), and `validationProofPages.go` writes the proof pages."
- (Each description is taken from that file's own header comment.)

**A3. L68-75, analysis passes: add ref-lowering.** SAFE. Add after the escape-analysis bullet:
> - `refLoweringAnalysisOperations.go` / `refLoweringEmissionOperations.go` — decide which pointer parameters can be C# `ref` parameters instead of `ж<T>` heap boxes, and emit them.

**A4. L82-87, the generators table (missing two of six).** SAFE. Add:
> | `PartialStubGenerator` | a bodyless `partial` method with no implementing part | A throwing stub, so a Go function with no Go body (assembly or cgo) compiles until a hand-owned companion supplies it. |
> | `StrGenerator` | `[GoStr]` on a method | The `@string` forwarder (and, for a package-level function, its value delegate) for an `sstring` twin. |
- (From each generator's header comment.)

**A5. L97, the string row (sstring is production, in its own file).** SAFE.
- Current: "| string | `@string` (UTF-8 backed); experimental ref-struct `sstring` — `string.cs` |"
- Proposed: "| string | `@string` (UTF-8 backed) — `string.cs`; `sstring`, a zero-copy stack view for a non-escaping `string([]byte)` — `sstring.cs` |"

---

## 5. src/tour/README.md (LOW)

**T1. L16, a link to the moved stub.** SAFE.
- Current: "([Comments](../../docs/ConversionStrategies-Reference.md#comments))."
- Proposed: "([Comments](../../docs/ConversionStrategies-Reference/comments.md#where-a-comment-is-attached-and-where-it-is-not))."
- The tour sentence is about where an end-of-line comment attaches, which the reference covers directly; the stub's
  `#comments` anchor resolved to the reference page too, so this keeps the link's depth without going through the
  stub. (The heading is at comments.md:13. The summary-page alternative is `../../docs/ConversionStrategies.md#comments`.)

**T2. L38, the Go version.** SAFE, a plain readability edit: no checker reads the line. src/migrate-gorelease.ps1:558
classifies `^src/tour/README\.md$` as MUST-NOT-CHANGE, so a hop never updates the line and it goes stale silently.
After T2 that path rule matches nothing; it is harmless and can stay.
- Current: "- Go 1.24.13 (the release `src/go2cs/go.mod` pins)"
- Proposed: "- Go, at the release `src/go2cs/go.mod` names"

**T3. L117-121, history plus a machine description.** SAFE.
- Current: "That build gets ten minutes, which is a guard against a hung `go build` rather than an expectation about pace: it was two minutes until 2026-08-10, and a cold build measured 1m58.8s on an i7-5820K under WSL2 with the repository on a `/mnt` DrvFs mount — inside the cap by about a second. Raise or lower it with ..."
- Proposed: "That build gets ten minutes, as a guard against a hung `go build`, not because it is expected to take that long. Raise or lower it with ..." (the rest of the paragraph is unchanged).

---

## 6. src/go2cs/README.md (LOW; no checker reads it)

**G1. L28-31, the usage pointer.** SAFE.
- Current: "See [`main.go`](main.go) for the authoritative flag set (`-stdlib`, `-recurse`, `-tests`, `-platforms`, `-go2cspath`, …) and the repository's [`CLAUDE.md`](../../CLAUDE.md) / [`docs/Architecture.md`](../../docs/Architecture.md) for how each mode is used in practice."
- Proposed: "See [`main.go`](main.go) for the authoritative flag set (`-stdlib`, `-recurse`, `-tests`, `-platforms`, `-go2cspath`, …), the [README's usage section](../../docs/README.md#usage) for how each mode is used, and [`docs/Architecture.md`](../../docs/Architecture.md) for a map of the code."

**G2. L35-42, layout: add the module and NuGet drivers, and replace the phase label.** SAFE.
- Current L41-42: "- `testConversion*.go` — the Phase-4 `-tests` pipeline: converts a package's `_test.go` suite, builds the runnable test host, and differentially compares results against `go test -json`."
- Proposed (two bullets):
  - "- `moduleConverter.go` — the `-recurse` module driver; `nuget*.go` — mapping third-party modules to published NuGet packages (`-recurse=nuget`)."
  - "- `testConversion.go` — the `-tests` pipeline: converts a package's `_test.go` suite, builds the runnable test host, and compares results against `go test -json`; `moduleTestsDriver.go` runs it across a whole module (`-tests -recurse`)."

**G3. L46, a link to the moved stub.** SAFE.
- Current: "[`docs/ConversionStrategies-Reference.md`](../../docs/ConversionStrategies-Reference.md)"
- Proposed: "[`docs/ConversionStrategies-Reference/`](../../docs/ConversionStrategies-Reference/README.md)"

---

## 7. docs/ConversionStrategies.md (LOW)

**CS1. L4, "one short section per topic".** SAFE. The page is about 12,300 lines.
- Current: "> **How `go2cs` turns each Go construct into C#, one short section per topic, with the Go and the C# it"
- Proposed: "> **How `go2cs` turns each Go construct into C#, one section per topic, with the Go and the C# it"

**CS2. L15-16, the snippet provenance release.** CHECKED-SITE: retire site 10 (0.2) and amend the ps1:348-349 comment. Recommended, because snippets are not re-taken at every hop, so naming the release invites a stale claim.
- Current: "> The C# snippets below are drawn from the actual converted standard library (`src/core/`,\n> Go 1.24.13) wherever possible, paired with their original Go source; ..."
- Proposed: "> The C# snippets below are drawn from the actual converted standard library (`src/core/`)\n> wherever possible, paired with their original Go source; ..."
- The per-block HTML comments keep their exact provenance, at no cost.
- Alternatively, keep the text VERBATIM.

**CS3. L1654-1657, an example version (REVIEW-only, not anchored).** SAFE.
- Current: "For example, Go 1.24.13 gives `1.24.13.*`, and restore takes the newest published `1.24.13.x` package."
- Proposed: "For example, Go 1.N.P gives `1.N.P.*`, and restore takes the newest published `1.N.P.x` package."
- Keep the trailing HTML comment.

**CS4. L1659-1662, example versions.** SAFE.
- Current: "The published packages match one Go release, such as 1.24. ... Only the major and minor numbers are compared, so a different patch number, such as 1.24.5, is accepted."
- Proposed: "The published packages match one Go release, by major and minor version. ... Only the major and minor numbers are compared, so a different patch number is accepted."
- Keep the trailing HTML comment.

**CS5. The 38 relative `../src/...` links.** No change; see 0.1.

---

## 8. docs/ConversionStrategies-Reference/README.md (LOW)

**CR1. L6, a phase label.** SAFE.
- Current: "*why*: the exact emitted form, the edge cases, the Phase-3 fixes, the behavioral-test guards, and"
- Proposed: "*why*: the exact emitted form, the edge cases, the converter fixes, the behavioral-test guards, and"

**CR2. L10-18, a dated banner, the retired working name, history and a CLAUDE.md see-also.** SAFE.
- Current: "> **Updated 2026-06-27 for the \"go2cs2\" generation of the converter.** This is a living document; as more use cases are converted these strategies are refined. The current converter is written in **Go** ... Notes that previously referenced the retired ANTLR4/C# converter or the old `gocore` library have been updated to reflect this. See also: [`Architecture.md`](../Architecture.md), [`Glossary.md`](../Glossary.md), [`Roadmap.md`](../Roadmap.md), and [`CLAUDE.md`](../../CLAUDE.md)."
- Proposed:

```markdown
> **A living document**, refined as more code is converted. The converter is written in **Go** (using the
> official `go/ast` + `go/types` toolchain, under `src/go2cs/`) and emits C# that leans on two things the visible
> code does not show in full: a hand-written runtime library, **`golib`** (`src/core/golib/`), and a set of
> **[Roslyn](../Glossary.md#roslyn) source generators** (`src/gen/go2cs-gen/`) that synthesize the Go semantics
> which cannot be written directly in C#. See also: [`Architecture.md`](../Architecture.md),
> [`Glossary.md`](../Glossary.md) and [`Roadmap.md`](../Roadmap.md).
```

**CR3. L27-28, a stale pointer.** SAFE. The rule lives in `.claude/rules/harness-gates.md` now, which is agent-only.
- Current: "> summary (see [`../CLAUDE.md`](../../CLAUDE.md), \"Record the conversion decision\")."
- Proposed: "> summary."
- The sentence becomes: "When updating the converter, add the deep detail here and the reader-facing example to the summary."

---

## 9. docs/ConversionStrategies-Reference/purego.md (LOW)

**PG1. L15, "the future getg() model".** SAFE. getg() is implemented (golib/runtime/Goroutine.cs:91, :337), and `runtime` validates.
- Current: "These are stubbed with `[module: GoManualConversion]` — a stub that compiles is an acceptable milestone solution and the operational body lands with the future `getg()`/thread-context model."
- Proposed: "These are hand-owned, as whole-file `[module: GoManualConversion]` replacements or `*_impl.cs` companions (`internal/bytealg` uses `bytealg_impl.cs`), wherever a package's tests need them; golib's goroutine model, for example, answers Go's `getg()`. Any that remain compile against a generated throwing stub until [Phase 5](../Roadmap.md#phase-5--implement-assembly-backed-declarations-in-c) supplies them."
- Why not "whose bodies the managed runtime supplies directly": Roadmap Phase 5 (L206-210) says the remaining
  declarations "compile against a throwing stub from the `PartialStubGenerator` until a C# implementation exists",
  with hand-owned companions supplying many of them. The getg() evidence holds for getg(), not for the whole bucket.

---

## 10. docs/ConversionStrategies-Reference/compiled-library-vs-source.md (LOW)

**CL1. L7, "assumes structures can escape except in the simplest cases".** SAFE. Stale: escape analysis plus
ref-lowering exist, and the summary at ConversionStrategies.md:1499-1600 states the current rule.
- Current: "Because this is a complex optimization, the converter currently assumes structures can escape to the heap except in the simplest-to-detect cases (see [Pointers](pointers.md#pointers)). A future option could distinguish ..."
- Proposed: "go2cs converts each package separately, so it chooses every function's signature from that package alone. An exported function's pointer parameter is a heap box (`ж<T>`), because another package may call it. An unexported function, whose callers are all visible, can take a C# `ref` instead, and a local whose address reaches only such parameters stays a plain local (see [Pointers](pointers.md#pointers)). A future option could distinguish ..."
- The rest of the paragraph is unchanged.

---

## 11. docs/NEWS.md (LOW). Dated entries stay as written; fix only these two.

**N1. L496-498, a stale present-tense clause in a 2020 entry.** SAFE.
- Current: "on `src/gocore` — the small, manually-converted subset of the Go library that survives today as the curated baseline in `src/core`."
- Proposed: "on `src/gocore`, a small, manually-converted subset of the Go library."

**N2. L352-355, a line inside the footer starts with `#4`.** SAFE. Rewrap defensively so no line starts with `#`.
- Proposed (the link text stays on one line):

```markdown
*Full story:
[`bytes` and `strings` tests pass, with disclosed-divergence](news/2026-07-18-bytes-strings-disclosed-divergence.md) ·
Phase-4 packages #3 and #4 · `sort` 63/63, `bytes` 81, `strings` 68 · reproduce from a clone via
[Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*
```

---

## 12. docs/KnownIssues.md (LOW)

**K1. L21, an exact figure.** SAFE. No checker reads KnownIssues.md content (csprojTemplate_test.go:553 checks only
that the template's comment links the page). Keep a cost statement anyway, because the emitted csproj comment
(csproj-template.xml:58, "costs about 90 ms of startup") points readers here for it. Leave csproj-template.xml alone:
changing it changes emission and goldens. Optional: keep "about 90 ms" so the page and the emitted comment agree.
- Current: "Converted executable projects carry this line, commented out. It costs about 90 ms (16%) more startup time, and steady-state loop speed stays within measurement noise."
- Proposed: "Converted executable projects carry this line, commented out. It adds a little startup time, and steady-state loop speed stays within measurement noise."

---

## 13. docs/coding-style.md (LOW)

**C1. L31 (rule 18), "LangVersion `latest`" is no longer true.** SAFE.
- Current: "Every hand-authored project here sets `LangVersion` to `latest`, so a name that compiles fine on the SDK you happen to have becomes a hard build break on the next one — that is exactly how `go2cs-gen` stopped building for anyone on the .NET 10 SDK ([issue #34](https://github.com/ritchiecarroll/go2cs/issues/34)), and an older SDK will not warn you."
- Proposed: "Projects pin `LangVersion` (`src/Directory.Build.props`) at a release where `field` is already a keyword, and raising the pin can turn other names into keywords the same way. That is how `go2cs-gen` once stopped building on a newer SDK, back when projects followed `latest` ([issue #34](https://github.com/ritchiecarroll/go2cs/issues/34)); an older SDK will not warn you."
- Why: the pin is already `LangVersion` 14 (src/Directory.Build.props:45, go2cs-gen.csproj:27), and C# 14 is the
  release that makes `field` a keyword, so a local named `field` breaks today, not at some future raise.
- The rest of rule 18 is unchanged.

---

## 14. CONTRIBUTING.md (LOW; section 2 must stay section 2)

**CT1. L71-72, section 3's pointer (CLAUDE.md is the agent index, thin for a human).** SAFE.
- Current: "The repository's engineering discipline is in [CLAUDE.md](CLAUDE.md) and [docs/Architecture.md](docs/Architecture.md): a converter change comes with a behavioral test that fails without it, ..."
- Proposed: "[docs/Architecture.md](docs/Architecture.md) maps the code, and [CLAUDE.md](CLAUDE.md) carries the same discipline in the form AI coding agents read. A converter change comes with a behavioral test that fails without it, ..."
- The rest of the paragraph is unchanged.

---

## 15. No change needed

- `docs/SECURITY.md`, `src/gen/go2cs-gen/README.md`, `src/core/golib/README.md`: no stale claims, versions or dates found.
- `docs/ConversionStrategies-Reference.md`, the moved-reference stub: it keeps the old anchors. T1 and G3 stop
  linking it, but keep it (validation snapshots and records link it about 44 times).
- `docs/Performance.md`: regenerated from §3 only.

---

## Appendix A. Outside the public refresh (inventory: not in scope). Take these only on the owner's word.

| Doc | Item | Proposal |
|:--|:--|:--|
| TestingInfrastructureRequirements.md L3-7 | The status says "Proposed design" for something implemented. Roadmap links this page. | Replace the paragraph under `## Status` with: "The design behind the converted-test pipeline. Phases 4A to 4C are implemented as the `-tests` pipeline; Phase 4D is planned ([Roadmap](Roadmap.md#phase-4d-examples-fuzz-seed-corpora-and-benchmarks))." Keep the body and the `#phase-4d--later-test-kinds` anchor. SAFE. |
| Glossary.md L4-5, L12 | Dated campaign framing; `CLAUDE.md` called "(authoritative workflow)". | L12: "Companion docs: [`Architecture.md`](Architecture.md) and the agent rules under `.claude/rules/`." Leave L2's Liquid guard and the `<a id="mvp">` anchor alone. SAFE. |
| CIMatrix.md L14-18, L56, L182, L7/L17 | Contradicts itself: "only ever compiled cross-target" vs "compiled clean since"; "no schedule" vs the daily census. Points to CLAUDE.md for content that moved. | L182: "no merge-gating schedule (the one daily darwin census is a regression guard)". Rewrite L16-19 in present tense: "darwin compiles on both Apple architectures, checked daily". Point L7/L17 at `.claude/rules/` instead of CLAUDE.md. NEEDS-OWNER: an internal page full of process vocabulary. |
| TargetAtlas.html | "Nothing else in the top 100 is converted" may mislead now. | NEEDS-OWNER: a dated, generated survey. Recommendation: leave it, or add one dated caveat line, never a rewrite. |
| LICENSING.md "Gbtc" header token | Legacy marker. | Not a docs-seat item: a corpus-wide header change plus TestLicensing* guard work. |

## Appendix B. NEEDS-OWNER summary (one line each, with the recommendation)

1. **R6c**: keep a one-sentence "not yet tested end to end" note for mapped packages that have third-party deps. *Yes.*
2. **R10/L294**: keep the `fatih/color@v1.18.0` pin and its comment exactly. *Yes.*
3. **R14b / RM1**: state that real third-party modules convert and validate, with no module named. *Yes, once a
   module's passing reading is recorded in the tree (uuid's 54 of 54 is only predicted at this SHA).*
4. **R16**: trim Milestones to nine turning-point rows. *Yes.*
5. **R18**: add a nugetgo.net bullet to Purpose once the registry is public. *Owner times it.*
6. **RM3**: restate Real-world modules as current state plus next (uuid converted, packed and consumed; gojq/jwt
   next). The identity-token prerequisite is in the tree, so its paragraph goes. *Yes.*
7. **RM4/RM6**: the registry launch status and date (Q1 2027 vs sooner), and which module leads the first wave
   (B8 says uuid and jwt; hashset is the packed proof of concept). *Owner.*
8. **RM7 and CS2**: drop the release from the two prose sites and retire their checker rows. *Yes (cuts hop work); keeping them VERBATIM is the fallback.*
9. **Appendix A**: whether to touch the internal pages at all. *TIR status and the Glossary pointer only.*
10. **R11 / RM8**: say the walkthrough's colors show on Linux too, and drop fatih/color from the Roadmap's remaining
    Linux items. *Yes, after one Linux pty re-run against the published packages confirms it.*

## Appendix C. What the Background.md rewrite must keep, because this plan depends on it

- Line 1 is exactly `# Background` (TestDocsPagesTitleFromTheirFirstHeading), so README L49's `Background.md#background` resolves.
- A heading `## How fast is converted code?`, with no emoji in it: R15 retargets README L542's "but not always" link
  to `Background.md#how-fast-is-converted-code`, because the rewrite moves its parity sentence there. Keep
  `## Why convert Go to C#?` as is too (the old target), also with no emoji. An emoji changes the generated id, and
  may produce different ids on GitHub and on the site.
- The rewrite drops `versioned \`1.24.13.<build>\`` and `packages whose Go 1.24.13 sources actually define`. Retire
  migrate-gorelease sites 8 (ps1:435-441) and 9 (ps1:442-448) in the same commit series, and amend the ps1:348-349
  comment (0.2). Otherwise the census exits 1 at the current pin as soon as the anchored prose is gone, in the seat
  itself.
- The anchors the draft links still exist after this plan: `README.md#milestones`, `#converting-a-real-world-module`,
  `#try-it-yourself--validate-a-converted-test-suite`, `#real-standard-library-conversions-side-by-side`,
  `ValidatedTestPackages.md#excluded-packages` and `Roadmap.md#platforms`; it also links `KnownIssues.md`,
  `Performance.md` and `ConversionStrategies.md` without a fragment. All resolve at this SHA, and this plan keeps them.
- **Land Background in the same seat as RM1, RM3 and RM4.** Its modules bullet and its nugetgo.net sentence agree
  with those edits, not with the Roadmap at this SHA, so two public pages would disagree if it landed alone. The
  nugetgo.net sentence also needs the owner's launch word; without it, drop that one sentence. (NEWS.md entries are
  dated and stay as written; the README NEWS block is held verbatim by §2e.)
- First person, unsigned. TestNoFleetIdentifiersInTrackedFiles refuses the owner's given name in any tracked file,
  and the pre-push census refuses the surname in prose. GitHub handle URLs are fine.
- No relative `.md` link text wraps across lines.
