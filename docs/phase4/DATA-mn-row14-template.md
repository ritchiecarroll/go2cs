# M:N row 14: N2's timed arms (template, to fill on a .NET box)

Row 14 as G handed it (215035Z): MnHttpClosure with its closure twinned, timed under the scheduler, the shipping
default against arm N, with an A/A pair, scored against the box-twin record's predictions **as written**. The kit is
`src/tools/mnspike2/run-row14-arms.sh`; it fills the readings table below and prints the scoring lines. C1 wrote the
kit on a Go-only box and could not run it (no .NET SDK there); the dry run against stubbed `dotnet`, `go` and closure
script is described at the end.

**Do not re-predict.** The predictions are DESIGN-mn-box-twin.md section 12 as revised by section 12.1 (12.1 says it
supersedes 12 where they differ). Name the tree and the box, and score.

## The box and the tree (fill in)

| item | value |
|---|---|
| box (name, CPU, cores, RAM) | |
| OS, .NET SDK, Go | |
| tree SHA (a clone at a committed SHA, nothing edited while the arms run) | |
| GOMAXPROCS (12 unless the box has fewer cores; then say so) | |
| free disk before arm d1 (floor 25 GB) | |
| anything else building on the box during the arms (must be nothing) | |

## The command

From the root of the clone, with `go` and `dotnet` on PATH, launched so it survives the session (the readings take
three conversions, three Release builds of the twinned closure and 15 runs):

```bash
ROW14_TWIN_PROPS='-p:GoStdlibTwins=true' bash src/tools/mnspike2/run-row14-arms.sh <results-root>
```

`ROW14_TWIN_PROPS` is how the closure is twinned. The value above twins every package of the closure through the
standard library switch, as arm S was ("every closure package"). It is on the command line because no tracked
script may spell that switch turned on (repoguard `TestStdlibTwinsDefaultOff`). `ROW14_TWIN_PROPS=none` twins only
the program and is not row 14.

The script sets `GOMAXPROCS=12` and `Q1_CLOSURE_RUNS=5` unless they are already set, and each run carries
`GO2CS_MN=1 GO2CS_MN_READING=1` (run-mn-closure.sh). It refuses to start when `GO2CS_MN`, `GoTwinsFor` or the twin
switch is already in the environment.

## The arms (in the order they run)

| arm | MSBuild properties added to the twin properties | role |
|---|---|---|
| d1 | none | the shipping default (byref-param on): the scored arm |
| n1 | `-p:GoTwinsArm=true -p:GoTwinsBoxParam=false` | arm N, the base 12.1's predictions are stated against |
| d2 | none | the shipping default again, into its own folder: with d1, the A/A pair |

Before each arm the script deletes `bin`, `obj` and `Generated` under the clone's `src/core` (a twinned build
rewrites them in place) and the arm's output folder, checks the disk floor, and touches a stamp so the arm's twin
reports can be told from older ones. The arm folders are all two characters long, so the go.*.dll byte totals
compare under the PDB-path rule (12.1: go.*.dll embeds each PDB path).

## The reading lines it collects

Per run, from run-mn-closure.sh's `summary.txt`:

```
READING closure run<r> exit=<rc> EQUAL|DIFFER wall=<ms>ms | READING go-scheduler ... peak-os-threads=<n> ... handoffs=<n> thread-blocks=<n> ...
```

- **PB-1's total** is `handoffs + thread-blocks` per run; the table gives the five and their median.
- **Peak OS threads** is `peak-os-threads`; the table gives the five and their maximum.
- **Wall** is the run's wall clock in milliseconds, measured around the run including the deadline helper's 1 s
  poll, so it is coarse (about +-1 s). The A/A pair bounds it. (`wall=` was added to run-mn-closure.sh for this row.)
- **EQUAL** means the run's stdout, READING lines removed, equals `go run` of the program on the box (Go's 9 lines).
  Any DIFFER or HUNG run fails the arm and the script stops: PB-6 is then MISSED.

Per arm:

- **PB-4's bytes** are the go.*.dll files beside `MnHttpClosure.dll`, summed.
- **The twin reports** (host-independent; PB-2, PB-3 and PB-5 read them): every `go2cs.twins.report.g.cs` newer than
  the arm's stamp, under `src/core` and the arm's output, with its `// candidates=`, `// refused`, `// box:` lines
  kept in `<arm>/reports.txt`. The table sums `twinned=`, `refused byref-param=` and the `// box:` lines' `plain=`.

## Readings (paste `row14-readings.md`'s table here)

| arm | total per run (handoffs + thread-blocks) | total, median | thread-blocks, max | peak OS threads per run | peak, max | wall ms per run | wall, median | go.*.dll | bytes | reports | twinned | refused byref-param | box plain |
|---|---|---:|---:|---|---:|---|---:|---:|---:|---:|---:|---:|---:|
| d1 | | | | | | | | | | | | | |
| n1 | | | | | | | | | | | | | |
| d2 | | | | | | | | | | | | | |

## Scoring, against the record as written

**Superseded by the dated block "Amendment 2026-11-27" at the end of this file** (scoring and the bytes). The text below
is kept as it was first posted.

12.1's revised predictions, scored on the default arm against arm N **on this box** (12.1: "on another host, read
arms O and N first ... and restate PB-1 relative to that N"; row 14 reads N, not O):

| line | as written (12.1) | d1 | d2 |
|---|---|---|---|
| PB-1 (host-sensitive, the headline) | the plain-block total's median falls **4-21%** from N; thread-blocks stay **0**; peak OS threads **at most N's** | | |
| PB-4 (host- and path-sensitive) | go.*.dll **+0.05 to +0.35 MB** over N | | |
| PB-6 (independent) | Go's 9 lines in **every** closure run | | |
| A/A | d1 against d2: the spread of the total's median and of the bytes | | |

Section 12 as originally written, for G's box only: the total's median **105-128** (from about 133) and peak OS
threads **at most 39 in every run**. Quote those lines only when the box is G's; on any other box the 12.1 form is
the one that applies.

How to read the scoring lines the script prints:

- A **MISS on one default arm and a MET on the other** is inside the A/A band. Say so and give both; do not pick one.
- **PB-4's unit**: the script scores the byte delta against both readings of "MB" (10^6 and 2^20 bytes) and says so
  when they disagree. Give the bytes.
- **Not read by row 14**: SB-3 needs the plain arm P (no twins); PB-2, PB-3 and PB-5 are host-independent and come
  from the report columns, for the reader. PB-7 and PB-8 (rules and probe rows) are not this row's.

## How the kit was checked (C1, on a Go-only box)

`bash -n` on both scripts. The driver ran end to end against a stub `run-mn-closure.sh` and stub `dotnet` and `go`
that wrote fixed READING lines and dll sizes: the table, medians, maxima, A/A line and scoring lines came out as
computed by hand, and the stub `src/core` obj folder was deleted by the purge. Negative controls: a default arm
reading above N scored PB-1 MISSED and a 100-byte delta PB-4 MISSED; a failing closure run stopped the driver at
arm d1 with exit 1; a disk floor above the free space refused before any arm; an unset `ROW14_TWIN_PROPS` refused.
Nothing in this kit has been built or run against .NET.

## Amendment 2026-11-27 (COORD, from G's first run of row 14 with the kit at `3300b095b9`)

G ran row 14 with this kit: every run completed, 15 of 15 EQUAL to Go. G scored it by hand, because the kit's own
scoring was defective in two places. Both are fixed on `claude/c1-mn-row14-16c-kit` and this block replaces the scoring
section above.

**1. The bytes read 0.** The kit looked for the assembly under `out/src/MnHttpClosure/bin`, while the build writes under
the output root's `.artifacts/bin/<hash>/…`. `dirname ""` is `.`, so the folder guard passed and the kit summed the
working directory. Now `run-mn-closure.sh` records the assembly MSBuild resolves (`-getProperty:TargetPath`) as an
`assembly:` line in each arm's `summary.txt`, and the kit sums the go.*.dll beside that file. The arm stops by name when
the line is missing, the recorded path is not a file, or no go.*.dll sits beside it. Red first: the old logic against an
empty arm folder passed its guard with the folder `.` and read 0 bytes; the new reader fails on all three cases and reads
the bytes from an `.artifacts` layout.

**2. PB-1 and PB-4 were scored relative to this tree's n1, but n1 is no longer section 12's arm N** (the tree's twin set
has moved since). The kit now scores section 12's clauses as written:

| line | as written (section 12) | scored against |
|---|---|---|
| PB-1 | the plain-block total's median **105-128**; thread-blocks **0** in every run; peak OS threads **at most 39** in every run | each default arm's own runs |
| PB-4 | go.*.dll **+0.05 to +0.35 MB** over arm N | section 12's arm N as recorded: **83,961,344** bytes (DATA-mn-n2-receivers.md). Path-length and SDK sensitive (12.1's PDB-path rule): off G's box, read it as reported |
| PB-6 | Go's 9 lines in **every** closure run | every run |

The readings file adds a **population table**: section 12's arm N as recorded (twinned 3,806; refused parkers 175;
byref-param 63; named-plain 24) beside this tree's n1, d1 and d2, read from the twin reports. It shows twinned, refused
(the sum of each `// refused <reason>=N`), refused parkers (the sum of `blocked=`), byref-param refused and parking, and
box named-plain. A population shift is then visible before any line reads as a miss. 12.1's n1-relative forms (a fall of
4-21% from N, threads at most N's, bytes over N) are still printed, marked REPORTED and not scored.

