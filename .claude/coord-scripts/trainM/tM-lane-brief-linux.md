# TRAIN M: the linux legs for P1 and P2

From COORD. DRAFT 2026-10-02. Filled from P1's template (`.claude/coord-scripts/templates/linux-brief.md`, seat
`p1-linux-legs-template`), which is TRAIN L's brief with the corrections two trains taught. These are **readings
only**: nothing to cut, commit or push. Start when COORD's GO message arrives in your inbox; the GO fills in the
values below.

| | |
|---|---|
| Union ref / SHA | `claude/coord-trainM-union` / `<UNION: 40 hex>` (the fixup commit is the head, or `<FIXUP: 40 hex>` is an ancestor) |
| Base | `aa0a07d5fd`, TRAIN L's landed master, + one signed merge per row of `.claude/coord-scripts/trainM/tM-seats-draft.txt` on `claude/coord-handover` (**24 rows, FROZEN** at 08:20 on 2026-10-02; the driver reads the rows, never a count) + the TRAIN M fixup (`fixup: TRAIN M`, and any `fixup-N: TRAIN M` commit on top of it). No follow-up merge rides: the follow list is empty |
| Driver | `.claude/coord-scripts/trainM/tM-linux-legs.sh` on `claude/coord-handover` (tip `<HND: filled in by COORD>`), with **two companions from the same commit**: `tM-helpers.py` and `tM-seats-draft.txt` |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`) |

## 0. Which readings these are: ROOT

Both linux boxes run as **uid 0** (P1 since 2026-09-30, P2 per COORD 2026-10-02). Every reading here is a root reading
and the post says so (`uid` on its box line). A root box **masks permission-class defects**: root ignores file modes
and read-only attributes, so a copy or staging step that fails for an unprivileged user passes (TRAIN L's module-cache
ReadOnly staging bug passed as root). Known shifted cases:
- `os`: Go's own oracle skips `TestFilePermissions` (9 subtests) as root and 4 tests move PASS to SKIP, so a root row
  reads `903 + 2` where the banked non-root annotation is `912 + 2` (P1's sizing, 2026-09-30). Expect the root number.
- **Leg LM (the two real modules) stages a module copy into the test sandbox.** A root reading of it does not stand in
  for a non-root one. If P1 can run LM as an unprivileged account (as it did for the x/sync red/green at TRAIN L, uid
  1001, with its own HOME, clone, toolchain and module cache), do that and say so. Otherwise post
  `NOT MEASURED: non-root` for LM beside the root reading. Never offer the root reading in its place.

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone has **no tracked changes and no untracked files under any path the union tracks**. The driver
   refuses both, naming each file. Clean them yourself, by exact path. Ignored build output does not count.
   `LEGS_CHECK_ONLY=1` on the command line below runs just these checks and stops before the checkout.
3. Fetch the driver and its two companions to ONE folder **outside** the clone, all from ONE fetch:
   ```
   git -C <clone> fetch origin claude/coord-handover
   mkdir -p ~/tM
   for f in tM-linux-legs.sh tM-helpers.py tM-seats-draft.txt; do
     git -C <clone> show FETCH_HEAD:.claude/coord-scripts/trainM/$f > ~/tM/$f
   done
   ```
4. Run it **in the background**, detached (`setsid nohup ... &`). Do not poll inside a turn. Read `SUMMARY.txt` when
   it exits.
   ```
   LANE=P1 UNION=<UNION> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/tM/tM-linux-legs.sh
   LANE=P2 UNION=<UNION> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/tM/tM-linux-legs.sh
   ```
   `FLOOR_GB` is the free-disk floor (default 15). A 13 GB box sets 6. P2 also takes `TBS_N` (default 10),
   `TBS_LOAD` (default 1) and `RT_CHILD` / `RT_OUTER`, the full runtime row's per-child and outer deadlines. Their
   defaults are **`210m` / `450m`** (raised from TRAIN L's 150 m / 330 m: you read that row at 9313 s wall against a
   9000 s per-child deadline, and M adds cost in every host); change them only when the GO says so. P1's leg LM takes
   `REALMOD_TEST_TIMEOUT` (default `2m`, see section 2). Evidence lands in `$R`, default `~/tM-linux-<LANE>/`.
   **The driver's exit status means something now: 0 = every expectation it checks held; 4 = at least one `MOVER:` or
   `FRESHNESS FAIL` line is in `SUMMARY.txt`** (the END line prints `movers=<n>`). All legs still run to the end.
   **A re-run takes a fresh `R=<new folder>`.** The driver appends to `$R/SUMMARY.txt`, and although it now deletes a
   row's own evidence files before the row runs, a folder that holds two runs is two runs to read.

What the driver sets for you (read it before running it):
- `GOTOOLCHAIN=local CGO_ENABLED=0`, `$GOROOT/bin` first on `PATH`. It checks the GOROOT value before exporting it
  (absolute, no backslash, no trailing slash, `VERSION` = go1.24.13; floor 6), then `go env GOROOT` and `go version`.
- `GO2CS_MODULE_ROOT` is **unset**: the converter sets it per test host for module packages only.
- **Seat asserts are read from the seat list beside the driver**: `aa0a07d5fd` and every row are ancestors of the
  union. It also refuses a union without a `fixup: TRAIN M` commit, or with any csproj still at
  `<LangVersion>latest</LangVersion>`.
- `GoTargetOS` is **not exported**. The `-tests` rows get it from the target platform, GolibTests commands pass
  `GoTargetOS=linux`, and the LB leg runs `env GoTargetOS=linux` per project.
- **A row is read at the execution config its roster line carries.** Every row runs Release with tiering OFF, and its
  record must say `tiered=False`, EXCEPT `log/slog`: the driver runs it with `-test-tiered` and its record must say
  `tiered=True`. A record at the other config prints `FRESHNESS FAIL` and is not a reading. The driver reads WHICH row
  that is from the roster of the union it checked out; it is not typed. The reader is `tM-helpers.py execrows` (the one
  the i7 battery uses): it reads the row's **annotation field**, `· execution: release-tiered ·`, never the phrase
  elsewhere on the row's line (net/http and internal/godebug still say "It carried `execution: release-tiered` from
  ..." in their notes). The driver stamps `DERIVED execution-config rows ... at the union=[...] at the base=[...]`:
  **expect `log/slog[release-tiered]` alone at the union**, and internal/godebug, log/slog and net/http at the base.
  If the line shows anything else, stop and post it before reading a row.
- **The by-name lists are derived on your box** from git between `aa0a07d5fd` and the union, and stamped as `DERIVED`
  lines: the converter tests the union adds (leg LCn), the GolibTests classes it adds or changes minus the ones a linux
  build does not compile (the `mseats` filter and the by-class TRX reading), the behavioral projects it adds (leg LB).
  Each row's expected counts are its roster line's `linux:` annotation (one stated exception: `os` as root, 903 + 2).
  A row accepted after this brief was written is therefore read without a new driver: post the `DERIVED` lines.
- Floors 3, 7 and 8. The converter is rebuilt after every purge. Every leg has an outer `timeout`, and GolibTests also
  runs with `--blame-hang-timeout 20m`.

## 2. Who runs what, and why

**P1:**

| Leg | What | Expected | Gates (seat) |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14; record fresh (targetGOOS linux). **The driver STOPS here otherwise.** | the toolchain and the fixup |
| L2 | GolibTests **Release and Debug**, FULL | 0 failed. G read 1491/0/15 at its own lead on P2's box; C1's seat read 1497 and 1505 passed, 0 failed. Report yours | every golib seat: G (`Goroutine.cs`), the i9's crash-class seat (four binders; **never run on linux**), c1-darwin-std-hygiene (`builtin.cs`) |
| L2m | GolibTests Release filtered to the M classes | all pass; list every SKIP by name | the named arms below |
| LCn | `go test -v -run` over `./...`: every converter and repoguard test the union ADDS (derived from the union's diff: 73 names at the 24-row list) + the five `TestProjitems*` | `--- PASS` for every name; 0 SKIP, 0 FAIL. A name that skips on linux by design is a mover to post with its skip text | every converter seat: G, the i9 (`TestByRefReceiversCarryGoRecv`), c1-fixture-tracking, the C1 darwin seats, P2's F4, F8 and H2, C2's two nuget seats, the registering seats |
| LCf | `go test -count=1 ./...` in `src/go2cs` | ok | report every FAIL by name; a red outside LCn's names is a finding to post, not a gate |
| L4 | `net/http` (60 m per child) | **1387**, rc 0 (the TestMain leak check clean), `TestRegisterErr` and `TestRegisterErr/a` pass/pass, `tiered=False` | G: net/http drops `execution: release-tiered` |
| L4 | `internal/godebug` | **5**, `TestCmdBisect` pass/pass, `tiered=False` | G: the PC-to-line fix; internal/godebug drops `execution: release-tiered` |
| L4 | `log/slog`, **with `-test-tiered`** | **199 + 17**, `tiered=True`; `TestSetDefault`, `TestPanics`, `TestCallDepth` by name | the one row that KEEPS release-tiered. The ruled reason is a timing flake on a busy disk: symbol-file reads at the first source-line resolution (rooted in `runtime.Callers` symbolizing at capture), not JIT latency: a `TestSetDefault` red is read as that class first. You read it green at TC0 at TRAIN L; this train reads it at the config the roster banks |
| L4 | `sync` | 46 + 6 | checkdead's neighbours under G's goroutine classification |
| L4 | `os`, `os/exec` | `os`: the root reading **903 + 2** (banked non-root: 912 + 2); `os/exec`: 87 + 1 | the i9's crash-class seat edits `internal/syscall/unix/linux/siginfo_linux.cs`, which compiles **only for linux** and was never built there by its seat; and `testing.cs`, in every host |
| LPB | `cmp -tests -test-publish-binlog`, with a 1 s background poll on `src/core/cmp/bin/tests/publish.binlog` | rc 0; the binlog **SEEN** during the run and **absent** afterwards; the record fresh with `targetGOOS` linux | P2's F4 rewrote the publish deadline in the function that also carries `-bl` and `-p:GoTargetOS=linux` |
| UF | (read after the rows, and again at END for both lanes) untracked, not-ignored paths under `src/core` | **0**; anything else is a MOVER | c1-fixture-tracking: a `-tests` run used to leave fixture copies there |
| LB | the behavioral runner, `env GoTargetOS=linux` per project, on the projects the union adds (derived: 7 at the 24-row list) + ten named ones | see below; the runner's build and each project's rc are gated (rc 0), except `SetegidBroadcastSeam` | the i9, P2's F-batch, hazard H3, c1-darwin-std-hygiene, G's linux goldens |
| LM | `go2cs -tests -recurse` over `golang.org/x/sync@v0.19.0` and `golang.org/x/mod@v0.33.0`, `-test-timeout $REALMOD_TEST_TIMEOUT` | see below | P2's fix batch END ACCEPTANCE; F4 |

**LB, by project.** Each passes its phases unless noted.
- `PromotedPtrMethodValueSet` (the i9: it prints Go's 63 lines; never run off windows).
- `VariadicClosureShadowParam` (F5), `AliasStructToInterface` and `AliasStructToInterfaceLib` (F3),
  `NamedArrayVsUnnamedCompare` (F7), `PanicOnlyFuncLiteralVar` and `FuncLiteralDeclaredResultIface` (F1+F6).
  `--filter AliasStructToInterface` is a substring match and also selects the Lib project.
- `ForeverWaitWorkersMainReturns`, `MainSelectForeverWorkerExits`, `MainSelectForeverAfterFunc` (Go exit 0), and
  the main-alone pair `ChannelReceiveFromNil`, `ChannelSendToNil`: stdout empty, stderr first line
  `fatal error: all goroutines are asleep - deadlock!`, **exit 2** on both sides. These are TRAIN L's checkdead guards.
  G's seat changes which goroutines count as user goroutines; nobody has read the two together.
- `LinuxSpawnBasics`, `StdoutCloseEofBarrier` (c1-darwin-std-hygiene's claim: both PASS on linux).
- `UnixAbstractAddrName`, `WritevIovecSeam` (G re-baselined both on linux).
- `SetegidBroadcastSeam`: **its Target phase is expected to read a residual.** G took its own 3 lines into that golden
  by hunk; the go1.24 alias family (lines 5, 43, 60, 61 on G's WSL box) stays owned by the hop. Report the residual's
  lines. A residual that is exactly the alias family is the expected reading; anything else is a mover.
- Without `env GoTargetOS=linux` the runner builds the windows flavour and the C# side dies on `kernel32.dll` with
  exit 2 against Go's 0. A failure like that in your log is the instrument: say so and re-read.

**LM, by package.** The numbers are P2's linux reading on its LOCAL merge of the six fixes (2026-10-02). The union is
the first pushed tree with all six, so this is the re-read. The driver stamps one `REALMOD` line per package.

| Module | Package | Expected |
|---|---|---|
| x/sync | `errgroup` | Validated 5 |
| x/sync | `syncmap` | Validated 3 |
| x/sync | `singleflight` | **With the H1 host seat in the union** (`p2-host-test-list`: it is a row of the 24-row list): **Validated 12**. With no such row: **KNOWN**, 11 of 12; `TestPanicDoChan` has no C# verdict (host finding H1: the host ignores `-test.list`). The driver reads the seat list and expects accordingly |
| x/sync | `semaphore` | **KNOWN**: 7 of 8; `TestWeightedAcquire` Go pass, C# fail (the known timing-class non-pass; P2's acceptance labels it "TC0 timing", its cause is not measured) |
| x/mod | `modfile` | Validated 323 |
| x/mod | `module`, `semver` | Validated 16, 9 |
| x/mod | `sumdb/dirhash`, `sumdb`, `sumdb/note`, `sumdb/storage` | Validated 6, 4, 7, 1 |
| x/mod | `sumdb/tlog` | **With the H2 host seat in the union** (`p2-host-event-line-start`: a row of the 24-row list): **Validated 17** (its Go side reads a network log: read a red there against your box's network first). With no such row: **KNOWN**, 16 of 17; `TestCertificateTransparency` has no C# verdict (host finding H2) |
| x/mod | `zip` | builds and runs; counts reported (P2 read 109 / 9 / 3; its 4 `TestVCS` mismatches were that box's network). **A differing name outside `TestVCS` is a FAIL** (`zip=build:TestVCS`) |

- A KNOWN line is not a mover. A KNOWN package that VALIDATES instead prints `CLEARED` and passes: its seat landed.
- Anything else is a mover: a package that fails differently, a count that differs, a package the run names that the
  table does not.
- `REALMOD-F4`: **0** `dotnet timed out` lines. F4 gives the publish alone `max(-test-timeout, 30m)`. At the `2m`
  default the driver gates it: any other count, or an unread one, is a MOVER.
- `-test-timeout` is passed explicitly. The default here is `2m`, the converter's own default and the value P2's
  acceptance was read at. With H1 unseated, `singleflight`'s child fan-out runs until that deadline.
  **F4's reading exists only at `2m`.** The publish floor is `max(-test-timeout, 30m)`, so at a longer
  `REALMOD_TEST_TIMEOUT` a first publish fits inside the deadline with or without F4 and 0 timeout lines proves
  nothing: post F4 as `NOT MEASURED` if you raised it (the driver stamps it so and does not gate it).
- If the x/tools pin cannot be verified (the cache's `v0.42.0.mod` missing, or the go.mod line not edited exactly
  once), the driver stamps `LM:xmod: NOT MEASURED -- the pin failed` and runs nothing for x/mod.
- **`NOT MEASURED` is never a pass.** Every `LM:... NOT MEASURED` (a module missing from the cache, a failed pin, a
  refused preflight) is also a `MOVER:` line and makes the driver exit 4: P2's end acceptance was written for this box,
  and a run that did not read it says so in its exit status.
- With the H2 seat in the union, a red on `sumdb/tlog` is stamped `read it against this box's NETWORK first`: do that
  before you call it a seat's.
- The modules are COPIED out of the module cache into `$R/lm/fx` (`GOPROXY=off`, nothing is downloaded), with output
  under `$R/lm/out`. If the cache holds `golang.org/x/tools` v0.42.0 and not v0.41.0, the x/mod COPY takes P1's stated
  deviation (its `go.mod` names v0.42.0, its `go.sum` gains that version's two lines) and the driver stamps it. A
  module that is not in the cache reads `NOT MEASURED` (and a MOVER, as above).

**The named arms in L2/L2m.** Report each as pass, fail or SKIP. A SKIP in a named arm is a finding, except where the
test's own text gates it to another OS.
- `SystemGoroutineTests` (5), `ReturnSiteLineTests` (1, new: red at G's base at TC0 and tiered), and the creator and
  caller neighbours `TestGoroutineCreatorTests`, `CreatedByPositionTests`, `TracebackDecorationTests`,
  `SkipCountedWalkerFrameTests` (G).
- `CopyBoundReceiverTests` (9), `CopyBoundReceiverAllowlistTests` (1: the 201-row allowlist is a three-OS union),
  `NoUncountedBackingAllocationsTests` (the i9). **First linux reading of all three.**
- `DarwinStdDescriptorContractTests` (2, c1-darwin-std-hygiene). It is compiled only when `GoTargetOS=linux`, so
  yours are the only boxes that can read it. NOT FOUND here is a finding. The driver now reads every GolibTests run's
  TRX by class and stamps `classes NOT FOUND: [...] DarwinStdDescriptorContractTests=[Passed=2]`: post that line (a
  filter clause that matches nothing only lowers the console total, and the console names methods, not classes).
- `ForeverWaitDeadlockDecisionTests` (6), `GoroutineProfileInstantTests`, `GoroutineParkAccountingTests`,
  `RuntimeParkTransitionTests`, `BubbledChannelTests`, `SyncMutexProfileTests` (TRAIN L's checkdead classes, under G's
  classification), and `ModuleAncestryTests`.
- **The list is derived by the driver** (its `DERIVED GolibTests classes` line): every class the union adds or changes,
  plus the neighbours above. At the 24-row list the union also adds `DarwinArm64VariadicSlotTests` (6; its glibc arm
  runs ONLY on linux, so yours is the box that reads all six), `DarwinInode64SymbolTests` (4 methods, data rows),
  `EventLineFramingTests` (2) and `HostTestListTests` (4) (P2's H2 and H1 host seats), and changes
  `ConsoleEventLineAtomicityTests`. A class of that line with 0 results is `NOT FOUND` and a mover.

**P2:**

| Leg | What | Expected | Gates (seat) |
|---|---|---|---|
| L2 | GolibTests Release, FULL and PLAIN | 0 failed (you read 1504/0/15 at TRAIN L and 1491/0/15 at G's lead) | everything golib, on a second box |
| L2m | the same M class filter as P1 | all pass; SKIPs named | as P1 |
| L5 | `runtime/pprof -tests`, once | **147 + 7**; `TestGoroutineProfileConcurrency`, `TestBlockProfile`, `TestMemoryProfiler` by name | G's PC-to-line fix names `TestMemoryProfiler` (+ `/debug=1`); checkdead takes the goroutine profile's gate exclusive |
| L6 | `runtime -tests`, the FULL row (**210 m per child, 450 m outer**: `RT_CHILD` / `RT_OUTER`, the driver's defaults) | **10810 + 73**, undisclosed 0. **`TestTracebackSystem/panic` and `/trap` read DISCLOSED**, by name (the driver stamps `DISCLOSED` or `MOVER`). The crash family pass/pass. `TestLineNumber` disclosed | **the re-read this train owes.** On G's branch alone you read `/panic` RED, deadlock-first, because the branch lacked TRAIN L's checkdead. The union holds both |
| TBS | `runtime -tests -test-filter '^TestTracebackSystem$'`, **TBS_N = 10** iterations beside `nproc` busy loops | every iteration: `/panic` disclosed (stamped per iteration), **0** `all goroutines are asleep` lines | hazard H3 under load. **A filtered run is DIAGNOSTIC ONLY and never banks.** The busy loops are an inference of real concurrent load; if you load the box another way, say how |

Not a red, and not to be chased: with tiering ON the runtime host dies at `TestStackGrowth` on base and on fix
alike (a pre-existing, unbanked configuration). This driver never runs runtime tiered.

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

- **The tree.** HEAD equals the union SHA. `aa0a07d5fd` and every seat row are ancestors, and so is the fixup. There
  were no tracked changes and no untracked files under tracked paths before the run.
- **The toolchain.** Each comparison record reads `oracleGoVersion` go1.24.13, `configuration` Release, `targetGOOS`
  linux, and `tiered` false for every row but `log/slog`, whose record reads true. A failure prints
  `FRESHNESS FAIL: <field>`, and that row is **not a reading**.
- **One converter.** The same `converterRevision` in every manifest, and matching `BUILD converter` stamps.
- **Fresh record or no reading.** A row's three result files are deleted first, and only a newer record is accepted.
- **GolibTests.** Read the `Passed!`/`Failed!` totals in THIS leg's log, never rc alone. The driver flags
  `HANG/ABORT/CRASH text present in the log` only on the host's crash or abort banners and on a Blame message other
  than its normal completion line, so any flag is real: read the log.
- **Test sources.** Rewritten test `.cs` files are counted per row, not chased. At M they are EXPECTED to gain
  `[MethodImpl(MethodImplOptions.NoInlining)]` on functions that execute a `go`, and its `using` line: G regenerated no
  committed test source. **csproj among the rewrites must be 0, except the known per-row exceptions, each bounded by
  its CONTENT, not by the row's name:** `runtime.pprof.tests.csproj` gains `rusage_test.cs` and the
  `internal/runtime/syscall` reference; `runtime.tests.csproj` gains its linux-only test files (the committed test
  projects are the windows record); `os` (1) and `runtime/debug` (1) as P1 read at TRAIN K. P2's F8
  (`p2-test-overload-references`) can add a reference to a regenerated test project, and `runtime` and `runtime/pprof`
  are exactly the rows P2 reads, so a whole-row exception would swallow the change it is there to catch: **diff each
  row's kept `tests-<row>-row.srcdiff.patch` csproj hunks against your TRAIN L patch of the same row; any csproj line
  TRAIN L's did not carry is a finding (F8): post it with its diff.** If you no longer hold the TRAIN L patch, post
  the csproj hunks themselves. Each row's `-U0` patch is kept for COORD.
- **Deadlock lines.** The driver counts `all goroutines are asleep` in every `-tests` log AND in each fresh record's
  stderr tails. Expect 0 everywhere in this brief's rows. In LB that line is the expected stderr of the main-alone pair
  only.
- **END.** `freshness-fails` 0, `movers` 0 and `tracked-deletions` 0, and the driver's exit status 0. Report
  `tracked-changes` and name the paths. On exit 4, `grep 'MOVER:' SUMMARY.txt` lists what missed: a leg's rc, a row's
  counts against its roster line, a record that is not fresh, `TestRegisterErr` or `TestCmdBisect` not pass/pass,
  `TestTracebackSystem/panic` or `/trap` not disclosed, a GolibTests failure or a class NOT FOUND, an LCn name that is
  not PASS, an LM package that reads FAIL. Since verify round 3 also: a GolibTests build that failed (its run was
  skipped), a TRX reader that printed no totals line, the LB runner's build or any LB project's rc (not
  `SetegidBroadcastSeam`'s: its residual is expected, report its lines), LB or LM `NOT MEASURED`, LM's F4 line at the
  2m default, LPB off its expected shape (rc 0, pre absent, SEEN, absent after), UF not 0, and a deadlock line in a
  TBS iteration (its log or its fresh record). The deadlock-line count of the ordinary rows is still a stamp: read it.

## 4. A row that moves

A mover is any of these:
- a row whose `N + D` differs from the expected column;
- an undisclosed error, or `TestTracebackSystem/panic` not reading disclosed;
- an orphaned disclosure;
- a GolibTests failure, or a SKIP in the named arms that this brief did not expect;
- a deadlock line where none is expected, or a TBS iteration that reds;
- an LM package that is not PASS, KNOWN or CLEARED.

For each mover, run a **CONTROL**: the same row at `aa0a07d5fd`, in a worktree that is a child of your clone:
```
git -C "$W" worktree add --detach "$W-ctl" aa0a07d5fd
cd "$W-ctl" && (cd src/go2cs && go build -o bin/go2cs .)
./src/go2cs/bin/go2cs -tests -test-action all -test-timeout 30m -test-config Release \
  -go2cspath "$W-ctl/src" "$GOROOT/src/<pkg>" "$W-ctl/src/core/<pkg>"     # rc captured before any pipe
```
For `net/http` and `internal/godebug` the control at `aa0a07d5fd` is their OLD config: add `-test-tiered` to the
control, or you compare two configs and not two trees. For `log/slog` add `-test-tiered` on both.
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
- Then remove the control, but only after checking it holds no tracked change. Copy its comparison record out first:
  ```
  git -C "$W-ctl" checkout -- "src/core/<pkg>"
  n=$(git -C "$W-ctl" status --porcelain | grep -vc '^??')          # must print 0; if not, STOP and post
  [ "$n" = 0 ] && git -C "$W" worktree remove --force "$W-ctl"
  ```
  Never `rm -rf` the control (floor 12).
- A mover's post gives: its name, both verdicts, the first line of the C# failure text, and the seat you think moved
  it. Mark that last part as inferred.
- Do not fix a mover. Report it.

## 5. What to post back

Send **one message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `TRAIN M linux legs (<LANE>)
at <union short SHA>`.
```
WHAT: <LANE> linux legs at the TRAIN M union <sha10>: <n> legs, <n> movers; driver exit <0 | 4>, END movers=<n> freshness-fails=<n>.
EVIDENCE:
- DERIVED (the driver's own lines): execution-config rows at the union [<expect log/slog[release-tiered] alone>]; GolibTests classes <n>; LCn names <n>; LB projects <n>; host seats H1=<ref | ABSENT> H2=<ref | ABSENT>
- box: uid <id -u>; Microsoft .NET 10.0.12; sdk <dotnet --version>; go1.24.13 GOROOT <as printed>; nproc <n>
- tree <sha10> tree <tree10>; seats <n> all ancestors; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>: <rows>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; named arms: <pass | the exceptions>; DarwinStdDescriptorContractTests <FOUND n/2 | NOT FOUND>
- (P1) LCn not-PASS <names | none>; LCf <ok | FAIL names>
- (P1) net/http <N + D> tiered=<False?> TestRegisterErr <v>; internal/godebug <N> tiered=<False?> TestCmdBisect <v>; log/slog <N + D> tiered=<True?> <the three names>
- (P1) sync <N + D>; os <N + D> (root); os/exec <N + D>; UF <n>
- (P1) LPB: rc <n>; binlog during <SEEN | never-seen>, after <absent | PRESENT>; targetGOOS <linux>
- (P1) LB: <project: phases; exit codes for the main-alone pair>; SetegidBroadcastSeam residual lines <...>
- (P1) LM: x/sync <one REALMOD line per package>; x/mod <...>; F4 lines <n>; pin <none | v0.42.0>; uid <n> (non-root reading: <done | NOT MEASURED: non-root>)
- (P2) runtime/pprof <N + D>, TestMemoryProfiler <v>; runtime <N + D>, undisclosed <n>; TestTracebackSystem/panic <verdict, DISCLOSED?> /trap <...>; crash family <all pass | names>
- (P2) TBS-LOOP: <k>/<TBS_N> clean (disclosed, 0 deadlock lines); load <how>; UF <n> (the END stamp: untracked, not-ignored paths under src/core)
- test sources rewritten: <rows: count>; csproj rewritten: <0 | names, each checked against section 3's exceptions>
- not measured: <what and why>
NEXT: <idle | the control you are running>
```
If the evidence runs past 40 lines, keep the files in `$R` and say so. COORD asks for what it needs.

## 6. What you do NOT do

- Change nothing in the checked-out union: no edits, commits or pushes. The driver restores the only writes, which
  come from the `-tests` pipeline and the behavioral runner.
- Never run two readings at once on the box. Never run a leg under `strace`.
- Do not run CNR, the roster guard or the sweep wrapper. There is no pwsh on your box, and those are COORD's legs on
  the i7.
- Kill a hung process by PID, never by name (floor 5). The TBS busy loops are killed by their PIDs.
- Never read an old results file, and never gate a reading on rc alone.
- Darwin is not yours here: the darwin run of c1-darwin-linkname-pulls and c1-darwin-std-hygiene is a mac CI
  dispatch C1 owns.
- If the driver itself is wrong (an instrument defect, not a seat), do not edit it mid-run: finish, report the defect
  in one line with the evidence, and re-read the affected leg by hand with the fix, labelled as a re-read.
