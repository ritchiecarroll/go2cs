# REVIEW-NOTES: the docs-refresh review pass

Applied to `Background.md`, `Background-notes.md` and `REFRESH-PLAN.md` in this directory. Every claim a finding made was
re-read at `8fbc1b0a132c501c305e9c8620750a62c8a75d23` with `git show` / `git grep` / `git log` / `git merge-base` only
(nothing in the tN worktree was read directly, run or written). `Background.editor-pass.md` is reviewer B's intermediate
pass; `Background.md` supersedes it.

Result: 38 findings. 36 applied (two of them with a precondition left for the seat), 1 declined, and 1 applied in part.

## Review A (16 findings)

| # | Sev | Finding | Disposition |
|:--|:--|:--|:--|
| A1 | blocker | Background's "impossible" example is allocation counts, but the largest disclosure class (`deferred`) is allocation asserts .NET *can* meet in principle, each with a retirement plan | **APPLIED.** Confirmed at ValidatedTestPackages.md:452-455, and the linked `bytes` manifest holds only `deferred` entries. The thorn now reads "**Some Go tests ask for the impossible, or the not-yet.**" The impossible ones are described from `runtime-capability` (Go's runtime internals) and `codegen-liveness` (collectibility); the not-yet ones are allocations with a retirement plan. I avoided the suggested "a handful", because the impossible classes are not small. |
| A2 | should-fix | R11 keeps "colors do not show yet on Linux", and the Roadmap still lists the terminal query, but `2b31027b77` fixed it before the release | **APPLIED, with a precondition.** Confirmed that `2b31027b77` records TCGETS ok and a byte-identical pty colordemo, and that it is an ancestor of both `65be55315f` and release `5732917a95`. R11 now says the colors show on both platforms, and the new **RM8** drops fatih/color from Roadmap Platforms L115-117. RM8 keeps a pointer, because PLAN-linux-operation still names open items (F8, F9/N0, F12). Both wait on one Linux pty re-run against the published packages; the plan gives the fallback wording. The antecedent is reworded: "at or after the commit that published them". |
| A3 | should-fix | RM3 keeps the "Before gojq" identity-token paragraph as pending, but `ж.PointerTokens.cs` implements it | **APPLIED.** Confirmed `IdentityBand`, "The id is UNIQUE", and the gojq mention. The paragraph, the "not confirmed" sentence and the BOARD instruction are gone. The wrap caveat is offered as an optional owner line. |
| A4 | should-fix | Section 3 misses the Performance Memory bullet ("AOT floor higher"), which contradicts all 14 rows | **APPLIED as P9.** I corrected the suggested wording: AOT sits *below* Go on Map, so "sit well above Go" became "well above Go on every row but Map". The Startup bullet's "see the memory note below for its trade" is dropped. |
| A5 | should-fix | PG1 says the managed runtime supplies every raw-metal body; Roadmap Phase 5 says the rest compile against throwing stubs | **APPLIED** with the suggested wording, and the Phase 5 anchor was checked against the existing double-hyphen anchor convention. |
| A6 | should-fix | RM3 says uuid was "validated", but the record measured 51/54 and only predicts 54/54; consume legs are win-x64 only | **APPLIED.** Confirmed (SIZING-j0-uuid.md:139, :231-232). RM3 now says "converted, packed and consumed from a local feed, with its output matching `go run`". R14b's evidence calls 54/54 a prediction and adds the reading as a precondition. I did not take the reading: it is a test run, out of scope for a docs pass. |
| A7 | nit | R8: the 30m floor applies to every publish, not only the first | **APPLIED.** Confirmed at testConversion.go:6722/:6728. |
| A8 | nit | R16 overstates the generators' role, and miscounts the rows | **APPLIED.** The row now names `golib` plus generators, and the count is 24 rows at L558-581. |
| A9 | nit | R15 "broader optimization is ongoing" overstates | **APPLIED.** Now "planned", linking `Roadmap.md#performance` (heading at L254; allocation kickoff Q1 2027). |
| A10 | nit | T2 rationale is wrong (the line is MUST-NOT-CHANGE, ps1:558), and the census reds at the *current* pin, not only at the next hop | **APPLIED.** Confirmed both, the second in the state logic around ps1:723 and :812. 0.2 now explains why retirements go in the same commit series (R13, RM7, CS2, Background), and the site 8/9 row and Appendix C were reworded to match. |
| A11 | nit | Appendix C lists anchors the draft doesn't link and omits ones it does | **APPLIED.** The list is now the revised draft's actual outbound links (adding `Roadmap.md#platforms` and `KnownIssues.md`, which the review pass introduced). All were checked at the SHA. |
| A12 | nit | Speed comes up three times; the TL;DR and "super slow" sentence repeat each other | **APPLIED.** Speed is now described once (How fast). The thorn is one line, and the "super slow" sentence is cut (owner question 4). |
| A13 | nit | Lead-in parallelism; "coming online" goes stale; modules claim ahead of Roadmap | **APPLIED.** The lead-in is now "To get C# you can read." The registry sentence is evergreen and needs the owner's launch word (owner question 2). Appendix C now requires Background to land in the same seat as RM1, RM3 and RM4. |
| A14 | nit | C1 says a future raise breaks `field`, but LangVersion 14 already makes it a keyword | **APPLIED** with the suggested wording. Confirmed Directory.Build.props:45 and go2cs-gen.csproj:27. |
| A15 | nit | R5 drops "when one qualifies"; RM4 omits hashset | **APPLIED.** R5 has its qualifier back. RM4 now names no module ("the first published modules"), with the hashset vs uuid/jwt choice raised as an owner question (B8 vs §6 of PLAN-nugetgo). |
| A16 | nit | T1 should link the reference page anchor, not the summary | **APPLIED** (optional, but clearly better). It now links `ConversionStrategies-Reference/comments.md#where-a-comment-is-attached-and-where-it-is-not` (heading at comments.md:13). |

## Review B (15 findings)

| # | Sev | Finding | Disposition |
|:--|:--|:--|:--|
| B1 | should-fix | Two idioms stacked word for word; "everything is not peaches and cream" is garbled | **APPLIED.** Roses kept and merged with "worth exactly as much as its worst *undisclosed* defect, so here are mine". Peaches dropped (owner question 5). |
| B2 | should-fix | Performance repeated in three places; How fast gives no spread | **APPLIED.** The thorn is one line, and How fast carries the shape (small multiple, strings several times, interface outlier, maps and stack strings at parity or faster). "Super slow" cut, TL;DR kept. To avoid a second "usually" next to "It's usually slower", the memory clause reads "travels lighter on memory". |
| B3 | should-fix | "More on why below" explains only startup; "as fast as comparable .NET applications" is unbacked | **APPLIED.** The "Why the gap?" paragraph covers emulated strings, slices and interfaces, readable-not-tuned output, and startup. The comparable-apps claim and the CIL link are gone. |
| B4 | should-fix | "Proof of the pudding 🍮" doubles "don't take my word for it" | **APPLIED.** The pudding line is cut, "You don't have to take my word for any of this" kept. |
| B5 | should-fix | Line 17 makes three points; "until now unheard of" overclaims; line 15's heading over-promises reach | **APPLIED.** The heading is now "To give Go code another place to run." *code in Go, run in .NET* moves there, and "until now unheard of" is dropped (owner question 4). |
| B6 | should-fix | Modules bullet uses "now being" / "coming online", and disagrees with NEWS.md and the Roadmap | **APPLIED in part.** The evergreen wording is taken, and it lands with RM1, RM3 and RM4. Not taken: "land at the same time as NEWS". NEWS.md entries are dated announcements that stay as written, and the README NEWS block is held verbatim by check-roster-format §2e, so there is nothing to change there. |
| B7 | should-fix | Honesty paragraph repeats "I'd rather", narrates a fixed bug, and links a docs/phase4 record | **APPLIED.** The paragraph now closes with two lines linking `KnownIssues.md` (present at the SHA). The `time.Second` story is offered back as owner question 6. |
| B8 | should-fix | macOS thorn wording goes stale | **APPLIED**, with one accuracy change. "Validation is under way" became "the work to run and validate it is under way", because the Roadmap describes the macOS run layer as still being built. It links `Roadmap.md#platforms` (L109). |
| B9 | should-fix | README:542 links `#why-convert-go-to-c` for the parity claim, which moves to How fast | **APPLIED.** R15's block now targets `Background.md#how-fast-is-converted-code`. Notes companion edit 3 and Appendix C say so, and `## Why convert Go to C#?` is kept as is. |
| B10 | nit | Emoji: drop 🐢 and 🍮, move 😄, add 🎉 | **APPLIED.** Four emoji now (😄 🧾 🎉 🌹), none in a heading. |
| B11 | nit | Line 13 says "compiles is not working" twice | **APPLIED.** Only the owner's line is kept. |
| B12 | nit | "So conversion isn't a one-way door" breaks the "To ..." pattern | **APPLIED.** It is now the paragraph's closer. |
| B13 | nit | Line 7 is heavy (Roslyn clause); line 5 "writing it into" is unclear | **APPLIED.** It now reads "reads code with the same packages Go's own tools use", and "by turning it into". The ANTLR rationale stays owner question 1. |
| B14 | nit | "Construct for construct" repeats; the cgo aside lacks its reason | **APPLIED.** "doubles as a phrasebook between the two languages"; "Need Go's own speed?" |
| B15 | nit | Trim the 80-word opening blockquote | **DECLINED.** It is the owner's signature quote, verbatim, and the reviewer marked it his call. Raised as owner question 7. |

## Review C (7 findings)

| # | Sev | Finding | Disposition |
|:--|:--|:--|:--|
| C1 | should-fix | 0.4 step 5 relies on a lane push triggering docs-site.yml, which only triggers on master, PRs and dispatch; R5/R6 depend on R6b's anchor | **APPLIED.** Confirmed the triggers and the master-only deploy job. Step 5 now runs `gh workflow run docs-site.yml --ref <seat-branch>` (or a PR) and requires `0 new` (the site-check output format) before landing. R6b now states that R5, R6 and RM4 land with it, falling back to `#common-options`. |
| C2 | nit | The ps1:348-349 comment already omits Roadmap | **APPLIED.** 0.2 and the notes give the exact list each choice produces. |
| C3 | nit | `Retired = '<sha>'` goes wrong on a rebase; the census never checks it | **APPLIED.** The descriptive form is preferred; the SHA form only after the final rebase. |
| C4 | nit | T2 rationale wrong | **APPLIED** (same as A10). The ps1:558 rule becoming inert is noted as harmless. |
| C5 | nit | K1 overstates the csprojTemplate guard; the page becomes vaguer than the emitted comment | **APPLIED.** The K1 note and 0.3's KnownIssues constraint are reworded, and csproj-template.xml stays untouched. Whether to keep "about 90 ms" so the two agree is owner question 9. |
| C6 | nit | 0.4 step 6's expected `1.24.13` leftovers are incomplete | **APPLIED.** It now lists sites 1-3, 5 and 6, NEWS L14/L21, and the milestone tags (R16's kept row, or L577-581 if R16 is not taken). |
| C7 | nit | Appendix C anchor list is from the old page | **APPLIED** (same as A11). |

## Readings owed before the seat lands (not owner decisions, but nothing here ran them)

1. **Linux pty re-run** of the README walkthrough against the published packages. It gates R11's "on both platforms" and
   RM8.
2. **A module's `-tests` reading recorded in the tree**: uuid at a tip that includes D4, or the hashset module's. It gates
   R14b, RM1's module clause and RM3's "validated".
3. **`gh workflow run docs-site.yml --ref <seat-branch>`** reporting `0 new`, plus `pwsh src/migrate-gorelease.ps1 -Quiet`
   exiting 0 with the Background, R13, RM7 and CS2 retirements in the same commit series.

## Questions only the owner can decide

1. **The ANTLR to Go rationale** in Background ("a grammar only knows what Go code *looks like* … needs to know what it
   *means*") is the drafter's inference. Is that the real reason, or do you want your own words there?
2. **nugetgo.net's launch.** Background says "[nugetgo.net] is the community registry for converted modules", as if it is
   live. Say the word, or that sentence comes out until launch. The same answer sets RM4's "Still to come" bullet and the
   RM6 timeline row (Q1 2027 or sooner).
3. **Which module leads the first published wave**: hashset (the packed proof of concept) or uuid and jwt (PLAN-nugetgo
   B8)? RM4 names none until you say.
4. **Your own lines that were cut from Background**: the 2020 "That doesn't mean your converted code is going to be super
   slow …" sentence, and "something until now unheard of" after *code in Go, run in .NET*. Either can come back; if the
   2020 line returns, the TL;DR goes.
5. **One idiom or two**: roses (kept) or "it's not all peaches and cream" in its place.
6. **The `time.Second` story** (a fixed bug, linked to an internal finding record) or the **Known issues** link (current)
   to close the thorns. One, not both.
7. **The opening quote**: verbatim (current) or trimmed with ellipses.
8. **gojq's identity-token wrap caveat**: one optional line in RM3, or nothing (the work itself is done).
9. **KnownIssues.md cost figure**: abstract ("adds a little startup time", K1 as proposed) or keep "about 90 ms" so it
   matches the comment every converted csproj carries.
10. Still open from the plan's Appendix B (unchanged by this pass): R6c, the L294 fatih/color pin, R16's milestone trim,
    R18's timing, RM7 and CS2, and the Appendix A internal pages.

## Not in this pass

The first-wave template tweak (drop "If you would rather this conversion not be listed") belongs to the first-wave
drafts. The string does not occur anywhere in the tree at this SHA, as the plan's header already records.
