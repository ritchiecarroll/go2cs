# M:N row 16b: T1 (size) and T2 (build) on the standard library (template, to fill on the i7)

Row 16b as G handed it (inbox/C2 20261126T215051Z-G), scored against DESIGN-mn-stdlib-twins.md section 4's **T1**
and **T2**. The kit is `src/tests/Performance/run-row16b-stdlib.ps1`.

The arms are three clean builds of `src/go2cs-stdlib.slnx` on ONE tree: plain, plain again (the A/A pair), then
twinned. T1 and T2 are both read from those same three builds, so no build is reused and none is extra. T2 is the build
wall itself, and its A/A pair has to be two real clean builds. The bytes come with each build at no extra cost.

**What has run, and where.** C2 wrote the kit on a Linux cloud box (.NET SDK 10.0.404, pwsh 7, 4 cores, 8 GB free
disk), which cannot hold three full stdlib builds.

Run there:

- the parser over the script;
- `-DryRun`, which reads 363 stdlib members from the solution;
- a rehearsal of all three arms on ONE package's closure (`-Solution src/core/fmt/fmt.csproj`, windows flavour). See
  "The rehearsal".

**Not run anywhere:** the full `go2cs-stdlib.slnx` arms, and Windows. COORD's i7 run is the first of each.

## The box and the tree (fill in)

| item | value |
|---|---|
| box (name, CPU, cores, RAM) | i7, |
| OS, .NET SDK | Windows, 10.0.400 |
| tree SHA (a clone at master `4a56f17a3a`, nothing edited while the arms run) | |
| free disk before each arm (floor 25 GB; the script refuses below it) | |
| anything else building on the box during the arms (must be nothing: T2 is a wall time) | |

## The command (one Windows box)

From `src\tests\Performance` in the clone, in PowerShell. Launch it detached with output to a file, and run
`-DryRun` first:

```powershell
./run-row16b-stdlib.ps1 -TwinArm 'GoStdlibTwins=true' -GoTargetOS windows -OutDir <results>\windows -DryRun
./run-row16b-stdlib.ps1 -TwinArm 'GoStdlibTwins=true' -GoTargetOS windows -OutDir <results>\windows
```

- `-TwinArm` names the environment variable set for the twins arm's build only. MSBuild reads it as a property in every
  child build. It is an argument because no tracked script may spell the switch turned on (repoguard
  `TestStdlibTwinsDefaultOff`). Never set it in a tracked file.
- The script refuses to start if `GO2CS_MN`, `GoTwinsFor`, `go2csPath` or the twin variable is already set.
- Linux and darwin, as the box allows: the same line with `-GoTargetOS linux`, then `darwin`, each into its own
  `-OutDir`. Each arm purges, so nothing carries across a target switch (a `GoTargetOS` switch changes the compile item
  set, not the timestamps). G's brief: read windows, then name any target not read.
- Expected wall: the i7 built `go2cs-stdlib.slnx` `--no-incremental` in 516 s (2026-08-14). With twins at 1.5-1.8x,
  three arms come to about 35-45 minutes per target.

## The arms (in the order they run)

| arm | environment of the build | role |
|---|---|---|
| plainA | twin variable unset | the plain standard library |
| plainB | twin variable unset | the same again. With plainA, the wall A/A band and the byte band |
| twins | twin variable set, from `-TwinArm` | the standard library twinned |

Per arm:

1. Check the disk floor. Delete `bin`, `obj` and `Generated` under `src/core` and `src/gen` (the analyzer is rebuilt in
   every arm, the same in all three).
2. Run `dotnet build ..\..\go2cs-stdlib.slnx -c Release -p:GoTargetOS=<os> --no-incremental
   -p:UseSharedCompilation=false -nodeReuse:false`, timed from process start to exit, with output in
   `<arm>.build.log`.
3. Measure each core/ member of the solution, by its OWN output: `<project dir>\bin\Release\<tfm>\<AssemblyName>.dll`.
   A copy in another project's bin is never counted. Record its bytes and its count of twin method names (the
   generator's suffix, U+A78F then `twin`).
4. **Assert the arm is what the flag says**, the 16c fold:
   - plainA's twin-name count is the baseline (a plain stdlib carries a few hand-written park-point twins: `sync`'s
     `Lock`, `time`'s `Sleep`);
   - plainB must equal it, and the twins arm must exceed it;
   - every arm must produce the same set of assemblies.

   Otherwise the arm stops by name.
5. Count GO2CS0017, GO2CS0018 and GO2CS0021 in the build log. This is a gate: 0 in every arm.
6. Write `<arm>.json`, with every assembly's bytes.

Re-score saved arms with `./run-row16b-stdlib.ps1 -ScoreFrom <results>\windows`. It builds nothing.

## Readings (paste each `row16b-<os>-readings.md` here)

### windows

| threshold | plainA | plainB | A/A band | twins | twins / plain mean | bar | verdict |
|---|---:|---:|---:|---:|---:|---|---|
| T1 size, summed go.*.dll bytes | | | | | | at most +8% | |
| T2 build wall, seconds | | | | | | at most 1.8x beyond A/A | |

The ten assemblies that grew most:

Gate GO2CS0017/0018/0021 per arm:

### linux, darwin

(the same tables, or "not read on this box", named)

## Scoring, against section 4 as written

- **T1:** "the stdlib packages' assemblies, summed, with twins are at most **+8%** over the same tree plain".
  - MISSED when twins bytes / mean of the two plain byte sums - 1 > 0.08.
  - Bytes have no noise to subtract. A nonzero byte A/A band means the build is not deterministic: the script reports
    it as a FINDING before T1 is read.
  - Expected, per section 4: about +6.6% (stage (c) +5.4-6.1%, 7a's forwarders +0.5%).
- **T2:** "the `-stdlib` corpus build, clean and uncontended, is at most **1.8x** plain", and section 4 says "every
  time threshold is beyond the A/A band".
  - MISSED when twins wall / mean of the two plain walls, less the wall A/A band, exceeds 1.8.
  - G: this is C2's reading of "beyond the A/A band" for a ratio bar. If you meant otherwise, re-score with
    `-ScoreFrom`. The arm files carry the raw walls.
  - Expected: 1.5-1.8x (stage (c), on the closure).
- The twin-warning gate is separate from T1 and T2.
- Exits: 0 met; 2 a miss, or the gate failed; 1 the run itself stopped (a refusal, a failed build, or a failed
  assertion).

A miss on T1-T5 means row 7 does not ship in .5, and it stops for the owner. Report the threshold that missed, with
its numbers, and stop there.

## The rehearsal (C2's box)

2026-11-27, C2. Setup:

- tree `4a56f17a3a`;
- Linux x64, 4 cores, SDK 10.0.404, a shared cloud container;
- `-Solution src/core/fmt/fmt.csproj -GoTargetOS windows -TwinArm` (the switch on).

It built `fmt`'s closure: 65 stdlib assemblies, not the 363-member solution. **It is not row 16b.**

**What the mechanics did:**

- The purge and the clean `--no-incremental` build ran in every arm.
- The same 65 assemblies came out of every arm.
- Twin names: plainA 17, plainB 17, twins 6,298. The assertion passed.
- The two plain builds were byte-identical (byte band 0). The build is deterministic here.
- golib was unchanged at 1,412,608 bytes in all three arms.
- GO2CS0017/0018/0021 were 0/0/0 in every arm.
- It exited 2, with REHEARSAL printed.

| | plainA | plainB | A/A band | twins | twins / plain mean |
|---|---:|---:|---:|---:|---:|
| summed bytes, 65 assemblies | 30,507,520 | 30,507,520 | 0.00% | 33,414,144 | **1.0953 (+9.53%)** |
| build wall, s | 291 | 260 | 10.92% | 418 | 1.516x (1.407x less the band) |

The assemblies that grew most:

| assembly | growth | bytes added |
|---|---:|---:|
| go.runtime | +38.3% | +1.53 MB |
| go.os | +52.3% | +0.35 MB |
| go.reflect | +19.7% | +0.18 MB |
| go.internal.poll | +36.4% | +0.17 MB |
| go.time | +25.4% | +0.15 MB |
| go.fmt | +21.5% | +0.10 MB |

**This is a signal for row 16b, not a reading of T1.** On this closure, twins add 9.53% to the summed bytes. That is
above T1's +8% bar and above section 4's expected +6.6% (stage (c)'s +5.4-6.1% plus 7a's +0.5%).

What I can say about it:

- **The closure is runtime-heavy.** `go.runtime` alone is 4.0 MB of 30.5 MB and grew 38%. `fmt`'s closure is the part
  of the library where goroutines park: os, poll, time, sync.
- **The full solution adds about 300 packages this closure does not hold** (crypto, encoding, go/*, compress, text,
  and others). Whether they dilute the ratio below +8% depends on how many of their functions can park. This
  rehearsal does not measure that.
- **Twin coverage may have grown since stage (c)** (N2's box twins, N3's pair calls, the func-value fix). Whether it
  did is NOT MEASURED here.
- The wall ratio, 1.52x (1.41x less a 10.9% band on a shared 4-core box), sits inside stage (c)'s 1.5-1.8x.

The full-solution arms on the i7 are what T1 is scored on.

## Related

- Row 16c, the consumer legs of T3: `src/tests/Performance/run-row16c-consumers.ps1` on
  `claude/c2-mn-row16c-consumers` at `33214bba5e`. COORD's three 16c folds are in it:
  - `-PublishArgs`;
  - the twinned-build assertion, which this kit shares;
  - the TypeScript convert rc=1 closure check.
- The Performance suite's leg of T3: C1's `run-row16c-t3.ps1`.
