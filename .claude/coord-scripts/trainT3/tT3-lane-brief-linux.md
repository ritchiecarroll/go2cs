# TRAIN T3: the linux legs (P1 and P2)

**T3 (2026-10-10), READ THIS FIRST; it leads every paragraph below.** TRAIN T3 is NOT the face lift and NOT a corpus
re-conversion: 24 rows (C1's darwin executable path, argv0, crc32 arm64, a disclosure retirement, the CNR alias-drift
entries and Phase 5 stubs; C2's module manifest rows, the nugetgo cuts 1-8 and pack README seat, the go.* README seat;
G's trim stages 3a to 3c-2b(iii) and the trim plan), base master `f43e0a2f4e` (`4e6322d770` plus the LICENSE rewrap). The union is pushed as
`claude/coord-trainT3-union` (COORD names the SHA in the GO: the T3 fixup, or a `fixup-N: TRAIN T3` on top). What the
FL text below says about the face lift does NOT apply: the face lift landed with TRAIN FL (refresh `0574b8336c`), the
face-lift -tests classes are OFF in this kit (`tT3-helpers.py` FL_CLASSES), and NO T3 row changes -stdlib or -tests
emission, so your sweeps are PREDICTED to rewrite ~0 committed test sources beyond the known line-ending / standing
OTHER set; any content rewrite is read BY NAME and posted. The previous train's record is FL's (your FL shard patch is
the TE baseline). The GolibTests classes T3 adds are G's trim guards: TrimStage3aTests, TrimStage3c1Tests,
TrimStage3c2aTests, TrimStage3c2bTests, GoZeroConstructionGuardTests (GoZeroResidualTests, SliceHeaderReinterpretTests
and CallerFrameTestVariantNamingTests changed); CNR N at the union = 871 (878 enumerated, 7 platform skips on windows;
on linux read your own enumeration); row 5 adds OsGetpagesize and SyscallKeystonePulls to CNR's documented alias-drift
set (OsGetpagesize's golden is the WINDOWS emission at row 1's tip, so a linux transpile is the drifting side).


**FL (2026-10-08), read this first.** DERIVED from TRAIN Q's brief (`trainQ/tQ-lane-brief-linux.md`), names only, plus:
(FL's text, superseded by the T3 block above:) TRAIN FL was the face lift (rows 1-10 hand-merged on FL's union, row 10 the ONE regeneration of the
-stdlib corpus on three targets and the goldens; rows 11 and 12 merged by COORD's assembler before the fixup). Your
sweeps rewrite the committed test sources in bulk (the face lift's classes): count and post them as before; a linux
lane's rewrites are never the refresh's input. The base is TRAIN Q's landed master line and COORD's docs commits
after it (`541766413e` at the derive). The single-volume `testing` reading P1 took at Q (green) is the control for the
i7's ruled divergence; nothing is owed for it at FL unless COORD's GO asks. Every -recurse module -tests row you hold
(LM: x/sync, x/sync2, x/mod) is read at the FIXUP head as soon as the GO names it, not after the battery (FL10).
The union's first-parent line may hold one `refresh: TRAIN T3` between the fixup and a `fixup-N` (FL2): the driver
admits it.

From COORD. DERIVED 2026-10-06 from TRAIN P's brief (`trainP/tP-lane-brief-linux.md`; at P's union + fixup-2 P1 read
43 legs with two explained movers, P2 exit 0 with 0 movers). **Q is an ordinary train**: whether a release follows
is decided at its landing; the train after it re-converts the corpus, so these are the linux readings of the tree
that becomes that base. **Readings only**: nothing to cut, commit or push. Start when COORD's GO names the SHAs.

| | |
|---|---|
| Union ref / SHA | `claude/coord-trainT3-union` / `38f1ce2510fc9b04f849afff027942d96ab92897` |
| Base | `f43e0a2f4e4a3d6b5891ad9430cb7bffe811a5e9` = TRAIN P's landed master line (the driver refuses a commit of the union's line that is not a Q seat merge or a Q fixup above it) |
| Driver, helper, seat list | `tT3-linux-legs.sh`, `tT3-helpers.py`, `tT3-seats-draft.txt` from `.claude/coord-scripts/trainT3/` on `claude/coord-handover` at `<HND: filled in by COORD>`, all from ONE fetch, in one folder outside the clone |
| Second-run row | `TWOPASS_ROW`, default `runtime/debug` (P2's control row at P) |

## Launch (P's form)

```
LANE=P1 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/tT3/tT3-linux-legs.sh
LANE=P2 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/tT3/tT3-linux-legs.sh
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
| P2, still out at review round 1 | the linux full behavioral for g-r2m-caller-closure-v2, with attribution re-reads (dispatched 50738bd2a0; the row is ACCEPTED since 14:59 10-06: a reading, never its condition) | as dispatched; not a leg of the driver |

## What changed since P's legs

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
