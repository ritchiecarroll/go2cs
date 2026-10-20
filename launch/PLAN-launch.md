# PLAN: launching go2cs

> **State: REWRITTEN 2026-10-20 for the owner's review.** This branch (`claude/launch`) is a planning
> branch. It is never merged to master and never published on the site. Every date here is an
> **estimate**, derived as section 4 shows; none is a promise, and no gate is traded for a date.
>
> **Why it was rewritten.** The owner's direction of 2026-10-20: **the Go 1.26 build is ready and
> published before ANY announcement, soft or hard**; the launch eases into the water with a soft stage
> first, then the venues that need more live response effort; and the number of venues comes down,
> because one person answers all of them. The 2026-10-11 plan (soft launch on 1.24.13.7, wide launch on
> Go 1.27 in January or February) is superseded. Its readiness work mostly stands and is carried below.

<!-- Provenance of the superseded plan (kept here, costs nothing on the rendered page):
     - 2026-10-11 draft (68621ab718): two stages, soft after 1.24.13.6, wide after Go 1.26.
     - 2026-10-12 amendment (d70107f5ae): wide launch moved to Go 1.27, Jan 19 to Feb 18, 2027, because
       comment parity moved before the hop and the hop was re-sized to 4 to 5.5 weeks.
     - 2026-10-13 amendment (4c1fdd67bd): 1.24.13.7 ruled the final 1.24 release and the soft-launch
       release; D2 and D9 settled; Discussions set up.
     - 2026-10-20 (this rewrite): owner direction, relayed by COORD: 1.26 published before any
       announcement; soft first, then harder venues; fewer venues. The hop is pinned to go1.26.10
       (ruled 2026-10-20, PLAN-hop-go126.md section 0) and COORD's rough estimate puts the hop's landing
       and the go.* 1.26 publish at Oct 27 to 29.
     Read at the trees on 2026-10-20: origin/master ae0c4c6e43, origin/claude/hop-go126 315a93e847,
     origin/claude/launch da487c7eea. Repository read-only checks the same day: has_discussions true,
     5 discussion categories (Announcements, General, Ideas, Q&A, Show and tell), 1 discussion ("Start
     here"), 3 open issues, 400 stars, GitHub Releases nuget-1.24.13.6 and nuget-1.24.13.7 published
     with binaries (the 2022 v0.1.2 pre-release is still listed below them), community profile
     71% with no code of conduct. -->

Contents: [1. The recommendation](#1-the-recommendation) · [2. Where things stand](#2-where-things-stand) ·
[3. Gates before the first post](#3-gates-before-the-first-post) · [4. Timeline](#4-timeline) ·
[5. Venues: kept and dropped](#5-venues-kept-and-dropped) · [6. The owner's hours](#6-the-owners-hours) ·
[7. Message handling](#7-message-handling) · [8. Drafts](#8-drafts) ·
[9. Decisions for the owner](#9-decisions-for-the-owner)

## 1. The recommendation

**Three steps, each gated on the one before, all on Go 1.26.** Day 0 is the day the go.* 1.26 packages
are published on NuGet and the post-publish smoke is green. It is estimated at **Tue Oct 27 to Thu
Oct 29, 2026**.

1. **Get ready, announce nothing** (now to about day +6). The 1.26 release, its GitHub Release, the
   nugetgo rebuild wave (every module republished on 1.26, with mousetrap and cobra added), a cold
   first-hour test on 1.26, a docs pre-flight, and the two agent designs in this folder running dry.
   The GitHub issues agent starts **now**, at tier 0, on today's low volume.
2. **Soft stage** (about day +7; estimated **Wed Nov 4 or Thu Nov 5**). Four quiet venues: the
   go2cs.net news post, a GitHub Discussions announcement, personal messages to a dozen or two people the
   owner knows, and one post in the Gophers Slack `#show-and-tell`. No public aggregator yet. Then ten
   days of listening and fixing what a newcomer trips on.
3. **Wide stage**, one venue a day and never two days running:
   - **Show HN** about day +20 (estimated **Tue Nov 17**, 08:30 US Eastern);
   - **r/dotnet** two days later (estimated **Thu Nov 19**, 09:00 US Eastern);
   - **two newsletter submissions** (Golang Weekly and one .NET newsletter) the Monday of that week,
     which cost no live replies;
   - **r/golang, optional**, after US Thanksgiving (estimated **Tue Dec 1**), only if the owner still
     has the hours. It is the most demanding audience on the list.

**Seven venues in all (eight with the optional r/golang), down from eleven.** Dropped: the C# Discord,
r/csharp, Lobsters and the extra newsletters (section 5 says why). Watcher agents read the live threads
and draft replies into Slack; **the owner posts every public word**
([`AGENTS-venue-watch.md`](AGENTS-venue-watch.md)). An issues agent drafts GitHub answers the same way,
starting now ([`AGENTS-issues-discussions.md`](AGENTS-issues-discussions.md)).

**What moves the dates:** the hop's landing and publish (day 0), the nugetgo wave (a second owner PIN
session), and anything the soft stage finds that a newcomer hits in the first hour. Each slips the
step after it; none is traded for a date. If the wide stage cannot start by about Dec 3, it waits for
**Jan 12 to 14, 2027** (section 4).

## 2. Where things stand

Read 2026-10-20; linked rather than restated, so the counts stay where they are maintained.

- **The release on NuGet is 1.24.13.7** (2026-10-14), the final Go 1.24 release, with a GitHub Release
  carrying converter binaries for six platforms.
- **The Go 1.26 hop** is on `claude/hop-go126`, pinned to **go1.26.10**, through its correctness and
  visual-parity seats and toward its re-bank. Go 1.27 is the current Go release; 1.26 stays in upstream
  support until Go 1.28 ships (about February 2027). The 1.27 hop follows the 1.26 release and the
  nugetgo wave ([hop plan](https://github.com/ritchiecarroll/go2cs/blob/claude/hop-go126/docs/PLAN-hop-go126.md)).
- **Validation.** Every implementable standard-library package validates against Go's own tests on
  Windows and Linux at 1.24.13, and macOS validation is under way row by row
  ([roster](../docs/ValidatedTestPackages.md)). The hop re-validates every row at 1.26.10; the roster's
  population grows at 1.26, so **its figures are read on the day**, never quoted from this plan.
- **Converted modules.** `hashset`, `google/uuid` and `golang-jwt/jwt` v5 are published through
  [nugetgo.net](https://nugetgo.net); `x/sync` and `pflag` are validated and ride the post-hop wave
  with mousetrap and cobra ([Validated Modules](../docs/ValidatedModules.md)).
- **Already done for a launch** (from the superseded plan): the README's expectation line; issue forms
  (conversion bug, behavior difference, nugetgo package) with questions routed to Discussions; the label
  set; Discussions with a pinned "Start here"; the security policy's supported-versions statement and
  private vulnerability reporting; the About box and topics; GitHub Releases per NuGet release with
  binaries; the cold first-hour test on Windows x64 (at 1.24.13.6); the macOS HTTPS fix (1.24.13.7).

**What the 1.26 release changes for a newcomer** (confirm each against the 1.26 NEWS entry before any
post quotes it):

- **The Go version objection goes away.** The converted standard library is on a Go release Go still
  patches.
- **Plain string literals.** In a user's own conversion a Go string literal is written as an ordinary C#
  string literal where it is used. The standard library keeps hoisted literals, and `-hoist-literals`
  restores them for performance-critical code.
- **Slices grow as Go's do,** including the stack-append buffer Go's escape analysis gives a local slice
  (a visible `xᴷ` buffer in the C#).
- **Code spelled more like the Go**: shifts, casts and parentheses as Go writes them, on top of
  1.24.13.7's comments and spacing.
- **Converted tests print Go's output** (`=== RUN`, `--- PASS`, the caller's `file:line`).
- **A program's first I/O deadline fires on time**, as Go's does, and **Native AOT** prints and reflects
  as Go does in cases that differed.

**What a newcomer will still hold against it**, in the order they meet it:

1. **Install.** Download a release binary, or clone and `go install`. Go itself is still needed to
   convert. There is no `dotnet tool` or package-manager install.
2. **Speed and size.** Usually slower than Go, slower to start, more memory; a Native AOT publish is slow
   and large. The published measurements predate the hop (section 3, item 4).
3. **The glyphs.** `ж<T>`, `ᴷ` and friends. The answer is the glyph table; ASCII spellings stay a
   rainy-day item.
4. **macOS.** Validation is in progress; Windows and Linux x64 are the finished platforms.
5. **How it is built.** The commit history shows AI assistance on most commits. Said first, in the
   owner's words (D6), it lands better than when it is discovered.
6. **Licensing.** Answered factually when asked, never raised in a post (section 7).

## 3. Gates before the first post

Nothing is announced until every **soft** row is done. "Who" uses the fleet's role names.

| # | Item | Who | Done when | Gates |
|:--|:--|:--|:--|:--|
| 1 | **go.* 1.26 published** | COORD; the owner (PIN) | the packages are on NuGet, the post-publish smoke is green on four RIDs, the GitHub Release carries the binaries and notes from the 1.26 NEWS entry | soft |
| 2 | **The nugetgo wave** | C2 prepares; the owner signs | every module rebuilt and republished on go.* 1.26 (hashset, uuid, jwt, x/sync, pflag) plus mousetrap and cobra, with the new README "About" paragraphs; nugetgo.net lists them | soft (the posts' first example is `uuid`) |
| 3 | **Docs pre-flight at the release tree** | R | no visitor page says "Go 1.24" or "out of upstream support" for the current release (on 2026-10-20 the hop branch's README Requirements still carried the 1.24 out-of-support sentence beside the 1.26.10 line, and the FAQ's version answer still says out of support); FAQ, Limitations, Roadmap "Go releases", Known issues and SECURITY read true at 1.26; every link the drafts use resolves | soft |
| 4 | **Performance page re-measured** | R or G | the table in [Performance](../docs/Performance.md) is measured at the 1.26 release, not at go1.23.1 (its table is dated 2026-08-25); the AOT honesty notes stand | wide (wanted for soft) |
| 5 | **Cold first-hour test on 1.26** | the owner runs it; COORD fixes | from the README to a converted program running, on Windows and on Linux, in under an hour, using a release binary; every stumble fixed or written into Known issues | soft |
| 6 | **The issues agent at tier 0 for a week** | COORD sets up; the owner approves drafts | [`AGENTS-issues-discussions.md`](AGENTS-issues-discussions.md) running, with at least a week of drafts read and the kinks written down | soft |
| 7 | **Watchers dry-run** | COORD | each watcher run once against an old public thread, posting into its Slack channel; the kill switch tested | wide |
| 8 | **The reply bank reviewed** | the owner | [`drafts/replies.md`](drafts/replies.md) read and edited in their voice, including D6 | soft |
| 9 | **Old pre-release** | the owner | the 2022 `v0.1.2` pre-release is deleted or marked clearly as historical, so the Releases list shows only current releases | soft |
| 10 | **A code of conduct** | R | `CODE_OF_CONDUCT.md` (the Contributor Covenant) exists; GitHub's community profile stops flagging it | wide |
| 11 | **A demo** | R | a recording of under a minute, convert, build, run, on the README | nice to have; not a gate |

Dropped from the old checklist: the `dotnet tool` before the wide launch (D3) is not a gate now; release
binaries are the install path, and the posts say so. Sizing ASCII glyph spellings (D5 of the old plan)
stays rainy-day.

## 4. Timeline

Day 0 = go.* 1.26 on NuGet with its smoke green. Calendar dates assume day 0 is **Wed Oct 28**, the
middle of COORD's Oct 27 to 29 estimate; a day's slip in day 0 moves everything after it by a day, up to
the hard edges listed below.

| Day | Estimated date | What | Owner time |
|:--|:--|:--|:--|
| now to −1 | Oct 20 to 27 | Issues agent tier 0 running; replies reviewed; Slack channels created; watcher dry-runs; drafts filled at the hop's NEWS entry | 3 to 4 h in all |
| 0 | **Wed Oct 28** (Oct 27 to 29) | go.* 1.26 published; GitHub Release; post-publish smoke | release sitting |
| +1 to +6 | Oct 29 to Nov 3 | nugetgo wave (one PIN session); docs pre-flight; cold first-hour test on Windows and Linux; fixes | PIN session; 2 h test |
| **+7** | **Wed Nov 4** (or Thu Nov 5) | **Soft stage.** Morning: news post live, Discussions announcement. Midday: personal messages | 2 h |
| +8 | Thu Nov 5 (or Fri Nov 6) | Gophers Slack `#show-and-tell`, 10:00 US Eastern | 1 h over two days |
| +7 to +16 | Nov 4 to 13 | Listen and fix. A first-hour defect is fixed ahead of other work, in a patch release if it must be | 30 min a day |
| +16 | **Fri Nov 13** | **Wide go or no-go** (D1): no open first-hour defect, the issues agent's drafts are good, the watchers have passed their dry run | 30 min |
| +19 | Mon Nov 16 | Newsletter submissions (Golang Weekly, one .NET newsletter); the news post updated with anything the soft stage taught | 30 min |
| **+20** | **Tue Nov 17**, 08:30 ET | **Show HN.** The owner present 08:30 to 12:30 ET, then checks at 15:00 and 20:00 ET | 4 to 6 h day one; 1 to 2 h day two |
| **+22** | **Thu Nov 19**, 09:00 ET | **r/dotnet** | 2 to 3 h day one; 1 h day two |
| +26 to +30 | Nov 23 to 27 | US Thanksgiving week: no new venue; watchers taper | |
| **+34** | **Tue Dec 1**, 09:00 ET | **r/golang, optional** (D5) | 3 to 4 h day one; 1 to 2 h day two |

**Hard edges in the calendar** (confirm each nearer the time):

- **Tue Nov 3 is US election day.** Keep the soft stage off it; news and attention are elsewhere.
- **.NET Conf and the .NET 11 release are expected in the second week of November** (about Nov 10 to 12).
  The .NET audience is busy and r/dotnet is full of release posts; no wide venue that week. The soft
  stage's listening period sits over it on purpose.
- **US Thanksgiving week, Nov 23 to 27.** No new venue; it is a dead week for both audiences.
- **Dec 18 to Jan 5.** Low traffic and nobody at a desk to try anything.

**If the soft stage finds a release-blocking stumble**, the wide stage moves two weeks, past
Thanksgiving: Show HN Tue Dec 1, r/dotnet Thu Dec 3, r/golang Tue Dec 8 or dropped. **If it cannot start
by about Dec 3, it waits for Jan 12 to 14, 2027.** That window has one wrinkle: Go 1.28 is expected in
February 2027 and ends 1.26's upstream support, and the 1.27 hop may be close by then. A January launch
says so plainly; it does not wait for 1.27, because the policy is to hop every release, so there is
always a next hop.

**Time of day.** 08:00 to 10:00 US Eastern on Tuesday to Thursday catches the US morning and the EU
afternoon (13:00 to 15:00 UK, 14:00 to 16:00 CET), which is when both HN's and Reddit's weekday
audiences are large. Earlier favours Europe; later loses it. Pick the time the owner can then stay at a
desk for three hours, which matters more than the minute.

**Assumptions.** The hop publishes in the Oct 27 to 29 range (COORD's rough estimate). The nugetgo wave
needs no converter fix. The owner has the release sitting, one PIN session, and the hours in section 6.
The soft stage finds nothing that needs more than a patch release.

## 5. Venues: kept and dropped

Rules change; read each venue's own rules on the day, and follow them over this table. Response load is
an estimate of the comments to read and the replies worth writing, not a forecast of success.

**Kept**

| Venue | Stage | Why kept | Expected response load | Best timing | Watched by |
|:--|:--|:--|:--|:--|:--|
| **go2cs.net news post** | soft | the page every other post links to; costs nothing to keep current | none on the page itself (no comments); traffic lands on GitHub | live the morning of the soft stage, updated before the wide stage | the issues agent (what arrives on GitHub) |
| **GitHub Discussions, Announcements** | soft | the repository's own channel; reaches the people who starred or watch it, who are the friendliest first readers | low: a handful of replies over a week | the soft-stage morning, beside the news post | the issues agent |
| **The owner's network** | soft | the best first-hour feedback there is: people who will say what confused them | low and spread out; one-to-one replies | the soft-stage day, midday US Eastern | the owner's own inbox |
| **Gophers Slack `#show-and-tell`** | soft | Go developers, a thread-contained post, and Slack's own notifications reach the owner | low: 0 to 15 thread replies over two days | Wed or Thu, 10:00 to 12:00 US Eastern (EU afternoon) | the owner's Slack notifications; no agent (reading another workspace would need an app its admins approve) |
| **Show HN** | wide | the largest single audience for a technical project, and both languages are there; the validation method is HN's kind of story | the highest and the most concentrated: if it reaches the front page, 50 to 200 comments, most in the first six hours; if it does not, under 10 | Tue to Thu, 08:00 to 10:00 US Eastern | `#launch-hn` watcher |
| **r/dotnet** | wide | the audience that can use go2cs today (a Go library as a NuGet package); the post leads with working code | medium-high: 20 to 80 comments over two days | Tue to Thu, 08:00 to 10:00 US Eastern, at least a day after HN | `#launch-r-dotnet` watcher |
| **Newsletters** (Golang Weekly; one .NET newsletter) | wide | reach without a thread to answer; the editors decide | none live; a trickle of GitHub traffic for a week | submit on the Monday of the wide week | `#launch-mentions` watcher (for any thread a listing starts) |

**Optional**

| Venue | Stage | Why optional | Expected response load | Best timing | Watched by |
|:--|:--|:--|:--|:--|:--|
| **r/golang** | wide, last | Go developers are a secondary audience (go2cs gives them nothing Go does not already do better), the subreddit is wary of AI-assisted projects, and the thread needs the most careful replies on the list. Worth it for the semantic-fidelity story only if the owner has the hours | high and the most contentious: 20 to 100 comments, more of them hostile | Tue or Wed, 09:00 US Eastern, after Thanksgiving | `#launch-r-golang` watcher |

**Dropped**

| Venue | Why dropped |
|:--|:--|
| **C# Discord showcase** | chat expects replies in minutes, not hours; its audience overlaps r/dotnet; Discord's terms forbid automated reading, so no watcher could help |
| **r/csharp** | a second .NET subreddit in the same week doubles the watching for an overlapping audience, and two self-promotional posts close together read as spam; r/dotnet carries the use case. The draft is kept in [`drafts/retired/`](drafts/retired/) |
| **Lobsters** | needs an invitation in hand (never settled); a small audience that overlaps HN; a third aggregator thread with a demanding readership. If someone else posts it there, the mentions watcher sees it and the owner can choose to answer |
| **Social accounts** (Bluesky, Mastodon, X, LinkedIn) | never in the plan; each is another stream to watch. Anything the owner posts there is their own choice, outside the watched set |
| **More than two newsletters** (the old plan named C# Digest, .NET Weekly and The Morning Brew) | each extra one is low yield; two is enough to learn whether the channel works |

## 6. The owner's hours

The whole launch, from now to the end of the wide stage, is estimated at **25 to 35 hours of the owner's
time across six weeks**, with the peaks on the HN and r/dotnet days. The rest of the work is drafting,
watching and triage, which the agents and the lanes carry.

| Week | What the owner does | Estimate |
|:--|:--|:--|
| Oct 20 to 27 | review the replies and posts; approve the issues agent's drafts in Slack; create the Slack channels | 3 to 4 h |
| Oct 28 to Nov 3 | release sitting; PIN session for the nugetgo wave; cold first-hour test | 4 to 6 h |
| Nov 4 to 13 (soft) | post four soft venues; answer the network and the Slack thread; approve issue drafts | 5 to 7 h |
| Nov 16 to 20 (wide) | newsletters; Show HN; r/dotnet; approve drafts | 9 to 13 h |
| Nov 30 to Dec 4 (optional) | r/golang | 4 to 6 h |

**The bandwidth rules:** one new venue a day at most, never two days running in the wide stage; a venue
day is a day at a desk; when the owner is away, nothing new goes up.

## 7. Message handling

**The rule that does not bend: nobody but the owner posts in public.** Agents read, draft and flag. They
never post, vote, react, or reply on any venue, and on GitHub they act only as the issues design's tiers
allow, each tier being the owner's explicit grant.

**Before the first post:** the expectation line in the README (done); issue forms and Discussions routing
(done); the reply bank reviewed (gate 8); the watchers and the issues agent dry-run (gates 6 and 7).

**On a venue day:**

- The owner posts and writes the first comment on HN: who they are, why they built it, and the three
  things it does not do. Then they stay three to four hours.
- The watcher posts each new comment into the venue's Slack channel with a suggested reply, a confidence
  and a needs-the-owner flag. The owner edits and posts, or skips.
- Every reply is short, grants what is true in the objection, and links the page with the detail.
  **Acknowledge and link; never argue.** A hostile comment gets one civil answer or none.
- A real defect gets thanks, an issue link within the hour, and a label.

**In the days after:** the issues agent triages GitHub; one daily digest in Slack says what came in, what
was answered, and what needs the owner. At the end of the soft stage, the stumbles are counted by kind;
that count shapes the wide posts and decides whether the glyphs need more than the table.

**Licensing, as a rule of tone:** no post mentions licensing, the license history or commercial terms.
When someone asks, the answer is short, factual and linked ([`drafts/replies.md`](drafts/replies.md)), and
the thread moves on.

**The predictable objections,** each with a prepared answer in the reply bank: why not cgo or a native
library; why not just use Go; performance and Native AOT; how the tests are validated; the Go version
and the cadence; maintenance burden and one maintainer; the license; the generated style and the glyphs;
was it written by AI; goroutines as threads; production-ready; macOS.

## 8. Drafts

In [`drafts/`](drafts/). Figures in [square brackets] are read on the day of posting, from the roster or
the release.

| File | For | Stage |
|:--|:--|:--|
| [`drafts/blog-soft-launch.md`](drafts/blog-soft-launch.md) | the go2cs.net news post | soft |
| [`drafts/short-posts.md`](drafts/short-posts.md) | the Discussions announcement, the network message, Gophers Slack, the newsletter blurbs | soft and wide |
| [`drafts/show-hn.md`](drafts/show-hn.md) | Show HN: title, URL and the author's first comment | wide |
| [`drafts/reddit-dotnet.md`](drafts/reddit-dotnet.md) | r/dotnet | wide |
| [`drafts/reddit-golang.md`](drafts/reddit-golang.md) | r/golang | wide, optional |
| [`drafts/replies.md`](drafts/replies.md) | the reply bank, which the watchers draft from | all |
| [`drafts/release-1.24.13.6.md`](drafts/release-1.24.13.6.md), [`drafts/release-1.24.13.7.md`](drafts/release-1.24.13.7.md) | GitHub Release bodies, published; kept as the pattern for the 1.26 release body, which R writes from the 1.26 NEWS entry | records |
| [`drafts/retired/`](drafts/retired/) | drafts for dropped venues (r/csharp) | none |

## 9. Decisions for the owner

**For the soft stage:**

- **D1. Go or no-go, twice.** The soft stage, on the first Wed or Thu at least a week after day 0 with
  gates 1, 2, 3, 5, 6, 8 and 9 done. The wide stage, on Fri Nov 13 (or the Friday before a moved date).
- **D6. How to say how the project is built.** Two sentences in the owner's words, in the FAQ and in each
  post. Every draft has the placeholder.
- **D8. Accounts.** Which of the owner's accounts posts on HN and Reddit. Both sites filter new or
  little-used accounts, and a brand-new Reddit account is likely to be held by r/dotnet's filters.
- **D12. The issues agent's tier 1** (labels, asking for a repro, linking a doc, closing exact duplicates):
  granted after the owner has read a week of tier-0 drafts, or not.
- **D13. The watchers' model and budget.** Sonnet 5.5 for watchers is recommended
  ([`AGENTS-venue-watch.md`](AGENTS-venue-watch.md), cost section).

**For the wide stage:**

- **D5. r/golang or not.** Decided on Nov 30 from the hours the first two venues actually took.
- **D14. A January fallback**, if the wide stage cannot start by Dec 3: Jan 12 to 14 on 1.26, or later
  on whatever release is current.
- **D11 (carried). Sponsorship and commercial licensing in the posts:** not mentioned. Recommended as
  before.
