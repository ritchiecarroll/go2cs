# Background.md rewrite: notes for review

Draft: `/h/go2cs-tmp-coord/coord-scratch/docs-refresh/Background.md` (ready to drop in as `docs/Background.md`).
Source read: `docs/Background.md` at 8fbc1b0a13 (TRAIN N union), using `git show` only. Nothing in the tN worktree was read directly or touched.

**Revision 2 (review pass).** The draft now carries the three reviews' fixes: the allocation-test example is corrected (the
blocker), the speed story is told once, one idiom replaces two, and the honesty close points at the live Known issues page.
Every finding and its disposition is in `REVIEW-NOTES.md` beside this file. `Background.editor-pass.md` is the reviewer's
intermediate pass; the draft supersedes it.

## Length

| | Lines | Words (wc -w) | Bytes | Read time (about 230 wpm) |
|:--|--:|--:|--:|--:|
| Current page (8fbc1b0a13) | 51 | 1,727 | 11,770 | 7 to 8 min |
| First draft | 49 | 1,245 | 8,118 | about 5 min |
| This draft | 49 | 1,155 | 7,408 | about 5 min |

The 80-word opening quote is kept verbatim, so the prose itself is about a third shorter than the current page. The longest
paragraph is now about 95 words; on the current page it is 233.

## Structure: before and after

| Before | After |
|:--|:--|
| `# Background`, then the 2018 quote and the "That was 2018 … eight years" bridge | `# Background`, then the same quote. The bridge is now dateless ("That's how this project [started](README.md#milestones)"). |
| (the ANTLR to Go move only hinted at: "two full converter architectures") | **New short paragraph:** the C#/ANTLR4 converter, then a restart in Go that reads code with `go/ast` + `go/types`. This is the one transition beat the brief allows. |
| `## Why convert Go to C#?`, seven bold paragraphs | `## Why convert Go to C#?` (**heading verbatim**), five parallel "To …" reasons: use Go from .NET; migrate with your eyes open; give Go code another place to run; get C# you can read; learn |
| "Honest engineering" and "What isn't true yet" (inside Why) | `## Where it stands`: a four-bullet "what works today" list, then "even roses have thorns" and four thorns, then a two-line close |
| "head start" line in the middle of the page | moved to the end as the sign-off |
| `## Converted Code`: TL;DR, CIL/JIT, philosophy, cgo | `## How fast is converted code?`: TL;DR, the shape of the numbers (the only place speed is described), why the gap exists, and the cgo aside. Renamed because nothing links `#converted-code` (I grepped the whole tree at 8fbc1b0a13, frozen validation pages included). |

Voice tells kept from the owner's own edits: the 2018 quote; "personal itch"; "gotten a lot better than 'I was curious'";
"things I can *show* you instead of argue about"; "compiling is not the same as working, and I try never to blur the two";
"keep the receipts"; "conversion isn't a one-way door" (now the closer of its paragraph); "fix a bug in the C# on a Tuesday
afternoon"; "*code in Go, run in .NET*"; "worth exactly as much as its worst *undisclosed* defect"; "Nothing here is graded
on a curve"; "That's the tool I wanted for myself" (without "in 2018"); "You don't have to take my word for it"; and the
**`TL;DR`** device. The dashes are kept as "--", along with italics for emphasis and bold lead-ins.

Removed from the owner's own wording, each an owner call to reverse (see `REVIEW-NOTES.md`, owner questions):
- the 2020 line "That doesn't mean your converted code is going to be super slow -- it should run as fast as other
  comparable .NET applications -- it just may not run as fast as the original Go". The TL;DR already says the first half,
  and the "comparable .NET applications" clause is not something the suite measures.
- "something until now unheard of" after *code in Go, run in .NET*: it invites "what about earlier transpilers?", and
  "until now" dates itself.

Expressive register (the owner's brief asked for it, in moderation):
- "even roses have thorns 🌹" (one of the brief's two examples; "peaches and cream" was dropped so the two don't stack)
- "did what every developer dreads and started over"
- "nobody understands Go quite like Go does"
- "all-in on .NET", "leap of faith", "keep the receipts 🧾"
- "the impossible, or the not-yet"
- "travels lighter on memory"
- "don't expect it to crawl"

Four emoji (😄 🧾 🎉 🌹), all in body text, each on a beat that earns it. None is in a heading, so no heading id changes.

## Facts the draft rests on (all read at 8fbc1b0a13)

- **The ANTLR to Go transition.** README Milestones (the 2018 inception row: "C#/.NET converter built on an ANTLR4 Go grammar";
  the 2025 rewrite row) and NEWS "converter is rewritten in Go": `go/ast` + `go/types`, the ANTLR4/C# converter retired.
- **"Every standard-library package that *can* be validated passes … on Windows and on Linux".**
  - README NEWS and Status: every implementable package validates; on Linux every applicable row does.
  - README links `ValidatedTestPackages.md#excluded-packages` for the packages outside that set; the heading is at
    ValidatedTestPackages.md:1025.
  - Each roster row links a proof page (README NEWS).
- **"hundreds of small Go programs".** README Status counts the behavioral projects in the high hundreds. "Hundreds"
  survives growth.
- **The NuGet and `-recurse=nuget` walkthrough, output matching `go run`.** README "Converting a real-world module" (the
  fatih/color walkthrough) and the Common options table.
- **nugetgo.net.** README options table: `-recurse=nuget` maps third-party modules through the nugetgo.net registry by
  default. The draft's sentence is evergreen ("is the community registry for converted modules"); Roadmap at this SHA
  still lists the registry launch as planned, so the page lands with RM4 and on the owner's launch word (see below).
- **Real third-party modules.** `-tests -recurse` (moduleTestsDriver.go) and third-party proof pages
  (validationProofPages.go) are in the tree; the coordinator context says modules now pass their own tests. The draft
  claims only that modules "convert too, and are judged the same way", which the shipped pipeline supports.
- **macOS.** Roadmap "Platforms": the library compiles for Darwin in CI, and the macOS run layer is being built. Hence
  "compiles there, and the work to run and validate it is under way", linked to `Roadmap.md#platforms` so progress shows
  there without another edit here.
- **Performance wording.**
  - Performance.md intro and README Performance: maps and the stack-string path at parity or faster in both C# variants;
    most compute within a small multiple; the structural-interface assert is the honest outlier.
  - String rows run several times Go's time in both variants; StringMatch is a few times Go's.
  - Go wins startup (Startup row).
  - Memory: Go is lower on every row except Map, where Native AOT is lower. Hence "travels lighter on memory", a
    general statement, not "always".
  - The cause of the gap ("a runtime library emulates" strings, slices and interfaces, plus readable rather than tuned
    output) follows the page's own Reading-the-results notes (`slice<T>` header emulation, `@string`, interface emulation).
  - Not one figure is quoted on the page.
- **The test-disclosure thorn.** ValidatedTestPackages.md's Disclosures table at this SHA:
  - "impossible": `runtime-capability` ("a facility defined over the replaced runtime's own internals, which the managed
    runtime cannot truthfully describe") is the draft's "inner workings of Go's own runtime"; `codegen-liveness` ("an
    object it just stopped using is collectible; the CLR can still report that slot live") is the draft's collectibility
    example. `structural` covers the few provably unmeetable allocation asserts.
  - "not-yet": `deferred`, the largest class, is "an allocation-count assert the CLR can meet in principle; it carries
    Go's want, a measured reading and a retirement plan". README:527-531 and Roadmap "Allocations" say the same.
  - The committed-file link is unchanged: `src/core/bytes/go2cs_test_disclosures.json` exists, and its entries are
    `deferred` ones, which matches the sentence that carries the link.
- **`-tags purego`.** README Status, and ConversionStrategies "The standard library reproduces Go `-tags purego`".
- **Known issues.** `docs/KnownIssues.md` exists at this SHA and is the live, present-tense page for faults a user can hit.
- **The Tour of go2cs.** `src/tour/README.md`: the Tour of Go beside the generated C#, live.
- **External links, checked live when first drafted:**
  - `https://pkg.go.dev/cmd/cgo#hdr-C_references_to_Go` (the anchor is confirmed). It replaces the old golang.org link.
  - `https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.dllimportattribute`. It replaces the
    .NET Framework-era msdn link.
  - `https://learn.microsoft.com/dotnet/core/deploying/native-aot/`, with the old tab pin dropped.
  - `https://en.wikipedia.org/wiki/Just-in-time_compilation`. The CIL link is gone with the CIL sentence.

## What was dropped, and why

- **The "158x Go … about 6x" interface anecdote and its `Performance.md#history` link.** It can't be checked: Performance.md
  at 8fbc1b0a13 has no such figure, and its History section holds earlier *toolchain* tables, not corrected-bug tables.
- **The first draft's allocation example** ("Go can put a value on the stack where .NET must use the heap", given as
  something no managed runtime can meet). Wrong for most of those tests: the largest disclosure class is allocation
  asserts .NET *can* meet in principle, each with a retirement plan. The current page made the same mistake.
- **The `time.Second` story and its link to a `docs/phase4` FINDING record.** It narrates a fixed bug and points a
  first-time reader at an internal ledger file. Known issues is the live page.
- **Exact figures, versions and dates.**
  - "Eight years", "2018" (outside the quote), and the testable-package count with its release.
  - The behavioral-project count, and the workload count.
  - The dependency-project count, the versioned package family, the measured-against release (twice), and "over 30x".
- **History narration:** "even after the `@string` window redesign", and the Phase 4 "ongoing effort" sentence with its
  RoadmapHistory link (Phase 4's goal is met).
- **README-level detail:** `go.lib`/`go.gen` package names, the `src/version.props` link, the local-deployment aside, the
  test-host pipeline mechanics, the frame-slot liveness class by name, and the Roslyn-generator clause (too technical here;
  "the machinery behind Go's semantics stays out of sight" carries it).
- **Repetition:** speed was described in three places and is now described once; the second "verdict for verdict"; the
  second "compiling is not working"; the duplicated JIT/AOT discussion; two "judge for yourself" lines; two idioms in a row.
  The Integration paragraph and the old "head start" line are merged.

## Companion edits owed IN THE SAME SEAT (not made; outside this draft)

1. **`src/migrate-gorelease.ps1`, sites 8 (:435-441) and 9 (:442-448).** The draft removes both anchored strings
   (``versioned `1.24.13.<build>` `` and `packages whose Go 1.24.13 sources actually define`). Each row has `Expect = 1`,
   and the census marks a non-retired site whose count is not its Expect as `mismatch` and exits 1 **at the current pin**,
   so `pwsh src/migrate-gorelease.ps1 -Quiet` goes red in the seat itself as soon as the prose is gone, not only at the
   next hop.
   - Retire both rows in the same commit series, on the precedent at :352-357 and :488-495. Keep the rows, and add
     `Retired` plus `RetiredNote`.
   - Keeping the rows also keeps Background.md in the DOC-STATEMENT file set, so the REVIEW scan still reports a release
     literal if one ever comes back.
   - Use the descriptive `Retired` form (the census only prints it, and a SHA goes wrong if the seat is rebased). A
     suggested shape:

   ```powershell
   @{
       File = 'docs/Background.md'; Class = 'DOC-STATEMENT'
       Find = 'versioned `{OLD}\.<build>`'
       Replace = 'versioned `{NEW}.<build>`'
       Expect = 1
       Retired = 'the docs-refresh commit that rewrote docs/Background.md (git log -S on this anchor, -- docs/Background.md)'
       RetiredNote = 'the page was rewritten release-neutral by owner order; the published package version family is stated in docs/README.md and on nuget.org'
       Note = 'the published package version family a consumer sees on nuget.org'
   }
   @{
       File = 'docs/Background.md'; Class = 'DOC-STATEMENT'
       Find = 'packages whose Go {OLD} sources actually define'
       Replace = 'packages whose Go {NEW} sources actually define'
       Expect = 1
       Retired = 'the docs-refresh commit that rewrote docs/Background.md (git log -S on this anchor, -- docs/Background.md)'
       RetiredNote = 'the page was rewritten release-neutral by owner order; the completion-goal denominator is defined on docs/ValidatedTestPackages.md (the roster of record)'
       Note = "the completion-goal denominator's definition"
   }
   ```

2. **`src/migrate-gorelease.ps1:348-349`, a comment.** It reads "The present-tense corpus release is stated in
   docs/README.md, docs/ValidatedTestPackages.md, docs/Background.md and docs/ConversionStrategies.md, each anchored below".
   It already omits docs/Roadmap.md although Roadmap site 7 is active. Rewrite the list to what the seat leaves:
   docs/README.md and docs/ValidatedTestPackages.md, plus docs/Roadmap.md if RM7 is skipped, plus
   docs/ConversionStrategies.md if CS2 is skipped.
3. **`docs/README.md:542`, the "but not always" link.** It targets `Background.md#why-convert-go-to-c` to back the parity
   claim. The parity sentence now lives under How fast, so retarget it to `Background.md#how-fast-is-converted-code` in the
   same commit (REFRESH-PLAN R15 carries the edit). Keep the `## Why convert Go to C#?` heading as is anyway.
4. **Land with the Roadmap edits.** The modules bullet and the nugetgo.net sentence agree with REFRESH-PLAN RM1, RM3 and
   RM4, not with the Roadmap at this SHA, so Background lands in the same seat as those three. The nugetgo.net sentence
   needs the owner's launch word; without it, drop that sentence (the bullet's first sentence stands alone).
5. **No runbook edit is owed.** GoCorpusMigration.md H12 (around :3485) defers to the script's doc-statement class and
   doesn't name Background.md. `docs/phase4/REHEARSAL-go12312.md:81-82` names the two rows, but it is a record and is not
   edited.
6. **Nothing else changes.** `src/go2cs.slnx:232` keeps `../docs/Background.md` (the filename is unchanged).

## Inbound and outbound links (verified at 8fbc1b0a13)

- **Inbound links still resolve.**
  - README:49 links `Background.md#background`; the heading is kept.
  - README:542 links `Background.md#why-convert-go-to-c`. The heading is kept, so the link resolves either way, but its
    promise ("but not always") now lives under How fast; companion edit 3 retargets it.
  - No other inbound links exist, including from the frozen validation pages.
- **Outbound relative links,** checked against heading slugs at 8fbc1b0a13. Every one resolves:
  - `README.md#milestones` (:552), `#converting-a-real-world-module` (:232),
    `#try-it-yourself--validate-a-converted-test-suite` (:483), `#real-standard-library-conversions-side-by-side` (:93)
  - `ValidatedTestPackages.md` and `#excluded-packages` (:1025)
  - `Roadmap.md#platforms` (:109)
  - `KnownIssues.md`, `Performance.md`, `ConversionStrategies.md`
  - in-page `#how-fast-is-converted-code`
- **New headings:** `#where-it-stands` and `#how-fast-is-converted-code`. They have no emoji.

## Guard self-checks (manual; the repoguard tests were not run, since nothing could be placed in a tree)

- **Title:** the page opens with `# Background`, with nothing above it (TestDocsPagesTitleFromTheirFirstHeading).
- **Liquid:** no `{{` or `{%`.
- **Line breaks:** every paragraph and list item is one line, so no link text spans a line break, and no line starts with
  `N. `.
- **Raw HTML and tables:** none, so no pipe table and no escaped-pipe code spans.
- **Identifiers:** no given name, surname, host, user name or path. The handle appears only inside
  `github.com/ritchiecarroll/…` URLs and the NuGet query URL, exactly as on the current page.
- **Release literals:** none. A grep for `1.2x`, `go1.`, and any 20xx year comes back clean, so the migrate-gorelease REVIEW
  scan has nothing to report on this file.

## Things I was unsure about (owner's call)

1. **The ANTLR to Go rationale** ("a grammar only knows what Go code *looks like* … needs to know what it *means*") is my
   inference. No public doc states why the rewrite happened. Please confirm it's the real reason, or swap in yours.
2. **nugetgo.net's launch.** The sentence is written as if the registry is live. It needs your launch word, or it comes out
   until then.
3. **Your 2020 "super slow" line** and **"something until now unheard of"** were cut (see above). Either can come back; if
   the 2020 line returns, the TL;DR goes, never both.
4. **"Peaches and cream"** was dropped so it doesn't stack with the roses line. If you prefer it, it can replace the roses:
   "Of course, it's not all peaches and cream."
5. **The quote is still a blockquote** (your words, verbatim, including the comma splice). It is your signature opening, so
   it stays whole unless you want it trimmed.
6. **The Performance numbers** were measured on an older Go pin (a rerun is queued, per README). The wording is abstract
   enough to survive the rerun.

## Side finding (for whoever refreshes Performance)

`docs/Performance.md` "Reading the results" (the Memory bullet) says the AOT working-set floor is *higher* than the JIT's.
The current .NET 10 table shows AOT *lower* on all fourteen rows, and the page's own History section says that penalty
"inverted, and did so universally". REFRESH-PLAN now carries the fix as P9 (in `src/tests/Performance/README.md`, then
re-mirrored). The current Background page repeated that stale claim; the draft drops it.

## REVISION 3 (owner rulings 2026-10-03)

**The owner's rulings (binding).** These supersede "Things I was unsure about" items 1, 3, 4 and 5 above, and the
Revision 2 idiom and opening choices.

1. The ANTLR-to-Go reason is right; it stays.
2. The idiom is "everything is not peaches and cream 🍑", word for word (the roses line is gone).
3. His two original lines are back, in his words: the "super slow ... comparable .NET applications" line in the TL;DR,
   and "*code in Go, run in .NET* -- something until now unheard of".
4. The drawbacks end at the Known issues link.
5. The opening blockquote is dropped, and nothing reads "itch" or "scratch". The page opens on a short first-person
   paragraph instead.

His sixth item (cut down the README Milestones) is about the README, not this page, so it is not applied here.

**What changed in this pass:** the TL;DR setup is now "Go is usually faster.", so his line is the payoff, not an echo. The
paragraph after the drawbacks ends on the link. "Conversion isn't a one-way door" and the 🎉 are cut. The modules bullet
now describes what go2cs can do, not a result. Native AOT's build cost is disclosed. A fifth drawback ("Not every Go
module converts cleanly yet") is added. A `---` rule sets off the sign-off. The rest are wording fixes. The page now has
50 lines, 1,202 words and three emoji (😄 🧾 🍑).

**Checks re-run on the draft:** line 1 is `# Background`; both required headings are verbatim, with no emoji in any
heading; there is no blockquote; a word-bounded search for `\b(itch|scratch)` finds nothing (a plain substring search
still hits the account handle inside the URLs, which is expected); the owner's name, hosts, TypeScript, years, `go1.`,
`ANTLR4` and Liquid are all absent; every link text sits on one line; and all four protected phrases are present once.
Every relative link target and anchor exists at 59ee0d21bf (`git show` only). The anchors are README :93, :232, :483 and
:552, ValidatedTestPackages :1025 and Roadmap :109.

**Findings, reviewer A** (12):

- A1 should-fix, TL;DR repeats itself: **APPLIED** as suggested. The backtick-in-bold `TL;DR` part is **DECLINED**,
  because it is the owner's own device (ace1ea3527, 2020).
- A2 should-fix, a sentence after the Known issues link: **APPLIED**. "Nothing here is graded on a curve." is deleted
  (ruling 4; a401b35cf2 era, not one of his rulings).
- A3 should-fix, "one-way door" is ambiguous: **APPLIED** (cut). It now reads as C#-to-Go, which README's "C# to Go?"
  section says is not offered.
- A4 nit, the README walkthrough is pointed to twice: **APPLIED**. The link moves onto "walks through one", and the
  sentence says "the converted app's output matches `go run`" (README :340).
- A5 nit, "with one command" said twice: **APPLIED** ("reproduce that yourself ... from a fresh clone").
- A6 nit, "It took two tries" is ambiguous, and ANTLR4 carries a version: **APPLIED** ("The converter took two tries";
  "the first was"; "an ANTLR grammar"). The reason itself is untouched (ruling 1).
- A7 nit, "instead of argue about": **APPLIED** ("rather than argue about").
- A8 nit, stack-string jargon and "honest outlier": **APPLIED** ("the big outlier"; "maps, and the string comparisons the
  converter can optimize"). Performance.md :12 and its StringView row (:26) back it.
- A9 nit, "yet" three times: **APPLIED**.
- A10 nit, four emoji: **APPLIED** (🎉 dropped; 🍑 kept per ruling 2).
- A11 nit, the sign-off sits under the performance heading: **APPLIED** with a `---` rule. README :10 and
  ConversionStrategies already use thematic breaks, and no repoguard test objects.
- A12 nit, the itch check note: **NO PAGE CHANGE**, confirmed by the word-bounded check above.

**Findings, reviewer B** (9):

- B1 should-fix, the modules bullet reads as a result: **APPLIED**. It now says "`go2cs -tests -recurse` judges a whole
  module the way the standard library is judged: by its own test suites" (README :195 and the `-tests` options row). I dropped
  the reviewer's "one command" wording so that "one command" isn't repeated (A5).
- B2 should-fix, a sentence after the Known issues link: **APPLIED** (deleted, same as A2). I chose the delete option
  over moving the line to the front, as the plainest reading of "Stop at known issues link".
- B3 should-fix, Native AOT's costs are not mentioned: **APPLIED** at the performance paragraph, with no figures: "which
  closes most of that gap, but takes a long time and a lot of memory to build and makes a much bigger executable".
  Performance.md :146-149 (startup: AOT is several times faster than the JIT, and a gap to Go remains) and :214-229 (hours
  per build, peak memory in the mid-teens of GB, images of hundreds of MB) back it. Line 13 is unchanged; ruling 3 governs
  its tail.
- B4 should-fix, the migrate-gorelease sites: **DECLINED as a page edit** (outside this file). Companion edits 1 and 2
  above already carry it. The sites are unchanged at 59ee0d21bf (:435-448, comment :348-349), so the census still goes
  red at the current pin unless the seat retires both rows.
- B5 nit, nugetgo.net is presented as launched: **DECLINED for now**. The launch question is not among today's rulings,
  and README at 59ee0d21bf (mapping on by default) supports the sentence while Roadmap :158-174 does not. Companion edit 4
  stands: the sentence lands only with RM4 and the owner's launch word, and is dropped otherwise.
- B6 nit, no drawback for real-world modules: **APPLIED** as a new bullet before macOS. README :376-378 backs it ("A
  dependency closure is not always convertible today", and `-recurse=module`).
- B7 nit, the grammar fix: **APPLIED** (same as A7).
- B8 nit, README :542 lands on the wrong section: **DECLINED as a page edit**. Companion edit 3 / R15 already carries
  it, and the `## Why convert Go to C#?` heading stays verbatim.
- B9 nit, the itch check note: **NO PAGE CHANGE** (same as A12).
