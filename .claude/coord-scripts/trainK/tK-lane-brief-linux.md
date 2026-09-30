# TRAIN K: the linux legs for P1 and P2

From COORD. These are **readings only**: nothing to cut, commit or push. Start when COORD's GO message arrives in your
inbox. The GO fills in the two values below.

| | |
|---|---|
| Union ref | `claude/coord-trainK-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` |
| Base | TRAIN J's landed master `f819887fa3` + TRAIN K's signed seat merges + the TRAIN K fixup (seat list: `.claude/coord-scripts/trainK/tK-seats.txt` on `claude/coord-handover`) |
| Driver | `.claude/coord-scripts/trainK/tK-linux-legs.sh` on `claude/coord-handover` (tip `<HND: filled in by COORD>`) |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`, in the union as seat `c2-ms-dotnet-recipe` 3b8a93ddb3) |

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone must have **no tracked changes**. The driver refuses a dirty clone. It fetches the union and
   checks it out detached, and it refuses when the remote SHA is not the one in the GO.
3. Fetch the driver to a path **outside** the clone:
   `git -C <clone> fetch origin claude/coord-handover && git -C <clone> show FETCH_HEAD:.claude/coord-scripts/trainK/tK-linux-legs.sh > ~/tK-linux-legs.sh`
4. Run it **in the background**. Do not poll inside a turn. Read `SUMMARY.txt` when it exits.
   ```
   LANE=P1 UNION=<union SHA> FIXUP=<fixup SHA> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> bash ~/tK-linux-legs.sh
   LANE=P2 UNION=<union SHA> FIXUP=<fixup SHA> W=<your clone> GOROOT=<same>                                             bash ~/tK-linux-legs.sh
   ```
   `FIXUP` is the TRAIN K fixup commit named in the GO. It is optional, because the driver also refuses a union that
   lacks the fixup on its own (below), but pass it when the GO names it.
   Evidence lands in `~/tK-linux-<LANE>/`: `SUMMARY.txt`, one log per leg, each row's fresh comparison record and
   manifest, and each row's test-source diff.

What the driver sets for you (read it before running it; it is short):
- `GOTOOLCHAIN=local CGO_ENABLED=0`, with `$GOROOT/bin` first on `PATH`. Before exporting it, the driver checks the
  value itself: an absolute path, no backslash, no trailing slash, and `$GOROOT/VERSION` reading go1.24.13 (floor 6).
  Then it aborts unless `go env GOROOT` equals it and `go version` is go1.24.13.
- It refuses a union that lacks the **TRAIN K fixup**: no `fixup: TRAIN K` commit after f819887fa3, or any
  `src/core`/Behavioral/Performance csproj still carrying `<LangVersion>latest</LangVersion>` (the S1 regeneration).
  Without the fixup every row would re-emit its committed test csproj, and you would only see it after the whole run.
- It evals the union's own `use-ms-dotnet.sh`. Your `~/.dotnet-ms` already exists from the switch, so this is
  instant. It runs `dotnet build-server shutdown` and aborts unless `dotnet` resolves to `$DOTNET_ROOT/dotnet`.
- It runs `dotnet --version` in the clone. It aborts if the union's `global.json` (the S1 pin: 10.0.100 with
  latestFeature) cannot resolve your SDK. **That is a finding: post it.**
- **`GoTargetOS` is NOT exported.** The union carries `p1-tests-goos`, so every `-tests` row must build the linux
  flavour on its own. Only the GolibTests commands pass `GoTargetOS=linux` explicitly, as the recipe says.
- Floor 3: every `-tests` call passes the output directory as the second positional. Floor 7: every rc is captured
  before any pipe. Floor 8: build output is purged between heavy legs, with a tracked-deletions check, and the
  converter is **rebuilt after every purge**. TRAIN J run 2 lost its T legs to rc=127 exactly there.
- Each leg has an outer `timeout`, so a hang is killed and stated instead of stalling the box. GolibTests also runs
  with `--blame-hang-timeout 20m`, which names the hanging test.

## 2. Who runs what, and why

**P1** (the uid-0 box):

| Leg | What | Expected at f819887fa3's annotation | Gates (seat) |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14 + 1 disclosed; the record's `targetGOOS` is linux; the host's `syscall.dll` names `PtraceRegs` (the linux flavour; type names live in the DLL's metadata, not the PDB) | `p1-tests-goos` 2051786a8b; also compiles C# 14 under the S1 pin and c2-gen-guard's WarningsAsErrors on linux. **The driver STOPS here if L1 does not validate 14, or its record fails a freshness check.** Post that alone. |
| L1v | the same row under `verify-ms-dotnet.sh` | VERIFY PASS; every libcoreclr path is Microsoft's, the host's bundled copy included | the switch, re-proved on a union-built host |
| L2 | GolibTests **Release and Debug**, the FULL suite, **nothing excluded** | 0 failed | everything golib; see the named arms below |
| L2k | GolibTests Release filtered to the classes K seats added or changed | all pass; list every SKIP by name | the seats below, read by name |
| L3 | `os -tests` | **903 + 2 at uid 0.** The banked `linux: 912 + 2` is a non-root reading; the 9 missing rows are TestFilePermissions' subtests, absent as root (your sizing of 2026-09-30) | `p1-os-withdrawal-gate` 1d09813887 (linux byte-identical: TestRemoveAllWithExecutedProcess is windows-only); D4's host order |
| L4 | `runtime/debug` | 8 + 1 | `p2-created-by` |
| L4 | `testing` | 53 + 15 | `p2-created-by` (test goroutines created by testing.(*T).Run), `i9-d4-go-test-order` (the host's order), `i9-d5-host-lf` (a bare LF on every OS; linux already wrote LF, so expect no mover) |
| L4 | `sync`, `net/http/pprof` | 46 + 6, 15 | `i9-a3-runtime-lock-profile` (it measured 6 = 6 and 0 = 0 on linux) |
| L4 | `reflect`, `fmt`, `internal/fmtsort`, `encoding/json` | 396 + 22, 62 + 1, 3, 532 | `c1-reflect-hash-band` (its linux reading moved 0 verdicts in these four); reflect also `r-field-ptr-equality` and `r-named-slice-reflect-dims` |

**The named arms in L2/L2k.** Report each one as pass, fail or SKIP. A SKIP is a finding for every arm in this list.
- `LinuxDescriptorLimitTests`, including `AManagedThreadStartNeedsDescriptorsGosLimitDenies` (you excluded it at
  master as the known hang; `p2-descriptor-guard` 7c0fca34d7 retires that hang, so it runs now) and
  `TheHeadroomArmIsNotFooledByHolesInTheDescriptorTable`.
- `GoroutineProfileInstantTests`, including `TheSnapshotIsOneInstantAcrossTheSetAndTheLabels`, plus
  `CreatedByPositionTests`, `TestGoroutineCreatorTests` and `TracebackDecorationTests`. These are PRERES-K8's proof set,
  the Goroutine.cs resolution between G's gate and P2's created-by.
- `ThreadStateCensusTests` reads PRERES-K9: both added rows, `t_chunk` and `t_runtimeLockProfilePending`.
- `ExecutionTracerParserTests.GosOwnParserAcceptsAManagedProgramsTrace` must **PASS, not SKIP**: `GOROOT` is the
  pinned go1.24.13 (`p1-tracer-oracle-pin`). Also `ExecutionTracerOracleResolutionTests`, 6/6.
- The rest, one line each: `UnsafeLengthLimitTests` (p1-spanlength-fix), `PrintThroughRuntimeTests` (c1-print-fidelity),
  `ReflectHashTokenBandTests`, `RuntimeCallerPCSpanTests`, `SyntheticPCRegistryTests` (c1-reflect-hash-band),
  `FieldPointerEqualityTests` (r-field-ptr-equality), `FrameSymbolNameTests` (g-pprof-symbol-name),
  `SliceBoundsR1aTests` (i9-r1a-commit2), `GoTestOrderTests` (i9-d4), `EventLineTerminatorTests` (i9-d5),
  `RuntimeLockProfileTests` (i9-a3).

**P2** (the box where the descriptor guard failed order-sensitively):

| Leg | What | Expected | Gates (seat) |
|---|---|---|---|
| L0v | GolibTests Release, `LinuxDescriptorLimitTests` only, under `verify-ms-dotnet.sh` | VERIFY PASS; the class passes | the runtime proof on your box |
| L2 | GolibTests Release, FULL, PLAIN | 0 failed (1445/1/15 at 346b26c81f on Microsoft, before your guard fix) | `p2-descriptor-guard` on the box that found it, plus everything golib |
| L2k | the same class filter as P1's | all pass; SKIPs named | as P1's list |
| L5 | `runtime/pprof -tests`, **twice** | It should validate, and both generic pins should match their disclosures. Prediction from windows (G's gate plus the symbol-name fix: Validated 145, 7 disclosed-divergent, 2 unsupported) and A3's linux reading (7 = 7). `TestGoroutineProfileConcurrency` passes in both runs. | `g-gpc-torn-snapshot` b5db4b2279 (its linux read, owed through you); `g-pprof-symbol-name` 6c1213399e. **Post the figures as the bank candidate's linux annotation, `linux: N + D`.** |
| L6 | `runtime -tests`, the FULL row (150 m per child, 330 m outer; about 90 min on your box) | **10,883 verdicts.** Prediction: **undisclosed errors 0.** Your head's 3 were exactly the TestRuntimeLockMetricsAndProfile family, which `i9-a3` fixes on linux (fail set 77 to 74, base-only exactly the 3 A3 names). TestTracebackParentChildGoroutines PASS. The traceback family (Args, Inlined, System, Systemstack) fails exactly as at your base. | `p2-created-by`, `i9-a3`, `c1-print-fidelity`, `p1-spanlength-fix` (the TestMemmoveOverflow pin holds), `c1-reflect-hash-band` (Callers/FuncForPC), `g-gpc-torn-snapshot`, `i9-d4` (order) |

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

- **The tree.** HEAD equals the union SHA, and f819887fa3 plus each seat this brief reads is an ancestor, and so is the
  TRAIN K fixup (no csproj left at `<LangVersion>latest</LangVersion>`). There were no tracked changes before the run.
- **The toolchain.** Each comparison record's `environment.oracleGoVersion` reads go1.24.13, `configuration` Release,
  `tiered` false. Each manifest's `targetGOOS` is linux.
- **One converter.** `converterRevision` is the same in every row's manifest (the first row's value is kept in
  `converter-revision.txt`), and the driver's `BUILD converter` stamps match after every purge rebuild.
- The driver **enforces** the toolchain and converter checks: a record that fails one prints `FRESHNESS FAIL: <field>`
  under its row, and the row is **not a reading**. The END line counts them (`freshness-fails=<n>`). L1 aborts on one.
- **Fresh record or no reading.** The driver deletes a row's three result files before the row and accepts the
  comparison record only if it is newer than the row's start stamp. `NO FRESH COMPARISON RECORD` means the row did not
  reach its compare, so there is no reading. Read the log tail and never an older record.
- **GolibTests.** Read the `Passed!`/`Failed!` totals line in THIS leg's log. A missing line is an aborted or
  truncated run: no reading. Never read rc alone (fleet rule, 2026-09-30).
- **Test sources.** After each row the driver records which tracked files the `-tests` convert rewrote under
  `src/core/<pkg>`, then restores them.
  - **csproj among them must be 0.** The S1 regeneration of committed test csproj must equal the template's own
    emission; any csproj diff is a fixup finding.
  - Rewritten test `.cs` files are expected on rows that R1-A commit 2 touches. Count them; do not chase them.

## 4. A row that moves

A mover is any of these:
- a row whose `N + D` differs from the expected column;
- any undisclosed error;
- an orphaned disclosure;
- a GolibTests failure, or a skip in the named arms.

For each mover, run a **CONTROL**: the same row at master f819887fa3, in a worktree that is a child of your clone.
```
git -C "$W" worktree add --detach "$W-ctl" f819887fa3
cd "$W-ctl" && (cd src/go2cs && go build -o bin/go2cs .)
GoTargetOS=linux ./src/go2cs/bin/go2cs -tests -test-action all -test-timeout 30m -test-config Release \
  -go2cspath "$W-ctl/src" "$GOROOT/src/<pkg>" "$W-ctl/src/core/<pkg>"     # rc captured before any pipe
```
- `GoTargetOS=linux` goes on the CONTROL only. Master lacks p1-tests-goos, and P1 measured that it compiles the
  windows flavour without it.
- Name the movers by diffing the two records:
  ```
  python3 - union.comparison.json control.comparison.json <<'PY'
  import json, sys
  a, b = (json.load(open(p)) for p in sys.argv[1:3])
  for side in ("go", "csharp"):
      x, y = a.get(side) or {}, b.get(side) or {}
      for k in sorted(set(x) | set(y)):
          if x.get(k) != y.get(k): print(side, k, "control:", y.get(k), "union:", x.get(k))
  PY
  ```
- Then remove the control, but only after checking it holds no tracked change. It always holds untracked build output
  and a comparison record, so a plain `worktree remove` refuses; `--force` is correct only after this check (floor 12):
  ```
  n=$(git -C "$W-ctl" status --porcelain | grep -vc '^??')          # must print 0; if not, STOP and post
  [ "$n" = 0 ] && git -C "$W" worktree remove --force "$W-ctl"
  ```
  Copy the control's comparison record out first. Never `rm -rf` the control.
- A mover's post gives: its name, both verdicts, the first line of its C# failure text, and which seat you think moved
  it. Mark that last part as inferred.
- Do not fix a mover. Report it.

## 5. What to post back

Send **one message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `TRAIN K linux legs (<LANE>) at
<union short SHA>`.
```
WHAT: <LANE> linux legs at the TRAIN K union <sha10>: <n> legs, <n> movers.
EVIDENCE:
- box: uid <id -u>; Microsoft .NET 10.0.12 (verifier PASS, <n> paths); sdk <dotnet --version>; go1.24.13 GOROOT <as printed>
- tree <sha10> tree <tree10>; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>: <rows>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; named arms: <pass | the exceptions>
- per row: <pkg> expected <N + D>, read <N + D>, record fresh yes, movers <names | none>   (one line each)
- runtime (P2): <verdicts> verdicts, undisclosed <n>: <names>; ParentChild <verdict>
- runtime/pprof (P2): run 1 <N + D>, run 2 <N + D>; TestGoroutineProfileConcurrency <v1>/<v2>; the bank annotation: linux: <N + D>
- test sources rewritten: <rows: count>; csproj rewritten: <0 | names>
- not measured: <what and why>
NEXT: <idle | the control you are running>
```
If the evidence runs past 40 lines, keep the files in `~/tK-linux-<LANE>/` and say so. COORD asks for what it needs.

## 6. What you do NOT do

- Change nothing in the checked-out union: no edits, commits or pushes. The only writes are the `-tests` pipeline's,
  and the driver restores them.
- Never run two readings at once on the box, and never run a leg under `strace` except L1v and L0v.
- Do not run CNR, the roster guard or the sweep wrapper. There is no pwsh on your box, and those are COORD's legs on the
  i7.
- Kill a hung process by PID, never by name (floor 5).
- Never read an old results file, and never gate a reading on rc alone.
