# TRAIN T4: the linux legs (P1 and P2)

**T4 RUN2 (COORD 2026-10-13), READ THIS FIRST; it leads every paragraph below, the T4 block included.** Read
`claude/coord-trainT4-union-r2` at `58f17868feec5e8e317ca8293ab662ef530a0839` ("fixup-2: TRAIN T4", the commentcensus
corpus re-baseline). Its chain: `fixup-2` 58f17868fe on `fixup: TRAIN T4` `94ca118a20d1504b4bbc2092e1bbd1543ac209fb`
(1,555 files) on the union `b258fe81db94baf80798e5781dc85688fc2c6bb1` (11 seat merges on BASE
`226afb810a21408ebda2412dd74d526345c46f65`). Run1's union, `claude/coord-trainT4-union` at `1438ed20ea`, is HISTORY:
row 2 was re-seated at `claude/c1-comment-parity-gap-fix` `fbf2a5ebc5`; read nothing at run1's ref.
- **The driver fetches run2's ref.** `tT4-linux-legs.sh` in this folder fetches `UNION_REF`, default
  `claude/coord-trainT4-union-r2` (run1's copy hard-coded `claude/coord-trainT4-union` and would ABORT on the SHA check).
  Take the driver from THIS commit; do not set `UNION_REF` unless the GO names another ref.
- **Row 2's footprint, CORRECTED.** C1's comment-parity chain did NOT commit its src/core footprint in the seat. The
  FIXUP regenerated 1,548 -stdlib src/core paths across windows, linux and darwin, admitted by an exact path file after
  a token-identity proof: comment / whitespace only, package_info.cs GoPositionMap lines only, plus two named code
  classes, DEREF-ALIAS-DROPPED (dead dereference aliases dropped, `11ef3a1207`) and LAMBDA-EXPR-BODY (a lambda's
  `{ return E; }` body emitted as `=> E`, `58413a5f57`); OTHER = 0. The committed -tests sources (`*_test.cs`) were NOT
  regenerated (the fixup holds 0 of them). Row 9 (P2's raw strings) DID commit its 117 `_test.cs` in the seat.
- **TE PREDICTION.** Your `tests-*.srcdiff.patch` files are PREDICTED to carry THE SAME comment-parity classes in the
  committed test sources, possibly in bulk (not "~0", as the T4 block below said; item 7's N-PARTIAL class is Q's and is
  history). Every content rewrite is classed: post every `tests-*.srcdiff.patch` (-U0) with its file count, and COORD
  classes them on the i7 with `tT4-i9-drift.sh` (the token-identity reader: comment / whitespace, the two named
  classes, and OTHER, which is read BY NAME). A linux lane's rewrites are never the refresh's input.
- **CNR** N = 873 on windows (880 enumerated, 7 platform skips); on linux read your own enumeration.
- **The previous train's record is T3's**: your T3 legs' srcdiff patches and SUMMARY are the baseline. T3's union
  (fixup `38f1ce2510`, fixup-2 `9adab1a13d`) is history.
- **Disk.** The driver's `FLOOR_GB` defaults to **15**: it reads free space before EVERY leg and stops with exit 3
  under it. A box with less than 15 GB free does not run these legs; COORD's GO names the box for each leg set.
- **Launch values**: `UNION=58f17868feec5e8e317ca8293ab662ef530a0839`, `BASE=226afb810a21408ebda2412dd74d526345c46f65`,
  `FIXUP=94ca118a20d1504b4bbc2092e1bbd1543ac209fb`.


**T4 (2026-10-12), READ THIS FIRST; it leads every paragraph below.** TRAIN T4 is 11 rows over base master
`226afb810a`: C1's darwin x509 roots companion (behavioral X509SystemVerify) and comment parity seats A to G (the corpus
and goldens re-emitted IN the seat) **[SUPERSEDED 2026-10-13 (RUN2): row 2 committed its behavioral goldens in the seat but NOT its src/core footprint; the FIXUP regenerated 1,548 src/core paths and no `_test.cs`: the T4 RUN2 block leads]**; G's promoted-field identity / trim 3d family E (row 3, two files RULED to the seat's
side), the per-site goroutine creator (golib Goroutine.cs) and a docs row seated BY SHA (a5ef12237c); C2's nugetgo RID
packs; P2's hop readiness tools, the .Raw() raw-string fix (behavioral RawStringLineEndings, 117 _test.cs committed IN
the seat) and the retired-hand-own guard; the i9's H5c GOOS-child check. The union is pushed as
`claude/coord-trainT4-union` **[run1, HISTORY: run2 is `claude/coord-trainT4-union-r2`]** (COORD names the SHA in the GO: the T4 fixup, or a `fixup-N: TRAIN T4` on top). The fixup
carries ONE projitems line (retiredHandOwns_test.go) and the union regeneration of RawStringLineEndings and
X509SystemVerify. Rows 2 and 9 change -stdlib AND -tests emission but committed their footprints in-seat, so your sweeps
are PREDICTED to rewrite ~0 committed test sources beyond the standing line-ending / OTHER set; any content rewrite is
read BY NAME and posted. **[SUPERSEDED 2026-10-13 (RUN2) for row 2: rows 2 and 9 did NOT both commit their footprints in-seat, and the test-source rewrites are PREDICTED in bulk, of the comment-parity classes: the T4 RUN2 block leads]** The previous train's record is T3's (your T3 shard patch is the TE baseline). CNR N at the
union = 873 on windows (880 enumerated, 7 platform skips; on linux read your own enumeration). The paragraphs below are
T3's and FL's, names moved: where they name T3's rows or SHAs they are history.


**FL (2026-10-08), read this first.** DERIVED from TRAIN Q's brief (`trainQ/tQ-lane-brief-linux.md`), names only, plus:
(FL's text, superseded by the T3 block above:) TRAIN FL was the face lift (rows 1-10 hand-merged on FL's union, row 10 the ONE regeneration of the
-stdlib corpus on three targets and the goldens; rows 11 and 12 merged by COORD's assembler before the fixup). Your
sweeps rewrite the committed test sources in bulk (the face lift's classes): count and post them as before; a linux
lane's rewrites are never the refresh's input. The base is TRAIN Q's landed master line and COORD's docs commits
after it (`541766413e` at the derive). The single-volume `testing` reading P1 took at Q (green) is the control for the
i7's ruled divergence; nothing is owed for it at FL unless COORD's GO asks. Every -recurse module -tests row you hold
(LM: x/sync, x/sync2, x/mod) is read at the FIXUP head as soon as the GO names it, not after the battery (FL10).
The union's first-parent line may hold one `refresh: TRAIN T4` between the fixup and a `fixup-N` (FL2): the driver
admits it.

(Q's text, HISTORY at T4:) From COORD. DERIVED 2026-10-06 from TRAIN P's brief (`trainP/tP-lane-brief-linux.md`; at P's union + fixup-2 P1 read
43 legs with two explained movers, P2 exit 0 with 0 movers). **Q is an ordinary train**: whether a release follows
is decided at its landing; the train after it re-converts the corpus, so these are the linux readings of the tree
that becomes that base. **Readings only**: nothing to cut, commit or push. Start when COORD's GO names the SHAs.

| | |
|---|---|
| Union ref / SHA | `claude/coord-trainT4-union-r2` / `58f17868feec5e8e317ca8293ab662ef530a0839` (RUN2: `fixup-2: TRAIN T4`, on the fixup `94ca118a20d1504b4bbc2092e1bbd1543ac209fb`; run1's `claude/coord-trainT4-union` is HISTORY) |
| Base | `226afb810a21408ebda2412dd74d526345c46f65` = TRAIN P's landed master line (the driver refuses a commit of the union's line that is not a Q seat merge or a Q fixup above it) |
| Driver, helper, seat list | `tT4-linux-legs.sh`, `tT4-helpers.py`, `tT4-seats-draft.txt` from `.claude/coord-scripts/trainT4/` on `claude/coord-handover` at `<HND>`, all from ONE fetch, in one folder outside the clone |
| Second-run row | `TWOPASS_ROW`, default `runtime/debug` (P2's control row at P) |

## Launch (P's form)

```
LANE=P1 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/tT4/tT4-linux-legs.sh
LANE=P2 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/tT4/tT4-linux-legs.sh
```
Launch DETACHED from your session (P2's launcher met a two-hour task limit inside the runtime row at P), and read
the free space first: P1's first two runs at P stopped on their own disk floor, not on a mover. The clone is FROZEN
while the driver runs (floor 4: C2 named a breach of its own at P, a commit in a worktree while its gate ran). Root
readings do not stand in for non-root ones: say your uid.

## What each lane owes, at the tip the GO names

| Lane | Legs | EXPECT |
|---|---|---|
| P1 | L1 `unicode/utf8` (the quick STOP gate) | Validated 14 |
| P1, P2 | **T2** (new): `TWOPASS_ROW` twice into one tree | the roster's linux count twice, the same file names beside the published host, more than one `.pdb` (P2 at P's fixup-2: 71 and 71) |
| P1 | GolibTests Release and Debug, full, then the classes the union adds or changes by name | the added classes are DetachedTestingValueTests, HostGoTestArgvTests, SigChanDirReaderTests; `Failed: 0` |
| P1 | LCn: the converter and repoguard tests the union adds, by name | every name `--- PASS`, 0 SKIP / FAIL: 44 added names at the draft (the driver's lexical scan drops fixture lines, stamped). A test that needs something this box lacks and SKIPS is a line to post with its skip text |
| P1 | the rows at their roster config | each at the roster line's linux reading |
| P1 | LPB (`cmp` with the publish binlog) | Validated 4 |
| P1 | LX: the linux-only behavioral projects re-emitted | 0 hunks but the one KNOWN residual |
| P1 | LB: the behavioral projects through the runner | every project the union adds (23 at the draft) four phases, and the linux-only seven |
| P1 | LM: `x/sync` and `x/mod` through `-tests -recurse`, then **`xsync2`** (new): `x/sync` again into the same root | errgroup 5, syncmap 3, singleflight 12, semaphore 8 or its KNOWN 7 of 8; x/mod 9 of 9 with tlog 17; the second run: the same verdicts, the same `.pdb` count beside each host AND more than one each (`floor before=ok after=ok`: equal counts of zero are a mover, not a pass) |
| P2 | GolibTests Release full + the classes by name | `Failed: 0`; the sampler test (below) is the KNOWN class |
| P2 | T2; runtime/pprof; the FULL runtime row; the TestTracebackSystem loop | 147 + 7; 10810 + 73; /panic and /trap disclosed, 0 deadlock lines |
| P2, still out at review round 1 (Q's row, HISTORY at T4: not owed) | the linux full behavioral for g-r2m-caller-closure-v2, with attribution re-reads (dispatched 50738bd2a0; the row is ACCEPTED since 14:59 10-06: a reading, never its condition) | as dispatched; not a leg of the driver |

## What changed since P's legs

**(COORD 2026-10-13: item 7's N-PARTIAL class is Q's, HISTORY at T4; T4's prediction is the T4 RUN2 block's. Items 1 to 6 stand.)**

1. **Nothing is deleted before a -tests leg** when the union's converter always writes the comparison record (it
   does: the i9's row; the driver reads the converter's source at the union). A record is fresh when it is newer
   than its leg's marker. A stale or absent record is still `NO FRESH COMPARISON RECORD`, a mover.
2. **T2 and xsync2**, the second-run arms. P's symbol loss showed on linux exactly as on windows (P2: pass 1 left 71
   `.pdb` and Validated 8; the unchanged pass 2 left 1 and TestStack went red). Three things close it and all are in
   the union; these arms read the outcome. `tleg` restores a row's rewrites after every run, so the first pass keeps
   them and the second restores. Do not move the clone's HEAD between the passes: a two-pass reading means
   something only at ONE head (the informational version carries the head sha).
3. **KNOWN-EXTERNAL.** A real-module package whose Go ORACLE fails on a network or quota text (P's i7 battery:
   x/mod sumdb/tlog, `quota exhausted` from the certificate-transparency log server) reads `EXTERNAL`, with the
   test and the text quoted. It is not a mover and it is NEVER a pass: re-read that module alone when the network
   answers and post both lines. A C#-side failure with such a text is a FAIL. **The driver EXITS 7** when an
   EXTERNAL verdict is all that stands (0 movers, 0 freshness fails), and its `END` and `LINUX LEGS DONE` lines carry
   `external=<n>`: **exit 7 = a re-read is owed, post both lines**. It is not exit 0 and it is never posted as green
   (before review round 1 this read exit 0 with `movers=0`, the one line a lane posts).
4. **The module legs build both sides with the `safe` tag** (g-module-safe-tag; the converter does it for the
   conversion and for its own go test baseline). PREDICTED: no count of x/sync or x/mod moves (neither names the tag
   in any build constraint, read from the i7's copies). A count that moves is a mover, not the tag.
5. **PUBSYM.** A -tests publish refuses a host that lacks a dependency symbol file. The driver counts that text
   over every log of the run: EXPECT 0; any is a MOVER.
6. **Address pairs.** A comparison record states `addressPairs` when the pairing re-keyed anything. A non-zero
   `nToN` at an unmoved count is a line to post; an address-shaped name left on one side is a mover.
7. **The rewrites carry one large class.** The carrier renders a no-inline method as a partial method and
   regenerated no committed test source: every `tests-*.srcdiff.patch` will show `[MethodImpl(...NoInlining)]
   internal static void f(` becoming `internal static partial void f(`. That is expected. Anything else in a
   srcdiff is what to post. (The landing refresh commits the WINDOWS emission; your rewrites are never its input.)

## The KNOWN classes of these boxes (each with its control; anything else is a mover)

- **os/exec 86 + 2 on P1's box** (against 87 + 1): the known mover of that box.
- **semaphore TestWeightedAcquire**: the timing class on a loaded box (KNOWN 7 of 8; 8 reads CLEARED).
- **x/mod zip under load**: at P it hit its 2 m package timeout with TestVCS in flight at load 5 to 6.5 on 4 cores; a
  hand re-read of the same host alone on a quiet box validated. Post it WITH that re-read, as load.
- **GolibTests EventPipeSamplerTests.AGoHogIsSampledAsItsSyntheticPCAndThinnedToTheRate** sits on its floor on a
  4-core box (it failed 2 of 10 alone at P's union AND at its base): post it with a control run at the base.
- **As uid 0**, a Go test that panics ends Go's binary while the converted host records the failure and continues: a
  known difference of the host, stated, not a mover of the union.

## What to post

One post per lane when the driver prints `LINUX LEGS DONE`: the driver's exit status and its `END` line (0 = green;
4 = a mover or a freshness fail; **7 = green and a re-read of one module alone is owed on your box**: the status and
the `external=<n>` on both lines say so); legs, movers by name with each control; GolibTests totals per
configuration; T2's line and (P1) xsync2's line with its floor; PUBSYM; any `EXTERNAL` line verbatim WITH its re-read
(an exit 7 with no re-read line is an unfinished post); LB and LX counts; the box (uid, nproc, free space, SDK,
GOROOT as printed).
