# TRAIN L: the linux legs for P1 and P2

From COORD. DRAFT 2026-10-01, adapted from TRAIN K's brief. These are **readings only**: nothing to cut, commit or
push. Start when COORD's GO message arrives in your inbox. The GO fills in the values below.

| | |
|---|---|
| Union ref | `claude/coord-trainL-union` |
| Union SHA | `<UNION: 40 hex, filled in by COORD at push>` (the TRAIN L fixup commit) |
| Base | TRAIN K's landed master `75648a022b` + 11 signed seat merges [+ the i9's nugetgo-pack 5.1 parse fix merge, when COORD seats it before the fixup (ruling R2)] + the TRAIN L fixup (seat list: `.claude/coord-scripts/trainL/tL-seats.txt` on `claude/coord-handover`) |
| Driver | `.claude/coord-scripts/trainL/tL-linux-legs.sh` on `claude/coord-handover` (tip `<HND: filled in by COORD>`) |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`), unchanged from K |

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone must have **no tracked changes**. The driver refuses a dirty clone. It fetches the union, checks
   it out detached, and refuses when the remote SHA is not the one in the GO.
3. Fetch the driver to a path **outside** the clone:
   `git -C <clone> fetch origin claude/coord-handover && git -C <clone> show FETCH_HEAD:.claude/coord-scripts/trainL/tL-linux-legs.sh > ~/tL-linux-legs.sh`
4. Run it **in the background**. Do not poll inside a turn. Read `SUMMARY.txt` when it exits.
   ```
   LANE=P1 UNION=<union SHA> FIXUP=<fixup SHA> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> bash ~/tL-linux-legs.sh
   LANE=P2 UNION=<union SHA> FIXUP=<fixup SHA> W=<your clone> GOROOT=<same>                                             bash ~/tL-linux-legs.sh
   ```
   P2 also takes `TBS_N` (default 10) and `TBS_LOAD` (default 1). Evidence lands in `~/tL-linux-<LANE>/`.

What the driver sets for you (it is K's driver with L's legs; read it before running it):
- `GOTOOLCHAIN=local CGO_ENABLED=0`, `$GOROOT/bin` first on `PATH`. It checks the GOROOT value before exporting it
  (absolute, no backslash, no trailing slash, `VERSION` = go1.24.13; floor 6), then `go env GOROOT` and `go version`.
  **The pin matters more in L than in K:** r-m6's `TestDefaultGODEBUGMatchesTheToolchain` compares the converter with
  `<GOROOT>/bin/go list`, and its go124 arm is empty only under go1.24.x. An ambient other GOROOT reds it falsely.
- `GO2CS_MODULE_ROOT` is **unset**. r-m4's converter sets it per test host for module packages only. An inherited
  value would hide a converter that stopped setting it.
- It refuses a union without the **TRAIN L fixup**: no `fixup: TRAIN L` commit after 75648a022b, or any
  `src/core`/Behavioral/Performance csproj still at `<LangVersion>latest</LangVersion>`. Two L behavioral projects
  were cut at J's template, and the fixup brings them to K's.
- `GoTargetOS` is NOT exported (K's p1-tests-goos is in). Only the GolibTests commands pass `GoTargetOS=linux`.
- Floors 3, 7 and 8 as in K. The converter is rebuilt after every purge. Every leg has an outer `timeout`, and
  GolibTests also runs with `--blame-hang-timeout 20m`.

## 2. Who runs what, and why

**P1** (the uid-0 box):

| Leg | What | Expected (the roster's linux annotation) | Gates (seat) |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14; record fresh (targetGOOS linux). **The driver STOPS here otherwise.** | the toolchain and the fixup, a sanity gate |
| L2 | GolibTests **Release and Debug**, FULL | 0 failed | everything golib: c1-token-ids, g-deadlock-checkdead, r-m4, r-m6 |
| L2l | GolibTests Release filtered to the L classes | all pass; list every SKIP by name | the named arms below |
| LCn | `go test -v -run` the 14 r-m2/m3/m4/m6 converter tests (+ 5 subtests) | `--- PASS` for all 19 names; 0 SKIP, 0 FAIL | r-m2 (the non-windows `file://` GOPROXY branch), r-m3 (the non-.exe driver binary), r-m6 (go list oracle under the pin). **No linux reading of these exists.** |
| LCf | `go test -count=1 ./...` in `src/go2cs` | a first linux reading of the whole converter suite | report every FAIL by name. A red outside LCn's names is a finding to post, not an L gate. |
| L4 | `reflect`, `fmt`, `internal/fmtsort`, `encoding/json`, `sync` | 396 + 22, 62 + 1, 3, 532, 46 + 6 | **c1-token-ids**: its "0 moved" was read on linux at its stacked base f0d257686f, before K's `r-field-ptr-equality` joined `ж.FieldRefBox.cs`. This is the first reading of the two together. |
| L4 | `net/rpc` | 15; `TestSendDeadlock` pass/pass; **0** `all goroutines are asleep` lines in the log and in the record's stderr tails | **g-deadlock-checkdead**: it removes the leaked-`select{}` shutdown fatal K's run 1 hit |
| L4 | `log/slog` at TC0 (the driver's default, tiering off) | **199 + 17**; `TestCallDepth` pass/pass | **the release-tiered retirement** (G's windows TC0 reading: Validated 199, `TestCallDepth` 10/10): green here drops log/slog's `execution: release-tiered` in L's roster commit |
| LPB | `cmp -tests -test-publish-binlog`, with a 1 s background poll on `src/core/cmp/bin/tests/publish.binlog` | rc 0; the binlog **SEEN** during the run and **absent** afterwards; the record fresh with `targetGOOS` linux | **c2-publish-binlog-onK**: the first reading of `-p:GoTargetOS=linux` and `-bl` composed on one publish (no unit test composes them; the dotnet argv is not echoed, so the poll is the proof) |
| LB | the behavioral runner on 7 projects: `ForeverWaitWorkersMainReturns`, `MainSelectForeverWorkerExits`, `MainSelectForeverAfterFunc`, `ChannelReceiveFromNil`, `ChannelSendToNil`, `ForClauseSpill`, `CrossPackagePromotedValueMethod` | each passes its phases. The main-alone pair: stdout empty, stderr first line `fatal error: all goroutines are asleep - deadlock!`, **exit 2** on both sides | g-deadlock (its deadlock exit code moves 1 -> 2, Go's), r-d6, i9-crosspkg. The runner's output path is inferred: a missing executable reads NOT MEASURED. |

**The named arms in L2/L2l.** Report each as pass, fail or SKIP. A SKIP is a finding for every arm here.
- `PointerTokenUniquenessTests` (NEW, c1-token): two live boxes with the same identity hash get two numbers, and a
  round trip returns the first. Its red was a linux reading (6/6 at f0d257686f).
- `AliasOverlapTests`, `ReflectHashTokenBandTests`, `RuntimePinnerTests` (CHANGED by c1-token), `FieldPointerEqualityTests`
  (K's, first run beside c1-token's `AllocationIdOf`-based FieldRefBox token), `FieldRefTokenTests` (its alloc assert
  `AFreshViewsFirstTokenAllocatesNothingOnceItsAccessorIsKnown`: a first `AllocationIdOf` of a non-box source allocates
  an IdentityCell).
- `ForeverWaitDeadlockDecisionTests` (NEW, g-deadlock): 6/6, including the residual row
  `ResidualAnOrdinaryChannelOrSyncWaitBlocksInsteadOfReporting`. Neighbours: `GoroutineProfileInstantTests`,
  `GoroutineParkAccountingTests`, `RuntimeParkTransitionTests`, `BubbledChannelTests`.
- `ModuleAncestryTests` (NEW, r-m4): 5/5 in Release and Debug. Copy and link-skip semantics on a case-sensitive FS
  with real symlinks.
- `SyncMutexProfileTests` + `SkipCountedWalkerFrameTests`: R's Release reading at r-m6's own base failed the pair
  (2/7). That base lacked G's saveblockevent fix, which is in K's master and so in this union. Expect 0 failed in
  every run. A red here is not r-m6 by mechanism (MSTest's entry assembly carries no attribute).
- `ParseDebugVarsAtStartTests`, `InternalCpuGodebugTests`, `NilPanicHookTests`, `JunctionGodebugToolchainChildTests`
  (r-m6 changed runtime's goenvs_impl.cs and internal/godebug). The last needs `GOTOOLCHAIN=local`, which the driver sets.

**P2** (the second box; non-root):

| Leg | What | Expected | Gates (seat) |
|---|---|---|---|
| L2 | GolibTests Release, FULL and PLAIN | 0 failed | everything golib, on a second box |
| L2l | the same L class filter as P1 | all pass; SKIPs named | as P1 |
| L5 | `runtime/pprof -tests`, once | **147 + 7** (the linux bank annotation); `TestGoroutineProfileConcurrency` pass | g-deadlock: every parked forever-waiter now takes `s_profileGate` EXCLUSIVE every 200 ms, the goroutine profile's gate; c1-token (token path) |
| L6 | `runtime -tests`, the FULL row (150 m per child, 330 m outer) | **10810 + 73**, undisclosed 0. `TestTracebackSystem/panic` and `/trap` read **disclosed**-divergent, not red. The crash family by name (`TestSimpleDeadlock`, `TestInitDeadlock`, `TestLockedDeadlock`/`2`, `TestGoexitDeadlock`, `TestGoNil`, `TestMainGoroutineID`, `TestNoHelperGoroutines`, `TestPanicDeadlockGosched`, `TestPanicDeadlockSyscall`, `TestStopTheWorldDeadlock`) pass/pass | g-deadlock (K's linux final head read /panic RED on a loaded box: the 200 ms race on the child's `go child(); select {}`); c1-token (`runtime over 10,827 reached`); r-m6 (runtime init reads the entry-assembly attribute; stdlib hosts carry none) |
| TBS | `runtime -tests -test-filter '^TestTracebackSystem$'`, **TBS_N = 10** iterations beside `nproc` busy loops (`TBS_LOAD=1`) | every iteration: `/panic` and `/trap` disclosed-divergent on their pinned signatures, **0** `all goroutines are asleep` lines | g-deadlock. **A filtered run is DIAGNOSTIC ONLY and never banks.** The load shape (busy loops) is the draft's inference; K's red came from real concurrent work. If you can load the box another way, say how. |

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

These are unchanged from K:
- **The tree.** HEAD equals the union SHA. 75648a022b and each seat this brief reads are ancestors, and so is the TRAIN L
  fixup. There were no tracked changes before the run.
- **The toolchain.** Each comparison record reads `oracleGoVersion` go1.24.13, `configuration` Release, `tiered` false,
  `targetGOOS` linux. A failure prints `FRESHNESS FAIL: <field>`, and that row is **not a reading**.
- **One converter.** The same `converterRevision` in every manifest, and matching `BUILD converter` stamps.
- **Fresh record or no reading.** A row's three result files are deleted first, and only a newer record is accepted.
- **GolibTests.** Read the `Passed!`/`Failed!` line in THIS leg's log, never rc alone.
- **Test sources.** **csproj among the rewrites must be 0.** The fixup's two csproj edits must equal the template's
  emission. Rewritten test `.cs` files are counted, not chased. Each row's `-U0` patch is kept: COORD reads it for r-d6
  and i9-crosspkg's signatures (leg TE).
- **Deadlock lines.** The driver counts `all goroutines are asleep` in every `-tests` log AND in each fresh record's
  stderr tails (go and csharp). Expect 0 everywhere except in records where that line is the expected output (none in
  this brief's rows).

## 4. A row that moves

A mover is any of these:
- a row whose `N + D` differs from the expected column;
- an undisclosed error;
- an orphaned disclosure;
- a GolibTests failure, or a SKIP in the named arms;
- a deadlock line in net/rpc or a TBS iteration.

For each mover, run a **CONTROL**: the same row at TRAIN K's master **75648a022b**, in a worktree that is a child of
your clone. K's master carries p1-tests-goos, so the control needs no `GoTargetOS`:
```
git -C "$W" worktree add --detach "$W-ctl" 75648a022b
cd "$W-ctl" && (cd src/go2cs && go build -o bin/go2cs .)
./src/go2cs/bin/go2cs -tests -test-action all -test-timeout 30m -test-config Release \
  -go2cspath "$W-ctl/src" "$GOROOT/src/<pkg>" "$W-ctl/src/core/<pkg>"     # rc captured before any pipe
```
- Name the movers by diffing the two records with the python snippet from TRAIN K's brief, section 4. It is
  unchanged.
- Then remove the control, but only after checking it holds no tracked change. A `-tests` control always rewrites
  tracked test sources under `src/core/<pkg>` and leaves untracked build output and a comparison record, so restore
  that package first, then run the gated removal (K's composed form; `--force` is correct only after this check,
  floor 12). Copy the control's comparison record out first:
  ```
  git -C "$W-ctl" checkout -- "src/core/<pkg>"
  n=$(git -C "$W-ctl" status --porcelain | grep -vc '^??')          # must print 0; if not, STOP and post
  [ "$n" = 0 ] && git -C "$W" worktree remove --force "$W-ctl"
  ```
  Never `rm -rf` the control (floor 12).
- A mover's post gives: its name, both verdicts, the first line of the C# failure text, and the seat you think moved
  it. Mark that last part as inferred.
- For a TBS iteration that reds, the control is the same filter at 75648a022b under the same load. The prediction is
  that K's master reds at some rate, because it still has the 200 ms timer.
- Do not fix a mover. Report it.

## 5. What to post back

Send **one message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `TRAIN L linux legs (<LANE>) at
<union short SHA>`.
```
WHAT: <LANE> linux legs at the TRAIN L union <sha10>: <n> legs, <n> movers.
EVIDENCE:
- box: uid <id -u>; Microsoft .NET 10.0.12; sdk <dotnet --version>; go1.24.13 GOROOT <as printed>; nproc <n>
- tree <sha10> tree <tree10>; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>: <rows>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; L arms: <pass | the exceptions>
- (P1) LCn not-PASS <names | none>; LCf <ok | FAIL names>
- (P1) LPB: rc <n>; binlog during <SEEN | never-seen>, after <absent | PRESENT>; targetGOOS <linux>
- per row: <pkg> expected <N + D>, read <N + D>, record fresh yes, deadlock lines <n>, movers <names | none>
- (P1) LB: <project: phases; exit codes for the main-alone pair>
- (P2) runtime <N + D>, undisclosed <n>; TestTracebackSystem/panic <verdict, disclosed?>; crash family <all pass | names>
- (P2) TBS-LOOP: <k>/10 clean (disclosed signature, 0 deadlock lines); load <how>
- test sources rewritten: <rows: count>; csproj rewritten: <0 | names>
- not measured: <what and why>
NEXT: <idle | the control you are running>
```

## 6. What you do NOT do

- Change nothing in the checked-out union: no edits, commits or pushes. The driver restores the only writes, which
  come from the `-tests` pipeline and the behavioral runner.
- Never run two readings at once on the box. Never run a leg under `strace`.
- Do not run CNR, the roster guard or the sweep wrapper. They are COORD's legs on the i7.
- Kill a hung process by PID, never by name (floor 5). The TBS busy loops are killed by their PIDs.
- Never read an old results file, and never gate a reading on rc alone.
