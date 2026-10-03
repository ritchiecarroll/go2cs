# TRAIN N: the linux legs for P1 and P2

From COORD. DERIVED 2026-10-03 from TRAIN M's brief (itself P1's template, `.claude/coord-scripts/templates/linux-brief.md`,
with the corrections three trains taught). These are **readings only**: nothing to cut, commit or push. Start when
COORD's GO message arrives in your inbox; the GO fills in the values below.

| | |
|---|---|
| Union ref / SHA | `claude/coord-trainN-union` / `<UNION: 40 hex>` (the fixup commit is the head, or `<FIXUP: 40 hex>` is an ancestor) |
| Base | `8f46a9adae`, TRAIN M's landed master, + one signed merge per row of `.claude/coord-scripts/trainN/tN-seats-draft.txt` on `claude/coord-handover` (27 rows drafted, cutoff 10:00 Central 2026-10-03; the driver reads the rows, never a count) + any ruled follow-up merge + the TRAIN N fixup (`fixup: TRAIN N`, and any `fixup-N: TRAIN N` commit on top of it) |
| Driver | `.claude/coord-scripts/trainN/tN-linux-legs.sh` on `claude/coord-handover` (tip `<HND: filled in by COORD>`), with **two companions from the same commit**: `tN-helpers.py` and `tN-seats-draft.txt` |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`) |

## 0. Which readings these are: ROOT

Both linux boxes run as **uid 0**. Every reading here is a root reading and the post says so. A root box **masks
permission-class defects** (root ignores file modes and read-only attributes). Known shifted cases:
- `os`: Go's own oracle skips `TestFilePermissions` as root: a root row reads `903 + 2` where the banked non-root
  annotation is `912 + 2`. Expect the root number (the driver's `OS_ROOT_EXPECT`).
- `os/exec` on P1's box reads `86 + 2` against the roster's `87 + 1`: `TestExtraFiles` go=pass cs=skip, the
  host-descriptor class (Q31, host-conditional; COORD 2026-10-02 15:18: "roster 87+1 stands"). The driver still raises
  it as a MOVER (no stated exception was ruled); post it as that known mover, with its one line.
- **Leg LM** stages a module copy into the test sandbox: a root reading does not stand in for a non-root one. Run it
  unprivileged if you can (say so), otherwise post `NOT MEASURED: non-root` beside the root reading.

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone has **no tracked changes and no untracked files under any path the union tracks**. The driver
   refuses both, naming each file. Clean them yourself, by exact path. **TRAIN M's runs left linux-only test sources
   untracked under `src/core`** (P1: 17 under `os/` and `os/exec/`; P2: 21, `runtime/pprof/rusage_test.cs` among them):
   remove those by exact path before the launch (`LEGS_CHECK_ONLY=1` names them).
3. Fetch the driver and its two companions to ONE folder **outside** the clone, all from ONE fetch:
   ```
   git -C <clone> fetch origin claude/coord-handover
   mkdir -p ~/tN
   for f in tN-linux-legs.sh tN-helpers.py tN-seats-draft.txt; do
     git -C <clone> show FETCH_HEAD:.claude/coord-scripts/trainN/$f > ~/tN/$f
   done
   ```
   **Warm your module cache for the union's converter** (new at N, `p1-hashset-module`: the converter REQUIRES
   `github.com/ritchiecarroll/hashset v1.0.0`; the i7's cache did not hold it on 2026-10-03, and LM runs with
   `GOPROXY=off`). From the union's own go.mod, in a scratch module outside the clone (the clone is not touched):
   ```
   git -C <clone> fetch origin claude/coord-trainN-union
   d=$(mktemp -d); git -C <clone> show <UNION>:src/go2cs/go.mod > $d/go.mod; git -C <clone> show <UNION>:src/go2cs/go.sum > $d/go.sum
   (cd $d && go mod download); echo rc=$?; rm -rf "$d"
   ```
4. Run it **in the background**, detached (`setsid nohup ... &`). Do not poll inside a turn. Read `SUMMARY.txt` when
   it exits. **A re-run takes a fresh `R=<new folder>`.**
   ```
   LANE=P1 UNION=<UNION> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/tN/tN-linux-legs.sh
   LANE=P2 UNION=<UNION> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/tN/tN-linux-legs.sh
   ```
   `FLOOR_GB` is the free-disk floor (default 15). P2 also takes `TBS_N` (10), `TBS_LOAD` (1) and `RT_CHILD` /
   `RT_OUTER` (**210m / 450m**: you read the full row in 6834 s at TRAIN M). P1's LM takes `REALMOD_TEST_TIMEOUT`
   (default `2m`). Evidence lands in `$R`, default `~/tN-linux-<LANE>/`.
   **Exit status: 0 = every expectation it checks held; 4 = at least one `MOVER:` or `FRESHNESS FAIL` line.**

What the driver sets for you:
- `GOTOOLCHAIN=local CGO_ENABLED=0`, `$GOROOT/bin` first on `PATH`; the GOROOT value is checked before export (floor 6).
- `GO2CS_MODULE_ROOT` unset; `GoTargetOS` not exported (GolibTests and the runner get `GoTargetOS=linux` per command).
- **Seat asserts from the seat list**: `8f46a9adae` and every row are ancestors; a `fixup: TRAIN N` commit; no csproj
  at `<LangVersion>latest</LangVersion>`.
- **Execution configs.** No row carries one at N (`g-slog-roster-tc0` drops log/slog's `release-tiered`, the last
  one). The driver stamps `DERIVED execution-config rows ... at the union=[] at the base 8f46a9adae=[log/slog[release-tiered]]`:
  **expect NONE at the union** (the base's log/slog is the reader's control). Every row's record must say
  `tiered=False`. The driver gates it against the ruled set (`tN-helpers.py execruled`, empty at N): a row in the union's
  list is a MOVER; post the DERIVED line with it.
- **The by-name lists are derived on your box** from git between `8f46a9adae` and the union (stamped `DERIVED`): the
  converter tests the union adds (LCn), the GolibTests classes it adds or changes (minus the ones a linux build does
  not compile), the behavioral projects it adds (LB), the linux-only behavioral packages (LX, new), the host seats.
- **Host seats H1/H2 LANDED with TRAIN M.** They are not rows of N's list; the driver reads them from master's
  first-parent merges and stamps `H1=p2-host-test-list(landed) H2=p2-host-event-line-start(landed)`: LM then expects
  singleflight **12** and sumdb/tlog **17**.

## 2. Who runs what, and why

**P1:**

| Leg | What | Expected | Gates (seat) |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14; record fresh. **The driver STOPS here otherwise.** | the toolchain and the fixup |
| L2 | GolibTests **Release and Debug**, FULL | 0 failed. M read Release 1541/0/15, Debug 1533/0/23 | N's golib seats: c2-native-array-view (native boxes), g-named-pointer-inbound, the g-trim seats (the GoFieldMetadata tripwire), i9-crosspkg-promoted-forwarders, i9-gomethodvalue-nilfunc, g-cctor-init-frame |
| L2m | GolibTests Release filtered to the union's classes (derived) | all pass; every SKIP named | `NativeFieldArrayViewTests`, `NamedPointerTokenCarrierTests`, `FieldMetadataGuardTests`, `InitFrameNameTests` (added), `ModuleAncestryTests`, `StackFirstFrameWarmTests`, `CopyBoundReceiverAllowlistTests`, `Sha3ReinterpretVectorTests` (changed) at the draft list, + the literal neighbours |
| LCn | `go test -v -run` over `./...`: every converter and repoguard test the union ADDS + the five `TestProjitems*` | `--- PASS` for every name; 0 SKIP, 0 FAIL | every converter seat |
| LCf | `go test -count=1 ./...` in `src/go2cs` | ok | p1-hashset-module: the converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0` (from your module cache or the proxy, as your box resolves go modules for `src/go2cs`); report every FAIL by name |
| L4 | `net/http` (60 m per child) | **1387**, rc 0, `TestRegisterErr` (+ `/a`) pass/pass, `tiered=False` | regression (the default since M) |
| L4 | `internal/godebug` | **5**, `TestCmdBisect` pass/pass, `tiered=False` | g-cctor-init-frame renames init frames in the call stacks the bisect reads |
| L4 | `log/slog` | **RETIRED at N** (item N3): its L4 reading existed to read the `release-tiered` pin, which G's seat drops. The i7 and the i9 read log/slog at TC0 on windows | |
| L4 | `sync` | 46 + 6 | regression |
| L4 | `os`, `os/exec` | `os` root **903 + 2**; `os/exec` 87 + 1 (P1 reads 86 + 2: the known mover above) | r-module-driver-gomod-less edits the testing host; the linux-only siginfo file |
| LPB | `cmp -tests -test-publish-binlog`, 1 s background poll | rc 0; binlog **SEEN**, then **absent**; record fresh, targetGOOS linux | regression |
| UF | after the rows, and at END for both lanes | **0** untracked paths outside the carved class. NEW at N: untracked **linux-only test sources** (an untracked `<pkg>/<name>_test.cs` whose `<name>_test.go` is a GOOS=linux test file and not a GOOS=windows one, read with `go list`) are CARVED OUT by name and stamped, never a mover; every other untracked path is a MOVER. `uf-rows.txt` / `uf-END.txt` list them all: clean them by exact path before your next launch | the committed test corpus is the windows record |
| LX | **NEW at N** (item N9a): every behavioral package windows CNR skips as platform-exclusive and that is native to linux (derived: the six TRAIN M's CNR named, MulticastGroupJoin, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam, UnixAbstractAddrName, WritevIovecSeam, + **NativeFieldPortAlias**, c2-native-array-view's new linux-only project), transpiled in place with the union's converter exactly as CNR transpiles a package | **0 hunks**: every tracked `.cs` or `.csproj` that moves is a MOVER named by file (the template changes of g-publish-keep and c2-nugetgo-id-pattern reach csproj; windows never re-reads these packages); untracked emission is a MOVER; a transpile that does not fully type-check is NOT MEASURED (a MOVER). One stated exception: SetegidBroadcastSeam's `main.cs` alias-family residual (every changed line names runtime) reads KNOWN. Diffs kept as `lx-<pkg>.patch`. **PREDICTED at the 27-row list (read with git, review round 1): `NativeFieldPortAlias.csproj` moves +3/-1** (cut before g-publish-keep's template: no TrimMode, unconditioned ReadyToRun; g-publish-keep already regenerated the six base projects' csproj). Until COORD rules it (the union carries the linux re-emission, or LX names it known: tN-README.md section 7) it stays a MOVER: post it with that cause. LX now checks only its OWN restore (the tracked state before LX = after LX); what the rows left (proof pages) is LB's restore, as at M | windows CNR cannot see these packages |
| LB | the behavioral runner, `env GoTargetOS=linux` per project, on the projects the union adds (derived: 12 at the draft list, NativeFieldPortAlias and the i9's four among them) + ten named ones | rc 0 each, except `SetegidBroadcastSeam` (residual expected) | the behavioral seats; TRAIN L's checkdead guards; the linux goldens |
| LM | `go2cs -tests -recurse` over `golang.org/x/sync@v0.19.0` and `golang.org/x/mod@v0.33.0`, `-test-timeout $REALMOD_TEST_TIMEOUT` | errgroup 5, syncmap 3, singleflight **12**, semaphore KNOWN (7 of 8); modfile 323, module 16, semver 9, dirhash 6, sumdb 4, note 7, storage 1, tlog **17**, zip builds (only TestVCS may differ); F4 0 lines | regression of the driver r-module-driver-gomod-less edits |

**P2:**

| Leg | What | Expected | Gates (seat) |
|---|---|---|---|
| L2 | GolibTests Release, FULL and PLAIN | 0 failed (M: 1556 total, 1541 passed, 15 skipped) | every golib seat, on a second box |
| L2m | the same class filter as P1 | all pass; SKIPs named | as P1 |
| L5 | `runtime/pprof -tests`, once | **147 + 7**; `TestGoroutineProfileConcurrency`, `TestBlockProfile`, `TestMemoryProfiler` by name | regression; g-cctor-init-frame (frame names in profiles) |
| L6 | `runtime -tests`, the FULL row (210 m per child, 450 m outer) | **10810 + 73**; **`TestTracebackSystem/panic` and `/trap` DISCLOSED** by name; the crash family pass/pass; `TestLineNumber` disclosed | the largest row under N's golib seats (init-frame naming, reflect, native boxes) |
| TBS | `runtime -tests -test-filter '^TestTracebackSystem$'`, 10 iterations beside `nproc` busy loops | every iteration `/panic` disclosed, **0** deadlock lines | hazard H3 under load (DIAGNOSTIC ONLY, never banks) |
| UF | at END | as P1 (the carve-out applies: `runtime/pprof/rusage_test.cs` and the runtime linux test files are the class) | |

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

- HEAD equals the union SHA; `8f46a9adae`, every seat row and the fixup are ancestors; the clone was clean.
- Each record reads `oracleGoVersion` go1.24.13, `configuration` Release, `targetGOOS` linux and **`tiered` false for
  every row** (no exception at N). A failure prints `FRESHNESS FAIL: <field>`: that row is not a reading.
- One converter revision in every manifest; a fresh record or no reading.
- GolibTests: read THIS leg's totals; the result-line reader accepts either console format (TRAIN M's fix).
- **Test sources.** Rewritten test `.cs` files are counted per row, not chased (until COORD's MS13 refresh they still
  carry G's NoInlining class and fixup-2's using-alias lines). **csproj among the rewrites:** `net/http` +1
  `internal/runtime/syscall` reference, `os` and `os/exec` swapping windows test files for the unix/linux ones, P1's
  TRAIN M reading; any csproj hunk TRAIN M's patch of the same row did not carry is a finding: post it with its diff.
- Deadlock lines 0 everywhere except LB's main-alone pair stderr.
- **END.** `freshness-fails` 0, `movers` 0 (or exactly the known ones on P1, stated: the `os/exec` one, and LX's
  predicted `NativeFieldPortAlias.csproj` +3/-1 while COORD's ruling on it is open), `tracked-deletions` 0.

## 4. A row that moves

For each mover, a **CONTROL**: the same row at `8f46a9adae`, in a child worktree of your clone (M's commands, with
the base changed):
```
git -C "$W" worktree add --detach "$W-ctl" 8f46a9adae
cd "$W-ctl" && (cd src/go2cs && go build -o bin/go2cs .)
./src/go2cs/bin/go2cs -tests -test-action all -test-timeout 30m -test-config Release \
  -go2cspath "$W-ctl/src" "$GOROOT/src/<pkg>" "$W-ctl/src/core/<pkg>"     # rc captured before any pipe
```
(The driver no longer reads `log/slog`. Only if COORD restores that row, tN-README.md section 7: its control at
`8f46a9adae` runs at the base's OLD config, `-test-tiered`, or you compare two configs and not two trees.) Diff the two
records by name (M's snippet), remove the control only after it holds no tracked
change (`git -C "$W" worktree remove --force "$W-ctl"`; never `rm -rf`, floor 12). An LX mover's control is the same
transpile at `8f46a9adae` (the committed golden there and its re-emission under M's converter). Do not fix a mover.

## 5. What to post back

**One message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `TRAIN N linux legs (<LANE>) at
<union short SHA>`.
```
WHAT: <LANE> linux legs at the TRAIN N union <sha10>: <n> legs, <n> movers; driver exit <0 | 4>, END movers=<n> freshness-fails=<n>.
EVIDENCE:
- DERIVED: execution-config rows at the union [<expect none>]; GolibTests classes <n>; LCn names <n>; LB projects <n>; LX packages [<names>]; host seats H1=<...> H2=<...>
- box: uid <id -u>; Microsoft .NET 10.0.12; sdk <dotnet --version>; go1.24.13 GOROOT <as printed>; nproc <n>
- tree <sha10> tree <tree10>; seats <n> all ancestors; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; class filter <pass | exceptions>
- (P1) LCn not-PASS <names | none>; LCf <ok | FAIL names>
- (P1) net/http <N + D> TestRegisterErr <v>; internal/godebug <N> TestCmdBisect <v>; sync <N + D>; os <N + D> (root); os/exec <N + D>
- (P1) LPB: rc <n>; binlog during <SEEN | never-seen>, after <absent | PRESENT>
- (P1) LX: <package: 0 hunks | files moved (+a/-b)>; SetegidBroadcastSeam residual <KNOWN lines | other>; NOT MEASURED <none | names>
- (P1) LB: <project: phases>; SetegidBroadcastSeam residual lines <...>
- (P1) LM: <one REALMOD line per package>; F4 lines <n>; pin <none | v0.42.0>; non-root <done | NOT MEASURED: non-root>
- (P2) runtime/pprof <N + D>, TestMemoryProfiler <v>; runtime <N + D>; /panic <v> /trap <v>; crash family <all pass | names>; TBS <k>/10 clean
- UF: carved linux-only test sources <n> (rows, END); other untracked <n: names | 0>
- test sources rewritten: <rows: count>; csproj rewritten: <0 | names, each checked against section 3>
- not measured: <what and why>
NEXT: <idle | the control you are running>
```

## 6. What you do NOT do

- Change nothing in the checked-out union: no edits, commits or pushes. The driver restores the only writes (the
  `-tests` pipeline, the behavioral runner, LX's transpiles), and removes only the untracked files LX itself created.
- Never run two readings at once on the box. Never run a leg under `strace`.
- Do not run CNR, the roster guard, the sweep wrapper or the published-output gate (the i7's legs).
- Kill a hung process by PID, never by name (floor 5).
- If the driver itself is wrong (an instrument defect), finish, report it in one line with the evidence, and re-read
  the affected leg by hand with the fix, labelled as a re-read.
