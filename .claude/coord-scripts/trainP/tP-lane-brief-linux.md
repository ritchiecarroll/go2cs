# TRAIN P: the linux legs for P1 and P2

From COORD. DERIVED 2026-10-04 from TRAIN O's brief (`trainO/tO-lane-brief-linux.md`; P2 read O's union with it: 15
legs, 0 movers, driver exit 0; P1: 46 legs, 2 movers, both explained). **TRAIN P is the release train for go.\*
1.24.13.4**: the release waits for it because it carries the fix for a converter defect in the published `go.sort`.
These are **readings only**: nothing to cut, commit or push. Start when COORD's GO message arrives in your inbox; the GO
fills in the values below.

| | |
|---|---|
| Union ref / SHA | `claude/coord-trainP-union` / `<UNION: 40 hex>` (the fixup commit is the head, or `<FIXUP: 40 hex>` is an ancestor) |
| Base | `<BASE: 40 hex>` = TRAIN O's LANDED master (O's union eb88ab9492 + a possible fixup-2 + its MS13 refresh), + one signed merge per row of `.claude/coord-scripts/trainP/tP-seats-draft.txt` on `claude/coord-handover` (22 rows drafted 2026-10-04; the driver reads the rows, never a count) + any ruled follow-up merge + the TRAIN P fixup (`fixup: TRAIN P`, and any `fixup-N: TRAIN P` on top of it). **The driver REQUIRES `BASE=`** and refuses one that is not on the union's first-parent line. |
| Driver | `.claude/coord-scripts/trainP/tP-linux-legs.sh` on `claude/coord-handover` (tip `<HND: filled in by COORD>`), with **two companions from the same commit**: `tP-helpers.py` and `tP-seats-draft.txt` |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`) |

## 0. Which readings these are: ROOT

Both linux boxes run as **uid 0**. Every reading here is a root reading and the post says so. A root box **masks
permission-class defects** (root ignores file modes and read-only attributes). Known shifted cases:
- `os`: Go's own oracle skips `TestFilePermissions` as root: a root row reads `903 + 2` where the banked non-root
  annotation is `912 + 2`. Expect the root number (the driver's `OS_ROOT_EXPECT`).
- `os/exec` on P1's box reads `86 + 2` against the roster's `87 + 1`: `TestExtraFiles` go=pass cs=skip, the
  host-descriptor class (Q31, host-conditional; COORD 2026-10-02 15:18: "roster 87+1 stands"). The driver still raises
  it as a MOVER (no stated exception is ruled); post it as that known mover, with its one line. **It is the ONE known
  mover at P.**
- **Leg LM** stages a module copy into the test sandbox: a root reading does not stand in for a non-root one. Run it
  unprivileged if you can (say so), otherwise post `NOT MEASURED: non-root` beside the root reading.

**The driver fix you asked for (P).** At O, P1's LCn read `not-PASS=[TestRunning]`: the name is a line of fixture source
held in a raw string in `warningEntries_test.go`, not a test. LCn's derived names now pass the i7's test-name scan: a
name is kept only when a LEXICAL scan of the test file (comments, interpreted strings, rune literals, raw strings)
finds its `func <name>(` declaration in code, and the dropped names are stamped. (O's i7 remedy counted the backticks
above the line; that parity drops 8 of the 9 real tests of P's `crashVerdict_test.go`, so P does not carry it.) At
P's union the scan drops ONE name, **`TestString`** (the i9's `incrementalWrites_test.go` holds it inside a fixture):
EXPECT `DERIVED converter tests: ... dropped: [TestString]` and no LCn mover (read with git at the pre-map's 22-row
chain head: 36 derived, 35 kept, so LCn reads 40 names with the five projitems tests). **The scan's own control runs
first on YOUR box's awk** (mawk or gawk): a `MOVER: the test-name scan's control read [...]` line means the scan
misread there, nothing was filtered, and `TestString` will read not-PASS: post the line with `awk -W version` or
`awk --version`. A dropped name that IS a real test would be the scan's fault: post the line if you doubt one.

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone has **no tracked changes and no untracked files under any path the union tracks**. The driver
   refuses both, naming each file. Clean them yourself, by exact path (`LEGS_CHECK_ONLY=1` names them; the linux-only
   test sources an earlier `-tests` run left under `src/core` are the usual ones).
3. Fetch the driver and its two companions to ONE folder **outside** the clone, all from ONE fetch:
   ```
   git -C <clone> fetch origin claude/coord-handover
   mkdir -p ~/tP
   for f in tP-linux-legs.sh tP-helpers.py tP-seats-draft.txt; do
     git -C <clone> show FETCH_HEAD:.claude/coord-scripts/trainP/$f > ~/tP/$f
   done
   ```
   The converter REQUIRES `github.com/ritchiecarroll/hashset v1.0.0` (in the base since O) and LM runs with
   `GOPROXY=off`: if your module cache is cold, warm it from the union's own go.mod, in a scratch module outside the
   clone (the clone is not touched):
   ```
   git -C <clone> fetch origin claude/coord-trainP-union
   d=$(mktemp -d); git -C <clone> show <UNION>:src/go2cs/go.mod > $d/go.mod; git -C <clone> show <UNION>:src/go2cs/go.sum > $d/go.sum
   (cd $d && go mod download); echo rc=$?; rm -rf "$d"
   ```
4. Run it **in the background**, detached (`setsid nohup ... &`). Do not poll inside a turn. Read `SUMMARY.txt` when
   it exits. **A re-run takes a fresh `R=<new folder>`.**
   ```
   LANE=P1 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/tP/tP-linux-legs.sh
   LANE=P2 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/tP/tP-linux-legs.sh
   ```
   `FLOOR_GB` is the free-disk floor (default 15; P1's O run stopped once at 3G: state your free space before the
   launch, floor 12). P2 also takes `TBS_N` (10), `TBS_LOAD` (1) and `RT_CHILD` / `RT_OUTER` (**210m / 450m**). P1's LM
   takes `REALMOD_TEST_TIMEOUT` (default `2m`). Evidence lands in `$R`, default `~/tP-linux-<LANE>/`.
   **Exit status: 0 = every expectation it checks held; 4 = at least one `MOVER:` or `FRESHNESS FAIL` line.**

What the driver sets for you:
- `GOTOOLCHAIN=local CGO_ENABLED=0`, `$GOROOT/bin` first on `PATH`; the GOROOT value is checked before export (floor 6).
- `GO2CS_MODULE_ROOT` unset; `GoTargetOS` not exported (GolibTests and the runner get `GoTargetOS=linux` per command).
- **Seat asserts from the seat list**: `BASE` on the first-parent line, every row an ancestor; a `fixup: TRAIN P`
  commit; no csproj at `<LangVersion>latest</LangVersion>`. **Since review round 1 a BASE BELOW the master the union
  was assembled on is refused**: every first-parent commit between `BASE` and the union must be a TRAIN P seat merge
  or a TRAIN P fixup. `ABORT: BASE ... is BELOW the master the union was assembled on ... (refresh: TRAIN O ...)` means
  the `BASE` given is O's union eb88ab9492 (the stand-in the P seats were cut on) or another point of O's landing
  line: take `BASE` from the GO (your roster-at-base and the LCn / GT derivations would otherwise read a range that
  holds O's landing commits).
- **Execution configs.** No row carries one at the base or at the union. The driver stamps `DERIVED execution-config
  rows ... at the union=[] at the base <BASE>=[]` and **the reader's control** at a derived ref (the parent of the newest
  commit that changes the count of `execution: release-tiered` in the base's roster and whose parent annotates log/slog:
  2d46eba8f0 unless a landing changed the count; a longer walk is stamped), where `log/slog[release-tiered]` must read:
  **expect NONE, NONE, and log/slog at the control**. Every row's record must say `tiered=False`.
- **The by-name lists are derived on your box** from git between `<BASE>` and the union (stamped `DERIVED`): the
  converter tests the union adds (LCn, kept by the test-name scan), the GolibTests classes it adds or changes, the behavioral
  projects it adds (LB), the linux-only behavioral packages (LX), the host seats (landed at M: read from the base's
  history; since P a row is read as a host seat only when its notes say `host seat H1` / `host seat H2`, because C2's
  go-cmp classes carry the bare names).

## 2. Who runs what, and why

**P1:**

| Leg | What | Expected | Gates (rows) |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14; record fresh. **The driver STOPS here otherwise.** | the toolchain and the fixup |
| L2 | GolibTests **Release and Debug**, FULL | 0 failed (totals REPORTED; P2's O post 91c4226d13 read Release 1573/0/15) | P's golib rows: `g-debugger-views` (debugger views on slice, array, map, string, pointer, channel; DebuggerNonUserCode on generated plumbing; `DebuggerViewsTests` added, `NoUncountedBackingAllocationsTests` changed: never run on linux), `g-method-value-fm-record-r3` (GoPositionMapAttribute's fifth argument) |
| L2m | GolibTests Release filtered to the union's classes (derived) | all pass; every SKIP named | `DebuggerViewsTests`, `HostPackageTimeoutRunningTestsTests` (the i9's crash-verdict row: its first linux reading), `NoUncountedBackingAllocationsTests` + the literal neighbours |
| LCn | `go test -v -run` over `./...`: every converter and repoguard test the union ADDS (kept by the test-name scan) + the five `TestProjitems*` | `--- PASS` for every name; 0 SKIP, 0 FAIL; the DERIVED line reads `dropped: [TestString]` | every converter row (15 change converter source; G's sort fix and the i9's two rows were cut and gated on WINDOWS only) |
| LCf | `go test -count=1 ./...` in `src/go2cs` | ok | the same rows; report every FAIL by name |
| L4 | `net/http` (60 m per child) | **1387**, rc 0, `TestRegisterErr` (+ `/a`) pass/pass, `tiered=False` | regression |
| L4 | `internal/godebug` | **5**, `TestCmdBisect` pass/pass, `tiered=False` | fm-record-r3 names value-receiver method values `-fm` in the call stacks the bisect reads |
| L4 | `sync` | 46 + 6 | regression |
| L4 | `os`, `os/exec` | `os` root **903 + 2**; `os/exec` 87 + 1 (P1 reads 86 + 2: the known mover above) | regression |
| LPB | `cmp -tests -test-publish-binlog`, 1 s background poll | rc 0; binlog **SEEN**, then **absent**; record fresh, targetGOOS linux | regression |
| UF | after the rows, and at END for both lanes | **0** untracked paths outside the carved class (untracked **linux-only test sources** are CARVED OUT by name and stamped, never a mover); every other untracked path is a MOVER | none of your rows is `math/bits` or `weak` (the `-tests`-only `.editorconfig` class the i7 and the i9 read) |
| LX | every behavioral package windows CNR skips as platform-exclusive and that is native to linux (derived: MulticastGroupJoin, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam, UnixAbstractAddrName, WritevIovecSeam, NativeFieldPortAlias), transpiled in place with the union's converter exactly as CNR transpiles a package | **0 hunks** (your O reading: 7 packages, 0 hunks); every tracked `.cs` or `.csproj` that moves is a MOVER named by file. **This is the FIRST reading of the windows-cut converter rows on these seven projects**: G's sort cast and its fm-record row, and the i9's row, ran CNR on windows, where the seven are skipped; C2's rows ran CNR on linux (817 to 819 packages). A moved `package_info.cs` position-map line (a value-receiver method value: fm-record-r3) or a cast at a same-name call site (the sort seat) is the shape to expect if one moves: post the hunk, COORD rules whether the union carries the linux re-emission | windows CNR cannot see these packages |
| LB | the behavioral runner, `env GoTargetOS=linux` per project, on the projects the union adds (derived: 8 at the draft list: AnonStructNamedConversion, GoHostModuleShadow, LiteralFloatConstFold, MethodValueFmRecord, NamedBasicConversion, SameNameImportAlias, SiblingPackageNames, SortMethodSelfCapture) + ten named ones | rc 0 each. **`SortMethodSelfCapture` is the release fix's behavioral reading on linux** (a red there is posted at once, not at the end). ScmRightsSeam, SendtoSeam and SetegidBroadcastSeam read Target pass since O (the driver still reports SetegidBroadcastSeam ungated; post its phases) | the behavioral rows; TRAIN L's checkdead guards; the linux goldens |
| LM | `go2cs -tests -recurse` over `golang.org/x/sync@v0.19.0` and `golang.org/x/mod@v0.33.0`, `-test-timeout $REALMOD_TEST_TIMEOUT` | errgroup 5, syncmap 3, singleflight **12**, semaphore KNOWN (7 of 8); modfile 323, module 16, semver 9, dirhash 6, sumdb 4, note 7, storage 1, tlog **17**, zip builds (only TestVCS may differ); F4 0 lines | regression of the -tests -recurse driver: four P rows edit -tests conversion (the sibling seat, c2-embed-promoted-refs, c2-lifted-iface-test-cast, the i9's incremental writes) and P2's rows its diagnostics |

**P2:**

| Leg | What | Expected | Gates (rows) |
|---|---|---|---|
| L2 | GolibTests Release, FULL and PLAIN | 0 failed (totals REPORTED) | every golib row, on a second box |
| L2m | the same class filter as P1 | all pass; SKIPs named | as P1 |
| L5 | `runtime/pprof -tests`, once | **147 + 7**; `TestGoroutineProfileConcurrency`, `TestBlockProfile`, `TestMemoryProfiler` by name | regression; fm-record-r3 (frame names in profiles: `-fm`) |
| L6 | `runtime -tests`, the FULL row (210 m per child, 450 m outer) | **10810 + 73**; **`TestTracebackSystem/panic` and `/trap` DISCLOSED** by name; the crash family pass/pass; `TestLineNumber` disclosed | the largest row under P's rows: fm-record-r3's and the else-if seat's runtime/linux/package_info.cs position-map lines, the hand-owned managed_impl.cs (+125/-5), golib's debugger attributes |
| TBS | `runtime -tests -test-filter '^TestTracebackSystem$'`, 10 iterations beside `nproc` busy loops | every iteration `/panic` disclosed, **0** deadlock lines | hazard H3 under load (DIAGNOSTIC ONLY, never banks) |
| UF | at END | as P1 (the carve-out applies) | |

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

- HEAD equals the union SHA; `<BASE>`, every seat row and the fixup are ancestors; the clone was clean.
- Each record reads `oracleGoVersion` go1.24.13, `configuration` Release, `targetGOOS` linux and **`tiered` false for
  every row**. A failure prints `FRESHNESS FAIL: <field>`: that row is not a reading.
- One converter revision in every manifest; a fresh record or no reading. (The i9's row leaves an unchanged converted
  source unwritten; the driver's freshness is the comparison record's, which the seat does not touch. A passing row
  with `NO FRESH COMPARISON RECORD` would be a finding about that interaction.)
- GolibTests: read THIS leg's totals; the result-line reader accepts either console format.
- **Test sources.** Rewritten test `.cs` files are counted per row, not chased (N's and O's MS13 refreshes are in the
  base: a G-frame NoInlining hunk now is a source the refreshes missed, count it separately). **csproj among the
  rewrites:** `net/http` +1 `internal/runtime/syscall` reference, `os` and `os/exec` swapping windows test files for the
  unix/linux ones (the standing linux reading); any other csproj hunk is a finding: post it with its diff.
- Deadlock lines 0 everywhere except LB's main-alone pair stderr.
- **A `timeout` where a row or a project used to pass is read as a possible SPIN first** (G, 2026-10-04 11:35: under
  Release with TieredCompilation off a self-recursive tail call is an infinite loop, not a stack overflow). Name the
  test in flight.
- **END.** `freshness-fails` 0, `movers` 0 (or exactly the known one on P1, stated: `os/exec`), `tracked-deletions` 0.

## 4. A row that moves

For each mover, a **CONTROL**: the same row at `<BASE>`, in a child worktree of your clone:
```
git -C "$W" worktree add --detach "$W-ctl" <BASE>
cd "$W-ctl" && (cd src/go2cs && go build -o bin/go2cs .)
./src/go2cs/bin/go2cs -tests -test-action all -test-timeout 30m -test-config Release \
  -go2cspath "$W-ctl/src" "$GOROOT/src/<pkg>" "$W-ctl/src/core/<pkg>"     # rc captured before any pipe
```
Diff the two records by name, remove the control only after it holds no tracked change
(`git -C "$W" worktree remove --force "$W-ctl"`; never `rm -rf`, floor 12). An LX mover's control is the same transpile
at `<BASE>` (the committed golden there and its re-emission under O's converter). Do not fix a mover.

## 5. What to post back

**One message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `TRAIN P linux legs (<LANE>) at
<union short SHA>`.
```
WHAT: <LANE> linux legs at the TRAIN P union <sha10>: <n> legs, <n> movers; driver exit <0 | 4>, END movers=<n> freshness-fails=<n>.
EVIDENCE:
- DERIVED: execution-config rows at the union [<expect none>] base [<expect none>] control <sha10>^ [<expect log/slog>]; GolibTests classes <n>; LCn names <n> (dropped: <[TestString] | other>); LB projects <n>; LX packages [<names>]; host seats H1=<...> H2=<...>
- box: uid <id -u>; Microsoft .NET 10.0.12; sdk <dotnet --version>; go1.24.13 GOROOT <as printed>; nproc <n>; free <n>G
- tree <sha10> tree <tree10>; base <sha10> on the first-parent line; seats <n> all ancestors; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; class filter <pass | exceptions>; DebuggerViewsTests <P/F/S>; HostPackageTimeoutRunningTestsTests <P/F/S>
- (P1) LCn not-PASS <names | none>; LCf <ok | FAIL names>
- (P1) net/http <N + D> TestRegisterErr <v>; internal/godebug <N> TestCmdBisect <v>; sync <N + D>; os <N + D> (root); os/exec <N + D>
- (P1) LPB: rc <n>; binlog during <SEEN | never-seen>, after <absent | PRESENT>
- (P1) LX: <package: 0 hunks | files moved (+a/-b), the hunk's line kind>; NOT MEASURED <none | names>
- (P1) LB: SortMethodSelfCapture <phases>; <the other seven: phases>; the ten named: <all rc 0 | names>
- (P1) LM: <one REALMOD line per package>; F4 lines <n>; pin <none | v0.42.0>; non-root <done | NOT MEASURED: non-root>
- (P2) runtime/pprof <N + D>, TestMemoryProfiler <v>; runtime <N + D>; /panic <v> /trap <v>; crash family <all pass | names>; TBS <k>/10 clean
- UF: carved linux-only test sources <n> (rows, END); other untracked <n: names | 0>
- test sources rewritten: <rows: count; G-frame hunks n>; csproj rewritten: <0 | names, each checked against section 3>
- not measured: <what and why>
NEXT: <idle | the control you are running>
```

## 6. What you do NOT do

- Change nothing in the checked-out union: no edits, commits or pushes. The driver restores the only writes (the
  `-tests` pipeline, the behavioral runner, LX's transpiles), and removes only the untracked files LX itself created.
- Never run two readings at once on the box. Never run a leg under `strace`.
- Do not run CNR, the roster guard, the sweep wrapper, the published-output gate or the warning-entries gate (the i7's legs).
- Kill a hung process by PID, never by name (floor 5).
- If the driver itself is wrong (an instrument defect), finish, report it in one line with the evidence, and re-read
  the affected leg by hand with the fix, labelled as a re-read.
