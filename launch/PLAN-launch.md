# PLAN: launching go2cs

> **State: DRAFT for the owner's review, 2026-10-11.** This branch (`claude/launch`) is a planning branch.
> It is never merged to master and never published on the site. Facts are stated as of the draft date and
> each links the page that carries the current figure. Every effort figure and every date range here is an
> **estimate**, derived as section 4 shows; none is a promise, and no gate is traded for a date.
>
> **AMENDED 2026-10-12.** Since the draft: 1.24.13.6 is published (Oct 12); the cold first-hour test
> passed on Windows x64; D3 (release binaries for the soft launch), D4 (the macOS certificate fix
> promoted) and D10 (a GitHub Release per NuGet release) are ruled; the issue forms, labels, About box
> and private vulnerability reporting are in place. Two things moved the dates: the comment and spacing
> parity work now lands **before** the hop, and the hop is re-sized to 100 to 165 lane-days, 4 to 5.5
> weeks. **The consequence, in one line: a December wide launch on Go 1.26 is now the optimistic case,
> not the expected one; plan for a wide launch on Go 1.27 in late January or February** (section 4, D7).
> Sections 2, 4 and 8 are rewritten to that; sections 1, 3 and 5 to 7 stand, with section 3's finished
> items listed at the top of section 8.

The question this plan answers: when should go2cs be announced widely, what has to be true first, where,
and how are the replies handled. The short answer is **two stages**. A **soft launch** to a few dozen
people after the 1.24.13.6 release, to find what a newcomer trips on. A **wide launch** after the Go 1.26
release, when the most common objection (the Go version) is gone.

Contents: [1. Where we are](#1-where-we-are) · [2. Work streams](#2-the-work-streams-and-their-order) ·
[3. Readiness checklist](#3-launch-readiness-checklist) · [4. Timeline](#4-timeline) ·
[5. Venues](#5-venues) · [6. Message handling](#6-message-handling) · [7. Draft posts](#7-draft-posts) ·
[8. Open decisions](#8-open-decisions-for-the-owner)

## 1. Where we are

Measured, and linked instead of restated:

- **The converted Go standard library passes Go's own tests on Windows and Linux (x64).** Every package
  that can be validated is validated, verdict for verdict against `go test`, with each difference
  disclosed by name. The current counts are in the
  [roster](../docs/ValidatedTestPackages.md). macOS validation is in progress, package by package.
- **Three third-party modules are converted, validated and published as NuGet packages**: `hashset`,
  `google/uuid` and `golang-jwt/jwt` v5, listed at [nugetgo.net](https://nugetgo.net)
  ([Validated Modules](../docs/ValidatedModules.md)).
- **The release on NuGet is 1.24.13.6** (2026-10-12).
- **The front page is new.** The README is a short first read with an FAQ, and the limits are written
  down plainly: [Limitations](../docs/Limitations.md), [Known issues](../docs/KnownIssues.md), and the
  [exported-reach census](../docs/phase4/CENSUS-phase5-exported-reach.md) of which calls can reach an
  unimplemented stub.
- **The repository** has 400 stars, 3 open issues, and Discussions switched on (read 2026-10-11).

What a newcomer will hold against it today, in the order they will meet it:

1. **The Go version.** It tracks Go 1.24.13, which is out of upstream support. The 1.26 hop fixes this.
2. **Install friction.** Clone, then `go install`. There is no one-line install.
3. **macOS HTTPS.** Verifying a certificate against the system roots reaches a stub on macOS, so an
   ordinary `https` request fails there.
4. **The glyphs.** Converted code uses letters such as `ж` that a C# reader has never typed.
5. **The license.** The converter is AGPL. The answer is good (generated code is not covered), and it has
   to be said before it is asked.

Two more that the brief did not list, found while drafting:

6. **How the project is built.** The commit history shows AI co-authorship on most commits. Go and C#
   forums both react to that, and the reaction is better when the project says it first (decision D6).
7. **GitHub's own front matter.** The Releases panel shows a 2022 pre-release, `v0.1.2`, as the latest
   release, and GitHub reports no detected license for the repository (read 2026-10-11). Both are
   first-glance signals of an abandoned or unclear project, and both are cheap to fix (checklist item 9).

## 2. The work streams and their order

| # | Stream | Depends on | Who | Estimate |
|:--|:--|:--|:--|:--|
| A | **The 1.24.13.6 release** | done | | published 2026-10-12 |
| B | **Three harder modules** as NuGet packages, rising in complexity | A | C2; the owner (sign, push) | 1 to 3 weeks, unmeasured until C2's proposal names them |
| P | **Comment and spacing parity**: converted code keeps Go's comments, blank lines and alignment | A | C1 leads, seat by seat; a train, then a release that carries it | the seats 3 to 6 more days at the pace so far, then a train and a release: about 1 to 2 weeks in all, an estimate with no plan figure behind it |
| C | **Soft-launch readiness** (section 3) | A | R, COORD, the owner | mostly done; what remains is in section 8 |
| D | **The soft launch**, then a week of listening and fixing | C | the owner posts; COORD triages daily | 1 week |
| E | **The Go 1.26 hop**, H0 to release, by the runbook | **P landed, then the final 1.24.13.N shipped** (the hop plan's O1). H0 is captured only after the last parity train | seven lanes and COORD, as the [hop plan](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-plan-hop-go126/docs/PLAN-hop-go126.md) section 7 proposes | **100 to 165 lane-days; 4 to 5.5 calendar weeks from H1** (amended 2026-10-12) |
| F | **Wide-launch readiness** (section 3) | D's findings; parts need E | C1 (macOS), R, COORD | 8 to 16 lane-days |
| G | **The wide launch** | E or H published, F done, a week of soak | the owner posts; COORD drafts replies | 1 week of presence |
| H | **The Go 1.27 hop** (generic methods, a design-sized feature) | E; its design drafted during E's H10 | to be planned | 60 to 100 lane-days, 11 to 21 of them language work (measured 2026-10-11, an estimate); by the 1.26 hop's ratio, about 2.5 to 3.5 calendar weeks |

**What runs in parallel, and what collides:**

- **B and C run alongside each other** from the release. B is C2's; C is R's and COORD's.
- **E's readiness stage can start during the wait for the final 1.24.13.N**, as the hop plan says.
- **P decides when E starts.** The parity work changes the text of every converted file, so the hop's
  starting baseline is taken only after the last parity train has landed, and the release that carries
  parity is the earliest candidate for the final 1.24.13.N. If the three modules (B) also need
  converter fixes, they ride that same release or one more (decision D2).
- **P helps the launch.** It is the change a first-time reader sees most: converted code that keeps
  Go's comments and spacing. A soft launch after the parity release shows better output than one
  before it (decision D1).
- **D overlaps E.** The soft launch brings issues while the hop is running. That is the reason to keep it
  small: a few dozen people, triaged daily, with fixes that can wait riding the hop.
- **R is in two streams.** R writes this plan's documents and is proposed for the hop's testing-host cut
  (H4) and its docs (H12). C comes first; the H4 cut does not start until the final 1.24.13.N ships.
- **C1 is in two streams.** The macOS certificate fix (F) and the hop's runtime share both sit with C1.
  The fix is a few days; it goes before C1's H6 share or after H7a, not during.
- **G is serial behind E**, by design. A wide launch on Go 1.24 would spend the one first impression on
  the objection the hop removes.

## 3. Launch readiness checklist

"Gates" says which launch an item must be finished before. Effort is an estimate in lane-days.

| # | Item | Owner | Done when | Effort | Gates |
|:--|:--|:--|:--|:--|:--|
| 1 | **Cold first-hour test** on a clean machine | the owner runs it; COORD and R fix | A person who has never seen go2cs goes from the README to a converted program running, following only the README, in under an hour, on Windows and on Linux. Every stumble is written down and either fixed or added to Known issues. The Windows Sandbox kit was handed over on 2026-10-11. | 0.5 day to run; 1 to 4 to fix | soft |
| 2 | **A one-line install** (options below) | COORD; the owner signs | A newcomer installs the converter with one command and `go2cs -version` names the release | 1 to 2 (release binaries) | wide; wanted for soft |
| 3 | **macOS certificate verification** against the system roots | C1 | An `https` request from a converted program succeeds on macOS, Intel and Apple silicon, and the package's row still validates | 3 to 6, unmeasured | wide |
| 4 | **README expectation text** | R drafts; the owner's words | The README says, in two or three lines, that this is one person's project, replies are best-effort, and where to ask | 0.5 | soft |
| 5 | **Issue templates, Discussions and labels** | R drafts the files; COORD or the owner applies the settings | A bug form asks for `go2cs -version`, the OS, the error and the smallest Go that shows it; a pinned "Start here" discussion exists; questions are routed to Discussions; the label set below exists | 1 | soft |
| 6 | **Security policy check** | R | `docs/SECURITY.md` names which releases get fixes, and private vulnerability reporting is confirmed switched on in the repository settings | 0.5 | soft |
| 7 | **A demo** | R records; the owner approves | A recording of under a minute shows convert, build, run on a real module, and is on the README | 1 | wide; wanted for soft |
| 8 | **nugetgo.net ready for visitors** | C2 | The page lists the published modules, shows the one command that adds one to a C# project, says how to propose a module, and says it is unofficial | 1 to 2 | soft |
| 9 | **GitHub front matter** | COORD; the owner | The Releases panel shows the current release with notes taken from NEWS; the About box has a description, the site and topics; the license question is answered where GitHub shows it | 0.5 to 1 | soft |
| 10 | **How the project is built**, said plainly | the owner's words; R drafts | One short paragraph in the FAQ: how AI assistance is used, and that correctness rests on Go's own tests and not on who typed the code | 0.5 | soft |
| 11 | **A code of conduct** | R | `CODE_OF_CONDUCT.md` exists (the Contributor Covenant is the usual choice) | 0.5 | wide |
| 12 | **The Go version** | the hop | The published packages are built from a supported Go release | the hop | wide |
| 13 | **The glyphs** (recommendation below) | G or R to size | A sized proposal exists for ASCII spellings, so the decision after the soft launch is informed | 1 to 2 to size | neither yet |
| 14 | **The reply bank and the posts** | R drafts; the owner edits | [`drafts/`](drafts/) holds a reviewed post for each venue and a short answer for each predictable objection | 2 | soft (blog, replies); wide (the rest) |

**Item 2, the install options:**

| Option | What the user types | Effort | For | Against |
|:--|:--|:--|:--|:--|
| **GitHub release binaries**, one per platform | download, unzip, put on `PATH` (or a three-line script) | 1 to 2 | Go cross-compiles them in one CI job; no new account, feed or signing chain; also fixes the stale Releases panel (item 9) | not a package manager; the user still needs Go installed to convert |
| **`go install <path>@<version>`** | one line, the way Go developers install tools | 1 to 3, plus a full byte-identical check | needs no hosting at all | the converter's module path is plain `go2cs` today, so every import path in it changes; a user's own toolchain builds it, so the version refusal has to hold; unmeasured |
| **A `dotnet tool`** | `dotnet tool install -g go2cs` | 3 to 5 | the natural command for a C# developer; rides the existing NuGet signing | a platform-specific tool package is new packaging work; signing happens on one machine |
| **winget and Homebrew** | `winget install`, `brew install` | 2 to 4 each, plus outside review | what each platform's users expect | review queues the project does not control; they build on the release binaries anyway |

**Recommendation: release binaries first**, attached to the 1.24.13.6 or the next release. They are the
cheapest, they repair the Releases panel, and both the `dotnet tool` and the package managers are built
on top of them. **Then the `dotnet tool` before the wide launch**, because the C# audience is the one that
will not have a Go habit. Whatever the choice, the README must keep saying that Go itself is required to
convert: no install method removes that.

**Item 5, the label set.** GitHub's defaults are there (`bug`, `enhancement`, `question`, `good first
issue`, `help wanted`, `duplicate`, `invalid`, `wontfix`). Add: `converter`, `runtime`, `stdlib`,
`module`, `docs`, `windows`, `linux`, `macos`, `needs-repro`, `known-limitation`.

**Item 13, the glyphs: the recommendation.** Do not start ASCII spellings before the soft launch, and do
not rule them out of the wide launch either. **Size them now and let the soft launch decide.** The soft
launch is the measurement: if the glyphs are among the first three things readers raise, pull the work
ahead of the wide launch; if they are a curiosity, a prepared answer and the glyph table are enough. The
work touches every converted file, so it must not run inside the hop's reconvert stages; it fits before
H4 or after the hop's release.

**Item 3, why the macOS fix is promoted.** The standing ruling keeps macOS in the background, behind the
modules and the train. This plan asks for one exception, for the wide launch only: a first-time user on a
Mac who fetches an `https` URL is on the commonest path there is, and the failure is a thrown exception
in the first minute. Until it is fixed, Known issues says so in one plain entry (part of item 1's fixes).

## 4. Timeline

Ranges, each derived from the estimates above at the current lane count (seven lanes and COORD).
Re-derived 2026-10-12.

```
2026   Oct 12     Oct 19     Oct 26     Nov 2      Nov 16     Nov 30     Dec 14  |  2027  Jan 11    Feb 1    Feb 15
A  release  [done]
P  parity   [== seats ==][train + release]            the release that carries parity: Oct 19 to Oct 30
B  modules  [============ 1 to 3 weeks ====]
D  soft launch          [ window: Oct 20 to Nov 5 ][ listen + fix ]
E  1.26 hop                  [H0][========== 4 to 5.5 weeks from H1 ==========]   publish Nov 19 to Dec 11
H  1.27 hop                                                    [===== 2.5 to 3.5 weeks, across the holidays =====]
G  wide launch   on 1.26: [ Dec 1 to Dec 17, only if E publishes by Dec 4 ]   on 1.27: [ Jan 19 to Feb 18 ]
```

| Milestone | Range | Derived from | What moves it |
|:--|:--|:--|:--|
| **1.24.13.6 on NuGet** | done, Oct 12 | | |
| **The release that carries parity** (the earliest final 1.24.13.N) | **Oct 19 to Oct 30** | Six parity seats were posted in the first day and a half. The classes the owner ruled on Oct 12 are still to build, the largest being Go's blank lines in every conversion, which re-baselines about 650 expected outputs. Then one train (the last took about a day and a half from assembly to landing) and one release | how the remaining seats go; whether the modules' fixes ride it |
| **Soft launch** | **Tue Oct 20 to Thu Nov 5** | Readiness is nearly done (section 8). Oct 20 to 22 on 1.24.13.6, or the first Tue to Thu after the parity release if it is worth the wait | decision D1; the owner's remaining hands (section 8) |
| **Hop start (H1)** | **Oct 22 to Nov 3** | the parity release, plus H0 (3 to 5 lane-days, two to three days of calendar) | the parity release; the hop plan's O1 and O2 (the toolchain installs) |
| **Go 1.26 packages published** | **Nov 19 to Dec 11** | H1 plus 4 to 5.5 weeks, the hop plan's section 6. The last hop took 16.4 days with six lanes and a hand-owned bill a little over half this one's | the hop plan's risks: the hand-owned review (re-sized upward on Oct 12 from a measurement), the runtime compile at 1.26, the TLS host. US Thanksgiving week falls inside this range, and release day needs the owner |
| **Go 1.27 packages published** | **mid-December to late January** | the 1.26 publish plus 2.5 to 3.5 weeks, stretched by the year-end holidays | the generic-methods design; the holidays; release day needs the owner |
| **Wide launch** | **on 1.27: Tue Jan 19 to Thu Feb 18, 2027** (expected). **On 1.26: Dec 1 to Dec 17** (only if the hop publishes by Dec 4) | the publish, then a week of soak, then the first Tue to Thu clear of the dates below | see "Does December still hold?" |

**Does December still hold?** At the optimistic end only. A December wide launch needs the 1.26 packages
published by about **Dec 4**: a week of soak, then the next Tuesday to Thursday, the last being Dec 15 to
17 before the holidays. The publish range is Nov 19 to Dec 11.

- It holds at any pace in the hop's range if the hop starts by about **Oct 26**. That needs the parity
  release by about Oct 23.
- For a start between Oct 27 and Nov 3 it holds only if the hop beats its slow end: from Nov 3 it has to
  finish in about four and a half weeks.
- Before the amendment the publish range was Nov 12 to Dec 14. It now starts a week later, and the hop
  itself is half a week longer at each end.

So December is possible and should not be planned on. **The expected wide launch is on Go 1.27, between
Jan 19 and Feb 18, 2027.** That is later, and it is the stronger launch: the newest Go release, a full
support window ahead, generic methods converted, and no "that Go is nearly out of support" reply to
answer. If the 1.26 hop does publish by Dec 4, a December launch is a choice the owner still has (D7).

**The date to avoid in that window:** Go 1.28 is expected in February 2027. Do not post in the week it
ships. Go 1.27 stays supported after it, so a launch on 1.27 just after 1.28 still stands, but the first
question becomes "when is 1.28", so earlier in the window is better.

**Dates to avoid generally** (confirm each nearer the time): .NET Conf and the .NET 11 release, expected
the second week of November; US Thanksgiving week, Nov 23 to 27; Dec 21 to Jan 8, when traffic is low and
nobody is at a desk to try anything. Tuesday to Thursday, morning US Eastern, is the working default.

**Assumptions.** Seven lanes and COORD stay available. The parity release needs no second train. The
owner is available for release days and for the remaining soft-launch hands. No converter defect from
the soft launch needs its own release. The Go 1.26 target does not move mid-hop. The parity stream's
estimate is this plan's own, read from its pace so far; the parity plan states none.

## 5. Venues

Rules change; read each venue's own rules on the day. Times are US Eastern.

| Venue | Stage | Audience | Angle | Format | Etiquette | Best time |
|:--|:--|:--|:--|:--|:--|:--|
| **go2cs.net blog post** | soft | anyone you send a link to | what it is, what works, what does not, how to try it in ten minutes | a NEWS entry with its own page | the page every other post links to; honest about the edges | publish the day before the first share |
| **The owner's network** | soft | people who know you | "I would value your first hour with this" | a personal message with the blog link | ask for the stumble, not for praise | any weekday |
| **Gophers Slack, `#show-and-tell`** | soft | Go developers | Go's own tests pass in .NET; goroutines as threads and what that costs | two or three lines and the link | one post; answer in the thread; no cross-posting to other channels | Tue to Thu, 10:00 to 12:00 |
| **C# Discord, the showcase channel** | soft | C# developers | a Go library as a NuGet package, called like any other | a short message, a code snippet, the link | read the channel's pinned rules first; one post | Tue to Thu, 11:00 to 14:00 |
| **Hacker News, Show HN** | wide | generalists, skeptics, both languages | the validation method: a transpiler held to the source language's own test suite | "Show HN:" title, a short text, the author's first comment | it must be something people can try; plain title, no superlatives; never ask for votes; be present for two to three hours | Tue to Thu, 8:00 to 10:00 |
| **r/golang** | wide | Go developers | what Go's semantics look like on another runtime, and where they do not fit | a text post, written for that subreddit | read the rules on self-promotion and on AI-assisted projects; say up front how it is built; reply to everything civil | Tue to Thu, 9:00 to 11:00, not the day of the HN post |
| **r/dotnet** and **r/csharp** | wide | .NET developers | Go libraries on NuGet; managed code, no native binary; Native AOT, with its cost | one post in each, each its own text | self-promotion limits apply; lead with the use, not the project | Tue to Thu, 9:00 to 11:00, a day apart |
| **Lobsters** | wide | systems and language people | the semantics mapping; the disclosed differences | a link to the blog post, tagged `go` and `dotnet`, marked as authored | invitation-only; the author flag is required; no drive-by posting | Tue to Thu, morning |
| **Golang Weekly** | wide | Go developers, by email | one line and a link | a submission to the editors | submit once, a week ahead; they choose | their weekly cycle |
| **.NET newsletters** (C# Digest, .NET Weekly, The Morning Brew) | wide | .NET developers, by email | Go modules as NuGet packages | a submission or an email to each editor | submit once | their weekly cycles |

**Order for the wide launch:** the blog post on Monday; Show HN on Tuesday; r/golang on Wednesday;
r/dotnet on Wednesday or Thursday and r/csharp the day after; Lobsters and the newsletters in the same
week. One venue a day keeps the replies answerable by one person.

## 6. Message handling

**Before a post goes up:**

- The README says what to expect: a personal project, best-effort replies, and where to ask (item 4).
- Issues ask for the right facts by template, questions go to Discussions, and a pinned "Start here"
  discussion links the FAQ, Limitations and Known issues (item 5).
- The reply bank is reviewed: [`drafts/replies.md`](drafts/replies.md).

**On the day:**

- The owner posts, and writes the first comment himself on Hacker News: who he is, why he built it, and
  the three things it does not do. Then he stays for two to three hours.
- Every reply is short, grants what is true in the objection, and links the page with the detail.
  **Acknowledge and link; never argue.** A hostile comment gets one civil answer or none.
- A report of a real defect gets thanks, an issue link within the hour, and a label.
- Nobody but the owner posts under the project's name. COORD drafts; the owner sends.

**In the week after:**

- COORD triages once a day: every new issue gets a label and one of three answers (a known limitation,
  with the link; a defect, with a reproduction and a lane; a question, moved to Discussions).
- A defect a newcomer can hit in the first hour is fixed ahead of other work. Everything else queues.
- One summary to the owner each day: what came in, what was answered, what needs his voice.
- At the end of the week, the stumbles are counted by kind. That count decides the glyph question
  (item 13) and feeds the wide launch's posts.

**The predictable objections**, each with a prepared answer in the reply bank: why not cgo or a native
wrapper; performance; the Go version; the license; the generated style and the glyphs; "why would
anyone"; was this written by AI; goroutines as threads; is it production-ready; one maintainer.

## 7. Draft posts

In [`drafts/`](drafts/). Each is written for its audience, leads with something a reader can use in ten
minutes, and names the edges. Figures in square brackets are read from the roster on the day of posting.

| File | For |
|:--|:--|
| [`drafts/blog-soft-launch.md`](drafts/blog-soft-launch.md) | the go2cs.net post, soft launch |
| [`drafts/show-hn.md`](drafts/show-hn.md) | the Show HN title, text and first comment |
| [`drafts/reddit-golang.md`](drafts/reddit-golang.md) | r/golang |
| [`drafts/reddit-dotnet.md`](drafts/reddit-dotnet.md) | r/dotnet, with the r/csharp variant |
| [`drafts/replies.md`](drafts/replies.md) | the reply bank |
| [`drafts/release-1.24.13.6.md`](drafts/release-1.24.13.6.md) | the GitHub Release body for 1.24.13.6 |

## 8. Open decisions for the owner

**Decided or done since the draft (2026-10-11 and 12):**

- **D3, in part:** release binaries for the soft launch are approved. The workflow exists; its first
  release run is being made for 1.24.13.6. The `dotnet tool` for the wide launch is still open.
- **D4:** the macOS certificate fix is promoted ahead of the wide launch.
- **D10:** a GitHub Release for each NuGet release, from 1.24.13.6.
- **Checklist 1:** the cold first-hour test passed on Windows x64, with no product friction found.
  Linux and macOS are still to run.
- **Checklist 5, 6 and 9:** the issue forms, the labels, the About box and topics, the security policy's
  supported-versions statement and private vulnerability reporting are all in place.
- **License detection on GitHub** (the settings draft's item 7): COORD recommends leaving `LICENSE` as it
  is.

**Still the owner's, for the soft launch:**

- **D1. When is the soft launch?** Recommended: yes, on Go 1.24, to a few dozen people. Either Oct 20 to
  22 on 1.24.13.6, or the first Tuesday to Thursday after the parity release (by about Nov 5). The second
  shows better-looking converted code and costs a week or two of early feedback. The plan leans to the
  second if the parity release is on track by Oct 23, and to the first if it is not.
- **D6. How to say how the project is built.** The wording is yours. The plan recommends saying it first,
  in the FAQ and in each post's first comment, in two sentences.
- **D9. The expectation text** for the README and the pinned post: one person's project, best-effort
  replies, where to ask. A placeholder marks its place.
- **Discussions:** add the Announcements category, remove Polls, and pin the "Start here" post. Only you
  can; the text is ready in `docs/phase4/DRAFT-launch-github-settings.md`.
- **D8. Accounts.** Which of your accounts post where, and whether a Lobsters invitation is in hand.

**Still the owner's, for the hop and the wide launch:**

- **D2. The final 1.24.13.N.** The release that carries comment parity is the earliest candidate. If the
  three modules need converter fixes, they ride it or one more. This is the hop plan's O1 and it sets H1.
- **D7. Which Go release the wide launch stands on.** Recommended: plan on **Go 1.27, Jan 19 to Feb 18**.
  Keep **Go 1.26, Dec 1 to 17** as an option to take only if the hop publishes by Dec 4. Not recommended:
  a wide launch on 1.26 in January, weeks before 1.26 leaves support.
- **D3, the rest.** A `dotnet tool` before the wide launch, with a Scoop bucket and a Homebrew tap.
- **D5. Size the ASCII glyph spellings now**, and decide after the soft launch from what readers raise.
- **D11. Sponsorship and commercial licensing in the posts.** Recommended: no mention in any launch post.
- **A code of conduct**, and which one.
- **D13. The hop's own owner hands, O1 to O10**, listed with their reasons in
  [the hop plan, section 2](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-plan-hop-go126/docs/PLAN-hop-go126.md).
  O2 (the Go 1.26 toolchain installs) and O3 (the TLS host and any cloud quota) have lead times and are
  worth starting now.
