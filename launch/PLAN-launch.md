# PLAN: launching go2cs

> **State: REFRESHED 2026-10-27, present tense, for the owner's review.** This branch (`claude/launch`)
> is a planning branch. It is never merged to master and never published on the site. Every date here
> is an **estimate**, derived as section 4 shows; none is a promise, and no gate is traded for a date.
>
> **The shape is the owner's direction of 2026-10-20:** a Go 1.26 build published before any
> announcement, soft or hard; a soft stage first, then the venues that need more live response effort;
> and few venues, because one person answers all of them. **What changed on 2026-10-27:** go.* 1.26.10.1
> is published and the nugetgo wave is done, so the first two gates are met. Day 0 is now **go.* 1.26.10.2**
> (the owner's ruling, D15), the release that ships the `go2cs` dotnet tool, because the tool
> removes a newcomer's first obstacle (install) and the README leads with it.

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
     71% with no code of conduct.
     - 2026-10-27 refresh (COORD, owner's ask "a present tense update given 1.26.10.2 pending"). Read
       the same day: origin/master 39f85c0e15 (batch 58); GitHub Releases: nuget-1.26.10.1 "Latest"
       (2026-10-26), nuget-1.24.13.7 and nuget-1.24.13.6 below it, the 2022 v0.1.2 pre-release still
       listed; 401 stars, 2 open issues, Discussions on, community profile 71%, no CODE_OF_CONDUCT.md
       at master. RD6 post-publish smoke green (release-smoke 40378300676, 2026-10-26). nugetgo wave
       published 2026-10-26/27 (S1: hashset, uuid, jwt v5, x/sync, pflag, mousetrap; S2: cobra; the
       registry rows merged; the section 6 demo ran 7 of 7 through the live registry). The 1.24 IDs
       carry deprecations pointing at 1.26. The tier-0 triage task's last completed run was
       2026-10-22; one run has been stuck "running" since then and holds the schedule.
       1.26.10.2: 11 of 13 seats on master (docs/PLAN-release-1-26-10-2.md); seats #9 and #10, the
       full re-bank and release day remain. The dotnet tool (seat #11) and signed binaries (seat #12)
       are among the landed seats; the README's tool-first docs commit is held for release day. -->


Contents: [1. The recommendation](#1-the-recommendation) · [2. Where things stand](#2-where-things-stand) ·
[3. Gates before the first post](#3-gates-before-the-first-post) · [4. Timeline](#4-timeline) ·
[5. Venues: kept and dropped](#5-venues-kept-and-dropped) · [6. The owner's hours](#6-the-owners-hours) ·
[7. Message handling](#7-message-handling) · [8. Drafts](#8-drafts) ·
[9. Decisions for the owner](#9-decisions-for-the-owner)

## 1. The recommendation

**Three steps, each gated on the one before, all on Go 1.26.** Day 0 is the day **go.* 1.26.10.2** is
published on NuGet with its post-publish smoke green. It is estimated at **Wed Nov 4 to Fri Nov 6,
2026** (section 4).

1. **Get ready, announce nothing** (now to about day +4). The 1.26.10.2 release, its GitHub Release, a
   cold first-hour test that installs with `dotnet tool install -g go2cs`, a docs pre-flight at the
   release tree, the performance page re-measured at 1.26, and the two agent designs in this folder
   running dry.
2. **Soft stage** (about day +5; estimated **Tue Nov 10**). Four quiet venues: the go2cs.net news post,
   a GitHub Discussions announcement, personal messages to a dozen or two people the owner knows, and one
   post in the Gophers Slack `#show-and-tell` the next day. No public aggregator yet. Then about ten days
   of listening and fixing what a newcomer trips on.
3. **Wide stage**, one venue a day and never two days running, after US Thanksgiving:
   - **two newsletter submissions** (Golang Weekly and one .NET newsletter) on **Mon Nov 30**, which cost
     no live replies;
   - **Show HN** on **Tue Dec 1**, 08:30 US Eastern;
   - **r/dotnet** two days later, **Thu Dec 3**, 09:00 US Eastern;
   - **r/golang, optional**, **Tue Dec 8**, only if the owner still has the hours. It is the most
     demanding audience on the list.

**Seven venues in all (eight with the optional r/golang).** Dropped: the C# Discord, r/csharp, Lobsters
and the extra newsletters (section 5 says why). Watcher agents read the live threads and draft replies
into Slack; **the owner posts every public word himself**
([`AGENTS-venue-watch.md`](AGENTS-venue-watch.md)). An issues agent drafts GitHub answers the same way
([`AGENTS-issues-discussions.md`](AGENTS-issues-discussions.md)).

**Why day 0 is 1.26.10.2 and not the published 1.26.10.1 (D15).** 1.26.10.2 ships the `go2cs` dotnet
tool: a .NET developer installs the converter with one command and converts with no Go installed (the
tool fetches a verified Go on first use). The README leads with it, so the first thing a newcomer reads
is the easiest path. 1.26.10.2 also changes compiled C# (an exported package-level variable becomes a
property that aliases the variable, as Go's address-of does), so announcing on 1.26.10.1 would send the
first readers to a shape that changes within days. The cost of waiting is about a week, and it moves the
wide stage past Thanksgiving, which the calendar edges in section 4 would have pushed it toward anyway.

**What moves the dates:** the 1.26.10.2 release (its last two seats, the full re-bank, the release
sitting), and anything the soft stage finds that a newcomer hits in the first hour. Each slips the step
after it; none is traded for a date. If the wide stage cannot start by about Dec 8, it waits for
**Jan 12 to 14, 2027** (section 4).

## 2. Where things stand

Read 2026-10-27; linked rather than restated, so the counts stay where they are maintained.

- **The release on NuGet is go.* 1.26.10.1** (2026-10-26), the first Go 1.26 release, pinned to
  **go1.26.10**. Its GitHub Release is published with converter binaries for six platforms, and its
  post-publish smoke is green on the four shipped RIDs. The 1.24 packages carry deprecations that point
  to it. Go 1.27 is the current Go release; 1.26 stays in upstream support until Go 1.28 ships (about
  February 2027), and the 1.27 hop follows.
- **go.* 1.26.10.2 is in progress** ([release plan](../docs/PLAN-release-1-26-10-2.md)): 11 of its 13
  seats are on master, among them the `go2cs` dotnet tool, Authenticode-signed Windows binaries with a
  GPG-signed `SHA256SUMS`, the cross-package address-of fix, a faster goroutine start for goroutines that
  cannot recurse, and visitor-facing disclosure reasons on the proof pages. Left: two seats (faster
  test-process start, an allocation-free element-address compare), the full re-bank, release day.
- **Validation.** Every implementable standard-library package validates against Go's own tests on
  Windows and Linux at 1.26.10, and macOS validation is under way row by row
  ([roster](../docs/ValidatedTestPackages.md)). **Figures are read on the day**, never quoted from this
  plan.
- **Converted modules.** `hashset`, `google/uuid`, `golang-jwt/jwt` v5, `x/sync`, `pflag`, `mousetrap`
  and `cobra` are published on go.* 1.26 through [nugetgo.net](https://nugetgo.net), and go2cs reads the
  nugetgo registry when it converts with `-recurse=nuget`, so a dependency listed there becomes a package
  reference instead of a fresh conversion ([Validated Modules](../docs/ValidatedModules.md)).
- **Docs.** The user-facing docs were reviewed for present-tense truth at 1.26.10.1, and pages reachable
  from go2cs.net carry no internal process language. A readability pass on Conversion Strategies is
  under way.
- **Already done for a launch:** the README's expectation line; issue forms (conversion bug, behavior
  difference, nugetgo package) with questions routed to Discussions; the label set; Discussions with a
  pinned "Start here"; the security policy's supported-versions statement and private vulnerability
  reporting; the About box and topics; GitHub Releases per NuGet release with binaries; the issues agent
  designed and scheduled at tier 0.

**What 1.26 changes for a newcomer** (confirm each against the NEWS entries before any post quotes it):

- **The Go version objection goes away.** The converted standard library is on a Go release Go still
  patches.
- **Install with one command** (1.26.10.2): `dotnet tool install -g go2cs`, with no Go installation
  needed to convert.
- **Plain string literals.** In a user's own conversion a Go string literal is written as an ordinary C#
  string literal where it is used. The standard library keeps hoisted literals, and `-hoist-literals`
  restores them for performance-critical code.
- **Slices grow as Go's do,** including the stack-append buffer Go's escape analysis gives a local slice
  (a visible `xᴷ` buffer in the C#).
- **Code spelled more like the Go**: shifts, casts and parentheses as Go writes them, with Go's comments
  and spacing.
- **Converted tests print Go's output** (`=== RUN`, `--- PASS`, the caller's `file:line`).
- **A program's first I/O deadline fires on time**, as Go's does, and **Native AOT** prints and reflects
  as Go does in cases that differed.

**What a newcomer will still hold against it**, in the order they meet it:

1. **Speed and size.** Usually slower than Go, slower to start, more memory; a Native AOT publish is slow
   and large. The performance page is being re-measured at 1.26 (section 3, item 4).
2. **The glyphs.** `ж<T>`, `ᴷ` and friends. The answer is the glyph table; ASCII spellings stay a
   rainy-day item.
3. **macOS.** Validation is in progress; Windows and Linux x64 are the finished platforms.
4. **How it is built.** The commit history shows AI assistance on most commits. Said first, in the
   owner's words (D6), it lands better than when it is discovered.
5. **Licensing.** Answered factually when asked, never raised in a post (section 7).

## 3. Gates before the first post

Nothing is announced until every **soft** row is done. "Who" uses the fleet's role names.

| # | Item | Who | Done when | Gates | Status 2026-10-27 |
|:--|:--|:--|:--|:--|:--|
| 1 | **go.* 1.26.10.2 published** | COORD; the owner (PIN) | the packages are on NuGet, the post-publish smoke is green on four RIDs, the GitHub Release carries the signed binaries and notes from the NEWS entry, and the `go2cs` tool package is on NuGet | soft | 1.26.10.1 done (2026-10-26); 1.26.10.2 in progress |
| 2 | **The nugetgo wave** | C2 prepares; the owner signs | every module republished on go.* 1.26, with mousetrap and cobra added; nugetgo.net lists them | soft (the posts' first example is `uuid`) | **done** (2026-10-26/27) |
| 3 | **Docs pre-flight at the release tree** | R | no visitor page states an older Go or an older install path for the current release; the README leads with the tool; FAQ, Limitations, Roadmap, Known issues and SECURITY read true at 1.26.10.2; every link the drafts use resolves | soft | done at 1.26.10.1; re-run at the 1.26.10.2 tree |
| 4 | **Performance page re-measured** | G, with the i9's AOT builds | the table in [Performance](../docs/Performance.md) is measured against the published 1.26 packages; the AOT honesty notes stand | wide (wanted for soft) | in progress |
| 5 | **Cold first-hour test on 1.26.10.2** | the owner runs it; COORD fixes | from the README to a converted program running, on Windows and on Linux, in under an hour, installing with the dotnet tool; every stumble fixed or written into Known issues | soft | waits for 1.26.10.2 |
| 6 | **The issues agent at tier 0 for a week** | COORD sets up; the owner approves drafts | [`AGENTS-issues-discussions.md`](AGENTS-issues-discussions.md) running, with at least a week of drafts read and the kinks written down | soft | **stalled**: scheduled, but no run has completed since 2026-10-22 (one run is stuck and holds the schedule); the week restarts once it runs |
| 7 | **Watchers dry-run** | COORD | each watcher run once against an old public thread, posting into its Slack channel; the kill switch tested | wide | not started |
| 8 | **The reply bank reviewed** | the owner | [`drafts/replies.md`](drafts/replies.md) read and edited in his voice, including D6 | soft | the owner's |
| 9 | **Old pre-release** | the owner | the 2022 `v0.1.2` pre-release is deleted or marked clearly as historical, so the Releases list shows only current releases | soft | open: still listed |
| 10 | **A code of conduct** | R | `CODE_OF_CONDUCT.md` (the Contributor Covenant) exists; GitHub's community profile stops flagging it | wide | open (profile 71%) |
| 11 | **A demo** | R | a recording of under a minute, install, convert, build, run, on the README | nice to have; not a gate | open |

Sizing ASCII glyph spellings stays rainy-day.

## 4. Timeline

Day 0 = go.* 1.26.10.2 on NuGet with its smoke green. Calendar dates assume day 0 is **Thu Nov 5**, the
middle of COORD's Nov 4 to 6 estimate; a day's slip in day 0 moves the soft stage by a day, up to the
hard edges listed below. The wide stage is anchored after Thanksgiving and has about a week of slack.

| Day | Estimated date | What | Owner time |
|:--|:--|:--|:--|
| now to −1 | Oct 27 to Nov 4 | 1.26.10.2's last seats and re-bank; the issues agent running again at tier 0; replies reviewed; the old pre-release settled; watcher dry-runs; the performance page re-measured; drafts filled from the NEWS entries | 3 to 4 h in all |
| 0 | **Thu Nov 5** (Nov 4 to 6) | go.* 1.26.10.2 published; GitHub Release; post-publish smoke | release sitting |
| +1 to +4 | Nov 6 to 9 | docs pre-flight at the release tree; cold first-hour test on Windows and Linux with the dotnet tool; fixes | 2 h test |
| **+5** | **Tue Nov 10** | **Soft stage.** Morning: news post live, Discussions announcement. Midday: personal messages | 2 h |
| +6 | Wed Nov 11 | Gophers Slack `#show-and-tell`, 10:00 US Eastern | 1 h over two days |
| +5 to +15 | Nov 10 to 20 | Listen and fix. A first-hour defect is fixed ahead of other work, in a patch release if it must be | 30 min a day |
| +15 | **Fri Nov 20** | **Wide go or no-go** (D1): no open first-hour defect, the issues agent's drafts are good, the watchers have passed their dry run | 30 min |
| +18 to +22 | Nov 23 to 27 | US Thanksgiving week: no new venue | |
| +25 | Mon Nov 30 | Newsletter submissions (Golang Weekly, one .NET newsletter); the news post updated with anything the soft stage taught | 30 min |
| **+26** | **Tue Dec 1**, 08:30 ET | **Show HN.** The owner present 08:30 to 12:30 ET, then checks at 15:00 and 20:00 ET | 4 to 6 h day one; 1 to 2 h day two |
| **+28** | **Thu Dec 3**, 09:00 ET | **r/dotnet** | 2 to 3 h day one; 1 h day two |
| **+33** | **Tue Dec 8**, 09:00 ET | **r/golang, optional** (D5) | 3 to 4 h day one; 1 to 2 h day two |

**Hard edges in the calendar** (confirm each nearer the time):

- **Tue Nov 3 is US election day.** Nothing goes up near it; day 0 sits after it.
- **.NET Conf and the .NET 11 release are expected in the second week of November** (about Nov 10 to 12).
  The soft stage overlaps it on purpose: its venues are Go-side and personal, and none of them is a .NET
  aggregator. No wide venue that week.
- **US Thanksgiving week, Nov 23 to 27.** No new venue; it is a dead week for both audiences.
- **Dec 18 to Jan 5.** Low traffic and nobody at a desk to try anything.

**If the soft stage finds a release-blocking stumble**, the wide stage moves one week: Show HN Tue Dec 8,
r/dotnet Thu Dec 10, r/golang dropped. **If it cannot start by about Dec 8, it waits for Jan 12 to 14,
2027.** That window has one wrinkle: Go 1.28 is expected in February 2027 and ends 1.26's upstream
support, and the 1.27 hop may be close by then. A January launch says so plainly; it does not wait for
1.27, because the policy is to hop every release, so there is always a next hop.

**Time of day.** 08:00 to 10:00 US Eastern on Tuesday to Thursday catches the US morning and the EU
afternoon (13:00 to 15:00 UK, 14:00 to 16:00 CET), which is when both HN's and Reddit's weekday
audiences are large. Earlier favours Europe; later loses it. Pick the time the owner can then stay at a
desk for three hours, which matters more than the minute.

**Assumptions.** 1.26.10.2 publishes in the Nov 4 to 6 range (COORD's rough estimate: two seats, the full
re-bank across the Windows boxes with Linux and macOS legs, then the release sitting). The owner has the
release sitting and the hours in section 6. The soft stage finds nothing that needs more than a patch
release.

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
| **Social accounts** (Bluesky, Mastodon, X, LinkedIn) | never in the plan; each is another stream to watch. Anything the owner posts there is his own choice, outside the watched set |
| **More than two newsletters** (the old plan named C# Digest, .NET Weekly and The Morning Brew) | each extra one is low yield; two is enough to learn whether the channel works |

## 6. The owner's hours

The whole launch, from now to the end of the wide stage, is estimated at **25 to 35 hours of the owner's
time across about six weeks** (Oct 27 to Dec 9), with the peaks on the HN and r/dotnet days. The rest of
the work is drafting, watching and triage, which the agents and the lanes carry.

| Week | What the owner does | Estimate |
|:--|:--|:--|
| Oct 27 to Nov 4 | review the replies and posts; approve the issues agent's drafts in Slack; settle the old pre-release; create the Slack channels | 3 to 4 h |
| Nov 5 to 9 | 1.26.10.2 release sitting (PIN); cold first-hour test with the dotnet tool | 4 to 6 h |
| Nov 10 to 20 (soft) | post four soft venues; answer the network and the Slack thread; approve issue drafts | 5 to 7 h |
| Nov 30 to Dec 4 (wide) | newsletters; Show HN; r/dotnet; approve drafts | 9 to 13 h |
| Dec 7 to 11 (optional) | r/golang | 4 to 6 h |

**The bandwidth rules:** one new venue a day at most, never two days running in the wide stage; a venue
day is a day at a desk; when the owner is away, nothing new goes up.

## 7. Message handling

**The rule that does not bend: nobody but the owner posts in public.** Agents read, draft and flag. They
never post, vote, react, or reply on any venue, and on GitHub they act only as the issues design's tiers
allow, each tier being the owner's explicit grant.

**Before the first post:** the expectation line in the README (done); issue forms and Discussions routing
(done); the reply bank reviewed (gate 8); the watchers and the issues agent dry-run (gates 6 and 7).

**On a venue day:**

- The owner posts and writes the first comment himself on HN: who he is, why he built it, and the three
  things it does not do. Then he stays three to four hours.
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
| [`drafts/release-1.24.13.6.md`](drafts/release-1.24.13.6.md), [`drafts/release-1.24.13.7.md`](drafts/release-1.24.13.7.md) | GitHub Release bodies, published, as is 1.26.10.1's; kept as the pattern for the 1.26.10.2 release body, written from its NEWS entry | records |
| [`drafts/retired/`](drafts/retired/) | drafts for dropped venues (r/csharp) | none |

## 9. Decisions for the owner

**For the soft stage:**

- **D15. Day 0 on 1.26.10.2. RULED by the owner 2026-10-27: day 0 is 1.26.10.2.** The alternative was a soft stage on the published 1.26.10.1 now
  (Wed Nov 4 at the earliest, after election day) with the tool arriving mid-listening. The second is
  about a week sooner, sends the first readers to a release whose compiled C# changes within days, and
  reaches them before the one-command install exists.
- **D1. Go or no-go, twice.** The soft stage, a few days after day 0 with gates 1, 2, 3, 5, 6, 8 and 9
  done. The wide stage, on Fri Nov 20 (or the Friday before a moved date).
- **D6. How to say how the project is built.** Two sentences in the owner's words, in the FAQ and in each
  post. Every draft has the placeholder.
- **D8. Accounts.** Which of the owner's accounts posts on HN and Reddit. Both sites filter new or
  little-used accounts, and a brand-new Reddit account is likely to be held by r/dotnet's filters.
- **D12. The issues agent's tier 1** (labels, asking for a repro, linking a doc, closing exact duplicates):
  granted after the owner has read a week of tier-0 drafts, or not.
- **D13. The watchers' model and budget.** Sonnet 5.5 for watchers is recommended
  ([`AGENTS-venue-watch.md`](AGENTS-venue-watch.md), cost section).

**For the wide stage:**

- **D5. r/golang or not.** Decided on Dec 4 from the hours the first two venues actually took.
- **D14. A January fallback**, if the wide stage cannot start by Dec 8: Jan 12 to 14 on 1.26, or later
  on whatever release is current.
- **D11 (carried). Sponsorship and commercial licensing in the posts:** not mentioned. Recommended as
  before.
