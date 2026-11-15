# Audit: the user-facing docs before 1.26.11.1

> **State: RECORD (2026-11-15), P4.** A findings list with proposed fixes, ranked wrong > stale > unsupported >
> wording. No visitor page is edited in this cut; the fixes follow as docs seats (section 7) and one nugetgo PR.
> Read against master `b2214e00e4` (batch 308). The pass over batch 309's tree and the delta pass at the release cut
> are amended below as dated blocks. Point-in-time: amend with dated blocks, never rewrite.

## 1. Scope and method

**Pages.** The link-walk of [AUDIT-user-facing-docs.md](AUDIT-user-facing-docs.md) section 1 (README, depth 1 and
depth 2), widened as ordered (COORD 2026-11-15 17:55Z):

| Set | Pages | Reader |
|---|---|---|
| 0 | `README.md`; `NEWS.md` (intro, the visible held 1.26.11.1 block, the November 8 entry); `DRAFT-news-1-26-10-4.md` (the 1.26.11.1 entry) | R1 |
| 1a | `Limitations`, `KnownIssues`, `Roadmap`, `FAQ`, `Features`, `GoFeatureSupport` | R2 |
| 1b | `CommandLine`, `DiagnosticCodes`, `DotnetTool`, `BuildingGo2cs`, `ConvertingAModule`, `ConsumingGoFromCSharp`, `PublicSurface`, `Architecture`, `src/tour/README.md` | R3 |
| 1c, 2 | `Tuning`, `Performance`, `CIMatrix`, `Glossary`, `RoadmapHistory`, `Background`, `BeforeAndAfterTheFaceLift`, `StdLibCompileMilestone`, `ValidatedModules`, `news/*.md`, `ValidatedTestPackages` (header, prose and footnotes; rows scanned for the rules) | R4 |
| 1d | `ConversionStrategies.md` (all visible prose) | R5 |
| ref | `ConversionStrategies-Reference/` sub-pages, part 1 (`comments`, `constants`, `empty-interface`, `expression-switch`, `manual-conversions`, `named-numeric-types`, `nil-and-zero-values`, `packages-that-do-not-type-check`, `purego`, `source-generators`, `struct-types`) | R6 |
| ref | `ConversionStrategies-Reference/` sub-pages, part 2 (`compiled-library-vs-source`, `defer-panic-recover`, `golib-namespace`, `implicit-dereferencing`, `multi-assignment`, `multi-result-and-comma-ok`, `pointers`, `slices-and-arrays`, `strings`, `struct-embedding`, `variable-initialization-order`) | R7 |
| ref | `ConversionStrategies-Reference/` `README` and `deterministic-output`, `floating-point-formatting`, `generic-constraints`, `interfaces`, `labels-and-loop-variables`, `maps-and-channels`, `native-and-narrow-integers`, `package-conversion`, `shadowing`, `type-aliasing`, `type-switch`, `value-receiver-delegates` | R8 |
| site | nugetgo.net: `ritchiecarroll/nugetgo` at main `03583c7fd4` (README, CONTRIBUTING, the generated index, the `v1/mappings.txt` header) | P4 |

`NugetgoPublish.md` is out of scope: `docs/_config.yml` excludes it from the site (it is the owner's runbook).

**Method.** Two passes. (a) Scripted, over the 68 pages: every relative link and `#anchor`; every converter flag
compared both ways with `go2cs -h` built at the tree (every `-h` flag has a CommandLine row and the reverse; every
default agrees); every version, RID, count and process-jargon line listed with its page:line (43 jargon, 299
version, 462 platform, 225 figure lines) and handed to the readers. (b) Eight readers, each given the standing rules
and a page group, read every visible line and quoted each finding with page:line and its evidence at the tree. HTML
comments were not audited as text, but were used as evidence for the claim beside them. P4 re-read at the tree every
claim section 2 rests on (each is marked **re-read**).

**Rules applied** (COORD 2026-11-15): present tense on reference pages (a NEWS entry is history, but must be true);
every platform or measured claim names its platform or RID; no exact corpus figure unless a standing process
re-measures it; nothing claimed that the tree does not do, Native AOT publish included (that line follows P1's
reproduction, still running); cgo states its phase (Phases 1, 2 and 3.1 in 1.26.11.1); the TypeScript and sqlite
demos are described only as far as their gates have read; no internal process names, machine names or visible links
into internal records; links resolve; pasteable commands work as written.

**A rule correction made during the audit.** The reader brief carried the 1.26.10.3 audit's rule "no TypeScript on
visitor pages". COORD's rule for this audit is narrower: the demos may be described as far as their gates have read.
The readers' TypeScript findings are therefore not counted as rule breaks here; they are listed once for the owner in
section 4 (Q1).

## 2. Summary

**244 findings over the 68 pages:** 29 wrong, 54 stale, 27 unsupported, 134 wording (TypeScript findings moved to
Q1 and not counted). Sections 5.1-5.4 hold 199 of them; R6's 45 are in 5.5. nugetgo.net: 2 wording findings and one
release action.

The findings fall into six themes. Fix them as themes, not page by page, or the same claim stays wrong somewhere else.

1. **Platform parity is under-stated almost everywhere.** The roster carries a linux-arm64 reading on 236 of 236
   applicable rows and a darwin reading on 236 of 236 (**re-read**: `ValidatedTestPackages.md:25` for linux-arm64;
   265 `darwin:` segments, the two rows without one being the Windows-only `internal/syscall/windows` pair). Yet
   README:24-26, 116-118, 218-220; Limitations:17, 428-434; Roadmap:129-131, 184-186, 296-297, 307-311, 567; FAQ:34,
   73-76; GoFeatureSupport:152-153; Background:9; ConvertingAModule:158-166; and the roster's own
   `ValidatedTestPackages.md:551-553` say macOS is "in progress" or name only x64. Section 3 drafts the replacement.
2. **The arm64 axis is missing from how-to and reference text.** CommandLine:63 (`-tests` default arch);
   ConvertingAModule:318-320 and PublicSurface:108-109 (no `GoTargetArch`); ConversionStrategies:1038, 1044, 12388
   (folders `amd64/`, `linux-arm64/`); package-conversion:1759 (`linux/arm64` maps to `linux-arm64`,
   **re-read** `moduleConverter.go:1106`); CIMatrix:160, 220, 228, 262, 264, 274, 378 say "four shipped RIDs" where
   os-matrix runs five (**re-read** `os-matrix.yml:475` "all five shipped RIDs"); CIMatrix:94 still calls the arm leg a
   calibration and says `sweep-shard` is refused there.
3. **The Go 1.26.11 hop left 1.26.10 in present-tense text.** README:113 "you need Go 1.26" (go.mod requires 1.26.11,
   **re-read**: README lines 110-115 at the tree); Limitations:419; DotnetTool:27, 44-46, 60; Performance:121;
   ConsumingGoFromCSharp:80; src/tour/README:119; and the roster header itself: `ValidatedTestPackages.md:22-23`
   "Every row's columns are its own Go 1.26.10 Windows measurement" against its own line 4 "its own Go 1.26.11 test
   suite" (**re-read**), plus 865-868, 1656, 1663 naming the 1.26.10 population file the guard no longer reads.
4. **Fixed defects still listed as limits, and retired machinery described as current.** Limitations:215-216 lists
   the type switch `case int:` catching `int32`, fixed in 1.26.10.2 (**re-read**: `visitTypeSwitchStmt.go:305` "an
   int32 (or rune) must NOT take the int arm"; fixture `TypeSwitchIntWidthCases` exists), and GoFeatureSupport:82
   points at it "(status to confirm)". multi-result-and-comma-ok:56 and pointers:936-946 describe golib behavior
   replaced in October; slices-and-arrays:183 describes the copying span the view doors replaced.
5. **Wrong claims about emitted code.** ConversionStrategies:4443, 4767, 6303, 7113, 9301 write the index-only range
   as `range(s)`; the converter emits `range(len(s))` (**re-read**: `visitRangeStmt.go:777` and the page's own table
   and examples). PublicSurface:23 and ConsumingGoFromCSharp:67-69 say every exported package variable is a `ref`
   property; only an address-taken one is (**re-read**: `strconv/number.cs:278` and `io/io.cs:42` are plain `public
   static` fields). value-receiver-delegates:375 says `main` becomes `Main`, against package-conversion:30.
6. **Lists that missed new codes.** CommandLine:39 omits `GO2CS2010` and `GO2CS2011` from the `-recurse` refusal
   list (**re-read**: `conversionDriver.go:393, 403` both call `refusePackage`); CommandLine:78 omits `GO2CS2011`;
   Features:38-42 says "except two generic forms" while `GO2CS2005` and `GO2CS2006` also refuse generic shapes;
   Architecture:118-120 and source-generators:69 name two of the four analyzers; purego:7 says `-recurse` does not
   apply `purego` (it does, `commandLineOptions.go:312`). The roster's disclosure-class table (953-971, 1302-1305)
   says `performance-margin` has no entries; `src/core/net/go2cs_test_disclosures.json:36` holds one (**re-read**).

**What is clean.** DiagnosticCodes.md (every code, side and severity checked against the three registries); every
flag on CommandLine.md against `-h`; StdLibCompileMilestone, ValidatedModules, BeforeAndAfterTheFaceLift (each
"after" example re-read at its cited file); two news pages; eleven reference sub-pages (sections 5.4 and 5.5); every visible
relative link and anchor on the 67 pages; every "Guarded by" name on the reference pages but one (pointers:3114).

## 3. Draft for the owner: three-OS roster parity

The claim needs a home a process re-measures. The roster header has guarded lines for the headline and for Linux
(both arches), and **no macOS line**. So the draft is in two parts, and the numbers appear only in the guarded header.

**(a) Roster header, a new line beside the Linux line** (`ValidatedTestPackages.md`, after line 25). It needs a
`check-roster-format.ps1` arm that sums the `darwin:` segments the way the Linux line sums its annotations, so the
counts are re-measured on every roster change. Wording, with the numbers the guard would fill:

> **macOS: 236 of 236 applicable rows validated, on Intel (osx-x64) and Apple silicon (osx-arm64)** — N matching
> verdicts · D disclosed · 2 rows platform-exclusive (`darwin: n/a`), the same two Windows-only rows as Linux.

**(b) One parity sentence, also in the roster header** (it can quote counts because the guard backs every number in
it), tied to the roster version:

> **Every package that applies to a platform is validated on it.** In the roster for go2cs 1.26.11.1: Windows, 238 of
> 238; Linux, 236 of 236 on x64 and on arm64; macOS, 236 of 236 on Intel and on Apple silicon. The two packages
> missing from Linux and macOS are Windows-only (`internal/syscall/windows` and its `registry` child).

**(c) README, `## Status` (lines 218-220), without numbers,** linking the roster:

> The whole Go standard library converts and compiles as .NET assemblies, and every package that can be validated
> matches its own Go test suite in C#, verdict for verdict, with every difference disclosed, on Windows (x64), on
> Linux (x64 and Arm64) and on macOS (Intel and Apple silicon).

The README's opening paragraph (24-26) and Requirements (116-118), and every page in theme 1, change to match.

**Two things to settle before it ships.** (1) The roster header's "Go 1.26.10 Windows measurement" (theme 3): the
hop re-read only the rows whose converted code moved, so either the header says that, as the NEWS draft already does
("Every validated test package whose converted code moved was run again at Go 1.26.11"), or the Windows columns are
re-read at 1.26.11. The parity sentence inherits whichever is true. (2) `ValidatedTestPackages.md:551-553` says one
`darwin:` annotation "covers both chips: a row gains it only when Apple silicon and Intel agree". That is wrong at the
tree: the per-RID form banks a row whose chips differ, with both readings shown (**re-read**: the
`go/internal/gcimporter` row, line 701, carries `darwin: 636 (osx-x64; osx-arm64: 634)`, and the guard parses that
form, `check-roster-format.ps1:217-234`). The sentence must say each chip has its reading, as (a) does.

## 4. Questions for the owner

- **Q1. TypeScript and sqlite mentions.** Under the rule "described as far as their gates have read", these stand as
  accurate and need only a decision on whether they belong on the page: Roadmap:151, 158, 333, 567 (the 1.26.11.1
  gate; line 158's "byte-identical ... at least under the JIT" matches the emit gate, Linux MET at `091935257c`,
  Windows MATCH at `651c4ad147`); Tuning:140 (`parsedemo` row, linux-x64) and 233-235; the NEWS draft 521-530, 813-814
  (which needs a RID: "on linux" becomes linux-x64), 880-881. Reference pages name typescript-go or sqlite as the
  source of a shape: generic-constraints:277, interfaces:1306, shadowing:456, 870 ("sqlite-cs row S7", an internal
  ledger name, is a wording finding regardless), 880. Recommendation: keep the gate statements on Roadmap and NEWS,
  replace the source names on reference pages with the shape itself.
- **Q2. The visible held NEWS block.** NEWS.md:11-88 publishes "November XX" and nine "PENDING" markers today, and
  repeats items of the draft (assembly renaming, the break floor). Recommendation: comment it out now and merge it
  into the draft's single entry at release.
- **Q3. Exact counts in the NEWS draft** (223 "13 lines in four files", 308 "15 internal members", 667 "seven such
  labels"). A NEWS entry is dated history; rule 3 says no exact corpus figure without a re-measuring process.
  Recommendation: allow dated counts in NEWS, and fix KnownIssues:112, whose heading and anchor carry "15" on a
  reference page.
- **Q4. PublicSurface:23 is a frozen promise the tree does not keep** ("An exported package variable is a `ref`
  property"). Either the page states the real mapping (a static field, or a `ref` property when Go takes its
  address), or the converter changes before the surface freezes. Recommendation: fix the page.
- **Q5. Native AOT lines** (inventory in section 5.2). They wait for P1's two-row reproduction (a local `-recurse`
  tree; a `-recurse=nuget` tree). Until then the audit proposes only adding linux-arm64 where pages list the RIDs the
  release check publishes on (KnownIssues:300-304 and Roadmap:285-287, which must stay word-identical), and naming the
  RIDs on FAQ:126-129 and GoFeatureSupport:143 once P1 reads.

## 5. Findings by severity

Each line: page:line, the claim as read (shortened), what the tree says, the fix. The readers' full quotes and
evidence are kept with the seats.

### 5.1 Wrong (24)

- **README:113** "To build the converter from source instead, you need Go 1.26" — `src/go2cs/go.mod` requires
  `go 1.26.11`; under `GOTOOLCHAIN=local` (step 1 of the same page) an older 1.26 refuses. Fix: "Go 1.26.11 or later".
- **Limitations:215-216** type switch `case int:` catches `int32`/`rune` — fixed in 1.26.10.2 (NEWS.md:418;
  `visitTypeSwitchStmt.go:305`; fixture `TypeSwitchIntWidthCases`). Fix: delete the bullet.
- **Roadmap:307-309** "A roster row gains its macOS count only when both chips agree" — the per-RID form banks
  differing chips (line 701). Fix: "the roster shows each package's macOS count; where the chips differ, both".
- **FAQ:73-76** "Windows and Linux on x64 ... macOS has packages" — linux-arm64 and darwin are validated on every
  applicable row. Fix: section 3 wording.
- **Features:38-42** "except two generic forms" — `GO2CS2005` and `GO2CS2006` also refuse generic shapes
  (DiagnosticCodes:42-43). Fix: "except a few generic forms, which it refuses by name".
- **GoFeatureSupport:153** `linux/arm64` "Packages ship for it (status to confirm)" — validated, 236 of 236. Fix:
  state validated; keep "with limits" only for the arm64-assembly members (KnownIssues:112).
- **CommandLine:39** `-recurse` refusal list of five — `GO2CS2010` and `GO2CS2011` also go through `refusePackage`
  (`conversionDriver.go:393, 403`). Fix: add them, or "any `GO2CS2xxx` refusal"; fix the `recurseRefusal.go:18`
  comment too.
- **ConsumingGoFromCSharp:67-69** "exported package-level value variables such as `os.Args` are ref properties" —
  only an address-taken variable is (`io.EOF`, `strconv.ErrSyntax` are fields; the page's own table at 197 agrees).
- **PublicSurface:23** "An exported package variable is a `ref` property" — as above (Q4).
- **Background:9** walkthrough output matches `go run` "on windows/amd64 and linux/amd64" — release-smoke gates it on
  all five RIDs (CIMatrix:170-171). Recorded under wrong because it contradicts Roadmap:307; fix with theme 1.
- **CIMatrix:94** the arm leg "is a calibration ... `sweep-shard` ... stay refused" — the flavour exists and
  `sweep-shard` is lifted (`os-matrix.yml:439-450`). Fix: present tense, four stages still refused.
- **CIMatrix:160, 188, 220, 228, 262, 264, 274, 378** "ALL FOUR shipped RIDs" — five (`os-matrix.yml:468-475`).
- **CIMatrix:207, 376** cgo job "one per shipped RID" — four of five; linux-arm64 has no pinned C toolchain yet
  (`$cgoUnpinnedRids`). Note batch 309 may retire this exception; re-read at 309.
- **ValidatedTestPackages:22-23** "Every row's columns are its own Go 1.26.10 Windows measurement" — contradicts
  lines 4 and 538 (Go 1.26.11). Fix: section 3, settle (1).
- **ValidatedTestPackages:551-553** "One annotation covers both chips ... only when [they] agree ... macOS validation
  is in progress" — section 3, settle (2). (P4's own reading.)
- **ValidatedTestPackages:971, 1302-1305** `performance-margin` "has no entries" — `net`'s
  `TestDialerFallbackDelay` darwin entry (`src/core/net/go2cs_test_disclosures.json:36`).
- **news/2026-07-18-utf16-validates:20** "disclosed a day earlier" — both announcements are dated July 18. Fix:
  "earlier the same day".
- **ConversionStrategies:4443, 4767, 6303, 7113, 9301** (5 findings) index-only range as `range(s)` — emitted
  `range(len(s))` (`visitRangeStmt.go:777`; the page's table at 7088 and the examples above each line).
- **package-conversion:78** "measured on all four shipped RIDs" — five ship; the cited runs are linux, darwin and
  windows. Fix: name the RIDs measured.
- **package-conversion:1759** `GoCompileRuntimeIdentifier` "linux → linux-x64" — `linux/arm64` → `linux-arm64`
  (`moduleConverter.go:1106`).
- **pointers:3114** guard `TestArrayPointerViewDoor` — the test is `TestArrayPointerViewDoorReplacesTheCopyingSpan`
  (`arrayPointerViewDoor_test.go:190`).

### 5.2 Stale (44)

- **README:24-26, 116-118, 218-220** macOS "a package is validated when its row carries a macOS count"; "Windows or
  Linux on x64" — theme 1, section 3.
- **Limitations:428-434, 462** "macOS ... is being validated package by package"; `encoding/json` and
  `text/template` "on Windows and Linux" — both rows carry darwin counts.
- **Limitations:419** "building the converter from a clone needs Go 1.26" — 1.26.11.
- **KnownIssues:300-304** Native AOT release check "on `linux-x64`, `win-x64` and `osx-arm64`" — five legs including
  linux-arm64 (`os-matrix.yml:468-476`); align with P1 (Q5).
- **KnownIssues:306-313** full-trim and publish-time readings "with the 1.24.13.5 packages" / "Go 1.24.13" — two
  release lines old. Fix: re-measure on 1.26.11.1 packages (aot-smoke gives per-RID times), or label as 1.24-era.
- **Roadmap:129-131, 184-186, 296-297, 307-311, 567** macOS "under way", "in progress" — theme 1.
- **Roadmap:285-287** as KnownIssues:300 (keep word-identical).
- **Roadmap:415-417, 425-427** stub-reach and throwing-stub inventories "on the Go 1.24 standard library" — re-run at
  1.26.11 or say they predate the Go 1.26 corpus and the linux-arm64 build.
- **GoFeatureSupport:82** type switch "(status to confirm)" pointing at the deleted limit.
- **GoFeatureSupport:152** darwin "validation is package by package" — theme 1.
- **CommandLine:63** `-tests` "defaults to the host's OS on amd64" — a linux/arm64 host keeps linux/arm64
  (`testsCorpusArch.go:60-73`, `platformLattice.go:48`).
- **CommandLine:78** `-warn` refused-code list lacks `GO2CS2011` (`warnOption.go:35-44`).
- **DotnetTool:27, 44-46, 60** "Go 1.26.10 for go2cs 1.26.10.x", cache and archive names `go1.26.10.*` — pin is
  `go1.26.11` (`toolchainPins.go:21`). Fix: `go<release>` with 1.26.11 as the example.
- **ConvertingAModule:158-166** "Windows (`windows/amd64`) and Linux (`linux/amd64`)" — add linux/arm64.
- **ConvertingAModule:318-320** `-p:GoTargetOS=linux` only — on Linux Arm64 also `-p:GoTargetArch=arm64`
  (`src/core/Directory.Build.props:55-58` defaults arch to amd64).
- **ConsumingGoFromCSharp:80** example pin `GoStdLibVersion=1.26.10.3` — below the 1.26.11.1 break floor the same
  page describes. Fix: a placeholder or the current release.
- **PublicSurface:108-109** `GoTargetOS` alone — add `GoTargetArch`.
- **Architecture:118-120** "Two analyzers" — four (`MethodLimitAnalyzer`, `RefusalDowngradedAnalyzer` too).
- **Glossary:140-141** Native AOT "the third column" — the fourth (ReadyToRun was added).
- **Performance:37-55, 147** "all 17 of the suite's programs" — the suite holds 18 (`PerfDeferPanic`, added after
  the table). Fix: "the 17 programs measured".
- **Performance:121** "Requirements: Go 1.26.10" — 1.26.11.
- **Tuning:100, 188, 316, 440** "of the 17 programs" — as Performance.
- **Tuning:497-498** every goroutine reserves 256 MiB / 16 MiB — `main` reserves 1 GiB (`MainThread.cs:47-50`).
- **RoadmapHistory:863-873** undated "Progress tracking" table says Linux validation "in progress" — date it or drop.
- **RoadmapHistory:875-880** "open converter items" lists done work (`visitMapType`, `-recurse`) and cgo as open.
- **ValidatedTestPackages:865-868, 1656, 1663** population file and figures "Go 1.26.10's" — the guard reads
  `population-go1.26.11.txt` (`check-roster-format.ps1:1043-1047`); line 868 also prints a `docs/phase4/` path.
- **ConversionStrategies:1038, 12388** "converted for Windows, Linux and macOS, each on amd64"; per-OS folders only —
  linux/arm64 and the `amd64/`, `linux-arm64/` folders.
- **reference README:21, 29** heading levels describe the single-page layout; 15 `##` rules have no index entry.
- **generic-constraints:645** "The refusal is fatal" — `-warn GO2CS2002` downgrades it (`main.go:303`).
- **package-conversion:1287** "A package that imports "C" is still refused ... C functions ... are not members yet" —
  cgo Phases 1, 2 and 3.1: calls of shared-library C functions convert under `-cgo` (`go2cs -h`).
- **value-receiver-delegates:375** "Go's `main` is emitted as ... `Main`" — `main` keeps its name; go2cs-gen
  generates the entry point (package-conversion:30). The runtime comment at `managed_impl.cs:2160-2162` is stale too.
- **multi-result-and-comma-ok:56** `TryConstructInterfaceWrapper` as current — retired; the rule is
  `AdapterBinder.TryCreate`.
- **pointers:936, 938, 946** `ж<T> → uintptr` answers 0 for a nil box; a native operand AVs — since 2026-10-16 the
  null case answers an order token (`ж.cs:1324-1333`), and a native operand gets a native view (`array.cs:311`) or a Go
  panic (`ж.cs:1287`).
- **slices-and-arrays:183** pointer-cast slice "copies" via `ReadOnlySpan` — now `@unsafe.Pointer.View<E>` doors
  (`arrayPointerViewDoor.go:18-30`; `runtime/heapdump.cs:61`).
- **slices-and-arrays:505** prologue `ref Ꮡt.Value` — the golden has `ref Ꮡt.DerefOrNull()`.

### 5.3 Unsupported (18)

Measured figures with no platform: KnownIssues:144-149 (16,000 goroutines; the source is linux-x64 under WSL);
DotnetTool:34 (download 65-75 MB); CIMatrix:370-371 ("on a laptop"); RoadmapHistory:63-64 (cold 48 s);
ValidatedTestPackages:1165 ("a 6-core host"); ConversionStrategies:8714 ("Go's goroutines reach about a million");
defer-panic-recover:64; pointers:2275; struct-embedding:326 (platform not recorded); Tuning (one finding: tables name
the platform but not the package version or build measured, unlike Performance). Exact corpus figures no process
re-measures: KnownIssues:112-118 (15 members, in the heading and anchor); ValidatedTestPackages:953-969 (disclosure
classes "as of 2026-10-01"; a raw count at the tree differs on every class); strings:133 (8 packages per target);
struct-embedding:681 (107,835 forwarders); package-conversion:1406 (66 dependencies, no version or target);
value-receiver-delegates:184 ("fires nowhere in the corpus today"); the NEWS draft 223, 308, 667 (Q3).

### 5.4 Wording (113)

Grouped; the readers' quotes travel with the seats.
- **Internal process names in visible text**: PublicSurface:49 (COORD); CIMatrix:94-95 (seat S3, S6, COORD R4);
  RoadmapHistory:474 (a scratch path), 573 (chip); ValidatedTestPackages:749 (BOARD), 1108 (F15b), 1165 (rung);
  reference README:96 and nil-and-zero-values (doctrine); deterministic-output:5; package-conversion:809 (honesty
  doctrine); value-receiver-delegates:227; the NEWS draft's 37 `###` headings carrying owner and seat tags and
  "sqlite-cs"; shadowing:870 ("sqlite-cs row S7").
- **Visible paths into internal records**: interfaces:177 (`docs/phase4/probes/...`), package-conversion:1764
  (`docs/phase4/DESIGN-multiplatform-corpus.md`), ValidatedTestPackages:868. The frozen roster snapshots
  `docs/validation/1.23.12.3` to `1.26.10.1` link `../../phase4/CENSUS-testing-osuser-rows.md` visibly (the current
  page and the 1.26.10.2 and 1.26.10.3 snapshots keep it in a comment); the current roster links two of those
  snapshots, so a visitor reaches the link in two clicks. Owner call: amend the frozen snapshots or accept as history.
- **Goroutine limits stated for one OS only**: Limitations:26, 85-93; FAQ:41 ("thousands are fine"; macOS ends near
  2,000 on Apple silicon, Linux near 16,000).
- **Generic refusals listed as one**: Limitations:36, FAQ:59-61, GoFeatureSupport:90 (add `GO2CS2005`, `GO2CS2006`).
- **Supported icons that contradict Limitations**: GoFeatureSupport:39, 42 (arrays, structs), 118 (`reflect`).
- **cgo phase labels**: Limitations:252, 256 say refused items wait for "Phase 2", which ships; the refusal text says
  "Phase 2 later". Fix: "a later step of Phase 2", here and in the Roadmap's cgo "Later".
- **Draft placeholders on published pages**: GoFeatureSupport:111 "(status to confirm)".
- **Other**: README:110-112 against 184-186 (no-Go tool vs "the release you installed"); Roadmap:330-331 (gojq cli
  is validated, not packed); CommandLine:51-59 (`-h` shows back-quoted usage words as argument names, a code fix in
  `main.go`); CommandLine:80 (`-debug` vs `GO2CS2008`); BuildingGo2cs:119-120 (link the codes page directly);
  src/tour/README:109 (`-tool-timeout=15m` reads as the default; it is 10m), 119 (`-nuget-version 1.26.10.1`);
  Background:43 and Glossary (four performance columns); Tuning:515-526 (`-inline-arrays`, `-trimpath`, `-warn`
  missing from the options table); news/2026-07-26:150-151 ("within a few percent (1.16×)"); ConversionStrategies:1044,
  1846 (`ᵇ` missing from the glyph table), 6609 (`[GoInit]` timing); interfaces:189-197 and type-switch:119, 124
  (".NET 9" measurements in present tense; the tree targets net10.0); package-conversion:223 vs 234 (1,724 vs 1,725);
  the NEWS draft 366-368 (omits `internal/platform`) and 899-900 (badge order); R7's 34 reference-page wording
  findings (sentence fragments, history in present tense, shapes described by their fix rather than their rule).

### 5.5 R6, reference part 1 (45: wrong 5, stale 10, unsupported 9, wording 21)

R6 reported after sections 5.1-5.4 were written, so its findings are listed here by class rather than merged.
- **Wrong.** purego:7 says `-recurse` defaults to the `safe` tag alone and "the purego default does not touch
  -recurse end-user conversions" — `defaultModuleBuildTags` is `safe, purego, noasm` (**re-read**:
  `commandLineOptions.go:312`). manual-conversions:3326 says `unsupportedRuntimeCapabilities` "is now **empty**" —
  it holds live entries (**re-read**: `testConversion.go:4378`). source-generators:69 "two analyzers" — four, and two
  generators (`ExtensionHostListGenerator`, `BoundedGoroutineInterceptorGenerator`) are missing from its list.
  empty-interface:270 a named untyped complex const "renders as `GoBigConst` ... with its own standing TODO" — the
  `ComplexConstContext` golden emits `UntypedComplex`, as constants:152-177 says. struct-types:1289 cites an
  `elementBoxViaAt` idiom that exists nowhere in `src/`.
- **Stale.** The arm64 flavour again: manual-conversions:35 (the linux flavour "cannot run on linux/arm64 ... and
  needs a flavour of its own" — it has one), 104 (`pollServerInit` refuses non-amd64 — arm64 is mapped), 1129 (the
  amd64 arch layer serves arm64 — `crc32/linux-arm64` exists). manual-conversions:478 (promoted-field search
  "deferred" — `reflect/value_impl.cs:4250` implements it); named-numeric-types:218 (composite literal "not yet
  handled" — handled, the page's own line 228); empty-interface:476 and purego:5, 23 ("Go 1.26.10");
  manual-conversions:352, 1832 (.NET 9 measurements).
- **Unsupported.** Exact corpus figures across manual-conversions (413-437, 635, 1802, 2261, 2767, 2836, 4396-4397)
  and struct-types:539; measured figures with no platform at manual-conversions:367, 388, 3336, 3345, 3463-3485 and
  struct-types:1537.
- **Wording (21).** History in present tense and internal names across manual-conversions, expression-switch,
  named-numeric-types, nil-and-zero-values (including the "doctrine" heading) and struct-types.

Clean: comments, constants, packages-that-do-not-type-check. manual-conversions lines about 2130-4660 were scanned
by targeted search, not read line by line.

## 6. nugetgo.net

Read at `ritchiecarroll/nugetgo` main `03583c7fd4` and the live site. go2cs reads `https://nugetgo.net/v1/mappings.txt`
by default under `-recurse=nuget` (`src/go2cs/main.go:284`) and honours `canonical`, `community` and `withdrawn`
(`nugetMap.go:139`); the live file has 9 rows, and each package resolves on nuget.org.
- **N1 (release action).** `cmd/sitegen/go2cs-releases.txt` lists go2cs releases up to 1.26.10.3. Check 4(b) refuses
  a package whose self-description names go2cs 1.26.11.1 until a maintainer adds that line. Add 1.26.11.1 in the
  nugetgo PR window of the go.* 1.26.11.1 publish, before any wave row PR.
- **N2 README:7 (wording)** "see Mapping modules to NuGet packages in the go2cs README" — the link is go2cs's
  ConvertingAModule page. Fix: "in the go2cs documentation".
- **N3 README:17 (wording)** "Browse published NuGet packages  with `nugetgo.*` prefix." — double space, missing
  article. Fix: "Browse the published `nugetgo.*` NuGet packages."

Clean: CONTRIBUTING (trust policy and checks 1-5 against `cmd/sitegen/checks.go`), the site text, the mappings header;
on the go2cs side, README's registry line and ConvertingAModule's mapping section agree with the site.

## 7. Fix seats (proposed)

1. **Parity and arm64 (themes 1, 2)**, after the owner reads section 3: the roster header line and its guard arm,
   then README, Limitations, Roadmap, FAQ, GoFeatureSupport, Background, ConvertingAModule, CommandLine,
   PublicSurface, CIMatrix, ConversionStrategies, package-conversion. Held to land with 1.26.11.1 where a sentence
   says linux-arm64 packages ship.
2. **The hop and the fixed defects (themes 3, 4, 6)**: versions, the type-switch limit, the refusal lists, the
   analyzers, the roster's population and disclosure tables.
3. **Emitted-code corrections (theme 5)** and the reference pages' stale and wrong findings (R5, R7, R8, R6).
4. **Unsupported and wording** after the owner's answers to Q1-Q5.
5. **nugetgo PR**: N2 and N3 now; N1 at the publish.

The Native AOT lines follow P1's reading (Q5). A short delta pass at the release cut re-reads whatever 309 and 310
changed on these pages.

## 8. Amendment, 2026-11-15 (owner-asked via COORD 18:36Z): wording that must hold in 1.26.11.1 and the next release

**The rule.** A present-tense sentence that is true at 1.26.11.1 but turns false when the next release lands an item
of Roadmap's "The next two releases" is reworded so it is true in both. Nothing predicts the next release in present
tense; the sentence just stops asserting what that release changes. `async` stays only where it names M:N's async
twins (none on a visitor page today). Version-qualified text (NEWS entries, roster rows, the release-notes draft)
stays as is.

**What the next release changes, as Roadmap and the rulings put it.** M:N scheduling arrives first as an option, a
runtime switch a program turns on (owner ruling 2026-11-12), while one thread per goroutine stays the default
(Roadmap, Performance). The same ruling reserves hidden twin members for it (the async twins of the functions
that can park; `DESIGN-mn-scheduler-sizing.md` option B is the selective lowering they come from), so the visible
converted code keeps its shape. So "by default" makes a thread claim
true in both releases, and a claim that goroutines are "not rewritten into `async`" is the one to drop. The cgo
steps (Go memory handed to C, cgo under `-recurse=nuget`, the `net` resolver) and door C lift refusals that pages
list today.

**Method.** A scan of the visitor page set (comments stripped) for `async`, `await`, own/OS/operating-system thread,
one thread, thread per goroutine, `GOMAXPROCS`, thread caps, and the cgo and door C refusals: 53 thread and async
lines, 20 cgo-shaped lines, each read in context.

### 8.1 For the owner first: the README header (docs/README.md:23-24)

As read: "Behavior comes first: a goroutine runs on its own thread and is not rewritten into `async`."

Proposed: "Behavior comes first: goroutines, channels, `select` and `defer` keep Go's semantics."

True at 1.26.11.1, and true under the next release's opt-in M:N, whose async twins are hidden from the converted
files the same sentence's paragraph describes.

### 8.2 Reword (true in both)

| Page:line | As read (shortened) | Proposed |
|---|---|---|
| Limitations:26 | "**Each live goroutine holds an operating-system thread.** Thousands alive at once are fine; Go's million are out of reach." | "**By default, each live goroutine holds an operating-system thread.** Thousands alive at once are fine; Go's million are out of reach." |
| Limitations:85, 89 | "**Goroutines are operating-system threads** ... `runtime.GOMAXPROCS(1)` does not make anything run one at a time." | "**By default, goroutines are operating-system threads** ... With threads, `runtime.GOMAXPROCS(1)` does not make anything run one at a time." |
| Limitations:113 | "break in with a debugger, where each goroutine is a thread named `goroutine-N`" | "...where, by default, each goroutine is a thread named `goroutine-N`" |
| KnownIssues:131, 146 | "Each goroutine is an operating-system thread, and macOS caps ..." / "... and on Linux each .NET thread uses ..." | "By default each goroutine is an operating-system thread, and ..." (both entries) |
| FAQ:40-41 | "A goroutine is an operating-system thread, so thousands are fine and hundreds of thousands are not." | "By default a goroutine is an operating-system thread, so thousands are fine and hundreds of thousands are not." |
| Features:61 | "Goroutines, each on its own thread (behavioral equivalence first — not rewritten into `async`)" | "Goroutines with Go's semantics, each on its own thread by default" |
| GoFeatureSupport:107 | "Go's semantics, on real threads, not rewritten into `async`. Each live goroutine holds an operating-system thread, so ..." | "Go's semantics, on real threads by default. Each live goroutine then holds an operating-system thread, so ..." |
| GoFeatureSupport:109 | "`runtime.GOMAXPROCS` ... Read and set, but it does not limit parallelism" | "Read and set; with the default threads it does not limit parallelism" |
| ConsumingGoFromCSharp:628 | "Each goroutine runs on its own thread for as long as it runs." | "By default, each goroutine runs on its own thread for as long as it runs." |
| Architecture:61 | "`go` (goroutine → its own OS thread, reused through `GoroutineThreadPool`)" | "`go` (a `goǃ` call; golib runs the goroutine on its own OS thread by default, reused through `GoroutineThreadPool`)" |
| Tuning:497 | "Each goroutine other than `main` runs on its own thread" (seat 2 wording) | "By default, each goroutine other than `main` runs on its own thread" |
| ConversionStrategies:8440 | "Each goroutine runs on an operating-system thread of its own, from start to finish. That is the main difference from Go" | "By default, each goroutine runs on an operating-system thread of its own, from start to finish. That is the main difference from Go" |
| ConversionStrategies:8633 | "**A goroutine keeps its own thread from start to finish.**" | "**By default, a goroutine keeps its own thread from start to finish.**" |
| ConversionStrategies:8701-8707 | "**Converted code is ordinary blocking code, not `async` code.** A goroutine pauses by blocking its thread ... golib cannot run many goroutines on fewer threads either, because .NET cannot move a running call to another stack. That is a property of .NET 10 ..." | "**Converted code is ordinary blocking code.** By default a goroutine pauses by blocking its thread, so each goroutine needs a thread of its own. `goǃ` does not use the .NET thread pool, because a blocked goroutine would hold a shared pool thread and starve the others." (the two sentences on what golib cannot do are dropped: the next release's opt-in scheduler does it) |
| ConversionStrategies:8713 | "**Operating-system threads bound live goroutines far below Go's limit.**" | "**With the default threads, the operating system bounds live goroutines far below Go's limit.**" (and the "about a million" figure goes, per finding 8714) |
| ConversionStrategies:12394 | "`runtime.GOMAXPROCS`, `Gosched`, `LockOSThread` and `Goexit`, on .NET threads" | keep: true in both (the M:N workers are .NET threads too) |
| manual-conversions:3198 | `GOMAXPROCS(n)`: "does **not** cap parallelism — the CLR schedules goroutine threads" | "with the default threads, does **not** cap parallelism ..." (reference page; the M:N seat re-reads this table anyway) |

### 8.3 Keep (true in both, or not about goroutines)

PublicSurface:79 (thread identity is not promised, which already
allows M:N); ConsumingGoFromCSharp:631 and 724 (`ReceiveAsync`/`SendAsync` are API names); CommandLine:50 (a shell
`cmd &`); manual-conversions:68, 75, 2798 (Windows overlapped I/O and `SemaphoreSlim`, not goroutines); package-conversion:53
(`main`'s thread); maps-and-channels:839; Roadmap:473-476 (future tense, already says "today" and "default").

### 8.4 .4-only by nature

NEWS and the release-notes draft (dated); roster rows ValidatedTestPackages:748-819 (version-qualified readings).

### 8.5 The cgo steps and door C: no wording holds both

Limitations:244 (the refused list, Go memory handed to C among it), Limitations:276-285 (the pointer rebuilt from a
number), FAQ:51 ("Other C uses are refused, by name, with the phase that covers them"), GoFeatureSupport:165, and
the Roadmap's "Next" lists. Each states a refusal the next release lifts. A version-free rewording would either
hide the refusal or predict the lift; a version-qualified one ("1.26.11.1 refuses ...") turns a reference page into
history. Recommendation: keep them, already phase-labelled as Limitations words them, and change each line in the
same seat that lifts its refusal. FAQ:51 already reads true in both ("refused ... with the phase that covers them").

## 9. Amendment, 2026-11-15: the pass over batch 309's tree (master `0c1d295ea8`)

309 changed twelve visitor pages: README (P4's own Explore lines), Limitations and ConversionStrategies (the cgo
pin-extent narrowing to three cases), the release-notes draft, and seven reference sub-pages (pointers +491 lines
for doors B and F1, slices-and-arrays, struct-embedding, interfaces, labels-and-loop-variables, manual-conversions,
variable-initialization-order).
- **Scripted, over the visible lines 309 inserted or replaced** (comments stripped, diffed against `b2214e00e4`):
  process names, TypeScript and sqlite names, internal record paths, exact counts and unplatformed measurements.
  0 hits; the scan was positive-controlled by a planted line, caught once. Every new "Guarded by" name (8) exists in
  `src/` at `0c1d295ea8`. repoguard (links, anchors, provenance) is green at 309 by the batch's own battery.
- **Read:** the new Limitations and ConversionStrategies cgo text states its three open cases in present tense and
  links Limitations' cgo section; no finding.
- **The record's findings still stand at 309**, re-read for the ones 309's pages touch: pointers:3114
  (`TestArrayPointerViewDoor`), ConversionStrategies' index-only `range(s)` (lines moved), slices-and-arrays:183 (the
  copying span) and :505 (`ref Ꮡt.Value`), manual-conversions:3326 ("now **empty**"). Seat 3 fixes them at 309.
