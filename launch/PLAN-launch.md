# PLAN: launching go2cs

> **State: DRAFT for the owner's review, 2026-10-11.** This branch (`claude/launch`) is a planning branch.
> It is never merged to master and never published on the site. Facts are stated as of the draft date and
> each links the page that carries the current figure. Every effort figure and every date range here is an
> **estimate**, derived as section 4 shows; none is a promise, and no gate is traded for a date.

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
- **The release on NuGet is 1.24.13.5** (2026-10-09). 1.24.13.6 follows the train now landing, this week.
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
| A | **The 1.24.13.6 release** | the train landing; one hosted check | COORD, the owner (the PIN) | this week |
| B | **Three harder modules** as NuGet packages, rising in complexity | A | C2; the owner (sign, push) | 1 to 3 weeks, unmeasured until C2's proposal names them |
| C | **Soft-launch readiness** (section 3, the items that gate the soft launch) | A | R (docs, templates, drafts), COORD (repository settings), the owner (the cold test) | 5 to 9 lane-days |
| D | **The soft launch**, then a week of listening and fixing | C | the owner posts; COORD triages daily | 1 week |
| E | **The Go 1.26 hop**, H0 to release, by the runbook | the final 1.24.13.N shipped (the hop plan's O1) | seven lanes and COORD, as the [hop plan](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-plan-hop-go126/docs/PLAN-hop-go126.md) section 7 proposes | 85 to 145 lane-days; 3.5 to 5 calendar weeks from H1 |
| F | **Wide-launch readiness** (section 3, the items that gate the wide launch) | D's findings; parts need E | C1 (macOS), R, COORD | 8 to 16 lane-days |
| G | **The wide launch** | E published, F done, a week of soak | the owner posts; COORD drafts replies | 1 week of presence |
| H | **The Go 1.27 hop** (generic methods, a design-sized feature) | E; its design drafted during E's H10 | to be planned | 60 to 100 lane-days, 11 to 21 of them language work; landing about December 2026 to January 2027 if it follows E's close directly ([measured 2026-10-11](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-go125-recon/docs/phase4/RECON-go125-hop.md), section X.7; an estimate) |

**What runs in parallel, and what collides:**

- **B and C run alongside each other** from the release. B is C2's; C is R's and COORD's.
- **E's readiness stage can start during the wait for the final 1.24.13.N**, as the hop plan says.
- **B decides when E starts.** If the three modules need converter fixes, the owner chooses between one
  more 1.24 release (E starts about a week later) and carrying the fixes into the hop (decision D2).
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

```
2026   Oct 12        Oct 19        Oct 26        Nov 2 ... Nov 30   Dec 7   Dec 14  |  Jan 11 ... Jan 28
A  release   [=====]
B  modules           [================== 1 to 3 weeks =====]
C  soft readiness    [=========]
D  soft launch                 [ window: Oct 20 to Oct 29 ][ listen + fix ]
E  1.26 hop          [ readiness ][======== 3.5 to 5 weeks from H1 ========] publish Nov 12 to Dec 14
F  wide readiness                        [=========================]
G  wide launch                                              [ Dec 1 to Dec 17 ] | or [ Jan 12 to Jan 28 ]
```

| Milestone | Range | Derived from | What moves it |
|:--|:--|:--|:--|
| **1.24.13.6 on NuGet** | the week of Oct 12 | ruled; one hosted check outstanding | that check |
| **Soft launch** | **Tue Oct 20 to Thu Oct 29** (Oct 20 to 22, or Oct 27 to 29) | release, plus 5 to 9 lane-days of readiness across R and COORD, plus the owner's cold test | the cold test. A stumble that needs a release moves it to Nov 3 to 5 |
| **Hop start (H1)** | Oct 19 to Nov 9 | H1 waits for the final 1.24.13.N. Oct 19 if 1.24.13.6 is final; up to Nov 9 if the modules need a 1.24.13.7 | decision D2 |
| **Go 1.26 packages published** | **Nov 12 to Dec 14** | H1 plus 3.5 to 5 weeks, the hop plan's section 6. The last hop took 16.4 days with six lanes and a smaller hand-owned bill | the runtime compile at 1.26, the 79 hand-owned files, the TLS host (the hop plan's risks 1 to 3) |
| **Wide launch** | **Tue Dec 1 to Thu Dec 17**, or **Tue Jan 12 to Thu Jan 28, 2027** | 1.26 published, then a week of soak, then the first Tue to Thu clear of the dates below | the hop's publish date. Published by Nov 20: Dec 1 to 3 or Dec 8 to 10. Published by Dec 4: Dec 15 to 17. Later: January |

**Dates to avoid** (confirm each nearer the time): .NET Conf and the .NET 11 release, expected the second
week of November; US Thanksgiving week, Nov 23 to 27; Dec 21 to Jan 8, when traffic is low and nobody is
at a desk to try anything. Tuesday to Thursday, morning US Eastern, is the working default.

**The edge on the far side.** Go 1.26 is supported until Go 1.28 ships, expected in February 2027. A wide
launch in late January is a launch on a release with weeks of support left, and the first objection comes
back. So the wide launch has a real last date on 1.26, about the end of January. If the hop's publish
slips past mid-December, the choice is January on 1.26 or waiting for the 1.27 hop (decision D7).

**The 1.27 hop changes that choice.** It is measured at 60 to 100 lane-days and could land between
December 2026 and January 2027 if it starts as the 1.26 hop closes (an estimate). By the 1.26 hop's own
ratio of lane-days to calendar time, that is roughly two and a half to three and a half weeks. So there are two honest shapes for the wide launch:
**on 1.26 in December**, the earliest date, with the 1.27 release following as the launch's first piece
of news; or **on 1.27 in late January or February**, later, but on the newest Go release with a full
support window ahead and generic methods converted. A launch on 1.26 in late January is the weakest
of the three and is the one to avoid.

**Assumptions.** Seven lanes and COORD stay available. The owner is available for the cold test in the
week of Oct 12 and for the PIN on release days. No converter defect from the soft launch needs its own
release. The Go 1.26 target does not move mid-hop.

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

- **D1. Is the soft launch on?** This plan recommends yes, on Go 1.24, in the window Oct 20 to 29, to a
  few dozen people, provided the cold first-hour test passes. Waiting for 1.26 costs nothing but the
  early feedback, and the feedback is the point.
- **D2. The final 1.24.13.N.** If the three modules need converter fixes: one more 1.24 release, or carry
  the fixes into the hop. This is the hop plan's O1, and it sets the hop's start.
- **D3. The install method.** Recommended: release binaries now, a `dotnet tool` before the wide launch.
- **D4. Promote the macOS certificate fix** ahead of the wide launch, as the one exception to "macOS stays
  in the background".
- **D5. Size the ASCII glyph spellings now**, and decide after the soft launch from what readers raise.
- **D6. How to say how the project is built.** The wording is yours. The plan recommends saying it first,
  in the FAQ and in each post's first comment, in two sentences.
- **D7. Which Go release the wide launch stands on.** On 1.26 in December (Dec 1 to 17), if the hop
  publishes by about Dec 4; otherwise on 1.27, in late January or February, once that hop lands. The
  plan recommends against a wide launch on 1.26 in late January, weeks before 1.26 leaves support.
- **D8. Accounts.** Hacker News and Reddit favor accounts with history, and Lobsters needs an invitation.
  Which of your accounts post, and is a Lobsters invitation in hand?
- **D9. The expectation text.** How quick a reply do you want to promise? The draft says best-effort,
  with no time named.
- **D10. GitHub Releases.** Publish a GitHub Release for each NuGet release from now on, so the Releases
  panel stops showing 2022.
- **D11. Sponsorship and commercial licensing in the posts.** Recommended: no mention in any launch post.
  The README's License section already says it for those who look.
- **D12. A code of conduct**, and which one.
- **D13. The hop's own owner hands, O1 to O10**: toolchains, the TLS host, the NuGet prefix reservations,
  the demotion cap and release day. They are listed, with their reasons, in
  [the hop plan, section 2](https://github.com/ritchiecarroll/go2cs/blob/claude/i9-plan-hop-go126/docs/PLAN-hop-go126.md).
  O3 (the TLS host and any cloud quota) has a lead time outside the project's control and is worth
  starting now.
