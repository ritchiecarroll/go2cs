# <TRAIN>: the linux legs for P1 and P2

TEMPLATE. Generalised from `trainL/tL-lane-brief-linux.md` (claude/coord-handover 51c5ab0e42, itself TRAIN K's brief
adapted), with the corrections the two trains taught. COORD copies it per train and fills every `<...>` below. It pairs
with `templates/linux-legs.sh`, the driver. These are **readings only**: nothing to cut, commit or push. Start when
COORD's GO message arrives in your inbox; the GO fills in the values.

| | |
|---|---|
| Union ref / SHA | `<UNION-REF>` / `<UNION: 40 hex>` (the fixup commit is the head, or `<FIXUP: 40 hex>` is an ancestor) |
| Base | `<BASE>`, the previous train's landed master, + the seats in `<SEATS-FILE>` on `<HND-REF>` + the fixup |
| Driver | `.claude/coord-scripts/templates/linux-legs.sh` on `<HND-REF>` (tip `<HND: filled in by COORD>`; until it is seated, on `claude/p1-linux-legs-template`) |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`) |

## 0. Which readings these are: ROOT

Both linux boxes run as **uid 0** today (P1 since 2026-09-30, P2 per COORD 2026-10-02). Every reading here is a root
reading and the post says so (`uid` on its box line). A root box **masks permission-class defects**: root ignores file
modes and read-only attributes, so a copy or staging step that fails for an unprivileged user passes (COORD's note:
the module-cache ReadOnly staging bug passed as root). Known masked or shifted cases:
- `os`: Go's own oracle skips `TestFilePermissions` (its 9 subtests) as root and 4 tests move PASS to SKIP, so a root row
  reads `903 + 2` where the banked non-root annotation is `912 + 2` (P1's sizing, 2026-09-30). Expect the root number.
- Any seat that touches file modes, read-only attributes, ownership, symlink or copy staging (module-cache copies, test
  fixture staging): the GO names a **NON-ROOT reading** for that seat. Run it as an unprivileged
  account with its own HOME and a clone it owns, with the same toolchain. If the box has no such account, post
  `NOT MEASURED: non-root` and name the seat. Never offer the root reading in its place.

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone has **no tracked changes and no untracked files under any path the union tracks**. The driver
   refuses both, naming each file (an untracked file at a path the union tracks would also fail the checkout). Clean
   them yourself, by exact path. Ignored build output does not count. `LEGS_CHECK_ONLY=1` on the command line below runs
   just these checks and stops before the checkout, so you can see a refusal without starting a reading.
3. Fetch the driver to a path **outside** the clone:
   `git -C <clone> fetch origin <HND-REF> && git -C <clone> show FETCH_HEAD:.claude/coord-scripts/templates/linux-legs.sh > ~/linux-legs.sh`
4. Run it **in the background**, detached (`setsid nohup ... &`). Do not poll inside a turn. Read `SUMMARY.txt` when it exits.
   ```
   LANE=P1 UNION=<UNION> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/linux-legs.sh
   LANE=P2 UNION=<UNION> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/linux-legs.sh
   ```
   `FLOOR_GB` is the free-disk floor (default 15). A 13 GB box sets 6; a launch with the default aborts at the first
   leg, and a floor above what the heaviest leg needs aborts mid-run (TRAIN K's first P1 launch stopped at an 8 GB floor).
   P2 also takes `TBS_N` (default 10) and `TBS_LOAD` (default 1). Evidence lands in `$R`, default `~/tL-linux-<LANE>/`
   (the name is TRAIN L's; set `R=` to rename it). The first SUMMARY line prints the lane, uid, head, tree, go, sdk and nproc.

What the driver sets for you (read it before running it):
- `GOTOOLCHAIN=local CGO_ENABLED=0`, `$GOROOT/bin` first on `PATH`. It checks the GOROOT value before exporting it
  (absolute, no backslash, no trailing slash, `VERSION` = go1.24.13; floor 6), then `go env GOROOT` and `go version`.
  An ambient other GOROOT reds oracle comparisons falsely.
- `GO2CS_MODULE_ROOT` is **unset**: the converter sets it per test host for module packages only, and an inherited
  value would hide a converter that stopped setting it.
- It refuses a union without the **fixup** (a `fixup:` commit after `<BASE>`, or any `src/core`/Behavioral/Performance
  csproj still at `<LangVersion>latest</LangVersion>`).
- `GoTargetOS` is **not exported**. The `-tests` rows get it from the target platform, GolibTests commands pass
  `GoTargetOS=linux`, and the LB leg runs `env GoTargetOS=linux` (section 2).
- Floors 3, 7 and 8. The converter is rebuilt after every purge. Every leg has an outer `timeout`, and GolibTests also
  runs with `--blame-hang-timeout 20m`.

## 2. Who runs what, and why

COORD's GO fills in the Expected and Gates columns from the roster's linux annotation (`docs/ValidatedTestPackages.md`)
and the train's seat list. The legs below stand in every train; `<per train>` marks what changes.

**P1:**

| Leg | What | Expected | Gates |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14; record fresh (targetGOOS linux). **The driver STOPS here otherwise.** | the toolchain and the fixup |
| L2 | GolibTests **Release and Debug**, FULL | 0 failed | everything golib |
| L2l | GolibTests Release filtered to the train's classes | all pass; list every SKIP by name | the named arms (`<per train>`) |
| LCn | `go test -v -run` the train's named converter tests | `--- PASS` for every name; 0 SKIP, 0 FAIL | `<per train>` |
| LCf | `go test -count=1 ./...` in `src/go2cs` | ok | report every FAIL by name; a red outside LCn's names is a finding to post, not a gate |
| L4 | the train's rows (`<per train>`) | the roster's linux `N + D`; **0** `all goroutines are asleep` lines in the log and in the record's stderr tails unless the line is that row's expected output | `<per train>` |
| LPB | `cmp -tests -test-publish-binlog`, with a 1 s background poll on `src/core/cmp/bin/tests/publish.binlog` | rc 0; the binlog **SEEN** during the run and **absent** afterwards; the record fresh with `targetGOOS` linux | `-p:GoTargetOS=linux` and `-bl` composed on one publish; the dotnet argv is not echoed, so the poll is the proof |
| LB | the behavioral runner on the train's projects (`<per train>`) | each passes its phases; a main-alone deadlock project: stdout empty, stderr first line `fatal error: all goroutines are asleep - deadlock!`, **exit 2** on both sides | the runner's output path is inferred; a missing executable reads NOT MEASURED |

**The LB command line.** Each project runs through the runner with the linux flavour set per command, so the driver
itself keeps `GoTargetOS` unexported, and the driver stamps the exact line before each project:
```
bash -c "cd '<W>/src/tests/Behavioral' && env GoTargetOS=linux '<W>/src/tests/Behavioral/BehavioralRunner/bin/Debug/net10.0/BehavioralRunner' --filter '<project>'"
```
Without `env GoTargetOS=linux` the runner builds the windows flavour: the C# side dies on `kernel32.dll`
(`DllNotFoundException`) with exit 2 against Go's 0 (TRAIN L: 6 of 7 projects failed that way, the deadlock project by its
first stderr line being the windows-build warning; with it set, 7 of 7 passed).
A failure like that in your log is the instrument, not the seat: say so, re-read with the flavour set, and report both.

**The named arms in L2/L2l.** Report each as pass, fail or SKIP. A SKIP in a named arm is a finding, except where the
test's own text gates it to another OS: TRAIN L's two linux skips were `JunctionGodebugToolchainChildTests` ("the
junction fallback exists only on Windows") and `ParseDebugVarsAtStartTests.GotracebackWerAtStart...` ("enableWER acts
only on Windows"). Name the arms and the reason a skip is expected in the GO (`<per train>`).

**P2:**

| Leg | What | Expected | Gates |
|---|---|---|---|
| L2 | GolibTests Release, FULL and PLAIN | 0 failed | everything golib, on a second box |
| L2l | the same class filter as P1 | all pass; SKIPs named | as P1 |
| L5 | `runtime/pprof -tests`, once (or twice, per the GO) | the roster's linux `N + D`; `TestGoroutineProfileConcurrency` pass | `<per train>` |
| L6 | `runtime -tests`, the FULL row (150 m per child, 330 m outer) | the roster's linux `N + D`, undisclosed 0; the disclosed-divergent tests read **disclosed**, not red; the crash family (`<per train>`) pass/pass | `<per train>` |
| TBS | `runtime -tests -test-filter '^TestTracebackSystem$'`, **TBS_N** iterations beside `nproc` busy loops (`TBS_LOAD=1`) | every iteration: `/panic` and `/trap` disclosed-divergent on their pinned signatures, **0** `all goroutines are asleep` lines | **A filtered run is DIAGNOSTIC ONLY and never banks.** The busy loops are an inference of a real concurrent load; if you load the box another way, say how |

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

- **The tree.** HEAD equals the union SHA. `<BASE>` and each seat the GO names are ancestors, and so is the fixup. There
  were no tracked changes and no untracked files under tracked paths before the run.
- **The toolchain.** Each comparison record reads `oracleGoVersion` go1.24.13, `configuration` Release, `tiered` false,
  `targetGOOS` linux. A failure prints `FRESHNESS FAIL: <field>`, and that row is **not a reading**.
- **One converter.** The same `converterRevision` in every manifest, and matching `BUILD converter` stamps.
- **Fresh record or no reading.** A row's three result files are deleted first, and only a newer record is accepted.
- **GolibTests.** Read the `Passed!`/`Failed!` totals in THIS leg's log, never rc alone. The driver flags
  `HANG/ABORT/CRASH text present in the log` only on the host's crash or abort banners and on a Blame message other than
  its normal completion line, so any flag is real: read the log.
- **Test sources.** Rewritten test `.cs` files are counted per row, not chased. **csproj among the rewrites must be 0,
  except the known per-row exceptions below.** A csproj rewrite elsewhere, or any `LangVersion` or S1 line in an
  exception's diff, is a fixup finding to post. Each row's `-U0` patch is kept for COORD to read.
  Known exceptions (read as the linux flavour's `-tests` emission over a committed windows-shaped csproj; P1 inferred it
  at TRAIN K and COORD agreed, it was not measured against a fixup):
  - `os`: 1 (P1, TRAIN K): the linux-flavour Compile and ProjectReference items replace the committed ones.
  - `runtime/debug`: 1 (P1, TRAIN K): adds `panic_test.cs` and the ProjectReferences `internal.runtime.syscall`,
    `syscall` and `unsafe`.
  - `runtime` and `runtime/pprof`: COORD's note of 2026-10-02 (linux-only test files and references); P1 has not read
    these two rows' csproj diffs.
  Everything else P1 read at TRAIN L read 0: utf8, reflect, fmt, internal/fmtsort, encoding/json, sync, net/rpc,
  log/slog and cmp. Name an exception only after reading its diff. Report a new one rather than waving it through.
- **Deadlock lines.** The driver counts `all goroutines are asleep` in every `-tests` log AND in each fresh record's
  stderr tails (go and csharp). Expect 0 except where that line is a row's expected output (`<per train>`).
- **END.** `freshness-fails` 0 and `tracked-deletions` 0. `tracked-changes` read 10 at TRAIN K (the `-tests` proof
  pages under `docs/validation/current/`, which the driver does not restore per row) and 0 at TRAIN L (the LB restore
  swept them). Report what you read and name the paths.

## 4. A row that moves

A mover is any of these:
- a row whose `N + D` differs from the expected column;
- an undisclosed error;
- an orphaned disclosure;
- a GolibTests failure, or a SKIP in the named arms that the GO did not expect;
- a deadlock line where none is expected, or a TBS iteration that reds.

For each mover, run a **CONTROL**: the same row at `<BASE>`, in a worktree that is a child of your clone. `<BASE>` is TRAIN K's master
or later and carries p1-tests-goos, so the control needs no `GoTargetOS`:
```
git -C "$W" worktree add --detach "$W-ctl" <BASE>
cd "$W-ctl" && (cd src/go2cs && go build -o bin/go2cs .)
./src/go2cs/bin/go2cs -tests -test-action all -test-timeout 30m -test-config Release \
  -go2cspath "$W-ctl/src" "$GOROOT/src/<pkg>" "$W-ctl/src/core/<pkg>"     # rc captured before any pipe
```
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
- Then remove the control, but only after checking it holds no tracked change. A `-tests` control always rewrites tracked
  test sources under `src/core/<pkg>` and leaves untracked build output and a comparison record, so restore that package
  first, then run the gated removal (`--force` is correct only after this check, floor 12). Copy the control's comparison
  record out first:
  ```
  git -C "$W-ctl" checkout -- "src/core/<pkg>"
  n=$(git -C "$W-ctl" status --porcelain | grep -vc '^??')          # must print 0; if not, STOP and post
  [ "$n" = 0 ] && git -C "$W" worktree remove --force "$W-ctl"
  ```
  Never `rm -rf` the control (floor 12).
- A mover's post gives: its name, both verdicts, the first line of the C# failure text, and the seat you think moved it.
  Mark that last part as inferred.
- For a TBS iteration that reds, the control is the same filter at `<BASE>` under the same load.
- Do not fix a mover. Report it.

## 5. What to post back

Send **one message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `<TRAIN> linux legs (<LANE>) at
<union short SHA>`.
```
WHAT: <LANE> linux legs at the <TRAIN> union <sha10>: <n> legs, <n> movers.
EVIDENCE:
- box: uid <id -u>; Microsoft .NET 10.0.12; sdk <dotnet --version>; go1.24.13 GOROOT <as printed>; nproc <n>
- tree <sha10> tree <tree10>; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>: <rows>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; named arms: <pass | the exceptions>
- (P1) LCn not-PASS <names | none>; LCf <ok | FAIL names>
- (P1) LPB: rc <n>; binlog during <SEEN | never-seen>, after <absent | PRESENT>; targetGOOS <linux>
- per row: <pkg> expected <N + D>, read <N + D>, record fresh yes, deadlock lines <n>, movers <names | none>   (one line each)
- (P1) LB: <project: phases; exit codes for the main-alone pair>; the command line stamped in SUMMARY
- (P2) runtime <N + D>, undisclosed <n>; the named tests' verdicts; crash family <all pass | names>
- (P2) TBS-LOOP: <k>/<TBS_N> clean (disclosed signature, 0 deadlock lines); load <how>
- test sources rewritten: <rows: count>; csproj rewritten: <0 | names, each checked against section 3's exceptions>
- permission-class seats: <none | seat, and the non-root reading or NOT MEASURED: non-root>
- not measured: <what and why>
NEXT: <idle | the control you are running>
```
If the evidence runs past 40 lines, keep the files in `$R` and say so. COORD asks for what it needs.

## 6. What you do NOT do

- Change nothing in the checked-out union: no edits, commits or pushes. The driver restores the only writes, which come
  from the `-tests` pipeline and the behavioral runner.
- Never run two readings at once on the box. Never run a leg under `strace`.
- Do not run CNR, the roster guard or the sweep wrapper. There is no pwsh on your box, and those are COORD's legs on the i7.
- Kill a hung process by PID, never by name (floor 5). The TBS busy loops are killed by their PIDs.
- Never read an old results file, and never gate a reading on rc alone.
- If the driver itself is wrong (an instrument defect, not a seat), do not edit it mid-run: finish, report the defect in
  one line with the evidence, and re-read the affected leg by hand with the fix, labelled as a re-read.
