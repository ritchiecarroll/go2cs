# Row 7's R7-3: the closure twinned against a twinned standard library (record)

**G, 2026-11-23.** Row 6 of the M:N .5 PLAN (inbox/COORD 20261123T153840Z-G): R7-3 of DESIGN-mn-stdlib-twins.md,
section 3, the input to row 10's T4. Predictions were written before any build. The kit is
C:\go2cs\scratch\mn2\n5\r73 (prediction.md, convert.sh, arm.sh, run.sh, attr.sh), adapted from 7a's closure arms.

## Setup

- **Program:** src/tools/mnspike2/MnHttpClosure, converted fresh by the tree's converter (-recurse, the output
  positional given). Its closure is 185 packages.
- **Runs:** 3 per arm, at GOMAXPROCS=12, GO2CS_MN=1 and GO2CS_MN_READING=1, against Go's 9 lines.
- **Arms:**
  - P: the program twinned, the standard library plain;
  - S: GoTwinsFor = every closure package plus the program (row 6's arm S, like for like);
  - T: the shipping switch, `-p:GoStdlibTwins=true` plus the program.
- **Trees:** train 6 + refblock, the local merge 634bed7300 (never pushed), for P, S and T. Master 27eabd6619 (train 6
  landed, no refblock) for S again: the attribution arm. The converter is byte-identical at both, so the converted
  program is reused.
- **Baseline of record:** row 6's arm S, 2026-11-21 ~20:18, g-mn4 on row 6's branch (about d0330a86ac; the exact SHA
  was not logged): peak-os-threads 38/39/39, steps 74, parks 36, handoffs 156-160, thread-blocks 0.

## Readings

| arm | tree | output | peak OS threads | steps | parks | handoffs | thread-blocks |
|---|---|---|---|---|---|---|---|
| P | train 6 + refblock | EQUAL x3 | 54-55 | 8 | 0 | 64-65 | 128-129 |
| S | train 6 + refblock | EQUAL x3 | 56-57 | 20 | 0 | 106-108 | 71-81 |
| T | train 6 + refblock | EQUAL x3 | 56 | 20 | 0 | 106-107 | 73-87 |
| S | master 27eabd6619 | EQUAL x3 | 56-57 | 20 | 0 | 106-107 | 77-79 |

Builds: rc 0 in every arm, with GO2CS0017, GO2CS0018 and GO2CS0021 all 0. S and T each wrote 5,079 twins.

## Scores

- R1 (EQUAL to Go, rc 0, program twinned): **MET**, all 12 runs.
- R2 (P: 34-42 threads, handoffs over 100): **MISSED**. 54-55 threads, 64-65 handoffs, 128-129 thread-blocks.
- R3 = R7-3 (S and T: 30 threads or fewer, handoffs 60 or fewer): **MISSED**. 56-57 threads, about 107 handoffs.
- R4 (T within 3 threads of S): **MET**: 56 against 56-57.
- R5 (GO2CS0017, 0018 and 0021 all 0 in S and T): **MET**.

## What the reading says

The twinned closure no longer parks. `parks=0` and `steps=20` in S and T, where row 6's S read 36 and 74, and
`thread-blocks` (a non-multiplexed goroutine blocking its own thread) went from 0 to about 80. Its goroutines run as
threads, not on carriers, so peak threads rose from 38-39 to 56-57.

It is NOT refblock: master without refblock reads the same. It is NOT golib's park path: MnMixedStorm, built on the
same tree today, reads 3,014 parks, 29 peak threads and 0 thread-blocks.

So something between row 6's tree (2026-11-21) and train 6 stopped the closure's goroutines from being multiplexed.
Its emission still names twins: main awaits channel twins and starts goroutines with `goǃtwin`. The cause is **not
read** here. The good endpoint is row 6's branch of 11-21, and the bad endpoint is master 27eabd6619. A bisect over
the trains between them, with this kit's arm S per point (about 8 minutes a build), names the change. T4 (25% fewer
threads) cannot be read until it is found.
