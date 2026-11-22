# Stage (c): the twin census on one closure (record)

**Read 2026-11-15/16 by G.** Plan approved by COORD 2026-11-15 (PC-1..5, SC-1..4; every run line states the
scheduler configuration). Record 10.3/10.8/10.9 of DATA-mn-spike-2.md asked for it: the park set under the
all-members interface rule, read two ways, and one closure built so. Branch `claude/g-mn-proto-b0`.

## The closure

MnHttpClosure (scratch): a loopback net/http server and its client in one process. Handlers park on a channel and a
timer, the client on the network; six GETs; it ends on its own. Go prints 9 lines, the same three runs out of three.
`go list -deps`: 186 packages. Converted with `-recurse` against the tree's `src/core`.

**Harness requirement (found here).** A converted program outside the tree must import the tree's
`Directory.Build.targets` (`<Import Project="$(go2csPath)Directory.Build.targets" …/>` in the output root's
`Directory.Build.targets`). Without it the program never sees `GoTwinsFor`, its `main` is not twinned, and the
scheduler never starts: arm A first read "equal to Go" with no scheduler at all. In-tree projects get it implicitly.

## Arms (final reading at a3f2f10be3, GOMAXPROCS=12, 12 cores)

| arm | rule | build | twinned of 13,502 | await sites | iface marked | iface awaits | sync fwd | go.*.dll bytes (vs P) |
|---|---|---|---|---|---|---|---|---|
| P | plain | 246 s | - | - | - | - | - | 78,031,360 |
| A | auto | 782 s* | 2,733 (20.2%) | 5,971 | 43 of 366 | 301 | 121 | 82,274,304 (+5.4%) |
| B | all members | 711 s* | 2,999 (22.2%) | 7,167 | 345 | 1,077 | 658 | 82,780,672 (+6.1%) |
| C | B, Stringer.String unmarked | 438 s* | 2,998 (22.2%) | 7,165 | 344 | 1,075 | 658 | 82,779,648 (+6.1%) |

\* Walls overlapped other builds on the box (the W18/W19 sqlite proofs). Uncontended at 7dd76643df they read A 427 s,
B 369 s, C 442 s: 1.5-1.8x P. Every twin run printed Go's 9 lines with procs=12, parks 4-11, hand-offs 112-117, peak
carriers 20-24, peak OS threads 51-60.

Refusals (all arms): byref-recv 2,377 (A 362 / B 479 park), byref-param 395 (54 / 60), unsafe 148, reflike 109,
host-not-partial 94 (2 park), native 19, instance 15, await-in-lock 3 (3 park), iterator 2, has-twin 2,
explicit-impl 2, ref-unrederivable 1. Interface members refused: 21 (19 by signature, 2 host-not-partial).
go-plain 6 (GO2CS0014 names each), go-only 8.

## Scores (as posted)

PC-1 MISS (neither arm built clean at first: 6 classes, 13 cures); PC-2 A 20.2% (12-20: MISS by 0.2), B 22.2%
(35-50: MISS), C 0.0 points below B (3-8: MISS); PC-3 A +5.4% HIT, B +6.1% (12-20: MISS), C = B (MISS); PC-4 HIT;
PC-5 MISS (94 methods + 2 interface members on non-partial hosts, 2 park). SC-1 fired and was ruled each time;
SC-2 (+30%), SC-3 and SC-4 (3x) did not.

## What it says

- **The park set is capped by byref-recv**, not by the interface rule. Marking every interface member adds 266 twins,
  776 interface await sites and 537 sync forwarders; 2,377 by-ref-receiver refusals (479 parking) are what bound
  it. N2, the box twin, is what moves it (its design: DESIGN-mn-box-twin.md).
- **The short list is immaterial here.** `error.Error` is unmarked by construction (golib's hand-owned `error` has no
  twin member); `Stringer.String` moves 1 twin and 2 await sites.
- **10.8's owed count**: 94 methods and 2 interface members on hand-owned non-partial hosts; 2 would park. Making
  those hosts partial is a separate corpus decision.

## The cures the closure forced (each red first; COORD ruled each new class before it was fixed)

a4e999288e GoTwinsIfaceUnmark (the switch arm C needs); bd86ea0ca9 host-not-partial (methods); 393cef03e3 a compound
store through a ref spills; 2052b5c1a3 await-in-lock; 26578a50e2 adapters skip twin members (719 adapters byte-
identical in a plain build, a plant caught); 4db1a48034 a ref read later in the statement spills the call;
86dd29fa01 Rule S derives refs after each moved call; 6985127517 a struct receiver reaching a ref; a23d98847d a by-ref
argument before an awaited one; 535a76b8a6 host-not-partial (interface members); 7dd76643df a struct receiver through
any reference; 2414b35286 only reads in the statement's own expression count (MnMixedStorm's regression);
a3f2f10be3 an await in a case guard makes every section derive the refs again. GenTests 564; sweep 29/29.
