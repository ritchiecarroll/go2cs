# M:N row 16c: T3 on the Performance suite (template, to fill on a .NET box)

Row 16c as G handed it (215035Z): the Performance suite, every row, JIT and Native AOT, timed against a plain
standard library and a twinned one on ONE tree, with an A/A pair of the plain arm first, scored against
DESIGN-mn-stdlib-twins.md section 4's **T3**. The kit is `src/tests/Performance/run-row16c-t3.ps1`. C1 wrote it on a
Go-only box with no PowerShell and no .NET SDK, so the script has **not been parsed or run**; the first run is also
its first parse. Rows 16a, 16b and 16d and the sqlite-cs and TypeScript-go-cs T3 legs are other lanes' (G and C2).

## The box and the tree (fill in)

| item | value |
|---|---|
| box (name, CPU, cores, RAM) | |
| OS, .NET SDK, Go, MSVC linker (Windows AOT) | |
| tree SHA (a clone at a committed SHA, nothing edited while the arms run) | |
| free disk before each arm (floor 30 GB; the script refuses below it) | |
| anything else building or timing on the box during the arms (must be nothing: AOT publishes run one at a time and only a quiet box's Measure numbers count) | |

## The command

From `src/tests/Performance` in the clone, launched detached with output to a file, so it survives the session (three
full suite runs, each with every AOT publish; on the i7 class each publish is about 25 minutes, so this is many
hours):

```powershell
./run-row16c-t3.ps1 -TwinArm 'GoStdlibTwins=true' -OutDir <results-dir>
```

`-TwinArm` names the environment variable set for the twins arm only, and removed after it. MSBuild reads an
environment variable as a property, so every build in that arm, including each consumer's closure, builds the
standard library with twins while the consumers stay plain. It is an argument because no tracked script may spell
that switch turned on (repoguard `TestStdlibTwinsDefaultOff`). Never set it in a tracked file.

The script refuses to start when `GO2CS_MN`, `GoTwinsFor`, `GOMAXPROCS` or the named variable is already set: the
consumers must not opt in, the scheduler must be off, and the runner's own headline guard refuses `GOMAXPROCS`.

## The arms (in the order they run)

| arm | environment | role |
|---|---|---|
| plainA | the twin variable unset | the plain standard library |
| plainB | the twin variable unset | the same again: with plainA, the A/A band (the noise floor) |
| twins | the twin variable set, from `-TwinArm` | the standard library twinned; the consumers plain |

Before each arm the script deletes `bin`, `obj` and `Generated` under the clone's `src/core` and
`src/tests/Performance` (a twinned build rewrites `src/core` in place, and the AOT publish-reuse stamp lives in `bin`,
so a stale publish cannot be reused across arms), checks the disk floor, and runs `run-performance.ps1 --runs 5` in
its own PowerShell process with stdout and stderr in `<arm>.log` and `<arm>.stderr.log`. It never passes
`--update-readme`. After each arm it lists any tracked file that changed (the suite mirrors README.md into
docs/Performance.md; with no README update the mirror should not move it).

## The reading lines it collects

- From each arm's log, the **Execution time** table the runner prints (milliseconds, the median of its runs after one
  discarded warmup; the Startup row is process wall time): the JIT and Native AOT cells of every row. The milliseconds
  are kept as the suite reports them; divide by 1,000 for seconds.
- From each arm's build output, before the next purge: every `Perf*/bin/Release/aot/<Perf...>` executable's size in
  bytes.

## Readings (paste `row16c-readings.md`'s tables here)

Execution time, milliseconds:

| Benchmark | leg | plainA | plainB | A/A band | twins | twins / plain mean | beyond A/A | T3 (+3%) |
|---|---|---:|---:|---:|---:|---:|---:|---|
| | | | | | | | | |

AOT executable bytes:

| Benchmark | plainA | plainB | twins | twins / plain mean | T3 (+1%) |
|---|---:|---:|---:|---:|---|
| | | | | | |

## Scoring, against section 4 as written

T3: "A consumer that does NOT opt in, built against the twinned stdlib: every Performance-suite row, sqlite-cs and
TypeScript-go-cs within **+3%** of the plain stdlib (beyond A/A), JIT and AOT; its AOT executable at most **+1%**
larger."

How the script computes it, per row and per leg:

- plain mean = (plainA + plainB) / 2; A/A band = |plainA - plainB| / plain mean.
- beyond A/A = twins / plain mean - 1 - A/A band. **MISSED** when it exceeds 0.03.
- AOT executable: **MISSED** when twins / plain mean of the two plain sizes exceeds 1.01.
- A row a run did not produce reads **NOT MEASURED**, which is not a pass.

On any MISS the script names every missing row and exits 2. Section 4: a miss on T1-T5 means row 7 does not ship in
.5, and **a T3 miss stops for the owner**: report the rows that missed and stop there.

## Not covered here

- T3's sqlite-cs and TypeScript-go-cs legs (C2), and T1, T2, T4, T5 (16a, 16d and the verdict are G's).
- A rehearsal (`-Filter` or `-NoAot`) is marked REHEARSAL in the readings and is not row 16c.
