# TRAIN O: the linux legs for P1 and P2

From COORD. DERIVED 2026-10-03 from TRAIN N's brief (`trainN/tN-lane-brief-linux.md`; P2 read N's union with it: 15
legs, 0 movers; P1's N legs were not run). These are **readings only**: nothing to cut, commit or push. Start when
COORD's GO message arrives in your inbox; the GO fills in the values below.

| | |
|---|---|
| Union ref / SHA | `claude/coord-trainO-union` / `<UNION: 40 hex>` (the fixup commit is the head, or `<FIXUP: 40 hex>` is an ancestor) |
| Base | `<BASE: 40 hex>` = TRAIN N's LANDED master (N's union 59ee0d21bf + its fixup-2 + its bank step + its MS13 refresh), + one signed merge per row of `.claude/coord-scripts/trainO/tO-seats-draft.txt` on `claude/coord-handover` (34 rows drafted 2026-10-03; the driver reads the rows, never a count) + any ruled follow-up merge + the TRAIN O fixup (`fixup: TRAIN O`, and any `fixup-N: TRAIN O` on top of it). **The driver REQUIRES `BASE=`** and refuses one that is not on the union's first-parent line. |
| Driver | `.claude/coord-scripts/trainO/tO-linux-legs.sh` on `claude/coord-handover` (tip `<HND: filled in by COORD>`), with **two companions from the same commit**: `tO-helpers.py` and `tO-seats-draft.txt` |
| Runtime | Microsoft .NET 10.0.12 (C2's recipe, `docs/phase4/recipes/microsoft-dotnet/`) |

## 0. Which readings these are: ROOT

Both linux boxes run as **uid 0**. Every reading here is a root reading and the post says so. A root box **masks
permission-class defects** (root ignores file modes and read-only attributes). Known shifted cases:
- `os`: Go's own oracle skips `TestFilePermissions` as root: a root row reads `903 + 2` where the banked non-root
  annotation is `912 + 2`. Expect the root number (the driver's `OS_ROOT_EXPECT`).
- `os/exec` on P1's box reads `86 + 2` against the roster's `87 + 1`: `TestExtraFiles` go=pass cs=skip, the
  host-descriptor class (Q31, host-conditional; COORD 2026-10-02 15:18: "roster 87+1 stands"). The driver still raises
  it as a MOVER (no stated exception was ruled at N); post it as that known mover, with its one line.
- **Leg LM** stages a module copy into the test sandbox: a root reading does not stand in for a non-root one. Run it
  unprivileged if you can (say so), otherwise post `NOT MEASURED: non-root` beside the root reading.

## 1. Setup (once, before the driver)

1. Stop or finish whatever is running. The box runs **one reading at a time**.
2. Your build clone has **no tracked changes and no untracked files under any path the union tracks**. The driver
   refuses both, naming each file. Clean them yourself, by exact path (`LEGS_CHECK_ONLY=1` names them; the linux-only
   test sources an earlier `-tests` run left under `src/core` are the usual ones).
3. Fetch the driver and its two companions to ONE folder **outside** the clone, all from ONE fetch:
   ```
   git -C <clone> fetch origin claude/coord-handover
   mkdir -p ~/tO
   for f in tO-linux-legs.sh tO-helpers.py tO-seats-draft.txt; do
     git -C <clone> show FETCH_HEAD:.claude/coord-scripts/trainO/$f > ~/tO/$f
   done
   ```
   **Warm your module cache for the union's converter** (`p1-hashset-module`, an O row: the converter REQUIRES
   `github.com/ritchiecarroll/hashset v1.0.0`, and LM runs with `GOPROXY=off`). From the union's own go.mod, in a scratch
   module outside the clone (the clone is not touched):
   ```
   git -C <clone> fetch origin claude/coord-trainO-union
   d=$(mktemp -d); git -C <clone> show <UNION>:src/go2cs/go.mod > $d/go.mod; git -C <clone> show <UNION>:src/go2cs/go.sum > $d/go.sum
   (cd $d && go mod download); echo rc=$?; rm -rf "$d"
   ```
4. Run it **in the background**, detached (`setsid nohup ... &`). Do not poll inside a turn. Read `SUMMARY.txt` when
   it exits. **A re-run takes a fresh `R=<new folder>`.**
   ```
   LANE=P1 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<go1.24.13 root, exactly as go env GOROOT prints it> FLOOR_GB=<n> bash ~/tO/tO-linux-legs.sh
   LANE=P2 UNION=<UNION> BASE=<BASE> FIXUP=<FIXUP> W=<your clone> GOROOT=<same> FLOOR_GB=<n>                                      bash ~/tO/tO-linux-legs.sh
   ```
   `FLOOR_GB` is the free-disk floor (default 15). P2 also takes `TBS_N` (10), `TBS_LOAD` (1) and `RT_CHILD` /
   `RT_OUTER` (**210m / 450m**). P1's LM takes `REALMOD_TEST_TIMEOUT` (default `2m`). Evidence lands in `$R`, default
   `~/tO-linux-<LANE>/`.
   **Exit status: 0 = every expectation it checks held; 4 = at least one `MOVER:` or `FRESHNESS FAIL` line.**

What the driver sets for you:
- `GOTOOLCHAIN=local CGO_ENABLED=0`, `$GOROOT/bin` first on `PATH`; the GOROOT value is checked before export (floor 6).
- `GO2CS_MODULE_ROOT` unset; `GoTargetOS` not exported (GolibTests and the runner get `GoTargetOS=linux` per command).
- **Seat asserts from the seat list**: `BASE` on the first-parent line, every row an ancestor; a `fixup: TRAIN O`
  commit; no csproj at `<LangVersion>latest</LangVersion>`.
- **Execution configs.** No row carries one at the base or at the union. The driver stamps `DERIVED execution-config
  rows ... at the union=[] at the base <BASE>=[]` and **the reader's control** at a derived ref (the parent of the newest
  commit that changes the count of `execution: release-tiered` in the base's roster and whose parent annotates log/slog:
  a walk of at most 12 such commits, 2d46eba8f0 unless N's landing changed the count; a longer walk is stamped), where
  `log/slog[release-tiered]` must read: **expect NONE, NONE, and log/slog at the control**. Every row's record must say `tiered=False`. A row in the
  union's list is a MOVER (gated against `tO-helpers.py execruled`, empty).
- **The by-name lists are derived on your box** from git between `<BASE>` and the union (stamped `DERIVED`): the
  converter tests the union adds (LCn), the GolibTests classes it adds or changes, the behavioral projects it adds (LB),
  the linux-only behavioral packages (LX), the host seats (H1/H2, landed at M: read from the base's history).

## 2. Who runs what, and why

**P1:**

| Leg | What | Expected | Gates (rows) |
|---|---|---|---|
| L1 | `unicode/utf8 -tests`, GoTargetOS unset | Validated 14; record fresh. **The driver STOPS here otherwise.** | the toolchain and the fixup |
| L2 | GolibTests **Release and Debug**, FULL | 0 failed (totals REPORTED; N's P2 post 5ce5a22d81 holds N's) | O's golib rows: c1-macos-flavors (buildTransitive targets), c1-darwin-xsys-libc (GoCgoDynamicImports), g-float-untyped-const-compare (UntypedInt), g-method-value-names (ж.PointerTokens), g-method-value-fm-record (GoPositionMapAttribute) |
| L2m | GolibTests Release filtered to the union's classes (derived) | all pass; every SKIP named | c1-darwin-xsys-libc's class, `UntypedIntFloatOperandTests` (added), + the literal neighbours |
| LCn | `go test -v -run` over `./...`: every converter and repoguard test the union ADDS + the five `TestProjitems*` | `--- PASS` for every name; 0 SKIP, 0 FAIL | every converter row (20 change converter source) |
| LCf | `go test -count=1 ./...` in `src/go2cs` | ok | p1-hashset-module (the module requirement), p2-converter-warning-clears (warningEntries.go and its unit tests), c1-tests-corpus-arch; report every FAIL by name |
| L4 | `net/http` (60 m per child) | **1387**, rc 0, `TestRegisterErr` (+ `/a`) pass/pass, `tiered=False` | regression; p2 changes net/http/httptest (server.cs, package_info.cs) |
| L4 | `internal/godebug` | **5**, `TestCmdBisect` pass/pass, `tiered=False` | g-method-value-names / fm-record rename frames in the call stacks the bisect reads |
| L4 | `sync` | 46 + 6 | regression; p2 writes sync's per-file `.editorconfig` |
| L4 | `os`, `os/exec` | `os` root **903 + 2**; `os/exec` 87 + 1 (P1 reads 86 + 2: the known mover above) | regression |
| LPB | `cmp -tests -test-publish-binlog`, 1 s background poll | rc 0; binlog **SEEN**, then **absent**; record fresh, targetGOOS linux | regression |
| UF | after the rows, and at END for both lanes | **0** untracked paths outside the carved class (untracked **linux-only test sources** are CARVED OUT by name and stamped, never a mover); every other untracked path is a MOVER | the committed test corpus is the windows record |
| LX | every behavioral package windows CNR skips as platform-exclusive and that is native to linux (derived: MulticastGroupJoin, ScmRightsSeam, SendtoSeam, SetegidBroadcastSeam, UnixAbstractAddrName, WritevIovecSeam, NativeFieldPortAlias), transpiled in place with the union's converter exactly as CNR transpiles a package | **0 hunks**; every tracked `.cs` or `.csproj` that moves is a MOVER named by file; untracked emission is a MOVER; a transpile that does not fully type-check is NOT MEASURED (a MOVER). **PREDICTED (read with git 2026-10-03): `NativeFieldPortAlias.csproj` moves +2/-0**, the TrimMode comment and line of N's template (it was cut before them; g-single-file-r2r-restore already gives it the unconditional ReadyToRun line, and reaches the other six linux-only csproj +1/-1 itself). Post it with that cause; COORD rules whether the union carries the linux re-emission. SetegidBroadcastSeam's `main.cs` alias-family exception stays in the driver but should now read no line (N regenerated it on linux) | windows CNR cannot see these packages |
| LB | the behavioral runner, `env GoTargetOS=linux` per project, on the projects the union adds (derived: 18 at the draft list) + ten named ones | rc 0 each. **NEW at O:** `ScmRightsSeam`, `SendtoSeam` and `SetegidBroadcastSeam` read their Target phase against P1's own `.cs.target` goldens (`p1-linux-only-goldens-targets`, an O row: each the same blob as its main.cs): EXPECT Target pass on all three (the driver still reports SetegidBroadcastSeam ungated; post its phases) | the behavioral rows; TRAIN L's checkdead guards; the linux goldens |
| LM | `go2cs -tests -recurse` over `golang.org/x/sync@v0.19.0` and `golang.org/x/mod@v0.33.0`, `-test-timeout $REALMOD_TEST_TIMEOUT` | errgroup 5, syncmap 3, singleflight **12**, semaphore KNOWN (7 of 8); modfile 323, module 16, semver 9, dirhash 6, sumdb 4, note 7, storage 1, tlog **17**, zip builds (only TestVCS may differ); F4 0 lines | regression of the -tests -recurse driver (c1-tests-corpus-arch, r-test-global-alias-type and c2-literal-lift-access edit -tests conversion) |

**P2:**

| Leg | What | Expected | Gates (rows) |
|---|---|---|---|
| L2 | GolibTests Release, FULL and PLAIN | 0 failed (totals REPORTED) | every golib row, on a second box |
| L2m | the same class filter as P1 | all pass; SKIPs named | as P1 |
| L5 | `runtime/pprof -tests`, once | **147 + 7**; `TestGoroutineProfileConcurrency`, `TestBlockProfile`, `TestMemoryProfiler` by name | regression; g-method-value-names / fm-record (frame names in profiles: `-fm`, `main.main`) |
| L6 | `runtime -tests`, the FULL row (210 m per child, 450 m outer) | **10810 + 73**; **`TestTracebackSystem/panic` and `/trap` DISCLOSED** by name; the crash family pass/pass; `TestLineNumber` disclosed | the largest row under O's golib and corpus rows: fm-record's runtime/{linux}/package_info.cs, p2's runtime1.cs and the RawBoxData pragma in managed_impl.cs, the frame-name rows |
| TBS | `runtime -tests -test-filter '^TestTracebackSystem$'`, 10 iterations beside `nproc` busy loops | every iteration `/panic` disclosed, **0** deadlock lines | hazard H3 under load (DIAGNOSTIC ONLY, never banks) |
| UF | at END | as P1 (the carve-out applies) | |

## 3. Fresh-results checks (the driver applies them; your post restates the outcome)

- HEAD equals the union SHA; `<BASE>`, every seat row and the fixup are ancestors; the clone was clean.
- Each record reads `oracleGoVersion` go1.24.13, `configuration` Release, `targetGOOS` linux and **`tiered` false for
  every row**. A failure prints `FRESHNESS FAIL: <field>`: that row is not a reading.
- One converter revision in every manifest; a fresh record or no reading.
- GolibTests: read THIS leg's totals; the result-line reader accepts either console format.
- **Test sources.** Rewritten test `.cs` files are counted per row, not chased (N's MS13 refresh is in the base, so a
  G-frame NoInlining hunk now is a source the refresh missed: count it separately). **csproj among the rewrites:**
  `net/http` +1 `internal/runtime/syscall` reference, `os` and `os/exec` swapping windows test files for the unix/linux
  ones (the standing linux reading); any other csproj hunk is a finding: post it with its diff.
- Deadlock lines 0 everywhere except LB's main-alone pair stderr.
- **END.** `freshness-fails` 0, `movers` 0 (or exactly the known ones on P1, stated: the `os/exec` one and LX's predicted
  `NativeFieldPortAlias.csproj` +2/-0), `tracked-deletions` 0.

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
at `<BASE>` (the committed golden there and its re-emission under N's converter). Do not fix a mover.

## 5. What to post back

**One message per lane** to COORD with `fleet-msg.sh`, at most 40 lines. Subject: `TRAIN O linux legs (<LANE>) at
<union short SHA>`.
```
WHAT: <LANE> linux legs at the TRAIN O union <sha10>: <n> legs, <n> movers; driver exit <0 | 4>, END movers=<n> freshness-fails=<n>.
EVIDENCE:
- DERIVED: execution-config rows at the union [<expect none>] base [<expect none>] control <sha10>^ [<expect log/slog>]; GolibTests classes <n>; LCn names <n>; LB projects <n>; LX packages [<names>]; host seats H1=<...> H2=<...>
- box: uid <id -u>; Microsoft .NET 10.0.12; sdk <dotnet --version>; go1.24.13 GOROOT <as printed>; nproc <n>
- tree <sha10> tree <tree10>; base <sha10> on the first-parent line; seats <n> all ancestors; converter exe <sha16>, one revision in every manifest (yes/no); freshness-fails <n>
- GolibTests Release <P/F/S>, Debug <P/F/S> (P1); Release <P/F/S> (P2); failed: <names>; class filter <pass | exceptions>
- (P1) LCn not-PASS <names | none>; LCf <ok | FAIL names>
- (P1) net/http <N + D> TestRegisterErr <v>; internal/godebug <N> TestCmdBisect <v>; sync <N + D>; os <N + D> (root); os/exec <N + D>
- (P1) LPB: rc <n>; binlog during <SEEN | never-seen>, after <absent | PRESENT>
- (P1) LX: <package: 0 hunks | files moved (+a/-b)>; SetegidBroadcastSeam residual <0 lines | lines>; NOT MEASURED <none | names>
- (P1) LB: <project: phases>; ScmRightsSeam / SendtoSeam / SetegidBroadcastSeam Target <pass | lines>
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
