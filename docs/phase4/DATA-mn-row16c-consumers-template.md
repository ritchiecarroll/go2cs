# M:N row 16c: T3 on sqlite-cs and TypeScript-go-cs (template, to fill on R-LAPTOP)

Row 16c as G handed it (inbox/C2 20261126T215051Z-G): sqlite-cs and TypeScript-go-cs, each built against a plain
standard library and against a twinned one on ONE tree, JIT and Native AOT, each project's own timed workload, an
A/A pair of the plain arm first, scored against DESIGN-mn-stdlib-twins.md section 4's **T3**. The kit is
`src/tests/Performance/run-row16c-consumers.ps1`, shaped after C1's `run-row16c-t3.ps1` (the Performance suite's leg,
DATA-mn-row16c-template.md), with the same arithmetic. One invocation is one consumer; run sqlite-cs first (minutes),
then TypeScript-go-cs (hours).

**What has run, and where.** C2 wrote the kit on a Linux cloud box (.NET SDK 10.0.404, pwsh 7, Go 1.26.11, 8 GB free
disk, no Windows, no Native AOT toolchain). Run there:

- the parser over the script;
- `-DryRun` on a stub consumer;
- `-ScoreFrom` on four synthetic arm sets: MET, a JIT miss plus a size miss, the warning gate, and `-NoAot`
  (NOT MEASURED). Exits were 0, 2, 2 and 3, as written;
- all three arms end to end, JIT only, on a 63-package hello consumer converted against this tree (section "The
  rehearsal").

**Not run anywhere yet:**

- the AOT leg;
- sqlite-cs;
- TypeScript-go-cs;
- Windows.

The first R-LAPTOP run is the first run of each of those.

## The box and the tree (fill in)

| item | value |
|---|---|
| box (name, CPU, cores, RAM) | R-LAPTOP, |
| OS, .NET SDK, Go, MSVC linker (AOT) | |
| tree SHA (a clone at a committed SHA, nothing edited while the arms run) | |
| consumer SHAs (sqlite-cs, typescript-go-cs) and inputs kit SHA | |
| free disk before each arm (floor 30 GB; the script refuses below it) | |
| anything else building or timing on the box during the arms (must be nothing) | |

## Before the arms: convert each consumer once, against THIS tree

Twins are a build-time property of `src/core`, so one conversion serves all three arms. Convert with Go 1.26.11
(`go version` first: an older local toolchain refuses the consumer's go.mod), and always pass the output directory:

```text
go -C <tree>/src/go2cs build -o <bin>/go2cs .
<bin>/go2cs -recurse -go2cspath <tree>/src <consumer-module-dir> <out>
```

- **sqlite-cs:** `<consumer-module-dir>` is sqlite-cs's `demo/` (DATA-sqlite-cs-demo.md section 3). The project is
  `<out>/src/sqlitecs/demo/sqlitecs.demo.csproj`. Native oracle: `(cd <demo> && go run .) > native-sqlite.txt`
  (the four lines in DATA-sqlite-cs-demo.md section 1).
- **TypeScript-go-cs:** `<consumer-module-dir>` is the module holding `cmd/checkdemo`. The project is the
  `checkdemo` `.csproj` under `<out>/src/` (fill in its path). Inputs: G's kit `claude/g-tsbench-kit` `d5678617c9`,
  units-101, with SHA256SUMS verified. `-ArgsFile` is the units-101 list, one path per line. `-WorkDir` is an empty
  directory, as in R's tables (`ts-prof-r/README.md` on `claude/r-ts-prof-tables`). The native oracle is the 405-line
  stdout, hash `3EB1134A0D5BFF02` there.

Do **not** use `-recurse=nuget`, and do not pass `-p:GoStdLibVersion`. Either builds against the published packages,
so the switch would twin nothing. The script refuses a root whose `go2csPath` pin is not this tree's `src`.

Native AOT on Windows needs "C:/Program Files (x86)/Microsoft Visual Studio/Installer" on PATH (the SDK's `vswhere`
probe). Put it on PATH in the shell that launches the script.

## The commands

From `src/tests/Performance` in the clone. Launch detached, with output to a file, so the run survives the session.
`-DryRun` first, every time:

```powershell
./run-row16c-consumers.ps1 -Leg sqlite-cs -ConsumerRoot <out-sqlite> `
    -Project <out-sqlite>/src/sqlitecs/demo/sqlitecs.demo.csproj `
    -ExpectedStdout <native-sqlite.txt> -Runs 21 -TwinArm 'GoStdlibTwins=true' -OutDir <results>/sqlite -DryRun

./run-row16c-consumers.ps1 -Leg typescript-go-cs -ConsumerRoot <out-ts> -Project <checkdemo.csproj> `
    -ArgsFile <units-101.args> -WorkDir <empty-dir> -ExpectedStdout <native-ts.txt> -Runs 5 `
    -TwinArm 'GoStdlibTwins=true' -OutDir <results>/ts -DryRun
```

Then the same lines without `-DryRun`.

- `-TwinArm` names the environment variable set for the twins arm only, and removed after it. MSBuild reads it as a
  property in every child build, so the arm's `src/core` closure builds twinned while the consumer stays plain. It
  is an argument because no tracked script may spell the switch turned on (repoguard `TestStdlibTwinsDefaultOff`).
  Never set it in a tracked file.
- `-Runs`: sqlite's demo is a sub-second program, so 21 runs. checkdemo is about 100 s a run on R-LAPTOP, so 5. Both
  add one discarded warm-up.
- The script refuses to start when any of these is already set: `GO2CS_MN`, `GoTwinsFor`, `GOMAXPROCS`, `GOGC`,
  `DOTNET_gcServer`, `DOTNET_GCgen0size`, `DOTNET_ROLL_FORWARD`, `go2csPath`, or the twin variable itself. The
  reasons: the consumer must not opt in, the scheduler must be off, the runtime must be the consumer's own
  runtimeconfig, and the pin must hold.

## The arms (in the order they run)

| arm | environment | role |
|---|---|---|
| plainA | twin variable unset | the plain standard library |
| plainB | twin variable unset | the same again. With plainA it gives the A/A band, the noise floor |
| twins | twin variable set, from `-TwinArm` | the standard library twinned; the consumer plain |

Per arm:

1. Check the disk floor, then delete `bin`, `obj`, `Generated` and `.artifacts` under the tree's `src/core` and under
   the consumer root.
2. JIT leg:
   - `dotnet build <project> -c Release -p:GoTargetOS=<os>`, with output in `<arm>.jit-build.log`;
   - read the built assembly's path from MSBuild (`-getProperty:TargetPath`);
   - run `dotnet <dll> <args>` once as a discarded warm-up, then `-Runs` measured times.
3. AOT leg:
   - `dotnet publish <project> -c Release -r <rid> -p:PublishAot=true -p:GoTargetOS=<os> -o <results>/<arm>/aot`.
     The dotnet and ilc processes' CPU is written to `<arm>.aot-cpu.log` every 60 s. A quiet ILC with rising CPU is
     working, not hung. G's brief: about 50 minutes a publish on Windows, quiet for 40+;
   - one warm-up run, then `-Runs` measured runs of the native executable, and its size in bytes.
4. Every run is a .NET `Process` with stdout and stderr redirected and drained, never a shell pipe.
   - Every run's stdout must be byte-equal to the first run's, across all arms, and to `-ExpectedStdout`. Otherwise
     the arm stops: a twinned stdlib that changes a consumer's output is not a timing question.
   - Both build logs are counted for GO2CS0017, GO2CS0018 and GO2CS0021.
5. Write `<arm>.json`, with every run's wall, the median, the build and publish seconds, the AOT bytes, the warning
   counts and the stdout hash.

Re-score saved arms with `./run-row16c-consumers.ps1 -ScoreFrom <results>/<leg>`. It builds nothing.

## Readings (paste each `row16c-<leg>-readings.md` here)

### sqlite-cs

Workload wall, seconds (median of the measured runs):

| leg | plainA | plainB | A/A band | twins | twins / plain mean | beyond A/A | T3 (+3%) |
|---|---:|---:|---:|---:|---:|---:|---|
| JIT | | | | | | | |
| AOT | | | | | | | |

AOT executable bytes:

| plainA | plainB | twins | twins / plain mean | T3 (+1%) |
|---:|---:|---:|---:|---|
| | | | | |

Gate GO2CS0017/0018/0021 per arm: plainA / plainB / twins = 

### TypeScript-go-cs

Workload wall, seconds:

| leg | plainA | plainB | A/A band | twins | twins / plain mean | beyond A/A | T3 (+3%) |
|---|---:|---:|---:|---:|---:|---:|---|
| JIT | | | | | | | |
| AOT | | | | | | | |

AOT executable bytes:

| plainA | plainB | twins | twins / plain mean | T3 (+1%) |
|---:|---:|---:|---:|---|
| | | | | |

Gate GO2CS0017/0018/0021 per arm: plainA / plainB / twins = 

## Scoring, against section 4 as written

T3: "A consumer that does NOT opt in, built against the twinned stdlib: every Performance-suite row, sqlite-cs and
TypeScript-go-cs within **+3%** of the plain stdlib (beyond A/A), JIT and AOT; its AOT executable at most **+1%**
larger."

Per consumer and per leg, the same arithmetic as C1's script:

- plain mean = (plainA + plainB) / 2;
- A/A band = |plainA - plainB| / plain mean;
- beyond A/A = twins / plain mean - 1 - A/A band. The leg is **MISSED** when this exceeds 0.03;
- AOT executable: **MISSED** when twins / mean of the two plain sizes exceeds 1.01;
- a leg a run did not produce reads **NOT MEASURED**, which is not a pass.

The twin-warning gate is separate from T3. Any GO2CS0017/0018/0021 in any arm reads **GATE FAILED**.

Exits:

- 0: everything MET;
- 2: a T3 miss, or the gate failed;
- 3: something NOT MEASURED;
- 1: the run itself stopped (a refusal, a failed build or publish, or stdout that diverged).

Section 4: a miss on T1-T5 means row 7 does not ship in .5, and **a T3 miss stops for the owner**. Report the leg
that missed, with its seconds, the A/A band and the ratio, and stop there.

**If an AOT publish fails** (the converted checkdemo's AOT history is in DATA-ts-aot-b4.md), the script stops with
exit 1. Re-run with `-NoAot` for the JIT reading and report the AOT leg NOT MEASURED, with the publish log's last
error. Do not work around it inside the run.

## The rehearsal (C2's box, JIT only)

(filled in below from the hello run)

## Not covered here

- The Performance suite's leg (C1, `run-row16c-t3.ps1`).
- T1 and T2 (row 16b), and T4 and T5 (G).
- A run with `-NoAot` or fewer than 3 runs is marked REHEARSAL in the readings and is not row 16c.
