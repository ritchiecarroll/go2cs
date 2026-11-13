# PLAN: launching go2cs

> **State: REFRESHED 2026-11-13, present tense, for the owner's review.** This branch (`claude/launch`) is a planning
> branch. It is never merged to master and never published on the site. **It holds no date the owner has not set:**
> each stage starts after the one before it, and the first starts after go.* 1.26.11.1 publishes.
>
> **The shape** is still the owner's direction of 2026-10-20: a release published before any announcement; a soft
> stage first, then the venues that need more live response; few venues, because one person answers all of them.
> **What changed:** the soft stage now waits for **go.* 1.26.11.1** (owner-ruled), and the wide stage waits for two
> more releases after it: M:N goroutine scheduling (.5), then one minor hop.

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
       are among the landed seats; the README's tool-first docs commit is held for release day.
     - 2026-11-13 refresh (C1, COORD's low-priority order relaying the owner, inbox C1 20261113T164903Z). The soft
       release is now go.* 1.26.11.1 (owner-ruled; it was 1.26.10.4, and before that the plan's day 0 was
       1.26.10.2 with a Nov 10 soft stage, which this ruling supersedes). The wide stage follows M:N (.5) and one minor hop.
       Read the same day: origin/master 651c4ad147 (batch 300); origin/claude/launch 414fe9d48b. GitHub Releases
       (REST): nuget-1.26.10.3 published 2026-11-09, nuget-1.26.10.2 2026-11-04, nuget-1.26.10.1 2026-10-26,
       nuget-1.24.13.7 and .6, and the 2022 v0.1.2 pre-release still listed. Repository: 402 stars, 2 open issues,
       Discussions on, community profile 71%, no CODE_OF_CONDUCT.md at master or in .github. Roster at master:
       238 / 242 testable packages validated (updated 2026-11-08); Linux 236 of 236 applicable rows at linux-x64
       and linux-arm64 236 of 236 banked. Limitations at master still lists C.CString, C.GoString and C.GoBytes as
       cgo Phase 3, so 3.1 is not on master yet. The release notes are being written in docs/DRAFT-news-1-26-10-4.md
       (its title still says 1.26.10.4). Performance.md measures against published 1.26.10.2. go2cs.net did not
       answer from the session that wrote this, and the typescript-go-cs and sqlite-cs repositories are not
       reachable from it, so their links are marked "check on the day". -->


Contents: [1. The recommendation](#1-the-recommendation) · [2. Where things stand](#2-where-things-stand) ·
[3. Gates before the first post](#3-gates-before-the-first-post) · [4. Order of events](#4-order-of-events) ·
[5. Venues: kept and dropped](#5-venues-kept-and-dropped) · [6. The owner's hours](#6-the-owners-hours) ·
[7. Message handling](#7-message-handling) · [8. Drafts](#8-drafts) ·
[9. Decisions for the owner](#9-decisions-for-the-owner)

## 1. The recommendation

**Two announcements, each on a release that carries its own story.**

1. **The soft release, go.* 1.26.11.1.** After it publishes, with its post-publish smoke green, four quiet venues
   in this order: the go2cs.net news post, a GitHub Discussions announcement, personal messages, and one post in the
   Gophers Slack `#show-and-tell` on a later day. Then listen and fix what a newcomer trips on. What the release
   carries (each item confirmed against its NEWS entry before a post quotes it):
   - **the Go 1.26.11 patch hop**: the converted standard library moves from Go 1.26.10 to 1.26.11;
   - **validation on four platforms**: Windows, Linux x64, Linux arm64 ([236 of 236] rows) and macOS ([234 of 236]);
   - **cgo Phase 2**, plus **3.1**'s `C.CString`, `C.GoString` and `C.GoBytes`, `void*`, and `defer C.free`, with the
     disclosed residue (if 3.1 misses the release, the posts say Phase 2 only; gate 14);
   - **the TypeScript proof**: typescript-go-cs, TypeScript's own Go compiler converted to C#: parse [17,422 of 17,422]
     test files, and type-check and emit [7,404 of 7,404] byte-identical to the native build;
   - **sqlite-cs**, modernc.org/sqlite converted, published as work in progress that fails openly;
   - **the public-surface breaks taken before outside use**: Go `internal` packages marked (OQ1), methods moved to
     holder classes (OQ2) and assemblies named after their packages (OQ4);
   - **the converted-by-go2cs badge**, and nugetgo.net;
   - **the Tour of go2cs**, Go and its C# side by side.
2. **The wide announcement**, after **M:N goroutine scheduling (.5)** and **one minor hop** after it. M:N answers the
   objection a Go reader raises first (goroutines as OS threads), so the large audiences hear about it after it
   ships, not before. Same venues as before: two newsletters, Show HN, r/dotnet, and r/golang only if the owner has
   the hours (section 5). One new venue a day, never two days running.

**Watcher agents read the live threads and draft replies into Slack; the owner posts every public word himself**
([`AGENTS-venue-watch.md`](AGENTS-venue-watch.md)). An issues agent drafts GitHub answers the same way
([`AGENTS-issues-discussions.md`](AGENTS-issues-discussions.md)).

**Why not announce on a published release now.** 1.26.10.3 is out (2026-11-09), but 1.26.11.1 changes what a
newcomer compiles against (the assembly rename, the holder classes) and carries the two public proofs. Announcing
on 1.26.10.3 would send the first readers to a shape that breaks in the next release.

**What moves the soft stage:** 1.26.11.1's remaining seats and re-bank, the two public repositories, and anything the
cold first-hour test finds. Each moves the step after it; none is traded for a date.

## 2. Where things stand

Read 2026-11-13; linked rather than restated, so the counts stay where they are maintained.

- **On NuGet: go.* 1.26.10.3** (2026-11-09), the third release on Go 1.26.10, after 1.26.10.1 (2026-10-26) and
  1.26.10.2 (2026-11-04, the `go2cs` dotnet tool). Each has a GitHub Release.
- **In progress: go.* 1.26.11.1.** The seats written so far for it are in the release notes being drafted on master
  (`docs/DRAFT-news-1-26-10-4.md`, still titled 1.26.10.4): default build tags `safe,purego,noasm` for `-recurse`;
  `main` keeps its name and runs on a 1 GiB stack; `go2cs -warn`; `-recurse` skipping a refused package; the
  assembly rename (OQ4, a break with a build-time floor, `GO2CS3002`); `GO2CS2005` and `GO2CS2006`; a promoted
  method's forwarder moved into a class of its struct's own. The Go 1.26.11 hop, cgo 3.1, OQ1's marking and OQ2's
  holder-class move are not in those notes yet.
- **Validation.** Every implementable standard-library package validates against Go's own tests
  ([roster](../docs/ValidatedTestPackages.md)); Linux x64 and arm64 are banked row for row, and macOS rows carry
  their own counts. **Figures are read on the day**, never quoted from this plan.
- **Install.** `dotnet tool install -g go2cs`, with no Go installation needed to convert (since 1.26.10.2).
- **Converted modules.** nugetgo.net lists the published modules. 1.26.11.1's break floor means each `nugetgo.*`
  package is rebuilt against it (gate 2).
- **Already done for a launch:** the README's expectation line; issue forms with questions routed to Discussions;
  labels; Discussions with a pinned "Start here"; the security policy and private reporting; the About box and
  topics; GitHub Releases with binaries; the issues agent designed.

**What a newcomer will still hold against it**, in the order they meet it:

1. **Speed and size.** Usually slower than Go, slower to start, more memory; a Native AOT publish is slow and large.
   The [performance page](../docs/Performance.md) measures against published 1.26.10.2.
2. **Goroutines are OS threads** until M:N (.5). The soft posts say so in their first lines.
3. **The glyphs.** `ж<T>`, `ᴷ` and friends; the answer is the glyph table.
4. **How it is built.** The commit history shows AI assistance on most commits. Said first, in the owner's words
   (D6), it lands better than when it is discovered.
5. **Licensing.** Answered factually when asked, never raised in a post (section 7).

## 3. Gates before the first post

Nothing is announced until every **soft** row is done. "Who" uses the fleet's role names. **Status** is read from
master and the ledger on 2026-11-13; "unknown" means nothing this session could read says.

| # | Item | Who | Done when | Gates | Status 2026-11-13 |
|:--|:--|:--|:--|:--|:--|
| 1 | **go.* 1.26.11.1 published** | COORD; the owner (PIN) | the packages are on NuGet, the post-publish smoke is green on the shipped RIDs, the GitHub Release carries the signed binaries and the notes from the NEWS entry, and the `go2cs` tool package is updated | soft | in progress; latest published is 1.26.10.3 |
| 2 | **The nugetgo rebuild wave** | C2 prepares; the owner signs | every `nugetgo.*` package republished against 1.26.11.1 (the break floor requires it); nugetgo.net lists them | soft | owed with the release |
| 3 | **Docs pre-flight at the release tree** | R | no visitor page states an older Go, an older install or an older assembly name; FAQ, Limitations, Roadmap, Known issues and SECURITY read true at 1.26.11.1; every link the drafts use resolves | soft | owed at the release tree |
| 4 | **Performance page current** | G | the table is measured against published packages, and the AOT honesty notes stand | wide (wanted for soft) | measured against 1.26.10.2; a re-measure at 1.26.11.1 is optional |
| 5 | **Cold first-hour test on 1.26.11.1** | the owner runs it; COORD fixes | from the README to a converted program running, on Windows, Linux and macOS, in under an hour, with the dotnet tool; every stumble fixed or written into Known issues | soft | waits for 1.26.11.1 |
| 6 | **The issues agent at tier 0 for a week** | COORD sets up; the owner approves drafts | [`AGENTS-issues-discussions.md`](AGENTS-issues-discussions.md) running, a week of drafts read | soft | unknown (the last record here: stalled since 2026-10-22) |
| 7 | **Watchers dry-run** | COORD | each watcher run once against an old public thread, posting into its Slack channel; the kill switch tested | wide | unknown |
| 8 | **The reply bank reviewed** | the owner | [`drafts/replies.md`](drafts/replies.md) read and edited in his voice, including D6 | soft | refreshed 2026-11-13; the owner's |
| 9 | **Old pre-release** | the owner | the 2022 `v0.1.2` pre-release deleted or marked as historical | soft | open: still listed |
| 10 | **A code of conduct** | R | `CODE_OF_CONDUCT.md` exists; the community profile stops flagging it | wide | open: none at master; profile 71% |
| 11 | **A demo** | R | under a minute: install, convert, build, run, on the README | not a gate | unknown |
| 12 | **typescript-go-cs public** | the owner | the repository is public, its README states the figures with how they were measured and links go2cs | soft (the posts link it) | unknown; public on push |
| 13 | **sqlite-cs public, failing openly** | the owner | the repository is public, and its README says it is work in progress and where it stops | soft (the posts link it) | unknown here |
| 14 | **cgo 3.1 in the release, or out of the posts** | H; COORD decides | 3.1's helpers are in 1.26.11.1 with their residue disclosed in Limitations, or every draft says Phase 2 only | soft | not on master (Limitations lists `C.CString` as Phase 3) |
| 15 | **The public-surface breaks shipped** | R, C2, P1 | OQ1, OQ2 and OQ4 are in 1.26.11.1, each under *Upgrading* in the NEWS entry, and [The public surface](../docs/PublicSurface.md) reads true | soft | OQ4 is in the draft notes; OQ1 and OQ2 unknown |

## 4. Order of events

Each step starts after the one before it. Dates are the owner's to set; this section holds none.

| Step | What | Owner time |
|:--|:--|:--|
| before release | the remaining 1.26.11.1 seats, the Go 1.26.11 hop and re-bank; the issues agent running at tier 0; replies reviewed; the old pre-release settled; the two repositories made public; drafts filled from the NEWS entry | 3 to 4 h in all |
| release day | go.* 1.26.11.1 published; GitHub Release; post-publish smoke; the nugetgo rebuild wave | release sitting |
| after release | docs pre-flight at the release tree; cold first-hour test on Windows, Linux and macOS with the dotnet tool; fixes | 2 h test |
| **soft stage, day 1** | morning: news post live, Discussions announcement; midday: personal messages | 2 h |
| **soft stage, a later day** | Gophers Slack `#show-and-tell`, 10:00 to 12:00 US Eastern on a Tuesday to Thursday | 1 h over two days |
| listening | fix what a newcomer trips on, a first-hour defect ahead of other work, in a patch release if it must be | 30 min a day |
| later releases | M:N (.5), then one minor hop | |
| **wide stage** | go or no-go (D1), then newsletters, Show HN, r/dotnet, and r/golang only if the owner has the hours, one venue a day and never two days running | section 6 |

**Calendar edges** to check when the owner sets a date: US Thanksgiving week (Nov 23 to 27, 2026) and Dec 18 to
Jan 5 are low-traffic, with nobody at a desk to try anything.

**Time of day.** 08:00 to 10:00 US Eastern on Tuesday to Thursday catches the US morning and the EU afternoon, which
is when HN's and Reddit's weekday audiences are large. Pick the time the owner can then stay at a desk for three
hours, which matters more than the minute.

## 5. Venues: kept and dropped

Rules change; read each venue's own rules on the day, and follow them over this table. Response load is an estimate
of the comments to read and the replies worth writing, not a forecast of success.

**The soft stage, in order**

| Order | Venue | Why | Expected response load | When | Watched by |
|:--|:--|:--|:--|:--|:--|
| 1 | **go2cs.net news post** | the page every other post links to | none on the page (no comments); traffic lands on GitHub | soft-stage morning | the issues agent |
| 2 | **GitHub Discussions, Announcements** | the repository's own channel; the people who starred or watch it are the friendliest first readers | low: a handful of replies over a week | beside the news post | the issues agent |
| 3 | **The owner's network** | the best first-hour feedback there is | low, one-to-one | the same day, midday US Eastern | the owner's inbox |
| 4 | **Gophers Slack `#show-and-tell`** | Go developers, a thread-contained post, notifications reach the owner | low: 0 to 15 thread replies over two days | a later Tuesday to Thursday, 10:00 to 12:00 US Eastern | the owner's Slack notifications; no agent (reading another workspace would need an app its admins approve) |

**The wide stage** (after M:N and one minor hop)

| Venue | Why kept | Expected response load | Best timing | Watched by |
|:--|:--|:--|:--|:--|
| **Newsletters** (Golang Weekly; one .NET newsletter) | reach without a thread to answer | none live | the Monday of the wide week | `#launch-mentions` watcher |
| **Show HN** | the largest single audience for a technical project, and the validation method is HN's kind of story | the highest and most concentrated: 50 to 200 comments if it reaches the front page, under 10 if not | Tue to Thu, 08:00 to 10:00 US Eastern | `#launch-hn` watcher |
| **r/dotnet** | the audience that can use go2cs today; the post leads with working code | 20 to 80 comments over two days | at least a day after HN | `#launch-r-dotnet` watcher |
| **r/golang, optional** (D5) | worth it for the semantic-fidelity story, and M:N removes its first objection, only if the owner has the hours | high and the most contentious | last | `#launch-r-golang` watcher |

**Dropped**

| Venue | Why dropped |
|:--|:--|
| **C# Discord showcase** | chat expects replies in minutes; its audience overlaps r/dotnet; Discord's terms forbid automated reading |
| **r/csharp** | a second .NET subreddit in the same week, for an overlapping audience; the draft is in [`drafts/retired/`](drafts/retired/) |
| **Lobsters** | needs an invitation in hand; a small audience that overlaps HN; the mentions watcher sees it if someone else posts it |
| **Social accounts** | never in the plan; anything the owner posts there is his own choice, outside the watched set |
| **More than two newsletters** | each extra one is low yield |

## 6. The owner's hours

| Stage | What the owner does | Estimate |
|:--|:--|:--|
| before release | review the replies and posts; approve the issues agent's drafts; settle the old pre-release; make the two repositories public | 3 to 4 h |
| release | the 1.26.11.1 release sitting (PIN); the nugetgo wave; the cold first-hour test | 4 to 6 h |
| soft stage | post the four soft venues; answer the network and the Slack thread; approve issue drafts | 5 to 7 h over about ten days |
| wide stage | newsletters; Show HN; r/dotnet; approve drafts | 9 to 13 h |
| wide, optional | r/golang | 4 to 6 h |

**The bandwidth rules:** one new venue a day at most, never two days running in the wide stage; a venue day is a day
at a desk; when the owner is away, nothing new goes up.

## 7. Message handling

**The rule that does not bend: nobody but the owner posts in public.** Agents read, draft and flag. They never post,
vote, react or reply on any venue, and on GitHub they act only as the issues design's tiers allow, each tier being
the owner's explicit grant.

**On a venue day:**

- The owner posts, and on HN writes the first comment himself: who he is, why he built it, and the three things it
  does not do. Then he stays three to four hours.
- The watcher posts each new comment into the venue's Slack channel with a suggested reply, a confidence and a
  needs-the-owner flag. The owner edits and posts, or skips.
- Every reply is short, grants what is true in the objection, and links the page with the detail. **Acknowledge and
  link; never argue.** A hostile comment gets one civil answer or none.
- A real defect gets thanks, an issue link within the hour, and a label.

**In the days after:** the issues agent triages GitHub; one daily digest in Slack says what came in, what was
answered and what needs the owner. At the end of the soft stage the stumbles are counted by kind, and that count
shapes the wide posts.

**Licensing, as a rule of tone:** no post mentions licensing, the license history or commercial terms. When someone
asks, the answer is short, factual and linked ([`drafts/replies.md`](drafts/replies.md)), and the thread moves on.

**The predictable objections,** each with a prepared answer in the reply bank: why not cgo or a native library; why
not just use Go; performance and Native AOT; how the tests are validated; the Go version and the cadence; maintenance
and one maintainer; the license; the generated style and the glyphs; was it written by AI; goroutines as threads;
production-ready; macOS; cgo's scope; the TypeScript claim; sqlite-cs.

## 8. Drafts

In [`drafts/`](drafts/). Figures in [square brackets] are read on the day of posting, from the roster, the release or
the repository that states them.

| File | For | Stage |
|:--|:--|:--|
| [`drafts/blog-soft-launch.md`](drafts/blog-soft-launch.md) | the go2cs.net news post | soft (not refreshed on 2026-11-13; written from the 1.26.11.1 NEWS entry on the day) |
| [`drafts/short-posts.md`](drafts/short-posts.md) | the Discussions announcement, the network message, Gophers Slack, the newsletter blurbs | soft and wide |
| [`drafts/show-hn.md`](drafts/show-hn.md) | Show HN | wide (rewritten after M:N) |
| [`drafts/reddit-dotnet.md`](drafts/reddit-dotnet.md) | r/dotnet | wide (rewritten after M:N) |
| [`drafts/reddit-golang.md`](drafts/reddit-golang.md) | r/golang | wide, optional |
| [`drafts/replies.md`](drafts/replies.md) | the reply bank, which the watchers draft from | all |
| [`drafts/release-1.24.13.6.md`](drafts/release-1.24.13.6.md), [`drafts/release-1.24.13.7.md`](drafts/release-1.24.13.7.md) | GitHub Release bodies, published; the pattern for 1.26.11.1's, written from its NEWS entry | records |
| [`drafts/retired/`](drafts/retired/) | drafts for dropped venues | none |

## 9. Decisions for the owner

**Ruled:**

- **D16. The soft release is go.* 1.26.11.1** (ruled by the owner; it replaced 1.26.10.4, which replaced D15's
  1.26.10.2). The wide announcement follows M:N (.5) and one minor hop.

**For the soft stage:**

- **D1. Go or no-go, twice.** The soft stage, once gates 1, 2, 3, 5, 6, 8, 9 and 12 to 15 are done. The wide stage,
  after the M:N release and the minor hop after it.
- **D6. How to say how the project is built.** Two sentences in the owner's words, in the FAQ and in each post.
  Every draft has the placeholder.
- **D17. sqlite-cs in the soft posts.** Link it as work in progress that fails openly (recommended: it shows the
  method on a real program, including where it stops), or leave it out until it runs.
- **D8. Accounts.** Which of the owner's accounts posts on HN and Reddit (wide stage).
- **D12. The issues agent's tier 1**, granted after a week of tier-0 drafts, or not.
- **D13. The watchers' model and budget** ([`AGENTS-venue-watch.md`](AGENTS-venue-watch.md), cost section).

**For the wide stage:**

- **D5. r/golang or not**, decided from the hours the first venues took.
- **D11 (carried). Sponsorship and commercial licensing in the posts:** not mentioned.
